using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace CredentialManagementPortal
{
    public partial class Dashboard : Page
    {
        private readonly string conString = ConfigurationManager.ConnectionStrings["Conn1"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            SetCurrentUserDisplay();
            if (!IsPostBack) { LoadCounts(); LoadRecentActivity(); LoadExpiringSoon(); }
        }


        private void SetCurrentUserDisplay()
        {
            string username = Convert.ToString(Session["AuthenticatedUser"]);
            string role = Convert.ToString(Session["UserRole"]);
            if (String.IsNullOrWhiteSpace(username)) username = "User";
            if (String.IsNullOrWhiteSpace(role)) role = "User";
            lblCurrentUserName.Text = Server.HtmlEncode(username);
            lblCurrentUserRole.Text = Server.HtmlEncode(role);
            lblAvatarInitial.Text = Server.HtmlEncode(username.Substring(0, 1).ToUpperInvariant());
        }
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect(ResolveUrl("~/Login"), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void LoadRecentActivity()
        {
            const string query = @"
                SELECT TOP (6) ActivityType, ActivityKey, ActivityName, ActivityIcon, CreatedDate
                FROM
                (
                    SELECT N'VPN Login' AS ActivityType, 'vpn' AS ActivityKey, VPNName AS ActivityName, 'VPN' AS ActivityIcon, CreatedDate FROM VPNCredentials WHERE CreatedDate IS NOT NULL
                    UNION ALL
                    SELECT N'AX Login', 'ax', Environment, 'AX', CreatedDate FROM AXLoginDetails WHERE CreatedDate IS NOT NULL
                    UNION ALL
                    SELECT N'Email Account', 'email', EmailID, '@', CreatedDate FROM EmailLoginDetails WHERE CreatedDate IS NOT NULL
                    UNION ALL
                    SELECT N'Product Key', 'product', Software, 'KEY', CreatedDate FROM ProductKeys WHERE CreatedDate IS NOT NULL
                    UNION ALL
                    SELECT N'Domain', 'domain', DomainName, 'DOM', CreatedDate FROM Domains WHERE CreatedDate IS NOT NULL
                    UNION ALL
                    SELECT N'SSL Certificate', 'ssl', Provider, 'SSL', CreatedDate FROM SSLDetails WHERE CreatedDate IS NOT NULL
                    UNION ALL
                    SELECT CASE ActionName WHEN 'CREATE USER' THEN N'User Created' WHEN 'UPDATE USER' THEN N'User Updated' WHEN 'DELETE USER' THEN N'User Deactivated' ELSE N'User Activity' END, 'user', UserName, 'USR', ActionDate FROM UserManagement WHERE ActionDate IS NOT NULL
                ) AS RecentRecords
                ORDER BY CreatedDate DESC;";

            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd = new SqlCommand(query, con))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable activity = new DataTable();
                    adapter.Fill(activity);
                    rptRecentActivity.DataSource = activity;
                    rptRecentActivity.DataBind();
                    pnlNoRecentActivity.Visible = rptRecentActivity.Items.Count == 0;
                }
            }
            catch
            {
                rptRecentActivity.DataSource = null;
                rptRecentActivity.DataBind();
                    pnlNoRecentActivity.Visible = rptRecentActivity.Items.Count == 0;
            }
        }
        private void LoadCounts()
        {
            try
            {
                int activeUsers = GetManagedUserCount();
                int baseTotal;
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd = new SqlCommand("dbo.InsertProductKeys", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "COUNTS");
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) throw new InvalidOperationException("Credential counts were not returned.");
                        lblAxCount.Text = Convert.ToInt32(reader["AXRecords"]).ToString();
                        lblEmailCount.Text = Convert.ToInt32(reader["EmailRecords"]).ToString();
                        lblUserCount.Text = activeUsers.ToString();
                        lblProductKeyCount.Text = Convert.ToInt32(reader["ProductKeyRecords"]).ToString();
                        lblVpnCount.Text = Convert.ToInt32(reader["VPNRecords"]).ToString();
                        baseTotal = Convert.ToInt32(reader["TotalRecords"]);
                    }
                }
                int domains = GetAssetCount("dbo.InsertDomainDetails", "TotalDomains");
                int certificates = GetAssetCount("dbo.InsertSSLDetails", "TotalSSL");
                lblDomainCount.Text = domains.ToString();
                lblSslCount.Text = certificates.ToString();
                lblTotalCount.Text = (baseTotal + domains + certificates + activeUsers).ToString();
            }
            catch { SetCountsUnavailable(); }
        }

        private int GetManagedUserCount()
        {
            using (SqlConnection con = new SqlConnection(conString))
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.UserManagement WHERE IsActive = 1", con))
            {
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
        private int GetAssetCount(string procedure, string resultColumn)
        {
            using (SqlConnection con = new SqlConnection(conString))
            using (SqlCommand cmd = new SqlCommand(procedure, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "COUNTS");
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) return 0;
                    return Convert.ToInt32(reader[resultColumn]);
                }
            }
        }

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
                AddExpiringRows(expiring, "dbo.InsertDomainDetails", "domain", "bi bi-globe2", "DomainName", "DNS");
                AddExpiringRows(expiring, "dbo.InsertSSLDetails", "ssl", "bi bi-shield-lock", "Provider", "CertificateType");
                DataView sorted = expiring.DefaultView;
                sorted.Sort = "Expiry ASC";
                rptExpiringSoon.DataSource = sorted;
                rptExpiringSoon.DataBind();
                lblExpiringEmpty.Visible = rptExpiringSoon.Items.Count == 0;
            }
            catch
            {
                rptExpiringSoon.DataSource = null;
                rptExpiringSoon.DataBind();
                lblExpiringEmpty.Text = "Expiry information could not be loaded.";
                lblExpiringEmpty.Visible = true;
            }
        }

        private void AddExpiringRows(DataTable target, string procedure, string typeKey, string icon, string nameColumn, string subtitleColumn)
        {
            DataTable records = new DataTable();
            using (SqlConnection con = new SqlConnection(conString))
            using (SqlCommand cmd = new SqlCommand(procedure, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "EXPIRY90");
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd)) adapter.Fill(records);
            }
            foreach (DataRow record in records.Rows)
            {
                if (record["Expiry"] == DBNull.Value || record["DaysRemaining"] == DBNull.Value) continue;
                DataRow item = target.NewRow();
                item["AssetTypeKey"] = typeKey;
                item["AssetTypeLabel"] = typeKey == "domain" ? "Domain" : "SSL Certificate";
                item["AssetIcon"] = icon;
                item["AssetName"] = Convert.ToString(record[nameColumn]);
                item["AssetSubtitle"] = records.Columns.Contains(subtitleColumn) ? Convert.ToString(record[subtitleColumn]) : String.Empty;
                item["Expiry"] = Convert.ToDateTime(record["Expiry"]);
                item["DaysRemaining"] = Convert.ToInt32(record["DaysRemaining"]);
                target.Rows.Add(item);
            }
        }

        protected string GetExpiryText(object value)
        {
            int days = Convert.ToInt32(value);
            if (days == 0) return "Expires today";
            if (days == 1) return "Expires tomorrow";
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
















