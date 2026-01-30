# Gap-Analysis: QuestEditor vs. blakston.khd

**Datum:** 2026-01-04
**Analysiert:** QuestEditorViewModel.cs, QuestModels.cs vs. blakston.khd
**Status:** ⚠️ ERHEBLICHE LÜCKEN GEFUNDEN

---

## Executive Summary

Der QuestEditor unterstützt **nur einen Bruchteil** der in blakston.khd definierten Konstanten:

| Kategorie | In blakston.khd | Im Editor | Abdeckung | Status |
|-----------|-----------------|-----------|-----------|--------|
| Quest Node Types | 11 | 5 | 45% | ⚠️ Unvollständig |
| NPC Modifiers | 5 | 0 | 0% | ❌ Fehlt komplett |
| Prize Types | 22 | 13 | 59% | ⚠️ Unvollständig |
| Penalty Types | ? | 0 | 0% | ❌ Fehlt komplett |
| Player Restrictions | 20+ | 7 | 35% | ❌ Kritisch |
| Boon Types | 11 | 0 | 0% | ❌ Fehlt komplett |
| Faction Types | 5 | 1 | 20% | ❌ Fehlt komplett |
| Statistics Types | 11 | 0 | 0% | ❌ Fehlt komplett |

**Gesamtabdeckung: ~30%** ❌

---

## 1. Quest Node Types (QN_TYPE_*)

### In blakston.khd (Zeilen 3864-3876)

```c
QN_TYPE_MESSAGE            = 0x01   // Deliver a message I give you
QN_TYPE_ITEM               = 0x02   // Deliver a specific item I give you
QN_TYPE_ITEMCLASS          = 0x03   // Deliver newly created item of class
QN_TYPE_ITEMFINDCLASS      = 0x04   // Deliver any item of class (find it)
QN_TYPE_SHOWUP             = 0x05   // Just show your face
QN_TYPE_CHESSMOVE          = 0x06   // Deliver chess move
QN_TYPE_USERNAME           = 0x07   // Deliver username (NYI)
QN_TYPE_LOGGEDONNAME       = 0x08   // Deliver logged-on username (NYI)
QN_TYPE_MONSTER            = 0x09   // Kill a monster of type
QN_TYPE_MONSTER_ITEMCLASS  = 0x10   // Kill monster, deliver item
QN_TYPE_MONSTER_BRING      = 0x0A   // Bring charmed monster
```

**Total: 11 Typen**

### Im Editor (QuestEditorViewModel.cs Zeile 191-198)

```csharp
private ObservableCollection<string> _nodeTypes = new()
{
    "1", // MESSAGE
    "2", // ITEM
    "3", // ITEMCLASS
    "5", // SHOWUP
    "9"  // MONSTER
};
```

**Total: 5 Typen**

### ❌ Fehlende Node Types

| Typ | Hex | Dezimal | Beschreibung | Priorität |
|-----|-----|---------|--------------|-----------|
| QN_TYPE_ITEMFINDCLASS | 0x04 | 4 | Find any item of class | HOCH |
| QN_TYPE_CHESSMOVE | 0x06 | 6 | Chess move (special) | NIEDRIG |
| QN_TYPE_USERNAME | 0x07 | 7 | Username delivery (NYI) | NIEDRIG |
| QN_TYPE_LOGGEDONNAME | 0x08 | 8 | Logged-on name (NYI) | NIEDRIG |
| QN_TYPE_MONSTER_ITEMCLASS | 0x10 | 16 | Kill monster + deliver item | **KRITISCH** |
| QN_TYPE_MONSTER_BRING | 0x0A | 10 | Bring charmed monster | MITTEL |

### 🔧 Fix Empfehlung

```csharp
private ObservableCollection<string> _nodeTypes = new()
{
    "1",  // QN_TYPE_MESSAGE
    "2",  // QN_TYPE_ITEM
    "3",  // QN_TYPE_ITEMCLASS
    "4",  // QN_TYPE_ITEMFINDCLASS  // NEU!
    "5",  // QN_TYPE_SHOWUP
    "9",  // QN_TYPE_MONSTER
    "10", // QN_TYPE_MONSTER_ITEMCLASS  // NEU! KRITISCH
    "10"  // QN_TYPE_MONSTER_BRING  // NEU!
    // 6, 7, 8 sind speziell/NYI, können weggelassen werden
};
```

