# M59AdminTool – Projektnotizen

## Programmaufbau (aktuell)
- **Connection**: `Services/M59ServerConnection` baut die TCP-Verbindung auf, empfängt `AP_REQ_LOGIN` → sendet Login → `AP_REQ_ADMIN`. Admin-Befehle werden im Login-Admin-Mode als CRLF-Text gesendet. Antworten werden zeilenweise gelesen und via `ResponseReceived` Event verteilt.
- **Admin UI**: 
  - Admin-Tab: Zentrales Antwort-Panel, Custom-Command darunter, feste Button-Spalten links/rechts.
  - Admin Console Tab: Spiegel der Admin-Antworten + Eingabefeld/Send-Button.
  - Logs Tab: Drei ListBoxen (Log/Error/Debug) gebunden an `DebugLogger`-Collections.
- **ViewModels**:
  - `AdminCommandsViewModel`: Sendet Admin-Befehle, sammelt Antworten (`AdminResponses`, `LastAdminResponse`), abonniert `ResponseReceived`, spiegelt Antworten in `DebugLogger` (Log/LogError per Keyword).
  - `ConnectionViewModel`: Handhabt Login/Status und hält die geteilte `ServerConnection` für Admin-Tabs.
- **Debug/Logging**: `DebugLogger` führt drei Collections (`Messages`, `Errors`, `DebugMessages`), `Log`/`LogError`/`Clear`. UI bindet direkt per `x:Static`.
- **Bindings/Context**: `MainWindow.xaml.cs` setzt `AdminCommandsViewModel` als DataContext für Admin-Tab und Admin Console. Logs-Tab liest direkt `DebugLogger`-Collections.

## Kürzlich umgesetzt
- Admin-Modus stabilisiert: Keine Game-Mode-Abhängigkeit für Admin-Textbefehle, CRLF-Terminator, Zeilenweises Lesen und Weiterreichen ans UI.
- UI-Layout Admin: Feste Buttonbreite/Höhe, Antwortpanel mittig, Custom-Command darunter. Zusätzlicher Admin-Console-Tab + Logs-Tab.
- Logging: Trennung Log/Error/Debug über `DebugLogger` und Heuristik im `AdminCommandsViewModel` (Errors per Keywords).

## Bekannte Punkte/Nächste Schritte
- **Logs-Filter**: Aktuell einfache Keyword-Heuristik. Optional: separate LogError-Aufrufe aus Verbindung/Fehlerpfaden oder ein Filter-Flag pro Nachricht.
- **Weitere Tabs nach Vorbild Meridian59.AdminUI**: Players (Online/Room info), RoomObjects, Inventory, Stats/Buffs/Spells, Chat/Guild. Erfordert neue ViewModels/Services und ggf. Game-Mode-Paketanalyse.
- **Game-Mode-Flow**: Falls künftig Game-Mode-Nachrichten nötig sind, PI-Verschlüsselung und AP_GAME-Handling wieder aktivieren (derzeit nur Login/Admin-Text).
- **Build/Tests**: Lokal `dotnet build` im Ordner `M59AdminTool` (dotnet fehlt in dieser Umgebung). UI manuell prüfen.

## Schnellstart
- Login im Connection-Tab, dann Admin-Tab oder Admin-Console nutzen. Antworten erscheinen in beiden und im Logs-Tab (Log/Error/Debug). Custom-Befehle funktionieren überall mit denselben Bindings.

