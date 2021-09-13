using Microsoft.Xaml.Interactivity;
using Syncfusion.UI.Xaml.Kanban;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KanbanTasker.Behaviors
{
    public class SaveColumnButtonClickBehavior : Behavior<Button>
    {
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                "Command",
                typeof(ICommand),
                typeof(SaveColumnButtonClickBehavior),
                new PropertyMetadata(default(ICommand)));

        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject != null)
            {
                AssociatedObject.Click += AssociatedObject_Click;
            }
        }

        private void AssociatedObject_Click(object sender, RoutedEventArgs e)
        {
            var header = 
                ((sender as Button).CommandParameter as ColumnTag).Header.ToString();

            if (Command != null)
                Command.Execute(header);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject != null)
            {
                AssociatedObject.Click -= AssociatedObject_Click;
            }
        }
    }
}
