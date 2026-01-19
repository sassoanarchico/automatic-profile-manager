using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Playnite.SDK;

namespace AutomationProfileManager.Services
{
    public class NotificationService
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IPlayniteAPI playniteApi;
        private bool showNotifications = true;

        public NotificationService(IPlayniteAPI api)
        {
            playniteApi = api;
        }

        public void SetShowNotifications(bool show)
        {
            showNotifications = show;
        }

        public void ShowProfileStarted(string profileName, string gameName, int actionCount)
        {
            if (!showNotifications) return;

            try
            {
                playniteApi.Notifications.Add(new NotificationMessage(
                    $"AutomationProfileManager_Start_{Guid.NewGuid()}",
                    $"?? Profilo '{profileName}' avviato per {gameName}\n{actionCount} azioni in esecuzione...",
                    NotificationType.Info
                ));
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "Failed to show profile started notification");
            }
        }

        public void ShowProfileCompleted(string profileName, int successCount, int failCount, double elapsedSeconds)
        {
            if (!showNotifications) return;

            try
            {
                string status = failCount == 0 ? "?" : "??";
                string message = $"{status} Profilo '{profileName}' completato\n" +
                                 $"Azioni: {successCount} OK, {failCount} errori\n" +
                                 $"Tempo: {elapsedSeconds:F1}s";

                playniteApi.Notifications.Add(new NotificationMessage(
                    $"AutomationProfileManager_Complete_{Guid.NewGuid()}",
                    message,
                    failCount == 0 ? NotificationType.Info : NotificationType.Error
                ));
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "Failed to show profile completed notification");
            }
        }

        public void ShowActionResult(string actionName, bool success, string message)
        {
            if (!showNotifications) return;

            try
            {
                string status = success ? "?" : "?";
                playniteApi.Notifications.Add(new NotificationMessage(
                    $"AutomationProfileManager_Action_{Guid.NewGuid()}",
                    $"{status} {actionName}: {message}",
                    success ? NotificationType.Info : NotificationType.Error
                ));
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "Failed to show action result notification");
            }
        }

        public void ShowError(string title, string message)
        {
            try
            {
                playniteApi.Notifications.Add(new NotificationMessage(
                    $"AutomationProfileManager_Error_{Guid.NewGuid()}",
                    $"? {title}: {message}",
                    NotificationType.Error
                ));
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "Failed to show error notification");
            }
        }

        public void ShowBackupCompleted(string backupPath)
        {
            if (!showNotifications) return;

            try
            {
                playniteApi.Notifications.Add(new NotificationMessage(
                    $"AutomationProfileManager_Backup_{Guid.NewGuid()}",
                    $"?? Backup configurazione completato:\n{backupPath}",
                    NotificationType.Info
                ));
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "Failed to show backup notification");
            }
        }
    }
}
