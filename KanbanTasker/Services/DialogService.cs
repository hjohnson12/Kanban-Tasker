using KanbanTasker.Model.Services;
using KanbanTasker.Views;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace KanbanTasker.Services
{
    /// <summary>
    /// A class containing methods to work with content dialogs.
    /// </summary>
    public class DialogService : IDialogService
    {
        private bool _isDialogOpen;

        public bool CheckForOpenDialog()
        {
            return _isDialogOpen;
        }

        public async Task ShowDialog(ContentDialog dialog)
        {
            if (_isDialogOpen)
                return;

            _isDialogOpen = true;

            await dialog.ShowAsync();

            _isDialogOpen = false;
        }

        public async Task ShowEditBoardDialog(object viewModel)
        {
            var dialog = new EditBoardDialogView() { DataContext = viewModel };
            await ShowDialog(dialog);
        }

        public async Task ShowFirstRunDialog()
        {
            var dialog = new FirstRunDialogView();
            await ShowDialog(dialog);
        }

        public async Task ShowAppUpdatedDialog()
        {
            var dialog = new AppUpdatedDialogView();
            await ShowDialog(dialog);
        }

        public async Task ShowSettingsDialog()
        {
            var dialog = new SettingsView();
            await ShowDialog(dialog);
        }

        public async Task ShowCalendarDialog(object viewModel)
        {
            var dialog = new CalendarDialogView(viewModel);
            await ShowDialog(dialog);
        }
    }
}