using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace M59AdminTool.Services
{
    public enum Language
    {
        English,
        German
    }

    public class LocalizationService : INotifyPropertyChanged
    {
        private static LocalizationService? _instance;
        public static LocalizationService Instance => _instance ??= new LocalizationService();

        private Language _currentLanguage = Language.English;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Language CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    System.Diagnostics.Debug.WriteLine($"LocalizationService: Language changing from {_currentLanguage} to {value}");
                    _currentLanguage = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsGerman));
                    OnPropertyChanged(nameof(IsEnglish));
                    OnPropertyChanged(nameof(LanguageToggleLabel));
                    OnPropertyChanged("Item[]");
                    System.Diagnostics.Debug.WriteLine($"LanguageChanged event has {LanguageChanged?.GetInvocationList().Length ?? 0} subscribers");
                    LanguageChanged?.Invoke(this, value);
                    System.Diagnostics.Debug.WriteLine("LanguageChanged event fired");
                }
            }
        }

        public bool IsGerman => CurrentLanguage == Language.German;
        public bool IsEnglish => CurrentLanguage == Language.English;
        public string LanguageToggleLabel => CurrentLanguage == Language.German
            ? GetString("Button_LanguageToggle_English")
            : GetString("Button_LanguageToggle_German");

        public event EventHandler<Language>? LanguageChanged;

        // UI Strings
        public Dictionary<string, Dictionary<Language, string>> Strings { get; } = new()
        {
            // Window titles
            ["Window_Title"] = new() { [Language.English] = "Meridian 59 Admin Tool", [Language.German] = "Meridian 59 Admin Tool" },
            ["Window_DebugLog_Title"] = new() { [Language.English] = "🐛 Debug Log", [Language.German] = "🐛 Debug-Protokoll" },
            ["Window_Error_Title"] = new() { [Language.English] = "Error", [Language.German] = "Fehler" },
            ["Window_ImagePreview_Title"] = new() { [Language.English] = "Image Preview", [Language.German] = "Bildvorschau" },
            ["Window_ListPreview_TitleFormat"] = new() { [Language.English] = "List {0}", [Language.German] = "Liste {0}" },
            ["Window_Help_Title"] = new() { [Language.English] = "Help", [Language.German] = "Hilfe" },
            ["Window_Notification_Title"] = new() { [Language.English] = "Command Ready", [Language.German] = "Befehl bereit" },
            ["Window_SpellSelect_Title"] = new() { [Language.English] = "Select Spell", [Language.German] = "Zauber wählen" },
            ["Window_UnexpectedError_Title"] = new() { [Language.English] = "Unexpected Error", [Language.German] = "Unerwarteter Fehler" },

            // Headers (explicit where emojis/punctuation matter)
            ["Header_Title"] = new() { [Language.English] = "Meridian 59 Admin Tool", [Language.German] = "Meridian 59 Admin Tool" },
            ["Header_Subtitle"] = new() { [Language.English] = "Server Administration & Management", [Language.German] = "Server-Administration & Verwaltung" },
            ["Header_Connection"] = new() { [Language.English] = "🔌 Server Connection", [Language.German] = "🔌 Serververbindung" },
            ["Header_WarpLocations"] = new() { [Language.English] = "Warp Locations", [Language.German] = "Warp-Orte" },
            ["Header_EditWarp"] = new() { [Language.English] = "Edit Warp", [Language.German] = "Warp bearbeiten" },
            ["Header_MonsterList"] = new() { [Language.English] = "Monster List", [Language.German] = "Monsterliste" },
            ["Header_EditMonster"] = new() { [Language.English] = "Edit Monster", [Language.German] = "Monster bearbeiten" },
            ["Header_ItemList"] = new() { [Language.English] = "Item List", [Language.German] = "Item-Liste" },
            ["Header_EditItem"] = new() { [Language.English] = "Edit Item", [Language.German] = "Item bearbeiten" },
            ["Header_DmCommands"] = new() { [Language.English] = "DM Commands", [Language.German] = "DM-Befehle" },
            ["Header_AdminCommands"] = new() { [Language.English] = "Admin Commands", [Language.German] = "Admin-Befehle" },
            ["Header_AdminConsole"] = new() { [Language.English] = "🖥️ Admin Console", [Language.German] = "🖥️ Admin-Konsole" },
            ["Header_ObjectInspector"] = new() { [Language.English] = "🔍 Object Inspector", [Language.German] = "🔍 Objekt-Inspektor" },
            ["Header_ObjectListReader"] = new() { [Language.English] = "Object List Reader", [Language.German] = "Objektlisten-Leser" },
            ["Header_ObjectDetails"] = new() { [Language.English] = "Object Details", [Language.German] = "Objektdetails" },
            ["Header_OnlinePlayers"] = new() { [Language.English] = "Online Players", [Language.German] = "Online-Spieler" },
            ["Header_PlayerDetails"] = new() { [Language.English] = "Player Details (show object)", [Language.German] = "Spielerdetails (show object)" },
            ["Header_Rooms"] = new() { [Language.English] = "Rooms", [Language.German] = "Räume" },
            ["Header_Help"] = new() { [Language.English] = "Help (Dropdown)", [Language.German] = "Hilfe (Dropdown)" },
            ["Header_UnexpectedError"] = new() { [Language.English] = "An unexpected error occurred. You can copy the details below:", [Language.German] = "Ein unerwarteter Fehler ist aufgetreten. Du kannst die Details unten kopieren:" },
            ["Header_ErrorOccurred"] = new() { [Language.English] = "❌ An error occurred", [Language.German] = "❌ Ein Fehler ist aufgetreten" },
            ["Header_DebugLog"] = new() { [Language.English] = "🐛 Debug Log (Live)", [Language.German] = "🐛 Debug-Protokoll (Live)" },
            ["Header_ListPreview_Format"] = new() { [Language.English] = "List {0} ({1} entries)", [Language.German] = "Liste {0} ({1} Einträge)" },
            ["Help_Section_Welcome"] = new() { [Language.English] = "Welcome", [Language.German] = "Willkommen" },
            ["Help_Section_QuickStart"] = new() { [Language.English] = "Quick Start", [Language.German] = "Schnellstart" },
            ["Help_Section_Tabs"] = new() { [Language.English] = "Tabs Overview", [Language.German] = "Tabs Übersicht" },
            ["Help_Section_Workflows"] = new() { [Language.English] = "Workflows", [Language.German] = "Workflows" },
            ["Help_Section_Workarounds"] = new() { [Language.English] = "Workarounds & Tips", [Language.German] = "Workarounds & Tipps" },
            ["Help_Section_Output"] = new() { [Language.English] = "Output & Navigation", [Language.German] = "Ausgabe & Navigation" },
            ["Help_Section_About"] = new() { [Language.English] = "About & License", [Language.German] = "Über & Lizenz" },
            ["Help_Section_Tab_Connection"] = new() { [Language.English] = "Tab: Connection", [Language.German] = "Tab: Connection" },
            ["Help_Section_Tab_Warps"] = new() { [Language.English] = "Tab: Warps", [Language.German] = "Tab: Warps" },
            ["Help_Section_Tab_Monsters"] = new() { [Language.English] = "Tab: Monsters", [Language.German] = "Tab: Monsters" },
            ["Help_Section_Tab_Items"] = new() { [Language.English] = "Tab: Items", [Language.German] = "Tab: Items" },
            ["Help_Section_Tab_DM"] = new() { [Language.English] = "Tab: DM", [Language.German] = "Tab: DM" },
            ["Help_Section_Tab_Admin"] = new() { [Language.English] = "Tab: Admin", [Language.German] = "Tab: Admin" },
            ["Help_Section_Tab_DJ"] = new() { [Language.English] = "Tab: DJ", [Language.German] = "Tab: DJ" },
            ["Help_Section_Tab_Arena"] = new() { [Language.English] = "Tab: Arena", [Language.German] = "Tab: Arena" },
            ["Help_Section_Tab_Players"] = new() { [Language.English] = "Tab: Players", [Language.German] = "Tab: Players" },
            ["Help_Section_Tab_QuestEditor"] = new() { [Language.English] = "Tab: QuestEditor", [Language.German] = "Tab: QuestEditor" },
            ["Help_Section_Tab_DeepInspector"] = new() { [Language.English] = "Tab: Deep Inspector", [Language.German] = "Tab: Deep Inspector" },
            ["Help_Section_Tab_ListReader"] = new() { [Language.English] = "Tab: List Reader", [Language.German] = "Tab: List Reader" },
            ["Help_Section_Tab_AdminConsole"] = new() { [Language.English] = "Tab: Admin Console", [Language.German] = "Tab: Admin Console" },
            ["Help_Section_Tab_ObjectInspector"] = new() { [Language.English] = "Tab: Object Inspector", [Language.German] = "Tab: Object Inspector" },
            ["Help_Section_Tab_EventManager"] = new() { [Language.English] = "Tab: Event Manager", [Language.German] = "Tab: Event Manager" },
            ["Help_Content_Welcome"] = new()
            {
                [Language.English] = @"M59AdminTool is a compact admin cockpit for Meridian 59 servers.

- It groups DM/Admin tools, lists, and inspectors in one place.
- Most features require an active connection.
- Tabs can be reordered with drag & drop and are saved automatically.",
                [Language.German] = @"M59AdminTool ist ein kompaktes Admin-Cockpit für Meridian 59 Server.

- DM/Admin-Tools, Listen und Inspektoren sind zentral gebündelt.
- Die meisten Funktionen benötigen eine aktive Verbindung.
- Tabs lassen sich per Drag & Drop neu anordnen und werden gespeichert."
            },
            ["Help_Content_QuickStart"] = new()
            {
                [Language.English] = @"1) Open the Connection tab and log in (IP, port, user, password, secret key).
2) Verify the Status area updates and test a command if needed.
3) Use the tab you need (Warps, Items, List Reader, Deep Inspector, etc.).",
                [Language.German] = @"1) Im Tab Connection einloggen (IP, Port, Benutzer, Passwort, Secret Key).
