<%@ Page Title="Email Login Details" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="EmailAccountDetails.aspx.cs" Inherits="CredentialManagementPortal.EmailAccountDetails" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
<link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />
<link href="~/Content/credential-pages.css" runat="server" rel="stylesheet" />
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
<script type="text/javascript">
    function openModal() { new bootstrap.Modal(document.getElementById('emailModal')).show(); }
    function openAddModal() {
        var modal = document.getElementById('emailModal');
        modal.querySelectorAll('input[type="text"],input[type="email"],input[type="password"],input[type="hidden"],textarea').forEach(function (field) { field.value = ''; });
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
        <div><div class="credential-eyebrow"><span></span> CREDENTIAL LIBRARY</div><h1 class="credential-page-title">Email Account Details</h1><p class="credential-page-subtitle">Manage email credentials, recovery addresses, and MFA notes.</p></div>
        <asp:Button ID="btnAdd" runat="server" Text="+ Add Email Account" CssClass="credential-primary-button" OnClientClick="openAddModal();return false;" />
    </header>
    <section class="credential-card">
        <div class="credential-toolbar"><div><h2 class="credential-toolbar-title">Saved email accounts</h2><p class="credential-toolbar-subtitle">Search by address, department, recovery email, or MFA.</p></div><input class="credential-search" type="search" placeholder="Search email accounts..." aria-label="Search email accounts" oninput="filterGrid(this,'gvEmailAccount')" /></div>
        <div class="credential-table-wrap">
            <asp:GridView ID="gvEmailAccount" runat="server" ClientIDMode="Static" CssClass="table credential-grid" AutoGenerateColumns="False" GridLines="None" DataKeyNames="EmailLoginID" OnRowCommand="gvEmailAccount_RowCommand" Width="100%">
                <Columns>
                    <asp:BoundField DataField="Department" HeaderText="Department" />
                    <asp:BoundField DataField="EmailID" HeaderText="Email ID" />
                    <asp:TemplateField HeaderText="Password"><ItemTemplate><span class="text-secondary">••••••••</span></ItemTemplate></asp:TemplateField>
                    <asp:BoundField DataField="RecoveryEmail" HeaderText="Recovery Email" />
                    <asp:BoundField DataField="MFA" HeaderText="MFA" />
                    <asp:TemplateField HeaderText="Actions"><ItemTemplate>
                        <asp:LinkButton ID="btnEdit" runat="server" CommandName="EditRow" CommandArgument='<%# Eval("EmailLoginID") %>' CssClass="btn btn-sm btn-warning me-1" ToolTip="Edit"><i class="bi bi-pencil-square"></i></asp:LinkButton>
                        <asp:LinkButton ID="btnDelete" runat="server" CommandName="DeleteRow" CommandArgument='<%# Eval("EmailLoginID") %>' CssClass="btn btn-sm btn-danger" ToolTip="Delete" OnClientClick="return confirm('Delete this email account?');"><i class="bi bi-trash"></i></asp:LinkButton>
                    </ItemTemplate></asp:TemplateField>
                </Columns>
                <EmptyDataTemplate><div class="credential-empty"><span class="credential-empty-icon"><i class="bi bi-inbox"></i></span>No email accounts yet. Use “Add Email Account” to create one.</div></EmptyDataTemplate>
            </asp:GridView>
        </div>
    </section>
</div>
<div class="modal fade credential-modal" id="emailModal" tabindex="-1" aria-labelledby="emailModalTitle" aria-hidden="true"><div class="modal-dialog modal-lg modal-dialog-centered"><div class="modal-content">
    <div class="modal-header"><div><h5 class="modal-title" id="emailModalTitle">Email Account Details</h5><div class="credential-toolbar-subtitle">Add or update the account and recovery information.</div></div><button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button></div>
    <div class="modal-body"><asp:HiddenField ID="hfID" runat="server" /><div class="row g-3">
        <div class="col-md-6"><label class="form-label">Department</label><asp:TextBox ID="txtDepartment" runat="server" CssClass="form-control" /></div>
        <div class="col-md-6"><label class="form-label">Email ID</label><asp:TextBox ID="txtEmailID" runat="server" TextMode="Email" CssClass="form-control" /></div>
        <div class="col-md-6"><label class="form-label">Password</label><asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="form-control" /></div>
        <div class="col-md-6"><label class="form-label">Recovery Email</label><asp:TextBox ID="txtRecoveryEmail" runat="server" TextMode="Email" CssClass="form-control" /></div>
        <div class="col-md-12"><label class="form-label">MFA</label><asp:TextBox ID="txtMFA" runat="server" CssClass="form-control" /></div>
    </div></div>
    <div class="modal-footer"><button type="button" class="btn btn-light" data-bs-dismiss="modal">Cancel</button><asp:Button ID="btnSave" runat="server" Text="Save Email Account" CssClass="btn btn-primary" OnClick="btnSave_Click" /></div>
</div></div></div>
</asp:Content>

