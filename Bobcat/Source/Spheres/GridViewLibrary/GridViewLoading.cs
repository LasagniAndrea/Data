#region using directives
using EFS.ACommon;
using EFS.Common.Web;
using EFS.GridViewProcessor;
using EfsML.DynamicData;
using System;
using System.Collections.Generic;
#endregion using directives

namespace EFS.Controls
{
    /// <summary>
    /// Mode Consultation Multi-critères (Affichage, Filtre, Tri,Totaux) (ie: Trade, ...)
    /// </summary>
    public partial class GridViewTemplate
    {
        /// <summary>
        /// 
        /// </summary>
        private void SQLInit_TitleAndObjecName()
        {
            //Consultation
            ListType = Cst.ListType.Consultation;
            Title = ListType.ToString();
            ObjectName = NVC_QueryString[Title];
            //
            //if (StrFunc.IsFilled(NVC_QueryString[Cst.ListType.ProcessBase.ToString()]))
            if (StrFunc.IsFilled(NVC_QueryString["ProcessName"]))
            {
                //ProcessBase (Warning: on conserve ici l'ObjectName issu de Consultation)
                ListType = Cst.ListType.ProcessBase;
                Title = ListType.ToString();
            }
        }
        /// <summary>
        /// Initialisation Mode LST  spécifique aux consultations
        /// </summary>
        /// <param name="pDynamicDatas"></param>
        /// <param name="pParam"></param>
        private void SQLInit_LoadReferential(Dictionary<string, StringDynamicData> pDynamicArgs)
        {
            string cs = ((PageBase)Page).CS;

            ((PageBase)Page).AddAuditTimeStep("SQLInit_LoadReferential", true, pDynamicArgs.ToString());

            //20090610 PL Gestion de valueFK en mode Multi-critères pour DEBTSECURITY
            valueFK = NVC_QueryString["FK"] + string.Empty;
            string idLstTemplate = string.Empty;
            int idA = SessionTools.Collaborator_IDA;
            //
            consult = new LstConsultData(cs, ObjectName, IDMenu);
            //20090429 PL 
            bool isloadNewTemplate = ((StrFunc.IsFilled(valueFK) || StrFunc.IsFilled(valueForFilter)));
            //isloadNewTemplate = false; //PL 20110929 A REVOIR AU CAS OU... (Mis à FALSE pour le zoom sur la PO détaillée depuis la PO synthétique)
            isloadNewTemplate = StrFunc.IsFilled(valueForFilter); //PL 20120529 A REVOIR AU CAS OU... (Mis ainsi pour le bouton "..." d'accès au référentiel DebtSecurity depuis la saisie des trades)
            //NB: - Bouton "..." d'accès au référentiel DebtSecurity: valueForFilter est valorisé
            //    - PO détaillée depuis la PO synthétique: valueFK est valorisé

            bool isLoadOnStart = false; //PL/CC False par défaut pour tout ce qui est consultation LST (5 boutons)

            //localLstConsult.GetLastUse((PageBase)Page, isloadNewTemplate, valueForFilter, null, null, isLoadOnStart, out idLstTemplate, out idA);
            RepositoryWeb.GetTemplate((PageBase)Page, consult.IdLstConsult, consult.Title, isloadNewTemplate, valueForFilter, null, null, isLoadOnStart, out idLstTemplate, out idA);

            //PL 20150601 GUEST New feature
            if (SessionTools.IsSessionGuest || RepositoryWeb.ExistsTemplate(cs, consult.IdLstConsult, idLstTemplate, idA))
            {
                consult.LoadTemplate(cs, idLstTemplate, idA);

                if (false == isApplyOptionalFilter)
                    consult.template.SetIsEnabledLstWhere(cs, idA, consult.template.IDLSTTEMPLATE, consult.template.IDLSTCONSULT, false);

                if (positionFilterDisabled > -1)
                    consult.template.SetEnabledLstWhere(cs, idA, consult.template.IDLSTTEMPLATE, consult.template.IDLSTCONSULT, positionFilterDisabled, false);

                bool isConsultWithDynamicArgs = StrFunc.IsFilled(NVC_QueryString["DA"]);
                consult.LoadLstDatatable(cs, isConsultWithDynamicArgs);

                referential = consult.GetReferentials(cs, condApp, param, pDynamicArgs).Items[0];
                //FI 20141211 [20563] Mise en commentaire
                //referential.SetDynamicArgs(pDynamicDatas);
                referential.Initialize(true, null, condApp, param, pDynamicArgs);
                RepositoryTools.InitializeID(ref referential);

                //Dans le cas d'une consultation, s'il existe des champs de type <IsResource> ou <IsSide>
                //alors, on initialise referential pour utiliser les TemplateColumn au lieu des BoundColumn
                if (referential.IsDataGridWithTemplateColumn)
                {
                    RepositoryTools.InitializeReferentialForGrid((PageBase)this.Page, ref referential, string.Empty);
                }
            }
            else
                throw new Exception(StrFunc.AppendFormat("Template not found for consult {0}, Template {1}, Actor {2}", consult.IdLstConsult, idLstTemplate, idA.ToString()));

            ((PageBase)Page).AddAuditTimeStep("SQLInit_LoadReferential", false);
        }
        private void XMLInit_TitleAndObjecName()
        {
            // EG 20151019 [21465] New
            GUIName = NVC_QueryString["GUIName"];

            //Referential
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
        /// <param name="pDynamicDatas"></param>
        /// FI 20141211 [20563] Modify
        private void XMLInit_LoadReferential(Dictionary<string, StringDynamicData> pDynamicArgs)
        {
            string cs = ((PageBase)Page).CS;
            //
            TitleMenu = NVC_QueryString["TitleMenu"];
            TitleRes = NVC_QueryString["TitleRes"];
            //
            Cst.ListType listeTypeEnum = Cst.ListType.Repository;
            if (Enum.IsDefined(typeof(Cst.ListType), Title))
                listeTypeEnum = (Cst.ListType)Enum.Parse(typeof(Cst.ListType), Title);

            //FI 20141211 [20563] pDynamicArgs est passé à la méthode DeserializeXML_ForModeRW
            List<string> ObjectNameForDeserialize = RepositoryTools.GetObjectNameForDeserialize(IDMenu, ObjectName);
            RepositoryTools.DeserializeXML_ForModeRW(cs, IDMenu, listeTypeEnum, ObjectNameForDeserialize, condApp, param, pDynamicArgs, out referential);
            // ConsultationMode
            // EG 20121029
            if (StrFunc.IsFilled(NVC_QueryString["M"]))
                referential.consultationMode = (Cst.ConsultationMode)Enum.Parse(typeof(Cst.ConsultationMode), NVC_QueryString["M"]);
            //ValueDataKeyField
            string valueForeignKeyField = NVC_QueryString["FK"];
            RepositoryTools.InitializeReferentialForGrid((PageBase)this.Page, ref referential, valueForeignKeyField);
            //FI 20141211 [20563] Mise en commentaire 
            //referential.SetDynamicArgs(pDynamicDatas);
            //
            queryStringDA = NVC_QueryString["DA"] + string.Empty;
            valueFK = NVC_QueryString["FK"] + string.Empty;
            //
            //Renseignement des données necessaires pour les critères     
            //2070219 PL Utilisation de lstTemplate/P1 pour corriger le pb. des critères sur des référentiels partageants un XML
            //20121024 EG Concatenation P1 et Objectname (voir par exemple menu commun EAR,ACCOUNTGEN pour COMMON et ADMIN
            string objectNameTmp = NVC_QueryString["P1"];
            //if (StrFunc.IsEmpty(objectNameTmp))
            objectNameTmp += ObjectName;
            consult = new LstConsultData(cs, RepositoryWeb.PrefixForReferential + Title + objectNameTmp, IDMenu);

            #region Get last used template
            IdLstTemplate = string.Empty;
            IdA = 0;
            bool isloadNewTemplate = (StrFunc.IsFilled(valueFK) || StrFunc.IsFilled(valueForFilter));
            int indexForFilter = GetIndexForFilter();
            string columnForFilter = null;
            string tableForFilter = null;
            if (indexForFilter >= 0)
            {
                columnForFilter = referential[indexForFilter].ColumnName;
                tableForFilter = referential.TableName;
            }
            string tmp_IdLstTemplate;
            int tmp_IdA;
            RepositoryWeb.GetTemplate((PageBase)Page, consult.IdLstConsult, consult.Title, isloadNewTemplate, valueForFilter, tableForFilter, columnForFilter, referential.LoadOnStart, out tmp_IdLstTemplate, out tmp_IdA);
            IdLstTemplate = tmp_IdLstTemplate;
            IdA = tmp_IdA;

            if (false == RepositoryWeb.ExistsTemplate(cs, consult.IdLstConsult, IdLstTemplate, IdA))
                throw new Exception(StrFunc.AppendFormat("Template not found for consult {0}, Template {1}, Actor {2}", consult.IdLstConsult, IdLstTemplate, IdA.ToString()));
            #endregion

            consult.LoadTemplate(cs, IdLstTemplate, IdA, isloadNewTemplate);

            if (false == isApplyOptionalFilter)
                consult.template.SetIsEnabledLstWhere(cs, IdA, consult.template.IDLSTTEMPLATE, consult.template.IDLSTCONSULT, false);

            if (positionFilterDisabled > -1)
                consult.template.SetEnabledLstWhere(cs, IdA, consult.template.IDLSTTEMPLATE, consult.template.IDLSTCONSULT, positionFilterDisabled, false);

            consult.AddLSTWHEREToReferential(cs, IdLstTemplate, IdA, ref referential);

            //sessionName_LstColumn = ReferentialWeb.SaveInSession_LstColumn((PageBase)Page, consult.IdLstConsult, IdLstTemplate, IdA, referential);
            sessionName_LstColumn = RepositoryWeb.SaveInSession_LstColumn2((PageBase)Page, consult.IdLstConsult, IdLstTemplate, IdA, referential);
        }

    }
}