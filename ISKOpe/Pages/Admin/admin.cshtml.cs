// Pages/Admin/admin.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISKOpe.Pages.Admin
{
    public class AdminModel : PageModel
    {
        public class Item
        {
            public int Id { get; set; }
            public string ItemStatus { get; set; } // "lost" or "found"
            public string ItemName { get; set; }
            public string Category { get; set; }
            public string Month { get; set; }
            public string Day { get; set; }
            public string Year { get; set; }
            public string Location { get; set; }
            public string ImagePath { get; set; }
        }

        public List<Item> AllItems { get; set; } = new();
        public int TotalLost { get; set; }
        public int TotalFound { get; set; }

        private readonly string _connectionString = "Data Source=.\\sqlexpress;Initial Catalog=mystore;Integrated Security=True;Encrypt=True;Trust Server Certificate=True";

        public async Task OnGetAsync()
        {
            await LoadAllItems();
        }

        private async Task LoadAllItems()
        {
            AllItems.Clear();
            TotalLost = 0;
            TotalFound = 0;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = @"
                    SELECT Id, ItemStatus, ItemName, Category, Month, Day, Year,
                           COALESCE(FoundLocation, LostLocation) AS Location, ImagePath
                    FROM Items
                    ORDER BY Id DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var status = reader["ItemStatus"].ToString().ToLower();
                            if (status == "lost") TotalLost++;
                            else if (status == "found") TotalFound++;

                            AllItems.Add(new Item
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                ItemStatus = status,
                                ItemName = reader["ItemName"].ToString(),
                                Category = reader["Category"].ToString(),
                                Month = reader["Month"].ToString(),
                                Day = reader["Day"].ToString(),
                                Year = reader["Year"].ToString(),
                                Location = reader["Location"].ToString(),
                                ImagePath = reader["ImagePath"]?.ToString() ?? "/Assets/placeholder.png"
                            });
                        }
                    }
                }
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "DELETE FROM Items WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            await LoadAllItems(); // Refresh list and counts
            return Page(); // Return updated page
        }
    }
}
