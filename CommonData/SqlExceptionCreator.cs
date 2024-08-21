using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Data.SqlClient;

namespace EFS.ApplicationBlocks.Data
{

    /// <summary>
    /// Génération via reflexion d'un SqlException
    /// </summary>
    /// FI 20210730 [XXXXX] Add
    public static class SqlExceptionCreator
    {
        /// <summary>
        /// Génération d'une SqlException 
        /// <para>Permet par exemple de simuler l'exception levée lorsqu'un deadlock se produit</para>
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static SqlException Create(string msg, int number)
        {
            Exception innerEx = null;
            var c = typeof(SqlErrorCollection).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            SqlErrorCollection errors = (c[0].Invoke(null) as SqlErrorCollection);
            var errorList = (errors.GetType().GetField("errors", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(errors) as System.Collections.ArrayList);
            c = typeof(SqlError).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            var nineC = c.FirstOrDefault(f => f.GetParameters().Length == 7);
            SqlError sqlError = (nineC.Invoke(new object[] { number, (byte)0, (byte)0, "", "", "", (int)0 }) as SqlError);
            errorList.Add(sqlError);
            SqlException ex = (Activator.CreateInstance(typeof(SqlException), BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { msg, errors,
            innerEx, Guid.NewGuid() }, null) as SqlException);
            return ex;
        }
    }
}
