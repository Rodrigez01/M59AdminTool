# Architektur-Diagramm: QuestEditor System

**Visuelle Übersicht über die System-Architektur**

---

## Gesamtsystem-Überblick

```
┌─────────────────────────────────────────────────────────────────┐
│                         QUESTEDITOR                             │
│                         (WPF Application)                        │
└─────────────────────────────────────────────────────────────────┘
                                │
                                │ Generiert
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                      .KOD DATEIEN                                │
│                    (Blakod Source Code)                          │
└─────────────────────────────────────────────────────────────────┘
                                │
                                │ Kompiliert
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                      .BOF DATEIEN                                │
│                    (Blakod Bytecode)                             │
└─────────────────────────────────────────────────────────────────┘
                                │
                                │ Lädt
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                   MERIDIAN 59 SERVER                             │
│                    (BlakSton/Blakserv)                           │
└─────────────────────────────────────────────────────────────────┘
                                │
                                │ Ausführt
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                      QUESTS IM SPIEL                             │
│                  (Quest Engine Runtime)                          │
└─────────────────────────────────────────────────────────────────┘
```

---

## QuestEditor MVVM-Architektur

```
┌──────────────────────────────────────────────────────────────────┐
│                            VIEW LAYER                             │
│  ┌────────────────────┐          ┌─────────────────────────┐    │
│  │   MainWindow.xaml  │          │  QuestEditor.xaml       │    │
│  │   (Main UI)        │◄────────►│  (Quest Edit UI)        │    │
│  └────────────────────┘          └─────────────────────────┘    │
└──────────────────────────────────────────────────────────────────┘
                │                              │
                │ Data Binding                 │ Data Binding
                ▼                              ▼
┌──────────────────────────────────────────────────────────────────┐
│                        VIEWMODEL LAYER                            │
│  ┌────────────────────┐          ┌─────────────────────────┐    │
│  │  MainViewModel     │          │ QuestEditorViewModel    │    │
│  │  - LoadQuests      │          │ - CurrentQuest          │    │
│  │  - SelectedQuest   │          │ - SaveCommand           │    │
│  │  - Commands        │          │ - AddNodeCommand        │    │
│  └────────────────────┘          └─────────────────────────┘    │
└──────────────────────────────────────────────────────────────────┘
                │                              │
                │ Uses Services                │ Uses Services
                ▼                              ▼
┌──────────────────────────────────────────────────────────────────┐
│                         SERVICE LAYER                             │
│  ┌──────────────┐  ┌──────────────┐  ┌────────────────────┐    │
│  │ KodFileServ. │  │ BlakstonKhd  │  │ ConfigService      │    │
│  │ - Create     │  │ Service      │  │ - LoadConfig       │    │
│  │ - Update     │  │ - AddConsts  │  │ - SaveConfig       │    │
│  │ - Delete     │  │              │  │                    │    │
│  │ - Load       │  │              │  │                    │    │
│  └──────────────┘  └──────────────┘  └────────────────────┘    │
│                                                                   │
│  ┌──────────────┐  ┌──────────────┐  ┌────────────────────┐    │
│  │ LocalizationS│  │ NpcImageServ.│  │ ResourceService    │    │
│  │ - GetString  │  │ - LoadImage  │  │ - LoadResource     │    │
│  └──────────────┘  └──────────────┘  └────────────────────┘    │
└──────────────────────────────────────────────────────────────────┘
                │                              │
                │ Operates on                  │
                ▼                              ▼
┌──────────────────────────────────────────────────────────────────┐
│                          MODEL LAYER                              │
│  ┌────────────────┐  ┌────────────────┐  ┌──────────────┐      │
│  │  Quest         │  │  QuestNode     │  │  Dialog      │      │
│  │  - QuestID     │  │  - NodeIndex   │  │  - Type      │      │
│  │  - Name        │  │  - NodeType    │  │  - Context   │      │
│  │  - Nodes       │  │  - NPCs        │  │  - Text      │      │
│  └────────────────┘  └────────────────┘  └──────────────┘      │
│                                                                   │
│  ┌────────────────┐  ┌────────────────┐  ┌──────────────┐      │
│  │  Cargo         │  │  Prize         │  │  Npc         │      │
│  │  - Type        │  │  - Type        │  │  - Name      │      │
│  │  - ItemClass   │  │  - ItemClass   │  │  - Class     │      │
│  │  - Quantity    │  │  - Quantity    │  │              │      │
│  └────────────────┘  └────────────────┘  └──────────────┘      │
└──────────────────────────────────────────────────────────────────┘
```

