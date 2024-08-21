#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Xml;

using EFS.ACommon;
using EFS.Common;

using EFS.ApplicationBlocks.Data;
using EFS.GUI.Interface;

using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;

using FpML.Enum;
using FpML.Interface;
#endregion Using Directives

namespace EfsML.Settlement.Message
{
    #region NetConventions
    public partial class NetConventions : IComparer
    {
        #region Accessors
        public int Count
        {
            get
            {
                return ArrFunc.Count(netConvention);
            }
        }
        #endregion
        #region Indexors
        public NetConvention this[int pIndex]
        {
            get
            {
                return netConvention[pIndex];
            }
        }
        #endregion Indexors
        #region Constructors
        public NetConventions() { }
        public NetConventions(int pIdI, string pIdC, int pIdA1, int pIdA2)
        {
            idI = pIdI;
            idC = pIdC;
            idA1 = pIdA1;
            idA2 = pIdA2;
        }
        #endregion Constructors
        #region Methods
        #region Sort
        public bool Sort()
        {
            bool isOk = (Count > 0);
            if (isOk)
                Array.Sort(netConvention, this);
            return isOk;
        }
        #endregion Sort
        #endregion Methods

        #region IComparer Members
        #region Compare
        public int Compare(object x, object y)
        {
            //Order by idA,sequenceNumber
            //si ret = -1, netConvX < netConvY,  netConvX est prioritaire 
            //si ret =  1, netConvY < netConvX,  netConvY est prioritaire 
            int ret = 0;
            NetConvention netConvX = (NetConvention)x;
            NetConvention netConvY = (NetConvention)y;

            if (netConvX.idNetConvention != netConvY.idNetConvention)
            {
                if (ret == 0)	//2-0 ou 2-1
                {
                    if (
                        ((netConvX.IsInfoInstrSpecified) && (netConvX.idCSpecified)) &&
                        ((false == netConvY.IsInfoInstrSpecified) || (false == netConvY.idCSpecified))
                        )
                        ret = -1;
                    else if
                        (
                        ((netConvY.IsInfoInstrSpecified) && (netConvY.idCSpecified)) &&
                        ((false == netConvX.IsInfoInstrSpecified) || (false == netConvX.idCSpecified))
                        )
                        ret = 1;
                }
                //
                if (ret == 0) //1-0
                {
                    if (
                        ((netConvX.IsInfoInstrSpecified) || (netConvX.idCSpecified)) &&
                        ((false == netConvY.IsInfoInstrSpecified) && (false == netConvY.idCSpecified))
                        )
                        ret = -1;
                    else if (
                        ((netConvY.IsInfoInstrSpecified) || (netConvY.idCSpecified)) &&
                        ((false == netConvX.IsInfoInstrSpecified) && (false == netConvX.idCSpecified))
                        )
                        ret = 1;
                }
                // Instrument prioritaire par rapport à devise 
                // si Comparaison d'une convention uniquement par instrument face à une convention uniquement par devise => C'est la 1er qui gagne.
                if (ret == 0)
                {
                    if ((netConvX.IsInfoInstrSpecified && (false == netConvX.idCSpecified))
                        ||
                        ((false == netConvY.IsInfoInstrSpecified) && (netConvY.idCSpecified))
                        )
                        ret = -1;
                    else if
                        ((netConvY.IsInfoInstrSpecified && (false == netConvY.idCSpecified))
                        ||
                        ((false == netConvX.IsInfoInstrSpecified) && (netConvX.idCSpecified))
                        )
                        ret = 1;
                }
                //	
                if (ret == 0)
                {
                    if ((netConvX.IsInfoInstrSpecified) && (netConvY.IsInfoInstrSpecified))
                    {
                        if (ret == 0)
                        {
                            if ((netConvX.idISpecified) && (false == netConvY.idISpecified))
                                ret = -1;
                            else if ((netConvY.idISpecified) && (false == netConvX.idISpecified))
                                ret = 1;
                        }
                        //
                        if (ret == 0)
                        {
                            if ((netConvX.idGInstrSpecified) && (false == netConvY.idGInstrSpecified))
                                ret = -1;
                            else if ((netConvY.idGInstrSpecified) && (false == netConvX.idGInstrSpecified))
                                ret = 1;
                        }
                        //
                        if (ret == 0)
                        {
                            if ((netConvX.idPSpecified) && (false == netConvY.idPSpecified))
                                ret = -1;
                            else if ((netConvY.idPSpecified) && (false == netConvX.idPSpecified))
                                ret = 1;
                        }
                        //
                        if (ret == 0)
                        {
                            if ((netConvX.gProductSpecified) && (false == netConvY.gProductSpecified))
                                ret = -1;
                            else if ((netConvY.gProductSpecified) && (false == netConvX.gProductSpecified))
                                ret = 1;
                        }
                    }

                }
                // Default si Egalite (par Exemple 2 conventions sur même Devise)
                if (ret == 0)
                {
                    if ((netConvX.idNetConvention) < (netConvY.idNetConvention))
                        ret = -1;
                    else if ((netConvY.idNetConvention) < (netConvX.idNetConvention))
                        ret = 1;
                }
            }
            return ret;
        }
        #endregion Compare
        #endregion IComparer Members
    }
    #endregion NetConventions
    #region NetConvention
    public partial class NetConvention
    {
        #region Accessors
        public bool IsInfoInstrSpecified
        {
            get
            {
                return (idISpecified || idGInstrSpecified || idPSpecified || gProductSpecified);
            }
        }
        #endregion Accessors
    }
    #endregion NetConvention

