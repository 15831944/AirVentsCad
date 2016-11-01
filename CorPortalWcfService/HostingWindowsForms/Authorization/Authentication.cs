using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;

namespace HostingWindowsForms.Authorization
{
    public class Authentication
    {
        public bool AuthenticateUser(string UserName, string Password)
        {
            bool status;
            try
            {
                var directoryEntry = new DirectoryEntry("LDAP://" + "vents.local", UserName, Password);
                var directorySearcher = new DirectorySearcher(directoryEntry);
                directorySearcher.FindOne();
                status = true;
            }
            catch
            {
                status = false;

            }
            
            return status;
        }
    }
}
