using System;
using System.Collections.Generic;
using System.ServiceModel;
using ConecctorOneC;
using HostingWindowsForms.Authorization;
using HostingWindowsForms.Data;
using HostingWindowsForms.EPDM;

namespace HostingWindowsForms
{
    [ServiceContract]
    public interface I1CService
    {
        [OperationContract]
        bool AuthenticationUser(string userName, string password);

        [OperationContract]
        List<Connection.ClassifierMeasure> GetClassifierMeasureList();

        [OperationContract]
        List<Connection.Nomenclature> SearchNomenclatureByName(string name);
    }

    [ServiceContract]
    public interface IEpdmService
    {
        //[WebInvoke(Method = "GET",
        //           ResponseFormat = WebMessageFormat.Json,
        //           UriTemplate = "SearchDoc/{name}")]
        [OperationContract]
        List<Epdm.SearchColumnName> SearchDoc(string name);

        //[WebInvoke(Method = "GET",
        //           ResponseFormat = WebMessageFormat.Json,
        //           //UriTemplate = "BOM?filePath = {filePath}&config = {config}")]
        //           UriTemplate = "BOM/{filePath}/{config}")]
        [OperationContract]
        IList<Epdm.BomCells> Bom(string filePath, string config);

        [OperationContract]
        IEnumerable<string> GetConfiguration(string filePath);

        [OperationContract]
        string GetLink(string fileName);
    }

    [ServiceContract]
    public interface ITaskService
    {
        [OperationContract]
        void AddTaskList(List<SqlQuery.TaskParam> list);
    }

    public class VentsService : I1CService, IEpdmService, ITaskService
    {
        #region Authentication
        public bool AuthenticationUser(string userName, string password)
        {
            var status = new Authentication();
            var statusReturn = status.AuthenticateUser(userName, password);

            return statusReturn;
        }
        #endregion

        #region Epdm

            #region Fields

            const int BomId = 8;

            #endregion

        public IList<Epdm.BomCells> Bom(string filePath, string config)
        {
            return null; // throw new NotImplementedException();
        }

        public IEnumerable<string> GetConfiguration(string filePath)
            {
                var epdmClass = new Epdm();

                var enumString = epdmClass.GetConfiguration(filePath);

                return enumString;
            }

            public List<Epdm.SearchColumnName> SearchDoc(string name)
            {
                var epdmClass = new Epdm();
                var searchList = epdmClass.SearchDoc(name);

                return searchList;
            }

            public IList<Epdm.BomCells> Bom(string filePath, string config, bool asBuild)
            {
                var bomClass = new Epdm
                {
                    BomId = BomId,
                    AssemblyPath = filePath,
                };
                Exception exception;
                return bomClass.BomList(filePath, config, asBuild, out exception);
            }
            
            public string GetLink(string fileName)
            {
                var strLink = new Epdm();

                var str = strLink.GetLink(fileName);

                return str;
            }
            #region Task
            
            public void AddTaskList(List<SqlQuery.TaskParam> list)
            {
                var sql = new SqlQuery();
                sql.AddTaskList(list);
            }

            #endregion
        #endregion

        #region 1C

        public List<Connection.ClassifierMeasure> GetClassifierMeasureList()
        {
            var conOneC = new Connection();

            var getList = conOneC.ClassifierMeasureList();

            return getList;
        }

        public List<Connection.Nomenclature> SearchNomenclatureByName(string name)
        {
            var conOneC = new Connection();

            //conOneC.ConnectionString();

            var getList = conOneC.SearchNomenclatureByName(name);

            return getList;

        }


        #endregion
    }
}