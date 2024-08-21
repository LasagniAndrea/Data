#region Using Directives
using System;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Reflection;
using EFS.ACommon;
using EFS.Common;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using Tz = EFS.TimeZone;
#endregion Using Directives

namespace EFS.GUI.Interface
{
	#region Interfaces
	#region IEFS_Array
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
	public interface IEFS_Array
	{
		object DisplayArray(object pCurrent,FieldInfo pFldCurrent,object pParent,FieldInfo pFldParent,ControlGUI pControlGUI,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor); 
	}
	#endregion IEFS_Array
	#region IEFS_Choice
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
	public interface IEFS_Choice
	{
		object DisplayChoice(object pCurrent,FieldInfo pFldCurrent,object pParent,FieldInfo pFldParent,ControlGUI pControlGUI,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor); 
	}
	#endregion IEFS_Choice
	#region IEFS_Control
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
	public interface IEFS_Control
	{
		#region Mandatory
		object Mandatory(object pCurrent,FieldInfo pFldCurrent,object pParent,FieldInfo pFldParent,ControlGUI pControlGUI,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor);
		#endregion Mandatory
		#region Optional
		object Optional(object pCurrent,FieldInfo pFldCurrent,object pParent,FieldInfo pFldParent,ControlGUI pControlGUI,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor);
		#endregion Optional
	}
	#endregion IEFS_Control
	#endregion Interfaces

	#region Substitution Classes
	#region Date Classes
	#region EFS_DateBase
	public class EFS_DateBase : DateCalendarGUI
	{
        /// <summary>
        ///  Obtient yyyy-MM-dd
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual string FmtDate
        {
            get { return DtFunc.FmtISODate; }
        }

        /// <summary>
        /// Obtient yyyy-MM-ddTHH:mm:ss
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual string FmtDateTime
        {
            get { return DtFunc.FmtISODateTime; }
        }
        
        /// <summary>
        ///  Représentation de la date selon les formats FmtDate (par défaut yyyy-MM-dd)  ou FmtDateTime (yyyy-MM-ddTHH:mm:ss)
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute()]
		[ControlGUI(IsPrimary=false,Name="value",LineFeed=MethodsGUI.LineFeedEnum.After)]
		public string Value;

