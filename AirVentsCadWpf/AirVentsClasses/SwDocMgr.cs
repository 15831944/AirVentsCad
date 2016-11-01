using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using SolidWorks.Interop.swdocumentmgr;

namespace AirVentsCadWpf.AirVentsClasses
{
    class SwDocMgr
    {
        private const string SLicenseKey = "82EC0E04A67C7A48A3AB757A947C507A8210C1E87738B58E";
        readonly SwDMClassFactory _swClassFact = new SwDMClassFactory();
        private SwDMApplication _swDocMgr;
        private SwDMDocument10 _swDocument10;
        private SwDMDocument13 _swDocument13;
        private SwDmDocumentOpenError _nRetVal;
        private SwDmDocumentType _nDocType;
        private SwDMConfigurationMgr _swCfgMgr;
        private SwDMConfiguration13 _swCfg;
        private SwDmDocumentType SwDocMgrDocType(string filePath)
        {
            if (filePath.Contains("sldprt"))
            {
                _nDocType = SwDmDocumentType.swDmDocumentPart;
            }
            else if (filePath.Contains("sldasm"))
            {
                _nDocType = SwDmDocumentType.swDmDocumentAssembly;
            }
            else if (filePath.Contains("slddrw"))
            {
                _nDocType = SwDmDocumentType.swDmDocumentDrawing;
            }
            else
            {
                _nDocType = SwDmDocumentType.swDmDocumentUnknown;
            }
            return _nDocType;
        }


        public void GetProperty(string filePath, string configName, string fieldName, out string value, out string nPropTypeStr)
        {
                //OpenDoc
                var swClassFact = new SwDMClassFactory();
                var swDocMgr = swClassFact.GetApplication("82EC0E04A67C7A48A3AB757A947C507A8210C1E87738B58E");
                //_nDocType = SwDocMgrDocType(filePath);
                const SwDmDocumentType nDocType = SwDmDocumentType.swDmDocumentPart;
                var swDocument15 = (SwDMDocument15) swDocMgr.GetDocument(@filePath, nDocType, true, out _nRetVal);
                var swDocument13 = (SwDMDocument13) swDocument15;
                var swCfgMgr = swDocument13.ConfigurationManager;
                var swCfg = (SwDMConfiguration13) swCfgMgr.GetConfigurationByName(configName);

                SwDmCustomInfoType nPropType;
                value = swCfg.GetCustomProperty(fieldName, out nPropType);
                nPropTypeStr = nPropType.ToString();

                // CloseDoc
                swDocument15.CloseDoc();
        }

        public void SetProperty(string filePath, string configName, string fieldName, string newValue)
        {
            //OpenDoc
            var swClassFact = new SwDMClassFactory();
            var swDocMgr = swClassFact.GetApplication("82EC0E04A67C7A48A3AB757A947C507A8210C1E87738B58E");
            //_nDocType = SwDocMgrDocType(filePath);
            const SwDmDocumentType nDocType = SwDmDocumentType.swDmDocumentPart;
            var swDocument15 = (SwDMDocument15)swDocMgr.GetDocument(@filePath, nDocType, true, out _nRetVal);
            var swDocument13 = (SwDMDocument13)swDocument15;
            var swCfgMgr = swDocument13.ConfigurationManager;
            var swCfg = (SwDMConfiguration13)swCfgMgr.GetConfigurationByName(configName);

            SwDmCustomInfoType nPropType;

            var value = swCfg.GetCustomProperty(fieldName, out nPropType);

            swCfg.SetCustomProperty(fieldName, newValue);
            
            MessageBox.Show(swCfg.GetCustomProperty(fieldName, out nPropType));
            // SaveDoc
            swDocument15.Save();
            // CloseDoc
            swDocument15.CloseDoc();
        }

