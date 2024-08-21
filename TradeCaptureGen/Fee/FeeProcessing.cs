using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using EfsML.StrategyMarker;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
//PasGlop Faire un search dans la solution de "TODO FEEMATRIX"

namespace EFS.TradeInformation
{

    /// <summary>
    /// FeeProcessing: Classe principale du processus de calcul des frais à partir du référentiel en vigueur.
    /// <para>NB: Cette classe "FeeProcessing" contient entre autre un élément "FeeRequest" et un array de "FeeResponse"</para>
    /// </summary>
    // EG 20130911 Add member _dbTransaction = Gestion mode Transactionel (exemple : Appel via EOD TradeMerge)
    public class FeeProcessing
    {
        #region Members
        private readonly FeeRequest _feeRequest;
        private FeeResponse[] _feeResponse;


        private StrBuilder _auditMessage;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient l'environment sur lequel Spheres® calcule des frais
        /// </summary>
        public FeeRequest FeeRequest
        {
            get { return _feeRequest; }
        }
        /// <summary>
        /// 
        /// </summary>
        public FeeResponse[] FeeResponse
        {
            get { return _feeResponse; }
        }

        public DataDocumentContainer DataDocument
        {
            get { return _feeRequest.DataDocument; }
        }

        #endregion Accessors

