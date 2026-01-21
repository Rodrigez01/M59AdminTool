# M59AdminTool

A comprehensive administration and management tool for Meridian 59 game servers.

[🇩🇪 Deutsche Version](README_DE.md)

![.NET 8.0](https://img.shields.io/badge/.NET-8.0-blue)
![WPF](https://img.shields.io/badge/UI-WPF-blue)
![License](https://img.shields.io/badge/license-MIT-green)

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Screenshots](#screenshots)
- [Installation](#installation)
- [Usage](#usage)
- [Project Structure](#project-structure)
- [Technologies](#technologies)
- [Configuration](#configuration)
- [Development](#development)
- [Contributing](#contributing)
- [License](#license)

## 🎮 Overview

**M59AdminTool** is a modern, feature-rich desktop application for Meridian 59 server administrators and dungeon masters. Built with WPF and .NET 8.0, it provides a centralized interface for server management, player monitoring, entity spawning, quest editing, and much more.

### What is Meridian 59?

Meridian 59 is one of the first 3D MMORPGs, originally released in 1996. This tool helps server administrators manage their Meridian 59 server instances with an intuitive graphical interface.

## ✨ Features

### 🔌 Connection Management
- TCP connection to Meridian 59 server
- PI encryption protocol support
- Secure authentication with username, password, and secret key
- Real-time connection status monitoring
- Debug log access

### 🌍 Warp Management
- Create, edit, and delete teleportation locations
- Organize warps into categories
- Search and filter functionality
- Import/Export warp configurations (JSON)
- Persistent storage in user AppData
- refreshes directly from kodfiles
- sorted by category from system.kod

### 👾 Monster & Item Database
- Comprehensive monster database browser
- Searchable item catalog
- DM spawn commands for monsters and items
- English/German localization support
- Quick access to entity IDs and class names
- refreshes directly from kodfiles
- sorted by category from system.kod

### 🎯 Dungeon Master Commands
- **Player Movement**
  - GoRoom - Teleport to room
  - GotoPlayer - Teleport to player
  - GetPlayer - Bring player to you
- **Visibility Controls**
  - Stealth mode
  - Anonymous mode
  - Blank avatar mode
- **Treasure Management**
  - Custom treasure generation

### 👨‍💼 Admin Tools
- Create admin and DM accounts
- Execute custom admin commands
- Real-time command response panel
- Account management interface
- Direct admin console access

### 👥 Player Monitoring
- Real-time online player list
- Room location tracking
- Player details via "who" command
- Refresh functionality

### 🗺️ Quest Editor
- Integrated Quest Editor (embedded project)
- Visual quest creation and editing
- Quest flow management
- NPC interaction setup

### 🔍 Advanced Tools
- **Deep Object Inspector** - Debug game objects
- **List Reader** - Parse server-sent lists
- **Event Manager** - Integrated event management system
- **BGF Converter** - Convert BMP images to BGF format

### 🎨 BGF Converter
- Batch conversion of BMP files to BGF format
- Live preview with palette validation
- 8-bit indexed color support (256 colors)
- Optional zlib compression
- 90° rotation for wall textures
- Configurable shrink factor and offsets
- Progress tracking

### 🌐 Localization
- Multi-language support (English/German)
- Language toggle in header
- Extensible localization system

## 📸 Screenshots

<img width="1922" height="1030" alt="grafik" src="https://github.com/user-attachments/assets/5f65b0af-f094-428a-9b9c-1b03d66c1bad" />



## 🚀 Installation

### Prerequisites

- **Windows OS** (WPF requirement)
- **.NET 8.0 Runtime** or SDK
- **Meridian 59 Server** (running and accessible)
- **Admin/DM credentials** for the server

### Download

1. Download the latest release from the [Releases](../../releases) page
2. Extract the ZIP file to a folder of your choice
3. Run `M59AdminTool.exe`

### Build from Source

```bash
# Clone the repository
git clone https://github.com/yourusername/M59AdminTool.git
cd M59AdminTool

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

## 📖 Usage

### Initial Setup

1. **Start the Application**
   - Launch `M59AdminTool.exe`

2. **Connect to Server**
   - Go to the **Connection** tab
   - Enter your server details:
     - **Server IP**: e.g., `127.0.0.1` (localhost) or remote IP
     - **Port**: Default `5959`
     - **Username**: Your admin/DM username
     - **Password**: Your password
     - **Secret Key**: Default `347`
   - Click **Connect**
   - Wait for "✅ Logged in" status

3. **Start Managing**
   - Once connected, all tabs become functional
   - Navigate between tabs to access different features

### Quick Start Guide

#### Warp Management
1. Go to **Warps** tab
2. Click **➕ Add Warp** to create a new teleportation location
3. Fill in room ID, coordinates, and description
4. Organize warps into categories
5. Use **Export** to save your configuration

#### Spawning Monsters
1. Go to **Monsters** tab
2. Search for the monster you want
3. Click on the monster in the list
4. Copy the DM command (e.g., `dm createmob 12 1`)
5. Go to **Admin Console** tab and paste the command
6. Press Enter to spawn

#### Converting BMPs to BGF
1. Go to **BGF Converter** tab
2. Click **➕ Add...** to select BMP files
   - BMPs must be 8-bit indexed with exactly 256 colors
3. Preview the selected file
4. Check palette status (✓ = OK, ✗ = invalid)
5. Set output directory and options
6. Click **🚀 Convert All Files**

#### Managing Players
1. Go to **Players** tab
2. View real-time list of online players
3. Click **Refresh** to update the list
4. See player locations and details

## 📁 Project Structure

```
M59AdminTool/
├── ViewModels/          # MVVM ViewModels (13 classes)
├── Services/            # Business logic and data services (19 classes)
├── Views/               # WPF XAML views and dialogs
├── Models/              # Data models (12 classes)
├── Protocol/            # Meridian 59 protocol implementation (11 classes)
├── Converters/          # XAML value converters
├── Data/                # JSON data files (monsters, items, etc.)
├── Resources/           # Images and resources
└── Scripts/             # Utility scripts
```

### Key Components

- **M59ServerConnection.cs** - TCP connection and protocol handler
- **PIEncryption.cs** - Meridian 59 PI encryption implementation
- **WarpsDataService.cs** - Warp persistence (JSON in AppData)
- **LocalizationService.cs** - Multi-language support
- **BGFWriter.cs** - BGF format writer with compression
- **DIBHelper.cs** - Bitmap validation and processing

## 🛠️ Technologies

### Frontend
- **WPF (Windows Presentation Foundation)** - UI framework
- **XAML** - Declarative UI markup
- **MVVM Pattern** - Architecture via CommunityToolkit.Mvvm

### Backend
- **.NET 8.0** - Runtime framework
- **C# 12** - Programming language
- **System.Net.Sockets** - TCP networking

### Libraries
- **CommunityToolkit.Mvvm 8.4.0** - MVVM framework
- **Microsoft.Extensions.Configuration 8.0.0** - Configuration management
- **Xein.SharpZipLib 1.3.3** - Compression utilities
- **MySqlConnector 2.5.0** - Database support (QuestEditor)

### Data Formats
- **JSON** - Configuration and data persistence
- **BGF** - BlakSton Graphics Format (game textures)
- **Named Pipes** - IPC with M59 game client

## ⚙️ Configuration

### Application Settings

Configuration files are stored in:
- **Application Directory**: `Data\appsettings.json`
- **User Data**: `%APPDATA%\M59AdminTool\warps.json`

### Data Files

The following data files are included:
- `Data\monsters.json` - Monster database
- `Data\items.json` - Item database
- `Data\room_names_german.json` - German room translations
- `Data\SpellDatabase.json` - Spell reference
- `Data\important_lists.json` - Pre-defined lists

### Server Connection

Default connection settings:
```
Host: 127.0.0.1
Port: 5959
Secret Key: 347
```

These can be changed in the Connection tab.

## 👨‍💻 Development

### Building

```bash
# Debug build
dotnet build

# Release build
dotnet build -c Release

# Publish as single file
dotnet publish -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true
```

### Project Dependencies

This solution includes:
- **M59AdminTool** - Main application
- **QuestEditor** - Quest editor component
- **Meridian59EventManager** - Event management system

### Debugging

1. Open `M59AdminTool.sln` in Visual Studio 2022
2. Set M59AdminTool as startup project
3. Press F5 to debug

### Adding Features

The application follows MVVM pattern:
1. Create a ViewModel in `ViewModels/`
2. Create a View in `Views/` (XAML + code-behind)
3. Add a new tab in `MainWindow.xaml`
4. Wire up the DataContext in `MainWindow.xaml.cs`

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Credits

- **Original makebgf Tool** - Andrew Kirmse and Chris Kirmse
- **Meridian 59** - Open source MMORPG
- **Community Contributors** - Thank you!

## 📞 Support

For issues, questions, or feature requests:
- Open an [Issue](../../issues)
- Check the [Wiki](../../wiki) (coming soon)

## 🗺️ Roadmap

- [ ] Add more admin commands
- [ ] Enhanced quest editor features
- [ ] Room editor integration
- [ ] Automated backup system
- [ ] Plugin system for extensions
- [ ] Web-based admin panel

## 📊 Status

- **Version**: 1.0.0
- **Status**: Active Development
- **Last Updated**: 2026-01-14

---

Made with ❤️ for the Meridian 59 community
