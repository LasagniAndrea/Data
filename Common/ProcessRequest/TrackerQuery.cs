using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;


namespace EFS.Process
{
    /// <summary>
    /// 
    /// </summary>
    public class TrackerQuery
    {
        #region Members
        private readonly string _cs;
        #endregion Members

        #region Constructors
        // EG 20180525 [23979] IRQ Processing
        public TrackerQuery(string pCS)
        {
            _cs = pCS;
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20180525 [23979] IRQ Processing
        public QueryParameters GetQuerySelect()
        {
            // FI 20201030 [25537] add NONEMSG
            string sqlSelect = @"
select trk.GROUPTRACKER, trk.READYSTATE, trk.STATUSTRACKER, trk.IDDATAIDENT, trk.IDDATA, trk.IDDATAIDENTIFIER, 
trk.SYSCODE, trk.SYSNUMBER, 
trk.DATA1, trk.DATA2, trk.DATA3, trk.DATA4, trk.DATA5, 
trk.POSTEDMSG, trk.POSTEDSUBMSG, 
trk.NONEMSG, trk.SUCCESSMSG, trk.WARNINGMSG, trk.ERRORMSG, 
trk.IDAINS, trk.DTINS, trk.IDSESSIONINS, trk.HOSTNAMEINS, 
trk.EXTLID, trk.ACKXML, trk.EXTLLINK, trk.ROWATTRIBUT,
ac.IDENTIFIER as IDAINS_IDENTIFIER
from dbo.TRACKER_L trk
inner join dbo.ACTOR ac on (ac.IDA = trk.IDAINS)
where (trk.IDTRK_L = @IDTRK_L)" + Cst.CrLf;

            DataParameters param = new DataParameters();
            param.Add(new DataParameter(_cs, "IDTRK_L", DbType.Int32));

            QueryParameters ret = new QueryParameters(_cs, sqlSelect.ToString(), param);

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// EG 20180525 [23979] IRQ Processing
        /// FI 20201030 [25537] Add NONEMSG
        public QueryParameters GetQueryInsert()
        {
            StrBuilder sqlInsert = new StrBuilder();
            sqlInsert += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.TRACKER_L.ToString() + Cst.CrLf;
            sqlInsert += @"(IDTRK_L, GROUPTRACKER, READYSTATE, STATUSTRACKER, IDDATAIDENT, IDDATA," + Cst.CrLf;
            sqlInsert += @"IDDATAIDENTIFIER, SYSCODE, SYSNUMBER, POSTEDMSG, POSTEDSUBMSG, NONEMSG, SUCCESSMSG, WARNINGMSG, ERRORMSG, " + Cst.CrLf;
            sqlInsert += @"DATA1, DATA2, DATA3, DATA4, DATA5, " + Cst.CrLf;
            sqlInsert += @"IDAINS, DTINS, IDSESSIONINS, HOSTNAMEINS," + Cst.CrLf;
            sqlInsert += @"EXTLID, ACKXML, ACKSTATUS, IRQIDTRK_L," + Cst.CrLf;
            sqlInsert += @"EXTLLINK, ROWATTRIBUT)" + Cst.CrLf;
            sqlInsert += @"values" + Cst.CrLf;
            sqlInsert += @"(@IDTRK_L, @GROUPTRACKER, @READYSTATE, @STATUSTRACKER, @IDDATAIDENT, @IDDATA, " + Cst.CrLf;
            sqlInsert += @"@IDDATAIDENTIFIER, @SYSCODE, @SYSNUMBER, @POSTEDMSG, @POSTEDSUBMSG, @NONEMSG, @SUCCESSMSG, @WARNINGMSG, @ERRORMSG, " + Cst.CrLf;
            sqlInsert += @"@DATA1, @DATA2, @DATA3, @DATA4, @DATA5, " + Cst.CrLf;
            sqlInsert += @"@IDAINS, @DTINS, @IDSESSIONINS, @HOSTNAMEINS, " + Cst.CrLf;
            sqlInsert += @"@EXTLID, @ACKXML, @ACKSTATUS, @IRQIDTRK_L, " + Cst.CrLf;
            sqlInsert += @"@EXTLLINK, @ROWATTRIBUT)" + Cst.CrLf;

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(_cs, "IDTRK_L", DbType.Int32));
            parameters.Add(new DataParameter(_cs, "GROUPTRACKER", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN));
            parameters.Add(new DataParameter(_cs, "READYSTATE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN));
            parameters.Add(new DataParameter(_cs, "STATUSTRACKER", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN));
            parameters.Add(new DataParameter(_cs, "IDDATA", DbType.Int32));
            parameters.Add(new DataParameter(_cs, "IDDATAIDENT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
            parameters.Add(new DataParameter(_cs, "IDDATAIDENTIFIER", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN));
            parameters.Add(new DataParameter(_cs, "SYSCODE", DbType.AnsiString, SQLCst.UT_EVENT_LEN));
            parameters.Add(new DataParameter(_cs, "SYSNUMBER", DbType.Int32));
            parameters.Add(new DataParameter(_cs, "POSTEDMSG", DbType.Int32));
            parameters.Add(new DataParameter(_cs, "POSTEDSUBMSG", DbType.Int32));
            parameters.Add(new DataParameter(_cs, "NONEMSG", DbType.Int32));
            parameters.Add(new DataParameter(_cs, "SUCCESSMSG", DbType.Int32));
            parameters.Add(new DataParameter(_cs, "WARNINGMSG", DbType.Int32));
            parameters.Add(new DataParameter(_cs, "ERRORMSG", DbType.Int32));
            parameters.Add(new DataParameter(_cs, "DATA1", DbType.AnsiString, 128));
            parameters.Add(new DataParameter(_cs, "DATA2", DbType.AnsiString, 128));
            parameters.Add(new DataParameter(_cs, "DATA3", DbType.AnsiString, 128));
            parameters.Add(new DataParameter(_cs, "DATA4", DbType.AnsiString, 128));
            parameters.Add(new DataParameter(_cs, "DATA5", DbType.AnsiString, 128));
            parameters.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.IDAINS));
            parameters.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.DTINSDATETIME2));
            parameters.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.HOSTNAMEINS));
            parameters.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.IDSESSIONINS));
            parameters.Add(new DataParameter(_cs, "EXTLID", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN));
            parameters.Add(new DataParameter(_cs, "ACKXML", DbType.Xml));
            parameters.Add(new DataParameter(_cs, "ACKSTATUS", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN));
            parameters.Add(new DataParameter(_cs, "IRQIDTRK_L", DbType.Int32));
            parameters.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.EXTLLINK));
            parameters.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.ROWATTRIBUT));

            QueryParameters ret = new QueryParameters(_cs, sqlInsert.ToString(), parameters);
            return ret;

        }

        /// <summary>
        /// Maj de GROUPTRACKER, READYSTATE, STATUSTRACKER
        /// </summary>
        /// <returns></returns>
        public QueryParameters GetQueryUpdate()
        {

            StrBuilder sqlUpdate = new StrBuilder();
            sqlUpdate += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.TRACKER_L.ToString() + Cst.CrLf;
            sqlUpdate += SQLCst.SET + "GROUPTRACKER=@GROUPTRACKER, READYSTATE=@READYSTATE, STATUSTRACKER=@STATUSTRACKER, " + Cst.CrLf;
            sqlUpdate += @"IDAUPD=@IDAUPD, DTUPD=@DTUPD, IDSESSIONUPD=@IDSESSIONUPD, HOSTNAMEUPD=@HOSTNAMEUPD" + Cst.CrLf;
            sqlUpdate += SQLCst.WHERE + "IDTRK_L = @IDTRK_L";

            DataParameters dataparameters = new DataParameters();
            dataparameters.Add(new DataParameter(_cs, "IDTRK_L", DbType.Int32));
            dataparameters.Add(new DataParameter(_cs, "GROUPTRACKER", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN));
            dataparameters.Add(new DataParameter(_cs, "READYSTATE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN));
            dataparameters.Add(new DataParameter(_cs, "STATUSTRACKER", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN));
            dataparameters.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.IDAUPD));
            dataparameters.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.DTUPDDATETIME2));
            dataparameters.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.HOSTNAMEUPD));
            dataparameters.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.IDSESSIONUPD));


            QueryParameters ret = new QueryParameters(_cs, sqlUpdate.ToString(), dataparameters);
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public QueryParameters GetQueryUpdateAckStatus()
        {

            StrBuilder sqlUpdate = new StrBuilder();
            sqlUpdate += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.TRACKER_L.ToString() + Cst.CrLf;
            sqlUpdate += SQLCst.SET + "ACKSTATUS=@ACKSTATUS, ACKXML=@ACKXML" + Cst.CrLf;
            sqlUpdate += SQLCst.WHERE + "IDTRK_L = @IDTRK_L";

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(_cs, "IDTRK_L", DbType.Int32));
            dataParameters.Add(new DataParameter(_cs, "ACKSTATUS", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN));
            dataParameters.Add(new DataParameter(_cs, "ACKXML", DbType.Xml));
            
            QueryParameters ret = new QueryParameters(_cs, sqlUpdate.ToString(), dataParameters);
            
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pStatus"></param>
        /// <param name="pPostedSubMsg"></param>
        /// <returns></returns>
        /// EG 20180525 [23979] IRQ Processing
        /// EG 20190318 Update IRQ Processing
        /// Reecriture allegée de la query
        /// FI 20201030 [25537] Gestion de NONEMSG
        public QueryParameters GetQueryUpdateCounter(ProcessStateTools.StatusEnum pStatus)
        {
            // PM 20121023 : Déplacement du bloc de création des DataParameters avant la construction de la Query
            // afin d'ajouter uniquement les paramètres utilisés dans la Query (dans le "switch").

            /* !!!!!!!!!!!!!!!!!!!!!!!!!!IMPORTANT!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            La requête utilise l'opérateur >= 
            La R&D pense que sa présence était nécessaire dans le cas suivant exprimé à travers un exemple
            soit un traitement de CB en cours. Le service RiskPerfomance est arrêté. Dans ce cas 
            => Le traitement termine en erreur et un message est reposté dans la queue pour (re)traitement
            => en fin du 2ème traitement, Spheres génère un accusé de reception 

            Ce comportement présent jusqu'en V12 (12.08369) n'est plus celui des versions ultérieures depuis http://svr-db01:8080/tfs/DefaultCollection/SpheresProject/_workitems/edit/487
            L'opérateur est conservé pour l'instant mais devrait certainement être remplacé par l'opérateur "="
            !!!!!!!!!!!!!!!!!!!!!!!!!!IMPORTANT!!!!!!!!!!!!!!!!!!!!!!!!!!!!! */


            DataParameters Dataparameters = new DataParameters();
            Dataparameters.Add(new DataParameter(_cs, "IDTRK_L", DbType.Int32));
            Dataparameters.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.IDAUPD));
            Dataparameters.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.DTUPDDATETIME2));
            Dataparameters.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.HOSTNAMEUPD));
            Dataparameters.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.IDSESSIONUPD));
            Dataparameters.Add(new DataParameter(_cs, "IRQIDTRK_L", DbType.Int32));
            Dataparameters.Add(new DataParameter(_cs, "POSTEDSUBMSG", DbType.Int32));

            StrBuilder sqlUpdate = new StrBuilder();
            sqlUpdate += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.TRACKER_L.ToString() + Cst.CrLf;
            sqlUpdate += SQLCst.SET + Cst.CrLf;
            sqlUpdate += "POSTEDSUBMSG = POSTEDSUBMSG + @POSTEDSUBMSG," + Cst.CrLf;

            switch (pStatus)
            {
                case ProcessStateTools.StatusEnum.NONE:
                    sqlUpdate += @"
NONEMSG = NONEMSG + 1,
STATUSTRACKER = 
case when IRQ = 1 then STATUSTRACKER 
else
    case when ((NONEMSG + SUCCESSMSG + WARNINGMSG + ERRORMSG + 1) >= (POSTEDMSG + POSTEDSUBMSG + @POSTEDSUBMSG)) then 
        case when  (ERRORMSG > 0) then 'ERROR'
             when  (WARNINGMSG > 0) then 'WARNING' 
             when  (SUCCESSMSG > 0) then 'SUCCESS' 
        else 
            'NONE' 
        end
    else 
        STATUSTRACKER 
    end 
end, " + Cst.CrLf;
                    break;

                case ProcessStateTools.StatusEnum.SUCCESS:
                    sqlUpdate += @"
SUCCESSMSG = SUCCESSMSG + 1,
STATUSTRACKER = 
case when IRQ = 1 then STATUSTRACKER 
else
    case when ((NONEMSG + SUCCESSMSG + WARNINGMSG + ERRORMSG + 1) >= (POSTEDMSG + POSTEDSUBMSG + @POSTEDSUBMSG)) then 
        case when  (ERRORMSG > 0) then 'ERROR'
             when  (WARNINGMSG > 0) then 'WARNING' 
             when  (SUCCESSMSG > 0) then 'SUCCESS' 
        else 
            'SUCCESS' 
        end
    else 
        STATUSTRACKER 
    end 
end, " + Cst.CrLf;
                    break;
                case ProcessStateTools.StatusEnum.WARNING:
                    sqlUpdate += @"
WARNINGMSG = WARNINGMSG + 1,
STATUSTRACKER = 
case when IRQ = 1 then STATUSTRACKER 
else
    case when ((NONEMSG + SUCCESSMSG + WARNINGMSG + ERRORMSG + 1) >= (POSTEDMSG + POSTEDSUBMSG + @POSTEDSUBMSG)) then 
        case when  (ERRORMSG > 0) then 'ERROR'
        else 
            'WARNING'
        end 
    else 
        STATUSTRACKER 
    end 
end, " + Cst.CrLf;
                    break;
                case ProcessStateTools.StatusEnum.ERROR:
                    sqlUpdate += @"
ERRORMSG = ERRORMSG + 1,
STATUSTRACKER = 
case when IRQ = 1 then STATUSTRACKER 
else
    case when ((NONEMSG + SUCCESSMSG + WARNINGMSG + ERRORMSG + 1) >= (POSTEDMSG + POSTEDSUBMSG + @POSTEDSUBMSG)) then 
        'ERROR'
    else 
        STATUSTRACKER 
    end 
end," + Cst.CrLf;
                    break;
                case ProcessStateTools.StatusEnum.IRQ:
                    sqlUpdate += @"
WARNINGMSG = WARNINGMSG + 1, 
STATUSTRACKER = 'IRQ', IRQ = 1, " + Cst.CrLf;
                    break;
            }
            sqlUpdate += @"READYSTATE = case when  (NONEMSG + SUCCESSMSG + WARNINGMSG + ERRORMSG + 1 >= (POSTEDMSG + POSTEDSUBMSG + @POSTEDSUBMSG)) then 'TERMINATED'
                                             when  (NONEMSG + SUCCESSMSG + WARNINGMSG + ERRORMSG + 1 > 0) then 'ACTIVE' 
                                        else READYSTATE 
                                        end, 
                           IDAUPD=@IDAUPD, DTUPD=@DTUPD, IDSESSIONUPD=@IDSESSIONUPD, HOSTNAMEUPD=@HOSTNAMEUPD, 
                           IRQIDTRK_L=@IRQIDTRK_L 
            where IDTRK_L = @IDTRK_L" + Cst.CrLf;

            QueryParameters ret = new QueryParameters(_cs, sqlUpdate.ToString(), Dataparameters);
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public QueryParameters GetQueryUpdateIdData()
        {

            StrBuilder sqlUpdate = new StrBuilder();
            sqlUpdate += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.TRACKER_L.ToString() + Cst.CrLf;
            sqlUpdate += SQLCst.SET + "IDDATA=@IDDATA, IDDATAIDENT=@IDDATAIDENT, IDDATAIDENTIFIER=@IDDATAIDENTIFIER" + Cst.CrLf;
            sqlUpdate += SQLCst.WHERE + "IDTRK_L = @IDTRK_L";

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(_cs, "IDTRK_L", DbType.Int32));
            dataParameters.Add(new DataParameter(_cs, "IDDATA", DbType.Int32));
            dataParameters.Add(new DataParameter(_cs, "IDDATAIDENT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
            dataParameters.Add(new DataParameter(_cs, "IDDATAIDENTIFIER", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN));

            QueryParameters ret = new QueryParameters(_cs, sqlUpdate.ToString(), dataParameters);
            return ret;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public QueryParameters GetQueryAddPostedSubMsg()
        {
            DataParameters dataparameters = new DataParameters();
            dataparameters.Add(new DataParameter(_cs, "IDTRK_L", DbType.Int32));
            dataparameters.Add(new DataParameter(_cs, "POSTEDSUBMSG", DbType.Int32));
            dataparameters.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.IDAUPD));
            dataparameters.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.DTUPDDATETIME2));
            dataparameters.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.HOSTNAMEUPD));
            dataparameters.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.IDSESSIONUPD));

            StrBuilder sqlUpdate = new StrBuilder();
            sqlUpdate += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.TRACKER_L.ToString() + Cst.CrLf;
            sqlUpdate += SQLCst.SET + "POSTEDSUBMSG = POSTEDSUBMSG + @POSTEDSUBMSG," + Cst.CrLf;
            sqlUpdate += @"IDAUPD=@IDAUPD, DTUPD=@DTUPD, IDSESSIONUPD=@IDSESSIONUPD, HOSTNAMEUPD=@HOSTNAMEUPD" + Cst.CrLf;
            sqlUpdate += SQLCst.WHERE + "IDTRK_L = @IDTRK_L" + Cst.CrLf;

            QueryParameters ret = new QueryParameters(_cs, sqlUpdate.ToString(), dataparameters);
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public QueryParameters GetQueryUpdatePostedMsg()
        {
            DataParameters dataparameters = new DataParameters();
            dataparameters.Add(new DataParameter(_cs, "IDTRK_L", DbType.Int32));
            dataparameters.Add(new DataParameter(_cs, "POSTEDMSG", DbType.Int32));
            dataparameters.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.IDAUPD));
            dataparameters.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.DTUPDDATETIME2));
            dataparameters.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.HOSTNAMEUPD));
            dataparameters.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.IDSESSIONUPD));

            StrBuilder sqlUpdate = new StrBuilder();
            sqlUpdate += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.TRACKER_L.ToString() + Cst.CrLf;
            sqlUpdate += SQLCst.SET + "POSTEDMSG =  @POSTEDMSG," + Cst.CrLf;
            sqlUpdate += @"IDAUPD=@IDAUPD, DTUPD=@DTUPD, IDSESSIONUPD=@IDSESSIONUPD, HOSTNAMEUPD=@HOSTNAMEUPD" + Cst.CrLf;
            sqlUpdate += SQLCst.WHERE + "IDTRK_L = @IDTRK_L" + Cst.CrLf;

            QueryParameters ret = new QueryParameters(_cs, sqlUpdate.ToString(), dataparameters);
            return ret;
        }

        #endregion Methods
    }
}
