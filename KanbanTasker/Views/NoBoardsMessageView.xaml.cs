using System;
using Windows.UI.Xaml.Controls;

namespace KanbanTasker.Views
{
    /// <summary>
    /// A view that can be navigated to within a frame for displaying 
    /// when there are no boards
    /// </summary>
    public sealed partial class NoBoardsMessageView : Page
    {
        public NoBoardsMessageView()
        {
            this.InitializeComponent();
        }
    }
}