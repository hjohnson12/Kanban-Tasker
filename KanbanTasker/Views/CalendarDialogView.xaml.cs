using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KanbanTasker.Models;

namespace KanbanTasker.Views
{
    public sealed partial class CalendarDialogView : ContentDialog
    {
        public ViewModels.MainViewModel ViewModel { get; set; }
        public ViewModels.CalendarViewModel CalendarViewModel { get; set; }
        private PresentationTask SelectedTask { get; set; }

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
            if(ViewModel.CurrentBoard != null)
            {
                if (sender.SelectedDates != null && sender.SelectedDates.Count != 0)
                    // Work-around: Fix using the AttachedProperty SelectedDate
                    CalendarViewModel.SelectedDate = sender.SelectedDates.First();

                CalendarViewModel.ScheudledTasks = CalendarViewModel.GetAvailableTasks(ViewModel.CurrentBoard.Board);
            }
        }

        private void lstView_ItemClick(object sender, ItemClickEventArgs e)
        {
            SelectedTask = e.ClickedItem as PresentationTask;
            this.Hide();
        }

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            // Note: There is a flicker when closing this dialog
            // Can't fully tell if it's memory leak or control issue
            // Adding stop timer and clearing of tasks to try and help with flickering for now
            CalendarViewModel.StopTimer();
            CalendarViewModel.ScheudledTasks.Clear();

            if(SelectedTask != null)
            {
                // Set CurrentTask and open EditPane
                ViewModel.CurrentBoard.EditTaskCommandHandler(SelectedTask.ID);
                BoardView.MySplitView.IsPaneOpen = true;
                SelectedTask = null;
            }
        }
    }
}