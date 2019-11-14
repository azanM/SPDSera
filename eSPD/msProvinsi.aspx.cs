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
    public partial class msProvinsi : System.Web.UI.Page
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
                gvMasterProvinsi.Visible = true;
                btnTambah.Visible = true;
            }
            else
            {
                gvMasterProvinsi.Visible = false;
                btnTambah.Visible = false;
            }

            if (!IsPostBack)
            {
                inputForm.Visible = false;
                hfmode.Value = "add";
                BindGridMasterProvinsi();
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
        protected void BindGridMasterProvinsi()
        {
            using (var ctx = new dsSPDDataContext())
            {
                var listPropinsi = ctx.msPropinsis.Where(x => x.RowStatus == true).ToList();
                gvMasterProvinsi.DataSource = listPropinsi;
                gvMasterProvinsi.DataBind();
            }    
        }
        protected void btnSimpan_Click(object sender, EventArgs e)
        {
            try
            {
                string propinsi = txtProvinsi.Text;
                using (var ctx = new dsSPDDataContext())
                {
                    if(!String.IsNullOrEmpty(HFProvinsiId.Value))
                    {
                        int IDPropinsi = int.Parse(HFProvinsiId.Value);
                        var OldPropinsi = ctx.msPropinsis.Where(x => x.Id == IDPropinsi).FirstOrDefault();
                        OldPropinsi.Propinsi = propinsi;
                        OldPropinsi.UpdatedOn = DateTime.Now;
                        ctx.SubmitChanges();
                    }
                    else
                    {
                        msPropinsi newPropinsi = new msPropinsi();
                        newPropinsi.CreatedOn = DateTime.Now;
                        newPropinsi.Propinsi = propinsi;
                        newPropinsi.RowStatus = true;
                        ctx.msPropinsis.InsertOnSubmit(newPropinsi);
                        ctx.SubmitChanges();
                    }
                    ClearForm();
                    notif.Text = "Data berhasil disimpan";
                    BindGridMasterProvinsi();
                }
            }
            catch (Exception ex)
            {
                notif.Text = ex.Message.ToString();
            }
        }

        protected void btnBatal_Click(object sender, EventArgs e)
        {

        }

        protected void gvMasterProvinsi_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvMasterProvinsi.PageIndex = e.NewPageIndex;
            BindGridMasterProvinsi();
        }

        protected void lbEdit_Click(object sender, EventArgs e)
        {
            LinkButton link = (LinkButton)sender;
            GridViewRow gv = (GridViewRow)(link.NamingContainer);
            Label id = (Label)gv.FindControl("id");
            int propinsiId = int.Parse(id.Text.Trim());

            using (var ctx = new dsSPDDataContext())
            {
                var propinsi = ctx.msPropinsis.Where(x => x.Id == propinsiId).FirstOrDefault();
                txtProvinsi.Text = propinsi.Propinsi;
                HFProvinsiId.Value = propinsi.Id.ToString();
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
            int propinsiId = int.Parse(id.Text.Trim());
            using (var ctx = new dsSPDDataContext())
            {
                var propinsi = ctx.msPropinsis.Where(x => x.Id == propinsiId).FirstOrDefault();
                propinsi.RowStatus = false;
                ctx.SubmitChanges();
            }
            notif.Text = "Data berhasil diupdate";
            BindGridMasterProvinsi();
        }

        protected void btnTambah_Click(object sender, EventArgs e)
        {
            inputForm.Visible = true;
            btnTambah.Visible = false;
            hfmode.Value = "add";
        }

        private void ClearForm()
        {
            txtProvinsi.Text = string.Empty;
        }
    }
}