---

## 2. NPC Modifiers (QN_NPCMOD_*)

### In blakston.khd (Zeilen 3942-3947)

```c
QN_NPCMOD_NONE          = 0x0   // Choose any NPC in the list
QN_NPCMOD_SAME          = 0x1   // Use current source NPC (return)
QN_NPCMOD_PREVIOUS      = 0x2   // Use previous source NPC
QN_NPCMOD_DIFFERENT     = 0x3   // Use different NPC than current
QN_NPCMOD_PASSED        = 0x4   // NPC will be passed from another node
```

**Total: 5 Typen**

### Im Editor (QuestEditorViewModel.cs)

```csharp
// NICHT DEFINIERT!
// NpcModifier wird in QuestNode.NpcModifier gespeichert, aber:
// - Keine Dropdown-Optionen
// - Keine Konstanten
// - Kein UI-Element in XAML
```

**Total: 0 Typen** ❌

### ❌ KOMPLETT FEHLEND

**Problem:** NPC Modifiers sind **kritisch** für Quest-Ketten!

**Beispiel aus chickensoupqt.kod:**
```kod
// Node 3: Return to SAME NPC (not different!)
if Send(oQE, @AddQuestNodeTemplate,
        #questnode_type=QN_TYPE_ITEM,
        #NPC_modifier=QN_NPCMOD_PREVIOUS,  // ← WIRD NICHT UNTERSTÜTZT!
        ...)
```

### 🔧 Fix Empfehlung (KRITISCH!)

**1. ViewModel ergänzen:**
```csharp
[ObservableProperty]
private ObservableCollection<string> _npcModifiers = new()
{
    "0",  // QN_NPCMOD_NONE - Any NPC
    "1",  // QN_NPCMOD_SAME - Return to same NPC
    "2",  // QN_NPCMOD_PREVIOUS - Go to previous NPC
    "3",  // QN_NPCMOD_DIFFERENT - Go to different NPC
    "4"   // QN_NPCMOD_PASSED - NPC passed from another node
};
```

**2. XAML ergänzen (QuestEditorWindow.xaml Zeile 265):**

**Aktuell:**
```xml
<ComboBox SelectedValue="{Binding SelectedNode.NpcModifier, Mode=TwoWay}">
    <!-- KEINE ITEMS DEFINIERT! -->
</ComboBox>
```

**Sollte sein:**
```xml
<ComboBox ItemsSource="{Binding NpcModifiers}"
          SelectedItem="{Binding SelectedNode.NpcModifier, Mode=TwoWay}">
    <ComboBox.ItemTemplate>
        <DataTemplate>
            <StackPanel Orientation="Horizontal">
                <TextBlock Text="{Binding}" Width="30"/>
                <TextBlock>
                    <TextBlock.Style>
                        <Style TargetType="TextBlock">
                            <Style.Triggers>
                                <DataTrigger Binding="{Binding}" Value="0">
                                    <Setter Property="Text" Value="- Any NPC from list"/>
                                </DataTrigger>
                                <DataTrigger Binding="{Binding}" Value="1">
                                    <Setter Property="Text" Value="- Return to same NPC"/>
                                </DataTrigger>
                                <DataTrigger Binding="{Binding}" Value="2">
                                    <Setter Property="Text" Value="- Go to previous NPC"/>
                                </DataTrigger>
                                <DataTrigger Binding="{Binding}" Value="3">
                                    <Setter Property="Text" Value="- Go to different NPC"/>
                                </DataTrigger>
                                <DataTrigger Binding="{Binding}" Value="4">
                                    <Setter Property="Text" Value="- NPC from another node"/>
                                </DataTrigger>
                            </Style.Triggers>
                        </Style>
                    </TextBlock.Style>
                </TextBlock>
            </StackPanel>
        </DataTemplate>
    </ComboBox.ItemTemplate>
</ComboBox>
```

---

## 3. Prize Types (QN_PRIZETYPE_*)

### In blakston.khd (Zeilen 3885-3907)

