using System;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace CredentialManagementPortal
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string currentPage = Request.AppRelativeCurrentExecutionFilePath;
            string pageName = Path.GetFileNameWithoutExtension(currentPage);
            bool isLoginPage = String.Equals(pageName, "Login", StringComparison.OrdinalIgnoreCase);

            phAppSidebar.Visible = !isLoginPage;
            SetNavState(navDashboard, pageName == "Dashboard");
            SetNavState(navVpn, pageName == "VPNLoingDetails");
            SetNavState(navAx, pageName == "AXLoginDetails");
            SetNavState(navEmail, pageName == "EmailAccountDetails");
            SetNavState(navProductKeys, pageName == "ProductKeys");
            SetNavState(navDomainSsl, pageName == "DomainAndSSL");
            string userRole = Convert.ToString(Session["UserRole"]);
            bool canViewUsers = String.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase);
            navUserManagement.Visible = canViewUsers;
            SetNavState(navUserManagement, pageName == "UserManagement");

            if (!isLoginPage && Session["AuthenticatedUser"] == null)
            {
                Response.Redirect(ResolveUrl("~/Login"), false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        private void SetNavState(HtmlAnchor link, bool isActive)
        {
            link.Attributes["class"] = isActive ? "app-nav-link active" : "app-nav-link";
        }
    }
}







