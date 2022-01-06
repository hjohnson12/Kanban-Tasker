using KanbanTasker.Model.Services;
using KanbanTasker.Views.Dialogs;
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
            var dialog = new EditBoardDialog() { DataContext = viewModel };
            await ShowDialog(dialog);
        }

        public async Task ShowFirstRunDialog()
        {
            var dialog = new FirstRunDialog();
            await ShowDialog(dialog);
        }

        public async Task ShowAppUpdatedDialog()
        {
            var dialog = new AppUpdatedDialog();
            await ShowDialog(dialog);
        }

        public async Task ShowSettingsDialog()
        {
            var dialog = new SettingsDialog();
            await ShowDialog(dialog);
        }

        public async Task ShowCalendarDialog(object viewModel)
        {
            var dialog = new CalendarDialog(viewModel);
            await ShowDialog(dialog);
        }

        public async Task ShowBoardListDialog(object viewModel)
        {
            var dialog = new BoardListDialog() { DataContext = viewModel };
            await ShowDialog(dialog);
        }
    }
}