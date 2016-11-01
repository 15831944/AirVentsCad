using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace HostingWindowsForms.Data
{
    public class SqlQuery
    {

        //string conString = @"Data Source = srvkb; Initial Catalog = SWPlusDB; Persist Security Info=True;User ID = sa; Password=PDMadmin; pooling=True ";
        //string conVentsPdm = @"Data Source = srvkb; Initial Catalog = Vents-Pdm; Persist Security Info=True;User ID = sa; Password=PDMadmin; pooling=True";

        public DataTable GetDocumentAndFolderId(string fileName)
        {
            var q = @"SELECT d.DocumentID, d.Filename, dip.ProjectID, dip.Deleted FROM Documents d
                    JOIN DocumentsInProjects dip ON dip.DocumentID = d.DocumentID WHERE d.Filename = '" + fileName + @"' AND dip.Deleted = 0";
            using (var sqlConn = new SqlConnection(ConnectionString.ConVentsPdm))
            using (var cmd = new SqlCommand(q, sqlConn))
            {
                sqlConn.Open();
                var dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                return dt;
            }
        }

        public class TaskParam
        {
            public int FileId { get; set; }
            public int ParentolderId { get; set; }
            public int CurrentVersion { get; set; }
            public int TaskId { get; set; }
        }

        public void AddTaskList(List<TaskParam> taskParam)
        {
            using (var con = new SqlConnection(ConnectionString.ConString))
            {
                using (var cmd = new SqlCommand("Epdm.AddTask", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();

                    foreach (var param in taskParam)
                    {                        
                        cmd.Parameters.Add("@DocumentId", SqlDbType.Int).Value = param.FileId;
                        cmd.Parameters.Add("@ProjectId", SqlDbType.Int).Value = param.ParentolderId;
                        cmd.Parameters.Add("@Version", SqlDbType.Int).Value = param.CurrentVersion;
                        cmd.Parameters.Add("@TaskType", SqlDbType.Int).Value = param.TaskId;
                        cmd.ExecuteNonQuery();
                    }
                   
                    con.Close();
                }
            }
        }
    }
}