        public void SetProperty(string filePath, string configName, string fieldName, out string value, out string nPropTypeStr)
        {
            nPropTypeStr = "";
            value = "";

            //OpenDoc
            var swClassFact = new SwDMClassFactory();
            var swDocMgr = swClassFact.GetApplication("82EC0E04A67C7A48A3AB757A947C507A8210C1E87738B58E");
            //_nDocType = SwDocMgrDocType(filePath);
            const SwDmDocumentType nDocType = SwDmDocumentType.swDmDocumentPart;
            var swDocument15 = (SwDMDocument15)swDocMgr.GetDocument(@filePath, nDocType, true, out _nRetVal);

            var swDocument13 = (SwDMDocument13)swDocument15;

            var swCfgMgr = swDocument13.ConfigurationManager;
            var swCfg = (SwDMConfiguration13)swCfgMgr.GetConfigurationByName(configName);

            ProcessConfigCustomProperties(swCfg);
            try
            {
                SwDmCustomInfoType nPropType;
                value = swCfg.GetCustomProperty("Материал", out nPropType);
                swCfg.SetCustomProperty("Материал", "Лист Оцинковка");
                MessageBox.Show(value);
                value = swCfg.GetCustomProperty("Материал", out nPropType);
                MessageBox.Show(value);
                nPropTypeStr = nPropType.ToString();
            }
            catch (Exception e)
            {

                MessageBox.Show(e.Message);
            }
            // CloseDoc
            swDocument15.CloseDoc();
        }


