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
        public KanbanModel SelectedModel { get; set; }
        public MainPage()
        {
            this.InitializeComponent();
            kanbanBoard.ItemsSource = DataAccess.GetData(); // Get data from database
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
        private async void MnuItemNewTask_Click(object sender, RoutedEventArgs e)
        {
            // Set corresponding TaskDialog properties
            TaskDialog newTaskDialog = new TaskDialog
            {
                Model = null,
                Kanban = kanbanBoard,
                Categories = GetCategories(kanbanBoard),
                ColorKeys = GetColorKeys(kanbanBoard),
                PrimaryButtonText = "Create",
                IsSecondaryButtonEnabled = false
            };
            await newTaskDialog.ShowAsync(); // Dialog open
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
            var TagsCollection = new ObservableCollection<string>();
            foreach (var tag in selectedModel.Tags)
                TagsCollection.Add(tag); // Add card tags to collection
            return TagsCollection;
        }

        private async void BtnNewTaskCurrentColumn_Click(object sender, RoutedEventArgs e)
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

            // Set corresponding TaskDialog properties
            TaskDialog newTaskDialog = new TaskDialog
            {
                Model = null,
                Kanban = kanbanBoard,
                Categories = lstCategories,
                ColorKeys = GetColorKeys(kanbanBoard),
                PrimaryButtonText = "Create",
                IsSecondaryButtonEnabled = false,
            };
            await newTaskDialog.ShowAsync(); // Dialog open
        }

        private async void FlyoutBtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Hide flyout
            taskFlyout.Hide();

            // Set corresponding TaskDialog properties
            // Edit TaskDialog Init
            TaskDialog editTaskDialog = new TaskDialog
            {
                Kanban = kanbanBoard,
                Model = SelectedModel,
                Categories = GetCategories(kanbanBoard), // Set Categories Property
                ColorKeys = GetColorKeys(kanbanBoard), // Set ColorKeys Property
                TaskTags = GetTagCollection(SelectedModel)
            };
            await editTaskDialog.ShowAsync(); // Dialog open
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
                // Delete Task and update kanban
                DataAccess.DeleteTask(SelectedModel.ID);
                kanbanBoard.ItemsSource = DataAccess.GetData();
            }
            else
                return; // Cancel
        }

        private async void FlyoutBtnNewTask_Click(object sender, RoutedEventArgs e)
        {
            // Set corresponding TaskDialog properties to create new task
            TaskDialog newTaskDialog = new TaskDialog
            {
                Model = null,
                Kanban = kanbanBoard,
                Categories = GetCategories(kanbanBoard),
                ColorKeys = GetColorKeys(kanbanBoard),
                PrimaryButtonText = "Create",
                IsSecondaryButtonEnabled = false
            };
            await newTaskDialog.ShowAsync(); // Dialog open
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
