# M59AdminTool

Ein umfassendes Administrations- und Verwaltungstool für Meridian 59 Spielserver.

[🇬🇧 English Version](README.md)

![.NET 8.0](https://img.shields.io/badge/.NET-8.0-blue)
![WPF](https://img.shields.io/badge/UI-WPF-blue)
![License](https://img.shields.io/badge/license-MIT-green)

## 📋 Inhaltsverzeichnis

- [Übersicht](#übersicht)
- [Features](#features)
- [Screenshots](#screenshots)
- [Installation](#installation)
- [Verwendung](#verwendung)
- [Projektstruktur](#projektstruktur)
- [Technologien](#technologien)
- [Konfiguration](#konfiguration)
- [Entwicklung](#entwicklung)
- [Mitwirken](#mitwirken)
- [Lizenz](#lizenz)

## 🎮 Übersicht

**M59AdminTool** ist eine moderne, funktionsreiche Desktop-Anwendung für Meridian 59 Serveradministratoren und Dungeon Master. Entwickelt mit WPF und .NET 8.0, bietet es eine zentrale Oberfläche für Serververwaltung, Spielerüberwachung, Entity-Spawning, Quest-Bearbeitung und vieles mehr.

### Was ist Meridian 59?

Meridian 59 ist eines der ersten 3D-MMORPGs, das ursprünglich 1996 veröffentlicht wurde. Dieses Tool hilft Serveradministratoren, ihre Meridian 59 Serverinstanzen mit einer intuitiven grafischen Oberfläche zu verwalten.

## ✨ Features

### 🔌 Verbindungsverwaltung
- TCP-Verbindung zum Meridian 59 Server
- Unterstützung des PI-Verschlüsselungsprotokolls
- Sichere Authentifizierung mit Benutzername, Passwort und Secret Key
- Echtzeit-Verbindungsstatusüberwachung
- Zugriff auf Debug-Logs

### 🌍 Warp-Verwaltung
- Erstellen, Bearbeiten und Löschen von Teleportationsorten
- Organisation von Warps in Kategorien
- Such- und Filterfunktionen
- Import/Export von Warp-Konfigurationen (JSON)
- Dauerhafte Speicherung in Benutzer-AppData

### 👾 Monster & Item Datenbank
- Umfassender Monster-Datenbank-Browser
- Durchsuchbarer Item-Katalog
- DM-Spawn-Befehle für Monster und Items
- Unterstützung für Englisch/Deutsche Lokalisierung
- Schnellzugriff auf Entity-IDs und Klassennamen

### 🎯 Dungeon Master Befehle
- **Spielerbewegung**
  - GoRoom - Teleport zu Raum
  - GotoPlayer - Teleport zu Spieler
  - GetPlayer - Spieler zu sich holen
- **Sichtbarkeitskontrollen**
  - Stealth-Modus
  - Anonymous-Modus
  - Blank-Avatar-Modus
- **Schatz-Verwaltung**
  - Benutzerdefinierte Schatzgenerierung

### 👨‍💼 Admin-Tools
- Erstellen von Admin- und DM-Konten
- Ausführen benutzerdefinierter Admin-Befehle
- Echtzeit-Befehlsantwort-Panel
- Kontoverwaltungsschnittstelle
- Direkter Admin-Konsolenzugriff

### 👥 Spielerüberwachung
- Echtzeit-Online-Spielerliste
- Raumstandort-Tracking
- Spielerdetails über "who"-Befehl
- Aktualisierungsfunktion

### 🔍 Erweiterte Tools
- **Deep Object Inspector** - Debug von Spielobjekten
- **List Reader** - Parsen von Server-gesendeten Listen
- **Event Manager** - Integriertes Event-Management-System
- **BGF Converter** - Konvertierung von BMP-Bildern in BGF-Format

### 🎨 BGF Converter
- Batch-Konvertierung von BMP-Dateien in BGF-Format
- Live-Vorschau mit Palettenvalidierung
- 8-Bit-Indexed-Color-Unterstützung (256 Farben)
- Optionale zlib-Kompression
- 90°-Drehung für Wand-Texturen
- Konfigurierbarer Shrink-Faktor und Offsets
- Fortschrittsverfolgung

### 🌐 Lokalisierung
- Mehrsprachige Unterstützung (Englisch/Deutsch)
- Sprachwechsel im Header
- Erweiterbares Lokalisierungssystem

## 📸 Screenshots

*Screenshots werden bald hinzugefügt*

## 🚀 Installation

### Voraussetzungen

- **Windows OS** (WPF-Anforderung)
- **.NET 8.0 Runtime** oder SDK
- **Meridian 59 Server** (läuft und ist erreichbar)
- **Admin/DM-Zugangsdaten** für den Server

### Download

1. Laden Sie die neueste Version von der [Releases](../../releases)-Seite herunter
2. Entpacken Sie die ZIP-Datei in einen Ordner Ihrer Wahl
3. Führen Sie `M59AdminTool.exe` aus

### Aus Quellcode erstellen

```bash
# Repository klonen
git clone https://github.com/yourusername/M59AdminTool.git
cd M59AdminTool

# Abhängigkeiten wiederherstellen
dotnet restore

# Projekt erstellen
dotnet build

# Anwendung ausführen
dotnet run
```

## 📖 Verwendung

### Ersteinrichtung

1. **Anwendung starten**
   - Starten Sie `M59AdminTool.exe`

2. **Mit Server verbinden**
   - Gehen Sie zum **Connection**-Tab
   - Geben Sie Ihre Serverdetails ein:
     - **Server IP**: z.B. `127.0.0.1` (localhost) oder Remote-IP
     - **Port**: Standard `5959`
     - **Username**: Ihr Admin/DM-Benutzername
     - **Password**: Ihr Passwort
     - **Secret Key**: Standard `347`
   - Klicken Sie auf **Connect**
   - Warten Sie auf den Status "✅ Eingeloggt"

3. **Mit der Verwaltung beginnen**
   - Nach der Verbindung sind alle Tabs funktionsfähig
   - Navigieren Sie zwischen den Tabs, um auf verschiedene Funktionen zuzugreifen

### Schnellstart-Anleitung

#### Warp-Verwaltung
1. Gehen Sie zum **Warps**-Tab
2. Klicken Sie auf **➕ Add Warp**, um einen neuen Teleportationsort zu erstellen
3. Füllen Sie Raum-ID, Koordinaten und Beschreibung aus
4. Organisieren Sie Warps in Kategorien
5. Verwenden Sie **Export**, um Ihre Konfiguration zu speichern

#### Monster spawnen
1. Gehen Sie zum **Monsters**-Tab
2. Suchen Sie nach dem Monster, das Sie spawnen möchten
3. Klicken Sie auf das Monster in der Liste
4. Kopieren Sie den DM-Befehl (z.B. `dm createmob 12 1`)
5. Gehen Sie zum **Admin Console**-Tab und fügen Sie den Befehl ein
6. Drücken Sie Enter zum Spawnen

#### BMPs zu BGF konvertieren
1. Gehen Sie zum **BGF Converter**-Tab
2. Klicken Sie auf **➕ Add...**, um BMP-Dateien auszuwählen
   - BMPs müssen 8-Bit indexed mit genau 256 Farben sein
3. Vorschau der ausgewählten Datei
4. Prüfen Sie den Palettenstatus (✓ = OK, ✗ = ungültig)
5. Setzen Sie Ausgabeverzeichnis und Optionen
6. Klicken Sie auf **🚀 Convert All Files**

#### Spieler verwalten
1. Gehen Sie zum **Players**-Tab
2. Sehen Sie die Echtzeit-Liste der Online-Spieler
3. Klicken Sie auf **Refresh**, um die Liste zu aktualisieren
4. Sehen Sie Spielerstandorte und Details

## 📁 Projektstruktur

```
M59AdminTool/
├── ViewModels/          # MVVM ViewModels (13 Klassen)
├── Services/            # Business-Logik und Datenservices (19 Klassen)
├── Views/               # WPF-XAML-Views und Dialoge
├── Models/              # Datenmodelle (12 Klassen)
├── Protocol/            # Meridian 59 Protokoll-Implementierung (11 Klassen)
├── Converters/          # XAML-Value-Converter
├── Data/                # JSON-Datendateien (Monster, Items, etc.)
├── Resources/           # Bilder und Ressourcen
└── Scripts/             # Utility-Skripte
```

### Hauptkomponenten

- **M59ServerConnection.cs** - TCP-Verbindung und Protokoll-Handler
- **PIEncryption.cs** - Meridian 59 PI-Verschlüsselungs-Implementierung
- **WarpsDataService.cs** - Warp-Persistenz (JSON in AppData)
- **LocalizationService.cs** - Mehrsprachige Unterstützung
- **BGFWriter.cs** - BGF-Format-Writer mit Kompression
- **DIBHelper.cs** - Bitmap-Validierung und -Verarbeitung

## 🛠️ Technologien

### Frontend
- **WPF (Windows Presentation Foundation)** - UI-Framework
- **XAML** - Deklaratives UI-Markup
- **MVVM-Pattern** - Architektur via CommunityToolkit.Mvvm

### Backend
- **.NET 8.0** - Runtime-Framework
- **C# 12** - Programmiersprache
- **System.Net.Sockets** - TCP-Networking

### Bibliotheken
- **CommunityToolkit.Mvvm 8.4.0** - MVVM-Framework
- **Microsoft.Extensions.Configuration 8.0.0** - Konfigurations-Management
- **Xein.SharpZipLib 1.3.3** - Kompressions-Utilities

### Datenformate
- **JSON** - Konfiguration und Datenpersistenz
- **BGF** - BlakSton Graphics Format (Spieltexturen)
- **Named Pipes** - IPC mit M59-Spiel-Client

## ⚙️ Konfiguration

### Anwendungseinstellungen

Konfigurationsdateien werden gespeichert in:
- **Anwendungsverzeichnis**: `Data\appsettings.json`
- **Benutzerdaten**: `%APPDATA%\M59AdminTool\warps.json`

### Datendateien

Die folgenden Datendateien sind enthalten:
- `Data\monsters.json` - Monster-Datenbank
- `Data\items.json` - Item-Datenbank
- `Data\room_names_german.json` - Deutsche Raum-Übersetzungen
- `Data\important_lists.json` - Vordefinierte Listen

### Serververbindung

Standard-Verbindungseinstellungen:
```
Host: 127.0.0.1
Port: 5959
Secret Key: 347
```

Diese können im Connection-Tab geändert werden.

## 👨‍💻 Entwicklung

### Erstellen

```bash
# Debug-Build
dotnet build

# Release-Build
dotnet build -c Release

# Als einzelne Datei veröffentlichen
dotnet publish -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true
```

### Projektabhängigkeiten

Diese Lösung umfasst:
- **M59AdminTool** - Hauptanwendung
- **Meridian59EventManager** - Event-Management-System

### Debugging

1. Öffnen Sie `M59AdminTool.sln` in Visual Studio 2022
2. Setzen Sie M59AdminTool als Startprojekt
3. Drücken Sie F5 zum Debuggen

### Features hinzufügen

Die Anwendung folgt dem MVVM-Pattern:
1. Erstellen Sie ein ViewModel in `ViewModels/`
2. Erstellen Sie eine View in `Views/` (XAML + Code-Behind)
3. Fügen Sie einen neuen Tab in `MainWindow.xaml` hinzu
4. Verdrahten Sie den DataContext in `MainWindow.xaml.cs`

## 🤝 Mitwirken

Beiträge sind willkommen! Bitte befolgen Sie diese Schritte:

1. Forken Sie das Repository
2. Erstellen Sie einen Feature-Branch (`git checkout -b feature/amazing-feature`)
3. Committen Sie Ihre Änderungen (`git commit -m 'Add amazing feature'`)
4. Pushen Sie zum Branch (`git push origin feature/amazing-feature`)
5. Öffnen Sie einen Pull Request

## 📝 Lizenz

Dieses Projekt ist unter der MIT-Lizenz lizenziert - siehe die [LICENSE](LICENSE)-Datei für Details.

## 🙏 Credits

- **Original makebgf Tool** - Andrew Kirmse und Chris Kirmse
- **Meridian 59** - Open-Source-MMORPG
- **Community-Mitwirkende** - Vielen Dank!

## 📞 Support

Für Probleme, Fragen oder Feature-Anfragen:
- Öffnen Sie ein [Issue](../../issues)
- Prüfen Sie das [Wiki](../../wiki) (kommt bald)

## 🗺️ Roadmap

- [ ] Mehr Admin-Befehle hinzufügen
- [ ] Raum-Editor-Integration
- [ ] Automatisches Backup-System
- [ ] Plugin-System für Erweiterungen
- [ ] Web-basiertes Admin-Panel

## 📊 Status

- **Version**: 1.0.0
- **Status**: Aktive Entwicklung
- **Zuletzt aktualisiert**: 2026-01-14

---

Mit ❤️ für die Meridian 59 Community entwickelt
