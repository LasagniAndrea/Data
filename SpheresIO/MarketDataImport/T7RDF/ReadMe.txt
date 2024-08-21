***************************************************
How Build FIXML Classes from RDIFixmlSchema
Remark : this note is based on T7 Release 10
***************************************************

1/XSD tools
command line 
xsd.exe {path}\T7_EMDI_MDI_RDI_XML_Representation_v.10.0.3\rdi\RDIFixmlSchema\fixml-main-5-0-SP2_.xsd /c /f /o:C:\Temp\
This command line create file c:\temp\fixml-main-5-0-SP2_.cs


2/fixml-main-5-0-SP2_.cs
Extract Enums into a new file named T7FixMLEnumv10.cs
Extract Classes into a new file named T7FixMLv10.cs

Add Files T7FixMLEnumv10.cs end T7FixMLv10.cs on SpheresIOProject

3/Enums Rename
-   Rename enums (Enum suffix have to be used as in each enum in Spheres® )
    MarketSegmentStatus_t => MarketSegmentStatusEnum

-   Replace Enum values Itemx by appropriate value declared in  xsd file fixml-fields-base-5-0-SP2_.xsd

-   Add #region, Summary , FIXML Tag name

Example 
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.fixprotocol.org/FIXML-5-0-SP2")]
public enum MarketSegmentStatus_t {
    
    /// <remarks/>
    [System.Xml.Serialization.XmlEnumAttribute("1")]
    Item1,
    
    /// <remarks/>
    [System.Xml.Serialization.XmlEnumAttribute("3")]
    Item3,
}

became 

#region MarketSegmentStatusEnum
/// <summary>
///    Status of market segment.
/// </summary>
/* MarketSegmentStatus(2542) */
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP2")]
public enum MarketSegmentStatusEnum
{
    [System.Xml.Serialization.XmlEnumAttribute("1")]
    Active,
    [System.Xml.Serialization.XmlEnumAttribute("3")]
    Published,
}
#endregion 


4/ class Batch_t
Element Message
- Remove  [System.Xml.Serialization.XmlElementAttribute("Message")]

- Add     [System.Xml.Serialization.XmlElementAttribute("MktDef", typeof(MarketDefinition_message_t))]
          [System.Xml.Serialization.XmlElementAttribute("SecDef", typeof(SecurityDefinition_message_t))]
          [System.Xml.Serialization.XmlElementAttribute("SecStat", typeof(SecurityStatus_message_t))]
          [System.Xml.Serialization.XmlElementAttribute("SecDefUpd", typeof(SecurityDefinitionUpdateReport_message_t))]

Expected XML declarations 
[System.Xml.Serialization.XmlElementAttribute("MktDef", typeof(MarketDefinition_message_t))]
[System.Xml.Serialization.XmlElementAttribute("SecDef", typeof(SecurityDefinition_message_t))]
[System.Xml.Serialization.XmlElementAttribute("SecStat", typeof(SecurityStatus_message_t))]
[System.Xml.Serialization.XmlElementAttribute("SecDefUpd", typeof(SecurityDefinitionUpdateReport_message_t))]
public Abstract_message_t[] Message;