using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.IO;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using FpML.Enum;
//
using FpML.Interface;
//
namespace EFS.SpheresIO
{
    #region Enum
    public enum ActionType
    {
        Insert,
        Amend,
        Cancel,
    }
    public enum CompareSource
    {
        Internal,
        External,
    }
    #endregion

    #region DataContract
    /// <summary>
    /// Class representing audit table
    /// </summary>
    [DataContract(
        Name = DataHelper<SNegoAudit>.DATASETROWNAME,
        Namespace = DataHelper<SNegoAudit>.DATASETNAMESPACE)]
    [XmlRoot(ElementName = "Audit")]
    public class SNegoAudit
    {
        int m_IdS_Nego_Audit;
        [DataMember(Name = "IDS_NEGO_AUDIT", Order = 1)]
        public int IdS_Nego_Audit
        {
            get { return m_IdS_Nego_Audit; }
            set { m_IdS_Nego_Audit = value; }
        }

        int m_Version;
        [DataMember(Name = "VERSION", Order = 2)]
        public int Version
        {
            get { return m_Version; }
            set { m_Version = value; }
        }

        string m_ActionType;
        [DataMember(Name = "ACTIONTYPE", Order = 3)]
        public string ActionType
        {
            get { return m_ActionType; }
            set { m_ActionType = value; }
        }

        int m_IdProcessL;
        [DataMember(Name = "IDPROCESS_L", Order = 4)]
        public int IdProcessL
        {
            get { return m_IdProcessL; }
            set { m_IdProcessL = value; }
        }

        DateTime m_DtProcessL;
        [DataMember(Name = "DTPROCESS_L ", Order = 5)]
        public DateTime DtProcessL
        {
            get { return m_DtProcessL; }
            set { m_DtProcessL = value; }
        }

        string m_Eurosys_User;
        [DataMember(Name = "EUROSYS_USER", Order = 6)]
        public string Eurosys_User
        {
            get { return m_Eurosys_User; }
            set { m_Eurosys_User = value; }
        }

        string m_HostName;
        [DataMember(Name = "HOSTNAME", Order = 7)]
        public string HostName
        {
            get { return m_HostName; }
            set { m_HostName = value; }
        }

        DateTime m_Age_Enreg;
        [DataMember(Name = "AGE_ENREG", Order = 8)]
        public DateTime Age_Enreg
        {
            get { return m_Age_Enreg; }
            set { m_Age_Enreg = value; }
        }

        double m_Cours_Clop;
        [DataMember(Name = "COURS_CLOP", Order = 9)]
        public double Cours_Clop
        {
            get { return m_Cours_Clop; }
            set { m_Cours_Clop = value; }
        }

        double m_Cours_Op;
        [DataMember(Name = "COURS_OP", Order = 10)]
        public double Cours_Op
        {
            get { return m_Cours_Op; }
            set { m_Cours_Op = value; }
        }

        double m_Cours_Op_Brk;
        [DataMember(Name = "COURS_OP_BRK", Order = 11)]
        public double Cours_Op_Brk
        {
            get { return m_Cours_Op_Brk; }
            set { m_Cours_Op_Brk = value; }
        }

        string m_Cpt_Inter;
        [DataMember(Name = "CPT_INTER", Order = 12)]
        public string Cpt_Inter
        {
            get { return m_Cpt_Inter; }
            set { m_Cpt_Inter = value; }
        }

        DateTime m_Date_Clop;
        [DataMember(Name = "DATE_CLOP", Order = 13)]
        public DateTime Date_Clop
        {
            get { return m_Date_Clop; }
            set { m_Date_Clop = value; }
        }

        DateTime m_Date_Nego;
        [DataMember(Name = "DATE_NEGO", Order = 14)]
        public DateTime Date_Nego
        {
            get { return m_Date_Nego; }
            set { m_Date_Nego = value; }
        }

        DateTime m_Date_Trait;
        [DataMember(Name = "DATE_TRAIT", Order = 15)]
        public DateTime Date_Trait
        {
            get { return m_Date_Trait; }
            set { m_Date_Trait = value; }
        }

        string m_Grp_Com;
        [DataMember(Name = "GRP_COM", Order = 16)]
        public string Grp_Com
        {
            get { return m_Grp_Com; }
            set { m_Grp_Com = value; }
        }

        string m_Grp_Court;
        [DataMember(Name = "GRP_COURT", Order = 17)]
        public string Grp_Court
        {
            get { return m_Grp_Court; }
            set { m_Grp_Court = value; }
        }

        string m_Grp_Court_Brk;
        [DataMember(Name = "GRP_COURT_BRK", Order = 18)]
        public string Grp_Court_Brk
        {
            get { return m_Grp_Court_Brk; }
            set { m_Grp_Court_Brk = value; }
        }

        string m_Grp_Court_Int;
        [DataMember(Name = "GRP_COURT_INT", Order = 19)]
        public string Grp_Court_Int
        {
            get { return m_Grp_Court_Int; }
            set { m_Grp_Court_Int = value; }
        }

        string m_Grp_Cpte;
        [DataMember(Name = "GRP_CPTE", Order = 20)]
        public string Grp_Cpte
        {
            get { return m_Grp_Cpte; }
            set { m_Grp_Cpte = value; }
        }

        DateTime m_Hre_Nego;
        [DataMember(Name = "HRE_NEGO", Order = 21)]
        public DateTime Hre_Nego
        {
            get { return m_Hre_Nego; }
            set { m_Hre_Nego = value; }
        }

        string m_Id_Erreur;
        [DataMember(Name = "ID_ERREUR", Order = 22)]
        public string Id_Erreur
        {
            get { return m_Id_Erreur; }
            set { m_Id_Erreur = value; }
        }

        string m_Id_Gn_Op;
        [DataMember(Name = "ID_GN_OP", Order = 23)]
        public string Id_Gn_Op
        {
            get { return m_Id_Gn_Op; }
            set { m_Id_Gn_Op = value; }
        }

        string m_Id_Gn_Op_Brk;
        [DataMember(Name = "ID_GN_OP_BRK", Order = 24)]
        public string Id_Gn_Op_Brk
        {
            get { return m_Id_Gn_Op_Brk; }
            set { m_Id_Gn_Op_Brk = value; }
        }

        string m_Id_Gn_Op_Grp_Cpte;
        [DataMember(Name = "ID_GN_OP_GRP_CPTE", Order = 25)]
        public string Id_Gn_Op_Grp_Cpte
        {
            get { return m_Id_Gn_Op_Grp_Cpte; }
            set { m_Id_Gn_Op_Grp_Cpte = value; }
        }

        string m_Id_Gn_Op_Int;
        [DataMember(Name = "ID_GN_OP_INT", Order = 26)]
        public string Id_Gn_Op_Int
        {
            get { return m_Id_Gn_Op_Int; }
            set { m_Id_Gn_Op_Int = value; }
        }

        string m_Instrt;
        [DataMember(Name = "INSTRT", Order = 27)]
        public string Instrt
        {
            get { return m_Instrt; }
            set { m_Instrt = value; }
        }

        string m_Marche;
        [DataMember(Name = "MARCHE", Order = 28)]
        public string Marche
        {
            get { return m_Marche; }
            set { m_Marche = value; }
        }

        string m_Meth_Cal_Com;
        [DataMember(Name = "METH_CAL_COM", Order = 29)]
        public string Meth_Cal_Com
        {
            get { return m_Meth_Cal_Com; }
            set { m_Meth_Cal_Com = value; }
        }

        string m_Meth_Cal_Crt;
        [DataMember(Name = "METH_CAL_CRT", Order = 30)]
        public string Meth_Cal_Crt
        {
            get { return m_Meth_Cal_Crt; }
            set { m_Meth_Cal_Crt = value; }
        }

        double m_Mont_Com_Globex;
        [DataMember(Name = "MONT_COM_GLOBEX", Order = 31)]
        public double Mont_Com_Globex
        {
            get { return m_Mont_Com_Globex; }
            set { m_Mont_Com_Globex = value; }
        }

        decimal m_Mont_Com_Lot;
        [DataMember(Name = "MONT_COM_LOT", Order = 32)]
        public decimal Mont_Com_Lot
        {
            get { return m_Mont_Com_Lot; }
            set { m_Mont_Com_Lot = value; }
        }

        decimal m_Mont_Com_Lot_Brk;
        [DataMember(Name = "MONT_COM_LOT_BRK", Order = 33)]
        public decimal Mont_Com_Lot_Brk
        {
            get { return m_Mont_Com_Lot_Brk; }
            set { m_Mont_Com_Lot_Brk = value; }
        }

        decimal m_Mont_Com_Lot_Grp;
        [DataMember(Name = "MONT_COM_LOT_GRP", Order = 34)]
        public decimal Mont_Com_Lot_Grp
        {
            get { return m_Mont_Com_Lot_Grp; }
            set { m_Mont_Com_Lot_Grp = value; }
        }

        double m_Mont_Court_Lot;
        [DataMember(Name = "MONT_COURT_LOT", Order = 35)]
        public double Mont_Court_Lot
        {
            get { return m_Mont_Court_Lot; }
            set { m_Mont_Court_Lot = value; }
        }

        double m_Mont_Court_Lot_Grp;
        [DataMember(Name = "MONT_COURT_LOT_GRP", Order = 36)]
        public double Mont_Court_Lot_Grp
        {
            get { return m_Mont_Court_Lot_Grp; }
            set { m_Mont_Court_Lot_Grp = value; }
        }

        double m_Mont_Court_Lot_Brk;
        [DataMember(Name = "MONT_COURT_LOT_BRK", Order = 37)]
        public double Mont_Court_Lot_Brk
        {
            get { return m_Mont_Court_Lot_Brk; }
            set { m_Mont_Court_Lot_Brk = value; }
        }

        double m_Mont_Court_Lot_Int;
        [DataMember(Name = "MONT_COURT_LOT_INT", Order = 38)]
        public double Mont_Court_Lot_Int
        {
            get { return m_Mont_Court_Lot_Int; }
            set { m_Mont_Court_Lot_Int = value; }
        }

        string m_Nom_Eche;
        [DataMember(Name = "NOM_ECHE", Order = 39)]
        public string Nom_Eche
        {
            get { return m_Nom_Eche; }
            set { m_Nom_Eche = value; }
        }

        string m_Num_Broker;
        [DataMember(Name = "NUM_BROKER", Order = 40)]
        public string Num_Broker
        {
            get { return m_Num_Broker; }
            set { m_Num_Broker = value; }
        }

        string m_Num_Compte;
        [DataMember(Name = "NUM_COMPTE", Order = 41)]
        public string Num_Compte
        {
            get { return m_Num_Compte; }
            set { m_Num_Compte = value; }
        }

        string m_Num_Ordre;
        [DataMember(Name = "NUM_ORDRE", Order = 42)]
        public string Num_Ordre
        {
            get { return m_Num_Ordre; }
            set { m_Num_Ordre = value; }
        }

        string m_Observation1;
        [DataMember(Name = "OBSERVATION1", Order = 43)]
        public string Observation1
        {
            get { return m_Observation1; }
            set { m_Observation1 = value; }
        }

        string m_Produit;
        [DataMember(Name = "PRODUIT", Order = 44)]
        public string Produit
        {
            get { return m_Produit; }
            set { m_Produit = value; }
        }

        int m_Quant_Op;
        [DataMember(Name = "QUANT_OP", Order = 45)]
        public int Quant_Op
        {
            get { return m_Quant_Op; }
            set { m_Quant_Op = value; }
        }

        string m_Refer_Clop;
        [DataMember(Name = "REFER_CLOP", Order = 46)]
        public string Refer_Clop
        {
            get { return m_Refer_Clop; }
            set { m_Refer_Clop = value; }
        }

        string m_Refer_Op;
        [DataMember(Name = "REFER_OP", Order = 47)]
        public string Refer_Op
        {
            get { return m_Refer_Op; }
            set { m_Refer_Op = value; }
        }

        string m_Sens_Op;
        [DataMember(Name = "SENS_OP", Order = 48)]
        public string Sens_Op
        {
            get { return m_Sens_Op; }
            set { m_Sens_Op = value; }
        }

        double m_Strk_Op;
        [DataMember(Name = "STRK_OP", Order = 49)]
        public double Strk_Op
        {
            get { return m_Strk_Op; }
            set { m_Strk_Op = value; }
        }

        string m_Time_Enreg;
        [DataMember(Name = "TIME_ENREG", Order = 50)]
        public string Time_Enreg
        {
            get { return m_Time_Enreg; }
            set { m_Time_Enreg = value; }
        }

        string m_Typ_Compte;
        [DataMember(Name = "TYP_COMPTE", Order = 51)]
        public string Typ_Compte
        {
            get { return m_Typ_Compte; }
            set { m_Typ_Compte = value; }
        }

        string m_Typ_Mar_Op;
        [DataMember(Name = "TYP_MAR_OP", Order = 52)]
        public string Typ_Mar_Op
        {
            get { return m_Typ_Mar_Op; }
            set { m_Typ_Mar_Op = value; }
        }

        string m_Typ_Marche;
        [DataMember(Name = "TYP_MARCHE", Order = 53)]
        public string Typ_Marche
        {
            get { return m_Typ_Marche; }
            set { m_Typ_Marche = value; }
        }

        string m_Typ_Op;
        [DataMember(Name = "TYP_OP", Order = 54)]
        public string Typ_Op
        {
            get { return m_Typ_Op; }
            set { m_Typ_Op = value; }
        }

        string m_Typ_Op_Brk;
        [DataMember(Name = "TYP_OP_BRK", Order = 55)]
        public string Typ_Op_Brk
        {
            get { return m_Typ_Op_Brk; }
            set { m_Typ_Op_Brk = value; }
        }

        string m_Typ_X_Depouille;
        [DataMember(Name = "TYP_X_DEPOUILLE", Order = 56)]
        public string Typ_X_Depouille
        {
            get { return m_Typ_X_Depouille; }
            set { m_Typ_X_Depouille = value; }
        }

        string m_Typ_Nego;
        [DataMember(Name = "TYP_NEGO", Order = 57)]
        public string Typ_Nego
        {
            get { return m_Typ_Nego; }
            set { m_Typ_Nego = value; }
        }

