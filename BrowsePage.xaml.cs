﻿using PoirotCollectionApp.DataAccess; // For DatabaseHelper
using PoirotCollectionApp.Models; // For Book and User models
using System; // For general system utilities
using System.ComponentModel; // For INotifyPropertyChanged
using System.Runtime.CompilerServices; // For CallerMemberName attribute
using Microsoft.Maui.Controls; // For UI elements
using System.Linq; // For LINQ queries
using System.Collections.ObjectModel; // For ObservableCollection

namespace PoirotCollectionApp
{
    public partial class BrowsePage : ContentPage, INotifyPropertyChanged
    {
        private string _userName = string.Empty; // Stores the active user's name
        private int _userId; // Stores the active user's ID
        private string _currentFilter = "All"; // Tracks the current filter: Owned, Wishlist, or All
        private bool _isAscending = true; // Tracks whether sorting is A-Z (true) or Z-A (false)
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
                    UpdateButtonStyles(); // Update button styles when the filter changes
                }
            }
        }

        public new event PropertyChangedEventHandler? PropertyChanged;

        // Constructor that takes in the UserID and UserName
        public BrowsePage(int userId, string userName)
        {
            InitializeComponent();

            // Set the active user's information
            _userId = userId;
            UserName = userName;

            CurrentFilter = "Owned"; // Default filter
            BindingContext = this; // Bind data to the UI

            SortButton.Text = "Year"; // Set initial button text to indicate database order
            ApplySearchAndSort(); // Load books and apply sorting
        }

        // Filter the book list to show "Owned" books
        private void FilterOwned(object sender, EventArgs e)
        {
            CurrentFilter = "Owned"; // Set the filter
            ApplySearchAndSort(); // Apply search and sorting with the updated filter
        }

        // Filter the book list to show "Wishlist" books
        private void FilterWishlist(object sender, EventArgs e)
        {
            CurrentFilter = "Wishlist"; // Set the filter
            ApplySearchAndSort(); // Apply search and sorting with the updated filter
        }

        // Filter the book list to show "All" books
        private void FilterAll(object sender, EventArgs e)
        {
            CurrentFilter = "All"; // Set the filter
            ApplySearchAndSort(); // Apply search and sorting with the updated filter
        }

        private async void ApplySearchAndSort()
        {
            try
            {
                // Capture the search text
                string searchText = SearchBar.Text?.Trim().ToLower() ?? string.Empty;

                var dbHelper = new DatabaseHelper();

                // Fetch books based on the current filter (Owned, Wishlist, All)
                var books = await dbHelper.GetPoirotCollectionAsync(_userId, CurrentFilter);

                // Apply search filtering
                var filteredBooks = string.IsNullOrEmpty(searchText)
                    ? books
                    : books.Where(book => book.Title.ToLower().Contains(searchText)).ToList();

                // Apply sorting based on the current sorting state
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

                // Update the ListView with the sorted and filtered books
                BooksListView.ItemsSource = new ObservableCollection<PoirotCollection>(sortedBooks);

                // Update the sorting indicator label dynamically
                SortIndicatorLabel.Text = $"Currently sorted by: {_currentSort}";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred while applying search and sort: {ex.Message}", "OK");
            }
        }



        // Event triggered when the SearchBar's text changes
        private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
        {
            // Call the method to apply search and sorting
            ApplySearchAndSort();
        }

        // Event handler for the Sort button to toggle sorting states
        private void OnSortButtonClicked(object sender, EventArgs e)
        {
            // Cycle through the sorting states: "Year" → "A-Z" → "Z-A"
            switch (_currentSort)
            {
                case "Year":
                    _currentSort = "A-Z";
                    _isAscending = true; // Start with A-Z sorting
                    break;
                case "A-Z":
                    _currentSort = "Z-A";
                    _isAscending = false; // Switch to Z-A sorting
                    break;
                case "Z-A":
                    _currentSort = "Year";
                    break;
            }

            // Update the button text to reflect the current sort state
            SortButton.Text = _currentSort;

            // Update the indicator label to display the current sorting state
            SortIndicatorLabel.Text = $"Currently sorted by: {_currentSort}";

            // Apply sorting and filtering based on the current state
            ApplySearchAndSort();
        }

        // Event handler for navigating back to the previous page
        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync(); // Go back to the previous page
        }

        // Event triggered when the user clicks "Change User"
        private async void OnChangeUserClicked(object sender, EventArgs e)
        {
            try
            {
                var dbHelper = new DatabaseHelper();
                var users = await dbHelper.GetUsersAsync(); // Fetch the list of users
                var userNames = users.Select(u => u.Username).ToArray(); // Extract usernames

                // Display the list of users to choose from
                string selectedUser = await DisplayActionSheet("Select User", "Cancel", null, userNames);

                if (selectedUser != "Cancel")
                {
                    // Preserve the current sorting state
                    string previousSort = _currentSort;

                    // Find the selected user and update the active user
                    var selected = users.First(u => u.Username == selectedUser);
                    _userId = selected.UserID;
                    UserName = selected.Username;

                    // Reload books for the new user
                    CurrentFilter = "All"; // Reset to show all books initially
                    _currentSort = previousSort; // Restore the previous sorting state
                    ApplySearchAndSort(); // Apply search, filter, and sorting logic
                }
            }
            catch (Exception ex)
            {
                // Handle errors
                await DisplayAlert("Error", $"Failed to load users: {ex.Message}", "OK");
            }
        }


        // Load books based on the selected filter
        private async void LoadBooks(string filter)
        {
            try
            {
                var dbHelper = new DatabaseHelper();
                var books = await dbHelper.GetPoirotCollectionAsync(_userId, filter);

                // Apply the current sort order to the loaded books
                var sortedBooks = books;

                switch (_currentSort)
                {
                    case "A-Z":
                        sortedBooks = books.OrderBy(book => book.Title).ToList();
                        break;
                    case "Z-A":
                        sortedBooks = books.OrderByDescending(book => book.Title).ToList();
                        break;
                    case "Year":
                        sortedBooks = books.OrderBy(book => book.ReleaseDate).ToList();
                        break;
                }

                // Update the ListView with sorted books
                BooksListView.ItemsSource = new ObservableCollection<PoirotCollection>(sortedBooks);
            }
            catch (Exception ex)
            {
                // Display an error message
                await DisplayAlert("Error", $"Failed to load books: {ex.Message}", "OK");
            }
        }


        // Updates button styles based on the active filter
        private void UpdateButtonStyles()
        {
            // Reset all button styles to LightGray
            OwnedButton.BackgroundColor = Colors.LightGray;
            WishlistButton.BackgroundColor = Colors.LightGray;
            AllButton.BackgroundColor = Colors.LightGray;

            // Highlight the button for the active filter
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

        // Notify the UI when a property changes
        protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            base.OnPropertyChanged(propertyName);
        }

        // BrowsePage.xaml.cs (OnBookTapped Method)
        private async void OnBookTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return; // Prevent errors if no item is tapped

            // Retrieve the selected book object
            var selectedBook = (PoirotCollection)e.Item;

            // Get the current list of filtered and sorted books
            var booksList = ((ObservableCollection<PoirotCollection>)BooksListView.ItemsSource).ToList();

            // Find the index of the selected book in the current list
            var selectedIndex = booksList.IndexOf(selectedBook);

            // Navigate to the TitleDetailsPage and pass the list and selected index
            await Navigation.PushAsync(new TitleDetailsPage(booksList, selectedIndex));

            // Deselect the tapped item to avoid retaining its highlight
            ((ListView)sender).SelectedItem = null;
        }


    }
}