		#region Accessors
        /// <summary>
        /// Obtient ou définit la date (date sans heure)
        /// </summary>
        /// FI 20140808 [20549] virtual Method 
        [System.Xml.Serialization.XmlIgnoreAttribute()]
		public virtual DateTime DateValue
		{
			get {return new DtFunc().StringToDateTime(this.Value);}
            set { Value = DtFunc.DateTimeToString(value, FmtDate); }
		}
        /// <summary>
        /// Obtient ou définit la date (date avec heure)
        /// </summary>
        /// FI 20140808 [20549] virtual Method 
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual DateTime DateTimeValue
		{
			get {return new DtFunc().StringToDateTime(this.Value);}
            set { Value = DtFunc.DateTimeToString(value, FmtDateTime); }
		}
		#endregion Accessors
		#region Constructors
		public EFS_DateBase() {}
		public EFS_DateBase(string pType)
		{
			ReflectionTools.SetDataTypeAttribute(this,pType);
		}
        public EFS_DateBase(string pType, string pValue)
            : this(pType)
        {
            Value = pValue;
        }
		#endregion Constructors
	}
	#endregion EFS_DateBase
	#region EFS_Date
	public class EFS_Date : EFS_DateBase, IEFS_Array
	{
		#region Constructors
		public EFS_Date()              : base(XSDTypeData.Date){}
		public EFS_Date(string pValue) : base(XSDTypeData.Date,pValue){}
		#endregion Constructors

		#region DisplayArray
		/// <revision>
		///     <version>1.2.0</version><date>20071003</date><author>EG</author>
		///     <comment>
		///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
		///     </comment>
		/// </revision>
		public object DisplayArray(object pCurrent,FieldInfo pFldCurrent,object pParent,FieldInfo pFldParent,ControlGUI pControlGUI,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
		{
			if (MethodsGUI.IsOptionalControl(pCurrent,pFldCurrent))
				return (new OptionalItem(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor));
			else
				return (new ObjectArray(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor));
		}
		#endregion DisplayArray 

		#region _Value
		/// <revision>
		///     <version>1.2.0</version><date>20071003</date><author>EG</author>
		///     <comment>
		///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
		///     </comment>
		/// </revision>
		public static object INIT_Value(object pCurrent,FieldInfo pFldCurrent,object pParent,FieldInfo pFldParent,ControlGUI pControlGUI,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
		{
			return (new DateCalendar(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor)); 
		}
		#endregion _Value
	}
	#endregion EFS_Date
	#region EFS_DateTime
    /// <summary>
    /// Représente une date 
    /// </summary>
	public class EFS_DateTime : EFS_DateBase
	{
		#region Constructors
        public EFS_DateTime()
            : base(XSDTypeData.DateTime)
        {
        }
        public EFS_DateTime(string pValue)
            : base(XSDTypeData.DateTime, pValue)
        {
        }
		#endregion Constructors
	}
	#endregion EFS_DateTime
    #region EFS_DateTimeUTC
    /// <summary>
    /// Représente un horodatage avec son equivalent UTC
    /// <para>Le format utilisé est yyyy-MM-ddTHH:mm:ss</para>
    /// </summary>
    /// FI 20140808 [20549] Add Class
    /// FI 20171120 [23580] Modify
    public class EFS_DateTimeUTC : EFS_DateTime
    {

        /// <summary>
        /// Obtient ou définit la date UTC au format "yyyy-MM-ddTHH:mm:ss"
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName="utc")]
        public string utcValue;

        /// <summary>
        /// Représente l'offset entre l'horaire local et l'horaire UTC
        /// <para>Exemple "+02:00"</para>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "offset")]
        public string offset;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tzdbIdSpecified;


        /// <summary>
        /// Obtient ou définit le timezone associé à l'horodatage
        /// <para>Lorsque non renseigné, l'horodatage est exprimé dans le timeZone Local du moteur SQL</para>
        /// </summary>
        /// FI 20171120 [23580] Add tzdbId
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "tzdbid")]
        public string tzdbId;

        /// <summary>
        /// Obtient ou définit la date UTC
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Nullable<DateTime> DateTimeValueUTC
        {
            get
            {
                Nullable<DateTime> ret = null;
                if (StrFunc.IsFilled(this.utcValue))
                    ret = new DtFunc().StringToDateTime(this.utcValue, this.FmtDateTime);
                return ret;
            }
            set
            {
                utcValue = null;
                if (value.HasValue)
                    utcValue = DtFunc.DateTimeToString(value.Value, FmtDateTime);

                SetOffset();
            }
        }


        /// <summary>
        /// Obtient ou définit la date  Local
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override DateTime DateTimeValue
        {
            get
            {
                return base.DateTimeValue;
            }
            set
            {
                base.DateTimeValue = value;
                SetOffset();
            }
        }


        /// <summary>
        /// Affecte le membre offset 
        /// </summary>
        // FI 20160624 [22286] Modify
		// RD 20161123 [22628] Modify
        private void SetOffset()
        {
            offset = string.Empty;
            if ((StrFunc.IsFilled(this.Value) && StrFunc.IsFilled(this.utcValue)))
            {
                DateTime dt = this.DateTimeValue;
                DateTime dtUTC = this.DateTimeValueUTC.Value;

                string sign = string.Empty;
                if (dt.CompareTo(dtUTC) >= 0)  // FI 20160624 [22286] >=0
                {
                    sign = "+";
                }
                else if (dt.CompareTo(dtUTC) < 0)
                {
                    sign = "-";
                }

                // RD 20161123 [22628] Faire la différence entre les deux Dates et pas uniquement les Heures
                //TimeSpan span = new TimeSpan(Math.Abs(dt.Hour - dtUTC.Hour), Math.Abs(dt.Minute - dtUTC.Minute), 0);
                //offset = StrFunc.AppendFormat("{0}{1}:{2}", sign, span.Hours.ToString("D2"), span.Minutes.ToString("D2"));
				TimeSpan span = dt.Subtract(dtUTC);
                offset = StrFunc.AppendFormat("{0}{1}:{2}", sign, Math.Abs(span.Hours).ToString("D2"), Math.Abs(span.Minutes).ToString("D2"));
            }
        }


        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public EFS_DateTimeUTC()
            : base()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pValue">datetime au format ISO yyyy-MM-ddTHH:mm:ss</param>
        public EFS_DateTimeUTC(string pValue)
            : base(pValue)
        {
        }
        #endregion Constructors
    }
    #endregion EFS_DateTimeUTC
    #region EFS_DateTimeOffset
    /// EG 20170918 [23342] New
	// EG 20240531 [WI926] Add member isTemplate
    public class EFS_DateTimeOffset : DateTimeOffsetGUI
    {
        #region Members
        [System.Xml.Serialization.XmlTextAttribute()]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 190, LineFeed = MethodsGUI.LineFeedEnum.After, IsAltTime = true)]
        public string Value;
		[System.Xml.Serialization.XmlIgnore()]
		public bool isTemplateSpecified;
		[System.Xml.Serialization.XmlAttribute()]
		public bool isTemplate;
        #endregion Members
        #region Accessors
		public bool GetValueSpecified
		{
			get { return StrFunc.IsFilled(GetValue);}
		}
		public string GetValue
        {
			get
            {
				return DateTimeValue.HasValue? ISODateTimeValue:isTemplate?Value: String.Empty;
			}
		}
		/// <summary>
        /// Convertie en chaine la DateTimeOffset au format ISO 8601
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        /// EG 20171004 [23452] Upd
        // EG 20171031 [23509] Upd
        public string ISODateTimeValue
        {
            get
            {
                string ret = string.Empty;
                if (this.DateTimeValue.HasValue)
                    ret = DtFunc.DateTimeOffsetUTCToStringTZ(this.DateTimeValue.Value);
                return ret;
            }
        }

        /// <summary>
        /// Convertie en DateTimeOffset dans la current culture une chaine au format ISO 8601 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
		// EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
        public Nullable<DateTimeOffset> DateTimeValue
        {
            get
            {
                Nullable<DateTimeOffset> ret = null;
                if (StrFunc.IsFilled(this.Value))
                    ret = Tz.Tools.ToDateTimeOffset(this.Value);
                return ret;
            }
        }
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public EFS_DateTimeOffset()
        {
        }
        /// <param name="pValue">Date au format ISO 8601 yyyy-mm-ddThh:mm:ss.ffffffzzz</param>
        /// EG 20171025 [23509] Add Regex matching
        public EFS_DateTimeOffset(string pValue)
        {
            ReflectionTools.SetDataTypeAttribute(this, XSDTypeData.DateTime);
			if (Tz.Tools.IsDateFilled(pValue))
				Value = DtFunc.AddEndUTCMarker(pValue);
			else
				Value = pValue;
        }
        #endregion Constructors
    }
    #endregion EFS_DateTimeOffset

    #region EFS_DateArray
    /// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
	public class EFS_DateArray : EFS_DateBase,IEFS_Array
	{
		#region Constructors
		public EFS_DateArray()              : base(XSDTypeData.Date){}
		public EFS_DateArray(string pValue) : base(XSDTypeData.Date,pValue){}
		#endregion Constructors
		#region Methods
		#region DisplayArray
		public object DisplayArray(object pCurrent,FieldInfo pFldCurrent,object pParent,FieldInfo pFldParent,ControlGUI pControlGUI,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
		{
			if (MethodsGUI.IsOptionalControl(pCurrent,pFldCurrent))
				return (new OptionalItem(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor));
			else
				return (new ObjectArray(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor));
		}
		#endregion DisplayArray 

		#region _Value
        public static object INIT_Value(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new DateCalendar(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor)); 
		}
		#endregion _Value
		#endregion Methods
	}
	#endregion EFS_DateArray
	#region EFS_DateTimeArray
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
	public class EFS_DateTimeArray : EFS_DateBase,IEFS_Array
	{
		#region Constructors
		public EFS_DateTimeArray()              : base(XSDTypeData.DateTime){}
		public EFS_DateTimeArray(string pValue) : base(XSDTypeData.DateTime,pValue){}
		#endregion Constructors
		#region Methods
		#region DisplayArray
		public object DisplayArray(object pCurrent,FieldInfo pFldCurrent,object pParent,FieldInfo pFldParent,ControlGUI pControlGUI,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
		{
			if (MethodsGUI.IsOptionalControl(pCurrent,pFldCurrent))
				return (new OptionalItem(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor));
			else
				return (new ObjectArray(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor));
		}
		#endregion DisplayArray 
		#region _Value
        public static object INIT_Value(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new DateCalendar(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor)); 
		}
		#endregion _Value
		#endregion Methods
	}
	#endregion EFS_DateTimeArray
	#endregion Date Classes
	#region Number Classes
	#region EFS_NumberBase
    [Serializable]
	public abstract class EFS_NumberBase : StringGUI 
	{
		//20050919 PL/FDA Value contient maintenant une donnée au format FpML (autrefois une donnée au format CurrentCulture)
		[System.Xml.Serialization.XmlTextAttribute()]
		public string Value;

		#region Accessors
		#region CultureValue
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public virtual string CultureValue
		{
			get {return StrFunc.FmtDecimalToCurrentCulture(Value);}
		}
		#endregion CultureValue

		#region DecValue
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public decimal DecValue
        {
            get
            {
                if (StrFunc.IsEmpty(this.Value))
                    return decimal.Zero;
                else
                    return DecFunc.DecValue(Value, CultureInfo.InvariantCulture);
            }
            set
            {
                Value = StrFunc.FmtDecimalToInvariantCulture(value);
            }
        }
		#endregion DecValue
		#region IntValue
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public int IntValue
        {
            get
            {
                int ret;
                try
                {
                    ret = IntFunc.IntValue2(this.Value, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    throw; 
                }
                return ret;
            }
            set { this.Value = value.ToString(); }
        }
		#endregion IntValue
        #region LongValue
        // EG 20150920 [21314] new
        // EG 20180423 Analyse du code Correction [CA2200]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public long LongValue
        {
            get
            {
                long ret;
                try
                {
                    ret = IntFunc.IntValue64(this.Value, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    throw;
                }
                return ret;
            }
            set { this.Value = value.ToString(); }
        }
        #endregion LongValue
        #endregion Accessors
		#region Constructors
		public EFS_NumberBase(){}
		public EFS_NumberBase(string pType)
		{
			ReflectionTools.SetDataTypeAttribute(this,pType);
		}

		public EFS_NumberBase(string pType,string pValue)
		{
			ReflectionTools.SetDataTypeAttribute(this,pType);
			Value = pValue;
		}
		public EFS_NumberBase(string pType,int pValue)
		{
			ReflectionTools.SetDataTypeAttribute(this,pType);
			IntValue = pValue;
		}
        public EFS_NumberBase(string pType, decimal pValue)
        {
            ReflectionTools.SetDataTypeAttribute(this, pType);
            DecValue = pValue;
        }
		public EFS_NumberBase(string pType,long pValue)
		{
			ReflectionTools.SetDataTypeAttribute(this,pType);
            LongValue = pValue;
		}
		#endregion Constructors
	}
	#endregion EFS_NumberBase
	#region EFS_Integer
    // EG 20150920 [21314] Int (int32) to Long (Int64)
	public class EFS_Integer : EFS_NumberBase,ICloneable
	{
		#region Constructors
		public EFS_Integer()              : base(XSDTypeData.Integer){}
		public EFS_Integer(string pValue) : base(XSDTypeData.Integer,pValue){}
        // EG 20150920 [21314]
        public EFS_Integer(long pValue) : base(XSDTypeData.Integer, pValue) { }
		#endregion Constructors
		#region Methods
        #region CultureValue
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string CultureValue
        {
            get { return StrFunc.FmtIntegerToCurrentCulture(Value); }
        }
        #endregion CultureValue
		#region Clone
		public object Clone()
		{
			return new EFS_Integer(this.Value);
		}
		#endregion Clone
		#endregion Methods
	}
	#endregion EFS_Integer
	#region EFS_Decimal
    [Serializable()]
	public class EFS_Decimal : EFS_NumberBase, ICloneable
	{
		//20050919 PL/FDA Modif de EFS_NumberBase
		#region Constructors
		public EFS_Decimal()               : base(XSDTypeData.Decimal){}
		public EFS_Decimal(string pValue)  : base(XSDTypeData.Decimal,pValue){}
		public EFS_Decimal(decimal pValue) : base(XSDTypeData.Decimal,pValue){}
		#endregion Constructors
		#region Methods
        // 20120821 MF - Ticket 18073
        /// <summary>
        /// Get the string representation of the decimal value, converting the fractional part to the given base and style
        /// </summary>
        /// <param name="pDisplayBase">numerical base used to display the value. 
        /// When this parameter is not a valid positive integer value, then it will be overwritten with the current base value</param>
        /// <param name="pCurrentBase">current numerical base of the value. 
        /// When this parameter is not a valid positive integer value, then 100 will be used</param>
        /// <param name="pMultiplier">multiplier to apply to the current value before performing the input conversion and style.
        /// When this parameter is not a valid positive integer value, then 1 will be used</param>
        /// <param name="pFormatStyle">style to be applied to the converted value</param>
        /// <returns>The string of the decimal value with the fractional part converted according to the input parameters.</returns>
        // FI 20150218 [20275]Modify
        // PL 20150417 Refactoring
        // FI 20161109 [21539] Modify 
        public string ToConvertedFractionalPartString(int pDisplayBase, int pCurrentBase, decimal pMultiplier, Cst.PriceFormatStyle pFormatStyle)
        {
            #region DEBUG code sample
            //Code suivant à copier/coller et utiliser dans une méthode main (ex. Default.aspx.cs\Page_Load())

            //EFS.GUI.Interface.EFS_Decimal x = new EFS.GUI.Interface.EFS_Decimal();
            //string convValue;
            //int displayBase, currentBase;
            //displayBase = currentBase = 100;
            //System.Diagnostics.Debug.Print("Format".PadRight(15, ' ') + "In Base".PadRight(10, ' ') + "Out Base".PadRight(10, ' ')
            //    + "In Value".PadRight(15, ' ') + "Out Value");
            //for (int i = 0; i <= 11; i++)
            //{
            //    if (i == 0)
            //    {
            //        displayBase = currentBase = 100;
            //        x.DecValue = 123.5M;
            //    }

            //    if (i == 1)
            //    {
            //        displayBase = currentBase = 100;
            //        x.DecValue = 123.56789M;
            //    }

            //    else if (i == 2)
            //    {
            //        currentBase = 100;
            //        displayBase = 32;
            //        x.DecValue = 123.50M;
            //    }
            //    else if (i == 3)
            //    {
            //        displayBase = currentBase = 32;
            //        x.DecValue = 123.16M;
            //    }
            //    else if (i == 4)
            //    {
            //        currentBase = 32;
            //        displayBase = 64;
            //        x.DecValue = 123.16M;
            //    }

            //    else if (i == 5)
            //    {
            //        currentBase = 100;
            //        displayBase = 32;
            //        x.DecValue = 123.5078125M;
            //    }
            //    else if (i == 6)
            //    {
            //        displayBase = currentBase = 32;
            //        x.DecValue = 123.1625M;
            //    }
            //    else if (i == 7)
            //    {
            //        currentBase = 32;
            //        displayBase = 64;
            //        x.DecValue = 123.1625M;
            //    }

            //    else if (i == 8)
            //    {
            //        currentBase = 100;
            //        displayBase = 32;
            //        x.DecValue = 123.515625M;
            //    }
            //    else if (i == 9)
            //    {
            //        currentBase = 100;
            //        displayBase = 64;
            //        x.DecValue = 123.515625M;
            //    }
            //    else if (i == 10)
            //    {
            //        currentBase = 32;
            //        displayBase = 64;
            //        x.DecValue = 123.165M;
            //    }
            //    else if (i == 11)
            //    {
            //        currentBase = 32;
            //        displayBase = 128;
            //        x.DecValue = 123.165M;
            //    }

            //    System.Diagnostics.Debug.Print("");
            //    foreach (Cst.PriceFormatStyle formatStyle in Enum.GetValues(typeof(Cst.PriceFormatStyle)))
            //    {
            //        convValue = x.ToConvertedFractionalPartString(displayBase, currentBase, 1, formatStyle);
            //        System.Diagnostics.Debug.Print(formatStyle.ToString().PadRight(15, ' ') + currentBase.ToString().PadRight(10, ' ') + displayBase.ToString().PadRight(10, ' ')
            //             + x.DecValue.ToString().PadRight(15, ' ') + convValue);
            //    }
            //}
            #endregion DEBUG code sample
            if (pCurrentBase <= 0)
                pCurrentBase = 100;
            if (pDisplayBase <= 0)
                pDisplayBase = pCurrentBase;
            if (pMultiplier <= 0)
                pMultiplier = 1;

            // 0. Apply the input multiplier
            decimal price = this.DecValue * pMultiplier;
            string convertedPrice; 

            // 1. Finding the decimal part of the value to be converted
            int integerPart = (int)decimal.Truncate(price);
            decimal fractionPart = price - integerPart;

            if ((pDisplayBase == pCurrentBase) && (pCurrentBase == 100) && (pFormatStyle == Cst.PriceFormatStyle.Default))
            {
                //Newness (PL 20150417): Sortie "immédiate" si aucune spécificité. 
                string strFractionPart = "0000";
                // FI 20151109 [21539]
                // Lorsque la variable fractionPart vaut zéro, la méthode .ToString() retourne généralement "0.00"  mais parfois elle retourne "0" et il y a alors plantage (plantage lors de fractionPart.ToString().Substring(2))
                // De plus decimal.Zero.ToString() retourne systématiquement "0"
                // => Mise en place d'un test (fractionPart == decimal.Zero) pour éviter tout plantage
                //string strFractionPart = fractionPart.ToString().Substring(2).PadRight(4, '0');
                if (fractionPart != decimal.Zero)
                    strFractionPart = fractionPart.ToString().Substring(2).PadRight(4, '0');

                convertedPrice = String.Format("{0}.{1:###0}", integerPart, strFractionPart);
            }
            else
            {
                #region Conversion and Apply the style
                // 2. Conversion 
                // 2.1 Value conversion to the default base (base 100)
                decimal convFractionPart = (fractionPart * 100) / pCurrentBase;

                // 2.2 Conversion from base 100 to the display base 
                //     WARNING: The conversion procedure stop at the first step, at the first multiplication, according to the market calculation.
                //              The real conversion of the fraction part should be defined with a recursive procedure as like that is affirmed to 
                //              the next url http://www.cut-the-knot.org/blue/frac_conv.shtml
                convFractionPart *= pDisplayBase;

                // UNDONE 20120821 MF - le point 3 suivant n'a pas été vraiment COMPRIS, ça semble que si la partie fractionnaire convertie
                //                      contient elle-même une partie fractionnaire MAIS on veut afficher un numéro entier à l'écran, alors
                //                      la prémière chiffre de la partie fractionnaire - de la partie fractionnaire convertie - 
                //                      doit être enchainée à la partie entiére de la partie fractionnaire convertie. 
                //                      Ex: 
                //                      - partie fractionnaire en base decimal à convertir : 0.5078125, 
                //                      - base de conversion : 32, 
                //                      - valeur convertie : (0.5078125 * 32) = 16.25
                //                      - valeur entière : 162

                // 3. Evaluation of the converted fractional part as integer (see also above UNDONE)
                int convFractionPart_SubIntPart = (int)decimal.Truncate(convFractionPart);
                if (convFractionPart - convFractionPart_SubIntPart > 0)
                {
                    // 3.1 adding the first decimal to the integer part and ignore the rest
                    convFractionPart_SubIntPart = (int)decimal.Truncate(convFractionPart * 10);
                }

                // 4. Find the max number of chars of the display base, and define the string format (integerPartStringFormat)
                #region Etape obsolète suite au rafctoring. Etape intégrée dans l'étape 5. 
                //    for the numerator of the numerator in the fraction styles.
                //    Ex: 
                //    - Base  32, max number: 2, string format of numerator: "00"
                //    - Base 128, max number: 3, string format of numerator: "000"
                //int baseCharsNumber = pDisplayBase.ToString().Length;
                //string integerPartStringFormat = pDisplayBase.ToString().ToCharArray().Select(elem => "0").Aggregate((elem, next) => elem + next);

                // 4.1 In case we have a fractional part after the conversion of the fractional part, we force a visualization on 2 digits ".##"
                // 4.2 The format xsl-fo inline present no namespace (fo:) because the serialization of message do not allow use of namespaces
                //     but the fix and efsml. Rather than inserting the fo namespace in the CacheSerializer object which could degrade performances,
                //     we use HTML nodes <sup> and <sub>. Theses nodes will be replaced by
                //     - <fo:inline vertical-align="super" font-size="75%"> for <sup>
                //     - <fo:inline vertical-align="sub" font-size="75%"> for <sub>
                //    in the xsl reports

                // FI 20150218 [20275] Suppression de sup et sub et &frasl;
                //string outputStringFormat = @"{0} <sup>{1:" + integerPartStringFormat + @".##}</sup>&frasl;<sub>{2}</sub>";
                //string outputStringFormat = @"{0} {1:" + new String('0', pDisplayBase.ToString().Length) + @".##}/{2}";
                #endregion Etape obsolète

                // 5. Apply the style
                switch (pFormatStyle)
                {
                    case Cst.PriceFormatStyle.Fraction:
                    case Cst.PriceFormatStyle.FractionDecimal:
                        convertedPrice = convFractionPart > 0 ?
                            String.Format(@"{0} {1:" + new String('0', pDisplayBase.ToString().Length) + @".##}/{2}", 
                                            integerPart,
                                            ((pFormatStyle == Cst.PriceFormatStyle.FractionDecimal) ? convFractionPart : convFractionPart_SubIntPart), 
                                            pDisplayBase)
                            :
                            String.Format("{0}", integerPart);
                        break;
                    case Cst.PriceFormatStyle.Apostrophe:
                        convertedPrice = String.Format("{0}'{1}", integerPart, convFractionPart_SubIntPart);
                        break;
                    case Cst.PriceFormatStyle.Minus:
                        convertedPrice = String.Format("{0}-{1}", integerPart, convFractionPart_SubIntPart);
                        break;
                    case Cst.PriceFormatStyle.Power:
                        convertedPrice = String.Format("{0}^{1}", integerPart, convFractionPart_SubIntPart);
                        break;
                    case Cst.PriceFormatStyle.Default:
                    default:
                        #region code obsolète réécrit par RD 20130616 [18320]
                        // RD 20130616 [18320] Prendre en considération les 0 à gauche de la partie décimale
                        // Ex. le code erroné ci-dessous affiche 0.5 pour 0.005 et affiche 0.1 pour 0.01 

                        //// The default style is treated to have at least 4 decimal (fraction part * 10^4 and format: ####), in order to be compliant with the max decimals treated by the xsl level
                        //convFractionPartIntegerPart = (int)decimal.Truncate(convFractionPart * 10000);

                        //for (; convFractionPartIntegerPart > 0 && convFractionPartIntegerPart % 10 == 0; )
                        //{ convFractionPartIntegerPart = convFractionPartIntegerPart / 10; }

                        //// UNDONE 20120822 MF - prochain step, passer la base de conversion au niveau xsl pour ne plus s'appuyer sur la présence d'une séquence spéciale de caractères (**) 
                        ////                      dans le template FormatConvertedPriceValue. Par le biais de la valeur de la base on pourra choisir quel formatage on va utiliser.

                        //// The default style use ** as separator. It is a special char in order to use the default xsl/Spheres formatting (culture specific) onto the converted value.
                        //convertedPrice = String.Format("{0}**{1:###0}", integerPart, convFractionPartIntegerPart);
                        #endregion

                        convFractionPart_SubIntPart = (int)decimal.Truncate(convFractionPart);
                        decimal convFractionPart_SubDecPart = convFractionPart - convFractionPart_SubIntPart;

                        // The default style is treated to have at least 4 decimal (fraction part 2 carateres and integer part 2 carateres), 
                        // in order to be compliant with the max decimals treated by the xsl level
                        string convFractionPartInt = convFractionPart_SubIntPart.ToString().PadLeft(2, '0');
                        string convFractionPartDec = ((int)decimal.Truncate(convFractionPart_SubDecPart * 100)).ToString().PadLeft(2, '0').Substring(0, 2);

                        string fractionPartDisplay = (convFractionPartInt + convFractionPartDec).TrimEnd('0');

                        if (StrFunc.IsFilled(fractionPartDisplay))
                        {
                            //// The default style use ** as separator. It is a special char in order to use the default xsl/Spheres formatting (culture specific) onto the converted value.
                            //convertedPrice = String.Format("{0}**{1:###0}", integerPart, fractionPartDisplay);

                            //PL 20150219 On ne retient plus l'usage du séparateur "**", mais celui du séparateur international "." (see also XSL template CheckDefaultFormatNumberV2)
                            convertedPrice = String.Format("{0}.{1:###0}", integerPart, fractionPartDisplay);
                        }
                        else
                        {
                            convertedPrice = integerPart.ToString();
                        }
                        break;
                }
                #endregion 
            }
            return convertedPrice;
        }

		#region Clone
		public object Clone()
		{
			return new EFS_Decimal(Value);
		}
		#endregion Clone
		#endregion Methods
	}
	#endregion EFS_Decimal
    #region EFS_NonNegativeDecimal
    /// EG 20161122 New Commodity Derivative
    public class EFS_NonNegativeDecimal : EFS_NumberBase, ICloneable
    {
        #region Constructors
        public EFS_NonNegativeDecimal() : base(XSDTypeData.NonNegativeDecimal) { }
        public EFS_NonNegativeDecimal(string pValue) : base(XSDTypeData.NonNegativeDecimal, pValue) { }
        public EFS_NonNegativeDecimal(decimal pValue) : base(XSDTypeData.NonNegativeDecimal, pValue) { }
        #endregion Constructors
        #region Methods
        #region Clone
        public object Clone()
        {
            return new EFS_NonNegativeDecimal(this.Value);
        }
        #endregion Clone
        #endregion Methods
    }
    #endregion EFS_NonNegativeDecimal
    #region EFS_NegativeInteger
    public class EFS_NegativeInteger : EFS_NumberBase,ICloneable
	{
		#region Constructors
		public EFS_NegativeInteger()              : base(XSDTypeData.NegativeInteger){}
		public EFS_NegativeInteger(string pValue) : base(XSDTypeData.NegativeInteger,pValue){}
		public EFS_NegativeInteger(int pValue)    : base(XSDTypeData.NegativeInteger,pValue){}
		#endregion Constructors
		#region Methods
		#region Clone
		public object Clone()
		{
			return new EFS_NegativeInteger(this.Value);
		}
		#endregion Clone
		#endregion Methods
	}
	#endregion EFS_NegativeInteger
	#region EFS_NonNegativeInteger
	public class EFS_NonNegativeInteger : EFS_NumberBase,ICloneable
	{
		#region Constructors
		public EFS_NonNegativeInteger()              : base(XSDTypeData.NonNegativeInteger){}
		public EFS_NonNegativeInteger(string pValue) : base(XSDTypeData.NonNegativeInteger,pValue){}
		public EFS_NonNegativeInteger(int pValue)    : base(XSDTypeData.NonNegativeInteger,pValue){}
		#endregion Constructors
		#region Methods
		#region Clone
		public object Clone()
		{
			return new EFS_NonNegativeInteger(this.Value);
		}
		#endregion Clone
		#endregion Methods
	}
	#endregion EFS_NonNegativeInteger
    #region EFS_PositiveDecimal
    /// EG 20161122 New Commodity Derivative
    public class EFS_PositiveDecimal : EFS_NumberBase, ICloneable
    {
        #region Constructors
        public EFS_PositiveDecimal() : base(XSDTypeData.PositiveDecimal) { }
        public EFS_PositiveDecimal(string pValue) : base(XSDTypeData.PositiveDecimal, pValue) { }
        public EFS_PositiveDecimal(decimal pValue) : base(XSDTypeData.PositiveDecimal, pValue) { }
        #endregion Constructors
        #region Methods
        #region Clone
        public object Clone()
        {
            return new EFS_PositiveDecimal(this.Value);
        }
        #endregion Clone
        #endregion Methods
    }
    #endregion EFS_PositiveDecimal	
    #region EFS_PosInteger
	public class EFS_PosInteger : EFS_NumberBase,ICloneable
	{
		#region Constructors
		public EFS_PosInteger()              : base(XSDTypeData.PosInteger){}
		public EFS_PosInteger(string pValue) : base(XSDTypeData.PosInteger,pValue){}
		public EFS_PosInteger(int pValue)    : base(XSDTypeData.PosInteger,pValue){}
		#endregion Constructors
		#region Methods
        #region CultureValue
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override string CultureValue
        {
            get { return StrFunc.FmtIntegerToCurrentCulture(Value); }
        }
        #endregion CultureValue

		#region Clone
		public object Clone()
		{
			return new EFS_PosInteger(this.Value);
		}
		#endregion Clone
		#endregion Methods
	}
	#endregion EFS_PosInteger
    #endregion Number Classes
    #region String Classes
    #region EFS_StringBase
    public class EFS_StringBase : StringGUI
	{
		[System.Xml.Serialization.XmlTextAttribute()]
		public string Value;

		#region Constructors
		public EFS_StringBase(){}
		public EFS_StringBase(string pType)
		{
			ReflectionTools.SetDataTypeAttribute(this,pType);
		}
		public EFS_StringBase(string pType,string pValue)
		{
			ReflectionTools.SetDataTypeAttribute(this,pType);
			this.Value = pValue;
		}
		#endregion Constructors
	}
	#endregion EFS_StringBase
    #endregion

	#region EFS_Attribute
	public class EFS_Attribute : StringGUI
	{
		public string Value;

		#region Constructors
		public EFS_Attribute() {}
		public EFS_Attribute(string pValue){this.Value = pValue;}
		#endregion Constructors
	}
	#endregion EFS_Attribute
	#region EFS_Boolean
    [Serializable()]
	public class EFS_Boolean  : BooleanGUI
	{
		[System.Xml.Serialization.XmlTextAttribute()]
		public string Value;

		#region Accessors
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public System.Boolean BoolValue
		{
            //PL 20091229 Bug on Oracle® (Boolean field is NUMBER(1) and data is 0 or 1)
            //get {return Convert.ToBoolean(this.Value);}
            get { return BoolFunc.IsTrue(this.Value); }
			//20060531 PL Add ToLower()
			set {this.Value = value.ToString().ToLower();}
		}
		#endregion Accessors
		#region Constructors
		public EFS_Boolean(){}
		public EFS_Boolean(bool pValue){BoolValue = pValue;}
		#endregion Constructors
	}
	#endregion EFS_Boolean
	#region EFS_RadioChoice
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
	public class EFS_RadioChoice : IEFS_Choice
	{
		#region Methods
		#region DisplayChoice
		public object DisplayChoice(object pCurrent,FieldInfo pFldCurrent,object pParent,FieldInfo pFldParent,ControlGUI pControlGUI,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
		{
			return (new Choice(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,0,pFullCtor));
		}
		#endregion DisplayChoice
		#endregion Methods
	}
	#endregion EFS_RadioChoice
	#region EFS_DropDownChoice
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
	public class EFS_DropDownChoice : IEFS_Choice
	{
		#region Methods
		#region DisplayChoice
		public object DisplayChoice(object pCurrent,FieldInfo pFldCurrent,object pParent,FieldInfo pFldParent,ControlGUI pControlGUI,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
		{
			return (new Choice(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,1,pFullCtor));
		}
		#endregion DisplayChoice
		#endregion Methods
	}
	#endregion EFS_Choice
	#region EFS_Href
	public class EFS_Href : HrefGUI 
	{
		public string Value;
		#region Constructors
		public EFS_Href() {}
		public EFS_Href(string pValue){this.Value = pValue;}
		#endregion Constructors
	}
	#endregion EFS_Href
	#region EFS_Id
	public class EFS_Id : StringGUI
	{
		public string Value;
		#region Constructors
		public EFS_Id() {}
		public EFS_Id(string pValue){this.Value = pValue;}
		#endregion Constructors
	}
	#endregion EFS_Id
	#region EFS_MonthYear
	public class EFS_MonthYear : StringGUI
	{
		[System.Xml.Serialization.XmlTextAttribute(DataType="anyURI")]
		public string Value;
		#region Constructors
		public EFS_MonthYear() {}
		public EFS_MonthYear(string pValue){this.Value = pValue;}
		#endregion Constructors
	}
	#endregion EFS_MonthYear
	#region EFS_Scheme
	public class EFS_Scheme : SchemeGUI
	{
		[System.Xml.Serialization.XmlTextAttribute(DataType="anyURI")]
		public string Value;
	}
	#endregion EFS_Scheme
	#region EFS_SchemeValue
	public class EFS_SchemeValue : SchemeValueGUI {}
	#endregion EFS_String
	#region EFS_String
	public class EFS_String : StringGUI
	{
		[System.Xml.Serialization.XmlTextAttribute(DataType="anyURI")]
		public string Value;

		#region Constructors
		public EFS_String() {}
		public EFS_String(string pValue){this.Value = pValue;}
		#endregion Constructors
	}
	#endregion EFS_String
	#region EFS_StringArray
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
	public class EFS_StringArray : IEFS_Array
	{
		[System.Xml.Serialization.XmlTextAttribute(DataType="anyURI")]
		[ControlGUI(Name="value",LineFeed=MethodsGUI.LineFeedEnum.After,Width=400)]
		public string Value;

		#region Constructors
		public EFS_StringArray() {Value = string.Empty;}
		public EFS_StringArray(string pValue){this.Value = pValue;}
		#endregion Constructors

		#region Methods
		#region DisplayArray
		public object DisplayArray(object pCurrent,FieldInfo pFldCurrent,object pParent,FieldInfo pFldParent,ControlGUI pControlGUI,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
		{
			if (MethodsGUI.IsOptionalControl(pCurrent,pFldCurrent))
				return (new OptionalItem(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor));
			else
				return (new ObjectArray(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor));
		}
		#endregion DisplayArray
		#region _Value
        public static object INIT_Value(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new SimpleString(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor));
		}
		#endregion _Value
		#endregion Methods
	}
	#endregion EFS_StringArray
	#region EFS_MultiLineString
	public class EFS_MultiLineString : StringGUI
	{
		[System.Xml.Serialization.XmlTextAttribute(DataType="anyURI")]
		public string Value;

		#region Constructors
		public EFS_MultiLineString() {}
		public EFS_MultiLineString(string pValue){this.Value = pValue;}
		#endregion Constructors
	}
	#endregion EFS_MultiLineString
	#region EFS_Time
	public class EFS_Time : StringGUI
	{
		[System.Xml.Serialization.XmlTextAttribute()]
		public string Value;

		#region Accessors
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public System.DateTime TimeValue
		{
			set {this.Value=DtFunc.DateTimeToString(value, DtFunc.FmtISOTime);}
			get {return new DtFunc().StringToDateTime(Value);}
		}
		#endregion Accessors
		#region Constructors
		public EFS_Time() {}
		public EFS_Time(string pValue) {this.Value = pValue;}
		#endregion Constructors
	}
	#endregion EFS_Time
	#region HistoryBalise
	public class HistoryBalise
	{
		private string m_Name;
		private bool   m_IsVisible;

		#region Accessors
		public string Name
		{
			get {return(m_Name);}
			set {m_Name = value;}
		}

		public bool IsVisible
		{
			get {return(m_IsVisible);}
			set {m_IsVisible = value;}
		}
		#endregion Accessors
		#region Constructors
		public HistoryBalise(string pName,bool pIsVisible,int pIndex)
		{
			m_Name      = pName;
			m_IsVisible = pIsVisible;
			if (0 < pIndex)
				m_Name += pIndex.ToString();
		}
		#endregion Constructors
	}
	#endregion HistoryBalise
	#region EFS_EnumArray
	/// <summary>
	/// Classe pour gestion interface full FpML pour Array d'enum
	/// </summary>
	// EG 20230808 [26454] New
	public class EFS_EnumArray : IEFS_Array
	{
		[System.Xml.Serialization.XmlTextAttribute(DataType = "anyURI")]
		[ControlGUI(Name = "value", LineFeed = MethodsGUI.LineFeedEnum.After, Width = 400)]
		public string Value;

		#region Constructors
		public EFS_EnumArray() { Value = string.Empty; }
		public EFS_EnumArray(string pValue) { this.Value = pValue; }
		#endregion Constructors

		#region Methods
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
				return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
			else
				return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#region _Value
		public static object INIT_Value(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return new SimpleEnum(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor);
		}
		#endregion _Value
		#endregion Methods
	}
	#endregion EFS_EnumArray
	#endregion Substitution Classes

	#region GUI Interface calling for FpML
	#region BaseGUI
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
    [Serializable()]
    public class BaseGUI
	{
		#region Methods
		#region Optional
		public object Optional(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
		{
			return (new OptionalItem(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor));            
		}
		#endregion Optional
		#endregion Methods
	}
	#endregion BaseGUI

	#region BooleanGUI
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
    [Serializable]
	public class BooleanGUI : BaseGUI,IEFS_Control
	{
		#region Methods
		#region Mandatory
		public object Mandatory(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent,ControlGUI pControlGUI,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
		{
			return (new SimpleBoolean(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor));
		}
		#endregion Mandatory
		#endregion Methods
	}
	#endregion BooleanGUI
	#region DateCalendarGUI
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
	public class DateCalendarGUI : BaseGUI,IEFS_Control
	{
		#region Methods
		#region Mandatory
		public object Mandatory(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
		{
			return (new DateCalendar(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor));
		}
		#endregion Mandatory
		#endregion Methods
	}
	#endregion DateCalendarGUI
	#region EmptyGUI
	public class EmptyGUI : ItemGUI,IEFS_Control {}
	#endregion EmptyGUI
	#region HrefGUI
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
	public class HrefGUI : BaseGUI,IEFS_Control
	{
		#region Methods
		#region Mandatory
		public object Mandatory(object pCurrent,FieldInfo pFldCurrent,object pParent,FieldInfo pFldParent,ControlGUI pControlGUI,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
		{
			return (new Href(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor));
		}
		#endregion Mandatory
		#endregion Methods
	}
	#endregion HrefGUI
	#region ItemGUI
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
	public class ItemGUI : BaseGUI, IEFS_Control
	{
		#region Methods
		#region Mandatory
		public object Mandatory(object pCurrent,FieldInfo pFldCurrent,object pParent,FieldInfo pFldParent,ControlGUI pControlGUI,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
		{
			return (new Item(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor));
		}
		#endregion Mandatory
		#endregion Methods
	}
	#endregion ItemGUI
	#region NodeGUI
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
	public class NodeGUI : BaseGUI,IEFS_Control
	{
		#region Methods
		#region Mandatory
		public object Mandatory(object pCurrent,FieldInfo pFldCurrent,object pParent,FieldInfo pFldParent,ControlGUI pControlGUI,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
		{
			return (new Node(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor));
		}
		#endregion Mandatory
		#endregion Methods
	}
	#endregion NodeGUI
	#region PartyIdGUI
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
	public class PartyIdGUI : BaseGUI,IEFS_Control
	{
		#region Methods
		#region Mandatory
		public object Mandatory(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
		{
			return (new PartyId(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor));
		}
		#endregion Mandatory
		#endregion Methods
	}
	#endregion PartyIdGUI
	#region ReferenceBankGUI
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
	public class ReferenceBankGUI : BaseGUI,IEFS_Control
	{
		#region Methods
		#region Mandatory
		public object Mandatory(object pCurrent,FieldInfo pFldCurrent,object pParent,FieldInfo pFldParent,ControlGUI pControlGUI,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
		{
			return (new ReferenceBankControl(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor));
		}
		#endregion Mandatory
		#endregion Methods
	}
	#endregion ReferenceBankGUI
	#region SchemeGUI
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
	public class SchemeGUI : BaseGUI,IEFS_Control
	{
		#region Methods
		#region Mandatory
		public object Mandatory(object pCurrent,FieldInfo pFldCurrent,object pParent,FieldInfo pFldParent,ControlGUI pControlGUI,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
		{
			return (new Scheme(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor));
		}
		#endregion Mandatory
		#endregion Methods
	}
	#endregion SchemeGUI
	#region SchemeBoxGUI
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
	public class SchemeBoxGUI : BaseGUI,IEFS_Control
	{
		#region Methods
		#region Mandatory
		public object Mandatory(object pCurrent,FieldInfo pFldCurrent,object pParent,FieldInfo pFldParent,ControlGUI pControlGUI,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
		{
			return (new SchemeBox(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor));
		}
		#endregion Mandatory
		#endregion Methods
	}
	#endregion SchemeBoxGUI
	#region SchemeIdGUI
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
	public class SchemeIdGUI : BaseGUI,IEFS_Control
	{
		#region Methods
		#region Mandatory
		public object Mandatory(object pCurrent, FieldInfo pFldCurrent,object pParent,FieldInfo pFldParent,ControlGUI pControlGUI,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
		{
			return (new SchemeId(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor));
		}
		#endregion Mandatory
		#endregion Methods
	}
	#endregion SchemeIdGUI
	#region SchemeTextGUI
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
	public class SchemeTextGUI : BaseGUI,IEFS_Control
	{
		#region Methods
		#region Mandatory
		public object Mandatory(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
		{
			return (new SchemeText(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor));
		}
		#endregion Mandatory
		#endregion Methods
	}
	#endregion SchemeTextGUI
	#region SchemeValueGUI
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
	public class SchemeValueGUI : BaseGUI,IEFS_Control
	{
		#region Methods
		#region Mandatory
		public object Mandatory(object pCurrent,FieldInfo pFldCurrent,object pParent,FieldInfo pFldParent,ControlGUI pControlGUI,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
		{
			return (new ElementId(pCurrent,pFldCurrent, pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor));
		}
		#endregion Mandatory
		#endregion Methods
	}
	#endregion SchemeValueGUI
	#region StringGUI
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
    [Serializable()]
	public class StringGUI : BaseGUI,IEFS_Control
	{
		#region Methods
		#region Mandatory
		public object Mandatory(object pCurrent,FieldInfo pFldCurrent,object pParent,FieldInfo pFldParent,ControlGUI pControlGUI,object pGrandParent,FieldInfo pFldGrandParent,FullConstructor pFullCtor)
		{
			return (new SimpleString(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor));
		}
		#endregion Mandatory
		#endregion Methods
	}
	#endregion StringGUI
	#region ExtendEnumsGUI
	/// <revision>
	///     <version>1.2.0</version><date>20071003</date><author>EG</author>
	///     <comment>
	///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent (used to determine REGEX type for derived classes
	///     </comment>
	/// </revision>
	public class ExtendEnumsGUI : ExtendEnums
	{
        public static object Mandatory(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new Scheme(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor));
		}

        public static object Optional(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new OptionalItem(pCurrent,pFldCurrent,pControlGUI,pParent,pFldParent,pGrandParent,pFldGrandParent,pFullCtor));
		}

		public ExtendEnumsGUI(ExtendEnum[] pItems):base(pItems){}
	}  
	#endregion ExtendEnumsGUI

    #region DateTimeOffsetGUI
    // EG 20170918 [23342] New class for MiFID II
    public class DateTimeOffsetGUI : BaseGUI, IEFS_Control
    {
        #region Methods
        #region Mandatory
        public object Mandatory(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new DateTimeOffsetCalendar(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Mandatory
        #endregion Methods
    }
    #endregion DateTimeOffsetGUI

	#endregion GUI Interface calling for FpML
}
