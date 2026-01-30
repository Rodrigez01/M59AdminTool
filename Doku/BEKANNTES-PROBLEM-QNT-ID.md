# Bekanntes Problem: QNT_ID Einfügung in blakston.khd

**Status:** KRITISCHER BUG
**Betroffen:** BlakstonKhdService.cs
**Erstellt:** 2026-01-04

---

## Problem-Beschreibung

### Symptom

Die `AddQuestConstants()` Funktion fügt QNT_ID-Konstanten (Quest Node Template IDs) an der **falschen Position** in die `blakston.khd`-Datei ein.

### Erwartet

```c
// Quest Template IDs
#define QST_ID_APOTHECARY               = 50

// Quest Node Template IDs. Each quest has multiple Quest Nodes.
#define QNT_ID_APOTHECARY_ONE           = 157
#define QNT_ID_APOTHECARY_TWO           = 158
#define QNT_ID_CHICKENSOUP_ONE          = 49
#define QNT_ID_CHICKENSOUP_TWO          = 50
#define QNT_ID_CHICKENSOUP_THREE        = 51
```

### Tatsächlich (BUG)

```c
// Quest Template IDs
#define QST_ID_APOTHECARY               = 50
#define QNT_ID_APOTHECARY_ONE           = 157   <-- FALSCH! Hier nicht!
#define QNT_ID_APOTHECARY_TWO           = 158

// Quest Node Template IDs. Each quest has multiple Quest Nodes.
#define QNT_ID_CHICKENSOUP_ONE          = 49
#define QNT_ID_CHICKENSOUP_TWO          = 50
#define QNT_ID_CHICKENSOUP_THREE        = 51
```

**Problem:** QNT_IDs werden direkt nach QST_IDs eingefügt statt in den separaten "Quest Node Template IDs"-Bereich.

---

## Root Cause Analysis

### Betroffene Datei

`Services/BlakstonKhdService.cs`

### Problemstelle 1: FindNodeIdInsertionPoint()

**Zeilen 208-219:**

```csharp
private int FindNodeIdInsertionPoint(List<string> lines)
{
    for (int i = lines.Count - 1; i >= 0; i--)
    {
        if (Regex.IsMatch(lines[i].Trim(), @"QNT_ID_\w+_\d+\s*=\s*\d+"))
        {
            return i + 1;  // ✓ Korrekt: Nach letzter QNT_ID
        }
    }
    // If no QNT_ID found, find after QST_ID section
    return FindQuestIdInsertionPoint(lines) + 10;  // ✗ FALSCH!
    //                                      ^^^^
    //                         Willkürlicher Offset!
}
```

**Problem:**
- Wenn KEINE QNT_IDs vorhanden sind, wird `FindQuestIdInsertionPoint() + 10` verwendet
- Der Offset `+10` ist willkürlich und nicht korrekt
- Führt dazu, dass QNT_IDs in der QST_ID-Section landen

### Problemstelle 2: Fehlende Section-Erkennung

Die Funktion sucht NICHT nach dem Kommentar `// Quest Node Template IDs`, sondern fügt einfach an einer zufälligen Position ein.

**Erwartetes Verhalten:**
1. Suche nach Kommentar `// Quest Node Template IDs`
2. Füge QNT_IDs **nach** diesem Kommentar ein
3. Falls Kommentar nicht existiert: Erstelle Section nach QST_ID-Bereich

---

## Lösung

### Schritt 1: Section-Header erkennen

Erweitere `FindNodeIdInsertionPoint()` um Section-Header-Erkennung:

```csharp
private int FindNodeIdInsertionPoint(List<string> lines)
{
    // 1. Versuche: Nach letzter existierender QNT_ID
    for (int i = lines.Count - 1; i >= 0; i--)
    {
        if (Regex.IsMatch(lines[i].Trim(), @"QNT_ID_\w+_\d+\s*=\s*\d+"))
        {
            return i + 1;
        }
    }

    // 2. Versuche: Finde "Quest Node Template IDs" Header
    for (int i = 0; i < lines.Count; i++)
    {
        if (lines[i].Contains("Quest Node Template IDs"))
        {
            // Finde erste Zeile nach dem Kommentar (skip blank lines)
            for (int j = i + 1; j < lines.Count; j++)
            {
                if (!string.IsNullOrWhiteSpace(lines[j]))
                {
                    return j;  // Füge vor erster Definition ein
                }
            }
            return i + 2;  // Falls keine Definitionen folgen
        }
    }

    // 3. Fallback: Erstelle neue Section nach QST_IDs
    int lastQstLine = FindLastQuestIdLine(lines);
    if (lastQstLine >= 0)
    {
        // Füge 2 Leerzeilen + Header ein
        lines.Insert(lastQstLine + 1, "");
        lines.Insert(lastQstLine + 2, "");
        lines.Insert(lastQstLine + 3, "// Quest Node Template IDs. Each quest has multiple Quest Nodes.");
        return lastQstLine + 4;
    }

    // 4. Absolute Fallback
    return FindQuestIdInsertionPoint(lines) + 3;
}

private int FindLastQuestIdLine(List<string> lines)
{
    for (int i = lines.Count - 1; i >= 0; i--)
    {
        if (Regex.IsMatch(lines[i].Trim(), @"QST_ID_\w+\s*=\s*\d+"))
        {
            return i;
        }
    }
    return -1;
}
```

