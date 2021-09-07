using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanbanTasker.Services
{
    /// <summary>
    /// Interface for handling navigation within a frame.
    /// </summary>
    public interface INavigationService
    {
        object Frame { get; set; }
        void NavigateTo(Type type);
        void NavigateTo(Type type, object parameter);
        void NavigateToDefaultView();
        void NavigateToBoard(object boardViewModel);
    }
}