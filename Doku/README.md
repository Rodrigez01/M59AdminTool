# Doku-Verzeichnis: QuestEditor & Blakod

Dieses Verzeichnis enthält KI-optimierte Dokumentation für die Arbeit am QuestEditor-Projekt.

**Erstellt am:** 2026-01-04

---

## Übersicht der Dateien

| Datei | Zweck | Zielgruppe |
|-------|-------|------------|
| **KI-SCHNELLDOKUMENTATION.md** | Kompakter Überblick über Projekt, Kod-Sprache, Architektur | KI-Assistenten, Neue Entwickler |
| **TECHNISCHE-DETAILS.md** | Code-Beispiele, Parsing, Generierung, Performance-Tipps | Entwickler, KI bei Code-Tasks |
| **PROBLEMLÖSUNGEN.md** | Troubleshooting-Guide, häufige Fehler und Lösungen | Support, Debugging |

---

## Schnellstart für KI-Assistenten

### 1. Neues Problem / Feature

**Ablauf:**
1. Lies `KI-SCHNELLDOKUMENTATION.md` für Kontext
2. Prüfe `PROBLEMLÖSUNGEN.md` ob Problem bereits bekannt
3. Konsultiere `TECHNISCHE-DETAILS.md` für Implementation-Details
4. Falls nötig: Referenziere Original-Doku in `../doc/`

### 2. Code-Generierung / Parser-Fix

**Relevante Abschnitte:**
- `TECHNISCHE-DETAILS.md` → Code-Generierung
- `KI-SCHNELLDOKUMENTATION.md` → Blakod-Syntax
- `../doc/kodsyntax.md` → Vollständige Syntax-Referenz

### 3. Debugging

**Workflow:**
1. `PROBLEMLÖSUNGEN.md` → Symptom identifizieren
2. Relevante Debugging-Strategie anwenden
3. `TECHNISCHE-DETAILS.md` → Code-Stellen prüfen

---

## Wichtige Konzepte (Kurzübersicht)

### Blakod (Kod) Sprache

```kod
ClassName is SuperClass

constants:
   include blakston.khd

resources:
   resource_name = "value"

classvars:
   viConstant = 100

properties:
   piVariable = $

messages:
   MessageName()
   {
      // Code
      propagate;
   }

end
```

**Wichtig:**
- `$` = Nil (null)
- Integer-Range: -134,217,728 bis 134,217,727
- Message Passing statt Methoden-Aufrufe
- `propagate` ruft Superklassen-Handler auf

### QuestEditor Datenfluss

```
GUI (XAML)
    ↓ Binding
ViewModel (QuestEditorViewModel)
    ↓ Commands
Service (KodFileService)
    ↓ Generate
.kod-Datei
    ↓ Compiler
.bof-Datei
    ↓ Server
Quest im Spiel
```

### Quest-Struktur

```
Quest
├── QuestKodClass (Klassenname)
├── QuestName (Anzeigename)
├── Nodes (Quest-Schritte)
│   ├── Node 1: QN_TYPE_SHOWUP (Erscheinen bei NPC)
│   ├── Node 2: QN_TYPE_MESSAGE (Trigger-Wort sagen)
│   └── Node 3: QN_TYPE_ITEM (Item abgeben)
└── Properties (piNumPlayers, piMaxPlayers, etc.)
```

---

## Häufigste Probleme (Top 5)

1. **Pfad-Konfiguration**
   - `QuestEditor.ini` fehlt oder enthält falsche Pfade
   - **Fix:** `PROBLEMLÖSUNGEN.md` → Editor-Start-Probleme

