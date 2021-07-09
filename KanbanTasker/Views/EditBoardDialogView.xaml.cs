using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace KanbanTasker.Views
{
    public sealed partial class EditBoardDialogView : ContentDialog
    {
        public ViewModels.MainViewModel ViewModel { get; set; }
        public EditBoardDialogView(ViewModels.MainViewModel viewModel)
        {
            this.InitializeComponent();

            ViewModel = viewModel;
        }

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