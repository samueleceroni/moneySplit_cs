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
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {

        private static readonly string ID_MUST_BE_INTEGER = "The Context ID must be an integer.";
        private static readonly string TELEGRAM_ID_TOO_LONG = "The Telegram ID must 16 char max.";
        private static readonly string ALERT = "Alert";
        private static readonly string ERROR = "Error";

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void AdminViewButton_Click(object sender, RoutedEventArgs e)
        {
            new AdminWindow { Owner = this }.ShowDialog();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string IDString = loginIDTextBox.Text;
            if (typeIDComboBox.SelectedItem == contextComboItem)
            {
                if (Result.Create(Int32.TryParse(IDString, out int contextID), ID_MUST_BE_INTEGER)
                          .OnFailure(errorValue => MessageBox.Show(errorValue, ERROR, MessageBoxButton.OK, MessageBoxImage.Error))
                          .IsFailure) { return; }
                DatabaseController.DatabaseController.CheckRealContextID(contextID)
                                  .Map(() => contextID)
                                  .OnFailure(errorValue => MessageBox.Show(errorValue, ERROR, MessageBoxButton.OK, MessageBoxImage.Error))
                                  .OnSuccess(value => new ContextWindow(value) { Owner = this }.ShowDialog());
            } else
            {
                DatabaseController.DatabaseController.TelegramToContextID(IDString)
                                  .OnFailure(errorValue => MessageBox.Show(errorValue, ERROR, MessageBoxButton.OK, MessageBoxImage.Error))
                                  .OnSuccess(value => new ContextWindow(value) { Owner = this }.ShowDialog());
            }
        }

        private void RegisterUserButton_Click(object sender, RoutedEventArgs e)
        {
            var newUserTelegramId = registerUserTelegramIdTextBox.Text.Replace(" ", String.Empty);
            if (newUserTelegramId.Length > 16)
            {
                MessageBox.Show(TELEGRAM_ID_TOO_LONG, ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!(bool)isTelegramUserCheckBox.IsChecked)
            {
                newUserTelegramId = null;
            }
            DatabaseController.DatabaseController.RegisterNewUser(registerUserNameTextBox.Text, newUserTelegramId)
                              .OnFailure(errorValue => MessageBox.Show(errorValue, ERROR, MessageBoxButton.OK, MessageBoxImage.Error))
                              .OnSuccess(value => new ContextWindow(value) { Owner = this }.ShowDialog());
        }

        private void RegisterGroupButton_Click(object sender, RoutedEventArgs e)
        {
            var newGroupTelegramId = registerGroupTelegramIdTextBox.Text.Replace(" ", String.Empty);
            DatabaseController.DatabaseController.RegisterNewGroup(registerGroupNameTextBox.Text, newGroupTelegramId)
                              .OnFailure(errorValue => MessageBox.Show(errorValue, ERROR, MessageBoxButton.OK, MessageBoxImage.Error))
                              .OnSuccess(value => new ContextWindow(value) { Owner = this }.ShowDialog());
        }
    }
}
