using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace CredentialManagementPortal
{
    public partial class Login : Page
    {
        protected void btnLogin_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            try
            {
                int userId;
                string username;
                string role;
                if (!TryAuthenticate(txtUsername.Text.Trim(), txtPassword.Text, out userId, out username, out role))
                {
                    lblMessage.Text = "Invalid username or password.";
                    lblMessage.Visible = true;
                    return;
                }

                Session["AuthenticatedUser"] = username;
                Session["UserID"] = userId;
                Session["UserRole"] = role;
                Response.Redirect("~/Dashboard", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (SqlException)
            {
                lblMessage.Text = "Unable to sign in. Please try again or contact your administrator.";
                lblMessage.Visible = true;
            }
        }

        private bool TryAuthenticate(string username, string password, out int userId, out string authenticatedUsername, out string role)
        {
            userId = 0;
            authenticatedUsername = null;
            role = null;
            string connectionString = ConfigurationManager.ConnectionStrings["Conn1"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand("dbo.usp_UserManagement", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@Action", SqlDbType.NVarChar, 30).Value = "LOGIN";
                command.Parameters.Add("@UserName", SqlDbType.NVarChar, 100).Value = username;
                command.Parameters.Add("@Password", SqlDbType.NVarChar, 100).Value = password;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!reader.Read()) return false;
                    userId = Convert.ToInt32(reader["ID"]);
                    authenticatedUsername = Convert.ToString(reader["UserName"]);
                    role = Convert.ToString(reader["Role"]);
                    return true;
                }
            }
        }
    }
}

