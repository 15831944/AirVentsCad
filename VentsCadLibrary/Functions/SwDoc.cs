using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;


namespace VentsCadLibrary
{
    public partial class VentsCad
    {
        public enum CompType
        {
            COMPONENT,
            DIMENSION,
            BODYFEATURE,
            SKETCH,
            FTRFOLDER
        }

        static string DocCompName(CompType docType)
        {
            switch (docType)
            {
                case CompType.COMPONENT:
                    return "COMPONENT";                    
                case CompType.DIMENSION:
                    return "DIMENSION";
                case CompType.BODYFEATURE:
                    return "BODYFEATURE";
                case CompType.SKETCH:
                    return "SKETCH";
                case CompType.FTRFOLDER:
                    return "FTRFOLDER";
                default:
                    return null;
            }
        }

        public enum Act
        {
            Delete,
            DeletWithOption,
            Unsuppress,
            Suppress,
            DoNothing           
        }

        public static void DoWithSwDoc(SldWorks swApp, CompType docType, string docId, Act doWithDoc)
        {   
            var doc = swApp.IActiveDoc2;            
            var select = doc.Extension.SelectByID2(docId + "@" + doc.GetTitle(), DocCompName(docType), 0, 0, 0, true, 0, null, 0);
            if (!select) return;
            
            if (doWithDoc == Act.DoNothing)
            {
                doc.ClearSelection2(true);
                return;
            }
            switch (doWithDoc)
            {
                case Act.Delete:
                    doc.EditDelete();
                    break;
                case Act.DeletWithOption:
                    const int deleteOption = (int)swDeleteSelectionOptions_e.swDelete_Absorbed +
                                                (int)swDeleteSelectionOptions_e.swDelete_Children;
                    doc.Extension.DeleteSelection2(deleteOption);
                    break;
                case Act.Unsuppress:
                    doc.EditUnsuppress2();
                    break;
                case Act.Suppress:
                    doc.EditSuppress2();
                    break;
                default:
                    break;
            }
            doc.ClearSelection2(true);
        }

        void SetPAram(SldWorks swApp)
        {
            //var doc = swApp.IActiveDoc2;
            //swDoc.Extension.SelectByID2("D1@Кривая16@" + nameDownPanel + "-1@" + nameAsm, "DIMENSION", 0, 0, 0, false, 0, null, 0);
            //((Dimension)(swDoc.Parameter("D1@Кривая16@" + nameDownPanel + ".Part"))).SystemValue = zaklHeight == 1 ? 2 : zaklHeight;
            //swDoc.EditRebuild3();
        }

       




    }
}
