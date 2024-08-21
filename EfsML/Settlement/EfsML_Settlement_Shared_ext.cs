#region using directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.GUI.Interface;
using EfsML.Interface;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

#endregion using directives

namespace EfsML.Settlement
{
    #region Issis
    public partial class Issis
    {
        #region Accessors
        #region Count
        public int Count
        {
            get
            {
                return ArrFunc.Count(issi);
            }
        }
        #endregion Count
        #endregion Accessors
        #region Indexors
        public Issi this[int pIndex]
        {
            get
            {
                return issi[pIndex];
            }
        }
        #endregion Indexors
        #region Constructors
        public Issis() { }
        #endregion Constructors
        #region Methods
        #endregion Methods
    }
    #endregion Issis

    #region IssiItem
    public partial class IssiItem : IComparable
    {
        #region IComparable Members
        public int CompareTo(object pobj)
        {
            IssiItem IssiItem2 = (IssiItem)pobj;
            int ret = 0;
            //idA
            if (IssiItem2.idA != idA)
                ret = -1;
            //idRoleActor
            if (ret == 0)
            {
                if (IssiItem2.idRoleActor != idRoleActor)
                    ret = -1;
            }
            //sequenceNumber
            if (ret == 0)
            {
                if (IssiItem2.sequenceNumber != sequenceNumber)
                    ret = -1;
            }
            //chainParty
            if (ret == 0)
            {
                if (IssiItem2.chainPartySpecified && chainPartySpecified && (IssiItem2.chainParty != chainParty))
                    ret = -1;
                else if ((false == IssiItem2.chainPartySpecified) && (chainPartySpecified))
                    ret = -1;
                else if ((IssiItem2.chainPartySpecified) && (false == chainPartySpecified))
                    ret = -1;
            }
            //idPartyRole		
            if (ret == 0)
            {
                if (IssiItem2.idPartyRoleSpecified && idPartyRoleSpecified && (IssiItem2.idPartyRole != idPartyRole))
                    ret = -1;
                else if ((false == IssiItem2.idPartyRoleSpecified) && (idPartyRoleSpecified))
                    ret = -1;
                else if ((IssiItem2.idPartyRoleSpecified) && (false == idPartyRoleSpecified))
                    ret = -1;
            }
            //cAccountNumber
            if (ret == 0)
            {
                if (IssiItem2.cAccountNumberSpecified && cAccountNumberSpecified && (IssiItem2.cAccountNumber != cAccountNumber))
                    ret = -1;
                else if ((false == IssiItem2.cAccountNumberSpecified) && (cAccountNumberSpecified))
                    ret = -1;
                else if ((IssiItem2.cAccountNumberSpecified) && (false == cAccountNumberSpecified))
                    ret = -1;
            }
            //cAccountNumberIdent
            if (ret == 0)
            {
                if (IssiItem2.cAccountNumberIdentSpecified && cAccountNumberIdentSpecified && (IssiItem2.cAccountNumberIdent != cAccountNumberIdent))
                    ret = -1;
                else if ((false == IssiItem2.cAccountNumberIdentSpecified) && (cAccountNumberIdentSpecified))
                    ret = -1;
                else if ((IssiItem2.cAccountNumberIdentSpecified) && (false == cAccountNumberIdentSpecified))
                    ret = -1;
            }
            //cAccountName
            if (ret == 0)
            {
                if (IssiItem2.cAccountNameSpecified && cAccountNameSpecified && (IssiItem2.cAccountName != cAccountName))
                    ret = -1;
                else if ((false == IssiItem2.cAccountNameSpecified) && (cAccountNameSpecified))
                    ret = -1;
                else if ((IssiItem2.cAccountNameSpecified) && (false == cAccountNameSpecified))
                    ret = -1;
            }


            //sAccountNumber
            if (ret == 0)
            {
                if (IssiItem2.sAccountNumberSpecified && sAccountNumberSpecified && (IssiItem2.sAccountNumber != sAccountNumber))
                    ret = -1;
                else if ((false == IssiItem2.sAccountNumberSpecified) && (sAccountNumberSpecified))
                    ret = -1;
                else if ((IssiItem2.sAccountNumberSpecified) && (false == sAccountNumberSpecified))
                    ret = -1;
            }
            //sAccountNumberIdent
            if (ret == 0)
            {
                if (IssiItem2.sAccountNumberIdentSpecified && sAccountNumberIdentSpecified && (IssiItem2.sAccountNumberIdent != sAccountNumberIdent))
                    ret = -1;
                else if ((false == IssiItem2.sAccountNumberIdentSpecified) && (sAccountNumberIdentSpecified))
                    ret = -1;
                else if ((IssiItem2.sAccountNumberIdentSpecified) && (false == sAccountNumberIdentSpecified))
                    ret = -1;
            }
            //cAccountName
            if (ret == 0)
            {
                if (IssiItem2.sAccountNameSpecified && sAccountNameSpecified && (IssiItem2.sAccountName != sAccountName))
                    ret = -1;
                else if ((false == IssiItem2.sAccountNameSpecified) && (sAccountNameSpecified))
                    ret = -1;
                else if ((IssiItem2.sAccountNameSpecified) && (false == sAccountNameSpecified))
                    ret = -1;
            }
            //
            return ret;

        }
        #endregion IComparable Members
    }
    #endregion IssiItem
    #region IssiItems
    public partial class IssiItems : IComparer, IComparable
    {

