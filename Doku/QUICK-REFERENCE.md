# Quick Reference: QuestEditor & Blakod

**Schnellreferenz für häufig benötigte Informationen**

---

## Blakod Syntax Cheat Sheet

### Grundstruktur

```kod
ClassName is SuperClass

constants:
   include blakston.khd
   MY_CONST = 100

resources:
   include classname.lkod
   name_rsc = "Name"

classvars:
   viConstant = 100
   vrResource = name_rsc

properties:
   piVariable = 10
   plList = $

messages:
   Constructor()
   {
      propagate;
   }

   MyMessage(param1 = $, param2 = 1)
   {
      local iLocal;
      return iLocal;
   }

end
```

### Namenskonventionen

| Prefix | Typ | Scope | Beispiel |
|--------|-----|-------|----------|
| `v` | Classvar | Klasse (read-only) | `viQuestID`, `vrIcon` |
| `p` | Property | Instanz (read-write) | `piNumPlayers`, `plNodes` |
| (keine) | Local/Param | Message | `iCount`, `oObject` |

| Typ-Buchstabe | Bedeutung | Beispiel |
|---------------|-----------|----------|
| `i` | Integer | `viCount`, `piHealth` |
| `r` | Resource | `vrName`, `vrIcon` |
| `o` | Object | `poOwner`, `oQuestEngine` |
| `l` | List | `plItems`, `lNPCs` |
| `b` | Boolean | `pbActive`, `bIsEnabled` |
| `s` | String | `psMessage`, `sName` |
| `c` | Class | `vcDefaultClass` |
| `h` | Hashtable | `phData` |
| `t` | Timer | `ptTimer` |

### Operatoren

| Operator | Bedeutung | Beispiel |
|----------|-----------|----------|
| `=` | Gleichheit & Zuweisung | `if a = 10` / `a = 10;` |
| `<>` | Ungleich | `if a <> 10` |
| `$` | Nil (null) | `oObject = $;` |
| `@` | Message-ID | `Send(obj, @Message)` |
| `&` | Class-ID | `&ClassName` |
| `*` | Local-Var-Adresse | `*iLocal` (für C-Calls) |
| `\` | Division | `i = 10 \ 3;` |
| `//` | Kommentar | `// Kommentar` |
| `/* */` | Multi-Line-Kommentar | `/* Kommentar */` |

### Control Flow

```kod
// If-Statement
if condition
{
   // code
}
else if other_condition
{
   // code
}
else
{
   // code
}

// While-Loop
while condition
{
   // code
   break;     // Beendet Loop
   continue;  // Nächste Iteration
}

// For-Loop
for (i = 0; i < 10; i++)
{
   // code
}

// Foreach-Loop
foreach oItem in lItems
{
   // code
}

// Do-While-Loop
do
{
   // code
} while condition;

// Switch-Case
switch (value)
{
   case CONST1:
      // code
      break;
   case CONST2:
      // code
      break;
   default:
      // code
      break;
}
```

### Message Passing

```kod
// Einfacher Send
Send(oObject, @MessageName);

// Mit Parametern (named parameters)
Send(oObject, @MessageName, #param1=value1, #param2=value2);

// Rückgabewert
iResult = Send(oObject, @GetValue);

// Propagate (Superklasse aufrufen)
propagate;

// Return
return;           // Gibt $ zurück
return iValue;    // Gibt Wert zurück
```

### Listen

```kod
// Liste erstellen
lItems = [item1, item2, item3];
lEmpty = $;

// Prepend (schnell, O(1))
lItems = Cons(newItem, lItems);

// First/Rest
oFirst = First(lItems);
lRest = Rest(lItems);

// Nth (1-basiert!)
oThird = Nth(lItems, 3);

// Length
iCount = Length(lItems);

// Append (langsam, O(n))
lItems = lItems @ [newItem];
```

---

## Quest-System Konstanten

### Quest Node Types

