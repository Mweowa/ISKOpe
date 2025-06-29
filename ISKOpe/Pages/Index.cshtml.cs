using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient; // Required for SQL Server interaction
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISKOpe.Pages
{
    public class IndexModel : PageModel
    {
        // Nested class to represent an item from the database for display
        public class Item
        {
            public int Id { get; set; }
            public string ItemStatus { get; set; }
            public string ItemName { get; set; }
            public string Category { get; set; }
            public string Month { get; set; }
            public string Day { get; set; }
            public string Year { get; set; }
            public string Location { get; set; } // Will hold FoundLocation or LostLocation
            public string ImagePath { get; set; }
        }

        public List<Item> RecentItems { get; set; } = new List<Item>();

        public async Task OnGetAsync()
        {
            // Database connection string (get this from appsettings.json in a real app)
            string connectionString = "Data Source=.\\sqlexpress;Initial Catalog=mystore;Integrated Security=True;Encrypt=True;Trust Server Certificate=True";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    // Query to select the top 5 most recent items
                    // We order by Id DESC assuming higher Id means more recent
                    string query = @"
                        SELECT TOP 5 Id, ItemStatus, ItemName, Category, Month, Day, Year,
                        FoundLocation, LostLocation, ImagePath
                        FROM Items
                        ORDER BY Id DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var item = new Item
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    ItemStatus = reader.IsDBNull(reader.GetOrdinal("ItemStatus")) ? string.Empty : reader.GetString(reader.GetOrdinal("ItemStatus")),
                                    ItemName = reader.IsDBNull(reader.GetOrdinal("ItemName")) ? string.Empty : reader.GetString(reader.GetOrdinal("ItemName")),
                                    Category = reader.IsDBNull(reader.GetOrdinal("Category")) ? string.Empty : reader.GetString(reader.GetOrdinal("Category")),
                                    Month = reader.IsDBNull(reader.GetOrdinal("Month")) ? string.Empty : reader.GetString(reader.GetOrdinal("Month")),
                                    Day = reader.IsDBNull(reader.GetOrdinal("Day")) ? string.Empty : reader.GetString(reader.GetOrdinal("Day")),
                                    Year = reader.IsDBNull(reader.GetOrdinal("Year")) ? string.Empty : reader.GetString(reader.GetOrdinal("Year")),
                                    ImagePath = reader.IsDBNull(reader.GetOrdinal("ImagePath")) ? "/Assets/placeholder.png" : reader.GetString(reader.GetOrdinal("ImagePath")) // Default placeholder if no image
                                };

                                // Determine which location to display based on ItemStatus
                                if (item.ItemStatus == "found")
                                {
                                    item.Location = reader.IsDBNull(reader.GetOrdinal("FoundLocation")) ? string.Empty : reader.GetString(reader.GetOrdinal("FoundLocation"));
                                }
                                else if (item.ItemStatus == "lost")
                                {
                                    item.Location = reader.IsDBNull(reader.GetOrdinal("LostLocation")) ? string.Empty : reader.GetString(reader.GetOrdinal("LostLocation"));
                                }
                                else
                                {
                                    item.Location = string.Empty; // Or a generic message
                                }

                                RecentItems.Add(item);
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                // In a real application, you'd log this error using a logger
                // For now, we'll just ensure RecentItems remains empty
                System.Console.WriteLine($"Error fetching recent items: {ex.Message}");
                // Optionally, add a TempData error message for the user
                // TempData["ErrorMessage"] = "Could not load recent items.";
            }
        }
    }
}