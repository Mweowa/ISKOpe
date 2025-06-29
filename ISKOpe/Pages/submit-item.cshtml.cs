using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting; // Added for IWebHostEnvironment
using System.Linq; // Added for SelectMany in error display

namespace ISKOpe.Pages
{
    public class SubmitModel : PageModel
    {
        private readonly IWebHostEnvironment _env;

        public SubmitModel(IWebHostEnvironment env)
        {
            _env = env;
        }

        [BindProperty]
        public IFormFile ItemImage { get; set; }
        [BindProperty]
        public string ItemStatus { get; set; }
        [BindProperty]
        public string ItemName { get; set; }
        [BindProperty]
        public string Category { get; set; }
        [BindProperty]
        public string Month { get; set; }
        [BindProperty]
        public string Day { get; set; }
        [BindProperty]
        public string Year { get; set; }

        [BindProperty]
        public string FoundLocation { get; set; }

        // Remove [BindProperty] for PickupLocation if you want it strictly server-controlled
        // or keep it if you still want to display it as a hidden field.
        // If it's not bound, the value will *only* come from server-side assignment.
        public string PickupLocation { get; set; } = "PUP Main Guard House"; // Default value

        [BindProperty]
        public string LostLocation { get; set; }
        [BindProperty]
        public string LostHour { get; set; }
        [BindProperty]
        public string LostMinute { get; set; }
        [BindProperty]
        public string LostAMPM { get; set; }

        public List<string> Months { get; } = new()
        {
            "January", "February", "March", "April", "May", "June",
            "July", "August", "September", "October", "November", "December"
        };

        public void OnGet()
        {
            if (string.IsNullOrEmpty(ItemStatus))
            {
                ItemStatus = "found"; // Or "lost" if you prefer that as default
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Clear any model errors related to fields that are not relevant to the current ItemStatus
            if (ItemStatus == "lost")
            {
                ModelState.Remove(nameof(FoundLocation));
                // No need to remove PickupLocation if it's not [BindProperty]
                // If it is [BindProperty], you should remove it if you don't want client-side changes affecting it for 'lost' status.
                ModelState.Remove(nameof(PickupLocation));
            }
            else if (ItemStatus == "found")
            {
                ModelState.Remove(nameof(LostLocation));
                ModelState.Remove(nameof(LostHour));
                ModelState.Remove(nameof(LostMinute));
                ModelState.Remove(nameof(LostAMPM));

                // IMPORANT: Force PickupLocation to the default value for 'found' items
                PickupLocation = "PUP Main Guard House";
            }
            else
            {
                // This handles cases where ItemStatus might be null or an unexpected value
                ModelState.AddModelError(string.Empty, "Please select whether you lost or found the item.");
                return Page();
            }

            // Important: Check ModelState.IsValid AFTER potentially removing errors for irrelevant fields
            if (!ModelState.IsValid)
            {
                // This will return the page with validation errors shown
                return Page();
            }

            try
            {
                // Re-evaluate required fields for server-side validation based on ItemStatus
                if (ItemStatus == "lost")
                {
                    if (string.IsNullOrWhiteSpace(LostLocation) ||
                        string.IsNullOrWhiteSpace(LostHour) ||
                        string.IsNullOrWhiteSpace(LostMinute) ||
                        string.IsNullOrWhiteSpace(LostAMPM))
                    {
                        ModelState.AddModelError(string.Empty, "All lost item details (location, time) must be filled.");
                        return Page();
                    }
                }
                else if (ItemStatus == "found")
                {
                    if (string.IsNullOrWhiteSpace(FoundLocation))
                    {
                        ModelState.AddModelError(string.Empty, "Found location is required for found items.");
                        return Page();
                    }
                    // No need to check PickupLocation here as it's set above
                }
                // The else for ItemStatus validation is already handled above the ModelState.IsValid check


                // Handle image upload
                string imagePath = null;
                if (ItemImage != null && ItemImage.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ItemImage.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ItemImage.CopyToAsync(stream);
                    }

                    imagePath = "/uploads/" + fileName;
                }
                else
                {
                    // If image is required but not provided, add a model error
                    ModelState.AddModelError(nameof(ItemImage), "An item image is required.");
                    return Page();
                }


                // Insert into DB
                string connectionString = "Data Source=.\\sqlexpress;Initial Catalog=mystore;Integrated Security=True;Encrypt=True;Trust Server Certificate=True";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    string query = @"
                        INSERT INTO Items (
                            ItemStatus, ItemName, Category, Month, Day, Year,
                            FoundLocation, PickupLocation, LostLocation,
                            LostHour, LostMinute, LostAMPM, ImagePath
                        ) VALUES (
                            @ItemStatus, @ItemName, @Category, @Month, @Day, @Year,
                            @FoundLocation, @PickupLocation, @LostLocation,
                            @LostHour, @LostMinute, @LostAMPM, @ImagePath
                        )";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Use DBNull.Value for null/empty strings to correctly insert NULL into DB
                        cmd.Parameters.AddWithValue("@ItemStatus", ItemStatus ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ItemName", ItemName ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Category", Category ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Month", Month ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Day", Day ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Year", Year ?? (object)DBNull.Value);

                        // Conditional parameters for Found/Lost items
                        cmd.Parameters.AddWithValue("@FoundLocation",
                            ItemStatus == "found" && !string.IsNullOrWhiteSpace(FoundLocation) ? (object)FoundLocation : DBNull.Value);

                        // Ensure PickupLocation is always "PUP Main Guard House" for found items
                        cmd.Parameters.AddWithValue("@PickupLocation",
                            ItemStatus == "found" ? (object)PickupLocation : DBNull.Value);

                        cmd.Parameters.AddWithValue("@LostLocation",
                            ItemStatus == "lost" && !string.IsNullOrWhiteSpace(LostLocation) ? (object)LostLocation : DBNull.Value);
                        cmd.Parameters.AddWithValue("@LostHour",
                            ItemStatus == "lost" && !string.IsNullOrWhiteSpace(LostHour) ? (object)LostHour : DBNull.Value);
                        cmd.Parameters.AddWithValue("@LostMinute",
                            ItemStatus == "lost" && !string.IsNullOrWhiteSpace(LostMinute) ? (object)LostMinute : DBNull.Value);
                        cmd.Parameters.AddWithValue("@LostAMPM",
                            ItemStatus == "lost" && !string.IsNullOrWhiteSpace(LostAMPM) ? (object)LostAMPM : DBNull.Value);

                        cmd.Parameters.AddWithValue("@ImagePath", imagePath ?? (object)DBNull.Value);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                TempData["SuccessMessage"] = "Item successfully submitted.";
                return RedirectToPage("/submit-item"); // Redirect to clear form data
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes in a real application
                // _logger.LogError(ex, "Error submitting item.");

                // Add a user-friendly error message
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while saving your item. Please try again. Error: " + ex.Message);
                return Page(); // Return the page with the error message
            }
        }
    }
}