```c
QN_TYPE_SHOWUP        // Spieler erscheint bei NPC
QN_TYPE_MESSAGE       // Spieler sagt Trigger-Wort
QN_TYPE_ITEM          // Spieler gibt Item ab
QN_TYPE_ITEMCLASS     // Spieler gibt Items einer Klasse ab
```

### NPC Modifiers

```c
QN_NPCMOD_NONE        // Beliebiger NPC aus Liste
QN_NPCMOD_SAME        // Derselbe NPC wie vorher
QN_NPCMOD_DIFFERENT   // Anderer NPC als vorher
QN_NPCMOD_PREVIOUS    // Vorheriger Quest-NPC
```

### Prize Types

```c
QN_PRIZETYPE_ITEMCLASS      // Item + Anzahl
QN_PRIZETYPE_OUTLAW         // Outlaw-Status (Strafe)
QN_PRIZETYPE_BOON           // Segen (Dauer in Sekunden)
```

### Player Restrictions

```c
Q_PLAYER_NOTNEWBIE          // Kein Newbie
Q_PLAYER_NEWBIE             // Nur Newbies
Q_PLAYER_LAWFUL             // Nur Lawful
Q_PLAYER_NOTTRIED_RECENTLY  // Quest nicht kürzlich gemacht
```

---

## QuestEditor C# Code-Snippets

### Quest erstellen

```csharp
var quest = new Quest
{
    QuestKodClass = "MyQuest",           // OHNE Pfad!
    QuestName = "My Quest",
    QuestDescription = "Quest description",
    IconFilename = "icon.bgf",
    NumPlayers = 1,
    MaxActivePlayers = 10,
    SchedulePercent = 100,
    PlayerRestrictions = new List<string>
    {
        "Q_PLAYER_NOTNEWBIE",
        "Q_PLAYER_NOTTRIED_RECENTLY"
    },
    Nodes = new List<QuestNode>()
};
```

### Quest Node erstellen

```csharp
var node = new QuestNode
{
    NodeIndex = 0,
    NodeType = "QN_TYPE_SHOWUP",
    NpcModifier = "QN_NPCMOD_NONE",
    NpcClasses = new List<string> { "&BarloqueTown", "&TosTown" },
    TimeLimit = 3600,  // Sekunden
    Dialogs = new List<Dialog>(),
    CargoList = new List<Cargo>(),
    PrizeList = new List<Prize>()
};
```

### Dialog erstellen

```csharp
// Englischer Dialog
var dialogEn = new Dialog
{
    DialogType = "assign",      // trigger, assign, success, failure
    DialogContext = "en",
    DialogText = "Please deliver %NUM %CARGO to %NPC."
};

// Deutsche Übersetzung
var dialogDe = new Dialog
{
    DialogType = "assign",
    DialogContext = "de",
    DialogText = "Bitte liefere %NUM %CARGO an %NPC."
};
```

### Cargo/Prize erstellen

```csharp
// Cargo (benötigte Items)
var cargo = new Cargo
{
    CargoType = "QN_PRIZETYPE_ITEMCLASS",
    ItemClass = "&Apple",
    Quantity = 5
};

// Prize (Belohnung)
var prize = new Prize
{
    PrizeType = "QN_PRIZETYPE_ITEMCLASS",
    ItemClass = "&Shillings",
    Quantity = 500
};
```

### Quest speichern

```csharp
// Neue Quest
int questId = await kodFileService.CreateQuestAsync(quest);

// Existierende Quest aktualisieren
bool success = await kodFileService.UpdateQuestAsync(quest);

// Quest löschen
bool success = await kodFileService.DeleteQuestAsync(questId);

// Alle Quests laden
List<Quest> quests = await kodFileService.GetAllQuestsAsync();
```

---

## Datei-Pfade

### Standard-Verzeichnisstruktur

