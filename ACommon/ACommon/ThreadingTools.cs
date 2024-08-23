using System;
using System.Globalization;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFS.ACommon
{
    /// <summary>
    /// Threading tools
    /// </summary>
    public class ThreadingTools
    {
        /// <summary>
        /// Retourne le {nom du Thread} [{ManagedThreadId]]
        /// </summary>
        /// <returns></returns>
        public static string GetThreadName()
        {
            string ret = "no thread name";
            if (null != Thread.CurrentThread)
            {
                if (null != Thread.CurrentThread.Name)
                {
                    ret = Thread.CurrentThread.Name;
                }
                ret += "[" + Thread.CurrentThread.ManagedThreadId.ToString() + "]";
            }
            return ret;
        }

        /// <summary>
        /// Définie la culture du thread
        /// </summary>
        /// <param name="pCulture"></param>
        public static void SetCurrentCulture(string pCulture)
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(pCulture);
            }
            catch
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(Cst.EnglishCulture);
            }

            //PL 20180924 Test in progress for fr-BE culture where ShortDatePattern = "dd-MM-yy"
            if (Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern == "dd-MM-yy")
            {
                Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern = "dd-MM-yyyy";
            }
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
        }
    }
}
