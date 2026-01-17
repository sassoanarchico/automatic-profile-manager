using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace AutomationProfileManager.Services
{
    public class InstalledApplication
    {
        public string Name { get; set; }
        public string ExecutablePath { get; set; }
        public string ProcessName { get; set; }
    }

    public class ApplicationDiscoveryService
    {
        public List<InstalledApplication> GetInstalledApplications()
        {
            var applications = new List<InstalledApplication>();

            try
            {
                // Get from Registry - Current User
                applications.AddRange(GetAppsFromRegistry(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Uninstall"));
                
                // Get from Registry - Local Machine (32-bit)
                applications.AddRange(GetAppsFromRegistry(Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Uninstall"));
                
                // Get from Registry - Local Machine (64-bit)
                using (var key = Registry.LocalMachine.OpenSubKey(@"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"))
                {
                    if (key != null)
                    {
                        applications.AddRange(GetAppsFromRegistry(Registry.LocalMachine, @"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"));
                    }
                }

                // Get from Start Menu shortcuts
                applications.AddRange(GetAppsFromStartMenu());

                // Remove duplicates and sort
                return applications
                    .GroupBy(a => a.ExecutablePath?.ToLowerInvariant())
                    .Where(g => !string.IsNullOrEmpty(g.Key) && File.Exists(g.Key))
                    .Select(g => g.First())
                    .OrderBy(a => a.Name)
                    .ToList();
            }
            catch
            {
                return applications;
            }
        }

        private List<InstalledApplication> GetAppsFromRegistry(RegistryKey baseKey, string subKeyPath)
        {
            var applications = new List<InstalledApplication>();

            try
            {
                using (var key = baseKey.OpenSubKey(subKeyPath))
                {
                    if (key == null) return applications;

                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        try
                        {
                            using (var subKey = key.OpenSubKey(subKeyName))
                            {
                                if (subKey == null) continue;

                                var displayName = subKey.GetValue("DisplayName")?.ToString();
                                var installLocation = subKey.GetValue("InstallLocation")?.ToString();
                                var executable = subKey.GetValue("DisplayIcon")?.ToString() ?? 
                                                subKey.GetValue("UninstallString")?.ToString();

                                if (string.IsNullOrEmpty(displayName)) continue;

                                // Clean executable path
                                if (!string.IsNullOrEmpty(executable))
                                {
                                    executable = executable.Split(',')[0].Trim('"');
                                }

                                // Try to find exe in install location
                                if (string.IsNullOrEmpty(executable) && !string.IsNullOrEmpty(installLocation))
                                {
                                    var exeFiles = Directory.GetFiles(installLocation, "*.exe", SearchOption.TopDirectoryOnly);
                                    if (exeFiles.Length > 0)
                                    {
                                        executable = exeFiles[0];
                                    }
                                }

                                if (!string.IsNullOrEmpty(executable) && File.Exists(executable))
                                {
                                    applications.Add(new InstalledApplication
                                    {
                                        Name = displayName,
                                        ExecutablePath = executable,
                                        ProcessName = Path.GetFileNameWithoutExtension(executable)
                                    });
                                }
                            }
                        }
                        catch
                        {
                            // Skip invalid entries
                        }
                    }
                }
            }
            catch
            {
                // Ignore errors
            }

            return applications;
        }

        private List<InstalledApplication> GetAppsFromStartMenu()
        {
            var applications = new List<InstalledApplication>();
            var startMenuPaths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms)),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs)),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs")
            };

            foreach (var startMenuPath in startMenuPaths)
            {
                if (!Directory.Exists(startMenuPath)) continue;

                try
                {
                    var lnkFiles = Directory.GetFiles(startMenuPath, "*.lnk", SearchOption.AllDirectories);
                    foreach (var lnkFile in lnkFiles)
                    {
                        try
                        {
                            var targetPath = GetShortcutTarget(lnkFile);
                            if (!string.IsNullOrEmpty(targetPath) && File.Exists(targetPath) && 
                                targetPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                            {
                                var name = Path.GetFileNameWithoutExtension(lnkFile);
                                applications.Add(new InstalledApplication
                                {
                                    Name = name,
                                    ExecutablePath = targetPath,
                                    ProcessName = Path.GetFileNameWithoutExtension(targetPath)
                                });
                            }
                        }
                        catch
                        {
                            // Skip invalid shortcuts
                        }
                    }
                }
                catch
                {
                    // Ignore errors
                }
            }

            return applications;
        }

        private string GetShortcutTarget(string shortcutPath)
        {
            return SimpleShortcutReader.GetShortcutTarget(shortcutPath);
        }

        public List<InstalledApplication> GetRunningProcesses()
        {
            var processes = new List<InstalledApplication>();

            try
            {
                var runningProcesses = Process.GetProcesses()
                    .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle) || 
                               !string.IsNullOrEmpty(p.ProcessName))
                    .GroupBy(p => p.ProcessName)
                    .Select(g => g.First())
                    .ToList();

                foreach (var process in runningProcesses)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(process.MainModule?.FileName))
                        {
                            processes.Add(new InstalledApplication
                            {
                                Name = string.IsNullOrEmpty(process.MainWindowTitle) 
                                    ? process.ProcessName 
                                    : $"{process.ProcessName} - {process.MainWindowTitle}",
                                ExecutablePath = process.MainModule.FileName,
                                ProcessName = process.ProcessName
                            });
                        }
                    }
                    catch
                    {
                        // Some processes can't be accessed
                    }
                }

                return processes.OrderBy(p => p.Name).ToList();
            }
            catch
            {
                return processes;
            }
        }
    }
}
