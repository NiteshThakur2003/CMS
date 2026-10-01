<%@ Page Title="User Management" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UserManagement.aspx.cs" Inherits="CredentialManagementPortal.UserManagement" %>
<asp:Content ID="UserManagementContent" ContentPlaceHolderID="MainContent" runat="server">
<link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>

<script type="text/javascript">
    function userTab(id) {
        var hidden = document.getElementById('hfActiveTab');
        if (hidden) hidden.value = id;
        var tab = document.getElementById(id);
        if (tab && window.bootstrap) bootstrap.Tab.getOrCreateInstance(tab).show();
    }
    function openUserDialog() { bootstrap.Modal.getOrCreateInstance(document.getElementById('userModal')).show(); }
    function newUserDialog() {
        document.getElementById('hfUserID').value = '';
        document.getElementById('txtUserName').value = '';
        document.getElementById('txtPassword').value = '';
        document.getElementById('ddlRole').selectedIndex = 0;
        document.getElementById('lblUserModalTitle').innerText = 'Create user';
        document.getElementById('btnSaveUser').value = 'Create user';
        document.getElementById('txtPassword').placeholder = 'Enter a password';
        openUserDialog();
    }
    function filterGrid(input, id) {
        var term = input.value.toLowerCase();
        document.querySelectorAll('#' + id + ' tr').forEach(function (row) {
            if (!row.querySelector('th')) row.style.display = row.innerText.toLowerCase().includes(term) ? '' : 'none';
        });
    }
    document.addEventListener('DOMContentLoaded', function () {
        var current = document.getElementById('hfActiveTab');
        if (current && current.value) userTab(current.value);
    });
</script>
<div class="credential-page">
    <header class="credential-page-header">
        <div><div class="credential-eyebrow"><span></span> ADMINISTRATION</div><h1 class="credential-page-title">User management</h1><p class="credential-page-subtitle">Create portal accounts, assign access roles, and review account activity.</p></div>
        <asp:Panel ID="pnlCreateUser" runat="server"><button type="button" class="credential-primary-button" onclick="newUserDialog()"><i class="bi bi-person-plus"></i> Create user</button></asp:Panel>
    </header>
    <asp:HiddenField ID="hfActiveTab" runat="server" ClientIDMode="Static" Value="users-tab" />
    <section class="credential-card">
        <ul class="nav nav-tabs user-management-tabs" id="userTabs" role="tablist">
            <li class="nav-item"><button class="nav-link active" id="users-tab" data-bs-toggle="tab" data-bs-target="#users-pane" type="button" role="tab" onclick="document.getElementById('hfActiveTab').value='users-tab'"><i class="bi bi-people"></i> Users</button></li>
            <asp:PlaceHolder ID="pnlAuditTab" runat="server"><li class="nav-item"><button class="nav-link" id="audit-tab" data-bs-toggle="tab" data-bs-target="#audit-pane" type="button" role="tab" onclick="document.getElementById('hfActiveTab').value='audit-tab'"><i class="bi bi-clock-history"></i> Audit history</button></li></asp:PlaceHolder>
        </ul>
        <div class="tab-content">
            <div class="tab-pane fade show active" id="users-pane" role="tabpanel">
                <div class="credential-toolbar"><div><h2 class="credential-toolbar-title">Active users</h2><p class="credential-toolbar-subtitle">Permissions are assigned by the selected role.</p></div><input class="credential-search" type="search" placeholder="Search users..." aria-label="Search users" oninput="filterGrid(this,'gvUsers')" /></div>
                <div class="credential-table-wrap">
                    <asp:GridView ID="gvUsers" runat="server" ClientIDMode="Static" CssClass="table credential-grid" AutoGenerateColumns="False" GridLines="None" DataKeyNames="ID" OnRowCommand="gvUsers_RowCommand" OnRowDataBound="gvUsers_RowDataBound" Width="100%">
                        <Columns>
                            <asp:BoundField DataField="UserName" HeaderText="Username" />
                            <asp:TemplateField HeaderText="Role"><ItemTemplate><span class='user-role role-<%# GetRoleCss(Eval("Role")) %>'><%#: Eval("Role") %></span></ItemTemplate></asp:TemplateField>
                            <asp:BoundField DataField="CreatedDate" HeaderText="Created" DataFormatString="{0:dd MMM yyyy}" HtmlEncode="true" />
                            <asp:TemplateField HeaderText="Actions"><ItemTemplate>
                                <asp:LinkButton ID="editUser" runat="server" CommandName="EditRow" CommandArgument='<%# Eval("ID") %>' CssClass="btn btn-sm btn-outline-primary me-1" ToolTip="Edit user" CausesValidation="false"><i class="bi bi-pencil"></i></asp:LinkButton>
                                <asp:LinkButton ID="deactivateUser" runat="server" CommandName="DeactivateRow" CommandArgument='<%# Eval("ID") %>' CssClass="btn btn-sm btn-outline-danger" ToolTip="Deactivate user" CausesValidation="false" OnClientClick="return confirm('Deactivate this user?');"><i class="bi bi-person-dash"></i></asp:LinkButton>
                            </ItemTemplate></asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate><div class="credential-empty"><span class="credential-empty-icon"><i class="bi bi-people"></i></span>No active users found.</div></EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>
            <asp:PlaceHolder ID="pnlAuditPane" runat="server"><div class="tab-pane fade" id="audit-pane" role="tabpanel">
                <div class="credential-toolbar"><div><h2 class="credential-toolbar-title">Audit history</h2><p class="credential-toolbar-subtitle">Account creation, changes, and deactivation events.</p></div><input class="credential-search" type="search" placeholder="Search activity..." aria-label="Search audit history" oninput="filterGrid(this,'gvAudit')" /></div>
                <div class="credential-table-wrap">
                    <asp:GridView ID="gvAudit" runat="server" ClientIDMode="Static" CssClass="table credential-grid" AutoGenerateColumns="False" GridLines="None" Width="100%">
                        <Columns>
                            <asp:BoundField DataField="UserName" HeaderText="Username" /><asp:BoundField DataField="Role" HeaderText="Role" /><asp:BoundField DataField="ActionName" HeaderText="Action" /><asp:BoundField DataField="ActionBy" HeaderText="Performed by" /><asp:BoundField DataField="ActionDate" HeaderText="Date" DataFormatString="{0:dd MMM yyyy, h:mm tt}" HtmlEncode="true" /><asp:BoundField DataField="ActionDetails" HeaderText="Details" />
                        </Columns>
                        <EmptyDataTemplate><div class="credential-empty"><span class="credential-empty-icon"><i class="bi bi-clock-history"></i></span>No audit events found.</div></EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div></asp:PlaceHolder>
        </div>
    </section>