        string m_Nom_User;
        [DataMember(Name = "NOM_USER", Order = 58)]
        public string Nom_User
        {
            get { return m_Nom_User; }
            set { m_Nom_User = value; }
        }

        double m_Mont_Court_Lg;
        [DataMember(Name = "MONT_COURT_LG", Order = 59)]
        public double Mont_Court_Lg
        {
            get { return m_Mont_Court_Lg; }
            set { m_Mont_Court_Lg = value; }
        }

        double m_Mont_Court_Lg_Grp;
        [DataMember(Name = "MONT_COURT_LG_GRP", Order = 60)]
        public double Mont_Court_Lg_Grp
        {
            get { return m_Mont_Court_Lg_Grp; }
            set { m_Mont_Court_Lg_Grp = value; }
        }

        double m_Mont_Court_Lg_Brk;
        [DataMember(Name = "MONT_COURT_LG_BRK", Order = 61)]
        public double Mont_Court_Lg_Brk
        {
            get { return m_Mont_Court_Lg_Brk; }
            set { m_Mont_Court_Lg_Brk = value; }
        }

        double m_Mont_Court_Lg_Int;
        [DataMember(Name = "MONT_COURT_LG_INT", Order = 62)]
        public double Mont_Court_Lg_Int
        {
            get { return m_Mont_Court_Lg_Int; }
            set { m_Mont_Court_Lg_Int = value; }
        }

        string m_Id_Valid;
        [DataMember(Name = "ID_VALID", Order = 63)]
        public string Id_Valid
        {
            get { return m_Id_Valid; }
            set { m_Id_Valid = value; }
        }

        double m_Mont_Court_Lg_Grp_Neg;
        [DataMember(Name = "MONT_COURT_LOT_GRP_NEG", Order = 64)]
        public double Mont_Court_Lg_Grp_Neg
        {
            get { return m_Mont_Court_Lg_Grp_Neg; }
            set { m_Mont_Court_Lg_Grp_Neg = value; }
        }

        double m_Mont_Court_Lot_Com;
        [DataMember(Name = "MONT_COURT_LOT_COM", Order = 65)]
        public double Mont_Court_Lot_Com
        {
            get { return m_Mont_Court_Lot_Com; }
            set { m_Mont_Court_Lot_Com = value; }
        }

        double m_Mont_Court_Lot_Neg;
        [DataMember(Name = "MONT_COURT_LOT_NEG", Order = 66)]
        public double Mont_Court_Lot_Neg
        {
            get { return m_Mont_Court_Lot_Neg; }
            set { m_Mont_Court_Lot_Neg = value; }
        }

        decimal m_Mont_Com_Lot_Com;
        [DataMember(Name = "MONT_COM_LOT_COM", Order = 67)]
        public decimal Mont_Com_Lot_Com
        {
            get { return m_Mont_Com_Lot_Com; }
            set { m_Mont_Com_Lot_Com = value; }
        }

        decimal m_Mont_Com_Lot_Grp_Com;
        [DataMember(Name = "MONT_COM_LOT_GRP_COM", Order = 68)]
        public decimal Mont_Com_Lot_Grp_Com
        {
            get { return m_Mont_Com_Lot_Grp_Com; }
            set { m_Mont_Com_Lot_Grp_Com = value; }
        }

        decimal m_Mont_Com_Lot_Neg;
        [DataMember(Name = "MONT_COM_LOT_NEG", Order = 69)]
        public decimal Mont_Com_Lot_Neg
        {
            get { return m_Mont_Com_Lot_Neg; }
            set { m_Mont_Com_Lot_Neg = value; }
        }

        decimal m_Mont_Com_Lot_Grp_Neg;
        [DataMember(Name = "MONT_COM_LOT_GRP_NEG", Order = 70)]
        public decimal Mont_Com_Lot_Grp_Neg
        {
            get { return m_Mont_Com_Lot_Grp_Neg; }
            set { m_Mont_Com_Lot_Grp_Neg = value; }
        }

        double m_Mont_Court_Lot_Grp_Com;
        [DataMember(Name = "MONT_COURT_LOT_GRP_COM", Order = 71)]
        public double Mont_Court_Lot_Grp_Com
        {
            get { return m_Mont_Court_Lot_Grp_Com; }
            set { m_Mont_Court_Lot_Grp_Com = value; }
        }

        string m_Meth_Cal_Com_Grp;
        [DataMember(Name = "METH_CAL_COM_GRP", Order = 72)]
        public string Meth_Cal_Com_Grp
        {
            get { return m_Meth_Cal_Com_Grp; }
            set { m_Meth_Cal_Com_Grp = value; }
        }

        string m_Meth_Cal_Crt_Grp;
        [DataMember(Name = "METH_CAL_CRT_GRP", Order = 73)]
        public string Meth_Cal_Crt_Grp
        {
            get { return m_Meth_Cal_Crt_Grp; }
            set { m_Meth_Cal_Crt_Grp = value; }
        }

        string m_User_Data;
        [DataMember(Name = "USER_DATA", Order = 74)]
        public string User_Data
        {
            get { return m_User_Data; }
            set { m_User_Data = value; }
        }

        string m_Meth_Cal_Crt_Brk;
        [DataMember(Name = "METH_CAL_CRT_BRK", Order = 75)]
        public string Meth_Cal_Crt_Brk
        {
            get { return m_Meth_Cal_Crt_Brk; }
            set { m_Meth_Cal_Crt_Brk = value; }
        }

        string m_Meth_Cal_Com_Brk;
        [DataMember(Name = "METH_CAL_COM_BRK", Order = 76)]
        public string Meth_Cal_Com_Brk
        {
            get { return m_Meth_Cal_Com_Brk; }
            set { m_Meth_Cal_Com_Brk = value; }
        }

        string m_Meth_Cal_Crt_Int;
        [DataMember(Name = "METH_CAL_CRT_INT", Order = 77)]
        public string Meth_Cal_Crt_Int
        {
            get { return m_Meth_Cal_Crt_Int; }
            set { m_Meth_Cal_Crt_Int = value; }
        }

        string m_Id_Matching;
        [DataMember(Name = "ID_MATCHING", Order = 78)]
        public string Id_Matching
        {
            get { return m_Id_Matching; }
            set { m_Id_Matching = value; }
        }

        int m_Split;
        [DataMember(Name = "SPLIT", Order = 79)]
        public int Split
        {
            get { return m_Split; }
            set { m_Split = value; }
        }

        int m_Split_Clop;
        [DataMember(Name = "SPLIT_CLOP", Order = 80)]
        public int Split_Clop
        {
            get { return m_Split_Clop; }
            set { m_Split_Clop = value; }
        }

        string m_Code_Type_Ordre;
        [DataMember(Name = "CODE_TYPE_ORDRE", Order = 81)]
        public string Code_Type_Ordre
        {
            get { return m_Code_Type_Ordre; }
            set { m_Code_Type_Ordre = value; }
        }

        string m_Strike_Modified;
        [DataMember(Name = "STRIKE_MODIFIED", Order = 82)]
        public string Strike_Modified
        {
            get { return m_Strike_Modified; }
            set { m_Strike_Modified = value; }
        }

        DateTime m_DtIns;
        [DataMember(Name = "DTINS", Order = 83)]
        public DateTime DtIns
        {
            get { return m_DtIns; }
            set { m_DtIns = value; }
        }

        int m_IdaIns;
        [DataMember(Name = "IDAINS", Order = 84)]
        public int IdaIns
        {
            get { return m_IdaIns; }
            set { m_IdaIns = value; }
        }

        DateTime m_DtUpd;
        [DataMember(Name = "DTUPD", Order = 85)]
        public DateTime DtUpd
        {
            get { return m_DtUpd; }
            set { m_DtUpd = value; }
        }

        int m_IdaUpd;
        [DataMember(Name = "IDAUPD", Order = 86)]
        public int IdaUpd
        {
            get { return m_IdaUpd; }
            set { m_IdaUpd = value; }
        }
    }

    /// <summary>
    /// Class representing export table
    /// </summary>
    [DataContract(
        Name = DataHelper<ExportBr_O>.DATASETROWNAME,
        Namespace = DataHelper<ExportBr_O>.DATASETNAMESPACE)]
    [XmlRoot(ElementName = "ExportBr_O")]
    public class ExportBr_O
    {
        int m_IdExportBRO;
        [DataMember(Name = "IDEXPORTBR_O", Order = 1)]
        public int IdExportBRO
        {
            get { return m_IdExportBRO; }
            set { m_IdExportBRO = value; }
        }

        int m_Version;
        [DataMember(Name = "VERSION", Order = 2)]
        public int Version
        {
            get { return m_Version; }
            set { m_Version = value; }
        }

        string m_ActionType;
        [DataMember(Name = "ACTIONTYPE", Order = 3)]
        public string ActionType
        {
            get { return m_ActionType; }
            set { m_ActionType = value; }
        }

        int m_IdS_Nego_Audit;
        [DataMember(Name = "IDS_NEGO_AUDIT", Order = 4)]
        public int IdS_Nego_Audit
        {
            get { return m_IdS_Nego_Audit; }
            set { m_IdS_Nego_Audit = value; }
        }

        DateTime m_DtInsS_Nego_Audit;
        [DataMember(Name = "DTINSS_NEGO_AUDIT ", Order = 5)]
        public DateTime DtInsS_Nego_Audit
        {
            get { return m_DtInsS_Nego_Audit; }
            set { m_DtInsS_Nego_Audit = value; }
        }

        int m_IdProcessL;
        [DataMember(Name = "IDPROCESS_L", Order = 6)]
        public int IdProcessL
        {
            get { return m_IdProcessL; }
            set { m_IdProcessL = value; }
        }

        DateTime m_DtProcessL;
        [DataMember(Name = "DTPROCESS_L ", Order = 7)]
        public DateTime DtProcessL
        {
            get { return m_DtProcessL; }
            set { m_DtProcessL = value; }
        }

        DateTime m_DtBusiness;
        [DataMember(Name = "DTBUSINESS", Order = 8)]
        public DateTime DtBusiness
        {
            get { return m_DtBusiness; }
            set { m_DtBusiness = value; }
        }

        string m_EurosysUser;
        [DataMember(Name = "EUROSYS_USER", Order = 9)]
        public string EurosysUser
        {
            get { return m_EurosysUser; }
            set { m_EurosysUser = value; }
        }

        string m_HostName;
        [DataMember(Name = "HOSTNAME", Order = 10)]
        public string HostName
        {
            get { return m_HostName; }
            set { m_HostName = value; }
        }

        DateTime m_Date_Nego;
        [DataMember(Name = "DATE_NEGO", Order = 11)]
        public DateTime Date_Nego
        {
            get { return m_Date_Nego; }
            set { m_Date_Nego = value; }
        }

        string m_Produit;
        [DataMember(Name = "PRODUIT", Order = 12)]
        public string Produit
        {
            get { return m_Produit; }
            set { m_Produit = value; }
        }

        string m_Nom_Eche;
        [DataMember(Name = "NOM_ECHE", Order = 13)]
        public string Nom_Eche
        {
            get { return m_Nom_Eche; }
            set { m_Nom_Eche = value; }
        }
        string m_Sens_Op;
        [DataMember(Name = "SENS_OP", Order = 14)]
        public string Sens_Op
        {
            get { return m_Sens_Op; }
            set { m_Sens_Op = value; }
        }

        string m_Instrt;
        [DataMember(Name = "INSTRT", Order = 15)]
        public string Instrt
        {
            get { return m_Instrt; }
            set { m_Instrt = value; }
        }

        int m_Quant_Op;
        [DataMember(Name = "QUANT_OP", Order = 16)]
        public int Quant_Op
        {
            get { return m_Quant_Op; }
            set { m_Quant_Op = value; }
        }

        double m_Cours_Op;
        [DataMember(Name = "COURS_OP", Order = 17)]
        public double Cours_Op
        {
            get { return m_Cours_Op; }
            set { m_Cours_Op = value; }
        }

        double m_Strk_Op;
        [DataMember(Name = "STRK_OP", Order = 18)]
        public double Strk_Op
        {
            get { return m_Strk_Op; }
            set { m_Strk_Op = value; }
        }

        string m_Refer_Op;
        [DataMember(Name = "REFER_OP", Order = 19)]
        public string Refer_Op
        {
            get { return m_Refer_Op; }
            set { m_Refer_Op = value; }
        }

        string m_Grp_Cpte;
        [DataMember(Name = "GRP_CPTE", Order = 20)]
        public string Grp_Cpte
        {
            get { return m_Grp_Cpte; }
            set { m_Grp_Cpte = value; }
        }

        string m_Num_Compte;
        [DataMember(Name = "NUM_COMPTE", Order = 21)]
        public string Num_Compte
        {
            get { return m_Num_Compte; }
            set { m_Num_Compte = value; }
        }

        string m_Marche;
        [DataMember(Name = "MARCHE", Order = 22)]
        public string Marche
        {
            get { return m_Marche; }
            set { m_Marche = value; }
        }

        string m_User_Data;
        [DataMember(Name = "USER_DATA", Order = 23)]
        public string User_Data
        {
            get { return m_User_Data; }
            set { m_User_Data = value; }
        }

        string m_Domiciliation;
        [DataMember(Name = "DOMICILIATION", Order = 24)]
        public string Domiciliation
        {
            get { return m_Domiciliation; }
            set { m_Domiciliation = value; }
        }

        double m_Mont_Court_Lg;
        [DataMember(Name = "MONT_COURT_LG", Order = 25)]
        public double Mont_Court_Lg
        {
            get { return m_Mont_Court_Lg; }
            set { m_Mont_Court_Lg = value; }
        }

        double m_Mont_Court_Lot;
        [DataMember(Name = "MONT_COURT_LOT", Order = 26)]
        public double Mont_Court_Lot
        {
            get { return m_Mont_Court_Lot; }
            set { m_Mont_Court_Lot = value; }
        }

        decimal m_Tva;
        [DataMember(Name = "TVA", Order = 27)]
        public decimal Tva
        {
            get { return m_Tva; }
            set { m_Tva = value; }
        }

        string m_Dev_Crt_Com;
        [DataMember(Name = "DEV_CRT_COM", Order = 28)]
        public string Dev_Crt_Com
        {
            get { return m_Dev_Crt_Com; }
            set { m_Dev_Crt_Com = value; }
        }

