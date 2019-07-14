using Syncfusion.UI.Xaml.Kanban;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;

namespace KanbanTasker.Helpers
{
    /*
     * Class provided by Syncfusion for creating custom columns in the kanban board
     * 
     * This class is used to work with the collapsing of the column
     * 
     */
    public class CustomKanbanColumn : KanbanColumn
    {

        #region Fields

        private ControlTemplate collapsedTemplate;
        private ControlTemplate expandedTemplate;

        #endregion

        #region ctor
        public CustomKanbanColumn()
        {
            PointerReleased += KanbanColumnAdv_PointerReleased;
            expandedTemplate = KanbanDictionaries.GenericCommonDictionary["ExpandedTemplate"] as ControlTemplate;
            collapsedTemplate = KanbanDictionaries.GenericCommonDictionary["CollapsedTemplate"] as ControlTemplate;
        }

        #endregion


        #region Properties

        public new bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public new static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
            "IsExpanded", typeof(bool), typeof(CustomKanbanColumn), new PropertyMetadata(true, OnIsExpandedChanged));

        public ControlTemplate CollapsedColumnTemplate
        {
            get { return (ControlTemplate)GetValue(CollapsedColumnTemplateProperty); }
            set { SetValue(CollapsedColumnTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CollapsedColumnTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CollapsedColumnTemplateProperty =
            DependencyProperty.Register("CollapsedColumnTemplate", typeof(ControlTemplate), typeof(CustomKanbanColumn), new PropertyMetadata(null));

        #endregion


        #region Methods

        private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomKanbanColumn column = d as CustomKanbanColumn;

            column.ExpandedChanged((bool)e.NewValue);
        }

        private void ExpandedChanged(bool isExpanded)
        {
            if (Tags != null)
                Tags.IsExpanded = IsExpanded;

            if (!isExpanded)
            {
                ClearValue(Control.WidthProperty);
                Width = 50;

                ClearValue(Control.TemplateProperty);

                if (CollapsedColumnTemplate != null)
                {
                    Template = CollapsedColumnTemplate;
                }
                else if (collapsedTemplate != null)
                {
                    Template = collapsedTemplate;
                }

                foreach (KanbanCardItem card in Cards)
                {
                    card.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                ClearValue(Control.WidthProperty);

                PropertyInfo propInfo = typeof(KanbanColumn).GetProperty("Area", BindingFlags.Instance |
                                                                                 BindingFlags.NonPublic |
                                                                                 BindingFlags.Public);

                SfKanban area = propInfo.GetValue(this) as SfKanban;

                if (area != null)
                {
                    Binding binding = new Binding()
                    {
                        Source = area,
                        Path = new PropertyPath("ActualColumnWidth")
                    };
                    SetBinding(Control.WidthProperty, binding);
                }

                if (expandedTemplate != null)
                {
                    Template = expandedTemplate;
                }

                foreach (KanbanCardItem card in Cards)
                {
                    card.Visibility = Visibility.Visible;
                }
            }
        }

        private void KanbanColumnAdv_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (e.OriginalSource is Border &&
                    ((e.OriginalSource as Border).Name == "CollapsedIcon"))
            {
                IsExpanded = false;
            }
            else if (!IsExpanded)
            {
                IsExpanded = true;
            }
        }

        #endregion
    }
}
