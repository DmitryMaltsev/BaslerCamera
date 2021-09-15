using System;
using System.Windows;

namespace Kogerent.LaserScan.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private byte counter = 5;
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void logInButton_Click(object sender, RoutedEventArgs e)
        {
            if (loginText.Text == "Введите свой логин..." && loginPassword.Password == "Kogerent2012")
                DialogResult = true;
            else
            {
                MessageBox.Show($"Неправильная пара логин-пароль{Environment.NewLine}Осталось попыток: {--counter}",
                                "Ошибка!!!", MessageBoxButton.OK, MessageBoxImage.Error);
                if (counter == 0)
                    DialogResult = false;
            }
        }

        private void logFailedButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void TextBlock_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
