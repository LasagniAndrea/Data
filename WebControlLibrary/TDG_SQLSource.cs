using System;
using System.Data;
using System.Linq;
using System.Collections.Specialized;
using System.Collections.Generic;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.Referential;
using EfsML.Business;
using EfsML.DynamicData;

namespace EFS.Controls
{
    /// <summary>
    /// Mode Consultation Multi-critères (Affichage, Filtre, Tri,Totaux) (ie: Trade, ...)
    /// </summary>
    public partial class TemplateDataGrid
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
        /// <param name="pDynamicArgs"></param>
        /// <param name="pIsLoadLSTParam">si true, les valeurs des pDynamicArgs de type GUI sont remplacées par les valeurs présentes dans LSTPARAM</param>
        /// FI 20200205 [XXXXX] pDynamicArgs est de type ReferentialsReferentialStringDynamicData
        /// FI 20200602 [25370] Refactoring
        private void SQLInit_LoadReferential(Dictionary<string, ReferentialsReferentialStringDynamicData> pDynamicArgs, bool pIsLoadLSTParam)
        {
            if (null == pDynamicArgs)
                throw new ArgumentNullException(@"pDynamicArgs argument is null");

            string cs = SessionTools.CS;

            ((PageBase)Page).AddAuditTimeStep("SQLInit_LoadReferential", true, pDynamicArgs.ToString());
            
            //20090610 PL Gestion de valueFK en mode Multi-critères pour DEBTSECURITY
            string valueFK = NVC_QueryString["FK"] + string.Empty;

            LocalLstConsult = new LstConsult(cs, ObjectName, IDMenu);

            // PL 20090429
            // FI 20200602 [25370] isNewTemporaryTemplate true si des paramètres GUI ont été forcés via des DA spécifiés dans l'URL (Ils sont alors flaggé GUI et URL)
            // FI 20200602 [25370] isNewTemporaryTemplate (uniquement si false == Page.IsPostBack)
            bool isNewTemporaryTemplate = (false == Page.IsPostBack)  &&    
                (StrFunc.IsFilled(valueFK) || StrFunc.IsFilled(valueForFilter) ||
                (pDynamicArgs.Values.Where(x => x.source.HasFlag(DynamicDataSourceEnum.GUI) && x.source.HasFlag(DynamicDataSourceEnum.URL)).Count() > 0));

            bool isExistFilter = StrFunc.IsFilled(valueForFilter);
            //NB: - Bouton "..." d'accès au référentiel DebtSecurity: valueForFilter est valorisé
            //    - PO détaillée depuis la PO synthétique: valueFK est valorisé

            // FI 20200602 [25370] add isCreateTemporaryTemplate
            Boolean isCreateTemporaryTemplate = (TemplateDataGridPage.IsOptionalFilterDisabled) ||
                                                (TemplateDataGridPage.PositionFilterDisabled > -1) ||
                                                StrFunc.IsFilled(TemplateDataGridPage.ClientIdCustomObjectChanged);

            ReferentialWeb.GetTemplate((PageBase)Page, LocalLstConsult,
                isCreateTemporaryTemplate,
                isNewTemporaryTemplate, isExistFilter, out string idLstTemplate, out int idA);
            //FI 2020021 [XXXXX] pour faire comme pour XMLInit_LoadReferential
            IdLstTemplate = idLstTemplate;
            IdA = idA;

            //PL 20150601 GUEST New feature
            if (SessionTools.IsSessionGuest || ReferentialWeb.ExistsTemplate(cs, LocalLstConsult.IdLstConsult, IdLstTemplate, IdA))
            {
                // FI 20200602 [25370] call InitAfterGetTemplate
                AfterGetTemplate(cs, ReferentialModeEnum.SQL, pDynamicArgs, pIsLoadLSTParam, isNewTemporaryTemplate, valueForFilter);

                Referential = LocalLstConsult.GetReferentials().Items[0];
                /* FI 20201215 [XXXXX] Mise en commentaire
                // FI 20201214 [XXXXX] Alimentation de valueForeignKeyField
                referential.valueForeignKeyField = valueFK;
                */

                // FI 20201215 [XXXXX] Alimenattion du paramètre pValueFK
                Referential.Initialize(true, condApp, param, pDynamicArgs, valueFK);
                ReferentialTools.InitializeID(Referential);

                //Dans le cas d'une consultation, s'il existe des champs de type <IsResource> ou <IsSide>
                //alors, on initialise referential pour utiliser les TemplateColumn au lieu des BoundColumn
                if (Referential.IsDataGridWithTemplateColumn)
                    ReferentialTools.InitializeReferentialForGrid(Referential);

                SetIsLoadDataOnStart(Referential, isNewTemporaryTemplate);

            }
            else
                throw new Exception(StrFunc.AppendFormat("Template not found for consult {0}, Template {1}, Actor {2}", LocalLstConsult.IdLstConsult, idLstTemplate, idA.ToString()));

            ((PageBase)Page).AddAuditTimeStep("SQLInit_LoadReferential", false);
        }