2) Status prüfen und bei Bedarf einen Testbefehl senden.
3) Den gewünschten Tab nutzen (Warps, Items, List Reader, Deep Inspector usw.)."
            },
            ["Help_Content_Tabs"] = new()
            {
                [Language.English] = @"- Connection: Login, status, debug log, test command, last response.
- Warps: Search, edit, execute warp, refresh rooms from KOD, save/reload/export/import, count display.
- Monsters: Search, edit, spawn, refresh from KOD, save/reload, count display.
- Items: Search, edit, spawn, categories, refresh from KOD, save/reload, count display.
- DM: Movement, visibility, disguise, combat tools, summon/test utilities.
- Admin: Accounts, system actions, resources, global actions, advanced object/class commands.
- DJ / Arena: Placeholders for future tools.
- Players: Online list, rooms list, player details, copy actions, deep inspector entry.
- QuestEditor: Embedded quest editor UI.
- Deep Inspector: Show object or list, edit properties, navigate lists/objects, history.
- List Reader: show class / instances / all / list; details view; list preview; edit INT.
- Admin Console: Send raw admin commands and copy responses.
- Object Inspector: Quick show object/users/accounts, raw response, edit selected line.
- Event Manager: Start/schedule/end events, active instances, event log.",
                [Language.German] = @"- Connection: Login, Status, Debug-Log, Testbefehl, letzte Antwort.
- Warps: Suchen, bearbeiten, Warp ausführen, Räume aus KOD aktualisieren, save/reload/export/import, Anzahl.
- Monster: Suchen, bearbeiten, spawnen, aus KOD aktualisieren, save/reload, Anzahl.
- Items: Suchen, bearbeiten, spawnen, Kategorien, aus KOD aktualisieren, save/reload, Anzahl.
- DM: Bewegung, Sichtbarkeit, Disguise, Combat-Tools, Summon/Test.
- Admin: Accounts, Systemaktionen, Ressourcen, globale Aktionen, Objekt/Klassen-Befehle.
- DJ / Arena: Platzhalter für zukünftige Tools.
- Players: Online-Liste, Rooms-Liste, Player-Details, Copy-Aktionen, Deep-Inspector Einstieg.
- QuestEditor: Eingebetteter Quest-Editor.
- Deep Inspector: Object/List anzeigen, Properties editieren, Listen/Object Navigation, Verlauf.
- List Reader: show class / instances / all / list; Detailansicht; Listen-Preview; INT bearbeiten.
- Admin Console: Admin-Befehle senden, Antworten kopieren.
- Object Inspector: show object/users/accounts, Rohantwort, Zeile editieren.
- Event Manager: Events starten/planen/beenden, aktive Instanzen, Eventlog."
            },
            ["Help_Content_Workflows"] = new()
            {
                [Language.English] = @"List Reader:
- Select command type, enter Class Name or List ID, Execute.
- Double-click a result to load details.
- In details, double-click a LIST line to open list preview.
- INT lines can be edited via double-click or context menu (Edit INT).

Deep Inspector:
- Enter object id (or ""list <id>"") and Load.
- Inventory/Spells/Skills buttons jump into common lists.
- Use Back for navigation history.",
                [Language.German] = @"List Reader:
- Command Type auswählen, Class Name oder List ID eintragen, Execute.
- Doppelklick auf Ergebnis -> Details laden.
- In den Details: Doppelklick auf LIST-Zeile -> Liste öffnen.
- INT-Zeilen per Doppelklick oder Kontextmenü bearbeiten.

Deep Inspector:
- Object ID (oder ""list <id>"") eingeben und Load.
- Inventory/Spells/Skills springen in häufige Listen.
- Back nutzt den Verlauf."
            },
            ["Help_Content_Workarounds"] = new()
            {
                [Language.English] = @"- No response: verify connection, wait 1–2 seconds, retry.
- List IDs are detected from lines containing ""LIST"" or ""list id"".
- Use context menus to copy selected/all lines.
- Tabs reorder is saved to %AppData%\\M59AdminTool\\tab-order.json.",
                [Language.German] = @"- Keine Antwort: Verbindung prüfen, 1–2 Sekunden warten, erneut senden.
- Listen-IDs werden aus Zeilen mit ""LIST"" oder ""list id"" erkannt.
- Kontextmenüs für Copy Selected / Copy All nutzen.
- Tab-Reihenfolge wird unter %AppData%\\M59AdminTool\\tab-order.json gespeichert."
            },
            ["Help_Content_Output"] = new()
            {
                [Language.English] = @"- Output lists auto-scroll to the newest line.
- Mouse wheel scrolling is enabled in output panes.
- Double-click in lists triggers details or editing where available.",
                [Language.German] = @"- Ausgaben scrollen automatisch zur letzten Zeile.
- Maus-Rad-Scrollen ist in den Ausgabefenstern aktiv.
- Doppelklick in Listen öffnet Details oder Bearbeitung (wo verfügbar)."
            },
            ["Help_Content_About"] = new()
            {
                [Language.English] = @"Program: M59AdminTool
Open Source License: MIT / GPL
Copyright (c) Frank Hortmann (Rod)",
                [Language.German] = @"Programm: M59AdminTool
Open-Source-Lizenz: MIT / GPL
Copyright (c) Frank Hortmann (Rod)"
            },
            ["Help_Content_Tab_Connection"] = new()
            {
                [Language.English] = @"- Enter IP, Port, Username, Password, Secret Key.
- Click Connect/Login to open the session.
- Use Test Command to verify the server responds.
- Status, Debug Log, and Last Response help diagnose issues.
- Last Response shows the most recent raw server reply.",
                [Language.German] = @"- IP, Port, Benutzername, Passwort, Secret Key eintragen.
- Connect/Login öffnet die Session.
- Test Command prüft die Serverantwort.
- Status, Debug-Log und Last Response helfen bei der Diagnose.
- Last Response zeigt die letzte Rohantwort."
            },
            ["Help_Content_Tab_Warps"] = new()
            {
                [Language.English] = @"- Search and select warp categories/entries.
- Add/Remove categories and warps.
- Edit name, room id, coordinates, description.
- Execute Warp sends you to the selected location (go RID_*).
- Refresh Rooms (from KOD) rebuilds extracted_rooms.json + German names.
- Save/Reload/Export/Import as needed.
- Count shows the number of filtered warps.",
                [Language.German] = @"- Warps suchen und auswählen.
- Kategorien/Warps hinzufügen und löschen.
- Name, Room ID, Koordinaten, Beschreibung bearbeiten.
- Execute Warp teleportiert zum Ziel (go RID_*).
- Räume aktualisieren (aus KOD) erzeugt extracted_rooms.json + deutsche Namen.
- Save/Reload/Export/Import nutzen.
- Anzahl zeigt die gefilterten Warps."
            },
            ["Help_Content_Tab_Monsters"] = new()
            {
                [Language.English] = @"- Search monsters, edit fields and save.
- Add/Remove monsters manually if needed.
- Spawn Monster creates the selected monster in game.
- Refresh Monsters (from KOD) rebuilds monsters.json from server sources.
- Save All / Reload for persistence and rollback.
- Count shows the number of filtered monsters.",
                [Language.German] = @"- Monster suchen, Felder bearbeiten und speichern.
- Monster manuell hinzufügen/entfernen.
- Spawn Monster erzeugt das gewählte Monster im Spiel.
- Monster aktualisieren (aus KOD) baut monsters.json neu aus den Serverquellen.
- Save All / Reload für Speichern und Rücksetzen.
- Anzahl zeigt die gefilterten Monster."
            },
            ["Help_Content_Tab_Items"] = new()
            {
                [Language.English] = @"- Search items, edit fields and save.
- Add/Remove items manually if needed.
- Spawn Item creates the selected item in game.
- Refresh Items (from KOD) rebuilds items.json from server sources.
- Categories can be edited for sorting.
- Save All / Reload for persistence and rollback.
- Count shows the number of filtered items.",
                [Language.German] = @"- Items suchen, Felder bearbeiten und speichern.
- Items manuell hinzufügen/entfernen.
- Spawn Item erzeugt das gewählte Item im Spiel.
- Items aktualisieren (aus KOD) baut items.json neu aus den Serverquellen.
- Kategorien sind bearbeitbar und dienen der Sortierung.
- Save All / Reload für Speichern und Rücksetzen.
- Anzahl zeigt die gefilterten Items."
            },
            ["Help_Content_Tab_DM"] = new()
            {
                [Language.English] = @"- Player movement: goto/get/room travel, go room, get player.
- Visibility: invisible/visible/anonymous/blank/hidden/shadow.
- Disguise: disguise/plain form/human form.
- Combat/tools: boost stats, karma good/neutral/evil, summon/test monster gen points.
- Use carefully on live servers.",
                [Language.German] = @"- Spielerbewegung: goto/get/room travel, go room, get player.
- Sichtbarkeit: invisible/visible/anonymous/blank/hidden/shadow.
- Disguise: disguise/plain form/human form.
- Combat/Tools: Stats boosten, Karma good/neutral/evil, Summon/Test Monster-Genpoints.
- Vorsichtig auf Live-Servern nutzen."
            },
            ["Help_Content_Tab_Admin"] = new()
            {
                [Language.English] = @"- Accounts: create admin/dm/user, finalize, set password.
- System: save game, reload system, recreate, world load/save.
- Global: give item/spell/skill, global give item, hall of heroes, frenzy.
- Resources: create resource, show instance/message, show object.
- Advanced: send object/class commands, set object/class properties.",
                [Language.German] = @"- Accounts: Admin/DM/User anlegen, finalisieren, Passwort setzen.
- System: save game, reload system, recreate, world load/save.
- Global: Item/Zauber/Skill geben, global item, Hall of Heroes, Frenzy.
- Ressourcen: Resource erstellen, Instanz/Message anzeigen, Objekt anzeigen.
- Advanced: Objekt/Klassen-Befehle, Objekt/Klassen-Properties setzen."
            },
            ["Help_Content_Tab_DJ"] = new()
            {
                [Language.English] = @"- Placeholder for future DJ tools.",
                [Language.German] = @"- Platzhalter für spätere DJ-Tools."
            },
            ["Help_Content_Tab_Arena"] = new()
            {
                [Language.English] = @"- Placeholder for future Arena tools.",
                [Language.German] = @"- Platzhalter für spätere Arena-Tools."
            },
            ["Help_Content_Tab_Players"] = new()
            {
                [Language.English] = @"- Online players list (who) with context actions.
- Rooms list for quick overview.
- Player details from show object output.
- Double-click INT lines to edit, right-click for copy options.
- Open selected player in Deep Inspector.",
                [Language.German] = @"- Online-Spielerliste (who) mit Kontextaktionen.
- Rooms-Liste für Übersicht.
- Player-Details aus show object.
- INT-Zeilen per Doppelklick bearbeiten, Rechtsklick für Kopieren.
- Spieler im Deep Inspector öffnen."
            },
            ["Help_Content_Tab_QuestEditor"] = new()
            {
                [Language.English] = @"QuestEditor (embedded)

Quick Start:
1) Settings → set Server Root and auto-fill paths, then save.
2) New Quest → set Quest Name, KOD Class (NO spaces), Icon.
3) Node 1 (MESSAGE): add NPC, Cargo trigger.
4) Node 2 (MONSTER/ITEM): add task, dialogs, rewards.
5) Save Quest → Build & Deploy → restart server.