        #region Constructors
        public FeeProcessing(FeeRequest pFeeRequest)
        {
            _feeRequest = pFeeRequest;
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Purge feeResponse
        /// </summary>
        public void Reset()
        {
            _feeResponse = null;
        }

        /// <summary>
        /// Check data and calculate the fee amount
        /// </summary>
        public void Calc(string pCS, IDbTransaction pDbTransaction)
        {

            #region Initialisation à l'aide de "FeeRequest" à partir de la table FEEMATRIX
            FeeMatrixs feeMatrixs = new FeeMatrixs(_feeRequest);
            feeMatrixs.Initialize(pCS, pDbTransaction);
            #endregion

            #region Balayage des éléments issus de la table FEEMATRIX pour vérifier si le trade courant match avec eux
            _auditMessage = new StrBuilder();
            int count = feeMatrixs.Count;
            int countFeeRequestOtherParty = ArrFunc.Count(FeeRequest.PartyBroker);

            if (count > 0)
            {
                StrBuilder tmp_auditMessage;
                StrBuilder det_auditMessage;
                int maxAuditMsg = 1000;

                _auditMessage += "===================================================" + Cst.CrLf;
                _auditMessage += "Candidate(s) - Count: " + count.ToString() + Cst.CrLf;
                for (int i = 0; i < count; i++)
                {
                    FeeMatrix feeMatrix = feeMatrixs[i];
                    det_auditMessage = new StrBuilder();

                    if (feeMatrix.FEESCHED_IDASpecified)
                    {
                        SQL_Actor sql_Actor = new SQL_Actor(pCS, feeMatrix.FEESCHED_IDA)
                        {
                            DbTransaction = pDbTransaction
                        };
                        feeMatrix.FEESCHED_IDA_Identifier = sql_Actor.Identifier;
                        feeMatrix.FEESCHED_IDA_Displayname = sql_Actor.DisplayName;
                    }

                    if (FeeRequest.IsWithAuditMsg && (i < maxAuditMsg))
                    {
                        _auditMessage += "---------------------------------------------------" + Cst.CrLf;
                        _auditMessage += MatrixScheduleCharacteristics(feeMatrixs[i]);
                    }

                    #region Ctrl Period
                    if (feeMatrix.Status == FeeMatrixs.StatusEnum.Unknown)
                    {
                        feeMatrix.CommentForDebug = "Period None";
                        if (feeMatrix.LOWHIGHBASISSpecified && (feeMatrix.LOWVALUESpecified || feeMatrix.HIGHVALUESpecified))
                        {
                            tmp_auditMessage = new StrBuilder();
                            tmp_auditMessage += "Ctrl Period: ";

                            try
                            {
                                feeMatrix.CommentForDebug = "Period Checked";
                                //
                                string errorMessage = string.Empty;
                                decimal assessmentBasisDecValue = 0;
                                string assessmentBasisStrValue = string.Empty;
                                //                                    
                                if (GetAssessmentBasisValue(pCS,pDbTransaction , feeMatrix.LOWHIGHBASIS, ref assessmentBasisDecValue, ref assessmentBasisStrValue, ref errorMessage))
                                {
                                    if ((feeMatrix.Status == FeeMatrixs.StatusEnum.Unknown) && (feeMatrix.LOWVALUESpecified))
                                    {
                                        if (assessmentBasisDecValue <= feeMatrix.LOWVALUE)
                                        {
                                            feeMatrix.Status = FeeMatrixs.StatusEnum.Unvalid;
                                            feeMatrix.CommentForDebug = "Period Lower";
                                        }
                                    }
                                    if ((feeMatrix.Status == FeeMatrixs.StatusEnum.Unknown) && (feeMatrix.HIGHVALUESpecified))
                                    {
                                        if (assessmentBasisDecValue > feeMatrix.HIGHVALUE)
                                        {
                                            feeMatrix.Status = FeeMatrixs.StatusEnum.Unvalid;
                                            feeMatrix.CommentForDebug = "Period Higher";
                                        }
                                    }
                                }
                                else
                                {
                                    feeMatrix.Status = FeeMatrixs.StatusEnum.Unvalid;
                                    feeMatrix.CommentForDebug = errorMessage;
                                }
                            }
                            catch
                            {
                                feeMatrix.Status = FeeMatrixs.StatusEnum.Error;
                                feeMatrix.CommentForDebug = "Error Low/High";
                            }

                            tmp_auditMessage += feeMatrix.CommentForDebug + Cst.CrLf;
                            if (feeMatrix.Status == FeeMatrixs.StatusEnum.Error || feeMatrix.Status == FeeMatrixs.StatusEnum.Unvalid)
                            {
                                det_auditMessage += tmp_auditMessage + Cst.CrLf;
                            }
                        }
                    }
                    #endregion Ctrl Period

                    #region Ctrl Broker
                    tmp_auditMessage = new StrBuilder();
                    tmp_auditMessage += "Ctrl Broker: ";
                    feeMatrix.CommentForDebug = "Broker Checked";

                    feeMatrix.MatchedOtherParty1Index = -1;
                    feeMatrix.MatchedOtherParty2Index = -1;

                    //Ctrl OtherParty1 / OtherParty2 
                    if (feeMatrix.Status == FeeMatrixs.StatusEnum.Unknown)
                    {
                        // Recherche le premier FeeRequest.OtherParty qui match avec OtherParty1 de feeMatrix
                        // S'il est trouvé, stockage de son index dans feeMatrix.MatchedOtherParty1Index
                        bool isExistOtherParty = false;
                        for (int j = 0; j < countFeeRequestOtherParty; j++)
                        {
                            isExistOtherParty = true;
                            feeMatrix.Status = FeeMatrixs.StatusEnum.Unknown;
                            CheckParty(pCS, pDbTransaction, TypeSidePartyEnum.OtherParty1, ref feeMatrix, FeeRequest.PartyBroker[j]);

                            if (feeMatrix.Status == FeeMatrixs.StatusEnum.Unknown)
                            {
                                feeMatrix.MatchedOtherParty1Index = j;
                                break;
                            }
                        }
                        if (!isExistOtherParty)
                        {
                            //Il n'existe aucune OtherParty1 sur le trade. Contrôle d'une éventuelle condition en vigueur dans feeMatrix, afin de voir si elle est compatible (ex. None ou All Broker)
                            CheckParty(pCS, pDbTransaction, TypeSidePartyEnum.OtherParty1, ref feeMatrix, null);
                        }
                    }
                    if (feeMatrix.Status == FeeMatrixs.StatusEnum.Unknown)
                    {
                        // Recherche le premier FeeRequest.OtherParty, différent de celui qui a déjà matché avec OtherParty1, et qui match avec OtherParty2 de feeMatrix
                        // S'il est trouvé, stockage de son index dans feeMatrix.MatchedOtherParty2Index
                        bool isExistOtherParty = false;
                        for (int j = 0; j < countFeeRequestOtherParty; j++)
                        {
                            if (feeMatrix.MatchedOtherParty1Index != j)
                            {
                                isExistOtherParty = true;
                                feeMatrix.Status = FeeMatrixs.StatusEnum.Unknown;
                                CheckParty(pCS, pDbTransaction, TypeSidePartyEnum.OtherParty2, ref feeMatrix, FeeRequest.PartyBroker[j]);

                                if (feeMatrix.Status == FeeMatrixs.StatusEnum.Unknown)
                                {
                                    feeMatrix.MatchedOtherParty2Index = j;
                                    break;
                                }
                            }
                        }
                        if (!isExistOtherParty)
                        {
                            //Il n'existe aucune OtherParty2 sur le trade. Contrôle d'une éventuelle condition en vigueur dans feeMatrix, afin de voir si elle est compatible (ex. None ou All Broker)
                            CheckParty(pCS, pDbTransaction, TypeSidePartyEnum.OtherParty2, ref feeMatrix, null);
                        }
                    }

                    tmp_auditMessage += feeMatrix.CommentForDebug + Cst.CrLf;
                    if (feeMatrix.Status == FeeMatrixs.StatusEnum.Error || feeMatrix.Status == FeeMatrixs.StatusEnum.Unvalid)
                    {
                        det_auditMessage += tmp_auditMessage + Cst.CrLf;
                    }
                    #endregion Ctrl Broker

                    #region Ctrl Party
                    tmp_auditMessage = new StrBuilder();
                    tmp_auditMessage += "Ctrl Party: ";
                    feeMatrix.CommentForDebug = "Party Checked";
                    if (feeMatrix.Status == FeeMatrixs.StatusEnum.Unknown)
                    {
                        //Identification PartyA / PartyB
                        Party Real_PartyA = FeeRequest.PartyA;
                        Party Real_PartyB = FeeRequest.PartyB;
                        if (feeMatrix.FEETYPEPARTYASpecified && feeMatrix.IDPARTYASpecified)
                        {
                            if (FeeRequest.PartyA != null)
                            {
                                //On contrôle si PartyA matche. Si ce n'est pas le cas on inversera PartyA et PartyB pour une autre tentative. 
                                feeMatrix.IsReverseParty = (!CheckActorOrBook(pCS,pDbTransaction, feeMatrix.FEETYPEPARTYA, feeMatrix.IDPARTYA, FeeRequest.PartyA));
                            }
                            if (feeMatrix.IsReverseParty)
                            {
                                //Inversion de PartyA et PartyB 
                                Real_PartyA = FeeRequest.PartyB;
                                Real_PartyB = FeeRequest.PartyA;
                            }
                        }
                        //20081104 PL TRIM 16381 Add "else if" 
                        else if (feeMatrix.FEETYPEPARTYBSpecified && feeMatrix.IDPARTYBSpecified)
                        {
                            if (FeeRequest.PartyB != null)
                            {
                                //On contrôle si PartyB matche. Si ce n'est pas le cas on inversera PartyA et PartyB pour une autre tentative. 
                                feeMatrix.IsReverseParty = (!CheckActorOrBook(pCS, pDbTransaction, feeMatrix.FEETYPEPARTYB, feeMatrix.IDPARTYB, FeeRequest.PartyB));
                            }

                            if (feeMatrix.IsReverseParty)
                            {
                                //Inversion de PartyA et PartyB 
                                Real_PartyA = FeeRequest.PartyB;
                                Real_PartyB = FeeRequest.PartyA;
                            }
                        }
                        //Ctrl Party A / Party B
                        if (feeMatrix.Status == FeeMatrixs.StatusEnum.Unknown)
                            CheckParty(pCS, pDbTransaction, TypeSidePartyEnum.PartyA, ref feeMatrix, Real_PartyA);

                        if (feeMatrix.Status == FeeMatrixs.StatusEnum.Unknown)
                            CheckParty(pCS, pDbTransaction, TypeSidePartyEnum.PartyB, ref feeMatrix, Real_PartyB);
                    }

                    tmp_auditMessage += feeMatrix.CommentForDebug + Cst.CrLf;
                    if (feeMatrix.Status == FeeMatrixs.StatusEnum.Error || feeMatrix.Status == FeeMatrixs.StatusEnum.Unvalid)
                    {
                        det_auditMessage += tmp_auditMessage + Cst.CrLf;
                    }
                    #endregion Ctrl Party

                    if (feeMatrix.Status == FeeMatrixs.StatusEnum.Unknown)
                    {
                        //Status == Unknown, cela veux dire que tous les tests d'environnement ont été passés avec succès
                        feeMatrix.Status = FeeMatrixs.StatusEnum.Valid;
                    }

                    if (FeeRequest.IsWithAuditMsg && (i < maxAuditMsg))
                    {
                        _auditMessage += "Status: " + feeMatrix.Status.ToString().ToUpper() + Cst.CrLf;
                        if (feeMatrix.Status != FeeMatrixs.StatusEnum.Valid)
                        {
                            _auditMessage += "..................................................." + Cst.CrLf;
                            _auditMessage += det_auditMessage;
                        }
                    }
                }
                _auditMessage += "===================================================" + Cst.CrLf2;

                // PL 20130118 Déplacement des 3 méthodes CheckFeeMatrix*() ci-dessous avant le contrôle des Barèmes dérogatoires
                // 20120607 MF Ticket 17864 - check when the fee matrix is targeting a specific strategy type or group, and verify if the current
                //                            trade is making part, or it is compatible with the given strategy 
                //CheckFeeMatrixIsStrategySpecific(feeMatrixs, ref count);
                CheckFeeMatrixIsStrategySpecific_V2(feeMatrixs);

                // 20120807 MF Ticket 18067 - check the extention values of the matrix/schedule in order to verify they match with the extention value
                //                            on the trade. If success, the fee is applied
                CheckFeeMatrixMatchExtention(pCS, pDbTransaction, feeMatrixs);

                // WARNING: ******************************************************************************************************
                //  Cette étape d'exclusion des barèmes généraux pour lesquels il existe un barème dérogatoire 
                //  doit IMPERATIVEMENT être l'ULTIME ETAPE avant de procéder au calcul des frais (items "FeeResponse").
                // ***************************************************************************************************************
                //CheckFeeMatrixOnSpecificSchedule2(feeMatrixs);
                //PL 20221124 PL 20230718 Test FL/Tradition - Use new signature with pCS, pDbTransaction
                CheckFeeMatrixOnSpecificSchedule2(pCS, pDbTransaction, feeMatrixs);

                // WARNING: ******************************************************************************************************
                //  Cette (nouvelle) étape d'exclusion des barèmes "CONTRACT", lorsqu'il existe un barème plus "fin"
                //  doit IMPERATIVEMENT être l'ULTIME-ULTIME ETAPE avant de procéder au calcul des frais.
                // ***************************************************************************************************************
                CheckFeeMatrixOnContractSchedule(feeMatrixs);

                // WARNING: ******************************************************************************************************
                //  Cette (nouvelle) étape d'exclusion des barèmes TRADING/CLEARING, lorsqu'il existe un barème plus "fin"
                //  doit IMPERATIVEMENT être l'ULTIME-ULTIME-ULTIME  ETAPE avant de procéder au calcul des frais.
                // ***************************************************************************************************************
                CheckFeeMatrixOnETDandAllocation2(pCS, pDbTransaction, feeMatrixs);

                #region Balayage des éléments "Valid" issus de la table FEEMATRIX pour créer des items "FeeResponse"
                IEnumerable<FeeMatrix> validFeeMatrix = feeMatrixs.feeMatrix.Where(matrix => matrix.Status == FeeMatrixs.StatusEnum.Valid);
                int countValid = validFeeMatrix.Count();
                if (countValid > 0)
                {
                    int iValid = 0;
                    _feeResponse = new FeeResponse[countValid];
                    foreach (FeeMatrix feeMatrix in validFeeMatrix)
                    {
                        _feeResponse[iValid] = new FeeResponse(this);
                        _feeResponse[iValid].Calc(pCS, pDbTransaction, feeMatrix);

                        if (iValid == 0)
                        {
                            //PL Utilisation du 1er élément pour y associer la piste d'audit (en attendant mieux)
                            _feeResponse[iValid].AuditMessage = _auditMessage.ToString();
                        }

                        iValid++;
                    }
                }
                #endregion

                System.Diagnostics.Debug.WriteLine(_auditMessage);
            }
            #endregion
        }

        // 20120807 MF Ticket 18067
        // EG 20180307 [23769] Gestion dbTransaction
        private void CheckFeeMatrixMatchExtention(string pCS, IDbTransaction pDbTransaction, FeeMatrixs feeMatrixs)
        {
            // 1. get all the valid matrix
            IEnumerable<FeeMatrix> validFeeMatrix = feeMatrixs.feeMatrix.Where(matrix => matrix.Status == FeeMatrixs.StatusEnum.Valid);

            // 2. check for matrix related to any product and with same extention than the trade.
            //    We check either the extention of the fee instruction, either the one of the fee schedule.
            foreach (FeeMatrix feeMatrix in validFeeMatrix)
            {
                bool invalidate = false;

                bool checkExtention =
                    (feeMatrix.IDDEFINEEXTENDDETMXSpecified && feeMatrix.IDDEFINEEXTENDDETMX > 0) ||
                    (feeMatrix.FEESCHED_IDDEFINEEXTENDDETSpecified && feeMatrix.FEESCHED_IDDEFINEEXTENDDET > 0);

                ITrade currentTrade = FeeRequest.DataDocument.CurrentTrade;
                string tradeExtentionValue = Cst.NotAvailable;

                // 3. the trade has not extentions but the extention are required by the instruction/schedule, the fee is invalidated
                if (checkExtention && !currentTrade.ExtendsSpecified)
                {
                    invalidate = true;
                }

                if (checkExtention && currentTrade.ExtendsSpecified)
                {
                    ITradeExtends extends = currentTrade.Extends;

                    // 2.1 check the extention of the fee instruction
                    if (feeMatrix.IDDEFINEEXTENDDETMX > 0)
                    {
                        SQL_DefineExtendDet defineExtendDetMx = new SQL_DefineExtendDet(pCS, feeMatrix.IDDEFINEEXTENDDETMX)
                        {
                            DbTransaction = pDbTransaction
                        };

                        ITradeExtend tradeExtend = extends.TradeExtend.Where(elem => elem.OTCmlId == feeMatrix.IDDEFINEEXTENDDETMX).FirstOrDefault();

                        if (tradeExtend == null || !Compare(tradeExtend, feeMatrix.EXTENDVALUEMX, feeMatrix.EXTENDOPERATORMX, defineExtendDetMx.DataType))
                        {
                            invalidate = true;
                        }
                    }

                    // 2.2 check the extention of the fee schedule
                    if (feeMatrix.FEESCHED_IDDEFINEEXTENDDET > 0)
                    {
                        SQL_DefineExtendDet defineExtendDetSched = new SQL_DefineExtendDet(pCS, feeMatrix.FEESCHED_IDDEFINEEXTENDDET)
                        {
                            DbTransaction = pDbTransaction
                        };

                        ITradeExtend tradeExtend = extends.TradeExtend.Where(elem => elem.OTCmlId == feeMatrix.FEESCHED_IDDEFINEEXTENDDET).FirstOrDefault();

                        if (tradeExtend == null || !Compare(tradeExtend, feeMatrix.FEESCHED_EXTENDVALUE, feeMatrix.FEESCHED_EXTENDOPERATOR, defineExtendDetSched.DataType))
                        {
                            invalidate = true;
                        }
                    }
                }

                if (invalidate)
                {
                    feeMatrix.Status = FeeMatrixs.StatusEnum.Unvalid;
                    // TODO 20120808 pourquoi on  a pas de multilangues sur les erreurs relatives aux strategies?
                    feeMatrix.CommentForDebug =
                        String.Format(@"Instruction desactivated. 
The instruction/schedule extention [instruction value: {0}; schedule value: {1}; instruction operator: {2}; schedule operator: {3}] 
does not match the trade extention [value: {4}].",
                       feeMatrix.IDDEFINEEXTENDDETMX > 0 ? feeMatrix.EXTENDVALUEMX : Cst.NotAvailable,
                       feeMatrix.FEESCHED_IDDEFINEEXTENDDET > 0 ? feeMatrix.FEESCHED_EXTENDVALUE : Cst.NotAvailable,
                       feeMatrix.IDDEFINEEXTENDDETMX > 0 ? feeMatrix.EXTENDOPERATORMX : Cst.NotAvailable,
                       feeMatrix.FEESCHED_IDDEFINEEXTENDDET > 0 ? feeMatrix.FEESCHED_EXTENDOPERATOR : Cst.NotAvailable,
                       tradeExtentionValue);
                }
            }
        }


        /// <summary>
        /// Exclusion des barèmes généraux, lorsqu'il existe un barème dérogatoire de mêmes caractéristiques.
        /// <para>NB: Barèmes dérogatoires (= Barèmes spécifiques à un acteur)</para>
        /// </summary>
        /// <param name="feeMatrixs"></param>
        //PL 20221124 PL 20230718 Test FL/Tradition - New signature - Add pCS, pDbTranscation
        //private void CheckFeeMatrixOnSpecificSchedule2(FeeMatrixs feeMatrixs)
        private void CheckFeeMatrixOnSpecificSchedule2(string pCS, IDbTransaction pDbTransaction, FeeMatrixs feeMatrixs)
        {
            //Balayage des éléments "Valid" issus de la table FEEMATRIX, afin de contrôler l'existence d'éventuels barèmes spécifiques.
            //Si de tels barèmes existes, alors les barèmes généraux (donc non restreints à un IDA) de mêmes caractéristiques sont étiquetés comme "Unvalid".

            // Obtention des matrix valides
            IEnumerable<FeeMatrix> validFeeMatrix = feeMatrixs.feeMatrix.Where(matrix => matrix.Status == FeeMatrixs.StatusEnum.Valid);
            if (validFeeMatrix.Count() > 0)
            {
                // Obtention des matrix valides comportant un barème spécifique
                IEnumerable<FeeMatrix> feeMatrix_Specific = validFeeMatrix.Where(matrix => matrix.FEESCHED_IDASpecified == true);
                if (feeMatrix_Specific.Count() > 0)
                {
                    _auditMessage += "===================================================" + Cst.CrLf;
                    _auditMessage += "Specific schedule(s) - Count: " + feeMatrix_Specific.Count().ToString() + "/" + validFeeMatrix.Count().ToString() + Cst.CrLf;

                    foreach (FeeMatrix feeMatrix_Specific_item in feeMatrix_Specific)
                    {
                        //int ida = feeMatrix_Specific_item.FEESCHED_IDA;
                        int ida_override = feeMatrix_Specific_item.FEESCHED_IDA; //Acteur dérogatoire

                        #region NEW UNIQUE STEP: Invalidation des barèmes spécifiques où l'acteur dérogatoire n'est ni Payer ni Receiver et Invalidation des barèmes généraux de mêmes caractéristiques
                        {
                            feeMatrix_Specific_item.PartyPayer = GetPartyPayerReceiver(feeMatrix_Specific_item.FEEPAYER, feeMatrix_Specific_item.IsReverseParty, feeMatrix_Specific_item.MatchedOtherParty1Index, feeMatrix_Specific_item.MatchedOtherParty2Index);
                            feeMatrix_Specific_item.PartyReceiver = GetPartyPayerReceiver(feeMatrix_Specific_item.FEERECEIVER, feeMatrix_Specific_item.IsReverseParty, feeMatrix_Specific_item.MatchedOtherParty1Index, feeMatrix_Specific_item.MatchedOtherParty2Index);

                            if ((feeMatrix_Specific_item.PartyPayer == null) || (feeMatrix_Specific_item.PartyReceiver == null))
                            {
                                feeMatrix_Specific_item.Status = FeeMatrixs.StatusEnum.Unvalid;
                                feeMatrix_Specific_item.CommentForDebug = "Discarded! Inability to identify the payer and/or receiver.";

                                AddAuditMessage(feeMatrix_Specific_item);
                            }
                            else
                            {
                                //PL 20221124 Test FL/Tradition - Refactoring and Test BookOwner
                                bool participating_OverridingActor = (feeMatrix_Specific_item.PartyPayer.m_Party_Ida == ida_override)
                                                                      || (feeMatrix_Specific_item.PartyReceiver.m_Party_Ida == ida_override);
                                if ((!participating_OverridingActor) && (feeMatrix_Specific_item.PartyPayer.m_Party_Idb > 0))
                                {
                                    //PL 20221124 Test FL/Tradition - Newness - Test BookOwner
                                    SQL_Book tmp_sql_Book = new SQL_Book(pCS, feeMatrix_Specific_item.PartyPayer.m_Party_Idb)
                                    {
                                        DbTransaction = pDbTransaction,
                                        IsUseTable = true
                                    };
                                    if (tmp_sql_Book.IsLoaded)
                                        participating_OverridingActor = (tmp_sql_Book.IdA == ida_override);

                                    if ((!participating_OverridingActor) && (feeMatrix_Specific_item.PartyReceiver.m_Party_Idb > 0))
                                    {
                                        tmp_sql_Book = new SQL_Book(pCS, feeMatrix_Specific_item.PartyReceiver.m_Party_Idb)
                                        {
                                            DbTransaction = pDbTransaction,
                                            IsUseTable = true
                                        };
                                        if (tmp_sql_Book.IsLoaded)
                                            participating_OverridingActor = (tmp_sql_Book.IdA == ida_override);
                                    }
                                }
                                if (participating_OverridingActor)
                                {
                                    //L'acteur dérogatoire est participant au trade en tant que Payer ou Receiver.
                                    //Il l'est soit directement en tant que Dealer soit indirectement en tant que propriétaire du Book Dealer.
                                    //--> Identification de barèmes de même nom et non dérogatoires, sur la base de la même condition, afin de les exclure
                                    IEnumerable<FeeMatrix> feeMatrix_NoSpecific = validFeeMatrix.Where(matrix =>
                                        (
                                               (matrix.FEESCHED_IDASpecified == false)
                                            && (matrix.FEESCHED_IDENTIFIER == feeMatrix_Specific_item.FEESCHED_IDENTIFIER)
                                            && (matrix.IDFEEMATRIX == feeMatrix_Specific_item.IDFEEMATRIX)
                                        )
                                    );
                                    if (feeMatrix_NoSpecific.Count() > 0)
                                    {
                                        UnvalidFeeMatrix(feeMatrix_Specific_item, ref feeMatrix_NoSpecific, "Discarded! Existence of the following Specific schedule:");
                                    }

                                }
                                else
                                {
                                    feeMatrix_Specific_item.Status = FeeMatrixs.StatusEnum.Unvalid;
                                    feeMatrix_Specific_item.CommentForDebug = "Discarded! Actor is neither payer (" + feeMatrix_Specific_item.PartyPayer.m_Party_PartyId + ") nor receiver (" + feeMatrix_Specific_item.PartyReceiver.m_Party_PartyId + ").";

                                    AddAuditMessage(feeMatrix_Specific_item);
                                }
                            }
                        }
                        #endregion NEW UNIQUE STEP
                    }

                    _auditMessage += "===================================================" + Cst.CrLf2;
                }
            }
        }
        

        /// <summary>
        /// Exclusion des barèmes "CONTRACT", lorsqu'il existe un barème plus "fin".
        /// </summary>
        /// <param name="feeMatrixs"></param>
        /// FI 20170908 [23409] Modify
        /// FI 20180209 [23782] Modify
        private void CheckFeeMatrixOnContractSchedule(FeeMatrixs feeMatrixs)
        {
            #region CONTRACT
            IEnumerable<FeeMatrix> validFeeMatrix = feeMatrixs.feeMatrix.Where(matrix => matrix.Status == FeeMatrixs.StatusEnum.Valid);
            if (validFeeMatrix.Count() > 0)
            {
                // Obtention des matrix valides dédiées à un unique Contract
                // FI 20170908 [23409] Contract remplacé par DerivativeContract et Ajout de 
                // FI 20180209 [23782] Usage d'un || (OR) TypeContract = DerivativeContract ou TypeContract = CommodityContract
                IEnumerable<FeeMatrix> feeMatrix_Contract = validFeeMatrix.Where(matrix =>
                    (matrix.FEESCHED_IDASpecified == true)
                    &&
                    (matrix.FEESCHED_TYPECONTRACTSpecified == true) && ((matrix.FEESCHED_TYPECONTRACT == TypeContractEnum.DerivativeContract.ToString()) || 
                                                                        (matrix.FEESCHED_TYPECONTRACT == TypeContractEnum.CommodityContract.ToString()))
                    &&
                    (matrix.FEESCHED_IDCONTRACTSpecified == true) && (matrix.FEESCHED_IDCONTRACT > 0)

                );
                if (feeMatrix_Contract.Count() > 0)
                {
                    _auditMessage += "===================================================" + Cst.CrLf;
                    _auditMessage += "Contract schedule(s) - Count: " + feeMatrix_Contract.Count().ToString() + "/" + validFeeMatrix.Count().ToString() + Cst.CrLf;

                    foreach (FeeMatrix feeMatrix_Contract_item in feeMatrix_Contract)
                    {
                        //Identification de barèmes de même nom et non dédiés à un unique Contract, sur la base de la même condition, afin de les exclure
                        // FI 20180209 [23782] typeContract != DerivativeContract && typeContract != CommodityContract
                        IEnumerable<FeeMatrix> feeMatrix_NoContract = validFeeMatrix.Where(matrix =>
                            (matrix.FEESCHED_IDASpecified == true)
                            &&
                            (
                                (matrix.FEESCHED_TYPECONTRACTSpecified == false)
                                ||
                                (matrix.FEESCHED_TYPECONTRACT == TypeContractEnum.None.ToString())
                                ||
                                (
                                    (matrix.FEESCHED_TYPECONTRACT != TypeContractEnum.DerivativeContract.ToString()) // FI 20170908 [23409] Contract remplacé par DerivativeContract
                                    &&
                                    (matrix.FEESCHED_TYPECONTRACT != TypeContractEnum.CommodityContract.ToString()) // FI 20170908 [23409] Add
                                )
                            )
                        );
                        if (feeMatrix_NoContract.Count() > 0)
                        {
                            UnvalidFeeMatrix(feeMatrix_Contract_item, ref feeMatrix_NoContract, "Discarded! Existence of the following Contract schedule:");
                        }
                    }

                    _auditMessage += "===================================================" + Cst.CrLf2;
                }
            }
            #endregion CONTRACT
            #region GRPCONTRACT
            validFeeMatrix = feeMatrixs.feeMatrix.Where(matrix => matrix.Status == FeeMatrixs.StatusEnum.Valid);
            if (validFeeMatrix.Count() > 0)
            {
                // Obtention des matrix valides dédiées à un Grp.Contract
                IEnumerable<FeeMatrix> feeMatrix_GrpContract = validFeeMatrix.Where(matrix =>
                    (matrix.FEESCHED_IDASpecified == true)
                    &&
                    (matrix.FEESCHED_TYPECONTRACTSpecified == true) && (matrix.FEESCHED_TYPECONTRACT == TypeContractEnum.GrpContract.ToString())
                    &&
                    (matrix.FEESCHED_IDCONTRACTSpecified == true) && (matrix.FEESCHED_IDCONTRACT > 0)
                );
                if (feeMatrix_GrpContract.Count() > 0)
                {
                    _auditMessage += "===================================================" + Cst.CrLf;
                    _auditMessage += "Grp.Contract schedule(s) - Count: " + feeMatrix_GrpContract.Count().ToString() + "/" + validFeeMatrix.Count().ToString() + Cst.CrLf;

                    foreach (FeeMatrix feeMatrix_GrpContract_item in feeMatrix_GrpContract)
                    {
                        //Identification de barèmes de même nom et non dédiés à un Grp.Contract, sur la base de la même condition, afin de les exclure
                        IEnumerable<FeeMatrix> feeMatrix_NoGrpContract = validFeeMatrix.Where(matrix =>
                            (matrix.FEESCHED_IDASpecified == true)
                            &&
                            (
                                (matrix.FEESCHED_TYPECONTRACTSpecified == false)
                                ||
                                (matrix.FEESCHED_TYPECONTRACT == TypeContractEnum.None.ToString())
                                ||
                                (
                                    (matrix.FEESCHED_TYPECONTRACT != TypeContractEnum.DerivativeContract.ToString()) // FI 20170908 [23409] Contract remplacé par DerivativeContract
                                    &&
                                    (matrix.FEESCHED_TYPECONTRACT != TypeContractEnum.CommodityContract.ToString())  // FI 20170908 [23409] Add 
                                    &&
                                    (matrix.FEESCHED_TYPECONTRACT != TypeContractEnum.GrpContract.ToString())
                                )
                            )
                        );
                        if (feeMatrix_NoGrpContract.Count() > 0)
                        {
                            UnvalidFeeMatrix(feeMatrix_GrpContract_item, ref feeMatrix_NoGrpContract, "Discarded! Existence of the following Grp.Contract schedule:");
                        }
                    }

                    _auditMessage += "===================================================" + Cst.CrLf2;
                }
            }
            #endregion GRPCONTRACT
            #region MARKET
            validFeeMatrix = feeMatrixs.feeMatrix.Where(matrix => matrix.Status == FeeMatrixs.StatusEnum.Valid);
            if (validFeeMatrix.Count() > 0)
            {
                // Obtention des matrix valides dédiées à un unique Market
                IEnumerable<FeeMatrix> feeMatrix_Market = validFeeMatrix.Where(matrix =>
                    (matrix.FEESCHED_IDASpecified == true)
                    &&
                    (matrix.FEESCHED_TYPECONTRACTSpecified == true) && (matrix.FEESCHED_TYPECONTRACT == TypeContractEnum.Market.ToString())
                    &&
                    (matrix.FEESCHED_IDCONTRACTSpecified == true) && (matrix.FEESCHED_IDCONTRACT > 0)

                );
                if (feeMatrix_Market.Count() > 0)
                {
                    _auditMessage += "===================================================" + Cst.CrLf;
                    _auditMessage += "Market schedule(s) - Count: " + feeMatrix_Market.Count().ToString() + "/" + validFeeMatrix.Count().ToString() + Cst.CrLf;

                    foreach (FeeMatrix feeMatrix_Market_item in feeMatrix_Market)
                    {
                        //Identification de barèmes de même nom et non dédiés à un unqiue Market, sur la base de la même condition, afin de les exclure
                        IEnumerable<FeeMatrix> feeMatrix_NoMarket = validFeeMatrix.Where(matrix =>
                            (matrix.FEESCHED_IDASpecified == true)
                            &&
                            (
                                (matrix.FEESCHED_TYPECONTRACTSpecified == false)
                                ||
                                (matrix.FEESCHED_TYPECONTRACT == TypeContractEnum.None.ToString())
                                ||
                                (
                                    (matrix.FEESCHED_TYPECONTRACT != TypeContractEnum.DerivativeContract.ToString()) // FI 20170908 [23409] Contract remplacé par DerivativeContract
                                    &&
                                    (matrix.FEESCHED_TYPECONTRACT != TypeContractEnum.CommodityContract.ToString())  // FI 20170908 [23409] Add 
                                    &&
                                    (matrix.FEESCHED_TYPECONTRACT != TypeContractEnum.GrpContract.ToString())
                                    &&
                                    (matrix.FEESCHED_TYPECONTRACT != TypeContractEnum.Market.ToString())
                                )
                            )
                        );
                        if (feeMatrix_NoMarket.Count() > 0)
                        {
                            UnvalidFeeMatrix(feeMatrix_Market_item, ref feeMatrix_NoMarket, "Discarded! Existence of the following Market schedule:");
                        }
                    }

                    _auditMessage += "===================================================" + Cst.CrLf2;
                }
            }
            #endregion MARKET
            #region GRPMARKET
            validFeeMatrix = feeMatrixs.feeMatrix.Where(matrix => matrix.Status == FeeMatrixs.StatusEnum.Valid);
            if (validFeeMatrix.Count() > 0)
            {
                // Obtention des matrix valides dédiées à un Grp.Market
                IEnumerable<FeeMatrix> feeMatrix_GrpMarket = validFeeMatrix.Where(matrix =>
                    (matrix.FEESCHED_IDASpecified == true)
                    &&
                    (matrix.FEESCHED_TYPECONTRACTSpecified == true) && (matrix.FEESCHED_TYPECONTRACT == TypeContractEnum.GrpMarket.ToString())
                    &&
                    (matrix.FEESCHED_IDCONTRACTSpecified == true) && (matrix.FEESCHED_IDCONTRACT > 0)

                );
                if (feeMatrix_GrpMarket.Count() > 0)
                {
                    _auditMessage += "===================================================" + Cst.CrLf;
                    _auditMessage += "Grp.Market schedule(s) - Count: " + feeMatrix_GrpMarket.Count().ToString() + "/" + validFeeMatrix.Count().ToString() + Cst.CrLf;

                    foreach (FeeMatrix feeMatrix_GrpMarket_item in feeMatrix_GrpMarket)
                    {
                        //Identification de barèmes de même nom et non dédiés à un Grp.Market, sur la base de la même condition, afin de les exclure
                        IEnumerable<FeeMatrix> feeMatrix_NoGrpMarket = validFeeMatrix.Where(matrix =>
                            (matrix.FEESCHED_IDASpecified == true)
                            &&
                            (
                                (matrix.FEESCHED_TYPECONTRACTSpecified == false)
                                ||
                                (matrix.FEESCHED_TYPECONTRACT == TypeContractEnum.None.ToString())
                                //||
                                //(
                                //    (matrix.FEESCHED_TYPECONTRACT != TypeContractEnum.Contract.ToString())
                                //    &&
                                //    (matrix.FEESCHED_TYPECONTRACT != TypeContractEnum.GrpContract.ToString())
                                //    &&
                                //    (matrix.FEESCHED_TYPECONTRACT != TypeContractEnum.Market.ToString())
                                //    &&
                                //    (matrix.FEESCHED_TYPECONTRACT != TypeContractEnum.GrpMarket.ToString())
                                //)
                            )
                        );
                        if (feeMatrix_NoGrpMarket.Count() > 0)
                        {
                            UnvalidFeeMatrix(feeMatrix_GrpMarket_item, ref feeMatrix_NoGrpMarket, "Discarded! Existence of the following Grp.Market schedule:");
                        }
                    }

                    _auditMessage += "===================================================" + Cst.CrLf2;
                }
            }
            #endregion GRPMARKET
        }

        /// <summary>
        /// Exclusion des barèmes TRADING/CLEARING, lorsqu'il existe un barème plus "fin".
        /// </summary>
        /// <param name="feeMatrixs"></param>
        private void CheckFeeMatrixOnETDandAllocation2(string pCS, IDbTransaction pDbTransaction, FeeMatrixs feeMatrixs)
        {
            if (FeeRequest.TradeInput.IsETDandAllocation)
            {
                // Obtention des matrix valides
                IEnumerable<FeeMatrix> validFeeMatrix = feeMatrixs.feeMatrix.Where(matrix => matrix.Status == FeeMatrixs.StatusEnum.Valid);
                if (validFeeMatrix.Count() > 0)
                {
                    Boolean isGiveUp = FeeRequest.TradeInput.IsGiveUp(pCS, pDbTransaction);
                    Boolean isTakeUp = FeeRequest.TradeInput.IsTakeUp(pCS, pDbTransaction);
                    
                    bool isTrade_TradingAndClearing = !isTakeUp && !isGiveUp;
                    bool isTrade_TradingOnly = (!isTakeUp) && (isGiveUp);
                    bool isTrade_ClearingOnly = (isTakeUp) && (!isGiveUp);

                    string commentForDebug = null;
                    IEnumerable<FeeMatrix> feeMatrix_Prioritary = null;

                    if (isTrade_TradingAndClearing)
                    {
                        //Trade de type Trading and Clearing (Full-Service)
                        commentForDebug = "Discarded! Existence of the following Trading and Clearing schedule!";
                        feeMatrix_Prioritary = validFeeMatrix.Where(matrix => matrix.FEESCHED_ISTRADINGCLEARING == true);
                    }
                    else if (isTrade_TradingOnly)
                    {
                        //Trade de type Trading seul (Give-Up)
                        commentForDebug = "Discarded! Existence of the following Trading only schedule!";
                        feeMatrix_Prioritary = validFeeMatrix.Where(matrix => matrix.FEESCHED_ISTRADINGONLY == true);
                    }
                    else if (isTrade_ClearingOnly)
                    {
                        //Trade de type Clearing seul (Take-Up)
                        commentForDebug = "Discarded! Existence of the following Clearing only schedule!";
                        feeMatrix_Prioritary = validFeeMatrix.Where(matrix => matrix.FEESCHED_ISCLEARINGONLY == true);
                    }

                    if (feeMatrix_Prioritary != null)
                    {
                        //Si Trade de type Trading and Clearing (Full-Service) et il existe un ou plusieurs barèmes dédiés au "Trading and Clearing"
                        //--> Exclusion des barèmes uniquement "Trading" ou "Clearing" de mêmes caractéristiques
                        //Sinon si Trade de type Trading seul (Give-Up) et il existe un ou plusieurs barèmes dédiés au "Trading seul" 
                        //--> Exclusion des barèmes "Trading global" de mêmes caractéristiques
                        //Sinon si Trade de type Clearing seul (Take-Up) et il existe un ou plusieurs barèmes dédiés au "Clearing seul" 
                        //--> Exclusion des barèmes "Clearing global" de mêmes caractéristiques

                        bool isSetHeaderAuditMessage = false;
                        foreach (FeeMatrix feeMatrix_Prioritary_item in feeMatrix_Prioritary)
                        {
                            if (!isSetHeaderAuditMessage)
                            {
                                isSetHeaderAuditMessage = true;
                                _auditMessage += "===================================================" + Cst.CrLf;
                                _auditMessage += "Trading/Clearing schedule(s) - Count: " + feeMatrix_Prioritary.Count().ToString() + "/" + validFeeMatrix.Count().ToString() + Cst.CrLf;
                            }

                            IEnumerable<FeeMatrix> feeMatrixClearing_Secondary = validFeeMatrix.Where(matrix =>
                                (
                                      (isTrade_TradingAndClearing && (matrix.FEESCHED_ISTRADINGCLEARING == false))
                                    || (isTrade_TradingOnly && (matrix.FEESCHED_ISTRADING == true))
                                    || (isTrade_ClearingOnly && (matrix.FEESCHED_ISCLEARING == true))
                                )
                             );

                            UnvalidFeeMatrix(feeMatrix_Prioritary_item, ref feeMatrixClearing_Secondary, commentForDebug);
                        }

                        if (isSetHeaderAuditMessage)
                        {
                            _auditMessage += "===================================================" + Cst.CrLf2;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Unvalidation des FeeMatrix de mêmes caractéristiques qu'un élément FeeMatrix de référence
        /// </summary>
        /// <param name="pFeeMatrix"></param>
        /// <param name="opFeeMatrix"></param>
        /// <param name="pCommentForDebug"></param>
        /// <returns></returns>
        private bool UnvalidFeeMatrix(FeeMatrix pBenchmarkMatrix, ref IEnumerable<FeeMatrix> opDiscardMatrix, string pCommentForDebug)
        {
            bool ret = false;

            IEnumerable<FeeMatrix> discardMatrix = opDiscardMatrix.Where(matrix =>
                                (
                                    //Si issus de la même Matrix on considère de fait que les caractéristiques sont semblables
                                    (matrix.IDFEEMATRIX == pBenchmarkMatrix.IDFEEMATRIX)
                                    ||
                                    //Sinon on considère que les caractéristiques sont semblables si les propriétés suivantes sont identiques
                                    (
                                           (matrix.FEE_PAYMENTTYPE == pBenchmarkMatrix.FEE_PAYMENTTYPE)
                                        && (matrix.FEE_EVENTCODE == pBenchmarkMatrix.FEE_EVENTCODE || (StrFunc.IsEmpty(matrix.FEE_EVENTCODE) && StrFunc.IsEmpty(pBenchmarkMatrix.FEE_EVENTCODE)))
                                        && (matrix.FEE_EVENTTYPE == pBenchmarkMatrix.FEE_EVENTTYPE)

                                        && (matrix.FEETYPEPARTYA == pBenchmarkMatrix.FEETYPEPARTYA)
                                        && (matrix.IDPARTYASpecified == pBenchmarkMatrix.IDPARTYASpecified) && (matrix.IDPARTYA == pBenchmarkMatrix.IDPARTYA)
                                        && (matrix.IDROLEACTOR_PARTYASpecified == pBenchmarkMatrix.IDROLEACTOR_PARTYASpecified) && (matrix.IDROLEACTOR_PARTYA == pBenchmarkMatrix.IDROLEACTOR_PARTYA)
                                        && (matrix.SIDEPARTYASpecified == pBenchmarkMatrix.SIDEPARTYASpecified) && (matrix.SIDEPARTYA == pBenchmarkMatrix.SIDEPARTYA)
                                        && (matrix.INTENTIONPARTYASpecified == pBenchmarkMatrix.INTENTIONPARTYASpecified) && (matrix.INTENTIONPARTYA == pBenchmarkMatrix.INTENTIONPARTYA)

                                        && (matrix.FEETYPEPARTYBSpecified == pBenchmarkMatrix.FEETYPEPARTYBSpecified) && (matrix.FEETYPEPARTYB == pBenchmarkMatrix.FEETYPEPARTYB)
                                        && (matrix.IDPARTYBSpecified == pBenchmarkMatrix.IDPARTYBSpecified) && (matrix.IDPARTYB == pBenchmarkMatrix.IDPARTYB)
                                        && (matrix.IDROLEACTOR_PARTYBSpecified == pBenchmarkMatrix.IDROLEACTOR_PARTYBSpecified) && (matrix.IDROLEACTOR_PARTYB == pBenchmarkMatrix.IDROLEACTOR_PARTYB)
                                        && (matrix.SIDEPARTYBSpecified == pBenchmarkMatrix.SIDEPARTYBSpecified) && (matrix.SIDEPARTYB == pBenchmarkMatrix.SIDEPARTYB)
                                        && (matrix.INTENTIONPARTYBSpecified == pBenchmarkMatrix.INTENTIONPARTYBSpecified) && (matrix.INTENTIONPARTYB == pBenchmarkMatrix.INTENTIONPARTYB)

                                        && (matrix.FEETYPEOTHERPARTY1Specified == pBenchmarkMatrix.FEETYPEOTHERPARTY1Specified) && (matrix.FEETYPEOTHERPARTY1 == pBenchmarkMatrix.FEETYPEOTHERPARTY1)
                                        && (matrix.IDOTHERPARTY1Specified == pBenchmarkMatrix.IDOTHERPARTY1Specified) && (matrix.IDOTHERPARTY1 == pBenchmarkMatrix.IDOTHERPARTY1)
                                        && (matrix.IDROLEACTOR_OPART1Specified == pBenchmarkMatrix.IDROLEACTOR_OPART1Specified) && (matrix.IDROLEACTOR_OPART1 == pBenchmarkMatrix.IDROLEACTOR_OPART1)

                                        && (matrix.FEETYPEOTHERPARTY2Specified == pBenchmarkMatrix.FEETYPEOTHERPARTY2Specified) && (matrix.FEETYPEOTHERPARTY2 == pBenchmarkMatrix.FEETYPEOTHERPARTY2)
                                        && (matrix.IDOTHERPARTY2Specified == pBenchmarkMatrix.IDOTHERPARTY2Specified) && (matrix.IDOTHERPARTY2 == pBenchmarkMatrix.IDOTHERPARTY2)
                                        && (matrix.IDROLEACTOR_OPART2Specified == pBenchmarkMatrix.IDROLEACTOR_OPART2Specified) && (matrix.IDROLEACTOR_OPART2 == pBenchmarkMatrix.IDROLEACTOR_OPART2)

                                        && (matrix.FEEPAYER == pBenchmarkMatrix.FEEPAYER)
                                        && (matrix.FEERECEIVER == pBenchmarkMatrix.FEERECEIVER)
                                    )
                                )
                            );

            foreach (FeeMatrix discardMatrix_item in discardMatrix)
            {
                ret = true;

                discardMatrix_item.Status = FeeMatrixs.StatusEnum.Unvalid;
                discardMatrix_item.CommentForDebug = pCommentForDebug;

                if (FeeRequest.IsWithAuditMsg)
                {
                    AddAuditMessage(discardMatrix_item);
                    _auditMessage += "- " + MatrixScheduleCharacteristics(pBenchmarkMatrix);
                }
            }

            return ret;
        }
        private string MatrixScheduleCharacteristics(FeeMatrix pFeeMatrix)
        {
            return "Matrix: " + pFeeMatrix.IDENTIFIER + " / " + pFeeMatrix.DISPLAYNAME + " (id:" + pFeeMatrix.IDFEEMATRIX + ")" + Cst.Space4
                 + "Schedule: " + pFeeMatrix.FEESCHED_IDENTIFIER + " / " + pFeeMatrix.FEESCHED_DISPLAYNAME + " (id:" + pFeeMatrix.IDFEESCHEDULE + ")"
                                + (pFeeMatrix.FEESCHED_IDASpecified ? " Actor: " + pFeeMatrix.FEESCHED_IDA_Identifier + " / " + pFeeMatrix.FEESCHED_IDA_Displayname + " (id: " + pFeeMatrix.FEESCHED_IDA.ToString() + ")" : string.Empty) + Cst.CrLf;
        }
        private void AddAuditMessage(FeeMatrix pFeeMatrix)
        {
            if (FeeRequest.IsWithAuditMsg)
            {
                _auditMessage += "---------------------------------------------------" + Cst.CrLf;
                _auditMessage += MatrixScheduleCharacteristics(pFeeMatrix);
                _auditMessage += "Status: " + pFeeMatrix.Status.ToString().ToUpper() + Cst.CrLf;
                _auditMessage += "..................................................." + Cst.CrLf;
                _auditMessage += pFeeMatrix.CommentForDebug + Cst.CrLf;
            }
        }


        // 20120807 MF Ticket 18067
        // UNDONE 20120809 MF - where to put this method to make it visible to all the spheres component. Implementing the interface
        //                      is not good because we need to implement the interface explicitly for all the versions of the EFSml objects.
        /// <summary>
        /// Check if the provided scheme obkect matches the value of the given input value according with the given operator
        /// </summary>
        /// <param name="pScheme">the scheme obkect we want to compare the value</param>
        /// <param name="pValue">the input value to compare with the current extention value</param>
        /// <param name="pOperator">operator used to match the extention values, possible values: "equals","different"
        /// (see also DDLLoad_EFSOperators)</param>
        /// <param name="pDataType">datatype of the extend value, possible values: TypeDataEnum enumeration. 
        /// IF null, a string type is assumed</param>
        /// <returns>true when the input value is equals to the current extention value</returns>
        private bool Compare(IScheme pScheme, string pValue, string pOperator, string pDataType)
        {
            bool equals = false;

            // The compare is type driven, we check just a subset of the possible types defined inside of the TypeDataEnum enumeration
            switch (pDataType)
            {
                case "bool":
                case "boolean":
                case "bool2v":
                case "bool2h":
                    {
                        bool parsed = bool.TryParse(pValue, out bool inputValue);
                        parsed = bool.TryParse(pScheme.Value, out bool schemeValue) && parsed;
                        if (parsed)
                            equals = inputValue == schemeValue;
                    }
                    break;
                case "date":
                case "datetime":
                case "time":
                    {
                       EFS_DateTime inputValue = new EFS_DateTime(pValue);
                       if (DateTime.TryParse(pScheme.Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime schemeValue))
                            equals = inputValue.DateTimeValue == schemeValue;
                    }
                    break;
                case "integer":
                case "int":
                    {
                        bool parsed = int.TryParse(pValue, out int inputValue);
                        parsed = int.TryParse(pScheme.Value, out int schemeValue) && parsed;
                        if (parsed)
                            equals = inputValue == schemeValue;
                    }
                    break;

                case "decimal":
                case "dec":
                    {

                        bool parsed = decimal.TryParse(pValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal inputValue);
                        parsed = decimal.TryParse(pScheme.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal schemeValue) && parsed;
                        if (parsed)
                            equals = inputValue == schemeValue;
                    }
                    break;
                default:
                case "string":
                    equals = (String.IsNullOrEmpty(pValue) && String.IsNullOrEmpty(pScheme.Value)) || pValue == pScheme.Value;
                    break;
            }

            bool success = false;

            switch (pOperator)
            {
                case "equals":
                    success = equals;
                    break;

                case "different":
                    success = !equals;
                    break;
            }

            return success;
        }

        // PL 20180305 
        private void CheckFeeMatrixIsStrategySpecific_V2(FeeMatrixs feeMatrixs)
        {
            // 1. get all the valid matrix
            IEnumerable<FeeMatrix> validFeeMatrix = feeMatrixs.feeMatrix.Where(matrix => matrix.Status == FeeMatrixs.StatusEnum.Valid);

            // 2. Strategy check for matrix targeting strategy
            foreach (FeeMatrix feeMatrix in validFeeMatrix)
            {
                // 2.1 check when the matrix is targeting a specific strategy (used ONLY for strategy from trades ALLOC, see point 5.)
                bool isMatrixReservedToStrategy = (!String.IsNullOrEmpty(feeMatrix.STRATEGYTYPEMX) && (feeMatrix.STRATEGYTYPEMX.ToUpper() != "NONE"));
                bool isScheduleReservedToStrategy = (!String.IsNullOrEmpty(feeMatrix.FEESCHED_STRATEGYTYPESCHED) && (feeMatrix.FEESCHED_STRATEGYTYPESCHED.ToUpper() != "NONE"));

                // 2.2 check if the current trade is a strategy ORDER/EXECUTION (then isExecutionStrategyETD turns into true)
                bool isEXECUTION_StrategyETD = FeeRequest.Product is StrategyContainer container && container.ContainsOnlyETD;

                // 2.3 check if the current trade is making part of a strategy issued 
                //     trades ALLOC provided of an electronic order (then isAllocStrategyETD turns into true)
                ExchangeTradedDerivativeContainer etdContainer = null;
                bool isALLOC_StrategyETD = false;
                if (FeeRequest.Product.IsExchangeTradedDerivative)
                {
                    etdContainer = new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)FeeRequest.Product.Product);
                    if (etdContainer.TradeCaptureReport.SecSubTyp != null)
                        isALLOC_StrategyETD = true;
                }

                // 3. strategy specific but the current trade is not a strategy
                if ((isMatrixReservedToStrategy || isScheduleReservedToStrategy) && (!isEXECUTION_StrategyETD) && (!isALLOC_StrategyETD))
                {
                    feeMatrix.Status = FeeMatrixs.StatusEnum.Unvalid;
                    feeMatrix.CommentForDebug = "Current ALLOC or EXECUTION is not a strategy";
                }

                // 4. Check for execution strategy if the fee must be applied on all the legs, when yes then duplicate the fee for all
                //    the resting legs
                if (isEXECUTION_StrategyETD && feeMatrix.FEESCHED_ISAPPLYONALLLEGS)
                {
                    // 4.1 duplicate the fee matrix for all the option legs
                    int legs =
                        ((StrategyContainer)FeeRequest.Product).SubProduct
                        // the specification (Ticket 17863) take care of options only for ORDER trades typeof strategy
                        .Where(elem => ((IExchangeTradedDerivative)elem).Category.Value == CfiCodeCategoryEnum.Option)
                        .Count();

                    List<FeeMatrix> additionalFeeMatrix = new List<FeeMatrix>();

                    // idxLeg = 1, the first leg is already target of the current matrix, we have to skip it
                    // PL-MF quel est exactement le principe ?
                    for (int idxLeg = 1; idxLeg < legs; idxLeg++)
                    {
                        additionalFeeMatrix.Add((FeeMatrix)feeMatrix.Clone());
                    }

                    feeMatrixs.feeMatrix = feeMatrixs.feeMatrix.Concat(additionalFeeMatrix).ToArray();
                }

                // 5. Check if the current leg of the trade ALLOC has to be concerned by the fee 
                //    (if the fee has to be applied on all legs then just the trades on the first leg will be concerned by)
                //PL-MF je ne comprends pas le commentaire ci-dessus !
                if ((isMatrixReservedToStrategy || isScheduleReservedToStrategy) && isALLOC_StrategyETD)
                {
                    if (feeMatrix.FEESCHED_ISAPPLYONALLLEGS || etdContainer.TradeCaptureReport.LegNo == 1)
                    {
                        // 5.1 Check if trade is part of a compatible strategy, if not unvalid the strategy
                        if (!CheckFeeMatrixStrategyType_V2(etdContainer.TradeCaptureReport.SecSubTyp, feeMatrix.FEESCHED_STRATEGYTYPESCHED, feeMatrix.STRATEGYTYPEMX))
                        {
                            feeMatrix.Status = FeeMatrixs.StatusEnum.Unvalid;
                            feeMatrix.CommentForDebug = "Current trade ALLOC has not the right strategy type";
                        }
                    }
                    else
                    {
                        feeMatrix.Status = FeeMatrixs.StatusEnum.Unvalid;
                        feeMatrix.CommentForDebug = "Current trade ALLOC leg has not to be evaluated";
                    }
                }
            }
        }

        private static bool CheckFeeMatrixStrategyType_V2(string pTradeCaptureReportStrategyType, string pStrategyTypeSched, string pStrategyTypeMx)
        {
            bool ret = true;

            bool isMatrixReservedToStrategy = (!String.IsNullOrEmpty(pStrategyTypeMx) && (pStrategyTypeMx.ToUpper() != "NONE"));
            if (ret && isMatrixReservedToStrategy)
                ret &= StrategyEnumRepository.IsStrategyPartOf(pTradeCaptureReportStrategyType, pStrategyTypeMx);

            bool isScheduleReservedToStrategy = (!String.IsNullOrEmpty(pStrategyTypeSched) && (pStrategyTypeSched.ToUpper() != "NONE"));
            if (ret && isScheduleReservedToStrategy)
                ret &= StrategyEnumRepository.IsStrategyPartOf(pTradeCaptureReportStrategyType, pStrategyTypeSched);
                        
            return ret;
        }

        /// <summary>
        /// Vérifie la cohérence de l'ACTEUR ou du BOOK entre FEEMATRIX et FEEREQUEST
        /// </summary>
        /// <param name="pFEETYPEPARTY"></param>
        /// <param name="pMatrix_IDAorIDB"></param>
        /// <param name="pRequest_Party"></param>
        /// <returns></returns>
        private bool CheckActorOrBook(string pCS, IDbTransaction pDbTransaction, string pFEETYPEPARTY, int pMatrix_IDAorIDB, PartyBase pRequest_Party)
        {
            if (pRequest_Party == null)
            {
                //Partie inexitante dans le trade (ex. OtherParty)
                return CheckActorOrBook(pCS, pDbTransaction, pFEETYPEPARTY, pMatrix_IDAorIDB, null);
            }
            else
            {
                if (pRequest_Party.GetType().Equals(typeof(Party)))
                {
                    //Buyer or Seller
                    return CheckActorOrBook(pCS, pDbTransaction, pFEETYPEPARTY, pMatrix_IDAorIDB, (Party)pRequest_Party);
                }
                else
                {
                    //OtherParty
                    Party party = new Party
                    {
                        m_Party_Ida = ((OtherParty)pRequest_Party).m_Party_Ida,
                        m_Party_Role = ((OtherParty)pRequest_Party).m_Party_Role
                    };

                    return CheckActorOrBook(pCS, pDbTransaction, pFEETYPEPARTY, pMatrix_IDAorIDB, party);
                }
            }
        }
        /// <summary>
        /// Vérifie la cohérence de l'ACTEUR ou du BOOK entre FEEMATRIX et FEEREQUEST
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pFEETYPEPARTY"></param>
        /// <param name="pMatrix_IDAorIDB"></param>
        /// <param name="pRequest_Party"></param>
        /// <returns></returns>
        // EG 20180307 [23769] Gestion dbTransaction
        private bool CheckActorOrBook(string pCS, IDbTransaction pDbTransaction, string pFEETYPEPARTY, int pMatrix_IDAorIDB, Party pRequest_Party)
        {
            bool ret = false;
            Object obj;
            DataParameters parameters = new DataParameters();
            string select;

            TypePartyEnum FEETYPEPARTY = (TypePartyEnum)Enum.Parse(typeof(TypePartyEnum), pFEETYPEPARTY);
            switch (FEETYPEPARTY)
            {
                case TypePartyEnum.GrpActor:
                    select = SQLCst.SELECT + "1" + Cst.CrLf;
                    select += SQLCst.FROM_DBO + Cst.OTCml_TBL.GACTOR.ToString() + " ga" + Cst.CrLf;
                    select += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTORG.ToString() + " ag on (ag.IDGACTOR=ga.IDGACTOR) and (ag.IDA=@IDA)";
                    select += SQLCst.WHERE + "(ga.IDGACTOR=@ID)";

                    parameters.Add(new DataParameter(pCS, "ID", DbType.Int32), pMatrix_IDAorIDB);
                    parameters.Add(new DataParameter(pCS, "IDA", DbType.Int32), pRequest_Party.m_Party_Ida);
                    obj = DataHelper.ExecuteScalar(pCS, pDbTransaction, CommandType.Text, select, parameters.GetArrayDbParameter());
                    ret = (obj != null);
                    break;

                case TypePartyEnum.Actor:
                    ret = (pMatrix_IDAorIDB == pRequest_Party.m_Party_Ida);
                    break;

                case TypePartyEnum.GrpBook:
                    select = SQLCst.SELECT + "1" + Cst.CrLf;
                    select += SQLCst.FROM_DBO + Cst.OTCml_TBL.GBOOK.ToString() + " gb" + Cst.CrLf;
                    select += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.BOOKG.ToString() + " bg on (bg.IDGBOOK=gb.IDGBOOK) and (bg.IDB=@IDB)";
                    select += SQLCst.WHERE + "(gb.IDGBOOK=@ID)";

                    parameters.Add(new DataParameter(pCS, "ID", DbType.Int32), pMatrix_IDAorIDB);
                    parameters.Add(new DataParameter(pCS, "IDB", DbType.Int32), pRequest_Party.m_Party_Idb);
                    obj = DataHelper.ExecuteScalar(pCS, pDbTransaction, CommandType.Text, select, parameters.GetArrayDbParameter());
                    ret = (obj != null);
                    break;

                case TypePartyEnum.Book:
                    ret = (pMatrix_IDAorIDB == pRequest_Party.m_Party_Idb);
                    break;
            }
            return ret;
        }

        /// <summary>
        /// Vérifie la cohérence de l'ensemble des caractéristiques d'une PARTY entre FEEMATRIX et FEEREQUEST
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTypeSide"></param>
        /// <param name="opFeeMatrix"></param>
        /// <param name="pParty"></param>
        /// <returns></returns>
        // EG 20180307 [23769] Gestion dbTransaction
        private void CheckParty(string pCS, IDbTransaction pDbTransaction, TypeSidePartyEnum pTypeSide, ref FeeMatrix opFeeMatrix, PartyBase pRequest_Party)
        {
            //Note: cette méthode est appelée pour chaque Party, le paramètre "pTypeSide" qualifiant le type de la Party à vérifier. Ce mot "type" est utilisé dans tous les commentaires ci-dessous.
            bool FeeTypePartySpecified = false;
            string FeeTypeParty = null;
            int IdParty = 0;
            bool IdRoleActor_PartySpecified = false;
            string IdRoleActor_Party = null;
            bool SidePartySpecified = false;
            string SideParty = null;
            bool IntentionSpecified = false;
            string Intention = null;

            try
            {
                #region switch (pTypeSide): Initialisation des variables à partir des valeurs en vigueur dans FEEMATRIX
                switch (pTypeSide)
                {
                    case TypeSidePartyEnum.PartyA:
                        FeeTypePartySpecified = opFeeMatrix.FEETYPEPARTYASpecified;
                        if (FeeTypePartySpecified)
                            FeeTypeParty = opFeeMatrix.FEETYPEPARTYA.Trim();

                        IdParty = opFeeMatrix.IDPARTYA;

                        IdRoleActor_PartySpecified = opFeeMatrix.IDROLEACTOR_PARTYASpecified;
                        if (IdRoleActor_PartySpecified)
                            IdRoleActor_Party = opFeeMatrix.IDROLEACTOR_PARTYA.Trim();

                        SidePartySpecified = opFeeMatrix.SIDEPARTYASpecified;
                        if (SidePartySpecified)
                            SideParty = opFeeMatrix.SIDEPARTYA.Trim();

                        IntentionSpecified = opFeeMatrix.INTENTIONPARTYASpecified;
                        if (IntentionSpecified)
                            Intention = opFeeMatrix.INTENTIONPARTYA.Trim();
                        break;

                    case TypeSidePartyEnum.PartyB:
                        FeeTypePartySpecified = opFeeMatrix.FEETYPEPARTYBSpecified;
                        if (FeeTypePartySpecified)
                            FeeTypeParty = opFeeMatrix.FEETYPEPARTYB.Trim();

                        IdParty = opFeeMatrix.IDPARTYB;

                        IdRoleActor_PartySpecified = opFeeMatrix.IDROLEACTOR_PARTYBSpecified;
                        if (IdRoleActor_PartySpecified)
                            IdRoleActor_Party = opFeeMatrix.IDROLEACTOR_PARTYB.Trim();

                        SidePartySpecified = opFeeMatrix.SIDEPARTYBSpecified;
                        if (SidePartySpecified)
                            SideParty = opFeeMatrix.SIDEPARTYB.Trim();

                        IntentionSpecified = opFeeMatrix.INTENTIONPARTYBSpecified;
                        if (IntentionSpecified)
                            Intention = opFeeMatrix.INTENTIONPARTYB.Trim();

                        break;

                    case TypeSidePartyEnum.OtherParty1:
                        FeeTypePartySpecified = opFeeMatrix.FEETYPEOTHERPARTY1Specified;
                        if (FeeTypePartySpecified)
                            FeeTypeParty = opFeeMatrix.FEETYPEOTHERPARTY1.Trim();

                        IdParty = opFeeMatrix.IDOTHERPARTY1;

                        IdRoleActor_PartySpecified = opFeeMatrix.IDROLEACTOR_OPART1Specified;
                        if (IdRoleActor_PartySpecified)
                            IdRoleActor_Party = opFeeMatrix.IDROLEACTOR_OPART1.Trim();
                        break;

                    case TypeSidePartyEnum.OtherParty2:
                        FeeTypePartySpecified = opFeeMatrix.FEETYPEOTHERPARTY2Specified;
                        if (FeeTypePartySpecified)
                            FeeTypeParty = opFeeMatrix.FEETYPEOTHERPARTY2.Trim();

                        IdParty = opFeeMatrix.IDOTHERPARTY2;

                        IdRoleActor_PartySpecified = opFeeMatrix.IDROLEACTOR_OPART2Specified;
                        if (IdRoleActor_PartySpecified)
                            IdRoleActor_Party = opFeeMatrix.IDROLEACTOR_OPART2.Trim();

                        break;
                }
                #endregion

                if (FeeTypePartySpecified)
                {
                    if (FeeTypeParty == TypePartyEnum.None.ToString())
                    {
                        #region NONE: FeeMatrix exige qu'il n'y ait aucun acteur. 
                        //Note: Ce paramétrage ne vaut que pour les OtherParty (ex. aucun OtherParty2), les PartyA et PartyB sont bien évidemment obligatoires sur un Trade.
                        if (pRequest_Party != null)
                        {
                            if (IdRoleActor_PartySpecified)
                            {
                                if (IdRoleActor_Party == pRequest_Party.m_Party_Role.ToString())
                                {
                                    //Note: FeeMatrix exige qu'il n'y ait aucun acteur avec un rôle précis (ex. aucun OtherParty2 de rôle BROKER) mais il existe sur le trade un acteur de ce type avec le rôle en question
                                    opFeeMatrix.Status = FeeMatrixs.StatusEnum.Unvalid;
                                    opFeeMatrix.CommentForDebug = "Exist Role " + pTypeSide;
                                }
                            }
                            else
                            {
                                //Note: FeeMatrix exige qu'il n'y ait aucun acteur (ex. aucun OtherParty2) mais il existe sur le trade un acteur de ce type
                                opFeeMatrix.Status = FeeMatrixs.StatusEnum.Unvalid;
                                opFeeMatrix.CommentForDebug = "Exist " + pTypeSide;
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        if (FeeTypeParty == TypePartyEnum.All.ToString())
                        {
                            #region ALL: FeeMatrix exige qu'il y ait un acteur
                            if (pRequest_Party == null)
                            {
                                //Note: FeeMatrix exige qu'il y ait un acteur (ex. présence d'un OtherParty2), quelqu'il soit, mais il n'existe sur le trade aucun acteur de ce type
                                opFeeMatrix.Status = FeeMatrixs.StatusEnum.Unvalid;
                                opFeeMatrix.CommentForDebug = "Not Exist" + pTypeSide;
                            }
                            #endregion
                        }
                        else if (IdParty > 0)
                        {
                            #region FeeMatrix exige qu'il y ait un acteur spécifique (ex. présence de MLBANK)
                            if (pRequest_Party == null)
                            {
                                //Note: FeeMatrix exige qu'il y ait un acteur spécifique mais il n'existe sur le trade aucun acteur de ce type
                                opFeeMatrix.Status = FeeMatrixs.StatusEnum.Unvalid;
                                opFeeMatrix.CommentForDebug = "Not Exist" + pTypeSide;
                            }
                            else if (!CheckActorOrBook(pCS, pDbTransaction, FeeTypeParty, IdParty, pRequest_Party))
                            {
                                #region TypePartyEnum.GrpActor, Actor, GrpBook, Book
                                //Note: FeeMatrix exige qu'il y ait un acteur spécifique, mais il n'existe pas sur le trade un tel acteur de ce type
                                opFeeMatrix.Status = FeeMatrixs.StatusEnum.Unvalid;
                                opFeeMatrix.CommentForDebug = "Incorrect " + pTypeSide;
                                #endregion
                            }
                            #endregion
                        }

                        #region Check Role
                        if (opFeeMatrix.Status == FeeMatrixs.StatusEnum.Unknown)
                        {
                            if (IdRoleActor_PartySpecified)
                            {
                                if (pRequest_Party == null)//PL 20120912 New case
                                {
                                    //Note: FeeMatrix exige un rôle spécifique (ex. présence d'un DESK) mais il n'existe sur le trade aucun acteur de ce type
                                    opFeeMatrix.Status = FeeMatrixs.StatusEnum.Unvalid;
                                    opFeeMatrix.CommentForDebug = "Not Exist" + pTypeSide;
                                }
                                else if (IdRoleActor_Party != pRequest_Party.m_Party_Role.ToString())
                                {
                                    bool isIncorrect = true;
                                    if (pRequest_Party.m_Party_Role == RoleActor.COUNTERPARTY)
                                    {
                                        //Cas particulier du rôle COUNTERPARTY
                                        //- Un acteur qui dispose sur un trade de ce rôle COUNTERPARTY, dispose souvent d'autres rôles (ex. CLIENT ou DESK ou CLEARINGHOUSE ou ...) 
                                        //- On recherche donc si l'acteur en question dispose d'un rôle identique à celui exigé par FeeMatrix
                                        RoleActor role = (RoleActor)Enum.Parse(typeof(RoleActor), IdRoleActor_Party);
                                        if (ActorTools.IsActorWithRole(pCS, pDbTransaction, pRequest_Party.m_Party_Ida, role))
                                            isIncorrect = false;
                                    }
                                    else if ((pRequest_Party.m_Party_Role == RoleActor.BROKER) && (IdRoleActor_Party == RoleActor.ENTITY.ToString()))
                                    {
                                        //Cas particulier du rôle BROKER (PL 20231219 Newness added for TRADITION)
                                        //- Un tel acteur dans le trade XML peut être une ENTITY 
                                        //- On recherche donc si l'acteur en question dispose du rôle ENTITY exigé par FeeMatrix
                                        if (ActorTools.IsActorWithRole(pCS, pDbTransaction, pRequest_Party.m_Party_Ida, RoleActor.ENTITY))
                                            isIncorrect = false;
                                    }

                                    if (isIncorrect)
                                    {
                                        //Note: FeeMatrix exige un rôle spécifique (ex. présence d'un DESK) mais il n'existe sur le trade aucun acteur de ce type avec ce rôle
                                        opFeeMatrix.Status = FeeMatrixs.StatusEnum.Unvalid;
                                        opFeeMatrix.CommentForDebug = "Incorrect Role " + pTypeSide;
                                    }
                                }
                            }
                        }
                        #endregion Check Role

                        #region Check Side
                        if (opFeeMatrix.Status == FeeMatrixs.StatusEnum.Unknown)
                        {
                            if (SidePartySpecified && (SideParty != TradeSideEnum.All.ToString()))
                            {
                                if (SideParty != ((Party)pRequest_Party).m_Party_Side.ToString())
                                {
                                    //Note: FeeMatrix exige un sens spécifique (ex. partyA doit être Buyer) mais il n'existe sur le trade aucun acteur de ce type avec ce sens
                                    opFeeMatrix.Status = FeeMatrixs.StatusEnum.Unvalid;
                                    opFeeMatrix.CommentForDebug = "Incorrect Side " + pTypeSide;
                                }
                            }
                        }
                        #endregion Check Side

                        #region Check Intention
                        if (opFeeMatrix.Status == FeeMatrixs.StatusEnum.Unknown)
                        {
                            if (IntentionSpecified)
                            {
                                bool isValid = true;
                                if (IntentionEnum.Initiator.ToString() == Intention)
                                    isValid = (_feeRequest.DataDocument.IsPartyInitiator(pRequest_Party.m_Party_PartyId));
                                else if (IntentionEnum.Reactor.ToString() == Intention)
                                    isValid = (_feeRequest.DataDocument.IsPartyReactor(pRequest_Party.m_Party_PartyId));

                                if (!isValid)
                                {
                                    //Note: FeeMatrix exige une intention spécifique (ex. partyA doit être Initiator) mais il n'existe sur le trade aucun acteur de ce type avec cette intention
                                    opFeeMatrix.Status = FeeMatrixs.StatusEnum.Unvalid;
                                    opFeeMatrix.CommentForDebug = "Incorrect Intention ";
                                }
                            }
                        }
                        #endregion
                    }
                }
            }
            catch
            {
                opFeeMatrix.Status = FeeMatrixs.StatusEnum.Error;
                opFeeMatrix.CommentForDebug = "Error" + pTypeSide;
            }
        }

        /// <summary>
        /// Return the value of an AssementBasis from a Product 
        /// </summary>
        /// <param name="pAssessmentBasis"></param>
        /// <param name="opReturnDecValue"></param>
        /// <param name="opErrorMessage"></param>
        /// <returns></returns>
        public bool GetAssessmentBasisValue(string pCS, IDbTransaction pDbTransaction,  string pAssessmentBasis, ref decimal opReturnDecValue, ref string opReturnStrValue, ref string opErrorMessage)
        {
            Cst.AssessmentBasisEnum assessmentBasis;
            int idDefineExtendDet = -1;
            bool isExtend;

            #region 1- parser pAssessmentBasis, pour savoir s'il sagit d'un Extend
            //
            try
            {
                //FL/PL 20120530
                //idDefineExtendDet = Convert.ToInt32(pAssessmentBasis);
                //isExtend = true;
                isExtend = Int32.TryParse(pAssessmentBasis, out idDefineExtendDet);
            }
            catch { isExtend = false; }
            bool ret;
            #endregion

            if (isExtend)
            {
                try
                {
                    #region Charger la valeur de l'Extend
                    opReturnDecValue = 0;
                    opReturnStrValue = null;
                    //
                    ITrade currentTrade = FeeRequest.DataDocument.CurrentTrade;
                    //
                    if (currentTrade.ExtendsSpecified && (null != currentTrade.Extends))
                    {
                        // 20090911 RD - Dans le cadre du chantier Sur l'enrichissement du flux XML pour les confirm ( transformation de l'attribut 'Id' en 'OTCmlId')
                        ISpheresIdScheme spheresIdScheme = currentTrade.Extends.GetSpheresIdFromScheme(idDefineExtendDet);
                        if (null != spheresIdScheme)
                        {
                            if (StrFunc.IsEmpty(spheresIdScheme.Value))
                                opReturnDecValue = decimal.Zero;
                            else
                                opReturnDecValue = DecFunc.DecValue(spheresIdScheme.Value);
                        }
                        else
                            throw new Exception("Specified assessment doesn't exist in the Trade Extends");
                    }
                    else
                        throw new Exception("Specified assessment is Extend, on the other hand, no Extends in the Trade");
                    //                
                    ret = true;
                    #endregion
                }
                catch (Exception ex)
                {
                    ret = false;
                    opErrorMessage += " Error on GetAssessmentBasisValue(" + pAssessmentBasis + "). " + ex.Message;
                }
                //
                return ret;
            }
            //
            try
            {
                #region 2- Sinon, parser pAssessmentBasis, pour savoir s'il sagit d'un Enum Cst.AssessmentBasisEnum
                assessmentBasis = (Cst.AssessmentBasisEnum)Enum.Parse(typeof(Cst.AssessmentBasisEnum), pAssessmentBasis);
                ret = GetAssessmentBasisValue(pCS, pDbTransaction, assessmentBasis, ref opReturnDecValue, ref opReturnStrValue, ref opErrorMessage);
                #endregion
            }
            catch
            {
                ret = false;
                opErrorMessage += " Error on GetAssessmentBasisValue(" + pAssessmentBasis + "). Incorrect Enum value";
            }
            //
            return ret;
        }

        /// <summary>
        /// Obtenir la valeur de l'assiette à partir des infos de FeeRequest
        /// <para>C'est pour gérer le cas du calcul des frais sur des actions (Exercice, Livraison de SSJ, Liquidation...). </para>
        /// <para>Ainsi le calcul des frais doit se baser sur:</para>
        /// <para>1- La quantité en position (FeeRequest.AssessmentBasis_DecValue) pour les assiettes QUANTITY et QUANTITYCONTRACTMULTIPLIER</para>
        /// <para>2- Le contract multiplier en vigueur pour l'assiette QUANTITYCONTRACTMULTIPLIER</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pAssessmentBasis"></param>
        /// <param name="opReturnDecValue"></param>
        /// <param name="opReturnStrValue"></param>
        /// <param name="opErrorMessage"></param>
        /// <returns></returns>
        // RD 20170308 [22921] Add
        public bool GetFeeRequestAssessmentBasisValue(string pCS, IDbTransaction pDbTransaction,   string pAssessmentBasis, ref decimal opReturnDecValue, ref string opReturnStrValue, ref string opErrorMessage)
        {
            string errorMessage = "Error on GetFeeRequestAssessmentBasisValue(" + pAssessmentBasis + ")";

            bool ret;
            try
            {
                Cst.AssessmentBasisEnum assessmentBasis = (Cst.AssessmentBasisEnum)Enum.Parse(typeof(Cst.AssessmentBasisEnum), pAssessmentBasis);

                #region isExchangeTradedDerivative
                if (this.FeeRequest.Product.IsExchangeTradedDerivative)
                {
                    switch (assessmentBasis)
                    {
                        #region QuantityContractMultiplier
                        case Cst.AssessmentBasisEnum.QuantityContractMultiplier:
                            IExchangeTradedDerivative exchangeTradedDerivative = this.FeeRequest.ExchangeTradedDerivative;
                            DateTime dtClearing = exchangeTradedDerivative.TradeCaptureReport.TradeDate.DateValue;

                            ExchangeTradedDerivativeContainer exchangeTradedDerivativeContainer =
                                new ExchangeTradedDerivativeContainer(pCS, pDbTransaction, exchangeTradedDerivative, this.FeeRequest.Product.DataDocument,
                                    SQL_Table.ScanDataDtEnabledEnum.Yes, dtClearing);

                            if (exchangeTradedDerivativeContainer.ContractMultiplier == decimal.Zero)
                            {
                                errorMessage += Cst.CrLf + "Missing Contract Multiplier on Derivative contract";
                            }
                            else
                            {
                                errorMessage = null;
                                opReturnDecValue = this.FeeRequest.AssessmentBasis_DecValue * exchangeTradedDerivativeContainer.ContractMultiplier;
                                opReturnStrValue = this.FeeRequest.AssessmentBasis_StrValue;
                            }
                            break;
                        #endregion
                        #region Others
                        default:
                            errorMessage = null;
                            opReturnDecValue = this.FeeRequest.AssessmentBasis_DecValue;
                            opReturnStrValue = this.FeeRequest.AssessmentBasis_StrValue;
                            break;
                            #endregion
                    }
                }
                #endregion
                #region Others
                else
                {
                    errorMessage = null;
                    opReturnDecValue = this.FeeRequest.AssessmentBasis_DecValue;
                    opReturnStrValue = this.FeeRequest.AssessmentBasis_StrValue;
                }
                #endregion

                ret = (StrFunc.IsEmpty(errorMessage));
            }
            catch (Exception ex)
            {
                ret = false;
                errorMessage += Cst.CrLf + ex.Message;
            }

            opErrorMessage += errorMessage;
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pAssessmentBasis"></param>
        /// <param name="opReturnDecValue"></param>
        /// <param name="opReturnStrValue"></param>
        /// <param name="opErrorMessage"></param>
        /// <returns></returns>
        //PM 20140507 [19970][19259] Utilisation de PriceQuotedCurrency au lieu de PriceCurrency
        //PM 20140509 [19970][19259] Utilisation de ContractMultiplierQuotedCurrency au lieu de ContractMultiplier
        //FI 20140909 [20340] Modify
        //FI 20141013 [20418] Modify
        // EG 20150708 [21103] Gestion Cst.AssessmentBasisEnum.MarketValue|Cst.AssessmentBasisEnum.Quantity sur SafekeepingAction (EquitySecurityTransaction|DebtSecurityTransaction)
        /// EG 20170510 [23153] Upd
        // EG 20171128 Add Test on exchangeTradedDerivativeContainer.derivativeContract
        // EG 20180205 [23769] Del EFS_TradeLibray use DataDocument
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20190716 [VCL : New FixedIncome] Upd Calcul MKV sur la base des datas dans EVENTDET
        // EG 20190926 [VCL : Equity] Toujours recalcul du MKV du jour sur la base de la qty en position DTSETTLEMENT
        public bool GetAssessmentBasisValue(string pCS, IDbTransaction pDbTransaction, Cst.AssessmentBasisEnum pAssessmentBasis, ref decimal opReturnDecValue, ref string opReturnStrValue, ref string opErrorMessage)
        {
            opReturnDecValue = 0;
            opReturnStrValue = null;
            string errorMessage = "Error on GetAssessmentBasisValue(" + pAssessmentBasis + ")";
            bool ret;
            try
            {
                #region Trade, Line
                // EG 20160404 Migration vs2013
                //PL 20100122 Remove Trade & Line
                //if ((pAssessmentBasis == Cst.AssessmentBasisEnum.Trade) || (pAssessmentBasis == Cst.AssessmentBasisEnum.Line))
                //if (false)
                //{
                //    errorMessage = null;
                //    opReturnDecValue = 1;
                //}
                #endregion Trade, Line
                #region PeriodInDay, PeriodInWeek, PeriodInMonth, PeriodInYear
                //else if (
                // EG 20160404 Migration vs2013
                if ((pAssessmentBasis == Cst.AssessmentBasisEnum.PeriodInDay) ||
                     (pAssessmentBasis == Cst.AssessmentBasisEnum.PeriodInWeek) ||
                     (pAssessmentBasis == Cst.AssessmentBasisEnum.PeriodInMonth) ||
                     (pAssessmentBasis == Cst.AssessmentBasisEnum.PeriodInYear))
                {
                    DateTime dt1, dt2;
                    dt1 = DateTime.MinValue;
                    dt2 = DateTime.MinValue;
                    //FI 20091223 [16471] Appel à DataDocument.GetStartAndEndDates
                    FeeRequest.DataDocument.GetStartAndEndDates(pCS, false, out dt1, out dt2);
                    //
                    if (DtFunc.IsDateTimeEmpty(dt1) || DtFunc.IsDateTimeEmpty(dt2))
                        errorMessage += " - Date is missing";
                    else
                    {
                        TimeSpan difference = dt2 - dt1;
                        errorMessage = null;
                        opReturnDecValue = ConvertPeriod(pAssessmentBasis, difference.TotalDays);
                    }
                }
                #endregion PeriodInDay, PeriodInWeek, PeriodInMonth, PeriodInYear
                else
                {
                    //TODO FEEMATRIX (autres products...)
                    #region IsSwap
                    if (this.FeeRequest.Product.IsSwap)
                    {
                        ISwap swap = (ISwap)this.FeeRequest.Product.Product;
                        InterestRateStreamsContainer streams = new InterestRateStreamsContainer(swap.Stream);
                        //
                        switch (pAssessmentBasis)
                        {
                            #region InitialNotionalAmount & CurrentNotionalAmount
                            //CurrencyReferenceAmount: Le notional de référence se trouve sur le stream qui ne dispose pas de "fxLinkedNotionalSchedule"
                            //                         On balaye donc "classiquement" les streams avec un "notional"
                            case Cst.AssessmentBasisEnum.InitialNotionalAmount:
                            case Cst.AssessmentBasisEnum.CurrentNotionalAmount:     //glop TODO
                            case Cst.AssessmentBasisEnum.CurrencyReferenceAmount:   //Voir commentaire ci-dessus
                            case Cst.AssessmentBasisEnum.Currency1Amount:           //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency2Amount:           //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency1AmountSpotLeg:    //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency2AmountSpotLeg:    //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency1AmountForwardLeg: //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency2AmountForwardLeg: //On considère "InitialNotionalAmount"
                                IMoney money = FeeRequest.DataDocument.GetMainCurrencyAmount(pCS, pDbTransaction);
                                if (null != money)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = money.Amount.DecValue;
                                    opReturnStrValue = money.GetCurrency.Value;
                                }
                                break;
                            #endregion
                            #region FixedRate
                            case Cst.AssessmentBasisEnum.FixedRate:
                            case Cst.AssessmentBasisEnum.FaceRate:                  //On considère "FixedRate"
                            case Cst.AssessmentBasisEnum.TradingRate:               //On considère "FixedRate"
                            case Cst.AssessmentBasisEnum.ForwardRate:
                                ISchedule fixedRate = null;
                                fixedRate = streams.GetFirstFixedRate(FeeRequest.DataDocument.DataDocument);
                                if (null != fixedRate)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = fixedRate.InitialValue.DecValue;
                                }
                                break;
                            #endregion
                            #region NumberOfPeriodInYear
                            case Cst.AssessmentBasisEnum.NumberOfPeriodInYear:
                                IInterestRateStream firstStream1 = streams.GetFirstStreamWithNotional(FeeRequest.DataDocument.DataDocument);
                                if (null != firstStream1)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = NumberOfPeriodInYear(firstStream1.PaymentDates.PaymentFrequency, ref errorMessage);
                                }
                                break;
                                #endregion
                        }
                    }
                    #endregion
                    #region IsCapFloor
                    else if (this.FeeRequest.Product.IsCapFloor)
                    {
                        ICapFloor capFloor = (ICapFloor)FeeRequest.Product.Product;
                        //
                        switch (pAssessmentBasis)
                        {
                            #region InitialNotionalAmount & CurrentNotionalAmount
                            case Cst.AssessmentBasisEnum.InitialNotionalAmount:
                            case Cst.AssessmentBasisEnum.CurrentNotionalAmount:     //glop TODO
                            case Cst.AssessmentBasisEnum.CurrencyReferenceAmount:   //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency1Amount:           //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency2Amount:           //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency1AmountSpotLeg:    //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency2AmountSpotLeg:    //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency1AmountForwardLeg: //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency2AmountForwardLeg: //On considère "InitialNotionalAmount"
                                IMoney money = FeeRequest.DataDocument.GetMainCurrencyAmount(pCS, pDbTransaction);
                                if (null != money)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = money.Amount.DecValue;
                                    opReturnStrValue = money.GetCurrency.Value;
                                }
                                break;
                            #endregion
                            #region Strike
                            case Cst.AssessmentBasisEnum.Strike:                    //On considère "FixedRate"
                            case Cst.AssessmentBasisEnum.FixedRate:                 //On considère "FixedRate"
                            case Cst.AssessmentBasisEnum.FaceRate:                  //On considère "FixedRate"
                            case Cst.AssessmentBasisEnum.TradingRate:               //On considère "FixedRate"
                            case Cst.AssessmentBasisEnum.ForwardRate:
                                if (capFloor.Stream.IsCapped)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = capFloor.Stream.CalculationPeriodAmount.Calculation.RateFloatingRate.CapRateSchedule[0].InitialValue.DecValue;
                                }
                                else if (capFloor.Stream.IsFloored)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = capFloor.Stream.CalculationPeriodAmount.Calculation.RateFloatingRate.FloorRateSchedule[0].InitialValue.DecValue;
                                }
                                break;
                            #endregion
                            #region Premium
                            case Cst.AssessmentBasisEnum.Premium:
                                if (capFloor.PremiumSpecified)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = capFloor.Premium[0].PaymentAmount.Amount.DecValue;
                                    opReturnStrValue = capFloor.Premium[0].PaymentAmount.Currency;
                                }
                                break;
                            #endregion
                            #region PremiumRate
                            case Cst.AssessmentBasisEnum.PremiumRate:
                                if (capFloor.PremiumSpecified && capFloor.Premium[0].PaymentQuoteSpecified)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = capFloor.Premium[0].PaymentQuote.PercentageRate.DecValue;
                                }
                                break;
                            #endregion
                            #region NumberOfPeriodInYear
                            case Cst.AssessmentBasisEnum.NumberOfPeriodInYear:
                                errorMessage = null;
                                opReturnDecValue = NumberOfPeriodInYear(capFloor.Stream.PaymentDates.PaymentFrequency, ref errorMessage);
                                break;
                                #endregion
                        }
                    }
                    #endregion
                    #region IsFra
                    else if (FeeRequest.Product.IsFra)
                    {
                        IFra fra = (IFra)this.FeeRequest.Product.Product;
                        //
                        switch (pAssessmentBasis)
                        {
                            #region InitialNotionalAmount & CurrentNotionalAmount
                            case Cst.AssessmentBasisEnum.InitialNotionalAmount:
                            case Cst.AssessmentBasisEnum.CurrentNotionalAmount:     //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.CurrencyReferenceAmount:   //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency1Amount:           //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency2Amount:           //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency1AmountSpotLeg:    //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency2AmountSpotLeg:    //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency1AmountForwardLeg: //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency2AmountForwardLeg: //On considère "InitialNotionalAmount"
                                IMoney money = FeeRequest.DataDocument.GetMainCurrencyAmount(pCS, pDbTransaction);
                                if (null != money)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = money.Amount.DecValue;
                                    opReturnStrValue = money.GetCurrency.Value;
                                }
                                break;
                            #endregion
                            #region FixedRate
                            case Cst.AssessmentBasisEnum.FixedRate:
                            case Cst.AssessmentBasisEnum.FaceRate:                  //On considère "FixedRate"
                            case Cst.AssessmentBasisEnum.TradingRate:               //On considère "FixedRate"
                            case Cst.AssessmentBasisEnum.Strike:                    //On considère "FixedRate"
                            case Cst.AssessmentBasisEnum.ForwardRate:
                                errorMessage = null;
                                opReturnDecValue = fra.FixedRate.DecValue;
                                break;
                            #endregion
                            #region NumberOfPeriodInYear
                            case Cst.AssessmentBasisEnum.NumberOfPeriodInYear:
                                errorMessage = null;
                                opReturnDecValue = (decimal)1;
                                break;
                                #endregion
                        }
                    }
                    #endregion
                    #region IsBulletPayment
                    else if (FeeRequest.Product.IsBulletPayment)
                    {
                        IBulletPayment bulletPayment = (IBulletPayment)this.FeeRequest.Product.Product;
                        //
                        switch (pAssessmentBasis)
                        {
                            #region InitialNotionalAmount & CurrentNotionalAmount
                            case Cst.AssessmentBasisEnum.InitialNotionalAmount:
                            case Cst.AssessmentBasisEnum.CurrentNotionalAmount:     //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.CurrencyReferenceAmount:   //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency1Amount:           //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency2Amount:           //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency1AmountSpotLeg:    //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency2AmountSpotLeg:    //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency1AmountForwardLeg: //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency2AmountForwardLeg: //On considère "InitialNotionalAmount"
                                IMoney money = FeeRequest.DataDocument.GetMainCurrencyAmount(pCS, pDbTransaction);
                                if (null != money)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = money.Amount.DecValue;
                                    opReturnStrValue = money.GetCurrency.Value;
                                }
                                break;
                                #endregion
                        }
                    }
                    #endregion
                    #region IsLoanDeposit
                    else if (FeeRequest.Product.IsLoanDeposit)
                    {
                        ILoanDeposit loanDeposit = (ILoanDeposit)this.FeeRequest.Product.Product;
                        InterestRateStreamsContainer streams = new InterestRateStreamsContainer(loanDeposit.Stream);
                        //
                        switch (pAssessmentBasis)
                        {
                            #region InitialNotionalAmount & CurrentNotionalAmount
                            case Cst.AssessmentBasisEnum.InitialNotionalAmount:
                            case Cst.AssessmentBasisEnum.CurrentNotionalAmount:     //glop TODO
                            case Cst.AssessmentBasisEnum.CurrencyReferenceAmount:   //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency1Amount:           //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency2Amount:           //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency1AmountSpotLeg:    //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency2AmountSpotLeg:    //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency1AmountForwardLeg: //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency2AmountForwardLeg: //On considère "InitialNotionalAmount"
                                IMoney money = FeeRequest.DataDocument.GetMainCurrencyAmount(pCS, pDbTransaction);
                                if (null != money)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = money.Amount.DecValue;
                                    opReturnStrValue = money.GetCurrency.Value;
                                }
                                break;
                            #endregion
                            #region FixedRate
                            case Cst.AssessmentBasisEnum.FixedRate:
                            case Cst.AssessmentBasisEnum.FaceRate:                  //On considère "FixedRate"
                            case Cst.AssessmentBasisEnum.TradingRate:               //On considère "FixedRate"
                            case Cst.AssessmentBasisEnum.ForwardRate:
                                ISchedule fixedRate = null;
                                fixedRate = streams.GetFirstFixedRate(this.FeeRequest.DataDocument.DataDocument);
                                if (null != fixedRate)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = fixedRate.InitialValue.DecValue;
                                }
                                break;
                            #endregion
                            #region NumberOfPeriodInYear
                            case Cst.AssessmentBasisEnum.NumberOfPeriodInYear:
                                IInterestRateStream firstStream2 = streams.GetFirstStreamWithNotional(this.FeeRequest.DataDocument.DataDocument);
                                if (null != firstStream2)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = NumberOfPeriodInYear(firstStream2.PaymentDates.PaymentFrequency, ref errorMessage);
                                }
                                break;
                                #endregion
                        }
                    }
                    #endregion
                    #region IsSwaption
                    else if (FeeRequest.Product.IsSwaption)
                    {
                        ISwaption swaption = (ISwaption)FeeRequest.Product.Product;
                        InterestRateStreamsContainer streams = new InterestRateStreamsContainer(swaption.Swap.Stream);
                        //
                        switch (pAssessmentBasis)
                        {
                            #region InitialNotionalAmount & CurrentNotionalAmount
                            //CurrencyReferenceAmount: Le notional de référence se trouve sur le stream qui ne dispose pas de "fxLinkedNotionalSchedule"
                            //                         On balaye donc "classiquement" les streams avec un "notional"
                            case Cst.AssessmentBasisEnum.InitialNotionalAmount:
                            case Cst.AssessmentBasisEnum.CurrentNotionalAmount:     //glop TODO
                            case Cst.AssessmentBasisEnum.CurrencyReferenceAmount:   //Voir commentaire ci-dessus
                            case Cst.AssessmentBasisEnum.Currency1Amount:           //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency2Amount:           //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency1AmountSpotLeg:    //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency2AmountSpotLeg:    //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency1AmountForwardLeg: //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency2AmountForwardLeg: //On considère "InitialNotionalAmount"
                                IMoney money = FeeRequest.DataDocument.GetMainCurrencyAmount(pCS, pDbTransaction);
                                if (null != money)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = money.Amount.DecValue;
                                    opReturnStrValue = money.GetCurrency.Value;
                                }
                                break;
                            #endregion
                            #region Strike
                            case Cst.AssessmentBasisEnum.FixedRate:
                            case Cst.AssessmentBasisEnum.Strike:                    //On considère "FixedRate"
                            case Cst.AssessmentBasisEnum.FaceRate:                  //On considère "FixedRate"
                            case Cst.AssessmentBasisEnum.TradingRate:               //On considère "FixedRate"
                                ISchedule fixedRate = null;
                                fixedRate = streams.GetFirstFixedRate(FeeRequest.DataDocument.DataDocument);
                                if (null != fixedRate)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = fixedRate.InitialValue.DecValue;
                                }
                                break;
                            #endregion
                            #region Premium
                            case Cst.AssessmentBasisEnum.Premium:
                                if (swaption.PremiumSpecified)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = swaption.Premium[0].PaymentAmount.Amount.DecValue;
                                    opReturnStrValue = swaption.Premium[0].PaymentAmount.Currency;
                                }
                                break;
                            #endregion
                            #region PremiumRate
                            case Cst.AssessmentBasisEnum.PremiumRate:
                                if (swaption.PremiumSpecified && swaption.Premium[0].PaymentQuoteSpecified)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = swaption.Premium[0].PaymentQuote.PercentageRate.DecValue;
                                }
                                break;
                                #endregion
                        }
                    }
                    #endregion IsSwaption
                    #region IsFxLeg
                    else if (FeeRequest.Product.IsFxLeg)
                    {
                        IFxLeg fxLeg = (IFxLeg)this.FeeRequest.Product.Product;
                        //
                        switch (pAssessmentBasis)
                        {
                            #region Currency1Amount
                            case Cst.AssessmentBasisEnum.Currency1Amount:
                            case Cst.AssessmentBasisEnum.Currency1AmountSpotLeg:
                            case Cst.AssessmentBasisEnum.InitialNotionalAmount:     //On considère "Currency1Amount"
                            case Cst.AssessmentBasisEnum.CurrentNotionalAmount:     //On considère "Currency1Amount"
                            case Cst.AssessmentBasisEnum.CurrencyReferenceAmount:   //On considère "Currency1Amount"
                            case Cst.AssessmentBasisEnum.Currency1AmountForwardLeg: //On considère "Currency1Amount"
                                IMoney money = FeeRequest.DataDocument.GetMainCurrencyAmount(pCS, pDbTransaction);
                                if (null != money)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = money.Amount.DecValue;
                                    opReturnStrValue = money.GetCurrency.Value;
                                }
                                break;
                            #endregion
                            #region Currency2Amount
                            case Cst.AssessmentBasisEnum.Currency2Amount:
                            case Cst.AssessmentBasisEnum.Currency2AmountSpotLeg:
                            case Cst.AssessmentBasisEnum.Currency2AmountForwardLeg: //On considère "Currency2Amount"
                                errorMessage = null;
                                opReturnDecValue = fxLeg.ExchangedCurrency2.PaymentAmount.Amount.DecValue;
                                opReturnStrValue = fxLeg.ExchangedCurrency2.PaymentAmount.Currency;
                                break;
                            #endregion
                            #region ForwardRate
                            case Cst.AssessmentBasisEnum.ForwardRate:
                            case Cst.AssessmentBasisEnum.TradingRate:
                            case Cst.AssessmentBasisEnum.FixedRate:                 //On considère "TradingRate"
                            case Cst.AssessmentBasisEnum.FaceRate:                  //On considère "TradingRate"
                            case Cst.AssessmentBasisEnum.Strike:                    //On considère "TradingRate"
                                errorMessage = null;
                                opReturnDecValue = fxLeg.ExchangeRate.Rate.DecValue;
                                break;
                            #endregion
                            #region SpotRate
                            case Cst.AssessmentBasisEnum.SpotRate:
                                Nullable<decimal> rate;
                                if (fxLeg.ExchangeRate.SpotRateSpecified)
                                {
                                    rate = fxLeg.ExchangeRate.SpotRate.DecValue;
                                }
                                else
                                {
                                    //Sur un FxSpot: SpotRate == TradingRate
                                    rate = fxLeg.ExchangeRate.Rate.DecValue;
                                }
                                if (rate != null)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = (decimal)rate;
                                }
                                break;
                                #endregion
                        }
                    }
                    #endregion IsFxLeg
                    #region IsFxSwap
                    else if (FeeRequest.Product.IsFxSwap)
                    {
                        FxSwapContainer fxSwap = new FxSwapContainer((IFxSwap)this.FeeRequest.Product.Product);
                        //
                        #region Recherche du Leg
                        IFxLeg firstLeg = fxSwap.GetFirstLeg();
                        IFxLeg lastLeg = fxSwap.GetLastLeg();
                        #endregion Recherche du Leg

                        switch (pAssessmentBasis)
                        {
                            #region Currency1Amount & Currency2Amount
                            case Cst.AssessmentBasisEnum.Currency1Amount:
                            case Cst.AssessmentBasisEnum.Currency1AmountSpotLeg:
                            case Cst.AssessmentBasisEnum.InitialNotionalAmount:     //On considère "Currency1Amount"
                            case Cst.AssessmentBasisEnum.CurrentNotionalAmount:     //On considère "Currency1Amount"
                            case Cst.AssessmentBasisEnum.CurrencyReferenceAmount:   //On considère "Currency1Amount"
                                errorMessage = null;
                                opReturnDecValue = firstLeg.ExchangedCurrency1.PaymentAmount.Amount.DecValue;
                                opReturnStrValue = firstLeg.ExchangedCurrency1.PaymentAmount.Currency;
                                break;
                            case Cst.AssessmentBasisEnum.Currency1AmountForwardLeg:
                                errorMessage = null;
                                opReturnDecValue = lastLeg.ExchangedCurrency1.PaymentAmount.Amount.DecValue;
                                opReturnStrValue = lastLeg.ExchangedCurrency1.PaymentAmount.Currency;
                                break;
                            case Cst.AssessmentBasisEnum.Currency2AmountSpotLeg:
                                errorMessage = null;
                                opReturnDecValue = firstLeg.ExchangedCurrency2.PaymentAmount.Amount.DecValue;
                                opReturnStrValue = firstLeg.ExchangedCurrency2.PaymentAmount.Currency;
                                break;
                            case Cst.AssessmentBasisEnum.Currency2Amount:
                            case Cst.AssessmentBasisEnum.Currency2AmountForwardLeg:
                                errorMessage = null;
                                opReturnDecValue = lastLeg.ExchangedCurrency2.PaymentAmount.Amount.DecValue;
                                opReturnStrValue = lastLeg.ExchangedCurrency2.PaymentAmount.Currency;
                                break;
                            #endregion
                            #region ForwardRate
                            case Cst.AssessmentBasisEnum.ForwardRate:
                            case Cst.AssessmentBasisEnum.TradingRate:
                            case Cst.AssessmentBasisEnum.FixedRate:                 //On considère "TradingRate"
                            case Cst.AssessmentBasisEnum.FaceRate:                  //On considère "TradingRate"
                            case Cst.AssessmentBasisEnum.Strike:                    //On considère "TradingRate"
                                errorMessage = null;
                                opReturnDecValue = lastLeg.ExchangeRate.Rate.DecValue;
                                break;
                            #endregion
                            #region SpotRate
                            case Cst.AssessmentBasisEnum.SpotRate:
                                Nullable<decimal> rate;
                                if (firstLeg.ExchangeRate.SpotRateSpecified)
                                {
                                    rate = firstLeg.ExchangeRate.SpotRate.DecValue;
                                }
                                else
                                {
                                    //Sur un FxSpot: SpotRate == TradingRate
                                    rate = firstLeg.ExchangeRate.Rate.DecValue;
                                }
                                if (rate != null)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = (decimal)rate;
                                }
                                break;
                                #endregion
                        }
                    }
                    #endregion IsFxSwap
                    #region IsFxTermDeposit
                    else if (FeeRequest.Product.IsFxTermDeposit)
                    {
                        ITermDeposit fxTermDeposit = (ITermDeposit)this.FeeRequest.Product.Product;
                        switch (pAssessmentBasis)
                        {
                            #region InitialNotionalAmount & CurrentNotionalAmount
                            case Cst.AssessmentBasisEnum.InitialNotionalAmount:
                            case Cst.AssessmentBasisEnum.CurrentNotionalAmount:     //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.CurrencyReferenceAmount:   //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency1Amount:           //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency2Amount:           //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency1AmountSpotLeg:    //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency2AmountSpotLeg:    //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency1AmountForwardLeg: //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency2AmountForwardLeg: //On considère "InitialNotionalAmount"
                                IMoney money = FeeRequest.DataDocument.GetMainCurrencyAmount(pCS, pDbTransaction);
                                if (null != money)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = money.Amount.DecValue;
                                    opReturnStrValue = money.GetCurrency.Value;
                                }
                                break;
                            #endregion
                            #region FixedRate
                            case Cst.AssessmentBasisEnum.FixedRate:
                            case Cst.AssessmentBasisEnum.ForwardRate:
                            case Cst.AssessmentBasisEnum.FaceRate:                  //On considère "FixedRate"
                            case Cst.AssessmentBasisEnum.TradingRate:               //On considère "FixedRate"
                            case Cst.AssessmentBasisEnum.Strike:                    //On considère "FixedRate"
                                // FI 20140909 [20340] fixedRate est une donnée obligatoire (voir FpML)     
                                //if (fxTermDeposit.fixedRateSpecified)
                                //{
                                errorMessage = null;
                                opReturnDecValue = fxTermDeposit.FixedRate.DecValue;
                                //}
                                break;
                            #endregion
                            #region NumberOfPeriodInYear
                            case Cst.AssessmentBasisEnum.NumberOfPeriodInYear:
                                errorMessage = null;
                                opReturnDecValue = (decimal)1;
                                break;
                                #endregion
                        }
                    }
                    #endregion IsFxTermDeposit
                    #region IsEquityOption
                    //FI 20080912 Add Equity
                    else if (FeeRequest.Product.IsEquityOption)
                    {
                        EFS_EquityOption equityOption = new EFS_EquityOption(pCS, (IEquityOption)this.FeeRequest.Product.Product, FeeRequest.DataDocument);

                        switch (pAssessmentBasis)
                        {
                            #region InitialNotionalAmount & CurrentNotionalAmount
                            case Cst.AssessmentBasisEnum.InitialNotionalAmount:
                            case Cst.AssessmentBasisEnum.CurrentNotionalAmount:
                            case Cst.AssessmentBasisEnum.CurrencyReferenceAmount:
                            case Cst.AssessmentBasisEnum.Currency1Amount:
                            case Cst.AssessmentBasisEnum.Currency2Amount:
                            case Cst.AssessmentBasisEnum.Currency1AmountSpotLeg:
                            case Cst.AssessmentBasisEnum.Currency2AmountSpotLeg:
                            case Cst.AssessmentBasisEnum.Currency1AmountForwardLeg:
                            case Cst.AssessmentBasisEnum.Currency2AmountForwardLeg:
                                IMoney money = FeeRequest.DataDocument.GetMainCurrencyAmount(pCS, pDbTransaction);
                                if (null != money)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = money.Amount.DecValue;
                                    opReturnStrValue = money.GetCurrency.Value;
                                }
                                break;
                            #endregion

                            #region Strike
                            case Cst.AssessmentBasisEnum.FixedRate:
                            case Cst.AssessmentBasisEnum.ForwardRate:
                            case Cst.AssessmentBasisEnum.FaceRate:
                            case Cst.AssessmentBasisEnum.TradingRate:
                            case Cst.AssessmentBasisEnum.Strike:
                                if (equityOption.strike.strikePriceSpecified)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = equityOption.strike.strikePrice.DecValue;
                                }
                                else { }//Nickel
                                break;
                            #endregion

                            #region Quantity
                            case Cst.AssessmentBasisEnum.Quantity:
                            case Cst.AssessmentBasisEnum.QuantityContractMultiplier:
                                errorMessage = null;
                                //20100316 PL 
                                //opReturnDecValue = equityOption.UnitValueTotalUnderlyer.DecValue;
                                opReturnDecValue = equityOption.NumberOptions.DecValue;
                                break;
                                #endregion

                        }
                    }
                    #endregion IsEquityOption
                    #region IsReturnSwap
                    //PL 20140710 Add ReturnSwap
                    else if (FeeRequest.Product.IsReturnSwap)
                    {
                        // EG 20170510 [23153] Upd
                        //ReturnSwapContainer returnSwapContainer = new ReturnSwapContainer(pCS, (IReturnSwap)this.FeeRequest.Product.product, FeeRequest.DataDocument);
                        ReturnSwapContainer returnSwapContainer = new ReturnSwapContainer((IReturnSwap)this.FeeRequest.Product.Product, FeeRequest.DataDocument);

                        switch (pAssessmentBasis)
                        {
                            #region CurrentNotionalAmount, InitialNotionalAmount, Currency1Amount
                            case Cst.AssessmentBasisEnum.CurrentNotionalAmount:
                            case Cst.AssessmentBasisEnum.InitialNotionalAmount:
                            case Cst.AssessmentBasisEnum.Currency1Amount:
                                IMoney money = FeeRequest.DataDocument.CurrentProduct.GetMainCurrencyAmount(pCS, pDbTransaction);
                                if (null != money)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = money.Amount.DecValue;
                                    opReturnStrValue = money.GetCurrency.Value;
                                }
                                break;
                            #endregion
                            #region Quantity
                            case Cst.AssessmentBasisEnum.Quantity:
                                errorMessage = null;
                                if (returnSwapContainer.MainOpenUnits.HasValue)
                                    opReturnDecValue = returnSwapContainer.MainOpenUnits.Value;
                                break;
                            #region TradingPrice,TradingRate
                            // FI 20141013 [20418] case TradingPrice et TradingRate
                            case Cst.AssessmentBasisEnum.TradingPrice:
                            case Cst.AssessmentBasisEnum.TradingRate:
                                errorMessage = null;
                                Pair<IReturnLeg, IReturnLegMainUnderlyer> mainReturnLeg = returnSwapContainer.MainReturnLeg;
                                IReturnLegValuationPrice initialPrice = mainReturnLeg.First.RateOfReturn.InitialPrice;
                                if (initialPrice.NetPriceSpecified)
                                    opReturnDecValue = initialPrice.NetPrice.Amount.DecValue;
                                break;
                                #endregion
                                #endregion
                        }
                    }
                    #endregion IsReturnSwap
                    #region IsDebtSecurityTransaction
                    else if (FeeRequest.Product.IsDebtSecurityTransaction)
                    {
                        IDebtSecurityTransaction debtSecurityTransaction = (IDebtSecurityTransaction)this.FeeRequest.Product.Product;
                        DebtSecurityTransactionContainer dstContainer = new DebtSecurityTransactionContainer(debtSecurityTransaction, this.FeeRequest.DataDocument);
                        InterestRateStreamsContainer streams = null;
                        if (debtSecurityTransaction.SecurityAssetSpecified && debtSecurityTransaction.SecurityAsset.DebtSecuritySpecified)
                            streams = new InterestRateStreamsContainer(debtSecurityTransaction.DebtSecurity.Stream);

                        switch (pAssessmentBasis)
                        {
                            #region InitialNotionalAmount & CurrentNotionalAmount
                            case Cst.AssessmentBasisEnum.SecurityNotionalAmount:
                            case Cst.AssessmentBasisEnum.InitialNotionalAmount:
                            case Cst.AssessmentBasisEnum.CurrentNotionalAmount:     //glop TODO
                            case Cst.AssessmentBasisEnum.CurrencyReferenceAmount:   //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency1Amount:           //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency2Amount:           //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency1AmountSpotLeg:
                            case Cst.AssessmentBasisEnum.Currency2AmountSpotLeg:
                            case Cst.AssessmentBasisEnum.Currency1AmountForwardLeg:
                            case Cst.AssessmentBasisEnum.Currency2AmountForwardLeg:
                                IMoney money = FeeRequest.DataDocument.GetMainCurrencyAmount(pCS, pDbTransaction);
                                if (null != money)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = money.Amount.DecValue;
                                    opReturnStrValue = money.GetCurrency.Value;
                                }
                                break;
                            #endregion

                            #region SecurityGrossAmount
                            case Cst.AssessmentBasisEnum.SecurityGrossAmount:
                                errorMessage = null;
                                IPayment grossAmount = debtSecurityTransaction.GrossAmount;
                                opReturnDecValue = grossAmount.PaymentAmount.Amount.DecValue;
                                opReturnStrValue = grossAmount.PaymentAmount.Currency;
                                break;
                            #endregion

                            #region SecurityCleanAmount (FI 20190918)
                            case Cst.AssessmentBasisEnum.SecurityCleanAmount:
                                errorMessage = null;

                                IMoney securityCleanAmount = dstContainer.ProductBase.CreateMoney(0, string.Empty);
                                IMoney principalAmount = dstContainer.CalcPrincipalAmount();
                                if (null != principalAmount)
                                    securityCleanAmount = principalAmount;

                                opReturnDecValue = securityCleanAmount.Amount.DecValue;
                                opReturnStrValue = securityCleanAmount.Currency;

                                break;
                            #endregion

                            #region FixedRate
                            case Cst.AssessmentBasisEnum.FixedRate:
                            case Cst.AssessmentBasisEnum.FaceRate:                  //On considère "FixedRate"
                            case Cst.AssessmentBasisEnum.TradingRate:               //On considère "FixedRate"
                            case Cst.AssessmentBasisEnum.ForwardRate:
                                errorMessage = null;
                                if (debtSecurityTransaction.Price.CleanPriceSpecified)
                                    opReturnDecValue = debtSecurityTransaction.Price.CleanPrice.DecValue;
                                else if (debtSecurityTransaction.Price.DirtyPriceSpecified)
                                    opReturnDecValue = debtSecurityTransaction.Price.DirtyPrice.DecValue;
                                break;
                            #endregion

                            #region NumberOfPeriodInYear
                            case Cst.AssessmentBasisEnum.NumberOfPeriodInYear:
                                //
                                IInterestRateStream firstStream = null;
                                if (null != streams)
                                    firstStream = streams.GetFirstStreamWithNotional(FeeRequest.DataDocument.DataDocument);
                                //
                                if (null != firstStream)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = NumberOfPeriodInYear(firstStream.PaymentDates.PaymentFrequency, ref errorMessage);
                                }
                                break;
                            #endregion

                            #region Quantity
                            case Cst.AssessmentBasisEnum.Quantity:
                            case Cst.AssessmentBasisEnum.QuantityContractMultiplier:
                                // EG 20150708 [21103] Si gestion des frais de garde alors on va lire les données dans _feeRequest.SafekeepingAction
                                if (this._feeRequest.IsSafekeeping)
                                {
                                    errorMessage = null;
                                    // EG 20150920 [21374] Int (int32) to Long (Int64) 
                                    // EG 20170127 Qty Long To Decimal
                                    opReturnDecValue = _feeRequest.SafekeepingAction.quantity.DecValue;
                                }
                                else if (debtSecurityTransaction.Quantity.NumberOfUnitsSpecified)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = debtSecurityTransaction.Quantity.NumberOfUnits.DecValue;
                                }
                                break;
                            #endregion

                            #region MarketValue (EG 20150708 [21103] New Nouvelle assiette MarketValue)
                            case Cst.AssessmentBasisEnum.MarketValue:
                                errorMessage = null;
                                if (_feeRequest.IsSafekeeping)
                                {
                                    decimal qty = _feeRequest.SafekeepingAction.quantity.DecValue;
                                    if (_feeRequest.SafekeepingAction.marketValueDataSpecified)
                                    {
                                        MarketValueData data = _feeRequest.SafekeepingAction.marketValueData;
                                        IMoney marketValuePrincipalAmount = dstContainer.CalcPrincipalAmount(pCS, data.unitNotional.Value, data.currency, qty, data.cleanPrice.Value);
                                        IMoney accruedInterestAmount = dstContainer.CalcAccruedInterestAmount(pCS, data.unitNotional.Value, data.currency, qty, data.accruedRate.Value);
                                        opReturnDecValue = marketValuePrincipalAmount.Amount.DecValue + accruedInterestAmount.Amount.DecValue;
                                        opReturnStrValue = marketValuePrincipalAmount.Currency;

                                    }
                                    else if (_feeRequest.SafekeepingAction.marketValueSpecified)
                                    {
                                        opReturnDecValue = _feeRequest.SafekeepingAction.marketValue.Amount.DecValue * (qty / _feeRequest.SafekeepingAction.marketValueQty);
                                        opReturnStrValue = _feeRequest.SafekeepingAction.marketValue.Currency;
                                    }
                                    else
                                    {
                                        IMoney marketValue = dstContainer.ProductBase.CreateMoney(0, string.Empty);
                                        opReturnDecValue = marketValue.Amount.DecValue;
                                        opReturnStrValue = marketValue.Currency;
                                    }
                                }
                                else
                                {
                                    opReturnDecValue = debtSecurityTransaction.GrossAmount.PaymentAmount.Amount.DecValue;
                                    opReturnStrValue = debtSecurityTransaction.GrossAmount.PaymentAmount.Currency;
                                }
                                break;
                            #endregion MarketValue

                            #region CleanMarketValue (FI 20190918)
                            case Cst.AssessmentBasisEnum.CleanMarketValue:
                                errorMessage = null;
                                if (_feeRequest.IsSafekeeping)
                                {
                                    decimal qty = _feeRequest.SafekeepingAction.quantity.DecValue;
                                    if (_feeRequest.SafekeepingAction.marketValueDataSpecified)
                                    {
                                        MarketValueData data = _feeRequest.SafekeepingAction.marketValueData;
                                        IMoney marketValuePrincipalAmount = dstContainer.CalcPrincipalAmount(pCS, data.unitNotional.Value, data.currency, qty, data.cleanPrice.Value);

                                        opReturnDecValue = marketValuePrincipalAmount.Amount.DecValue;
                                        opReturnStrValue = marketValuePrincipalAmount.Currency;
                                    }
                                    else
                                    {
                                        GetAssessmentBasisValue(pCS, pDbTransaction, Cst.AssessmentBasisEnum.MarketValue, ref opReturnDecValue, ref opReturnStrValue, ref opErrorMessage);
                                    }
                                }
                                else
                                {
                                    GetAssessmentBasisValue(pCS, pDbTransaction, Cst.AssessmentBasisEnum.SecurityCleanAmount, ref opReturnDecValue, ref opReturnStrValue, ref opErrorMessage);
                                }
                                break;
                                #endregion CleanMarketValue
                        }
                    }
                    #endregion IsDebtSecurityTransaction
                    #region IsRepo or IsBuyAndSellBack
                    else if (FeeRequest.Product.IsRepo || FeeRequest.Product.IsBuyAndSellBack)
                    {
                        ISaleAndRepurchaseAgreement saleAndRepurchaseAgreement = (ISaleAndRepurchaseAgreement)FeeRequest.Product.Product;
                        InterestRateStreamsContainer streams = new InterestRateStreamsContainer(saleAndRepurchaseAgreement.CashStream);
                        //
                        switch (pAssessmentBasis)
                        {
                            #region InitialNotionalAmount & CurrentNotionalAmount
                            case Cst.AssessmentBasisEnum.InitialNotionalAmount:
                            case Cst.AssessmentBasisEnum.CurrentNotionalAmount:     //glop TODO
                            case Cst.AssessmentBasisEnum.CurrencyReferenceAmount:   //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency1Amount:           //On considère "InitialNotionalAmount"
                            case Cst.AssessmentBasisEnum.Currency2Amount:           //On considère "InitialNotionalAmount"
                                IMoney money = FeeRequest.DataDocument.GetMainCurrencyAmount(pCS, pDbTransaction);
                                if (null != money)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = money.Amount.DecValue;
                                    opReturnStrValue = money.GetCurrency.Value;
                                }
                                break;
                            #endregion
                            #region SecurityNotionalAmount
                            case Cst.AssessmentBasisEnum.SecurityNotionalAmount:
                                IMoney money1 = null;
                                if (ArrFunc.IsFilled(saleAndRepurchaseAgreement.SpotLeg))
                                    money1 = saleAndRepurchaseAgreement.SpotLeg[0].DebtSecurityTransaction.Quantity.NotionalAmount;
                                //
                                if (null != money1)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = money1.Amount.DecValue;
                                    opReturnStrValue = money1.GetCurrency.Value;
                                }
                                break;
                            #endregion
                            #region SecurityGrossAmount
                            case Cst.AssessmentBasisEnum.SecurityGrossAmount:
                                IPayment grossAmount = null;
                                //
                                if (ArrFunc.IsFilled(saleAndRepurchaseAgreement.SpotLeg))
                                    grossAmount = saleAndRepurchaseAgreement.SpotLeg[0].DebtSecurityTransaction.GrossAmount;
                                //
                                if (null != grossAmount)
                                {
                                    errorMessage = null;
                                    grossAmount = saleAndRepurchaseAgreement.SpotLeg[0].DebtSecurityTransaction.GrossAmount;
                                    opReturnDecValue = grossAmount.PaymentAmount.Amount.DecValue;
                                    opReturnStrValue = grossAmount.PaymentAmount.Currency;
                                }
                                break;
                            #endregion
                            #region FixedRate
                            case Cst.AssessmentBasisEnum.FixedRate:
                            case Cst.AssessmentBasisEnum.FaceRate:                  //On considère "FixedRate"
                            case Cst.AssessmentBasisEnum.TradingRate:               //On considère "FixedRate"
                                ISchedule fixedRate = null;
                                fixedRate = streams.GetFirstFixedRate(FeeRequest.DataDocument.DataDocument);
                                if (null != fixedRate)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = fixedRate.InitialValue.DecValue;
                                }
                                break;
                            #endregion
                            #region NumberOfPeriodInYear
                            case Cst.AssessmentBasisEnum.NumberOfPeriodInYear:
                                IInterestRateStream firstStream2 = streams.GetFirstStreamWithNotional(FeeRequest.DataDocument.DataDocument);
                                if (null != firstStream2)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = NumberOfPeriodInYear(firstStream2.PaymentDates.PaymentFrequency, ref errorMessage);
                                }
                                break;
                            #endregion
                            #region CurrencyAmountSpotLeg
                            case Cst.AssessmentBasisEnum.Currency1AmountSpotLeg:
                            case Cst.AssessmentBasisEnum.Currency2AmountSpotLeg:
                                errorMessage = null;
                                opReturnDecValue = saleAndRepurchaseAgreement.SpotLeg[0].DebtSecurityTransaction.Quantity.NotionalAmount.Amount.DecValue;
                                opReturnStrValue = saleAndRepurchaseAgreement.SpotLeg[0].DebtSecurityTransaction.Quantity.NotionalAmount.Currency;
                                break;
                            #endregion
                            #region CurrencyAmountForwardLeg
                            case Cst.AssessmentBasisEnum.Currency1AmountForwardLeg:
                            case Cst.AssessmentBasisEnum.Currency2AmountForwardLeg:
                                if (saleAndRepurchaseAgreement.ForwardLegSpecified)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = saleAndRepurchaseAgreement.ForwardLeg[0].DebtSecurityTransaction.Quantity.NotionalAmount.Amount.DecValue;
                                    opReturnStrValue = saleAndRepurchaseAgreement.ForwardLeg[0].DebtSecurityTransaction.Quantity.NotionalAmount.Currency;
                                }
                                break;
                            #endregion
                            #region SpotRate
                            case Cst.AssessmentBasisEnum.SpotRate:
                                errorMessage = null;
                                if (saleAndRepurchaseAgreement.SpotLeg[0].DebtSecurityTransaction.Price.CleanPriceSpecified)
                                    opReturnDecValue = saleAndRepurchaseAgreement.SpotLeg[0].DebtSecurityTransaction.Price.CleanPrice.DecValue;
                                else if (saleAndRepurchaseAgreement.SpotLeg[0].DebtSecurityTransaction.Price.DirtyPriceSpecified)
                                    opReturnDecValue = saleAndRepurchaseAgreement.SpotLeg[0].DebtSecurityTransaction.Price.DirtyPrice.DecValue;
                                break;
                            #endregion
                            #region ForwardRate
                            case Cst.AssessmentBasisEnum.ForwardRate:
                                if (saleAndRepurchaseAgreement.ForwardLegSpecified)
                                {
                                    errorMessage = null;
                                    if (saleAndRepurchaseAgreement.ForwardLeg[0].DebtSecurityTransaction.Price.CleanPriceSpecified)
                                        opReturnDecValue = saleAndRepurchaseAgreement.ForwardLeg[0].DebtSecurityTransaction.Price.CleanPrice.DecValue;
                                    else if (saleAndRepurchaseAgreement.ForwardLeg[0].DebtSecurityTransaction.Price.DirtyPriceSpecified)
                                        opReturnDecValue = saleAndRepurchaseAgreement.ForwardLeg[0].DebtSecurityTransaction.Price.DirtyPrice.DecValue;
                                }
                                break;
                            #endregion
                            #region Quantity
                            case Cst.AssessmentBasisEnum.Quantity:
                            case Cst.AssessmentBasisEnum.QuantityContractMultiplier:
                                if (saleAndRepurchaseAgreement.SpotLeg[0].DebtSecurityTransaction.Quantity.NumberOfUnitsSpecified)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = saleAndRepurchaseAgreement.SpotLeg[0].DebtSecurityTransaction.Quantity.NumberOfUnits.DecValue;
                                }
                                break;
                                #endregion
                        }
                    }
                    #endregion IsRepo or IsBuyAndSellBack
                    #region  isExchangeTradedDerivative
                    else if ((FeeRequest.Product.IsExchangeTradedDerivative) || (FeeRequest.Product.IsStrategy))
                    {
                        // RD 20110429
                        // Utilisation d'un accesseur
                        IExchangeTradedDerivative exchangeTradedDerivative = FeeRequest.ExchangeTradedDerivative;
                        //
                        switch (pAssessmentBasis)
                        {
                            #region CurrentNotionalAmount,InitialNotionalAmount,Currency1Amount
                            case Cst.AssessmentBasisEnum.CurrentNotionalAmount:
                            case Cst.AssessmentBasisEnum.InitialNotionalAmount:
                            case Cst.AssessmentBasisEnum.Currency1Amount:
                                IMoney money = FeeRequest.DataDocument.CurrentProduct.GetMainCurrencyAmount(pCS, pDbTransaction);
                                if (null != money)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = money.Amount.DecValue;
                                    opReturnStrValue = money.GetCurrency.Value;
                                }
                                break;
                            #endregion
                            #region Quantity
                            case Cst.AssessmentBasisEnum.Quantity:
                                errorMessage = null;
                                // 20120606 MF TODO Ticket 17863 - Comment ça peut-il marcher pour les ordres/stratégies ?
                                opReturnDecValue = exchangeTradedDerivative.TradeCaptureReport.LastQty.DecValue;
                                break;
                            #endregion
                            #region QuantityContractMultiplier, Premium, PremiumRate, Strike, ETDNotionalAmount
                            case Cst.AssessmentBasisEnum.QuantityContractMultiplier:
                            case Cst.AssessmentBasisEnum.Premium:
                            case Cst.AssessmentBasisEnum.PremiumRate:
                            case Cst.AssessmentBasisEnum.Strike:
                            case Cst.AssessmentBasisEnum.ETDNotionalAmount:
                                bool isOption = (exchangeTradedDerivative.Category == CfiCodeCategoryEnum.Option);

                                //RD 20140801 [20245] Gestion des dates Activation/désativation des DC à l'application des barêmes de frais
                                DateTime dtClearing = exchangeTradedDerivative.TradeCaptureReport.TradeDate.DateValue;
                                if (exchangeTradedDerivative.TradeCaptureReport.ClearingBusinessDateSpecified)
                                    dtClearing = exchangeTradedDerivative.TradeCaptureReport.ClearingBusinessDate.DateValue;

                                ExchangeTradedDerivativeContainer exchangeTradedDerivativeContainer =
                                    new ExchangeTradedDerivativeContainer(pCS, pDbTransaction, exchangeTradedDerivative, FeeRequest.Product.DataDocument,
                                        SQL_Table.ScanDataDtEnabledEnum.Yes, dtClearing);

                                // EG 20171128 Add Test on exchangeTradedDerivativeContainer.derivativeContract
                                if (null != exchangeTradedDerivativeContainer.DerivativeContract)
                                {
                                    if ((exchangeTradedDerivativeContainer.ContractMultiplier == decimal.Zero) && (pAssessmentBasis != Cst.AssessmentBasisEnum.Premium))
                                    {
                                        //NB: Test sur Cst.AssessmentBasisEnum.Premium pour compatibilité ascendante (PL)
                                        errorMessage += Cst.CrLf + "Missing Contract Multiplier on Derivative contract";
                                    }
                                    else
                                    {
                                        switch (pAssessmentBasis)
                                        {
                                            #region QuantityContractMultiplier
                                            case Cst.AssessmentBasisEnum.QuantityContractMultiplier:
                                                //PL 20150316 Harmonisation...    
                                                if (exchangeTradedDerivative.TradeCaptureReport.LastQtySpecified)
                                                {
                                                    errorMessage = null;
                                                    // 20120606 MF TODO Ticket 17863 - Comment ça peut-il marcher pour les ordres/stratégies ? 
                                                    //opReturnDecValue = exchangeTradedDerivative.tradeCaptureReport.LastQty.DecValue * FeeRequest.Product.GetContractMultiplier(pCS);
                                                    opReturnDecValue = exchangeTradedDerivative.TradeCaptureReport.LastQty.DecValue
                                                                        * exchangeTradedDerivativeContainer.ContractMultiplier;
                                                }
                                                break;
                                            #endregion
                                            #region Premium, ETDNotionalAmount
                                            case Cst.AssessmentBasisEnum.Premium:
                                            case Cst.AssessmentBasisEnum.ETDNotionalAmount:
                                                if (isOption)
                                                {
                                                    //Option: ETDNotionalAmount is Premium
                                                    EFS_ETDPremium premium = new EFS_ETDPremium(pCS, pDbTransaction, exchangeTradedDerivativeContainer);
                                                    errorMessage = null;
                                                    opReturnStrValue = premium.premiumAmount.GetCurrency.Value;
                                                    opReturnDecValue = premium.premiumAmount.Amount.DecValue;
                                                }
                                                else if (pAssessmentBasis == Cst.AssessmentBasisEnum.ETDNotionalAmount)
                                                {
                                                    //Option: ETDNotionalAmount is Qty*CM*Px
                                                    if (exchangeTradedDerivative.TradeCaptureReport.LastPxSpecified && exchangeTradedDerivative.TradeCaptureReport.LastQtySpecified)
                                                    {
                                                        errorMessage = null;
                                                        opReturnDecValue = exchangeTradedDerivative.TradeCaptureReport.LastQty.DecValue
                                                                            * exchangeTradedDerivativeContainer.ContractMultiplierQuotedCurrency
                                                                            * exchangeTradedDerivative.TradeCaptureReport.LastPx.DecValue;
                                                    }
                                                }
                                                break;
                                            #endregion
                                            #region PremiumRate
                                            case Cst.AssessmentBasisEnum.PremiumRate:
                                                if (isOption)
                                                {
                                                    errorMessage = null;
                                                    //PM 20140507 [19970][19259] Utilisation de PriceQuotedCurrency au lieu de PriceCurrency
                                                    opReturnStrValue = exchangeTradedDerivativeContainer.PriceQuotedCurrency;
                                                    //PM 20140509 [19970][19259] Utilisation de ContractMultiplierQuotedCurrency au lieu de ContractMultiplier
                                                    //opReturnDecValue = exchangeTradedDerivative.tradeCaptureReport.LastPx.DecValue
                                                    //                    * exchangeTradedDerivativeContainer.Qty
                                                    //                    * exchangeTradedDerivativeContainer.ContractMultiplierQuotedCurrency;
                                                    //PL 20150316 Retourne uniquement (sans CM ni QTY)
                                                    opReturnDecValue = exchangeTradedDerivative.TradeCaptureReport.LastPx.DecValue;
                                                }
                                                break;
                                            #endregion
                                            #region Strike
                                            case Cst.AssessmentBasisEnum.Strike:
                                                if (isOption && exchangeTradedDerivative.TradeCaptureReport.Instrument.StrikePriceSpecified)
                                                {
                                                    errorMessage = null;
                                                    opReturnStrValue = exchangeTradedDerivativeContainer.NominalCurrency;
                                                    //PL 20131022 Si devise du nominal non renseignée on considère devise du prix
                                                    if (String.IsNullOrEmpty(opReturnStrValue))
                                                    {
                                                        //PM 20140507 [19970][19259] Utilisation de PriceQuotedCurrency au lieu de PriceCurrency
                                                        opReturnStrValue = exchangeTradedDerivativeContainer.PriceQuotedCurrency;
                                                    }
                                                    //PM 20140509 [19970][19259] Utilisation de ContractMultiplierQuotedCurrency au lieu de ContractMultiplier
                                                    opReturnDecValue = exchangeTradedDerivative.TradeCaptureReport.Instrument.StrikePrice.DecValue
                                                                        * exchangeTradedDerivativeContainer.Qty
                                                                        * exchangeTradedDerivativeContainer.ContractMultiplierQuotedCurrency;
                                                }
                                                break;
                                                #endregion
                                        }
                                    }
                                }

                                break;
                            #endregion
                            // 20120606 MF Ticket 17863 - case MaxQuantity for ETD strategy orders
                            case Cst.AssessmentBasisEnum.LegsMaxQuantity:
                            case Cst.AssessmentBasisEnum.LegsMaxQuantityContractMultiplier:
                            case Cst.AssessmentBasisEnum.LegsMinQuantity:
                            case Cst.AssessmentBasisEnum.LegsMinQuantityContractMultiplier:
                            case Cst.AssessmentBasisEnum.LegsAvgQuantity:
                            case Cst.AssessmentBasisEnum.LegsAvgQuantityContractMultiplier:
                                errorMessage = null;
                                opReturnDecValue = GetAssesmentBasisValueStrategyExecution(pCS, pDbTransaction, pAssessmentBasis, ref errorMessage);
                                break;

                            // FI 20141013 [20418] add TradingPrice
                            case Cst.AssessmentBasisEnum.TradingPrice:
                            case Cst.AssessmentBasisEnum.TradingRate:
                                //PL 20150312 Future or Option
                                //if (exchangeTradedDerivative.category.Value == CfiCodeCategoryEnum.Future)
                                //{
                                errorMessage = null;
                                if (exchangeTradedDerivative.TradeCaptureReport.LastPxSpecified)
                                    opReturnDecValue = exchangeTradedDerivative.TradeCaptureReport.LastPx.DecValue;
                                //}
                                break;
                        }
                    }
                    #endregion
                    #region IsEquitySecurityTransaction
                    //PL 20120525
                    else if (FeeRequest.Product.IsEquitySecurityTransaction)
                    {
                        IEquitySecurityTransaction equitySecurityTransaction = (IEquitySecurityTransaction)FeeRequest.Product.Product;
                        switch (pAssessmentBasis)
                        {
                            #region CurrentNotionalAmount, InitialNotionalAmount, Currency1Amount
                            case Cst.AssessmentBasisEnum.CurrentNotionalAmount:
                            case Cst.AssessmentBasisEnum.InitialNotionalAmount:
                            case Cst.AssessmentBasisEnum.Currency1Amount:
                            case Cst.AssessmentBasisEnum.SecurityGrossAmount:
                            case Cst.AssessmentBasisEnum.SecurityNotionalAmount:
                                IMoney money = FeeRequest.DataDocument.GetMainCurrencyAmount(pCS, pDbTransaction);
                                if (null != money)
                                {
                                    errorMessage = null;
                                    opReturnDecValue = money.Amount.DecValue;
                                    opReturnStrValue = money.GetCurrency.Value;
                                }
                                break;
                            #endregion
                            #region Quantity
                            case Cst.AssessmentBasisEnum.Quantity:
                                errorMessage = null;
                                // EG 20150708 [21103] lecture da la quantité sur _feeRequest.SafekeepingAction
                                if (this._feeRequest.IsSafekeeping)
                                    // EG 20150920 [21374] Int (int32) to Long (Int64) 
                                    // EG 20170127 Qty Long To Decimal
                                    opReturnDecValue = _feeRequest.SafekeepingAction.quantity.DecValue;
                                else
                                    opReturnDecValue = equitySecurityTransaction.TradeCaptureReport.LastQty.DecValue;
                                break;
                            #endregion
                            #region FixedRate
                            case Cst.AssessmentBasisEnum.FixedRate:
                            case Cst.AssessmentBasisEnum.FaceRate:                  //On considère "FixedRate"
                            case Cst.AssessmentBasisEnum.TradingRate:               //On considère "FixedRate"
                            case Cst.AssessmentBasisEnum.ForwardRate:               //On considère "FixedRate"
                            case Cst.AssessmentBasisEnum.TradingPrice:              //On considère "FixedRate" // FI 20141013 [20418] add 
                                errorMessage = null;
                                opReturnDecValue = equitySecurityTransaction.TradeCaptureReport.LastPx.DecValue;
                                break;
                            #endregion
                            // EG 20150708 [21103] New
                            #region MarketValue
                            case Cst.AssessmentBasisEnum.MarketValue:
                                errorMessage = null;
                                if (this._feeRequest.IsSafekeeping)
                                {
                                    // EG 20190926 Pas de lecture du MKV du jour : 
                                    // Recalcul permanent sur la babse de la quantité en position en date de Settlement
                                    //if (_feeRequest.SafekeepingAction.marketValueSpecified)
                                    //{
                                    //    opReturnDecValue = _feeRequest.SafekeepingAction.marketValue.amount.DecValue;
                                    //    opReturnStrValue = _feeRequest.SafekeepingAction.marketValue.currency;
                                    //}
                                    //else
                                    //{
                                    EquitySecurityTransactionContainer equitySecurityTransactionContainer =
                                        new EquitySecurityTransactionContainer(pCS, pDbTransaction, equitySecurityTransaction, this.FeeRequest.DataDocument);

                                    IMoney marketValue = equitySecurityTransactionContainer.CalcGrossAmount(pCS, pDbTransaction,
                                        _feeRequest.SafekeepingAction.quantity.IntValue, _feeRequest.SafekeepingAction.closingPrice.value);
                                    if (null != marketValue)
                                    {
                                        opReturnDecValue = marketValue.Amount.DecValue;
                                        opReturnStrValue = marketValue.Currency;
                                    }
                                    //}
                                }
                                else
                                {
                                    opReturnDecValue = equitySecurityTransaction.GrossAmount.PaymentAmount.Amount.DecValue;
                                    opReturnStrValue = equitySecurityTransaction.GrossAmount.PaymentAmount.Currency;
                                }
                                break;

                                #endregion MarketValue
                        }
                    }
                    #endregion IsEquitySecurityTransaction
                    #region  IsCommoditySpot
                    else if (FeeRequest.Product.IsCommoditySpot)
                    {
                        ICommoditySpot commoditySpot = (ICommoditySpot)this.FeeRequest.Product.Product;
                        CommoditySpotContainer commoditySpotContainer = new CommoditySpotContainer(commoditySpot);
                        //
                        switch (pAssessmentBasis)
                        {
                            #region Quantity
                            case Cst.AssessmentBasisEnum.Quantity:
                                errorMessage = null;
                                opReturnDecValue = commoditySpotContainer.Qty;
                                break;
                            #endregion Quantity
                            #region TradingPrice
                            case Cst.AssessmentBasisEnum.TradingPrice:
                            case Cst.AssessmentBasisEnum.TradingRate:
                                errorMessage = null;
                                opReturnDecValue = commoditySpot.FixedLeg.FixedPrice.Price.DecValue;
                                break;
                            #endregion TradingPrice
                            #region MarketValue
                            case Cst.AssessmentBasisEnum.MarketValue:
                                errorMessage = null;
                                opReturnDecValue = commoditySpot.FixedLeg.GrossAmount.PaymentAmount.Amount.DecValue;
                                break;
                                #endregion MarketValue
                        }
                    }
                    #endregion IsCommoditySpot
                }
                //                
                ret = (StrFunc.IsEmpty(errorMessage));
            }
            catch (Exception ex)
            {
                ret = false;
                errorMessage = " Error on GetAssessmentBasisValue('" + pAssessmentBasis + "')" + Cst.CrLf + ex.Message;
            }
            opErrorMessage = errorMessage;
            return ret;
        }

        // Ticket 17863
        // TODO 20120606 MF - Les Les Stratégies ETD « identifiées en EOD », ALLOC. 
        //                    Instrument simple sur lequel il n'existe qu'une seule jambe (Leg). 
        //                    Elles ne sont pas encore gérées. 
        // EG 20180307 [23769] Gestion dbTransaction
        private decimal GetAssesmentBasisValueStrategyExecution(string pCS, IDbTransaction pDbTransaction,  Cst.AssessmentBasisEnum pAssessmentBasis, ref string opErrorMessage)
        {
            decimal multiplier = 1;
            decimal qty = 0;

            if (
                pAssessmentBasis == Cst.AssessmentBasisEnum.LegsMaxQuantityContractMultiplier
                ||
                pAssessmentBasis == Cst.AssessmentBasisEnum.LegsMinQuantityContractMultiplier
                ||
                pAssessmentBasis == Cst.AssessmentBasisEnum.LegsAvgQuantityContractMultiplier
                )
            {
                multiplier = FeeRequest.Product.GetContractMultiplier(pCS, pDbTransaction);
            }

            // the specification (Ticket 17863) take care of strategy ETD, 
            // containing just internal sub products type of ETD
            if (FeeRequest.Product is StrategyContainer container && container.ContainsOnlyETD)
            {
                opErrorMessage = null;

                IEnumerable<IExchangeTradedDerivative> optionLegs =
                    container.SubProduct
                    // the specification (Ticket 17863) take care of options only
                    .Where(elem => ((IExchangeTradedDerivative)elem).Category.Value == CfiCodeCategoryEnum.Option)
                    .Select(elem => (IExchangeTradedDerivative)elem);

                switch (pAssessmentBasis)
                {
                    case Cst.AssessmentBasisEnum.LegsMaxQuantityContractMultiplier:
                    case Cst.AssessmentBasisEnum.LegsMaxQuantity:

                        qty =
                            optionLegs
                            .Select(elem => ((IExchangeTradedDerivative)elem).TradeCaptureReport.LastQty.DecValue)
                            .Max();

                        break;

                    case Cst.AssessmentBasisEnum.LegsMinQuantityContractMultiplier:
                    case Cst.AssessmentBasisEnum.LegsMinQuantity:

                        qty =
                            optionLegs
                            .Select(elem => ((IExchangeTradedDerivative)elem).TradeCaptureReport.LastQty.DecValue)
                            .Min();

                        break;

                    case Cst.AssessmentBasisEnum.LegsAvgQuantityContractMultiplier:
                    case Cst.AssessmentBasisEnum.LegsAvgQuantity:

                        qty =
                            optionLegs
                            .Select(elem => ((IExchangeTradedDerivative)elem).TradeCaptureReport.LastQty.DecValue)
                            .Sum()
                            /
                            optionLegs.Count();

                        break;
                }

            }

            return multiplier * qty;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTypeSideParty"></param>
        /// <param name="pIsReverseParty"></param>
        /// <param name="pOtherParty1Index"></param>
        /// <param name="pOtherParty2Index"></param>
        /// <returns></returns>
        //PL 20221124 PL 20230718 Test FL/Tradition - New signature with return Party instead of PartyBase
        //public PartyBase GetPartyPayerReceiver(string pTypeSideParty, bool pIsReverseParty, int pOtherParty1Index, int pOtherParty2Index)
        public Party GetPartyPayerReceiver(string pTypeSideParty, bool pIsReverseParty, int pOtherParty1Index, int pOtherParty2Index)
        {
            //PL 20221124 PartyBase ret = null; 
            Party ret = null;
            //
            try
            {
                if (pTypeSideParty == TypeSidePartyEnum.PartyA.ToString())
                {
                    ret = (pIsReverseParty ? FeeRequest.PartyB : FeeRequest.PartyA);
                }
                else if (pTypeSideParty == TypeSidePartyEnum.PartyB.ToString())
                {
                    ret = (pIsReverseParty ? FeeRequest.PartyA : FeeRequest.PartyB);
                }
                else if (pTypeSideParty == TypeSidePartyEnum.OtherParty1.ToString())
                {
                    // RD 20110216 (à supprimer)
                    //ret = (pIsReverseOtherParty ? FeeRequest.OtherParty2 : FeeRequest.OtherParty1);
                    //
                    // RD 20110216 (new)
                    if (pOtherParty1Index >= 0)
                    {
                        //PL 20221124 PL 20230718 ret = FeeRequest.PartyBroker[pOtherParty1Index];
                        PartyBase tmp = FeeRequest.PartyBroker[pOtherParty1Index];
                        ret = new Party(tmp.m_Party_Ida, tmp.m_Party_Role, 0, TradeSideEnum.All, tmp.m_Party_Href, tmp.m_Party_PartyId);
                    }
                }
                else if (pTypeSideParty == TypeSidePartyEnum.OtherParty2.ToString())
                {
                    // RD 20110216 (à supprimer)
                    //ret = (pIsReverseOtherParty ? FeeRequest.OtherParty1 : FeeRequest.OtherParty2);
                    //
                    // RD 20110216 (new)                    
                    if (pOtherParty2Index >= 0)
                    {
                        //PL 20221124 PL 20230718 ret = FeeRequest.PartyBroker[pOtherParty2Index];
                        PartyBase tmp = FeeRequest.PartyBroker[pOtherParty2Index];
                        ret = new Party(tmp.m_Party_Ida, tmp.m_Party_Role, 0, TradeSideEnum.All, tmp.m_Party_Href, tmp.m_Party_PartyId);
                    }
                }
                else if (pTypeSideParty == TypeSidePartyEnum.ClearingHouse.ToString())
                {
                    ret = null;
                    //TODO_F&Oml
                    ret = (pIsReverseParty ? FeeRequest.PartyB : FeeRequest.PartyA);
                }
            }
            catch
            {
                //Party absente du trade
                ret = null;
            }
            //
            return ret;
        }

        /// <summary>
        /// Return a amount countervalue of an amount
        /// </summary>
        /// <param name="pExchangeType"></param>
        /// <param name="pPartyPayer"></param>
        /// <param name="pPartyReceiver"></param>
        /// <param name="pDate"></param>
        /// <param name="pExchangeIDC"></param>
        /// <param name="opValue"></param>
        /// <param name="opIDC"></param>
        /// <param name="opMessage"></param>
        /// <returns></returns>
        // EG 20180307 [23769] Gestion dbTransaction
        public bool GetExchangeValue(string pCS, IDbTransaction pDbTransaction, string pExchangeType, PartyBase pPartyPayer, PartyBase pPartyReceiver, DateTime pDate,
            string pExchangeIDC,
            ref decimal opValue, ref string opIDC, ref string opMessage)
        {
            bool ret;
            try
            {
                opMessage = string.Empty;
                string errMessage = string.Empty;
                decimal quoteValue = 0;
                QuoteBasisEnum quoteBasis = QuoteBasisEnum.Currency1PerCurrency2;
                KeyQuote keyQuote = null;
                KeyAssetFxRate keyAssetFXRate = new KeyAssetFxRate();
                SQL_Quote quote = null;
                DateTime feeExchangeDate = pDate;
                //
                if ((pExchangeType == FeeExchangeTypeEnum.SpotRate.ToString())
                    || (pExchangeType == FeeExchangeTypeEnum.ExchangeRate.ToString()))
                {
                    #region Exchange from rate
                    //20090128 20090227 PL/EPL 
                    Cst.AssessmentBasisEnum tmp_AssessmentBasis = Cst.AssessmentBasisEnum.SpotRate;
                    if (pExchangeType == FeeExchangeTypeEnum.ExchangeRate.ToString())
                        tmp_AssessmentBasis = Cst.AssessmentBasisEnum.ForwardRate;
                    //
                    string strValue = null;
                    //Get the rate value from GetAssessmentBasisValue() method
                    ret = GetAssessmentBasisValue(pCS, pDbTransaction, tmp_AssessmentBasis, ref quoteValue, ref strValue, ref errMessage);
                    if (ret)
                    {
                        opMessage = pExchangeType + " ";
                        opMessage += " " + StrFunc.FmtDecimalToInvariantCulture(quoteValue);
                        #region KeyAssetFXRate
                        keyAssetFXRate.IdC1 = opIDC;
                        keyAssetFXRate.IdC2 = pExchangeIDC;
                        #endregion KeyAssetFXRate
                        //Get the currencies of the rate value
                        string idc1 = null, idc2 = null;
                        if (FeeRequest.Product.IsFxLeg)
                        {
                            IFxLeg fxLeg = (IFxLeg)this.FeeRequest.Product.Product;
                            idc1 = fxLeg.ExchangeRate.QuotedCurrencyPair.Currency1;
                            idc2 = fxLeg.ExchangeRate.QuotedCurrencyPair.Currency2;
                            quoteBasis = fxLeg.ExchangeRate.QuotedCurrencyPair.QuoteBasis;
                        }
                        else if (FeeRequest.Product.IsFxSwap)
                        {
                            FxSwapContainer fxSwap = new FxSwapContainer((IFxSwap)this.FeeRequest.Product.Product);
                            //
                            IFxLeg fxLeg;
                            if (tmp_AssessmentBasis == Cst.AssessmentBasisEnum.SpotRate)
                                fxLeg = fxSwap.GetFirstLeg();
                            else
                                fxLeg = fxSwap.GetLastLeg();
                            //
                            idc1 = fxLeg.ExchangeRate.QuotedCurrencyPair.Currency1;
                            idc2 = fxLeg.ExchangeRate.QuotedCurrencyPair.Currency2;
                            quoteBasis = fxLeg.ExchangeRate.QuotedCurrencyPair.QuoteBasis;
                        }
                        else
                        {
                            ret = false;
                            errMessage = "Invalid ExchangeType";
                        }
                        if (ret)
                        {
                            //Compare the currencies
                            if ((idc1 == opIDC && idc2 == pExchangeIDC) || (idc2 == opIDC && idc1 == pExchangeIDC))
                            {
                                ret = true;
                            }
                            else
                            {
                                ret = false;
                                errMessage = "Incoherence between the currencies (Rate:" + idc1 + "/" + idc2 + "  Schedule:" + opIDC + "/" + pExchangeIDC + ")";
                            }
                        }
                    }
                    #endregion Exchange from rate
                }
                else
                {
                    #region Exchange from fixing
                    ret = GetExchangeFixingDate(pCS, pDbTransaction, pExchangeType, pDate, opIDC, pExchangeIDC, ref feeExchangeDate);
                    //
                    if (ret)
                    {
                        //
                        int payer_Ida, payer_Idb, receiver_Ida, receiver_Idb;
                        if (pPartyPayer.GetType().Equals(typeof(Party)))
                        {
                            payer_Ida = ((Party)pPartyPayer).m_Party_Ida;
                            payer_Idb = ((Party)pPartyPayer).m_Party_Idb;
                        }
                        else
                        {
                            payer_Ida = ((OtherParty)pPartyPayer).m_Party_Ida;
                            payer_Idb = 0;
                        }
                        if (pPartyReceiver.GetType().Equals(typeof(Party)))
                        {
                            receiver_Ida = ((Party)pPartyReceiver).m_Party_Ida;
                            receiver_Idb = ((Party)pPartyReceiver).m_Party_Idb;
                        }
                        else
                        {
                            receiver_Ida = ((OtherParty)pPartyReceiver).m_Party_Ida;
                            receiver_Idb = 0;
                        }
                        keyQuote = new KeyQuote(pCS, feeExchangeDate, payer_Ida, payer_Idb, receiver_Ida, receiver_Idb, QuoteTimingEnum.Close);

                        #region KeyAssetFXRate
                        keyAssetFXRate.IdC1 = opIDC;
                        keyAssetFXRate.IdC2 = pExchangeIDC;
                        keyAssetFXRate.SetQuoteBasis(true);
                        #endregion KeyAssetFXRate
                        quote = new SQL_Quote(pCS, QuoteEnum.FXRATE, AvailabilityEnum.Enabled,
                            FeeRequest.DataDocument.CurrentProduct.ProductBase, keyQuote, keyAssetFXRate)
                        {
                            DbTransaction = pDbTransaction
                        };
                        //
                        // 20090609 RD Utilisation de la devise Pivot
                        //
                        opMessage = DtFunc.DateTimeToStringDateISO(feeExchangeDate) + " ";
                        if (((KeyAssetFxRate)quote.KeyAssetIN).QuoteBasis == QuoteBasisEnum.Currency1PerCurrency2)
                            opMessage += keyAssetFXRate.IdC2 + "./" + keyAssetFXRate.IdC1;
                        else
                            opMessage += keyAssetFXRate.IdC1 + "./" + keyAssetFXRate.IdC2;
                        //                            
                        bool isLoaded = quote.IsLoaded;
                        if (isLoaded)
                        {
                            ret = isLoaded && (quote.QuoteValueCodeReturn == Cst.ErrLevel.SUCCESS);
                            //
                            if (ret)
                            {
                                quoteValue = quote.QuoteValue;
                                quoteBasis = ((KeyAssetFxRate)quote.KeyAssetIN).QuoteBasis;
                            }
                            else
                                errMessage = quote.QuoteValueMessage;
                        }
                        else
                        {
                            errMessage = quote.QuoteValueMessage;
                            //
                            // Tentative de recherche du fixing en passant par la devise pivot:
                            // - soit la devise comptable 
                            // - ou bien la devise "EUR" par défaut
                            //
                            //if (quote.QuoteValueCodeReturn == Cst.ErrLevel.DATANOTFOUND)
                            if (quote.QuoteValueCodeReturn == Cst.ErrLevel.QUOTENOTFOUND)
                            {
                                string accountingCurrency = "EUR";
                                //
                                #region Rechercher la devise Comptable
                                int IdEntity = 0;
                                //
                                if (receiver_Idb > 0)
                                {
                                    SQL_Book receiverBook = new SQL_Book(pCS, receiver_Idb)
                                    {
                                        DbTransaction = pDbTransaction
                                    };
                                    if (receiverBook.IsLoaded)
                                        IdEntity = receiverBook.IdA_Entity;
                                }
                                if (IdEntity <= 0 && payer_Idb > 0)
                                {
                                    SQL_Book payerBook = new SQL_Book(pCS, payer_Idb)
                                    {
                                        DbTransaction = pDbTransaction
                                    };
                                    if (payerBook.IsLoaded)
                                        IdEntity = payerBook.IdA_Entity;
                                }
                                //
                                if (IdEntity > 0)
                                {
                                    SQL_Actor actor = new SQL_Actor(pCS, IdEntity)
                                    {
                                        DbTransaction = pDbTransaction,
                                        WithInfoEntity = true
                                    };
                                    if (actor.IsLoaded)
                                    {
                                        // UNDONE 20120808 Annulation renaming IDCAccount, roll back to IdCAccount to allow solution to generate
                                        if (actor.IsEntityExist && (StrFunc.IsFilled(actor.IdCAccount)))
                                            accountingCurrency = actor.IdCAccount;
                                    }
                                }
                                #endregion
                                //
                                ret = quote.GetQuoteByPivotCurrency(accountingCurrency);
                                //
                                if (ret)
                                {
                                    quoteValue = quote.QuoteValue;
                                    quoteBasis = ((KeyAssetFxRate)quote.KeyAssetIN).QuoteBasis;
                                    //
                                    errMessage = string.Empty;
                                }
                            }
                        }
                    }
                    #endregion Exchange from fixing
                }
                //
                if (ret)
                {
                    EFS_Cash cash = new EFS_Cash(pCS, pDbTransaction, keyAssetFXRate.IdC1, keyAssetFXRate.IdC2, opValue,
                        quoteValue, quoteBasis);
                    opValue = cash.ExchangeAmount;
                    opIDC = pExchangeIDC;
                    //
                    opMessage += " " + StrFunc.FmtDecimalToInvariantCulture(quoteValue);
                }
                else
                {
                    opMessage += " " + errMessage;
                }
            }
            catch (Exception e)
            {
                ret = false;
                opMessage = e.Message;
            }
            //
            return ret;
        }


        /// <summary>
        /// Return the Exchange Fixing Date
        /// </summary>
        /// <param name="pExchangeType"></param>
        /// <param name="pDate"></param>
        /// <param name="pIDC1"></param>
        /// <param name="pIDC2"></param>
        /// <param name="pExchangeFixingDate"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        private bool GetExchangeFixingDate(string pCS, IDbTransaction pDbTransaction, string pExchangeType, DateTime pDate, string pIDC1, string pIDC2,
            ref DateTime opExchangeFixingDate)
        {
            bool ret;
            try
            {
                if ((pExchangeType == FeeExchangeTypeEnum.DayBeforeTransactDate.ToString())
                    || (pExchangeType == FeeExchangeTypeEnum.DayBeforeEventDate.ToString()))
                {
                    IOffset offset = FeeRequest.DataDocument.CurrentProduct.ProductBase.CreateOffset(PeriodEnum.D, -1, DayTypeEnum.CurrencyBusiness);
                    IBusinessCenters businessCenters = offset.GetBusinessCentersCurrency(pCS, pDbTransaction, pIDC1, pIDC2);
                    IBusinessDayAdjustments bda = businessCenters.GetBusinessDayAdjustments(BusinessDayConventionEnum.PRECEDING);
                    EFS_Offset efsOffset = null;
                    if (pExchangeType == FeeExchangeTypeEnum.DayBeforeTransactDate.ToString())
                        efsOffset = new EFS_Offset(pCS, offset, FeeRequest.DtReference, bda, FeeRequest.DataDocument);
                    else if (pExchangeType == FeeExchangeTypeEnum.DayBeforeEventDate.ToString())
                        efsOffset = new EFS_Offset(pCS, offset, pDate, bda, FeeRequest.DataDocument);
                    //
                    opExchangeFixingDate = efsOffset.offsetDate[0];
                }
                else if (pExchangeType == FeeExchangeTypeEnum.TransactDate.ToString())
                {
                    opExchangeFixingDate = FeeRequest.DataDocument.TradeHeader.TradeDate.DateValue;
                }
                else if (pExchangeType == FeeExchangeTypeEnum.EventDate.ToString())
                {
                    opExchangeFixingDate = pDate;
                }
                else if (pExchangeType == FeeExchangeTypeEnum.ValueDate.ToString())
                {
                    //PL 20100316 TBD isStrategy et isExchangeTradedDerivative 
                    //PL 20140710 TBD IsreturnSwap
                    DateTime dt1 = DateTime.MinValue;
                    //TODO FEEMATRIX (autres products...)
                    #region IsSwap
                    if (FeeRequest.Product.IsSwap)
                    {
                        ISwap swap = (ISwap)this.FeeRequest.Product.Product;
                        //
                        InterestRateStreamsContainer streams = new InterestRateStreamsContainer(swap.Stream);
                        dt1 = streams.GetMinEffectiveDate(FeeRequest.DataDocument.DataDocument);
                    }
                    #endregion
                    #region IsCapFloor
                    else if (this.FeeRequest.Product.IsCapFloor)
                    {
                        ICapFloor capFloor = (ICapFloor)this.FeeRequest.Product.Product;
                        //
                        InterestRateStreamContainer stream = new InterestRateStreamContainer(capFloor.Stream);
                        dt1 = stream.GetEffectiveDate(FeeRequest.DataDocument.DataDocument);
                    }
                    #endregion
                    #region IsFra
                    else if (this.FeeRequest.Product.IsFra)
                    {
                        IFra fra = (IFra)this.FeeRequest.Product.Product;
                        //
                        dt1 = fra.AdjustedEffectiveDate.DateValue;
                    }
                    #endregion
                    #region IsBulletPayment
                    else if (this.FeeRequest.Product.IsBulletPayment)
                    {
                        IBulletPayment bulletPayment = (IBulletPayment)this.FeeRequest.Product.Product;
                        //
                        dt1 = bulletPayment.Payment.AdjustedPaymentDate;
                    }
                    #endregion
                    #region IsLoanDeposit
                    else if (this.FeeRequest.Product.IsLoanDeposit)
                    {
                        ILoanDeposit loanDeposit = (ILoanDeposit)this.FeeRequest.Product.Product;
                        InterestRateStreamsContainer streams = new InterestRateStreamsContainer(loanDeposit.Stream);
                        //
                        dt1 = streams.GetMinEffectiveDate(FeeRequest.DataDocument.DataDocument);
                    }
                    #endregion
                    #region IsSwaption
                    else if (this.FeeRequest.Product.IsSwaption)
                    {
                        //ISwaption swaption = (ISwaption)this.FeeRequest.Trade.currentTrade.product;
                        //
                        dt1 = this.FeeRequest.DtReference;
                    }
                    #endregion IsSwaption
                    #region IsFxLeg
                    else if (this.FeeRequest.Product.IsFxLeg)
                    {
                        IFxLeg fxLeg = (IFxLeg)this.FeeRequest.Product.Product;
                        //
                        if (fxLeg.FxDateValueDateSpecified)
                            dt1 = fxLeg.FxDateValueDate.DateValue;
                        else if (fxLeg.FxDateCurrency1ValueDateSpecified)
                            dt1 = fxLeg.FxDateCurrency1ValueDate.DateValue;
                        else if (fxLeg.FxDateCurrency2ValueDateSpecified)
                            dt1 = fxLeg.FxDateCurrency2ValueDate.DateValue;
                    }
                    #endregion IsFxLeg
                    #region IsFxSwap
                    else if (this.FeeRequest.Product.IsFxSwap)
                    {
                        IFxSwap fxSwap = (IFxSwap)this.FeeRequest.Product.Product;
                        //
                        dt1 = DateTime.MinValue;
                        if (fxSwap.FxSingleLeg[0].FxDateValueDateSpecified)
                            dt1 = fxSwap.FxSingleLeg[0].FxDateValueDate.DateValue;
                        else if (fxSwap.FxSingleLeg[0].FxDateCurrency1ValueDateSpecified)
                            dt1 = fxSwap.FxSingleLeg[0].FxDateCurrency1ValueDate.DateValue;
                        else if (fxSwap.FxSingleLeg[0].FxDateCurrency2ValueDateSpecified)
                            dt1 = fxSwap.FxSingleLeg[0].FxDateCurrency2ValueDate.DateValue;
                        //
                        DateTime dtTmp;
                        for (int i = 1; i < fxSwap.FxSingleLeg.Length; i++)
                        {
                            dtTmp = DateTime.MinValue;
                            if (fxSwap.FxSingleLeg[i].FxDateValueDateSpecified)
                                dtTmp = fxSwap.FxSingleLeg[i].FxDateValueDate.DateValue;
                            else if (fxSwap.FxSingleLeg[i].FxDateCurrency1ValueDateSpecified)
                                dtTmp = fxSwap.FxSingleLeg[i].FxDateCurrency1ValueDate.DateValue;
                            else if (fxSwap.FxSingleLeg[i].FxDateCurrency2ValueDateSpecified)
                                dtTmp = fxSwap.FxSingleLeg[i].FxDateCurrency2ValueDate.DateValue;
                            if (DtFunc.IsDateTimeFilled(dtTmp))
                            {
                                if (dt1 > dtTmp)
                                    dt1 = dtTmp;
                            }
                        }
                    }
                    #endregion IsFxSwap
                    #region IsFxTermDeposit
                    else if (this.FeeRequest.Product.IsFxTermDeposit)
                    {
                        ITermDeposit fxTermDeposit = (ITermDeposit)this.FeeRequest.Product.Product;
                        //
                        dt1 = fxTermDeposit.StartDate.DateValue;
                    }
                    #endregion IsFxTermDeposit

                    #region IsEquityOption
                    else if (this.FeeRequest.Product.IsEquityOption)
                    {
                        EFS_EquityOption equityOption = new EFS_EquityOption(pCS, (IEquityOption)this.FeeRequest.Product.Product, 
                            FeeRequest.DataDocument);
                        dt1 = equityOption.EffectiveDate.unadjustedDate.DateValue;
                    }
                    #endregion IsEquityOption

                    #region IsDebtSecurityTransaction
                    else if (this.FeeRequest.Product.IsDebtSecurityTransaction)
                    {
                        IDebtSecurityTransaction debtSecurityTransaction = (IDebtSecurityTransaction)this.FeeRequest.Product.Product;
                        //
                        if (debtSecurityTransaction.GrossAmount.PaymentDateSpecified)
                            dt1 = debtSecurityTransaction.GrossAmount.PaymentDate.UnadjustedDate.DateValue;
                    }
                    #endregion IsDebtSecurityTransaction
                    #region IsRepo or IsBuyAndSellBack
                    else if (this.FeeRequest.Product.IsRepo || this.FeeRequest.Product.IsBuyAndSellBack)
                    {
                        ISaleAndRepurchaseAgreement saleAndRepurchaseAgreement = (ISaleAndRepurchaseAgreement)this.FeeRequest.Product.Product;
                        InterestRateStreamsContainer streams = new InterestRateStreamsContainer(saleAndRepurchaseAgreement.CashStream);
                        dt1 = streams.GetMinEffectiveDate(this.FeeRequest.DataDocument.DataDocument);
                    }
                    #endregion IsRepo or IsBuyAndSellBack
                    
                    opExchangeFixingDate = dt1;
                }
                
                ret = DtFunc.IsDateTimeFilled(opExchangeFixingDate);
            }
            catch
            {
                ret = false;
            }
            //
            return ret;
        }

        /// <summary>
        /// Return the DCF object from a Product
        /// </summary>
        /// <param name="pDCF"></param>
        /// <param name="opDCF"></param>
        /// <returns></returns>
        public bool GetDCF(string pCS, DayCountFractionEnum pDCF, out EFS_DayCountFraction opDCF)
        {
            bool ret = false;
            opDCF = null;

            //FI 20091223 [16471] Appel à DataDocument.GetStartAndEndDates
            FeeRequest.DataDocument.GetStartAndEndDates(pCS, false, out DateTime dt1, out DateTime dt2);

            if (DtFunc.IsDateTimeFilled(dt1) && DtFunc.IsDateTimeFilled(dt2))
                ret = GetDCF(pDCF, dt1, dt2, out opDCF);
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDCF"></param>
        /// <param name="pDt1"></param>
        /// <param name="pDt2"></param>
        /// <param name="opDCF"></param>
        /// <returns></returns>
        public bool GetDCF(DayCountFractionEnum pDCF, DateTime pDt1, DateTime pDt2, out EFS_DayCountFraction opDCF)
        {
            bool ret = false;
            opDCF = null;
            try
            {
                if (DtFunc.IsDateTimeFilled(pDt1) && DtFunc.IsDateTimeFilled(pDt2))
                {
                    IInterval interval = FeeRequest.DataDocument.CurrentProduct.ProductBase.CreateInterval(PeriodEnum.T, 1);
                    EFS_DayCountFraction efs_dcf = new EFS_DayCountFraction(pDt1, pDt2, pDCF, interval);
                    //
                    opDCF = efs_dcf;
                    //
                    ret = true;
                }
            }
            catch
            {
                opDCF = null;
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAssessmentBasis"></param>
        /// <param name="pValue"></param>
        /// <returns></returns>
        private static decimal ConvertPeriod(Cst.AssessmentBasisEnum pAssessmentBasis, double pValue)
        {
            decimal ret = 0;
            switch (pAssessmentBasis)
            {
                case Cst.AssessmentBasisEnum.PeriodInDay:
                    ret = (decimal)pValue;
                    break;
                case Cst.AssessmentBasisEnum.PeriodInWeek:
                    ret = (decimal)pValue / 7;
                    break;
                case Cst.AssessmentBasisEnum.PeriodInMonth:
                    ret = (decimal)pValue / 30;
                    break;
                case Cst.AssessmentBasisEnum.PeriodInYear:
                    ret = (decimal)pValue / 365;
                    break;
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pInterval"></param>
        /// <param name="opErrorMessage"></param>
        /// <returns></returns>
        private static decimal NumberOfPeriodInYear(IInterval pInterval, ref string opErrorMessage)
        {
            decimal ret = 0;
            opErrorMessage = null;
            switch (pInterval.Period)
            {
                case PeriodEnum.T:
                    ret = (decimal)1;
                    break;
                case PeriodEnum.M:
                    ret = (decimal)12 / pInterval.PeriodMultiplier.IntValue;
                    break;
                case PeriodEnum.W:
                    ret = (decimal)52 / pInterval.PeriodMultiplier.IntValue;
                    break;
                default:
                    opErrorMessage = "Error: Period is " + pInterval.Period.ToString();
                    break;
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsProductWithADP()
        {
            bool ret =
                   (this.FeeRequest.Product.IsSwap)
                || (this.FeeRequest.Product.IsCapFloor)
                || (this.FeeRequest.Product.IsLoanDeposit)
                || (this.FeeRequest.Product.IsReturnSwap)
                ;
            return ret;
        }

        #endregion Methods
    }
}