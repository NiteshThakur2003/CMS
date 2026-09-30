<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="CredentialManagementPortal.Dashboard" %>
<asp:Content ID="DashboardContent" ContentPlaceHolderID="MainContent" runat="server">
<style>
 .body-content{width:100%;max-width:none;padding:0 12px} .body-content>hr, .body-content>footer{display:none}
.cms-dashboard{display:flex;min-height:calc(100vh - 112px);margin:0 -12px;background:#f4f6f9;color:#172b4d;border:1px solid #e3e8ef;border-radius:8px;overflow:hidden;font-family:Arial,sans-serif}
.cms-content{flex:1;min-width:0}.cms-topbar{height:62px;background:#fff;border-bottom:1px solid #e7ebf0;display:flex;align-items:center;justify-content:flex-end;gap:20px;padding:0 24px}.cms-search{width:min(300px,45vw);position:relative}.cms-search input{width:100%;border:1px solid #e0e5eb;background:#fbfcfd;border-radius:7px;padding:9px 12px 9px 34px;font-size:13px}.cms-search span{position:absolute;left:11px;top:7px;color:#8995a5}.cms-user{display:flex;align-items:center;gap:10px;font-size:13px}.cms-avatar{width:34px;height:34px;border-radius:50%;display:grid;place-items:center;background:#e4efff;color:#2367b1;font-weight:700}.cms-user small{display:block;color:#8995a5;font-size:11px;margin-top:2px}.user-menu-wrap{position:relative}.user-menu{position:absolute;right:0;top:42px;min-width:140px;padding:6px;background:#fff;border:1px solid #e1e6ed;border-radius:8px;box-shadow:0 8px 24px rgba(25,45,75,.16);z-index:20}.user-menu a{display:block;padding:9px 11px;border-radius:5px;color:#344563;text-decoration:none;font-size:13px}.user-menu a:hover{background:#f2f6fb;color:#0d6efd}
.cms-main{padding:25px 26px 30px}.cms-page-title{font-size:24px;font-weight:700;margin:0 0 20px}.metric-grid{display:grid;grid-template-columns:repeat(4,minmax(0,1fr));gap:14px}.metric-card{background:#fff;border:1px solid #e5e9ef;border-radius:9px;padding:17px 18px;min-height:112px;box-shadow:0 2px 5px rgba(30,50,75,.03)}.metric-title{font-size:13px;font-weight:600;color:#44546a}.metric-row{display:flex;align-items:center;justify-content:space-between;margin:10px 0 5px}.metric-value{font-size:25px;font-weight:700;color:#172b4d}.metric-icon{width:34px;height:34px;border-radius:8px;display:grid;place-items:center;font-weight:700;font-size:15px}.metric-card a{font-size:12px;color:#2878d0;text-decoration:none}.metric-total .metric-icon{color:#c4505a;background:#fff0f1}.metric-vpn .metric-icon{color:#3184c6;background:#eaf5ff}.metric-ax .metric-icon{color:#159675;background:#e8f8f2}.metric-email .metric-icon{color:#d48b24;background:#fff5e5}.metric-product .metric-icon{color:#7654c7;background:#f1edff}.metric-domain .metric-icon{color:#3184c6;background:#eaf5ff}.metric-ssl .metric-icon{color:#159675;background:#e8f8f2}.metric-domain .metric-icon{color:#3184c6;background:#eaf5ff}.metric-ssl .metric-icon{color:#159675;background:#e8f8f2}
.lower-grid{display:grid;grid-template-columns:1.15fr 1fr;gap:15px;margin-top:17px}.panel{background:#fff;border:1px solid #e5e9ef;border-radius:9px;padding:18px}.panel h2{font-size:15px;font-weight:700;margin:0 0 14px}.quick-link{display:flex;align-items:center;gap:12px;padding:12px 0;border-bottom:1px solid #edf0f3;color:#344563;text-decoration:none;font-size:13px}.quick-link:last-child{border-bottom:0;padding-bottom:2px}.quick-link:hover{color:#2878d0}.quick-icon{width:30px;height:30px;border-radius:7px;background:#f1f5fa;display:grid;place-items:center;font-weight:700;color:#52677d}.quick-link small{display:block;color:#8995a5;margin-top:3px}.quick-arrow{margin-left:auto;color:#8995a5}.panel-note{font-size:12px;color:#8793a2;line-height:1.5;margin:0}.panel-note+.panel-note{margin-top:12px}.metric-hint{font-size:11px;color:#8995a5;margin-top:3px}
.expiring-panel{padding:0 18px}.expiring-heading{display:flex;align-items:flex-start;justify-content:space-between;gap:12px;padding:18px 0 13px;border-bottom:1px solid #edf0f3}.expiring-heading h2{margin:0}.expiring-heading p{margin:5px 0 0;color:#8995a5;font-size:12px}.expiring-window{padding:5px 8px;border-radius:20px;background:#f1f5f9;color:#64748b;font-size:10px;font-weight:700;white-space:nowrap}.expiring-row{display:flex;align-items:center;gap:11px;padding:12px 0;border-bottom:1px solid #edf0f3}.expiring-row:last-child{border-bottom:0}.expiring-icon{width:31px;height:31px;flex:0 0 31px;display:grid;place-items:center;border-radius:8px;background:#eef4ff;color:#2878d0}.expiring-domain{background:#eef4ff;color:#2878d0}.expiring-ssl{background:#eaf8f3;color:#159675}.expiring-copy{min-width:0;flex:1}.expiring-copy strong{display:block;overflow:hidden;color:#344563;font-size:12px;font-weight:650;text-overflow:ellipsis;white-space:nowrap}.expiring-copy small{display:flex;align-items:center;gap:6px;margin-top:4px;color:#8995a5;font-size:11px}.expiring-kind{font-size:9px;font-weight:750;letter-spacing:.04em;text-transform:uppercase}.expiring-kind-domain{color:#2878d0}.expiring-kind-ssl{color:#159675}.expiring-date{text-align:right;white-space:nowrap}.expiring-date time{display:block;color:#526176;font-size:11px;font-weight:600}.expiring-date small{display:block;margin-top:3px;color:#b7791f;font-size:10px}.activity-panel{padding:0 18px}.activity-heading{display:flex;align-items:flex-start;justify-content:space-between;gap:12px;padding:18px 0 13px;border-bottom:1px solid #edf0f3}.activity-heading h2{margin:0}.activity-heading p{margin:5px 0 0;color:#8995a5;font-size:12px}.activity-live{display:inline-flex;align-items:center;gap:6px;padding:5px 8px;border-radius:20px;background:#ecfdf3;color:#13834b;font-size:10px;font-weight:700}.activity-live i{width:6px;height:6px;border-radius:50%;background:#20a464}.activity-row{display:flex;align-items:center;gap:11px;padding:13px 0;border-bottom:1px solid #edf0f3}.activity-row:last-child{border-bottom:0}.activity-type{width:32px;height:32px;flex:0 0 32px;display:grid;place-items:center;border-radius:8px;background:#eef4ff;color:#2878d0;font-size:11px;font-weight:700}.activity-email{background:#fff4e5;color:#c47a13}.activity-ax{background:#eaf8f3;color:#159675}.activity-copy{min-width:0;flex:1}.activity-copy strong{display:block;overflow:hidden;color:#344563;font-size:12px;font-weight:650;text-overflow:ellipsis;white-space:nowrap}.activity-copy small{display:block;margin-top:3px;color:#8995a5;font-size:11px}.activity-row time{color:#8290a2;font-size:10px;white-space:nowrap}.activity-empty{padding:25px 0;text-align:center;color:#8995a5;font-size:12px}
@media(max-width:950px){.metric-grid{grid-template-columns:repeat(2,minmax(0,1fr))}.lower-grid{grid-template-columns:1fr}}
@media(max-width:620px){.cms-dashboard{display:block}.cms-topbar{height:54px;padding:0 14px}.cms-main{padding:20px 14px}.metric-grid{gap:9px}.metric-card{padding:13px;min-height:105px}.metric-title{font-size:12px}.metric-value{font-size:22px}.metric-icon{width:30px;height:30px}}
.metric-icon svg{width:19px;height:19px;fill:none;stroke:currentColor;stroke-width:1.7;stroke-linecap:round;stroke-linejoin:round}
</style>
<script type="text/javascript">
    function toggleUserMenu(button) {
        var menu = document.getElementById("userMenu");
        var isOpen = menu.style.display === "block";
        menu.style.display = isOpen ? "none" : "block";
        button.setAttribute("aria-expanded", isOpen ? "false" : "true");
    }
</script>
<div class="cms-dashboard">
<div class="cms-content">
        <header class="cms-topbar">
           
           
            <div class="cms-user"><div><strong>Manish</strong><small>Administrator</small></div><div class="user-menu-wrap"><button type="button" class="cms-avatar" aria-label="Open account menu" aria-expanded="false" onclick="toggleUserMenu(this)">M</button><div id="userMenu" class="user-menu" style="display:none"><asp:LinkButton ID="btnLogout" runat="server" OnClick="btnLogout_Click">Logout</asp:LinkButton></div></div></div>
        </header>
        <main class="cms-main">
            <h1 class="cms-page-title">Dashboard</h1>
            <section class="metric-grid" aria-label="Credential totals">
                <article class="metric-card metric-total"><div class="metric-title">Total Records</div><div class="metric-row"><asp:Label ID="lblTotalCount" runat="server" CssClass="metric-value" Text="ï¿½" /><span class="metric-icon" aria-hidden="true"><svg viewBox="0 0 24 24"><rect x="4" y="4" width="16" height="5" rx="1.5"/><rect x="4" y="11" width="16" height="5" rx="1.5"/><path d="M7 18.5h10"/></svg></span></div><div class="metric-hint">Across all credential types</div></article>
                <article class="metric-card metric-vpn"><div class="metric-title">VPN Logins</div><div class="metric-row"><asp:Label ID="lblVpnCount" runat="server" CssClass="metric-value" Text="ï¿½" /><span class="metric-icon" aria-hidden="true"><svg viewBox="0 0 24 24"><path d="M12 2.8 19.5 6v5.1c0 5-3.2 8.3-7.5 10.1-4.3-1.8-7.5-5.1-7.5-10.1V6L12 2.8Z"/><path d="m9.5 12 1.7 1.7 3.5-3.8"/></svg></span></div><a runat="server" href="~/VPNLoingDetails.aspx">View all</a></article>
                <article class="metric-card metric-ax"><div class="metric-title">AX Logins</div><div class="metric-row"><asp:Label ID="lblAxCount" runat="server" CssClass="metric-value" Text="ï¿½" /><span class="metric-icon">AX</span></div><a runat="server" href="~/AXLoginDetails.aspx">View all</a></article>
                <article class="metric-card metric-email"><div class="metric-title">Email Accounts</div><div class="metric-row"><asp:Label ID="lblEmailCount" runat="server" CssClass="metric-value" Text="ï¿½" /><span class="metric-icon" aria-hidden="true"><svg viewBox="0 0 24 24"><rect x="3" y="5" width="18" height="14" rx="2"/><path d="m4 7 8 6 8-6"/></svg></span></div><a runat="server" href="~/EmailAccountDetails.aspx">View all</a></article>
                <article class="metric-card metric-product"><div class="metric-title">Product Keys</div><div class="metric-row"><asp:Label ID="lblProductKeyCount" runat="server" CssClass="metric-value" Text="0" /><span class="metric-icon" aria-hidden="true"><svg viewBox="0 0 24 24"><circle cx="8" cy="15" r="4"/><path d="m11 12 8-8 2 2-2 2 2 2-3 3-2-2-2 2"/></svg></span></div><a runat="server" href="~/ProductKeys.aspx">View all</a></article>
                <article class="metric-card metric-domain"><div class="metric-title">Domains</div><div class="metric-row"><asp:Label ID="lblDomainCount" runat="server" CssClass="metric-value" Text="0" /><span class="metric-icon" aria-hidden="true"><svg viewBox="0 0 24 24"><circle cx="12" cy="12" r="9"/><path d="M3 12h18M12 3a14 14 0 0 1 0 18M12 3a14 14 0 0 0 0 18"/></svg></span></div><a runat="server" href="~/DomainAndSSL.aspx?tab=domains">View all</a></article>
                <article class="metric-card metric-ssl"><div class="metric-title">SSL Certificates</div><div class="metric-row"><asp:Label ID="lblSslCount" runat="server" CssClass="metric-value" Text="0" /><span class="metric-icon" aria-hidden="true"><svg viewBox="0 0 24 24"><path d="M12 2.8 19.5 6v5.1c0 5-3.2 8.3-7.5 10.1-4.3-1.8-7.5-5.1-7.5-10.1V6L12 2.8Z"/><path d="M12 9v4m0 3h.01"/></svg></span></div><a runat="server" href="~/DomainAndSSL.aspx?tab=ssl">View all</a></article>
            </section>
            <section class="lower-grid">
                <div class="panel expiring-panel"><div class="expiring-heading"><div><h2>Expiring Soon</h2><p>Domains and SSL certificates due within 90 days</p></div><span class="expiring-window">90 days</span></div>
                    <asp:Repeater ID="rptExpiringSoon" runat="server"><ItemTemplate>
                        <div class="expiring-row"><span class='expiring-icon expiring-<%# Eval("AssetTypeKey") %>'><i class='<%# Eval("AssetIcon") %>'></i></span><div class="expiring-copy"><strong><%#: Eval("AssetName") %></strong><small><span class="expiring-kind expiring-kind-<%# Eval("AssetTypeKey") %>"><%#: Eval("AssetTypeLabel") %></span><span><%#: Eval("AssetSubtitle") %></span></small></div><div class="expiring-date"><time><%# Eval("Expiry", "{0:dd MMM yyyy}") %></time><small><%#: GetExpiryText(Eval("DaysRemaining")) %></small></div></div>
                    </ItemTemplate></asp:Repeater>
                    <asp:Label ID="lblExpiringEmpty" runat="server" CssClass="activity-empty" Visible="false" Text="No domains or SSL certificates expire within the next 90 days." />
                </div>
                <div class="panel activity-panel"><div class="activity-heading"><div><h2>Recent Activity</h2><p>Latest credential records across your workspace</p></div><span class="activity-live"><i></i> Live</span></div>
                    <asp:Repeater ID="rptRecentActivity" runat="server"><ItemTemplate>
                        <div class="activity-row"><span class='activity-type activity-<%# Eval("ActivityKey") %>'><%# Eval("ActivityIcon") %></span><div class="activity-copy"><strong><%# Eval("ActivityName") %></strong><small><%# Eval("ActivityType") %></small></div><time><%# Eval("CreatedDate", "{0:MMM d, yyyy h:mm tt}") %></time></div>
                    </ItemTemplate></asp:Repeater>
                    <asp:Panel ID="pnlNoRecentActivity" runat="server" CssClass="activity-empty" Visible="false">No recent records to show.</asp:Panel>
                </div>
            </section>
        </main>
    </div>
</div>
</asp:Content>












