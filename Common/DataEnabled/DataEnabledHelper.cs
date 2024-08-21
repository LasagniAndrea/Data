using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;

namespace EFS.Common
{
    /// <summary>
    /// 
    /// </summary>
    public static class DataEnabledHelper
    {
        /// <summary>
        /// Suppresssion des caches sur les classes qui dérivent de <see cref="DataEnabledBase"/>
        /// </summary>
        public static int ClearCache()
        {
            return DataEnabledBase.ClearCache();
        }


        /// <summary>
        /// Suppresssion des caches  sur les classes qui dérivent de <see cref="DataEnabledBase"/> en relation avec une base de donnée
        /// </summary>
        /// <param name="cs"></param>
        public static int ClearCache(string cs)
        {
            return DataEnabledBase.ClearCache(cs);
        }

        /// <summary>
        /// Suppresssion des caches existants sur les classes qui dérivent de <see cref="DataEnabledBase"/> en relation avec une base de donnée et qui dépendent uniquement de <paramref name="table"/> 
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="table"></param>
        public static void ClearCache(string cs, Cst.OTCml_TBL table)
        {
            ClearCache(cs, new Cst.OTCml_TBL[] { table });
        }

        /// <summary>
        /// Suppresssion des caches existants sur les classes qui dérivent de <see cref="DataEnabledBase"/> en relation avec une base de donnée et qui dépendent uniquement de <paramref name="table"/> 
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="table"></param>
        public static void ClearCache(string cs, IEnumerable<Cst.OTCml_TBL> table)
        {
            if (null == table)
                throw new ArgumentNullException(nameof(table));

            IEnumerable<Type> type = GetDataEnabledType(true);

            foreach (Type item in type)
            {
                Cst.DependsOnTableAttribute[] att = (Cst.DependsOnTableAttribute[])Attribute.GetCustomAttributes(item, typeof(Cst.DependsOnTableAttribute));
                if (ArrFunc.IsFilled(att) && att.Any(x => table.Contains<Cst.OTCml_TBL>(x.Table)))
                {
                    InvokeClearCache(item, cs);
                }
            }
        }
        /// <summary>
        /// Retourne les classes qui dérivent de <see cref="DataEnabledBase"/>
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Type> GetDataEnabledType(Boolean throwIfNotExist = true)
        {
            Type dataEnabledBasetype = typeof(DataEnabledBase);

            IEnumerable<Type> type = Assembly.GetAssembly(dataEnabledBasetype).GetTypes().Where(TheType => TheType.IsClass && !TheType.IsAbstract && TheType.IsSubclassOf(dataEnabledBasetype));

            if (!type.Any() && throwIfNotExist)
                throw new InvalidOperationException($"Typebase: {typeof(DataEnabledBase).FullName}. There is no subclass of DataEnabledBase");

            return type;
        }

        /// <summary>
        /// Appel de la méthode ClearCache(string cs) du type <paramref name="type"/>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="cs">si différent de null. suppression des caches en ralation avec la base de donnée. Sinon suppression de cache dans sa totalité</param>
        private static int InvokeClearCache(Type type, string cs)
        {
            int ret = 0;

            IEnumerable<MethodInfo> methods = type.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Where(w => w.Name.Equals("ClearCache"));

            bool isMethodExists = (methods.Any());
            if (!isMethodExists)
                throw new InvalidOperationException($"Type: {type.FullName}. Method ClearCache doesn't exists.");

            Object instance = Activator.CreateInstance(type);

            MethodInfo method = methods.FirstOrDefault(f => f.GetParameters().Length == 1 && f.GetParameters()[0].ParameterType.Equals(typeof(string)));
            if (null == method)
                throw new InvalidOperationException($"Type: {type.FullName}. Method ClearCache with 1 string parameter doesn't exists.");

            ret = (int)method.Invoke(instance, new object[] { cs });

            return ret;
        }

    }
}
