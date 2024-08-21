using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

using EFS.ACommon;



namespace EFS.Common
{
	#region public class Rate
	abstract public class Rate
	{
		#region Members
		private decimal _value;
		private bool    _isError = false;
		#endregion Members
		
		#region  Accessor
		public bool    IsError
		{
			get {return _isError;}
		}
		public decimal Value
		{
			get {return _value;}
			set 
			{
				_isError = false;
				_value   = value;
			}
		}
		public string  StringValue
		{
			get {return this.ToString();}
			set 
			{
				_isError = false;
				StringToDecimal(value, Thread.CurrentThread.CurrentCulture );
			}
		}
		#endregion

		#region Constructors
		public Rate() 
		{
			Value = 0;
		}
		public Rate(decimal pValue)
		{
			Value = pValue;
		}
		public Rate(string pValue): this(pValue,Thread.CurrentThread.CurrentCulture){}
		public Rate(string pValue,CultureInfo pCultureInfo )
		{
			StringToDecimal(pValue, pCultureInfo );
		}
		#endregion Constructors
		
		#region protected StringToDecimal
		protected void StringToDecimal(string pRate, CultureInfo  pCultureInfo)
		{
			int coeff = 1;
			string rate = string.Empty; 
			//
			if (StrFunc.IsFilled(pRate))
				rate= pRate.Trim();  
			//
			if (rate.EndsWith("%"))
			{
				rate  = rate.Replace("%", string.Empty);
				coeff = 100;
			}
			_value = DecFunc.DecValue(rate, pCultureInfo)/ coeff;
		}
		#endregion protected StringToDecimal
		#region public virtual ToString (int pDecimal
		public virtual  string ToString(int pDecimal){return Value.ToString();}
		#endregion
		#region public override ToString
		public  override string ToString()
		{
            int dec = 0;
            decimal val;
            //
            if (this.GetType().Equals(typeof(FixedRate)))
                // Cas particulier sur les taux fixe exprimé en %			
                val = this.Value * 100; // 0.025 => 2.5% => nbr de decimal après la virgule 1 (et non pas 3)  
            else
                val = this.Value;
            //
            int guard = 0;
            try
            {
                while (val != Convert.ToInt64(val) && (++guard < 10))
                {
                    dec++;
                    val *= 10;
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("Problem With value=" + Value.ToString(CultureInfo.InvariantCulture));
                Debug.WriteLine("             val=" + val.ToString(CultureInfo.InvariantCulture));
                throw;
            }
			return this.ToString(Math.Max(2, dec));
		}
		#endregion
		#region public ToStringZero
		public string ToStringZero()
		{
			if (Value == 0)
				return 0.ToString();
			else
				return this.ToString();
		}
		public string ToStringZero(int pDecimal)
		{
			if (Value == 0)
				return 0.ToString(pDecimal.ToString());
			else
				return this.ToString(pDecimal);
		}
		#endregion public ToStringZero
		#region public Round
        // EG 20130315 Ajout du paramètre MidpointRounding.AwayFromZero pour être en phase avec Arrondi Nearest de type FpML.
        // Exemples:
        // Decimal.round(3.45, 1, MidpointRounding.AwayFromZero) = 3.5
        // Decimal.round(3.45, 1, MidpointRounding.ToEven) = 3.4
        // Decimal.round(3.55, 1, MidpointRounding.AwayFromZero) = 3.6
        // Decimal.round(3.55, 1, MidpointRounding.ToEven) = 3.6
		public decimal Round(int pDecimal)
		{
			return Decimal.Round(this.Value, pDecimal, MidpointRounding.AwayFromZero);
		}
		#endregion
		#region public Truncate
		public decimal Truncate(int pDecimal)
		{
			decimal pow = (decimal) Math.Pow(10, pDecimal);
			return ( Decimal.Truncate( (decimal) this.Value * pow ) / pow );
		}
		#endregion Truncate
		#region public Test
		public void Test()
		{
			Debug.WriteLine("Value: " + Convert.ToString(this.Value));
			Debug.WriteLine("ToString(): " + this.ToString());
			Debug.WriteLine("ToString(2): " + this.ToString(2));
			Debug.WriteLine("ToString(4): " + this.ToString(4));
			Debug.WriteLine("ToStringZero(): " + this.ToStringZero());
			Debug.WriteLine("ToStringZero(2): " + this.ToStringZero(2));
			Debug.WriteLine("ToStringZero(4): " + this.ToStringZero(4));
			Debug.WriteLine("Round(0): " + this.Round(0));
			Debug.WriteLine("Round(2): " + this.Round(2));
			Debug.WriteLine("Round(4): " + this.Round(4));
			Debug.WriteLine("Truncate(0): " + this.Truncate(0));
			Debug.WriteLine("Truncate(2): " + this.Truncate(2));
			Debug.WriteLine("Truncate(4): " + this.Truncate(4));
			this.StringToDecimal(this.ToString(), System.Globalization.CultureInfo.CurrentCulture);
			Debug.WriteLine("Value (StringToDecimal): " + Convert.ToString(this.Value));
		}
		#endregion public Test
	}
	#endregion class Rate
    