```
meridian59/
├── server/
│   ├── blakserv          # Server-Executable
│   ├── kod/
│   │   ├── include/
│   │   │   └── blakston.khd     # Konstanten
│   │   └── object/
│   │       └── passive/
│   │           └── questtemplate/
│   │               ├── makefile
│   │               ├── myquest.kod
│   │               ├── myquest.lkod
│   │               └── myquest.bof
│   └── loadkod/          # Kompilierte .bof-Dateien
└── QuestEditor/
    ├── QuestEditor.exe
    └── QuestEditor.ini   # Pfad-Konfiguration
```

### QuestEditor.ini

```ini
[Paths]
ServerRootPath=C:\meridian59\server
KodPath=C:\meridian59\server\kod
```

---

## Command-Line Tools

### Blakod Compiler

```bash
# Einzelne Datei kompilieren
bc myquest.kod

# Mit Debug-Informationen
bc -d myquest.kod

# Alle Dateien in Verzeichnis
make

# Clean + Build
make clean
make
```

### Server

```bash
# Server starten
./blakserv

# Mit Debug-Modus
./blakserv -debug

# Mit spezifischer Konfiguration
./blakserv -config myconfig.txt
```

### In-Game Admin-Befehle

```
/show quest QST_ID_MYQUEST           # Quest-Info anzeigen
/show questnode QNT_ID_MYQUEST_ONE   # Node-Info anzeigen
/reload quests                       # Quests neu laden
/reload system                       # System neu laden
```

---

## Regex-Patterns (für Parsing)

### Klassenname extrahieren

```csharp
var regex = new Regex(@"^(\w+)\s+is\s+(\w+)", RegexOptions.Multiline);
// Gruppe 1: Klassenname
// Gruppe 2: Superklasse
```

### Resources extrahieren

```csharp
// Einzeilig
var regex = new Regex(@"(\w+)\s*=\s*""([^""]+)""", RegexOptions.Multiline);

// Mehrzeilig (mit \)
var regex = new Regex(@"(\w+)\s*=\s*\\\s*\n\s*""([^""]+)""", RegexOptions.Multiline);
```

### Quest Node IDs

```csharp
var regex = new Regex(@"plQuestNodes\s*=\s*\[([\w\s,]+)\]", RegexOptions.Multiline);
```

### NPC Classes

```csharp
var regex = new Regex(@"#cNPC_class\s*=\s*(&\w+)", RegexOptions.Multiline);
```

---

## Häufige Fehler & Quick-Fixes

| Fehler | Quick-Fix |
|--------|-----------|
| `ArgumentException: Invalid QuestKodClass` | Nur Klassenname, keine Pfade: `MyQuest` statt `questtemplate\MyQuest` |
| `DirectoryNotFoundException` | QuestEditor.ini Pfade korrigieren |
| `Duplicate resource name` | Einzigartige Namen: `myquest_node1_assign`, `myquest_node2_assign` |
| `Undefined constant QST_ID_X` | BlakstonKhdService.AddQuestConstants() aufrufen |
| `.bof` nicht in makefile | TryAddBofToMakefileLines() prüfen |
| Umlaute falsch | UTF-8 ohne BOM: `new UTF8Encoding(false)` |
| Quest erscheint nicht im Spiel | Server neu starten oder `/reload quests` |
| Node triggert nicht | Case-sensitive Trigger prüfen |
| Integer overflow | Max: 134,217,727 |

---

## Dialog-Platzhalter

| Platzhalter | Ersetzt durch | Beispiel |
|-------------|---------------|----------|
| `%NPC` | Ziel-NPC Name | "Barloque Elder" |
| `%SOURCE_NPC` | Quest-Geber Name | "Jasper Innkeeper" |
| `%HIMHER_NPC` | him/her | "him" |
| `%HISHER_NPC` | his/her | "his" |
| `%CARGO` | Cargo-Item Name | "apples" |
| `%INDEF_CARGO` | Artikel + Cargo | "some apples" |
| `%PRIZE` | Belohnungs-Name | "Long Sword" |
| `%NAME` | Spieler-Name | "PlayerName" |
| `%NUM` | Anzahl | "5" |

