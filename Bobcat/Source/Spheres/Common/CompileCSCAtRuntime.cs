using System;
using System.Collections;
using System.Collections.Generic; 
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace EFS.Common
{
    public static class CompileCSCAtRuntime
    {
        public static decimal ExecuteFunctionP4(string pCode, decimal p1, decimal p2, decimal p3, decimal p4)
        {
            decimal result = 0;
            MethodInfo function = CreateFunctionP4(pCode);
            var dlgFunction = (Func<decimal, decimal, decimal, decimal, decimal>)Delegate.CreateDelegate(typeof(Func<decimal, decimal, decimal, decimal, decimal>), function);

            //result = (decimal)function.Invoke(null, new object[] { p1, p2, p3, p4 });
            result = dlgFunction(p1, p2, p3, p4);

            return result;
        }
        public static MethodInfo CreateFunctionP4(string pCode)
        {
            string nameSpace = "EFS.UserFunctions";
            string className = "UserFunction";
            string pMethodName = "Function";
            string code = @"using System;
            namespace {0}
            {                
                public class {1}
                {                
                    public static decimal {2}(decimal pR1, decimal pA1, decimal pR2, decimal pA2)
                    {
                        return {3};
                    }
                }
            }";

            pCode = pCode.Replace("@R1", "pR1").Replace("@A1", "pA1");
            pCode = pCode.Replace("@R2", "pR2").Replace("@A2", "pA2");

            //code = String.Format(code, nameSpace, className, pMethodName, pCode);
            code = code.Replace("{0}", nameSpace).Replace("{1}", className);
            code = code.Replace("{2}", pMethodName).Replace("{3}", pCode);

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerResults results = provider.CompileAssemblyFromSource(new CompilerParameters(), code);
            //PL TEST IN PROGRESS... TBD ERROR MANAGEMENT

            Type binaryFunction = results.CompiledAssembly.GetType(nameSpace + "." + className);
            return binaryFunction.GetMethod(pMethodName);
        }
    }
    //----------------------------------------------------------------








}