```c
QN_PRIZETYPE_ITEM                      = 0x01   // Specific item's object ID
QN_PRIZETYPE_ITEMCLASS                 = 0x02   // New item of this class
QN_PRIZETYPE_STATISTIC                 = 0x03   // Modifier to a statistic
QN_PRIZETYPE_SKILL                     = 0x04   // Modifier to a skill
QN_PRIZETYPE_SPELL                     = 0x05   // Modifier to a spell
QN_PRIZETYPE_BOON                      = 0x06   // Temporary stat boost
QN_PRIZETYPE_FACTION                   = 0x07   // Change in faction status
QN_PRIZETYPE_ACTIVATE_QUEST            = 0x08   // trigger another quest
QN_PRIZETYPE_PASS_QUEST_RESTRICT2      = 0x09   // Change in quest special restrictions
QN_PRIZETYPE_PASS_CARGO                = 0x0A   // Set quest cargo (NYI)
QN_PRIZETYPE_PASS_NPC                  = 0x0B   // Set dest npc
QN_PRIZETYPE_PASS_PLAYER               = 0x0C   // Set quest cargo (NYI)
QN_PRIZETYPE_INSIGNIA                  = 0x0D   // set guildshield insignia
QN_PRIZETYPE_PASS_SOURCE_NPC           = 0x0E   // Set dest npc -> SourceNPC
QN_PRIZETYPE_PASS_PREVIOUS_SOURCE_NPC  = 0x0F   // Set dest npc -> PrevSourceNPC
QN_PRIZETYPE_SCHEDULE_QUEST            = 0x10   // Schedule a quest
QN_PRIZETYPE_ESTABLISH_NECROGUILD      = 0x11   // Form necromancer guild
QN_PRIZETYPE_OUTLAW                    = 0x12   // Make player outlaw
QN_PRIZETYPE_NPC_RESPONSE              = 0x13   // NPC response
QN_PRIZETYPE_TRAINING_POINTS           = 0x14   // Award training points
QN_PRIZETYPE_EXPERIENCE_POINTS         = 0x15   // Award experience
QN_PRIZETYPE_NO_PVP_FLAG               = 0x16   // Switch PVP off
```

**Total: 22 Typen**

### Im Editor (QuestEditorViewModel.cs Zeile 142-157)

```csharp
private ObservableCollection<string> _rewardTypes = new()
{
    "ITEMCLASS",              // Give item (needs RewardClass & Quantity)
    "EXPERIENCE_POINTS",      // Give XP (needs Value or Quantity)
    "TRAINING_POINTS",        // Give HP/Skills (needs Value or Quantity)
    "FACTION",                // Faction points (needs Value)
    "MONEY",                  // Give shillings (needs Value)
    "OUTLAW",                 // Make player outlaw
    "SCHEDULE_QUEST",         // Schedule another quest (needs RewardClass = quest name)
    "INSIGNIA",               // Give insignia (needs Value)
    "NPC_RESPONSE",           // NPC response (needs RewardClass)
    "ACTIVATE_QUEST",         // Activate quest (needs RewardClass = quest name)
    "PASS_NPC",               // Pass NPC (needs RewardClass = NPC name)
    "ESTABLISH_NECROGUILD",   // Establish necromancer guild
    "NO_PVP_FLAG"             // Give no-PVP flag
};
```

**Total: 13 Typen**

### ❌ Fehlende Prize Types

| Hex | Typ | Beschreibung | Priorität |
|-----|-----|--------------|-----------|
| 0x01 | ITEM | Specific item by object ID | MITTEL |
| 0x03 | STATISTIC | Modify player statistic | **KRITISCH** |
| 0x04 | SKILL | Modify player skill | **KRITISCH** |
| 0x05 | SPELL | Modify player spell | **KRITISCH** |
| 0x06 | BOON | Temporary stat boost | **KRITISCH** |
| 0x09 | PASS_QUEST_RESTRICT2 | Change quest restrictions | NIEDRIG |
| 0x0A | PASS_CARGO | Pass cargo (NYI) | NIEDRIG |
| 0x0C | PASS_PLAYER | Pass player (NYI) | NIEDRIG |
| 0x0E | PASS_SOURCE_NPC | Pass source NPC | MITTEL |
| 0x0F | PASS_PREVIOUS_SOURCE_NPC | Pass previous source NPC | MITTEL |

**KRITISCH:** STATISTIC, SKILL, SPELL, BOON fehlen!

### 🔧 Fix Empfehlung

