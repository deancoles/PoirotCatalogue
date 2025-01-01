using MySqlConnector; // For connecting to MySQL
using PoirotCollectionApp.Models; // To use the models (e.g., PoirotCollection, User)
using System.Collections.Generic; // For working with lists
using System.Data; // For database operations
using System.Threading.Tasks; // For asynchronous methods

namespace PoirotCollectionApp.DataAccess
{
    // Provides database interaction methods
    public class DatabaseHelper
    {
        
        private readonly string _connectionString; // Stores the database connection string

        public DatabaseHelper() // Constructor - Sets up the database connection string
        {
            _connectionString = "Server=sql8.freemysqlhosting.net;Database=sql8746381;User=sql8746381;Password=SUVJ3DqUNZ;";
        }

        // Creates and returns a MySQL connection
        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        // Tests the database connection
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    await connection.OpenAsync(); // Attempt to open the connection
                    return true; // Connection succeeded
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}"); // Log error to the console
                return false; // Connection failed
            }
        }

        // Fetches all users from the database
        public async Task<List<User>> GetUsersAsync()
        {
            var users = new List<User>(); // List to store users

            try
            {
                using (var connection = GetConnection())
                {
                    await connection.OpenAsync(); // Open the database connection

                    // Query to fetch user details
                    string query = "SELECT UserID, UserName FROM users";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync()) // Read each row in the result
                            {
                                // Add user to the list
                                users.Add(new User
                                {
                                    UserID = reader.IsDBNull("UserID") ? 0 : reader.GetInt32("UserID"), // Default to 0 if UserID is null
                                    Username = reader.IsDBNull("UserName") ? "Unknown" : reader.GetString("UserName") // Default to "Unknown" if UserName is null
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching users: {ex.Message}");
            }

            return users; // Return the list of users
        }

        // Fetches a list of books based on the user ID and filter
        public async Task<List<PoirotCollection>> GetPoirotCollectionAsync(int userId, string filter)
        {
            var books = new List<PoirotCollection>(); // List to store books

            using (var connection = GetConnection())
            {
                await connection.OpenAsync(); // Open the database connection

                string query = "SELECT * FROM poirotcollection WHERE UserID = @UserID"; // Base query to fetch books for the user

                // Add filter for "Owned" books
                if (filter == "Owned")
                {
                    query += " AND Owned = 1";
                }
                // Add filter for "Wishlist" books
                else if (filter == "Wishlist")
                {
                    query += " AND Wishlist = 1";
                }

                // Set up the query with parameters
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userId); // Add user ID parameter

                    using (var reader = await command.ExecuteReaderAsync()) // Execute the query
                    {
                        while (await reader.ReadAsync()) // Read each row in the result
                        {
                            // Add book details to the list
                            books.Add(new PoirotCollection
                            {
                                UserID = reader.GetInt32("UserID"),
                                BookID = reader.GetInt32("BookID"),
                                Title = reader.GetString("Title"),
                                ReleaseDate = reader.IsDBNull("ReleaseDate") ? (DateTime?)null : reader.GetDateTime("ReleaseDate"), // Handle null dates
                                Notes = reader.IsDBNull("Notes") ? string.Empty : reader.GetString("Notes"), // Default to empty if Notes is null
                                CoverImagePath = reader.IsDBNull("CoverImagePath") ? string.Empty : reader.GetString("CoverImagePath"), // Default to empty if CoverImagePath is null
                                Owned = reader.GetBoolean("Owned"), // Check if the book is owned
                                Wishlist = reader.GetBoolean("Wishlist") // Check if the book is in the wishlist
                            });
                        }
                    }
                }
            }

            return books; // Return the list of books
        }
    }
}