        string m_Ra_Social;
        [DataMember(Name = "RA_SOCIAL", Order = 29)]
        public string Ra_Social
        {
            get { return m_Ra_Social; }
            set { m_Ra_Social = value; }
        }

        decimal m_Tx_Tva_Court;
        [DataMember(Name = "TX_TVA_COURT", Order = 30)]
        public decimal Tx_Tva_Court
        {
            get { return m_Tx_Tva_Court; }
            set { m_Tx_Tva_Court = value; }
        }

        DateTime m_Age_Enreg;
        [DataMember(Name = "AGE_ENREG", Order = 31)]
        public DateTime Age_Enreg
        {
            get { return m_Age_Enreg; }
            set { m_Age_Enreg = value; }
        }

        double m_Cours_Clop;
        [DataMember(Name = "COURS_CLOP", Order = 31)]
        public double Cours_Clop
        {
            get { return m_Cours_Clop; }
            set { m_Cours_Clop = value; }
        }

        string m_Cpt_Inter;
        [DataMember(Name = "CPT_INTER", Order = 33)]
        public string Cpt_Inter
        {
            get { return m_Cpt_Inter; }
            set { m_Cpt_Inter = value; }
        }

        string m_Id_Gn_Op_Int;
        [DataMember(Name = "ID_GN_OP_INT", Order = 34)]
        public string Id_Gn_Op_Int
        {
            get { return m_Id_Gn_Op_Int; }
            set { m_Id_Gn_Op_Int = value; }
        }

        double m_Mont_Court_Lot_Int;
        [DataMember(Name = "MONT_COURT_LOT_INT", Order = 35)]
        public double Mont_Court_Lot_Int
        {
            get { return m_Mont_Court_Lot_Int; }
            set { m_Mont_Court_Lot_Int = value; }
        }

        double m_Mont_Court_Lg_Int;
        [DataMember(Name = "MONT_COURT_LG_INT", Order = 36)]
        public double Mont_Court_Lg_Int
        {
            get { return m_Mont_Court_Lg_Int; }
            set { m_Mont_Court_Lg_Int = value; }
        }

        string m_Typ_X_Depouille;
        [DataMember(Name = "TYP_X_DEPOUILLE", Order = 37)]
        public string Typ_X_Depouille
        {
            get { return m_Typ_X_Depouille; }
            set { m_Typ_X_Depouille = value; }
        }

        string m_Num_Ordre;
        [DataMember(Name = "NUM_ORDRE", Order = 38)]
        public string Num_Ordre
        {
            get { return m_Num_Ordre; }
            set { m_Num_Ordre = value; }
        }

        string m_Typ_Compte;
        [DataMember(Name = "TYP_COMPTE", Order = 39)]
        public string Typ_Compte
        {
            get { return m_Typ_Compte; }
            set { m_Typ_Compte = value; }
        }

        string m_Dev_Produit;
        [DataMember(Name = "DEV_PRODUIT", Order = 40)]
        public string Dev_Produit
        {
            get { return m_Dev_Produit; }
            set { m_Dev_Produit = value; }
        }

        DateTime m_DtIns;
        [DataMember(Name = "DTINS", Order = 41)]
        public DateTime DtIns
        {
            get { return m_DtIns; }
            set { m_DtIns = value; }
        }

        int m_IdaIns;
        [DataMember(Name = "IDAINS", Order = 42)]
        public int IdaIns
        {
            get { return m_IdaIns; }
            set { m_IdaIns = value; }
        }
    }
    #endregion

    #region CompareData

    //    internal sealed class CompareData 
    //    {    
    //        #region Members
    //        //private DateTime m_Age_Enreg;
    //        private decimal m_Cours_Op;
    //        private DateTime m_Date_Nego;
    //        private DateTime m_Date_Trait;
    //        private string m_Instrt;
    //        private string m_Marche;
    //        private string m_Refer_Op;
    //        private string m_Num_Compte;
    //        #endregion
    //        #region Accessors
    //        //public DateTime Age_Enreg
    //        //{
    //        //    get { return this.m_Age_Enreg; }
    //        //    set { this.m_Age_Enreg = value; }
    //        //}
    //        public Decimal Cours_Op
    //        {
    //            get { return this.m_Cours_Op; }
    //            set { this.m_Cours_Op = value; }
    //        }
    //        public DateTime Date_Nego
    //        {
    //            get { return this.m_Date_Nego; }
    //            set { this.m_Date_Nego = value; }
    //        }
    //        public DateTime Date_Trait
    //        {
    //            get { return this.m_Date_Trait; }
    //            set { this.m_Date_Trait = value; }
    //        }
    //        public string Instrt
    //        {
    //            get { return this.m_Instrt; }
    //            set { this.m_Instrt = value; }
    //        }
    //        public string Marche
    //        {
    //            get { return this.m_Marche; }
    //            set { this.m_Marche = value; }
    //        }
    //        public string Refer_Op
    //        {
    //            get { return this.m_Refer_Op; }
    //            set { this.m_Refer_Op = value; }
    //        }
    //        public string Num_Compte
    //        {
    //            get { return this.m_Num_Compte; }
    //            set { this.m_Num_Compte = value; }
    //        }
    //        #endregion

    //        //Dictionary<string, ValueErrorStatus> m_Test = null;

    //        //public void  Initialise()
    //        //{
    //        //    m_Test = new Dictionary<string, ValueErrorStatus>();
    //        //    //
    //        //    m_Test.Add("COURSOP", new ValueErrorStatus(Cours_Op, MatchStatus.UNMATCH ));
    //        //    m_Test.Add("DATENEGO", new ValueErrorStatus(Date_Nego, MatchStatus.UNMATCH));
    //        //    m_Test.Add("DATETRAIT", new ValueErrorStatus(Date_Trait, MatchStatus.UNMATCH));
    //        //    m_Test.Add("INSTRT", new ValueErrorStatus(Instrt, MatchStatus.UNMATCH));
    //        //    m_Test.Add("MARCHE", new ValueErrorStatus(Marche MatchStatus.UNMATCH));
    //        //    m_Test.Add("REFEROP", new ValueErrorStatus(Refer_Op MatchStatus.UNMATCH));
    //        //}

    //        //public QtyErrorStatus[]  Qtys
    //        //{
    //        //    get { throw new NotImplementedException(); }
    //        //}

    //        //public QtyErrorStatus  QtyErrorStatusByKey(string keyname)
    //        //{
    //        //    throw new NotImplementedException();
    //        //}

    //        //public object[]  ComparisonKey
    //        //{
    //        //    get { throw new NotImplementedException(); }
    //        //}

    //        //public string  Message
    //        //{
    //        //    get { throw new NotImplementedException(); }
    //        //}

    //        //public object  Id
    //        //{
    //        //    get { throw new NotImplementedException(); }
    //        //}

    //        //public DateTime  Age_Enreg_Eurosys
    //        //{
    //        //    get { throw new NotImplementedException(); }
    //        //}

    //        //public string  Time_Enreg_Eurosys
    //        //{
    //        //    get { throw new NotImplementedException(); }
    //        //}

    //        //public string  Typ_Compte_Eurosys
    //        //{
    //        //    get { throw new NotImplementedException(); }
    //        //}
    //}

    public class EurosysTradesCompareData
    {
        private DateTime m_Date_Nego;
        public DateTime Date_Nego
        {
            get { return this.m_Date_Nego; }
            set { this.m_Date_Nego = value; }
        }

        private string m_Produit;
        public string Produit
        {
            get { return m_Produit; }
            set { m_Produit = value; }
        }

        private string m_Nom_Eche;
        public string Nom_Eche
        {
            get { return m_Nom_Eche; }
            set { m_Nom_Eche = value; }
        }

        private string m_Sens_Op;
        public string Sens_Op
        {
            get { return m_Sens_Op; }
            set { m_Sens_Op = value; }
        }

        private string m_Instrt;
        public string Instrt
        {
            get { return this.m_Instrt; }
            set { this.m_Instrt = value; }
        }

        private int m_Quant_Op;
        public int Quant_Op
        {
            get { return m_Quant_Op; }
            set { m_Quant_Op = value; }
        }

        private double m_Cours_Op;
        public double Cours_Op
        {
            get { return m_Cours_Op; }
            set { m_Cours_Op = value; }
        }

        private double m_Strk_Op;
        public double Strk_Op
        {
            get { return m_Strk_Op; }
            set { m_Strk_Op = value; }
        }

        private string m_Refer_Op;
        public string Refer_Op
        {
            get { return this.m_Refer_Op; }
            set { this.m_Refer_Op = value; }
        }

        private string m_Grp_Cpte;
        public string Grp_Cpte
        {
            get { return m_Grp_Cpte; }
            set { m_Grp_Cpte = value; }
        }

        private string m_Num_Compte;
        public string Num_Compte
        {
            get { return this.m_Num_Compte; }
            set { this.m_Num_Compte = value; }
        }

        private string m_Marche;
        public string Marche
        {
            get { return this.m_Marche; }
            set { this.m_Marche = value; }
        }

        private string m_User_Data;
        public string User_Data
        {
            get { return m_User_Data; }
            set { m_User_Data = value; }
        }

        private double m_Mont_Court_Lot;
        public double Mont_Court_Lot
        {
            get { return m_Mont_Court_Lot; }
            set { m_Mont_Court_Lot = value; }
        }

        private double m_Mont_Court_Lg;
        public double Mont_Court_Lg
        {
            get { return m_Mont_Court_Lg; }
            set { m_Mont_Court_Lg = value; }
        }

        private double m_Cours_Clop;
        public double Cours_Clop
        {
            get { return m_Cours_Clop; }
            set { m_Cours_Clop = value; }
        }

        private string m_Cpt_Inter;
        public string Cpt_Inter
        {
            get { return m_Cpt_Inter; }
            set { m_Cpt_Inter = value; }
        }

        private string m_Id_Gn_Op_Int;
        public string Id_Gn_Op_Int
        {
            get { return m_Id_Gn_Op_Int; }
            set { m_Id_Gn_Op_Int = value; }
        }

        private double m_Mont_Court_Lot_Int;
        public double Mont_Court_Lot_Int
        {
            get { return m_Mont_Court_Lot_Int; }
            set { m_Mont_Court_Lot_Int = value; }
        }

        private double m_Mont_Court_Lg_Int;
        public double Mont_Court_Lg_Int
        {
            get { return m_Mont_Court_Lg_Int; }
            set { m_Mont_Court_Lg_Int = value; }
        }

        private string m_Typ_X_Depouille;
        public string Typ_X_Depouille
        {
            get { return m_Typ_X_Depouille; }
            set { m_Typ_X_Depouille = value; }
        }

        private string m_Num_Ordre;
        public string Num_Ordre
        {
            get { return m_Num_Ordre; }
            set { m_Num_Ordre = value; }
        }

        private string m_Typ_Compte;
        public string Typ_Compte
        {
            get { return m_Typ_Compte; }
            set { m_Typ_Compte = value; }
        }

    }
    #endregion

    public class EurosysTradesAudit
    {
        #region Members
        protected string m_Cs;
        protected IOTask m_IOTask;
        protected ProcessBase m_ProcessBase;
        private int m_UserId;
        protected DateTime m_NowParam;
        public const string nowParamName = "pdtTreatmentDateTime";
        protected DateTime m_EarlierClearingDateParam;
        public const string earlierClearingDateParamName = "pdtEarlierClearingDate";
        public const string taskIdentifierParamName = "psTaskIdentifier";
        protected string m_TaskIdentifierParam;
        public const string processLogIdParamName = "pnProcessLogId";
        protected string m_ProcessLogIdParam;
        protected List<SNegoAudit> m_AuditRecordsSet;
        protected string m_LogMsg;
        protected Cst.ErrLevel m_CodeReturn;
        protected IDbTransaction m_DbTransaction;
        protected string m_MarketType;
        #endregion

        #region Accessors
        public string Cs
        {
            get { return this.m_Cs; }
            set { this.m_Cs = value; }
        }
        public IOTask IOTask
        {
            get { return this.m_IOTask; }
            set { this.m_IOTask = value; }
        }
        public ProcessBase ProcessBase
        {
            get { return this.m_ProcessBase; }
            set { this.m_ProcessBase = value; }
        }
        public int UserId
        {
            get { return this.m_UserId; }
            set { this.m_UserId = value; }
        }
        public DateTime NowParam
        {
            get { return this.m_NowParam; }
            set { this.m_NowParam = value; }
        }
        public DateTime EarlierClearingDateParam
        {
            get { return this.m_EarlierClearingDateParam; }
            set { this.m_EarlierClearingDateParam = value; }
        }
        public string TaskIdentifierParam
        {
            get { return this.m_TaskIdentifierParam; }
            set { this.m_TaskIdentifierParam = value; }
        }
        public string ProcessLogIdParam
        {
            get { return this.m_ProcessLogIdParam; }
            set { this.m_ProcessLogIdParam = value; }
        }
        public List<SNegoAudit> AuditRecordsSet
        {
            get { return this.m_AuditRecordsSet; }
            set { this.m_AuditRecordsSet = value; }
        }
        public string LogMsg
        {
            get { return this.m_LogMsg; }
            set { this.m_LogMsg = value; }
        }
        public IDbTransaction DbTransaction
        {
            get { return this.m_DbTransaction; }
            set { this.m_DbTransaction = value; }
        }
        public Cst.ErrLevel CodeReturn
        {
            get { return this.m_CodeReturn; }
            set { this.m_CodeReturn = value; }
        }
        public string MarketType
        {
            get { return this.m_MarketType; }
            set { this.m_MarketType = value; }
        }

        #endregion

        #region constructor
        public EurosysTradesAudit(string pCs, IOTask pIOTask, ProcessBase pProcessBase, int pUserId)
        {
            Cs = pCs;
            m_IOTask = pIOTask;
            m_ProcessBase = pProcessBase;
            m_UserId = pUserId;
            m_AuditRecordsSet = new List<SNegoAudit>();
            m_LogMsg = String.Empty;
            DbTransaction = null;
            m_MarketType = "NON MEMBRE";
        }
        #endregion

