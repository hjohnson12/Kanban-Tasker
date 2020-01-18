using KanbanTasker.Models;
using KanbanTasker.ViewModels;
using Syncfusion.UI.Xaml.Kanban;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace KanbanTasker.Views
{
    public sealed partial class BoardView : Page
    {
        public BoardViewModel ViewModel { get; set; }

        public BoardView()
        {
            this.InitializeComponent();
            DataContext = ViewModel;
            kanbanBoard.CardStyle.CornerRadius = new CornerRadius(3);
        }

        #region Methods

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var selectedBoard = e.Parameter as BoardViewModel;
            ViewModel = selectedBoard;
        }

        public void ShowContextMenu(PresentationTask selectedModel)
        {
            // Workaround to show context menu next to selected card model
            foreach (var col in kanbanBoard.ActualColumns)
            {
                if (col.Categories.Contains(selectedModel.Category.ToString()))
                {
                    // Find card inside column
                    foreach (var card in col.Cards)
                    {
                        int cardIndex = 0;
                        var cardModel = card.Content as PresentationTask;
                        if (cardModel.ID == selectedModel.ID)
                        {
                            // Get current index of card
                            cardIndex = col.Cards.IndexOf(card);
                        }

                        // Set flyout to selected card index
                        for (int i = 0; i <= col.Cards.Count; i++)
                        {
                            if (i == cardIndex)
                            {
                                FlyoutShowOptions myOption = new FlyoutShowOptions();
                                myOption.ShowMode = FlyoutShowMode.Transient;
                                taskFlyout.ShowAt(col.Cards[i], myOption);
                            }
                        }
                    }
                }
            }
        }
        #endregion Methods

        #region UIEvents

        private void FlyoutBtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Hide flyout
            taskFlyout.Hide();

            // Open pane if closed
            if (splitView.IsPaneOpen == false)
                splitView.IsPaneOpen = true;

            // Give title textbox focus once pane opens
            txtBoxTitle.Focus(FocusState.Programmatic);
            txtBoxTitle.SelectionStart = txtBoxTitle.Text.Length;
            txtBoxTitle.SelectionLength = 0;
        }

        private void CardBtnEdit_Click(object sender, RoutedEventArgs e)
        {
            taskFlyout.Hide();

            if (splitView.IsPaneOpen == false)
                splitView.IsPaneOpen = true;

            // Give title textbox focus once pane opens
            txtBoxTitle.Focus(FocusState.Programmatic);
            txtBoxTitle.SelectionStart = txtBoxTitle.Text.Length;
            txtBoxTitle.SelectionLength = 0;
        }

        private void CardBtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var originalSource = (FrameworkElement)sender;

            // Show flyout attached to button
            // Delete task if "Yes" button is clicked inside flyout
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void BtnNewTaskCurrentColumn_Click(object sender, RoutedEventArgs e)
        {
            // Open pane if not already
            if (splitView.IsPaneOpen == false)
                splitView.IsPaneOpen = true;

            txtBoxTitle.Focus(FocusState.Programmatic);
        }

        private void appBarBtnClosePane_Click(object sender, RoutedEventArgs e)
        {
            // Reset changes and close pane
            // To Do: Change when adding task

            if (splitView.IsPaneOpen == true)
                splitView.IsPaneOpen = false;

        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Reset changes and close pane
            // To Do: Change when adding task

            if (splitView.IsPaneOpen == true)
                splitView.IsPaneOpen = false;

        }

        private void BtnSaveTask_Click(object sender, RoutedEventArgs e)
        {
            // Close pane when done
            if (splitView.IsPaneOpen == true)
                splitView.IsPaneOpen = false;
        }

        private void TxtBoxTags_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            // Add Tag to listview on keydown event
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var tagsTextBox = sender as TextBox;

                if (string.IsNullOrEmpty(tagsTextBox.Text))
                    return;

                if (ViewModel.AddTag(tagsTextBox.Text))
                    tagsTextBox.Text = string.Empty;

            }
        }

        private void MnuItemExitApp_Click(object sender, RoutedEventArgs e)
        {
            CoreApplication.Exit();
        }

        private void Card_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            // Pre: Get information to pass to the dialog for displaying
            //      Set corresponding properties in TaskDialog
            // Post: Information passed, dialog opened

            // Always show in standard mode
            var originalSource = (FrameworkElement)sender;
            //ShowContextMenu(SelectedModel);
        }

        private void KanbanBoard_CardDragEnd(object sender, KanbanDragEndEventArgs e)
        {
            // Change column and category of task when dragged to new column
            // ObservableCollection Tasks already updated
            var targetCategory = e.TargetKey.ToString();
            var selectedCardModel = e.SelectedCard.Content as PresentationTask;
            int sourceCardIndex = e.SelectedCardIndex;
            int targetCardIndex = e.TargetCardIndex;
            ViewModel.UpdateCardColumn(targetCategory, selectedCardModel, targetCardIndex);

            // Reorder cards when dragging to & from same column
            if (e.TargetColumn.Title.ToString() == e.SelectedColumn.Title.ToString())
            {
                // Update every card index in the column after rearrange
                foreach (var card in e.TargetColumn.Cards)
                {
                    int currentIndex = e.TargetColumn.Cards.IndexOf(card);
                    if (currentIndex != Convert.ToInt32(targetCardIndex))
                    {
                        var currentModel = card.Content as PresentationTask;
                        currentModel.ColumnIndex = currentIndex;
                        ViewModel.UpdateCardIndex(currentModel.ID, currentIndex);

                        // NOTE FROM DEBUGGING:

                        //  After its updated in the database, it's not updating the Tasks list? so next iteration when I delete second card, it's using old tasks?
                        // Found inside of (this) at runtime by viewing the ViewModel and PresentationBoard inside it
                        // Part of the bug explained in BoardViewModel.cs, Line 222. Low severity, current fixes stops the major crashing, this is just a hidden issue
                    }
                }
            }
            // Reorder cards when dragging from one column to another 
            else
            {
                // Reorder target col after drop
                // Only items above the targetCardIndex need to be updated
                if (e.TargetColumn.Cards.Count != 0)
                {
                    foreach (var card in e.TargetColumn.Cards)
                    {
                        int currentIndex = e.TargetColumn.Cards.IndexOf(card);
                        if (currentIndex > Convert.ToInt32(targetCardIndex))
                        {
                            var currentModel = card.Content as PresentationTask;
                            currentModel.ColumnIndex = currentIndex;
                            ViewModel.UpdateCardIndex(currentModel.ID, currentIndex);
                        }
                    }
                }

                // Reorder source column after dragged card is removed
                if (e.SelectedColumn.Cards.Count != 0)
                {
                    foreach (var card in e.SelectedColumn.Cards)
                    {
                        int currentIndex = e.SelectedColumn.Cards.IndexOf(card);
                        var currentModel = card.Content as PresentationTask;
                        currentModel.ColumnIndex = currentIndex;
                        ViewModel.UpdateCardIndex(currentModel.ID, currentIndex);
                    }
                }
            }
        }

        private void FlyoutDeleteCardBtnYes_Click(object sender, RoutedEventArgs e)
        {
            splitView.IsPaneOpen = false;
        }

        private void PaneBtnDeleteTaskYes_Click(object sender, RoutedEventArgs e)
        {
        
            // Close pane when done
            splitView.IsPaneOpen = false;
            PaneBtnDeleteTaskConfirmationFlyout.Hide();
        }

        /// <summary>
        /// Used for touch screen users, but works for PC users too
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void FlyoutBtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Hide flyout
            taskFlyout.Hide();

            // Create dialog and check button click result
            var deleteDialog = new DeleteConfirmationView();
            var result = await deleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Close pane when done
                splitView.IsPaneOpen = false;

                // Delete Task from collection and databaseIn
                var deleteSuccess = true;

            }
            else
                return;
        }

        #endregion UIEvents

        private void btnDeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (PaneBtnDeleteTaskConfirmationFlyout.IsOpen)
                PaneBtnDeleteTaskConfirmationFlyout.Hide();
            else
                PaneBtnDeleteTaskConfirmationFlyout.ShowAt((FrameworkElement)sender);
                //FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender); 
        }
    }
}
