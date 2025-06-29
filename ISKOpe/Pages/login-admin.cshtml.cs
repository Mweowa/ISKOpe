using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;

namespace ISKOpe.Pages
{
    public class login_adminModel : PageModel
    {
        [BindProperty]
        public LoginInput Input { get; set; }

        public string ErrorMessage { get; set; }

        public class LoginInput
        {
            [Required]
            public string Username { get; set; }

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
                string query = "SELECT password FROM admins WHERE username = @username";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", Input.Username);
                    var result = command.ExecuteScalar();

                    if (result != null)
                    {
                        string storedHash = result.ToString();

                        if (BCrypt.Net.BCrypt.Verify(Input.Password, storedHash))
                        {
                            HttpContext.Session.SetString("AdminUsername", Input.Username);
                            return RedirectToPage("/Admin/admin");
                        }
                    }
                }
            }

            ErrorMessage = "Invalid username or password.";
            return Page();
        }
    }
}
