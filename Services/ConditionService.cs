using System;
using System.Diagnostics;
using System.IO;
using AutomationProfileManager.Models;
using Playnite.SDK;

namespace AutomationProfileManager.Services
{
    public class ConditionService
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public bool EvaluateCondition(ActionCondition? condition)
        {
            if (condition == null || condition.Type == ConditionType.None)
            {
                return true;
            }

            try
            {
                switch (condition.Type)
                {
                    case ConditionType.ProcessRunning:
                        return IsProcessRunning(condition.Value);

                    case ConditionType.ProcessNotRunning:
                        return !IsProcessRunning(condition.Value);

                    case ConditionType.FileExists:
                        return FileExists(condition.Value);

                    case ConditionType.FileNotExists:
                        return !FileExists(condition.Value);

                    case ConditionType.TimeRange:
                        return IsWithinTimeRange(condition.TimeStart, condition.TimeEnd);

                    default:
                        return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to evaluate condition: {condition.Type}");
                return true;
            }
        }

        private bool IsProcessRunning(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
                return false;

            processName = Path.GetFileNameWithoutExtension(processName);
            var processes = Process.GetProcessesByName(processName);
            bool isRunning = processes.Length > 0;
            
            logger.Info($"Condition check: Process '{processName}' running = {isRunning}");
            return isRunning;
        }

        private bool FileExists(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            string expandedPath = Environment.ExpandEnvironmentVariables(filePath);
            bool exists = File.Exists(expandedPath) || Directory.Exists(expandedPath);
            
            logger.Info($"Condition check: File/Directory '{expandedPath}' exists = {exists}");
            return exists;
        }

        private bool IsWithinTimeRange(string startTime, string endTime)
        {
            if (string.IsNullOrWhiteSpace(startTime) || string.IsNullOrWhiteSpace(endTime))
                return true;

            try
            {
                var now = DateTime.Now.TimeOfDay;
                var start = TimeSpan.Parse(startTime);
                var end = TimeSpan.Parse(endTime);

                bool isWithin;
                if (start <= end)
                {
                    isWithin = now >= start && now <= end;
                }
                else
                {
                    isWithin = now >= start || now <= end;
                }

                logger.Info($"Condition check: Time {now:hh\\:mm} within {startTime}-{endTime} = {isWithin}");
                return isWithin;
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"Failed to parse time range: {startTime} - {endTime}");
                return true;
            }
        }
    }
}
