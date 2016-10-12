Joule Protocol, interfacing, reverse engineering, and API documentation
=================================
The goal is to establish open documentation of the Joule protocol and interfaces so 3rd parties can interact with the Joule.  There could be some security ramifications as the Joule is NOT a local only device (it does take commands from the chefsteps servers).  The Joule itself does not seem to have any publicly exposed ports however, so even if there was a security flaw the chefsteps team could block it relatively easily.  There is little	reason for chefsteps to try and interfere with this process, and previously they had a 'developer signup' which would seem to indicate wanting to support this in the future.   For now it seems ChefSteps is WAY to busy to offer any assistance, documentation, development endpoints or test options (or their development program they had a signup for).  We have reached out a few times without luck.

The API itself is not a basic REST api as one might hope, and seems fairly complex.  While MITM of the traffic (BT/web) does work, most likely the best reverse engineering will be through source inspection (thankfully most logic seems JS based).

Table of Contents
=================

   * [Joule Communication Modes](#joule-communication-modes)
     * [Joule Bluetooth Communication](#joule-bluetooth-communication)
     * [Joule Internet Communication](#joule-internet-communication)
   * [App Communication Modes](#app-communication-modes)
     * [App Bluetooth Communication](#app-bluetooth-communication)
     * [App Internet Communication](#app-internet-communication)     
   * [Official App Source](#official-app-source)
   * [App Logging](#app-logging)



Joule Communication Modes
------------
The Joule device itself primarily communicates by one of two methods: bluetooth or the internet (via wifi). The chipset is most likely from Espressif. There is little reason to spend too much time on its outbound internet communication. A scan of the most common ports did not show anything open on the device itself, although a full port scan is still needed.

### Joule Bluetooth Communication
Joule can be completely controlled by bluetooth for all aspects of operation and by multiple users.  There is no pin or security requirement for pairing just being within range.  

### Joule Internet Communication
The bluetooth connection can be used to connect the device via wifi.  The Joule only supports b/g/n and not AC networks.  It also does not support WPA/WPA2 enterprise (username/password) based wifi authentication (does not even prompt for a username/password). Joule makes an outbound https persistent connection to an AWS heroku instance (ie: ec2-54-225-183-177.compute-1.amazonaws.com), most likely including a web socket connection as well.


App Communication Modes
------------
The app has two methods of communication with the Joule bluetooth and internet.  We do not believe it is possible to talk directly to the Joule over the wifi/tcp connection. 

### App Bluetooth Communication
The protocol does seem to be somewhat binary, it might be jsonp but have not figured out the proper decoding yet. Android easily supports enabling bluetooth HCI packet logging under developer settings (wireshark can then be used to open the log file).
Internet Communication

### App Internet Communication
Using a mitm proxy like Fiddler (which also supports web sockets) with the proper root cert installed it is possible to capture and monitor the Joule's traffic (http://docs.telerik.com/fiddler/Configure-Fiddler/Tasks/ConfigureForAndroid covers basic setup).  The chefsteps API (base url: https://www.chefsteps.com/api/v0/) uses REST/JSON/POST actions paired with a websocket for communication.  Unfortunately it *looks* like signaling (temperature control, etc) happens over the web socket.  Further the web socket seems to use a binary protocol (at least not straight ascii but maybe jsonp) and is somewhat chatty (probably constant temperature readings).  The REST api uses several hashes and a basic auth block.  Most of the json posts include a huge wealth of un-necessary information on every request (We have not tested to see if requests are rejected without all this excess information).   We are not sure how needed it will be to catalog many of the REST calls, as we believe most control is through the socket.  The app also fetches more static resources from amazon cloudfront (ie https://d1azuiz827qxpe.cloudfront.net) and there may be a test resource location of http://api.jouleapp.com.



Official App Source
------------
The Joule app android source (minus videos and crosswalk libraries) can be found at: https://github.com/mitchcapper/JouleUWP/releases/download/0.0/jouleapp.zip .  Classes are decompiled.  The classes largely seem like helper libraries.  The bulk of the Joule logic seems to be in the javascript itself (assets/www/chefsteps.js) with most of the temp setting done with controls like ```circulatorManager.startProgram(programOptions)```.


App Logging
------------
The app logs a lot of detailed data fairly often to azure blob endpoint that is pulled off the device itself it seems (although maybe its just app logs).  This is in addition to app actions being posted directly back to the API.  The app logs a lot more on every api request than needed, this is probably in part for analytics but they seem to pull the maximal information from the device.