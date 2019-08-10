using KanbanTasker.Models;
using KanbanTasker.ViewModels;
using Syncfusion.UI.Xaml.Kanban;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace KanbanTasker.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainView : Page
    {
        public BoardViewModel BoardVM { get; set; }
        public MainViewModel ViewModel { get; set; }

        public MainView()
        {
            this.InitializeComponent();

            // Set XAML element as a draggable region.
            Window.Current.SetTitleBar(AppTitleBar);

            //ViewModel = new BoardViewModel();
            ViewModel = App.mainViewModel;

            BoardVM = new BoardViewModel();
        }

        private void BtnCloseNewBoardFlyout_Click(object sender, RoutedEventArgs e)
        {
            newBoardFlyout.Hide();
        }

        private void BtnCloseEditBoardFlyout_Click(object sender, RoutedEventArgs e)
        {
            editBoardFlyout.Hide();
        }

        private void FlyoutBtnCreateNewBoard_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CreateBoard();
            newBoardFlyout.Hide();
        }

        private void NewBoardFlyout_Opening(object sender, object e)
        {
            ResetNewBoardFlyout();
        }

        private void ResetNewBoardFlyout()
        {
            ViewModel.BoardName = "";
            ViewModel.BoardNotes = "";
        }
    }
}
