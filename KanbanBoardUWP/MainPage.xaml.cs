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
        public MainPage()
        {
            this.InitializeComponent();
            kanbanBoard.ItemsSource = DataAccess.GetData(); // Get data from database

            // Add rounded corners to each card
            kanbanBoard.CardStyle.CornerRadius = new CornerRadius(10.0);
        }

        //=====================================================================
        // FUNCTIONS & EVENTS FOR EDITING A TASK
        //=====================================================================

        private async void KanbanBoard_CardTapped(object sender, KanbanTappedEventArgs e)
        {
            // Pre: Get information to pass to the dialog for displaying
            //      Set corresponding properties in TaskDialog
            // Post: Information passed, dialog opened

            // Get selected card
            var selectedCardModel = e.SelectedCard.Content as KanbanModel;

            // Set corresponding TaskDialog properties
            // Edit TaskDialog Init
            TaskDialog editTaskDialog = new TaskDialog
            {
                Kanban = kanbanBoard,
                Model = selectedCardModel,
                Categories = GetCategories(kanbanBoard), // Set Categories Property
                ColorKeys = GetColorKeys(kanbanBoard), // Set ColorKeys Property
                TaskTags = GetTagCollection(selectedCardModel)
            };
            await editTaskDialog.ShowAsync(); // Dialog open
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
    }

}
