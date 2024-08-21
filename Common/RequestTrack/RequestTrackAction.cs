
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;


using EFS.ACommon;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.Actor;



namespace EFS.Common.Log
{
    /// <summary>
    ///  Type d'actions utilisateurs 
    /// </summary>
    public enum RequestTrackActionEnum
    {
        /// <summary>
        /// Chargement d'une liste (rafraîchissement du grid...)
        /// </summary>
        ListLoad,
        /// <summary>
        /// Exportation d'une liste (Export du grid [SQL, ZIP, XLS, PDF, etc])
        /// </summary>
        ListExport,
        /// <summary>
        /// Process appliqué à une liste (Exemple Unclearing)
        /// </summary>
        ListProcess,
        /// <summary>
        /// Chargement d'un élément (consultation d'un élément de referentiel, consultation d'un trade,...)
        /// </summary>
        ItemLoad,
        /// <summary>
        /// Process appliqué un élément (Création, Modification, Suppression, Exercise, Transfert, etc....)
        /// </summary>
        ItemProcess,
    }

    /// <summary>
    ///  Type de process appliqué à une liste ou à un élément
    /// </summary>
    /// FI 20141021 [20350] Add Enum
    public enum RequestTrackProcessEnum
    {
        /// <summary>
        /// Création 
        /// </summary>
        New,
        /// <summary>
        /// Modification
        /// </summary>
        Modify,
        /// <summary>
        /// Suppression, Annulation
        /// </summary>
        Remove,
        /// <summary>
        /// Annulation avec remplaçante
        /// </summary>
        RemoveReplace,
        /// <summary>
        /// Correction (sur deposit)
        /// </summary>
        Correction,
        /// <summary>
        /// Exercice d'option 
        /// </summary>
        OptionExercise,
        /// <summary>
        /// Assignation d'option 
        /// </summary>
        OptionAssignment,
        /// <summary>
        /// Abandon d'option 
        /// </summary>
        OptionAbandon,
        /// <summary>
        /// Transfert de position
        /// </summary>
        PositionTransfer,
        /// <summary>
        /// Split 
        /// </summary>
        Split,
        /// <summary>
        /// Livraison
        /// </summary>
        UnderlyingDelivery,
        /// <summary>
        /// Reduction de position 
        /// </summary>
        PositionCancellation,
        /// <summary>
        /// Traitement des clôture spécifiques
        /// </summary>
        ClearingSpecific,
        /// <summary>
        /// Traitement des Décompensation
        /// </summary>
        UnClearing,
        /// <summary>
        /// Traitement des Annulation de denouements auto. à l'échéance
        /// </summary>
        UnClearing_MOO,
        /// <summary>
        /// Traitement des Annulation de liquidations de futures à l'échéance
        /// </summary>
        UnClearing_MOF,
        /// <summary>
        /// Traitement des mise à jour des clôtures
        /// </summary>
        UpdateEntry,
        /// <summary>
        /// Traitement de compensation globale 
        /// </summary>
        ClearingBulk,
    }


    /// <summary>
    ///  Mode automatique ou manuelle
    ///  <para>Rafraîchissement auto ou manuel du grid</para>
    /// </summary>
    public enum RequestTrackActionMode
    {
        /// <summary>
        /// Rafraîchissement auto
        /// </summary>
        auto,
        /// <summary>
        /// Rafraîchissement manuel
        /// </summary>
        manual
    }

    /// <summary>
    ///  Type d'exportation du grid
    /// </summary>
    // EG 20190411 [ExportFromCSV]
    public enum RequestTrackExportType
    {
        /// <summary>
        /// 
        /// </summary>
        PDF,
        /// <summary>
        /// 
        /// </summary>
        XLS,
        /// <summary>
        /// 
        /// </summary>
        ZIP,
        /// <summary>
        /// 
        /// </summary>
        SQL,
        CSV,
    }

    /// <summary>
    /// Représente l'action opérée par l'utilisateur 
    /// <para></para>
    /// </summary>
    public class RequestTrackAction
    {
        /// <summary>
        /// Identifiant de l'action
        /// </summary>
        [XmlAttribute("type")]
        public RequestTrackActionEnum type;

        /// <summary>
        /// Action manuelle ou automatique
        /// <para>En principe, les actions sont manuelles</para>
        /// <para>seules les actions de chargement du grid peuvent être automatiques (chgt à l'ouverture ou rafraîchissement)</para>
        /// </summary>
        [XmlAttribute("mode")]
        public RequestTrackActionMode mode;

        /// <summary>
        /// Détail de l'action
        /// <para>les actions s'appliquent sur une liste ou sur un formulaire</para>
        /// </summary>
        /// FI 20141021 [20350] Add element de serialization itemActionDetail
        [XmlElementAttribute("listLoadDetail", typeof(RequestTrackListLoadDetail))]
        [XmlElementAttribute("listExportDetail", typeof(RequestTrackListExportDetail))]
        [XmlElementAttribute("listProcessDetail", typeof(RequestTrackListProcessDetail))]
        [XmlElementAttribute("itemLoadDetail", typeof(RequestTrackItemLoadDetail))]
        [XmlElementAttribute("itemProcessDetail", typeof(RequestTrackItemProcessDetail))]
        public RequestTrackActionDetailBase detail;

