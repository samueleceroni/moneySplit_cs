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
using System.Windows.Shapes;

namespace GUI
{
    /// <summary>
    /// Interaction logic for TransactionWindow.xaml
    /// </summary>
    public partial class TransactionWindow : Window
    {
        public TransactionWindow()
        {
            InitializeComponent();
            Loaded += OnLoad;
            Closed += new EventHandler(OnClose);
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            this.Owner.Hide();
        }

        void OnClose(object sender, EventArgs e)
        {
            this.Owner.Show();
        }

        private void StoreComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            storeReviewLabel.IsEnabled = (storeComboBox.SelectedItem == noStoreComboBoxItem);
            storeReviewTextBox.IsEnabled = (storeComboBox.SelectedItem == noStoreComboBoxItem);
        }
    }
}