        // parameters
        /// <summary>
        /// GetParameters
        /// Enhances the objects with input datas from task parameters 
        /// </summary>
        /// <param name="m_IOTask"></param>
        public void GetParameters()
        {
            NowParam = Convert.ToDateTime(IOTask.GetTaskParamValue(nowParamName));
            EarlierClearingDateParam = Convert.ToDateTime(IOTask.GetTaskParamValue(earlierClearingDateParamName));
            TaskIdentifierParam = IOTask.GetTaskParamValue(taskIdentifierParamName);
            ProcessLogIdParam = IOTask.GetTaskParamValue(processLogIdParamName);
        }
        // Controls
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClearingdate"></param>
        /// <returns></returns>
        //private bool IsTreatedDay(DateTime pClearingDate)
        //{
        //    bool ret = false;
        //    DateTime date = DateTime.MinValue;
        //    IDataReader dr = null;
        //    try
        //    {
        //        DataParameters parameters = new DataParameters();
        //        StrBuilder query = new StrBuilder();
        //        query += SQLCst.SELECT + SQLCst.MIN + " (DATE_ANTER) as DATE_ANTER " + Cst.CrLf;
        //        query += SQLCst.FROM_DBO + " SOCIETE " + Cst.CrLf;
        //        dr = DataHelper.ExecuteReader(Cs, CommandType.Text, query.ToString(), parameters.GetArrayDbParameter());
        //        if (dr.Read())
        //        {
        //            if (false == Convert.IsDBNull(dr["DATE_ANTER"]))
        //                date = Convert.ToDateTime((dr["DATE_ANTER"]));
        //        }

        //        if (pClearingDate <= date)
        //            ret = true;
        //    }
        //    finally
        //    {
        //        if (null != dr)
        //        {
        //            dr.Close();
        //            dr.Dispose();
        //        }
        //    }

        //    return ret;
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClearingDate"></param>
        /// <returns></returns>
        // EG 20180426 Analyse du code Correction [CA2202]
        private bool FindDuplicateTrades(DateTime pClearingDate)
        {
            int count = 0;
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(Cs, "CLEARINGDATE", DbType.DateTime), pClearingDate);
            StrBuilder query = new StrBuilder();
            query += SQLCst.SELECT + SQLCst.COUNT_ALL + SQLCst.AS + " COUNT " + Cst.CrLf;
            query += SQLCst.FROM_DBO + " S_NEGO " + Cst.CrLf;
            query += SQLCst.WHERE + " DATE_TRAIT = @CLEARINGDATE " + Cst.CrLf;
            query += SQLCst.GROUPBY + " REFER_OP " + SQLCst.HAVING + SQLCst.COUNT_ALL + " > 1 " + Cst.CrLf;

