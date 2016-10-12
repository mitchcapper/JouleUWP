JouleUWP - Unofficial Windows 10 UWP App (phone, desktop, hololens, xbox) for ChefSteps Joule Sous-vide device
=================================
NOTE!
-----
Unfortunately the API seems more complicated with most.  There is no direct TCP traffic with joule only through a chefsteps server and the API is not simple (it seems like most functional changes are over web sockets). See [Joule Protocol Documentation](JOULE_PROTOCOL.md) for more details and help is very welcome!  Right now there has not been success in 3rd party api access.

OVERVIEW
-----
Joule (https://www.chefsteps.com/joule) is a nice device but only works with IOS or Android out of the box.  This app adds windows support.  Control Joule from any Windows 10 device. Note that this is an un-official app.  In addition ChefSteps was WAY to busy to offer any assistance, documentation, development endpoints or test options (or their development program they had a signup for).  As such it requires reversing the android/iphone apps to generally add features.  Hopefully they will care about Windows at some point, we have reached out a few times.

Table of Contents
=================

   * [Description](#description)
     * [Screenshot](#screenshot)
     * [Features](#features)
   * [Installation](#installation)
   * [Official App Limitations](#official-app-limitations)
   * Development
     * [Compiling](#compiling)
     * [Code Formatting](#code-formatting)
     * [Git Flow Tips](#git-flow-tips)
     * [Issue / Request / Feature Tracking](#issue--request--feature-tracking)
     * [XBOX One Deployment](#xbox-one-deployment)
     * [Protocol Notes](#protocol-notes)     
     * Code Function Notes
       * [MVVM](#mvvm)
       * [Debugging](#debugging)

Description
------------
Joule is a Sous-vide cooking device.  The official app has a lot of features: bluetooth pairing, setting temperature on the device, cook time, device status, control the wifi network it is connected to, and various recipe walkthroughs.  This unofficial app will concentrate first on core features and expand from there.

### Screenshot
Coming soon..

### Features
Here we will note the major features that are added to the app
-   .


Installation
------------
The app itself can be found in the store under the name JouleUWP (once we put out the first version).  It currently can only communicate with the Joule over wifi (issue #1).  This means you must use an Android or iOS device (or an emulator) to first connect your Joule to your wifi network before you can use this app.   Bluetooth support will be added in the future.

Official App Limitations
------------
The official app has several limitations in its current form.  Only the primary user can control the device over wifi.  It can only manage one Joule.  Hopefully we can add these features and more.

Development
------------


### Compiling
Install https://www.sqlite.org/2016/sqlite-uwp-3130000.vsix its required in addition to the nuget packages.
Clone the source open the solution file.  It should restore all the nuget packages on first compile.


### Code Formatting
Tabs not spaces. Unix line endings. Opening brackets on the same line. Otherwise whatever:)
For your .gitconfig:
```
[core]
	autocrlf = false
	eol = lf
```
### Git Flow Tips
This is an authenticated repo so to clone you will need to do something like (using paegent with an ssh key for example):
`git clone ssh://git@github.com/mitchcapper/JouleUWP.git`

Try to do development in branches, one branch per feature.   Don't commit any binaries here.  Don't clone this repo publicly. I would recommend a workflow like below:
```
git checkout -b add_timer_support
'do some work
git commit -a -m "start work"
'do some work
git commit -a -m "more work"
git push -u github add_timer_support
```
You only need to do the --set-upstream / -u line once, afterwards git push works like normal.

Once pushed create a PR for merging into master, its cleaner than us merging into master directly. We can also have the PR squash down on merge for a cleaner commit into master.  This is really easy to do if you go to github.com you will generally see a prompt for new branch commits to be turned into PR.  Afterwards we can delete the old branches.  The downside of our squash merging is that if you branch off of one of your branches merging later will result in conflicts.  By squashing we don't have the history that allows git to merge more nicely.  For now this seems like a small price to pay.  To avoid conflicts: *) Don't branch your branches or pre-merge features into other branches *)try to sync with master before branching.


### Issue / Request / Feature Tracking
Create issues / feature requests in github's issues.  Its an easy way to track work to be done etc.

### XBOX One Deployment
The main guide is at: https://msdn.microsoft.com/windows/uwp/xbox-apps/getting-started while you need to join windows insider to download the latest windows SDK (build 14295 or newer) you do not need to run an insider build of windows on your PC (so normal Win10 will work).  There is additional documentation at: https://msdn.microsoft.com/windows/uwp/xbox-apps/index .

### Protocol Notes
Please see [Joule Protocol Documentation](JOULE_PROTOCOL.md)


### Code Function Notes

#### MVVM
We are using MVVM Light for development.  Its pretty lightweight and stays out of the way. MVVM is Model View ViewModel meaning you have a model for logic, a View for the UI, and a ViewModel that ties them together and has the specific view specific logic.  In general your UI (Views) go in the Views folder one view per page, your UI logic/data bindings are in your ViewModel file in ViewModels,  we use data binding. In general we try and tie the UI to the logic using data bindings.  Bindings can be two way (an update in the UI changes the code, and update in code affects the UI) or one way (updates to code affect the UI but not the other way, this is default).  Buttons also use data binding by binding to Commands.  To make command binding easier there is the GetCmd helper.  It generates a wrapper around a standard function for example:
```
public OurCommand JumpBackCmd => GetOurCmdSync(JumpBack);
	public void JumpBack() {
}
```		
The true arg means it will automatically disable the command when clicked temporarily and re-enable when the function is done.  There is also GetOurCmd to use with async functions.

Some items cannot be bound to the ViewModel, some events for example you have to implement in the PageName.xaml.cs

True business logic (not view specific) generally goes in the Models folder.

Design time only items go in the design folder.

Beware of putting code that auto-happens on the VM or page being created,  these actions happen in the designer too.  If you have code that should not run in the designer then make sure to use the IsDesignMode to do so.

#### Debugging
Sometimes VS likes to gobble up exceptions silently with tasks so there are times you will have to turn them on for first break.  There are also some debug flags you can set to assist.  There is now the debug manager WPF tool to allow you to configure the debug json file.  This file goes in c:\users\YourUserName\Videos\ as joule_debug.json (you can have a few of these and just rename them around for quick debug setting swapping).

