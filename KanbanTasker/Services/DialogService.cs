using KanbanTasker.ViewModels;
using KanbanTasker.Views;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace KanbanTasker.Services
{
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

        public async Task ShowEditDialog(MainViewModel mainViewModel)
        {
            if (_isDialogOpen)
                return;

            _isDialogOpen = true;

            var dialog = new EditBoardDialogView() { DataContext = mainViewModel };
            var result = await dialog.ShowAsync();

            _isDialogOpen = false;
        }

        public async Task ShowFirstRunDialog()
        {
            if (_isDialogOpen)
                return;

            _isDialogOpen = true;

            var dialog = new FirstRunDialogView();
            await dialog.ShowAsync();

            _isDialogOpen = false;
        }

        public async Task ShowAppUpdatedDialog()
        {
            if (_isDialogOpen)
                return;

            _isDialogOpen = true;

            var dialog = new AppUpdatedDialogView();
            await dialog.ShowAsync();

            _isDialogOpen = false;
        }

        public async Task ShowSettingsDialog()
        {
            if (_isDialogOpen)
                return;

            _isDialogOpen = true;

            var dialog = new SettingsView();
            await dialog.ShowAsync();

            _isDialogOpen = false;
        }

        public async Task ShowCalendarDialog(MainViewModel mainViewModel)
        {
            if (_isDialogOpen)
                return;

            _isDialogOpen = true;

            var dialog = new CalendarDialogView(mainViewModel);
            await dialog.ShowAsync();

            _isDialogOpen = false;
        }
    }
}