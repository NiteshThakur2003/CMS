<%@ Page Title="Product Keys" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ProductKeys.aspx.cs" Inherits="CredentialManagementPortal.ProductKeys" %>
<asp:Content ID="ProductKeysContent" ContentPlaceHolderID="MainContent" runat="server">
<link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
<script type="text/javascript">
    function openProductKeyModal() { bootstrap.Modal.getOrCreateInstance(document.getElementById('productKeyModal')).show(); }
    function openAddProductKeyModal() {
        var modal = document.getElementById('productKeyModal');
        modal.querySelectorAll('input[type="text"],input[type="password"],input[type="hidden"]').forEach(function (field) { field.value = ''; });
        openProductKeyModal();
    }
    function filterProductKeys(input) {
        var term = input.value.toLowerCase();
        document.querySelectorAll('#gvProductKeys tr').forEach(function (row) {
            if (row.querySelector('th')) return;
            row.style.display = row.textContent.toLowerCase().indexOf(term) >= 0 ? '' : 'none';
        });
    }
</script>
<div class="credential-page">
    <header class="credential-page-header">
        <div><div class="credential-eyebrow"><span></span> CREDENTIAL LIBRARY</div><h1 class="credential-page-title">Product Keys</h1><p class="credential-page-subtitle">Manage software licenses and assigned users.</p></div>
        <asp:Button ID="btnAdd" runat="server" Text="+ Add Product Key" CssClass="credential-primary-button" OnClientClick="openAddProductKeyModal();return false;" />
    </header>
    <section class="credential-card">
        <div class="credential-toolbar"><div><h2 class="credential-toolbar-title">Saved product licenses</h2><p class="credential-toolbar-subtitle">Product key values are hidden in this list.</p></div><input class="credential-search" type="search" placeholder="Search software, version, or assignee..." aria-label="Search product keys" oninput="filterProductKeys(this)" /></div>
        <div class="credential-table-wrap">
            <asp:GridView ID="gvProductKeys" runat="server" ClientIDMode="Static" CssClass="table credential-grid" AutoGenerateColumns="False" GridLines="None" DataKeyNames="ProductKeyID" OnRowCommand="gvProductKeys_RowCommand" Width="100%">
                <Columns>
                    <asp:BoundField DataField="Software" HeaderText="Software" />
                    <asp:BoundField DataField="Version" HeaderText="Version" />
                    <asp:TemplateField HeaderText="Product Key"><ItemTemplate><span class="credential-secret" aria-label="Product key hidden">••••••••</span></ItemTemplate></asp:TemplateField>
                    <asp:BoundField DataField="LicenseType" HeaderText="License Type" />
                    <asp:BoundField DataField="AssignedTo" HeaderText="Assigned To" />
                    <asp:BoundField DataField="CreatedDate" HeaderText="Created" DataFormatString="{0:MMM d, yyyy}" HtmlEncode="true" />
                    <asp:BoundField DataField="UpdatedDate" HeaderText="Updated" DataFormatString="{0:MMM d, yyyy}" HtmlEncode="true" />
                    <asp:TemplateField HeaderText="Actions"><ItemTemplate>
                        <asp:LinkButton ID="btnEdit" runat="server" CommandName="EditRow" CommandArgument='<%# Eval("ProductKeyID") %>' CssClass="btn btn-sm btn-warning me-1" ToolTip="Edit product key" CausesValidation="false"><i class="bi bi-pencil-square"></i></asp:LinkButton>
                        <asp:LinkButton ID="btnDelete" runat="server" CommandName="DeleteRow" CommandArgument='<%# Eval("ProductKeyID") %>' CssClass="btn btn-sm btn-danger" ToolTip="Delete product key" CausesValidation="false" OnClientClick="return confirm('Delete this product key?');"><i class="bi bi-trash"></i></asp:LinkButton>
                    </ItemTemplate></asp:TemplateField>
                </Columns>
                <EmptyDataTemplate><div class="credential-empty"><span class="credential-empty-icon"><i class="bi bi-key"></i></span>No product keys yet. Use “Add Product Key” to create one.</div></EmptyDataTemplate>
            </asp:GridView>
        </div>
    </section>
</div>
<div class="modal fade credential-modal" id="productKeyModal" tabindex="-1" aria-labelledby="productKeyModalTitle" aria-hidden="true"><div class="modal-dialog modal-lg modal-dialog-centered"><div class="modal-content">
    <div class="modal-header"><div><h5 class="modal-title" id="productKeyModalTitle">Product Key Details</h5><div class="credential-toolbar-subtitle">Add software and license assignment information.</div></div><button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button></div>
    <div class="modal-body"><asp:HiddenField ID="hfID" runat="server" /><div class="row g-3">
        <div class="col-md-6"><asp:Label runat="server" AssociatedControlID="txtSoftware" CssClass="form-label" Text="Software" /><asp:TextBox ID="txtSoftware" runat="server" MaxLength="200" CssClass="form-control" /></div>
        <div class="col-md-6"><asp:Label runat="server" AssociatedControlID="txtVersion" CssClass="form-label" Text="Version" /><asp:TextBox ID="txtVersion" runat="server" MaxLength="100" CssClass="form-control" /></div>
        <div class="col-md-12"><asp:Label runat="server" AssociatedControlID="txtProductKey" CssClass="form-label" Text="Product Key" /><asp:TextBox ID="txtProductKey" runat="server" TextMode="Password" MaxLength="500" CssClass="form-control" autocomplete="new-password" /><small class="credential-field-hint">When editing, leave this blank to keep the existing key.</small></div>
        <div class="col-md-6"><asp:Label runat="server" AssociatedControlID="txtLicenseType" CssClass="form-label" Text="License Type" /><asp:TextBox ID="txtLicenseType" runat="server" MaxLength="100" CssClass="form-control" /></div>
        <div class="col-md-6"><asp:Label runat="server" AssociatedControlID="txtAssignedTo" CssClass="form-label" Text="Assigned To" /><asp:TextBox ID="txtAssignedTo" runat="server" MaxLength="150" CssClass="form-control" /></div>
    </div></div>
    <div class="modal-footer"><button type="button" class="btn btn-light" data-bs-dismiss="modal">Cancel</button><asp:Button ID="btnSave" runat="server" Text="Save Product Key" CssClass="btn btn-primary" OnClick="btnSave_Click" /></div>
</div></div></div>
</asp:Content>

