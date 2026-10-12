
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CredentialManagementPortal
{
    public partial class VPNLoingDetails : Page
    {
        private readonly string conString =
            ConfigurationManager.ConnectionStrings["Conn1"].ConnectionString;

        // =========================================================
        // ADMIN CHECK
        // =========================================================
        private bool IsAdmin()
        {
            return String.Equals(
                Convert.ToString(Session["UserRole"]),
                "Admin",
                StringComparison.OrdinalIgnoreCase);
        }

        // =========================================================
        // PAGE LOAD
        // =========================================================
        protected void Page_Load(object sender, EventArgs e)
        {
            btnAdd.Visible = IsAdmin();
            btnSave.Visible = IsAdmin();

            if (!IsPostBack)
                LoadGrid();
        }

        // =========================================================
        // ADD / UPDATE VPN CREDENTIAL
        // =========================================================
        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!IsAdmin())
                return;

            int vpnID = 0;
            bool isUpdate = !String.IsNullOrWhiteSpace(hfID.Value);

            if (isUpdate && !Int32.TryParse(hfID.Value, out vpnID))
            {
                ShowMessage("Invalid VPN credential ID.");
                return;
            }

            string vpnName = txtVPNName.Text.Trim();

            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd =
                    new SqlCommand("usp_InsertVPNCredential", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue(
                        "@Action", isUpdate ? "UPDATE" : "INSERT");

                    if (isUpdate)
                        cmd.Parameters.AddWithValue("@VPNID", vpnID);

                    cmd.Parameters.AddWithValue("@VPNName", vpnName);
                    cmd.Parameters.AddWithValue("@URL", txtURL.Text.Trim());
                    cmd.Parameters.AddWithValue("@UserName", txtUserName.Text.Trim());
                    cmd.Parameters.AddWithValue("@Password", txtPassword.Text.Trim());
                    cmd.Parameters.AddWithValue("@MFA", txtMFA.Text.Trim());
                    cmd.Parameters.AddWithValue("@Owner", txtOwner.Text.Trim());
                    cmd.Parameters.AddWithValue("@Notes", txtNotes.Text.Trim());

                    con.Open();

                    object result = cmd.ExecuteScalar();

                    // Preserve the existing INSERT behavior.
                    if (!isUpdate && result != null && result != DBNull.Value)
                        vpnID = Convert.ToInt32(result);
                }

                // Log only after the save/update succeeds.
                LogDashboardActivity(
                    isUpdate ? "Updated" : "Added",
                    isUpdate && vpnID > 0 ? Convert.ToString(vpnID) :
                        (!isUpdate && vpnID > 0 ? Convert.ToString(vpnID) : null),
                    vpnName);

                ClearControls();
                LoadGrid();

                ScriptManager.RegisterStartupScript(
                    this,
                    GetType(),
                    "saved",
                    "var modal=bootstrap.Modal.getInstance(" +
                    "document.getElementById('vpnModal'));" +
                    "if(modal)modal.hide();" +
                    "alert('" +
                    (isUpdate
                        ? "VPN credential updated successfully."
                        : "VPN credential saved successfully.") +
                    "');",
                    true);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        // =========================================================
        // GRID ROW DATA BOUND
        // =========================================================
        protected void gvVPN_RowDataBound(
            object sender, GridViewRowEventArgs e)
        {
            if (!IsAdmin() && e.Row.Cells.Count > 0)
                e.Row.Cells[e.Row.Cells.Count - 1].Visible = false;
        }

        // =========================================================
        // GRID COMMAND
        // =========================================================
        protected void gvVPN_RowCommand(
            object sender, GridViewCommandEventArgs e)
        {
            if (!IsAdmin())
                return;

            int vpnID;

            if (!Int32.TryParse(Convert.ToString(e.CommandArgument), out vpnID))
                return;

            if (e.CommandName == "EditRow")
            {
                LoadRecord(vpnID);
            }
            else if (e.CommandName == "DeleteRow")
            {
                DeleteRecord(vpnID);
                LoadGrid();
            }
        }

        // =========================================================
        // LOAD VPN RECORD FOR EDIT
        // =========================================================
        private void LoadRecord(int vpnID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd =
                    new SqlCommand("usp_InsertVPNCredential", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "SELECTBYID");
                    cmd.Parameters.AddWithValue("@VPNID", vpnID);

                    DataTable record = new DataTable();

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        adapter.Fill(record);

                    if (record.Rows.Count == 0)
                        return;

                    DataRow row = record.Rows[0];

                    hfID.Value = Convert.ToString(row["VPNID"]);
                    txtVPNName.Text = Convert.ToString(row["VPNName"]);
                    txtURL.Text = Convert.ToString(row["URL"]);
                    txtUserName.Text = Convert.ToString(row["UserName"]);

                    // Existing behavior retained. Never log this value.
                    txtPassword.Attributes["value"] =
                        Convert.ToString(row["Password"]);

                    txtMFA.Text = Convert.ToString(row["MFA"]);
                    txtOwner.Text = Convert.ToString(row["Owner"]);
                    txtNotes.Text = Convert.ToString(row["Notes"]);

                    ScriptManager.RegisterStartupScript(
                        this, GetType(), "openVpnModal",
                        "openModal();", true);
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        // =========================================================
        // DELETE VPN CREDENTIAL
        // =========================================================
        private void DeleteRecord(int vpnID)
        {
            try
            {
                string vpnName = String.Empty;

                // Get the VPN name before deleting the record.
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd =
                    new SqlCommand("usp_InsertVPNCredential", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "SELECTBYID");
                    cmd.Parameters.AddWithValue("@VPNID", vpnID);

                    DataTable record = new DataTable();

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        adapter.Fill(record);

                    if (record.Rows.Count > 0)
                        vpnName = Convert.ToString(record.Rows[0]["VPNName"]);
                }

                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd =
                    new SqlCommand("usp_InsertVPNCredential", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "DELETE");
                    cmd.Parameters.AddWithValue("@VPNID", vpnID);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                // Log only after the delete succeeds.
                LogDashboardActivity(
                    "Deleted",
                    Convert.ToString(vpnID),
                    vpnName);

                ScriptManager.RegisterStartupScript(
                    this, GetType(), "deleted",
                    "alert('VPN credential deleted successfully.');", true);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        // =========================================================
        // DASHBOARD ACTIVITY LOGGER
        // Uses dbo.usp_Dashboard, @Action = LOG
        // =========================================================
        private void LogDashboardActivity(
            string actionName,
            string recordId,
            string vpnName)
        {
            try
            {
                string actionBy =
                    Convert.ToString(Session["AuthenticatedUser"]);

                if (String.IsNullOrWhiteSpace(actionBy))
                    actionBy = "System";

                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd =
                    new SqlCommand("dbo.usp_Dashboard", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Action", SqlDbType.VarChar, 20)
                        .Value = "LOG";

                    cmd.Parameters.Add("@ActivityType", SqlDbType.NVarChar, 100)
                        .Value = "VPN Credential " + actionName;

                    cmd.Parameters.Add("@ActivityKey", SqlDbType.NVarChar, 50)
                        .Value = "vpn";

                    cmd.Parameters.Add("@ActivityName", SqlDbType.NVarChar, 200)
                        .Value = "VPN: " + (vpnName ?? String.Empty);

                    cmd.Parameters.Add("@ActivityIcon", SqlDbType.NVarChar, 50)
                        .Value = "bi-shield-lock";

                    cmd.Parameters.Add("@ActionBy", SqlDbType.NVarChar, 100)
                        .Value = actionBy;

                    cmd.Parameters.Add("@PageName", SqlDbType.NVarChar, 100)
                        .Value = "VPNLoingDetails";

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
                // Logging failure should not undo the main operation.
                System.Diagnostics.Trace.TraceError(
                    "VPN dashboard activity logging failed: " + ex);
            }
        }

        // =========================================================
        // LOAD GRID
        // =========================================================
        private void LoadGrid()
        {
            using (SqlConnection con = new SqlConnection(conString))
            using (SqlCommand cmd =
                new SqlCommand("usp_InsertVPNCredential", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECT");

                DataTable records = new DataTable();

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    adapter.Fill(records);

                gvVPN.DataSource = records;
                gvVPN.DataBind();
            }
        }

        // =========================================================
        // CLEAR FORM
        // =========================================================
        private void ClearControls()
        {
            hfID.Value = String.Empty;
            txtVPNName.Text = String.Empty;
            txtURL.Text = String.Empty;
            txtUserName.Text = String.Empty;
            txtPassword.Text = String.Empty;
            txtMFA.Text = String.Empty;
            txtOwner.Text = String.Empty;
            txtNotes.Text = String.Empty;
        }

        // =========================================================
        // SHOW MESSAGE
        // =========================================================
        private void ShowMessage(string message)
        {
            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "vpnMessage",
                "alert('" +
                HttpUtility.JavaScriptStringEncode(message ?? String.Empty) +
                "');",
                true);
        }

        private void ShowError(Exception ex)
        {
            ShowMessage(ex.Message);
        }
    }
}
