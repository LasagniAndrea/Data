#region using directives
using EFS.ACommon;
using EFS.Common;
using EfsML.Enum;
using EfsML.Interface;
using EfsML.Settlement;
using EfsML.v30.Doc;
using EfsML.v30.Shared;
using FpML.Enum;
using FpML.v44.Shared;
using System;
#endregion using directives

namespace EfsML.v30.Settlement
{

    #region IssiItemsRoutingActorsInfo
    public partial class IssiItemsRoutingActorsInfo : IIssiItemsRoutingActorsInfo
    {
        #region Constructors
        public IssiItemsRoutingActorsInfo() : base() { }
        public IssiItemsRoutingActorsInfo(string pCs, int pIdIssi, IssiItem[] pIssiItems)
            : base(pCs, pIdIssi, pIssiItems)
        {
            issiActorsInfo = new SettlementRoutingActorsBuilder(pCs, idCss, new RoutingCreateElement());
            issiActorsInfo.Load(pCs, GetActorList());
        }
        #endregion Constructors
        #region Methods
        #region protected override CreateEfsSettlementInstruction
        protected override IEfsSettlementInstruction CreateEfsSettlementInstruction()
        {
            return new EfsSettlementInstruction();
        }
        #endregion
        #region GetInstruction
        private IEfsSettlementInstruction GetInstruction(PayerReceiverEnum pPayerReceiver, bool pIsSecurityFlow)
        {
            return GetInstruction(issiActorsInfo, pPayerReceiver, pIsSecurityFlow);
        }
        #endregion GetInstruction
        #endregion Methods

        #region IIssiItemsRoutingActorsInfo
        SettlementRoutingActorsBuilder IIssiItemsRoutingActorsInfo.IssiActorsInfo
        {
            get { return this.issiActorsInfo; }
        }
        #endregion

        #region IIssiItems Members
        IEfsSettlementInstruction IIssiItems.GetInstruction(PayerReceiverEnum pPayerReceiver, bool pIsSecurityFlow)
        {
            return this.GetInstruction(pPayerReceiver, pIsSecurityFlow );
        }
        #endregion IIssiItemCollection
    }
	#endregion IssiItems

