using System.Windows;
using System.Windows.Controls;


namespace NautilusXP2024
{
    public partial class CustomMessageBoxInserttoSQL : Window
    {
        public enum CustomMessageBoxResult
        {
            Replace,
            Cancel
        }

        public CustomMessageBoxResult Result { get; private set; }

        public CustomMessageBoxInserttoSQL(string message, string button1Text, string button2Text)
        {
            InitializeComponent();
            MessageTextBlock.Text = message;
            AddButton(button1Text, CustomMessageBoxResult.Replace);
            AddButton(button2Text, CustomMessageBoxResult.Cancel);
        }

        private void AddButton(string content, CustomMessageBoxResult result)
        {
            var button = new Button
            {
                Content = content,
                Style = (Style)FindResource("DarkModeButtonStyle"),
                Width = 100,
                Margin = new Thickness(5)
            };
            button.Click += (s, e) =>
            {
                Result = result;
                DialogResult = true;
            };
            ButtonPanel.Children.Add(button);
        }

        public static CustomMessageBoxResult Show(string message, string button1Text, string button2Text)
        {
            var messageBox = new CustomMessageBoxInserttoSQL(message, button1Text, button2Text);
            messageBox.ShowDialog();
            return messageBox.Result;
        }
    }
}
