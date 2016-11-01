
using System.Windows.Forms;
using System.Collections.Generic;

namespace VentsCadLibrary
{
    partial class VentsCad
    {
        public class ProductFactory
        {
            public Product product;            

            public class Parameters
            {                
                public string Name { get; set; }
                
                public Type Type { get; set; }

                public List<Sizes> Sizes { get; set; }

                public List<Material> Materials { get; set; }
            }

            public class Type
            {
                public string SubType { get; set; }

                public bool AddBoolParam1 { get; set; }

                public bool AddBoolParam2 { get; set; }

                public bool AddBoolParam3 { get; set; }

                public string AddParam1 { get; set; }
                public string AddParam2 { get; set; }
                public string AddParam3 { get; set; }
            }
          

            public class Sizes
            {
                public string Width { get; set; }

                public string Height { get; set; }

                public string Lenght { get; set; }
                
                public string Depth { get; set; }

                public string Thikness { get; set; }

                public string Additional1 { get; set; }

                public string Additional2 { get; set; }

                public string Additional3 { get; set; }
            }

            public class Material
            {                
                public string Value { get; set; }

                public string Thikness { get; set; }

                public string Name { get; set; }

                public string Code { get; set; }
            }


            public ProductFactory(Parameters parameters)
            {
                try
                {
                    using (var server = new VentsCad())
                    {
                        switch (parameters.Name)
                        {
                            case "spigot":                                
                                product = new Spigot(parameters.Type.SubType, parameters.Sizes[0].Width, parameters.Sizes[0].Height);
                                break;
                            case "dumper":
                                var material = new string[] { parameters.Materials[0].Value, parameters.Materials[0].Thikness, parameters.Materials[0].Name, parameters.Materials[0].Code };
                                product = new Dumper(parameters.Type.SubType, parameters.Sizes[0].Width, parameters.Sizes[0].Height, true, material);
                                break;
                            case "montageFrame":                                
                                product = new MontageFrame(parameters.Type.SubType, parameters.Sizes[0].Width, parameters.Sizes[0].Lenght, parameters.Sizes[0].Additional1, parameters.Materials[0]);
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
                }

                
            }


            public ProductFactory(string[] query)
            {
                using (var server = new VentsCad())
                {
                    switch (query[0])
                    {
                        case "spigot":
                            product = new Spigot(query[1], query[2], query[3]);
                            break;                        
                        default:
                            break;
                    }
                }
            }         
        }
    }
}
