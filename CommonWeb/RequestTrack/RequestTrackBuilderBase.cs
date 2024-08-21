using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common.MQueue;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;


// EG 20231129 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense
namespace EFS.Common.Log
{
    /// <summary>
    /// Classe de base chargée d'alimenter le journal des actions utilisateurs 
    /// </summary>
    /// FI 20140519 [19923] add class
    public abstract class RequestTrackBuilderBase : IRequestTrackBuilder
    {
        /// <summary>
        ///  Représente le flux qui alimente le journal
        ///  <para></para>
        /// </summary>
        protected RequestTrackDocument doc;


        /// <summary>
        /// Représente l'utilisateur 
        /// </summary>
        public User User
        {
            get;
            set;
        }

        /// <summary>
        /// Représente l'application Spheres®
        /// </summary>
        public AppSession Session
        {
            get;
            set;
        }

        /// <summary>
        ///  Repésente la base de donnée
        /// </summary>
        public string Cs
        {
            get;
            set;
        }

        /// <summary>
        ///  Représente l'URL
        /// </summary>
        public string Url
        {
            get;
            set;
        }

        ///  Représente l'identifiant unique de la page web
        /// </summary>
        public string PageGuid
        {
            get;
            set;
        }

        /// <summary>
        ///  Représente l'heure de la demande
        /// </summary>
        public DateTime Timestamp
        {
            get;
            set;
        }

        /// <summary>
        /// <para>
        /// 1er element : action (exemple ListRefresh|ListExportExcel|ListExportPdf|ListExportSQL)
        /// </para>
        /// <para>
        /// 2nd element : auto|manuel (Rafraîchissement manuel ou automatique du grid)
        /// </para>
        /// </summary>
        public Pair<RequestTrackActionEnum, RequestTrackActionMode> action;


        /// <summary>
        /// 
        /// </summary>
        public RequestTrackBuilderBase()
        {
        }

        /// <summary>
        /// Crétion du flux 
        /// </summary>
        public void BuildRequestTrack()
        {
            doc = new RequestTrackDocument();

            //doc.database 
            CSManager csManager = new CSManager(Cs);
            doc.database.cs = csManager.GetCSWithoutPwd();

            //doc.request
            SetRequest();

            //doc.user
            SetUser();

            //doc.action
            SetAction();

            //doc.action
            SetActionDetail();

            //doc.data
            SetData();
        }

        /// <summary>
        /// Alimente doc.request
        /// </summary>
        private void SetRequest()
        {
            RequestTrackWebRequest ret = new RequestTrackWebRequest
            {
                timestamp = DtFunc.DateTimeToStringISO(Timestamp),
                url = new CDATA(Url),
                guid = this.PageGuid
            };
            ret.url.isXmltext = false;
            ret.session.hostname = Session.AppInstance.HostName;
            ret.session.guid = Session.SessionId;

            doc.request = ret;
        }

        /// <summary>
        /// Alimente doc.user
        /// </summary>
        private void SetUser()
        {
            RequestTrackUser ret = new RequestTrackUser();
            CopyToRequestTrackActor(CSTools.SetCacheOn(Cs), User.Identification, ret);
            ret.roleSpecified = true;
            ret.role = User.UserType.ToString();

            ret.parentSpecified = ((User.Entity_IdA > 0) || (User.Department_IdA > 0));
            if (ret.parentSpecified)
            {
                List<RequestTrackActor> lst = new List<RequestTrackActor>();

                if (User.Entity_IdA > 0)
                {
                    RequestTrackActor entity = new RequestTrackActor();
                    CopyToRequestTrackActor(CSTools.SetCacheOn(Cs), User.EntityIdentification, entity);
                    entity.roleSpecified = true;
                    entity.role = RoleActor.ENTITY.ToString();
                    lst.Add(entity);
                }
                if (User.Department_IdA > 0)
                {
                    RequestTrackActor department = new RequestTrackActor();
                    CopyToRequestTrackActor(CSTools.SetCacheOn(Cs), User.DepartmentIdentification, department);
                    department.roleSpecified = true;
                    department.role = RoleActor.DEPARTMENT.ToString();
                    lst.Add(department);
                }
                ret.parent = lst.ToArray();
            }

            doc.user = ret;
        }