        #region Constructors
        public IssiItems() { }
        public IssiItems(string pConnectionString, int pIdIssi, IssiItem[] pIssiItem)
        {
            issiItem = pIssiItem;
            source = pConnectionString;
            idIssi = pIdIssi;

            #region Maj des Sequence Number
            int occurence = 0;
            string roleActor = string.Empty;
            for (int i = 0; i < issiItem.Length; i++)
            {
                if (roleActor == issiItem[i].idRoleActor.ToString())
                {
                    occurence++;
                }
                else
                {
                    roleActor = issiItem[i].idRoleActor.ToString();
                    occurence = 0;
                }
                issiItem[i].sequenceNumber = occurence;
            }
            #endregion Maj des Sequence Number
            //
            IssiItem css = this[RoleActorSSI.CSS];
            if (null != css)
                idCss = css.idA;

        }
        #endregion Constructors
        #region Indexors
        public IssiItem this[RoleActorSSI pIdRole]
        {
            get
            {
                return this[pIdRole, 0];
            }
        }
        public IssiItem this[RoleActorSSI pIdRole, int pSequenceNumber]
        {
            get
            {
                IssiItem ret = null;
                for (int i = 0; i < issiItem.Length; i++)
                {
                    if ((issiItem[i].idRoleActor == pIdRole) && (issiItem[i].sequenceNumber == pSequenceNumber))
                    {
                        ret = issiItem[i];
                        break;
                    }
                }
                return ret;
            }
        }
        #endregion Indexors

