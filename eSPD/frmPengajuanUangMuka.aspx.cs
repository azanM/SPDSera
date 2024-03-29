﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using eSPD.Core;
using System.Drawing;
using System.IO;


using System.Data;
using System.Configuration;
using System.Web.Security;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace eSPD
{
    public partial class frmPengajuanUangMuka : System.Web.UI.Page
    {
        private string strStatusSPD = string.Empty;
        public string LoginID
        {
            get
            {
                object o = Request.QueryString["LoginID"];
                if ((o != null))
                    return Convert.ToString(o);
                else
                    return UserID();
            }
            set { Request.QueryString["LoginID"] = value; }
        }
        private string UserID()
        {
            System.Security.Principal.WindowsIdentity User = null;
            User = System.Web.HttpContext.Current.Request.LogonUserIdentity;
            string UID = null;
            //UID = "anton009190"
            UID = User.Name;
            for (int i = 0; i <= UID.Length - 1; i++)
            {
                if (UID[i] == '\\')
                {
                    UID = UID.Remove(0, i + 1);
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
            return UID;
        }
        msKaryawan karyawan = new msKaryawan();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                //Session["noSPD"] = string.Empty;
                //Session["editable"] = false;
                AD ad = new AD();
                //ArrayList al = ad.Groups("trac\\"+LoginID,true);
                ArrayList o = ad.Groups();

                string strLoginID = string.Empty;
                if (Session["IDLogin"] != null)
                {
                    strLoginID = (string)Session["IDLogin"];
                }
                else
                {
                    strLoginID = SetLabelWelcome();
                }

                if (strLoginID.ToLower() == "wawan010193")
                {
                    //strLoginID = "spd";
                    // strLoginID = "Putu005001";

                    // strLoginID = "arum00003359";
                    //!!!!!! debug Only";
                }
                Session["IDLogin"] = strLoginID;
                classSpd oSPD = new classSpd();
                karyawan = oSPD.getKaryawan(strLoginID);
                btnListFind_Click(null, null);

            }
        }

        public string SetLabelWelcome()
        {
            System.Security.Principal.WindowsIdentity User = null;
            User = System.Web.HttpContext.Current.Request.LogonUserIdentity;
            string username = null;
            //username = "anton009190"
            username = User.Name;
            for (int i = 0; i <= username.Length - 1; i++)
            {
                if (username[i] == '\\')
                {
                    username = username.Remove(0, i + 1);
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
            return username;
        }

        protected void btnListFind_Click(object sender, EventArgs e)
        {
            bindFindList();
        }

        void bindFindList()
        {
            dsSPDDataContext data = new dsSPDDataContext();
            try
            {
                if (txtTglSpd.Text == "" || txtTglSpd == null)
                {
                    var query = (from spd in data.trSPDs
                                 where spd.noSPD.Contains(txtNoSpd.Text) &&
                                 spd.namaLengkap.Contains(txtNama.Text) &&
                                 spd.nrp.Contains(txtNrp.Text) &&  (spd.statusUM.Equals(DropDownList1.SelectedValue) ||
                               spd.statusUM.Equals(null)) 
                                 select new
                                 {
                                     noSPD = spd.noSPD,
                                     nrp = spd.nrp,
                                     namaLengkap = spd.namaLengkap,
                                     uangMuka = spd.uangMuka,
                                     tglPenyelesaian = spd.tglPenyelesaian,
                                     status = spd.statusUM //spd.status
                                 }).OrderByDescending(spd => spd.noSPD).ToList();
                    grvList.DataSource = query;
                    grvList.DataBind();
                    if (!query.Any())
                    {
                        Panel5.Visible = true;

                    }
                    else
                        Panel5.Visible = true;
                    data.Dispose();
                }
                else
                {
                    DateTime str = Convert.ToDateTime(txtTglSpd.Text);
                    var query = (from spd in data.trSPDs
                                 where spd.noSPD.Contains(txtNoSpd.Text) &&
                                 spd.namaLengkap.Contains(txtNama.Text) &&
                                 spd.nrp.Contains(txtNrp.Text) &&
                                (spd.statusUM.Equals(DropDownList1.SelectedValue) ||
                               spd.statusUM.Equals("")) &&
                                 (spd.tglPenyelesaian >= str && spd.tglPenyelesaian <= str)
                                 select new
                                 {
                                     noSPD = spd.noSPD,
                                     nrp = spd.nrp,
                                     namaLengkap = spd.namaLengkap,
                                     uangMuka = spd.uangMuka,
                                     tglPenyelesaian = spd.tglPenyelesaian,
                                     status = spd.statusUM
                                 }).OrderByDescending(spd => spd.noSPD).ToList();
                    grvList.DataSource = query;
                    grvList.DataBind();
                    if (!query.Any())
                    {
                        Panel5.Visible = true;

                    }
                    else
                        Panel5.Visible = true;
                    data.Dispose();
                }

            }
            catch (Exception ex)
            {
                //Response.Write(ex.Message);

            }
        }

        protected void lbApprove_Click(object sender, EventArgs e)
        {
            if (Session["IDLogin"] == null)
            {
                Response.Redirect("frmHome.aspx");
            }
            else
            {
                LinkButton link = (LinkButton)sender;
                GridViewRow gv = (GridViewRow)(link.NamingContainer);
                string strNoSpd = gv.Cells[0].Text;
                string status = gv.Cells[7].Text; //.Split('-')[0];
                Session["noSPD"] = strNoSpd;
                if (status.ToLower() == "save" || status == "SPD Perlu Revisi (Tujuan)" || status == "SPD Perlu Revisi (Atasan)")
                    Session["editable"] = true;
                else
                    Session["editable"] = false;
                Response.Redirect("frmRequestInput.aspx");
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtNoSpd.Text = string.Empty;
            txtNrp.Text = string.Empty;
            txtNama.Text = string.Empty;
            txtTglSpd.Text = string.Empty;
            txtTglSpdEnd.Text = string.Empty;
            btnListFind_Click(null, null);
        }

        protected void btnExportToExcel_Click(object sender, EventArgs e)
        {
            btnListFind_Click(null, null);
            ExportToExcel("Report.xls", grvList);

        }
        private void ExportToExcel(string strFileName, GridView gv)
        {
            Response.ClearContent();
            Response.AddHeader("content-disposition", "attachment; filename=" + strFileName);
            Response.ContentType = "application/excel";
            System.IO.StringWriter sw = new System.IO.StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);
            gv.RenderControl(htw);
            Response.Write(sw.ToString());
            Response.End();
        }
        public override void VerifyRenderingInServerForm(Control control)
        {
            /* Verifies that the control is rendered */
        }

        protected void gvList_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {


                var statusUM = DataBinder.Eval(e.Row.DataItem, "status");
                LinkButton lbApprove = (LinkButton)e.Row.FindControl("lbApprove");
                if (statusUM == "approve")
                {


                    lbApprove.Visible = false;

                }
                else lbApprove.Visible = false;


            }
        }
        protected void gvList_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Approve")
            {
                using (var ctx = new dsSPDDataContext())
                {
                    var dataSpd = ctx.trSPDs.FirstOrDefault(o => o.noSPD == e.CommandArgument.ToString());
                    dataSpd.tglPenyelesaian = DateTime.Now;
                    dataSpd.statusUM = "approve";

                    ctx.SubmitChanges();
                 
                }
            }

        }

        protected void gvList_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {

            btnListFind_Click(null, null);
        }
    }
}