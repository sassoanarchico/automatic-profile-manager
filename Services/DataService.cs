using System;
using System.Collections.Generic;
using System.IO;
using AutomationProfileManager.Models;
using Newtonsoft.Json;
using Playnite.SDK;

namespace AutomationProfileManager.Services
{
    public class DataService
    {
        private readonly IPlayniteAPI playniteAPI;
        private readonly string dataPath;
        private const string DataFileName = "automation_data.json";
        // Usa l'ID GUID dell'estensione per garantire coerenza tra aggiornamenti
        private const string ExtensionId = "A1B2C3D4-E5F6-7890-ABCD-EF1234567890";

        public DataService(IPlayniteAPI api)
        {
            playniteAPI = api;
            // Usa l'ID dell'estensione come nome cartella per evitare problemi di percorso
            dataPath = Path.Combine(api.Paths.ExtensionsDataPath, ExtensionId);
            
            // Migrazione: se esiste la vecchia cartella, migra i dati
            var oldDataPath = Path.Combine(api.Paths.ExtensionsDataPath, "AutomationProfileManager");
            if (Directory.Exists(oldDataPath) && !Directory.Exists(dataPath))
            {
                try
                {
                    Directory.Move(oldDataPath, dataPath);
                }
                catch
                {
                    // Se la migrazione fallisce, copia i file
                    Directory.CreateDirectory(dataPath);
                    foreach (var file in Directory.GetFiles(oldDataPath))
                    {
                        File.Copy(file, Path.Combine(dataPath, Path.GetFileName(file)), true);
                    }
                }
            }
            
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }
        }

        public ExtensionData LoadData()
        {
            var filePath = Path.Combine(dataPath, DataFileName);
            
            if (!File.Exists(filePath))
            {
                return new ExtensionData();
            }

            try
            {
                var json = File.ReadAllText(filePath);
                var data = JsonConvert.DeserializeObject<ExtensionData>(json) ?? new ExtensionData();
                NormalizeData(data);
                return data;
            }
            catch (Exception ex)
            {
                playniteAPI.Notifications.Add(new NotificationMessage(
                    "AutomationProfileManager_LoadError",
                    $"Failed to load data: {ex.Message}",
                    NotificationType.Error
                ));
                return new ExtensionData();
            }
        }

        private void NormalizeData(ExtensionData data)
        {
            if (data == null) return;
            
            if (data.ActionLibrary == null)
            {
                data.ActionLibrary = new List<GameAction>();
            }
            
            if (data.Profiles == null)
            {
                data.Profiles = new List<AutomationProfile>();
            }
            
            if (data.Mappings == null)
            {
                data.Mappings = new ProfileMapping();
            }
            
            if (data.Mappings.GameToProfile == null)
            {
                data.Mappings.GameToProfile = new Dictionary<Guid, Guid>();
            }
            
            if (data.Settings == null)
            {
                data.Settings = new ExtensionSettings();
            }
            
            if (data.ActionLog == null)
            {
                data.ActionLog = new List<ActionLogEntry>();
            }
        }

        public void SaveData(ExtensionData data)
        {
            var filePath = Path.Combine(dataPath, DataFileName);
            
            try
            {
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                playniteAPI.Notifications.Add(new NotificationMessage(
                    "AutomationProfileManager_SaveError",
                    $"Failed to save data: {ex.Message}",
                    NotificationType.Error
                ));
            }
        }
    }
}
