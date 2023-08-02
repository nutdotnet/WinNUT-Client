WinNUT is a WinForms client for connecting to a Network UPS Tools monitoring server.

- 📈 Monitor important values of your UPS like voltage and power consumption
- 🌩️ Receive notifications for abnormal power conditions (power outage)
- ❤️ Keep your hardware and data safe with configurable suspend and shutdown triggers

<br />
<br />

# Future development
WinNUT has gone through a long evolution with several different programming languages and maintainers. The time has come to retire this iteration of WinNUT and begin planning for the next version. See [issue #40](https://github.com/nutdotnet/WinNUT-Client/issues/40) for more information. Join in the discussions at [Coco.Nut](https://github.com/nutdotnet/Coco.Nut), and shape the future of the next iteration of WinNUT.

# Installation
1. Get the [latest Release](https://github.com/nutdotnet/WinNUT-Client/releases)
2. Install WinNUT using the downloaded executable
3. If you were using an older version of WinNUT (v1.x), copy your "ups.ini" configuration file to the WinNUT-Client installation directory (by default "C:\Program Files(x86)\WinNUT-Client ") for an automatic import of your parameters during the first launch
4. Start WinNUT and modify the settings according to your needs

## Synology NAS 
If you are connecting to a Synology NAS with a UPS attached, there is some additional configuration that needs to be done.

Referring to the [Synology documentation](https://kb.synology.com/en-us/DSM/help/DSM/AdminCenter/system_hardware_ups?version=7), note that you must add your client computer's IP address to the *Permitted DiskStation Devices* window. In addition, WinNUT requires the following settings:

- **Login**: upsmon
- **Password**: secret
- **UPS Name**: ups


*See [issue 47](https://github.com/gawindx/WinNUT-Client/issues/47#issuecomment-759180793) for more information.*

## QNAP NAS 
If your NUT server is hosted on a QNAP NAS, be sure to provide the following connection information (default):

- **UPS Name**: qnapups
- (Login and Password can be empty)

Also check the "Enable network UPS master" box on the Control Panel -> External device page on the QNAP webinterface and add the IP address of the WinNUT-Client to allow the client to connect to the QNAP for UPS information.

# Contributing
- [Translations](https://github.com/nutdotnet/WinNUT-Client/wiki/Translations)
- [Code/Bug fixes](https://github.com/nutdotnet/WinNUT-Client/wiki/Development)

# Updates

WinNUT has built-in update functionality. This process can be started automatically on startup or manually on demand, and you can choose whether you want to update to the stable or development version. During this process, the new files will be automatically downloaded and installed.

## Third Party Components / Acknowledgments

WinNUT uses:
- a modified version of AGauge initially developed by [Code-Artist](https://github.com/Code-Artist/AGauge) and under [MIT license](https://opensource.org/licenses/MIT)
- Class IniReader developed by [Ludvik Jerabek](https://www.codeproject.com/Articles/21896/INI-Reader-Writer-Class-for-C-VB-NET-and-VBScript) and under [The Code Project Open License](http://www.codeproject.com/info/cpol10.aspx)
- Newtonsoft.Json Library is used in this Project [Newtonsoft.json Website](https://www.newtonsoft.com/json) and under [MIT license](https://opensource.org/licenses/MIT)

## License

WinNUT-Client is a NUT windows client for monitoring your ups hooked up to your favorite linux server.

- Copyright (C) 2019-2021 Gawindx (Decaux Nicolas)
- Copyright (C) 2022+ NUT Dot Net project

This program is free software: you can redistribute it and/or modify it under the terms of the
GNU General Public License as published by the Free Software Foundation, either version 3 of the
License, or any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY.