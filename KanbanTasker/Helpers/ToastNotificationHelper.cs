using System;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;

namespace KanbanTasker.Helpers
{
    /// <summary>
    /// A helper class used to work with Windows 10 toast notifications.
    /// </summary>
    public static class ToastNotificationHelper
    {
        /// <summary>
        /// Schedules a reminder notification at the specified time for a task that's due or soon-to-be due. <br />
        /// The scheduled alarm time must be at least 5 seconds later than the current day
        /// and time otherwise it will not be scheduled.
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="taskTitle"></param>
        /// <param name="taskDescription"></param>
        /// <param name="scheduledAlarmTime">Scheduled time for the toast notification.</param>
        /// <param name="taskDueDate">Full due date of the task.</param>
        public static void ScheduleTaskDueNotification(string taskId, string taskTitle, string taskDescription, DateTimeOffset scheduledAlarmTime, DateTimeOffset taskDueDate)
        {
            // Verify that the scheduled alarm is after the current time
            if (scheduledAlarmTime > DateTime.Now.AddSeconds(5))
            {
                var toastContent = ConstructToastContent(taskTitle, taskDescription, taskDueDate);

                // Create the toast notification and scheudle it
                // Use the task's unique ID as the tag to reference the toast later on
                var scheduledNotification = new ScheduledToastNotification(toastContent.GetXml(), scheduledAlarmTime);
                scheduledNotification.Tag = taskId;
                ToastNotificationManager.CreateToastNotifier().AddToSchedule(scheduledNotification);
            }
        }

        /// <summary>
        /// Removes the scheduled toast notification uniquely identified by its tag from the schedule, if there are
        /// any notifications scheduled by that tag.
        /// </summary>
        /// <param name="tag"></param>
        public static void RemoveScheduledNotification(string tag)
        {
            var scheduledNotifications = ToastNotificationManager.CreateToastNotifier().GetScheduledToastNotifications();
            foreach (var notif in scheduledNotifications) 
            {
                // The tag value is the unique ScheduledTileNotification.Id assigned to the 
                // notification when it was created
                if (notif.Tag == tag)
                    ToastNotificationManager.CreateToastNotifier().RemoveFromSchedule(notif);
            }
        }

        /// <summary>
        /// Builds and returns the toast notification content object.  
        /// </summary>
        /// <param name="alarmTime"></param>
        /// <returns>Returns a toast content object to be used for creating the toast notification.</returns>
        private static ToastContent ConstructToastContent(string taskTitle, string taskDescription, DateTimeOffset alarmTime)
        {
            return new ToastContent()
            {
                //DisplayTimestamp = new DateTime(alarmTime.Year, alarmTime.Month, alarmTime.Day, alarmTime.Hour, alarmTime.Minute, alarmTime.Second, DateTimeKind.Utc),
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                            {
                                new AdaptiveText()
                                { // Toast Header Title
                                    Text = "You have a task due"
                                },
                                new AdaptiveText()
                                { // Title of Task & Description
                                    Text = ((taskTitle != null) ? taskTitle : "") + "\n" +
                                           ((taskDescription != null) ? taskDescription : "")
                                },
                                new AdaptiveText()
                                { // Task Due Date
                                    Text = "Due " + alarmTime.ToString("t") + ", " + alarmTime.Date.ToShortDateString()
                                },

                            },
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = "ms-appx:///Assets/Square44x44Logo.targetsize-256.png",
                        },
                    }
                },
                Actions = new ToastActionsCustom()
                {
                    Inputs =
                        {
                            new ToastSelectionBox("snoozeTime")
                            {
                                DefaultSelectionBoxItemId = "15",
                                Items =
                                {
                                    new ToastSelectionBoxItem("1", "1 minute"),
                                    new ToastSelectionBoxItem("15", "15 minutes"),
                                    new ToastSelectionBoxItem("60", "1 hour"),
                                    new ToastSelectionBoxItem("240", "4 hours"),
                                    new ToastSelectionBoxItem("1440", "1 day")
                                }
                            }
                        },
                    Buttons =
                        {
                            new ToastButtonSnooze()
                            {
                                SelectionBoxId = "snoozeTime"
                            },
                            new ToastButtonDismiss()
                        }
                },
                Launch = "action=viewEvent&eventId=1983",
                Scenario = ToastScenario.Reminder
            };
        }
    }
}