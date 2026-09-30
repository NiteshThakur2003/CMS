<%@ Page Title="Sign In" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="CredentialManagementPortal.Login" %>
<asp:Content ID="LoginContent" ContentPlaceHolderID="MainContent" runat="server">
<style>
.body-content{width:100%;max-width:none;padding:0;margin:0}.body-content>hr,.body-content>footer{display:none}
.login-page{position:relative;isolation:isolate;min-height:100vh;display:flex;align-items:center;justify-content:center;padding:32px 18px;overflow:hidden;background:#0871d5}
.login-background{position:absolute;inset:0;width:100%;height:100%;object-fit:cover;z-index:-2}
.login-page:after{content:"";position:absolute;inset:0;background:rgba(4,27,58,.42);z-index:-1}
.login-card{position:relative;z-index:1;width:100%;max-width:410px;background:#fff;box-sizing:border-box;border:1px solid rgba(255,255,255,.75);border-radius:14px;padding:38px 34px;box-shadow:0 18px 50px rgba(5,30,73,.28)}
.login-card h1{font-size:25px;font-weight:700;color:#172b4d;text-align:center;margin:0 0 8px}.login-subtitle{color:#78869a;font-size:13px;text-align:center;margin:0 0 28px}
.login-label{font-size:12px;font-weight:700;color:#35465e;margin-bottom:7px}.login-input{height:44px;border:1px solid #dfe5ec;border-radius:6px;font-size:13px}.login-input:focus{border-color:#367fe5;box-shadow:0 0 0 3px rgba(54,127,229,.12)}.login-button{height:44px;border-radius:6px;font-weight:600;font-size:14px;margin-top:8px;background:#2875e5;border-color:#2875e5}
.login-error{display:block;color:#b42318;background:#fff2f0;border-radius:7px;padding:10px 12px;margin-top:16px;font-size:13px}.login-lock{color:#8592a3;text-align:center;font-size:11px;margin-top:30px}
@media(max-width:480px){.login-page{padding:20px 14px}.login-card{padding:30px 23px}}
</style>
<div class="login-page">
    <asp:Image ID="imgLoginBackground" runat="server" ImageUrl="~/Content/login-background.svg" CssClass="login-background" AlternateText="" />
    <section class="login-card" aria-labelledby="loginTitle">
        <h1 id="loginTitle">Welcome Back</h1>
        <p class="login-subtitle">Sign in to continue to your account.</p>
        <div class="mb-3">
            <asp:Label ID="lblUsername" runat="server" AssociatedControlID="txtUsername" CssClass="form-label login-label" Text="Username" />
            <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control login-input" placeholder="Enter your username" autocomplete="username" />
        </div>
        <div class="mb-3">
            <asp:Label ID="lblPassword" runat="server" AssociatedControlID="txtPassword" CssClass="form-label login-label" Text="Password" />
            <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control login-input" TextMode="Password" placeholder="Enter your password" autocomplete="current-password" />
        </div>
        <asp:RequiredFieldValidator ID="valUsername" runat="server" ControlToValidate="txtUsername" ErrorMessage="Enter your username." CssClass="text-danger small mb-2" Display="Dynamic" ValidationGroup="Login" />
        <asp:RequiredFieldValidator ID="valPassword" runat="server" ControlToValidate="txtPassword" ErrorMessage="Enter your password." CssClass="text-danger small mb-2" Display="Dynamic" ValidationGroup="Login" />
        <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="btn btn-primary w-100 login-button" OnClick="btnLogin_Click" ValidationGroup="Login" />
        <asp:Label ID="lblMessage" runat="server" Visible="false" CssClass="login-error" role="alert" />
        <div class="login-lock">© <%: DateTime.Now.Year %> Credential Management System</div>
    </section>
</div>
</asp:Content>



