using System;
using System.Web.UI;

namespace CredentialManagementPortal
{
    public partial class Login : Page
    {
        protected void btnLogin_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            bool validUsername = String.Equals(txtUsername.Text.Trim(), "manish", StringComparison.OrdinalIgnoreCase);
            bool validPassword = String.Equals(txtPassword.Text, "manish@2026", StringComparison.Ordinal);

            if (validUsername && validPassword)
            {
                Session["AuthenticatedUser"] = "manish";
                Response.Redirect("~/Dashboard", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            lblMessage.Text = "Invalid username or password.";
            lblMessage.Visible = true;
        }
    }
}

