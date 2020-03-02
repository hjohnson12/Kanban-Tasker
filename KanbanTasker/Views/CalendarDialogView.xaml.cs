using System;
using System.Collections.Generic;
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

namespace KanbanTasker.Views
{
    public sealed partial class CalendarDialogView : ContentDialog
    {
        public ViewModels.MainViewModel ViewModel { get; set; }
        public ViewModels.CalendarViewModel CalendarViewModel { get; set; }

        public CalendarDialogView(ViewModels.MainViewModel viewModel)
        {
            this.InitializeComponent();
            ViewModel = viewModel;
            CalendarViewModel = new ViewModels.CalendarViewModel();
        }

        private void btnCloseNewBoardFlyout_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void TaskCalendarView_SelectedDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs args)
        {
            if (sender.SelectedDates != null && sender.SelectedDates.Count != 0)
                // Work-around: Fix using the AttachedProperty SelectedDate
                CalendarViewModel.SelectedDate = sender.SelectedDates.First();

            CalendarViewModel.ScheudledTasks = CalendarViewModel.GetAvailableTasks(ViewModel.CurrentBoard.Board);
        }
    }
}
