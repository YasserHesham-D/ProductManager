using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;

namespace ProductManager
{
    public partial class UnitListWindow : Window
    {
        // Public property to read back modified units
        public ObservableCollection<Unit> Units { get; private set; } = new();

        // internal original reference (optional)
        private readonly IList<Unit> _originalListRef;
        private readonly string _unitsFilePath;

        public UnitListWindow(IList<Unit> existingUnits)
        {
            InitializeComponent();

            _originalListRef = existingUnits ?? new List<Unit>();
            Units = new ObservableCollection<Unit>(_originalListRef.Select(u => new Unit
            {
                Id = u.Id,
                Name = u.Name,

                ProductUnits = u.ProductUnits ?? new List<ProductUnit>()
            }));

            UnitsGrid.ItemsSource = Units;

            _unitsFilePath = Path.Combine(AppContext.BaseDirectory, "Units.json");
        }

        private void AddUnit_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new UnitDialog();
            dlg.Owner = this;
            if (dlg.ShowDialog() == true)
            {
                // assign Id
                var nextId = Units.Any() ? Units.Max(u => u.Id) + 1 : 1;
                var unit = new Unit
                {
                    Id = nextId,
                    Name = dlg.UnitName,

                    ProductUnits = new List<ProductUnit>()
                };
                Units.Add(unit);
            }
        }

        private void EditUnit_Click(object sender, RoutedEventArgs e)
        {
            if (UnitsGrid.SelectedItem is Unit sel)
            {
                var dlg = new UnitDialog(sel.Name);
                dlg.Owner = this;
                if (dlg.ShowDialog() == true)
                {
                    sel.Name = dlg.UnitName;

                    UnitsGrid.Items.Refresh();
                }
            }
            else MessageBox.Show("Please select a unit to edit.", "No selection", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteUnit_Click(object sender, RoutedEventArgs e)
        {
            if (UnitsGrid.SelectedItem is Unit sel)
            {
                var res = MessageBox.Show($"Delete unit '{sel.Name}'? This cannot be undone.", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (res == MessageBoxResult.Yes)
                {
                    Units.Remove(sel);

                    // Additionally: remove references from products (if you want)
                    // For safer cleanup, we do NOT automatically remove ProductUnit links here;
                    // leave that to the caller who knows the product data model.
                }
            }
            else MessageBox.Show("Please select a unit to delete.", "No selection", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveClose_Click(object sender, RoutedEventArgs e)
        {
            // Save to Units.json
            try
            {
                var toSave = Units.Select(u => new Unit
                {
                    Id = u.Id,
                    Name = u.Name,

                    ProductUnits = u.ProductUnits ?? new List<ProductUnit>()
                }).ToList();

                var opts = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(_unitsFilePath, JsonSerializer.Serialize(toSave, opts));

            }
            catch { throw new Exception("ON saviing exc"); }
            }

        private void FilterBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
