using System;
using System.Data;
using System.Data.SqlClient;

namespace TestSql
{
    class Program
    {
        static void Main(string[] args)
        {
            var table = PanelTypeId();

            foreach (DataRow item in table.Rows)
            {
                PanelTypeId(item["col1"].ToString(), item["IDNomenclature"].ToString());
                //Console.WriteLine(item["IDNomenclature"] + " - - " +item["col1"]);
            }           

            Console.ReadKey();
        }

        public static DataTable PanelTypeId()
        {            
            var dataTable = new DataTable("panelTypeName");
            using (var con = new SqlConnection("Data Source = srvkb; Initial Catalog = SWPlusDB; Persist Security Info = True; User ID = sa; Password = PDMadmin; MultipleActiveResultSets = True"))
            {
                try
                {
                    con.Open();
                    var sqlCommand =
                        new SqlCommand(
                            @"SELECT t.DocumentID,  
n.IDNomenclature,
f.col1
FROM Nomenclature n
INNER JOIN[Table] t ON n.IDPDM = DocumentID
INNER JOIN AirVents.table1 f ON n.Nomenclature = f.col2", con);
                    var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                    sqlDataAdapter.Fill(dataTable);                  
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
            return dataTable;
        }

        public static DataTable PanelTypeId(string col1, string nomenclatureId)
        {
            var dataTable = new DataTable("panelTypeName");
            using (var con = new SqlConnection("Data Source = srvkb; Initial Catalog = SWPlusDB; Persist Security Info = True; User ID = sa; Password = PDMadmin; MultipleActiveResultSets = True"))
            {
                try
                {
                    con.Open();
                    var sqlCommand =
                        new SqlCommand(
                            @"SELECT t.DocumentID,  
n.IDNomenclature,
f.col1
FROM Nomenclature n
INNER JOIN[Table] t ON n.IDPDM = DocumentID
INNER JOIN AirVents.table1 f ON n.Nomenclature = f.col2
update [AirVents].[PanelsPart]
  set NomenclatureID = " + nomenclatureId
  + " WHERE PartID = " + col1, con);
                    var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                    sqlDataAdapter.Fill(dataTable);
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
            return dataTable;
        }
        
        //        public static DataTable PanelTypeId( string col1, string nomenclatureId)
        //        {            
        //            var dataTable = new DataTable("panelTypeName");
        //            using (var con = new SqlConnection("Data Source = srvkb; Initial Catalog = SWPlusDB; Persist Security Info = True; User ID = sa; Password = PDMadmin; MultipleActiveResultSets = True"))
        //            {
        //                try
        //                {
        //                    con.Open();
        //                    var sqlCommand =
        //                        new SqlCommand(
        //                            @"SELECT t.DocumentID,  
        //n.IDNomenclature,
        //f.col1
        //FROM Nomenclature n
        //INNER JOIN[Table] t ON n.IDPDM = DocumentID
        //INNER JOIN AirVents.table1 f ON n.Nomenclature = f.col2
        //update [AirVents].[PanelsPart]
        //  set NomenclatureID = " + nomenclatureId
        //  +  " WHERE PartID = " + col1, con);
        //                    var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
        //                    sqlDataAdapter.Fill(dataTable);
        //                }
        //                catch (Exception)
        //                {
        //                    con.Close();
        //                }
        //                finally
        //                {
        //                    con.Close();
        //                }
        //            }
        //            return dataTable;
        //        }        
        //        public static DataTable PanelTypeId()
        //        {
        //            //iDNomenclature = 0;
        //            //col1 = 0;
        //            var dataTable = new DataTable("panelTypeName");
        //            using (var con = new SqlConnection("Data Source = srvkb; Initial Catalog = SWPlusDB; Persist Security Info = True; User ID = sa; Password = PDMadmin; MultipleActiveResultSets = True"))
        //            {
        //                try
        //                {
        //                    con.Open();
        //                    var sqlCommand =
        //                        new SqlCommand(
        //                            @"SELECT t.DocumentID,  
        //n.IDNomenclature,
        //f.col1
        //FROM Nomenclature n
        //INNER JOIN[Table] t ON n.IDPDM = DocumentID
        //INNER JOIN AirVents.table1 f ON n.Nomenclature = f.col2"
        //                            //@"SELECT *  FROM [AirVents].[PanelType] WHERE[PanelTypeCode]   = '" + panelTypeCode + "'"
        //                            , con);
        //                    var sqlDataAdapter = new SqlDataAdapter(sqlCommand);                    
        //                    sqlDataAdapter.Fill(dataTable);
        //                    //iDNomenclature = Convert.ToInt32(dataTable.Rows[0]["IDNomenclature"]);
        //                    //col1 = Convert.ToInt32(dataTable.Rows[0]["col1"]);
        //                }
        //                catch (Exception)
        //                {
        //                    con.Close();
        //                }
        //                finally
        //                {
        //                    con.Close();
        //                }
        //            }
        //            return dataTable;           
        //        }

    }
}