        #region Methods
        #region CreateEfsSettlementInstruction
        protected virtual IEfsSettlementInstruction CreateEfsSettlementInstruction()
        {
            return (IEfsSettlementInstruction)null;
        }
        #endregion
        #region GetActorList
        public int[] GetActorList()
        {
            ArrayList list = new ArrayList();
            foreach (string s in System.Enum.GetNames(typeof(RoleActorSSI)))
            {
                RoleActorSSI role = (RoleActorSSI)System.Enum.Parse(typeof(RoleActorSSI), s);
                int i = 0;
                while (i > -1)
                {
                    IssiItem item = this[role, i];
                    if (null == item)
                        i = -1;
                    else
                    {
                        i++;
                        if (!list.Contains(item.idA))
                            list.Add(item.idA);
                    }
                }
            }
            return (int[])list.ToArray(typeof(int));

        }
        #endregion
        #region GetInstruction
        /// <summary>
        /// Génération d'une instruction de RL 
        /// </summary>
        /// <param name="pSettlementActorsBuilder"></param>
        /// <param name="pPayerReceiver"></param>
        /// <param name="pIsSecurityFlow"></param>
        /// <returns></returns>
        protected IEfsSettlementInstruction GetInstruction(SettlementRoutingActorsBuilder pSettlementActorsBuilder, PayerReceiverEnum pPayerReceiver, bool pIsSecurityFlow)
        {

            #region CSS
            IssiItem css = this[RoleActorSSI.CSS];
            IEfsSettlementInstruction efsSi = CreateEfsSettlementInstruction();

            efsSi.SettlementMethodInformationSpecified = (null != css);
            if (efsSi.SettlementMethodInformationSpecified)
            {
                IRoutingIdsAndExplicitDetails routingInfo = GetRoutingIdsAndExplicitDetails(css, pSettlementActorsBuilder, pIsSecurityFlow);
                //
                efsSi.SettlementMethodInformation = efsSi.CreateRouting();
                efsSi.SettlementMethodInformation.RoutingIdsAndExplicitDetailsSpecified = (null != routingInfo);
                if (efsSi.SettlementMethodInformation.RoutingIdsAndExplicitDetailsSpecified)
                {
                    efsSi.SettlementMethodInformation.RoutingIdsAndExplicitDetails = routingInfo;
                    efsSi.SettlementMethod = efsSi.CreateSettlementMethod();
                    efsSi.SettlementMethod.Value = efsSi.SettlementMethodInformation.RoutingIdsAndExplicitDetails.RoutingName.Value;
                    efsSi.SettlementMethodSpecified = StrFunc.IsFilled(efsSi.SettlementMethod.Value);
                }
            }
            #endregion CSS
            if (PayerReceiverEnum.Payer == pPayerReceiver)
            {
                #region CORRESPONDANT
                IssiItem agent = this[RoleActorSSI.CORRESPONDENT];
                efsSi.CorrespondentInformationSpecified = (null != agent);
                if (efsSi.CorrespondentInformationSpecified)
                {
                    IRoutingIdsAndExplicitDetails routingInfo = GetRoutingIdsAndExplicitDetails(agent, pSettlementActorsBuilder, pIsSecurityFlow);
                    //
                    efsSi.CorrespondentInformation = efsSi.CreateCorrespondentInformation();
                    efsSi.CorrespondentInformation.RoutingIdsAndExplicitDetailsSpecified = (null != routingInfo);
                    if (efsSi.CorrespondentInformation.RoutingIdsAndExplicitDetailsSpecified)
                        efsSi.CorrespondentInformation.RoutingIdsAndExplicitDetails = routingInfo;
                }
                #endregion CORRESPONDANT
            }
            else if (PayerReceiverEnum.Receiver == pPayerReceiver)
            {
                #region INTERMEDIAIRE
                ArrayList listIntermediary = new ArrayList();
                int i = 0;
                while (i > -1)
                {
                    IssiItem intermediary = this[RoleActorSSI.INTERMEDIARY, i];
                    if (null == intermediary)
                        i = -1;
                    else
                    {
                        i++;
                        IIntermediaryInformation intermediaryInformation = efsSi.CreateIntermediaryInformation();
                        if (intermediary.sequenceNumberSpecified)
                            intermediaryInformation.IntermediarySequenceNumber = new EFS_PosInteger(intermediary.sequenceNumber + 1);
                        //
                        IRoutingIdsAndExplicitDetails routingInfo = GetRoutingIdsAndExplicitDetails(intermediary, pSettlementActorsBuilder, pIsSecurityFlow);
                        intermediaryInformation.RoutingIdsAndExplicitDetailsSpecified = (null != routingInfo);
                        if (intermediaryInformation.RoutingIdsAndExplicitDetailsSpecified)
                            intermediaryInformation.RoutingIdsAndExplicitDetails = routingInfo;
                        listIntermediary.Add(intermediaryInformation);
                    }
                }
                efsSi.IntermediaryInformationSpecified = (0 < listIntermediary.Count);
                if (efsSi.IntermediaryInformationSpecified)
                    efsSi.IntermediaryInformation = (IIntermediaryInformation[])listIntermediary.ToArray(listIntermediary[0].GetType());
                #endregion INTERMEDIAIRE
            }
            #region ACCOUNTSERVICER
            IssiItem accountServicer = this[EFS.Actor.RoleActorSSI.ACCOUNTSERVICER];
            efsSi.BeneficiaryBankSpecified = (null != accountServicer);
            if (efsSi.BeneficiaryBankSpecified)
            {
                IRoutingIdsAndExplicitDetails routingInfo = GetRoutingIdsAndExplicitDetails(accountServicer, pSettlementActorsBuilder, pIsSecurityFlow);
                efsSi.BeneficiaryBank = efsSi.CreateRouting();
                efsSi.BeneficiaryBank.RoutingIdsAndExplicitDetailsSpecified = (null != routingInfo);
                if (efsSi.BeneficiaryBank.RoutingIdsAndExplicitDetailsSpecified)
                    efsSi.BeneficiaryBank.RoutingIdsAndExplicitDetails = routingInfo;
            }
            #endregion ACCOUNTSERVICER
            #region ACCOUNTOWNER
            IssiItem accountOwner = this[EFS.Actor.RoleActorSSI.ACCOUNTOWNER];
            if (null != accountOwner)
            {
                IRoutingIdsAndExplicitDetails routingInfo = GetRoutingIdsAndExplicitDetails(accountOwner, pSettlementActorsBuilder, pIsSecurityFlow);
                efsSi.Beneficiary = efsSi.CreateBeneficiary();
                efsSi.Beneficiary.RoutingIdsAndExplicitDetailsSpecified = (null != routingInfo);
                if (efsSi.Beneficiary.RoutingIdsAndExplicitDetailsSpecified)
                    efsSi.Beneficiary.RoutingIdsAndExplicitDetails = routingInfo;
            }
            #endregion ACCOUNTOWNER
            #region INVESTOR
            IssiItem investor = this[EFS.Actor.RoleActorSSI.INVESTOR];
            if (null != investor)
            {
                IRoutingIdsAndExplicitDetails routingInfo = GetRoutingIdsAndExplicitDetails(investor, pSettlementActorsBuilder, pIsSecurityFlow);
                efsSi.InvestorInformation = efsSi.CreateRouting();
                efsSi.InvestorInformation.RoutingIdsAndExplicitDetailsSpecified = (null != routingInfo);
                if (efsSi.InvestorInformation.RoutingIdsAndExplicitDetailsSpecified)
                    efsSi.InvestorInformation.RoutingIdsAndExplicitDetails = routingInfo;
            }
            #endregion INVESTOR
            #region ORIGINATOR
            IssiItem originator = this[RoleActorSSI.ORIGINATOR];
            if (null != originator)
            {
                IRoutingIdsAndExplicitDetails routingInfo = GetRoutingIdsAndExplicitDetails(originator, pSettlementActorsBuilder, pIsSecurityFlow);
                efsSi.OriginatorInformation = efsSi.CreateRouting();
                efsSi.OriginatorInformation.RoutingIdsAndExplicitDetailsSpecified = (null != routingInfo);
                if (efsSi.OriginatorInformation.RoutingIdsAndExplicitDetailsSpecified)
                    efsSi.OriginatorInformation.RoutingIdsAndExplicitDetails = routingInfo;
            }
            #endregion ORIGINATOR

            return efsSi;


        }
        #endregion GetInstruction
        #region GetRoutingIdsAndExplicitDetails
        /// <summary>
        /// Génération d'un IRoutingIdsAndExplicitDetails d'un acteur de la chaîne de RL (pIssiItem)
        /// <para>
        /// Génération à partir des infos acteurs (displayName, description,adresse...) présents dans la chaîne (pSettlementRoutingActorsBuilder)
        /// </para>
        /// </summary>
        /// <param name="pIssiItem">acteur de la chaîne pour lequel on cherche à générer un IRoutingIdsAndExplicitDetails </param>
        /// <param name="pSettlementRoutingActorsBuilder">Liste enrichie des acteurs de la chaîne</param>
        /// <param name="pIsSecurityFlow">true si evènement matière (EVENTTYPE = QTY)</param>
        /// <returns></returns>
        private static IRoutingIdsAndExplicitDetails GetRoutingIdsAndExplicitDetails(IssiItem pIssiItem, SettlementRoutingActorsBuilder pSettlementRoutingActorsBuilder, bool pIsSecurityFlow)
        {
            //
            // Add Info From Actor
            IRoutingIdsAndExplicitDetails routingInfo = pSettlementRoutingActorsBuilder.GetRoutingIdsAndExplicitDetails(pIssiItem.idA);
            //
            // Add Info From ISSIITEM
            ArrayList aRoutingId = new ArrayList();
            if (false == pIsSecurityFlow)
            {
                routingInfo.RoutingAccountNumberSpecified = pIssiItem.cAccountNumberSpecified;
                if (routingInfo.RoutingAccountNumberSpecified)
                    routingInfo.RoutingAccountNumber = new EFS_String(pIssiItem.cAccountNumber);
                //
                routingInfo.RoutingReferenceTextSpecified = pIssiItem.cAccountNameSpecified;
                if (routingInfo.RoutingReferenceTextSpecified)
                    routingInfo.RoutingReferenceText = new EFS_StringArray[] { new EFS_StringArray(pIssiItem.cAccountName) };
            }
            else
            {
                routingInfo.RoutingAccountNumberSpecified = pIssiItem.sAccountNumberSpecified;
                if (routingInfo.RoutingAccountNumberSpecified)
                    routingInfo.RoutingAccountNumber = new EFS_String(pIssiItem.sAccountNumber);
                //
                routingInfo.RoutingReferenceTextSpecified = pIssiItem.sAccountNameSpecified;
                if (routingInfo.RoutingReferenceTextSpecified)
                    routingInfo.RoutingReferenceText = new EFS_StringArray[] { new EFS_StringArray(pIssiItem.sAccountName) };

            }
            //
            if (routingInfo.RoutingAccountNumberSpecified)
            {
                aRoutingId.AddRange(routingInfo.RoutingIds[0].RoutingId);
                IRoutingId rId = pSettlementRoutingActorsBuilder.RoutingCreateElement.CreateRoutingId();
                rId.RoutingIdCodeScheme = Cst.OTCml_ActorAccountNumberIdent;
                rId.Value = pIssiItem.cAccountNumberIdent;
                aRoutingId.Add(rId);
                routingInfo.RoutingIds[0].SetRoutingId(aRoutingId);
            }
            //
            return routingInfo;

        }
        #endregion GetRoutingIdsAndExplicitDetails
        #region Sort
        public bool Sort()
        {
            bool isOk = (null != issiItem);
            if (isOk)
                Array.Sort(issiItem, this);
            return isOk;
        }
        #endregion Sort
        #endregion Methods

