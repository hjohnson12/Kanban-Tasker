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
            if (txtBoxNewBoardName.Text == "")
                ChooseBoardNameTeachingTip.IsOpen = true;
            else
            {

                if (ViewModel.BoardList.Count == 0)
                {
                    var newBoard = ViewModel.CreateBoard();
                   // kanbanNavView.SelectedItem = newBoard as BoardViewModel;
                }
                else if (ViewModel.BoardList.Count > 0)
                {
                    var newBoard = ViewModel.CreateBoard();
                    newBoardFlyout.Hide();
                }
                
            }
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

        private async void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SettingsView();
            var result = await dialog.ShowAsync();
        }

        private void KanbanNavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var selectedBoard = args.InvokedItem as BoardViewModel;
            contentFrame.Navigate(typeof(BoardView), selectedBoard);
        }

        private void KanbanNavView_Loaded(object sender, RoutedEventArgs e)
        {
            // TO-DO: Check for when there are no boards on the screen
            kanbanNavView.SelectedItem = ViewModel.BoardList[0];
            contentFrame.Navigate(typeof(BoardView), ViewModel.BoardList[0]);
        }

        private void FlyoutBtnDeleteBoard_Click(object sender, RoutedEventArgs e)
        {
            var currentBoard = kanbanNavView.SelectedItem as BoardViewModel;
            var currentBoardId = currentBoard.BoardId;
            var oldIndex = kanbanNavView.MenuItems.IndexOf(kanbanNavView.SelectedItem);
            if (oldIndex > 0)
            {
                // Delete board from BoardList and then the database
                var deleteBoardSuccess = ViewModel.DeleteBoard(currentBoard);
                kanbanNavView.SelectedItem = kanbanNavView.MenuItems[oldIndex - 1] as BoardViewModel;
                contentFrame.Navigate(typeof(BoardView), ViewModel.BoardList[0]);
            }
            else
            {

                // Navigate to a page saying that there are no boards 
            }
        }

        private void NavViewBtnDeleteBoard_Click(object sender, RoutedEventArgs e)
        {
            var currentBoard = kanbanNavView.SelectedItem as BoardViewModel;
            var currentBoardId = currentBoard.BoardId;
            var oldIndex = ViewModel.BoardList.IndexOf(currentBoard);
            if (oldIndex > 0)
            {
                kanbanNavView.SelectedItem = ViewModel.BoardList[oldIndex - 1];

                // Deletes both the board and corresponding tasks, then sets new view
                var deleteBoardSuccess = ViewModel.DeleteBoard(currentBoard);
                var newBoard = ViewModel.BoardList[oldIndex - 1];
                contentFrame.Navigate(typeof(BoardView), newBoard);
            }
            else
            {
                var deleteBoardSuccess = ViewModel.DeleteBoard(currentBoard);
               
                contentFrame.Navigate(typeof(BoardView), ViewModel.CreateDefaultBoard());

                // Navigate to a page saying that there are no boards 
            }
        }
    }
}
