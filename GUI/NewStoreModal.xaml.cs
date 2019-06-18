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
using CSharpFunctionalExtensions;

namespace GUI
{
    /// <summary>
    /// Interaction logic for newStoreModal.xaml
    /// </summary>
    public partial class NewStoreModal : Window
    {

        private static readonly string SUCCESSFULLY_CREATED = "Store successfully created.";
        private static readonly string ERROR = "Error.";

        public NewStoreModal()
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DatabaseController.DatabaseController
                              .RegisterNewStore(vatAccountTextBox.Text, storeNameTextBox.Text, storeAddressTextBox.Text)
                              .OnSuccess(() => MessageBox.Show(SUCCESSFULLY_CREATED, SUCCESSFULLY_CREATED, MessageBoxButton.OK, MessageBoxImage.Information))
                              .OnFailure(error => MessageBox.Show(error, ERROR, MessageBoxButton.OK, MessageBoxImage.Error));
        }
    }
}
