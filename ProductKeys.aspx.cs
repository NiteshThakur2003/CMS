
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CredentialManagementPortal
{
    public partial class ProductKeys : Page
    {
        private readonly string conString =
            ConfigurationManager.ConnectionStrings["Conn1"].ConnectionString;

        private const string ExistingKeySessionName = "ProductKeys.EditingKey";

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

        // =========================================
        // LOAD PRODUCT KEYS
        // =========================================
        private void LoadGrid()
        {
            using (SqlConnection con = new SqlConnection(conString))
            using (SqlCommand cmd =
                new SqlCommand("dbo.InsertProductKeys", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20)
                    .Value = "SELECT";

                DataTable records = new DataTable();

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(records);
                }

                gvProductKeys.DataSource = records;
                gvProductKeys.DataBind();
            }
        }

        // =========================================
        // ADD / UPDATE PRODUCT KEY
        // =========================================
        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!IsAdmin())
            {
                ShowMessage("You are not authorized to save product keys.");
                return;
            }

            int id;
            bool isUpdate = Int32.TryParse(hfID.Value, out id);

            string softwareName = txtSoftware.Text.Trim();
            string productKey = txtProductKey.Text.Trim();

            if (String.IsNullOrWhiteSpace(softwareName))
            {
                ShowMessage("Enter the software name.");
                return;
            }

            if (!isUpdate && String.IsNullOrWhiteSpace(productKey))
            {
                ShowMessage("Enter the product key.");
                return;
            }

            // Keep existing key if the edit field is left blank.
            if (isUpdate && String.IsNullOrWhiteSpace(productKey))
            {
                productKey = Convert.ToString(
                    Session[ExistingKeySessionName]);
            }

            if (isUpdate && String.IsNullOrWhiteSpace(productKey))
            {
                ShowMessage(
                    "The existing product key could not be loaded. " +
                    "Close the dialog and try again.");
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd =
                    new SqlCommand("dbo.InsertProductKeys", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20)
                        .Value = isUpdate ? "UPDATE" : "INSERT";

                    if (isUpdate)
                    {
                        cmd.Parameters.Add("@ID", SqlDbType.Int)
                            .Value = id;
                    }

                    cmd.Parameters.Add("@Software", SqlDbType.NVarChar, 200)
                        .Value = softwareName;

                    cmd.Parameters.Add("@Version", SqlDbType.NVarChar, 100)
                        .Value = DbValue(txtVersion.Text);

                    cmd.Parameters.Add("@ProductKey", SqlDbType.NVarChar, 500)
                        .Value = productKey;

                    cmd.Parameters.Add("@LicenseType", SqlDbType.NVarChar, 100)
                        .Value = DbValue(txtLicenseType.Text);

                    cmd.Parameters.Add("@AssignedTo", SqlDbType.NVarChar, 150)
                        .Value = DbValue(txtAssignedTo.Text);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                // Log only after successful INSERT / UPDATE.
                LogDashboardActivity(
                    isUpdate ? "Updated" : "Added",
                    isUpdate ? Convert.ToString(id) : null,
                    softwareName);

                ClearControls();
                LoadGrid();

                ScriptManager.RegisterStartupScript(
                    this,
                    GetType(),
                    "productKeySaved",
                    "var el=document.getElementById('productKeyModal');" +
                    "var m=bootstrap.Modal.getInstance(el);" +
                    "if(m)m.hide();" +
                    "alert('Product key " +
                    (isUpdate ? "updated" : "saved") +
                    " successfully.');",
                    true);
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message);
            }
        }

        // =========================================
        // GRID ROW BINDING
        // =========================================
        protected void gvProductKeys_RowDataBound(
            object sender, GridViewRowEventArgs e)
        {
            if (!IsAdmin() && e.Row.Cells.Count > 0)
            {
                e.Row.Cells[e.Row.Cells.Count - 1].Visible = false;
            }
        }

        // =========================================
        // GRID EDIT / DELETE
        // =========================================
        protected void gvProductKeys_RowCommand(
            object sender, GridViewCommandEventArgs e)
        {
            if (!IsAdmin())
            {
                return;
            }

            int id;

            if (!Int32.TryParse(
                Convert.ToString(e.CommandArgument), out id))
            {
                return;
            }

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
        // LOAD RECORD FOR EDIT
        // =========================================
        private void LoadRecord(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd =
                    new SqlCommand("dbo.InsertProductKeys", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20)
                        .Value = "SELECTBYID";

                    cmd.Parameters.Add("@ID", SqlDbType.Int)
                        .Value = id;

                    DataTable records = new DataTable();

                    using (SqlDataAdapter adapter =
                        new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(records);
                    }

                    if (records.Rows.Count == 0)
                    {
                        ShowMessage("Product key record was not found.");
                        return;
                    }

                    DataRow row = records.Rows[0];

                    hfID.Value = Convert.ToString(row["ProductKeyID"]);
                    txtSoftware.Text = Convert.ToString(row["Software"]);
                    txtVersion.Text = Convert.ToString(row["Version"]);
                    txtLicenseType.Text =
                        Convert.ToString(row["LicenseType"]);
                    txtAssignedTo.Text =
                        Convert.ToString(row["AssignedTo"]);

                    // Do not display the existing secret in the edit box.
                    txtProductKey.Text = String.Empty;

                    Session[ExistingKeySessionName] =
                        Convert.ToString(row["ProductKey"]);

                    ScriptManager.RegisterStartupScript(
                        this,
                        GetType(),
                        "openProductKeyModal",
                        "openProductKeyModal();",
                        true);
                }
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message);
            }
        }

        // =========================================
        // DELETE PRODUCT KEY
        // =========================================
        private void DeleteRecord(int id)
        {
            try
            {
                string softwareName = String.Empty;

                // Get software name before deletion.
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd =
                    new SqlCommand("dbo.InsertProductKeys", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20)
                        .Value = "SELECTBYID";

                    cmd.Parameters.Add("@ID", SqlDbType.Int)
                        .Value = id;

                    DataTable dt = new DataTable();

                    using (SqlDataAdapter adapter =
                        new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }

                    if (dt.Rows.Count == 0)
                    {
                        ShowMessage("Product key record was not found.");
                        return;
                    }

                    softwareName = Convert.ToString(
                        dt.Rows[0]["Software"]);
                }

                // Delete the record.
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd =
                    new SqlCommand("dbo.InsertProductKeys", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20)
                        .Value = "DELETE";

                    cmd.Parameters.Add("@ID", SqlDbType.Int)
                        .Value = id;

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                // Log after successful DELETE.
                LogDashboardActivity(
                    "Deleted",
                    id.ToString(),
                    softwareName);
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message);
            }
        }

        // =========================================
        // DASHBOARD ACTIVITY LOGGER
        // Uses existing dbo.usp_Dashboard
        // =========================================
        private void LogDashboardActivity(
            string actionName,
            string recordId,
            string softwareName)
        {
            try
            {
                string actionBy =
                    Convert.ToString(Session["AuthenticatedUser"]);

                if (String.IsNullOrWhiteSpace(actionBy))
                {
                    actionBy = "Unknown User";
                }

                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd =
                    new SqlCommand("dbo.usp_Dashboard", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Action", SqlDbType.VarChar, 20)
                        .Value = "LOG";

                    cmd.Parameters.Add("@ActivityType", SqlDbType.NVarChar, 100)
                        .Value = "Product Key " + actionName;

                    cmd.Parameters.Add("@ActivityKey", SqlDbType.NVarChar, 50)
                        .Value = "product";

                    cmd.Parameters.Add("@ActivityName", SqlDbType.NVarChar, 200)
                        .Value = "Product Key: " +
                            (softwareName ?? String.Empty);

                    cmd.Parameters.Add("@ActivityIcon", SqlDbType.NVarChar, 50)
                        .Value = "bi-key";

                    cmd.Parameters.Add("@ActionBy", SqlDbType.NVarChar, 100)
                        .Value = actionBy;

                    cmd.Parameters.Add("@PageName", SqlDbType.NVarChar, 100)
                        .Value = "ProductKeys";

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
                // Activity logging failure must not hide a successful save.
                System.Diagnostics.Trace.TraceError(
                    "Product key dashboard activity logging failed: " + ex);
            }
        }

        // =========================================
        // DATABASE NULL HELPER
        // =========================================
        private static object DbValue(string value)
        {
            value = (value ?? String.Empty).Trim();

            return value.Length == 0
                ? (object)DBNull.Value
                : value;
        }

        // =========================================
        // CLEAR FORM
        // =========================================
        private void ClearControls()
        {
            hfID.Value = String.Empty;
            txtSoftware.Text = String.Empty;
            txtVersion.Text = String.Empty;
            txtProductKey.Text = String.Empty;
            txtLicenseType.Text = String.Empty;
            txtAssignedTo.Text = String.Empty;

            Session.Remove(ExistingKeySessionName);
        }

        // =========================================
        // SHOW MESSAGE
        // =========================================
        private void ShowMessage(string message)
        {
            string safeMessage =
                HttpUtility.JavaScriptStringEncode(
                    message ?? String.Empty);

            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "productKeyMessage",
                "alert('" + safeMessage + "');",
                true);
        }
    }
}