    #region NetConventionIds
    public partial class NetConventionIds
    {
        #region Constructors
        public NetConventionIds() { }
        #endregion Constructors
    }
    #endregion NetConventionIds
    #region NetConventionId
    public partial class NetConventionId
    {
        #region Constructors
        public NetConventionId() { }
        public NetConventionId(string pIdScheme, string pValue)
        {
            netConventionIdScheme = pIdScheme;
            Value = pValue;
        }
        #endregion Constructors
    }
    #endregion NetConventionId

    #region NetDesignationIds
    public partial class NetDesignationIds
    {
        #region Constructors
        public NetDesignationIds() { }
        #endregion Constructors
    }
    #endregion NetDesignationIds
    #region NetDesignationId
    public partial class NetDesignationId
    {
        #region Constructors
        public NetDesignationId() { }
        public NetDesignationId(string pIdScheme, string pValue)
        {
            netDesignationIdScheme = pIdScheme;
            Value = pValue;
        }
        #endregion Constructors
    }
    #endregion NetDesignationId

    #region NettingInformation
    public partial class NettingInformation
    {
        #region Constructors
        public NettingInformation() { }
        #endregion Constructors
    }
    #endregion NettingInformation

    #region PaymentId
    public partial class PaymentId
    {
        #region Constructors
        public PaymentId() { }
        public PaymentId(string pIdScheme, string pValue)
        {
            paymentIdScheme = pIdScheme;
            Value = pValue;
        }
        #endregion Constructors
    }
    #endregion PaymentId

    #region SettlementMessagePaymentStructure
    public partial class SettlementMessagePaymentStructure
    {
        #region Constructor
        public SettlementMessagePaymentStructure(int pId, string pIdScheme, DateTime pdtSTM, DateTime pdtSTMForced,
            int pIdASenderParty, int pIdASender, int pIdAReceiver, int pIdAReceiverParty,
            int pIdACss, string pSCRef, string pIdC,
            NettingMethodEnum pNetMethod, int pIdNetConvention, int pIdNetDesignation,
            decimal pAmount, PayerReceiverEnum pPayerReceiverEnum,
            int pIdT, EfsMLDocumentVersionEnum pVersion)
        {
            id = pId;
            idScheme = pIdScheme;
            dtSTM = pdtSTM;
            dtSTMForced = pdtSTMForced;
            //
            idASenderParty = pIdASenderParty;
            idASender = pIdASender;
            idAReceiver = pIdAReceiver;
            idAReceiverParty = pIdAReceiverParty;
            idACss = pIdACss;
            //
            settlementChainReference = pSCRef;
            idC = pIdC;
            //
            netMethod = pNetMethod;
            idNetConvention = pIdNetConvention;
            idNetDesignation = pIdNetDesignation;
            //			
            amount = pAmount;
            payerReceiver = pPayerReceiverEnum;
            //
            idT = pIdT;
            //
            efsMLversion = pVersion;
        }
        public SettlementMessagePaymentStructure(DataRow pRow, string pIdScheme, EfsMLDocumentVersionEnum pVersion)
            : this
            (
            Convert.ToInt32(pRow["ID"]),
            pIdScheme,
            Convert.ToDateTime(pRow["DTSTM"]),
            Convert.ToDateTime(pRow["DTSTMFORCED"]),
            Convert.ToInt32(pRow["IDA_SENDERPARTY"]),
            Convert.ToInt32(pRow["IDA_SENDER"]),
            Convert.ToInt32(pRow["IDA_RECEIVER"]),
            Convert.ToInt32(pRow["IDA_RECEIVERPARTY"]),
            Convert.ToInt32(pRow["IDA_CSS"]),
            Convert.ToString(pRow["SCREF"]),
            Convert.ToString(pRow["IDC"]),
            (NettingMethodEnum)System.Enum.Parse(typeof(NettingMethodEnum), Convert.ToString(pRow["NETMETHOD"]), true),
            pRow["IDNETCONVENTION"] is DBNull ? 0 : Convert.ToInt32(pRow["IDNETCONVENTION"]),
            pRow["IDNETDESIGNATION"] is DBNull ? 0 : Convert.ToInt32(pRow["IDNETDESIGNATION"]),
            pRow["AMOUNT"] is DBNull ? 0 : Convert.ToDecimal(pRow["AMOUNT"]),
            (PayerReceiverEnum)System.Enum.Parse(typeof(PayerReceiverEnum), Convert.ToString(pRow["PAYER_RECEIVER"]), true),
            pRow["IDT"] is DBNull ? 0 : Convert.ToInt32(pRow["IDT"]), pVersion) { }

        #endregion Constructor
    }
    #endregion struct SettlementMessagePaymentStructure

    #region SettlementMessageIds
    public partial class SettlementMessageIds
    {
        #region Constructors
        public SettlementMessageIds() { }
        #endregion Constructors
    }
    #endregion SettlementMessageIds
    #region SettlementMessageId
    public partial class SettlementMessageId
    {
        #region Constructors
        public SettlementMessageId() { }
        public SettlementMessageId(string pIdScheme, string pValue)
        {
            settlementMessageIdScheme = pIdScheme;
            Value = pValue;
        }
        #endregion Constructors
    }
    #endregion SettlementMessageId

}
