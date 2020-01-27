using System.Collections.Generic;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KanbanTasker.Services.SQLite;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace KanbanTasker.Views
{
    public sealed partial class SettingsView : ContentDialog
    {
        public SettingsView()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void BtnCloseSettings_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private async void SettingsDialog_ViewUpdatesClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
            var dialog = new AppUpdatedDialogView();
            var result = await dialog.ShowAsync();
        }

        private async void btnBackupDb_Click(object sender, RoutedEventArgs e)
        {
            // SQLite
            // Backup Db

            // Use FilePicker to have user pick the path where they want to save a backup to
            // Then use the BackupDatabase function in the DatabaseServices.cs for SQLite
           
        }

    }
}
