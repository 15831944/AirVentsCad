using System.Data;
using System.Data.SqlClient;

namespace AirVentsCadWpf.AirVentsClasses
{
    class GenerateUnitNames
    {
        public string Connect = "Data Source=srvkb;Initial Catalog=SWPlusDB;Persist Security Info=True;User ID=sa;Password=PDMadmin;MultipleActiveResultSets=True";
        
        //<~~~ Тип блока
        public void LoadBlockType(out string valueMember, out string displayMember, out object tables)
        {

            var con = new SqlConnection(Connect);
            var cmd = new SqlCommand {Connection = con};

            con.Open();

            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select BlockTypeID, BlockTypeName from AirVents.BlockType order by BlockTypeCode";

            var objDs = new DataSet();
            var dAdapter = new SqlDataAdapter {SelectCommand = cmd};
            dAdapter.Fill(objDs, "BlockType");

            valueMember = "BlockTypeID";
            displayMember = "BlockTypeName";
            tables = objDs.Tables[0].DefaultView;

            con.Close();

        }



        //<~~~ Типоряд
        public void LoadTypeLine(out string valueMember, out string displayMember, out object tables)
        {
            
            var con = new SqlConnection(Connect);
            var cmd = new SqlCommand {Connection = con};

            con.Open();

            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select SizeID, Type from AirVents.StandardSize ORDER BY Type";

            var objDs = new DataSet();
            var dAdapter = new SqlDataAdapter {SelectCommand = cmd};
            dAdapter.Fill(objDs, "StandardSize");

            valueMember = "SizeID";
            displayMember = "Type";
            tables = objDs.Tables[0].DefaultView;

            con.Close();
        }

    }
}
