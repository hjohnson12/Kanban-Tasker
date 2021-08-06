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

namespace KanbanTasker.Views
{
    /// <summary>
    /// A view for displaying a message when the app is updated
    /// </summary>
    public sealed partial class AppUpdatedDialogView : ContentDialog
    {
        public AppUpdatedDialogView()
        {
            this.InitializeComponent();
        }

        private void btnCloseDialog_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}