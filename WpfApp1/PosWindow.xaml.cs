using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace ProductManager
{
    public partial class PosWindow : Window
    {
        // constructor receives existing data from your main app
        private readonly List<Product> _products;
        private readonly List<Unit> _units;
        private readonly List<Sale> _sales; // existing sales list (will be added to)

        private ObservableCollection<CartLine> Cart { get; set; } = new();

        // small helper used for UnitsGrid display
        private class UnitDisplay
        {
            public int ProductId { get; set; }
            public int UnitId { get; set; }
            public string UnitName { get; set; } = "";
            public decimal UnitPrice { get; set; }
        }

        // internal cart line
        private class CartLine
        {
            public int ProductId { get; set; }
            public int UnitId { get; set; }
            public string ProductName { get; set; } = "";
            public string UnitName { get; set; } = "";
            public decimal UnitPrice { get; set; }
            public decimal Quantity { get; set; } = 1;
            public decimal Total => UnitPrice * Quantity;
        }

        public PosWindow(List<Product> products, List<Unit> units, List<Sale> sales)
        {
            InitializeComponent();
            _products = products;
            _units = units;
            _sales = sales;

            ProductsGrid.ItemsSource = _products;
            CartGrid.ItemsSource = Cart;

            ProductsGrid.SelectionChanged += ProductsGrid_SelectionChanged;

            RefreshTotal();
        }

        private void ProductsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // display units for selected product
            if (ProductsGrid.SelectedItem is Product prod)
            {
                // map ProductUnit -> UnitDisplay
                var unitDisplays = prod.ProductUnits
                    .Select(pu =>
                    {
                        var unit = _units.FirstOrDefault(u => u.Id == pu.UnitId);
                        return new UnitDisplay
                        {
                            ProductId = prod.Id,
                            UnitId = pu.UnitId,
                            UnitName = unit?.Name ?? $"Unit {pu.UnitId}",
                            UnitPrice = pu.Price
                        };
                    })
                    .ToList();

                UnitsGrid.ItemsSource = unitDisplays;
            }
            else
            {
                UnitsGrid.ItemsSource = null;
            }
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement fe && fe.DataContext != null)) return;
            var ctx = fe.DataContext;
            var ud = ctx as UnitDisplay;
            if (ud == null) return;

            var prod = _products.FirstOrDefault(p => p.Id == ud.ProductId);
            if (prod == null) return;

            // if same product+unit+price exists, increment qty
            var existing = Cart.FirstOrDefault(c => c.ProductId == ud.ProductId && c.UnitId == ud.UnitId && c.UnitPrice == ud.UnitPrice);
            if (existing != null)
            {
                existing.Quantity += 1;
                CartGrid.Items.Refresh();
            }
            else
            {
                Cart.Add(new CartLine
                {
                    ProductId = ud.ProductId,
                    UnitId = ud.UnitId,
                    ProductName = prod.Name,
                    UnitName = ud.UnitName,
                    UnitPrice = ud.UnitPrice,
                    Quantity = 1
                });
            }

            RefreshTotal();
        }

        private void IncreaseQty_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is CartLine line)
            {
                line.Quantity += 1;
                CartGrid.Items.Refresh();
                RefreshTotal();
            }
        }

        private void DecreaseQty_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is CartLine line)
            {
                line.Quantity = Math.Max(0, line.Quantity - 1);
                if (line.Quantity == 0)
                {
                    Cart.Remove(line);
                }
                CartGrid.Items.Refresh();
                RefreshTotal();
            }
        }

        private void RemoveLine_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is CartLine line)
            {
                Cart.Remove(line);
                RefreshTotal();
            }
        }

        private void ClearCart_Click(object sender, RoutedEventArgs e)
        {
            Cart.Clear();
            RefreshTotal();
        }

        private void RefreshTotal()
        {
            decimal total = Cart.Sum(c => c.Total);
            TotalText.Text = $"Total: {total:C}";
            CartGrid.Items.Refresh();
        }

        private void CompleteSale_Click(object sender, RoutedEventArgs e)
        {
            if (!Cart.Any())
            {
                MessageBox.Show("Cart is empty.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // create Sale
            var sale = new Sale
            {
                Id = (_sales.Any() ? _sales.Max(s => s.Id) + 1 : 1),
                EntryDate = DateTime.Now,
                TotalPrice = Cart.Sum(c => c.Total),
                TotalQuantity = Cart.Sum(c => c.Quantity),
                Items = Cart.Select(c => new SaleItem
                {
                    ProductId = c.ProductId,
                    UnitId = c.UnitId,
                    ProductName = c.ProductName,
                    UnitName = c.UnitName,
                    UnitPrice = c.UnitPrice,
                    Quantity = c.Quantity
                }).ToList()
            };

            _sales.Add(sale);

            // persist sales to Sales.json in app folder
            try
            {
                var path = Path.Combine(AppContext.BaseDirectory, "Sales.json");
                // read existing if exists
                List<Sale> toSave = new();
                if (File.Exists(path))
                {
                    var existingJson = File.ReadAllText(path);
                    toSave = JsonSerializer.Deserialize<List<Sale>>(existingJson) ?? new List<Sale>();
                }
                toSave.Add(sale);
                var opt = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(path, JsonSerializer.Serialize(toSave, opt));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save sale: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            MessageBox.Show($"Sale completed. Total: {sale.TotalPrice:C}", "Sale", MessageBoxButton.OK, MessageBoxImage.Information);

            // clear cart and close or keep open depending on your flow
            Cart.Clear();
            RefreshTotal();

            // indicate success and close
            DialogResult = true;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
