using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EdmLib;
using HostingWindowsForms.Data;

namespace HostingWindowsForms.EPDM
{
    public class Epdm
    {
        public IEdmFile7 EdmFile7;

        #region Поля 

        public int BomId { get; set; }

        public string AssemblyInfoLabel;

        public string AssemblyPath { get; set; }

        public string AsmConfiguration { get; set; }
        
        
        public class BomCells
        {
            public decimal? Количество { get; set; }
            public string ТипФайла { get; set; }
            public string Конфигурация { get; set; }
            public int? ПоследняяВерсия { get; set; }
            public int? Уровень { get; set; }
            public string Состояние { get; set; }
            public string Раздел { get; set; }
            public string Обозначение { get; set; }
            public string Наименование { get; set; }
            public string Материал { get; set; }
            public string МатериалЦми { get; set; }
            public string ТолщинаЛиста { get; set; }
            public int? IdPdm { get; set; }
            public string FileName { get; set; }
            public string FilePath { get; set; }
            public string ErpCode { get; set; }
            public string SummMaterial { get; set; }
            public string Weight { get; set; }
            public string CodeMaterial { get; set; }
            public string Format { get; set; }
            public string Note { get; set; }
            public int? Position { get; set; }
            public List<decimal> КолПоКонфигурациям { get; set; }
            public string КонфГлавнойСборки { get; set; }
            public string ТипОбъекта { get; set; }
            public string GetPathName { get; set; }           

        }

     
        public List<BomCells> BomList(string filePath, string config, bool asBuild , out Exception exception)
        {
            var bomFlag = asBuild ? EdmBomFlag.EdmBf_AsBuilt : EdmBomFlag.EdmBf_ShowSelected;
            Getbom(BomId, filePath, config, EdmBomFlag.EdmBf_ShowSelected, out exception);
            return _bomList;
        }

        #endregion

        public IEnumerable<string> GetConfiguration(string filePath)
        {
            
            IEdmFolder5 oFolder;

            CheckPdmVault();

            var edmFile5 = _edmVault5.GetFileFromPath(filePath, out oFolder);
            var configs = edmFile5.GetConfigurations("");

            var headPosition = configs.GetHeadPosition();

            var configsList = new List<string>();

            while (!headPosition.IsNull)
            {
                var configName = configs.GetNext(headPosition);
                if (configName != "@")
                {
                    configsList.Add(configName);
                }
            }
            return configsList;

        }

        #region Методы получения листа
        public class SearchColumnName
        {
            public string FileName { get; set; }

            public int FileId { get; set; }

            public int FolderId { get; set; }

            public string PartNumber { get; set; }

            public string FilePath { get; set; }
        }


        private EdmVault5 _edmVault5;
        private IEdmVault7 _edmVault7;

