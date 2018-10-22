# NOLFPS2AutoRecorder

**Early prototype release ! **

    Code needs to be modified if you want to record something else than "Berlin By Night - Scene Five", but that's pretty easy.
    Wait (Idle) times around the PCSX2 emulator need to be modified too, in order to account for difference in performance between users.

Remember :

    The LilyPad.ini file needs to be used by PCSX2
    You need to have the fmedia tool installed.
    The paths in the App.config file need to match :

            <setting name="PCSX2ExePath" serializeAs="String">
                <value>C:\Jeux\PCSX2\PCSX2.EXE</value>
            </setting>
            <setting name="TempRecordingsWorkDir" serializeAs="String">
                <value>C:\Jeux\ISOs\VOICE_FR\_Recordings</value>
            </setting>
            <setting name="FMediaExePath" serializeAs="String">
                <value>C:\Jeux\Outils\fmedia.exe</value>
            </setting>
            <setting name="ISOPath" serializeAs="String">
                <value>C:\Jeux\ISOs\NOLF.ISO</value>
            </setting>


.NET 4.0 Client Profile Project

.NET 4.0 is installed with Windows since Windows Vista.

This project uses Visual Studio. If you don't have it, you can get it here :
https://visualstudio.microsoft.com/fr/thank-you-downloading-visual-studio/?sku=Community&rel=15

Uses PCSX2 (https://buildbot.orphis.net/pcsx2/) and fmedia (http://fmedia.firmdev.com/) and savestate slot 0 (at the very start of the loading screen of each scene), and a custom Lilypad keyboard configuration file (provided at the root of the sourcetree) to record the game's voices from a modified ISO files provided by Tgames (http://tgames.fr)
