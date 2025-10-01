using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace ProductManager
{
    public partial class MainWindow : Window
    {
        private List<Product> allProducts;
        private ObservableCollection<Product> filteredProducts;
        private List<Unit> allUnits;
        private List<Sale> allSales;


        public MainWindow()
        {

            InitializeComponent();
            InitializeData();
        }

        private void InitializeData()
        {
            LoadProducts(); // ✅ Load saved products here

            filteredProducts = new ObservableCollection<Product>(allProducts);
            ProductsGrid.ItemsSource = filteredProducts;

        }


        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }



        private void SaveProducts()
        {
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                var json = System.Text.Json.JsonSerializer.Serialize(allProducts, options);
                System.IO.File.WriteAllText("Products.json", json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save products: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilters()
        {
            var searchText = SearchBox?.Text?.ToLower() ?? "";

            var filtered = allProducts.Where(p =>
                (string.IsNullOrEmpty(searchText) ||
                 p.Name.ToLower().Contains(searchText) ||
                 p.Description.ToLower().Contains(searchText))
            ).ToList();

            filteredProducts.Clear();
            foreach (var product in filtered)
            {
                filteredProducts.Add(product);
            }
        }

        private void ClearFilter_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ProductDialog();
            if (dialog.ShowDialog() == true)
            {
                var newProduct = dialog.Product;
                newProduct.Id = allProducts.Any() ? allProducts.Max(p => p.Id) + 1 : 1;

                allProducts.Add(newProduct);
                SaveProducts();
                ApplyFilters();
                MessageBox.Show("Product added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LoadProducts()
        {
            if (System.IO.File.Exists("Products.json"))
            {
                try
                {
                    var json = System.IO.File.ReadAllText("Products.json");
                    allProducts = System.Text.Json.JsonSerializer.Deserialize<List<Product>>(json) ?? new List<Product>();
                }
                catch
                {
                    allProducts = new List<Product>();
                }
            }
            else
            {
                allProducts = new List<Product>();
            }
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem is Product selectedProduct)
            {
                var dialog = new ProductDialog(selectedProduct);
                if (dialog.ShowDialog() == true)
                {
                    var index = allProducts.IndexOf(selectedProduct);
                    allProducts[index] = dialog.Product;

                    ApplyFilters();
                    MessageBox.Show("Product updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select a product to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem is Product selectedProduct)
            {
                var result = MessageBox.Show($"Are you sure you want to delete '{selectedProduct.Name}'?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    allProducts.Remove(selectedProduct);
                    ApplyFilters();
                    MessageBox.Show("Product deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select a product to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void OpenPOS_Click(object sender, RoutedEventArgs e)
        {
            // Assume you have lists: allProducts (List<Product>), allUnits (List<Unit>), allSales (List<Sale>)
            var pos = new PosWindow(allProducts, allUnits, allSales);
            if (pos.ShowDialog() == true)
            {
                // sales were possibly updated; optional: persist sales somewhere central if needed
            }
        }
        private void ManageUnits_Click(object sender, RoutedEventArgs e)
        {
            // Suppose you have a List<Unit> allUnits field in the window
            var unitsWindow = new UnitListWindow(allUnits);
            unitsWindow.Owner = this;
            if (unitsWindow.ShowDialog() == true)
            {
                // unitsWindow.Units has the updated collection (ObservableCollection<Unit>)
                // If you need to refresh any UI that depends on units, do it here:
                // e.g. Refresh units shown for selected product or persist via DataService
                // Example: _units = unitsWindow.Units.ToList();
            }
        }

        private void ProductsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CopyExeToDesktop()
        {
            try
            {
                // Get current folder (bin\Debug\net8.0\ or similar)
                string publishFolder = Path.Combine(AppContext.BaseDirectory, "Publish"); // or absolute path
                string exePath = Directory.GetFiles(publishFolder, "WpfApp1.exe").First();
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                File.Copy(exePath, Path.Combine(desktopPath, Path.GetFileName(exePath)), true);

                string destination = Path.Combine(desktopPath, Path.GetFileName(exePath));

                // Copy it
                File.Copy(exePath, destination, true);
                MessageBox.Show("✅ Executable copied to Desktop!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Copy failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private void CopyToDesktop_Click(object sender, RoutedEventArgs e)
        {
            CopyExeToDesktop();
        }

    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<ProductUnit> ProductUnits { get; set; } = new(); // ✅ Make it public
    }

    public class Unit
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public List<ProductUnit> ProductUnits { get; set; } = new();
    }

    public class ProductUnit
    {
        public int ProductId { get; set; }
        public int UnitId { get; set; }
        public decimal Price { get; set; }
    }

    public class Sale
    {
        public int Id { get; set; }
        public DateTime EntryDate { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalQuantity { get; set; }
        public List<SaleItem> Items { get; set; } = new();
        public List<ProductUnit> productUnits { get; set; } = new();
    }
    // SaleItem.cs (new)
    public class SaleItem
    {
        public int ProductId { get; set; }
        public int UnitId { get; set; }
        public string ProductName { get; set; } = "";
        public string UnitName { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal Total => UnitPrice * Quantity;
    }

}