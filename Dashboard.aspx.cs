
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace CredentialManagementPortal
{
    public partial class Dashboard : Page
    {
        private readonly string conString =
            ConfigurationManager.ConnectionStrings["Conn1"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            SetCurrentUserDisplay();

            if (!IsPostBack)
            {
                LoadCounts();
                LoadRecentActivity();
                LoadExpiringSoon();
            }
        }

        // Logged-in user display
        private void SetCurrentUserDisplay()
        {
            string username = Convert.ToString(Session["AuthenticatedUser"]);
            string role = Convert.ToString(Session["UserRole"]);

            if (String.IsNullOrWhiteSpace(username))
                username = "User";

            if (String.IsNullOrWhiteSpace(role))
                role = "User";

            lblCurrentUserName.Text = Server.HtmlEncode(username);
            lblCurrentUserRole.Text = Server.HtmlEncode(role);
            lblAvatarInitial.Text =
                Server.HtmlEncode(username.Substring(0, 1).ToUpperInvariant());
        }

        // Logout
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();

            Response.Redirect(ResolveUrl("~/Login"), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        // Dashboard counts
        private void LoadCounts()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd = new SqlCommand("dbo.usp_Dashboard", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Action", SqlDbType.VarChar, 20)
                        .Value = "COUNTS";

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lblAxCount.Text = Convert.ToString(reader["AXRecords"]);
                            lblEmailCount.Text = Convert.ToString(reader["EmailRecords"]);
                            lblProductKeyCount.Text = Convert.ToString(reader["ProductKeyRecords"]);
                            lblVpnCount.Text = Convert.ToString(reader["VPNRecords"]);
                            lblUserCount.Text = Convert.ToString(reader["ActiveUsers"]);
                            lblDomainCount.Text = Convert.ToString(reader["TotalDomains"]);
                            lblSslCount.Text = Convert.ToString(reader["TotalSSL"]);
                            lblTotalCount.Text = Convert.ToString(reader["TotalRecords"]);
                        }
                        else
                        {
                            SetCountsUnavailable();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "Dashboard counts error: " + ex);

                SetCountsUnavailable();
            }
        }

        // Recent activity from DashboardActivityLog
        private void LoadRecentActivity()
        {
            try
            {
                DataTable activities = new DataTable();

                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd = new SqlCommand("dbo.usp_Dashboard", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Action", SqlDbType.VarChar, 20)
                        .Value = "ACTIVITY";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(activities);
                    }
                }

                rptRecentActivity.DataSource = activities;
                rptRecentActivity.DataBind();

                pnlNoRecentActivity.Visible = activities.Rows.Count == 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "Dashboard activity error: " + ex);

                rptRecentActivity.DataSource = null;
                rptRecentActivity.DataBind();
                pnlNoRecentActivity.Visible = true;
            }
        }

        // Reusable method to log Add, Update and Delete
        // Call this method from the relevant page after its DB operation succeeds.
        public static void LogDashboardActivity(
            string pageName,
            string actionName,
            string recordId,
            string recordName)
        {
            try
            {
                string connectionString =
                    ConfigurationManager.ConnectionStrings["Conn1"].ConnectionString;

                string actionBy = "Unknown User";

                // Caller identity is supplied by the page's session below instead.
                // Use LogDashboardActivityFromPage for normal WebForms handlers.
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "Dashboard activity logging error: " + ex);
            }
        }

        // Call this overload from a WebForms page using its Session values.
        public static void LogDashboardActivityFromPage(
            string actionBy,
            string pageName,
            string actionName,
            string recordId,
            string recordName)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(actionBy))
                    actionBy = "Unknown User";

                using (SqlConnection con = new SqlConnection(
                    ConfigurationManager.ConnectionStrings["Conn1"].ConnectionString))
                using (SqlCommand cmd = new SqlCommand("dbo.usp_Dashboard", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Action", SqlDbType.VarChar, 20)
                        .Value = "LOG";

                    cmd.Parameters.Add("@ActivityType", SqlDbType.NVarChar, 100)
                        .Value = pageName + " " + actionName;

                    cmd.Parameters.Add("@ActivityKey", SqlDbType.NVarChar, 50)
                        .Value = GetActivityKey(pageName);

                    cmd.Parameters.Add("@ActivityName", SqlDbType.NVarChar, 200)
                        .Value = String.IsNullOrWhiteSpace(recordName)
                            ? (object)(pageName + " " + actionName)
                            : recordName;

                    cmd.Parameters.Add("@ActivityIcon", SqlDbType.NVarChar, 50)
                        .Value = GetActivityIcon(pageName);

                    cmd.Parameters.Add("@ActionBy", SqlDbType.NVarChar, 100)
                        .Value = actionBy;

                    cmd.Parameters.Add("@PageName", SqlDbType.NVarChar, 100)
                        .Value = pageName;

                    cmd.Parameters.Add("@RecordID", SqlDbType.NVarChar, 100)
                        .Value = String.IsNullOrWhiteSpace(recordId)
                            ? (object)DBNull.Value
                            : recordId;

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "Dashboard activity logging error: " + ex);
            }
        }

        private static string GetActivityKey(string pageName)
        {
            switch ((pageName ?? "").ToLowerInvariant())
            {
                case "vpn": return "vpn";
                case "ax": return "ax";
                case "email": return "email";
                case "product keys": return "product";
                case "domain": return "domain";
                case "ssl": return "ssl";
                case "user management": return "user";
                default: return "activity";
            }
        }

        private static string GetActivityIcon(string pageName)
        {
            switch ((pageName ?? "").ToLowerInvariant())
            {
                case "vpn": return "VPN";
                case "ax": return "AX";
                case "email": return "@";
                case "product keys": return "KEY";
                case "domain": return "DOM";
                case "ssl": return "SSL";
                case "user management": return "USR";
                default: return "INFO";
            }
        }

        // Match each activity icon to the icon shown on its dashboard card.
        protected string GetActivityIconMarkup(object value)
        {
            switch (Convert.ToString(value).ToLowerInvariant())
            {
                case "vpn":
                    return "<svg viewBox='0 0 24 24'><path d='M12 2.8 19.5 6v5.1c0 5-3.2 8.3-7.5 10.1-4.3-1.8-7.5-5.1-7.5-10.1V6L12 2.8Z'/><path d='m9.5 12 1.7 1.7 3.5-3.8'/></svg>";
                case "ax":
                    return "AX";
                case "email":
                    return "<svg viewBox='0 0 24 24'><rect x='3' y='5' width='18' height='14' rx='2'/><path d='m4 7 8 6 8-6'/></svg>";
                case "product":
                    return "<svg viewBox='0 0 24 24'><circle cx='8' cy='15' r='4'/><path d='m11 12 8-8 2 2-2 2 2 2-3 3-2-2-2 2'/></svg>";
                case "domain":
                    return "<svg viewBox='0 0 24 24'><circle cx='12' cy='12' r='9'/><path d='M3 12h18M12 3a14 14 0 0 1 0 18M12 3a14 14 0 0 0 0 18'/></svg>";
                case "ssl":
                    return "<svg viewBox='0 0 24 24'><path d='M12 2.8 19.5 6v5.1c0 5-3.2 8.3-7.5 10.1-4.3-1.8-7.5-5.1-7.5-10.1V6L12 2.8Z'/><path d='M12 9v4m0 3h.01'/></svg>";
                case "user":
                    return "<svg viewBox='0 0 24 24'><circle cx='9' cy='8' r='3.5'/><path d='M3 20v-1.5A5.5 5.5 0 0 1 8.5 13h1a5.5 5.5 0 0 1 5.5 5.5V20M17 8h4m-2-2v4'/></svg>";
                default:
                    return "<svg viewBox='0 0 24 24'><circle cx='12' cy='12' r='9'/><path d='M12 11v5m0-8h.01'/></svg>";
            }
        }
        // Load expiring Domains and SSL certificates
        private void LoadExpiringSoon()
        {
            DataTable expiring = new DataTable();

            expiring.Columns.Add("AssetTypeKey", typeof(string));
            expiring.Columns.Add("AssetTypeLabel", typeof(string));
            expiring.Columns.Add("AssetIcon", typeof(string));
            expiring.Columns.Add("AssetName", typeof(string));
            expiring.Columns.Add("AssetSubtitle", typeof(string));
            expiring.Columns.Add("Expiry", typeof(DateTime));
            expiring.Columns.Add("DaysRemaining", typeof(int));

            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd = new SqlCommand("dbo.usp_Dashboard", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Action", SqlDbType.VarChar, 20)
                        .Value = "EXPIRY90";

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        AddExpiryRows(reader, expiring, "domain", "Domain",
                            "bi bi-globe2", "DomainName", "DNS");

                        if (reader.NextResult())
                        {
                            AddExpiryRows(reader, expiring, "ssl", "SSL Certificate",
                                "bi bi-shield-lock", "Provider", "CertificateType");
                        }
                    }
                }

                DataView sorted = expiring.DefaultView;
                sorted.Sort = "Expiry ASC";

                rptExpiringSoon.DataSource = sorted;
                rptExpiringSoon.DataBind();

                lblExpiringEmpty.Text =
                    "No domains or SSL certificates expire within the next 90 days.";

                lblExpiringEmpty.Visible = expiring.Rows.Count == 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "Dashboard expiry error: " + ex);

                rptExpiringSoon.DataSource = null;
                rptExpiringSoon.DataBind();

                lblExpiringEmpty.Text =
                    "Expiry information could not be loaded.";

                lblExpiringEmpty.Visible = true;
            }
        }

        private void AddExpiryRows(
            SqlDataReader reader,
            DataTable target,
            string typeKey,
            string typeLabel,
            string icon,
            string nameColumn,
            string subtitleColumn)
        {
            int expiryIndex = reader.GetOrdinal("Expiry");
            int daysIndex = reader.GetOrdinal("DaysRemaining");
            int nameIndex = reader.GetOrdinal(nameColumn);
            int subtitleIndex = -1;

            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (String.Equals(reader.GetName(i), subtitleColumn,
                    StringComparison.OrdinalIgnoreCase))
                {
                    subtitleIndex = i;
                    break;
                }
            }

            while (reader.Read())
            {
                if (reader.IsDBNull(expiryIndex) || reader.IsDBNull(daysIndex))
                    continue;

                DataRow row = target.NewRow();

                row["AssetTypeKey"] = typeKey;
                row["AssetTypeLabel"] = typeLabel;
                row["AssetIcon"] = icon;
                row["AssetName"] = reader.IsDBNull(nameIndex)
                    ? String.Empty
                    : Convert.ToString(reader.GetValue(nameIndex));

                row["AssetSubtitle"] =
                    subtitleIndex < 0 || reader.IsDBNull(subtitleIndex)
                        ? String.Empty
                        : Convert.ToString(reader.GetValue(subtitleIndex));

                row["Expiry"] = Convert.ToDateTime(reader.GetValue(expiryIndex));
                row["DaysRemaining"] = Convert.ToInt32(reader.GetValue(daysIndex));

                target.Rows.Add(row);
            }
        }

        protected string GetExpiryText(object value)
        {
            int days = Convert.ToInt32(value);

            if (days == 0) return "Expires today";
            if (days == 1) return "Expires tomorrow";
            if (days < 0) return "Expired " + Math.Abs(days) + " days ago";

            return "Expires in " + days + " days";
        }

        private void SetCountsUnavailable()
        {
            lblAxCount.Text = "—";
            lblEmailCount.Text = "—";
            lblUserCount.Text = "—";
            lblProductKeyCount.Text = "—";
            lblDomainCount.Text = "—";
            lblSslCount.Text = "—";
            lblVpnCount.Text = "—";
            lblTotalCount.Text = "—";
        }
    }
}
