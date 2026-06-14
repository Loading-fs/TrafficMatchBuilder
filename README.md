# TrafficMatch Builder

![TrafficMatch Builder](Screenshots/banner.png)

**Automatic Model Matching Generator for Microsoft Flight Simulator, vPilot and VATSIM**

TrafficMatch Builder is a lightweight utility that automatically scans installed traffic model libraries, supported addon liveries and Microsoft Flight Simulator Community packages to generate ready-to-use VMR (Virtual Model Rule) files for vPilot and VATSIM.

Instead of manually editing XML files and creating model matching rules by hand, TrafficMatch Builder analyzes available aircraft configuration data and generates matching configurations within seconds.

---

## Features

* Automatic traffic library scanning
* Automatic addon livery scanning
* Automatic Microsoft Flight Simulator package discovery
* Automatic VMR generation
* Fenix A320 Family support
* PMDG 737 Family support
* PMDG 777 Family support
* Automatic base aircraft resolution
* Confidence-based model validation
* Multi-language user interface
* Built-in Help and Tutorial system
* Detailed scan reports
* Supported Packs information window
* Guided Community Folder workflow
* Modern dark mode interface
* Offline operation
* Portable application
* No installation required
* No administrator privileges required
* Community folder protection warning
* Built-in About and Credits windows
* Application branding and custom icon

---

## Screenshots

### Language Selection

![Language Selection](Screenshots/language-selection.png)

Choose your preferred language when launching the application.

---

### Main Window

![Main Window](Screenshots/main-window.png)

Guided workflow:

**Select Community Folder → Select Scan Source → Scan Models → Export VMR**

The application guides users through the complete process with a clean and beginner-friendly interface.

---

### Scan Complete

![Scan Complete](Screenshots/scan-complete.png)

TrafficMatch Builder automatically detects installed aircraft models, resolves supported addon packages and generates ready-to-use VMR rules for export.

---

## What is a VMR File?

A VMR (Virtual Model Rule) file tells vPilot which aircraft model should be displayed when another pilot connects to the VATSIM network.

When flying online, vPilot receives information such as:

* Aircraft type
* Airline code
* Callsign
* Livery information

It then attempts to match this information with aircraft models installed on your computer.

Without proper model matching you may encounter:

* Incorrect aircraft types
* Missing liveries
* Generic substitute aircraft
* Reduced realism during online flying

TrafficMatch Builder helps automate the creation of these matching rules based on the traffic models and supported addon liveries already installed on your system.

---

## How It Works

TrafficMatch Builder scans the selected folder and searches for aircraft configuration files used by traffic model packages and supported addon liveries.

The application automatically reads information such as:

* Aircraft model names
* Aircraft type codes
* Airline codes
* Available model variations
* Livery metadata
* Supported package information

Using this information, TrafficMatch Builder generates VMR rules and exports them into a ready-to-use VMR file.

The application does not install aircraft, download content or modify simulator files.

It only reads aircraft configuration data and generates model matching rules based on the information found.

---

## Traffic Library Scan vs Addon Livery Scan

TrafficMatch Builder supports two different scanning methods.

### Traffic Library Scan Recommended

Designed for:

* FSLTL
* AIG
* Other AI traffic model libraries
* Custom traffic model collections

Advantages:

* Best performance
* Lowest simulator resource usage
* Recommended for most users
* Large airline coverage

### Addon Livery Scan

Supported addons:

* Fenix A320 Family
* PMDG 737 Family
* PMDG 777 Family

Addon aircraft are full-fidelity aircraft and may consume significantly more simulator resources than dedicated traffic models.

For best results, addon liveries should only be used when the desired livery is not already available within a traffic library such as FSLTL or AIG.

Other addon aircraft may work but are currently unsupported and untested.

---

## Quick Start

1. Launch TrafficMatch Builder
2. Select your preferred language
3. Select your Microsoft Flight Simulator Community folder
4. Select a traffic library or supported addon livery
5. Click **Scan Models**
6. Review the scan report
7. Click **Export VMR**
8. Save the generated VMR file
9. Import the generated file into vPilot

Simply scan, export and fly.

### Important

For best results, select a dedicated traffic model package such as FSLTL, AIG or another AI traffic library.

Scanning the entire Community folder is possible but may increase scan times and produce less useful results.

---

## Automatic MSFS Discovery

TrafficMatch Builder can automatically search for:

* Microsoft Flight Simulator Community folders
* Compatible traffic libraries
* Fenix aircraft packages
* PMDG aircraft packages
* Supported addon liveries

The application attempts to automatically resolve compatible aircraft models whenever possible.

---

## Community Folder Workflow

TrafficMatch Builder uses a guided Community Folder workflow.

The Community folder is selected before choosing the actual scan source.

This allows the application to automatically discover:

* Fenix aircraft packages
* PMDG aircraft packages
* Related addon liveries
* Additional traffic model packages

This improves model detection and helps generate more reliable VMR rules.

---

## Version 0.6 Beta

### Highlights

* Visual overhaul and UI polish
* Improved overall application layout
* Improved Help window readability
* Improved Credits window layout
* Improved Supported Packs window
* Improved scan report readability
* Improved Community Folder workflow
* Improved application branding
* Improved application icon handling
* Improved localization and translation consistency
* Improved warning messages and user guidance
* Improved window sizing and scaling
* Fixed multiple UI layout issues
* Fixed text selection issue in the Help window
* Fixed various translation inconsistencies
* Fixed several visual bugs
* Fixed multiple stability issues
* General code cleanup and optimization
* Additional bug fixes and performance improvements

---

## Portable Application

TrafficMatch Builder is fully portable.

* No installation required
* No setup wizard
* No administrator privileges required

Simply download, extract and launch the application.

### Important

Version 0.6 Beta includes an **Assets** folder containing application branding resources.

Do not remove the **Assets** folder from the release package.

The executable and Assets folder should remain together.

Expected release structure:

```text
TrafficMatchBuilder.exe
Assets/
  banner.png
  logo.png
  icon.ico