</div>
<div class="modal fade credential-modal" id="userModal" tabindex="-1" aria-labelledby="userModalTitle" aria-hidden="true"><div class="modal-dialog modal-lg modal-dialog-centered"><div class="modal-content">
    <div class="modal-header"><div><asp:Label ID="lblUserModalTitle" runat="server" ClientIDMode="Static" CssClass="modal-title" Text="Create user" /><div class="credential-toolbar-subtitle">Choose a role to apply its permissions.</div></div><button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button></div>
    <div class="modal-body"><asp:HiddenField ID="hfUserID" runat="server" ClientIDMode="Static" /><div class="row g-3">
        <div class="col-md-6"><asp:Label runat="server" AssociatedControlID="txtUserName" CssClass="form-label" Text="Username" /><asp:TextBox ID="txtUserName" runat="server" ClientIDMode="Static" MaxLength="100" CssClass="form-control" autocomplete="off" /></div>
        <div class="col-md-6"><asp:Label runat="server" AssociatedControlID="ddlRole" CssClass="form-label" Text="Role" /><asp:DropDownList ID="ddlRole" runat="server" ClientIDMode="Static" CssClass="form-select"><asp:ListItem Text="Select a role" Value="" /><asp:ListItem Text="Admin" Value="Admin" /><asp:ListItem Text="User" Value="User" /></asp:DropDownList></div>
        <asp:Panel ID="pnlPasswordField" runat="server" CssClass="col-12"><asp:Label runat="server" AssociatedControlID="txtPassword" CssClass="form-label" Text="Password" /><asp:TextBox ID="txtPassword" runat="server" ClientIDMode="Static" TextMode="Password" MaxLength="100" CssClass="form-control" autocomplete="new-password" /><small class="credential-field-hint">Enter a password when creating or editing a user.</small></asp:Panel>
    </div></div>
    <div class="modal-footer"><button type="button" class="btn btn-light" data-bs-dismiss="modal">Cancel</button><asp:Button ID="btnSaveUser" runat="server" ClientIDMode="Static" Text="Create user" CssClass="btn btn-primary" OnClick="btnSaveUser_Click" /></div>
</div></div></div>
</asp:Content>







