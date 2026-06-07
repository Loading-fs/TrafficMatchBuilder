TrafficMatch Builder v0.2 Beta

Built by simmers, for simmers.

============================================================
WELCOME
=======

Thank you for downloading TrafficMatch Builder.

TrafficMatch Builder is a lightweight utility designed to simplify model matching for vPilot and VATSIM users. Instead of manually editing XML files and creating model matching rules by hand, the application automatically scans your installed traffic model libraries and generates ready-to-use VMR files based on the aircraft models available on your system.

Whether you use FSLTL, AIG, custom traffic model collections or other compatible traffic libraries, TrafficMatch Builder helps automate the VMR creation process and significantly reduces the amount of manual work required to maintain model matching configurations.

The goal of the project is simple:

Create accurate VMR files with only a few clicks.

No manual XML editing.
No complicated setup process.
No advanced configuration required.

============================================================
WHAT IS A VMR FILE?
===================

A VMR (Virtual Model Rule) file is used by vPilot to determine which aircraft model should be displayed when another pilot connects to the VATSIM network.

When flying online, vPilot receives information such as:

• Aircraft type

• Airline code

• Callsign

• Livery information

It then attempts to match this information with aircraft models installed on your computer.

Without proper model matching you may encounter:

• Incorrect aircraft types

• Missing liveries

• Generic substitute aircraft

• Reduced realism during online flying

TrafficMatch Builder helps generate these matching rules automatically.

============================================================
HOW TRAFFICMATCH BUILDER WORKS
==============================

TrafficMatch Builder scans the folder selected by the user and searches for aircraft configuration files used by installed traffic model packages.

From these files the application automatically reads information such as:

• Aircraft model names

• Aircraft type codes

• Airline codes

• Available model variations

Using the information found, TrafficMatch Builder automatically generates VMR rules and exports them into a VMR file that can be imported directly into vPilot.

The application does not install aircraft, download content or modify simulator files.

It only reads aircraft configuration data and generates model matching rules based on the information found.

============================================================
QUICK START GUIDE
=================

1. Launch TrafficMatch Builder

2. Select your preferred language

3. Choose the folder containing your traffic models

4. Click "Scan Models"

5. Review the detected aircraft models

6. Click "Export VMR"

7. Save the generated VMR file

8. Import the VMR file into vPilot

The entire process usually takes less than a minute.

============================================================
VERSION 0.2 BETA
================

Version 0.2 Beta introduces the largest update since the project began.

This release includes a complete rewrite of the application using C# and .NET 8, providing improved stability, performance and maintainability while preserving the simple workflow that TrafficMatch Builder is known for.

New in Version 0.2 Beta:

✔ Complete rewrite in C# / .NET 8

✔ New standalone Windows executable

✔ Improved traffic model scanning engine

✔ Improved VMR generation logic

✔ Improved compatibility with large traffic model libraries

✔ Integrated multi-language support

✔ Built-in Help and Tutorial system

✔ Enhanced scan reports and statistics

✔ Improved user interface

✔ Reduced antivirus false-positive detections

✔ Numerous internal improvements and optimizations

============================================================
FEATURES
========

✔ Automatic traffic model scanning

✔ Automatic VMR generation

✔ Multi-language user interface

✔ Built-in Help and Tutorial system

✔ Detailed scan reports

✔ Custom export file names

✔ Modern dark mode interface

✔ Offline operation

✔ Support for custom traffic model collections

✔ Generation of multiple VMR files

✔ No manual XML editing required

✔ Beginner-friendly workflow


============================================================
PORTABLE APPLICATION
====================

TrafficMatch Builder is a portable application.

No installation is required.

No installer is included.

Simply launch the executable file and begin using the application.

To remove TrafficMatch Builder from your system, simply delete the executable and any generated VMR files you no longer wish to keep.

No uninstaller is required.

============================================================
SECURITY, PRIVACY & SYSTEM BEHAVIOR
===================================

TrafficMatch Builder was designed with simplicity, transparency and privacy in mind.

The application:

• Does not require administrator privileges

• Does not install Windows services

• Does not create startup entries

• Does not run background processes after closing

• Does not modify simulator files

• Does not modify traffic model files

• Does not modify vPilot configuration files

• Does not modify Windows settings

• Does not collect personal information

• Does not collect telemetry

• Does not send usage statistics

• Does not upload files

• Does not download files

• Does not connect to external servers

• Does not connect to the VATSIM network

TrafficMatch Builder only performs the following actions:

• Reads aircraft configuration files selected by the user

• Analyzes available aircraft model information

• Generates VMR model matching rules

• Saves exported VMR files to a user-selected location

The application operates entirely offline.

An active internet connection is not required for normal operation.

============================================================
ANTIVIRUS NOTICE
================

TrafficMatch Builder is currently distributed as an unsigned Windows application.

Because newly released software may have limited reputation data, some antivirus products may occasionally report heuristic or machine-learning based false positive detections.

The application does not contain malicious functionality.

Users are encouraged to perform their own security checks if desired.

Microsoft Defender and other antivirus products regularly update their detection databases, which may influence future scan results.

============================================================
NEED HELP?
==========

TrafficMatch Builder includes a built-in Help and Tutorial system.

Simply click the "Help" button in the top-right corner of the main window to access a quick guide explaining the complete workflow from folder selection to VMR export.

============================================================
BETA NOTICE
===========

TrafficMatch Builder is currently an early beta release.

While extensive testing has been performed, minor bugs, compatibility issues or unexpected behavior may still occur.

Feedback, bug reports and feature suggestions are always welcome and help improve future versions of the project.

============================================================
ABOUT THIS PROJECT
==================

TrafficMatch Builder is a free community project created for flight simulation enthusiasts.

The application is completely free to use and there are currently no plans for subscriptions, advertisements, premium editions or paid features.

The goal is to provide a simple and reliable solution for generating VMR model matching files without requiring advanced technical knowledge.

Development, testing and feature design were supported through the use of AI-assisted development tools.

Thank you for using TrafficMatch Builder and enjoy your flights.

============================================================
LEGAL NOTICE
============

TrafficMatch Builder is an independent community project.

This software is not affiliated with, endorsed by or sponsored by VATSIM, vPilot, Microsoft, Asobo Studio, FSLTL, AIG or any other third-party organization.

All trademarks and product names are the property of their respective owners.

============================================================
COPYRIGHT & DISTRIBUTION
========================

Copyright (c) 2026 Nils

TrafficMatch Builder is provided free of charge.

Redistribution of the original, unmodified release package is permitted.

Modification, repackaging, reverse engineering for redistribution, re-uploading under a different name or claiming the project as your own work is not permitted without prior permission from the author.
