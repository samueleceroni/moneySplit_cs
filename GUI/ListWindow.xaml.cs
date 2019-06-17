﻿using System;
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
        private static readonly string AMOUNT_MUST_BE_NUMBER = "Amount for new transaction must be a number.";
        private static readonly string REPETITION_MUST_BE_INTEGER = "Repetition number must be an integer.";
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
            LoadTransactionsTable();
        }

        private void LoadTransactionsTable()
        {
            int? versionNull = null;
            if (Int32.TryParse(versionComboBox.SelectedValue.ToString(), out int version)) { versionNull = version; };
            int? transactionType = null;
            if (transactionTypeComboBox.SelectedItem == OnlyRecurring) { transactionType = 1; }
            if (transactionTypeComboBox.SelectedItem == OnlyNormal) { transactionType = 0; }
            var transactionsResult = DatabaseController.DatabaseController.GetAllTransaction(UserContextID, ListID, versionNull, transactionType);
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

            var amountRes = Result.Create(Double.TryParse(newTransactionAmountTextBox.Text, out double newTransactionAmount), newTransactionAmount, AMOUNT_MUST_BE_NUMBER);
            if (amountRes.IsFailure) { MessageBox.Show(amountRes.Error, ERROR, MessageBoxButton.OK, MessageBoxImage.Error); return; }


            Result resAddTrans;
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
                                                .AddNewTransaction(UserContextID, listIdLastVersion.Value, amountRes.Value, newTransactionDescriptionTextBox.Text, 1, dayRecurrence, monthRecurrence, null, newTransactionRecurrenceStartDatePicker.SelectedDate, newTransactionRecurrenceEndDatePicker.SelectedDate, UserContextID);
            } else
            {
                resAddTrans = DatabaseController.DatabaseController
                                                .AddNewTransaction(UserContextID, listIdLastVersion.Value, amountRes.Value, newTransactionDescriptionTextBox.Text);
            }
            if (resAddTrans.IsFailure) { MessageBox.Show(resAddTrans.Error, ERROR, MessageBoxButton.OK, MessageBoxImage.Error); return; }
            DatabaseController.DatabaseController
                              .GetListFullDetail(UserContextID, ListID)
                              .OnSuccess(list => totalValueLabel.Content = list.TotalAmount + " €")
                              .OnFailure(error => { MessageBox.Show(error, ERROR, MessageBoxButton.OK, MessageBoxImage.Error); this.Close(); return; });
            LoadTransactionsTable();
        }
    }
}
