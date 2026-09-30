using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CredentialManagementPortal
{
    public partial class DomainAndSSL : Page
    {
        private readonly string conString = ConfigurationManager.ConnectionStrings["Conn1"].ConnectionString;
        private const string DomainsTab = "domains-tab";
        private const string SslTab = "ssl-tab";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                hfActiveTab.Value = String.Equals(Request.QueryString["tab"], "ssl", StringComparison.OrdinalIgnoreCase) ? SslTab : DomainsTab;
            }
            if (!IsPostBack)
            {
                LoadDomains();
                LoadSslCertificates();
            }
        }

        private void LoadDomains()
        {
            gvDomains.DataSource = GetRecords("dbo.InsertDomainDetails", "SELECT");
            gvDomains.DataBind();
        }

        private void LoadSslCertificates()
        {
            gvSsl.DataSource = GetRecords("dbo.InsertSSLDetails", "SELECT");
            gvSsl.DataBind();
        }

        private DataTable GetRecords(string procedure, string action, int? id = null)
        {
            using (SqlConnection con = new SqlConnection(conString))
            using (SqlCommand cmd = new SqlCommand(procedure, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = action;
                if (id.HasValue) cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id.Value;
                DataTable data = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd)) adapter.Fill(data);
                return data;
            }
        }

        protected void btnSaveDomain_Click(object sender, EventArgs e)
        {
            int id;
            bool isUpdate = Int32.TryParse(hfDomainID.Value, out id);
            DateTime expiry;
            if (String.IsNullOrWhiteSpace(txtDomainName.Text))
            {
                ShowMessage("Enter a domain name.", DomainsTab);
                return;
            }
            if (!DateTime.TryParseExact(txtDomainExpiry.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out expiry))
            {
                ShowMessage("Select a valid domain expiry date.", DomainsTab);
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd = new SqlCommand("dbo.InsertDomainDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = isUpdate ? "UPDATE" : "INSERT";
                    if (isUpdate) cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                    cmd.Parameters.Add("@DomainName", SqlDbType.NVarChar, 255).Value = txtDomainName.Text.Trim();
                    cmd.Parameters.Add("@Registrar", SqlDbType.NVarChar, 150).Value = DbValue(txtRegistrar.Text);
                    cmd.Parameters.Add("@Expiry", SqlDbType.Date).Value = expiry.Date;
                    cmd.Parameters.Add("@DNS", SqlDbType.NVarChar, 500).Value = DbValue(txtDNS.Text);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
                ClearDomainForm();
                LoadDomains();
                RegisterUiScript("domainSaved", "var m=bootstrap.Modal.getInstance(document.getElementById('domainModal'));if(m)m.hide();selectAssetTab('domains-tab');alert('Domain " + (isUpdate ? "updated" : "saved") + " successfully.');");
            }
            catch (Exception ex) { ShowMessage(ex.Message, DomainsTab); }
        }

        protected void btnSaveSsl_Click(object sender, EventArgs e)
        {
            int id;
            bool isUpdate = Int32.TryParse(hfSslID.Value, out id);
            DateTime expiry;
            if (String.IsNullOrWhiteSpace(txtSslProvider.Text))
            {
                ShowMessage("Enter an SSL provider.", SslTab);
                return;
            }
            if (!DateTime.TryParseExact(txtSslExpiry.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out expiry))
            {
                ShowMessage("Select a valid certificate expiry date.", SslTab);
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd = new SqlCommand("dbo.InsertSSLDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = isUpdate ? "UPDATE" : "INSERT";
                    if (isUpdate) cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                    cmd.Parameters.Add("@Provider", SqlDbType.NVarChar, 150).Value = txtSslProvider.Text.Trim();
                    cmd.Parameters.Add("@Expiry", SqlDbType.Date).Value = expiry.Date;
                    cmd.Parameters.Add("@CertificateType", SqlDbType.NVarChar, 100).Value = DbValue(txtCertificateType.Text);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
                ClearSslForm();
                LoadSslCertificates();
                RegisterUiScript("sslSaved", "var m=bootstrap.Modal.getInstance(document.getElementById('sslModal'));if(m)m.hide();selectAssetTab('ssl-tab');alert('SSL certificate " + (isUpdate ? "updated" : "saved") + " successfully.');");
            }
            catch (Exception ex) { ShowMessage(ex.Message, SslTab); }
        }

        protected void gvDomains_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id;
            if (!Int32.TryParse(Convert.ToString(e.CommandArgument), out id)) return;
            if (e.CommandName == "EditRow") LoadDomain(id);
            else if (e.CommandName == "DeleteRow")
            {
                ExecuteDelete("dbo.InsertDomainDetails", id);
                LoadDomains();
                RegisterUiScript("domainTab", "selectAssetTab('domains-tab');");
            }
        }

        protected void gvSsl_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id;
            if (!Int32.TryParse(Convert.ToString(e.CommandArgument), out id)) return;
            if (e.CommandName == "EditRow") LoadSsl(id);
            else if (e.CommandName == "DeleteRow")
            {
                ExecuteDelete("dbo.InsertSSLDetails", id);
                LoadSslCertificates();
                RegisterUiScript("sslTab", "selectAssetTab('ssl-tab');");
            }
        }

        private void LoadDomain(int id)
        {
            try
            {
                DataTable rows = GetRecords("dbo.InsertDomainDetails", "SELECTBYID", id);
                if (rows.Rows.Count == 0) return;
                DataRow row = rows.Rows[0];
                hfDomainID.Value = Convert.ToString(row["DomainID"]);
                txtDomainName.Text = Convert.ToString(row["DomainName"]);
                txtRegistrar.Text = Convert.ToString(row["Registrar"]);
                txtDNS.Text = Convert.ToString(row["DNS"]);
                txtDomainExpiry.Text = Convert.ToDateTime(row["Expiry"]).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                RegisterUiScript("editDomain", "selectAssetTab('domains-tab');openAssetModal('domainModal');");
            }
            catch (Exception ex) { ShowMessage(ex.Message, DomainsTab); }
        }

        private void LoadSsl(int id)
        {
            try
            {
                DataTable rows = GetRecords("dbo.InsertSSLDetails", "SELECTBYID", id);
                if (rows.Rows.Count == 0) return;
                DataRow row = rows.Rows[0];
                hfSslID.Value = Convert.ToString(row["SSLID"]);
                txtSslProvider.Text = Convert.ToString(row["Provider"]);
                txtCertificateType.Text = Convert.ToString(row["CertificateType"]);
                txtSslExpiry.Text = Convert.ToDateTime(row["Expiry"]).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                RegisterUiScript("editSsl", "selectAssetTab('ssl-tab');openAssetModal('sslModal');");
            }
            catch (Exception ex) { ShowMessage(ex.Message, SslTab); }
        }

        private void ExecuteDelete(string procedure, int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd = new SqlCommand(procedure, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "DELETE";
                    cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex) { ShowMessage(ex.Message, procedure == "dbo.InsertDomainDetails" ? DomainsTab : SslTab); }
        }

        protected string GetExpiryStatusCss(object value)
        {
            string status = Convert.ToString(value).ToUpperInvariant();
            if (status == "EXPIRED") return "status-expired";
            if (status.StartsWith("EXPIRING", StringComparison.Ordinal)) return "status-expiring";
            return "status-valid";
        }

        private static object DbValue(string value)
        {
            value = (value ?? String.Empty).Trim();
            return value.Length == 0 ? (object)DBNull.Value : value;
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
            string safeMessage = HttpUtility.JavaScriptStringEncode(message ?? String.Empty);
            RegisterUiScript("assetMessage", "selectAssetTab('" + tabId + "');alert('" + safeMessage + "');");
        }

        private void RegisterUiScript(string key, string script)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), key, script, true);
        }
    }
}

