# Dokumentations-Index

**QuestEditor & Blakod System - Vollständige Dokumentation**
**Erstellt:** 2026-01-04

---

## 📚 Verfügbare Dokumentationen

| Datei | Größe | Zweck | Zielgruppe |
|-------|-------|-------|------------|
| [README.md](README.md) | 6 KB | Übersicht, Schnellstart | Alle |
| [KI-SCHNELLDOKUMENTATION.md](KI-SCHNELLDOKUMENTATION.md) | 15 KB | Kompakter Gesamt-Überblick | KI, Neue Entwickler |
| [TECHNISCHE-DETAILS.md](TECHNISCHE-DETAILS.md) | 23 KB | Code-Details, Parsing, Generierung | Entwickler |
| [PROBLEMLÖSUNGEN.md](PROBLEMLÖSUNGEN.md) | 17 KB | Troubleshooting, Debugging | Support, Entwickler |
| [QUICK-REFERENCE.md](QUICK-REFERENCE.md) | 13 KB | Cheat Sheet, Snippets | Alle |
| [ARCHITEKTUR-DIAGRAMM.md](ARCHITEKTUR-DIAGRAMM.md) | 6 KB | Visuelle Diagramme | Architekten, Entwickler |
| [BEKANNTES-PROBLEM-QNT-ID.md](BEKANNTES-PROBLEM-QNT-ID.md) | 6 KB | Kritischer Bug + Lösung | Entwickler |
| [BLAKSTON-KHD-ANALYSE.md](BLAKSTON-KHD-ANALYSE.md) | 15 KB | Analyse der blakston.khd | Entwickler, Editor-Team |

**Gesamt:** ~100 KB Dokumentation

---

## 🎯 Einstiegspunkte

### Für KI-Assistenten

**Start hier:** [KI-SCHNELLDOKUMENTATION.md](KI-SCHNELLDOKUMENTATION.md)

Gibt vollständigen Kontext über:
- Was ist QuestEditor?
- Was ist Blakod?
- Wie funktioniert das System?
- Welche Probleme gibt es?

**Dann:**
- [QUICK-REFERENCE.md](QUICK-REFERENCE.md) für Syntax-Details
- [TECHNISCHE-DETAILS.md](TECHNISCHE-DETAILS.md) für Code-Implementierung
- [BLAKSTON-KHD-ANALYSE.md](BLAKSTON-KHD-ANALYSE.md) für blakston.khd-Details

### Für neue Entwickler

**Start hier:** [README.md](README.md)

**Lernpfad:**
1. [KI-SCHNELLDOKUMENTATION.md](KI-SCHNELLDOKUMENTATION.md) - Grundlagen
2. [ARCHITEKTUR-DIAGRAMM.md](ARCHITEKTUR-DIAGRAMM.md) - Visueller Überblick
3. [QUICK-REFERENCE.md](QUICK-REFERENCE.md) - Syntax-Cheat-Sheet
4. [TECHNISCHE-DETAILS.md](TECHNISCHE-DETAILS.md) - Code-Details
5. [PROBLEMLÖSUNGEN.md](PROBLEMLÖSUNGEN.md) - Debugging

### Für Bug-Fixing

**Start hier:** [BEKANNTES-PROBLEM-QNT-ID.md](BEKANNTES-PROBLEM-QNT-ID.md)

**Kritischer Bug:** QNT_IDs werden an falscher Position eingefügt

**Dann:**
- [BLAKSTON-KHD-ANALYSE.md](BLAKSTON-KHD-ANALYSE.md) - Korrekte Struktur
- [TECHNISCHE-DETAILS.md](TECHNISCHE-DETAILS.md) - Code-Implementierung

### Für Troubleshooting

**Start hier:** [PROBLEMLÖSUNGEN.md](PROBLEMLÖSUNGEN.md)

**7 Kategorien:**
1. Editor-Start-Probleme
2. Quest-Lade-Probleme
3. Quest-Speicher-Probleme
4. Code-Generierungs-Fehler
5. Build-System-Probleme
6. Laufzeit-Fehler
7. GUI-Probleme

---

## 📖 Themen-Index

### Blakod (Kod) Sprache

| Thema | Datei | Abschnitt |
|-------|-------|-----------|
| Syntax-Grundlagen | [KI-SCHNELLDOKUMENTATION.md](KI-SCHNELLDOKUMENTATION.md) | Blakod (Kod) Sprache |
| Syntax Cheat Sheet | [QUICK-REFERENCE.md](QUICK-REFERENCE.md) | Blakod Syntax Cheat Sheet |
| Datentypen | [KI-SCHNELLDOKUMENTATION.md](KI-SCHNELLDOKUMENTATION.md) | Datentypen |
| Namenskonventionen | [KI-SCHNELLDOKUMENTATION.md](KI-SCHNELLDOKUMENTATION.md) | Namenskonventionen |
| Operatoren | [QUICK-REFERENCE.md](QUICK-REFERENCE.md) | Operatoren |
| Control Flow | [QUICK-REFERENCE.md](QUICK-REFERENCE.md) | Control Flow |
| Message Passing | [QUICK-REFERENCE.md](QUICK-REFERENCE.md) | Message Passing |
| Listen | [QUICK-REFERENCE.md](QUICK-REFERENCE.md) | Listen |

