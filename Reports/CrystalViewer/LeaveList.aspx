<%@ Page Language="C#" MasterPageFile="~/Views/Shared/ReportSite.Master"  AutoEventWireup="true" CodeBehind="LeaveList.aspx.cs" Inherits="Leave.Reports.CrystalViewer.LeaveList" %>

<%@ Register Assembly="CrystalDecisions.Web, Version=13.0.2000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304" Namespace="CrystalDecisions.Web" TagPrefix="CR" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
    <asp:Button  Text="Preview"  runat="server" ID="Preview" ValidationGroup="view" type="submit"   OnClick="Preview_Click" />
    <asp:DropDownList ID="Perioddropdown" runat="server"></asp:DropDownList>
<CR:CrystalReportViewer ID="LeaveListCrystalReport" runat="server" HasCrystalLogo="False" AutoDataBind="true"  EnableParameterPrompt="false" EnableDatabaseLogonPrompt="false"  
     ToolPanelView="None" ></CR:CrystalReportViewer>
        </div>
</asp:Content>