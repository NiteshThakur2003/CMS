<%@ Page Title="AX Login Details" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AXLoginDetails.aspx.cs" Inherits="CredentialManagementPortal.AXLoginDetails" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
<link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />
<link href="~/Content/credential-pages.css" runat="server" rel="stylesheet" />
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
<script type="text/javascript">
    function openModal() { new bootstrap.Modal(document.getElementById('axModal')).show(); }
    function openAddModal() {
        var modal = document.getElementById('axModal');
        modal.querySelectorAll('input[type="text"],input[type="password"],input[type="hidden"],select').forEach(function (field) { field.value = ''; });
        modal.querySelectorAll('select').forEach(function (field) { field.selectedIndex = 0; });
        openModal();
    }
    function filterGrid(input, gridId) {
        var term = input.value.toLowerCase();
        document.querySelectorAll('#' + gridId + ' tr').forEach(function (row) {
            if (row.querySelector('th')) return;
            row.style.display = row.textContent.toLowerCase().indexOf(term) >= 0 ? '' : 'none';
        });
    }
</script>
<div class="credential-page">
    <header class="credential-page-header">
        <div><div class="credential-eyebrow"><span></span> CREDENTIAL LIBRARY</div><h1 class="credential-page-title">AX Login Details</h1><p class="credential-page-subtitle">Manage AX environments and account access.</p></div>
        <asp:Button ID="btnAdd" runat="server" Text="+ Add AX Login" CssClass="credential-primary-button" OnClientClick="openAddModal();return false;" />
    </header>
    <section class="credential-card">
        <div class="credential-toolbar"><div><h2 class="credential-toolbar-title">Saved AX accounts</h2><p class="credential-toolbar-subtitle">Search environments, URLs, users, or roles.</p></div><input class="credential-search" type="search" placeholder="Search AX accounts..." aria-label="Search AX accounts" oninput="filterGrid(this,'gvAXLogin')" /></div>
        <div class="credential-table-wrap">
            <asp:GridView ID="gvAXLogin" runat="server" ClientIDMode="Static" CssClass="table credential-grid" AutoGenerateColumns="False" GridLines="None" DataKeyNames="AXLoginID" OnRowCommand="gvAXLogin_RowCommand" Width="100%">
                <Columns>
                    <asp:BoundField DataField="Environment" HeaderText="Environment" />
                    <asp:BoundField DataField="URL" HeaderText="URL" />
                    <asp:BoundField DataField="UserID" HeaderText="User ID" />
                    <asp:TemplateField HeaderText="Password"><ItemTemplate><span class="text-secondary">••••••••</span></ItemTemplate></asp:TemplateField>
                    <asp:BoundField DataField="Role" HeaderText="Role" />
                    <asp:BoundField DataField="Status" HeaderText="Status" />
                    <asp:TemplateField HeaderText="Actions"><ItemTemplate>
                        <asp:LinkButton ID="btnEdit" runat="server" CommandName="EditRow" CommandArgument='<%# Eval("AXLoginID") %>' CssClass="btn btn-sm btn-warning me-1" ToolTip="Edit"><i class="bi bi-pencil-square"></i></asp:LinkButton>
                        <asp:LinkButton ID="btnDelete" runat="server" CommandName="DeleteRow" CommandArgument='<%# Eval("AXLoginID") %>' CssClass="btn btn-sm btn-danger" ToolTip="Delete" OnClientClick="return confirm('Delete this AX login?');"><i class="bi bi-trash"></i></asp:LinkButton>
                    </ItemTemplate></asp:TemplateField>
                </Columns>
                <EmptyDataTemplate><div class="credential-empty"><span class="credential-empty-icon"><i class="bi bi-inbox"></i></span>No AX accounts yet. Use “Add AX Login” to create one.</div></EmptyDataTemplate>
            </asp:GridView>
        </div>
    </section>
</div>
<div class="modal fade credential-modal" id="axModal" tabindex="-1" aria-labelledby="axModalTitle" aria-hidden="true"><div class="modal-dialog modal-lg modal-dialog-centered"><div class="modal-content">
    <div class="modal-header"><div><h5 class="modal-title" id="axModalTitle">AX Login Details</h5><div class="credential-toolbar-subtitle">Enter the environment and account information.</div></div><button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button></div>
    <div class="modal-body"><asp:HiddenField ID="hfID" runat="server" /><div class="row g-3">
        <div class="col-md-6"><label class="form-label">Environment</label><asp:TextBox ID="txtEnvironment" runat="server" CssClass="form-control" /></div>
        <div class="col-md-6"><label class="form-label">URL</label><asp:TextBox ID="txtURL" runat="server" CssClass="form-control" /></div>
        <div class="col-md-6"><label class="form-label">User ID</label><asp:TextBox ID="txtUserid" runat="server" CssClass="form-control" /></div>
        <div class="col-md-6"><label class="form-label">Password</label><asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="form-control" /></div>
        <div class="col-md-6"><label class="form-label">Role</label><asp:TextBox ID="txtRole" runat="server" CssClass="form-control" /></div>
        <div class="col-md-6"><label class="form-label">Status</label><asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select"><asp:ListItem Text="Active" Value="Active" /><asp:ListItem Text="Inactive" Value="Inactive" /></asp:DropDownList></div>
    </div></div>
    <div class="modal-footer"><button type="button" class="btn btn-light" data-bs-dismiss="modal">Cancel</button><asp:Button ID="btnSave" runat="server" Text="Save AX Login" CssClass="btn btn-primary" OnClick="btnSave_Click" /></div>
</div></div></div>
</asp:Content>

