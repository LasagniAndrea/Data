#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Serialization;
// EG 20160404 Migration vs2013
using System.Security.Cryptography;
using System.Web.UI.WebControls;
using System.Diagnostics;


#endregion Using Directives
// EG 20121015 TRACKER COMPARE AND UPDATE WITH LAST RD VERSION 
namespace EFS.ACommon
{
    #region Enum
    public enum TestEnum
    {
        PL, CC
    }
    public enum CompareEnum
    {
        Normal,
        Upper,
        Lower
    }
    /// <summary>
    /// 
    /// </summary>
    /// FI 20150915 [21315] Add
    public enum OccurenceEnum
    {
        First,
        Last,
    }
    #endregion


    /// <summary>
    /// Constant
    /// </summary>
    public sealed class Cst
    {
        public Cst()
        { }

        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public enum CSSModeEnum
        {
            vlight,
            vdark
        }
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public enum CSSColorEnum
        {
            blue,
            cyan,
            gray,
            green,
            marron,
            orange,
            red,
            rose,
            violet,
            yellow
        }

        //PL/FI 20170504 Add IsDisabledRestrictOnEntity 
        public const bool IsDisabledRestrictOnEntity = true;

        // MF 20120530 Ticket 17811
        public static Regex Regex_CodeNumber = new Regex(@"^([A-Z]{3})\-(\d{5})(.*)$",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);

        #region public enum CommandTypeEnum
        public enum CommandTypeEnum
        {
            SQLStoredProcedure,
            SQLText,
            SQLTrigger,
            XSLFile
        };
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// FI 20220601 [XXXXX] Add
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
        public class ETDMaturityInputFormatAttribute : ResourceAttribut
        {
            /// <summary>
            /// True si lorsque format s'applique uniquement aux échéances mensuelles
            /// </summary>
            public Boolean IsForMonthOnly
            {
                get;
                set;
            }
        }



        /// <summary>
        /// Représente les formats d'affichage et de saisie des échéances ETD 
        /// <para>paramétrable sur le profil utilisateur</para>
        /// </summary>
        /// FI 20171025 [23533] Modify DefaultValue et ResourceAttribut
        [DefaultValue(FIX)]
        public enum ETDMaturityInputFormatEnum
        {
            /// <summary>
            /// Représente le Format Fix
            /// <para>YYYYMM, YYYYMMDD, YYYYMMwN</para>
            /// <para></para>
            /// </summary>
            [ETDMaturityInputFormatAttribute(IsForMonthOnly = false, Resource = "ETDMatFmt_FIX")]
            FIX,
            /// <summary>
            /// MMM YY
            /// <para>MMM => The abbreviated name of the month</para>
            /// </summary>
            [ETDMaturityInputFormatAttribute(IsForMonthOnly = true, Resource = "ETDMatFmt_MMMspaceYY")]
            MMMspaceYY,
            /// <summary>
            /// MY
            /// <para>M => valeurs possibles : FGHJKMNQUVXZ</para>
            /// </summary>
            [ETDMaturityInputFormatAttribute(IsForMonthOnly = true, Resource = "ETDMatFmt_MY")]
            MY,
            /// <summary>
            /// 
            /// </summary>
            [ETDMaturityInputFormatAttribute(IsForMonthOnly = true, Resource = "ETDMatFmt_MMseparatorYYYY")]
            MMseparatorYYYY,
            /// <summary>
            /// 
            ///</summary>
            [ETDMaturityInputFormatAttribute(IsForMonthOnly = true, Resource = "ETDMatFmt_YYYYseparatorMM")]
            YYYYseparatorMM
        }

        /// <summary>
        /// Représente le fuseau horaire d'affichage des horodatages de trading 
        /// <para>paramétrable sur le profil utilisateur</para>
        /// </summary>
        /// FI 20171025 [23533] Add
        [DefaultValue(Facility)]
        public enum TradingTimestampZone
        {
            /// <summary>
            /// Affichage selon le fuseau horaire de la plateforme
            /// </summary>
            [ResourceAttribut(Resource = "TimestampZone_Facility")]
            Facility,
            /// <summary>
            /// Affichage selon le fuseau horaire UTC
            /// </summary>
            [ResourceAttribut(Resource = "TimestampZone_UTC")]
            UTC,
            /// <summary>
            /// Affichage selon le fuseau horaire du user
            /// </summary>
            [ResourceAttribut(Resource = "TimestampZone_User")]
            User,
            /// <summary>
            /// Affichage selon le fuseau horaire du department
            /// </summary>
            [ResourceAttribut(Resource = "TimestampZone_Department")]
            Department,
            /// <summary>
            /// Affichage selon le fuseau horaire de l'entité
            /// </summary>
            [ResourceAttribut(Resource = "TimestampZone_Entity")]
            Entity
        }

        /// <summary>
        /// Représente précision d'affichage des horodatages de trading 
        /// <para>paramétrable sur le profil utilisateur</para>
        /// </summary>
        /// FI 20171025 [23533] Add
        [DefaultValue(Second)]
        public enum TradingTimestampPrecision
        {
            /// <summary>
            /// précision en minute
            /// </summary>
            [ResourceAttribut(Resource = "TimestampPrecision_Minute")]
            Minute,
            /// <summary>
            /// précision en seconde
            /// </summary>
            [ResourceAttribut(Resource = "TimestampPrecision_Second")]
            Second,
            /// <summary>
            /// précision en millisecondes
            /// </summary>
            [ResourceAttribut(Resource = "TimestampPrecision_Millisecond")]
            Millisecond,
            /// <summary>
            /// précision en millisecondes
            /// </summary>
            [ResourceAttribut(Resource = "TimestampPrecision_Microsecond")]
            Microsecond,

        }

        /// <summary>
        /// Représente le fuseau horaire d'affichage des horodatages de livraison 
        /// </summary>
        /// FI 20221207 [XXXXX] Add
        [DefaultValue(Delivery)]
        public enum DeliveryTimestampZone
        {
            /// <summary>
            /// Affichage selon le fuseau horaire du lieu de livraison
            /// </summary>
            [ResourceAttribut(Resource = "TimestampZone_Delivery")]
            Delivery,
            /// <summary>
            /// Affichage selon le fuseau horaire UTC
            /// </summary>
            [ResourceAttribut(Resource = "TimestampZone_UTC")]
            UTC,
            /// <summary>
            /// Affichage selon le fuseau horaire du user
            /// </summary>
            [ResourceAttribut(Resource = "TimestampZone_User")]
            User,
            /// <summary>
            /// Affichage selon le fuseau horaire du department
            /// </summary>
            [ResourceAttribut(Resource = "TimestampZone_Department")]
            Department,
            /// <summary>
            /// Affichage selon le fuseau horaire de l'entité
            /// </summary>
            [ResourceAttribut(Resource = "TimestampZone_Entity")]
            Entity
        }


        /// <summary>
        /// Représente précision d'affichage des horodatages de livraison 
        /// </summary>
        /// FI 20221207 [XXXXX] Add
        [DefaultValue(Minute)]
        public enum DeliveryTimestampPrecision
        {
            /// <summary>
            /// précision en minute
            /// </summary>
            [ResourceAttribut(Resource = "TimestampPrecision_Minute")]
            Minute,
            /// <summary>
            /// précision en seconde
            /// </summary>
            [ResourceAttribut(Resource = "TimestampPrecision_Second")]
            Second,
        }

        /// <summary>
        /// Représente le Timezone d'affichage des horodatages d'audit (Log, tracker) 
        /// <para>paramétrable sur le profil utilisateur</para>
        /// </summary>
        /// FI 20171025 [23533] Add
        [DefaultValue(Entity)]
        public enum AuditTimestampZone
        {
            /// <summary>
            /// Affichage selon le fuseau horaire UTC
            /// </summary>
            [ResourceAttribut(Resource = "TimestampZone_UTC")]
            UTC,
            /// <summary>
            /// Affichage selon le fuseau horaire du user
            /// </summary>
            [ResourceAttribut(Resource = "TimestampZone_User")]
            User,
            /// <summary>
            /// Affichage selon le fuseau horaire du department
            /// </summary>
            [ResourceAttribut(Resource = "TimestampZone_Department")]
            Department,
            /// <summary>
            /// Affichage selon le fuseau horaire de l'entité
            /// </summary>
            [ResourceAttribut(Resource = "TimestampZone_Entity")]
            Entity
        }

        /// <summary>
        /// Représente la précision d'affichage des horodatages d'audit (Log, tracker) 
        /// <para>paramétrable sur le profil utilisateur</para>
        /// </summary>
        /// FI 20171025 [23533] Add 
        [DefaultValue(Second)]
        public enum AuditTimestampPrecision
        {
            /// <summary>
            /// précision en minute
            /// </summary>
            [ResourceAttribut(Resource = "TimestampPrecision_Minute")]
            Minute,
            /// <summary>
            /// précision en seconde
            /// </summary>
            [ResourceAttribut(Resource = "TimestampPrecision_Second")]
            Second,
            /// <summary>
            /// précision en millisecondes
            /// </summary>
            [ResourceAttribut(Resource = "TimestampPrecision_Millisecond")]
            Millisecond,
            /// <summary>
            /// Utilisé pour le tracker Format date sur lignes hors journée en cours
            /// </summary>
            DDMM,

        }

        /// <summary>
        /// Pilote l'affichage des données 
        /// </summary>
        /// FI 20171025 [23533] Add
        public enum DataTypeDisplayMode
        {
            /// <summary>
            /// Affichage d'un horodatage selon les règles "horodatages de trading" défini sur le profil
            /// </summary>
            Trading,
            /// <summary>
            /// Affichage d'un horodatage selon les règles "horodatages d'audit" défini sur le profil
            /// </summary>
            Audit,
            /// <summary>
            /// Affichage d'un horodatage selon les règles "horodatages des livraisons" défini sur le profil
            /// </summary>
            Delivery,
            /// <summary>
            ///  Affiche d'une échéance selon le format  MMM YY (Réservé à usage ultérieur)
            /// </summary>
            MatFmt_MMMspaceYY,
            /// <summary>
            ///  Affiche d'une échéance selon le format MY (Réservé à usage ultérieur)
            /// </summary>
            MatFmt_MY,
            /// <summary>
            /// Affiche d'une échéance selon les règles "Format d'affichage des échéances sur les Dérivés Listés" sur le profil (Réservé à usage ultérieur)
            /// </summary>
            MatFmt_Profil
        }

        /// <summary>
        /// Donne des informations supplémentaires sur le contenu de la colonne afin d'appliquer des affichages spécifiques  
        /// </summary>
        /// FI 20171025 [23533] Add
        public enum DataKind
        {
            /// <summary>
            /// Spécifie que la donnée datetime représente un horodatage 
            /// </summary>
            Timestamp,
            /// <summary>
            /// Spécifie que la donnée string représente une échéance d'un contrat ETD
            /// </summary>
            ETDMaturity,
            /// <summary>
            /// Spécifie que la donnée decimale représente une quantité
            /// <para>L'affichage est généralement de type integer (sans décimale) ou fonction des proprités du contrat CommoditySpot</para>
            /// </summary>
            Qty,
            /// <summary>
            /// Specifies that the integer data represents a number of seconds            
            /// </summary>
            /// AL 20240703 [WI605] Datakind Seconds for integer type
            Seconds,
        }

        #region public enum DBStatusEnum
        public enum DBStatusEnum
        {
            NA, ONLINE, OFFLINE, CORRUPTED, UPGRADE, POSTUPGRADE, POSTCORRUPTED

        }
        #endregion public enum DBStatusEnum

        #region public enum WindowOpenStyle
        /// <summary>
        /// 
        /// </summary>
        /// FI 20161124 [22634] Modify
        /// EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
        public enum WindowOpenStyle
        {
            _blank, _help, _media,
            FpML_Help, EfsML_Help, EfsML_Main, EfsML_FormReferential, EfsML_ListReferential, EfsML_Status
        }
        #endregion public enum WindowOpenStyle

        /// <summary>
        /// 
        /// </summary>
        /// FI 20200922 [XXXXX] Add
        public enum HyperLinkTargetEnum
        {
            /// <summary>
            ///  Opens the linked document in a new window or tab (Restitue le contenu dans une nouvelle fenêtre sans frame)
            /// </summary>
            _blank,
            /// <summary>
            /// Opens the linked document in the same frame as it was clicked(this is default) (Restitue le contenu dans le frame ayant le focus)
            /// </summary>
            _self,
            /// <summary>
            /// Opens the linked document in the parent frame (Restitue le contenu dans le parent du jeu de frames immédiat)
            /// </summary>
            _parent,
            /// <summary>
            /// Opens the linked document in the full body of the window (_top  Restitue le contenu dans la fenêtre entière sans frame)
            /// </summary>
            _top
        }


        #region public enum ConnectionState
        /// <summary>
        ///  Représente les états de connexion de la session
        /// </summary>
        /// EG 20210614 [25290] Introduction d'un nouvel état de connexion 'LOGINREQUEST' indiquant qu'une demande de connexion est en cours
        public enum ConnectionState
        {
            /// <summary>
            /// La session n'a pas encore tenté une connexion à Spheres®
            /// </summary>
            INIT,
            /// <summary>
            ///  La session est connectée à Spheres® 
            /// </summary>
            LOGIN,
            /// <summary>
            ///  Demande de connexion en cours
            /// </summary>
            LOGINREQUEST,
            /// <summary>
            ///  La session est déconnectée de Spheres®
            /// </summary>
            LOGOUT,
            /// <summary>
            /// La dernière tentative de connexion à Spheres n'a pas aboutie 
            /// </summary>
            FAIL,
            /// <summary>
            /// La dernière tentative de connexion à Spheres n'a pas aboutie 
            /// </summary>
            /// EG 20240524 [WI941][26663] Security : Account blocking - Captcha page
            CAPTCHA
        };
        #endregion
        #region public enum ActionOnConnect
        /// RD 20220322 [25958] Introduction d'une nouvelle action 'PWDCHANGING' indiquant qu'un changement de mot de passe est en cours
        /// EG 20220123 [26235][WI543] Refactoring de la gestion des actions sur le PASSWORD liée aux règles de sécurité de l'acteur en cours de connexion
        public enum ActionOnConnect
        {
            NONE,
            /// <summary>
            /// signifie que le user doit changer de mot de passe à la connexion
            /// </summary>
            CHANGEPWD,
            /// <summary>
            /// signifie que le mot de passe va bientôt expirer et le user a le choix de le changer
            /// </summary>
            EXPIREDPWD,
            /// <summary>
            /// signifie qu'une action de changement de mot de passe est en cours
            /// </summary>
            /// EG 20220123 [26235][WI543] Remove
            //PWDCHANGING,
            /// <summary>
            /// signifie qu'une action de changement de mot de passe a été validée
            /// </summary>
            /// EG 20220123 [26235][WI543] New
            VALIDPWDCHANGING,
            /// <summary>
            /// signifie qu'une action de changement de mot de passe a été reportée à plus tard
            /// </summary>
            /// EG 20220123 [26235][WI543] New
            POSTPONEPWDCHANGING,
        };
        #endregion

        #region  public class StatusTrigger
        public class StatusTrigger
        {
            public enum StatusTriggerEnum
            {
                NA, NONE, ACTIV, DEACTIV
            }

            public static bool IsStatusDeclared(string pStatus)
            {
                return IsStatusActivated(pStatus) || IsStatusDeActivated(pStatus);
            }

            public static bool IsStatusActivated(string pStatus)
            {
                return (StatusTriggerEnum.ACTIV.ToString() == pStatus);
            }

            public static bool IsStatusDeActivated(string pStatus)
            {
                return (StatusTriggerEnum.DEACTIV.ToString() == pStatus);
            }
        }
        #endregion  public class

        #region public class MOM
        public class MOM
        {
            public const string MOMType = "MOMType";
            public const string MOMPath = "MOMPath";
            public const string MOMEncrypt = "MOMEncrypt";
            public const string MOMRecoverable = "MOMRecoverable";

            /// <summary>
            /// Système de messagerie MOM
            /// </summary>
            public enum MOMEnum
            {
                FileWatcher,
                MSMQ,
                MQSeries,
                Unknown,
            }


            /// <summary>
            /// Retourne la valeur Enum correspondant à {pMOMType}
            /// <para>Retourne MOMEnum.Unknown si pMOMType ne correspond à aucune valeur de l'enum</para>
            /// </summary>
            /// <param name="pMOMType"></param>
            /// <returns></returns>
            public static MOMEnum GetMOMEnum(string pMOMType)
            {
                MOMEnum ret = MOMEnum.Unknown;
                if (StrFunc.IsFilled(pMOMType))
                {
                    if (Enum.IsDefined(typeof(MOMEnum), pMOMType))
                    {
                        ret = (MOMEnum)Enum.Parse(typeof(MOMEnum), pMOMType);
                    }
                }
                return ret;
            }


            /// <summary>
            /// 
            /// </summary>
            /// /// <param name="pMOMType"></param>
            /// <returns></returns>
            public static bool IsFileWatcher(MOMEnum pMOMType)
            {
                return (MOMEnum.FileWatcher == pMOMType);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="pMOMType"></param>
            /// <returns></returns>
            public static bool IsFileWatcher(string pMOMType)
            {
                return IsFileWatcher(GetMOMEnum(pMOMType));
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pMOMType"></param>
            /// <returns></returns>
            public static bool IsMsMQueue(MOMEnum pMOMType)
            {
                return (MOMEnum.MSMQ == pMOMType);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="pMOMType"></param>
            /// <returns></returns>
            public static bool IsMsMQueue(string pMOMType)
            {
                return IsMsMQueue(GetMOMEnum(pMOMType));
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pMOMType"></param>
            /// <returns></returns>
            public static bool IsMQSeries(MOMEnum pMOMType)
            {
                return (MOMEnum.MQSeries == pMOMType);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="pMOMType"></param>
            /// <returns></returns>
            public static bool IsMQSeries(string pMOMType)
            {
                return IsMQSeries(GetMOMEnum(pMOMType));
            }
        }
        #endregion

        #region public class Capture
        public class Capture
        {
            #region ToolbarList
            public enum ToolbarList
            {
                Instrument,
                Screen,
                Template,
            }
            #endregion ToolbarList
            #region public MenuEnum
            /// <summary>
            /// 
            /// </summary>
            public enum MenuEnum
            {
                Template = 0,
                Consult = 1,
                Mode = 2,
                Action = 3,
                Screen = 4,
                ResultAction = 5,
                Report = 6
            }
            #endregion
            #region public MenuConsultEnum
            public enum MenuConsultEnum // différent Menu de chargement de trade
            {
                /// <summary>
                ///Chargement d'un template pour une création 
                /// </summary>
                SetTemplate = 0,
                /// <summary>
                ///Chargement d'un trade (regular, template, ...) en consultation 
                /// </summary>
                LoadTrade = 1,
                //
                FirstTrade = 2,
                PreviousTrade = 3,
                NextTrade = 4,
                EndTrade = 5,
                //
                /// <summary>
                ///Chargement d'un template suite à selection d'un instrument
                /// </summary>
                SetProduct = 6,
                /// <summary>
                ///Chargement d'un trade  suite à selection d'un Ecran 
                /// </summary>
                SetScreen = 7,
                /// <summary>
                ///Chargement d'un trade avant de passer en modif 
                /// </summary>
                GoUpdate = 8,
                /// <summary>
                ///Chargement d'un évènement 
                /// </summary>
                LoadEvent = 9,
                FirstEvent = 10,
                PreviousEvent = 11,
                NextEvent = 12,
                EndEvent = 13,
                /// <summary>
                ///Chargement d'un évènement après First/Previous... 
                /// </summary>
                GoEvent = 14,  //==> 

            }
            #endregion
            /// <summary>
            /// 
            /// </summary>
            public enum MenuValidateEnum
            {
                Close = 0,
                Record,
                Annul
            }

            /// <summary>
            /// 
            /// </summary>
            public enum TypeEnum
            {
                Customised = 0,
                Full = 1
            }


            /// <summary>
            /// Type d'écran
            /// </summary>
            /// FI 20171025 [23533] Modify (add ResourceAttribut)
            public enum GUIType
            {
                [ResourceAttribut(Resource = "NA")]
                NA,
                /// <summary>
                /// Représente un écran Quick
                /// </summary>
                Quick,
                /// <summary>
                /// Représente un écran Light
                /// </summary>
                Light,
                /// <summary>
                /// Représente un écran amélioré (Ecran par exemple modifié pour des besoin spécifiques )
                /// </summary>
                Enhanced,
                /// <summary>
                /// Représente un écran Full
                /// </summary>
                Full,
            }


            /// <summary>
            /// New,Consult,Update,RemoveOnly,RemoveReplace, etc..
            /// <para>Représente le type de saisie</para>
            /// </summary>
            [Serializable]
            public enum ModeEnum
            {
                /// <summary>
                /// Création
                /// </summary>
                /// FI 20190722 [XXXXX] Add SysNumber_TradeDebtSec
                [TrackerSystemMsg(SysNumber = 1, SysNumber_TradeAdmin = 10, SysNumber_TradeDebtSec = 31)]
                New = 0,
                /// <summary>
                /// Consultation
                /// </summary>
                Consult = 1,
                /// <summary>
                /// Duplication
                /// </summary>
                /// FI 20190722 [XXXXX] Add SysNumber_TradeDebtSec
                [TrackerSystemMsg(SysNumber = 1, SysNumber_TradeAdmin = 10, SysNumber_TradeDebtSec = 31)]
                Duplicate = 2,
                /// <summary>
                /// Modification avec regénération d'évènement
                /// </summary>
                /// FI 20190722 [XXXXX] Add SysNumber_TradeDebtSec
                [TrackerSystemMsg(SysNumber = 2, SysNumber_TradeAdmin = 20, SysNumber_TradeDebtSec = 32)]
                Update = 3,
                /// <summary>
                /// Modification sans génération d'évènement
                /// </summary>
                UpdatePostEvts = 4,
                /// <summary>
                /// Annulation sans remplaçante
                /// </summary>
                [TrackerSystemMsg(SysNumber = 101)]
                RemoveOnly = 5,
                /// <summary>
                /// Annulation avec remplaçante
                /// </summary>
                [TrackerSystemMsg(SysNumber = 102)]
                RemoveReplace = 6,
                /// <summary>
                /// 
                /// </summary>
                [TrackerSystemMsg(SysNumber_TradeAdmin = 25)]
                UpdateAllocatedInvoice = 7,
                /// <summary>
                /// Correction de position (réduction de position)
                /// </summary>
                [TrackerSystemMsg(SysNumber = 184)]
                PositionCancelation = 8,
                /// <summary>
                /// Exercice d'une quantité pour négociations sur options
                /// </summary>
                [TrackerSystemMsg(SysNumber = 105)]
                OptionExercise = 9,
                /// <summary>
                /// Assignation d'une quantité pour négociations sur options
                /// </summary>
                [TrackerSystemMsg(SysNumber = 104)]
                OptionAssignment = 10,
                /// <summary>
                /// Abandon d'une quantité pour négociations sur options
                /// </summary>
                [TrackerSystemMsg(SysNumber = 103)]
                OptionAbandon = 11,
                /// <summary>
                /// Correction
                /// <para>Modification du trade et des évènements associés (ex TradeDeposit)</para>
                /// </summary>
                Correction = 12,
                /// <summary>
                /// Transfert de position
                /// </summary>
                [TrackerSystemMsg(SysNumber = 185)]
                PositionTransfer = 13,
                /// <summary>
                /// Cloture Spécifique
                /// </summary>
                [TrackerSystemMsg(SysNumber = 190)]
                ClearingSpecific = 14,
                /// <summary>
                /// Remove Allocation (s'applique uniquement aux allocations sur instrument fongible)
                /// </summary>
                [TrackerSystemMsg(SysNumber = 4045)]
                RemoveAllocation = 15,
                /// <summary>
                /// TradeSplitting 
                /// </summary>
                [TrackerSystemMsg(SysNumber = 186)]
                TradeSplitting = 16,
                /// <summary>
                /// Livraison du sous-jacent
                /// </summary>
                // PM 20130822 [17949] Livraison Matif
                UnderlyingDelivery = 17,
                /// <summary>
                /// "Matching d'une saisie"
                /// <para>Mode qui consiste à mettre en avant certaines zones de manière à ce que l'utilisateur puisse en contrôler les valeurs</para>
                /// <para>L'utilisateur peut alors valider (statut "match") ou invalider (statut "unmatch") chacune des zones</para>
                /// </summary>
                /// FI 20140708 [20179] add
                Match = 18,
                /// <summary>
                /// Reflection
                /// </summary>
                [TrackerSystemMsg(SysNumber = 1, SysNumber_TradeAdmin = 10)]
                Reflect = 19,

                /// <summary>
                /// FxOptionEarlyTermination
                /// </summary>
                // EG 20180514 [23812] Report
                FxOptionEarlyTermination = 20,
                /// <summary>
                /// Modification des frais non facturés exclusivement
                /// </summary>
                // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
                [TrackerSystemMsg(SysNumber = 50)]
                UpdateFeesUninvoiced = 21,
            }


            #region Method
            /// <summary>
            /// 
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            public static bool IsModeDuplicate(ModeEnum pCaptureEnum)
            {
                return (ModeEnum.Duplicate == pCaptureEnum);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            public static bool IsModeReflect(ModeEnum pCaptureEnum)
            {
                return (ModeEnum.Reflect == pCaptureEnum);
            }

            /// <summary>
            /// Obtient true si IsModeDuplicate ou IsModeReflect
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            public static bool IsModeDuplicateOrReflect(ModeEnum pCaptureEnum)
            {
                return (IsModeDuplicate(pCaptureEnum) || IsModeReflect(pCaptureEnum));
            }

            /// <summary>
            /// Obtient true si IsModeRemoveOnly ou IsModeRemoveReplace
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            public static bool IsModeRemove(ModeEnum pCaptureEnum)
            {
                return IsModeRemoveOnly(pCaptureEnum) || IsModeRemoveReplace(pCaptureEnum);
            }

            /// <summary>
            /// Obtient true si pCaptureEnum vaut RemoveOnly 
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            public static bool IsModeRemoveOnly(ModeEnum pCaptureEnum)
            {
                return (ModeEnum.RemoveOnly == pCaptureEnum);
            }
            /// <summary>
            /// Obttient true si pCaptureEnum vaut RemoveOnly / RemoveAllocation
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            public static bool IsModeRemoveOnlyAll(ModeEnum pCaptureEnum)
            {
                return IsModeRemoveOnly(pCaptureEnum) || IsModeRemoveAllocation(pCaptureEnum);
            }

            /// <summary>
            /// Obtient true si {pCaptureEnum}= ModeEnum.RemoveReplace
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            public static bool IsModeRemoveReplace(ModeEnum pCaptureEnum)
            {
                return (ModeEnum.RemoveReplace == pCaptureEnum);
            }

            /// <summary>
            /// Obtient true si {pCaptureEnum}= ModeEnum.Consult
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            public static bool IsModeConsult(ModeEnum pCaptureEnum)
            {
                return (ModeEnum.Consult == pCaptureEnum);
            }

            /// <summary>
            /// Obtient true si {pCaptureEnum}= ModeEnum.New
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            public static bool IsModeNew(ModeEnum pCaptureEnum)
            {
                return (ModeEnum.New == pCaptureEnum);
            }

            /// <summary>
            /// Obtient true si IsModeNew ou IsModeDuplicate
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            public static bool IsModeNewOrDuplicate(ModeEnum pCaptureEnum)
            {
                return (IsModeNew(pCaptureEnum) || IsModeDuplicate(pCaptureEnum));
            }

            /// <summary>
            /// Obtient true si IsModeNew ou IsModeDuplicate ou IsModeReflect
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            public static bool IsModeNewOrDuplicateOrReflect(ModeEnum pCaptureEnum)
            {
                return (IsModeNew(pCaptureEnum) || IsModeDuplicate(pCaptureEnum) || IsModeReflect(pCaptureEnum));
            }

            /// <summary>
            /// Obtient true si IsModeNew ou IsModeDuplicate
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            public static bool IsModeNewOrDuplicateOrRemove(ModeEnum pCaptureEnum)
            {
                return (IsModeNew(pCaptureEnum) || IsModeDuplicate(pCaptureEnum) || IsModeRemove(pCaptureEnum));
            }

            /// <summary>
            /// Obtient true si IsModeNew ou IsModeDuplicate
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            public static bool IsModeNewOrDuplicateOrReflectOrRemove(ModeEnum pCaptureEnum)
            {
                return (IsModeNew(pCaptureEnum) || IsModeDuplicate(pCaptureEnum) || IsModeReflect(pCaptureEnum) || IsModeRemove(pCaptureEnum));
            }

            /// <summary>
            /// Obtient true si Spheres® génère un nouveau trade 
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            public static bool IsModeNewCapture(ModeEnum pCaptureEnum)
            {
                return (IsModeNew(pCaptureEnum) || IsModeDuplicateOrReflect(pCaptureEnum)
                    || IsModeRemoveReplace(pCaptureEnum) || IsModePositionTransfer(pCaptureEnum));
            }

            /// <summary>
            /// Obtient true si modification totale
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            public static bool IsModeUpdate(ModeEnum pCaptureEnum)
            {
                return (ModeEnum.Update == pCaptureEnum);
            }

            /// <summary>
            /// Obtient true si modification partielle (sans génération des évènements)
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            public static bool IsModeUpdatePostEvts(ModeEnum pCaptureEnum)
            {
                return (ModeEnum.UpdatePostEvts == pCaptureEnum);
            }
            /// <summary>
            /// Obtient true si le mode est :
            /// Modification exclusivement des frais non facturés (avec regénération des évènements des frais)
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
            public static bool IsModeUpdateFeesUninvoiced(ModeEnum pCaptureEnum)
            {
                return (ModeEnum.UpdateFeesUninvoiced == pCaptureEnum);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            public static bool IsModeUpdateAllocatedInvoice(ModeEnum pCaptureEnum)
            {
                return (ModeEnum.UpdateAllocatedInvoice == pCaptureEnum);
            }

            /// <summary>
            /// Obtient true si modification de trade (avec ou sans génération d'évènement)
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            public static bool IsModeUpdateOrUpdatePostEvts(ModeEnum pCaptureEnum)
            {
                return IsModeUpdate(pCaptureEnum) || IsModeUpdatePostEvts(pCaptureEnum);
            }

            /// <summary>
            /// Obtient true si modification en générale (notamment avec ou sans génération d'évènement)
            /// <para>Obtient true si UpdateAllocatedInvoice</para>
            /// <para>Obtient true si UpdateFeesOnly</para>
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
            public static bool IsModeUpdateGen(ModeEnum pCaptureEnum)
            {
                return IsModeUpdateOrUpdatePostEvts(pCaptureEnum) ||
                    IsModeUpdateAllocatedInvoice(pCaptureEnum) ||
                    IsModeUpdateFeesUninvoiced(pCaptureEnum);
            }

            /// <summary>
            /// Mode qui implique une saisie
            /// Création, Duplication, Modification ou Action
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            public static bool IsModeInput(ModeEnum pCaptureEnum)
            {
                return (IsModeNewCapture(pCaptureEnum) ||
                    IsModeUpdateGen(pCaptureEnum) ||
                        IsModeAction(pCaptureEnum));
            }

            /// <summary>
            /// Obtient true lorsque le type de saisie correspond à une action (Remove, CorrectionOfQuantity, Exercise,...)
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            // EG 20180514 [23812] Report 
            public static bool IsModeAction(ModeEnum pCaptureEnum)
            {
                return IsModeRemove(pCaptureEnum) ||
                    (pCaptureEnum == ModeEnum.PositionCancelation) ||
                    (pCaptureEnum == ModeEnum.PositionTransfer) ||
                    (pCaptureEnum == ModeEnum.OptionExercise) ||
                    (pCaptureEnum == ModeEnum.OptionAssignment) ||
                    (pCaptureEnum == ModeEnum.OptionAbandon) ||
                    (pCaptureEnum == ModeEnum.Correction) ||
                    (pCaptureEnum == ModeEnum.RemoveAllocation) ||
                    (pCaptureEnum == ModeEnum.UnderlyingDelivery) || // PM 20130822 [17949] Livraison Matif
                    (pCaptureEnum == ModeEnum.TradeSplitting) ||
                    (pCaptureEnum == ModeEnum.FxOptionEarlyTermination);

            }

            /// <summary>
            /// Obtient true lorsque le type de saisie correspond à une action de dénouement sur option
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            // EG 20151102 [21465] New
            public static bool IsModeDenActionOption(ModeEnum pCaptureEnum)
            {
                return (pCaptureEnum == ModeEnum.OptionExercise) ||
                    (pCaptureEnum == ModeEnum.OptionAssignment) ||
                    (pCaptureEnum == ModeEnum.OptionAbandon);//PL_SPLIT
            }

            /// <summary>
            /// Obtient true si Mode correction
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            public static bool IsModeCorrection(ModeEnum pCaptureEnum)
            {
                return (pCaptureEnum == ModeEnum.Correction);
            }

            /// <summary>
            /// Obtient true si {pCaptureEnum} vaut ModeEnum.PositionTransfer
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            public static bool IsModePositionTransfer(ModeEnum pCaptureEnum)
            {
                return (pCaptureEnum == ModeEnum.PositionTransfer);
            }

            /// <summary>
            /// Obtient true si {pCaptureEnum}= ModeEnum.PositionCancelation
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            public static bool IsModePositionCancelation(ModeEnum pCaptureEnum)
            {
                return (ModeEnum.PositionCancelation == pCaptureEnum);
            }

            /// <summary>
            /// Obtient true si {pCaptureEnum}= ModeEnum.RemoveAllocation
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            public static bool IsModeRemoveAllocation(ModeEnum pCaptureEnum)
            {
                return (ModeEnum.RemoveAllocation == pCaptureEnum);
            }

            /// <summary>
            /// Obtient true si {pCaptureEnum}= ModeEnum.TradeSplitting
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            public static bool IsModeTradeSplitting(ModeEnum pCaptureEnum)
            {
                return (ModeEnum.TradeSplitting == pCaptureEnum);
            }

            /// <summary>
            /// Obtient true si {pCaptureEnum}= ModeEnum.UnderlyingDelivery
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            // PM 20130822 [17949] Livraison Matif
            public static bool IsModeUnderlyingDelivery(ModeEnum pCaptureEnum)
            {
                return (ModeEnum.UnderlyingDelivery == pCaptureEnum);
            }

            /// <summary>
            /// Obtient true si mode "Matching d'une saisie"
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            /// FI 20140708 [20179] add
            public static bool IsModeMatch(ModeEnum pCaptureEnum)
            {
                return (ModeEnum.Match == pCaptureEnum);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <returns></returns>
            public static string GetLabel(ModeEnum pCaptureEnum)
            {
                return GetLabel(pCaptureEnum, CultureInfo.CurrentCulture);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="pCaptureEnum"></param>
            /// <param name="pCulture"></param>
            /// <returns></returns>
            // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
            public static string GetLabel(ModeEnum pCaptureEnum, CultureInfo pCulture)
            {
                string res = string.Empty;
                switch (pCaptureEnum)
                {
                    case ModeEnum.Consult:
                        res = "Consultation";
                        break;
                    case ModeEnum.Update:
                        res = "Modification";
                        break;
                    case ModeEnum.UpdatePostEvts:
                    case ModeEnum.Correction:
                        res = "Correction";
                        break;
                    case ModeEnum.UpdateFeesUninvoiced:
                        res = "FeesCorrection";
                        break;
                    case ModeEnum.UpdateAllocatedInvoice:
                        res = "UpdateAllocatedInvoice";
                        break;
                    case ModeEnum.New:
                    case ModeEnum.Duplicate:
                    case ModeEnum.Reflect:
                        res = "Creation";
                        break;
                    case ModeEnum.RemoveOnly:
                    case ModeEnum.RemoveAllocation:
                        res = "RemoveTrade";
                        break;
                    case ModeEnum.RemoveReplace:
                        res = "RemoveReplaceTrade";
                        break;
                    case ModeEnum.PositionCancelation:
                    case ModeEnum.PositionTransfer:
                    case ModeEnum.OptionExercise:
                    case ModeEnum.OptionAssignment:
                    case ModeEnum.OptionAbandon:
                    case ModeEnum.TradeSplitting:
                    case ModeEnum.UnderlyingDelivery: // PM 20130822 [17949] Livraison Matif
                        res = pCaptureEnum.ToString();
                        break;
                    case ModeEnum.Match:
                        res = "Matching";
                        break;
                }
                return Ressource.GetString(res, pCulture);
            }

            #endregion
        }
        #endregion public class Capture
        #region public class Process
        /// <summary>
        /// ///  Liste des Process OTCml
        /// </summary>
        public class Process
        {

            public static string AccountGen { get { return ProcessTypeEnum.ACCOUNTGEN.ToString(); } }
            public static string AccrualsGen { get { return ProcessTypeEnum.ACCRUALSGEN.ToString(); } }
            public static string EventsGen { get { return ProcessTypeEnum.EVENTSGEN.ToString(); } }
            public static string EventsVal { get { return ProcessTypeEnum.EVENTSVAL.ToString(); } }
            public static string EarGen { get { return ProcessTypeEnum.EARGEN.ToString(); } }
            public static string LinearDepGen { get { return ProcessTypeEnum.LINEARDEPGEN.ToString(); } }
            public static string MTMGen { get { return ProcessTypeEnum.MTMGEN.ToString(); } }
            public static string SettlementInstrGen { get { return ProcessTypeEnum.SIGEN.ToString(); } }
            public static string ConfirmationInstrGen { get { return ProcessTypeEnum.CIGEN.ToString(); } }
            public static string ConfirmationMsgGen { get { return ProcessTypeEnum.CMGEN.ToString(); } }
            public static string ConfirmationMsgGenIO { get { return ProcessTypeEnum.CMGEN_IO.ToString(); } }
            public static string ReportInstrMsgGen { get { return ProcessTypeEnum.RIMGEN.ToString(); } }
            public static string ReportMsgGen { get { return ProcessTypeEnum.RMGEN.ToString(); } }
            //
            public static string ESRGen { get { return ProcessTypeEnum.ESRGEN.ToString(); } }
            public static string ESRStdGen { get { return ProcessTypeEnum.ESRSTDGEN.ToString(); } }
            public static string ESRNetGen { get { return ProcessTypeEnum.ESRNETGEN.ToString(); } }


            #region public IsAccountGen
            public static bool IsAccountGen(string pProcessType) { return (AccountGen == pProcessType); }
            public static bool IsAccountGen(ProcessTypeEnum pProcessTypeEnum) { return (ProcessTypeEnum.ACCOUNTGEN == pProcessTypeEnum); }
            #endregion
            #region public IsAccrualsGen
            public static bool IsAccrualsGen(string pProcessType) { return (AccrualsGen == pProcessType); }
            public static bool IsAccrualsGen(ProcessTypeEnum pProcessTypeEnum) { return (ProcessTypeEnum.ACCRUALSGEN == pProcessTypeEnum); }
            #endregion
            #region public IsMTMGen
            public static bool IsMTMGen(string pProcessType) { return (MTMGen == pProcessType); }
            public static bool IsMTMGen(ProcessTypeEnum pProcessTypeEnum) { return (ProcessTypeEnum.MTMGEN == pProcessTypeEnum); }
            #endregion
            #region public IsConfirmationInstrGen
            public static bool IsConfirmationInstrGen(string pProcessType) { return (ConfirmationInstrGen == pProcessType); }
            public static bool IsConfirmationInstrGen(ProcessTypeEnum pProcessTypeEnum) { return (ProcessTypeEnum.CIGEN == pProcessTypeEnum); }
            #endregion
            #region public IsConfirmationMsgGen
            public static bool IsConfirmationMsgGen(string pProcessType) { return (ConfirmationMsgGen == pProcessType); }
            public static bool IsConfirmationMsgGen(ProcessTypeEnum pProcessTypeEnum) { return (ProcessTypeEnum.CMGEN == pProcessTypeEnum); }
            #endregion IsConfirmationMsgGen
            #region public IsReportInstrMsgGen
            public static bool IsReportInstrMsgGen(string pProcessType) { return (ReportInstrMsgGen == pProcessType); }
            public static bool IsReportInstrMsgGen(ProcessTypeEnum pProcessTypeEnum) { return (ProcessTypeEnum.RIMGEN == pProcessTypeEnum); }
            #endregion IsReportInstrMsgGen
            #region public IsReportMsgGen
            public static bool IsReportMsgGen(string pProcessType) { return (ReportMsgGen == pProcessType); }
            public static bool IsReportMsgGen(ProcessTypeEnum pProcessTypeEnum) { return (ProcessTypeEnum.RMGEN == pProcessTypeEnum); }
            #endregion IsReportMsgGen
            #region public IsConfirmationMsgGenIO
            public static bool IsConfirmationMsgGenIO(string pProcessType) { return (ConfirmationMsgGenIO == pProcessType); }
            public static bool IsConfirmationMsgGenIO(ProcessTypeEnum pProcessTypeEnum) { return (ProcessTypeEnum.CMGEN_IO == pProcessTypeEnum); }
            #endregion IsConfirmationMsgGenIO
            #region public IsEarGen
            public static bool IsEarGen(string pProcessType) { return (EarGen == pProcessType); }
            public static bool IsEarGen(ProcessTypeEnum pProcessTypeEnum) { return (ProcessTypeEnum.EARGEN == pProcessTypeEnum); }
            #endregion EarGen
            #region public IsEventsGen
            public static bool IsEventsGen(string pProcessType) { return (EventsGen == pProcessType); }
            public static bool IsEventsGen(ProcessTypeEnum pProcessTypeEnum) { return (ProcessTypeEnum.EVENTSGEN == pProcessTypeEnum); }
            #endregion EventsGen
            #region public IsEventsVal
            public static bool IsEventsVal(string pProcessType) { return (EventsVal == pProcessType); }
            public static bool IsEventsVal(ProcessTypeEnum pProcessTypeEnum) { return (ProcessTypeEnum.EVENTSVAL == pProcessTypeEnum); }
            #endregion EventsVal
            #region public IsLinearDepGen
            public static bool IsLinearDepGen(string pProcessType) { return (LinearDepGen == pProcessType); }
            public static bool IsLinearDepGen(ProcessTypeEnum pProcessTypeEnum) { return (ProcessTypeEnum.LINEARDEPGEN == pProcessTypeEnum); }
            #endregion EventsVal
            #region public IsSettlementInstrGen
            public static bool IsSettlementInstrGen(string pProcessType) { return (SettlementInstrGen == pProcessType); }
            public static bool IsSettlementInstrGen(ProcessTypeEnum pProcessTypeEnum) { return (ProcessTypeEnum.SIGEN == pProcessTypeEnum); }
            #endregion SettlementInstrGen
            #region public IsESRStdGen
            public static bool IsESRStdGen(string pProcessType) { return (ESRStdGen == pProcessType); }
            public static bool IsESRStdGen(ProcessTypeEnum pProcessTypeEnum) { return (ProcessTypeEnum.ESRSTDGEN == pProcessTypeEnum); }
            #endregion IsESRStdGen
            #region public IsESRSNetGen
            public static bool IsESRSNetGen(string pProcessType) { return (ESRNetGen == pProcessType); }
            public static bool IsESRNetGen(ProcessTypeEnum pProcessTypeEnum) { return (ProcessTypeEnum.ESRNETGEN == pProcessTypeEnum); }
            #endregion
            #region public IsESRGen
            public static bool IsESRGen(string pProcessType) { return (ESRGen == pProcessType); }
            public static bool IsESRGen(ProcessTypeEnum pProcessTypeEnum) { return ((ProcessTypeEnum.ESRGEN == pProcessTypeEnum)); }
            #endregion

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pProcessTypeEnum"></param>
            /// <returns></returns>
            public static bool IsPosKeepingEntry(ProcessTypeEnum pProcessTypeEnum) { return (ProcessTypeEnum.POSKEEPENTRY == pProcessTypeEnum); }

            /// <summary>
            /// Retourne le service qui traite le process
            /// Si le process n'est pas traité par un service alors retourne NA
            /// </summary>
            /// <param name="pProcessTypeEnum"></param>
            /// <returns></returns>
            /// EG 20101206 Add POSKEEPING Process
            /// EG 20120328 Ticket 17706 Recalcul des frais
            /// FI 20120801 [18058] add CASHBALANCEINTEREST
            /// FI 20140519 [19923] add case REQUESTTRACK
            /// PL 20221222 Add case IRQ for SpheresNormMsgFactory
            /// EG 20230102 Add case CORPOACTIONINTEGRATE for SpheresNormMsgFactory
            /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory (Nouveau type de process)
            /// EG 20240109 [WI801] Invoicing : Suppression et Validation de factures simulées prise en charge par le service
            // EG 20240123 [WI816] New ProcessType.FEESUNINVOICED
            public static ServiceEnum GetService(ProcessTypeEnum pProcessTypeEnum)
            {
                ServiceEnum ret = ServiceEnum.NA;

                switch (pProcessTypeEnum)
                {
                    case ProcessTypeEnum.ACCOUNTGEN:
                        ret = ServiceEnum.SpheresAccountGen;
                        break;
                    case ProcessTypeEnum.ACCRUALSGEN:
                    case ProcessTypeEnum.LINEARDEPGEN:
                        //case ProcessTypeEnum.ETDCLOSINGGEN:
                        ret = ServiceEnum.SpheresClosingGen;
                        break;
                    case ProcessTypeEnum.CMGEN:
                    case ProcessTypeEnum.CMGEN_IO:
                    case ProcessTypeEnum.CIGEN:
                    case ProcessTypeEnum.RIMGEN:
                    case ProcessTypeEnum.RMGEN:
                        ret = ServiceEnum.SpheresConfirmationMsgGen;
                        break;
                    case ProcessTypeEnum.EARGEN:
                        ret = ServiceEnum.SpheresEarGen;
                        break;
                    case ProcessTypeEnum.EVENTSGEN:
                        ret = ServiceEnum.SpheresEventsGen;
                        break;
                    case ProcessTypeEnum.EVENTSVAL:
                    case ProcessTypeEnum.COLLATERALVAL:
                        ret = ServiceEnum.SpheresEventsVal;
                        break;
                    case ProcessTypeEnum.INVOICINGGEN:
                    case ProcessTypeEnum.INVCANCELSIMUL:
                    case ProcessTypeEnum.INVVALIDSIMUL:
                        ret = ServiceEnum.SpheresInvoicingGen;
                        break;
                    case ProcessTypeEnum.IO:
                        ret = ServiceEnum.SpheresIO;
                        break;
                    case ProcessTypeEnum.MTMGEN:
                        ret = ServiceEnum.SpheresMarkToMarketGen;
                        break;
                    case ProcessTypeEnum.POSKEEPENTRY:
                    case ProcessTypeEnum.POSKEEPREQUEST:
                        ret = ServiceEnum.SpheresClosingGen;
                        break;
                    case ProcessTypeEnum.QUOTHANDLING:
                        ret = ServiceEnum.SpheresQuotationHandling;
                        break;
                    case ProcessTypeEnum.SIGEN:
                        ret = ServiceEnum.SpheresSettlementInstrGen;
                        break;
                    case ProcessTypeEnum.ESRGEN:
                    case ProcessTypeEnum.ESRSTDGEN:
                    case ProcessTypeEnum.ESRNETGEN:
                    case ProcessTypeEnum.MSOGEN:
                        ret = ServiceEnum.SpheresSettlementMsgGen;
                        break;
                    case ProcessTypeEnum.TRADEACTGEN:
                    case ProcessTypeEnum.ACTIONGEN:
                    case ProcessTypeEnum.FEESCALCULATION:
                    case ProcessTypeEnum.FEESUNINVOICED:
                        ret = ServiceEnum.SpheresTradeActionGen;
                        break;
                    case ProcessTypeEnum.GATEBCS:
                        ret = ServiceEnum.SpheresGateBCS;
                        break;
                    case ProcessTypeEnum.SHELL:
                        ret = ServiceEnum.SpheresShell;
                        break;
                    case ProcessTypeEnum.RESPONSE:
                        ret = ServiceEnum.SpheresResponse;
                        break;
                    case ProcessTypeEnum.RISKPERFORMANCE:
                    case ProcessTypeEnum.CASHBALANCE:
                    case ProcessTypeEnum.CASHINTEREST:
                        ret = ServiceEnum.SpheresRiskPerformance;
                        break;
                    case ProcessTypeEnum.NORMMSGFACTORY:
                    case ProcessTypeEnum.IRQ: //PL 20221222 Add IRQ (Une demande IRQ entraine le postage d'un message à NormMsgFactory)
                    case ProcessTypeEnum.CORPOACTIONINTEGRATE: //EG 20230102 Add CORPOACTIONINTEGRATE (Une demande CORPOACTIONINTEGRATE entraine le postage d'un message à NormMsgFactory)
                    case ProcessTypeEnum.CLOSINGREOPENINGINTEGRATE:
                        ret = ServiceEnum.SpheresNormMsgFactory;
                        break;
                    case ProcessTypeEnum.REQUESTTRACK:
                        ret = ServiceEnum.SpheresWebSession;
                        break;
                    default:
                        ret = ServiceEnum.NA;
                        break;
                }
#if DEBUG
                if (ret == ServiceEnum.NA)
                    throw new NotImplementedException(StrFunc.AppendFormat("Process {0} is not Implemented", pProcessTypeEnum.ToString()));

#endif

                return ret;
            }


            /// <summary>
            /// Libelle anglais pour Chaque process (use by tracker)
            /// Ds le fichier SpheresRessource.resx
            /// </summary>
            /// <param name="pProcess"></param>
            /// <returns></returns>
            public static string GetResInvariantCulture(ProcessTypeEnum pProcess)
            {
                return Ressource.GetString(pProcess.ToString() + "_invariantCulture");
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="pLibProcess"></param>
            /// <returns></returns>
            public static string GetResInvariantCulture(string pLibProcess)
            {
                return Ressource.GetString(pLibProcess + "_invariantCulture");
            }
        }
        #endregion
        #region public class ListProcess
        /// <summary>
        ///  Process géré par la page List.Aspx
        ///  Création d'un bouton qui permet :
        ///  l'execution de procedure Csharp (Reflexion) ou 
        ///  l'execution de procedure SQL (stored proc)  ou
        ///  d'envoyer  un message vers un process d'un service
        /// </summary>
        public class ListProcess
        {
            #region public ListProcessTypeEnum
            public enum ListProcessTypeEnum
            {
                ProcessCSharp,	 // Process C# 
                Service,         // Services
                StoredProcedure, // Stored procedure
                Unknown,
            }
            #endregion

            #region public IsListProcess
            public static bool IsListProcess(string pListProcess)
            {
                return Enum.IsDefined(typeof(ListProcessTypeEnum), pListProcess);
            }
            #endregion IsListProcess
            //
            #region public IsProcessCSharp
            public static bool IsProcessCSharp(ListProcessTypeEnum pListProcessTypeEnum)
            {
                return (ListProcessTypeEnum.ProcessCSharp == pListProcessTypeEnum);
            }
            public static bool IsProcessCSharp(string pProcessType)
            {
                ListProcessTypeEnum typeEnum = ListProcessTypeEnum.Unknown;
                if (StrFunc.IsFilled(pProcessType))
                    typeEnum = (ListProcessTypeEnum)Enum.Parse(typeof(ListProcessTypeEnum), pProcessType, true);
                return IsProcessCSharp(typeEnum);
            }
            #endregion
            //
            #region public IsService
            public static bool IsService(ListProcessTypeEnum pListProcessTypeEnum)
            {
                return (ListProcessTypeEnum.Service == pListProcessTypeEnum);
            }
            public static bool IsService(string pProcessType)
            {
                ListProcessTypeEnum typeEnum = ListProcessTypeEnum.Unknown;
                if (StrFunc.IsFilled(pProcessType))
                    typeEnum = (ListProcessTypeEnum)Enum.Parse(typeof(ListProcessTypeEnum), pProcessType, true);
                return IsService(typeEnum);
            }
            #endregion
            //
            #region public IsStoredProcedure
            public static bool IsStoredProcedure(ListProcessTypeEnum pListProcessTypeEnum)
            {
                return (ListProcessTypeEnum.StoredProcedure == pListProcessTypeEnum);
            }
            public static bool IsStoredProcedure(string pProcessType)
            {
                ListProcessTypeEnum typeEnum = ListProcessTypeEnum.Unknown;
                if (StrFunc.IsFilled(pProcessType))
                    typeEnum = (ListProcessTypeEnum)Enum.Parse(typeof(ListProcessTypeEnum), pProcessType, true);
                return IsStoredProcedure(typeEnum);
            }
            #endregion
        }
        #endregion

        #region Constantes des préfixes pour les ID des controls
        /// EG 20170918 [23342] New TMS
        public const string
            CHK = "CHK",
            HCK = "HCK",//HTMLCHECK
            DDL = "DDL",
            HSL = "HSL",//HTMLSELECT
            LBL = "LBL",
            DSP = "DSP",//Display --> Label destiné à l'affichage d'un libellé long
            CHEAD = "CHEAD",//CellHeader --> Label dans Cell destiné à l'affichage d'un libellé dans une cellule de table
            CFOOT = "CFOOT",//CellFooter --> Label dans Cell destiné à l'affichage d'un libellé dans une cellule de table
            TXT = "TXT",
            HID = "HDN",
            HDN = "HDN",
            BUT = "BUT",
            IMG = "IMG",
            QKI = "QKI",// QuickInput => Zone Text
            PNL = "PNL",
            LNK = "LNK", // HYPERLINK
            TMS = "TMS", // TIMESTAMP
            TMZ = "TMZ"; // TIMEZONE
        #endregion

        #region Constantes mots clés dynamiques
        public const string
            DYNAMIC_ALIASTABLE = "<aliasTable>",
            DYNAMIC_OPERATOR = "<operator>",
            DYNAMIC_VALUE = "<value>";
        #endregion

        #region public class TypeInformationMessage
        /// <summary>
        /// Type of message (used by XML Repository)
        /// </summary>
        public class TypeInformationMessage
        {
            public const string Information = "information";
            public const string Warning = "warning";
            public const string Alert = "alert";
            public const string Success = "success";
        }
        #endregion

        #region Constantes d'alias pour ATTACHEDDOC & NOTEPAD
        public const string AliasNOTEPAD = "EFSnp";
        public const string AliasNOTEPADACTOR = "EFSanp";
        public const string AliasATTACHEDDOC = "EFSad";
        public const string AliasATTACHEDDOCACTOR = "EFSaad";
        #endregion Constantes d'alias pour ATTACHEDDOC & NOTEPAD

        #region WithExchangeEnum
        public enum EarExchangeEnum
        {
            YES,
            NO,
        }
        #endregion WithExchangeEnum
        #region EarCommonEnum
        public enum EarCommonEnum
        {
            NONE,
            ALL,
            OTC,
            ETD,
        }
        #endregion EarCommonEnum
        #region public GetPowerOfAllProcess
        public static Int64 GetPowerOfAllProcess()
        {
            Cst.ProcessTypeEnum processEnum = new Cst.ProcessTypeEnum();
            FieldInfo[] processFlds = processEnum.GetType().GetFields();
            Int64 powerProcess = 0;
            foreach (FieldInfo processFld in processFlds)
            {
                object[] processRequestedAttrs = processFld.GetCustomAttributes(typeof(Cst.ProcessRequestedAttribute), true);
                if (0 < processRequestedAttrs.Length)
                {
                    Cst.ProcessTypeEnum process = (Cst.ProcessTypeEnum)Enum.Parse(typeof(Cst.ProcessTypeEnum), processFld.Name, false);
                    int i = int.Parse(Enum.Format(typeof(Cst.ProcessTypeEnum), process, "d"));
                    powerProcess += Convert.ToInt64(Math.Pow(2, i));
                }
            }
            return powerProcess;
        }
        #endregion GetPowerOfAllProcess
        #region GetPowerOfAllGroupTracker
        public static Int64 GetPowerOfAllGroupTracker()
        {
            Int64 powerProcess = 0;
            foreach (Cst.GroupTrackerEnum group in Enum.GetValues(typeof(Cst.GroupTrackerEnum)))
            {
                if (group != Cst.GroupTrackerEnum.ALL)
                {
                    string hexValue = Enum.Format(typeof(Cst.GroupTrackerEnum), group, "x");
                    powerProcess += int.Parse(hexValue, NumberStyles.HexNumber);

                }
            }
            return powerProcess;
        }
        #endregion GetPowerOfGroupTracker

        #region PosRequestAssetQuoteEnum
        // EG 20110201 New Enum 
        public enum PosRequestAssetQuoteEnum
        {
            None,
            UnderlyerAsset,
            Asset,
        }
        #endregion PosRequestAssetQuoteEnum
        #region PosRequestSourceEnum
        // EG 20110201 New Enum 
        public enum PosRequestSourceEnum
        {
            None,
            [System.Xml.Serialization.XmlEnumAttribute("USR")]
            User,
            [System.Xml.Serialization.XmlEnumAttribute("SYS")]
            System,
        }
        #endregion PosRequestSourceEnum
        #region PosRequestTypeEnum
        // EG 20110125 New Enum 
        // PM 20130212 [18414] Add Cascading, EOD_CascadingGroupLevel, Shifting, SOD_ShiftingGroupLevel
        // EG 20130405 Réordonnancement des Enums
        // EG 20130607 [18740] Add RemoveCAExecuted
        // EG 20130701 Add AllocMissing
        // FI 20160819 [22364] Modify
        // EG 20170206 [22787] Add DLYPERIODIC et EOD_DLYPERIODIC (Réordonnancement + Flag attribute + + Valeur hexadécimale)
        // EG 20170224 [22717] EOD : PriceControl
        // EG 20180525 [23979] IRQ Processing Add IRQManaged|SysNumber_IRQRequest on TrackerSystemMsgAttribute
        // EG 20190214 Correction messages Tracker pour NormMsgFactory (SysNumber_NMF)
        // EG 20190214 ACTIONREQUEST (New ClosingPosition & ClosingReopeningPosition)
        // EG 20190926 REFACTORING CAR LIMITE 64 items atteint
        // RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)
        [FlagsAttribute]
        public enum PosRequestTypeEnum : long
        {
            None = 0,
            [System.Xml.Serialization.XmlEnumAttribute("ALLOCMISSING")]
            AllocMissing = 1,
            [System.Xml.Serialization.XmlEnumAttribute("AUTOASS")]
            AutomaticOptionAssignment = 2,
            [System.Xml.Serialization.XmlEnumAttribute("AUTOABN")]
            AutomaticOptionAbandon = 4,
            [System.Xml.Serialization.XmlEnumAttribute("AUTOEXE")]
            AutomaticOptionExercise = 8,
            [System.Xml.Serialization.XmlEnumAttribute("CAS")]
            Cascading = 16,
            [System.Xml.Serialization.XmlEnumAttribute("CLEARBULK")]
            [TrackerSystemMsg(SysNumber = 200)]
            ClearingBulk = 32,
            [System.Xml.Serialization.XmlEnumAttribute("CLEAREOD")]
            ClearingEndOfDay = 64,
            [System.Xml.Serialization.XmlEnumAttribute("CLEARSPEC")]
            [TrackerSystemMsg(SysNumber = 190)]
            ClearingSpecific = 128,
            [System.Xml.Serialization.XmlEnumAttribute("CLOSINGDAY")]
            [TrackerSystemMsg(IRQManaged = true, SysNumber = 4110, SysNumber_MTM = 4115, SysNumber_IRQRequest = 7110, SysNumber_NMF = 4116)]
            ClosingDay = 256,
            [System.Xml.Serialization.XmlEnumAttribute("CLO")]
            Closure = 512,
            [System.Xml.Serialization.XmlEnumAttribute("CORPOACTION")]
            CorporateAction = 1024,
            [System.Xml.Serialization.XmlEnumAttribute("ENTRY")]
            Entry = 2048,
            [System.Xml.Serialization.XmlEnumAttribute("EOD")]
            [TrackerSystemMsg(IRQManaged = true, SysNumber = 4100, SysNumber_MTM = 4105, SysNumber_IRQRequest = 7100, SysNumber_NMF = 4106)]
            EndOfDay = 4096,
            // EG 20231129 [WI762] End of Day processing : Possibility to request processing without initial margin (Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin)
            [System.Xml.Serialization.XmlEnumAttribute("EOD_WOIM")]
            [TrackerSystemMsg(IRQManaged = true, SysNumber = 4108, SysNumber_MTM = 4105, SysNumber_IRQRequest = 7100, SysNumber_NMF = 4109)]
            EndOfDayWithoutInitialMargin = EndOfDay - InitialMargin,
            [System.Xml.Serialization.XmlEnumAttribute("EVENTMISSING")]
            EventMissing = 8192,
            [System.Xml.Serialization.XmlEnumAttribute("MOO")]
            MaturityOffsettingOption = 16384,
            [System.Xml.Serialization.XmlEnumAttribute("MOF")]
            MaturityOffsettingFuture = 32768,
            [System.Xml.Serialization.XmlEnumAttribute("ABN")]
            [TrackerSystemMsg(SysNumber = 180)]
            OptionAbandon = 65536,
            [System.Xml.Serialization.XmlEnumAttribute("ASS")]
            [TrackerSystemMsg(SysNumber = 181)]
            OptionAssignment = 131072,
            [System.Xml.Serialization.XmlEnumAttribute("EXE")]
            [TrackerSystemMsg(SysNumber = 182)]
            OptionExercise = 262144,
            [System.Xml.Serialization.XmlEnumAttribute("DLYPERIODIC")]
            PhysicalPeriodicDelivery = 524288,
            [System.Xml.Serialization.XmlEnumAttribute("POC")]
            [TrackerSystemMsg(SysNumber = 184)]
            PositionCancelation = 1048576,
            [System.Xml.Serialization.XmlEnumAttribute("POI")]
            PositionInsertion = 2097152,
            [System.Xml.Serialization.XmlEnumAttribute("POT")]
            [TrackerSystemMsg(SysNumber = 185)]
            PositionTransfer = 4194304,
            [System.Xml.Serialization.XmlEnumAttribute("PRICECONTROL")]
            PriceControl = 8388608,
            [System.Xml.Serialization.XmlEnumAttribute("RMVALLOC")]
            [TrackerSystemMsg(SysNumber = 183)]
            RemoveAllocation = 16777216,
            [System.Xml.Serialization.XmlEnumAttribute("RMVCAEXECUTED")]
            [TrackerSystemMsg(SysNumber = 6102)]
            RemoveCAExecuted = 33554432,
            [System.Xml.Serialization.XmlEnumAttribute("REMOVEEOD")]
            [TrackerSystemMsg(SysNumber = 4210, SysNumber_MTM = 4215)]
            RemoveEndOfDay = 67108864,
            [System.Xml.Serialization.XmlEnumAttribute("SHI")]
            Shifting = 134217728,
            [System.Xml.Serialization.XmlEnumAttribute("TRADEMERGING")]
            TradeMerging = 268435456,
            [System.Xml.Serialization.XmlEnumAttribute("TRADESPLITTING")]
            [TrackerSystemMsg(SysNumber = 186)]
            TradeSplitting = 536870912,
            [System.Xml.Serialization.XmlEnumAttribute("UNCLEARING")]
            [TrackerSystemMsg(SysNumber = 210)]
            UnClearing = 1073741824,
            [System.Xml.Serialization.XmlEnumAttribute("UNLDLVR")]
            UnderlyerDelivery = 2147483648,
            [System.Xml.Serialization.XmlEnumAttribute("UPDENTRY")]
            [TrackerSystemMsg(SysNumber = 4040)]
            UpdateEntry = 4294967296,
            [System.Xml.Serialization.XmlEnumAttribute("CLOSINGPOS")]
            ClosingPosition = 8589934592,
            [System.Xml.Serialization.XmlEnumAttribute("CLOSINGREOPENINGPOS")]
            ClosingReopeningPosition = 17179869184,
            [System.Xml.Serialization.XmlEnumAttribute("MOD")]
            MaturityRedemptionOffsettingDebtSecurity = 34359738368,

            [System.Xml.Serialization.XmlEnumAttribute("MARKET")]
            Market = 68719476736,
            [System.Xml.Serialization.XmlEnumAttribute("CTRL")]
            Control = 137438953472,
            [System.Xml.Serialization.XmlEnumAttribute("VALIDATION")]
            Validation = 341986770944,

            [System.Xml.Serialization.XmlEnumAttribute("CASHFLOWS")]
            CashFlows = 683973541888,
            [System.Xml.Serialization.XmlEnumAttribute("CASHBALANCE")]
            CashBalance = 1367947083776,
            [System.Xml.Serialization.XmlEnumAttribute("FEES")]
            Fees = 2735894167552,
            [System.Xml.Serialization.XmlEnumAttribute("INITIALMARGIN")]
            InitialMargin = 5471788335104,
            [System.Xml.Serialization.XmlEnumAttribute("SAFEKEEPING")]
            Safekeeping = 10943576670208,
            [System.Xml.Serialization.XmlEnumAttribute("UTICALCULATION")]
            UTICalculation = 21887153340416,

            [System.Xml.Serialization.XmlEnumAttribute("NEX")]
            [TrackerSystemMsg(SysNumber = 180)]
            OptionNotExercised = 43774306680832,
            [System.Xml.Serialization.XmlEnumAttribute("NAS")]
            [TrackerSystemMsg(SysNumber = 180)]
            OptionNotAssigned = 87548613361664,

            // EOD Group Level
            [System.Xml.Serialization.XmlEnumAttribute("EOD_CASHBALANCE")]
            EOD_CashBalanceGroupLevel = EndOfDay + CashBalance,
            [System.Xml.Serialization.XmlEnumAttribute("EOD_CASHFLOWS")]
            EOD_CashFlowsGroupLevel = EndOfDay + CashFlows,
            [System.Xml.Serialization.XmlEnumAttribute("EOD_CTRL")]
            EOD_ControlGroupLevel = EndOfDay + Control,
            [System.Xml.Serialization.XmlEnumAttribute("EOD_FEES")]
            EOD_FeesGroupLevel = EndOfDay + Fees,
            [System.Xml.Serialization.XmlEnumAttribute("EOD_INITIALMARGIN")]
            EOD_InitialMarginGroupLevel = EndOfDay + InitialMargin,
            [System.Xml.Serialization.XmlEnumAttribute("EODMARKET")]
            EOD_MarketGroupLevel = EndOfDay + Market,
            [System.Xml.Serialization.XmlEnumAttribute("EOD_SAFEKEEPING")]
            EOD_SafekeepingGroupLevel = EndOfDay + Safekeeping,
            [System.Xml.Serialization.XmlEnumAttribute("EOD_UTICALCULATION")]
            EOD_UTICalculationGroupLevel = EndOfDay + UTICalculation,
            [System.Xml.Serialization.XmlEnumAttribute("EOD_AUTOOPTION")]
            EOD_AutomaticOptionGroupLevel = EndOfDay + (AutomaticOptionAbandon | AutomaticOptionAssignment | AutomaticOptionExercise),
            [System.Xml.Serialization.XmlEnumAttribute("EOD_MANUALOPTION")]
            EOD_ManualOptionGroupLevel = EndOfDay + (OptionAbandon | OptionNotExercised | OptionNotAssigned | OptionAssignment | OptionExercise),
            [System.Xml.Serialization.XmlEnumAttribute("EOD_SHI")]
            EOD_ShiftingGroupLevel = EndOfDay + Shifting,
            [System.Xml.Serialization.XmlEnumAttribute("EOD_UNLDLVR")]
            EOD_UnderlyerDeliveryGroupLevel = EndOfDay + UnderlyerDelivery,
            [System.Xml.Serialization.XmlEnumAttribute("EOD_UPDENTRY")]
            EOD_UpdateEntryGroupLevel = EndOfDay + UpdateEntry,
            [System.Xml.Serialization.XmlEnumAttribute("EOD_MOF")]
            EOD_MaturityOffsettingFutureGroupLevel = EndOfDay + MaturityOffsettingFuture,
            [System.Xml.Serialization.XmlEnumAttribute("EOD_CLEAREOD")]
            EOD_ClearingEndOfDayGroupLevel = EndOfDay + ClearingEndOfDay,
            [System.Xml.Serialization.XmlEnumAttribute("EOD_CORPOACTION")]
            EOD_CorporateActionGroupLevel = EndOfDay + CorporateAction,
            [System.Xml.Serialization.XmlEnumAttribute("EOD_CAS")]
            EOD_CascadingGroupLevel = EndOfDay + Cascading,
            [System.Xml.Serialization.XmlEnumAttribute("EOD_DLYPERIODIC")]
            EOD_PhysicalPeriodicDelivery = EndOfDay + PhysicalPeriodicDelivery,
            [System.Xml.Serialization.XmlEnumAttribute("EOD_MOD")]
            EOD_MaturityRedemptionOffsettingDebtSecurityGroupLevel = EndOfDay + MaturityRedemptionOffsettingDebtSecurity,

            // CLOSINGDAY Group Level
            [System.Xml.Serialization.XmlEnumAttribute("CLOSINGDAY_ALLOCMISSING")]
            ClosingDay_AllocMissing = ClosingDay + AllocMissing,
            [System.Xml.Serialization.XmlEnumAttribute("CLOSINGDAY_CORPOACTION")]
            ClosingDay_CorpoActionGroupLevel = ClosingDay + CorporateAction,
            [System.Xml.Serialization.XmlEnumAttribute("CLOSINGDAY_EOD")]
            ClosingDay_EndOfDayGroupLevel = ClosingDay + EndOfDay,
            [System.Xml.Serialization.XmlEnumAttribute("CLOSINGDAY_ENTRY")]
            ClosingDay_EntryGroupLevel = ClosingDay + Entry,
            [System.Xml.Serialization.XmlEnumAttribute("CLOSINGDAY_EODMARKET")]
            ClosingDay_EOD_MarketGroupLevel = ClosingDay + EOD_MarketGroupLevel,
            [System.Xml.Serialization.XmlEnumAttribute("CLOSINGDAY_RMVALLOC")]
            ClosingDay_RemoveAllocationGroupLevel = ClosingDay + RemoveAllocation,
            [System.Xml.Serialization.XmlEnumAttribute("CLOSINGDAY_CTRL")]
            ClosingDay_ControlGroupLevel = ClosingDay + Control,
            [System.Xml.Serialization.XmlEnumAttribute("CLOSINGDAYMARKET")]
            ClosingDayMarketGroupLevel = ClosingDay + Market,
            [System.Xml.Serialization.XmlEnumAttribute("CLOSINGDAY_INITIALMARGIN_CTRL")]
            ClosingDay_InitialMarginControlGroupLevel = ClosingDay + InitialMargin + Control,
            [System.Xml.Serialization.XmlEnumAttribute("CLOSINGDAY_VALID")]
            ClosingDay_ValidationGroupLevel = ClosingDay + Validation,
            // EG 20240520 [WI930] Euronext Clearing migration for Commodities & Financial Derivatives : Closing|Reopening position processing
            [System.Xml.Serialization.XmlEnumAttribute("CLOSINGDAY_CRP_VALID")]
            ClosingDay_ClosingReopeningValidationGroupLevel = ClosingReopeningPosition + Validation,

            // Others Group
            RequestTypeAutomaticOption = AutomaticOptionAbandon | AutomaticOptionAssignment | AutomaticOptionExercise,
            RequestTypeManualOption = OptionAbandon | OptionNotExercised | OptionNotAssigned | OptionAssignment | OptionExercise,
            RequestTypeOption = RequestTypeAutomaticOption | RequestTypeManualOption,
            RequestTypeTradeLinkPosRequest = OptionAssignment | AutomaticOptionAssignment | OptionExercise | AutomaticOptionExercise | CorporateAction | PositionTransfer | ClosingPosition | ClosingReopeningPosition,
            RequestTypeOptionAndMaturityOffSetting = RequestTypeAutomaticOption | RequestTypeManualOption | MaturityOffsettingFuture,

            RequestTypeMarket = RequestTypeOptionAndMaturityOffSetting | MaturityOffsettingOption | Cascading | Shifting | ClearingSpecific | UnderlyerDelivery | UpdateEntry | ClearingEndOfDay |
            PhysicalPeriodicDelivery | TradeMerging | EOD_UTICalculationGroupLevel | CorporateAction | ClosingPosition | ClosingReopeningPosition | MaturityRedemptionOffsettingDebtSecurity,

            RequestTypeAll = RequestTypeMarket,

            RequestTypeClosing = ClosingDay_ControlGroupLevel | ClosingDay_EndOfDayGroupLevel | ClosingDay_EntryGroupLevel | ClosingDay_ValidationGroupLevel |
            ClosingDay_CorpoActionGroupLevel | ClosingPosition | ClosingReopeningPosition,

            RequestTypePosEffectOnly = ClearingBulk | ClearingEndOfDay | ClearingSpecific | Entry | UpdateEntry,
        }

        #endregion PosRequestTypeEnum
        #region public enum GroupTrackerEnum
        /// EG 20161122 Add FlagsAttribute
        /// FI 20201102 [XXXXX] use ResourceAttribut
        public enum GroupTrackerEnum
        {
            [ResourceAttribut(Resource = "GroupTrackerALL")]
            ALL = 0x00,
            [ResourceAttribut(Resource = "GroupTrackerTRD")]
            TRD = 0x01,
            [ResourceAttribut(Resource = "GroupTrackerIO")]
            IO = 0x02,
            [ResourceAttribut(Resource = "GroupTrackerMSG")]
            MSG = 0x04,
            [ResourceAttribut(Resource = "GroupTrackerACC")]
            ACC = 0x08,
            [ResourceAttribut(Resource = "GroupTrackerCLO")]
            CLO = 0x10,
            [ResourceAttribut(Resource = "GroupTrackerINV")]
            INV = 0x20,
            [ResourceAttribut(Resource = "GroupTrackerEXT")]
            EXT = 0x40,
        }
        #endregion public enum GroupTrackerEnum
        #region public enum ProcessTypeEnum
        /// <summary>
        /// Représente les process de Spheres
        /// </summary>
        // EG 20120328 Ticket 17706 Recalcul des frais
        /// EG 20140826 Add InvoicingValidation|InvoicingCancellation
        // EG 20180525 [23979] IRQ Processing Add IRQManaged|SysNumber_IRQRequest on TrackerSystemMsgAttribute
        // EG 20220221 [XXXXX] Activation IRQManaged on IO
        // EG 20240109 [WI801] Invoicing : Suppression et Validation de factures simulées prise en charge par le service
        // EG 20240123 [WI816] New ProcessType.FEESUNINVOICED
        public enum ProcessTypeEnum
        {
            /// <summary>
            /// Process d'actions diverses 
            /// </summary>
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.TRD, SysNumber = 100, SysNumber_ObserverMode = 250)]
            ACTIONGEN,
            /// <summary>
            /// Processus de recalcul des frais sur un trade
            /// </summary>
            [ProcessRequested(ServiceName = ServiceEnum.SpheresTradeActionGen)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.CLO, SysNumber = 4030, SysNumber_ObserverMode = 4031)]
            FEESCALCULATION,
            /// <summary>
            /// Processus de recalcul des frais non facturés sur un trade
            /// </summary>
            [ProcessRequested(ServiceName = ServiceEnum.SpheresTradeActionGen)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.TRD, SysNumber = 50, SysNumber_ObserverMode = 51)]
            FEESUNINVOICED,
            /// <summary>
            /// Process de génération des écritures comptables
            /// </summary>
            [ProcessRequested(ServiceName = ServiceEnum.SpheresAccountGen)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.ACC, SysNumber = 3100, SysNumber_ObserverMode = 3101)]
            ACCOUNTGEN,
            /// <summary>
            /// Process de génération des intérêts courus
            /// </summary>
            [ProcessRequested(ServiceName = ServiceEnum.SpheresClosingGen)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.CLO, SysNumber = 4010, SysNumber_ObserverMode = 4011)]
            ACCRUALSGEN,
            /// <summary>
            /// Process de génération des instructions de confirmation
            /// </summary>
            [ProcessRequested(ServiceName = ServiceEnum.SpheresConfirmationMsgGen)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.MSG, SysNumber = 2000, SysNumber_ObserverMode = 2001)]
            CIGEN,
            /// <summary>
            /// Process de génération des messages de confirmation
            /// </summary>
            [ProcessRequested(ServiceName = ServiceEnum.SpheresConfirmationMsgGen)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.MSG, SysNumber = 2010, SysNumber_ObserverMode = 2011)]
            CMGEN,
            /// <summary>
            /// Process d'intégration d'une notificiation de CorporateAction
            /// </summary>
            [ProcessRequested(ServiceName = ServiceEnum.SpheresNormMsgFactory)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.EXT, SysNumber = 6100, SysNumber_ObserverMode = 6101)]
            CORPOACTIONINTEGRATE,
            /// <summary>
            /// Process d'intégration d'une notificiation de Closing/Reopening
            /// </summary>
            /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory (Nouveau type de process)
            /// EG 20230918 [WI702] Closing / Reopening module : NormMsgFactory
            [ProcessRequested(ServiceName = ServiceEnum.SpheresNormMsgFactory)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.EXT, SysNumber = 6200, SysNumber_ObserverMode = 6201)] // GLOP EG 20230822 sysnumber
            CLOSINGREOPENINGINTEGRATE,
            /// <summary>
            /// Process de génération des messages de confimation avec déclenchement une tâche IO pour l'émission de la confirmation
            /// </summary>
            ///             
            [ProcessRequested(ServiceName = ServiceEnum.SpheresConfirmationMsgGen)]
            CMGEN_IO,
            /// <summary>
            /// Process de génération  des messages pour les états ( Avis d'opéré, ...)
            /// </summary>
            [ProcessRequested(ServiceName = ServiceEnum.SpheresConfirmationMsgGen)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.MSG, IRQManaged = true, SysNumber = 2020, SysNumber_ObserverMode = 2025 /*, SysNumber_IRQRequest = 7350*/, SysNumber_NMF = 2027)]
            RIMGEN,
            /// <summary>
            /// Process de génération/Regénération des messages pour les états ( Avis d'opéré, ...)
            /// </summary>
            [ProcessRequested(ServiceName = ServiceEnum.SpheresConfirmationMsgGen)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.MSG, SysNumber = 2021, SysNumber_ObserverMode = 2025)]
            RMGEN,
            // Virtual used by Accrued
            CLOSINGGEN,
            /// <summary>
            /// Process de génération des EARs
            /// </summary>
            [ProcessRequested(ServiceName = ServiceEnum.SpheresEarGen)]
            [TrackerSystemMsg(IRQManaged = true, Group = Cst.GroupTrackerEnum.ACC, SysNumber = 3000, SysNumber_ObserverMode = 3001, SysNumber_IRQRequest = 7600)]
            EARGEN,
            /// <summary>
            /// 
            /// </summary>
            [ProcessRequested(ServiceName = ServiceEnum.SpheresSettlementMsgGen)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.MSG, SysNumber = 2110, SysNumber_ObserverMode = 2111)]
            ESRGEN,
            /// <summary>
            /// Génération des nettings par trade
            /// </summary>
            [ProcessRequested(ServiceName = ServiceEnum.SpheresSettlementMsgGen)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.MSG, SysNumber = 2112, SysNumber_ObserverMode = 2113)]
            ESRSTDGEN,
            /// <summary>
            /// Génération des nettings par convention
            /// </summary>
            [ProcessRequested(ServiceName = ServiceEnum.SpheresSettlementMsgGen)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.MSG, SysNumber = 2114, SysNumber_ObserverMode = 2115)]
            ESRNETGEN,
            /// <summary>
            /// Process de génération des événèment
            /// </summary>
            [ProcessRequested(ServiceName = ServiceEnum.SpheresEventsGen)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.TRD, SysNumber = 300, SysNumber_ObserverMode = 301)]
            EVENTSGEN,
            /// <summary>
            /// Process de valorisation des évènements
            /// </summary>
            [ProcessRequested(ServiceName = ServiceEnum.SpheresEventsVal)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.TRD, SysNumber = 310, SysNumber_ObserverMode = 311)]
            EVENTSVAL,

            [ProcessRequested(ServiceName = ServiceEnum.SpheresInvoicingGen)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.INV, SysNumber = 5020, SysNumber_ObserverMode = 5021)]
            INVCANCELSIMUL,
            [ProcessRequested(ServiceName = ServiceEnum.SpheresInvoicingGen)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.INV, SysNumber = 5030, SysNumber_ObserverMode = 5031)]
            INVVALIDSIMUL,
            /// <summary>
            /// 
            /// </summary>
            // EG 20220519 [WI637] Valorisation attribut IRQManaged = true
            [ProcessRequested(ServiceName = ServiceEnum.SpheresInvoicingGen)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.INV, IRQManaged = true, SysNumber = 5000, SysNumber_ObserverMode = 5001, SysNumber_IRQRequest = 7500)]
            INVOICINGGEN,
            /// <summary>
            /// 
            /// </summary>
            [ProcessRequested(ServiceName = ServiceEnum.SpheresIO)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.IO, IRQManaged = true, SysNumber = 1000, SysNumber_ObserverMode = 1010, SysNumber_IRQRequest = 7400, SysNumber_NMF = 1011)]
            IO,
            /// <summary>
            /// 
            /// </summary>
            SHELL,
            [ProcessRequested(ServiceName = ServiceEnum.SpheresClosingGen)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.CLO, SysNumber = 4000, SysNumber_ObserverMode = 4001)]
            LINEARDEPGEN,
            /// <summary>
            /// Process de génération du MarkToMarket
            /// </summary>
            [ProcessRequested(ServiceName = ServiceEnum.SpheresMarkToMarketGen)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.CLO, SysNumber = 4020, SysNumber_ObserverMode = 4021)]
            MTMGEN,
            [ProcessRequested(ServiceName = ServiceEnum.SpheresClosingGen)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.TRD, SysNumber = 195, SysNumber_ObserverMode = 251)]
            POSKEEPENTRY,
            [ProcessRequested(ServiceName = ServiceEnum.SpheresClosingGen)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.CLO, SysNumber = 4200, SysNumber_ObserverMode = 4201)]
            POSKEEPREQUEST,
            /// <summary>
            /// 
            /// </summary>
            [ProcessRequested(ServiceName = ServiceEnum.SpheresQuotationHandling)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.TRD, SysNumber = 400, SysNumber_ObserverMode = 440)]
            QUOTHANDLING,
            /// <summary>
            /// 
            /// </summary>
            SCHEDULER,
            /// <summary>
            /// Process de génération des messages de règlement
            /// </summary>
            [ProcessRequested(ServiceName = ServiceEnum.SpheresSettlementMsgGen)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.MSG, SysNumber = 2110, SysNumber_ObserverMode = 2111)]
            MSOGEN,
            /// <summary>
            /// Process de génération des instructions de règlement
            /// </summary>
            [ProcessRequested(ServiceName = ServiceEnum.SpheresSettlementInstrGen)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.MSG, SysNumber = 2100, SysNumber_ObserverMode = 2101)]
            SIGEN,
            [ProcessRequested(ServiceName = ServiceEnum.SpheresTradeActionGen)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.TRD, SysNumber = 100, SysNumber_ObserverMode = 250)]
            TRADEACTGEN,
            /// <summary>
            /// Process de génération des déposits (Initial Margin Requirement) (Service RiskPerformance)
            /// </summary>
            [ProcessRequested(ServiceName = ServiceEnum.SpheresRiskPerformance)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.CLO, IRQManaged = true, SysNumber = 4050, SysNumber_ObserverMode = 4051, SysNumber_IRQRequest = 7200)]
            RISKPERFORMANCE,
            /// <summary>
            /// Process de génération des soldes (Service RiskPerformance)
            /// </summary>
            [ProcessRequested(ServiceName = ServiceEnum.SpheresRiskPerformance)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.CLO, IRQManaged = true, SysNumber = 4060, SysNumber_ObserverMode = 4061, SysNumber_IRQRequest = 7300, SysNumber_NMF = 4062)]
            CASHBALANCE,
            /// <summary>
            /// Valorisation des dépôts de garantie Titre
            /// </summary>
            [ProcessRequested(ServiceName = ServiceEnum.NA)]
            COLLATERALVAL,
            /// <summary>
            /// 
            /// </summary>
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.EXT, SysNumber = 6000, SysNumber_ObserverMode = 6001)]
            GATEBCS,

            /// <summary>
            /// <para>
            /// Calcul des intérêts sur solde
            /// </para>
            /// <para>
            /// Calcul des intérêts sur espèce en couverture des déposits
            /// </para>
            /// </summary>
            [ProcessRequested(ServiceName = ServiceEnum.SpheresRiskPerformance)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.INV, SysNumber = 4070, SysNumber_ObserverMode = 4071)]
            CASHINTEREST,
            /// <summary>
            /// Process de construction de message normalisés Spheres
            /// </summary>
            [ProcessRequested(ServiceName = ServiceEnum.SpheresNormMsgFactory)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.EXT, SysNumber = 6010, SysNumber_ObserverMode = 6011, SysNumber_Default = 6012)]
            NORMMSGFACTORY,

            /// <summary>
            /// Demande d'interruption d'un traitement
            /// </summary>
            [ProcessRequested(ServiceName = ServiceEnum.SpheresNormMsgFactory)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.EXT, SysNumber = 7000, SysNumber_ObserverMode = 7011)]
            IRQ,

            #region  Process non géré ds les services
            /// <summary>
            /// 
            /// </summary>
            WEBSESSION,
            /// <summary>
            /// Saisi d'un trade (voir CheckAndRecord)
            /// </summary>
            TRADECAPTURE,
            /// <summary>
            /// 
            /// </summary>
            USER,
            /// <summary>
            /// 
            /// </summary>
            RESPONSE,
            /// <summary>
            /// Process de journalisation des actions utilisateurs
            /// </summary>
            REQUESTTRACK,
            /// <summary>
            /// Calcul des dates suite à la mise à jour d'une règle d'échéance sur contrat dérivé
            /// </summary>
            /// JN 20190508
            [ProcessRequested(ServiceName = ServiceEnum.NA)]
            [TrackerSystemMsg(Group = Cst.GroupTrackerEnum.TRD, SysNumber = 400, SysNumber_ObserverMode = 440)]
            MATURITYRULEUPDATE,

            /// <summary>
            /// 
            /// </summary>
            NA,
            #endregion
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public enum ProcessIOType
        {
            /// <summary>
            /// IO uniquement
            /// </summary>
            IO,
            /// <summary>
            /// Process puis IO
            /// </summary>
            ProcessAndIO
        }

        #region  ProcessRequestedAttribute
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
        public sealed class ProcessRequestedAttribute : Attribute
        {
            #region Members
            private ServiceEnum m_ServiceName;
            #endregion Members
            #region Accessors
            public ServiceEnum ServiceName
            {
                get { return (m_ServiceName); }
                set { m_ServiceName = value; }
            }
            #endregion Accessors
        }
        #endregion ProcessRequestedAttribute

        #region  TrackerSysNumberTypeEnum

        /// <summary>
        /// 
        /// </summary>
        /// EG 20180525 [23979] IRQ Processing Add IRQRequest
        /// EG 20190214 Correction messages Tracker pour NormMsgFactory
        /// FI 20190722 [XXXXX] Add TradeDebtSec
        public enum TrackerSysNumberType
        {
            Default,
            MarkToMarket,
            Observer,
            Regular,
            TradeAdmin,
            TradeDebtSec,
            IRQRequest,
            NormMsgFactory,
        }
        #endregion  TrackerSysNumberTypeEnum

        #region  TrackerSystemMsgAttribute
        // EG 20180525 [23979] IRQ Processing Add IRQManaged|SysNumber_IRQRequest
        // EG 20190214 Correction messages Tracker pour NormMsgFactory (SysNumber_NMF)
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
        public sealed class TrackerSystemMsgAttribute : Attribute
        {
            #region Members
            private Cst.GroupTrackerEnum m_Group;
            private bool m_IRQManaged;
            private int m_SysNumber_Default;
            private int m_SysNumber;
            private int m_SysNumber_MTM;
            private int m_SysNumber_TradeAdmin;
            private int m_SysNumber_ObserverMode;
            private int m_SysNumber_IRQRequest;
            private int m_SysNumber_NMF;
            #endregion Members
            #region Accessors
            public Cst.GroupTrackerEnum Group
            {
                get { return m_Group; }
                set { m_Group = value; }
            }
            public bool IRQManaged
            {
                get { return (m_IRQManaged); }
                set { m_IRQManaged = value; }
            }
            public int SysNumber_Default
            {
                get { return m_SysNumber_Default; }
                set { m_SysNumber_Default = value; }
            }

            public int SysNumber
            {
                get { return m_SysNumber; }
                set { m_SysNumber = value; }
            }
            public int SysNumber_MTM
            {
                get { return m_SysNumber_MTM; }
                set { m_SysNumber_MTM = value; }
            }
            public int SysNumber_ObserverMode
            {
                get { return m_SysNumber_ObserverMode; }
                set { m_SysNumber_ObserverMode = value; }
            }
            public int SysNumber_TradeAdmin
            {
                get { return m_SysNumber_TradeAdmin; }
                set { m_SysNumber_TradeAdmin = value; }
            }
            /// <summary>
            /// 
            /// </summary>
            /// FI 20190722 [XXXXX] Add TradeDebtSec
            public int SysNumber_TradeDebtSec
            {
                get;
                set;
            }

            public int SysNumber_IRQRequest
            {
                get { return m_SysNumber_IRQRequest; }
                set { m_SysNumber_IRQRequest = value; }
            }
            public int SysNumber_NMF
            {
                get { return m_SysNumber_NMF; }
                set { m_SysNumber_NMF = value; }
            }

            #endregion Accessors
        }
        #endregion TrackerSystemMsgAttribute

        #region public enum ServiceEnum
        /// <summary>
        /// Type de service
        /// </summary>
        /// Attention conserver l'ordre de l'enum ou penser à redefinir l'ordre des images list de ServiceManager
        public enum ServiceEnum
        {

            SpheresAccountGen,
            SpheresClosingGen,
            SpheresEarGen,
            /// <summary>
            /// Service de génération des évènements
            /// </summary>
            SpheresEventsGen,
            /// <summary>
            /// Service de valorisation des évènements
            /// </summary>
            SpheresEventsVal,
            /// <summary>
            /// Service Input/Output
            /// </summary>
            SpheresIO,
            SpheresMarkToMarketGen,
            SpheresQuotationHandling,
            SpheresSettlementInstrGen,
            SpheresTradeActionGen,
            SpheresScheduler,
            /// <summary>
            /// Service de Messagerie de notification/confirmation
            /// </summary>
            SpheresConfirmationMsgGen,
            /// <summary>
            /// Service de Messagerie de rglt
            /// </summary>
            SpheresSettlementMsgGen,
            NA,
            /// <summary>
            /// Service gateway BCS idem
            /// </summary>
            SpheresGateBCS,
            /// <summary>
            /// service de facturation
            /// </summary>
            SpheresInvoicingGen,
            SpheresShell,
            SpheresResponse,
            SpheresRiskPerformance,
            TestService,
            SpheresGateFIXMLEurex,
            // EG 20121126  - Ticket 18241
            SpheresNormMsgFactory,
            /// <summary>
            /// Service d'analyse des données fournies par l'application web
            /// <para>En 4.1, ce service n'existe pas encore, le message est consommé par un outils tiers</para>
            /// </summary>
            SpheresWebSession,
            /// <summary>
            /// Service de gestion du journal
            /// </summary>
            // PM 20200102 [XXXXX] New Log
            SpheresLogger,
        }
        #endregion ServiceEnum

        #region public enum CheckModeEnum
        /// <summary>
        /// 
        /// </summary>
        /// FI 20171025 [23533] Modify (ResourceAttribut) 
        public enum CheckModeEnum
        {
            [ResourceAttribut(Resource = "CheckModeEnum_None")]
            None,
            [ResourceAttribut(Resource = "CheckModeEnum_Warning")]
            Warning,
            [ResourceAttribut(Resource = "CheckModeEnum_Error")]
            Error,
        }
        #endregion
        #region public enum	ListRetrievalEnum
        public enum ListRetrievalEnum
        {
            /// <summary>
            ///Predefined: eg. country, currency, ... (idem à DDLType du référentiel)
            /// </summary>
            PREDEF,
            /// <summary>
            ///SQL Query avec colonne Text et Value: eg. select IDA as Value, IDENTIFIER as Text from dbo.ACTOR
            /// </summary>
            SQL,
            /// <summary>
            ///Delimited list with semicolon: eg. Buy;Sell; ou Buy|BUYER;Sell|SELLER;
            /// </summary>
            DELIMLIST,
            /// <summary>
            /// XML data with element (TODO)
            /// </summary>
            XML,
        }
        #endregion
        #region public enum	CountryTypeEnum
        public enum CountryTypeEnum
        {
            COUNTRY, LOCAL, SECTOR, UNION,
        }
        #endregion
        #region public enum	MarketTypeEnum
        // PL 20171006 [23469] New values
        public enum MarketTypeEnum
        {
            //MARKET, SEGMENT,
            OPERATING, SEGMENT
        }
        #endregion
        #region public enum	CtrlLastUserEnum
        public enum CtrlLastUserEnum
        {
            INDIFFERENT, IDENTIC, DIFFERENT,
        }
        #endregion
        #region public enum	CtrlStatusEnum
        /// <summary>
        /// EVERY ou ATLEASTONE
        /// </summary>
        public enum CtrlStatusEnum
        {
            /// <summary>
            /// Tous
            /// </summary>
            EVERY,
            /// <summary>
            /// Au moins 1 
            /// </summary>
            ATLEASTONE
        }
        #endregion
        #region public enum	CtrlSendingEnum
        public enum CtrlSendingEnum
        {
            INDIFFERENT, NOSENDING, SENDING,
        }
        #endregion
        #region public enum	StatusBusiness
        public enum StatusBusiness
        {
            //Warning: UNDEFINED n'est pas un statut autorisé (il n'existe pas dans la table ENUM).
            //         Il est présent ici à des fins d'utilisation pour des interrogations non restrictives.
            PRETRADE,
            EXECUTED,
            INTERMED,
            ALLOC,
            UNDEFINED
        }
        #endregion
        #region public enum	StatusEnvironment
        public enum StatusEnvironment
        {
            //Warning: UNDEFINED n'est pas un statut autorisé (il n'existe pas dans la table ENUM).
            //         Il est présent ici à des fins d'utilisation pour des interrogations non restrictives.
            //20100311 PL-StatusBusiness 
            //REGULAR, PRETRADE, SIMUL, TEMPLATE, SYSTEM, UNDEFINED
            REGULAR,
            SIMUL,
            TEMPLATE,
            UNDEFINED
        }
        #endregion
        #region public enum	StatusActivation
        public enum StatusActivation
        {
            REGULAR,
            LOCKED,
            DEACTIV,
            MISSING,
            REMOVED, // Used for Remove EAR
        }
        #endregion
        #region public enum	StatusUsedBy
        public enum StatusUsedBy
        {
            REGULAR,
            RESERVED,
        }
        #endregion
        #region public enum	StatusPriority
        public enum StatusPriority
        {
            REGULAR, LOW, HIGH
        }
        #endregion

        // CC 20120625 17939
        #region public enum	StatusTask
        public enum StatusTask
        {
            ONSUCCESS, ONERROR, ONTERMINATED, ONWARNING, ONUNSUCCESS
        }
        #endregion

        // CC 20120625 17870
        #region public enum	ActorAmountType
        public enum ActorAmountType
        {
            CAPITAL, LIMIT, CAUTION
        }
        #endregion

        #region public enum	TradeBusinessType
        //20100311 PL-StatusBusiness
        //public enum TradeBusinessType
        //{
        //    PRETRADE,
        //    TRADE,
        //    EXECUTION,
        //    INTERMEDIATION,
        //    ALLOCATION
        //}
        #endregion

        #region public enum	ExtlType (External Data Type)
        public enum ExtlType
        {
            EXTLLINK, EXTLLINK2, EXTLATTRB
        }
        #endregion
        #region public enum	UploadTag
        public enum UploadTag
        {
            NONE, PRICE, LOGO
        }
        #endregion
        #region public enum	RoundingDirectionSQL
        //Warning, garder cet enum en phase avec l'enum RoundingDirectionEnum
        public enum RoundingDirectionSQL
        {
            U, D, N
        }
        #endregion
        #region public enum ReturnSPParamTypeEnum
        //GP 20070124. Type de paramètre de retour des Procédure Stockées pour la gestion des tables du Tracker et du Log.
        public enum ReturnSPParamTypeEnum
        {
            NA,
            RETURNCODE,
            RETURNMESSAGE,
            RETURNDATA,
        }
        #endregion
        //
        #region public class TypeMIME
        /// <summary>
        /// Type MIME
        /// </summary>
        /// FI 20160308 [21782] Modify
        // EG 20190411 [ExportFromCSV]
        public sealed class TypeMIME
        {
            public const string ALL = "*/*";
            //liste à amender avec les besoins...
            public struct Text
            {
                public const string ALL = "text/*";
                public const string Plain = "text/plain";      // .txt
                public const string RichText = "text/richtext";
                public const string Html = "text/html";
                public const string Xml = "text/xml";
                public const string Csv = "text/csv";
            }
            public struct Image
            {
                public const string ALL = "image/*";
                public const string Gif = "image/gif";
                public const string Jpeg = "image/jpeg";
                public const string PJpeg = "image/pjpeg";
                public const string Tiff = "image/tiff";
                public const string Png = "image/png";
                public const string XPng = "image/x-png";
                public const string XXBitmap = "image/x-xbitmap";
                public const string Bmp = "image/bmp";
                public const string XJg = "image/x-jg";
                public const string XEmf = "image/x-emf";
                public const string XWmf = "image/x-wmf";

            }
            public struct Application
            {
                public const string ALL = "application/*";
                public const string Postscript = "application/postscript";
                public const string Base64 = "application/base64";
                public const string Macbinhex40 = "application/macbinhex40";
                public const string Pdf = "application/pdf";
                public const string XCompressed = "application/x-compressed";
                public const string XZipCompressed = "application/x-zip-compressed";
                public const string XGzipCompressed = "application/x-gzip-compressed";
                public const string Java = "application/java";
                public const string XMsdownload = "application/x-msdownload";
            }
            public struct Audio
            {
                public const string ALL = "audio/*";
                public const string XAiff = "audio/x-aiff";
                public const string Basic = "audio/basic";
                public const string Wav = "audio/wav";
            }
            public struct Video
            {
                public const string ALL = "video/*";
                public const string Avi = "video/avi";
                public const string Mpeg = "video/mpeg";
            }
            /// <summary>
            /// fichiers spécifiques à certains éditeurs
            /// </summary>
            /// FI 20160308 [21782] Add
            public struct Vnd
            {
                /// <summary>
                /// fichiers Microsoft Excel (.xls).
                /// </summary>
                public const string MsExcel = "application/vnd.ms-excel";
                /// <summary>
                /// feuille de calcul OpenDocument (http://www.iana.org/assignments/media-types/application/vnd.oasis.opendocument.spreadsheet)
                /// </summary>
                public const string spreadsheetml = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            }

        }
        #endregion public class TypeMIME
        //
        #region public enum HashAlgorithm
        public enum HashAlgorithm
        {
            None = 1, MD5 = 2, SHA1 = 3, SHA256 = 4, SHA384 = 5, SHA512 = 6
        }
        #endregion
        #region public enum DispType
        public enum DispType
        {
            display = 1, visibility = 2
        }
        #endregion
        #region public enum ListType
        /// <summary>
        /// Type de 
        /// </summary>
        /// FI 20160804 [Migration TFS] (Repository remplate referential)
        public enum ListType
        {
            //20080313 PL Add Consultation
            //20110314 PL Add Report
            Report = 0,
            Repository = 1,
            Log = 2,
            Price = 3,
            ProcessBase = 5,
            SettlementMsg = 6,
            TRIM = 7,
            Accounting = 8,
            Trade = 9,
            Consultation = 10,
            ConfirmationMsg = 11,
            Invoicing = 12,
            Event = 13,
            EAR = 14,
            Monitoring = 15,
        }
        #endregion
        #region public enum OperatorType
        public enum OperatorType
        {
            add = 1, substract = 2, clone = 3, copy = 4, copyitem = 5, paste = 6, pasteitem = 7, pastechoice = 8, pastechoiceitem = 9,
        }
        #endregion
        #region public enum ErrLevel
        // EG 20180525 [23979] IRQ Processing Add IRQ_EXECUTED
        // EG 20180620 Add REQUEST_REJECTED
        // EG 20190613 [24683] New CLOSINGREOPENINGREJECTED
        // PM 20221020 [25617] Add CONNECTIONFAILED
        public enum ErrLevel
        {
            EXECUTED = -30,   //PL 20101028
            CONTINUE = -23,   //PL 20130701
            STARTED = -22,    //PL 20101028
            START = -21,      //PL 20130701
            CONNECTED = -20,  //PL 20101028
            INITIALIZE = -10, //PL 20101028
            //
            SUCCESS = -1,
            //
            UNDEFINED = 0,
            //
            BREAK = 1,
            STOP = 2,         //PL 20130701
            SQLDEFINED = 5,
            USERDEFINED = 10,
            LOGINUNSUCCESSFUL = 30,
            LOGINSIMULTANEOUS_USER = 31,
            LOGINSIMULTANEOUS_ALLUSER = 32,
            ACCESDENIED = 35,
            NOTCONNECTED = 36,
            LOCKUNSUCCESSFUL = 40,
            /// <summary>
            /// Statut retour d'un process lorsque le paramétrage en vigueur dans PROCESSTUNING n'indique ni de procéder au traitement, ni d'ignorer le traitement
            /// <para>Dans ce cas le traitement s'auto-poste un nouveau message et retente le traitement</para>
            /// </summary>
            TUNING_UNMATCH = 45,
            /// <summary>
            /// Statut retour d'un process lorsque le paramétrage en vigueur dans PROCESSTUNING inidique d'ignorer le traitement
            /// </summary>
            TUNING_IGNORE = 46,
            TUNING_IGNOREFORCED = 47,
            MOM_UNKNOWN = 50,
            MOM_PATH_ERROR = 51,
            INITIALIZE_ERROR = 52,
            MESSAGE_NOTFOUND = 53,
            MESSAGE_MOVE_ERROR = 54,
            MESSAGE_READ_ERROR = 55,
            MESSAGE_CAST_ERROR = 56,
            MESSAGE_NOTCONFORM = 57,
            MESSAGE_DEL_ERROR = 58,
            INCORRECTPARAMETER = 60,
            MISSINGPARAMETER = 61,
            FOLDERNOTFOUND = 70,
            FILENOTFOUND = 71,
            URLNOTFOUND = 72,
            DATANOTFOUND = 73,
            DATAUNMATCH = 74,
            DATADISABLED = 75,
            DATAIGNORE = 76,
            MULTIDATAFOUND = 77,
            DATAREJECTED = 78,
            FAILURE = 80,
            FAILUREWARNING = 81, // EG 20140120 Report 3.7
            TIMEOUT = 82,
            DEADLOCK = 84,
            IRVIOLATION = 86,
            QUOTENOTFOUND = 93,     //New
            QUOTEDISABLED = 95,     //New
            MULTIQUOTEFOUND = 97,   //New
            ABORTED = 99,
            NOBOOKMANAGED = 120,
            XMLDOCUMENT_NOTCONFORM = 200,
            STOREDPROCEDURE_EXCEPTION = 210,
            STOREDPROCEDURE_ERROR = 211,
            SERVICE_STOPPED = 300,
            EMAILNOTSENT = 350,
            EMAILNOTDELIVERED = 351,
            FLYDOCNOTSENT = 352,
            ATTACHFILEERROR = 353,
            SQL_ERROR = 400,
            CS_OBJECTREFNOTSET = 500,   //New
            NOTHINGTODO = 600,
            RFACTOR_NOTCONFORM = 700, // New
            ENTITYMARKET_UNMANAGED = 710, // New
            IRQ_EXECUTED = 720,
            REQUEST_REJECTED = 730, // New
            CLOSINGREOPENINGREJECTED = 740, // New
            CMECONNECTIONFAILED = 800,
        }
        #endregion

        #region public enum IRQLevel
        // EG 20180525 [23979] IRQ Processing New
        public enum IRQLevel
        {
            REQUESTED = 10,
            REJECTED = 20,
            EXECUTED = 30,
            NAMEDSEMAPHORE_EXISTS = 40,
            NAMEDSEMAPHORE_NOTEXISTS = 41,
            NAMEDSEMAPHORE_UNAUTHORIZED = 42,
        }
        #endregion public enum IRQLevel

        #region public enum DataGridMode
        /// <summary>
        /// Mode de fonctionnement du Grid
        /// </summary>
        public enum DataGridMode
        {
            /// <summary>
            /// Mode sélection via formulaire
            /// <para>Un double-click sur une ligne, ou la loupe, ouvre, dans un nouvel onglet, un formulaire en mode "validation"</para>
            /// </summary>
            FormSelect = 0,

            /// <summary>
            /// Mode sélection via datagrid
            /// <para>Un double-click sur une ligne ferme le datagrid</para>
            /// <para>(ie aide à la saisie => btn 3 pts)</para>
            /// </summary>
            GridSelect = 1,

            /// <summary>
            /// Mode saisie via formulaire
            /// <para>Un double-click sur une ligne, ou la loupe, ouvre, dans un nouvel onglet, un formulaire en mode "saisie"</para>
            /// </summary>
            FormInput = 2,

            /// <summary>
            /// Mode saisie via datagrid
            /// <para>Un bouton Edit est disponible sur chaque ligne afin d'entrer en mode saisie directement sur la ligne</para>
            /// <para>Warning: mode plus ou moins obsolète (20111110)</para>
            /// </summary>
            GridInput = 3,

            /// <summary>
            /// ???????????
            /// </summary>
            LocalGridSelect = 4,

            /// <summary>
            /// Mode consultation via formulaire
            /// <para>Un double-click sur une ligne, ou la loupe, ouvre, dans un nouvel onglet, un formulaire en mode "consultation"</para>
            /// </summary>
            FormViewer = 5,
        }
        #endregion
        #region public enum ConsultationMode
        /// <summary>
        /// Mode de fonctionnement de la form de saisie
        /// </summary>
        public enum ConsultationMode
        {
            /// <summary>
            /// Spheres® affiche le formulaire en mode saisie. 
            /// <para>Boutons disponibles: Record,Cancel,Duplicate,Remove</para>
            /// </summary>
            Normal = 0,
            /// <summary>
            /// Spheres® affiche le formulaire en mode readonly (pour validation)
            /// <para>Boutons disponibles: Validate,Cancel</para>
            /// </summary>
            Select = 1,
            /// <summary>
            /// Spheres® affiche le formulaire en mode readonly (pour consultation)
            /// <para>Boutons disponibles: Ok</para>
            /// </summary>
            ReadOnly = 2,
            /// <summary>
            /// Spheres® affiche le formulaire en mode readonly (pour consultation), mais sans les menus enfants
            /// <para>Boutons disponibles: Ok</para>
            /// </summary>
            ReadOnlyWithoutChildren = 3,
            /// <summary>
            /// Spheres® affiche le formulaire: 
            /// en mode readonly pour les zones non updatable 
            /// en mode normal pour les zones updatable
            /// </summary>
            PartialReadOnly = 4,
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public enum ClearingMemberTypeEnum
        {
            /// <summary>
            /// Direct Clearing Member
            /// </summary>
            DCM,
            /// <summary>
            /// General Clearing Member
            /// </summary>
            GCM,
            /// <summary>
            /// Non Clearing Member
            /// </summary>
            NCM
        }

        #region public enum UnderlyingAsset
        /// <summary>
        /// type d'asset sous-jacent 
        /// <para>Issu de de Fpml (ComplexType:Asset)</para>
        /// http://svr-rd01/fpml52_help/fpml-5-2-1-wd-1-confirmation_doc/schemaRef/fpml-asset-5-2.xsd.html_type_Asset.html
        /// </summary>
        /// EG 20140702 Power2 
        /// PL 20231222 [WI789] New "*Future" virtual values and combined values 
        ///                     WARNING: les valeurs virtuelles sont réservés aux Futures 
        ///                     WARNING: le separateur des valeurs combinés doit impérativement être l'underscore, car il est utilisé dans les Queries SQL
        public enum UnderlyingAsset : long
        {
            Bond = 1,
            Cash = 2,
            Commodity = 4,
            ConvertibleBond = 8,
            Deposit = 16,
            EquityAsset = 32,
            ExchangeTradedContract = 64,
            ExchangeTradedFund = 256,
            Future = 512,
            FxRateAsset = 1024,
            Index = 2048,
            MutualFund = 4096,
            RateIndex = 8192,
            SimpleCreditDefaultSwap = 16384,
            SimpleFra = 32768,
            SimpleIRSwap = 65536,
            //=================================================================================================
            //Catégories virtuelles (Catégories composées pour les Options sur Future. Usage réservé aux Frais)
            //-------------------------------------------------------------------------------------------------
            BondFuture = 131072,
            CommodityFuture = 262144,
            EquityAssetFuture = 524288,
            FxRateAssetFuture = 1048576,
            IndexFuture = 2097152,
            RateIndexFuture = 4194304,
            //=================================================================================================
            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
            //Catégories combinées (Usage réservé aux Frais)
            //-------------------------------------------------------------------------------------------------
            Bond_BondFuture = Bond + BondFuture, 
            Commodity_CommodityFuture = Commodity + CommodityFuture, 
            EquityAsset_EquityAssetFuture = EquityAsset + EquityAssetFuture, 
            FxRateAsset_FxRateAssetFuture = FxRateAsset + FxRateAssetFuture, 
            Index_IndexFuture = Index + IndexFuture, 
            RateIndex_RateIndexFuture = RateIndex + RateIndexFuture,
            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        }
        #endregion
        #region public enum UnderlyingAsset_Rate
        /// <summary>
        /// Type d'asset sous-jacent de class taux
        /// http://svr-rd01/fpml52_help/fpml-5-2-1-wd-1-confirmation_doc/schemaRef/fpml-asset-5-2.xsd.html
        /// </summary>
        public enum UnderlyingAsset_Rate
        {
            Bond, Deposit, Future, RateIndex, SimpleFra, SimpleIRSwap
        }
        #endregion
        #region public enum UnderlyingAsset_ETD
        /// <summary>
        /// Type d'asset sous-jacent pour les ETDs 
        /// </summary>
        public enum UnderlyingAsset_ETD
        {
            //PL 20130110 Add ExchangeTradedFund for CBOE options (see also PM)
            Bond, Commodity, EquityAsset, ExchangeTradedFund, Future, FxRateAsset, Index, RateIndex
        }
        #endregion

        #region public enum WeeklyRollConvention
        public enum WeeklyRollConvention
        {
            MON = 1,
            TUE = 2,
            WED = 3,
            THU = 4,
            FRI = 5,
            SAT = 6,
            SUN = 7
        }

        #endregion
        #region public enum HolidayName
        public enum HolidayName
        {
            GOODFRIDAY,
            EASTER,
            EASTERMONDAY,
            ASCENSION,
            PENTECOST,
            PENTECOSTMONDAY,
            CORPUSCHRISTI,
            MIDSUMMEREVE,
            MIDSUMMERDAY,
            //KODOMO,
            //KEIRO,
            //SHUBUN,
            //BUNKA
        }
        #endregion
        #region public enum HolidayMethodOfAdjustment
        public enum HolidayMethodOfAdjustment
        {
            NONE,
            ADJ_TO_MONDAY_IF_SUNDAY,
            ADJ_TO_FRIDAY_IF_SATURDAY,

            ADD_TWO_DAY_IF_HOLIDAYWEEKLY,

            ADJ_TO_NEXTDAY_IF_HOLIDAY,
            ADJ_TO_NEXTDAY_IF_HOLIDAYWEEKLY,

            //ADJ_TO_NEARESTDAY_IF_HOLIDAY, 
            ADJ_TO_NEARESTDAY_IF_HOLIDAYWEEKLY,
        }
        #endregion
        #region public enum HolidayType
        public enum HolidayType
        {
            HOLIDAYWEEKLY,
            HOLIDAYMONTHLY,
            HOLIDAYYEARLY,
            HOLIDAYCALCULATED,
            HOLIDAYMISC
        }
        #endregion
        #region public enum PositionDay
        public enum PositionDay
        {
            FIRST = 1,
            SECOND = 2,
            THIRD = 3,
            FOURTH = 4,
            LAST = 99
        }

        #endregion

        #region public enum IndexSelfCompounding
        public enum IndexSelfCompounding
        {
            NONE,
            CASHFLOW,
            ACCRUEDINTEREST,
            VALORISATION
        }
        #endregion
        #region public enum AgregateFunction
        public enum AgregateFunction
        {
            AVGFUNC,
            DIFFFUNC,
            SUMFUNC,
        };
        #endregion
        #region public enum FlowTypeEnum
        public enum FlowTypeEnum
        {
            ALL,
            CASH_FLOWS,
            CLOSING,
        };
        #endregion FlowTypeEnum

        #region public enum LabelPositionEnum
        public enum LabelPositionEnum
        {
            PREFIX,
            SUFFIX,
        }
        #endregion LabelPositionEnum

        /// <summary>
        /// 
        /// </summary>
        /// FI 20220601 [XXXXX] Add
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
        public class MaturityMonthYearFmtAttribute : RegExAttribute
        {
        }

        /// <summary>
        /// Représente les formats d'échéance de fix®
        /// </summary>
        public enum MaturityMonthYearFmtEnum
        {
            /// <summary>
            /// Format YYYYMM
            /// </summary>
            [MaturityMonthYearFmtAttribute(RegExPattern = @"^(\d{4})(0[1-9]|1[012])$")]
            [System.Xml.Serialization.XmlEnumAttribute("0")]
            YearMonthOnly = 0,
            /// <summary>
            /// Format YYYYMMDD
            /// </summary>
            [MaturityMonthYearFmtAttribute(RegExPattern = @"^(\d{4})(0[1-9]|1[012])(0[1-9]|1\d|2\d|3[01])$")]
            [System.Xml.Serialization.XmlEnumAttribute("1")]
            YearMonthDay = 1,
            /// <summary>
            /// Format YYYYMMwN
            /// </summary>
            [MaturityMonthYearFmtAttribute(RegExPattern = @"^(\d{4})(0[1-9]|1[012])w([12345])$")]
            [System.Xml.Serialization.XmlEnumAttribute("2")]
            YearMonthWeek = 2
        }

        /// <summary>
        /// 
        /// </summary>
        public enum MaturityMonthYearIncrUnitEnum
        {
            Months = 0,
            Days = 1,
            Weeks = 2,
            Years = 3
        }

        #region Capture Constants (Form Element Names)
        // Culture
        public const string FpMLCulture = "en-US";
        public const string FrenchCulture = "fr-FR";
        public const string EnglishCulture = "en-GB";
        public const string ItalianCulture = "it-IT";
        public const string FrenchCultureThousandsSeparator = " "; // space 
        public const string NonBreakSpace = " "; // Attention this isn't a space character but a non break space (Alt 0160)
        public const char FpMLTraderSeparator = '~';
        //
        public const int WidthDate = 90;
        public const int WidthDateTime = 150;
        public const int WidthTime = 60;
        public const int WidthNumeric = 150;
        public const int WidthBool = 1;
        public const int WidthText = 450;

        public enum ExchangeRateType
        {
            SPOT,
            FORWARDPTS,
            EXCHANGE,
            NONE
        }

        public enum FxLegType
        {
            LIGHT,
            RICH,
            NONE
        }


        #endregion Capture Constants (Form Element Names)

        #region Constant for using from script (Import, Export, Xml, ...)
        /// <summary>
        /// Constantes 'dynamiques' utilisées pour les referenciels/consultations
        /// </summary>
        public const string IDEFSSOFTWARE = "%%IDEFSSOFTWARE%%";  //IDEFSSOFTWARE of current sofware
        public const string SESSIONID = "%%SESSIONID%%";      //SESSION.GUID  of current user connected
        public const string SHORTSESSIONID = "%%SHORTSESSIONID%%";      //SESSION.GUID  of current user connected (the first 10 characters)
        public const string PARENT_IDENTIFIER_USER = "%%PARENT_IDENTIFIER_USER%%";//IDENTIFIER of parent of current user connected
        public const string PARENT_BIC_USER = "%%PARENT_BIC_USER%%";//BIC of parent of current user connected
        public const string PARENT_BIC4_USER = "%%PARENT_BIC4_USER%%";
        public const string PARENT_BIC6_USER = "%%PARENT_BIC6_USER%%";
        public const string PARENT_BIC8_USER = "%%PARENT_BIC8_USER%%";
        public const string PARENT_IDA_USER = "%%PARENT_IDA_USER%%";//IDA of parent of current user connected
        public const string IDENTIFIER_USER = "%%IDENTIFIER_USER%%";//IDENTIFIER of current user connected
        public const string IDA_USER = "%%ID_USER%%";        //IDA of current user connected
        public const string ROLE_USER = "%%ROLE_USER%%";      //Role of current user connected
        public const string IDA_ENTITY = "%%IDA_ENTITY%%";     //ENTITY of the User
        public const string IDA_ANCESTOR = "%%ID_ANCESTOR%%";    //IDAs of ancestors
        public const string EXTLLINK_USER = "%%EXTLLINK_USER%%";  //EXTLLINK of current user connected
        public const string CULTURE_USER_EFSCHAR1 = "%%CULTURE_USER_EFS_ALPHA1%%";     //Culture of current user connected (format EFS.LANG_APPLI : F,I,D ou A)
        public const string CULTURE_USER_ISOCHAR2 = "%%CULTURE_USER_ISO3166_ALPHA2%%"; //Culture of current user connected (format ISO3166 ALPHA2)
        public const string CULTURE_USER_ISOCHAR3 = "%%CULTURE_USER_ISO3166_ALPHA3%%"; //Culture of current user connected (format ISO3166 ALPHA3)

        public const string FOREIGN_KEY = "%%FOREIGN_KEY%%";     //Foreign key of current record
        public const string COLUMN_VALUE = "%%COLUMN_VALUE%%";    //Column value XXX of current record
        public const string MODE_NEWRECORD = "%%MODE_NEWRECORD%%";  //Flag of inserted mode
        // EG 202201102 New utilisée dans d'éventuelles sous-select pour intégrer la clause WHERE de base (DATAKEY).
        public const string WHEREDATAKEY = "%%WHEREDATAKEY%%";    // Utilisée pour intégrer la clause where du DATAKEY spécifié

        public const string SELECT_COLUMN = "%%SELECT_COLUMN%%";

        public const string PARAM_START = "%%PARAM";//Parameter in URL
        public const string PARAM_END = "%%";       //Parameter in URL

        public const string DA_START2 = "%%";
        public const string DA_DEFAULT = "%%DA_DEFAULT%%"; //20081120 PL Add

        public const string DA_START = "%%DA:";     //Dynamic argument
        public const string DA_END = "%%";          //Dynamic argument

        //PL 20170403 [23015]
        public const string EXCLUDEDVALUESFORFEES = "%%R:EXCLUDEDVALUESFORFEES%%";    //See also TrdType_ExcludedValuesForFees_ETD

        public const string SESSIONRESTRICT = "SESSIONRESTRICT";

        // RD 20220211 [25943]
        public const string PWDFORBIDDENLIST_ALL_ID = "%%ALL_ID%%";
        public const string PWDFORBIDDENLIST_USER_ID = "%%USER_ID%%";
        public const string PWDFORBIDDENLIST_PERSONAL_ID = "%%PERSONAL_ID%%";
        public const string PWDFORBIDDENLIST_ADDRESS_ID = "%%ADDRESS_ID%%";
        public const string PWDFORBIDDENLIST_ENTITY_ID = "%%ENTITY_ID%%";
        public const string PWDFORBIDDENLIST_PARENTS_ID = "%%PARENTS_ID%%";

        /// <summary>
        /// 
        /// </summary>
        public const string SR_START = "%%SR:";
        public const string SR_END = "%%";

        public const string SR_TRADE_JOIN = "%%SR:TRADE_JOIN%%";
        public const string SR_TRADE_WHERE_PREDICATE = "%%SR:TRADE_WHERE_PREDICATE%%";

        /// FI 20141107 [20441] 
        public const string SR_TRADEALLOC_JOIN = "%%SR:TRADEALLOC_JOIN%%";
        /// FI 20141107 [20441] 
        public const string SR_TRADEALLOC_WHERE_PREDICATE = "%%SR:TRADEALLOC_WHERE_PREDICATE%%";

        /// FI 20141107 [20441] 
        public const string SR_TRADERISK_JOIN = "%%SR:TRADERISK_JOIN%%";
        /// FI 20141107 [20441] 
        public const string SR_TRADERISK_WHERE_PREDICATE = "%%SR:TRADERISK_WHERE_PREDICATE%%";


        public const string SR_TRADEDEBTSEC_JOIN = "%%SR:TRADEDEBTSEC_JOIN%%";
        public const string SR_TRADEDEBTSEC_WHERE_PREDICATE = "%%SR:TRADEDEBTSEC_WHERE_PREDICATE%%";

        public const string SR_TRADEADMIN_JOIN = "%%SR:TRADEADMIN_JOIN%%";
        public const string SR_TRADEADMIN_WHERE_PREDICATE = "%%SR:TRADEADMIN_WHERE_PREDICATE%%";

        public const string SR_POSCOLLATERAL_JOIN = "%%SR:POSCOLLATERAL_JOIN%%";
        public const string SR_POSCOLLATERAL_WHERE_PREDICATE = "%%SR:POSCOLLATERAL_WHERE_PREDICATE%%";

        public const string SR_MARKET_JOIN = "%%SR:MARKET_JOIN%%";
        public const string SR_INSTR_JOIN = "%%SR:INSTR_JOIN%%";
        public const string SR_ACTOR_JOIN = "%%SR:ACTOR_JOIN%%";
        public const string SR_IOTASK_JOIN = "%%SR:IOTASK_JOIN%%";
        public const string SR_POSREQUEST_SELECT = "%%SR:POSREQUEST_SELECT%%";

        /// <summary>
        /// DH: Data Helper Start
        /// </summary>
        public const string DH_START = "%%DH:";
        /// <summary>
        /// DH: Data Helper End
        /// </summary>
        public const string DH_END = "%%";
        /// <summary>
        /// 
        /// </summary>
        public const string DH_DTENABLED_WHERE_PREDICATE = "%%DH:DTENABLED_WHERE_PREDICATE%%";
        /// <summary>
        /// 
        /// </summary>
        public const string DH_FIRSTDAY_OF_MONTH = "%%DH:FIRSTDAY_OF_MONTH%%";
        /// <summary>
        /// 
        /// </summary>
        public const string DH_LASTDAY_OF_MONTH = "%%DH:LASTDAY_OF_MONTH%%";
        /// <summary>
        /// SQL conditionnel
        /// </summary>
        public const string DH_SQLIF = "%%DH:SQLIF%%";
        /// <summary>
        ///  Applique les citères sur les colonnes GPRODUCT , TYPEINSTR , IDINSTR
        /// </summary>
        /// FI 20180502 [23926] Add
        public const string DH_SQLInstrCriteria = "%%DH:SQLInstrCriteria%%";
        /// <summary>
        ///  Applique les citères sur les colonnes GPRODUCT , TYPEINSTR , IDINSTR
        /// </summary>
        /// FI 20180502 [23926] Add TYPEINSTR_UNL et IDINSTR_UNL
        public const string DH_SQLInstrUnlCriteria = "%%DH:SQLInstrUnlCriteria%%";
        /// <summary>
        ///  Applique les citères sur les colonnes  TYPECONTRACT , IDCONTRACT
        /// </summary>
        /// FI 20180502 [23926] Add
        public const string DH_SQLContractCriteria = "%%DH:SQLContractCriteria%%";

        /// <summary>
        /// CC: Consultation Criteria Start
        /// <para>Représente des Mots clés présents dans certaines requêtes SQL et qui seront remplacé par des expressions SQL générées en fonction des critères spécifiés par l'utilisateur</para>
        /// </summary>
        public const string CC_START = "%%CC:";
        /// <summary>
        /// CC: Consultation Criteria End
        /// </summary>
        public const string CC_END = "%%";

        /// <summary>
        /// Jointures qui s'appliquent à la table ITRADE
        /// <para>I pour interface</para>
        /// <para>ITRADE représente toute table qui contient la colonne DTBUSINESS, IDA_DEALER, IDB_DEALER, IDB_DEALER, IDB_CLEARER, IDM, IDASSET, IDA_CSSCUSTODIAN</para>
        /// </summary>
        public const string CC_ITRADE_JOIN = "%%CC:ITRADE_JOIN%%";
        public const string CC_ITRADE_WHERE_PREDICATE = "%%CC:ITRADE_WHERE_PREDICATE%%";

        public const string IDMARKETENV_DEFAULT = "%%IDMARKETENV_DEFAULT%%";
        public const string IDVALSCENARIO_DEFAULT = "%%IDVALSCENARIO_DEFAULT%%";
        //
        public const string TRD_IDA_BUYER = "%%ID_BUYER%%";    //IDA of buyer
        public const string TRD_IDA_SELLER = "%%ID_SELLER%%";  //IDA of seller
        public const string TRD_IDI_INSTRUMENTMASTER = "%%ID_INSTRUMENT%%";  //IDI of master instrument
        public const string TRD_IDI_INSTRUMENTUNDERLYER = "%%ID_INSTRUMENT_UNDERLYER%%";  //IDI of underlyer instrument
        public const string TRD_ID_UNDERLYER = "%%ID_UNDERLYER%%";  //ID of underlyer Asset
        public const string TRD_CURRENCY = "%%CURRENCY%%";    //IDC of first stream or IDC1 of first leg
        public const string TRD_CURRENCY1 = "%%CURRENCY1%%";  //IDC of first stream or IDC1 of first leg
        public const string TRD_CURRENCY2 = "%%CURRENCY2%%";  //IDC of first stream or IDC2 of first leg        
        public const string TRD_MARKET = "%%MARKET%%";  //IDENTIFIER of trading market 
        public const string TRD_DERIVATIVE_CONTRACT = "%%DERIVATIVE_CONTRACT%%";  //IDENTIFIER of exchange traded derivative contract

        public const string TRD_IDA_BROKER = "%%ID_BROKER%%";  //IDAs of brokers
        public const string TRD_IDA_BROKER_BUYER = "%%ID_BROKER_BUYER%%";  //IDA of broker of buyer
        public const string TRD_IDA_BROKER_SELLER = "%%ID_BROKER_SELLER%%";  //IDA of broker of seller
        public const string TRD_IDA_TRADER_BUYER = "%%ID_TRADER_BUYER%%";
        public const string TRD_IDA_TRADER_SELLER = "%%ID_TRADER_SELLER%%";
        public const string TRD_IDA_SALES_BUYER = "%%ID_SALES_BUYER%%";
        public const string TRD_IDA_SALES_SELLER = "%%ID_SALES_SELLER%%";
        public const string TRD_IDA_PARTY1 = "%%ID_PARTY1%%";
        public const string TRD_IDA_PARTY2 = "%%ID_PARTY2%%";

        public const string TRD_IDENTIFIER_ENTITY = "%%IDENTIFIER_ENTITY%%";//IDENTIFIER de l'entité 
        public const string TRD_EXTLLINK_ENTITY = "%%EXTLLINK_ENTITY%%";//EXTLLINK de l'entité 
        public const string TRD_EXTLLINKENTITY_ENTITY = "%%EXTLLINKENTITY_ENTITY%%";//EXTLLINK du référentiel ENTITY de l'entité 
        public const string TRD_BIC_ENTITY = "%%BIC_ENTITY%%";//BIC de l'entité 
        public const string TRD_BIC4_ENTITY = "%%BIC4_ENTITY%%";
        public const string TRD_BIC6_ENTITY = "%%BIC6_ENTITY%%";
        public const string TRD_BIC8_ENTITY = "%%BIC8_ENTITY%%";

        public const string TRANSACTDATE = "%%TRANSACTDATE%%";
        public const string BUSINESSDATE = "%%BUSINESSDATE%%";
        // EG 20190613 [24683] New
        public const string CLOSINGREOPENING = "%%CLOSINGREOPENING%%";
        public const string CORPOACTIONDATE = "%%CORPOACTIONDATE%%";

        // EG 20171113 [23509] New
        public const string TRD_FACILITY = "%%FACILITY%%";

        /// <summary>
        /// 
        /// </summary>
        public const string AUTOMATIC_COMPUTE = "%%AUTOMATIC_COMPUTE%%";
        #endregion

        #region EurosysFeed
        public enum EurosysFeed_FileExtension
        {
            DAT, PA2, PAR, LZH, TXT, ZIP
        }
        public static bool IsDDLTypeEurosysFeed_FileExtension(string pType)
        {
            return (pType == "EurosysFeed_fileextension");
        }
        public enum EurosysFeed_YN
        {
            N, Y
        }
        public enum EurosysFeed_YesNo
        {
            No, Yes
        }
        public static bool IsDDLTypeEurosysFeed_YesNo(string pType)
        {
            return (pType == "EurosysFeed_YesNo");
        }
        #endregion EurosysFeed
        #region EurosysWeb
        public static bool IsDDLTypeEurosysWebCatPropriet(string pType)
        {
            return (pType == "eurosyswebcatpropriet");
        }
        #endregion EurosysWeb

        #region Enums FpML Capture Validator
        public enum TypeValidator
        {
            None = 0,
            RequireFieldNavigator = 1,
            ExpressionValidator = 2,
            RangeValidator = 3,
            CompareValidator = 4,
            CustomValidator = 5
        };
        #endregion

        #region Enums FpML Capture Validator CompareValidator Type for DataTypeCheck
        public enum DataTypeCheck
        {
            None,
            String,
            Integer,
            Double,
            Date,
            Currency,
        };
        #endregion

        #region public ErrLevelMessage
        public class ErrLevelMessage
        {
            #region Members
            private Cst.ErrLevel errLevel;
            private string message;
            #endregion Members
            #region Accessors
            public Cst.ErrLevel ErrLevel
            {
                set { errLevel = value; }
                get { return errLevel; }
            }
            public string Message
            {
                set { message = value; }
                get { return message; }
            }
            #endregion Accessors
            #region Constructors
            public ErrLevelMessage() { }
            public ErrLevelMessage(Cst.ErrLevel pErrLevel, string pMessage)
            {
                errLevel = pErrLevel;
                message = pMessage;
            }
            #endregion Constructors
        }
        #endregion ErrLevelMessage

        #region public StatusNewsEnum
        //20070116 PL A priori inutilisé
        //		public enum StatusNewsEnum
        //		{
        //			NA, NONE, SUCCESS, ERROR, PENDING, PROGRESS
        //		}
        #endregion

        #region DenOptionActionType
        // EG 20151102 [21465] New
        public enum DenOptionActionType
        {
            /* Nouveau dénouement (ou modification du dénouement si unique) avec quantité restante en position */
            newRemaining,
            /* Nouveau dénouement (ou modification du dénouement si unique) avec quantité saisie */
            @new,
            /* Annulation du dénouement */
            remove,
        }
        #endregion DenOptionActionType

        #region General String Constant
        /// <summary>
        /// General String Constant
        /// </summary>
        public const string
            None = "None",
            NotAvailable = "N/A",
            NotFound = "Not Found",

            CalculationRule_EQUIVALENTPERIODICRATE = "EQUIVALENTPERIODICRATE",
            CalculationRule_RELEVANTVALUE = "RELEVANTVALUE",
            CalculationRule_SELFCOMPOUNDING = "SELFCOMPOUNDING",

            PriceCurve_Forward = "FORWARD",
            PriceCurve_Spot = "SPOT",

            RATE = "RATE",
            INDEX = "INDEX",

            RelativeToDT_Start = "CalculationPeriodStartDate",
            RelativeToDT_End = "CalculationPeriodEndDate",
            RelativeToDT_Reset = "ResetDate",

            IdxUnit_percent = "PERCENT",
            IdxUnit_currency = "CURRENCY",
            IdxUnit_degrees = "DEGREES",
            IdxUnit_NA = "N/A",

            RateType_overnight = "OVERNIGHT",
            RateType_regular = "REGULAR",
            RateType_compounding = "COMPOUNDING",

            RateValueType_prospective = "PROSPECTIVE",
            RateValueType_retrospective = "RETROSPECTIVE",

            AccruedInt_Native = "NATIVE",
            AccruedInt_Linear = "LINEAR",
            AccruedInt_Compounding = "COMPOUNDING",
            AccruedInt_Prorata = "PRORATA",

            AccruedIntPeriod_Remaining = "REMAINING",
            AccruedIntPeriod_Accrued = "ACCRUED",

            LinearDepreciation_Remaining = "REMAINING",
            LinearDepreciation_Amortized = "AMORTIZED",

            FxMTMMethod_MTM = "MTM",
            FxMTMMethod_Discount = "DISCOUNT",
            FxMTMMethod_LinearDepreciation = "LINEARDEPRECIATION",

            //ProductSource ----------------------------------------
            //ProductSource_NA = "N/A",

            //ProductSource_ISO = "ISO",
            //ProductSource_ISO15022 = "ISO15022",
            //ProductSource_ISO20022 = "ISO20022",

            //ProductSource_ISDA = "ISDA",
            //ProductSource_FPML = "FpML",
            //ProductSource_FIX = "FIX",
            //ProductSource_FixML = "FixML",
            //ProductSource_Exchange = "Exchange",

            //ProductSource_OTCML = "OTCml",
            //ProductSource_FOML = "F&Oml",
            //ProductSource_EFS = "EFS",

            ///// EG 20161122 New Commodity Derivative
            //ProductSource_EFSML = "EfsML",

            //ProductSource_Internal = "Internal",
            //ProductSource_External = "External",
            //ProductSource ----------------------------------------

            ProductClass_AGREE = "AGREEMENT",
            ProductClass_AUTHOR = "AUTHORISATION",
            ProductClass_REGULAR = "REGULAR",
            ProductClass_NA = "N/A",
            ProductClass_STRATEGY = "STRATEGY",
            // EG 20150410 [20513] BANCAPERTA
            // Bond Option
            ProductFamily_BO = "BO",
            ProductFamily_CD = "CD",
            //DebtSEcurity
            ProductFamily_DSE = "DSE",
            ProductFamily_EQD = "EQD",
            ProductFamily_EQF = "EQF",
            /// EG 20140702 Replace Family EQS            
            ProductFamily_RTS = "RTS",
            ProductFamily_EQCS = "EQVS",
            //EquitySEcurity
            ProductFamily_ESE = "ESE",
            ProductFamily_FIX = "FIX",
            ProductFamily_FX = "FX",
            ProductFamily_INV = "INV",
            ProductFamily_IRD = "IRD",
            ProductFamily_LSD = "LSD",
            ProductFamily_STRATEGY = "STRATEGY",
            ProductFamily_MARGIN = "MARGIN",
            ProductFamily_CASHBALANCE = "CASHBALANCE",
            // FI 20140930 [XXXXX] CASHPAYMENT
            ProductFamily_CASHPAYMENT = "CASHPAYMENT",
            // PM 20120809 [18058] Add ProductFamily_CASHINTEREST
            ProductFamily_CASHINTEREST = "CASHINTEREST",
            ProductFamily_NA = "N/A",
            // EG 20161122 New Commodity Derivative
            ProductFamily_COMS = "COMS",
            ProductFamily_COMD = "COMD",

            ProductGProduct_OTC = "OTC",
            ProductGProduct_MTM = "MTM", // EG 20153013 [POC] Virtual
            ProductGProduct_FX = "FX",
            ProductGProduct_COM = "COM",
            ProductGProduct_SEC = "SEC",
            ProductGProduct_FUT = "FUT",
            ProductGProduct_ADM = "ADM",
            ProductGProduct_ASSET = "ASSET",
            ProductGProduct_RISK = "RISK",
            ProductGProduct_NA = "N/A",

            ProductFxSingleLeg = "fxSingleLeg",
            ProductFxSwap = "fxSwap",
            ProductFxSimpleOption = "fxSimpleOption",
            ProductFxDigitalOption = "fxDigitalOption",
            ProductFxBarrierOption = "fxBarrierOption",
            ProductFxAverageRateOption = "fxAverageRateOption",

            ProductSwap = "swap",
            ProductSwaption = "swaption",
            ProductSTGexchangeTradedDerivative = "STGexchangeTradedDerivative",
            ProductInvoice = "invoice",
            ProductAdditionalInvoice = "additionalInvoice",
            ProductCreditNote = "credit",
            ProductInvoiceSettlement = "invoiceSettlement",
            ProductEquitySecurityTransaction = "equitySecurityTransaction",
            ProductRepo = "repo",
            ProductBuyAndSellBack = "buyAndSellBack",
            ProductDebtSecurity = "debtSecurity",
            ProductDebtSecurityTransaction = "debtSecurityTransaction",
            ProductMarginRequirement = "marginRequirement",
            ProductCashBalance = "cashBalance",
            ProductCashPayment = "cashPayment",
            ProductCollateral = "collateral",
            ProductCashBalanceInterest = "cashBalanceInterest",

            // EG 20161122 New Commodity Derivative
            ProductCommoditySpot = "commoditySpot",
            ProductCommoditySwap = "commoditySwap",

            ProductExchangeTradedDerivative = "exchangeTradedDerivative",
            //PL 20170403 [23015] New
            /*
             *   42: PortfolioTransfer
             *   45: OptionExercise
             * 1000: PositionOpening  
             * 1001: Cascading  
             * 1002: Shifting
             * 1003: CorporateAction  
            */
            // EG 20190613 [24683] New
            /*
             * 63: TechnicalTrade
            */
            TrdType_ExcludedValuesForFees_ETD = "'1000', '1001', '1002', '1003', '42', '45', '63'",
            TrdType_ExcludedValuesForFees_OTC = "'1000', '42', '63'";
        #endregion
        #region General String Constant
        public const string
            SYSTEM_EXCEPTION = "Exception type: System",
            //
            CULTURE_SEPARATOR = "----------------------------------------------",
            StringForTextBoxModePassword = "$~EFS~Password~$",
            Space = " ",
            Space2 = "  ",
            Space4 = "    ",
            CrLf = "\r\n",
            CrLf2 = "\r\n\r\n",
            Cr = "\r",
            Lf = "\n",
            Tab = "\t",
            Tab2 = "\t\t",
            Tab3 = "\t\t\t",
            HTMLSpace = "&nbsp;",
            HTMLSpace2 = "&nbsp;&nbsp;",
            HTMLSpace4 = "&nbsp;&nbsp;&nbsp;&nbsp;",
            HTMLBold = "<b>",
            HTMLEndBold = "</b>",
            HTMLBreakLine = "<br/>",
            HTMLBreakLine2 = "<br/><br/>",
            HTMLHorizontalLine = "<hr/>",
            HTMLtd_Space = @"<td width=""1%"">&nbsp;</td>",
            HTMLOrderedList = "<ol>",
            HTMLEndOrderedList = "</ol>",
            HTMLUnorderedList = "<ul>",
            HTMLEndUnorderedList = "</ul>",
            HTMLListItem = "<li>",
            HTMLEndListItem = "</li>",
            HTMLSpan = "<span>",
            HTMLEndSpan = "</span>",
            //
            SymbolAlign_Left = "L",
            SymbolAlign_Right = "R",
            //
            RowAttribut_Deleted = "D", // Deleted
            RowAttribut_Hidden = "H", // Hidden    
            RowAttribut_System = "S", // System    (Remove and modify not authorized)
            RowAttribut_Protected = "P", // Protected (Remove not authorized)
            RowAttribut_InvoiceClosed = "C"; // Closed     (Remove and modify not authorized)

        //FI 20160804 [Migration TFS] Replace XML_Files par CCIML
        /*
        public const string CustomTradePath = @"XML_Files\CustomTrade";
        public const string CustomTradeAdminPath = @"XML_Files\CustomTradeAdmin";
        public const string CustomTradeRiskPath = @"XML_Files\CustomTradeRisk";
        public const string CustomEventPath = @"XML_Files\CustomEvent";
        */
        public const string CustomTradePath = @"CCIML\CustomTrade";
        public const string CustomTradeAdminPath = @"CCIML\CustomTradeAdmin";
        public const string CustomTradeRiskPath = @"CCIML\CustomTradeRisk";
        public const string CustomEventPath = @"CCIML\CustomEvent";

        public const string UploadPath = @"Upload\";
        public const string STATUSREGULAR = "REGULAR";
        #endregion

        #region Source Log String Constant
        /// <summary>
        /// Nom du journal des évènements des services
        /// </summary>
        public const string SpheresEventLog = "SpheresServices";
        /// <summary>
        /// Nom du journal des évènements des gateways
        /// </summary>
        public const string SpheresGatewayEventLog = "SpheresGateways";
        /// <summary>
        /// 
        /// </summary>
        public const string EventLogSourceExtension = "_L";
        #endregion Source Log String Constant

        #region ItemValue for DDL
        /// <summary>
        /// *
        /// </summary>
        public const string DDLVALUE_ALL_Old = "*";
        /// <summary>
        /// ALL
        /// </summary>
        public const string DDLVALUE_ALL = "ALL";
        /// <summary>
        /// 
        /// </summary>
        /// FI 20121106 add ALL_UNMATCH
        public const string DDLVALUE_ALL_UNMATCH = "ALL_UNMATCH";
        public const string DDLVALUE_DEFAULT = "DEFAULT";
        public const string DDLVALUE_INHERITED = "INHERITED";
        public const string DDLVALUE_NA = "NA";
        public const string DDLVALUE_NONE = "NONE";
        #endregion

        /// <summary>
        /// MissingUTI mode
        /// </summary>
        public enum MissingUTI
        {
            /// <summary>
            ///  Missing UTI on Dealer or Clearer
            /// </summary>
            MISSINGONLY = 0,
            /// <summary>
            ///  Missing UTI on Dealer
            /// </summary>
            MISSINGDEALERONLY = 1,
            /// <summary>
            ///  Missing UTI on Clearer
            /// </summary>
            MISSINGCLEARERONLY = 2,
            /// <summary>
            ///  Sans restriction
            /// </summary>
            UNRESTRICTED = 9,
        };

        #region public enum RuleOnError
        public enum RuleOnError
        {
            ABORT = 0,
            IGNORE = 1,
        };
        #endregion
        #region public enum IOReturnCodeEnum
        // EG 20220221 [XXXXX] New IRQ return code
        public enum IOReturnCodeEnum
        {
            ERROR,
            SUCCESS,
            DEADLOCK,
            TIMEOUT,
            WARNING,
            NA,
            IRQ,
        };
        #endregion
        #region public enum Writemode
        public enum WriteMode
        {
            APPEND,
            WRITE,
        };
        #endregion
        #region public enum Alignment
        /// <summary>
        /// 
        /// </summary>
        /// FI 20171025 [23533] Modify (ResourceAttribut) 
        public enum Alignment
        {
            [ResourceAttribut(Resource = "LEFT")]
            LEFT,
            [ResourceAttribut(Resource = "CENTER")]
            CENTER,
            [ResourceAttribut(Resource = "RIGHT")]
            RIGHT,
        };
        #endregion
        #region public enum DataStyle
        public enum StartInfoStyle
        {
            COMMANDFILE,
            EXECUTABLEFILE,
            SQLFILE,
            STOREDPROCEDURE,
        };

        /// <summary>
        /// Représente les sources de flux en entrée pour une importation
        /// </summary>
        public enum InputSourceDataStyle
        {
            /// <summary>
            ///  Message Queue with ANSI DATA
            /// </summary>
            ANSIDATA,
            /// <summary>
            ///  Message Queue with UNICODE DATA
            /// </summary>
            UNICODEDATA,
            /// <summary>
            ///  Message Queue with XML DATA
            /// </summary>
            XMLDATA,
            /// <summary>
            ///  File with ANSI DATA
            /// </summary>
            ANSIFILE,
            /// <summary>
            ///  File with EBCDIC DATA
            /// </summary>
            EBCDICFILE,
            /// <summary>
            /// EXCEL FILE
            /// </summary>
            /// RD 20100111 [16818] MS Excel® file import
            EXCELFILE,
            /// <summary>
            ///  File with UNICODE DATA
            /// </summary>
            UNICODEFILE,
            /// <summary>
            /// File with XML DATA
            /// </summary>
            XMLFILE,
            /// <summary>
            /// Clearnet-SA Repository
            /// </summary>
            [InputSourceDataStyleAttribute(Description = "Derivatives Data Referential File", IsFilterFile = true)]
            CLEARNETMARKETDATAFILE,
            /// <summary>
            /// ICE Futures Europe Repository
            /// </summary>
            [InputSourceDataStyleAttribute(IsFilterFile = true)]
            ICEFUTURESEUROPEMARKETDATAFILE,
            /// <summary>
            /// Eurex Repository
            /// <para>fichier ctyyyyMMdd.txt</para>
            /// </summary>
            [InputSourceDataStyleAttribute(IsFilterFile = true)]
            EUREXMARKETDATAFILE,
            /// <summary>
            /// Eurex Risk Data (Quotation)
            /// <para>Intégration des cotations présentes dans le fichier Tpn</para>
            /// <para>fichier tpnyyyyMMdd.txt</para>
            /// </summary>
            [InputSourceDataStyleAttribute(IsFilterFile = true)]
            EUREXTHEORETICALPRICEFILE_QUOTE,
            /// <summary>
            /// Eurex Risk Data (Methode RBM)
            /// <para>Intégration des riskData présentes dans le fichier Tpn</para>
            /// <para>fichier tpnyyyyMMdd.txt</para>
            /// </summary>
            [InputSourceDataStyleAttribute(IsDirectImport = true)]
            EUREXTHEORETICALPRICEFILE_RISKARRAY,
            /// <summary>
            /// Eurex Theoretical prices and instrument Configuration (Methode Prisma)
            /// </summary>
            [InputSourceDataStyleAttribute(Description = "Prices and Instrument File", IsPRISMA = true, IsDirectImport = true)]
            EUREXPRISMA_THEORETICALPRICEFILE,
            /// <summary>
            /// Eurex Risk Measure Configuration (Methode Prisma)
            /// </summary>
            [InputSourceDataStyleAttribute(Description = "Risk Measure File", IsPRISMA = true, IsDirectImport = true)]
            EUREXPRISMA_RISKMEASUREFILE,
            /// <summary>
            /// Eurex Risk Risk Measure Aggregation Configuration (Methode Prisma)
            /// </summary>
            [InputSourceDataStyleAttribute(Description = "Risk Measure Aggregation File", IsPRISMA = true, IsDirectImport = true)]
            EUREXPRISMA_RISKMEASUREAGGREGATIONFILE,
            /// <summary>
            /// Eurex Market Capacities Configuration (Methode Prisma)
            /// </summary>
            [InputSourceDataStyleAttribute(Description = "Market Capacities File", IsPRISMA = true, IsDirectImport = true)]
            EUREXPRISMA_MARKETCAPACITIESFILE,
            /// <summary>
            /// Eurex Liquidity Factors Configuration (Methode Prisma)
            /// </summary>
            [InputSourceDataStyleAttribute(Description = "Liquidity Factors File", IsPRISMA = true, IsDirectImport = true)]
            EUREXPRISMA_LIQUIDITYFACTORSFILE,
            /// <summary>
            /// Eurex Foreign Exchange Rate Configuration (Methode Prisma)
            /// </summary>
            [InputSourceDataStyleAttribute(Description = "Foreign Exchange Rates File", IsPRISMA = true, IsDirectImport = true)]
            EUREXPRISMA_FXRATESFILE,
            /// <summary>
            /// Settlement Price (Methode Prisma)
            /// </summary>
            /// FI 20140617 [19911] Add EUREXPRISMA_STLPRICEFILE
            [InputSourceDataStyleAttribute(Description = "Settlement Prices File", IsPRISMA = true, IsDirectImport = true)]
            EUREXPRISMA_STLPRICESFILE,
            /// <summary>
            /// SPAN LIFFE Repository
            /// </summary>
            [InputSourceDataStyleAttribute(IsFilterFile = true)]
            SPANLIFFEMARKETDATAFILE,
            /// <summary>
            /// SPAN LONDON Risk Data
            /// </summary>
            [InputSourceDataStyleAttribute(IsFilterFile = true)]
            LONDONSPANTHEORETICALPRICEFILE,
            /// <summary>
            /// SPAN Risk Data
            /// </summary>
            [InputSourceDataStyleAttribute(IsFilterFile = true)]
            SPANTHEORETICALPRICEFILE,
            /// <summary>
            /// SPAN Risk Data with Maturity Date on asset
            /// </summary>
            /// PM 20160518 [22145] Ajout SPANRISKPARAMETERMATDATEFILE
            [InputSourceDataStyleAttribute(IsFilterFile = true)]
            SPANRISKPARAMETERMATDATEFILE,
            /// <summary>
            /// SPAN XML-based Risk Data
            /// </summary>
            /// PM 20150707 [21104] Add SPANXMLRISKPARAMETERFILE
            [InputSourceDataStyleAttribute(Description = "SPAN XML-based Risk Parameter File", IsDirectImport = true)]
            SPANXMLRISKPARAMETERFILE,
            /// <summary>
            /// CBOE Risk Data
            /// </summary>
            /// BD 20130409 RiskData CBOE
            [InputSourceDataStyleAttribute(IsFilterFile = true)]
            CBOETHEORETICALPRICEFILE,
            /// <summary>
            /// MEFF Asset (CCONTRACTS.ch)
            /// </summary>
            [InputSourceDataStyleAttribute(IsFilterFile = true)]
            MEFFCONTRACTASSETFILE,
            /// <summary>
            /// MEFF Risk Data par Asset (CCONTRSTAT.ch, CTHEORPRICES.ch, CDELTAS.ch)
            /// </summary>
            [InputSourceDataStyleAttribute(IsFilterFile = true)]
            MEFFASSETRISKDATAFILE,
            /// <summary>
            /// T7 Reference Data File (RDF)
            /// </summary>
            /// PM 20171212 [23646] Add T7RDFFIXMLFILE
            [InputSourceDataStyleAttribute(Description = "T7 Reference Data File (RDF)", IsDirectImport = true)]
            T7RDFFIXMLFILE,
            /// <summary>
            /// NASDAX-OMX Curve Correlation Cubes
            /// </summary>
            /// PM 20190222 [24326] Add NASDAQOMXCCTFILE
            [InputSourceDataStyleAttribute(Description = "NASDAX-OMX CCT File")]
            NASDAQOMXCCTFILE,
            /// <summary>
            /// NASDAX-OMX Margin Series Volatilities and Price, Final
            /// </summary>
            /// PM 20190222 [24326] Replace OMXNORDICFMSFILE : OMXNORDIC Quotation (final series prices for the current day)
            [InputSourceDataStyleAttribute(Description = "NASDAX-OMX FMS File", IsFilterFile = true)]
            NASDAQOMXFMSFILE,
            /// <summary>
            /// NASDAX-OMX Risk Cubes for Instrument
            /// </summary>
            /// PM 20190222 [24326] Add NASDAQOMXRCTFILE
            [InputSourceDataStyleAttribute(Description = "NASDAX-OMX RCT File")]
            NASDAQOMXRCTFILE,
            /// <summary>
            /// OMXNORDIC Risk Data (vector file)
            /// </summary>
            /// PM 20190222 [24326] Replace OMXNORDICVCTFILE : OMXNORDIC Risk Data (vector file)
            [InputSourceDataStyleAttribute(Description = "NASDAX-OMX VCT File", IsFilterFile = true)]
            NASDAQOMXVCTFILE,
            /// <summary>
            /// NASDAX-OMX Yield Curve Names
            /// </summary>
            /// PM 20190222 [24326] Add NASDAQOMXYCTFILE
            [InputSourceDataStyleAttribute(Description = "NASDAX-OMX YCT File")]
            NASDAQOMXYCTFILE,
            /// <summary>
            /// Euronext Var Based Method - Parameters TXT File ('RF01' TXT)
            /// </summary>
            /// PM 20230522 [26091] Add EURONEXTVARPARAMETERSTXTFILE
            [InputSourceDataStyleAttribute(Description = "Euronext Var Based Method - Parameters TXT File", IsDirectImport = true)]
            EURONEXTVARPARAMETERSTXTFILE,
            /// <summary>
            /// Euronext Var Based Method - Instruments Scenarios Prices File ('RF02')
            /// </summary>
            /// PM 20230522 [26091] Add EURONEXTVARSCENARIOSFILE
            [InputSourceDataStyleAttribute(Description = "Euronext Var Based Method - Instruments Scenarios Prices File", IsDirectImport = true)]
            EURONEXTVARSCENARIOSFILE,
            /// <summary>
            /// Euronext Var Based Method - Forex Scenario Values File ('RF03')
            /// </summary>
            /// PM 20230522 [26091] Add EURONEXTVARFOREXFILE
            [InputSourceDataStyleAttribute(Description = "Euronext Var Based Method - Forex Scenario Values File")]
            EURONEXTVARFOREXFILE,
            /// <summary>
            /// Euronext Legacy Var Based Method - Equity Model Parameters File ('RF01F')
            /// </summary>
            /// PM 20230522 [26091] Add EURONEXTVARPARAMETERSFILE
            /// PM 20240122 [WI822] Change EURONEXTVARPARAMETERSFILE to EURONEXTLEGACYEQYVARPARAMETERSFILE
            [InputSourceDataStyleAttribute(Description = "Euronext Legacy Var Based Method - Equity Model Parameters File", IsDirectImport = true)]
            EURONEXTLEGACYEQYVARPARAMETERSFILE,
            /// <summary>
            /// Euronext Legacy Var Based Method - Commodity Model Parameters File ('RF01C1')
            /// </summary>
            /// PM 20240122 [WI822] Add EURONEXTLEGACYCOMVARPARAMETERSFILE
            [InputSourceDataStyleAttribute(Description = "Euronext Legacy Var Based Method - Commodity Model Parameters File", IsDirectImport = true)]
            EURONEXTLEGACYCOMVARPARAMETERSFILE,
            /// <summary>
            /// Euronext Legacy Var Based Method - Commodity Model Parameters for Physical Delivery File ('RF01C2')
            /// </summary>
            /// PM 20240122 [WI822] Add EURONEXTLEGACYCOMDLYVARPARAMETERSFILE
            [InputSourceDataStyleAttribute(Description = "Euronext Legacy Var Based Method - Commodity Model Parameters for Physical Delivery File", IsDirectImport = true)]
            EURONEXTLEGACYCOMDLYVARPARAMETERSFILE,
            /// <summary>
            /// Euronext Legacy Var Based Method - Equity Instruments Scenarios Prices File ('RF02F')
            /// </summary>
            /// PM 20240122 [WI822] Add EURONEXTLEGACYEQYVARSCENARIOSFILE
            [InputSourceDataStyleAttribute(Description = "Euronext Legacy Var Based Method - Equity Instruments Scenarios Prices File", IsDirectImport = true)]
            EURONEXTLEGACYEQYVARSCENARIOSFILE,
            /// <summary>
            /// Euronext Legacy Var Based Method - Commodity Instruments Scenarios Prices File ('RF02C1')
            /// </summary>
            /// PM 20240122 [WI822] Add EURONEXTLEGACYCOMVARSCENARIOSFILE
            [InputSourceDataStyleAttribute(Description = "Euronext Legacy Var Based Method - Commodity Instruments Scenarios Prices File", IsDirectImport = true)]
            EURONEXTLEGACYCOMVARSCENARIOSFILE,
            /// <summary>
            /// Euronext Legacy Var Based Method - Commodity Instruments Scenarios Prices for Physical Delivery File ('RF02C2')
            /// </summary>
            /// PM 20240122 [WI822] Add EURONEXTLEGACYCOMDLYVARSCENARIOSFILE
            [InputSourceDataStyleAttribute(Description = "Euronext Legacy Var Based Method - Commodity Instruments Scenarios Prices for Physical Delivery File", IsDirectImport = true)]
            EURONEXTLEGACYCOMDLYVARSCENARIOSFILE,
            /// <summary>
            /// Euronext Legacy Var Based Method - Equity Forex Scenarios Values File ('RF03F')
            /// </summary>
            /// PM 20240122 [WI822] Change EURONEXTVARFOREXFILE to EURONEXTLEGACYEQYVARFOREXFILE
            [InputSourceDataStyleAttribute(Description = "Euronext Legacy Var Based Method - Equity Forex Scenarios Values File", IsDirectImport = true)]
            EURONEXTLEGACYEQYVARFOREXFILE,
            /// <summary>
            /// Euronext Legacy Var Based Method - Commodity Forex Scenarios Values File ('RF03C1')
            /// </summary>
            /// PM 20240122 [WI822] Add EURONEXTLEGACYCOMVARFOREXFILE
            [InputSourceDataStyleAttribute(Description = "Euronext Legacy Var Based Method - Commodity Forex Scenarios Values File", IsDirectImport = true)]
            EURONEXTLEGACYCOMVARFOREXFILE,
            /// <summary>
            /// Euronext Legacy Var Based Method - Commodity Forex Scenarios Values for Physical Delivery File ('RF03C2')
            /// </summary>
            /// PM 20240122 [WI822] Add EURONEXTLEGACYCOMDLYVARFOREXFILE
            [InputSourceDataStyleAttribute(Description = "Euronext Legacy Var Based Method - Commodity Forex Scenarios Values for Physical Delivery File", IsDirectImport = true)]
            EURONEXTLEGACYCOMDLYVARFOREXFILE,
            /// <summary>
            /// Euronext Legacy Var Based Method - Equity Instruments Prices and Referential Data File ('RF04F')
            /// </summary>
            /// PM 20230522 [26091] Add EURONEXTLEGACYEQYVARREFERENTIALDATAFILE
            [InputSourceDataStyleAttribute(Description = "Euronext Legacy Var Based Method - Equity Instruments Prices and Referential Data File", IsDirectImport = true)]
            EURONEXTLEGACYEQYVARREFERENTIALDATAFILE,
            /// <summary>
            /// Euronext Legacy Var Based Method - Commodity Instruments Prices and Referential Data File ('RF04C')
            /// </summary>
            /// PM 20230522 [26091] Add EURONEXTLEGACYEQYVARREFERENTIALDATAFILE
            [InputSourceDataStyleAttribute(Description = "Euronext Legacy Var Based Method - Commodity Instruments Prices and Referential Data File", IsDirectImport = true)]
            EURONEXTLEGACYCOMVARREFERENTIALDATAFILE,
            /// <summary>
            /// Euronext Equity Derivative instruments expiry prices ('RF05F') - Euronext Legacy only
            /// </summary>
            /// PM 20240122 [WI822] Add EURONEXTLEGACYEQYEXPIRYPRICESFILE
            [InputSourceDataStyleAttribute(Description = "Euronext Legacy - Equity Derivative Instruments Expiry Prices File", IsDirectImport = true)]
            EURONEXTLEGACYEQYEXPIRYPRICESFILE,
            /// <summary>
            /// Euronext Equity Derivative instruments expiry prices ('RF05C') - Euronext Legacy only
            /// </summary>
            /// PM 20240122 [WI822] Add EURONEXTLEGACYCOMEXPIRYPRICESFILE
            [InputSourceDataStyleAttribute(Description = "Euronext Legacy - Commodity Derivative Instruments Expiry Prices File", IsDirectImport = true)]
            EURONEXTLEGACYCOMEXPIRYPRICESFILE,
            /// <summary>
            /// Euronext Stock indices values ('RF06F') - Euronext Legacy only
            /// </summary>
            /// PM 20240122 [WI822] Add EURONEXTLEGACYSTOCKINDICESVALUESFILE
            [InputSourceDataStyleAttribute(Description = "Euronext Legacy - Stock Indices Values File", IsDirectImport = true)]
            EURONEXTLEGACYSTOCKINDICESVALUESFILE,
            /// <summary>
            /// Euronext Equity Options deltas ('RF07F') - Euronext Legacy only
            /// </summary>
            /// PM 20240122 [WI822] Add EURONEXTLEGACYEQYOPTIONSDELTASFILE
            [InputSourceDataStyleAttribute(Description = "Euronext Legacy - Equity Options Deltas File", IsDirectImport = true)]
            EURONEXTLEGACYEQYOPTIONSDELTASFILE,
            /// <summary>
            /// Euronext Equity Options deltas ('RF07C') - Euronext Legacy only
            /// </summary>
            /// PM 20240122 [WI822] Add EURONEXTLEGACYCOMOPTIONSDELTASFILE
            [InputSourceDataStyleAttribute(Description = "Euronext Legacy - Equity Options Deltas File", IsDirectImport = true)]
            EURONEXTLEGACYCOMOPTIONSDELTASFILE,
            /// <summary>
            /// Euronext Legacy Var Based Method - Market Calendar File ('RF08C')
            /// </summary>
            /// PM 20240122 [WI822] Add EURONEXTLEGACYVARMARKETCALENDARFILE
            [InputSourceDataStyleAttribute(Description = "Euronext Legacy Var Based Method - Market Calendar File", IsDirectImport = true)]
            EURONEXTLEGACYVARMARKETCALENDARFILE,
        };

        /// <summary>
        /// Représente les différents type d'exportations
        /// </summary>
        public enum OutputSourceDataStyle
        {
            /// <summary>
            /// Exportation à partir d'une table
            /// </summary>
            RDBMSTABLE,
            /// <summary>
            /// Exportation à partir d'une vue
            /// </summary>
            RDBMSVIEW,
            /// <summary>
            /// Exportation à partir d'une stored procedure
            /// </summary>
            RDBMSSP,
            /// <summary>
            /// Exportation à partir d'une consultation
            /// </summary>
            RDBMSLST,
            /// <summary>
            /// Exportation à partir du commande text SQL
            /// </summary>
            /// FI 20130503[] Add 
            RDBMSCOMMAND
        };

        public enum OutputTargetDataStyle
        {
            ANSIFILE,
            EBCDICFILE,
            UNICODEFILE,
            XMLFILE,
            PDFFILE,
            SENDSMTP,
            SENDFLYDOC,
        };
        #endregion
        #region public enum In_Out mode
        // EG 20180525 [23979] IRQ Processing Add IRQManaged|SysNumber_IRQRequest on TrackerSystemMsgAttribute
        // EG 20220221 [XXXXX] Activation IRQManaged on IO
        public enum In_Out
        {
            [TrackerSystemMsg(IRQManaged = true, SysNumber = 1001, SysNumber_IRQRequest = 7401)]
            INPUT = 0,
            [TrackerSystemMsg(IRQManaged = true, SysNumber = 1002, SysNumber_IRQRequest = 7402)]
            OUTPUT = 1,
            [TrackerSystemMsg(IRQManaged = true, SysNumber = 1003, SysNumber_IRQRequest = 7403)]
            COMPARE = 2,
            [TrackerSystemMsg(IRQManaged = true, SysNumber = 1004, SysNumber_IRQRequest = 7404)]
            INPUTOUTPUT = 3,
            [TrackerSystemMsg(IRQManaged = true, SysNumber = 1005, SysNumber_IRQRequest = 7405)]
            INPUTCOMPARE = 4,
            [TrackerSystemMsg(IRQManaged = true, SysNumber = 1000, SysNumber_IRQRequest = 7400)]
            MISC = 5,
        };
        #endregion

        /// <summary>
        /// Commit mode for Process (Input, Output, ...)
        /// </summary>
        /// FI 20171025 [23533] Modify (ResourceAttribut) 
        public enum CommitMode
        {
            /// <summary>
            /// Identique à RECORDCOMMIT
            /// </summary>
            //[ResourceAttribut(Resource ="COMMITMODE_DEFAULT")]
            //DEFAULT = 0,
            [ResourceAttribut(Resource = "COMMITMODE_INHERITED")]
            INHERITED = 0,
            /// <summary>
            ///  Commit à chaque instruction SQL
            /// </summary>
            [ResourceAttribut(Resource = "COMMITMODE_AUTOCOMMIT")]
            AUTOCOMMIT = 1,
            /// <summary>
            /// Commit en fin de record
            /// </summary>
            [ResourceAttribut(Resource = "COMMITMODE_RECORDCOMMIT")]
            RECORDCOMMIT = 2,
            /// <summary>
            /// Commit en fin de traitement
            /// </summary>
            [ResourceAttribut(Resource = "COMMITMODE_FULLCOMMIT")]
            FULLCOMMIT = 3,
        };

        #region public enum IOElementType
        public enum IOElementType
        {
            COMPARE,
            INPUT,
            OUTPUT,
            SHELL,
        }
        #endregion
        #region public enum IOElementType
        public enum IOSerializeMode
        {
            LIGHT,
            NORMAL,
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public enum Visibility
        {
            HIDE,
            MASK,
            SHOW,
        };

        /// <summary>
        /// Afficher/Masquer
        /// </summary>
        /// FI 20190607 [XXXXX] Add
        public enum DisplayMask
        {
            /// <summary>
            /// 
            /// </summary>
            [ResourceAttribut(Resource = "DisplayMask_Display")]
            Display,
            /// <summary>
            /// 
            /// </summary>
            [ResourceAttribut(Resource = "DisplayMask_Mask")]
            Mask
        }

        // CC 20140723
        #region public enum MarginType
        public enum MarginType
        {
            MARGIN,
            LEVERAGE,
        }
        #endregion

        // CC 20150312
        #region public enum MarginingMode
        public enum MarginingMode
        {
            None,
            MarkToMarketAmount,
            PercentageOfNotional,
        }
        #endregion

        // EG 20150319 [POC]
        #region public enum FundingType
        public enum FundingType
        {
            Funding,
            Borrowing,
            FundingAndBorrowing,
        }
        #endregion

        // CC 20140724
        #region public enum MarginAssessmentBasis
        public enum MarginAssessmentBasis
        {
            InitialNotional,
        }
        #endregion

        /// <summary>
        /// unmatch, match, mismatch
        /// </summary>
        public enum MatchEnum
        {
            /// <summary>
            /// Donnée non encore "matchée"
            /// </summary>
            unmatch,
            /// <summary>
            /// Donnée "matchant"
            /// </summary>
            match,
            /// <summary>
            /// Donnée ne "matchant" pas
            /// </summary>
            mismatch,
        };

        #region public enum MatchingMode

        /// <summary>
        /// Comparison templates 
        /// </summary>
        /// <remarks>
        /// (used by the IOCompare module)
        /// </remarks>
        public enum MatchingMode
        {
            /// <summary>
            /// Dynamic grouping (not used yet)
            /// </summary>
            /// <remarks>
            /// Not yet tested
            /// </remarks>
            Dynamic,
            /// <summary>
            /// Match the exact quantity for any single element
            /// </summary>
            Quantity_OneAgainstOne,
            /// <summary>
            /// Match a quantity for grouped elements
            /// </summary>
            SumOfQuantities,
            /// <summary>
            /// Default value
            /// </summary>
            Unknown,
        };

        #endregion

        #region public enum Application Rule of DefaultValue
        public enum RuleOnDefaultValue
        {
            FIRSTRECORD = 0,
        };
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public enum AssessmentBasisEnum
        {
            CallAmount,
            Currency1Amount,
            Currency1AmountForwardLeg,
            Currency1AmountSpotLeg,
            Currency2Amount,
            Currency2AmountForwardLeg,
            Currency2AmountSpotLeg,
            CurrencyReferenceAmount,
            CurrentNotionalAmount,//
            /// <summary>
            /// Assessment basis used on IFTT (Tobin Italia)
            /// </summary>
            ETDNotionalAmount, //PL 20150316 for IFTT (Tobin Italia)
            FaceRate,
            FixedRate,
            //PL 20100121 FolderId,
            //PL 20100121 Formula,
            ForwardRate,
            //PL 20100121 FrontId,
            InitialNotionalAmount,//
            // EG 20150708 [21103] New
            MarketValue,
            /// <summary>
            /// Assiette disponible uniquement sur les debtSecurityTransaction 
            /// <para>si SKP, il s'agit du composant PAM du MKV</para>
            /// <para>sinon il s'agit du composant PAM lors de l'exécution du trade</para>
            /// </summary>
            /// FI 20190918 [XXXXX] Add
            CleanMarketValue,
            //PL 20100121 Line,
            NumberOfPeriodInYear,
            PeriodInDay,//
            PeriodInMonth,//
            PeriodInWeek,//
            PeriodInYear,//
            Premium,
            PremiumRate,
            PutAmount,
            Quantity,
            QuantityContractMultiplier,
            SpotRate,
            SecurityGrossAmount,
            /// <summary>
            /// Assiette disponible uniquement sur les debtSecurityTransaction 
            /// <para>il s'agit du composant PAM lors de l'exécution du trade</para>
            /// </summary>
            /// FI 20190918 [XXXXX] Add
            SecurityCleanAmount,
            SecurityNotionalAmount,
            Strike,
            //PL 20100121 Trade,//
            TradingRate,
            /// <summary>
            /// 
            /// </summary>
            /// FI 20141013 [20418] add TradingPrice 
            TradingPrice,
            // 20120606 Ticket 17863 Assessment basis for strategy ETD - START
            LegsMaxQuantity,
            LegsMinQuantity,
            LegsAvgQuantity,
            LegsMaxQuantityContractMultiplier,
            LegsMinQuantityContractMultiplier,
            LegsAvgQuantityContractMultiplier,
            // 20120606 Ticket 17863 Assessment basis for strategy ETD - END

        };

        /// <summary>
        /// Additional conditions to set fee/brokerage conditions targeting strategies
        /// </summary>
        /// <remarks>these conditions apply only for ETD strategy ORDERS and strategies ETD issued by ALLOCs</remarks>
        public enum FeeStrategyTypeEnum
        {
            /// <summary>
            /// All strategies
            /// </summary>
            ALL,
            /// <summary>
            /// All known strategies
            /// </summary>
            ALLKNOWN,
            /// <summary>
            /// All 2 legs strategies
            /// </summary>
            ALL2LEGS,
            /// <summary>
            /// All 3 legs strategies
            /// </summary>
            ALL3LEGS,
            /// <summary>
            /// All 4 legs strategies
            /// </summary>
            ALL4LEGS,
            /// <summary>
            /// All 4 legs or more strategies
            /// </summary>
            ALL5MORELEGS,
            /// <summary>
            /// All unknown strategies
            /// </summary>
            ALLUNKNOWN,
            /// <summary>
            /// All 2 legs unknown strategies
            /// </summary>
            ALLUNKNOWN2LEGS,
            /// <summary>
            /// All 3 legs unknown strategies
            /// </summary>
            ALLUNKNOWN3LEGS,
            /// <summary>
            /// All 4 legs unknown strategies
            /// </summary>
            ALLUNKNOWN4LEGS,
        }

        /// <summary>
        /// Modalité pour le recalcul des frais 
        /// </summary>
        /// FI 20180328 [23871] Add
        public enum FeesCalculationMode
        {
            /// <summary>
            /// Toutes les modalités
            /// </summary>
            [ResourceAttribut(Resource = "FeesCalculation_ALL")]
            ALL,
            /// <summary>
            /// Frais réglés au quotidien
            /// </summary>
            [ResourceAttribut(Resource = "FeesCalculation_STL")]
            STL,
            /// <summary>
            /// Frais réglés dans le cadre d'une facture
            /// </summary>
            [ResourceAttribut(Resource = "FeesCalculation_INV")]
            INV
        }

        /// <summary>
        /// Etendue d'application d'un barème de frais
        /// </summary>
        public enum FeeScopeEnum
        {
            FolderId,
            OrderId,
            Trade
        };
        public const string TOUCHED_YES = "[Touched: yes]";
        public const string TOUCHED_YESALREADY = "[Touched: yesAlready]";
        public const string TOUCHED_YESPARTIALLY = "[Touched: yesPartially]";
        public const string TOUCHED_NO = "[Touched: no]";

        #region HeaderType
        public enum HeaderType
        {
            [ResourceAttribut(Resource = "None")]
            None,
            [ResourceAttribut(Resource = "Default")]
            Default,
            [ResourceAttribut(Resource = "Logo")]
            Logo,
            [ResourceAttribut(Resource = "CompanyName")]
            CompanyName,
            /// <summary>
            /// Book Published on the DD MMM YYYY at HH:MM:SS
            /// </summary>
            [ResourceAttribut(Resource = "Published_Model1")]
            Published_Model1,
            /// <summary>
            /// Book Published on the DD MMM YYYY at HH:MM:SS (UTC Time)
            /// </summary>
            [ResourceAttribut(Resource = "Published_Model1UTC")]
            Published_Model1UTC,
            /// <summary>
            /// Book Published on the DD MMM YYYY
            /// </summary>
            [ResourceAttribut(Resource = "Published_Model2")]
            Published_Model2,
            /// <summary>
            /// Published on the DD MMM YYYY
            /// </summary>
            [ResourceAttribut(Resource = "Published_Model3")]
            Published_Model3,
            /// <summary>
            /// Actor Invoice Published on the DD MMM YYYY
            /// </summary>
            [ResourceAttribut(Resource = "Published_Model4")]
            Published_Model4,
            /// <summary>
            /// Published on the DD MMM YYYY at HH:MM:SS
            /// </summary>
            [ResourceAttribut(Resource = "Published_Model5")]
            Published_Model5,
            /// <summary>
            /// Published on the DD MMM YYYY at HH:MM:SS (UTC Time)
            /// </summary>
            [ResourceAttribut(Resource = "Published_Model5UTC")]
            Published_Model5UTC,
            /// <summary>
            /// Published on the DD MMM YYYY at HH:MM:SS (City of sender)
            /// </summary>
            [ResourceAttribut(Resource = "Published_Model6")]
            Published_Model6,
            [ResourceAttribut(Resource = "Custom")]
            Custom,
            [ResourceAttribut(Resource = "LegalInfo_Model1")]
            LegalInfo_Model1,
            [ResourceAttribut(Resource = "LegalInfo_Model2")]
            LegalInfo_Model2,
            [ResourceAttribut(Resource = "LegalInfo_Model3")]
            LegalInfo_Model3,
            [ResourceAttribut(Resource = "LegalInfo_Model4")]
            LegalInfo_Model4
        };
        #endregion HeaderType

        #region FooterType
        public enum FooterType
        {
            [ResourceAttribut(Resource = "None")]
            None,
            [ResourceAttribut(Resource = "Default")]
            Default,
            [ResourceAttribut(Resource = "PageNumber")]
            PageNumber,
            [ResourceAttribut(Resource = "LegalInfo_Model1")]
            LegalInfo_Model1,
            [ResourceAttribut(Resource = "LegalInfo_Model2")]
            LegalInfo_Model2,
            [ResourceAttribut(Resource = "LegalInfo_Model3")]
            LegalInfo_Model3,
            [ResourceAttribut(Resource = "LegalInfo_Model4")]
            LegalInfo_Model4,
            [ResourceAttribut(Resource = "Custom")]
            Custom,
        };
        #endregion FooterType

        #region HeaderTitlePosition
        public enum HeaderTitlePosition
        {
            [ResourceAttribut(Resource = "Default")]
            Default,
            [ResourceAttribut(Resource = "Left")]
            Left,
            [ResourceAttribut(Resource = "Center")]
            Center,
            [ResourceAttribut(Resource = "Right")]
            Right
        };
        #endregion HeaderTitlePosition

        #region FooterLegend
        public enum FooterLegend
        {
            [ResourceAttribut(Resource = "Default")]
            Default,
            [ResourceAttribut(Resource = "None")]
            None,
            [ResourceAttribut(Resource = "LastPageOnly")]
            LastPageOnly,
            //AllPages (Reserved for future use)
        };
        #endregion FooterLegend

        #region HeaderFooterSort
        public enum HeaderFooterSort
        {
            [ResourceAttribut(Resource = "Default")]
            Default,
            [ResourceAttribut(Resource = "Book")]
            Book,
            [ResourceAttribut(Resource = "Asset")]
            Asset
        };
        #endregion HeaderFooterSort

        #region HeaderFooterSummary
        public enum HeaderFooterSummary
        {
            [ResourceAttribut(Resource = "None")]
            None,
            [ResourceAttribut(Resource = "Bottom")]
            Bottom
        };
        #endregion HeaderFooterSummary

        #region AmountFormat
        public enum AmountFormat
        {
            [ResourceAttribut(Resource = "Default")]
            Default,
            [ResourceAttribut(Resource = "RightDRCR")]
            RightDRCR,
            [ResourceAttribut(Resource = "LeftDRCR")]
            LeftDRCR,
            [ResourceAttribut(Resource = "RightDR")]
            RightDR,
            [ResourceAttribut(Resource = "LeftDR")]
            LeftDR,
            [ResourceAttribut(Resource = "RightMinusPlus")]
            RightMinusPlus,
            [ResourceAttribut(Resource = "LeftMinusPlus")]
            LeftMinusPlus,
            [ResourceAttribut(Resource = "RightMinus")]
            RightMinus,
            [ResourceAttribut(Resource = "LeftMinus")]
            LeftMinus,
            [ResourceAttribut(Resource = "Parenthesis")]
            Parenthesis
        };
        #endregion AmountFormat

        #region BannerPosition
        public enum BannerAlign
        {
            Left,
            Center,
            Right
        };
        #endregion BannerPosition

        #region AssetBannerStyle
        public enum AssetBannerStyle
        {
            Contract_Model1,
            Contract_Model2,
            Contract_Model3
        };
        #endregion AssetBannerStyle

        #region SectionBannerStyle
        public enum SectionBannerStyle
        {
            None,
            Banner,
            Tab
        };
        #endregion SectionBannerStyle

        #region TradeIdentificationEnum
        public enum TradeIdentificationEnum
        {
            Default,
            ExecutionId,
            TradeId
        }
        #endregion

        #region TimestampType
        /// <summary>
        /// 
        /// </summary>
        /// FI 20150529 [XXXXX] Modify
        public enum TimestampType
        {
            None,
            [System.Xml.Serialization.XmlEnumAttribute("HH24:MI:SS")]
            HH24MISS,
            [System.Xml.Serialization.XmlEnumAttribute("HH24:MI")]
            HH24MI,
            [System.Xml.Serialization.XmlEnumAttribute("H24:MI:SS AM/PM")]
            H24MISS_AMPM,
            [System.Xml.Serialization.XmlEnumAttribute("H24:MI AM/PM")]
            H24MI_AMPM,
            [System.Xml.Serialization.XmlEnumAttribute("HH12:MI:SS AM/PM")]
            HH12MISS_AMPM,
            [System.Xml.Serialization.XmlEnumAttribute("HH12:MI AM/PM")]
            HH12MI_AMPM,
            [System.Xml.Serialization.XmlEnumAttribute("H12:MI:SS AM/PM")]
            H12MISS_AMPM,
            [System.Xml.Serialization.XmlEnumAttribute("H12:MI AM/PM")]
            H12MI_AMPM,
        };
        #endregion TimestampType

        #region UTISummary
        public enum UTISummary
        {
            None,
            TradeOnly,
            All
        };
        #endregion UTISummary

        #region PriceQuoteUnits
        /// <summary>
        /// Specifies the units in which a price is quoted
        /// <para>Issu du scheme FpML http://www.fpml.org/coding-scheme/price-quote-units-1-1.xml</para>
        /// </summary>
        public enum PriceQuoteUnits
        {
            /// <summary>
            /// A price, expressed in percentage of face value as a decimal, e.g. 101.5
            /// </summary>
            ParValueDecimal,
            /* FI 20151202 [21609] Mise en commentaire (cette valeur est non gérée alors autant mettre en commentaire)
            /// <summary>
            /// A price, expressed in percentage of face value with fractions, e.g. 101 3/8. Normally used for quoting bonds
            /// </summary>
            ParValueFraction,
            */
            /// <summary>
            /// A price, expressed in currency units
            /// </summary>
            Price,
            /// <summary>
            /// A yield (typically an interest rate) expressed as a decimal. I.e. 0.05 means 5%
            /// </summary>
            Rate
        }
        #endregion

        #region OriginOfFeeEnum
        public enum OriginOfFeeEnum
        {
            NoFee,
            FeeFromManualInput,
            FeeFromCalculateBySchedule,
            FeeFromManualInputAndCalculateBySchedule
        };
        #endregion OriginOfFeeEnum
        #region General EfsML Constant
        public const string
            EFSmL_Name = "EfsML",
            EFSmL_Namespace_1_0 = "http://www.efs.org/2004/EFSmL-1-0",
            EFSmL_Namespace_2_0 = "http://www.efs.org/2005/EFSmL-2-0",
            EFSmL_Namespace_3_0 = "http://www.efs.org/2007/EFSmL-3-0",
            Track_Name = "TradeTrack",
            Track_Namespace = "http://www.efs.org/2005/TradeTrack-1-0",
            XMLSchema_Namespace = "http://www.w3.org/2001/XMLSchema",
            XMLSchemaInstance_Namespace = "http://www.w3.org/2001/XMLSchema-instance";
        #endregion General EfsML Constant
        #region General FpML Constant
        public const string
            EfsMLDocument = "efs:EfsDocument",
            OTCml_Name = "OTCml",
            OTCml_ScreenBox = "ScreenBox",
            FpML_Name = "FpML",
            FpML_Namespace_4_0 = "http://www.fpml.org/2003/FpML-4-0",
            FpML_Namespace_4_2 = "http://www.fpml.org/2005/FpML-4-2",
            FpML_Namespace_4_4 = "http://www.fpml.org/2007/FpML-4-4",
            FpML_ScreenFullCapture = "Full",
            FpML_IdPrefix = "ID",
            FpML_InstrumentNo = "INSTRNO",
            FpML_SerializeKeyEnum = "Enum",
            FpML_SerializeKeySpecified = "Specified",
            FpML_EFSPrefixClass = "EFS_",
            FpML_ClassPartyReference = "PartyReference",
            FpML_ClassReference = "Reference",
            FpML_ClassRelativeTo = "RelativeTo",
            FpML_Boolean_True = "true",
            FpML_Boolean_False = "false",
            FpML_EntityOfUserIdentifier = "EntityOfUser",
            FpML_DeskOfUserIdentifier = "DeskOfUser",  //Pour une utilisation future (20050809 PL)
            FpML_IDAError = "Error",
            FpML_IdAttribute = "id",
            EFSmL_IdAttribute = "efs_id",
            FpML_OTCmlIdAttribute = "otcmlId",
            FpML_hRefAttribute = "href",
            FpML_textAttribute = "value",
            FpML_OTCmlTextAttribute = "Value";

        static public string GetAccessKey(string pAccessKey)
        {
            string ret = string.Empty;
            //
            if (StrFunc.IsFilled(pAccessKey) &&
                pAccessKey.ToLower().Trim() != "undefined" &&
                pAccessKey.ToLower().Trim() != "nextblock")
            {
                ret = " (Alt+" + pAccessKey + ")";
            }
            //
            return ret;
        }

        static public string GetIDA_from_party_id(string pParty_id)
        {
            if (pParty_id.StartsWith(FpML_IdPrefix))
                return pParty_id.Remove(0, FpML_IdPrefix.Length);
            else
                return pParty_id;
        }
        static public string Getparty_id_from_IDA(string pIda)
        {
            return FpML_IdPrefix + pIda;
        }
        static public string Getparty_id_from_IDA(int pIda)
        {
            return Getparty_id_from_IDA(pIda.ToString());
        }
        static public string GettradeId_from_IDB(string pIdb)
        {
            return FpML_IdPrefix + pIdb;
        }
        static public string GettradeId_from_IDB(int pIdb)
        {
            return GettradeId_from_IDB(pIdb.ToString());
        }
        static public string GetfloatingRate_id_forFpML(string pIdi)
        {
            return FpML_IdPrefix + pIdi;
        }
        static public string GetfloatingRate_id_forFpML(int pIdi)
        {
            return GetfloatingRate_id_forFpML(pIdi.ToString());
        }

        public const string
            FpML_Swap = "FpML.Ird.Swap",
            FpML_Swaption = "FpML.Ird.Swaption",
            FpML_CapFloor = "FpML.Ird.CapFloor";

        public const string
            FpML_NumberDecimalSeparator = ".",
            FpML_NumberGroupSeparator = "",
            FpML_PercentDecimalSeparator = ".",
            FpML_PercentGroupSeparator = ""
            ;

        public enum CapType
        {
            CAP,
            FLOOR,
            COLLAR
        }

        public static bool IsCap(CapType pCapType) { return CapType.CAP.Equals(pCapType); }
        public static bool IsFloor(CapType pCapType) { return CapType.FLOOR.Equals(pCapType); }
        public static bool IsCollar(CapType pCapType) { return CapType.COLLAR.Equals(pCapType); }


        public enum ExerciseType
        {
            American,
            European,
            Bermuda
        }

        public enum SettlementInformationType
        {
            None,
            Instruction,
            Standard
        }

        public const string
            FrequencyAnnual = "1Y",
            FrequencyContinuous = "OD",
            FrequencyDaily = "1D",
            FrequencyWeekly = "1W",
            FrequencyMonthly = "1M",
            FrequencyQuaterly = "3M",
            FrequencySemiAnnually = "6M",
            FrequencyInFine = "1T",
            FrequencyZeroCoupon = "0T";




        public enum SettlementTypeEnum
        {
            None,
            Standard,
            Net,
            StandardAndNet,
            Instruction
        }
        public const string
            // ===========================
            // OTCml constantes
            // ===========================
        #region OTCml constantes
            //--A
            OTCml_ActorIdScheme = "http://www.euro-finance-systems.fr/otcml/Actorid", //Actorid Laisser pour cause de compatibilité (actorId mieux)
            OTCml_ActorIdentifierScheme = "http://www.euro-finance-systems.fr/otcml/actorIdentifier",
            OTCml_ActorDescriptionScheme = "http://www.euro-finance-systems.fr/otcml/actorDescription",
            OTCml_ActorBicScheme = "http://www.euro-finance-systems.fr/otcml/actorBic",
            OTCml_ActorLTAdressScheme = "http://www.euro-finance-systems.fr/otcml/actorLTAdress",
            //-- --LEI (Begin) PL 20140127 Newness
            OTCml_ActorLEIScheme = "http://www.fpml.org/coding-scheme/external/iso17442",                             //LEI (ISO17442) distributed by a LOU (Local Operating Unit)
            OTCml_ActorCICIScheme = "http://www.fpml.org/coding-scheme/external/cftc/interim-compliant-identifier",   //Pre-LEI distributed by CFTC for DTCC/SWIFT (Commodity Futures Trading Commission )
            OTCml_ActorINSEEScheme = "http://www.euro-finance-systems.fr/insee/prelei",                               //Pre-LEI distributed by INSEE (FRANCE)
            OTCml_ActorLSEScheme = "http://www.euro-finance-systems.fr/lse/prelei",                                   //Pre-LEI distributed by London Stock Exchange (ROYAUME-UNI)
            OTCml_ActorTAKASBANKScheme = "http://www.euro-finance-systems.fr/takasbank/prelei",                       //Pre-LEI distributed by Takasbank  (TURQUIE)
            OTCml_ActorWMDATENSERVICEScheme = "http://www.euro-finance-systems.fr/wmdatenservice/prelei",             //Pre-LEI distributed by WMdatenservice (ALLEMAGNE)
            OTCml_ActorPreLEIScheme = "http://www.euro-finance-systems.fr/prelei",                                    //Pre-LEI distributed by a Pre-LOU
                                                                                                                      //-- --LEI (End)
            OTCml_ActorCssIdentifierScheme = "http://www.euro-finance-systems.fr/otcml/actorcssIdentifier",
            OTCml_ActorExtlLinkScheme = "http://www.euro-finance-systems.fr/otcml/actorExtlLink",
            OTCml_ActorAddress1Scheme = "http://www.euro-finance-systems.fr/otcml/actorAddress1",
            OTCml_ActorIbeiScheme = "http://www.euro-finance-systems.fr/otcml/actorIbei",
            OTCml_ActorIso18773Part1Scheme = "http://www.euro-finance-systems.fr/otcml/actorIso18773Part1",
            OTCml_ActorEconomicAreaCodeScheme = "http://www.euro-finance-systems.fr/otcml/actorEconomicAreaCode",
            OTCml_ActorEconomicAgentCodeScheme = "http://www.euro-finance-systems.fr/otcml/actorEconomicAgentCode",
            OTCml_ActorIdcCapitalScheme = "http://www.euro-finance-systems.fr/otcml/actorIdcCapital",
            OTCml_ActorCapitalScheme = "http://www.euro-finance-systems.fr/otcml/actorCapital",
            OTCml_ActorBusinessNumberScheme = "http://www.euro-finance-systems.fr/otcml/actorBusinessNumber",
            OTCml_ActorTaxNumberScheme = "http://www.euro-finance-systems.fr/otcml/actorTaxNumber",
            OTCml_ActorAccountNumberIdent = "http://www.euro-finance-systems.fr/otcml/accountNumberIdent",
            OTCml_AssetIdScheme = "http://www.euro-finance-systems.fr/otcml/assetid",
            //--B
            OTCml_BookIdScheme = "http://www.euro-finance-systems.fr/otcml/bookid",
            //--C
            OTCml_CssIdScheme = "http://www.euro-finance-systems.fr/otcml/CssId",
            OTCml_CssPhysicalAddressScheme = "http://www.euro-finance-systems.fr/otcml/cssPhysicalAddress",
            OTCml_CssLogicalAddressScheme = "http://www.euro-finance-systems.fr/otcml/cssLogicalAddress",
            OTCml_CNFMessageIdScheme = "http://www.euro-finance-systems.fr/otcml/CNFMessageId",
            OTCml_CNFMessageExtlLinkScheme = "http://www.euro-finance-systems.fr/otcml/CNFMessageExtlLink",
            OTCml_CNFMessageIdentifierScheme = "http://www.euro-finance-systems.fr/otcml/CNFMessageIdentifier",
            OTCml_CNFMessageMSGTypeScheme = "http://www.euro-finance-systems.fr/otcml/CNFMessageMSGType",
            //--E
            OTCml_EmailScheme = "http://www.euro-finance-systems.fr/otcml/email",
            OTCml_EventIdScheme = "http://www.euro-finance-systems.fr/otcml/eventid",
            OTCml_EsrIdScheme = "http://www.euro-finance-systems.fr/otcml/esrid",
            //--F
            OTCml_FaxNumberScheme = "http://www.euro-finance-systems.fr/otcml/faxNumber",
            OTCml_FolderIdScheme = "http://www.euro-finance-systems.fr/otcml/folderid",
            OTCml_FrontTradeIdScheme = "http://www.euro-finance-systems.fr/otcml/frontid",
            OTCmL_FxClassScheme = "http://www.euro-finance-systems.fr/otcml/fxClass",
            //--H
            OTCmL_HedgeClassDervScheme = "http://www.euro-finance-systems.fr/otcml/hedgeClassDerv",
            OTCmL_HedgeClassNDrvScheme = "http://www.euro-finance-systems.fr/otcml/hedgeClassNDrv",
            OTCmL_hedgingFolderid = "http://www.euro-finance-systems.fr/otcml/hedgingFolderid",
            //--I
            OTCmL_IASClassDervScheme = "http://www.euro-finance-systems.fr/otcml/iasClassDerv",
            OTCmL_IASClassNDrvScheme = "http://www.euro-finance-systems.fr/otcml/iasClassNDrv",
            //--L
            OTCmL_LocalClassDervScheme = "http://www.euro-finance-systems.fr/otcml/localClassDerv",
            OTCmL_LocalClassNDrvScheme = "http://www.euro-finance-systems.fr/otcml/localClassNDrv",
            //--M
            OTCml_MobileNumberScheme = "http://www.euro-finance-systems.fr/otcml/mobileNumber",
            //--N
            OTCml_NetConventionIdScheme = "http://www.euro-finance-systems.fr/otcml/netconventionId",
            OTCml_NetConventionIdentifierScheme = "http://www.euro-finance-systems.fr/otcml/netconventionIdentifier",
            OTCml_NetConventionExtlLinkScheme = "http://www.euro-finance-systems.fr/otcml/netconventionExtlLink",
            OTCml_NetDesignationIdScheme = "http://www.euro-finance-systems.fr/otcml/netdesignationId",
            OTCml_NetDesignationIdentifierScheme = "http://www.euro-finance-systems.fr/otcml/netdesignationIdentifier",
            OTCml_NetDesignationExtlLinkScheme = "http://www.euro-finance-systems.fr/otcml/netdesignationExtlLink",
            OTCml_NotificationConfirmationSystemIdScheme = "http://www.euro-finance-systems.fr/otcml/notificationConfirmationSystemId",
            OTCml_NotificationConfirmationSystemIdentifierScheme = "http://www.euro-finance-systems.fr/otcml/notificationConfirmationSystemIdentifier",
            OTCml_NotificationConfirmationSystemExtlLinkScheme = "http://www.euro-finance-systems.fr/otcml/notificationConfirmationIdentifierSystemExtlLink",
            //--P
            OTCml_ProductTypeScheme = "http://www.euro-finance-systems.fr/otcml/producttype",
            //--R
            OTCml_RepositoryFeeScheme = "http://www.euro-finance-systems.fr/otcml/fee",
            OTCml_RepositoryFeeScheduleScheme = "http://www.euro-finance-systems.fr/otcml/feeSchedule",
            /// FI 20180502 [23926] Représente le barème déterminé par Spheres® via la calculette 
            /// Ce Sheme existe lorsque l'utilisateur effectue une substitution de barème, il est utilisé pour stocker le barème initialement déterminé par Spheres®
            OTCml_RepositoryFeeScheduleSys = "http://www.euro-finance-systems.fr/otcml/feeScheduleSys",
            OTCml_RepositoryFeeSchedBracket1Scheme = "http://www.euro-finance-systems.fr/otcml/feeSchedBracket1",
            OTCml_RepositoryFeeSchedBracket2Scheme = "http://www.euro-finance-systems.fr/otcml/feeSchedBracket2",
            OTCml_RepositoryFeeSchedScopeScheme = "http://www.euro-finance-systems.fr/otcml/feeSchedScope", //PL 20191210 [25099]
            OTCml_RepositoryFeeSchedFormulaScheme = "http://www.euro-finance-systems.fr/otcml/feeSchedFormula",
            OTCml_RepositoryFeeSchedFormulaValue1Scheme = "http://www.euro-finance-systems.fr/otcml/feeSchedFormulaValue1",
            OTCml_RepositoryFeeSchedFormulaValue2Scheme = "http://www.euro-finance-systems.fr/otcml/feeSchedFormulaValue2",
            OTCml_RepositoryFeeSchedFormulaDCFScheme = "http://www.euro-finance-systems.fr/otcml/feeSchedFormulaDCF",
            OTCml_RepositoryFeeSchedFormulaMinScheme = "http://www.euro-finance-systems.fr/otcml/feeSchedFormulaMin",
            OTCml_RepositoryFeeSchedFormulaMaxScheme = "http://www.euro-finance-systems.fr/otcml/feeSchedFormulaMax",
            OTCml_RepositoryFeeInvoicingScheme = "http://www.euro-finance-systems.fr/otcml/feeInvoicing",
            OTCml_RepositoryFeePaymentFrequencyScheme = "http://www.euro-finance-systems.fr/otcml/feePaymentFrequency",
            //PL 20141017 DEPRECATED OTCml_RepositoryFeeSchedAssessmentBasisValueScheme = "http://www.euro-finance-systems.fr/otcml/feeSchedAssessementBasisValue",
            OTCml_RepositoryFeeSchedAssessmentBasisValue1Scheme = "http://www.euro-finance-systems.fr/otcml/feeSchedAssessementBasisValue1",
            OTCml_RepositoryFeeSchedAssessmentBasisValue2Scheme = "http://www.euro-finance-systems.fr/otcml/feeSchedAssessementBasisValue2",
            //OTCml_RepositoryFeeSchedPeriodBasisValueScheme = "http://www.euro-finance-systems.fr/otcml/feeSchedPeriodBasisValue",
            OTCml_RepositoryFeeSchedPeriodCharacteristicsScheme = "http://www.euro-finance-systems.fr/otcml/feeSchedPeriodCharacteristics",
            OTCml_RepositoryFeeMatrixScheme = "http://www.euro-finance-systems.fr/otcml/feeMatrix",
            //
            OTCml_RepositoryTaxScheme = "http://www.euro-finance-systems.fr/otcml/tax",
            OTCml_RepositoryFeeTaxApplicationScheme = "http://www.euro-finance-systems.fr/otcml/taxApplication",
            OTCml_RepositoryFeeEventTypeScheme = "http://www.euro-finance-systems.fr/otcml/feeEventType",
            OTCml_RepositoryTaxDetailScheme = "http://www.euro-finance-systems.fr/otcml/taxDetail",
            OTCml_RepositoryTaxDetailCountryScheme = "http://www.euro-finance-systems.fr/otcml/taxDetailCountry",
            OTCml_RepositoryTaxDetailRateScheme = "http://www.euro-finance-systems.fr/otcml/taxDetailRate",
            OTCml_RepositoryTaxDetailTypeScheme = "http://www.euro-finance-systems.fr/otcml/taxDetailType",
            OTCml_RepositoryTaxDetailEventTypeScheme = "http://www.euro-finance-systems.fr/otcml/taxDetailEventType",
            OTCml_RepositoryTaxDetailCollected = "http://www.euro-finance-systems.fr/otcml/taxDetailCollected",
            //
            OTCml_RepositoryInvoiceTradeSortScheme = "http://www.euro-finance-systems.fr/otcml/invoiceTradeSort",
            OTCml_RepositoryReportTradeSortScheme = "http://www.euro-finance-systems.fr/otcml/reportTradeSort",
            //--S
            OTCmL_SecurityIdSourceScheme = "http://www.euro-finance-systems.fr/spheres-enum/FIX/SecurityIDSource",
            OTCml_STLMessageIdScheme = "http://www.euro-finance-systems.fr/otcml/STLMessageId",
            OTCml_STLMessageIdentifierScheme = "http://www.euro-finance-systems.fr/otcml/STLMessageIdentifier",
            OTCml_STLMessageExtlLinkScheme = "http://www.euro-finance-systems.fr/otcml/STLMessageExtlLink",
            //--T
            OTCml_TradeIdentifierScheme = "http://www.euro-finance-systems.fr/otcml/tradeidentifier",
            OTCml_TradeIdScheme = "http://www.euro-finance-systems.fr/otcml/tradeid",
            //-- --UTI (Begin) PL 20140127 Newness
            //OTCml_TradeIdUTIIssuerCCPScheme = "http://www.euro-finance-systems.fr/ccp/issuer-identifier",               //CCP UTI issuer
            //OTCml_TradeIdUTIIssuerCCeGScheme = "http://www.euro-finance-systems.fr/cceg/issuer-identifier",             //CCeG UTI issuer
            //OTCml_TradeIdUTIIssuerCFTCScheme = "http://www.fpml.org/coding-scheme/external/cftc/issuer-identifier",     //CFTC UTI issuer
            //OTCml_TradeIdUTIIssuerEUREXScheme = "http://www.euro-finance-systems.fr/eurex/issuer-identifier",           //EUREX UTI issuer
            //OTCml_TradeIdUTIIssuerSpheresScheme = "http://www.euro-finance-systems.fr/spheres/issuer-identifier",       //SPHERES UTI issuer
            //OTCml_TradeIdUTICCPScheme = "http://www.euro-finance-systems.fr/ccp/unique-transaction-identifier",         //UTI with CCP issuer
            OTCml_TradeIdUTISpheresScheme = "http://www.euro-finance-systems.fr/spheres/unique-transaction-identifier", //UTI with SPHERES issuer
                                                                                                                        //OTCml_TradeIdUTIScheme = "http://www.fpml.org/coding-scheme/external/unique-transaction-identifier",        //UTI without issuer (used 
                                                                                                                        //-- --UTI (End) PL 20140127 Newness
            OTCml_TelephoneNumberScheme = "http://www.euro-finance-systems.fr/otcml/telephoneNumber",
            OTCml_TelexNumberScheme = "http://www.euro-finance-systems.fr/otcml/telexNumber",
            //
            //--W
            OTCml_WebScheme = "http://www.euro-finance-systems.fr/otcml/web",

            OTCml_ActorId = "actorId",
            OTCml_BookId = "bookid",
            OTCml_LocalClassDerv = "localClassDerv",
            OTCml_LocalClassNDrv = "localClassNDrv",
            OTCml_FxClass = "fxClass",
            OTCml_IASClassDerv = "iasClassDerv",
            OTCml_IASClassNDrv = "iasClassNDrv",
            OTCml_HedgeClassDerv = "hedgeClassDerv",
            OTCml_HedgeClassNDrv = "hedgeClassNDrv",
            OTCml_FrontId = "frontid",
            OTCml_FolderId = "folderid",
            OTCml_ProductId = "producttype",
        #endregion
            // ===========================
            // Spheres constantes
            // ===========================
        #region OTCml constantes
            //--C
            Spheres_Canceled_TradeIdentifierScheme = "http://www.euro-finance-systems.fr/spheres/canceled-tradeidentifier",
            Spheres_CashBalance_TradeIdentifierScheme = "http://www.euro-finance-systems.fr/spheres/cashbalance-tradeidentifier",
            Spheres_CashPayment_TradeIdentifierScheme = "http://www.euro-finance-systems.fr/spheres/cashpayment-tradeidentifier",
            //--E
            Spheres_ETD_TradeIdentifierScheme = "http://www.euro-finance-systems.fr/spheres/etd-tradeidentifier",
            //--M
            Spheres_MarginRequirement_TradeIdentifierScheme = "http://www.euro-finance-systems.fr/spheres/marginrequirement-tradeidentifier",
            //--N
            Spheres_NextCashBalance_TradeIdentifierScheme = "http://www.euro-finance-systems.fr/spheres/nextcashbalance-tradeidentifier",
            //--P
            Spheres_PrevCashBalance_TradeIdentifierScheme = "http://www.euro-finance-systems.fr/spheres/prevcashbalance-tradeidentifier",
            //--R
            Spheres_ReflectedBy_TradeIdentifierScheme = "http://www.euro-finance-systems.fr/spheres/reflectedby-tradeidentifier",
            Spheres_ReflectionOf_TradeIdentifierScheme = "http://www.euro-finance-systems.fr/spheres/reflectionof-tradeidentifier",
            Spheres_Replacement_TradeIdentifierScheme = "http://www.euro-finance-systems.fr/spheres/replacement-tradeidentifier",
            //--T
            // EG 20240227 [WI855] Trade input : New data TVTIC (Trading Venue Transaction Identification Code)
            Spheres_TradeIdTvticScheme = "http://www.euro-finance-systems.fr/spheres/tvtic",
            // EG 20240227 [WI858] Trade input : New data TRDID (Market Transaction Id)
            Spheres_TradeIdMarketTransactionIdScheme = "http://www.euro-finance-systems.fr/spheres/markettransactionid",
            // EG 20240227 [WI856] New
            Market_Iso10383Scheme = "http://www.fpml.org/coding-scheme/external/mifir/extension-iso10383"
        #endregion
;

        #endregion
        #region General FixML Constant
        public const string
            FixML_Name = "FixML",
            FixML_Namespace_4_4 = "http://www.fixprotocol.org/FIXML-4-4",
            FixML_Namespace_5_0_SP1 = "http://www.fixprotocol.org/FIXML-5-0-SP1",
            FixML_DateFmt = "yyyyMMdd",
            FixML_DateTimeFmt = "yyyyMMdd-HH:mm:ss",
            FixML_TimeFmt = "HH:mm:ss";
        #endregion

        #region MPD Object Constant

        public enum EFS_TBL
        {
            EFSSOFTWARE,
            EFSLOCK
        }

        /// <summary>
        /// 
        /// </summary>
        [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
        public sealed class DependsOnTableAttribute : Attribute
        {
            /// <summary>
            /// 
            /// </summary>
            public Cst.OTCml_TBL Table { get; set; }
        }


        //[NewReferential] Keyword to find for adding a new referential
        // RD 20120830 [18102] Gestion des compteurs IOTRACK
        // Ajout d'un attribut "Description" sur chaque table 
        // qui sera une valorisé avec une description "Business" de la table
        // Cette description est restituée dans le Log de l'importation (IOTRACK)
        // FI 20170928 [23452] Modify
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (Add TRADEXML table, Remove TRADESTSYS table)
        [Serializable]
        public enum OTCml_TBL
        {
            ACCCONDITION, ACCDAYBOOK, ACCDAYBOOK_EVENT, ACCENTITYMODEL, ACCINSTRENV, ACCINSTRENVDET,
            ACCKEY, ACCKEYENUM, ACCKEYVALUE, ACCLABEL, ACCMODEL,
            ACCOUNTAT,
            ACCOUNTING,
            [Description("Internal account")]
            ACCOUNTINTERNAL,
            ACCSCHEME, ACCENTRY, ACCVARIABLE,
            //
            ACTIONTUNING,
            [Description("Actor")]
            ACTOR,
            ACTORG,
            ACTORACTOR,
            ACTORAMOUNT,
            ACTORDAYHOUR,
            // AL 20240607 [WI955] Impersonate mode
            [Description("Impersonation")]
            ACTORIMPERSONATE,
            ACTORINSTRUMENT,
            ACTORMARKET,
            ACTORMENU,
            ACTORMODEL,
            ACTORPERMISSION,
            [Description("Role")]
            ACTORROLE,
            ACTORTAX,
            [Description("Complementary address")]
            ADDRESSCOMPL,
            ASSET_x,
            [Description("Bond asset")]
            [UnderlyingAssetAttribute(Cst.UnderlyingAsset.Bond)]
            ASSET_BOND,
            [Description("Cash asset")]
            [UnderlyingAssetAttribute(Cst.UnderlyingAsset.Cash)]
            ASSET_CASH,
            [Description("commodity asset")]
            [UnderlyingAssetAttribute(Cst.UnderlyingAsset.Commodity)]
            ASSET_COMMODITY,
            [Description("Deposit asset")]
            ASSET_DEPOSIT,
            [Description("Equities asset")]
            [UnderlyingAssetAttribute(Cst.UnderlyingAsset.EquityAsset)]
            ASSET_EQUITY,
            ASSET_EQUITY_RDCMK,
            [Description("Exchange traded derivative asset")]
            [UnderlyingAssetAttribute(Cst.UnderlyingAsset.ExchangeTradedContract)]
            ASSET_ETD,
            [Description("Exchange traded fund asset")]
            [UnderlyingAssetAttribute(Cst.UnderlyingAsset.ExchangeTradedFund)]
            ASSET_EXTRDFUND,
            [Description("Exchange rate asset")]
            [UnderlyingAssetAttribute(Cst.UnderlyingAsset.FxRateAsset)]
            ASSET_FXRATE,
            [Description("Indexes asset")]
            [UnderlyingAssetAttribute(Cst.UnderlyingAsset.Index)]
            ASSET_INDEX,
            [Description("Mutual fund asset")]
            ASSET_MUTUALFUND,
            [Description("Rate index asset")]
            [UnderlyingAssetAttribute(Cst.UnderlyingAsset.RateIndex)]
            ASSET_RATEINDEX,
            [Description("Simple Fra asset")]
            [UnderlyingAssetAttribute(Cst.UnderlyingAsset.SimpleFra)]
            ASSET_SIMPLEFRA,
            [Description("Simple Swap asset")]
            [UnderlyingAssetAttribute(Cst.UnderlyingAsset.SimpleIRSwap)]
            ASSET_SIMPLEIRS,
            ASSETENV,
            ASSETRATING,
            ATTACHEDDOC,
            ATTACHEDDOCS,
            BENCHMARKREF,
            [Description("Book")]
            BOOK,
            BOOKACTOR_R,
            BOOKG,
            BOOKPOSEFCT,

            BUSINESSCENTER,
            [Description("Collateral/Cash balance")]
            CASHBALANCE,
            CBINTERESTRULE,
            CBREQUEST,
            // EG 20150723 [21187] New
            CBENTITY_MODEL,
            CBACTOR_MODEL,
            CBACTORCBO_MODEL,
            // EG 20180906 PERF
            CBBOOK_MODEL,
            // EG 20181010 PERF
            CBTRBOOK_MODEL,
            CBTRADE_AF_MODEL,
            CBTRADE_ANF_MODEL,
            CBTRADE_EXEC_MODEL,
            /// <summary>
            ///  Table modèle pour création de table temporaire
            /// </summary>
            /// FI 20170316 [22950] add
            CBTRADE_MODEL,
            CLEARINGCOMPART,
            CLEARINGORGPARAM,
            CLEARINGTEMPLATE,
            COLLATERALENV,
            [Description("Asset priority for collateral use")]
            COLLATERALPRIORITY,
            [Description("Currency priority for collateral use")]
            MGCCURPRIORITY,
            COMMODITYCONTRACT,
            [Description("Derivative contract cascading definition")]
            CONTRACTCASCADING,
            CONTRACTG, //Regroupement de DERIVATIVECONTRACT
            [Description("Corporate action issued by Market data")]
            CORPOACTIONISSUE,
            CORPOACTION,
            CORPOACTIONNOTICE,
            CORPOEVENT,
            CORPOEVENTASSET,
            CORPOEVENTCONTRACT,
            CORPOEVENTDATTRIB,
            CORPOEVENTMKTRULES,
            COOKIE,
            COUNTRY,
            COUNTRYOF,
            CSS,
            CSSLINK,
            CSMID,
            CURRENCY,
            CURVEPTSCHED,
            CURVEPTSCHEDDET,
            CUSTODIAN,
            DAYHOUR,
            DAYHOURMODEL,
            //
            DEBTSECURITY, /* Virtual table */
            DEFINEEARCALC,
            DEFINEEXTEND, DEFINEEXTENDDET,
            DEFINEEXTLID,
            DEFINEEXTLLINK,
            //
            [Description("Contract maturity")]
            DERIVATIVEATTRIB,
            [Description("Derivative contract")]
            DERIVATIVECONTRACT,
            [Description("Derivative contract additional maturity Rules")]
            DRVCONTRACTMATRULE,
            //
            EAR,
            EAR_ACCMODEL,
            EARDET,
            EARCALC,
            EARCALC_EVENT,
            EARCALCAMOUNT,
            EARCOMMON,
            EARCOMMONAMOUNT,
            EARDAY,
            EARDAYAMOUNT,
            EARNOM,
            EARNOM_EVENT,
            EARNOMAMOUNT,
            EARRESULT,
            //
            EFSOBJECT,
            EFSOBJECTDET,
            EFSSOFTWARE,
            EFSSOFTWAREUPG,
            ENTITY,
            ENTITYMARKET,
            ENTITYMARKETTRAIL,
            //ERROR_L,
            ESR,
            ESR_T,
            ESRDET,
            ESRDET_T,
            EVENT,
            EVENTASSET,
            EVENTCLASS,
            EVENTDET,
            //EVENTDET_ETD,
            EVENTENUMS,
            EVENTENUM,
            EVENTFEE,
            EVENTGROUP,
            EVENTPOSACTIONDET,
            EVENTPRICING,
            EVENTPRICING2,
            EVENTPROCESS,
            EVENTSI,
            EVENTSI_T,
            EVENTSTCHECK,
            EVENTSTMATCH,
            [Description("External information")]
            EXTLID,
            EXTLIDS,
            ENUM,
            ENUMS,
            //
            FEE, FEESCHEDULE, FEESCHEDBRACKET1, FEESCHEDBRACKET2, FEEMATRIX,
            FUNDINGSCHEDULE, FUNDINGMATRIX, FUNDINGMATRIXRELTO,
            //
            GACTOR, GACTORROLE,
            GBOOK, GBOOKROLE,
            GCONTRACT, GCONTRACTROLE,
            GINSTR, GINSTRROLE,
            GMARKET, GMARKETROLE,
            GPARAM,
            GTAX,
            HOLIDAYCALCULATED,
            HOLIDAYMISC,
            HOLIDAYMONTHLY,
            HOLIDAYYEARLY,
            HOLIDAYWEEKLY,
            HOLIDAYRESULT,
            HOUR,
            IMREQMARKETPARAM,
            IMREQUEST,
            INCI,
            INCIITEM,
            INDEX,
            INSTRUMENT,
            INSTRUMENTGUI,
            INSTRUMENTMODEL,
            INSTRUMENTOF,
            INSTRG,
            INVOICINGRULES,
            INVOICEFEESDETAIL, /* it's not a table */
            INVRULESBRACKET,
            // 20100830 - MF
            IOCOMPARE,
            // 20100901 - MF
            [Description("External data")]
            EXTLDATA,
            // 20100902 - MF
            [Description("External data detail")]
            EXTLDATADET,
            IOINPUT,
            IOINPUT_PARSING,
            IOOUTPUT,
            IOOUTPUT_PARSING,
            IOPARAM,
            IOPARAMDET,
            IOPARSING,
            IOPARSINGDET,
            IOSHELL,
            IOTASK,
            IOTASKDET,
            IOTASK_PARAM,
            IOTRACK,
            ISSI,
            ISSIITEM,
            LICENSEE,
            LINKID,
            LOGIN_L,
            LSTCONSULT,
            LSTCONSULTWHERE,
            LSTCONSULTALIAS,
            LSTCONSALIASJOIN,
            LSTCONSCOLUMNDET,
            LSTTABLE,
            LSTTABLEDET,
            LSTALIAS,
            LSTJOIN,
            LSTTEMPLATE,
            LSTTEMPLATEDEF,
            LSTSELECT,
            LSTWHERE,
            LSTORDERBY,
            LSTCOLUMN,
            /// <summary>
            ///  Table de stockage des paramètres GUI (CustomObject)
            /// </summary>
            /// FI 20200602 [25370] 
            LSTPARAM,
            MARGINTRACK,
            MARKET, MARKETG,
            MARKETENV,
            MARKETMODEL,

            MARGINSCHEDULE, MARGINMATRIX, MARGINMATRIXRELTO,

            MASTERAGREEMENT,
            MATRIXDEF,
            MATRIXVAL_H,
            MATRIXPT_H,
            MATRIXPTSTRUCT_H,
            //
            [Description("Maturity")]
            MATURITY,
            [Description("Maturity rule")]
            MATURITYRULE,
            //
            MENU,
            MENUOF,
            MENUMODEL,
            MODELACTOR,
            MODELDAYHOUR,
            MODELINSTRUMENT,
            MODELMARKET,
            MODELMENU,
            MODELPERMISSION,
            MODELSAFETY,
            MCO,
            MCODET,
            MCO_T,
            MCODET_T,
            MSO,
            MSO_T,
            MSODET,
            MSODET_T,
            CNFMESSAGE,
            CNFMESSAGEDET,
            CNFMESSAGENCS,
            NCS,
            NETCONVENTION,
            NETDESIGNATION,
            NOTEPAD,
            NOTEPADS,
            PARTYTEMPLATE,
            PARTYTEMPLATEDET,
            PARAMG,
            PERMISSIONMODEL,
            POSACTION,
            POSACTIONDET,
            [Description("Collateral position")]
            POSCOLLATERAL,
            POSCOLLATERALVAL,
            POSEQUSECURITY,
            POSREQUEST,
            PROCESS_L,
            PROCESSDET_L,
            PROCESSTUNING,
            PRODUCT,
            PERMISSION,
            PWDEXPIRED_H,
            [Description("Commodity price")]
            QUOTE_COMMODITY_H,
            QUOTE_BOND_H,
            [Description("Debt security price")]
            QUOTE_DEBTSEC_H,
            QUOTE_DEPOSIT_H,
            [Description("Equity price")]
            QUOTE_EQUITY_H,
            [Description("Exchange traded derivative price")]
            QUOTE_ETD_H,
            [Description("Exchange traded fund price")]
            QUOTE_EXTRDFUND_H,
            [Description("Exchange rate price")]
            QUOTE_FXRATE_H,
            [Description("Index price")]
            QUOTE_INDEX_H,
            QUOTE_MUTUALFUND_H,
            QUOTE_OTHER_H,
            [Description("Rate index price")]
            QUOTE_RATEINDEX_H,
            QUOTE_SCDEFSWAP_H,
            QUOTE_SIMPLEFRA_H,
            QUOTE_SIMPLEIRS_H,
            QUOTE_x_H,
            RATEINDEX,
            REQUEST_L,
            RDBMS_L,
            RDBMSTRACE,
            [Description("Risk (calculation parameters, ...)")]
            RISKMARGIN,
            ROLEACTOR,
            SELFCOMPOUNDING_CF,
            SELFCOMPOUNDING_AI,
            SELFCOMPOUNDING_V,
            SERVICE,
            SERVICE_L,
            SERVICESCHEDULE,
            SESSIONCLIPBOARD,
            SESSIONRESTRICT,
            SMTPSERVER,
            SSIDB,
            STCHECK,
            STCHECKTUNING,
            STMATCH,
            STMATCHTUNING,
            STLMESSAGE,
            SYSTEM_L,
            SYSTEMMSG, SYSTEMMSGDET,
            TAX, TAXDET, TAXEVENT,
            TMPEARCALCAMOUNT,
            TMPEARCALCDET,
            TRACKER_L,
            /// EG 20170125 [Refactoring URL] New
            TRACKER_POSREQUEST,
            [Description("Trade")]
            TRADE,
            TRADETRAIL,
            TRADE_P,
            TRADE_S,
            TRADEXML,
            TRADEXML_P,
            TRADELINK,
            TRADELINK_P,
            // EG 20130924
            TRLINKPOSREQUEST,
            TRADEMARGINTRACK,
            // EG 20130731 [18859]
            TRADEMERGERULE,
            TRADESTCHECK,
            TRADESTCHECK_P,
            TRADESTMATCH,
            TRADESTMATCH_P,
            TRADEACTOR,
            TRADEADMIN,        /* Virtual table */
            TRADEDEBTSEC,      /* Virtual table */
            TRADERISK,         /* Virtual table */
            TRADEID,
            TRADEINSTRUMENT,
            TRADESTREAM,
            TRADEASSET,
            VALSCENARIO,
            YIELDCURVEDEF,
            YIELDCURVEPOINT_H,
            YIELDCURVEVAL_H,
            //
            VW_ACCCONDITIONDATA,
            VW_ACCKEY,
            VW_ACCKEYDATA,
            VW_ACCKEYENUMVALUE,
            VW_ACCVARIABLEDATA,
            VW_ALL_ENUM,
            VW_ALL_ENUMS,
            VW_ALL_VW_ENUM,
            VW_ALL_VW_PERMIS_MENU,
            VW_ALLOCATEDINVOICESTL,
            VW_AMOUNTTYPE,
            VW_ASSET,
            VW_ASSET_COMMODITY_EXPANDED,
            VW_ASSET_FORCURVE,
            VW_ASSET_DEBTSECURITY,
            VW_ASSET_RATEINDEX,
            VW_ASSET_EQUITY_EXPANDED,
            VW_ASSET_ETD_EXPANDED,
            // FI 20191002 Add
            /// <summary>
            /// All attached document by tracker	
            /// </summary>
            VW_ATTACHEDDOC_TRACKER_L,
            VW_AVAILABLECREDITSTL,
            VW_AVAILABLEINVOICESTL,
            VW_BOOK_VIEWER,

            VW_COLLATERALPOS,
            VW_CONTRACT,
            // FI 20170223 [22883] Modify
            VW_COMMODITYCONTRACT,
            VW_DRVCONTRACTMATRULE,
            VW_EARACCAMOUNT,
            VW_EARACCSCHEME,
            VW_EARALLAMOUNT,
            VW_EARDET,
            VW_EAREVENT,
            VW_EAREVENTENUM,
            VW_ENUM,
            VW_ENTITYCSS,
            VW_ENTITY_CSSCUSTODIAN,
            VW_EVENT,
            VW_EVENTASSET,
            VW_EVENTESR,
            VW_EVENTSOURCE,
            VW_EVENTRESET,
            VW_EVENTSI,
            VW_EVENT_STL,
            // EG 20130731 [18859]
            VW_GACTORROLE,
            VW_GBOOKROLE,
            VW_GCONTRACTROLE,
            VW_GINSTRROLE,
            VW_INSTR_PRODUCT,
            // EG 20091110
            VW_INVOICEFEESDETAIL,
            VW_MENU,
            VW_INVMCO,
            VW_LASTPOSITION,
            VW_GMARKETROLE,
            VW_MARKET_IDENTIFIER,
            [Description("Maturity rule")]
            VW_MATURITYRULE,
            // EG 20171113 [23509] New
            VW_MARKET_FACILITY,
            VW_MCO,
            VW_MCO_MULTITRADES,
            VW_PERMIS_MENU,
            VW_POSITIONACTION,
            VW_QUOTE_H,
            VW_TRADE,
            VW_TRADE_ASSET,
            VW_TRADE_POSETD,
            VW_TRADE_POSOTC,
            VW_TRADE_POSSEC,
            VW_TRADE_POSCOM,
            VW_TRADE_POSKEEPING_ALLOC,
            VW_TRADE_FUNGIBLE,
            VW_TRADE_FUNGIBLE_ETD,
            VW_TRADE_FUNGIBLE_OTC,
            VW_TRADE_FUNGIBLE_SEC,
            VW_TRADE_FUNGIBLE_COM,
            VW_TRADE_FUNGIBLE_LIGHT,
            VW_TRADE_FUNGIBLE_LIGHT_ETD,
            VW_TRADE_FUNGIBLE_LIGHT_OTC,
            VW_TRADE_FUNGIBLE_LIGHT_SEC,
            VW_TRADE_FUNGIBLE_LIGHT_COM,
            VW_TRADEADMIN,
            VW_TRADEDEBTSEC,
            VW_TRADE_INSTR_TEMPLATE,
            VW_TRADEINSTRUMENT,
            VW_TRADE_PARTY,
            //PM 20150311 [POC] Add VW_TRADELINK
            VW_TRADELINK,
            //
            UNKNOW,
        };
        public enum OTCml_COL
        {
            DTHOLIDAYVALUE,
            DTHOLIDAYNEXTDATE,
            ROWATTRIBUT,
            ROWVERSION
        }
        public enum OTCml_StoredProcedure
        {
            UP_CREATETABLEFROMTABLE,
            UP_GETACTOR,
            UP_GETENTITY,
            UP_GETID,
        }

        public enum OTCml_StoredProcedure_Params
        {
            pActorIDA,
            pIdRoleActor,
            pIdentifier,
            pIsBookIdentifier,
            pIsGetFirstEntity,
            pComparisonDate,
            pAskParentLevel,
            opHasRole,
            opHasRoleSelf,
            opParentLevel,
            opIDA,
            opActorIDA,
            pGETVERSION_1_0_0,
            ReturnValue
        }

        /// <summary>
        /// List all the database constraint objects
        /// </summary>
        public static class OTCml_Constraint
        {
            /// <summary>
            /// Foreign key constraint among the EVENTCLASS.IDEC column and the EARDAY.IDEC column. 
            /// </summary>
            public const string FK_EARDAY_EVENTCLASS = "FK_EARDAY_EVENTCLASS";
        }

        #endregion

        #region public enum Cookie Constants
        /// <summary>
        /// 
        /// </summary>
        /// EG 20161122 Add TrackerDisplayMode & TrackerDisplayValues
        /// FI 20161214 [21916] Modify
        /// FI 20171025 [23533] Modify 
        // EG 20171114 [23509] Add TradeDefaultFacility, TradeDefaultFacility_xxx
        // EG 20190419 [EventHistoric Settings] New
        // EG 20200107 [25560] Gestion valeur par defaut des données visibles dans les référentiels 
        public enum SQLCookieElement
        {
            NumberRowByPage,
            IsDisplayDescriptive,
            DefaultPage,
            ETDMaturityFormat,
            TradingTimestampZone, // FI 20171025 [23533] add
            TradingTimestampPrecision, // FI 20171025 [23533] add
            AuditTimestampZone, // FI 20171025 [23533] add
            AuditTimestampPrecision, // FI 20171025 [23533] add
            UniquePage,
            BackgroundWhite,
            Lookv1,
            PagerPosition,

            EventHistoric,

            TrackerAlert,
            TrackerAlertProcess,
            TrackerRefreshInterval,
            TrackerNbRowPerGroup,
            TrackerHistoric,

            TrackerGroupFav,
            TrackerGroupDetail,


            TrackerDisplayMode,
            TrackerDisplayValues,

            TradeDefaultFacility,
            TradeDefaultFacility_ESE,
            TradeDefaultFacility_COMS_gas,
            TradeDefaultFacility_COMS_elec,
            TradeDefaultFacility_COMS_env, // EG 20221101 [25639][WI484] new
            TradeDefaultFacility_Other,
            TradeDefaultMarket,
            TradeDefaultMarket_ESE,
            TradeDefaultMarket_COMS_gas, // FI 20161214 [21916] add
            TradeDefaultMarket_COMS_elec, // FI 20161214 [21916] add
            TradeDefaultMarket_COMS_env,  // EG 20221101 [25639][WI484] new

            MonitoringObserverElement,
            MonitoringRefreshInterval,

            TrackerVelocity,
            TrackerRefreshActive,
            CSSMode,

            ValidityData, // EG 20200107 [XXXXX] add
        }
        #endregion

        #region public enum SQLCookieGrpElement
        /// <summary>
        /// 
        /// </summary>
        /// FI 20140930 [XXXXX] Add
        public enum SQLCookieGrpElement
        {
            DBNull,
            Requester,
            SelADMProduct,
            SelProduct,
            SelDebtSecProduct,
            /// <summary>
            /// Regroupement GPRODUCT = 'RISK' 
            /// </summary>
            SelRiskProduct,
            /// <summary>
            /// Regroupement GPRODUCT = 'RISK' et FAMILY = 'MARGIN'
            /// </summary>
            SelRiskProductMargin,
            /// <summary>
            /// Regroupement GPRODUCT = 'RISK' et FAMILY = 'CASHBALANCE'
            /// </summary>
            SelRiskProductCashBalance,
            /// <summary>
            /// Regroupement GPRODUCT = 'RISK' et FAMILY = 'CASHPAYMENT'
            /// </summary>
            SelRiskProductCashPayment,
            /// <summary>
            /// Regroupement GPRODUCT = 'RISK' et FAMILY = 'CASHINTEREST'
            /// </summary>
            SelRiskProductCashInterest,

            Tracker,
            TradeEars,
            TradeEvents,

            Monitoring,
        }
        #endregion

        #region public enum EnumElement
        public enum EnumElement
        {
            Product = 0,
            Instrument = 1,
            Template = 2,
            Screen = 3
        };
        #endregion

        /// <summary>
        /// Type de contract
        /// </summary>
        /// FI 20170908 [23409] Add
        public enum ContractCategory
        {
            DerivativeContract,
            CommodityContract,
        }

        #region public enum MaturityRelativeTo
        /// <summary>
        /// nom de l’échéance relatif à une date (ex. "Dernier jour de négo." pour des contrats MSCI)
        /// </summary>
        /// FI 20230125 [XXXXX] Add
        public enum MaturityRelativeTo
        {
            /// <summary>
            /// Nom d'échéance relatif à EXP (Expiry date)
            /// </summary>
            [ResourceAttribut(Resource = "MaturityRelativeTo_EXP")]
            EXP,
            /// <summary>
            /// Nom d'échéance relatif à LTD (Last Trading Date)
            /// </summary>
            [ResourceAttribut(Resource = "MaturityRelativeTo_LTD")]
            LTD
        }
        #endregion

        #region General Methods
        public static bool IsSpaceCultureSeparator(string pSeparator)
        {
            return (Cst.NonBreakSpace == pSeparator) || (Cst.FrenchCultureThousandsSeparator == pSeparator);
        }
        #endregion

        #region Methods: IsUserType_xxx(), IsTypexxx(), IsDDLTypexxx()
        // EG 20170929 [23450] New
        public static bool IsDDLTypeMapZone(string pType)
        {
            if (pType == null)
                return false;
            else
                return (pType.StartsWith("mapzone."));
        }

        public static bool IsDDLTypeEnum(string pType)
        {
            if (pType == null)
                return false;
            else
                return (pType.StartsWith("enum."));
        }
        public static bool IsDDLEmpty(string pType)
        { return (pType == "empty"); }
        public static bool IsDDLErrorOnLoad(string pType)
        { return (pType == "erroronload"); }
        public static bool IsDDLTypeSubsetPrice(string pType)
        { return (pType == "subsetprice"); }
        public static bool IsDDLTypeSource(string pType)
        { return (pType == "source"); }
        public static bool IsDDLTypeStatusCalculEnum(string pType)
        { return (pType == "statuscalcul"); }
        public static bool IsDDLTypeStatusTrigger(string pType)
        { return (pType == "statustrigger"); }
        public static bool IsDDLTypeSourceAndServices(string pType)
        { return (pType == "sourceandservices"); }
        public static bool IsDDLTypeCashFlow(string pType)
        { return (pType == "cashflow"); }
        public static bool IsDDLTypeQuoteTiming(string pType)
        { return (pType == "quotetiming"); }
        public static bool IsDDLTypeSettlSessIDEnum(string pType)
        { return (pType == "settlsessidenum"); }
        public static bool IsDDLTypePriceQuoteUnits(string pType)
        { return (pType == "pricequoteunits"); }
        public static bool IsDDLTypeAssetMeasure(string pType)
        { return (pType == "assetmeasure"); }
        public static bool IsDDLTypeCountryType(string pType)
        { return (pType == "countrytype"); }
        public static bool IsDDLTypeMarketType(string pType)
        { return (pType == "markettype"); }
        //		public static bool IsDDLTypeInputAction(string pType)
        //		{return (pType=="inputaction");}   
        public static bool IsDDLTypeTradeInputAction(string pType)
        { return (pType == "tradeinputaction"); }
        public static bool IsDDLTypeInputSource(string pType)
        { return (pType == "inputsource"); }
        public static bool IsDDLTypeQuotationSide(string pType)
        { return (pType == "quotationside"); }
        public static bool IsDDLTypeProductSource(string pType)
        { return (pType == "productsource"); }
        public static bool IsDDLTypeDataType(string pType)
        {
            // 20081113 RD : DDL avec Filtre dans le Referential
            if (pType == null)
                return false;
            else
                return ((pType == "datatype") || pType.StartsWith("datatype."));
        }
        public static bool IsDDLTypeWebControlType(string pType)
        {
            // 20081113 RD : DDL avec Filtre dans le Referential
            if (pType == null)
                return false;
            else
                return ((pType == "webcontroltype") || pType.StartsWith("webcontroltype."));
        }
        public static bool IsDDLTypeAveragingMethod(string pType)
        { return (pType == "averagingmethod"); }
        public static bool IsDDLTypeRoundDir(string pType)
        { return (pType == "rounddir"); }
        public static bool IsDDLTypeRoundPrec(string pType)
        { return (pType == "roundprec"); }
        public static bool IsDDLTypeCountry(string pType)
        { return (pType == "country"); }
        public static bool IsDDLTypeCountry_Country(string pType)
        { return (pType == "country_country"); }
        public static bool IsDDLTypeColor(string pType)
        { return (pType == "color"); }
        public static bool IsDDLTypeStyleComponentColor(string pType)
        {
            return (IsDDLTypeStyle_Color(pType)
                || IsDDLTypeStyle_BackColor(pType)
                || IsDDLTypeStyle_BorderColor(pType));
        }
        public static bool IsDDLTypeStyle_Color(string pType)
        { return (pType == "textcolor"); }
        public static bool IsDDLTypeStyle_BackColor(string pType)
        { return (pType == "backcolor"); }
        public static bool IsDDLTypeStyle_BorderColor(string pType)
        { return (pType == "bordercolor"); }
        public static bool IsDDLTypeCulture(string pType)
        { return (pType == "culture"); }
        public static bool IsDDLTypeCSS(string pType)
        { return !StrFunc.IsEmpty(pType) && pType.ToLower() == "css"; }
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static bool IsDDLTypeCSSColor(string pType)
        { return !StrFunc.IsEmpty(pType) && pType.ToLower() == "csscolor"; }
        public static bool IsDDLTypeBusinessCenterId(string pType)
        { return (pType == "businesscenterid"); }
        public static bool IsDDLTypeBusinessCenter(string pType)
        { return (pType == "businesscenter"); }
        public static bool IsDDLTypeWeeklyRollConvention(string pType)
        { return (pType == "weeklyrollconvention"); }
        public static bool IsDDLTypePositionDay(string pType)
        { return (pType == "positionday"); }
        public static bool IsDDLTypeDay(string pType)
        { return (pType == "day"); }
        public static bool IsDDLTypeMonth(string pType)
        { return (pType == "month"); }
        public static bool IsDDLTypeHolidayName(string pType)
        { return (pType == "holidayname"); }
        public static bool IsDDLTypeHolidayType(string pType)
        { return (pType == "holidaytype"); }
        public static bool IsDDLTypeMenu(string pType)
        { return (pType == "menu"); }
        public static bool IsDDLTypePermission(string pType)
        { return (pType == "permission"); }
        public static bool IsDDLTypeCurrency(string pType)
        { return (pType == "currency"); }
        public static bool IsDDLTypeSumAmount(string pType)
        { return (pType == "sumamount"); }
        public static bool IsDDLTypeDayCountFraction(string pType)
        { return (pType == "daycountfraction"); }
        public static bool IsDDLTypeDayCountFraction_String(string pType)
        { return (pType == "daycountfraction_string"); }
        public static bool IsDDLTypeCustomisedInput(string pType)
        { return (pType == "customisedinput"); }
        public static bool IsEFSRateorIndex(string pType)
        { return (pType == "rateorindex"); }
        public static bool IsDDLTypeIndexUnit(string pType)
        { return (pType == "indexunit"); }
        public static bool IsDDLTypeRateType(string pType)
        { return (pType == "ratetype"); }
        public static bool IsDDLTypeRateExpression(string pType)
        { return (pType == "rateexpression"); }
        public static bool IsDDLTypeRateValueType(string pType)
        { return (pType == "ratevaluetype"); }
        public static bool IsDDLTypeVolatilityMatrix(string pType)
        { return (pType == "volatilitymatrix"); }
        public static bool IsDDLTypeFxCurve(string pType)
        { return (pType == "fxcurve"); }
        public static bool IsDDLTypeInformationProvider(string pType)
        { return (pType == "informationprovider"); }
        public static bool IsDDLTypeLogLevel(string pType)
        { return (pType == "loglevel"); }
        public static bool IsDDLTypeLogLevel_Inherited(string pType)
        { return (pType == "loglevel_inherited"); }
        public static bool IsDDLTypeRecursiveLevel(string pType)
        { return (pType == "recursivelevel"); }
        public static bool IsDDLTypePosRequestLevel(string pType)
        { return (pType == "posrequestlevel"); }
        //public static bool IsDDLTypeMarketTradedDerivative(string pType)
        //{ return (pType == "market_tradedderivative"); }
        //public static bool IsDDLTypeMarketShortIdentifier(string pType)
        //{ return (pType == "market_shortidentifier"); }
        public static bool IsDDLTypeMarketId(string pType)
        { return (pType == "market_id"); }
        public static bool IsDDLTypeMarket(string pType)
        { return (pType == "market"); }
        //public static bool IsDDLTypeMarketFull(string pType)
        //{ return (pType == "market_full"); }
        public static bool IsDDLTypeMissingUTI(string pType)
        { return (pType == "missinguti"); }
        public static bool IsDDLTypeMarketETD(string pType)
        { return (pType == "market_etd"); }
        public static bool IsDDLGrpMarketCNF(string pType)
        { return (pType == "grpmarket_cnf"); }
        public static bool IsDDLTypeClearingHouse(string pType)
        { return (pType == "clearinghouse"); }
        public static bool IsDDLTypeCssCustodian(string pType)
        { return (pType == "csscustodian"); }
        public static bool IsDDLTypeAllRatingValue(string pType)
        { return (pType.StartsWith("allratingvalue")); }
        public static bool IsDDLTypePeriod(string pType)
        { return (pType == "period"); }
        public static bool IsDDLTypePeriodMulti(string pType)
        { return (pType == "periodmulti"); }
        public static bool IsDDLTypeAllProcessType(string pType)
        { return (pType == "allprocesstype"); }
        public static bool IsDDLTypeProcessType(string pType)
        { return (pType == "processtype"); }
        public static bool IsDDLTypeRollConvention(string pType)
        { return (pType == "rollconvention"); }
        public static bool IsDDLTypeRollConvention_String(string pType)
        { return (pType == "rollconvention_string"); }
        public static bool IsDDLTypeDayType(string pType)
        { return (pType == "daytype"); }
        public static bool IsDDLTypeDayTypeEntry(string pType)
        { return (pType == "daytypeentry"); }
        public static bool IsDDLTypeRelativeToPaymentDT(string pType)
        { return (pType == "relativetopaymentdt"); }
        public static bool IsDDLTypeRelativeToResetDT(string pType)
        { return (pType == "relativetoresetdt"); }
        public static bool IsDDLTypeRelativeToFixingDT(string pType)
        { return (pType == "relativetofixingdt"); }
        public static bool IsDDLTypeMaturityRelativeTo(string pType)
        { return (pType == "maturityrelativeto"); }
        public static bool IsDDLTypeSymbolAlign(string pType)
        { return (pType == "symbolalign"); }
        public static bool IsDDLTypeRoleType(string pType)
        { return (pType == "roletype"); }
        public static bool IsDDLTypeBusinessDayConvention(string pType)
        { return (pType == "businessdayconvention"); }
        public static bool IsDDLTypeExerciseStyle(string pType)
        { return (pType == "exercisestyle"); }
        public static bool IsDDLOptionType(string pType)
        { return (pType == "optiontype"); }
        public static bool IsDDLTypePriceCurve(string pType)
        { return (pType == "pricecurve"); }
        public static bool IsDDLTypeCalculationRule(string pType)
        { return (pType == "calculationrule"); }
        public static bool IsDDLTypeRateTreatment(string pType)
        { return (pType == "ratetreatment"); }
        public static bool IsDDLTypeCompoundingMethod(string pType)
        { return (pType == "compoundingmethod"); }
        public static bool IsDDLTypeCaptureGUIType(string pType)
        { return (pType == "captureguitype"); }
        public static bool IsDDLTypeOTCmlSettlementType(string pType)
        { return (pType == "otcmlsettlementtype"); }
        public static bool IsDDLTypeSettlementType(string pType)
        { return (pType == "settlementtype"); }
        public static bool IsDDLTypeStandardSettlementStyle(string pType)
        { return (pType == "standardsettlementstyle"); }
        public static bool IsDDLTypeStatusProcess(string pType)
        { return (pType == "statusprocess"); }
        public static bool IsDDLTypeStrikeQuoteBasis(string pType)
        { return (pType == "strikequotebasis"); }
        public static bool IsDDLTypeQuoteBasis(string pType)
        { return (pType == "quotebasis"); }
        public static bool IsDDLTypePremiumQuoteBasis(string pType)
        { return (pType == "premiumquotebasis"); }
        public static bool IsDDLTypeCutName(string pType)
        { return (pType == "cutname"); }
        public static bool IsDDLTypeFutureClearing(string pType)
        { return (pType == "futureclearing"); }
        public static bool IsDDLTypeSecurityClearing(string pType)
        { return (pType == "securityclearing"); }
        public static bool IsDDLTypeSecurityClass(string pType)
        { return (pType == "securityclass"); }
        public static bool IsDDLTypeSecurityClassIAS(string pType)
        { return (pType == "securityclassIAS"); }
        public static bool IsDDLTypePaymentType(string pType)
        { return (pType == "paymenttype"); }
        public static bool IsDDLTypeCommissionDenomination(string pType)
        { return (pType == "commissiondenomination"); }
        public static bool IsDDLTypePriceExpression(string pType)
        { return (pType == "priceexpression"); }
        public static bool IsDDLTypeDeterminationMethodValuation(string pType)
        { return (pType == "determinationmethodvaluation"); }
        /// EG 20140702 New
        public static bool IsDDLTypeDeterminationMethodValuationPrice(string pType)
        { return (pType == "determinationmethodvaluationprice"); }
        public static bool IsDDLTypeValuationTime(string pType)
        { return (pType == "valuationtime"); }
        public static bool IsDDLReturnType(string pType)
        { return (pType == "returntype"); }
        public static bool IsDDLDayType(string pType)
        { return (pType == "daytype"); }
        public static bool IsDDLDayTypeEntry(string pType)
        { return (pType == "daytypeentry"); }
        public static bool IsDDLDividendEntitlement(string pType)
        { return (pType == "dividendentitlement"); }
        /// EG 20140702 New
        public static bool IsDDLDividendPeriod(string pType)
        { return (pType == "dividendperiod"); }
        public static bool IsDDLDividendAmountType(string pType)
        { return (pType == "dividendamounttype"); }
        public static bool IsDDLDividendDateReference(string pType)
        { return (pType == "dividenddatereference"); }
        public static bool IsDDLReferenceAmount(string pType)
        { return (pType == "referenceamount"); }
        public static bool IsDDLPayout(string pType)
        { return (pType == "payout"); }
        public static bool IsDDLCallPut(string pType)
        { return (pType == "callput"); }
        public static bool IsDDLTypeRequestMode(string pType)
        { return (pType == "requestmode"); }
        public static bool IsDDLTypeTouchCondition(string pType)
        { return (pType == "touchcondition"); }
        public static bool IsDDLTypeTriggerCondition(string pType)
        { return (pType == "triggercondition"); }
        public static bool IsDDLFxBarrierType(string pType)
        { return (pType == "fxbarriertype"); }
        public static bool IsDDLCtrlLastUser(string pType)
        { return (pType == "ctrllastuser"); }
        public static bool IsDDLExtlType(string pType)
        { return (pType == "extltype"); }
        //
        public static bool IsDDLStatusActivation(string pType)
        { return (pType == "statusactivation"); }
        public static bool IsDDLStatusEnvironment(string pType)
        { return (pType == "statusenvironment"); }
        public static bool IsDDLStatusPriority(string pType)
        { return (pType == "statuspriority"); }
        public static bool IsDDLStatusCheck(string pType)
        { return (pType == "statuscheck"); }
        public static bool IsDDLStatusMatch(string pType)
        { return (pType == "statusmatch"); }
        public static bool IsDDLStatusProcess(string pType)
        { return (pType == "statusprocess"); }
        // CC 20120625 17939
        public static bool IsDDLStatusTask(string pType)
        { return (pType == "statustask"); }
        // CC 20120625 17870
        public static bool IsDDLActorAmountType(string pType)
        { return (pType == "actoramounttype"); }

        //
        public static bool IsDDLCtrlStatus(string pType)
        { return (pType == "ctrlstatus"); }
        public static bool IsDDLCtrlSending(string pType)
        { return (pType == "ctrlsending"); }
        public static bool IsDDLRightType(string pType)
        { return (pType == "righttype"); }
        public static bool IsDDLCommitMode(string pType)
        { return (pType == "commitmode"); }
        public static bool IsDDLCommitMode_Inherited(string pType)
        { return (pType == "commitmode_inherited"); }
        public static bool IsDDLIn_Out(string pType)
        { return (pType == "in_out"); }
        public static bool IsDDLStartInfoStyle(string pType)
        { return (pType == "startinfostyle"); }
        public static bool IsDDLInputSourceDataStyle(string pType)
        { return (pType == "inputsourcedatastyle"); }
        public static bool IsDDLOutputSourceDataStyle(string pType)
        { return (pType == "outputsourcedatastyle"); }
        public static bool IsDDLOutputTargetDataStyle(string pType)
        { return (pType == "outputtargetdatastyle"); }
        public static bool IsDDLIOSerializeMode(string pType)
        { return (pType == "ioserializemode"); }
        public static bool IsDDLGlobal_Elementary(string pType)
        { return (pType == "global_elementary"); }
        public static bool IsDDLVisibility(string pType)
        { return (pType == "visibility"); }
        public static bool IsDDLMatchingMode(string pType)
        { return (pType == "matchingmode"); }
        public static bool IsDDLWriteMode(string pType)
        { return (pType == "writemode"); }
        public static bool IsDDLAlignment(string pType)
        { return (pType == "alignment"); }
        public static bool IsDDLParameterDirection(string pType)
        { return (pType == "parameterdirection"); }
        public static bool IsDDLRuleOnError(string pType)
        { return (pType == "ruleonerror"); }
        public static bool IsDDLRetCodeOnNoData(string pType)
        { return (pType == "retcodeonnodata"); }
        public static bool IsDDLRDBMS(string pType)
        { return (pType == "rdbms"); }
        public static bool IsDDLPayRecType(string pType)
        { return (pType == "payrec"); }
        // EG 20230526 [WI640] Gestion des parties PAYER/RECEIVER sur facturation (BENEFICIARY/PAYER)
        public static bool IsDDLInvoicePayRecType(string pType)
        { return (pType == "invpayrec"); }

        //PL 20180531
        public static bool IsDDLCRDRType(string pType)
        { return (pType == "crdr"); }
        public static bool IsDDLDepositWithdrawalType(string pType)
        { return (pType == "depositwithdrawal"); }
        public static bool IsDDLLendBorrowType(string pType)
        { return (pType == "lendborrow"); }
        public static bool IsDDLBuyerSellerType(string pType)
        { return (pType == "buyerseller"); }
        public static bool IsDDLPositionType(string pType)
        { return (pType == "position"); }
        public static bool IsDDLMandatoryOptionalType(string pType)
        { return (pType == "mandatoryoptional"); }
        public static bool IsDDLBuySellType(string pType)
        { return (pType == "buysell"); }
        public static bool IsDDLBuySellFixType(string pType)
        { return (pType == "fixbuysell"); }
        public static bool IsDDLBuySellCur1Type(string pType)
        { return (pType == "buysellcur1"); }
        public static bool IsDDLXXXYYYType(string pType)
        { return (pType == "xxxyyy"); }
        //Create_DropDownList_For_Referential Step2: Create a new testing method
        public static bool IsUnderlyingAsset_Rate(string pType)
        { return (pType == "underlyingasset_rate"); }
        public static bool IsUnderlyingAsset_Rating(string pType)
        { return (pType == "underlyingasset_rating"); }
        public static bool IsUnderlyingAsset_ETD(string pType)
        { return (pType == "underlyingasset_etd"); }
        public static bool IsUnderlyingAsset_ETD_WithCombinedValues(string pType)
        { return (pType == "underlyingasset_etd_combined"); }
        /// EG 20140702 New
        public static bool IsUnderlyingAsset_ReturnSwap(string pType)
        { return (pType == "underlyingasset_returnswap"); }
        public static bool IsUnderlyingAsset_Collateral(string pType)
        { return (pType == "underlyingasset_collateral"); }
        public static bool IsIOElementType(string pType)
        { return (pType == "ioelementtype"); }
        public static bool IsUnderlyingAsset(string pType)
        { return (pType == "underlyingasset"); }
        public static bool IsDDLAgregateFunction(string pType)
        { return (pType == "agregatefunction"); }
        public static bool IsDDLExchangeType(string pType)
        { return (pType == "exchangetype"); }
        public static bool IsDDLEarExchangeEnum(string pType)
        { return (pType.ToLower() == "earexchangeenum"); }
        public static bool IsDDLEarCommonEnum(string pType)
        { return (pType.ToLower() == "earcommonenum"); }
        public static bool IsDDLCBExchangeType(string pType)
        { return (pType == "cbexchangetype"); }
        public static bool IsDDLQuoteExchangeType(string pType)
        { return (pType == "quoteexchangetype"); }
        public static bool IsDDLAmountType(string pType)
        { return (pType == "amounttype"); }
        public static bool IsDDLCheckModeEnum(string pType)
        { return (pType.ToLower() == "checkmodeenum"); }
        public static bool IsDDLReturnSPParamTypeEnum(string pType) //GP 20070124 
        { return (pType.ToLower() == "returnspparamtypeenum"); }
        public static bool IsDDLFlowTypeEnum(string pType)
        { return (pType == "flowtypeenum"); }
        public static bool IsDDLFungibilityMode(string pType)
        { return (pType == "fungibilitymode"); }
        public static bool IsDDLPreSettlementMethod(string pType)
        { return (pType == "presettlementmethodenum"); }
        public static bool IsDDLActor(string pType)
        { return (pType == "actor"); }
        public static bool IsDDLActorInvoiceBeneficiary(string pType)
        { return (pType == "actorinvoicebeneficiary"); }
        public static bool IsOptDDLActorInvoiceBeneficiary(string pType)
        { return (pType == "optactorinvoicebeneficiary"); }
        public static bool IsDDLActorEntity(string pType)
        { return (pType == "actorentity"); }
        public static bool IsDDLActorSettltOffice(string pType)
        { return (pType == "actorsettltoffice"); }
        public static bool IsDDLActorMarginRequirementOffice(string pType)
        { return (pType == "actorMarginRequirementOffice".ToLower()); }
        public static bool IsConfirmationRecipientType(string pType)
        { return (pType == "confirmationrecipienttype"); }
        public static bool IsConfAccessKeyEnum(string pType)
        { return (pType == "confaccesskeyenum"); }
        public static bool IsDDLStepLifeEnum(string pType)
        { return (pType == "steplifeenum"); }
        public static bool IsFeeFormulaEnum(string pType)
        { return (pType == "feeformulaenum"); }
        public static bool IsAssessmentBasisEnum(string pType)
        { return (pType == "assessmentbasisenum"); }
        public static bool IsFee(string pType)
        { return (pType == "fee"); }
        public static bool IsFeeMatrix(string pType)
        { return (pType == "feematrix"); }
        public static bool IsFeeSchedule(string pType)
        { return (pType == "feeschedule"); }
        public static bool IsFeesCalcultationMode(string pType)
        { return (pType == "feescalculationmode"); }
        public static bool IsFeeScopeEnum(string pType)
        { return (pType == "feescopeenum"); }
        public static bool IsFeeExchangeTypeEnum(string pType)
        { return (pType == "feeexchangetypeenum"); }
        public static bool IsTypePartyEnum(string pType)
        { return (pType == "typepartyenum"); }
        public static bool IsTypeSidePartyEnum(string pType)
        { return (pType == "typesidepartyenum"); }
        //CC 20120621 TRIM 17921
        public static bool IsTypeInformationMessage(string pType)
        { return (pType == "typeinformationmessage"); }
        public static bool IsDDLTypeHelpDisplay(string pType)
        { return (pType == "typehelpdisplay"); }
        //CC 20140617 [19923]
        public static bool IsDDLRequestTrackMode(string pType)
        { return (pType == "requesttrackmode"); }
        public static bool IsTypeGroupTrackerEnum(string pType)
        { return (pType == "grouptrackerenum"); }
        public static bool IsTypeReadyStateEnum(string pType)
        { return (pType == "readystateenum"); }
        public static bool IsTypeStatusTrackerEnum(string pType)
        { return (pType == "statustrackerenum"); }
        public static bool IsTypeMarketEnum(string pType)
        { return (pType == "typemarketenum"); }
        public static bool IsTradeSideEnum(string pType)
        { return (pType == "tradesideenum"); }
        public static bool IsTaxApplicationEnum(string pType)
        { return (pType == "taxapplicationenum"); }
        public static bool IsPaymentRuleEnum(string pType)
        { return (pType == "paymentruleenum"); }
        public static bool IsInvoiceApplicationPeriodEnum(string pType)
        { return (pType == "invoiceapplicationperiodenum"); }
        public static bool IsBracketApplicationEnum(string pType)
        { return (pType == "bracketapplicationenum"); }
        public static bool IsInvoicingSortEnum(string pType)
        { return (pType == "invoicingsortenum"); }
        public static bool IsInvoicingTradeDetailEnum(string pType)
        { return (pType == "invoicingtradedetailenum"); }
        public static bool IsMaturityMonthYearFmtEnum(string pType)
        { return (pType == "maturitymonthyearfmtenum"); }
        public static bool IsMaturityMonthYearIncrUnitEnum(string pType)
        { return (pType == "maturitymonthyearincrunitenum"); }
        public static bool IsCfiCodeCategoryEnum(string pType)
        { return (pType == "cficodecategoryenum"); }
        public static bool IsDerivativeExerciseStyleEnum(string pType)
        { return (pType == "derivativeexercisestyleenum"); }
        public static bool IsMatchStatusEnum(string pType)
        { return (pType == "matchstatusenum"); }
        // EG 20150722 New PositionDateType
        public static bool IsDDLPositionDateType(string pType)
        { return (pType == "positiondatetype"); }
        public static bool IsDDLPositionOtcSideView(string pType)
        { return (pType == "positionotcsideview"); }
        public static bool IsDDLPositionSideView(string pType)
        { return (pType == "positionsideview"); }
        public static bool IsDDLActorSideView(string pType)
        { return (pType == "actorsideview"); }
        public static bool IsDDLAggregateDateTypeView(string pType)
        { return (pType == "aggregatedatetypeview"); }
        // CC 20170127
        public static bool IsDDLFinancialProductTypeView(string pType)
        { return (pType == "financialproducttypeview"); }
        public static bool IsDDLActivityTypeView(string pType)
        { return (pType == "activitytypeview"); }
        public static bool IsDDLResultsType(string pType)
        { return (pType == "resultstype"); }
        public static bool IsDDLPeriodReportType(string pType)
        { return (pType == "periodreporttype"); }
        public static bool IsDDLHolidayMethodOfAdjustment(string pType)
        { return (pType == "holidaymethodofadjustment"); }
        public static bool IsDDLTypeGProduct_Trading(string pType)
        { return (pType == "gproduct_trading"); }
        public static bool IsDDLHeaderType(string pType)
        { return (pType == "headertype"); }
        public static bool IsDDLFooterType(string pType)
        { return (pType == "footertype"); }
        public static bool IsDDLHeaderTitlePosition(string pType)
        { return (pType == "headertitleposition"); }
        public static bool IsDDLFooterLegend(string pType)
        { return (pType == "footerlegend"); }
        public static bool IsDDLHeaderFooterSort(string pType)
        { return (pType == "headerfootersort"); }
        public static bool IsDDLHeaderFooterSummary(string pType)
        { return (pType == "headerfootersummary"); }
        public static bool IsDDLAmountFormat(string pType)
        { return (pType == "amountformat"); }
        public static bool IsDDLBannerPosition(string pType)
        { return (pType == "bannerposition"); }
        public static bool IsDDLAssetBannerStyle(string pType)
        { return (pType == "assetbannerstyle"); }
        public static bool IsDDLSectionBannerStyle(string pType)
        { return (pType == "sectionbannerstyle"); }
        public static bool IsDDLTimestampType(string pType)
        { return (pType == "timestamptype"); }
        public static bool IsDDLUTISummary(string pType)
        { return (pType == "utisummary"); }
        public static bool IsDDLStrategyTypeScheme_Extended(string pType)
        { return (pType == "strategytypescheme_extended"); }
        // CC 20140723
        public static bool IsDDLMarginType(string pType)
        { return (pType == "margintype"); }
        // CC 20150312
        public static bool IsDDLMarginingMode(string pType)
        { return (pType == "marginingmode"); }
        // CC 20140724
        public static bool IsDDLMarginAssessmentBasis(string pType)
        { return (pType == "marginassessmentbasis"); }
        // EG 20150320 (POC]
        public static bool IsDDLFundingType(string pType)
        { return (pType == "fundingtype"); }
        // EG 20151102 [21465] New
        public static bool IsDDLDenOptionActionType(string pType)
        { return (pType == "denoptionactiontype"); }
        public static bool IsDDLOptionITM_ATM_OTM(string pType)
        { return (pType == "optionitm_atm_otm"); }
        // FI 20181002 [24219] Add method
        public static bool IsDDLCnfTypeMessage(string pType)
        { return (pType == "cnftypemessage"); }

        #endregion

        #region Constantes de Paramètres TypeField pour la Fonction Javascript du pop-up référenciel
        public const string
            KeyField = "KF",   //ex ACTOR.IDENTIFIER
            DataKeyField = "DKF";  //ex ACTOR.IDA
        #endregion

        /// <summary>
        /// Renvoi la valeur de l'attribut "Descripton" de l'enum
        /// </summary>
        /// <param name="pValue"></param>
        /// <returns></returns>
        public static string GetDescription(Enum pValue)
        {
            FieldInfo fi = pValue.GetType().GetField(pValue.ToString());
            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return (attributes.Length > 0) ? attributes[0].Description : string.Empty;
        }

        /// <summary>
        /// Get the quote relative value to the input asset category
        /// </summary>
        /// <param name="pAssetCategory"></param>
        /// <returns>the converted category, the default category (Bond) when the input category is not recognized</returns>
        public static Cst.UnderlyingAsset ConvertToUnderlyingAsset(string pUnderlyingCategory)
        {

            Cst.UnderlyingAsset ret = default;

            try
            {
                ret = (Cst.UnderlyingAsset)System.Enum.Parse(typeof(Cst.UnderlyingAsset), pUnderlyingCategory);
            }
            catch (ArgumentException)
            { }

            return ret;
        }

        # region GroupingSet

        public enum GroupingSet
        {
            [XmlEnum("0")]
            Unknown = 0,
            [XmlEnum("1")]
            Details = 1,
            [XmlEnum("2")]
            SubTotal = 2,
            [XmlEnum("4")]
            Total = 4,
            [XmlEnum("5")]
            TotalDetails = 5,
            [XmlEnum("6")]
            TotalSubTotal = 6,
            [XmlEnum("7")]
            TotalSubTotalDetails = 7,
        }

        public static GroupingSet MaxGroupingSet { get { return GroupingSet.TotalSubTotalDetails; } }

        public static bool IsWithTotalOrSubTotal(GroupingSet pGroupingSet)
        {
            return (pGroupingSet & (GroupingSet.SubTotal | GroupingSet.Total)) > 0;
        }

        public static bool IsWithSubTotal(GroupingSet pGroupingSet)
        {
            return (pGroupingSet & (GroupingSet.SubTotal)) > 0;
        }

        public static bool IsWithDetails(GroupingSet pGroupingSet)
        {
            return (pGroupingSet & (GroupingSet.Details)) > 0;
        }

        public static GroupingSet CastDataColumnToGroupingSet(DataRow pRow, string pColumnName)
        {
            if (pRow == null || String.IsNullOrEmpty(pColumnName))
            {
                throw new ArgumentException("CastDataColumnToGroupingSet input parameters not null or empty");
            }

            GroupingSet ret;
            if (pRow[pColumnName] is DBNull)
                ret = GroupingSet.Unknown;
            else
                ret = (Cst.GroupingSet)Convert.ToInt32(pRow[pColumnName]);
            return ret;

        }

        public static string CastGroupingSetToDDLValue(GroupingSet pGroupingSet)
        {
            return Convert.ToInt32(pGroupingSet).ToString();
        }

        #endregion GroupingSet

        // 20120820 MF - Ticket 18073
        /// <summary>
        /// List of the possible format style for displaying the converted fractional part of trade prices and asset strikes
        /// </summary>
        public enum PriceFormatStyle
        {
            /// <summary>
            /// Display the converted fractional part using a point as separator from the integer part
            /// </summary>
            Default = 0,
            /// <summary>
            /// Displaying the converted fractional part using a fractional expression 
            /// having as denominator the numerical base of conversion and numerator the converted (truncated) fractional part
            /// </summary>
            Fraction,
            /// <summary>
            /// Displaying the converted fractional part using a slash symbol 
            /// having as denominator the numerical base of conversion and numerator the converted fractional part, 
            /// preserving its own fractional part
            /// </summary>
            FractionDecimal,
            /// <summary>
            /// Displaying the converted (truncated) fractional part using a power symbol as separator from the related integer part
            /// </summary>
            Power,
            /// <summary>
            /// Displaying the converted (truncated) fractional part using an apex symbol as separator from the related integer part
            /// </summary>
            Apostrophe,
            /// <summary>
            /// Displaying the converted (truncated) fractional part using a minus symbol as separator from the related integer part
            /// </summary>
            Minus,
        }

        /// <summary>
        /// Attributs associés à InputSourceDataStyle
        /// </summary>
        /// FI 20131109 Add classe
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
        public sealed class InputSourceDataStyleAttribute : Attribute
        {
            #region Members

            #endregion Members

            #region Accessors
            /// <summary>
            /// Description du fichier
            /// </summary>
            public string Description
            {
                get;
                set;
            }

            /// <summary>
            ///  
            /// </summary>
            /// FI 20220325 [XXXXX] Add
            public Boolean IsPRISMA
            {
                get;
                set;
            }

            /// <summary>
            /// true si importation direct d'un fichier 
            /// </summary>
            // FI 20220325 [XXXXX] Add
            public Boolean IsDirectImport
            {
                get;
                set;
            }

            /// <summary>
            /// true si réduction de fichier avant importation
            /// </summary>
            /// FI 20220325 [XXXXX] Add 
            public Boolean IsFilterFile
            {
                get;
                set;
            }

            #endregion Accessors
        }

        /// <summary>
        ///  type de journalisation des actions utilisateurs 
        /// </summary>
        /// FI 20141021 [20350] Modify
        public enum RequestTrackMode
        {
            /// <summary>
            /// Aucune journalisation
            /// </summary>
            NONE,
            /// <summary>
            ///  Jounalisation des consultations utilisateurs (de type LST ou XML) 
            /// </summary>
            /// FI 20141021 [20350] Add
            CONSULT,
            /// <summary>
            ///  Jounalisation des opérations utilisateurs 
            /// </summary>
            /// FI 20141021 [20350] Add
            PROCESS,
            /// <summary>
            ///  Jounalisation des consultations et des opérations
            /// </summary>
            ALL,
        }

        #region public enum LoggerParameterLink
        /// <summary>
        /// 
        /// </summary>
        /// PM 20200615 [XXXXX] Ajout pour new log
        public enum LoggerParameterLink
        {
            // Indique que le paramètre est un ID (alimentant les données IDDATA et IDDATAIDENT de la table de log)
            IDDATA,
            // Indique que le paramètre est le nom d'une Queue (alimentant la donnée QUEUEMSG de la table de log)
            QUEUEMSG,
            // Indique que le paramètre est une longue chaine de caractères (Alimenté avec les paramètres d'une tâche)
            LODATATXT,
        }
        #endregion public enum LoggerParameterLink

        #region InitialMarginCurrencyTypeEnum
        /// <summary>
        /// Type de devise pour le résultat du calcul du déposit SPAN 2
        /// </summary>
        // PM 20231030 [26547][WI735] Ajout
        public enum InitialMarginCurrencyTypeEnum
        {
            /// <summary>
            /// Devise du contrat
            /// </summary>
            [ResourceAttribut(Resource = "InitialMarginCurrencyTypeContract")]
            ContractCurrency,
            /// <summary>
            /// Devise de contrevaleur de la chambre
            /// </summary>
            [ResourceAttribut(Resource = "InitialMarginCurrencyTypeClearingHouse")]
            ClearingHouseCurrency,
        }
        #endregion InitialMarginCurrencyTypeEnum
    }

    #region public class ArrFunc
    ///	<summary>
    ///	Management of Array
    ///	</summary>
    public static class ArrFunc
    {
        #region public Count
        public static int Count(ICollection pCol)
        {
            int ret = 0;
            if (IsFilled(pCol))
                ret = pCol.Count;
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCol"></param>
        /// <returns></returns>
        /// FI 20240801 Add
        public static int Count(IEnumerable pCol)
        {
            int ret = 0;
            if (null != pCol)
            {
                foreach (var item in pCol)
                    ret++;
            }
            return ret;
        }

        #endregion public Count

        #region public IsEmpty
        public static bool IsEmpty(ICollection pCol)
        {
            return ((null == pCol) || (0 == pCol.Count));
        }
        #endregion public IsEmpty

        #region public IsFilled
        public static bool IsFilled(ICollection pCol)
        {
            return !ArrFunc.IsEmpty(pCol);
        }
        #endregion public IsFilled


        #region public IsAllElementEmpty
        public static bool IsAllElementEmpty(ICollection pCol)
        {
            return IsAllElementEmpty(TransformCollectionToArrayList(pCol));
        }
        public static bool IsAllElementEmpty(ArrayList pArrayList)
        {
            bool isFilled = (IsEmpty(pArrayList) == false);
            //
            if (isFilled)
            {
                bool isAllEmpty = true;
                //
                for (int i = 0; i < pArrayList.Count; i++)
                {
                    if (null != pArrayList[i])
                        isAllEmpty = false;
                }
                //
                isFilled = (false == isAllEmpty);
            }
            //
            return (false == isFilled);
        }
        #endregion public IsAllElementEmpty
        // RD 20100111 [16818] MS Excel® file import
        #region public IsAllElementEmpty2
        public static bool IsAllElementEmpty2(ICollection pCol)
        {
            return IsAllElementEmpty2(TransformCollectionToArrayList(pCol));
        }
        public static bool IsAllElementEmpty2(ArrayList pArrayList)
        {
            bool isAllElementEmpty = IsEmpty(pArrayList);
            //
            if (false == isAllElementEmpty)
            {
                isAllElementEmpty = true;
                //
                for (int i = 0; i < pArrayList.Count; i++)
                {
                    if ((null != pArrayList[i]) && StrFunc.IsFilled(pArrayList[i].ToString()))
                    {
                        isAllElementEmpty = false;
                        break;
                    }
                }
            }
            //
            return isAllElementEmpty;
        }
        #endregion public IsAllElementEmpty2

        #region public GetStringList
        public static string GetStringList(ArrayList pArrayList)
        {
            return GetStringList(pArrayList, ",");
        }
        public static string GetStringList(ArrayList pArrayList, string pSeparator)
        {
            string ret = string.Empty;

            if (ArrFunc.IsFilled(pArrayList))
            {
                for (int i = 0; i < pArrayList.Count; i++)
                {
                    ret += pArrayList[i].ToString();
                    if (i != pArrayList.Count - 1)
                        ret += pSeparator;
                }
            }
            return ret;
        }

        public static string GetStringList(ICollection pCol)
        {
            return GetStringList(TransformCollectionToArrayList(pCol));
        }
        public static string GetStringList(ICollection pCol, string pSeparator)
        {
            return GetStringList(TransformCollectionToArrayList(pCol), pSeparator);
        }
        #endregion public GetStringList

        #region public GetFirstItem
        public static IComparable GetFirstItem(IComparable[] pArray, IComparable pObject)
        {
            IComparable ret = null;
            int index = GetFirstItemIndex(pArray, pObject);
            //
            if (index != -1)
                ret = pArray[index];
            return ret;
        }
        #endregion GetFirstItem

        public static T[] ConcatArray<T>(T[] first, T[] second) where T : IEnumerable
        {
            T[] resultingArray = null;

            int lengthFirst = 0;

            if (!ArrFunc.IsEmpty(first))
                lengthFirst = first.Length;

            int lengthSecond = 0;

            if (!ArrFunc.IsEmpty(second))
                lengthSecond = second.Length;

            if (lengthFirst + lengthSecond > 0)
            {
                resultingArray = new T[lengthFirst + lengthSecond];

                if (lengthFirst > 0)
                    first.CopyTo(resultingArray, 0);

                if (lengthSecond > 0)
                    second.CopyTo(resultingArray, lengthFirst);
            }

            return resultingArray;
        }

        /// <summary>
        /// The map() method creates a new array populated with the results of calling a provided function on every element in the calling array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="arr"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        /// <see cref="https://www.codeproject.com/Articles/451628/Efficient-Map-Operations-for-Arrays-in-Csharp"/>
        public static TResult[] Map<T, TResult>(T[] arr, Func<T, TResult> f)
        {
            if (arr == null)
                throw new ArgumentException("arr is null", "arr");

            TResult[] ret = new TResult[arr.Length];
            for (int i = 0; i < arr.Length; ++i)
                ret[i] = f(arr[i]);

            return ret;
        }


        #region public GetFirstItemIndex
        public static int GetFirstItemIndex(IComparable[] pArray, IComparable pObject)
        {
            int ret = -1;
            if (ArrFunc.IsFilled(pArray))
            {
                for (int i = 0; i < pArray.Length; i++)
                {
                    if (pArray[i].CompareTo(pObject) == 0)
                    {
                        ret = i;
                        break;
                    }
                }
            }
            return ret;
        }
        #endregion GetFirstItemIndex

        #region public ExistInArray
        /// <summary>
        ///  Retourne true si {pObject} existe dans le tableau {pArray}
        /// </summary>
        /// <param name="pArray"></param>
        /// <param name="pObject"></param>
        /// <returns></returns>
        public static bool ExistInArray(IComparable[] pArray, IComparable pObject)
        {
            bool ret = false;
            if (-1 != GetFirstItemIndex(pArray, pObject))
                ret = true;
            return ret;
        }
        /* FI 2170323 [XXXXX] Mise en commentaire => méthode trop dangereuse (usage de Object)
        public static bool ExistInArray(ICollection pCol, Object pObject)
        {
            return ExistInArray(TransformCollectionToArrayList(pCol), pObject);
        }
        public static bool ExistInArray(ArrayList pArrayList, Object pObject)
        {
            bool ret = false;
            //
            if (IsFilled(pArrayList))
            {
                for (int i = 0; i < pArrayList.Count; i++)
                {
                    if (pObject == pArrayList[i])
                    {
                        ret = true;
                        break;
                    }
                }
            }
            //
            return ret;
        }
         */
        /// <summary>
        ///  Retourne true si {pString} existe dans la collection {pCol}
        /// </summary>
        /// <param name="pCol">Collection contenant des string</param>
        /// <param name="pString"></param>
        /// <returns></returns>
        public static bool ExistInArrayString(ICollection pCol, string pString)
        {
            return ExistInArrayString(TransformCollectionToArrayList(pCol), pString);
        }
        private static bool ExistInArrayString(ArrayList pArrayList, string pObject)
        {
            bool ret = false;
            //
            if (IsFilled(pArrayList))
            {
                for (int i = 0; i < pArrayList.Count; i++)
                {
                    if (pObject.Trim() == pArrayList[i].ToString().Trim())
                    {
                        ret = true;
                        break;
                    }
                }
            }
            //
            return ret;
        }
        #endregion

        #region public CountNbOf
        public static int CountNbOf(Array pArray, IComparable pValue)
        {
            int ret = 0;
            if (ArrFunc.IsFilled(pArray))
            {
                for (int i = 0; i < pArray.Length; i++)
                {
                    if (pValue.CompareTo(pArray.GetValue(i)) == 0)
                        ret++;
                }
            }
            return ret;
        }
        #endregion

        #region ConvertToStringArray
        public static string[] ConvertIntArrayToStringArray(int[] pIntArray)
        {
            string[] ret = null;
            ArrayList al = new ArrayList();
            //
            string[] toto = ArrFunc.Map<int, string>(pIntArray, (a) => { return a.ToString(); });


            if (ArrFunc.Count(pIntArray) > 0)
            {
                for (int i = 0; i < ArrFunc.Count(pIntArray); i++)
                    al.Add(pIntArray[i].ToString());
            }
            //
            if (ArrFunc.IsFilled(al))
            {
                ret = (string[])al.ToArray(typeof(string));
            }
            return ret;
        }
        #endregion

        #region private  TransformCollectionToArrayList
        public static ArrayList TransformCollectionToArrayList(ICollection pCol)
        {
            return new ArrayList(pCol);
        }
        #endregion private  TransformArrayToArrayList

        /// <summary>
        /// Split a collection into n parts with LINQ
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="parts"></param>
        /// <returns></returns>
        /// FI 20200522 [XXXXX] code from stackoverflow
        /// https://stackoverflow.com/questions/438188/split-a-collection-into-n-parts-with-linq
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> list, int parts)
        {
            int i = 0;
            var splits = from item in list
                         group item by i++ % parts into part
                         select part.AsEnumerable();
            return splits;
        }
    }
    #endregion class ArrFunc
    #region public class BoolFunc
    ///	<summary>
    ///	Management of boolean
    ///	</summary>
    public sealed class BoolFunc
    {
        #region public IsFalse
        /// <summary>
        /// Return True si la chaîne est: null, empty, 0, False, F, No, N, ...
        /// </summary>
        /// <param name="pString"></param>
        /// <returns></returns>
        public static bool IsFalse(string pString)
        {
            bool ret = true;
            //
            if (StrFunc.IsFilled(pString))
            {
                pString = pString.Trim().ToUpper();
                ret = (("0" == pString) || ("FALSE" == pString) || ("F" == pString) || ("NO" == pString) || ("N" == pString) || ("OFF" == pString));
            }
            //
            return ret;
        }
        public static bool IsFalse(object pObj)
        {
            return IsFalse((null == pObj) ? string.Empty : pObj.ToString());
        }
        public static bool IsFalse(Nullable<bool> pBool)
        {
            return (null == pBool) || (false == pBool);
        }
        #endregion
        #region public IsTrue
        public static bool IsTrue(object pObject)
        {
            return !BoolFunc.IsFalse(pObject);
        }
        #endregion
        #region public IsTrue
        public static bool IsTrue(Nullable<bool> pBool)
        {
            return !BoolFunc.IsFalse(pBool);
        }
        #endregion


        #region IsEmpty
        public static bool IsEmpty(Nullable<bool> pBool)
        {
            return ((null == pBool));
        }
        #endregion
        #region IsFilled
        public static bool IsFilled(Nullable<bool> pBool)
        {
            return !BoolFunc.IsEmpty(pBool);
        }
        #endregion
    }
    #endregion
    #region public class DecFunc
    public sealed class DecFunc
    {
        #region public IsDecimal
        /// <summary>
        /// La string represente-t-elle une donnée décimale (ex 1.55 ou 1 si Invariant culture)
        /// <para>si Invariant Culture {Fr} retourne true si pString vaut 1.55 ou 1 ou 1500</para>
        /// <para>si Culture {Fr} retourne true si pString vaut 1.55 ou 1 ou 1,55 ou 1 500</para>
        /// </summary>
        /// <param name="pString"></param>
        /// <returns></returns>
        public static bool IsDecimal(string pString)
        {
            Regex regex = new Regex(EFSRegex.RegularExpression(EFSRegex.TypeRegex.RegexDecimalExtend));
            return regex.IsMatch(pString);
        }
        #endregion IsDecimal

        #region public IsDecimalNotInteger
        public static bool IsDecimalNotInteger(string pString)
        {
            bool bRet = false;
            if (IsDecimal(pString))
            {
                Decimal decValue = DecFunc.DecValue(pString);
                Decimal intValue = Decimal.Truncate(decValue);
                //
                bRet = (decValue != intValue);
            }
            return bRet;
        }
        #endregion

        #region public DecValue
        /// <summary>
        /// Retourne le résultat de la convertion d'une string en décimal
        /// <para>Intreprète le . comme séparateur de décimal s'il est unique</para>
        /// <para>Usage de la culture du Thread</para>
        /// </summary>
        /// <param name="pStr"></param>
        /// <returns></returns>
        public static decimal DecValue(string pStr)
        {
            return DecValue(pStr, Thread.CurrentThread.CurrentCulture);
        }
        /// <summary>
        /// Retourne le résultat de la convertion d'une string en décimal
        /// <para>Intreprète le . comme séparateur de décimal s'il est présent qu'une seule fois</para>
        /// </summary>
        /// <param name="pStr"></param>
        /// <param name="pCultureInfo"></param>
        /// <returns></returns>
        public static decimal DecValue(string pStr, CultureInfo pCultureInfo)
        {
            decimal decValue = 0;
            if (StrFunc.IsFilled(pStr))
            {
                string str = StrFunc.ReplaceDecimalSeparatorInvariantToCulture(pStr, pCultureInfo);
                decValue = Decimal.Parse(str, pCultureInfo);
            }
            return decValue;

        }
        /// <summary>
        /// Retourne le résultat de la convertion d'une string au format invariant  en décimal
        /// </summary>
        /// <param name="pStr"></param>
        /// <returns></returns>
        public static decimal DecValueFromInvariantCulture(string pStr)
        {

            decimal decValue = 0;
            if (StrFunc.IsFilled(pStr))
            {
                decValue = Decimal.Parse(pStr, CultureInfo.InvariantCulture);
            }
            return decValue;

        }
        #endregion
        #region public ToCurrentCulture
        public static string ToCurrentCulture(decimal pData, int pPrecision)
        {
            return StrFunc.FmtDecimalToCurrentCulture(pData, pPrecision);
        }
        #endregion
        /// <summary>
        ///  Retourne la précision (nombre de chiffres utilisés pour exprimer la partie déciaml)
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        /// FI 20170228 [22883] Add (Méthode récupérée sur le net)
        public static int PrecisionOf(decimal d)
        {
            var text = d.ToString(CultureInfo.InvariantCulture).TrimEnd('0');
            var decpoint = text.IndexOf('.');
            if (decpoint < 0)
                return 0;
            return text.Length - decpoint - 1;
        }

        /// <summary>
        ///  Retourne la précision la plus fine parmi une liste de deciaml
        /// </summary>
        /// <param name="pDecvalues">Array de decimal</param>
        /// <returns></returns>
        /// FI 20170302 [22883] Add
        public static int PrecisionOf(decimal[] pDecvalues)
        {
            if (null == pDecvalues)
                throw new ArgumentNullException("pDecvalues is null");

            int scale = 0;
            foreach (Decimal item in pDecvalues)
            {
                int currentScale = DecFunc.PrecisionOf(item);
                if (currentScale > scale)
                    scale = currentScale;
            }
            return scale;
        }

        // EG 20180713 New
        public static decimal RoundedDecimal(decimal pValue, int pPrecision)
        {
            decimal step = (decimal)Math.Pow(10, pPrecision);
            decimal tmp = Math.Round(step * pValue);
            return tmp / step;
        }
    }
    #endregion class DecFunc
    #region public class IntFunc
    public sealed class IntFunc
    {

        /// <summary>
        /// Convertie un Nullable integer en integer
        /// </summary>
        /// <param name="pInteger"></param>
        /// <returns></returns>
        public static int ConvertToInt(Nullable<int> pInteger)
        {
            int ret = 0;
            if (IsFilledAndNoZero(pInteger))
                ret = (int)pInteger;
            return ret;

        }

        /// <summary>
        /// La string represente-t-elle une donnée integer non négative 
        /// </summary>
        /// <param name="pString"></param>
        /// <returns></returns>
        public static bool IsPositiveInteger(string pString)
        {
            Regex regex = new Regex(EFSRegex.RegularExpression(EFSRegex.TypeRegex.RegexPositiveInteger));
            return regex.IsMatch(pString);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pInteger"></param>
        /// <returns></returns>
        public static bool IsEmptyOrZero(Nullable<int> pInteger)
        {
            return ((null == pInteger) || (0 == pInteger));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pInteger"></param>
        /// <returns></returns>
        public static bool IsEmpty(Nullable<int> pInteger)
        {
            return ((null == pInteger));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pInteger"></param>
        /// <returns></returns>
        public static bool IsFilledAndNoZero(Nullable<int> pInteger)
        {
            return !IntFunc.IsEmptyOrZero(pInteger);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pInteger"></param>
        /// <returns></returns>
        public static bool IsFilled(Nullable<int> pInteger)
        {
            return !IntFunc.IsEmpty(pInteger);
        }

        /// <summary>
        /// Parse la donnée {pStr} en integer
        /// </summary>
        public static int IntValue(string pStr)
        {

            int ret = 0;
            //(pStr != Cst.HTMLSpace) specila Bidouille for automate du referentiel
            if (StrFunc.IsFilled(pStr) && (pStr != Cst.HTMLSpace))
                ret = int.Parse(pStr);
            //
            return ret;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pStr"></param>
        /// <returns></returns>
        public static long IntValue64(string pStr)
        {
            return IntValue64(pStr, Thread.CurrentThread.CurrentCulture);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pStr"></param>
        /// <param name="pCultureInfo"></param>
        /// <returns></returns>
        public static long IntValue64(string pStr, CultureInfo pCultureInfo)
        {
            //long ret = 0;
            //if (StrFunc.IsFilled(pStr))
            //{
            //    if (null != pCultureInfo.NumberFormat.NumberGroupSeparator && pCultureInfo.NumberFormat.NumberGroupSeparator.Length == 1)
            //    {
            //        char numberGroupSeparator = pCultureInfo.NumberFormat.NumberGroupSeparator.ToCharArray()[0];
            //        if (char.IsWhiteSpace(numberGroupSeparator))
            //            pStr = pStr.Replace(Cst.Space, string.Empty);
            //    }
            //    pStr = pStr.Replace(pCultureInfo.NumberFormat.NumberGroupSeparator, string.Empty);
            //    ret = long.Parse(pStr, pCultureInfo);
            //}
            //return ret;
            return Convert.ToInt64(IntValueString(pStr, pCultureInfo));

        }

        /// <summary>
        /// Parse la donnée {pStr} en décimal à partir des spécifications de la culture courante, puis convertie le résultat en  integer
        /// </summary>
        public static int IntValue2(string pStr)
        {
            return IntValue2(pStr, Thread.CurrentThread.CurrentCulture);
        }
        /// <summary>
        /// Parse la donnée {pStr} en décimal à partir des spécifications de la culture {pCultureInfo}, puis convertie le résultat en  integer
        /// </summary>
        /// <param name="pStr"></param>
        /// <param name="pCultureInfo"></param>
        /// <returns></returns>
        public static int IntValue2(string pStr, CultureInfo pCultureInfo)
        {
            //int ret = 0;
            //if (StrFunc.IsFilled(pStr) && (pStr != Cst.HTMLSpace))
            //{
            //    //Suppression du séparateur de millier
            //    NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            //    nfi.NumberGroupSeparator = string.Empty;
            //    nfi.NumberDecimalDigits = 0;
            //    ret = Convert.ToInt32(Decimal.Parse(pStr, pCultureInfo).ToString("n", nfi));
            //}
            //return ret;
            return Convert.ToInt32(IntValueString(pStr, pCultureInfo));
        }

        private static string IntValueString(string pStr, CultureInfo pCultureInfo)
        {
            string ret = "0";
            if (StrFunc.IsFilled(pStr) && (pStr != Cst.HTMLSpace))
            {
                //Suppression du séparateur de millier
                NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
                nfi.NumberGroupSeparator = string.Empty;
                nfi.NumberDecimalDigits = 0;
                ret = Decimal.Parse(pStr, pCultureInfo).ToString("n", nfi);
            }
            return ret;
        }

        /// <summary>
        /// Retourne true si la string représente un integer 
        /// </summary>
        /// <param name="pStr"></param>
        /// <returns></returns>
        public static bool IsInt64(string pStr)
        {
            bool ret = true;
            try { IntFunc.IntValue64(pStr); }
            catch { ret = false; }
            return ret;

        }

        /// <summary>
        ///  Retourne le représentation d'un integer en bits
        ///  Ex 5 donne 11
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        /// FI 20130411
        public static string ConvertToBits(int n)
        {
            string ret = string.Empty;
            int remainder;
            while (n > 0)
            {
                remainder = n % 2;
                n /= 2;
                ret = remainder.ToString() + ret;
            }
            return ret;
        }

        /// <summary>
        ///  Retourne le représentation d'un integer en bits
        ///  Ex 5 donne 0000000000000011
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        /// FI 20130411
        public static string ConvertTo16Bits(int n)
        {
            if (n > Math.Pow(2, 16) - 1)
                throw new ArgumentException("parameter is too large. Max value is 65535");

            string ret = ConvertToBits(n);
            ret = ret.PadLeft(16, '0');

            return ret;
        }

        /// <summary>
        /// Retourne la valeur base 10(integer) d'une représentation binaire
        /// </summary>
        /// <param name="binaryNumber">Représentation binaire</param>
        /// <returns></returns>
        /// FI 20130411
        public static int BitsToInt(string binaryNumber)
        {
            int multiplier = 1;
            int ret = 0;

            for (int i = binaryNumber.Length - 1; i >= 0; i--)
            {
                int t = Convert.ToInt16(binaryNumber[i].ToString());
                ret += (t * multiplier);
                multiplier *= 2;
            }
            return ret;
        }

        /// <summary>
        /// Fonction pour convertir une représentation base64 en long
        /// </summary>
        /// <param name="base64String">8 ou 12 caractères</param>
        /// <returns></returns>
        /// FI/AL 20240229 [WI860] Add Method
        public static long ConvertFromBase64String(string base64String)
        {
            List<byte> bytes = Convert.FromBase64String(base64String).ToList();
            bytes.Reverse(); //little endian
            StringBuilder bits = new StringBuilder();
            foreach (var b in bytes)
                bits.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
            long num = Convert.ToInt64(bits.ToString(), 2);
            return num;
        }

        /// <summary>
        /// Fonction pour convertir un nombre base10 (long) en Base64
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        /// FI 20240229 [WI860]  Add Method
        public static string ConvertToBase64String(long number)
        {
            // Convertir le nombre en une représentation binaire (en tant que tableau de bytes)
            byte[] bytes = BitConverter.GetBytes(number);
            // Convertir les bytes en une chaîne Base64
            string ret = Convert.ToBase64String(bytes);
            return ret;
        }

#if DEBUG
        /// <summary>
        ///   vérification du décodage depuis la gaweway BCS (voir ticket 26656)
        /// </summary>
        /// FI 20240229 [WI860]  Add Method
        public static void TestConvertFromBase64StringBCS()
        {
            string strValue;
            long longValue;

            // ConvertFromBase64String
            strValue = "N00BAQAA";
            longValue = ConvertFromBase64String(strValue);
            Debug.Assert(longValue == 16862519);


            strValue = "N00BYAAA";
            longValue = ConvertFromBase64String(strValue);
            Debug.Assert(longValue == 1610698039);

            strValue = "N00B+QEA";
            longValue = ConvertFromBase64String(strValue);
            Debug.Assert(longValue == 8472579383);

            strValue = "N00BCgAA";
            longValue = ConvertFromBase64String(strValue);
            Debug.Assert(longValue == 167857463);

        }

#endif
    }
    #endregion class intFunc

    #region public class DtFunc
    /// <summary>
    /// 
    /// </summary>
    public partial class DtFunc
    {
        #region FourDigitReading
        /// <summary>
        /// Interpréation de 4 digits
        /// </summary>
        public enum FourDigitReadingEnum
        {
            /// <summary>
            /// 4 digit représente une année (YYYY)
            /// </summary>
            FourDigitHasYYYY,
            /// <summary>
            /// 4 digit représente le jour et le mois (DDMM)
            /// </summary>
            FourDigitHasDDMM
        }
        #endregion

        #region Constantes
        public const string EOY = "EOY";
        public const string EOM = "EOM";
        /// <summary>
        /// TODAY
        /// </summary>
        public const string TODAY = "TODAY";
        /// <summary>
        /// OPEN
        /// </summary>
        /// EG 20140702 New
        public const string OPEN = "OPEN";
        /// <summary>
        /// NOW
        /// </summary>
        public const string NOW = "NOW";
        /// <summary>
        /// BUSINESS
        /// </summary>
        public const string BUSINESS = "BUSINESS";
        /// <summary>
        /// "s" (Equivalent à yyyy-MM-ddTHH:mm:ss)
        /// <para>Modèle de date ISO 8601(yyyy-MM-ddTHH:mm:ss)</para>
        /// </summary>
        public const string FmtISODateTime = "s";
        /// <summary>
        /// "yyyy-MM-ddTHH:mm:ss"
        /// <para>Modèle de date ISO 8601(yyyy-MM-ddTHH:mm:ss)</para>
        /// </summary>
        public const string FmtISODateTime2 = "yyyy-MM-ddTHH:mm:ss";

        /// <summary>
        /// "yyyy-MM-ddTHH:mm:ssZ" (Foramt d'une date UTC)
        /// <para>Modèle de date ISO 8601(yyyy-MM-ddTHH:mm:ssZ)</para>
        /// </summary>
        /// FI 20200819 [XXXXX] Add
        public const string FmtTZISODateTime2 = "yyyy-MM-ddTHH:mm:ssZ";

        /// <summary>
        /// "yyyy-MM-ddTHH:mm:ss.ffffffZ"
        /// <para>Modèle de date ISO 8601</para>
        /// </summary>
        /// EG 20170918 [23342] New
        /// EG 20170926 [22374] Rename
        public const string FmtTZISOLongDateTime = "yyyy-MM-ddTHH:mm:ss.ffffffZ";

        /// <summary>
        /// "yyyy-MM-ddTHH:mm:ss:ffffff"
        /// <para>Modèle de date ISO 8601(yyyy-MM-ddTHH:mm:ss.ffffff)</para>
        /// </summary>
        public const string FmtISOLongDateTime = "yyyy-MM-ddTHH:mm:ss.ffffff";
        /// <summary>
        /// "yyyy-MM-dd"
        /// </summary>
        public const string FmtISODate = "yyyy-MM-dd";
        /// <summary>
        /// "HH:mm:ss"
        /// </summary>
        public const string FmtISOTime = "HH:mm:ss";
        /// <summary>
        /// "HH:mm"
        /// </summary>
        public const string FmtISOShortTime = "HH:mm";
        /// <summary>
        /// Format yyyyMMdd
        /// </summary>
        public const string FmtDateyyyyMMdd = "yyyyMMdd";
        /// <summary>
        /// Format yyMMdd
        /// </summary>
        public const string FmtDateyyMMdd = "yyMMdd";
        /// <summary>
        /// Modèle d'heure courte (fonction de la culture du thread) 
        /// <para>Sans seconde</para
        /// </summary>
        public const string FmtShortTime = "t";
        /// <summary>
        /// Modèle d'heure Long (fonction de la culture du thread)
        /// <para>Avec secondes</para>
        /// </summary>
        public const string FmtLongTime = "T";
        /// <summary>
        /// Modèle date courte (fonction de la culture du thread)
        /// <para>Ex : 2009-06-15T13:45:30 -> 15/06/2009 (fr-FR)</para>
        /// </summary>
        public const string FmtShortDate = "d";
        /// <summary>
        /// Modèle Date/Heure (heure courte) (fonction de la culture du thread)
        /// <para>sans seconde</para>
        /// <para>Ex : 2009-06-15T13:45:30 -> 15/06/2009 13:45 (es-ES)</para>
        /// </summary>
        public const string FmtDateTime = "g";
        /// <summary>
        /// Modèle Date/Heure (heure longue) (fonction de la culture du thread)
        /// <para>avec secondes</para>
        /// <para>Ex: 2009-06-15T13:45:30 -> 15/06/2009 13:45:30 (es-ES)</para>
        /// </summary>
        public const string FmtDateLongTime = "G";

        #endregion Constante

        #region Members
        private FourDigitReadingEnum _fourDigitReading;
        #endregion

        #region accessor
        /// <summary>
        /// Obtient ou définit l'interprétation de 4 digit lors des convertions de String en Date
        /// <para>FourDigitHasDDMM => 4 digits sont interprétés comme un jour, mois [DEFAULT]</para>
        /// <para>FourDigitHasYYYY => 4 digits sont interprétés comme une année</para>
        /// </summary>
        public FourDigitReadingEnum FourDigitReading
        {
            get { return _fourDigitReading; }
            set { _fourDigitReading = value; }
        }
        /// <summary>
        /// Obtient le séparateur de date de la culture courante
        /// </summary>
        public static string CurrentCultureDateSeparator
        {
            get { return Thread.CurrentThread.CurrentUICulture.DateTimeFormat.DateSeparator; }
        }
        // EG 20170918 [23342] New
        public static DateTimeFormatInfo DateTimeOffsetPattern
        {
            get
            {
                DateTimeFormatInfo dfi = CultureInfo.CurrentCulture.DateTimeFormat.Clone() as DateTimeFormatInfo;
                dfi.LongTimePattern = System.Text.RegularExpressions.Regex.Replace(dfi.LongTimePattern, "(:ss|:s)", "$1.ffffff zzz");
                return dfi;
            }
        }
        #endregion

        #region constructor
        public DtFunc()
        {
            _fourDigitReading = FourDigitReadingEnum.FourDigitHasDDMM;
        }
        #endregion constructor

        #region Method
        /// <summary>
        /// Ajoute le suffixe UTC ("Z") pour une chaine datetime ISO 8601
        /// </summary>
        /// <param name="pValue"></param>
        /// <returns></returns>
        /// EG 20171025 [23509] New
        public static string AddEndUTCMarker(string pValue)
        {
            string ret = pValue;
            Regex regex = new Regex(@"([zZ]|([\+-])([01]\d|2[0-3]):?([0-5]\d)?)$");
            if (false == regex.IsMatch(ret) && StrFunc.IsFilled(ret))
                ret += "Z";
            return ret;
        }
        /// <summary>
        /// Retourne true si le format en entré est de type Invariant
        /// </summary>
        /// <param name="pFmt"></param>
        /// <returns></returns>
        /// FI 20171106 [XXXXX] Add Modify
        public static bool IsFormatInvariantCulture(string pFmt)
        {
            bool ret = FmtDateyyyyMMdd == pFmt;
            ret = ret || (FmtISODate == pFmt);
            ret = ret || (FmtISOTime == pFmt);
            // FI 20171106 [XXXXX] ajout de FmtISOLongDateTime
            ret = ret || (FmtISODateTime == pFmt) || (FmtISODateTime2 == pFmt) || (FmtISOLongDateTime == pFmt);
            //2008/03/07 FI Ticket 15857 voir EFS Thread id="44"
            //GLOP La ligne suivante est à supprimer lorsque BIM aura installé la nouvelle version de l'import MTM
            //Les trnaformation font référence au format  yyyy-MM-ddThh:mm:ss à la place de  yyyy-MM-ddTHH:mm:ss
            ret = ret || ("yyyy-MM-ddThh:mm:ss" == pFmt);
            return ret;
        }

        /// <summary>
        ///Retourne true si la date est null ou égale à DateTime.MinValue
        /// </summary>
        /// <param name="pDateTime"></param>
        /// <returns></returns>
        public static bool IsDateTimeEmpty(DateTime pDateTime)
        {
            return ((Convert.ToDateTime(null) == pDateTime) || pDateTime.Equals(DateTime.MinValue));
        }

        /// <summary>
        ///Retourne true si la date n'est pas vide (voir IsDateTimeEmpty)
        /// </summary>
        /// <param name="pDateTime"></param>
        /// <returns></returns>
        public static bool IsDateTimeFilled(DateTime pDateTime)
        {
            return (!IsDateTimeEmpty(pDateTime));
        }

        /// <summary>
        /// Ajoute des heures, minutes, secondes à une une date
        /// </summary>
        /// <param name="pDate">date initiale</param>
        /// <param name="pTime">date qui contient les heures, minutes, secondes</param>
        /// <returns></returns>
        public static DateTime AddTimeToDate(DateTime pDate, DateTime pTime)
        {
            DateTime dt = pDate;
            if (IsDateTimeFilled(pDate) && IsDateTimeFilled(pTime))
            {
                int hh = pTime.Hour;
                int mm = pTime.Minute;
                int ss = pTime.Second;
                TimeSpan hhmmss = new TimeSpan(hh, mm, ss);
                dt = dt.Add(hhmmss);
            }
            return dt;
        }

        /// <summary>
        /// Retourne le 1er jour de la semaine (numéro <paramref name="pWeekNumber"/> d'un mois qui comprendrait 31 jour et dont le 1er jour serait un lundi
        /// <para>1 pour la 1ère semaine, 8 pour la 2ème semaine, 15 pour la 3ème semaine, 22 pour la 4ème semaine, 29 pour la 5ème semaine... </para>
        /// </summary>
        /// <param name="pWeekNumber"></param>
        /// <returns></returns>
        public static int GetDayOfWeek(int pWeekNumber)
        {
            return (7 * (pWeekNumber - 1) + 1);
        }

        #region  DateTime => String
        /// <summary>
        /// Retourne la Date en string selon le format pOutputFmt
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pOutputFmt"></param>
        /// <returns></returns>
        public static string DateTimeToString(DateTime pDate, string pOutputFmt)
        {
            string ret;
            if (IsFormatInvariantCulture(pOutputFmt))
                ret = DateTimeToString(pDate, pOutputFmt, CultureInfo.InvariantCulture);
            else
                ret = DateTimeToString(pDate, pOutputFmt, null); // null == CurrentCulture
            return ret;
        }

        /// <summary>
        /// Retourne la Date en string selon le format associé une culture
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pOutputFmt">Représente le format de sortie</param>
        /// <param name="pCultureInfo">Représente la culture, null est equivalent à CurrentCulture</param>
        /// <returns></returns>
        public static string DateTimeToString(DateTime pDate, string pOutputFmt, CultureInfo pCultureInfo)
        {
            return pDate.ToString(pOutputFmt, pCultureInfo);
        }

        /// <summary>
        /// Retourne la Date au format ISO yyyy-MM-ddTHH:mm:ss  
        /// </summary>
        /// <param name="pDate"></param>
        /// <returns></returns>
        public static string DateTimeToStringISO(DateTime pDate)
        {
            return DateTimeToString(pDate, FmtISODateTime, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Retourne la Date au format ISO yyyy-MM-dd
        /// </summary>
        /// <param name="pDate"></param>
        /// <returns></returns>
        public static string DateTimeToStringDateISO(DateTime pDate)
        {
            return DateTimeToString(pDate, FmtISODate, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Retourne la Date au format ISO yyyyMMdd
        /// </summary>
        /// <param name="pDate"></param>
        /// <returns></returns>
        public static string DateTimeToStringyyyyMMdd(DateTime pDate)
        {
            return DateTimeToString(pDate, FmtDateyyyyMMdd, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Retourne la Date au format ISO yyMMdd
        /// </summary>
        /// <param name="pDate"></param>
        /// <returns></returns>
        public static string DateTimeToStringyyMMdd(DateTime pDate)
        {
            return DateTimeToString(pDate, FmtDateyyMMdd, CultureInfo.InvariantCulture);
        }
        #endregion

        #region  string => Date
        /// <summary>
        /// Convertit la représentation sous forme de chaîne spécifiée en System.DateTime 
        /// <para>Les mots clefs (ex TODAY) sont interprétés</para>
        /// </summary>
        /// <param name="pDate"></param>
        /// <returns></returns>
        public DateTime StringToDateTime(string pDate)
        {
            return StringToDateTime(pDate, null, null, true);
        }

        /// <summary>
        /// Convertit la représentation sous forme de chaîne spécifiée en System.DateTime 
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pIsCalcDateFunction">si true interprétation des mots clefs</param>
        /// <returns></returns>
        public DateTime StringToDateTime(string pDate, bool pIsCalcDateFunction)
        {
            return StringToDateTime(pDate, null, null, pIsCalcDateFunction);
        }

        /// <summary>
        /// Convertit la représentation sous forme de chaîne spécifiée en System.DateTime 
        /// <para>Les mots clefs (ex TODAY) sont interprétés</para>
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pInputFmt">Représente le format en entrée</param>
        /// <returns></returns>
        public DateTime StringToDateTime(string pDate, string pInputFmt)
        {
            return StringToDateTime(pDate, pInputFmt, null, true);
        }

        /// <summary>
        /// Convertit la représentation sous forme de chaîne spécifiée en System.DateTime 
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pInputFmt">Représente le format en entrée</param>
        /// <param name="pIsCalcDateFunction">si true interprétation des mots clefs (ex TODAY)</param>
        /// <returns></returns>
        public DateTime StringToDateTime(string pDateValue, string pInputFmt, bool pIsCalcDateFunction)
        {
            return StringToDateTime(pDateValue, pInputFmt, null, pIsCalcDateFunction);
        }

        /// <summary>
        /// Convertit la représentation sous forme de chaîne spécifiée en System.DateTime 
        /// <para>Les mots clefs (ex TODAY) sont interprétés</para>
        /// </summary>
        /// <param name="pDateValue"></param>
        /// <param name="pInputFmt">Format en entrée</param>
        /// <param name="pCulture">Culture associée au format</param>
        /// <returns></returns>
        public DateTime StringToDateTime(string pDateValue, string pInputFmt, CultureInfo pCulture)
        {
            return StringToDateTime(pDateValue, pInputFmt, pCulture, true);
        }

        /// <summary>
        /// Convertit la représentation sous forme de chaîne spécifiée en System.DateTime 
        /// <para>précision du format, de la culture associé au format, avec ou sans interprétation ou non des mots clefs [ex TODAY] etc...)</para>
        /// <para>Retourne DateTime.MinValue si conversion impossible</para>
        /// </summary>
        /// <param name="pDateValue"></param>
        /// <param name="pInputFmt">Format en entrée (valeur null possible)</param>
        /// <param name="pCulture">Culture associée au format</param>
        /// <param name="pIsCalcDateFunction"></param>
        /// <returns></returns>
        /// FI 20110713 REFACTORING =>Si TODAY,NOW,ou BUSINESS alors Spheres® ne tente pas de parse (cela évite les exceptions inutiles et couteuses)
        /// 
        public DateTime StringToDateTime(string pDateValue, string pInputFmt, CultureInfo pCulture, bool pIsCalcDateFunction)
        {
            // 20090302 RD 
            // REFACTORING à faire dans une prochaine grosse release (ex. Migration sou sVS2008)
            // - Ajouter un constructeur pour cette classe
            // - Ajouter un membre IsCalcDateFunction 
            // - Supprimer les surcharges avec pIsCalcDateFunction
            // 

            DateTime dt = DateTime.MinValue;
            bool isToCalc = false;

            if (StrFunc.IsFilled(pDateValue))
            {
                //20121106 [] use methode  IsParsableValue
                bool isToParse = IsParsableValue(pDateValue);
                isToCalc = (false == isToParse);
                if (isToParse)
                {
                    try
                    {
                        dt = ParseDate(pDateValue, pInputFmt, pCulture);
                    }
                    catch
                    {
                        //Si plantage Spheres® tente une interprétation
                        isToCalc = true;
                    }
                }
            }

            if ((DateTime.MinValue == dt) && isToCalc && pIsCalcDateFunction)
            {
                if (IsDateFunction(pDateValue))
                    dt = GetDateFunction(pDateValue);
                else if (IsDateTimeNow(pDateValue))
                    dt = DateTime.Now;
                else
                    dt = DateTime.MinValue;
            }
            return dt;
        }

        /// <summary>
        /// Convertit la représentation sous forme de chaîne spécifiée d'une date 
        /// en System.DateTime équivalent à l'aide des informations de format
        /// propres à la culture et au format spécifiés. Le format de la chaîne doit
        /// correspondre exactement au format spécifié.
        /// <para>Cet méthode ne prend pas en considération les mots clefs (Ex TODAY)</para>
        /// </summary>
        /// <param name="pDateValue"></param>
        /// <param name="pInputFmt">Format en entrée (valeur null possible)</param>
        /// <param name="pCulture">Culture associée au format(valeur null si Format ISO)</param>
        /// <returns></returns>
        public static DateTime ParseDate(string pDateValue, string pInputFmt, CultureInfo pCulture)
        {
            DateTime dt;
            if (StrFunc.IsFilled(pInputFmt))
            {
                if (IsFormatInvariantCulture(pInputFmt))
                    dt = DateTime.ParseExact(pDateValue, pInputFmt, CultureInfo.InvariantCulture);
                else
                    dt = DateTime.ParseExact(pDateValue, pInputFmt, pCulture);
            }
            else
                dt = DateTime.Parse(pDateValue);
            return dt;
        }

        /// <summary>
        /// Convertit la représentation sous forme de chaîne spécifiée d'une date 
        /// en System.DateTime équivalent à l'aide des informations de format
        /// propres à la culture et au format spécifiés. Le format de la chaîne doit
        /// correspondre exactement à l'un des formats spécifiés.
        /// <para>Cet méthode ne prend pas en considération les mots clefs (Ex TODAY)</para>
        /// </summary>
        /// <param name="pDateValue"></param>
        /// <param name="pInputFmt">Liste de Formats en entrée (valeur null possible)</param>
        /// <param name="pCulture">Culture associée aux formats (valeur null si Format ISO)</param>
        /// <returns></returns>
        /// RD 20180103 [23694] Add 
        public static DateTime ParseDate(string pDateValue, string[] pInputFmt, CultureInfo pCulture)
        {
            string errMsg = "String was not recognized as a valid DateTime." + Cst.CrLf + "Details:" + Cst.CrLf;

            CultureInfo culture = pCulture;
            if (pCulture == null)
                culture = CultureInfo.InvariantCulture;

            DateTime dt;
            if (ArrFunc.IsFilled(pInputFmt))
            {
                if (false == DateTime.TryParseExact(pDateValue, pInputFmt, culture, DateTimeStyles.None, out dt))
                    throw new Exception(string.Format(errMsg + "- Value: {0}" + Cst.CrLf + "- Format: {1}" + Cst.CrLf + "- Culture: {2}" + Cst.CrLf,
                        pDateValue, ArrFunc.GetStringList(pInputFmt), culture.EnglishName));
            }
            else
            {
                if (false == DateTime.TryParse(pDateValue, culture, DateTimeStyles.None, out dt))
                    throw new Exception(string.Format(errMsg + "- Value: {0}" + Cst.CrLf + "- Culture: {1}" + Cst.CrLf,
                        pDateValue, culture.EnglishName));
            }
            return dt;
        }

        /// <summary>
        /// yyyyMMdd to DateTime
        /// </summary>
        /// <param name="pDate"></param>
        /// <returns></returns>
        public DateTime StringyyyyMMddToDateTime(string pDate)
        {
            return StringToDateTime(pDate, FmtDateyyyyMMdd);
        }

        /// <summary>
        /// yyyy-MM-ddTHH:mm:ss to DateTime
        /// </summary>
        /// <param name="pDateTime"></param>
        /// <returns></returns>
        public DateTime StringDateTimeISOToDateTime(string pDateTime)
        {
            return StringToDateTime(pDateTime, FmtISODateTime);
        }

        /// <summary>
        /// yyyy-MM-dd to DateTime
        /// </summary>
        /// <param name="pDateTime"></param>
        /// <returns></returns>
        public DateTime StringDateISOToDateTime(string pDateTime)
        {
            return StringToDateTime(pDateTime, FmtISODate);
        }

        /// <summary>
        /// HH:mm:ss to DateTime
        /// </summary>
        /// <param name="pDateTime"></param>
        /// <returns></returns>
        public DateTime StringTimeISOToDateTime(string pDateTime)
        {
            return StringToDateTime(pDateTime, FmtISOTime);
        }
        #endregion

        #region  string => Time (DateTime)
        /// <summary>
        /// Retourne une string qui représente une time en DateTime
        /// </summary>
        /// <param name="pTime"></param>
        /// <returns></returns>
        public DateTime StringToTime(string pTime)
        {
            return StringToTime(pTime, true);
        }

        /// <summary>
        /// Retourne une string qui représente une time en DateTime (avec interprétation ou non des mots clefs [ex NOW] etc...)
        /// </summary>
        public DateTime StringToTime(string pTime, bool pIsCalcDateFunction)
        {
            // 20090302 RD 
            // Voir le commentaire de StringToDateTime
            DateTime dt = DateTime.MinValue;
            if (StrFunc.IsFilled(pTime))
            {
                try
                {
                    dt = DateTime.Parse(pTime);
                }
                catch
                {
                    if (pIsCalcDateFunction)
                    {
                        if (IsDateFunction(pTime))
                            dt = GetTimeFunction(pTime);
                        else if (IsDateTimeNow(pTime))
                            dt = DateTime.Now;
                        else
                            dt = DateTime.MinValue;
                    }
                    else
                        dt = DateTime.MinValue;
                }
            }
            return dt;
        }
        #endregion

        #region string => string    manipulation de string qui represente des dates

        /// <summary>
        /// Convertie en date la donnée en entrée puis retourne le résultat au format FmtShortDate (spécifique à la culture courante)
        /// </summary>
        /// <param name="pDateValue"></param>
        /// <returns></returns>
        public string GetDateString(string pDateValue)
        {
            return GetDateTimeString(pDateValue, FmtShortDate);
        }

        /// <summary>
        /// Convertie en date la donnée en entrée puis retourne le résultat au format FmtShortTime (spécifique à la culture courante)
        /// </summary>
        /// <param name="pTimeValue"></param>
        /// <returns></returns>
        public string GetShortTimeString(string pTimeValue)
        {
            return GetDateTimeString(pTimeValue, FmtShortTime);
        }

        /// <summary>
        /// Convertie en date la donnée en entrée puis retourne le résultat au format FmtLongTime (spécifique à la culture courante)
        /// </summary>
        /// <param name="pTimeValue"></param>
        /// <returns></returns>        
        public string GetLongTimeString(string pTimeValue)
        {
            return GetTimeString(pTimeValue, FmtLongTime);
        }

        /// <summary>
        /// Convertie en date la donnée en entrée puis retourne le résultat au format FmtDateTime (spécifique à la culture courante)
        /// </summary>
        /// <param name="pDate"></param>
        /// <returns></returns>
        public string GetDateTimeString(string pDate)
        {
            return GetDateTimeString(pDate, FmtDateTime);
        }

        /// <summary>
        /// Convertie en date la donnée en entrée puis retourne le résultat au format désiré
        /// </summary>
        public string GetDateTimeString(string pDate, string pOutputFmt)
        {
            string ret = string.Empty;
            //
            DateTime dt = StringToDateTime(pDate);
            //
            if (IsDateTimeFilled(dt))
                ret = DateTimeToString(dt, pOutputFmt);
            //
            return ret;
        }

        /// <summary>
        /// Convertie en time la donnée en entrée puis retourne le résultat au format désiré
        /// <para>Retourne string.Empty si la donnée en entrée ne représente pas une date</para>
        /// <param name="pTime">donnée string en entrée</param>
        /// <param name="pOutputFmt">Format désiré</param>
        /// </summary>
        public string GetTimeString(string pTime, string pOutputFmt)
        {
            string ret = string.Empty;
            DateTime dt = StringToTime(pTime);
            if (IsDateTimeFilled(dt))
                ret = DateTimeToString(dt, pOutputFmt);
            return ret;
        }

        /// <summary>
        /// Convertie en date la donnée en entrée au format yyyyMMdd puis retourne le résultat au format FmtShortDate (spécifique à la culture courante)
        /// </summary>
        /// <param name="pDateValue"></param>
        /// <returns></returns>
        public string GetDateStringFromFmtyyyyMMdd(string pDateValue)
        {
            return GetDateStringFromFmtyyyyMMdd(pDateValue, FmtShortDate);
        }

        /// <summary>
        /// Convertie en date la donnée en entrée au format yyyyMMdd puis retourne le résultat au format spécifié
        /// </summary>
        public string GetDateStringFromFmtyyyyMMdd(string pDateValue, string pOutPutFmt)
        {
            DateTime dt = StringyyyyMMddToDateTime(pDateValue);
            return DateTimeToString(dt, pOutPutFmt);
        }

        /// <summary>
        /// Convertie en date la donnée en entrée au format ISO puis retourne le résultat au format FmtDateTime (spécifique à la culture courante)
        /// </summary>
        public string GetDateTimeStringFromIso(string pDateValue)
        {
            return GetDateTimeStringFromIso(pDateValue, FmtDateTime);
        }

        /// <summary>
        /// Convertie en date la donnée en entrée au format ISO puis retourne le résultat au format spécifié
        /// </summary>
        public string GetDateTimeStringFromIso(string pDateValue, string pOutPutFmt)
        {
            DateTime dt = StringDateTimeISOToDateTime(pDateValue);
            return DateTimeToString(dt, pOutPutFmt);
        }

        /// <summary>
        /// Convertie {pDateValue} en DateTime et retourne le résulat au format fmtShortTime
        /// </summary>
        /// <param name="pDateValue"></param>
        /// <returns></returns>
        public string GetTimeStringFromIso(string pDateValue)
        {
            return GetTimeStringFromIso(pDateValue, FmtShortTime);
        }

        /// <summary>
        /// Convertie {pDateValue} en DateTime et retourne le résulat au format {pOutPutFmt}
        /// </summary>
        /// <param name="pDateValue"></param>
        /// <param name="pOutPutFmt"></param>
        /// <returns></returns>
        public string GetTimeStringFromIso(string pDateValue, string pOutPutFmt)
        {
            DateTime dt = StringDateTimeISOToDateTime(pDateValue);
            return DateTimeToString(dt, pOutPutFmt);
        }
        #endregion


        /// <summary>
        /// Retourne la date de référence par interprétation de mots clefs ou d'une date partiellement renseignée  
        /// </summary>
        /// <param name="pDateValue">String à interpréter</param>
        /// <returns></returns>
        /// EG 20140702 New Add OPEN (=MAXDATEVALUE)
        protected virtual DateTime GetDateReference(string pDateValue)
        {
            DateTime dtRef = DateTime.MinValue;
            Regex regex;
            #region OPEN
            regex = new Regex(@"^(" + OPEN + @"|O)", RegexOptions.IgnoreCase);
            bool isOk = regex.IsMatch(pDateValue.ToUpper());
            if (isOk)
            {
                dtRef = DateTime.MaxValue;
            }
            #endregion
            #region IsTODAY
            if (!isOk)
            {
                regex = new Regex(@"^(" + TODAY + @"|D)", RegexOptions.IgnoreCase);
                isOk = regex.IsMatch(pDateValue.ToUpper());
                if (!isOk)
                {
                    regex = new Regex(@"^[/+/-]\d+");//Ex +6 
                    isOk = regex.IsMatch(pDateValue);
                }

                if (isOk)
                    dtRef = DateTime.Today;
            }
            #endregion

            #region IsEOM
            if (!isOk)
            {
                regex = new Regex(@"^" + EOM, RegexOptions.IgnoreCase);//Dernier jour du mois 
                isOk = regex.IsMatch(pDateValue.ToUpper());
                if (isOk)
                {
                    dtRef = DateTime.Today.AddMonths(1);
                    dtRef = new DateTime(dtRef.Year, dtRef.Month, 1);
                    dtRef = dtRef.AddDays(-1);
                }
            }
            if (!isOk)
            {
                regex = new Regex(@"^-" + EOM, RegexOptions.IgnoreCase); //Dernier jour du mois précédent
                isOk = regex.IsMatch(pDateValue.ToUpper());
                if (isOk)
                {
                    dtRef = DateTime.Today;
                    dtRef = new DateTime(dtRef.Year, dtRef.Month, 1);
                    dtRef = dtRef.AddDays(-1);
                }
            }
            #endregion

            #region  IsEOY
            if (!isOk)
            {
                regex = new Regex(@"^" + EOY, RegexOptions.IgnoreCase);//Dernier jour de l'année
                isOk = regex.IsMatch(pDateValue.ToUpper());
                if (isOk)
                {
                    dtRef = DateTime.Today.AddYears(1);
                    dtRef = new DateTime(dtRef.Year, 1, 1);
                    dtRef = dtRef.AddDays(-1);
                }
            }
            if (!isOk)
            {
                regex = new Regex(@"^-" + EOY, RegexOptions.IgnoreCase); //Dernier jour du l'année précédent
                isOk = regex.IsMatch(pDateValue.ToUpper());
                if (isOk)
                {
                    dtRef = DateTime.Today;
                    dtRef = new DateTime(dtRef.Year, 1, 1);
                    dtRef = dtRef.AddDays(-1);
                }
            }
            #endregion

            #region Format autres
            if (!isOk)
            {
                //Default Value
                int day = DateTime.Now.Day;
                int month = DateTime.Now.Month;
                int year = DateTime.Now.Year;

                try
                {

                    string result;
                    if (!isOk)
                    {
                        regex = new Regex(@"^\d{8}");//MMDDYYYY (Anglais EU) DDMMYYYY (IT,FR,....)
                        isOk = regex.IsMatch(pDateValue);
                        if (isOk)
                        {
                            //
                            result = regex.Match(pDateValue).Value;
                            //
                            day = int.Parse(result.Substring(GetDayPosition(), 2));
                            month = int.Parse(result.Substring(GetMonthPosition(), 2));
                            year = int.Parse(result.Substring(4, 4));
                        }
                    }
                    if (!isOk)
                    {
                        regex = new Regex(@"^\d{6}");//MMDDYY ou DDMMYY 
                        isOk = regex.IsMatch(pDateValue);
                        if (isOk)
                        {
                            //
                            result = regex.Match(pDateValue).Value;
                            //
                            day = int.Parse(result.Substring(GetDayPosition(), 2));
                            month = int.Parse(result.Substring(GetMonthPosition(), 2));
                            year = int.Parse("20" + result.Substring(4, 2));
                        }
                    }
                    if (!isOk)
                    {
                        regex = new Regex(@"^\d{4}");//MMDD ou DDMM  ou YYYY
                        isOk = regex.IsMatch(pDateValue);
                        if (isOk)
                        {
                            result = regex.Match(pDateValue).Value;
                            if (_fourDigitReading == FourDigitReadingEnum.FourDigitHasDDMM)
                            {
                                day = int.Parse(result.Substring(GetDayPosition(), 2));
                                month = int.Parse(result.Substring(GetMonthPosition(), 2));
                            }
                            else if (_fourDigitReading == FourDigitReadingEnum.FourDigitHasYYYY)
                            {
                                year = int.Parse(result);
                            }
                        }
                    }
                    if (!isOk)
                    {
                        regex = new Regex(@"^\d{2}");//DD
                        isOk = regex.IsMatch(pDateValue);
                        if (isOk)
                        {
                            result = regex.Match(pDateValue).Value;
                            if (_fourDigitReading == FourDigitReadingEnum.FourDigitHasDDMM)
                            {
                                day = int.Parse(result.Substring(0, 2));
                            }
                            else
                            {
                                year = int.Parse(result.Substring(0, 2));
                            }
                        }
                    }
                }
                catch { isOk = false; }
                if (isOk)
                {
                    try
                    {
                        dtRef = new DateTime(year, month, day);
                    }
                    catch { dtRef = DateTime.MinValue; };
                }

            }
            #endregion YYMMDD
            return dtRef;
        }
        /// <summary>
        /// Interprète une expression String en date (voir le ticket 16079)
        /// </summary>
        /// <param name="pDateValue">Expression (ex TODAY-2M, ou -3)</param>
        /// <returns></returns>
        protected virtual DateTime GetDateFunction(string pDateValue)
        {
            string dateValue = pDateValue.ToUpper();
            //Recherche de la date de référence
            DateTime dtRef = GetDateReference(dateValue);
            //Lorsque la date de référence est déterminée, application d'un décalage
            if (DtFunc.IsDateTimeFilled(dtRef))
            {
                Regex regex = new Regex(@"[\-\+]\d+");
                if (regex.IsMatch(dateValue))
                {
                    string regExValue = regex.Match(dateValue).Value;
                    int number = int.Parse(regExValue);
                    //FI 20091114 [16760] les crochets sont supprimés car [EOY$] s'applique si le string se termine pae E ou O ou Y    
                    //if (new Regex(@"[EOY$]").IsMatch(dateValue))
                    if (new Regex(@"EOY$", RegexOptions.IgnoreCase).IsMatch(dateValue))
                    {
                        dtRef = DateTime.Today.AddYears(number + ((number <= 0) ? 1 : 0));
                        dtRef = new DateTime(dtRef.Year, 1, 1);
                        dtRef = dtRef.AddDays(-1);
                    }
                    //FI 20091114 [16760] les crochets sont supprimés car [EOM$] s'applique si le string se termine pae E ou O ou M    
                    //else if (new Regex(@"[EOM$]").IsMatch(dateValue))
                    else if (new Regex(@"EOM$", RegexOptions.IgnoreCase).IsMatch(dateValue))
                    {
                        dtRef = dtRef.AddMonths(number + ((number <= 0) ? 1 : 0));
                        dtRef = new DateTime(dtRef.Year, dtRef.Month, 1);
                        dtRef = dtRef.AddDays(-1);
                    }
                    //FI 20091114 [16760] suppression crochets
                    else if (new Regex(@"M$", RegexOptions.IgnoreCase).IsMatch(dateValue))
                        dtRef = dtRef.AddMonths(number);
                    //FI 20091114 [16760] suppression crochets
                    else if (new Regex(@"Y$", RegexOptions.IgnoreCase).IsMatch(dateValue))
                        dtRef = dtRef.AddYears(number);
                    else
                        dtRef = dtRef.AddDays(double.Parse(number.ToString()));
                }
            }
            return dtRef;
        }

        /// <summary>
        /// Retourne true lorsque la pDateValue respecte la regex RegexDateRelative
        /// <para>Signifie que l'expression {pDateValue} peut être convertie en date</para>
        /// </summary>
        /// <param name="pDateValue"></param>
        /// <returns></returns>
        protected virtual bool IsDateFunction(string pDateValue)
        {
            bool isOk = false;
            if (StrFunc.IsFilled(pDateValue))
                isOk = EFSRegex.IsMatch(pDateValue.ToUpper(), EFSRegex.TypeRegex.RegexDateRelative);
            return isOk;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDate"></param>
        /// <returns></returns>
        public bool IsDateTimeFunction(string pDate)
        {
            bool isOk = false;
            //
            if (StrFunc.IsFilled(pDate))
            {
                if (IsDateFunction(pDate) || IsDateTimeNow(pDate))
                    isOk = true;
            }
            return isOk;

        }

        /// <summary>
        /// Retourne le mois au format MMM 
        /// <para>The abbreviated name of the month</para>
        /// </summary>
        /// <param name="pMonth"></param>
        /// <returns></returns>
        public static string CurrentCultureFmtMonthMMM(int pMonth)
        {
            return (new DateTime(1965, pMonth, 25)).ToString("MMM");
        }

        /// <summary>
        /// Retourne le mois dont le format MMM est {pMonthMMM}
        /// <para>Retourne 0 si le format ne correspond à rien</para>
        /// </summary>
        /// <param name="pMonthMMM"></param>
        /// <returns></returns>
        public static int MonthFromCurrentCultureFmtMonthMMM(string pMonthMMM)
        {
            int ret = 0;
            for (int i = 1; i < 13; i++)
            {
                string currentcultFmtMonthMMM = CurrentCultureFmtMonthMMM(i);
                if (StrFunc.IsFilled(currentcultFmtMonthMMM))
                {
                    if ((currentcultFmtMonthMMM == pMonthMMM) ||
                        (StrFunc.FirstUpperCase(currentcultFmtMonthMMM) == pMonthMMM))
                    {
                        ret = i;
                        break;
                    }
                }
            }
            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTimeValue"></param>
        /// <returns></returns>
        private static DateTime GetTimeFunction(string pTimeValue)
        {
            DateTime dtRef = DateTime.MinValue;
            Regex regex;
            int hour = 0, minute = 0, second = 0;
            #region Format NOW
            bool isOk = IsDateTimeNow(pTimeValue);
            if (false == isOk)
            {
                regex = new Regex(@"^[/+/-]\d+");//Ex +6 
                isOk = regex.IsMatch(pTimeValue);
            }
            if (isOk)
                dtRef = DateTime.Today;
            #endregion Format TODAY
            #region Format HHMMSS
            if (false == isOk)
            {
                try
                {
                    // Only Hour & Minute & Second
                    regex = new Regex(@"^\d{6}");//HHMMSS
                    isOk = regex.IsMatch(pTimeValue);
                    string result;
                    if (isOk)
                    {
                        result = regex.Match(pTimeValue).Value;
                        hour = int.Parse(result.Substring(0, 2));
                        minute = int.Parse(result.Substring(2, 2));
                        second = int.Parse(result.Substring(4, 2));
                    }
                    // Only Hour & Minute
                    if (false == isOk)
                    {
                        regex = new Regex(@"^\d{4}"); //HHMM
                        isOk = regex.IsMatch(pTimeValue);
                        if (isOk)
                        {
                            result = regex.Match(pTimeValue).Value;
                            hour = int.Parse(result.Substring(0, 2));
                            minute = int.Parse(result.Substring(2, 2));
                        }
                    }
                    // Only Hour
                    if (false == isOk)
                    {
                        regex = new Regex(@"^\d{2}"); //HH
                        isOk = regex.IsMatch(pTimeValue);
                        if (isOk)
                        {
                            result = regex.Match(pTimeValue).Value;
                            hour = int.Parse(result.Substring(0, 2));
                        }
                    }
                }
                catch { isOk = false; }

                if (isOk)
                    dtRef = new DateTime(dtRef.Year, dtRef.Month, dtRef.Day, hour, minute, second);
            }
            //
            #endregion HHMMSS
            return dtRef;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDateValue"></param>
        /// <returns></returns>
        private static bool IsDateTimeNow(string pDateValue)
        {
            bool isOk = false;
            if (StrFunc.IsFilled(pDateValue))
            {
                string regularExpression = @"(^(" + NOW + @"|N)?$)";
                Regex regex = new Regex(regularExpression);
                isOk = regex.IsMatch(pDateValue.ToUpper());
            }
            return isOk;
        }

        /// <summary>
        /// Retourne la position de la donnée qui représente le mois
        /// </summary>
        /// <returns></returns>
        private static int GetMonthPosition()
        {
            return IsddMM() ? 2 : 0;
        }

        /// <summary>
        /// Retourne la position de la donnée qui représente le jour
        /// </summary>
        /// <returns></returns>
        private static int GetDayPosition()
        {
            return IsddMM() ? 0 : 2;
        }

        /// <summary>
        /// Retourne true si sur la CurrentCulture les jours sont saisis avant les mois (cas de la culture française)
        /// </summary>
        /// <returns></returns>
        private static bool IsddMM()
        {
            bool ret;
            try
            {
                // Anlais  (americain)=>M/d/yyyy
                // Français =>dd/MM/yyy
                ret = (Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern.Substring(0, 1) == "d");
            }
            catch { ret = true; }
            return ret;
        }

        /// <summary>
        /// <para>Retourne true si la donnée peut être parsée en une date, cela ne signigie que le parse fonctionnera</para>
        /// <para>Retourne false si la donnée ne peut être parsée en une date, cela signigie le parse générerait une exception systématique</para>
        /// </summary>
        /// <param name="pValue"></param>
        /// <returns></returns>
        /// FI 20121106 [] add 
        /// EG 20140702 New Add OPEN
        public static bool IsParsableValue(string pDateValue)
        {
            bool ret = true;
            if (StrFunc.IsFilled(pDateValue))
            {
                ret = (false == pDateValue.ToUpper().Contains(DtFunc.TODAY)) &&
                (false == pDateValue.ToUpper().Contains(DtFunc.NOW)) &&
                (false == pDateValue.ToUpper().Contains(DtFunc.OPEN)) &&
                (false == pDateValue.ToUpper().Contains(DtFunc.BUSINESS));
                // FI 20190327 [24603] Usage d'une regex
                ret = ret && (false == new Regex(@"^[-|+]\d+[D|M|Y]$").IsMatch(pDateValue));
            }
            return ret;
        }

        /// <summary>
        /// Retourne la date à partir d'une représentation binaire sur 16 bits
        /// <para>les 7 premiers bits représente l'année</para>
        /// <para>les 4 bits suivants représente le mois</para>
        /// <para>les 5 bits suivants représente le jour</para>
        /// <para>Cet stockage binaire est utilisé sur certains marché (Exemple OMX Nordic)</para>
        /// <para>0000001000100001 Représente alors le 1 janvier 1990</para>
        /// </summary>
        /// <param name="pBinaryDate">Représente une date sur 16 bits</param>
        /// FI 20130411
        public static DateTime StringDate16BitsToDate(string pBinaryDate)
        {
            if (StrFunc.IsEmpty(pBinaryDate))
                throw new ArgumentException("BinaryDate is empty");

            if (pBinaryDate.Length != 16)
                throw new ArgumentException("BinaryDate is not a 16 bits representation");

            char[] bitArray = pBinaryDate.ToCharArray();
            for (int i = 0; i < ArrFunc.Count(bitArray); i++)
            {
                if (bitArray[i] != '0' & bitArray[i] != '1')
                    throw new ArgumentException("BinaryDate is not a 16 bits representation");
            }

            string sYear = pBinaryDate.Substring(0, 7);
            string sMonth = pBinaryDate.Substring(7, 4);
            string sDay = pBinaryDate.Substring(11, 5);

            int year = IntFunc.BitsToInt(sYear) + 1989;
            int month = IntFunc.BitsToInt(sMonth);
            int day = IntFunc.BitsToInt(sDay);

            DateTime dt = new DateTime(year, month, day);
            return dt;
        }

        /// <summary>
        /// Retourne la date qui correspond à la valeur base 10 d'une représentation sur 16 bits
        /// <para>Exemple 545 retourne le 1er janvier 1990</para>
        /// <para>Cette représentation est utilisée sur certains marché (Exemple OMX Nordic)</para>
        /// <para>545 Représente alors le 1 janvier 1990</para>
        /// </summary>
        /// <param name="pDateBase10">Représente une date en base 10</param>
        /// <returns></returns>
        /// FI 20130411
        public static DateTime StringDec16BitsToDate(int pDateBase10)
        {
            string binaryPicture = IntFunc.ConvertTo16Bits(pDateBase10);
            DateTime dt = StringDate16BitsToDate(binaryPicture);
            return dt;
        }

        /// <summary>
        /// Initialise un DateTimeOffset à partir d'une date et d'un offset
        /// </summary>
        /// <param name="pDate">Date locale</param>
        /// <param name="pOffset">Représentation string de l'offset (ex "+02:00")</param>
        /// FI 20160624 [22286] Add
        public static DateTimeOffset ConvertToDateTimeOffset(DateTime pDate, string pOffset)
        {
            DateTimeOffset ret;
            if (StrFunc.IsFilled(pOffset))
            {
                string[] splitResult = pOffset.Split(':');

                if (ArrFunc.Count(splitResult) != 2)
                    throw new NotSupportedException(StrFunc.AppendFormat("offset: {0} is not a valid format", pOffset));

                string hours = splitResult[0];
                string minutes = splitResult[1];

                TimeSpan tsOffset = new TimeSpan(Convert.ToInt32(hours), Convert.ToInt32(minutes), 0);

                ret = new DateTimeOffset(pDate, tsOffset);
            }
            else
            {
                ret = new DateTimeOffset(pDate);
            }
            return ret;
        }

        /// <summary>
        /// Retourne la Date au format ISO yyyy-MM-ddTHH:mm:ss+hh:mm 
        /// </summary>
        /// <param name="pDate"></param>
        /// <returns></returns>
        public static string DateTimeToStringDateISO(DateTimeOffset pDate)
        {
            return pDate.ToString(StrFunc.AppendFormat("{0}{1}", FmtISODateTime2, "zzz"), CultureInfo.InvariantCulture);
        }
        /// <summary>
        /// Retourne la datetime convertie en UTC au format pPatternText
        /// </summary>
        // EG 20171026 [23509] Rename
        public static string DateTimeOffsetUTCToString(DateTimeOffset pDate, string pPatternText)
        {
            return pDate.ToUniversalTime().ToString(pPatternText, CultureInfo.InvariantCulture.DateTimeFormat);
        }
        /// <summary>
        /// Retourne la datetime convertie en UTC au format ISO avec marker offset UTC (Z)  (yyyy-MM-ddTHH:mm:ss.ffffffZ)
        /// </summary>
        // EG 20171026 [23509] Rename
        public static string DateTimeOffsetUTCToStringTZ(DateTimeOffset pDate)
        {
            return DateTimeOffsetUTCToString(pDate, FmtTZISOLongDateTime);
        }
        /// <summary>
        /// Retourne la datetime convertie en UTC au format ISO sans marker offset UTC (yyyy-MM-ddTHH:mm:ss.ffffff)
        /// </summary>
        // EG 20171026 [23509] Rename
        public static string DateTimeOffsetUTCToStringISO(DateTimeOffset pDate)
        {
            return DateTimeOffsetUTCToString(pDate, FmtISOLongDateTime);
        }
        /// <summary>
        ///  Retourne le nombre de {dayOfWeek} (lundi, mardi, etc...) sur un mois 
        ///  <para>Exemple : retourne le nombre de mardi présent sur un mois donné</para>
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="dayOfWeek">Lundi, mardi, etc..</param>
        /// <returns></returns>
        /// FI 20170320 [22985] Add 
        public static int GetNumberOfDayInMonth(int year, int month, DayOfWeek dayOfWeek)
        {
            int ret = 0;
            int daysInMonth = DateTime.DaysInMonth(year, month);
            for (int i = 1; i <= daysInMonth; i++)
            {
                DateTime d = new DateTime(year, month, i);
                if (d.DayOfWeek == dayOfWeek)
                    ret++;
            }
            return ret;
        }

        /// <summary>
        /// Retourne le nombre de jour de semaine (lundi, Mardi, Mercredi, Jeudi, Vendredi) entre 2 dates 
        /// </summary>
        /// <param name="pDtStart">Date début</param>
        /// <param name="pDtend">Date fin (inclue)</param>
        /// <returns></returns>
        /// FI 20170320 [22985] Add 
        public static int GetWeekDayInPeriod(DateTime pDtStart, DateTime pDtend)
        {
            int daysTotal = (pDtend - pDtStart).Days + 1;

            int ret = 0;
            for (int i = 0; i < daysTotal; i++)
            {
                DateTime d = pDtStart.AddDays(i);
                switch (d.DayOfWeek)
                {
                    case DayOfWeek.Saturday:
                    case DayOfWeek.Sunday:
                        break;
                    default:
                        ret++;
                        break;
                }
            }
            return ret;
        }

        /// <summary>
        ///  Retourne la liste des dates d'un mois
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static List<DateTime> GetDatesInMonth(int year, int month)
        {
            return Enumerable.Range(1, DateTime.DaysInMonth(year, month))  // Days: 1, 2 ... 31 etc.
                             .Select(day => new DateTime(year, month, day)) // Map each day to a date
                             .ToList(); // Load dates into a list
        }

        /// <summary>
        /// Retourne le n° de semaine associé à <paramref name="date"/> (commence par 1)
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static int GetWeekNumber(DateTime date)
        {
            int WeekNumber = Math.DivRem(date.Day, 7, out int res);
            if (res > 0)
                WeekNumber++;
            return WeekNumber;
        }
        #endregion
    }
    public partial class DtFunc
    {
#if DEBUG
        /// <summary>
        /// 
        /// </summary>
        public static class Test
        {
            /// <summary>
            ///  Plusieurs exemples d'usage de GetDateFunction 
            /// </summary>
            /// <para>Tests en relation avec les explications du ticket 16079</para>
            public static void Run()
            {
                DtFunc dtFunc = new DtFunc();

                //-7D
                _ = dtFunc.IsDateFunction("-7D");
                _ = dtFunc.GetDateFunction("-7D");

                //The Last day of the month
                _ = dtFunc.IsDateFunction("EOM");
                _ = dtFunc.GetDateFunction("EOM");

                //The Last day of the year
                _ = dtFunc.IsDateFunction("EOY");
                _ = dtFunc.GetDateFunction("EOY");

                //Date of the day less 1 calendar day
                _ = dtFunc.IsDateFunction("TODAY-1");
                _ = dtFunc.GetDateFunction("TODAY-1");

                //Date of the day less 2 months
                _ = dtFunc.IsDateFunction("TODAY-2M");
                _ = dtFunc.GetDateFunction("TODAY-2M");

                //Date of the day less 2 years 
                _ = dtFunc.IsDateFunction("TODAY-2Y");
                _ = dtFunc.GetDateFunction("TODAY-2Y");

                //The 22nd of the current month more 2 months
                _ = dtFunc.IsDateFunction("22+2M");
                _ = dtFunc.GetDateFunction("22+2M");

            }
        }
#endif
    }


    #endregion
    #region public class ObjFunc
    ///	<summary>
    ///	Management of object
    ///	</summary>
    public sealed class ObjFunc
    {
        #region public IsDBNull/IsNotDBNull
        public static bool IsDBNull(object pObject)
        {
            return (Convert.IsDBNull(pObject));
        }
        public static bool IsNotDBNull(object pObject)
        {
            return (!Convert.IsDBNull(pObject));
        }
        #endregion
        #region public IsNull/IsNotNull
        public static bool IsNull(object pObject)
        {
            return (null == pObject);
        }
        public static bool IsNotNull(object pObject)
        {
            return (null != pObject);
        }
        #endregion
        #region public IsFilled
        public static bool IsFilled(object pObject)
        {
            //PL 20111228 Refactoring
            //return !ObjFunc.IsNull(pObject);
            return ObjFunc.IsNotNull(pObject) && ObjFunc.IsNotDBNull(pObject);
        }
        #endregion
        #region public FmtToISo
        /// <summary>
        /// Retourne la donnée {pValue} au format ISO
        /// </summary>
        /// <param name="pValue">Valeur qui sera formatée </param>
        /// <param name="pDataType">typde de donnée </param>
        /// <returns></returns>
        /// FI 20171106 [XXXXX] Modify
        public static string FmtToISo(object pValue, TypeData.TypeDataEnum pDataType)
        {
            string ret = string.Empty;

            if (TypeData.IsTypeBool(pDataType))
            {
                if (BoolFunc.IsTrue(pValue))
                {
                    ret = "true";
                }
                else
                {
                    ret = "false";
                }
            }
            else if (TypeData.IsTypeString(pDataType) || TypeData.IsTypeText(pDataType) || TypeData.IsTypeXml(pDataType))
            {
                if (null != pValue)
                {
                    ret = pValue.ToString();
                }
            }
            else if (TypeData.IsTypeInt(pDataType))
            {
                if (null != pValue)
                {
                    ret = pValue.ToString();
                }
            }
            else if (TypeData.IsTypeDec(pDataType))
            {
                if (null != pValue)
                {
                    CultureInfo myCIclone = (CultureInfo)CultureInfo.InvariantCulture.Clone();
                    myCIclone.NumberFormat.NumberDecimalSeparator = ".";
                    myCIclone.NumberFormat.NumberGroupSeparator = string.Empty;
                    //Donnée formatée en invariant sans sépareteur de group (ex 1500.25)
                    //20080724 PL Bug avec une valeur 0.0
                    //ret = ((Decimal)pValue).ToString(myCIclone);  
                    ret = Convert.ToDecimal(pValue).ToString(myCIclone);
                }
            }
            else if (TypeData.IsTypeDateTime(pDataType))
            {
                if (null != pValue)
                {
                    //PL 20130205
                    //ret = DtFunc.DateTimeToString((DateTime)pValue, DtFunc.FmtISODateTime); // Modèle de date ISO 8601(yyyy-MM-ddTHH:mm:ss)
                    DateTime dt;
                    if (pValue.GetType() == System.Type.GetType("System.String"))
                    {
                        // RD 20180103 [23694] Use FmtISOLongDateTime also
                        //dt = DtFunc.ParseDate((string)pValue, DtFunc.FmtISODateTime2, null);   
                        dt = DtFunc.ParseDate((string)pValue, new string[] { DtFunc.FmtISODateTime2, DtFunc.FmtISOLongDateTime }, null);
                    }
                    else
                    {
                        dt = (DateTime)pValue;
                    }
                    // FI 20171106 [XXXXX] S'il existent des millisecondes Spheres® bascule vers FmtISOLongDateTime
                    if (dt.Millisecond > 0)
                        ret = DtFunc.DateTimeToString(dt, DtFunc.FmtISOLongDateTime);
                    else
                        ret = DtFunc.DateTimeToString(dt, DtFunc.FmtISODateTime);
                }
            }
            else if (TypeData.IsTypeDate(pDataType))
            {
                if (null != pValue)
                {
                    //PL 20130205
                    //ret = DtFunc.DateTimeToString((DateTime)pValue, DtFunc.FmtISODate);    
                    DateTime dt;
                    if (pValue.GetType() == System.Type.GetType("System.String"))
                    {
                        dt = DtFunc.ParseDate((string)pValue, DtFunc.FmtISODate, null);
                    }
                    else
                    {
                        dt = (DateTime)pValue;
                    }
                    ret = DtFunc.DateTimeToString(dt, DtFunc.FmtISODate);
                }
            }
            else if (TypeData.IsTypeTime(pDataType))
            {
                if (null != pValue)
                {
                    //PL 20130205
                    //ret = DtFunc.DateTimeToString((DateTime)pValue, DtFunc.FmtISOTime);     // HH:mm:ss
                    DateTime dt;
                    if (pValue.GetType() == System.Type.GetType("System.String"))
                    {
                        dt = DtFunc.ParseDate((string)pValue, DtFunc.FmtISOTime, null);
                    }
                    else
                    {
                        dt = (DateTime)pValue;
                    }
                    ret = DtFunc.DateTimeToString(dt, DtFunc.FmtISOTime);      //HH:mm:ss
                }
            }
            else
            {
                // RD 20190917 [24948] Clarifier le message d'erreur
                //throw new SpheresException(MethodInfo.GetCurrentMethod().Name, "Unable to tormat Datatype :" + pDataType.ToString());
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "<b>Unable to format data.</b>" + Cst.CrLf + "- Type: <b>" + pDataType.ToString() + "</b>" + Cst.CrLf + "- Value: <b>" + (pValue == null ? "" : pValue.ToString()) + "</b>");
            }
            return ret;
        }
        #endregion
        #region public NullToEmpty
        public static string NullToEmpty(object pObject)
        {
            //PL 20111228 Refactoring
            //if (null == pObject)
            //    return string.Empty;
            //else
            //    return pObject.ToString();
            if (IsFilled(pObject))
                return pObject.ToString();
            else
                return string.Empty;
        }
        #endregion public NullToEmpty
    }
    #endregion
    #region public class StrFunc
    ///	<summary>
    ///	Management of string
    ///	</summary>
    public sealed class StrFunc
    {
        /// <summary>
        /// Construit une string avec des éléments séparés par des virgule ','
        /// <para>Lorsque la lise dépasse le nombre d'éléments max, la string se termine par ',...'</para>
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="pElement"></param>
        /// <param name="pMaxElement">-1 pour ne pas limiter</param>
        // FI 20130423 [18601] Add Method
        // PL 20140312 Methode auparavant dans List.aspx
        public static void BuildStringListElement(ref string pData, string pDataElement, int pMaxElement)
        {
            if ((pMaxElement == 0) || (pMaxElement < -1))
                throw new ArgumentException("{0} is not valid for parameter pMaxElement", pMaxElement.ToString());

            if (StrFunc.IsFilled(pData))
            {
                string[] array = pData.Split(',');
                int arrayCount = ArrFunc.Count(array);
                if ((arrayCount < pMaxElement) || pMaxElement == -1)
                {
                    if ((arrayCount) == 0)
                        pData = pDataElement;
                    else
                        pData += "," + pDataElement;
                }
                else
                {
                    if (false == pData.EndsWith(",..."))
                        pData += ",...";
                }
            }
            else
            {
                pData = pDataElement;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pString"></param>
        /// <param name="pData"></param>
        /// <returns></returns>
        public static string AppendFormat(string pString, params object[] pData)
        {
            return new StringBuilder().AppendFormat(pString, pData).ToString();
        }

        /// <summary>
        /// Contains string pString In String pContainer
        /// </summary>
        /// <param name="pContainer"></param>
        /// <param name="pString"></param>
        /// <returns></returns>
        public static bool ContainsIn(string pContainer, string pString)
        {
            bool ret = false;
            if (IsFilled(pContainer))
                ret = (pContainer.IndexOf(pString) != -1);
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pString"></param>
        /// <returns></returns>
        public static bool IsDecimalInvariantFilled(string pString)
        {
            bool isFilled = IsFilled(pString);
            if (isFilled)
            {
                try
                {
                    isFilled = (DecFunc.DecValue(pString).CompareTo(Convert.ToDecimal(0)) != 0);
                }
                catch
                {
                    isFilled = false;
                }
            }
            return isFilled;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pString"></param>
        /// <returns></returns>
        public static bool IsIntegerInvariantFilled(string pString)
        {
            bool isFilled = IsFilled(pString);
            if (isFilled)
            {
                try
                {
                    isFilled = (IntFunc.IntValue64(pString).CompareTo(Convert.ToInt64(0)) != 0);
                }
                catch
                {
                    isFilled = false;
                }
            }
            return isFilled;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pString"></param>
        /// <returns></returns>
        public static bool IsDecimalFilled(string pString)
        {
            bool isFilled = IsFilled(pString);
            if (isFilled)
            {
                try
                {
                    isFilled = !DecFunc.DecValue(pString.ToString()).Equals(0);
                }
                catch
                {
                    isFilled = false;
                }
            }
            return isFilled;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pString"></param>
        /// <returns></returns>
        public static bool IsEmpty(string pString)
        {
            return ((null == pString) || (0 == pString.Length) || (0 == pString.TrimEnd().Length));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pString"></param>
        /// <returns></returns>
        public static bool IsFilled(string pString)
        {
            return !StrFunc.IsEmpty(pString);
        }

        // EG 20161122 New
        public static bool IsUndefined(string pString)
        {
            return "undefined" == pString.ToLower();
        }

        // EG 20161122 New
        public static bool IsEmptyOrUndefined(string pString)
        {
            return StrFunc.IsEmpty(pString) || StrFunc.IsUndefined(pString);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pString"></param>
        /// <returns></returns>
        public static bool IsFilledOrSpace(string pString)
        {
            bool ret = IsFilled(pString);
            if ((!ret) && ((null != pString)))
                ret = (pString.Length > 0);
            return ret;
        }

        /// <summary>
        /// Obtient true si la string en entrée est un flux XML
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        public static bool IsXML(string pData)
        {
            //Formule trouvée sur le net
            //
            //un flux XML 
            //1 Commence par <
            //2 suivit de ? (pour le tag xml) ou un ensemble de caractères
            //3. suivit de tout sauf un >
            //4. terminé par un >
            Regex regEx = new Regex(@"<\??\w+[^>]*>");
            return regEx.IsMatch(pData);
        }

        #region public IsDate
        public static bool IsDate(string pCheckValue, string pDataFormat)
        {
            return IsDate(pCheckValue, pDataFormat, null);
        }
        public static bool IsDate(string pCheckValue, string pDataFormat, CultureInfo pCulture)
        {
            return IsDate(pCheckValue, pDataFormat, pCulture, out _);
        }
        public static bool IsDate(string pCheckValue, string pDataFormat, CultureInfo pCulture, out DateTime pDate)
        {

            if (StrFunc.IsFilled(pDataFormat))
                pDate = (Convert.IsDBNull(pCheckValue) ? DateTime.MinValue.Date : new DtFunc().StringToDateTime(pCheckValue, pDataFormat, pCulture).Date);
            else
                pDate = (Convert.IsDBNull(pCheckValue) ? DateTime.MinValue.Date : new DtFunc().StringToDateTime(pCheckValue).Date);

            return DtFunc.IsDateTimeFilled(pDate);
        }
        #endregion
        #region public IsDateTime
        public static bool IsDateTime(string pCheckValue, string pDataFormat)
        {
            return IsDateTime(pCheckValue, pDataFormat, null);
        }
        public static bool IsDateTime(string pCheckValue, string pDataFormat, CultureInfo pCulture)
        {
            DateTime dtDate;
            if (StrFunc.IsFilled(pDataFormat))
                dtDate = (Convert.IsDBNull(pCheckValue) ? DateTime.MinValue : new DtFunc().StringToDateTime(pCheckValue, pDataFormat, pCulture));
            else
                dtDate = (Convert.IsDBNull(pCheckValue) ? DateTime.MinValue : new DtFunc().StringToDateTime(pCheckValue));
            return DtFunc.IsDateTimeFilled(dtDate);
        }
        #endregion
        #region public IsDateSys
        public static bool IsDateSys(string pCheckValue, string pDataFormat)
        {
            return IsDateSys(pCheckValue, pDataFormat, null);
        }
        public static bool IsDateSys(string pCheckValue, string pDataFormat, CultureInfo pCulture)
        {
            DateTime dtDate;
            if (StrFunc.IsFilled(pDataFormat))
                dtDate = (Convert.IsDBNull(pCheckValue) ? DateTime.MinValue.Date : new DtFunc().StringToDateTime(pCheckValue, pDataFormat, pCulture).Date);
            else
                dtDate = (Convert.IsDBNull(pCheckValue) ? DateTime.MinValue.Date : new DtFunc().StringToDateTime(pCheckValue).Date);
            return DtFunc.DateTimeToStringDateISO(dtDate.Date) == DtFunc.DateTimeToStringDateISO(DateTime.Now);
        }
        #endregion
        #region public IsDateTimeSys
        public static bool IsDateTimeSys(string pCheckValue, string pDataFormat)
        {
            return IsDateTimeSys(pCheckValue, pDataFormat, null);
        }
        public static bool IsDateTimeSys(string pCheckValue, string pDataFormat, CultureInfo pCulture)
        {
            DateTime dtDate;
            if (StrFunc.IsFilled(pDataFormat))
                dtDate = (Convert.IsDBNull(pCheckValue) ? DateTime.MinValue : new DtFunc().StringToDateTime(pCheckValue, pDataFormat, pCulture));
            else
                dtDate = (Convert.IsDBNull(pCheckValue) ? DateTime.MinValue : new DtFunc().StringToDateTime(pCheckValue));
            return DtFunc.DateTimeToStringISO(dtDate) == DtFunc.DateTimeToStringISO(DateTime.Now);
        }
        #endregion
        //
        #region public ReverseString
        /// <summary>
        /// Reverse string
        /// </summary>
        /// <param name="psString">String</param>
        /// <returns>String</returns>
        /// RD 20220211 [25943] New
        public static string ReverseString(string psString)
        {
            char[] array = psString.ToCharArray();
            Array.Reverse(array);
            return new string(array);
        }
        #endregion
        #region public IntercalateString
        /// <summary>
        /// Add an interlayer character between all characters in the string
        /// </summary>
        /// <param name="psString"></param>
        /// <param name="pcInterlayer"></param>
        /// RD 20220215 [25943] New
        /// <returns></returns>
        public static string IntercalateString(string psString, char pcInterlayer)
        {
            string dest = string.Empty;

            foreach (var c in psString)
                dest += c.ToString() + pcInterlayer.ToString();

            return dest.TrimEnd(pcInterlayer);
        }
        #endregion        
        #region public ExtractString
        /// <summary>
        /// Extract string between two separator<c>psSeparator</c> at position <c>plPosition</c>
        /// </summary>
        /// <param name="psString">String</param>
        /// <param name="plPosition">Start position</param>
        /// <param name="psSeparator">Separator</param>
        /// <param name="pbReverse">Reverse scan</param>
        /// <returns>String</returns>
        public static string ExtractString(string psString, int plPosition, string psSeparator, bool pbReverse)
        {
            string[] aTmp;
            int lNbExpression;

            aTmp = Regex.Split(psString, psSeparator, RegexOptions.IgnoreCase);
            lNbExpression = aTmp.GetUpperBound(0);

            if (pbReverse)
            {
                plPosition = lNbExpression - plPosition;
            }

            if ((plPosition >= 0) && (plPosition <= lNbExpression))
            {
                return aTmp[plPosition];
            }
            else
            {
                return "";
            }
        }
        public static string ExtractString(string psString, int plPosition, string psSeparator)
        {
            return ExtractString(psString, plPosition, psSeparator, false);
        }
        public static string ExtractString(string psString, int plPosition)
        {
            return ExtractString(psString, plPosition, ";", false);
        }
        #endregion
        #region public FirstUpperCase
        public static string FirstUpperCase(string pStr)
        {
            string ret = string.Empty;
            if (IsFilled(pStr))
            {
                if (pStr.Length == 1)
                    ret = pStr.ToUpper();
                else
                {
                    ret = Char.ToUpper(pStr[0]).ToString() + pStr.Substring(1);
                }
            }
            return ret;
        }

        #endregion
        #region public FirstLowerCase
        public static string FirstLowerCase(string pStr)
        {
            string ret = string.Empty;
            if (IsFilled(pStr))
            {
                if (pStr.Length == 1)
                    ret = pStr.ToLower();
                else
                {
                    ret = Char.ToLower(pStr[0]).ToString() + pStr.Substring(1);
                }
            }
            return ret;
        }
        #endregion
        #region public HashData
        public static string HashData(string pData, string pAlgorithm)
        {
            //switch (pAlgorithm)
            //{
            //    case "MD5":
            //        return StrFunc.HashData(pData, Cst.HashAlgorithm.MD5);
            //    case "SHA1":
            //        return StrFunc.HashData(pData, Cst.HashAlgorithm.SHA1);
            //    default:
            //        return StrFunc.HashData(pData, Cst.HashAlgorithm.None);
            //}
            //PL 20110718 Refactoring
            if (Enum.IsDefined(typeof(Cst.HashAlgorithm), pAlgorithm))
                return StrFunc.HashData(pData, (Cst.HashAlgorithm)Enum.Parse(typeof(Cst.HashAlgorithm), pAlgorithm, true));
            else
                return StrFunc.HashData(pData, Cst.HashAlgorithm.None);
        }
        // EG 20160404 Migration vs2013
        public static string HashData(string pData, Cst.HashAlgorithm pAlgorithm)
        {
            string hashData = string.Empty;
            // EG 20160404 Migration vs2013
            byte[] data;
            switch (pAlgorithm)
            {
                // EG 20160404 Migration vs2013
                //case Cst.HashAlgorithm.MD5:
                //    hashData = FormsAuthentication.HashPasswordForStoringInConfigFile(pData, "MD5");
                //    break;

                //case Cst.HashAlgorithm.SHA1:
                //    hashData = FormsAuthentication.HashPasswordForStoringInConfigFile(pData, "SHA1");
                //    break;
                case Cst.HashAlgorithm.MD5:
                case Cst.HashAlgorithm.SHA1:
                    HashAlgorithm _hashAlgorithm;
                    if (pAlgorithm == Cst.HashAlgorithm.MD5)
                        _hashAlgorithm = MD5.Create();
                    else
                        _hashAlgorithm = SHA1.Create();
                    data = _hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(pData));
                    hashData = string.Empty;
                    for (int i = 0; i < data.Length; i++)
                    {
                        hashData += data[i].ToString("x2").ToUpperInvariant();
                    }
                    break;

                case Cst.HashAlgorithm.SHA256:
                case Cst.HashAlgorithm.SHA384:
                case Cst.HashAlgorithm.SHA512:
                    byte[] result;
                    // EG 20160404 Migration vs2013
                    data = new byte[pData.Length * 8];
                    for (int i = 0; i <= pData.Length - 1; i++)
                        data[i] = Convert.ToByte(pData[i]);

                    switch (pAlgorithm)
                    {
                        case Cst.HashAlgorithm.SHA256:
                            System.Security.Cryptography.SHA256 sha256M = new System.Security.Cryptography.SHA256Managed();
                            result = sha256M.ComputeHash(data);
                            hashData = Convert.ToBase64String(result);
                            break;
                        case Cst.HashAlgorithm.SHA384:
                            System.Security.Cryptography.SHA256 sha384M = new System.Security.Cryptography.SHA256Managed();
                            result = sha384M.ComputeHash(data);
                            hashData = Convert.ToBase64String(result);
                            break;
                        case Cst.HashAlgorithm.SHA512:
                            System.Security.Cryptography.SHA512 sha512M = new System.Security.Cryptography.SHA512Managed();
                            result = sha512M.ComputeHash(data);
                            hashData = Convert.ToBase64String(result);
                            break;
                    }
                    break;

                case Cst.HashAlgorithm.None:
                default:
                    hashData = pData;
                    break;
            }

            //NB: ACTOR.PWD is a varchar(128)
            if (hashData.Length > 128)
            {
                int start = Convert.ToInt32((hashData.Length - 128) / 2);
                hashData = hashData.Substring(start, 128);
            }

            return hashData;
        }
        #endregion
        //
        #region public FmtAmountToInvariantCulture
        /// <summary>
        /// Formatage d'un decimal représenté dans la culture CurrentCulture en Culture Invaraint
        /// <para>
        /// <param name="pAmount">Montant représenté dans la culture CurrentCulture</param>
        /// <returns></returns>
        public static string FmtAmountToInvariantCulture(string pAmount)
        {
            string retVal;
            try
            {
                // used to fill space for French Group separator
                // this used to format 1,000.00 => 1000.00 for FpML
                NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
                nfi.NumberGroupSeparator = string.Empty;
                retVal = Decimal.Parse(pAmount, Thread.CurrentThread.CurrentCulture).ToString("n", nfi);
            }
            catch
            {
                retVal = pAmount;
            }
            return retVal;
        }
        #endregion
        #region public FmtAmountToGUI
        /// <summary>
        /// Formatage d'un decimal représenté dans la culture Invariant dans la culture CurrentCulture
        /// <para>
        /// Résultat : chiffres intégraux et décimaux, séparateurs de groupes et séparateur décimal avec un signe négatif facultatif.
        /// <para>
        /// Spécificateur de précision par défaut : défini par NumberFormatInfo.NumberDecimalDigits de la CurrentCulture.</para>    
        /// </para>
        /// </summary>
        /// <param name="pAmount">Montant représenté dans la culture invariant</param>
        /// <returns></returns>
        public static string FmtAmountToGUI(string pAmount)
        {
            return Decimal.Parse(pAmount, CultureInfo.InvariantCulture).ToString("n", Thread.CurrentThread.CurrentCulture);
        }
        /// <summary>
        /// Formatage d'un decimal représenté dans la culture Invariant dans la culture CurrentCulture (avec précision)
        /// <para>
        /// Résultat : chiffres intégraux et décimaux, séparateurs de groupes et séparateur décimal avec un signe négatif facultatif.
        /// </summary>
        /// <param name="pAmount">Montant représenté dans la culture Invariant</param>
        /// <param name="pDecimalDigits">précision demandée</param>
        /// <returns></returns>
        public static string FmtAmountToGUI(string pAmount, int pDecimalDigits)
        {
            int decimalDigits = pDecimalDigits;
            string retVal;
            try
            {
                // FI 20191217 [XXXXX] Usage de clone() car lors de l'éxécution de tâche Asynchrone Thread.CurrentThread.CurrentCulture peut-être en ReadOnly
                NumberFormatInfo nfi = (NumberFormatInfo)Thread.CurrentThread.CurrentCulture.NumberFormat.Clone();
                nfi.NumberDecimalDigits = decimalDigits;
                retVal = Decimal.Parse(pAmount, CultureInfo.InvariantCulture).ToString("n", nfi);
            }
            catch
            {
                retVal = pAmount;
            }
            return retVal;
        }
        #endregion

        //
        #region public FmtIntegerToInvariantCulture2
        /// <summary>
        /// Retourne un integer au format iso (sans séparateur de millier) 
        /// <para>
        /// Interprétation des constantes K et M 
        /// </para>
        /// <para>
        /// 1,2M ou 1M2 sur une station french donne 1200000
        /// </para>
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        public static string FmtIntegerToInvariantCulture2(string pData)
        {
            string data = pData;
            //
            if (StrFunc.IsFilled(data))
            {
                try
                {
                    data = InterceptMK(data);
                }
                catch { data = "0"; }
            }
            return FmtIntegerToInvariantCulture(data);
        }
        #endregion
        #region public FmtIntegerToInvariantCulture
        /// <summary>
        /// Retourne un integer au format iso (sans séparateur de millier) 
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        public static string FmtIntegerToInvariantCulture(string pData)
        {
            return FmtInteger(pData, false, false);
        }
        public static string FmtIntegerToInvariantCulture(long pData)
        {
            return FmtInteger(pData, false, false);
        }
        #endregion
        #region public FmtIntegerToCurrentCulture
        public static string FmtIntegerToCurrentCulture(string pData)
        {
            return FmtInteger(pData, true, false);
        }
        #endregion
        #region private FmtInteger
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        private static string FmtInteger(string pData, bool pIsFromInvariantToCurrentCulture, bool pIsSigned)
        {
            string data = string.Empty;
            //
            CultureInfo cultureFrom;
            //
            if (pIsFromInvariantToCurrentCulture)
            {
                cultureFrom = CultureInfo.InvariantCulture;
            }
            else
            {
                cultureFrom = Thread.CurrentThread.CurrentCulture;
            }
            //
            if (StrFunc.IsFilled(pData))
            {
                data = pData.Trim();
                //20090602 FI Add ReplaceDecimalSeparatorInvariantToCulture 
                //au cas où la donnée pData est un decimal au foramt ISO 
                //Ex si en entrée nous avons 85.25 => en sortie nous aurons 85,25 (en culture French)
                if (false == pIsFromInvariantToCurrentCulture)
                {
                    //PL 20091214 / FI 20091214 Ajout du If() sur NumberGroupSeparator pour éviter un dysfonctionnement (ex. it-IT, cf détail dans ReplaceDecimalSeparatorInvariantToCulture())
                    if (cultureFrom.NumberFormat.NumberGroupSeparator != ".")
                    {
                        data = StrFunc.ReplaceDecimalSeparatorInvariantToCulture(data, cultureFrom);
                    }
                }
            }
            //
            //20090602 FI utilisation de IntValue2, IntValue2 ne plante pas si la donnée est un décimal
            //Ex si en entrée nous avons 85,25 => en sortie nous aurons 85 (en culture French)
            // EG 20150920 [21314]
            long intVal = IntFunc.IntValue64(data, cultureFrom);
            //
            return FmtInteger(intVal, pIsFromInvariantToCurrentCulture, pIsSigned);
        }
        private static string FmtInteger(long pData, bool pIsFromInvariantToCurrentCulture, bool pIsSigned)
        {
            string retVal = string.Empty;
            //
            //CultureInfo cultureFrom;
            CultureInfo cultureTo;
            if (pIsFromInvariantToCurrentCulture)
            {
                //cultureFrom = CultureInfo.InvariantCulture;
                cultureTo = Thread.CurrentThread.CurrentCulture;
            }
            else
            {
                //cultureFrom = Thread.CurrentThread.CurrentCulture;
                cultureTo = CultureInfo.InvariantCulture;
            }
            //
            try
            {
                if (pIsFromInvariantToCurrentCulture)
                {
                    retVal = pData.ToString("n0", cultureTo);
                    if (pIsSigned && (0 < pData))
                        retVal = "+" + retVal;
                }
                else
                {
                    // this used to format 1,000 => 1000 for FpML
                    NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
                    nfi.NumberGroupSeparator = string.Empty;
                    retVal = pData.ToString(nfi);
                }
            }
            catch
            { }
            return retVal;
        }
        #endregion FmtInteger
        //
        #region public FmtDecimalToInvariantCulture2
        /// <summary>
        /// Retourne un decimal au format iso (sans séparateur de millier, "." est le séparateur de décimal) 
        /// <para>
        /// Interprétation des constantes K et M 
        /// </para>
        /// <para>
        /// 1,2M ou 1M2 sur une station french donne 1200000.00
        /// </para>
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        public static string FmtDecimalToInvariantCulture2(string pData)
        {
            string data = pData;
            //
            if (StrFunc.IsFilled(data))
            {
                try
                {
                    data = InterceptMK(data);
                }
                catch { data = "0"; }
            }
            //
            return FmtDecimalToInvariantCulture(data);
        }
        #endregion
        #region public FmtDecimalToInvariantCulture
        /// <summary>
        /// Retourne un decimal au format iso (sans séparateur de millier, "." est le séparateur de décimal) 
        /// <para>2 decimales au minimum</para>
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// FI 20150417 [XXXX] Modify
        public static string FmtDecimalToInvariantCulture(string pData)
        {
            /// FI 20150417 [XXXX] null pour pNumberOfDecimalMin
            return FmtDecimal(pData, false, null);
        }
        /// <summary>
        /// Retourne un decimal au format iso (sans séparateur de millier, "." est le séparateur de décimal) 
        /// <para>Possibilité de définir un minimum de decimale</para>
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="pNumberOfDecimalMin"></param>
        /// <returns></returns>
        /// FI 20150417 [XXXX] Add
        public static string FmtDecimalToInvariantCulture(string pData, Nullable<int> pNumberOfDecimalMin)
        {
            return FmtDecimal(pData, false, pNumberOfDecimalMin);
        }

        /// <summary>
        /// Retourne un decimal au format iso (sans séparateur de millier, "." est le séparateur de décimal) 
        /// <para>2 decimales au minimum</para>
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// FI 20131112 [17861] Modification
        public static string FmtDecimalToInvariantCulture(decimal pData)
        {
            // FI 20131112 passage de 9 à 12 caractères# puisque sur PRISMA les cours de change ont tous 12 décimales après la virgule
            //string data = pData.ToString("0.#########;-0.#########;0");

            string data = pData.ToString("0.############;-0.############;0");
            // Rq sur la variable dtata =>le séparateur de decimal est celui de la culture du thread. Il n'y a pas de séparateur de millier.

            return FmtDecimalToInvariantCulture(data);
        }


        /// <summary>
        /// Retourne un decimal au format iso (sans séparateur de millier, "." est le séparateur de décimal) 
        /// <para>Possibilité de choisir le nb de decimal minimun</para>
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// FI 20150417 [XXXX] Add
        public static string FmtDecimalToInvariantCulture(decimal pData, Nullable<int> pNumberOfDecimalMin)
        {
            // FI 20131112 passage de 9 à 12 caractères# puisque sur PRISMA les cours de change ont tous 12 décimales après la virgule
            //string data = pData.ToString("0.#########;-0.#########;0");

            string data = pData.ToString("0.############;-0.############;0");
            // Rq sur la variable dtata =>le séparateur de decimal est celui de la culture du thread. Il n'y a pas de séparateur de millier.

            return FmtDecimalToInvariantCulture(data, pNumberOfDecimalMin);
        }

        #endregion
        #region public FmtDecimalToCurrentCulture
        /// <summary>
        /// Formate la donnée string {pDada} dans la culture du thread. {pDada} doit représenter un decimal au format invariant culture.
        /// <para>2 décimale au minimum</para>
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// FI 20150417 [XXXX] Modify
        public static string FmtDecimalToCurrentCulture(string pData)
        {
            // FI 20150417 [XXXX] paramètre pNumberOfDecimalMin à null
            return FmtDecimal(pData, true, null);
        }
        /// <summary>
        /// Formate une donnée décimale dans la culture du thread. Le montant est arrondi à 2 décimales
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="pPrecision">Nbr de decimale</param>
        /// <returns></returns>
        public static string FmtDecimalToCurrentCulture(decimal pData)
        {
            return FmtDecimal(pData, 2, true);
        }
        /// <summary>
        /// Formate une donnée décimale en string selon la culture du thread
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="pPrecision">Nbr de decimale</param>
        /// <returns></returns>
        public static string FmtDecimalToCurrentCulture(decimal pData, int pPrecision)
        {
            return FmtDecimal(pData, pPrecision, true);
        }
        #endregion
        #region private FmtDecimal
        /// <summary>
        /// Convertie une donnée en décimale puis formate la donnée
        /// <para>Possibilité de spécifier un nbre de decimal minimum</para>
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="pIsFromInvariantToCurrentCulture">si true:Formatage selon la culture du thread sinon formatage en Invariant Culture</param>
        /// <param name="pNumberOfDecimalMin">Optionel, si non renseigné, il y a 2 décimale minimum</param>
        /// <returns></returns>
        /// FI 20150417 [XXXXX] Modify 
        private static string FmtDecimal(string pData, bool pIsFromInvariantToCurrentCulture, Nullable<int> pNumberOfDecimalMin)
        {
            string data = string.Empty;

            CultureInfo cultureFrom;
            if (pIsFromInvariantToCurrentCulture)
                cultureFrom = CultureInfo.InvariantCulture;
            else
                cultureFrom = Thread.CurrentThread.CurrentCulture;

            if (StrFunc.IsFilled(pData))
            {
                data = pData.Trim();
                if (false == pIsFromInvariantToCurrentCulture)
                    data = StrFunc.ReplaceDecimalSeparatorInvariantToCulture(data, cultureFrom);
            }
            //
            // Note: general function. Need to make better by taking into
            // consideration that the number of currency digits for a number
            // can be different from the number of digits after a number.
            // find last significant digit after decimal
            Decimal decVal = DecFunc.DecValue(data, cultureFrom);
            //
            int totaLength = data.Length;
            int numberOfDecimal = 0;

            int posDecimalSeparator = data.IndexOf(cultureFrom.NumberFormat.NumberDecimalSeparator);

            bool isInteger = (-1 == posDecimalSeparator);

            if (!isInteger)
            {
                numberOfDecimal = (totaLength - posDecimalSeparator - 1);

                char c = data[totaLength - 1];

                int posLastDigit;
                for (posLastDigit = totaLength; posLastDigit > posDecimalSeparator; posLastDigit--)
                {
                    if (c != '0')
                        break;
                    else
                        c = data[posLastDigit - 2];
                }
                _ = (posDecimalSeparator + 1 == posLastDigit);
            }

            // FI 20150417 [XXXXX] valorisation de numberOfDecimalMin 
            int numberOfDecimalMin = 2;
            if (pNumberOfDecimalMin.HasValue)
                numberOfDecimalMin = pNumberOfDecimalMin.Value;

            if (numberOfDecimal < numberOfDecimalMin)
                numberOfDecimal = numberOfDecimalMin;

            return FmtDecimal(decVal, numberOfDecimal, pIsFromInvariantToCurrentCulture);
        }
        /// <summary>
        /// Formate une donnée décimale en string.
        /// <para>Il n'y a pas de séparteur de milliers lors du formatage en  Invariant Culture</para>
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="pPrecision">nombre de décimale</param>
        /// <param name="pIsToCurrentCulture">si true:Formatage selon la culture du thread sinon formatage en Invariant Culture</param>
        /// <returns></returns>
        private static string FmtDecimal(decimal pData, int pPrecision, bool pIsToCurrentCulture)
        {
            string retVal = string.Empty;

            CultureInfo cultureTo;
            if (pIsToCurrentCulture)
            {
                cultureTo = Thread.CurrentThread.CurrentCulture;
            }
            else
            {
                cultureTo = CultureInfo.InvariantCulture;
            }

            try
            {
                if (pIsToCurrentCulture)
                {
                    retVal = pData.ToString("n" + pPrecision, cultureTo);
                }
                else
                {
                    // This used to format 1,000.00 => 1000.00 for FpML
                    NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
                    nfi.NumberGroupSeparator = string.Empty;
                    retVal = pData.ToString("n" + pPrecision, nfi);
                }
            }
            catch { }

            return retVal;
        }
        #endregion FmtDecimal
        //PL 20141017 New FmtMoneyToCurrentCulture
        #region public FmtMoneyToCurrentCulture
        /// <summary>
        /// Retourne sous forme de string un Money (Montant + Devise), formaté sur la base de la culture courante.
        /// <para>exemples.</para>
        /// <para>- culture "en": USD1,000.50</para>
        /// <para>- culture "fr": 1 000,50 USD</para>
        /// </summary>
        /// <param name="pAmount"></param>
        /// <param name="pCurrency"></param>
        /// <returns></returns>
        public static string FmtMoneyToCurrentCulture(decimal pAmount, string pCurrency)
        {
            string twoLetterISOLanguageName = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToLower();
            string amount = FmtDecimalToCurrentCulture(pAmount);
            if (twoLetterISOLanguageName == "fr" || twoLetterISOLanguageName == "it")
                return amount + " " + pCurrency;
            else
                return pCurrency + amount;
        }
        #endregion
        //
        #region private InterceptMK
        /// <summary>
        /// Interprétation de la donnée pData si elle contient la constantes K ou M 
        /// Si l'interprétation aboutit alors la fonction retourne le résultat décimal au format iso
        /// sinon la fonction retourne la donnée en entrée
        /// 
        /// <para>
        /// 1,2M ou 1M2 sur une station french donne 1200000.00
        /// </para>
        /// </summary>
        /// 
        /// <param name="pData"></param>
        /// <returns></returns>
        private static string InterceptMK(string pData)
        {
            string data = pData.ToUpper();
            string MK = "M";

            if (StrFunc.IsFilled(data))
            {
                int posMK = data.IndexOf("K");
                if (posMK > 0)
                    MK = "K";

                if (false == (posMK > 0))
                {
                    posMK = data.IndexOf("M");
                    if (posMK > 0)
                        MK = "M";
                }

                if (posMK > 0)
                {
                    NumberFormatInfo nbfi = Thread.CurrentThread.CurrentUICulture.NumberFormat;
                    Regex regex = new Regex(@"[\.\" + nbfi.NumberDecimalSeparator + "]");
                    bool IsDecimal = regex.IsMatch(data);
                    string[] dataSplit = data.Split(MK.ToCharArray());
                    string dataLeftSep = dataSplit[0];

                    if (IsDecimal) //=> Ex  1,2 M
                    {
                        decimal decValue = DecFunc.DecValue(dataLeftSep, Thread.CurrentThread.CurrentCulture);
                        decValue *= (("M" == MK) ? 1000000 : 1000);
                        data = StrFunc.FmtDecimalToInvariantCulture(decValue);   // Data est déjà en invariant culture=> il sera interprete correctement par  FmtDecimalToInvariantCulture
                    }
                    else	//=> Ex  1M2
                    {
                        string dataRightSep = dataSplit[1];
                        data = dataLeftSep + dataRightSep.PadRight((("M" == MK) ? 6 : 3), Convert.ToChar("0"));
                        data = StrFunc.FmtDecimalToInvariantCulture(data);   // Data est déjà en invariant culture=> il sera interprete correctement par  FmtDecimalToInvariantCulture
                    }
                }
            }
            return data;
        }
        #endregion
        //
        #region public FmtFractionToInvariantCulture
        public static string FmtFractionToInvariantCulture(string pDataValue)
        {
            bool isOk = false;
            string[] sTemp = null;

            if (StrFunc.IsFilled(pDataValue))
            {
                sTemp = pDataValue.Split("/".ToCharArray());
                isOk = (ArrFunc.IsFilled(sTemp) && (sTemp.Length == 2));
            }
            if (false == isOk)
                throw new ArgumentException("Argument is not a valid Fraction");

            sTemp[0] = sTemp[0].Trim();
            sTemp[1] = sTemp[1].Trim();

            bool isDecimal = false;
            for (int i = 0; i < sTemp.Length; i++)
            {
                isDecimal = (DecFunc.IsDecimalNotInteger(sTemp[i]));
                if (isDecimal)
                    break;
            }
            string ret;
            if (isDecimal)
                ret = FmtDecimalToInvariantCulture(sTemp[0]) + "/" + FmtDecimalToInvariantCulture(sTemp[1]);
            else
                ret = sTemp[0] + "/" + sTemp[1];
            return ret;
        }
        #endregion FmtFractionToInvariantCulture


        #region public DeleteGroupSeparator
        /// <summary>
        /// Suppression du séparateur de millier à partir des spécifications de la culture courante
        /// </summary>
        /// <param name="pStr"></param>
        /// <returns></returns>
        public static string DeleteGroupSeparator(string pStr)
        {
            return DeleteGroupSeparator(pStr, Thread.CurrentThread.CurrentCulture);
        }
        /// <summary>
        /// Suppression du séparateur de millier à partir des spécifications de la culture {pCultureInfo}
        /// </summary>
        /// <param name="pStr"></param>
        /// <param name="pCultureInfo"></param>
        /// <returns></returns>
        public static string DeleteGroupSeparator(string pStr, CultureInfo pCultureInfo)
        {
            string ret = pStr;
            //
            if (StrFunc.IsFilled(pStr) && (pStr != Cst.HTMLSpace))
            {
                //Suppression du séparateur de millier
                NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
                nfi.NumberGroupSeparator = string.Empty;
                nfi.NumberDecimalDigits = 0;
                ret = Decimal.Parse(pStr, pCultureInfo).ToString("n", nfi);
            }
            //
            return ret;
        }
        #endregion

        #region public ReplaceDecimalSeparatorInvariantToCulture
        /// <summary>
        /// Interprétation du "." comme séparateur de décimal quelle que soit la culture
        /// <para>
        /// Remplace le séparateur décimal Invariant "." par le séparateur de pCultureInfo 
        /// </para>
        /// <para>
        /// Ceci n'est effectué que 
        /// si le séparateur de pCultureInfo n'existe pas dans la donnée en entrée
        /// et s'il le "." n'est présent qu'une seule fois
        /// </para>
        /// 20080305 FI Ticket 16120  
        /// </summary>
        /// <param name="pStr"></param>
        /// <param name="pCultureInfo"></param>
        /// <returns></returns>
        public static string ReplaceDecimalSeparatorInvariantToCulture(string pStr, CultureInfo pCultureInfo)
        {
            string ret = pStr;
            //
            // S'il n'existe pas le separateur de décimal Tentative d'interpretation du '.' s'il est présent
            // Ex Italien 1.000.000,00 => 1.000.000,00 (aucune intervention)
            // Ex Italien 1.000.000    => 1.000.000    (aucune intervention)
            // Ex Italien 1.000		   => 1,000        (interpretation du .)=> interprétation abusive, l'utilisateur saisi 1000, et il se retrouve avec 1 (on laisse en l'état)
            //                                                                 Cas à gérer en amont de cette méthode(ex. FmtInteger() PL 20091214 / FI 20091214) 
            // Ex Italien 1.32		   => 1,32         (interpretation du .)
            // Ex Italien 1.320,25	   => 1.320,25     (aucune intervention)
            //
            if (StrFunc.IsFilled(ret))
            {
                //Replace de blancs
                ret = ret.ToString();
                //
                bool ContainsCultureDecimalSeparator = (ret.IndexOf(pCultureInfo.NumberFormat.NumberDecimalSeparator) != -1);
                if (!ContainsCultureDecimalSeparator)
                {
                    int lastPointPosition = ret.LastIndexOf(".");
                    int firstPointPosition = ret.IndexOf(".");
                    // Remplacement du point "." par le separateur de decimal 
                    // Lorqu'il n'existe qu'1 seul  point ds la string 
                    bool bSubstitutePoint = (firstPointPosition != -1) && (lastPointPosition == firstPointPosition);
                    if (bSubstitutePoint)
                        ret = ret.Replace(".", pCultureInfo.NumberFormat.NumberDecimalSeparator);
                }
            }
            //
            return ret;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSource"></param>
        /// <returns></returns>
        public static string ReplaceNoBreakSpaceByWhiteSpace(string pSource)
        {
            string dest = string.Empty;
            string[] elements = pSource.Split();
            foreach (string element in elements)
                dest += element + Cst.Space;

            return dest.TrimEnd();
        }


        ///	<summary>
        ///	Return a string containing hexadecimal value of char at position <c>plPosition</c> in string <c>psString</c>
        ///	</summary>
        ///	<param name="psString">String</param>
        ///	<param name="plPosition">Position of char</param>
        ///	<returns>String containing hexadecimal value of char</returns>
        public static string ToHexa(string psString, int plPosition)
        {
            char cTmp;
            if ((plPosition >= 0) && (plPosition < psString.Length))
            {
                cTmp = psString.ToCharArray()[plPosition];
                return String.Format("{0:x}", Convert.ToInt32(cTmp));
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// TOTO215 =>  TOTO
        /// </summary>
        /// <param name="pString"></param>
        /// <returns></returns>
        public static string PutOffSuffixNumeric(string pString)
        {
            string ret = pString;
            //PL 20120906 Afin d'éviter d'avoir un WARNING en compilation, utilisation de GetSuffixNumeric2() car GetSuffixNumeric() est obsolete.
            //int numSuffix = GetSuffixNumeric(pString);
            int numSuffix = GetSuffixNumeric2(pString);
            //
            if (-1 < numSuffix)
            {
                int lenSuffix = numSuffix.ToString().Length;
                ret = pString.Substring(0, pString.Length - lenSuffix);
            }
            //
            return ret;
        }

        

        /// <summary>
        /// Retourne la valeur numérique présente à la fin de la chaîne {pString}
        /// <para>Retourne -1, si aucune valeur numérique rencontrée</para>
        /// </summary>
        /// <param name="pString"></param>
        /// <returns></returns>
        public static int GetSuffixNumeric2(string pString)
        {
            int ret = -1;
            Regex re = new Regex(@"\d+$");
            if (re.IsMatch(pString))
                ret = Int32.Parse(re.Match(pString).Value);
            return ret;
        }

        /// <summary>
        /// Retourne la valeur numérique présente au début de la chaîne {pString}
        /// <para>Retourne -1, si aucune valeur numérique rencontrée</para>
        /// </summary>
        /// <param name="pString"></param>
        /// <returns></returns>
        public static int GetPrefixNumeric(string pString)
        {
            int ret = -1;
            Regex re = new Regex(@"^\d+");
            if (re.IsMatch(pString))
                ret = Int32.Parse(re.Match(pString).Value);
            return ret;
        }

        /// <summary>
        /// Obtient la lettre d'échéance mensuelle par interprétation de {pMM}
        /// <para>H pour 03 (Mars), M pour 06 (Juin).... </para>
        /// </summary>
        /// <param name="pMM">Représente un mois</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException : lorsque pData n'est pas interprétée"></exception>
        public static string GetMaturityLetter(string pMM)
        {
            string ret = "-";
            if (StrFunc.IsFilled(pMM))
            {
                switch (pMM.ToUpper())
                {
                    case "01":
                    case "JAN":
                    case "1":
                        ret = "F";
                        break;
                    case "02":
                    case "FEB":
                    case "2":
                        ret = "G";
                        break;
                    case "03":
                    case "MAR":
                    case "3":
                        ret = "H";
                        break;
                    case "04":
                    case "APR":
                    case "4":
                        ret = "J";
                        break;
                    case "05":
                    case "MAI":
                    case "5":
                        ret = "K";
                        break;
                    case "06":
                    case "JUN":
                    case "6":
                        ret = "M";
                        break;
                    case "07":
                    case "JUL":
                    case "7":
                        ret = "N";
                        break;
                    case "08":
                    case "AUG":
                    case "8":
                        ret = "Q";
                        break;
                    case "09":
                    case "SEP":
                    case "9":
                        ret = "U";
                        break;
                    case "OCT":
                    case "10":
                        ret = "V";
                        break;
                    case "NOV":
                    case "11":
                        ret = "X";
                        break;
                    case "DEC":
                    case "12":
                        ret = "Z";
                        break;

                    default:
                        throw new ArgumentException(StrFunc.AppendFormat("data {0} is unknown", pMM));
                }
            }
            return ret;
        }

        /// <summary>
        /// Obtient le mois sur 2 caractères numériques par interprétation de {pData}
        /// <para>01 pour janvier,02 pour Février, etc.... </para>
        /// </summary>
        /// <param name="pData">Représente un mois</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException : lorsque pData n'est pas interprétée"></exception>
        public static string GetMonthMM(string pData)
        {
            string ret = "XX";
            if (StrFunc.IsFilled(pData))
            {
                switch (pData.ToUpper())
                {
                    case "F":
                    case "JAN":
                    case "1":
                        ret = "01";
                        break;
                    case "G":
                    case "FEB":
                    case "2":
                        ret = "02";
                        break;
                    case "H":
                    case "MAR":
                    case "3":
                        ret = "03";
                        break;
                    case "J":
                    case "APR":
                    case "4":
                        ret = "04";
                        break;
                    case "K":
                    case "MAI":
                    case "5":
                        ret = "05";
                        break;
                    case "M":
                    case "JUN":
                    case "6":
                        ret = "06";
                        break;
                    case "N":
                    case "JUL":
                    case "7":
                        ret = "07";
                        break;
                    case "Q":
                    case "AUG":
                    case "8":
                        ret = "08";
                        break;
                    case "U":
                    case "SEP":
                    case "9":
                        ret = "09";
                        break;
                    case "V":
                    case "OCT":
                    case "10":
                        ret = "10";
                        break;
                    case "X":
                    case "NOV":
                    case "11":
                        ret = "11";
                        break;
                    case "Z":
                    case "DEC":
                    case "12":
                        ret = "12";
                        break;

                    default:
                        throw new ArgumentException(StrFunc.AppendFormat("data {0} is unknown", pData));
                }
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDay"></param>
        /// <returns></returns>
        public static string GetDayDD(string pDay)
        {
            string ret = "XX";
            //
            if (StrFunc.IsFilled(pDay))
            {
                if (pDay.Length == 1)
                    pDay = "0" + pDay;
                //
                if (pDay.Length == 2)
                    ret = pDay;
            }
            //
            return ret;
        }

        /// <summary>
        /// Remplissage à gauche avec des zéro sur la longueur {pPadding}
        /// </summary>
        /// <param name="pNumber">L'entier en question</param>
        /// <param name="pPadding">La longueur à atteindre</param>
        /// <returns></returns>
        public static string IntegerPadding(int pNumber, int pPadding)
        {
            return IntegerPadding(pNumber, pPadding, "0");
        }

        /// <summary>
        /// Remplissage à gauche avec {pPaddingSymbol} sur la longueur {pPadding}
        /// </summary>
        /// <param name="pNumber">L'entier en question</param>
        /// <param name="pPadding">La longueur à atteindre</param>
        /// <param name="pPaddingSymbol">Symbol de remplissage</param>
        /// <returns></returns>
        public static string IntegerPadding(int pNumber, int pPadding, string pPaddingSymbol)
        {
            string ret = pNumber.ToString();

            while (ret.Length < pPadding)
                ret = pPaddingSymbol + ret;

            return ret;
        }

        /// <summary>
        /// Remplissage à droite avec {pPaddingSymbol} sur la longueur {pPadding}
        /// </summary>
        /// <param name="pString">Le string en question</param>
        /// <param name="pPadding">La longueur à atteindre</param>
        /// <param name="pPaddingSymbol">Symbol de remplissage</param>
        /// <returns></returns>
        public static string Padding(string pString, int pPadding, string pPaddingSymbol)
        {
            string ret = pString;
            while (ret.Length < pPadding)
                ret += pPaddingSymbol;
            return ret;
        }


        #region public ScanRP_SPACE
        /// <summary>
        /// Return index of first occurence of "(" or " "
        /// </summary>
        /// <param name="psString">String</param>
        /// <returns>position de l'occurence</returns>
        public static int ScanRP_SPACE(string psString)
        {
            char[] cScan = (" )").ToCharArray();

            return psString.Trim().IndexOfAny(cScan);
        }
        #endregion public ScanRP_SPACE
        #region public StringToIntTime
        public static int[] StringToIntTime(string pValue)
        {
            int[] ret = new int[2];
            string[] timer = pValue.Split(":".ToCharArray());
            if (timer.GetLength(0) > 0)
            {
                ret[0] = Convert.ToInt32(timer[0].Trim());
                ret[1] = Convert.ToInt32(timer[1].Trim());
            }
            return ret;
        }
        #endregion public StringToIntTime
        #region public TrimStart_CrLfTabSpace
        /// <summary>
        /// Remove CR, Lf, Tab and Space at the beginning of chain
        /// </summary>
        /// <param name="pStr"></param>
        /// <returns></returns>
        public static string TrimStart_CrLfTabSpace(string pStr)
        {
            if ((pStr != null) && (pStr.Length > 0))
            {
                int guard = 0;
                char[] trimChar = new char[]
                    {
                        Convert.ToChar(Cst.Cr),
                        Convert.ToChar(Cst.Lf),
                        Convert.ToChar(Cst.Tab),
                        Convert.ToChar(" ")
                    };
                while (guard < 999)
                {
                    guard++;
                    string tmpStr = pStr.TrimStart(trimChar);
                    if (tmpStr == pStr)
                        break;
                    else
                        pStr = tmpStr;
                }
            }
            return pStr;
        }
        #endregion public TrimStart_CrLfTabSpace
        #region public TrimEnd_CrLfTabSpace
        /// <summary>
        /// Remove CR, Lf, Tab and Space at the ending of chain
        /// </summary>
        /// <param name="pStr"></param>
        /// <returns></returns>
        public static string TrimEnd_CrLfTabSpace(string pStr)
        {
            if ((pStr != null) && (pStr.Length > 0))
            {
                int guard = 0;
                char[] trimChar = new char[]
                    {
                        Convert.ToChar(Cst.Cr),
                        Convert.ToChar(Cst.Lf),
                        Convert.ToChar(Cst.Tab),
                        Convert.ToChar(" ")
                    };
                while (guard < 999)
                {
                    guard++;
                    string tmpStr = pStr.TrimEnd(trimChar);
                    if (tmpStr == pStr)
                        break;
                    else
                        pStr = tmpStr;
                }
            }
            return pStr;
        }
        #endregion public TrimEnd_CrLfTabSpace
        #region public Trim_CrLfTabSpace
        /// <summary>
        /// Remove CR, Lf, Tab and Space at the beginning and ending of chain
        /// </summary>
        /// <param name="pStr"></param>
        /// <returns></returns>
        public static string Trim_CrLfTabSpace(string pStr)
        {
            if ((pStr != null) && (pStr.Length > 0))
            {
                int guard = 0;
                char[] trimChar = new char[]
                    {
                        Convert.ToChar(Cst.Cr),
                        Convert.ToChar(Cst.Lf),
                        Convert.ToChar(Cst.Tab),
                        Convert.ToChar(" ")
                    };
                while (guard < 999)
                {
                    guard++;
                    string tmpStr = pStr.Trim(trimChar);
                    if (tmpStr == pStr)
                        break;
                    else
                        pStr = tmpStr;
                }
            }
            return pStr;
        }
        #endregion public Trim_CrLfTabSpace

        #region public QueryStringData
        /// <summary>
        /// Obsolete use StringArrayList a la place
        /// </summary>
        public sealed class QueryStringData
        {
            public const char LIST_SEPARATOR = ';';
            //
            public static string StringArrayToStringList(string[] pStringArray, bool pAddListSeparatorOnLastItem)
            {
                string retList = string.Empty;
                if (ArrFunc.IsFilled(pStringArray))
                {
                    for (int i = 0; i < pStringArray.Length; i++)
                    {
                        retList += pStringArray[i];
                        retList += LIST_SEPARATOR;
                    }
                }
                if (false == pAddListSeparatorOnLastItem)
                    retList = retList.Substring(0, retList.Length - LIST_SEPARATOR.ToString().Length);
                //
                return retList;
            }
            public static string StringArrayToStringList(string[] pStringArray)
            {
                return StringArrayToStringList(pStringArray, true);
            }

            public static string[] StringListToStringArray(string pList)
            {
                string[] retStringArray;
                if (pList.EndsWith(LIST_SEPARATOR.ToString()))
                    pList = pList.Substring(0, pList.Length - 1);
                retStringArray = pList.Split(LIST_SEPARATOR.ToString().ToCharArray());
                return retStringArray;
            }
        }
        #endregion StringArrayList

        #region public StringArrayList
        /// <summary>
        ///  Classe de convertion d'un type String en type array de string (string[]) et vis et versa 
        /// </summary>
        public sealed class StringArrayList
        {
            public const char LIST_SEPARATOR = ';';
            /// <summary>
            /// Transforme un array de string en une sinple string. Chaque élement de l'array initial est séparé par LIST_SEPARATOR
            /// </summary>
            /// <param name="pStringArray"></param>
            /// <param name="pAddListSeparatorOnLastItem"></param>
            /// <returns></returns>
            public static string StringArrayToStringList(string[] pStringArray, bool pAddListSeparatorOnLastItem)
            {
                string retList = string.Empty;
                if (ArrFunc.IsFilled(pStringArray))
                {
                    for (int i = 0; i < pStringArray.Length; i++)
                    {
                        retList += pStringArray[i];
                        retList += LIST_SEPARATOR;
                    }
                }
                if (false == pAddListSeparatorOnLastItem)
                    retList = retList.Substring(0, retList.Length - LIST_SEPARATOR.ToString().Length);
                //
                return retList;
            }
            /// <summary>
            /// Transforme un array de string en une sinple string. Chaque élement de l'array initial est séparé par LIST_SEPARATOR
            /// <para>la String résultat se termine par le séparateur LIST_SEPARATOR</para>
            /// </summary>
            /// <param name="pStringArray"></param>
            /// <param name="pAddListSeparatorOnLastItem"></param>
            /// <returns></returns>
            public static string StringArrayToStringList(string[] pStringArray)
            {
                return StringArrayToStringList(pStringArray, true);
            }
            /// <summary>
            /// Retourne string[] à partir d'une string.   
            /// </summary>
            /// <param name="pList"></param>
            /// <returns></returns>
            public static string[] StringListToStringArray(string pList)
            {
                string[] retStringArray;
                if (pList.EndsWith(LIST_SEPARATOR.ToString()))
                    pList = pList.Substring(0, pList.Length - 1);
                retStringArray = pList.Split(LIST_SEPARATOR.ToString().ToCharArray());
                return retStringArray;
            }
        }
        #endregion StringArrayList


        #region public GetProcessLogMessage
        /// <summary>
        /// Retourne {pErrorMsg} "OTCml Data:" {pOTCmlData}
        /// </summary>
        /// <param name="pErrorMsg"></param>
        /// <param name="pOTCmlData"></param>
        /// <returns></returns>
        public static string GetProcessLogMessage(string pErrorMsg, string pOTCmlData)
        {
            return GetProcessLogMessage(pErrorMsg, pOTCmlData, string.Empty);
        }
        /// <summary>
        /// Retourne {pErrorMsg}  "System Exception:" {pExceptionMsg}  "Spheres® Data:" {pOTCmlData}
        /// </summary>
        /// <param name="pErrorMsg"></param>
        /// <param name="pOTCmlData"></param>
        /// <param name="pExceptionMsg"></param>
        /// <returns></returns>
        public static string GetProcessLogMessage(string pErrorMsg, string pOTCmlData, string pExceptionMsg)
        {
            string logMessage = pErrorMsg;
            //
            if (IsFilled(pExceptionMsg))
            {
                if (IsFilled(logMessage))
                    logMessage += Cst.CrLf + Cst.CrLf;
                //			
                logMessage += "<b>System Exception:</b>" + Cst.CrLf + pExceptionMsg;
            }
            //
            if (IsFilled(pOTCmlData))
            {
                if (IsFilled(logMessage))
                    logMessage += Cst.CrLf + Cst.CrLf;
                //			
                logMessage += "<b>Spheres® Data:</b>" + Cst.CrLf + pOTCmlData;
            }
            //
            if (IsEmpty(logMessage))
                logMessage = "<b>Unknown error occurred</b>";
            //
            return logMessage;
        }
        #endregion GetProcessLogMessage

        #region public UTF8ByteArrayToString
        /// <summary>
        /// To convert a Byte Array of Unicode values (UTF-8 encoded) to a complete String.
        /// </summary>
        /// <param name="characters">Unicode Byte Array to be converted to String</param>
        /// <returns>String converted from Unicode Byte Array</returns>
        public static String UTF8ByteArrayToString(Byte[] characters)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            String constructedString = encoding.GetString(characters);
            return (constructedString);
        }
        #endregion
        #region public StringToUTF8ByteArray
        /// <summary>
        /// Converts the String to UTF8 Byte array and is used in De serialization
        /// </summary>
        /// <param name="pXmlString"></param>
        /// <returns></returns>
        public static Byte[] StringToUTF8ByteArray(String pString)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            Byte[] byteArray = encoding.GetBytes(pString);
            return byteArray;
        }
        #endregion

        /// <summary>
        /// Retrun the Enoding by Encoding name
        /// </summary>
        /// <param name="pStrEncodingName"></param>
        /// <returns>Encoding</returns>
        public static Encoding GetEncoding(string pStrEncodingName)
        {
            Encoding retEncoding = Encoding.Default;
            //
            foreach (EncodingInfo ei in Encoding.GetEncodings())
            {
                Encoding e = ei.GetEncoding();
                if (e.BodyName.ToLower() == pStrEncodingName.ToLower())
                {
                    retEncoding = e;
                    break;
                }
            }
            //
            return retEncoding;
        }


        /// <summary>
        /// Entoure la donnée en entrée avec 2 parentheses
        /// </summary>
        /// <param name="pString"></param>
        /// <returns></returns>
        public static string Frame(string pString)
        {
            return Frame(pString, "(");
        }
        /// <summary>
        /// Entoure la donnée en entrée avec des '( )' ou des '[]' ou des '{}' ou des ...  
        /// </summary>
        /// <param name="pString"></param>
        /// <param name="pBracket"></param>
        /// <returns></returns>
        public static string Frame(string pString, string pBracket)
        {
            switch (pBracket)
            {
                case "(":
                    pString = pBracket + pString + ")";
                    break;
                case "[":
                    pString = pBracket + pString + "]";
                    break;
                case "{":
                    pString = pBracket + pString + "}";
                    break;
                case "/*":
                    pString = pBracket + Cst.Space + pString + Cst.Space + "*/";
                    break;
                default:
                    pString = pBracket + pString + pBracket;
                    break;
            }
            return pString;
        }




        /// <summary>
        /// Return result of CompareTo with lengths of both strings.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int CompareLength(string a, string b)
        {
            if ((null == a) || (null == b))
                throw new ArgumentException("parameter null not allowed");

            return a.Length.CompareTo(b.Length);
        }


        /// <summary>
        /// Retourne un array avec les paramètres rattachés à la 1ère occurence du mot clef {pKeyWord}
        /// <para>Exemple de mot clef SESSIONRESTRICT => %%SR:MARKET_JOIN%%(t)</para>
        /// <para>Retourne un array avec 1 seul item qui contient "t"</para>
        /// </summary>
        ///<param name="pData"></param>
        ///<param name="pKeyWord"></param>
        /// <returns></returns>
        public static string[] GetArgumentKeyWord(string pData, string pKeyWord)
        {
            return GetArgumentKeyWord(pData, pKeyWord, "(", ")", ",");
        }

        /// <summary>
        /// Retourne un array avec les paramètres rattachés à la 1ère occurence du mot clef {pKeyWord}
        /// <para>Exemple de mot clef SESSIONRESTRICT => %%SR:MARKET_JOIN%%(t)</para>
        /// <para>Retourne un array avec 1 seul item qui contient "t"</para>
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="pKeyWord"></param>
        /// <param name="pArgumentStart"></param>
        /// <param name="pArgumentEnd"></param>
        /// <param name="pArgumentSeparator"></param>
        /// <returns></returns>
        public static string[] GetArgumentKeyWord(string pData, string pKeyWord,
            string pArgumentStart, string pArgumentEnd, string pArgumentSeparator)
        {
            string[] ret = null;
            //
            if (pData.Contains(pKeyWord))
            {
                if (pData.Substring(pData.IndexOf(pKeyWord) + pKeyWord.Length, pArgumentStart.Length) != pArgumentStart)
                    throw new Exception(StrFunc.AppendFormat("Method [{0}] {1} is expected", pKeyWord, "("));
                //
                // position de la parenthèse ouverte
                int openIndex = pData.IndexOf(pArgumentStart, pData.IndexOf(pKeyWord));
                if (openIndex == -1)
                    throw new Exception(StrFunc.AppendFormat("Method [{0}] without Argument", pKeyWord));
                //
                // position de la parenthère fermante
                int closeIndex = pData.IndexOf(pArgumentEnd, pData.IndexOf(pKeyWord));
                if (closeIndex == -1)
                    throw new Exception(StrFunc.AppendFormat("Method [{0}] {1} is expected", pKeyWord, ")"));
                //                
                string arg = pData.Substring(openIndex + 1, closeIndex - openIndex - 1);
                ret = arg.Split(pArgumentSeparator.ToCharArray());
            }
            return ret;
        }


        /// <summary>
        /// Get string value before [first] a.
        /// </summary>
        /// FI 20150513 [XXXXX] Add (code copié sur le net)
        public static string Before(string value, string a)
        {
            return Before(value, a, OccurenceEnum.First);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="a"></param>
        /// <param name="pMode"></param>
        /// <returns></returns>
        public static string Before(string value, string a, OccurenceEnum pMode)
        {

            int posA;
            if (pMode == OccurenceEnum.Last)
                posA = value.LastIndexOf(a);
            else if (pMode == OccurenceEnum.First)
                posA = value.IndexOf(a);
            else
                throw new NotImplementedException(StrFunc.AppendFormat("Mode:{0} is not implemented", pMode.ToString()));

            if (posA == -1)
            {
                return "";
            }

            return value.Substring(0, posA);
        }


        /// <summary>
        /// Get string value after [last] a.
        /// </summary>
        /// FI 20150915 [21315] Modify 
        public static string After(string value, string a)
        {
            // FI 20150915 [21315] call 
            return After(value, a, OccurenceEnum.Last);
        }

        /// <summary>
        /// Get string value after [last] a or after [first] a.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        /// FI 20150915 [21315] Add Method (code copié sur le net à la base)
        public static string After(string value, string a, OccurenceEnum pMode)
        {
            int posA;
            if (pMode == OccurenceEnum.Last)
                posA = value.LastIndexOf(a);
            else if (pMode == OccurenceEnum.First)
                posA = value.IndexOf(a);
            else
                throw new NotImplementedException(StrFunc.AppendFormat("Mode:{0} is not implemented", pMode.ToString()));

            if (posA == -1)
            {
                return "";
            }

            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= value.Length)
            {
                return "";
            }
            return value.Substring(adjustedPosA);
        }

        /// <summary>
        /// Get string between a and b
        /// </summary>
        /// <param name="value"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static string Between(string value, string a, string b)
        {
            int index1 = value.IndexOf(a);
            int index2 = value.LastIndexOf(b);
            if (index1 > -1 && index2 > -1)
            {
                return StrFunc.Before(StrFunc.After(value, a, OccurenceEnum.First), b, OccurenceEnum.Last);
            }
            else
                return "";
        }
    }
    #endregion
    #region public class StringData
    /// <summary>
    /// 
    /// </summary>
    /// 20081211 FI [16421] add interface IComparable
    // EG 20180425 Analyse du code Correction [CA1033]
    public sealed class StringData : IComparable
    {
        #region Members
        [System.Xml.Serialization.XmlTextAttribute()]
        public string value;
        //
        [System.Xml.Serialization.XmlAttribute()]
        public string name;
        //
        [System.Xml.Serialization.XmlAttribute()]
        public string datatype;
        //
        [System.Xml.Serialization.XmlAttribute()]
        public string dataformat;
        #endregion Members

        #region accessor
        [System.Xml.Serialization.XmlIgnore()]
        public TypeData.TypeDataEnum DatatypeEnum
        {
            get
            {
                return TypeData.GetTypeDataEnum(datatype);
            }
        }
        #endregion

        #region Constructor
        public StringData() { }
        public StringData(string pName, string pType, string pValue, string pFormat)
        {
            name = pName;
            datatype = pType;
            value = pValue;
            dataformat = pFormat;
        }
        public StringData(string pName, TypeData.TypeDataEnum pType, string pValue, string pFormat)
        {
            name = pName;
            datatype = pType.ToString();
            value = pValue;
            dataformat = pFormat;
        }
        #endregion Constructor

        #region IComparable Membres
        int IComparable.CompareTo(object pObj)
        {
            int ret = -1;
            StringData obj = pObj as StringData;
            if (null != obj)
            {
                if (obj.name == this.name)
                    ret = 0;
            }
            if (pObj is String str)
            {
                if (str == this.name)
                    ret = 0;
            }
            else
                throw new NotImplementedException(obj.GetType().ToString() + " not NotImplemented");
            return ret;
        }
        #endregion
    }
    #endregion StringData
    //
    #region public class StrBuilder
    public class StrBuilder
    {
        readonly StringBuilder sb;

        #region Accessors
        #region Capacity
        public int Capacity { get { return sb.Capacity; } }
        #endregion Capacity
        #region Length
        public int Length { get { return sb.Length; } }
        #endregion Length
        #region MaxCapacity
        public int MaxCapacity { get { return sb.MaxCapacity; } }
        #endregion MaxCapacity
        #region StringBuilder
        public StringBuilder StringBuilder { get { return sb; } }
        #endregion StringBuilder
        #endregion Accessors
        #region Constructors
        public StrBuilder() { sb = new StringBuilder(); }
        public StrBuilder(string pText) { sb = new StringBuilder(pText); }
        public StrBuilder(int pCapacity) { sb = new StringBuilder(pCapacity); }
        public StrBuilder(int pCapacity, int pMaxCapacity) { sb = new StringBuilder(pCapacity, pMaxCapacity); }
        public StrBuilder(string pText, int pStartIndex, int pLength, int pCapacity) { sb = new StringBuilder(pText, pStartIndex, pLength, pCapacity); }
        public StrBuilder(string pText, int pCapacity) { sb = new StringBuilder(pText, pCapacity); }
        #endregion Constructors

        #region Methods
        #region Operators +
        public static StrBuilder operator +(StrBuilder pStrBuilder, string pAddText) { return pStrBuilder.Append(pAddText); }
        public static StrBuilder operator +(StrBuilder pStrBuilder, object pAddText) { return pStrBuilder.Append(pAddText); }
        public static StrBuilder operator +(StrBuilder pStrBuilder, ulong pAddText) { return pStrBuilder.Append(pAddText); }
        public static StrBuilder operator +(StrBuilder pStrBuilder, uint pAddText) { return pStrBuilder.Append(pAddText); }
        public static StrBuilder operator +(StrBuilder pStrBuilder, ushort pAddText) { return pStrBuilder.Append(pAddText); }
        public static StrBuilder operator +(StrBuilder pStrBuilder, decimal pAddText) { return pStrBuilder.Append(pAddText); }
        public static StrBuilder operator +(StrBuilder pStrBuilder, double pAddText) { return pStrBuilder.Append(pAddText); }
        public static StrBuilder operator +(StrBuilder pStrBuilder, float pAddText) { return pStrBuilder.Append(pAddText); }
        public static StrBuilder operator +(StrBuilder pStrBuilder, long pAddText) { return pStrBuilder.Append(pAddText); }
        public static StrBuilder operator +(StrBuilder pStrBuilder, int pAddText) { return pStrBuilder.Append(pAddText); }
        public static StrBuilder operator +(StrBuilder pStrBuilder, short pAddText) { return pStrBuilder.Append(pAddText); }
        public static StrBuilder operator +(StrBuilder pStrBuilder, char pAddText) { return pStrBuilder.Append(pAddText); }
        public static StrBuilder operator +(StrBuilder pStrBuilder, byte pAddText) { return pStrBuilder.Append(pAddText); }
        public static StrBuilder operator +(StrBuilder pStrBuilder, sbyte pAddText) { return pStrBuilder.Append(pAddText); }
        public static StrBuilder operator +(StrBuilder pStrBuilder, bool pAddText) { return pStrBuilder.Append(pAddText); }
        #endregion Operators +
        #region Append
        public StrBuilder Append(char[] pValue)
        {
            sb.Append(pValue);
            return this;
        }
        public StrBuilder Append(object pValue)
        {
            sb.Append(pValue);
            return this;
        }
        public StrBuilder Append(ulong pValue)
        {
            sb.Append(pValue);
            return this;
        }
        public StrBuilder Append(uint pValue)
        {
            sb.Append(pValue);
            return this;
        }
        public StrBuilder Append(ushort pValue)
        {
            sb.Append(pValue);
            return this;
        }
        public StrBuilder Append(decimal pValue)
        {
            sb.Append(pValue);
            return this;
        }
        public StrBuilder Append(double pValue)
        {
            sb.Append(pValue);
            return this;
        }
        public StrBuilder Append(float pValue)
        {
            sb.Append(pValue);
            return this;
        }
        public StrBuilder Append(long pValue)
        {
            sb.Append(pValue);
            return this;
        }
        public StrBuilder Append(int pValue)
        {
            sb.Append(pValue);
            return this;
        }
        public StrBuilder Append(short pValue)
        {
            sb.Append(pValue);
            return this;
        }
        public StrBuilder Append(char pValue)
        {
            sb.Append(pValue);
            return this;
        }
        public StrBuilder Append(byte pValue)
        {
            sb.Append(pValue);
            return this;
        }
        public StrBuilder Append(sbyte pValue)
        {
            sb.Append(pValue);
            return this;
        }
        public StrBuilder Append(bool pValue)
        {
            sb.Append(pValue);
            return this;
        }
        public StrBuilder Append(string pValue, int pStartIndex, int pCount)
        {
            sb.Append(pValue, pStartIndex, pCount);
            return this;
        }
        public StrBuilder Append(string pValue)
        {
            sb.Append(pValue);
            return this;
        }
        public StrBuilder Append(char[] pValue, int pStartIndex, int pCount)
        {
            sb.Append(pValue, pStartIndex, pCount);
            return this;
        }
        public StrBuilder Append(char pValue, int pRepeatCount)
        {
            sb.Append(pValue, pRepeatCount);
            return this;
        }
        #endregion Append
        #region AppendFormat
        public StrBuilder AppendFormat(string pFormat, params object[] pArgs)
        {
            sb.AppendFormat(pFormat, pArgs);
            return this;
        }
        public StrBuilder AppendFormat(string pFormat, object pArg0)
        {
            sb.AppendFormat(pFormat, pArg0);
            return this;
        }
        public StrBuilder AppendFormat(string pFormat, object pArg0, object pArg1)
        {
            sb.AppendFormat(pFormat, pArg0, pArg1);
            return this;
        }
        public StrBuilder AppendFormat(string pFormat, object pArg0, object pArg1, object pArg2)
        {
            sb.AppendFormat(pFormat, pArg0, pArg1, pArg2);
            return this;
        }
        public StrBuilder AppendFormat(IFormatProvider pProvider, string pFormat, params object[] pArgs)
        {
            sb.AppendFormat(pProvider, pFormat, pArgs);
            return this;
        }
        #endregion AppendFormat
        #region EnsureCapacity
        public int EnsureCapacity(int pCapacity) { return sb.EnsureCapacity(pCapacity); }
        #endregion EnsureCapacity
        #region Equals
        public bool Equals(StrBuilder pStrBuilder) { return sb.Equals(pStrBuilder.StringBuilder); }
        #endregion Equals
        #region ToString()
        public override string ToString() { return sb.ToString(); }
        public string ToString(int pStartIndex, int pLength) { return sb.ToString(pStartIndex, pLength); }
        #endregion ToString()
        #endregion Methods
    }
    #endregion class StrBuilder
    //
    #region public class TypeData
    // EG 20170918 [23342] Add datetimeoffset, timeoffset,
    public class TypeData
    {
        #region public enum TypeDataEnum
        public enum TypeDataEnum
        {
            //PL 20100914 Refactoring
            @bool, boolean, bool2v, bool2h,
            date,
            datetime,
            datetimeoffset,
            time,
            timeoffset,
            integer, @int,
            @decimal, dec,
            @string,
            text,
            image,
            cursor,
            xml,
            unknown
        }
        #endregion public enum TypeDataEnum
        #region public GetTypeDataEnum
        public static TypeDataEnum GetTypeDataEnum(string pType)
        {
            return GetTypeDataEnum(pType, false);
        }
        public static TypeDataEnum GetTypeDataEnum(string pType, bool pWithExceptionWhenError)
        {
            TypeDataEnum ret = TypeDataEnum.unknown;
            //
            if (Enum.IsDefined(typeof(TypeDataEnum), pType))
                ret = (TypeDataEnum)Enum.Parse(typeof(TypeDataEnum), pType, true);
            else if (Enum.IsDefined(typeof(TypeDataEnum), pType.ToLower()))
                ret = (TypeDataEnum)Enum.Parse(typeof(TypeDataEnum), pType, true);
            else
            {
                if (StrFunc.IsFilled(pType))
                {
                    string pTypeToLower = pType.ToLower();
                    if ("dec" == pTypeToLower)
                        ret = TypeDataEnum.@decimal;
                    else if ("curs" == pTypeToLower)
                        ret = TypeDataEnum.cursor;
                    else if ("int" == pTypeToLower)
                        ret = TypeDataEnum.@integer;
                    else if (pTypeToLower.StartsWith("bool"))
                        ret = TypeDataEnum.@bool;
                    else if ("xmltype" == pTypeToLower)
                        ret = TypeDataEnum.xml;
                    else if ("string" == pTypeToLower)
                        ret = TypeDataEnum.@string;
                }
            }
            //
            if ((ret == TypeDataEnum.unknown) && pWithExceptionWhenError)
                throw new Exception(StrFunc.AppendFormat("{0} is unknown", pType));
            //    
            return ret;
        }
        #endregion public GetTypeDataEnum
        #region public IsTypeXXXX
        //From Enum
        public static bool IsTypeCursor(TypeDataEnum pType)
        { return (pType == TypeDataEnum.cursor); }
        public static bool IsTypeBool(TypeDataEnum pType)
        { return (pType == TypeDataEnum.@bool) || (pType == TypeDataEnum.boolean) || (pType == TypeDataEnum.bool2v) || (pType == TypeDataEnum.bool2h); }
        public static bool IsTypeDate(TypeDataEnum pType)
        { return (pType == TypeDataEnum.date); }
        // EG 20170918 [23342] New 
        public static bool IsTypeDateTimeOffset(TypeDataEnum pType)
        { return (pType == TypeDataEnum.datetimeoffset); }
        public static bool IsTypeDateTime(TypeDataEnum pType)
        { return (pType == TypeDataEnum.datetime); }
        public static bool IsTypeTime(TypeDataEnum pType)
        { return (pType == TypeDataEnum.time); }
        // EG 20170918 [22374] New 
        public static bool IsTypeTimeOffset(TypeDataEnum pType)
        { return (pType == TypeDataEnum.timeoffset); }
        public static bool IsTypeString(TypeDataEnum pType)
        { return (pType == TypeDataEnum.@string); }
        public static bool IsTypeInt(TypeDataEnum pType)
        {
            return (pType == TypeDataEnum.@integer) || (pType == TypeDataEnum.@int);
        }
        public static bool IsTypeDec(TypeDataEnum pType)
        {
            return (pType == TypeDataEnum.@decimal) || (pType == TypeDataEnum.dec);
        }
        public static bool IsTypeText(TypeDataEnum pType)
        { return (pType == TypeDataEnum.@text); }
        public static bool IsTypeXml(TypeDataEnum pType)
        { return (pType == TypeDataEnum.xml); }
        public static bool IsTypeImage(TypeDataEnum pType)
        { return (pType == TypeDataEnum.image); }
        public static bool IsTypeUnknown(TypeDataEnum pType)
        { return (pType == TypeDataEnum.unknown); }
        //
        public static bool IsTypeNumeric(TypeDataEnum pType)
        { return (IsTypeDec(pType) || IsTypeInt(pType)); }
        public static bool IsTypeDateOrDateTime(TypeDataEnum pType)
        { return (IsTypeDate(pType) || IsTypeDateTime(pType)); }

        //From String
        public static bool IsTypeCursor(string pType)
        { return (GetTypeDataEnum(pType) == TypeDataEnum.cursor); }
        public static bool IsTypeBool(string pType)
        {
            //PL 20100914 Refactoring
            TypeData.TypeDataEnum type = GetTypeDataEnum(pType);
            return (type == TypeDataEnum.@bool || type == TypeDataEnum.bool2v || type == TypeDataEnum.bool2h || type == TypeDataEnum.boolean);
        }
        public static bool IsTypeDate(string pType)
        { return (GetTypeDataEnum(pType) == TypeDataEnum.date); }
        public static bool IsTypeDateTime(string pType)
        { return (GetTypeDataEnum(pType) == TypeDataEnum.datetime); }
        public static bool IsTypeTime(string pType)
        { return (GetTypeDataEnum(pType) == TypeDataEnum.time); }
        // EG 20170918 [23342] New 
        public static bool IsTypeDateTimeOffset(string pType)
        { return (GetTypeDataEnum(pType) == TypeDataEnum.datetimeoffset); }
        // EG 20170918 [22374] New 
        public static bool IsTypeTimeOffset(string pType)
        { return (GetTypeDataEnum(pType) == TypeDataEnum.timeoffset); }
        public static bool IsTypeString(string pType)
        { return (GetTypeDataEnum(pType) == TypeDataEnum.@string); }
        public static bool IsTypeInt(string pType)
        {
            //PL 20100914 Refactoring
            TypeData.TypeDataEnum type = GetTypeDataEnum(pType);
            return (type == TypeDataEnum.@integer || type == TypeDataEnum.@int);
        }
        public static bool IsTypeDec(string pType)
        {
            //PL 20100914 Refactoring
            TypeData.TypeDataEnum type = GetTypeDataEnum(pType);
            return (type == TypeDataEnum.@decimal || type == TypeDataEnum.dec);
        }
        public static bool IsTypeText(string pType)
        { return (GetTypeDataEnum(pType) == TypeDataEnum.@text); }
        public static bool IsTypeXml(string pType)
        { return (GetTypeDataEnum(pType) == TypeDataEnum.xml); }
        public static bool IsTypeImage(string pType)
        { return (GetTypeDataEnum(pType) == TypeDataEnum.image); }
        public static bool IsTypeUnknown(string pType)
        { return (GetTypeDataEnum(pType) == TypeDataEnum.unknown); }
        //
        public static bool IsTypeNumeric(string pType)
        { return (IsTypeDec(pType) || IsTypeInt(pType)); }
        public static bool IsTypeDateOrDateTime(string pType)
        { return (IsTypeDate(pType) || IsTypeDateTime(pType)); }

        #endregion
        #region public GetTypeFromSystemType
        public static TypeDataEnum GetTypeFromSystemType(Type pType)
        {
            TypeDataEnum ret = TypeDataEnum.unknown;
            //
            if (pType.Equals(typeof(System.Boolean)))
                ret = TypeDataEnum.@bool;
            else if (pType.Equals(typeof(System.Char)))
                ret = TypeDataEnum.@string;
            else if (pType.Equals(typeof(System.DateTime)))
                ret = TypeDataEnum.@datetime;
            else if (pType.Equals(typeof(System.Decimal)))
                ret = TypeDataEnum.@decimal;
            else if (pType.Equals(typeof(System.Double)))
                ret = TypeDataEnum.@decimal;
            else if ((pType.Equals(typeof(System.Int16))) ||
                (pType.Equals(typeof(System.Int32))) ||
                (pType.Equals(typeof(System.Int64)))
                )
                ret = TypeDataEnum.integer;
            else if (pType.Equals(typeof(System.Single)))
                ret = TypeDataEnum.integer;
            else if (pType.Equals(typeof(System.String)))
                ret = TypeDataEnum.@string;
            else if (pType.FullName.Equals(DbType.Xml))
                ret = TypeDataEnum.@string;
            else if ((pType.Equals(typeof(System.UInt16))) ||
                (pType.Equals(typeof(System.UInt32))) ||
                (pType.Equals(typeof(System.UInt64)))
                )
                ret = TypeDataEnum.integer;
            // RD 20190918 [24948] Add 
            else if ((pType.BaseType.Equals(typeof(System.Enum))))
                ret = TypeDataEnum.@string;

            return ret;
        }
        #endregion
    }
    #endregion class TypeData

    #region public class XSDTypeData
    /// EG 20161122 New Commodity Derivative

    public class XSDTypeData
    {
        public const string Bool = "boolean";
        public const string Date = "date";
        public const string DateTime = "dateTime";
        public const string Time = "time";
        public const string String = "string";
        public const string Integer = "integer";
        public const string NegativeInteger = "negativeInteger";
        public const string NonNegativeInteger = "nonNegativeInteger";
        public const string NonNegativeDecimal = "nonNegativeDecimal";
        public const string PositiveDecimal = "positiveDecimal";
        public const string PosInteger = "posInteger";
        public const string Decimal = "decimal";
    }
    #endregion class XSDTypeData
    //
    #region publci class TradeActionMode
    /// <summary>TradeAction mode list and properties</summary>
    /// EG 20150429 [20513] Ajout CalculCashSettlementOrBondPaymentAmount
    public sealed class TradeActionMode
    {
        #region TradeActionModeEnum
        /// <summary>Trade action mode list</summary>
        public enum TradeActionModeEnum
        {
            ///<summary>To leave the edition mode without safeguarding</summary>
            Annul,
            ///<summary>To active or deactive barrier/Trigger</summary>
            ActiveDeactiveBarrierTrigger,
            ///<summary>Calculation of the cash settlement in the money amount</summary>
            CalculCashSettlementMoneyAmount,
            ///<summary>Calculation of the Call/Put Currency amount of exercise</summary>
            CalculCurrencyAmountExercise,
            ///<summary>Calculation of the Cash Settlement Or BondPayment amount</summary>
            CalculCashSettlementOrBondPaymentAmount,
            ///<summary>Calculation of the Notional amount/Fee/CashSettlement provision of exercise (CANCELABLE/EXTENDIBLE...)</summary>
            CalculProvisionExercise,
            ///<summary>Calculation of the amount of payout</summary>
            CalculPayout,
            ///<summary>Close mode</summary>
            Close,
            ///<summary>Consult mode</summary>
            Consult,
            ///<summary>Edit mode</summary>
            Edit,
            ///<summary>Edit mode after validation rules</summary>
            EditErrorValidationRules,
            ///<summary>Format</summary>
            FormatControl,
            ///<summary>None</summary>
            None,
            ///<summary>Return to the original version (last loading)</summary>
            Release,
            ///<summary>Reload the last version</summary>
            Refresh,
            ///<summary>Save the modification</summary>
            Save,
            ///<summary>Search a FxRate quotation</summary>
            SearchQuote_FxRate,
            ///<summary>Synchronize 2 dropdown list Payer and receiver</summary>
            SynchronizePayerReceiver,
            ///<summary>Final validation of all the modifications</summary>
            Validate,

        }
        #endregion TradeActionModeEnum
        #region Methods
        #region TradeActionMode
        #region IsAnnul
        public static bool IsAnnul(string pTradeActionMode) { return IsAnnul(SetTradeActionMode(pTradeActionMode)); }
        public static bool IsAnnul(TradeActionModeEnum pTradeActionMode) { return (TradeActionModeEnum.Annul == pTradeActionMode); }
        #endregion IsAnnul
        #region IsActiveDeactiveBarrierTrigger
        public static bool IsActiveDeactiveBarrierTrigger(string pTradeActionMode) { return IsActiveDeactiveBarrierTrigger(SetTradeActionMode(pTradeActionMode)); }
        public static bool IsActiveDeactiveBarrierTrigger(TradeActionModeEnum pTradeActionMode) { return (TradeActionModeEnum.ActiveDeactiveBarrierTrigger == pTradeActionMode); }
        #endregion IsActiveDeactiveBarrierTrigger
        #region IsCalcul
        // EG 20150429 [20513] New IsCalculNbOptionsAndNotionalAmount
        public static bool IsCalcul(string pTradeActionMode) { return IsCalcul(SetTradeActionMode(pTradeActionMode)); }
        public static bool IsCalcul(TradeActionModeEnum pTradeActionMode)
        {
            return (IsActiveDeactiveBarrierTrigger(pTradeActionMode) || IsCalculCurrencyAmountExercise(pTradeActionMode) ||
                    IsCalculCashSettlementMoneyAmount(pTradeActionMode) || IsSearchQuote_FxRate(pTradeActionMode) ||
                    IsCalculPayout(pTradeActionMode) || IsCalculProvisionExercise(pTradeActionMode) ||
                    IsFormatControl(pTradeActionMode) || IsSynchronizePayerReceiver(pTradeActionMode) ||
                    IsCalculCashSettlementOrBondPaymentAmount(pTradeActionMode));
        }
        #endregion IsCalcul
        #region IsCalculCashSettlementMoneyAmount
        public static bool IsCalculCashSettlementMoneyAmount(string pTradeActionMode)
        {
            return IsCalculCashSettlementMoneyAmount(SetTradeActionMode(pTradeActionMode));
        }
        public static bool IsCalculCashSettlementMoneyAmount(TradeActionModeEnum pTradeActionMode)
        {
            return (TradeActionModeEnum.CalculCashSettlementMoneyAmount == pTradeActionMode);
        }
        #endregion IsCalculCashSettlementMoneyAmount
        #region IsCalculCurrencyAmountExercise
        public static bool IsCalculCurrencyAmountExercise(string pTradeActionMode)
        {
            return IsCalculCurrencyAmountExercise(SetTradeActionMode(pTradeActionMode));
        }
        public static bool IsCalculCurrencyAmountExercise(TradeActionModeEnum pTradeActionMode)
        {
            return (TradeActionModeEnum.CalculCurrencyAmountExercise == pTradeActionMode);
        }
        #endregion IsCalculCurrencyAmountExercise
        #region IsCalculCashSettlementOrBondPaymentAmount
        // EG 20150429 (20513] New
        public static bool IsCalculCashSettlementOrBondPaymentAmount(string pTradeActionMode)
        {
            return IsCalculCashSettlementOrBondPaymentAmount(SetTradeActionMode(pTradeActionMode));
        }
        public static bool IsCalculCashSettlementOrBondPaymentAmount(TradeActionModeEnum pTradeActionMode)
        {
            return (TradeActionModeEnum.CalculCashSettlementOrBondPaymentAmount == pTradeActionMode);
        }
        #endregion IsCalculCashSettlementOrBondPaymentAmount

        #region IsCalculProvisionExercise
        public static bool IsCalculProvisionExercise(string pTradeActionMode)
        {
            return IsCalculProvisionExercise(SetTradeActionMode(pTradeActionMode));
        }
        public static bool IsCalculProvisionExercise(TradeActionModeEnum pTradeActionMode)
        {
            return (TradeActionModeEnum.CalculProvisionExercise == pTradeActionMode);
        }
        #endregion IsCalculProvisionExercise
        #region IsCalculPayout
        public static bool IsCalculPayout(string pTradeActionMode)
        {
            return IsCalculPayout(SetTradeActionMode(pTradeActionMode));
        }
        public static bool IsCalculPayout(TradeActionModeEnum pTradeActionMode)
        {
            return (TradeActionModeEnum.CalculPayout == pTradeActionMode);
        }
        #endregion IsCalculPayout
        #region IsConsult
        public static bool IsConsult(string pTradeActionMode)
        {
            return IsConsult(SetTradeActionMode(pTradeActionMode));
        }
        public static bool IsConsult(TradeActionModeEnum pTradeActionMode)
        {
            return (TradeActionModeEnum.Consult == pTradeActionMode);
        }
        #endregion IsConsult
        #region IsClose
        public static bool IsClose(string pTradeActionMode)
        {
            return IsClose(SetTradeActionMode(pTradeActionMode));
        }
        public static bool IsClose(TradeActionModeEnum pTradeActionMode)
        {
            return (TradeActionModeEnum.Close == pTradeActionMode);
        }
        #endregion IsClose
        #region IsEdit
        public static bool IsEdit(string pTradeActionMode)
        {
            return IsEdit(SetTradeActionMode(pTradeActionMode));
        }
        public static bool IsEdit(TradeActionModeEnum pTradeActionMode)
        {
            return (TradeActionModeEnum.Edit == pTradeActionMode);
        }
        #endregion IsEdit
        #region IsEditErrorValidationRules
        public static bool IsEditErrorValidationRules(string pTradeActionMode)
        {
            return IsEditErrorValidationRules(SetTradeActionMode(pTradeActionMode));
        }
        public static bool IsEditErrorValidationRules(TradeActionModeEnum pTradeActionMode)
        {
            return (TradeActionModeEnum.EditErrorValidationRules == pTradeActionMode);
        }
        #endregion IsEditErrorValidationRules
        #region IsEditPlus
        public static bool IsEditPlus(string pTradeActionMode)
        {
            return IsEditPlus(SetTradeActionMode(pTradeActionMode));
        }
        public static bool IsEditPlus(TradeActionModeEnum pTradeActionMode)
        {
            return (IsEdit(pTradeActionMode) || IsEditErrorValidationRules(pTradeActionMode) || IsCalcul(pTradeActionMode));
        }
        #endregion IsEditPlus
        #region IsFormatControl
        public static bool IsFormatControl(string pTradeActionMode) { return IsFormatControl(SetTradeActionMode(pTradeActionMode)); }
        public static bool IsFormatControl(TradeActionModeEnum pTradeActionMode) { return (TradeActionModeEnum.FormatControl == pTradeActionMode); }
        #endregion IsFormatControl

        #region IsIdSpecified
        public static bool IsIdSpecified(string pTradeActionMode)
        {
            return IsIdSpecified(SetTradeActionMode(pTradeActionMode));
        }
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        public static bool IsIdSpecified(TradeActionModeEnum pTradeActionMode)
        {
            return (false == IsNone(pTradeActionMode) && false == IsCalcul(pTradeActionMode) && false == IsToolbarAction(pTradeActionMode));
        }
        #endregion IsIdSpecified
        #region IsNone
        public static bool IsNone(TradeActionModeEnum pTradeActionMode) { return (TradeActionModeEnum.None == pTradeActionMode); }
        #endregion IsNone
        #region IsRefresh
        public static bool IsRefresh(TradeActionModeEnum pTradeActionMode) { return (TradeActionModeEnum.Refresh == pTradeActionMode); }
        #endregion IsRefresh
        #region IsRelease
        public static bool IsRelease(string pTradeActionMode)
        {
            return IsRelease(SetTradeActionMode(pTradeActionMode));
        }
        public static bool IsRelease(TradeActionModeEnum pTradeActionMode)
        {
            return (TradeActionModeEnum.Release == pTradeActionMode);
        }
        #endregion IsRelease
        #region IsSave
        public static bool IsSave(string pTradeActionMode)
        {
            return IsSave(SetTradeActionMode(pTradeActionMode));
        }
        public static bool IsSave(TradeActionModeEnum pTradeActionMode)
        {
            return (TradeActionModeEnum.Save == pTradeActionMode);
        }
        #endregion IsSave
        #region IsSearchQuote_FxRate
        public static bool IsSearchQuote_FxRate(string pTradeActionMode)
        {
            return IsSearchQuote_FxRate(SetTradeActionMode(pTradeActionMode));
        }
        public static bool IsSearchQuote_FxRate(TradeActionModeEnum pTradeActionMode)
        {
            return (TradeActionModeEnum.SearchQuote_FxRate == pTradeActionMode);
        }
        #endregion IsSearchQuote_FxRate
        #region IsSynchronizePayerReceiver
        public static bool IsSynchronizePayerReceiver(string pTradeActionMode)
        {
            return IsSynchronizePayerReceiver(SetTradeActionMode(pTradeActionMode));
        }
        public static bool IsSynchronizePayerReceiver(TradeActionModeEnum pTradeActionMode)
        {
            return (TradeActionModeEnum.SynchronizePayerReceiver == pTradeActionMode);
        }
        #endregion IsSynchronizePayerReceiver

        #region IsValidate
        public static bool IsValidate(string pTradeActionMode)
        {
            return IsValidate(SetTradeActionMode(pTradeActionMode));
        }
        public static bool IsValidate(TradeActionModeEnum pTradeActionMode)
        {
            return (TradeActionModeEnum.Validate == pTradeActionMode);
        }
        #endregion IsValidate
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        #region IsToolbarAction
        public static bool IsToolbarAction(TradeActionModeEnum pTradeActionMode)
        {
            return IsRefresh(pTradeActionMode) || IsValidate(pTradeActionMode) || IsSave(pTradeActionMode) || IsRelease(pTradeActionMode) || IsAnnul(pTradeActionMode);
        }
        #endregion IsValidate

        #region SetTradeActionMode
        public static TradeActionModeEnum SetTradeActionMode(string pTradeActionMode)
        {
            if (System.Enum.IsDefined(typeof(TradeActionModeEnum), pTradeActionMode))
                return (TradeActionModeEnum)System.Enum.Parse(typeof(TradeActionModeEnum), pTradeActionMode, true);
            return TradeActionModeEnum.None;
        }
        #endregion SetVaMode
        #endregion TradeActionMode
        #endregion Methods
    }
    #endregion class TradeActionMode
    #region public class TradeActionType
    /// <summary>Trade action type list and properties</summary>
    public sealed class TradeActionType
    {
        #region TradeActionTypeEnum
        /// <summary>Trade action mode list</summary>
        public enum TradeActionTypeEnum
        {
            ///<summary>Abandon</summary>
            AbandonEvents,
            //CC 20110204
            ///<summary>Assignment</summary>
            AssignmentEvents,
            ///<summary>Barriers and triggers events action</summary>
            BarrierAndTriggerEvents,
            ///<summary>Remove trade</summary>
            RemoveTradeEvents,
            ///<summary>Customer Settlement Rate</summary>
            //PL 20100628
            //CustomerSettlementRateEvents,
            CalculationAgentSettlementRateEvents,
            ///<summary>Exercise</summary>
            ExerciseEvents,
            ///<summary>CancelableProvision</summary>
            CancelableProvisionEvents,
            ///<summary>ExtendibleProvision</summary>
            ExtendibleProvisionEvents,
            ///<summary>OptionalEarlyTerminationProvision</summary>
            OptionalEarlyTerminationProvisionEvents,
            ///<summary>MandatoryEarlyTerminationProvision</summary>
            MandatoryEarlyTerminationProvisionEvents,
            //CC 20110204
            ///<summary>QuantityCorrection</summary>
            PositionCancelationEvents,
            ///<summary>StepUpProvision</summary>
            StepUpProvisionEvents,
            //CC 20110215
            ///<summary>PositionsTransfer</summary>
            PositionTransferEvents,
            ///<summary>Splitting</summary>
            TradeSplitting,
            ///<summary>ClearingSpecific</summary>
            ClearingSpecificEvents,
            ///<summary>RemoveAllocation</summary>
            RemoveAllocationTradeEvents,
            ///<summary>Invoicing (Additional or CreditNote)</summary>
            InvoicingCorrectionEvents,
            ///<summary>Underlying Delivery</summary>
            // PM 20130822 [17949] Livraison Matif
            UnderlyingDeliveryEvents,
            ///<summary>None</summary>
            None,
        }
        #endregion VaTypeEnum

        #region public IsAbandonEvents
        public static bool IsAbandonEvents(string pTradeActionType)
        {
            return IsAbandonEvents(GetTradeActionTypeEnum(pTradeActionType));
        }
        public static bool IsAbandonEvents(TradeActionTypeEnum pTradeActionType)
        {
            return (TradeActionTypeEnum.AbandonEvents == pTradeActionType);
        }
        #endregion IsAbandonEvents
        //CC 20110204
        #region public IsAssignmentEvents
        public static bool IsAssignmentEvents(string pTradeActionType)
        {
            return IsAssignmentEvents(GetTradeActionTypeEnum(pTradeActionType));
        }
        public static bool IsAssignmentEvents(TradeActionTypeEnum pTradeActionType)
        {
            return (TradeActionTypeEnum.AssignmentEvents == pTradeActionType);
        }
        #endregion IsAssignmentEvents
        #region public IsBarrierAndTriggerEvents
        public static bool IsBarrierAndTriggerEvents(string pTradeActionType)
        {
            return IsBarrierAndTriggerEvents(GetTradeActionTypeEnum(pTradeActionType));
        }
        public static bool IsBarrierAndTriggerEvents(TradeActionTypeEnum pTradeActionType)
        {
            return (TradeActionTypeEnum.BarrierAndTriggerEvents == pTradeActionType);
        }
        #endregion IsBarrierAndTriggerEvents
        #region public IsRemoveTradeEvents
        public static bool IsRemoveTradeEvents(string pTradeActionType)
        {
            return IsRemoveTradeEvents(GetTradeActionTypeEnum(pTradeActionType));
        }
        public static bool IsRemoveTradeEvents(TradeActionTypeEnum pTradeActionType)
        {
            return (TradeActionTypeEnum.RemoveTradeEvents == pTradeActionType);
        }
        #endregion IsRemoveTradeEvents
        #region public IsCancelableProvisionEvents
        public static bool IsCancelableProvisionEvents(string pTradeActionType)
        {
            return IsCancelableProvisionEvents(GetTradeActionTypeEnum(pTradeActionType));
        }
        public static bool IsCancelableProvisionEvents(TradeActionTypeEnum pTradeActionType)
        {
            return (TradeActionTypeEnum.CancelableProvisionEvents == pTradeActionType);
        }
        #endregion IsCancelableProvisionEvents
        #region public IsCalculationAgentSettlementRateEvents
        //PL 20100628
        public static bool IsCalculationAgentSettlementRateEvents(string pTradeActionType)
        {
            return IsCalculationAgentSettlementRateEvents(GetTradeActionTypeEnum(pTradeActionType));
        }
        public static bool IsCalculationAgentSettlementRateEvents(TradeActionTypeEnum pTradeActionType)
        {
            return (TradeActionTypeEnum.CalculationAgentSettlementRateEvents == pTradeActionType);
        }
        #endregion IsCalculationAgentSettlementRateEvents
        #region public IsExerciseEvents
        public static bool IsExerciseEvents(string pTradeActionType)
        {
            return IsExerciseEvents(GetTradeActionTypeEnum(pTradeActionType));
        }
        public static bool IsExerciseEvents(TradeActionTypeEnum pTradeActionType)
        {
            return (TradeActionTypeEnum.ExerciseEvents == pTradeActionType);
        }
        #endregion IsExerciseEvents
        #region public IsExtendibleProvisionEvents
        public static bool IsExtendibleProvisionEvents(string pTradeActionType)
        {
            return IsExtendibleProvisionEvents(GetTradeActionTypeEnum(pTradeActionType));
        }
        public static bool IsExtendibleProvisionEvents(TradeActionTypeEnum pTradeActionType)
        {
            return (TradeActionTypeEnum.ExtendibleProvisionEvents == pTradeActionType);
        }
        #endregion IsExtendibleProvisionEvents
        #region public IsOptionalEarlyTerminationProvisionEvents
        public static bool IsOptionalEarlyTerminationProvisionEvents(string pTradeActionType)
        {
            return IsOptionalEarlyTerminationProvisionEvents(GetTradeActionTypeEnum(pTradeActionType));
        }
        public static bool IsOptionalEarlyTerminationProvisionEvents(TradeActionTypeEnum pTradeActionType)
        {
            return (TradeActionTypeEnum.OptionalEarlyTerminationProvisionEvents == pTradeActionType);
        }
        #endregion IsOptionalEarlyTerminationProvisionEvents
        #region public IsInvoicingCorrectionEvents
        public static bool IsInvoicingCorrectionEvents(string pTradeActionType)
        {
            return IsInvoicingCorrectionEvents(GetTradeActionTypeEnum(pTradeActionType));
        }
        public static bool IsInvoicingCorrectionEvents(TradeActionTypeEnum pTradeActionType)
        {
            return (TradeActionTypeEnum.InvoicingCorrectionEvents == pTradeActionType);
        }
        #endregion IsInvoicingCorrectionEvents
        #region public IsMandatoryEarlyTerminationProvisionEvents
        public static bool IsMandatoryEarlyTerminationProvisionEvents(string pTradeActionType)
        {
            return IsMandatoryEarlyTerminationProvisionEvents(GetTradeActionTypeEnum(pTradeActionType));
        }
        public static bool IsMandatoryEarlyTerminationProvisionEvents(TradeActionTypeEnum pTradeActionType)
        {
            return (TradeActionTypeEnum.MandatoryEarlyTerminationProvisionEvents == pTradeActionType);
        }
        #endregion IsMandatoryEarlyTerminationProvisionEvents
        //CC 20110204
        #region public IsPositionCancelationEvents
        public static bool IsPositionCancelationEvents(string pTradeActionType)
        {
            return IsPositionCancelationEvents(GetTradeActionTypeEnum(pTradeActionType));
        }
        public static bool IsPositionCancelationEvents(TradeActionTypeEnum pTradeActionType)
        {
            return (TradeActionTypeEnum.PositionCancelationEvents == pTradeActionType);
        }
        #endregion IsQuantityCorrectionEvents
        #region public IsStepUpProvisionEvents
        public static bool IsStepUpProvisionEvents(string pTradeActionType)
        {
            return IsStepUpProvisionEvents(GetTradeActionTypeEnum(pTradeActionType));
        }
        public static bool IsStepUpProvisionEvents(TradeActionTypeEnum pTradeActionType)
        {
            return (TradeActionTypeEnum.StepUpProvisionEvents == pTradeActionType);
        }
        #endregion IsStepUpProvisionEvents
        //CC 20110215
        #region public IsPositionTransferEvents
        public static bool IsPositionTransferEvents(string pTradeActionType)
        {
            return IsPositionTransferEvents(GetTradeActionTypeEnum(pTradeActionType));
        }
        public static bool IsPositionTransferEvents(TradeActionTypeEnum pTradeActionType)
        {
            return (TradeActionTypeEnum.PositionTransferEvents == pTradeActionType);
        }
        #endregion IsPositionTransferEvents
        #region public IsTradeSplitting
        public static bool IsTradeSplitting(string pTradeActionType)
        {
            return IsTradeSplitting(GetTradeActionTypeEnum(pTradeActionType));
        }
        public static bool IsTradeSplitting(TradeActionTypeEnum pTradeActionType)
        {
            return (TradeActionTypeEnum.TradeSplitting == pTradeActionType);
        }
        #endregion IsTradeSplitting
        #region public IsRemoveAllocationTradeEvents
        public static bool IsRemoveAllocationTradeEvents(string pTradeActionType)
        {
            return IsRemoveAllocationTradeEvents(GetTradeActionTypeEnum(pTradeActionType));
        }
        public static bool IsRemoveAllocationTradeEvents(TradeActionTypeEnum pTradeActionType)
        {
            return (TradeActionTypeEnum.RemoveAllocationTradeEvents == pTradeActionType);
        }
        #endregion public IsRemoveAllocationTradeEvents
        // PM 20130822 [17949] Livraison Matif
        #region public IsUnderlyingDeliveryEvents
        public static bool IsUnderlyingDeliveryEvents(string pTradeActionType)
        {
            return IsUnderlyingDeliveryEvents(GetTradeActionTypeEnum(pTradeActionType));
        }
        public static bool IsUnderlyingDeliveryEvents(TradeActionTypeEnum pTradeActionType)
        {
            return (TradeActionTypeEnum.UnderlyingDeliveryEvents == pTradeActionType);
        }
        #endregion IsUnderlyingDeliveryEvents

        #region public IsNone
        public static bool IsNone(TradeActionTypeEnum pTradeActionType)
        {
            return (TradeActionTypeEnum.None == pTradeActionType);
        }
        #endregion
        #region public GetTradeActionTypeEnum
        public static TradeActionTypeEnum GetTradeActionTypeEnum(string pTradeActionType)
        {
            if (System.Enum.IsDefined(typeof(TradeActionTypeEnum), pTradeActionType))
                return (TradeActionTypeEnum)System.Enum.Parse(typeof(TradeActionTypeEnum), pTradeActionType, true);
            return TradeActionTypeEnum.None;
        }
        #endregion
        #region public GetMenuActionType
        public static string GetMenuActionType(TradeActionTypeEnum pTradeActionTypeEnum)
        {
            string ret;
            switch (pTradeActionTypeEnum)
            {
                case TradeActionTypeEnum.AbandonEvents:
                    ret = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_ABN);
                    break;
                //CC 20110204
                case TradeActionTypeEnum.AssignmentEvents:
                    ret = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_ASS);
                    break;
                case TradeActionTypeEnum.BarrierAndTriggerEvents:
                    ret = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_BAR_TRG);
                    break;
                case TradeActionTypeEnum.RemoveTradeEvents:
                    ret = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_RMV);
                    break;
                //PL 20100628
                case TradeActionTypeEnum.CalculationAgentSettlementRateEvents:
                    ret = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_CSR);
                    break;
                case TradeActionTypeEnum.ExerciseEvents:
                    ret = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_EXE);
                    break;
                case TradeActionTypeEnum.CancelableProvisionEvents:
                    ret = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_RES);
                    break;
                case TradeActionTypeEnum.ExtendibleProvisionEvents:
                    ret = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_PRO);
                    break;
                case TradeActionTypeEnum.OptionalEarlyTerminationProvisionEvents:
                    ret = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_OET);
                    break;
                case TradeActionTypeEnum.MandatoryEarlyTerminationProvisionEvents:
                    ret = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_MET);
                    break;
                //CC 20110204
                case TradeActionTypeEnum.PositionCancelationEvents:
                    ret = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_POC);
                    break;
                case TradeActionTypeEnum.StepUpProvisionEvents:
                    ret = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_SUP);
                    break;
                //CC 20110215
                case TradeActionTypeEnum.PositionTransferEvents:
                    ret = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_POT);
                    break;
                case TradeActionTypeEnum.TradeSplitting:
                    ret = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_SPLIT);
                    break;
                case TradeActionTypeEnum.InvoicingCorrectionEvents:
                    ret = IdMenu.GetIdMenu(IdMenu.Menu.InputTradeAdmin_COR);
                    break;
                case TradeActionTypeEnum.ClearingSpecificEvents:
                    ret = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_CLEARSPEC);
                    break;
                case TradeActionTypeEnum.RemoveAllocationTradeEvents:
                    ret = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_RMVALLOC);
                    break;
                case TradeActionTypeEnum.UnderlyingDeliveryEvents:
                    ret = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_DLV);
                    break;
                default:
                    throw new Exception("Current action with no Menu defined");
            }
            return ret;
        }
        #endregion
    }
    #endregion class TradeActionType
    #region public class TradeActionCode
    // EG 20240123 [WI816] New TradeActionCode : FeesEventGenUninvoiced
    public sealed class TradeActionCode
    {
        #region TradeActionCodeEnum
        /// <summary>
        /// 
        /// </summary>
        /// FI 20160907 [21831] Modify
        /// FI 20170306 [22225] Modify
        public enum TradeActionCodeEnum
        {
            Barrier = 1,
            Trigger = 2,
            Payout = 3,
            Rebate = 4,
            CashSettlement = 5,
            Exercise = 6,
            Abandon = 7,
            RemoveTrade = 8,
            CustomerSettlementRate = 9,
            CancelableProvision = 10,
            ExtendibleProvision = 11,
            OptionalEarlyTerminationProvision = 12,
            MandatoryEarlyTerminationProvision = 13,
            StepUpProvision = 14,

            /// <summary>
            /// Génération des évènements de frais
            /// </summary>
            /// FI 20160907 [21831] Add
            FeesEventGen = 25,

            /// <summary>
            /// Recalcul des frais  
            /// </summary>
            /// FI 20170306 [22225] Add
            FeesCalculation = 26,
            FeesEventGenUninvoiced = 27,

            InvoiceCorrection = 50,

            Unknown = 99,
        }
        #endregion TradeActionCodeEnum
    }
    #endregion class TradeActionCode
    //
    #region public class LevelStatusTools
    public sealed class LevelStatusTools
    {
        #region public class  LevelAssociateAttribute
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
        public sealed class LevelAssociateAttribute : Attribute
        {
            #region Members
            private LevelEnum m_Level;
            #endregion Members
            #region Accessors
            public LevelEnum Level
            {
                get { return (m_Level); }
                set { m_Level = value; }
            }
            #endregion Accessors
        }
        #endregion LevelAssociateAttribute
        #region public class  HelpAssociateAttribute
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        public sealed class HelpAssociateAttribute : Attribute
        {
            #region Members
            private string m_Help;
            private string m_ImageUrl;
            #endregion Members
            #region Accessors
            public string Help
            {
                get { return (m_Help); }
                set { m_Help = value; }
            }
            public string ImageUrl
            {
                get { return (m_ImageUrl); }
                set { m_ImageUrl = value; }
            }
            #endregion Accessors
        }
        #endregion HelpAssociateAttribute
        //
        #region Members
        #region CodeReturnEnum
        //public enum CodeReturnEnum
        //{
        //    SUCCESS = -1,
        //    UNDEFINED = 0,
        //    BREAK = 1,
        //    SQLDEFINED = 5,
        //    USERDEFINED = 10,
        //    LOGINUNSUCCESSFUL = 30,
        //    LOGINSIMULTANEOUS_USER = 31,
        //    LOGINSIMULTANEOUS_ALLUSER = 32,
        //    ACCESDENIED = 35,
        //    LOCKUNSUCCESSFUL = 40,
        //    INCORRECTPARAMETER = 60,
        //    FOLDERNOTFOUND = 70,
        //    FILENOTFOUND = 71,
        //    URLNOTFOUND = 72,
        //    DATANOTFOUND = 73,
        //    DATAUNMATCH = 74,
        //    DATADISABLED = 75,
        //    DATAIGNORE = 76,
        //    MULTIDATAFOUND = 77,
        //    BUG = 80,
        //    TIMEOUT = 82,
        //    DEADLOCK = 84,
        //    IRVIOLATION = 86,
        //    MISCELLANEOUS = 99
        //}
        #endregion CodeReturnEnum
        #region LevelEnum
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        public enum LevelEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("LevelAlertMsg")]
            [HelpAssociate(Help = "LevelAlertMsgHelp", ImageUrl = "fa-icon fas fa-folder-open red")]
            ALERT,
            [System.Xml.Serialization.XmlEnumAttribute("LevelWarningMsg")]
            [HelpAssociate(Help = "LevelWarningMsgHelp", ImageUrl = "fa-icon fas fa-folder-open orange")]
            WARNING,
            [System.Xml.Serialization.XmlEnumAttribute("LevelInfoMsg")]
            [HelpAssociate(Help = "LevelInfoMsgHelp", ImageUrl = "fa-icon fas fa-folder-open blue")]
            INFO,
            [System.Xml.Serialization.XmlEnumAttribute("LevelOtherMsg")]
            [HelpAssociate(Help = "LevelUnknownMsgHelp", ImageUrl = "fa-icon fas fa-folder-open gray")]
            NA,
        }
        #endregion LevelEnum
        #region StatusEnum
        public enum StatusEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("StatusError")]
            [LevelAssociate(Level = LevelEnum.ALERT)]
            [HelpAssociate(Help = "StatusErrorMsgHelp", ImageUrl = "red")]
            ERROR,
            [System.Xml.Serialization.XmlEnumAttribute("StatusPending")]
            [LevelAssociate(Level = LevelEnum.WARNING)]
            [HelpAssociate(Help = "StatusPendingMsgHelp", ImageUrl = "marron")]
            PENDING,
            [System.Xml.Serialization.XmlEnumAttribute("StatusProgress")]
            [LevelAssociate(Level = LevelEnum.INFO)]
            [HelpAssociate(Help = "StatusProgressMsgHelp", ImageUrl = "violet")]
            PROGRESS,
            [System.Xml.Serialization.XmlEnumAttribute("StatusSuccess")]
            [LevelAssociate(Level = LevelEnum.INFO)]
            [HelpAssociate(Help = "StatusSuccessMsgHelp", ImageUrl = "green")]
            SUCCESS,
            [System.Xml.Serialization.XmlEnumAttribute("StatusInfo")]
            [LevelAssociate(Level = LevelEnum.INFO)]
            [HelpAssociate(Help = "StatusUnknownMsgHelp", ImageUrl = "blue")]
            INFO,
            [System.Xml.Serialization.XmlEnumAttribute("StatusOthers")]
            [LevelAssociate(Level = LevelEnum.NA)]
            [HelpAssociate(Help = "StatusUnknownMsgHelp", ImageUrl = "gray")]
            NA,
            [System.Xml.Serialization.XmlEnumAttribute("StatusSuccessWarning")]
            [LevelAssociate(Level = LevelEnum.WARNING)]
            [HelpAssociate(Help = "StatusSuccessWarningMsgHelp", ImageUrl = "orange")]
            SUCCESSWITHMESSAGE, /* VIRTUAL */
        }
        #endregion StatusEnum
        #endregion Members
        //
        #region Accessors
        #region CodeReturn
        public static string CodeReturnSuccess { get { return Cst.ErrLevel.SUCCESS.ToString(); } }
        public static string CodeReturnUndefined { get { return Cst.ErrLevel.UNDEFINED.ToString(); } }
        public static string CodeReturnBreak { get { return Cst.ErrLevel.BREAK.ToString(); } }
        public static string CodeReturnSqlDefined { get { return Cst.ErrLevel.SQLDEFINED.ToString(); } }
        public static string CodeReturnUserDefined { get { return Cst.ErrLevel.USERDEFINED.ToString(); } }
        public static string CodeReturnLoginUnsuccessful { get { return Cst.ErrLevel.LOGINUNSUCCESSFUL.ToString(); } }
        public static string CodeReturnLoginSimultaneous_User { get { return Cst.ErrLevel.LOGINSIMULTANEOUS_USER.ToString(); } }
        public static string CodeReturnLoginSimultaneous_AllUser { get { return Cst.ErrLevel.LOGINSIMULTANEOUS_ALLUSER.ToString(); } }
        public static string CodeReturnAccessDenied { get { return Cst.ErrLevel.ACCESDENIED.ToString(); } }
        public static string CodeReturnLockUnsuccessful { get { return Cst.ErrLevel.LOCKUNSUCCESSFUL.ToString(); } }
        public static string CodeReturnMOMUnknown { get { return Cst.ErrLevel.MOM_UNKNOWN.ToString(); } }
        public static string CodeReturnMOMPathError { get { return Cst.ErrLevel.MOM_PATH_ERROR.ToString(); } }
        public static string CodeReturnInitializeError { get { return Cst.ErrLevel.INITIALIZE_ERROR.ToString(); } }
        public static string CodeReturnMessageNotFound { get { return Cst.ErrLevel.MESSAGE_NOTFOUND.ToString(); } }
        public static string CodeReturnMessageMove { get { return Cst.ErrLevel.MESSAGE_MOVE_ERROR.ToString(); } }
        public static string CodeReturnMessageRead { get { return Cst.ErrLevel.MESSAGE_READ_ERROR.ToString(); } }
        public static string CodeReturnMessageCast { get { return Cst.ErrLevel.MESSAGE_CAST_ERROR.ToString(); } }

        public static string CodeReturnIncorrectParameter { get { return Cst.ErrLevel.INCORRECTPARAMETER.ToString(); } }
        public static string CodeReturnMissingParameter { get { return Cst.ErrLevel.MISSINGPARAMETER.ToString(); } }
        public static string CodeReturnFolderNotFound { get { return Cst.ErrLevel.FOLDERNOTFOUND.ToString(); } }
        public static string CodeReturnFileNotFound { get { return Cst.ErrLevel.FILENOTFOUND.ToString(); } }
        public static string CodeReturnUrlNotFound { get { return Cst.ErrLevel.URLNOTFOUND.ToString(); } }
        //public static string CodeReturnDataNotFound { get { return Cst.ErrLevel.DATANOTFOUND.ToString(); } }
        public static string CodeReturnDataUnMatch { get { return Cst.ErrLevel.DATAUNMATCH.ToString(); } }
        public static string CodeReturnDataDisabled { get { return Cst.ErrLevel.DATADISABLED.ToString(); } }
        public static string CodeReturnDataIgnore { get { return Cst.ErrLevel.DATAIGNORE.ToString(); } }
        public static string CodeReturnMultiDataFound { get { return Cst.ErrLevel.MULTIDATAFOUND.ToString(); } }
        public static string CodeReturnFailure { get { return Cst.ErrLevel.FAILURE.ToString(); } }
        public static string CodeReturnTimeOut { get { return Cst.ErrLevel.TIMEOUT.ToString(); } }
        public static string CodeReturnDeadLock { get { return Cst.ErrLevel.DEADLOCK.ToString(); } }
        public static string CodeReturnIrViolation { get { return Cst.ErrLevel.IRVIOLATION.ToString(); } }
        public static string CodeReturnAborted { get { return Cst.ErrLevel.ABORTED.ToString(); } }
        public static string CodeReturnNoBookManaged { get { return Cst.ErrLevel.NOBOOKMANAGED.ToString(); } }

        public static Cst.ErrLevel CodeReturnSuccessEnum { get { return Cst.ErrLevel.SUCCESS; } }
        public static Cst.ErrLevel CodeReturnUndefinedEnum { get { return Cst.ErrLevel.UNDEFINED; } }
        public static Cst.ErrLevel CodeReturnBreakEnum { get { return Cst.ErrLevel.BREAK; } }
        public static Cst.ErrLevel CodeReturnSqlDefinedEnum { get { return Cst.ErrLevel.SQLDEFINED; } }
        public static Cst.ErrLevel CodeReturnUserDefinedEnum { get { return Cst.ErrLevel.USERDEFINED; } }
        public static Cst.ErrLevel CodeReturnLoginUnsuccessfulEnum { get { return Cst.ErrLevel.LOGINUNSUCCESSFUL; } }
        public static Cst.ErrLevel CodeReturnLoginSimultaneous_UserEnum { get { return Cst.ErrLevel.LOGINSIMULTANEOUS_USER; } }
        public static Cst.ErrLevel CodeReturnLoginSimultaneous_AllUserEnum { get { return Cst.ErrLevel.LOGINSIMULTANEOUS_ALLUSER; } }
        public static Cst.ErrLevel CodeReturnAccessDeniedEnum { get { return Cst.ErrLevel.ACCESDENIED; } }
        public static Cst.ErrLevel CodeReturnLockUnsuccessfulEnum { get { return Cst.ErrLevel.LOCKUNSUCCESSFUL; } }
        public static Cst.ErrLevel CodeReturnMOMUnknownEnum { get { return Cst.ErrLevel.MOM_UNKNOWN; } }
        public static Cst.ErrLevel CodeReturnMOMPathErrorEnum { get { return Cst.ErrLevel.MOM_PATH_ERROR; } }
        public static Cst.ErrLevel CodeReturnInitializeErrorEnum { get { return Cst.ErrLevel.INITIALIZE_ERROR; } }
        public static Cst.ErrLevel CodeReturnInterruptEnum { get { return Cst.ErrLevel.IRQ_EXECUTED; } }
        public static Cst.ErrLevel CodeReturnMessageNotFoundEnum { get { return Cst.ErrLevel.MESSAGE_NOTFOUND; } }
        public static Cst.ErrLevel CodeReturnMessageMoveEnum { get { return Cst.ErrLevel.MESSAGE_MOVE_ERROR; } }
        public static Cst.ErrLevel CodeReturnMessageReadEnum { get { return Cst.ErrLevel.MESSAGE_READ_ERROR; } }
        public static Cst.ErrLevel CodeReturnMessageCastEnum { get { return Cst.ErrLevel.MESSAGE_CAST_ERROR; } }
        public static Cst.ErrLevel CodeReturnMessageNotConformEnum { get { return Cst.ErrLevel.MESSAGE_NOTCONFORM; } }
        public static Cst.ErrLevel CodeReturnIncorrectParameterEnum { get { return Cst.ErrLevel.INCORRECTPARAMETER; } }
        public static Cst.ErrLevel CodeReturnFolderNotFoundEnum { get { return Cst.ErrLevel.FOLDERNOTFOUND; } }
        public static Cst.ErrLevel CodeReturnFileNotFoundEnum { get { return Cst.ErrLevel.FILENOTFOUND; } }
        public static Cst.ErrLevel CodeReturnUrlNotFoundEnum { get { return Cst.ErrLevel.URLNOTFOUND; } }
        public static Cst.ErrLevel CodeReturnDataNotFoundEnum { get { return Cst.ErrLevel.DATANOTFOUND; } }
        public static Cst.ErrLevel CodeReturnDataUnMatchEnum { get { return Cst.ErrLevel.DATAUNMATCH; } }
        public static Cst.ErrLevel CodeReturnDataDisabledEnum { get { return Cst.ErrLevel.DATADISABLED; } }
        public static Cst.ErrLevel CodeReturnDataIgnoreEnum { get { return Cst.ErrLevel.DATAIGNORE; } }
        public static Cst.ErrLevel CodeReturnMultiDataFoundEnum { get { return Cst.ErrLevel.MULTIDATAFOUND; } }
        public static Cst.ErrLevel CodeReturnFailureEnum { get { return Cst.ErrLevel.FAILURE; } }
        public static Cst.ErrLevel CodeReturnTimeOutEnum { get { return Cst.ErrLevel.TIMEOUT; } }
        public static Cst.ErrLevel CodeReturnDeadLockEnum { get { return Cst.ErrLevel.DEADLOCK; } }
        public static Cst.ErrLevel CodeReturnIrViolationEnum { get { return Cst.ErrLevel.IRVIOLATION; } }
        public static Cst.ErrLevel CodeReturnAbortedEnum { get { return Cst.ErrLevel.ABORTED; } }
        public static Cst.ErrLevel CodeReturnNoBookManagedEnum { get { return Cst.ErrLevel.NOBOOKMANAGED; } }
        #endregion CodeReturn
        #region Level
        public static string LevelAlert { get { return LevelEnum.ALERT.ToString(); } }
        public static string LevelInfo { get { return LevelEnum.INFO.ToString(); } }
        public static string LevelUnknown { get { return LevelEnum.NA.ToString(); } }
        public static string LevelWarning { get { return LevelEnum.WARNING.ToString(); } }

        public static LevelEnum LevelAlertEnum { get { return LevelEnum.ALERT; } }
        public static LevelEnum LevelInfoEnum { get { return LevelEnum.INFO; } }
        public static LevelEnum LevelUnknownEnum { get { return LevelEnum.NA; } }
        public static LevelEnum LevelWarningEnum { get { return LevelEnum.WARNING; } }
        #endregion Level
        #region Status
        public static string StatusError { get { return StatusEnum.ERROR.ToString(); } }
        public static string StatusPending { get { return StatusEnum.PENDING.ToString(); } }
        public static string StatusProgress { get { return StatusEnum.PROGRESS.ToString(); } }
        public static string StatusUnknown { get { return StatusEnum.NA.ToString(); } }
        public static string StatusSuccess { get { return StatusEnum.SUCCESS.ToString(); } }

        public static StatusEnum StatusErrorEnum { get { return StatusEnum.ERROR; } }
        public static StatusEnum StatusPendingEnum { get { return StatusEnum.PENDING; } }
        public static StatusEnum StatusProgressEnum { get { return StatusEnum.PROGRESS; } }
        public static StatusEnum StatusUnknownEnum { get { return StatusEnum.NA; } }
        public static StatusEnum StatusSuccessEnum { get { return StatusEnum.SUCCESS; } }
        #endregion Status
        #endregion Accessors
        //
        #region Methods
        #region public GetCodeReturn
        public static Cst.ErrLevel GetCodeReturn(string pCodeReturn)
        {
            if (Enum.IsDefined(typeof(Cst.ErrLevel), pCodeReturn))
                return (Cst.ErrLevel)Enum.Parse(typeof(Cst.ErrLevel), pCodeReturn, true);
            else
                return (Cst.ErrLevel)Enum.Parse(typeof(Cst.ErrLevel), LevelStatusTools.CodeReturnUndefined, true);
        }
        #endregion GetCodeReturn
        #region public GetLevel
        public static LevelStatusTools.LevelEnum GetLevel(string pLevel)
        {
            if (Enum.IsDefined(typeof(LevelStatusTools.LevelEnum), pLevel))
                return (LevelStatusTools.LevelEnum)Enum.Parse(typeof(LevelStatusTools.LevelEnum), pLevel, true);
            else
                return (LevelStatusTools.LevelEnum)Enum.Parse(typeof(LevelStatusTools.LevelEnum), LevelStatusTools.LevelUnknown, true);
        }
        #endregion GetLevel
        #region public GetStatus
        public static LevelStatusTools.StatusEnum GetStatus(string pStatus)
        {
            if (Enum.IsDefined(typeof(LevelStatusTools.StatusEnum), pStatus))
                return (LevelStatusTools.StatusEnum)Enum.Parse(typeof(LevelStatusTools.StatusEnum), pStatus, true);
            else
                return (LevelStatusTools.StatusEnum)Enum.Parse(typeof(LevelStatusTools.StatusEnum), LevelStatusTools.StatusUnknown, true);
        }
        #endregion GetStatus
        #region public IsCodeReturn...
        public static bool IsCodeReturnSuccess(string pCodeReturn) { return CodeReturnSuccess == pCodeReturn; }
        public static bool IsCodeReturnSuccess(Cst.ErrLevel pCodeReturn) { return CodeReturnSuccessEnum == pCodeReturn; }
        public static bool IsCodeReturnUnsuccessful(string pCodeReturn) { return CodeReturnSuccess != pCodeReturn; }
        public static bool IsCodeReturnUnsuccessful(Cst.ErrLevel pCodeReturn) { return CodeReturnSuccessEnum != pCodeReturn; }
        public static bool IsCodeReturnInterrupt(Cst.ErrLevel pCodeReturn) { return CodeReturnInterruptEnum == pCodeReturn; }
        #endregion IsCodeReturn...
        #region public IsLevel...
        public static bool IsLevelAlert(string pLevel) { return LevelAlert == pLevel; }
        public static bool IsLevelInfo(string pLevel) { return LevelInfo == pLevel; }
        public static bool IsLevelUnknown(string pLevel) { return LevelUnknown == pLevel; }
        public static bool IsLevelWarning(string pLevel) { return LevelWarning == pLevel; }

        public static bool IsLevelAlert(LevelEnum pLevel) { return LevelAlertEnum == pLevel; }
        public static bool IsLevelInfo(LevelEnum pLevel) { return LevelInfoEnum == pLevel; }
        public static bool IsLevelUnknown(LevelEnum pLevel) { return LevelUnknownEnum == pLevel; }
        public static bool IsLevelWarning(LevelEnum pLevel) { return LevelWarningEnum == pLevel; }
        #endregion
        #region public IsStatus...
        public static bool IsStatusError(string pStatus) { return StatusError == pStatus; }
        public static bool IsStatusPending(string pStatus) { return StatusPending == pStatus; }
        public static bool IsStatusProgress(string pStatus) { return StatusProgress == pStatus; }
        public static bool IsStatusUnknown(string pStatus) { return StatusUnknown == pStatus; }
        public static bool IsStatusSuccess(string pStatus) { return StatusSuccess == pStatus; }

        public static bool IsStatusError(StatusEnum pStatus) { return StatusErrorEnum == pStatus; }
        public static bool IsStatusPending(StatusEnum pStatus) { return StatusPendingEnum == pStatus; }
        public static bool IsStatusProgress(StatusEnum pStatus) { return StatusProgressEnum == pStatus; }
        public static bool IsStatusUnknown(StatusEnum pStatus) { return StatusUnknownEnum == pStatus; }
        public static bool IsStatusSuccess(StatusEnum pStatus) { return StatusSuccessEnum == pStatus; }
        #endregion
        #region public GetLevelCssClass
        public static string GetLevelCssClass(string pLevel, string pStatus)
        {
            return GetLevelCssClass(GetLevel(pLevel), GetStatus(pStatus));
        }
        private static string GetLevelCssClass(LevelEnum pLevel, StatusEnum pStatus)
        {
            string cssClass = string.Empty;
            if (IsLevelAlert(pLevel))
                cssClass = "TrackerRed";
            else if (IsLevelWarning(pLevel))
            {
                if (IsStatusPending(pStatus))
                    cssClass = "TrackerMarron";
                else if (IsStatusSuccess(pStatus))
                    cssClass = "TrackerOrange";
            }
            else if (IsLevelInfo(pLevel))
            {
                if (IsStatusProgress(pStatus))
                    cssClass = "TrackerViolet";
                else if (IsStatusPending(pStatus))
                    cssClass = "TrackerMarron";
                else if (IsStatusSuccess(pStatus))
                    cssClass = "TrackerGreen";
                else if (IsStatusUnknown(pStatus))
                    cssClass = "TrackerBlue";
            }
            else if (IsLevelUnknown(pLevel))
                cssClass = "TrackerGray";
            return cssClass;
        }
        #endregion public GetLevelCssClass
        #region public GetForeColorMessage
        public static Color GetForeColorMessage(string pLevel, string pStatus)
        {
            return GetForeColorMessage(GetLevel(pLevel), GetStatus(pStatus));
        }
        public static Color GetForeColorMessage(LevelEnum pLevel, StatusEnum pStatus)
        {
            Color foreColor = Color.Black;
            if (IsLevelAlert(pLevel))
                foreColor = CstCSSColor.Convert(CstCSSColor.red);
            else if (IsLevelWarning(pLevel))
            {
                if (IsStatusPending(pStatus))
                    foreColor = CstCSSColor.Convert(CstCSSColor.marron);
                else if (IsStatusSuccess(pStatus))
                    foreColor = CstCSSColor.Convert(CstCSSColor.orange);
                else
                    foreColor = CstCSSColor.Convert(CstCSSColor.orange);
            }
            else if (IsLevelInfo(pLevel))
            {
                if (IsStatusPending(pStatus))
                    foreColor = CstCSSColor.Convert(CstCSSColor.marron);
                if (IsStatusProgress(pStatus))
                    foreColor = CstCSSColor.Convert(CstCSSColor.violet);
                else if (IsStatusSuccess(pStatus))
                    foreColor = CstCSSColor.Convert(CstCSSColor.green);
                else if (IsStatusUnknown(pStatus))
                    foreColor = CstCSSColor.Convert(CstCSSColor.blue);
            }
            else if (IsLevelUnknown(pLevel))
                foreColor = CstCSSColor.Convert(CstCSSColor.gray);

            return foreColor;
        }
        #endregion
        #region public GetResourceStatus
        public static string GetResourceStatus(string pLevel, string pStatus)
        {
            return GetResourceStatus(GetLevel(pLevel), GetStatus(pStatus));
        }
        public static string GetResourceStatus(LevelEnum pLevel, StatusEnum pStatus)
        {
            string ret = string.Empty;
            if (IsStatusSuccess(pStatus))
            {
                if (IsLevelWarning(pLevel))
                    ret = Ressource.GetString("StatusSuccessWarning");
                else
                    ret = Ressource.GetString("StatusSuccess");
            }
            else if (IsStatusError(pStatus))
            {
                ret = Ressource.GetString("StatusError");
            }
            else if (IsStatusProgress(pStatus))
            {
                ret = Ressource.GetString("StatusProgress");
            }
            else if (IsStatusPending(pStatus))
            {
                ret = Ressource.GetString("StatusPending");
            }
            else if (IsStatusUnknown(pStatus))
            {
                ret = Ressource.GetString("StatusOthers");
            }
            return ret;
        }
        #endregion
        #region public GetPowerOfAllStatusByLevel
        public static int GetPowerOfAllStatusByLevel(LevelStatusTools.LevelEnum pLevel)
        {
            LevelStatusTools.StatusEnum statusEnum = new LevelStatusTools.StatusEnum();
            FieldInfo[] statusFlds = statusEnum.GetType().GetFields();
            int powerStatus = 0;
            foreach (FieldInfo statusFld in statusFlds)
            {
                object[] levelAssociateAttrs = statusFld.GetCustomAttributes(typeof(LevelStatusTools.LevelAssociateAttribute), true);
                if (0 < levelAssociateAttrs.Length)
                {
                    foreach (LevelStatusTools.LevelAssociateAttribute levelAssociate in levelAssociateAttrs)
                    {
                        if (pLevel == levelAssociate.Level)
                        {
                            LevelStatusTools.StatusEnum status = (LevelStatusTools.StatusEnum)Enum.Parse(typeof(LevelStatusTools.StatusEnum), statusFld.Name, false);
                            int i = int.Parse(Enum.Format(typeof(LevelStatusTools.StatusEnum), status, "d"));
                            powerStatus += (int)Math.Pow(2, i);
                        }
                    }
                }
            }
            return powerStatus;
        }
        #endregion GetPowerOfAllStatusByLevel
        #region  public GetStatusByLevel
        public static string[] GetStatusByLevel(LevelStatusTools.LevelEnum pLevel, int pPowerStatus)
        {
            string[] ret = null;
            //
            ArrayList aStatus = new ArrayList();
            LevelStatusTools.StatusEnum statusEnum = new LevelStatusTools.StatusEnum();
            FieldInfo[] statusFlds = statusEnum.GetType().GetFields();
            foreach (FieldInfo statusFld in statusFlds)
            {
                object[] levelAssociateAttrs = statusFld.GetCustomAttributes(typeof(LevelStatusTools.LevelAssociateAttribute), true);
                if (ArrFunc.Count(levelAssociateAttrs) > 0)
                {
                    foreach (LevelStatusTools.LevelAssociateAttribute levelAssociate in levelAssociateAttrs)
                    {
                        if (pLevel == levelAssociate.Level)
                        {
                            LevelStatusTools.StatusEnum status = (LevelStatusTools.StatusEnum)Enum.Parse(typeof(LevelStatusTools.StatusEnum), statusFld.Name, false);
                            int i = int.Parse(Enum.Format(typeof(LevelStatusTools.StatusEnum), status, "d"));
                            //if (0 < (pPowerStatus & Convert.ToInt32(Math.Pow(2, i))))
                            // EG 20100410 Bidouille suite Difference éventuelle entre Cookie (ancien) et Puissance de 2
                            bool isOk = (0 < (pPowerStatus & Convert.ToInt32(Math.Pow(2, i))));
                            isOk |= (pLevel == LevelEnum.NA) && (16 == pPowerStatus) && (32 == Convert.ToInt32(Math.Pow(2, i)));
                            if (isOk)
                            {
                                string statusValue = statusFld.Name;
                                //20090129 FI ticket 16339 SUCCESSWITHMESSAGE est un status virtual => Le vrai status est SUCCESS
                                if ((LevelEnum.WARNING == pLevel) && (status == LevelStatusTools.StatusEnum.SUCCESSWITHMESSAGE))
                                    statusValue = LevelStatusTools.StatusEnum.SUCCESS.ToString();
                                aStatus.Add(statusValue);
                            }
                        }
                    }
                }
            }
            //
            if (aStatus.Count > 0)
                ret = (string[])aStatus.ToArray(typeof(string));
            //
            return ret;
        }
        #endregion
        #endregion

    }
    #endregion class LevelStatusTools
    #region public class LevelStatus
    public class LevelStatus
    {
        #region Members
        private LevelStatusTools.LevelEnum m_Level;
        private LevelStatusTools.StatusEnum m_Status;
        private Cst.ErrLevel m_CodeReturn;
        #endregion Members
        //
        #region Accessors
        #region public CodeReturn
        public Cst.ErrLevel CodeReturn
        {
            set { m_CodeReturn = value; }
            get { return m_CodeReturn; }
        }
        #endregion CodeReturn
        #region public Level
        public LevelStatusTools.LevelEnum Level
        {
            set { m_Level = value; }
            get { return m_Level; }
        }
        #endregion Level
        #region Status
        public LevelStatusTools.StatusEnum Status
        {
            set { m_Status = value; }
            get { return m_Status; }
        }
        #endregion Status
        #endregion Accessors
        //
        #region Constructors
        public LevelStatus()
            : this(LevelStatusTools.LevelUnknown, LevelStatusTools.StatusUnknown, LevelStatusTools.CodeReturnUndefined) { }
        public LevelStatus(string pLevel, string pStatus)
            : this(pLevel, pStatus, LevelStatusTools.CodeReturnUndefined) { }
        public LevelStatus(LevelStatusTools.LevelEnum pLevel, LevelStatusTools.StatusEnum pStatus)
            : this(pLevel, pStatus, LevelStatusTools.CodeReturnUndefinedEnum) { }
        public LevelStatus(string pLevel, string pStatus, string pCodeReturn)
        {
            m_Level = LevelStatusTools.GetLevel(pLevel);
            m_Status = LevelStatusTools.GetStatus(pStatus);
            m_CodeReturn = LevelStatusTools.GetCodeReturn(pCodeReturn);
        }
        public LevelStatus(LevelStatusTools.LevelEnum pLevel, LevelStatusTools.StatusEnum pStatus, Cst.ErrLevel pCodeReturn)
        {
            m_Level = pLevel;
            m_Status = pStatus;
            m_CodeReturn = pCodeReturn;
        }
        #endregion Constructors
    }
    #endregion class LevelStatus

    #region ProcessState
    // EG 20190613 [24683] Upd
    [Serializable]
    public class ProcessState
    {
        #region Members

        /// <summary>
        /// Status spécifique au traitement en cours (sera utilisé pour gérer les déplacements des messages dans les folders appropriés (en mode FilWatcher) 
        /// </summary>
        private ProcessStateTools.StatusEnum m_CurrentStatus;


        #endregion Members

        #region Accessors
        /* FI 20200623 [XXXXX] Mise en commentaire
         #region SpheresException
        // PL 20130703 [WindowsEvent Project] Newness
        // EG 20190613 [24683] Upd
        [XmlIgnoreAttribute]
        public SpheresException LastSpheresException
        {
            set
            {
                m_LastSpheresException = value;
                if (alSpheresException == null)
                    alSpheresException = new ArrayList();
                alSpheresException.Add(value);
            }
            get { return m_LastSpheresException; }
        }
        // EG 20190613 [24683] Upd
        [XmlIgnoreAttribute]
        public ArrayList alSpheresException
        {
            set { m_alSpheresException = value; }
            get { return m_alSpheresException; }
        }
        #endregion SpheresException
        */


        /// <summary>
        /// Obtient la dernière exception stockée dans SpheresExceptions (la plus récente)
        /// </summary>
        /// PL 20130703 [WindowsEvent Project] Newness
        /// EG 20190613 [24683] Upd
        /// FI 20200623 [XXXXX] Refactoring  
        [XmlIgnoreAttribute]
        public SpheresException2 LastSpheresException2
        {
            get
            {
                return ArrFunc.Count(SpheresExceptions) > 0 ? SpheresExceptions.Last() : null;
            }
        }

        /// <summary>
        /// Obtient la liste des exceptions stockées
        /// </summary>
        /// EG 20190613 [24683] Upd
        /// FI 20200623 [XXXXX] Refactoring  
        [XmlIgnoreAttribute]
        public List<SpheresException2> SpheresExceptions
        {
            private set;
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20190613 [24683] Upd 
        [XmlAttribute(AttributeName = "codeReturn")]
        public Cst.ErrLevel CodeReturn
        {
            set;
            get;
        }
        /// <summary>
        /// Status général pour l'ensemble des traitements rattaché à une et une seule ligne du TRACKER
        /// </summary>
        // EG 20190613 [24683] Upd
        [XmlIgnoreAttribute()]
        public ProcessStateTools.StatusEnum Status
        {
            set;
            get;
        }

        /// <summary>
        ///Status spécifique au traitement en cours (sera utilisé pour gérer les déplacements des messages dans les folders appropriés (en mode FilWatcher) 
        /// </summary>
        /// EG 20130703 Test CurrentStatus à sa valeur par défaut = NA (1)
        // EG 20190613 [24683] Upd
        [XmlIgnoreAttribute]
        public ProcessStateTools.StatusEnum CurrentStatus
        {
            set { m_CurrentStatus = value; }
            get { return (m_CurrentStatus == 0) ? ProcessStateTools.StatusEnum.NA : m_CurrentStatus; }
        }


        // EG 20190613 [24683] Upd
        [XmlIgnoreAttribute]
        public int PostedSubMsg
        {
            set;
            get;
        }

        #endregion Accessors

        #region Constructors
        public ProcessState()
        {

        }
        public ProcessState(ProcessStateTools.StatusEnum pStatus) :
            this(pStatus, ProcessStateTools.CodeReturnUndefinedEnum)
        {
        }
        public ProcessState(ProcessStateTools.StatusEnum pStatus, Cst.ErrLevel pCodeReturn)
        {
            Status = pStatus;
            CodeReturn = pCodeReturn;
        }
        #endregion Constructors
        #region Methods


        /* FI 20200623 [XXXXX] Mise en commentaire
        /// <summary>
        /// Mise à jour du statut générale avec {pStatus} si le statut général n'est pas déjà en erreur et si {pStatus} a pour valeur WARNING ou ERROR
        /// </summary>
        /// FI 20131213 [19337] add summary
        public void SetStatus(string pStatus)
        {
            SetStatus(ProcessStateTools.ParseStatus(pStatus));
        }
        /// <summary>
        /// Mise à jour du statut générale avec {pStatus} si le statut général n'est pas déjà en erreur et si {pStatus} a pour valeur WARNING ou ERROR
        /// </summary>
        /// <param name="pStatus"></param>
        /// FI 20131213 [19337] add summary
        public void SetStatus(ProcessStateTools.StatusEnum pStatus)
        {
            if ((false == ProcessStateTools.IsStatusError(m_Status)) && ProcessStateTools.IsStatusErrorWarning(pStatus))
                m_Status = pStatus;
        }

   
        public void SetErrorWarning(string pStatus, string pCodeReturn)
        {
            SetErrorWarning(ProcessStateTools.ParseStatus(pStatus), ProcessStateTools.ParseCodeReturn(pCodeReturn));
        }
        */


        /// <summary>
        /// Mise à jour du statut générale avec {pStatus} si le statut général n'est pas déjà en erreur et si {pStatus} a pour valeur WARNING ou ERROR
        /// </summary>
        /// <param name="pStatus"></param>
        /// FI 20131213 [19337] add summary
        /// FI 20200623 [XXXXX] Rename en SetErrorWarning (ancien nom SetStatus)
        public void SetErrorWarning(ProcessStateTools.StatusEnum pStatus)
        {
            if ((false == ProcessStateTools.IsStatusError(Status)) && ProcessStateTools.IsStatusErrorWarning(pStatus))
                Status = pStatus;
        }

        /// <summary>
        /// Mise à jour du statut générale avec {pStatus} et du code retour avec {pCodeReturn} si le statut général n'est pas déjà en erreur et si {pStatus} a pour valeur WARNING ou ERROR
        /// </summary>
        /// <param name="pStatus"></param>
        /// <param name="pCodeReturn"></param>
        public void SetErrorWarning(ProcessStateTools.StatusEnum pStatus, Cst.ErrLevel pCodeReturn)
        {
            if ((false == ProcessStateTools.IsStatusError(Status)) && ProcessStateTools.IsStatusErrorWarning(pStatus))
            {
                CodeReturn = pCodeReturn;
                Status = pStatus;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// EG 20190308 New
        public void SetInterrupt()
        {
            CodeReturn = Cst.ErrLevel.IRQ_EXECUTED;
            Status = ProcessStateTools.StatusInterruptEnum;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pStatus"></param>
        /// FI 20200623 [XXXXX] Add (Déplacé ici, ancienenement dans processbase)
        public void SetProcessState(ProcessStateTools.StatusEnum pStatus)
        {
            if (false == ProcessStateTools.IsStatusErrorWarning(Status))
                Status = pStatus;

            if (pStatus != ProcessStateTools.StatusEnum.PROGRESS)
                CurrentStatus = pStatus;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pStatus"></param>
        /// <param name="pCodeReturn"></param>
        /// EG 20180525 [23979] IRQ Processing
        /// FI 20200623 [XXXXX] Add (Déplacé ici, ancienenement dans processbase)
        public void SetProcessState(ProcessStateTools.StatusEnum pStatus, Cst.ErrLevel pCodeReturn)
        {
            if (false == ProcessStateTools.IsStatusErrorWarningInterrupt(Status) || (pCodeReturn == Cst.ErrLevel.IRQ_EXECUTED))
            {
                Status = pStatus;
                CodeReturn = pCodeReturn;
            }

            if (pStatus != ProcessStateTools.StatusEnum.PROGRESS)
                CurrentStatus = pStatus;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProcessState"></param>
        /// FI 20200623 [XXXXX] Add (Déplacé ici, ancienenement dans processbase)
        public void SetProcessState(ProcessState pProcessState)
        {
            SetProcessState(pProcessState.Status, pProcessState.CodeReturn);
        }



        /// <summary>
        /// Ajoute l'exception {pEx} dans une liste
        /// <para>L'exption {pEx} est convertie en SpheresException si elle n'est pas dans ce type</para>
        /// </summary>
        /// <param name="pEx"></param>
        /// FI 20200623 [XXXXX] Add
        public void AddException(Exception pEx)
        {
            if (SpheresExceptions == null)
                SpheresExceptions = new List<SpheresException2>();
            // 20200803 [XXXXX] Add lock pour la gestion du MT
            lock (SpheresExceptions)
            {
                if (pEx is SpheresException2)
                    SpheresExceptions.Add(pEx as SpheresException2);
                else
                    SpheresExceptions.Add(SpheresExceptionParser.GetSpheresException(null, pEx));
            }
        }


        /// <summary>
        /// Ajoute l'exception {pEx} dans une liste si elle est reconnue comme "critique" ( cad CSharpException ou RDBMSException ou system Exception)
        /// </summary>
        /// <param name="pEx"></param>
        /// FI 20200623 [XXXXX] Add
        public void AddCriticalException(Exception pEx)
        {
            if ((StrFunc.IsFilled(pEx.Message) && pEx.Message.Contains(Cst.SYSTEM_EXCEPTION)) ||
                (null != ExceptionTools.GetFirtsCSharpException(pEx)) || (null != ExceptionTools.GetFirstRDBMSException(pEx)))
                AddException(pEx);
        }

        /// <summary>
        /// Affecte la propriété CodeReturn à partir de la dernière exception stockée(*).  
        /// <para>Si la dernière exception stockée fait suite à un timeout (system ou SQL) ou à un deadlock (SQL) affecte la propriété CodeReturn avec Cst.ErrLevel.TIMEOUT ou Cst.ErrLevel.DEADLOCK</para>
        /// <para>(*)voir propriété LastSpheresException2</para>
        /// </summary>
        /// EG 20140220 timeout/deadlock avec espace
        /// FI 20140819 [20291] Modify
        /// FI 20200623 [XXXXX] Remplace SetCodeReturnFromLastException
        /// FI 20200708 [XXXXX] Méthode déplacée (anciennement présente dans processBase)
        public void SetCodeReturnFromLastException2()
        {
            if (null != LastSpheresException2)
            {
                SpheresException2 ex = LastSpheresException2;
                CodeReturn = ex.ProcessState.CodeReturn;

                if (ex.IsInnerException)
                {
                    Exception RDBMSException = ExceptionTools.GetFirstRDBMSException(ex.InnerException);
                    if (null != RDBMSException)
                    {
                        string message = ExceptionTools.GetMessageAndStackExtended(RDBMSException);

                        if (message.ToLower().Contains(" timeout "))
                            CodeReturn = Cst.ErrLevel.TIMEOUT;
                        // EG 20151123 Add Test en français
                        else if (message.ToLower().Contains(" deadlock ") || (message.ToLower().Contains(" a été bloquée ") && message.ToLower().Contains(" choisie comme victime ")))
                            CodeReturn = Cst.ErrLevel.DEADLOCK;
                    }
                    else if (null != ExceptionTools.GetFirstException(ex.InnerException, typeof(TimeoutException)))
                    {
                        // FI 20140819 [20291] add test sur TimeoutException pour être en phase avec DataAccessBase.AnalyseSQLException      
                        // Car Spheres® reposte le message si une exception TimeoutException est détectée (voir méthode SpheresServiceBase.ActiveProcess)
                        CodeReturn = Cst.ErrLevel.TIMEOUT;
                    }
                }
            }
        }
        #endregion Methods
    }
    #endregion ProcessState
    #region ProcessStateTools
    public sealed class ProcessStateTools
    {
        #region ProcessStateAssociateAttribute
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
        public sealed class ProcessStateAssociateAttribute : Attribute
        {
            #region Members
            private ReadyStateEnum m_ReadyState;
            #endregion Members
            #region Accessors
            public ReadyStateEnum ReadyState
            {
                get { return (m_ReadyState); }
                set { m_ReadyState = value; }
            }
            #endregion Accessors
        }
        #endregion ProcessStateAssociateAttribute
        #region HelpAssociateAttribute
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        public sealed class HelpAssociateAttribute : Attribute
        {
            #region Members
            private string m_Help;
            private string m_ColorName;
            #endregion Members
            #region Accessors
            public string Help
            {
                get { return (m_Help); }
                set { m_Help = value; }
            }
            public string ColorName
            {
                get { return (m_ColorName); }
                set { m_ColorName = value; }
            }
            #endregion Accessors
        }
        #endregion HelpAssociateAttribute
        //
        #region Members
        #region CodeReturnEnum
        //public enum CodeReturnEnum
        //{
        //    SUCCESS = -1,
        //    UNDEFINED = 0,
        //    BREAK = 1,
        //    SQLDEFINED = 5,
        //    USERDEFINED = 10,
        //    LOGINUNSUCCESSFUL = 30,
        //    LOGINSIMULTANEOUS_USER = 31,
        //    LOGINSIMULTANEOUS_ALLUSER = 32,
        //    ACCESDENIED = 35,
        //    LOCKUNSUCCESSFUL = 40,
        //    INCORRECTPARAMETER = 60,
        //    FOLDERNOTFOUND = 70,
        //    FILENOTFOUND = 71,
        //    URLNOTFOUND = 72,
        //    DATANOTFOUND = 73,
        //    DATAUNMATCH = 74,
        //    DATADISABLED = 75,
        //    DATAIGNORE = 76,
        //    MULTIDATAFOUND = 77,
        //    BUG = 80,
        //    TIMEOUT = 82,
        //    DEADLOCK = 84,
        //    IRVIOLATION = 86,
        //    MISCELLANEOUS = 99
        //}
        #endregion CodeReturnEnum
        #region ReadyStateEnum
        /// EG 20161122 Add FlagsAttribute
        /// EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        /// FI 20201102 [XXXXX] use ResourceAttribut
        public enum ReadyStateEnum
        {
            [ResourceAttribut(Resource = "ReadyStateREQUESTED")]
            [HelpAssociate(Help = "HELP_TRACKERREADYSTATE_REQUESTED", ColorName = "Gray")]
            REQUESTED = 0x01,
            [ResourceAttribut(Resource = "ReadyStateACTIVE")]
            [HelpAssociate(Help = "HELP_TRACKERREADYSTATE_ACTIVE", ColorName = "Violet")]
            ACTIVE = 0x02,
            [ResourceAttribut(Resource = "ReadyStateTERMINATED")]
            [HelpAssociate(Help = "HELP_TRACKERREADYSTATE_TERMINATED", ColorName = "Blue")]
            TERMINATED = 0x04,
        }
        #endregion ReadyStateEnum
        #region StatusEnum



        /// <summary>
        /// 
        /// </summary>
        /// EG 20161122 Add FlagsAttribute
        /// EG 20180525 [23979] IRQ Processing Add IRQ [FlagsAttribute]
        /// EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        /// FI 20201030 [XXXXX] ResourceAttribut
        public enum StatusEnum
        {
            [ProcessStateAssociate(ReadyState = ReadyStateEnum.REQUESTED)]
            [ResourceAttribut(Resource = "TRACKERSTATUS_NA")]
            [HelpAssociate(Help = "HELP_TRACKERSTATUS_NA", ColorName = "Gray")]
            NA = 0x01,

            [ProcessStateAssociate(ReadyState = ReadyStateEnum.TERMINATED)]
            [ResourceAttribut(Resource = "TRACKERSTATUS_ERROR")]
            [HelpAssociate(Help = "HELP_TRACKERSTATUS_ERROR", ColorName = "Red")]
            ERROR = 0x02,

            [ProcessStateAssociate(ReadyState = ReadyStateEnum.ACTIVE)]
            [ResourceAttribut(Resource = "TRACKERSTATUS_PENDING")]
            [HelpAssociate(Help = "HELP_TRACKERSTATUS_PENDING", ColorName = "Marron")]
            PENDING = 0x04,

            [ProcessStateAssociate(ReadyState = ReadyStateEnum.ACTIVE)]
            [ResourceAttribut(Resource = "TRACKERSTATUS_PROGRESS")]
            [HelpAssociate(Help = "HELP_TRACKERSTATUS_PROGRESS", ColorName = "Violet")]
            PROGRESS = 0x08,

            [ResourceAttribut(Resource = "TRACKERSTATUS_SUCCESS")]
            [HelpAssociate(Help = "HELP_TRACKERSTATUS_SUCCESS", ColorName = "Green")]
            [ProcessStateAssociate(ReadyState = ReadyStateEnum.TERMINATED)]
            SUCCESS = 0x10,

            [ProcessStateAssociate(ReadyState = ReadyStateEnum.TERMINATED)]
            [ResourceAttribut(Resource = "TRACKERSTATUS_WARNING")]
            [HelpAssociate(Help = "HELP_TRACKERSTATUS_WARNING", ColorName = "Orange")]
            WARNING = 0x20,

            [ProcessStateAssociate(ReadyState = ReadyStateEnum.TERMINATED)]
            [ResourceAttribut(Resource = "TRACKERSTATUS_NONE")]
            [HelpAssociate(Help = "HELP_TRACKERSTATUS_NONE", ColorName = "Blue")]
            NONE = 0x40,

            [ProcessStateAssociate(ReadyState = ReadyStateEnum.TERMINATED)]
            [ResourceAttribut(Resource = "TRACKERSTATUS_IRQ")]
            [HelpAssociate(Help = "HELP_TRACKERSTATUS_IRQ", ColorName = "Black")] // FI 20201103 color Black
            IRQ = 0x80,
        }
        #endregion StatusEnum
        #endregion Members
        //
        #region Accessors
        #region CodeReturn
        // EG 20180525 [23979] IRQ Processing
        public static string CodeReturnSuccess { get { return Cst.ErrLevel.SUCCESS.ToString(); } }
        public static string CodeReturnUndefined { get { return Cst.ErrLevel.UNDEFINED.ToString(); } }
        public static string CodeReturnBreak { get { return Cst.ErrLevel.BREAK.ToString(); } }
        public static string CodeReturnSqlDefined { get { return Cst.ErrLevel.SQLDEFINED.ToString(); } }
        public static string CodeReturnUserDefined { get { return Cst.ErrLevel.USERDEFINED.ToString(); } }
        public static string CodeReturnLoginUnsuccessful { get { return Cst.ErrLevel.LOGINUNSUCCESSFUL.ToString(); } }
        public static string CodeReturnLoginSimultaneous_User { get { return Cst.ErrLevel.LOGINSIMULTANEOUS_USER.ToString(); } }
        public static string CodeReturnLoginSimultaneous_AllUser { get { return Cst.ErrLevel.LOGINSIMULTANEOUS_ALLUSER.ToString(); } }
        public static string CodeReturnAccessDenied { get { return Cst.ErrLevel.ACCESDENIED.ToString(); } }
        public static string CodeReturnLockUnsuccessful { get { return Cst.ErrLevel.LOCKUNSUCCESSFUL.ToString(); } }
        public static string CodeReturnMOMUnknown { get { return Cst.ErrLevel.MOM_UNKNOWN.ToString(); } }
        public static string CodeReturnMOMPathError { get { return Cst.ErrLevel.MOM_PATH_ERROR.ToString(); } }
        public static string CodeReturnInitializeError { get { return Cst.ErrLevel.INITIALIZE_ERROR.ToString(); } }
        public static string CodeReturnMessageNotFound { get { return Cst.ErrLevel.MESSAGE_NOTFOUND.ToString(); } }
        public static string CodeReturnMessageMove { get { return Cst.ErrLevel.MESSAGE_MOVE_ERROR.ToString(); } }
        public static string CodeReturnMessageRead { get { return Cst.ErrLevel.MESSAGE_READ_ERROR.ToString(); } }
        public static string CodeReturnMessageCast { get { return Cst.ErrLevel.MESSAGE_CAST_ERROR.ToString(); } }

        public static string CodeReturnIncorrectParameter { get { return Cst.ErrLevel.INCORRECTPARAMETER.ToString(); } }
        public static string CodeReturnMissingParameter { get { return Cst.ErrLevel.MISSINGPARAMETER.ToString(); } }
        public static string CodeReturnFolderNotFound { get { return Cst.ErrLevel.FOLDERNOTFOUND.ToString(); } }
        public static string CodeReturnFileNotFound { get { return Cst.ErrLevel.FILENOTFOUND.ToString(); } }
        public static string CodeReturnUrlNotFound { get { return Cst.ErrLevel.URLNOTFOUND.ToString(); } }
        //public static string CodeReturnDataNotFound { get { return Cst.ErrLevel.DATANOTFOUND.ToString(); } }
        public static string CodeReturnDataUnMatch { get { return Cst.ErrLevel.DATAUNMATCH.ToString(); } }
        public static string CodeReturnDataDisabled { get { return Cst.ErrLevel.DATADISABLED.ToString(); } }
        public static string CodeReturnDataIgnore { get { return Cst.ErrLevel.DATAIGNORE.ToString(); } }
        public static string CodeReturnMultiDataFound { get { return Cst.ErrLevel.MULTIDATAFOUND.ToString(); } }
        public static string CodeReturnFailure { get { return Cst.ErrLevel.FAILURE.ToString(); } }
        public static string CodeReturnTimeOut { get { return Cst.ErrLevel.TIMEOUT.ToString(); } }
        public static string CodeReturnDeadLock { get { return Cst.ErrLevel.DEADLOCK.ToString(); } }
        public static string CodeReturnIrViolation { get { return Cst.ErrLevel.IRVIOLATION.ToString(); } }
        public static string CodeReturnAborted { get { return Cst.ErrLevel.ABORTED.ToString(); } }
        public static string CodeReturnNoBookManaged { get { return Cst.ErrLevel.NOBOOKMANAGED.ToString(); } }
        public static string CodeReturnNothingToDo { get { return Cst.ErrLevel.NOTHINGTODO.ToString(); } }

        public static Cst.ErrLevel CodeReturnMissingParameterEnum { get { return Cst.ErrLevel.MISSINGPARAMETER; } }
        public static Cst.ErrLevel CodeReturnTuningIgnoreEnum { get { return Cst.ErrLevel.TUNING_IGNORE; } }
        public static Cst.ErrLevel CodeReturnTuningIgnoreForcedEnum { get { return Cst.ErrLevel.TUNING_IGNOREFORCED; } }
        public static Cst.ErrLevel CodeReturnTuningUnmatchEnum { get { return Cst.ErrLevel.TUNING_UNMATCH; } }
        public static Cst.ErrLevel CodeReturnSuccessEnum { get { return Cst.ErrLevel.SUCCESS; } }
        public static Cst.ErrLevel CodeReturnUndefinedEnum { get { return Cst.ErrLevel.UNDEFINED; } }
        public static Cst.ErrLevel CodeReturnBreakEnum { get { return Cst.ErrLevel.BREAK; } }
        public static Cst.ErrLevel CodeReturnSqlDefinedEnum { get { return Cst.ErrLevel.SQLDEFINED; } }
        public static Cst.ErrLevel CodeReturnUserDefinedEnum { get { return Cst.ErrLevel.USERDEFINED; } }
        public static Cst.ErrLevel CodeReturnLoginUnsuccessfulEnum { get { return Cst.ErrLevel.LOGINUNSUCCESSFUL; } }
        public static Cst.ErrLevel CodeReturnLoginSimultaneous_UserEnum { get { return Cst.ErrLevel.LOGINSIMULTANEOUS_USER; } }
        public static Cst.ErrLevel CodeReturnLoginSimultaneous_AllUserEnum { get { return Cst.ErrLevel.LOGINSIMULTANEOUS_ALLUSER; } }
        public static Cst.ErrLevel CodeReturnAccessDeniedEnum { get { return Cst.ErrLevel.ACCESDENIED; } }
        public static Cst.ErrLevel CodeReturnLockUnsuccessfulEnum { get { return Cst.ErrLevel.LOCKUNSUCCESSFUL; } }
        public static Cst.ErrLevel CodeReturnMOMUnknownEnum { get { return Cst.ErrLevel.MOM_UNKNOWN; } }
        public static Cst.ErrLevel CodeReturnMOMPathErrorEnum { get { return Cst.ErrLevel.MOM_PATH_ERROR; } }
        public static Cst.ErrLevel CodeReturnInitializeErrorEnum { get { return Cst.ErrLevel.INITIALIZE_ERROR; } }
        public static Cst.ErrLevel CodeReturnMessageNotFoundEnum { get { return Cst.ErrLevel.MESSAGE_NOTFOUND; } }
        public static Cst.ErrLevel CodeReturnMessageMoveEnum { get { return Cst.ErrLevel.MESSAGE_MOVE_ERROR; } }
        public static Cst.ErrLevel CodeReturnMessageReadEnum { get { return Cst.ErrLevel.MESSAGE_READ_ERROR; } }
        public static Cst.ErrLevel CodeReturnMessageCastEnum { get { return Cst.ErrLevel.MESSAGE_CAST_ERROR; } }
        public static Cst.ErrLevel CodeReturnMessageNotConformEnum { get { return Cst.ErrLevel.MESSAGE_NOTCONFORM; } }
        public static Cst.ErrLevel CodeReturnIncorrectParameterEnum { get { return Cst.ErrLevel.INCORRECTPARAMETER; } }
        public static Cst.ErrLevel CodeReturnFolderNotFoundEnum { get { return Cst.ErrLevel.FOLDERNOTFOUND; } }
        public static Cst.ErrLevel CodeReturnFileNotFoundEnum { get { return Cst.ErrLevel.FILENOTFOUND; } }
        public static Cst.ErrLevel CodeReturnUrlNotFoundEnum { get { return Cst.ErrLevel.URLNOTFOUND; } }
        public static Cst.ErrLevel CodeReturnDataNotFoundEnum { get { return Cst.ErrLevel.DATANOTFOUND; } }
        public static Cst.ErrLevel CodeReturnDataUnMatchEnum { get { return Cst.ErrLevel.DATAUNMATCH; } }
        public static Cst.ErrLevel CodeReturnDataDisabledEnum { get { return Cst.ErrLevel.DATADISABLED; } }
        public static Cst.ErrLevel CodeReturnDataIgnoreEnum { get { return Cst.ErrLevel.DATAIGNORE; } }
        public static Cst.ErrLevel CodeReturnMultiDataFoundEnum { get { return Cst.ErrLevel.MULTIDATAFOUND; } }
        public static Cst.ErrLevel CodeReturnFailureEnum { get { return Cst.ErrLevel.FAILURE; } }
        public static Cst.ErrLevel CodeReturnTimeOutEnum { get { return Cst.ErrLevel.TIMEOUT; } }
        public static Cst.ErrLevel CodeReturnDeadLockEnum { get { return Cst.ErrLevel.DEADLOCK; } }
        public static Cst.ErrLevel CodeReturnIrViolationEnum { get { return Cst.ErrLevel.IRVIOLATION; } }
        public static Cst.ErrLevel CodeReturnAbortedEnum { get { return Cst.ErrLevel.ABORTED; } }
        public static Cst.ErrLevel CodeReturnNoBookManagedEnum { get { return Cst.ErrLevel.NOBOOKMANAGED; } }
        public static Cst.ErrLevel CodeReturnNothingToDoEnum { get { return Cst.ErrLevel.NOTHINGTODO; } }
        public static Cst.ErrLevel CodeReturnQuoteNotFoundEnum { get { return Cst.ErrLevel.QUOTENOTFOUND; } }
        public static Cst.ErrLevel CodeReturnIRQExecutedEnum { get { return Cst.ErrLevel.IRQ_EXECUTED; } }
        #endregion CodeReturn
        #region ReadyState
        public static string ReadyStateActive { get { return ReadyStateEnum.ACTIVE.ToString(); } }
        public static string ReadyStateRequested { get { return ReadyStateEnum.REQUESTED.ToString(); } }
        public static string ReadyStateTerminated { get { return ReadyStateEnum.TERMINATED.ToString(); } }

        public static ReadyStateEnum ReadyStateActiveEnum { get { return ReadyStateEnum.ACTIVE; } }
        public static ReadyStateEnum ReadyStateRequestedEnum { get { return ReadyStateEnum.REQUESTED; } }
        public static ReadyStateEnum ReadyStateTerminatedEnum { get { return ReadyStateEnum.TERMINATED; } }
        #endregion ReadyState
        #region Status
        // EG 20180525 [23979] IRQ Processing
        public static string StatusError { get { return StatusEnum.ERROR.ToString(); } }
        public static string StatusPending { get { return StatusEnum.PENDING.ToString(); } }
        public static string StatusProgress { get { return StatusEnum.PROGRESS.ToString(); } }
        public static string StatusUnknown { get { return StatusEnum.NA.ToString(); } }
        public static string StatusSuccess { get { return StatusEnum.SUCCESS.ToString(); } }
        public static string StatusWarning { get { return StatusEnum.WARNING.ToString(); } }
        public static string StatusNone { get { return StatusEnum.NONE.ToString(); } }
        public static string StatusInterrupt { get { return StatusEnum.IRQ.ToString(); } }

        public static StatusEnum StatusErrorEnum { get { return StatusEnum.ERROR; } }
        public static StatusEnum StatusPendingEnum { get { return StatusEnum.PENDING; } }
        public static StatusEnum StatusProgressEnum { get { return StatusEnum.PROGRESS; } }
        public static StatusEnum StatusUnknownEnum { get { return StatusEnum.NA; } }
        public static StatusEnum StatusSuccessEnum { get { return StatusEnum.SUCCESS; } }
        public static StatusEnum StatusWarningEnum { get { return StatusEnum.WARNING; } }
        public static StatusEnum StatusNoneEnum { get { return StatusEnum.NONE; } }
        public static StatusEnum StatusInterruptEnum { get { return StatusEnum.IRQ; } }
        #endregion Status
        #endregion Accessors
        //
        #region Methods
        #region GetCodeReturn
        public static Cst.ErrLevel ParseCodeReturn(string pCodeReturn)
        {
            if (Enum.IsDefined(typeof(Cst.ErrLevel), pCodeReturn))
                return (Cst.ErrLevel)Enum.Parse(typeof(Cst.ErrLevel), pCodeReturn, true);
            else
                return (Cst.ErrLevel)Enum.Parse(typeof(Cst.ErrLevel), ProcessStateTools.CodeReturnUndefined, true);
        }
        #endregion GetCodeReturn
        #region GetReadyState
        public static ProcessStateTools.ReadyStateEnum ParseReadyState(string pReadyState)
        {
            if (Enum.IsDefined(typeof(ProcessStateTools.ReadyStateEnum), pReadyState))
                return (ProcessStateTools.ReadyStateEnum)Enum.Parse(typeof(ProcessStateTools.ReadyStateEnum), pReadyState, true);
            else
                return (ProcessStateTools.ReadyStateEnum)Enum.Parse(typeof(ProcessStateTools.ReadyStateEnum), ProcessStateTools.ReadyStateRequested, true);
        }
        #endregion GetReadyState
        #region GetStatus
        public static ProcessStateTools.StatusEnum ParseStatus(string pStatus)
        {
            if (Enum.IsDefined(typeof(ProcessStateTools.StatusEnum), pStatus))
                return (ProcessStateTools.StatusEnum)Enum.Parse(typeof(ProcessStateTools.StatusEnum), pStatus, true);
            else
                return (ProcessStateTools.StatusEnum)Enum.Parse(typeof(ProcessStateTools.StatusEnum), ProcessStateTools.StatusUnknown, true);
        }
        #endregion GetStatus

        #region IsCodeReturn...
        /// <summary>
        /// Traitement en attente
        /// </summary>
        /// <param name="pCodeReturn"></param>
        /// <returns></returns>
        // EG 20180525 [23979] IRQ Processing 
        public static bool IsCodeReturnUnmatchTuning(Cst.ErrLevel pCodeReturn) { return (CodeReturnTuningUnmatchEnum == pCodeReturn); }
        /// <summary>
        /// Traitement ignoré
        /// </summary>
        /// <param name="pCodeReturn"></param>
        /// <returns></returns>
        public static bool IsCodeReturnIgnoreTuning(Cst.ErrLevel pCodeReturn) { return (CodeReturnTuningIgnoreEnum == pCodeReturn); }
        public static bool IsCodeReturnLockUnsuccessful(Cst.ErrLevel pCodeReturn) { return (CodeReturnLockUnsuccessfulEnum == pCodeReturn); }
        public static bool IsCodeReturnSuccess(string pCodeReturn) { return CodeReturnSuccess == pCodeReturn; }
        public static bool IsCodeReturnSuccess(Cst.ErrLevel pCodeReturn) { return CodeReturnSuccessEnum == pCodeReturn; }
        public static bool IsCodeReturnUnsuccessful(string pCodeReturn) { return CodeReturnSuccess != pCodeReturn; }
        public static bool IsCodeReturnUnsuccessful(Cst.ErrLevel pCodeReturn) { return CodeReturnSuccessEnum != pCodeReturn; }
        public static bool IsCodeReturnUndefined(string pCodeReturn) { return CodeReturnUndefined == pCodeReturn; }
        public static bool IsCodeReturnUndefined(Cst.ErrLevel pCodeReturn) { return CodeReturnUndefinedEnum == pCodeReturn; }
        public static bool IsCodeReturnNothingToDo(string pCodeReturn) { return CodeReturnNothingToDo == pCodeReturn; }
        public static bool IsCodeReturnNothingToDo(Cst.ErrLevel pCodeReturn) { return CodeReturnNothingToDoEnum == pCodeReturn; }
        public static bool IsCodeReturnIRQExecuted(Cst.ErrLevel pCodeReturn) { return CodeReturnIRQExecutedEnum == pCodeReturn; }
        #endregion IsCodeReturn...
        #region IsReadyState...
        public static bool IsReadyStateActive(string pReadyState) { return ReadyStateActive == pReadyState; }
        public static bool IsReadyStateRequested(string pReadyState) { return ReadyStateRequested == pReadyState; }
        public static bool IsReadyStateTerminated(string pReadyState) { return ReadyStateTerminated == pReadyState; }

        public static bool IsReadyStateActive(ReadyStateEnum pReadyState) { return ReadyStateActiveEnum == pReadyState; }
        public static bool IsReadyStateRequested(ReadyStateEnum pReadyState) { return ReadyStateRequestedEnum == pReadyState; }
        public static bool IsReadyStateTerminated(ReadyStateEnum pReadyState) { return ReadyStateTerminatedEnum == pReadyState; }
        #endregion IsReadyState...
        #region IsStatus...
        public static bool IsStatusError(string pStatus) { return StatusError == pStatus; }
        public static bool IsStatusPending(string pStatus) { return StatusPending == pStatus; }
        public static bool IsStatusProgress(string pStatus) { return StatusProgress == pStatus; }
        public static bool IsStatusUnknown(string pStatus) { return StatusUnknown == pStatus; }
        public static bool IsStatusSuccess(string pStatus) { return StatusSuccess == pStatus; }
        public static bool IsStatusNone(string pStatus) { return StatusNone == pStatus; }
        public static bool IsStatusWarning(string pStatus) { return StatusWarning == pStatus; }
        public static bool IsStatusErrorWarning(string pStatus)
        {
            return IsStatusError(pStatus) || IsStatusWarning(pStatus);
        }

        public static bool IsStatusErrorWarningInterrupt(string pStatus)
        {
            return IsStatusError(pStatus) || IsStatusWarning(pStatus) || IsStatusInterrupt(pStatus);
        }
        public static bool IsStatusInterrupt(string pStatus) { return StatusInterrupt == pStatus; }
        public static bool IsStatusError(StatusEnum pStatus) { return StatusErrorEnum == pStatus; }
        public static bool IsStatusPending(StatusEnum pStatus) { return StatusPendingEnum == pStatus; }
        public static bool IsNOTStatusPending(StatusEnum pStatus) { return StatusPendingEnum != pStatus; }
        public static bool IsStatusProgress(StatusEnum pStatus) { return StatusProgressEnum == pStatus; }
        public static bool IsStatusUnknown(StatusEnum pStatus) { return StatusUnknownEnum == pStatus; }
        public static bool IsStatusSuccess(StatusEnum pStatus) { return StatusSuccessEnum == pStatus; }
        public static bool IsStatusNone(StatusEnum pStatus) { return StatusNoneEnum == pStatus; }
        public static bool IsStatusWarning(StatusEnum pStatus) { return StatusWarningEnum == pStatus; }
        public static bool IsStatusErrorWarning(StatusEnum pStatus)
        {
            return IsStatusError(pStatus) || IsStatusWarning(pStatus);
        }
        public static bool IsStatusErrorWarningInterrupt(StatusEnum pStatus)
        {
            return IsStatusError(pStatus) || IsStatusWarning(pStatus) || IsStatusInterrupt(pStatus);
        }
        public static bool IsStatusInterrupt(StatusEnum pStatus) { return StatusInterruptEnum == pStatus; }
        /// <summary>
        /// Retourne true si <paramref name="pStatus"/> vaut ERROR ou WARNING ou SUCCESS ou NONE ou IRQ
        /// </summary>
        /// <param name="pStatus"></param>
        /// <returns></returns>
        public static bool IsStatusTerminated(StatusEnum pStatus)
        {
            // FI 20221205 [XXXXX] Usage de ReflectionTools.GetAttribute
            ProcessStateTools.ProcessStateAssociateAttribute stateAssociate = ReflectionTools.GetAttribute<ProcessStateTools.ProcessStateAssociateAttribute>(pStatus);
            return (stateAssociate.ReadyState == ReadyStateEnum.TERMINATED);
        }
        #endregion
        #region GetStatusCssClass
        public static string GetStatusCssClass(string pStatus)
        {
            return GetStatusCssClass(ParseStatus(pStatus));
        }
        private static string GetStatusCssClass(StatusEnum pStatus)
        {
            return "status" + pStatus.ToString().ToLower();
        }
        #endregion GetStatusCssClass
        #region GetForeColorMessage
        public static Color GetForeColorMessage(string pStatus)
        {
            return GetForeColorMessage(ParseStatus(pStatus));
        }
        // EG 20180525 [23979] IRQ Processing
        public static Color GetForeColorMessage(StatusEnum pStatus)
        {
            Color foreColor = Color.Black;
            switch (pStatus)
            {
                case ProcessStateTools.StatusEnum.ERROR:
                    foreColor = CstCSSColor.Convert(CstCSSColor.red);
                    break;
                case ProcessStateTools.StatusEnum.IRQ:
                    foreColor = CstCSSColor.Convert(CstCSSColor.black);
                    break;
                case ProcessStateTools.StatusEnum.NA:
                    foreColor = CstCSSColor.Convert(CstCSSColor.grayMedium);
                    break;
                case ProcessStateTools.StatusEnum.NONE:
                    foreColor = CstCSSColor.Convert(CstCSSColor.blue);
                    break;
                case ProcessStateTools.StatusEnum.PENDING:
                    foreColor = CstCSSColor.Convert(CstCSSColor.marron);
                    break;
                case ProcessStateTools.StatusEnum.PROGRESS:
                    foreColor = CstCSSColor.Convert(CstCSSColor.violet);
                    break;
                case ProcessStateTools.StatusEnum.SUCCESS:
                    foreColor = CstCSSColor.Convert(CstCSSColor.green);
                    break;
                case ProcessStateTools.StatusEnum.WARNING:
                    foreColor = CstCSSColor.Convert(CstCSSColor.orange);
                    break;
            }
            return foreColor;
        }
        #endregion

        #region GetPowerOfAllStatusByReadyState
        public static int GetPowerOfAllStatusByReadyState(ProcessStateTools.ReadyStateEnum pReadyState)
        {
            ProcessStateTools.StatusEnum statusEnum = new ProcessStateTools.StatusEnum();
            FieldInfo[] statusFlds = statusEnum.GetType().GetFields();
            int powerStatus = 0;
            foreach (FieldInfo statusFld in statusFlds)
            {
                object[] readyStateAssociateAttrs = statusFld.GetCustomAttributes(typeof(ProcessStateTools.ProcessStateAssociateAttribute), true);
                if (0 < readyStateAssociateAttrs.Length)
                {
                    foreach (ProcessStateTools.ProcessStateAssociateAttribute readyStateAssociate in readyStateAssociateAttrs)
                    {
                        if (pReadyState == readyStateAssociate.ReadyState)
                        {
                            ProcessStateTools.StatusEnum status =
                                (ProcessStateTools.StatusEnum)Enum.Parse(typeof(ProcessStateTools.StatusEnum), statusFld.Name, false);
                            int i = int.Parse(Enum.Format(typeof(ProcessStateTools.StatusEnum), status, "d"));
                            powerStatus += (int)Math.Pow(2, i);
                        }
                    }
                }
            }
            return powerStatus;
        }
        #endregion GetPowerOfAllStatusByReadyState

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pReadyState"></param>
        /// <param name="pPowerStatus"></param>
        /// <returns></returns>
        public static string[] GetStatusByReadyState(ProcessStateTools.ReadyStateEnum pReadyState, int pPowerStatus)
        {
            string[] ret = null;
            //
            ArrayList aStatus = new ArrayList();
            ProcessStateTools.StatusEnum statusEnum = new ProcessStateTools.StatusEnum();
            FieldInfo[] statusFlds = statusEnum.GetType().GetFields();
            foreach (FieldInfo statusFld in statusFlds)
            {
                object[] processStateAssociateAttrs = statusFld.GetCustomAttributes(typeof(ProcessStateTools.ProcessStateAssociateAttribute), true);
                if (ArrFunc.Count(processStateAssociateAttrs) > 0)
                {
                    foreach (ProcessStateTools.ProcessStateAssociateAttribute processState in processStateAssociateAttrs)
                    {
                        if (pReadyState == processState.ReadyState)
                        {
                            ProcessStateTools.StatusEnum status =
                                (ProcessStateTools.StatusEnum)Enum.Parse(typeof(ProcessStateTools.StatusEnum), statusFld.Name, false);
                            int i = int.Parse(Enum.Format(typeof(ProcessStateTools.StatusEnum), status, "d"));
                            bool isOk = (0 < (pPowerStatus & Convert.ToInt32(Math.Pow(2, i))));
                            if (isOk)
                                aStatus.Add(statusFld.Name);
                        }
                    }
                }
            }
            //
            if (aStatus.Count > 0)
                ret = (string[])aStatus.ToArray(typeof(string));
            //
            return ret;
        }

        /// <summary>
        /// Retourne la liste des status associés à {pReadyState}
        /// </summary>
        /// <param name="pReadyState"></param>
        /// <returns></returns>
        /// FI 20141103 [XXXXX] Add Method
        public static string[] GetStatusByReadyState(ProcessStateTools.ReadyStateEnum pReadyState)
        {
            string[] ret = null;

            ArrayList aStatus = new ArrayList();

            ProcessStateTools.StatusEnum statusEnum = new ProcessStateTools.StatusEnum();
            FieldInfo[] statusFlds = statusEnum.GetType().GetFields();
            foreach (FieldInfo statusFld in statusFlds)
            {
                object[] processStateAssociateAttrs = statusFld.GetCustomAttributes(typeof(ProcessStateTools.ProcessStateAssociateAttribute), true);
                if (ArrFunc.Count(processStateAssociateAttrs) > 0)
                {
                    foreach (ProcessStateTools.ProcessStateAssociateAttribute processState in processStateAssociateAttrs)
                    {
                        if (pReadyState == processState.ReadyState)
                            aStatus.Add(statusFld.Name);
                    }
                }
            }

            if (aStatus.Count > 0)
                ret = (string[])aStatus.ToArray(typeof(string));

            return ret;
        }
        #endregion Methods

    }
    #endregion ProcessStateTools

    /// <summary>
    /// Gestion du temps qui coule à partir d'une date de référence 
    /// </summary>
    public class DatetimeProfiler
    {
        #region members
        /// <summary>
        /// Date start de reference
        /// </summary>
        private DateTime _dtStart;
        /// <summary>
        /// Date system sur le Serveur applicatif 
        /// </summary>
        private DateTime _dtStartOS;
        #endregion members

        #region Accessor
        /// <summary>
        /// Obtient la date de référence initiale
        /// </summary>
        public DateTime DtStart
        {
            get { return _dtStart; }
        }
        #endregion

        #region constructor
        /// <summary>
        /// constructeur avec Initialisation de la date de reference à DateTime.MinValue
        /// </summary>
        public DatetimeProfiler()
            : this(DateTime.MinValue)
        {
        }
        /// <summary>
        /// constructeur avec Initialisation de la date de reference à {pDateStart}
        /// </summary>
        /// <param name="pDateStart"></param>
        public DatetimeProfiler(DateTime pDateStart)
        {
            Start(pDateStart);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initialisation de la date de reference
        /// </summary>
        /// <param name="pDateStart"></param>
        public void Start(DateTime pDateStart)
        {
            _dtStart = pDateStart;
            _dtStartOS = DateTime.Now;
        }

        /// <summary>
        /// Retourne la date start augmentée du temps écoulé
        /// </summary>
        /// <returns></returns>
        public DateTime GetDate()
        {
            return _dtStart.Add(GetTimeSpan());
        }

        /// <summary>
        /// Donne le delai écoulé depuis le start
        /// </summary>
        /// <returns></returns>
        public TimeSpan GetTimeSpan()
        {
            return new TimeSpan(DateTime.Now.Ticks - _dtStartOS.Ticks);
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20230119 [XXXXX] Add
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class IdMenuAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public String IdMenu
        {
            get;
            set;
        }
    }

    
    /// <summary>
    /// 
    /// </summary>
    public class IdMenu
    {
        /// <summary>
        /// Paramètre à rajouter dans l'URL de certains menus quand il s'agit d'un Software autre que OTCml/FOmL
        /// <para>Ainsi, dans les scripts SQL d'affichage du contenu du référentiel, la référence à des tables business (TARDE, MATURITY, ...) sera néttoyée automatiquement dans le cas des software EFS (portail), Vision, ....</para>
        /// <para>On doit avoir dans le script SQL que des référence à tables systeme (ACTOR, ...) et à des tables de Spheres IO</para>
        /// </summary>    
        //Attention: cette constante est utilisée en dur dans:</para>
        //- Les fichiers XML descriptifs des consultations</para>
        //- Dans les URL de certains menus Spheres Vision et du Portail (VISION_ADM_SAF_LOG_LOGREQUEST,EFS_VIEW_LOGREQUEST, ...)</para>         
        public const string Param_ONLYSIO = "ONLYSIO";

        /// <summary>
        /// Menu de Spheres®
        /// </summary>
        /// EG 20141230 [20587]
        /// FI 20170313 [22225] Modify
        /// FI 20170928 [23452] Modify
        /// EG 20190214 New InputClosingReopeningPosition
        public enum Menu
        {
            /// <summary>
            /// 
            /// </summary>
            About,
            /// <summary>
            ///  Menu: Référentiel
            /// </summary>
            Repository,
            /// <summary>
            ///  Menu: Administration
            /// </summary>
            Admin,
            /// <summary>
            /// Menu: Liste des Ordres/Trades OTC (Gré à Gré)
            /// </summary>
            ViewTradeOTCOrder,
            /// <summary>
            /// Menu: Liste des Dépouillements (Gré à Gré)
            /// </summary>
            ViewTradeOTCAlloc,
            /// <summary>
            /// Menu: Liste des Dépouillements/Allocations  F&O (Dérivés listés)
            /// </summary>
            ViewTradeFnOAlloc,
            /// <summary>
            /// Menu: Liste des Ordres/Block trades F&O (Dérivés listés)
            /// </summary>
            ViewTradeFnOOrder,
            /// <summary>
            /// Menu : Saisie
            /// </summary>
            Input,
            /// <summary>
            ///  Menu: Référentiel Instrument
            /// </summary>
            Instrument,
            /// <summary>
            ///  Menu: Référentiel Instrument Strategy
            /// </summary>
            /// FI 20240703 [WI989] Add
            Strategy,
            /// <summary>
            ///  Menu: Group Instrument
            /// </summary>
            GrpInstrument,
            /// <summary>
            ///  Menu: Référentiel Actor
            /// </summary>
            Actor,
            /// <summary>
            ///  Menu: Group Actor
            /// </summary>
            GrpActor,
            /// <summary>
            ///  Menu: Référentiel Algorithm
            /// </summary>
            Algorithm, // FI 20170928 [23452] Add
            /// <summary>
            /// 
            /// </summary>
            ActorTaxCollected,
            /// <summary>
            /// Référentiel Book
            /// </summary>
            Book,
            /// <summary>
            /// Parent List
            /// </summary>
            ParentList, //LP 20240712 [WI1001] add parent list
            /// <summary>
            ///  Menu: Group Book
            /// </summary>
            GrpBook,
            /// <summary>
            /// Menu: Référentiel Asset Cash
            /// </summary>
            /// FI 20150928 [XXXXX]
            AssetCash,
            /// <summary>
            /// Menu: Référentiel Asset sur commodity
            /// </summary>
            AssetCommodity,
            // EG 20161122 New Commodity Derivative
            /// <summary>
            /// Menu: Référentiel Asset sur commodity contract
            /// </summary>
            AssetCommodityContract,
            /// <summary>
            /// Menu: Référentiel Asset action
            /// </summary>
            AssetEquity,
            /// <summary>
            /// Menu: Référentiel Asset action d'un panier
            /// </summary>
            AssetEquityBasketConstituent,
            /// <summary>
            /// Menu: Référentiel Asset taux de change
            /// </summary>
            AssetFxRate,
            /// <summary>
            /// Menu: Référentiel Asset de taux
            /// </summary>
            AssetRateIndex,
            /// <summary>
            /// Menu: Référentiel Asset sur indice
            /// </summary>
            AssetIndex,
            /// <summary>
            /// Menu: Référentiel Asset ETD
            /// </summary>
            AssetETD,
            /// <summary>
            /// Menu: Référentiel Asset ETF
            /// </summary>
            AssetExchangeTradedFund,
            /// <summary>
            /// Menu: Référentiel Asset environnement
            /// </summary>
            AssetEnv,
            /// <summary>
            /// Menu: Référentiel Business Center
            /// </summary>
            BusinessCenter,
            /// <summary>
            /// Menu: Référentiel Commodity Contract
            /// </summary>
            CommodityContract,
            /// <summary>
            /// Menu: Référentiel Contrat Dérivé  
            /// </summary>
            DrvContract,
            /// <summary>
            /// Menu: échéances ouvertes
            /// </summary>
            /// FI 20170313 [22225] Add
            DrvAttrib,
            /// <summary>
            /// 
            /// </summary>
            [IdMenu(IdMenu = "OTC_REF_PRD_ADM_DEFINEEXTEND")]
            DefineExtend,
            /// <summary>
            /// Menu: Référentiel Enums
            /// </summary>
            /// FI 20160804 [Migration TFS] add
            Enums,
            /// <summary>
            /// 
            /// </summary>
            ///
            Fee,
            /// <summary>
            /// 
            /// </summary>
            /// CC 20111025
            FeeMatrix,
            /// <summary>
            /// 
            /// </summary>
            FeeSchedule,
            /// <summary>
            /// Menu: Filtre d'accès aux acteurs
            /// </summary>
            FilterActor,
            /// <summary>
            /// Menu: Filtre d'accès aux instruments
            /// </summary>
            FilterInstrument,
            /// <summary>
            /// Menu: Filtre d'accès aux marchés
            /// </summary>
            FilterMarket,
            /// <summary>
            /// Menu: Filtre d'accès aux menus
            /// </summary>
            FilterMenu,
            /// <summary>
            /// Menu: Filtre d'accès aux fonctionnalités
            /// </summary>
            FilterPermission,
            /// <summary>
            /// Menu: Liste des Factures, Avoirs et Règlements
            /// </summary>
            ViewTradeAdmin,
            /// <summary>
            /// Menu: Référentiel des paramètres pour le déposit "CBOE Margin"
            /// Accessible depuis le menu détail du référentiel Contrats Dérivés
            /// Permet de définir des paramètres de calcul spécifiques à un contrat
            /// </summary>
            ImCboeContract,
            /// <summary>
            /// Menu: Référentiel des paramètres pour le déposit "CBOE Margin"
            /// Accessible depuis le menu détail du référentiel Marchés
            /// Permet de définir des paramètres de calcul spécifiques à une catégorie d'actif (ex. EquityAsset, ExchangeTradedFund,...)
            /// </summary>
            ImCboeMarket,
            /// <summary>
            /// Menu: Référentiel des paramètres pour le déposit "TIMS IDEM"
            /// Accessible depuis le menu détail du référentiel Contrats Dérivés
            /// Permet de définir des paramètres de calcul spécifiques à un contrat
            /// </summary>
            ImTimsIdemContract,
            /// <summary>
            /// Menu: Référentiel des paramètres pour le déposit "IMSM"
            /// Accessible depuis le menu détail du référentiel Commodity Contracts
            /// Permet de définir des paramètres de calcul spécifiques à un Commodity Contract
            // PM 20231129 [XXXXX][WI759] Ajout
            ImIMSMCESMCommodityContract,
            /// <summary>
            /// Menu: Saisie des Factures, Avoirs et Règlements
            /// </summary>
            InputTradeAdmin,
            /// <summary>
            /// Menu: Annulation d'un Trade administratif
            /// </summary>
            InputTradeAdmin_RMV,
            /// <summary>
            /// Menu: Correction de facture
            /// </summary>
            [Cst.TrackerSystemMsg(SysNumber = 5010)]
            InputTradeAdmin_COR,
            /// <summary>
            /// Menu: Validation de factures
            /// </summary>
            [Cst.TrackerSystemMsg(SysNumber = 5020)]
            InvoicingValidation,
            /// <summary>
            /// Menu: Suppression de factures simulées
            /// </summary>
            [Cst.TrackerSystemMsg(SysNumber = 5030)]
            InvoicingCancellation,
            /// <summary>
            /// Menu: Génération de factures
            /// </summary>
            InvoicingGeneration,
            /// <summary>
            /// Menu: Instructions de facturation
            /// </summary>
            InvoicingRules,
            /// <summary>
            /// Menu: Traitement Fin de jounée
            /// </summary>
            //ClosingDay,
            /// <summary>
            /// Menu: Référentiel Marché
            /// </summary>
            Market,
            /// <summary>
            /// Menu: Référentiel Indice
            /// </summary>
            /// FI 20190404 [XXXXX]Add
            RateIndex,
            /// <summary>
            /// Menu: Intégration d'un Corporate action (publiées)
            /// </summary>
            InputCorporateActionIssue,
            /// <summary>
            /// Menu: Intégration d'un Corporate action (intégrées)
            /// </summary>
            InputCorporateActionEmbedded,
            /// <summary>
            /// Menu: Intégration d'une demande de cloture/Réouverture de position
            /// </summary>
            InputClosingReopeningPosition,
            /// <summary>
            /// Menu: Saisie des trades
            /// </summary>
            InputTrade,
            /// <summary>
            /// Menu: Abandon
            /// </summary>
            [Cst.TrackerSystemMsg(SysNumber = 103)]
            InputTrade_ABN,
            /// <summary>
            /// Menu: Abandon (automatique)[Menu fictif pour récupérer une permission create]
            /// </summary>
            InputTrade_ABN_AUTOMATIC,
            /// <summary>
            /// Menu: Assignation
            /// </summary>
            [Cst.TrackerSystemMsg(SysNumber = 104)]
            InputTrade_ASS,
            /// <summary>
            /// Menu: Assignation (automatique)[Menu fictif pour récupérer une permission create]
            /// </summary>
            InputTrade_ASS_AUTOMATIC,
            /// <summary>
            /// Menu: Barrier & trigger
            /// </summary>
            [Cst.TrackerSystemMsg(SysNumber = 110)]
            InputTrade_BAR_TRG,
            /// <summary>
            /// Menu: Dénonciation
            /// </summary>
            [Cst.TrackerSystemMsg(SysNumber = 120)]
            InputTrade_CAL,
            /// <summary>
            /// Menu: Correction quantité
            /// </summary>
            InputTrade_POC,
            /// <summary>
            /// Menu: Annulation d'un Trade
            /// </summary>
            InputTrade_RMV,
            /// <summary>
            /// Menu: Fixing Clientèle
            /// </summary>
            [Cst.TrackerSystemMsg(SysNumber = 140)]
            InputTrade_CSR,
            /// <summary>
            /// Menu: Renouvellement
            /// </summary>
            [Cst.TrackerSystemMsg(SysNumber = 170)]
            InputTrade_REN,
            /// <summary>
            /// Menu: Exercice (Manuel)
            /// </summary>
            [Cst.TrackerSystemMsg(SysNumber = 105)]
            InputTrade_EXE,
            /// <summary>
            /// Menu: Clearing
            /// </summary>
            InputTrade_EXE_CLR,
            /// <summary>
            /// Menu: Exercice (automatique)[Menu fictif pour récupérer une permission create]
            /// </summary>
            InputTrade_EXE_AUTOMATIC,
            /// <summary>
            /// Menu: Levée anticipée
            /// </summary>
            [Cst.TrackerSystemMsg(SysNumber = 150)]
            InputTrade_EEX,
            /// <summary>
            /// Menu: Annulation avec Remplaçante
            /// </summary>
            InputTrade_REP,
            /// <summary>
            /// Menu: Resiliation
            /// </summary>
            [Cst.TrackerSystemMsg(SysNumber = 133)]
            InputTrade_RES,
            /// <summary>
            /// Menu: Resilitation (sans CashSetttlement)  (CancelableProvision)
            /// </summary>
            [Cst.TrackerSystemMsg(SysNumber = 130)]
            InputTrade_CAN,
            /// <summary>
            /// Menu (virtuel): Cascading (Utile au Frais sur l'action de Cascading)
            /// </summary>
            InputTrade_CASC,
            /// <summary>
            /// Menu: Resilitation obligatoire (avec CashSetttlement)  (MandatoryEarlyTerminationProvision)
            /// </summary>
            [Cst.TrackerSystemMsg(SysNumber = 131)]
            InputTrade_MET,
            /// <summary>
            /// Menu: Resilitation optionelle (avec CashSetttlement)  (OptionalEarlyTerminationProvision)
            /// </summary>
            [Cst.TrackerSystemMsg(SysNumber = 132)]
            InputTrade_OET,
            /// <summary>
            /// Menu: Prorogation (Extendible provision)
            /// </summary>
            [Cst.TrackerSystemMsg(SysNumber = 160)]
            InputTrade_PRO,
            /// <summary>
            /// Menu: Augmentation de capital (Step-Up provision)
            /// </summary>
            InputTrade_SUP,
            /// <summary>
            /// Menu: Position Transfert
            /// </summary>
            InputTrade_POT,
            /// <summary>
            /// Menu: TradeSplitting
            /// </summary>
            InputTrade_SPLIT,
            /// <summary>
            /// Menu: Clôture spécifique
            /// </summary>
            InputTrade_CLEARSPEC,
            /// <summary>
            /// Annulation Allocation
            /// </summary>
            InputTrade_RMVALLOC,
            /// <summary>
            /// Livraison
            /// <para>Utiliser pour les actions de livraison post échéance</para>
            /// </summary>
            // PM 20130822 [17949] Livraison Matif
            InputTrade_DLV,
            /// <summary>
            /// Menu: Saisie des évènements
            /// </summary>
            [Cst.TrackerSystemMsg(SysNumber = 3)]
            InputEvent,
            /// <summary>
            /// Menu: Saisie des titres
            /// </summary>
            InputDebtSec,
            /// EG 20170125 [Refactoring URL] New
            InputTradeRisk,
            /// <summary>
            /// Menu: Saisie des Deposits
            /// </summary>
            InputTradeRisk_InitialMargin,
            /// <summary>
            /// Menu: Correction d'un Deposit
            /// </summary>
            InputTradeRisk_InitialMargin_Correction,
            /// <summary>
            /// Menu: Saisie des Cash-Balance
            /// </summary>
            InputTradeRisk_CashBalance,
            /// <summary>
            /// Menu: Correction d'un Cash-Balance
            /// </summary>
            InputTradeRisk_CashBalance_Correction,
            /// <summary>
            /// Menu: Saisie des Cash-Payment (Versement)
            /// </summary>
            InputTradeRisk_CashPayment,
            /// <summary>
            /// Menu: Saisie des échelles d'intérêts
            /// </summary>
            InputTradeRisk_CashInterest,
            /// <summary>
            /// Menu: Correction d'un Cash-Payment
            /// </summary>
            InputTradeRisk_CashPayment_Correction,
            /// <summary>
            /// 
            /// </summary>
            Confirm,
            /// <summary>
            /// 
            /// </summary>
            AdminSafetyLoginLog,
            // EG 20151215 [21305] New
            UserLockoutLog,
            HostLockoutLog,
            /// <summary>
            /// 
            /// </summary>
            IOTASK,
            /// <summary>
            /// 
            /// </summary>
            IOTASKDET,
            /// <summary>
            /// 
            /// </summary>
            IOTRACK,
            /// <summary>
            /// 
            /// </summary>
            IOTRACKCOMPARE,
            /// <summary>
            /// 
            /// </summary>
            IOINPUT,
            /// <summary>
            /// 
            /// </summary>
            IOOUTPUT,
            /// <summary>
            /// 
            /// </summary>
            IOSHELL,
            /// <summary>
            /// 
            /// </summary>
            IOCOMPARE,
            /// <summary>
            /// 
            /// </summary>
            IOPARSING,
            /// <summary>
            /// 
            /// </summary>
            IOPARAM,
            /// <summary>
            /// 
            /// </summary>
            REQUEST_L,

            /// <summary>
            /// 
            /// </summary>
            PROCESS_L,
            /// <summary>
            /// 
            /// </summary>
            PROCESSDET_L,
            /// <summary>
            /// Menu : Cotations
            /// </summary>
            [Cst.TrackerSystemMsg(SysNumber = 401)]
            QUOTE_BOND_H,
            [Cst.TrackerSystemMsg(SysNumber = 402)]
            QUOTE_DEBTSEC_H,
            [Cst.TrackerSystemMsg(SysNumber = 403)]
            QUOTE_DEPOSIT_H,
            [Cst.TrackerSystemMsg(SysNumber = 404)]
            QUOTE_EQUITY_H,
            /// EG 20170125 [Refactoring URL] Upd
            QUOTE_EXTRDFUND_H,
            [Cst.TrackerSystemMsg(SysNumber = 405)]
            QUOTE_ETD_H,
            [Cst.TrackerSystemMsg(SysNumber = 406)]
            QUOTE_FXRATE_H,
            [Cst.TrackerSystemMsg(SysNumber = 407)]
            QUOTE_INDEX_H,
            [Cst.TrackerSystemMsg(SysNumber = 408)]
            QUOTE_RATEINDEX_H,
            [Cst.TrackerSystemMsg(SysNumber = 409)]
            QUOTE_SIMPLEFRA_H,
            [Cst.TrackerSystemMsg(SysNumber = 410)]
            QUOTE_SIMPLEIRS_H,
            /// <summary>
            /// 
            /// </summary>
            MATURITY,
            [Cst.TrackerSystemMsg(SysNumber = 420)]
            [IdMenu(IdMenu = "OTC_REF_LST_MATURITYRULE")]
            MATURITYRULE,
            [IdMenu(IdMenu = "OTC_ADM_SAF_RULE")]
            MODELSAFETY,
            /// <summary>
            /// 
            /// </summary>
            [Cst.TrackerSystemMsg(SysNumber = 430)]
            [IdMenu(IdMenu = "OTC_INP_POSCOLLATERAL")]
            POSCOLLATERAL,
            /// <summary>
            /// 
            /// </summary>
            POSCOLLATERALVAL,
            /// <summary>
            /// 
            /// </summary>
            SERVICE_L,
            /// <summary>
            /// 
            /// </summary>
            MARGINTRACK,
            /// <summary>
            /// Menu : Référentiel Net par convention
            /// </summary>
            NETCONVENTION,
            /// <summary>
            /// Menu : Référentiel Net par désignation
            /// </summary>
            NETDESIGNATION,
            /// <summary>
            /// 
            /// </summary>
            POSKEEPING_EOD,
            POSKEEPING_CLOSINGDAY,
            /// <summary>
            /// Mise à jour des clôtures 
            /// </summary>
            [Cst.TrackerSystemMsg(SysNumber = 4040)]
            POSKEEPING_UPDATENTRY,
            /// <summary>
            /// compensation globale
            /// </summary>
            [Cst.TrackerSystemMsg(SysNumber = 200)]
            POSKEEPING_CLEARINGBULK,
            /// <summary>
            /// Transfer de masse
            /// </summary>
            [Cst.TrackerSystemMsg(SysNumber = 4046)]
            POSKEEPING_TRANSFERBULK,
            /// <summary>
            /// Assignment/Exercise de masse
            /// </summary>
            // EG 20151019 [21495] New
            [Cst.TrackerSystemMsg(SysNumber = 4047)]
            POSKEEPING_ASSEXEBULK,
            /// <summary>
            /// Abandon de masse
            /// </summary>
            // EG 20151019 [21495] New
            [Cst.TrackerSystemMsg(SysNumber = 4048)]
            POSKEEPING_ABNBULK,
            /// <summary>
            /// Décompensation
            /// </summary>
            [Cst.TrackerSystemMsg(SysNumber = 210)]
            POSKEEPING_UNCLEARING,
            /// <summary>
            /// Annulation de liquidation de future à l'échéance 
            /// </summary>
            POSKEEPING_UNCLEARING_MOF,
            /// <summary>
            /// Annulation de denouements auto. à l'échéance
            /// </summary>
            POSKEEPING_UNCLEARING_MOO,
            /// <summary>
            /// 
            /// </summary>
            POSREQUEST_L,
            /// <summary>
            /// Menu: Référentiel Message de règlement
            /// </summary>
            STLMESSAGE,
            /// <summary>
            /// Menu: Référentiel Message de confirmation
            /// </summary>
            CNFMessage,
            /// <summary>
            /// Menu: Référentiel Message de regroupement 
            /// </summary>
            TSMessage,
            /// <summary>
            /// Tracker unitaire 
            /// . Affichage  d'un ligne du tracker en mode formulaire sur click d'une ligne dans Tracker page principale
            /// . Affichage  du tracker en mode liste sur click du bouton détail d'un groupe dans Tracker page principale
            /// </summary>
            /// FI 20240402 [WI888] Il n'existe pas de menu Spheres® (IdMenu = "")
            [IdMenu(IdMenu = "")]
            TRACKER_L,

            /// <summary>
            /// Logs: Processing requests
            /// </summary>
            /// FI 20240402 [WI888] Add
            [IdMenu(IdMenu = "OTC_VIEW_LOGREQUEST")]
            LOGREQUEST,

            /// <summary>
            /// 
            /// </summary>
            /// FI 20240402 [WI888] add (replace TRACKERDET_L)
            [IdMenu(IdMenu= "OTC_VIEW_LOGREQUESTPROCESS")]
            LOGREQUESTDET_L,
            /* FI 20240402 [WI888] Mise en commentaire (remplacé par  OTC_VIEW_LOGREQUESTPROCESS)
            /// <summary>
            /// 
            /// </summary>
            TRACKERDET_L,
            */
            /// <summary>
            /// </summary>
            PROCESS_INV_CIGEN,
            /// <summary>
            /// </summary>
            PROCESS_INV_CMGEN,
            /// <summary>
            /// </summary>
            PROCESS_CIGEN,
            /// <summary>
            /// </summary>
            PROCESS_CMGEN,
            /// <summary>
            /// </summary>
            [Cst.TrackerSystemMsg(SysNumber = 2020, SysNumber_ObserverMode = 2025)]
            PROCESS_RIMGEN,
            [Cst.TrackerSystemMsg(SysNumber = 2021, SysNumber_ObserverMode = 2025)]
            PROCESS_RMGEN,
            [Cst.TrackerSystemMsg(SysNumber = 2022, SysNumber_ObserverMode = 2026)]
            PROCESS_FINPER_RIMGEN,
            [Cst.TrackerSystemMsg(SysNumber = 2023, SysNumber_ObserverMode = 2026)]
            PROCESS_FINPER_RMGEN,
            /// <summary>
            /// Menu de suppression des allocations en masse FO (ETD) et OTC
            /// </summary>
            /// EG 20150722 Replace PROCESS_REMOVEALLOC
            [Cst.TrackerSystemMsg(SysNumber = 4045)]
            PROCESS_FO_REMOVEALLOC,
            [Cst.TrackerSystemMsg(SysNumber = 4045)]
            PROCESS_OTC_REMOVEALLOC,
            /// <summary>
            /// 
            /// </summary>
            Product,
            /// EG 20170125 [Refactoring URL] New
            LogEndOfDay,
            LogClosingDay,
            /// <summary>
            /// 
            /// </summary>
            LogPosRequest,
            PosSynthesis,
            PosSynthesis_OTC,
            TrackerPosRequest,
            EntityMarket,
            // EG 20170215 New
            PosSynthesisDet,
            PosSynthesisDet_OTC,
            GrpContract, // FI 20170908 [23409] add
            GrpMarket,
            /// <summary>
            /// Menu: Mise à jour de la règle d'échéance d'un Contrat Dérivé
            /// </summary>
            /// JN 20190513
            DerivativeContractUpdateMaturityRule,
            /// <summary>
            /// Menu: Mise à jour de la règle d'échéance additionnelle d'un Contrat Dérivé
            /// </summary>
            /// FI 20220503 [XXXXX]
            DerivativeContractUpdateAdditionnalMaturityRule
        }

        /// <summary>
        /// Retourne la valeur enum du menu {pIdMenu}
        /// <para>Retourne null si {pIdMenu} n'exise pas dans les valeurs de l'enum Menu</para>
        /// </summary>
        /// <param name="pIdMenu"></param>
        /// <returns></returns>
        public static Nullable<Menu> ConvertToMenu(string pIdMenu)
        {
            Nullable<Menu> ret = null;
            foreach (Menu @value in Enum.GetValues(typeof(Menu)))
            {
                if (pIdMenu == IdMenu.GetIdMenu(@value))
                {
                    ret = @value;
                    break;
                }
            }
            return ret;
        }
        /// <summary>
        /// Retourne idMenu (codification dans la table Menu) associé à {pIdMenu}
        /// </summary>
        /// <param name="pIdMenu"></param>
        /// <returns></returns>
        /// EG 20141230 [20587]
        /// FI 20170313 [22225] Modify
        /// FI 20170928 [23452] Modify
        public static string GetIdMenu(Menu pIdMenu)
        {

            // FI 20230119 [XXXXX] en priorité lecture de IdMenuAttribute lorsque présent
            IdMenuAttribute attribute = ReflectionTools.GetAttribute<IdMenuAttribute>(pIdMenu);
            string ret;
            if (null != attribute)
            {
                ret = attribute.IdMenu;
            }
            else
            {
                switch (pIdMenu)
                {
                    case Menu.About:
                        ret = "OTC_ABOUT";
                        break;
                    case Menu.Repository:
                        ret = "OTC_REF";
                        break;
                    case Menu.Admin:
                        ret = "OTC_ADM";
                        break;
                    case Menu.ViewTradeOTCOrder:
                        ret = "OTC_VIEW_OTC_INP_ORDER";
                        break;
                    case Menu.ViewTradeOTCAlloc:
                        ret = "OTC_VIEW_OTC_INP_ALLOC";
                        break;
                    case Menu.ViewTradeFnOAlloc:
                        ret = "OTC_VIEW_FO_INP_ALLOC";
                        break;
                    case Menu.ViewTradeFnOOrder:
                        ret = "OTC_VIEW_FO_INP_ORDER";
                        break;
                    case Menu.Input:
                        ret = "OTC_INP";
                        break;
                    case Menu.Actor:
                        ret = "OTC_REF_ACT_ACT";
                        break;
                    case Menu.GrpActor:
                        ret = "OTC_REF_GRP_ACTOR_COMMON";
                        break;
                    case Menu.Algorithm: //FI 20170928 [23452]
                        ret = "OTC_REF_ACT_ACT_ALGORITHM";
                        break;
                    case Menu.BusinessCenter:
                        ret = "OTC_REF_LST_BC";
                        break;
                    case Menu.Instrument:
                        ret = "OTC_REF_PRD_LST_INSTR";
                        break;
                    case Menu.Strategy: // FI 20240703 [WI989] Add
                        ret = "OTC_REF_PRD_LST_STRATEGY";
                        break;
                    case Menu.GrpInstrument:
                        ret = "OTC_REF_GRP_INSTR_COMMON";
                        break;
                    case Menu.ActorTaxCollected:
                        ret = "OTC_REF_ACT_ACTOR_TAX";
                        break;
                    case Menu.Book:
                        ret = "OTC_REF_ACT_ACT_BOOK";
                        break;
                    case Menu.ParentList: //LP 20240712 [WI1001] add parent list
                        ret = "OTC_REF_ACT_ACT_PARENTLST";
                        break;
                    case Menu.GrpBook:
                        ret = "OTC_REF_GRP_BOOK_COMMON";
                        break;
                    // FI 20150928 [XXXXX] add AssetCash
                    case Menu.AssetCash:
                        ret = "OTC_REF_DATA_UNDERASSET_OTHER_CASHASSET";
                        break;
                    case Menu.AssetCommodity:
                        ret = "OTC_REF_DATA_UNDERASSET_COMMODITY_ASSET";
                        break;
                    /// EG 20161122 New Commodity Derivative
                    case Menu.AssetCommodityContract:
                        ret = "OTC_REF_DATA_UNDERASSET_COMMODITY_CONTRACT";
                        break;
                    case Menu.AssetEquity:
                        ret = "OTC_REF_DATA_UNDERASSET_SECURITY_EQUITYASSET";
                        break;
                    case Menu.AssetEquityBasketConstituent:
                        ret = "OTC_REF_DATA_UNDERASSET_SECURITY_EQUITYBASKETCONST";
                        break;
                    case Menu.AssetFxRate:
                        ret = "OTC_REF_DATA_UNDERASSET_RATE_FXRATEASSET";
                        break;
                    case Menu.AssetRateIndex:
                        ret = "OTC_REF_DATA_UNDERASSET_RATE_RATEINDEXASSET";
                        break;
                    case Menu.AssetIndex:
                        ret = "OTC_REF_DATA_UNDERASSET_RATE_INDEXASSET";
                        break;
                    case Menu.AssetETD:
                        ret = "OTC_REF_DATA_UNDERASSET_LISTED_CONTRACT_ATTRIB_ASSET";
                        break;
                    case Menu.AssetExchangeTradedFund:
                        ret = "OTC_REF_DATA_UNDERASSET_SECURITY_EXCHANGETRADEDFUNDASSET";
                        break;
                    case Menu.AssetEnv:
                        ret = "OTC_REF_PRD_LST_INSTR_ASSETENV";
                        break;
                    case Menu.CommodityContract:
                        ret = "OTC_REF_DATA_UNDERASSET_COMMODITY_CONTRACT";
                        break;
                    case Menu.DrvContract:
                        ret = "OTC_REF_DATA_UNDERASSET_LISTED_CONTRACT";
                        break;
                    case Menu.DrvAttrib: // FI 20170313 [22225] Add
                        ret = "OTC_REF_DATA_UNDERASSET_LISTED_CONTRACT_ATTRIB";
                        break;
                    case Menu.Enums: // FI 20160804 [Migration TFS] add
                        ret = "OTC_REF_LST_ENUMS";
                        break;
                    case Menu.GrpContract:
                        ret = "OTC_REF_GRP_CONTRACT_COMMON";
                        break;
                    case Menu.GrpMarket:
                        ret = "OTC_REF_GRP_MARKET_COMMON";
                        break;
                    case Menu.Fee:
                        ret = "OTC_REF_CHARGE_FEES_FEE";
                        break;
                    case Menu.FeeMatrix:
                        ret = "OTC_REF_CHARGE_FEES_FEE_MATRIX";
                        break;
                    case Menu.FeeSchedule:
                        ret = "OTC_REF_CHARGE_FEES_FEE_SCHEDULE";
                        break;
                    case Menu.FilterActor:
                        ret = "OTC_ADM_FILTER_ACTOR_FLT";
                        break;
                    case Menu.FilterInstrument:
                        ret = "OTC_ADM_FILTER_INSTR_FLT";
                        break;
                    case Menu.FilterMarket:
                        ret = "OTC_ADM_FILTER_MARKET_FLT";
                        break;
                    case Menu.FilterMenu:
                        ret = "OTC_ADM_FILTER_MNU_FLT";
                        break;
                    case Menu.FilterPermission:
                        ret = "OTC_ADM_FILTER_PERM_FLT";
                        break;
                    case Menu.ViewTradeAdmin:
                        ret = "OTC_INV_BROFEE_VIEW_TRD";
                        break;
                    case Menu.ImCboeContract:
                        ret = "OTC_REF_DATA_UNDERASSET_LISTED_CONTRACT_IMCBOECONTRACT";
                        break;
                    case Menu.ImCboeMarket:
                        ret = "OTC_REF_LST_MARKET_IMCBOEMARKET";
                        break;
                    case Menu.ImTimsIdemContract:
                        ret = "OTC_REF_DATA_UNDERASSET_LISTED_CONTRACT_IMTIMSIDEMCONTRACT";
                        break;
                    case Menu.ImIMSMCESMCommodityContract:
                        // PM 20231129 [XXXXX][WI759] Ajout
                        ret = "OTC_REF_DATA_UNDERASSET_COMMODITY_CONTRACT_IMIMSMCESM";
                        break;
                    case Menu.DerivativeContractUpdateMaturityRule:
                        ret = "OTC_REF_DATA_UNDERASSET_LISTED_CONTRACT_MATURITYRULE";
                        break;
                    case Menu.DerivativeContractUpdateAdditionnalMaturityRule:
                        ret = "OTC_REF_DATA_UNDERASSET_LISTED_CONTRACT_DRVCONTRACTMATRULE_MR";
                        break;
                    case Menu.InputTradeAdmin:
                        ret = "OTC_INV_BROFEE_INP_TRD";
                        break;
                    case Menu.InputTradeAdmin_RMV:
                        ret = "OTC_INV_BROFEE_INP_TRD_RMV";
                        break;
                    case Menu.InputTradeAdmin_COR:
                        ret = "OTC_INV_BROFEE_INP_TRD_COR";
                        break;
                    case Menu.InvoicingValidation:
                        ret = "OTC_INV_BROFEE_PROCESS_VALIDATION";
                        break;
                    case Menu.InvoicingCancellation:
                        ret = "OTC_INV_BROFEE_PROCESS_CANCELLATION";
                        break;
                    case Menu.InvoicingGeneration:
                        ret = "OTC_INV_BROFEE_PROCESS_GENERATION";
                        break;
                    case Menu.InvoicingRules:
                        ret = "OTC_REF_ACT_ACT_INVOICINGRULES";
                        break;
                    //case Menu.ClosingDay:
                    //    ret = "OTC_PROCESS_CLOSINGDAY";
                    //    break;
                    case Menu.Market:
                        ret = "OTC_REF_LST_MARKET";
                        break;
                    case Menu.RateIndex: // FI 20190404 add
                        ret = "OTC_REF_LST_RATEINDEX";
                        break;
                    case Menu.InputTrade:
                        ret = "OTC_INP_TRD";
                        break;
                    case Menu.InputTrade_ABN:
                        ret = "OTC_INP_TRD_ABN";
                        break;
                    case Menu.InputTrade_ABN_AUTOMATIC:
                        ret = "OTC_INP_TRD_ABN_MAT";
                        break;
                    case Menu.InputTrade_ASS:
                        ret = "OTC_INP_TRD_ASS";
                        break;
                    case Menu.InputTrade_ASS_AUTOMATIC:
                        ret = "OTC_INP_TRD_ASS_MAT";
                        break;
                    case Menu.InputTrade_BAR_TRG:
                        ret = "OTC_INP_TRD_BAR_TRG";
                        break;
                    case Menu.InputTrade_CAL:
                        ret = "OTC_INP_TRD_CAL";
                        break;
                    case Menu.InputTrade_POC:
                        ret = "OTC_INP_TRD_POC";
                        break;
                    case Menu.InputTrade_RMV:
                        ret = "OTC_INP_TRD_RMV";
                        break;
                    case Menu.InputTrade_RMVALLOC:
                        ret = "OTC_INP_TRD_RMVALLOC";
                        break;
                    case Menu.InputTrade_CSR:
                        ret = "OTC_INP_TRD_CSR";
                        break;
                    case Menu.InputTrade_REN:
                        ret = "OTC_INP_TRD_REN";
                        break;
                    case Menu.InputTrade_EXE:
                        ret = "OTC_INP_TRD_EXE";
                        break;
                    case Menu.InputTrade_EXE_CLR: //PL 20150320 Clearing
                        ret = "OTC_INP_TRD_EXE_CLR";
                        break;
                    case Menu.InputTrade_EXE_AUTOMATIC:
                        ret = "OTC_INP_TRD_EXE_MAT";
                        break;
                    case Menu.InputTrade_EEX:
                        ret = "OTC_INP_TRD_EEX";
                        break;
                    case Menu.InputTrade_REP:
                        ret = "OTC_INP_TRD_REP";
                        break;
                    case Menu.InputTrade_RES:
                        ret = "OTC_INP_TRD_RES";
                        break;
                    case Menu.InputTrade_CAN:
                        ret = "OTC_INP_TRD_CAN";
                        break;
                    case Menu.InputTrade_CASC:
                        ret = "OTC_INP_TRD_CASC";
                        break;
                    case Menu.InputTrade_MET:
                        ret = "OTC_INP_TRD_MET";
                        break;
                    case Menu.InputTrade_OET:
                        ret = "OTC_INP_TRD_OET";
                        break;
                    case Menu.InputTrade_PRO:
                        ret = "OTC_INP_TRD_PRO";
                        break;
                    case Menu.InputTrade_SUP:
                        ret = "OTC_INP_TRD_SUP";
                        break;
                    case Menu.InputTrade_POT:
                        ret = "OTC_INP_TRD_POT";
                        break;
                    case Menu.InputTrade_SPLIT:
                        ret = "OTC_INP_TRD_SPLIT";
                        break;
                    case Menu.InputTrade_CLEARSPEC:
                        ret = "OTC_INP_TRD_CLEARSPEC";
                        break;
                    case Menu.InputTrade_DLV:
                        ret = "OTC_INP_TRD_DLV";
                        break;
                    case Menu.InputEvent:
                        ret = "OTC_INP_OTHERS_EVT";
                        break;
                    case Menu.InputDebtSec:
                        ret = "OTC_INP_DEBTSECURITY";
                        break;
                    /// EG 20170125 [Refactoring URL] New
                    case Menu.InputTradeRisk:
                        ret = "OTC_INP_RISK_TRADE";
                        break;
                    case Menu.InputTradeRisk_InitialMargin:
                        ret = "OTC_INP_INITMARGIN";
                        break;
                    case Menu.InputTradeRisk_InitialMargin_Correction:
                        ret = "OTC_INP_INITMARGIN_COR";
                        break;
                    case Menu.InputTradeRisk_CashBalance:
                        ret = "OTC_INP_CASHBALANCE";
                        break;
                    case Menu.InputTradeRisk_CashBalance_Correction:
                        ret = "OTC_INP_CASHBALANCE_COR";
                        break;
                    case Menu.InputTradeRisk_CashPayment:
                        ret = "OTC_INP_CASHPAYMENT";
                        break;
                    case Menu.InputTradeRisk_CashInterest:
                        ret = "OTC_INV_CASHINTEREST_INP_TRD";
                        break;


                    case Menu.InputTradeRisk_CashPayment_Correction:
                        ret = "OTC_INP_CASHPAYMENT_COR";
                        break;
                    case Menu.Confirm:
                        ret = "OTC_MAIL_CNF";
                        break;
                    case Menu.AdminSafetyLoginLog:
                        // EG 20151215 [21305] Change OTC_ADM_SAF_LOGLOGIN
                        ret = "OTC_ADM_SAF_LOG_LOG_LOGIN";
                        break;
                    case Menu.UserLockoutLog:
                        // EG 20151215 [21305] New
                        ret = "OTC_ADM_SAF_LOG_LOG_USERLOCKOUT";
                        break;
                    case Menu.HostLockoutLog:
                        // EG 20151215 [21305] New
                        ret = "OTC_ADM_SAF_LOG_LOG_HOSTLOCKOUT";
                        break;
                    case Menu.IOTASK:
                        ret = "OTC_ADM_TOOL_IO_TASK";
                        break;
                    case Menu.IOTASKDET:
                        ret = "OTC_ADM_TOOL_IO_TASK_TASKDET";
                        break;
                    case Menu.IOINPUT:
                        ret = "OTC_ADM_TOOL_IO_INPUT";
                        break;
                    case Menu.IOOUTPUT:
                        ret = "OTC_ADM_TOOL_IO_OUTPUT";
                        break;
                    case Menu.IOSHELL:
                        ret = "OTC_ADM_TOOL_IO_SHELL";
                        break;
                    case Menu.IOCOMPARE:
                        ret = "OTC_ADM_TOOL_IO_COMPARE";
                        break;
                    case Menu.IOPARSING:
                        ret = "OTC_ADM_TOOL_IO_PARSING";
                        break;
                    case Menu.IOPARAM:
                        ret = "OTC_ADM_TOOL_IO_PARAM";
                        break;

                    case Menu.IOTRACK:
                        // 20120315 MF Special fase for Vision
                        if (Software.IsSoftwareVision())
                        {
                            ret = "VISION_ADM_SAF_LOG_IOTRACK";
                        }
                        else
                        {
                            ret = "OTC_VIEW_IOTRACK";
                        }
                        break;
                    case Menu.IOTRACKCOMPARE:
                        // 20120315 MF Special fase for Vision
                        if (Software.IsSoftwareVision())
                            ret = "VISION_ADM_SAF_LOG_IOTRACK_COMPARE";
                        else
                            ret = "OTC_VIEW_IOTRACK_COMPARE";
                        break;


                    case Menu.REQUEST_L:
                        ret = "OTC_VIEW_LOGREQUEST";
                        break;
                    case Menu.PROCESS_L:
                        ret = "OTC_VIEW_LOGPROCESS";
                        break;
                    case Menu.PROCESSDET_L:
                        ret = "OTC_VIEW_LOGPROCESS_DET";
                        break;

                    case Menu.QUOTE_BOND_H:
                        ret = "OTC_REF_DATA_QUOTE_BOND";
                        break;
                    case Menu.QUOTE_DEBTSEC_H:
                        ret = "OTC_REF_DATA_QUOTE_DEBTSECURITY";
                        break;
                    case Menu.QUOTE_DEPOSIT_H:
                        ret = "OTC_REF_DATA_QUOTE_DEPOSIT";
                        break;
                    case Menu.QUOTE_EQUITY_H:
                        ret = "OTC_REF_DATA_QUOTE_EQUITY";
                        break;
                    /// EG 20170125 [Refactoring URL] Upd
                    case Menu.QUOTE_EXTRDFUND_H:
                        ret = "OTC_REF_DATA_QUOTE_EXCHANGETRADEDFUND";
                        break;
                    case Menu.QUOTE_ETD_H:
                        ret = "OTC_REF_DATA_QUOTE_ETD";
                        break;
                    case Menu.QUOTE_FXRATE_H:
                        ret = "OTC_REF_DATA_QUOTE_FXRATE";
                        break;
                    case Menu.QUOTE_INDEX_H:
                        ret = "OTC_REF_DATA_QUOTE_INDEX";
                        break;
                    case Menu.QUOTE_RATEINDEX_H:
                        ret = "OTC_REF_DATA_QUOTE_RATEINDEX";
                        break;
                    case Menu.QUOTE_SIMPLEFRA_H:
                        ret = "OTC_REF_DATA_QUOTE_SIMPLEFRA";
                        break;
                    case Menu.QUOTE_SIMPLEIRS_H:
                        ret = "OTC_REF_DATA_QUOTE_SIMPLEIRS";
                        break;
                    case Menu.MATURITY:
                        ret = "OTC_REF_LST_MATURITYRULE_MATURITY";
                        break;
                    case Menu.POSCOLLATERALVAL:
                        ret = "OTC_INP_POSCOLLATERAL_VAL";
                        break;
                    case Menu.SERVICE_L:
                        ret = "OTC_VIEW_LOGSERVICE";
                        break;
                    case Menu.MARGINTRACK:
                        ret = "OTC_VIEW_MARGINTRACK";
                        break;
                    case Menu.NETCONVENTION:
                        ret = "OTC_REF_MAIL_STL_NETCONVENTION";
                        break;
                    case Menu.NETDESIGNATION:
                        ret = "OTC_REF_MAIL_STL_NETDESIGNATION";
                        break;
                    case Menu.POSKEEPING_EOD:
                        ret = "OTC_PROCESS_ENDOFDAY";
                        break;
                    case Menu.POSKEEPING_CLOSINGDAY:
                        ret = "OTC_PROCESS_CHANGEOFDAY";
                        break;
                    case Menu.POSKEEPING_UPDATENTRY:
                        ret = "OTC_PROCESS_POSITION_RISK_ITD_UPDATENTRY";
                        break;
                    case Menu.POSKEEPING_CLEARINGBULK:
                        ret = "OTC_PROCESS_POSITION_RISK_ITD_CLEARINGBLK";
                        break;
                    case Menu.POSKEEPING_UNCLEARING:
                        ret = "OTC_PROCESS_POSITION_RISK_CORR_UNCLEARING";
                        break;
                    case Menu.POSKEEPING_UNCLEARING_MOF:
                        ret = "OTC_PROCESS_POSITION_RISK_CORR_UNCLEARING_MOF";
                        break;
                    case Menu.POSKEEPING_UNCLEARING_MOO:
                        ret = "OTC_PROCESS_POSITION_RISK_CORR_UNCLEARING_MOO";
                        break;
                    case Menu.POSKEEPING_TRANSFERBULK:
                        ret = "OTC_PROCESS_POSITION_RISK_CORR_TRANSFERBLK";
                        break;
                    case Menu.POSREQUEST_L:
                        ret = "OTC_VIEW_POSREQUEST";
                        break;
                    case Menu.STLMESSAGE:
                        ret = "OTC_REF_MAIL_STL_STLMESSAGE";
                        break;
                    case Menu.CNFMessage:
                        ret = "OTC_REF_MAIL_CNF_CNFMESSAGE";
                        break;
                    case Menu.TSMessage:
                        ret = "OTC_REF_MAIL_REPORT_MESSAGE";
                        break;
                    case Menu.TRACKER_L:
                        ret = "OTC_VIEW_TRACKER";
                        break;
                    /*  // FI 20240402 [WI888] Mise en commentaire  
                    case Menu.TRACKERDET_L:
                        //PL 20121102 A priori c'est maintenant OTC_VIEW_LOGREQUESTPROCESS
                        //ret = "OTC_VIEW_TRACKER_DET";
                        ret = "OTC_VIEW_LOGREQUESTPROCESS";
                        break;
                    */
                    /// EG 20170125 [Refactoring URL] New
                    case Menu.EntityMarket:
                        ret = "ENTITYMARKET";
                        break;
                    /// EG 20170125 [Refactoring URL] New
                    case Menu.LogEndOfDay:
                    case Menu.LogClosingDay:
                    case Menu.LogPosRequest:
                    case Menu.TrackerPosRequest:
                        ret = "OTC_VIEW_LOGPOSREQUEST";
                        break;
                    // EG 20120912
                    case Menu.PROCESS_INV_CIGEN:
                        ret = "OTC_INV_PROCESS_CNF_CIGEN";
                        break;
                    case Menu.PROCESS_INV_CMGEN:
                        ret = "OTC_INV_PROCESS_CNF_CMGEN";
                        break;
                    case Menu.PROCESS_CIGEN:
                        ret = "OTC_PROCESS_CIGEN";
                        break;
                    case Menu.PROCESS_CMGEN:
                        ret = "OTC_PROCESS_CMGEN";
                        break;
                    case Menu.PROCESS_RIMGEN:
                        ret = "OTC_PROCESS_MAIL_REPORT_RIMGEN";
                        break;
                    case Menu.PROCESS_RMGEN:
                        ret = "OTC_PROCESS_MAIL_REPORT_RMGEN";
                        break;
                    case Menu.PROCESS_FINPER_RIMGEN:
                        ret = "OTC_PROCESS_MAIL_FINPER_RIMGEN";
                        break;
                    case Menu.PROCESS_FINPER_RMGEN:
                        ret = "OTC_PROCESS_MAIL_FINPER_RMGEN";
                        break;
                    case Menu.Product:
                        ret = "OTC_REF_PRD_LST";
                        break;
                    case Menu.InputCorporateActionIssue:
                        ret = "OTC_INP_CORPOACTIONISSUE";
                        break;
                    case Menu.InputCorporateActionEmbedded:
                        ret = "OTC_INP_CORPOACTIONEMBEDDED";
                        break;
                    case Menu.InputClosingReopeningPosition:
                        // EG 20190318 Upd ClosingReopeningPosition
                        ret = "OTC_INP_CLOSINGREOPENINGREQUEST";
                        break;
                    // EG 20150722 Replace PROCESS_REMOVEALLOC
                    case Menu.PROCESS_FO_REMOVEALLOC:
                        ret = "OTC_PROCESS_POSITION_RISK_CORR_FO_REMOVEALLOC";
                        break;
                    // EG 20150722 New 
                    case Menu.PROCESS_OTC_REMOVEALLOC:
                        ret = "OTC_PROCESS_POSITION_RISK_CORR_OTC_REMOVEALLOC";
                        break;
                    // EG 20151019 [21465] New Assignation/Exercice de masse
                    case Menu.POSKEEPING_ASSEXEBULK:
                        ret = "OTC_PROCESS_POSITION_RISK_ASSEXEBLK";
                        break;
                    // EG 20151019 [21465] New Abandon de masse
                    case Menu.POSKEEPING_ABNBULK:
                        ret = "OTC_PROCESS_POSITION_RISK_ABNBLK";
                        break;
                    /// EG 20170125 [Refactoring URL] New
                    case Menu.PosSynthesis:
                        ret = "OTC_VIEW_FO_POSITION_RISK_SYNT";
                        break;
                    /// EG 20170125 [Refactoring URL] New
                    case Menu.PosSynthesis_OTC:
                        ret = "OTC_VIEW_OTC_POSITION_RISK_SYNT";
                        break;
                    /// EG 20170215 [Refactoring URL] New
                    case Menu.PosSynthesisDet:
                        ret = "OTC_VIEW_FO_POSITIONDET";
                        break;
                    /// EG 20170215 [Refactoring URL] New
                    case Menu.PosSynthesisDet_OTC:
                        ret = "OTC_VIEW_OTC_POSITIONDET";
                        break;
                    default:
                        throw new NotImplementedException(StrFunc.AppendFormat("Menu[{0}] is not implemented", pIdMenu));
                }
            }
            return ret;
        }
        /// <summary>
        /// Retourne true si le menu commence par IdMenu.InputTrade)
        /// </summary>
        /// <param name="pMenu"></param>
        /// <returns></returns>
        public static bool IsMenuTradeInput(string pMenu)
        {
            return pMenu.StartsWith(IdMenu.GetIdMenu(IdMenu.Menu.InputTrade));
        }
        /// <summary>
        /// Retourne true si le menu commence par IdMenu.InputTradeAdmin)
        /// </summary>
        /// <param name="pMenu"></param>
        /// <returns></returns>
        public static bool IsMenuTradeAdminInput(string pMenu)
        {
            return pMenu.StartsWith(IdMenu.GetIdMenu(IdMenu.Menu.InputTradeAdmin));
        }
        /// <summary>
        /// Retourne true si le menu commence par IdMenu.IntputTradeRisk)
        /// </summary>
        /// <param name="pMenu"></param>
        /// <returns></returns>
        public static bool IsMenuTradeRiskInput(string pMenu)
        {
            //PL 20140930 
            //return pMenu.StartsWith(IdMenu.GetIdMenu(IdMenu.Menu.IntputTradeRisk));
            bool isMenuTradeRiskInput = pMenu.StartsWith(IdMenu.GetIdMenu(IdMenu.Menu.InputTradeRisk_InitialMargin));
            if (!isMenuTradeRiskInput)
            {
                isMenuTradeRiskInput = pMenu.StartsWith(IdMenu.GetIdMenu(IdMenu.Menu.InputTradeRisk_CashBalance)); ;
                if (!isMenuTradeRiskInput)
                {
                    isMenuTradeRiskInput = pMenu.StartsWith(IdMenu.GetIdMenu(IdMenu.Menu.InputTradeRisk_CashPayment)); ;
                }
            }
            return isMenuTradeRiskInput;
        }
    }
}