        void CheckPdmVault()
        {
            try
            {
                if (_edmVault5 == null)
                {
                    _edmVault5 = new EdmVault5();
                }

                _edmVault7 = _edmVault5;

                var ok = _edmVault5.IsLoggedIn;

                if (!ok)
                {
                    _edmVault5.LoginAuto("Vents-PDM", 0);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public List<SearchColumnName> SearchDoc(string name)
        {
            var namedoc = new List<SearchColumnName>();

            CheckPdmVault();
            
            var search = (IEdmSearch5)_edmVault7.CreateUtility(EdmUtility.EdmUtil_Search);

            search.FindFiles = true;
            search.FindFolders = false;

            search.FileName = "%" + name + "%";

            var result = search.GetFirstResult();
            
            while ((result != null))
            {
                var columnClass = new SearchColumnName()
                {
                    FileName = result.Name,
                    FileId = result.ID,
                    FolderId = result.ParentFolderID,
                    FilePath = result.Path
                };

                namedoc.Add(columnClass);

                result = search.GetNextResult();
            }

            return namedoc;
        }

        void Getbom(int bomId, string filePath,  string bomConfiguration, EdmBomFlag bomFlag, out Exception exception)
        {
            exception = null;

            try
            {
                IEdmFolder5 oFolder;
                CheckPdmVault();
                EdmFile7 = (IEdmFile7) _edmVault5.GetFileFromPath(filePath, out oFolder);

                var bomView = EdmFile7.GetComputedBOM(Convert.ToInt32(bomId), Convert.ToInt32(-1), bomConfiguration, (int) bomFlag); 

                if (bomView == null) return;
                Array bomRows;
                Array bomColumns;
                bomView.GetRows(out bomRows);
                bomView.GetColumns(out bomColumns);

                var bomTable = new DataTable();

                foreach (EdmBomColumn bomColumn in bomColumns)
                {
                    bomTable.Columns.Add(new DataColumn {ColumnName = bomColumn.mbsCaption});
                }

                //bomTable.Columns.Add(new DataColumn { ColumnName = "Путь" });
                bomTable.Columns.Add(new DataColumn {ColumnName = "Уровень"});
                bomTable.Columns.Add(new DataColumn {ColumnName = "КонфГлавнойСборки"});
                bomTable.Columns.Add(new DataColumn { ColumnName = "ТипОбъекта" });
                bomTable.Columns.Add(new DataColumn { ColumnName = "GetPathName" });

                for (var i = 0; i < bomRows.Length; i++)
                {
                    var cell = (IEdmBomCell) bomRows.GetValue(i);

                    bomTable.Rows.Add();

                    for (var j = 0; j < bomColumns.Length; j++)
                    {
                        var column = (EdmBomColumn) bomColumns.GetValue(j);                       

                        object value;
                        object computedValue;
                        string config;
                        bool readOnly;

                        cell.GetVar(column.mlVariableID, column.meType, out value, out computedValue, out config, out readOnly);

                        if (value != null)
                        {
                            bomTable.Rows[i][j] = value;
                        }
                        else
                        {
                            bomTable.Rows[i][j] = null;
                        }
                        bomTable.Rows[i][j + 1] = cell.GetTreeLevel();

                        bomTable.Rows[i][j + 2] = bomConfiguration;
                        bomTable.Rows[i][j + 3] = config;
                        bomTable.Rows[i][j + 4] = cell.GetPathName();
                    }
                }

                _bomList = BomTableToBomList(bomTable);
                exception = null;
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        }

        #endregion

        #region ДанныеДляВыгрузки - Поля с х-ками деталей

        List<BomCells> _bomList = new List<BomCells>();
        
        private static List<BomCells> BomTableToBomList(DataTable table)
        {

            var bomList = new List<BomCells>(table.Rows.Count);

            bomList.AddRange(from DataRow row in table.Rows
                             select row.ItemArray into values
                             select new BomCells
                             {
                                Раздел = values[0].ToString(),
                                Обозначение = values[1].ToString(),
                                Наименование = values[2].ToString(),
                                Материал = values[3].ToString(),
                                МатериалЦми = values[4].ToString(),
                                ТолщинаЛиста = values[5].ToString(),
                                Количество = Convert.ToDecimal(values[6]),
                                ТипФайла = values[7].ToString(),
                                Конфигурация = values[8].ToString(),
                                ПоследняяВерсия = Convert.ToInt32(values[9]),
                                IdPdm = Convert.ToInt32(values[10]),
                                FileName = values[11].ToString(),
                                FilePath = values[12].ToString(),
                                ErpCode = values[13].ToString(),
                                SummMaterial = values[14].ToString(),
                                Weight = values[15].ToString(),
                                CodeMaterial = values[16].ToString(),
                                Format = values[17].ToString(),
                                Note = values[18].ToString(),
                                Уровень = Convert.ToInt32(values[19]),
                                КонфГлавнойСборки = values[20].ToString(),
                                ТипОбъекта = values[21].ToString(),
                                GetPathName = values[22].ToString()
                             });

            //LoggerInfo("Список из полученой таблицы успешно заполнен элементами в количестве" + bomList.Count);
            return bomList;
        }

        #endregion

        readonly SqlQuery _sqlQuery = new SqlQuery();
        public string GetLink(string fileName)
        {
            var url = "";

            foreach (DataRow r in _sqlQuery.GetDocumentAndFolderId(fileName).Rows)
            {
                if (_sqlQuery.GetDocumentAndFolderId(fileName).Rows.Count == 1)
                {
                    var a = r["ProjectID"].ToString();
                    var b = r["DocumentID"].ToString();

                    var link = "conisio://Vents-PDM/explore?projectid=" + a + "&documentid=" + b + "&objecttype=1";

                    url = link;
                }
            }

            return url;
        }
    }
}
