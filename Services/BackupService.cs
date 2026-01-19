using System;
using System.IO;
using System.Linq;
using AutomationProfileManager.Models;
using Newtonsoft.Json;
using Playnite.SDK;

namespace AutomationProfileManager.Services
{
    public class BackupService
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly string backupPath;
        private readonly NotificationService? notificationService;

        public BackupService(IPlayniteAPI api, NotificationService? notifyService = null)
        {
            backupPath = Path.Combine(api.Paths.ExtensionsDataPath, "AutomationProfileManager", "backups");
            notificationService = notifyService;

            if (!Directory.Exists(backupPath))
            {
                Directory.CreateDirectory(backupPath);
            }
        }

        public bool ShouldBackup(ExtensionSettings settings)
        {
            if (!settings.AutoBackupEnabled)
                return false;

            var daysSinceLastBackup = (DateTime.Now - settings.LastBackupDate).TotalDays;
            return daysSinceLastBackup >= settings.BackupIntervalDays;
        }

        public string? CreateBackup(ExtensionData data)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string fileName = $"backup_{timestamp}.json";
                string filePath = Path.Combine(backupPath, fileName);

                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(filePath, json);

                logger.Info($"Created backup: {filePath}");
                
                CleanupOldBackups(data.Settings?.MaxBackupCount ?? 5);

                notificationService?.ShowBackupCompleted(filePath);

                return filePath;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to create backup");
                return null;
            }
        }

        public ExtensionData? RestoreFromBackup(string backupFile)
        {
            try
            {
                if (!File.Exists(backupFile))
                {
                    logger.Warn($"Backup file not found: {backupFile}");
                    return null;
                }

                var json = File.ReadAllText(backupFile);
                var data = JsonConvert.DeserializeObject<ExtensionData>(json);

                logger.Info($"Restored from backup: {backupFile}");
                return data;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to restore from backup: {backupFile}");
                return null;
            }
        }

        public string[] GetAvailableBackups()
        {
            try
            {
                if (!Directory.Exists(backupPath))
                    return Array.Empty<string>();

                return Directory.GetFiles(backupPath, "backup_*.json")
                    .OrderByDescending(f => f)
                    .ToArray();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to get available backups");
                return Array.Empty<string>();
            }
        }

        private void CleanupOldBackups(int maxCount)
        {
            try
            {
                var backups = GetAvailableBackups();
                if (backups.Length <= maxCount)
                    return;

                var toDelete = backups.Skip(maxCount);
                foreach (var file in toDelete)
                {
                    try
                    {
                        File.Delete(file);
                        logger.Info($"Deleted old backup: {file}");
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex, $"Failed to delete old backup: {file}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "Failed to cleanup old backups");
            }
        }
    }
}
