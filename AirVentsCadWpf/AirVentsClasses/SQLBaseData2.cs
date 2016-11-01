using System;
using System.Data;
using System.Data.SqlClient;

namespace AirVentsCadWpf.AirVentsClasses
{
    /// <summary>
    /// Класс реализующий работу с базой данных
    /// </summary>
    public partial class SqlBaseData
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sizeId"></param>
        /// <param name="profilId"></param>
        /// <returns></returns>
        public string[] StandartSize2(int sizeId, int profilId)
        {
            string width = null;
            string height = null;
            using (var con = new SqlConnection(Properties.Settings.Default.ConnectionToSQL))
            {
                try
                {
                    con.Open();
                    var sqlCommand =
                        new SqlCommand(@"SELECT
AirVents.StandardSize.Type,
Profil.Description,
AirVents.Dimension.Wight,
AirVents.Dimension.Hight
FROM AirVents.DimensionType
INNER JOIN AirVents.Dimension
  ON AirVents.DimensionType.DimensionID = AirVents.Dimension.DimensionID
INNER JOIN dbo.[AirVents.StandardSize
  ON AirVents.DimensionType.SizeID = AirVents.StandardSize.SizeID
INNER JOIN dbo.Profil
  ON AirVents.DimensionType.ProfilID = Profil.ProfilID
  WHERE AirVents.StandardSize.SizeID = " + sizeId + " AND  Profil.ProfilID = " + profilId, con);
                    var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                    var dataTable = new DataTable("StandartSize");
                    sqlDataAdapter.Fill(dataTable);
                    width = dataTable.Rows[0]["Wight"].ToString();
                    height = dataTable.Rows[0]["Hight"].ToString();
                }
                catch (Exception)
                {
                    con.Close();
                }
                finally
                {
                    con.Close();
                }
            }

            return new[] { width, height };
        }
       
    }

}