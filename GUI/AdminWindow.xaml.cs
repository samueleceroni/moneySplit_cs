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
using DatabaseController;

namespace GUI
{
    /// <summary>
    /// Interaction logic for AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        private static readonly string AMOUNT_MUST_BE_A_NUMBER = "Amount must be a number.";
        private static readonly string IBAN_NOT_SELECTED = "Iban not selected.";
        private static readonly string DESCRIPTION = "New Bank account transaction automatically created.";
        private static readonly string ERROR = "Error.";
        private static readonly string SUCCESSFULLY_ADDED = "Successfully added.";

        public AdminWindow()
        {
            InitializeComponent();
            Loaded += OnLoad;
            Closed += new EventHandler(OnClose);
            LoadIbans();
            LoadTags();
            LoadStores();
        }

        private void LoadStores()
        {
            var stores = DatabaseController.DatabaseController.GetStores();
            storeComboBox.Items.Clear();
            foreach (var store in stores)
            {
                storeComboBox.Items.Add(store.VatAccount);
            }
        }

        private void LoadTags()
        {
            var tags = DatabaseController.DatabaseController.GetTagScore();
            tagScoreboardDataGrid.Items.Clear();
            foreach (var tag in tags)
            {
                tagScoreboardDataGrid.Items.Add(tag);
            }
        }

        private void LoadIbans()
        {
            ibanAccountSelectorComboBox.Items.Clear();
            var ibans = DatabaseController.DatabaseController.GetAllIban();
            foreach (string iban in ibans)
            {
                ibanAccountSelectorComboBox.Items.Add(iban);
            }
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            this.Owner.Hide();
        }

        void OnClose(object sender, EventArgs e)
        {
            this.Owner.Show();
        }

        private void AddTransactionToBankAccountButton_Click(object sender, RoutedEventArgs e)
        {
            var checks = Result.Create(decimal.TryParse(addTransactionToBankAccountAmountTextBox.Text, out decimal amount), AMOUNT_MUST_BE_A_NUMBER)
                  .Ensure(() => !string.IsNullOrWhiteSpace(ibanAccountSelectorComboBox.Text.Trim()), IBAN_NOT_SELECTED);
            if (checks.IsFailure) { MessageBox.Show(checks.Error, ERROR, MessageBoxButton.OK, MessageBoxImage.Error); return; }
            DatabaseController.DatabaseController
                              .RegisterNewBankAccountTransaction(ibanAccountSelectorComboBox.Text.Trim(), amount, DESCRIPTION)
                              .OnSuccess(() => MessageBox.Show(SUCCESSFULLY_ADDED, SUCCESSFULLY_ADDED, MessageBoxButton.OK, MessageBoxImage.Information))
                              .OnFailure(error => MessageBox.Show(error, ERROR, MessageBoxButton.OK, MessageBoxImage.Error));
        }

        private void StoreComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(storeComboBox.SelectedItem != null)
            {
                storeReviewsDataGrid.Items.Clear();
                var reviews = DatabaseController.DatabaseController.GetStoreReviews((string)storeComboBox.SelectedItem);
                foreach (var review in reviews)
                {
                    storeReviewsDataGrid.Items.Add(review);
                }
            }
        }
    }
}