        /// <summary>
        ///  Alimentation de {pDynamicArgs} à partir des données présentes dans la table LSTPARAM
        /// </summary>
        /// <param name="pLstTablePARAM">représente la table LSTPARAM en mémoire</param>
        /// <param name="pDynamicArgs"></param>
        /// FI 20200602 [25370] Add 
        /// EG 20210419 [XXXXX] Upd Usage du businessCenter de l'entité
        private static void SetReferentialDynamicArgGUI(DataTable pLstTablePARAM, IEnumerable<ReferentialsReferentialStringDynamicData> pDynamicArgs)
        {
            if (null == pDynamicArgs)
                throw new ArgumentException("pDynamicArgs argument is null");
            if (pDynamicArgs.Where(x => !(x.source.HasFlag(DynamicDataSourceEnum.GUI))).Count() > 0)
                throw new ArgumentException("Only GUI dynamicData are expected");

            if ((pDynamicArgs.Count() > 0) && pLstTablePARAM.Rows.Count > 0)
            {
                //Replace des éventuelles valeurs 
                foreach (ReferentialsReferentialStringDynamicData item in pDynamicArgs)
                {
                    DataRow[] row = pLstTablePARAM.Select(StrFunc.AppendFormat("PARAMNAME='{0}'", item.name));
                    if (row.Length == 1)
                    {
                        if (row[0]["PARAMVALUE"] == Convert.DBNull)
                        {
                            item.value = null;
                        }
                        else
                        {
                            string paramValue = Convert.ToString(row[0]["PARAMVALUE"]); // la colonne PARAMVALUE est de type String
                            if (TypeData.IsTypeDateTimeOffset(item.datatype) || TypeData.IsTypeDate(item.datatype))
                            {
                                //Interprétation d'un éventuel mot clé (TODAY, BUSINESS, etc..)
                                DtFuncML dtFuncML = new DtFuncML(SessionTools.CS, SessionTools.User.Entity_BusinessCenter, SessionTools.User.Entity_IdA, 0, 0, null);

                                string fmt = TypeData.IsTypeDateTimeOffset(item.datatype) ? DtFunc.FmtTZISOLongDateTime : DtFunc.FmtISODate;
                                item.value = dtFuncML.GetDateTimeString(paramValue, fmt);
                            }
                            else if (TypeData.IsTypeInt(item.datatype))
                            {
                                //Interprétation du mot clé ENTITY
                                if (paramValue == "ENTITY" && SessionTools.User.Entity_IdA > 0)
                                    item.value = SessionTools.User.Entity_IdA.ToString();
                                else
                                    item.value = paramValue;
                            }
                            else
                            {
                                item.value = paramValue;
                            }
                        }
                    }
                }
            }
        }
    }
}