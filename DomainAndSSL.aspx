<%@ Page Title="Domain & SSL" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="DomainAndSSL.aspx.cs" Inherits="CredentialManagementPortal.DomainAndSSL" %>
<asp:Content ID="DomainSslContent" ContentPlaceHolderID="MainContent" runat="server">
<link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
<script type="text/javascript">
    function selectAssetTab(tabId) {
        document.getElementById('hfActiveTab').value = tabId;
        bootstrap.Tab.getOrCreateInstance(document.getElementById(tabId)).show();
    }
    function openAssetModal(modalId) {
        bootstrap.Modal.getOrCreateInstance(document.getElementById(modalId)).show();
    }
    function openAddAssetModal(modalId) {
        var modal = document.getElementById(modalId);
        modal.querySelectorAll('input[type="text"],input[type="date"],input[type="hidden"],textarea').forEach(function (field) { field.value = ''; });
        openAssetModal(modalId);
    }
    function filterAssetGrid(input, gridId) {
        var term = input.value.toLowerCase();
        document.querySelectorAll('#' + gridId + ' tr').forEach(function (row) {
            if (row.querySelector('th')) return;
            row.style.display = row.textContent.toLowerCase().indexOf(term) >= 0 ? '' : 'none';
        });
    }
    document.addEventListener('DOMContentLoaded', function () {
        var activeTab = document.getElementById('hfActiveTab').value || 'domains-tab';
        var tab = document.getElementById(activeTab);
        if (tab) bootstrap.Tab.getOrCreateInstance(tab).show();
        document.querySelectorAll('#assetTabs button[data-bs-toggle="tab"]').forEach(function (button) {
            button.addEventListener('shown.bs.tab', function () { document.getElementById('hfActiveTab').value = button.id; });
        });
    });
