# Meridian 59 Listen-Auslese Routine

## Übersicht
Diese Routine beschreibt, wie man Listen aus dem Meridian 59 Server ausliest. Der Server verwendet Lisp-ähnliche Knoten-Strukturen (cons cells) zur Datenspeicherung.

## Grundlegende Befehle

### Listen anzeigen
```
show list <listid>
```
Zeigt den gesamten Inhalt einer Liste an und durchläuft alle Knoten.

### Einzelne List-Knoten anzeigen
```
show listnode <nodeid>
```
Zeigt einen einzelnen Knoten in der Liste an.

## Struktur einer Liste

Listen in Meridian 59 folgen der Cons-Struktur (https://en.wikipedia.org/wiki/Cons):
- **first**: Der erste Wert/Subknoten des aktuellen Knotens
- **rest**: Der Rest der Liste (nächster Knoten) oder `$ 0` (Ende)

## Schritt-für-Schritt Auslese-Routine

### 1. Hauptliste identifizieren
Finde die List-ID, die du untersuchen möchtest. Diese findest du in:
- Raum-Eigenschaften (z.B. `plMonsters` für Monster-Spawns)
- Objekt-Eigenschaften (z.B. `plItem_attributes` für Item-Attribute)
- Server-Eigenschaften über `show object <object_id>`

### 2. Liste komplett anzeigen (Schnellmethode)
```
show list <listid>
```

**Beispiel-Output:**
```
:<
: [
: [
: CLASS SpiderBaby
: INT 75
: ]
: [
: CLASS Centipede
: INT 25
: ]
: ]
:>
```

### 3. Liste Knoten-für-Knoten durchlaufen (Detailmethode)

#### Schritt A: Starte mit dem Hauptknoten
```
show listnode <hauptknoten_id>
```

**Output:**
```
:< first = LIST 23290
:  rest = LIST 23293
:>
```

#### Schritt B: Untersuche den first-Knoten
```
show listnode <first_id>
```

**Output:**
```
:< first = CLASS SpiderBaby
:  rest = LIST 23289
:>
```

#### Schritt C: Untersuche den rest-Knoten des first-Knotens
```
show listnode <rest_id>
```

**Output:**
```
:< first = INT 75
:  rest = $ 0
```
`$ 0` bedeutet Ende dieser Unterliste.

#### Schritt D: Gehe zurück zum rest-Knoten des Hauptknotens
Wiederhole Schritt B und C für `rest = LIST 23293` vom Hauptknoten.

### 4. Listen-Traversierung-Algorithmus

```
FUNKTION traverse_list(listnode_id):
    WENN listnode_id == $ 0:
        RETURN  // Ende der Liste

    // Aktuellen Knoten anzeigen
    OUTPUT "show listnode " + listnode_id

    // Lese first und rest Werte
    first_value = Ergebnis.first
    rest_value = Ergebnis.rest

    // Verarbeite first
    WENN first_value ist LIST:
        traverse_list(first_value)  // Rekursiv durchlaufen
    SONST:
        OUTPUT "Wert gefunden: " + first_value

    // Fahre mit rest fort
    WENN rest_value ist LIST:
        traverse_list(rest_value)  // Nächster Knoten
```

## Praktisches Beispiel: Monster-Spawn-Liste auslesen

### Schritt 1: Raum-Objekt finden
```
show object <room_object_id>
```
Suche nach der `plMonsters` Eigenschaft, z.B. `plMonsters = LIST 23294`

### Schritt 2: Monster-Liste anzeigen
```
show list 23294
```

### Schritt 3: Interpretation
Die Liste enthält verschachtelte Listen im Format:
```
[ [MonsterClass1, SpawnRate1], [MonsterClass2, SpawnRate2], ... ]
```

**Beispiel:**
```
[ [&SpiderBaby, 75], [&Centipede, 25] ]
```
- SpiderBaby spawnt mit 75% Wahrscheinlichkeit
- Centipede spawnt mit 25% Wahrscheinlichkeit

## Häufige Listen-Typen im Server

### 1. Monster-Spawn-Listen (`plMonsters`)
```
Struktur: [ [CLASS MonsterName, INT SpawnRate%], ... ]
Beispiel: [ [&Troll, 75], [&Spider, 25] ]
```

### 2. Item-Attribut-Listen (`plItem_attributes`)
```
Struktur: [ [INT AttributeCode, TIMER Duration], ... ]
Beispiel: [ [WA_BLINDER, TIMER 448], ... ]
Hinweis: INT AttributeCode ist eine 3-stellige Zahl:
  - Einer: ungerade = identifiziert, gerade = nicht identifiziert
  - Zehner: Stärke des Attributs (0-9)
  - Hunderter/Tausender: Attribut-Typ (siehe blakston.khd)
```

### 3. Spieler-Zauber-Listen (`plSpells`)
```
Struktur: [ [INT SpellID, INT Ability%], ... ]
```

### 4. Spieler-Skill-Listen (`plSkills`)
```
Struktur: [ [INT SkillID, INT Ability%], ... ]
```

## Vollständige Routine zum manuellen Auslesen

```bash
# 1. Identifiziere das Objekt mit der Liste
show object <object_id>

# 2. Finde die Listen-Eigenschaft (z.B. plMonsters = LIST 12345)
# Notiere die List-ID: 12345

# 3. Zeige die gesamte Liste
show list 12345

# 4. Für detaillierte Analyse: Starte mit dem Hauptknoten
show listnode 12345

# 5. Durchlaufe jeden Knoten:
#    - Zeige first-Knoten: show listnode <first_id>
#    - Zeige rest-Knoten: show listnode <rest_id>
#    - Wiederhole bis rest = $ 0

# 6. Dokumentiere die Werte
```

## Automatisiertes Skript-Beispiel

Für eine automatisierte Auslese könnte man eine Befehlsdatei erstellen:

```bash
# Datei: read_monster_list.txt
# Diese Datei kann mit dem 'load' Befehl geladen werden

echo Lese Monster-Spawn-Liste für Raum aus...
show object <room_id>
show list <plMonsters_list_id>
```

Dann im Admin-Konsolenbefehl:
```
load read_monster_list.txt
```

## Wichtige Hinweise

1. **$ 0 bedeutet NULL/Ende**: Wenn `rest = $ 0`, ist das Ende der Liste erreicht.

2. **Listen sind verschachtelt**: Listen können andere Listen enthalten (Sublisten).

3. **Konsistente Struktur nicht garantiert**: Manche Listen folgen nicht strikt dem erwarteten Format (siehe Item-Attribute mit optionalem Timer).

4. **Listnodes sind flüchtig**: Bei Server-Neustarts oder Änderungen können sich List-IDs ändern.

5. **Blakston.khd Referenz**: Viele INT-Werte in Listen verweisen auf Konstanten in `blakston.khd`.

## Troubleshooting

**Problem:** Liste zeigt `$ 0`
- **Lösung:** Die Liste ist leer oder nicht initialisiert.

**Problem:** Listnode nicht gefunden
- **Lösung:** Die List-ID ist ungültig oder die Liste wurde gelöscht.

**Problem:** Verschachtelte Listen sind verwirrend
- **Lösung:** Verwende `show list <id>` für eine vereinfachte Ansicht.

## Weiterführende Informationen

- Cons-Datenstruktur: https://en.wikipedia.org/wiki/Cons
- Listen modifizieren: Siehe README.md Zeilen 366-463
- Listen erstellen: Verwende `create listnode` Befehle
