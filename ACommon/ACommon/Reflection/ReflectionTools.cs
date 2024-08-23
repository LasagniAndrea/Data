using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace EFS.ACommon
{
    internal class CompareFieldInfo : IComparer
    {
        public int Compare(object pFld1, object pFld2)
        {
            FieldInfo fld1 = (FieldInfo)pFld1;
            FieldInfo fld2 = (FieldInfo)pFld2;
            return (fld1.FieldHandle.Value.ToInt64().CompareTo(fld2.FieldHandle.Value.ToInt64()));
        }
    }

    public sealed class ReflectionTools
    {

        #region enum CloneStyle
        public enum CloneStyle
        {
            CloneField,
            CloneProperty,
            CloneFieldAndProperty,
        }
        #endregion enum CloneStyle

        /// <summary>
        ///  Retourne un array de type string avec les valeur de l'emun <paramref name="pEnumType"/>
        /// </summary>
        /// <param name="pEnumType"></param>
        /// <returns></returns>
        /// FI 20141204 [XXXXX] Add Method
        public static string[] EnumToStringArray(Type pEnumType)
        {
            string[] ret = null;
            if (false == pEnumType.IsEnum)
                throw new ArgumentException(StrFunc.AppendFormat("Type:{0} is not an Enum", pEnumType.ToString()));

            List<string> lst = new List<string>();
            foreach (object item in Enum.GetValues(pEnumType))
                lst.Add(item.ToString());

            if (lst.Count > 0)
                ret = lst.ToArray();

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEnum"></param>
        /// <param name="pValue"></param>
        /// <returns></returns>
        public static object EnumParse(object pEnum, string pValue)
        {
            Type tEnum = pEnum.GetType();
            if (tEnum.IsEnum)
            {
                if (false == System.Enum.IsDefined(tEnum, pValue))
                {
                    object obj = tEnum.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                    if (null != obj)
                    {
                        FieldInfo[] flds = obj.GetType().GetFields();
                        foreach (FieldInfo fld in flds)
                        {
                            object[] attributes = fld.GetCustomAttributes(typeof(XmlEnumAttribute), true);
                            if ((0 != attributes.GetLength(0)) && (pValue == ((XmlEnumAttribute)attributes[0]).Name))
                                return fld.GetValue(obj);
                        }
                    }
                }
                else
                    return System.Enum.Parse(tEnum, pValue);
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pField"></param>
        /// <param name="pType"></param>
        /// <returns></returns>
        public static bool IsBaseType(FieldInfo pField, Type pType)
        {
            Type tField = pField.FieldType;
            bool isBaseType = tField.Equals(pType);
            while (false == isBaseType)
            {
                tField = tField.BaseType;
                if (null == tField)
                    break;
                isBaseType = tField.Equals(pType);
            }
            return isBaseType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pType"></param>
        public static void SetDataTypeAttribute(object pObject, string pType)
        {
            FieldInfo fld = pObject.GetType().GetField(Cst.FpML_OTCmlTextAttribute);
            object[] attributes = fld.GetCustomAttributes(typeof(XmlTextAttribute), false);
            XmlTextAttribute xmlTextAttribute = (XmlTextAttribute)attributes[0];
            xmlTextAttribute.DataType = pType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vObj"></param>
        /// <param name="pCloneStyle"></param>
        /// <returns></returns>
        public static object Clone(object vObj, CloneStyle pCloneStyle)
        {

            Type tObj = vObj.GetType();
            if (tObj.IsValueType || tObj.Equals(typeof(System.String)))
                return vObj;
            else
            {
                //New instance Of Object
                object newObject = Activator.CreateInstance(tObj);
                Type tNewObject = newObject.GetType();

                object clone;
                #region Set FieldInfo
                if ((CloneStyle.CloneField == pCloneStyle) || (CloneStyle.CloneFieldAndProperty == pCloneStyle))
                {
                    foreach (FieldInfo item in tNewObject.GetFields())
                    {
                        if (null != item.GetValue(vObj))
                        {
                            Type tItem = item.GetType();
                            if (null != tItem.GetInterface("ICloneable"))
                            {
                                clone = ((ICloneable)item.GetValue(vObj)).Clone();
                                item.SetValue(newObject, clone);
                            }
                            else
                            {
                                if (item.GetValue(vObj).GetType().IsArray)
                                {
                                    ArrayList aObjItems = new ArrayList();
                                    foreach (object objItem in (System.Array)item.GetValue(vObj))
                                    {
                                        aObjItems.Add(Clone(objItem, pCloneStyle));
                                    }
                                    // EG 20130409
                                    //item.SetValue(newObject, aObjItems.ToArray(aObjItems[0].GetType()));
                                    if (0 < aObjItems.Count)
                                    {
                                        // RD 20151027 [21490]                                        
                                        //if (aObjItems[0].GetType().BaseType.IsAbstract && (false == aObjItems[0].GetType().IsEnum))
                                        //    item.SetValue(newObject, aObjItems.ToArray(aObjItems[0].GetType().BaseType));
                                        //else
                                        //    item.SetValue(newObject, aObjItems.ToArray(aObjItems[0].GetType()));
                                        item.SetValue(newObject, aObjItems.ToArray(aObjItems[0].GetType()));
                                    }
                                }
                                else
                                {
                                    // EG 20130409
                                    if (false == item.IsLiteral)
                                    {
                                        clone = Clone(item.GetValue(vObj), pCloneStyle);
                                        item.SetValue(newObject, clone);
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion FieldInfo

                #region set PropertyInfo
                if ((CloneStyle.CloneProperty == pCloneStyle) || (CloneStyle.CloneFieldAndProperty == pCloneStyle))
                {
                    foreach (PropertyInfo item in tNewObject.GetProperties())
                    {
                        if (item.CanWrite && (null != item.GetValue(vObj, null)))
                        {
                            Type tItem = item.GetType();
                            if (null != tItem.GetInterface("ICloneable"))
                            {
                                clone = ((ICloneable)item.GetValue(vObj, null)).Clone();
                                item.SetValue(newObject, clone, null);
                            }
                            else
                            {
                                clone = Clone(item.GetValue(vObj, null), pCloneStyle);
                                item.SetValue(newObject, clone, null);
                            }
                        }
                    }
                }
                #endregion PropertyInfo

                return newObject;
            }

        }

        /// <summary>
        /// Ajoute un item dans un membre de type Array 
        ///<para>Ne fonctionne pas sur les propriétés</para> 
        ///<para>Fonctionne sur les membres privés</para> 
        /// </summary>
        /// <param name="pParent">Instance qui contient l'élément</param>
        /// <param name="pElementName">Nom de l'élément</param>
        /// <param name="posArray"></param>
        public static void AddItemInArray(object pParent, string pElementName, int posArray)
        {
            AddOrRemoveItemInArray(pParent, pElementName, false, posArray);
        }

        /// <summary>
        /// Supprime un item dans un membre de type Array 
        ///<para>Ne fonctionne pas sur les propriétés</para> 
        ///<para>Fonctionne sur les membres privés</para> 
        /// </summary>
        /// <param name="pParent">Instance qui contient l'élément</param>
        /// <param name="pElementName">Nom de l'élément</param>
        /// <param name="posArray"></param>
        public static void RemoveItemInArray(object pParent, string pElementName, int posArray)
        {
            AddOrRemoveItemInArray(pParent, pElementName, true, posArray);
        }

        /// <summary>
        /// Ajoute ou supprime un item dans un membre de type Array 
        ///<para>Ne fonctionne pas sur les propriétés</para> 
        ///<para>Fonctionne sur les membres privés</para> 
        /// </summary>
        /// <param name="pParent">Instance qui contient l'élément</param>
        /// <param name="pElementName">Nom de l'élément</param>
        /// <param name="pIsRemove">true pour supprimer</param>
        /// <param name="posArray"></param>
        public static void AddOrRemoveItemInArray(object pParent, string pElementName, bool pIsRemove, int posArray)
        {
            if (pParent == null)
                throw new ArgumentNullException("Argument pParents is null");
            if (StrFunc.IsEmpty(pElementName))
                throw new ArgumentNullException("Argument pElementName is null");

            Type tParent = pParent.GetType();
            FieldInfo fldElement = tParent.GetField(pElementName);
            if (null == fldElement)
                fldElement = tParent.GetField(pElementName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (null == fldElement)
                throw new NullReferenceException(StrFunc.AppendFormat("Element {0}  doesn't exist in {1}", pElementName, pParent.GetType().ToString()));
            //
            object obj = fldElement.GetValue(pParent);
            ArrayList aObjects = new ArrayList();
            if (null != obj)
            {
                Array aObj = (Array)obj;
                if (pIsRemove && (posArray < aObj.Length))
                    aObj.SetValue(null, posArray);
                //
                for (int i = 0; i < aObj.Length; i++)
                {
                    if (null != aObj.GetValue(i))
                        aObjects.Add(aObj.GetValue(i));
                }
            }
            if (false == pIsRemove)
            {
                object element = fldElement.FieldType.GetElementType().InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                aObjects.Add(element);
            }
            fldElement.SetValue(pParent, aObjects.ToArray(fldElement.FieldType.GetElementType()));

            #region Set associate Specified Field (if exists)
            FieldInfo fldElementSpecified = tParent.GetField(pElementName + Cst.FpML_SerializeKeySpecified);
            if (null != fldElementSpecified)
                fldElementSpecified.SetValue(pParent, aObjects.Count > 0);
            #endregion Set associate Specified Field (if exists)

        }

        /// <summary>
        /// Affecte à null un item dans instance de type Array 
        /// </summary>
        /// <param name="pArray"></param>
        /// <param name="posArray"></param>
        public static void SetNullOnItemInArray(object pArray, int posArray)
        {
            Array array = (Array)pArray;
            if (posArray < array.Length)
                array.SetValue(null, posArray);
        }

        /// <summary>
        /// Retourne le 1er object appartenant à <paramref name="pObject"/> dont la property id vaut <paramref name="pIdValue"/>
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pIdValue"></param>
        /// <returns></returns>
        /// EG 20240105 [WI756] Spheres Core : Refactoring Code Analysis - Correctifs après tests (property Id)
        public static object GetObjectById(object pObject, string pIdValue)
        {
            //System.Diagnostics.Debug.WriteLine(pObject.ToString());
            System.Reflection.BindingFlags FpMLBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField;
            if (null != pObject)
            {
                Type tObject = pObject.GetType();
                if (tObject.IsArray)
                {
                    foreach (object objElement in (System.Array)pObject)
                    {
                        if (null != objElement)
                        {
                            PropertyInfo pty = objElement.GetType().GetProperty("Id", FpMLBindingFlags);
                            if (null != pty)
                            {
                                if (pIdValue == (string)pty.GetValue(objElement, null))
                                    return objElement;
                            }
                            object obj = GetObjectById(objElement, pIdValue);
                            if (null != obj)
                                return obj;
                        }
                    }
                }
                else
                {
                    FieldInfo[] flds = tObject.GetFields(FpMLBindingFlags);
                    foreach (FieldInfo fld in flds)
                    {
                        object objElement = fld.GetValue(pObject);
                        if (null != objElement)
                        {
                            PropertyInfo pty = objElement.GetType().GetProperty("Id", FpMLBindingFlags);
                            if (null != pty)
                            {
                                if (pIdValue == (string)pty.GetValue(objElement, null))
                                    return objElement;
                            }
                            object obj = GetObjectById(objElement, pIdValue);
                            if (null != obj)
                                return obj;
                        }
                    }
                }
            }
            return null;

        }

        /// <summary>
        /// Retourne true s'il existe au moins 1 membre nommé <paramref name="pFieldName"/> sous <paramref name="pObject"/>
        /// </summary>
        /// <para>La recherche s'applique à pObject et à ses enfants</para>
        /// <param name="pObject"></param>
        /// <param name="pFieldName"></param>
        /// <returns></returns>
        public static bool IsExistFieldName(object pObject, string pFieldName)
        {
            return ArrFunc.IsFilled(GetObjectByName(pObject, pFieldName, true));
        }

        #region public GetFieldByXmlElementAttributeName
        /// <summary>
        /// Retourne le membre de  pObject sur lequel il existe 1 attribut de serialization nommé pElementName
        /// <para>La recherche ne s'applique pas aux enfants de pObject</para>
        /// </summary>
        /// <exception cref="ArgumentException">si l'élement n'existe pas</exception>
        /// <param name="pObject"></param>
        /// <param name="pFieldName"></param>
        /// <returns></returns>
        public static Object GetFieldByXmlElementAttributeName(object pObject, string pElementName)
        {
            return GetFieldByXmlElementAttributeName(pObject, pElementName, out _);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pElementName"></param>
        /// <param name="pFld"></param>
        /// <returns></returns>
        /// FI 20121123 [18224] le paramètre pFld est de type out
        public static Object GetFieldByXmlElementAttributeName(object pObject, string pElementName, out FieldInfo pFld)
        {
            object ret = null;
            pFld = null;

            bool isFind = false;
            FieldInfo[] ArrayfieldInfo = pObject.GetType().GetFields();
            foreach (FieldInfo fieldInfo in ArrayfieldInfo)
            {
                object[] attributes = fieldInfo.GetCustomAttributes(typeof(XmlElementAttribute), true);
                if (0 < attributes.Length)
                {
                    for (int i = 0; i < ArrFunc.Count(attributes); i++)
                    {
                        XmlElementAttribute attribute = (XmlElementAttribute)attributes[i];
                        isFind = (attribute.ElementName == pElementName);
                        if (isFind)
                        {
                            ret = fieldInfo.GetValue(pObject);
                            pFld = fieldInfo;
                            break;
                        }
                    }
                }
                if (isFind)
                    break;
            }

            if (false == isFind)
                throw new ArgumentException(StrFunc.AppendFormat("XmlElementAttribute {0} doesn't exist in {1}", pElementName, pObject.GetType().ToString()));

            return ret;
        }
        #endregion

        #region public GetFieldByName
        /// <summary>
        /// Retourne le membre de pObject nommé pFieldName 
        /// <para>La recherche ne s'applique pas aux enfants de pObject</para>
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pFieldName"></param>
        /// <returns></returns>
        public static object GetFieldByName(object pObject, string pFieldName)
        {
            return GetFieldByName(pObject, pFieldName, out _);
        }
        /// <summary>
        /// Retourne le membre de pObject nommé pFieldName 
        /// <para>La recherche ne s'applique pas aux enfants de pObject</para>
        /// <para>Retourne null lorsque le champ n'existe pas</para>
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pFieldName"></param>
        /// <param name="pFld">retourne null si le champ n'existe pas</param>
        /// <returns></returns>
        /// FI 20121123 [18224] le paramètre pFld est de type out
        /// FI 20121126 [18224] cette methode ne génère plus une exeption si le membre n'existe pas
        public static object GetFieldByName(object pObject, string pFieldName, out FieldInfo pFld)
        {
            pFld = null;
            object ret = null;
            BindingFlags FpMLBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField;
            if (null != pObject && (false == pObject.GetType().IsArray))
            {
                Type tObject = pObject.GetType();
                FieldInfo[] flds = tObject.GetFields(FpMLBindingFlags);
                foreach (FieldInfo fld in flds)
                {
                    bool isFound = (fld.Name.Equals(pFieldName));
                    if (isFound)
                    {
                        ret = fld.GetValue(pObject);
                        pFld = fld;
                        break;
                    }
                }
            }
            //FI 20121126 [18224] La méthode ne génère plus une exception ( meilleur pour les perfs)
            //if (false == isFound)
            //    throw new ArgumentException(StrFunc.AppendFormat("Element {0}  doesn't exist in {1}", pFieldName, pObject.GetType().ToString()));

            return ret;
        }

        /// <summary>
        ///  Retourne la valeur du champs {fieldName} de l'objet {pObj}
        ///  <para>Cette méthode gère les type Anonyme</para>
        ///  <para>Retourne null lorsque le champ n'existe pas</para>
        /// </summary>
        /// <param name="pObj"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        /// FI 20150413 [20275] add method
        public static object GetFieldByName2(object pObj, string fieldName, out MemberInfo pMemberInfo)
        {
            pMemberInfo = null;
            object ret = null;

            if (ReflectionTools.IsAnonymousType(pObj.GetType()))
            {
                Type type = pObj.GetType();
                PropertyInfo[] fields = type.GetProperties();
                foreach (PropertyInfo fieldItem in fields)
                {
                    bool isFind = (fieldItem.Name == fieldName);
                    if (isFind)
                    {
                        ret = fieldItem.GetValue(pObj, null);
                        pMemberInfo = fieldItem;
                        break;
                    }
                }
            }
            else
            {
                ret = GetFieldByName(pObj, fieldName, out FieldInfo fieldInfo);
                if (null != fieldInfo)
                    pMemberInfo = fieldInfo;
            }

            return ret;
        }


        #endregion

        #region public GetElementByName
        /// <summary>
        /// Retourne le membre de pObject qui se nomme pElementName ou qui possède un attribut de serialization  qui se nomme pElementName
        /// <para>Retourne null si l'élément est inexistant</para>
        /// <para>Remarque: Retourne null si l'élément vaut null</para>
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pElementName"></param>
        /// <returns></returns>
        public static object GetElementByName(object pObject, string pElementName)
        {
            return GetElementByName(pObject, pElementName, out _);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pElementName"></param>
        /// <param name="pFld"></param>
        /// <returns></returns>
        /// FI 20121102 [18224] Tuning 
        /// FI 20121123 [18224] le paramètre pFld est de type out
        /// FI 20121126 [18224] la méthode GetFieldByName ne génère plus d'exception 
        // EG 20160404 Migration vs2013
        public static object GetElementByName(object pObject, string pElementName, out FieldInfo pFld)
        {
            object ret = null;
            bool isFieldFound = false;
            pFld = null;

            if (false == isFieldFound)
            {
                ret = GetFieldByName(pObject, pElementName, out pFld);
                //FI 20121102 [18224] Alimentation de isFieldFound, cela permet de ne plus passer dans GetFieldByXmlElementAttributeName 
                // EG 20160404 Migration vs2013
                //isFieldFound = (ret != pFld);
                isFieldFound = (null != pFld) && pFld.Name.Equals(pElementName);
            }

            if (false == isFieldFound)
            {
                ret = GetFieldByXmlElementAttributeName(pObject, pElementName, out pFld);
            }

            return ret;
        }
        #endregion

        /// <summary>
        /// Retourne le membre de pObject trouvé avec le chemin pFieldXPath 
        /// <para>La recherche s'applique à pObject et à ses enfants</para>
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pFieldName"></param>
        /// <param name="pExitAfterFounded"></param>
        /// <returns></returns>
        /// FI 20121123 [18224] refactoring les paramètres pFld, pSpecifiedFld et pParentObject  sont de type out
        public static object GetElementByXPath(object pObject, string pFieldXPath, out FieldInfo pFld, out FieldInfo pSpecifiedFld, out object pParentObject)
        {
            pFld = null;
            pSpecifiedFld = null;
            pParentObject = null;
            //
            object retObject = null;
            object currentObject = pObject;
            //
            string fieldXPath = pFieldXPath.Replace("//", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty);
            Regex regEx = new Regex("/");
            string[] clientId = regEx.Split(fieldXPath);
            //
            for (int i = 0; i < ArrFunc.Count(clientId) - 1; i++)
            {
                string elementName = StrFunc.PutOffSuffixNumeric(clientId[i]);
                object obj = GetElementByName(currentObject, elementName);
                //
                currentObject = null;
                if (null != obj)
                {
                    if (obj.GetType().IsArray)
                    {
                        int index = StrFunc.GetSuffixNumeric2(clientId[i]) - 1;
                        if (index < 0) index = 0; //garde fou
                                                  //
                        Array arrayObject = (Array)obj;
                        if (ArrFunc.Count(arrayObject) > index)
                            currentObject = arrayObject.GetValue(index);
                    }
                    else
                    {
                        currentObject = obj;
                    }
                }
                //
                if (null == currentObject)
                    break;
            }
            //
            if (null != currentObject)
            {
                pParentObject = currentObject;
                //
                string lastElementName = StrFunc.PutOffSuffixNumeric(clientId[ArrFunc.Count(clientId) - 1]);
                retObject = GetElementByName(currentObject, lastElementName, out pFld);
                //
                if (IsOptionalField(pParentObject, lastElementName))
                    GetElementByName(pParentObject, lastElementName + Cst.FpML_SerializeKeySpecified, out pSpecifiedFld);
                //
                // RD 20110207 / à revoir pour les types Array
                //if (null != retObject && retObject.GetType().IsArray)
                //{
                //    int index = StrFunc.GetSuffixNumeric(clientId[ArrFunc.Count(clientId) - 1]) - 1;
                //    if (index < 0) index = 0; //garde fou
                //    //
                //    Array arrayObject = (Array)retObject;
                //    if (ArrFunc.Count(arrayObject) > index)
                //        retObject = arrayObject.GetValue(index);
                //    else
                //        retObject = null;
                //}
            }
            //
            return retObject;

        }

        #region public GetObjectByName
        /// <summary>
        /// Retourne les membres nommés pFieldName dans l'objet pObject 
        /// <para>La recherche s'applique à pObject et à ses enfants</para>
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pFieldName"></param>
        /// <param name="pExitAfterFounded"></param>
        /// <returns></returns>
        public static ArrayList GetObjectByName(object pObject, string pFieldName, bool pExitAfterFounded)
        {
            FieldInfo fld = null;
            return GetObjectByName(pObject, pFieldName, pExitAfterFounded, ref fld);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pFieldName"></param>
        /// <param name="pExitAfterFounded"></param>
        /// <param name="pFld"></param>
        /// <returns></returns>
        public static ArrayList GetObjectByName(object pObject, string pFieldName, bool pExitAfterFounded, ref FieldInfo pFld)
        {
            object parentObject = null;
            return GetObjectByName(pObject, pFieldName, pExitAfterFounded, ref pFld, ref parentObject);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pFieldName"></param>
        /// <param name="pExitAfterFounded"></param>
        /// <param name="pFld"></param>
        /// <param name="pParentObject"></param>
        /// <returns></returns>
        public static ArrayList GetObjectByName(object pObject, string pFieldName, bool pExitAfterFounded, ref FieldInfo pFld, ref object pParentObject)
        {

            ArrayList objResult = new ArrayList();
            System.Reflection.BindingFlags FpMLBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField;
            if (null != pObject)
            {
                Type tObject = pObject.GetType();
                if (tObject.IsArray)
                {
                    foreach (object objElement in (System.Array)pObject)
                    {
                        objResult.AddRange(GetObjectByName(objElement, pFieldName, pExitAfterFounded, ref pFld, ref pParentObject));
                    }
                }
                else
                {
                    FieldInfo[] flds = tObject.GetFields(FpMLBindingFlags);
                    foreach (FieldInfo fld in flds)
                    {
                        object obj = fld.GetValue(pObject);
                        if ((null != obj) && fld.Name.Equals(pFieldName))
                        {
                            objResult.Add(obj);
                            pFld = fld;
                            pParentObject = pObject;
                        }
                        else
                            objResult.AddRange(GetObjectByName(obj, pFieldName, pExitAfterFounded, ref pFld, ref pParentObject));

                        if (0 < objResult.Count && pExitAfterFounded)
                            break;
                    }
                }
            }
            return objResult;

        }
        #endregion GetObjectByName

        #region public GetObjectByNameSorted
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pFieldName"></param>
        /// <param name="pExitAfterFounded"></param>
        /// <returns></returns>
        public static ArrayList GetObjectByNameSorted(object pObject, string pFieldName, bool pExitAfterFounded)
        {
            FieldInfo fld = null;
            return GetObjectByNameSorted(pObject, pFieldName, pExitAfterFounded, true, ref fld);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pFieldName"></param>
        /// <param name="pExitAfterFounded"></param>
        /// <param name="pIsFieldSorted"></param>
        /// <param name="pFld"></param>
        /// <returns></returns>
        public static ArrayList GetObjectByNameSorted(object pObject, string pFieldName, bool pExitAfterFounded, bool pIsFieldSorted, ref FieldInfo pFld)
        {

            ArrayList objResult = new ArrayList();
            //                
            if (null != pObject)
            {
                Type tObject = pObject.GetType();
                if (tObject.IsArray)
                {
                    foreach (object objElement in (System.Array)pObject)
                    {
                        objResult.AddRange(GetObjectByNameSorted(objElement, pFieldName, pExitAfterFounded, pIsFieldSorted, ref pFld));
                    }
                }
                else
                {
                    ArrayList aFields = SortFieldInfo(pObject);
                    FieldInfo fld;
                    for (int i = 0; i < aFields.Count; i++)
                    {
                        fld = (FieldInfo)aFields[i];
                        object obj = fld.GetValue(pObject);
                        if ((null != obj) && fld.Name.Equals(pFieldName))
                        {
                            objResult.Add(obj);
                            pFld = fld;
                        }
                        else
                            objResult.AddRange(GetObjectByNameSorted(obj, pFieldName, pExitAfterFounded, pIsFieldSorted, ref pFld));

                        if (0 < objResult.Count && pExitAfterFounded)
                            break;
                    }
                }
            }
            return objResult;
        }
        #endregion GetObjectByNameSorted

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCurrent"></param>
        /// <returns></returns>
        public static ArrayList SortFieldInfo(object pCurrent)
        {
            ArrayList aFlds = new ArrayList();

            bool hasContinue = true;
            Type tCurrent = pCurrent.GetType();
            FieldInfo[] flds = tCurrent.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.DeclaredOnly);

            if (ArrFunc.IsFilled(flds))
            {
                Array.Sort(flds, new CompareFieldInfo());
                aFlds.AddRange(flds);
            }
            while (hasContinue)
            {
                tCurrent = tCurrent.BaseType;
                if ((null == tCurrent) || (tCurrent.IsPrimitive) || (tCurrent.IsValueType))
                    hasContinue = false;
                else
                {
                    flds = tCurrent.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.DeclaredOnly);
                    if (ArrFunc.IsFilled(flds))
                    {
                        Array.Sort(flds, new CompareFieldInfo());
                        aFlds.InsertRange(0, flds);
                    }
                }
            }
            return aFlds;
        }

        #region public GetObjectsByType
        public static ArrayList GetObjectsByType(object pObject, System.Type pType, bool pIsCheckBaseType)
        {

            ArrayList list = new ArrayList();
            //
            if (null != pObject)
            {
                Type tObject = pObject.GetType();
                if (tObject.IsArray)
                {
                    foreach (object objElement in (System.Array)pObject)
                    {
                        ArrayList listElement = null;
                        listElement = GetObjectsByType(objElement, pType, pIsCheckBaseType);
                        if (ArrFunc.IsFilled(listElement))
                        {
                            foreach (object listElementItem in listElement)
                                list.Add(listElementItem);
                        }
                    }
                }
                else
                {
                    if ((pObject.GetType().Equals(pType) || (pObject.GetType().BaseType.Equals(pType) && pIsCheckBaseType)))
                        list.Add(pObject);

                    //
                    System.Reflection.BindingFlags FpMLBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField;
                    FieldInfo[] flds = tObject.GetFields(FpMLBindingFlags);
                    foreach (FieldInfo fld in flds)
                    {
                        object objElement = fld.GetValue(pObject);
                        if (null != objElement)
                        {
                            ArrayList listElement = GetObjectsByType(objElement, pType, pIsCheckBaseType);
                            if (ArrFunc.IsFilled(listElement))
                            {
                                foreach (object listElementItem in listElement)
                                    list.Add(listElementItem);
                            }
                        }
                    }
                }
            }
            return list;

        }
        public static ArrayList GetObjectsByType2(object pObject, Type pType, bool pIsCheckBaseType)
        {
            return GetObjectsByType2(pObject, pType, pIsCheckBaseType, null, null);
        }
        private static ArrayList GetObjectsByType2(object pObject, Type pType, bool pIsCheckBaseType, object pObjectParent, FieldInfo pField)
        {

            ArrayList list = new ArrayList();
            BindingFlags FpMLBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField;
            if (null != pObject)
            {
                Type tObject = pObject.GetType();
                if (tObject.IsArray)
                {
                    bool isAdded = (null != pObjectParent) && tObject.GetElementType().Equals(pType);
                    isAdded = isAdded && ReflectionTools.IsMandatoryField(pObjectParent, pField.Name);
                    foreach (object objElement in (Array)pObject)
                    {
                        if (isAdded)
                            list.Add(objElement);
                        ArrayList listElement = GetObjectsByType2(objElement, pType, pIsCheckBaseType);
                        if (0 < listElement.Count)
                            list.AddRange(listElement);
                    }
                }
                else
                {
                    if (null != pObjectParent)
                    {
                        if ((tObject.Equals(pType) || (tObject.BaseType.Equals(pType) && pIsCheckBaseType)))
                        {
                            // Add only if Associate Specified Field exist and is equal to true or doesn't exist
                            if (ReflectionTools.IsMandatoryField(pObjectParent, pField.Name))
                                list.Add(pObject);
                        }
                    }
                    FieldInfo[] flds = tObject.GetFields(FpMLBindingFlags);
                    foreach (FieldInfo fld in flds)
                    {
                        object objElement = fld.GetValue(pObject);
                        if (null != objElement)
                        {
                            Type tObjElement = objElement.GetType();
                            if ((tObjElement.Equals(pType) || (tObjElement.BaseType.Equals(pType) && pIsCheckBaseType)))
                            {
                                // Add only if Associate Specified Field exist and is equal to true or doesn't exist
                                if (ReflectionTools.IsMandatoryField(pObject, fld.Name))
                                    list.Add(objElement);
                            }
                            ArrayList listElement = GetObjectsByType2(objElement, pType, pIsCheckBaseType, pObject, fld);
                            if (0 < listElement.Count)
                                list.AddRange(listElement);
                        }
                    }
                }
            }
            return list;

        }
        #endregion

        /// <summary>
        /// Retourne True s'il existe un membre Specified à False associé au membre pFieldName
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pFieldName"></param>
        /// <returns></returns>
        public static bool IsOptionalField(Object pObject, string pFieldName)
        {

            bool fieldSpecifiedValue = false;
            FieldInfo fldSpecified = pObject.GetType().GetField(pFieldName + Cst.FpML_SerializeKeySpecified);
            bool isFieldSpecifiedExist = (null != fldSpecified);
            if (isFieldSpecifiedExist)
                fieldSpecifiedValue = (bool)fldSpecified.GetValue(pObject);
            //
            return (isFieldSpecifiedExist && (fieldSpecifiedValue == false));

        }

        /// <summary>
        /// Retourne True, dans les deux cas suivants:
        /// - s'il n'existe pas um membre Specified associé au membre pFieldName
        /// - s'il existe um membre Specified à True associé au membre pFieldName
        /// </summary>
        /// <param name="pObject"></param>
        /// <param name="pFieldName"></param>
        /// <returns></returns>
        public static bool IsMandatoryField(Object pObject, string pFieldName)
        {
            return (false == IsOptionalField(pObject, pFieldName));
        }

        /// <summary>
        /// Get the custom <see cref="XmlEnumAttribute"/> value for the given member
        /// </summary>
        /// <param name="pClassType">type of the class containing the searched member attribute</param>
        /// <param name="pMemberName">name of the member decorated with the XmlEnumAttribute attribute</param>
        /// <returns></returns>
        /// FI 20171025 [23533] Refactoring
        public static string GetXmlEnumAttributName(Type pClassType, string pMemberName)
        {
            string ret = String.Empty;

            XmlEnumAttribute attribute = GetAttribute<XmlEnumAttribute>(pClassType, pMemberName);
            if (null != attribute)
                ret = attribute.Name;

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClassType"></param>
        /// <param name="pValue"></param>
        /// <param name="pInherit"></param>
        /// <returns></returns>
        public static string GetMemberNameByXmlEnumAttribute(Type pClassType, string pValue, bool pInherit)
        {
            string membername = String.Empty;

            MemberInfo[] members = pClassType.GetMembers();

            foreach (MemberInfo member in members)
            {
                object[] attributes = member.GetCustomAttributes(typeof(XmlEnumAttribute), pInherit);

                XmlEnumAttribute[] xmlEnumAttributes = new XmlEnumAttribute[attributes.Length];
                attributes.CopyTo(xmlEnumAttributes, 0);

                foreach (XmlEnumAttribute attribute in xmlEnumAttributes)
                {
                    if (attribute.Name == pValue)
                    {
                        membername = member.Name;
                    }
                }
            }

            return membername;
        }

        /// <summary>
        ///  Invoke la méthode {methodName} de la classe {pClass} 
        /// </summary>
        /// <param name="pClass">Représente le type de la classe</param>
        /// <param name="pMethodName">Représente le nom de méthode</param>
        /// <param name="pArgs">Représente les arguments de la méthode</param>
        /// <returns></returns>
        /// FI 20130423 [18601] add Method
        public static Object InvokeMethod(Type pClass, string pMethodName, Object[] pArgs)
        {
            Object ret = null;
            Object obj = pClass.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
            MethodInfo method = pClass.GetMethod(pMethodName.Trim());
            if (null != method)
            {
                ParameterInfo[] parameterInfos = method.GetParameters();
                if (ArrFunc.IsEmpty(parameterInfos))
                    ret = pClass.InvokeMember(method.Name, BindingFlags.InvokeMethod, null, obj, null, null, null, null);
                else
                    ret = pClass.InvokeMember(method.Name, BindingFlags.InvokeMethod, null, obj, pArgs, null, null, null);
            }
            return ret;
        }



        /// <summary>
        /// Retourne true si le type est anonyme
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// FI 20150413 [20275] add method
        public static bool IsAnonymousType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            // HACK: The only way to detect anonymous types right now.
            return Attribute.IsDefined(type, typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false)
                && type.IsGenericType && type.Name.Contains("AnonymousType")
                && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }

        /// <summary>
        ///  Retourne le 1er attribut de type <typeparamref name="T"/> spécifié sur le membre <paramref name="pMember"/> du type <paramref name="pType"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pType"></param>
        /// <param name="pMember">Nom du membre</param>
        /// <returns></returns>
        /// FI 20171025 [23533] Add
        public static T GetAttribute<T>(Type pType, string pMember) where T : Attribute
        {
            T ret;

            MemberInfo memberInfo = pType.GetMember(pMember).FirstOrDefault();
            if (null == memberInfo)
                throw new NullReferenceException(StrFunc.AppendFormat("Member (Name:{0}) doesnt' exist (Type:{1})", pMember, typeof(T).ToString()));

            ret = (T)memberInfo.GetCustomAttributes(typeof(T), false).FirstOrDefault();

            return ret;
        }
        /// <summary>
        ///  Retourne le 1er attribut de type <typeparamref name="T"/> spécifié sur la valeur de l'enum <paramref name="enumValue"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        /// FI 20171025 [23533] Add
        public static T GetAttribute<T>(System.Enum enumValue) where T : Attribute
        {
            return GetAttribute<T>(enumValue.GetType(), enumValue.ToString());
        }

        /// <summary>
        /// Retourne la valeur par défaut de l'enum <typeparamref name="T"/>
        /// <para>Pris en considération d'un DefaultValueAttribute éventuellement présent dans la déclaration</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// FI 20171025 [23533] Add
        public static T GetDefaultValue<T>() where T : struct
        {
            DefaultValueAttribute[] attributes = (DefaultValueAttribute[])typeof(T).GetCustomAttributes(typeof(DefaultValueAttribute), false);

            if (ArrFunc.IsFilled(attributes))
            {
                return (T)attributes[0].Value;
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        ///  Convertie la valeur d'enum <paramref name="pEnumValue"/> en string 
        ///  <para>Prise en compte de l'attribut de serialization s'il est présent</para>
        /// </summary>
        /// <param name="pEnumValue"></param>
        /// <returns></returns>
        /// FI 20231129 [WI758] Add Method
        public static string ConvertEnumToString(System.Enum pEnumValue)
        {
            XmlEnumAttribute xmlEnum = GetAttribute<XmlEnumAttribute>(pEnumValue);
            string ret;
            if (null != xmlEnum)
                ret = xmlEnum.Name;
            else
                ret = pEnumValue.ToString();

            return ret;
        }


        /// <summary>
        ///  Convertie la valeur d'enum <paramref name="pEnumValue"/> en string 
        ///  <para>Prise en compte de l'attribut de serialization s'il est présent</para>
        /// </summary>
        /// <typeparam name="T">Représente un enum</typeparam>
        /// <param name="pEnumValue"></param>
        /// <returns></returns>
        /// FI 20171025 [23533] Add
        /// FI 20231129 [XXXXX]
        public static string ConvertEnumToString<T>(T pEnumValue) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }
            // FI 20231129 [WI758] call ConvertEnumToString
            return ConvertEnumToString(pEnumValue as System.Enum);
        }
        /// <summary>
        ///  Convertie la valeur string <paramref name="pValue"/> en Enum <typeparamref name="T"/>
        ///  <para>Prise en compte de l'attribut de serialization s'il est présent</para>
        /// </summary>
        /// <typeparam name="T">Représente un enum</typeparam>
        /// <param name="pValue"></param>
        /// <returns></returns>
        /// FI 20171025 [23533] Add
        // EG 20171113 Upd
        public static T ConvertStringToEnum<T>(string pValue) where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type");
            return ConvertStringToEnumOrDefault<T>(pValue, default);
        }
        // EG 20171113 New
        public static T ConvertStringToEnumOrDefault<T>(string pValue) where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type");
            return ConvertStringToEnumOrDefault(pValue, default(T));
        }
        // EG 20171113 New
        public static T ConvertStringToEnumOrDefault<T>(string pValue, T pDefaultEnumValue) where T : struct
        {
            Nullable<T> ret = ConvertStringToEnumOrNullable<T>(pValue);
            if (false == ret.HasValue)
                ret = pDefaultEnumValue;
            return ret.Value;
        }
        // EG 20171113 New
        public static Nullable<T> ConvertStringToEnumOrNullable<T>(string pValue) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            Nullable<T> ret = null;
            if (StrFunc.IsFilled(pValue))
            {
                if (false == System.Enum.IsDefined(typeof(T), pValue))
                {
                    foreach (FieldInfo fld in typeof(T).GetFields())
                    {
                        XmlEnumAttribute xmlEnum = GetAttribute<XmlEnumAttribute>(typeof(T), fld.Name);
                        if (null != xmlEnum && xmlEnum.Name == pValue)
                        {
                            ret = (T)fld.GetValue(typeof(T));
                            break;
                        }
                    }
                }
                else
                {
                    ret = (T)System.Enum.Parse(typeof(T), pValue);
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne les valeurs dans l'énumération <typeparamref name="TEnum"/> qui possède l'attribut <typeparamref name="TAttrib"/>
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <typeparam name="TAttrib"></typeparam>
        /// <returns></returns>
        /// FI 20190718 [XXXXX] Add Method
        public static List<TEnum> GetEnumValues<TEnum, TAttrib>()
            where TEnum : struct, IComparable
            where TAttrib : Attribute
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException("TEnum must be an enumerated type");
            }

            Type enumType = typeof(TEnum);
            Type attribType = typeof(TAttrib);

            List<TEnum> ret = new List<TEnum>();
            foreach (TEnum item in Enum.GetValues(enumType))
            {
                MemberInfo itemMemberInfo = enumType.GetMember(item.ToString())[0];
                if (Attribute.IsDefined(itemMemberInfo, attribType))
                    ret.Add(item);
            }
            return ret;
        }

        /// <summary>
        /// Retourne la resource associée à un enum (Gestion des ResourceAttribut lorsque spécifié)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pEnumValue"></param>
        /// <param name="pResourcePrefix">Prefix pour déterminer l'attribut ResourceAttribut(lorsque renseigné) ou pour déterminer la resource</param>
        /// <returns></returns>
        /// FI 20201102 [XXXXX] Add 
        public static string GetEnumResource<T>(Enum pEnumValue, string pResourcePrefix) where T : struct
        {
            string ret = string.Empty;

            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            FieldInfo fldAttr = pEnumValue.GetType().GetField(pEnumValue.ToString());
            object[] enumAttrs = fldAttr.GetCustomAttributes(typeof(ResourceAttribut), true);
            if (ArrFunc.IsFilled(enumAttrs))
            {
                ResourceAttribut resourceAttribut = null;
                if (StrFunc.IsFilled(pResourcePrefix))
                    resourceAttribut = (ResourceAttribut)enumAttrs.Where(x => ((ResourceAttribut)x).Resource.StartsWith(pResourcePrefix)).FirstOrDefault();
                else
                    resourceAttribut = (ResourceAttribut)enumAttrs[0];

                if (null != resourceAttribut)
                    ret = Ressource.GetString(resourceAttribut.Resource, pEnumValue.ToString());
            }

            if (StrFunc.IsEmpty(ret))
                ret = Ressource.GetString(StrFunc.IsFilled(pResourcePrefix) ? pResourcePrefix + "_" + pEnumValue.ToString() : pEnumValue.ToString(), pEnumValue.ToString()); // Même comportement que  resourcePrefix

            return ret;
        }

    }
}