</script>
<div class="credential-page domain-ssl-page">
    <header class="credential-page-header">
        <div><div class="credential-eyebrow"><span></span> INFRASTRUCTURE</div><h1 class="credential-page-title">Domain &amp; SSL</h1><p class="credential-page-subtitle">Track domain ownership, DNS providers, certificates, and expiry dates.</p></div>
    </header>
    <asp:HiddenField ID="hfActiveTab" runat="server" ClientIDMode="Static" Value="domains-tab" />
    <section class="credential-card domain-ssl-card">
        <ul class="nav nav-tabs domain-ssl-tabs" id="assetTabs" role="tablist">
            <li class="nav-item" role="presentation"><button class="nav-link active" id="domains-tab" data-bs-toggle="tab" data-bs-target="#domains-pane" type="button" role="tab" aria-controls="domains-pane" aria-selected="true" onclick="document.getElementById('hfActiveTab').value='domains-tab';"><i class="bi bi-globe2"></i> Domains</button></li>
            <li class="nav-item" role="presentation"><button class="nav-link" id="ssl-tab" data-bs-toggle="tab" data-bs-target="#ssl-pane" type="button" role="tab" aria-controls="ssl-pane" aria-selected="false" onclick="document.getElementById('hfActiveTab').value='ssl-tab';"><i class="bi bi-shield-lock"></i> SSL Certificates</button></li>
        </ul>
        <div class="tab-content">
            <div class="tab-pane fade show active" id="domains-pane" role="tabpanel" aria-labelledby="domains-tab" tabindex="0">
                <div class="credential-toolbar"><div><h2 class="credential-toolbar-title">Domains</h2><p class="credential-toolbar-subtitle">Monitor registrar, DNS, and renewal dates.</p></div><div class="asset-toolbar-actions"><input class="credential-search" type="search" placeholder="Search domains..." aria-label="Search domains" oninput="filterAssetGrid(this,'gvDomains')" /><asp:Button ID="btnAddDomain" runat="server" Text="+ Add Domain" CssClass="credential-primary-button" OnClientClick="openAddAssetModal('domainModal');return false;" /></div></div>
                <div class="credential-table-wrap">
                    <asp:GridView ID="gvDomains" runat="server" ClientIDMode="Static" CssClass="table credential-grid" AutoGenerateColumns="False" GridLines="None" DataKeyNames="DomainID" OnRowCommand="gvDomains_RowCommand" Width="100%">
                        <Columns>
                            <asp:BoundField DataField="DomainName" HeaderText="Domain Name" />
                            <asp:BoundField DataField="Registrar" HeaderText="Registrar" />
                            <asp:BoundField DataField="DNS" HeaderText="DNS Provider" />
                            <asp:BoundField DataField="Expiry" HeaderText="Expiry Date" DataFormatString="{0:dd MMM yyyy}" HtmlEncode="true" />
                            <asp:BoundField DataField="DaysRemaining" HeaderText="Days Remaining" />
                            <asp:TemplateField HeaderText="Status"><ItemTemplate><span class='expiry-status <%# GetExpiryStatusCss(Eval("ExpiryStatus")) %>'><%#: Eval("ExpiryStatus") %></span></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Actions"><ItemTemplate>
                                <asp:LinkButton ID="btnEditDomain" runat="server" CommandName="EditRow" CommandArgument='<%# Eval("DomainID") %>' CssClass="btn btn-sm btn-warning me-1" ToolTip="Edit domain" CausesValidation="false"><i class="bi bi-pencil-square"></i></asp:LinkButton>
                                <asp:LinkButton ID="btnDeleteDomain" runat="server" CommandName="DeleteRow" CommandArgument='<%# Eval("DomainID") %>' CssClass="btn btn-sm btn-danger" ToolTip="Delete domain" CausesValidation="false" OnClientClick="return confirm('Delete this domain?');"><i class="bi bi-trash"></i></asp:LinkButton>
                            </ItemTemplate></asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate><div class="credential-empty"><span class="credential-empty-icon"><i class="bi bi-globe2"></i></span>No domains yet. Add a domain to start tracking it.</div></EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>
            <div class="tab-pane fade" id="ssl-pane" role="tabpanel" aria-labelledby="ssl-tab" tabindex="0">
                <div class="credential-toolbar"><div><h2 class="credential-toolbar-title">SSL Certificates</h2><p class="credential-toolbar-subtitle">Track certificate providers and renewal deadlines.</p></div><div class="asset-toolbar-actions"><input class="credential-search" type="search" placeholder="Search certificates..." aria-label="Search SSL certificates" oninput="filterAssetGrid(this,'gvSsl')" /><asp:Button ID="btnAddSsl" runat="server" Text="+ Add Certificate" CssClass="credential-primary-button" OnClientClick="openAddAssetModal('sslModal');return false;" /></div></div>
                <div class="credential-table-wrap">
                    <asp:GridView ID="gvSsl" runat="server" ClientIDMode="Static" CssClass="table credential-grid" AutoGenerateColumns="False" GridLines="None" DataKeyNames="SSLID" OnRowCommand="gvSsl_RowCommand" Width="100%">
                        <Columns>
                            <asp:BoundField DataField="Provider" HeaderText="SSL Provider" />
                            <asp:BoundField DataField="CertificateType" HeaderText="Certificate Type" />
                            <asp:BoundField DataField="Expiry" HeaderText="Expiry Date" DataFormatString="{0:dd MMM yyyy}" HtmlEncode="true" />
                            <asp:BoundField DataField="DaysRemaining" HeaderText="Days Remaining" />
                            <asp:TemplateField HeaderText="Status"><ItemTemplate><span class='expiry-status <%# GetExpiryStatusCss(Eval("ExpiryStatus")) %>'><%#: Eval("ExpiryStatus") %></span></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Actions"><ItemTemplate>
                                <asp:LinkButton ID="btnEditSsl" runat="server" CommandName="EditRow" CommandArgument='<%# Eval("SSLID") %>' CssClass="btn btn-sm btn-warning me-1" ToolTip="Edit certificate" CausesValidation="false"><i class="bi bi-pencil-square"></i></asp:LinkButton>
                                <asp:LinkButton ID="btnDeleteSsl" runat="server" CommandName="DeleteRow" CommandArgument='<%# Eval("SSLID") %>' CssClass="btn btn-sm btn-danger" ToolTip="Delete certificate" CausesValidation="false" OnClientClick="return confirm('Delete this SSL certificate?');"><i class="bi bi-trash"></i></asp:LinkButton>
                            </ItemTemplate></asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate><div class="credential-empty"><span class="credential-empty-icon"><i class="bi bi-shield-lock"></i></span>No SSL certificates yet. Add a certificate to track it.</div></EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </section>
