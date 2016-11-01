using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using AirVentsCadWpf.Properties;
using AirVentsCadWpf.Логирование;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace AirVentsCadWpf.AirVentsClasses.UnitsBuilding
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ModelSw
    {

        #region Fields

        /// <summary>
        ///  Папка с исходной моделью "Панель бескаркасной установки". 
        /// </summary>
        const string FrameLessFolder = @"\Библиотека проектирования\DriveWorks\01 - Frameless Design 40mm";

        /// <summary>
        /// The unit frameless oreders
        /// </summary>
        public const string UnitFramelessOreders = @"\Заказы AirVents Frameless";
        
        const string ModelName = "Frameless Design 40mm.SLDASM";

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="byWidth"></param>
        /// <param name="byHeight"></param>
        /// <param name="flange30"></param>
        /// <param name="nemberPanel"></param>
        /// <returns></returns>
        public static string BackPanelConfig(double width, double height, double byWidth, double byHeight, string flange30, int nemberPanel)
        {
            if (width < 10 || height < 10 ) return null;
            return
                $"{width + "_" + height + "_" + byWidth + "_" + byHeight + "_" + flange30 + "_" + nemberPanel}; ";
        }
        
        /// <summary>
        /// 
        /// </summary>
        public class BlockDimensions
        {

            /// <summary>
            /// 
            /// </summary>
            public double Width { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public double Height { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public double Lenght { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public void GetInfo()
            {
                MessageBox.Show("Width - " + Width + " Height - " + Height + " Lenght - " + Lenght +
                    "\ndimO1 - " + DimO1 +
                    "\nPlanel1Width - " + Planel1Width +
                    "\ndimO2 - " + DimO1 +
                    "\nPlanel2Width - " + Planel2Width +
                    "\ndimO3 - " + DimO3 +
                    "\nPlanel3Width - " + Planel3Width);
            }
            
            /// <summary>
            /// 
            /// </summary>
            public double Planel1Width { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double Planel2Width { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double Planel3Width { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double DimO1 { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double DimO2 { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double DimO3 { get; set; }
            
            /// <summary>
            /// 
            /// </summary>
            public double PlanelCentrDoor1 => DimO1 + Planel1Width / 2;

            /// <summary>
            /// 
            /// </summary>
            public double PlanelCentrDoor2  => DimO1 + Planel1Width + DimO2 + Planel2Width / 2;

            /// <summary>
            /// 
            /// </summary>
            public double PlanelCentrDoor3  => DimO1 + Planel1Width + DimO2 + Planel2Width + DimO3 + Planel3Width / 2;
            
            /// <summary>
            /// 
            /// </summary>
            public double Profil1 { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double Profil2 { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double Profil3 { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double Profil4 => Lenght - Planel3Width - Profil3 - Planel2Width - Profil2 - Planel1Width - Profil1;

            /// <summary>
            /// 
            /// </summary>
            public double ProfilCentr1 { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double ProfilCentr2 { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double ProfilCentr3 { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double ProfilCentr4 { get; set; }
        }

        #region FramelessBlock

        /// <summary>
        /// Framelesses the block.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <param name="order">The order.</param>
        /// <param name="leftSide">The side.</param>
        /// <param name="section">The section.</param>
        /// <param name="pDown">The p down.</param>
        /// <param name="pFixed">The p fixed.</param>
        /// <param name="pUp">The p up.</param>
        /// <param name="съемныеПанели">The СЪЕМНЫЕ ПАНЕЛИ.</param>
        /// <param name="промежуточныеСтойки">The ПРОМЕЖУТОЧНЫЕ СТОЙКИ.</param>
        /// <param name="dimensions">The height.</param>
        /// <param name="троцевыеПанели"></param>
        public void FramelessBlock(string size, string order, bool leftSide, string section, string pDown, string pFixed,
            string pUp, string[] съемныеПанели, string[] промежуточныеСтойки, BlockDimensions dimensions, string[] троцевыеПанели)
        {
            Логгер.Информация($"Бескаркасная установка {size}-{order}  {section}", "", "FramelessBlock", "FramelessBlock");
            
            if (!InitializeSw(true)) return;

            var path = FramelessBlockStr(size, order, leftSide, section, pDown, pFixed, pUp, съемныеПанели, промежуточныеСтойки, dimensions, троцевыеПанели);
            
            _swApp.ExitApp();
            DeleteAllPartsToDelete();

            if (OpenIfExist(path, VentsCadLibrary.VaultSystem.VentsCadFile.Type.Part, null)) return;          

            try
            {
                CloseSldAsm(pDown);
                CloseSldAsm(pFixed);
                CloseSldAsm(pUp);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            
            foreach (var панель in съемныеПанели)
            {
                try
                {
                    CloseSldAsm(панель);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }               
            }
            FramelessBlockStr(size, order, leftSide, section, pDown, pFixed, pUp, съемныеПанели, промежуточныеСтойки, dimensions, троцевыеПанели);
            Логгер.Информация("Блок згенерирован", "" , "FramelessBlock", "FramelessBlock");
        }

        void CloseSldAsm(string pDown)
        {
            try
            {
                var fileName = Path.GetFileName(pDown);
                var namePrt = fileName != null && fileName.ToLower().Contains(".sldasm")
                    ? Path.GetFileName(pDown)
                    : Path.GetFileName(pDown) + ".sldasm";
                _swApp.CloseDoc(namePrt);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }            
        }


        /// <summary>
        /// Framelesses the block string.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <param name="order">The order.</param>
        /// <param name="leftSide">The side.</param>
        /// <param name="section">The section.</param>
        /// <param name="панельНижняя">The p down.</param>
        /// <param name="панельНесъемная">The p fixed.</param>
        /// <param name="панельВерхняя">The p up.</param>
        /// <param name="панелиСъемные">The ПАНЕЛИ СЪЕМНЫЕ.</param>
        /// <param name="промежуточныеСтойки">The ПРОМЕЖУТОЧНЫЕ СТОЙКИ.</param>
        /// <param name="dimensions"></param>
        /// <param name="троцевыеПанели"></param>
        /// <returns></returns>
        public string FramelessBlockStr(
            string size,
            string order,
            bool leftSide,
            string section,
            string панельНижняя,
            string панельНесъемная,
            string панельВерхняя,
            string[] панелиСъемные,
            string[] промежуточныеСтойки,
            BlockDimensions dimensions,
            string[] троцевыеПанели)
        {

            #region Start

            var unitAsMmodel = $@"{Settings.Default.SourceFolder}{FrameLessFolder}\{ModelName}";

            var framelessBlockNewName = $"{size} {order}B Section {section}";

            var orderFolder = $@"{Settings.Default.DestinationFolder}\{UnitFramelessOreders}\{size}\{size} {order}B";

            CreateDistDirectory(orderFolder, Settings.Default.PdmBaseName);

            var framelessBlockNewPath =  new FileInfo($@"{orderFolder}\{framelessBlockNewName}.SLDASM").FullName;

            if (File.Exists(framelessBlockNewPath))
            {
                GetLastVersionPdm(new FileInfo(framelessBlockNewPath).FullName, Settings.Default.TestPdmBaseName);
                _swApp.OpenDoc6(framelessBlockNewPath, (int)swDocumentTypes_e.swDocASSEMBLY, (int)swOpenDocOptions_e.swOpenDocOptions_LoadModel, "00", 0, 0);
                return framelessBlockNewPath;
            }

            GetLatestVersionAsmPdm(unitAsMmodel, Settings.Default.PdmBaseName);

            var swDoc = _swApp.OpenDoc6(unitAsMmodel, (int)swDocumentTypes_e.swDocASSEMBLY, (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
            _swApp.Visible = true;
            var swAsm = (AssemblyDoc) swDoc;
            swAsm.ResolveAllLightWeightComponents(false);

            #endregion

            #region Метизы

            if (троцевыеПанели != null)
            {

                if (string.IsNullOrEmpty(троцевыеПанели[0]))
                {
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-163@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-164@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-198@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-199@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-171@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-206@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-172@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-207@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("Панель торцевая входа", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Панель торцевая входа массив", "FTRFOLDER", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();                   
                }
                else if (!leftSide)
                {
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-171@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-206@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                }
                else if (leftSide)
                {
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-172@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-207@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                }

                if (string.IsNullOrEmpty(троцевыеПанели[1]))
                { 
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-181@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-182@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-216@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-217@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();


                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-190@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-225@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-189@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-224@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();


                    swDoc.Extension.SelectByID2("Панель торцевая выхода", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Панель торцевая выхода массив", "FTRFOLDER", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                }
                else if (!leftSide)
                {
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-189@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-224@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                }
                else if (leftSide)
                {
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-190@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-225@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditDelete();
                }
            }

            switch (панелиСъемные[3])
            {
                case "Со скотчем":
                    swDoc.Extension.SelectByID2("Threaded Rivets Increased-11@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Threaded Rivets Increased-8@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Threaded Rivets Increased-12@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Threaded Rivets Increased-7@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Threaded Rivets Increased-1@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Threaded Rivets Increased-2@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
                    break;
                default:
                    swDoc.Extension.SelectByID2("Threaded Rivets-11@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Threaded Rivets-8@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Threaded Rivets-12@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Threaded Rivets-7@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Threaded Rivets-1@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Threaded Rivets-2@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
                    break;
            }

            if (leftSide)
            {
                #region Верхняя и нижняя панели

                try
                {
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-18@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-53@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-53@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Право", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                }
                catch (Exception excepiton)
                {
                    MessageBox.Show(excepiton.ToString());
                }

                #endregion

                #region Передняя и задняя панели

                try
                {
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-74@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-39@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-75@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-40@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-40@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Винт и заглушка правая панель", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("ЗеркальныйКомпонент8", "COMPPATTERN", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Винт и заглушка правая панель массив", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();

                }
                catch (Exception excepiton)
                {
                    MessageBox.Show(excepiton.ToString());
                }

                #endregion

                #region Съемные панели

                if (string.IsNullOrEmpty(панелиСъемные[2]))
                {
                    // Удаление 3-й панели
                    swDoc.Extension.SelectByID2("Третья панель право", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("SC GOST 17473_gost-11@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Washer 11371_gost-41@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets Increased-11@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets-11@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets-11@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();

                    if (string.IsNullOrEmpty(панелиСъемные[1]))
                    {
                        // Удаление 2-й панели
                        swDoc.Extension.SelectByID2("Вторая панель право", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                        swDoc.EditDelete();
                        swDoc.Extension.SelectByID2("SC GOST 17473_gost-7@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("Washer 11371_gost-37@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("Threaded Rivets Increased-7@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("Threaded Rivets-7@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("Threaded Rivets-7@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swDoc.EditDelete();
                    }
                }

                try
                {
                    swDoc.Extension.SelectByID2("Washer 11371_gost-38@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets Increased-8@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets Increased-8@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("Вторая панель лево", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("SC GOST 17473_gost-8@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("Washer 11371_gost-38@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets Increased-8@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.ShowConfiguration2("Рифленая клепальная гайка М8");
                    swDoc.Extension.SelectByID2("Threaded Rivets-8@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Вторая панель лево", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("SC GOST 17473_gost-12@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets-12@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets Increased-12@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Washer 11371_gost-42@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Третья панель лево", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("SC GOST 17473_gost-2@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets-2@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets Increased-2@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Washer 11371_gost-32@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Первая панель лево", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Третья панель лево зерк", "COMPPATTERN", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("DerivedCrvPattern6", "COMPPATTERN", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("DerivedCrvPattern7", "COMPPATTERN", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("DerivedCrvPattern7", "COMPPATTERN", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                }
                catch (Exception excepiton)
                {
                    MessageBox.Show(excepiton.ToString());
                }

                #endregion
            }

            else
            {

                #region Съемные панели

                if (string.IsNullOrEmpty(панелиСъемные[2]))
                {
                    // Удаление 3-й панели
                    swDoc.Extension.SelectByID2("Третья панель лево", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("SC GOST 17473_gost-12@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets-12@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets Increased-12@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Washer 11371_gost-42@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Washer 11371_gost-42@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();

                    if (string.IsNullOrEmpty(панелиСъемные[1]))
                    {
                        // Удаление 2-й панели
                        swDoc.Extension.SelectByID2("Вторая панель лево", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                        swDoc.EditDelete();
                        swDoc.Extension.SelectByID2("SC GOST 17473_gost-8@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("Washer 11371_gost-38@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("Threaded Rivets Increased-8@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("Threaded Rivets-8@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.Extension.SelectByID2("Threaded Rivets-8@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                        swDoc.EditDelete();
                    }
                }

                try
                {
                    swDoc.Extension.SelectByID2("SC GOST 17473_gost-1@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets Increased-1@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Washer 11371_gost-31@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets-1@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets-1@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Первая панель право", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("SC GOST 17473_gost-7@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Washer 11371_gost-37@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets Increased-7@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets-7@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Вторая панель право", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("SC GOST 17473_gost-11@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Washer 11371_gost-41@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets Increased-11@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Threaded Rivets-11@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Третья панель право", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Первая панель право зерк", "COMPPATTERN", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Третья панель право зерк", "COMPPATTERN", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("DerivedCrvPattern4", "COMPPATTERN", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("DerivedCrvPattern5", "COMPPATTERN", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("DerivedCrvPattern5", "COMPPATTERN", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                }
                catch (Exception excepiton)
                {
                    MessageBox.Show(excepiton.ToString());
                }

                #endregion

                #region Верхняя и нижняя панели

                try
                {
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-37@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-2@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-2@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Лево", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                }
                catch (Exception excepiton)
                {
                    MessageBox.Show(excepiton.ToString());
                }

                #endregion

                #region Передняя и задняя панели

                try
                {
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-37@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-72@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-73@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-38@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-38@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Винт и заглушка левая панель", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("ЗеркальныйКомпонент6", "COMPPATTERN", 0, 0, 0, false, 0, null, 0);
                    swDoc.Extension.SelectByID2("Винт и заглушка левая панель массив", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("ЗеркальныйКомпонент6", "COMPPATTERN", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                }
                catch (Exception excepiton)
                {
                    MessageBox.Show(excepiton.ToString());
                }

                #endregion

            }
            

            #endregion

            #region Панели || панелиСъемные[0, 1, 2, 3] - Съемные панели || панелиСъемные[4, 5] - Усиливающие панели

            #region Сторона обслуживания

            var panelLeft1 = leftSide ? панельНесъемная : панелиСъемные[0];
            var panelRight1 = leftSide ? панелиСъемные[0] : панельНесъемная;

            var panelLeft2 = панелиСъемные.Length > 1 ? панелиСъемные[1] : "-";
            var panelLeft3 = панелиСъемные.Length > 2 ? панелиСъемные[2] : "-";

            var panelRight2 = панелиСъемные.Length > 1 ? панелиСъемные[1] : "-";
            var panelRight3 = панелиСъемные.Length > 2 ? панелиСъемные[2] : "-";

            #endregion

            if (panelLeft2 == "-" & string.IsNullOrEmpty(панелиСъемные[4]) & string.IsNullOrEmpty(панелиСъемные[5]))
            {
                swDoc.Extension.SelectByID2("Вторая панель право зерк", "COMPPATTERN", 0, 0, 0, false, 0, null, 0);swDoc.EditSuppress2();
                swDoc.Extension.SelectByID2("Вторая панель право массив", "COMPPATTERN", 0, 0, 0, false, 0, null, 0);swDoc.EditSuppress2();

                swDoc.Extension.SelectByID2("Совпадение1707", "MATE", 0, 0, 0, false, 0, null, 0);swDoc.EditUnsuppress2();swDoc.ClearSelection2(true);
                swDoc.Extension.SelectByID2("Совпадение1708", "MATE", 0, 0, 0, false, 0, null, 0);swDoc.EditUnsuppress2();swDoc.ClearSelection2(true);
            }

            if (panelLeft3 == "-")
            {
                swDoc.Extension.SelectByID2("Третья панель право зерк", "COMPPATTERN", 0, 0, 0, false, 0, null, 0);swDoc.EditSuppress2();
                swDoc.Extension.SelectByID2("Третья панель право массив", "COMPPATTERN", 0, 0, 0, false, 0, null, 0);swDoc.EditSuppress2();
            }
            
            swDoc.Extension.SelectByID2("02-11-40-1-1@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
            try
            {
                swAsm.ReplaceComponents(панельНижняя, "", false, true);
                CloseSldAsm(панельНижняя);
            }
            catch (Exception e){MessageBox.Show(e.Message);}

            swDoc.Extension.SelectByID2("02-11-40-1-3@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
            try
            {
                swAsm.ReplaceComponents(панельВерхняя, "", false, true);
                CloseSldAsm(панельВерхняя);
            }
            catch (Exception e) { MessageBox.Show(e.Message); }

            swDoc.Extension.SelectByID2("02-11-40-1-2@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
            try
            {
                swAsm.ReplaceComponents(panelLeft1, "", false, true);
                CloseSldAsm(panelLeft1);
            }
            catch (Exception e) { MessageBox.Show(e.Message); }

            swDoc.Extension.SelectByID2("02-11-40-1-4@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
            try
            {
                swAsm.ReplaceComponents(panelRight1, "", false, true);
                CloseSldAsm(panelRight1);
            }
            catch (Exception e) { MessageBox.Show(e.Message); }

            if (!leftSide)
            {
                swDoc.Extension.SelectByID2("Совпадение1709", "MATE", 0, 0, 0, false, 0, null, 0);
                swDoc.EditSuppress2();swDoc.ClearSelection2(true);
                swDoc.Extension.SelectByID2("Совпадение9", "MATE", 0, 0, 0, false, 0, null, 0);
                swDoc.EditUnsuppress2();swDoc.ClearSelection2(true);
            }
            else
            {
                swDoc.Extension.SelectByID2("Совпадение1710", "MATE", 0, 0, 0, false, 0, null, 0);
                swDoc.EditSuppress2();swDoc.ClearSelection2(true);
                swDoc.Extension.SelectByID2("Совпадение4", "MATE", 0, 0, 0, false, 0, null, 0);
                swDoc.EditUnsuppress2();swDoc.ClearSelection2(true);
            }

            // Если две съемных панели

            if (panelLeft2 != "-")
            {
                swDoc.Extension.SelectByID2("02-11-40-1-5@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0,
                    false, 0, null, 0);
                if (leftSide)
                {
                    try
                    {
                        swAsm.ReplaceComponents(panelLeft2, null, false, true);
                        CloseSldAsm(panelLeft2);
                    }
                    catch (Exception e) { MessageBox.Show(e.Message); }
                }
                else { swDoc.EditDelete(); }

                swDoc.Extension.SelectByID2("02-11-40-1-7@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0,
                    false, 0, null, 0);

                if (!leftSide)
                {
                    try
                    {
                        swAsm.ReplaceComponents(panelRight2, null, false, true);
                        CloseSldAsm(panelRight2);
                    }
                    catch (Exception e) { MessageBox.Show(e.Message); }
                }
                else { swDoc.EditDelete(); }
            }
            else
            {
                swDoc.Extension.SelectByID2("02-11-40-1-5@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0,
                    false, 0, null, 0);swDoc.EditDelete();
                swDoc.Extension.SelectByID2("02-11-40-1-7@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0,
                    false, 0, null, 0);swDoc.EditDelete();
            }

            // Если три съемных панели

            if (panelLeft3 != "-")
            {
                swDoc.Extension.SelectByID2("02-11-40-1-6@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                if (leftSide)
                {
                    try
                    {
                        swAsm.ReplaceComponents(panelLeft3, null, false, true);
                        CloseSldAsm(panelLeft3);
                    }
                    catch (Exception e) { MessageBox.Show(e.Message); }
                }
                else { swDoc.EditDelete(); }
                
                swDoc.Extension.SelectByID2("02-11-40-1-8@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                if (!leftSide)
                {
                    try
                    {
                        swAsm.ReplaceComponents(panelRight3, null, false, true);
                        CloseSldAsm(panelRight3);
                    }
                    catch (Exception e) { MessageBox.Show(e.Message); }
                }
                else { swDoc.EditDelete(); }
            }
            else
            {
                swDoc.Extension.SelectByID2("02-11-40-1-6@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("02-11-40-1-8@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
            }

            #endregion

            #region Промежуточные стойки

            #region Первая

            if (промежуточныеСтойки[0] != "-" || промежуточныеСтойки[0] != "05")
            {
                swDoc.Extension.SelectByID2("02-11-08-40--1@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                if (leftSide)
                {
                    try
                    {
                        swAsm.ReplaceComponents(промежуточныеСтойки[0], null, false, true);
                        CloseSldAsm(промежуточныеСтойки[0]);
                    }
                    catch (Exception e) { MessageBox.Show(e.Message); }
                }
                else { swDoc.EditDelete(); }

                swDoc.Extension.SelectByID2("02-11-08-40--5@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                if (!leftSide)
                {
                    try
                    {
                        swAsm.ReplaceComponents(промежуточныеСтойки[0], null, false, true);
                        CloseSldAsm(промежуточныеСтойки[0]);
                    }
                    catch (Exception e) { MessageBox.Show(e.Message); }
                }
                else { swDoc.EditDelete(); }
            }
            else
            {
                swDoc.Extension.SelectByID2("02-11-08-40--1@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
                swDoc.Extension.SelectByID2("02-11-08-40--5@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
            }

            if (промежуточныеСтойки[0] == "-" || string.IsNullOrEmpty(промежуточныеСтойки[0]))
            {
                swDoc.Extension.SelectByID2("02-11-08-40--1@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
                swDoc.Extension.SelectByID2("02-11-08-40--5@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
            }

            #endregion

            #region Вторая

            if (промежуточныеСтойки[1] != "-" || промежуточныеСтойки[1] != "05")
            {
                swDoc.Extension.SelectByID2("02-11-08-40--2@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                if (leftSide)
                {
                    try
                    {
                        swAsm.ReplaceComponents(промежуточныеСтойки[1], "", false, true);
                        CloseSldAsm(промежуточныеСтойки[1]);
                    }
                    catch (Exception e) { MessageBox.Show(e.Message); }
                }
                else { swDoc.EditDelete(); }

                swDoc.Extension.SelectByID2("02-11-08-40--6@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                if (!leftSide)
                {
                    try
                    {
                        swAsm.ReplaceComponents(промежуточныеСтойки[1], "", false, true);
                        CloseSldAsm(промежуточныеСтойки[1]);
                    }
                    catch (Exception e) { MessageBox.Show(e.Message); }
                }
                else { swDoc.EditDelete(); }
            }
            else
            {
                swDoc.Extension.SelectByID2("02-11-08-40--2@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("02-11-08-40--6@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
            }

            if (промежуточныеСтойки[1] == "-" || string.IsNullOrEmpty(промежуточныеСтойки[1]))
            {
                swDoc.Extension.SelectByID2("02-11-08-40--2@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("02-11-08-40--6@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
            }

            #endregion

            #region Третья

            if (промежуточныеСтойки[2] != "-" || промежуточныеСтойки[2] != "05")
            {
                swDoc.Extension.SelectByID2("02-11-08-40--3@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                if (leftSide)
                {
                    try
                    {
                        swAsm.ReplaceComponents(промежуточныеСтойки[2], "", false, true);
                        CloseSldAsm(промежуточныеСтойки[2]);
                    }
                    catch (Exception e) { MessageBox.Show(e.Message); }
                }
                else { swDoc.EditDelete(); }

                swDoc.Extension.SelectByID2("02-11-08-40--7@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                if (!leftSide)
                {
                    try
                    {
                        swAsm.ReplaceComponents(промежуточныеСтойки[2], "", false, true);
                        CloseSldAsm(промежуточныеСтойки[2]);
                    }
                    catch (Exception e) { MessageBox.Show(e.Message); }
                }
                else { swDoc.EditDelete(); }
            }
            else
            {
                swDoc.Extension.SelectByID2("02-11-08-40--3@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("02-11-08-40--7@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
            }

            if (промежуточныеСтойки[2] == "-" || string.IsNullOrEmpty(промежуточныеСтойки[2]))
            {
                swDoc.Extension.SelectByID2("02-11-08-40--3@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("02-11-08-40--7@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
            }

            #endregion

            #region Четвертая

            if (промежуточныеСтойки[3] != "-" || промежуточныеСтойки[3] != "05")
            {
                swDoc.Extension.SelectByID2("02-11-08-40--4@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                if (leftSide)
                {
                    try
                    {
                        swAsm.ReplaceComponents(промежуточныеСтойки[3], "", false, true);
                        CloseSldAsm(промежуточныеСтойки[3]);
                    }
                    catch (Exception e) { MessageBox.Show(e.Message); }
                }
                else { swDoc.EditDelete(); }

                swDoc.Extension.SelectByID2("02-11-08-40--8@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                if (!leftSide)
                {
                    try
                    {
                        swAsm.ReplaceComponents(промежуточныеСтойки[3], "", false, true);
                        CloseSldAsm(промежуточныеСтойки[3]);
                    }
                    catch (Exception e) { MessageBox.Show(e.Message); }
                }
                else { swDoc.EditDelete(); }
            }
            else
            {
                swDoc.Extension.SelectByID2("02-11-08-40--4@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("02-11-08-40--8@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
            }

            if (промежуточныеСтойки[3] == "-" || string.IsNullOrEmpty(промежуточныеСтойки[3]))
            {
                swDoc.Extension.SelectByID2("02-11-08-40--4@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("02-11-08-40--8@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
            }

            #endregion

            #endregion

            #region Сохранение

            swDoc.Extension.SelectByID2("17-AV06-3C-L-3@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
            swDoc.Extension.SelectByID2("16-AV06-2H-1@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
            
            _swApp.IActivateDoc2(ModelName, true, 0);
            swDoc.ForceRebuild3(false);

            #region Услиливающие панели

            if (!string.IsNullOrEmpty(панелиСъемные[4]))
            {
                swDoc.Extension.SelectByID2("ЗеркальныйКомпонент12", "COMPPATTERN", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();
                swDoc.Extension.SelectByID2("02-11-40-1-9@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                if (!leftSide)
                {
                    try
                    {
                        swAsm.ReplaceComponents(панелиСъемные[4], "", false, true);
                        CloseSldAsm(панелиСъемные[4]);
                    }
                    catch (Exception e) { MessageBox.Show(e.Message); }

                    swDoc.Extension.SelectByID2("ПУВ9", "FTRFOLDER", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();
                    swDoc.Extension.SelectByID2("ПУ9", "FTRFOLDER", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();

                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-158@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditUnsuppress2();
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-159@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditUnsuppress2();
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-123@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditUnsuppress2();
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-124@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();

                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-159@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditUnsuppress2();
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-123@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditUnsuppress2();
                }
                else
                {
                    swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-158@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-159@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-123@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-124@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-159@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-123@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("ПУВ9", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("ПУ9", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("МПУ9", "COMPPATTERN", 0, 0, 0, false, 0, null, 0); swDoc.EditSuppress2();
                    swDoc.Extension.SelectByID2("МПУВ9", "COMPPATTERN", 0, 0, 0, false, 0, null, 0); swDoc.EditSuppress2();
                }

                swDoc.Extension.SelectByID2("02-11-40-1-11@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                if (leftSide)
                {
                    try
                    {
                        swAsm.ReplaceComponents(панелиСъемные[4], "", false, true);
                        CloseSldAsm(панелиСъемные[4]);
                    }
                    catch (Exception e) { MessageBox.Show(e.Message); }

                    swDoc.Extension.SelectByID2("ПУВ11", "FTRFOLDER", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();
                    swDoc.Extension.SelectByID2("ПУ11", "FTRFOLDER", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();

                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-138@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditUnsuppress2();
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-139@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditUnsuppress2();
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-103@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditUnsuppress2();
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-104@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();

                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-180@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditUnsuppress2();
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-145@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditUnsuppress2();
                }
                else
                {
                    swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-138@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-139@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-103@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-104@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-180@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-145@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("ПУВ11", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("ПУ11", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("МПУ11", "COMPPATTERN", 0, 0, 0, false, 0, null, 0); swDoc.EditSuppress2();
                    swDoc.Extension.SelectByID2("МПУВ11", "COMPPATTERN", 0, 0, 0, false, 0, null, 0); swDoc.EditSuppress2();
                }
            }
            if (string.IsNullOrEmpty(панелиСъемные[4]))
            {
                swDoc.Extension.SelectByID2("02-11-40-1-9@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("02-11-40-1-11@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();

                swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-158@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-159@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-123@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-124@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-159@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-123@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("ПУВ9", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
                swDoc.Extension.SelectByID2("ПУ9", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();

                swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-138@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-139@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-103@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-104@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-180@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-145@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("ПУВ11", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();
                swDoc.Extension.SelectByID2("ПУ11", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);swDoc.EditDelete();

                swDoc.Extension.SelectByID2("МПУ9", "COMPPATTERN", 0, 0, 0, false, 0, null, 0); swDoc.EditSuppress2();
                swDoc.Extension.SelectByID2("МПУВ9", "COMPPATTERN", 0, 0, 0, false, 0, null, 0); swDoc.EditSuppress2();
                swDoc.Extension.SelectByID2("МПУ11", "COMPPATTERN", 0, 0, 0, false, 0, null, 0); swDoc.EditSuppress2();
                swDoc.Extension.SelectByID2("МПУВ11", "COMPPATTERN", 0, 0, 0, false, 0, null, 0); swDoc.EditSuppress2();
            }

            if (!string.IsNullOrEmpty(панелиСъемные[5]))
            {
                swDoc.Extension.SelectByID2("02-11-40-1-10@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                if (!leftSide)
                {
                    try
                    {
                        swAsm.ReplaceComponents(панелиСъемные[5], "", false, true);
                        CloseSldAsm(панелиСъемные[5]);
                    }
                    catch (Exception e) { MessageBox.Show(e.Message); }

                    swDoc.Extension.SelectByID2("ПУВ10", "FTRFOLDER", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();
                    swDoc.Extension.SelectByID2("ПУ10", "FTRFOLDER", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();

                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-168@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditUnsuppress2();
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-169@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditUnsuppress2();
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-133@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditUnsuppress2();
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-134@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();

                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-179@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditUnsuppress2();
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-144@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditUnsuppress2();
                }
                else
                {
                    swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-168@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-169@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-133@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-134@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-179@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-144@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("ПУВ10", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("ПУ10", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("МПУ10", "COMPPATTERN", 0, 0, 0, false, 0, null, 0); swDoc.EditSuppress2();
                    swDoc.Extension.SelectByID2("МПУВ10", "COMPPATTERN", 0, 0, 0, false, 0, null, 0); swDoc.EditSuppress2();
                }
               
                swDoc.Extension.SelectByID2("02-11-40-1-12@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                if (leftSide)
                {
                    try
                    {
                        swAsm.ReplaceComponents(панелиСъемные[5], "", false, true);
                        CloseSldAsm(панелиСъемные[5]);
                    }
                    catch (Exception e) { MessageBox.Show(e.Message); }

                    swDoc.Extension.SelectByID2("ПУВ12", "FTRFOLDER", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();
                    swDoc.Extension.SelectByID2("ПУ12", "FTRFOLDER", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();

                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-148@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditUnsuppress2();
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-149@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditUnsuppress2();
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-113@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditUnsuppress2();
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-114@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0); swDoc.EditUnsuppress2();

                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-181@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditUnsuppress2();
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-146@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0); swDoc.EditUnsuppress2();
                }
                else
                {
                    swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-148@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-149@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-113@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-114@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-181@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-146@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                    swDoc.Extension.SelectByID2("ПУВ12", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();
                    swDoc.Extension.SelectByID2("ПУ12", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                    swDoc.EditDelete();

                    swDoc.Extension.SelectByID2("МПУ12", "COMPPATTERN", 0, 0, 0, false, 0, null, 0); swDoc.EditSuppress2();
                    swDoc.Extension.SelectByID2("МПУВ12", "COMPPATTERN", 0, 0, 0, false, 0, null, 0); swDoc.EditSuppress2();
                }
            }
            else if (string.IsNullOrEmpty(панелиСъемные[5]))
            {
                swDoc.Extension.SelectByID2("02-11-40-1-10@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("02-11-40-1-12@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();

                swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-168@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-169@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-133@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-134@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-179@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-144@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("ПУВ10", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("ПУ10", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();

                swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-148@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-149@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-113@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-114@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("Заглушка пластикова для т.о. 16х13-181@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("Винт саморез DIN 7504 K-146@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                swDoc.Extension.SelectByID2("ПУВ12", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();
                swDoc.Extension.SelectByID2("ПУ12", "FTRFOLDER", 0, 0, 0, false, 0, null, 0);
                swDoc.EditDelete();

                swDoc.Extension.SelectByID2("МПУ10", "COMPPATTERN", 0, 0, 0, false, 0, null, 0); swDoc.EditSuppress2();
                swDoc.Extension.SelectByID2("МПУВ10", "COMPPATTERN", 0, 0, 0, false, 0, null, 0); swDoc.EditSuppress2();
                swDoc.Extension.SelectByID2("МПУ12", "COMPPATTERN", 0, 0, 0, false, 0, null, 0); swDoc.EditSuppress2();
                swDoc.Extension.SelectByID2("МПУВ12", "COMPPATTERN", 0, 0, 0, false, 0, null, 0); swDoc.EditSuppress2();
            }

            #endregion

            #region Крыша 

            //if (Path.GetFileNameWithoutExtension(панельВерхняя).Remove(5).Contains("22"))
            //{
            //    swDoc.Extension.SelectByID2("15-000-1@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
            //    swDoc.EditUnsuppress2();swDoc.ClearSelection2(true);
            //}

            #endregion

            #region Рама монтажная

            //if (Path.GetFileNameWithoutExtension(панельНижняя).Remove(5).Contains("31"))
            //{
            //    swDoc.Extension.SelectByID2("10-3-980-700-1@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
            //    swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
            //}

            #endregion

            #region Ножки опорные

            string value = null;
            try
            {
                value = Path.GetFileNameWithoutExtension(панельНижняя).Remove(5);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }

            if (!string.IsNullOrEmpty(value))
            {
                if (value.Contains("32"))
                {
                    try
                    {
                        swDoc.Extension.SelectByID2("ВНС-901.92.001-1@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                        swDoc.Extension.SelectByID2("Washer 11371_gost-1@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                        swDoc.Extension.SelectByID2("Washer 11371_gost-2@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                        swDoc.Extension.SelectByID2("Hex Nut 5915_gost-1@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                        swDoc.Extension.SelectByID2("Hex Nut 5915_gost-2@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                        swDoc.Extension.SelectByID2("Регулируемая ножка-1@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                        swDoc.Extension.SelectByID2("ВНС-901.92.001-2@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                        swDoc.Extension.SelectByID2("Washer 11371_gost-3@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                        swDoc.Extension.SelectByID2("Washer 11371_gost-4@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                        swDoc.Extension.SelectByID2("Hex Nut 5915_gost-3@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                        swDoc.Extension.SelectByID2("Hex Nut 5915_gost-4@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                        swDoc.Extension.SelectByID2("Регулируемая ножка-2@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                        swDoc.Extension.SelectByID2("ВНС-901.92.001-3@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                        swDoc.Extension.SelectByID2("Washer 11371_gost-5@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                        swDoc.Extension.SelectByID2("Washer 11371_gost-6@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                        swDoc.Extension.SelectByID2("Hex Nut 5915_gost-5@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                        swDoc.Extension.SelectByID2("Hex Nut 5915_gost-6@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                        swDoc.Extension.SelectByID2("Регулируемая ножка-3@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                        swDoc.Extension.SelectByID2("ВНС-901.92.001-4@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                        swDoc.Extension.SelectByID2("Washer 11371_gost-7@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                        swDoc.Extension.SelectByID2("Washer 11371_gost-8@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                        swDoc.Extension.SelectByID2("Hex Nut 5915_gost-7@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                        swDoc.Extension.SelectByID2("Hex Nut 5915_gost-8@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                        swDoc.Extension.SelectByID2("Регулируемая ножка-4@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                        swDoc.Extension.SelectByID2("Регулируемая ножка-4@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, true, 0, null, 0);
                        swDoc.EditUnsuppress2(); swDoc.ClearSelection2(true);
                    }
                    catch (Exception)
                    {
                        //
                    }
                }
            }

            #endregion

            #region Торцевые панели

            if (троцевыеПанели != null)
            {
                _swApp.IActivateDoc2(ModelName, true, 0);
                swDoc.ForceRebuild3(false);

                swDoc.Extension.SelectByID2("02-11-40-1-13@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);
                if (!string.IsNullOrEmpty(троцевыеПанели[0]))
                {
                    try
                    {
                        swAsm.ReplaceComponents(троцевыеПанели[0], "", false, true);
                        CloseSldAsm(троцевыеПанели[0]);
                    }
                    catch (Exception e) { MessageBox.Show(e.Message); }
                }

                if (string.IsNullOrEmpty(троцевыеПанели[0]))
                {
                    swDoc.EditDelete();
                }

                swDoc.Extension.SelectByID2("02-11-40-1-14@" + ModelName.Replace(".SLDASM", ""), "COMPONENT", 0, 0, 0, false, 0, null, 0);

                if (!string.IsNullOrEmpty(троцевыеПанели[1]))
                {
                    try
                    {
                        swAsm.ReplaceComponents(троцевыеПанели[1], "", false, true);
                        CloseSldAsm(троцевыеПанели[1]);
                    }
                    catch (Exception e) { MessageBox.Show(e.Message); }
                }

                if (string.IsNullOrEmpty(троцевыеПанели[1]))
                {
                    swDoc.EditDelete();
                }
            }

            #endregion
            
            swDoc.Extension.SelectByID2("Расстояние5", "MATE", 0, 0, 0, false, 0, null, 0);
            swDoc.ActivateSelectedFeature();
            swDoc.Extension.SelectByID2("D1@Расстояние5@" + ModelName.Replace(".SLDASM", ""), "DIMENSION", 0, 0, 0, true, 0, null, 0);
            ((Dimension)(swDoc.Parameter("D1@Расстояние5"))).SystemValue = (dimensions.Height) / 1000;

            swDoc.Extension.SelectByID2("Расстояние8", "MATE", 0, 0, 0, false, 0, null, 0);
            swDoc.ActivateSelectedFeature();
            swDoc.Extension.SelectByID2("D1@Расстояние8@" + ModelName.Replace(".SLDASM", ""), "DIMENSION", 0, 0, 0, true, 0, null, 0);
            ((Dimension)(swDoc.Parameter("D1@Расстояние8"))).SystemValue = (dimensions.Width) / 1000;

            try
            {
                swDoc.Extension.SelectByID2("D1@WL@" + ModelName.Replace(".SLDASM", ""), "DIMENSION", 0, 0, 0, true, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D1@WL"))).SystemValue = (dimensions.Width / 2) / 1000;
                swDoc.Extension.SelectByID2("D1@WR@" + ModelName.Replace(".SLDASM", ""), "DIMENSION", 0, 0, 0, true, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D1@WR"))).SystemValue = (dimensions.Width / 2) / 1000;

                swDoc.Extension.SelectByID2("D1@HU@" + ModelName.Replace(".SLDASM", ""), "DIMENSION", 0, 0, 0, true, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D1@HU"))).SystemValue = (dimensions.Height / 2) / 1000;
                swDoc.Extension.SelectByID2("D1@HD@" + ModelName.Replace(".SLDASM", ""), "DIMENSION", 0, 0, 0, true, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D1@HD"))).SystemValue = (dimensions.Height / 2) / 1000;

                swDoc.Extension.SelectByID2("D1@LF@" + ModelName.Replace(".SLDASM", ""), "DIMENSION", 0, 0, 0, true, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D1@LF"))).SystemValue = (dimensions.Lenght / 2) / 1000;
                swDoc.Extension.SelectByID2("D1@LB@" + ModelName.Replace(".SLDASM", ""), "DIMENSION", 0, 0, 0, true, 0, null, 0);
                ((Dimension)(swDoc.Parameter("D1@LB"))).SystemValue = (dimensions.Lenght / 2) / 1000;

                try
                {
                    swDoc.Extension.SelectByID2("D1@Plane1@" + ModelName.Replace(".SLDASM", ""), "DIMENSION", 0, 0, 0, true, 0, null, 0);
                    ((Dimension)(swDoc.Parameter("D1@Plane1"))).SystemValue = (dimensions.PlanelCentrDoor1) / 1000;
                }
                catch (Exception e){MessageBox.Show(e.ToString());}
                try
                {
                    swDoc.Extension.SelectByID2("D1@Plane2@" + ModelName.Replace(".SLDASM", ""), "DIMENSION", 0, 0, 0, true, 0, null, 0);
                    ((Dimension)(swDoc.Parameter("D1@Plane2"))).SystemValue = (dimensions.PlanelCentrDoor2) / 1000;
                }
                catch (Exception e){MessageBox.Show(e.ToString());}
                try
                {
                    swDoc.Extension.SelectByID2("D1@Plane3@" + ModelName.Replace(".SLDASM", ""), "DIMENSION", 0, 0, 0, true, 0, null, 0);
                    ((Dimension)(swDoc.Parameter("D1@Plane3"))).SystemValue = (dimensions.PlanelCentrDoor3) / 1000;
                }
                catch (Exception e){MessageBox.Show(e.ToString());}

                #region

                //try
                //{
                //    swDoc.Extension.SelectByID2("D1@RemProfilPlane1@" + ModelName.Replace(".SLDASM", ""), "DIMENSION", 0, 0, 0, true, 0, null, 0);
                //    ((Dimension)(swDoc.Parameter("D1@RemProfilPlane1"))).SystemValue = (dimensions.PlanelCentrDoor3) / 1000;
                //}
                //catch (Exception e)
                //{
                //    MessageBox.Show(e.ToString());
                //}

                #endregion

            }
            catch (Exception e){MessageBox.Show(e.ToString());}

            swDoc = ((ModelDoc2)(_swApp.ActiveDoc));
            GabaritsForPaintingCamera(swDoc);
            swDoc.ForceRebuild3(true);
            swDoc.ForceRebuild3(false);

            foreach (var keyValuePair in _patternsInAsmToDelete)
            {
                swDoc.Extension.SelectByID2(keyValuePair.Value + "@" + ModelName.Replace(".SLDASM", ""), "COMPPATTERN", 0, 0, 0, true, 0, null, 0);
                swAsm.DissolveComponentPattern();
            }

            swDoc.SaveAs2(framelessBlockNewPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion, false, true);
            //NewComponents.Add(new FileInfo(framelessBlockNewPath));
            _swApp.CloseDoc(new FileInfo(framelessBlockNewPath).Name);
            CheckInOutPdm(new List<FileInfo> { new FileInfo(framelessBlockNewPath)}, true, Settings.Default.TestPdmBaseName);
            
            #endregion

            return framelessBlockNewPath;
        }

        #endregion
    }
}