### Schritt 2: Separates Einfügen

Die Funktion `AddQuestConstants()` sollte QST_IDs und QNT_IDs in **zwei separaten Durchläufen** einfügen:

```csharp
public void AddQuestConstants(string questName, int nodeCount)
{
    var questNameUpper = questName.ToUpper();
    var (questIds, nodeIds) = ParseConstants();

    if (ConstantsExist(questName, nodeCount))
    {
        Console.WriteLine($"Constants for {questName} already exist. Skipping.");
        return;
    }

    var lines = File.ReadAllLines(_blakstonKhdPath).ToList();

    // === PHASE 1: QST_ID einfügen ===
    if (!questIds.ContainsKey(questNameUpper))
    {
        int questIdInsertLine = FindQuestIdInsertionPoint(lines);
        var questId = FindNextQuestId();
        var questIdLine = $"   QST_ID_{questNameUpper.PadRight(30)} = {questId}";
        lines.Insert(questIdInsertLine, questIdLine);
    }

    // === PHASE 2: QNT_IDs einfügen (NACH QST_ID!) ===
    var nextNodeId = FindNextNodeId();
    int nodeIdInsertLine = FindNodeIdInsertionPoint(lines);  // Neu berechnen!

    for (int i = nodeCount; i >= 1; i--)  // Rückwärts!
    {
        var nodeKey = $"{questNameUpper}_{i}";
        if (!nodeIds.ContainsKey(nodeKey))
        {
            var nodeId = nextNodeId + (i - 1);
            var nodeIdLine = $"   QNT_ID_{questNameUpper}_{i.ToString().PadRight(25)} = {nodeId}";
            lines.Insert(nodeIdInsertLine, nodeIdLine);
        }
    }

    // Write back
    File.WriteAllLines(_blakstonKhdPath, lines, new UTF8Encoding(false));
    Console.WriteLine($"Added constants for {questName}: QST_ID + {nodeCount} QNT_IDs");
}
```

**Wichtig:** QNT_IDs werden **rückwärts** eingefügt (i = nodeCount; i >= 1), damit die Reihenfolge korrekt ist:
- QNT_ID_QUEST_ONE
- QNT_ID_QUEST_TWO
- QNT_ID_QUEST_THREE

---

## Test-Plan

### Test 1: Erste Quest (keine QNT_IDs vorhanden)

**Input:**
```c
// Quest Template IDs
#define QST_ID_EXISTINGQUEST = 10

(keine QNT_IDs)
```

**Erwartete Ausgabe:**
```c
// Quest Template IDs
#define QST_ID_EXISTINGQUEST = 10
#define QST_ID_TESTQUEST     = 11

// Quest Node Template IDs. Each quest has multiple Quest Nodes.
#define QNT_ID_TESTQUEST_ONE   = 1
#define QNT_ID_TESTQUEST_TWO   = 2
```

### Test 2: Quest hinzufügen (QNT_IDs vorhanden)

**Input:**
```c
// Quest Template IDs
#define QST_ID_EXISTINGQUEST = 10

// Quest Node Template IDs. Each quest has multiple Quest Nodes.
#define QNT_ID_EXISTINGQUEST_ONE = 1
#define QNT_ID_EXISTINGQUEST_TWO = 2
```

**Erwartete Ausgabe:**
```c
// Quest Template IDs
#define QST_ID_EXISTINGQUEST = 10
#define QST_ID_TESTQUEST     = 11

// Quest Node Template IDs. Each quest has multiple Quest Nodes.
#define QNT_ID_EXISTINGQUEST_ONE = 1
#define QNT_ID_EXISTINGQUEST_TWO = 2
#define QNT_ID_TESTQUEST_ONE     = 3
#define QNT_ID_TESTQUEST_TWO     = 4
#define QNT_ID_TESTQUEST_THREE   = 5
```

### Test 3: Quest mit vielen Nodes

**Input:** Quest mit 10 Nodes

**Erwartete Ausgabe:**
```c
#define QNT_ID_BIGQUEST_ONE    = 100
#define QNT_ID_BIGQUEST_TWO    = 101
...
#define QNT_ID_BIGQUEST_TEN    = 109
```

**ALLE QNT_IDs müssen in der "Quest Node Template IDs"-Section sein, NICHT in der QST_ID-Section!**

---

## Workaround (Temporär)

Bis der Fix implementiert ist:

1. **Quest erstellen** mit Editor
2. **Manuell `blakston.khd` editieren**:
   - QNT_IDs aus QST_ID-Section **ausschneiden**
   - In "Quest Node Template IDs"-Section **einfügen**
   - IDs anpassen (eindeutige, fortlaufende Nummern)
3. **Speichern** (UTF-8 ohne BOM!)
4. **Server neu kompilieren**

---

## Priorität

**HOCH** - Betrifft jede neu erstellte Quest!

Ohne Fix:
- Manuelle Nacharbeit nötig
- Fehleranfällig
- blakston.khd wird unübersichtlich

---

## Nächste Schritte

1. BlakstonKhdService.cs mit vorgeschlagener Lösung updaten
2. Unit-Tests schreiben (Test 1-3 oben)
3. Testen mit echten Quests
4. Commit & Deploy

---

**Ende der Problem-Dokumentation**
