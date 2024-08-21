#region Using Directives
using System;
using System.Collections;
using System.Reflection;

using EFS.ACommon;
using EFS.Common;
using EFS.GUI.Interface;

using EfsML.Enum;
using EfsML.Extended;

using FpML.Doc;
using FpML.Ird;
using FpML.Fx;
using FpML.Eqs;
using FpML.Eqd;
using FpML.EqShared;
using FpML.Shared;
#endregion Using Directives

namespace EfsML.EventMatrix
{
	#region EFS_Event
	public class EFS_Event
	{
		#region Variables
		#region Event
		#region EventReference
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool eventReferenceSpecified;
		public string eventReference;
		#endregion EventReference
		#region EventKey
		public EFS_EventKey eventKey;
		#endregion EventKey
		#region Periods
		public EFS_EventDate startDate;
		public EFS_EventDate endDate;
		#endregion Periods
		#region Payer/Receiver
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool payerSpecified;
		public string payer;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool receiverSpecified;
		public string receiver;
		#endregion Payer/Receiver
		#region Valorisation/Unit
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool valorisationSpecified;
		public EFS_Decimal valorisation;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool unitTypeSpecified;
		public EFS_UnitTypeEnum unitType;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool unitSpecified;
		public string unit;
		#endregion Valorisation/Unit
		#region IdStCalcul
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public StatusCalculEnum idStCalcul;
		#endregion IdStCalcul
		#endregion Event
		#region EventAsset / EventDet
		[System.Xml.Serialization.XmlElementAttribute("eventDetail")]
		public EFS_EventDetail eventDetail;
		#endregion EventAsset / EventDet
		#region EventClass
		[System.Xml.Serialization.XmlElementAttribute("subEvent")]
		public EFS_EventClass[] eventClass;
		#endregion Event
		#region Others
		[System.Xml.Serialization.XmlElementAttribute("productId")]
		public int productId;
		[System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, DataType="ID")]
		public string id;
		[System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, DataType="normalizedString")]
		public string name;
		#endregion Others
		#endregion Variables
		#region Constructors
		public EFS_Event(){}
		public EFS_Event(EFS_EventKey pEventKey,EFS_EventMatrixGroupKey pEventGroupKeyParent,EFS_EventMatrixGroupKey pEventGroupKey)
		{
			if (null != pEventGroupKeyParent)
				this.eventReference = pEventGroupKeyParent.result;

			if (null != pEventGroupKey)
			{
				this.id   = pEventGroupKey.result;
				this.name = pEventGroupKey.name;
			}
			eventReferenceSpecified = (null != pEventGroupKeyParent);
			eventKey                = (EFS_EventKey) pEventKey.Clone();
		}
		
		#endregion Constructors
	}
	#endregion EFS_Event 
	#region EFS_EventDetail
	public class EFS_EventDetail
	{
		#region Variables
		#region Assets
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool assetSpecified;
		public EFS_Asset asset;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool asset2Specified;
		public EFS_Asset asset2;
		#endregion Assets
		#region Details FX
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool paymentQuoteSpecified;
		public EFS_PaymentQuote paymentQuote;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool exchangeRateSpecified;
		public EFS_ExchangeRate exchangeRate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool sideRateSpecified;
		public SideRate sideRate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool fixingRateSpecified;
		public EFS_FxFixing fixingRate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool settlementRateSpecified;
		public EFS_SettlementRate settlementRate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool currencyPairSpecified;
		public QuotedCurrencyPair currencyPair;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool triggerRateSpecified;
		public EFS_TriggerRate triggerRate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool strikePriceSpecified;
		public FxStrikePrice strikePrice;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool premiumQuoteSpecified;
		public EFS_FxPremiumQuote premiumQuote;
		#endregion Details FX
		#region Details IRD
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool fixedRateSpecified;
		public EFS_Decimal fixedRate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool dayCountFractionSpecified;
		public EFS_DayCountFraction dayCountFraction;
		#endregion Details IRD
		#endregion Variables
	}
	#endregion EFS_EventDetail
	#region EFS_Events
	[System.Xml.Serialization.XmlRootAttribute("Events", IsNullable=false)]
	public class EFS_Events
	{
		[System.Xml.Serialization.XmlElementAttribute("event")]
		public EFS_Event[] events;
		private string productName;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public string ProductName {get {return productName;}}

		[System.Xml.Serialization.XmlElementAttribute("party")]
		public Party[] party;

		#region Constructors
		public EFS_Events(){}
		public EFS_Events(object pProduct,ref Cst.ErrLevel pRet)
		{
			party = (Party[])EFS_Current.tradeLibrary.Party.Clone();
			pRet = Calc(pProduct);
		}
		#endregion Constructors
		#region IsProduct
		private bool IsProduct(Type pTypeObject)
		{
			if (pTypeObject.Equals(typeof(Product)))
				return true;
			else if (null != pTypeObject.BaseType)
				return IsProduct(pTypeObject.BaseType);
			else
				return false;
		}
		#endregion IsProduct
		#region Calc
		/// <summary>
		/// Calculate events for a trade
		/// </summary>
		/// <param name="pProduct">FpML Product element</param>
		/// <returns>Integer ErrorLevel (-1 = Succes)</returns>
		public Cst.ErrLevel Calc(object pProduct)
		{
			Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
			try
			{
				productName = pProduct.GetType().Name;

				#region Global parameters
				SetParameters(EFS_Current.tradeLibrary,EFS_Current.tradeLibrary.EventMatrix);
				#endregion Global parameters
				#region Read EventMatrix
				EFS_Current.eventResults   = new ArrayList();
				EFS_EventMatrixTrade trade = EFS_Current.tradeLibrary.EventMatrix.trade;
				if (null != trade)
				{
					#region parameters
					SetParameters(EFS_Current.tradeLibrary.FpMLTrade,trade);
					#endregion parameters

					#region Group treatment (alert : recursivity)
					ret = ReadEventMatrixGroup((EFS_EventMatrixGroup)trade);
					#endregion Group treatment (alert : recursivity)

					#region Copy ArrayList to Events class
					if (0 < EFS_Current.eventResults.Count && (Cst.ErrLevel.SUCCESS == ret))
						this.events = (EFS_Event[])EFS_Current.eventResults.ToArray(EFS_Current.eventResults[0].GetType());
					#endregion Copy ArrayList to Events class
				}
				#endregion Read EventMatrix
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS_Events..Calc",ex);}
			return ret;
		}
		#endregion Calc
		#region Private functions & methods
		#region CreateEvent
		private Cst.ErrLevel CreateEvent(object pObject,EFS_EventMatrixItem pEventMatrixItem,EFS_EventMatrixGroupKey pEventMatrixGroupKey)
		{
			return CreateEvent(pObject,pEventMatrixItem,pEventMatrixGroupKey,null);
		}
		private Cst.ErrLevel CreateEvent(object pObject,EFS_EventMatrixItem pEventMatrixItem,
			EFS_EventMatrixGroupKey pEventMatrixGroupKeyParent,EFS_EventMatrixGroupKey pEventMatrixGroupKey)
		{
			Cst.ErrLevel ret = Cst.ErrLevel.MISCELLANEOUS;
			try
			{
				bool isConditional = true;
				if (null != pObject) 
				{
					#region Event parameters
					SetParameters(pObject,pEventMatrixItem);
					#endregion Event parameters
					#region Event Conditional
					if (pEventMatrixItem.conditionalSpecified)
						isConditional = Convert.ToBoolean(SetEvent(pObject,pEventMatrixItem,"conditional"));
					#endregion Event Conditional

					if ((null != pEventMatrixItem.key) && isConditional)
					{
						#region Event
						EFS_EventKey efs_eventKey = new EFS_EventKey();
						efs_eventKey.eventCode = (string) SetEvent(pObject,pEventMatrixItem.key,"eventCode");
						efs_eventKey.eventType = (string) SetEvent(pObject,pEventMatrixItem.key,"eventType");
						#endregion Event

						EFS_Event efs_event = new EFS_Event(efs_eventKey,pEventMatrixGroupKeyParent,pEventMatrixGroupKey);

						#region Start & End Periods dates
						efs_event.startDate = (EFS_EventDate) SetEvent(pObject,pEventMatrixItem,"startDate");
						efs_event.endDate   = (EFS_EventDate) SetEvent(pObject,pEventMatrixItem,"endDate");
						#endregion Start & End Periods dates
						#region EventClass
						if (null != pEventMatrixItem.subItem)
							CreateEventClass(pObject,efs_event,pEventMatrixItem.subItem);
						#endregion EventClass
						#region Payer & Receiver parties
						if (pEventMatrixItem.payerSpecified)
							efs_event.payer = (string) SetEvent(pObject,pEventMatrixItem,"payer");
						efs_event.payerSpecified = (null != efs_event.payer);
						if (pEventMatrixItem.receiverSpecified)
							efs_event.receiver = (string) SetEvent(pObject,pEventMatrixItem,"receiver");
						efs_event.receiverSpecified = (null != efs_event.receiver);
						#endregion Payer & Receiver parties
						#region Valorisation
						if (pEventMatrixItem.valorisationSpecified)
							efs_event.valorisation = (EFS_Decimal) SetEvent(pObject,pEventMatrixItem,"valorisation");
						efs_event.valorisationSpecified = (null != efs_event.valorisation);
						#endregion Valorisation
						#region UnitType
						efs_event.unitType = EFS_UnitTypeEnum.None;
						if (pEventMatrixItem.unitTypeSpecified)
						{
							object result = SetEvent(pObject,pEventMatrixItem,"unitType");
							if (System.Enum.IsDefined(typeof(EFS_UnitTypeEnum),result.ToString()))
								efs_event.unitType = (EFS_UnitTypeEnum)System.Enum.Parse(typeof(EFS_UnitTypeEnum),result.ToString(),true); 
						}
						efs_event.unitTypeSpecified = (EFS_UnitTypeEnum.None != efs_event.unitType);
						#endregion UnitType
						#region Unit
						if (pEventMatrixItem.unitSpecified)
							efs_event.unit = (string) SetEvent(pObject,pEventMatrixItem,"unit");
						efs_event.unitSpecified = (null != efs_event.unit);
						#endregion Unit
						#region IdStCalcul
						if (pEventMatrixItem.idStCalculSpecified)
						{
							object result = SetEvent(pObject,pEventMatrixItem,"idStCalcul");
							if (null == result)
								result = StatusCalculFunc.ToCalculate;
							efs_event.idStCalcul = (StatusCalculEnum)System.Enum.Parse(typeof(StatusCalculEnum),result.ToString(),true); 
						}
						#endregion IdStCalcul
						#region Asset & Details
						if (pEventMatrixItem.itemDetailsSpecified)
							ret = CreateEventDetails(efs_event,pObject,pEventMatrixItem.itemDetails);
						#endregion Asset & Details
						if (IsProduct(pObject.GetType()) && (null != ((Product)pObject).productType))
							efs_event.productId = ((Product)pObject).productType.OTCmlId;
						EFS_Current.eventResults.Add(efs_event);
					}
					ret = Cst.ErrLevel.SUCCESS;
				}
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS_Events..CreateEvent",ex);}
			return ret;
		}
		#endregion CreateEvent
		#region CreateEventClass
		private Cst.ErrLevel CreateEventClass(object pObject,EFS_Event pEfs_Event,EFS_EventMatrixSubItem[] pEventMatrixSubItem)
		{
			Cst.ErrLevel ret = Cst.ErrLevel.MISCELLANEOUS;
			try
			{
				bool isConditional    = true;
				ArrayList aEventClass = new ArrayList();
				EFS_Date eventDate;
				EFS_EventMatrixSubItem subItem;
				string eventClass;
				for (int i=0;i < pEventMatrixSubItem.Length;i++)
				{
					subItem = pEventMatrixSubItem[i];
					#region SubItem Conditional
					isConditional = true;
					if (subItem.conditionalSpecified)
						isConditional = Convert.ToBoolean(SetEvent(pObject,subItem,"conditional"));
					#endregion SubItem Conditional
					if (isConditional)
					{
						#region EventDate
						eventDate = (EFS_Date) SetEvent(pObject,subItem,"eventDate");
						#endregion EventDate
						#region EventClass
						eventClass = (string) SetEvent(pObject,subItem,"eventClass");
						#endregion EventClass
						#region IsPayment
						bool isPayment = subItem.isPaymentSpecified;
						if (isPayment)
							isPayment = Convert.ToBoolean(SetEvent(pObject,subItem,"isPayment"));
						#endregion IsPayment
						EFS_EventClass efs_EventClass     = new EFS_EventClass(eventClass,eventDate,isPayment);
						efs_EventClass.eventDateSpecified = (null != eventDate);
						aEventClass.Add(efs_EventClass);
					}
				}
				if (0 < aEventClass.Count)
					pEfs_Event.eventClass = (EFS_EventClass[]) aEventClass.ToArray(typeof(EFS_EventClass));
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS_Events..CreateEventClass",ex);}
			return ret;
		}
		#endregion CreateEventClass
		#region CreateEventOccurrence
		private Cst.ErrLevel CreateEventOccurrence(object pObject,EFS_EventMatrixItem pEventMatrixItem,
			EFS_EventMatrixGroupKey pEventMatrixGroupKey)
		{
			Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
			try
			{
				object baseObject = pObject;
				if (pEventMatrixItem.occurenceSpecified)
				{
					EFS_EventMatrixOccurence occurence = pEventMatrixItem.occurence;
					if (occurence.fieldSpecified)
					{
						FieldInfo fld = null;
						string[] fields = occurence.field.Split(new char[]{'/'});
						for (int i=0;i<fields.Length;i++)
						{
							fld = baseObject.GetType().GetField(fields[i]);
							if (null != fld)
								baseObject = fld.GetValue(baseObject);
							if ((null == fld) || (null == baseObject))
								break;
						}
					}

					if (null != baseObject)
					{
						if (baseObject.GetType().IsArray)
						{
							int start;
							int end;
							ret = GetSequenceOccurence(baseObject,pEventMatrixItem,out start,out end);
							if (Cst.ErrLevel.SUCCESS == ret)
							{
								for (int i=start;i<end;i++)
									ret = CreateEvent(((Array)baseObject).GetValue(i),pEventMatrixItem,pEventMatrixGroupKey);
							}
						}
						else
							ret = CreateEvent(baseObject,pEventMatrixItem,pEventMatrixGroupKey);
					}
				}
				else
				{
					ret = Cst.ErrLevel.MISCELLANEOUS;
					throw new OTCmlException("EFS_Events..CreateEventOccurence","No EventOccurence");
				}
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS_Events..CreateEventOccurrence",ex);}
			return ret;
		}
		#endregion CreateEventOccurrence
		#region CreateEventDetails
		private Cst.ErrLevel CreateEventDetails(EFS_Event pEfs_event,object pObject,EFS_EventMatrixItemDetails pEventMatrixItemDetails)
		{
			Cst.ErrLevel ret = Cst.ErrLevel.MISCELLANEOUS;
			try
			{
				pEfs_event.eventDetail      = new EFS_EventDetail();
				EFS_EventDetail eventDetail = pEfs_event.eventDetail;
				#region Assets
				#region IdAsset
				if (pEventMatrixItemDetails.assetSpecified)
					eventDetail.asset = (EFS_Asset) SetEvent(pObject,pEventMatrixItemDetails,"asset");
				eventDetail.assetSpecified = (null != eventDetail.asset);
				#endregion IdAsset
				#region IdAsset2
				if (pEventMatrixItemDetails.asset2Specified)
					eventDetail.asset2 = (EFS_Asset) SetEvent(pObject,pEventMatrixItemDetails,"asset2");
				eventDetail.asset2Specified = (null != eventDetail.asset2);
				#endregion IdAsset
				#endregion Assets
				#region FX Details
				#region ExchangeRate
				if (pEventMatrixItemDetails.exchangeRateSpecified)
					eventDetail.exchangeRate = (EFS_ExchangeRate) SetEvent(pObject,pEventMatrixItemDetails,"exchangeRate");
				eventDetail.exchangeRateSpecified = (null != eventDetail.exchangeRate);
				#endregion ExchangeRate
				#region PaymentQuote
				if (pEventMatrixItemDetails.paymentQuoteSpecified)
					eventDetail.paymentQuote = (EFS_PaymentQuote) SetEvent(pObject,pEventMatrixItemDetails,"paymentQuote");
				eventDetail.paymentQuoteSpecified = (null != eventDetail.paymentQuote);
				#endregion ExchangeRate
				#region SideRate
				if (pEventMatrixItemDetails.sideRateSpecified)
					eventDetail.sideRate = (SideRate) SetEvent(pObject,pEventMatrixItemDetails,"sideRate");
				eventDetail.sideRateSpecified = (null != eventDetail.sideRate);
				#endregion SideRate
				#region FixingRate
				if (pEventMatrixItemDetails.fixingRateSpecified)
					eventDetail.fixingRate = (EFS_FxFixing) SetEvent(pObject,pEventMatrixItemDetails,"fixingRate");
				eventDetail.fixingRateSpecified = (null != eventDetail.fixingRate);
				#endregion FixingRate
				#region SettlementRate
				if (pEventMatrixItemDetails.settlementRateSpecified)
					eventDetail.settlementRate = (EFS_SettlementRate) SetEvent(pObject,pEventMatrixItemDetails,"settlementRate");
				eventDetail.settlementRateSpecified = (null != eventDetail.settlementRate);
				#endregion SettlementRate
				#region CurrencyPair
				if (pEventMatrixItemDetails.currencyPairSpecified)
					eventDetail.currencyPair = (QuotedCurrencyPair) SetEvent(pObject,pEventMatrixItemDetails,"currencyPair");
				eventDetail.currencyPairSpecified = (null != eventDetail.currencyPair);
				#endregion CurrencyPair
				#region TriggerRate
				if (pEventMatrixItemDetails.triggerRateSpecified)
					eventDetail.triggerRate = (EFS_TriggerRate) SetEvent(pObject,pEventMatrixItemDetails,"triggerRate");
				eventDetail.triggerRateSpecified = (null != eventDetail.triggerRate);
				#endregion TriggerRate
				#region StrikePrice
				if (pEventMatrixItemDetails.strikePriceSpecified)
					eventDetail.strikePrice = (FxStrikePrice) SetEvent(pObject,pEventMatrixItemDetails,"strikePrice");
				eventDetail.strikePriceSpecified = (null != eventDetail.strikePrice);
				#endregion StrikePrice
				#region PremiumQuote
				if (pEventMatrixItemDetails.premiumQuoteSpecified)
					eventDetail.premiumQuote = (EFS_FxPremiumQuote) SetEvent(pObject,pEventMatrixItemDetails,"premiumQuote");
				eventDetail.premiumQuoteSpecified = (null != eventDetail.premiumQuote);
				#endregion PremiumQuote
				#endregion FX Details
				#region IRD Details
				#region FixedRate
				if (pEventMatrixItemDetails.fixedRateSpecified)
					eventDetail.fixedRate = (EFS_Decimal) SetEvent(pObject,pEventMatrixItemDetails,"fixedRate");
				eventDetail.fixedRateSpecified = (null != eventDetail.fixedRate);
				#endregion FixedRate
				#region DayCountFraction
				if (pEventMatrixItemDetails.dayCountFractionSpecified)
					eventDetail.dayCountFraction = (EFS_DayCountFraction) SetEvent(pObject,pEventMatrixItemDetails,"dayCountFraction");
				eventDetail.dayCountFractionSpecified = (null != eventDetail.dayCountFraction);
				#endregion DayCountFraction
				#endregion IRD Details
				ret = Cst.ErrLevel.SUCCESS;
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS_Events..CreateEventDetails",ex);}
			return ret;
		}
		#endregion CreateEventDetails
		#region GetSequenceOccurence
		private Cst.ErrLevel GetSequenceOccurence(object pObject,EFS_EventMatrixItem pEventMatrixItem,
			out int opStartIndex,out int opEndIndex)
		{
			Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
			try
			{
				int start = 0;
				int end = 0;
				if (pObject.GetType().IsArray)
				{
					end = ((Array)pObject).Length;
					if (pEventMatrixItem.occurenceSpecified)
					{
						EFS_EventMatrixOccurence occurence = pEventMatrixItem.occurence;
						EventOccurenceEnum occurenceValue = (EventOccurenceEnum)System.Enum.Parse(typeof(EventOccurenceEnum),occurence.to,true);
						switch (occurenceValue)
						{
							case EventOccurenceEnum.All:
								break;
							case EventOccurenceEnum.AllExceptFirst:
								start++;
								break;
							case EventOccurenceEnum.AllExceptFirstAndLast:
								start++;
								end--;
								break;
							case EventOccurenceEnum.AllExceptLast:
								end--;
								break;
							case EventOccurenceEnum.First:
							case EventOccurenceEnum.Unique:
								end=start+1;
								break;
							case EventOccurenceEnum.Last:
								start=end-1;
								break;
							case EventOccurenceEnum.None:
								end=start;
								break;
							case EventOccurenceEnum.Item:
								if (occurence.positionSpecified)
									start=Convert.ToInt32(occurence.position)-1;
								end=start+1;
								break;

						}
					}
				}
				opStartIndex = start;
				opEndIndex   = end;
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS_Events..GetSequenceOccurrence",ex);}
			return ret;
		}
		#endregion GetSequenceOccurence
		#region MemberType
		private EFS_FieldMemberEnum MemberType(EFS_EventMatrixField pEventMatrixField)
		{
			try
			{
				string fieldValue = pEventMatrixField.Value;
				if (null != fieldValue)
				{
					if (("0" == fieldValue) || ("1" == fieldValue))
						return EFS_FieldMemberEnum.Boolean;
					else if (fieldValue.StartsWith("[") && fieldValue.EndsWith("]"))
						return EFS_FieldMemberEnum.Constant;
					else if (-1 < fieldValue.IndexOf("|"))
					{
						if (fieldValue.EndsWith(")"))
							return EFS_FieldMemberEnum.StaticMethod;
						else
							return EFS_FieldMemberEnum.StaticProperty;
					}
					else if (fieldValue.EndsWith(")"))
						return EFS_FieldMemberEnum.Method;
					else
						return EFS_FieldMemberEnum.Property;
				}
				return EFS_FieldMemberEnum.Unknown;
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS_Events..MemberType",ex);}
		}
		#endregion MemberType
		#region ReadEventMatrixGroup
		/// <summary>Calculate events GROUP informative</summary>
		/// <param name="pEventMatrixGroup">EventMatrixGroup element</param>
		/// <returns>Integer ErrorLevel (-1 = Succes)</returns>
 
		private Cst.ErrLevel ReadEventMatrixGroup(EFS_EventMatrixGroup pEventMatrixGroup)
		{
			return ReadEventMatrixGroup(EFS_Current.tradeLibrary.FpMLDocument,pEventMatrixGroup);
		}
		private Cst.ErrLevel  ReadEventMatrixGroup(object pObject,EFS_EventMatrixGroup pEventMatrixGroup)
		{
			return ReadEventMatrixGroup(pObject,pEventMatrixGroup,null);
		}
		private Cst.ErrLevel ReadEventMatrixGroup(object pObject,EFS_EventMatrixGroup pEventMatrixGroup,
			EFS_EventMatrixGroupKey pKeyGroupParent)
		{
			Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
			try
			{
				EFS_EventMatrixItem itemGroup = pEventMatrixGroup.itemGroup;

				if (itemGroup.occurenceSpecified)
				{
					#region private variables
					bool IsUnique = (itemGroup.occurence.to == EventOccurenceEnum.Unique.ToString());
					bool isObjectSpecified = true;
					bool isFieldSpecified  = true;
					ArrayList aObjectGroup = new ArrayList();
					object objectGroup = null;
					#endregion private variables
					#region Return if exist value of "occurence + Cst.FpML_SerializeKeySpecified"
					if (itemGroup.occurence.isOptional)
					{
						string fieldSpecified = itemGroup.occurence.Value + Cst.FpML_SerializeKeySpecified;
						ArrayList aIsSpecified = ReflectionTools.GetObjectByName(pObject,fieldSpecified,true);
						isObjectSpecified = (0 == aIsSpecified.Count) || Boolean.Equals(isObjectSpecified,aIsSpecified[0]);
					}
					#endregion Return if exist value of "occurence + Cst.FpML_SerializeKeySpecified"
					#region Get object
					if (isObjectSpecified)
						aObjectGroup = ReflectionTools.GetObjectByName(pObject,itemGroup.occurence.Value,IsUnique);
					#endregion Get object
					#region Loop in object occurence
					for (int i=0;i<aObjectGroup.Count;i++)
					{
						objectGroup = aObjectGroup[i];
						FieldInfo fld = null;
						#region FieldSpecified
						if (itemGroup.occurence.fieldSpecified)
						{
							string[] fields = itemGroup.occurence.field.Split(new char[]{'/'});
							for (int j=0;j<fields.Length;j++)
							{
								fld = objectGroup.GetType().GetField(fields[j]);
								if (null != fld)
								{
									if (itemGroup.occurence.isFieldOptional)
									{
										string fieldSpecified  = fields[j] + Cst.FpML_SerializeKeySpecified;
										ArrayList aIsSpecified = ReflectionTools.GetObjectByName(objectGroup,fieldSpecified,true);
										isFieldSpecified = (0 == aIsSpecified.Count) || Boolean.Equals(isObjectSpecified,aIsSpecified[0]);
									}
									objectGroup = fld.GetValue(objectGroup);
								}

								if ((null == fld) || (null == objectGroup))
								{
									if (isFieldSpecified)
										ret = Cst.ErrLevel.MISCELLANEOUS;
									break;
								}
							}
						}
						#endregion FieldSpecified

						if ((null != objectGroup) && (Cst.ErrLevel.MISCELLANEOUS != ret))
						{
							int idGroup=1;
							if (objectGroup.GetType().IsArray)
							{
								int start;
								int end;
								ret = GetSequenceOccurence(objectGroup,pEventMatrixGroup.itemGroup,out start,out end);
								if (Cst.ErrLevel.SUCCESS == ret)
								{
									idGroup = start + 1;
									for (int j=start;j<end;j++)
									{
										object obj = ((Array)objectGroup).GetValue(j);
										ret = TrtEventMatrixGroup(obj,pEventMatrixGroup,pKeyGroupParent,idGroup);
										idGroup++;
									}
									//if (end <= start)
									//	RemoveEventMatrixGroup(pEventMatrixGroup);
								}
							}
							else
							{
								ret = TrtEventMatrixGroup(objectGroup,pEventMatrixGroup,pKeyGroupParent,idGroup);
							}
						}
					}
					#endregion Loop in object occurence
				}
				else
				{
					ret = TrtEventMatrixGroup(pObject,pEventMatrixGroup,pKeyGroupParent,1);
				}
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS_Events..ReadEventMatrixGroup",ex);}
			return ret;
		}
		#endregion ReadEventMatrixGroup
		#region ReadEventMatrixItems
		private Cst.ErrLevel ReadEventMatrixItems(object pObject,EFS_EventMatrixGroup pEventMatrixGroup)
		{
			Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
			try
			{
				if (null != pObject)
				{
					EFS_EventMatrixItem[] items = pEventMatrixGroup.item;
					EFS_EventMatrixGroupKey keyGroup = pEventMatrixGroup.keyGroup;
					if (null != items)
					{
						foreach (EFS_EventMatrixItem item in items)
						{
							if (item.occurenceSpecified)
							{
								#region private variables
								bool isObjectSpecified = true;
								ArrayList aObjectOccurence = new ArrayList();
								bool IsUnique = (item.occurence.to == EventOccurenceEnum.Unique.ToString());
								#endregion private variables

								#region Return if exist value of "occurence + Cst.FpML_SerializeKeySpecified"
								if (item.occurence.isOptional)
								{
									string fieldSpecified  = item.occurence.Value + Cst.FpML_SerializeKeySpecified;
									ArrayList aIsSpecified = ReflectionTools.GetObjectByName(pObject,fieldSpecified,true);
									isObjectSpecified = (0 == aIsSpecified.Count) || Boolean.Equals(isObjectSpecified,aIsSpecified[0]);
								}
								#endregion Return if exist value of "occurence + Cst.FpML_SerializeKeySpecified"
								if (isObjectSpecified)
									aObjectOccurence = ReflectionTools.GetObjectByName(pObject,item.occurence.Value,IsUnique);

								for (int i=0;i<aObjectOccurence.Count;i++)
									ret = CreateEventOccurrence(aObjectOccurence[i],item,keyGroup);
							}
							else
								ret = CreateEvent(pObject,item,keyGroup);
						}
					}
				}
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS_Events..ReadEventMatrixItems",ex);}
			return ret;
		}
		#endregion ReadEventMatrixItems
		#region ReadEventMatrixSubGroup
		private Cst.ErrLevel ReadEventMatrixSubGroup(object pObject,EFS_EventMatrixGroup pEventMatrixGroup,
			EFS_EventMatrixGroupKey pKeyGroupParent)
		{
			Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
			try
			{
				Type tEventMatrixGroup = pEventMatrixGroup.GetType();
				FieldInfo fld = tEventMatrixGroup.GetField("group");
				if (null != fld)
				{
					object obj = fld.GetValue(pEventMatrixGroup);
					if (null!= obj)
					{
						Type tObj = obj.GetType();
						if (tObj.IsArray)

							foreach (EFS_EventMatrixGroup subEventMatrixGroup in (EFS_EventMatrixGroup[])obj)
								ret = ReadEventMatrixGroup(pObject,subEventMatrixGroup,pKeyGroupParent);
						else
						{
							if (obj.GetType().BaseType.Equals(typeof(EFS_EventMatrixProduct)))
								if (productName == ((EFS_EventMatrixProduct)obj).productName)
									ret = ReadEventMatrixGroup(pObject,(EFS_EventMatrixGroup)obj,pKeyGroupParent);
						}
					}
				}
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS_Events..ReadEventMatrixSubGroup",ex);}
			return ret;
		}
		#endregion ReadEventMatrixSubGroup
		#region RemoveEventMatrixGroup
		private void RemoveEventMatrixGroup(EFS_EventMatrixGroup pEventMatrixGroup)
		{
			try
			{
				Type t = pEventMatrixGroup.GetType();
				if (null != pEventMatrixGroup.parameters)
					pEventMatrixGroup.parameters = null;

				FieldInfo fld = t.GetField("group");
				if (null != fld)
					fld.SetValue(pEventMatrixGroup,null);
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS_Events..RemoveEventMatrixGroup",ex);}
		}

		#endregion RemoveEventMatrixGroup
		#region SetEvent
		private object SetEvent(object pObject,object pEvent,string pEventField)
		{
			try
			{
				object result = null;
				Type tEvent = pEvent.GetType();
				FieldInfo fld  = tEvent.GetField(pEventField);
				if (null != fld)
				{
					object obj = fld.GetValue(pEvent);
					if (null !=obj)
					{
						if (obj.GetType().Equals(typeof(EFS_EventMatrixField)))
						{
							EFS_FieldMemberEnum fieldMemberEnum = MemberType((EFS_EventMatrixField)obj);
							switch (fieldMemberEnum)
							{
								case EFS_FieldMemberEnum.Boolean:
									#region Boolean
									result = BoolFunc.BoolValue(((EFS_EventMatrixField)obj).Value);
									#endregion Boolean
									break;
								case EFS_FieldMemberEnum.Property:
									#region Property
									result = SetEventByProperty(pObject,(EFS_EventMatrixField)obj);
									#endregion Property
									break;
								case EFS_FieldMemberEnum.Method:
									#region Method
									if (pEvent.GetType().Equals(typeof(EFS_EventMatrixItem)) ||
										pEvent.GetType().BaseType.Equals(typeof(EFS_EventMatrixItemDetails)))
										result = SetEventByMethod(pObject,(EFS_EventMatrixField)obj,pEvent);
									else
										result = SetEventByMethod(pObject,(EFS_EventMatrixField)obj, null);
									#endregion Method
									break;
								case EFS_FieldMemberEnum.Constant:
									#region Constant
									result = SetEventByConstant((EFS_EventMatrixField)obj);
									#endregion Constant
									break;
								case EFS_FieldMemberEnum.StaticMethod:
									#region Static Method
									if (pEvent.GetType().Equals(typeof(EFS_EventMatrixItem)) ||
										pEvent.GetType().BaseType.Equals(typeof(EFS_EventMatrixItemDetails)))
										result = SetEventByStaticMethod(pObject,(EFS_EventMatrixField)obj,pEvent);
									else
										result = SetEventByStaticMethod(pObject,(EFS_EventMatrixField)obj, null);
									#endregion Static Method
									break;
								case EFS_FieldMemberEnum.StaticProperty:
									#region Static Property
									result = SetEventByStaticProperty((EFS_EventMatrixField)obj);
									#endregion Static Property
									break;
								default:
									break;
							}
						}
						else if (obj.GetType().Equals(typeof(EFS_EventMatrixFieldReference)))
						{
							#region Parameter
							result = SetEventByReference((EFS_EventMatrixFieldReference)obj);
							#endregion Parameter
						}
					}
				}
				return result;
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS_Events..SetEvent",ex);}
		}
		#endregion SetEvent
		#region SetEventByConstant
		private object SetEventByConstant(EFS_EventMatrixField pEventMatrixField)
		{
			try
			{
				return pEventMatrixField.Value.Substring(1,pEventMatrixField.Value.Length-2);
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS_Events..SetEventByConstant",ex);}
		}
		#endregion SetEventByConstant
		#region SetEventByMethod
		private object SetEventByMethod(object pObject,EFS_EventMatrixField pEventMatrixField,object pEventMatrixItemProduct)
		{
			try
			{
				object result = null;
				Type tObject = pObject.GetType();
				if (tObject.IsArray)
					tObject = tObject.GetElementType();

				string[] sTemp = pEventMatrixField.Value.Split("(".ToCharArray());
				string func = sTemp[0].Replace("(",string.Empty);

				MethodInfo method = tObject.GetMethod(func);
				if (null != method)
				{
					string[] parameters = sTemp[1].Replace(")",string.Empty).Split(",".ToCharArray());
					object[] argValues = new object [parameters.Length];
					for (int i=0;i<parameters.Length;i++)
					{
						if (-1 < parameters[i].IndexOf("'"))
							argValues[i] = parameters[i].Replace("'",string.Empty);
						else
						{
							object parameter = null;
							if (null != pEventMatrixItemProduct)
								parameter = ReflectionTools.GetObjectById(pEventMatrixItemProduct,parameters[i]);

							if (null == parameter)
								parameter = ReflectionTools.GetObjectById(EFS_Current.tradeLibrary.EventMatrix,parameters[i]);
							if (null != parameter)
								argValues[i] = ((EFS_EventMatrixParameter) parameter).result;
						}
					}
					result = tObject.InvokeMember(method.Name,BindingFlags.InvokeMethod,null,pObject,argValues,null,null,null);
				}
				return result;
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS_Events..SetEventByMethod",ex);}
		}
		#endregion SetEventByMethod
		#region SetEventByProperty
		private object SetEventByProperty(object pObject,EFS_EventMatrixField pEventMatrixField)
		{
			object result    = null;
			PropertyInfo pty = null;
			Type tObject     = pObject.GetType();
			try
			{
				if (tObject.IsArray)
					tObject = tObject.GetElementType();

				if (pEventMatrixField.declaringTypeSpecified)
				{
					pty = tObject.GetProperty(pEventMatrixField.declaringType);
					if (null != pty)
					{
						object target = tObject.InvokeMember(pty.Name,BindingFlags.GetProperty,null,pObject,null);
						if (null !=target)
						{
							tObject = target.GetType();
							if (tObject.IsArray)
								tObject = tObject.GetElementType();
							pty = tObject.GetProperty(pEventMatrixField.Value);
							if (null != pty)
								result = tObject.InvokeMember(pty.Name,BindingFlags.GetProperty,null,target,null);
						}
					}
				}
				else
				{
					pty = tObject.GetProperty(pEventMatrixField.Value);
					if (null != pty)
						result = tObject.InvokeMember(pty.Name,BindingFlags.GetProperty,null,pObject,null);
				}
				return result;
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) 
			{
				if ((null != pty) && (null != tObject))
					throw new OTCmlException("EFS_Events..SetEventByProperty",ex,pty.Name,tObject.Name,pEventMatrixField.Value);
				else
					throw new OTCmlException("EFS_Events..SetEventByProperty",ex);
			}
		}
		#endregion SetEventByProperty
		#region SetEventByStaticMethod
		private object SetEventByStaticMethod(object pObject,EFS_EventMatrixField pEventMatrixField,object pEventMatrixItemProduct)
		{
			try
			{
				object result             = null;
				string[] staticMethodInfo = pEventMatrixField.Value.Split('|');
				string className          = string.Empty;
				string fullMethod         = string.Empty;
				string assemblyName       = string.Empty;

				if (1 < staticMethodInfo.Length)
				{
					className = staticMethodInfo[0];
					if (2 == staticMethodInfo.Length)
					{
						assemblyName = staticMethodInfo[0].Substring(0,staticMethodInfo[0].LastIndexOf("."));
						fullMethod   = staticMethodInfo[1];
					}
					else
					{
						assemblyName = staticMethodInfo[1];
						fullMethod   = staticMethodInfo[2];
					}

					string[] sTemp    = fullMethod.Split("(".ToCharArray());
					string methodName = sTemp[0].Replace("(",string.Empty);

					Type tStaticClass = Type.GetType(className + "," + assemblyName,true,false);
					MethodInfo method = tStaticClass.GetMethod(methodName);
					if (null != method)
					{
						string[] parameters = sTemp[1].Replace(")",string.Empty).Split(",".ToCharArray());
						object[] argValues = new object [parameters.Length];
						for (int i=0;i<parameters.Length;i++)
						{
							if (-1 < parameters[i].IndexOf("'"))
								argValues[i] = parameters[i].Replace("'",string.Empty);
							else
							{
								object parameter = null;
								if (null != pEventMatrixItemProduct)
									parameter = ReflectionTools.GetObjectById(pEventMatrixItemProduct,parameters[i]);

								if (null == parameter)
									parameter = ReflectionTools.GetObjectById(EFS_Current.tradeLibrary.EventMatrix,parameters[i]);
								if (null != parameter)
									argValues[i] = ((EFS_EventMatrixParameter) parameter).result;
							}
						}
						result = tStaticClass.InvokeMember(method.Name,BindingFlags.InvokeMethod,null,null,argValues,null,null,null);
					}
				}
				return result;
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS_Events..SetEventByStaticMethod",ex);}
		}
		#endregion SetEventByStaticMethod
		#region SetEventByStaticProperty
		private object SetEventByStaticProperty(EFS_EventMatrixField pEventMatrixField)
		{
			try
			{
				object result               = null;
				string[] staticPropertyInfo = pEventMatrixField.Value.Split('|');
				string className            = string.Empty;
				string ptyName              = string.Empty;
				string assemblyName         = string.Empty;

				if (1 < staticPropertyInfo.Length)
				{
					className = staticPropertyInfo[0];
					if (2 == staticPropertyInfo.Length)
					{
						assemblyName = staticPropertyInfo[0].Substring(0,staticPropertyInfo[0].LastIndexOf("."));
						ptyName      = staticPropertyInfo[1];
					}
					else
					{
						assemblyName = staticPropertyInfo[1];
						ptyName      = staticPropertyInfo[2];
					}
					Type tStaticClass = Type.GetType(className + "," + assemblyName,true,false);
					PropertyInfo pty  = tStaticClass.GetProperty(ptyName);
					if (null != pty)
						result = tStaticClass.InvokeMember(pty.Name,BindingFlags.GetProperty,null,null,null);
				}
				return result;
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS_Events..SetEventByStaticProperty",ex);}
		}
		#endregion SetEventByStaticProperty
		#region SetEventByReference
		private object SetEventByReference(EFS_EventMatrixFieldReference pEventMatrixField)
		{
			try
			{
				object result = null;
				object parameter = ReflectionTools.GetObjectById(EFS_Current.tradeLibrary.EventMatrix,pEventMatrixField.hRef);
				if (null != parameter)
					result = ((EFS_EventMatrixParameter) parameter).result;
				return result;
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS_Events..SetEventByReference",ex);}
		}
		#endregion SetEventByReference
		#region SetParameter
		private object SetParameter(object pObject,EFS_EventMatrixParameter pEventMatrixParameter)
		{
			try
			{
				object result = null;
				EFS_FieldMemberEnum fieldMemberEnum = MemberType(pEventMatrixParameter);
				switch (fieldMemberEnum)
				{
					case EFS_FieldMemberEnum.Property:
						#region Property
						result = SetEventByProperty(pObject,(EFS_EventMatrixField)pEventMatrixParameter);
						#endregion Property
						break;
					case EFS_FieldMemberEnum.Method:
						#region Method
						result = SetEventByMethod(pObject,(EFS_EventMatrixField)pEventMatrixParameter,null);
						#endregion Method
						break;
					case EFS_FieldMemberEnum.Constant:
						#region Constant
						result = SetEventByConstant((EFS_EventMatrixField)pEventMatrixParameter);
						#endregion Constant
						break;
					default:
						break;
				}
				return result;
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS_Events..SetParameter",ex);}
		}
		#endregion SetParameter
		#region SetParameters
		private void SetParameters(object pObject,object pDeclaringParameters)
		{
			try
			{
				Type tDeclaringParameters = pDeclaringParameters.GetType();
				FieldInfo fld = tDeclaringParameters.GetField("parameters");
				if (null != fld)
				{
					EFS_EventMatrixParameter[] eventMatrixParameters = (EFS_EventMatrixParameter[])fld.GetValue(pDeclaringParameters);
					if ((null != eventMatrixParameters) && (0 != eventMatrixParameters.Length))
					{
						foreach (EFS_EventMatrixParameter eventMatrixParameter in eventMatrixParameters)
						{
							eventMatrixParameter.result = SetParameter(pObject,eventMatrixParameter);
						}
					}
				}
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS_Events..SetParameters",ex);}
		}
		#endregion SetParameters
		#region TrtEventMatrixGroup
		private Cst.ErrLevel TrtEventMatrixGroup(object pObject,EFS_EventMatrixGroup pEventMatrixGroup,
			EFS_EventMatrixGroupKey pKeyGroupParent,int pIdGroup)
		{
			Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
			try
			{
				#region EventMatrixGroup parameters
				SetParameters(pObject,pEventMatrixGroup);
				#endregion EventMatrixGroup parameters
				#region EventMatrixGroup item
				EFS_EventMatrixGroupKey keyGroup = pEventMatrixGroup.keyGroup;
				keyGroup.result = keyGroup.key + pIdGroup.ToString().PadLeft(3,'0');
				ret = CreateEvent(pObject,pEventMatrixGroup.itemGroup,pKeyGroupParent,keyGroup);
				#endregion EventMatrixGroup item
				#region Recursivity
				if (Cst.ErrLevel.SUCCESS == ret)
					ret = ReadEventMatrixSubGroup(pObject,pEventMatrixGroup,keyGroup);
				#endregion Recursivity
				#region EventMatrixGroup items
				if (Cst.ErrLevel.SUCCESS  == ret)
					ret = ReadEventMatrixItems(pObject,pEventMatrixGroup);
				#endregion EventMatrixGroup items
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS_Events..TrtEventMatrixGroup",ex);}
			return ret;
		}
		#endregion TrtEventMatrixGroup
		#endregion Private functions & methods
	}
	#endregion EFS_Events
	#region EFS_EventDate
	public class EFS_EventDate
	{
		public EFS_Date unadjustedDate;
		public EFS_Date adjustedDate;

		public EFS_EventDate()
		{
			unadjustedDate = new EFS_Date();
			adjustedDate = new EFS_Date();
		}
		public EFS_EventDate(DateTime pUnadjustedDate,DateTime pAdjustedDate)
		{
			unadjustedDate = new EFS_Date();
			unadjustedDate.DateValue = pUnadjustedDate;
			adjustedDate = new EFS_Date();
			adjustedDate.DateValue = pAdjustedDate;
		}

		public EFS_EventDate(EFS_AdjustableDate pAdjustableDate)
		{
			unadjustedDate = new EFS_Date(pAdjustableDate.adjustableDate.unadjustedDate.Value);
			if (pAdjustableDate.adjustableDateSpecified)
				adjustedDate = new EFS_Date(pAdjustableDate.adjustedDate.Value);
		}

	}
	#endregion EFS_EventDates 
	#region EFS_EventKey
	public class EFS_EventKey : ICloneable	
	{
		[System.Xml.Serialization.XmlElementAttribute("eventCode")]
		public string eventCode;
		[System.Xml.Serialization.XmlElementAttribute("eventType")]
		public string eventType;

		public EFS_EventKey(){}
		#region Membres de ICloneable
		public object Clone()
		{
			EFS_EventKey clone = new EFS_EventKey();
			clone.eventCode = this.eventCode;
			clone.eventType = this.eventType;
			return clone;
		}
		#endregion
	}
	#endregion EFS_EventKey
	#region EFS_EventClass
	public class EFS_EventClass 
	{
		[System.Xml.Serialization.XmlElementAttribute("eventClass")]
		public string eventClass;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool eventDateSpecified;
		public EFS_Date eventDate;
		public bool isPayment;

		public EFS_EventClass(){}
		public EFS_EventClass(string pEventClass,EFS_Date pEventDate, bool pIsPayment)
		{
			eventClass = pEventClass;
			if (null != pEventDate)
			{
				eventDate           = new EFS_Date();
				eventDate.DateValue = pEventDate.DateValue;
				isPayment           = pIsPayment;
			}
			eventDateSpecified = (null != eventDate);
		}
	}
	#endregion EFS_EventClass 

	#region Global Complex Types [Events Matrix]
	#region General
	#region EFS_EventMatrix
	/// <summary>
	/// <newpara><b>Description :</b> The EFS_EventMatrix element forms the root for any conforming Events referential 
	/// instance document</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>parameters (zero or one occurrence; of the type EFS_EventMatrixParameter)</newpara>
	/// <newpara>trade (exactly one occurrence; of the type EFS_EventMatrixTrade)</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	[System.Xml.Serialization.XmlRootAttribute("EventMatrix", IsNullable=false)]
	public class EFS_EventMatrix
	{
		[System.Xml.Serialization.XmlElementAttribute("parameter")]
		public EFS_EventMatrixParameter[] parameters;

		[System.Xml.Serialization.XmlElementAttribute("trade")]
		public EFS_EventMatrixTrade trade;
	}
	#endregion EFS_EventMatrix
	#region EFS_EventMatrixConditional
	public class EFS_EventMatrixConditional : EFS_EventMatrixField
	{
		[System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, DataType="ID")]
		public string id
		{
			set {efs_id=new EFS_Id(value);}
			get 
			{
				if (efs_id==null)
					return null;
				else
					return efs_id.Value;
			}
		}
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_Id efs_id;
	}
	#endregion EFS_EventMatrixConditional
	#region EFS_EventMatrixField 
	/// <summary>
	/// <newpara><b>Description :</b></newpara>
	/// <newpara><b>Contents :</b>The base element type which all items of EFS_EventsMatrixItem extend</newpara>
	/// <newpara>declaringType (zero or one occurrence; of the type xsd:string) name of EFS or FpML class who declare Value.</newpara>
	/// <newpara>Value (exactly one occurrence; of the type xsd:string) the name of method or property or constant to apply.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: EFS_EventMatrixItem</newpara>
	///<newpara>• Complex type: EFS_EventMatrixKey</newpara>
	///<newpara>• Complex type: EFS_EventMatrixSubItem</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	public class EFS_EventMatrixField
	{
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool declaringTypeSpecified;
		[System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, DataType="anyURI")]
		public string declaringType;
		[System.Xml.Serialization.XmlTextAttribute(DataType="normalizedString")]
		public string Value;
		public object result;

		public EFS_EventMatrixField(){}
	}
	#endregion EFS_EventMatrixField
	#region EFS_EventMatrixFieldReference 
	/// <summary>
	/// <newpara><b>Description :</b></newpara>
	/// <newpara><b>Contents :</b>The base element type which all items of EFS_EventsMatrixItem extend</newpara>
	/// <newpara>hRef (exactly one occurrence; of the type xsd:string) the id of parameter defined before in the document</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: EFS_EventMatrixItem</newpara>
	///<newpara>• Complex type: EFS_EventMatrixKey</newpara>
	///<newpara>• Complex type: EFS_EventMatrixSubItem</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	public class EFS_EventMatrixFieldReference
	{
		[System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, DataType="IDREF")]
		public string hRef;

		public EFS_EventMatrixFieldReference(){}
	}
	#endregion EFS_EventMatrixFieldReference
	#region EFS_EventMatrixGroup
	/// <summary>
	/// <newpara><b>Description :</b> The events definitions for a specific group.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>parameters (zero or more occurrence; of the type EFS_EventMatrixParameter)</newpara>
	/// <newpara>itemGroup (exactly one occurrence; of the type EFS_EventMatrixItem) 
	/// Define the informative event elements for this group</newpara>
	/// <newpara>item (zero or more occurrence; of the type EFS_EventMatrixItem)</newpara>
	/// <newpara>keyGroup (exactly one occurrence; of the type EFS_EventMatrixGroupKey) Define the key for this group</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: EFS_EventMatrixTrade</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///<newpara>• Complex type: EFS_EventMatrixCapFloorStream</newpara>
	///<newpara>• Complex type: EFS_EventMatrixSwapStream</newpara>
	///<newpara>• Complex type: EFS_EventMatrixProduct</newpara>
	///<newpara>• Complex type: EFS_EventMatrixTrade</newpara>
	///</remarks>
	public class EFS_EventMatrixGroup
	{
		[System.Xml.Serialization.XmlElementAttribute("parameter")]
		public EFS_EventMatrixParameter[] parameters;
		[System.Xml.Serialization.XmlElementAttribute("keyGroup")]
		public EFS_EventMatrixGroupKey keyGroup;
		[System.Xml.Serialization.XmlElementAttribute("itemGroup")]
		public EFS_EventMatrixItem itemGroup;
		[System.Xml.Serialization.XmlElementAttribute("item")]
		public EFS_EventMatrixItem[] item;

		#region constructor
		public EFS_EventMatrixGroup(){}
		#endregion constructor
	}
	#endregion EFS_EventMatrixGroup
	#region EFS_EventMatrixGroupKey
	/// <summary>
	/// <newpara><b>Description :</b> the key for a events group.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>key (zero or one occurrence; of the type xsd:string) prefix key of the group</newpara>
	/// <newpara>name (zero or one occurrence; of the type xsd:string) name of the group</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: EFS_EventMatrixGroup</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	public class EFS_EventMatrixGroupKey
	{
		[System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, DataType="normalizedString")]
		public string key;
		[System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, DataType="normalizedString")]
		public string name;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public string result;
	}
	#endregion EFS_EventMatrixGroupKey
	#region EFS_EventMatrixItem
	/// <summary>
	/// <newpara><b>Description :</b> The event matrix element</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>parameters (zero or one occurrence; of the type EFS_EventMatrixParameter)</newpara>
	/// <newpara>occurence (zero or one occurrence; of the type EFS_EventMatrixOccurence) EFS or FpML complex type element, 
	/// for where events definition will occur</newpara>
	/// <newpara>key (exactly one occurrence; of the type EFS_EventMatrixKey) the key event.</newpara>
	/// <newpara>Either</newpara> 
	/// <newpara>startDate (exactly one occurrence; of the type EFS_EventMatrixField)
	/// the element (method or property or constant) to retrieve the Start Period event.</newpara>
	/// <newpara>Or</newpara>
	/// <newpara>startDateReference (exactly one occurrence; of the type EFS_EventMatrixFieldReference)
	/// the element reference (parameter) to retrieve the Start Period event.</newpara>
	/// <newpara>Either</newpara> 
	/// <newpara>endDate (exactly one occurrence; of the type EFS_EventMatrixField)
	/// the element (method or property or constant) to retrieve the End Period event.</newpara>
	/// <newpara>Or</newpara>
	/// <newpara>endDateReference (exactly one occurrence; of the type EFS_EventMatrixFieldReference)
	/// the element reference (parameter) to retrieve the End Period event.</newpara>
	/// <newpara>Either</newpara> 
	/// <newpara>payer (zero or one occurrence; of the type EFS_EventMatrixField)
	/// the element (method or property or constant) to retrieve the payer event.</newpara>
	/// <newpara>Or</newpara>
	/// <newpara>payerReference (zero or one occurrence; of the type EFS_EventMatrixFieldReference)
	/// the element reference (parameter) to retrieve the payer event.</newpara>
	/// <newpara>Either</newpara> 
	/// <newpara>receiver (zero or one occurrence; of the type EFS_EventMatrixField)
	/// the element (method or property or constant) to retrieve the receiver event.</newpara>
	/// <newpara>Or</newpara>
	/// <newpara>receiverReference (zero or one occurrence; of the type EFS_EventMatrixFieldReference)
	/// the element reference (parameter) to retrieve the receiver event.</newpara>
	/// <newpara>Either</newpara> 
	/// <newpara>valorisation (zero or one occurrence; of the type EFS_EventMatrixField)
	/// the element (method or property or constant) to retrieve the valorisation event.</newpara>
	/// <newpara>Or</newpara>
	/// <newpara>valorisationReference (zero or one occurrence; of the type EFS_EventMatrixFieldReference)
	/// the element reference (parameter) to retrieve the valorisation event.</newpara>
	/// <newpara>Either</newpara> 
	/// <newpara>unit (zero or one occurrence; of the type EFS_EventMatrixField)
	/// the element (method or property or constant) to retrieve the unit event.</newpara>
	/// <newpara>Or</newpara>
	/// <newpara>unitReference (zero or one occurrence; of the type EFS_EventMatrixFieldReference)
	/// the element reference (parameter) to retrieve the unit event.</newpara>
	/// <newpara>Either</newpara> 
	/// <newpara>subItem (one or more occurrence; of the type EFS_EventMatrixSubItem) the sub-event element definition.
	/// </newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: EFS_EventMatrixGroup</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	public class EFS_EventMatrixItem
	{
		#region Members
		#region Event
		[System.Xml.Serialization.XmlElementAttribute("startDate", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("startDateReference", typeof(EFS_EventMatrixFieldReference))]
		public object startDate;
		[System.Xml.Serialization.XmlElementAttribute("endDate", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("endDateReference", typeof(EFS_EventMatrixFieldReference))]
		public object endDate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool payerSpecified;
		[System.Xml.Serialization.XmlElementAttribute("payer", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("payerReference", typeof(EFS_EventMatrixFieldReference))]
		public object payer;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool receiverSpecified;
		[System.Xml.Serialization.XmlElementAttribute("receiver", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("receiverReference", typeof(EFS_EventMatrixFieldReference))]
		public object receiver;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool valorisationSpecified;
		[System.Xml.Serialization.XmlElementAttribute("valorisation", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("valorisationReference", typeof(EFS_EventMatrixFieldReference))]
		public object valorisation;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool unitTypeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("unitType", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("unitTypeReference", typeof(EFS_EventMatrixFieldReference))]
		public object unitType;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool unitSpecified;
		[System.Xml.Serialization.XmlElementAttribute("unit", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("unitReference", typeof(EFS_EventMatrixFieldReference))]
		public object unit;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool idStCalculSpecified;
		[System.Xml.Serialization.XmlElementAttribute("idStCalcul", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("idStCalculReference", typeof(EFS_EventMatrixFieldReference))]
		public object idStCalcul;
		#endregion Event
		#region EventAsset & EventDet
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool itemDetailsSpecified;
		[System.Xml.Serialization.XmlElementAttribute("item_Details")]
		public EFS_EventMatrixItemDetails itemDetails;
		#endregion EventAsset & EventDet
		#region EventClass
		[System.Xml.Serialization.XmlElementAttribute("subItem")]
		public EFS_EventMatrixSubItem[] subItem;
		[System.Xml.Serialization.XmlElementAttribute("subItems")]
		public EFS_EventMatrixSubItems[] subItems;

		#endregion EventClass
		#region Others
		[System.Xml.Serialization.XmlElementAttribute("parameter")]
		public EFS_EventMatrixParameter[] parameters;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool occurenceSpecified;
		public EFS_EventMatrixOccurence occurence;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool conditionalSpecified;
		[System.Xml.Serialization.XmlElementAttribute("conditional", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("conditionalReference", typeof(EFS_EventMatrixFieldReference))]
		public object conditional;

		[System.Xml.Serialization.XmlElementAttribute("key")]
		public EFS_EventMatrixKey key;
		#endregion Others
		#endregion Members
		#region Indexors
		public EFS_EventMatrixParameter this[string pId]
		{
			get 
			{
				for (int i=0;i<parameters.Length;i++)
				{
					if (pId == parameters[i].id)
						return parameters[i];
				}
				return null;
			}
		}
		#endregion Indexors
		#region Constructors
		public EFS_EventMatrixItem(){}
		#endregion Constructors
	}
	#endregion EFS_EventMatrixItem
	#region EFS_EventMatrixItemDetails
	public class EFS_EventMatrixItemDetails
	{
		#region Variables
		#region Assets
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool assetSpecified;
		[System.Xml.Serialization.XmlElementAttribute("asset", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("assetReference", typeof(EFS_EventMatrixFieldReference))]
		public object asset;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool asset2Specified;
		[System.Xml.Serialization.XmlElementAttribute("asset2", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("asset2Reference", typeof(EFS_EventMatrixFieldReference))]
		public object asset2;
		#endregion Assets
		#region FX Details
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool paymentQuoteSpecified;
		[System.Xml.Serialization.XmlElementAttribute("paymentQuote", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("paymentQuoteReference", typeof(EFS_EventMatrixFieldReference))]
		public object paymentQuote;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool exchangeRateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("exchangeRate", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("exchangeRateReference", typeof(EFS_EventMatrixFieldReference))]
		public object exchangeRate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool sideRateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("sideRate", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("sideRateReference", typeof(EFS_EventMatrixFieldReference))]
		public object sideRate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool fixingRateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("fixingRate", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("fixingReference", typeof(EFS_EventMatrixFieldReference))]
		public object fixingRate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool settlementRateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("settlementRate", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("settlementRateReference", typeof(EFS_EventMatrixFieldReference))]
		public object settlementRate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool currencyPairSpecified;
		[System.Xml.Serialization.XmlElementAttribute("currencyPair", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("currencyPairReference", typeof(EFS_EventMatrixFieldReference))]
		public object currencyPair;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool triggerRateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("triggerRate", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("triggerRateReference", typeof(EFS_EventMatrixFieldReference))]
		public object triggerRate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool strikePriceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("strikePrice", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("strikePriceReference", typeof(EFS_EventMatrixFieldReference))]
		public object strikePrice;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool premiumQuoteSpecified;
		[System.Xml.Serialization.XmlElementAttribute("premiumQuote", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("premiumQuoteReference", typeof(EFS_EventMatrixFieldReference))]
		public object premiumQuote;
		#endregion FX Details
		#region IRD Details
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool fixedRateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("fixedRate", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("fixedRateReference", typeof(EFS_EventMatrixFieldReference))]
		public object fixedRate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool dayCountFractionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("dayCountFraction", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("dayCountFractionReference", typeof(EFS_EventMatrixFieldReference))]
		public object dayCountFraction;
		#endregion IRD Details
		#endregion Variables
		#region Constructors
		public EFS_EventMatrixItemDetails(){}
		#endregion Constructors
	}
	#endregion EFS_EventMatrixItemDetails
	#region EFS_EventMatrixKey
	/// <summary>
	/// <newpara><b>Description :</b> The key Event.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>category (exactly one occurrence; of the type EFS_EventField) 
	/// the element (method or property or constant) to retrieve the category event.</newpara>
	/// <newpara>Or</newpara>
	/// <newpara>categoryReference (exactly one occurrence; of the type EFS_EventFieldReference)
	/// the element reference (parameter) to retrieve the category event.</newpara>
	/// <newpara>Either</newpara> 
	/// <newpara>relativeTo (exactly one occurrence; of the type EFS_EventField)
	/// the element (method or property or constant) to retrieve the relativeTo event.</newpara>
	/// <newpara>Or</newpara>
	/// <newpara>relativeToReference (exactly one occurrence; of the type EFS_EventFieldReference)
	/// the element reference (parameter) to retrieve the relativeTo event.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: EFS_EventMatrixItem</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	public class EFS_EventMatrixKey : ICloneable	
	{
		[System.Xml.Serialization.XmlElementAttribute("eventCode", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("eventCodeReference", typeof(EFS_EventMatrixFieldReference))]
		public object eventCode;

		[System.Xml.Serialization.XmlElementAttribute("eventType", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("eventTypeReference", typeof(EFS_EventMatrixFieldReference))]
		public object eventType;

		public EFS_EventMatrixKey(){}

		#region Membres de ICloneable
		public object Clone()
		{
			EFS_EventMatrixKey clone = new EFS_EventMatrixKey();
			clone.eventCode = this.eventCode;
			clone.eventType = this.eventType;
			return clone;
		}
		#endregion
	}
	#endregion EFS_EventMatrixKey
	#region EFS_EventMatrixOccurence
	/// <summary>
	/// <newpara><b>Description :</b> The occurence element (FpML or EFS element reference) 
	/// for a group or item event matrix declaration</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>to (exactly one occurrence; of the type EventOccurenceEnum) The type of occurence Loop.
	/// All (All elements), AllExceptFirst (All elements except first), AllExceptLast (All elements except last),
	/// AllExceptFirstAndLast (All elements except first and last), First (First element only), Last (Last element only),
	/// Unique (for no array),
	/// </newpara>
	/// <newpara>field (zero or one occurrence; of the type xsd:string) a child's element of element declared in Value.</newpara>
	/// <newpara>field (zero or one occurrence; of the type xsd:boolean) true if element declared is optinal.</newpara>
	/// <newpara>Value (exactly one occurrence; of the type xsd:string) a element of EFS or FpML trade.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: EFS_EventMatrixItem</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	public class EFS_EventMatrixOccurence
	{
		[System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, DataType="anyURI")]
		public string to;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool positionSpecified;
		[System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, DataType="anyURI")]
		public string position;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool fieldSpecified;
		[System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, DataType="anyURI")]
		public string field;
		[System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, DataType="boolean")]
		public bool isOptional;
		[System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, DataType="boolean")]
		public bool isFieldOptional;
		[System.Xml.Serialization.XmlTextAttribute(DataType="normalizedString")]
		public string Value;

		public EFS_EventMatrixOccurence(){}
	}
	#endregion EFS_EventMatrixOccurence
	#region EFS_EventMatrixParameter
	/// <summary>
	/// <newpara><b>Description :</b> The list of event's parameters</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara><b>Contents :</b> Inherited element(s): (This definition inherits the content defined by the type 
	/// EFS_EventMatrixField)</newpara>
	/// <newpara>id (exactly one occurrence; of the type xsd:string) key of the parameter</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: EFS_EventMatrixGroup</newpara>
	///<newpara>• Complex type: EFS_EventMatrixItem</newpara>
	///<newpara>• Complex type: EFS_EventMatrix</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	public class EFS_EventMatrixParameter : EFS_EventMatrixField
	{
		[System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, DataType="ID")]
		public string id
		{
			set {efs_id=new EFS_Id(value);}
			get 
			{
				if (efs_id==null)
					return null;
				else
					return efs_id.Value;
			}
		}
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_Id efs_id;
	}
	#endregion EFS_EventMatrixParameter
	#region EFS_EventMatrixSubItems
	public class EFS_EventMatrixSubItems
	{
		[System.Xml.Serialization.XmlElementAttribute("subItem")]
		public EFS_EventMatrixSubItem[] subItem;

		#region Constructors
		public EFS_EventMatrixSubItems(){}
		#endregion Constructors
	}
	#endregion EFS_EventMatrixSubItems
	#region EFS_EventMatrixSubItem
	public class EFS_EventMatrixSubItem
	{
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool conditionalSpecified;
		[System.Xml.Serialization.XmlElementAttribute("conditional", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("conditionalReference", typeof(EFS_EventMatrixFieldReference))]
		public object conditional;

		[System.Xml.Serialization.XmlElementAttribute("eventClass", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("eventClassReference", typeof(EFS_EventMatrixFieldReference))]
		public object eventClass;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool eventDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("eventDate", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("eventDateReference", typeof(EFS_EventMatrixFieldReference))]
		public object eventDate;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool isPaymentSpecified;
		[System.Xml.Serialization.XmlElementAttribute("isPayment", typeof(EFS_EventMatrixField))]
		[System.Xml.Serialization.XmlElementAttribute("isPaymentReference", typeof(EFS_EventMatrixFieldReference))]
		public object isPayment;

		#region Constructors
		public EFS_EventMatrixSubItem(){}
		#endregion Constructors
	}
	#endregion EFS_EventMatrixSubItem
	#endregion General
	#region Trade
	#region EFS_EventMatrixTrade
	/// <summary>
	/// <newpara><b>Description :</b> </newpara>
	/// <newpara><b>Contents :</b> Inherited element(s): (This definition inherits the content defined by the type 
	/// EFS_EventMatrixGroup)</newpara>
	/// <newpara><b>• The abstract base type from which all Events structures must be derived.</b></newpara>
	/// <newpara>eventMatrixGroup (one or more occurrence; of the type EFS_EventMatrixGroup) 
	/// A grouping of product Events definitions.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: EFS_EventMatrix</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	public class EFS_EventMatrixTrade : EFS_EventMatrixGroup
	{
		[System.Xml.Serialization.XmlElementAttribute("bulletPayment"       , typeof(EFS_EventMatrixBulletPayment))]
		[System.Xml.Serialization.XmlElementAttribute("capFloor"            , typeof(EFS_EventMatrixCapFloor))]
		[System.Xml.Serialization.XmlElementAttribute("fra"                 , typeof(EFS_EventMatrixFra))]
		[System.Xml.Serialization.XmlElementAttribute("fxAverageRateOption" , typeof(EFS_EventMatrixFxAverageRateOption))]
		[System.Xml.Serialization.XmlElementAttribute("fxBarrierOption"     , typeof(EFS_EventMatrixFxBarrierOption))]
		[System.Xml.Serialization.XmlElementAttribute("fxDigitalOption"     , typeof(EFS_EventMatrixFxDigitalOption))]
		[System.Xml.Serialization.XmlElementAttribute("fxSimpleOption"      , typeof(EFS_EventMatrixFxOptionLeg))]
		[System.Xml.Serialization.XmlElementAttribute("fxSingleLeg"         , typeof(EFS_EventMatrixFxLeg))]
		[System.Xml.Serialization.XmlElementAttribute("fxSwap"              , typeof(EFS_EventMatrixFxSwap))]
		[System.Xml.Serialization.XmlElementAttribute("returnSwap"          , typeof(EFS_EventMatrixReturnSwap))]
		[System.Xml.Serialization.XmlElementAttribute("strategy"            , typeof(EFS_EventMatrixStrategy))]
		[System.Xml.Serialization.XmlElementAttribute("swap"                , typeof(EFS_EventMatrixSwap))]
		[System.Xml.Serialization.XmlElementAttribute("swaption"            , typeof(EFS_EventMatrixSwaption))]
		[System.Xml.Serialization.XmlElementAttribute("termDeposit"         , typeof(EFS_EventMatrixTermDeposit))]
		public EFS_EventMatrixGroup[] group;
	}
	#endregion EFS_EventMatrixTrade
	#region EFS_EventMatrixProduct
	/// <summary>
	/// <newpara><b>Description :</b> The base type which all EFS Events matrix products extend.</newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Inherited element(s): (This definition inherits the content defined by the type EFS_EventMatrixGroup)</newpara>
	/// <newpara>• The base type which all EFS Events matrix extend.</newpara>
	/// <newpara>productName (zero or one occurrence; of the type xsd:string) The name of the product ( for reporting).</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara>• Complex type: EFS_EventMatrixBulletPayment</newpara>
	///<newpara>• Complex type: EFS_EventMatrixCapFloor</newpara>
	///<newpara>• Complex type: EFS_EventMatrixFra</newpara>
	///<newpara>• Complex type: EFS_EventMatrixFxLeg</newpara>
	///<newpara>• Complex type: EFS_EventMatrixFxSwap</newpara>
	///<newpara>• Complex type: EFS_EventMatrixReturnSwap</newpara>
	///<newpara>• Complex type: EFS_EventMatrixSwap</newpara>
	///<newpara>• Complex type: EFS_EventMatrixSwaption</newpara>
	///<newpara>• Complex type: EFS_EventMatrixTermDeposit</newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///<newpara>• Complex type: EFS_EventMatrixBulletPayment</newpara>
	///<newpara>• Complex type: EFS_EventMatrixCapFloor</newpara>
	///<newpara>• Complex type: EFS_EventMatrixFra</newpara>
	///<newpara>• Complex type: EFS_EventMatrixFxLeg</newpara>
	///<newpara>• Complex type: EFS_EventMatrixFxSwap</newpara>
	///<newpara>• Complex type: EFS_EventMatrixReturnSwap</newpara>
	///<newpara>• Complex type: EFS_EventMatrixSwap</newpara>
	///<newpara>• Complex type: EFS_EventMatrixSwaption</newpara>
	///<newpara>• Complex type: EFS_EventMatrixTermDeposit</newpara>
	///</remarks>
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixBulletPayment))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixCapFloor))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixFra))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixFxAverageRateOption))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixFxBarrierOption))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixFxDigitalOption))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixFxLeg))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixFxOptionLeg))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixFxSwap))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixReturnSwap))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixStrategy))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixSwap))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixSwaption))]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(EFS_EventMatrixTermDeposit))]
	public class EFS_EventMatrixProduct : EFS_EventMatrixGroup
	{
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public string productName;
	}

	#endregion EFS_EventMatrixProduct
	#endregion Trade
	#region Product
	#region EFS_EventMatrixBulletPayment
	/// <summary>
	/// <newpara><b>Description :</b></newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Inherited element(s): (This definition inherits the content defined by the type EFS_EventMatrixProduct)</newpara>
	/// <newpara>• The base type which all EFS Events matrix product extend.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	public class EFS_EventMatrixBulletPayment : EFS_EventMatrixProduct
	{
		[System.Xml.Serialization.XmlElementAttribute("groupLevel", typeof(EFS_EventMatrixBulletPaymentLevel))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixBulletPayment()
		{
			Type tProduct = typeof(BulletPayment);
			productName = tProduct.Name;
		}
	}
	#endregion EFS_EventMatrixBulletPayment
	#region EFS_EventMatrixCapFloor
	/// <summary>
	/// <newpara><b>Description :</b></newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Inherited element(s): (This definition inherits the content defined by the type EFS_EventMatrixProduct)</newpara>
	/// <newpara>• The base type which all EFS Events matrix product extend.</newpara>
	/// <newpara>group (one or more occurrence; of the type EFS_EventMatrixGroup) 
	/// subGroup of Swap Events matrix group (example : capFloorStream)</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	public class EFS_EventMatrixCapFloor : EFS_EventMatrixProduct
	{
		[System.Xml.Serialization.XmlElementAttribute("capFloorStream", typeof(EFS_EventMatrixStream))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixCapFloor()
		{
			Type tProduct = typeof(CapFloor);
			productName   = tProduct.Name;
		}
	}
	#endregion EFS_EventMatrixCapFloor
	#region EFS_EventMatrixFra
	/// <summary>
	/// <newpara><b>Description :</b></newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Inherited element(s): (This definition inherits the content defined by the type EFS_EventMatrixProduct)</newpara>
	/// <newpara>• The base type which all EFS Events matrix product extend.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	public class EFS_EventMatrixFra : EFS_EventMatrixProduct
	{
		[System.Xml.Serialization.XmlElementAttribute("groupLevel", typeof(EFS_EventMatrixFraLevel))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixFra()
		{
			Type tProduct = typeof(Fra);
			productName = tProduct.Name;
		}
	}
	#endregion EFS_EventMatrixFra
	#region EFS_EventMatrixFxAverageRateOption
	/// <summary>
	/// <newpara><b>Description :</b></newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Inherited element(s): (This definition inherits the content defined by the type EFS_EventMatrixProduct)</newpara>
	/// <newpara>• The base type which all EFS Events matrix product extend.</newpara>
	/// <newpara>group (one or more occurrence; of the type EFS_EventMatrixGroup) 
	/// subGroup of Change Events matrix group</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	public class EFS_EventMatrixFxAverageRateOption : EFS_EventMatrixProduct
	{
		[System.Xml.Serialization.XmlElementAttribute("premium", typeof(EFS_EventMatrixFxOptionPremium))]
		[System.Xml.Serialization.XmlElementAttribute("fxOptionType", typeof(EFS_EventMatrixFxOptionType))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixFxAverageRateOption()
		{
			Type tProduct = typeof(FxAverageRateOption);
			productName   = tProduct.Name;
		}
	}
	#endregion EFS_EventMatrixFxAverageRateOption
	#region EFS_EventMatrixFxBarrierOption
	/// <summary>
	/// <newpara><b>Description :</b></newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Inherited element(s): (This definition inherits the content defined by the type EFS_EventMatrixProduct)</newpara>
	/// <newpara>• The base type which all EFS Events matrix product extend.</newpara>
	/// <newpara>group (one or more occurrence; of the type EFS_EventMatrixGroup) 
	/// subGroup of Change Events matrix group</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	public class EFS_EventMatrixFxBarrierOption : EFS_EventMatrixProduct
	{
		[System.Xml.Serialization.XmlElementAttribute("premium", typeof(EFS_EventMatrixFxOptionPremium))]
		[System.Xml.Serialization.XmlElementAttribute("fxOptionType", typeof(EFS_EventMatrixFxOptionType))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixFxBarrierOption()
		{
			Type tProduct = typeof(FxBarrierOption);
			productName = tProduct.Name;
		}
	}
	#endregion EFS_EventMatrixFxBarrierOption
	#region EFS_EventMatrixFxDigitalOption
	/// <summary>
	/// <newpara><b>Description :</b></newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Inherited element(s): (This definition inherits the content defined by the type EFS_EventMatrixProduct)</newpara>
	/// <newpara>• The base type which all EFS Events matrix product extend.</newpara>
	/// <newpara>group (one or more occurrence; of the type EFS_EventMatrixGroup) 
	/// subGroup of Change Events matrix group</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	public class EFS_EventMatrixFxDigitalOption : EFS_EventMatrixProduct
	{
		[System.Xml.Serialization.XmlElementAttribute("premium", typeof(EFS_EventMatrixFxOptionPremium))]
		[System.Xml.Serialization.XmlElementAttribute("digitalOptionType", typeof(EFS_EventMatrixDigitalOptionType))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixFxDigitalOption()
		{
			Type tProduct = typeof(FxDigitalOption);
			productName = tProduct.Name;
		}
	}
	#endregion EFS_EventMatrixFxDigitalOption
	#region EFS_EventMatrixFxLeg
	/// <summary>
	/// <newpara><b>Description :</b></newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Inherited element(s): (This definition inherits the content defined by the type EFS_EventMatrixProduct)</newpara>
	/// <newpara>• The base type which all EFS Events matrix product extend.</newpara>
	/// <newpara>group (one or more occurrence; of the type EFS_EventMatrixGroup) 
	/// subGroup of Change Events matrix group</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	public class EFS_EventMatrixFxLeg : EFS_EventMatrixProduct
	{
		[System.Xml.Serialization.XmlElementAttribute("deliverableLeg", typeof(EFS_EventMatrixFxDeliverableLeg))]
		[System.Xml.Serialization.XmlElementAttribute("nonDeliverableLeg", typeof(EFS_EventMatrixFxNonDeliverableLeg))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixFxLeg()
		{
			Type tProduct = typeof(FxLeg);
			productName = tProduct.Name;
		}
	}
	#endregion EFS_EventMatrixFxLeg
	#region EFS_EventMatrixFxSwap
	/// <summary>
	/// <newpara><b>Description :</b></newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Inherited element(s): (This definition inherits the content defined by the type EFS_EventMatrixProduct)</newpara>
	/// <newpara>• The base type which all EFS Events matrix product extend.</newpara>
	/// <newpara>group (one or more occurrence; of the type EFS_EventMatrixGroup) 
	/// subGroup of Change Events matrix group</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	public class EFS_EventMatrixFxSwap : EFS_EventMatrixProduct
	{
		[System.Xml.Serialization.XmlElementAttribute("deliverableLeg", typeof(EFS_EventMatrixFxDeliverableLeg))]
		[System.Xml.Serialization.XmlElementAttribute("nonDeliverableLeg", typeof(EFS_EventMatrixFxNonDeliverableLeg))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixFxSwap()
		{
			Type tProduct = typeof(FxSwap);
			productName = tProduct.Name;
		}
	}
	#endregion EFS_EventMatrixFxLeg
	#region EFS_EventMatrixFxOptionLeg
	/// <summary>
	/// <newpara><b>Description :</b></newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Inherited element(s): (This definition inherits the content defined by the type EFS_EventMatrixProduct)</newpara>
	/// <newpara>• The base type which all EFS Events matrix product extend.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	public class EFS_EventMatrixFxOptionLeg : EFS_EventMatrixProduct
	{
		[System.Xml.Serialization.XmlElementAttribute("premium", typeof(EFS_EventMatrixFxOptionPremium))]
		[System.Xml.Serialization.XmlElementAttribute("fxOptionType", typeof(EFS_EventMatrixFxOptionType))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixFxOptionLeg()
		{
			Type tProduct = typeof(FxOptionLeg);
			productName   = tProduct.Name;
		}
	}
	#endregion EFS_EventMatrixFxOptionLeg
	#region EFS_EventMatrixReturnSwap
	/// <summary>
	/// <newpara><b>Description :</b></newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Inherited element(s): (This definition inherits the content defined by the type EFS_EventMatrixProduct)</newpara>
	/// <newpara>• The base type which all EFS Events matrix product extend.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	public class EFS_EventMatrixReturnSwap : EFS_EventMatrixProduct
	{
		[System.Xml.Serialization.XmlElementAttribute("groupLevel", typeof(EFS_EventMatrixReturnSwapLevel))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixReturnSwap()
		{
			Type tProduct = typeof(EquitySwap);
			productName   = tProduct.Name;
		}
	}
	#endregion EFS_EventMatrixReturnSwap
	#region EFS_EventMatrixStrategy
	/// <summary>
	/// <newpara><b>Description :</b></newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Inherited element(s): (This definition inherits the content defined by the type EFS_EventMatrixProduct)</newpara>
	/// <newpara>• The base type which all EFS Events matrix product extend.</newpara>
	/// <newpara>group (one or more occurrence; of the type EFS_EventMatrixGroup)</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	public class EFS_EventMatrixStrategy : EFS_EventMatrixProduct
	{
		[System.Xml.Serialization.XmlElementAttribute("bulletPayment"       , typeof(EFS_EventMatrixBulletPayment))]
		[System.Xml.Serialization.XmlElementAttribute("capFloor"            , typeof(EFS_EventMatrixCapFloor))]
		[System.Xml.Serialization.XmlElementAttribute("fra"                 , typeof(EFS_EventMatrixFra))]
		[System.Xml.Serialization.XmlElementAttribute("fxAverageRateOption" , typeof(EFS_EventMatrixFxAverageRateOption))]
		[System.Xml.Serialization.XmlElementAttribute("fxBarrierOption"     , typeof(EFS_EventMatrixFxBarrierOption))]
		[System.Xml.Serialization.XmlElementAttribute("fxDigitalOption"     , typeof(EFS_EventMatrixFxDigitalOption))]
		[System.Xml.Serialization.XmlElementAttribute("fxSimpleOption"      , typeof(EFS_EventMatrixFxOptionLeg))]
		[System.Xml.Serialization.XmlElementAttribute("fxSingleLeg"         , typeof(EFS_EventMatrixFxLeg))]
		[System.Xml.Serialization.XmlElementAttribute("fxSwap"              , typeof(EFS_EventMatrixFxSwap))]
		[System.Xml.Serialization.XmlElementAttribute("returnSwap"          , typeof(EFS_EventMatrixReturnSwap))]
		[System.Xml.Serialization.XmlElementAttribute("strategy"            , typeof(EFS_EventMatrixStrategy))]
		[System.Xml.Serialization.XmlElementAttribute("swap"                , typeof(EFS_EventMatrixSwap))]
		[System.Xml.Serialization.XmlElementAttribute("swaption"            , typeof(EFS_EventMatrixSwaption))]
		[System.Xml.Serialization.XmlElementAttribute("termDeposit"         , typeof(EFS_EventMatrixTermDeposit))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixStrategy()
		{
			Type tProduct = typeof(Strategy);
			productName = tProduct.Name;
		}
	}
	#endregion EFS_EventMatrixStrategy
	#region EFS_EventMatrixSwap
	/// <summary>
	/// <newpara><b>Description :</b></newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Inherited element(s): (This definition inherits the content defined by the type EFS_EventMatrixProduct)</newpara>
	/// <newpara>• The base type which all EFS Events matrix product extend.</newpara>
	/// <newpara>group (one or more occurrence; of the type EFS_EventMatrixGroup) 
	/// subGroup of Swap Events matrix group (example : swapStream)</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	public class EFS_EventMatrixSwap : EFS_EventMatrixProduct
	{
		[System.Xml.Serialization.XmlElementAttribute("swapStream", typeof(EFS_EventMatrixStream))]
		[System.Xml.Serialization.XmlElementAttribute("cancelableProvision", typeof(EFS_EventMatrixCancelableProvision))]
		[System.Xml.Serialization.XmlElementAttribute("earlyTerminationProvision", typeof(EFS_EventMatrixEarlyTerminationProvision))]
		[System.Xml.Serialization.XmlElementAttribute("extendibleProvision", typeof(EFS_EventMatrixExtendibleProvision))]
		[System.Xml.Serialization.XmlElementAttribute("mandatoryEarlyTerminationProvision", typeof(EFS_EventMatrixMandatoryEarlyTerminationProvision))]
		[System.Xml.Serialization.XmlElementAttribute("stepUpProvision", typeof(EFS_EventMatrixStepUpProvision))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixSwap()
		{
			Type tProduct = typeof(Swap);
			productName = tProduct.Name;
		}
	}
	#endregion EFS_EventMatrixSwap
	#region EFS_EventMatrixSwaption
	/// <summary>
	/// <newpara><b>Description :</b></newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Inherited element(s): (This definition inherits the content defined by the type EFS_EventMatrixProduct)</newpara>
	/// <newpara>• The base type which all EFS Events matrix product extend.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	public class EFS_EventMatrixSwaption : EFS_EventMatrixProduct
	{
		public EFS_EventMatrixSwaption()
		{
			Type tProduct = typeof(Swaption);
			productName = tProduct.Name;
		}
	}
	#endregion EFS_EventsParametersSwaption
	#region EFS_EventMatrixTermDeposit
	/// <summary>
	/// <newpara><b>Description :</b></newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Inherited element(s): (This definition inherits the content defined by the type EFS_EventMatrixProduct)</newpara>
	/// <newpara>• The base type which all EFS Events matrix product extend.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	public class EFS_EventMatrixTermDeposit : EFS_EventMatrixProduct
	{
		[System.Xml.Serialization.XmlElementAttribute("groupLevel", typeof(EFS_EventMatrixTermDepositLevel))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixTermDeposit()
		{
			Type tProduct = typeof(TermDeposit);
			productName = tProduct.Name;
		}
	}
	#endregion EFS_EventMatrixTermDeposit
	#endregion Product

	#region ProductGroupLevel
	#region EFS_EventMatrixBulletPaymentLevel
	public class EFS_EventMatrixBulletPaymentLevel : EFS_EventMatrixGroup
	{
		public EFS_EventMatrixBulletPaymentLevel(){}
	}
	#endregion EFS_EventMatrixBulletPaymentLevel
	#region EFS_EventMatrixFraLevel
	public class EFS_EventMatrixFraLevel : EFS_EventMatrixGroup
	{
		[System.Xml.Serialization.XmlElementAttribute("nominalPeriods", typeof(EFS_EventMatrixNominalPeriods))]
		[System.Xml.Serialization.XmlElementAttribute("paymentDates", typeof(EFS_EventMatrixPaymentDates))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixFraLevel(){}
	}
	#endregion EFS_EventMatrixFraLevel
	#region EFS_EventMatrixTermDepositLevel
	public class EFS_EventMatrixTermDepositLevel : EFS_EventMatrixGroup
	{
		[System.Xml.Serialization.XmlElementAttribute("nominalPeriod", typeof(EFS_EventMatrixNominalPeriods))]
		[System.Xml.Serialization.XmlElementAttribute("interest", typeof(EFS_EventMatrixInterest))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixTermDepositLevel(){}
	}
	#endregion EFS_EventMatrixTermDepositLevel
	#region EFS_EventMatrixReturnSwapLevel
	public class EFS_EventMatrixReturnSwapLevel : EFS_EventMatrixGroup
	{
		[System.Xml.Serialization.XmlElementAttribute("returnLeg", typeof(EFS_EventMatrixReturnLeg))]
		[System.Xml.Serialization.XmlElementAttribute("interestLeg", typeof(EFS_EventMatrixInterestLeg))]
		[System.Xml.Serialization.XmlElementAttribute("varianceLeg", typeof(EFS_EventMatrixVarianceLeg))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixReturnSwapLevel(){}
	}
	#endregion EFS_EventMatrixReturnSwapLevel
	#endregion ProductGroupLevel

	#region ReturnSwap Legs
	#region EFS_EventMatrixReturnLeg
	public class EFS_EventMatrixReturnLeg : EFS_EventMatrixGroup
	{
		[System.Xml.Serialization.XmlElementAttribute("initialValuation", typeof(EFS_EventMatrixReturnLegInitialValuation))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixReturnLeg(){}
	}
	#endregion EFS_EventMatrixReturnLeg
	#region EFS_EventMatrixReturnLegInitialValuation
	public class EFS_EventMatrixReturnLegInitialValuation : EFS_EventMatrixGroup
	{
		[System.Xml.Serialization.XmlElementAttribute("cashNotional", typeof(EFS_EventMatrixCashNotional))]
		[System.Xml.Serialization.XmlElementAttribute("securityNotional", typeof(EFS_EventMatrixSecurityNotional))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixReturnLegInitialValuation(){}
	}
	#endregion EFS_EventMatrixReturnLegInitialValuation
	#region EFS_EventMatrixInterestLeg
	public class EFS_EventMatrixInterestLeg : EFS_EventMatrixGroup
	{
		public EFS_EventMatrixInterestLeg(){}
	}
	#endregion EFS_EventMatrixInterestLeg
	#region EFS_EventMatrixVarianceLeg
	public class EFS_EventMatrixVarianceLeg : EFS_EventMatrixGroup
	{
		public EFS_EventMatrixVarianceLeg(){}
	}
	#endregion EFS_EventMatrixVarianceLeg

	#region EFS_EventMatrixBasketComponent
	public class EFS_EventMatrixBasketComponent : EFS_EventMatrixGroup
	{
		public EFS_EventMatrixBasketComponent(){}
	}
	#endregion EFS_EventMatrixBasketComponent
	#region EFS_EventMatrixCashNotional
	public class EFS_EventMatrixCashNotional : EFS_EventMatrixGroup
	{
		public EFS_EventMatrixCashNotional(){}
	}
	#endregion EFS_EventMatrixCashNotional
	#region EFS_EventMatrixSecurityNotional
	public class EFS_EventMatrixSecurityNotional : EFS_EventMatrixGroup
	{
		[System.Xml.Serialization.XmlElementAttribute("basketComponent", typeof(EFS_EventMatrixBasketComponent))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixSecurityNotional(){}
	}
	#endregion EFS_EventMatrixSecurityNotional
	#endregion ReturnSwap Legs

	#region Fx
	#region EFS_EventMatrixAvgQuotedFixing
	public class EFS_EventMatrixAvgQuotedFixing : EFS_EventMatrixGroup
	{
		[System.Xml.Serialization.XmlElementAttribute("quotedFixing", typeof(EFS_EventMatrixQuotedFixing))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixAvgQuotedFixing(){}
	}
	#endregion EFS_EventMatrixAvgQuotedFixing
	#region EFS_EventMatrixAvgSettlementFixing
	public class EFS_EventMatrixAvgSettlementFixing : EFS_EventMatrixGroup
	{
		[System.Xml.Serialization.XmlElementAttribute("settlementFixing", typeof(EFS_EventMatrixSettlementFixing))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixAvgSettlementFixing(){}
	}
	#endregion EFS_EventMatrixAvgSettlementFixing
	#region EFS_EventMatrixFxDeliverableLeg
	public class EFS_EventMatrixFxDeliverableLeg : EFS_EventMatrixGroup
	{
		[System.Xml.Serialization.XmlElementAttribute("exchangeCurrency", typeof(EFS_EventMatrixExchangeCurrency))]
		[System.Xml.Serialization.XmlElementAttribute("sideRate", typeof(EFS_EventMatrixSideRate))]
		[System.Xml.Serialization.XmlElementAttribute("fwpDepreciableAmount", typeof(EFS_EventMatrixFwpDepreciableAmount))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixFxDeliverableLeg(){}
	}
	#endregion EFS_EventMatrixFxDeliverableLeg
	#region EFS_EventMatrixExchangeCurrency
	public class EFS_EventMatrixExchangeCurrency : EFS_EventMatrixGroup
	{
		[System.Xml.Serialization.XmlElementAttribute("spotFixing", typeof(EFS_EventMatrixSpotFixing))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixExchangeCurrency(){}
	}
	#endregion EFS_EventMatrixExchangeCurrency
	#region EFS_EventMatrixFxNonDeliverableLeg
	public class EFS_EventMatrixFxNonDeliverableLeg : EFS_EventMatrixGroup
	{
		[System.Xml.Serialization.XmlElementAttribute("exchangeCurrency", typeof(EFS_EventMatrixExchangeCurrency))]
		[System.Xml.Serialization.XmlElementAttribute("sideRate", typeof(EFS_EventMatrixSideRate))]
		[System.Xml.Serialization.XmlElementAttribute("settlementCurrency", typeof(EFS_EventMatrixSettlementCurrency))]
		[System.Xml.Serialization.XmlElementAttribute("fwpDepreciableAmount", typeof(EFS_EventMatrixFwpDepreciableAmount))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixFxNonDeliverableLeg(){}
	}
	#endregion EFS_EventMatrixFxNonDeliverableLeg
	#region EFS_EventMatrixExchangeCurrency
	public class EFS_EventMatrixFxPremiumDepreciableAmount : EFS_EventMatrixGroup
	{
		public EFS_EventMatrixFxPremiumDepreciableAmount(){}
	}
	#endregion EFS_EventMatrixExchangeCurrency
	#region EFS_EventMatrixSettlementCurrency
	public class EFS_EventMatrixFwpDepreciableAmount : EFS_EventMatrixGroup
	{
		public EFS_EventMatrixFwpDepreciableAmount(){}
	}
	#endregion EFS_EventMatrixSettlementCurrency
	#region EFS_EventMatrixSettlementCurrency
	public class EFS_EventMatrixSettlementCurrency : EFS_EventMatrixGroup
	{
		[System.Xml.Serialization.XmlElementAttribute("avgSettlementFixing", typeof(EFS_EventMatrixAvgSettlementFixing))]
		[System.Xml.Serialization.XmlElementAttribute("settlementFixing", typeof(EFS_EventMatrixSettlementFixing))]
		[System.Xml.Serialization.XmlElementAttribute("avgQuotedFixing", typeof(EFS_EventMatrixAvgQuotedFixing))]
		[System.Xml.Serialization.XmlElementAttribute("quotedFixing", typeof(EFS_EventMatrixQuotedFixing))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixSettlementCurrency(){}
	}
	#endregion EFS_EventMatrixSettlementCurrency
	#region EFS_EventMatrixSettlementFixing
	public class EFS_EventMatrixSettlementFixing : EFS_EventMatrixGroup
	{
		public EFS_EventMatrixSettlementFixing(){}
	}
	#endregion EFS_EventMatrixDeliveryFixing
	#region EFS_EventMatrixSideRate
	public class EFS_EventMatrixSideRate : EFS_EventMatrixGroup
	{
		public EFS_EventMatrixSideRate(){}
	}
	#endregion EFS_EventMatrixSideRate
	#region EFS_EventMatrixSpotFixing
	public class EFS_EventMatrixSpotFixing : EFS_EventMatrixGroup
	{
		public EFS_EventMatrixSpotFixing(){}
	}
	#endregion EFS_EventMatrixSpotFixing	
	#region EFS_EventMatrixQuotedFixing
	public class EFS_EventMatrixQuotedFixing : EFS_EventMatrixGroup
	{
		public EFS_EventMatrixQuotedFixing(){}
	}
	#endregion EFS_EventMatrixQuotedFixing
	#endregion Fx
	#region FxOption
	#region FxDigitalOption
	#region EFS_EventMatrixDigitalOptionType
	public class EFS_EventMatrixDigitalOptionType : EFS_EventMatrixGroup
	{
		[System.Xml.Serialization.XmlElementAttribute("payout", typeof(EFS_EventMatrixFxPayout))]
		[System.Xml.Serialization.XmlElementAttribute("trigger", typeof(EFS_EventMatrixFxTrigger))]
		[System.Xml.Serialization.XmlElementAttribute("barrier", typeof(EFS_EventMatrixFxBarrier))]
		[System.Xml.Serialization.XmlElementAttribute("exerciseDates", typeof(EFS_EventMatrixFxExerciseDates))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixDigitalOptionType(){}
	}
	#endregion EFS_EventMatrixDigitalOptionType
	#region EFS_EventMatrixFxPayout
	public class EFS_EventMatrixFxPayout : EFS_EventMatrixGroup{}
	#endregion EFS_EventMatrixFxPayout
	#endregion FxDigitalOption
	#region EFS_EventMatrixFxBarrier
	public class EFS_EventMatrixFxBarrier : EFS_EventMatrixGroup{}
	#endregion EFS_EventMatrixFxBarrier
	#region EFS_EventMatrixFxCurrencyAmount
	public class EFS_EventMatrixFxCurrencyAmount : EFS_EventMatrixGroup{}
	#endregion EFS_EventMatrixFxCurrencyAmount
	#region EFS_EventMatrixFxExerciseDates
	public class EFS_EventMatrixFxExerciseDates : EFS_EventMatrixGroup{}
	#endregion EFS_EventMatrixFxExerciseDates
	#region EFS_EventMatrixFxExerciseProcedure
	public class EFS_EventMatrixFxExerciseProcedure : EFS_EventMatrixGroup{}
	#endregion EFS_EventMatrixFxExerciseProcedure
	#region EFS_EventMatrixFxOptionPremium
	public class EFS_EventMatrixFxOptionPremium : EFS_EventMatrixGroup{}
	#endregion EFS_EventMatrixFxOptionPremium
	#region EFS_EventMatrixFxOptionType
	public class EFS_EventMatrixFxOptionType : EFS_EventMatrixGroup
	{
		[System.Xml.Serialization.XmlElementAttribute("fxCurrencyAmount", typeof(EFS_EventMatrixFxCurrencyAmount))]
		[System.Xml.Serialization.XmlElementAttribute("exerciseDates", typeof(EFS_EventMatrixFxExerciseDates))]
		[System.Xml.Serialization.XmlElementAttribute("barrier", typeof(EFS_EventMatrixFxBarrier))]
		[System.Xml.Serialization.XmlElementAttribute("rebate", typeof(EFS_EventMatrixFxRebate))]
		[System.Xml.Serialization.XmlElementAttribute("exerciseProcedure", typeof(EFS_EventMatrixFxExerciseProcedure))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixFxOptionType(){}
	}
	#endregion EFS_EventMatrixFxOptionType
	#region EFS_EventMatrixFxRebate
	public class EFS_EventMatrixFxRebate : EFS_EventMatrixGroup{}
	#endregion EFS_EventMatrixFxRebate

	#region EFS_EventMatrixFxTrigger
	public class EFS_EventMatrixFxTrigger : EFS_EventMatrixGroup{}
	#endregion EFS_EventMatrixFxTrigger
	#endregion FxOption

	#region Swap
	#region EFS_EventMatrixCancelableProvision
	public class EFS_EventMatrixCancelableProvision : EFS_EventMatrixGroup
	{
		public EFS_EventMatrixCancelableProvision(){}
	}
	#endregion EFS_EventMatrixCancelableProvision
	#region EFS_EventMatrixEarlyTerminationProvision
	public class EFS_EventMatrixEarlyTerminationProvision : EFS_EventMatrixGroup
	{
		public EFS_EventMatrixEarlyTerminationProvision(){}
	}
	#endregion EFS_EventMatrixCancelableProvision
	#region EFS_EventMatrixEarlyTerminationProvision
	public class EFS_EventMatrixMandatoryEarlyTerminationProvision : EFS_EventMatrixGroup
	{
		public EFS_EventMatrixMandatoryEarlyTerminationProvision(){}
	}
	#endregion EFS_EventMatrixCancelableProvision
	#region EFS_EventMatrixExtendibleProvision
	public class EFS_EventMatrixExtendibleProvision : EFS_EventMatrixGroup
	{
		public EFS_EventMatrixExtendibleProvision(){}
	}
	#endregion EFS_EventMatrixExtendibleProvision
	#region EFS_EventMatrixStepUpProvision
	public class EFS_EventMatrixStepUpProvision : EFS_EventMatrixGroup
	{
		public EFS_EventMatrixStepUpProvision(){}
	}
	#endregion EFS_EventMatrixStepUpProvision
	#endregion Swap
	#region Stream
	#region EFS_EventMatrixStream
	/// <summary>
	/// <newpara><b>Description :</b></newpara>
	/// <newpara><b>Contents :</b></newpara>
	/// <newpara>Inherited element(s): (This definition inherits the content defined by the type EFS_EventMatrixGroup)
	/// </newpara>
	/// <newpara>• The base type which all EFS Events matrix extend.</newpara>
	///</summary>
	///<remarks>
	///<newpara><b>Used by :</b></newpara>
	///<newpara><b>Derived Types :</b></newpara>
	///</remarks>
	public class EFS_EventMatrixStream : EFS_EventMatrixGroup
	{
		[System.Xml.Serialization.XmlElementAttribute("nominalPeriods", typeof(EFS_EventMatrixNominalPeriods))]
		[System.Xml.Serialization.XmlElementAttribute("nominalPeriodsVariation", typeof(EFS_EventMatrixNominalPeriodsVariation))]
		[System.Xml.Serialization.XmlElementAttribute("paymentDates", typeof(EFS_EventMatrixPaymentDates))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixStream(){}
	}
	#endregion EFS_EventMatrixStream
	#region EFS_EventMatrixCalculationPeriodDates
	public class EFS_EventMatrixCalculationPeriodDates : EFS_EventMatrixGroup
	{
		[System.Xml.Serialization.XmlElementAttribute("resetDates", typeof(EFS_EventMatrixResetDates))]
		[System.Xml.Serialization.XmlElementAttribute("capFlooreds", typeof(EFS_EventMatrixCapFlooreds))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixCalculationPeriodDates(){}
	}
	#endregion EFS_EventMatrixCalculationPeriodDates
	#region EFS_EventMatrixCapFloored
	public class EFS_EventMatrixCapFlooreds : EFS_EventMatrixGroup
	{
		public EFS_EventMatrixCapFlooreds(){}
	}
	#endregion EFS_EventMatrixCapFlooreds
	#region EFS_EventMatrixNominalPeriods
	public class EFS_EventMatrixNominalPeriods : EFS_EventMatrixGroup
	{
		public EFS_EventMatrixNominalPeriods(){}
	}
	#endregion EFS_EventMatrixNominalPeriods
	#region EFS_EventMatrixNominalPeriodsVariation
	public class EFS_EventMatrixNominalPeriodsVariation : EFS_EventMatrixGroup
	{
		public EFS_EventMatrixNominalPeriodsVariation(){}
	}
	#endregion EFS_EventMatrixNominalPeriodsVariation
	#region EFS_EventMatrixPaymentDates
	public class EFS_EventMatrixPaymentDates : EFS_EventMatrixGroup
	{
		[System.Xml.Serialization.XmlElementAttribute("calculationPeriodDates", typeof(EFS_EventMatrixCalculationPeriodDates))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixPaymentDates(){}
	}
	#endregion EFS_EventMatrixPaymentDates

	#region EFS_EventMatrixResetDates
	public class EFS_EventMatrixResetDates : EFS_EventMatrixGroup
	{
		[System.Xml.Serialization.XmlElementAttribute("selfAverageDates", typeof(EFS_EventMatrixSelfAverageDates))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixResetDates(){}
	}
	#endregion EFS_EventMatrixResetDates
	#region EFS_EventMatrixSelfAverageDates
	public class EFS_EventMatrixSelfAverageDates : EFS_EventMatrixGroup
	{
		[System.Xml.Serialization.XmlElementAttribute("selfResetDates", typeof(EFS_EventMatrixSelfResetDates))]
		public EFS_EventMatrixGroup[] group;

		public EFS_EventMatrixSelfAverageDates(){}
	}
	#endregion EFS_EventMatrixSelfAverageDates
	#region EFS_EventMatrixSelfResetDates
	public class EFS_EventMatrixSelfResetDates : EFS_EventMatrixGroup
	{
		public EFS_EventMatrixSelfResetDates(){}
	}
	#endregion EFS_EventMatrixSelfResetDates
	#endregion Stream

	#region TermDeposit
	#region EFS_EventMatrixInterest
	public class EFS_EventMatrixInterest : EFS_EventMatrixGroup
	{
		public EFS_EventMatrixInterest(){}
	}
	#endregion EFS_EventMatrixInterest
	#endregion TermDeposit

	#endregion Global Complex Types [Events Matrix]

	#region Global Shared Enum Types
	#region Events Matrix Occurence Enum
	public enum EventOccurenceEnum
	{
		All,
		AllExceptFirst,
		AllExceptLast,
		AllExceptFirstAndLast,
		First,
		FirstAndLast,
		Last,
		None,
		Unique,
		Item,
	}
	#endregion Events Matrix Occurence Enum
	#region Events Field Member Enum
	public enum EFS_FieldMemberEnum
	{
		Boolean,
		Method,
		Property,
		Constant,
		StaticMethod,
		StaticProperty,
		Unknown,
	}
	#endregion EFS_FieldMemberEnum
	#endregion Global Shared Enum Types

}
