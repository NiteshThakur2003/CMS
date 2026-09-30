<%@ Page Title="VPN Login Details" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="VPNLoingDetails.aspx.cs" Inherits="CredentialManagementPortal.VPNLoingDetails" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
<link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />
<link href="~/Content/credential-pages.css" runat="server" rel="stylesheet" />
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
<script type="text/javascript">
    function openModal() { new bootstrap.Modal(document.getElementById('vpnModal')).show(); }
    function openAddModal() {
        var modal = document.getElementById('vpnModal');
        modal.querySelectorAll('input[type="text"],input[type="password"],input[type="hidden"],textarea').forEach(function (field) { field.value = ''; });
        openModal();
    }
    function filterVpnGrid() {
        var term = document.getElementById('txtSearch').value.toLowerCase();
        document.querySelectorAll('#gvVPN tr').forEach(function (row) {
            if (row.querySelector('th')) return;
            row.style.display = row.textContent.toLowerCase().indexOf(term) >= 0 ? '' : 'none';
        });
    }
</script>
<div class="credential-page">
    <header class="credential-page-header">
        <div><div class="credential-eyebrow"><span></span> CREDENTIAL LIBRARY</div><h1 class="credential-page-title">VPN Login Details</h1><p class="credential-page-subtitle">Manage VPN access, connection URLs, and account owners.</p></div>
        <asp:Button ID="btnAdd" runat="server" Text="+ Add VPN Login" CssClass="credential-primary-button" OnClientClick="openAddModal();return false;" />
    </header>
    <section class="credential-card">
        <div class="credential-toolbar"><div><h2 class="credential-toolbar-title">Saved VPN accounts</h2><p class="credential-toolbar-subtitle">Search names, URLs, usernames, MFA, or owners.</p></div><asp:TextBox ID="txtSearch" runat="server" ClientIDMode="Static" TextMode="Search" CssClass="credential-search" placeholder="Search VPN accounts..." aria-label="Search VPN accounts" oninput="filterVpnGrid()" /></div>
        <div class="credential-table-wrap">
            <asp:GridView ID="gvVPN" runat="server" ClientIDMode="Static" DataKeyNames="VPNID" OnRowCommand="gvVPN_RowCommand" CssClass="table credential-grid" AutoGenerateColumns="False" GridLines="None" Width="100%">
                <Columns>
                    <asp:BoundField HeaderText="VPN Name" DataField="VPNName" />
                    <asp:BoundField HeaderText="URL" DataField="URL" />
                    <asp:BoundField HeaderText="Username" DataField="UserName" />
                    <asp:TemplateField HeaderText="Password"><ItemTemplate><span class="text-secondary">••••••••</span></ItemTemplate></asp:TemplateField>
                    <asp:BoundField HeaderText="MFA" DataField="MFA" />
                    <asp:BoundField HeaderText="Owner" DataField="Owner" />
                    <asp:TemplateField HeaderText="Actions"><ItemTemplate>
                        <asp:LinkButton ID="btnEdit" runat="server" CommandName="EditRow" CommandArgument='<%# Eval("VPNID") %>' CssClass="btn btn-sm btn-warning me-1" ToolTip="Edit"><i class="bi bi-pencil-square"></i></asp:LinkButton>
                        <asp:LinkButton ID="btnDelete" runat="server" CommandName="DeleteRow" CommandArgument='<%# Eval("VPNID") %>' CssClass="btn btn-sm btn-danger" ToolTip="Delete" OnClientClick="return confirm('Delete this VPN login?');"><i class="bi bi-trash"></i></asp:LinkButton>
                    </ItemTemplate></asp:TemplateField>
                </Columns>
                <EmptyDataTemplate><div class="credential-empty"><span class="credential-empty-icon"><i class="bi bi-inbox"></i></span>No VPN accounts yet. Use “Add VPN Login” to create one.</div></EmptyDataTemplate>
            </asp:GridView>
        </div>
    </section>
</div>
<div class="modal fade credential-modal" id="vpnModal" tabindex="-1" aria-labelledby="vpnModalTitle" aria-hidden="true"><div class="modal-dialog modal-lg modal-dialog-centered"><div class="modal-content">
    <div class="modal-header"><div><h5 class="modal-title" id="vpnModalTitle">VPN Login Details</h5><div class="credential-toolbar-subtitle">Enter connection and account details.</div></div><button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button></div>
    <div class="modal-body"><asp:HiddenField ID="hfID" runat="server" /><div class="row g-3">
        <div class="col-md-6"><label class="form-label">VPN Name</label><asp:TextBox ID="txtVPNName" runat="server" CssClass="form-control" /></div>
        <div class="col-md-6"><label class="form-label">URL</label><asp:TextBox ID="txtURL" runat="server" CssClass="form-control" /></div>
        <div class="col-md-6"><label class="form-label">Username</label><asp:TextBox ID="txtUserName" runat="server" CssClass="form-control" /></div>
        <div class="col-md-6"><label class="form-label">Password</label><asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="form-control" /></div>
        <div class="col-md-6"><label class="form-label">MFA</label><asp:TextBox ID="txtMFA" runat="server" CssClass="form-control" /></div>
        <div class="col-md-6"><label class="form-label">Owner</label><asp:TextBox ID="txtOwner" runat="server" CssClass="form-control" /></div>
        <div class="col-12"><label class="form-label">Notes</label><asp:TextBox ID="txtNotes" runat="server" TextMode="MultiLine" Rows="4" CssClass="form-control" /></div>
    </div></div>
    <div class="modal-footer"><button type="button" class="btn btn-light" data-bs-dismiss="modal">Cancel</button><asp:Button ID="btnSave" runat="server" Text="Save VPN Login" CssClass="btn btn-primary" OnClick="btnSave_Click" /></div>
</div></div></div>
</asp:Content>

