
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CredentialManagementPortal
{
    public partial class EmailAccountDetails : Page
    {
        private readonly string conString =
            ConfigurationManager.ConnectionStrings["Conn1"].ConnectionString;

        private bool IsAdmin()
        {
            return String.Equals(
                Convert.ToString(Session["UserRole"]),
                "Admin",
                StringComparison.OrdinalIgnoreCase);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            btnAdd.Visible = IsAdmin();
            btnSave.Visible = IsAdmin();

            if (!IsPostBack)
                LoadGrid();
        }

        private void LoadGrid()
        {
            using (SqlConnection con = new SqlConnection(conString))
            using (SqlCommand cmd =
                new SqlCommand("InsertEmailLoginDetails", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECT");

                DataTable dt = new DataTable();

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    da.Fill(dt);

                gvEmailAccount.DataSource = dt;
                gvEmailAccount.DataBind();
            }
        }

        // =========================================
        // ADD / UPDATE EMAIL ACCOUNT
        // =========================================
        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!IsAdmin()) return;

            int id = 0;
            bool isUpdate = Int32.TryParse(hfID.Value, out id);

            string department = txtDepartment.Text.Trim();
            string emailID = txtEmailID.Text.Trim();

            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd =
                    new SqlCommand("InsertEmailLoginDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue(
                        "@Action", isUpdate ? "UPDATE" : "INSERT");

                    if (isUpdate)
                        cmd.Parameters.AddWithValue("@ID", id);

                    cmd.Parameters.AddWithValue("@Department", department);
                    cmd.Parameters.AddWithValue("@EmailID", emailID);
                    cmd.Parameters.AddWithValue("@Password", txtPassword.Text.Trim());
                    cmd.Parameters.AddWithValue("@RecoveryEmail", txtRecoveryEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@MFA", txtMFA.Text.Trim());

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                // Log activity only after successful save
                LogDashboardActivity(
                    isUpdate ? "Updated" : "Added",
                    isUpdate ? Convert.ToString(id) : null,
                    emailID);

                ClearControls();
                LoadGrid();

                ScriptManager.RegisterStartupScript(
                    this,
                    GetType(),
                    "saved",
                    "var m=bootstrap.Modal.getInstance(document.getElementById('emailModal'));" +
                    "if(m)m.hide();" +
                    "alert('Record " +
                    (isUpdate ? "Updated" : "Saved") +
                    " Successfully.');",
                    true);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        // =========================================
        // GRID ROW BINDING
        // =========================================
        protected void gvEmailAccount_RowDataBound(
            object sender, GridViewRowEventArgs e)
        {
            if (!IsAdmin() && e.Row.Cells.Count > 0)
                e.Row.Cells[e.Row.Cells.Count - 1].Visible = false;
        }

        // =========================================
        // EDIT / DELETE
        // =========================================
        protected void gvEmailAccount_RowCommand(
            object sender, GridViewCommandEventArgs e)
        {
            if (!IsAdmin()) return;

            int id;

            if (!Int32.TryParse(Convert.ToString(e.CommandArgument), out id))
                return;

            if (e.CommandName == "EditRow")
            {
                LoadRecord(id);
            }
            else if (e.CommandName == "DeleteRow")
            {
                DeleteRecord(id);
                LoadGrid();
            }
        }

        // =========================================
        // LOAD EMAIL ACCOUNT FOR EDIT
        // =========================================
        private void LoadRecord(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd =
                    new SqlCommand("InsertEmailLoginDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "SELECTBYID");
                    cmd.Parameters.AddWithValue("@ID", id);

                    DataTable dt = new DataTable();

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        da.Fill(dt);

                    if (dt.Rows.Count == 0) return;

                    DataRow row = dt.Rows[0];

                    hfID.Value = Convert.ToString(row["EmailLoginID"]);
                    txtDepartment.Text = Convert.ToString(row["Department"]);
                    txtEmailID.Text = Convert.ToString(row["EmailID"]);

                    txtPassword.Attributes["value"] =
                        Convert.ToString(row["Password"]);

                    txtRecoveryEmail.Text =
                        Convert.ToString(row["RecoveryEmail"]);

                    txtMFA.Text = Convert.ToString(row["MFA"]);

                    ScriptManager.RegisterStartupScript(
                        this, GetType(), "openEmailModal",
                        "openModal();", true);
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        // =========================================
        // DELETE EMAIL ACCOUNT
        // =========================================
        private void DeleteRecord(int id)
        {
            try
            {
                string emailID = String.Empty;

                // Get email ID before deletion for the activity log
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd =
                    new SqlCommand("InsertEmailLoginDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "SELECTBYID");
                    cmd.Parameters.AddWithValue("@ID", id);

                    DataTable dt = new DataTable();

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        da.Fill(dt);

                    if (dt.Rows.Count == 0)
                        return;

                    emailID = Convert.ToString(dt.Rows[0]["EmailID"]);
                }

                // Delete the record
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd =
                    new SqlCommand("InsertEmailLoginDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "DELETE");
                    cmd.Parameters.AddWithValue("@ID", id);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                // Log deletion after successful delete
                LogDashboardActivity(
                    "Deleted",
                    Convert.ToString(id),
                    emailID);

                ScriptManager.RegisterStartupScript(
                    this, GetType(), "deleted",
                    "alert('Record Deleted Successfully.');", true);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        // =========================================
        // INLINE DASHBOARD ACTIVITY LOGGER
        // =========================================
        private void LogDashboardActivity(
            string actionName,
            string recordId,
            string emailID)
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
                        .Value = "Email Account " + actionName;

                    cmd.Parameters.Add("@ActivityKey", SqlDbType.NVarChar, 50)
                        .Value = "email";

                    cmd.Parameters.Add("@ActivityName", SqlDbType.NVarChar, 200)
                        .Value = "Email Account: " + emailID;

                    cmd.Parameters.Add("@ActivityIcon", SqlDbType.NVarChar, 50)
                        .Value = "Email";

                    cmd.Parameters.Add("@ActionBy", SqlDbType.NVarChar, 100)
                        .Value = actionBy;

                    cmd.Parameters.Add("@PageName", SqlDbType.NVarChar, 100)
                        .Value = "EmailAccountDetails";

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
                // Log the error without interrupting the main operation
                System.Diagnostics.Trace.TraceError(
                    "Email account dashboard activity logging failed: " + ex);
            }
        }

        // =========================================
        // CLEAR FORM
        // =========================================
        private void ClearControls()
        {
            hfID.Value = String.Empty;
            txtDepartment.Text = String.Empty;
            txtEmailID.Text = String.Empty;
            txtPassword.Text = String.Empty;
            txtRecoveryEmail.Text = String.Empty;
            txtMFA.Text = String.Empty;
        }

        // =========================================
        // ERROR MESSAGE
        // =========================================
        private void ShowError(Exception ex)
        {
            string message = HttpUtility.JavaScriptStringEncode(
                ex.Message ?? String.Empty);

            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "error",
                "alert('" + message + "');",
                true);
        }
    }
}