        #region IComparer Members
        public int Compare(object x, object y)
        {
            int ret = 0;

            if ((x is IssiItem issiItemX) && (y is IssiItem issiItemY))
            {
                //CSS
                if ((issiItemX.idRoleActor == RoleActorSSI.CSS) && (issiItemY.idRoleActor != RoleActorSSI.CSS))
                    ret = -1;
                else if ((issiItemY.idRoleActor == RoleActorSSI.CSS) && (issiItemX.idRoleActor != RoleActorSSI.CSS))
                    ret = 1;
                else if ((issiItemX.idRoleActor == RoleActorSSI.CSS) && (issiItemY.idRoleActor == RoleActorSSI.CSS))
                    ret = (issiItemX.sequenceNumber - issiItemY.sequenceNumber);

                //CORRESPONDENT
                if (ret == 0)
                {
                    if ((issiItemX.idRoleActor == RoleActorSSI.CORRESPONDENT) && (issiItemY.idRoleActor != RoleActorSSI.CORRESPONDENT))
                        ret = -1;
                    else if ((issiItemY.idRoleActor == RoleActorSSI.CORRESPONDENT) && (issiItemX.idRoleActor != RoleActorSSI.CORRESPONDENT))
                        ret = 1;
                    else if ((issiItemX.idRoleActor == RoleActorSSI.CORRESPONDENT) && (issiItemY.idRoleActor == RoleActorSSI.CORRESPONDENT))
                        ret = (issiItemX.sequenceNumber - issiItemY.sequenceNumber);
                }
                //INTERMEDIARY
                if (ret == 0)
                {

                    if ((issiItemX.idRoleActor == RoleActorSSI.INTERMEDIARY) && (issiItemY.idRoleActor != RoleActorSSI.INTERMEDIARY))
                        ret = -1;
                    else if ((issiItemY.idRoleActor == RoleActorSSI.INTERMEDIARY) && (issiItemX.idRoleActor != RoleActorSSI.INTERMEDIARY))
                        ret = 1;
                    else if ((issiItemX.idRoleActor == RoleActorSSI.INTERMEDIARY) && (issiItemY.idRoleActor == RoleActorSSI.INTERMEDIARY))
                        ret = (issiItemX.sequenceNumber - issiItemY.sequenceNumber);
                }
                //ACCOUNTSERVICER
                if (ret == 0)
                {

                    if ((issiItemX.idRoleActor == RoleActorSSI.ACCOUNTSERVICER) && (issiItemY.idRoleActor != RoleActorSSI.ACCOUNTSERVICER))
                        ret = -1;
                    else if ((issiItemY.idRoleActor == RoleActorSSI.ACCOUNTSERVICER) && (issiItemX.idRoleActor != RoleActorSSI.ACCOUNTSERVICER))
                        ret = 1;
                    else if ((issiItemX.idRoleActor == RoleActorSSI.ACCOUNTSERVICER) && (issiItemY.idRoleActor == RoleActorSSI.ACCOUNTSERVICER))
                        ret = (issiItemX.sequenceNumber - issiItemY.sequenceNumber);
                }
                //ACCOUNTOWNER
                if (ret == 0)
                {

                    if ((issiItemX.idRoleActor == RoleActorSSI.ACCOUNTOWNER) && (issiItemY.idRoleActor != RoleActorSSI.ACCOUNTOWNER))
                        ret = -1;
                    else if ((issiItemY.idRoleActor == RoleActorSSI.ACCOUNTOWNER) && (issiItemX.idRoleActor != RoleActorSSI.ACCOUNTOWNER))
                        ret = 1;
                    else if ((issiItemX.idRoleActor == RoleActorSSI.ACCOUNTOWNER) && (issiItemY.idRoleActor == RoleActorSSI.ACCOUNTOWNER))
                        ret = (issiItemX.sequenceNumber - issiItemY.sequenceNumber);
                }
                //INVESTOR
                if (ret == 0)
                {

                    if ((issiItemX.idRoleActor == RoleActorSSI.INVESTOR) && (issiItemY.idRoleActor != RoleActorSSI.INVESTOR))
                        ret = -1;
                    else if ((issiItemY.idRoleActor == RoleActorSSI.INVESTOR) && (issiItemX.idRoleActor != RoleActorSSI.INVESTOR))
                        ret = 1;
                    else if ((issiItemX.idRoleActor == RoleActorSSI.INVESTOR) && (issiItemY.idRoleActor == RoleActorSSI.INVESTOR))
                        ret = (issiItemX.sequenceNumber - issiItemY.sequenceNumber);
                }
                //ORIGINATOR
                if (ret == 0)
                {

                    if ((issiItemX.idRoleActor == RoleActorSSI.ORIGINATOR) && (issiItemY.idRoleActor != RoleActorSSI.ORIGINATOR))
                        ret = -1;
                    else if ((issiItemY.idRoleActor == RoleActorSSI.ORIGINATOR) && (issiItemX.idRoleActor != RoleActorSSI.ORIGINATOR))
                        ret = 1;
                    else if ((issiItemX.idRoleActor == RoleActorSSI.ORIGINATOR) && (issiItemY.idRoleActor == RoleActorSSI.ORIGINATOR))
                        ret = (issiItemX.sequenceNumber - issiItemY.sequenceNumber);
                }
            }
            return ret;

        }
        #endregion IComparer Members
        #region IComparable Members
        public int CompareTo(object pobj)
        {

            int ret = 0;
            IssiItems issiItems2 = (IssiItems)pobj;
            //
            if (issiItem.Length != issiItems2.issiItem.Length)
                ret = -1;
            //
            if (0 == ret)
            {
                Sort();
                issiItems2.Sort();
                for (int i = 0; i < issiItem.Length; i++)
                {
                    ret = issiItem[i].CompareTo(issiItems2.issiItem[i]);
                    if (0 != ret)
                        break;
                }
            }
            return ret;

        }
        #endregion IComparable Members
    }
    #endregion

