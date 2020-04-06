using KanbanTasker.Helpers.Extensions;
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
using Windows.UI.Xaml.Media;
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

            var brushColor = (Application.Current.Resources["RegionBrush"] as AcrylicBrush);
            DueDateCalendarPicker.Background = brushColor;

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
            
            // Schedule toast notification if user chose a due date and reminder time
            // Note: UWP TimePicker doesn't support Nullable values, defaults to a value either way
            //var dueDate = ConvertToDateTimeOffset(ViewModel.CurrentTask.DueDate);
            //var reminderTime = ConvertToDateTimeOffset(ViewModel.CurrentTask.ReminderTime);
            //if (dueDate != null && reminderTime != null)
            //    ScheduleToastNotification(dueDate, reminderTime);
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
                autoSuggestBoxTags.ItemsSource = ViewModel.SuggestedTagsCollection;
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
                autoSuggestBoxTags.ItemsSource = ViewModel.SuggestedTagsCollection;
            }
        }

        private void autoSuggestBoxTags_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Test
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var results = ViewModel.SuggestedTagsCollection.Where(i => i.StartsWith(sender.Text)).ToList();

                if (results.Count > 0)
                    sender.ItemsSource = results;
                else
                {
                    results.Add(sender.Text);
                    sender.ItemsSource = results;
                }
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

                        var dueDate = ViewModel.CurrentTask.DueDate.ToNullableDateTimeOffset();
                        var timeDue = ViewModel.CurrentTask.TimeDue.ToNullableDateTimeOffset();
                        DateTimeOffset? today = DateTimeOffset.Now;

                        DateTimeOffset taskDueDate = new DateTimeOffset(
                          dueDate.Value.Year, dueDate.Value.Month, dueDate.Value.Day,
                          timeDue.Value.Hour, timeDue.Value.Minute, timeDue.Value.Second,
                          timeDue.Value.Offset
                        );

                        if (today > taskDueDate)
                        {
                            var brush = new SolidColorBrush(Windows.UI.Colors.Red);
                            brush.Opacity = 0.6;
                            (DueDateCalendarPicker.Background) = brush;
                        }
                        else
                        {
                            var brushColor = (Application.Current.Resources["RegionBrush"] as AcrylicBrush);
                            DueDateCalendarPicker.Background = brushColor;
                        }

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
            //var dueDate = ConvertToDateTimeOffset(ViewModel.CurrentTask.DueDate);
            //var reminderTime = ConvertToDateTimeOffset(ViewModel.CurrentTask.ReminderTime);
            //if (dueDate != null && reminderTime != null)
            //    ScheduleToastNotification(dueDate, reminderTime);
            //else
            //    ViewModel.ShowInAppNotification("Failed to schedule toast notification. Due date and/or alarm time not set, please try again.");
        }

        private void TaskReminderTimePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.NewTime.ToString()))
                return;
            else
                ViewModel.SetTimeDue(e.NewTime.ToString());
        }

        private void autoSuggestBoxTags_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            // When a user navigates through the suggestion list using the keyboard, you need to update the text in the text box to match.
            // In this case, we're just adding it to the tag collection automatically
        }

        private void autoSuggestBoxTags_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as AutoSuggestBox).IsSuggestionListOpen = true;
        }

        #endregion UIEvents
        public void ShowContextMenu(PresentationTask selectedModel)
        {
            // Workaround to show context menu next to selected card model
            foreach (var col in kanbanBoard.ActualColumns)
            {
                if (col.Categories.Contains(selectedModel.Category.ToString()))
                {
                    // Find card inside column
                    foreach (var card in col.Cards)
                    {
                        int cardIndex = 0;
                        var cardModel = card.Content as PresentationTask;
                        if (cardModel.ID == selectedModel.ID)
                        {
                            // Get current index of card and set on selected card
                            cardIndex = col.Cards.IndexOf(card);
                            FlyoutShowOptions myOption = new FlyoutShowOptions();
                            myOption.ShowMode = FlyoutShowMode.Transient;
                            taskFlyout.ShowAt(col.Cards[cardIndex], myOption);
                        }
                    }
                }
            }
        }
        private void Card_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            // Pre: Get information to pass to the dialog for displaying
            //      Set corresponding properties in TaskDialog
            // Post: Information passed, dialog opened

            // Always show in standard mode
            var originalSource = (FrameworkElement)sender;
            var selectedCard = originalSource.DataContext as PresentationTask;
            ViewModel.CurrentTask = selectedCard;
            ShowContextMenu(selectedCard);
        }

        private void FlyoutBtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Hide flyout
            taskFlyout.Hide();

            // Open pane if closed
            if (splitView.IsPaneOpen == false)
                splitView.IsPaneOpen = true;

            // Give title textbox focus once pane opens
            txtBoxTitle.Focus(FocusState.Programmatic);
            txtBoxTitle.SelectionStart = txtBoxTitle.Text.Length;
            txtBoxTitle.SelectionLength = 0;
        }

        private void TappedFlyoutBtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Hide flyout
            taskFlyout.Hide();

            // hack, command binding isn't working??
            ViewModel.EditTaskCommandHandler(ViewModel.CurrentTask.ID); 

            // Open pane if closed
            if (splitView.IsPaneOpen == false)
                splitView.IsPaneOpen = true;

            // Give title textbox focus once pane opens
            txtBoxTitle.Focus(FocusState.Programmatic);
            txtBoxTitle.SelectionStart = txtBoxTitle.Text.Length;
            txtBoxTitle.SelectionLength = 0;
        }


        private void tappedFlyoutBtnDeleteCardYes_Click(object sender, RoutedEventArgs e)
        {
            splitView.IsPaneOpen = false;
            taskFlyout.Hide();
            tappedFlyoutDeleteCard.Hide();

            // hack, binding not working
            ViewModel.DeleteTaskCommandHandler(ViewModel.CurrentTask.ID);
        }
    }
}
