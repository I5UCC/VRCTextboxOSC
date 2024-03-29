# <img src="https://user-images.githubusercontent.com/40323669/192174553-d4d38974-3da4-4b9b-b459-042e27a9c25b.png" width="32" height="32"> VRCTextboxOSC [![Github All Releases](https://img.shields.io/github/downloads/i5ucc/VRCTextboxOSC/total.svg)](https://github.com/I5UCC/VRCTextboxOSC/releases/latest) <a href='https://ko-fi.com/i5ucc' target='_blank'><img height='35' style='border:0px;height:25px;' src='https://az743702.vo.msecnd.net/cdn/kofi3.png?v=0' border='0' alt='Buy Me a Coffee at ko-fi.com' />

An OSC application to communicate with VRChats new "Text Chatbox" system. 
This can directly replace the in game Keyboard for the Chat box and update it in real time! (With a bit of a delay tho) <br>

Features:
- Auto update 
  - Continuosly Updates the textbox with your written text. Pressing enter clears the Chatbox.
  - Continuous Writing: When the maximum Character limit is reached it removes the first word so you can continue typing.
- Manually send by pressing enter or the send button.

- Window focus feature, press Alt+A to focus your keyboard and mouse from VRChat to VRCTextboxOSC
- "Set Window Always on top" mode,
  This is useful for Users who want to keep the textbox app window ontop of all other apps.

### Want to talk rather then type? [Try my SpeechToText implementation](https://github.com/I5UCC/VRCTextboxSTT)

### [Discord Support Server](https://discord.gg/rqcWHje3hn)

# [Download here](https://github.com/I5UCC/VRCTextboxOSC/releases/download/v0.1.3/VRCTextboxOSCv0.1.3.zip)

# Showcase

https://user-images.githubusercontent.com/43730681/184694019-4a272780-5bba-40a5-835e-1b62ce395943.mp4

## How to use

Activate OSC in VRChat: <br/><br/>
![EnableOSC](https://user-images.githubusercontent.com/43730681/172059335-db3fd6f9-86ae-4f6a-9542-2a74f47ff826.gif)

In Action menu, got to Options>OSC>Enable <br/>

Then just run the ```VRCTextboxOSC.exe``` and you are all set! <br/>

## OSC Troubleshoot

If you have problems with this program, try this to fix it:
- Close VRChat.
- Open 'Run' in Windows (Windows Key + R)
- Type in `%APPDATA%\..\LocalLow\VRChat\VRChat\OSC`
- Delete the folders that start with 'usr_*'.
- Startup VRChat again and it should work.

# Requirements

- [.NET 6 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0/runtime)
