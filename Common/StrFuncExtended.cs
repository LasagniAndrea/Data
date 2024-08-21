using System;
using System.Xml;
using System.Reflection;
using System.Text.RegularExpressions; 
using System.Globalization;
using System.Collections.Generic;
using System.Linq; 

using System.CodeDom.Compiler;
using Microsoft.CSharp;

using EFS.ACommon;
using EFS.Common;


namespace EFS.Common
{

    ///	<summary>
    ///	Management of string with extended functionality 
    ///	</summary>
    public sealed class StrFuncExtended
    {

        /// <summary>
        ///  Remplace dans la string {pInput} les champs de {pObject} par leur valeur avec certaines fonctionalités étendues (overflow-ellipsis, overflow-hidden)
        ///  <para>Chaque champ est spécifié via des accolades</para>
        ///  <para>Les données sont formatées selon la culture du thread</para>
        ///  <para>Au préalable, Il y a évaluation des expressions "choose" dans {pInput} s'il en existe</para>
        /// </summary>
        /// <param name="pInput"></param>
        /// <param name="pObject"></param>
        /// <returns></returns>
        public static string ReplaceObjectField(string pInput, object pObject)
        {
            return ReplaceObjectField(pInput, pObject, CultureInfo.CurrentCulture);
        }
        /// <summary>
        ///  Remplace dans la string {pInput} les champs de {pObject} par leur valeur avec certaines fonctionalités étendues (overflow-ellipsis, overflow-hidden)
        ///  <para>Chaque champ est spécifié via des accolades</para>
        ///  <para>Au préalable, Il y a évaluation des expressions "choose" dans {pInput} s'il en existe </para>
        ///  <para>Possibilité de spécifier une culture pour le formatage des données</para>
        /// </summary>
        /// <param name="pInput"></param>
        /// <param name="pObject"></param>
        /// <param name="pCulture">Culture utilisée pour formatée les données</param>
        /// <returns></returns>
        /// FI 20150413 [20275] add method
        /// FI 20150702 [XXXXX] Modify
        public static string ReplaceObjectField(string pInput, object pObject, CultureInfo pCulture)
        {
            // FI 20150702 [XXXXX] Appel à ReplaceChooseExpression2
            //Evaluation des expressions choose 
            string ret = ReplaceChooseExpression2(pInput, pObject, true);


            //Gestion du {Field}.overflow-ellipsis(32) ou {Field}.overflow-hidden(32) ou right(2) ou format('yyyyMMdd') lorsqu'il existe
            Regex regEx = new Regex(@"{\w+}.(\w+-?\w*)\((\d+|'\w+'|\s*)\)", RegexOptions.IgnoreCase); //Accepte éventuellement 1 et 1 seul '-' ds le nom de méthode 
            Match match = regEx.Match(ret);
            while (match.Success)
            {
                string[] split = match.Value.Split('.');
                string fieldName = split[0].Replace("{", string.Empty).Replace("}", string.Empty);
                if (IsExistField(pObject, fieldName))
                {
                    string methodName = match.Groups[1].Value.ToLower();
                    string arg = match.Groups[2].Value;
                    int intArg;
                    object value;
                    string sValue;
                    switch (methodName)
                    {
                        case "overflow-ellipsis":
                        case "overflow-hidden":
                            if (StrFunc.IsEmpty(arg))
                                throw new InvalidProgramException(StrFunc.AppendFormat("one argument is expected for method (name:{0})", methodName));

                            intArg = Convert.ToInt32(arg);
                            if (intArg == 0)
                                throw new InvalidProgramException(StrFunc.AppendFormat("argument 0 is not expected for method (name:{0})", methodName));

                            value = GetField(pObject, fieldName);
                            sValue = ConvertToString(value, pCulture);

                            sValue = ApplyOverflow(sValue, methodName, intArg);
                            break;

                        case "right":
                        case "left":
                            if (StrFunc.IsEmpty(arg))
                                throw new InvalidProgramException(StrFunc.AppendFormat("one argument is expected for method (name:{0})", methodName));

                            intArg = Convert.ToInt32(arg);
                            if (intArg == 0)
                                throw new InvalidProgramException(StrFunc.AppendFormat("argument 0 is not expected for method (name:{0})", methodName));

                            value = GetField(pObject, fieldName);
                            sValue = ConvertToString(value, pCulture);
                            if (methodName == "right")
                                sValue = sValue.Substring(sValue.Length - intArg, intArg);
                            else if (methodName == "left")
                                sValue = sValue.Substring(0, intArg);
                            else
                                throw new InvalidProgramException(StrFunc.AppendFormat(" method (name:{0}) not expected", methodName));
                            break;

                        case "format":
                            if (StrFunc.IsEmpty(arg))
                                throw new InvalidProgramException(StrFunc.AppendFormat("one argument is expected for method (name:{0})", methodName));

                            if (false == arg.StartsWith("'") || false == arg.EndsWith("'"))
                                throw new InvalidProgramException(StrFunc.AppendFormat(@"argument is invalid for method (name:{0}).Argument must start and end with a quote", methodName));

                            /* suppression de la 1er quote et de la dernière quote */
                            string sArg = arg.Substring(1);
                            sArg = sArg.Remove(sArg.Length - 1);

                            value = GetField(pObject, fieldName);
                            Type iFormattableType = value.GetType().GetInterface("IFormattable");
                            if (null == iFormattableType)
                                throw new InvalidProgramException(StrFunc.AppendFormat(@"fieldName (name:{0}) is not IFormattable", fieldName));

                            IFormattable iFormattableValue = (IFormattable)value;

                            sValue = iFormattableValue.ToString(sArg, pCulture);
                            break;

                        default:
                            throw new NotImplementedException(StrFunc.AppendFormat("method (name:{0}) is not implemented", split[1]));
                    }

                    ret = ret.Replace(match.Value, sValue);
                }
                match = match.NextMatch();
            }

            //Gestion du {Field}.overflow-ellipsis(32lt) ou {Field}.overflow-hidden(32lt) lorsqu'il existe
            regEx = new Regex(@"{\w+}.(overflow-ellipsis|overflow-hidden)\(\d+tl\)", RegexOptions.IgnoreCase);
            match = regEx.Match(ret);
            if (match.Captures.Count > 1)
            {
                throw new NotSupportedException("only one call of overflow function with 'tl' argument is expected");
            }
            else if (match.Captures.Count == 1)
            {
                string[] split = match.Value.Split('.');

                string fieldName = split[0].Replace("{", string.Empty).Replace("}", string.Empty);
                string methodName = split[1].Split('(')[0];
                string arg = split[1].Replace(methodName, string.Empty).Replace("(", String.Empty).Replace("tl)", string.Empty);

                if (IsExistField(pObject, fieldName))
                {

                    ret = ret.Replace(StrFunc.AppendFormat("{{{0}}}.{1}({2}tl)", fieldName, methodName, arg), StrFunc.AppendFormat("#{0}#.{1}({2}tl)", fieldName, methodName, arg));
                    ret = ReplaceObjectFieldBasic(ret, pObject, pCulture);
                    ret = ret.Replace(StrFunc.AppendFormat("#{0}#.{1}({2}tl)", fieldName, methodName, arg), StrFunc.AppendFormat("{{{0}}}.{1}({2}tl)", fieldName, methodName, arg));

                    switch (methodName)
                    {
                        case "overflow-ellipsis":
                        case "overflow-hidden":
                            if (StrFunc.IsEmpty(arg))
                                throw new NotImplementedException(StrFunc.AppendFormat("one argument is expected for method (name:{0})", methodName));

                            int argValue = Convert.ToInt32(arg);
                            if (argValue == 0)
                                throw new NotImplementedException(StrFunc.AppendFormat("argument value 0 is not expected for method (name:{0})", methodName));

                            Object value = GetField(pObject, fieldName);
                            string sValue = ConvertToString(value, pCulture);

                            string tmp = ret.Replace(match.Value, sValue); // tmp contient la string théorique finale
                            if (tmp.Length > argValue) // Si la taille théorique finale est > à la taille max le champ {fieldName} est réduit de manière à ne pas dépasser la taille max
                            {
                                int lenMax = sValue.Length - (tmp.Length - argValue);
                                if (lenMax > 0)
                                    sValue = ApplyOverflow(sValue, methodName, lenMax);
                                else
                                    sValue = string.Empty;
                            }

                            ret = ret.Replace(match.Value, sValue);
                            break;
                    }
                }
            }
            else if (match.Captures.Count == 0)
            {
                ret = ReplaceObjectFieldBasic(ret, pObject, pCulture);
            }

            return ret;
        }

