# KI-Schnelldokumentation: QuestEditor & Blakod-System

**Erstellt am:** 2026-01-04
**Zweck:** Schneller Kontext für KI-Assistenten bei der Arbeit am QuestEditor-Projekt

---

## Inhaltsverzeichnis

1. [Projekt-Übersicht](#projekt-übersicht)
2. [Blakod (Kod) Sprache](#blakod-kod-sprache)
3. [QuestEditor Architektur](#questeditor-architektur)
4. [Dateistruktur](#dateistruktur)
5. [Wichtige Komponenten](#wichtige-komponenten)
6. [Häufige Probleme](#häufige-probleme)
7. [Entwickler-Workflows](#entwickler-workflows)

---

## Projekt-Übersicht

### Was ist der QuestEditor?

Ein WPF-basierter GUI-Editor zum Erstellen und Bearbeiten von Quests für **Meridian 59**, ein MMORPG. Der Editor:

- Generiert automatisch **Blakod (.kod)** Dateien aus GUI-Eingaben
- Verwaltet Quest-Nodes (Quest-Schritte)
- Unterstützt Mehrsprachigkeit via `.lkod` Dateien
- Integriert sich in das Meridian 59 Build-System (Makefile, blakston.khd)

### Technologie-Stack

- **Frontend:** WPF (XAML + C#)
- **Framework:** .NET (aktuelle Version prüfen)
- **Pattern:** MVVM (Model-View-ViewModel)
- **Ausgabe:** Blakod (.kod) Skriptdateien

---

## Blakod (Kod) Sprache

### Grundlagen

Blakod ist eine **objektorientierte Skriptsprache** für Meridian 59:

- **Syntax:** C/Pascal-ähnlich
- **Typisierung:** Dynamisch (Runtime-Type-Checking)
- **Vererbung:** Single Inheritance
- **Paradigma:** Message Passing (ähnlich Smalltalk)

### Datentypen

4-Bit-Tag + 28-Bit-Daten = 32-Bit-Werte

| Tag | Typ | Beschreibung |
|-----|-----|--------------|
| 0 | Nil | `$` (null-Wert) |
| 1 | Integer | -134,217,728 bis 134,217,727 |
| 2 | Object | Referenz auf Spielobjekt |
| 3 | List | Singly-linked List |
| 4 | Resource | String oder Dateiname |
| 5 | Timer | Timer-ID |
| 9 | String | Kod String (Spieler-Eingabe) |
| 10 | Class | Klassen-ID |
| 11 | Message | Message-Handler-ID |
| 13 | Table | Hash Table |

### Dateistruktur (.kod)

```kod
ClassName is SuperClass

constants:
   include blakston.khd
   MY_CONSTANT = 100

resources:
   include classname.lkod
   quest_name_rsc = "Quest Name"
   quest_desc_rsc = "Description"

classvars:
   vrIcon = quest_icon_rsc
   viQuestID = QST_ID_MYQUEST

properties:
   piNumPlayers = 1
   plQuestNodes = $

messages:
   Constructor()
   {
      // Code hier
      propagate;
   }

   SendQuestNodeTemplates()
   {
      local oQE, oLib;
      // Quest-Node-Setup
      return;
   }

end
```

### Namenskonventionen

**Classvars (v):**
- `vi` - integer (viQuestID)
- `vr` - resource (vrIcon, vrName)
- `vb` - boolean
- `vc` - class

**Properties (p):**
- `pi` - integer (piNumPlayers)
- `pl` - list (plQuestNodes)
- `po` - object (poBrain)
- `pr` - resource
- `pb` - boolean

**Lokale Variablen:**
- `i` - integer (iCount)
- `l` - list (lNPCs)
- `o` - object (oQE, oNPC)
- `s` - string (sMessage)

### Wichtige Syntax-Elemente

**Nil-Wert:**
```kod
local oObject;
oObject = $;  // Nil-Zuweisung
```

**Vergleiche:**
```kod
if oObject = $  // Gleichheit
if value <> 10  // Ungleich
```

**Listen:**
```kod
lItems = [item1, item2, item3];
lItems = Cons(newItem, lItems);  // Prepend
foreach oItem in lItems { }
```

**Message Passing:**
```kod
Send(oObject, @MessageName, #param1=value1, #param2=value2);
```

**Propagate vs. Return:**
```kod
propagate;           // Ruft Superklassen-Handler auf
return;              // Gibt $ zurück
return iValue;       // Gibt Wert zurück
```

---

## QuestEditor Architektur

### MVVM-Struktur

```
QuestEditor/
├── Models/              # Datenmodelle (Quest, QuestNode, Dialog, etc.)
├── ViewModels/          # UI-Logik (MainViewModel, QuestEditorViewModel)
├── Views/               # XAML-UI-Definitionen
├── Services/            # Business-Logik
│   ├── KodFileService.cs         # .kod-Dateien lesen/schreiben
│   ├── BlakstonKhdService.cs     # blakston.khd modifizieren
│   ├── ConfigService.cs          # QuestEditor.ini verwalten
│   ├── LocalizationService.cs    # Übersetzungen
│   └── NpcImageService.cs        # NPC-Grafiken
├── questtemplate/       # Beispiel .kod-Dateien
└── doc/                 # Blakod-Dokumentation
```

### Wichtige Services

#### KodFileService.cs

**Hauptaufgaben:**
- `.kod`-Dateien aus `questtemplate/` laden
- Neue `.kod`-Dateien generieren
- `.lkod`-Dateien für Übersetzungen erstellen
- Makefile-Einträge automatisch hinzufügen

**Kern-Methoden:**
```csharp
Task<int> CreateQuestAsync(Quest quest)
Task<bool> UpdateQuestAsync(Quest quest)
Task<List<Quest>> GetAllQuestsAsync()
string GenerateKodFile(Quest quest, bool hasNonEnDialogs)
string GenerateLkodFile(Quest quest)
```

#### BlakstonKhdService.cs

Verwaltet `include/blakston.khd` (Konstanten-Datei):
- Fügt Quest-IDs hinzu (QST_ID_MYQUEST)
- Fügt Quest-Node-IDs hinzu (QNT_ID_MYQUEST_ONE, QNT_ID_MYQUEST_TWO)

#### ConfigService.cs

Lädt/speichert `QuestEditor.ini`:
```ini
[Paths]
ServerRootPath=C:\meridian59\server
KodPath=C:\meridian59\server\kod
```

### Quest-Datenmodell

```csharp
public class Quest
{
    public int QuestTemplateId { get; set; }
    public string QuestKodClass { get; set; }        // Klassenname (z.B. "ApothecaryQuest")
    public string QuestName { get; set; }            // Anzeigename
    public string QuestDescription { get; set; }
    public string IconFilename { get; set; }

    public int NumPlayers { get; set; }              // piNumPlayers
    public int MaxActivePlayers { get; set; }        // piMaxPlayers
    public int SchedulePercent { get; set; }         // piSchedulePct

    public List<QuestNode> Nodes { get; set; }       // Quest-Schritte
    public List<string> PlayerRestrictions { get; set; }
}

public class QuestNode
{
    public int NodeIndex { get; set; }
    public string NodeType { get; set; }             // QN_TYPE_SHOWUP, QN_TYPE_MESSAGE, etc.
    public string NpcModifier { get; set; }          // QN_NPCMOD_NONE, QN_NPCMOD_DIFFERENT

    public List<string> NpcClasses { get; set; }     // &BarloqueTown, &TosTown
    public List<Dialog> Dialogs { get; set; }        // Trigger-Wörter, Antworten
    public List<Cargo> CargoList { get; set; }       // Benötigte Items
    public List<Prize> PrizeList { get; set; }       // Belohnungen

    public int TimeLimit { get; set; }               // Sekunden
}

public class Dialog
{
    public string DialogContext { get; set; }        // "en", "de", etc.
    public string DialogType { get; set; }           // "trigger", "assign", "success", "failure"
    public string DialogText { get; set; }
}
```

---

## Dateistruktur

### Quest-Dateien

**Beispiel: apothecaryqt.kod**
```
questtemplate/
├── apothecaryqt.kod      # Haupt-Kod-Datei
├── apothecaryqt.lkod     # Übersetzungen (optional)
└── makefile              # Build-Konfiguration
```

### Build-System

**Workflow:**
1. `.kod` → Compiler → `.bof` (Bytecode)
2. `.bof` wird vom Server geladen
3. Makefile listet alle `.bof`-Dateien

**Makefile-Format:**
```makefile
BOFS = \
    abstainpvp.bof \
    apothecaryqt.bof \
    chickensoupqt.bof
```

**blakston.khd-Eintrag:**
```c
// Quest Template IDs
#define QST_ID_APOTHECARY 50

// Quest Node Template IDs
#define QNT_ID_APOTHECARY_ONE 157
#define QNT_ID_APOTHECARY_TWO 158
```

---

## Wichtige Komponenten

### Quest-Node-Typen

| Node Type | Beschreibung | Verwendung |
|-----------|--------------|------------|
| `QN_TYPE_SHOWUP` | Spieler erscheint bei NPC | Quest-Start |
| `QN_TYPE_MESSAGE` | Spieler sagt Trigger-Wort | Dialog-Quest |
| `QN_TYPE_ITEM` | Spieler gibt Item ab | Item-Lieferung |
| `QN_TYPE_ITEMCLASS` | Mehrere Items einer Klasse | Sammel-Quest |

### NPC-Modifier

| Modifier | Bedeutung |
|----------|-----------|
| `QN_NPCMOD_NONE` | Beliebiger NPC aus Liste |
| `QN_NPCMOD_SAME` | Derselbe NPC wie vorher |
| `QN_NPCMOD_DIFFERENT` | Anderer NPC als vorher |
| `QN_NPCMOD_PREVIOUS` | Vorheriger Quest-NPC |

### Prize-Typen

```kod
[ QN_PRIZETYPE_ITEMCLASS, &Shillings, 600 ]      // Item + Menge
[ QN_PRIZETYPE_OUTLAW ]                          // Strafe: Outlaw-Status
[ QN_PRIZETYPE_BOON, 60*60*24*3 ]               // Segen (3 Tage)
```

---

## Häufige Probleme

### Problem 1: Pfad-Konfiguration

**Symptom:** Editor findet keine `.kod`-Dateien

**Lösung:**
```ini
[Paths]
ServerRootPath=C:\Pfad\zu\meridian59\server
KodPath=C:\Pfad\zu\meridian59\server\kod
```

Prüfen:
- `KodPath\object\passive\questtemplate` existiert
- Enthält `.kod`-Dateien

### Problem 2: Ungültige QuestKodClass

**Symptom:** ArgumentException bei Create/Update

**Lösung:**
- Nur Klassenname, keine Pfade: `ApothecaryQuest` ✓
- Nicht: `questtemplate\ApothecaryQuest` ✗
- Keine Sonderzeichen außer Buchstaben

### Problem 3: Encoding-Probleme

**Symptom:** Umlaute/Sonderzeichen in .kod falsch

**Lösung:**
- Dateien MÜSSEN UTF-8 **ohne BOM** sein
- `new UTF8Encoding(false)` verwenden

### Problem 4: Quest-Node-Validierung

**Symptom:** Generierte .kod-Datei ist ungültig

**Häufige Fehler:**
- Fehlende Quest-Node-IDs in blakston.khd
- Falsche NPC-Klassennamen (&NPCName statt &NPC_Name)
- Ungültige Cargo/Prize-Definitionen

### Problem 5: Makefile-Integration

**Symptom:** Quest wird nicht kompiliert

**Lösung:**
- Prüfen: `makefile` enthält `questname.bof`
- Format: `    questname.bof \` (4 Spaces + Backslash am Ende)

---

## Entwickler-Workflows

### Neue Quest erstellen

1. **GUI:** Quest-Daten eingeben
2. **KodFileService:** `CreateQuestAsync(quest)`
3. **Generierung:**
   - `questname.kod` erstellen
   - Optional: `questname.lkod` für Übersetzungen
4. **Integration:**
   - `blakston.khd` aktualisieren (QST_ID, QNT_IDs)
   - `makefile` aktualisieren (questname.bof eintragen)
5. **Build:** Server neu kompilieren

### Quest bearbeiten

1. **Laden:** `GetQuestByIdAsync(id)` oder `GetAllQuestsAsync()`
2. **Parsen:** `.kod`-Datei analysieren
3. **Anzeigen:** Daten in GUI laden
4. **Speichern:** `UpdateQuestAsync(quest)`
5. **Generierung:** Neue `.kod`-Datei überschreiben

### Debug-Workflow

**Generierte .kod-Datei prüfen:**
```bash
# Vergleichen mit Beispiel-Quest
diff questtemplate/chickensoupqt.kod questtemplate/meine_neue_quest.kod
```

**Kompilierung testen:**
```bash
cd server/blakserv
make
# Fehler in .kod? → Syntax-Fehler in Generierung
```

**Quest im Spiel testen:**
- Server starten
- Quest sollte in Quest-Engine erscheinen
- NPCs sollten Quest vergeben können

---

## Code-Generierungs-Beispiel

### Input (Quest-Objekt)

```csharp
var quest = new Quest
{
    QuestKodClass = "TestQuest",
    QuestName = "Test Quest",
    NumPlayers = 1,
    Nodes = new List<QuestNode>
    {
        new QuestNode
        {
            NodeIndex = 0,
            NodeType = "QN_TYPE_SHOWUP",
            NpcClasses = new List<string> { "&BarloqueTown" }
        },
        new QuestNode
        {
            NodeIndex = 1,
            NodeType = "QN_TYPE_MESSAGE",
            Dialogs = new List<Dialog>
            {
                new Dialog { DialogType = "trigger", DialogText = "help me" },
                new Dialog { DialogType = "assign", DialogText = "Please help!" }
            }
        }
    }
};
```

### Output (testquest.kod)

```kod
TestQuest is QuestTemplate

constants:
   include blakston.khd

resources:
   testquest_icon_rsc = default.bgf
   testquest_name_rsc = "Test Quest"
   testquest_desc_rsc = "Test Quest Description"

classvars:
   vrIcon = testquest_icon_rsc
   vrName = testquest_name_rsc
   vrDesc = testquest_desc_rsc
   viQuestID = QST_ID_TESTQUEST

properties:
   piNumPlayers = 1
   piMaxPlayers = 10
   piSchedulePct = 100

messages:
   Constructor()
   {
      plQuestNodes = [QNT_ID_TESTQUEST_ONE, QNT_ID_TESTQUEST_TWO];
      propagate;
   }

   SendQuestNodeTemplates()
   {
      local oQE, oLib, lNPCs, oNPC;

      oQE = Send(SYS, @GetQuestEngine);
      oLib = Send(SYS, @GetLibrary);

      if Send(oQE, @AddQuestNodeTemplate,
              #questnode_type=QN_TYPE_SHOWUP,
              #quest_node_index=QNT_ID_TESTQUEST_ONE)
      {
         lNPCs = $;
         foreach oNPC in Send(oLib, @GetOccupationList, #cNPC_class=&BarloqueTown)
         {
            lNPCs = Cons(oNPC, lNPCs);
         }
         Send(oQE, @SetQuestNodeNPCList,
              #index=QNT_ID_TESTQUEST_ONE,
              #new_NPC_list=lNPCs);
      }

      return;
   }

end
```

---

## Referenzen

### Wichtige Dateien

| Datei | Pfad | Zweck |
|-------|------|-------|
| blakston.khd | `include/blakston.khd` | Konstanten-Definitionen |
| kodspec.md | `doc/kodspec.md` | Blakod-Spezifikation |
| kodsyntax.md | `doc/kodsyntax.md` | Syntax-Referenz |
| koddatatypes.md | `doc/koddatatypes.md` | Datentypen-Referenz |
| kodccalls.md | `doc/kodccalls.md` | C-Call-Referenz (Standard-Library) |

### Nützliche Konstanten

```c
// Player Restrictions
Q_PLAYER_NOTNEWBIE
Q_PLAYER_NEWBIE
Q_PLAYER_LAWFUL
Q_PLAYER_NOTTRIED_RECENTLY

// Quest Node Types
QN_TYPE_SHOWUP
QN_TYPE_MESSAGE
QN_TYPE_ITEM
QN_TYPE_ITEMCLASS

// NPC Modifiers
QN_NPCMOD_NONE
QN_NPCMOD_SAME
QN_NPCMOD_DIFFERENT
QN_NPCMOD_PREVIOUS

// Prize Types
QN_PRIZETYPE_ITEMCLASS
QN_PRIZETYPE_OUTLAW
QN_PRIZETYPE_BOON
```

---

## Nächste Schritte bei Problemen

1. **Editor startet nicht:**
   - Prüfe `QuestEditor.ini`
   - Prüfe .NET-Version
   - Prüfe XAML-Compile-Fehler

2. **Quests laden nicht:**
   - Prüfe Pfad zu `questtemplate/`
   - Prüfe `.kod`-Datei-Syntax
   - Prüfe Parsing-Logik in `KodFileService`

3. **Quest-Generierung fehlerhaft:**
   - Vergleiche mit Beispiel-Quest (chickensoupqt.kod)
   - Prüfe `GenerateKodFile()` Methode
   - Validiere Quest-Datenmodell

4. **Build-Fehler:**
   - Prüfe `makefile` Eintrag
   - Prüfe `blakston.khd` Konstanten
   - Kompiliere mit `make` und prüfe Fehler

---

**Ende der KI-Schnelldokumentation**

Bei weiteren Fragen: Analysiere die referenzierten Dateien im `doc/` Verzeichnis.
