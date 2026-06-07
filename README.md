# TrafficMatch Builder

**Automatic Model Matching Generator for Microsoft Flight Simulator**

TrafficMatch Builder is a lightweight utility that automatically scans installed traffic model libraries and generates ready-to-use VMR (Virtual Model Rule) files for vPilot and VATSIM.

Instead of manually editing XML files and creating model matching rules by hand, TrafficMatch Builder analyzes your installed traffic models and generates matching configurations within seconds.

---

## Features

* Automatic traffic model scanning
* Automatic VMR generation
* Support for large traffic model libraries
* Multi-language user interface
* Built-in Help and Tutorial system
* Detailed scan reports and statistics
* Modern dark mode interface
* Offline operation
* No installation required
* No administrator privileges required
* Beginner-friendly workflow

---

## What is a VMR File?

A VMR (Virtual Model Rule) file tells vPilot which aircraft model should be displayed when another pilot connects to the VATSIM network.

Without proper model matching you may encounter:

* Incorrect aircraft types
* Missing liveries
* Generic substitute aircraft
* Reduced realism during online flying

TrafficMatch Builder helps generate these matching rules automatically based on the traffic models already installed on your system.

---

## How It Works

TrafficMatch Builder scans the folder selected by the user and searches for aircraft configuration files used by installed traffic model packages.

The application automatically reads information such as:

* Aircraft model names
* Aircraft type codes
* Airline codes
* Available model variations

Using this information, TrafficMatch Builder generates model matching rules and exports them into a ready-to-use VMR file.

The application does not install aircraft, download content or modify simulator files.

---

## Quick Start

1. Launch TrafficMatch Builder
2. Select your preferred language
3. Choose the folder containing your traffic models
4. Click **Scan Models**
5. Review the detected aircraft models
6. Click **Export VMR**
7. Save the generated file
8. Import the VMR file into vPilot

The entire process usually takes less than a minute.

---

## Version 0.2 Beta

Version 0.2 Beta introduces the largest update since the project began.

### New in Version 0.2 Beta

* Complete rewrite in C# / .NET 8
* New standalone Windows executable
* Improved traffic model scanning engine
* Improved VMR generation logic
* Improved compatibility with large traffic model libraries
* Integrated multi-language support
* Built-in Help and Tutorial system
* Enhanced scan reports and statistics
* Improved user interface
* Reduced antivirus false-positive detections
* Numerous internal improvements and optimizations

---

## Portable Application

TrafficMatch Builder is a portable application.

* No installation required
* No installer included
* No administrator privileges required

Simply launch the executable and start generating VMR files.

---

## Security & Privacy

TrafficMatch Builder operates entirely offline.

The application:

* Does not collect personal information
* Does not collect telemetry
* Does not upload files
* Does not download files
* Does not modify simulator files
* Does not modify traffic model files
* Does not modify Windows settings
* Does not connect to external servers
* Does not connect to the VATSIM network

TrafficMatch Builder only reads aircraft configuration files selected by the user and generates model matching files based on the information found.

---

## Disclaimer

TrafficMatch Builder is designed to work with traffic packages for Microsoft Flight Simulator by indexing installed aircraft models and generating configuration files.

Users are solely responsible for ensuring that their use of third-party content complies with the respective terms of use.

The developer does not endorse or encourage any use that violates third-party conditions.

---

## Beta Notice

TrafficMatch Builder is currently an early beta release.

While extensive testing has been performed, minor bugs, compatibility issues or unexpected behavior may still occur.

Feedback, bug reports and feature suggestions are always welcome and help improve future versions of the project.

---

## Legal Notice

TrafficMatch Builder is an independent community project.

This software is not affiliated with, endorsed by or sponsored by VATSIM, vPilot, Microsoft, Asobo Studio, FSLTL, AIG or any other third-party organization.

All trademarks and product names are the property of their respective owners.

---

## License

Copyright © 2026 Nils

TrafficMatch Builder is provided free of charge.

Redistribution of the original, unmodified release package is permitted.

Modification, repackaging, redistribution under a different name or claiming the project as your own work is not permitted without prior permission from the author.

---

**Built by simmers, for simmers.**
