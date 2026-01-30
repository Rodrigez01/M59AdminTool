# blakston.khd Quest-System Analyse

**Analysiert am:** 2026-01-04
**Datei:** blakston.khd (4048 Zeilen)
**Quest-Bereich:** Zeilen 3344-3993

---

## Inhaltsverzeichnis

1. [Struktur-Übersicht](#struktur-übersicht)
2. [Quest Template IDs](#quest-template-ids)
3. [Quest Node Template IDs](#quest-node-template-ids)
4. [Quest-Konstanten](#quest-konstanten)
5. [Schlussfolgerungen für den Editor](#schlussfolgerungen-für-den-editor)

---

## Struktur-Übersicht

### Zeilen 3344-3993: Kompletter Quest-Bereich

```
3344-3352:  Quest-Basis-Konstanten
3353-3364:  QuestTemplate Field Names
3365-3379:  QuestNodeTemplate Field Names
3380-3464:  Quest Template IDs (QST_ID_*)
3466-3786:  Quest Node Template IDs (QNT_ID_*)  ← KRITISCH!
3787-3947:  Quest Misc Constants & Types
3949+:      Andere Konstanten (Necromancer, Game Events)
```

### Wichtige Erkenntnis

**ZWEI KLAR GETRENNTE BEREICHE:**

1. **Quest Template IDs** (Zeilen 3380-3464): 83 Quests
2. **Quest Node Template IDs** (Zeilen 3466-3786): 246 Quest-Nodes

**Separator:**
```c
// Quest Node Template IDs. Each quest has multple Quest Nodes.
```
Zeile 3466 - **HIER** müssen QNT_IDs eingefügt werden!

---

## Quest Template IDs

### Zeilen 3380-3464 (85 Zeilen)

```c
// Quest Template IDs
QST_ID_NONE                   = 0
QST_ID_SHALILLE_DISCIPLE      = 1
QST_ID_KRAANAN_DISCIPLE       = 2
...
QST_ID_CASTLEVICTORIA_KEY     = 82
```

### Struktur

- **Kommentar:** `// Quest Template IDs` (Zeile 3380)
- **IDs:** 0-82 (83 Quests total)
- **Format:** `QST_ID_QUESTNAME = ID`
- **Spacing:** 3 Spaces Einrückung, Padding mit Spaces

### Beispiele

```c
QST_ID_NONE                   = 0
QST_ID_CHICKEN_SOUP           = 23
QST_ID_APOTHECARY             = 50
QST_ID_CASTLEVICTORIA_KEY     = 82
```

**Letzte ID:** 82 (QST_ID_CASTLEVICTORIA_KEY)

**Nächste verfügbare ID:** 83

---

## Quest Node Template IDs

### Zeilen 3466-3786 (320 Zeilen)

```c
// Quest Node Template IDs. Each quest has multple Quest Nodes.
QNT_ID_DUKE_JOIN_ONE          = 1
QNT_ID_DUKE_JOIN_TWO          = 2
...
QNT_ID_CASTLEVICTORIA_KEY_FOUR = 246
```

### Struktur

- **Kommentar:** `// Quest Node Template IDs. Each quest has multple Quest Nodes.` (Zeile 3466)
- **IDs:** 1-246 (246 Quest-Nodes total)
- **Format:** `QNT_ID_QUESTNAME_NODENAME = ID`
- **Spacing:** 3 Spaces Einrückung, Padding mit Spaces
- **Gruppierung:** Nodes einer Quest stehen zusammen (mit Leerzeile dazwischen)

### Beispiele

```c
// Chicken Soup Quest (3 Nodes)
QNT_ID_CHICKEN_SOUP_ONE       = 49
QNT_ID_CHICKEN_SOUP_TWO       = 50
QNT_ID_CHICKEN_SOUP_THREE     = 51

// Apothecary Quest (2 Nodes)
QNT_ID_APOTHECARY_ONE         = 157
QNT_ID_APOTHECARY_TWO         = 158
```

### Node-Namenskonvention

**Pattern:** `QNT_ID_QUESTNAME_NODENAME`

**Node-Namen:**
- `_ONE`, `_TWO`, `_THREE`, ... (Zahlen als Wörter)
- Nicht: `_1`, `_2`, `_3` (Zahlen als Ziffern)

**Spezial-Fälle:**
```c
QNT_ID_CESS_MURDER_ONE_THREE  = 184   // Komplexe Quests mit Sub-Nodes
QNT_ID_CESS_MURDER_TWO_THREE  = 185
QNT_ID_CESS_MURDER_THREE_THREE = 186
```

**Letzte ID:** 246 (QNT_ID_CASTLEVICTORIA_KEY_FOUR)

**Nächste verfügbare ID:** 247

---

## Quest-Konstanten

### Quest Template Field Names (Zeilen 3353-3364)

```c
// Field names for a QuestTemplate
QT_QST_ID            = 1   // Quest ID
QT_QUEST_OBJECT      = 2   // Quest-Objekt
QT_NUM_PLAYERS       = 3   // Anzahl Spieler (piNumPlayers)
QT_QUEST_TYPE        = 4   // Quest-Typ
QT_PLAYER_RESTRICT   = 5   // Spieler-Einschränkungen
QT_QUEST_NODES       = 6   // Quest-Nodes (plQuestNodes)
QT_MAX_NUM_ACTIVE    = 7   // Max. aktive Quests (piMaxPlayers)
QT_ACTIVE_QUESTS     = 8   // Aktuell aktive Quests
QT_SCHEDULE_CHANCE   = 9   // Schedule-Prozent (piSchedulePct)
QT_PLAYER_RESTRICT2  = 10  // Erweiterte Einschränkungen
```

**Relevanz für Editor:**
- Diese Felder definieren die **Struktur** eines QuestTemplate
- Editor muss diese Properties korrekt setzen

### Quest Node Template Field Names (Zeilen 3365-3379)

```c
// Field names for a QuestNodeTemplate
QNT_QNT_ID        = 1   // Node ID
QNT_NPC_LIST      = 2   // NPC-Liste
QNT_NPC_MODIFIER  = 3   // NPC-Modifier (QN_NPCMOD_*)
QNT_TYPE          = 4   // Node-Typ (QN_TYPE_*)
QNT_CARGO_LIST    = 5   // Cargo-Liste
QNT_MONSTER_LIST  = 6   // Monster-Liste
QNT_PRIZE_LIST    = 7   // Prize-Liste
QNT_PENALTY_LIST  = 8   // Penalty-Liste
QNT_ASSIGN_HINT   = 9   // Assign-Dialog
QNT_SUCCESS_HINT  = 10  // Success-Dialog
QNT_FAILURE_HINT  = 11  // Failure-Dialog
QNT_TIME_LIMIT    = 12  // Zeit-Limit (Sekunden)
QNT_AMOUNT_NEEDED = 13  // Benötigte Anzahl
```

**Relevanz für Editor:**
- Diese Felder definieren die **Struktur** eines QuestNode
- Editor muss alle diese Felder im Quest-Objekt unterstützen

### Quest Node Types (Zeilen 3864-3876)

```c
// Quest node types
QN_TYPE_MESSAGE            = 0x01   // Deliver a message I give you
QN_TYPE_ITEM               = 0x02   // Deliver a specific item I give you
QN_TYPE_ITEMCLASS          = 0x03   // Deliver newly created item of class
QN_TYPE_ITEMFINDCLASS      = 0x04   // Deliver any item of class (find it)
QN_TYPE_SHOWUP             = 0x05   // Just show your face
QN_TYPE_CHESSMOVE          = 0x06   // Chess move (special)
QN_TYPE_USERNAME           = 0x07   // NYI
QN_TYPE_LOGGEDONNAME       = 0x08   // NYI
QN_TYPE_MONSTER            = 0x09   // Kill a monster of type
QN_TYPE_MONSTER_ITEMCLASS  = 0x10   // Kill monster, deliver item
QN_TYPE_MONSTER_BRING      = 0x0A   // Bring charmed monster
```

**Editor sollte unterstützen:**
- ✓ QN_TYPE_MESSAGE
- ✓ QN_TYPE_ITEM
- ✓ QN_TYPE_ITEMCLASS
- ✓ QN_TYPE_ITEMFINDCLASS
- ✓ QN_TYPE_SHOWUP
- ? QN_TYPE_CHESSMOVE (speziell)
- ✓ QN_TYPE_MONSTER
- ✓ QN_TYPE_MONSTER_ITEMCLASS
- ✓ QN_TYPE_MONSTER_BRING

### Quest Player Restrictions (Zeilen 3802-3842)

**Karma-Einschränkungen:**
```c
Q_PLAYER_KARMA_SAME        = 0x1
Q_PLAYER_KARMA_DIFFERENT   = 0x2
Q_PLAYER_KARMA_GOOD        = 0x3
Q_PLAYER_KARMA_NEUTRAL     = 0x4
Q_PLAYER_KARMA_EVIL        = 0x5
Q_PLAYER_KARMA_MASK        = 0xF
```

**Factions-Einschränkungen:**
```c
Q_PLAYER_FACTION_DUKE      = 0x10
Q_PLAYER_FACTION_NEUTRAL   = 0x20
Q_PLAYER_FACTION_PRINCESS  = 0x30
Q_PLAYER_FACTION_REBEL     = 0x40
Q_PLAYER_FACTION_SAME      = 0x50
Q_PLAYER_FACTION_DIFFERENT = 0x60
Q_PLAYER_FACTION_IN        = 0x70   // NYI
Q_PLAYER_FACTION_OUT       = 0x80   // NYI
Q_PLAYER_FACTION_MASK      = 0xF0
```

**Kill/Status-Einschränkungen:**
```c
Q_PLAYER_NEWBIE            = 0x0100
Q_PLAYER_NOTNEWBIE         = 0x0200
Q_PLAYER_MURDERER          = 0x0400
Q_PLAYER_NOTMURDERER       = 0x0800
Q_PLAYER_OUTLAW            = 0x1000
Q_PLAYER_NOTOUTLAW         = 0x2000
Q_PLAYER_LAWFUL            = 0x2800   // not murderer or outlaw
Q_PLAYER_KILL_MASK         = 0x3F00
```

**Quest-History-Einschränkungen:**
```c
Q_PLAYER_NOTSUCCEEDED_RECENTLY   = 0x04000
Q_PLAYER_NOTSUCCEEDED            = 0x08000
Q_PLAYER_NOTFAILED_RECENTLY      = 0x10000
Q_PLAYER_NOTFAILED               = 0x20000
Q_PLAYER_NOTTRIED_RECENTLY       = 0x14000
Q_PLAYER_NOTTRIED                = 0x28000
Q_PLAYER_QUEST_MASK              = 0x3C000
```

**Sonstige:**
```c
Q_PLAYER_INTRIGUING           = 0x40000   // Can join factions
Q_PLAYER_GUILDMASTER          = 0x80000   // Is guildmaster
Q_PLAYER_PVP_ENABLED          = 0x100000  // Has PVP enabled
```

**Editor sollte diese als Checkboxen anbieten!**

### Quest Node NPC Modifiers (Zeilen 3942-3947)

```c
// Quest node NPC selection modifiers
QN_NPCMOD_NONE          = 0x0   // Choose any NPC in the list
QN_NPCMOD_SAME          = 0x1   // Use current source NPC (return)
QN_NPCMOD_PREVIOUS      = 0x2   // Use previous source NPC
QN_NPCMOD_DIFFERENT     = 0x3   // Use different NPC than current
QN_NPCMOD_PASSED        = 0x4   // NPC will be passed from another node
```

**Editor sollte Dropdown mit diesen Optionen haben!**

### Quest Node Prize Types (Zeilen 3885-3907)

```c
// Quest node prize types
QN_PRIZETYPE_ITEM                      = 0x01   // Specific item's object ID
QN_PRIZETYPE_ITEMCLASS                 = 0x02   // New item of this class
QN_PRIZETYPE_STATISTIC                 = 0x03   // Modifier to statistic
QN_PRIZETYPE_SKILL                     = 0x04   // Modifier to skill
QN_PRIZETYPE_SPELL                     = 0x05   // Modifier to spell
QN_PRIZETYPE_BOON                      = 0x06   // Temporary stat boost
QN_PRIZETYPE_FACTION                   = 0x07   // Change faction status
QN_PRIZETYPE_ACTIVATE_QUEST            = 0x08   // Trigger another quest
QN_PRIZETYPE_PASS_QUEST_RESTRICT2      = 0x09   // Change quest restrictions
QN_PRIZETYPE_PASS_CARGO                = 0x0A   // Set quest cargo (NYI)
QN_PRIZETYPE_PASS_NPC                  = 0x0B   // Set dest NPC
QN_PRIZETYPE_PASS_PLAYER               = 0x0C   // Set quest cargo (NYI)
QN_PRIZETYPE_INSIGNIA                  = 0x0D   // Set guildshield insignia
QN_PRIZETYPE_PASS_SOURCE_NPC           = 0x0E   // Set dest NPC -> SourceNPC
QN_PRIZETYPE_PASS_PREVIOUS_SOURCE_NPC  = 0x0F   // Set dest NPC -> PrevSourceNPC
QN_PRIZETYPE_SCHEDULE_QUEST            = 0x10   // Schedule quest
QN_PRIZETYPE_ESTABLISH_NECROGUILD      = 0x11   // Form necromancer guild
QN_PRIZETYPE_OUTLAW                    = 0x12   // Make player outlaw
QN_PRIZETYPE_NPC_RESPONSE              = 0x13   // Template gives responses
QN_PRIZETYPE_TRAINING_POINTS           = 0x14   // Award training points
QN_PRIZETYPE_EXPERIENCE_POINTS         = 0x15   // Award experience/gain chance
QN_PRIZETYPE_NO_PVP_FLAG               = 0x16   // Switch PVP off
```

**Editor sollte ALLE Prize-Typen unterstützen!**

### Prize Sub-Types

**Statistik-Typen (Zeilen 3909-3921):**
```c
QN_PRIZE_STAT_MIGHT         = 0x0
QN_PRIZE_STAT_INTELLECT     = 0x1
QN_PRIZE_STAT_AIM           = 0x2
QN_PRIZE_STAT_STAMINA       = 0x3
QN_PRIZE_STAT_AGILITY       = 0x4
QN_PRIZE_STAT_MYSTICISM     = 0x5
QN_PRIZE_STAT_HEALTH        = 0x6
QN_PRIZE_STAT_MAXHEALTH     = 0x7
QN_PRIZE_STAT_BASEMAXHEALTH = 0x8
QN_PRIZE_STAT_MANA          = 0x9
QN_PRIZE_STAT_KARMA         = 0xA
```

**Boon-Typen (Zeilen 3922-3934):**
```c
QN_PRIZE_BOON_VIGOR         = 0x0
QN_PRIZE_BOON_INTELLECT     = 0x1
QN_PRIZE_BOON_AIM           = 0x2
QN_PRIZE_BOON_STAMINA       = 0x3
QN_PRIZE_BOON_AGILITY       = 0x4
QN_PRIZE_BOON_MYSTICISM     = 0x5
// QN_PRIZE_BOON_HEALTH     = 0x6   // NYI
QN_PRIZE_BOON_STRENGTH      = 0x7
QN_PRIZE_BOON_HITPOINTS     = 0x8
QN_PRIZE_BOON_MANA          = 0x9
// QN_PRIZE_BOON_KARMA      = 0xA   // NYI
```

**Faction-Typen (Zeilen 3935-3941):**
```c
QN_PRIZE_FACTION_UPDATE     = 0x0   // Reset faction timer
QN_PRIZE_FACTION_DUKE       = 0x1   // Join duke
QN_PRIZE_FACTION_PRINCESS   = 0x2   // Join princess
QN_PRIZE_FACTION_REBEL      = 0x3   // Join rebels
QN_PRIZE_FACTION_NEUTRAL    = 0x4   // Mainly for penalty
```

---

## Schlussfolgerungen für den Editor

### 1. QNT_ID Einfüge-Position (KRITISCH!)

**Problem bestätigt:**

Die QNT_IDs werden an der **falschen Position** eingefügt.

**Korrekte Position:**
- **Nach Zeile 3466:** `// Quest Node Template IDs. Each quest has multple Quest Nodes.`
- **NICHT** in der QST_ID-Section (Zeilen 3380-3464)

**Fix für BlakstonKhdService.cs:**

```csharp
private int FindNodeIdInsertionPoint(List<string> lines)
{
    // 1. Nach letzter existierender QNT_ID
    for (int i = lines.Count - 1; i >= 0; i--)
    {
        if (Regex.IsMatch(lines[i].Trim(), @"QNT_ID_\w+\s*=\s*\d+"))
        {
            return i + 1;
        }
    }

    // 2. Suche Header "Quest Node Template IDs"
    for (int i = 0; i < lines.Count; i++)
    {
        if (lines[i].Contains("Quest Node Template IDs"))
        {
            // Finde erste nicht-leere Zeile nach Header
            for (int j = i + 1; j < lines.Count; j++)
            {
                if (!string.IsNullOrWhiteSpace(lines[j]))
                {
                    return j; // Füge VOR erster Definition ein
                }
            }
            return i + 2; // Falls keine Definitionen folgen
        }
    }

    // 3. Fallback: Erstelle Section nach letzter QST_ID
    int lastQstLine = FindLastQuestIdLine(lines);
    if (lastQstLine >= 0)
    {
        lines.Insert(lastQstLine + 1, "");
        lines.Insert(lastQstLine + 2, "");
        lines.Insert(lastQstLine + 3, "   // Quest Node Template IDs. Each quest has multple Quest Nodes.");
        return lastQstLine + 4;
    }

    throw new InvalidOperationException("Could not find insertion point for QNT_IDs");
}
```

### 2. Node-Namenskonvention

**Aktuell (Editor):**
```
QNT_ID_MYQUEST_1 = 247
QNT_ID_MYQUEST_2 = 248
```

**Sollte sein (Standard):**
```
QNT_ID_MYQUEST_ONE   = 247
QNT_ID_MYQUEST_TWO   = 248
QNT_ID_MYQUEST_THREE = 249
```

**Fix für KodFileService.cs:**

```csharp
private string NumberToWord(int num)
{
    string[] words = {
        "ZERO", "ONE", "TWO", "THREE", "FOUR", "FIVE",
        "SIX", "SEVEN", "EIGHT", "NINE", "TEN",
        "ELEVEN", "TWELVE", "THIRTEEN", "FOURTEEN", "FIFTEEN"
    };

    return num < words.Length ? words[num] : num.ToString();
}

// Verwendung:
var nodeName = $"QNT_ID_{questNameUpper}_{NumberToWord(nodeIndex).ToUpper()}";
```

### 3. Formatting/Spacing

**QST_ID Format:**
```c
QST_ID_QUESTNAME.PadRight(30) = ID
   ^^^                    ^^^
   3 Spaces               Padding bis Spalte 30
```

**QNT_ID Format:**
```c
QNT_ID_QUESTNAME_NODENAME.PadRight(30) = ID
   ^^^                          ^^^
   3 Spaces                     Padding bis Spalte 30
```

**Fix:**
```csharp
// QST_ID
var questIdLine = $"   QST_ID_{questNameUpper.PadRight(25)} = {questId}";

// QNT_ID
var nodeIdLine = $"   QNT_ID_{nodeName.PadRight(25)} = {nodeId}";
```

### 4. ID-Vergabe

**Nächste verfügbare IDs (Stand: blakston.khd):**
- **QST_ID:** 83 (letzte: 82)
- **QNT_ID:** 247 (letzte: 246)

**Editor muss:**
- IDs aus blakston.khd lesen
- Maximum finden
- Neue IDs = Maximum + 1

**NICHT:** Hash-basierte IDs verwenden!

```csharp
// FALSCH:
quest.QuestTemplateId = quest.QuestKodClass.GetHashCode();

// RICHTIG:
var khdService = new BlakstonKhdService(serverPath);
quest.QuestTemplateId = khdService.FindNextQuestId();
```

### 5. Gruppierung der QNT_IDs

**Standard in blakston.khd:**

```c
QNT_ID_CHICKEN_SOUP_ONE       = 49
QNT_ID_CHICKEN_SOUP_TWO       = 50
QNT_ID_CHICKEN_SOUP_THREE     = 51
                                        <- Leerzeile zwischen Quests
QNT_ID_STUNTED_DWARF_ONE      = 52
QNT_ID_STUNTED_DWARF_TWO      = 53
```

**Editor sollte:**
- Alle QNT_IDs einer Quest zusammen einfügen
- Leerzeile danach einfügen (für Lesbarkeit)

```csharp
public void AddQuestConstants(string questName, int nodeCount)
{
    // ... (QST_ID einfügen)

    // QNT_IDs einfügen
    int insertLine = FindNodeIdInsertionPoint(lines);

    for (int i = nodeCount; i >= 1; i--)
    {
        var nodeName = $"{questNameUpper}_{NumberToWord(i).ToUpper()}";
        var nodeId = nextNodeId + (i - 1);
        var nodeIdLine = $"   QNT_ID_{nodeName.PadRight(25)} = {nodeId}";
        lines.Insert(insertLine, nodeIdLine);
    }

    // Leerzeile nach Quest-Nodes einfügen
    lines.Insert(insertLine + nodeCount, "");
}
```

### 6. Player Restrictions - GUI-Verbesserung

**Editor sollte Checkboxen/Dropdowns bieten für:**

**Karma:**
- ( ) Same as NPC
- ( ) Different from NPC
- ( ) Good
- ( ) Neutral
- ( ) Evil

**Faction:**
- ( ) Duke
- ( ) Neutral
- ( ) Princess
- ( ) Rebel
- ( ) Same as NPC
- ( ) Different from NPC

**Status:**
- [ ] Newbie only
- [ ] Not Newbie
- [ ] Murderer
- [ ] Not Murderer
- [ ] Outlaw
- [ ] Not Outlaw
- [ ] Lawful (not murderer/outlaw)

**Quest History:**
- [ ] Not succeeded recently
- [ ] Not succeeded ever
- [ ] Not failed recently
- [ ] Not failed ever
- [ ] Not tried recently
- [ ] Not tried ever

**Sonstige:**
- [ ] Can join factions (Intriguing)
- [ ] Is Guildmaster
- [ ] Has PVP enabled

**Implementierung:**
```csharp
// Combine flags mit bitwise OR
var restrictions = 0;
if (chkNewbie.IsChecked)
    restrictions |= 0x0100;  // Q_PLAYER_NEWBIE
if (chkLawful.IsChecked)
    restrictions |= 0x2800;  // Q_PLAYER_LAWFUL

quest.PlayerRestrictions = restrictions;
```

### 7. Quest Node Types - Dropdown

**Editor sollte Dropdown mit diesen Optionen haben:**

```
- Show Up (QN_TYPE_SHOWUP)
- Deliver Message (QN_TYPE_MESSAGE)
- Deliver Specific Item (QN_TYPE_ITEM)
- Deliver Item of Class (QN_TYPE_ITEMCLASS)
- Find & Deliver Item (QN_TYPE_ITEMFINDCLASS)
- Kill Monster (QN_TYPE_MONSTER)
- Kill Monster & Deliver Item (QN_TYPE_MONSTER_ITEMCLASS)
- Bring Charmed Monster (QN_TYPE_MONSTER_BRING)
- Chess Move (QN_TYPE_CHESSMOVE) [Special]
```

### 8. Prize Types - Erweiterte Unterstützung

**Editor sollte ALLE Prize-Typen unterstützen:**

**Basis-Typen:**
- Item (specific object)
- Item Class (new item of class)
- Statistic (with sub-dropdown: Might, Intellect, etc.)
- Skill (with skill selection)
- Spell (with spell selection)
- Boon (with sub-dropdown: Vigor, Intellect, etc.)
- Faction (with sub-dropdown: Duke, Princess, Rebel, Neutral)
- Outlaw (penalty)
- Training Points
- Experience Points
- Disable PVP

**Advanced-Typen (für Quest-Chaining):**
- Activate Quest
- Schedule Quest
- Pass Cargo
- Pass NPC
- Pass Source NPC
- Pass Previous Source NPC

### 9. Monster-Listen

**Editor muss unterstützen:**

```c
QNT_MONSTER_LIST  = 6   // Monster-Liste für QN_TYPE_MONSTER
```

**Ähnlich wie Cargo-List:**
```csharp
public class Monster
{
    public string MonsterClass { get; set; }  // &Orc, &Troll, etc.
    public int Quantity { get; set; }         // Anzahl zu töten
}
```

### 10. Penalty-Listen

**Editor muss unterstützen:**

```c
QNT_PENALTY_LIST  = 8   // Penalty-Liste
```

**Beispiel:**
```kod
#penaltylist=[ [ QN_PRIZETYPE_OUTLAW ] ]
```

---

## Checkliste: Editor-Verbesserungen

### Kritisch (muss behoben werden)

- [ ] **QNT_ID Einfüge-Position korrigieren** (BEKANNTES-PROBLEM-QNT-ID.md)
- [ ] **Node-Namen zu Wörtern konvertieren** (ONE statt 1)
- [ ] **Korrekte ID-Vergabe** (aus blakston.khd lesen, nicht Hash)
- [ ] **Spacing/Padding korrekt** (PadRight(25))

### Wichtig (sollte implementiert werden)

- [ ] **Player Restrictions GUI** (Checkboxen/Dropdowns)
- [ ] **Quest Node Types Dropdown** (alle Typen)
- [ ] **Prize Types erweitert** (alle 22 Typen)
- [ ] **Monster-Listen Support**
- [ ] **Penalty-Listen Support**
- [ ] **QNT_IDs gruppieren** (mit Leerzeile zwischen Quests)

### Optional (Nice-to-Have)

- [ ] **Quest-Validierung** (prüfe auf fehlende Dialogs/Cargo/etc.)
- [ ] **Preview der generierten .kod-Datei**
- [ ] **Preview des blakston.khd-Eintrags**
- [ ] **Import bestehender Quests** (aus .kod-Datei)

---

## Zusammenfassung

Die Analyse von blakston.khd zeigt:

1. **Klare Struktur:** QST_IDs und QNT_IDs sind **strikt getrennt**
2. **Namenskonventionen:** Wörter statt Zahlen (ONE, TWO, THREE)
3. **Umfangreiche Konstanten:** 22 Prize-Typen, 12 Player-Restrictions, etc.
4. **Editor-Lücken:** Viele Features werden noch nicht unterstützt

**Nächste Schritte:**
1. Fix QNT_ID Einfügung (kritisch!)
2. Erweitere GUI für Player Restrictions
3. Füge fehlende Prize/Node-Typen hinzu
4. Implementiere Monster/Penalty-Listen

---

**Ende der blakston.khd Analyse**