        /// <summary>
        /// Déverse un acteur (type SpheresIdentification) vers un acteur (type RequestTrackActorBase) 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="actorScr"></param>
        /// <param name="actorTarget"></param>
        protected static void CopyToRequestTrackActor(string pCS, SpheresIdentification actorScr, RequestTrackActor actorTarget)
        {
            if (null == actorScr)
                throw new ArgumentException("Parameter {actorScr} is null");
            if ((actorScr.OTCmlId) == 0)
                throw new ArgumentException("Parameter {actorScr} is Zero");
            if (null == actorTarget)
                throw new ArgumentException("Parameter {actorTarget} is null");


            SQL_Actor sqlActor = new SQL_Actor(pCS, actorScr.OTCmlId);
            if (false == sqlActor.LoadTable(new string[] { "ADDRESS1", "ADDRESS2", "EXTLLINK2" }))
                throw new InvalidProgramException(StrFunc.AppendFormat("unable to load actor id:{0}", actorScr.OTCmlId.ToString()));

            actorTarget.id = actorScr.OTCmlId;
            actorTarget.idSpecified = true;
            actorTarget.identifier = actorScr.Identifier;
            actorTarget.displayName = actorScr.Displayname;
            actorTarget.descriptionSpecified = actorScr.DescriptionSpecified;
            if (actorTarget.descriptionSpecified)
                actorTarget.description = actorScr.Description;

            actorTarget.extl1Specified = actorScr.ExtllinkSpecified;
            if (actorTarget.extl1Specified)
                actorTarget.extl1 = actorScr.Extllink;

            actorTarget.extl2Specified = StrFunc.IsFilled(sqlActor.ExtlLink2);
            if (actorTarget.extl2Specified)
                actorTarget.extl2 = sqlActor.ExtlLink2;

            actorTarget.addr1Specified = StrFunc.IsFilled(sqlActor.Address1);
            if (actorTarget.addr1Specified)
                actorTarget.addr1 = sqlActor.Address1;

            actorTarget.addr2Specified = StrFunc.IsFilled(sqlActor.Address2);
            if (actorTarget.addr2Specified)
                actorTarget.addr2 = sqlActor.Address2;

        }

        /// <summary>
        /// Déverse un acteur (type int) vers un acteur (type RequestTrackActorBase) 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="actorScr"></param>
        /// <param name="actorTarget"></param>
        protected static void CopyToRequestTrackActor(string pCS, int actorScrId, RequestTrackActor actorTarget)
        {
            if (0 == actorScrId)
                throw new ArgumentException("Parameter {actorScr} == 0");
            if (null == actorTarget)
                throw new ArgumentException("Parameter {actorTarget} is null");

            SQL_Actor sqlActor = new SQL_Actor(pCS, actorScrId);
            if (false == sqlActor.LoadTable(new string[] { "IDA", "IDENTIFIER", "DISPLAYNAME", "DESCRIPTION", "ADDRESS1", "ADDRESS2", "EXTLLINK", "EXTLLINK2" }))
                throw new InvalidProgramException(StrFunc.AppendFormat("unable to load actor:Id{0}", actorScrId.ToString()));

            CopyToRequestTrackRepository(sqlActor, actorTarget);


            actorTarget.addr1Specified = StrFunc.IsFilled(sqlActor.Address1);
            if (actorTarget.addr1Specified)
                actorTarget.addr1 = sqlActor.Address1;

            actorTarget.addr2Specified = StrFunc.IsFilled(sqlActor.Address2);
            if (actorTarget.addr2Specified)
                actorTarget.addr2 = sqlActor.Address2;

        }

