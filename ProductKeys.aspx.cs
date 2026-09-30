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
        private readonly string conString = ConfigurationManager.ConnectionStrings["Conn1"].ConnectionString;
        private const string ExistingKeySessionName = "ProductKeys.EditingKey";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) LoadGrid();
        }

        private void LoadGrid()
        {
            using (SqlConnection con = new SqlConnection(conString))
            using (SqlCommand cmd = new SqlCommand("dbo.InsertProductKeys", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "SELECT";
                DataTable records = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd)) adapter.Fill(records);
                gvProductKeys.DataSource = records;
                gvProductKeys.DataBind();
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            bool isUpdate = !String.IsNullOrWhiteSpace(hfID.Value);
            string productKey = txtProductKey.Text.Trim();
            if (String.IsNullOrWhiteSpace(txtSoftware.Text))
            {
                ShowMessage("Enter the software name.");
                return;
            }
            if (!isUpdate && String.IsNullOrWhiteSpace(productKey))
            {
                ShowMessage("Enter the product key.");
                return;
            }
            if (isUpdate && String.IsNullOrWhiteSpace(productKey))
                productKey = Convert.ToString(Session[ExistingKeySessionName]);
            if (isUpdate && String.IsNullOrWhiteSpace(productKey))
            {
                ShowMessage("The existing product key could not be loaded. Close the dialog and try again.");
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd = new SqlCommand("dbo.InsertProductKeys", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = isUpdate ? "UPDATE" : "INSERT";
                    if (isUpdate) cmd.Parameters.Add("@ID", SqlDbType.Int).Value = Convert.ToInt32(hfID.Value);
                    cmd.Parameters.Add("@Software", SqlDbType.NVarChar, 200).Value = txtSoftware.Text.Trim();
                    cmd.Parameters.Add("@Version", SqlDbType.NVarChar, 100).Value = DbValue(txtVersion.Text);
                    cmd.Parameters.Add("@ProductKey", SqlDbType.NVarChar, 500).Value = productKey;
                    cmd.Parameters.Add("@LicenseType", SqlDbType.NVarChar, 100).Value = DbValue(txtLicenseType.Text);
                    cmd.Parameters.Add("@AssignedTo", SqlDbType.NVarChar, 150).Value = DbValue(txtAssignedTo.Text);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                ClearControls();
                LoadGrid();
                ScriptManager.RegisterStartupScript(this, GetType(), "productKeySaved",
                    "var el=document.getElementById('productKeyModal');var m=bootstrap.Modal.getInstance(el);if(m)m.hide();alert('Product key " + (isUpdate ? "updated" : "saved") + " successfully.');", true);
            }
            catch (Exception ex) { ShowMessage(ex.Message); }
        }

        protected void gvProductKeys_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id;
            if (!Int32.TryParse(Convert.ToString(e.CommandArgument), out id)) return;
            if (e.CommandName == "EditRow") LoadRecord(id);
            else if (e.CommandName == "DeleteRow")
            {
                DeleteRecord(id);
                LoadGrid();
            }
        }

        private void LoadRecord(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd = new SqlCommand("dbo.InsertProductKeys", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "SELECTBYID";
                    cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                    DataTable records = new DataTable();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd)) adapter.Fill(records);
                    if (records.Rows.Count == 0) return;

                    DataRow row = records.Rows[0];
                    hfID.Value = Convert.ToString(row["ProductKeyID"]);
                    txtSoftware.Text = Convert.ToString(row["Software"]);
                    txtVersion.Text = Convert.ToString(row["Version"]);
                    txtLicenseType.Text = Convert.ToString(row["LicenseType"]);
                    txtAssignedTo.Text = Convert.ToString(row["AssignedTo"]);
                    txtProductKey.Text = String.Empty;
                    Session[ExistingKeySessionName] = Convert.ToString(row["ProductKey"]);
                    ScriptManager.RegisterStartupScript(this, GetType(), "openProductKeyModal", "openProductKeyModal();", true);
                }
            }
            catch (Exception ex) { ShowMessage(ex.Message); }
        }

        private void DeleteRecord(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd = new SqlCommand("dbo.InsertProductKeys", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 20).Value = "DELETE";
                    cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex) { ShowMessage(ex.Message); }
        }

        private static object DbValue(string value)
        {
            value = value.Trim();
            return value.Length == 0 ? (object)DBNull.Value : value;
        }

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

        private void ShowMessage(string message)
        {
            string safeMessage = HttpUtility.JavaScriptStringEncode(message ?? String.Empty);
            ScriptManager.RegisterStartupScript(this, GetType(), "productKeyMessage", "alert('" + safeMessage + "');", true);
        }
    }
}
