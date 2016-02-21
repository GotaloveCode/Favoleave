using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Leave.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Leave.Reports.CrystalViewer
{
    public partial class LeaveList2 : System.Web.UI.Page
    {
        private LeaveEntities db = new LeaveEntities();

        ReportDocument reportDocument = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Page.IsPostBack)
            //{
                LoadReport();
            //}
            //else
            //{
            //    DataTable periods = new DataTable();

            //    EntityConnection ec = new EntityConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveEntities"].ConnectionString);
            //    SqlConnection sqlConn = ec.StoreConnection as SqlConnection;
            //    using (SqlConnection con = new SqlConnection(sqlConn.ConnectionString))
            //    {
            //        try
            //        {
            //            SqlDataAdapter adapter = new SqlDataAdapter("SELECT id, startdate FROM leave_period", con);
            //            adapter.Fill(periods);

            //            Perioddropdown.DataSource = periods;
            //            Perioddropdown.DataTextField = "startdate";
            //            Perioddropdown.DataValueField = "id";
            //            Perioddropdown.DataBind();
            //        }
            //        catch (Exception ex)
            //        {
            //            // Handle the error
            //        }

            //    }
            //}

        }
        protected void Preview_Click(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                LoadReport();
            }
        }
        private void LoadReport()
        {
            if (this.reportDocument != null)
            {
                this.reportDocument.Close();
                this.reportDocument.Dispose();
            }
            EntityConnection ec = new EntityConnection(System.Configuration.ConfigurationManager.ConnectionStrings["LeaveEntities"].ConnectionString);
            SqlConnection sqlConn = ec.StoreConnection as SqlConnection;
            SqlConnectionStringBuilder SConn = new SqlConnectionStringBuilder(sqlConn.ConnectionString);
            var leavelist = db.leaves.Select(l => new
            {
                l.Employee,
                l.leave_type,
                l.leave_period
            }).ToList();

            reportDocument = new ReportDocument();
            string reportPath = Server.MapPath("~/Reports/Crystal/LeaveCumulative.rpt");
            reportDocument.Load(reportPath);
            // Report connection
            ConnectionInfo connInfo = new ConnectionInfo();
            connInfo.ServerName = SConn.DataSource;
            connInfo.DatabaseName = SConn.InitialCatalog;
            connInfo.UserID = SConn.UserID;
            connInfo.Password = SConn.Password;
            TableLogOnInfo tableLogOnInfo = new TableLogOnInfo();
            tableLogOnInfo.ConnectionInfo = connInfo;
            foreach (CrystalDecisions.CrystalReports.Engine.Table table in reportDocument.Database.Tables)
            {
                table.ApplyLogOnInfo(tableLogOnInfo);
                table.LogOnInfo.ConnectionInfo.ServerName = connInfo.ServerName;
                table.LogOnInfo.ConnectionInfo.DatabaseName = connInfo.DatabaseName;
                table.LogOnInfo.ConnectionInfo.UserID = connInfo.UserID;
                table.LogOnInfo.ConnectionInfo.Password = connInfo.Password;
                table.Location = "dbo." + table.Location;
            }

            LeaveCumulative.RefreshReport();
            //reportDocument.RecordSelectionFormula = "{leave_period.id} = " + Perioddropdown.SelectedValue.ToString();
            LeaveCumulative.HasRefreshButton = true;
            LeaveCumulative.ReportSource = reportDocument;
            LeaveCumulative.DataBind();


        }
    }
}