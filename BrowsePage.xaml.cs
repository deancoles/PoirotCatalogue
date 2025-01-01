using PoirotCollectionApp.DataAccess; // Handles database interactions
using PoirotCollectionApp.Models; // Defines models like PoirotCollection and User
using System; // General system utilities
using System.ComponentModel; // For property change notifications
using System.Runtime.CompilerServices; // For CallerMemberName attribute
using Microsoft.Maui.Controls; // UI elements and navigation
using System.Linq; // For LINQ queries
using System.Collections.ObjectModel; // For ObservableCollection

namespace PoirotCollectionApp
{
    public partial class BrowsePage : ContentPage, INotifyPropertyChanged
    {

        private string _userName = string.Empty; // Stores the current user's name
        private int _userId; // Stores the current user's ID
        private string _currentFilter = "All"; // Tracks the active filter: Owned, Wishlist, or All
        private bool _isAscending = true; // Indicates sorting order (true for A-Z, false for Z-A)
        private string _currentSort = "Year"; // Tracks the current sorting state (Year, A-Z, Z-A)


        public string UserName
        {
            get => _userName;
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CurrentFilter
        {
            get => _currentFilter;
            set
            {
                if (_currentFilter != value)
                {
                    _currentFilter = value;
                    OnPropertyChanged();
                    UpdateButtonStyles(); // Update button visuals when filter changes
                }
            }
        }

        // Notifies the UI of any property changes
        public new event PropertyChangedEventHandler? PropertyChanged;

        // Constructor - Initialize the page with the user's data
        public BrowsePage(int userId, string userName)
        {
            InitializeComponent();

            // Set the current users information
            _userId = userId;
            UserName = userName;

            CurrentFilter = "Owned"; // Default filter
            BindingContext = this; // Bind UI to this class

            SortButton.Text = "Year"; // Default sorting 
            ApplySearchAndSort(); // Load and sort the book list
        }

        // Filter Handlers - Changes the active filter and refreshes the book list
        private void FilterOwned(object sender, EventArgs e)
        {
            CurrentFilter = "Owned";
            ApplySearchAndSort();
        }

        private void FilterWishlist(object sender, EventArgs e)
        {
            CurrentFilter = "Wishlist";
            ApplySearchAndSort();
        }

        private void FilterAll(object sender, EventArgs e)
        {
            CurrentFilter = "All";
            ApplySearchAndSort();
        }

        // Sorting Handler - Cycle through sorting options (Year, A-Z, Z-A)
        private void OnSortButtonClicked(object sender, EventArgs e)
        {
            switch (_currentSort)
            {
                case "Year":
                    _currentSort = "A-Z";
                    _isAscending = true;
                    break;
                case "A-Z":
                    _currentSort = "Z-A";
                    _isAscending = false;
                    break;
                case "Z-A":
                    _currentSort = "Year";
                    break;
            }

            SortButton.Text = _currentSort; // Update sort button text
            SortIndicatorLabel.Text = $"Currently sorted by: {_currentSort}"; // Update sort label
            ApplySearchAndSort();
        }

        // Navigation Handlers - Navigate back or change the active user
        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync(); // Return to the previous page
        }

        private async void OnChangeUserClicked(object sender, EventArgs e)
        {
            try
            {
                var dbHelper = new DatabaseHelper();
                var users = await dbHelper.GetUsersAsync();
                var userNames = users.Select(u => u.Username).ToArray();

                string selectedUser = await DisplayActionSheet("Select User", "Cancel", null, userNames);

                if (selectedUser != "Cancel")
                {
                    string previousSort = _currentSort; // Preserve sorting preference
                    var selected = users.First(u => u.Username == selectedUser);
                    _userId = selected.UserID;
                    UserName = selected.Username;

                    CurrentFilter = "All"; // Reset filter
                    _currentSort = previousSort; // Restore sorting
                    ApplySearchAndSort();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load users: {ex.Message}", "OK");
            }
        }

        // Search Handler - Filter the book list based on search input
        private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearchAndSort();
        }

        // Main Method - Apply filtering, sorting, and searching to the book list
        private async void ApplySearchAndSort()
        {
            try
            {
                string searchText = SearchBar.Text?.Trim().ToLower() ?? string.Empty;
                var dbHelper = new DatabaseHelper();
                var books = await dbHelper.GetPoirotCollectionAsync(_userId, CurrentFilter);

                var filteredBooks = string.IsNullOrEmpty(searchText)
                    ? books
                    : books.Where(book => book.Title.ToLower().Contains(searchText)).ToList();

                List<PoirotCollection> sortedBooks = filteredBooks;

                switch (_currentSort)
                {
                    case "A-Z":
                        sortedBooks = filteredBooks.OrderBy(p => p.Title).ToList();
                        break;
                    case "Z-A":
                        sortedBooks = filteredBooks.OrderByDescending(p => p.Title).ToList();
                        break;
                    case "Year":
                        sortedBooks = filteredBooks.OrderBy(p => p.ReleaseDate).ToList();
                        break;
                }

                BooksListView.ItemsSource = new ObservableCollection<PoirotCollection>(sortedBooks);
                SortIndicatorLabel.Text = $"Currently sorted by: {_currentSort}";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        // Book Selection - Navigate to the details page when a book is tapped
        private async void OnBookTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            var selectedBook = (PoirotCollection)e.Item;
            var booksList = ((ObservableCollection<PoirotCollection>)BooksListView.ItemsSource).ToList();
            var selectedIndex = booksList.IndexOf(selectedBook);

            await Navigation.PushAsync(new TitleDetailsPage(booksList, selectedIndex));
            ((ListView)sender).SelectedItem = null; // Clear the selection
        }

        // Helper Method - Update button styles based on the active filter
        private void UpdateButtonStyles()
        {
            OwnedButton.BackgroundColor = Colors.LightGray;
            WishlistButton.BackgroundColor = Colors.LightGray;
            AllButton.BackgroundColor = Colors.LightGray;

            switch (CurrentFilter)
            {
                case "Owned":
                    OwnedButton.BackgroundColor = Colors.Purple;
                    break;
                case "Wishlist":
                    WishlistButton.BackgroundColor = Colors.Purple;
                    break;
                case "All":
                    AllButton.BackgroundColor = Colors.Purple;
                    break;
            }
        }

        // Notify the UI of property changes
        protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            base.OnPropertyChanged(propertyName);
        }
    }
}
