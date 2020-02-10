using KanbanTasker.Models;
using KanbanTasker.ViewModels;
using Microsoft.Toolkit.Uwp.Notifications;
using Syncfusion.UI.Xaml.Kanban;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace KanbanTasker.Views
{
    public sealed partial class BoardView : Page
    {
        public BoardViewModel ViewModel { get; set; }

        public BoardView()
        {
            this.InitializeComponent();
            DataContext = ViewModel;
            kanbanBoard.CardStyle.CornerRadius = new CornerRadius(3);
        }

        #region Methods

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var selectedBoard = e.Parameter as BoardViewModel;
            ViewModel = selectedBoard;
        }

        public DateTimeOffset? ConvertToDateTimeOffset(string dateTime)
        {
            DateTimeOffset? dt = null;
            DateTime dt2;
            if (dateTime != null && dateTime is string)
            {
                var stringToConvert = dateTime as string;
                bool success = DateTime.TryParse(dateTime, out dt2);
                if (success)
                    dt = dt2;
                else
                    return dt;
            }
            return dt;
        }

        /// <summary>
        /// Initializes the ToastContent for the current task and schedules the toast notificiaton.
        /// </summary>
        /// <param name="dueDate"></param>
        /// <param name="reminderTime"></param>
        private void ScheduleToastNotification(DateTimeOffset? dueDate, DateTimeOffset? reminderTime)
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
                var toastContent = ConstructToastContent(alarmTime);

                // Create the toast notification
                var scheduledNotif = new ScheduledToastNotification(toastContent.GetXml(), alarmTime);

                // And schedule the notification
                ToastNotificationManager.CreateToastNotifier().AddToSchedule(scheduledNotif);
            }
        }

        /// <summary>
        /// Builds and returns the toast notification content object.  
        /// </summary>
        /// <param name="alarmTime"></param>
        /// <returns>Returns a toast content object to be used for creating the toast notification.</returns>
        public ToastContent ConstructToastContent(DateTimeOffset alarmTime)
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
                                { // Title
                                    Text = "You have a task due"
                                },
                                new AdaptiveText()
                                { // Description
                                    Text = ViewModel.CurrentTask.Title.ToString() + "\n" + ViewModel.CurrentTask.Description.ToString()
                                },
                                new AdaptiveText()
                                { // Time
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

        #endregion Methods

        #region UIEvents

        private void CardBtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (splitView.IsPaneOpen == false)
                splitView.IsPaneOpen = true;

            // Give title textbox focus once pane opens
            txtBoxTitle.Focus(FocusState.Programmatic);
            txtBoxTitle.SelectionStart = txtBoxTitle.Text.Length;
            txtBoxTitle.SelectionLength = 0;
        }

        private void CardBtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var originalSource = (FrameworkElement)sender;

            // Show flyout attached to button
            // Delete task if "Yes" button is clicked inside flyout
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void BtnNewTaskCurrentColumn_Click(object sender, RoutedEventArgs e)
        {
            // Open pane if not already
            if (splitView.IsPaneOpen == false)
                splitView.IsPaneOpen = true;

            txtBoxTitle.Focus(FocusState.Programmatic);
        }

        private void appBarBtnClosePane_Click(object sender, RoutedEventArgs e)
        {
            // Close pane when done
            if (splitView.IsPaneOpen == true)
                splitView.IsPaneOpen = false;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Close pane when done
            if (splitView.IsPaneOpen == true)
                splitView.IsPaneOpen = false;
        }

        private void BtnSaveTask_Click(object sender, RoutedEventArgs e)
        {
            // Close pane when done
            if (splitView.IsPaneOpen == true)
                splitView.IsPaneOpen = false;
            
            //Schedule toast notification if they chose a due date and reminder time
            // Note: UWP TimePicker doesn't support Nullable values, autoselects current time
            var dueDate = ConvertToDateTimeOffset(ViewModel.CurrentTask.DueDate);
            var reminderTime = ConvertToDateTimeOffset(ViewModel.CurrentTask.ReminderTime);
            if (dueDate != null && reminderTime != null)
                ScheduleToastNotification(dueDate, reminderTime);
        }

        private void TxtBoxTags_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            // Add Tag to listview on keydown event
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var tagsTextBox = sender as TextBox;

                if (string.IsNullOrEmpty(tagsTextBox.Text))
                    return;

                if (ViewModel.AddTag(tagsTextBox.Text))
                    tagsTextBox.Text = string.Empty;
            }
        }

        private void MnuItemExitApp_Click(object sender, RoutedEventArgs e)
        {
            CoreApplication.Exit();
        }

        private void KanbanBoard_CardDragEnd(object sender, KanbanDragEndEventArgs e)
        {
            // Change column and category of task when dragged to new column
            // ObservableCollection Tasks already updated
            var targetCategory = e.TargetKey.ToString();
            var selectedCardModel = e.SelectedCard.Content as PresentationTask;
            int sourceCardIndex = e.SelectedCardIndex;
            int targetCardIndex = e.TargetCardIndex;
            ViewModel.UpdateCardColumn(targetCategory, selectedCardModel, targetCardIndex);

            // Reorder cards when dragging to & from same column
            if (e.TargetColumn.Title.ToString() == e.SelectedColumn.Title.ToString())
            {
                // Update every card index in the column after rearrange
                foreach (var card in e.TargetColumn.Cards)
                {
                    int currentIndex = e.TargetColumn.Cards.IndexOf(card);
                    if (currentIndex != Convert.ToInt32(targetCardIndex))
                    {
                        var currentModel = card.Content as PresentationTask;
                        currentModel.ColumnIndex = currentIndex;
                        ViewModel.UpdateCardIndex(currentModel.ID, currentIndex);

                        // NOTE FROM DEBUGGING:

                        //  After its updated in the database, it's not updating the Tasks list? so next iteration when I delete second card, it's using old tasks?
                        // Found inside of (this) at runtime by viewing the ViewModel and PresentationBoard inside it
                        // Part of the bug explained in BoardViewModel.cs, Line 222. Low severity, current fixes stops the major crashing, this is just a hidden issue
                    }
                }
            }
            // Reorder cards when dragging from one column to another 
            else
            {
                // Reorder target col after drop
                // Only items above the targetCardIndex need to be updated
                if (e.TargetColumn.Cards.Count != 0)
                {
                    foreach (var card in e.TargetColumn.Cards)
                    {
                        int currentIndex = e.TargetColumn.Cards.IndexOf(card);
                        if (currentIndex > Convert.ToInt32(targetCardIndex))
                        {
                            var currentModel = card.Content as PresentationTask;
                            currentModel.ColumnIndex = currentIndex;
                            ViewModel.UpdateCardIndex(currentModel.ID, currentIndex);
                        }
                    }
                }

                // Reorder source column after dragged card is removed
                if (e.SelectedColumn.Cards.Count != 0)
                {
                    foreach (var card in e.SelectedColumn.Cards)
                    {
                        int currentIndex = e.SelectedColumn.Cards.IndexOf(card);
                        var currentModel = card.Content as PresentationTask;
                        currentModel.ColumnIndex = currentIndex;
                        ViewModel.UpdateCardIndex(currentModel.ID, currentIndex);
                    }
                }
            }
        }

        private void FlyoutDeleteCardBtnYes_Click(object sender, RoutedEventArgs e)
        {
            splitView.IsPaneOpen = false;
        }

        private void PaneBtnDeleteTaskYes_Click(object sender, RoutedEventArgs e)
        {
            // Close pane when done
            splitView.IsPaneOpen = false;
            PaneBtnDeleteTaskConfirmationFlyout.Hide();
        }

        private void btnDeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (PaneBtnDeleteTaskConfirmationFlyout.IsOpen)
                PaneBtnDeleteTaskConfirmationFlyout.Hide();
            else
                PaneBtnDeleteTaskConfirmationFlyout.ShowAt((FrameworkElement)sender);
            //FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender); 
        }

        private void autoSuggestBoxTags_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                // User selected an item from the suggestion list, take an action on it here.
                var autoSuggestBoxTags = sender as AutoSuggestBox;
                if (string.IsNullOrEmpty(args.ChosenSuggestion.ToString()))
                    return;
                if (ViewModel.AddTag(args.ChosenSuggestion.ToString()))
                    autoSuggestBoxTags.Text = string.Empty;
            }
            else if (!string.IsNullOrEmpty(args.QueryText))
            {
                // Use args.QueryText to determine what to do.

                // Currently works like the textbox did with Enter
                var autoSuggestBoxTags = sender as AutoSuggestBox;
                if (string.IsNullOrEmpty(args.QueryText))
                    return;
                if (ViewModel.AddTag(args.QueryText))
                    autoSuggestBoxTags.Text = string.Empty;
            }
        }

        private void autoSuggestBoxTags_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Test
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && sender.Text != "")
            {
                var results = ViewModel.Board.TagsCollection.Where(i => i.StartsWith(sender.Text)).ToList();
                autoSuggestBoxTags.ItemsSource = results;
                autoSuggestBoxTags.IsSuggestionListOpen = true;
            }
    }

        private void CalendarPicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            var calendarName = sender.Name.ToString();
            if (string.IsNullOrEmpty(args.NewDate.ToString()))
                return;
            else
            {
                //var datePicked = args.NewDate.Value.Date.ToShortDateString();
                var datePicked = args.NewDate.ToString();
                switch (calendarName)
                {
                    case "DueDateCalendarPicker":
                        ViewModel.SetDueDate(datePicked);
                        break;
                    case "StartDateCalendarPicker":
                        ViewModel.SetStartDate(datePicked);
                        break;
                    case "FinishDateCalendarPicker":
                        ViewModel.SetFinishDate(datePicked);
                        break;
                }
            }
        }

        private void BtnTestReminder_Click(object sender, RoutedEventArgs e)
        {
            var dueDate = ConvertToDateTimeOffset(ViewModel.CurrentTask.DueDate);
            var reminderTime = ConvertToDateTimeOffset(ViewModel.CurrentTask.ReminderTime);
            if (dueDate != null && reminderTime != null)
                ScheduleToastNotification(dueDate, reminderTime);
            else
                ViewModel.ShowInAppNotification("Failed to schedule toast notification. Due date and/or alarm time not set, please try again.");
        }

        private void TaskReminderTimePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.NewTime.ToString()))
                return;
            else
                ViewModel.SetReminderTime(e.NewTime.ToString());
        }

        #endregion UIEvents

        private void autoSuggestBoxTags_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (!string.IsNullOrEmpty(autoSuggestBoxTags.Text))
                autoSuggestBoxTags.Text = string.Empty;
        }

        private void autoSuggestBoxTags_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as AutoSuggestBox).IsSuggestionListOpen = true;
        }

        private async void lstViewTags_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Ideally, edit the tag through Edit Pane
            // User is able to select tag through the card too, should maybe handle that case
            // when CurrentTask would be null
            var tag = e.ClickedItem;
            ViewModel.CurrentTask.SelectedTag = tag.ToString();
            var dialog = new TagEditDialogView(ViewModel);
            await dialog.ShowAsync();
        }
    }
}