    #region SettlementChain
	public partial class SettlementChain : ICloneable, ISettlementChain
	{
		#region Accessors
		#region cssLinkSpecified
		public bool CssLinkSpecified
		{
			get { return (cssLink > 0); }
		}
		#endregion cssLinkSpecified
		#endregion Accessors
		#region Indexors
		public SettlementChainItem this[PayerReceiverEnum payRec]
		{
			get
			{
				if (PayerReceiverEnum.Receiver == payRec)
					return settlementChainItem[0];
				else
					return settlementChainItem[1];
			}
		}
		#endregion Indexors
		#region Constructors
		public SettlementChain()
		{
			settlementChainItem = new SettlementChainItem[2] { new SettlementChainItem(), new SettlementChainItem() };
		}
		#endregion Constructors
		#region Methods
		#region SetSettlementInstruction
        public void SetSettlementInstruction(string pCs, SettlementInstruction pfpmlInstruction)
        {
            SettlementInstruction fpmlsi = (SettlementInstruction)pfpmlInstruction.Clone();
            //
            #region Payer
            //Settlement Method
            EfsSettlementInstruction efsSiPay = new EfsSettlementInstruction();
            if (fpmlsi.settlementMethodSpecified)
            {
                efsSiPay.settlementMethod = (SettlementMethod)fpmlsi.settlementMethod.Clone();
                SQL_Css sqlCss = new SQL_Css(pCs, efsSiPay.settlementMethod.Value, SQL_Table.ScanDataDtEnabledEnum.Yes);
                if (sqlCss.IsLoaded)
                {
                    SettlementRoutingActorsBuilder cssInfo = new SettlementRoutingActorsBuilder(pCs, sqlCss.Id, new RoutingCreateElement());
                    cssInfo.Load(pCs, new int[] { sqlCss.Id });
                    //
                    efsSiPay.settlementMethodInformation = (Routing)cssInfo.GetRouting(sqlCss.Id);
                    efsSiPay.settlementMethodInformationSpecified = (null != efsSiPay.settlementMethodInformation);
                }
            }
            //Correspondant
            efsSiPay.correspondentInformationSpecified = fpmlsi.correspondentInformationSpecified;
            if (fpmlsi.correspondentInformationSpecified)
                efsSiPay.correspondentInformation = (CorrespondentInformation)fpmlsi.correspondentInformation.Clone();

            #endregion Payer
            #region Receiver
            EfsSettlementInstruction efsSiRec = new EfsSettlementInstruction
            {
                intermediaryInformationSpecified = fpmlsi.intermediaryInformationSpecified,
                settlementMethodSpecified = fpmlsi.settlementMethodSpecified,
                // beneficiary is mandatory
                beneficiary = (Beneficiary)fpmlsi.beneficiary.Clone()
            };
            //Settlement Method
            if (fpmlsi.settlementMethodSpecified)
            {
                efsSiRec.settlementMethod = (SettlementMethod)fpmlsi.settlementMethod.Clone();
                //
                if (null != efsSiPay.settlementMethodInformation)
                {
                    efsSiRec.settlementMethodInformation = (Routing)efsSiPay.settlementMethodInformation.Clone();
                    efsSiRec.settlementMethodInformationSpecified = efsSiPay.settlementMethodInformationSpecified;
                }
            }
            //  intermediary
            if (fpmlsi.intermediaryInformationSpecified)
                efsSiRec.intermediaryInformation = (IntermediaryInformation[])fpmlsi.intermediaryInformation.Clone();
            #endregion
            //
            this[PayerReceiverEnum.Payer].settlementInstruction = efsSiPay;
            this[PayerReceiverEnum.Receiver].settlementInstruction = efsSiRec;
        }
		#endregion SetSettlementInstruction
		#endregion Methods

		#region ICloneable Members
		public object Clone()
		{
            SettlementChain clone = (SettlementChain)ReflectionTools.Clone(this, ReflectionTools.CloneStyle.CloneFieldAndProperty);
            return clone;
		}
		#endregion ICloneable Members
		#region ISettlementChain Members
		ISettlementChainItem[] ISettlementChain.SettlementChainItem
		{
			get { return this.settlementChainItem; }
		}
		bool ISettlementChain.CssLinkSpecified
		{
			get { return this.CssLinkSpecified; }
		}
		int ISettlementChain.CssLink
		{
			set { this.cssLink = value; }
			get { return this.cssLink; }
		}
		ISettlementChainItem ISettlementChain.this[PayerReceiverEnum pPayerReceiver]
		{
			get { return this[pPayerReceiver]; }
		}
		IEfsSettlementInstruction[] ISettlementChain.CreateEfsSettlementInstructions()
		{
			return new EfsSettlementInstruction[] { new EfsSettlementInstruction() };
		}
		#endregion ISettlementChain Members
	}
	#endregion SettlementChain
	#region SettlementChainItem
	public partial class SettlementChainItem : ISettlementChainItem
	{
		#region Constructors
		public SettlementChainItem()
		{
			settlementInstruction	= new EfsSettlementInstruction();
			siMode					= SiModeEnum.Undefined;
			//idIssi					= 0;
		}
		#endregion Constructors

		#region ISettlementChainItem Members
		SiModeEnum ISettlementChainItem.SiMode
		{
			set { this.siMode = value; }
			get {return this.siMode;}
		}
		int ISettlementChainItem.IdIssi
		{
			get { return this.idIssi; }
		}
		IEfsSettlementInstruction ISettlementChainItem.SettlementInstruction
		{
			set {this.settlementInstruction = (EfsSettlementInstruction)value;}
			get {return this.settlementInstruction;}
		}
		int ISettlementChainItem.IdASettlementOffice
		{
			set {this.idASettlementOffice = value;}
			get {return this.idASettlementOffice;}
		}
		#endregion ISettlementChainItem Members
	}
	#endregion SettlementChainItem
}
