# Vortex Collections Automator

This is a tool to 'somewhat' automate Vortex collection downloads.
<br>
<br>
<br>

## Possible questions

### What does this tool do exactly?
This tool, after having added screenshots of your Vortex download button (Don't forget the hover version) and optionally of the 'Slow download'-button on NexusMods.com allows for automated button clicks while the user is not using their device.  


### How does it do this?
- The application creates a screenshot of the users desktop.
- The application attempts to find the download button images in said screenshot.
- If found, the application moves the cursor to the found button and sends a click event.  


### Are the screenshots stored anywhere externally?
This tool does not make any network calls, all screenshots may remain in cache for use by the tool only, but are not actively stored.  


### Can I use this code for -_insert thing here_-
Use it for whatever you want.  
<br>

## Use

### First Use
- Put the portable .exe file in it's own folder, as it will create 2 folders to use.
- Run it and the folders will be created, you will be asked to put screenshots of the download buttons in the images folder. (If you use your own, try to crop out the borders and don't forget that there might be a different coloration on hover)  


### Further Use
- Ensure that the Vortex aplication is open with the download button showing.
- Open the .exe file and it will start by itself
- To quit the application midway, either regularly close it by hitting the window close-button, or hold any key while the console is showing to interrupt it.  


### Speeding things up
To prevent the tool from constantly demanding system resources, there is a delay build in which can be adjusted in the settings.
Lowering this delay to too short a time may result in the tool clicking the download button over and over before Vortex has a chance to close the modal.
Due to this the recommended delay is 5000ms or more, I personally use 6000ms to simply await the 5 seconds that the slow download button demands you wait.

One way to still speed this up is by using a TamperMonkey UserScript to click the Slow download button as soon as it is available.
Such a script can be found [here](https://github.com/ShadeOfChaos/Vortex_Collections_Automator/blob/main/Optional_TamperMonkey_UserScript/NexusMods%20Auto-click%20slow%20download.js)  
<br>

## Note
- While this tools' functionality isn't limited to Vortex and can be used for any similar use, just be aware of the disconnect with the text outputted into the console window.
