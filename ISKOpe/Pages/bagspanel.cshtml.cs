using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISKOpe.Pages
{
    public class BagsPanelModel : PageModel
    {
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

        public List<Item> LostItems { get; set; } = new List<Item>();
        public List<Item> FoundItems { get; set; } = new List<Item>();

        public async Task OnGetAsync()
        {
            string connectionString = "Data Source=.\\sqlexpress;Initial Catalog=mystore;Integrated Security=True;Encrypt=True;Trust Server Certificate=True";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    string query = @"
                        SELECT Id, ItemStatus, ItemName, Category, Month, Day, Year,
                        FoundLocation, LostLocation, ImagePath
                        FROM Items
                        WHERE Category = @Category
                        ORDER BY Id DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Category", "bags"); // Filtering only "bags"

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
                                    ImagePath = reader.IsDBNull(reader.GetOrdinal("ImagePath")) ? "/Assets/placeholder.png" : reader.GetString(reader.GetOrdinal("ImagePath"))
                                };

                                if (item.ItemStatus == "found")
                                {
                                    item.Location = reader.IsDBNull(reader.GetOrdinal("FoundLocation")) ? string.Empty : reader.GetString(reader.GetOrdinal("FoundLocation"));
                                    FoundItems.Add(item);
                                }
                                else if (item.ItemStatus == "lost")
                                {
                                    item.Location = reader.IsDBNull(reader.GetOrdinal("LostLocation")) ? string.Empty : reader.GetString(reader.GetOrdinal("LostLocation"));
                                    LostItems.Add(item);
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Error fetching bag items: {ex.Message}");
            }
        }
    }
}
