using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HostingWindowsForms.Data
{
    public class ConnectionString
    {
        public static string ConString = @"Data Source = srvkb; Initial Catalog = SWPlusDB; Persist Security Info=True;User ID = sa; Password=PDMadmin; pooling=True ";
        public static string ConVentsPdm = @"Data Source = srvkb; Initial Catalog = Vents-Pdm; Persist Security Info=True;User ID = sa; Password=PDMadmin; pooling=True";
    }
}