---

## Datenfluss: Quest Erstellen

```
┌─────────────────┐
│   User Input    │
│  (GUI Formular) │
└────────┬────────┘
         │
         │ Fill Quest Object
         ▼
┌─────────────────────────┐
│  QuestEditorViewModel   │
│  - Quest-Objekt gefüllt │
└────────┬────────────────┘
         │
         │ SaveCommand.Execute()
         ▼
┌──────────────────────────────────────────────┐
│  KodFileService.CreateQuestAsync(quest)      │
│                                              │
│  1. Validate Quest Structure                │
│  2. Generate KOD Content                     │
│  3. Write .kod File                          │
│  4. Generate LKOD Content (if needed)        │
│  5. Write .lkod File                         │
│  6. Update blakston.khd (Quest IDs)          │
│  7. Update makefile (add .bof)               │
└────────┬─────────────────────────────────────┘
         │
         │ Files Created
         ▼
┌─────────────────────────────────────────────┐
│  File System                                │
│  ├─ questtemplate/myquest.kod               │
│  ├─ questtemplate/myquest.lkod (optional)   │
│  ├─ questtemplate/makefile (updated)        │
│  └─ include/blakston.khd (updated)          │
└─────────────────────────────────────────────┘
         │
         │ Ready for Compilation
         ▼
┌─────────────────────────────────────────────┐
│  Build System                               │
│  $ make                                     │
│  ├─ Compile: myquest.kod → myquest.bof      │
│  └─ Copy to: loadkod/myquest.bof            │
└─────────────────────────────────────────────┘
         │
         │ Server Restart
         ▼
┌─────────────────────────────────────────────┐
│  Meridian 59 Server                         │
│  ├─ Load: myquest.bof                       │
│  ├─ Register: QST_ID_MYQUEST                │
│  └─ Quest Active in Game                    │
└─────────────────────────────────────────────┘
```

---

## Datenfluss: Quest Laden

```
┌─────────────────┐
│   User Action   │
│  (Open Editor)  │
└────────┬────────┘
         │
         │ LoadQuestsAsync()
         ▼
┌──────────────────────────────────────────┐
│  KodFileService.GetAllQuestsAsync()      │
│                                          │
│  1. Scan questtemplate/ for .kod files  │
│  2. Read each .kod file                  │
│  3. Parse KOD content                    │
│  4. Extract Quest Data                   │
│  5. Create Quest Objects                 │
└────────┬─────────────────────────────────┘
         │
         │ List<Quest>
         ▼
┌──────────────────────────────────┐
│  MainViewModel.LoadQuests()      │
│  - Update QuestList              │
│  - Bind to UI                    │
└────────┬─────────────────────────┘
         │
         │ ObservableCollection<Quest>
         ▼
┌──────────────────────────────────┐
│  MainWindow.xaml                 │
│  - Display Quest List            │
│  - Enable Selection              │
└──────────────────────────────────┘
```

---

## KodFileService Workflow