```csharp
private ObservableCollection<string> _rewardTypes = new()
{
    "ITEM",                   // 0x01 - Specific item by object ID  // NEU!
    "ITEMCLASS",              // 0x02 - Item by class
    "STATISTIC",              // 0x03 - Modify statistic  // NEU! KRITISCH
    "SKILL",                  // 0x04 - Modify skill  // NEU! KRITISCH
    "SPELL",                  // 0x05 - Modify spell  // NEU! KRITISCH
    "BOON",                   // 0x06 - Temporary stat boost  // NEU! KRITISCH
    "FACTION",                // 0x07 - Change faction
    "ACTIVATE_QUEST",         // 0x08 - Trigger another quest
    // 0x09 - NYI
    // 0x0A - NYI
    "PASS_NPC",               // 0x0B - Pass NPC
    // 0x0C - NYI
    "INSIGNIA",               // 0x0D - Guild insignia
    "PASS_SOURCE_NPC",        // 0x0E - Pass source NPC  // NEU!
    "PASS_PREVIOUS_SOURCE_NPC", // 0x0F - Pass prev source NPC  // NEU!
    "SCHEDULE_QUEST",         // 0x10 - Schedule quest
    "ESTABLISH_NECROGUILD",   // 0x11 - Establish necro guild
    "OUTLAW",                 // 0x12 - Make outlaw
    "NPC_RESPONSE",           // 0x13 - NPC response
    "TRAINING_POINTS",        // 0x14 - Training points
    "EXPERIENCE_POINTS",      // 0x15 - Experience points
    "NO_PVP_FLAG"             // 0x16 - Disable PVP
};
```

**Zusätzlich benötigt:** Sub-Typen für STATISTIC, SKILL, BOON, FACTION!

---

## 4. Prize Sub-Types

### 4.1 Statistic Types (QN_PRIZE_STAT_*)

**In blakston.khd (Zeilen 3909-3921):**
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

**Im Editor:** ❌ FEHLT KOMPLETT

**Benötigt für:** STATISTIC Prize Type

**Fix:**
```csharp
[ObservableProperty]
private ObservableCollection<string> _statisticTypes = new()
{
    "0",  // MIGHT
    "1",  // INTELLECT
    "2",  // AIM
    "3",  // STAMINA
    "4",  // AGILITY
    "5",  // MYSTICISM
    "6",  // HEALTH
    "7",  // MAXHEALTH
    "8",  // BASEMAXHEALTH
    "9",  // MANA
    "10"  // KARMA
};
```

### 4.2 Boon Types (QN_PRIZE_BOON_*)

**In blakston.khd (Zeilen 3922-3934):**
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

**Im Editor:** ❌ FEHLT KOMPLETT

**Benötigt für:** BOON Prize Type (Temporary Buffs!)

**Fix:**
```csharp
[ObservableProperty]
private ObservableCollection<string> _boonTypes = new()
{
    "0",  // VIGOR
    "1",  // INTELLECT
    "2",  // AIM
    "3",  // STAMINA
    "4",  // AGILITY
    "5",  // MYSTICISM
    "7",  // STRENGTH
    "8",  // HITPOINTS
    "9"   // MANA
    // 6, 10 sind NYI
};
```

### 4.3 Faction Types (QN_PRIZE_FACTION_*)

**In blakston.khd (Zeilen 3935-3941):**
```c
QN_PRIZE_FACTION_UPDATE     = 0x0   // Reset faction timer
QN_PRIZE_FACTION_DUKE       = 0x1   // Join duke
QN_PRIZE_FACTION_PRINCESS   = 0x2   // Join princess
QN_PRIZE_FACTION_REBEL      = 0x3   // Join rebels
QN_PRIZE_FACTION_NEUTRAL    = 0x4   // Mainly for penalty
```

**Im Editor:**
```csharp
"FACTION",  // Nur generisch!
```

**Fehlt:** Spezifische Faction-Auswahl!

**Fix:**
```csharp
[ObservableProperty]
private ObservableCollection<string> _factionTypes = new()
{
    "0",  // UPDATE - Reset timer
    "1",  // DUKE - Join Duke's faction
    "2",  // PRINCESS - Join Princess' faction
    "3",  // REBEL - Join Rebels
    "4"   // NEUTRAL - Become neutral (penalty)
};
```

