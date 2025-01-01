using PoirotCollectionApp.Models;

namespace PoirotCollectionApp
{
    public partial class TitleDetailsPage : ContentPage
    {
        private List<PoirotCollection> _books;
        private int _currentIndex;

        public TitleDetailsPage(List<PoirotCollection> books, int selectedIndex)
        {
            InitializeComponent();

            _books = books;
            _currentIndex = selectedIndex;

            DisplayBookDetails(_books[_currentIndex]);
        }

        private void DisplayBookDetails(PoirotCollection book)
        {
            TitleLabel.Text = book.Title;
            ReleaseDateLabel.Text = book.ReleaseDate.HasValue
                ? book.ReleaseDate.Value.ToString("dd-MM-yyyy")
                : "Unknown";
            DescriptionLabel.Text = !string.IsNullOrEmpty(book.Notes)
                ? book.Notes
                : "No description available.";

            if (!string.IsNullOrEmpty(book.CoverImagePath))
            {
                CoverImage.Source = ImageSource.FromUri(new Uri(book.CoverImagePath));
            }
            else
            {
                CoverImage.Source = "default_cover.png";
            }
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private void OnPreviousBookClicked(object sender, EventArgs e)
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                DisplayBookDetails(_books[_currentIndex]);
            }
        }

        private void OnNextBookClicked(object sender, EventArgs e)
        {
            if (_currentIndex < _books.Count - 1)
            {
                _currentIndex++;
                DisplayBookDetails(_books[_currentIndex]);
            }
        }
    }
}
