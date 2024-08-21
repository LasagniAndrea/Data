#region Using Directives
using System;
using System.Data;
using System.Collections;

using EFS.ACommon;
using EFS.Common;
using EFS.ApplicationBlocks.Data;

using EfsML.Enum.Tools;
using EfsML.Enum;
#endregion 

namespace EFSML.Swift
{
	
	public sealed class SwiftTools
	{
		public static bool CalcCommonReference(string pBICPartyA, string pBICPartyB, out string opPartyLeft, out string opPartyRight)		
		{
            bool ret;
            try
			{
				int retCompare;
				//bank and location codes 
				string pPartyA = pBICPartyA.Substring(0,4) + pBICPartyA.Substring(6,2);
				string pPartyB = pBICPartyB.Substring(0,4) + pBICPartyB.Substring(6,2);
				//letters take precedence over numbers
				retCompare = CompareInEBCDIC( pPartyA, pPartyB );
				if (retCompare < 0)
				{
					opPartyLeft  = pPartyA;
					opPartyRight = pPartyB;
				}
				else
				{
					opPartyLeft  = pPartyB;
					opPartyRight = pPartyA;
				}
				ret = true;
			}
			catch
			{
				opPartyLeft  = string.Empty;
				opPartyRight = string.Empty;
				ret = false;
			}
			return ret;
		}
		private static int CompareInEBCDIC(string pData1, string pData2)
		{
			int ret = 0;
			int minLenght = Math.Min(pData1.Length, pData2.Length);
			//
			if (pData1.Length < pData2.Length)
				ret = -1;
			else if (pData1.Length > pData2.Length)
				ret = 1;
			//
			for (int i=0;i<minLenght;i++)
			{
				int ascii1 = AsciiToEbcdic(Convert.ToChar(pData1.Substring(i, 1)));
				int ascii2 = AsciiToEbcdic(Convert.ToChar(pData2.Substring(i, 1)));
				//
				if (ascii1 < ascii2)
				{
					ret = -1;
					break;
				}
				else if (ascii1 > ascii2)
				{
					ret = 1;
					break;
				}
			}
			//
			return ret;
		}
		public static string AsciiToEbcdic(string pData)
		{
			string ret = string.Empty;
			for (int i=0;i<pData.Length;i++)
			{
				ret += Convert.ToChar(AsciiToEbcdic(Convert.ToChar(pData.Substring(i,1)))).ToString();
			}
			return ret;
		}
		public static int AsciiToEbcdic(Char pChar)
		{
			return AsciiToEbcdic(Convert.ToInt32(pChar));
		}
		public static int AsciiToEbcdic(int pAscii)		
		{
            int ebcdic;
            switch (pAscii)
			{
				case 0:// To 31         ' ? ? ? ? ? ? • ? ? ? ? ? ? ? ¤ ? ? ? ? ¶ § ? ? ? ? ? ? ? ? ? ?
				case 1:
				case 2:
				case 3:
				case 4:
				case 5:
				case 6:
				case 7:
				case 8:
				case 9:
				case 10:
				case 11:
				case 12:
				case 13:
				case 14:
				case 15:
				case 16:
				case 17:
				case 18:
				case 19:
				case 20:
				case 21:
				case 22:
				case 23:
				case 24:
				case 25:
				case 26:
				case 27:
				case 28:
				case 29:
				case 30:
				case 31:
					ebcdic = 1;
					break;
				case 32://              ' Space
					ebcdic = 64;
					break;
				case 33://				' ! " # $ % & ' ( ) * + , - . /
			    case 34:
				case 35:
				case 36:
				case 37:
				case 38:
				case 39:
				case 40:
				case 41:
				case 42:
				case 43:
				case 44:
				case 45:
				case 46:
				case 47:
					ebcdic = 1;
					break;
				case 48://	           ' 0 à 9
				case 49:
				case 50:
				case 51:
				case 52:
				case 53:
				case 54:
				case 55:
				case 56:
				case 57:
					ebcdic = 240 + pAscii - 48;//     ' 240,241,242,243,244,245,246,247,248,249
					break;
				case 58://				' : ; < = > ? @
				case 59:
				case 60:
				case 61:
				case 62:
				case 63:
				case 64:
					ebcdic = 1;
					break;
				case 65://					' A B C D E F G H I
				case 66:
				case 67:
				case 68:
				case 69:
				case 70:
				case 71:
				case 72:
				case 73:
					ebcdic = 193 + pAscii - 65;//     ' 193,194,195,196,197,198,199,200,201
					break;
				case 74://					' J K L M N O P Q R
				case 75:
				case 76:
				case 77:
				case 78:
				case 79:
				case 80:
				case 81:
				case 82:
					ebcdic = 209 + pAscii - 74;//     ' 209,210,211,212,213,214,215,216,217
					break;
				case 83://					' S T U V W X Y Z
				case 84:
				case 85:
				case 86:
				case 87:
				case 88:
				case 89:
				case 90:
					ebcdic = 226 + pAscii - 83;//     ' 226,227,228,229,230,231,232,233
					break;
				case 91://					' [ \ ] ^ _ `
				case 92:
				case 93:
				case 94:
				case 95:
				case 96:
					ebcdic = 1;
					break;
				case 97://					' a b c d e f g h i
				case 98:
				case 99:
				case 100:
				case 101:
				case 102:
				case 103:
				case 104:
				case 105:
					ebcdic = 129 + pAscii - 97;//     ' 129,130,131,132,133,134,135,136,137
					break;
				case 106://					' j k l m n o p q r
				case 107:
				case 108:
				case 109:
				case 110:
				case 111:
				case 112:
				case 113:
				case 114:
					ebcdic = 145 + pAscii - 106;//    ' 145,146,147,148,149,150,151,152,153
					break;
				case 115://					' s t u v w x y z
				case 116:
				case 117:
				case 118:
				case 119:
				case 120:
				case 121:
				case 122:
					ebcdic = 162 + pAscii - 115;//    ' 162,163,164,165,166,167,168,169
					break;
				case 123://			       ' { | } ~ ¦
				case 124:
				case 125:
				case 126:
				case 127:
					ebcdic = 1;
					break;
				default:
					ebcdic = 1;
					break;
			}
			return ebcdic;
		}

        
    }
	
        
}