```
                    ┌──────────────────────────┐
                    │  KodFileService          │
                    │                          │
                    │  Constructor()           │
                    │  ├─ Resolve Paths        │
                    │  └─ Initialize           │
                    └────────┬─────────────────┘
                             │
                ┌────────────┴────────────┐
                │                         │
                ▼                         ▼
    ┌───────────────────┐     ┌────────────────────┐
    │  READ Operations  │     │  WRITE Operations  │
    ├───────────────────┤     ├────────────────────┤
    │ LoadAllQuests()   │     │ CreateQuestAsync() │
    │ ├─ ScanDirectory  │     │ ├─ Validate        │
    │ ├─ ReadFiles      │     │ ├─ GenerateKod     │
    │ └─ ParseKod       │     │ ├─ GenerateLkod    │
    │                   │     │ ├─ WriteFiles      │
    │ GetQuestById()    │     │ └─ UpdateSystem    │
    │ └─ FilterList     │     │                    │
    │                   │     │ UpdateQuestAsync() │
    │                   │     │ └─ Similar to      │
    │                   │     │    Create          │
    │                   │     │                    │
    │                   │     │ DeleteQuestAsync() │
    │                   │     │ └─ Remove Files    │
    └───────────────────┘     └────────────────────┘
                │                         │
                └────────────┬────────────┘
                             │
                             ▼
                ┌────────────────────────┐
                │  Helper Methods        │
                ├────────────────────────┤
                │ GenerateKodFile()      │
                │ GenerateLkodFile()     │
                │ ParseKodContent()      │
                │ ValidateStructure()    │
                │ AddToMakefile()        │
                └────────────────────────┘
```

---

## Code-Generierungs-Pipeline

```
┌─────────────────────────────────────────────────────────────────┐
│                    Quest Object (C#)                             │
│  ┌──────────────────────────────────────────────────────┐      │
│  │  Quest                                               │      │
│  │  ├─ QuestKodClass: "MyQuest"                         │      │
│  │  ├─ QuestName: "My Quest Name"                       │      │
│  │  ├─ Nodes: [Node1, Node2, Node3]                     │      │
│  │  └─ Properties: NumPlayers, MaxPlayers, etc.         │      │
│  └──────────────────────────────────────────────────────┘      │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             │ GenerateKodFile()
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                StringBuilder (In-Memory)                         │
│  ┌──────────────────────────────────────────────────────┐      │
│  │  MyQuest is QuestTemplate                            │      │
│  │                                                       │      │
│  │  constants:                                          │      │
│  │     include blakston.khd                             │      │
│  │                                                       │      │
│  │  resources:                                          │      │
│  │     myquest_name_rsc = "My Quest Name"               │      │
│  │     ...                                              │      │
│  │                                                       │      │
│  │  messages:                                           │      │
│  │     Constructor() { ... }                            │      │
│  │     SendQuestNodeTemplates() { ... }                 │      │
│  │                                                       │      │
│  │  end                                                 │      │
│  └──────────────────────────────────────────────────────┘      │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             │ WriteAllTextAsync()
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                  myquest.kod (File System)                       │
│  Encoding: UTF-8 without BOM                                    │
└─────────────────────────────────────────────────────────────────┘
```

---

## Quest-Node Typen & Workflow

```
┌──────────────────────────────────────────────────────────────┐
│                    QUEST NODE TYPES                           │
└──────────────────────────────────────────────────────────────┘

Node 1: QN_TYPE_SHOWUP
┌────────────────────────────────┐
│  Spieler erscheint bei NPC     │
│  ├─ Kein Trigger               │
│  ├─ Automatisch aktiviert      │
│  └─ Startet Quest-Kette        │
└────────────────────────────────┘
         │
         │ Quest gestartet
         ▼
Node 2: QN_TYPE_MESSAGE
┌────────────────────────────────┐
│  Spieler sagt Trigger-Wort     │
│  ├─ Trigger: "help me"         │
│  ├─ Case-sensitive             │
│  └─ Zu bestimmtem NPC          │
└────────────────────────────────┘
         │
         │ Trigger gesagt
         ▼
Node 3: QN_TYPE_ITEM
┌────────────────────────────────┐
│  Spieler gibt Item ab          │
│  ├─ Cargo: 1x ChickenSoup      │
│  ├─ Zu NPC (kann anders sein)  │
│  └─ Prize: Belohnung           │
└────────────────────────────────┘
         │
         │ Item abgegeben
         ▼
┌────────────────────────────────┐
│  QUEST COMPLETED               │
│  ├─ Prize erhalten             │
│  └─ Quest beendet              │
└────────────────────────────────┘
```

---

## NPC-Modifier Logik