---

## 5. Player Restrictions (Q_PLAYER_*)

### In blakston.khd (Zeilen 3802-3842)

**Karma (0xF Mask):**
```c
Q_PLAYER_KARMA_SAME         = 0x1
Q_PLAYER_KARMA_DIFFERENT    = 0x2
Q_PLAYER_KARMA_GOOD         = 0x3
Q_PLAYER_KARMA_NEUTRAL      = 0x4
Q_PLAYER_KARMA_EVIL         = 0x5
```

**Faction (0xF0 Mask):**
```c
Q_PLAYER_FACTION_DUKE       = 0x10
Q_PLAYER_FACTION_NEUTRAL    = 0x20
Q_PLAYER_FACTION_PRINCESS   = 0x30
Q_PLAYER_FACTION_REBEL      = 0x40
Q_PLAYER_FACTION_SAME       = 0x50
Q_PLAYER_FACTION_DIFFERENT  = 0x60
Q_PLAYER_FACTION_IN         = 0x70  // NYI
Q_PLAYER_FACTION_OUT        = 0x80  // NYI
```

**Status (0x3F00 Mask):**
```c
Q_PLAYER_NEWBIE             = 0x0100
Q_PLAYER_NOTNEWBIE          = 0x0200
Q_PLAYER_MURDERER           = 0x0400
Q_PLAYER_NOTMURDERER        = 0x0800
Q_PLAYER_OUTLAW             = 0x1000
Q_PLAYER_NOTOUTLAW          = 0x2000
Q_PLAYER_LAWFUL             = 0x2800  // not murderer or outlaw
```

**Quest History (0x3C000 Mask):**
```c
Q_PLAYER_NOTSUCCEEDED_RECENTLY   = 0x04000
Q_PLAYER_NOTSUCCEEDED            = 0x08000
Q_PLAYER_NOTFAILED_RECENTLY      = 0x10000
Q_PLAYER_NOTFAILED               = 0x20000
Q_PLAYER_NOTTRIED_RECENTLY       = 0x14000
Q_PLAYER_NOTTRIED                = 0x28000
```

**Sonstige:**
```c
Q_PLAYER_INTRIGUING         = 0x40000   // Can join factions
Q_PLAYER_GUILDMASTER        = 0x80000   // Is guildmaster
Q_PLAYER_PVP_ENABLED        = 0x100000  // Has PVP enabled
```

**Total: 20+ Restrictions**

### Im Editor (QuestEditorViewModel.cs Zeile 185-188)

```csharp
private ObservableCollection<string> _restrictionTypes = new()
{
    "min_level", "max_level", "min_faction", "class", "race", "guild", "stat_requirement"
};
```

**Total: 7 Restrictions (aber FALSCH!)**

### ❌ KRITISCHES PROBLEM

**Editor verwendet eigene, inkompatible Restriction-Namen!**

- `"min_level"` ≠ Blakod (Blakod nutzt keine Level-Restrictions!)
- `"max_level"` ≠ Blakod
- `"class"` ≠ Blakod (Blakod hat keine Class-Restrictions!)
- `"race"` ≠ Blakod (Blakod hat keine Race-Restrictions!)

**Korrekte Restrictions sind FLAGS, keine Strings!**

### 🔧 Fix Empfehlung (KRITISCH!)

**Problem:** Restrictions sind **bitwise OR-kombinierbare Flags**, keine einfachen Strings!

**Beispiel aus chickensoupqt.kod:**
```kod
piPlayerRestrict = Q_PLAYER_NOTTRIED_RECENTLY | Q_PLAYER_NEWBIE
//                 ^^^^^^^^^^^^^^^^^^^^^^^^^ | ^^^^^^^^^^^^^^^
//                 0x14000                  | 0x0100
//                 = 0x14100 (kombiniert!)
```

**Richtige Implementierung:**

**1. Model ändern (QuestModels.cs):**
```csharp
public class Quest
{
    // FALSCH: Alte Implementierung
    // public ObservableCollection<QuestRestriction> Restrictions { get; set; }

    // RICHTIG: Bitwise Flags
    public int PlayerRestrictions { get; set; } = 0;  // Kombinierte Flags
}
```

