﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using eSPD.Core;

namespace eSPD
{
    public partial class frmMsKeperluan : System.Web.UI.Page
    {
        protected string userLoginID = "";
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

            if (!IsPostBack)
            {
                pnlForm.Visible = false;
                fillGV(txtFilterName.Text.Trim());
            }

            if (roleid == Konstan.GA || strLoginID.Contains("yudisss") || strLoginID.Contains("Syam005812") || roleid == Konstan.SYSADMIN)
            {
                pnlGrid.Visible = true;
                gvRole.Visible = true;
            }
            else
            {
                pnlGrid.Visible = false;
                gvRole.Visible = false;
            }

            userLoginID = !String.IsNullOrEmpty(strLoginID) ? strLoginID.Length > 10 ? strLoginID.Substring(0, 10).ToString() : strLoginID : "";
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

        private void fillGV(string filter)
        {
            dsSPDDataContext dsp = new dsSPDDataContext();
            if (!String.IsNullOrEmpty(filter))
            {
                var query = (from m in dsp.msKeperluans
                             select new { 
                                 id = m.id, 
                                 keperluan = m.keperluan, 
                                 status = m.status.Equals(1) ? "Aktif" : "Tidak Aktif" }
                             ).Where(o => o.keperluan.Contains(filter));
                gvRole.DataSource = query.ToList();
            }
            else
            {
                var query = (from m in dsp.msKeperluans
                             select new
                             {
                                 id = m.id,
                                 keperluan = m.keperluan,
                                 status = m.status.Equals(1) ? "Aktif" : "Tidak Aktif"
                             }
                             );
                gvRole.DataSource = query.ToList();
            }

            gvRole.DataBind();
            dsp.Dispose();
            gvRole.Visible = true;
        }

        protected void gvRole_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvRole.PageIndex = e.NewPageIndex;
            fillGV(txtFilterName.Text.Trim());
        }


        protected void btnTambah_Click(object sender, EventArgs e)
        {
            pnlForm.Visible = true;
            pnlGrid.Visible = false;
            hfmode.Value = "add";

            //hdnRoleName.Value = string.Empty;
            //txtRoleName.Enabled = true;
            hdnRoleID.Value = string.Empty;
            txtNrp.Text = string.Empty;
            //cmbUser.Items.Clear();
            //cmbUser.Enabled = false;
            txtNrp.Enabled = true;
            cmbUser.Enabled = true;
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            clear_form();
            pnlForm.Visible = false;
            pnlGrid.Visible = true;
            fillGV("");
        }
        protected void lbEdit_Click(object sender, EventArgs e)
        {
            dsSPDDataContext dss = new dsSPDDataContext();
            LinkButton link = (LinkButton)sender;
            GridViewRow gv = (GridViewRow)(link.NamingContainer);
            string nrp = gv.Cells[1].Text; Label id = (Label)gv.FindControl("id");
            string status = gv.Cells[2].Text;
            var x = (from m in dss.msKeperluans
                     where m.id.ToString().Trim() == id.Text.Trim()
                     select m).FirstOrDefault();
            fill_form(ref x, status);
            hfmode.Value = "edit";

            pnlForm.Visible = true;
            pnlGrid.Visible = false;
        }

        protected void btnSimpan_Click(object sender, EventArgs e)
        {
            dsSPDDataContext dss = new dsSPDDataContext();
            string mode = "add";
            mode = hfmode.Value.ToString();
            if (mode == "add")
            {
                msKeperluan cst = (from k in dss.msKeperluans
                                   where k.keperluan.ToString().Trim() == txtNrp.Text.Trim()
                                   select k).FirstOrDefault();
                if (cst == null)
                {
                    int b = cmbUser.SelectedValue == "Aktif" ? 1 : 0;
                    msKeperluan role = new msKeperluan();
                    //role.id = Convert.ToInt32(hdnRoleID.Value);
                    role.keperluan = txtNrp.Text.Trim();
                    role.status = b;
                    role.dibuatOleh = userLoginID;
                    role.dibuatTanggal = DateTime.Now;
                    role.diubahOleh = userLoginID;
                    role.diubahTanggal = DateTime.Now;

                    dss.msKeperluans.InsertOnSubmit(role);
                    dss.SubmitChanges();
                    dss.Dispose();
                    //clear_form();
                    notif.Text = "Data berhasil disimpan";
                    //fillGV("");
                }
                else
                {
                    notif.Text = "Nama Role atau Posisi sudah terdaftar";
                }
            }
            ////mode edit gadipake
            else if (mode == "edit")
            {

                msKeperluan cst = (from k in dss.msKeperluans
                                   where k.id.ToString().Trim() == hdnRoleID.Value
                                   select k).FirstOrDefault();

                int b = cmbUser.SelectedValue == "Aktif" ? 1 : 0;

                cst.id = Convert.ToInt32(hdnRoleID.Value);
                cst.keperluan = txtNrp.Text.Trim();
                cst.diubahOleh = userLoginID;
                cst.diubahTanggal = DateTime.Now;
                cst.status = b;
                dss.SubmitChanges();
                dss.Dispose();
                notif.Text = "Data berhasil disimpan";
            }
            fillGV(txtFilterName.Text.Trim());
        }

        private void clear_form()
        {
            //txtRoleName.Text = string.Empty;
            hdnRoleID.Value = string.Empty;
            txtNrp.Text = string.Empty;
        }

        private void fill_form(ref msKeperluan x, string status)
        {
            hdnRoleID.Value = x.id.ToString();
            txtNrp.Text = x.keperluan;
            cmbUser.SelectedValue = status;
            cmbUser.Visible = true;

            //fillCmbUser(status);
            txtNrp.Enabled = true;
            cmbUser.Enabled = true;
        }
        protected void btnFilter_Click(object sender, EventArgs e)
        {
            fillGV(txtFilterName.Text.Trim());
        }

    }
}