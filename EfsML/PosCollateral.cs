using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;


using EFS.ACommon;
using EFS.Actor;
using EFS.Common;
using EFS.Common.MQueue;


using EFS.ApplicationBlocks.Data;


using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;

namespace EfsML.Business
{
    /// <summary>
    /// DataSet POSCOLLATERAL
    /// </summary>
    public class DatasetPosCollateral
    {
        #region Members
        private DataSet _ds;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient la table POSCOLLATERAL
        /// </summary>
        public DataTable DtPOSCOLLATERAL
        {
            get { return _ds.Tables["POSCOLLATERAL"]; }
        }
        /// <summary>
        /// Obtient la table POSCOLLATERALVAL
        /// </summary>
        public DataTable DtPOSCOLLATERALVAL
        {
            get { return _ds.Tables["POSCOLLATERALVAL"]; }
        }
        /// <summary>
        /// 
        /// </summary>
        public DataRelation ChildPOSCOLLATERAL_POSCOLLATERALVAL
        {
            get { return _ds.Relations["POSCOLLATERAL_POSCOLLATERALVAL"]; }
        }
        #endregion Accessors

        #region Constructors
        public DatasetPosCollateral()
        {

        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Charge le Dataset pour un IDPOSCOLLATERAL 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pId"></param>
        // EG 20180205 [23769] Upd DataHelper.ExecuteDataSet
        public void LoadDs(string pCS, IDbTransaction pDbTransaction, int pId)
        {
            DataParameters sqlParam = new DataParameters();
            sqlParam.Add(new DataParameter(pCS, "ID", DbType.Int32), pId);
            //
            SQLWhere sqlWhere = new SQLWhere("(poscol.IDPOSCOLLATERAL = @ID)");
            //
            StrBuilder sqlSelectPoscol = new StrBuilder(GetSelectPOSCOLLATERALColumn());
            sqlSelectPoscol += sqlWhere;
            //
            StrBuilder sqlSelectPosColVal = new StrBuilder(GetSelectPOSCOLLATERALVALColumn());
            sqlSelectPosColVal += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.POSCOLLATERAL + " poscol on poscol.IDPOSCOLLATERAL=poscolval.IDPOSCOLLATERAL";
            sqlSelectPosColVal += sqlWhere;
            //
            string sqlSelect = sqlSelectPoscol.ToString() + SQLCst.SEPARATOR_MULTISELECT;
            sqlSelect += sqlSelectPosColVal.ToString();
            //
            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, sqlParam);
            _ds = DataHelper.ExecuteDataset(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            InitializeDs();
        }

        /// <summary>
        /// Retourne le Row ds POSCOLLATERALVAL tel DTBUSINESS = {pDTBUSINESS}
        /// <para>Retourne null si n'existe pas </para>
        /// </summary>
        /// <param name="pDtBusiness"></param>
        /// <returns></returns>
        public DataRow GetPosCollateralValRow(DateTime pDtBusiness)
        {
            DataRow ret = null;
            string dateIso = DtFunc.DateTimeToStringDateISO(pDtBusiness);
            //
            DataRow[] row = DtPOSCOLLATERALVAL.Select(StrFunc.AppendFormat("DTBUSINESS = '{0}'", dateIso));
            if (ArrFunc.IsFilled(row))
                ret = row[0];
            //
            return ret;
        }

        /// <summary>
        /// Mise à jour de la table POSCOLLATERAL
        /// </summary>
        /// <param name="pDbTransaction"></param>
        // EG 20180205 [23769] Upd DataHelper.ExecuteDataAdapter
        public void ExecuteDataAdapterPOSCOLLATERAL(string pCS, IDbTransaction pDbTransaction)
        {
            string sqlSelect = GetSelectPOSCOLLATERALColumn();
            DataHelper.ExecuteDataAdapter(pCS, pDbTransaction, sqlSelect, DtPOSCOLLATERAL);
        }
        /// <summary>
        /// Mise à jour de la table POSCOLLATERALVAL
        /// </summary>
        /// <param name="pDbTransaction"></param>
        // EG 20180205 [23769] Upd DataHelper.ExecuteDataAdapter
        public void ExecuteDataAdapterPOSCOLLATERALVAL(string pCS, IDbTransaction pDbTransaction)
        {
            string sqlSelect = GetSelectPOSCOLLATERALVALColumn();
            DataHelper.ExecuteDataAdapter(pCS, pDbTransaction, sqlSelect, DtPOSCOLLATERALVAL);
        }

        /// <summary>
        /// Retourne le select avec les colonnes de POSCOLLATERAL 
        /// </summary>
        /// <returns></returns>
        private string GetSelectPOSCOLLATERALColumn()
        {
            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
            sqlSelect += "poscol.IDPOSCOLLATERAL," + Cst.CrLf;
            sqlSelect += "poscol.IDA_PAY,poscol.IDB_PAY," + Cst.CrLf;
            sqlSelect += "poscol.IDA_REC,poscol.IDB_REC," + Cst.CrLf;
            sqlSelect += "poscol.IDA_CSS," + Cst.CrLf;
            sqlSelect += "poscol.DTBUSINESS, poscol.DTTERMINATION,poscol.DURATION," + Cst.CrLf;
            sqlSelect += "poscol.IDASSET,poscol.ASSETCATEGORY," + Cst.CrLf;
            sqlSelect += "poscol.QTY," + Cst.CrLf;
            sqlSelect += "poscol.HAIRCUT,poscol.HAIRCUTFORCED," + Cst.CrLf;
            sqlSelect += "poscol.DTUPD,poscol.IDAUPD,poscol.DTINS,poscol.IDAINS," + Cst.CrLf;
            sqlSelect += "poscol.EXTLLINK,poscol.ROWATTRIBUT" + Cst.CrLf;
            //
            string tbl = Cst.OTCml_TBL.POSCOLLATERAL.ToString();
            sqlSelect += SQLCst.FROM_DBO + tbl + " poscol " + Cst.CrLf;
            //				
            return sqlSelect.ToString();
        }
        /// <summary>
        /// Retourne le select avec les colonnes de POSCOLLATERALVAL 
        /// </summary>
        private string GetSelectPOSCOLLATERALVALColumn()
        {

            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
            sqlSelect += "poscolval.IDPOSCOLLATERALVAL,poscolval.IDPOSCOLLATERAL," + Cst.CrLf;
            sqlSelect += "poscolval.DTBUSINESS," + Cst.CrLf;
            sqlSelect += "poscolval.QTY," + Cst.CrLf;
            sqlSelect += "poscolval.VALORISATION,poscolval.IDC," + Cst.CrLf;
            sqlSelect += "poscolval.VALORISATIONSYS,poscolval.IDCSYS," + Cst.CrLf;
            sqlSelect += "poscolval.IDSTACTIVATION," + Cst.CrLf;
            sqlSelect += "poscolval.SOURCE," + Cst.CrLf;

            sqlSelect += "poscolval.DTUPD,poscolval.IDAUPD,poscolval.DTINS,poscolval.IDAINS," + Cst.CrLf;
            sqlSelect += "poscolval.EXTLLINK,poscolval.ROWATTRIBUT" + Cst.CrLf;
            //
            string tbl = Cst.OTCml_TBL.POSCOLLATERALVAL.ToString();
            sqlSelect += SQLCst.FROM_DBO + tbl + " poscolval " + Cst.CrLf;
            //				
            return sqlSelect.ToString();

        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeDs()
        {
            string columnID = OTCmlHelper.GetColunmID(Cst.OTCml_TBL.POSCOLLATERAL.ToString());

            DataTable dt = _ds.Tables[0];
            dt.TableName = Cst.OTCml_TBL.POSCOLLATERAL.ToString();
            dt = _ds.Tables[1];
            dt.TableName = Cst.OTCml_TBL.POSCOLLATERALVAL.ToString();

            //Relations
            DataRelation rel_poscol_poscolval =
                new DataRelation("POSCOLLATERAL_POSCOLLATERALVAL", DtPOSCOLLATERAL.Columns[columnID], DtPOSCOLLATERALVAL.Columns[columnID], false);
            _ds.Relations.Add(rel_poscol_poscolval);
        }

        #endregion Methods
    }

    /// <summary>
    /// 
    /// </summary>
    public class PosCollateralKey
    {
        /// <summary>
        /// Représente le couple Acteur/book associé au payer
        /// </summary>
        public Pair<int, int> Payer
        {
            get;
            set;
        }
        /// <summary>
        /// Représente le couple Acteur/book associé au receiver
        /// </summary>
        Pair<int, int> Receiver
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public DateTime DtBusiness
        {
            get;
            set;
        }
        /// <summary>
        /// Représente le couple Cst.UnderlyingAsset/asset associé au titre
        /// </summary>
        public Pair<Cst.UnderlyingAsset, int> Asset
        {
            get;
            set;
        }

        #region Methods
        /// <summary>
        /// Retourne l'Id POSCOLLATERAL correspondant
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        /// FI 20130425 [18598] modification de la signature GetIdPosCollateral retourne un array 
        public int[] GetIdPosCollateral(string pCS)
        {
            return PosCollateralRDBMSTools.GetId(pCS, Payer, Receiver, DtBusiness, Asset);
        }

        #endregion

    }

}
