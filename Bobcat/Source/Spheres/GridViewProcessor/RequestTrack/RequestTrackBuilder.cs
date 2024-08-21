using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

#region using directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.GridViewProcessor;
using EFS.Common.MQueue;
#endregion using directives

namespace EFS.Common.Log
{
    /// <summary>
    /// Classe de base pour alimentation de l'audit des actions utilisateurs lors de l'utilsation du grid ou du formulaire 
    /// <para>Gère les consultations, référentiels (XML ou LST)</para>
    /// </summary>
    public class RequestTrackRepositoryBuilderBase : RequestTrackBuilderBase
    {
        /// <summary>
        ///  Pilote la consultation LST/XML
        /// </summary>
        public Referential referential;

        /// <summary>
        ///  Représente le jeu de résultat associé à la liste ou au formulaire (1 seul enregistrement dans ce cas) 
        /// </summary>
        public DataRow[] row
        {
            get;
            set;
        }

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public RequestTrackRepositoryBuilderBase()
        {

        }
        #endregion

        /// <summary>
        /// Alimente la partie data du document avec les éléments présents dans la liste ou dans le formulaire
        /// </summary>
        protected override void SetData()
        {
            if (false == referential.RequestTrackSpecified)
                throw new InvalidProgramException("RequestTrack is not specified");

            List<RequestTrackDataColumn> columnList = (from
                                                      rtd in referential.RequestTrack.RequestTrackData
                                                       select new RequestTrackDataColumn
                                                       {
                                                           columnGrp = rtd.columnGrp.alias,
                                                           columnIdA = rtd.columnIdASpecified ? rtd.columnIdA.alias : string.Empty,
                                                           columnIdB = rtd.columnIdBSpecified ? rtd.columnIdB.alias : string.Empty,
                                                       }).ToList();

            SetDataFromDataRow(columnList, row);
        }
    }

    /// <summary>
    /// Classe chargée d'alimenter l'audit des actions utilisateurs lors d'une consultation/action sur le formulaire (Referential.aspx)
    /// </summary>
    public class RequestTrackRepositoryBuilder : RequestTrackRepositoryBuilderBase
    {

        /// <summary>
        /// Action appliquée sur l'enregistrement
        /// <para>cette propriété doit être renseignée lorsque l'action utilisateur concerne une action (New,Modify,Remove)</para>
        /// </summary>
        public Nullable<RequestTrackProcessEnum> processType
        {
            get;
            set;
        }


        /// <summary>
        /// Alimente le détail de la partie action
        /// </summary>
        /// FI 20141021 [20350] Modify (gestion du mode RequestTrackActionEnum.ItemAction)
        protected override void SetActionDetail()
        {
            RequestTrackActionDetailBase detail = null;

            switch (this.action.First)
            {
                case RequestTrackActionEnum.ItemLoad:
                    detail = new RequestTrackItemLoadDetail();
                    break;
                case RequestTrackActionEnum.ItemProcess:
                    if (null == this.processType)
                        throw new NullReferenceException("actionType must be assigned on ItemAction");

                    detail = new RequestTrackItemProcessDetail();
                    ((RequestTrackItemProcessDetail)detail).type = processType.Value;
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("RequestTrackAction {0} is not implemented", this.action.First.ToString()));
            }

            IRequestTrackItem requestTrackItem = detail as IRequestTrackItem;
            if (null == requestTrackItem)
                throw new NullReferenceException("IRequestTrackItem is not implemented on detail");


            if (ArrFunc.IsFilled(row))
                InitRequestTrackItem(requestTrackItem);

            doc.action.detail = detail;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pItemContainer"></param>
        private void InitRequestTrackItem(IRequestTrackItem pItemContainer)
        {
            DataRow fRow = row[0];

            pItemContainer.Item.type = referential.TableName;

            pItemContainer.Item.idSpecified = false;
            if (IntFunc.IsPositiveInteger(fRow[referential.IndexColSQL_DataKeyField].ToString()))
            {
                pItemContainer.Item.idSpecified = true;
                pItemContainer.Item.id = Convert.ToInt32(fRow[referential.IndexColSQL_DataKeyField].ToString());
            }

            pItemContainer.Item.identifier = string.Empty;
            if (referential.ExistsColumnKeyField)
                pItemContainer.Item.identifier = fRow[referential.IndexColSQL_KeyField].ToString();

            pItemContainer.Item.displayName = string.Empty;
            if (referential.ExistsColumnDISPLAYNAME)
                pItemContainer.Item.displayName = fRow[referential.IndexColSQL_DISPLAYNAME].ToString();

            pItemContainer.Item.descriptionSpecified = referential.ExistsColumnDESCRIPTION;
            if (pItemContainer.Item.descriptionSpecified)
                pItemContainer.Item.description = fRow[referential.IndexColSQL_DESCRIPTION].ToString();

            pItemContainer.Item.extl1Specified = referential.ExistsColumnEXTLLINK;
            if (pItemContainer.Item.extl1Specified)
                pItemContainer.Item.extl1 = fRow[referential.IndexColSQL_EXTLLINK].ToString();

            pItemContainer.Item.extl2Specified = referential.ExistsColumnEXTLLINK2;
            if (pItemContainer.Item.extl2Specified)
                pItemContainer.Item.extl2 = fRow[referential.IndexColSQL_EXTLLINK2].ToString();
        }

    }

