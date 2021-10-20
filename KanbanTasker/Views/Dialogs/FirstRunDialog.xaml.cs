using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KanbanTasker.Views.Dialogs
{
    public sealed partial class FirstRunDialog : ContentDialog
    {
        public FirstRunDialog()
        {
            this.InitializeComponent();
        }

        private void btnCloseDialog_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}