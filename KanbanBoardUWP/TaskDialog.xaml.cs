using Syncfusion.UI.Xaml.Kanban;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace KanbanBoardUWP
{
    public sealed partial class TaskDialog : ContentDialog
    {
        public List<string> Categories { get; set; }
        public List<string> ColorKeys { get; set; }
        public KanbanModel Model { get; set; }
        public ObservableCollection<string> TaskTags { get; set; }

        public SfKanban Kanban { get; set; }

        public TaskDialog()
        {
            this.InitializeComponent();
        }

        private void TaskDialog_DeleteButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Delete Task and update kanban
            DataAccess.DeleteTask(Model.ID);
            Kanban.ItemsSource = DataAccess.GetData();
        }

        private void TxtBoxTags_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            // Add Tag to listview on keydown event
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var currentTextBox = sender as TextBox;
                if (currentTextBox.Text == "")
                    return;
                else
                {
                    TaskTags.Add(currentTextBox.Text);
                    currentTextBox.Text = "";
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

        private void TaskDialog_SaveButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Store tags as a single string using csv format
            // When calling GetData(), the string will be parsed into separate tags and stored into the list view
            List<string> tagsList = new List<string>();
            foreach (var tag in lstViewTags.Items)
                tagsList.Add(tag.ToString());
            var tags = string.Join(',', tagsList); // Convert to a csv string to store in database cell

            // Update item in database
            DataAccess.UpdateTask(txtBoxID.Text, txtBoxTitle.Text,
                txtBoxDescription.Text, comboBoxCategories.SelectedItem.ToString(),
                comboBoxColorKey.SelectedItem.ToString(), tags);

            Kanban.ItemsSource = DataAccess.GetData(); // Update kanban
        }

        private void TaskDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            sender.Hide(); // Cancel dialog
        }

    }
}
