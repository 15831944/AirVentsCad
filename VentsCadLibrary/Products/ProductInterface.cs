namespace VentsCadLibrary
{
    partial class VentsCad
    { 
        interface WorkWithProduct
        {
            void Build();            
        }

        public class ProductPlace
        {
            public ProductPlace(string path, int idPdm, int projectId)
            {
                IdPdm = idPdm;
                ProjectId = projectId;
                Path = path;
            }
            public int IdPdm;
            public int ProjectId;
            public string Path;
        }        

        public abstract class Product : WorkWithProduct
        {
            internal abstract string ModelName { get; set; }

            internal abstract string ModelPath { get; set; }

            internal abstract string DestinationFolder { get; }
                        
            internal abstract string TemplateFolder { get; }

            public bool Exist => Place != null;

            public abstract ProductPlace Place { get; set; }

            public abstract void Build();

            public ProductPlace GetPlace()
            {
                if (Place != null) return Place;

                string path;
                int fileId;
                int projectId;

                GetExistingFile(ModelName, out path, out fileId, out projectId);
                if (string.IsNullOrEmpty(path))
                {
                    Place = null;
                }
                else
                {
                    Place = new ProductPlace(path, fileId, projectId);
                }

                return Place;
            }            
        }
    }

}