### QuestEditor Architektur

| Thema | Datei | Abschnitt |
|-------|-------|-----------|
| MVVM-Struktur | [KI-SCHNELLDOKUMENTATION.md](KI-SCHNELLDOKUMENTATION.md) | QuestEditor Architektur |
| Diagramme | [ARCHITEKTUR-DIAGRAMM.md](ARCHITEKTUR-DIAGRAMM.md) | Alle Abschnitte |
| Services | [KI-SCHNELLDOKUMENTATION.md](KI-SCHNELLDOKUMENTATION.md) | Wichtige Services |
| Datenmodell | [KI-SCHNELLDOKUMENTATION.md](KI-SCHNELLDOKUMENTATION.md) | Quest-Datenmodell |
| Datenfluss | [ARCHITEKTUR-DIAGRAMM.md](ARCHITEKTUR-DIAGRAMM.md) | Datenfluss: Quest Erstellen |

### Code-Generierung

| Thema | Datei | Abschnitt |
|-------|-------|-----------|
| KodFileService | [TECHNISCHE-DETAILS.md](TECHNISCHE-DETAILS.md) | KodFileService Details |
| .kod-Generierung | [TECHNISCHE-DETAILS.md](TECHNISCHE-DETAILS.md) | .kod-Datei Generierung |
| .lkod-Generierung | [TECHNISCHE-DETAILS.md](TECHNISCHE-DETAILS.md) | .lkod-Datei |
| Makefile-Integration | [TECHNISCHE-DETAILS.md](TECHNISCHE-DETAILS.md) | Makefile-Integration |
| Code-Beispiele | [TECHNISCHE-DETAILS.md](TECHNISCHE-DETAILS.md) | Beispiel-Quests |

### blakston.khd

| Thema | Datei | Abschnitt |
|-------|-------|-----------|
| Struktur-Übersicht | [BLAKSTON-KHD-ANALYSE.md](BLAKSTON-KHD-ANALYSE.md) | Struktur-Übersicht |
| Quest Template IDs | [BLAKSTON-KHD-ANALYSE.md](BLAKSTON-KHD-ANALYSE.md) | Quest Template IDs |
| Quest Node IDs | [BLAKSTON-KHD-ANALYSE.md](BLAKSTON-KHD-ANALYSE.md) | Quest Node Template IDs |
| Quest-Konstanten | [BLAKSTON-KHD-ANALYSE.md](BLAKSTON-KHD-ANALYSE.md) | Quest-Konstanten |
| Player Restrictions | [BLAKSTON-KHD-ANALYSE.md](BLAKSTON-KHD-ANALYSE.md) | Quest Player Restrictions |
| Node Types | [BLAKSTON-KHD-ANALYSE.md](BLAKSTON-KHD-ANALYSE.md) | Quest Node Types |
| Prize Types | [BLAKSTON-KHD-ANALYSE.md](BLAKSTON-KHD-ANALYSE.md) | Quest Node Prize Types |

### Probleme & Lösungen

| Problem | Datei | Abschnitt |
|---------|-------|-----------|
| **QNT_ID falsche Position** | [BEKANNTES-PROBLEM-QNT-ID.md](BEKANNTES-PROBLEM-QNT-ID.md) | Gesamtes Dokument |
| Editor startet nicht | [PROBLEMLÖSUNGEN.md](PROBLEMLÖSUNGEN.md) | Editor-Start-Probleme |
| Quests laden nicht | [PROBLEMLÖSUNGEN.md](PROBLEMLÖSUNGEN.md) | Quest-Lade-Probleme |
| Quest speichert nicht | [PROBLEMLÖSUNGEN.md](PROBLEMLÖSUNGEN.md) | Quest-Speicher-Probleme |
| .kod-Datei ungültig | [PROBLEMLÖSUNGEN.md](PROBLEMLÖSUNGEN.md) | Code-Generierungs-Fehler |
| Quest wird nicht kompiliert | [PROBLEMLÖSUNGEN.md](PROBLEMLÖSUNGEN.md) | Build-System-Probleme |
| Quest erscheint nicht im Spiel | [PROBLEMLÖSUNGEN.md](PROBLEMLÖSUNGEN.md) | Laufzeit-Fehler |

---

## 🔧 Implementierungs-Prioritäten

### Kritisch (sofort beheben)

