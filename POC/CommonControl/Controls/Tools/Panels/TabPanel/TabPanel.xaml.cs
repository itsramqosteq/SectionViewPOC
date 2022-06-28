using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace POC
{
    /// <summary>
    /// Interaction logic for FooterPanel.xaml
    /// </summary>
    public partial class TabPanelUserControl : UserControl
    {
        public event SelectionChangedEventHandler SelectionChanged;
        public delegate void SelectionChangedEventHandler(object sender);
        public static readonly DependencyProperty SelectedIndexProperty =
             DependencyProperty.Register("SelectedIndex", typeof(int),
               typeof(TabPanelUserControl), new PropertyMetadata(0, OnPropertyChangedSelectedIndex));
        
        public static readonly DependencyProperty ItemsSourceProperty =
           DependencyProperty.Register("ItemsSource", typeof(List<CustomTab>),
             typeof(TabPanelUserControl), new PropertyMetadata(new List<CustomTab>(), OnPropertyChangedItemsSource));
        public List<CustomTab> ItemsSource { get { return (List<CustomTab>)GetValue(ItemsSourceProperty); } set { SetValue(ItemsSourceProperty, value); } }
        public int SelectedIndex { get { return (int)GetValue(SelectedIndexProperty); } set { SetValue(SelectedIndexProperty, value); } }
        private static void OnPropertyChangedSelectedIndex(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TabPanelUserControl control = (TabPanelUserControl)d;
            if (control.ItemsSource.Any())
            {
                control.lstTab.SelectedIndex =(int) e.NewValue;
            }
        }
        private static void OnPropertyChangedItemsSource(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TabPanelUserControl control = (TabPanelUserControl)d;
            List<CustomTab>  customTabs = e.NewValue as List<CustomTab>;
            if (customTabs.Any())
            {
                control.lstTab.ItemsSource = customTabs;
                control.lstTab.SelectedIndex =control.SelectedIndex;

            }
        }
            public TabPanelUserControl()
        {
            InitializeComponent();

            if (lstTab.ItemsSource != null)
            {
                lstTab.SelectedIndex = 0;
            }

        }

        private void LstTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBoxItem listBox = ((sender as ListBox).SelectedItem as ListBoxItem);
            SelectedIndex = lstTab.SelectedIndex;
            SelectionChanged?.Invoke(this);
        }
    }
}
