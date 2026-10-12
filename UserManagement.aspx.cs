
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
            if (!CanViewUsers())
            {
                Response.Redirect(ResolveUrl("~/Dashboard"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            bool isAdmin = IsAdmin();

            pnlCreateUser.Visible = isAdmin;
            pnlAuditTab.Visible = isAdmin;
            pnlAuditPane.Visible = isAdmin;

            ddlRole.Enabled = isAdmin;
            pnlPasswordField.Visible = isAdmin;

            if (!IsPostBack)
            {
                BindUsers();

                if (isAdmin)
                    BindAudit();
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
            // Preserve existing access rule: Admin only.
            return IsAdmin();
        }

        // =========================================================
        // RUN SELECT / AUDIT QUERY
        // =========================================================
        private DataTable RunQuery(string action, int? id = null)
        {
            DataTable result = new DataTable();

            using (SqlConnection connection = new SqlConnection(conString))
            using (SqlCommand command =
                new SqlCommand("dbo.usp_UserManagement", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("@Action", SqlDbType.NVarChar, 30)
                    .Value = action;

                command.Parameters.Add("@Role", SqlDbType.NVarChar, 50)
                    .Value = Convert.ToString(Session["UserRole"]);

                if (id.HasValue)
                {
                    command.Parameters.Add("@ID", SqlDbType.Int)
                        .Value = id.Value;
                }

                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
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
            gvUsers.DataSource = RunQuery("SELECT");
            gvUsers.DataBind();
        }

        // =========================================================
        // BIND AUDIT
        // =========================================================
        private void BindAudit()
        {
            gvAudit.DataSource = RunQuery("AUDIT");
            gvAudit.DataBind();
        }

        // =========================================================
        // CREATE / UPDATE USER
        // =========================================================
        protected void btnSaveUser_Click(object sender, EventArgs e)
        {
            if (!IsAdmin())
            {
                ShowMessage("Only an Admin can create or edit users.");
                return;
            }

            int userId = 0;

            bool editing = !String.IsNullOrWhiteSpace(hfUserID.Value);

            if (editing &&
                !Int32.TryParse(hfUserID.Value, out userId))
            {
                ShowMessage("Invalid user ID.");
                return;
            }

            string username = txtUserName.Text.Trim();
            string password = txtPassword.Text;
            string role = ddlRole.SelectedValue;

            if (username.Length == 0)
            {
                ShowMessage("Enter a username.");
                return;
            }

            if (String.IsNullOrEmpty(role))
            {
                ShowMessage("Select Admin or User.");
                return;
            }

            if (role != "Admin" && role != "User")
            {
                ShowMessage("Invalid role. Select Admin or User.");
                return;
            }

            if (!editing && String.IsNullOrWhiteSpace(password))
            {
                ShowMessage("Enter a password for the new user.");
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(conString))
                using (SqlCommand command =
                    new SqlCommand("dbo.usp_UserManagement", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@Action", SqlDbType.NVarChar, 30)
                        .Value = editing ? "UPDATE" : "INSERT";

                    if (editing)
                    {
                        command.Parameters.Add("@ID", SqlDbType.Int)
                            .Value = userId;
                    }

                    command.Parameters.Add("@UserName", SqlDbType.NVarChar, 100)
                        .Value = username;

                    command.Parameters.Add("@Role", SqlDbType.NVarChar, 50)
                        .Value = role;

                    command.Parameters.Add("@ActionBy", SqlDbType.NVarChar, 100)
                        .Value = CurrentUser();

                    if (!editing || !String.IsNullOrEmpty(password))
                    {
                        command.Parameters.Add("@Password", SqlDbType.NVarChar, 100)
                            .Value = password;
                    }

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int status = Convert.ToInt32(reader["Status"]);
                            string message = Convert.ToString(reader["Message"]);

                            if (status != 1)
                            {
                                ShowMessage(message);
                                return;
                            }
                        }
                    }
                }

                // Dashboard log after successful user operation.
                LogDashboardActivity(
                    editing ? "Updated" : "Created",
                    editing ? Convert.ToString(userId) : null,
                    username);

                ClearEditor();
                BindUsers();

                if (IsAdmin())
                    BindAudit();

                RegisterScript(
                    "saved",
                    "var modal = bootstrap.Modal.getInstance(" +
                    "document.getElementById('userModal'));" +
                    "if(modal) modal.hide();" +
                    "alert('" +
                    HttpUtility.JavaScriptStringEncode(
                        editing
                            ? "User updated successfully."
                            : "User created successfully.") +
                    "');");
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
            object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (!IsAdmin())
                {
                    LinkButton edit =
                        e.Row.FindControl("editUser") as LinkButton;

                    LinkButton deactivate =
                        e.Row.FindControl("deactivateUser") as LinkButton;

                    if (edit != null)
                        edit.Visible = false;

                    if (deactivate != null)
                        deactivate.Visible = false;
                }
            }
        }

        // =========================================================
        // GRID COMMAND
        // =========================================================
        protected void gvUsers_RowCommand(
            object sender, GridViewCommandEventArgs e)
        {
            if (!IsAdmin())
            {
                ShowMessage("Your role is view only.");
                return;
            }

            int id;

            if (!Int32.TryParse(Convert.ToString(e.CommandArgument), out id))
                return;

            if (e.CommandName == "EditRow")
            {
                EditUser(id);
            }
            else if (e.CommandName == "DeactivateRow")
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
                DataTable users = RunQuery("SELECT");
                DataRow[] rows = users.Select("ID = " + id.ToString());

                if (rows.Length == 0)
                {
                    ShowMessage("User not found.");
                    return;
                }

                DataRow row = rows[0];

                hfUserID.Value = Convert.ToString(row["ID"]);
                txtUserName.Text = Convert.ToString(row["UserName"]);

                string role = Convert.ToString(row["Role"]);

                if (ddlRole.Items.FindByValue(role) != null)
                    ddlRole.SelectedValue = role;

                // Never load the existing password.
                txtPassword.Text = String.Empty;

                txtPassword.Attributes["placeholder"] =
                    "Optional: enter a new password, or leave blank to keep the current password";

                pnlPasswordField.Visible = IsAdmin();
                lblUserModalTitle.Text = "Edit user";
                btnSaveUser.Text = "Save changes";

                RegisterScript("edit", "openUserDialog();");
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
                ShowMessage("Only an Admin can deactivate users.");
                return;
            }

            try
            {
                // Fetch username before deactivation for activity history.
                string username = String.Empty;

                DataTable users = RunQuery("SELECT", id);

                if (users.Rows.Count > 0 &&
                    users.Columns.Contains("UserName"))
                {
                    username = Convert.ToString(users.Rows[0]["UserName"]);
                }
                else
                {
                    // Fallback if SELECT does not support filtering by ID.
                    DataTable allUsers = RunQuery("SELECT");
                    DataRow[] rows = allUsers.Select("ID = " + id.ToString());

                    if (rows.Length > 0)
                        username = Convert.ToString(rows[0]["UserName"]);
                }

                using (SqlConnection connection = new SqlConnection(conString))
                using (SqlCommand command =
                    new SqlCommand("dbo.usp_UserManagement", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@Action", SqlDbType.NVarChar, 30)
                        .Value = "DELETE";

                    command.Parameters.Add("@ID", SqlDbType.Int)
                        .Value = id;

                    command.Parameters.Add("@Role", SqlDbType.NVarChar, 50)
                        .Value = "Admin";

                    command.Parameters.Add("@ActionBy", SqlDbType.NVarChar, 100)
                        .Value = CurrentUser();

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int status = Convert.ToInt32(reader["Status"]);
                            string message = Convert.ToString(reader["Message"]);

                            if (status != 1)
                            {
                                ShowMessage(message);
                                return;
                            }
                        }
                    }
                }

                // Log only after successful deactivation.
                LogDashboardActivity(
                    "Deactivated",
                    Convert.ToString(id),
                    username);

                BindUsers();

                if (IsAdmin())
                    BindAudit();

                RegisterScript(
                    "deactivated",
                    "alert('User deactivated successfully.');");
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message);
            }
        }

        // =========================================================
        // DASHBOARD ACTIVITY LOGGER
        // Uses dbo.usp_Dashboard with @Action = LOG
        // =========================================================
        private void LogDashboardActivity(
            string actionName,
            string recordId,
            string username)
        {
            try
            {
                string actionBy = CurrentUser();

                using (SqlConnection connection = new SqlConnection(conString))
                using (SqlCommand command =
                    new SqlCommand("dbo.usp_Dashboard", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@Action", SqlDbType.VarChar, 20)
                        .Value = "LOG";

                    command.Parameters.Add("@ActivityType", SqlDbType.NVarChar, 100)
                        .Value = "User " + actionName;

                    command.Parameters.Add("@ActivityKey", SqlDbType.NVarChar, 50)
                        .Value = "user";

                    command.Parameters.Add("@ActivityName", SqlDbType.NVarChar, 200)
                        .Value = "User: " + (username ?? String.Empty);

                    command.Parameters.Add("@ActivityIcon", SqlDbType.NVarChar, 50)
                        .Value = "bi-people";

                    command.Parameters.Add("@ActionBy", SqlDbType.NVarChar, 100)
                        .Value = actionBy;

                    command.Parameters.Add("@PageName", SqlDbType.NVarChar, 100)
                        .Value = "UserManagement";

                    command.Parameters.Add("@RecordID", SqlDbType.NVarChar, 100)
                        .Value = String.IsNullOrWhiteSpace(recordId)
                            ? (object)DBNull.Value
                            : recordId;

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Do not expose passwords or stop the main operation
                // if dashboard logging fails.
                System.Diagnostics.Trace.TraceError(
                    "User management dashboard logging failed: " + ex);
            }
        }

        // =========================================================
        // CURRENT LOGGED-IN USER
        // =========================================================
        private string CurrentUser()
        {
            string user = Convert.ToString(Session["AuthenticatedUser"]);

            return String.IsNullOrWhiteSpace(user) ? "System" : user;
        }

        // =========================================================
        // ROLE CSS
        // =========================================================
        protected string GetRoleCss(object value)
        {
            string role = Convert.ToString(value);

            if (String.Equals(
                role, "Admin", StringComparison.OrdinalIgnoreCase))
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
            hfUserID.Value = String.Empty;
            txtUserName.Text = String.Empty;
            txtPassword.Text = String.Empty;

            if (ddlRole.Items.Count > 0)
                ddlRole.SelectedIndex = 0;

            pnlPasswordField.Visible = true;

            txtPassword.Attributes["placeholder"] = "Enter a password";
            lblUserModalTitle.Text = "Create user";
            btnSaveUser.Text = "Create user";
        }

        // =========================================================
        // SHOW MESSAGE
        // =========================================================
        private void ShowMessage(string message)
        {
            RegisterScript(
                "message",
                "alert('" +
                HttpUtility.JavaScriptStringEncode(message ?? String.Empty) +
                "');");
        }

        // =========================================================
        // REGISTER JAVASCRIPT
        // =========================================================
        private void RegisterScript(string key, string script)
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
