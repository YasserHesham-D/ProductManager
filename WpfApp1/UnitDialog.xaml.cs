using System;
using System.Globalization;
using System.Windows;

namespace ProductManager
{
    public partial class UnitDialog : Window
    {
        public string UnitName { get; private set; } = "";
        public decimal ConversionRate { get; private set; } = 1m;

        public UnitDialog(string initialName = "", decimal initialConversion = 1m)
        {
            InitializeComponent();
            NameBox.Text = initialName ?? "";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var name = NameBox.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Please enter unit name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                NameBox.Focus();
                return;
            }


            UnitName = name;

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
