using System;
using System.Collections.Generic;
using System.Windows.Forms;
using VentsCadLibrary;

namespace VentsCadServiceLibrary
{    
    public class VentsService : IVentsCadService
    {  
        public void BuildSp(string type, string width, string height, out ProductPlace place)
        {
            busy = true;
            var projectId = 0;
            var idPdm = 0;
            BuildSpigot(type, width, height, out projectId, out idPdm);
            place = new ProductPlace
            {
                IdPdm = idPdm,
                ProjectId = projectId
            };            
        }

        public void Build(Parameters parameters, out ProductPlace place)
        {
            place = null;

            try
            {
                #region Get Type Params

                VentsCad.ProductFactory.Type type = null;

                if (parameters.Type != null)
                {
                    type = new VentsCad.ProductFactory.Type
                    {
                        SubType = parameters.Type.SubType,
                        AddBoolParam1 = parameters.Type.AddBoolParam1,
                        AddBoolParam2 = parameters.Type.AddBoolParam2,
                        AddBoolParam3 = parameters.Type.AddBoolParam3,
                        AddParam1 = parameters.Type.AddParam1,
                        AddParam2 = parameters.Type.AddParam2,
                        AddParam3 = parameters.Type.AddParam3
                    };                    
                }

                #endregion


                #region Get Sizes

                List<VentsCad.ProductFactory.Sizes> sizes = null;

                if (parameters.Sizes?.Count > 0)
                {
                    sizes = new List<VentsCad.ProductFactory.Sizes>();
                    for (int i = 0; i < parameters.Materials.Count; i++)
                    {
                        sizes.Add(new VentsCad.ProductFactory.Sizes
                        {
                            Width = parameters.Sizes[i]?.Width,
                            Height = parameters.Sizes[i]?.Height,
                            Lenght = parameters.Sizes[i]?.Lenght,
                            Depth = parameters.Sizes[i]?.Depth,
                            Thikness = parameters.Sizes[i]?.Thikness,
                            Additional1 = parameters.Sizes[i]?.Additional1,
                            Additional2 = parameters.Sizes[i]?.Additional2,
                            Additional3 = parameters.Sizes[i]?.Additional3,
                        });
                    }
                }

                #endregion

                #region Get Materials

                List<VentsCad.ProductFactory.Material> materials = null;
                if (parameters.Materials?.Count > 0)
                {
                    materials = new List<VentsCad.ProductFactory.Material>();
                    for (int i = 0; i < parameters.Materials.Count; i++)
                    {
                        materials.Add(new VentsCad.ProductFactory.Material
                        {
                            Name = parameters.Materials[i]?.Name,
                            Code = parameters.Materials[i]?.Code,
                            Thikness = parameters.Materials[i]?.Thikness,
                            Value = parameters.Materials[i]?.Value
                        });
                    }
                }

                #endregion

                VentsCad.ProductFactory serviceObj = new VentsCad.ProductFactory(
                    new VentsCad.ProductFactory.Parameters
                    {
                        Name = parameters.Name,
                        Type = type,
                        Sizes = sizes,
                        Materials = materials
                    });

                //MessageBox.Show(serviceObj.product.Exist.ToString(), "idPdm - " + serviceObj.product.Place?.IdPdm.ToString());
                serviceObj.product.Build();

                var getPlace = serviceObj.product.GetPlace();

                place = new ProductPlace
                {
                    IdPdm = getPlace.IdPdm,
                    ProjectId = getPlace.ProjectId
                };

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        public void BuildSpigot(string type, string width, string height, out int projectId, out int idPdm)
        {
            projectId = 0;
            idPdm = 0;
            try
            {
                using (var server = new VentsCad())
                {
                    var newSpigot = new VentsCad.Spigot(type, width, height);
                    if (!newSpigot.Exist)
                    {
                        newSpigot.Build();
                    }
                    var place = newSpigot.GetPlace();

                    projectId = place.ProjectId;
                    idPdm = place.IdPdm;
                }
            }
            catch (Exception ex)
            {               
                MessageBox.Show(ex.Message);
            }
            busy = false;
        }       

        public bool IsBusy()
        {
            return busy;
        }

        internal bool busy { get; set; }
    }
}