            using (IDataReader dr = DataHelper.ExecuteReader(Cs, CommandType.Text, query.ToString(), parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                {
                    if (false == Convert.IsDBNull(dr["COUNT"]))
                        count = Convert.ToInt32((dr["COUNT"]));
                }
            }
            //default value 
            //exist duplicate trades
            bool check = 1 < count;
            return check;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pActionTypeString"></param>
        /// <returns></returns>
        private ActionType GetActionTypeEnum(string pActionTypeString)
        {
            ActionType ret = (ActionType)Enum.Parse(typeof(ActionType), pActionTypeString);
            return ret;
        }
        /// <summary>
        ///  returns new version depending on last exported version
        /// </summary>
        /// <param name="pPreviousVersion"></param>
        /// <returns></returns>
        private int SetVersion(int pPreviousVersion)
        {
            int ret = 1;

            if (pPreviousVersion == 0 || pPreviousVersion == int.MinValue)
                return ret;
            else
            {
                ret = pPreviousVersion + 1;
                return ret;
            }
        }
        // dataReader (SELECT from Spheres and EUROSYS tables)
        /// <summary>
        /// Returns last export date from PROCESS_L table
        /// chiamare il metodo apposito che si occupa di convertire la sintassi oracle in sintassi T-SQL quando necessario
        /// esempio: DataHelper.SQLNumberToChar( );
        /// </summary>
        /// <param name="pTaskIdentifier"></param>
        /// <returns></returns>
        //private DateTime GetLastExportDate()
        //{
        //    DateTime lastExportDate = DateTime.MinValue;
        //    string successStatus = "SUCCESS";
        //    //
        //    DateTime defaultDate = Convert.ToDateTime("31-12-2009");
        //    //
        //    IDataReader dr = null;
        //    try
        //    {
        //        DataParameters parameters = new DataParameters();
        //        parameters.Add(new DataParameter(Cs, "TASKIDENTIFIER", DbType.String), TaskIdentifier);
        //        parameters.Add(new DataParameter(Cs, "SUCCESSSTATUS", DbType.String), successStatus);
        //        parameters.Add(new DataParameter(Cs, "IDPROCESS_L", DbType.Int32), ProcessLogId);
        //        StrBuilder query = new StrBuilder();
        //        query += SQLCst.SELECT + " max(p.DTSTPROCESS) as DTSTPROCESS " + Cst.CrLf;
        //        query += SQLCst.FROM_DBO + Cst.OTCml_TBL.PROCESS_L.ToString() + " p " + Cst.CrLf;
        //        query += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.IOTASK + " iot " + SQLCst.ON + " (iot.IDIOTASK = p.IDDATA) " + Cst.CrLf;
        //        query += SQLCst.WHERE + "iot.IDENTIFIER = @TASKIDENTIFIER " + Cst.CrLf;
        //        query += SQLCst.AND + "p.IDPROCESS_L != @IDPROCESS_L" + Cst.CrLf;
        //        query += SQLCst.AND + "p.IDSTPROCESS = @SUCCESSSTATUS" + Cst.CrLf;
        //        dr = DataHelper.ExecuteReader(Cs, CommandType.Text, query.ToString(), parameters.GetArrayDbParameter());
        //        if (dr.Read())
        //        {
        //            if (false == Convert.IsDBNull(dr["DTSTPROCESS"]))
        //                lastExportDate = Convert.ToDateTime((dr["DTSTPROCESS"]));
        //            else
        //                lastExportDate = defaultDate;
        //        }
        //        return lastExportDate;
        //    }
        //    finally
        //    {
        //        if (null != dr)
        //        {
        //            dr.Close();
        //            dr.Dispose();
        //        }
        //    }
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProcessLogId"></param>
        /// <returns></returns>
        // EG 20180426 Analyse du code Correction [CA2202]
        private DateTime GetDtProcess_L()
        {
            DateTime dtProcess_L = DateTime.MinValue;
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(Cs, "IDPROCESS_L", DbType.Int32), ProcessLogIdParam);
            StrBuilder query = new StrBuilder();
            query += SQLCst.SELECT + " DTSTPROCESS as DTSTPROCESS " + Cst.CrLf;
            query += SQLCst.FROM_DBO + Cst.OTCml_TBL.PROCESS_L.ToString() + " p " + Cst.CrLf;
            query += SQLCst.WHERE + "p.IDPROCESS_L = @IDPROCESS_L " + Cst.CrLf;

            using (IDataReader dr = DataHelper.ExecuteReader(Cs, CommandType.Text, query.ToString(), parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                {
                    if (false == Convert.IsDBNull(dr["DTSTPROCESS"]))
                        dtProcess_L = Convert.ToDateTime((dr["DTSTPROCESS"]));
                }
            }
            return dtProcess_L;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20180426 Analyse du code Correction [CA2202]
        private DateTime GetLastTreatedDate(string pMarketType)
        {
            DateTime date = DateTime.MinValue;
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(Cs, "MARKETTYPE", DbType.String), pMarketType);
            StrBuilder query = new StrBuilder();
            query += SQLCst.SELECT + SQLCst.MAX + " (DATE_TRAIT) as DATE_TRAIT " + Cst.CrLf;
            query += SQLCst.FROM_DBO + " EVW_MOUCHARD_EXT " + Cst.CrLf;
            query += SQLCst.WHERE + " TYP_MARCHE = @MARKETTYPE " + Cst.CrLf;

            using (IDataReader dr = DataHelper.ExecuteReader(Cs, CommandType.Text, query.ToString(), parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                {
                    if (false == Convert.IsDBNull(dr["DATE_TRAIT"]))
                        date = Convert.ToDateTime((dr["DATE_TRAIT"]));
                }
            }
            return date;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20180426 Analyse du code Correction [CA2202]
        private string GetEntityLangAppli()
        {
            string culture = String.Empty;
            DataParameters parameters = new DataParameters();
            StrBuilder query = new StrBuilder();
            query += SQLCst.SELECT + " a.CULTURE as CULTURE" + Cst.CrLf;
            query += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a " + Cst.CrLf;
            query += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTORROLE + " ar " + SQLCst.ON + " (a.IDA = ar.IDA) " + Cst.CrLf;
            query += SQLCst.WHERE + " ar.IDROLEACTOR = 'ENTITY' " + Cst.CrLf;

            using (IDataReader dr = DataHelper.ExecuteReader(Cs, CommandType.Text, query.ToString(), parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                {
                    if (false == Convert.IsDBNull(dr["CULTURE"]))
                        culture = Convert.ToString((dr["CULTURE"]));
                }
            }
            string langAppli;
            switch (culture)
            {
                case ("fr-FR"):
                    langAppli = "F";
                    break;
                case ("en-EN"):
                    langAppli = "E";
                    break;
                case ("it-IT"):
                    langAppli = "I";
                    break;
                default:
                    langAppli = "F";
                    break;
            }
            return langAppli;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pNumCompte"></param>
        /// <returns></returns>
        // EG 20180426 Analyse du code Correction [CA2202]
        private string GetDomiciliation(string pNumCompte)
        {
            string domiciliation = String.Empty;
            string langAppli = GetEntityLangAppli();

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(Cs, "NUM_COMPTE", DbType.String), pNumCompte);
            parameters.Add(new DataParameter(Cs, "LANG_APPLI", DbType.String), langAppli);
            StrBuilder query = new StrBuilder();
            query += SQLCst.SELECT + " l.LIB_CHAMP as LIB_CHAMP " + Cst.CrLf;
            query += SQLCst.FROM_DBO + " COMPTE c " + Cst.CrLf;
            query += SQLCst.INNERJOIN_DBO + " LISTE l " + SQLCst.ON + " (l.VAL_CHAMP = c.TYP_RESID and l.NOM_CHAMP = 'TYP_RESIDENT' and L.LANG_APPLI = @LANG_APPLI) " + Cst.CrLf;
            query += SQLCst.WHERE + "c.NUM_COMPTE = @NUM_COMPTE " + Cst.CrLf;

            using (IDataReader dr = DataHelper.ExecuteReader(Cs, CommandType.Text, query.ToString(), parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                {
                    if (false == Convert.IsDBNull(dr["LIB_CHAMP"]))
                        domiciliation = Convert.ToString((dr["LIB_CHAMP"]));
                }
            }
            return domiciliation.ToUpper();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTxTvaCourt"></param>
        /// <param name="pMethCalCrt"></param>
        /// <param name="pMontCourtLot"></param>
        /// <param name="pMontCourtLg"></param>
        /// <param name="pQuantOp"></param>
        /// <returns></returns>
        private decimal GetTva(decimal pTxTvaCourt, string pMethCalCrt, decimal pMontCourtLot, decimal pMontCourtLg, int pQuantOp)
        {
            decimal brokerageAmount;
            if (pMethCalCrt == "F")
                brokerageAmount = pMontCourtLot + pMontCourtLg;
            else
                brokerageAmount = (pMontCourtLot * pQuantOp) + pMontCourtLg;

            // query delivered by the client
            // round( ( nvl(COMPTE.TX_TVA_COURT, 0) * decode(nego.METH_CAL_CRT, 'F', (nego.MONT_COURT_LOT + nego.MONT_COURT_LG), ((nego.MONT_COURT_LOT * nego.QUANT_OP) + nego.MONT_COURT_LG))) / 100, 2) TVA,
            decimal tva = pTxTvaCourt * brokerageAmount / 100;

            return tva;
        }
        /// <summary>
        /// Returns the class of a product
        /// </summary>
        /// <param name="pProduit"></param>
        /// <returns></returns>
        // EG 20180426 Analyse du code Correction [CA2202]
        private string GetClassProduit(string pProduit)
        {
            string devClassProduit = String.Empty;

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(Cs, "PRODUIT", DbType.String), pProduit);
            StrBuilder query = new StrBuilder();
            query += SQLCst.SELECT + " CLASSE as CLASSE " + Cst.CrLf;
            query += SQLCst.FROM_DBO + " CLS_PROD  " + Cst.CrLf;
            query += SQLCst.WHERE + "PRODUIT = @PRODUIT " + Cst.CrLf;

            using (IDataReader dr = DataHelper.ExecuteReader(Cs, CommandType.Text, query.ToString(), parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                {
                    if (false == Convert.IsDBNull(dr["CLASSE"]))
                        devClassProduit = Convert.ToString((dr["CLASSE"]));
                }
            }
            return devClassProduit;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduit"></param>
        /// <param name="pNumCompte"></param>
        /// <returns></returns>
        // EG 20180426 Analyse du code Correction [CA2202]
        private string GetDevCrtCom(string pProduit, string pNumCompte)
        {
            string devCrtCom = String.Empty;
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(Cs, "PRODUIT", DbType.String), pProduit);
            parameters.Add(new DataParameter(Cs, "NUM_PROPRIET", DbType.String), pNumCompte);
            StrBuilder query = new StrBuilder();
            query += SQLCst.SELECT + " d.DEV_CRT as DEV_CRT " + Cst.CrLf;
            query += SQLCst.FROM_DBO + " DEV_CRT_COM d " + Cst.CrLf;
            query += SQLCst.WHERE + "d.PRODUIT = @PRODUIT " + Cst.CrLf;
            query += SQLCst.AND + " d.NUM_PROPRIET = @NUM_PROPRIET " + Cst.CrLf;

            using (IDataReader dr = DataHelper.ExecuteReader(Cs, CommandType.Text, query.ToString(), parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                {
                    if (false == Convert.IsDBNull(dr["DEV_CRT"]))
                        devCrtCom = Convert.ToString((dr["DEV_CRT"]));
                }
            }
            return devCrtCom;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pNumCompte"></param>
        /// <returns></returns>
        // EG 20180426 Analyse du code Correction [CA2202]
        private string GetRaSocial(string pNumCompte)
        {
            string raSocial = String.Empty;

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(Cs, "NUM_COMPTE", DbType.String), pNumCompte);
            StrBuilder query = new StrBuilder();
            query += SQLCst.SELECT + " c.RA_SOCIAL as RA_SOCIAL " + Cst.CrLf;
            query += SQLCst.FROM_DBO + " COMPTE c " + Cst.CrLf;
            query += SQLCst.WHERE + "c.NUM_COMPTE = @NUM_COMPTE " + Cst.CrLf;

            using (IDataReader dr = DataHelper.ExecuteReader(Cs, CommandType.Text, query.ToString(), parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                {
                    if (false == Convert.IsDBNull(dr["RA_SOCIAL"]))
                        raSocial = Convert.ToString((dr["RA_SOCIAL"]));
                }
            }
            return raSocial;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pNumCompte"></param>
        /// <returns></returns>
        // EG 20180426 Analyse du code Correction [CA2202]
        private decimal GetTxTvaCourt(string pNumCompte)
        {
            decimal txTvaCourt = Decimal.Zero;
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(Cs, "NUM_COMPTE", DbType.String), pNumCompte);
            StrBuilder query = new StrBuilder();
            query += SQLCst.SELECT + " c.TX_TVA_COURT as TX_TVA_COURT " + Cst.CrLf;
            query += SQLCst.FROM_DBO + " COMPTE c " + Cst.CrLf;
            query += SQLCst.WHERE + "c.NUM_COMPTE = @NUM_COMPTE " + Cst.CrLf;

            using (IDataReader dr = DataHelper.ExecuteReader(Cs, CommandType.Text, query.ToString(), parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                {
                    if (false == Convert.IsDBNull(dr["TX_TVA_COURT"]))
                        txTvaCourt = Convert.ToDecimal((dr["TX_TVA_COURT"]));
                }
            }
            return txTvaCourt;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduit"></param>
        /// <returns></returns>
        // EG 20180426 Analyse du code Correction [CA2202]
        private string GetDevProduit(string pProduit)
        {
            string devProduit = String.Empty;

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(Cs, "PRODUIT", DbType.String), pProduit);
            StrBuilder query = new StrBuilder();
            query += SQLCst.SELECT + " p.DEV_PRODUIT as DEV_PRODUIT " + Cst.CrLf;
            query += SQLCst.FROM_DBO + " PRODUIT p " + Cst.CrLf;
            query += SQLCst.WHERE + "p.PRODUIT = @PRODUIT " + Cst.CrLf;

            using (IDataReader dr = DataHelper.ExecuteReader(Cs, CommandType.Text, query.ToString(), parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                {
                    if (false == Convert.IsDBNull(dr["DEV_PRODUIT"]))
                        devProduit = Convert.ToString((dr["DEV_PRODUIT"]));
                }
            }
            return devProduit;
        }

        // Audit dataset and feed record
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLastExportDate"></param>
        /// <param name="pNow"></param>
        /// <param name="pClearingDate"></param>
        /// <returns></returns>
        private List<SNegoAudit> FeedAuditRecordsSet(DateTime pClearingDate, DateTime pLastTreatedDate)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(Cs, "CLEARINGDATE", DbType.DateTime), pClearingDate);
            parameters.Add(new DataParameter(Cs, "LASTTREATEDDATE", DbType.DateTime), pLastTreatedDate);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SELECT + "IDS_NEGO_AUDIT,VERSION,ACTIONTYPE,IDPROCESS_L,DTPROCESS_L,EUROSYS_USER,HOSTNAME," + Cst.CrLf;
            sqlQuery += "AGE_ENREG,COURS_CLOP,COURS_OP,COURS_OP_BRK,CPT_INTER,DATE_CLOP,DATE_NEGO,DATE_TRAIT,GRP_COM," + Cst.CrLf;
            sqlQuery += "GRP_COURT,GRP_COURT_BRK,GRP_COURT_INT,GRP_CPTE,HRE_NEGO,ID_ERREUR,ID_GN_OP,ID_GN_OP_BRK,ID_GN_OP_GRP_CPTE," + Cst.CrLf;
            sqlQuery += "ID_GN_OP_INT,INSTRT,MARCHE,METH_CAL_COM,METH_CAL_CRT,MONT_COM_GLOBEX,MONT_COM_LOT,MONT_COM_LOT_BRK,MONT_COM_LOT_GRP," + Cst.CrLf;
            sqlQuery += "MONT_COURT_LOT,MONT_COURT_LOT_GRP,MONT_COURT_LOT_BRK,MONT_COURT_LOT_INT,NOM_ECHE,NUM_BROKER,NUM_COMPTE,NUM_ORDRE,OBSERVATION1," + Cst.CrLf;
            sqlQuery += "PRODUIT,QUANT_OP,REFER_CLOP,REFER_OP,SENS_OP,STRK_OP,TIME_ENREG,TYP_COMPTE,TYP_MAR_OP,TYP_MARCHE,TYP_OP,TYP_OP_BRK," + Cst.CrLf;
            sqlQuery += "TYP_X_DEPOUILLE,TYP_NEGO,NOM_USER,MONT_COURT_LG,MONT_COURT_LG_GRP,MONT_COURT_LG_BRK,MONT_COURT_LG_INT,ID_VALID,MONT_COURT_LOT_GRP_NEG," + Cst.CrLf;
            sqlQuery += "MONT_COURT_LOT_COM,MONT_COURT_LOT_NEG,MONT_COM_LOT_COM,MONT_COM_LOT_GRP_COM,MONT_COM_LOT_NEG,MONT_COM_LOT_GRP_NEG,MONT_COURT_LOT_GRP_COM," + Cst.CrLf;
            sqlQuery += "METH_CAL_COM_GRP,METH_CAL_CRT_GRP,USER_DATA,METH_CAL_CRT_BRK,METH_CAL_COM_BRK,METH_CAL_CRT_INT,ID_MATCHING,SPLIT,SPLIT_CLOP," + Cst.CrLf;
            sqlQuery += "CODE_TYPE_ORDRE,STRIKE_MODIFIED,DTINS,IDAINS,DTUPD,IDAUPD" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + "S_NEGO_AUDIT" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + " DATE_TRAIT <= @CLEARINGDATE " + Cst.CrLf;
            sqlQuery += SQLCst.AND + " DATE_TRAIT <= @LASTTREATEDDATE " + Cst.CrLf;
            sqlQuery += SQLCst.AND + " IDPROCESS_L " + SQLCst.IS_NULL + Cst.CrLf;
            // feed auditRecordset from dataset
            List<SNegoAudit> recordsSet = DataHelper<SNegoAudit>.ExecuteDataSet(DataHelper.OpenConnection(Cs), CommandType.Text, sqlQuery.ToString(), parameters.GetArrayDbParameter());
            return recordsSet;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAuditRecordsSet"></param>
        /// <param name="pReferOp"></param>
        /// <param name="pDtIns"></param>
        /// <returns></returns>
        private SNegoAudit FeedAuditRecord(string pReferOp, int pId)
        {
            SNegoAudit auditRecord = (from audit in AuditRecordsSet
                                      where audit.Refer_Op == pReferOp && audit.IdS_Nego_Audit == pId
                                      select new SNegoAudit
                                      {
                                          IdS_Nego_Audit = audit.IdS_Nego_Audit,
                                          Version = audit.Version,
                                          ActionType = audit.ActionType,
                                          Eurosys_User = audit.Eurosys_User,
                                          HostName = audit.HostName,
                                          Age_Enreg = audit.Age_Enreg,
                                          Cours_Clop = audit.Cours_Clop,
                                          Cours_Op = audit.Cours_Op,
                                          Cours_Op_Brk = audit.Cours_Op_Brk,
                                          Cpt_Inter = audit.Cpt_Inter,
                                          Date_Clop = audit.Date_Clop,
                                          Date_Nego = audit.Date_Nego,
                                          Date_Trait = audit.Date_Trait,
                                          Grp_Com = audit.Grp_Com,
                                          Grp_Court = audit.Grp_Court,
                                          Grp_Court_Brk = audit.Grp_Court_Brk,
                                          Grp_Court_Int = audit.Grp_Court_Int,
                                          Grp_Cpte = audit.Grp_Cpte,
                                          Hre_Nego = audit.Hre_Nego,
                                          Id_Erreur = audit.Id_Erreur,
                                          Id_Gn_Op = audit.Id_Gn_Op,
                                          Id_Gn_Op_Brk = audit.Id_Gn_Op_Brk,
                                          Id_Gn_Op_Grp_Cpte = audit.Id_Gn_Op_Grp_Cpte,
                                          Id_Gn_Op_Int = audit.Id_Gn_Op_Int,
                                          Instrt = audit.Instrt,
                                          Marche = audit.Marche,
                                          Meth_Cal_Com = audit.Meth_Cal_Com,
                                          Meth_Cal_Crt = audit.Meth_Cal_Crt,
                                          Mont_Com_Globex = audit.Mont_Com_Globex,
                                          Mont_Com_Lot = audit.Mont_Com_Lot,
                                          Mont_Com_Lot_Brk = audit.Mont_Com_Lot_Brk,
                                          Mont_Com_Lot_Grp = audit.Mont_Com_Lot_Grp,
                                          Mont_Court_Lot = audit.Mont_Court_Lot,
                                          Mont_Court_Lot_Grp = audit.Mont_Court_Lot_Grp,
                                          Mont_Court_Lot_Brk = audit.Mont_Court_Lot_Brk,
                                          Mont_Court_Lot_Int = audit.Mont_Court_Lot_Int,
                                          Nom_Eche = audit.Nom_Eche,
                                          Num_Broker = audit.Num_Broker,
                                          Num_Compte = audit.Num_Compte,
                                          Num_Ordre = audit.Num_Ordre,
                                          Observation1 = audit.Observation1,
                                          Produit = audit.Produit,
                                          Quant_Op = audit.Quant_Op,
                                          Refer_Clop = audit.Refer_Clop,
                                          Refer_Op = audit.Refer_Op,
                                          Sens_Op = audit.Sens_Op,
                                          Strk_Op = audit.Strk_Op,
                                          Time_Enreg = audit.Time_Enreg,
                                          Typ_Compte = audit.Typ_Compte,
                                          Typ_Mar_Op = audit.Typ_Mar_Op,
                                          Typ_Marche = audit.Typ_Marche,
                                          Typ_Op = audit.Typ_Op,
                                          Typ_Op_Brk = audit.Typ_Op_Brk,
                                          Typ_X_Depouille = audit.Typ_X_Depouille,
                                          Typ_Nego = audit.Typ_Nego,
                                          Nom_User = audit.Nom_User,
                                          Mont_Court_Lg = audit.Mont_Court_Lg,
                                          Mont_Court_Lg_Grp = audit.Mont_Court_Lg,
                                          Mont_Court_Lg_Brk = audit.Mont_Court_Lg_Brk,
                                          Mont_Court_Lg_Int = audit.Mont_Court_Lg_Int,
                                          Id_Valid = audit.Id_Valid,
                                          Mont_Court_Lg_Grp_Neg = audit.Mont_Court_Lg_Grp_Neg,
                                          Mont_Court_Lot_Com = audit.Mont_Court_Lot_Com,
                                          Mont_Court_Lot_Neg = audit.Mont_Court_Lot_Neg,
                                          Mont_Com_Lot_Com = audit.Mont_Com_Lot_Com,
                                          Mont_Com_Lot_Grp_Com = audit.Mont_Com_Lot_Grp_Com,
                                          Mont_Com_Lot_Neg = audit.Mont_Com_Lot_Neg,
                                          Mont_Com_Lot_Grp_Neg = audit.Mont_Com_Lot_Grp_Neg,
                                          Mont_Court_Lot_Grp_Com = audit.Mont_Court_Lot_Grp_Com,
                                          Meth_Cal_Com_Grp = audit.Meth_Cal_Com_Grp,
                                          Meth_Cal_Crt_Grp = audit.Meth_Cal_Crt_Grp,
                                          User_Data = audit.User_Data,
                                          Meth_Cal_Crt_Brk = audit.Meth_Cal_Crt_Brk,
                                          Meth_Cal_Com_Brk = audit.Meth_Cal_Com_Brk,
                                          Meth_Cal_Crt_Int = audit.Meth_Cal_Crt_Int,
                                          Id_Matching = audit.Id_Matching,
                                          Split = audit.Split,
                                          Split_Clop = audit.Split_Clop,
                                          Code_Type_Ordre = audit.Code_Type_Ordre,
                                          Strike_Modified = audit.Strike_Modified,
                                          DtIns = audit.DtIns,
                                          IdaIns = audit.IdaIns,
                                      }
                        ).FirstOrDefault();

            return auditRecord;
        }

        // Export dataset and feed record
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pReferOp"></param>
        /// <returns></returns>
        private List<ExportBr_O> FeedExportedRecordsSet(string pReferOp)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(Cs, "REFER_OP", DbType.String), pReferOp);
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SELECT_ALL + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + "EXPORTBR_O" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "REFER_OP = @REFER_OP" + Cst.CrLf;
            // feed audit collection from dataset 
            List<ExportBr_O> recordsSet = DataHelper<ExportBr_O>.ExecuteDataSet(DataHelper.OpenConnection(Cs), CommandType.Text, sqlQuery.ToString(), parameters.GetArrayDbParameter());
            return recordsSet;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pExportRecordsSet"></param>
        /// <param name="pDtIns"></param>
        /// <returns></returns>
        private ExportBr_O FeedExportRecord(List<ExportBr_O> pExportRecordsSet, DateTime pDtIns)
        {
            ExportBr_O exportRecord = (from export in pExportRecordsSet
                                       where export.DtIns == pDtIns
                                       select new ExportBr_O
                                       {
                                           IdExportBRO = export.IdExportBRO,
                                           Version = export.Version,
                                           ActionType = export.ActionType,
                                           IdS_Nego_Audit = export.IdS_Nego_Audit,
                                           DtInsS_Nego_Audit = export.DtInsS_Nego_Audit,
                                           IdProcessL = export.IdProcessL,
                                           DtProcessL = export.DtProcessL,
                                           DtBusiness = export.DtBusiness,
                                           EurosysUser = export.EurosysUser,
                                           HostName = export.HostName,
                                           Date_Nego = export.Date_Nego,
                                           Produit = export.Produit,
                                           Nom_Eche = export.Nom_Eche,
                                           Sens_Op = export.Sens_Op,
                                           Instrt = export.Instrt,
                                           Quant_Op = export.Quant_Op,
                                           Cours_Op = export.Cours_Op,
                                           Strk_Op = export.Strk_Op,
                                           Refer_Op = export.Refer_Op,
                                           Grp_Cpte = export.Grp_Cpte,
                                           Num_Compte = export.Num_Compte,
                                           Marche = export.Marche,
                                           User_Data = export.User_Data,
                                           Domiciliation = export.Domiciliation,
                                           Mont_Court_Lot = export.Mont_Court_Lot,
                                           Mont_Court_Lg = export.Mont_Court_Lg,
                                           Tva = export.Tva,
                                           Dev_Crt_Com = export.Dev_Crt_Com,
                                           Ra_Social = export.Ra_Social,
                                           Tx_Tva_Court = export.Tx_Tva_Court,
                                           Age_Enreg = export.Age_Enreg,
                                           Cours_Clop = export.Cours_Clop,
                                           Cpt_Inter = export.Cpt_Inter,
                                           Id_Gn_Op_Int = export.Id_Gn_Op_Int,
                                           Mont_Court_Lot_Int = export.Mont_Court_Lot_Int,
                                           Mont_Court_Lg_Int = export.Mont_Court_Lg_Int,
                                           Typ_X_Depouille = export.Typ_X_Depouille,
                                           Num_Ordre = export.Num_Ordre,
                                           Typ_Compte = export.Typ_Compte,
                                           Dev_Produit = export.Dev_Produit,
                                           DtIns = export.DtIns,
                                           IdaIns = export.IdaIns,
                                       }
                        ).FirstOrDefault();
            return exportRecord;
        }

        // Compare data
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAuditRecord">SNegoAudit</param>
        /// <param name="pExportRecord">ExportBr_O</param>
        private bool CompareData(SNegoAudit pAuditRecord, ExportBr_O pExportRecord)
        {
            // false --> Different business values beetween internal and external data elements
            // true --> The same business values beetween internal and external data elements
            bool match = true;

            EurosysTradesCompareData intDataElems = new EurosysTradesCompareData();
            intDataElems = FeedCompareData(intDataElems, CompareSource.Internal, pAuditRecord, null);
            //
            EurosysTradesCompareData extDataElems = new EurosysTradesCompareData();
            extDataElems = FeedCompareData(extDataElems, CompareSource.External, null, pExportRecord);

            if (
                (intDataElems.Date_Nego != extDataElems.Date_Nego) ||
                (intDataElems.Produit != extDataElems.Produit) ||
                (intDataElems.Nom_Eche != extDataElems.Nom_Eche) ||
                (intDataElems.Sens_Op != extDataElems.Sens_Op) ||
                (intDataElems.Instrt != extDataElems.Instrt) ||
                (intDataElems.Quant_Op != extDataElems.Quant_Op) ||
                (intDataElems.Cours_Op != extDataElems.Cours_Op) ||
                (intDataElems.Strk_Op != extDataElems.Strk_Op) ||
                (intDataElems.Refer_Op != extDataElems.Refer_Op) ||
                (intDataElems.Grp_Cpte != extDataElems.Grp_Cpte) ||
                (intDataElems.Num_Compte != extDataElems.Num_Compte) ||
                (intDataElems.Marche != extDataElems.Marche) ||
                (intDataElems.User_Data != extDataElems.User_Data) ||
                (intDataElems.Mont_Court_Lot != extDataElems.Mont_Court_Lot) ||
                (intDataElems.Mont_Court_Lg != extDataElems.Mont_Court_Lg) ||
                (intDataElems.Cours_Clop != extDataElems.Cours_Clop) ||
                (intDataElems.Cpt_Inter != extDataElems.Cpt_Inter) ||
                (intDataElems.Id_Gn_Op_Int != extDataElems.Id_Gn_Op_Int) ||
                (intDataElems.Mont_Court_Lot_Int != extDataElems.Mont_Court_Lot_Int) ||
                (intDataElems.Mont_Court_Lg_Int != extDataElems.Mont_Court_Lg_Int) ||
                (intDataElems.Typ_X_Depouille != extDataElems.Typ_X_Depouille) ||
                (intDataElems.Num_Ordre != extDataElems.Num_Ordre) ||
                (intDataElems.Typ_Compte != extDataElems.Typ_Compte)
                )
                match = false;
            return match;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDataElems"></param>
        /// <param name="PSource"></param>
        /// <param name="pAuditRecord"></param>
        /// <param name="pExportRecord"></param>
        /// <returns></returns>
        private EurosysTradesCompareData FeedCompareData(EurosysTradesCompareData pDataElems, CompareSource PSource, SNegoAudit pAuditRecord, ExportBr_O pExportRecord)
        {

            switch (PSource)
            {
                case (CompareSource.Internal):
                    pDataElems.Date_Nego = pAuditRecord.Date_Nego;
                    pDataElems.Produit = pAuditRecord.Produit;
                    pDataElems.Nom_Eche = pAuditRecord.Nom_Eche;
                    pDataElems.Sens_Op = pAuditRecord.Sens_Op;
                    pDataElems.Instrt = pAuditRecord.Instrt;
                    pDataElems.Quant_Op = pAuditRecord.Quant_Op;
                    pDataElems.Cours_Op = pAuditRecord.Cours_Op;
                    pDataElems.Strk_Op = pAuditRecord.Strk_Op;
                    pDataElems.Refer_Op = pAuditRecord.Refer_Op;
                    pDataElems.Grp_Cpte = pAuditRecord.Grp_Cpte;
                    pDataElems.Num_Compte = pAuditRecord.Num_Compte;
                    pDataElems.Marche = pAuditRecord.Marche;
                    pDataElems.User_Data = pAuditRecord.User_Data;
                    pDataElems.Mont_Court_Lot = pAuditRecord.Mont_Court_Lot;
                    pDataElems.Mont_Court_Lg = pAuditRecord.Mont_Court_Lg;
                    pDataElems.Cours_Clop = pAuditRecord.Cours_Clop;
                    pDataElems.Cpt_Inter = pAuditRecord.Cpt_Inter;
                    pDataElems.Id_Gn_Op_Int = pAuditRecord.Id_Gn_Op_Int;
                    pDataElems.Mont_Court_Lot_Int = pAuditRecord.Mont_Court_Lot_Int;
                    pDataElems.Mont_Court_Lg_Int = pAuditRecord.Mont_Court_Lg_Int;
                    pDataElems.Typ_X_Depouille = pAuditRecord.Typ_X_Depouille;
                    pDataElems.Num_Ordre = pAuditRecord.Num_Ordre;
                    pDataElems.Typ_Compte = pAuditRecord.Typ_Compte;
                    break;
                case (CompareSource.External):
                    pDataElems.Date_Nego = pExportRecord.Date_Nego;
                    pDataElems.Produit = pExportRecord.Produit;
                    pDataElems.Nom_Eche = pExportRecord.Nom_Eche;
                    pDataElems.Sens_Op = pExportRecord.Sens_Op;
                    pDataElems.Instrt = pExportRecord.Instrt;
                    pDataElems.Quant_Op = pExportRecord.Quant_Op;
                    pDataElems.Cours_Op = pExportRecord.Cours_Op;
                    pDataElems.Strk_Op = pExportRecord.Strk_Op;
                    pDataElems.Refer_Op = pExportRecord.Refer_Op;
                    pDataElems.Grp_Cpte = pExportRecord.Grp_Cpte;
                    pDataElems.Num_Compte = pExportRecord.Num_Compte;
                    pDataElems.Marche = pExportRecord.Marche;
                    pDataElems.User_Data = pExportRecord.User_Data;
                    pDataElems.Mont_Court_Lot = pExportRecord.Mont_Court_Lot;
                    pDataElems.Mont_Court_Lg = pExportRecord.Mont_Court_Lg;
                    pDataElems.Cours_Clop = pExportRecord.Cours_Clop;
                    pDataElems.Cpt_Inter = pExportRecord.Cpt_Inter;
                    pDataElems.Id_Gn_Op_Int = pExportRecord.Id_Gn_Op_Int;
                    pDataElems.Mont_Court_Lot_Int = pExportRecord.Mont_Court_Lot_Int;
                    pDataElems.Mont_Court_Lg_Int = pExportRecord.Mont_Court_Lg_Int;
                    pDataElems.Typ_X_Depouille = pExportRecord.Typ_X_Depouille;
                    pDataElems.Num_Ordre = pExportRecord.Num_Ordre;
                    pDataElems.Typ_Compte = pExportRecord.Typ_Compte;
                    break;
            }
            return pDataElems;
        }

        // DML (INSERT/UPDATE) 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pActionType"></param>
        /// <param name="pVersion"></param>
        /// <param name="pRecord"></param>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void WriteExportRecord(ActionType pActionType, int pVersion, SNegoAudit pRecord)
        {
            try
            {
                DateTime dtProcess_L = GetDtProcess_L();
                DateTime businessDate = GetLastTreatedDate(MarketType);
                string domiciliation = GetDomiciliation(pRecord.Num_Compte);
                decimal txTvaCourt = GetTxTvaCourt(pRecord.Num_Compte);
                decimal tva = GetTva(txTvaCourt, pRecord.Meth_Cal_Crt, Convert.ToDecimal(pRecord.Mont_Court_Lot), Convert.ToDecimal(pRecord.Mont_Court_Lg), pRecord.Quant_Op);
                string devCrtCom = GetDevCrtCom(pRecord.Produit, pRecord.Num_Compte);

                // GS 20120606: when fees currency is missing for a specific product
                // the fees currency of class product is used
                if (string.IsNullOrEmpty(devCrtCom))
                {
                    string classProduit = GetClassProduit(pRecord.Produit);
                    devCrtCom = GetDevCrtCom(classProduit, pRecord.Num_Compte);
                    if (string.IsNullOrEmpty(devCrtCom))
                    {
                        // FI 20200623 [XXXXX] SetErrorWarning
                        ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                        LogMsg = "Row discarded. Brokerage currency is missing for PRODUIT = [" + pRecord.Produit + "] and NUM_COMPTE = [" + pRecord.Num_Compte + "].";
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, LogMsg));
                    }
                }
                
                string raSocial = GetRaSocial(pRecord.Num_Compte);
                string devProduit = GetDevProduit(pRecord.Produit);
                //            
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(Cs, "VERSION", DbType.String), pVersion);
                parameters.Add(new DataParameter(Cs, "ACTIONTYPE", DbType.String), pActionType);
                parameters.Add(new DataParameter(Cs, "IDS_NEGO_AUDIT", DbType.Int32), pRecord.IdS_Nego_Audit);
                parameters.Add(new DataParameter(Cs, "DTINSS_NEGO_AUDIT", DbType.DateTime), pRecord.DtIns);
                parameters.Add(new DataParameter(Cs, "IDPROCESS_L", DbType.Int32), ProcessLogIdParam);
                parameters.Add(new DataParameter(Cs, "DTPROCESS_L", DbType.DateTime), dtProcess_L);
                parameters.Add(new DataParameter(Cs, "DTBUSINESS", DbType.DateTime), businessDate);
                parameters.Add(new DataParameter(Cs, "EUROSYS_USER", DbType.String), pRecord.Eurosys_User);
                parameters.Add(new DataParameter(Cs, "HOSTNAME", DbType.String), pRecord.HostName);
                parameters.Add(new DataParameter(Cs, "DATE_NEGO", DbType.DateTime), pRecord.Date_Nego);
                parameters.Add(new DataParameter(Cs, "PRODUIT", DbType.String), pRecord.Produit);
                parameters.Add(new DataParameter(Cs, "NOM_ECHE", DbType.String), pRecord.Nom_Eche);
                parameters.Add(new DataParameter(Cs, "SENS_OP", DbType.String), pRecord.Sens_Op);
                parameters.Add(new DataParameter(Cs, "INSTRT", DbType.String), pRecord.Instrt);
                parameters.Add(new DataParameter(Cs, "QUANT_OP", DbType.Int32), pRecord.Quant_Op);
                parameters.Add(new DataParameter(Cs, "COURS_OP", DbType.Double), pRecord.Cours_Op);
                parameters.Add(new DataParameter(Cs, "STRK_OP", DbType.Double), pRecord.Strk_Op);
                parameters.Add(new DataParameter(Cs, "REFER_OP", DbType.String), pRecord.Refer_Op);
                parameters.Add(new DataParameter(Cs, "GRP_CPTE", DbType.String), pRecord.Grp_Cpte);
                parameters.Add(new DataParameter(Cs, "NUM_COMPTE", DbType.String), pRecord.Num_Compte);
                parameters.Add(new DataParameter(Cs, "MARCHE", DbType.String), pRecord.Marche);
                parameters.Add(new DataParameter(Cs, "USER_DATA", DbType.String), pRecord.User_Data);
                parameters.Add(new DataParameter(Cs, "DOMICILIATION", DbType.String), domiciliation);
                parameters.Add(new DataParameter(Cs, "MONT_COURT_LOT", DbType.Double), pRecord.Mont_Court_Lot);
                parameters.Add(new DataParameter(Cs, "MONT_COURT_LG", DbType.Double), pRecord.Mont_Court_Lg);
                parameters.Add(new DataParameter(Cs, "TVA", DbType.Decimal), tva);
                parameters.Add(new DataParameter(Cs, "DEV_CRT_COM", DbType.String), devCrtCom);
                parameters.Add(new DataParameter(Cs, "RA_SOCIAL", DbType.String), raSocial);
                parameters.Add(new DataParameter(Cs, "TX_TVA_COURT", DbType.Decimal), txTvaCourt);
                parameters.Add(new DataParameter(Cs, "AGE_ENREG", DbType.DateTime), pRecord.Age_Enreg);
                parameters.Add(new DataParameter(Cs, "COURS_CLOP", DbType.Double), pRecord.Cours_Clop);
                parameters.Add(new DataParameter(Cs, "CPT_INTER", DbType.String), pRecord.Cpt_Inter);
                parameters.Add(new DataParameter(Cs, "ID_GN_OP_INT", DbType.String), pRecord.Id_Gn_Op_Int);
                parameters.Add(new DataParameter(Cs, "MONT_COURT_LOT_INT", DbType.Double), pRecord.Mont_Court_Lot_Int);
                parameters.Add(new DataParameter(Cs, "MONT_COURT_LG_INT", DbType.Double), pRecord.Mont_Court_Lg_Int);
                parameters.Add(new DataParameter(Cs, "TYP_X_DEPOUILLE", DbType.String), pRecord.Typ_X_Depouille);
                parameters.Add(new DataParameter(Cs, "NUM_ORDRE", DbType.String), pRecord.Num_Ordre);
                parameters.Add(new DataParameter(Cs, "TYP_COMPTE", DbType.String), pRecord.Typ_Compte);
                parameters.Add(new DataParameter(Cs, "DEV_PRODUIT", DbType.String), devProduit);
                parameters.Add(new DataParameter(Cs, "DTINS", DbType.DateTime), OTCmlHelper.GetDateSys(Cs));
                parameters.Add(new DataParameter(Cs, "IDAINS", DbType.Int32), UserId);
                //
                StrBuilder sqlInsert = new StrBuilder();
                sqlInsert += SQLCst.INSERT_INTO_DBO + "EXPORTBR_O" + Cst.CrLf;
                sqlInsert += "(VERSION,ACTIONTYPE,IDS_NEGO_AUDIT,DTINSS_NEGO_AUDIT,IDPROCESS_L,DTPROCESS_L,DTBUSINESS,EUROSYS_USER,HOSTNAME,DATE_NEGO,PRODUIT,NOM_ECHE,SENS_OP,INSTRT,QUANT_OP,COURS_OP,STRK_OP,REFER_OP,GRP_CPTE,NUM_COMPTE,MARCHE,USER_DATA,DOMICILIATION,MONT_COURT_LOT,MONT_COURT_LG,TVA,DEV_CRT_COM,RA_SOCIAL,TX_TVA_COURT,AGE_ENREG,COURS_CLOP,CPT_INTER,ID_GN_OP_INT,MONT_COURT_LOT_INT,MONT_COURT_LG_INT,TYP_X_DEPOUILLE,NUM_ORDRE,TYP_COMPTE,DEV_PRODUIT,DTINS,IDAINS) values " + Cst.CrLf;
                sqlInsert += @"(@VERSION,@ACTIONTYPE,@IDS_NEGO_AUDIT,@DTINSS_NEGO_AUDIT,@IDPROCESS_L,@DTPROCESS_L,@DTBUSINESS,@EUROSYS_USER,@HOSTNAME,@DATE_NEGO,@PRODUIT,@NOM_ECHE,@SENS_OP,@INSTRT,@QUANT_OP,@COURS_OP,@STRK_OP,@REFER_OP,@GRP_CPTE,@NUM_COMPTE,@MARCHE,@USER_DATA,@DOMICILIATION,@MONT_COURT_LOT,@MONT_COURT_LG,@TVA,@DEV_CRT_COM,@RA_SOCIAL,@TX_TVA_COURT,@AGE_ENREG,@COURS_CLOP,@CPT_INTER,@ID_GN_OP_INT,@MONT_COURT_LOT_INT,@MONT_COURT_LG_INT,@TYP_X_DEPOUILLE,@NUM_ORDRE,@TYP_COMPTE,@DEV_PRODUIT,@DTINS,@IDAINS)" + Cst.CrLf;
                // DataHelper.ExecuteNonQuery(Cs, CommandType.Text, sqlInsert.ToString(), parameters.GetArrayDbParameter());
                DataHelper.ExecuteNonQuery(DbTransaction, CommandType.Text, sqlInsert.ToString(), parameters.GetArrayDbParameter());
            }
            catch (Exception)
            {
                CodeReturn = Cst.ErrLevel.FAILURE;
                DataHelper.RollbackTran(DbTransaction);
                throw;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void UpdateIdProcessIntoAuditRecordsSet(string pReferOp, DateTime pClearingDate)
        {
            DateTime dtProcess_L = GetDtProcess_L();
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(Cs, "IDPROCESS_L", DbType.Int32), ProcessLogIdParam);
                parameters.Add(new DataParameter(Cs, "DTPROCESS_L", DbType.DateTime), dtProcess_L);
                parameters.Add(new DataParameter(Cs, "REFER_OP", DbType.String), pReferOp);
                parameters.Add(new DataParameter(Cs, "CLEARINGDATE", DbType.DateTime), pClearingDate);
                parameters.Add(new DataParameter(Cs, "DTUPD", DbType.DateTime), OTCmlHelper.GetDateSys(Cs));
                parameters.Add(new DataParameter(Cs, "IDAUPD", DbType.Int32), UserId);
                StrBuilder sqlUpdate = new StrBuilder();
                sqlUpdate += SQLCst.UPDATE_DBO + "S_NEGO_AUDIT" + Cst.CrLf;
                sqlUpdate += SQLCst.SET + "IDPROCESS_L = @IDPROCESS_L, DTPROCESS_L=@DTPROCESS_L, DTUPD=@DTUPD, IDAUPD=@IDAUPD" + Cst.CrLf;
                sqlUpdate += SQLCst.WHERE + "REFER_OP = @REFER_OP" + Cst.CrLf;
                sqlUpdate += SQLCst.AND + "DATE_TRAIT <= @CLEARINGDATE" + Cst.CrLf;
                sqlUpdate += SQLCst.AND + "IDPROCESS_L " + SQLCst.IS_NULL + Cst.CrLf;
                DataHelper.ExecuteNonQuery(DbTransaction, CommandType.Text, sqlUpdate.ToString(), parameters.GetArrayDbParameter());
            }
            catch (Exception)
            {
                CodeReturn = Cst.ErrLevel.FAILURE;
                DataHelper.RollbackTran(DbTransaction);
                throw;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdSNegoAudit"></param>
        /// <param name="pExportVersion"></param>
        private void UpdateVersionIntoAuditRecord(int pIdSNegoAudit, int pExportVersion)
        {
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(Cs, "IDS_NEGO_AUDIT", DbType.String), pIdSNegoAudit);
                parameters.Add(new DataParameter(Cs, "VERSION", DbType.String), pExportVersion);
                parameters.Add(new DataParameter(Cs, "DTUPD", DbType.DateTime), OTCmlHelper.GetDateSys(Cs));
                parameters.Add(new DataParameter(Cs, "IDAUPD", DbType.Int32), UserId);
                StrBuilder sqlUpdate = new StrBuilder();
                sqlUpdate += SQLCst.UPDATE_DBO + "S_NEGO_AUDIT" + Cst.CrLf;
                sqlUpdate += SQLCst.SET + "VERSION = @VERSION, DTUPD=@DTUPD, IDAUPD=@IDAUPD" + Cst.CrLf;
                sqlUpdate += SQLCst.WHERE + "IDS_NEGO_AUDIT = @IDS_NEGO_AUDIT" + Cst.CrLf;
                DataHelper.ExecuteNonQuery(DbTransaction, CommandType.Text, sqlUpdate.ToString(), parameters.GetArrayDbParameter());
            }
            catch (Exception)
            {
                CodeReturn = Cst.ErrLevel.FAILURE;
                DataHelper.RollbackTran(DbTransaction);
                throw;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pReferOp"></param>
        /// <param name="pCandidatToExportRecord"></param>
        /// <param name="pNextExportVersion"></param>
        /// <param name="pExportedRecordsSet"></param>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void HandleInsertSituation(string pReferOp, SNegoAudit pCandidateToExportRecord, List<ExportBr_O> pExportedRecordsSet)
        {
            int nextExportVersion = 1;

            if (pExportedRecordsSet.Count == 0)
            {
                WriteExportRecord(ActionType.Insert, nextExportVersion, pCandidateToExportRecord);
                UpdateIdProcessIntoAuditRecordsSet(pReferOp, EarlierClearingDateParam);
                UpdateVersionIntoAuditRecord(pCandidateToExportRecord.IdS_Nego_Audit, nextExportVersion);
            }
            if (pExportedRecordsSet.Count >= 1)
            {
                DateTime lastExportedDtIns = (from record in pExportedRecordsSet select record.DtIns).Max();
                ExportBr_O exportedLastRecord = new ExportBr_O();
                exportedLastRecord = FeedExportRecord(pExportedRecordsSet, lastExportedDtIns);
                ActionType lastExportedActionType = GetActionTypeEnum(exportedLastRecord.ActionType);
                nextExportVersion = SetVersion(exportedLastRecord.Version);
                switch (lastExportedActionType)
                {
                    // if previous action type in export record is insert or amend and data values are different
                    // we write a new record with ActionType=Amend
                    case ActionType.Insert:
                    case ActionType.Amend:
                        bool match = CompareData(pCandidateToExportRecord, exportedLastRecord);
                        //Different business values beetween internal and external data elements
                        if (match == false)
                        {
                            WriteExportRecord(ActionType.Amend, nextExportVersion, pCandidateToExportRecord);
                            UpdateIdProcessIntoAuditRecordsSet(pReferOp, EarlierClearingDateParam);
                            UpdateVersionIntoAuditRecord(pCandidateToExportRecord.IdS_Nego_Audit, nextExportVersion);
                        }
                        else
                        {
                            UpdateIdProcessIntoAuditRecordsSet(pReferOp, EarlierClearingDateParam);
                            // control
                            LogMsg = "No action is done. The last version of the trade is the same of the previous exported version. REFER_OP = [" + pReferOp + "]. No differences";
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Info, LogMsg));
                        }

                        break;
                    case ActionType.Cancel:
                        // FI 20200623 [XXXXX] SetErrorWarning
                        ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                        // control
                        LogMsg = "Business constraint violated. The trade has already been exported with an ActionType= 'Cancel' then no other export can be done. REFER_OP = [" + pReferOp + "].";
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, LogMsg));
                        
                        break;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pReferOp"></param>
        /// <param name="pCandidateToExportRecord"></param>
        /// <param name="pExportedRecordsSet"></param>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void HandleAmendSituation(string pReferOp, SNegoAudit pCandidateToExportRecord, List<ExportBr_O> pExportedRecordsSet)
        {
            int nextExportVersion = 1;

            if (pExportedRecordsSet.Count == 0)
            {
                // search auditId for the preceeding row using ActionType=Insert 
                int InsertActionAuditId = int.MinValue;
                InsertActionAuditId = (from audit in AuditRecordsSet
                                       where audit.Refer_Op == pReferOp && GetActionTypeEnum(audit.ActionType) == ActionType.Insert
                                       select audit.IdS_Nego_Audit).FirstOrDefault();

                if (InsertActionAuditId == int.MinValue)
                {
                    // FI 20200623 [XXXXX] SetErrorWarning
                    ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                    // control
                    LogMsg = "Business constraint violated. The last trade found into the audit table has an ActionType = 'Amended', but a corresponding (previous) trade with ActionType = 'Insert' does not exist. REFER_OP = [" + pReferOp + "].";
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Error, LogMsg));
                }
                else
                {
                    //SNegoAudit auditSecondLastRecord = FeedAuditRecord(AuditRecordsSet, pReferOp, InsertActionAuditId);
                    WriteExportRecord(ActionType.Insert, nextExportVersion, pCandidateToExportRecord);
                    UpdateIdProcessIntoAuditRecordsSet(pReferOp, EarlierClearingDateParam);
                    UpdateVersionIntoAuditRecord(pCandidateToExportRecord.IdS_Nego_Audit, nextExportVersion);

                }

            }
            if (pExportedRecordsSet.Count >= 1)
            {

                DateTime lastExportedDtIns = (from record in pExportedRecordsSet select record.DtIns).Max();
                ExportBr_O exportedLastRecord = new ExportBr_O();
                exportedLastRecord = FeedExportRecord(pExportedRecordsSet, lastExportedDtIns);
                ActionType lastExportedActionType = GetActionTypeEnum(exportedLastRecord.ActionType);
                nextExportVersion = SetVersion(exportedLastRecord.Version);

                switch (lastExportedActionType)
                {
                    case ActionType.Insert:
                    case ActionType.Amend:
                        bool match = CompareData(pCandidateToExportRecord, exportedLastRecord);
                        //Different business values beetween internal and external data elements
                        if (match == false)
                        {
                            WriteExportRecord(ActionType.Amend, nextExportVersion, pCandidateToExportRecord);
                            UpdateIdProcessIntoAuditRecordsSet(pReferOp, EarlierClearingDateParam);
                            UpdateVersionIntoAuditRecord(pCandidateToExportRecord.IdS_Nego_Audit, nextExportVersion);
                        }
                        else
                        {
                            UpdateIdProcessIntoAuditRecordsSet(pReferOp, EarlierClearingDateParam);
                            // control
                            LogMsg = "No action is done. The last version of the trade is the same of the previous exported version. REFER_OP = [" + pReferOp + "].";
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Info, LogMsg));
                        }
                        break;
                    case ActionType.Cancel:
                        // FI 20200623 [XXXXX] SetErrorWarning
                        ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                        // control
                        LogMsg = "Business constraint violated. The trade has already been exported with an ActionType= 'Cancel' then no other export can be done. REFER_OP = [" + pReferOp + "].";

                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, LogMsg));

                        break;
                }

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pReferOp"></param>
        /// <param name="pCandidateToExportRecord"></param>
        /// <param name="pExportedRecordsSet"></param>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void HandleCancelSituation(string pReferOp, SNegoAudit pCandidateToExportRecord, List<ExportBr_O> pExportedRecordsSet)
        {
            int nextExportVersion = 1;

            if (pExportedRecordsSet.Count == 0)
            {
                UpdateIdProcessIntoAuditRecordsSet(pReferOp, EarlierClearingDateParam);
                LogMsg = "No action is done. The trade has been deleted (ActionType = 'Cancel') but it has never been exported. REFER_OP = [" + pReferOp + "]";
                
                Logger.Log(new LoggerData(LogLevelEnum.Info, LogMsg));
            }
            if (pExportedRecordsSet.Count >= 1)
            {
                DateTime lastExportedDtIns = (from record in pExportedRecordsSet select record.DtIns).Max();
                ExportBr_O exportedLastRecord = new ExportBr_O();
                exportedLastRecord = FeedExportRecord(pExportedRecordsSet, lastExportedDtIns);
                ActionType lastExportedActionType = GetActionTypeEnum(exportedLastRecord.ActionType);
                nextExportVersion = SetVersion(exportedLastRecord.Version);

                switch (lastExportedActionType)
                {
                    case ActionType.Insert:
                    case ActionType.Amend:
                        WriteExportRecord(ActionType.Cancel, nextExportVersion, pCandidateToExportRecord);
                        UpdateIdProcessIntoAuditRecordsSet(pReferOp, EarlierClearingDateParam);
                        UpdateVersionIntoAuditRecord(pCandidateToExportRecord.IdS_Nego_Audit, nextExportVersion);
                        break;
                    case ActionType.Cancel:
                        // control
                        // FI 20200623 [XXXXX] SetErrorWarning
                        ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                        LogMsg = "Business constraint violated. The trade has already been exported with an ActionType= 'Cancel' then no other export can be done. REFER_OP = [" + pReferOp + "].";
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, LogMsg));

                        break;
                }

            }
        }
        /// <summary>
        /// Main method
        /// </summary>
        /// FI 20170215 [XXXXX] Modify
        // EG 20190114 Add detail to ProcessLog Refactoring
        public Cst.ErrLevel TradesAuditProcess()
        {
            CodeReturn = Cst.ErrLevel.SUCCESS;

            //English is default culture
            //FI 20170215 [XXXXX] Appel ThreadTools.SetCurrentCulture
            //SystemTools.SetCurrentCulture(Cst.EnglishCulture);
            ThreadTools.SetCurrentCulture(Cst.EnglishCulture);

            // enhances objects with input datas from task parameters
            GetParameters();

            // GS 20120622: ticket 11088 change request
            // Remove control: checks that the treatments has been performed for the input clearing date 
            // bool isTreatedDay = IsTreatedDay(EarlierClearingDate);
            // if clearing date have been treated: start the process
            // if (isTreatedDay == true)

            DateTime lastTreatedDate = GetLastTreatedDate(MarketType);

            if ( EarlierClearingDateParam > lastTreatedDate )
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                LogMsg = "No End-Of-Day process is found for the clearing date " + String.Format("{0:MMMM dd, yyyy}", EarlierClearingDateParam);
                
                Logger.Log(new LoggerData(LogLevelEnum.Warning, LogMsg));
            }    
            
            // control: check if exists duplicate trades
            bool isDuplicateTrades = FindDuplicateTrades(lastTreatedDate);
            // if not exists duplicate trades: start the process
            if (isDuplicateTrades == false)
            {  
                // creating a list to containing the audit data records set
                AuditRecordsSet = FeedAuditRecordsSet(EarlierClearingDateParam, lastTreatedDate);
                // when audit recordset is empty no action is required 
                if (AuditRecordsSet.Count == 0)
                {
                    CodeReturn = Cst.ErrLevel.FAILURE;
                    // FI 20200623 [XXXXX] SetErrorWarning
                    ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                    
                    LogMsg = "No candidate trades found : no trade has been inserted/updated/deleted since last exportation.";
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Error, LogMsg));
                }

