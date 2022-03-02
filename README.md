# Zenith_bhaptics
Basic bHaptics support for Zenith

## Warning

Let me be very clear up front: ***Zenith is a multiplayer game. If you try to cheat via mods, you are likely going to get your account banned, and
nobody will like you.*** Even with this workaround, Zenith will know that you are running a mod, and it will tell you right at startup that it knows.
This mod here should be fine, it is not cheating and only provides additional immersive feedback, as anyone can see in this source code.

If you have any questions or comments, ping me (Florian) in the bhaptics Discord:

[https://discord.gg/mKaWnRuAjC](https://discord.gg/mKaWnRuAjC)

## Automated Installation

Get the Setup Catch file from [the latest release here](https://github.com/floh-bhaptics/Zenith_bhaptics/releases/latest/download/Install_Zenith_bhaptics.bat).
Save it into your Zenith main folder and execute (double click) it. Windows will warn you that it does not know the published, and you will have to
click on "More info..." to pick "Run anyway". If you aren't sure what the file does, you can open it in a text editor. It will download the Zenith modding
tool, the mod loader (BepInEx) and my mod from GitHub and unzip them into the current directory.

Afterwards, you will have a "ZenithModding.exe" file in your main game folder. Run this to start Zenith with the mod. The first time will take
quite a while because the modding tool will do its magic, and Zenith might not even start. If the command Window closes and nothing happens, just
run the executable again and it should work.

That's it. You can still play unmodded Zenith by starting it "the normal way" and not via the "ZenithModding.exe".

## Manual installation

Basically do what the Setup file does:
* Download the latest release of [the Zenith modding tool](https://github.com/Christoffyw/ZenithModding/releases)
* Download and install the latest release of [BepInEx 6 (!)](https://builds.bepinex.dev/projects/bepinex_be)
* Download the two libraries in the latest release of [this mod](https://github.com/floh-bhaptics/Zenith_bhaptics/releases/latest) and put them into the "BepInEx/plugins" folder
