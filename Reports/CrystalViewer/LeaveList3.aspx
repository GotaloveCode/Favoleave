<%@ Page Language="C#" MasterPageFile="~/Views/Shared/ReportSite.Master"  AutoEventWireup="true" CodeBehind="LeaveList3.aspx.cs" Inherits="Leave.Reports.CrystalViewer.LeaveList3" %>

<%@ Register Assembly="CrystalDecisions.Web, Version=13.0.2000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304" Namespace="CrystalDecisions.Web" TagPrefix="CR" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
     <div class="container">
         <asp:Label ID="Label2" runat="server" Text="Period "></asp:Label>
    <asp:DropDownList ID="Perioddropdown" runat="server"></asp:DropDownList>
         <asp:Label ID="Label1" runat="server" Text="Employee"></asp:Label>
    <asp:DropDownList ID="employeedropdown" runat="server"></asp:DropDownList>
    <asp:Button  Text="Preview"  runat="server" ID="Preview" ValidationGroup="view" type="submit"   OnClick="Preview_Click" CssClass="btn-primary" />
<CR:CrystalReportViewer ID="LeaveByName" runat="server" HasCrystalLogo="False" AutoDataBind="true"  EnableParameterPrompt="false" EnableDatabaseLogonPrompt="false"  
     ToolPanelView="None" ></CR:CrystalReportViewer>
        </div>

</asp:Content>