        /// <summary>
        /// Remplace dans la string {pInput} les champs de {pObject} par leur valeur
        ///  <para>Chaque champ est spécifié via des accolades</para>
        /// </summary>
        /// <param name="pInput"></param>
        /// <param name="pObject"></param>
        /// <param name="pCulture">Culture utilisée pour formatée les données</param>
        /// <returns></returns>
        /// FI 20150907 [XXXXX] Modify
        public static string ReplaceObjectFieldBasic(string pInput, object pObject, CultureInfo pCulture)
        {
            string matchValue = string.Empty; ;
            // FI 20150907 [XXXXX] Appel à ReplaceChooseExpression2
            string ret = ReplaceChooseExpression2(pInput, pObject, true);

            Regex regEx = new Regex(@"{\w+}", RegexOptions.IgnoreCase);
            Match match = regEx.Match(ret);
            while (match.Success)
            {
                if (false == matchValue.Contains(match.Value))
                {
                    string fieldName = match.Value;
                    if (IsExistField(pObject, match.Value))
                    {
                        Object value = GetField(pObject, fieldName);
                        string sValue = ConvertToString(value, pCulture);

                        ret = ret.Replace(match.Value, sValue);
                    }
                    matchValue += match.Value;
                }
                match = match.NextMatch();
            }
            return ret;

        }
        /// <summary>
        /// Remplace dans la string {pInput} les champs de {pObject} par leur valeur
        ///  <para>Chaque champ est spécifié via des accolades</para>
        ///  <para>Les données sont formatées selon la culture du thread</para>
        /// </summary>
        /// <param name="pInput"></param>
        /// <param name="pObject"></param>
        /// <param name="pCulture">Culture utilisée pour formatée les données</param>
        /// <returns></returns>
        public static string ReplaceObjectFieldBasic(string pInput, object pObject)
        {
            return ReplaceObjectFieldBasic(pInput, pObject, CultureInfo.CurrentCulture);
        }


        

