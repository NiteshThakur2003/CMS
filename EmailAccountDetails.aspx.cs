using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CredentialManagementPortal
{
    public partial class EmailAccountDetails : Page
    {
        private readonly string conString = ConfigurationManager.ConnectionStrings["Conn1"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) LoadGrid();
        }

        private void LoadGrid()
        {
            using (SqlConnection con = new SqlConnection(conString))
            using (SqlCommand cmd = new SqlCommand("InsertEmailLoginDetails", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "SELECT");
                DataTable dt = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(dt);
                gvEmailAccount.DataSource = dt;
                gvEmailAccount.DataBind();
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd = new SqlCommand("InsertEmailLoginDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", String.IsNullOrWhiteSpace(hfID.Value) ? "INSERT" : "UPDATE");
                    if (!String.IsNullOrWhiteSpace(hfID.Value)) cmd.Parameters.AddWithValue("@ID", Convert.ToInt32(hfID.Value));
                    cmd.Parameters.AddWithValue("@Department", txtDepartment.Text.Trim());
                    cmd.Parameters.AddWithValue("@EmailID", txtEmailID.Text.Trim());
                    cmd.Parameters.AddWithValue("@Password", txtPassword.Text.Trim());
                    cmd.Parameters.AddWithValue("@RecoveryEmail", txtRecoveryEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@MFA", txtMFA.Text.Trim());
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
                ClearControls();
                LoadGrid();
                ScriptManager.RegisterStartupScript(this, GetType(), "saved", "var m=bootstrap.Modal.getInstance(document.getElementById('emailModal'));if(m)m.hide();alert('Record Saved Successfully.');", true);
            }
            catch (Exception ex) { ShowError(ex); }
        }

        protected void gvEmailAccount_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id;
            if (!Int32.TryParse(Convert.ToString(e.CommandArgument), out id)) return;
            if (e.CommandName == "EditRow") LoadRecord(id);
            else if (e.CommandName == "DeleteRow") { DeleteRecord(id); LoadGrid(); }
        }

        private void LoadRecord(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd = new SqlCommand("InsertEmailLoginDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "SELECTBYID");
                    cmd.Parameters.AddWithValue("@ID", id);
                    DataTable dt = new DataTable();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(dt);
                    if (dt.Rows.Count == 0) return;
                    DataRow row = dt.Rows[0];
                    hfID.Value = row["EmailLoginID"].ToString();
                    txtDepartment.Text = row["Department"].ToString();
                    txtEmailID.Text = row["EmailID"].ToString();
                    txtPassword.Attributes["value"] = row["Password"].ToString();
                    txtRecoveryEmail.Text = row["RecoveryEmail"].ToString();
                    txtMFA.Text = row["MFA"].ToString();
                    ScriptManager.RegisterStartupScript(this, GetType(), "openEmailModal", "openModal();", true);
                }
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private void DeleteRecord(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                using (SqlCommand cmd = new SqlCommand("InsertEmailLoginDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "DELETE");
                    cmd.Parameters.AddWithValue("@ID", id);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
                ScriptManager.RegisterStartupScript(this, GetType(), "deleted", "alert('Record Deleted Successfully.');", true);
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private void ClearControls()
        {
            hfID.Value = String.Empty;
            txtDepartment.Text = String.Empty;
            txtEmailID.Text = String.Empty;
            txtPassword.Text = String.Empty;
            txtRecoveryEmail.Text = String.Empty;
            txtMFA.Text = String.Empty;
        }

        private void ShowError(Exception ex)
        {
            string message = ex.Message.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\r", " ").Replace("\n", " ");
            ScriptManager.RegisterStartupScript(this, GetType(), "error", "alert('" + message + "');", true);
        }
    }
}



