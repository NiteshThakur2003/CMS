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
        string conString = ConfigurationManager.ConnectionStrings["Conn1"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadGrid();
            }
        }

        private void LoadGrid()
        {
            using (SqlConnection con = new SqlConnection(conString))
            {
                using (SqlCommand cmd = new SqlCommand("usp_InsertAXLoginDetails", con))
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
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                {
                    using (SqlCommand cmd = new SqlCommand("usp_InsertAXLoginDetails", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        if (string.IsNullOrEmpty(hfID.Value))
                        {
                            cmd.Parameters.AddWithValue("@Action", "INSERT");
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@Action", "UPDATE");
                            cmd.Parameters.AddWithValue("@ID", Convert.ToInt32(hfID.Value));
                        }

                        cmd.Parameters.AddWithValue("@Environment", txtEnvironment.Text.Trim());
                        cmd.Parameters.AddWithValue("@URL", txtURL.Text.Trim());
                        cmd.Parameters.AddWithValue("@UserID", txtUserid.Text.Trim());
                        cmd.Parameters.AddWithValue("@Password", txtPassword.Text.Trim());
                        cmd.Parameters.AddWithValue("@Role", txtRole.Text.Trim());
                        cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);

                        con.Open();

                        cmd.ExecuteNonQuery();

                        con.Close();
                    }
                }

                ClearControls();

                LoadGrid();

                ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "msg",
                @"
                var modalEl = document.getElementById('axModal');
                var modal = bootstrap.Modal.getInstance(modalEl);
                if(modal){
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
                    "alert('" + ex.Message.Replace("'", "") + "');",
                    true);
            }
        }

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
        protected void gvAXLogin_RowCommand(object sender, GridViewCommandEventArgs e)
        {
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

        private void LoadRecord(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                {
                    using (SqlCommand cmd = new SqlCommand("usp_InsertAXLoginDetails", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Action", "SELECTBYID");
                        cmd.Parameters.AddWithValue("@ID", id);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);

                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            hfID.Value = dt.Rows[0]["AXLoginID"].ToString();

                            txtEnvironment.Text = dt.Rows[0]["Environment"].ToString();
                            txtURL.Text = dt.Rows[0]["URL"].ToString();
                            txtUserid.Text = dt.Rows[0]["UserID"].ToString();
                            txtPassword.Attributes["value"] = dt.Rows[0]["Password"].ToString();
                            txtRole.Text = dt.Rows[0]["Role"].ToString();
                            ddlStatus.SelectedValue = dt.Rows[0]["Status"].ToString();

                            OpenModal();
                        }
                    }
                }
               
            }

            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "err",
                    "alert('" + ex.Message.Replace("'", "") + "');", true);
            }
           
        }
           
        private void DeleteRecord(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conString))
                {
                    using (SqlCommand cmd = new SqlCommand("usp_InsertAXLoginDetails", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Action", "DELETE");
                        cmd.Parameters.AddWithValue("@ID", id);

                        con.Open();

                        cmd.ExecuteNonQuery();

                        con.Close();
                    }
                }

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
                    "alert('" + ex.Message.Replace("'", "") + "');",
                    true);
            }
        }

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
