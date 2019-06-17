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
    /// Interaction logic for ContextWindow.xaml
    /// </summary>
    public partial class ContextWindow : Window
    {
        private static readonly string ERROR = "Error";
        private static readonly string CAN_NOT_LOAD_LISTS = "Can't load lists: ";
        
        private int ContextID { get; set; }
        private string ContextName { get; set; }
        private string TelegramID { get; set; }

        public ContextWindow(int contextId)
        {
            var resultUser = DatabaseController.DatabaseController.GetCompleteUser(contextId);
            var resultGroup = DatabaseController.DatabaseController.GetCompleteGroup(contextId);
            if (resultUser.IsFailure && resultGroup.IsFailure)
            {
                MessageBox.Show(resultUser.Error, ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            resultUser.OnSuccess( value =>
                                    {
                                        ContextID = (int) value.ContextId;
                                        ContextName = value.Name;
                                        TelegramID = value.TelegramId ?? string.Empty;
                                    });

            resultGroup.OnSuccess(value =>
                                    {
                                        ContextID = (int) value.ContextId;
                                        ContextName = value.Name;
                                        TelegramID = value.TelegramId ?? string.Empty;
                                    });

            InitializeComponent();
            Loaded += OnLoad;
            Closed += new EventHandler(OnClose);

            contextIDValueLabel.Content = ContextID;
            contextNameValueLabel.Content = ContextName;
            contextTelegramIDValueLabel.Content = TelegramID;
            LoadListsTable();

            if (DatabaseController.DatabaseController.IsGroup(ContextID))
            {
                LoadGroupUserTable();
            }
            else
            {
                addUserToGroup.Visibility = Visibility.Hidden;
                addUserToGroupTelegramIdLabel.Visibility = Visibility.Hidden;
                addUserToGroupTelegramIdTextBox.Visibility = Visibility.Hidden;
                groupUsersDataGrid.Visibility = Visibility.Hidden;
                groupPartecipantLabel.Visibility = Visibility.Hidden;
                addUserToGroupIsAdminCheckBox.Visibility = Visibility.Hidden;
            }


        }

        private void LoadGroupUserTable()
        {
            groupUsersDataGrid.Items.Clear();
            var users = DatabaseController.DatabaseController.GetAllMembersOfGroup(TelegramID);
            foreach (var user in users)
            {
                groupUsersDataGrid.Items.Add(user);
            }
        }

        private void LoadListsTable()
        {
            listsDataGrid.Items.Clear();
            bool includeGeneralList = showNormalListCheckBox.IsChecked ?? true;
            bool includeBankAccount = showBankAccountCheckBox.IsChecked ?? true;
            var listsResult = DatabaseController.DatabaseController.GetAllListsDouble(ContextID, includeGeneralList, includeBankAccount);

            if (listsResult.IsFailure) { MessageBox.Show(CAN_NOT_LOAD_LISTS + listsResult.Error, ERROR, MessageBoxButton.OK, MessageBoxImage.Error); return; }
            var lists = listsResult.Value;
            foreach (var list in lists)
            {
                listsDataGrid.Items.Add(list);
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

        private void NewStoreButton_Click(object sender, RoutedEventArgs e)
        {
            new NewStoreModal { Owner = this }.ShowDialog();
        }
        
        private void AddUserToGroup_Click(object sender, RoutedEventArgs e)
        {
            DatabaseController.DatabaseController.GroupAddUser(TelegramID, addUserToGroupTelegramIdTextBox.Text.Trim(), addUserToGroupIsAdminCheckBox.IsChecked ?? false)
                              .OnFailure(errorValue => MessageBox.Show(errorValue, ERROR, MessageBoxButton.OK, MessageBoxImage.Error))
                              .OnSuccess(() => LoadGroupUserTable());
        }

        private void NewListButton_Click(object sender, RoutedEventArgs e)
        {
            string IBAN = null;
            string ownerCF = null;
            if (newListIsBankAccountCheckBox.IsChecked ?? false)
            {
                IBAN = newListIbanTextBox.Text.Trim();
                ownerCF = CFOwnerTextBox.Text.Trim();
            }

            DatabaseController.DatabaseController.AddNewList(ContextID, newListNameTextBox.Text, newListIsBankAccountCheckBox.IsChecked ?? false, IBAN, ownerCF)
                  .OnFailure(errorValue => MessageBox.Show(errorValue, ERROR, MessageBoxButton.OK, MessageBoxImage.Error))
                  .OnSuccess(() => LoadListsTable());
        }

        private void ShowBankAccountCheckBox_CheckChange(object sender, RoutedEventArgs e)
        {
            if (showBankAccountCheckBox != null && showNormalListCheckBox != null)
            {
                LoadListsTable();
            }
        }

        private void ListsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listsDataGrid.SelectedItem == null) return;
            int listID = ((GeneralListDouble) listsDataGrid.SelectedItem).ListId;

            new ListWindow(ContextID, listID) { Owner = this }.ShowDialog();
        }
    }
}
