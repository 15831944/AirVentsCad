using AirVentsCadWpf.AirVentsClasses.UnitsBuilding;
using System;
using System.Windows;

namespace AirVentsCadWpf.ServiceVents
{   
    public class ServiceV
    {
        bool paramsService;

        VentsCadService.Parameters Parameters;
        VentsCadService.VentsCadServiceClient client;

        public ServiceV(VentsCadService.Parameters parameters)
        {
            Parameters = parameters;
            client = new VentsCadService.VentsCadServiceClient(App.Service.Binding, App.Service.Address);
            client.Open();
            paramsService = true;
        }
        
        public ServiceV(string Type, string Width, string Height)
        {
            type = Type;
            width = Width;
            height = Height;

            client = new VentsCadService.VentsCadServiceClient(App.Service.Binding, App.Service.Address);
            client.Open();

            paramsService = false;
        }

        internal  string type { get; set; }
        internal string width { get; set; }
        internal  string height { get; set; }
        
        internal void build()
        {         
            VentsCadService.ProductPlace place = null;
            try
            {
                place = client.Build(Parameters);               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            if (place == null) return;

            ModelSw.Open(place.IdPdm, place.ProjectId, "");
        }

        internal void Busy()
        {           
            try
            {
                MessageBox.Show(client.IsBusy().ToString(), client.State.ToString());              
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }            
        }
    }
}
