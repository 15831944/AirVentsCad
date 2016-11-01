using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using AirVentsCadWpf.ServiceVents;

namespace AirVentsCadWpf.DataControls
{
    /// <summary>
    /// Interaction logic for SpigotUC.xaml
    /// </summary>
    public partial class SpigotUc
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpigotUc"/> class.
        /// </summary>
        public SpigotUc()
        {
            InitializeComponent();
        }

        #region To delete

        //void build()
        //{
        //    VentsCadService.ProductPlace place = null;
        //    try
        //    {
        //        using (var client = new VentsCadService.VentsCadServiceClient(App.Service.Binding, App.Service.Address))
        //        {                    
        //            client.Open();                 
        //            place = client.BuildSp(type, width, height);
        //            client.Close();
        //        }
        //    }
        //    catch (Exception ex)
        //    {                
        //        MessageBox.Show(ex.Message);
        //    }
        //    finally
        //    {
        //        ModelSw.Open(place.IdPdm, place.ProjectId, "");              
        //    }
        //}

        //Task Build;

        //void ExportTaskRun(Action action)
        //{
        //    Build = new Task(action);
        //    Build.Start();
        //}

        //string type;
        //string width;
        //string height;

        #endregion

      //  ServiceV serv { get; set; }

        void BuildSpigot_Click(object sender, RoutedEventArgs e)
        {
            // var serv = new ServiceV(new VentsCadService.Parameters
            //{
            //    Name = "spigot",
            //    Type  = new VentsCadService.Type
            //    {
            //        SubType = TypeOfSpigot.Text
            //    },
            //    Sizes = new VentsCadService.Sizes[]
            //    {
            //        new VentsCadService.Sizes
            //        {
            //                Width = WidthSpigot.Text,
            //                Height = HeightSpigot.Text
            //        }
            //    }               
            //});           
            //var Build = new Task(serv.build);
            //Build.Start();                     

            //MessageBox.Show("Чекайте повідомлення після закінчення генерації");

            //return;         

            //m1:

            //VentsCadLibrary.VentsCad.ProductFactory serviceObj = new VentsCadLibrary.VentsCad.ProductFactory(new[] { "spigot", TypeOfSpigot.Text, WidthSpigot.Text, HeightSpigot.Text } );

            //MessageBox.Show(serviceObj.product.Exist.ToString(), "idPdm - " + serviceObj.product.Place?.IdPdm.ToString());
            //serviceObj.product.Build();

            //MessageBox.Show(serviceObj.product.Exist.ToString());
            //MessageBox.Show(serviceObj.product.Place?.IdPdm.ToString());
            //return;

            #region VentsCadLibrary         

            try
            {
                using (var server = new VentsCadLibrary.VentsCad())
                {
                    var newSpigot = new VentsCadLibrary.VentsCad.Spigot(TypeOfSpigot.Text, WidthSpigot.Text, HeightSpigot.Text);
                    if (!newSpigot.Exist)
                    {
                        newSpigot.Build();
                    }
                    var place2 = newSpigot.GetPlace();
                    MessageBox.Show(place2.Path + "\n" + place2.IdPdm + "\n" + place2.ProjectId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            #endregion

        }

        void WidthSpigot_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuildSpigot_Click(this, new RoutedEventArgs());
            }
        }

        void HeightSpigot_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuildSpigot_Click(this, new RoutedEventArgs());
            }
        }

        void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }
      
    }
}
