using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace eSPD.Core
{
    public class SQLHelper
    {
        public DataTable getHardship(int idTunjanganKejauhan)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(ConfigurationManager.AppSettings["SPDConnectionString1"]))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("Select * from Hardship where Id = " + idTunjanganKejauhan + "", con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                     
                    }
                }
            }
            return dt;
        }

        public DataTable getBiayaTransportasi(int idBiayaTransportasi)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(ConfigurationManager.AppSettings["SPDConnectionString1"]))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("Select * from msBiayaTransportasi where id = " + idBiayaTransportasi + "", con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            return dt;
        }

        public DataTable LoadListADH(string CostCenter)
        {
            DataTable dt = new DataTable();
            string Query  = "select distinct msADH.nrp, msKaryawan.namaLengkap from msADH inner join msCost on msADH.costcenterId = msCost.costId inner join msKaryawan on msADH.nrp = msKaryawan.nrp where RowStatus = 1 and msCost.costDesc='" + CostCenter + "'";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.AppSettings["SPDConnectionString1"]))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(Query, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            return dt;
        }

        public string getNRPADH(int costCenterID)
        {
            string NRP = string.Empty;
            using (SqlConnection con = new SqlConnection(ConfigurationManager.AppSettings["SPDConnectionString1"]))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("Select msADH.nrp from msADH where msADH.costcenterId = " + costCenterID + "", con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            NRP = reader["nrp"].ToString();
                        }
                    }
                }
            }
            return NRP;
        }

        public DataTable getPersonalArea(String ddlPersonalAreaTujuan)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(ConfigurationManager.AppSettings["SPDConnectionString1"]))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("Select PersonalAreaCode, Code, Name from view_personnel_sub_area where PersonalAreaCode = '" + ddlPersonalAreaTujuan + "'", con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            return dt;
        }

        public DataTable getListHardship(string Golongan, int propinsiId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(ConfigurationManager.AppSettings["SPDConnectionString1"]))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("Select Id, Lokasi from Hardship where Lokasi='Lain-Lain' OR (Golongan = '" + Golongan + "' and RowStatus = 1 and PropinsiId = " + propinsiId + ")", con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            return dt;
        }

        public DataTable getMsPropinsi()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(ConfigurationManager.AppSettings["SPDConnectionString1"]))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("Select Id, Propinsi from msPropinsi where RowStatus = 1", con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            return dt;
        }

        public DataTable getMsKota()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(ConfigurationManager.AppSettings["SPDConnectionString1"]))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("Select ID, NamaKota from msKota where RowStatus = 1 order by NamaKota", con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            return dt;
        }

        public DataTable getMsKotaByNamaKota(String NamaKota)
        {
            DataTable dt = new DataTable();
            String Query = "select NamaKota, PropinsiID from msKota where NamaKota='" + NamaKota + "' and RowStatus = 1";
            using (SqlConnection con = new SqlConnection(ConfigurationManager.AppSettings["SPDConnectionString1"]))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(Query, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            return dt;
        }

        public String getHarshipValue(int idLokasiPropinsi)
        {
            String result = String.Empty;
            using (SqlConnection con = new SqlConnection(ConfigurationManager.AppSettings["SPDConnectionString1"]))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand("Select Harga from Hardship where Id = " + idLokasiPropinsi + "", con))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result = reader["Harga"].ToString();
                        }
                    }
                }
            }
            return result;
        }

        public int getIdBiayaTransportasi(string tipePesawat, int idKotaAsal, int idKotaTujuan)
        {
            int idBiayaTransportasi = 0;
            String Query = "select TOP 1 ID, TipePesawat from msBiayaTransportasi where(TipePesawat = '" + tipePesawat + "' and IDKotaAsal = " + idKotaAsal + " and IDKotaTujuan = " + idKotaTujuan + ")OR(TipePesawat = '" + tipePesawat + "' and IDKotaAsal = " + idKotaTujuan + " and IDKotaTujuan = " + idKotaAsal + ") and RowStatus = 1";
            using (SqlConnection con = new SqlConnection(ConfigurationManager.AppSettings["SPDConnectionString1"]))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(Query, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            idBiayaTransportasi = int.Parse(reader["ID"].ToString());
                        }
                        return idBiayaTransportasi;
                    }
                }
            }
        }

    }
}