2. **Ungültige Quest-Namen**
   - `QuestKodClass` enthält Pfad-Zeichen
   - **Fix:** Nur Klassenname ohne `\` `/` `:`

3. **Fehlende blakston.khd-Einträge**
   - Quest-IDs nicht definiert
   - **Fix:** `BlakstonKhdService.AddQuestConstants()`

4. **Makefile-Integration**
   - `.bof` nicht in `BOFS`-Liste
   - **Fix:** `TryAddBofToMakefileLines()`

5. **Encoding-Probleme**
   - UTF-8 mit BOM statt ohne BOM
   - **Fix:** `new UTF8Encoding(false)`

---

## Code-Beispiele

### Minimal-Quest erstellen

```csharp
var quest = new Quest
{
    QuestKodClass = "MinimalQuest",
    QuestName = "Minimal Quest",
    QuestDescription = "Just show up at NPC",
    NumPlayers = 1,
    MaxActivePlayers = 10,
    SchedulePercent = 100,
    Nodes = new List<QuestNode>
    {
        new QuestNode
        {
            NodeIndex = 0,
            NodeType = "QN_TYPE_SHOWUP",
            NpcClasses = new List<string> { "&BarloqueTown" }
        }
    }
};

await kodFileService.CreateQuestAsync(quest);
```

### .kod-Datei parsen

```csharp
var kodContent = File.ReadAllText("myquest.kod");

// Klassenname extrahieren
var classNameRegex = new Regex(@"^(\w+)\s+is\s+(\w+)", RegexOptions.Multiline);
var match = classNameRegex.Match(kodContent);
var className = match.Groups[1].Value;

// Resources extrahieren
var resourceRegex = new Regex(@"(\w+)\s*=\s*""([^""]+)""", RegexOptions.Multiline);
var resources = resourceRegex.Matches(kodContent)
    .Cast<Match>()
    .ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value);
```

### Dialog mit Übersetzung

```csharp
var dialog = new Dialog
{
    DialogType = "assign",
    DialogContext = "de",  // Deutsch
    DialogText = "Bitte liefere %NUM %CARGO zu %NPC."
};

node.Dialogs.Add(dialog);

// Generiert:
// myquest_node1_assign = de "Bitte liefere %NUM %CARGO zu %NPC."
```

---

## Referenzen

### Externe Dokumentation

- **Blakod-Spezifikation:** `../doc/kodspec.md`
- **Syntax-Referenz:** `../doc/kodsyntax.md`
- **Datentypen:** `../doc/koddatatypes.md`
- **C-Calls (Standard-Library):** `../doc/kodccalls.md`
- **Resource-System:** `../doc/kodresource.md`

### Wichtige Code-Dateien

- **KodFileService:** `../Services/KodFileService.cs`
- **BlakstonKhdService:** `../Services/BlakstonKhdService.cs`
- **QuestEditorViewModel:** `../ViewModels/QuestEditorViewModel.cs`
- **Quest Model:** `../Models/Quest.cs`

### Beispiel-Quests

- **Minimal:** `../questtemplate/chickensoupqt.kod`
- **Item-Delivery:** `../questtemplate/apothecaryqt.kod`
- **Multi-Node:** `../questtemplate/loveletterqt.kod`

---

## Wartung dieser Dokumentation

### Wann aktualisieren?

- **Neue Fehler entdeckt** → `PROBLEMLÖSUNGEN.md` ergänzen
- **Neue Features** → `KI-SCHNELLDOKUMENTATION.md` aktualisieren
- **Code-Änderungen** → `TECHNISCHE-DETAILS.md` anpassen

### Format-Richtlinien

- Markdown (GitHub-Flavored)
- Code-Blöcke mit Syntax-Highlighting
- Konkrete Beispiele statt abstrakte Beschreibungen
- Schritt-für-Schritt-Anleitungen für Problemlösungen

### Versionierung

```
v1.0 - 2026-01-04 - Initiale Erstellung
                   - Blakod-Grundlagen
                   - QuestEditor-Architektur
                   - Top-5-Probleme dokumentiert
```

---

## Kontakt / Feedback

Bei Fragen oder Verbesserungsvorschlägen:
- Issue im Repository erstellen
- Dokumentation direkt ergänzen (Pull Request)

---

**Happy Coding!**

Diese Dokumentation soll die Arbeit mit dem QuestEditor erleichtern und typische Stolpersteine vermeiden.
