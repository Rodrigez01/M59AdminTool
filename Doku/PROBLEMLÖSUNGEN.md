# Problemlösungen: QuestEditor Troubleshooting

**Erstellt am:** 2026-01-04
**Zweck:** Sammlung bekannter Probleme und deren Lösungen

---

## Inhaltsverzeichnis

1. [Editor-Start-Probleme](#editor-start-probleme)
2. [Quest-Lade-Probleme](#quest-lade-probleme)
3. [Quest-Speicher-Probleme](#quest-speicher-probleme)
4. [Code-Generierungs-Fehler](#code-generierungs-fehler)
5. [Build-System-Probleme](#build-system-probleme)
6. [Laufzeit-Fehler](#laufzeit-fehler)
7. [GUI-Probleme](#gui-probleme)

---

## Editor-Start-Probleme

### Problem: Editor startet nicht / Weißer Bildschirm

**Symptome:**
- Editor-Fenster öffnet sich, aber bleibt weiß
- Keine UI-Elemente sichtbar
- Keine Fehlermeldung

**Mögliche Ursachen & Lösungen:**

#### 1. Fehlende QuestEditor.ini

```bash
# Prüfen, ob Datei existiert
ls QuestEditor.ini

# Falls nicht vorhanden: Erstellen
cat > QuestEditor.ini << EOF
[Paths]
ServerRootPath=C:\meridian59\server
KodPath=C:\meridian59\server\kod
EOF
```

#### 2. Ungültige Pfade in QuestEditor.ini

**Prüfen:**
```bash
# Pfade müssen existieren
dir "C:\meridian59\server"
dir "C:\meridian59\server\kod"
dir "C:\meridian59\server\kod\object\passive\questtemplate"
```

**Fix:**
- Öffne QuestEditor.ini
- Korrigiere Pfade auf tatsächliche Verzeichnisse
- Verwende vollständige, absolute Pfade

#### 3. .NET Runtime fehlt

**Symptom:**
```
Error: The application requires .NET Desktop Runtime 6.0 (or higher)
```

**Lösung:**
```bash
# Prüfe installierte .NET-Versionen
dotnet --list-runtimes

# Falls nicht installiert: Download von
# https://dotnet.microsoft.com/download/dotnet/6.0
```

#### 4. XAML-Compile-Fehler

**Symptom:** Editor startet mit Fehlermeldung

**Prüfen:**
```bash
# Build-Fehler anzeigen
dotnet build QuestEditor.csproj
```

**Häufige Fehler:**
- Fehlende NuGet-Pakete → `dotnet restore`
- Inkompatible Framework-Version → `TargetFramework` in .csproj prüfen

---

## Quest-Lade-Probleme

### Problem: "No quests found" / Leere Quest-Liste

**Symptome:**
- Editor startet erfolgreich
- Quest-Liste bleibt leer
- Keine .kod-Dateien werden angezeigt

**Diagnose:**

```bash
# 1. Prüfe, ob questtemplate-Verzeichnis existiert
cd "C:\meridian59\server\kod\object\passive\questtemplate"

# 2. Prüfe, ob .kod-Dateien vorhanden sind
ls *.kod

# 3. Prüfe Datei-Berechtigungen
# (Windows: Rechtsklick → Eigenschaften → Sicherheit)
```

**Lösungen:**

#### Lösung 1: Falscher Pfad

```ini
# QuestEditor.ini korrigieren
[Paths]
ServerRootPath=C:\richtiger\pfad\zu\server
KodPath=C:\richtiger\pfad\zu\server\kod
```

#### Lösung 2: Keine .kod-Dateien vorhanden

```bash
# Kopiere Beispiel-Quests
cp questtemplate/*.kod "C:\meridian59\server\kod\object\passive\questtemplate\"
```

#### Lösung 3: Datei-Encoding-Problem

**Symptom:** .kod-Dateien vorhanden, aber werden nicht erkannt

```bash
# Prüfe Encoding (muss UTF-8 ohne BOM sein)
file -i *.kod

# Falls BOM vorhanden: Konvertieren
# (Mit Notepad++: Encoding → Convert to UTF-8 without BOM)
```

### Problem: Quest lädt, aber Daten sind inkorrekt

**Symptome:**
- Quest wird angezeigt
- Aber: Fehlende/falsche Node-Informationen
- Dialoge fehlen

**Diagnose:**

```csharp
// Setze Breakpoint in KodFileService.cs:
// - LoadAllQuests()
// - ParseKodFile()

// Prüfe Parsing-Logik
```

**Häufige Ursachen:**

1. **Regex-Pattern stimmt nicht**
   - `.kod`-Datei hat unerwartetes Format
   - Mehrere Leerzeichen/Tabs
   - Ungewöhnliche Kommentare

2. **Node-Parsing fehlgeschlagen**
   - Verschachtelte Listen nicht korrekt geparst
   - Cargo/Prize-Format weicht ab

**Fix:**
```csharp
// Erweitere Regex um optionale Whitespaces
var regex = new Regex(@"(\w+)\s*=\s*""([^""]+)""", RegexOptions.Multiline);
// zu:
var regex = new Regex(@"(\w+)\s*=\s*\\\s*\n\s*""([^""]+)""", RegexOptions.Multiline);
```

---

## Quest-Speicher-Probleme

### Problem: "Failed to save quest"

**Symptome:**
- Quest-Daten eingegeben
- "Save" geklickt
- Fehlermeldung erscheint

**Diagnose:**

```bash
# Prüfe Schreib-Berechtigungen
cd "C:\meridian59\server\kod\object\passive\questtemplate"
echo test > testfile.txt
# Falls Fehler → Berechtigungen fehlen
```

**Lösungen:**

#### Lösung 1: Keine Schreibrechte

```bash
# Windows: Als Administrator ausführen
# Oder: Berechtigungen für Verzeichnis setzen
# Rechtsklick → Eigenschaften → Sicherheit → Bearbeiten
```

#### Lösung 2: Ungültiger QuestKodClass-Name

**Symptom:**
```
ArgumentException: Invalid QuestKodClass: 'questtemplate\MyQuest'
```

**Fix:**
- Nur Klassenname eingeben: `MyQuest`
- **NICHT:** `questtemplate\MyQuest`
- **NICHT:** `C:\...\MyQuest`

#### Lösung 3: Datei bereits geöffnet

**Symptom:**
```
IOException: The file is being used by another process
```

**Fix:**
- Schließe alle Text-Editoren, die .kod-Datei geöffnet haben
- Beende laufende Compiler-Prozesse

### Problem: .lkod-Datei wird nicht erstellt

**Symptome:**
- Quest gespeichert
- .kod-Datei existiert
- Aber .lkod fehlt

**Diagnose:**

```csharp
// Prüfe in KodFileService.cs:CreateQuestAsync()

var hasNonEnDialogs = quest.Nodes
    .Any(n => n.Dialogs != null && n.Dialogs.Any(d =>
        !string.IsNullOrWhiteSpace(d.DialogContext) &&
        d.DialogContext.ToLower() != "en"));

// Wenn false → keine .lkod-Datei wird erstellt
```

**Lösung:**
- Füge Dialoge mit `DialogContext != "en"` hinzu
- Z.B.: `DialogContext = "de"`, `DialogText = "Deutscher Text"`

---

## Code-Generierungs-Fehler

### Problem: Generierte .kod-Datei ist syntaktisch ungültig

**Symptome:**
- Quest gespeichert
- .kod-Datei erstellt
- Aber: Compiler-Fehler beim Build

**Diagnose:**

```bash
cd server/blakserv
bc kod/object/passive/questtemplate/myquest.kod

# Compiler zeigt Zeile mit Fehler
```

**Häufige Fehler:**

#### Fehler 1: Fehlende Kommas

```kod
// FALSCH:
plQuestNodes = [QNT_ID_ONE QNT_ID_TWO];

// RICHTIG:
plQuestNodes = [QNT_ID_ONE, QNT_ID_TWO];
```

**Fix in KodFileService.cs:**
```csharp
// Stelle sicher, dass Join mit Komma erfolgt
var nodeIds = quest.Nodes.Select(...);
sb.Append(string.Join(", ", nodeIds));  // WICHTIG: ", " nicht nur " "
```

#### Fehler 2: Nicht-geschlossene Strings

```kod
// FALSCH:
quest_desc_rsc = \
   "Text ohne schließendes Anführungszeichen

// RICHTIG:
quest_desc_rsc = \
   "Text mit schließendem Anführungszeichen"
```

**Fix:**
```csharp
// Escape-Sequenzen korrekt behandeln
var escapedText = text.Replace("\"", "\\\"");
sb.AppendLine($"   resource_rsc = \"{escapedText}\"");
```

#### Fehler 3: Ungültige NPC-Klassennamen

```kod
// FALSCH:
#cNPC_class=BarloqueTown  // Fehlendes &

// RICHTIG:
#cNPC_class=&BarloqueTown
```

**Fix:**
```csharp
// Stelle sicher, dass & vorhanden ist
var npcClass = node.NpcClass;
if (!npcClass.StartsWith("&"))
    npcClass = "&" + npcClass;
sb.AppendLine($"#cNPC_class={npcClass}");
```

### Problem: Resource-Namen-Konflikt

**Symptom:**
```
Compiler error: Duplicate resource name 'quest_assign'
```

**Ursache:**
- Mehrere Nodes verwenden denselben Resource-Namen

**Fix:**
```csharp
// Einzigartige Resource-Namen pro Node
var resName = $"{quest.QuestKodClass.ToLower()}_node{node.NodeIndex}_{dialog.DialogType}";

// Beispiel:
// myquest_node1_assign
// myquest_node2_assign  (unterschiedlich!)
```

---

## Build-System-Probleme

### Problem: Quest wird nicht kompiliert

**Symptome:**
- .kod-Datei existiert
- `make` ausgeführt
- Aber .bof-Datei fehlt

**Diagnose:**

```bash
cd kod/object/passive/questtemplate

# Prüfe makefile
grep "myquest.bof" makefile

# Falls nicht gefunden → nicht in BOFS-Liste
```

**Lösung:**

```bash
# Manuell hinzufügen
nano makefile

# Füge hinzu:
BOFS = \
    ...
    existingquest.bof \
    myquest.bof \        # <-- Neue Zeile
    anotherquest.bof
```

**Automatische Lösung (sollte Editor machen):**
- Prüfe `TryAddBofToMakefileLines()` in KodFileService.cs
- Stelle sicher, dass Methode aufgerufen wird

### Problem: blakston.khd fehlt Konstanten

**Symptome:**
- .kod-Datei verwendet `QST_ID_MYQUEST`
- Compiler-Fehler: "Undefined constant"

**Diagnose:**

```bash
grep "QST_ID_MYQUEST" include/blakston.khd

# Falls nicht gefunden → Konstante fehlt
```

**Lösung:**

```bash
# Manuell hinzufügen
nano include/blakston.khd

# Füge hinzu:
// Quest Template IDs
#define QST_ID_MYQUEST 999

// Quest Node Template IDs
#define QNT_ID_MYQUEST_ONE 1000
#define QNT_ID_MYQUEST_TWO 1001
```

**Automatische Lösung:**
- Prüfe `BlakstonKhdService.AddQuestConstants()`
- Stelle sicher, dass IDs eindeutig sind (nicht bereits verwendet)

### Problem: ID-Kollision

**Symptom:**
```
Warning: QST_ID_MYQUEST already defined
```

**Ursache:**
- Generierte ID bereits vergeben
- Hash-Kollision (QuestKodClass.GetHashCode())

**Lösung:**

```csharp
// Bessere ID-Generierung
public int GenerateUniqueQuestId(string questName)
{
    // Prüfe existierende IDs
    var existingIds = GetAllQuestIds();

    int id = questName.GetHashCode() & 0x7FFFFFFF;  // Positiv machen

    while (existingIds.Contains(id))
    {
        id++;  // Inkrementiere bis freie ID gefunden
    }

    return id;
}
```

---

## Laufzeit-Fehler

### Problem: Quest erscheint nicht im Spiel

**Symptome:**
- Quest kompiliert erfolgreich
- Server läuft
- Aber Quest wird nicht vergeben

**Diagnose:**

```bash
# Im Spiel (als Admin):
/show quest QST_ID_MYQUEST

# Zeigt Quest-Status
# Falls "Quest not found" → Quest nicht registriert
```

**Lösungen:**

#### Lösung 1: Server-Neustart

```bash
# Server neu starten
./blakserv

# Quest-Engine lädt Quests beim Start
```

#### Lösung 2: Quest manuell aktivieren

```bash
# Im Spiel (als Admin):
/reload quests

# Oder: Quest-Engine neu initialisieren
/reload system
```

#### Lösung 3: Quest-Bedingungen prüfen

```kod
// Prüfe piPlayerRestrict
properties:
   piPlayerRestrict = Q_PLAYER_NOTNEWBIE | Q_PLAYER_LAWFUL

// Spieler muss:
// - Kein Newbie sein
// - Lawful sein

// Sonst: Quest wird nicht angeboten
```

### Problem: Quest-Nodes funktionieren nicht

**Symptome:**
- Quest startet
- Aber Node 2 triggert nicht

**Diagnose:**

```bash
# Im Spiel (als Admin):
/show questnode QNT_ID_MYQUEST_TWO

# Zeigt Node-Konfiguration
# Prüfe:
# - NPC-Liste (muss NPCs enthalten)
# - Node-Typ korrekt
# - Trigger-Text korrekt
```

**Häufige Fehler:**

#### Fehler 1: Falsche NPC-Modifier

```kod
// Quest-Geber: NPC A
// Node 2: NPC_modifier=QN_NPCMOD_SAME, NPC-List=[NPC B]

// PROBLEM: Modifier sagt "same", aber Liste enthält anderen NPC
// FIX: Verwende QN_NPCMOD_DIFFERENT oder korrigiere Liste
```

#### Fehler 2: Falsche Cargo-Definition

```kod
// FALSCH:
#cargolist=[ QN_PRIZETYPE_ITEMCLASS, &Apple, 5 ]

// RICHTIG (verschachtelt!):
#cargolist=[ [ QN_PRIZETYPE_ITEMCLASS, &Apple, 5 ] ]
```

#### Fehler 3: Case-Sensitive Trigger

```kod
// Quest definiert:
chickensoupquest_trigger = "chicken soup"

// Spieler tippt:
"Chicken Soup"  // GROSSSCHREIBUNG

// PROBLEM: Trigger matched nicht (case-sensitive!)
// FIX: Alles in Kleinbuchstaben
```

---

## GUI-Probleme

### Problem: Felder können nicht editiert werden

**Symptome:**
- Quest lädt
- Aber Textfelder sind ausgegraut
- Keine Eingabe möglich

**Ursache:**
- ViewModel-Binding fehlerhaft
- IsEnabled=False in XAML

**Fix:**

```xaml
<!-- Prüfe XAML -->
<TextBox Text="{Binding QuestName}"
         IsEnabled="True" />  <!-- Muss True sein -->
```

```csharp
// Prüfe ViewModel
public string QuestName
{
    get => _questName;
    set
    {
        _questName = value;
        OnPropertyChanged(nameof(QuestName));  // WICHTIG!
    }
}
```

### Problem: Änderungen werden nicht gespeichert

**Symptome:**
- Quest bearbeitet
- "Save" geklickt
- Aber Änderungen gehen verloren

**Diagnose:**

```csharp
// Setze Breakpoint in:
// - QuestEditorViewModel.SaveCommand
// - KodFileService.UpdateQuestAsync()

// Prüfe, ob Methoden aufgerufen werden
```

**Mögliche Ursachen:**

1. **Command-Binding fehlt**

```xaml
<!-- FALSCH: -->
<Button Content="Save" Click="SaveButton_Click" />

<!-- RICHTIG: -->
<Button Content="Save" Command="{Binding SaveCommand}" />
```

2. **CanExecute gibt false zurück**

```csharp
public ICommand SaveCommand { get; }

SaveCommand = new RelayCommand(
    execute: () => SaveQuest(),
    canExecute: () => CanSaveQuest()  // Prüfe diese Methode!
);

private bool CanSaveQuest()
{
    // Muss true zurückgeben, sonst Button deaktiviert
    return !string.IsNullOrWhiteSpace(QuestName);
}
```

### Problem: Quest-Liste aktualisiert sich nicht

**Symptome:**
- Neue Quest erstellt
- Aber erscheint nicht in Liste

**Fix:**

```csharp
// Nach CreateQuestAsync():
private async Task CreateNewQuest()
{
    await _questService.CreateQuestAsync(newQuest);

    // WICHTIG: Liste neu laden!
    await LoadQuestsAsync();

    // ODER: Quest zur Liste hinzufügen
    Quests.Add(newQuest);
}
```

### Problem: Dialoge werden nicht angezeigt

**Symptome:**
- Quest hat Dialoge
- Aber werden in GUI nicht angezeigt

**Diagnose:**

```csharp
// Prüfe Dialog-Collection-Binding
public ObservableCollection<Dialog> Dialogs { get; set; }

// Bei Quest-Load:
Dialogs = new ObservableCollection<Dialog>(quest.Nodes[0].Dialogs);
OnPropertyChanged(nameof(Dialogs));  // WICHTIG!
```

```xaml
<!-- Prüfe ItemsControl -->
<ItemsControl ItemsSource="{Binding Dialogs}">
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding DialogText}" />
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

---

## Allgemeine Debugging-Strategien

### 1. Logging aktivieren

```csharp
// In KodFileService.cs
private async Task<int> CreateQuestAsync(Quest quest)
{
    Debug.WriteLine($"Creating quest: {quest.QuestKodClass}");
    Debug.WriteLine($"Node count: {quest.Nodes.Count}");

    try
    {
        // ... Code
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"ERROR: {ex.Message}");
        Debug.WriteLine($"Stack: {ex.StackTrace}");
        throw;
    }
}
```

### 2. Output-Window überwachen

```
Visual Studio → View → Output
Auswahl: "Debug"

Zeigt alle Debug.WriteLine() Ausgaben
```

### 3. Breakpoints setzen

**Wichtige Stellen:**
- `KodFileService.GenerateKodFile()` - Code-Generierung
- `KodFileService.LoadAllQuests()` - Quest-Laden
- `QuestEditorViewModel.SaveCommand` - Save-Action
- `QuestEditorViewModel.LoadQuestsAsync()` - Quest-Liste-Laden

### 4. Datei-Vergleich

```bash
# Generierte .kod mit Vorlage vergleichen
diff questtemplate/myquest.kod questtemplate/chickensoupqt.kod

# Zeigt Unterschiede in Struktur
```

### 5. Minimales Beispiel erstellen

```csharp
// Test-Quest mit minimalem Setup
var testQuest = new Quest
{
    QuestKodClass = "TestQuest",
    QuestName = "Test",
    Nodes = new List<QuestNode>
    {
        new QuestNode
        {
            NodeType = "QN_TYPE_SHOWUP",
            NpcClasses = new List<string> { "&BarloqueTown" }
        }
    }
};

await _questService.CreateQuestAsync(testQuest);

// Falls das funktioniert → Problem ist in komplexeren Daten
// Falls das fehlschlägt → Problem ist in Basis-Logik
```

---

## Checkliste bei Problemen

Wenn ein Problem auftritt, arbeite diese Checkliste ab:

- [ ] QuestEditor.ini existiert und enthält gültige Pfade
- [ ] questtemplate-Verzeichnis existiert
- [ ] .NET Runtime ist installiert
- [ ] Schreib-Berechtigungen für questtemplate-Verzeichnis vorhanden
- [ ] blakston.khd ist schreibbar
- [ ] makefile ist schreibbar
- [ ] Keine .kod-Dateien in Text-Editor geöffnet
- [ ] Server ist gestoppt (falls Dateien gesperrt)
- [ ] Output-Window zeigt keine Exceptions
- [ ] Breakpoints in kritischen Methoden gesetzt
- [ ] Minimales Test-Beispiel funktioniert

---

**Ende der Problemlösungen**

Bei persistierenden Problemen: Logs, Fehlermeldungen und .kod-Dateiinhalte sammeln für detaillierte Analyse.
