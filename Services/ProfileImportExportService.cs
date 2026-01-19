using System;
using System.Collections.Generic;
using System.IO;
using AutomationProfileManager.Models;
using Newtonsoft.Json;
using Playnite.SDK;

namespace AutomationProfileManager.Services
{
    public class ProfileImportExportService
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public class ExportData
        {
            public string Version { get; set; } = "1.0";
            public DateTime ExportDate { get; set; } = DateTime.Now;
            public List<AutomationProfile> Profiles { get; set; } = new List<AutomationProfile>();
            public List<GameAction> Actions { get; set; } = new List<GameAction>();
        }

        public bool ExportProfiles(List<AutomationProfile> profiles, List<GameAction> actions, string filePath)
        {
            try
            {
                var exportData = new ExportData
                {
                    Version = "1.0",
                    ExportDate = DateTime.Now,
                    Profiles = profiles,
                    Actions = actions
                };

                var json = JsonConvert.SerializeObject(exportData, Formatting.Indented);
                File.WriteAllText(filePath, json);
                
                logger.Info($"Exported {profiles.Count} profiles and {actions.Count} actions to {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to export profiles to {filePath}");
                return false;
            }
        }

        public bool ExportSingleProfile(AutomationProfile profile, string filePath)
        {
            try
            {
                var exportData = new ExportData
                {
                    Version = "1.0",
                    ExportDate = DateTime.Now,
                    Profiles = new List<AutomationProfile> { profile },
                    Actions = new List<GameAction>()
                };

                var json = JsonConvert.SerializeObject(exportData, Formatting.Indented);
                File.WriteAllText(filePath, json);
                
                logger.Info($"Exported profile '{profile.Name}' to {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to export profile to {filePath}");
                return false;
            }
        }

        public ExportData? ImportFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    logger.Warn($"Import file not found: {filePath}");
                    return null;
                }

                var json = File.ReadAllText(filePath);
                var importData = JsonConvert.DeserializeObject<ExportData>(json);
                
                if (importData == null)
                {
                    logger.Warn("Failed to deserialize import file");
                    return null;
                }

                importData.Profiles ??= new List<AutomationProfile>();
                importData.Actions ??= new List<GameAction>();

                foreach (var profile in importData.Profiles)
                {
                    profile.Id = Guid.NewGuid();
                    foreach (var action in profile.Actions)
                    {
                        action.Id = Guid.NewGuid();
                    }
                }

                foreach (var action in importData.Actions)
                {
                    action.Id = Guid.NewGuid();
                }

                logger.Info($"Imported {importData.Profiles.Count} profiles and {importData.Actions.Count} actions from {filePath}");
                return importData;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to import from {filePath}");
                return null;
            }
        }
    }
}