    #region SsiDbs
    public partial class SsiDbs : IComparer
    {
        #region Accessors
        #region Count
        public int Count
        {
            get
            {
                return (ArrFunc.Count(ssidb));
            }
        }
        #endregion Count
        #endregion Accessors
        #region Indexors
        public SsiDb this[int pIndex]
        {
            get
            {
                return ssidb[pIndex];
            }
        }
        public SsiDb this[int pIdAStlOffice, int pPriorityRank]
        {
            get
            {
                SsiDb ret = null;
                if (null != ssidb)
                {
                    for (int i = 0; i < ssidb.Length; i++)
                    {
                        if (ssidb[i].idAStlOffice == pIdAStlOffice && ssidb[i].priorityRank == pPriorityRank)
                        {
                            ret = ssidb[i];
                            break;
                        }
                    }
                }
                return ret;
            }
        }
        #endregion Indexors
        #region Methods
        #region InitializeActorSsiDb
        public void InitializeActorSsiDb(string pSource, int pIdA)
        {
            SsiDbs ssidbs = SsiDbs.LoadActorSsiDb(pSource, pIdA);
            ssidb = ssidbs.ssidb;
        }
        #endregion
        #region LoadActorSsiDb
        public static SsiDbs LoadActorSsiDb(string pCS, int pIdA)
        {
            SsiDbs ssiDbs = null;
            TextWriter writer = null;
            try
            {
                SQL_SSIdb sql_ssidb = new SQL_SSIdb(pCS, SQL_Table.ScanDataDtEnabledEnum.Yes, pIdA);
                QueryParameters qry = sql_ssidb.GetQueryParameters(
                    new string[]{"IDSSIDB","IDA_STLOFFICE","DESCRIPTION",
									DataHelper.SQLRTrim(pCS, "DBTYPE","DBTYPE"),"IDA_CUSTODIAN",
									"PRIORITYRANK",
									DataHelper.SQLRTrim(pCS, "CODE","CODE"),
									"URL","IDSSIFORMATREQUEST","IDSSIFORMATANSWER","EXTLLINK"});
                //
                DataSet dsResult = DataHelper.ExecuteDataset(qry.Cs, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
                dsResult.DataSetName = "SSIDBS";
                DataTable dtTable = dsResult.Tables[0];
                dtTable.TableName = "SSIDB";
                //
                string serializerResult = new DatasetSerializer(dsResult).Serialize();
                EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(SsiDbs), serializerResult);
                ssiDbs = (SsiDbs)CacheSerializer.Deserialize(serializeInfo);
            }
            catch (Exception) { throw; }
            finally
            {
                if (null != writer)
                    writer.Close();
            }
            return ssiDbs;
        }
        #endregion Load
        #region Sort
        public bool Sort()
        {
            bool isOk = (Count > 0);
            if (isOk)
                Array.Sort(ssidb, this);
            return isOk;

        }
        #endregion Sort
        #endregion Methods

