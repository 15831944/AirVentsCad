using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using AirVentsCadWpf.Properties;
using System.ServiceModel;
using System.Xml;

namespace AirVentsCadWpf
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        static App()
        {
            try
            {
                FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static int VaultSystemType { get; set; }

        enum VaultTypes : int
        {
            None,
            SwEpdm,
            Intermech
        }

        /// <summary>
        /// 
        /// </summary>
        public static int VaultType
        {
            get { return (int)VaultTypes.Intermech; }
        }

        public class Service
        {
            public static BasicHttpBinding Binding = new BasicHttpBinding
            {
                ReceiveTimeout = TimeSpan.FromMinutes(15),
                SendTimeout = TimeSpan.FromMinutes(15),
                MaxBufferPoolSize = 2147483647, 
                MaxBufferSize = 2147483647,
                MaxReceivedMessageSize = 2147483647, 
                Name = "BasicHttpBinding_IVentsCadService"
            };

            XmlDictionaryReaderQuotas myReaderQuotas = new XmlDictionaryReaderQuotas
            {
                MaxStringContentLength = 2147483647,
                MaxArrayLength = 2147483647,
                MaxBytesPerRead = 2147483647,
                MaxDepth = 2000000,
                MaxNameTableCharCount = 2147483647
            };
            
            //_binding.GetType().GetProperty("ReaderQuotas").SetValue(_binding, myReaderQuotas, null);

            public static EndpointAddress Address { get; set; } = new EndpointAddress(Settings.Default.ServiceAddress);

            public static void SetAddress (string address)
            {
                Address = new EndpointAddress(address);
            }


            public static EndpointAddress GetAddress(string address)
            {
                return new EndpointAddress(address);
            }
        }

        public static class ProductsInWork
        {
            public static void AddProduct(string Name)
            {
                if (List == null)
                {
                    List = new List<string>();
                }
                List.Add(Name);
            }

            public static List<string> List { get; set; }

        }

        //public static string sdfv()
        //{
        //    return productInWork;
        //}



        /// <summary>
        /// Gets the SQL connection string.
        /// </summary>
        /// <value>
        /// The SQL connection string.
        /// </value>
        public static string SqlTestConnectionString
        {
            get
            {
                var builder = new SqlConnectionStringBuilder
                {
                    ["Data Source"] = "PDMSRV",//"192.168.14.11",
                    ["Initial Catalog"] = Settings.Default.TestSqlConnection,
                    ["User ID"] = "sa",
                    ["Password"] = "P@$$w0rd",//  "PDMadmin",
                    ["Persist Security Info"] = true
                };
         
                return builder.ConnectionString;
            }
        }

        /// <summary>
        /// Gets the SQL connection string.
        /// </summary>
        /// <value>
        /// The SQL connection string.
        /// </value>
        public static string SqlConnectionString
        {
            get
            {
                var builder = new SqlConnectionStringBuilder
                {
                    ["Data Source"] = "PDMSRV",//"192.168.14.11",
                    ["Initial Catalog"] = Settings.Default.WorkingSqlConnection,
                    ["User ID"] = "sa",
                    ["Password"] = "P@$$w0rd",//"PDMadmin",
                    ["Persist Security Info"] = true
                };                
                return builder.ConnectionString;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class ElementVisibility
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="picturePath"></param>
            /// <param name="imageUi"></param>
            public static void SetImage(string picturePath, Image imageUi)
            {
                var mapLoader = new BitmapImage();
                mapLoader.BeginInit();
                mapLoader.UriSource = new Uri(picturePath, UriKind.RelativeOrAbsolute);
                mapLoader.EndInit();
                if (imageUi != null) imageUi.Source = mapLoader;
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="list"></param>
            /// <param name="visibility"></param>
            public static void ForUiElementsList(IEnumerable<UIElement> list, Visibility visibility)
            {
                foreach (var uiElement in list)
                {
                    try
                    {
                        uiElement.Visibility = visibility;
                    }
                    catch (Exception)
                    {
                        //
                    }
                }
            }
        }
    }
}