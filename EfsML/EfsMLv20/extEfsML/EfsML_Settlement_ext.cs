#region using directives
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Collections;
using System.Xml.Serialization;
using System.Reflection;

using EFS.ACommon;
using EFS.Common;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;

using EFS.GUI.Interface;

using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using EfsML.Settlement;

using EfsML.v20;

using FpML.Enum;
using FpML.Interface;

using FpML.v42.Enum;
using FpML.v42.Shared;
using FpML.v42.Doc;
#endregion using directives

namespace EfsML.v20.Settlement
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

        #region CreateEfsSettlementInstruction
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
        SettlementRoutingActorsBuilder IIssiItemsRoutingActorsInfo.issiActorsInfo
        {
            get { return this.issiActorsInfo; }
        }
        #endregion

        #region IIssiItems Members
        IEfsSettlementInstruction IIssiItems.GetInstruction(PayerReceiverEnum pPayerReceiver, bool pIsSecurityFlow)
        {
            return this.GetInstruction(pPayerReceiver, pIsSecurityFlow);
        }
        #endregion IIssiItems Members
    }
	#endregion IssiItemCollection

	#region SettlementChain
	public partial class SettlementChain : ICloneable, ISettlementChain
	{
		#region Accessors
		#region cssLinkSpecified
		public bool cssLinkSpecified
		{
			get { return (0 < cssLink); }
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
				efsSiPay.correspondentInformation = (Routing)fpmlsi.correspondentInformation.Clone();

			#endregion Payer
			#region Receiver
			EfsSettlementInstruction efsSiRec = new EfsSettlementInstruction();
			//Settlement Method
			efsSiRec.settlementMethodSpecified = fpmlsi.settlementMethodSpecified;
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
			efsSiRec.intermediaryInformationSpecified = fpmlsi.intermediaryInformationSpecified;
			if (fpmlsi.intermediaryInformationSpecified)
				efsSiRec.intermediaryInformation = (IntermediaryInformation[])fpmlsi.intermediaryInformation.Clone();
			//
			// beneficiary is mandatory
			efsSiRec.beneficiary = (Routing)fpmlsi.beneficiary.Clone();
			//
			#endregion
			//
			this[PayerReceiverEnum.Payer].settlementInstructions = efsSiPay;
			this[PayerReceiverEnum.Receiver].settlementInstructions = efsSiRec;
		}
		#endregion SetSettlementInstruction
		#endregion Methods

		#region ICloneable Members
		public object Clone()
		{
			SettlementChain clone = new SettlementChain();
			clone = (SettlementChain)ReflectionTools.Clone(this, ReflectionTools.CloneStyle.CloneFieldAndProperty);
			return clone;
		}
		#endregion ICloneable Members
		#region ISettlementChain Members
		ISettlementChainItem[] ISettlementChain.settlementChainItem
		{
			get {return this.settlementChainItem; }
		}
		bool ISettlementChain.cssLinkSpecified
		{
			get { return this.cssLinkSpecified; }
		}
		int ISettlementChain.cssLink
		{
			set {this.cssLink = value;}
			get {return this.cssLink;}
		}
		ISettlementChainItem ISettlementChain.this[PayerReceiverEnum pPayerReceiver]
		{
			get {return this[pPayerReceiver];}
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
			settlementInstructions = new EfsSettlementInstruction();
			siMode = SiModeEnum.Undefined;
			//idIssi = 0;
		}
		#endregion Constructors

		#region ISettlementChainItem Members
		SiModeEnum ISettlementChainItem.siMode
		{
			set { this.siMode = value; }
			get { return this.siMode; }
		}
		int ISettlementChainItem.idIssi
		{
			get { return this.idIssi; }
		}
		IEfsSettlementInstruction ISettlementChainItem.settlementInstruction
		{
			set { this.settlementInstructions = (EfsSettlementInstruction)value; }
			get { return this.settlementInstructions; }
		}
		int ISettlementChainItem.idASettlementOffice
		{
			set { this.idASettlementOffice = value; }
			get { return this.idASettlementOffice; }
		}
		#endregion ISettlementChainItem Members
	}
	#endregion SettlementChainItem

}


