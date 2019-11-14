 using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using eSPD.Core;

namespace eSPD
{
    public partial class msADH : System.Web.UI.Page
    {
        string constr = ConfigurationManager.ConnectionStrings["SPDConnectionString1"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            classSpd cspd = new classSpd();

            string strLoginID = string.Empty;
            classSpd oSPD = new classSpd();
            if (Session["IDLogin"] != null)
            {
                strLoginID = (string)Session["IDLogin"];
            }
            else
            {
                strLoginID = SetLabelWelcome();
            }
            msKaryawan karyawan = oSPD.getKaryawan(strLoginID);
            dsSPDDataContext data = new dsSPDDataContext();
            Int32 roleid = (from k in data.msUsers
                            where k.nrp == karyawan.nrp && (k.roleId == Konstan.SYSADMIN || k.roleId == Konstan.GA)
                            select k.roleId).FirstOrDefault();
            if (roleid == Konstan.GA || strLoginID.Contains("yudisss") || roleid == Konstan.SYSADMIN)
            {
                gvMasterADH.Visible = true;
                btnTambah.Visible = true;
                Filter.Visible = true;
            }
            else
            {
                gvMasterADH.Visible = false;
                btnTambah.Visible = false;
            }
            if (!IsPostBack)
            {
                inputForm.Visible = false;
                hfmode.Value = "add";
                BindDropdownCostCenter();
                BindGridMasterADH(txtFilter.Text);
                Filter.Visible = true;
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

        protected void BindDropdownCostCenter()
        {
            
            using (var ctx = new dsSPDDataContext())
            {                
                var data = (from list in ctx.msCosts
                           select new
                           {
                               costId = list.costId,
                               costCenter = list.costCenter,
                               costDesc = list.costDesc
                           }).ToList();

                costCenterList.DataSource = data;
                costCenterList.DataValueField = "costId";
                costCenterList.DataTextField = "costDesc";
                costCenterList.DataBind();

            }
        }

        protected void BindGridMasterADH(string Filter)
        {
            string CostCenterCode = string.Empty;
            
            using (var ctx = new dsSPDDataContext())
            {
                var data = (from list in ctx.msADHs
                            join x in ctx.msCosts
                            on list.costcenterId equals x.costId
                            where list.RowStatus == true
                            select new
                            {
                                Id = list.id,
                                nrp = list.nrp,
                                nama = list.nama,
                                costDesc = x.costDesc

                            }).ToList();

                if (!string.IsNullOrEmpty(Filter))
                {
                    CostCenterCode = Filter;
                    data = data.Where(x => x.costDesc.Contains(CostCenterCode)).ToList();
                }
                gvMasterADH.DataSource = data;
                gvMasterADH.DataBind();
            }
        }

        protected void costCenter_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            BindGridMasterADH(txtFilter.Text);
        }

        protected void btnSimpan_Click(object sender, EventArgs e)
        {
            try
            {
                string name = txtNamaLengkap.Text;
                string nrp = txtNRP.Text;
                string costCenterId = costCenterList.SelectedValue;
                using (var ctx = new dsSPDDataContext())
                {
                    var data = ctx.msADHs.Where(x => x.costcenterId == int.Parse(costCenterId) && x.RowStatus == true).FirstOrDefault();
                    if (data == null)
                    {
                        msADH newADH = new msADH();
                        newADH.nrp = nrp;
                        newADH.nama = name;
                        newADH.costcenterId = int.Parse(costCenterId);
                        newADH.RowStatus = true;
                        ctx.msADHs.InsertOnSubmit(newADH);
                        ctx.SubmitChanges();
                        notif.Text = "ADH Berhasil disimpan";
                        BindGridMasterADH(txtFilter.Text);
                    }
                    else
                    {
                        notif.Text = "Cost Center Sudah Terdaftar";
                        BindGridMasterADH(txtFilter.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                notif.Text = ex.Message.ToString();
            }
        }

        protected void btnBatal_Click(object sender, EventArgs e)
        {
            //Filter.Visible = false;
        }

        protected void gvMasterADH_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvMasterADH.PageIndex = e.NewPageIndex;
            BindGridMasterADH(txtFilter.Text);
        }

        protected void lbEdit_Click(object sender, EventArgs e)
        {
            Filter.Visible = false;
            LinkButton link = (LinkButton)sender;
            GridViewRow gv = (GridViewRow)(link.NamingContainer);
            Label id = (Label)gv.FindControl("id");
            int msADHid = int.Parse(id.Text.Trim());

            using (var ctx = new dsSPDDataContext())
            {
                var ADH = (from data in ctx.msADHs
                           join x in ctx.msCosts on data.costcenterId equals x.costId
                           where data.id == msADHid
                           select new
                           {
                               nrp = data.nrp,
                               nama = data.nama,
                               costId = x.costId,
                               Id = data.id
                           }).FirstOrDefault();

                txtNRP.Text = ADH.nrp;
                txtNamaLengkap.Text = ADH.nama;
                costCenterList.SelectedValue = ADH.costId.ToString();
                hfmasterADHId.Value = ADH.Id.ToString();

            }
            hfmode.Value = "edit";
            inputForm.Visible = true;
            btnTambah.Visible = false;
        }

        protected void lbDelete_Click(object sender, EventArgs e)
        {
            LinkButton link = (LinkButton)sender;
            GridViewRow gv = (GridViewRow)(link.NamingContainer);
            Label id = (Label)gv.FindControl("id");
            int msADHid = int.Parse(id.Text.Trim());

            using (var ctx = new dsSPDDataContext())
            {
                if (!String.IsNullOrEmpty(hfmasterADHId.Value))
                {
                    var adh = ctx.msADHs.Where(x => x.id == msADHid).FirstOrDefault();
                    adh.RowStatus = false;
                    ctx.SubmitChanges();
                }
            }
            notif.Text = "Data berhasil diupdate";
            BindGridMasterADH(txtFilter.Text);
        }

        protected void btnTambah_Click(object sender, EventArgs e)
        {
            inputForm.Visible = true;
            btnTambah.Visible = false;
            hfmode.Value = "add";
            //Filter.Visible = false;
        }

        private void ClearForm()
        {
            txtNRP.Text = string.Empty;
            txtNamaLengkap.Text = string.Empty;
        }

        protected void gvMasterADH_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvMasterADH.EditIndex = e.NewEditIndex;
            BindGridMasterADH(txtFilter.Text);
        }

        protected void gvMasterADH_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            GridViewRow row = gvMasterADH.Rows[e.RowIndex];
            string id = (row.FindControl("Id") as Label).Text;
            String nrp = (row.FindControl("nrp") as TextBox).Text;
            String idCostCenter = (row.FindControl("ddlCostCenter") as DropDownList).SelectedValue;
            using (var ctx = new dsSPDDataContext())
            {
                var adh = ctx.msADHs.Where(x => x.id == int.Parse(id)).FirstOrDefault();
                adh.nrp = nrp;
                adh.costcenterId = int.Parse(idCostCenter);
                ctx.SubmitChanges();
            }
            gvMasterADH.EditIndex = -1;
            BindGridMasterADH(txtFilter.Text);
        }

        protected void gvMasterADH_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if ((e.Row.RowState & DataControlRowState.Edit) > 0)
                {
                    int costCenterID = 0;
                    string costCenter = DataBinder.Eval(e.Row.DataItem, "costDesc").ToString();
                    DropDownList ddList = (DropDownList)e.Row.FindControl("ddlCostCenter");
                    using (var ctx = new dsSPDDataContext())
                    {
                        var data = (from list in ctx.msCosts
                                    select new
                                    {
                                        costId = list.costId,
                                        costCenter = list.costCenter,
                                        costDesc = list.costDesc
                                    }).ToList();

                        ddList.DataSource = data;
                        ddList.DataValueField = "costId";
                        ddList.DataTextField = "costDesc";
                        ddList.DataBind();
                        costCenterID = data.Where(x => x.costDesc == costCenter).Select(x=>x.costId).FirstOrDefault();
                    }
                    ddList.SelectedValue = costCenterID.ToString();
                }
            }
        }

        protected void gvMasterADH_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            txtFilter.Text = string.Empty;
            gvMasterADH.EditIndex = -1;
            BindGridMasterADH(txtFilter.Text);
        }

        protected void gvMasterADH_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            GridViewRow row = gvMasterADH.Rows[e.RowIndex];
            string id = (row.FindControl("Id") as Label).Text;
            using (var ctx = new dsSPDDataContext())
            {
                var adh = ctx.msADHs.Where(x => x.id == int.Parse(id)).FirstOrDefault();
                adh.RowStatus = false;
                ctx.SubmitChanges();
            }
            gvMasterADH.EditIndex = -1;
            BindGridMasterADH(txtFilter.Text);
        }
    }
}