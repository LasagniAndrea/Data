#region Using Directives
using System;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using System.IO;
using System.Globalization;
using System.Collections.Generic;  

using EFS.ACommon;
using EFS.Common;
using EFS.ApplicationBlocks.Data;
using EFS.Tuning;


using EfsML.Business;



using System.CodeDom.Compiler;
using Microsoft.CSharp;


#endregion Using Directives

namespace EfsML.DynamicData
{
    /// <summary>
    ///  Génération dynamique de classe c# à partir d'une liste de StringDynamicData
    /// </summary>
    /// FI 20150903 [XXXXX] Modify
    public class TypeBuilder
    {
        /// <summary>
        /// Type obtenu après compilation de la classe
        /// </summary>
        public Type binaryType;

        /// <summary>
        /// Génère un type où chaque élement de la liste devient un membre public
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pList"></param>
        /// <param name="pClassName">Nom de la classe </param>
        /// <param name="pNamespace">Nom du namespace, null accepté (EfsML.DynamicData est alors considéré)</param>
        /// FI 20150914 [XXXXX] Modify
        /// FI 20180328 [23871] Modify
        public TypeBuilder(string pCs, List<StringDynamicData> pList, string pClassName, string pNamespace)
        {
            if (null == pList)
                throw new ArgumentNullException(nameof(pList));

            string declarationItem = "public {0} {1} = {2};";

            string declarations = string.Empty;
            foreach (StringDynamicData item in pList)
            {
                string itemValue = item.GetDataValue(pCs, null);
                string declaration = string.Empty;
                // FI 20150903 [XXXXX] possibilité d'avoir des type nullable sur decimal, boolean, integer
                // FI 20150914 [XXXXX] gestion du "bool"
                switch (item.datatype.ToLower())
                {
                    case "decimal":
                    case "boolean":
                    case "bool":
                    case "integer":
                    case "int":
                        string datatype = item.datatype.ToLower();
                        if ((datatype == "integer") || (datatype == "int"))
                            datatype = "int32";
                        else if (datatype == "bool")
                            datatype = "boolean";

                        if (StrFunc.IsEmpty(itemValue))
                            itemValue = "null";
                        else if (datatype == "decimal")
                            itemValue += "m";

                        declaration = StrFunc.AppendFormat(declarationItem, StrFunc.FirstUpperCase(datatype) + "?", item.name, itemValue);
                        break;
                    case "string":
                    case "text":
                        if (itemValue == null)
                            declaration = StrFunc.AppendFormat(declarationItem, StrFunc.FirstUpperCase("string"), item.name, "null");
                        else
                            declaration = StrFunc.AppendFormat(declarationItem, StrFunc.FirstUpperCase("string"), item.name, @"""" + itemValue + @"""");
                        break;
                    case "date":
                        DateTime date = new DtFunc().StringDateISOToDateTime(itemValue);
                        string sdate = StrFunc.AppendFormat("new DateTime({0},{1},{2})", date.Year.ToString(), date.Month.ToString(), date.Day.ToString());
                        declaration = StrFunc.AppendFormat(declarationItem, "DateTime", item.name, sdate);
                        break;
                    case "datetime":
                        DateTime datetime = new DtFunc().StringDateTimeISOToDateTime(itemValue);
                        string sdatetime = StrFunc.AppendFormat("new DateTime({0},{1},{2},{3},{4},{5})",
                            datetime.Year.ToString(), datetime.Month.ToString(), datetime.Day.ToString(),
                            datetime.Hour.ToString(), datetime.Minute.ToString(), datetime.Second.ToString());
                        declaration = StrFunc.AppendFormat(declarationItem, "DateTime", item.name, sdatetime);
                        break;
                    case "time":
                        // FI 20211901 [XXXXX] une donné time donne naissance à un type TimeSpan ou Nullable<TimeSpan>
                        // le type TimeSpan est IComparable et est, par conséquent, mieux adapté pour l'usage de comparaison   
                        DateTime datetime2 = new DtFunc().StringTimeISOToDateTime(itemValue);
                        if (itemValue == null)
                            declaration = StrFunc.AppendFormat(declarationItem, "Nullable<TimeSpan>", item.name, "null");
                        else
                        {
                            string sTimeSpan = StrFunc.AppendFormat("new TimeSpan({0},{1},{2})",
                            datetime2.Hour.ToString(), datetime2.Minute.ToString(), datetime2.Second.ToString());
                            declaration = StrFunc.AppendFormat(declarationItem, "TimeSpan", item.name, sTimeSpan);
                        }
                        break;
                    default:
                        throw new NotImplementedException(StrFunc.AppendFormat("type :{0} is not implemented", item.datatype));
                }
                declarations += declaration + Cst.CrLf;
            }

            string @namespace = pNamespace;
            if (StrFunc.IsEmpty(pNamespace))
                @namespace = "EfsML.DynamicData";


            string className = pClassName;

            string code = @"
            using System;
            namespace {0}
            {                
                public class {1}
                {                
                    {2}        
                }
            }";

            code = code.Replace("{0}", @namespace).Replace("{1}", className).Replace("{2}", declarations);

            CSharpCodeProvider provider = new CSharpCodeProvider();
            
            CompilerResults results = provider.CompileAssemblyFromSource(new CompilerParameters(), code);

            // FI 20180328 [23871] new Exception is HasErrors
            if (results.Errors.HasErrors)
                throw new Exception($"Unvalide c# code:\r\n{code},\r\nPathToAssembly:{results.PathToAssembly}");

            binaryType = results.CompiledAssembly.GetType(@namespace + "." + className);
        }

        /// <summary>
        /// Retourne une nouvelle instance du type {binaryType} à l'aide du constructeur par défaut
        /// </summary>
        /// <returns></returns>
        /// EG 20161122 GetNewObject (before GetNewObjec)
        public object GetNewObject()
        {
            object newObject = Activator.CreateInstance(binaryType);
            
            


            return newObject;



        }
    }
}