        /// <summary>
        /// Déverse un acteur (type int) vers un acteur (type RequestTrackActorBase) 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="actorScr"></param>
        /// <param name="actorTarget"></param>
        protected static void CopyToRequestTrackBook(string pCS, int bookScrId, RequestTrackBook bookTarget)
        {
            if (0 == bookScrId)
                throw new ArgumentException("Parameter {bookScrId} == 0");
            if (null == bookTarget)
                throw new ArgumentException("Parameter {bookTarget} is null");

            SQL_Book sqlBook = new SQL_Book(pCS, bookScrId);
            if (false == sqlBook.LoadTable(new string[] { "ID", "IDENTIFIER", "DISPLAYNAME", "DESCRIPTION", "EXTLLINK", "EXTLLINK2" }))
                throw new InvalidProgramException(StrFunc.AppendFormat("unable to load actor:Id{0}", bookScrId.ToString()));

            CopyToRequestTrackRepository(sqlBook, bookTarget);


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlTableWithID"></param>
        /// <param name="rtRepository"></param>
        protected static void CopyToRequestTrackRepository(SQL_TableWithID sqlTableWithID, RequestTrackRepository rtRepository)
        {
            if (null == sqlTableWithID)
                throw new ArgumentException("Parameter {sqlTableWithID} is null");
            if (null == sqlTableWithID)
                throw new ArgumentException("Parameter {rtRepository} is null");

            rtRepository.id = sqlTableWithID.Id;
            rtRepository.idSpecified = true;

            rtRepository.identifier = sqlTableWithID.Identifier;
            rtRepository.displayName = sqlTableWithID.DisplayName;

            rtRepository.descriptionSpecified = StrFunc.IsFilled(sqlTableWithID.Description);
            if (rtRepository.descriptionSpecified)
                rtRepository.description = sqlTableWithID.Description;

            rtRepository.extl1Specified = StrFunc.IsFilled(sqlTableWithID.ExtlLink);
            if (rtRepository.extl1Specified)
                rtRepository.extl1 = sqlTableWithID.ExtlLink;

            rtRepository.extl2Specified = StrFunc.IsFilled(sqlTableWithID.ExtlLink2);
            if (rtRepository.extl2Specified)
                rtRepository.extl2 = sqlTableWithID.ExtlLink2;
        }

        /// <summary>
        /// Envoi du flux
        /// <para> Génération d'un Message de type RequestTrackMqueue</para>
        /// </summary>
        public void SendRequestTrack()
        {

            MQueueSendInfo mqSendInfo = new MQueueSendInfo
            {
                MOMSetting = MOMSettings.LoadMOMSettings(Cst.ProcessTypeEnum.REQUESTTRACK)
            };

            RequestTrackMqueue queue = new RequestTrackMqueue(new MQueueAttributes() { connectionString = Cs })
            {
                RequestTrack = doc
            };

            MQueueTools.Send(queue, mqSendInfo);
        }

        /// <summary>
        /// Alimente doc.data
        /// </summary>
        protected virtual void SetData()
        {
        }

        /// <summary>
        /// Alimente doc.action
        /// </summary>
        protected void SetAction()
        {

            RequestTrackAction ret = new RequestTrackAction
            {
                type = action.First,
                mode = action.Second
            };

            doc.action = ret;
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void SetActionDetail()
        {
        }


        /// <summary>
        /// Alimente doc.data à partir des enregistrement présents dans n datarow
        /// </summary>
        /// <param name="column">Description des colonnes présentes dans les enregistrements qui seront déversées dans le document</param>
        /// <param name="row"></param>
        protected void SetDataFromDataRow(List<RequestTrackDataColumn> column, DataRow[] row)
        {
            if (ArrFunc.IsFilled(row))
            {
                foreach (DataRow r in row)
                {
                    foreach (RequestTrackDataColumn item in column)
                    {
                        if (r[item.columnGrp] == Convert.DBNull)
                            r[item.columnGrp] = "N/A";
                    }
                }

                //liste des groupes 
                List<string> lstGroup =
                    (
                    from r in row
                    from c in column
                    select Convert.ToString(r[c.columnGrp])).Distinct().ToList();

                RequestTrackDataGrp[] @group = new RequestTrackDataGrp[lstGroup.Count];
                for (int i = 0; i < lstGroup.Count; i++)
                {
                    @group[i] = new RequestTrackDataGrp
                    {
                        identifier = lstGroup[i]
                    };
                }

                foreach (RequestTrackDataGrp item in @group)
                {
                    // Liste des acteurs qui appartiennent à chaque groupe
                    var lstidA =
                        (
                        from r in row
                        from c in column.Where(e => StrFunc.IsFilled(e.columnIdA))
                        where ((Convert.ToString(r[c.columnGrp]) == item.identifier) && (r[c.columnIdA] != Convert.DBNull))
                        select Convert.ToInt32(r[c.columnIdA])).Distinct().ToList();

                    item.actorSpecified = (lstidA.Count > 0);
                    if (item.actorSpecified)
                        item.actor = GetRequestTrackActor(lstidA);


                    // Liste des couples book/actor qui appartiennent à grpItem
                    //List(Pair(<int, int>) idBIdA =
                    var lstIdBIdA =
                        (
                        from r in row
                        from c in column.Where(e => StrFunc.IsFilled(e.columnIdB))
                        where ((Convert.ToString(r[c.columnGrp]) == item.identifier) && (r[c.columnIdB] != Convert.DBNull))
                        select
                            new
                            {
                                idB = Convert.ToInt32(r[c.columnIdB]),
                                idA = (StrFunc.IsFilled(c.columnIdA) && (r[c.columnIdA] != Convert.DBNull)) ? Convert.ToInt32(r[c.columnIdA]) : 0
                            }
                        ).Distinct().ToList();

                    item.bookSpecified = (lstIdBIdA.Count > 0);
                    if (item.bookSpecified)
                    {
                        item.book = new RequestTrackBook[lstIdBIdA.Count];
                        for (int i = 0; i < lstIdBIdA.Count; i++)
                        {
                            int idB = lstIdBIdA[i].idB;
                            if (idB == 0)
                                throw new Exception("zero is not expected from IDB column");

                            SQL_Book sqlBook = new SQL_Book(CSTools.SetCacheOn(Cs), idB);
                            if (false == sqlBook.LoadTable(new string[] { "IDB", "IDENTIFIER", "DISPLAYNAME", "DESCRIPTION", "EXTLLINK", "EXTLLINK2", "IDA" }))
                                throw new InvalidProgramException(StrFunc.AppendFormat("unable to load book:Id{0}", idB.ToString()));

                            item.book[i] = new RequestTrackBook();
                            CopyToRequestTrackRepository(sqlBook, item.book[i]);

                            item.book[i].actorRefSpecified = lstIdBIdA[i].idA > 0;
                            if (item.book[i].actorRefSpecified)
                                item.book[i].actorRef.href = lstIdBIdA[i].idA.ToString();

                            item.book[i].ownerRef.href = sqlBook.IdA.ToString();
                        }

                        if (item.actorSpecified)
                        {
                            // Ajout des propriétaires de book non encore présents
                            var lstidAOwner =
                            (from b in item.book
                             select Convert.ToInt32(b.ownerRef.href)).Except(from a in item.actor
                                                                             select a.id).ToList();

                            if (lstidAOwner.Count > 0)
                            {

                                RequestTrackActor[] rtaOwner = GetRequestTrackActor(lstidAOwner);
                                item.actor = ((from a in item.actor select a).Union(
                                                from aOwner in rtaOwner select aOwner)).ToArray();
                            }
                        }
                        else
                        {
                            // Ajout des propriétaires de book 
                            var lstidAOwner = (from b in item.book
                                               select Convert.ToInt32(b.ownerRef.href)).ToList();

                            item.actorSpecified = (lstidAOwner.Count > 0);
                            if (item.actorSpecified)
                                item.actor = GetRequestTrackActor(lstidAOwner);
                        }
                    }
                }

                // Liste des groupes renseignés => un groupe est renseigné s'il contient au minimum un acteur
                List<RequestTrackDataGrp> lstGroupFilled =
                    (from groupItem in @group
                     where groupItem.actorSpecified == true
                     select groupItem).ToList();

                // Alimentation de doc.data
                doc.data = new RequestTrackData();
                doc.dataSpecified = (lstGroupFilled.Count > 0);
                if (doc.dataSpecified)
                    doc.data.group = lstGroupFilled.ToArray();

            }

        }


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
                CopyToRequestTrackActor(CSTools.SetCacheOn(Cs), idA[i], ret[i]);
            }
            return ret;

        }

    }

    /// <summary>
    /// Représente le nom des DataColumn 
    /// </summary>
    public class RequestTrackDataColumn
    {
        /// <summary>
        /// 
        /// </summary>
        public string columnGrp;
        /// <summary>
        /// 
        /// </summary>
        public string columnIdA;
        /// <summary>
        /// 
        /// </summary>
        public string columnIdB;


    }

}
