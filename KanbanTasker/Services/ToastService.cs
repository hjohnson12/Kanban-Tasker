using KanbanTasker.Model;
using KanbanTasker.Model.Services;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace KanbanTasker.Services
{
    /// <summary>
    /// A class to schedule and show toast notifications in UWP
    /// </summary>
    public class ToastService : IToastService
    {
        /// <summary>
        /// Schedules a reminder notification at the specified time for a task that's due or soon-to-be due. <br />
        /// <p>The scheduled alarm time must be at least 5 seconds later than the current day
        /// and time otherwise it will not be scheduled.</p>
        /// </summary>
        /// <param name="taskDto">Task to schedule the notification for</param>
        /// <param name="scheduledTime">Scheduled time for the toast notification</param>
        /// <param name="dueDate">Due date of the toast</param>
        public void ScheduleToast(TaskDTO taskDto, DateTimeOffset scheduledTime, DateTimeOffset dueDate)
        {
            // Verify that the scheduled alarm is after the current time
            if (scheduledTime > DateTime.Now.AddSeconds(5))
            {
                var toastContent = ConstructToastContent(taskDto.Title, taskDto.Description, dueDate);

                // Create the toast notification and scheudle it
                // Use the task's unique ID as the tag to reference the toast later on
                var scheduledNotification = new ScheduledToastNotification(toastContent.GetXml(), scheduledTime);
                scheduledNotification.Tag = taskDto.Id.ToString();

                ToastNotificationManager.CreateToastNotifier().AddToSchedule(scheduledNotification);
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
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                            {
                                new AdaptiveText()
                                { // Toast Header Title
                                    Text = "Task Due"
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

        /// <summary>
        /// Removes the scheduled toast notification uniquely identified by its tag from the schedule, if there are
        /// any notifications scheduled by that tag.
        /// </summary>
        /// <param name="tag"></param>
        public void RemoveScheduledToast(string tag)
        {
            var scheduledNotifications = 
                ToastNotificationManager.CreateToastNotifier().GetScheduledToastNotifications();

            foreach (ScheduledToastNotification notification in scheduledNotifications)
            {
                // The tag value is the unique ScheduledTileNotification.Id assigned to the 
                // notification when it was created
                if (notification.Tag == tag)
                    ToastNotificationManager.CreateToastNotifier().RemoveFromSchedule(notification);
            }
        }

        /// <summary>
        /// Displays a toast notification with the given title and text.
        /// </summary>
        /// <param name="title">Title shown on the toast notification</param>
        /// <param name="text">Text shown on the toast noficiation.</param>
        public void ShowToastNotification(string title, string text)
        {
            throw new NotImplementedException();
        }
    }
}