        /// <summary>
        /// Retourne le résultat d'une expression choose. les éléments entre accolades indiquent des champs de pObject. 
        /// <para></para>
        /// <para>exemple d'expression choose &lt;choose&gt;&lt;when test="{SIDE}='CR'"&gt;Deposit/Credit&lt;/when&gt;&lt;otherwise&gt;Withdrawal/Debit&lt;/otherwise&gt;&lt;/choose&gt;
        /// <![CDATA[<choose><when test="{SIDE}='CR'">Deposit/Credit</when><otherwise>Withdrawal/Debit</otherwise></choose>]]>
        /// </para>
        /// </summary>
        /// <param name="pXmlNodeChoose"></param>
        /// <param name="pObject"></param>
        /// <returns></returns>
        /// FI 20150413 [20275] add method
        /// FI 20150907 [XXXXX] Modify
        private static string GetResultOfChooseExpression(string pXmlNodeChoose, object pObject, Boolean pIsCheckChild)
        {
            XmlDocument xmlDocument = new XmlDocument();
            try
            {
                xmlDocument.LoadXml(pXmlNodeChoose);
            }
            catch (Exception ex)
            {
                throw new NotSupportedException(StrFunc.AppendFormat("Error when loading pXmlNodeChoose parameter. pXmlNodeChoose: {0}", pXmlNodeChoose), ex);
            }

            string rootDocument = xmlDocument.DocumentElement.Name;
            if (rootDocument != "choose")
                throw new NotSupportedException("xml document is not an choose element");

            string ret = GetResultOfChooseExpression(xmlDocument.DocumentElement, pObject, pIsCheckChild);

            //FI 20150907 [XXXXX] S'il existe un noeud <choose> ds la string e, entrée, cela signifie qu'il existe potentiellement les entities &lt; et "&gt;"
            //En sortie de méthode ces dernières sont remplacées par les vrais caractères attendus (à savoir "<" et ">") 
            if (StrFunc.IsFilled(ret))
            {
                ret = ret.Replace("&lt;", "<");
                ret = ret.Replace("&gt;", ">");
            }

            return ret;
        }