	#region public class FixedRate
	/// <summary>
	///  Class Qui represente un taux de remunération Fixe ou un pourcentage (ex taux des primes)
	/// </summary>
	public class FixedRate : Rate
	{
		#region Constructors
		public FixedRate() : base() 
		{}
		public FixedRate(decimal pValue) : base(pValue) 
		{}
		public FixedRate(string pValue, CultureInfo pCultureInfo) : base(pValue, pCultureInfo) 
		{}
		public FixedRate(string pValue) : this(pValue, Thread.CurrentThread.CurrentCulture)    
		{}
		#endregion Constructors
		
		#region public override ToString
        override public string ToString(int pDecimal)
        {
            // 20090608 RD Ticket 16248 Mise en commentaire
            // Pour pouvoir saisir un taux fixe à 0% sur une jambe d'un swap.
            // NB: Cette particularité permet de saisir des Swaptions "Straddle" avec une rémunération du type : ABS(0% - (Index10y - Index2y)) * notional * daycountfraction.
            // 
            // --------------------------------
            // Debut Code avant 20090608 
            //if (Value == 0)
            //{
            //    ret = string.Empty;
            //}

            ////20090507 FI mise en commentaire du pavé spécialement effectué pour les  commodities
            ////si je saisi 170 => je veux 170,00% dans l'écran,la donnée vaut 1.7
            ////
            ////else if (Value > 1) // New cas des commodities Les porcent sont > 100%
            ////{
            ////    //50.259 => 50,259 (Value vaut 50.259 lorsque l'utilisateur a saisi 5025.9 (=5025.9 pourcent))
            ////    ret = (Value * 100).ToString("n" + pDecimal.ToString());
            ////}
            //else
            //{
            //    //0.03 => 3 %
            //    ret = Value.ToString("p" + pDecimal.ToString());
            //}
            // Fin Code avant 20090608 
            // --------------------------------
            // 
            // --------------------------------
            // Debut Code après 20090608 
            //0.03 => 3 %
            string ret = Value.ToString("p" + pDecimal.ToString());
            // Fin Code après 20090608 
            // --------------------------------
            return ret;
        }
		#endregion public override ToString
	}
	#endregion class FixedRate
    
	#region public class FxRate
    /// <summary>
    /// FI 20161114 [RATP] Modify
    /// </summary>
	public class FxRate : Rate
	{
		#region Constructors
		public FxRate() : base() 
		{}
		public FxRate(decimal pValue) : base(pValue) 
		{}
		public FxRate(string pValue ) : this(pValue, Thread.CurrentThread.CurrentCulture)    
		{}
		public FxRate(string pValue, CultureInfo pCultureInfo  ) : base(pValue, pCultureInfo) 
		{}

		#endregion Constructors
		#region public override ToString
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDecimal"></param>
        /// <returns></returns>
        /// FI 20161114 [RATP] Modify
        override public string ToString(int pDecimal)
        {
            // FI 20161114 [RATP] pas de cas particulier sur 0 qui devient 0,00
            //if (Value == 0)
            //    return string.Empty;
            //else
            //    return Value.ToString("n" + pDecimal.ToString());
            return Value.ToString("n" + pDecimal.ToString());
        }
		#endregion public override ToString
	}
	#endregion class FxRate

	#region public class Fraction
	public class Fraction
	{
		#region Members
		private readonly bool m_isPercent;
		private readonly string m_Fraction;
		#endregion
		
		#region constructor
		public Fraction(string pFraction,bool pIsPercent)
		{
			m_Fraction  = pFraction;
			m_isPercent = pIsPercent;
		}
		public Fraction(string pFraction): this(pFraction, false)
		{
		}
		#endregion
		
		#region public DecValue
		/// <summary>
		/// Ex 1/8  => 0.125
		/// Ex 1/8% => 0.00125
		/// </summary>
		/// <returns></returns>
		public decimal DecValue()
		{
			decimal ret = 0;
			if (StrFunc.IsFilled(m_Fraction))
			{
				string[] sTemp    = m_Fraction.Split("/".ToCharArray());
				if (sTemp.Length ==2)
					ret = DecFunc.DecValue(sTemp[0]) /  DecFunc.DecValue(sTemp[1]);
				else
					ret = DecFunc.DecValue(sTemp[0]);
				//
				if (m_isPercent)
				{
					string dataValue = StrFunc.FmtDecimalToInvariantCulture(ret);
					FixedRate fixedRate = new FixedRate(dataValue + " %",System.Globalization.CultureInfo.InvariantCulture);
					ret = fixedRate.Value;
				}
			}
			return ret;
		}
		#endregion
	
		#region public override ToString
		/// <summary>
		/// Formatage Au format station d'une fraction
		/// Ex 1.2/8.5 => 1,2/8,5 ou 1,2/8,5 % (si percent)
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			string ret = string.Empty;
			//
			string[] sTemp    = m_Fraction.Split("/".ToCharArray());
			if (ArrFunc.IsFilled(sTemp) && (sTemp.Length ==2))  
			{
				bool isDecimal = false;
				for (int i = 0 ;i < sTemp.Length ; i++)
				{
					isDecimal =  (DecFunc.IsDecimalNotInteger(sTemp[i]));
					if (isDecimal) 
						break;
				}
				if (isDecimal)
				{
					for (int i = 0 ;i < sTemp.Length ; i++)
						sTemp[i] = StrFunc.FmtDecimalToCurrentCulture(DecFunc.DecValue(sTemp[i]));
				}

				ret = sTemp[0] + "/" + sTemp[1];

				if (m_isPercent)
					ret +=  " %";
			}
			//
			return ret;
		}
		#endregion
	}
	#endregion Fraction

}
