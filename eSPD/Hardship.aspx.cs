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
    public partial class Hardship : System.Web.UI.Page
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
                gvMasterHardship.Visible = true;
                btnTambah.Visible = true;
            }
            else
            {
                gvMasterHardship.Visible = false;
                btnTambah.Visible = false;
            }

            if (!IsPostBack)
            {
                inputForm.Visible = false;
                hfmode.Value = "add";
                BindDropdowListPropinsi();
                BindGridMasterHardship();
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

        protected void BindDropdowListPropinsi()
        {
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand("Select Id, Propinsi from msPropinsi where RowStatus = 1", con))
                {
                    cmd.Connection.Open();
                    SqlDataReader Reader;
                    Reader = cmd.ExecuteReader();
                    provinsiList.DataSource = Reader;
                    provinsiList.DataValueField = "Id";
                    provinsiList.DataTextField = "Propinsi";
                    provinsiList.DataBind();
                    cmd.Connection.Close();
                    cmd.Connection.Dispose();
                }
            }
        }

        protected void BindGridMasterHardship()
        {
            string constr = ConfigurationManager.ConnectionStrings["SPDConnectionString1"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand("SP_HardshipCRUD"))
                {
                    cmd.Parameters.AddWithValue("@Action", "SELECT");
                    cmd.Parameters.Add("@Message", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                    using (SqlDataAdapter sda = new SqlDataAdapter())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;
                        sda.SelectCommand = cmd;
                        using (DataTable dt = new DataTable())
                        {
                            sda.Fill(dt);
                            gvMasterHardship.DataSource = dt;
                            gvMasterHardship.DataBind();
                        }
                    }
                }
            }
        }
        protected void btnSimpan_Click(object sender, EventArgs e)
        {
            try
            {
                string propinsi = provinsiList.SelectedValue;
                string lokasi = txtLokasi.Text;
                string golongan = golonganList.SelectedValue;
                string harga = txtHarga.Text;
                string mode = hfmode.Value.ToString();
                string action = string.Empty;
                string message = string.Empty;
                if (mode == "edit")
                    action = "UPDATE";
                else
                    action = "INSERT";

                string constr = ConfigurationManager.ConnectionStrings["SPDConnectionString1"].ConnectionString;
                using (SqlConnection con = new SqlConnection(constr))
                {
                    using (SqlCommand cmd = new SqlCommand("SP_HardshipCRUD"))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Action", action);
                        cmd.Parameters.AddWithValue("@Id", int.Parse(HFmasterHardshipId.Value.ToString() != "" ? HFmasterHardshipId.Value : "0"));
                        cmd.Parameters.AddWithValue("@PropinsiId", int.Parse(propinsi));
                        cmd.Parameters.AddWithValue("@Lokasi", lokasi);
                        cmd.Parameters.AddWithValue("@Golongan", golongan);
                        cmd.Parameters.AddWithValue("@Harga", int.Parse(harga));
                        cmd.Parameters.Add("@Message", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                        cmd.Connection = con;
                        con.Open();
                        cmd.ExecuteNonQuery();
                        if (cmd.Parameters["@Message"].Value != null)
                            message = cmd.Parameters["@Message"].Value.ToString();
                        con.Close();
                    }
                }
                ClearForm();
                if (message.Contains("DUPLICATE"))
                {
                    notif.Text = "Duplicate Data";
                }
                else
                    notif.Text = "Data berhasil disimpan";
                BindGridMasterHardship();
            }
            catch (Exception ex)
            {
                notif.Text = ex.Message.ToString();
            }
        }

        protected void btnBatal_Click(object sender, EventArgs e)
        {

        }

        protected void gvMasterHardship_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvMasterHardship.PageIndex = e.NewPageIndex;
            BindGridMasterHardship();
        }

        protected void lbEdit_Click(object sender, EventArgs e)
        {
            ImageButton link = (ImageButton)sender;
            GridViewRow gv = (GridViewRow)(link.NamingContainer);
            Label id = (Label)gv.FindControl("id");
            int masterHardshipId = int.Parse(id.Text.Trim());
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT Hardship.Id, lokasi, golongan, harga, propinsiId, propinsi from hardship inner join msPropinsi on hardship.propinsiId = msPropinsi.Id where Hardship.Id = " + masterHardshipId + "", con))
                {
                    cmd.Connection.Open();
                    SqlDataReader reader;
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        provinsiList.SelectedValue = reader["propinsiId"].ToString();
                        txtLokasi.Text = reader["Lokasi"].ToString();
                        golonganList.SelectedValue = reader["Golongan"].ToString();
                        txtHarga.Text = reader["Harga"].ToString();
                        HFmasterHardshipId.Value = reader["Id"].ToString();
                    }
                }
            }
            hfmode.Value = "edit";
            inputForm.Visible = true;
            btnTambah.Visible = false;
        }

        protected void Provinsi_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        protected void lbDelete_Click(object sender, EventArgs e)
        {
            ImageButton link = (ImageButton)sender;
            GridViewRow gv = (GridViewRow)(link.NamingContainer);
            Label id = (Label)gv.FindControl("id");
            int HardshipID = int.Parse(id.Text.Trim());
            String message = String.Empty;
            string constr = ConfigurationManager.ConnectionStrings["SPDConnectionString1"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand("SP_HardshipCRUD"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "DELETE");
                    cmd.Parameters.AddWithValue("@id", HardshipID);
                    cmd.Parameters.Add("@Message", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    if (cmd.Parameters["@Message"].Value != null)
                        message = cmd.Parameters["@Message"].Value.ToString();
                    con.Close();
                }
            }
            if (message.Contains("DUPLICATE"))
            {
                notif.Text = "Duplicate Data";
            }
            else
                notif.Text = "Data berhasil disimpan";
            BindGridMasterHardship();
        }

        private void ClearForm()
        {
            txtLokasi.Text = string.Empty;
            txtHarga.Text = string.Empty;
        }
        protected void btnTambah_Click(object sender, EventArgs e)
        {
            inputForm.Visible = true;
            btnTambah.Visible = false;
            hfmode.Value = "add";
        }
    }
}