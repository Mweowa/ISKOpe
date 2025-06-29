using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace ISKOpe.Pages
{
    public class ProfileModel : PageModel
    {
        [BindProperty]
        public Student Student { get; set; }

        [BindProperty]
        public IFormFile ProfileImage { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }

        public IActionResult OnGet()
        {
            var studentNumber = HttpContext.Session.GetString("StudentNumber");
            if (string.IsNullOrEmpty(studentNumber))
            {
                return RedirectToPage("/Login");
            }

            string connectionString = "Data Source=.\\sqlexpress;Initial Catalog=mystore;Integrated Security=True;Encrypt=True;Trust Server Certificate=True";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var query = "SELECT student_number, name, email, contact_number, profile_picture FROM students WHERE student_number = @student_number";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@student_number", studentNumber);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Student = new Student
                            {
                                StudentNumber = reader["student_number"]?.ToString(),
                                Name = reader["name"]?.ToString(),
                                Email = reader["email"]?.ToString(),
                                ContactNumber = reader["contact_number"]?.ToString(),
                                ProfilePicture = reader["profile_picture"]?.ToString()
                            };
                        }
                    }
                }
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            var studentNumber = HttpContext.Session.GetString("StudentNumber");
            if (string.IsNullOrEmpty(studentNumber))
            {
                return RedirectToPage("/Login");
            }

            string fileName = null;

            // ✅ Step 1: Save uploaded image
            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                fileName = $"{Guid.NewGuid()}{Path.GetExtension(ProfileImage.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ProfileImage.CopyTo(stream);
                }
            }

            // ✅ Step 2: Update database
            string connectionString = "Data Source=.\\sqlexpress;Initial Catalog=mystore;Integrated Security=True;Encrypt=True;Trust Server Certificate=True";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"UPDATE students 
                         SET name = @name, 
                             email = @email, 
                             contact_number = @contact";

                if (!string.IsNullOrEmpty(fileName))
                {
                    query += ", profile_picture = @profile_picture";
                }

                query += " WHERE student_number = @student_number";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", Student.Name);
                    command.Parameters.AddWithValue("@email", Student.Email);
                    command.Parameters.AddWithValue("@contact", Student.ContactNumber);
                    command.Parameters.AddWithValue("@student_number", studentNumber);

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        command.Parameters.AddWithValue("@profile_picture", fileName);
                    }

                    command.ExecuteNonQuery();
                }
            }

            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToPage();
        }
    }

    public class Student
    {
        public string StudentNumber { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public string ProfilePicture { get; set; }
    }
}
