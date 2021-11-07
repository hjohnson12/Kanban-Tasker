using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Syncfusion.UI.Xaml.Kanban;
using KanbanTasker.Models;
using KanbanTasker.ViewModels;
using System.Collections.Generic;
using KanbanTasker.Helpers;
using Windows.UI.Xaml.Data;

namespace KanbanTasker.Views
{
    public sealed partial class BoardView : Page
    {
        public BoardViewModel ViewModel { get; set; }

        public BoardView()
        {
            this.InitializeComponent();

            kanbanBoard.CardStyle.CornerRadius = new CornerRadius(3);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set view model upon navigation
            var selectedBoard = e.Parameter as BoardViewModel;
            ViewModel = selectedBoard;
            DataContext = ViewModel;

            ConfigureColumns();
        }

        private void ConfigureColumns()
        {
            ViewModel.ConfigureBoardColumns();
            var columns = ViewModel.Columns;

            // Set bindings and add columns to board
            foreach (var column in columns)
            {
                CustomKanbanColumn newColumn = new CustomKanbanColumn()
                {
                    Title = column.ColumnName,
                    Categories = column.ColumnName,
                    CollapsedColumnTemplate = this.Resources["CollapsedColumnTemplate"] as ControlTemplate,
                    MaximumLimit = column.MaxTaskLimit
                };

                var myBinding = new Binding()
                {
                    Path = new PropertyPath("ColumnName"),
                    Source = column,
                    Mode = BindingMode.OneWay
                };

                var myBinding2 = new Binding()
                {
                    Path = new PropertyPath("MaxTaskLimit"),
                    Source = column,
                    Mode = BindingMode.TwoWay
                };

                // Set bindings
                newColumn.SetBinding(KanbanColumn.TitleProperty, myBinding);
                newColumn.SetBinding(KanbanColumn.CategoriesProperty, myBinding);
                newColumn.SetBinding(KanbanColumn.MaximumLimitProperty, myBinding2);

                kanbanBoard.Columns.Add(newColumn);
            }
        }

        private void CardBtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Give title textbox focus once pane opens
            txtBoxTitle.Focus(FocusState.Programmatic);
            txtBoxTitle.SelectionStart = txtBoxTitle.Text.Length;
            txtBoxTitle.SelectionLength = 0;
        }

        private void CardBtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Show flyout attached to button
            // Delete task if "Yes" button is clicked inside flyout
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void BtnNewTaskCurrentColumn_Click(object sender, RoutedEventArgs e)
        {
            var brushColor = (Application.Current.Resources["RegionBrush"] as AcrylicBrush);
            DueDateCalendarPicker.Background = brushColor;

            txtBoxTitle.Focus(FocusState.Programmatic);
        }

