using System.Diagnostics;

namespace EFS.Common.Log
{
    public static class ErrorLogTools
    {
        public static string GetMethodName()
        {
            string methodName = string.Empty;
            // Ajout de la méthode appelante
            StackTrace st = new StackTrace();
            if (st.FrameCount >= 1)
            {
                StackFrame sf = st.GetFrame(1);
                methodName = sf.GetMethod().DeclaringType.Name + "." + sf.GetMethod().Name;
            }
            return methodName;
        }
    }
}
