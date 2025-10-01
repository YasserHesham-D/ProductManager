using System.Windows;
using static ProductManager.MainWindow;

namespace ProductManager
{
    public partial class ProductDialog : Window
    {
        public Product Product { get; private set; }

        public ProductDialog(Product? existingProduct = null)
        {
            InitializeComponent();

            if (existingProduct != null)
            {
                // Edit mode - populate fields with existing product data
                Title = "Edit Product";
                Product = new Product
                {
                    Id = existingProduct.Id,
                    Name = existingProduct.Name,
                    Description = existingProduct.Description
                };

                NameTextBox.Text = Product.Name;
                DescriptionTextBox.Text = Product.Description;
            }
            else
            {
                // Add mode - create new empty product
                Title = "Add New Product";
                Product = new Product();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Validate product name
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Please enter a product name.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }



            // Validate price
            if (!decimal.TryParse(PriceTextBox.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Please enter a valid price (must be a positive number).",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                PriceTextBox.Focus();
                return;
            }



            // Save product data
            Product.Name = NameTextBox.Text.Trim();
            Product.Description = DescriptionTextBox.Text.Trim();

            // Close dialog with success
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // Close dialog without saving
            DialogResult = false;
            Close();
        }
    }
}