using KanbanTasker.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanbanTasker.Services
{
    /// <summary>
    /// Interface for displaying content dialogs.
    /// </summary>
    public interface IDialogService
    {
        bool CheckForOpenDialog();

        Task ShowEditDialog(MainViewModel mainViewModel);

        Task ShowFirstRunDialog();

        Task ShowAppUpdatedDialog();

        Task ShowSettingsDialog();

        Task ShowCalendarDialog(MainViewModel mainViewModel);
    }
}