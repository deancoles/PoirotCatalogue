using PoirotCollectionApp.Models; // For PoirotCollection model

namespace PoirotCollectionApp
{
    public partial class TitleDetailsPage : ContentPage
    {
        // Fields to store the book list and the current index
        private List<PoirotCollection> _books; // List of books passed to the page
        private int _currentIndex; // Index of the currently displayed book

        // Constructor - Initialize the page with the book list and selected book index
        public TitleDetailsPage(List<PoirotCollection> books, int selectedIndex)
        {
            InitializeComponent();

            _books = books; // Store the book list
            _currentIndex = selectedIndex; // Set the initial book index

            DisplayBookDetails(_books[_currentIndex]);
        }

        // Displays details of the currently selected book
        private void DisplayBookDetails(PoirotCollection book)
        {
            // Update the title label
            TitleLabel.Text = book.Title;

            // Update the release date label or show "Unknown" if no date is available
            ReleaseDateLabel.Text = book.ReleaseDate.HasValue
                ? book.ReleaseDate.Value.ToString("dd-MM-yyyy")
                : "Unknown";

            // Update the description or show a default message if none is available
            DescriptionLabel.Text = !string.IsNullOrEmpty(book.Notes)
                ? book.Notes
                : "No description available.";

            // Update the cover image if a valid URL is provided, or use a default cover image
            if (!string.IsNullOrEmpty(book.CoverImagePath))
            {
                CoverImage.Source = ImageSource.FromUri(new Uri(book.CoverImagePath));
            }
            else
            {
                CoverImage.Source = "default_cover.png";
            }
        }

        // Navigates back to the previous page
        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        // Displays the previous book in the list
        private void OnPreviousBookClicked(object sender, EventArgs e)
        {
            if (_currentIndex > 0) // Ensure there is a previous book
            {
                _currentIndex--; // Move to the previous index
                DisplayBookDetails(_books[_currentIndex]); // Update the display
            }
        }

        // Displays the next book in the list 
        private void OnNextBookClicked(object sender, EventArgs e)
        {
            if (_currentIndex < _books.Count - 1) // Ensure there is a next book
            {
                _currentIndex++; // Move to the next index
                DisplayBookDetails(_books[_currentIndex]); // Update the display
            }
        }
    }
}