**2. ViewModel mit Checkboxen (QuestEditorViewModel.cs):**
```csharp
// Karma Restrictions (nur EINE kann gewählt werden)
[ObservableProperty]
private bool _restrictKarmaSame;

[ObservableProperty]
private bool _restrictKarmaDifferent;

[ObservableProperty]
private bool _restrictKarmaGood;

[ObservableProperty]
private bool _restrictKarmaNeutral;

[ObservableProperty]
private bool _restrictKarmaEvil;

// Faction Restrictions (nur EINE kann gewählt werden)
[ObservableProperty]
private bool _restrictFactionDuke;

[ObservableProperty]
private bool _restrictFactionNeutral;

[ObservableProperty]
private bool _restrictFactionPrincess;

[ObservableProperty]
private bool _restrictFactionRebel;

// Status Restrictions (MEHRERE können gewählt werden)
[ObservableProperty]
private bool _restrictNewbie;

[ObservableProperty]
private bool _restrictNotNewbie;

[ObservableProperty]
private bool _restrictMurderer;

[ObservableProperty]
private bool _restrictNotMurderer;

[ObservableProperty]
private bool _restrictOutlaw;

[ObservableProperty]
private bool _restrictNotOutlaw;

[ObservableProperty]
private bool _restrictLawful;

// Quest History (MEHRERE können gewählt werden)
[ObservableProperty]
private bool _restrictNotTriedRecently;

[ObservableProperty]
private bool _restrictNotTried;

[ObservableProperty]
private bool _restrictNotSucceededRecently;

[ObservableProperty]
private bool _restrictNotSucceeded;

[ObservableProperty]
private bool _restrictNotFailedRecently;

[ObservableProperty]
private bool _restrictNotFailed;

// Sonstige
[ObservableProperty]
private bool _restrictIntriguing;

[ObservableProperty]
private bool _restrictGuildmaster;

[ObservableProperty]
private bool _restrictPvpEnabled;

// Methode zum Kombinieren der Flags
public int GetPlayerRestrictions()
{
    int restrictions = 0;

    // Karma (nur eine)
    if (RestrictKarmaSame) restrictions |= 0x1;
    else if (RestrictKarmaDifferent) restrictions |= 0x2;
    else if (RestrictKarmaGood) restrictions |= 0x3;
    else if (RestrictKarmaNeutral) restrictions |= 0x4;
    else if (RestrictKarmaEvil) restrictions |= 0x5;

    // Faction (nur eine)
    if (RestrictFactionDuke) restrictions |= 0x10;
    else if (RestrictFactionNeutral) restrictions |= 0x20;
    else if (RestrictFactionPrincess) restrictions |= 0x30;
    else if (RestrictFactionRebel) restrictions |= 0x40;

    // Status (mehrere möglich)
    if (RestrictNewbie) restrictions |= 0x0100;
    if (RestrictNotNewbie) restrictions |= 0x0200;
    if (RestrictMurderer) restrictions |= 0x0400;
    if (RestrictNotMurderer) restrictions |= 0x0800;
    if (RestrictOutlaw) restrictions |= 0x1000;
    if (RestrictNotOutlaw) restrictions |= 0x2000;
    if (RestrictLawful) restrictions |= 0x2800;

    // Quest History (mehrere möglich)
    if (RestrictNotTriedRecently) restrictions |= 0x14000;
    if (RestrictNotTried) restrictions |= 0x28000;
    if (RestrictNotSucceededRecently) restrictions |= 0x04000;
    if (RestrictNotSucceeded) restrictions |= 0x08000;
    if (RestrictNotFailedRecently) restrictions |= 0x10000;
    if (RestrictNotFailed) restrictions |= 0x20000;

    // Sonstige
    if (RestrictIntriguing) restrictions |= 0x40000;
    if (RestrictGuildmaster) restrictions |= 0x80000;
    if (RestrictPvpEnabled) restrictions |= 0x100000;

    return restrictions;
}
```

