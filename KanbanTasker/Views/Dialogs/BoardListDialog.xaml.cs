﻿using System;
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

namespace KanbanTasker.Views.Dialogs
{
    public sealed partial class BoardListDialog : ContentDialog
    {
        public BoardListDialog()
        {
            this.InitializeComponent();

           
        }

        public ViewModels.MainViewModel ViewModel => (ViewModels.MainViewModel) DataContext;

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
        }

        private void DeleteItemsBtn_Click(object sender, RoutedEventArgs e)
        {
            // Retreive selected items from list view to delete
            var selectedItems = BoardListView.SelectedItems.ToArray();
            foreach (ViewModels.BoardViewModel selectedItem in selectedItems)
            {
                // Delete the selected boards and update list
                ViewModel.DataProvider.Call(x => x.BoardServices.DeleteBoard(selectedItem.Board.ID));
                ViewModel.BoardList.Remove(selectedItem);
            }

            // Change the current board if the selected one is deleted
            if (selectedItems.Contains(ViewModel.CurrentBoard))
            {
                ViewModel.CurrentBoard.Board.Name = ""; // uwp bug
                ViewModel.CurrentBoard.Board.Notes = ""; // uwp bug
                ViewModel.CurrentBoard = null; // uwp bug
                ViewModel.CurrentBoard = ViewModel.BoardList.LastOrDefault();
            }

            if (BoardListView.Items.Count.Equals(0))
                NoBoardsTextBlock.Visibility = Visibility.Visible;
        }

        private void BoardListDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (BoardListView.Items.Count.Equals(0))
                NoBoardsTextBlock.Visibility = Visibility.Visible;
            else
                NoBoardsTextBlock.Visibility = Visibility.Collapsed;
        }
    }
}
