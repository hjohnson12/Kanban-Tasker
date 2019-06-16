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
        //=====================================================================
        // COLLECTIONS USED AS ITEM SOURCE FOR TAGS LIST VIEWS
        //=====================================================================
        public static ObservableCollection<string> NewTaskTagsCollection { get; set; }
        public static ObservableCollection<string> EditTaskTagsCollection { get; set; }

        //=====================================================================
        // PROPERTIES FOR CONTROLS USED IN CONTENT DIALOGS
        //=====================================================================
        public StackPanel StackPanelEditTaskDiag { get; set; }
        public TextBox TxtBoxEditTaskTitle { get; set; }
        public TextBox TxtBoxEditTaskId { get; set; }
        public TextBox TxtBoxEditTaskAssignee { get; set; }
        public TextBox TxtBoxEditTaskDescr { get; set; }
        public ComboBox ComboBoxEditTaskCategory { get; set; }
        public ComboBox ComboBoxEditTaskColorKey { get; set; }
        public TextBox TxtBoxEditTaskTags { get; set; }
        public ListView ListViewEditTaskTags { get; set; }
        public Button BtnEditTaskDeleteTags { get; set; }
        public TextBox TxtBoxEditTaskImageUrl { get; set; }
        public ScrollViewer ScrollViewerEditTaskDiag { get; set; }
        public StackPanel StackPanelNewTaskDiag { get; set; }
        public TextBox TxtBoxNewTaskTitle { get; set; }
        public TextBox TxtBoxNewTaskId { get; set; }
        public TextBox TxtBoxNewTaskDescr { get; set; }
        public TextBox TxtBoxNewTaskAssignee { get; set; }
        public ComboBox ComboBoxNewTaskCategory { get; set; }
        public ComboBox ComboBoxNewTaskColorKey { get; set; }
        public TextBox TxtBoxNewTaskTags { get; set; }
        public ListView ListViewNewTaskTags { get; set; }
        public Button BtnNewTaskDeleteTags { get; set; }
        public TextBox TxtBoxNewTaskImageUrl { get; set; }
        public ScrollViewer ScrollViewerNewTaskDiag { get; set; }

        public List<KanbanModel> Items { get ; set; }

        public MainPage()
        {
            this.InitializeComponent();
            kanbanControl.ItemsSource = DataAccess.GetData(); // Get data from database


            //=======================================================
            // ADD REVEAL BRUSHES TO CARD ITEMS
            //=======================================================

            // Reveal Border Brush
            // Make the kanban card semi-transparent
            //RevealBorderBrush revealBorderBrush = new RevealBorderBrush
            //{
            //    Color = Colors.Black,
            //    FallbackColor = Colors.Gray,
            //    Opacity = 0.8,
            //    TargetTheme = ApplicationTheme.Dark
            //};

            // Reveal Background Brush
            // Make the kanban card semi-transparent
            RevealBackgroundBrush revealBackgroundBrush = new RevealBackgroundBrush
            {
                TargetTheme = ApplicationTheme.Light,
                FallbackColor = Colors.AliceBlue,
                Color = Colors.AliceBlue,
                Opacity = 0.3
            };

            // Set KanbanCardStyle properties
            KanbanCardStyle kanbanCardStyle = new KanbanCardStyle
            {
                CornerRadius = new CornerRadius(10.0),
                BorderThickness = new Thickness(3.0),
                Background = revealBackgroundBrush
            };
            //kanbanCardStyle.Background = (RevealBackgroundBrush)Application.Current.Resources["SystemControlHighlightListLowRevealBackgroundBrush"];
            kanbanControl.CardStyle = kanbanCardStyle; // Set cardstyle property as the card style obj

            // TEST: Color columns?
            //AcrylicBrush bckBrush = new AcrylicBrush
            //{
            //    BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
            //    FallbackColor = Colors.AliceBlue,
            //    TintColor = Colors.AliceBlue,
            //    TintOpacity = 0.3
                
            //};
            //foreach (var col in kanbanControl.ActualColumns)
            //{

            //    col.Background = bckBrush;
            //}
        }

        //=====================================================================
        // FUNCTIONS & EVENTS FOR EDITING A TASK
        //=====================================================================
        private void KanbanControl_CardTapped(object sender, KanbanTappedEventArgs e)
        {
            // Call helper function for editing a task

            //e.SelectedCard.Background = new SolidColorBrush(Colors.Blue);
            //EditTaskHelper(sender, e);



            // TEST, refactor content dialog to XAML:
            EditTaskHelper(sender, e);
        }

        public async void EditTaskHelper(object sender, KanbanTappedEventArgs e)
        {
            // Pre: Get information to pass to the dialog for displaying
            //      Set corresponding properties in TaskDialog
            // Post: Information passed, dialog opened

            // Get selected card
            var selectedCardModel = e.SelectedCard.Content as KanbanModel;

            // Add column categories to a list
            // Displayed in a combobox in TaskDialog for the user to choose
            // which column for the task to be in
            List<string> categories = new List<string>();
            foreach (var col in kanbanControl.ActualColumns)
            {
                // Fill categories list with the categories from the col
                var strCategories = col.Categories;
                if (strCategories.Contains(",")) 
                {
                    // >1 sections in col, split into separate sections
                    var tokens = strCategories.Split(",");
                    foreach (var token in tokens)
                        categories.Add(token);
                }
                else // 1 section in column
                    categories.Add(strCategories);
            }

            // Add color keys to a list
            // Displayed in a combobox in TaskDialog for user to choose
            // the color key for a task
            List<string> colorKeys = new List<string>();
            foreach (var colorMap in kanbanControl.IndicatorColorPalette)
            {
                // Add each key from the color palette to the combobox
                var key = colorMap.Key;
                colorKeys.Add(key.ToString());
            }

            // Add selected card tags to a collection
            // Tags Collection is displayed in a listview in TaskDialog 
            var TagsCollection = new ObservableCollection<string>();
            foreach (var tag in selectedCardModel.Tags)
                TagsCollection.Add(tag); // Add card tags to collection


            // Set corresponding TaskDialog properties
            // Edit TaskDialog Init
            TaskDialog taskDialog = new TaskDialog
            {
                Kanban = kanbanControl,
                Model = selectedCardModel,
                Categories = categories, // Set Categories Property
                ColorKeys = colorKeys, // Set ColorKeys Property
                TaskTags = TagsCollection
            };
            var result = await taskDialog.ShowAsync();

            #region DEBUG
            //if (result == ContentDialogResult.Primary)
            //{
            //    //  Save Task & Update UI

            //    // Store tags as a single string using csv format
            //    // When calling GetData(), the string will be parsed into separate tags and stored into the list view
            //    List<string> tagsList = new List<string>();
            //    foreach (var tag in ListViewEditTaskTags.Items)
            //        tagsList.Add(tag.ToString());

            //    var tags = string.Join(',', tagsList); // Convert to a csv string to store in database cell
            //    // Update item in database
            //    DataAccess.UpdateTask(TxtBoxEditTaskId.Text, TxtBoxEditTaskTitle.Text,
            //        TxtBoxEditTaskDescr.Text, ComboBoxEditTaskCategory.SelectedItem.ToString(),
            //        ComboBoxEditTaskColorKey.SelectedItem.ToString(), tags);

            //    kanbanControl.ItemsSource = DataAccess.GetData(); // Update kanban
            //}
            //else if (result == ContentDialogResult.Secondary)
            //{
            //    // Delete Task & Update UI
            //    DataAccess.DeleteTask(selectedCardModel.ID);
            //    kanbanControl.ItemsSource = DataAccess.GetData();

            //    // Test
            //    //this.Items.Remove(selectedCardModel);
            //    //OnPropertyChanged(nameof(Items)); // Inform Kanban that ItemSource binding is changed
            //}
            #endregion DEBUG
        }

        //=====================================================================
        // FUNCTIONS & EVENTS FOR ADDING A NEW TASK
        //=====================================================================
        private void MnuItemNewTask_Click(object sender, RoutedEventArgs e)
        {
            // Call helper function for adding a new task
            // Handles showing the content dialog controls and working with the data
            NewTaskHelper();
        }

        public async void NewTaskHelper()
        {
            // Add column categories to a list
            // Displayed in a combobox in TaskDialog for the user to choose
            // which column for the task to be in
            List<string> categories = new List<string>();
            foreach (var col in kanbanControl.ActualColumns)
            {
                // Fill categories list with the categories from the col
                var strCategories = col.Categories;
                if (strCategories.Contains(","))
                {
                    // >1 sections in col, split into separate sections
                    var tokens = strCategories.Split(",");
                    foreach (var token in tokens)
                        categories.Add(token);
                }
                else // 1 section in column
                    categories.Add(strCategories);
            }

            // Add color keys to a list
            // Displayed in a combobox in TaskDialog for user to choose
            // the color key for a task
            List<string> colorKeys = new List<string>();
            foreach (var colorMap in kanbanControl.IndicatorColorPalette)
            {
                // Add each key from the color palette to the combobox
                var key = colorMap.Key;
                colorKeys.Add(key.ToString());
            }

            // Set corresponding TaskDialog properties
            TaskDialog taskDialog = new TaskDialog();
            taskDialog.Model = new KanbanModel();
            taskDialog.Kanban = kanbanControl;
            taskDialog.Categories = categories;
            taskDialog.ColorKeys = colorKeys;
            taskDialog.PrimaryButtonText = "Create";
            taskDialog.CloseButtonText = "Cancel";
            taskDialog.IsSecondaryButtonEnabled = false;

            // Create manual event handler to create event instead of edit
            taskDialog.PrimaryButtonClick += TaskDialog_CreateButtonClick;
            var result = await taskDialog.ShowAsync();
            // Create ScrollViewer to be used in the content dialog
            //ScrollViewerNewTaskDiag = new ScrollViewer();
            //ScrollViewerNewTaskDiag.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            //// Stack panel to hold our controls
            //StackPanelNewTaskDiag = new StackPanel();

            //// TITLE TEXTBOX
            //TxtBoxNewTaskTitle = new TextBox
            //{
            //    Header = "Title:",
            //    PlaceholderText = "Type your information here"
            //};
            //StackPanelNewTaskDiag.Children.Add(TxtBoxNewTaskTitle);

            // ASSIGNEE TEXTBOX (RE-IMPLEMENT LATER, BUG WITH SWIMLANE VIEW ON THEIR END)
            // Got up to where when you create a new task with the same assignee, it wouldn't show as a card, but the numbers would incr.
            //txtBoxNewTaskAssignee = new TextBox
            //{
            //    Header = "Assignee:",
            //    PlaceholderText = "Type your information here"
            //};
            //stackPanelNewTaskDiag.Children.Add(txtBoxNewTaskAssignee);

            // DESCRIPTION TEXTBOX
            //TxtBoxNewTaskDescr = new TextBox
            //{
            //    Header = "Description:",
            //    AcceptsReturn = true,
            //    TextWrapping = TextWrapping.Wrap,
            //    MaxLength = 100,
            //    MaxHeight = 400
            //};
            //ScrollViewer.SetVerticalScrollBarVisibility(TxtBoxNewTaskDescr, ScrollBarVisibility.Auto); // Allows scroll
            //StackPanelNewTaskDiag.Children.Add(TxtBoxNewTaskDescr);

            //// CATEGORY COMBOBOX
            //ComboBoxNewTaskCategory = new ComboBox
            //{
            //    IsEditable = true,
            //    Header = "Category:"
            //};
            //foreach (var col in kanbanControl.ActualColumns)
            //{
            //    // Fill category combobox with categories from the columns
            //    var categories = col.Categories;
            //    if (categories.Contains(", "))
            //    {
            //        var tokens = categories.Split(", ");
            //        foreach (var category in tokens)
            //            ComboBoxNewTaskCategory.Items.Add(category);
            //    }
            //    else
            //        ComboBoxNewTaskCategory.Items.Add(categories);
            //}
            //StackPanelNewTaskDiag.Children.Add(ComboBoxNewTaskCategory);

            //// COLOR KEY COMBOBOX
            //ComboBoxNewTaskColorKey = new ComboBox
            //{
            //    IsEditable = false,
            //    Header = "Color Key: "
            //};
            //foreach (var colorMap in kanbanControl.IndicatorColorPalette)
            //{
            //    // Add each key from the color palette to the combobox
            //    var key = colorMap.Key;
            //    ComboBoxNewTaskColorKey.Items.Add(key);
            //}
            //StackPanelNewTaskDiag.Children.Add(ComboBoxNewTaskColorKey);

            //// TAGS LIST VIEW & COLLECTION (Added to stackpanel after textbox below)
            //NewTaskTagsCollection = new ObservableCollection<string>();
            //ListViewNewTaskTags = new ListView
            //{
            //    CanDragItems = true,
            //    CanReorderItems = true,
            //    AllowDrop = true,
            //    MaxHeight = 150,
            //    SelectionMode = ListViewSelectionMode.Multiple,
            //    ItemsSource = NewTaskTagsCollection
            //};

            //// TAGS TEXTBOX & KEY DOWN EVENT HANDLER
            //TxtBoxNewTaskTags = new TextBox
            //{
            //    Header = "Tags:",
            //    PlaceholderText = "Type and press enter to add a new tag to the list."
            //};
            //TxtBoxNewTaskTags.KeyDown += (sender2, e2) => TxtBoxNewTaskTags_EnterKeyDown(sender2, e2, ListViewNewTaskTags); // Event to add tag to collection
            //StackPanelNewTaskDiag.Children.Add(TxtBoxNewTaskTags);
            //StackPanelNewTaskDiag.Children.Add(ListViewNewTaskTags);

            //// DELETE SELECTED ITEMS BUTTON & CLICK EVENT HANDLER
            //BtnNewTaskDeleteTags = new Button
            //{
            //    Content = "Delete Selected Items"
            //};
            //StackPanelNewTaskDiag.Children.Add(BtnNewTaskDeleteTags);
            //BtnNewTaskDeleteTags.Click += (sender2, e2) => btnNewTaskDeleteTags_Click(sender2, e2, ListViewNewTaskTags); // Event to delete tags from collection

            //// IMAGE URL TEXTBOX
            ////TxtBoxNewTaskImageUrl = new TextBox
            ////{
            ////    Header = "Image URL",
            ////    PlaceholderText = "Type your information here"
            ////};
            ////StackPanelNewTaskDiag.Children.Add(TxtBoxNewTaskImageUrl);

            //// Set scrollViewer content to our StackPanel
            //ScrollViewerNewTaskDiag.Content = StackPanelNewTaskDiag;

            //// Create dialog for adding new task & attach event handlers
            //ContentDialog contentDialogNewTask = new ContentDialog()
            //{
            //    Title = "New Task",
            //    Content = ScrollViewerNewTaskDiag,
            //    PrimaryButtonText = "Create",
            //    SecondaryButtonText = "Cancel",
            //    BorderThickness = new Windows.UI.Xaml.Thickness(0, 0, 0, 0)
            //};
            //contentDialogNewTask.CloseButtonClick += contentDialogNewTask_CloseButtonClick;
            //contentDialogNewTask.PrimaryButtonClick += contentDialogNewTask_CreateClick;
            //contentDialogNewTask.Opened += contentDialogNewTask_Opened;

            //await contentDialogNewTask.ShowAsync(); // Show Dialog
        }

        private void TaskDialog_CreateButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var msg = new MessageDialog("LOLOL");
        }

        // Add brush to New Task content dialog
        private void contentDialogNewTask_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.XamlCompositionBrushBase")) // if they have acrylic
            {
                //AcrylicBrush myBrush = new AcrylicBrush
                //{
                //    BackgroundSource = Windows.UI.Xaml.Media.AcrylicBackgroundSource.HostBackdrop,
                //    TintColor = Color.FromArgb(255, 30, 144, 255),
                //    FallbackColor = Color.FromArgb(255, 30, 144, 255),
                //    TintOpacity = 0.6
                //};

                SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.AliceBlue);

                sender.Background = solidColorBrush;
            }
            else
            {
                SolidColorBrush myBrush = new SolidColorBrush(Color.FromArgb(255, 202, 24, 37));
                sender.Background = myBrush;
            }
        }

        // Add new task to database
        private async void contentDialogNewTask_CreateClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            List<string> tagsList = new List<string>();
            foreach (var tag in ListViewNewTaskTags.Items)
                tagsList.Add(tag.ToString());
            var tags = string.Join(',', tagsList); // Convert to single string

            // To allow a draft task, require user to have category and colorkey chosen
            if (ComboBoxNewTaskCategory.SelectedItem == null || ComboBoxNewTaskColorKey.SelectedItem == null)
            {
                var messageDialog = new MessageDialog("NOTE: You must fill out a category and color key to be able to create a draft task", "ERROR");
                await messageDialog.ShowAsync();
            }
            else
            {
                // Add task to database
                DataAccess.AddTask(TxtBoxNewTaskTitle.Text,
                    TxtBoxNewTaskDescr.Text, ComboBoxNewTaskCategory.SelectedItem.ToString(),
                    ComboBoxNewTaskColorKey.SelectedItem.ToString(), tags);

                // Update KanbanControl
                kanbanControl.ItemsSource = DataAccess.GetData();
            }
        }

        private void btnNewTaskDeleteTags_Click(object sender2, RoutedEventArgs e2, ListView listViewNewTaskTags)
        {
            // Delete selected items in the New Task tags listview
            var copyOfSelectedItems = listViewNewTaskTags.SelectedItems.ToArray();
            foreach (var item in copyOfSelectedItems)
                (listViewNewTaskTags.ItemsSource as IList).Remove(item);
        }
        private void contentDialogNewTask_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            sender.Hide(); // Hide dialog when user cancels
        }

        // Add tag to New Task tags list view when user presses enter key while inside the textbox
        private void TxtBoxNewTaskTags_EnterKeyDown(object sender, KeyRoutedEventArgs e, ListView listViewNewTaskTags)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var currentTextBox = sender as TextBox;
                if (currentTextBox.Text == "")
                    return;
                else
                {
                    NewTaskTagsCollection.Add(currentTextBox.Text);
                    currentTextBox.Text = "";
                }
            }
        }

        private void MnuItemExitApp_Click(object sender, RoutedEventArgs e)
        {
            CoreApplication.Exit();
        }
    }

    public class StringToColorConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value.ToString() == "Low")
            {

                RevealBorderBrush revealBorderBrush = new RevealBorderBrush
                {
                    Color = Colors.Green,
                    FallbackColor = Colors.Green,
                    Opacity = 0.8,
                    TargetTheme = ApplicationTheme.Light
                };

                // Low Priority - Reveal Brush
                return revealBorderBrush;
            }
            if (value.ToString() == "Normal")
            {
                // Normal Priority - Reveal Brush
                RevealBorderBrush revealBorderBrush = new RevealBorderBrush
                {
                    Color = Colors.Orange,
                    FallbackColor = Colors.Orange,
                    Opacity = 0.8,
                    TargetTheme = ApplicationTheme.Light
                };

                // Low Priority - Reveal Brush
                return revealBorderBrush;
            }
            if (value.ToString() == "High")
            {
                // High Priority - Reveal Brush
                RevealBorderBrush revealBorderBrush = new RevealBorderBrush
                {
                    Color = Colors.Red,
                    FallbackColor = Colors.Red,
                    Opacity = 0.8,
                    TargetTheme = ApplicationTheme.Light
                };

                // Low Priority - Reveal Brush
                return revealBorderBrush;
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }

}
