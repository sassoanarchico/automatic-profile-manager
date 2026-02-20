<div align="center">

# Automation Profile Manager

**Playnite extension that automates app management and system settings when you play games**

[![Version](https://img.shields.io/badge/version-0.4.0-blue.svg)]()
[![Platform](https://img.shields.io/badge/platform-Playnite%2010+-purple.svg)]()
[![Framework](https://img.shields.io/badge/.NET-Framework%204.8-green.svg)]()
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg)](LICENSE)

Close distracting apps, launch utilities, change resolution, control volume, run PowerShell scripts — all automatically when you start or stop a game.

</div>

---

## Table of Contents

- [Features](#features)
- [Screenshots](#screenshots)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Settings](#settings)
- [Project Structure](#project-structure)
- [Development](#development)
- [Roadmap](#roadmap)
- [Known Issues](#known-issues)
- [Changelog](#changelog)
- [License](#license)

---

## Features

### Automation Profiles
| Feature | Description |
|---------|-------------|
| **Reusable Profiles** | Create named profiles with ordered lists of actions |
| **Game Assignment** | Assign a profile to one or more games |
| **Three Execution Phases** | Actions run Before Starting, After Starting, or After Closing a game |
| **Priority Ordering** | Control the exact execution order of actions within a profile |
| **Duplicate Profiles** | Clone an existing profile to create variations |
| **Import / Export** | Share profiles and actions in JSON format |

### Action Types
| Feature | Description |
|---------|-------------|
| **Start App** | Launch any executable with optional arguments |
| **Close App** | Kill a running process by name |
| **PowerShell Script** | Execute inline PowerShell commands or `.ps1` scripts |
| **System Command** | Run CMD commands (power plans, registry tweaks, etc.) |
| **Wait** | Pause execution for a configurable number of seconds |
| **Change Resolution** | Switch display resolution before a game, auto-restore after |
| **Set Volume** | Set system volume (0–100) or restore the previous level |
| **Mute / Unmute App** | Silence specific apps (e.g. Discord) while gaming |

### Smart Features
| Feature | Description |
|---------|-------------|
| **Mirror Actions** | Close an app before a game → automatically reopen it after |
| **Conditions** | Skip actions based on `ProcessRunning`, `FileExists`, or `TimeRange` |
| **Parallel Execution** | Run non-dependent actions in parallel (`RunInParallel` flag) |
| **Timeout** | Automatically kill actions that exceed a configurable duration |
| **Action Dependencies** | Execute an action only if its dependency succeeded |
| **Dry-Run Mode** | Test a profile without actually executing any action |

### Discovery & Setup
| Feature | Description |
|---------|-------------|
| **Setup Wizard** | Guided first-run experience with 6 steps |
| **Auto-Discovery** | Scans Start Menu, Program Files, and Registry for installed apps |
| **Smart Categorization** | Detected apps are auto-categorized (Browser, Gaming, Multimedia, etc.) |
| **Default Action Library** | ~40 pre-built actions for common apps (Chrome, Discord, Steam, etc.) |
| **Emulator Templates** | Ready-made profiles for RetroArch, Dolphin, PCSX2, PPSSPP, Cemu |

### Monitoring & Safety
| Feature | Description |
|---------|-------------|
| **Notifications** | Playnite notifications when a profile starts/completes |
| **Action Log** | Timestamped log of every action execution with success/failure status |
| **Statistics** | Track total executions, time saved, most-used profiles, and error rate |
| **Automatic Backup** | Periodic JSON backup of your configuration |
| **Backup Restore** | Recover previous configurations from backup history |

### Interface
| Feature | Description |
|---------|-------------|
| **Tabbed Settings UI** | Action Library, Profile Manager, Log, Statistics, Settings — all in one view |
| **TreeView with Categories** | Actions grouped in collapsible categories with item counts |
| **Drag & Drop** | Reorder actions within a profile by dragging |
| **Multi-Select** | Select multiple actions with Ctrl+Click or Shift+Click |
| **Custom Tags** | Organize actions with comma-separated tags |
| **Find & Replace Paths** | Bulk-update executable paths across profiles |
| **Dark Theme** | Follows Playnite theme (dark/light) |
| **Localization** | English and Italian — extensible via XAML resource files |

---

## Screenshots

> The settings view contains five tabs: Action Library, Profile Manager, Action Log, Statistics, and Settings.
> The setup wizard guides new users through app discovery and initial profile creation.

---

## Installation

### Quick Method (.pext file)

1. Download `AutomationProfileManager.pext` from the [Releases](https://github.com/sassoanarchico/automation-profile-manager/releases) page
2. In Playnite: **Menu → Add-ons... → Install from file**
3. Select the downloaded `.pext` file
4. **Restart Playnite**
5. Configure your first profile in **Menu → Extensions → Extension settings → Automation Profile Manager**

### Manual Installation (developers)

```powershell
# Clone the repository
git clone https://github.com/sassoanarchico/automation-profile-manager.git
cd automation-profile-manager

# Build and package
.\build.ps1
```

Or copy the compiled folder to `%AppData%\Playnite\Extensions\AutomationProfileManager\`.

---

## Quick Start

### First Run — Setup Wizard

On first launch, a wizard guides you through:

1. **Welcome** — Overview of what the extension does
2. **App Discovery** — Scans your system for installed applications
3. **App Selection** — Choose which apps to manage (common apps pre-selected)
4. **Action Types** — Pick whether to generate Close, Start, or both action types
5. **Categorization** — Review auto-assigned categories
6. **Summary** — Confirm and create your initial action library

### Creating a Profile

1. Open **Extension settings → Automation Profile Manager → Profile Manager**
2. Click **New Profile** and enter a name (e.g., "Gaming Focus")
3. From the action tree on the left, double-click or drag actions to the profile
4. Set the **Execution Phase** for each action:
   - `BeforeStarting` — Runs before the game launches (e.g., close Chrome, set resolution)
   - `AfterStarting` — Runs after the game process is detected
   - `AfterClosing` — Runs when the game exits (e.g., restore resolution, reopen Chrome)
5. Reorder with drag & drop, adjust priorities

### Assigning a Profile to a Game

1. In Playnite, right-click a game → **Automation Profile Manager → Assign Profile**
2. Select a profile from the list
3. The profile will execute automatically on game start/stop

### Testing with Dry-Run

1. Enable **Dry-Run** in the Settings tab
2. Start any game — actions are logged but not actually executed
3. Check the **Action Log** tab to verify correct behavior
4. Disable Dry-Run when ready for real execution

---

## Settings

Accessible from **Menu → Extensions → Extension settings → Automation Profile Manager → Settings tab**.

| Setting | Default | Description |
|---------|---------|-------------|
| Show Notifications | `Yes` | Playnite notifications on profile start/completion |
| Enable Dry-Run | `No` | Log actions without executing them |
| Automatic Backup | `Yes` | Periodic backup of configuration data |
| Backup Interval | `7 days` | Time between automatic backups |
| Max Backups | `5` | Number of backup files to retain |

---

## Project Structure

```
AutomationProfileManager/
├── AutomationProfileManager.cs           # Entry point: GenericPlugin, game events, profile execution
├── AutomationProfileManagerSettings.cs   # ISettings implementation
├── extension.yaml                        # Playnite manifest (v0.4.0)
│
├── Models/
│   ├── GameAction.cs              # Action definition (type, path, args, conditions, phase)
│   ├── ActionType.cs              # Enum: StartApp, CloseApp, PowerShell, SystemCommand, Wait, ...
│   ├── ExecutionPhase.cs          # Enum: BeforeStarting, AfterStarting, AfterClosing
│   ├── AutomationProfile.cs      # Named list of GameAction items
│   ├── ProfileMapping.cs         # Game ID → Profile ID dictionary
│   ├── ExtensionData.cs          # Root data model (profiles, actions, mappings, settings, stats)
│   ├── ExtensionSettings.cs      # Settings DTO + ActionStatistics + ProfileStatistics
│   └── ActionLogEntry.cs         # Log record (timestamp, action, result)
│
├── Services/
│   ├── ActionExecutor.cs          # Executes all action types with parallel/timeout support
│   ├── ActionLogService.cs        # In-memory action log with entry trimming
│   ├── ApplicationDiscoveryService.cs  # Registry + Start Menu app discovery
│   ├── AudioService.cs            # Volume control and per-app mute
│   ├── BackupService.cs           # JSON backup/restore with retention
│   ├── ConditionService.cs        # Pre-condition evaluation (process, file, time)
│   ├── DataService.cs             # JSON persistence + data migration
│   ├── DefaultActionsProvider.cs  # ~40 built-in default actions
│   ├── InstalledAppsService.cs    # Alternative app discovery with categorization
│   ├── LocalizationService.cs     # Localized string helper
│   ├── MirrorActionTracker.cs     # Tracks close→reopen mirror state
│   ├── NotificationService.cs     # Playnite notification wrappers
│   ├── ProfileImportExportService.cs  # JSON import/export
│   ├── ProfileTemplateService.cs  # Emulator profile templates
│   ├── ResolutionService.cs       # Win32 P/Invoke display resolution
│   ├── SimpleShortcutReader.cs    # COM-based .lnk shortcut resolver
│   └── StatisticsService.cs       # Execution stats tracking
│
├── Views/
│   ├── AutomationProfileManagerSettingsView.xaml/.cs  # Main 5-tab settings UI
│   ├── ActionEditDialog.xaml/.cs           # Create/edit action dialog
│   ├── ApplicationSelectionDialog.xaml/.cs  # Installed app picker
│   ├── ProfileAssignmentDialog.xaml/.cs    # Profile-to-game assignment
│   ├── SetupWizardDialog.xaml/.cs          # First-run wizard (6 steps)
│   ├── TextInputDialog.xaml/.cs            # Simple text prompt
│   └── Styles/
│       └── DarkTheme.xaml                  # Centralized dark theme styles
│
├── Localization/
│   ├── en_US.xaml                 # English strings
│   └── it_IT.xaml                 # Italian strings
│
├── Presets/
│   └── DefaultActions.json        # Sample preset file
│
├── build.ps1                      # MSBuild + .pext packaging
├── build-pext.ps1                 # Alternative .pext packaging (pre-built)
├── CHANGELOG.txt                  # Version history
└── LICENSE                        # MIT License
```

---

## Development

### Prerequisites

- Visual Studio 2022+ or VS Code with C# extension
- .NET Framework 4.8 Developer Pack
- Playnite installed (for `Playnite.SDK.dll`)

### Building

```powershell
cd automatic-profile-manager
.\build.ps1
```

The script:
1. Finds `Playnite.SDK.dll` in `%LOCALAPPDATA%\Playnite`
2. Compiles in Release mode
3. Creates the `.pext` file with the version from `extension.yaml`

### NuGet Dependencies

| Package | Version | Usage |
|---------|---------|-------|
| Newtonsoft.Json | 13.0.3 | JSON serialization for data persistence |

### Architecture

The extension follows a **Model–Service–View** pattern:

1. **Models** — Plain C# objects serialized to JSON via Newtonsoft
2. **Services** — Business logic layer: action execution, app discovery, audio, resolution, backup, statistics
3. **Views** — WPF dialogs and the main tabbed settings UserControl

**Data flow**: `DataService` loads/saves a single `ExtensionData` JSON file in Playnite's extension data folder. The plugin holds it in memory and passes references to the settings UI.

**Execution flow**:
1. Playnite fires `OnGameStarting` → plugin looks up the game's assigned profile
2. Actions matching `BeforeStarting` phase are sorted by priority and executed
3. `OnGameStarted` → `AfterStarting` actions execute
4. `OnGameStopped` → `AfterClosing` actions execute + mirror actions restore state

---

## Roadmap

### High Priority
- [ ] **Fix mirror action tracking** — Track process state *before* executing close actions, not after
- [ ] **Wire up ActionLogService** — Connect log service to ActionExecutor so the Log tab shows real data
- [ ] **Fix action timeout** — Pass `CancellationToken` to process execution methods
- [ ] **Fix `Process.WaitForExit()` deadlocks** — Read stdout/stderr streams or use timeouts
- [ ] **Add `GetGameMenuItems()` override** — Enable profile assignment from the game right-click menu
- [ ] **Fix redirected stream deadlocks** — Read `StandardOutput`/`StandardError` before `WaitForExit()`

### Medium Priority
- [ ] **Replace audio SendKeys approach** — Use Windows Core Audio API (P/Invoke to `IAudioEndpointVolume`) for reliable volume control
- [ ] **Complete localization** — Move all hardcoded Italian/English strings to XAML resource files
- [ ] **Implement `ISettings` properly** — Support Begin/Cancel/End edit for Playnite undo behavior
- [ ] **Merge duplicate discovery services** — Unify `ApplicationDiscoveryService` and `InstalledAppsService`
- [ ] **Merge duplicate shortcut readers** — Single implementation for `.lnk` resolution
- [ ] **Fix build scripts** — Include `Localization/` and `Presets/` folders in `.pext` packaging
- [ ] **Add `ObservableCollection` to models** — Replace `List<T>` for proper WPF data binding
- [ ] **Dispose `Process` objects** — Fix handle leaks in `ActionExecutor`, `ConditionService`, `MirrorActionTracker`
- [ ] **Fix thread safety** — Add synchronization for concurrent game events accessing `extensionData`

### Future Ideas
- [ ] Profile enable/disable toggle
- [ ] Per-action enable/disable
- [ ] Bulk profile assignment (by platform, source, category)
- [ ] Per-game execution history
- [ ] Action path validation warnings
- [ ] Multi-monitor resolution support
- [ ] Conditional profiles (time-of-day, battery level, etc.)
- [ ] Unit test project
- [ ] CI/CD pipeline with GitHub Actions
- [ ] Playnite add-on database submission

---

## Known Issues

### Critical
| Issue | Description |
|-------|-------------|
| **Mirror restore broken** | Process state is tracked *after* close actions execute, so tracking always records `isRunning = false`. Mirror restore never triggers. |
| **Timeout non-functional** | `CancellationToken` is created but never passed to action execution. `TimeoutSeconds` has no effect. |
| **Process deadlocks** | `Process.WaitForExit()` is called without timeout. Stdout/stderr are redirected but never read, causing pipe buffer deadlocks. |
| **Action log always empty** | `ActionLogService` is instantiated but never wired to `ActionExecutor`. The Log tab shows no data. |

### High
| Issue | Description |
|-------|-------------|
| **`async void` event handler** | `ExecuteProfileActions` is `async void` — unhandled exceptions crash the application. |
| **No game context menu** | `GetGameMenuItems()` is not overridden. There is no way to assign profiles from the game right-click menu. |
| **Volume control unreliable** | Uses `SendKeys` to simulate volume key presses (audible, imprecise, focus-dependent). |
| **Audio module dependency** | `Get-AudioDevice` PowerShell module must be installed separately; failure is silent. |
| **Process handle leaks** | `Process.GetProcessesByName()` results are never disposed in multiple services. |

### Medium
| Issue | Description |
|-------|-------------|
| **Hardcoded strings** | Many UI strings in Italian are not using the localization system. |
| **Build scripts incomplete** | `.pext` packaging doesn't include localization files or presets. |
| **Settings cancel no-op** | `ISettings.CancelEdit()` is empty — Playnite's Cancel button has no effect. |
| **Execution phase forced** | Adding an action to a profile always sets phase to `BeforeStarting`, ignoring the original. |

---

## Changelog

See [CHANGELOG.txt](CHANGELOG.txt) for the complete list of changes.

### Latest versions

- **v0.4.0** — Current development version
- **v0.3.2** — Multi-select actions, Delete key support, improved action categories
- **v0.3.1** — Wizard persistence fix (GUID-based folder), custom tags, TreeView categories
- **v0.3.0** — Installed app scanning, smart conditions, auto-categorization, redesigned wizard
- **v0.2.4** — Manual .exe/.lnk selection, shortcut resolution, improved app list UI
- **v0.2.0** — Dry-run, log viewer, emulator templates, resolution/volume control, parallel execution, backup, statistics
- **v0.1.0** — Initial release: PowerShell execution, action library, mirror actions, drag & drop profiles

---

## License

Distributed under the [MIT](LICENSE) license.

---

## Author

**Sassoanarchico** — [GitHub](https://github.com/sassoanarchico)

---

<div align="center">
<sub>Made with coffee and automation for Playnite</sub>
</div>
