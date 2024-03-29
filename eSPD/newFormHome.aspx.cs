﻿using eSPD.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace eSPD
{
    public partial class newFormHome : System.Web.UI.Page
    {
        private static msKaryawan karyawan = new msKaryawan();
        private static classSpd oSPD = new classSpd();
        private static string strID = string.Empty;
        private static ApprovalSPDUrl approvalSPDUrl = new ApprovalSPDUrl();

        private static ClaimApprovalUrl claimApproval = new ClaimApprovalUrl();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["IDLogin"] == null)
            {
                Response.Redirect("frmHome.aspx");
            }


            strID = (string)Session["IDLogin"];
            karyawan = oSPD.getKaryawan(strID);
            Session["nrpLogin"] = karyawan.nrp;
            
            if (string.IsNullOrEmpty(karyawan.nrp)) Response.Redirect("frmHome.aspx");

            if (!IsPostBack)
            {
                if (ifga(karyawan.nrp))
                {
                    bindFindSPDGa();
                    spdGA.Visible = true;
                    BindAtasan();
                }
                bindADH();
                bindFind();
                bindFindTujuan();
                BindAtasan();

                if (Session["IDLogin"].ToString() == "siska90000308")
                    PnlChange.Visible = true;
            }
        }

        public void bindADH()
        {
            if (ifADH(karyawan.nrp))
            {
                using (var ctx = new dsSPDDataContext())
                {
                    string costCenter = string.Empty;
                    DataTable dtReference = new DataTable();
                    string filter = !string.IsNullOrEmpty(txtTglBerangkat.Text.Trim()) ? "tglBerangkat" : string.Empty;
                    string param = txtTglBerangkat.Text;
                    var dataADH = ctx.sp_GetApprovalSPDADH(karyawan.nrp, filter, param).ToList();
                    
                    dataADH = dataADH.Where(x => x.IndexLevel == 0).ToList();
                    if (dataADH.Count > 0)
                    {
                        DataTable dataSource = new DataTable();
                        DataTable dtNoSPD = new DataTable(); dtNoSPD.Columns.Add("NomorSPD", typeof(string));
                        foreach (var item in dataADH)
                        {
                            dtNoSPD.Rows.Add(item.NoSPD);
                        }
                        var listSPD = (from data in dataADH
                                      select new trSPD
                                      {
                                          noSPD = data.NoSPD,
                                          costCenter = data.costCenter
                                      }).ToList();
                        
                        foreach (var item in listSPD)
                        {
                            if (item.costCenter != costCenter)
                            {
                                var result = ESPDInformation.CheckBudgetToSAP(item);
                                ESPDInformation.PerhitunganBiayaSPDByBudget(result, dataSource, dtNoSPD);
                                costCenter = item.costCenter;
                            }
                        }
                        ViewADH.DataSource = dataSource;
                        ViewADH.DataBind();
                        ViewADH.Visible = true;
                    }
                    if (!dataADH.Any())
                    {
                        ListBudgetCostCenter.Visible = false;
                        ViewADH.Visible = false;
                        Label1.Text = "No Data";
                    }
                    else
                    {
                        ViewADH.Visible = true;
                    }
                }
            }
            else
            {
                Label1.Text = "No Data"; Label1.Visible = true;
                ViewADH.DataBind();
            }
        }

        #region check if login ga
        private bool ifga(string nrp)
        {
            bool returner = false;

            using (var ctx = new dsSPDDataContext())
            {
                try
                {
                    int role = ctx.msUsers.First(o => o.nrp.Equals(nrp)).roleId;
                    if (role == 17) returner = true;
                }
                catch (Exception)
                {
                    returner = false;
                }
            }

            return returner;
        }

        private bool ifADH(string nrp)
        {
            bool IsADH = false;
            String CostCenterId = String.Empty;
            try
            {
                SqlCommand cmd = new SqlCommand("select top 1 * from msADH where nrp = " + int.Parse(nrp) + "", new SqlConnection(ConfigurationManager.AppSettings["SPDConnectionString1"]));
                cmd.Connection.Open();
                SqlDataReader Reader;
                Reader = cmd.ExecuteReader();
                while (Reader.Read())
                {
                    CostCenterId = Reader["costcenterId"].ToString();
                }
                if (!String.IsNullOrEmpty(CostCenterId))
                    IsADH = true;
                cmd.Connection.Dispose();
                cmd.Connection.Close();
            }
            catch (Exception)
            {
                return false;
            }
            return IsADH;
        }

        #endregion

        #region approvalAtasan
        protected void gvViewSPD_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            string paramField = DropDownList1.Text.Trim() == "--Select--" ? "" : DropDownList1.Text.Trim();
            string a = DropDownList1.SelectedValue;


            if (a == "No")
            {
                using (var ctx = new dsSPDDataContext())
                {
                    string filter = !string.IsNullOrEmpty(txtcari.Text.Trim()) ? "noSPD" : string.Empty;
                    string param = txtcari.Text;
                    var data = ctx.sp_GetApprovalSPDAtasanLangsung(karyawan.nrp, filter, param).ToList();
                    gvViewSPD.DataSource = data;
                    gvViewSPD.PageIndex = e.NewPageIndex;
                    gvViewSPD.DataBind();
                }
            }
            else if (a == "Nama")
            {
                using (var ctx = new dsSPDDataContext())
                {
                    string filter = !string.IsNullOrEmpty(txtcari.Text.Trim()) ? "namaLengkap" : string.Empty;
                    string param = txtcari.Text;
                    var data = ctx.sp_GetApprovalSPDAtasanLangsung(karyawan.nrp, filter, param).ToList();
                    gvViewSPD.DataSource = data;
                    gvViewSPD.PageIndex = e.NewPageIndex;
                    gvViewSPD.DataBind();
                }
            }
            else if (a == "BPHUM")
            {
                using (var ctx = new dsSPDDataContext())
                {
                    string filter = !string.IsNullOrEmpty(txtcari.Text.Trim()) ? "BPHUM" : string.Empty;
                    string param = txtcari.Text;
                    var data = ctx.sp_GetApprovalSPDAtasanLangsung(karyawan.nrp, filter, param).ToList();
                    gvViewSPD.DataSource = data;
                    gvViewSPD.PageIndex = e.NewPageIndex;
                    gvViewSPD.DataBind();
                }

            }
            else if (a == "ErrorBPHUM")
            {
                using (var ctx = new dsSPDDataContext())
                {
                    string filter = !string.IsNullOrEmpty(txtcari.Text.Trim()) ? "ErrorBPHUM" : string.Empty;
                    string param = txtcari.Text;
                    var data = ctx.sp_GetApprovalSPDAtasanLangsung(karyawan.nrp, filter, param).ToList();
                    gvViewSPD.DataSource = data;
                    gvViewSPD.PageIndex = e.NewPageIndex;
                    gvViewSPD.DataBind();
                }
            }
            else
            {
                using (var ctx = new dsSPDDataContext())
                {
                    var data = ctx.sp_GetApprovalSPDAtasanLangsung(karyawan.nrp, string.Empty, string.Empty).ToList();
                    gvViewSPD.DataSource = data;
                    gvViewSPD.PageIndex = e.NewPageIndex;
                    gvViewSPD.DataBind();
                }
            }

            gvViewSPD.PageIndex = e.NewPageIndex;
            bindFind();
        }

        protected void btnFind_Click(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
            bindFind();
        }
        
        void bindFind()
        {
            using (var ctx = new dsSPDDataContext())
            {
                string filter = !string.IsNullOrEmpty(txtTglBerangkat.Text.Trim()) ? "tglBerangkat" : string.Empty;
                string param = txtTglBerangkat.Text;
                var data = ctx.sp_GetApprovalSPDAtasanLangsung(karyawan.nrp, filter, param).ToList();
                data = data.Where(x => x.IndexLevel != 0).ToList();
                //var data = ctx.sp_GetApprovalSPDAtasanLangsung("364", filter, param).ToList();
                gvViewSPD.DataSource = data;
                gvViewSPD.DataBind();

                if (!data.Any())
                {
                    gvViewSPD.Visible = false;
                    lblMessage.Visible = true;
                    lblMessage.Text = "No data";
                }
                else
                {
                    gvViewSPD.Visible = true;
                }
            }
        }

        protected void gvViewSPD_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                lblMessage.Visible = false;
                string[] command = e.CommandArgument.ToString().Split(';');
                ViewState["NoSPD"] = command[0];
                //split parameter nospd, nrpapproval, index
                switch (e.CommandName)
                {
                    case "Detail":
                        string URL = "~/newFormRequestDetail.aspx?encrypt=" + Encrypto.Encrypt(e.CommandArgument.ToString());
                        URL = Page.ResolveClientUrl(URL);
                        ScriptManager.RegisterStartupScript(this, GetType(), "openDetail", "openDetail('" + URL + "');", true);
                        break;

                    case "Approve":

                        lblMessage.Text = approvalSPDUrl.ChangeStatus(command[0], e.CommandName, command[1], command[2]);
                        break;

                    case "Reject":
                        lblMessage.Text = approvalSPDUrl.ChangeStatus(command[0], e.CommandName, command[1], command[2]);
                        break;

                    case "ApproveADH":
                        lblMessage.Text = approvalSPDUrl.ChangeStatus(command[0], "Approve", karyawan.nrp, "0");
                        break;

                    case "RejectADH":
                        lblMessage.Text = approvalSPDUrl.ChangeStatus(command[0], "Reject", karyawan.nrp, "0");
                        break;

                    case "Edit":
                        Response.Redirect("newFormRequestInput.aspx?noSPD=" + e.CommandArgument.ToString() + "", true);
                        break;

                    case "Synch":
                        bindADH();
                        break;
                        
                    default:
                        break;
                }
                lblMessage.Visible = true;
                if (e.CommandName.Contains("ADH"))
                {
                    bindADH();
                }
                else
                {
                    bindFind();
                }
            }
            catch (Exception ex)
            {
                LogError.Log_Error(ex, "copy file request", ViewState["NoSPD"].ToString());
            }
        }

        protected void gvViewSPD_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            lblMessage.Visible = true;
        }
        #endregion

        #region approval tujuan
        void bindFindTujuan()
        {
            using (var ctx = new dsSPDDataContext())
            {
                DateTime tglBerangkat = (string.IsNullOrEmpty(txTglBerangkatTujuan.Text) ? DateTime.Now : Convert.ToDateTime(txTglBerangkatTujuan.Text));

                var data = (from p in ctx.trSPDs
                            where p.nrpApprovalTujuan.Trim() == karyawan.nrp
                            && p.status == "SPD Menunggu approval tujuan"
                            && p.isSubmit == true
                            && p.isApproved == null
                            && (p.isCancel == null || p.isCancel == false)
                            && (string.IsNullOrEmpty(txTglBerangkatTujuan.Text) ? true : p.tglBerangkat == tglBerangkat)
                            && p.status != "SPD Expired"
                            select new
                            {
                                noSPD = p.noSPD,
                                nrp = p.nrp,
                                namaLengkap = p.namaLengkap,
                                cabangTujuan = p.companyCodeTujuan != null ? p.companyCodeTujuan + " - " + p.personelAreaTujuan + " - " + p.pSubAreaTujuan : p.tempatTujuanLain,
                                keperluan = p.idKeperluan != null ? p.msKeperluan.keperluan : p.keperluanLain,
                                tglBerangkat = p.tglBerangkat.ToShortDateString(),
                                tglKembali = p.tglKembali.ToShortDateString(),
                                status = p.status
                            }).OrderByDescending(o => o.noSPD).ToList();

                #region Untuk Testing
                //var data = (from p in ctx.trSPDs
                //            where p.nrpApprovalTujuan.Trim() == "3302"
                //            select new
                //            {
                //                noSPD = p.noSPD,
                //                nrp = p.nrp,
                //                namaLengkap = p.namaLengkap,
                //                cabangTujuan = p.companyCodeTujuan != null ? p.companyCodeTujuan + " - " + p.personelAreaTujuan + " - " + p.pSubAreaTujuan : p.tempatTujuanLain,
                //                keperluan = p.idKeperluan != null ? p.msKeperluan.keperluan : p.keperluanLain,
                //                tglBerangkat = p.tglBerangkat.ToShortDateString(),
                //                tglKembali = p.tglKembali.ToShortDateString(),
                //                status = p.status
                //            }).OrderByDescending(o => o.noSPD).ToList();
                #endregion

                gvViewSPDTujuan.DataSource = data;
                gvViewSPDTujuan.DataBind();

                if (!data.Any())
                {
                    gvViewSPDTujuan.Visible = false;
                    lblMessageTujuan.Visible = true;
                    lblMessageTujuan.Text = "No data";
                }
                else
                {
                    gvViewSPDTujuan.Visible = true;
                }
            }
        }

        protected void gvViewSPDTujuan_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvViewSPDTujuan.PageIndex = e.NewPageIndex;
            bindFindTujuan();
        }
        protected void gvViewSPDTujuan_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            lblMessageTujuan.Visible = false;
            switch (e.CommandName)
            {
                case "Detail":
                    string URL = "~/newFormRequestDetail.aspx?encrypt=" + Encrypto.Encrypt(e.CommandArgument.ToString());
                    URL = Page.ResolveClientUrl(URL);
                    ScriptManager.RegisterStartupScript(this, GetType(), "openDetail", "openDetail('" + URL + "');", true);
                    break;

                case "Approve":
                    lblMessageTujuan.Text = approvalSPDUrl.ChangeStatus(e.CommandArgument.ToString(), e.CommandName, karyawan.nrp, string.Empty);

                    break;

                case "Reject":
                    lblMessageTujuan.Text = approvalSPDUrl.ChangeStatus(e.CommandArgument.ToString(), e.CommandName, karyawan.nrp, string.Empty);

                    break;
                default:
                    break;
            }
            lblMessageTujuan.Visible = true;
            bindFindTujuan();
        }

        protected void ViewADH_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                
                if (!String.IsNullOrEmpty(DataBinder.Eval(e.Row.DataItem, "SisaUangMakan").ToString()))
                {
                    if (int.Parse(DataBinder.Eval(e.Row.DataItem, "SisaUangMakan").ToString()) < 0)
                    {
                        Button btnApproveADH = (Button)e.Row.FindControl("btnApproveADH");
                        btnApproveADH.Enabled = false;

                        Button btnSynch = (Button)e.Row.FindControl("btnShync");
                        btnSynch.Enabled = true;
                    }
                }
                if (!String.IsNullOrEmpty(DataBinder.Eval(e.Row.DataItem, "SisaUangSaku").ToString()))
                {
                    if (int.Parse(DataBinder.Eval(e.Row.DataItem, "SisaUangSaku").ToString()) < 0)
                    {
                        Button btnApproveADH = (Button)e.Row.FindControl("btnApproveADH");
                        btnApproveADH.Enabled = false;

                        Button btnSynch = (Button)e.Row.FindControl("btnShync");
                        btnSynch.Enabled = true;
                    }
                }
                
                if (!String.IsNullOrEmpty(DataBinder.Eval(e.Row.DataItem, "SisaTransportasi").ToString()))
                {
                    if (int.Parse(DataBinder.Eval(e.Row.DataItem, "SisaTransportasi").ToString()) < 0)
                    {
                        Button btnApproveADH = (Button)e.Row.FindControl("btnApproveADH");
                        btnApproveADH.Enabled = false;

                        Button btnSynch = (Button)e.Row.FindControl("btnShync");
                        btnSynch.Enabled = true;
                    }
                }

                if (!String.IsNullOrEmpty(DataBinder.Eval(e.Row.DataItem, "SisaAkomodasi").ToString()))
                {
                    if (int.Parse(DataBinder.Eval(e.Row.DataItem, "SisaAkomodasi").ToString()) < 0)
                    {
                        Button btnApproveADH = (Button)e.Row.FindControl("btnApproveADH");
                        btnApproveADH.Enabled = false;

                        Button btnSynch = (Button)e.Row.FindControl("btnShync");
                        btnSynch.Enabled = true;
                    }
                }
            }
        }

        protected void gvViewSPDTujuan_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // add by Yehezkiel (20171003)
                var berangkat = DataBinder.Eval(e.Row.DataItem, "tglBerangkat");
                //var status = DataBinder.Eval(e.Row.DataItem, "status");
                try
                {
                    DateTime Berangkat = Convert.ToDateTime(berangkat);
                    //string Status = Convert.ToString(status);
                    if (DateTime.Now.Date < Berangkat)
                    {
                        Button btnApprove = (Button)e.Row.FindControl("btnApprove");
                        btnApprove.Visible = false;

                        Button btnReject = (Button)e.Row.FindControl("btnReject");
                        btnReject.Visible = false;
                    }

                    string noSPD = Convert.ToString(DataBinder.Eval(e.Row.DataItem, "noSPD"));

                    using (var ctx = new dsSPDDataContext())
                    {
                        var data = (from p in ctx.trSPDs
                                    where p.noSPD == noSPD
                                    select new
                                    {
                                        p.status,
                                        //StatusTgl = DateTime.Now.Date >= p.tglExpired ? "SPD Expired" : ""
                                    }).FirstOrDefault();
                        //|| data.StatusTgl == "SPD Expired"
                        if (data.status == "SPD Expired" )
                        {
                            Button btnApprove = (Button)e.Row.FindControl("btnApprove");
                            btnApprove.Visible = false;

                            Button btnReject = (Button)e.Row.FindControl("btnReject");
                            btnReject.Visible = false;
                        }
                    }

                    //if (Status == "Expired")
                    //{
                    //    Button btnApprove = (Button)e.Row.FindControl("btnApprove");
                    //    btnApprove.Visible = false;

                    //    Button btnReject = (Button)e.Row.FindControl("btnReject");
                    //    btnReject.Visible = false;
                    //}
                }
                catch (Exception)
                { }
            }
        }
        protected void btnFindTujuan_Click(object sender, EventArgs e)
        {
            lblMessageTujuan.Visible = false;
            bindFindTujuan();
        }

        protected void gvViewSPDTujuan_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            lblMessageTujuan.Visible = true;
        }
        #endregion

        #region list SPD GA
        protected void gvViewSPDGA_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvViewSPDGA.PageIndex = e.NewPageIndex;
            bindFindSPDGa();
        }

        protected void gvViewSPDGA_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Detail")
            {
                string URL = "~/newFormRequestDetail.aspx?encrypt=" + Encrypto.Encrypt(e.CommandArgument.ToString());
                URL = Page.ResolveClientUrl(URL);
                ScriptManager.RegisterStartupScript(this, GetType(), "openDetail", "openDetail('" + URL + "');", true);
            }

            if (e.CommandName == "Cancel")
            {
                lblMessageGA.Text = approvalSPDUrl.ChangeStatus(e.CommandArgument.ToString(), e.CommandName, karyawan.nrp, string.Empty);
            }

            if (e.CommandName == "Revisi")
            {
                string URL = "~/newFormRequestInput.aspx?noSPD=" + e.CommandArgument.ToString()+ "&Action=" + e.CommandName;
                URL = Page.ResolveClientUrl(URL);
                ScriptManager.RegisterStartupScript(this, GetType(), "openDetail", "openDetail('" + URL + "');", true);
               
            }

            if (e.CommandName == "History")
            {
                using (var ctx = new dsSPDDataContext())
                {
                    var Data = ctx.HistorySPDs.Where(x => x.NoSPD == e.CommandArgument.ToString()).ToList();
                    gvHistorySPD.DataSource = Data;
                    gvHistorySPD.DataBind();
                    popupHistory.Show();
                }
            }
        }

        protected void ddlParamGA_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlParamGA.SelectedValue != "tglBerangkat") ceFilterGA.Enabled = false;
            else ceFilterGA.Enabled = true;
        }

        protected void btnFindGA_Click(object sender, EventArgs e)
        {
            bindFindSPDGa();
        }

        void bindFindSPDGa()
        {
            using (var ctx = new dsSPDDataContext())
            {
                string filter = ddlParamGA.Text.Trim() == "--Select--" ? "" : ddlParamGA.SelectedItem.Value;
                string param = txFilterGA.Text;
                var data = ctx.sp_GetListSPDGA(filter, param);
                gvViewSPDGA.DataSource = data;
                gvViewSPDGA.DataBind();
            }
        }

        protected void gvViewSPDGA_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            lblMessageGA.Visible = true;
            bindFindSPDGa();
        }

        protected void gvViewSPDGA_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var Cancel = DataBinder.Eval(e.Row.DataItem, "isCancel");
                bool isCancel = false;
                if (Cancel != null) isCancel = true;

                if (isCancel)
                {
                    Button btnCancel = (Button)e.Row.FindControl("btnCancel");
                    btnCancel.Visible = false;
                }

                string noSPD = Convert.ToString(DataBinder.Eval(e.Row.DataItem, "noSPD"));
                using (var ctx = new dsSPDDataContext())
                {
                    var Data = ctx.HistorySPDs.Where(x => x.NoSPD == noSPD).ToList();
                    if (Data.Count <= 1)
                    {
                        Button btnHistory = (Button)e.Row.FindControl("btnHistory");
                        btnHistory.Visible = false;
                    }
                }

                // add by Yehezkiel (20171003)
                var status = DataBinder.Eval(e.Row.DataItem, "status");
                try
                {
                    string Status = Convert.ToString(status);
                    if (Status == "SPD Expired")
                    {
                        Button btnApprove = (Button)e.Row.FindControl("btnCancel");
                        btnApprove.Visible = false;
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        #endregion

        //add by martha
        protected void DropDownList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string filterBy = DropDownList1.SelectedValue;

            switch (filterBy)
            {
                case "No":
                case "Nama":
                case "BPHUM":
                case "ErrorBPHUM":
                    txtcari.Text = "";
                    txtcari.Visible = true;

                    break;
                default:
                    txtcari.Text = "";
                    txtcari.Visible = false;

                    break;
            }
        }

        //add by martha
        protected void btncari_Click(object sender, EventArgs e)
        {
            string paramField = DropDownList1.Text.Trim() == "--Select--" ? "" : DropDownList1.Text.Trim();
            string a = DropDownList1.SelectedValue;


            if (a == "No")
            {
                using (var ctx = new dsSPDDataContext())
                {
                    string filter = !string.IsNullOrEmpty(txtcari.Text.Trim()) ? "noSPD" : string.Empty;
                    string param = txtcari.Text;
                    var data = ctx.sp_GetApprovalSPDAtasanLangsung(karyawan.nrp, filter, param).ToList();
                    gvViewSPD.DataSource = data;
                    gvViewSPD.DataBind();
                }
            }
            else if (a == "Nama")
            {
                using (var ctx = new dsSPDDataContext())
                {
                    string filter = !string.IsNullOrEmpty(txtcari.Text.Trim()) ? "namaLengkap" : string.Empty;
                    string param = txtcari.Text;
                    var data = ctx.sp_GetApprovalSPDAtasanLangsung(karyawan.nrp, filter, param).ToList();
                    gvViewSPD.DataSource = data;
                    gvViewSPD.DataBind();
                }

            }
            else if (a == "BPHUM")
            {
                using (var ctx = new dsSPDDataContext())
                {
                    string filter = !string.IsNullOrEmpty(txtcari.Text.Trim()) ? "BPHUM" : string.Empty;
                    string param = txtcari.Text;
                    var data = ctx.sp_GetApprovalSPDAtasanLangsung(karyawan.nrp, filter, param).ToList();
                    gvViewSPD.DataSource = data;
                    gvViewSPD.DataBind();
                }

            }
            else if (a == "ErrorBPHUM")
            {
                using (var ctx = new dsSPDDataContext())
                {
                    string filter = !string.IsNullOrEmpty(txtcari.Text.Trim()) ? "ErrorBPHUM" : string.Empty;
                    string param = txtcari.Text;
                    var data = ctx.sp_GetApprovalSPDAtasanLangsung(karyawan.nrp, filter, param).ToList();
                    gvViewSPD.DataSource = data;
                    gvViewSPD.DataBind();
                }
            }
            else
            {
                using (var ctx = new dsSPDDataContext())
                {
                    var data = ctx.sp_GetApprovalSPDAtasanLangsung(karyawan.nrp, string.Empty, string.Empty).ToList();
                    gvViewSPD.DataSource = data;
                    gvViewSPD.DataBind();
                }
            }
        }

        // Add by Yehezkiel (20171003)
        protected void gvViewSPD_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                try
                {
                    string noSPD = Convert.ToString(DataBinder.Eval(e.Row.DataItem, "noSPD"));
                    //string dateNow = DateTime.Now.ToString("yyyy-MM-dd");

                    using (var ctx = new dsSPDDataContext())
                    {
                        //=====================================================
                        var data = (from p in ctx.trSPDs
                                    where p.noSPD == noSPD
                                    select new
                                    {
                                        status = p.status,
                                        tglexpired = p.tglExpired
                                    }).FirstOrDefault();

                        if (data.status == "SPD Expired")
                        {
                            Button btnApprove = (Button)e.Row.FindControl("btnApprove");
                            btnApprove.Visible = false;

                            Button btnReject = (Button)e.Row.FindControl("btnReject");
                            btnReject.Visible = false;
                        }
                        //=====================================================
                    }
                }
                catch (Exception)
                { }
            }
        }

        #region Atasan
        protected void gvAtasan_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Detail")
            {
                string URL = "~/newFormRequestDetailClaim.aspx?encrypt=" + Encrypto.Encrypt(e.CommandArgument.ToString());
                URL = Page.ResolveClientUrl(URL);
                ScriptManager.RegisterStartupScript(this, GetType(), "openDetail", "openDetail('" + URL + "');", true);
            }

            if (e.CommandName == "Approve")
            {
                lblMessage.Text = claimApproval.ChangeStatus(e.CommandArgument.ToString(), e.CommandName, karyawan.nrp, "Atasan");
                lblMessage.Visible = true;
            }

            if (e.CommandName == "Reject")
            {
                lblMessage.Text = claimApproval.ChangeStatus(e.CommandArgument.ToString(), e.CommandName, karyawan.nrp, "Atasan");
                lblMessage.Visible = true;
            }

            if (e.CommandName != "Detail")
                BindAtasan();
        }

        protected void gvAtasan_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvAtasan.PageIndex = e.NewPageIndex;
            BindAtasan();
        }
        protected void OnPageIndexChangingViewADH(object sender, GridViewPageEventArgs e)
        {
            ViewADH.PageIndex = e.NewPageIndex;
            bindADH();
        }
        protected void IndexChanging_ListBudgetCostCenter(object sender, GridViewPageEventArgs e)
        {
            ListBudgetCostCenter.PageIndex = e.NewPageIndex;
            bindADH();
        }
        protected void gvAtasan_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var statusClaim = DataBinder.Eval(e.Row.DataItem, "status");
                try
                {
                    string Status = Convert.ToString(statusClaim);
                    if (Status == "Claim Expired")
                    {
                        Button btnApprove = (Button)e.Row.FindControl("btnApprove");
                        btnApprove.Visible = false;

                        Button btnReject = (Button)e.Row.FindControl("btnReject");
                        btnReject.Visible = false;
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        void BindAtasan()
        {
            using (var ctx = new dsSPDDataContext())
            {
                var data = (from claim in ctx.trClaims
                            join spd in ctx.trSPDs
                                on claim.noSPD equals spd.noSPD
                            where claim.nrpAtasan.Trim() == karyawan.nrp
                                && spd.isApproved == true
                                && claim.isSubmit == true
                                && claim.isApprovedAtasan == null
                                && claim.isApprovedGA == null
                                && claim.isApprovedFinance == null
                                && claim.isCancel == null
                                && claim.status != "Claim Expired"
                            select new
                            {
                                noSPD = claim.noSPD,
                                nrp = spd.nrp,
                                namaLengkap = spd.namaLengkap,
                                cabangTujuan = spd.companyCodeTujuan != "0" || spd.companyCodeTujuan != null ? spd.companyCodeTujuan + " - " + spd.personelAreaTujuan + " - " + spd.pSubAreaTujuan : spd.tempatTujuanLain,
                                tglBerangkat = spd.tglBerangkat.ToShortDateString(),
                                tglKembali = spd.tglKembali.ToShortDateString(),
                                uangMakan = claim.biayaMakan,
                                uangSaku = claim.uangSaku,
                                Tiket = claim.tiket,
                                Hotel = claim.hotel,
                                BBM = claim.BBM,
                                Tol = claim.tol,
                                Taxi = claim.taxi,
                                AirportTax = claim.airportTax,
                                laundry = claim.laundry,
                                Parkir = claim.parkir,
                                Lain = claim.biayaLainLain,
                                Total = claim.biayaMakan + claim.uangSaku + claim.tiket + claim.hotel + claim.BBM + claim.tol + claim.taxi + claim.airportTax + claim.laundry + claim.parkir + claim.biayaLainLain + claim.komunikasi,
                                status = claim.status,
                                tglExpired = spd.tglExpired

                            }).ToList();

                if (!data.Any())
                {
                    ClaimAtasan.Visible = false;
                }
                else
                {
                    ClaimAtasan.Visible = true;
                    gvAtasan.DataSource = data;
                    gvAtasan.DataBind();
                }
            }
        }
        
        #endregion

        protected void BtnChange_Click(object sender, EventArgs e)
        {
            Session["IDLogin"] = TxtChange.Text;
            strID = (string)Session["IDLogin"];
            karyawan = oSPD.getKaryawan(strID);
            Session["nrpLogin"] = karyawan.nrp;

            spdGA.Visible = true;
            BindAtasan();
            bindFind();
            bindFindTujuan();
            bindFindSPDGa();
            bindADH();
        }
        
    }
}