```
Quest-Geber (Node 1): NPC A (Jasper Innkeeper)
                      ↓
           ┌──────────┴──────────┐
           │                     │
           ▼                     ▼
    QN_NPCMOD_SAME        QN_NPCMOD_DIFFERENT
    ┌──────────┐          ┌──────────────┐
    │  NPC A   │          │  NPC B, C    │
    │  (Same)  │          │  (Andere)    │
    └──────────┘          └──────────────┘

           │                     │
           ▼                     ▼
    QN_NPCMOD_NONE        QN_NPCMOD_PREVIOUS
    ┌──────────┐          ┌──────────────┐
    │  NPC aus │          │  Vorheriger  │
    │  Liste   │          │  Quest-NPC   │
    └──────────┘          └──────────────┘
```

---

## Build-System Integration

```
┌──────────────────────────────────────────────────────────────┐
│                    Quest Created/Updated                      │
└───────────────────────────┬──────────────────────────────────┘
                            │
              ┌─────────────┼─────────────┐
              │             │             │
              ▼             ▼             ▼
    ┌─────────────┐  ┌──────────┐  ┌─────────────┐
    │  .kod File  │  │ .lkod    │  │ blakston.khd│
    │  Created    │  │ (opt.)   │  │ Updated     │
    └─────────────┘  └──────────┘  └─────────────┘
              │             │             │
              └─────────────┼─────────────┘
                            │
                            ▼
              ┌─────────────────────────┐
              │   makefile Updated      │
              │   (BOFS += myquest.bof) │
              └──────────┬──────────────┘
                         │
                         │ $ make
                         ▼
              ┌─────────────────────────┐
              │   Blakod Compiler       │
              │   ├─ Parse .kod         │
              │   ├─ Check Syntax       │
              │   └─ Generate .bof      │
              └──────────┬──────────────┘
                         │
                         │ Success
                         ▼
              ┌─────────────────────────┐
              │   .bof File             │
              │   (Bytecode)            │
              └──────────┬──────────────┘
                         │
                         │ Copy to loadkod/
                         ▼
              ┌─────────────────────────┐
              │   Server Restart        │
              │   ├─ Load .bof          │
              │   └─ Register Quest     │
              └─────────────────────────┘
```

---

## Datei-Abhängigkeiten

```
myquest.kod
    │
    ├─ include blakston.khd
    │   └─ Benötigt: QST_ID_MYQUEST, QNT_ID_*
    │
    ├─ include myquest.lkod (optional)
    │   └─ Enthält: Übersetzungen (de, es, fr, etc.)
    │
    ├─ makefile
    │   └─ Enthält: myquest.bof in BOFS-Liste
    │
    └─ Referenced Classes
        ├─ &BarloqueTown (NPC-Klasse)
        ├─ &Apple (Item-Klasse)
        └─ &Shillings (Item-Klasse)
```

---

## Error-Handling Flow

```
┌─────────────────┐
│  User Action    │
│  (Save Quest)   │
└────────┬────────┘
         │
         ▼
┌──────────────────────────┐
│  Try CreateQuestAsync()  │
└────────┬─────────────────┘
         │
    ┌────┴─────┐
    │          │
    ▼          ▼
  Success    Exception
    │          │
    │          ├─ ArgumentException
    │          │   └─ Invalid QuestKodClass
    │          │       → Show Error Dialog
    │          │
    │          ├─ IOException
    │          │   └─ File Access Error
    │          │       → Show Error Dialog
    │          │
    │          └─ DirectoryNotFoundException
    │              └─ Path Invalid
    │                  → Show Config Dialog
    │
    ▼
┌─────────────────┐
│  Quest Saved    │
│  Success Dialog │
└─────────────────┘
```

---

## Legende

```
┌─────┐
│ Box │  = Komponente / Prozess
└─────┘

  │     = Datenfluss / Abhängigkeit
  ▼

  ◄──►  = Bidirektionale Verbindung

  ├──   = Verzweigung / Option
  └──
```

---

**Ende des Architektur-Diagramms**

Diese Diagramme zeigen die wichtigsten Zusammenhänge im QuestEditor-System.
