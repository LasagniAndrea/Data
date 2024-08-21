using EFS.ACommon;
using EFS.Common.MQueue;
using EFS.Referential;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;



namespace EFS.Common.Log
{
    /// <summary>
    /// Classe de base pour alimentation de l'audit des actions utilisateurs lors de l'utilsation du grid ou du formulaire 
    /// <para>Gère les consultations, référentiels (XML ou LST)</para>
    /// </summary>
    public class RequestTrackReferentialBuilderBase : RequestTrackBuilderBase
    {
        /// <summary>
        ///  Pilote la consultation LST/XML
        /// </summary>
        public ReferentialsReferential referential;

        /// <summary>
        ///  Représente le jeu de résultat associé à la liste ou au formulaire (1 seul enregistrement dans ce cas) 
        /// </summary>
        public DataRow[] Row
        {
            get;
            set;
        }

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public RequestTrackReferentialBuilderBase()
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

            SetDataFromDataRow(columnList, Row);
        }
    }

    /// <summary>
    /// Classe chargée d'alimenter l'audit des actions utilisateurs lors d'une consultation/action sur le formulaire (Referential.aspx)
    /// </summary>
    public class RequestTrackItemBuilder : RequestTrackReferentialBuilderBase
    {

        /// <summary>
        /// Action appliquée sur l'enregistrement
        /// <para>cette propriété doit être renseignée lorsque l'action utilisateur concerne une action (New,Modify,Remove)</para>
        /// </summary>
        public Nullable<RequestTrackProcessEnum> ProcessType
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
            RequestTrackActionDetailBase detail;
            switch (this.action.First)
            {
                case RequestTrackActionEnum.ItemLoad:
                    detail = new RequestTrackItemLoadDetail();
                    break;
                case RequestTrackActionEnum.ItemProcess:
                    if (null == this.ProcessType)
                        throw new NullReferenceException("actionType must be assigned on ItemAction");

                    detail = new RequestTrackItemProcessDetail();
                    ((RequestTrackItemProcessDetail)detail).type = ProcessType.Value;
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("RequestTrackAction {0} is not implemented", this.action.First.ToString()));
            }

            if (!(detail is IRequestTrackItem requestTrackItem))
                throw new NullReferenceException("IRequestTrackItem is not implemented on detail");


            if (ArrFunc.IsFilled(Row))
                InitRequestTrackItem(requestTrackItem);

            doc.action.detail = detail;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pItemContainer"></param>
        private void InitRequestTrackItem(IRequestTrackItem pItemContainer)
        {
            DataRow fRow = Row[0];

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
    public class RequestTrackListBuilder : RequestTrackReferentialBuilderBase
    {

        /// <summary>
        ///  true si consultation (LST), false si referential(XML) 
        /// </summary>
        public Boolean isConsultation;

        /// <summary>
        ///  Gestion des templates, critères, etc...
        /// </summary>
        public LstConsult lstConsult;

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
        public Nullable<RequestTrackProcessEnum> ProcessType
        {
            get;
            set;
        }


        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public RequestTrackListBuilder()
        {

        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// EG 20210419 [XXXXX] Upd Usage du businessCenter de l'entité
        protected override void SetActionDetail()
        {
            RequestTrackActionDetailBase ret = null;
            RequestTrackListView view = null;

            switch (action.First)
            {
                case RequestTrackActionEnum.ListExport:
                    RequestTrackListExportDetail lstExport = new RequestTrackListExportDetail
                    {
                        type = exportType.Value
                    };
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
                    if (null == ProcessType)
                        throw new NullReferenceException("processType is null");
                    lstProcess.type = this.ProcessType.Value;

                    ret = lstProcess;
                    view = lstProcess.view;
                    break;
            }

            view.identifier = lstConsult.IdLstConsult;

            //Template
            view.template.idSpecified = false;
            view.template.identifier = lstConsult.template.IDLSTTEMPLATE;
            view.template.displayName = lstConsult.template.DISPLAYNAME;
            view.template.descriptionSpecified = StrFunc.IsFilled(lstConsult.template.DESCRIPTION);
            if (view.template.descriptionSpecified)
                view.template.description = lstConsult.template.DESCRIPTION;

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
            InfosLstWhere[] where;
            if (isConsultation)
                where = lstConsult.GetInfoWhere(Cs, User.Entity_IdA, User.Entity_BusinessCenter);
            else
                where = lstConsult.GetInfoWhereFromReferencial(Cs, referential, User.Entity_IdA, User.Entity_BusinessCenter);

            view.filterSpecified = ArrFunc.IsFilled(where);


            if (view.filterSpecified)
            {
                view.filter = new RequestTrackConsultationFilter();

                string filter = string.Empty;
                for (int i = 0; i < ArrFunc.Count(where); i++)
                {
                    if (i != 0)
                        filter += Cst.Space2 + Ressource.GetString("And", true) + Cst.Space2;

                    filter += where[i].ColumnIdentifier.Replace(Cst.HTMLBreakLine, Cst.Space);
                    filter += Cst.Space;
                    filter += where[i].GetDisplayOperator();
                    if (("Checked" != where[i].Operator) && ("Unchecked" != where[i].Operator))
                    {
                        filter += Cst.Space;
                        filter += where[i].LstValue;
                    }
                }
                view.filter.literal = new CDATA(filter)
                {
                    isXmltext = false
                };
            }

            doc.action.detail = ret;

        }
    }

}