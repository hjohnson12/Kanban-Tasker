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
        public ViewModels.CalendarViewModel CalViewModel { get; set; }

        public CalendarDialogView(ViewModels.MainViewModel viewModel)
        {
            this.InitializeComponent();

            ViewModel = viewModel;
            CalViewModel = new ViewModels.CalendarViewModel();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_tick;
            timer.Start();
        }

        private void timer_tick(object sender, object e)
        {
            txtCurrentTime.Text = DateTime.Now.ToLongTimeString();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void btnCloseNewBoardFlyout_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void TaskCalendarView_SelectedDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs args)
        {
            if (sender.SelectedDates != null && sender.SelectedDates.Count != 0)
                // Work-around: Fix using the AttachedProperty SelectedDate
                CalViewModel.SelectedDate = sender.SelectedDates.First();

            CalViewModel.ScheudledTasks = CalViewModel.GetAvailableTasks(ViewModel.CurrentBoard.Board);

        }
    }
}
