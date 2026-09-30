using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CredentialManagementPortal
{
    public partial class VPNLoingDetails : Page
    {
        private readonly string conString = ConfigurationManager.ConnectionStrings["Conn1"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) LoadGrid();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                bool isUpdate = !String.IsNullOrWhiteSpace(hfID.Value);
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd = new SqlCommand("usp_InsertVPNCredential", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", isUpdate ? "UPDATE" : "INSERT");
                    if (isUpdate) cmd.Parameters.AddWithValue("@VPNID", Convert.ToInt32(hfID.Value));
                    cmd.Parameters.AddWithValue("@VPNName", txtVPNName.Text.Trim());
                    cmd.Parameters.AddWithValue("@URL", txtURL.Text.Trim());
                    cmd.Parameters.AddWithValue("@UserName", txtUserName.Text.Trim());
                    cmd.Parameters.AddWithValue("@Password", txtPassword.Text.Trim());
                    cmd.Parameters.AddWithValue("@MFA", txtMFA.Text.Trim());
                    cmd.Parameters.AddWithValue("@Owner", txtOwner.Text.Trim());
                    cmd.Parameters.AddWithValue("@Notes", txtNotes.Text.Trim());

                    con.Open();
                    object result = cmd.ExecuteScalar();
                    if (!isUpdate && result != null && result != DBNull.Value)
                        hfID.Value = Convert.ToString(result);
                }

                ClearControls();
                LoadGrid();
                ScriptManager.RegisterStartupScript(this, GetType(), "saved",
                    "var modal=bootstrap.Modal.getInstance(document.getElementById('vpnModal'));if(modal)modal.hide();alert('VPN credential " + (isUpdate ? "updated" : "saved") + " successfully.');", true);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        protected void gvVPN_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int vpnID;
            if (!Int32.TryParse(Convert.ToString(e.CommandArgument), out vpnID)) return;

            if (e.CommandName == "EditRow")
                LoadRecord(vpnID);
            else if (e.CommandName == "DeleteRow")
            {
                DeleteRecord(vpnID);
                LoadGrid();
            }
        }

        private void LoadRecord(int vpnID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd = new SqlCommand("usp_InsertVPNCredential", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "SELECTBYID");
                    cmd.Parameters.AddWithValue("@VPNID", vpnID);

                    DataTable record = new DataTable();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd)) adapter.Fill(record);
                    if (record.Rows.Count == 0) return;

                    DataRow row = record.Rows[0];
                    hfID.Value = row["VPNID"].ToString();
                    txtVPNName.Text = row["VPNName"].ToString();
                    txtURL.Text = row["URL"].ToString();
                    txtUserName.Text = row["UserName"].ToString();
                    txtPassword.Attributes["value"] = row["Password"].ToString();
                    txtMFA.Text = row["MFA"].ToString();
                    txtOwner.Text = row["Owner"].ToString();
                    txtNotes.Text = row["Notes"].ToString();

                    ScriptManager.RegisterStartupScript(this, GetType(), "openVpnModal", "openModal();", true);
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void DeleteRecord(int vpnID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd = new SqlCommand("usp_InsertVPNCredential", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "DELETE");
                    cmd.Parameters.AddWithValue("@VPNID", vpnID);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
                ScriptManager.RegisterStartupScript(this, GetType(), "deleted", "alert('VPN credential deleted successfully.');", true);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void LoadGrid()
        {
            using (SqlConnection con = new SqlConnection(conString))
            using (SqlCommand cmd = new SqlCommand("usp_InsertVPNCredential", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECT");

                DataTable records = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd)) adapter.Fill(records);
                gvVPN.DataSource = records;
                gvVPN.DataBind();
            }
        }

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

        private void ShowError(Exception ex)
        {
            string message = ex.Message.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\r", " ").Replace("\n", " ");
            ScriptManager.RegisterStartupScript(this, GetType(), "vpnError", "alert('" + message + "');", true);
        }
    }
}
