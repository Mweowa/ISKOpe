using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISKOpe.Pages
{
    public class ClothingPanelModel : PageModel
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
            public string Location { get; set; }
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
                        cmd.Parameters.AddWithValue("@Category", "clothing");

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var item = new Item
                                {
                                    Id = reader.GetInt32(0),
                                    ItemStatus = reader["ItemStatus"]?.ToString() ?? "",
                                    ItemName = reader["ItemName"]?.ToString() ?? "",
                                    Category = reader["Category"]?.ToString() ?? "",
                                    Month = reader["Month"]?.ToString() ?? "",
                                    Day = reader["Day"]?.ToString() ?? "",
                                    Year = reader["Year"]?.ToString() ?? "",
                                    ImagePath = reader["ImagePath"]?.ToString() ?? "/Assets/placeholder.png"
                                };

                                item.Location = item.ItemStatus == "found"
                                    ? reader["FoundLocation"]?.ToString() ?? ""
                                    : reader["LostLocation"]?.ToString() ?? "";

                                if (item.ItemStatus == "found") FoundItems.Add(item);
                                else if (item.ItemStatus == "lost") LostItems.Add(item);
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Error fetching clothing items: {ex.Message}");
            }
        }
    }
}
