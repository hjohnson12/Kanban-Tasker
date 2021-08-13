using KanbanTasker.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanbanTasker.Services
{
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