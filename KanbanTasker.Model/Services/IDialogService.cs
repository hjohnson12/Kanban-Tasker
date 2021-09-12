using System;
using System.Threading.Tasks;

namespace KanbanTasker.Model.Services
{
    /// <summary>
    /// Interface that provides methods for displaying dialogs.
    /// </summary>
    public interface IDialogService
    {
        bool CheckForOpenDialog();
        Task ShowFirstRunDialog();
        Task ShowAppUpdatedDialog();
        Task ShowSettingsDialog();
        Task ShowEditBoardDialog(object viewModel);
        Task ShowCalendarDialog(object viewModel);
    }
}