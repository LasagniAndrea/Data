using System;
using System.Linq;
using System.Collections.Specialized;
using System.Collections.Generic;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.Referential;
using EfsML.DynamicData;

namespace EFS.Controls
{
    /// <summary>
    /// Mode Consultation Fixe (Filtre seulement) (ie: Repository, Log, Price, ...)
    /// </summary>
    public partial class TemplateDataGrid
    {
        /// <summary>
        /// 
        /// </summary>
        private void XMLInit_TitleAndObjecName()
        {
            // EG 20151019 [21465] New
            GUIName = NVC_QueryString["GUIName"];

            //Repository
            ListType = Cst.ListType.Repository;
            Title = ListType.ToString();
            ObjectName = NVC_QueryString[Title];

            if (StrFunc.IsEmpty(ObjectName))
            {
                //TRIM
                ListType = Cst.ListType.TRIM;
                Title = ListType.ToString();
                ObjectName = NVC_QueryString[Title];
            }
            if (StrFunc.IsEmpty(ObjectName))
            {
                //Log
                ListType = Cst.ListType.Log;
                Title = ListType.ToString();
                ObjectName = NVC_QueryString[Title];
            }
            if (StrFunc.IsEmpty(ObjectName))
            {
                //Price
                ListType = Cst.ListType.Price;
                Title = ListType.ToString();
                ObjectName = NVC_QueryString[Title];
            }
            if (StrFunc.IsEmpty(ObjectName))
            {
                //ProcessBase
                ListType = Cst.ListType.ProcessBase;
                Title = ListType.ToString();
                ObjectName = NVC_QueryString[Title];
            }
            if (StrFunc.IsEmpty(ObjectName))
            {
                //ESR
                ListType = Cst.ListType.SettlementMsg;
                Title = ListType.ToString();
                ObjectName = NVC_QueryString[Title];
            }
            if (StrFunc.IsEmpty(ObjectName))
            {
                //CFR
                ListType = Cst.ListType.ConfirmationMsg;
                Title = ListType.ToString();
                ObjectName = NVC_QueryString[Title];
            }
            if (StrFunc.IsEmpty(ObjectName))
            {
                //Accounting
                ListType = Cst.ListType.Accounting;
                Title = ListType.ToString();
                ObjectName = NVC_QueryString[Title];
            }
            if (StrFunc.IsEmpty(ObjectName))
            {
                //Trade
                ListType = Cst.ListType.Trade;
                Title = ListType.ToString();
                ObjectName = NVC_QueryString[Title];
            }
            if (StrFunc.IsEmpty(ObjectName))
            {
                //TradeAdmin
                ListType = Cst.ListType.Invoicing;
                Title = ListType.ToString();
                ObjectName = NVC_QueryString[Title];
            }
            if (StrFunc.IsEmpty(ObjectName))
            {
                //Report (Consultation spécifique, donc non basée sur LSTCONSULT)
                ListType = Cst.ListType.Report;
                Title = ListType.ToString();
                ObjectName = NVC_QueryString[Title];
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDynamicArgs"></param>
        /// <param name="pIsLoadLSTParam">si true, les valeurs des pDynamicArgs de type GUI sont remplacées par les valeurs présentes dans LSTPARAM</param>
        /// FI 20141211 [20563] Modify
        /// FI 20200205 [XXXXX] pDynamicArgs est de type ReferentialsReferentialStringDynamicData
        /// FI 20200602 [25370] Add
        private void XMLInit_LoadReferential2(Dictionary<string, ReferentialsReferentialStringDynamicData> pDynamicArgs, bool pIsLoadLSTParam)
        {
            string cs = SessionTools.CS;

            TitleMenu = NVC_QueryString["TitleMenu"];
            TitleRes = NVC_QueryString["TitleRes"];


            string valueFK = NVC_QueryString["FK"] + string.Empty;

            queryStringDA = NVC_QueryString["DA"] + string.Empty;

            //Renseignement des données necessaires pour les critères     
            //2070219 PL Utilisation de lstTemplate/P1 pour corriger le pb. des critères sur des référentiels partageants un XML
            //20121024 EG Concatenation P1 et Objectname (voir par exemple menu commun EAR,ACCOUNTGEN pour COMMON et ADMIN
            string objectNameTmp = NVC_QueryString["P1"];
            //if (StrFunc.IsEmpty(objectNameTmp))
            objectNameTmp += ObjectName;
            LocalLstConsult = new LstConsult(cs, ReferentialWeb.PrefixForReferential + Title + objectNameTmp, IDMenu);


            #region Get last used template
            IdLstTemplate = string.Empty;
            IdA = 0;
            // FI 20200602 [25370] isNewTemporaryTemplate true si des paramètres GUI ont été forcés via des DA spécifiés dans l'URL (Ils sont alors flaggé GUI et URL)
            // FI 20200602 [25370] isNewTemporaryTemplate (uniquement si false == Page.IsPostBack)
            bool isNewTemporaryTemplate = (false == Page.IsPostBack) &&
                (StrFunc.IsFilled(valueFK) || StrFunc.IsFilled(valueForFilter) ||
                (pDynamicArgs.Values.Where(x => x.source.HasFlag(DynamicDataSourceEnum.GUI) && x.source.HasFlag(DynamicDataSourceEnum.URL)).Count() > 0));

            bool isExistFilter = StrFunc.IsFilled(valueForFilter);

            Boolean isCreateTemporaryTemplate = ((TemplateDataGridPage.IsOptionalFilterDisabled) ||
                                                (TemplateDataGridPage.PositionFilterDisabled > -1) || StrFunc.IsFilled(TemplateDataGridPage.ClientIdCustomObjectChanged));
            ReferentialWeb.GetTemplate((PageBase)Page, LocalLstConsult,
                                                isCreateTemporaryTemplate,
                                                isNewTemporaryTemplate, isExistFilter, out string tmp_IdLstTemplate, out int tmp_IdA);
            IdLstTemplate = tmp_IdLstTemplate;
            IdA = tmp_IdA;

            if (!ReferentialWeb.ExistsTemplate(cs, LocalLstConsult.IdLstConsult, IdLstTemplate, IdA))
                throw new Exception(StrFunc.AppendFormat("Template not found for consult {0}, Template {1}, Actor {2}", LocalLstConsult.IdLstConsult, IdLstTemplate, IdA.ToString()));
            #endregion

            AfterGetTemplate(cs, ReferentialModeEnum.XML, pDynamicArgs, pIsLoadLSTParam, isNewTemporaryTemplate, valueForFilter);

            //FI 20141211 [20563] pDynamicArgs est passé à la méthode DeserializeXML_ForModeRW
            List<string> ObjectNameForDeserialize = ReferentialTools.GetObjectNameForDeserialize(IDMenu, ObjectName);

            // FI 20201215 [XXXXX] Alimentation du paramètre pValueFK
            ReferentialTools.DeserializeXML_ForModeRW(cs, GetListType(), ObjectNameForDeserialize, condApp, param, pDynamicArgs, valueFK, out ReferentialsReferential rr);
            Referential = rr;

            SetIsLoadDataOnStart(rr, isNewTemporaryTemplate);

            // ConsultationMode
            // EG 20121029
            if (StrFunc.IsFilled(NVC_QueryString["M"]))
                Referential.consultationMode = (Cst.ConsultationMode)Enum.Parse(typeof(Cst.ConsultationMode), NVC_QueryString["M"]);

            ReferentialTools.InitializeReferentialForGrid(Referential);

            if (isNewTemporaryTemplate && isExistFilter)
            {
                // insertion dans LSTWhere s'il existe un filtre
                SetIndexForFilter();
                Pair<string, Pair<string, string>> filter = new Pair<string, Pair<string, string>>(valueForFilter, null);
                if (indexKeyField >= 0)
                    filter.Second = new Pair<string, string>(Referential.TableName, Referential[indexKeyField].ColumnName);

                ReferentialWeb.InsertFilter(cs, LocalLstConsult.IdLstConsult, LocalLstConsult.template.IDLSTTEMPLATE, LocalLstConsult.template.IDA, filter);
            }
            ReferentialWeb.AddLSTWHEREToReferential(cs, LocalLstConsult.IdLstConsult, LocalLstConsult.template.IDLSTTEMPLATE, LocalLstConsult.template.IDA, Referential);


            // FI 20200220 [XXXXX] Mise en commentaire. L'appel est effectué dans TemmplateDatagrid.LoadReferential 
            //sessionName_LstColumn = ReferentialWeb.SaveInSession_LstColumn((PageBase)this.Page, localLstConsult.IdLstConsult, IdLstTemplate, IdA, referential);
        }

        
    }
}