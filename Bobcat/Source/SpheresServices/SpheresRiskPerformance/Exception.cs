using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

using EFS.ACommon;

namespace EFS.ParseException
{
    /// <summary>
    /// Helper class to parse a generic CLR exception and build the relative OTCMlException
    /// </summary>
    public static class OTCmlExceptionParser 
    {
        /// <summary>
        /// RegEx finding all the spheres namespaces
        /// </summary>
        /// <remarks>in case you want to add more prefixes, you can add another pipe condition in the first capturing group</remarks>
        static Regex regEFSExNameSpaces = new Regex(@"^(?:EFS|EfsML|SpheresProcessBase|SpheresServiceBase){1}(?:\.\w+)+$", RegexOptions.IgnoreCase);

        /// <summary>
        /// Get the OTCmlException instance which boxes the exception input parameter
        /// </summary>
        /// <param name="pMessage">Custom exception message, it could be empty</param>
        /// <param name="pEx">Exception to parse, not null</param>
        /// <returns>the OTCMlException instance, null in case of the exception input parameter is null</returns>
        public static OTCmlException GetOTCmlException(string pMessage, Exception pEx)
        {
            OTCmlException resEx = null;

            if (pEx is OTCmlException)
            {
                resEx = (OTCmlException)pEx;
            }
            else if (pEx != null)
            {
                string methodName = GetFirstEFSMethodFromStack(pEx);

                resEx = new OTCmlException(methodName, pMessage, pEx);
            }

            return resEx;
        }

        /// <summary>
        /// Get the first EFS method in the stacktrace of the given exception 
        /// </summary>
        /// <param name="pEx">Exception to parse, not null</param>
        /// <returns>the first method  of the exceptions stacktrace</returns>
        private static string GetFirstEFSMethodFromStack(Exception pEx)
        {
            string methodName = Cst.NotAvailable;

            StackTrace stack = new StackTrace(pEx);

            StackFrame[] frames = stack.GetFrames();

            foreach (StackFrame frame in frames)
            {
               MethodBase method =  frame.GetMethod();

               string methodNameSpace = method.ReflectedType.Namespace;

               bool ok = IsEFSNameSpace(methodNameSpace);

               if (ok)
               {
                   methodName = String.Format("{0}.{1}", method.ReflectedType.FullName, method.Name);
                   break;
               }
            }

            return methodName;
        }
        
        private static bool IsEFSNameSpace(string methodNameSpace)
        {
            Match match = regEFSExNameSpaces.Match(methodNameSpace);

            return match.Success;
        }
    }
}