        #region IComparer Members
        public int Compare(object x, object y)
        {
            //Order by idA,priorityRank
            //si ret = -1, ssiDbX < ssiDbY,  ssiDbX est prioritaire 
            //si ret =  1, ssiDbY < ssiDbX,  ssiDbY est prioritaire 
            SsiDb ssiDbX = (SsiDb)x;
            SsiDb ssiDbY = (SsiDb)y;

            int ret = (ssiDbX.idAStlOffice - ssiDbY.idAStlOffice);
            if (ret == 0)
                ret = (ssiDbX.priorityRank - ssiDbY.priorityRank);
            return ret;
        }
        #endregion IComparer Members
    }
    #endregion
    #region SsiDb
    public partial class SsiDb
    {
        #region Accessor
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EfsML.Enum.StandInstDbType DbTypeEnum
        {
            get
            {
                EfsML.Enum.StandInstDbType dbTypeEnum = EfsML.Enum.StandInstDbType.Other;
                EfsML.Enum.StandInstDbType ret = EfsML.Enum.StandInstDbType.Other;
                //
                FieldInfo[] flds = dbTypeEnum.GetType().GetFields();
                foreach (FieldInfo fld in flds)
                {
                    object[] attributes = fld.GetCustomAttributes(typeof(XmlEnumAttribute), true);
                    if ((0 != attributes.GetLength(0)) && (dbType == ((XmlEnumAttribute)attributes[0]).Name))
                    {
                        ret = (EfsML.Enum.StandInstDbType)fld.GetValue(dbTypeEnum);
                        break;
                    }
                }
                return ret;
            }
        }
        public bool IsLocalDatabase
        {
            get { return (url == SettlementCst.SSILocalDatabase); }
        }
        #endregion Accessor
    }
    #endregion SsiDb

}