                // there are exportable candidate trades
                else
                {
                    LogMsg = "Begin process " + TaskIdentifierParam;
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Info, LogMsg));

                    // create object with distinct candidate trades 
                    IEnumerable<string> distinctReferOp = AuditRecordsSet.Select(elem => elem.Refer_Op).Distinct();
                    //
                    // Begin transaction
                    DbTransaction = DataHelper.BeginTran(Cs);

                    //int count = 0;

                    // cycle for each refer_op
                    foreach (string referOp in distinctReferOp)
                    {

                        // get row into S_NEGO_AUDIT table with most recent insert date
                        int lastAuditId = (from audit in AuditRecordsSet where audit.Refer_Op == referOp select audit.IdS_Nego_Audit).Max();
                        // returns audit record candidate export
                        SNegoAudit lastAuditRecordCandidateToExport = new SNegoAudit();
                        lastAuditRecordCandidateToExport = FeedAuditRecord(referOp, lastAuditId);

                        // creating a list to containing the previously exported records set for a specific refer_op
                        List<ExportBr_O> exportedRecordsSet = new List<ExportBr_O>();
                        exportedRecordsSet = FeedExportedRecordsSet(referOp);
                        // returns action type from  lastAuditRecordCandidateToExport 
                        ActionType actionType = GetActionTypeEnum(lastAuditRecordCandidateToExport.ActionType);

                        // call specific methods depending on last audit record action type 
                        switch (actionType)
                        {
                            case ActionType.Insert:
                                HandleInsertSituation(referOp, lastAuditRecordCandidateToExport, exportedRecordsSet);
                                break;
                            case ActionType.Amend:
                                HandleAmendSituation(referOp, lastAuditRecordCandidateToExport, exportedRecordsSet);
                                break;
                            case ActionType.Cancel:
                                HandleCancelSituation(referOp, lastAuditRecordCandidateToExport, exportedRecordsSet);
                                break;
                            default:
                                // control
                                CodeReturn = Cst.ErrLevel.FAILURE;
                                LogMsg = "Action type unknown [" + actionType + "].";

                                // FI 20200623 [XXXXX] SetErrorWarning
                                ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                                
                                Logger.Log(new LoggerData(LogLevelEnum.Error, LogMsg));

                                break;
                        }
                    }
                    // End tran (commit/rollback)
                    if (null != DbTransaction)
                    {
                        if (Cst.ErrLevel.SUCCESS == CodeReturn)
                            DataHelper.CommitTran(DbTransaction);
                        DbTransaction.Dispose();
                    }
                    LogMsg = "End process " + TaskIdentifierParam;
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Info, LogMsg));
                }
            }
            // There are duplicate trades: stop the process
            else
            {
                
                CodeReturn = Cst.ErrLevel.FAILURE;

                // FI 20200623 [XXXXX] SetErrorWarning
                ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                LogMsg = "Business constraint violated : several trades (S_NEGO) have the same REFER_OP. Clearing date : " + String.Format("{0:MMMM dd, yyyy}", EarlierClearingDateParam) + ". " + "The process is stopped.";
                
                
                Logger.Log(new LoggerData(LogLevelEnum.Error, LogMsg));
            }

            return CodeReturn;
        }
    }
}