**3. XAML mit GroupBoxen:**
```xml
<GroupBox Header="Player Restrictions">
    <StackPanel>
        <!-- Karma Restrictions (RadioButtons!) -->
        <GroupBox Header="Karma Requirement">
            <StackPanel>
                <RadioButton Content="Same as NPC" IsChecked="{Binding RestrictKarmaSame}"/>
                <RadioButton Content="Different from NPC" IsChecked="{Binding RestrictKarmaDifferent}"/>
                <RadioButton Content="Good Karma" IsChecked="{Binding RestrictKarmaGood}"/>
                <RadioButton Content="Neutral Karma" IsChecked="{Binding RestrictKarmaNeutral}"/>
                <RadioButton Content="Evil Karma" IsChecked="{Binding RestrictKarmaEvil}"/>
            </StackPanel>
        </GroupBox>

        <!-- Faction Restrictions (RadioButtons!) -->
        <GroupBox Header="Faction Requirement">
            <StackPanel>
                <RadioButton Content="Duke's Faction" IsChecked="{Binding RestrictFactionDuke}"/>
                <RadioButton Content="Neutral Faction" IsChecked="{Binding RestrictFactionNeutral}"/>
                <RadioButton Content="Princess' Faction" IsChecked="{Binding RestrictFactionPrincess}"/>
                <RadioButton Content="Rebel Faction" IsChecked="{Binding RestrictFactionRebel}"/>
            </StackPanel>
        </GroupBox>

        <!-- Status Restrictions (CheckBoxes!) -->
        <GroupBox Header="Player Status">
            <StackPanel>
                <CheckBox Content="Newbie Only" IsChecked="{Binding RestrictNewbie}"/>
                <CheckBox Content="Not Newbie" IsChecked="{Binding RestrictNotNewbie}"/>
                <CheckBox Content="Murderer" IsChecked="{Binding RestrictMurderer}"/>
                <CheckBox Content="Not Murderer" IsChecked="{Binding RestrictNotMurderer}"/>
                <CheckBox Content="Outlaw" IsChecked="{Binding RestrictOutlaw}"/>
                <CheckBox Content="Not Outlaw" IsChecked="{Binding RestrictNotOutlaw}"/>
                <CheckBox Content="Lawful (not murderer/outlaw)" IsChecked="{Binding RestrictLawful}"/>
            </StackPanel>
        </GroupBox>

        <!-- Quest History Restrictions (CheckBoxes!) -->
        <GroupBox Header="Quest History">
            <StackPanel>
                <CheckBox Content="Not Tried Recently" IsChecked="{Binding RestrictNotTriedRecently}"/>
                <CheckBox Content="Not Tried Ever" IsChecked="{Binding RestrictNotTried}"/>
                <CheckBox Content="Not Succeeded Recently" IsChecked="{Binding RestrictNotSucceededRecently}"/>
                <CheckBox Content="Not Succeeded Ever" IsChecked="{Binding RestrictNotSucceeded}"/>
                <CheckBox Content="Not Failed Recently" IsChecked="{Binding RestrictNotFailedRecently}"/>
                <CheckBox Content="Not Failed Ever" IsChecked="{Binding RestrictNotFailed}"/>
            </StackPanel>
        </GroupBox>

        <!-- Other Restrictions (CheckBoxes!) -->
        <GroupBox Header="Other Requirements">
            <StackPanel>
                <CheckBox Content="Can Join Factions (Intriguing)" IsChecked="{Binding RestrictIntriguing}"/>
                <CheckBox Content="Is Guildmaster" IsChecked="{Binding RestrictGuildmaster}"/>
                <CheckBox Content="Has PVP Enabled" IsChecked="{Binding RestrictPvpEnabled}"/>
            </StackPanel>
        </GroupBox>
    </StackPanel>
</GroupBox>
```

---

## 6. Penalty Types

### In blakston.khd

**Penalties verwenden dieselben Typen wie Prizes (QN_PRIZETYPE_*)!**

Häufige Penalties:
- `QN_PRIZETYPE_OUTLAW` (0x12) - Make player outlaw
- `QN_PRIZETYPE_FACTION` (0x07) mit `QN_PRIZE_FACTION_NEUTRAL` - Lose faction
- `QN_PRIZETYPE_STATISTIC` (0x03) - Negative stat change
- `QN_PRIZETYPE_BOON` (0x06) - Debuff statt Buff

### Im Editor

```csharp
private ObservableCollection<string> _penaltyTypes = new();
// ❌ LEER!
```

### 🔧 Fix

