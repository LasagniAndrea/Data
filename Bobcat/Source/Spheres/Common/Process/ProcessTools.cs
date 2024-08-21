using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using EFS.ACommon;
using EFS.Common;

namespace EFS.Process
{
    public sealed class ProcessTools
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static Dictionary<Cst.ServiceEnum, List<Pair<Cst.ProcessTypeEnum, string>>> CreateDicServiceProcess()
        {
            Dictionary<Cst.ServiceEnum, List<Pair<Cst.ProcessTypeEnum, string>>>
                ret = new Dictionary<Cst.ServiceEnum, List<Pair<Cst.ProcessTypeEnum, string>>>();
            
            foreach (Cst.ProcessTypeEnum process in Enum.GetValues(typeof(Cst.ProcessTypeEnum)))
            {
                FieldInfo fldAttr = process.GetType().GetField(process.ToString());
                object[] enumAttrs = fldAttr.GetCustomAttributes(typeof(Cst.ProcessRequestedAttribute), true);
                if (ArrFunc.IsFilled(enumAttrs))
                {
                    string _process = (process == Cst.ProcessTypeEnum.IRQ ? "IRQ2" : process.ToString());
                    Pair<Cst.ProcessTypeEnum, string> pair = new Pair<Cst.ProcessTypeEnum, string>(process, Ressource.GetString(_process));
                    Cst.ServiceEnum service = ((Cst.ProcessRequestedAttribute)enumAttrs[0]).ServiceName;
                    if (Cst.ServiceEnum.NA != service)
                    {
                        if (false == ret.ContainsKey(service))
                        {
                            List<Pair<Cst.ProcessTypeEnum, string>> lstProcess = new List<Pair<Cst.ProcessTypeEnum, string>>
                            {
                                pair
                            };
                            ret.Add(service, lstProcess);
                        }
                        else
                        {
                            ret[service].Add(pair);
                        }
                    }
                }
            }
            return ret;
        }
        /// <summary>
        /// Fonction d'alimentation de la trace (et du log) en cas d'erreur sur un traitement multithreading
        /// La tâche qui vient de s'achever est en erreur (IsFaulted = true)
        /// - Récupération de la totalité des exceptions élémentaires dans l'exception aggrégée et écriture dans la trace.
        /// - mise à jour du statut (FAILURE) si demandé (pProcessState not null)
        /// - Génération d'un throw et dans ce cas sortie du traitement multithreading (pIsThrow = true) 
        /// </summary>
        /// <param name="pSource">Source de l'appel</param>
        /// <param name="pTask">Tâche achevée</param>
        /// <param name="pTitleMessage">Titre du message dans trace (TXT)</param>
        /// <param name="pProcessState">processState en cours</param>
        /// <param name="pIsThrow">Indicateur de génération de throw</param>
        // EG 20230928 [26506] New 
        public static void AddTraceExceptionAndProcessStateFailure(object pSource, Task pTask, string pTitleMessage, ProcessState pProcessState = null, bool pIsThrow = false)
        {
            if (pTask.IsFaulted)
            {
                AggregateException agEx = pTask.Exception.Flatten();
                string error = ExceptionTools.GetMessageAndStackExtended(agEx);
                Common.AppInstance.TraceManager.TraceError(pSource, $"ERROR {pTitleMessage} : {error}");
                if (null != pProcessState)
                    pProcessState.SetErrorWarning(ProcessStateTools.StatusEnum.ERROR, Cst.ErrLevel.FAILURE);
                if (pIsThrow)
                    throw agEx;
            }
        }
    }
}