### Beispiel

```kod
quest_assign =
   "%SOURCE_NPC needs %NUM %CARGO delivered to %NPC. "
   "Bring them to %HIMHER_NPC quickly!"

// Wird zu:
// "Jasper Innkeeper needs 5 apples delivered to Barloque Elder.
//  Bring them to him quickly!"
```

---

## Debugging Checkliste

**Bei Problemen systematisch durchgehen:**

### Editor startet nicht
- [ ] QuestEditor.ini existiert
- [ ] Pfade in .ini sind gültig
- [ ] .NET Runtime installiert
- [ ] Output-Window für Exceptions prüfen

### Quest lädt nicht
- [ ] .kod-Datei in questtemplate/ vorhanden
- [ ] Datei-Encoding UTF-8 ohne BOM
- [ ] Syntax-Fehler in .kod (bc kompilieren)
- [ ] Parsing-Regex passt zu Dateiformat

### Quest speichert nicht
- [ ] Schreibrechte für questtemplate/
- [ ] QuestKodClass ohne Pfadzeichen
- [ ] Datei nicht in Editor geöffnet
- [ ] Exceptions im Output-Window

### Quest kompiliert nicht
- [ ] .bof in makefile BOFS-Liste
- [ ] QST_ID in blakston.khd definiert
- [ ] QNT_IDs in blakston.khd definiert
- [ ] Syntax-Fehler mit bc prüfen

### Quest funktioniert nicht im Spiel
- [ ] Server neu gestartet
- [ ] .bof in loadkod/ vorhanden
- [ ] Quest-Bedingungen erfüllt (piPlayerRestrict)
- [ ] NPC-Klassen existieren
- [ ] /show quest im Spiel testen

---

## Performance-Tipps

### Kod-Code

```kod
// LANGSAM: Append bei Listen
lItems = lItems @ [newItem];  // O(n)

// SCHNELL: Prepend bei Listen
lItems = Cons(newItem, lItems);  // O(1)

// LANGSAM: Mehrfache Send-Aufrufe
lNPCs = Send(oLib, @GetOccupationList, #cNPC_class=&BarloqueTown);
lNPCs2 = Send(oLib, @GetOccupationList, #cNPC_class=&BarloqueTown);

// SCHNELL: Ergebnis cachen
lNPCs = Send(oLib, @GetOccupationList, #cNPC_class=&BarloqueTown);
// lNPCs wiederverwenden
```

### C# Code

```csharp
// LANGSAM: Regex für jede Zeile neu erstellen
foreach (var line in lines)
{
    var regex = new Regex(@"pattern");  // Schlecht!
}

// SCHNELL: Regex wiederverwenden
var regex = new Regex(@"pattern");
foreach (var line in lines)
{
    var match = regex.Match(line);
}

// LANGSAM: String-Konkatenation
string result = "";
foreach (var item in items)
{
    result += item.ToString();  // Schlecht!
}

// SCHNELL: StringBuilder
var sb = new StringBuilder();
foreach (var item in items)
{
    sb.Append(item.ToString());
}
string result = sb.ToString();
```

---

## Nützliche Links

### Dokumentation
- Blakod-Spezifikation: `../doc/kodspec.md`
- Syntax-Referenz: `../doc/kodsyntax.md`
- C-Calls: `../doc/kodccalls.md`

### Code-Dateien
- KodFileService: `../Services/KodFileService.cs`
- QuestEditorViewModel: `../ViewModels/QuestEditorViewModel.cs`

### Beispiele
- Minimal-Quest: `../questtemplate/chickensoupqt.kod`
- Item-Quest: `../questtemplate/apothecaryqt.kod`

---

**Letzte Aktualisierung:** 2026-01-04
