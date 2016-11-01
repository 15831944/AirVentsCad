using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace VentsCadServiceLibrary
{    
    [ServiceContract]
    public interface IVentsCadService
    {
        [OperationContract]
        void Build(Parameters parameters, out ProductPlace place);

        [OperationContract]
        bool IsBusy();

        [OperationContract]
        void BuildSp(string type, string width, string height, out ProductPlace place);

        [OperationContract]
        void BuildSpigot(string type, string width, string height, out int projectId, out int idPdm);
    }

    [Serializable]
    [DataContract]
    public class Parameters
    {
        [DataMember]
        public string Name;
        [DataMember]
        public Type Type;
        [DataMember]
        public List<Sizes> Sizes;
        [DataMember]
        public List<Material> Materials;
    }

    public class Type
    {
        [DataMember]
        public string SubType;
        [DataMember]
        public bool AddBoolParam1; 
        [DataMember]
        public bool AddBoolParam2;
        [DataMember]
        public bool AddBoolParam3;
        [DataMember]
        public string AddParam1;
        [DataMember]
        public string AddParam2;
        [DataMember]
        public string AddParam3;
    }


    [DataContract]
    public class Sizes
    {
        [DataMember]
        public string Width;
        [DataMember]
        public string Height;
        [DataMember]
        public string Lenght;
        [DataMember]
        public string Depth;
        [DataMember]
        public string Thikness;
        [DataMember]
        public string Additional1;
        [DataMember]
        public string Additional2;
        [DataMember]
        public string Additional3;
    }
   
    [DataContract]
    public class ProductPlace
    {
        [DataMember]
        public int IdPdm;
        [DataMember]
        public int ProjectId;
    }

    [DataContract]
    public class Material
    {
        [DataMember]
        public string Value;
        [DataMember]
        public string Thikness;
        [DataMember]
        public string Name;
        [DataMember]
        public string Code;
    }

    #region Example

    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    // You can add XSD files into the project. After building the project, you can directly use the data types defined there, with the namespace "VentsCadServiceLibrary.ContractType".
    [DataContract]
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "Hello ";

        [DataMember]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }

    #endregion
}
