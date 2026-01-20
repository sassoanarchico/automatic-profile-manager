using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutomationProfileManager.Models;
using Playnite.SDK;

namespace AutomationProfileManager.Services
{
    public class StatisticsService
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private List<ActionStatistics> actionStats;
        private List<ProfileStatistics> profileStats;

        public StatisticsService(List<ActionStatistics> actions, List<ProfileStatistics> profiles)
        {
            actionStats = actions ?? new List<ActionStatistics>();
            profileStats = profiles ?? new List<ProfileStatistics>();
        }

        public void RecordActionExecution(GameAction action, bool success, double executionTimeMs)
        {
            try
            {
                var stat = actionStats.FirstOrDefault(s => s.ActionId == action.Id);
                if (stat == null)
                {
                    stat = new ActionStatistics
                    {
                        ActionId = action.Id,
                        ActionName = action.Name,
                        FirstExecution = DateTime.Now
                    };
                    actionStats.Add(stat);
                }

                stat.ExecutionCount++;
                stat.TotalExecutionTimeMs += executionTimeMs;
                stat.LastExecution = DateTime.Now;

                if (success)
                    stat.SuccessCount++;
                else
                    stat.FailureCount++;
            }
            catch (Exception ex)
            {
                logger.Warn(ex, LocalizationService.GetString("LOC_APM_Log_FailedToRecordActionStats"));
            }
        }

        public void RecordProfileExecution(AutomationProfile profile, double timeSavedSeconds)
        {
            try
            {
                var stat = profileStats.FirstOrDefault(s => s.ProfileId == profile.Id);
                if (stat == null)
                {
                    stat = new ProfileStatistics
                    {
                        ProfileId = profile.Id,
                        ProfileName = profile.Name
                    };
                    profileStats.Add(stat);
                }

                stat.ExecutionCount++;
                stat.TotalTimeSavedSeconds += timeSavedSeconds;
                stat.LastUsed = DateTime.Now;
            }
            catch (Exception ex)
            {
                logger.Warn(ex, LocalizationService.GetString("LOC_APM_Log_FailedToRecordProfileStats"));
            }
        }

        public double GetTotalTimeSaved()
        {
            return profileStats.Sum(s => s.TotalTimeSavedSeconds);
        }

        public int GetTotalActionsExecuted()
        {
            return actionStats.Sum(s => s.ExecutionCount);
        }

        public List<ActionStatistics> GetMostUsedActions(int count = 10)
        {
            return actionStats
                .OrderByDescending(s => s.ExecutionCount)
                .Take(count)
                .ToList();
        }

        public List<ActionStatistics> GetMostFailingActions(int count = 10)
        {
            return actionStats
                .Where(s => s.FailureCount > 0)
                .OrderByDescending(s => s.FailureCount)
                .Take(count)
                .ToList();
        }

        public double GetAverageSuccessRate()
        {
            var total = actionStats.Sum(s => s.ExecutionCount);
            if (total == 0) return 100;
            var success = actionStats.Sum(s => s.SuccessCount);
            return (double)success / total * 100;
        }

        public StatisticsSummary GetSummary()
        {
            return new StatisticsSummary
            {
                TotalActionsExecuted = GetTotalActionsExecuted(),
                TotalTimeSavedSeconds = GetTotalTimeSaved(),
                AverageSuccessRate = GetAverageSuccessRate(),
                TotalProfiles = profileStats.Count,
                MostUsedActions = GetMostUsedActions(5),
                MostFailingActions = GetMostFailingActions(5)
            };
        }
    }

    public class StatisticsSummary
    {
        public int TotalActionsExecuted { get; set; }
        public double TotalTimeSavedSeconds { get; set; }
        public double AverageSuccessRate { get; set; }
        public int TotalProfiles { get; set; }
        public List<ActionStatistics> MostUsedActions { get; set; } = new List<ActionStatistics>();
        public List<ActionStatistics> MostFailingActions { get; set; } = new List<ActionStatistics>();

        public string FormattedTimeSaved
        {
            get
            {
                var ts = TimeSpan.FromSeconds(TotalTimeSavedSeconds);
                if (ts.TotalHours >= 1)
                    return string.Format(LocalizationService.GetString("LOC_APM_Stats_Hours"), ts.TotalHours);
                else if (ts.TotalMinutes >= 1)
                    return string.Format(LocalizationService.GetString("LOC_APM_Stats_Minutes"), ts.TotalMinutes);
                else
                    return string.Format(LocalizationService.GetString("LOC_APM_Stats_Seconds"), ts.TotalSeconds);
            }
        }
    }
}