1. **QNT_ID Einfüge-Position**
   - [BEKANNTES-PROBLEM-QNT-ID.md](BEKANNTES-PROBLEM-QNT-ID.md) - Komplette Lösung
   - [BLAKSTON-KHD-ANALYSE.md](BLAKSTON-KHD-ANALYSE.md#1-qnt_id-einfüge-position-kritisch) - Struktur-Analyse

2. **Node-Namen Konvertierung**
   - [BLAKSTON-KHD-ANALYSE.md](BLAKSTON-KHD-ANALYSE.md#2-node-namenskonvention) - ONE statt 1

3. **ID-Vergabe aus blakston.khd**
   - [BLAKSTON-KHD-ANALYSE.md](BLAKSTON-KHD-ANALYSE.md#4-id-vergabe) - Korrekte Implementierung

### Wichtig (nächste Phase)

4. **Player Restrictions GUI**
   - [BLAKSTON-KHD-ANALYSE.md](BLAKSTON-KHD-ANALYSE.md#6-player-restrictions---gui-verbesserung) - Checkboxen/Dropdowns

5. **Quest Node Types erweitert**
   - [BLAKSTON-KHD-ANALYSE.md](BLAKSTON-KHD-ANALYSE.md#7-quest-node-types---dropdown) - Alle Typen

6. **Prize Types erweitert**
   - [BLAKSTON-KHD-ANALYSE.md](BLAKSTON-KHD-ANALYSE.md#8-prize-types---erweiterte-unterstützung) - 22 Typen

### Optional (Verbesserungen)

7. **Monster-Listen Support**
   - [BLAKSTON-KHD-ANALYSE.md](BLAKSTON-KHD-ANALYSE.md#9-monster-listen)

8. **Penalty-Listen Support**
   - [BLAKSTON-KHD-ANALYSE.md](BLAKSTON-KHD-ANALYSE.md#10-penalty-listen)

9. **Quest-Validierung**
   - [BLAKSTON-KHD-ANALYSE.md](BLAKSTON-KHD-ANALYSE.md#checkliste-editor-verbesserungen)

---

## 🎓 Lern-Ressourcen

### Externe Dokumentation

Verweise in `../doc/`:

- [kodspec.md](../doc/kodspec.md) - Blakod-Spezifikation
- [kodsyntax.md](../doc/kodsyntax.md) - Syntax-Referenz
- [koddatatypes.md](../doc/koddatatypes.md) - Datentypen-Referenz
- [kodccalls.md](../doc/kodccalls.md) - C-Call-Referenz (Standard-Library)
- [kodresource.md](../doc/kodresource.md) - Resource-System

### Beispiel-Quests

In `../questtemplate/`:

- [chickensoupqt.kod](../questtemplate/chickensoupqt.kod) - Minimal-Quest (3 Nodes)
- [apothecaryqt.kod](../questtemplate/apothecaryqt.kod) - Item-Delivery (2 Nodes)
- [loveletterqt.kod](../questtemplate/loveletterqt.kod) - Multi-Quest-Chain

### Code-Dateien

- [KodFileService.cs](../Services/KodFileService.cs) - Quest-Datei-Operationen
- [BlakstonKhdService.cs](../Services/BlakstonKhdService.cs) - blakston.khd-Operationen
- [QuestEditorViewModel.cs](../ViewModels/QuestEditorViewModel.cs) - UI-Logik

---

## 🔍 Such-Hilfe

### Nach Thema suchen

**Blakod-Syntax:**
```
grep -r "Blakod" Doku/
grep -r "syntax" Doku/
```

**Code-Generierung:**
```
grep -r "GenerateKodFile" Doku/
grep -r "KodFileService" Doku/
```

**Quest-Konstanten:**
```
grep -r "QST_ID" Doku/
grep -r "QNT_ID" Doku/
grep -r "blakston.khd" Doku/
```

**Probleme:**
```
grep -r "Problem" Doku/
grep -r "Error" Doku/
grep -r "Bug" Doku/
```

### Nach Code suchen

**C# Code-Snippets:**
```
grep -A 10 "```csharp" Doku/
```

**Kod Code-Beispiele:**
```
grep -A 10 "```kod" Doku/
```

**Regex-Patterns:**
```
grep -A 5 "Regex" Doku/
```

---

## 📊 Statistiken

### Dokumentations-Abdeckung

| Bereich | Status | Dokumentation |
|---------|--------|---------------|
| Blakod-Sprache | ✅ 100% | KI-SCHNELLDOKUMENTATION, QUICK-REFERENCE |
| QuestEditor-Architektur | ✅ 100% | KI-SCHNELLDOKUMENTATION, ARCHITEKTUR-DIAGRAMM |
| Code-Generierung | ✅ 100% | TECHNISCHE-DETAILS |
| blakston.khd | ✅ 100% | BLAKSTON-KHD-ANALYSE |
| Troubleshooting | ✅ 100% | PROBLEMLÖSUNGEN |
| Bug-Dokumentation | ✅ 100% | BEKANNTES-PROBLEM-QNT-ID |

### Code-Beispiele

| Typ | Anzahl | Datei |
|-----|--------|-------|
| Blakod-Beispiele | 10+ | TECHNISCHE-DETAILS, QUICK-REFERENCE |
| C# Code-Snippets | 20+ | TECHNISCHE-DETAILS, BLAKSTON-KHD-ANALYSE |
| Regex-Patterns | 10+ | TECHNISCHE-DETAILS, QUICK-REFERENCE |
| Komplette Quests | 3 | TECHNISCHE-DETAILS |

### Problem-Abdeckung

| Kategorie | Probleme dokumentiert |
|-----------|----------------------|
| Editor-Start | 4 Probleme |
| Quest-Laden | 3 Probleme |
| Quest-Speichern | 3 Probleme |
| Code-Generierung | 3 Probleme |
| Build-System | 3 Probleme |
| Laufzeit | 2 Probleme |
| GUI | 3 Probleme |

**Gesamt:** 21 häufige Probleme mit Lösungen

---

## ✅ Checkliste: Dokumentation nutzen

### Als KI-Assistent

- [ ] [KI-SCHNELLDOKUMENTATION.md](KI-SCHNELLDOKUMENTATION.md) gelesen
- [ ] [BLAKSTON-KHD-ANALYSE.md](BLAKSTON-KHD-ANALYSE.md) für blakston.khd-Struktur studiert
- [ ] [BEKANNTES-PROBLEM-QNT-ID.md](BEKANNTES-PROBLEM-QNT-ID.md) für kritischen Bug verstanden
- [ ] [QUICK-REFERENCE.md](QUICK-REFERENCE.md) als Cheat Sheet griffbereit

### Als Entwickler

- [ ] [README.md](README.md) für Übersicht gelesen
- [ ] [ARCHITEKTUR-DIAGRAMM.md](ARCHITEKTUR-DIAGRAMM.md) für visuellen Überblick angesehen
- [ ] [TECHNISCHE-DETAILS.md](TECHNISCHE-DETAILS.md) für Code-Implementierung studiert
- [ ] [PROBLEMLÖSUNGEN.md](PROBLEMLÖSUNGEN.md) für Debugging-Strategien gelesen
- [ ] [BLAKSTON-KHD-ANALYSE.md](BLAKSTON-KHD-ANALYSE.md) für Editor-Verbesserungen studiert

### Für Bug-Fix

- [ ] [BEKANNTES-PROBLEM-QNT-ID.md](BEKANNTES-PROBLEM-QNT-ID.md) gelesen
- [ ] [BLAKSTON-KHD-ANALYSE.md](BLAKSTON-KHD-ANALYSE.md) für korrekte Struktur verstanden
- [ ] [TECHNISCHE-DETAILS.md](TECHNISCHE-DETAILS.md) für Implementierungs-Details gelesen
- [ ] Test-Plan aus BEKANNTES-PROBLEM-QNT-ID.md ausgeführt

---

## 📝 Aktualisierungs-Historie

### v1.0 - 2026-01-04

**Initiale Erstellung:**
- 8 Dokumentationsdateien erstellt
- ~100 KB Dokumentation
- Komplette Abdeckung aller Bereiche
- Kritischer Bug (QNT_ID) dokumentiert + Lösung
- blakston.khd vollständig analysiert

**Nächste geplante Updates:**
- Nach Bug-Fix: Update mit neuer Implementierung
- Nach GUI-Verbesserungen: Screenshots hinzufügen
- Nach neuen Features: Feature-Dokumentation

---

## 🤝 Beitragen

### Dokumentation erweitern

**Neue Probleme gefunden?**
→ [PROBLEMLÖSUNGEN.md](PROBLEMLÖSUNGEN.md) erweitern

**Neue Code-Beispiele?**
→ [TECHNISCHE-DETAILS.md](TECHNISCHE-DETAILS.md) oder [QUICK-REFERENCE.md](QUICK-REFERENCE.md) ergänzen

**Neue Features?**
→ [KI-SCHNELLDOKUMENTATION.md](KI-SCHNELLDOKUMENTATION.md) aktualisieren

### Format-Richtlinien

- Markdown (GitHub-Flavored)
- Code-Blöcke mit Syntax-Highlighting
- Konkrete Beispiele
- Schritt-für-Schritt-Anleitungen
- Verlinkungen zwischen Dokumenten

---

**Ende des Dokumentations-Index**

Bei Fragen: Diese Übersicht als Einstiegspunkt verwenden → passende Dokumentation finden → Details lesen.