Structure:
- Node 1: MESSAGE with Cargo trigger, no Dialogs, no Rewards.
- Node 2: MONSTER/ITEM main task, Dialogs + Rewards here.
- Node 3: SHOWUP optional final message, no Rewards.

Build & Deploy:
- Saves .kod, compiles to .bof, compiles NPCs,
  builds resources/bundles, copies to client.
- Restart server after deploy.

NPC Browser:
- Validate NPCs to find MOB_NOQUEST.
- Fix NPCs to remove MOB_NOQUEST and add MOB_LISTEN.

Common mistakes:
- Rewards must be on MONSTER/ITEM node (Node 2).
- Node 1 is MESSAGE with Cargo only (no Dialogs).
- KOD Class without spaces; trigger word in Cargo.",
                [Language.German] = @"QuestEditor (eingebettet)

Schnellstart:
1) Settings → Server Root setzen, Pfade auto-füllen, speichern.
2) New Quest → Quest Name, KOD Class (KEINE Leerzeichen), Icon.
3) Node 1 (MESSAGE): NPC hinzufügen, Cargo Trigger.
4) Node 2 (MONSTER/ITEM): Aufgabe, Dialoge, Rewards.
5) Save Quest → Build & Deploy → Server neu starten.

Struktur:
- Node 1: MESSAGE mit Cargo-Trigger, keine Dialoge, keine Rewards.
- Node 2: MONSTER/ITEM Hauptaufgabe, Dialoge + Rewards hier.
- Node 3: SHOWUP optionaler Abschluss, keine Rewards.

Build & Deploy:
- Speichert .kod, kompiliert zu .bof, kompiliert NPCs,
  erstellt Ressourcen/Bundles, kopiert zum Client.
- Nach Deploy Server neu starten.

NPC Browser:
- Validate NPCs prüft MOB_NOQUEST.
- Fix NPCs entfernt MOB_NOQUEST und setzt MOB_LISTEN.

