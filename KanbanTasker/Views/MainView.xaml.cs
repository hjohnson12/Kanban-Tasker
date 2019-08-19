using KanbanTasker.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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

            ViewModel = App.mainViewModel;

            BoardVM = new BoardViewModel();
            foreach (BoardViewModel boardViewModel in ViewModel.BoardList)
            {
                kanbanNavView.MenuItems.Add(boardViewModel);
            }
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
            {
                ChooseBoardNameTeachingTip.IsOpen = true;
            }
            if (txtBoxNewBoardNotes.Text == "")
            {
                AddBoardNotesTeachingTip.IsOpen = true;
            }
            else
            {
                kanbanNavView.SelectedItem = null;

                ViewModel.CreateBoard();

                kanbanNavView.MenuItems.Add(ViewModel.BoardList[ViewModel.BoardList.Count - 1]);
                kanbanNavView.SelectedItem = ViewModel.BoardList[ViewModel.BoardList.Count - 1];
                contentFrame.Navigate(typeof(BoardView), ViewModel.BoardList[ViewModel.BoardList.Count - 1]);

                newBoardFlyout.Hide();
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
            if (ViewModel.BoardList.Count == 0)
                contentFrame.Navigate(typeof(NoBoardsMessageView));
            else
            {
                kanbanNavView.SelectedItem = ViewModel.BoardList[0];
                contentFrame.Navigate(typeof(BoardView), ViewModel.BoardList[0]);
            }
        }

        private void BtnDeleteCurrentBoard_Click(object sender, RoutedEventArgs e)
        {
            var currentBoard = kanbanNavView.SelectedItem as BoardViewModel;
            deleteBoardFlyout.Hide();
            if (currentBoard != null)
            {
                try
                {
                    var deleteBoardSuccess = ViewModel.DeleteBoard(currentBoard);
                    if (deleteBoardSuccess)
                    {
                        kanbanNavView.SelectedItem = null;
                        var index = kanbanNavView.MenuItems.IndexOf(currentBoard);
                        kanbanNavView.MenuItems.Remove(currentBoard);
                        if (ViewModel.BoardList.Count == 0)
                        {
                            TitleBarCurrentBoardTextblock.Text = ""; // Clear heading on title bar
                            contentFrame.Navigate(typeof(NoBoardsMessageView));
                        }
                        else
                        {
                            kanbanNavView.SelectedItem = ViewModel.BoardList[index - 1];
                            contentFrame.Navigate(typeof(BoardView), ViewModel.BoardList[index - 1]);
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    KanbanInAppNotification.Show("Unable to delete board. Try again or restart the application and then try.", 3000);
                }
            }
            else
                UnableToDeleteBoardTeachingTip.IsOpen = true;
        }

        private void FlyoutBtnUpdateBoard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentBoard = kanbanNavView.SelectedItem as BoardViewModel;
                var currentIndex = kanbanNavView.MenuItems.IndexOf(currentBoard);
                var updateBoardSuccess = ViewModel.UpdateBoard(currentBoard, currentIndex);
                editBoardFlyout.Hide();
                if (updateBoardSuccess)
                {
                    // Already updated in db, now update navview
                    currentBoard.BoardName = ViewModel.BoardName;
                    currentBoard.BoardNotes = ViewModel.BoardNotes;
                    kanbanNavView.MenuItems[currentIndex] = currentBoard;
                    ViewModel.Current = currentBoard;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                KanbanInAppNotification.Show("Unable to update board. Try again or restart the application and then try.", 3000);
            }
        }

        private void EditBoardFlyout_Opening(object sender, object e)
        {
            if (kanbanNavView.SelectedItem != null)
            {
                ViewModel.BoardName = (kanbanNavView.SelectedItem as BoardViewModel).BoardName;
                ViewModel.BoardNotes = (kanbanNavView.SelectedItem as BoardViewModel).BoardNotes;
            }
            else
            {
                var flyout = sender as Flyout;
                flyout.Hide();
                UnableToEditBoardTeachingTip.IsOpen = true;
            }
        }

        private void KanbanNavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            ViewModel.Current = args.SelectedItem as BoardViewModel;
        }

        private void DeleteBoardFlyout_Opening(object sender, object e)
        {
            if (kanbanNavView.SelectedItem == null)
            {
                var flyout = sender as Flyout;
                flyout.Hide();
                UnableToDeleteBoardTeachingTip.IsOpen = true;
            }
        }
    }
}
