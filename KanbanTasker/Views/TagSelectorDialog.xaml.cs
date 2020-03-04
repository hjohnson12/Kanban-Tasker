using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    public sealed partial class TagSelectorDialog : ContentDialog
    {
        public ViewModels.BoardViewModel ViewModel { get; set; }
        const int MIDDLE = 382; // Middle sum of RGB - Max is 765
        string colorCode = "";
        public TagSelectorDialog(ViewModels.BoardViewModel viewModel)
        {
            this.InitializeComponent();
            ViewModel = viewModel;

            // test data
            for (int i = 0; i <= 50; i++)
                lstViewTags.Items.Add(new ListViewItem() { Content = "Test" });
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private string ToStringg(int v)
        {
            return v.ToString();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        /// <summary>
        /// Gets RGB values of color object passed and returns the sum.
        /// </summary>
        /// <param name="c"></param>
        /// <returns>Sum of parameters RGB values.</returns>
        private int ConvertToRGB(Windows.UI.Color c)
        {
            int r = c.R, // RED component value
                g = c.G, // GREEN component value
                b = c.B; // BLUE component value

            return r + g + b;
        }

        private void tagColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            // Note: Old code copied here for later

            //ViewModel.TagBackground = new SolidColorBrush(sender.Color);
            // ViewModel.TagBackground = sender.Color;

            //var color = Color.FromArgb(sender.Color.A, sender.Color.R, sender.Color.G, sender.Color.B);
            //colorCode = sender.Color.ToString(); // ex: #FFFFFF

            // 255,255,255 = White and 0,0,0 = Black
            // Max sum of RGB values is 765 -> (255 + 255 + 255)
            // Middle sum of RGB values is 382 -> (765/2)
            // Color is considered darker if its <= 382
            // Color is considered lighter if its > 382
            //int sumRGB = ConvertToRGB(color);    // get the color objects sum of the RGB value
            //if (sumRGB <= MIDDLE)          // Darker Background
            //{
            //    // (ViewModel.CurrentTask.TagListViewItem as ListViewItem).Foreground = new SolidColorBrush(Colors.White); // Set to white text
            //    ViewModel.TagForeground = new SolidColorBrush(Colors.White); // Set to white text
            //}
            //else if (sumRGB > MIDDLE)     // Lighter Background
            //{
            //    //  (ViewModel.CurrentTask.TagListViewItem as ListViewItem).Foreground = new SolidColorBrush(Colors.Black); // Set to black text
            //    ViewModel.TagForeground = new SolidColorBrush(Colors.Black); // Set to black text
            //}
        }

        private void lstViewTags_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txtCount.Text = (sender as ListView).SelectedItems.Count.ToString() + " item(s) selected";
        }

        private void btnSaveTags_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnCloseDialog_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
