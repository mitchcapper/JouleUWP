Joule Protocol, interfacing, reverse engineering, and API documentation
=================================
The goal is to establish open documentation of the Joule protocol and interfaces so 3rd parties can interact with the Joule.  There could be some security ramifications as the Joule is NOT a local only device (it does take commands from the chefsteps servers).  The Joule itself does not seem to have any publicly exposed ports however, so even if there was a security flaw the chefsteps team could block it relatively easily.  There is little	reason for chefsteps to try and interfere with this process, and previously they had a 'developer signup' which would seem to indicate wanting to support this in the future.   For now it seems ChefSteps is WAY to busy to offer any assistance, documentation, development endpoints or test options (or their development program they had a signup for).  We have reached out a few times without luck.

3rd Party support not only allows for other platforms (ie Windows or Web) but also would allow for other features (automation, controlling multiple joules at once, multiple users able to control a joule via the internet, etc).

The API itself is not a basic REST api as one might hope, and seems fairly complex.  While MITM of the traffic (BT/web) does work, most likely the best reverse engineering will be through source inspection (thankfully most logic seems JS based).

Table of Contents
=================

   * [Current Goals / Next Steps](#current-goals--next-steps)
   * [Joule Communication Modes](#joule-communication-modes)
     * [Joule Bluetooth Communication](#joule-bluetooth-communication)
     * [Joule Internet Communication](#joule-internet-communication)
   * [App Communication Modes](#app-communication-modes)
     * [Protocol Buffers](#protocol-buffers)
     * [App Bluetooth Communication](#app-bluetooth-communication)
     * [App Internet Communication](#app-internet-communication)
     * [Firmware Programs](#firmware-programs)
   * [Official App Source](#official-app-source)
   * [App Logging](#app-logging)
   * [Firmware Flashing](#firmware-flashing)

Current Goals / Next Steps
------------
Here is an outline of what we will (or others are welcome) to try and do next to further the decoding of the Joule communication stack. The concentration is useful control of the Joule is over protocol buffers and the bluetooth or web socket connection so this is our current focus. 
-   The safest approach to start would be to capture the web socket or bluetooth messages then run them through a simple decoder against the base.proto/remote.proto specs to make sure things are as expected.  This would verify both use a similar communication mode, and that the protocol buffer decoding is working.
-   The next easiest step would likely be to try and emulate the web socket itself.  This will require further research into the app as to how web socket authentication occurs, and what would be needed to establish a similar connection.  This may require temporarily copying authentication keys from the session out of the app/traffic to do so.  If we are able to establish the web socket connection to the server, and receive the Joule's messages we will be fairly far towards a working 3rd party API.
-   Assuming the above two are successful it would next be simply a matter of what REST / authentication calls are required to get the web socket stood up.  While there are several keys/hashes that seem to be used in these steps it should not be that big of a task.
-   As the Joule most likely uses protocol buffers both over bluetooth and the web socket, it may not be very difficult to pair over bluetooth with the Joule and then try to see if there is anything more to the stream than just a protocol buffer message stream.  Looking at the decoded bluetooth traffic can also help with that.  This may be an even easier route if establishing the web socket becomes non-trivial (of course this does not give remote access to the Joule, but would provide at least local control).


Joule Communication Modes
------------
The Joule device itself primarily communicates by one of two methods: bluetooth or the internet (via wifi). The chipset is most likely from Espressif. There is little reason to spend too much time on its outbound internet communication. A scan of the most common ports did not show anything open on the device itself, although a full port scan is still needed.

### Joule Bluetooth Communication
Joule can be completely controlled by bluetooth for all aspects of operation and by multiple users.  There is no pin or security requirement for pairing just being within range.  

### Joule Internet Communication
The bluetooth connection can be used to connect the device via wifi.  The Joule only supports b/g/n and not AC networks.  It also does not support WPA/WPA2 enterprise (username/password) based wifi authentication (does not even prompt for a username/password). Joule makes an outbound https persistent connection to an AWS heroku instance (ie: ec2-54-225-183-177.compute-1.amazonaws.com), most likely including a web socket connection as well.


App Communication Modes
------------
The app has two methods of communication with the Joule bluetooth and internet.  We do not believe it is possible to talk directly to the Joule over the wifi/tcp connection (except for firmware flashing as a tftp server).  The only slightly good news is it looks like messages are built the same way for bluetooth and wifi/tcp so work done to get one working should go towards making the other work as well. @Yitzchok pointed out that it looks like the binary format of google's protocol-buffers is likely what is at play over bluetooth and the web socket.

### Protocol Buffers
Google's protocol buffer messaging system (https://developers.google.com/protocol-buffers) is most likely what is used for communication over bluetooth or the web socket with the Joule.  This is where most command&control seems to take place, and how virtually all changes, status updates, etc are transmitted.  Protocol buffers is a binary transfer method, that relies on proto specification files for the message format.  There are two proto files in the App in the assets\www\protobuf-files folder. remote.proto just is an encapsulation message used when not talking directly with the Joule and contains the stream message. base.proto is the meat of the message formats used for communication.  There are a lot of messages that flow over the normal web socket/bluetooth connection most of them likely to be status updates.  All messages are stream messages to start, with various sub-messages possible.  There are protocol buffer libraries for many languages including .NET.  


### App Bluetooth Communication
The protocol does seem to be somewhat binary, protocol buffers are used for communication over web sockets, so it is likely the same here.  We have not yet decoded the messages to verify. Android easily supports enabling bluetooth HCI packet logging under developer settings (wireshark can then be used to open the log file).

### App Internet Communication
Using a mitm proxy like Fiddler (which also supports web sockets) with the proper root cert installed it is possible to capture and monitor the Joule's traffic (http://docs.telerik.com/fiddler/Configure-Fiddler/Tasks/ConfigureForAndroid covers basic setup).  The chefsteps API (base url: https://www.chefsteps.com/api/v0/) uses REST/JSON/POST actions paired with a websocket for communication.  Unfortunately it *looks* like signaling (temperature control, etc) happens over the web socket.  Further the web socket seems to use a binary protocol(likely protocol buffers) and is somewhat chatty (probably constant temperature readings).  The REST api uses several hashes and a basic auth block, along with sequence numbers (maybe for duplication checking).  Most of the json posts include a huge wealth of un-necessary information on every request (We have not tested to see if requests are rejected without all this excess information).   We are not sure how needed it will be to catalog many of the REST calls, as we believe most control is through the web socket.  The app also fetches more static resources from amazon cloudfront (ie https://d1azuiz827qxpe.cloudfront.net) and there may be a test resource location of http://api.jouleapp.com.

### Firmware Programs
The Joule essentially has the idea of a 'program' it currently runs.  Programs cannot be changed so when the app goes to update the program really its stopping the current program and starting a new one (this is why you can sometimes hear the circulator stop).  In theory a program has the following properties:
-    @param {float} setPoint - The temperature where the cooking will be done
-    @param {int} cookTime - Time (in seconds) to perform the cook
-    @param {int} delayedStart - Time (in seconds) to wait before heating the water bath.
-    @param {float} holdingTemperature - The temperature to drop the bath down to after the cookTime
-    It also takes other args like is it a manual/automatic program, and metadata that has guide/program id information. We are guessing the metadata is not used by Joule itself for anything.
You can see the message construction in assets/www/js/bundle.js (search for makeMessage)
There are other parameters like turbo (allows overshooting for faster getting to temperature) or waitForPreheat, but we don't actually see these used anywhere in code (maybe future plans).  It could be somehow just serialized but that seems unlikely.  It is also not clear how useful holding temperature is, given the fact the bath has to naturally cool to reach a lower temperature.



Official App Source
------------
The Joule app android source (minus videos and crosswalk libraries) can be found at: https://github.com/mitchcapper/JouleUWP/releases/download/0.0/jouleapp.zip .  Classes are decompiled.  The classes largely seem like helper libraries.  The bulk of the Joule logic seems to be in the javascript itself (assets/www/chefsteps.js) with most of the temp setting done with controls like ```circulatorManager.startProgram(programOptions)```.


App Logging
------------
The app logs a lot of detailed data fairly often to azure blob endpoint that is pulled off the device itself it seems (although maybe its just app logs).  This is in addition to app actions being posted directly back to the API.  The app logs a lot more on every api request than needed, this is probably in part for analytics but they seem to pull the maximal information from the device.  The device status update messages are also something of interest.  The report not just the current temperature, but the message spec shows the heater temperature, the lower and upper board temperature sensors, some of the circulator information (rpm,etc), and even the current pressure (maybe for adjusting for altitude).

Firmware Flashing
------------
While there have been no public firmware update launches yet the code describes a few features for firmware flashing.  The chefsteps.js talks about http, download, and tftp support.  The protocol buffers prototype file (base.proto) covers several aspects of TFTP transfer.  It sounds like firmware updates could be applied directly from the local network using tftp.  You can ask the Joule to download over TFTP from any host it seems.  The Joule does not verify any signature on the firmware (assuming the notes are correct), however when you send it a message to download a firmware you also send a sha256 sum for it to verify once it downloads it.  This could allow for 3rd party firmwares as essentially there is not a current restriction to somehow verifying the source (as far as we can tell).