```csharp
[ObservableProperty]
private ObservableCollection<string> _penaltyTypes = new()
{
    "OUTLAW",                 // Make player outlaw
    "FACTION_NEUTRAL",        // Lose faction standing
    "STATISTIC",              // Negative stat change
    "BOON",                   // Debuff
    "SKILL",                  // Skill penalty
    "SPELL",                  // Spell penalty
    "EXPERIENCE_POINTS"       // Negative XP
};
```

---

## 7. Weitere Fehlende Features

### 7.1 Monster Lists (QNT_MONSTER_LIST)

**In blakston.khd:** Field #6 in QuestNodeTemplate
**Im Editor:** ✅ Vorhanden (QuestMonster Model)

**Status:** ✅ OK

### 7.2 Amount Needed (QNT_AMOUNT_NEEDED)

**In blakston.khd:** Field #13 in QuestNodeTemplate
**Im Editor:** ✅ Vorhanden (QuestNode.AmountNeeded)

**Status:** ✅ OK

### 7.3 Cargo Lists (QNT_CARGO_LIST)

**In blakston.khd:** Field #5 in QuestNodeTemplate
**Im Editor:** ✅ Vorhanden (QuestCargo Model)

**Status:** ✅ OK (aber CargoType sollte validiert werden)

---

## 8. Code-Generierungs-Probleme

### Problem: Hardcoded Strings statt Konstanten

**KodFileService.cs verwendet:**
```csharp
if (node.NodeType == "1")  // MESSAGE
if (node.NodeType == "2")  // ITEM
// etc.
```

**Sollte sein:**
```csharp
public static class QuestConstants
{
    // Node Types
    public const string QN_TYPE_MESSAGE = "1";
    public const string QN_TYPE_ITEM = "2";
    public const string QN_TYPE_ITEMCLASS = "3";
    // ...

    // NPC Modifiers
    public const string QN_NPCMOD_NONE = "0";
    public const string QN_NPCMOD_SAME = "1";
    // ...
}

// Verwendung:
if (node.NodeType == QuestConstants.QN_TYPE_MESSAGE)
```

---

## Priorisierte To-Do-Liste

### 🔴 KRITISCH (muss behoben werden)

1. **NPC Modifiers implementieren**
   - Dropdown mit 5 Optionen
   - Kritisch für Quest-Ketten!

2. **Player Restrictions komplett überarbeiten**
   - Alte String-basierte Restrictions entfernen
   - Neue Checkbox/RadioButton-Implementierung
   - Bitwise Flag-Kombination

3. **Prize Types ergänzen: STATISTIC, SKILL, SPELL, BOON**
   - Mit Sub-Type-Auswahl
   - Dropdowns für Stat/Skill/Spell/Boon-Typen

4. **Node Type: QN_TYPE_MONSTER_ITEMCLASS hinzufügen**
   - Wird in vielen Quests verwendet!

### 🟡 WICHTIG (sollte implementiert werden)

5. **Faction Sub-Types implementieren**
   - Dropdown für Faction-Auswahl

6. **Node Types ergänzen: ITEMFINDCLASS, MONSTER_BRING**

7. **Penalty Types implementieren**

8. **Code-Konstanten-Klasse erstellen**

### 🟢 OPTIONAL (Nice-to-Have)

9. **Validation für NPC Modifiers**
   - "SAME" nur wenn vorheriger Node vorhanden

10. **UI-Tooltips**
    - Erklärung der einzelnen Restrictions

11. **Preview-Funktion**
    - Zeige, wie generierter Kod aussieht

---

## Zusammenfassung

**Der QuestEditor hat erhebliche Lücken:**

| Problem | Schweregrad | Impact |
|---------|-------------|--------|
| NPC Modifiers fehlen komplett | 🔴 KRITISCH | Quest-Ketten unmöglich |
| Player Restrictions falsch | 🔴 KRITISCH | Quests targeting funktioniert nicht |
| Prize Types unvollständig | 🔴 KRITISCH | Stat/Skill/Spell-Rewards unmöglich |
| Node Types unvollständig | 🟡 WICHTIG | Einige Quest-Typen unmöglich |
| Penalty Types fehlen | 🟡 WICHTIG | Keine Quest-Strafen |

**Geschätzte Implementierungs-Zeit:**
- Kritisch: 8-16 Stunden
- Wichtig: 4-8 Stunden
- Optional: 2-4 Stunden

**Total: 14-28 Stunden**

---

**Ende der Gap-Analysis**