#region Deprecated methods

//MF 20120320
// IEnumerable<string> codesReferOp2 = (from audit in AuditCollectionList select audit.Refer_op).Distinct();
//CandidateTradesList = (from audit in AuditCollectionList
//                       join code in
//                           (from audit in AuditCollectionList select audit.Refer_op).Distinct()
//                       on audit.Refer_op equals code                  
//                       select                         
//                          new CandidateTrades
//                          {
//                              Refer_op = audit.Refer_op,
//                          }
//                          )
//                          .ToList();


//AuditLastRow = (from audit in AuditCollection
//                where audit.Refer_Op == referOp && audit.DtIns.Value == lastDtIns
//                select audit).ToList();
//string actionType = (from row in AuditLastRow select row.ActionType).First();


// GS 20120330 deprecated method
// call/invoke methods (actions) based on different Audit action types
//private void HandleExportableRecord(string pReferOp, ActionType pAuditLastActionType)
//{           
//    // returns audit record candidate export
//    AuditRecordCandidateToExport = AuditLastRecord;
//    // creating a list to containing the previously exported records set for a specific refer_op
//    List<ExportBr_O> exportedRecordsSet = new List<ExportBr_O>();
//    exportedRecordsSet = FeedExportedRecordsSet(pReferOp);
//    //none rows are availables in export records set
//    if (exportedRecordsSet.Count == 0)
//    {
//        NextExportVersion = SetVersion(ExportLastRecord.Version);
//        switch (pAuditLastActionType)
//        {
//            //******************************************************
//            // Case insert action type into audit record
//            //******************************************************
//            case ActionType.Insert:
//                WriteExportRecord(ActionType.Insert, NextExportVersion, AuditRecordCandidateToExport);
//                UpdateAuditRecord(AuditRecordCandidateToExport.IdS_Nego_Audit, NextExportVersion);
//                break;
//            //******************************************************
//            // Case amend action type into audit record
//            //******************************************************
//            case ActionType.Amend:

//                // we look first row using insert action into audit records  
//                DateTime secondLastDtIns = (from audit in AuditRecordsSet
//                                            where audit.Refer_Op == pReferOp && GetActionTypeEnum(audit.ActionType) == ActionType.Insert
//                                            select audit.DtIns).FirstOrDefault();

//                if (secondLastDtIns == DateTime.MinValue)
//                {
//                    // control
//                    LogMsg = "Business constraint violated. The last trade found into the audit table has an ActionType = 'Amended', but a corresponding (previous) trade with ActionType = 'Insert' does not exist. REFER_OP = [" + pReferOp + "].";
//                    WriteIntoProcessL(LevelStatusTools.StatusEnum.SUCCESSWITHMESSAGE, LogMsg);
//                }
//                else
//                {
//                    SNegoAudit auditSecondLastRecord = FeedAuditRecord(AuditRecordsSet, pReferOp, secondLastDtIns);
//                    bool match = CompareData(auditSecondLastRecord, ExportLastRecord);
//                    //Different business values beetween internal and external data elements
//                    if (match == false)
//                    {
//                        WriteExportRecord(ActionType.Insert, NextExportVersion, AuditRecordCandidateToExport);
//                        UpdateAuditRecord(AuditRecordCandidateToExport.IdS_Nego_Audit, NextExportVersion);
//                    }
//                    else
//                    {
//                        // control
//                        LogMsg = "No action is done. The last version of the trade is the same of the previous exported version. REFER_OP = [" + pReferOp + "]. No differences";
//                        WriteIntoProcessL(LevelStatusTools.StatusEnum.INFO, LogMsg);
//                    }
//                }

//                break;
//            //******************************************************
//            // Case cancel action type into audit record
//            //******************************************************
//            case ActionType.Cancel:
//                // control
//                LogMsg = "No action is done. The trade has been deleted (ActionType = 'Cancel') but it has never been exported. REFER_OP = [" + pReferOp + "]";
//                WriteIntoProcessL(LevelStatusTools.StatusEnum.INFO, LogMsg);
//                break;
//        }
//    }

//    if (exportedRecordsSet.Count >= 1)
//    {
//        DateTime lastExportedDtIns = (from record in exportedRecordsSet select record.DtIns).Max();
//        ExportLastRecord = FeedExportRecord(exportedRecordsSet, lastExportedDtIns);
//        ActionType lastExportActionType = GetActionTypeEnum(ExportLastRecord.ActionType);
//        /// returns new version depending on last exported version
//        NextExportVersion = SetVersion(ExportLastRecord.Version);
//        //******************************************************
//        // Case insert action type into audit record
//        //******************************************************
//        // handle last audit record depending on action type
//        switch (pAuditLastActionType)
//        {
//            case ActionType.Insert:
//                // leggo precedente azione su EXPORTBR_O
//                switch (ExportLastActionType)
//                {
//                    // if previous action type in export record is insert or amend and data values are different
//                    // we write a new record with ActionType=Amend
//                    case ActionType.Insert:
//                    case ActionType.Amend:
//                        bool match = CompareData(AuditLastRecord, ExportLastRecord);
//                        //Different business values beetween internal and external data elements
//                        if (match == false)
//                        {
//                            WriteExportRecord(ActionType.Amend, NextExportVersion, AuditRecordCandidateToExport);
//                            UpdateAuditRecord(AuditRecordCandidateToExport.IdS_Nego_Audit, NextExportVersion);
//                        }
//                        else
//                        {
//                            // control
//                            LogMsg = "No action is done. The last version of the trade is the same of the previous exported version. REFER_OP = [" + pReferOp + "]. No differences";
//                            WriteIntoProcessL(LevelStatusTools.StatusEnum.INFO, LogMsg);
//                        }

//                        break;
//                    case ActionType.Cancel:
//                        // control
//                        LogMsg = "Business constraint violated. The trade has already been exported has an ActionType= 'Cancel' then no other export can be done. REFER_OP = [" + pReferOp + "].";
//                        WriteIntoProcessL(LevelStatusTools.StatusEnum.SUCCESSWITHMESSAGE, LogMsg);
//                        break;
//                }
//                break;
//            //******************************************************
//            // Case amend action type into audit record
//            //******************************************************
//            case ActionType.Amend:
//                switch (ExportLastActionType)
//                {
//                    case ActionType.Insert:
//                    case ActionType.Amend:
//                        bool match = CompareData(AuditLastRecord, ExportLastRecord);
//                        //Different business values beetween internal and external data elements
//                        if (match == false)
//                        {
//                            WriteExportRecord(ActionType.Amend, NextExportVersion, AuditRecordCandidateToExport);
//                            UpdateAuditRecord(AuditRecordCandidateToExport.IdS_Nego_Audit, NextExportVersion);
//                        }
//                        else
//                        {
//                            // control
//                            LogMsg = "No action is done. The last version of the trade is the same of the previous exported version. REFER_OP = [" + pReferOp + "]. No differences";
//                            WriteIntoProcessL(LevelStatusTools.StatusEnum.INFO, LogMsg);
//                        }
//                        break;
//                    case ActionType.Cancel:
//                        // control
//                        LogMsg = "Business constraint violated. The trade has already been exported has an ActionType= 'Cancel' then no other export can be done. REFER_OP = [" + pReferOp + "].";
//                        WriteIntoProcessL(LevelStatusTools.StatusEnum.SUCCESSWITHMESSAGE, LogMsg);
//                        break;
//                }
//                break;
//            //******************************************************
//            // Case cancel action type into audit record
//            //******************************************************
//            case ActionType.Cancel:
//                switch (ExportLastActionType)
//                {
//                    case ActionType.Insert:
//                    case ActionType.Amend:
//                        WriteExportRecord(ActionType.Cancel, NextExportVersion, AuditRecordCandidateToExport);
//                        UpdateAuditRecord(AuditRecordCandidateToExport.IdS_Nego_Audit, NextExportVersion);
//                        break;
//                    case ActionType.Cancel:
//                        // control
//                        LogMsg = "Business constraint violated. The trade has already been exported has an ActionType= 'Cancel' then no other export can be done. REFER_OP = [" + pReferOp + "].";
//                        WriteIntoProcessL(LevelStatusTools.StatusEnum.SUCCESSWITHMESSAGE, LogMsg);    
//                        break;
//                }
//                break;
//        }
//    }
//}

#endregion

