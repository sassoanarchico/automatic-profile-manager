using System;
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

        public DataService(IPlayniteAPI api)
        {
            playniteAPI = api;
            dataPath = Path.Combine(api.Paths.ExtensionsDataPath, "AutomationProfileManager");
            
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
                return JsonConvert.DeserializeObject<ExtensionData>(json) ?? new ExtensionData();
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
