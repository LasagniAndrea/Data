using EFS.GUI.CCI;
using EFS.Import;

// TODO FI 20130321 cette espace de nom doit sortir de SpheresIO
// PM 20180219 [23824] Déplacé dans CommonIO à partir de SpheresIO (PosRequestImport.cs)
namespace EFS.PosRequestInformation.Import
{
    /// <summary>
    /// Représente les constantes utilisées par l'importation ds POSREQUEST
    /// </summary>
    public sealed class PosRequestImportCst
    {
        public const string id = "http://www.efs.org/Spheres/posRequestImport/idPosRequest";
        public const string extlLink = "http://www.efs.org/Spheres/posRequestImport/extllink";

        public const string instrumentIdentifier = "http://www.efs.org/Spheres/posRequestImport/instrumentIdentifier";
    }

    /// <summary>
    /// Pilote l'importation dans POSREQUET 
    /// <para>Seules les importations des actions sur une position sont gérées</para>
    /// <para>Les importations des actions sur un trade sont gérés par TradeImport</para>
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("posRequestImport", IsNullable = false)]
    public class PosRequestImport
    {

        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool settingsSpecified;
        /// <summary>
        /// Représente les rèlages de l'importation
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("settings", IsNullable = false)]
        public ImportSettings settings;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool posRequestInputSpecified;
        /// <summary>
        /// Représente les données du POSREQUEST à importer
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("posRequestInput", typeof(PosRequestImportInput))]
        public PosRequestImportInput posRequestInput;
        #endregion Members

        #region constructor
        public PosRequestImport()
        {
        }
        #endregion
    }

    /// <summary>
    /// Représente les données à importer ds POSREQUEST
    /// </summary>
    public class PosRequestImportInput
    {
        #region accessors
        /// <summary>
        /// Représente les données sous forme d'une collection de CCI
        /// </summary>
        [System.Xml.Serialization.XmlArray("customCaptureInfos")]
        [System.Xml.Serialization.XmlArrayItem("customCaptureInfo", typeof(CustomCaptureInfoDynamicData))]
        public PosRequestCustomCaptureInfos CustomCaptureInfos
        {
            set;
            get;
        }
        #endregion accessors
    }
}