        public void GetProperty(string filePath, string configName)
        {
            // OpenDoc
            var swClassFact = new SwDMClassFactory();
            var swDocMgr = swClassFact.GetApplication("82EC0E04A67C7A48A3AB757A947C507A8210C1E87738B58E");
            //_nDocType = SwDocMgrDocType(filePath);
            var nDocType = SwDmDocumentType.swDmDocumentPart;
            var swDocument15 = (SwDMDocument15)swDocMgr.GetDocument(@filePath, nDocType, true, out _nRetVal);

            //MessageBox.Show(_nRetVal.ToString());

            var swDocument13 = (SwDMDocument13)swDocument15;

            var swCfgMgr = swDocument13.ConfigurationManager;
            var swCfg = (SwDMConfiguration13)swCfgMgr.GetConfigurationByName(configName);
            
            ProcessConfigCustomProperties(swCfg);
            try
            {
                SwDmCustomInfoType nPropType;
                var propStr = swCfg.GetCustomProperty("Материал", out nPropType);
                swCfg.SetCustomProperty("Материал", "Лист Оцинковка");
                MessageBox.Show(propStr);
                propStr = swCfg.GetCustomProperty("Материал", out nPropType);
                MessageBox.Show(propStr);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            
            // CloseDoc
            swDocument15.CloseDoc();
        }


        public static void ProcessConfigCustomProperties(SwDMConfiguration13 swCfg)
        {
            var vCustPropNameArr = (string[])swCfg.GetCustomPropertyNames();

            if ((vCustPropNameArr == null)) return;
            Debug.Print(" Custom Properties:");

            var array = "";

            foreach (var vCustPropName in vCustPropNameArr)
            {
                SwDmCustomInfoType nPropType;
                var sCustPropStr = swCfg.GetCustomProperty(vCustPropName, out nPropType);
                array = array + "\n" + " " + vCustPropName + " <" + nPropType + "> = " + sCustPropStr;
                Debug.Print(" " + vCustPropName + " <" + nPropType + "> = " + sCustPropStr);
            }
            MessageBox.Show(array);
              Debug.Print("");
        }


        public string SDocFileName { get; set; }

        public List<PartProperties> CustomPropertiesList()
        {
            OpenDoc();
            var customPropertiesList = new List<PartProperties>();
            var partProperties = new PartProperties();
            var customPropertyNames = (object[])_swDocument13.GetCustomPropertyNames();

            //if (customPropertyNames == null) return null;

            foreach (string customPropertyName in customPropertyNames)
            {
                string linkedTo;
                SwDmCustomInfoType nType;
                partProperties.PropName = customPropertyName;
                //MessageBox.Show(customPropertyName.ToString());
                customPropertiesList.Add(partProperties);
                var values = _swDocument13.GetCustomPropertyValues(customPropertyName, out nType, out linkedTo);
                if (values == null) return null;
            }
            return customPropertiesList;
        }

        public class PartProperties
        {
            public string PropName { get; set; }
            private string _nType;
            private string _linkedTo;
        }

        private SwDmDocumentType SwDocMgrDocType()
        {
            if (SDocFileName.ToLower().EndsWith("sldprt"))
            {
                _nDocType = SwDmDocumentType.swDmDocumentPart;
            }
            else if (SDocFileName.ToLower().EndsWith("sldasm"))
            {
                _nDocType = SwDmDocumentType.swDmDocumentAssembly;
            }
            else if (SDocFileName.ToLower().EndsWith("slddrw"))
            {
                _nDocType = SwDmDocumentType.swDmDocumentDrawing;
            }
            else
            {
                // Not a SolidWorks file 
                _nDocType = SwDmDocumentType.swDmDocumentUnknown;
            }
            return _nDocType;
        }
        



        void OpenDoc()
        {
            _swDocMgr = _swClassFact.GetApplication(SLicenseKey);
            _nDocType = SwDocMgrDocType();
            _swDocument10 = (SwDMDocument10)_swDocMgr.GetDocument(SDocFileName, _nDocType, false, out _nRetVal);
            _swDocument13 = (SwDMDocument13)_swDocument10; 
        }

        void CloseDoc()
        {
            _swDocument10.CloseDoc();
        }

        void SaveDoc()
        {
            _swDocument10.Save();
        }



        List<string> GetConfigNames()
        {
            OpenDoc();
            var configNamesList = new List<string>();
            var configmanager = _swDocument13.ConfigurationManager;
            var configs = configmanager.GetConfigurationNames();
            configNamesList.AddRange(configs);
            return configNamesList;
        }

        void GetAllCustomPropertyNamesAndValues()
        {
            var names="";
            string linkedTo;
            
            SwDmCustomInfoType nType;
            var configmanager = _swDocument13.ConfigurationManager;
            var configs = configmanager.GetConfigurationNames();
            foreach (var config in configs)
            {
              MessageBox.Show(config.ToString());
            }

            var customPropertyNames = (object[])_swDocument13.GetCustomPropertyNames(); 
            foreach (var customPropertyName in customPropertyNames)
            {

              var values = _swDocument13.GetCustomPropertyValues((string)customPropertyName, out nType, out linkedTo);
              if (values == null) return;
              names = names + "\n Свойство - " + customPropertyName + " Тип - " + nType + " linkedTo: " + linkedTo;
            }

            MessageBox.Show(names);
        }

        public void GetCustomProperties()
        {
            OpenDoc();


            GetAllCustomPropertyNamesAndValues();



            
            var vCutListItems = (object[])_swDocument13.GetCutListItems2();
            
            
            var sdfvgew = _swDocument13.ConfigurationManager.Document;

            
           
            



            SwDMCutListItem2 cutlist;
            long I;
            SwDmCustomInfoType nType;
            string nLink;
            long j;
            object[] vPropNames;

           var message = "";

           if (vCutListItems == null) return;

           foreach (SwDMCutListItem2 vCutListItem in vCutListItems)
           {
               message = "\n\n" + message + " Name - " + vCutListItem.Name + " Quantity - " + vCutListItem.Quantity; 

               vPropNames = (object[])vCutListItem.GetCustomPropertyNames();

               if (vPropNames == null) continue;
               foreach (var vPropName in vPropNames)
               {
                   message = message + "\n" + " vPropName - " + vPropName + " Value - " +
                       vCutListItem.GetCustomPropertyValue2((string)vPropName, out nType, out nLink) +
                       " Type : " + nType + " Link : " + nLink;
               }
           }
           MessageBox.Show(message);





            Debug.Print("GET CUT-LIST ITEM");

            for (I = 0; I <= vCutListItems.GetUpperBound(0); I++)
            {
                cutlist = (SwDMCutListItem2)vCutListItems[I];
                Debug.Print("Name : " + cutlist.Name);
                Debug.Print(" Quantity : " + cutlist.Quantity);
                vPropNames = (object[])cutlist.GetCustomPropertyNames();

                if (vPropNames != null)
                {
                    Debug.Print(" GET CUSTOM PROPERTIES");
                    for (j = 0; j <= vPropNames.GetUpperBound(0); j++)
                    {
                        Debug.Print(" Property Name : " + vPropNames[j]);
                        Debug.Print(" Property Value : " + cutlist.GetCustomPropertyValue2((string)vPropNames[j], out nType, out nLink));
                        Debug.Print(" Type : " + nType);
                        Debug.Print(" Link : " + nLink);
                    }
                }

                Debug.Print("_________________________");
            }

            cutlist = (SwDMCutListItem2)vCutListItems[0];
            Debug.Print("ADD CUSTOM PROPERTY CALLED Testing1");
            Debug.Print(" Custom Property added? " + cutlist.AddCustomProperty("Testing1", SwDmCustomInfoType.swDmCustomInfoText, "Verify1"));
            Debug.Print(" GET CUSTOM PROPERTIES");

            vPropNames = (object[])cutlist.GetCustomPropertyNames();

            for (j = 0; j <= vPropNames.GetUpperBound(0); j++)
            {
                Debug.Print(" Property Name : " + vPropNames[j]);
                Debug.Print(" Property Value : " + cutlist.GetCustomPropertyValue2((string)vPropNames[j], out nType, out nLink));
                Debug.Print(" Type : " + nType);
                Debug.Print(" Link : " + nLink);
            }

            Debug.Print("_________________________");
            Debug.Print("SET NEW CUSTOM PROPERTY VALUE FOR Testing1");
            Debug.Print(" Property Value Before Setting: " + cutlist.GetCustomPropertyValue2("Testing1", out nType, out nLink));
            Debug.Print(" New Value Set? " + cutlist.SetCustomProperty("Testing1", "Verify3"));
            Debug.Print(" Property Value After Setting : " + cutlist.GetCustomPropertyValue2("Testing1", out nType, out nLink));
            Debug.Print(" GET CUSTOM PROPERTIES");

            vPropNames = (object[])cutlist.GetCustomPropertyNames();

            for (j = 0; j <= vPropNames.GetUpperBound(0); j++)
            {
                Debug.Print(" Property Name : " + vPropNames[j]);
                Debug.Print(" Property Value : " + cutlist.GetCustomPropertyValue2((string)vPropNames[j], out nType, out nLink));
                Debug.Print(" Type : " + nType);
                Debug.Print(" Link : " + nLink);
            }

            Debug.Print("_________________________");
            Debug.Print("DELETE CUSTOM PROPERTY Testing1");
            Debug.Print(" Delete Property Value? " + cutlist.DeleteCustomProperty("Testing1"));

            vPropNames = (object[])cutlist.GetCustomPropertyNames();
            if (vPropNames != null)
            {
                Debug.Print(" GET CUSTOM PROPERTIES");
                for (j = 0; j <= vPropNames.GetUpperBound(0); j++)
                {
                    Debug.Print(" Property Name : " + vPropNames[j]);
                    Debug.Print(" Property Value : " + cutlist.GetCustomPropertyValue2((string)vPropNames[j], out nType, out nLink));
                    Debug.Print(" Type : " + nType);
                    Debug.Print(" Link : " + nLink);
                }
            }

            Debug.Print("_________________________");

            _swDocument10.Save();

            _swDocument10.CloseDoc();

            Console.ReadLine();

        }

         
    }
}