        /// <summary>
        ///  Constructor
        /// </summary>
        public RequestTrackAction()
        {
            mode = RequestTrackActionMode.manual;
            type = default;
        }
    }

    /// <summary>
    /// Détails de l'action utilisateur
    /// </summary>
    /// FI 20141021 [20350] Add RequestTrackItemProcessDetail
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestTrackListLoadDetail))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestTrackListExportDetail))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestTrackItemLoadDetail))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestTrackItemProcessDetail))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestTrackListProcessDetail))]
    public abstract class RequestTrackActionDetailBase
    {


    }

    /// <summary>
    /// Détail lié au chargement du grid
    /// </summary>
    public class RequestTrackListLoadDetail : RequestTrackActionDetailBase
    {
        /// <summary>
        /// Représente  
        /// </summary>
        [XmlElement("view")]
        public RequestTrackListView view;

        public RequestTrackListLoadDetail()
        {
            view = new RequestTrackListView();
        }

    }

    /// <summary>
    /// Détail lié à l'exportation du grid
    /// </summary>
    public class RequestTrackListExportDetail : RequestTrackActionDetailBase
    {
        /// <summary>
        /// Représente le  type d'export
        /// </summary>
        [XmlAttribute("type")]
        public RequestTrackExportType type;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("view")]
        public RequestTrackListView view;

        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        public Boolean reportSpecified;

        /// <summary>
        /// Représente le report
        /// </summary>
        [XmlElement("report")]
        public RequestTrackRepository report;

        /// <summary>
        /// 
        /// </summary>
        public RequestTrackListExportDetail()
        {
            view = new RequestTrackListView();
        }

    }



    /// <summary>
    /// Détail lié au proces du grid
    /// </summary>
    public class RequestTrackListProcessDetail : RequestTrackListLoadDetail
    {

        /// <summary>
        /// Représente le type de process
        /// <para>Création, suppression, modification, etc....</para>
        /// </summary>
        [XmlAttribute("type")]
        public RequestTrackProcessEnum type;

        /// <summary>
        /// 
        /// </summary>
        public RequestTrackListProcessDetail()
            : base()
        {

        }
    }






    /// <summary>
    /// Détail lié au chargement d'un élément
    /// </summary>
    public class RequestTrackItemLoadDetail : RequestTrackActionDetailBase, IRequestTrackItem
    {

        RequestTrackItem IRequestTrackItem.Item
        {
            get { return this.item; }
        }

        /// <summary>
        /// Représente l'élément consulté
        /// </summary>
        [XmlElement("item")]
        public RequestTrackItem item;

        /// <summary>
        /// constructor
        /// </summary>
        public RequestTrackItemLoadDetail()
        {
            item = new RequestTrackItem();
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// FI 20141021 [20350] Add
    public interface IRequestTrackItem
    {
        RequestTrackItem Item { get; }
    }



    /// <summary>
    /// Détail du process opéré sur un élément
    /// </summary>
    /// FI 20141021 [20350] Add
    public class RequestTrackItemProcessDetail : RequestTrackActionDetailBase, IRequestTrackItem
    {
        /// <summary>
        /// Obtient l'élément 
        /// </summary>
        RequestTrackItem IRequestTrackItem.Item
        {
            get { return this.item; }
        }

        /// <summary>
        /// Représente le type d'action opérée sur l'élément
        /// <para>Création, suppression, modification, etc....</para>
        /// </summary>
        [XmlAttribute("type")]
        public RequestTrackProcessEnum type;

        /// <summary>
        /// Représente l'élément sur lequel une action est opérée
        /// </summary>
        [XmlElement("item")]
        public RequestTrackItem item;

        /// <summary>
        /// constructor
        /// </summary>
        public RequestTrackItemProcessDetail()
        {
            item = new RequestTrackItem();
        }
    }





    /// <summary>
    /// Représente les éléments à l'origine d'une liste
    /// </summary>
    public class RequestTrackListView
    {
        /// <summary>
        /// Identifiant de la consultation (Exemple TRADEFnO_ALLOC) 
        /// </summary>
        [XmlAttribute("sid")]
        public string identifier;

        /// <summary>
        /// Représente le modèle utilisé par l'utilisateur
        /// </summary>
        [XmlElement("template", IsNullable = false)]
        public RequestTrackRepository template;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean parameterSpecified;
        /// <summary>
        /// Représente les paramètres de la consultation
        /// </summary>
        [XmlArray("parameters")]
        [XmlArrayItem("parameter")]
        public MQueueparameter[] parameter;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean filterSpecified;
        /// <summary>
        /// Représente les filtres utilisés par l'utilisateur
        /// </summary>
        [XmlElement("filter", IsNullable = false)]
        public RequestTrackConsultationFilter filter;


        /// <summary>
        /// constructor
        /// </summary>
        public RequestTrackListView()
        {
            template = new RequestTrackRepository();
        }
    }

    /// <summary>
    /// Représente le filtre
    /// </summary>
    public class RequestTrackConsultationFilter
    {
        [XmlElement("literal", IsNullable = false)]
        public CDATA literal;

        public RequestTrackConsultationFilter()
        {
            literal = new CDATA();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RequestTrackItem : RequestTrackRepository
    {
        /// <summary>
        /// type d'enregistrement (TRADE,TRADERISK,POSCOLLATERAL etc..)
        /// </summary>
        [XmlAttribute("type")]
        public string type;
    }


}