using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CredentialManagementPortal
{
    public partial class UserManagement : Page
    {
        private readonly string conString =
            ConfigurationManager.ConnectionStrings["Conn1"].ConnectionString;


        // =========================================================
        // PAGE LOAD
        // =========================================================

        protected void Page_Load(object sender, EventArgs e)
        {
            // Only Admin and User can access this page
            if (!CanViewUsers())
            {
                Response.Redirect(
                    ResolveUrl("~/Dashboard"),
                    false);

                Context.ApplicationInstance.CompleteRequest();
                return;
            }


            bool isAdmin = IsAdmin();


            // Admin-only controls
            pnlCreateUser.Visible = isAdmin;
            pnlAuditTab.Visible = isAdmin;
            pnlAuditPane.Visible = isAdmin;

            ddlRole.Enabled = isAdmin;
            pnlPasswordField.Visible = isAdmin;


            if (!IsPostBack)
            {
                BindUsers();

                if (isAdmin)
                {
                    BindAudit();
                }
            }
        }


        // =========================================================
        // ROLE CHECK
        // =========================================================

        private bool IsAdmin()
        {
            return String.Equals(
                Convert.ToString(Session["UserRole"]),
                "Admin",
                StringComparison.OrdinalIgnoreCase);
        }


        private bool CanViewUsers()
        {
            return IsAdmin();
        }


        // =========================================================
        // RUN STORED PROCEDURE - SELECT / AUDIT
        // =========================================================

        private DataTable RunQuery(
            string action,
            int? id = null)
        {
            DataTable result = new DataTable();

            using (SqlConnection connection =
                   new SqlConnection(conString))

            using (SqlCommand command =
                   new SqlCommand(
                       "dbo.usp_UserManagement",
                       connection))
            {
                command.CommandType =
                    CommandType.StoredProcedure;


                command.Parameters.Add(
                    "@Action",
                    SqlDbType.NVarChar,
                    30
                ).Value = action;

                command.Parameters.Add(
                    "@Role",
                    SqlDbType.NVarChar,
                    50
                ).Value = Convert.ToString(Session["UserRole"]);


                // ID only when required
                if (id.HasValue)
                {
                    command.Parameters.Add(
                        "@ID",
                        SqlDbType.Int
                    ).Value = id.Value;
                }


                using (SqlDataAdapter adapter =
                       new SqlDataAdapter(command))
                {
                    adapter.Fill(result);
                }
            }

            return result;
        }


        // =========================================================
        // BIND USERS
        // =========================================================

        private void BindUsers()
        {
            gvUsers.DataSource =
                RunQuery("SELECT");

            gvUsers.DataBind();
        }


        // =========================================================
        // BIND AUDIT
        // =========================================================

        private void BindAudit()
        {
            gvAudit.DataSource =
                RunQuery("AUDIT");

            gvAudit.DataBind();
        }


        // =========================================================
        // CREATE / UPDATE USER
        // =========================================================

        protected void btnSaveUser_Click(
            object sender,
            EventArgs e)
        {
            // Only Admin can create/update
            if (!IsAdmin())
            {
                ShowMessage(
                    "Only an Admin can create or edit users.");

                return;
            }


            bool editing =
                !String.IsNullOrWhiteSpace(
                    hfUserID.Value);


            string username =
                txtUserName.Text.Trim();


            string password =
                txtPassword.Text;


            string role =
                ddlRole.SelectedValue;


            // =====================================================
            // VALIDATION
            // =====================================================

            if (username.Length == 0)
            {
                ShowMessage(
                    "Enter a username.");

                return;
            }


            if (String.IsNullOrEmpty(role))
            {
                ShowMessage(
                    "Select Admin or User.");

                return;
            }


            if (role != "Admin" && role != "User")
            {
                ShowMessage(
                    "Invalid role. Select Admin or User.");

                return;
            }
            if (!editing && String.IsNullOrWhiteSpace(password))
            {
                ShowMessage("Enter a password for the new user.");
                return;
            }


            try
            {
                using (SqlConnection connection =
                       new SqlConnection(conString))

                using (SqlCommand command =
                       new SqlCommand(
                           "dbo.usp_UserManagement",
                           connection))
                {
                    command.CommandType =
                        CommandType.StoredProcedure;


                    // =================================================
                    // ACTION
                    // =================================================

                    command.Parameters.Add(
                        "@Action",
                        SqlDbType.NVarChar,
                        30
                    ).Value =
                        editing ? "UPDATE" : "INSERT";


                    // =================================================
                    // ID - ONLY FOR UPDATE
                    // =================================================

                    if (editing)
                    {
                        int userId;

                        if (!Int32.TryParse(
                            hfUserID.Value,
                            out userId))
                        {
                            ShowMessage(
                                "Invalid user ID.");

                            return;
                        }


                        command.Parameters.Add(
                            "@ID",
                            SqlDbType.Int
                        ).Value = userId;
                    }


                    // =================================================
                    // USERNAME
                    // =================================================

                    command.Parameters.Add(
                        "@UserName",
                        SqlDbType.NVarChar,
                        100
                    ).Value = username;


                    // =================================================
                    // ROLE
                    // =================================================

                    command.Parameters.Add(
                        "@Role",
                        SqlDbType.NVarChar,
                        50
                    ).Value = role;


                    // =================================================
                    // ACTION BY
                    // =================================================

                    command.Parameters.Add(
                        "@ActionBy",
                        SqlDbType.NVarChar,
                        100
                    ).Value = CurrentUser();


                    // =================================================
                    // PASSWORD
                    // =================================================

                    if (!editing || !String.IsNullOrEmpty(password))
                    {
                        command.Parameters.Add(
                            "@Password",
                            SqlDbType.NVarChar,
                            100
                        ).Value = password;
                    }


                    // =================================================
                    // EXECUTE
                    // =================================================

                    connection.Open();


                    using (SqlDataReader reader =
                           command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int status =
                                Convert.ToInt32(
                                    reader["Status"]);

                            string message =
                                Convert.ToString(
                                    reader["Message"]);


                            if (status != 1)
                            {
                                ShowMessage(message);
                                return;
                            }
                        }
                    }
                }


                // =====================================================
                // SUCCESS
                // =====================================================

                ClearEditor();

                BindUsers();


                if (IsAdmin())
                {
                    BindAudit();
                }


                RegisterScript(
                    "saved",
                    "var modal = bootstrap.Modal.getInstance(document.getElementById('userModal')); " +
                    "if(modal) modal.hide(); " +
                    "alert('" +
                    HttpUtility.JavaScriptStringEncode(
                        editing
                            ? "User updated successfully."
                            : "User created successfully.")
                    +
                    "');"
                );
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message);
            }
        }


        // =========================================================
        // GRID ROW DATA BOUND
        // =========================================================

        protected void gvUsers_RowDataBound(
            object sender,
            GridViewRowEventArgs e)
        {
            if (e.Row.RowType ==
                DataControlRowType.DataRow)
            {
                // User role is view-only
                if (!IsAdmin())
                {
                    LinkButton edit =
                        e.Row.FindControl(
                            "editUser") as LinkButton;


                    LinkButton deactivate =
                        e.Row.FindControl(
                            "deactivateUser") as LinkButton;


                    if (edit != null)
                    {
                        edit.Visible = false;
                    }


                    if (deactivate != null)
                    {
                        deactivate.Visible = false;
                    }
                }
            }
        }


        // =========================================================
        // GRID COMMAND
        // =========================================================

        protected void gvUsers_RowCommand(
            object sender,
            GridViewCommandEventArgs e)
        {
            // Only Admin can perform actions
            if (!IsAdmin())
            {
                ShowMessage(
                    "Your role is view only.");

                return;
            }


            int id;


            if (!Int32.TryParse(
                Convert.ToString(
                    e.CommandArgument),
                out id))
            {
                return;
            }


            if (e.CommandName == "EditRow")
            {
                EditUser(id);
            }

            else if (e.CommandName ==
                     "DeactivateRow")
            {
                DeactivateUser(id);
            }
        }


        // =========================================================
        // EDIT USER
        // =========================================================

        private void EditUser(int id)
        {
            try
            {
                DataTable users =
                    RunQuery("SELECT");


                DataRow[] rows =
                    users.Select(
                        "ID = " + id.ToString());


                if (rows.Length == 0)
                {
                    ShowMessage(
                        "User not found.");

                    return;
                }


                DataRow row = rows[0];


                // User ID
                hfUserID.Value =
                    Convert.ToString(
                        row["ID"]);


                // Username
                txtUserName.Text =
                    Convert.ToString(
                        row["UserName"]);


                // Role
                string role =
                    Convert.ToString(
                        row["Role"]);


                if (ddlRole.Items.FindByValue(role)
                    != null)
                {
                    ddlRole.SelectedValue =
                        role;
                }


                // IMPORTANT:
                // Never load existing password
                txtPassword.Text = String.Empty;


                txtPassword.Attributes["placeholder"] =
                    "Optional: enter a new password, or leave blank to keep the current password";


                pnlPasswordField.Visible =
                    IsAdmin();


                lblUserModalTitle.Text =
                    "Edit user";


                btnSaveUser.Text =
                    "Save changes";


                RegisterScript(
                    "edit",
                    "openUserDialog();"
                );
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message);
            }
        }


        // =========================================================
        // DEACTIVATE USER
        // =========================================================

        private void DeactivateUser(int id)
        {
            if (!IsAdmin())
            {
                ShowMessage(
                    "Only an Admin can deactivate users.");

                return;
            }


            try
            {
                using (SqlConnection connection =
                       new SqlConnection(conString))

                using (SqlCommand command =
                       new SqlCommand(
                           "dbo.usp_UserManagement",
                           connection))
                {
                    command.CommandType =
                        CommandType.StoredProcedure;


                    command.Parameters.Add(
                        "@Action",
                        SqlDbType.NVarChar,
                        30
                    ).Value = "DELETE";


                    command.Parameters.Add(
                        "@ID",
                        SqlDbType.Int
                    ).Value = id;


                    command.Parameters.Add(
                        "@Role",
                        SqlDbType.NVarChar,
                        50
                    ).Value = "Admin";


                    command.Parameters.Add(
                        "@ActionBy",
                        SqlDbType.NVarChar,
                        100
                    ).Value = CurrentUser();


                    connection.Open();


                    using (SqlDataReader reader =
                           command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int status =
                                Convert.ToInt32(
                                    reader["Status"]);


                            string message =
                                Convert.ToString(
                                    reader["Message"]);


                            if (status != 1)
                            {
                                ShowMessage(message);
                                return;
                            }
                        }
                    }
                }


                BindUsers();


                if (IsAdmin())
                {
                    BindAudit();
                }


                RegisterScript(
                    "deactivated",
                    "alert('User deactivated successfully.');"
                );
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message);
            }
        }


        // =========================================================
        // CURRENT LOGGED-IN USER
        // =========================================================

        private string CurrentUser()
        {
            string user =
                Convert.ToString(
                    Session["AuthenticatedUser"]);


            if (String.IsNullOrWhiteSpace(user))
            {
                return "System";
            }


            return user;
        }


        // =========================================================
        // ROLE CSS
        // =========================================================

        protected string GetRoleCss(object value)
        {
            string role =
                Convert.ToString(value);


            if (String.Equals(
                role,
                "Admin",
                StringComparison.OrdinalIgnoreCase))
            {
                return "admin";
            }


            return "readonly";
        }


        // =========================================================
        // CLEAR USER FORM
        // =========================================================

        private void ClearEditor()
        {
            hfUserID.Value =
                String.Empty;


            txtUserName.Text =
                String.Empty;


            txtPassword.Text =
                String.Empty;


            ddlRole.SelectedIndex =
                0;


            pnlPasswordField.Visible =
                true;


            txtPassword.Attributes["placeholder"] =
                "Enter a password";


            lblUserModalTitle.Text =
                "Create user";


            btnSaveUser.Text =
                "Create user";
        }


        // =========================================================
        // SHOW MESSAGE
        // =========================================================

        private void ShowMessage(
            string message)
        {
            RegisterScript(
                "message",
                "alert('" +
                HttpUtility.JavaScriptStringEncode(
                    message ?? String.Empty)
                +
                "');"
            );
        }


        // =========================================================
        // REGISTER JAVASCRIPT
        // =========================================================

        private void RegisterScript(
            string key,
            string script)
        {
            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                key,
                script,
                true);
        }
    }
}



