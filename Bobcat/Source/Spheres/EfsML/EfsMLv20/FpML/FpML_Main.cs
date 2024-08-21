#region Using Directives
using System;
using System.Reflection;

using EFS.GUI.Interface;
using EFS.GUI.Attributes;

using EfsML;
using EfsML.v20;

using FpML.Enum;
using FpML.Interface;

using FpML.v42.Enum;
using FpML.v42.Doc;
using FpML.v42.Msg;
#endregion Using Directives

namespace FpML.v42.Main
{
    #region Document
    /// <summary><newpara></newpara>
    /// <newpara><b>Description :</b> The abstract base type from which all FpML compliant messages and documents must 
    /// be derived.</newpara>
    /// <newpara><b>Contents :</b></newpara> 
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Element: FpML</newpara>
    ///<newpara>• Complex type: DataDocument</newpara>
    ///<newpara>• Complex type: Message</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///<newpara>• Complex type: DataDocument</newpara>
    ///<newpara>• Complex type: Message</newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DataDocument))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EfsDocument))]
    [System.Xml.Serialization.XmlRootAttribute("FpML", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public abstract partial class Document
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public DocumentVersionEnum version;
		#endregion Members
	}
    #endregion Document
}
