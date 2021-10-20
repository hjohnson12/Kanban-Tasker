using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KanbanTasker.Views.Dialogs
{
    public sealed partial class EditBoardDialog : ContentDialog
    {
        public EditBoardDialog()
        {
            this.InitializeComponent();
        }

        public ViewModels.MainViewModel ViewModel => (ViewModels.MainViewModel)DataContext;

        private void btnCloseNewBoardFlyout_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void flyoutBtnCreateNewBoard_Click(object sender, RoutedEventArgs e)
        {
            if (txtBoxNewBoardName.Text == "")
                ChooseBoardNameTeachingTip.IsOpen = true;
            if (txtBoxNewBoardNotes.Text == "")
                AddBoardNotesTeachingTip.IsOpen = true;
            if (txtBoxNewBoardName.Text != "" && txtBoxNewBoardNotes.Text != "")
            {
                this.Hide();
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}