        private void KanbanBoard_CardDragEnd(object sender, KanbanDragEndEventArgs e)
        {
            // Change column and category of task when dragged to new column
            // ObservableCollection Tasks already updated
            var targetCategory = e.TargetKey.ToString();
            var selectedCardModel = e.SelectedCard.Content as PresentationTask;
            int sourceCardIndex = e.SelectedCardIndex;
            int targetCardIndex = e.TargetCardIndex;

            // Update item in collection and database
            var selectedTask = ViewModel.Board.Tasks.FirstOrDefault(x => x.ID == selectedCardModel.ID);
            selectedTask.ColumnIndex = targetCardIndex;
            //t2.Category = targetCategory;

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

                        if (currentModel == null) 
                            return;

                        // Update task in collection, otherwise stale data
                        var task = ViewModel.Board.Tasks
                            .FirstOrDefault(x => x.ID == currentModel.ID);
                        task.ColumnIndex = currentIndex;

                        ViewModel.UpdateCardIndex(currentModel.ID, currentIndex);
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

                            if (currentModel == null)
                                return;

                            // Update task in collection, otherwise stale data
                            var task = ViewModel.Board.Tasks
                                .FirstOrDefault(x => x.ID == currentModel.ID);
                            task.ColumnIndex = currentIndex;

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

                        // Update task in collection, otherwise stale data
                        var task = ViewModel.Board.Tasks
                            .FirstOrDefault(x => x.ID == currentModel.ID);
                        task.ColumnIndex = currentIndex;
                        
                        ViewModel.UpdateCardIndex(currentModel.ID, currentIndex);
                    }
                }
            }
        }

        private void FlyoutDeleteCardBtnYes_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.IsPaneOpen = false;
        }

        private void PaneBtnDeleteTaskYes_Click(object sender, RoutedEventArgs e)
        {
            // Close pane when finished
            ViewModel.IsPaneOpen = false;
            PaneBtnDeleteTaskConfirmationFlyout.Hide();
        }

        private void btnDeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (PaneBtnDeleteTaskConfirmationFlyout.IsOpen)
                PaneBtnDeleteTaskConfirmationFlyout.Hide();
            else
                PaneBtnDeleteTaskConfirmationFlyout.ShowAt((FrameworkElement)sender);
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
                var results = ViewModel.SuggestedTagsCollection
                    .Where(i => i.StartsWith(sender.Text))
                    .ToList();

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

        private void TaskReminderTimePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.NewTime.ToString()))
                return;

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

        private void Card_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            // Display context menu to the side of card on tap
            var originalSource = (FrameworkElement)sender;
            var selectedCard = originalSource.DataContext as PresentationTask;

            ViewModel.IsPaneOpen = false;
            ViewModel.CurrentTask = selectedCard;

            ShowContextMenu(selectedCard);
        }

        private void FlyoutBtnEdit_Click(object sender, RoutedEventArgs e)
        {
            taskFlyout.Hide();

            // Give title textbox focus once pane opens
            txtBoxTitle.Focus(FocusState.Programmatic);
            txtBoxTitle.SelectionStart = txtBoxTitle.Text.Length;
            txtBoxTitle.SelectionLength = 0;
        }

        private void TappedFlyoutBtnEdit_Click(object sender, RoutedEventArgs e)
        {
            taskFlyout.Hide();

            // Give title textbox focus once pane opens
            txtBoxTitle.Focus(FocusState.Programmatic);
            txtBoxTitle.SelectionStart = txtBoxTitle.Text.Length;
            txtBoxTitle.SelectionLength = 0;
        }

        private void tappedFlyoutBtnDeleteCardYes_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.IsPaneOpen = false;
            taskFlyout.Hide();
            tappedFlyoutDeleteCard.Hide();

            // hack, binding not working
            ViewModel.DeleteTask(ViewModel.CurrentTask.ID);
        }

        public void ShowContextMenu(PresentationTask selectedModel)
        {
            // Workaround to show context menu next to selected card model
            foreach (var column in kanbanBoard.ActualColumns)
            {
                if (column.Categories.Contains(selectedModel.Category.ToString()))
                {
                    foreach (var card in column.Cards)
                    {
                        var cardModel = card.Content as PresentationTask;

                        if (cardModel.ID == selectedModel.ID)
                        {
                            // Get current index of card and set on selected card
                            int cardIndex = column.Cards.IndexOf(card);
                            FlyoutShowOptions myOption = new FlyoutShowOptions();
                            myOption.ShowMode = FlyoutShowMode.Transient;
                            taskFlyout.ShowAt(column.Cards[cardIndex], myOption);
                        }
                    }
                }
            }
        }

        private void btnSaveColChanges_Click(object sender, RoutedEventArgs e)
        {
            var header =
                ((sender as Button).CommandParameter).ToString();

            var newColName = ViewModel.NewColumnName;
            var newMaxLimit = ViewModel.NewColumnMax;

            if (string.IsNullOrEmpty(newColName))
                newColName = header;

            ViewModel.UpdateColumn(header);

            var oldColumnExists = kanbanBoard.Columns.Any(x => x.Title.Equals(header));
            if (oldColumnExists)
            {
                // Update title/categories to keep UI consistent
                var column = kanbanBoard.Columns.Single(x => x.Title.Equals(header));
                column.Categories = newColName;
                column.Title = newColName;
                column.MaximumLimit = newMaxLimit;
            }

            flyoutEditColumn.Hide();
        }

        private void txtBoxColName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.NewColumnName = (sender as TextBox).Text;
        }

        private void txtBoxColName_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).Select((sender as TextBox).Text.Length, 0);
        }

        private void btnEditColumn_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void maxLimitNumberBox_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            ViewModel.NewColumnMax = Convert.ToInt32((sender).Value);
        }

        private void btnDeleteColumn_Click(object sender, RoutedEventArgs e)
        {
            var columnName = ((sender as Button).CommandParameter as ColumnTag).Header.ToString();
            
            // Delete column from database and collection
            // Reorders the remaining column positions
            var isDeleteSuccessful = ViewModel.DeleteColumn(columnName);

            if (isDeleteSuccessful)
            {
                // Remove column from control
                var column = kanbanBoard.Columns.Single(x => x.Title.Equals(columnName));
                kanbanBoard.Columns.Remove(column);
            }
        }

        private async void AddColumnButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new NewColumnDialog();
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var columnName = dialog.ColumnName;
                var maxTaskLimit = dialog.MaxTaskLimit;

                // Verify variable data is in correct format
                if (ViewModel.Columns.Any(x => x.ColumnName.Equals(columnName)))
                {
                    ViewModel.ShowInAppNotification("A column with that name already exists");
                    return;
                }
                else if (columnName == "" || maxTaskLimit == 0)
                {
                    ViewModel.ShowInAppNotification("Please fill out the required fields");
                    return;
                }

                // Continue with creating column 
                var column = ViewModel.CreateColumn(columnName, maxTaskLimit);
                CustomKanbanColumn newColumn = new CustomKanbanColumn()
                {
                    Title = column.ColumnName,
                    Categories = column.ColumnName,
                    CollapsedColumnTemplate = this.Resources["CollapsedColumnTemplate"] as ControlTemplate,
                    MaximumLimit = column.MaxTaskLimit
                };

                var myBinding = new Binding()
                {
                    Path = new PropertyPath("ColumnName"),
                    Source = column,
                    Mode = BindingMode.OneWay
                };

                var myBinding2 = new Binding()
                {
                    Path = new PropertyPath("MaxTaskLimit"),
                    Source = column,
                    Mode = BindingMode.TwoWay
                };

                // Set bindings
                newColumn.SetBinding(KanbanColumn.TitleProperty, myBinding);
                newColumn.SetBinding(KanbanColumn.CategoriesProperty, myBinding);
                newColumn.SetBinding(KanbanColumn.MaximumLimitProperty, myBinding2);

                // Add to kanban columns
                kanbanBoard.Columns.Add(newColumn);
            }
        }
    }
}