</div>
<div class="modal fade credential-modal" id="domainModal" tabindex="-1" aria-labelledby="domainModalTitle" aria-hidden="true"><div class="modal-dialog modal-lg modal-dialog-centered"><div class="modal-content">
    <div class="modal-header"><div><h5 class="modal-title" id="domainModalTitle">Domain Details</h5><div class="credential-toolbar-subtitle">Add registrar, DNS, and expiry information.</div></div><button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button></div>
    <div class="modal-body"><asp:HiddenField ID="hfDomainID" runat="server" /><div class="row g-3">
        <div class="col-md-6"><asp:Label runat="server" AssociatedControlID="txtDomainName" CssClass="form-label" Text="Domain Name" /><asp:TextBox ID="txtDomainName" runat="server" MaxLength="255" CssClass="form-control" placeholder="example.com" /></div>
        <div class="col-md-6"><asp:Label runat="server" AssociatedControlID="txtRegistrar" CssClass="form-label" Text="Registrar" /><asp:TextBox ID="txtRegistrar" runat="server" MaxLength="150" CssClass="form-control" placeholder="e.g. Namecheap" /></div>
        <div class="col-md-6"><asp:Label runat="server" AssociatedControlID="txtDNS" CssClass="form-label" Text="DNS Provider" /><asp:TextBox ID="txtDNS" runat="server" MaxLength="500" CssClass="form-control" placeholder="e.g. Cloudflare" /></div>
        <div class="col-md-6"><asp:Label runat="server" AssociatedControlID="txtDomainExpiry" CssClass="form-label" Text="Expiry Date" /><asp:TextBox ID="txtDomainExpiry" runat="server" TextMode="Date" CssClass="form-control" /></div>
    </div></div>
    <div class="modal-footer"><button type="button" class="btn btn-light" data-bs-dismiss="modal">Cancel</button><asp:Button ID="btnSaveDomain" runat="server" Text="Save Domain" CssClass="btn btn-primary" OnClick="btnSaveDomain_Click" /></div>
</div></div></div>
<div class="modal fade credential-modal" id="sslModal" tabindex="-1" aria-labelledby="sslModalTitle" aria-hidden="true"><div class="modal-dialog modal-lg modal-dialog-centered"><div class="modal-content">
    <div class="modal-header"><div><h5 class="modal-title" id="sslModalTitle">SSL Certificate Details</h5><div class="credential-toolbar-subtitle">Add provider, certificate type, and expiry information.</div></div><button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button></div>
    <div class="modal-body"><asp:HiddenField ID="hfSslID" runat="server" /><div class="row g-3">
        <div class="col-md-6"><asp:Label runat="server" AssociatedControlID="txtSslProvider" CssClass="form-label" Text="SSL Provider" /><asp:TextBox ID="txtSslProvider" runat="server" MaxLength="150" CssClass="form-control" placeholder="e.g. Let's Encrypt" /></div>
        <div class="col-md-6"><asp:Label runat="server" AssociatedControlID="txtCertificateType" CssClass="form-label" Text="Certificate Type" /><asp:TextBox ID="txtCertificateType" runat="server" MaxLength="100" CssClass="form-control" placeholder="e.g. Wildcard" /></div>
        <div class="col-md-6"><asp:Label runat="server" AssociatedControlID="txtSslExpiry" CssClass="form-label" Text="Expiry Date" /><asp:TextBox ID="txtSslExpiry" runat="server" TextMode="Date" CssClass="form-control" /></div>
    </div></div>
    <div class="modal-footer"><button type="button" class="btn btn-light" data-bs-dismiss="modal">Cancel</button><asp:Button ID="btnSaveSsl" runat="server" Text="Save Certificate" CssClass="btn btn-primary" OnClick="btnSaveSsl_Click" /></div>
</div></div></div>
</asp:Content>
