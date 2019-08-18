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
        public CustomKanbanModel SelectedModel { get; set; }

        public BoardView()
        {
            this.InitializeComponent();

            ViewModel = App.mainViewModel.Current;

            // Add rounded corners to each card
            kanbanBoard.CardStyle.CornerRadius = new CornerRadius(10.0);
        }

        #region Methods

        public List<string> GetColorKeys(SfKanban kanban)
        {
            // Add color keys to a list
            // Displayed in a combobox in TaskDialog for user to choose
            // the color key for a task
            List<string> lstColorKeys = new List<string>();
            foreach (var colorMap in kanban.IndicatorColorPalette)
            {
                // Add each key from the color palette to the combobox
                var key = colorMap.Key;
                lstColorKeys.Add(key.ToString());
            }
            return lstColorKeys;
        }

        public ObservableCollection<string> GetTagCollection(CustomKanbanModel selectedModel)
        {
            // Add selected card tags to a collection
            // Tags Collection is displayed in a listview in TaskDialog 
            var tagsCollection = new ObservableCollection<string>();
            foreach (var tag in selectedModel.Tags)
                tagsCollection.Add(tag); // Add card tags to collection
            return tagsCollection;
        }

        private void UpdateCardIndexes()
        {
            // Look at every card in the current column and update their indexes in the db
            foreach (var col in kanbanBoard.ActualColumns)
            {
                // Compare to selected models category to determine current column
                if (col.Categories.Contains(SelectedModel.Category.ToString()))
                {
                    if (col.Cards.Count != 0)
                    {
                        foreach (var card in col.Cards)
                        {
                            var currentModel = card.Content as CustomKanbanModel;
                            var currentCardIndex = col.Cards.IndexOf(card);
                            ViewModel.UpdateCardIndex(currentModel.ID, currentCardIndex);
                        }
                    }
                }
            }
        }


        public void ShowContextMenu(CustomKanbanModel selectedModel)
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
                        var cardModel = card.Content as CustomKanbanModel;
                        if (cardModel.ID == selectedModel.ID)
                        {
                            // Get current index of card
                            cardIndex = col.Cards.IndexOf(card);
                        }

                        // Set flyout to selected card index
                        for (int i = 0; i <= col.Cards.Count; i++)
                        {
                            if (i == cardIndex)
                            {
                                FlyoutShowOptions myOption = new FlyoutShowOptions();
                                myOption.ShowMode = FlyoutShowMode.Transient;
                                taskFlyout.ShowAt(col.Cards[i], myOption);
                            }
                        }
                    }
                }
            }
        }
        #endregion Methods

        #region UIEvents

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var selectedBoard = e.Parameter as BoardViewModel;
            ViewModel = selectedBoard;
        }

        private void FlyoutBtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Call helper from ViewModel to handle model-related data
            ViewModel.EditTaskHelper(SelectedModel,
                GetColorKeys(kanbanBoard), GetTagCollection(SelectedModel));

            // UI RELATED CODE

            // Set selected items in combo box
            ViewModel.CurrentCategory = SelectedModel.Category.ToString();
            comboBoxColorKey.SelectedItem = SelectedModel.ColorKey;

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

        private void CardBtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var originalSource = (FrameworkElement)sender;
            SelectedModel = originalSource.DataContext as CustomKanbanModel;

            // Call helper from ViewModel to handle model-related data
            ViewModel.EditTaskHelper(SelectedModel,
                GetColorKeys(kanbanBoard), GetTagCollection(SelectedModel));

            // UI RELATED CODE

            // Set selected item in combo box
            ViewModel.CurrentCategory = SelectedModel.Category.ToString();
            comboBoxColorKey.SelectedItem = SelectedModel.ColorKey;

            taskFlyout.Hide();

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
            SelectedModel = originalSource.DataContext as CustomKanbanModel;

            // Show flyout attached to button
            // Delete task if "Yes" button is clicked inside flyout
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        /// <summary>
        /// Used for touch screen users, but works for PC users too
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void FlyoutBtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Hide flyout
            taskFlyout.Hide();

            // Create dialog and check button click result
            var deleteDialog = new DeleteConfirmationView();
            var result = await deleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Close pane when done
                splitView.IsPaneOpen = false;

                // Delete Task from collection and databaseIn
                var deleteSuccess = (SelectedModel != null) ? ViewModel.DeleteTask(SelectedModel) : false;

                UpdateCardIndexes();

                if (deleteSuccess)
                    KanbanInAppNotification.Show("Task deleted from board successfully", 3000);
            }
            else
                return;
        }

        private void BtnNewTaskCurrentColumn_Click(object sender, RoutedEventArgs e)
        {
            // Add task to specific column
            // Only show categories within that column
            var btn = sender as Button;
            var context = btn.DataContext as ColumnTag;
            var currentColTitle = context.Header.ToString();

            // Null card for new task
            ViewModel.NewTaskHelper(currentColTitle, GetColorKeys(kanbanBoard));

            // Open pane if not already
            if (splitView.IsPaneOpen == false)
                splitView.IsPaneOpen = true;
        }

        private void appBarBtnClosePane_Click(object sender, RoutedEventArgs e)
        {
            // Reset changes and close pane
            // To Do: Change when adding task
            SelectedModel = ViewModel.OriginalCardModel;

            if (splitView.IsPaneOpen == true)
                splitView.IsPaneOpen = false;

            ViewModel.CardModel = null; // Reset selected card property
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Reset changes and close pane
            // To Do: Change when adding task
            SelectedModel = ViewModel.OriginalCardModel;

            if (splitView.IsPaneOpen == true)
                splitView.IsPaneOpen = false;

            ViewModel.CardModel = null; // Reset selected card property
        }

        private void BtnSaveTask_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CardModel != null) // Editing a Task
            {
                // UI-related operations
                // Store tags as a single string using csv format
                // When calling GetData(), the string will be parsed into separate tags and stored into the list view
                List<string> tagsList = new List<string>();
                foreach (var tag in lstViewTags.Items)
                    tagsList.Add(tag.ToString());
                var tags = string.Join(',', tagsList); // Convert to a csv string to store in database cell

                // Use view model to operate on model-related data
                var selectedCategory = ViewModel.CurrentCategory;
                var selectedColorKey = comboBoxColorKey.SelectedItem;
                var updateSuccess = ViewModel.SaveTask(tags, selectedCategory, selectedColorKey, SelectedModel);

                // Close pane when done
                if (splitView.IsPaneOpen == true)
                    splitView.IsPaneOpen = false;

                if (updateSuccess)
                {
                    SelectedModel.ColorKey = selectedColorKey;
                    KanbanInAppNotification.Show("Task successfully updated", 3000);

                }
                else
                    KanbanInAppNotification.Show("Task could not be updated", 3000);
            }
            else if (ViewModel.CardModel == null) // Creating a Task
            {
                List<string> tagsList = new List<string>();
                foreach (var tag in lstViewTags.Items)
                    tagsList.Add(tag.ToString());
                var tags = string.Join(',', tagsList); // Convert to single string
                if (tags == "")
                    tags = null;

                // To allow a draft task, require user to have category and colorkey chosen
                if (comboBoxColorKey.SelectedItem == null)
                {
                    ChoosePriorityInidcatorTeachingTip.IsOpen = true;
                }
                else
                {
                    var selectedCategory = ViewModel.CurrentCategory;
                    var selectedColorKey = comboBoxColorKey.SelectedItem;

                    // Returns a tuple (bool addSuccess, int id) for success validation and 
                    // compare the created cards ID to the 
                    var returnedTuple = ViewModel.AddTask(tags, selectedCategory, selectedColorKey);
                    var addSuccess = returnedTuple.Item1;
                    var newTaskId = returnedTuple.Item2;

                    // Get column index of card just added
                    // To-do: Efficient implementation once functionality is working
                    foreach (var col in kanbanBoard.ActualColumns)
                    {
                        if (col.Title.ToString() == selectedCategory)
                        {
                            foreach (var card in col.Cards)
                            {
                                var currentModel = card.Content as CustomKanbanModel;
                                if (currentModel.ID == newTaskId.ToString())
                                {
                                    // Get column index
                                    var currentCardIndex = col.Cards.IndexOf(card);
                                    currentModel.ColumnIndex = currentCardIndex.ToString();

                                    // Add the column index to the database entry
                                    ViewModel.UpdateCardIndex(currentModel.ID, currentCardIndex);
                                }
                            }
                        }

                    }

                    // Close pane when done
                    if (splitView.IsPaneOpen == true)
                        splitView.IsPaneOpen = false;

                    if (addSuccess)
                        KanbanInAppNotification.Show("Task successfully added to the board", 3000);
                    else
                        KanbanInAppNotification.Show("Task could not be created", 3000);
                }
            }
        }

        private void TxtBoxTags_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            // Add Tag to listview on keydown event
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var tagsTextBox = sender as TextBox;
                if (tagsTextBox.Text == "")
                    return;
                else
                {
                    if (ViewModel.TagsCollection.Contains(tagsTextBox.Text))
                        KanbanInAppNotification.Show("Tag already exists", 3000);
                    else
                        ViewModel.AddTagToCollection(tagsTextBox.Text);
                    tagsTextBox.Text = "";
                }
            }
        }

        private void BtnDeleteTags_Click(object sender, RoutedEventArgs e)
        {
            // Delete selected items in the New Task tags listview
            var copyOfSelectedItems = lstViewTags.SelectedItems.ToArray();
            foreach (var item in copyOfSelectedItems)
                (lstViewTags.ItemsSource as IList).Remove(item);
        }

        private void MnuItemExitApp_Click(object sender, RoutedEventArgs e)
        {
            CoreApplication.Exit();
        }

        private void BtnDeleteTag_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var tagName = btn.DataContext as string;
            var deleteSuccess = ViewModel.DeleteTag(tagName);

            if (deleteSuccess)
                KanbanInAppNotification.Show("Tag deleted successfully", 3000);
            else
                KanbanInAppNotification.Show("Tag could not be deleted", 3000);
        }

        private void Card_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            // Pre: Get information to pass to the dialog for displaying
            //      Set corresponding properties in TaskDialog
            // Post: Information passed, dialog opened

            // Always show in standard mode
            var originalSource = (FrameworkElement)sender;
            SelectedModel = originalSource.DataContext as CustomKanbanModel;
            ShowContextMenu(SelectedModel);
        }

        private void KanbanBoard_CardDragEnd(object sender, KanbanDragEndEventArgs e)
        {
            // Change column and category of task when dragged to new column
            // ObservableCollection Tasks already updated
            var targetCategory = e.TargetKey.ToString();
            var selectedCardModel = e.SelectedCard.Content as CustomKanbanModel;
            int sourceCardIndex = e.SelectedCardIndex;
            int targetCardIndex = e.TargetCardIndex;
            ViewModel.UpdateCardColumn(targetCategory, selectedCardModel, targetCardIndex.ToString());

            // Reorder cards when dragging to & from same column
            if (e.TargetColumn.Title.ToString() == e.SelectedColumn.Title.ToString())
            {
                // Update every card index in the column after rearrange
                foreach (var card in e.TargetColumn.Cards)
                {
                    int currentIndex = e.TargetColumn.Cards.IndexOf(card);
                    if (currentIndex != Convert.ToInt32(targetCardIndex))
                    {
                        var currentModel = card.Content as CustomKanbanModel;
                        ViewModel.UpdateCardIndex(currentModel.ID, currentIndex);
                    }
                }
            }
            // Reorder cards when dragging from one column to another 
            else
            {
                // Reorder target col after drop
                // Only items below the targetCardIndex need to be updated
                if (e.TargetColumn.Cards.Count != 0)
                {
                    foreach (var card in e.TargetColumn.Cards)
                    {
                        int currentIndex = e.TargetColumn.Cards.IndexOf(card);
                        if (currentIndex > Convert.ToInt32(targetCardIndex))
                        {
                            var currentModel = card.Content as CustomKanbanModel;
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
                        var currentModel = card.Content as CustomKanbanModel;
                        ViewModel.UpdateCardIndex(currentModel.ID, currentIndex);
                    }
                }
            }
        }

        private void FlyoutDeleteCardBtnYes_Click(object sender, RoutedEventArgs e)
        {
           // Close pane when done
            splitView.IsPaneOpen = false;

            // Delete Task from collection and database
            var deleteSuccess = (SelectedModel != null) ? ViewModel.DeleteTask(SelectedModel) : false;

            UpdateCardIndexes();

            if (deleteSuccess)
                KanbanInAppNotification.Show("Task deleted from board successfully", 3000);
        }

        private void FlyoutDeleteCardBtnNo_Click(object sender, RoutedEventArgs e)
        {
        }

        #endregion UIEvents

    
    }
}
