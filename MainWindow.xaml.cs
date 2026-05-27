using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfCrud
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseService _db = new DatabaseService();
        private ObservableCollection<Contact> _contacts = new();

        public MainWindow()
        {
            InitializeComponent();
            LoadContacts();
        }

        // Load / Search

        private void LoadContacts(string search = "")
        {
            var all = _db.GetAll();
            var filtered = string.IsNullOrWhiteSpace(search)
                ? all
                : all.Where(c =>
                    c.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.Email.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            _contacts = new ObservableCollection<Contact>(filtered);
            ContactGrid.ItemsSource = _contacts;
            CountText.Text = $"{filtered.Count} record{(filtered.Count != 1 ? "s" : "")}";
            SetStatus($"Loaded {filtered.Count} contact(s).");
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
            => LoadContacts(SearchBox.Text);

        // Save (Insert or Update)

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var name  = NameBox.Text.Trim();
            var email = EmailBox.Text.Trim();
            var phone = PhoneBox.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                SetStatus("⚠ Name is required.", isError: true);
                NameBox.Focus();
                return;
            }

            bool isEdit = int.TryParse(IdBox.Text, out int id) && id > 0;

            if (isEdit)
            {
                _db.Update(new Contact { Id = id, Name = name, Email = email, Phone = phone });
                SetStatus($"✔ Updated contact #{id}.");
            }
            else
            {
                _db.Insert(new Contact { Name = name, Email = email, Phone = phone });
                SetStatus("✔ Contact added.");
            }

            ClearForm();
            LoadContacts(SearchBox.Text);
        }

        // Edit

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ContactGrid.SelectedItem is not Contact c) return;
            IdBox.Text    = c.Id.ToString();
            NameBox.Text  = c.Name;
            EmailBox.Text = c.Email;
            PhoneBox.Text = c.Phone;
            SaveBtn.Content = "💾 Update";
            NameBox.Focus();
            SetStatus($"Editing contact #{c.Id}.");
        }

        // Delete

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ContactGrid.SelectedItem is not Contact c) return;

            var result = MessageBox.Show(
                $"Delete \"{c.Name}\"?", "Confirm Delete",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            _db.Delete(c.Id);
            SetStatus($"🗑 Deleted contact #{c.Id}.");
            ClearForm();
            LoadContacts(SearchBox.Text);
        }

        // Selection

        private void ContactGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = ContactGrid.SelectedItem != null;
            EditBtn.IsEnabled   = hasSelection;
            DeleteBtn.IsEnabled = hasSelection;
        }


        private void ClearBtn_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void ClearForm()
        {
            IdBox.Text = NameBox.Text = EmailBox.Text = PhoneBox.Text = "";
            SaveBtn.Content = "💾 Save";
            ContactGrid.SelectedItem = null;
            SetStatus("Ready.");
        }

        private void SetStatus(string msg, bool isError = false)
        {
            StatusText.Text = msg;
            StatusText.Foreground = isError
                ? System.Windows.Media.Brushes.Salmon
                : new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x60, 0x60, 0xA0));
        }
    }
}
