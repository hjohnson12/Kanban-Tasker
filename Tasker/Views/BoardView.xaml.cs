using Tasker.Models;
using Tasker.ViewModels;
using Syncfusion.UI.Xaml.Kanban;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Tasker.Views
{
    public sealed partial class BoardView : UserControl
    {

        public MainViewModel ViewModel { get; set; }
        public CustomKanbanModel SelectedModel { get; set; }

        public BoardView()
        {
            this.InitializeComponent();
            ViewModel = new MainViewModel();
            // Add rounded corners to each card
            kanbanBoard.CardStyle.CornerRadius = new CornerRadius(10.0);
        }

        //=====================================================================
        // HELPER FUNCTIONS
        //=====================================================================

        public List<string> GetCategories(SfKanban kanban)
        {
            // Add column categories to a list
            // Displayed in a combobox in TaskDialog for the user to choose
            // which column for the task to be in
            List<string> lstCategories = new List<string>();
            foreach (var col in kanban.ActualColumns)
            {
                // Fill categories list with the categories from the col
                var strCategories = col.Categories;
                if (strCategories.Contains(","))
                {
                    // >1 sections in col, split into separate sections
                    var tokens = strCategories.Split(",");
                    foreach (var token in tokens)
                        lstCategories.Add(token);
                }
                else // 1 section in column
                    lstCategories.Add(strCategories);
            }
            return lstCategories;
        }

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

        public void ShowContextMenu(int currentCardindex, string currentCol)
        {
            // Workaround to show context menu next to selected card model
            foreach (var col in kanbanBoard.ActualColumns)
            {
                if (col.Title.ToString() == currentCol)
                {
                    // Set flyout to selected card index
                    for (int i = 0; i <= col.Cards.Count; i++)
                    {
                        if (i == currentCardindex)
                        {
                            FlyoutShowOptions myOption = new FlyoutShowOptions();
                            myOption.ShowMode = FlyoutShowMode.Transient;
                            taskFlyout.ShowAt(col.Cards[i], myOption);
                        }
                    }
                }
            }
        }

        //=====================================================================
        // UI Events
        //=====================================================================

        private void KanbanBoard_CardTapped(object sender, KanbanTappedEventArgs e)
        {
            // Pre: Get information to pass to the dialog for displaying
            //      Set corresponding properties in TaskDialog
            // Post: Information passed, dialog opened

            // Always show in standard mode
            // Get selected card
            var currentCol = e.SelectedColumn.Title.ToString();
            var selectedCardIndex = e.SelectedCardIndex;
            SelectedModel = e.SelectedCard.Content as CustomKanbanModel;
            // Show context menu next to selected card
            ShowContextMenu(selectedCardIndex, currentCol);
        }

        private void BtnNewTask_Click(object sender, RoutedEventArgs e)
        {

            // Null card for new task
            ViewModel.NewTaskHelper(GetCategories(kanbanBoard), GetColorKeys(kanbanBoard));

            // UI RELATED CODE 

            // Hide kanban flyout if used to create new task
            if (kanbanFlyout.IsOpen)
                kanbanFlyout.Hide();

            // Open pane if not already
            if (splitView.IsPaneOpen == false)
                splitView.IsPaneOpen = true;

            // Give title textbox focus when pane opens
            txtBoxTitle.Focus(FocusState.Programmatic);
        }

        private void FlyoutBtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Call helper from ViewModel to handle model-related data
            ViewModel.EditTaskHelper(SelectedModel, GetCategories(kanbanBoard),
                GetColorKeys(kanbanBoard), GetTagCollection(SelectedModel));

            // UI RELATED CODE

            // Set selected items in combo box
            comboBoxCategories.SelectedItem = SelectedModel.Category;
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

        private async void FlyoutBtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Hide flyout
            taskFlyout.Hide();

            // Create dialog and check button click result
            var deleteDialog = new DeleteConfirmationView();
            var result = await deleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Delete Task from collection and database
                ViewModel.DeleteTask(SelectedModel);

                // Close pane when done
                splitView.IsPaneOpen = false;
            }
            else
                return; // Cancel
        }

        private void BtnNewTaskCurrentColumn_Click(object sender, RoutedEventArgs e)
        {
            // Add task to specific column
            // Only show categories within that column
            var btn = sender as Button;
            var context = btn.DataContext as ColumnTag;
            var currentColTitle = context.Header.ToString();

            // Add current column categories to a list
            // Displayed in a combobox in TaskDialog for the user to
            // choose which category to put the task in the current column
            List<string> lstCategories = new List<string>();
            foreach (var col in kanbanBoard.ActualColumns)
            {
                if (col.Title.ToString() == currentColTitle)
                {
                    // Fill categories list with the categories from the col
                    var strCategories = col.Categories;
                    if (strCategories.Contains(","))
                    {
                        // >1 sections in col, split into separate sections
                        var tokens = strCategories.Split(",");
                        foreach (var token in tokens)
                            lstCategories.Add(token);
                    }
                    else // 1 section in column
                        lstCategories.Add(strCategories);
                }
            }

            // Hide flyout
            kanbanFlyout.Hide();

            // Null card for new task
            ViewModel.NewTaskHelper(lstCategories, GetColorKeys(kanbanBoard));

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

        private async void BtnSaveTask_Click(object sender, RoutedEventArgs e)
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
                var selectedCategory = comboBoxCategories.SelectedItem;
                var selectedColorKey = comboBoxColorKey.SelectedItem;
                ViewModel.SaveTask(tags, selectedCategory, selectedColorKey, SelectedModel);

                // Close pane when done
                if (splitView.IsPaneOpen == true)
                    splitView.IsPaneOpen = false;
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
                if (comboBoxCategories.SelectedItem == null || comboBoxColorKey.SelectedItem == null)
                {
                    var messageDialog = new MessageDialog("NOTE: You must fill out a category and color key to be able to create a draft task", "ERROR");
                    await messageDialog.ShowAsync();
                }

                var selectedCategory = comboBoxCategories.SelectedItem;
                var selectedColorKey = comboBoxColorKey.SelectedItem;
                ViewModel.AddTask(tags, selectedCategory, selectedColorKey);

                // Close pane when done
                if (splitView.IsPaneOpen == true)
                    splitView.IsPaneOpen = false;
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
    }
}
