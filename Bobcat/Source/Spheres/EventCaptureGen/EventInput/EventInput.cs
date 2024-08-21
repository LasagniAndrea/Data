#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Linq; 

using EFS.ACommon;
using EFS.Common.Web;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;

using EFS.GUI;
using EFS.GUI.CCI;
using EFS.GUI.Interface;



using EFS.Status;
using EFS.Tuning;
using EFS.Permission;

using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;

using FpML.Enum;
using FpML.Interface;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Représente les évènements 
    /// </summary>
    public class EventInput : CommonInput, ICustomCaptureInfos
    {
        #region Members
        private EventCustomCaptureInfos m_CustomCaptureInfos;
        #endregion Members

        #region Accessors
        /// <summary>
        ///  Retourne le résultat du dernier appel de la méthode CloneCurrentEvent
        /// </summary>
        public Event CurrentEventClone
        {
            get;
            set;
        }

        /// <summary>
        /// Obtient les status de l'évènement courant
        /// </summary>
        /// FI 20161124 [22634] Add
        public EventStatus CurrentEventStatus
        {
            set;
            get;
        }


        /// <summary>
        /// Obtient l'évènement courant
        /// </summary>
        public Event CurrentEvent
        {
            get
            {
                if ((-1 < CurrentEventIndex) && (null != EventDocReader) && (null != EventDocReader.@event) && (0 < EventDocReader.@event.Length))
                    return EventDocReader.@event[CurrentEventIndex];
                else
                    return null;
            }
        }

        /// <summary>
        ///  Obtient l'évènement parent du CurrentEvent
        ///  <para>Obtient l'évènement lui même s'il n'a pas de parent</para>
        /// </summary>
        // EG 20180423 Analyse du code Correction [CA1065]
        public Event CurrentEventParent
        {
            get
            {
                if (null == CurrentEvent)
                    throw new InvalidOperationException("CurrentEventParent : CurrentEvent is null");

                int indexEventParent = GetEventIndex(CurrentEvent.idEParent);
                return EventDocReader.@event[indexEventParent];
            }
        }

        /// <summary>
        ///  Obtient l'évènement parent du CurrentEventParent
        ///  <para>Obtient l'évènement lui même s'il n'a pas de parent</para>
        /// </summary>
        public Event GetEventGrandParent
        {
            get
            {
                return EventDocReader[CurrentEventParent.idEParent];
            }
        }


        /// <summary>
        /// Obtient les évènements enfants de l'évènement courant
        /// </summary>
        public Event[] CurrentEventChilds
        {
            get
            {
                Event[] ret = null;

                var childEvent = from item in EventDocReader.@event.Where(x => x.idEParent == CurrentEvent.idE)
                                 select item;

                if (childEvent.Count() > 0)
                    ret = childEvent.ToArray();

                return ret;
            }
        }


        /// <summary>
        /// Index dans la collection de l'évènement courant
        /// </summary>
        public int CurrentEventIndex
        {
            set;
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        public EventCustomCaptureInfos CustomCaptureInfos
        {
            set { m_CustomCaptureInfos = value; }
            get { return m_CustomCaptureInfos; }
        }


        /// <summary>
        ///  Tous les éènements du trade 
        /// </summary>
        public DataSetEvent DataSetEvent
        {
            private set;
            get;
        }


        /// <summary>
        /// 
        /// </summary>
        public Events EventDocReader
        {
            private set;
            get;
        }

        /// <summary>
        ///  Obitent i'IdE de l'évènement courant
        /// </summary>
        public string CurrentEventIdentifier
        {
            get
            {
                if (null != CurrentEvent)
                    return CurrentEvent.idE.ToString();
                return string.Empty;
            }
        }



        #region IdT
        public int IdT
        {
            get
            {
                int ret = 0;
                if (IsTradeFound)
                    ret = SQLTrade.Id;
                return ret;
            }
        }
        #endregion IdT

        /// <summary>
        ///  Retourne s'il existe des évènements sur le trade
        /// </summary>
        public bool IsEventFilled
        {
            get { return (null != DataSetEvent) && (0 < DataSetEvent.DtEvent.Rows.Count); }
        }

        #region	IsTradeFound
        public bool IsTradeFound
        {
            get { return ((null != SQLTrade) && SQLTrade.IsFound); }

        }
        #endregion IsTradeFound

        #region SQLInstrument
        [System.Xml.Serialization.XmlIgnore()]
        public SQL_Instrument SQLInstrument
        {
            private set;
            get;
        }
        #endregion SQLInstrument
        #region SQLProduct
        [System.Xml.Serialization.XmlIgnore()]
        public SQL_Product SQLProduct
        {
            private set;
            get;
        }
        #endregion SQLProduct
        #region SQLTrade
        public SQL_TradeCommon SQLTrade
        {
            private set;
            get;
        }
        /// <summary>
        /// 
        /// </summary>
        public SpheresIdentification TradeIdentification
        {
            private set;
            get;
        }
        
        
        #endregion SQLTrade

        /// <summary>
        /// Identifiant du trade
        /// </summary>
        public string TradeIdentifier
        {
            get
            {
                string ret = string.Empty;
                if (IsTradeFound)
                    ret = SQLTrade.Identifier;
                return ret;
            }
        }

        #endregion Accessors

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        public EventInput()
            : base()
        {
            
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            DataSetEvent = null;
            TradeIdentification = null;
            SQLInstrument = null;
            SQLProduct = null;
            CurrentEventIndex = -1;
            CurrentEventStatus = null;
        }

        /// <summary>
        ///  Charge tous les évènements du trade dans le dataset et alimente m_CurrentEventIndex avec 0
        ///  <para>CurrentEvent est positionné sur le 1er évènement  (m_CurrentEventIndex=0)</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT">idT</param>
        /// <param name="pIdE">IdE</param>
        /// <returns></returns>        
        public void SearchAndDeserializeEvents(string pCS, int pIdT)
        {
            SearchAndDeserializeEvent(pCS, pIdT, 0);
        }

        /// <summary>
        ///  Charge tous les évènements du trade dans le dataset  et alimente m_CurrentEventIndex
        ///  <para>CurrentEvent est positionné sur l'évènement {pIdE}  (m_CurrentEventIndex > 0)</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT">idT</param>
        /// <param name="pIdE">IdE</param>
        /// <returns></returns>
        public void SearchAndDeserializeEvent(string pCS, int pIdT, int pIdE)
        {

            Clear();

            LoadParentTable(pCS, pIdT);

            DataSetEvent = new DataSetEvent(pCS);
            DataSetEvent.Load(null, pIdT);

            EventDocReader = DataSetEvent.GetEvents();

            CurrentEventIndex = -1;
            CurrentEventStatus = null;
            if (null != EventDocReader)
            {
                CurrentEventIndex = GetEventIndex(pIdE);
                LoadCurrentEventStatus(pCS);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// FI 20161124 [22634] Add Method
        public void LoadCurrentEventStatus(string pCS)
        {
            if (CurrentEventIndex == -1)
                throw new NotSupportedException("LoadCurrentEventStatus is not Supported if CurrentEventIndex ==-1");

            int idE = EventDocReader.@event[CurrentEventIndex].idE;
            CurrentEventStatus = new EventStatus();
            CurrentEventStatus.Initialize(pCS, idE);
        }


        /// <summary>
        ///  Recherche l'index ds m_EventDocReader.@event de l'évènement pIdE
        ///  <para>Retourne 0 si lévènement n'existe pas</para>
        /// </summary>
        /// <param name="pIdE"></param>
        /// <returns></returns>
        private int GetEventIndex(int pIdE)
        {
            int ret = 0;
            if ((null != EventDocReader) && (null != EventDocReader.@event) && (0 < EventDocReader.@event.Length))
            {
                for (int i = 0; i < EventDocReader.@event.Length; i++)
                {
                    if (EventDocReader.@event[i].idE == pIdE)
                    {
                        ret = i;
                        break;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        ///  chargment du trade, du produit, instrument
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        // EG 20171025 [23509] Add DTEXECUTION, DTORDERENTERED, TZFACILITY
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private void LoadParentTable(string pCS, int pIdT)
        {
            SQL_TradeCommon sqlTrade = new SQL_TradeCommon(pCS, pIdT);
            bool isOk = sqlTrade.IsFound;
            if (isOk)
            {
                SQLTrade = new SQL_TradeCommon(pCS, pIdT)
                {
                    IsAddRowVersion = true
                };
                isOk = SQLTrade.LoadTable(new string[]{"TRADE.IDT","IDI","IDENTIFIER","DISPLAYNAME","DESCRIPTION","DTTRADE","DTTIMESTAMP","DTEXECUTION","DTORDERENTERED","TZFACILITY","IDT_SOURCE","IDT_TEMPLATE",
                "SOURCE","DTSYS",
                "IDSTENVIRONMENT", "DTSTENVIRONMENT", "IDASTENVIRONMENT", "IDSTBUSINESS", "DTSTBUSINESS", "IDASTBUSINESS",
                "IDSTACTIVATION", "DTSTACTIVATION", "IDASTACTIVATION", "IDSTPRIORITY", "DTSTPRIORITY", "IDASTPRIORITY",
                "IDASTUSEDBY", "IDSTUSEDBY", "DTSTUSEDBY", "LIBSTUSEDBY", "EXTLLINK","ROWATTRIBUT"});
            }
            if (isOk)
            {
                SQLInstrument = new SQL_Instrument(pCS, SQLTrade.IdI);
                isOk = SQLInstrument.IsLoaded;
            }
            if (isOk)
            {
                SQLProduct = new SQL_Product(pCS, SQLInstrument.IdP);
                isOk = SQLProduct.IsLoaded;
            }

            if (isOk)
                TradeIdentification = TradeRDBMSTools.GetTradeIdentification(pCS, null, pIdT);

        }


        /// <summary>
        ///  Alimentation de CurrentEventClone (résultat du clone de CurrentEvent)
        /// </summary>
        public void CloneCurrentEvent()
        {
            if (null != CurrentEvent)
            {
                EFS_SerializeInfoBase serializerInfo = new EFS_SerializeInfoBase(CurrentEvent.GetType(), CurrentEvent);
                CurrentEventClone = (Event)CacheSerializer.CloneDocument(serializerInfo);
            }
        }


        #endregion Methods

        #region ICustomCaptureInfos Membres
        public CustomCaptureInfosBase CcisBase
        {
            get { return (CustomCaptureInfosBase)m_CustomCaptureInfos; }
        }
        #endregion
    }
    
    
    /// <summary>
    /// Pilote la saisie des évènements
    /// </summary>
    public class EventInputGUI : InputGUI
    {
        #region Members
        private string m_ProductCss;
        private int m_IdP;
        private int m_IdI;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public string ProductCss
        {
            get { return m_ProductCss; }
            set { m_ProductCss = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int IdI
        {
            set { m_IdI = value; }
            get { return m_IdI; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int IdP
        {
            set { m_IdP = value; }
            get { return m_IdP; }
        }

        #endregion Accessors

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdMenu"></param>
        /// <param name="pUser"></param>
        /// <param name="pXMLFilePath"></param>
        public EventInputGUI(string pIdMenu, User pUser, string pXMLFilePath)
            : base(pUser, pIdMenu, pXMLFilePath)
        {
            CurrentIdScreen = "AdministrationTradeEvent";
        }
        #endregion Constructors

        #region method
        /// <summary>
        /// 
        /// </summary>
        /// FI 20161123 [22629] Add Method
        public override void InitCaptureMode()
        {
            CaptureMode = Cst.Capture.ModeEnum.Consult;
        }
        #endregion

    }
    
}
