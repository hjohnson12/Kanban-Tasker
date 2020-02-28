using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace KanbanTasker.Helpers
{
    /// <summary>
    /// A class used to work with toast notifications on tasks.
    /// </summary>
    public static class ToastHelper
    {
        /// <summary>
        /// Schedules a toast notification on the due date of the specified task.
        /// </summary>
        /// <param name="dueDate"></param>
        /// <param name="reminderTime"></param>
        public static void ScheduleTaskNotification(string taskId, string taskTitle, string taskDescription, DateTimeOffset? dueDate, DateTimeOffset? reminderTime)
        {
            DateTimeOffset alarmTime = new DateTimeOffset(
               dueDate.Value.Year, dueDate.Value.Month, dueDate.Value.Day,
               reminderTime.Value.Hour, reminderTime.Value.Minute, reminderTime.Value.Second,
               reminderTime.Value.Offset
           );

            // Verify that the alarm is after the current time
            if (alarmTime > DateTime.Now.AddSeconds(5))
            {
                // Construct toast notification content
                var toastContent = ConstructToastContent(taskTitle, taskDescription, alarmTime);

                // Create the toast notification and scheudle it
                // Use the task's unique ID as the tag to reference the toast later on
                var scheduledNotif = new ScheduledToastNotification(toastContent.GetXml(), alarmTime);
                scheduledNotif.Tag = taskId;
                ToastNotificationManager.CreateToastNotifier().AddToSchedule(scheduledNotif);
            }
        }

        /// <summary>
        /// Removes the scheduled toast notification uniquely identified by its tag from the schedule.
        /// </summary>
        /// <param name="tag"></param>
        public static void RemoveScheduledNotification(string tag)
        {
            var scheduledNotifs = ToastNotificationManager.CreateToastNotifier().GetScheduledToastNotifications();
            foreach (var notif in scheduledNotifs) 
            {
                // The tag value is the unique ScheduledTileNotification.Id assigned to the 
                // notification when it was created.
                if (notif.Id == tag)
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
