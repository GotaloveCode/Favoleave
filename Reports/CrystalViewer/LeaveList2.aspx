<%@ Page Language="C#" MasterPageFile="~/Views/Shared/ReportSite.Master"  AutoEventWireup="true" CodeBehind="LeaveList2.aspx.cs" Inherits="Leave.Reports.CrystalViewer.LeaveList2" %>

<%@ Register Assembly="CrystalDecisions.Web, Version=13.0.2000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304" Namespace="CrystalDecisions.Web" TagPrefix="CR" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">        
<CR:CrystalReportViewer ID="LeaveCumulative" runat="server" HasCrystalLogo="False" AutoDataBind="true"   EnableParameterPrompt="false" EnableDatabaseLogonPrompt="false" 
     ToolPanelView="None" ></CR:CrystalReportViewer>
     </div> 
</asp:Content>
