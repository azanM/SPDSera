using eSPD.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace eSPD
{
  
    public partial class formInvoice : System.Web.UI.Page
    {
        private static msKaryawan karyawan = new msKaryawan();
        private static classSpd oSPD = new classSpd();
        private static string strID = string.Empty;
        string constr = ConfigurationManager.ConnectionStrings["SPDConnectionString1"].ConnectionString;
        String errMessage = "Tanggal terima invoice melebihi 9 hari dari tanggal issued";

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
                gvInvoice.Visible = true;
            }
            else
            {
                gvInvoice.Visible = false;
            }
            if (!IsPostBack)
            {
                BindGridView(1);
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

        protected void gvInvoice_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvInvoice.EditIndex = e.NewEditIndex;
            gvInvoice.Columns[1].Visible = false;
            BindGridView(gvInvoice.PageIndex + 1); 
        }

        int GetOfficeDays(DateTime dtmStart, DateTime dtmEnd)
        {
            int dowStart = ((int)dtmStart.DayOfWeek == 0 ? 7 : (int)dtmStart.DayOfWeek);
            int dowEnd = ((int)dtmEnd.DayOfWeek == 0 ? 7 : (int)dtmEnd.DayOfWeek);
            TimeSpan tSpan = dtmEnd - dtmStart;
            if (dowStart <= dowEnd)
            {
                return (((tSpan.Days / 7) * 5) + Math.Max((Math.Min((dowEnd + 1), 6) - dowStart), 0));
            }
            return (((tSpan.Days / 7) * 5) + Math.Min((dowEnd + 6) - Math.Min(dowStart, 6), 5));
        }

        bool isValidDate(String tglIssued, String tglInvoice)
        {
            bool result = true;
            try
            {
                int interval = GetOfficeDays(DateTime.Parse(tglInvoice).Date, DateTime.Parse(tglIssued).Date);
                if ((interval - 1) > 8)
                {
                    result = false;
                }
            }
            catch (Exception)
            {
                return true;
            }
            return result;
        }

        protected void gvInvoice_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            GridViewRow row = gvInvoice.Rows[e.RowIndex];
            LinkButton link = row.FindControl("noSPD") as LinkButton;
            gvInvoice.Columns[1].Visible = true;
            string noSPD = link.Text;
            string tglIssued1 = (row.FindControl("TextBox1") as TextBox).Text;
            string tglIssued2 = (row.FindControl("TextBox2") as TextBox).Text;
            string tglIssued3 = (row.FindControl("TextBox3") as TextBox).Text;
            string tglIssued4 = (row.FindControl("TextBox4") as TextBox).Text;

            string noInvoiceHotel1 = (row.FindControl("TextBox5") as TextBox).Text;
            string noInvoiceHotel2 = (row.FindControl("TextBox6") as TextBox).Text;
            string noInvoiceHotel3 = (row.FindControl("TextBox7") as TextBox).Text;
            string noInvoiceHotel4 = (row.FindControl("TextBox8") as TextBox).Text;

            string tglInvoiceHotel1 = (row.FindControl("TextBox9") as TextBox).Text;
            string tglInvoiceHotel2 = (row.FindControl("TextBox10") as TextBox).Text;
            string tglInvoiceHotel3 = (row.FindControl("TextBox11") as TextBox).Text;
            string tglInvoiceHotel4 = (row.FindControl("TextBox12") as TextBox).Text;

            string hargaHotel1 = (row.FindControl("TextBox13") as TextBox).Text;
            string hargaHotel2 = (row.FindControl("TextBox14") as TextBox).Text;
            string hargaHotel3 = (row.FindControl("TextBox15") as TextBox).Text;
            string hargaHotel4 = (row.FindControl("TextBox16") as TextBox).Text;

            string noInvoiceTiket1 = (row.FindControl("TextBox17") as TextBox).Text;
            string noInvoiceTiket2 = (row.FindControl("TextBox18") as TextBox).Text;
            string noInvoiceTiket3 = (row.FindControl("TextBox19") as TextBox).Text;
            string noInvoiceTiket4 = (row.FindControl("TextBox20") as TextBox).Text;

            string tglInvoiceTiket1 = (row.FindControl("TextBox21") as TextBox).Text;
            string tglInvoiceTiket2 = (row.FindControl("TextBox22") as TextBox).Text;
            string tglInvoiceTiket3 = (row.FindControl("TextBox23") as TextBox).Text;
            string tglInvoiceTiket4 = (row.FindControl("TextBox24") as TextBox).Text;

            string hargaTiket1 = (row.FindControl("TextBox25") as TextBox).Text;
            string hargaTiket2 = (row.FindControl("TextBox26") as TextBox).Text;
            string hargaTiket3 = (row.FindControl("TextBox27") as TextBox).Text;
            string hargaTiket4 = (row.FindControl("TextBox28") as TextBox).Text;
            
            string tglTerimaInvoice1 = (row.FindControl("TextBox29") as TextBox).Text + ' ' + (row.FindControl("TextBox33") as TextBox).Text;
            string tglTerimaInvoice2 = (row.FindControl("TextBox30") as TextBox).Text + ' ' + (row.FindControl("TextBox34") as TextBox).Text;
            string tglTerimaInvoice3 = (row.FindControl("TextBox31") as TextBox).Text + ' ' + (row.FindControl("TextBox35") as TextBox).Text;
            string tglTerimaInvoice4 = (row.FindControl("TextBox32") as TextBox).Text + ' ' + (row.FindControl("TextBox36") as TextBox).Text;
            
            using (var data = new dsSPDDataContext())
            {
                var listInvoice = data.InvoiceSPDs.Where(x => x.NoSPD != noSPD).Select(x=>new{
                    x.NoInvoiceHotelA, x.NoInvoiceHotelB, x.NoInvoiceHotelC, x.NoInvoiceHotelD
                    ,x.NoInvoiceTiketA, x.NoInvoiceTiketB,x.NoInvoiceTiketC, x.NoInvoiceTiketD}
                    ).ToList();

                
                if (!String.IsNullOrEmpty(noInvoiceHotel1))
                {
                    var invoice = getListEqualNoInvoice(listInvoice, noInvoiceHotel1);

                    if (invoice.Count() > 0)
                    {

                        labelA.Text = "No Invoice " + noInvoiceHotel1 + " Sudah Digunakan";
                        popupMessage.Show();
                        return;
                    }

                    if (isDuplicateInvoice(noInvoiceHotel1, row, "TextBox6", "TextBox7", "TextBox8", "TextBox17", "TextBox18", "TextBox19", "TextBox20"))
                    {
                        labelA.Text = "No Invoice " + noInvoiceHotel1 + " Sudah Digunakan";
                        popupMessage.Show();
                        return;
                    }
                }

                if (!String.IsNullOrEmpty(noInvoiceHotel2))
                {
                    var invoice = getListEqualNoInvoice(listInvoice, noInvoiceHotel2);

                    if (invoice.Count() > 0)
                    {
                        labelA.Text = "No Invoice " + noInvoiceHotel2 + " Sudah Digunakan";
                        popupMessage.Show();
                        return;
                    }
                    if (isDuplicateInvoice(noInvoiceHotel2, row, "TextBox5", "TextBox7", "TextBox8", "TextBox17", "TextBox18", "TextBox19", "TextBox20"))
                    {
                        labelA.Text = "No Invoice " + noInvoiceHotel2 + " Sudah Digunakan";
                        popupMessage.Show();
                        return;
                    }
                }

                if (!String.IsNullOrEmpty(noInvoiceHotel3))
                {
                    var invoice = getListEqualNoInvoice(listInvoice, noInvoiceHotel3);

                    if (invoice.Count() > 0)
                    {
                        labelA.Text = "No Invoice " + noInvoiceHotel3 + " Sudah Digunakan";
                        popupMessage.Show();
                        return;
                    }
                    if (isDuplicateInvoice(noInvoiceHotel3, row, "TextBox5", "TextBox6", "TextBox8", "TextBox17", "TextBox18", "TextBox19", "TextBox20"))
                    {
                        labelA.Text = "No Invoice " + noInvoiceHotel3 + " Sudah Digunakan";
                        popupMessage.Show();
                        return;
                    }
                }
                if (!String.IsNullOrEmpty(noInvoiceHotel4))
                {
                    var invoice = getListEqualNoInvoice(listInvoice, noInvoiceHotel4);

                    if (invoice.Count() > 0)
                    {
                        labelA.Text = "No Invoice " + noInvoiceHotel4 + " Sudah Digunakan";
                        popupMessage.Show();
                        return;
                    }
                    if (isDuplicateInvoice(noInvoiceHotel4, row, "TextBox5", "TextBox6", "TextBox7", "TextBox17", "TextBox18", "TextBox19", "TextBox20"))
                    {
                        labelA.Text = "No Invoice " + noInvoiceHotel4 + " Sudah Digunakan";
                        popupMessage.Show();
                        return;
                    }
                }

                if (!String.IsNullOrEmpty(noInvoiceTiket1))
                {
                    var invoice = getListEqualNoInvoice(listInvoice, noInvoiceTiket1);

                    if (invoice.Count() > 0)
                    {
                        labelA.Text = "No Invoice " + noInvoiceTiket1 + " Sudah Digunakan";
                        popupMessage.Show();
                        return;
                    }
                    if (isDuplicateInvoice(noInvoiceTiket1, row, "TextBox5", "TextBox6", "TextBox7", "TextBox8", "TextBox18", "TextBox19", "TextBox20"))
                    {
                        labelA.Text = "No Invoice " + noInvoiceTiket1 + " Sudah Digunakan";
                        popupMessage.Show();
                        return;
                    }
                }

                if (!String.IsNullOrEmpty(noInvoiceTiket2))
                {
                    var invoice = getListEqualNoInvoice(listInvoice, noInvoiceTiket2);

                    if (invoice.Count() > 0)
                    {
                        labelA.Text = "No Invoice " + noInvoiceTiket2 + " Sudah Digunakan";
                        popupMessage.Show();
                        return;
                    }
                    if (isDuplicateInvoice(noInvoiceTiket2, row, "TextBox5", "TextBox6", "TextBox7", "TextBox8", "TextBox17", "TextBox19", "TextBox20"))
                    {
                        labelA.Text = "No Invoice " + noInvoiceTiket2 + " Sudah Digunakan";
                        popupMessage.Show();
                        return;
                    }
                }
                if (!String.IsNullOrEmpty(noInvoiceTiket3))
                {
                    var invoice = getListEqualNoInvoice(listInvoice, noInvoiceTiket3);

                    if (invoice.Count() > 0)
                    {
                        labelA.Text = "No Invoice " + noInvoiceTiket3 + " Sudah Digunakan";
                        popupMessage.Show();
                        return;
                    }
                    if (isDuplicateInvoice(noInvoiceTiket3, row, "TextBox5", "TextBox6", "TextBox7", "TextBox8", "TextBox17", "TextBox18", "TextBox20"))
                    {
                        labelA.Text = "No Invoice " + noInvoiceTiket3 + " Sudah Digunakan";
                        popupMessage.Show();
                        return;
                    }
                }
                if (!String.IsNullOrEmpty(noInvoiceTiket4))
                {
                    var invoice = getListEqualNoInvoice(listInvoice, noInvoiceTiket4);

                    if (invoice.Count() > 0)
                    {
                        labelA.Text = "No Invoice " + noInvoiceTiket4 + " Sudah Digunakan";
                        popupMessage.Show();
                        return;
                    }
                    if (isDuplicateInvoice(noInvoiceTiket4, row, "TextBox5", "TextBox6", "TextBox7", "TextBox8", "TextBox17", "TextBox18", "TextBox19"))
                    {
                        labelA.Text = "No Invoice " + noInvoiceTiket4 + " Sudah Digunakan";
                        popupMessage.Show();
                        return;
                    }
                }

                var currentInvoice = data.InvoiceSPDs.Where(x => x.NoSPD.Equals(noSPD)).FirstOrDefault();
                DateTime? dateT = null;
                int? hargaDef = null;

                if (currentInvoice != null)
                {
                    currentInvoice.NoSPD = noSPD;
                    currentInvoice.TanggalIssuedA = String.IsNullOrEmpty(tglIssued1) ? dateT : DateTime.Parse(tglIssued1);
                    currentInvoice.TanggalIssuedB = String.IsNullOrEmpty(tglIssued2) ? dateT : DateTime.Parse(tglIssued2);
                    currentInvoice.TanggalIssuedC = String.IsNullOrEmpty(tglIssued3) ? dateT : DateTime.Parse(tglIssued3);
                    currentInvoice.TanggalIssuedD = String.IsNullOrEmpty(tglIssued4) ? dateT : DateTime.Parse(tglIssued4);

                    currentInvoice.NoInvoiceHotelA = noInvoiceHotel1;
                    currentInvoice.NoInvoiceHotelB = noInvoiceHotel2;
                    currentInvoice.NoInvoiceHotelC = noInvoiceHotel3;
                    currentInvoice.NoInvoiceHotelD = noInvoiceHotel4;


                    currentInvoice.TglInvoiceHotelA = String.IsNullOrEmpty(tglInvoiceHotel1) ? dateT : DateTime.Parse(tglInvoiceHotel1);
                    currentInvoice.TglInvoiceHotelB = String.IsNullOrEmpty(tglInvoiceHotel2) ? dateT : DateTime.Parse(tglInvoiceHotel2);
                    currentInvoice.TglInvoiceHotelC = String.IsNullOrEmpty(tglInvoiceHotel3) ? dateT : DateTime.Parse(tglInvoiceHotel3);
                    currentInvoice.TglInvoiceHotelD = String.IsNullOrEmpty(tglInvoiceHotel4) ? dateT : DateTime.Parse(tglInvoiceHotel4);

                    currentInvoice.HargaHotelA = String.IsNullOrEmpty(hargaHotel1) ? hargaDef : int.Parse(hargaHotel1);
                    currentInvoice.HargaHotelB = String.IsNullOrEmpty(hargaHotel2) ? hargaDef : int.Parse(hargaHotel2);
                    currentInvoice.HargaHotelC = String.IsNullOrEmpty(hargaHotel3) ? hargaDef : int.Parse(hargaHotel3);
                    currentInvoice.HargaHotelD = String.IsNullOrEmpty(hargaHotel4) ? hargaDef : int.Parse(hargaHotel4);
                    
                    currentInvoice.NoInvoiceTiketA = noInvoiceTiket1;
                    currentInvoice.NoInvoiceTiketB = noInvoiceTiket2;
                    currentInvoice.NoInvoiceTiketC = noInvoiceTiket3;
                    currentInvoice.NoInvoiceTiketD = noInvoiceTiket4;


                    currentInvoice.TglInvoiceTiketA = String.IsNullOrEmpty(tglInvoiceTiket1) ? dateT : DateTime.Parse(tglInvoiceTiket1);
                    currentInvoice.TglInvoiceTiketB = String.IsNullOrEmpty(tglInvoiceTiket2) ? dateT : DateTime.Parse(tglInvoiceTiket2);
                    currentInvoice.TglInvoiceTiketC = String.IsNullOrEmpty(tglInvoiceTiket3) ? dateT : DateTime.Parse(tglInvoiceTiket3);
                    currentInvoice.TglInvoiceTiketD = String.IsNullOrEmpty(tglInvoiceTiket4) ? dateT : DateTime.Parse(tglInvoiceTiket4);

                    currentInvoice.HargaTiketA = String.IsNullOrEmpty(hargaTiket1) ? hargaDef : int.Parse(hargaTiket1);
                    currentInvoice.HargaTiketB = String.IsNullOrEmpty(hargaTiket2) ? hargaDef : int.Parse(hargaTiket2);
                    currentInvoice.HargaTiketC = String.IsNullOrEmpty(hargaTiket3) ? hargaDef : int.Parse(hargaTiket3);
                    currentInvoice.HargaTiketD = String.IsNullOrEmpty(hargaTiket4) ? hargaDef : int.Parse(hargaTiket4);

                    currentInvoice.TglTerimaInvoiceA = String.IsNullOrEmpty(tglTerimaInvoice1.Trim()) ? dateT : DateTime.Parse(tglTerimaInvoice1);
                    currentInvoice.TglTerimaInvoiceB = String.IsNullOrEmpty(tglTerimaInvoice2.Trim()) ? dateT : DateTime.Parse(tglTerimaInvoice2);
                    currentInvoice.TglTerimaInvoiceC = String.IsNullOrEmpty(tglTerimaInvoice3.Trim()) ? dateT : DateTime.Parse(tglTerimaInvoice3);
                    currentInvoice.TglTerimaInvoiceD = String.IsNullOrEmpty(tglTerimaInvoice4.Trim()) ? dateT : DateTime.Parse(tglTerimaInvoice4);

                    data.SubmitChanges();
                    
                }
                else
                {
                    InvoiceSPD newInvoice = new InvoiceSPD();

                    newInvoice.NoSPD = noSPD;
                    newInvoice.TanggalIssuedA = String.IsNullOrEmpty(tglIssued1) ? dateT : DateTime.Parse(tglIssued1);
                    newInvoice.TanggalIssuedB = String.IsNullOrEmpty(tglIssued2) ? dateT : DateTime.Parse(tglIssued2);
                    newInvoice.TanggalIssuedC = String.IsNullOrEmpty(tglIssued3) ? dateT : DateTime.Parse(tglIssued3);
                    newInvoice.TanggalIssuedD = String.IsNullOrEmpty(tglIssued4) ? dateT : DateTime.Parse(tglIssued4);

                    newInvoice.NoInvoiceHotelA = noInvoiceHotel1;
                    newInvoice.NoInvoiceHotelB = noInvoiceHotel2;
                    newInvoice.NoInvoiceHotelC = noInvoiceHotel3;
                    newInvoice.NoInvoiceHotelD = noInvoiceHotel4;

                    newInvoice.NoInvoiceTiketA = noInvoiceTiket1;
                    newInvoice.NoInvoiceTiketB = noInvoiceTiket2;
                    newInvoice.NoInvoiceTiketC = noInvoiceTiket3;
                    newInvoice.NoInvoiceTiketD = noInvoiceTiket4;

                    newInvoice.TglInvoiceHotelA = String.IsNullOrEmpty(tglInvoiceHotel1) ? dateT : DateTime.Parse(tglInvoiceHotel1);
                    newInvoice.TglInvoiceHotelB = String.IsNullOrEmpty(tglInvoiceHotel2) ? dateT : DateTime.Parse(tglInvoiceHotel2);
                    newInvoice.TglInvoiceHotelC = String.IsNullOrEmpty(tglInvoiceHotel3) ? dateT : DateTime.Parse(tglInvoiceHotel3);
                    newInvoice.TglInvoiceHotelD = String.IsNullOrEmpty(tglInvoiceHotel4) ? dateT : DateTime.Parse(tglInvoiceHotel4);

                    newInvoice.TglInvoiceTiketA = String.IsNullOrEmpty(tglInvoiceTiket1) ? dateT : DateTime.Parse(tglInvoiceTiket1);
                    newInvoice.TglInvoiceTiketB = String.IsNullOrEmpty(tglInvoiceTiket2) ? dateT : DateTime.Parse(tglInvoiceTiket2);
                    newInvoice.TglInvoiceTiketC = String.IsNullOrEmpty(tglInvoiceTiket3) ? dateT : DateTime.Parse(tglInvoiceTiket3);
                    newInvoice.TglInvoiceTiketD = String.IsNullOrEmpty(tglInvoiceTiket3) ? dateT : DateTime.Parse(tglInvoiceTiket4);

                    newInvoice.HargaHotelA = String.IsNullOrEmpty(hargaHotel1) ? hargaDef : int.Parse(hargaHotel1);
                    newInvoice.HargaHotelB = String.IsNullOrEmpty(hargaHotel2) ? hargaDef : int.Parse(hargaHotel2);
                    newInvoice.HargaHotelC = String.IsNullOrEmpty(hargaHotel3) ? hargaDef : int.Parse(hargaHotel3);
                    newInvoice.HargaHotelD = String.IsNullOrEmpty(hargaHotel4) ? hargaDef : int.Parse(hargaHotel4);

                    newInvoice.HargaTiketA = String.IsNullOrEmpty(hargaTiket1) ? hargaDef : int.Parse(hargaTiket1);
                    newInvoice.HargaTiketB = String.IsNullOrEmpty(hargaTiket2) ? hargaDef : int.Parse(hargaTiket2);
                    newInvoice.HargaTiketC = String.IsNullOrEmpty(hargaTiket3) ? hargaDef : int.Parse(hargaTiket3);
                    newInvoice.HargaTiketD = String.IsNullOrEmpty(hargaTiket4) ? hargaDef : int.Parse(hargaTiket4);

                    newInvoice.TglTerimaInvoiceA = String.IsNullOrEmpty(tglTerimaInvoice1.Trim()) ? dateT : DateTime.Parse(tglTerimaInvoice1);
                    newInvoice.TglTerimaInvoiceB = String.IsNullOrEmpty(tglTerimaInvoice2.Trim()) ? dateT : DateTime.Parse(tglTerimaInvoice2);
                    newInvoice.TglTerimaInvoiceC = String.IsNullOrEmpty(tglTerimaInvoice3.Trim()) ? dateT : DateTime.Parse(tglTerimaInvoice3);
                    newInvoice.TglTerimaInvoiceD = String.IsNullOrEmpty(tglTerimaInvoice4.Trim()) ? dateT : DateTime.Parse(tglTerimaInvoice4);
                    
                    data.InvoiceSPDs.InsertOnSubmit(newInvoice);
                    data.SubmitChanges();
                }
            }
            gvInvoice.EditIndex = -1;
            BindGridView(gvInvoice.PageIndex + 1);
        }

        public void BindGridView(int pageNo)
        {
            int pageSize = 10;
            gvInvoice.PageSize = pageSize;
            string noSPD = txtNomorSPDSearch.Text;
            SqlConnection conn = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            DataSet dataSet = new DataSet();
            String defaultValue = null;
            conn.ConnectionString = ConfigurationManager.ConnectionStrings["SPDConnectionString1"].ConnectionString;

            cmd.Connection = conn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_GetInvoiceByCustomPaging";

            cmd.Parameters.AddWithValue("@PageNo", pageNo);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);
            cmd.Parameters.AddWithValue("@NoSPD", String.IsNullOrEmpty(noSPD) ? defaultValue : noSPD);

            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
            sqlDataAdapter.SelectCommand = cmd;

            try
            {
                sqlDataAdapter.Fill(dataSet);
                gvInvoice.VirtualItemCount = Convert.ToInt32(dataSet.Tables[0].Rows[0]["Total"]);
                gvInvoice.DataSource = dataSet.Tables[1];
                gvInvoice.DataBind();
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
            }
            finally
            {

            }
        }

        
        protected void gvInvoice_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvInvoice.PageIndex = e.NewPageIndex;
            BindGridView(e.NewPageIndex + 1);
        }

        protected void gvInvoice_RowDataBound(object sender, GridViewRowEventArgs e)
        {

        }

        protected void gvInvoice_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Select")
            {
                LinkButton CommandSource = e.CommandSource as LinkButton;
                string URL = "~/newFormRequestDetail.aspx?encrypt=" + Encrypto.Encrypt(CommandSource.Text);
                URL = Page.ResolveClientUrl(URL);
                ScriptManager.RegisterStartupScript(this, GetType(), "openDetail", "openDetail('" + URL + "');", true);
            }
        }

        public IEnumerable<dynamic> getListEqualNoInvoice(IEnumerable<dynamic> listInput, String noInvoice)
        {
            
            var result = listInput.Where(x =>
                            x.NoInvoiceHotelA.Equals(noInvoice) ||
                            x.NoInvoiceHotelB.Equals(noInvoice) ||
                            x.NoInvoiceHotelC.Equals(noInvoice) ||
                            x.NoInvoiceHotelD.Equals(noInvoice) ||

                            x.NoInvoiceTiketA.Equals(noInvoice) ||
                            x.NoInvoiceTiketB.Equals(noInvoice) ||
                            x.NoInvoiceTiketC.Equals(noInvoice) ||
                            x.NoInvoiceTiketD.Equals(noInvoice)
                            ).ToList();
            return result;

        }

        public bool isDuplicateInvoice(string value, GridViewRow row, string textbox1, string textbox2,
                    string textbox3, string textbox4, string textbox5, string textbox6, string textbox7)
        {
            bool result = false;
            if (value == (row.FindControl(textbox1) as TextBox).Text
                    || value == (row.FindControl(textbox2) as TextBox).Text
                    || value == (row.FindControl(textbox3) as TextBox).Text
                    || value == (row.FindControl(textbox4) as TextBox).Text
                    || value == (row.FindControl(textbox5) as TextBox).Text
                    || value == (row.FindControl(textbox6) as TextBox).Text
                    || value == (row.FindControl(textbox7) as TextBox).Text)
            {
                result = true;
            }
            return result;
        }

        protected void gvInvoice_RowCancelingEdit(object sender, EventArgs e)
        {
            gvInvoice.EditIndex = -1;
            txtNomorSPDSearch.Text = string.Empty;
            gvInvoice.Columns[1].Visible = true;
            BindGridView(gvInvoice.PageIndex + 1);

        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            BindGridView(1);
        }
        
        protected void TextBox29_TextChanged(object sender, EventArgs e)
        {
            GridViewRow row = gvInvoice.Rows[((GridViewRow)((TextBox)sender).NamingContainer).RowIndex];
            string tglIssued1 = (row.FindControl("TextBox1") as TextBox).Text;
            string tglTerimaInvoice1 = (row.FindControl("TextBox29") as TextBox).Text;

            if (!String.IsNullOrEmpty(tglIssued1) && !String.IsNullOrEmpty(tglTerimaInvoice1))
            {
                if (!isValidDate(tglIssued1, tglTerimaInvoice1))
                {
                    labelA.Text = errMessage;
                    popupMessage.Show();
                }
            }
        }

        protected void TextBox30_TextChanged(object sender, EventArgs e)
        {
            GridViewRow row = gvInvoice.Rows[((GridViewRow)((TextBox)sender).NamingContainer).RowIndex];
            string tglIssued2 = (row.FindControl("TextBox2") as TextBox).Text;
            string tglTerimaInvoice2 = (row.FindControl("TextBox30") as TextBox).Text;

            if (!String.IsNullOrEmpty(tglIssued2) && !String.IsNullOrEmpty(tglTerimaInvoice2))
            {
                if (!isValidDate(tglIssued2, tglTerimaInvoice2))
                {
                    labelA.Text = errMessage;
                    popupMessage.Show();
                }
            }
        }

        protected void TextBox31_TextChanged(object sender, EventArgs e)
        {
            GridViewRow row = gvInvoice.Rows[((GridViewRow)((TextBox)sender).NamingContainer).RowIndex];
            string tglIssued3 = (row.FindControl("TextBox3") as TextBox).Text;
            string tglTerimaInvoice3 = (row.FindControl("TextBox31") as TextBox).Text;

            if (!String.IsNullOrEmpty(tglIssued3) && !String.IsNullOrEmpty(tglTerimaInvoice3))
            {
                if (!isValidDate(tglIssued3, tglTerimaInvoice3))
                {
                    labelA.Text = errMessage;
                    popupMessage.Show();
                }
            }
        }

        protected void TextBox32_TextChanged(object sender, EventArgs e)
        {
            GridViewRow row = gvInvoice.Rows[((GridViewRow)((TextBox)sender).NamingContainer).RowIndex];
            string tglIssued4 = (row.FindControl("TextBox4") as TextBox).Text;
            string tglTerimaInvoice4 = (row.FindControl("TextBox32") as TextBox).Text;
            if (!String.IsNullOrEmpty(tglIssued4) && !String.IsNullOrEmpty(tglTerimaInvoice4))
            {
                if (!isValidDate(tglIssued4, tglTerimaInvoice4))
                {
                    labelA.Text = errMessage;
                    popupMessage.Show();
                }
            }
        }
    }
}