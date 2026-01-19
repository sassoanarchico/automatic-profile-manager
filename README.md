# Automation Profile Manager (Playnite)

Estensione Playnite per eseguire profili di automazione quando avvii o chiudi un gioco:
- Chiudi/avvia app comuni (browser, Discord, Spotify, client di gioco).
- Comandi di sistema (piani energetici, attese, script PowerShell, Game Mode).
- Azioni mirror per ripristinare lo stato alla chiusura del gioco.

## Installazione
1. Scarica `AutomationProfileManager.pext` dalla sezione Releases.
2. In Playnite: Menu → Add-ons... → Install from file → seleziona il `.pext`.

## Uso rapido
- Imposta le azioni nella libreria (chiudi app, avvia app, comandi di sistema, script PS, attese).
- Crea un profilo e trascina le azioni nell’ordine desiderato.
- Assegna il profilo a uno o più giochi dal menu contestuale di Playnite.

## Build e pacchetto
- `dotnet build -c Release`
- `powershell -ExecutionPolicy Bypass -File build-pext.ps1 -Configuration Release` (genera `AutomationProfileManager.pext`).

## Licenza
MIT License - vedi il file `LICENSE`.
