using eSPD.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Transactions;
using System.Collections;

namespace eSPD
{
    public partial class newFormRequestInput : System.Web.UI.Page
    {
        private static msKaryawan karyawan = new msKaryawan();
        private static classSpd oSPD = new classSpd();
        private static string strID = string.Empty;
        List<string> errorMessageHidden = new List<string>();
        private static List<string> atasanArr = new List<string>();
        string NoSPD = string.Empty;
        String Action = String.Empty;
        SQLHelper hp = new SQLHelper();

        protected void Page_Load(object sender, EventArgs e)
        {
            NoSPD = Request.QueryString["noSPD"];
            Action = Request.QueryString["Action"];

            if (Session["IDLogin"] == null)
                Response.Redirect("frmHome.aspx");

            strID = (string)Session["IDLogin"];
            karyawan = oSPD.getKaryawan(strID);
            Session["nrpLogin"] = karyawan.nrp;

            if (NoSPD != null)
            {
                if (!IsPostBack)
                    setDetailSPD();
            }
            else
            {    
                if (string.IsNullOrEmpty(karyawan.nrp))
                    Response.Redirect("frmHome.aspx");
                isSec(karyawan.nrp);// jika sekertaris

                if (!IsPostBack)
                {
                    setInitial(karyawan);
                    LoadDdlCompanyTujuan();
                    BindDropdownProvinsi();
                    BindKotaAsalTujuan();
                }
                if (IsPostBack)
                    approvalViewState();
                enableUangMukaAndAlasan();//tujuanLain(txTempatTujuanLain.Enabled);
            }
        }

        private void setInitialRowTunjanganKejauahan()
        {
            DataTable dt = new DataTable();
            DataRow dr = null;
            dt.Columns.Add(new DataColumn("RowNumber", typeof(string)));
            dt.Columns.Add(new DataColumn("Column1", typeof(string)));//dropdown lokasi
            dt.Columns.Add(new DataColumn("Column2", typeof(string)));//harga
            dt.Columns.Add(new DataColumn("Column3", typeof(string)));//jumlah hari
            dr = dt.NewRow();
            dr["RowNumber"] = 1;
            dr["Column2"] = string.Empty;
            dr["Column3"] = string.Empty;
            dt.Rows.Add(dr);
            ViewState["CurrentTable"] = dt;
            gvTunjanganHarship.DataSource = dt;
            gvTunjanganHarship.DataBind();
            DropDownList ddl1 = (DropDownList)gvTunjanganHarship.Rows[0].Cells[1].FindControl("listLokasi1");
            FillDropDownList(ddl1);
        }

        private void FillDropDownList(DropDownList ddl)
        {
            string Golongan = ddlGolongan.SelectedValue;
            string PropinsiId = String.Empty;
            if (ViewState["PropinsiId"] != null)
            {
                PropinsiId = ViewState["PropinsiId"].ToString();
            }
            else
                PropinsiId = listProvinsi.SelectedValue;
            DataTable dt = hp.getListHardship(Golongan, int.Parse(PropinsiId));
            DataRow item = dt.NewRow();
            item[0] = 0;
            item[1] = "- Select One -";
            dt.Rows.InsertAt(item, 0);
            ddl.DataSource = dt;
            ddl.DataBind();
        }
        protected void CostCenter_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            string costCenter = ddlCostCenter.SelectedValue;
            LoadApprovalADH(costCenter);
        }
        private void LoadApprovalADH(string CostCenter)
        {
            DataTable dt = hp.LoadListADH(CostCenter);
            ddlApprovalADH.DataSource = dt;
            ddlApprovalADH.DataValueField = "nrp";
            ddlApprovalADH.DataTextField = "namaLengkap";
            ddlApprovalADH.DataBind();
        }

        protected void ddlSelfOrDirect_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlDireksi.Visible = false;
            rvDireksi.Enabled = false;
            ddDireksi.Visible = false;

