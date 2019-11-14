using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace eSPD.Core
{
    public class ESPDInformation
    {
        public string NoSPD { set; get; }
        public string Fiscal_Year { set; get; }
        public string CostCenterDesc { set; get; }
        public string CostCenterCode { set; get; }
        public string UangMakan { set; get; }
        public string UangSaku { set; get; }
        public string TunjanganKejauhan { set; get; }
        public string Transportasi { set; get; }
        public string Akomodasi { set; get; }
        public string CostUangMakan { set; get; }
        public string CostUangSaku { set; get; }
        public string CostTransportasi { set; get; }
        public string CostAkomodasi { set; get; }
        public string SisaBudget { set; get; }
        public string Message { set; get; }
        public string SisaUangMakan { set; get; }
        public string SisaUangSaku { set; get; }
        public string SisaTransportasi { set; get; }
        public string SisaAkomodasi { set; get; }


        public static ESPDInformation CheckBudgetToSAP(trSPD spd)
        {
            ESPDInformation Row = new ESPDInformation();
            Row.Fiscal_Year = DateTime.Now.Year.ToString();
            Row.CostCenterCode = spd.costCenter.ToString().Split('-')[0].ToString(); 
            Row.CostCenterDesc = spd.costCenter;
            Row.NoSPD = spd.noSPD;
            try
            {
                string Uri = ConfigurationManager.AppSettings["apiURL"].ToString() + Row.Fiscal_Year + "/" + Row.CostCenterCode + "/0/0/0/0/0";
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(Uri);
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.Method = "GET";
                HttpWebResponse Respon = httpWebRequest.GetResponse() as HttpWebResponse;
                using (var streamReader = new StreamReader(Respon.GetResponseStream()))
                {
                    string Response = streamReader.ReadToEnd();
                    if (Response.Contains("berhasil terkirim"))
                    {
                        var result = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(Response);
                        Row.CostTransportasi = result["response1"]["TRANSPORT"];
                        Row.CostAkomodasi = result["response1"]["AKOMODASI"];
                        Row.CostUangMakan = result["response1"]["UANG_MAKAN"];
                        Row.CostUangSaku = result["response1"]["UANG_SAKU"];
                        Row.Message = "Success";
                    }
                    else
                    {
                        Row.Message = "Informasi budget belum tersedia ";
                    }
                }
                /*
                string Uri = ConfigurationManager.AppSettings["APIUrl"].ToString();
                httpWebRequest.Method = "POST";
                string username = ConfigurationManager.AppSettings["SAPUserName"].ToString();
                string password = ConfigurationManager.AppSettings["SAPPassword"].ToString(); ;
                var bytes = Encoding.UTF8.GetBytes($"{username}:{password}");
                httpWebRequest.Headers.Add("Authorization", $"Basic {Convert.ToBase64String(bytes)}");
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    Request = "{\"ROW\": {\"FISCAL_YEAR\": \"" + Row.Fiscal_Year + "\",\"FUNDS_CTR\": \"" + Row.CostCenterCode + "\",\"NO_ESPD\": \"" + Row.NoSPD + "\",\"TRANSPORT\": \"" + "0" + "\",\"AKOMODASI\": \"" + "0" + "\",\"UANG_MAKAN\": \"" + "0" + "\",\"UANG_SAKU\": \"" + "0" + "\"}}";
                    streamWriter.Write(Request);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                */

            }
            catch (Exception ex)
            {
                Row.Message = ex.Message + " " +  ex.InnerException;
                return Row;
            }
            finally {
                SaveSPDBudgetToDB(Row);
            }
            return Row;
        }

        private static void SaveSPDBudgetToDB(ESPDInformation Row)
        {
            int? defaultValue = null;
            using (var ctx = new dsSPDDataContext())
            {
                BudgetSPD newBudget = new BudgetSPD();
                newBudget.NoSPD = Row.NoSPD;
                newBudget.CostCenter = Row.CostCenterDesc;
                newBudget.UangMakan = string.IsNullOrEmpty(Row.CostUangMakan) ? defaultValue : int.Parse(Row.CostUangMakan, NumberStyles.Currency); 
                newBudget.UangSaku = string.IsNullOrEmpty(Row.CostUangSaku) ? defaultValue : int.Parse(Row.CostUangSaku, NumberStyles.Currency); 
                newBudget.Transportasi = string.IsNullOrEmpty(Row.CostTransportasi) ? defaultValue : int.Parse(Row.CostTransportasi, NumberStyles.Currency);
                newBudget.Akomodasi = string.IsNullOrEmpty(Row.CostAkomodasi) ? defaultValue : int.Parse(Row.CostAkomodasi, NumberStyles.Currency); 
                newBudget.keterangan = Row.Message;
                newBudget.RowStatus = 1;
                newBudget.CreatedOn = DateTime.Now;
                ctx.BudgetSPDs.InsertOnSubmit(newBudget);
                ctx.SubmitChanges();
            }
        }
        public static DataTable getBudgetFromDB(DataTable listCostCenter)
        {
            DataTable result = new DataTable();
            string constr = ConfigurationManager.ConnectionStrings["SPDConnectionString1"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand("SP_INSERTBUDGETSPD"))
                {
                    cmd.Parameters.AddWithValue("@Action", "SELECT");
                    cmd.Parameters.AddWithValue("@ListCostCenter", listCostCenter);
                    using (SqlDataAdapter sda = new SqlDataAdapter())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;
                        sda.SelectCommand = cmd;
                        sda.Fill(result);
                    }
                }
            }
            return result;
        }

        public static void PerhitunganBiayaSPDByBudget(ESPDInformation spdDetail, DataTable dtResult, DataTable listSPD)
        {

            string constr = ConfigurationManager.ConnectionStrings["SPDConnectionString1"].ConnectionString;
            CultureInfo culture = new CultureInfo("id-ID");
            string CostCenterDesc = string.Empty;
            int UangMakan = 0, UangSaku = 0, Akomodasi = 0, Transportasi = 0;
            

            CostCenterDesc = spdDetail.CostCenterDesc;
            UangMakan = string.IsNullOrEmpty(spdDetail.CostUangMakan) ? 0 : int.Parse(spdDetail.CostUangMakan, NumberStyles.Currency);
            UangSaku = string.IsNullOrEmpty(spdDetail.CostUangSaku) ? 0 : int.Parse(spdDetail.CostUangSaku, NumberStyles.Currency);
            Akomodasi = string.IsNullOrEmpty(spdDetail.CostAkomodasi) ? 0 : int.Parse(spdDetail.CostAkomodasi, NumberStyles.Currency);
            Transportasi = string.IsNullOrEmpty(spdDetail.CostTransportasi) ? 0 : int.Parse(spdDetail.CostTransportasi, NumberStyles.Currency);

            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand("SP_GetBiayaSPD"))
                {
                    cmd.Parameters.AddWithValue("@CostCenter", CostCenterDesc);
                    cmd.Parameters.AddWithValue("@UangMakan", UangMakan);
                    cmd.Parameters.AddWithValue("@UangSaku", UangSaku);
                    cmd.Parameters.AddWithValue("@Transportasi", Transportasi);
                    cmd.Parameters.AddWithValue("@Akomodasi", Akomodasi);
                    cmd.Parameters.AddWithValue("@ListNomorSPD", listSPD);
                    using (SqlDataAdapter sda = new SqlDataAdapter())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;
                        sda.SelectCommand = cmd;
                        sda.Fill(dtResult);
                    }
                }
            }
        }
        public void SetBiayaSPD(ref ESPDInformation spdDetail, DataTable dtResult)
        {
            CultureInfo culture = new CultureInfo("id-ID");
            spdDetail.UangMakan = string.IsNullOrEmpty(dtResult.Rows[0]["UangMakan"].ToString()) ? "":int.Parse(dtResult.Rows[0]["UangMakan"].ToString()).ToString("N2", culture);
            spdDetail.UangSaku = String.IsNullOrEmpty(dtResult.Rows[0]["UangSaku"].ToString()) ? "" : int.Parse(dtResult.Rows[0]["UangSaku"].ToString()).ToString("N2", culture);
            spdDetail.Transportasi = String.IsNullOrEmpty(dtResult.Rows[0]["BiayaTransportasi"].ToString()) ? "":  int.Parse(dtResult.Rows[0]["BiayaTransportasi"].ToString()).ToString("N2", culture);
            spdDetail.Akomodasi = string.IsNullOrEmpty(dtResult.Rows[0]["Akomodasi"].ToString()) ? "": int.Parse(dtResult.Rows[0]["Akomodasi"].ToString()).ToString("N2", culture);
            spdDetail.TunjanganKejauhan = string.IsNullOrEmpty(dtResult.Rows[0]["TunjanganKejauhan"].ToString()) ?"": int.Parse(dtResult.Rows[0]["TunjanganKejauhan"].ToString()).ToString("N2", culture);
            spdDetail.SisaBudget =string.IsNullOrEmpty(dtResult.Rows[0]["SisaBudget"].ToString()) ? "": int.Parse(dtResult.Rows[0]["SisaBudget"].ToString()).ToString("N2", culture);
            spdDetail.SisaUangMakan = string.IsNullOrEmpty(dtResult.Rows[0]["SisaUangMakan"].ToString()) ? "" : int.Parse(dtResult.Rows[0]["SisaUangMakan"].ToString()).ToString("N2", culture); 
            spdDetail.SisaUangSaku = string.IsNullOrEmpty(dtResult.Rows[0]["SisaUangSaku"].ToString()) ? "" : int.Parse(dtResult.Rows[0]["SisaUangSaku"].ToString()).ToString("N2", culture); 
            spdDetail.SisaTransportasi = string.IsNullOrEmpty(dtResult.Rows[0]["SisaTransportasi"].ToString()) ? "" : int.Parse(dtResult.Rows[0]["SisaTransportasi"].ToString()).ToString("N2", culture);
            spdDetail.SisaAkomodasi = string.IsNullOrEmpty(dtResult.Rows[0]["SisaAkomodasi"].ToString()) ? "" : int.Parse(dtResult.Rows[0]["SisaAkomodasi"].ToString()).ToString("N2", culture);
        }
    }
}