using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using Playnite.SDK;

namespace AutomationProfileManager.Services
{
    public class InstalledApp
    {
        public string Name { get; set; } = string.Empty;
        public string ExecutablePath { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public string Category { get; set; } = "Generale";
    }

    public class InstalledAppsService
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public List<InstalledApp> GetInstalledApps()
        {
            var apps = new List<InstalledApp>();

            try
            {
                // 1. Scan Start Menu shortcuts
                ScanStartMenu(apps);

                // 2. Scan common program directories
                ScanProgramDirectories(apps);

                // 3. Scan registry for installed apps
                ScanRegistry(apps);

                // Remove duplicates by process name
                apps = apps
                    .GroupBy(a => a.ProcessName.ToLowerInvariant())
                    .Select(g => g.First())
                    .OrderBy(a => a.Name)
                    .ToList();

                logger.Info($"Found {apps.Count} installed apps");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to scan installed apps");
            }

            return apps;
        }

        private void ScanStartMenu(List<InstalledApp> apps)
        {
            var startMenuPaths = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                    @"Microsoft\Windows\Start Menu\Programs")
            };

            foreach (var startMenuPath in startMenuPaths)
            {
                if (Directory.Exists(startMenuPath))
                {
                    try
                    {
                        var shortcuts = Directory.GetFiles(startMenuPath, "*.lnk", SearchOption.AllDirectories);
                        foreach (var shortcut in shortcuts)
                        {
                            try
                            {
                                var targetPath = ResolveShortcut(shortcut);
                                if (!string.IsNullOrEmpty(targetPath) && 
                                    targetPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) &&
                                    File.Exists(targetPath))
                                {
                                    var name = Path.GetFileNameWithoutExtension(shortcut);
                                    var processName = Path.GetFileNameWithoutExtension(targetPath);

                                    // Skip system/installer apps
                                    if (IsSystemApp(name) || IsSystemApp(processName))
                                        continue;

                                    apps.Add(new InstalledApp
                                    {
                                        Name = name,
                                        ExecutablePath = targetPath,
                                        ProcessName = processName,
                                        Category = CategorizeApp(name, processName)
                                    });
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
            }
        }

        private void ScanProgramDirectories(List<InstalledApp> apps)
        {
            var programDirs = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs")
            };

            foreach (var programDir in programDirs)
            {
                if (Directory.Exists(programDir))
                {
                    try
                    {
                        // Only scan first level subdirectories
                        foreach (var appDir in Directory.GetDirectories(programDir))
                        {
                            try
                            {
                                var exeFiles = Directory.GetFiles(appDir, "*.exe", SearchOption.TopDirectoryOnly);
                                foreach (var exe in exeFiles)
                                {
                                    var name = Path.GetFileNameWithoutExtension(exe);
                                    var processName = name;

                                    if (IsSystemApp(name))
                                        continue;

                                    // Check if already added
                                    if (!apps.Any(a => a.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        apps.Add(new InstalledApp
                                        {
                                            Name = Path.GetFileName(appDir), // Use folder name as app name
                                            ExecutablePath = exe,
                                            ProcessName = processName,
                                            Category = CategorizeApp(name, processName)
                                        });
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
            }
        }

        private void ScanRegistry(List<InstalledApp> apps)
        {
            var registryPaths = new[]
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            foreach (var regPath in registryPaths)
            {
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(regPath))
                    {
                        if (key == null) continue;

                        foreach (var subKeyName in key.GetSubKeyNames())
                        {
                            try
                            {
                                using (var subKey = key.OpenSubKey(subKeyName))
                                {
                                    if (subKey == null) continue;

                                    var displayName = subKey.GetValue("DisplayName") as string;
                                    var installLocation = subKey.GetValue("InstallLocation") as string;
                                    var displayIcon = subKey.GetValue("DisplayIcon") as string;

                                    if (string.IsNullOrEmpty(displayName))
                                        continue;

                                    string? exePath = null;

                                    // Try to find exe from DisplayIcon
                                    if (!string.IsNullOrEmpty(displayIcon))
                                    {
                                        var iconPath = displayIcon.Split(',')[0].Trim('"');
                                        if (iconPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) && File.Exists(iconPath))
                                        {
                                            exePath = iconPath;
                                        }
                                    }

                                    // Try to find exe in install location
                                    if (exePath == null && !string.IsNullOrEmpty(installLocation) && Directory.Exists(installLocation))
                                    {
                                        var exeFiles = Directory.GetFiles(installLocation, "*.exe", SearchOption.TopDirectoryOnly);
                                        if (exeFiles.Length > 0)
                                        {
                                            exePath = exeFiles[0];
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                                    {
                                        var processName = Path.GetFileNameWithoutExtension(exePath);

                                        if (IsSystemApp(displayName) || IsSystemApp(processName))
                                            continue;

                                        if (!apps.Any(a => a.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase)))
                                        {
                                            apps.Add(new InstalledApp
                                            {
                                                Name = displayName,
                                                ExecutablePath = exePath,
                                                ProcessName = processName,
                                                Category = CategorizeApp(displayName, processName)
                                            });
                                        }
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch { }
            }
        }

        private string ResolveShortcut(string shortcutPath)
        {
            try
            {
                byte[] fileBytes = File.ReadAllBytes(shortcutPath);
                string content = System.Text.Encoding.Default.GetString(fileBytes);

                var match = System.Text.RegularExpressions.Regex.Match(content,
                    @"[A-Za-z]:\\[^\x00]+?\.exe",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    return match.Value;
                }
            }
            catch { }
            return "";
        }

        private bool IsSystemApp(string name)
        {
            var systemKeywords = new[]
            {
                "uninstall", "setup", "install", "update", "updater", "helper", "service",
                "crash", "report", "microsoft", "windows", "driver", "runtime",
                "redistributable", "vcredist", "dotnet", ".net", "sdk", "tool",
                "diagnostic", "repair", "remove", "cleanup", "temp"
            };

            var lowerName = name.ToLowerInvariant();
            return systemKeywords.Any(k => lowerName.Contains(k));
        }

        private string CategorizeApp(string name, string processName)
        {
            var lowerName = (name + " " + processName).ToLowerInvariant();

            if (new[] { "chrome", "firefox", "edge", "opera", "brave", "vivaldi", "safari" }.Any(b => lowerName.Contains(b)))
                return "Browser";

            if (new[] { "discord", "teams", "slack", "zoom", "skype", "telegram", "whatsapp", "signal" }.Any(c => lowerName.Contains(c)))
                return "Comunicazione";

            if (new[] { "spotify", "itunes", "music", "vlc", "media", "player", "youtube" }.Any(m => lowerName.Contains(m)))
                return "Multimedia";

            if (new[] { "steam", "epic", "gog", "origin", "uplay", "battle.net", "riot", "game" }.Any(g => lowerName.Contains(g)))
                return "Gaming";

            if (new[] { "obs", "nvidia", "amd", "geforce", "radeon", "afterburner", "hwinfo", "gpu" }.Any(h => lowerName.Contains(h)))
                return "Hardware/Overlay";

            if (new[] { "code", "visual studio", "notepad", "sublime", "atom", "intellij", "pycharm" }.Any(d => lowerName.Contains(d)))
                return "Sviluppo";

            if (new[] { "photoshop", "premiere", "blender", "gimp", "audacity", "davinci" }.Any(c => lowerName.Contains(c)))
                return "Creativita";

            if (new[] { "onedrive", "dropbox", "google drive", "backup", "sync" }.Any(s => lowerName.Contains(s)))
                return "Cloud/Sync";

            return "Altro";
        }
    }
}