            if (ddlSelfOrDirect.SelectedValue == "0") setDirect();
            else setInitial(karyawan);
        }

        protected void ddlDireksi_SelectedIndexChanged(object sender, EventArgs e)
        {
            using (var ctx = new dsSPDDataContext())
            {
                if (ddlSelfOrDirect.SelectedValue == "0" && !string.IsNullOrWhiteSpace(ddlDireksi.SelectedValue))
                {
                    var direksi = ctx.msKaryawans.FirstOrDefault(o => o.nrp == ddlDireksi.SelectedValue);
                    setInitial(direksi);
                }
                else
                {
                    setInitial(karyawan);
                }
            }
        }

        protected void ddlAsal_SelectedIndexChanged(object sender, EventArgs e)
        {
            setContent();
            setApproval();
        }

        protected void ddlGolongan_SelectedIndexChanged(object sender, EventArgs e)
        {
            setContent();
            setApproval();
        }

        protected void ddlPosisi_SelectedIndexChanged(object sender, EventArgs e)
        {
            setContent();
            setApproval();
        }

        protected void ddlTujuan_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlWilayahTujuan.Items.Clear();
            ddlWilayahTujuan.Items.Add(new ListItem(" - Select One - ", "", true));
            ddlWilayahTujuan.AppendDataBoundItems = true;
            setContent();
            setApproval();
        }

        protected void ddlWilayahTujuan_SelectedIndexChanged(object sender, EventArgs e)
        {
            setContent();
        }

        protected void ddlCompanyTujuan_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlPersonalAreaTujuan.Items.Clear();
            ddlPersonalAreaTujuan.Items.Add(new ListItem(" - Select One - ", "", true));
            ddlPersonalAreaTujuan.AppendDataBoundItems = true;

            ddlPSubAreaTujuan.Items.Clear();
            ddlPSubAreaTujuan.Items.Add(new ListItem(" - Select One - ", "", true));
            ddlPSubAreaTujuan.AppendDataBoundItems = true;

            if (ddlCompanyTujuan.SelectedValue == "0")
            {
                ddlPersonalAreaTujuan.Items.Clear();
                ddlPSubAreaTujuan.Items.Clear();

                ddlPersonalAreaTujuan.Enabled = false;
                rvPersonalAreaTujuan.Enabled = false;
                ddlPSubAreaTujuan.Enabled = false;
                rvPSubAreaTujuan.Enabled = false;
                txTempatTujuanLain.Enabled = true;
                rvTempatTujuanLain.Enabled = true;
            }
            else
            {
                ddlPersonalAreaTujuan.Enabled = true;
                ddlPSubAreaTujuan.Enabled = true;
                txTempatTujuanLain.Enabled = false;
            }
        }

        protected void ddlPersonalAreaTujuan_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlPSubAreaTujuan.Items.Clear();
            string kodePATujuan = String.Empty;
            if (ViewState["kodePATujuan"] != null)
            {
                kodePATujuan = ViewState["kodePATujuan"].ToString();
            }
            else
                kodePATujuan = ddlPersonalAreaTujuan.SelectedValue;
            DataTable dt = hp.getPersonalArea(kodePATujuan);
            ddlPSubAreaTujuan.DataSource = dt;
            ddlPSubAreaTujuan.DataValueField = "Code";
            ddlPSubAreaTujuan.DataTextField = "Name";
            ddlPSubAreaTujuan.DataBind();
            ddlPSubAreaTujuan.Items.Insert(0,new ListItem(" - Select One - ", "", true));
            ddlPSubAreaTujuan.AppendDataBoundItems = true;
        }

        protected void txtTglBerangkat_TextChanged(object sender, EventArgs e)

        {
            if (ddlJamBerangkat.Text == "")
            {
                string tglBerangkat = txtTglBerangkat.Text + " " + "00" + ":" + ddlMenitBerangkat.Text + ":" + "00.000";
                
            }
            else
            {
                string tglBerangkat = txtTglBerangkat.Text + " " + ddlJamBerangkat.Text + ":" + ddlMenitBerangkat.Text + ":" + "00.000";
              
            }
        
            enableUangMukaAndAlasan();
            setDisabledHotelAndNotLessThan();
        }

        protected void txtTglKembali_TextChanged(object sender, EventArgs e)
        {
            if (ddlJamKembali.Text == "")
            {
                string tglkembali = txtTglKembali.Text + " " + "00" + ":" + ddlMenitKembali.Text + ":" + "00.000";   
            }
            else
            {
                string tglkembali = txtTglKembali.Text + " " + ddlJamKembali.Text + ":" + ddlMenitKembali.Text + ":" + "00.000";
            }

            DateTime a = DateTime.Now;
            enableUangMukaAndAlasan();
            setDisabledHotelAndNotLessThan();

            txtTglExp.Text = Convert.ToDateTime(txtTglKembali.Text).AddDays(15).ToString("MM/dd/yyyy");
            
            DateTime today = DateTime.Now;
            DateTime answer = today.AddDays(36);
        }

        protected void ddlAngkutan_SelectedIndexChanged(object sender, EventArgs e)
        {
            angkutanLain(false);
            if (ddlAngkutan.SelectedValue == "5") angkutanLain(true);
            if (ddlAngkutan.SelectedValue == "1" || ddlAngkutan.SelectedValue == "4")
            {
                ddlTipePesawat.Enabled = true;
                ddlKotaAsal.Enabled = true;
                ddlKotaTujuan.Enabled = true;
            }
            else
            {
                ddlTipePesawat.Enabled = false;
                ddlTipePesawat.SelectedValue = "NonGaruda";
                ddlKotaAsal.Enabled = false;
                ddlKotaTujuan.Enabled = false;
                listProvinsi.Enabled = false;
                //listLokasiProvinsi.Enabled = false;
                ddlKotaAsal.ClearSelection();
                ddlKotaTujuan.ClearSelection();
                listProvinsi.ClearSelection();
                //listLokasiProvinsi.ClearSelection();
            }
        }

        protected void GvApproval_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;

            int iRow = e.Row.DataItemIndex;

            TextBox txt = e.Row.Cells[1].FindControl("txNrpApproval") as TextBox;
            TextBox txtDesc = e.Row.Cells[1].FindControl("txDesc") as TextBox;
            if (txt != null)
                txt.Attributes.Add("data-index", iRow.ToString());
            if (txtDesc != null)
                txtDesc.Attributes.Add("data-index-desc", iRow.ToString());
        }

        protected void RevisiSPD(trSPD spd, dsSPDDataContext ctx)
        {
            HistorySPD historySPD = new HistorySPD();
            spd.tglBerangkat = DateTime.Parse(txtTglBerangkat.Text.ToString());
            spd.jamBerangkat = ddlJamBerangkat.SelectedValue;
            spd.menitBerangkat = ddlMenitBerangkat.SelectedValue;
            spd.tglKembali = DateTime.Parse(txtTglKembali.Text.ToString());
            spd.jamKembali = ddlJamKembali.SelectedValue;
            spd.menitKembali = ddlMenitKembali.SelectedValue;
            spd.tglExpired = DateTime.Parse(txtTglExp.Text.ToString());
            spd.diubahTanggal = DateTime.Now;
            spd.diubahOleh = (string)Session["IDLogin"];
            spd.isRevisi = true;
            var listApproval = ctx.ApprovalStatus.Where(x => x.NoSPD == NoSPD).ToList();

            foreach (var item in listApproval)
            {
                item.Status = null;
                item.ModifiedDate = DateTime.Now;
            }
            historySPD.NoSPD = spd.noSPD;
            historySPD.tglBerangkat = DateTime.Parse(txtTglBerangkat.Text.ToString());
            historySPD.jamBerangkat = ddlJamBerangkat.SelectedValue;
            historySPD.menitBerangkat = ddlMenitBerangkat.SelectedValue;
            historySPD.tglKembali = DateTime.Parse(txtTglKembali.Text.ToString());
            historySPD.jamKembali = ddlJamKembali.SelectedValue;
            historySPD.menitKembali = ddlMenitKembali.SelectedValue;
            historySPD.CreatedDate = DateTime.Now;
            historySPD.CreatedBy = Session["nrpLogin"].ToString();

            using (var ctp = new ESSDataContext())
            {
                if (checkJadwalToSeraESS(ctx) == true)
                {
                    ctx.HistorySPDs.InsertOnSubmit(historySPD);
                    ctx.SubmitChanges();
                    pnlSuccess.Visible = true;
                    pnlError.Visible = false;
                    lblSuccess.Text = "SPD Berhasil di Save";
                    btnSave.Enabled = false;
                    btnSubmit.Enabled = true;
                    btnReset.Disabled = true;

                    if (errorMessageHidden.Count() > 0)
                    {
                        errorMessage.DataSource = errorMessageHidden;
                        errorMessage.DataBind();
                        pnlError.Visible = true;
                        pnlSuccess.Visible = false;
                    }
                }
                else
                {
                    lblGagagl.Text = "Ada transaksi in dan out di SERAESS. SPD dapat dicreate diluar rentang clockin clockout";
                    pnlError.Visible = true;
                    pnlSuccess.Visible = false;
                }
            }
        }

        protected void EditADH(trSPD spd, dsSPDDataContext ctx, ESPDInformation SPDInformation)
        {
            ApprovalStatus approvalADH = ctx.ApprovalStatus.Where(x => x.NoSPD == NoSPD && x.IndexLevel == 0).FirstOrDefault();
            msCost costCenter = ctx.msCosts.Where(x => x.costDesc == ddlCostCenter.SelectedValue).FirstOrDefault();
            if (!String.IsNullOrEmpty(ddlApprovalADH.SelectedValue))
            {
                var newADH = (from p in ctx.msKaryawans where p.nrp == ddlApprovalADH.SelectedValue select p).FirstOrDefault();
                approvalADH.NrpApproval = newADH.nrp;
                approvalADH.Nama = newADH.namaLengkap;
                approvalADH.Email = newADH.EMail;
                spd.costCenter = ddlCostCenter.SelectedValue;
                ctx.SubmitChanges();
                EmailCore.InformasiSPD(spd.dibuatOleh,spd);//kirim email ke pembuat SPD
                EmailCore.ApprovalSPD(newADH.nrp, approvalADH.IndexLevel.Value.ToString(), spd, SPDInformation);//kirim email ke ADH
                pnlSuccess.Visible = true;
                btnReset.Disabled = true;
                lblSuccess.Text = "SPD Berhasil di Update, No SPD : " + NoSPD;
                btnSave.Enabled = false;
                pnlError.Visible = false;
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(NoSPD))
            {
                using (var ctx = new dsSPDDataContext())
                {
                    try
                    {
                        trSPD spd = ctx.trSPDs.FirstOrDefault(o => o.noSPD == NoSPD.ToString());
                        DataTable dtResult = new DataTable();
                        DataTable dtNoSPD = new DataTable();
                        dtNoSPD.Columns.Add("NomorSPD", typeof(string));
                        dtNoSPD.Rows.Add(spd.noSPD);

                        var SPDInformation = ESPDInformation.CheckBudgetToSAP(spd);//ambil informasi budget dari SAP
                        
                        if (SPDInformation.Message == "Success")
                        {
                            ESPDInformation.PerhitunganBiayaSPDByBudget(SPDInformation, dtResult, dtNoSPD);//hitung budget yang sisa
                            Result = dtResult;
                            int SisaUangMakan = string.IsNullOrEmpty(dtResult.Rows[0]["SisaUangMakan"].ToString()) ? 0 : int.Parse(dtResult.Rows[0]["SisaUangMakan"].ToString());
                            int SisaUangSaku = string.IsNullOrEmpty(dtResult.Rows[0]["SisaUangSaku"].ToString()) ? 0 : int.Parse(dtResult.Rows[0]["SisaUangSaku"].ToString());
                            int SisaTransportasi = string.IsNullOrEmpty(dtResult.Rows[0]["SisaTransportasi"].ToString()) ? 0 : int.Parse(dtResult.Rows[0]["SisaTransportasi"].ToString());
                            int SisaAkomodasi = string.IsNullOrEmpty(dtResult.Rows[0]["SisaAkomodasi"].ToString()) ? 0 : int.Parse(dtResult.Rows[0]["SisaAkomodasi"].ToString());

                            if (SisaUangSaku < 0)
                                errorMessageHidden.Add("Budget Uang Saku Tidak Mencukupi(" + SisaUangSaku.ToString("N2", new CultureInfo("id-ID")) + ")");
                            if (SisaUangMakan < 0)
                                errorMessageHidden.Add("Budget Uang Makan Tidak Mencukupi(" + SisaUangMakan.ToString("N2", new CultureInfo("id-ID")) + ")");
                            if (SisaTransportasi < 0)
                                errorMessageHidden.Add("Budget Uang Transportasi Tidak Mencukupi(" + SisaTransportasi.ToString("N2", new CultureInfo("id-ID")) + ")");
                            if (SisaAkomodasi < 0)
                                errorMessageHidden.Add("Budget Uang Akomodasi Tidak Mencukupi(" + SisaAkomodasi.ToString("N2", new CultureInfo("id-ID")) + ")");
                        }
                        else
                        {
                            errorMessageHidden.Add(SPDInformation.Message);
                        }

                        if (errorMessageHidden.Count() > 0)
                        {
                            errorMessage.DataSource = errorMessageHidden;
                            errorMessage.DataBind();
                            pnlError.Visible = true;
                            pnlSuccess.Visible = false;
                            return;
                        }

                        if (Action == "Revisi")
                        {
                            RevisiSPD(spd, ctx);
                        }
                        else if (Action == "EditADH")
                        {
                            EditADH(spd, ctx, SPDInformation);
                        }
                    }
                    catch (Exception ex)
                    {
                        errorMessageHidden.Add(ex.Message.ToString());
                        errorMessage.DataSource = errorMessageHidden;
                        errorMessage.DataBind();
                        pnlError.Visible = true;
                        pnlSuccess.Visible = false;
                    }
                }
            }
            else
            {
                using (var ctx = new dsSPDDataContext())
                {
                    List<Approval> Approvals = (List<Approval>)ViewState["Approval"];
                    trSPD spd = new trSPD();
                    spd.asal = ddlAsal.SelectedValue;
                    spd.nrp = txNrp.Value;
                    spd.namaLengkap = txNamaLengkap.Text;
                    if (ddlSubGolongan.Visible == true)
                    {
                        string strSubGolongan = ddlGolongan.SelectedValue.Trim() + ddlSubGolongan.SelectedValue.Trim();
                        spd.idGolongan = strSubGolongan;
                    }
                    else
                    {
                        spd.idGolongan = ddlGolongan.SelectedValue;
                    }
                    spd.jabatan = txJabatan.Text;
                    spd.email = txEmail.Text;
                    spd.coCdTujuan = ddlCompanyTujuan.SelectedValue;
                    spd.companyCodeTujuan = ddlCompanyTujuan.SelectedItem.Text;

                    if (ddlCompanyTujuan.SelectedValue != "0")
                    {
                        spd.kodePATujuan = ddlPersonalAreaTujuan.SelectedValue;
                        spd.personelAreaTujuan = ddlPersonalAreaTujuan.SelectedItem.Text;
                        spd.kodePSubAreaTujuan = ddlPSubAreaTujuan.SelectedValue;
                        spd.pSubAreaTujuan = ddlPSubAreaTujuan.SelectedItem.Text;
                    }
                    else
                    {
                        spd.personelAreaTujuan = ddlCompanyTujuan.SelectedItem.Text;
                        spd.pSubAreaTujuan = ddlCompanyTujuan.SelectedItem.Text;
                    }
                    spd.nrpApprovalTujuan = ddlApprovalTujuan.SelectedValue;
                    spd.Tujuan = ddlTujuan.SelectedItem.Text;
                    spd.WilayahTujuan = ddlWilayahTujuan.SelectedValue;
                    spd.NoHP = txNoPonsel.Text;
                    spd.tempatTujuanLain = txTempatTujuanLain.Text;
                    spd.idKeperluan = Convert.ToInt32(ddlKeperluan.SelectedValue);
                    spd.ketKeperluan = txKetKeperluan.Text;
                    spd.tiket = "Dicarikan";

                    spd.tglBerangkat = DateTime.Parse(txtTglBerangkat.Text.ToString());
                    spd.jamBerangkat = ddlJamBerangkat.SelectedValue;
                    spd.menitBerangkat = ddlMenitBerangkat.SelectedValue;
                    spd.tglKembali = DateTime.Parse(txtTglKembali.Text.ToString());
                    spd.jamKembali = ddlJamKembali.SelectedValue;
                    spd.menitKembali = ddlMenitKembali.SelectedValue;
                    spd.tglExpired = DateTime.Parse(txtTglExp.Text.ToString());

                    spd.Alasan = txAlasan.Text;
                    spd.idAngkutan = Convert.ToInt32(ddlAngkutan.SelectedValue);
                    spd.angkutanLain = txAngkutanLain.Text;
                    spd.penginapan = ddlPenginapan.SelectedValue;
                    
                    int rowIndex = 0; int TotalHari = 0;
                    foreach (GridViewRow row in gvTunjanganHarship.Rows)
                    {
                        if (rowIndex == 0)
                        {
                            TextBox box1 = (TextBox)gvTunjanganHarship.Rows[rowIndex].Cells[2].FindControl("harga1");
                            TextBox box2 = (TextBox)gvTunjanganHarship.Rows[rowIndex].Cells[3].FindControl("jumlahHari");
                            DropDownList ddl1 = (DropDownList)gvTunjanganHarship.Rows[rowIndex].Cells[1].FindControl("listLokasi1");

                            if (ddl1.SelectedValue != "0" && !String.IsNullOrEmpty(box1.Text) && !string.IsNullOrEmpty(box2.Text))
                            {
                                spd.idTunjanganKejauhan = int.Parse(ddl1.SelectedValue);
                                spd.hariTunjangan1 = int.Parse(box2.Text);
                                TotalHari = TotalHari + int.Parse(box2.Text);
                            }
                        }
                        else if (rowIndex == 1)
                        {
                            TextBox box1 = (TextBox)gvTunjanganHarship.Rows[rowIndex].Cells[2].FindControl("harga1");
                            TextBox box2 = (TextBox)gvTunjanganHarship.Rows[rowIndex].Cells[3].FindControl("jumlahHari");
                            DropDownList ddl1 = (DropDownList)gvTunjanganHarship.Rows[rowIndex].Cells[1].FindControl("listLokasi1");

                            if (ddl1.SelectedValue != "0" && !String.IsNullOrEmpty(box1.Text) && !string.IsNullOrEmpty(box2.Text))
                            {
                                spd.idTunjanganKejauhan2 = int.Parse(ddl1.SelectedValue);
                                spd.hariTunjangan2 = int.Parse(box2.Text);
                                TotalHari = TotalHari + int.Parse(box2.Text);
                            }
                        }
                        else if (rowIndex == 2)
                        {
                            TextBox box1 = (TextBox)gvTunjanganHarship.Rows[rowIndex].Cells[2].FindControl("harga1");
                            TextBox box2 = (TextBox)gvTunjanganHarship.Rows[rowIndex].Cells[3].FindControl("jumlahHari");
                            DropDownList ddl1 = (DropDownList)gvTunjanganHarship.Rows[rowIndex].Cells[1].FindControl("listLokasi1");
                            if (ddl1.SelectedValue != "0" && !String.IsNullOrEmpty(box1.Text) && !string.IsNullOrEmpty(box2.Text))
                            {
                                spd.idTunjanganKejauhan3 = int.Parse(ddl1.SelectedValue);
                                spd.hariTunjangan3 = int.Parse(box2.Text);
                                TotalHari = TotalHari + int.Parse(box2.Text);
                            }
                        }
                        else if (rowIndex == 3)
                        {
                            TextBox box1 = (TextBox)gvTunjanganHarship.Rows[rowIndex].Cells[2].FindControl("harga1");
                            TextBox box2 = (TextBox)gvTunjanganHarship.Rows[rowIndex].Cells[3].FindControl("jumlahHari");
                            DropDownList ddl1 = (DropDownList)gvTunjanganHarship.Rows[rowIndex].Cells[1].FindControl("listLokasi1");
                            if (ddl1.SelectedValue != "0" && !String.IsNullOrEmpty(box1.Text) && !string.IsNullOrEmpty(box2.Text))
                            {
                                spd.idTunjanganKejauhan4 = int.Parse(ddl1.SelectedValue);
                                spd.hariTunjangan4 = int.Parse(box2.Text);
                                TotalHari = TotalHari + int.Parse(box2.Text);
                            }
                        }
                        else if (rowIndex == 4)
                        {
                            TextBox box1 = (TextBox)gvTunjanganHarship.Rows[rowIndex].Cells[2].FindControl("harga1");
                            TextBox box2 = (TextBox)gvTunjanganHarship.Rows[rowIndex].Cells[3].FindControl("jumlahHari");
                            DropDownList ddl1 = (DropDownList)gvTunjanganHarship.Rows[rowIndex].Cells[1].FindControl("listLokasi1");

                            if (ddl1.SelectedValue != "0" && !String.IsNullOrEmpty(box1.Text) && !string.IsNullOrEmpty(box2.Text))
                            {
                                spd.idTunjanganKejauhan5 = int.Parse(ddl1.SelectedValue);
                                spd.hariTunjangan5 = int.Parse(box2.Text);
                                TotalHari = TotalHari + int.Parse(box2.Text);
                            }
                        }
                        rowIndex++;
                    }

                    TimeSpan Jumlahhari = spd.tglKembali - spd.tglBerangkat;
                    Jumlahhari += TimeSpan.FromDays(1);
                    
                    if (TotalHari > Jumlahhari.Days)
                    {
                        lblGagagl.Text = ",Total Jumlah hari pada tunjangan kejauhan harus sama dengan Lama SPD";
                        pnlError.Visible = true;
                        pnlSuccess.Visible = false;
                        return;
                    }
                    if (!String.IsNullOrEmpty(ddlTipePesawat.SelectedValue) && !String.IsNullOrEmpty(ddlKotaAsal.SelectedValue) && !String.IsNullOrEmpty(ddlKotaTujuan.SelectedValue))//get biaya transportasi
                    {
                        spd.idBiayaTransportasi = hp.getIdBiayaTransportasi(ddlTipePesawat.SelectedValue, int.Parse(ddlKotaAsal.SelectedValue), int.Parse(ddlKotaTujuan.SelectedValue));
                    }

                    if (ddlPenginapan.SelectedValue.ToLower().Contains("disediakan"))
                    {
                        spd.isHotel = true;
                    }
                    else
                    {
                        spd.isHotel = false;
                    }

                    // first Index Sequence approval, di assign pas even submit
                    var dataAtasan = (from a in ctx.msKaryawans
                                      where a.nrp == karyawan.nrp
                                      select a).FirstOrDefault();

                    spd.nrpAtasan = dataAtasan.nrpAtasan == null ? string.Empty : dataAtasan.nrpAtasan;
                    if (txUangMuka.Text != "" || txUangMuka.Text != "" || txUangMuka.Text != string.Empty)
                    {
                        spd.statusUM = "pending";
                    }

                    spd.uangMuka = txUangMuka.Text;
                    spd.costCenter = ddlCostCenter.SelectedValue;
                    spd.keterangan = txKeterangan.Text;
                    spd.status = "Save";
                    spd.dibuatOleh = txNrp.Value;
                    spd.dibuatTanggal = DateTime.Now;
                    spd.posisi = ddlPosisi.SelectedValue;

                    if (Approvals.Count() == 0)
                    {
                        errorMessageHidden.Add("Tidak ada approval atasan.");
                    }

                    if (!validateDate())
                    {
                        errorMessageHidden.Add("Waktu berangkat harus lebih kecil nilainya, dari pada waktu kembali.");
                    }

                    if (errorMessageHidden.Count() == 0)
                    {
                        if (checkJadwalToSeraESS(ctx) == true)
                        {
                            try
                            {
                                spd.noSPD = ctx.sp_GenerateNoSpd().FirstOrDefault().number; // generate number belakangan
                                ctx.trSPDs.InsertOnSubmit(spd);
                                ctx.SubmitChanges();

                                ESPDInformation SPDInformation = new ESPDInformation(); //CultureInfo culture = new CultureInfo("id-ID");
                                DataTable dtResult = new DataTable();
                                DataTable dtNoSPD = new DataTable();
                                dtNoSPD.Columns.Add("NomorSPD", typeof(string));
                                dtNoSPD.Rows.Add(spd.noSPD);
                                SPDInformation = ESPDInformation.CheckBudgetToSAP(spd);//ambil informasi budget dari SAP

                                if (SPDInformation.Message == "Success")
                                {
                                    ESPDInformation.PerhitunganBiayaSPDByBudget(SPDInformation, dtResult, dtNoSPD);//hitung budget yang tersedia
                                    Result = dtResult;
                                    if (dtResult.Rows.Count > 0)
                                    {
                                        int SisaUangMakan = string.IsNullOrEmpty(dtResult.Rows[0]["SisaUangMakan"].ToString()) ? 0 : int.Parse(dtResult.Rows[0]["SisaUangMakan"].ToString());
                                        int SisaUangSaku = string.IsNullOrEmpty(dtResult.Rows[0]["SisaUangSaku"].ToString()) ? 0 : int.Parse(dtResult.Rows[0]["SisaUangSaku"].ToString());
                                        int SisaTransportasi = string.IsNullOrEmpty(dtResult.Rows[0]["SisaTransportasi"].ToString()) ? 0 : int.Parse(dtResult.Rows[0]["SisaTransportasi"].ToString());
                                        int SisaAkomodasi = string.IsNullOrEmpty(dtResult.Rows[0]["SisaAkomodasi"].ToString()) ? 0 : int.Parse(dtResult.Rows[0]["SisaAkomodasi"].ToString());

                                        if (SisaUangSaku < 0)
                                            errorMessageHidden.Add("Budget Uang Saku Tidak Mencukupi(" + SisaUangSaku.ToString("N2", new CultureInfo("id-ID")) + ")");
                                        if (SisaUangMakan < 0)
                                            errorMessageHidden.Add("Budget Uang Makan Tidak Mencukupi(" + SisaUangMakan.ToString("N2", new CultureInfo("id-ID")) + ")");
                                        if (SisaTransportasi < 0)
                                            errorMessageHidden.Add("Budget Uang Transportasi Tidak Mencukupi(" + SisaTransportasi.ToString("N2", new CultureInfo("id-ID")) + ")");
                                        if (SisaAkomodasi < 0)
                                            errorMessageHidden.Add("Budget Uang Akomodasi Tidak Mencukupi(" + SisaAkomodasi.ToString("N2", new CultureInfo("id-ID")) + ")");
                                    }
                                }
                                else
                                {
                                    errorMessageHidden.Add(SPDInformation.Message);
                                }

                            }
                            catch (Exception ex)
                            {
                                errorMessageHidden.Add("Error Save SPD, ketika save spd header");
                                errorMessageHidden.Add(ex.Message);

                                if (ctx.trSPDs.FirstOrDefault(o => o.noSPD == spd.noSPD && o.namaLengkap == spd.namaLengkap) != null)
                                {
                                    ctx.trSPDs.DeleteOnSubmit(spd);
                                    ctx.SubmitChanges();
                                }
                            }

                            if (errorMessageHidden.Count() == 0)
                            {
                                // method insert approval
                                List<ApprovalStatus> newApprovalList = (from p in Approvals
                                                                        select new ApprovalStatus
                                                                        {
                                                                            NoSPD = spd.noSPD,
                                                                            RuleID = p.RuleID,
                                                                            NrpApproval = p.NrpApproval,
                                                                            Nama = p.Nama,
                                                                            Email = p.Email,
                                                                            IndexLevel = p.IndexLevel
                                                                        }).ToList();

                                //penambahan approval untuk ADH

                                var ADH = (from p in ctx.msKaryawans where p.nrp == ddlApprovalADH.SelectedValue select p).FirstOrDefault();
                                ApprovalStatus approvalADH = new ApprovalStatus();
                                approvalADH.NoSPD = spd.noSPD;
                                approvalADH.RuleID = ctx.ApprovalRules.Where(x => x.Deskripsi.Contains("PIC Budget")).First().RuleID;
                                approvalADH.NrpApproval = ADH.nrp;
                                approvalADH.Nama = ADH.namaLengkap;
                                approvalADH.Email = ADH.EMail;
                                approvalADH.IndexLevel = 0;
                                newApprovalList.Insert(0, approvalADH);

                                HistorySPD historySPD = new HistorySPD();
                                historySPD.NoSPD = spd.noSPD;
                                historySPD.tglBerangkat = DateTime.Parse(txtTglBerangkat.Text.ToString());
                                historySPD.jamBerangkat = ddlJamBerangkat.SelectedValue;
                                historySPD.menitBerangkat = ddlMenitBerangkat.SelectedValue;
                                historySPD.tglKembali = DateTime.Parse(txtTglKembali.Text.ToString());
                                historySPD.jamKembali = ddlJamKembali.SelectedValue;
                                historySPD.menitKembali = ddlMenitKembali.SelectedValue;
                                historySPD.CreatedDate = DateTime.Now;
                                historySPD.CreatedBy = Session["nrpLogin"].ToString();

                                try
                                {
                                    ctx.ApprovalStatus.InsertAllOnSubmit(newApprovalList);
                                    ctx.HistorySPDs.InsertOnSubmit(historySPD);
                                    ctx.SubmitChanges();
                                }
                                catch (Exception ex)
                                {
                                    errorMessageHidden.Add("Error Save Approval");
                                    errorMessageHidden.Add(ex.Message);
                                    if (ctx.trSPDs.FirstOrDefault(o => o.noSPD == spd.noSPD) != null)
                                    {
                                        ctx.trSPDs.DeleteOnSubmit(spd);
                                        ctx.SubmitChanges();
                                    }
                                    if (ctx.HistorySPDs.FirstOrDefault(o => o.NoSPD == spd.noSPD) != null)
                                    {
                                        ctx.HistorySPDs.DeleteOnSubmit(historySPD);
                                        ctx.SubmitChanges();
                                    }
                                }
                                finally
                                {
                                    txNoSPD.Text = spd.noSPD;
                                    pnlSuccess.Visible = true;
                                    pnlError.Visible = false;
                                    lblSuccess.Text = "SPD Berhasil di Save, No SPD : " + spd.noSPD;
                                    btnSave.Enabled = false;
                                    btnSubmit.Enabled = true;
                                    btnReset.Disabled = true;

                                }
                            }
                            if (errorMessageHidden.Count() > 0)
                            {
                                if (ctx.trSPDs.FirstOrDefault(o => o.noSPD == spd.noSPD) != null)
                                {
                                    ctx.trSPDs.DeleteOnSubmit(spd);
                                    ctx.SubmitChanges();
                                }
                                errorMessage.DataSource = errorMessageHidden;
                                errorMessage.DataBind();
                                pnlError.Visible = true;
                                pnlSuccess.Visible = false;

                            }
                        }
                        else
                        {
                            lblGagagl.Text = "Ada transaksi in dan out di SERAESS. SPD dapat dicreate diluar rentang clockin clockout";
                            pnlError.Visible = true;
                            pnlSuccess.Visible = false;

                        }
                    }

                }
                if (lblBackDate.Visible == true)
                    lblBackDate.Visible = false;
            }
        }

        protected bool checkJadwalToSeraESS(dsSPDDataContext ctx)
        {
            bool result = true;
            try
            {
                using (var ctp = new ESSDataContext())
                {
                    string tglBerangkat = txtTglBerangkat.Text + " " + ddlJamBerangkat.Text + ":" + ddlMenitBerangkat.Text + ":" + "00.000";
                    string tglkembali = txtTglKembali.Text + " " + ddlJamKembali.Text + ":" + ddlMenitKembali.Text + ":" + "00.000";

                    var dataCico = (from cico in ctx.v_essinouts
                                    where cico.Nrp == Convert.ToInt32(txNrp.Value) &&
                                    (Convert.ToDateTime(tglBerangkat) > cico.Time && Convert.ToDateTime(tglkembali) < cico.timeout) ||
                                    (Convert.ToDateTime(tglBerangkat) < cico.Time && Convert.ToDateTime(tglkembali) > cico.Time) ||
                                    (Convert.ToDateTime(tglBerangkat) < cico.timeout && Convert.ToDateTime(tglkembali) > cico.timeout)

                                    select cico).ToList();

                    dataCico = dataCico.Where(x => x.Nrp == Convert.ToInt32(txNrp.Value)).ToList();
                    if (dataCico.Count > 0)
                        result = false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return result;
        }
        protected void provinsi_SelectedIndexChanged(object sender, EventArgs e)
        {
            string Golongan = ddlGolongan.SelectedValue;
            setInitialRowTunjanganKejauahan();
        }

        protected void BindDropdownProvinsi()
        {
            DataTable dt = hp.getMsPropinsi();
            listProvinsi.DataSource = dt;
            listProvinsi.DataValueField = "Id";
            listProvinsi.DataTextField = "Propinsi";
            listProvinsi.DataBind();
        }

        protected void BindKotaAsalTujuan()
        {
            DataTable dt = hp.getMsKota();
            ddlKotaAsal.DataSource = dt;
            ddlKotaAsal.DataValueField = "Id";
            ddlKotaAsal.DataTextField = "NamaKota";
            ddlKotaAsal.DataBind();
            ddlKotaTujuan.DataSource = dt;
            ddlKotaTujuan.DataValueField = "Id";
            ddlKotaTujuan.DataTextField = "NamaKota";
            ddlKotaTujuan.DataBind();
        }

        protected void ddlKotaTujuan_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            listProvinsi.Enabled = true;
            string NamaKota = ddlKotaTujuan.SelectedItem.Text;
            DataTable dt = hp.getMsKotaByNamaKota(NamaKota);
            if (dt.Rows.Count > 0)
            {
                listProvinsi.SelectedValue = dt.Rows[0]["PropinsiID"].ToString();
                provinsi_SelectedIndexChanged(listProvinsi.SelectedValue, EventArgs.Empty);
            }
            else
            {
                listProvinsi.Enabled = false;
            }
        }
        
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            using (var ctx = new dsSPDDataContext())
            {
                trSPD spd = ctx.trSPDs.FirstOrDefault(o => o.noSPD == txNoSPD.Text);
                ApprovalStatus Atasan = ctx.ApprovalStatus.FirstOrDefault(o => o.NoSPD == txNoSPD.Text && o.IndexLevel.Value == 1); //sebelum ada approval adh
                ApprovalStatus firstApproval = ctx.ApprovalStatus.FirstOrDefault(o => o.NoSPD == txNoSPD.Text && o.IndexLevel.Value == 0);//get approval adh
                DataTable t = Result;

                try
                {
                    if (t != null)
                    {
                        if (t.Rows.Count == 0)
                        {
                            ESPDInformation SPDInformation = new ESPDInformation(); //CultureInfo culture = new CultureInfo("id-ID");
                            DataTable dtResult = new DataTable();
                            DataTable dtNoSPD = new DataTable();
                            dtNoSPD.Columns.Add("NomorSPD", typeof(string));
                            dtNoSPD.Rows.Add(spd.noSPD);
                            SPDInformation = ESPDInformation.CheckBudgetToSAP(spd);//ambil informasi budget dari SAP
                            if (SPDInformation.Message == "Success")
                            {
                                ESPDInformation.PerhitunganBiayaSPDByBudget(SPDInformation, dtResult, dtNoSPD);
                                t = dtResult;
                            }
                            else
                            {
                                errorMessageHidden.Add(SPDInformation.Message);
                            }
                        }

                    }
                    else if (t == null)
                    {
                        ESPDInformation SPDInformation = new ESPDInformation(); //CultureInfo culture = new CultureInfo("id-ID");
                        DataTable dtResult = new DataTable();
                        DataTable dtNoSPD = new DataTable();
                        dtNoSPD.Columns.Add("NomorSPD", typeof(string));
                        dtNoSPD.Rows.Add(spd.noSPD);
                        SPDInformation = ESPDInformation.CheckBudgetToSAP(spd);//ambil informasi budget dari SAP
                        if (SPDInformation.Message == "Success")
                        {
                            ESPDInformation.PerhitunganBiayaSPDByBudget(SPDInformation, dtResult, dtNoSPD);
                            t = dtResult;
                        }
                        else
                        {
                            errorMessageHidden.Add(SPDInformation.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorMessageHidden.Add("Error Submit SPD, gangguan teknis");
                }
                if (spd == null)
                    errorMessageHidden.Add("SPD tidak ditemukan");
                
                if (firstApproval == null)
                    errorMessageHidden.Add("Approval pertama tidak ditemukan");

                if (errorMessageHidden.Count() == 0)
                {
                    spd.diubahOleh = karyawan.nrp;
                    spd.diubahTanggal = DateTime.Now;
                    spd.isSubmitDate = DateTime.Now;
                    spd.nrpAtasan = Atasan.NrpApproval;
                    spd.status = "Menunggu approval " + firstApproval.ApprovalRule.Deskripsi;
                    spd.isSubmit = true;

                    try
                    {
                        ctx.SubmitChanges();
                        ESPDInformation SPDInformation = new ESPDInformation();
                        SPDInformation.SetBiayaSPD(ref SPDInformation, t);
                        EmailCore.InformasiSPD(spd.dibuatOleh,spd);//kirim email ke pembuat SPD
                        EmailCore.ApprovalSPD(firstApproval.NrpApproval, firstApproval.IndexLevel.ToString(), spd, SPDInformation);//kirim email ke atasan atau approval
                    }
                    catch (Exception ex)
                    {
                        errorMessageHidden.Add("Error Submit SPD, gangguan teknis");
                        errorMessageHidden.Add(ex.Message);
                    }
                    finally
                    {
                        pnlSuccess.Visible = true;
                        pnlError.Visible = false;
                        lblSuccess.Text = "SPD Berhasil di Submit, No SPD : " + spd.noSPD + ", status spd saat ini adalah " + spd.status;
                        btnSave.Enabled = false;
                        btnSubmit.Enabled = false;

                        btnReset.Disabled = false;
                    }
                }
                if (errorMessageHidden.Count() > 0)
                {
                    errorMessage.DataSource = errorMessageHidden;
                    errorMessage.DataBind();

                    pnlError.Visible = true;
                    pnlSuccess.Visible = false;
                }
            }
        }

        void setInitial(msKaryawan karyawan)
        {
            txNrp.Value = karyawan.nrp;

            if (karyawan.nrp.Equals("99999999"))
            {
                txNamaLengkap.ReadOnly = false;
                txEmail.ReadOnly = false;
                txJabatan.ReadOnly = false;
                ddlGolongan.SelectedValue = "III";
                rvNamaLengkap.Enabled = true;
                rvEmail.Enabled = true;
                rvJabatan.Enabled = true;
            }
            else
            {
                ddlGolongan.SelectedValue = karyawan.payscalegroup == null ? "" : karyawan.payscalegroup.Trim();

                if (karyawan.payscalegroup.Trim() == "A" || karyawan.payscalegroup.Trim() == "C" || karyawan.payscalegroup.Trim() == "S")
                {
                    ddlSubGolongan.Visible = true;
                    LoadDdlSubGolongan(karyawan.payscalegroup.Trim());
                }
            }

            txNamaLengkap.Text = karyawan.namaLengkap;
            txEmail.Text = karyawan.EMail;
            txJabatan.Text = karyawan.Job;
            txCompanyCode.Text = karyawan.companyCode;
            if (karyawan.companyCode == "PT. SERASI AUTORAYA" || karyawan.companyCode == "PT SERASI AUTORAYA")
            {
                if (karyawan.personelArea.Trim() == "HEAD OFFICE" || karyawan.personelArea.Trim() == "Head Office" || karyawan.personelArea.Trim() == "Trac Head Office" || karyawan.personelArea.Trim() == "TRAC HEAD OFFICE")
                {
                    ddlAsal.SelectedValue = "HO";
                }
                else
                {
                    ddlAsal.SelectedValue = "Cabang";
                }
            }
            else
            {
                ddlAsal.SelectedValue = "Cabang";
            }
            txPersonalArea.Text = karyawan.personelArea;
            txPersonalSubArea.Text = karyawan.pSubArea;

        }

        private void LoadDdlSubGolongan(string golongan)
        {
            if (golongan == "A")
            {
                ddlSubGolongan.Items.Clear();
                ddlSubGolongan.Items.Add(new ListItem("- Select One -"));
                ddlSubGolongan.Items.Add(new ListItem("0"));
                ddlSubGolongan.Items.Add(new ListItem("1"));
                ddlSubGolongan.Items.Add(new ListItem("2"));

                ddlSubGolongan.AppendDataBoundItems = true;
            }
            else if (golongan == "C")
            {
                ddlSubGolongan.Items.Clear();
                ddlSubGolongan.Items.Add(new ListItem("- Select One -"));
                ddlSubGolongan.Items.Add(new ListItem("0"));
                ddlSubGolongan.Items.Add(new ListItem("1"));
                ddlSubGolongan.Items.Add(new ListItem("2"));
                ddlSubGolongan.Items.Add(new ListItem("3"));

                ddlSubGolongan.AppendDataBoundItems = true;
            }
            else if (golongan == "S")
            {
                ddlSubGolongan.Items.Clear();
                ddlSubGolongan.Items.Add(new ListItem("- Select One -"));
                ddlSubGolongan.Items.Add(new ListItem("0"));
                ddlSubGolongan.Items.Add(new ListItem("1"));
                ddlSubGolongan.Items.Add(new ListItem("2"));
                ddlSubGolongan.Items.Add(new ListItem("3"));

                ddlSubGolongan.AppendDataBoundItems = true;
            }
        }

        void approvalViewState()
        {
            using (var ctx = new dsSPDDataContext())
            {
                List<Approval> Approvals = new List<Approval>();
                try
                {
                    for (int i = 0; i < gvApproval.Rows.Count; i++)
                    {
                        Approval newApproval = new Approval();

                        var NrpApproval = (TextBox)gvApproval.Rows[i].Cells[1].FindControl("txNrpApproval");
                        var IndexLevel = (TextBox)gvApproval.Rows[i].Cells[1].FindControl("txIndexLevel");
                        var Deskripsi = (TextBox)gvApproval.Rows[i].Cells[1].FindControl("txDesc");
                        var RuleID = (TextBox)gvApproval.Rows[i].Cells[1].FindControl("txRuleID");

                        if (!string.IsNullOrEmpty(NrpApproval.Text))
                        {
                            var atasanData = (from p in ctx.msKaryawans
                                              where p.nrp == NrpApproval.Text
                                              select p).FirstOrDefault();

                            newApproval.Email = atasanData.EMail;
                            newApproval.Nama = atasanData.namaLengkap;
                        }

                        newApproval.NrpApproval = NrpApproval.Text;
                        newApproval.IndexLevel = Convert.ToInt32(IndexLevel.Text); ;
                        newApproval.Deskripsi = Deskripsi.Text;
                        newApproval.RuleID = Convert.ToInt32(RuleID.Text); ;

                        Approvals.Add(newApproval);
                    }
                    ViewState.Remove("Approval");
                    //ViewState.Clear();
                    ViewState.Add("Approval", Approvals);
                    gvApproval.DataSource = ViewState["Approval"];
                    gvApproval.DataBind();
                }
                catch (Exception)
                {
                    // nothing
                    //ViewState.Clear();
                    Approvals.RemoveRange(0, 9999);
                    pnlApproval.Visible = true;
                    pnlRequiredAppval.Visible = false;
                    setApproval();
                }

                pnlApproval.Visible = false;
                pnlRequiredAppval.Visible = true;

                if (Approvals.Count > 0)
                {
                    pnlApproval.Visible = true;
                    pnlRequiredAppval.Visible = false;
                }
            }
        }

        void setContent()
        {
            string golongan = "";

            if (ddlGolongan.SelectedValue == "A" || ddlGolongan.SelectedValue == "C" || ddlGolongan.SelectedValue == "S")
            {
                ddlSubGolongan.Visible = true;
                string strSubGolongan = ddlGolongan.SelectedValue.Trim() + ddlSubGolongan.SelectedValue.Trim();
                if (strSubGolongan == "A0" || strSubGolongan == "A1" || strSubGolongan == "C0" || strSubGolongan == "C1" || strSubGolongan == "C2" || strSubGolongan == "C3" || strSubGolongan == "S0" || strSubGolongan == "S1" || strSubGolongan == "S2")
                {
                    golongan = "III";
                }
                else if (strSubGolongan == "A2" || strSubGolongan == "S3")
                {
                    golongan = "IV";
                }

                using (var ctx = new dsSPDDataContext())
                {
                    // plafon
                    var data = (from p in ctx.msGolonganPlafons
                               where p.golongan.Equals(golongan.Trim()) 
                               && p.jenisSPD.Equals(ddlTujuan.SelectedItem.Text)
                               select p).ToList();

                    if (ddlWilayahTujuan.SelectedItem != null)
                    {
                        data = data.Where(x => x.wilayah.Equals(ddlWilayahTujuan.SelectedItem.Text)).ToList();
                    }
                    var plafon = data.Where(p => p.idPlafon == 1).ToList();
                    
                    if (plafon.Count > 0) txBiayaMakan.Text = plafon.First().harga == null ? "" : plafon.First().harga.ToString();

                    plafon = data.Where(p => p.idPlafon == 2).ToList();

                    if (plafon.Count > 0) txUangSaku.Text = plafon.First().harga == null ? "" : plafon.First().harga.ToString();

                    plafon = data.Where(p => p.idPlafon == 3).ToList();

                    if (plafon.Count > 0) txTransportasi.Text = plafon.First().deskripsi == null ? "" : plafon.First().deskripsi;
                }
            }
            else
            {
                using (var ctx = new dsSPDDataContext())
                {
                    // plafon
                    var data = (from p in ctx.msGolonganPlafons
                               where p.golongan.Equals(ddlGolongan.SelectedValue.Trim()) 
                               && p.jenisSPD.Equals(ddlTujuan.SelectedItem.Text)
                               select p).ToList();

                    if (ddlWilayahTujuan.SelectedItem != null)
                    {
                        data = data.Where(x => x.wilayah.Equals(ddlWilayahTujuan.SelectedItem.Text)).ToList();
                    }

                    var plafon = data.Where(p => p.idPlafon == 1).ToList();
                    
                    if (plafon.Count > 0) txBiayaMakan.Text = plafon.First().harga == null ? "" : plafon.First().harga.ToString();

                    plafon = data.Where(p => p.idPlafon == 2).ToList();
                    
                    if (plafon.Count > 0) txUangSaku.Text = plafon.First().harga == null ? "" : plafon.First().harga.ToString();

                    plafon = data.Where(p => p.idPlafon == 3).ToList();

                    if (plafon.Count > 0) txTransportasi.Text = plafon.First().deskripsi == null ? "" : plafon.First().deskripsi;
                }
            }
        }

        void setApproval()
        {
            if (ddlGolongan.SelectedValue == "A" || ddlGolongan.SelectedValue == "C" || ddlGolongan.SelectedValue == "S")
            {
                string strSubGolongan = ddlGolongan.SelectedValue.Trim() + ddlSubGolongan.SelectedValue.Trim();
                if (strSubGolongan == "A0" || strSubGolongan == "A1" || strSubGolongan == "C0" || strSubGolongan == "C1" || strSubGolongan == "C2" || strSubGolongan == "C3" || strSubGolongan == "S0" || strSubGolongan == "S1" || strSubGolongan == "S2")
                {
                    using (var ctx = new dsSPDDataContext())
                    {
                        ViewState.Remove("Approval");
                        //ViewState.Clear();

                        var dataApproval = (from x in ctx.ApprovalRules
                                            join y in ctx.ApprovalStatus
                                            on new { X1 = x.RuleID, X2 = txNoSPD.Text } equals new { X1 = y.RuleID, X2 = y.NoSPD } into aps
                                            from y1 in aps.DefaultIfEmpty()
                                            where
                                                  x.Tipe == ddlTujuan.SelectedValue &&
                                                  x.TipeDetail == ddlAsal.SelectedValue &&
                                                  x.Golongan == "III" &&
                                                  x.Posisi.Equals(ddlPosisi.SelectedValue)
                                            select new Approval
                                            {
                                                NrpApproval = y1.NrpApproval,
                                                Nama = y1.Nama,
                                                IndexLevel = x.IndexLevel,
                                                Deskripsi  = x.Deskripsi == "Presiden Director" ? x.Deskripsi + " Sera" : x.Deskripsi,
                                                Email = y1.Email,
                                                RuleID = x.RuleID
                                            }).ToList();

                        ViewState.Add("Approval", dataApproval);

                        gvApproval.DataSource = dataApproval;
                        gvApproval.DataBind();

                        pnlApproval.Visible = false;
                        pnlRequiredAppval.Visible = true;

                        if (dataApproval.Count > 0)
                        {
                            pnlApproval.Visible = true;
                            pnlRequiredAppval.Visible = false;
                        }
                    }
                }
                else if (strSubGolongan == "A2" || strSubGolongan == "S3")
                {
                    using (var ctx = new dsSPDDataContext())
                    {
                        ViewState.Remove("Approval");
                        //ViewState.Clear();
                        var dataApproval = (from x in ctx.ApprovalRules
                                            join y in ctx.ApprovalStatus
                                            on new { X1 = x.RuleID, X2 = txNoSPD.Text } equals new { X1 = y.RuleID, X2 = y.NoSPD } into aps
                                            from y1 in aps.DefaultIfEmpty()
                                            where
                                                  x.Tipe == ddlTujuan.SelectedValue &&
                                                  x.TipeDetail == ddlAsal.SelectedValue &&
                                                  x.Golongan == "IV" &&
                                                  x.Posisi.Equals(ddlPosisi.SelectedValue)
                                            select new Approval
                                            {
                                                NrpApproval = y1.NrpApproval,
                                                Nama = y1.Nama,
                                                IndexLevel = x.IndexLevel,
                                                Deskripsi =  x.Deskripsi == "Presiden Director" ? x.Deskripsi + " Sera" : x.Deskripsi,
                                                Email = y1.Email,
                                                RuleID = x.RuleID
                                            }).ToList();

                        ViewState.Add("Approval", dataApproval);

                        gvApproval.DataSource = dataApproval;
                        gvApproval.DataBind();

                        pnlApproval.Visible = false;
                        pnlRequiredAppval.Visible = true;

                        if (dataApproval.Count > 0)
                        {
                            pnlApproval.Visible = true;
                            pnlRequiredAppval.Visible = false;
                        }
                    }
                }
            }
            else
            {
                using (var ctx = new dsSPDDataContext())
                {
                    ViewState.Remove("Approval");
                    //ViewState.Clear();
                    var dataApproval = (from x in ctx.ApprovalRules
                                        join y in ctx.ApprovalStatus
                                        on new { X1 = x.RuleID, X2 = txNoSPD.Text } equals new { X1 = y.RuleID, X2 = y.NoSPD } into aps
                                        from y1 in aps.DefaultIfEmpty()
                                        where
                                              x.Tipe == ddlTujuan.SelectedValue &&
                                              x.TipeDetail == ddlAsal.SelectedValue &&
                                              x.Golongan == ddlGolongan.SelectedValue &&
                                              x.Posisi.Equals(ddlPosisi.SelectedValue)
                                        select new Approval
                                        {
                                            NrpApproval = y1.NrpApproval,
                                            Nama = y1.Nama,
                                            IndexLevel = x.IndexLevel,
                                            Deskripsi = x.Deskripsi == "Presiden Director" ? x.Deskripsi + " Sera" : x.Deskripsi,
                                            Email = y1.Email,
                                            RuleID = x.RuleID
                                        }).OrderBy(x=>x.IndexLevel).ToList();

                    ViewState.Add("Approval", dataApproval);

                    gvApproval.DataSource = dataApproval;
                    gvApproval.DataBind();

                    pnlApproval.Visible = false;
                    pnlRequiredAppval.Visible = true;

                    if (dataApproval.Count > 0)
                    {
                        pnlApproval.Visible = true;
                        pnlRequiredAppval.Visible = false;
                    }
                }
            }
        }

        List<ListValueApproverModel> DataInput = new List<ListValueApproverModel>();
        public List<ListValueApproverModel> getApproveAtasan(string[] nrpArr)
        {
            var ctxOrg = new dsSPDDataContext(Konstan.ConnectionString);
            using (var ctx = new dsSPDDataContext())
            {
                var _OuRep = (from a in ctxOrg.OrgUnits
                              where nrpArr.Contains(a.Nrp.ToString())
                              select new
                              {
                                  a.OuRep
                              }).AsEnumerable().Select(x => string.Format("{0}", x.OuRep)).ToArray();

                string[] _OuRepArr = _OuRep;

                var atasan = (from a in ctxOrg.OrgUnits.AsEnumerable()
                              where
                                _OuRepArr.Contains(a.OuId)
                                && int.Parse(a.JobAbbreviation.Split('_')[0]) >= 90
                                && a.OuEnd >= DateTime.Now
                              select new
                              {
                                  a.Nrp
                              }).AsEnumerable().Select(x => string.Format("{0}", x.Nrp)).ToArray();

                string[] atasanArr = atasan;
                if (atasan.Length > 0)
                {
                    DataInput = (from a in ctx.msKaryawans
                                 where atasanArr.Contains(a.nrp)
                                 select new ListValueApproverModel
                                 {
                                     ID = a.nrp,
                                     Text = a.namaLengkap + " - " + a.nrp,
                                     Email = a.EMail
                                 }).ToList();
                }
                else
                {
                    getApproveAtasan(atasanArr);
                }

                return DataInput;
            }
        }

        public class ListValueApproverModel
        {
            public string ID { get; set; }
            public string Text { get; set; }
            public string NRP { get; set; }
            public string Email { get; set; }
            public string Position { get; set; }
            public string OuId { get; set; }
            public string NamaLengkap { get; set; }
            public string JobAbservation { get; set; }
            public string IndexLevel { get; set; }
            public string Desckripsi { get; set; }
            public string RuleID { get; set; }
            public string ReportUIID { get; set; }
            public string ApproverRule { get; set; }
        }

        void angkutanLain(bool enable)
        {
            txAngkutanLain.Enabled = enable;
            rvAngkutanLain.Enabled = enable;
        }

        void enableUangMukaAndAlasan()
        {
            if (!string.IsNullOrEmpty(txtTglBerangkat.Text) && !string.IsNullOrEmpty(txtTglKembali.Text))
            {
                TimeSpan Jumlahhari = (Convert.ToDateTime(txtTglKembali.Text) - Convert.ToDateTime(txtTglBerangkat.Text));
                //TimeSpan Jumlahhari = DateTime.ParseExact(txtTglKembali.Text, "MM/dd/yyyy", CultureInfo.InvariantCulture) - DateTime.ParseExact(txtTglBerangkat.Text, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                if (Jumlahhari.Days >= 5)
                {
                    txUangMuka.ReadOnly = false;
                }
                else
                {
                    txUangMuka.ReadOnly = true;
                }

                if (Jumlahhari.Days > 0)
                {
                    txAlasan.Enabled = true;
                    rvAlasan.Enabled = true;
                    reAlasan.Enabled = true;
                }
                else
                {
                    txAlasan.Enabled = false;
                    rvAlasan.Enabled = false;
                    reAlasan.Enabled = false;
                }
            }
        }

        void isSec(string nrp)
        {
            using (var ctx = new dsSPDDataContext())
            {
                var user = (from u in ctx.msUsers
                            join k in ctx.msKaryawans on u.nrp equals k.nrp
                            where u.roleId == 23 && k.nrp == nrp
                            orderby k.namaLengkap
                            select new
                            {
                                namaLengkap = k.namaLengkap,
                                nrp = k.nrp
                            }).Distinct();
                if (user.Count() > 0)
                {
                    pnlSekertaris.Visible = true;
                    rvSelfOrDirect.Enabled = true;
                }
                else
                {
                    pnlSekertaris.Visible = false;
                    rvSelfOrDirect.Enabled = false;
                }
            }
        }

        void setDirect()
        {
            using (var ctx = new dsSPDDataContext())
            {
                ddlDireksi.Visible = true;
                rvDireksi.Enabled = true;
                ddDireksi.Visible = true;

                ddlDireksi.Items.Clear();

                List<SelectListItem> direksi = new List<SelectListItem>();
                direksi.Add(new SelectListItem() { id = "", text = " - Select One - " });

                direksi.AddRange((from p in ctx.msUsers
                                  join k in ctx.msKaryawans on p.nrp equals k.nrp
                                  where p.roleId == 14 || p.roleId == 13
                                  orderby k.namaLengkap
                                  select new SelectListItem
                                  {
                                      id = k.nrp,
                                      text = k.namaLengkap
                                  }).Distinct().ToList());

                ddlDireksi.DataSource = direksi;

                ddlDireksi.DataValueField = "id";
                ddlDireksi.DataTextField = "text";

                ddlDireksi.DataBind();
                ddlDireksi.SelectedValue = string.Empty;
            }
        }

        // disable hotel jika tanggal sama, validasi tanggal berangkat tidak boleh lebih kecil dari pada kembali
        void setDisabledHotelAndNotLessThan()
        {
            DateTime? berangkat = thisDateTime(txtTglBerangkat.Text, "00"), kembali = thisDateTime(txtTglKembali.Text, "00");

            ddlPenginapan.SelectedValue = "Disediakan";
            ddlPenginapan.Enabled = true;

            if (berangkat == kembali)
            {
                ddlPenginapan.SelectedValue = "Tidak disediakan";
                ddlPenginapan.Enabled = false;
            }

            if (berangkat > kembali) lblDateLessThan.Visible = true;
            else lblDateLessThan.Visible = false;
        }

        public bool validateDate()
        {
            DateTime? berangkat = DateTime.Now, kembali = berangkat;
            if (!string.IsNullOrEmpty(txtTglBerangkat.Text) &&
                !string.IsNullOrEmpty(ddlJamBerangkat.SelectedValue) &&
                !string.IsNullOrEmpty(txtTglKembali.Text) &&
                !string.IsNullOrEmpty(ddlJamKembali.SelectedValue))
            {
                berangkat = thisDateTime(txtTglBerangkat.Text, ddlJamBerangkat.SelectedValue);
                kembali = thisDateTime(txtTglKembali.Text, ddlJamKembali.SelectedValue);
            }
            if (kembali <= berangkat) return false;
            else return true;
        }

        private DateTime? thisDateTime(string date, string time)
        {
            try
            {
                DateTime newDate = Convert.ToDateTime(date);
                double newTime = Convert.ToDouble(time);
                return new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 0).AddHours(newTime);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void setDetailSPD()
        {
            using (var ctx = new dsSPDDataContext())
            {

                Menu menu = (Menu)Master.FindControl("mnuMain");
                menu.Visible = false;
                LoadDdlCompanyTujuan();
                BindDropdownProvinsi();
                BindKotaAsalTujuan();

                //Asal
                trSPD spd = ctx.trSPDs.FirstOrDefault(o => o.noSPD == NoSPD.ToString());
                msKaryawan karyawan = ctx.msKaryawans.Where(x => x.nrp == spd.nrp).FirstOrDefault();
                setInitial(karyawan);
                ddlGolongan.DataBind();

                ddlPosisi.SelectedValue = spd.posisi;
                txNoSPD.Text = spd.noSPD;
                txNoPonsel.Text = spd.NoHP;

                //tujuan
                if (spd.Tujuan == "Dalam Negeri")
                    ddlTujuan.SelectedValue = "DN";
                else
                    ddlTujuan.SelectedValue = "LN";
                ddlTujuan.Enabled = false;

                ddlCompanyTujuan.SelectedValue = spd.coCdTujuan; ddlCompanyTujuan.Enabled = false;
                ddlCompanyTujuan_SelectedIndexChanged(ddlCompanyTujuan, EventArgs.Empty);

                ViewState["kodePATujuan"] = spd.kodePATujuan;
                ddlPersonalAreaTujuan.SelectedValue = spd.kodePATujuan; ddlPersonalAreaTujuan.Enabled = false;
                ddlPersonalAreaTujuan_SelectedIndexChanged(ddlPersonalAreaTujuan, EventArgs.Empty);
                
                ddlPSubAreaTujuan.SelectedValue = spd.kodePSubAreaTujuan; ddlPSubAreaTujuan.Enabled = false;

                txTempatTujuanLain.Text = spd.tempatTujuanLain; txTempatTujuanLain.Enabled = false;
                ddlKeperluan.SelectedValue = spd.idKeperluan.ToString(); ddlKeperluan.Enabled = false;
                txKetKeperluan.Text = spd.ketKeperluan; txKetKeperluan.Enabled = false;

                txtTglBerangkat.Text = spd.tglBerangkat.ToString(); 
                ddlJamBerangkat.SelectedValue = spd.jamBerangkat;   
                txtTglKembali.Text = spd.tglKembali.ToString();     
                ddlJamKembali.SelectedValue = spd.jamKembali;       

                txtTglExp.Text = spd.tglExpired.ToString(); 
                txAlasan.Text = spd.Alasan; txAlasan.Enabled = false;
                ddlCostCenter.SelectedValue = spd.costCenter; ddlCostCenter.Enabled = false;
                txKeterangan.Text = spd.keterangan; txKeterangan.Enabled = false;
                ddlWilayahTujuan.SelectedValue = spd.WilayahTujuan; ddlWilayahTujuan.Enabled = false;

                //informasi transportasi
                ddlAngkutan.SelectedValue = spd.idAngkutan.ToString(); ddlAngkutan.Enabled = false;
                txAngkutanLain.Text = spd.angkutanLain; txAngkutanLain.Enabled = false;
                rvTipePesawat.Enabled = false;
                ddlPenginapan.SelectedValue = spd.penginapan; ddlPenginapan.Enabled = false;
                txUangMuka.Text = spd.uangMuka; txUangMuka.Enabled = false;

                // get informasi plafon buat spd ini
                var GolonganPlafon = ctx.msGolonganPlafons.Where(
                                       q => q.golongan.Equals(spd.idGolongan) &&
                                       q.jenisSPD.ToLower().Equals(spd.Tujuan) &&
                                       q.wilayah.ToLower().Equals(spd.WilayahTujuan));

                // itung jumlah hari
                TimeSpan Jumlahhari = spd.tglKembali - spd.tglBerangkat;
                Jumlahhari += TimeSpan.FromDays(1);

                var UangMakan = GolonganPlafon.FirstOrDefault(o => o.idPlafon == 1);
                txBiayaMakan.Text = (UangMakan != null ? UangMakan.harga.Value * Jumlahhari.Days : 0).ToString();

                var UangSaku = GolonganPlafon.FirstOrDefault(o => o.idPlafon == 2);
                txUangSaku.Text = (UangSaku != null ? UangSaku.harga.Value * Jumlahhari.Days : 0).ToString();

                txTransportasi.Text = "";

                DataTable dtTunjangan = new DataTable();
                dtTunjangan.Columns.Add(new DataColumn("RowNumber", typeof(string)));
                dtTunjangan.Columns.Add(new DataColumn("Column1", typeof(string)));
                dtTunjangan.Columns.Add(new DataColumn("Column2", typeof(string)));
                dtTunjangan.Columns.Add(new DataColumn("Column3", typeof(string)));
                int RowNumber = 1;
                DataTable dt = new DataTable();
                if (spd.idTunjanganKejauhan != null && spd.idTunjanganKejauhan > 0 && spd.hariTunjangan1 != null)
                {
                    //provinsi_SelectedIndexChanged(listProvinsi.SelectedValue, EventArgs.Empty);
                    listProvinsi.ClearSelection();
                    dt = hp.getHardship(spd.idTunjanganKejauhan.Value);
                    listProvinsi.SelectedValue = dt.Rows[0]["PropinsiId"].ToString();
                    ViewState["PropinsiId"]    = dt.Rows[0]["PropinsiId"].ToString();
                    DataRow newRow = null;
                    newRow = dtTunjangan.NewRow();
                    newRow["RowNumber"] = RowNumber;
                    newRow["Column1"] = dt.Rows[0]["Id"].ToString();
                    newRow["Column2"] = dt.Rows[0]["Harga"].ToString();
                    newRow["Column3"] = spd.hariTunjangan1;
                    dtTunjangan.Rows.Add(newRow);

                    if (spd.idTunjanganKejauhan2 != null && spd.idTunjanganKejauhan2 > 0 && spd.hariTunjangan2 != null)
                    {
                        dt = hp.getHardship(spd.idTunjanganKejauhan2.Value);
                        DataRow newRow2 = null;
                        newRow2 = dtTunjangan.NewRow();
                        newRow2["RowNumber"] = RowNumber + 1;
                        newRow2["Column1"] = dt.Rows[0]["Id"].ToString();
                        newRow2["Column2"] = dt.Rows[0]["Harga"].ToString();
                        newRow2["Column3"] = spd.hariTunjangan2;
                        dtTunjangan.Rows.Add(newRow2);

                        if (spd.idTunjanganKejauhan3 != null && spd.idTunjanganKejauhan3 > 0 && spd.hariTunjangan3 != null)
                        {
                            dt = hp.getHardship(spd.idTunjanganKejauhan3.Value);
                            DataRow newRow3 = null;
                            newRow3 = dtTunjangan.NewRow();
                            newRow3["RowNumber"] = RowNumber + 1;
                            newRow3["Column1"] = dt.Rows[0]["Id"].ToString();
                            newRow3["Column2"] = dt.Rows[0]["Harga"].ToString();
                            newRow3["Column3"] = spd.hariTunjangan3;
                            dtTunjangan.Rows.Add(newRow3);

                            if (spd.idTunjanganKejauhan4 != null && spd.idTunjanganKejauhan4 > 0 && spd.hariTunjangan4 != null)
                            {
                                dt = hp.getHardship(spd.idTunjanganKejauhan4.Value);
                                DataRow newRow4 = null;
                                newRow4 = dtTunjangan.NewRow();
                                newRow4["RowNumber"] = RowNumber + 1;
                                newRow4["Column1"] = dt.Rows[0]["Id"].ToString();
                                newRow4["Column2"] = dt.Rows[0]["Harga"].ToString();
                                newRow4["Column3"] = spd.hariTunjangan4;
                                dtTunjangan.Rows.Add(newRow4);

                                if (spd.idTunjanganKejauhan5 != null && spd.idTunjanganKejauhan5 > 0 && spd.hariTunjangan5 != null)
                                {
                                    dt = hp.getHardship(spd.idTunjanganKejauhan5.Value);
                                    DataRow newRow5 = null;
                                    newRow5 = dtTunjangan.NewRow();
                                    newRow5["RowNumber"] = RowNumber + 1;
                                    newRow5["Column1"] = dt.Rows[0]["Id"].ToString();
                                    newRow5["Column2"] = dt.Rows[0]["Harga"].ToString();
                                    newRow5["Column3"] = spd.hariTunjangan5;
                                    dtTunjangan.Rows.Add(newRow5);
                                }
                            }
                        }
                    }
                }

                ViewState["CurrentTable1"] = dtTunjangan;
                gvTunjanganHarship.DataSource = dtTunjangan;
                gvTunjanganHarship.DataBind();

                if (spd.idBiayaTransportasi != null && spd.idBiayaTransportasi > 0)
                {
                    DataTable dt1 = hp.getBiayaTransportasi(spd.idBiayaTransportasi.Value);
                    ddlTipePesawat.Text = dt1.Rows[0]["TipePesawat"].ToString();
                    ddlKotaAsal.SelectedValue = dt1.Rows[0]["IDKotaAsal"].ToString();
                    ddlKotaTujuan.SelectedValue = dt1.Rows[0]["IDKotaTujuan"].ToString();
                }

                //approval
                setContent();
                setApproval();
                approvalViewState();
                LoadApprovalADH(spd.costCenter);
                ddlApprovalTujuan.SelectedValue = spd.nrpApprovalTujuan;
                ddlSubGolongan.Visible = false;
            }
        }

        //============== try TO DO OrgUnit Command ============== start
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static List<SelectListItem> GetAtasan(string searchText, string additionalFilter, int page, string posisi, string tujuan)
        {
            using (var ctx = new dsSPDDataContext())
            {
                List<SelectListItem> data = new List<SelectListItem>();

                if (!string.IsNullOrEmpty(posisi))
                {
                    SqlCommand cmd = new SqlCommand("sp_GetAtasan", new SqlConnection(ConfigurationManager.AppSettings["SPDConnectionString1"]));
                    cmd.Parameters.AddWithValue("@namaLengkap", searchText.ToLower());
                    cmd.Parameters.AddWithValue("@position", posisi.ToLower());
                    cmd.Parameters.AddWithValue("@nrp", karyawan.nrp);
                    cmd.Parameters.AddWithValue("@tujuan", tujuan);
                    cmd.Parameters.AddWithValue("@coCd", karyawan.coCd);
                    cmd.Parameters.AddWithValue("@golongan", karyawan.payscalegroup == null ? "" : karyawan.payscalegroup);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Connection.Open();
                    SqlDataReader reader;
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        SelectListItem item = new SelectListItem();
                        item.id = reader["nrp"].ToString();
                        item.text = reader["namaLengkap"].ToString() + '-' + reader["nrp"].ToString();
                        data.Add(item);
                    }
                    if (data != null)
                    {
                        data = data.Skip(10 * (page - 1)).Take(10).ToList();
                    }
                    cmd.Connection.Close();
                    cmd.Connection.Dispose();
                }
                else
                {
                    data = (from p in ctx.msKaryawans
                            where
                            p.namaLengkap.ToLower().Contains(searchText.ToLower()) && p.nrp != karyawan.nrp
                            orderby p.namaLengkap
                            select new SelectListItem
                            {
                                id = p.nrp,
                                text = p.namaLengkap + '-' + p.nrp,
                            }).Skip(10 * (page - 1)).Take(10).ToList();
                }

                return data;
            }
        }
        public static List<SelectListItem> GetApproveAtasan(List<string> nrps, int page, string posisi, int index)
        {
            List<SelectListItem> datas = new List<SelectListItem>();
            DateTime now = DateTime.Now;

            using (var ctx = new dsSPDDataContext())
            {
                //get data OuName
                var OuName = (from a in ctx.vwOrgUnits
                              where
                                nrps.Contains(a.Nrp.ToString())
                                && (a.OuStart <= now && a.OuEnd >= now)
                                && (a.PosStart <= now && a.PosEnd >= now)
                                && (a.JobStart <= now && a.JobEnd >= now)
                                && (a.JobStart <= now && a.JobEnd >= now)
                                && (a.PerStart <= now && a.PerEnd >= now)
                                && (a.CcStart <= now && a.CcEnd >= now)
                              select new
                              {
                                  a.OuName
                              }).FirstOrDefault();

                if (OuName != null)
                {
                    //cek OuName SERA Director atau bukan jika iya, maka dropdown menampilkan nrp dan nama finance director
                    if (OuName.OuName == "SERA Director")
                    {
                        return (from mp in ctx.msPositions
                                join ou in ctx.vwOrgUnits on mp.PositionId equals ou.PosId
                                join mk in ctx.msKaryawans on ou.Nrp.ToString() equals mk.nrp
                                where
                                    mp.ApprovalRule == "Finance Director"
                                    && (ou.OuStart <= now && ou.OuEnd >= now)
                                    && (ou.PosStart <= now && ou.PosEnd >= now)
                                    && (ou.JobStart <= now && ou.JobEnd >= now)
                                    && (ou.JobStart <= now && ou.JobEnd >= now)
                                    && (ou.PerStart <= now && ou.PerEnd >= now)
                                    && (ou.CcStart <= now && ou.CcEnd >= now)
                                select new SelectListItem
                                {
                                    id = mk.nrp,
                                    text = mk.namaLengkap + '-' + mk.nrp,
                                }).Skip(10 * (page - 1)).Take(10).ToList();
                    }
                }

                //jika ouname bukan Finance Director
                var _Ou = (from a in ctx.vwOrgUnits
                           where
                                nrps.Contains(a.Nrp.ToString())
                                && (a.OuStart <= now && a.OuEnd >= now)
                                && (a.PosStart <= now && a.PosEnd >= now)
                                && (a.JobStart <= now && a.JobEnd >= now)
                                && (a.JobStart <= now && a.JobEnd >= now)
                                && (a.PerStart <= now && a.PerEnd >= now)
                                && (a.CcStart <= now && a.CcEnd >= now)
                           select new
                           {
                               a.OuId
                                ,
                               a.OuRep
                           }).FirstOrDefault();

                var _OuIdData = (from a in ctx.vwOrgUnits
                                 where
                                    a.OuId == _Ou.OuId
                                    && a.PosChief != "0"
                                    && !nrps.Contains(a.Nrp.ToString())
                                    && (a.OuStart <= now && a.OuEnd >= now)
                                    && (a.PosStart <= now && a.PosEnd >= now)
                                    && (a.JobStart <= now && a.JobEnd >= now)
                                    && (a.JobStart <= now && a.JobEnd >= now)
                                    && (a.PerStart <= now && a.PerEnd >= now)
                                    && (a.CcStart <= now && a.CcEnd >= now)
                                 select new
                                 {
                                     a.Nrp
                                 }).AsEnumerable().Select(x => string.Format("{0}", x.Nrp)).ToList();

                if (_OuIdData.Count == 0)
                {
                    var _OuRepData = (from a in ctx.vwOrgUnits
                                      where
                                        a.OuId == _Ou.OuRep
                                        && a.PosChief != "0"
                                        && !nrps.Contains(a.Nrp.ToString())
                                        && (a.OuStart <= now && a.OuEnd >= now)
                                        && (a.PosStart <= now && a.PosEnd >= now)
                                        && (a.JobStart <= now && a.JobEnd >= now)
                                        && (a.JobStart <= now && a.JobEnd >= now)
                                        && (a.PerStart <= now && a.PerEnd >= now)
                                        && (a.CcStart <= now && a.CcEnd >= now)
                                      select new
                                      {
                                          a.Nrp
                                      }).AsEnumerable().Select(x => string.Format("{0}", x.Nrp)).ToList();
                    datas = (from a in ctx.msKaryawans
                             where _OuRepData.Contains(a.nrp)
                             select new SelectListItem
                             {
                                 id = a.nrp,
                                 text = a.namaLengkap + " - " + a.nrp
                             }).Skip(10 * (page - 1)).Take(10).ToList();

                    if (datas.Count > 0)
                    {
                        return datas;
                    }
                    else
                    {
                        var dataJob = (from a in ctx.vwOrgUnits
                                       where
                                           nrps.Contains(a.Nrp.ToString())
                                           && (a.OuStart <= now && a.OuEnd >= now)
                                           && (a.PosStart <= now && a.PosEnd >= now)
                                           && (a.JobStart <= now && a.JobEnd >= now)
                                           && (a.JobStart <= now && a.JobEnd >= now)
                                           && (a.PerStart <= now && a.PerEnd >= now)
                                           && (a.CcStart <= now && a.CcEnd >= now)
                                       select new
                                       {
                                           a.JobAbbreviation
                                       }).FirstOrDefault();

                        if (int.Parse(dataJob.JobAbbreviation.Split('_')[0]) == 120)
                        {
                            datas = (from a in ctx.msKaryawans
                                     where nrps.Contains(a.nrp)
                                     select new SelectListItem
                                     {
                                         id = a.nrp,
                                         text = a.namaLengkap + " - " + a.nrp
                                     }).Skip(10 * (page - 1)).Take(10).ToList();
                        }

                        return datas;
                    }
                }
                else
                {
                    datas = (from a in ctx.msKaryawans
                             where _OuIdData.Contains(a.nrp)
                             select new SelectListItem
                             {
                                 id = a.nrp,
                                 text = a.namaLengkap + " - " + a.nrp
                             }).Skip(10 * (page - 1)).Take(10).ToList();

                    return datas;
                }
            }
        }

        public DataTable Result {

            get
            {
                return (DataTable)ViewState["dataTable"];
            }
            set
            {
                ViewState["dataTable"] = value;
            }
        }

        public class SelectListItem
        {
            public string id { get; set; }
            public string text { get; set; }
        }

        [Serializable]
        public class Approval
        {
            public string NrpApproval { get; set; }
            public string Nama { get; set; }
            public int? IndexLevel { get; set; }
            public string Deskripsi { get; set; }
            public string Email { get; set; }
            public int RuleID { get; set; }
        }

        private void LoadDdlCompanyTujuan()
        {
            ddlCompanyTujuan.Items.Clear();
            ddlCompanyTujuan.Items.Add(new ListItem("- Select One -"));
            ddlCompanyTujuan.Items.Add(new ListItem("LAIN - LAIN", "0"));

            ddlCompanyTujuan.AppendDataBoundItems = true;

            using (var ctx = new dsSPDDataContext())
            {
                var data = (from a in ctx.msKaryawans
                            select new
                            {
                                a.coCd,
                                a.companyCode
                            }).Distinct().ToList();

                ddlCompanyTujuan.DataSource = data;
                ddlCompanyTujuan.DataTextField = "companyCode";
                ddlCompanyTujuan.DataValueField = "coCd";
                ddlCompanyTujuan.DataBind();
            }
        }

        protected void ddlSubGolongan_SelectedIndexChanged(object sender, EventArgs e)
        {
            string golongan = "";
            string strSubGolongan = ddlGolongan.SelectedValue.Trim() + ddlSubGolongan.SelectedValue.Trim();
            if (strSubGolongan == "A0" || strSubGolongan == "A1" || strSubGolongan == "C0" || strSubGolongan == "C1" || strSubGolongan == "C2" || strSubGolongan == "C3" || strSubGolongan == "S0" || strSubGolongan == "S1" || strSubGolongan == "S2")
            {
                golongan = "III";
            }
            else if (strSubGolongan == "A2" || strSubGolongan == "S3")
            {
                golongan = "IV";
            }

            using (var ctx = new dsSPDDataContext())
            {
                // plafon
                var data = from p in ctx.msGolonganPlafons
                           where p.golongan.Equals(golongan.Trim()) && p.jenisSPD.Equals(ddlTujuan.SelectedItem.Text)
                           select p;

                var plafon = data.Where(p => p.idPlafon == 1).ToList();

                if (plafon.Count > 0) txBiayaMakan.Text = plafon.First().harga == null ? "" : plafon.First().harga.ToString();

                plafon = data.Where(p => p.idPlafon == 2).ToList();

                if (plafon.Count > 0) txUangSaku.Text = plafon.First().harga == null ? "" : plafon.First().harga.ToString();

                plafon = data.Where(p => p.idPlafon == 3).ToList();

                if (plafon.Count > 0) txTransportasi.Text = plafon.First().deskripsi == null ? "" : plafon.First().deskripsi;
            }

            setApproval();
        }

        protected void listLokasi1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddl = (DropDownList)sender;
            var row = ddl.NamingContainer;

            GridViewRow row1 = (GridViewRow)ddl.NamingContainer;
            int RowIndex = row1.RowIndex;
            TextBox txt = (TextBox)row.FindControl("harga1");
            txt.Text = hp.getHarshipValue(int.Parse(ddl.SelectedValue));
            if (RowIndex == 0)
            {
                TextBox txt2 = (TextBox)row.FindControl("jumlahHari");
                TimeSpan Jumlahhari = DateTime.Parse(txtTglKembali.Text.ToString()) - DateTime.Parse(txtTglBerangkat.Text.ToString());
                Jumlahhari += TimeSpan.FromDays(1);
                txt2.Text = Jumlahhari.Days.ToString();
            }
        }

        protected void ButtonAddLokasi_Click(object sender, EventArgs e)
        {
            AddNewRowToGrid();           
        }

        private void AddNewRowToGrid()
        {
            if (ViewState["CurrentTable"] != null)
            {
                DataTable dtCurrentTable = (DataTable)ViewState["CurrentTable"];
                DataRow drCurrentRow = null;
                if (dtCurrentTable.Rows.Count > 0)
                {
                    drCurrentRow = dtCurrentTable.NewRow();//new row

                    drCurrentRow["RowNumber"] = dtCurrentTable.Rows.Count + 1;//fill new row

                    dtCurrentTable.Rows.Add(drCurrentRow); //add new row

                    ViewState["CurrentTable"] = dtCurrentTable;//update viewstate table

                    for (int i = 0; i < dtCurrentTable.Rows.Count - 1 ; i++) //0,1
                    {
                        DropDownList ddl1 = (DropDownList)gvTunjanganHarship.Rows[i].Cells[1].FindControl("listLokasi1");//find dropdown lokasi

                        TextBox box1 = (TextBox)gvTunjanganHarship.Rows[i].Cells[2].FindControl("harga1");//find textbox harga

                        TextBox box2 = (TextBox)gvTunjanganHarship.Rows[i].Cells[3].FindControl("jumlahHari");//find textbox jml hari
                        
                        dtCurrentTable.Rows[i]["Column1"] = ddl1.SelectedItem.Text;//fill table

                        dtCurrentTable.Rows[i]["Column2"] = box1.Text;//fill table

                        dtCurrentTable.Rows[i]["Column3"] = box2.Text;//fill table
                    }
                    if (dtCurrentTable.Rows.Count == 5)
                    {
                        Button addButtton = (Button)gvTunjanganHarship.FooterRow.FindControl("ButtonAddLokasi");
                        addButtton.Enabled = false;
                    }
                    ViewState["CurrentTable"] = dtCurrentTable;

                    gvTunjanganHarship.DataSource = dtCurrentTable;

                    gvTunjanganHarship.DataBind();

                }
            }
            else
            {
                Response.Write("ViewState is null");
            }
            SetPreviousData();
        }

        private void SetPreviousData()
        {
            int rowIndex = 0;

            if (ViewState["CurrentTable"] != null)
            {
                DataTable dt = (DataTable)ViewState["CurrentTable"];
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DropDownList ddl1 = (DropDownList)gvTunjanganHarship.Rows[rowIndex].Cells[1].FindControl("listLokasi1");

                        TextBox box2 = (TextBox)gvTunjanganHarship.Rows[i].Cells[2].FindControl("harga1");

                        TextBox box3 = (TextBox)gvTunjanganHarship.Rows[i].Cells[3].FindControl("jumlahHari");

                        FillDropDownList(ddl1);

                        if (i < dt.Rows.Count - 1)
                        {
                            ddl1.ClearSelection();

                            ddl1.Items.FindByText(dt.Rows[i]["Column1"].ToString()).Selected = true;

                            box2.Text = dt.Rows[i]["Column2"].ToString();

                            box3.Text = dt.Rows[i]["Column3"].ToString();
                        }
                        rowIndex++;
                    }
                }
            }
        }
        protected void LinkRemove_Click(object sender, EventArgs e)
        {
            LinkButton lb = (LinkButton)sender;
            GridViewRow gvRow = (GridViewRow)lb.NamingContainer;
            int rowID = gvRow.RowIndex;
            if (ViewState["CurrentTable"] != null)
            {
                DataTable dt = (DataTable)ViewState["CurrentTable"];
                if (dt.Rows.Count > 1)
                {
                    if (gvRow.RowIndex < dt.Rows.Count - 1)
                    { 
                        dt.Rows.Remove(dt.Rows[rowID]);
                        ResetRowID(dt);
                    }
                }
                ViewState["CurrentTable"] = dt;
                gvTunjanganHarship.DataSource = dt;
                gvTunjanganHarship.DataBind();
            }
            SetPreviousData();
        }

        private void ResetRowID(DataTable dt)
        {
            int rowNumber = 1;
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    row[0] = rowNumber;
                    rowNumber++;
                }
            }
        }

        protected void gvTunjanganHarship_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                Action = Request.QueryString["Action"];
                DataTable dt = new DataTable();
                if (Action == "Revisi")
                    dt = (DataTable)ViewState["CurrentTable1"];
                else
                    dt = (DataTable)ViewState["CurrentTable"];

                
                LinkButton lb = (LinkButton)e.Row.FindControl("LinkRemove");
                if (lb != null)
                {
                    if (dt != null)
                    {
                        if (dt.Rows.Count > 1)
                        {
                            if (e.Row.RowIndex == dt.Rows.Count - 1)
                            {
                                lb.Visible = false;
                            }
                        }
                        else
                        {
                            lb.Visible = false;
                        }
                    }
                }
            }
        }

        protected void gvTunjanganHarship_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (Action == "Revisi")
                {
                    DataTable dt = (DataTable)ViewState["CurrentTable1"];
                    if (dt != null)
                    {
                        DropDownList ddl1 = (e.Row.FindControl("listLokasi1") as DropDownList);
                        TextBox box1 = (e.Row.FindControl("harga1") as TextBox);
                        TextBox box2 = (e.Row.FindControl("jumlahHari") as TextBox);
                        FillDropDownList(ddl1);
                        string Id = dt.Rows[e.Row.RowIndex]["Column1"].ToString();
                        ddl1.Items.FindByValue(Id).Selected = true;
                        box1.Text = dt.Rows[e.Row.RowIndex]["Column2"].ToString();
                        box2.Text = dt.Rows[e.Row.RowIndex]["Column3"].ToString();
                    }
                }
            }
        }
    }
}