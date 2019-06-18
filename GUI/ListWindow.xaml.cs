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
    /// Interaction logic for ListWindow.xaml
    /// </summary>
    public partial class ListWindow : Window
    {
        private static readonly string ERROR = "Error.";
        private static readonly string SUCCESSFULLY_SHARED = "Successfully shared.";
        private static readonly string AMOUNT_MUST_BE_NUMBER = "Amount for new transaction must be a number.";
        private static readonly string REPETITION_MUST_BE_INTEGER = "Repetition number must be an integer.";
        private static readonly string CONTEXT_ID_MUST_BE_INTEGER= "Context id must be an integer.";
        private static readonly string CHECK_STORE_TRANSACTION_DATA= "Check store transaction data.";
        private static readonly string TRANSACTION_DONE_BUT_STORE_NOT_REGISTERED = "Transaction registered, but not the store and review.";
        private static readonly string STORE_SUCCESSFULLY_ADDED = "Store successfully added.";
        private static readonly string TRANSACTION_ID_MUST_BE_INTEGER = "Transaction id must be an integer.";
        private static readonly string SUCCESSFULLY_DELETED = "Succesfully deleted.";

        private int UserContextID { get; }
        private int ListID { get; }
        private int ListContextID { get; }
        private string ListName { get; }
        private string IBAN { get; }
        private string CF_Owner { get; }

        public ListWindow(int userContextID, int listID)
        {
            UserContextID = userContextID;
            ListID = listID;

            var listResult = DatabaseController.DatabaseController.GetListFullDetail(userContextID, listID);

            if (listResult.IsFailure) { MessageBox.Show(listResult.Error, ERROR, MessageBoxButton.OK, MessageBoxImage.Error); this.Close(); return; }

            var list = listResult.Value;

            ListContextID = (int) list.ContextId;
            ListName = list.Name;
            IBAN = list.Iban ?? string.Empty;
            CF_Owner = list.CF_owner ?? string.Empty;

            InitializeComponent();
            Loaded += OnLoad;
            Closed += new EventHandler(OnClose);

            listIDValueLabel.Content = ListID;
            operatorContextIDValueLabel.Content = userContextID;
            listContextIDValueLabel.Content = ListContextID;
            listNameValueLabel.Content = ListName;
            IBANValueLabel.Content = IBAN;
            CF_OwnerValueLabel.Content = CF_Owner;
            totalValueLabel.Content = list.TotalAmount + " €";
            
            var numberOfVersionsResult = DatabaseController.DatabaseController.GetNumberOfListVersions(UserContextID, listID);
            if (numberOfVersionsResult.IsFailure) { MessageBox.Show(numberOfVersionsResult.Error, ERROR, MessageBoxButton.OK, MessageBoxImage.Error); this.Close(); return; }
            int numberOfVersions = numberOfVersionsResult.Value;
            foreach (int i in Enumerable.Range(1, numberOfVersions))
            {
                versionComboBox.Items.Add(i);
            }
            LoadTransactionsTable();
            LoadStores();
        }

        private void LoadStores()
        {
            var stores = DatabaseController.DatabaseController.GetStores();
            storesComboBox.Items.Clear();

            foreach (var store in stores)
            {
                storesComboBox.Items.Add(store.VatAccount);
            }
        }

        private void LoadTransactionsTable()
        {
            IEnumerable<string> hashtags = null;
            if (hashTagTextBox != null && hashTagTextBox.Text.Trim().Length > 0) hashtags = hashTagTextBox.Text.Trim().Split(' ');
            int? versionNull = null;
            if (Int32.TryParse(versionComboBox.SelectedValue.ToString(), out int version)) { versionNull = version; };
            int? transactionType = null;
            if (transactionTypeComboBox == null) { transactionType = null; }
            else if (transactionTypeComboBox.SelectedItem == OnlyRecurring) { transactionType = 1; }
            else if (transactionTypeComboBox.SelectedItem == OnlyNormal) { transactionType = 0; }
            var transactionsResult = DatabaseController.DatabaseController.GetAllTransaction(UserContextID, ListID, versionNull, transactionType, hashtags);
            if (transactionsResult.IsFailure) { MessageBox.Show(transactionsResult.Error, ERROR, MessageBoxButton.OK, MessageBoxImage.Error); this.Close(); return; }
            var transactions = transactionsResult.Value;
            transactionDataGrid.Items.Clear();
            foreach (var transaction in transactions)
            {
                transactionDataGrid.Items.Add(transaction);
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

        private void AddNewTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            var listIdLastVersion = DatabaseController.DatabaseController.GetIDOfLastVersionOfList(UserContextID, ListID);
            if (listIdLastVersion.IsFailure) { MessageBox.Show(listIdLastVersion.Error, ERROR, MessageBoxButton.OK, MessageBoxImage.Error); this.Close(); return; }

            var amountRes = Result.Create(Decimal.TryParse(newTransactionAmountTextBox.Text, out decimal newTransactionAmount), newTransactionAmount, AMOUNT_MUST_BE_NUMBER);
            if (amountRes.IsFailure) { MessageBox.Show(amountRes.Error, ERROR, MessageBoxButton.OK, MessageBoxImage.Error); return; }

            Result<int> resAddTrans;
            if (newTransactionIsRecurringCheckBox.IsChecked ?? false)
            {
                var repetitionRes = Result.Create(Int32.TryParse(newTransactionRecurrenceRepetitionTextBox.Text, out int repetition), repetition, REPETITION_MUST_BE_INTEGER);
                if (repetitionRes.IsFailure) { MessageBox.Show(repetitionRes.Error, ERROR, MessageBoxButton.OK, MessageBoxImage.Error); return; }
                int? dayRecurrence = null;
                int? monthRecurrence = null;
                var chosen = newTransactionRecurrenceRepetitionComboBox.SelectedItem;
                if (chosen == Day)
                {
                    dayRecurrence = repetition;
                } else if(chosen == Week)
                {
                    dayRecurrence = repetition * 7;
                } else if(chosen == Month)
                {
                    monthRecurrence = repetition;
                } else
                {
                    monthRecurrence = repetition * 12;
                }
                resAddTrans = DatabaseController.DatabaseController
                                                .AddNewTransaction(UserContextID, listIdLastVersion.Value, amountRes.Value, newTransactionDescriptionTextBox.Text, 1, null, dayRecurrence, monthRecurrence, newTransactionRecurrenceStartDatePicker.SelectedDate, newTransactionRecurrenceEndDatePicker.SelectedDate);
            } else
            {
                resAddTrans = DatabaseController.DatabaseController
                                                .AddNewTransaction(UserContextID, listIdLastVersion.Value, amountRes.Value, newTransactionDescriptionTextBox.Text, 0);
            }
            if (resAddTrans.IsFailure) { MessageBox.Show(resAddTrans.Error, ERROR, MessageBoxButton.OK, MessageBoxImage.Error); return; }

            UpdateTotal();

            LoadTransactionsTable();

            if (addStoreCheckBox.IsChecked ?? false)
            {
                string storeVatAccount = storesComboBox.SelectedValue.ToString();

                if (string.IsNullOrWhiteSpace(storeVatAccount))
                {
                    MessageBox.Show(TRANSACTION_DONE_BUT_STORE_NOT_REGISTERED + CHECK_STORE_TRANSACTION_DATA, ERROR, MessageBoxButton.OK, MessageBoxImage.Error); return;
                }

                string revTitle = null;
                string revText = null;
                int? revStar = null;
                if (addStoreReviewCheckBox.IsChecked ?? false)
                {
                    revTitle = titleTextBox.Text.Trim();
                    revText = revTextTextBox.Text.Trim();
                    if ((Int32.TryParse(starTextBox.Text, out int intRevStar)) && !string.IsNullOrWhiteSpace(revTitle) && !string.IsNullOrWhiteSpace(revText))
                    {
                        revStar = intRevStar;
                    } else
                    {
                        MessageBox.Show(TRANSACTION_DONE_BUT_STORE_NOT_REGISTERED + CHECK_STORE_TRANSACTION_DATA, ERROR, MessageBoxButton.OK, MessageBoxImage.Error); return;
                    }
                }
                DatabaseController.DatabaseController
                                  .AddStoreTransaction(storeVatAccount, resAddTrans.Value, revTitle, revText, revStar)
                                  .OnSuccess(() => MessageBox.Show(STORE_SUCCESSFULLY_ADDED, STORE_SUCCESSFULLY_ADDED, MessageBoxButton.OK, MessageBoxImage.Information))
                                  .OnFailure(error => MessageBox.Show(error, ERROR, MessageBoxButton.OK, MessageBoxImage.Error));
            }

        }

        private void UpdateTotal()
        {
            DatabaseController.DatabaseController
                  .GetListFullDetail(UserContextID, ListID)
                  .OnSuccess(list => totalValueLabel.Content = list.TotalAmount + " €")
                  .OnFailure(error => { MessageBox.Show(error, ERROR, MessageBoxButton.OK, MessageBoxImage.Error); this.Close(); return; });
        }

        private void VersionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadTransactionsTable();
        }

        private void TransactionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadTransactionsTable();
        }

        private void ReloadTable_Click(object sender, RoutedEventArgs e)
        {
            LoadTransactionsTable();
        }

        private void ShareListButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Int32.TryParse(shareListContextIdTextBox.Text, out int contextIdToShareTheListWith))
            {
                MessageBox.Show(CONTEXT_ID_MUST_BE_INTEGER, ERROR, MessageBoxButton.OK, MessageBoxImage.Error); return;
            }
            DatabaseController.DatabaseController.ShareList(UserContextID, ListID, contextIdToShareTheListWith)
                              .OnSuccess(sharedContextId => MessageBox.Show(SUCCESSFULLY_SHARED, SUCCESSFULLY_SHARED, MessageBoxButton.OK, MessageBoxImage.Information))
                              .OnFailure(error => MessageBox.Show(error, ERROR, MessageBoxButton.OK, MessageBoxImage.Error));
        }

        private void CreateStore_Click(object sender, RoutedEventArgs e)
        {
            new NewStoreModal { Owner = this }.ShowDialog();
        }

        private void AddStoreCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            addStoreReviewCheckBox.IsChecked = false;
        }

        private void DeleteTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Int32.TryParse(manageTransactionIDTextBox.Text, out int transID)) { MessageBox.Show(TRANSACTION_ID_MUST_BE_INTEGER, ERROR, MessageBoxButton.OK, MessageBoxImage.Error); return; }
            DatabaseController.DatabaseController.RemoveTransaction(UserContextID, transID)
                              .OnSuccess(() => LoadTransactionsTable())
                              .OnSuccess(() => UpdateTotal())
                              .OnSuccess(() => MessageBox.Show(SUCCESSFULLY_DELETED, SUCCESSFULLY_DELETED, MessageBoxButton.OK, MessageBoxImage.Information))
                              .OnFailure(error => MessageBox.Show(error, ERROR, MessageBoxButton.OK, MessageBoxImage.Error));
            
        }
    }
}
