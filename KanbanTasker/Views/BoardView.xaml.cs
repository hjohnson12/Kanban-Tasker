using KanbanTasker.Models;
using KanbanTasker.ViewModels;
using Syncfusion.UI.Xaml.Kanban;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
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
            //if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && sender.Text != "")
            //{
            //    var autoSuggestBoxTags = sender as AutoSuggestBox;
            //    List<string> suggestions = new List<string>()
            //    {
            //        sender.Text,
            //        sender.Text + "2"
            //    };
            //    autoSuggestBoxTags.ItemsSource = suggestions;
            //}
        }

        private void CalendarPicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            var calendarName = sender.Name.ToString();
            if (string.IsNullOrEmpty(args.NewDate.ToString()))
                return;
            else
            {
                var datePicked = args.NewDate.Value.Date.ToShortDateString();
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

        #endregion UIEvents
    }
}
