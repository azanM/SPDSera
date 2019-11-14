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
    public partial class msKota : System.Web.UI.Page
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
                var data = (from list in ctx.msPropinsis
                           select new
                           {
                               costId = list.Id,
                               costCenter = list.Propinsi
                           }).ToList();

                costCenterList.DataSource = data;
                costCenterList.DataValueField = "costId";
                costCenterList.DataTextField = "costCenter";
                costCenterList.DataBind();

            }
        }

        protected void BindGridMasterADH(string Filter)
        {
            string CostCenterCode = string.Empty;
            
            using (var ctx = new dsSPDDataContext())
            {
                var data = (from list in ctx.msKotas
                            join x in ctx.msPropinsis
                            on list.PropinsiID equals x.Id
                            where list.RowStatus == 1
                            select new
                            {
                                Id = list.ID,
                                nrp = list.NamaKota,
                                costDesc = x.Propinsi

                            }).ToList();

                if (!string.IsNullOrEmpty(Filter))
                {
                    CostCenterCode = Filter;
                    data = data.Where(x => x.nrp.Contains(CostCenterCode)).ToList();
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
                string nrp = txtNRP.Text;
                string costCenterId = costCenterList.SelectedValue;
                using (var ctx = new dsSPDDataContext())
                {
                    var data = ctx.msKotas.Where(x => x.NamaKota == nrp && x.RowStatus == 1).FirstOrDefault();
                    if (data == null)
                    {
                        msKota newADH = new msKota();
                        newADH.NamaKota = nrp;
                        newADH.PropinsiID = int.Parse(costCenterId);
                        newADH.RowStatus = 1;
                        ctx.msKotas.InsertOnSubmit(newADH);
                        ctx.SubmitChanges();
                        notif.Text = "Kota Berhasil disimpan";
                        BindGridMasterADH(txtFilter.Text);
                    }
                    else
                    {
                        notif.Text = "Kota Sudah Terdaftar";
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
                var ADH = (from data in ctx.msKotas
                           join x in ctx.msPropinsis on data.PropinsiID equals x.Id
                           where data.ID == msADHid
                           select new
                           {
                               nama = data.NamaKota,
                               costId = x.Id,
                               Id = data.ID
                           }).FirstOrDefault();

                txtNRP.Text = ADH.nama;
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
                    var adh = ctx.msKotas.Where(x => x.ID == msADHid).FirstOrDefault();
                    adh.RowStatus = 0;
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
                var adh = ctx.msKotas.Where(x => x.ID == int.Parse(id)).FirstOrDefault();
                adh.NamaKota = nrp;
                adh.PropinsiID = int.Parse(idCostCenter);
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
                        var data = (from list in ctx.msPropinsis
                                    select new
                                    {
                                        costId = list.Id,
                                        costCenter = list.Propinsi
                                    }).ToList();

                        ddList.DataSource = data;
                        ddList.DataValueField = "costId";
                        ddList.DataTextField = "costCenter";
                        ddList.DataBind();
                      costCenterID = data.Where(x => x.costCenter == costCenter).Select(x=>x.costId).FirstOrDefault();
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
                var adh = ctx.msKotas.Where(x => x.ID == int.Parse(id)).FirstOrDefault();
                adh.RowStatus = 0;
                ctx.SubmitChanges();
            }
            gvMasterADH.EditIndex = -1;
            BindGridMasterADH(txtFilter.Text);
        }
    }
}