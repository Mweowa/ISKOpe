using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using BCrypt.Net;

namespace ISKOpe.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public LoginInput Input { get; set; }

        public string ErrorMessage { get; set; }

        public class LoginInput
        {
            [Required]
            public string StudentNumber { get; set; }

            [Required]
            public string Password { get; set; }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            string connectionString = "Data Source=.\\sqlexpress;Initial Catalog=mystore;Integrated Security=True;Encrypt=True;Trust Server Certificate=True";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var query = "SELECT password_hash FROM students WHERE student_number = @student_number";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@student_number", Input.StudentNumber);
                    var result = command.ExecuteScalar();

                    if (result != null)
                    {
                        var storedHash = result.ToString();

                        // Verify using BCrypt
                        if (BCrypt.Net.BCrypt.Verify(Input.Password, storedHash))
                        {
                            HttpContext.Session.SetString("StudentNumber", Input.StudentNumber);
                            return RedirectToPage("/Index");
                        }
                    }
                }
            }

            ErrorMessage = "Invalid student number or password.";
            return Page();
        }

       

       
    }
}