    /// <summary>
    /// Classe chargée d'alimenter l'audit des actions utilisateurs lors d'une consultation d'une liste (List.aspx)
    /// </summary>
    public class RequestTrackListViewerBuilder : RequestTrackRepositoryBuilderBase
    {

        /// <summary>
        ///  true si consultation (LST), false si referential(XML) 
        /// </summary>
        public Boolean isConsultation;

        /// <summary>
        ///  Gestion des templates, critères, etc...
        /// </summary>
        public LstConsultData consult;

        /// <summary>
        ///  Paramètres de la consultation (Exemple consultation des postions synthétiques)
        /// </summary>
        public MQueueparameter[] parameter;

        /// <summary>
        ///  Type d'export lorsque la liste est constituée suite à une demande d'exportation
        /// </summary>
        public Nullable<RequestTrackExportType> exportType;

        /// <summary>
        ///  Nom du report lorsque la liste est constituée  suite à une demande d'exportation PDF
        /// </summary>
        public string reportName;


        /// <summary>
        /// process appliquée sur les enregistrements
        /// </summary>
        public Nullable<RequestTrackProcessEnum> processType
        {
            get;
            set;
        }


        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public RequestTrackListViewerBuilder()
        {

        }
        #endregion

        /// <summary>
        ///  Retourne 
        /// </summary>
        /// <param name="idA"></param>
        /// <returns></returns>
        private RequestTrackActor[] GetRequestTrackActor(List<Int32> idA)
        {
            RequestTrackActor[] ret = new RequestTrackActor[idA.Count];
            for (int i = 0; i < idA.Count; i++)
            {
                ret[i] = new RequestTrackActor();
                CopyToRequestTrackActor(CSTools.SetCacheOn(cs), idA[i], ret[i]);
            }
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        protected override void SetActionDetail()
        {
            RequestTrackActionDetailBase ret = null;
            RequestTrackListView view = null;

            switch (action.First)
            {
                case RequestTrackActionEnum.ListExport:
                    RequestTrackListExportDetail lstExport = new RequestTrackListExportDetail();

                    lstExport.type = exportType.Value;
                    if (exportType.Value == RequestTrackExportType.PDF)
                    {
                        lstExport.reportSpecified = true;
                        lstExport.report = new RequestTrackRepository { identifier = this.reportName };
                    }
                    ret = lstExport;
                    view = lstExport.view;
                    break;
                case RequestTrackActionEnum.ListLoad:
                    RequestTrackListLoadDetail lstLoad = new RequestTrackListLoadDetail();
                    ret = lstLoad;
                    view = lstLoad.view;
                    break;
                case RequestTrackActionEnum.ListProcess:
                    RequestTrackListProcessDetail lstProcess = new RequestTrackListProcessDetail();
                    if (null == processType)
                        throw new NullReferenceException("processType is null");
                    lstProcess.type = this.processType.Value;

                    ret = lstProcess;
                    view = lstProcess.view;
                    break;
            }

            view.identifier = consult.IdLstConsult;

            //Template
            view.template.idSpecified = false;
            view.template.identifier = consult.template.IDLSTTEMPLATE;
            view.template.displayName = consult.template.DISPLAYNAME;
            view.template.descriptionSpecified = StrFunc.IsFilled(consult.template.DESCRIPTION);
            if (view.template.descriptionSpecified)
                view.template.description = consult.template.DESCRIPTION;

            //Parameters
            view.parameterSpecified = ArrFunc.IsFilled(this.parameter);
            if (view.parameterSpecified)
                view.parameter = this.parameter;
            //Subtilité sur les paramerets => Afin de supprimer l'attribut id dans le résultat de la serialization, id est remplacé par name 
            if (view.parameterSpecified)
            {
                foreach (MQueueparameter parameter in view.parameter)
                {
                    parameter.nameSpecified = true;
                    parameter.name = parameter.id;
                    parameter.id = null;
                }
            }

            //filter
            LstWhereData[] where = null;
            if (isConsultation)
                where = consult.GetInfoWhere(cs, user.entity_IdA);
            else
                where = consult.GetInfoWhereFromReferential(cs, referential, user.entity_IdA);

            view.filterSpecified = ArrFunc.IsFilled(where);


            if (view.filterSpecified)
            {
                view.filter = new RequestTrackConsultationFilter();

                string filter = string.Empty;
                for (int i = 0; i < ArrFunc.Count(where); i++)
                {
                    if (i != 0)
                        filter += Cst.Space2 + Ressource.GetString("And", true) + Cst.Space2;

                    filter += where[i].columnIdentifier.Replace(Cst.HTMLBreakLine, Cst.Space);
                    filter += Cst.Space;
                    filter += where[i].GetDisplayOperator();
                    if (("Checked" != where[i].@operator) && ("Unchecked" != where[i].@operator))
                    {
                        filter += Cst.Space;
                        filter += where[i].lstValue;
                    }
                }
                view.filter.literal = new CDATA(filter);
                view.filter.literal.isXmltext = false;
            }

            doc.action.detail = ret;

        }
    }

}