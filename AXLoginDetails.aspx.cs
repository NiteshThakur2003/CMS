
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CredentialManagementPortal
{
    public partial class AXLoginDetails : System.Web.UI.Page
    {
        string conString =
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
            {
                LoadGrid();
            }
        }

        // Load AX records
        private void LoadGrid()
        {
            using (SqlConnection con = new SqlConnection(conString))
            using (SqlCommand cmd =
                new SqlCommand("usp_InsertAXLoginDetails", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECT");

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                da.Fill(dt);

                gvAXLogin.DataSource = dt;
                gvAXLogin.DataBind();
            }
        }

        // Insert / Update AX record
        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!IsAdmin())
                return;

            try
            {
                bool isNew = string.IsNullOrEmpty(hfID.Value);
                int id = 0;

                if (!isNew)
                    id = Convert.ToInt32(hfID.Value);

                string environmentName = txtEnvironment.Text.Trim();

                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd =
                    new SqlCommand("usp_InsertAXLoginDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (isNew)
                    {
                        cmd.Parameters.AddWithValue("@Action", "INSERT");
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@Action", "UPDATE");
                        cmd.Parameters.AddWithValue("@ID", id);
                    }

                    cmd.Parameters.AddWithValue(
                        "@Environment", environmentName);

                    cmd.Parameters.AddWithValue(
                        "@URL", txtURL.Text.Trim());

                    cmd.Parameters.AddWithValue(
                        "@UserID", txtUserid.Text.Trim());

                    cmd.Parameters.AddWithValue(
                        "@Password", txtPassword.Text.Trim());

                    cmd.Parameters.AddWithValue(
                        "@Role", txtRole.Text.Trim());

                    cmd.Parameters.AddWithValue(
                        "@Status", ddlStatus.SelectedValue);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                // Log only after the save procedure succeeds.
                string actionName = isNew ? "Added" : "Updated";

                string recordId = isNew
                    ? null
                    : id.ToString();

                LogDashboardActivity(
                    actionName,
                    recordId,
                    environmentName);

                ClearControls();
                LoadGrid();

                ScriptManager.RegisterStartupScript(
                    this,
                    GetType(),
                    "msg",
                    @"
                    var modalEl = document.getElementById('axModal');
                    var modal = bootstrap.Modal.getInstance(modalEl);
                    if (modal) {
                        modal.hide();
                    }
                    alert('Record Saved Successfully.');
                    ",
                    true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(
                    this,
                    GetType(),
                    "err",
                    "alert('" + ex.Message
                        .Replace("\\", "\\\\")
                        .Replace("'", "\\'")
                        .Replace("\r", " ")
                        .Replace("\n", " ")
                    + "');",
                    true);
            }
        }

        // Log activity through dbo.usp_Dashboard
        private void LogDashboardActivity(
            string actionName,
            string recordId,
            string recordName)
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
                        .Value = "AX Login " + actionName;

                    cmd.Parameters.Add("@ActivityKey", SqlDbType.NVarChar, 50)
                        .Value = "ax";

                    cmd.Parameters.Add("@ActivityName", SqlDbType.NVarChar, 200)
                        .Value = "AX Login: " + recordName;

                    cmd.Parameters.Add("@ActivityIcon", SqlDbType.NVarChar, 50)
                        .Value = "AX";

                    cmd.Parameters.Add("@ActionBy", SqlDbType.NVarChar, 100)
                        .Value = actionBy;

                    cmd.Parameters.Add("@PageName", SqlDbType.NVarChar, 100)
                        .Value = "AXLoginDetails";

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
                // Logging failure should not undo the successful AX operation.
                System.Diagnostics.Trace.TraceError(
                    "AX Dashboard logging error: " + ex);
            }
        }

        // Clear form
        private void ClearControls()
        {
            hfID.Value = "";

            txtEnvironment.Text = "";
            txtURL.Text = "";
            txtUserid.Text = "";
            txtPassword.Text = "";
            txtRole.Text = "";

            ddlStatus.SelectedIndex = 0;
        }

        // Grid row permissions
        protected void gvAXLogin_RowDataBound(
            object sender, GridViewRowEventArgs e)
        {
            if (!IsAdmin() && e.Row.Cells.Count > 0)
                e.Row.Cells[e.Row.Cells.Count - 1].Visible = false;
        }

        // Edit / Delete commands
        protected void gvAXLogin_RowCommand(
            object sender, GridViewCommandEventArgs e)
        {
            if (!IsAdmin())
                return;

            int id = Convert.ToInt32(e.CommandArgument);

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

        // Load selected AX record
        private void LoadRecord(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd =
                    new SqlCommand("usp_InsertAXLoginDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Action", "SELECTBYID");
                    cmd.Parameters.AddWithValue("@ID", id);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();

                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        hfID.Value =
                            dt.Rows[0]["AXLoginID"].ToString();

                        txtEnvironment.Text =
                            dt.Rows[0]["Environment"].ToString();

                        txtURL.Text =
                            dt.Rows[0]["URL"].ToString();

                        txtUserid.Text =
                            dt.Rows[0]["UserID"].ToString();

                        txtPassword.Attributes["value"] =
                            dt.Rows[0]["Password"].ToString();

                        txtRole.Text =
                            dt.Rows[0]["Role"].ToString();

                        ddlStatus.SelectedValue =
                            dt.Rows[0]["Status"].ToString();

                        OpenModal();
                    }
                }
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(
                    this,
                    GetType(),
                    "err",
                    "alert('" + ex.Message
                        .Replace("\\", "\\\\")
                        .Replace("'", "\\'")
                        .Replace("\r", " ")
                        .Replace("\n", " ")
                    + "');",
                    true);
            }
        }

        // Delete AX record
        private void DeleteRecord(int id)
        {
            try
            {
                // Read the name before deleting the record.
                string environmentName = "";

                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd =
                    new SqlCommand("usp_InsertAXLoginDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Action", "SELECTBYID");
                    cmd.Parameters.AddWithValue("@ID", id);

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            environmentName =
                                Convert.ToString(reader["Environment"]);
                    }
                }

                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd =
                    new SqlCommand("usp_InsertAXLoginDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Action", "DELETE");
                    cmd.Parameters.AddWithValue("@ID", id);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                // Log only after delete succeeds.
                LogDashboardActivity(
                    "Deleted",
                    id.ToString(),
                    environmentName);

                ScriptManager.RegisterStartupScript(
                    this,
                    GetType(),
                    "msg",
                    "alert('Record Deleted Successfully.');",
                    true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(
                    this,
                    GetType(),
                    "err",
                    "alert('" + ex.Message
                        .Replace("\\", "\\\\")
                        .Replace("'", "\\'")
                        .Replace("\r", " ")
                        .Replace("\n", " ")
                    + "');",
                    true);
            }
        }

        // Open edit modal
        private void OpenModal()
        {
            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "open",
                "openModal();",
                true);
        }
    }
}
