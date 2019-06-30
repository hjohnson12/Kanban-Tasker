using KanbanBoardUWP.ViewModel;
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
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace KanbanBoardUWP
{


    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; set; }
        public ComboBoxItem comboBoxItem { get; set; }
        public KanbanModel SelectedModel { get; set; }
        public bool IsOpen { get; set; }
        public MainPage()
        {
            this.InitializeComponent();

            ViewModel = new MainViewModel();

            //kanbanBoard.ItemsSource = DataAccess.GetData(); // Get data from database
            // Add rounded corners to each card
            kanbanBoard.CardStyle.CornerRadius = new CornerRadius(10.0);
            // kanbanBoard.CardStyle.IconVisibility = Visibility.Collapsed;
        }

        //=====================================================================
        // FUNCTIONS & EVENTS FOR EDITING A TASK
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
            SelectedModel = e.SelectedCard.Content as KanbanModel;
            // Show context menu next to selected card
            ShowContextMenu(selectedCardIndex, currentCol);
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
        // FUNCTIONS & EVENTS FOR ADDING A NEW TASK
        //=====================================================================
        private void MnuItemNewTask_Click(object sender, RoutedEventArgs e)
        {
            // Hide flyout
            kanbanFlyout.Hide();

            // Null card for new task
            ViewModel.NewTaskHelper(GetCategories(kanbanBoard), GetColorKeys(kanbanBoard));

            // Open pane if not already
            if (splitView.IsPaneOpen == false)
                splitView.IsPaneOpen = true;
        }

        private void MnuItemExitApp_Click(object sender, RoutedEventArgs e)
        {
            CoreApplication.Exit();
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

        public ObservableCollection<string> GetTagCollection(KanbanModel selectedModel)
        {
            // Add selected card tags to a collection
            // Tags Collection is displayed in a listview in TaskDialog 
            var tagsCollection = new ObservableCollection<string>();
            foreach (var tag in selectedModel.Tags)
                tagsCollection.Add(tag); // Add card tags to collection
            return tagsCollection;
        }

        private async void BtnNewTaskCurrentColumn_Click(object sender, RoutedEventArgs e)
        {
            // TO DO

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
                if(col.Title.ToString() == currentColTitle)
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

            //// Set corresponding TaskDialog properties
            //TaskDialog newTaskDialog = new TaskDialog
            //{
            //    Model = null,
            //    Kanban = kanbanBoard,
            //    Categories = lstCategories,
            //    ColorKeys = GetColorKeys(kanbanBoard),
            //    PrimaryButtonText = "Create",
            //    IsSecondaryButtonEnabled = false,
            //};
            //await newTaskDialog.ShowAsync(); // Dialog open
        }

        private void FlyoutBtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Call helper from ViewModel to handle model-related data
            ViewModel.EditTaskHelper(SelectedModel, GetCategories(kanbanBoard),
                GetColorKeys(kanbanBoard), GetTagCollection(SelectedModel));

            // Set selected items in combo box
            comboBoxCategories.SelectedItem = SelectedModel.Category;
            comboBoxColorKey.SelectedItem = SelectedModel.ColorKey;

            // UI Related Code

            // Hide flyout
            taskFlyout.Hide();

            if (splitView.IsPaneOpen == false)
                splitView.IsPaneOpen = true;
           
            
            //// Set corresponding TaskDialog properties
            //// Edit TaskDialog Init
            //TaskDialog editTaskDialog = new TaskDialog
            //{
            //    Kanban = kanbanBoard,
            //    Model = SelectedModel,
            //    Categories = GetCategories(kanbanBoard), // Set Categories Property
            //    ColorKeys = GetColorKeys(kanbanBoard), // Set ColorKeys Property
            //    TaskTags = GetTagCollection(SelectedModel)
            //};
            //await editTaskDialog.ShowAsync(); // Dialog open
        }

        private async void FlyoutBtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Hide flyout
            taskFlyout.Hide();

            ContentDialog deleteDialog = new ContentDialog()
            {
                Title = "Delete Task Confirmation",
                PrimaryButtonText = "Yes",
                Content = "Are you sure you wish to delete this task?",
                SecondaryButtonText = "No"
            };
            var result = await deleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Delete Task from collection and database
                ViewModel.DeleteTask(SelectedModel);
                DataAccess.DeleteTask(SelectedModel.ID);
            }
            else
                return; // Cancel
        }

        private void FlyoutBtnNewTask_Click(object sender, RoutedEventArgs e)
        {
            // Set corresponding TaskDialog properties to create new task
            //TaskDialog newTaskDialog = new TaskDialog
            //{
            //    Model = null,
            //    Kanban = kanbanBoard,
            //    Categories = GetCategories(kanbanBoard),
            //    ColorKeys = GetColorKeys(kanbanBoard),
            //    PrimaryButtonText = "Create",
            //    IsSecondaryButtonEnabled = false
            //};
            //await newTaskDialog.ShowAsync(); // Dialog open

            // Hide flyout
            kanbanFlyout.Hide();

            //ViewModel.Task = new KanbanModel(); // New Task, null model
            ViewModel.NewTaskHelper(GetCategories(kanbanBoard), GetColorKeys(kanbanBoard));

            if (splitView.IsPaneOpen == false)
                splitView.IsPaneOpen = true;
        }

        private void appBarBtnClosePane_Click(object sender, RoutedEventArgs e)
        {
            splitView.IsPaneOpen = false;
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
            //ViewModel.DeleteTagsFromCollection(copyOfSelectedItems);
        }

        private void BtnCancelEdit_Click(object sender, RoutedEventArgs e)
        {
            // Reset changes and close pane\
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
                ViewModel.SaveTask(tags, selectedCategory, selectedColorKey);

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
                    //var messageDialog = new MessageDialog("NOTE: You must fill out a category and color key to be able to create a draft task", "ERROR");
                    //await messageDialog.ShowAsync();
                    comboBoxCategories.SelectedItem = "To Do";
                    comboBoxColorKey.SelectedItem = "Low";
                }

                var selectedCategory = comboBoxCategories.SelectedItem;
                var selectedColorKey = comboBoxColorKey.SelectedItem;
                ViewModel.AddTask(tags, selectedCategory, selectedColorKey);

                // Close pane when done
                if (splitView.IsPaneOpen == true)
                    splitView.IsPaneOpen = false;
            }
        }
    }


    public class CollapsedHeaderMargin : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            TextBlock textBlock = value as TextBlock;
            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double top = textBlock.DesiredSize.Width - 10;
            Thickness thickness = new Thickness(-12, top, 0, 0);
            return thickness;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }

    public class CardTemplateConvertor : DependencyObject, IValueConverter
    {

        public object column
        {
            get { return (object)GetValue(columnProperty); }
            set { SetValue(columnProperty, value); }
        }

        // Using a DependencyProperty as the backing store for column.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty columnProperty =
            DependencyProperty.Register("column", typeof(object), typeof(CardTemplateConvertor), new PropertyMetadata(null));


        public object Convert(object value, Type targetType, object parameter, string language)
        {
            //KanbanColumn column = (KanbanColumn)parameter;

            //if (column.IsExpanded)
            //{
            //    column.Template = (ControlTemplate)column.Resources["CollapsedTemplate"];
            //}
            //else
            //{
            //    column.Template = (ControlTemplate)column.Resources["DefaultKanbanHeaderTemplate"];
            //}

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            //KanbanColumn column = (KanbanColumn)parameter;

            //if (column.IsExpanded)
            //{
            //    column.Template = (ControlTemplate)column.Resources["CollapsedTemplate"];
            //}
            //else
            //{
            //    column.Template = (ControlTemplate)column.Resources["DefaultKanbanHeaderTemplate"];
            //}

            return value;
        }
    }

}
