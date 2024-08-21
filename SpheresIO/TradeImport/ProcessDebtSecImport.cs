#region using
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Authenticate;
using EFS.Common;
using EFS.Common.EfsSend;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.GUI.CCI;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.TradeInformation;
using EFS.TradeInformation.Import;
//
using EfsML.Business;
using EfsML.DynamicData;
using EfsML.Interface;
//
using FixML.Enum;
//
using FpML.Interface;
#endregion

namespace EFS.SpheresIO.Trade
{
    /// <summary>
    /// Class pour importation des debtSecurity (Titres)
    /// </summary>
    internal class ProcessDebtSecImport : ProcessTradeImportBase
    {
        #region Members

        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public DebtSecCaptureGen DebtSecCaptureGen
        {
            get { return (DebtSecCaptureGen)_captureGen; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string TradeKey
        {
            get { return "DebtSecurity"; }
        }

        // FI 20130730 [18847] Mise en commentaire
        ///// <summary>
        ///// 
        ///// </summary>
        //public override bool isActorSYSTEMAvailable
        //{
        //    get { return true; }
        //}

        /// <summary>
        /// 
        /// </summary>
        public override bool IsInitFromPartyTemplateAvailable
        {
            get { return false; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsFeeCalcAvailable
        {
            get { return false; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsGetNewIdForIdentifier
        {
            get { return false; }
        }
        #endregion

        #region Constructor
        public ProcessDebtSecImport(TradeImport pTradeImport, IDbTransaction pDbTransaction, Task pTask)
            : base(pTradeImport, pDbTransaction, pTask)
        {
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        protected override void InitializeCaptureGen()
        {
            _captureGen = new DebtSecCaptureGen();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCaptureMode"></param>
        /// FI 20131213 [19337] add LogHeader
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override void CheckValidationRule(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, User pUser)
        {
            CheckDebtSecValidationRule check = new CheckDebtSecValidationRule(this.DebtSecCaptureGen.Input, pCaptureMode, pUser);
            check.ValidationRules(CSTools.SetCacheOn(pCS), pDbTransaction, CheckTradeValidationRule.CheckModeEnum.Warning);
            string msgValidationrules = check.GetConformityMsg();
            //
            if (StrFunc.IsFilled(msgValidationrules))
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                // FI 20200706 [XXXXX] SetErrorWarning delegate
                //_task.process.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                
                Logger.Log(new LoggerData(LogLevelEnum.Warning, LogHeader + Cst.CrLf + msgValidationrules));
            }
        }

        /// <summary>
        /// Contrôle la présence des paramètres nécessaires
        /// </summary>
        protected override void CheckParameter()
        {
            if (IsModeNew)
            {
                if (StrFunc.IsEmpty(GetParameter(TradeImportCst.identifier)))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("identifier not specified, parameter[scheme:{0}] is mandatory", TradeImportCst.identifier);
                    FireException(sb.ToString());

                }
                if (StrFunc.IsEmpty(GetParameter(TradeImportCst.templateIdentifier)))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("Template identifier not specified, parameter[scheme:{0}] is mandatory", TradeImportCst.templateIdentifier);
                    FireException(sb.ToString());
                }
            }
            else if (IsModeUpdate || IsModeRemoveOnlyAll)
            {
                if (StrFunc.IsEmpty(GetParameter(TradeImportCst.identifier)))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat(TradeKey + " identifier not specified, parameter[scheme:{0}] is mandatory", TradeImportCst.identifier);
                    FireException(sb.ToString());
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void ProcessSpecific()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20180507 Analyse du code Correction [CA2214]
        protected override void SetCustomCaptureInfos(string pCS)
        {
            //IsGetDefaultonInitialize = false => Les defaults sont issus du webConfig, ici il n'y en a pas
            this.CommonInput.CustomCaptureInfos = new TradeCustomCaptureInfos(pCS, CommonInput, null, string.Empty, false);
            this.CommonInput.CustomCaptureInfos.InitializeCciContainer();
        }
        #endregion
    }

}