        /// <summary>
        /// Retourne le résultat d'une expression choose. les éléments entre accolades indiquent des champs de pObject. 
        /// <para></para>
        /// <para>exemple d'expression choose &lt;choose&gt;&lt;when test="{SIDE}='CR'"&gt;Deposit/Credit&lt;/when&gt;&lt;otherwise&gt;Withdrawal/Debit&lt;/otherwise&gt;&lt;/choose&gt;
        /// <![CDATA[<choose><when test="{SIDE}='CR'">Deposit/Credit</when><otherwise>Withdrawal/Debit</otherwise></choose>]]>
        /// </para>
        /// </summary>
        /// <param name="pXmlNodeChoose">Noeud choose</param>
        /// <param name="pObject"></param>
        /// <param name="pIsCheckInChild">si true, interprétation des expressions choose enfants éventuellement présentes</param>
        /// <returns></returns>
        /// FI 20150413 [20275] add method
        /// FI 20150907 [XXXXX] Modify
        private static string GetResultOfChooseExpression(XmlNode pXmlNodeChoose, object pObject, Boolean pIsCheckInChild)
        {
            if (null == pXmlNodeChoose)
                throw new NullReferenceException("parameter pXmlNodeChoose is null");

            if (pXmlNodeChoose.Name != "choose")
                throw new NotSupportedException("node choose expected");

            string ret = string.Empty;


            if (pXmlNodeChoose.HasChildNodes)
            {
                foreach (XmlNode item in pXmlNodeChoose.ChildNodes)
                {
                    Boolean isTrue = true;
                    switch (item.Name)
                    {
                        case "when":
                            XmlAttribute xmlAttribute = item.Attributes["test"];
                            if (null == xmlAttribute)
                                throw new Exception("when element must contains test attribute");

                            //    string[] expression = xmlAttribute.Value.Split(new string[] { "and" }, StringSplitOptions.RemoveEmptyEntries);
                            //    foreach (string itemExpression in expression)
                            //        isTrue = isTrue && IsExpressionIsTrue(itemExpression, pObject);

                            // FI 20150907 [XXXXX] Appel à la méthode IsExpressionWhenIsTrue (méthode qui gère des expressions complexes)
                            isTrue = IsExpressionWhenIsTrue(xmlAttribute.Value, pObject);
                            break;

                        case "otherwise":
                            isTrue = true;
                            break;
                        default:
                            throw new NotImplementedException(StrFunc.AppendFormat("{0} is not expected", item.Name));
                    }

                    if (isTrue)
                    {
                        if (null == item.FirstChild)
                            throw new Exception(StrFunc.AppendFormat("element (name:{0}) element must contains a childNode", item.Name));

                        if (pIsCheckInChild)
                        {
                            if (item.FirstChild.NodeType == XmlNodeType.Text || item.LastChild.NodeType == XmlNodeType.Text)
                            {
                                // On rentre ici lorsque le noeud contient du text (Il pourrait y avoir un element choose également)
                                // FI 20150907 [XXXXX] use ReplaceChooseExpression2
                                ret = ReplaceChooseExpression2(item.InnerXml, pObject, pIsCheckInChild);
                            }
                            else if (item.FirstChild.NodeType == XmlNodeType.Element)
                            {
                                //seul un noeud de type choose est accepté
                                ret = GetResultOfChooseExpression(item.FirstChild, pObject, pIsCheckInChild);
                            }
                            else
                                throw new NotImplementedException(StrFunc.AppendFormat("Node (type:{0}) is not supported", item.FirstChild.NodeType.ToString()));
                        }
                        else
                        {
                            ret = item.InnerXml.Trim();
                        }
                        break;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Retoure la valeur boolean d'une expression "=" ou "!=" ou "&lt;" ou "&lt;=" ou "&gt;" ou "&gt;="
        /// </summary>
        /// <param name="pExpression">Expression litteral (Exemple : {SIDE}='1')</param>
        /// <param name="pObj">objet</param>
        /// <returns></returns>
        /// FI 20150413 [20275] add method
        // EG 20151019 [21465] New contains (valueB contenu dans valueA)
        private static Boolean IsExpressionIsTrue(string pExpression, object pObj)
        {
            pExpression = pExpression.Trim();
            if (pExpression.StartsWith("("))
                pExpression = pExpression.Remove(0, 1);
            if (pExpression.EndsWith(")"))
                pExpression = pExpression.Remove(pExpression.Length - 1, 1);

            string @operator = GetOperator(pExpression);

            string[] splitRes = pExpression.Split(new string[] { @operator }, StringSplitOptions.RemoveEmptyEntries);

            object valueA = null;
            object valueB = null;
            for (int i = 0; i < ArrFunc.Count(splitRes); i++)
            {
                string svalue = splitRes[i].Trim();

                object value;
                if (svalue.StartsWith("{") && svalue.EndsWith("}")) //{} est utilsé pour signifier la présence d'un champ de  {pObj}
                    value = GetField(pObj, svalue);
                else if (svalue.StartsWith("'") && svalue.EndsWith("'"))
                    value = svalue.Replace("'", String.Empty);
                else if (svalue == "null")
                    value = null;
                else
                    value = svalue;

                if (0 == i)
                    valueA = value;
                else
                    valueB = value;
            }

            Nullable<int> retCompare;
            Boolean ret;
            switch (@operator)
            {
                case @"=":
                    ret = IsEqual(valueA, valueB);
                    break;
                case @"!=":
                    ret = (false == IsEqual(valueA, valueB));
                    break;
                case @">":
                    retCompare = Compare(valueA, valueB);
                    ret = retCompare.HasValue && retCompare.Value > 0;
                    break;
                case @">=":
                    ret = IsEqual(valueA, valueB);
                    if (false == ret)
                    {
                        retCompare = Compare(valueA, valueB);
                        ret = retCompare.HasValue && retCompare.Value > 0;
                    }
                    break;
                case @"<":
                    retCompare = Compare(valueA, valueB);
                    ret = retCompare.HasValue && retCompare.Value < 0;
                    break;
                case @"<=":
                    ret = IsEqual(valueA, valueB);
                    if (false == ret)
                    {
                        retCompare = Compare(valueA, valueB);
                        ret = retCompare.HasValue && retCompare.Value < 0;
                    }
                    break;
                // EG 20151019 [21465] New valueB contenu dans valueA
                case @"contains":
                    ret = IsContains(valueA, valueB);
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("operator :{0} is not implemendted", @operator));
            }

            return ret;
        }

        /// <summary>
        /// Retourne l'opérateur présent dans {pExpression}
        /// </summary>
        /// <param name="pEqualExpression"></param>
        /// <returns></returns>
        // EG 20151019 [21465] New operator (contains)
        private static string GetOperator(string pExpression)
        {
            if (StrFunc.IsEmpty(pExpression))
                throw new ArgumentNullException("pExpression is null");

            string @operator = string.Empty;

            string[] arrayOperator = new string[] { "!=", ">=", "<=", "=", ">", "<", "contains" };
            foreach (string item in arrayOperator)
            {
                string[] splitRes = pExpression.Split(new string[] { item }, StringSplitOptions.RemoveEmptyEntries);
                if (ArrFunc.Count(splitRes) == 2)
                {
                    @operator = item;
                    break;
                }
            }

            if (StrFunc.IsEmpty(@operator))
                throw new Exception(StrFunc.AppendFormat("Expression :{0} is not valid", pExpression));

            return @operator;
        }



        /// <summary>
        /// Retourne true si la valeur {valueA} est identique à la valeur {valueB}
        /// <para>valueB et valueB sont de type primaire (ex date, string, int, decimal, etc..)</para>
        /// </summary>
        /// <param name="valueA"></param>
        /// <param name="valueB"></param>
        /// <returns></returns>
        /// FI 20150709 [XXXXX] Modify
        private static Boolean IsEqual(object valueA, object valueB)
        {
            bool isEqual;
            if (valueA == null && valueB == null) //FI 20150709 Egalité si valeur null
            {
                isEqual = true;
            }
            else if (valueA == null || valueB == null) // FI 20180528 [XXXXX] Add
            {
                isEqual = false;
            }
            else if (valueA.GetType().Equals(valueB.GetType()))
            {
                isEqual = (valueA.Equals(valueB)); //Equal utilise le fonction de comparaison du type pour établier l'égalité (Il ne faut pas utiliser "==") 
            }
            else if ((false == valueA.GetType().Equals(typeof(string))) && (false == valueB.GetType().Equals(typeof(string))))
            {
                // si les données valueA et valueB ne sont pas de même type et si elles en sont pas de type string => les données sont nécessairement non identique
                isEqual = false;
            }
            else
            {
                // si les données valueA et valueB ne sont pas de même type et si 1 est de type String => Spheres® tente de convertir la string dans le type de l'autre donnée
                Type typeCompare;
                if (valueA.GetType().Equals(typeof(string)))
                {
                    typeCompare = valueB.GetType();
                    valueA = ConvertStringTo((string)valueA, typeCompare);
                }
                else if (valueB.GetType().Equals(typeof(string)))
                {
                    typeCompare = valueA.GetType();
                    valueB = ConvertStringTo((string)valueB, typeCompare);
                }
                isEqual = (valueA.Equals(valueB)); //Equal utilise le fonction de comparaison du type pour établier l'égalité (Il ne faut pas utiliser "==") 
            }
            return isEqual;
        }

        /// <summary>
        /// Retourne true si la valeur {valueB} est contenue dans la valeur {valueA}
        /// <para>valueA et valueB sont de type string</para>
        /// </summary>
        /// <param name="valueA"></param>
        /// <param name="valueB"></param>
        /// <returns></returns>
        /// EG 20151019 [21465] New
        private static Boolean IsContains(object valueA, object valueB)
        {
            bool ret;
            if (valueA == null || valueB == null) // FI 20210409 [XXXXX] Add case of null value
                ret = false;
            else if (valueA.GetType().Equals(valueB.GetType()) && valueA.GetType().Equals(typeof(string)))
                ret = valueA.ToString().Contains(valueB.ToString());
            else
                throw new NotSupportedException(StrFunc.AppendFormat(" contains can not be used : type:{0} and type:{1}", valueA.GetType().ToString(), valueB.GetType().ToString()));

            return ret;
        }

        /// <summary>
        ///  Compare valueA à valueB. 
        ///  <para>Retourne null si valueA ou valueB est null</para>
        /// </summary>
        /// <param name="valueA"></param>
        /// <param name="valueB"></param>
        /// <returns></returns>
        private static Nullable<int> Compare(object valueA, object valueB)
        {
            Nullable<int> ret = 0;
            if (valueA == null || valueB == null) // FI 20210409 [XXXXX]
            {
                ret = null;
            }
            else if (valueA.GetType().Equals(valueB.GetType()))
            {
                if (null != valueA.GetType().GetInterface("IComparable"))
                    ret = ((IComparable)valueA).CompareTo((IComparable)valueB);
            }
            else if ((false == valueA.GetType().Equals(typeof(string))) && (false == valueB.GetType().Equals(typeof(string))))
            {
                throw new NotSupportedException(StrFunc.AppendFormat(" type:{0} and type:{1} are not comparable", valueA.GetType().ToString(), valueB.GetType().ToString()));
            }
            else
            {
                Type typeCompare = null;
                if (valueA.GetType().Equals(typeof(string)))
                {
                    typeCompare = valueB.GetType();
                    valueA = ConvertStringTo((string)valueA, typeCompare);
                }
                else if (valueB.GetType().Equals(typeof(string)))
                {
                    typeCompare = valueA.GetType();
                    valueB = ConvertStringTo((string)valueB, typeCompare);
                }
                if (null != typeCompare.GetInterface("IComparable"))
                    ret = ((IComparable)valueA).CompareTo((IComparable)valueB);
                else
                    throw new NotSupportedException(StrFunc.AppendFormat(" type:{0} and type:{1} are not comparable", valueA.GetType().ToString(), valueB.GetType().ToString()));
            }
            return ret;
        }



        /// <summary>
        ///  Convertie la donnée {pData} de type string (et formatée selon l'invariant culture) dans le type {pType}
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="pType"></param>
        /// <returns></returns>
        /// FI 20150413 [20275] add method
        /// FI 20151023 [XXXXX] Modify 
        private static object ConvertStringTo(string pData, Type pType)
        {
            object ret;
            if (pType.Equals(typeof(Int32)) || pType.Equals(typeof(Int64)))
                ret = IntFunc.IntValue2(pData, CultureInfo.InvariantCulture);
            else if (pType.Equals(typeof(DateTime)))
                ret = new DtFunc().StringToDateTime(pData);
            else if (pType.Equals(typeof(decimal)))
                ret = DecFunc.DecValue(pData, CultureInfo.InvariantCulture);
            else if (pType.Equals(typeof(double)))
                ret = (double)DecFunc.DecValue(pData, CultureInfo.InvariantCulture);
            else if (pType.Equals(typeof(String)))
                ret = pData;
            else if (pType.Equals(typeof(Boolean))) // FI 20151023 [XXXXX] Gestion des boolean
                ret = BoolFunc.IsTrue(pData);
            else
                throw new NotImplementedException(StrFunc.AppendFormat("type (name:{0}) is not implemented", pType.ToString()));
            return ret;
        }

        /// <summary>
        ///  Formatage la donnée {pData} selon culture {pCulture}
        ///  <para>Utilisation du formatage par Défaut</para>
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="pCulture"></param>
        /// <returns></returns>
        /// FI 20150413 [20275] add method (GLOP l'usage d'un delegate pourrait être une bonne approche pour formater autrement les données)
        private static string ConvertToString(object pData, CultureInfo pCulture)
        {
            string ret = Convert.ToString(pData, pCulture);

            //Type type = pData.GetType();
            //if (type.Equals(typeof(Int32)) || type.Equals(typeof(Int64)))
            //    ret = Convert.ToString(pData, pCulture);
            //else if (type.Equals(typeof(DateTime)))
            //    ret = DtFunc.DateTimeToString((DateTime)pData, DtFunc.FmtISODateTime, pCulture);
            //else if (type.Equals(typeof(decimal)) || type.Equals(typeof(double)))
            //    ret = StrFunc.FmtDecimalToCurrentCulture((decimal)pData);
            //else if (type.Equals(typeof(String)))
            //    ret = (String)pData;
            //else
            //    throw new NotImplementedException(StrFunc.AppendFormat("type (name:{0}) is not implemented", type.ToString()));
            //
            return ret;
        }

        /// <summary>
        /// Retourne la valeur du champ {pFieldName} de l'objet {pObject}
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pFieldName"></param>
        /// <returns></returns>
        // EG 20180629 PERF - No throw
        private static Object GetField(object pObject, string pFieldName)
        {
            string fieldName = pFieldName.Replace("{", string.Empty).Replace("}", string.Empty);
            Object value = ReflectionTools.GetFieldByName2(pObject, fieldName, out _);
            return value;
        }

        /// <summary>
        /// Retourne true si le champ {pFieldName} de l'objet {pObject}
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pFieldName"></param>
        /// <returns></returns>
        private static Boolean IsExistField(object pObject, string pFieldName)
        {
            string fieldName = pFieldName.Replace("{", string.Empty).Replace("}", string.Empty);
            _ = ReflectionTools.GetFieldByName2(pObject, fieldName, out MemberInfo memberInfo);
            return (null != memberInfo);
        }


        /// <summary>
        /// Retourne {pdata} tonquée lorsque cette dernière des de taille supérieure à {lenMax}
        /// </summary>
        /// <param name="pInput"></param>
        /// <param name="overfowType">overflow-hidden : tronqué simple. overflow-ellipsis : tronqué avec usage de "..."  </param>
        /// <param name="lenMax"></param>
        /// <returns></returns>
        private static string ApplyOverflow(string pData, string overfowType, int lenMax)
        {
            if (null == pData)
                throw new ArgumentNullException("pData is not specified");
            if (StrFunc.IsEmpty(overfowType))
                throw new ArgumentNullException("overfowType is not specified");

            string ret = string.Empty;
            if (lenMax > 0)
            {
                if (pData.Length < lenMax)
                {
                    ret = pData;
                }
                else if (overfowType == "overflow-hidden")
                {
                    ret = pData.Substring(0, lenMax);
                }
                else if (overfowType == "overflow-ellipsis")
                {
                    if (lenMax > 3)
                        ret = pData.Substring(0, lenMax - 3) + "...";
                    else
                        ret = pData.Substring(0, lenMax);
                }
                else
                    throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", overfowType));
            }

            return ret;
        }

        /// <summary>
        ///	 Replace dans une string les n expressions choose pouvant exister 
        /// </summary>
        /// <param name="pInput">donnée string en entrée</param>
        /// <param name="pObject"></param>
        /// <param name="pIsCheckInChild"></param>
        /// <returns></returns>
        /// FI 20150907 [XXXXX] Modify
        /// FI 20150907 [XXXXX] Modify (Optimisation)
        // EG 20180629 PERF  Call Trim_CrLfTabSpace
        public static string ReplaceChooseExpression2(string pInput, object pObject, Boolean pIsCheckInChild)
        {
            string ret = pInput;

            List<Pair<String, String>> lst = new List<Pair<String, String>>();

            if (pInput.Contains(@"<choose>") && pInput.Contains(@"</choose>"))
            {
                int indexInit = 0;
                while (pInput.IndexOf(@"<choose>", indexInit) > -1)
                {
                    int indexStartInit = pInput.IndexOf(@"<choose>", indexInit);
                    int indexStart = indexStartInit + 8;
                    int i = 1;
                    while (i > 0)
                    {
                        int nextChooseStartIndex = pInput.IndexOf("<choose>", indexStart);
                        int nextChooseEndIndex = pInput.IndexOf(@"</choose>", indexStart);
                        if (nextChooseEndIndex == -1)
                            throw new Exception("</choose> is missing");

                        if ((nextChooseStartIndex) > -1 && (nextChooseStartIndex < nextChooseEndIndex))
                        {
                            i++;
                            indexStart = nextChooseStartIndex + 8;
                        }
                        else if (nextChooseEndIndex > 0)
                        {
                            indexStart = nextChooseEndIndex + 9;
                            i--;
                        }
                    }

                    int lenght = indexStart - indexStartInit;
                    string expressionChoose = pInput.Substring(indexStartInit, lenght);

                    var search = from item in lst.Where(x => x.First == expressionChoose) select item.First;
                    if (search.Count() == 0)
                    {
                        string chooseResult = GetResultOfChooseExpression(expressionChoose, pObject, pIsCheckInChild);
                        lst.Add(new Pair<string, string>(expressionChoose, chooseResult));
                    }

                    indexInit = indexStart;
                }
            }

            if (lst.Count > 0)
            {
                foreach (Pair<string, string> item in lst)
                {
                    string old = item.First;
                    string @new = item.Second;
                    // PL 20180620 PERF
                    // FI 20181115 [24309] Ajour d'un espace pour éviter erreur de syntaxe sql 
                    @new = Cst.Space + StrFunc.Trim_CrLfTabSpace(@new);

                    ret = ret.Replace(old, @new);
                }
            }

            return ret;
        }

        /// <summary>
        /// Evalue une Expression éventuellement complexe constituée de and et or 
        /// <para>Exemple ({SIDE}!='CR') and (3 &lt; {INT1} or {DATE1}>'2015-04-04T01:01:01')</para>
        /// </summary>
        /// <param name="pWhenExpression"></param>
        /// <param name="pObject"></param>
        /// <returns></returns>
        /// FI 20150907 [XXXXX] Modify
        // EG 20180629 User CompilerParameters.GenerateInMemory and CompilerParameters.
        private static Boolean IsExpressionWhenIsTrue(string pWhenExpression, Object pObject)
        {
            Boolean ret = false;

            string whenExpressionResult = pWhenExpression;
            string[] expression = pWhenExpression.Split(new string[] { "and", "or" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string itemExpression in expression)
            {
                string itemExpression2 = itemExpression.Replace("(", string.Empty).Replace(")", string.Empty);
                Boolean isTrue2 = IsExpressionIsTrue(itemExpression2, pObject);

                int start = itemExpression.LastIndexOf("(");
                int end = itemExpression.IndexOf(")");

                string newitemExpression = string.Empty;
                if (start > -1)
                    newitemExpression = itemExpression.Substring(0, start + 1);
                newitemExpression += Cst.Space + isTrue2.ToString().ToLower() + Cst.Space;
                if (end > -1)
                    newitemExpression += itemExpression.Substring(end, itemExpression.Length - end);
                whenExpressionResult = whenExpressionResult.Replace(itemExpression, newitemExpression);
            }

            //string csharpWhenExpressionResult = whenExpressionResult.Replace("or", @"||").Replace("and", @"&&");
            Regex reg = new Regex(@"(or|and)");
            string eval(Match match)
            {
                switch (match.Value)
                {
                    case "or": return "||";
                    case "and": return "&&";
                    default: return match.Value;
                }
            }
            string csharpWhenExpressionResult = reg.Replace(whenExpressionResult, eval);
            string @namespace = "EFS.Common";
            string className = "BoolCalc";

            string code = @"
            using System;
            namespace {0}
            {                
                public class {1}
                {                
                    public Boolean Result = {2};        
                }
            }";
            code = code.Replace("{0}", @namespace).Replace("{1}", @className).Replace("{2}", csharpWhenExpressionResult);

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters
            {
                GenerateInMemory = true,
                CompilerOptions = "/optimize"
            };
            CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);
            object newObject = results.CompiledAssembly.CreateInstance(@namespace + "." + className);
            object result = ReflectionTools.GetFieldByName(newObject, "Result");
            ret = BoolFunc.IsTrue(result);

            return ret;
        }




#if DEBUG
        /// <summary>
        /// Méthodes pour tester la classe StrFuncExtended
        /// </summary>
        public class Test
        {
            /// <summary>
            /// 
            /// </summary>
            /// FI 20150413 [20275] add Methode (Methode pour tester la méthode ReplaceObjectField
            public static void ReplaceObjectFieldMethod()
            {

                CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("en-GB");


                //Exemple 1 : usage basic
                string expression1 = "Number: {number1}, Decimal: {dec1}, string1: {string1}, date1: {date1}";
                var context1 = new
                {
                    number1 = (int)2018,
                    dec1 = (decimal)3200.54,
                    string1 = "toto",
                    date1 = DateTime.Now
                };
                //Display "Number: 2018, Decimal: 3200.54, string1: toto, date1: 29/04/2021 15:19:05"
                _ = StrFuncExtended.ReplaceObjectField(expression1, context1, cultureInfo);


                //Exemple 2 : usage basic avec formatage
                string expression2 = "Number: {number1}.format('D8'), Decimal: {dec1}.format('G'), string1: {string1}, date1: {date1}.format('D'), date1: {date1}.format('yyyyMMdd')";
                var context2 = new
                {
                    number1 = (int)2018,
                    dec1 = (decimal)3200.54,
                    string1 = "toto",
                    date1 = DateTime.Now
                };
                //Displays "Number: 00002018, Decimal: 3200.54, string1: toto, date1: 29 April 2021, date1: 20210429"
                _ = StrFuncExtended.ReplaceObjectField(expression2, context2, cultureInfo);


                //Exemple 3 : usage des methode right et format
                string expression3 = "{YEAR}.right(2){DAYOFYEAR}.format('000'){TOKEN}.format('00000')";
                var context3 = new
                {
                    YEAR = 2018,
                    DAYOFYEAR = 58,
                    TOKEN = 300
                };
                // on doit obtenir 1805800300
                _ = StrFuncExtended.ReplaceObjectField(expression3, context3, cultureInfo);


                //Exemple 4 : usage de choose Expression et des opérateurs != et < et >
                string expression4 = @"<choose>
                                        <when test=""{SIDE}!='CR' and 6 &lt; {INT1} and {DATE1}>'2015-04-04T01:01:01'"">result: {SIDE}</when>
                                        <otherwise>otherwise result: {DATE1}</otherwise>
                                      </choose>";
                var context4 = new
                {
                    SIDE = "DR",
                    INT1 = 7,
                    DATE1 = new DateTime(2016, 04, 04, 1, 1, 1),
                };
                //Displays "result: DR"
                _ = StrFuncExtended.ReplaceObjectField(expression4, context4, cultureInfo);

                var context44 = new
                {
                    SIDE = "CR",
                    INT1 = 7,
                    DATE1 = new DateTime(2016, 04, 04, 1, 1, 1),
                };
                //Displays " otherwise result: 04/04/2016 01:01:01"
                _ = StrFuncExtended.ReplaceObjectField(expression4, context44, cultureInfo);


                //Exemple 5 : usage de overflow-ellipsis
                string expression5 = @"{IDENTIFIER}.overflow-ellipsis(30tl) {LBL}";
                var context5 = new
                {
                    IDENTIFIER = "TOTOTOTOTOTO",
                    LBL = " AUTRES INFORMATIONS"
                };
                //Displays "TOTOTO...  AUTRES INFORMATIONS"
                _ = StrFuncExtended.ReplaceObjectField(expression5, context5, cultureInfo);


                //Exemple 6 : usage de d'expression complète avec des and, des or et des parenthèses
                string expression6 = @"<choose>
                                        <when test=""(({SIDE}!='CR') and (3 &lt; {INT1} or {DATE1}>'2015-04-04T01:01:01'))"">when result:{SIDE}</when>
                                        <otherwise>otherwise result:{DATE1}</otherwise>
                                      </choose>";
                var context6 = new
                {
                    SIDE = "DR",
                    INT1 = 7,
                    DATE1 = new DateTime(2016, 04, 04, 1, 1, 1),
                };
                //Displays "when result:DR"
                _ = StrFuncExtended.ReplaceObjectField(expression6, context6, cultureInfo);

                //Exemple 7 : choose assez complexe 
                //Rq il est important de constater qu'il est possible de mettre sous l'élément when et otherwise 
                //- un string
                //- un nouvel élément choose
                //- une string avec un nouvel élément choose
                string expression7 = @"<choose>
                        <when test=""{SIDE}='CR' and {PAYMENTTYPE}='Cash' and {DATE1}='2015-04-04T01:01:01'"">when Result is: {MT}.overflow-ellipsis(15tl) {MT} x {DATE1}.overflow-ellipsis(6)</when>
                        <otherwise>otherwise Result is: <choose><when test=""{SIDE}='CR' and {PAYMENTTYPE}='Cash'"">{MT}</when><otherwise>{DATE1}</otherwise></choose></otherwise>
                      </choose>";

                var context7 = new
                {
                    SIDE = "CR",
                    PAYMENTTYPE = "Cash",
                    DATE1 = new DateTime(2015, 04, 04, 1, 1, 1),
                    MT = (decimal)1232.25,
                };
                //Displays "when Result is:  1232.25 x 04/..."
                _ = StrFuncExtended.ReplaceObjectField(expression7, context7, cultureInfo);

            }

            public static void ReplaceChooseExpression2Method()
            {

                var context = new { SIDE = "CR", SIDE2 = "DR", GGGGG = "GGGGG", GGGGG2 = "GGGGG2", FFFFF = "FFFFF", FFFFF2 = "FFFFF2" };

                //Exemple 1 Interprétation des chooses 
                string expression1 = @"<choose>
                        <when test=""{SIDE}='CR'"">
                            <choose>
                                <when test=""{SIDE2}='CR'"">{GGGGG}</when>
                                <otherwise>{FFFFF2}</otherwise>
                            </choose>
                        </when>
                        <otherwise>{FFFFF}</otherwise>
                      </choose>";

                //Displays "{FFFFF2}"
                _ = StrFuncExtended.ReplaceChooseExpression2(expression1, context, true);

                //Exemple 1 (cas très particulier): interprétation du 1er choose uniquement 
                //Displays "<choose><when test="{SIDE2}='CR'">{GGGGG}</when><otherwise>{FFFFF2}</otherwise></choose>"
                _ = StrFuncExtended.ReplaceChooseExpression2(expression1, context, false);


                //Exemple 2 : Usage de &lt; et &gt;
                string expression2 = @"<choose>
                        <when test=""{SIDE}='CR'"">
                            &lt;TOTO&gt;
                            <choose>
                                <when test=""{SIDE2}='CR'""></when>
                                <otherwise>&lt;TITI{FFFFF2}&gt;</otherwise>
                            </choose>
                        </when>
                        <otherwise>{FFFFF}</otherwise>
                      </choose>";
                //Displays
                /* <TOTO>
                             <TITI{FFFFF2}>
                */
                _ = StrFuncExtended.ReplaceChooseExpression2(expression2, context, true);

            }
        }
#endif
    }
}