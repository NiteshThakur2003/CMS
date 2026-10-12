
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Diagnostics;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CredentialManagementPortal
{
    public partial class DomainAndSSL : Page
    {
        private readonly string conString =
            ConfigurationManager.ConnectionStrings["Conn1"].ConnectionString;

        private const string DomainsTab = "domains-tab";
        private const string SslTab = "ssl-tab";

        private bool IsAdmin()
        {
            return String.Equals(
                Convert.ToString(Session["UserRole"]),
                "Admin",
                StringComparison.OrdinalIgnoreCase);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            btnAddDomain.Visible = IsAdmin();
            btnAddSsl.Visible = IsAdmin();
            btnSaveDomain.Visible = IsAdmin();
            btnSaveSsl.Visible = IsAdmin();

            if (!IsPostBack)
            {
                hfActiveTab.Value =
                    String.Equals(Request.QueryString["tab"], "ssl",
                        StringComparison.OrdinalIgnoreCase)
                    ? SslTab
                    : DomainsTab;

                LoadDomains();
                LoadSslCertificates();
            }
        }

        // =========================
        // LOAD DATA
        // =========================
        private void LoadDomains()
        {
            gvDomains.DataSource =
                GetRecords("dbo.InsertDomainDetails", "SELECT");

            gvDomains.DataBind();
        }

        private void LoadSslCertificates()
        {
            gvSsl.DataSource =
                GetRecords("dbo.InsertSSLDetails", "SELECT");

            gvSsl.DataBind();
        }

        private DataTable GetRecords(
            string procedure, string action, int? id = null)
        {
            using (SqlConnection con = new SqlConnection(conString))
            using (SqlCommand cmd = new SqlCommand(procedure, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20)
                    .Value = action;

                if (id.HasValue)
                {
                    cmd.Parameters.Add("@ID", SqlDbType.Int)
                        .Value = id.Value;
                }

                DataTable data = new DataTable();

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(data);
                }

                return data;
            }
        }

        // =========================
        // SAVE DOMAIN
        // =========================
        protected void btnSaveDomain_Click(object sender, EventArgs e)
        {
            if (!IsAdmin()) return;

            int id;
            bool isUpdate = Int32.TryParse(hfDomainID.Value, out id);
            DateTime expiry;

            if (String.IsNullOrWhiteSpace(txtDomainName.Text))
            {
                ShowMessage("Enter a domain name.", DomainsTab);
                return;
            }

            if (!DateTime.TryParseExact(
                txtDomainExpiry.Text,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out expiry))
            {
                ShowMessage("Select a valid domain expiry date.", DomainsTab);
                return;
            }

            string domainName = txtDomainName.Text.Trim();

            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd =
                    new SqlCommand("dbo.InsertDomainDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20)
                        .Value = isUpdate ? "UPDATE" : "INSERT";

                    if (isUpdate)
                    {
                        cmd.Parameters.Add("@ID", SqlDbType.Int)
                            .Value = id;
                    }

                    cmd.Parameters.Add("@DomainName", SqlDbType.NVarChar, 255)
                        .Value = domainName;

                    cmd.Parameters.Add("@Registrar", SqlDbType.NVarChar, 150)
                        .Value = DbValue(txtRegistrar.Text);

                    cmd.Parameters.Add("@Expiry", SqlDbType.Date)
                        .Value = expiry.Date;

                    cmd.Parameters.Add("@DNS", SqlDbType.NVarChar, 500)
                        .Value = DbValue(txtDNS.Text);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                LogDashboardActivity(
                    "Domain " + (isUpdate ? "Updated" : "Added"),
                    domainName,
                    "domain",
                    "bi-globe2",
                    isUpdate ? Convert.ToString(id) : null);

                ClearDomainForm();
                LoadDomains();

                RegisterUiScript(
                    "domainSaved",
                    "var m=bootstrap.Modal.getInstance(" +
                    "document.getElementById('domainModal'));" +
                    "if(m)m.hide();" +
                    "selectAssetTab('domains-tab');" +
                    "alert('Domain " +
                    (isUpdate ? "updated" : "saved") +
                    " successfully.');");
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, DomainsTab);
            }
        }

        // =========================
        // SAVE SSL CERTIFICATE
        // =========================
        protected void btnSaveSsl_Click(object sender, EventArgs e)
        {
            if (!IsAdmin()) return;

            int id;
            bool isUpdate = Int32.TryParse(hfSslID.Value, out id);
            DateTime expiry;

            if (String.IsNullOrWhiteSpace(txtSslProvider.Text))
            {
                ShowMessage("Enter an SSL provider.", SslTab);
                return;
            }

            if (!DateTime.TryParseExact(
                txtSslExpiry.Text,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out expiry))
            {
                ShowMessage("Select a valid certificate expiry date.", SslTab);
                return;
            }

            string provider = txtSslProvider.Text.Trim();

            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd =
                    new SqlCommand("dbo.InsertSSLDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20)
                        .Value = isUpdate ? "UPDATE" : "INSERT";

                    if (isUpdate)
                    {
                        cmd.Parameters.Add("@ID", SqlDbType.Int)
                            .Value = id;
                    }

                    cmd.Parameters.Add("@Provider", SqlDbType.NVarChar, 150)
                        .Value = provider;

                    cmd.Parameters.Add("@Expiry", SqlDbType.Date)
                        .Value = expiry.Date;

                    cmd.Parameters.Add(
                        "@CertificateType", SqlDbType.NVarChar, 100)
                        .Value = DbValue(txtCertificateType.Text);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                LogDashboardActivity(
                    "SSL Certificate " + (isUpdate ? "Updated" : "Added"),
                    provider,
                    "ssl",
                    "bi-shield-lock",
                    isUpdate ? Convert.ToString(id) : null);

                ClearSslForm();
                LoadSslCertificates();

                RegisterUiScript(
                    "sslSaved",
                    "var m=bootstrap.Modal.getInstance(" +
                    "document.getElementById('sslModal'));" +
                    "if(m)m.hide();" +
                    "selectAssetTab('ssl-tab');" +
                    "alert('SSL certificate " +
                    (isUpdate ? "updated" : "saved") +
                    " successfully.');");
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, SslTab);
            }
        }

        // =========================
        // DOMAIN GRID EVENTS
        // =========================
        protected void gvDomains_RowDataBound(
            object sender, GridViewRowEventArgs e)
        {
            if (!IsAdmin() && e.Row.Cells.Count > 0)
            {
                e.Row.Cells[e.Row.Cells.Count - 1].Visible = false;
            }
        }

        protected void gvDomains_RowCommand(
            object sender, GridViewCommandEventArgs e)
        {
            if (!IsAdmin()) return;

            int id;

            if (!Int32.TryParse(Convert.ToString(e.CommandArgument), out id))
                return;

            if (e.CommandName == "EditRow")
            {
                LoadDomain(id);
            }
            else if (e.CommandName == "DeleteRow")
            {
                ExecuteDelete("dbo.InsertDomainDetails", id);
                LoadDomains();

                RegisterUiScript(
                    "domainTab",
                    "selectAssetTab('domains-tab');");
            }
        }

        // =========================
        // SSL GRID EVENTS
        // =========================
        protected void gvSsl_RowDataBound(
            object sender, GridViewRowEventArgs e)
        {
            if (!IsAdmin() && e.Row.Cells.Count > 0)
            {
                e.Row.Cells[e.Row.Cells.Count - 1].Visible = false;
            }
        }

        protected void gvSsl_RowCommand(
            object sender, GridViewCommandEventArgs e)
        {
            if (!IsAdmin()) return;

            int id;

            if (!Int32.TryParse(Convert.ToString(e.CommandArgument), out id))
                return;

            if (e.CommandName == "EditRow")
            {
                LoadSsl(id);
            }
            else if (e.CommandName == "DeleteRow")
            {
                ExecuteDelete("dbo.InsertSSLDetails", id);
                LoadSslCertificates();

                RegisterUiScript(
                    "sslTab",
                    "selectAssetTab('ssl-tab');");
            }
        }

        // =========================
        // LOAD DOMAIN FOR EDIT
        // =========================
        private void LoadDomain(int id)
        {
            try
            {
                DataTable rows =
                    GetRecords("dbo.InsertDomainDetails", "SELECTBYID", id);

                if (rows.Rows.Count == 0) return;

                DataRow row = rows.Rows[0];

                hfDomainID.Value = Convert.ToString(row["DomainID"]);
                txtDomainName.Text = Convert.ToString(row["DomainName"]);
                txtRegistrar.Text = Convert.ToString(row["Registrar"]);
                txtDNS.Text = Convert.ToString(row["DNS"]);

                txtDomainExpiry.Text =
                    Convert.ToDateTime(row["Expiry"])
                    .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                RegisterUiScript(
                    "editDomain",
                    "selectAssetTab('domains-tab');" +
                    "openAssetModal('domainModal');");
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, DomainsTab);
            }
        }

        // =========================
        // LOAD SSL FOR EDIT
        // =========================
        private void LoadSsl(int id)
        {
            try
            {
                DataTable rows =
                    GetRecords("dbo.InsertSSLDetails", "SELECTBYID", id);

                if (rows.Rows.Count == 0) return;

                DataRow row = rows.Rows[0];

                hfSslID.Value = Convert.ToString(row["SSLID"]);
                txtSslProvider.Text = Convert.ToString(row["Provider"]);
                txtCertificateType.Text =
                    Convert.ToString(row["CertificateType"]);

                txtSslExpiry.Text =
                    Convert.ToDateTime(row["Expiry"])
                    .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                RegisterUiScript(
                    "editSsl",
                    "selectAssetTab('ssl-tab');" +
                    "openAssetModal('sslModal');");
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, SslTab);
            }
        }

        // =========================
        // DELETE DOMAIN / SSL
        // =========================
        private void ExecuteDelete(string procedure, int id)
        {
            bool isDomain = String.Equals(
                procedure,
                "dbo.InsertDomainDetails",
                StringComparison.OrdinalIgnoreCase);

            string nameColumn = isDomain ? "DomainName" : "Provider";
            string activityType =
                isDomain ? "Domain Deleted" : "SSL Certificate Deleted";

            string activityKey = isDomain ? "domain" : "ssl";
            string activityIcon = isDomain ? "bi-globe2" : "bi-shield-lock";
            string entityName = String.Empty;

            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                {
                    con.Open();

                    using (SqlTransaction transaction = con.BeginTransaction())
                    {
                        try
                        {
                            // Read the record name before deletion.
                            using (SqlCommand select =
                                new SqlCommand(procedure, con, transaction))
                            {
                                select.CommandType = CommandType.StoredProcedure;

                                select.Parameters.Add(
                                    "@Action", SqlDbType.NVarChar, 20)
                                    .Value = "SELECTBYID";

                                select.Parameters.Add("@ID", SqlDbType.Int)
                                    .Value = id;

                                using (SqlDataReader reader = select.ExecuteReader())
                                {
                                    if (!reader.Read())
                                    {
                                        transaction.Rollback();
                                        return;
                                    }

                                    entityName = Convert.ToString(reader[nameColumn]);
                                }
                            }

                            // Delete the selected record.
                            using (SqlCommand delete =
                                new SqlCommand(procedure, con, transaction))
                            {
                                delete.CommandType = CommandType.StoredProcedure;

                                delete.Parameters.Add(
                                    "@Action", SqlDbType.NVarChar, 20)
                                    .Value = "DELETE";

                                delete.Parameters.Add("@ID", SqlDbType.Int)
                                    .Value = id;

                                delete.ExecuteNonQuery();
                            }

                            // Write activity log in the same transaction.
                            using (SqlCommand log =
                                new SqlCommand("dbo.usp_Dashboard", con, transaction))
                            {
                                log.CommandType = CommandType.StoredProcedure;

                                log.Parameters.Add("@Action", SqlDbType.VarChar, 20)
                                    .Value = "LOG";

                                log.Parameters.Add(
                                    "@ActivityType", SqlDbType.NVarChar, 100)
                                    .Value = activityType;

                                log.Parameters.Add(
                                    "@ActivityKey", SqlDbType.NVarChar, 50)
                                    .Value = activityKey;

                                log.Parameters.Add(
                                    "@ActivityName", SqlDbType.NVarChar, 200)
                                    .Value = (isDomain ? "Domain: " : "SSL: ")
                                        + entityName;

                                log.Parameters.Add(
                                    "@ActivityIcon", SqlDbType.NVarChar, 50)
                                    .Value = activityIcon;

                                string actionBy =
                                    Convert.ToString(Session["AuthenticatedUser"]);

                                if (String.IsNullOrWhiteSpace(actionBy))
                                    actionBy = "Unknown User";

                                log.Parameters.Add(
                                    "@ActionBy", SqlDbType.NVarChar, 100)
                                    .Value = actionBy;

                                log.Parameters.Add(
                                    "@PageName", SqlDbType.NVarChar, 100)
                                    .Value = "DomainAndSSL";

                                log.Parameters.Add(
                                    "@RecordID", SqlDbType.NVarChar, 100)
                                    .Value = Convert.ToString(id);

                                log.ExecuteNonQuery();
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            if (transaction.Connection != null)
                                transaction.Rollback();

                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage(
                    ex.Message,
                    isDomain ? DomainsTab : SslTab);
            }
        }

        // =========================
        // DASHBOARD ACTIVITY LOGGER
        // =========================
        private void LogDashboardActivity(
            string activityType,
            string recordName,
            string activityKey,
            string activityIcon,
            string recordId)
        {
            try
            {
                string actionBy =
                    Convert.ToString(Session["AuthenticatedUser"]);

                if (String.IsNullOrWhiteSpace(actionBy))
                    actionBy = "Unknown User";

                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd =
                    new SqlCommand("dbo.usp_Dashboard", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Action", SqlDbType.VarChar, 20)
                        .Value = "LOG";

                    cmd.Parameters.Add("@ActivityType", SqlDbType.NVarChar, 100)
                        .Value = activityType;

                    cmd.Parameters.Add("@ActivityKey", SqlDbType.NVarChar, 50)
                        .Value = activityKey;

                    cmd.Parameters.Add("@ActivityName", SqlDbType.NVarChar, 200)
                        .Value = recordName;

                    cmd.Parameters.Add("@ActivityIcon", SqlDbType.NVarChar, 50)
                        .Value = activityIcon;

                    cmd.Parameters.Add("@ActionBy", SqlDbType.NVarChar, 100)
                        .Value = actionBy;

                    cmd.Parameters.Add("@PageName", SqlDbType.NVarChar, 100)
                        .Value = "DomainAndSSL";

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
                // Do not fail a successful save because logging failed.
                System.Diagnostics.Trace.TraceError(
                    "Dashboard activity logging failed: " + ex);
            }
        }

        // =========================
        // EXPIRY STATUS CSS
        // =========================
        protected string GetExpiryStatusCss(object value)
        {
            string status = Convert.ToString(value).ToUpperInvariant();

            if (status == "EXPIRED")
                return "status-expired";

            if (status.StartsWith("EXPIRING", StringComparison.Ordinal))
                return "status-expiring";

            return "status-valid";
        }

        // =========================
        // HELPER METHODS
        // =========================
        private static object DbValue(string value)
        {
            value = (value ?? String.Empty).Trim();

            return value.Length == 0
                ? (object)DBNull.Value
                : value;
        }

        private void ClearDomainForm()
        {
            hfDomainID.Value = String.Empty;
            txtDomainName.Text = String.Empty;
            txtRegistrar.Text = String.Empty;
            txtDNS.Text = String.Empty;
            txtDomainExpiry.Text = String.Empty;
        }

        private void ClearSslForm()
        {
            hfSslID.Value = String.Empty;
            txtSslProvider.Text = String.Empty;
            txtCertificateType.Text = String.Empty;
            txtSslExpiry.Text = String.Empty;
        }

        private void ShowMessage(string message, string tabId)
        {
            string safeMessage =
                HttpUtility.JavaScriptStringEncode(message ?? String.Empty);

            RegisterUiScript(
                "assetMessage",
                "selectAssetTab('" + tabId + "');" +
                "alert('" + safeMessage + "');");
        }

        private void RegisterUiScript(string key, string script)
        {
            ScriptManager.RegisterStartupScript(
                this, GetType(), key, script, true);
        }
    }
}
