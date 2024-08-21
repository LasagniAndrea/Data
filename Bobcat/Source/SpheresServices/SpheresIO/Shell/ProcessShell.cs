#region using
using System;
using System.Collections;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common.IO;
using EfsML.DynamicData;

#endregion

namespace EFS.SpheresIO
{

    /// <summary>
    /// 
    /// </summary>
    public class ProcessShell : ProcessShellBase
    {
        #region Members
        protected Task m_Task;
        #endregion Members

        #region Constructor
        public ProcessShell(Task pTask, string pIdIOShell)
            : base(pTask.Cs, pIdIOShell, pTask.AppInstance)
        {
            m_Task = pTask;
        }
        #endregion Constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pParamData"></param>
        /// <returns></returns>
        private DataParameter ValuateDataParameter(ParamData pParamData)
        {
            string Value = string.Empty;
            ValuateDataParameter(pParamData, ref Value);
            //
            pParamData.sql = null;
            pParamData.spheresLib = null;
            pParamData.value = Value;
            //
            return pParamData.GetDataParameter(m_Task.Cs, null, GetSqlCommandType());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pParamData"></param>
        /// <param name="pValue"></param>
        private void ValuateDataParameter(ParamData pParamData, ref string pValue)
        {
            pValue = string.Empty;
            IOMappedData mappedData = new IOMappedData
            {
                datatype = pParamData.datatype,
                dataformat = pParamData.dataformat,
                value = pParamData.value,
                name = pParamData.name,
                spheresLib = pParamData.spheresLib,
                sql = pParamData.sql,
                Default = null,
                Controls = null
            };

            string result = null;
            ProcessMapping.GetMappedDataValue(m_Task, m_Task.SetErrorWarning, mappedData, null, ref result);
            if (null != result)
                pValue = result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override DataParameters GetSqlParameter()
        {
            DataParameters ret = new DataParameters();

            if (null != m_SIArgumentsParamData)
            {
                foreach (ParamData paramData in m_SIArgumentsParamData)
                    ret.Add(ValuateDataParameter(paramData));
            }

            return ret;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override string GetDataParameter()
        {

            string ret = string.Empty;
            //
            if (null != m_SIArgumentsParamData)
            {
                ArrayList retArray = new ArrayList();
                //
                foreach (ParamData paramData in m_SIArgumentsParamData)
                {
                    string Value = string.Empty;
                    //
                    ValuateDataParameter(paramData, ref Value);
                    //
                    retArray.Add(Value);
                }
                //
                ret = ArrFunc.GetStringList(retArray, ";");
            }
            //
            return ret;

        }
    }
}