Häufige Fehler:
- Rewards gehören auf MONSTER/ITEM (Node 2).
- Node 1 ist MESSAGE mit Cargo (keine Dialoge).
- KOD Class ohne Leerzeichen; Trigger-Wort im Cargo."
            },
            ["Help_Content_Tab_DeepInspector"] = new()
            {
                [Language.English] = @"- Load object by ID or list by ""list <id>"".
- Use Inventory/Spells/Skills to jump to list properties.
- Edit properties, navigate list/object references, use history.",
                [Language.German] = @"- Object per ID oder Liste via ""list <id>"" laden.
- Inventory/Spells/Skills springen zu Listen-Properties.
- Properties bearbeiten, Listen/Object-Referenzen öffnen, Verlauf nutzen."
            },
            ["Help_Content_Tab_ListReader"] = new()
            {
                [Language.English] = @"- Execute show class / instances / all / list.
- Double-click a result to fetch details.
- Double-click LIST lines to open list preview.
- INT lines can be edited via double-click/context menu.",
                [Language.German] = @"- show class / instances / all / list ausführen.
- Doppelklick auf Ergebnis lädt Details.
- LIST-Zeilen per Doppelklick öffnen Listen-Preview.
- INT-Zeilen per Doppelklick/Kontextmenü bearbeiten."
            },
            ["Help_Content_Tab_AdminConsole"] = new()
            {
                [Language.English] = @"- Send raw admin commands.
- Responses are stored and can be copied.",
                [Language.German] = @"- Rohbefehle an den Server senden.
- Antworten werden gesammelt und können kopiert werden."
            },
            ["Help_Content_Tab_ObjectInspector"] = new()
            {
                [Language.English] = @"- Quick show object/users/accounts.
- Double-click lines to edit property values.
- Useful for raw server responses and fast edits.",
                [Language.German] = @"- show object/users/accounts schnell ausführen.
- Doppelklick auf Zeilen zum Editieren.
- Für Rohantworten und schnelle Anpassungen."
            },
            ["Help_Content_Tab_EventManager"] = new()
            {
                [Language.English] = @"- Embedded Event Manager UI (Windows Forms).
- Connect to maintenance socket (default 127.0.0.1:9998).
- Schedule, start, cancel events, view timers/logs, and handle recurring/custom classes.",
                [Language.German] = @"- Eingebettete Event-Manager UI (Windows Forms).
- Verbindung über Maintenance-Socket (Standard 127.0.0.1:9998).
- Events planen, starten, abbrechen; Timer/Log und wiederkehrende/Custom-Klassen im Blick."
            },
            // Tabs
            ["Tab_Connection"] = new() { [Language.English] = "🔌 Connection", [Language.German] = "🔌 Verbindung" },
            ["Tab_Warps"] = new() { [Language.English] = "Warps", [Language.German] = "Warps" },
            ["Tab_Monsters"] = new() { [Language.English] = "Monsters", [Language.German] = "Monster" },
            ["Tab_Items"] = new() { [Language.English] = "Items", [Language.German] = "Items" },
            ["Tab_QuestEditor"] = new() { [Language.English] = "QuestEditor", [Language.German] = "QuestEditor" },
            ["Tab_Dm"] = new() { [Language.English] = "DM", [Language.German] = "DM" },
            ["Tab_Admin"] = new() { [Language.English] = "Admin", [Language.German] = "Admin" },
            ["Tab_ObjectInspector"] = new() { [Language.English] = "Object Inspector", [Language.German] = "Objekt-Inspektor" },
            ["Tab_EventManager"] = new() { [Language.English] = "Event Manager", [Language.German] = "Event-Manager" },
            ["Tab_Dj"] = new() { [Language.English] = "DJ", [Language.German] = "DJ" },
            ["Tab_Arena"] = new() { [Language.English] = "Arena", [Language.German] = "Arena" },
            ["Tab_AdminConsole"] = new() { [Language.English] = "Admin Console", [Language.German] = "Admin-Konsole" },
            ["Tab_Players"] = new() { [Language.English] = "Players", [Language.German] = "Spieler" },
            ["Tab_ListReader"] = new() { [Language.English] = "List Reader", [Language.German] = "Listenleser" },
            ["Tab_DeepInspector"] = new() { [Language.English] = "🔬 Deep Inspector", [Language.German] = "🔬 Deep Inspector" },

            // Sections (explicit where needed)
            ["Section_ConnectionSettings"] = new() { [Language.English] = "Connection Settings", [Language.German] = "Verbindungseinstellungen" },
            ["Section_DebugLog"] = new() { [Language.English] = "Debug Log", [Language.German] = "Debug-Protokoll" },
            ["Section_TestConnection"] = new() { [Language.English] = "Test Connection", [Language.German] = "Verbindung testen" },
            ["Section_DmPlayers"] = new() { [Language.English] = "🎮 Players", [Language.German] = "🎮 Spieler" },
            ["Section_DmVisibility"] = new() { [Language.English] = "👻 Visibility", [Language.German] = "👻 Sichtbarkeit" },
            ["Section_DmDisguise"] = new() { [Language.English] = "🎭 Disguise", [Language.German] = "🎭 Verkleidung" },
            ["Section_DmBoostKarma"] = new() { [Language.English] = "💚 Boost & Karma", [Language.German] = "💚 Boost & Karma" },
            ["Section_DmGodMode"] = new() { [Language.English] = "⚔️ God Mode", [Language.German] = "⚔️ Gottmodus" },
            ["Section_DmPkStatus"] = new() { [Language.English] = "🛡️ PK Status", [Language.German] = "🛡️ PK-Status" },
            ["Section_DmGetItems"] = new() { [Language.English] = "🎁 Get Items", [Language.German] = "🎁 Items holen" },
            ["Section_DmTime"] = new() { [Language.English] = "🕐 Time", [Language.German] = "🕐 Zeit" },
            ["Section_DmLights"] = new() { [Language.English] = "💡 Lights", [Language.German] = "💡 Lichter" },
            ["Section_DmMapTour"] = new() { [Language.English] = "🗺️ Map & Tour", [Language.German] = "🗺️ Karte & Tour" },
            ["Section_DmCommunication"] = new() { [Language.English] = "💬 Communication", [Language.German] = "💬 Kommunikation" },
            ["Section_DmTestCommands"] = new() { [Language.English] = "🔧 Test Commands", [Language.German] = "🔧 Testbefehle" },
            ["Section_DmLogoffGhost"] = new() { [Language.English] = "👻 Logoff Ghost", [Language.German] = "👻 Logoff-Geist" },
            ["Section_DmMisc"] = new() { [Language.English] = "🎲 Misc", [Language.German] = "🎲 Sonstiges" },
            ["Section_DmCustomCommand"] = new() { [Language.English] = "⌨️ Custom Command", [Language.German] = "⌨️ Eigener Befehl" },
            ["Section_AdminObjects"] = new() { [Language.English] = "📦 Objects", [Language.German] = "📦 Objekte" },
            ["Section_AdminPlayerManagement"] = new() { [Language.English] = "👤 Player Management", [Language.German] = "👤 Spielerverwaltung" },
            ["Section_AdminAccounts"] = new() { [Language.English] = "👥 Accounts", [Language.German] = "👥 Accounts" },
            ["Section_AdminSystem"] = new() { [Language.English] = "💾 System", [Language.German] = "💾 System" },
            ["Section_AdminGlobalActions"] = new() { [Language.English] = "🌍 Global Actions", [Language.German] = "🌍 Globale Aktionen" },
            ["Section_AdminResources"] = new() { [Language.English] = "🔨 Resources", [Language.German] = "🔨 Ressourcen" },
            ["Section_AdminAdvanced"] = new() { [Language.English] = "⚡ Advanced", [Language.German] = "⚡ Erweitert" },
            ["Section_AdminResponses"] = new() { [Language.English] = "📥 Server Responses", [Language.German] = "📥 Server-Antworten" },
            ["Section_AdminCustomCommand"] = new() { [Language.English] = "⌨️ Custom Command", [Language.German] = "⌨️ Eigener Befehl" },
            ["Section_AdminSpellsSkills"] = new() { [Language.English] = "📚 Spells & Skills", [Language.German] = "📚 Zauber & Skills" },
            ["Section_AdminServerSettings"] = new() { [Language.English] = "⚙️ Server Settings", [Language.German] = "⚙️ Servereinstellungen" },
            ["Section_QuickCommands"] = new() { [Language.English] = "Quick Commands", [Language.German] = "Schnellbefehle" },
            ["Section_QuickChecks"] = new() { [Language.English] = "Quick Checks", [Language.German] = "Schnellprüfungen" },
            ["Section_ShowObject"] = new() { [Language.English] = "Show Object", [Language.German] = "Objekt anzeigen" },

            // Labels
            ["Label_ServerIp"] = new() { [Language.English] = "Server IP:", [Language.German] = "Server-IP:" },
            ["Label_ServerPort"] = new() { [Language.English] = "Server Port:", [Language.German] = "Server-Port:" },
            ["Label_Username"] = new() { [Language.English] = "Username:", [Language.German] = "Benutzername:" },
            ["Label_Password"] = new() { [Language.English] = "Password:", [Language.German] = "Passwort:" },
            ["Label_SecretKey"] = new() { [Language.English] = "Secret Key:", [Language.German] = "Geheimer Schlüssel:" },
            ["Label_Status"] = new() { [Language.English] = "Status", [Language.German] = "Status" },
            ["Label_LastResponse"] = new() { [Language.English] = "Last Response", [Language.German] = "Letzte Antwort" },
            ["Label_LastResponseInline"] = new() { [Language.English] = "Last Response:", [Language.German] = "Letzte Antwort:" },
            ["Label_WarpName"] = new() { [Language.English] = "Name:", [Language.German] = "Name:" },
            ["Label_RoomId"] = new() { [Language.English] = "Room ID:", [Language.German] = "Raum-ID:" },
            ["Label_Description"] = new() { [Language.English] = "Description:", [Language.German] = "Beschreibung:" },
            ["Label_Category"] = new() { [Language.English] = "Category:", [Language.German] = "Kategorie:" },
            ["Label_Name"] = new() { [Language.English] = "Name:", [Language.German] = "Name:" },
            ["Label_XCoordinate"] = new() { [Language.English] = "X Coordinate:", [Language.German] = "X Koordinate:" },
            ["Label_YCoordinate"] = new() { [Language.English] = "Y Coordinate:", [Language.German] = "Y Koordinate:" },
            ["Label_ClassName"] = new() { [Language.English] = "Class Name:", [Language.German] = "Klassenname:" },
            ["Label_EnglishName"] = new() { [Language.English] = "English Name:", [Language.German] = "Englischer Name:" },
            ["Label_GermanName"] = new() { [Language.English] = "German Name:", [Language.German] = "Deutscher Name:" },
            ["Label_DmCommand"] = new() { [Language.English] = "DM Command:", [Language.German] = "DM-Befehl:" },
            ["Label_ItemQuantity"] = new() { [Language.English] = "Quantity:", [Language.German] = "Menge:" },
            ["Label_KodFile"] = new() { [Language.English] = "KOD File:", [Language.German] = "KOD-Datei:" },
            ["Label_Count"] = new() { [Language.English] = "Count:", [Language.German] = "Anzahl:" },
            ["Label_ObjectId"] = new() { [Language.English] = "Object ID:", [Language.German] = "Objekt-ID:" },
            ["Label_ServerResponse"] = new() { [Language.English] = "Server Response", [Language.German] = "Server-Antwort" },
            ["Label_CommandType"] = new() { [Language.English] = "Command Type:", [Language.German] = "Befehlstyp:" },
            ["Label_ImportantLists"] = new() { [Language.English] = "Important Lists:", [Language.German] = "Wichtige Listen:" },
            ["Label_EventType"] = new() { [Language.English] = "Event Type", [Language.German] = "Event-Typ" },
            ["Label_CustomBlakodClass"] = new() { [Language.English] = "Custom Blakod Class", [Language.German] = "Eigene Blakod-Klasse" },
            ["Label_ScheduleMinutes"] = new() { [Language.English] = "Schedule in minutes", [Language.German] = "Planen in Minuten" },
            ["Label_EventLog"] = new() { [Language.English] = "Event Log", [Language.German] = "Event-Log" },
            ["Label_ActiveInstances"] = new() { [Language.English] = "Active Instances", [Language.German] = "Aktive Instanzen" },
            ["Label_Spell"] = new() { [Language.English] = "Spell:", [Language.German] = "Zauber:" },
            ["Label_MessageCount"] = new() { [Language.English] = "(0 Messages)", [Language.German] = "(0 Nachrichten)" },
            ["Label_DebugStatus"] = new() { [Language.English] = "Ready | Auto-Scroll: ON", [Language.German] = "Bereit | Auto-Scroll: AN" },
            ["Label_MessageCountFormat"] = new() { [Language.English] = "({0} Messages)", [Language.German] = "({0} Nachrichten)" },
            ["Label_NavigationHistory"] = new() { [Language.English] = "Navigation History", [Language.German] = "Navigationsverlauf" },
            ["Label_Equals"] = new() { [Language.English] = " = ", [Language.German] = " = " },
            ["Label_On"] = new() { [Language.English] = "ON", [Language.German] = "AN" },
            ["Label_Off"] = new() { [Language.English] = "OFF", [Language.German] = "AUS" },
            ["Checkbox_AutoScroll"] = new() { [Language.English] = "Auto-Scroll", [Language.German] = "Auto-Scroll" },

            // Menu items
            ["Menu_OpenInDeepInspector"] = new() { [Language.English] = "Open in Deep Inspector", [Language.German] = "Im Deep Inspector öffnen" },
            ["Menu_CopySelected"] = new() { [Language.English] = "Copy Selected", [Language.German] = "Auswahl kopieren" },
            ["Menu_CopyAll"] = new() { [Language.English] = "Copy All", [Language.German] = "Alles kopieren" },
            ["Menu_OpenList"] = new() { [Language.English] = "Open List", [Language.German] = "Liste öffnen" },
            ["Menu_EditInt"] = new() { [Language.English] = "Edit INT", [Language.German] = "INT bearbeiten" },
            ["Menu_File"] = new() { [Language.English] = "File", [Language.German] = "Datei" },
            ["Menu_Help"] = new() { [Language.English] = "Help", [Language.German] = "Hilfe" },
            ["Menu_OpenHelp"] = new() { [Language.English] = "Open Help", [Language.German] = "Hilfe öffnen" },

            // Buttons with explicit punctuation/emoji
            ["Button_LanguageToggle_English"] = new() { [Language.English] = "🌐 English", [Language.German] = "🌐 English" },
            ["Button_LanguageToggle_German"] = new() { [Language.English] = "🌐 Deutsch", [Language.German] = "🌐 Deutsch" },
            ["Button_Save"] = new() { [Language.English] = "💾 Save", [Language.German] = "💾 Speichern" },
            ["Button_Reload"] = new() { [Language.English] = "🔄 Reload", [Language.German] = "🔄 Neu laden" },
            ["Button_Export"] = new() { [Language.English] = "📤 Export", [Language.German] = "📤 Exportieren" },
            ["Button_Import"] = new() { [Language.English] = "📥 Import", [Language.German] = "📥 Importieren" },
            ["Button_SaveChanges"] = new() { [Language.English] = "💾 Save Changes", [Language.German] = "💾 Änderungen speichern" },
            ["Button_SpawnMonster"] = new() { [Language.English] = "➜ Spawn Monster", [Language.German] = "➜ Monster spawnen" },
            ["Button_RefreshMonsters"] = new() { [Language.English] = "Refresh Monsters (from KOD)", [Language.German] = "Monster aktualisieren (aus KOD)" },
            ["Button_RefreshItems"] = new() { [Language.English] = "Refresh Items (from KOD)", [Language.German] = "Items aktualisieren (aus KOD)" },
            ["Button_SpawnItem"] = new() { [Language.English] = "➜ Spawn Item", [Language.German] = "➜ Item spawnen" },
            ["Button_OpenDebugLog"] = new() { [Language.English] = "🐛 Open Debug Log", [Language.German] = "🐛 Debug-Protokoll öffnen" },
            ["Button_CopyLog"] = new() { [Language.English] = "Copy Log", [Language.German] = "Log kopieren" },
            ["Button_CopyDetails"] = new() { [Language.English] = "Copy Details", [Language.German] = "Details kopieren" },
            ["Button_CopyAll"] = new() { [Language.English] = "📋 Copy All", [Language.German] = "📋 Alles kopieren" },
            ["Button_Clear"] = new() { [Language.English] = "Clear", [Language.German] = "Löschen" },
            ["Button_Close"] = new() { [Language.English] = "Close", [Language.German] = "Schließen" },
            ["Button_Copy"] = new() { [Language.English] = "Copy", [Language.German] = "Kopieren" },
            ["Button_Ok"] = new() { [Language.English] = "OK", [Language.German] = "OK" },
            ["Button_Cancel"] = new() { [Language.English] = "Cancel", [Language.German] = "Abbrechen" },
            ["Button_ExecuteCommand"] = new() { [Language.English] = "Execute Command", [Language.German] = "Befehl ausführen" },
            ["Button_SendCommand"] = new() { [Language.English] = "Send Command", [Language.German] = "Befehl senden" },
            ["Button_Load"] = new() { [Language.English] = "Load", [Language.German] = "Laden" },
            ["Button_Back"] = new() { [Language.English] = "← Back", [Language.German] = "← Zurück" },
            ["Button_Edit"] = new() { [Language.English] = "✏️ Edit", [Language.German] = "✏️ Bearbeiten" },
            ["Button_ListNavigate"] = new() { [Language.English] = "List →", [Language.German] = "Liste →" },
            ["Button_Navigate"] = new() { [Language.English] = "Navigate →", [Language.German] = "Navigieren →" },

            // Placeholders
            ["Placeholder_SearchWarp"] = new() { [Language.English] = "🔍 Search warps...", [Language.German] = "🔍 Warps suchen..." },
            ["Button_RefreshRooms"] = new() { [Language.English] = "Refresh Rooms (from KOD)", [Language.German] = "Räume aktualisieren (aus KOD)" },
            ["Placeholder_SearchMonster"] = new() { [Language.English] = "🔍 Search monsters...", [Language.German] = "🔍 Monster suchen..." },
            ["Placeholder_SearchItem"] = new() { [Language.English] = "🔍 Search items...", [Language.German] = "🔍 Items suchen..." },
            ["Placeholder_FilterResults"] = new() { [Language.English] = "Filter results...", [Language.German] = "Ergebnisse filtern..." },
            ["Placeholder_DjComingSoon"] = new() { [Language.English] = "DJ Music Tab - Coming Soon", [Language.German] = "DJ-Musik-Tab - Demnächst" },
            ["Placeholder_ArenaComingSoon"] = new() { [Language.English] = "Arena Tab - Coming Soon", [Language.German] = "Arena-Tab - Demnächst" },

            // Defaults
            ["Default_WarpName"] = new() { [Language.English] = "New Warp", [Language.German] = "Neuer Warp" },
            ["Default_WarpDescription"] = new() { [Language.English] = "New warp location", [Language.German] = "Neue Warp-Location" },
            ["Default_CategoryName"] = new() { [Language.English] = "New Category {0}", [Language.German] = "Neue Kategorie {0}" },
            ["Default_ItemClassName"] = new() { [Language.English] = "NewItem", [Language.German] = "NewItem" },
            ["Default_ItemEnglishName"] = new() { [Language.English] = "New Item", [Language.German] = "New Item" },
            ["Default_ItemDmCommand"] = new() { [Language.English] = "dm item New Item", [Language.German] = "dm item New Item" },
            ["Default_MonsterClassName"] = new() { [Language.English] = "NewMonster", [Language.German] = "NewMonster" },
            ["Default_MonsterEnglishName"] = new() { [Language.English] = "New Monster", [Language.German] = "New Monster" },
            ["Default_MonsterDmCommand"] = new() { [Language.English] = "dm monster NewMonster", [Language.German] = "dm monster NewMonster" },
            ["Default_Unknown"] = new() { [Language.English] = "Unknown", [Language.German] = "Unbekannt" },
            ["Default_ExtractedDescription"] = new() { [Language.English] = "Extracted from server kod files", [Language.German] = "Aus Server-KOD-Dateien extrahiert" },
            ["Default_SpecialRoomCategory"] = new() { [Language.English] = "Special Room", [Language.German] = "Spezialraum" },
            ["Default_SchoolRoomName"] = new() { [Language.English] = "School Room", [Language.German] = "Der Schulraum" },
            ["Default_SchoolRoomDescription"] = new() { [Language.English] = "Training room for new players", [Language.German] = "Schulungsraum für neue Spieler" },
            ["Default_GodsMeetingName"] = new() { [Language.English] = "Meeting of the Gods", [Language.German] = "Der Treffpunkt der Götter" },
            ["Default_GodsMeetingDescription"] = new() { [Language.English] = "Meeting place of the gods", [Language.German] = "Versammlungsort der Götter" },

            // Titles
            ["Title_CategoryRequired"] = new() { [Language.English] = "Category Required", [Language.German] = "Kategorie erforderlich" },
            ["Title_DeleteCategory"] = new() { [Language.English] = "Delete Category", [Language.German] = "Kategorie löschen" },
            ["Title_DeleteWarp"] = new() { [Language.English] = "Delete Warp", [Language.German] = "Warp löschen" },
            ["Title_DeleteItem"] = new() { [Language.English] = "Delete Item", [Language.German] = "Item löschen" },
            ["Title_DeleteMonster"] = new() { [Language.English] = "Delete Monster", [Language.German] = "Monster löschen" },
            ["Title_NotConnected"] = new() { [Language.English] = "Not Connected", [Language.German] = "Nicht verbunden" },
            ["Title_NoSelection"] = new() { [Language.English] = "No Selection", [Language.German] = "Keine Auswahl" },
            ["Title_NoItemSelected"] = new() { [Language.English] = "No Item Selected", [Language.German] = "Kein Item gewählt" },
            ["Title_NoMonsterSelected"] = new() { [Language.English] = "No Monster Selected", [Language.German] = "Kein Monster gewählt" },
            ["Title_Success"] = new() { [Language.English] = "Success", [Language.German] = "Erfolg" },
            ["Title_Saved"] = new() { [Language.English] = "Saved", [Language.German] = "Gespeichert" },
            ["Title_ReloadWarps"] = new() { [Language.English] = "Reload Warps", [Language.German] = "Warps neu laden" },
            ["Title_RefreshRooms"] = new() { [Language.English] = "Refresh Rooms", [Language.German] = "Räume aktualisieren" },
            ["Title_ReloadItems"] = new() { [Language.English] = "Reload Items", [Language.German] = "Items neu laden" },
            ["Title_ReloadMonsters"] = new() { [Language.English] = "Reload Monsters", [Language.German] = "Monster neu laden" },
            ["Title_RefreshMonsters"] = new() { [Language.English] = "Refresh Monsters", [Language.German] = "Monster aktualisieren" },
            ["Title_RefreshItems"] = new() { [Language.English] = "Refresh Items", [Language.German] = "Items aktualisieren" },
            ["Title_ClientNotFound"] = new() { [Language.English] = "Client Not Found", [Language.German] = "Client nicht gefunden" },
            ["Title_CommandCopied"] = new() { [Language.English] = "Command Copied", [Language.German] = "In Zwischenablage kopiert" },
            ["Title_ExportSuccess"] = new() { [Language.English] = "Export Successful", [Language.German] = "Export erfolgreich" },
            ["Title_ImportWarps"] = new() { [Language.English] = "Import Warps", [Language.German] = "Warps importieren" },
            ["Title_ImportSuccess"] = new() { [Language.English] = "Import Successful", [Language.German] = "Import erfolgreich" },
            ["Title_LoadError"] = new() { [Language.English] = "Load Error", [Language.German] = "Ladefehler" },
            ["Title_SaveError"] = new() { [Language.English] = "Save Error", [Language.German] = "Speicherfehler" },
            ["Title_ExportError"] = new() { [Language.English] = "Export Error", [Language.German] = "Export-Fehler" },
            ["Title_ImportError"] = new() { [Language.English] = "Import Error", [Language.German] = "Import-Fehler" },
            ["Title_SendFailed"] = new() { [Language.English] = "Send Failed", [Language.German] = "Senden fehlgeschlagen" },
            ["Title_Error"] = new() { [Language.English] = "Error", [Language.German] = "Fehler" },
            ["Title_Copied"] = new() { [Language.English] = "Copied", [Language.German] = "Kopiert" },
            ["Title_CopyFailed"] = new() { [Language.English] = "Copy Failed", [Language.German] = "Kopieren fehlgeschlagen" },
            ["Title_SendObjectCommand"] = new() { [Language.English] = "Send Object Command", [Language.German] = "Objektbefehl senden" },
            ["Title_SendClassCommand"] = new() { [Language.English] = "Send Class Command", [Language.German] = "Klassenbefehl senden" },
            ["Title_SetObjectProperty"] = new() { [Language.English] = "Set Object Property", [Language.German] = "Objekt-Eigenschaft setzen" },
            ["Title_SetServerHour"] = new() { [Language.English] = "Set Server Hour", [Language.German] = "Serverstunde setzen" },
            ["Title_ShowObject"] = new() { [Language.English] = "Show Object", [Language.German] = "Objekt anzeigen" },
            ["Title_ShowInstance"] = new() { [Language.English] = "Show Instance", [Language.German] = "Instanz anzeigen" },
            ["Title_ShowMessage"] = new() { [Language.English] = "Show Message", [Language.German] = "Nachricht anzeigen" },
            ["Title_CreateObject"] = new() { [Language.English] = "Create Object", [Language.German] = "Objekt erstellen" },
            ["Title_CreateResource"] = new() { [Language.English] = "Create Resource", [Language.German] = "Ressource erstellen" },
            ["Title_CreateAdminAccount"] = new() { [Language.English] = "Create Admin Account", [Language.German] = "Admin-Account erstellen" },
            ["Title_CreateAdminAccountFinalize"] = new() { [Language.English] = "Create Admin Account (Finalize)", [Language.German] = "Admin-Account erstellen (Finalisieren)" },
            ["Title_CreateDmAccount"] = new() { [Language.English] = "Create DM Account", [Language.German] = "DM-Account erstellen" },
            ["Title_CreateUserAccount"] = new() { [Language.English] = "Create User Account", [Language.German] = "User-Account erstellen" },
            ["Title_CreateUserAccountFinalize"] = new() { [Language.English] = "Create User Account (Finalize)", [Language.German] = "User-Account erstellen (Finalisieren)" },
            ["Title_DeleteObject"] = new() { [Language.English] = "Delete Object", [Language.German] = "Objekt löschen" },
            ["Title_TeleportObject"] = new() { [Language.English] = "Teleport Object", [Language.German] = "Objekt teleportieren" },
            ["Title_TeleportToSafety"] = new() { [Language.English] = "Teleport to Safety", [Language.German] = "In Sicherheit teleportieren" },
            ["Title_GiveSpell"] = new() { [Language.English] = "Give Spell", [Language.German] = "Zauber geben" },
            ["Title_GiveSkill"] = new() { [Language.English] = "Give Skill", [Language.German] = "Skill geben" },
            ["Title_GlobalGiveItem"] = new() { [Language.English] = "Global Give Item", [Language.German] = "Global Item geben" },
            ["Title_Warning"] = new() { [Language.English] = "Warning", [Language.German] = "Warnung" },

            // Prompts
            ["Prompt_Username"] = new() { [Language.English] = "Enter username:", [Language.German] = "Username eingeben:" },
            ["Prompt_Password"] = new() { [Language.English] = "Enter password:", [Language.German] = "Passwort eingeben:" },
            ["Prompt_Email"] = new() { [Language.English] = "Enter email:", [Language.German] = "E-Mail eingeben:" },
            ["Prompt_AccountNumber"] = new() { [Language.English] = "Enter account number (from create account):", [Language.German] = "Accountnummer eingeben (Rückgabe aus create account):" },
            ["Prompt_ClassName"] = new() { [Language.English] = "Enter class name:", [Language.German] = "Class Name eingeben:" },
            ["Prompt_ObjectId"] = new() { [Language.English] = "Enter object ID:", [Language.German] = "Object ID eingeben:" },
            ["Prompt_RoomId"] = new() { [Language.English] = "Enter room ID:", [Language.German] = "Room ID eingeben:" },
            ["Prompt_PlayerObjectId"] = new() { [Language.English] = "Enter player object ID:", [Language.German] = "Player Object ID eingeben:" },
            ["Prompt_SpellId"] = new() { [Language.English] = "Enter spell ID:", [Language.German] = "Spell ID eingeben:" },
            ["Prompt_SkillId"] = new() { [Language.English] = "Enter skill ID:", [Language.German] = "Skill ID eingeben:" },
            ["Prompt_AbilityPercent"] = new() { [Language.English] = "Enter ability % (0-99):", [Language.German] = "Ability % eingeben (0-99):" },
            ["Prompt_ServerHour"] = new() { [Language.English] = "Enter hour (1-21):", [Language.German] = "Stunde eingeben (1-21):" },
            ["Prompt_ItemClassName"] = new() { [Language.English] = "Enter item class name:", [Language.German] = "Item Class Name eingeben:" },
            ["Prompt_Count"] = new() { [Language.English] = "Enter amount:", [Language.German] = "Anzahl eingeben:" },
            ["Prompt_ResourceName"] = new() { [Language.English] = "Enter resource name/text:", [Language.German] = "Resource Name/Text eingeben:" },
            ["Prompt_MessageName"] = new() { [Language.English] = "Enter message name:", [Language.German] = "Message Name eingeben:" },
            ["Prompt_MessageCommand"] = new() { [Language.English] = "Enter message/command:", [Language.German] = "Message/Command eingeben:" },
            ["Prompt_PropertyName"] = new() { [Language.English] = "Enter property name:", [Language.German] = "Property Name eingeben:" },
            ["Prompt_PropertyValue"] = new() { [Language.English] = "Enter value (e.g. INT 100, $ 0):", [Language.German] = "Value eingeben (z.B. INT 100, $ 0):" },
            ["Prompt_EditDetailMessage"] = new() { [Language.English] = "Current: {0}\n\nNew value for {1} (leave type blank to reuse current):", [Language.German] = "Aktuell: {0}\n\nNeuer Wert für {1} (Typ leer lassen um aktuellen zu übernehmen):" },

            // List Reader (command types)
            ["ListReader_CommandType_Class"] = new() { [Language.English] = "Show Class", [Language.German] = "Klasse anzeigen" },
            ["ListReader_CommandType_Instances"] = new() { [Language.English] = "Show Instances", [Language.German] = "Instanzen anzeigen" },
            ["ListReader_CommandType_All"] = new() { [Language.English] = "Show All", [Language.German] = "Alles anzeigen" },
            ["ListReader_CommandType_List"] = new() { [Language.English] = "Show List", [Language.German] = "Liste anzeigen" },

            // List Reader (messages)
            ["ListReader_Message_ListIdMissing"] = new() { [Language.English] = "❌ This list entry has no list ID configured.", [Language.German] = "❌ Für diesen Eintrag ist keine Listen-ID hinterlegt." },
            ["ListReader_Message_ClassNameRequired"] = new() { [Language.English] = "❌ Please enter a class name or list ID.", [Language.German] = "❌ Bitte einen Class Name oder eine Listen-ID eingeben." },
            ["ListReader_Message_CommandTypeRequired"] = new() { [Language.English] = "❌ Please select a command type.", [Language.German] = "❌ Bitte einen Command Type auswählen." },
            ["ListReader_Message_Executing"] = new() { [Language.English] = "Executing {0} for {1}...", [Language.German] = "Führe {0} für {1} aus..." },
            ["ListReader_Message_NoServerResponse"] = new() { [Language.English] = "❌ No response received from server.", [Language.German] = "❌ Keine Antwort vom Server erhalten." },
            ["ListReader_Message_UnknownCommandType"] = new() { [Language.English] = "❌ Unknown command type: {0}", [Language.German] = "❌ Unbekannter Command Type: {0}" },
            ["ListReader_Message_ResultsFound"] = new() { [Language.English] = "✅ {0} result(s) found.", [Language.German] = "✅ {0} Ergebnis(se) gefunden." },
            ["ListReader_Message_ResultsCleared"] = new() { [Language.English] = "Results cleared.", [Language.German] = "Ergebnisse gelöscht." },
            ["ListReader_Message_NoResultsToExport"] = new() { [Language.English] = "❌ No results to export.", [Language.German] = "❌ Keine Ergebnisse zum Exportieren vorhanden." },
            ["ListReader_Message_ExportSuccess"] = new() { [Language.English] = "✅ Export successful: {0}", [Language.German] = "✅ Export erfolgreich: {0}" },
            ["ListReader_Message_ExportCanceled"] = new() { [Language.English] = "Export canceled.", [Language.German] = "Export abgebrochen." },
            ["ListReader_Message_ExportFailed"] = new() { [Language.English] = "❌ Export failed: {0}", [Language.German] = "❌ Export fehlgeschlagen: {0}" },
            ["ListReader_Message_SelectResultFirst"] = new() { [Language.English] = "❌ Please select a result first.", [Language.German] = "❌ Bitte zuerst ein Objekt aus der Liste auswählen." },
            ["ListReader_Message_LoadingDetails"] = new() { [Language.English] = "Loading details for object {0}...", [Language.German] = "Lade Details für Object {0}..." },
            ["ListReader_Message_DetailsLoaded"] = new() { [Language.English] = "✅ Details for object {0} loaded.", [Language.German] = "✅ Details für Object {0} geladen." },
            ["ListReader_Message_NoDetailsReceived"] = new() { [Language.English] = "❌ No details received from server.", [Language.German] = "❌ Keine Details vom Server erhalten." },
            ["ListReader_Message_PropertiesShown"] = new() { [Language.English] = "✅ Properties shown ({0} entries).", [Language.German] = "✅ Properties angezeigt ({0} Einträge)." },
            ["ListReader_Message_RawResponseShown"] = new() { [Language.English] = "Raw response shown.", [Language.German] = "Raw response angezeigt." },
            ["ListReader_Message_NoDetailsAvailable"] = new() { [Language.English] = "❌ No details available.", [Language.German] = "❌ Keine Details verfügbar." },
            ["ListReader_Message_NotConnected"] = new() { [Language.English] = "❌ Not connected. Please connect in the \"Connection\" tab.", [Language.German] = "❌ Nicht verbunden. Bitte zuerst im Tab \"Connection\" verbinden." },
            ["ListReader_Message_SendFailed"] = new() { [Language.English] = "Send failed: {0}", [Language.German] = "Fehler beim Senden: {0}" },
            ["ListReader_Message_ResultsDisplayed"] = new() { [Language.English] = "{0} result(s) displayed.", [Language.German] = "{0} Ergebnis(se) angezeigt." },
            ["ListReader_Message_ListIdNotFound"] = new() { [Language.English] = "❌ No list ID found in this line.", [Language.German] = "❌ Keine Listen-ID in dieser Zeile gefunden." },
            ["ListReader_Message_EditIntNotInt"] = new() { [Language.English] = "❌ This line is not an INT property.", [Language.German] = "❌ Diese Zeile ist keine INT-Property." },
            ["ListReader_Message_EditIntNoObjectId"] = new() { [Language.English] = "❌ No object ID available for editing.", [Language.German] = "❌ Keine Object-ID zum Bearbeiten verfügbar." },
            ["ListReader_Message_InvalidIntValue"] = new() { [Language.English] = "❌ Invalid INT value.", [Language.German] = "❌ Ungültiger INT-Wert." },
            ["ListReader_Message_EditIntUpdated"] = new() { [Language.English] = "✅ Property {0} updated.", [Language.German] = "✅ Property {0} aktualisiert." },
            ["ListReader_Prompt_EditInt_Title"] = new() { [Language.English] = "Edit INT Property", [Language.German] = "INT-Property bearbeiten" },
            ["ListReader_Prompt_EditInt_Message"] = new() { [Language.English] = "Property: {0}\nCurrent: {1}\n\nNew INT value:", [Language.German] = "Property: {0}\nAktuell: {1}\n\nNeuer INT-Wert:" },

            // Messages
            ["Message_CategoryRequired"] = new() { [Language.English] = "Please select a category first!", [Language.German] = "Bitte wähle zuerst eine Kategorie aus!" },
            ["Message_CategoryDeleteConfirm"] = new() { [Language.English] = "Category '{0}' contains {1} warps!\n\nDelete it?", [Language.German] = "Kategorie '{0}' enthält {1} Warps!\n\nWirklich löschen? (Alle Warps gehen verloren)" },
            ["Message_DeleteWarp"] = new() { [Language.English] = "Delete warp '{0}'?", [Language.German] = "Warp '{0}' wirklich löschen?" },
            ["Message_WarpSaved"] = new() { [Language.English] = "Warp saved!", [Language.German] = "Warp gespeichert!" },
            ["Message_WarpsSaved"] = new() { [Language.English] = "All warps saved successfully!", [Language.German] = "Alle Warps erfolgreich gespeichert!" },
            ["Message_ReloadWarpsConfirm"] = new() { [Language.English] = "All unsaved changes will be lost!\n\nReload now?", [Language.German] = "Alle ungespeicherten Änderungen gehen verloren!\n\nWirklich neu laden?" },
            ["Message_WarpsReloaded"] = new() { [Language.English] = "Warps reloaded!", [Language.German] = "Warps neu geladen!" },
            ["Message_RefreshRoomsOk"] = new() { [Language.English] = "Rooms updated from source files.", [Language.German] = "Räume aus den Quelldateien aktualisiert." },
            ["Message_RefreshRoomsFailed"] = new() { [Language.English] = "Failed to refresh rooms:\n{0}", [Language.German] = "Räume konnten nicht aktualisiert werden:\n{0}" },
            ["Message_WarpsExported"] = new() { [Language.English] = "Warps exported to:\n{0}", [Language.German] = "Warps exportiert nach:\n{0}" },
            ["Message_WarpsImportConfirm"] = new() { [Language.English] = "Warning: Import replaces ALL current warps!\n\nContinue?", [Language.German] = "Warnung: Importieren ersetzt ALLE aktuellen Warps!\n\nFortfahren?" },
            ["Message_WarpsImported"] = new() { [Language.English] = "{0} warps imported!", [Language.German] = "{0} Warps importiert!" },
            ["Message_ClientNotRunning"] = new() { [Language.English] = "Meridian 59 client is not running!\n\nCopy command to clipboard anyway?", [Language.German] = "Meridian 59 Client ist nicht aktiv!\n\nSoll der Befehl trotzdem in die Zwischenablage kopiert werden?" },
            ["Message_CommandCopied"] = new() { [Language.English] = "Command copied to clipboard:\n\n{0}\n\nStart the client and paste with CTRL+V.", [Language.German] = "Befehl in Zwischenablage kopiert:\n\n{0}\n\nStarte den Client und füge mit CTRL+V ein." },
            ["Message_SelectItemFirst"] = new() { [Language.English] = "Please select an item first.", [Language.German] = "Bitte wähle zuerst ein Item aus." },
            ["Message_DeleteItem"] = new() { [Language.English] = "Delete item '{0}'?", [Language.German] = "Item '{0}' wirklich löschen?" },
            ["Message_ItemSaved"] = new() { [Language.English] = "Item changes saved!", [Language.German] = "Item-Änderungen gespeichert!" },
            ["Message_ItemsSaved"] = new() { [Language.English] = "All items saved successfully!", [Language.German] = "Alle Items erfolgreich gespeichert!" },
            ["Message_ReloadItemsConfirm"] = new() { [Language.English] = "All unsaved changes will be lost!\n\nReload now?", [Language.German] = "Alle ungespeicherten Änderungen gehen verloren!\n\nWirklich neu laden?" },
            ["Message_ItemsReloaded"] = new() { [Language.English] = "Items reloaded!", [Language.German] = "Items neu geladen!" },
            ["Message_RefreshItemsOk"] = new() { [Language.English] = "Items updated from source files.", [Language.German] = "Items aus den Quelldateien aktualisiert." },
            ["Message_RefreshItemsFailed"] = new() { [Language.English] = "Failed to refresh items:\n{0}", [Language.German] = "Items konnten nicht aktualisiert werden:\n{0}" },
            ["Message_SelectMonsterFirst"] = new() { [Language.English] = "Please select a monster first.", [Language.German] = "Bitte wähle zuerst ein Monster aus." },
            ["Message_DeleteMonster"] = new() { [Language.English] = "Delete monster '{0}'?", [Language.German] = "Monster '{0}' wirklich löschen?" },
            ["Message_MonsterSaved"] = new() { [Language.English] = "Monster changes saved!", [Language.German] = "Monster-Änderungen gespeichert!" },
            ["Message_MonstersSaved"] = new() { [Language.English] = "All monsters saved successfully!", [Language.German] = "Alle Monster erfolgreich gespeichert!" },
            ["Message_ReloadMonstersConfirm"] = new() { [Language.English] = "All unsaved changes will be lost!\n\nReload now?", [Language.German] = "Alle ungespeicherten Änderungen gehen verloren!\n\nWirklich neu laden?" },
            ["Message_MonstersReloaded"] = new() { [Language.English] = "Monsters reloaded!", [Language.German] = "Monster neu geladen!" },
            ["Message_RefreshMonstersOk"] = new() { [Language.English] = "Monsters updated from source files.", [Language.German] = "Monster aus den Quelldateien aktualisiert." },
            ["Message_RefreshMonstersFailed"] = new() { [Language.English] = "Failed to refresh monsters:\n{0}", [Language.German] = "Monster konnten nicht aktualisiert werden:\n{0}" },
            ["Message_NotConnectedConnectionTab"] = new() { [Language.English] = "Please log in on the \"Connection\" tab first.", [Language.German] = "Bitte zuerst im Tab \"Connection\" einloggen." },
            ["Message_SelectPlayerFirst"] = new() { [Language.English] = "Please select a player first.", [Language.German] = "Bitte zuerst einen Spieler auswählen." },
            ["Message_NoResponseReceived"] = new() { [Language.English] = "(No response received)", [Language.German] = "(Keine Antwort erhalten)" },
            ["Message_AdminNotConnected"] = new() { [Language.English] = "Please log in on the \"Connection\" tab first. Admin commands are sent directly to the server.", [Language.German] = "Bitte zuerst im Tab \"Connection\" einloggen. Admin-Befehle werden direkt an den Server gesendet." },
            ["Message_AdminSendFailed"] = new() { [Language.English] = "Admin command could not be sent. See debug log for details.", [Language.German] = "Admin-Befehl konnte nicht gesendet werden. Sieh ins Debug-Log für Details." },
            ["Message_AdminCommandAborted"] = new() { [Language.English] = "Admin command aborted: {0}", [Language.German] = "Admin-Befehl abgebrochen: {0}" },
            ["Message_ErrorCopied"] = new() { [Language.English] = "Error text copied to clipboard!", [Language.German] = "Fehlertext wurde in die Zwischenablage kopiert!" },
            ["Message_ClipboardBusy"] = new() { [Language.English] = "Clipboard is busy. Please try again.", [Language.German] = "Clipboard ist gerade gesperrt. Bitte erneut versuchen." },
            ["Message_LogCopied"] = new() { [Language.English] = "✓ {0} lines copied to clipboard!", [Language.German] = "✓ {0} Zeilen in die Zwischenablage kopiert!" },
            ["Message_NoMessagesToCopy"] = new() { [Language.English] = "⚠ No messages to copy.", [Language.German] = "⚠ Keine Nachrichten zum Kopieren vorhanden." },
            ["Message_LogCleared"] = new() { [Language.English] = "🗑️ Log cleared", [Language.German] = "🗑️ Log geleert" },
            ["Message_AutoScrollStatus"] = new() { [Language.English] = "Auto-Scroll: {0}", [Language.German] = "Auto-Scroll: {0}" },
            ["Message_WarpsLoadError"] = new() { [Language.English] = "Failed to load warps:\n{0}\n\nLoading default warps.", [Language.German] = "Fehler beim Laden der Warps:\n{0}\n\nStandard-Warps werden geladen." },
            ["Message_WarpsSaveError"] = new() { [Language.English] = "Failed to save warps:\n{0}", [Language.German] = "Fehler beim Speichern der Warps:\n{0}" },
            ["Message_WarpsExportError"] = new() { [Language.English] = "Failed to export:\n{0}", [Language.German] = "Fehler beim Exportieren:\n{0}" },
            ["Message_WarpsImportInvalid"] = new() { [Language.English] = "The file contains no valid warp data.", [Language.German] = "Die Datei enthält keine gültigen Warp-Daten." },
            ["Message_WarpsImportError"] = new() { [Language.English] = "Failed to import:\n{0}", [Language.German] = "Fehler beim Importieren:\n{0}" },
            ["Message_ExtractedRoomsLoadWarning"] = new() { [Language.English] = "Warning: Could not load extracted rooms:\n{0}\n\nLoading default warps...", [Language.German] = "Warnung: Konnte extrahierte Räume nicht laden:\n{0}\n\nLade Standard-Warps..." },
            ["Message_ItemsLoadError"] = new() { [Language.English] = "Failed to load items:\n{0}", [Language.German] = "Fehler beim Laden der Items:\n{0}" },
            ["Message_ItemsSaveError"] = new() { [Language.English] = "Failed to save items:\n{0}", [Language.German] = "Fehler beim Speichern der Items:\n{0}" },
            ["Message_MonstersLoadError"] = new() { [Language.English] = "Failed to load monsters:\n{0}", [Language.German] = "Fehler beim Laden der Monster:\n{0}" },
            ["Message_MonstersSaveError"] = new() { [Language.English] = "Failed to save monsters:\n{0}", [Language.German] = "Fehler beim Speichern der Monster:\n{0}" },
        };

        public string GetString(string key)
        {
            if (Strings.TryGetValue(key, out var translations) &&
                translations.TryGetValue(CurrentLanguage, out var value))
            {
                return value;
            }
            return BuildFallback(key, CurrentLanguage);
        }

        public string this[string key] => GetString(key);

        private static readonly HashSet<string> KeyPrefixes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Button", "Tab", "Header", "Label", "Section", "Menu", "Title", "Window", "Message", "Prompt", "Placeholder", "Default"
        };

        private static readonly Dictionary<string, string> GermanTokenMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Add"] = "Hinzufügen",
            ["Admin"] = "Admin",
            ["Account"] = "Account",
            ["Accounts"] = "Accounts",
            ["All"] = "Alle",
            ["Arena"] = "Arena",
            ["Back"] = "Zurück",
            ["Boost"] = "Boost",
            ["Category"] = "Kategorie",
            ["Clear"] = "Löschen",
            ["Command"] = "Befehl",
            ["Commands"] = "Befehle",
            ["Console"] = "Konsole",
            ["Connection"] = "Verbindung",
            ["Copy"] = "Kopieren",
            ["Create"] = "Erstellen",
            ["Custom"] = "Eigener",
            ["Debug"] = "Debug",
            ["Delete"] = "Löschen",
            ["Details"] = "Details",
            ["Disable"] = "Deaktivieren",
            ["Disconnect"] = "Trennen",
            ["Edit"] = "Bearbeiten",
            ["Enable"] = "Aktivieren",
            ["EndTour"] = "Tour beenden",
            ["Event"] = "Event",
            ["Export"] = "Exportieren",
            ["Execute"] = "Ausführen",
            ["Food"] = "Essen",
            ["Get"] = "Holen",
            ["Gems"] = "Edelsteine",
            ["Give"] = "Geben",
            ["Global"] = "Global",
            ["Hide"] = "Ausblenden",
            ["Header"] = "Header",
            ["Import"] = "Importieren",
            ["Inspector"] = "Inspektor",
            ["Item"] = "Item",
            ["Items"] = "Items",
            ["Inventory"] = "Inventar",
            ["Light"] = "Licht",
            ["Lights"] = "Lichter",
            ["Lock"] = "Sperren",
            ["Unlock"] = "Entsperren",
            ["Teleport"] = "Teleportieren",
            ["List"] = "Liste",
            ["Load"] = "Laden",
            ["Log"] = "Log",
            ["Login"] = "Anmelden",
            ["Connect"] = "Verbinden",
            ["Message"] = "Nachricht",
            ["Messages"] = "Nachrichten",
            ["Monster"] = "Monster",
            ["Monsters"] = "Monster",
            ["Map"] = "Karte",
            ["Misc"] = "Sonstiges",
            ["Object"] = "Objekt",
            ["Objects"] = "Objekte",
            ["Open"] = "Öffnen",
            ["Player"] = "Spieler",
            ["Players"] = "Spieler",
            ["Property"] = "Eigenschaft",
            ["Refresh"] = "Aktualisieren",
            ["Reload"] = "Neu laden",
            ["Remove"] = "Entfernen",
            ["Resource"] = "Ressource",
            ["Responses"] = "Antworten",
            ["Rings"] = "Ringe",
            ["Wands"] = "Stäbe",
            ["Necklaces"] = "Halsketten",
            ["Ammo"] = "Munition",
            ["Summon"] = "Beschwören",
            ["Save"] = "Speichern",
            ["Send"] = "Senden",
            ["Server"] = "Server",
            ["Settings"] = "Einstellungen",
            ["Show"] = "Anzeigen",
            ["Shadow"] = "Schatten",
            ["Stealth"] = "Stealth",
            ["Anonymous"] = "Anonym",
            ["Blank"] = "Leer",
            ["Hidden"] = "Versteckt",
            ["Immortal"] = "Unsterblich",
            ["Mortal"] = "Sterblich",
            ["Karma"] = "Karma",
            ["Skills"] = "Skills",
            ["Spells"] = "Zauber",
            ["Start"] = "Start",
            ["Stop"] = "Stoppen",
            ["End"] = "Beenden",
            ["Status"] = "Status",
            ["System"] = "System",
            ["Test"] = "Test",
            ["Time"] = "Zeit",
            ["Tour"] = "Tour",
            ["Update"] = "Aktualisieren",
            ["Users"] = "User",
            ["Warp"] = "Warp",
            ["Warps"] = "Warps"
        };

        private static readonly HashSet<string> GermanVerbFirstTokens = new(StringComparer.OrdinalIgnoreCase)
        {
            "Add", "Remove", "Delete", "Show", "Hide", "Start", "End", "Save", "Reload", "Copy",
            "Clear", "Send", "Set", "Create", "Enable", "Disable", "Give", "Get", "Open", "Update", "Refresh"
        };

        private static string BuildFallback(string key, Language language)
        {
            if (string.IsNullOrWhiteSpace(key))
                return string.Empty;

            var rawTokens = key.Split('_', StringSplitOptions.RemoveEmptyEntries);
            var tokens = rawTokens.SelectMany(SplitToken).ToArray();
            if (tokens.Length == 0)
                return key;

            if (KeyPrefixes.Contains(tokens[0]))
            {
                tokens = tokens.Skip(1).ToArray();
            }

            if (tokens.Length == 0)
                return key;

            if (tokens.Length == 2 &&
                string.Equals(tokens[0], "Login", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(tokens[1], "Connect", StringComparison.OrdinalIgnoreCase))
            {
                return language == Language.German ? "Anmelden & Verbinden" : "Login & Connect";
            }

            if (language == Language.German && tokens.Length == 2 && GermanVerbFirstTokens.Contains(tokens[0]))
            {
                var noun = TranslateToken(tokens[1], language);
                var verb = TranslateToken(tokens[0], language).ToLowerInvariant();
                return $"{noun} {verb}";
            }

            return string.Join(" ", tokens.Select(token => TranslateToken(token, language)));
        }

        private static IEnumerable<string> SplitToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                yield break;

            var current = new List<char> { token[0] };
            for (int i = 1; i < token.Length; i++)
            {
                char c = token[i];
                char prev = token[i - 1];

                bool boundary = char.IsUpper(c) && char.IsLower(prev);
                if (boundary)
                {
                    yield return new string(current.ToArray());
                    current.Clear();
                }
                current.Add(c);
            }

            if (current.Count > 0)
                yield return new string(current.ToArray());
        }

        private static string TranslateToken(string token, Language language)
        {
            var normalized = NormalizeToken(token);
            if (language == Language.German && GermanTokenMap.TryGetValue(normalized, out var translated))
                return translated;

            return NormalizeToken(normalized);
        }

        private static string NormalizeToken(string token)
        {
            if (string.Equals(token, "Dm", StringComparison.OrdinalIgnoreCase)) return "DM";
            if (string.Equals(token, "Pk", StringComparison.OrdinalIgnoreCase)) return "PK";
            if (string.Equals(token, "Npc", StringComparison.OrdinalIgnoreCase)) return "NPC";
            if (string.Equals(token, "Id", StringComparison.OrdinalIgnoreCase)) return "ID";
            if (string.Equals(token, "Ip", StringComparison.OrdinalIgnoreCase)) return "IP";
            if (string.Equals(token, "Ok", StringComparison.OrdinalIgnoreCase)) return "OK";
            return token;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
