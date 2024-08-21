<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt">
<!--  
=============================================================================================
Summary : Trade input - Spheres standard mapping
          
File    : LSD_TradeImport_Map.xsl
=============================================================================================

RD 20170418 [23080] Set Mandatory Cci "otherPartyPayment1_payer" and "otherPartyPayment1_receiver"
  
FI 20161005 [XXXXX] Modify 

FI 20160929 [22507] Modify 
Description: 

FI 20160907 [21831]
Description: Mode UpdateFeesOnly (Mise à jour des frais) 

FI 20160829 [22412]
Description: ClearingTemplate

FI 20160502 [22107]
Description: Add RelatedPositionID  (RPID)

FI 20140408 [19851]
Description: Suppression des modifications liées au ticket 19597

RD 20140408 [19597]
Description:
When the trade is from a Gateway flows and the value of TrdTyp greater then or equal to 1000, then:
  - set TrdTyp value to 0 (RegularTrade) 
  - put the original value in SecondaryTrdTyp (TrdTyp2)

RD 20140408 [19816]
Description:
Use templates
  - ovrETD_SQLMarket                instead of ETD_SQLMarket
  - ovrSQLDerivativeContract        instead of SQLDerivativeContract
  - ovrSQLDealerActorAndBook        instead of SQLActorAndBook
  - ovrSQLClearingOrganization      instead of existing code
  - ovrSQLClearingOrganizationBook  instead of SQLActorAndBook

RD 20140211 [19591]
Description:
- Le Cci "_FIXML_TrdCapRpt_RptSide_SesID" n'est plus alimenté

RD 20140207 [19591]
Description:
- Correction de l'alimentation des cci "_FIXML_TrdCapRpt_RptSide_InptSrc" et "_FIXML_TrdCapRpt_RptSide_SesID"

FI 20140204 [19566]
Description:
- gestion des UTI sur les trades ALLOC

RD 20140120 [19513]
Description: 
- suppression d'une lettre superflue "s" sur le paramètre "pAssetCode" à l'appel du template ETD_SQLInstrumentAndAsset 
pour valoriser le paramètre de l'import "instrumentIdentifier"

FI 20140103 [19433]
Description: 
- Modification de la requête dans SQLCheckTrade

FI 20131126 [19271] 
Description:
- Gestion de la date de maturité de l'asset

FI 20131122 [19233]
Description:
- Modification dans le log, les messagent commencent systématématiquement avec LogRowDesc

FI 20130703 [18798]
Description:
- Gestion du paramètre ALLOWDATAMISSING

FI 20130702 [18798]
Description:
- Alimentation des status d'activation, d'environnement et de priorité 

FI 20130528 [18662]
Description:
- Alimentation de l'attribut action type du noeud importMode
  action type est une donnée qui devient accessible côté c#

FI 20130521 [18672] 
Description:
- Spheres limite la recherche aux trades dont date de compensation est identique à la date de compensation en entrée.
- Suppression des template GetParamDate et SQLWhere

version 3.0.0 (Spheres 3.3)
FI 20130327 [] 
Description:
- include ImportTools.xsl

version 2.1.0 (Spheres 3.3)
FI 20130301 [18465] 
Description:
- Possibilité de choisir un identifier pour le trade

version 2.0.0 (Spheres 3.2)
FI 20130222 [18439] 
Description:
 - Les cci MARKET doi être alimenté avec la colonne FIXML_SECURITYEXCHANGE et non plus l'identifier

version 1.8.0 (Spheres 3.2)
FI 20130213 [18416] 
Description:
  - Gestion de SpheresSecurityExchange pour l'identification des marchés   
  - Les select sur MARKET sont remplacés par des select sur VW_MARKET_IDENTIFIER

version 1.7.0 (Spheres 3.1)
  Date: 25/01/2013 [Ticket 18363]
  Author: FI
  Description:
  - Prise en compte de l'action 'G'
  - 'G' permmet d'insérer des trades Give-up.
  - 'G' génère une modification du trade s'il existe déjà et s'il ne contient pas de giveupClearingFirm
  - 'G' génère une création si le trade n'existe pas
  - 'G' n'effectue aucune action si le trade existe et qu'il contient un giveupClearingFirm
  - Refactoring

version 1.6.0(Spheres 3.1)
  Date: 16/01/2013 [Ticket 18363]
  Author: BD
  Description:
  - Prise en compte de l'action 'M'

version 1.5.1(Spheres 3.1)
  Date: 16/01/2013 [Ticket 18343]
  Author: FI
  Description:
  - Appel au template SQLEnumValue2 à la place de SQLEnumValue
  SQLEnumValue2 fait appel à une fonction SpheresLib qui évite l'appel à un select systématiquement 
  
  
  Version 1.5.0 (Spheres 3.1)
  Date: 14/01/2013 [Ticket 18343] 
  Author: RD
  Description: Corrections de bugs rencontrés lors de l'analyse de l'importation des trades issus d'une gateway
  - Correction de l'alimentation du Cci TrdCapRpt_RptSide_InptDev "ALID" -> "ALD"
  - ALimentation du Cci TrdCapRpt_TrdID par "ALID" 

  Version 1.4.1 (Spheres 3.0)
  Date: 24/12/2012 [Ticket 18323]
  Author: RD
  Description:
  Importation de trades en annulation: correction en cas d'existence de plusieurs trades:
  - avec le même ExtlLink
  - avec des statuts différents (DEACTIV, REGULAR, ...).

  Version 1.4.0 (Spheres 3.0)
  Date: 07/12/2012 [Ticket 18240]
  Author: RD
  Description:
  - Importation de trades avec des données destinées à l'alimentation des "extensions" en vigueur sur le produit concerné.

  Version 1.3.1 (Spheres 2.7.0.0)
  Date: 31/10/2012 [Ticket 18213]
  add xsl:variable vSpheresValidationXSDLeg1 and vSpheresValidationRulesLeg1
  
  Version 1.3.0 (Spheres 2.6.4.0)
  Date: 09/07/2012 [Ticket 18006] 
  Author: MF
  Description: 
  - nouvelles données :
        AllocationSecondaryType ALT2
        
  Version 1.3.1
  Date: 09/07/2012 [17991] [17993]
  Author: FI
  Description: integration des trades equitySecurityTransaction
      
  Version 1.2.1 (Spheres 2.6.2.0)
  Date: 09/05/2012
  Author: MF
  Description: 
  - nouvelles données :
        OrderElectCategory
        OrderElectStatus
        ExecutionType
        AllocationType
        AllocationSubType
        AllocationInputSource
  - Modification codes :
        ExecutionId: EXI->EXID
        AllocationId: ALI->ALID
        AllocationQuantity: ALQT->ALQ
        PositionEffect: PSE->ALPE
        OrderElectId: OEI->OEID
        OrderElectType: OETP->OET

  Version 1.2.0 (Spheres 2.6.1.0)
  Date: 26/03/2012
  Author: RD
  Description:
  - Intégration des trades avec le statut d'activation "Incomplet", dans le cas d'un Book du Dealer manquant.
  - Gérer la suppression des Trades (ça rvient à Annuler le trade)

  Version 1.1.3 (Spheres 2.6.0.0)
  Date: 30/01/2012
  Author: RD
  Description: Gérer l’utilisation de données identifiables via la colonne EXTLLINK2.

  Version 1.1.2 (Spheres 2.6.0.0)
  Date: 16/09/2011 [17542]
  Author: FI
  Description: Correction dans le template SQLJoinExtlId, où il manquait un espace avant "inner join EXTLID"

  Version 1.1.2 (Spheres 2.5.1.0)
  Date: 04/04/2011
  Author: RD
  Description: Gérer le chargement du DerivativeContract en utilisant Market et Category

  Version 1.1.1 (Spheres 2.5.1.0)
  Date: 26/01/2011
  Author: RD
  Description: Gérer le statut DEACTIV des Trades

  Version 1.1.0 (Spheres 2.5.0.0)
  Date: 19/01/2011
  Author: RD
  Description:
  - Gérer la modification des Trades
  - Gérer la valorisation automatique du NegociationBroker:
  * avec l'Entity Broker qui gère le book,
  * pour les Alloc
  * s'il n'est pas présent dans le fichier source

  Version 1.0.2 (Spheres 2.4.1.2)
  Date: 19/08/2010
  Author: RD
  Description: Dans le template CustomCaptureInfo, ajout de l'attribut "mandatory" sur les Cci

  Version 1.0.1 (Spheres 2.4.1.0)
  Date: 17/08/2010
  Author: RD
  Description: Correction dans le template SQLJoinExtlId, où il manquait un espace avant le where

  Version 1.0.0 (Spheres 2.4.1.0)
  Date: 09/07/2010
  Author: RD
  Description: first version

  ================================================================================================================
  -->

  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml;"/>

  <!-- ================================================== -->
  <!--        include(s)                                  -->
  <!-- ================================================== -->
  <xsl:include href="..\..\Common\ImportTools.xsl"/>

  <!-- ================================================== -->
  <!--        Global Constantes                           -->
  <!-- ================================================== -->
  <xsl:variable name ="ConstPartyNumber1">1</xsl:variable>
  <xsl:variable name ="ConstPartyNumber2">2</xsl:variable>
  <xsl:variable name ="ConstStrategy" select ="'strategy_'"/>

  <!-- ================================================== -->
  <!--        Global Variables                            -->
  <!-- ================================================== -->
  <!--S'il existe des paramètres => la tâche IO d'importation de trade est exécutée suite à une demande manuellement (RunIo.aspx)-->
  <!--S'il n'existe pas de paramètre => la tâche IO d'importation de trade est exécutée suite à une demande gateway-->
  <xsl:variable name ="gIsModegateway">
    <xsl:choose>
      <!-- FI 20240118 [WI817] use of parameter ISCREATEDBY_GATEWAY -->
      <!-- FI 20130701 [18798] utilisation de la variable $gIsExistParameters-->
      <!--<xsl:when test="gIsExistParameters = $ConstTrue">-->
      <xsl:when test="/iotask/parameters/parameter[@id='ISCREATEDBY_GATEWAY']=$ConstTrue">
        <xsl:value-of select="$ConstTrue"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$ConstFalse"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- 
  Prise en compte en priorité du paramètre ALLOWDATAMISSDEALER 
    Par défaut, en mode Gateway ALLOWDATAMISSDEALER => 'Allowed'
    Par défaut, en mode Manuel  ALLOWDATAMISSDEALER => 'NotAllowed' 
  
  Important: En mode Gateway, cette donnée n'est pas issue d'un "paramètre", mais d'une "donnée" (DATA).
  -->
  <xsl:variable name ="gIsAllowDataMissDealer">
    <xsl:choose>
      <xsl:when test="/iotask/parameters/parameter[@id='ALLOWDATAMISSDEALER']='Allowed'">
        <xsl:value-of select="$ConstTrue"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$gIsModegateway"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- 
  FI 20130703 [18798]
  Prise en compte en priorité du paramètre ALLOWDATAMISSING 
    Par défaut, en mode Gateway ALLOWDATAMISSING => 'Allowed'
    Par défaut, en mode Manuel  ALLOWDATAMISSING => 'NotAllowed' 
  
  Important: En mode Gateway, cette donnée n'est pas issue d'un "paramètre", mais d'une "donnée" (DATA).
  -->
  <xsl:variable name ="gIsAllowDataMissing">
    <xsl:choose>
      <xsl:when test="/iotask/parameters/parameter[@id='ALLOWDATAMISSING']='Allowed'">
        <xsl:value-of select="$ConstTrue"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$gIsModegateway"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>


  <!-- FI 20130301 [18465]-->
  <!-- Mode de détermination de l'identifier du trade -->
  <!-- "Default" => L'identifier du trade est calculé par Spheres®   -->
  <!-- "TradeId" => L'identifier du trade est TradeId -->
  <xsl:variable name ="gIdentifierSource">
    <xsl:value-of select="/iotask/parameters/parameter[@id='IDENTIFIERSRC']"/>
  </xsl:variable>



  <!-- ================================================== -->
  <!--        Templates match                             -->
  <!-- ================================================== -->
  <!-- Main template -->
  <xsl:template match="/iotask">
    <iotask id="{@id}" name="{@name}" displayname="{@displayname}" loglevel="{@loglevel}" commitmode="{@commitmode}">
      <xsl:apply-templates select="parameters"/>
      <xsl:apply-templates select="iotaskdet"/>
    </iotask>
  </xsl:template>
  <!-- //// -->
  <!-- Details templates -->
  <xsl:template match="parameters">
    <parameters>
      <xsl:for-each select="parameter" >
        <parameter id="{@id}" name="{@name}" displayname="{@displayname}" direction="{@direction}" datatype="{@datatype}">
          <xsl:value-of select="."/>
        </parameter>
      </xsl:for-each>
    </parameters>
  </xsl:template>
  <xsl:template match="iotaskdet">
    <iotaskdet id="{@id}" loglevel="{@loglevel}" commitmode="{@commitmode}">
      <xsl:apply-templates select="ioinput"/>
    </iotaskdet>
  </xsl:template>
  <xsl:template match="ioinput">
    <ioinput id="{@id}" name="{@name}" displayname="{@displayname}" loglevel="{@loglevel}" commitmode="{@commitmode}">
      <xsl:apply-templates select="file"/>
    </ioinput>
  </xsl:template>
  <xsl:template match="file">
    <file name="{@name}" folder="{@folder}" date="{@date}" size="{@size}">
      <!-- Start ETD Trade process -->
      <!-- RD Glop-->
      <!-- Reste à traiter les Row de type Header et Footer -->
      <!-- On traite d'abord tous les Row de type Allocation -->
      <xsl:apply-templates select="row[@status='success' and data[@name='RTP']='A']"/>
      <!-- Ensuite, On traite tous les Row de type NON Allocation et avec LegSequenceNumber = 1 -->
      <xsl:apply-templates select="row[@status='success' and data[@name='RTP']!='A' and data[@name='LSN']=1]"/>
    </file>
  </xsl:template>
  <!-- //// -->
  <!-- ================================================================= 
        In Strategy case: Process each Row with SequenceLegNumber = 1 
                          and his attached Rows with SequenceLegNumber>1 
       ================================================================= -->
  <xsl:template match="row">
    <xsl:variable name ="vId" select ="normalize-space(@id)"/>
    <xsl:variable name ="vSrc" select ="normalize-space(@src)"/>
    <xsl:variable name ="vRecordTypeLeg1" select ="normalize-space(data[@name='RTP'])"/>
    <xsl:variable name="vRecordTypeLog">
      <xsl:call-template name="GetRecordTypeLog">
        <xsl:with-param name="pRecordType" select="$vRecordTypeLeg1"/>
      </xsl:call-template>
    </xsl:variable>

    <!-- FI 20130702 [18798] add variable vStBusiness, vStActivation, vStEnvironment, vStPriority -->
    <xsl:variable name ="vStBusiness" >
      <xsl:call-template name="GetStBusiness">
        <xsl:with-param name="pRecordType">
          <xsl:value-of select="$vRecordTypeLeg1" />
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    <!-- FI 20130703 [18798] Il faudra modifier le fichier standard pour y introduire ces données -->
    <!-- Pour l'instant ces données sont renseignées à partir du template -->
    <xsl:variable name ="vStActivation" select ="normalize-space(data[@name='STACT'])"/>
    <xsl:variable name ="vStEnvironment" select ="normalize-space(data[@name='STENV'])"/>
    <xsl:variable name ="vStPriority" select ="normalize-space(data[@name='STPRY'])"/>

    <xsl:variable name ="isAllowDataMissDealer">
      <xsl:choose>
        <xsl:when test="data[@name='ALLOWDATAMISSDEALER']">
          <!-- S'il existe une data ALLOWDATAMISSDEALER Spheres® prend en considération sa valeur -->
          <xsl:choose>
            <xsl:when test="data[@name='ALLOWDATAMISSDEALER']='Allowed'">
              <xsl:value-of select="$ConstTrue"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$ConstFalse"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$gIsAllowDataMissDealer"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- FI 20130703 [18798] -->
    <xsl:variable name ="isAllowDataMissing">
      <xsl:choose>
        <xsl:when test="data[@name='ALLOWDATAMISSING']">
          <!-- S'il existe une data ALLOWDATAMISSING Spheres® prend en considération sa valeur -->
          <!-- sinon Spheres® utilise la valeur par défaut présente dans $gIsAllowDataMissing -->
          <xsl:choose>
            <xsl:when test="data[@name='ALLOWDATAMISSING']='Allowed'">
              <xsl:value-of select="$ConstTrue"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$ConstFalse"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$gIsAllowDataMissing"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!--///-->
    <xsl:variable name ="vStrategyTypeLeg1">
      <xsl:if test="$vRecordTypeLeg1 != 'A'">
        <xsl:value-of select ="normalize-space(data[@name='STA'])"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name ="vTotalNumberOfLegsLeg1">
      <xsl:if test="$vRecordTypeLeg1 != 'A'">
        <xsl:value-of select ="normalize-space(data[@name='TNL'])"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name ="vIsStrategyLeg1">
      <xsl:choose>
        <xsl:when test="$vRecordTypeLeg1 != 'A' and (string-length($vStrategyTypeLeg1) > 0) and $vStrategyTypeLeg1 != 'NONE'">
          <xsl:value-of select ="$ConstTrue"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$ConstFalse"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name ="vCciPrefixStrategy">
      <xsl:if test="$vIsStrategyLeg1 = $ConstTrue">
        <xsl:value-of select="$ConstStrategy"/>
      </xsl:if>
    </xsl:variable>

    <!--///-->
    <xsl:variable name ="vTradeIdLeg1" select ="normalize-space(data[@name='TRID'])"/>
    <xsl:variable name ="vAllTradeRows" select ="$gAllRows/row[data[@name='TRID']=$vTradeIdLeg1]"/>
    <!--///-->
    <xsl:variable name ="vActionTypeLeg1" select ="normalize-space(data[@name='ATP'])"/>
    <xsl:variable name ="vActionTypeLog">
      <xsl:call-template name="GetActionTypeLog">
        <xsl:with-param name="pActionType" select="$vActionTypeLeg1"/>
      </xsl:call-template>
    </xsl:variable>
    <!--///-->
    <xsl:variable name ="vSpheresTemplateIdentifierLeg1" select ="normalize-space(data[@name='TPID'])"/>
    <xsl:variable name ="vSpheresScreenLeg1" select ="normalize-space(data[@name='SCR'])"/>
    <xsl:variable name ="vSpheresFeeCalculationLeg1">
      <!--PL 20130718 FeeCalculation Project-->
      <!--<xsl:call-template name="IsToApplyOrToIgnore">
        <xsl:with-param name="pApplyIgnore">
          <xsl:value-of select="normalize-space(data[@name='SFC'])" />
        </xsl:with-param>
      </xsl:call-template>-->
      <xsl:value-of select="normalize-space(data[@name='SFC'])" />
    </xsl:variable>
    <!-- FI 20160829 [22412] Modify -->
    <!-- RD 20170127 [22779] Use  $ConstTrue and $ConstFalse instead of 'true' and 'false'-->
    <xsl:variable name ="vSpheresPartyTemplateLeg1">
      <xsl:choose>
        <xsl:when test ="normalize-space(data[@name='SPT'])='Apply'">
          <xsl:value-of select="$ConstTrue"/>
        </xsl:when>
        <xsl:when test ="normalize-space(data[@name='SPT'])='ApplyParty'">
          <xsl:value-of select="$ConstTrue"/>
        </xsl:when>
        <xsl:when test ="normalize-space(data[@name='SPT'])='ApplyAll'">
          <xsl:value-of select="$ConstTrue"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$ConstFalse"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- FI 20160829 [22412] Add vSpheresClearingTemplateLeg1 -->
    <!-- RD 20170127 [22779] Use  $ConstTrue and $ConstFalse instead of 'true' and 'false'-->
    <xsl:variable name ="vSpheresClearingTemplateLeg1">
      <xsl:choose>
        <xsl:when test ="normalize-space(data[@name='SPT'])='ApplyBroker'">
          <xsl:value-of select="$ConstTrue"/>
        </xsl:when>
        <xsl:when test ="normalize-space(data[@name='SPT'])='ApplyAll'">
          <xsl:value-of select="$ConstTrue"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$ConstFalse"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name ="vSpheresValidationXSDLeg1">
      <xsl:call-template name="IsToApplyOrToIgnore">
        <xsl:with-param name="pApplyIgnore">
          <xsl:value-of select="normalize-space(data[@name='SVX'])" />
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vSpheresValidationRulesLeg1">
      <xsl:call-template name="IsToApplyOrToIgnore">
        <xsl:with-param name="pApplyIgnore">
          <xsl:value-of select="normalize-space(data[@name='SVR'])" />
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vSpheresDisplaynameLeg1" select ="normalize-space(data[@name='DSP'])"/>
    <xsl:variable name ="vSpheresDescriptionLeg1" select ="normalize-space(data[@name='DSC'])"/>
    <xsl:variable name ="vSessionLeg1" select ="normalize-space(data[@name='SSS'])"/>
    <xsl:variable name ="vCommentLeg1" select ="normalize-space(data[@name='CMT'])"/>
    <!--///-->
    <xsl:variable name ="vTransactionDateLeg1" select ="normalize-space(data[@name='TDT'])"/>
    <xsl:variable name ="vClearingBusinessDateLeg1" select ="normalize-space(data[@name='CDT'])"/>
    <!--///-->
    <xsl:variable name ="vAssetCodeLeg1" select ="normalize-space(data[@name='ASC'])"/>
    <xsl:variable name ="vAssetCodeIdentLeg1" select ="normalize-space(data[@name='ASCI'])"/>
    <xsl:variable name ="vAssetCodeColumnLeg1">
      <xsl:call-template name="GetAssetIdentColumn">
        <xsl:with-param name="pIdent">
          <xsl:value-of select="$vAssetCodeIdentLeg1"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <!--///-->
    <!-- vMarketLeg1 contient la valeur qui identifie le marché-->
    <xsl:variable name ="vMarketLeg1" select ="normalize-space(data[@name='MKT'])"/>
    <!-- vMarketColumnLeg1 contient la colonne MARKET en fonction du type d'identification-->
    <xsl:variable name ="vMarketColumnLeg1">
      <xsl:call-template name="GetMarketIdentColumn">
        <xsl:with-param name="pIdent">
          <xsl:value-of select="normalize-space(data[@name='MKTI'])"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <!-- FI 20170404 [23039] Mise en commentaire -->
    <!--<xsl:variable name ="vMarketForAssetCodeIdentMARKET">
      <xsl:if test="$vAssetCodeIdent = 'MARKET'">
        <xsl:value-of select="$vMarket"/>
      </xsl:if>
    </xsl:variable>-->

    <!--///-->
    <xsl:variable name ="vDerivativeContractLeg1" select ="normalize-space(data[@name='DVT'])"/>
    <xsl:variable name ="vDerivativeContractColumnLeg1">
      <xsl:call-template name="GetDerivativeContractIdentColumn">
        <xsl:with-param name="pIdent">
          <xsl:value-of select="normalize-space(data[@name='DVTI'])"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vDerivativeContractVersionLeg1" select ="normalize-space(data[@name='DVTV'])"/>
    <xsl:variable name ="vDerivativeContractCategoryLeg1" select ="normalize-space(data[@name='DVTC'])"/>

    <!-- FI 20120704 [17991] add vProductLeg1 and vInstrumentLeg1 and vFamilyLeg1 and vCciPrefixFamily -->
    <xsl:variable name ="vProductLeg1" select ="normalize-space(data[@name='FIL1'])"/>
    <xsl:variable name ="vInstrumentLeg1" select ="normalize-space(data[@name='FIL2'])"/>
    <xsl:variable name ="vFamilyLeg1">
      <xsl:choose>
        <xsl:when test="$vProductLeg1='equitySecurityTransaction' or $vProductLeg1='STGequitySecurityTransaction'">
          <xsl:value-of select="'ESE'"/>
        </xsl:when>
        <xsl:otherwise>
          <!--Default value -->
          <xsl:value-of select="'ETD'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name ="vCciPrefixFamily">
      <xsl:choose>
        <xsl:when test="$vFamilyLeg1='ETD'">
          <xsl:value-of select="'exchangeTradedDerivative'"/>
        </xsl:when>
        <xsl:when test="$vFamilyLeg1='ESE'">
          <xsl:value-of select="'equitySecurityTransaction'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'ERROR'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!--Instrument-->
    <!-- L'instrument doit être en entrée (cas normal) et exigé si ESE -->
    <!-- s'il est non renseigné et si famile ETD, Spheres® récupère l'instrument à partir des caractéristiques de l'asset -->
    <xsl:variable name="vInstrumentIdentifier">
      <xsl:choose>
        <!-- FI 20120704 [17991] if Instrument is specified -->
        <xsl:when test="string-length($vInstrumentLeg1) > 0">
          <xsl:value-of select="$vInstrumentLeg1"/>
        </xsl:when>
        
        <!-- Instrument is unknown, Instrument is Strategy -->
        <xsl:when test="$vIsStrategyLeg1 = $ConstTrue">
          <xsl:value-of select="$vStrategyTypeLeg1"/>
        </xsl:when>

        <!-- Instrument is not Strategy and AssetCode is unknown and Family is ETD -->
        <!-- FI 20120704 [17991] only on ETD -->
        <xsl:when test="string-length($vAssetCodeLeg1) = 0 and $vFamilyLeg1 = 'ETD'">
          <xsl:choose>
            <xsl:when test="$vDerivativeContractCategoryLeg1 = 'F'">
              <xsl:value-of select="'ExchangeTradedFuture'"/>
            </xsl:when>
            <xsl:when test="$vDerivativeContractCategoryLeg1 = 'O'">
              <xsl:value-of select="'ExchangeTradedOption'"/>
            </xsl:when>
          </xsl:choose>
        </xsl:when>

        <!-- Instrument is not Strategy and AssetCode is unknown and Family is ETD -->
        <xsl:otherwise>
          <!-- FI 20120704 [17991] only on ETD -->
          <xsl:if test="$vFamilyLeg1 = 'ETD'">
            <xsl:value-of select="$ConstUseSQL"/>
          </xsl:if>
        </xsl:otherwise>

      </xsl:choose>
    </xsl:variable>
    <!-- RD 20121204 [18240] Import: Trades with data for extensions -->
    <xsl:variable name ="vXMLExtends">
      <xsl:choose>
        <!-- Instrument is Strategy -->
        <xsl:when test="$vIsStrategyLeg1 = $ConstTrue">
          <xsl:for-each select ="$vAllTradeRows">
            <xsl:call-template name="XMLExtends">
              <xsl:with-param name="pXMLRow">
                <xsl:copy-of select ="."/>
              </xsl:with-param>
            </xsl:call-template>
          </xsl:for-each >
        </xsl:when>
        <!-- Instrument is NOT Strategy -->
        <xsl:otherwise>
          <xsl:call-template name="XMLExtends">
            <xsl:with-param name="pXMLRow">
              <xsl:copy-of select ="."/>
            </xsl:with-param>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- FI 20131122 [19233] add vRowInfo -->
    <xsl:variable name ="vLogRowMsg">
      <xsl:call-template name ="LogRowInfo">
        <xsl:with-param name ="pActionTypeLog" select ="$vActionTypeLog"/>
        <xsl:with-param name ="pRecordTypeLog"  select ="$vRecordTypeLog"/>
        <xsl:with-param name ="pTradeIdLeg1" select ="$vTradeIdLeg1"/>
        <xsl:with-param name ="pStrategyTypeLeg1" select ="$vStrategyTypeLeg1"/>
        <xsl:with-param name ="pTNL" select ="data[@name='TNL']"/>
      </xsl:call-template>
    </xsl:variable>

    <!-- FI 20131122 [19233] add vLogRowDesc -->
    <xsl:variable name ="vLogRowDesc">
      <xsl:call-template name ="LogRowDesc">
        <xsl:with-param name ="pRowId" select ="$vId"/>
        <xsl:with-param name ="pRowSrc"  select ="$vSrc"/>
      </xsl:call-template>
    </xsl:variable>

    <row id="{$vId}" src="{$vSrc}">
      <logInfo>
        <message>
          <xsl:value-of select ="$vLogRowMsg"/>
        </message>
      </logInfo>
      <!--///-->
      <tradeImport xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
        <settings>
          <importMode >
            <xsl:attribute name ="actionType" >
              <xsl:value-of select ="$vActionTypeLeg1"/>
            </xsl:attribute>
            <xsl:choose>
              <xsl:when test="$vActionTypeLeg1 = 'N' or $vActionTypeLeg1 = 'M' or $vActionTypeLeg1 = 'G'">
                <!-- l'action [N]ew peut peut générer un Update ou New -->
                <!-- l'action [M]odify peut peut générer un Update ou New -->
                <xsl:call-template name="SQLCheckTrade">
                  <xsl:with-param name="pActionType" select="$vActionTypeLeg1"/>
                  <xsl:with-param name="pRecordType" select="$vRecordTypeLeg1"/>
                  <xsl:with-param name="pTradeId" select="$vTradeIdLeg1"/>
                  <xsl:with-param name="pResultColumn" select="'IMPORTMODE'"/>
                  <xsl:with-param name="pClearingBusinessDate" select="$vClearingBusinessDateLeg1"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name="GetDefaultImportMode">
                  <xsl:with-param name="pActionType" select="$vActionTypeLeg1"/>
                  <xsl:with-param name="pRecordType" select="$vRecordTypeLeg1"/>
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </importMode>
          <!-- ================================================== -->
          <!--                CONDITIONS SECTION                  -->
          <!-- ================================================== -->

          <conditions>
            <!-- FI 20120130 si le tradeId est non spécifié, l'importation termine en erreur -->
            <condition name="IsTradeIdSpecified" datatype="bool">
              <logInfo status="ERROR">
                <code>LOG</code>
                <number>06019</number>
                <data1>
                  <xsl:value-of select="$vLogRowDesc"/>
                </data1>
              </logInfo>
              <xsl:choose>
                <xsl:when test="string-length($vTradeIdLeg1) > 0">
                  <xsl:text>true</xsl:text>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:text>false</xsl:text>
                </xsl:otherwise>
              </xsl:choose>
            </condition>
            <xsl:choose>
              <!-- FI 20160907 [21831] add vActionTypeLeg1 = 'F'-->
              <xsl:when test="$vActionTypeLeg1 = 'N' or $vActionTypeLeg1 = 'G' or 
                              $vActionTypeLeg1 = 'U' or $vActionTypeLeg1 = 'M' or $vActionTypeLeg1 = 'F' or 
                              $vActionTypeLeg1 = 'S'">
                <xsl:variable name ="logNumber">
                  <xsl:choose>
                    <xsl:when test="$vActionTypeLeg1 = 'N' or $vActionTypeLeg1 = 'G'">
                      <xsl:value-of select ="06020" />
                    </xsl:when>
                    <xsl:when test="$vActionTypeLeg1 = 'U' or $vActionTypeLeg1 = 'M' or $vActionTypeLeg1 = 'F' ">
                      <xsl:value-of select ="06021" />
                    </xsl:when>
                    <xsl:when test="$vActionTypeLeg1 = 'S' ">
                      <xsl:value-of select ="06026" />
                    </xsl:when>
                  </xsl:choose>
                </xsl:variable>

                <condition name="IsActionOk" datatype="bool">
                  <!--lors des intégrations gateway, l'importation termine en succes si la condition est non respectée -->
                  <!--lors des intégrations manuelle,l'importation termine en succes/waring si la condition est non respectée -->
                  <xsl:variable name="status">
                    <xsl:choose>
                      <xsl:when test="$gIsModegateway=$ConstTrue">INFO</xsl:when>
                      <xsl:otherwise>WARNING</xsl:otherwise>
                    </xsl:choose>
                  </xsl:variable>
                  <logInfo status="{$status}">
                    <code>LOG</code>
                    <number>
                      <xsl:value-of select="$logNumber"/>
                    </number>
                    <data1>
                      <xsl:value-of select="$vLogRowDesc"/>
                    </data1>
                    <data2>
                      <xsl:value-of select="$vTradeIdLeg1"/>
                    </data2>
                  </logInfo>
                  <xsl:call-template name="SQLCheckTrade">
                    <xsl:with-param name="pActionType" select="$vActionTypeLeg1"/>
                    <xsl:with-param name="pRecordType" select="$vRecordTypeLeg1"/>
                    <xsl:with-param name="pTradeId" select="$vTradeIdLeg1"/>
                    <xsl:with-param name="pResultColumn" select="'ISOK'"/>
                    <xsl:with-param name="pClearingBusinessDate" select="$vClearingBusinessDateLeg1"/>
                  </xsl:call-template>
                </condition>
              </xsl:when>
              <xsl:when test="string-length($vActionTypeLeg1) > 0">
                <condition name="IsActionTypeManaged" datatype="bool">
                  <logInfo status="ERROR">
                    <code>LOG</code>
                    <number>06022</number>
                    <data1>
                      <xsl:value-of select="$vLogRowDesc"/>
                    </data1>
                  </logInfo>
                  <xsl:text>false</xsl:text>
                </condition>
              </xsl:when>
              <xsl:otherwise>
                <condition name="IsActionTypeSpecified" datatype="bool">
                  <logInfo status="ERROR">
                    <code>LOG</code>
                    <number>06023</number>
                    <data1>
                      <xsl:value-of select="$vLogRowDesc"/>
                    </data1>
                    <data2>
                      <xsl:value-of select="$vActionTypeLeg1"/>
                    </data2>
                  </logInfo>
                  <xsl:text>false</xsl:text>
                </condition>
              </xsl:otherwise>
            </xsl:choose>
            <xsl:for-each select='msxsl:node-set($vXMLExtends)/extend'>
              <xsl:variable name ="vExtendPosition" select="position()"/>
              <condition name="IsExtendExist" datatype="bool">
                <logInfo status="ERROR">
                  <xsl:choose>
                    <xsl:when test="$vInstrumentIdentifier = $ConstUseSQL">
                      <code>SYS</code>
                      <number>06009</number>
                      <data1>
                        <xsl:value-of select="$vLogRowDesc"/>
                      </data1>
                      <data2>
                        <xsl:value-of select="@identifier"/>
                      </data2>
                      <data3>
                        <xsl:value-of select="@detail"/>
                      </data3>
                      <data4>
                        <xsl:value-of select="$vAssetCodeLeg1"/>
                        <xsl:if test="$vAssetCodeColumnLeg1 != 'IDENTIFIER'">
                          <xsl:value-of select="concat(' (',$vAssetCodeColumnLeg1,')')"/>
                        </xsl:if>
                      </data4>
                      <data5>
                        <xsl:value-of select="$vTradeIdLeg1"/>
                      </data5>
                    </xsl:when>
                    <xsl:otherwise>
                      <code>SYS</code>
                      <number>06010</number>
                      <data1>
                        <xsl:value-of select="$vLogRowDesc"/>
                      </data1>
                      <data2>
                        <xsl:value-of select="@identifier"/>
                      </data2>
                      <data3>
                        <xsl:value-of select="@detail"/>
                      </data3>
                      <data4>
                        <xsl:value-of select="$vInstrumentIdentifier"/>
                      </data4>
                      <data5>
                        <xsl:value-of select="$vTradeIdLeg1"/>
                      </data5>
                    </xsl:otherwise>
                  </xsl:choose>
                </logInfo>
                <xsl:call-template name="SQLExtendAttribute">
                  <xsl:with-param name="pExtIdentifier" select="@identifier"/>
                  <xsl:with-param name="pExtDet" select="@detail"/>
                  <xsl:with-param name="pResultColumn" select="'ISEXTENDEXIST'"/>
                  <xsl:with-param name="pInstrumentIdentifier" select="$vInstrumentIdentifier"/>
                  <xsl:with-param name="pAssetCode" select="$vAssetCodeLeg1"/>
                  <xsl:with-param name="pAssetColumn" select="$vAssetCodeColumnLeg1"/>
                  <!-- FI 20170404 [23039] select="$vMarket" -->
                  <!--<xsl:with-param name="pMarket" select="$vMarketForAssetCodeIdentMARKET"/>-->
                  <xsl:with-param name="pMarket" select="$vMarketLeg1"/>
                  <xsl:with-param name="pMarketColumn" select="$vMarketColumnLeg1"/>
                  <xsl:with-param name="pClearingBusinessDate" select="$vClearingBusinessDateLeg1"/>
                </xsl:call-template>
              </condition>
            </xsl:for-each>
          </conditions>
          <user>SYSADM</user>
          <!-- ================================================== -->
          <!--                PARAMETERS SECTION                  -->
          <!-- ================================================== -->
          <parameters>
            <parameter name="http://www.efs.org/otcml/tradeImport/instrumentIdentifier" datatype="string">
              <xsl:choose>
                <xsl:when test="$vInstrumentIdentifier = $ConstUseSQL">
                  <xsl:call-template name="ETD_SQLInstrumentAndAsset">
                    <xsl:with-param name="pBusinessDate" select="$vClearingBusinessDateLeg1"/>
                    <xsl:with-param name="pAssetCode" select="$vAssetCodeLeg1"/>
                    <xsl:with-param name="pAssetColumn" select="$vAssetCodeColumnLeg1"/>
                    <!-- FI 20170404 [23039] select="$vMarket" -->
                    <!--<xsl:with-param name="pMarket" select="$vMarketForAssetCodeIdentMARKET"/>-->
                    <xsl:with-param name="pMarket" select="$vMarketLeg1"/>
                    <xsl:with-param name="pMarketColumn" select="$vMarketColumnLeg1"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$vInstrumentIdentifier"/>
                </xsl:otherwise>
              </xsl:choose>
            </parameter>
            <!-- template from instrument default -->
            <parameter name="http://www.efs.org/otcml/tradeImport/templateIdentifier" datatype="string">
              <xsl:value-of select="$vSpheresTemplateIdentifierLeg1"/>
            </parameter>
            <parameter name="http://www.efs.org/otcml/tradeImport/screen" datatype="string">
              <xsl:value-of select="$vSpheresScreenLeg1"/>
            </parameter>
            <!--/////-->
            <parameter name="http://www.efs.org/otcml/tradeImport/identifier" datatype="string">
              <!-- FI 20130301 [18465] appel systématique à SQLTradeIdentifier  -->
              <xsl:call-template name="SQLTradeIdentifier">
                <xsl:with-param name="pActionType" select="$vActionTypeLeg1"/>
                <xsl:with-param name="pTradeId" select="$vTradeIdLeg1"/>
                <xsl:with-param name="pResultColumn" select="'IDENTIFIER'"/>
                <xsl:with-param name="pClearingBusinessDate" select="$vClearingBusinessDateLeg1"/>
              </xsl:call-template>
            </parameter>
            <parameter name="http://www.efs.org/otcml/tradeImport/displayname" datatype="string">
              <xsl:value-of select="$vSpheresDisplaynameLeg1"/>
            </parameter>
            <parameter name="http://www.efs.org/otcml/tradeImport/description" datatype="string">
              <xsl:value-of select="$vSpheresDescriptionLeg1"/>
            </parameter>
            <parameter name="http://www.efs.org/otcml/tradeImport/extllink" datatype="string">
              <xsl:value-of select="$vTradeIdLeg1"/>
            </parameter>
            <!--/////-->
            <!--PL 20130718 FeeCalculation Project-->
            <!--<parameter name="http://www.efs.org/otcml/tradeImport/isApplyFeeCalculation" datatype="bool">
              <xsl:value-of select="$vSpheresFeeCalculationLeg1"/>
            </parameter>-->
            <parameter name="http://www.efs.org/otcml/tradeImport/feeCalculation" datatype="string">
              <xsl:value-of select="$vSpheresFeeCalculationLeg1"/>
            </parameter>
            <parameter name="http://www.efs.org/otcml/tradeImport/isApplyPartyTemplate" datatype="bool">
              <xsl:value-of select="$vSpheresPartyTemplateLeg1"/>
            </parameter>
            <!-- FI 20160829 [22412] Add parameter isApplyClearingTemplate -->
            <parameter name="http://www.efs.org/otcml/tradeImport/isApplyClearingTemplate" datatype="bool">
              <xsl:value-of select="$vSpheresClearingTemplateLeg1"/>
            </parameter>
            <parameter name="http://www.efs.org/otcml/tradeImport/isPostToEventsGen" datatype="bool">true</parameter>
            <xsl:if test="string-length($vSpheresValidationXSDLeg1) > 0">
              <parameter name="http://www.efs.org/otcml/tradeImport/isApplyValidationXSD" datatype="bool">
                <xsl:value-of select="$vSpheresValidationXSDLeg1"/>
              </parameter>
            </xsl:if>
            <xsl:if test="string-length($vSpheresValidationRulesLeg1) > 0">
              <parameter name="http://www.efs.org/otcml/tradeImport/isApplyValidationRules" datatype="bool">
                <xsl:value-of select="$vSpheresValidationRulesLeg1"/>
              </parameter>
            </xsl:if>
            <!-- FI 20160907 [21831] Add UpdateMode Parameter -->
            <xsl:if test ="$vActionTypeLeg1 = 'F'">
              <parameter name="http://www.efs.org/otcml/tradeImport/UpdateMode" datatype="string">UpdateFeesOnly</parameter>
            </xsl:if>
            <!-- FI 20160929 [22507] Add -->
            <xsl:if test ="string-length(normalize-space(data[@name='ISNCMACTIVITY']))>0">
              <parameter name="http://www.efs.org/otcml/tradeImport/isApplyReverseClearingTemplate" datatype="bool">
                <xsl:value-of select ="normalize-space(data[@name='ISNCMACTIVITY'])"/>
              </parameter>
            </xsl:if>
          </parameters>
        </settings>

        <tradeInput>
          <!-- ================================================== -->
          <!--                TRADESTATUS SECTION                 -->
          <!-- ================================================== -->
          <tradeStatus>
            <stBusiness>
              <xsl:value-of select ="$vStBusiness"/>
            </stBusiness>
            <!-- FI 20130702 [18798] stActivation -->
            <xsl:if test="string-length($vStActivation) > 0">
              <stActivation>
                <xsl:value-of select ="$vStActivation"/>
              </stActivation>
            </xsl:if>
            <!-- FI 20130702 [18798] stEnvironment -->
            <xsl:if test="string-length($vStEnvironment) > 0">
              <stEnvironment>
                <xsl:value-of select ="$vStEnvironment"/>
              </stEnvironment>
            </xsl:if>
            <!-- FI 20130702 [18798] vStPriority -->
            <xsl:if test="string-length($vStPriority) > 0">
              <stPriority>
                <xsl:value-of select ="$vStPriority"/>
              </stPriority>
            </xsl:if>
          </tradeStatus>

          <!-- ================================================== -->
          <!--                CCI SECTION                         -->
          <!-- ================================================== -->
          <customCaptureInfos>
            <!-- ======================================================================================= -->
            <!--                                   Common Data SECTION                                   -->
            <!-- ======================================================================================= -->
            <!--Market Cci-->
            <xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId">
                <xsl:value-of select ="concat($vCciPrefixStrategy,$vCciPrefixFamily,'_FIXML_TrdCapRpt_Instrmt_Exch')"/>
              </xsl:with-param>
              <xsl:with-param name="pDataType">
                <xsl:value-of select ="'string'"/>
              </xsl:with-param>
              <xsl:with-param name="pXMLDynamicValue">
                <xsl:choose>
                  <!-- FI 20120704 [17991] use template for ETD-->
                  <!-- RD 20140409 [19816] use template ovrETD_SQLMarket-->
                  <!-- FI 20140418 [19562] Alimentation du paramètre pMarketColumn-->
                  <xsl:when test="$vFamilyLeg1='ETD'">
                    <!-- FI 20161005 [XXXXX] Call ETD_SQLMarket -->
                    <!--<xsl:call-template name="ovrETD_SQLMarket">-->
                    <xsl:call-template name="ETD_SQLMarket">
                      <xsl:with-param name="pBusinessDate" select="$vClearingBusinessDateLeg1"/>
                      <xsl:with-param name="pMarket" select="$vMarketLeg1"/>
                      <!--<xsl:with-param name="pMarketColumn" select="vMarketColumnLeg1"/>-->
                      <xsl:with-param name="pMarketColumn" select="$vMarketColumnLeg1"/>
                      <xsl:with-param name="pAssetCode" select="$vAssetCodeLeg1"/>
                      <xsl:with-param name="pAssetColumn" select="$vAssetCodeColumnLeg1"/>
                      <xsl:with-param name="pDerivativeContract" select="$vDerivativeContractLeg1"/>
                      <xsl:with-param name="pDerivativeContractColumn" select="$vDerivativeContractColumnLeg1"/>
                      <xsl:with-param name="pDerivativeContractVersion" select="$vDerivativeContractVersionLeg1"/>
                      <!-- FI 20170404 [23039] le paramètre se nomme pDerivativeContractCategory -->
                      <xsl:with-param name="pDerivativeContractCategory" select="$vDerivativeContractCategoryLeg1"/>
                    </xsl:call-template>
                  </xsl:when>
                  <!-- FI 20120704 [17991] use template for ESE-->
                  <xsl:when test="$vFamilyLeg1='ESE'">
                    <xsl:call-template name="SQLMarket">
                      <xsl:with-param name="pMarket">
                        <xsl:value-of select="$vMarketLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name="pMarketColumn">
                        <xsl:value-of select="$vMarketColumnLeg1"/>
                      </xsl:with-param>
                    </xsl:call-template>
                  </xsl:when>
                </xsl:choose>
              </xsl:with-param>
              <xsl:with-param name="pIsMissingMode" select ="$isAllowDataMissing"/>
              <xsl:with-param name="pIsMissingModeSpecified" select ="$ConstTrue"/>
              <xsl:with-param name="pIsMandatory" select ="$ConstTrue"/>
            </xsl:call-template>
            <!--Source Cci-->
            <!--<xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId">
                <xsl:value-of select ="concat($vCciPrefixStrategy,$vCciPrefixFamily,'_FIXML_TrdCapRpt_RptSide_SesID')"/>
              </xsl:with-param>
              <xsl:with-param name="pDataType">
                <xsl:value-of select ="'string'"/>
              </xsl:with-param>
              <xsl:with-param name="pValue">
                <xsl:value-of select="$vSessionLeg1"/>
              </xsl:with-param>
            </xsl:call-template>-->
            <!--Comment Cci-->
            <xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId">
                <xsl:value-of select ="concat($vCciPrefixStrategy,$vCciPrefixFamily,'_FIXML_TrdCapRpt_RptSide_Txt')"/>
              </xsl:with-param>
              <xsl:with-param name="pDataType">
                <xsl:value-of select ="'string'"/>
              </xsl:with-param>
              <xsl:with-param name="pValue">
                <xsl:value-of select="$vCommentLeg1"/>
              </xsl:with-param>
            </xsl:call-template>

            <!-- ======================================================================================= -->
            <!--                                   PARTIES SECTION                                       -->
            <!-- ======================================================================================= -->
            <xsl:variable name ="vXMLTradePartiesAndBrokers">
              <xsl:choose>
                <!-- Allocation-->
                <xsl:when test ="$vRecordTypeLeg1='A'">
                  <xsl:variable name ="vBuyerOrSellerAccount" select ="data[@name='BSA']"/>
                  <xsl:variable name ="vBuyerOrSellerAccountIdent" select ="data[@name='BSAI']"/>
                  <!-- RD 20140409 [19816] use template ovrSQLDealerActorAndBook-->
                  <xsl:variable name ="vBuyerOrSellerActor">
                    <xsl:call-template name="ovrSQLDealerActorAndBook">
                      <xsl:with-param name="pBuyerOrSellerAccount" select="$vBuyerOrSellerAccount"/>
                      <xsl:with-param name="pBuyerOrSellerAccountIdent" select="$vBuyerOrSellerAccountIdent"/>
                      <xsl:with-param name="pResultColumn" select="'IDENTIFIER'"/>
                      <xsl:with-param name="pIsAllowDataMissDealer" select ="$isAllowDataMissDealer"/>
                      <xsl:with-param name="pDefaultValue" select ="$ConstActorSYSTEM"/>
                    </xsl:call-template>
                  </xsl:variable>
                  <!-- RD 20140409 [19816] use template ovrSQLDealerActorAndBook-->
                  <xsl:variable name ="vBuyerOrSellerBook">
                    <xsl:call-template name="ovrSQLDealerActorAndBook">
                      <xsl:with-param name="pBuyerOrSellerAccount" select="$vBuyerOrSellerAccount"/>
                      <xsl:with-param name="pBuyerOrSellerAccountIdent" select="$vBuyerOrSellerAccountIdent"/>
                      <xsl:with-param name="pResultColumn" select="'BOOK_IDENTIFIER'"/>
                      <xsl:with-param name="pIsAllowDataMissDealer" select ="$isAllowDataMissDealer"/>
                    </xsl:call-template>
                  </xsl:variable>
                  <!--///-->
                  <xsl:variable name ="vNegociationBroker" select ="data[@name='BSNB']"/>
                  <xsl:variable name ="vNegociationBrokerIdent" select ="data[@name='BSNBI']"/>
                  <xsl:variable name ="vNegociationBrokerActor">
                    <xsl:choose>
                      <!-- Broker présent dans le fichier source-->
                      <xsl:when test ="string-length($vNegociationBroker) > 0">
                        <xsl:call-template name="SQLActor">
                          <xsl:with-param name="pValue">
                            <xsl:value-of select="$vNegociationBroker"/>
                          </xsl:with-param>
                          <xsl:with-param name="pValueIdent">
                            <xsl:value-of select="$vNegociationBrokerIdent"/>
                          </xsl:with-param>
                        </xsl:call-template>
                      </xsl:when>
                      <!--FI 20111124 [17627] Mise en commentaire du pavé xsl:otherwise, la pré-proposition est effectuée en automatique lorsque le book est renseigné
                        voir la méthode CciTradePary.DumpBook_ToDocument
                      -->
                      <!-- Broker non présent dans le fichier source, 
                      alors essayer de proposer l’entité de gestion du book, si cette entité a le role Broker -->
                      <!--
                      <xsl:otherwise>
                        <xsl:call-template name="SQLEntityBrokerOfBook">
                          <xsl:with-param name="pBook">
                            <xsl:value-of select="$vBuyerOrSellerAccount"/>
                          </xsl:with-param>
                          <xsl:with-param name="pBookIdent">
                            <xsl:value-of select="$vBuyerOrSellerAccountIdent"/>
                          </xsl:with-param>
                        </xsl:call-template>
                      </xsl:otherwise>-->
                    </xsl:choose>
                  </xsl:variable>
                  <!--///-->
                  <xsl:variable name ="vClearingOrganization" select ="data[@name='BSCO']"/>
                  <xsl:variable name ="vClearingOrganizationIdent" select ="data[@name='BSCOI']"/>
                  <xsl:variable name ="vClearingOrganizationAccount" select ="data[@name='BSCA']"/>
                  <xsl:variable name ="vClearingOrganizationAccountIdent" select ="data[@name='BSCAI']"/>
                  <!-- RD 20140409 [19816] use template ovrSQLClearingOrganization-->
                  <xsl:variable name ="vClearingOrganizationActor">
                    <!-- FI 20160929 [22507] Chgt de signature du template ovrSQLClearingOrganization -->
                    <!-- FI 20170404 [23039] Chgt de signature du template ovrSQLClearingOrganization ( add pFamily, pMarket, pMarketColumn ) -->
                    <xsl:call-template name="ovrSQLClearingOrganization">
                      <xsl:with-param name="pClearingOrganization" select="$vClearingOrganization"/>
                      <xsl:with-param name="pClearingOrganizationIdent" select="$vClearingOrganizationIdent"/>
                      <xsl:with-param name="pClearingOrganizationAccount" select="$vClearingOrganizationAccount"/>
                      <xsl:with-param name="pClearingOrganizationAccountIdent" select="$vClearingOrganizationAccountIdent"/>
                      <xsl:with-param name="pClearingBusinessDate" select="$vClearingBusinessDateLeg1"/>
                      <xsl:with-param name="pFamily" select ="'ETD'"/>
                      <xsl:with-param name="pMarket" select="$vMarketLeg1"/>
                      <xsl:with-param name="pMarketColumn" select="$vMarketColumnLeg1"/>
                      <xsl:with-param name="pAssetCode" select="$vAssetCodeLeg1"/>
                      <xsl:with-param name="pAssetColumn" select="$vAssetCodeColumnLeg1"/>
                      <xsl:with-param name="pDerivativeContract" select="$vDerivativeContractLeg1"/>
                      <xsl:with-param name="pDerivativeContractColumn" select="$vDerivativeContractColumnLeg1"/>
                      <xsl:with-param name="pDerivativeContractVersion" select="$vDerivativeContractVersionLeg1"/>
                      <xsl:with-param name="pDerivativeContractCategory" select="$vDerivativeContractCategoryLeg1"/>
                      <xsl:with-param name="pBuyerOrSellerAccount" select="$vBuyerOrSellerAccount"/>
                      <xsl:with-param name="pDefaultMode">
                        <xsl:choose>
                          <xsl:when test ="string-length(data[@name='MID'])>0 and data[@name='MIDTYP'] = 'GCM'">
                            <!-- FI 20170404 [23039] DefaultFromMarket -->
                            <!-- Recherche de la chambre de compensation à partir du marché  -->
                            <xsl:value-of select ="'DefaultFromMarket'"/>
                          </xsl:when>
                          <xsl:otherwise>
                            <!-- Recherche du propriétaire du book -->
                            <xsl:value-of select ="'DefaultFromBook'"/>
                          </xsl:otherwise>
                        </xsl:choose>
                      </xsl:with-param>
                    </xsl:call-template>
                  </xsl:variable>
                  <xsl:variable name ="vXMLClearingOrganizationParsingValue">
                    <xsl:if test="string-length($vClearingOrganization) > 0 and string-length($vClearingOrganizationIdent) > 0">
                      <parsingValue>
                        <xsl:value-of select="$vClearingOrganization"/>
                      </parsingValue>
                    </xsl:if>
                    <xsl:if test="string-length($vClearingOrganizationAccount) > 0 and string-length($vClearingOrganizationAccountIdent) > 0">
                      <parsingValue>
                        <xsl:value-of select="$vClearingOrganizationAccount"/>
                      </parsingValue>
                    </xsl:if>
                  </xsl:variable>
                  <!-- RD 20140409 [19816] use template vClearingOrganizationBook-->
                  <xsl:variable name ="vClearingOrganizationBook">
                    <!-- FI 20160929 [22507] Chgt de signature template ovrSQLClearingOrganizationBook -->
                    <xsl:call-template name="ovrSQLClearingOrganizationBook">
                      <xsl:with-param name="pClearingOrganizationAccount" select="$vClearingOrganizationAccount"/>
                      <xsl:with-param name="pClearingOrganizationAccountIdent" select="$vClearingOrganizationAccountIdent"/>
                      <xsl:with-param name="pBuyerOrSellerAccount" select="$vBuyerOrSellerAccount"/>
                    </xsl:call-template>
                  </xsl:variable>
                  <!--///-->
                  <xsl:variable name ="vExecutingBroker" select ="data[@name='BSEB']"/>
                  <xsl:variable name ="vExecutingBrokerIdent" select ="data[@name='BSEBI']"/>
                  <xsl:variable name ="vExecutingBrokerActor">
                    <xsl:call-template name="SQLActor">
                      <xsl:with-param name="pValue">
                        <xsl:value-of select="$vExecutingBroker"/>
                      </xsl:with-param>
                      <xsl:with-param name="pValueIdent">
                        <xsl:value-of select="$vExecutingBrokerIdent"/>
                      </xsl:with-param>
                    </xsl:call-template>
                  </xsl:variable>
                  <!--///-->
                  <xsl:call-template name="XMLParty">
                    <xsl:with-param name="pName">
                      <xsl:value-of select="$ConstBuyerOrSeller"/>
                    </xsl:with-param>
                    <xsl:with-param name="pIsMissingMode" select ="$isAllowDataMissDealer"/>
                    <xsl:with-param name="pIsMissingModeSpecified" select ="$ConstTrue"/>
                    <xsl:with-param name="pParsingValue">
                      <xsl:value-of select ="$vBuyerOrSellerAccount"/>
                    </xsl:with-param>
                    <xsl:with-param name="pActor">
                      <xsl:copy-of select="$vBuyerOrSellerActor"/>
                    </xsl:with-param>
                    <xsl:with-param name="pBook">
                      <xsl:copy-of select="$vBuyerOrSellerBook"/>
                    </xsl:with-param>
                  </xsl:call-template>
                  <xsl:if test="string-length($vNegociationBrokerActor) >0">
                    <xsl:call-template name="XMLParty">
                      <xsl:with-param name="pName">
                        <xsl:value-of select="$ConstNegociationBroker"/>
                      </xsl:with-param>
                      <xsl:with-param name="pParsingValue">
                        <xsl:value-of select ="$vNegociationBroker"/>
                      </xsl:with-param>
                      <xsl:with-param name="pActor">
                        <xsl:copy-of select="$vNegociationBrokerActor"/>
                      </xsl:with-param>
                    </xsl:call-template>
                  </xsl:if>
                  <xsl:call-template name="XMLParty">
                    <xsl:with-param name="pName">
                      <xsl:value-of select="$ConstClearingOrganization"/>
                    </xsl:with-param>
                    <xsl:with-param name="pXMLParsingValue">
                      <xsl:copy-of select ="$vXMLClearingOrganizationParsingValue"/>
                    </xsl:with-param>
                    <xsl:with-param name="pActor">
                      <xsl:copy-of select="$vClearingOrganizationActor"/>
                    </xsl:with-param>
                    <xsl:with-param name="pBook">
                      <xsl:copy-of select="$vClearingOrganizationBook"/>
                    </xsl:with-param>
                    <!-- FI 20130703 [18798] 
                    Si $isAllowDataMissing vaut true
                    lorsque la clearing House est non renseignée ou incorrecte Spheres® génère unn trade incomplet 
                    -->
                    <xsl:with-param name="pIsMissingMode" select ="$isAllowDataMissing"/>
                    <xsl:with-param name="pIsMissingModeSpecified" select ="$ConstTrue"/>
                  </xsl:call-template>
                  <xsl:if test="string-length($vExecutingBrokerActor) >0">
                    <xsl:call-template name="XMLParty">
                      <xsl:with-param name="pName">
                        <xsl:value-of select="$ConstExecutingBroker"/>
                      </xsl:with-param>
                      <xsl:with-param name="pParsingValue">
                        <xsl:value-of select ="$vExecutingBroker"/>
                      </xsl:with-param>
                      <xsl:with-param name="pActor">
                        <xsl:copy-of select="$vExecutingBrokerActor"/>
                      </xsl:with-param>
                    </xsl:call-template>
                  </xsl:if>
                </xsl:when>
                <!-- Execution/Intermediation-->
                <xsl:otherwise>
                  <xsl:variable name ="vBuyerAccountLeg1" select ="data[@name='BYA']"/>
                  <xsl:variable name ="vBuyerAccountIdentLeg1" select ="data[@name='BYAI']"/>
                  <xsl:variable name ="vBuyerActorLeg1">
                    <xsl:call-template name="SQLActorAndBook">
                      <xsl:with-param name="pValue">
                        <xsl:value-of select="$vBuyerAccountLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name="pValueIdent">
                        <xsl:value-of select="$vBuyerAccountIdentLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name="pResultColumn">
                        <xsl:value-of select="'IDENTIFIER'"/>
                      </xsl:with-param>
                      <xsl:with-param name="pUseDefaultValue" select ="$isAllowDataMissDealer"/>
                      <xsl:with-param name="pDefaultValue" select ="$ConstActorSYSTEM"/>
                    </xsl:call-template>
                  </xsl:variable>
                  <xsl:variable name ="vBuyerBookLeg1">
                    <xsl:call-template name="SQLActorAndBook">
                      <xsl:with-param name="pValue">
                        <xsl:value-of select="$vBuyerAccountLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name="pValueIdent">
                        <xsl:value-of select="$vBuyerAccountIdentLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name="pResultColumn">
                        <xsl:value-of select="'BOOK_IDENTIFIER'"/>
                      </xsl:with-param>
                      <xsl:with-param name="pUseDefaultValue" select ="$isAllowDataMissDealer"/>
                    </xsl:call-template>
                  </xsl:variable>
                  <!--///-->
                  <xsl:variable name ="vBuyerNegociationBrokerLeg1" select ="data[@name='BYNB']"/>
                  <xsl:variable name ="vBuyerNegociationBrokerIdentLeg1" select ="data[@name='BYNBI']"/>
                  <xsl:variable name ="vBuyerNegociationBrokerActorLeg1">
                    <xsl:call-template name="SQLActor">
                      <xsl:with-param name="pValue">
                        <xsl:value-of select="$vBuyerNegociationBrokerLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name="pValueIdent">
                        <xsl:value-of select="$vBuyerNegociationBrokerIdentLeg1"/>
                      </xsl:with-param>
                    </xsl:call-template>
                  </xsl:variable>
                  <!--///-->
                  <xsl:variable name ="vBuyerExecutingBrokerLeg1" select ="data[@name='BYEB']"/>
                  <xsl:variable name ="vBuyerExecutingBrokerIdentLeg1" select ="data[@name='BYEBI']"/>
                  <xsl:variable name ="vBuyerExecutingBrokerActorLeg1">
                    <xsl:call-template name="SQLActor">
                      <xsl:with-param name="pValue">
                        <xsl:value-of select="$vBuyerExecutingBrokerLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name="pValueIdent">
                        <xsl:value-of select="$vBuyerExecutingBrokerIdentLeg1"/>
                      </xsl:with-param>
                    </xsl:call-template>
                  </xsl:variable>
                  <!--///-->
                  <xsl:variable name ="vBuyerClearingBrokerLeg1" select ="data[@name='BYCB']"/>
                  <xsl:variable name ="vBuyerClearingBrokerIdentLeg1" select ="data[@name='BYCBI']"/>
                  <xsl:variable name ="vBuyerClearingBrokerActorLeg1">
                    <xsl:call-template name="SQLActor">
                      <xsl:with-param name="pValue">
                        <xsl:value-of select="$vBuyerClearingBrokerLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name="pValueIdent">
                        <xsl:value-of select="$vBuyerClearingBrokerIdentLeg1"/>
                      </xsl:with-param>
                    </xsl:call-template>
                  </xsl:variable>
                  <!--///-->
                  <xsl:variable name ="vSellerAccountLeg1" select ="data[@name='SLA']"/>
                  <xsl:variable name ="vSellerAccountIdentLeg1" select ="data[@name='SLAI']"/>
                  <xsl:variable name ="vSellerActorLeg1">
                    <xsl:call-template name="SQLActorAndBook">
                      <xsl:with-param name="pValue">
                        <xsl:value-of select="$vSellerAccountLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name="pValueIdent">
                        <xsl:value-of select="$vSellerAccountIdentLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name="pUseDefaultValue" select ="$isAllowDataMissDealer"/>
                      <xsl:with-param name="pDefaultValue" select ="$ConstActorSYSTEM"/>
                    </xsl:call-template>
                  </xsl:variable>
                  <xsl:variable name ="vSellerBookLeg1">
                    <xsl:call-template name="SQLActorAndBook">
                      <xsl:with-param name="pValue">
                        <xsl:value-of select="$vSellerAccountLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name="pValueIdent">
                        <xsl:value-of select="$vSellerAccountIdentLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name="pResultColumn">
                        <xsl:value-of select="'BOOK_IDENTIFIER'"/>
                      </xsl:with-param>
                      <xsl:with-param name="pUseDefaultValue" select ="$isAllowDataMissDealer"/>
                    </xsl:call-template>
                  </xsl:variable>
                  <!--///-->
                  <xsl:variable name ="vSellerNegociationBrokerLeg1" select ="data[@name='SLNB']"/>
                  <xsl:variable name ="vSellerNegociationBrokerIdentLeg1" select ="data[@name='SLNBI']"/>
                  <xsl:variable name ="vSellerNegociationBrokerActorLeg1">
                    <xsl:call-template name="SQLActor">
                      <xsl:with-param name="pValue">
                        <xsl:value-of select="$vSellerNegociationBrokerLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name="pValueIdent">
                        <xsl:value-of select="$vSellerNegociationBrokerIdentLeg1"/>
                      </xsl:with-param>

                    </xsl:call-template>
                  </xsl:variable>
                  <!--///-->
                  <xsl:variable name ="vSellerExecutingBrokerLeg1" select ="data[@name='SLEB']"/>
                  <xsl:variable name ="vSellerExecutingBrokerIdentLeg1" select ="data[@name='SLEBI']"/>
                  <xsl:variable name ="vSellerExecutingBrokerActorLeg1">
                    <xsl:call-template name="SQLActor">
                      <xsl:with-param name="pValue">
                        <xsl:value-of select="$vSellerExecutingBrokerLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name="pValueIdent">
                        <xsl:value-of select="$vSellerExecutingBrokerIdentLeg1"/>
                      </xsl:with-param>

                    </xsl:call-template>
                  </xsl:variable>
                  <!--///-->
                  <xsl:variable name ="vSellerClearingBrokerLeg1" select ="data[@name='SLCB']"/>
                  <xsl:variable name ="vSellerClearingBrokerIdentLeg1" select ="data[@name='SLCBI']"/>
                  <xsl:variable name ="vSellerClearingBrokerActorLeg1">
                    <xsl:call-template name="SQLActor">
                      <xsl:with-param name="pValue">
                        <xsl:value-of select="$vSellerClearingBrokerLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name="pValueIdent">
                        <xsl:value-of select="$vSellerClearingBrokerIdentLeg1"/>
                      </xsl:with-param>
                    </xsl:call-template>
                  </xsl:variable>
                  <!--///-->
                  <xsl:call-template name="XMLParty">
                    <xsl:with-param name="pName">
                      <xsl:value-of select="$ConstBuyer"/>
                    </xsl:with-param>
                    <xsl:with-param name="pIsMissingMode" select ="$isAllowDataMissDealer"/>
                    <xsl:with-param name="pIsMissingModeSpecified" select ="$ConstTrue"/>
                    <xsl:with-param name="pParsingValue">
                      <xsl:value-of select ="$vBuyerAccountLeg1"/>
                    </xsl:with-param>
                    <xsl:with-param name="pActor">
                      <xsl:copy-of select="$vBuyerActorLeg1"/>
                    </xsl:with-param>
                    <xsl:with-param name="pBook">
                      <xsl:copy-of select="$vBuyerBookLeg1"/>
                    </xsl:with-param>
                  </xsl:call-template>
                  <xsl:if test="string-length($vBuyerNegociationBrokerActorLeg1) >0">
                    <xsl:call-template name="XMLParty">
                      <xsl:with-param name="pName">
                        <xsl:value-of select="$ConstBuyerNegociationBroker"/>
                      </xsl:with-param>
                      <xsl:with-param name="pParsingValue">
                        <xsl:value-of select ="$vBuyerNegociationBrokerLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name="pActor">
                        <xsl:copy-of select="$vBuyerNegociationBrokerActorLeg1"/>
                      </xsl:with-param>
                    </xsl:call-template>
                  </xsl:if>
                  <xsl:if test="string-length($vBuyerExecutingBrokerActorLeg1) >0">
                    <xsl:call-template name="XMLParty">
                      <xsl:with-param name="pName">
                        <xsl:value-of select="$ConstBuyerExecutingBroker"/>
                      </xsl:with-param>
                      <xsl:with-param name="pParsingValue">
                        <xsl:value-of select ="$vBuyerExecutingBrokerLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name="pActor">
                        <xsl:copy-of select="$vBuyerExecutingBrokerActorLeg1"/>
                      </xsl:with-param>
                    </xsl:call-template>
                  </xsl:if>
                  <xsl:if test="string-length($vBuyerClearingBrokerActorLeg1) >0">
                    <xsl:call-template name="XMLParty">
                      <xsl:with-param name="pName">
                        <xsl:value-of select="$ConstBuyerClearingBroker"/>
                      </xsl:with-param>
                      <xsl:with-param name="pParsingValue">
                        <xsl:value-of select ="$vBuyerClearingBrokerLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name="pActor">
                        <xsl:copy-of select="$vBuyerClearingBrokerActorLeg1"/>
                      </xsl:with-param>
                    </xsl:call-template>
                  </xsl:if>
                  <xsl:call-template name="XMLParty">
                    <xsl:with-param name="pName">
                      <xsl:value-of select="$ConstSeller"/>
                    </xsl:with-param>
                    <xsl:with-param name="pIsMissingMode" select ="$isAllowDataMissDealer"/>
                    <xsl:with-param name="pIsMissingModeSpecified" select ="$ConstTrue"/>
                    <xsl:with-param name="pParsingValue">
                      <xsl:value-of select ="$vSellerAccountLeg1"/>
                    </xsl:with-param>
                    <xsl:with-param name="pActor">
                      <xsl:copy-of select="$vSellerActorLeg1"/>
                    </xsl:with-param>
                    <xsl:with-param name="pBook">
                      <xsl:copy-of select="$vSellerBookLeg1"/>
                    </xsl:with-param>
                  </xsl:call-template>
                  <xsl:if test="string-length($vSellerNegociationBrokerActorLeg1) >0">
                    <xsl:call-template name="XMLParty">
                      <xsl:with-param name="pName">
                        <xsl:value-of select="$ConstSellerNegociationBroker"/>
                      </xsl:with-param>
                      <xsl:with-param name="pParsingValue">
                        <xsl:value-of select ="$vSellerNegociationBrokerLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name="pActor">
                        <xsl:copy-of select="$vSellerNegociationBrokerActorLeg1"/>
                      </xsl:with-param>
                    </xsl:call-template>
                  </xsl:if>
                  <xsl:if test="string-length($vSellerExecutingBrokerActorLeg1) >0">
                    <xsl:call-template name="XMLParty">
                      <xsl:with-param name="pName">
                        <xsl:value-of select="$ConstSellerExecutingBroker"/>
                      </xsl:with-param>
                      <xsl:with-param name="pParsingValue">
                        <xsl:value-of select ="$vSellerExecutingBrokerLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name="pActor">
                        <xsl:copy-of select="$vSellerExecutingBrokerActorLeg1"/>
                      </xsl:with-param>
                    </xsl:call-template>
                  </xsl:if>
                  <xsl:if test="string-length($vSellerClearingBrokerActorLeg1) >0">
                    <xsl:call-template name="XMLParty">
                      <xsl:with-param name="pName">
                        <xsl:value-of select="$ConstSellerClearingBroker"/>
                      </xsl:with-param>
                      <xsl:with-param name="pParsingValue">
                        <xsl:value-of select ="$vSellerClearingBrokerLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name="pActor">
                        <xsl:copy-of select="$vSellerClearingBrokerActorLeg1"/>
                      </xsl:with-param>
                    </xsl:call-template>
                  </xsl:if>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>

            <!-- PARTY 1 SECTION -->
            <xsl:choose>
              <!-- Allocation-->
              <xsl:when test ="$vRecordTypeLeg1='A'">
                <xsl:variable name ="vBuyerOrSellerFrontId" select ="data[@name='BSFRI']"/>
                <xsl:variable name ="vBuyerOrSellerFolderId" select ="data[@name='BSFLI']"/>
                <xsl:variable name ="vBuyerOrSellerUTI" select ="data[@name='DLUTI']"/>
                <!--///-->
                <xsl:variable name ="vBuyerOrSellerTrader" select ="data[@name='BSTR']"/>
                <xsl:variable name ="vBuyerOrSellerTraderIdent" select ="data[@name='BSTRI']"/>
                <xsl:variable name ="vXMLBuyerOrSellerSales">
                  <xsl:call-template name="XMLSale">
                    <xsl:with-param name="pIdent">
                      <xsl:value-of select ="data[@name='BSSLI1']"/>
                    </xsl:with-param>
                    <xsl:with-param name="pCoeff">
                      <xsl:value-of select ="data[@name='BSSLC1']"/>
                    </xsl:with-param>
                    <xsl:with-param name="pValue">
                      <xsl:value-of select="data[@name='BSSL1']"/>
                    </xsl:with-param>
                  </xsl:call-template>
                  <xsl:call-template name="XMLSale">
                    <xsl:with-param name="pIdent">
                      <xsl:value-of select ="data[@name='BSSLI2']"/>
                    </xsl:with-param>
                    <xsl:with-param name="pCoeff">
                      <xsl:value-of select ="data[@name='BSSLC2']"/>
                    </xsl:with-param>
                    <xsl:with-param name="pValue">
                      <xsl:value-of select="data[@name='BSSL2']"/>
                    </xsl:with-param>
                  </xsl:call-template>
                  <xsl:call-template name="XMLSale">
                    <xsl:with-param name="pIdent">
                      <xsl:value-of select ="data[@name='BSSLI3']"/>
                    </xsl:with-param>
                    <xsl:with-param name="pCoeff">
                      <xsl:value-of select ="data[@name='BSSLC3']"/>
                    </xsl:with-param>
                    <xsl:with-param name="pValue">
                      <xsl:value-of select="data[@name='BSSL3']"/>
                    </xsl:with-param>
                  </xsl:call-template>
                  <xsl:call-template name="XMLSale">
                    <xsl:with-param name="pIdent">
                      <xsl:value-of select ="data[@name='BSSLI4']"/>
                    </xsl:with-param>
                    <xsl:with-param name="pCoeff">
                      <xsl:value-of select ="data[@name='BSSLC4']"/>
                    </xsl:with-param>
                    <xsl:with-param name="pValue">
                      <xsl:value-of select="data[@name='BSSL4']"/>
                    </xsl:with-param>
                  </xsl:call-template>
                  <xsl:call-template name="XMLSale">
                    <xsl:with-param name="pIdent">
                      <xsl:value-of select ="data[@name='BSSLI5']"/>
                    </xsl:with-param>
                    <xsl:with-param name="pCoeff">
                      <xsl:value-of select ="data[@name='BSSLC5']"/>
                    </xsl:with-param>
                    <xsl:with-param name="pValue">
                      <xsl:value-of select="data[@name='BSSL5']"/>
                    </xsl:with-param>
                  </xsl:call-template>
                </xsl:variable>
                <!--///-->
                <xsl:call-template name="CcisParty">
                  <xsl:with-param name="pPartyNumber">
                    <xsl:value-of select="$ConstPartyNumber1"/>
                  </xsl:with-param>
                  <xsl:with-param name ="pXMLParty">
                    <xsl:copy-of select="msxsl:node-set($vXMLTradePartiesAndBrokers)/party[@name=$ConstBuyerOrSeller]"/>
                    <xsl:copy-of select="msxsl:node-set($vXMLTradePartiesAndBrokers)/book[@name=$ConstBuyerOrSeller]"/>
                  </xsl:with-param>
                  <xsl:with-param name ="pXMLBroker">
                    <xsl:copy-of select="msxsl:node-set($vXMLTradePartiesAndBrokers)/party[@name=$ConstNegociationBroker]"/>
                  </xsl:with-param>
                  <xsl:with-param name ="pFrontId">
                    <xsl:value-of select="$vBuyerOrSellerFrontId"/>
                  </xsl:with-param>
                  <xsl:with-param name ="pFolderId">
                    <xsl:value-of select="$vBuyerOrSellerFolderId"/>
                  </xsl:with-param>
                  <xsl:with-param name ="pTrader">
                    <xsl:value-of select="$vBuyerOrSellerTrader"/>
                  </xsl:with-param>
                  <xsl:with-param name ="pTraderIdent">
                    <xsl:value-of select="$vBuyerOrSellerTraderIdent"/>
                  </xsl:with-param>
                  <xsl:with-param name ="pXMLSales">
                    <xsl:copy-of select="$vXMLBuyerOrSellerSales"/>
                  </xsl:with-param>
                  <!--FI 20130702 [18798] add pIsMandatoryActor and pIsMandatoryBook -->
                  <xsl:with-param name ="pIsMandatoryActor">
                    <xsl:copy-of select="$ConstTrue"/>
                  </xsl:with-param>
                  <xsl:with-param name ="pIsMandatoryBook">
                    <xsl:copy-of select="$ConstTrue"/>
                  </xsl:with-param>
                  <!--FI 20140204 [19566] add pUTI-->
                  <xsl:with-param name ="pUTI">
                    <xsl:value-of select="$vBuyerOrSellerUTI"/>
                  </xsl:with-param>
                </xsl:call-template>
              </xsl:when>
              <!-- Execution/Intermediation-->
              <xsl:otherwise>
                <xsl:call-template name="CcisOrderParty">
                  <xsl:with-param name="pPartyNumber">
                    <xsl:value-of select="$ConstPartyNumber1"/>
                  </xsl:with-param>
                  <xsl:with-param name ="pPartyName">
                    <xsl:copy-of select="$ConstBuyer"/>
                  </xsl:with-param>
                  <xsl:with-param name ="pXMLRowLeg1">
                    <xsl:copy-of select="."/>
                  </xsl:with-param>
                  <xsl:with-param name ="pXMLTradePartiesAndBrokers">
                    <xsl:copy-of select ="$vXMLTradePartiesAndBrokers"/>
                  </xsl:with-param>
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>

            <!-- PARTY 2 SECTION -->
            <xsl:choose>
              <!-- Allocation-->
              <xsl:when test ="$vRecordTypeLeg1='A'">
                <xsl:variable name ="vCOUTI" select ="data[@name='COUTI']"/>
                <xsl:call-template name="CcisParty">
                  <xsl:with-param name="pPartyNumber">
                    <xsl:value-of select="$ConstPartyNumber2"/>
                  </xsl:with-param>
                  <xsl:with-param name="pIsNCM">
                    <xsl:value-of select="$ConstFalse"/>
                  </xsl:with-param>
                  <xsl:with-param name ="pXMLParty">
                    <xsl:copy-of select="msxsl:node-set($vXMLTradePartiesAndBrokers)/party[@name=$ConstClearingOrganization]"/>
                    <xsl:copy-of select="msxsl:node-set($vXMLTradePartiesAndBrokers)/book[@name=$ConstClearingOrganization]"/>
                  </xsl:with-param>
                  <!-- FI 20170718 [23326] Modify  (prise en compte de l'élément partie tel que @name=$ConstClearingBroker (opérateur or))
                  Depuis une gateway, les flux post-parsing peuvent désormais produire l'élément ClearingBroker  (voir ErxFixMLTradeSpheresImp-par.xsl et BCSTradeSpheresImp-par.xsl) 
                  Avant il n'y a avait que l'élément @name=$ConstExecutingBroker (alimenté en fonction des cas avec un clearingBroker ou un executingBroker)  
                  Rq le format standard v2.6 ne préfoit également que l'existence d'un élément executingBroker
                  -->
                  <xsl:with-param name ="pXMLBroker">
                    <xsl:copy-of select="msxsl:node-set($vXMLTradePartiesAndBrokers)/party[@name=$ConstExecutingBroker or @name=$ConstClearingBroker]"/>
                  </xsl:with-param>
                  <xsl:with-param name ="pFrontId"/>
                  <xsl:with-param name ="pFolderId"/>
                  <xsl:with-param name ="pTrader"/>
                  <xsl:with-param name ="pTraderIdent"/>
                  <xsl:with-param name ="pXMLSales"/>
                  <!--FI 20130703 [18798] add pIsMandatoryActor à true de manière à intégrer les trades lorsque la clearing Hiouse est non renseignée
                  Si la donnée est vide => cela génère un message d'erreur dans le cci => le trade passe en mode Incomplet
                  Ce comportement s'applique si $isAllowDataMissing
                  -->
                  <xsl:with-param name ="pIsMandatoryActor">
                    <xsl:copy-of select="$ConstTrue"/>
                  </xsl:with-param>
                  <!--FI 20140204 [19566] add pUTI-->
                  <xsl:with-param name ="pUTI">
                    <xsl:value-of select="$vCOUTI"/>
                  </xsl:with-param>
                </xsl:call-template>
              </xsl:when>
              <!-- Execution/Intermediation-->
              <xsl:otherwise>
                <xsl:call-template name="CcisOrderParty">
                  <xsl:with-param name="pPartyNumber">
                    <xsl:value-of select="$ConstPartyNumber2"/>
                  </xsl:with-param>
                  <xsl:with-param name ="pPartyName">
                    <xsl:copy-of select="$ConstSeller"/>
                  </xsl:with-param>
                  <xsl:with-param name ="pXMLRowLeg1">
                    <xsl:copy-of select="."/>
                  </xsl:with-param>
                  <xsl:with-param name ="pXMLTradePartiesAndBrokers">
                    <xsl:copy-of select ="$vXMLTradePartiesAndBrokers"/>
                  </xsl:with-param>
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>

            <!-- ======================================================================================= -->
            <!--                                   DATES SECTION                                         -->
            <!-- ======================================================================================= -->
            <!-- TransactionDate Cci-->
            <xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId">
                <xsl:value-of select ="'tradeHeader_tradeDate'"/>
              </xsl:with-param>
              <xsl:with-param name="pDataType">
                <xsl:value-of select ="'date'"/>
              </xsl:with-param>
              <xsl:with-param name="pValue">
                <xsl:value-of select="$vTransactionDateLeg1"/>
              </xsl:with-param>
            </xsl:call-template>
            <!-- Horodatage Cci-->
            <!-- RD Glop / Revoir Time vs datetime-->
            <xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId">
                <xsl:value-of select ="'tradeHeader_timeStamping'"/>
              </xsl:with-param>
              <xsl:with-param name="pDataType">
                <xsl:value-of select ="'time'"/>
              </xsl:with-param>
              <xsl:with-param name="pRegex">
                <xsl:value-of select ="'RegexLongTime'"/>
              </xsl:with-param>
              <xsl:with-param name="pValue">
                <xsl:value-of select="data[@name='TDT']"/>
              </xsl:with-param>
            </xsl:call-template>
            <!-- ClearingBusinessDate Cci-->
            <xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId">
                <xsl:value-of select ="concat($vCciPrefixStrategy,$vCciPrefixFamily,'_FIXML_TrdCapRpt_BizDt')"/>
              </xsl:with-param>
              <xsl:with-param name="pDataType">
                <xsl:value-of select ="'date'"/>
              </xsl:with-param>
              <xsl:with-param name="pValue">
                <xsl:value-of select="$vClearingBusinessDateLeg1"/>
              </xsl:with-param>
            </xsl:call-template>

            <!-- ======================================================================================= -->
            <!--                                   LEGS SECTION                                          -->
            <!-- ======================================================================================= -->
            <xsl:choose>
              <!-- Instrument is Strategy -->
              <xsl:when test="$vIsStrategyLeg1 = $ConstTrue">
                <xsl:for-each select ="$vAllTradeRows">
                  <xsl:sort select="data[@name='LSN']"/>

                  <xsl:variable name ="vLegNumber" select="position()"/>

                  <!-- ....................................................................................... -->
                  <!--                      Strategy Specific Ccis SECTION                                     -->
                  <!-- ....................................................................................... -->
                  <!-- Buyer Cci-->
                  <xsl:variable name ="vBuyer" select ="data[@name='BYA']"/>
                  <xsl:call-template name="CustomCaptureInfo">
                    <xsl:with-param name="pClientId">
                      <xsl:value-of select ="concat($vCciPrefixStrategy,$vCciPrefixFamily,$vLegNumber,'_buyer')"/>
                    </xsl:with-param>
                    <xsl:with-param name="pDataType">
                      <xsl:value-of select ="'string'"/>
                    </xsl:with-param>
                    <xsl:with-param name="pIsMandatory">
                      <xsl:value-of select ="$ConstTrue"/>
                    </xsl:with-param>
                    <xsl:with-param name="pIsMissingMode" select ="$isAllowDataMissDealer"/>
                    <xsl:with-param name="pIsMissingModeSpecified" select ="$ConstTrue"/>
                    <xsl:with-param name="pXMLDynamicValue">
                      <xsl:copy-of select ="msxsl:node-set($vXMLTradePartiesAndBrokers)/party[normalize-space(parsingValue)=normalize-space($vBuyer)][1]/dynamicValue"/>
                    </xsl:with-param>
                  </xsl:call-template>
                  <!-- Seller Cci-->
                  <xsl:variable name ="vSeller" select ="data[@name='SLA']"/>
                  <xsl:call-template name="CustomCaptureInfo">
                    <xsl:with-param name="pClientId">
                      <xsl:value-of select ="concat($vCciPrefixStrategy,$vCciPrefixFamily,$vLegNumber,'_seller')"/>
                    </xsl:with-param>
                    <xsl:with-param name="pDataType">
                      <xsl:value-of select ="'string'"/>
                    </xsl:with-param>
                    <xsl:with-param name="pIsMandatory">
                      <xsl:value-of select ="$ConstTrue"/>
                    </xsl:with-param>
                    <xsl:with-param name="pIsMissingMode" select ="$isAllowDataMissDealer"/>
                    <xsl:with-param name="pIsMissingModeSpecified" select ="$ConstTrue"/>
                    <xsl:with-param name="pXMLDynamicValue">
                      <xsl:copy-of select ="msxsl:node-set($vXMLTradePartiesAndBrokers)/party[normalize-space(parsingValue)=normalize-space($vSeller)][1]/dynamicValue"/>
                    </xsl:with-param>
                  </xsl:call-template>
                  <!-- RD Glop-->
                  <!-- HedgeRatio Cci-->
                  <xsl:variable name ="vHedgeRatio" select ="data[@name='HRA']"/>
                  <!--<xsl:call-template name="CustomCaptureInfo">
                    <xsl:with-param name="pClientId">
                      <xsl:value-of select ="concat($vCciPrefixStrategy,$vCciPrefixFamily,$vLegNumber,'_seller')"/>
                    </xsl:with-param>
                    <xsl:with-param name="pDataType">
                      <xsl:value-of select ="'string'"/>
                    </xsl:with-param>
                    <xsl:with-param name="pXMLDynamicValue">
                      <xsl:copy-of select ="msxsl:node-set($vXMLTradePartiesAndBrokers)/party[normalize-space(parsingValue)=$vSeller][1]/dynamicValue"/>
                    </xsl:with-param>
                  </xsl:call-template>-->
                  <!-- ....................................................................................... -->
                  <!--                             Common Leg Ccis SECTION                                     -->
                  <!-- ....................................................................................... -->
                  <xsl:call-template name="CcisLeg">
                    <xsl:with-param name="pLegNumber">
                      <xsl:value-of select="$vLegNumber"/>
                    </xsl:with-param>
                    <xsl:with-param name ="pXMLLegRow">
                      <xsl:copy-of select="."/>
                    </xsl:with-param>
                    <xsl:with-param name ="pCciPrefix">
                      <xsl:value-of select="$vCciPrefixStrategy"/>
                    </xsl:with-param>
                    <xsl:with-param name ="pCciPrefixFamily">
                      <xsl:value-of select="$vCciPrefixFamily"/>
                    </xsl:with-param>
                    <xsl:with-param name ="pRecordTypeLeg1">
                      <xsl:value-of select="$vRecordTypeLeg1"/>
                    </xsl:with-param>
                    <xsl:with-param name ="pMarketLeg1">
                      <xsl:value-of select="$vMarketLeg1"/>
                    </xsl:with-param>
                    <xsl:with-param name ="pMarketColumnLeg1">
                      <xsl:value-of select="$vMarketColumnLeg1"/>
                    </xsl:with-param>
                    <xsl:with-param name ="pFamily">
                      <xsl:value-of select="$vFamilyLeg1"/>
                    </xsl:with-param>
                    <xsl:with-param name ="pClearingBusinessDate" select="$vClearingBusinessDateLeg1"/>
                    <xsl:with-param name ="pIsAllowDataMissing" select="$isAllowDataMissing"/>
                  </xsl:call-template>
                </xsl:for-each >
              </xsl:when>
              <!-- Instrument is NOT Strategy -->
              <xsl:otherwise>
                <xsl:choose>
                  <!-- Allocation-->
                  <xsl:when test ="$vRecordTypeLeg1='A'">
                    <!-- ....................................................................................... -->
                    <!--                      Allocation Specific Ccis SECTION                                   -->
                    <!-- ....................................................................................... -->
                    <!-- Side Cci-->
                    <xsl:call-template name="CustomCaptureInfo">
                      <xsl:with-param name="pClientId">
                        <xsl:value-of select ="concat($vCciPrefixFamily,'_FIXML_TrdCapRpt_RptSide_Side')"/>
                      </xsl:with-param>
                      <xsl:with-param name="pDataType">
                        <xsl:value-of select ="'string'"/>
                      </xsl:with-param>
                      <xsl:with-param name="pValue">
                        <xsl:value-of select="normalize-space(data[@name='ALS'])"/>
                      </xsl:with-param>
                    </xsl:call-template>

                    <!-- ....................................................................................... -->
                    <!--                             Common Leg Ccis SECTION                                     -->
                    <!-- ....................................................................................... -->
                    <xsl:call-template name="CcisLeg">
                      <xsl:with-param name="pLegNumber"/>
                      <xsl:with-param name ="pXMLLegRow">
                        <xsl:copy-of select="."/>
                      </xsl:with-param>
                      <xsl:with-param name ="pCciPrefix">
                        <xsl:value-of select="$vCciPrefixStrategy"/>
                      </xsl:with-param>
                      <xsl:with-param name ="pCciPrefixFamily">
                        <xsl:value-of select="$vCciPrefixFamily"/>
                      </xsl:with-param>
                      <xsl:with-param name ="pRecordTypeLeg1">
                        <xsl:value-of select="$vRecordTypeLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name ="pMarketLeg1">
                        <xsl:value-of select="$vMarketLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name ="pMarketColumnLeg1">
                        <xsl:value-of select="$vMarketColumnLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name ="pFamily">
                        <xsl:value-of select="$vFamilyLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name ="pClearingBusinessDate" select="$vClearingBusinessDateLeg1"/>
                      <xsl:with-param name ="pIsAllowDataMissing" select="$isAllowDataMissing"/>
                    </xsl:call-template>

                    <!-- ....................................................................................... -->
                    <!--                    Electronic Trade and Order Ccis SECTION                              -->
                    <!-- ....................................................................................... -->
                    <!-- ExecutionId Cci-->
                    <xsl:call-template name="CustomCaptureInfo">
                      <xsl:with-param name="pClientId">
                        <xsl:value-of select ="concat($vCciPrefixFamily,'_FIXML_TrdCapRpt_ExecID')"/>
                      </xsl:with-param>
                      <xsl:with-param name="pDataType">
                        <xsl:value-of select ="'string'"/>
                      </xsl:with-param>
                      <xsl:with-param name="pValue">
                        <xsl:value-of select="normalize-space(data[@name='EXID'])"/>
                      </xsl:with-param>
                    </xsl:call-template>
                    <!-- MF 20120509 -->
                    <!-- ExecType Cci-->
                    <xsl:call-template name="CustomCaptureInfo">
                      <xsl:with-param name="pClientId">
                        <xsl:value-of select ="concat($vCciPrefixFamily,'_FIXML_TrdCapRpt_ExecType')"/>
                      </xsl:with-param>
                      <xsl:with-param name="pDataType">
                        <xsl:value-of select ="'string'"/>
                      </xsl:with-param>
                      <!--FI 20120709 [17991] call-template SQLEnumValue -->
                      <!--<xsl:with-param name="pValue">
                        <xsl:value-of select="normalize-space(data[@name='EXT'])"/>
                      </xsl:with-param>-->
                      <xsl:with-param name="pXMLDynamicValue">
                        <xsl:call-template name="SQLEnumValue2">
                          <xsl:with-param name="pCode" select="'ExecTypeEnum'"/>
                          <xsl:with-param name="pExtValue" select="normalize-space(data[@name='EXT'])"/>
                        </xsl:call-template>
                      </xsl:with-param>
                    </xsl:call-template>
                    <xsl:variable name="vTrdTypValue">
                      <xsl:value-of select="normalize-space(data[@name='ALT'])"/>
                    </xsl:variable>
                    <xsl:variable name="vTrdTyp2Value">
                      <xsl:value-of select="normalize-space(data[@name='ALT2'])"/>
                    </xsl:variable>
                    <!-- MF 20120509 -->
                    <!-- TrdType Cci-->
                    <xsl:call-template name="CustomCaptureInfo">
                      <xsl:with-param name="pClientId">
                        <xsl:value-of select ="concat($vCciPrefixFamily,'_FIXML_TrdCapRpt_TrdTyp')"/>
                      </xsl:with-param>
                      <xsl:with-param name="pDataType">
                        <xsl:value-of select ="'string'"/>
                      </xsl:with-param>
                      <!--FI 20120709 [17991] call-template SQLEnumValue -->
                      <!--<xsl:with-param name="pValue">
                        <xsl:value-of select="normalize-space(data[@name='ALT'])"/>
                      </xsl:with-param>-->
                      <xsl:with-param name="pXMLDynamicValue">
                        <xsl:call-template name="SQLEnumValue2">
                          <xsl:with-param name="pCode" select="'TrdTypeEnum'"/>
                          <!--RD 20140414 [19597]-->
                          <!--<xsl:with-param name="pExtValue" select="normalize-space(data[@name='ALT'])"/>-->
                          <xsl:with-param name="pExtValue" select="$vTrdTypValue"/>
                        </xsl:call-template>
                      </xsl:with-param>
                    </xsl:call-template>
                    <!-- MF 20120509 -->
                    <!-- TrdSubType Cci-->
                    <xsl:call-template name="CustomCaptureInfo">
                      <xsl:with-param name="pClientId">
                        <xsl:value-of select ="concat($vCciPrefixFamily,'_FIXML_TrdCapRpt_TrdSubTyp')"/>
                      </xsl:with-param>
                      <xsl:with-param name="pDataType">
                        <xsl:value-of select ="'string'"/>
                      </xsl:with-param>
                      <!--FI 20120709 [17991] call-template SQLEnumValue -->
                      <!--<xsl:with-param name="pValue">
                        <xsl:value-of select="normalize-space(data[@name='ALST'])"/>
                      </xsl:with-param>-->
                      <xsl:with-param name="pXMLDynamicValue">
                        <xsl:call-template name="SQLEnumValue2">
                          <!-- MF 20120710 Bug on pCode, TrdSubTyp contains TrdSubTypeEnum values  -->
                          <!--<xsl:with-param name="pCode" select="'SecondaryTrdTypeEnum'"/>-->
                          <xsl:with-param name="pCode" select="'TrdSubTypeEnum'"/>
                          <xsl:with-param name="pExtValue" select="normalize-space(data[@name='ALST'])"/>
                        </xsl:call-template>
                      </xsl:with-param>
                    </xsl:call-template>
                    <!-- MF 20120509 -->
                    <!-- TradeInputSource Cci-->
                    <xsl:call-template name="CustomCaptureInfo">
                      <xsl:with-param name="pClientId">
                        <xsl:value-of select ="concat($vCciPrefixFamily,'_FIXML_TrdCapRpt_RptSide_InptSrc')"/>
                      </xsl:with-param>
                      <xsl:with-param name="pDataType">
                        <xsl:value-of select ="'string'"/>
                      </xsl:with-param>
                      <xsl:with-param name="pValue">
                        <xsl:value-of select="normalize-space(data[@name='ALSRC'])"/>
                      </xsl:with-param>
                    </xsl:call-template>
                    <!-- MF 20120710 Ticket 18006 -->
                    <!-- AllocationSecondaryType Cci-->
                    <xsl:call-template name="CustomCaptureInfo">
                      <xsl:with-param name="pClientId">
                        <xsl:value-of select ="concat($vCciPrefixFamily,'_FIXML_TrdCapRpt_TrdTyp2')"/>
                      </xsl:with-param>
                      <xsl:with-param name="pDataType">
                        <xsl:value-of select ="'string'"/>
                      </xsl:with-param>
                      <xsl:with-param name="pXMLDynamicValue">
                        <xsl:call-template name="SQLEnumValue2">
                          <xsl:with-param name="pCode" select="'SecondaryTrdTypeEnum'"/>
                          <!--RD 20140414 [19597]-->
                          <!--<xsl:with-param name="pExtValue" select="normalize-space(data[@name='ALT2'])"/>-->
                          <xsl:with-param name="pExtValue" select="$vTrdTyp2Value"/>
                        </xsl:call-template>
                      </xsl:with-param>
                    </xsl:call-template>
                    <!-- AllocationID Cci-->
                    <xsl:call-template name="CustomCaptureInfo">
                      <xsl:with-param name="pClientId">
                        <xsl:value-of select ="concat($vCciPrefixFamily,'_FIXML_TrdCapRpt_TrdID')"/>
                      </xsl:with-param>
                      <xsl:with-param name="pDataType">
                        <xsl:value-of select ="'string'"/>
                      </xsl:with-param>
                      <xsl:with-param name="pValue">
                        <xsl:value-of select="normalize-space(data[@name='ALID'])"/>
                      </xsl:with-param>
                    </xsl:call-template>
                    <!-- AllocationInputDevice Cci-->
                    <xsl:call-template name="CustomCaptureInfo">
                      <xsl:with-param name="pClientId">
                        <xsl:value-of select ="concat($vCciPrefixFamily,'_FIXML_TrdCapRpt_RptSide_InptDev')"/>
                      </xsl:with-param>
                      <xsl:with-param name="pDataType">
                        <xsl:value-of select ="'string'"/>
                      </xsl:with-param>
                      <xsl:with-param name="pValue">
                        <xsl:value-of select="normalize-space(data[@name='ALD'])"/>
                      </xsl:with-param>
                    </xsl:call-template>
                    <!-- ///-->
                    <!-- OrderElectId Cci-->
                    <xsl:call-template name="CustomCaptureInfo">
                      <xsl:with-param name="pClientId">
                        <xsl:value-of select ="concat($vCciPrefixFamily,'_FIXML_TrdCapRpt_RptSide_OrdID')"/>
                      </xsl:with-param>
                      <xsl:with-param name="pDataType">
                        <xsl:value-of select ="'string'"/>
                      </xsl:with-param>
                      <xsl:with-param name="pValue">
                        <xsl:value-of select="normalize-space(data[@name='OEID'])"/>
                      </xsl:with-param>
                    </xsl:call-template>
                    <!-- OrderElectCategory Cci -->
                    <xsl:call-template name="CustomCaptureInfo">
                      <xsl:with-param name="pClientId">
                        <xsl:value-of select ="concat($vCciPrefixFamily,'_FIXML_TrdCapRpt_OrdCat')"/>
                      </xsl:with-param>
                      <xsl:with-param name="pDataType">
                        <xsl:value-of select ="'string'"/>
                      </xsl:with-param>
                      <!-- PL 20120504 -->
                      <!--FI 20120709 [17991] call-template SQLEnumValue -->
                      <!--<xsl:with-param name="pValue">
                        <xsl:value-of select="normalize-space(data[@name='OEC'])"/>
                      </xsl:with-param>-->
                      <xsl:with-param name="pXMLDynamicValue">
                        <xsl:call-template name="SQLEnumValue2">
                          <xsl:with-param name="pCode" select="'OrderCategoryEnum'"/>
                          <xsl:with-param name="pExtValue" select="normalize-space(data[@name='OEC'])"/>
                        </xsl:call-template>
                      </xsl:with-param>
                    </xsl:call-template>
                    <!-- OrderElectType Cci-->
                    <xsl:call-template name="CustomCaptureInfo">
                      <xsl:with-param name="pClientId">
                        <xsl:value-of select ="concat($vCciPrefixFamily,'_FIXML_TrdCapRpt_RptSide_OrdTyp')"/>
                      </xsl:with-param>
                      <xsl:with-param name="pDataType">
                        <xsl:value-of select ="'string'"/>
                      </xsl:with-param>
                      <!-- MF 20120514 Ticket 17047 Item 15 -->
                      <!--<xsl:with-param name="pValue">
                        <xsl:call-template name="GetOrderElecType">
                          <xsl:with-param name="pOrderElecType">
                            <xsl:value-of select="normalize-space(data[@name='OET'])"/>
                          </xsl:with-param>
                        </xsl:call-template>
                      </xsl:with-param>-->
                      <xsl:with-param name="pXMLDynamicValue">
                        <xsl:call-template name="SQLEnumValue2">
                          <xsl:with-param name="pCode" select="'OrdTypeEnum'"/>
                          <xsl:with-param name="pExtValue" select="normalize-space(data[@name='OET'])"/>
                        </xsl:call-template>
                      </xsl:with-param>
                    </xsl:call-template>
                    <!-- OrderElectStatus Cci-->
                    <xsl:call-template name="CustomCaptureInfo">
                      <xsl:with-param name="pClientId">
                        <xsl:value-of select ="concat($vCciPrefixFamily,'_FIXML_TrdCapRpt_OrdStat')"/>
                      </xsl:with-param>
                      <xsl:with-param name="pDataType">
                        <xsl:value-of select ="'string'"/>
                      </xsl:with-param>
                      <!-- PL 20120504 -->
                      <!--FI 20120709 [17991] call-template SQLEnumValue -->
                      <!--<xsl:with-param name="pValue">
                        <xsl:value-of select="normalize-space(data[@name='OES'])"/>
                      </xsl:with-param>-->
                      <xsl:with-param name="pXMLDynamicValue">
                        <xsl:call-template name="SQLEnumValue2">
                          <xsl:with-param name="pCode" select="'OrdStatusEnum'"/>
                          <xsl:with-param name="pExtValue" select="normalize-space(data[@name='OES'])"/>
                        </xsl:call-template>
                      </xsl:with-param>
                    </xsl:call-template>
                    <!-- OrderElectInputDevice Cci-->
                    <xsl:call-template name="CustomCaptureInfo">
                      <xsl:with-param name="pClientId">
                        <xsl:value-of select ="concat($vCciPrefixFamily,'_FIXML_TrdCapRpt_RptSide_OrdInptDev')"/>
                      </xsl:with-param>
                      <xsl:with-param name="pDataType">
                        <xsl:value-of select ="'string'"/>
                      </xsl:with-param>
                      <xsl:with-param name="pValue">
                        <xsl:value-of select="normalize-space(data[@name='OED'])"/>
                      </xsl:with-param>
                    </xsl:call-template>
                    <!-- ///-->
                    <!-- FI 20160502 [22107] Add RelatedPositionID -->
                    <!-- RelatedPositionID Cci-->
                    <xsl:call-template name="CustomCaptureInfo">
                      <xsl:with-param name="pClientId">
                        <xsl:value-of select ="concat($vCciPrefixFamily,'_FIXML_TrdCapRpt_RptSide_ReltdPosID')"/>
                      </xsl:with-param>
                      <xsl:with-param name="pDataType">
                        <xsl:value-of select ="'string'"/>
                      </xsl:with-param>
                      <xsl:with-param name="pValue">
                        <xsl:value-of select="normalize-space(data[@name='RPID'])"/>
                      </xsl:with-param>
                    </xsl:call-template>

                  </xsl:when>
                  <!-- Execution/Intermediation-->
                  <xsl:otherwise>
                    <!-- ....................................................................................... -->
                    <!--                             Common Leg Ccis SECTION                                     -->
                    <!-- ....................................................................................... -->
                    <xsl:call-template name="CcisLeg">
                      <xsl:with-param name="pLegNumber"/>
                      <xsl:with-param name ="pXMLLegRow">
                        <xsl:copy-of select="."/>
                      </xsl:with-param>
                      <xsl:with-param name ="pCciPrefix">
                        <xsl:value-of select="$vCciPrefixStrategy"/>
                      </xsl:with-param>
                      <xsl:with-param name ="pCciPrefixFamily">
                        <xsl:value-of select="$vCciPrefixFamily"/>
                      </xsl:with-param>
                      <xsl:with-param name ="pRecordTypeLeg1">
                        <xsl:value-of select="$vRecordTypeLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name ="pMarketLeg1">
                        <xsl:value-of select="$vMarketLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name ="pMarketColumnLeg1">
                        <xsl:value-of select="$vMarketColumnLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name ="pFamily">
                        <xsl:value-of select="$vFamilyLeg1"/>
                      </xsl:with-param>
                      <xsl:with-param name ="pClearingBusinessDate" select="$vClearingBusinessDateLeg1"/>
                      <xsl:with-param name ="pIsAllowDataMissing" select="$isAllowDataMissing"/>
                    </xsl:call-template>

                    <!-- ....................................................................................... -->
                    <!--                      Order Specific Ccis SECTION                                        -->
                    <!-- ....................................................................................... -->
                    <!--Buyer Cci-->
                    <xsl:call-template name="CustomCaptureInfo">
                      <xsl:with-param name="pClientId">
                        <xsl:value-of select ="concat($vCciPrefixFamily,'_buyer')"/>
                      </xsl:with-param>
                      <xsl:with-param name="pDataType">
                        <xsl:value-of select ="'string'"/>
                      </xsl:with-param>
                      <xsl:with-param name="pIsMissingMode" select ="$isAllowDataMissDealer"/>
                      <xsl:with-param name="pIsMissingModeSpecified" select ="$ConstTrue"/>
                      <xsl:with-param name="pXMLDynamicValue">
                        <xsl:copy-of select ="msxsl:node-set($vXMLTradePartiesAndBrokers)/party[normalize-space(@name)=$ConstBuyer]/dynamicValue"/>
                      </xsl:with-param>
                    </xsl:call-template>
                    <!--Seller Cci-->
                    <xsl:call-template name="CustomCaptureInfo">
                      <xsl:with-param name="pClientId">
                        <xsl:value-of select ="concat($vCciPrefixFamily,'_seller')"/>
                      </xsl:with-param>
                      <xsl:with-param name="pDataType">
                        <xsl:value-of select ="'string'"/>
                      </xsl:with-param>
                      <xsl:with-param name="pIsMissingMode" select ="$isAllowDataMissDealer"/>
                      <xsl:with-param name="pIsMissingModeSpecified" select ="$ConstTrue"/>
                      <xsl:with-param name="pXMLDynamicValue">
                        <xsl:copy-of select ="msxsl:node-set($vXMLTradePartiesAndBrokers)/party[normalize-space(@name)=$ConstSeller]/dynamicValue"/>
                      </xsl:with-param>
                    </xsl:call-template>
                    <!--Side Cci-->
                    <xsl:call-template name="CustomCaptureInfo">
                      <xsl:with-param name="pClientId">
                        <xsl:value-of select ="concat($vCciPrefixFamily,'_FIXML_TrdCapRpt_RptSide_Side')"/>
                      </xsl:with-param>
                      <xsl:with-param name="pDataType">
                        <xsl:value-of select ="'string'"/>
                      </xsl:with-param>
                      <xsl:with-param name="pValue">
                        <xsl:value-of select="'1'"/>
                      </xsl:with-param>
                    </xsl:call-template>

                  </xsl:otherwise>
                </xsl:choose>
              </xsl:otherwise>
            </xsl:choose>

            <!-- ======================================================================================= -->
            <!--                                   EXTEND SECTION                                        -->
            <!-- ======================================================================================= -->
            <!-- RD 20121204 [18240] Import: Trades with data for extensions -->
            <xsl:call-template name="CcisExtends">
              <xsl:with-param name="pXMLExtends">
                <xsl:copy-of select="$vXMLExtends"/>
              </xsl:with-param>
              <xsl:with-param name="pInstrumentIdentifier" select="$vInstrumentIdentifier"/>
              <xsl:with-param name="pAssetCode" select="$vAssetCodeLeg1"/>
              <xsl:with-param name="pAssetColumn" select="$vAssetCodeColumnLeg1"/>
              <!-- FI 20170404 [23039] select="$vMarket" -->
              <!--<xsl:with-param name="pMarket" select="$vMarketForAssetCodeIdentMARKET"/>-->
              <xsl:with-param name="pMarket" select="$vMarketLeg1"/>
              <xsl:with-param name="pMarketColumn" select="$vMarketColumnLeg1"/>
              <xsl:with-param name="pClearingBusinessDate" select="$vClearingBusinessDateLeg1"/>
            </xsl:call-template>

            <!-- ======================================================================================= -->
            <!--                                   FEES SECTION                                          -->
            <!-- ======================================================================================= -->
            <xsl:variable name ="vXMLFees">
              <xsl:choose>
                <!-- Instrument is Strategy -->
                <xsl:when test="$vIsStrategyLeg1 = $ConstTrue">
                  <xsl:for-each select ="$vAllTradeRows">
                    <xsl:call-template name="XMLFees">
                      <xsl:with-param name="pXMLRow">
                        <xsl:copy-of select ="."/>
                      </xsl:with-param>
                      <xsl:with-param name="pXMLTradePartiesAndBrokers">
                        <xsl:copy-of select ="$vXMLTradePartiesAndBrokers"/>
                      </xsl:with-param>
                      <xsl:with-param name="pRecordTypeLeg1">
                        <xsl:copy-of select ="$vRecordTypeLeg1"/>
                      </xsl:with-param>
                    </xsl:call-template>
                  </xsl:for-each >
                </xsl:when>
                <!-- Instrument is NOT Strategy -->
                <xsl:otherwise>
                  <xsl:call-template name="XMLFees">
                    <xsl:with-param name="pXMLRow">
                      <xsl:copy-of select ="."/>
                    </xsl:with-param>
                    <xsl:with-param name="pXMLTradePartiesAndBrokers">
                      <xsl:copy-of select ="$vXMLTradePartiesAndBrokers"/>
                    </xsl:with-param>
                    <xsl:with-param name="pRecordTypeLeg1">
                      <xsl:copy-of select ="$vRecordTypeLeg1"/>
                    </xsl:with-param>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <!--RD 20130822 FeeCalculation Project / Gestion da la valeur Businessdate / Ajout parametre pBusinessDate-->
            <xsl:call-template name="CcisFees">
              <xsl:with-param name="pTransactionDate" select="$vTransactionDateLeg1"/>
              <xsl:with-param name="pBusinessDate" select="$vClearingBusinessDateLeg1"/>
              <xsl:with-param name="pXMLFees">
                <xsl:copy-of select="$vXMLFees"/>
              </xsl:with-param>
            </xsl:call-template>
          </customCaptureInfos>
        </tradeInput>
      </tradeImport>
    </row>

  </xsl:template >

  <!-- Get default value for ImportMode according  {pActionType}, {pRecordType}  -->
  <!-- FI 20160907 [21831] Modify -->
  <xsl:template name="GetDefaultImportMode">
    <xsl:param name="pActionType"/>
    <xsl:param name="pRecordType"/>
    <xsl:choose>
      <xsl:when test="$pActionType='N' or $pActionType='G'">
        <xsl:value-of select="'New'"/>
      </xsl:when>
      <!-- FI 20160907 [21831] add ActionType='F' -->
      <xsl:when test="$pActionType='U' or $pActionType='M' or $pActionType='F'">
        <xsl:value-of select="'Update'"/>
      </xsl:when>
      <xsl:when test="$pActionType='S'">
        <xsl:choose>
          <xsl:when test="$pRecordType='E' or $pRecordType='I'">
            <xsl:value-of select="'RemoveOnly'"/>
          </xsl:when>
          <xsl:when test="$pRecordType='A'">
            <xsl:value-of select="'RemoveAllocation'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$ConstUnknown"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$ConstUnknown"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="GetStBusiness">
    <xsl:param name="pRecordType"/>
    <xsl:choose>
      <xsl:when test="$pRecordType='E'">
        <xsl:value-of select="'EXECUTED'"/>
      </xsl:when>
      <xsl:when test="$pRecordType='I'">
        <xsl:value-of select="'INTERMED'"/>
      </xsl:when>
      <xsl:when test="$pRecordType='A'">
        <xsl:value-of select="'ALLOC'"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!--FI 20120709 [17991] [Deprecated] Use SQLEnumValue-->
  <xsl:template name="GetPutCall">
    <xsl:param name="pPutCall"/>
    <xsl:choose>
      <xsl:when test="$pPutCall='P'">
        <xsl:value-of select="'0'"/>
      </xsl:when>
      <xsl:when test="$pPutCall='C'">
        <xsl:value-of select="'1'"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!--FI 20120709 [17991] [Deprecated] Use SQLEnumValue-->
  <xsl:template name="GetPositionEffect">
    <xsl:param name="pPositionEffect"/>
    <xsl:choose>
      <xsl:when test="$pPositionEffect='OPEN'">
        <xsl:value-of select="'O'"/>
      </xsl:when>
      <xsl:when test="$pPositionEffect='CLOSE'">
        <xsl:value-of select="'C'"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!--[Deprecated] Use SQLEnumValue-->
  <xsl:template name="GetOrderElecType">
    <xsl:param name="pOrderElecType"/>
    <!-- Cet ordre est important-->
    <xsl:choose>
      <xsl:when test="contains($pOrderElecType,'Funari')">
        <xsl:value-of select="'I'"/>
      </xsl:when>
      <!-- Ne pas mettre avant 'Funari'-->
      <xsl:when test="contains($pOrderElecType,'Market On Close')">
        <xsl:value-of select="'5'"/>
      </xsl:when>
      <xsl:when test="contains($pOrderElecType,'Forex Market')">
        <xsl:value-of select="'C'"/>
      </xsl:when>
      <xsl:when test="contains($pOrderElecType,'Market If Touched')">
        <xsl:value-of select="'J'"/>
      </xsl:when>
      <xsl:when test="contains($pOrderElecType,'Market With Left Over as Limit')">
        <xsl:value-of select="'K'"/>
      </xsl:when>
      <!-- Ne pas mettre avant 'Funari','Market On Close','Forex Market','Market If Touched','Market With Left Over as Limit'-->
      <xsl:when test="contains($pOrderElecType,'Market')">
        <xsl:value-of select="'1'"/>
      </xsl:when>
      <xsl:when test="contains($pOrderElecType,'Stop Limit')">
        <xsl:value-of select="'4'"/>
      </xsl:when>
      <xsl:when test="contains($pOrderElecType,'Limit Or Better')">
        <xsl:value-of select="'7'"/>
      </xsl:when>
      <xsl:when test="contains($pOrderElecType,'Limit With Or Without')">
        <xsl:value-of select="'8'"/>
      </xsl:when>
      <xsl:when test="contains($pOrderElecType,'Limit On Close')">
        <xsl:value-of select="'B'"/>
      </xsl:when>
      <xsl:when test="contains($pOrderElecType,'Forex Limit')">
        <xsl:value-of select="'F'"/>
      </xsl:when>
      <!-- Ne pas mettre avant 'Stop Limit','Limit Or Better','Limit With Or Without','Limit On Close','Forex Limit','Funari','Market With Left Over as Limit'  -->
      <xsl:when test="contains($pOrderElecType,'Limit')">
        <xsl:value-of select="'2'"/>
      </xsl:when>
      <xsl:when test="contains($pOrderElecType,'Stop / Stop Loss')">
        <xsl:value-of select="'3'"/>
      </xsl:when>
      <!-- Ne pas mettre avant 'Limit With Or Without'-->
      <xsl:when test="contains($pOrderElecType,'With Or Without')">
        <xsl:value-of select="'6'"/>
      </xsl:when>
      <xsl:when test="contains($pOrderElecType,'On Basis')">
        <xsl:value-of select="'9'"/>
      </xsl:when>
      <!-- Ne pas mettre avant 'Market On Close','Limit On Close'-->
      <xsl:when test="contains($pOrderElecType,'On Close')">
        <xsl:value-of select="'A'"/>
      </xsl:when>
      <xsl:when test="contains($pOrderElecType,'Forex Previously Quoted')">
        <xsl:value-of select="'H'"/>
      </xsl:when>
      <!-- Ne pas mettre avant 'Forex Previously Quoted'-->
      <xsl:when test="contains($pOrderElecType,'Previously Quoted')">
        <xsl:value-of select="'D'"/>
      </xsl:when>
      <xsl:when test="contains($pOrderElecType,'Previously Indicated')">
        <xsl:value-of select="'E'"/>
      </xsl:when>
      <xsl:when test="contains($pOrderElecType,'Forex Swap')">
        <xsl:value-of select="'G'"/>
      </xsl:when>
      <xsl:when test="contains($pOrderElecType,'Previous Fund Valuation')">
        <xsl:value-of select="'L'"/>
      </xsl:when>
      <xsl:when test="contains($pOrderElecType,'Next Fund Valuation')">
        <xsl:value-of select="'M'"/>
      </xsl:when>
      <xsl:when test="contains($pOrderElecType,'Pegged')">
        <xsl:value-of select="'P'"/>
      </xsl:when>
      <xsl:when test="contains($pOrderElecType,'Counter-order selection')">
        <xsl:value-of select="'Q'"/>
      </xsl:when>
    </xsl:choose>

  </xsl:template>

  <xsl:template name="GetRecordTypeLog">
    <xsl:param name="pRecordType"/>
    <xsl:choose>
      <xsl:when test="$pRecordType='E'">
        <xsl:value-of select ="concat($pRecordType,' (Execution)')"/>
      </xsl:when>
      <xsl:when test="$pRecordType='I'">
        <xsl:value-of select ="concat($pRecordType,' (Intermediation)')"/>
      </xsl:when>
      <xsl:when test="$pRecordType='A'">
        <xsl:value-of select ="concat($pRecordType,' (Allocation)')"/>
      </xsl:when>
      <xsl:when test="string-length($pRecordType) > 0">
        <xsl:value-of select="$pRecordType"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$ConstUnknown"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!--///-->
  <xsl:template name="CcisOrderParty">
    <xsl:param name ="pPartyNumber"/>
    <xsl:param name ="pPartyName"/>
    <xsl:param name ="pXMLRowLeg1"/>
    <xsl:param name ="pXMLTradePartiesAndBrokers"/>

    <xsl:variable name ="vDataNamePrefix">
      <xsl:choose>
        <xsl:when test="$pPartyName=$ConstBuyer">
          <xsl:value-of select="'BY'"/>
        </xsl:when>
        <xsl:when test="$pPartyName=$ConstSeller">
          <xsl:value-of select="'SL'"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name ="vIntentionLeg1" select ="msxsl:node-set($pXMLRowLeg1)/row/data[@name=concat($vDataNamePrefix,'I')]"/>
    <xsl:variable name ="vFrontIdLeg1" select ="msxsl:node-set($pXMLRowLeg1)/row/data[@name=concat($vDataNamePrefix,'FRI')]"/>
    <xsl:variable name ="vFolderIdLeg1" select ="msxsl:node-set($pXMLRowLeg1)/row/data[@name=concat($vDataNamePrefix,'FLI')]"/>
    <!--///-->
    <xsl:variable name ="vTraderLeg1" select ="msxsl:node-set($pXMLRowLeg1)/row/data[@name=concat($vDataNamePrefix,'TR')]"/>
    <xsl:variable name ="vTraderIdentLeg1" select ="msxsl:node-set($pXMLRowLeg1)/row/data[@name=concat($vDataNamePrefix,'TRI')]"/>
    <xsl:variable name ="vXMLSalesLeg1">
      <xsl:call-template name="XMLSale">
        <xsl:with-param name="pIdent">
          <xsl:value-of select ="msxsl:node-set($pXMLRowLeg1)/row/data[@name=concat($vDataNamePrefix,'SLI1')]"/>
        </xsl:with-param>
        <xsl:with-param name="pCoeff">
          <xsl:value-of select ="msxsl:node-set($pXMLRowLeg1)/row/data[@name=concat($vDataNamePrefix,'SLC1')]"/>
        </xsl:with-param>
        <xsl:with-param name="pValue">
          <xsl:value-of select="msxsl:node-set($pXMLRowLeg1)/row/data[@name=concat($vDataNamePrefix,'SL1')]"/>
        </xsl:with-param>
      </xsl:call-template>
      <xsl:call-template name="XMLSale">
        <xsl:with-param name="pIdent">
          <xsl:value-of select ="msxsl:node-set($pXMLRowLeg1)/row/data[@name=concat($vDataNamePrefix,'SLI2')]"/>
        </xsl:with-param>
        <xsl:with-param name="pCoeff">
          <xsl:value-of select ="msxsl:node-set($pXMLRowLeg1)/row/data[@name=concat($vDataNamePrefix,'SLC2')]"/>
        </xsl:with-param>
        <xsl:with-param name="pValue">
          <xsl:value-of select="msxsl:node-set($pXMLRowLeg1)/row/data[@name=concat($vDataNamePrefix,'SL2')]"/>
        </xsl:with-param>
      </xsl:call-template>
      <xsl:call-template name="XMLSale">
        <xsl:with-param name="pIdent">
          <xsl:value-of select ="msxsl:node-set($pXMLRowLeg1)/row/data[@name=concat($vDataNamePrefix,'SLI3')]"/>
        </xsl:with-param>
        <xsl:with-param name="pCoeff">
          <xsl:value-of select ="msxsl:node-set($pXMLRowLeg1)/row/data[@name=concat($vDataNamePrefix,'SLC3')]"/>
        </xsl:with-param>
        <xsl:with-param name="pValue">
          <xsl:value-of select="msxsl:node-set($pXMLRowLeg1)/row/data[@name=concat($vDataNamePrefix,'SL3')]"/>
        </xsl:with-param>
      </xsl:call-template>
      <xsl:call-template name="XMLSale">
        <xsl:with-param name="pIdent">
          <xsl:value-of select ="msxsl:node-set($pXMLRowLeg1)/row/data[@name=concat($vDataNamePrefix,'SLI4')]"/>
        </xsl:with-param>
        <xsl:with-param name="pCoeff">
          <xsl:value-of select ="msxsl:node-set($pXMLRowLeg1)/row/data[@name=concat($vDataNamePrefix,'SLC4')]"/>
        </xsl:with-param>
        <xsl:with-param name="pValue">
          <xsl:value-of select="msxsl:node-set($pXMLRowLeg1)/row/data[@name=concat($vDataNamePrefix,'SL4')]"/>
        </xsl:with-param>
      </xsl:call-template>
      <xsl:call-template name="XMLSale">
        <xsl:with-param name="pIdent">
          <xsl:value-of select ="msxsl:node-set($pXMLRowLeg1)/row/data[@name=concat($vDataNamePrefix,'SLI5')]"/>
        </xsl:with-param>
        <xsl:with-param name="pCoeff">
          <xsl:value-of select ="msxsl:node-set($pXMLRowLeg1)/row/data[@name=concat($vDataNamePrefix,'SLC5')]"/>
        </xsl:with-param>
        <xsl:with-param name="pValue">
          <xsl:value-of select="msxsl:node-set($pXMLRowLeg1)/row/data[@name=concat($vDataNamePrefix,'SL5')]"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <!--///-->
    <xsl:call-template name="CcisParty">
      <xsl:with-param name="pPartyNumber">
        <xsl:value-of select="$pPartyNumber"/>
      </xsl:with-param>
      <xsl:with-param name ="pXMLParty">
        <xsl:copy-of select="msxsl:node-set($pXMLTradePartiesAndBrokers)/party[@name=$pPartyName]"/>
        <xsl:copy-of select="msxsl:node-set($pXMLTradePartiesAndBrokers)/book[@name=$pPartyName]"/>
      </xsl:with-param>
      <xsl:with-param name ="pXMLBroker">
        <xsl:choose>
          <xsl:when test="$pPartyName=$ConstBuyer">
            <xsl:copy-of select="msxsl:node-set($pXMLTradePartiesAndBrokers)/party[@name=$ConstBuyerNegociationBroker or @name=$ConstBuyerExecutingBroker or @name=$ConstBuyerClearingBroker]"/>
          </xsl:when>
          <xsl:when test="$pPartyName=$ConstSeller">
            <xsl:copy-of select="msxsl:node-set($pXMLTradePartiesAndBrokers)/party[@name=$ConstSellerNegociationBroker or @name=$ConstSellerExecutingBroker or @name=$ConstSellerClearingBroker]"/>
          </xsl:when>
        </xsl:choose>
      </xsl:with-param>
      <xsl:with-param name ="pIntention">
        <xsl:value-of select="$vIntentionLeg1"/>
      </xsl:with-param>
      <xsl:with-param name ="pFrontId">
        <xsl:value-of select="$vFrontIdLeg1"/>
      </xsl:with-param>
      <xsl:with-param name ="pFolderId">
        <xsl:value-of select="$vFolderIdLeg1"/>
      </xsl:with-param>
      <xsl:with-param name ="pTrader">
        <xsl:value-of select="$vTraderLeg1"/>
      </xsl:with-param>
      <xsl:with-param name ="pTraderIdent">
        <xsl:value-of select="$vTraderIdentLeg1"/>
      </xsl:with-param>
      <xsl:with-param name ="pXMLSales">
        <xsl:copy-of select="$vXMLSalesLeg1"/>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <!-- 
  FI 20130702 [18798] add pIsMandatoryActor et pIsMandatoryBook 
  FI 20140204 [19566] add pUTI (Unique Trade Identifier)
  -->
  <xsl:template name="CcisParty">
    <xsl:param name ="pPartyNumber"/>
    <xsl:param name ="pIsNCM" select="$ConstTrue"/>
    <xsl:param name ="pXMLParty"/>
    <xsl:param name ="pXMLBroker"/>
    <xsl:param name ="pIntention"/>
    <xsl:param name ="pFrontId"/>
    <xsl:param name ="pFolderId"/>
    <xsl:param name ="pTrader"/>
    <xsl:param name ="pTraderIdent"/>
    <xsl:param name ="pXMLSales"/>
    <xsl:param name ="pIsMandatoryActor" select="$ConstFalse"/>
    <xsl:param name ="pIsMandatoryBook" select="$ConstFalse"/>
    <xsl:param name ="pUTI"/>


    <xsl:variable name ="vIntention" select ="normalize-space($pIntention)"/>
    <xsl:variable name ="vFrontId" select ="normalize-space($pFrontId)"/>
    <xsl:variable name ="vFolderId" select ="normalize-space($pFolderId)"/>
    <!-- FI 20140204 [19566] normalize-space ne doit pas être utilisé (cela supprimerait les éventuels space présents entre le issuer et l'UTI) -->
    <xsl:variable name ="vUTI" select ="$pUTI"/>

    <xsl:variable name ="vTrader" select ="normalize-space($pTrader)"/>
    <xsl:variable name ="vTraderIdent" select ="normalize-space($pTraderIdent)"/>
    <!--///-->
    <!--Actor Cci-->
    <xsl:call-template name="CustomCaptureInfo">
      <xsl:with-param name="pClientId">
        <xsl:value-of select ="concat('tradeHeader_party',$pPartyNumber,'_actor')"/>
      </xsl:with-param>
      <xsl:with-param name="pDataType">
        <xsl:value-of select ="'string'"/>
      </xsl:with-param>
      <xsl:with-param name="pIsMissingMode" select="msxsl:node-set($pXMLParty)/party/@isMissingMode"/>
      <xsl:with-param name="pIsMissingModeSpecified" select ="string-length(msxsl:node-set($pXMLParty)/party/@isMissingMode)> 0"/>
      <xsl:with-param name="pXMLDynamicValue">
        <xsl:copy-of select="msxsl:node-set($pXMLParty)/party/dynamicValue"/>
      </xsl:with-param>
      <xsl:with-param name="pIsMandatory" select="$pIsMandatoryActor"/>
    </xsl:call-template>
    <!--Book Cci-->
    <xsl:call-template name="CustomCaptureInfo">
      <xsl:with-param name="pClientId">
        <xsl:value-of select ="concat('tradeHeader_party',$pPartyNumber,'_book')"/>
      </xsl:with-param>
      <xsl:with-param name="pDataType">
        <xsl:value-of select ="'string'"/>
      </xsl:with-param>
      <xsl:with-param name="pIsMissingMode" select="msxsl:node-set($pXMLParty)/party/@isMissingMode"/>
      <xsl:with-param name="pIsMissingModeSpecified" select ="string-length(msxsl:node-set($pXMLParty)/party/@isMissingMode)> 0"/>
      <xsl:with-param name="pXMLDynamicValue">
        <xsl:copy-of select="msxsl:node-set($pXMLParty)/book/dynamicValue"/>
      </xsl:with-param>
      <xsl:with-param name="pIsMandatory" select="$pIsMandatoryBook"/>
    </xsl:call-template>
    <!--InitiatorReactor Cci-->
    <xsl:call-template name="CustomCaptureInfo">
      <xsl:with-param name="pClientId">
        <xsl:value-of select ="concat('tradeHeader_party',$pPartyNumber,'_initiatorReactor')"/>
      </xsl:with-param>
      <xsl:with-param name="pDataType">
        <xsl:value-of select ="'string'"/>
      </xsl:with-param>
      <xsl:with-param name="pValue">
        <xsl:value-of select="$pIntention"/>
      </xsl:with-param>
    </xsl:call-template>
    <!--ISNCMINI,ISNCMINT,ISNCMFIN Cci-->
    <xsl:call-template name="CustomCaptureInfo">
      <xsl:with-param name="pClientId">
        <xsl:value-of select ="concat('tradeHeader_party',$pPartyNumber,'_ISNCMINI')"/>
      </xsl:with-param>
      <xsl:with-param name="pDataType">
        <xsl:value-of select ="'bool'"/>
      </xsl:with-param>
      <xsl:with-param name="pValue">
        <xsl:copy-of select="$pIsNCM"/>
      </xsl:with-param>
    </xsl:call-template>
    <xsl:call-template name="CustomCaptureInfo">
      <xsl:with-param name="pClientId">
        <xsl:value-of select ="concat('tradeHeader_party',$pPartyNumber,'_ISNCMINT')"/>
      </xsl:with-param>
      <xsl:with-param name="pDataType">
        <xsl:value-of select ="'bool'"/>
      </xsl:with-param>
      <xsl:with-param name="pValue">
        <xsl:copy-of select="$pIsNCM"/>
      </xsl:with-param>
    </xsl:call-template>
    <xsl:call-template name="CustomCaptureInfo">
      <xsl:with-param name="pClientId">
        <xsl:value-of select ="concat('tradeHeader_party',$pPartyNumber,'_ISNCMFIN')"/>
      </xsl:with-param>
      <xsl:with-param name="pDataType">
        <xsl:value-of select ="'bool'"/>
      </xsl:with-param>
      <xsl:with-param name="pValue">
        <xsl:copy-of select="$pIsNCM"/>
      </xsl:with-param>
    </xsl:call-template>
    <!--FrontId Cci-->
    <xsl:call-template name="CustomCaptureInfo">
      <xsl:with-param name="pClientId">
        <xsl:value-of select ="concat('tradeHeader_party',$pPartyNumber,'_frontId')"/>
      </xsl:with-param>
      <xsl:with-param name="pDataType">
        <xsl:value-of select ="'string'"/>
      </xsl:with-param>
      <xsl:with-param name="pValue">
        <xsl:value-of select="$vFrontId"/>
      </xsl:with-param>
    </xsl:call-template>
    <!--Folder Cci-->
    <xsl:call-template name="CustomCaptureInfo">
      <xsl:with-param name="pClientId">
        <xsl:value-of select ="concat('tradeHeader_party',$pPartyNumber,'_folder')"/>
      </xsl:with-param>
      <xsl:with-param name="pDataType">
        <xsl:value-of select ="'string'"/>
      </xsl:with-param>
      <xsl:with-param name="pValue">
        <xsl:value-of select="$vFolderId"/>
      </xsl:with-param>
    </xsl:call-template>
    <!--Trader Cci-->
    <xsl:call-template name="CustomCaptureInfo">
      <xsl:with-param name="pClientId">
        <xsl:value-of select ="concat('tradeHeader_party',$pPartyNumber,'_trader1_identifier')"/>
      </xsl:with-param>
      <xsl:with-param name="pDataType">
        <xsl:value-of select ="'string'"/>
      </xsl:with-param>
      <xsl:with-param name="pXMLDynamicValue">
        <xsl:call-template name="SQLTraderSales">
          <xsl:with-param name="pValue">
            <xsl:value-of select="$vTrader"/>
          </xsl:with-param>
          <xsl:with-param name="pValueIdent">
            <xsl:value-of select="$vTraderIdent"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:with-param>
    </xsl:call-template>
    <!--Sales Cci-->
    <xsl:for-each select='msxsl:node-set($pXMLSales)/sale'>
      <xsl:variable name ="vSalePosition" select="position()"/>

      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId">
          <xsl:value-of select ="concat('tradeHeader_party',$pPartyNumber,'_sales',$vSalePosition,'_identifier')"/>
        </xsl:with-param>
        <xsl:with-param name="pDataType">
          <xsl:value-of select ="'string'"/>
        </xsl:with-param>
        <xsl:with-param name="pXMLDynamicValue">
          <xsl:call-template name="SQLTraderSales">
            <xsl:with-param name="pValue">
              <xsl:value-of select="normalize-space(text())"/>
            </xsl:with-param>
            <xsl:with-param name="pValueIdent">
              <xsl:value-of select="normalize-space(@ident)"/>
            </xsl:with-param>
          </xsl:call-template>
        </xsl:with-param>
      </xsl:call-template>
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId">
          <xsl:value-of select ="concat('tradeHeader_party',$pPartyNumber,'_sales',$vSalePosition,'_factor')"/>
        </xsl:with-param>
        <xsl:with-param name="pDataType">
          <xsl:value-of select ="'decimal'"/>
        </xsl:with-param>
        <xsl:with-param name="pValue">
          <xsl:value-of select="normalize-space(@coeff) div 100"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:for-each>
    <!--Brokers Cci-->
    <xsl:for-each select='msxsl:node-set($pXMLBroker)/party'>
      <xsl:variable name ="vBrokerPosition" select="position()"/>

      <xsl:if test="string-length(dynamicValue) > 0 or string-length(value) > 0">
        <!--Broker Cci-->
        <xsl:call-template name="CustomCaptureInfo">
          <xsl:with-param name="pClientId">
            <xsl:value-of select ="concat('tradeHeader_party',$pPartyNumber,'_broker',$vBrokerPosition,'_actor')"/>
          </xsl:with-param>
          <xsl:with-param name="pDataType">
            <xsl:value-of select ="'string'"/>
          </xsl:with-param>
          <xsl:with-param name="pXMLDynamicValue">
            <xsl:copy-of select="dynamicValue"/>
          </xsl:with-param>
        </xsl:call-template>
        <!--Broker FrontId Cci-->
        <xsl:call-template name="CustomCaptureInfo">
          <xsl:with-param name="pClientId">
            <xsl:value-of select ="concat('tradeHeader_party',$pPartyNumber,'_broker',$vBrokerPosition,'_frontId')"/>
          </xsl:with-param>
          <xsl:with-param name="pDataType">
            <xsl:value-of select ="'string'"/>
          </xsl:with-param>
        </xsl:call-template>
        <!--Broker Trader Cci-->
        <xsl:call-template name="CustomCaptureInfo">
          <xsl:with-param name="pClientId">
            <xsl:value-of select ="concat('tradeHeader_party',$pPartyNumber,'_broker',$vBrokerPosition,'_trader1_identifier')"/>
          </xsl:with-param>
          <xsl:with-param name="pDataType">
            <xsl:value-of select ="'string'"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:if>
    </xsl:for-each>
    <!-- RD 20140717 [19689] Problème de report de la 3.7 vers la NV-->
    <!--UTI Cci-->
    <xsl:call-template name="CustomCaptureInfo">
      <xsl:with-param name="pClientId">
        <xsl:value-of select ="concat('tradeHeader_party',$pPartyNumber,'_partyTradeIdentifier_UTI_value')"/>
      </xsl:with-param>
      <xsl:with-param name="pDataType">
        <xsl:value-of select ="'string'"/>
      </xsl:with-param>
      <xsl:with-param name="pValue">
        <xsl:value-of select="$vUTI"/>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <!--FI 20120704 add parameter pCciPrefixFamily and pFamily-->
  <!--FI 20130703 [18798] add parameter pIsAllowDataMissing-->
  <xsl:template name="CcisLeg">
    <xsl:param name ="pLegNumber"/>
    <xsl:param name ="pXMLLegRow"/>
    <xsl:param name ="pCciPrefix"/>
    <xsl:param name ="pCciPrefixFamily"/>
    <xsl:param name ="pRecordTypeLeg1"/>
    <xsl:param name ="pMarketLeg1"/>
    <xsl:param name ="pMarketColumnLeg1"/>
    <xsl:param name ="pFamily"/>
    <xsl:param name ="pClearingBusinessDate"/>
    <xsl:param name ="pIsAllowDataMissing"/>

    <xsl:variable name ="vLegPrefix">
      <xsl:value-of select ="concat($pCciPrefix,$pCciPrefixFamily,$pLegNumber)"/>
    </xsl:variable>
    <xsl:variable name ="vAssetCode" select ="normalize-space(msxsl:node-set($pXMLLegRow)/row/data[@name='ASC'])"/>
    <xsl:variable name ="vAssetCodeIdent" select ="normalize-space(msxsl:node-set($pXMLLegRow)/row/data[@name='ASCI'])"/>
    <xsl:variable name ="vAssetCodeColumn">
      <xsl:call-template name="GetAssetIdentColumn">
        <xsl:with-param name="pIdent">
          <xsl:value-of select="$vAssetCodeIdent"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <!--/// -->
    <!-- FI 20170404 [23039] Mise en commentaire -->
    <!--<xsl:variable name ="vMarketForAssetCodeIdentMARKET">
      <xsl:if test="$vAssetCodeIdent = 'MARKET'">
        <xsl:value-of select="$pMarketLeg1"/>
      </xsl:if>
    </xsl:variable>-->
    <!--/// -->
    <xsl:variable name ="vDerivativeContract" select ="normalize-space(msxsl:node-set($pXMLLegRow)/row/data[@name='DVT'])"/>
    <xsl:variable name ="vDerivativeContractColumn">
      <xsl:call-template name="GetDerivativeContractIdentColumn">
        <xsl:with-param name="pIdent">
          <xsl:value-of select="normalize-space(msxsl:node-set($pXMLLegRow)/row/data[@name='DVTI'])"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vDerivativeContractVersion" select ="normalize-space(msxsl:node-set($pXMLLegRow)/row/data[@name='DVTV'])"/>
    <xsl:variable name ="vDerivativeContractCategory" select ="normalize-space(msxsl:node-set($pXMLLegRow)/row/data[@name='DVTC'])"/>

    <!-- Asset Cci-->
    <xsl:call-template name="CustomCaptureInfo">
      <xsl:with-param name="pClientId">
        <xsl:value-of select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_Instrmt_ID')"/>
      </xsl:with-param>
      <xsl:with-param name="pDataType">
        <xsl:value-of select ="'string'"/>
      </xsl:with-param>
      <xsl:with-param name="pXMLDynamicValue">
        <xsl:choose>
          <xsl:when test="$pFamily='ETD'">
            <xsl:call-template name="ETD_SQLInstrumentAndAsset">
              <xsl:with-param name="pBusinessDate" select="$pClearingBusinessDate"/>
              <xsl:with-param name="pAssetCode" select="$vAssetCode"/>
              <xsl:with-param name="pAssetColumn" select="$vAssetCodeColumn"/>
              <!-- FI 20170404 [23039] select="$vMarket" -->
              <!--<xsl:with-param name="pMarket" select="$vMarketForAssetCodeIdentMARKET"/>-->
              <xsl:with-param name="pMarket" select="$pMarketLeg1"/>
              <xsl:with-param name="pMarketColumn" select="$pMarketColumnLeg1"/>
              <xsl:with-param name="pResultColumn" select="'ASSET_IDENTIFIER'"/>
              <xsl:with-param name="pIsMandatory" select="$ConstFalse"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pFamily='ESE'">
            <xsl:call-template name="ESE_SQLAsset">
              <xsl:with-param name="pAssetCode" select="$vAssetCode"/>
              <xsl:with-param name="pAssetColumn" select="$vAssetCodeColumn"/>
              <!-- FI 20170404 [23039] add pMarket, pMarketColumn  -->
              <xsl:with-param name="pMarket" select="$pMarketLeg1"/>
              <xsl:with-param name="pMarketColumn" select="$pMarketColumnLeg1"/>
              <xsl:with-param name="pIsMandatory" select="$ConstTrue"/>
            </xsl:call-template>
            
          </xsl:when>
        </xsl:choose>
      </xsl:with-param>
    </xsl:call-template>
    <!-- Qty Cci-->
    <xsl:call-template name="CustomCaptureInfo">
      <xsl:with-param name="pClientId">
        <xsl:value-of select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_LastQty')"/>
      </xsl:with-param>
      <xsl:with-param name="pDataType">
        <xsl:value-of select ="'integer'"/>
      </xsl:with-param>
      <xsl:with-param name="pRegex">
        <xsl:value-of select ="'RegexPositiveInteger'"/>
      </xsl:with-param>
      <xsl:with-param name="pValue">
        <xsl:choose>
          <xsl:when test="$pRecordTypeLeg1 = 'A'">
            <!--AllocationQuantity-->
            <xsl:value-of select="normalize-space(msxsl:node-set($pXMLLegRow)/row/data[@name='ALQ'])"/>
          </xsl:when>
          <xsl:otherwise>
            <!--LegQuantity-->
            <xsl:value-of select="normalize-space(msxsl:node-set($pXMLLegRow)/row/data[@name='LQTY'])"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:with-param>
      <!--RD 20160329 [22007] add pIsMandatory à true-->
      <xsl:with-param name="pIsMandatory">
        <xsl:value-of select ="$ConstTrue"/>
      </xsl:with-param>
    </xsl:call-template>
    <!-- Price Cci-->
    <xsl:call-template name="CustomCaptureInfo">
      <xsl:with-param name="pClientId">
        <xsl:value-of select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_LastPx')"/>
      </xsl:with-param>
      <xsl:with-param name="pDataType">
        <xsl:value-of select ="'decimal'"/>
      </xsl:with-param>
      <xsl:with-param name="pValue">
        <xsl:value-of select="normalize-space(msxsl:node-set($pXMLLegRow)/row/data[@name='PRI'])"/>
      </xsl:with-param>
      <!--RD 20160329 [22007] add pIsMandatory à true-->
      <xsl:with-param name="pIsMandatory">
        <xsl:value-of select ="$ConstTrue"/>
      </xsl:with-param>
    </xsl:call-template>
    <!-- For Allocation only-->
    <xsl:if test="$pRecordTypeLeg1 = 'A'">
      <!--RD 20150610 [21099] Alimenter le Cci FIXML_TrdCapRpt_RptSide_PosEfct uniquement si la donnée ALPE existe dans le fichier d'entrée-->
      <xsl:variable name="posEfct" select ="normalize-space(msxsl:node-set($pXMLLegRow)/row/data[@name='ALPE'])"/>
      <xsl:if test="$posEfct">
        <!-- PositionEffect Cci-->
        <xsl:call-template name="CustomCaptureInfo">
          <xsl:with-param name="pClientId">
            <xsl:value-of select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_RptSide_PosEfct')"/>
          </xsl:with-param>
          <xsl:with-param name="pDataType">
            <xsl:value-of select ="'string'"/>
          </xsl:with-param>
          <xsl:with-param name="pXMLDynamicValue">
            <!--FI 20120709 [17991] call-template SQLEnumValue -->
            <xsl:call-template name="SQLEnumValue2">
              <xsl:with-param name="pCode" select="'PositionEffectEnum'"/>
              <xsl:with-param name="pExtValue" select="$posEfct"/>
            </xsl:call-template>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:if>
    </xsl:if>
    <!-- Si l'Asset est renseigné, on ne renseigne pas les cci : Contract, Maturity, PutCall et Strike-->
    <!-- FI 20120704 [17991] only on ETD -->
    <xsl:if test="string-length($vAssetCode)=0 and $pFamily='ETD' ">
      <!--FI 20131126 [19271] add mmy -->
      <xsl:variable name ="mmy" select ="normalize-space(msxsl:node-set($pXMLLegRow)/row/data[@name='DVTM'])"/>
      <!-- Contract Cci-->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId">
          <xsl:value-of select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_Instrmt_Sym')"/>
        </xsl:with-param>
        <xsl:with-param name="pDataType">
          <xsl:value-of select ="'string'"/>
        </xsl:with-param>
        <!-- RD 20140409 [19816] use template ovrSQLDerivativeContract-->
        <xsl:with-param name="pXMLDynamicValue">
          <!-- FI 20161005 [XXXXX] Call SQLDerivativeContract -->
          <xsl:call-template name="SQLDerivativeContract">
          <!--<xsl:call-template name="ovrSQLDerivativeContract">-->
            <xsl:with-param name="pBusinessDate" select="$pClearingBusinessDate"/>
            <xsl:with-param name="pDerivativeContract" select="$vDerivativeContract"/>
            <xsl:with-param name="pDerivativeContractColumn" select="$vDerivativeContractColumn"/>
            <xsl:with-param name="pDerivativeContractVersion" select="$vDerivativeContractVersion"/>
            <xsl:with-param name="pMarket" select="$pMarketLeg1"/>
            <xsl:with-param name="pMarketColumn" select="$pMarketColumnLeg1"/>
            <xsl:with-param name="pDerivativeContractCategory" select="$vDerivativeContractCategory"/>
          </xsl:call-template>
        </xsl:with-param>
        <!--FI 20130702 [18798] add pIsMandatory à true de manière à intégrer les trades lorsque le DC est non renseigné
        Si la donnée est vide => cela génère un message d'erreur dans le cci => le trade passe en mode Incomplet
        Ce comportement s'applique si $pIsAllowDataMissing
        -->
        <xsl:with-param name="pIsMandatory">
          <xsl:value-of select ="$ConstTrue"/>
        </xsl:with-param>
        <!-- FI 20130703 [18798] 
        Si $pIsAllowDataMissing vaut true
        lorsque le DC est non renseigné ou incorrecte Spheres® génère unn trade incomplet 
        -->
        <xsl:with-param name="pIsMissingMode" select ="$pIsAllowDataMissing"/>
        <xsl:with-param name="pIsMissingModeSpecified" select ="$ConstTrue"/>
      </xsl:call-template>
      <!-- Maturity Cci-->
      <!-- FI 20131126 [19271] upd cci MMY -->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId">
          <xsl:value-of select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_Instrmt_MMY')"/>
        </xsl:with-param>
        <xsl:with-param name="pDataType">
          <xsl:value-of select ="'string'"/>
        </xsl:with-param>
        <xsl:with-param name="pValue">
          <!--<xsl:value-of select="normalize-space(msxsl:node-set($pXMLLegRow)/row/data[@name='DVTM'])"/>-->
          <xsl:choose>
            <xsl:when test ="contains($mmy,'@')">
              <xsl:value-of select="substring-before($mmy,'@')"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$mmy"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:with-param>
      </xsl:call-template>
      <!-- MaturityDate Cci-->
      <!-- FI [19271] add cci MatDt -->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId">
          <xsl:value-of select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_Instrmt_MatDt')"/>
        </xsl:with-param>
        <xsl:with-param name="pDataType">
          <xsl:value-of select ="'date'"/>
        </xsl:with-param>
        <xsl:with-param name="pValue">
          <xsl:choose>
            <xsl:when test ="contains($mmy,'@')">
              <xsl:value-of select="substring-after($mmy,'@')"/>
            </xsl:when>
          </xsl:choose>
        </xsl:with-param>
      </xsl:call-template>
      <!-- For Option only-->
      <xsl:if test="$vDerivativeContractCategory = 'O'">
        <!-- Type Cci-->
        <xsl:call-template name="CustomCaptureInfo">
          <xsl:with-param name="pClientId">
            <xsl:value-of select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_Instrmt_PutCall')"/>
          </xsl:with-param>
          <xsl:with-param name="pDataType">
            <xsl:value-of select ="'string'"/>
          </xsl:with-param>
          <xsl:with-param name="pXMLDynamicValue">
            <!--FI 20120709 [17991] call-template SQLEnumValue -->
            <xsl:call-template name="SQLEnumValue2">
              <xsl:with-param name="pCode" select="'PutOrCallEnum'"/>
              <xsl:with-param name="pExtValue" select="normalize-space(msxsl:node-set($pXMLLegRow)/row/data[@name='DVTO'])"/>
            </xsl:call-template>
          </xsl:with-param>
        </xsl:call-template>
        <!-- Strike Cci-->
        <xsl:call-template name="CustomCaptureInfo">
          <xsl:with-param name="pClientId">
            <xsl:value-of select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_Instrmt_StrkPx')"/>
          </xsl:with-param>
          <xsl:with-param name="pDataType">
            <xsl:value-of select ="'string'"/>
          </xsl:with-param>
          <xsl:with-param name="pValue">
            <xsl:value-of select="normalize-space(msxsl:node-set($pXMLLegRow)/row/data[@name='DVTS'])"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <!--RD 20130822 FeeCalculation Project / Gestion da la valeur Businessdate
    - Ajout du parametre pBusinessDate
    - Renommer le parametre pFeesDate en pTransactionDate-->
  <xsl:template name="CcisFees">
    <xsl:param name ="pTransactionDate"/>
    <xsl:param name ="pBusinessDate"/>
    <xsl:param name ="pXMLFees"/>

    <xsl:for-each select='msxsl:node-set($pXMLFees)/fee'>
      <xsl:variable name ="vFeePosition" select="position()"/>

      <!-- FeeSchedule Cci-->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId">
          <xsl:value-of select ="concat('otherPartyPayment',$vFeePosition,'_paymentSource_feeSchedule')"/>
        </xsl:with-param>
        <xsl:with-param name="pDataType">
          <xsl:value-of select ="'string'"/>
        </xsl:with-param>
      </xsl:call-template>
      <!-- Payer Cci-->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId">
          <xsl:value-of select ="concat('otherPartyPayment',$vFeePosition,'_payer')"/>
        </xsl:with-param>
        <xsl:with-param name="pDataType">
          <xsl:value-of select ="'string'"/>
        </xsl:with-param>
        <xsl:with-param name="pXMLDynamicValue">
          <xsl:copy-of select="payer/dynamicValue"/>
        </xsl:with-param>
        <xsl:with-param name="pIsMandatory" select="$ConstTrue"/>
      </xsl:call-template>
      <!-- Receiver Cci-->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId">
          <xsl:value-of select ="concat('otherPartyPayment',$vFeePosition,'_receiver')"/>
        </xsl:with-param>
        <xsl:with-param name="pDataType">
          <xsl:value-of select ="'string'"/>
        </xsl:with-param>
        <xsl:with-param name="pXMLDynamicValue">
          <xsl:copy-of select="receiver/dynamicValue"/>
        </xsl:with-param>
        <xsl:with-param name="pIsMandatory" select="$ConstTrue"/>
      </xsl:call-template>
      <!-- Date Cci-->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId">
          <xsl:value-of select ="concat('otherPartyPayment',$vFeePosition,'_paymentDate_unadjustedDate')"/>
        </xsl:with-param>
        <xsl:with-param name="pDataType">
          <xsl:value-of select ="'date'"/>
        </xsl:with-param>
        <xsl:with-param name="pValue">
          <!--PL 20130722 FeeCalculation Project2-->
          <!--<xsl:value-of select="normalize-space($pTransactionDate)"/>-->
          <xsl:choose>
            <!--RD 20130822 FeeCalculation Project
            default:
            =======
              - ETD: date de compensation + 1JO sur le BC du Market du DC. 
              - OTC: date de transaction
            NB: La date sera calculée à l'intégration du trade dans le code C# (voir la méthode CciPayment.PaymentDateInitialize )
            
            TransactDate: Date de transaction spécifiée sur la ligne
            ============
            
            BusinessDate: 
            ============
              - ETD: date de compensation spécifiée sur la ligne. 
              - OTC: date de transaction car la date de compensation n'est pas spécifiée
            
            NextBusinessDate: Idem que "default"
            ================
            
            Dans tous les autres cas: Date de transaction spécifiée sur la ligne
            ========================            
            -->
            <xsl:when test="@relativeto='default'">
              <xsl:value-of select="'default'"/>
            </xsl:when>
            <!--RD 20130822 FeeCalculation Project / Gestion da la valeur TransactDate-->
            <xsl:when test="@relativeto='TransactDate'">
              <xsl:value-of select="normalize-space($pTransactionDate)"/>
            </xsl:when>
            <!--RD 20130822 FeeCalculation Project / Gestion da la valeur BusinessDate-->
            <xsl:when test="@relativeto='BusinessDate' and string-length($pBusinessDate) > 0">
              <xsl:value-of select="normalize-space($pBusinessDate)"/>
            </xsl:when>
            <!--RD 20130822 FeeCalculation Project / Gestion da la valeur NextBusinessDate-->
            <xsl:when test="@relativeto='NextBusinessDate'">
              <xsl:value-of select="'default'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="normalize-space($pTransactionDate)"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:with-param>
      </xsl:call-template>
      <!-- Ajustement Cci-->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId">
          <xsl:value-of select ="concat('otherPartyPayment',$vFeePosition,'_paymentDate_dateAdjustments_bDC')"/>
        </xsl:with-param>
        <xsl:with-param name="pDataType">
          <xsl:value-of select ="'string'"/>
        </xsl:with-param>
        <xsl:with-param name="pValue"/>
      </xsl:call-template>
      <!-- Type Cci-->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId">
          <xsl:value-of select ="concat('otherPartyPayment',$vFeePosition,'_paymentType')"/>
        </xsl:with-param>
        <xsl:with-param name="pDataType">
          <xsl:value-of select ="'string'"/>
        </xsl:with-param>
        <xsl:with-param name="pValue">
          <xsl:value-of select="normalize-space(@type)"/>
        </xsl:with-param>
      </xsl:call-template>
      <!-- Amount Cci-->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId">
          <xsl:value-of select ="concat('otherPartyPayment',$vFeePosition,'_paymentAmount_amount')"/>
        </xsl:with-param>
        <xsl:with-param name="pDataType">
          <xsl:value-of select ="'decimal'"/>
        </xsl:with-param>
        <xsl:with-param name="pValue">
          <xsl:value-of select="normalize-space(@amount)"/>
        </xsl:with-param>
      </xsl:call-template>
      <!-- Currency Cci-->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId">
          <xsl:value-of select ="concat('otherPartyPayment',$vFeePosition,'_paymentAmount_currency')"/>
        </xsl:with-param>
        <xsl:with-param name="pDataType">
          <xsl:value-of select ="'string'"/>
        </xsl:with-param>
        <xsl:with-param name="pValue">
          <xsl:value-of select="normalize-space(@currency)"/>
        </xsl:with-param>
      </xsl:call-template>
      <!-- SettlementInformation Cci-->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId">
          <xsl:value-of select ="concat('otherPartyPayment',$vFeePosition,'_settlementInformation')"/>
        </xsl:with-param>
        <xsl:with-param name="pDataType">
          <xsl:value-of select ="'string'"/>
        </xsl:with-param>
        <xsl:with-param name="pValue">
          <xsl:value-of select="normalize-space(@information)"/>
        </xsl:with-param>
      </xsl:call-template>
      <!-- FeeInvoicing Cci-->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId">
          <xsl:value-of select ="concat('otherPartyPayment',$vFeePosition,'_paymentSource_feeInvoicing')"/>
        </xsl:with-param>
        <xsl:with-param name="pDataType">
          <xsl:value-of select ="'bool'"/>
        </xsl:with-param>
        <xsl:with-param name="pValue">
          <xsl:value-of select="normalize-space(@invoicing)"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:for-each>

  </xsl:template>
  <xsl:template name="CcisExtends">
    <xsl:param name ="pXMLExtends"/>
    <xsl:param name="pInstrumentIdentifier"/>
    <xsl:param name="pAssetCode"/>
    <xsl:param name="pAssetColumn"/>
    <xsl:param name="pMarket"/>
    <xsl:param name="pMarketColumn"/>
    <xsl:param name="pClearingBusinessDate"/>

    <xsl:for-each select='msxsl:node-set($pXMLExtends)/extend'>
      <xsl:variable name ="vExtendPosition" select="position()"/>
      <!-- 
      La structure de l'élément "extend" issu du Parsing est la suivante:
        XTI (DEFINEEXTEND.IDENTIFIER) + XTD1 (DEFINEEXTENDDET.IDENTIFIER) + XTVL (valeur)
      La structure du "clientId" est la suivante: 
        "tradeExtends_" + DEFINEEXTENDDET.IDDEFINEEXTENDDET + "_" + DEFINEEXTENDDET.IDENTIFIER
      Provisoirement, on met dans "clientId" ce qui suit: 
        "tradeExtends_" + -Position() + "_" + XTI + "_" + XTDI
      Le "-Position()" sert pour deux choses:
        1- le "-" c'est pour représenter un Extend inexistant dans la base de donnée (IDDEFINEEXTENDDET<0)
        1- Le "Position()" pour avoir le document XML du trade les valeurs des Extends
        
      En réalité "clientId" est alimenté avec la vrai valeur calculée dans "dynamicAttrib"
      -->
      <customCaptureInfo clientId="{concat('tradeExtends_-',$vExtendPosition,'_',normalize-space(@identifier),'_',normalize-space(@detail))}" dataType="string">
        <dynamicAttribs>
          <dynamicAttrib name="clientId">
            <xsl:call-template name="SQLExtendAttribute">
              <xsl:with-param name="pExtIdentifier" select="@identifier"/>
              <xsl:with-param name="pExtDet" select="@detail"/>
              <xsl:with-param name="pResultColumn" select="'CLIENTID'"/>
              <xsl:with-param name="pInstrumentIdentifier" select="$pInstrumentIdentifier"/>
              <xsl:with-param name="pAssetCode" select="$pAssetCode"/>
              <xsl:with-param name="pAssetColumn" select="$pAssetColumn"/>
              <xsl:with-param name="pMarket" select="$pMarket"/>
              <xsl:with-param name="pMarketColumn" select="$pMarketColumn"/>
              <xsl:with-param name="pClearingBusinessDate" select="$pClearingBusinessDate"/>
            </xsl:call-template>
          </dynamicAttrib >
          <dynamicAttrib name="dataType">
            <xsl:call-template name="SQLExtendAttribute">
              <xsl:with-param name="pExtIdentifier" select="@identifier"/>
              <xsl:with-param name="pExtDet" select="@detail"/>
              <xsl:with-param name="pResultColumn" select="'DATATYPE'"/>
              <xsl:with-param name="pInstrumentIdentifier" select="$pInstrumentIdentifier"/>
              <xsl:with-param name="pAssetCode" select="$pAssetCode"/>
              <xsl:with-param name="pAssetColumn" select="$pAssetColumn"/>
              <xsl:with-param name="pMarket" select="$pMarket"/>
              <xsl:with-param name="pMarketColumn" select="$pMarketColumn"/>
              <xsl:with-param name="pClearingBusinessDate" select="$pClearingBusinessDate"/>
            </xsl:call-template>
          </dynamicAttrib >
          <dynamicAttrib name="mandatory">
            <xsl:call-template name="SQLExtendAttribute">
              <xsl:with-param name="pExtIdentifier" select="@identifier"/>
              <xsl:with-param name="pExtDet" select="@detail"/>
              <xsl:with-param name="pResultColumn" select="'ISMANDATORY'"/>
              <xsl:with-param name="pInstrumentIdentifier" select="$pInstrumentIdentifier"/>
              <xsl:with-param name="pAssetCode" select="$pAssetCode"/>
              <xsl:with-param name="pAssetColumn" select="$pAssetColumn"/>
              <xsl:with-param name="pMarket" select="$pMarket"/>
              <xsl:with-param name="pMarketColumn" select="$pMarketColumn"/>
              <xsl:with-param name="pClearingBusinessDate" select="$pClearingBusinessDate"/>
            </xsl:call-template>
          </dynamicAttrib >
          <dynamicAttrib name="regex">
            <xsl:call-template name="SQLExtendAttribute">
              <xsl:with-param name="pExtIdentifier" select="@identifier"/>
              <xsl:with-param name="pExtDet" select="@detail"/>
              <xsl:with-param name="pResultColumn" select="'REGULAREXPRESSION'"/>
              <xsl:with-param name="pInstrumentIdentifier" select="$pInstrumentIdentifier"/>
              <xsl:with-param name="pAssetCode" select="$pAssetCode"/>
              <xsl:with-param name="pAssetColumn" select="$pAssetColumn"/>
              <xsl:with-param name="pMarket" select="$pMarket"/>
              <xsl:with-param name="pMarketColumn" select="$pMarketColumn"/>
              <xsl:with-param name="pClearingBusinessDate" select="$pClearingBusinessDate"/>
            </xsl:call-template>
          </dynamicAttrib >
          <dynamicAttrib name="defaultValue">
            <xsl:call-template name="SQLExtendAttribute">
              <xsl:with-param name="pExtIdentifier" select="@identifier"/>
              <xsl:with-param name="pExtDet" select="@detail"/>
              <xsl:with-param name="pResultColumn" select="'DEFAULTVALUE'"/>
              <xsl:with-param name="pInstrumentIdentifier" select="$pInstrumentIdentifier"/>
              <xsl:with-param name="pAssetCode" select="$pAssetCode"/>
              <xsl:with-param name="pAssetColumn" select="$pAssetColumn"/>
              <xsl:with-param name="pMarket" select="$pMarket"/>
              <xsl:with-param name="pMarketColumn" select="$pMarketColumn"/>
              <xsl:with-param name="pClearingBusinessDate" select="$pClearingBusinessDate"/>
            </xsl:call-template>
          </dynamicAttrib >
        </dynamicAttribs>

        <xsl:choose>
          <xsl:when test="string-length(@value) > 0">
            <value>
              <xsl:value-of select ="@value"/>
            </value >
          </xsl:when>
          <xsl:otherwise>
            <value/>
          </xsl:otherwise>
        </xsl:choose>
      </customCaptureInfo>

    </xsl:for-each>

  </xsl:template>

  <xsl:template name="XMLSale">
    <xsl:param name="pIdent"/>
    <xsl:param name="pCoeff"/>
    <xsl:param name="pValue"/>
    <xsl:if test="string-length(normalize-space($pValue)) > 0">
      <sale ident="{$pIdent}" coeff="{$pCoeff}">
        <xsl:value-of select="$pValue"/>
      </sale>
    </xsl:if>
  </xsl:template>

  <xsl:template name="XMLFee">
    <xsl:param name="pAmount"/>
    <xsl:param name="pCurrency"/>
    <xsl:param name="pXMLTradePartiesAndBrokers"/>
    <xsl:param name="pPayer"/>
    <xsl:param name="pReceiver"/>
    <xsl:param name="pType"/>
    <xsl:param name="pInformation"/>
    <xsl:if test="string-length($pPayer) > 0">
      <!--PL 20130722 FeeCalculation Project2-->
      <!--<fee amount="{$pAmount}" currency="{$pCurrency}" type="{$pType}" information="{$pInformation}" invoicing="{$ConstTrue}">-->
      <!--<xsl:when test="contains($pOrderElecType,'Funari')">-->
      <xsl:variable name="vInvoicing">
        <xsl:choose>
          <xsl:when test="contains(normalize-space($pInformation),'-perinv no')">
            <xsl:value-of select="$ConstFalse"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$ConstTrue"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="vRelativeTo">
        <xsl:choose>
          <xsl:when test="contains($pInformation,'-relto')">
            <xsl:value-of select="normalize-space(substring-before(substring-after($pInformation,'-relto'),'-'))"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="''"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="vInformation">
        <xsl:choose>
          <xsl:when test="contains($pInformation,'-stlinf')">
            <xsl:value-of select="normalize-space(substring-before(substring-after($pInformation,'-stlinf'),'-'))"/>
          </xsl:when>
          <xsl:when test="contains($pInformation,'-')">
            <xsl:value-of select="''"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$pInformation"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <fee amount="{$pAmount}" currency="{$pCurrency}" type="{$pType}" information="{$vInformation}" relativeto="{$vRelativeTo}" invoicing="{$vInvoicing}">
        <payer>
          <xsl:copy-of select ="msxsl:node-set($pXMLTradePartiesAndBrokers)/party[normalize-space(parsingValue)=$pPayer][1]/dynamicValue"/>
        </payer>
        <receiver>
          <xsl:copy-of select ="msxsl:node-set($pXMLTradePartiesAndBrokers)/party[normalize-space(parsingValue)=$pReceiver][1]/dynamicValue"/>
        </receiver>
      </fee>
    </xsl:if>
  </xsl:template>

  <xsl:template name="XMLFees">
    <xsl:param name ="pXMLRow"/>
    <xsl:param name ="pXMLTradePartiesAndBrokers"/>
    <xsl:param name ="pRecordTypeLeg1"/>

    <xsl:variable name="vRow" select="msxsl:node-set($pXMLRow)/row"/>

    <xsl:call-template name="XMLFee">
      <xsl:with-param name="pAmount">
        <xsl:value-of select ="$vRow/data[@name='MFE1']"/>
      </xsl:with-param>
      <xsl:with-param name="pCurrency">
        <xsl:value-of select ="$vRow/data[@name='CFE1']"/>
      </xsl:with-param>
      <xsl:with-param name="pXMLTradePartiesAndBrokers">
        <xsl:copy-of select ="$pXMLTradePartiesAndBrokers"/>
      </xsl:with-param>
      <xsl:with-param name="pPayer">
        <xsl:value-of select="$vRow/data[@name='PFE1']"/>
      </xsl:with-param>
      <xsl:with-param name="pReceiver">
        <xsl:value-of select ="$vRow/data[@name='RFE1']"/>
      </xsl:with-param>
      <xsl:with-param name="pType">
        <xsl:value-of select ="$vRow/data[@name='TFE1']"/>
      </xsl:with-param>
      <xsl:with-param name="pInformation">
        <xsl:value-of select="$vRow/data[@name='IFE1']"/>
      </xsl:with-param>
    </xsl:call-template>
    <xsl:call-template name="XMLFee">
      <xsl:with-param name="pAmount">
        <xsl:value-of select ="$vRow/data[@name='MFE2']"/>
      </xsl:with-param>
      <xsl:with-param name="pCurrency">
        <xsl:value-of select ="$vRow/data[@name='CFE2']"/>
      </xsl:with-param>
      <xsl:with-param name="pXMLTradePartiesAndBrokers">
        <xsl:copy-of select ="$pXMLTradePartiesAndBrokers"/>
      </xsl:with-param>
      <xsl:with-param name="pPayer">
        <xsl:value-of select="$vRow/data[@name='PFE2']"/>
      </xsl:with-param>
      <xsl:with-param name="pReceiver">
        <xsl:value-of select ="$vRow/data[@name='RFE2']"/>
      </xsl:with-param>
      <xsl:with-param name="pType">
        <xsl:value-of select ="$vRow/data[@name='TFE2']"/>
      </xsl:with-param>
      <xsl:with-param name="pInformation">
        <xsl:value-of select="$vRow/data[@name='IFE2']"/>
      </xsl:with-param>
    </xsl:call-template>
    <xsl:call-template name="XMLFee">
      <xsl:with-param name="pAmount">
        <xsl:value-of select ="$vRow/data[@name='MFE3']"/>
      </xsl:with-param>
      <xsl:with-param name="pCurrency">
        <xsl:value-of select ="$vRow/data[@name='CFE3']"/>
      </xsl:with-param>
      <xsl:with-param name="pXMLTradePartiesAndBrokers">
        <xsl:copy-of select ="$pXMLTradePartiesAndBrokers"/>
      </xsl:with-param>
      <xsl:with-param name="pPayer">
        <xsl:value-of select="$vRow/data[@name='PFE3']"/>
      </xsl:with-param>
      <xsl:with-param name="pReceiver">
        <xsl:value-of select ="$vRow/data[@name='RFE3']"/>
      </xsl:with-param>
      <xsl:with-param name="pType">
        <xsl:value-of select ="$vRow/data[@name='TFE3']"/>
      </xsl:with-param>
      <xsl:with-param name="pInformation">
        <xsl:value-of select="$vRow/data[@name='IFE3']"/>
      </xsl:with-param>
    </xsl:call-template>
    <xsl:call-template name="XMLFee">
      <xsl:with-param name="pAmount">
        <xsl:value-of select ="$vRow/data[@name='MFE4']"/>
      </xsl:with-param>
      <xsl:with-param name="pCurrency">
        <xsl:value-of select ="$vRow/data[@name='CFE4']"/>
      </xsl:with-param>
      <xsl:with-param name="pXMLTradePartiesAndBrokers">
        <xsl:copy-of select ="$pXMLTradePartiesAndBrokers"/>
      </xsl:with-param>
      <xsl:with-param name="pPayer">
        <xsl:value-of select="$vRow/data[@name='PFE4']"/>
      </xsl:with-param>
      <xsl:with-param name="pReceiver">
        <xsl:value-of select ="$vRow/data[@name='RFE4']"/>
      </xsl:with-param>
      <xsl:with-param name="pType">
        <xsl:value-of select ="$vRow/data[@name='TFE4']"/>
      </xsl:with-param>
      <xsl:with-param name="pInformation">
        <xsl:value-of select="$vRow/data[@name='IFE4']"/>
      </xsl:with-param>
    </xsl:call-template>
    <xsl:call-template name="XMLFee">
      <xsl:with-param name="pAmount">
        <xsl:value-of select ="$vRow/data[@name='MFE5']"/>
      </xsl:with-param>
      <xsl:with-param name="pCurrency">
        <xsl:value-of select ="$vRow/data[@name='CFE5']"/>
      </xsl:with-param>
      <xsl:with-param name="pXMLTradePartiesAndBrokers">
        <xsl:copy-of select ="$pXMLTradePartiesAndBrokers"/>
      </xsl:with-param>
      <xsl:with-param name="pPayer">
        <xsl:value-of select="$vRow/data[@name='PFE5']"/>
      </xsl:with-param>
      <xsl:with-param name="pReceiver">
        <xsl:value-of select ="$vRow/data[@name='RFE5']"/>
      </xsl:with-param>
      <xsl:with-param name="pType">
        <xsl:value-of select ="$vRow/data[@name='TFE5']"/>
      </xsl:with-param>
      <xsl:with-param name="pInformation">
        <xsl:value-of select="$vRow/data[@name='IFE5']"/>
      </xsl:with-param>
    </xsl:call-template>

    <xsl:if test ="$pRecordTypeLeg1!='A'">
      <xsl:call-template name="XMLFee">
        <xsl:with-param name="pAmount">
          <xsl:value-of select ="$vRow/data[@name='MFE6']"/>
        </xsl:with-param>
        <xsl:with-param name="pCurrency">
          <xsl:value-of select ="$vRow/data[@name='CFE6']"/>
        </xsl:with-param>
        <xsl:with-param name="pXMLTradePartiesAndBrokers">
          <xsl:copy-of select ="$pXMLTradePartiesAndBrokers"/>
        </xsl:with-param>
        <xsl:with-param name="pPayer">
          <xsl:value-of select="$vRow/data[@name='PFE6']"/>
        </xsl:with-param>
        <xsl:with-param name="pReceiver">
          <xsl:value-of select ="$vRow/data[@name='RFE6']"/>
        </xsl:with-param>
        <xsl:with-param name="pType">
          <xsl:value-of select ="$vRow/data[@name='TFE6']"/>
        </xsl:with-param>
        <xsl:with-param name="pInformation">
          <xsl:value-of select="$vRow/data[@name='IFE6']"/>
        </xsl:with-param>
      </xsl:call-template>
      <xsl:call-template name="XMLFee">
        <xsl:with-param name="pAmount">
          <xsl:value-of select ="$vRow/data[@name='MFE7']"/>
        </xsl:with-param>
        <xsl:with-param name="pCurrency">
          <xsl:value-of select ="$vRow/data[@name='CFE7']"/>
        </xsl:with-param>
        <xsl:with-param name="pXMLTradePartiesAndBrokers">
          <xsl:copy-of select ="$pXMLTradePartiesAndBrokers"/>
        </xsl:with-param>
        <xsl:with-param name="pPayer">
          <xsl:value-of select="$vRow/data[@name='PFE7']"/>
        </xsl:with-param>
        <xsl:with-param name="pReceiver">
          <xsl:value-of select ="$vRow/data[@name='RFE7']"/>
        </xsl:with-param>
        <xsl:with-param name="pType">
          <xsl:value-of select ="$vRow/data[@name='TFE7']"/>
        </xsl:with-param>
        <xsl:with-param name="pInformation">
          <xsl:value-of select="$vRow/data[@name='IFE7']"/>
        </xsl:with-param>
      </xsl:call-template>
      <xsl:call-template name="XMLFee">
        <xsl:with-param name="pAmount">
          <xsl:value-of select ="$vRow/data[@name='MFE8']"/>
        </xsl:with-param>
        <xsl:with-param name="pCurrency">
          <xsl:value-of select ="$vRow/data[@name='CFE8']"/>
        </xsl:with-param>
        <xsl:with-param name="pXMLTradePartiesAndBrokers">
          <xsl:copy-of select ="$pXMLTradePartiesAndBrokers"/>
        </xsl:with-param>
        <xsl:with-param name="pPayer">
          <xsl:value-of select="$vRow/data[@name='PFE8']"/>
        </xsl:with-param>
        <xsl:with-param name="pReceiver">
          <xsl:value-of select ="$vRow/data[@name='RFE8']"/>
        </xsl:with-param>
        <xsl:with-param name="pType">
          <xsl:value-of select ="$vRow/data[@name='TFE8']"/>
        </xsl:with-param>
        <xsl:with-param name="pInformation">
          <xsl:value-of select="$vRow/data[@name='IFE8']"/>
        </xsl:with-param>
      </xsl:call-template>
      <xsl:call-template name="XMLFee">
        <xsl:with-param name="pAmount">
          <xsl:value-of select ="$vRow/data[@name='MFE9']"/>
        </xsl:with-param>
        <xsl:with-param name="pCurrency">
          <xsl:value-of select ="$vRow/data[@name='CFE9']"/>
        </xsl:with-param>
        <xsl:with-param name="pXMLTradePartiesAndBrokers">
          <xsl:copy-of select ="$pXMLTradePartiesAndBrokers"/>
        </xsl:with-param>
        <xsl:with-param name="pPayer">
          <xsl:value-of select="$vRow/data[@name='PFE9']"/>
        </xsl:with-param>
        <xsl:with-param name="pReceiver">
          <xsl:value-of select ="$vRow/data[@name='RFE9']"/>
        </xsl:with-param>
        <xsl:with-param name="pType">
          <xsl:value-of select ="$vRow/data[@name='TFE9']"/>
        </xsl:with-param>
        <xsl:with-param name="pInformation">
          <xsl:value-of select="$vRow/data[@name='IFE9']"/>
        </xsl:with-param>
      </xsl:call-template>
      <xsl:call-template name="XMLFee">
        <xsl:with-param name="pAmount">
          <xsl:value-of select ="$vRow/data[@name='MFE10']"/>
        </xsl:with-param>
        <xsl:with-param name="pCurrency">
          <xsl:value-of select ="$vRow/data[@name='CFE10']"/>
        </xsl:with-param>
        <xsl:with-param name="pXMLTradePartiesAndBrokers">
          <xsl:copy-of select ="$pXMLTradePartiesAndBrokers"/>
        </xsl:with-param>
        <xsl:with-param name="pPayer">
          <xsl:value-of select="$vRow/data[@name='PFE10']"/>
        </xsl:with-param>
        <xsl:with-param name="pReceiver">
          <xsl:value-of select ="$vRow/data[@name='RFE10']"/>
        </xsl:with-param>
        <xsl:with-param name="pType">
          <xsl:value-of select ="$vRow/data[@name='TFE10']"/>
        </xsl:with-param>
        <xsl:with-param name="pInformation">
          <xsl:value-of select="$vRow/data[@name='IFE10']"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:if>

  </xsl:template>

  <!-- RD 20121204 [18240] Import: Trades with data for extensions -->
  <xsl:template name="XMLExtend">
    <xsl:param name="pExtendIdentifier"/>
    <xsl:param name="pExtendDetIdentifier"/>
    <xsl:param name="pExtendValue"/>

    <xsl:if test="string-length($pExtendIdentifier) > 0 and string-length($pExtendDetIdentifier) > 0">
      <extend identifier="{$pExtendIdentifier}" detail="{$pExtendDetIdentifier}" value="{$pExtendValue}"/>
    </xsl:if>
  </xsl:template>
  <xsl:template name="XMLExtends">
    <xsl:param name ="pXMLRow"/>

    <xsl:variable name="vRow" select="msxsl:node-set($pXMLRow)/row"/>

    <xsl:if test="$vRow/data[starts-with(@name,'XT')]">
      <xsl:call-template name="XMLExtend">
        <xsl:with-param name="pExtendIdentifier">
          <xsl:value-of select ="$vRow/data[@name='XTI1']"/>
        </xsl:with-param>
        <xsl:with-param name="pExtendDetIdentifier">
          <xsl:value-of select ="$vRow/data[@name='XTDI1']"/>
        </xsl:with-param>
        <xsl:with-param name="pExtendValue">
          <xsl:value-of select ="$vRow/data[@name='XTVL1']"/>
        </xsl:with-param>
      </xsl:call-template>
      <xsl:call-template name="XMLExtend">
        <xsl:with-param name="pExtendIdentifier">
          <xsl:value-of select ="$vRow/data[@name='XTI2']"/>
        </xsl:with-param>
        <xsl:with-param name="pExtendDetIdentifier">
          <xsl:value-of select ="$vRow/data[@name='XTDI2']"/>
        </xsl:with-param>
        <xsl:with-param name="pExtendValue">
          <xsl:value-of select ="$vRow/data[@name='XTVL2']"/>
        </xsl:with-param>
      </xsl:call-template>
      <xsl:call-template name="XMLExtend">
        <xsl:with-param name="pExtendIdentifier">
          <xsl:value-of select ="$vRow/data[@name='XTI3']"/>
        </xsl:with-param>
        <xsl:with-param name="pExtendDetIdentifier">
          <xsl:value-of select ="$vRow/data[@name='XTDI3']"/>
        </xsl:with-param>
        <xsl:with-param name="pExtendValue">
          <xsl:value-of select ="$vRow/data[@name='XTVL3']"/>
        </xsl:with-param>
      </xsl:call-template>
      <xsl:call-template name="XMLExtend">
        <xsl:with-param name="pExtendIdentifier">
          <xsl:value-of select ="$vRow/data[@name='XTI4']"/>
        </xsl:with-param>
        <xsl:with-param name="pExtendDetIdentifier">
          <xsl:value-of select ="$vRow/data[@name='XTDI4']"/>
        </xsl:with-param>
        <xsl:with-param name="pExtendValue">
          <xsl:value-of select ="$vRow/data[@name='XTVL4']"/>
        </xsl:with-param>
      </xsl:call-template>
      <xsl:call-template name="XMLExtend">
        <xsl:with-param name="pExtendIdentifier">
          <xsl:value-of select ="$vRow/data[@name='XTI5']"/>
        </xsl:with-param>
        <xsl:with-param name="pExtendDetIdentifier">
          <xsl:value-of select ="$vRow/data[@name='XTDI5']"/>
        </xsl:with-param>
        <xsl:with-param name="pExtendValue">
          <xsl:value-of select ="$vRow/data[@name='XTVL5']"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>


  <!-- Get an enum value from the ENUM table related to the the enum type [pCode] and the given enum external value [pExtValue] -->
  <!-- FI 20120705 [17993] refactoring query-->
  <xsl:template name="SQLEnumValue">
    <xsl:param name="pCode"/>
    <xsl:param name="pExtValue"/>
    <xsl:if test="string-length($pExtValue) > 0">
      <SQL command="select" result="RETDATA" cache="true">
        <![CDATA[
        select
        case count(e.VALUE) when 0 then @EXTVALUE else min(e.VALUE) end
        ||
        case count(e.VALUE) when 0 then '!NotFound' when 1 then '' else '!NotUnique' end as RETDATA
        from dbo.ENUM e
        where e.CODE = @CODE
        and ( e.EXTVALUE = @EXTVALUE or e.VALUE = @EXTVALUE or e.EXTLLINK = @EXTVALUE or 
	            e.EXTLATTRB = @EXTVALUE or e.EXTLLINK2 = @EXTVALUE)
        order by
        case min(e.EXTVALUE) when @EXTVALUE then 1
                             else case min(e.VALUE) when @EXTVALUE then 2
                                  else case min(e.EXTLATTRB) when @EXTVALUE then 3
                                       else case min(e.EXTLLINK) when @EXTVALUE then 4
                                            else 5
                                            end
                                       end
                                  end
                             end
        ]]>
        <Param name="CODE" datatype="string">
          <xsl:value-of select="$pCode"/>
        </Param>
        <Param name="EXTVALUE" datatype="string">
          <xsl:value-of select="$pExtValue"/>
        </Param>
      </SQL>
    </xsl:if>
  </xsl:template>
  
  <xsl:template name="SQLEntityBrokerOfBook">
    <xsl:param name="pBook"/>
    <xsl:param name="pBookIdent"/>
    <xsl:param name="pResultColumn" select="'IDENTIFIER'"/>

    <xsl:variable name ="vBookColumnName">
      <xsl:call-template name="GetBookIdentColumn">
        <xsl:with-param name="pIdent">
          <xsl:copy-of select="$pBookIdent"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    <xsl:if test="string-length($pBook) > 0 and string-length($vBookColumnName) > 0 and $vBookColumnName != 'IDENTNOTFOUND'">
      <xsl:variable name ="vXMLExtlIdJoins">
        <xsl:call-template name="XMLJoinExtlId">
          <xsl:with-param name="pTableName">
            <xsl:value-of select="'BOOK'"/>
          </xsl:with-param>
          <xsl:with-param name="pTableAlias">
            <xsl:value-of select="'b'"/>
          </xsl:with-param>
          <xsl:with-param name="pJoinId">
            <xsl:value-of select="'b.IDB'"/>
          </xsl:with-param>
          <xsl:with-param name="pValueColumn">
            <xsl:value-of select="$vBookColumnName"/>
          </xsl:with-param>
          <xsl:with-param name="pValue">
            <xsl:value-of select="$pBook"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:variable>
      <SQL command="select" result="IDENTIFIER" cache="true">
        select a.IDENTIFIER
        from dbo.BOOK b
        inner join dbo.ENTITY e on e.IDA=b.IDA_ENTITY
        inner join dbo.ACTORROLE ar on ar.IDA=e.IDA and ar.IDROLEACTOR='BROKER'
        inner join dbo.ACTOR a on a.IDA=e.IDA
        <xsl:call-template name="SQLJoinExtlId">
          <xsl:with-param name="pXMLExtlIdJoins">
            <xsl:copy-of select="$vXMLExtlIdJoins"/>
          </xsl:with-param>
        </xsl:call-template>
      </SQL>
    </xsl:if>
  </xsl:template>

  <xsl:template name="SQLTraderSales">
    <xsl:param name="pValue"/>
    <xsl:param name="pValueIdent"/>
    <xsl:param name="pUseDefaultValue" select="$ConstFalse"/>

    <xsl:variable name ="vActorColumn">
      <xsl:call-template name="GetCommonIdentColumn">
        <xsl:with-param name="pIdent">
          <xsl:copy-of select="$pValueIdent"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    
    <xsl:call-template name="SQLActorBase">
      <xsl:with-param name="pValue">
        <xsl:value-of select="$pValue"/>
      </xsl:with-param>
      <xsl:with-param name="pActorColumn">
        <xsl:value-of select="$vActorColumn"/>
      </xsl:with-param>
      <xsl:with-param name="pUseDefaultValue" select="$pUseDefaultValue"/>
    </xsl:call-template>

  </xsl:template>


  <!-- BD 20120120 [18363] Gestion des Trades Modify et Update -->
  <!-- FI 20130521 [18672] suppression du paramètre date de transaction -->
  <!-- FI 20140103 [19433] modification de la requête associée à la colonne IDTGIVEUP (utilisation de la colonne FIXPARTYROLE) -->
  <!-- EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML) -->
  <xsl:template name="SQLCheckTrade">
    <xsl:param name="pActionType"/>
    <xsl:param name="pRecordType"/>
    <xsl:param name="pTradeId"/>
    <xsl:param name="pResultColumn"/>
    <xsl:param name="pClearingBusinessDate"/>

    <SQL command="select" result="{$pResultColumn}">
      <xsl:choose>
        <!--
        ISOK=> Le trade n'existe pas ou tous les trades sont Deactiv ou tous les Trades sont Deactiv sauf un qui est Missing
        IMPORTMODE => tous les Trades sont Deactiv sauf un qui est Missing alors update sinon New
        -->
        <xsl:when test="$pActionType='N'">
          <xsl:text>
          <![CDATA[
          select
          case when
          (tabAll.COUNTIDT = 0
          or (tabAll.COUNTIDT = tabDeactiv.COUNTIDT)
          or (tabMissing.COUNTIDT = 1 and (tabAll.COUNTIDT = tabDeactiv.COUNTIDT + 1)))
          then 1 else 0 end as ISOK,
          case when
          (tabMissing.COUNTIDT = 1 and (tabAll.COUNTIDT = tabDeactiv.COUNTIDT + 1)) then 'Update'
          else 'New' end as IMPORTMODE
          from
          (
          select count(t.IDT) as COUNTIDT
          from dbo.TRADE t
          where upper(t.EXTLLINK)=upper(@TRADEID) and t.DTBUSINESS = @DT
          ) tabAll,
          (
          select count(t.IDT) as COUNTIDT
          from dbo.TRADE t
          where upper(t.EXTLLINK)=upper(@TRADEID) and t.DTBUSINESS = @DT and t.IDSTACTIVATION='DEACTIV'
          ) tabDeactiv,
          (
          select count(t.IDT) as COUNTIDT
          from dbo.TRADE t
          where upper(t.EXTLLINK)=upper(@TRADEID) and t.DTBUSINESS = @DT and t.IDSTACTIVATION='MISSING'
          ) tabMissing
          ]]> 
          </xsl:text>
        </xsl:when>

        <!--
        ISOK=> (Le trade existe et n'est pas Deactiv) ou (le Trade n'existe pas)
        IMPORTMODE => Si le trade n'existe pas alors New sinon Update
        -->
        <xsl:when test="$pActionType='M'">
          <xsl:text>
          <![CDATA[
          select
          case when(
          (tabAll.COUNTIDT > 0 and tabAll.COUNTIDT = tabDeactiv.COUNTIDT + 1)
          or
          (tabAll.COUNTIDT = 0)
          ) then 
          1 else 0 end as ISOK,
          case when (tabAll.COUNTIDT = 0) then
          'New' else 'Update' end as IMPORTMODE
          from
          (
            select count(t.IDT) as COUNTIDT
            from dbo.TRADE t
            where upper(t.EXTLLINK)=upper(@TRADEID) and t.DTBUSINESS = @DT
          ) tabAll,
          (
          select count(t.IDT) as COUNTIDT
          from dbo.TRADE t
          where upper(t.EXTLLINK)=upper(@TRADEID) and t.DTBUSINESS = @DT and t.IDSTACTIVATION='DEACTIV'
          ) tabDeactiv
          ]]>
          </xsl:text>
        </xsl:when>

        <!--
        ISOK=> (Le trade existe et n'est pas Deactiv et n'est pas un give-up) ou (le Trade n'existe pas)
        IMPORTMODE => Si le trade n'existe pas alors New sinon Update
        -->
        <xsl:when test="$pActionType='G'">
          <xsl:text>
          <![CDATA[
          select
          case when
          (
          (tabAll.COUNTIDT > 0 and tabAll.COUNTIDT = tabDeactiv.COUNTIDT + 1 and IDTGIVEUP=0)
          or
          (tabAll.COUNTIDT = 0)
          )
          then 1 else 0 end as ISOK,
          case when (tabAll.COUNTIDT = 0) then
          'New' else 'Update' end as IMPORTMODE
          from
          (
          select count(t.IDT) as COUNTIDT
          from dbo.TRADE t
          where upper(t.EXTLLINK)=upper(@TRADEID) and t.DTBUSINESS = @DT
          ) tabAll,
          (
          select count(t.IDT) as COUNTIDT
          from dbo.TRADE t
          where upper(t.EXTLLINK)=upper(@TRADEID) and t.DTBUSINESS = @DT and t.IDSTACTIVATION='DEACTIV'
          ) tabDeactiv,
          (
          select isnull(MAX(t.IDT),0) as IDTGIVEUP
          from dbo.TRADE t
          inner join dbo.TRADEACTOR ta on ta.IDT = t.IDT and ta.FIXPARTYROLE = '14'
          where upper(t.EXTLLINK)=upper(@TRADEID) and t.DTBUSINESS = @DT and t.IDSTACTIVATION='REGULAR'
          )giveup
          ]]>
          </xsl:text>
        </xsl:when>

        <!--
        ISOK=> (Le trade existe et n'est pas Deactiv)
        IMPORTMODE => Update
        -->
        <!-- FI 20160907 [21831] Add $pActionType='F'-->
        <xsl:when test="$pActionType='U' or $pActionType='F'">
          <xsl:text>
          <![CDATA[
          select
          case when
          (tabAll.COUNTIDT > 0 and tabAll.COUNTIDT = tabDeactiv.COUNTIDT + 1)
          then 1 else 0 end as ISOK,
          'Update' as IMPORTMODE
          from
          (
          select count(t.IDT) as COUNTIDT
          from dbo.TRADE t
          where upper(t.EXTLLINK)=upper(@TRADEID) and t.DTBUSINESS = @DT
          ) tabAll,
          (
          select count(t.IDT) as COUNTIDT
          from dbo.TRADE t
          where upper(t.EXTLLINK)=upper(@TRADEID) and t.DTBUSINESS = @DT and t.IDSTACTIVATION='DEACTIV'
          ) tabDeactiv
          ]]>
          </xsl:text>
        </xsl:when>

        <!--
        ISOK=> (Le trade existe et n'est pas Deactiv)
        IMPORTMODE => RemoveOnly OU RemoveAllocation
        -->
        <xsl:when test="$pActionType='S'">
          <xsl:text>
          <![CDATA[
          select
          case when
          ( (tabAll.COUNTIDT > 0
          and tabAll.COUNTIDT = tabDeactiv.COUNTIDT + 1) )
          then 1 else 0 end as ISOK,
          @IMPORTMODE as IMPORTMODE
          from
          (
          select count(t.IDT) as COUNTIDT
          from dbo.TRADE t
          where upper(t.EXTLLINK)=upper(@TRADEID) and t.DTBUSINESS = @DT
          ) tabAll,
          (
          select count(t.IDT) as COUNTIDT
          from dbo.TRADE t
          where upper(t.EXTLLINK)=upper(@TRADEID) and t.DTBUSINESS = @DT and t.IDSTACTIVATION='DEACTIV'
          ) tabDeactiv
          ]]>
          </xsl:text>
          <Param name="IMPORTMODE" datatype="string">
            <xsl:call-template name="GetDefaultImportMode">
              <xsl:with-param name="pActionType" select="$pActionType"/>
              <xsl:with-param name="pRecordType" select="$pRecordType"/>
            </xsl:call-template>
          </Param>
        </xsl:when>
      </xsl:choose>
      <Param name="TRADEID" datatype="string">
        <xsl:value-of select="$pTradeId" />
      </Param>
      <Param name="DT" datatype="date">
        <xsl:value-of select="$pClearingBusinessDate"/>
      </Param>
    </SQL>
  </xsl:template>

  <!-- FI 20130301 [18465] Refactoring En mode création de trade il est possible de spécifier un identifier -->
  <!-- FI 20130521 [18672] suppression du paramètre date de transaction et recordType -->
  <!-- EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML) -->
  <xsl:template name="SQLTradeIdentifier">
    <xsl:param name="pActionType"/>
    <xsl:param name="pTradeId"/>
    <xsl:param name="pResultColumn"/>
    <xsl:param name="pClearingBusinessDate"/>
    <SQL command="select" result="{$pResultColumn}">
      <xsl:choose>
        <!-- 'N' peut donner lieu à une modification de trade MISSING => Recherche du trade MISSING -->
        <!-- 'N' peut donner lieu à mise en place d'un identifier fourni en entrée -->
        <xsl:when test="$pActionType='N'">
          <xsl:text>
            <![CDATA[
            select t.IDENTIFIER, 0 as SORT            
            from dbo.TRADE t
            where upper(t.EXTLLINK)=upper(@TRADEID) and t.DTBUSINESS = @DT and t.IDSTACTIVATION='MISSING'
            ]]>
          </xsl:text>
          <xsl:if test ="$gIdentifierSource = 'TradeId'">
            <xsl:text>
            <![CDATA[
            union
            select @TRADEID as IDENTIFIER , 1 as SORT
            from DUAL
            order by SORT ASC
            ]]>
            </xsl:text>
          </xsl:if>
        </xsl:when>
        <!-- Recherche d'un trade existant -->
        <!-- 'M' et 'G' peuvent donner lieu à un nouveau trade si le select n'aboutie pas -->
        <!-- 'U' et 'S' génère une exception lorsque le select n'aboutie pas -->
        <!-- FI 20160907 [21831] Add $pActionType='F'-->
        <xsl:when test="$pActionType = 'M' or $pActionType = 'G' or $pActionType = 'U' or $pActionType='F' or $pActionType = 'S'">
          <xsl:text>
          <![CDATA[
            select
            t.IDENTIFIER
            from dbo.TRADE t
            where upper(t.EXTLLINK)=upper(@TRADEID) and t.DTBUSINESS = @DT
            order by case when t.IDSTACTIVATION='REGULAR' then 1 
                          when t.IDSTACTIVATION='MISSING' then 2  
                          else  3 end
          ]]>
          </xsl:text>
        </xsl:when>
      </xsl:choose>
      <Param name="TRADEID" datatype="string">
        <xsl:value-of select="$pTradeId" />
      </Param>
      <Param name="DT" datatype="date">
        <xsl:value-of select="$pClearingBusinessDate"/>
      </Param>
    </SQL>
  </xsl:template>

  <!-- RD 20121204 [18240] Import: Trades with data for extensions -->
  <xsl:template name="SQLExtendAttribute">
    <xsl:param name="pExtIdentifier"/>
    <xsl:param name="pExtDet"/>
    <xsl:param name="pResultColumn"/>
    <xsl:param name="pInstrumentIdentifier"/>
    <xsl:param name="pAssetCode"/>
    <xsl:param name="pAssetColumn"/>
    <xsl:param name="pMarket"/>
    <xsl:param name="pMarketColumn"/>
    <xsl:param name="pClearingBusinessDate"/>

    <SQL command="select" result="{$pResultColumn}" cache="true">
      <xsl:text>
        <![CDATA[
        select 1 as ISEXTENDEXIST,
        'tradeExtends_' || convert(varchar,dextd.IDDEFINEEXTENDDET) || '_' || dextd.IDENTIFIER as CLIENTID,
        dextd.DATATYPE, dextd.ISMANDATORY, dextd.REGULAREXPRESSION, dextd.DEFAULTVALUE
        from dbo.INSTRUMENT i
        inner join dbo.DEFINEEXTEND dext on (dext.IDENTIFIER=@EXTIDENTIFIER)
        ]]>
      </xsl:text>
      <xsl:call-template name="SQLDTENABLEDDTDISABLED">
        <xsl:with-param name="pTable" select="'dext'"/>
      </xsl:call-template>
      <xsl:text>
        <![CDATA[
        and 
        (
        (dext.TYPEINSTR is null)
        or ((dext.TYPEINSTR=@TYPEPRODUCT) and (dext.IDINSTR=i.IDP))
        or ((dext.TYPEINSTR=@TYPEINSTR) and (dext.IDINSTR=i.IDI))
        or ((dext.TYPEINSTR=@TYPEGINSTR) and (dext.IDINSTR in 
        (
        select ig.IDGINSTR 
        from dbo.INSTRG ig
        where (ig.IDI=i.IDI) 
        ]]>
      </xsl:text>
      <xsl:call-template name="SQLDTENABLEDDTDISABLED">
        <xsl:with-param name="pTable" select="'ig'"/>
      </xsl:call-template>
      <xsl:text>
        <![CDATA[
        ))))
        inner join dbo.DEFINEEXTENDDET dextd on (dextd.IDDEFINEEXTEND=dext.IDDEFINEEXTEND) and (dextd.IDENTIFIER=@EXTDETAIL)
        ]]>
      </xsl:text>
      <xsl:choose>
        <xsl:when test="$pInstrumentIdentifier != $ConstUseSQL">
          <xsl:text>
where (i.IDENTIFIER=@INSTRIDENTIFIER)
          </xsl:text>
          <Param name="INSTRIDENTIFIER" datatype="string">
            <xsl:value-of select="$pInstrumentIdentifier"/>
          </Param>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>inner join dbo.DERIVATIVECONTRACT dc on (dc.IDI=i.IDI)</xsl:text>
          <xsl:call-template name="SQLDTENABLEDDTDISABLED">
            <xsl:with-param name="pTable" select="'dc'"/>
          </xsl:call-template>
          <xsl:text>inner join dbo.DERIVATIVEATTRIB da on (da.IDDC=dc.IDDC)</xsl:text>
          <xsl:call-template name="SQLDTENABLEDDTDISABLED">
            <xsl:with-param name="pTable" select="'da'"/>
          </xsl:call-template>
          <xsl:text>inner join dbo.ASSET_ETD a on (a.IDDERIVATIVEATTRIB=da.IDDERIVATIVEATTRIB)</xsl:text>
          <xsl:call-template name="SQLDTENABLEDDTDISABLED">
            <xsl:with-param name="pTable" select="'a'"/>
          </xsl:call-template>
          <xsl:if test="string-length($pMarket) > 0 and string-length($pMarketColumn) > 0">
            <xsl:text>inner join dbo.VW_MARKET_IDENTIFIER m on (m.IDM=dc.IDM)</xsl:text>
            <xsl:call-template name="SQLDTENABLEDDTDISABLED">
              <xsl:with-param name="pTable" select="'m'"/>
            </xsl:call-template>
          </xsl:if>
          <xsl:variable name ="vXMLExtlIdJoins">
            <xsl:call-template name="XMLJoinExtlId">
              <xsl:with-param name="pTableName">
                <xsl:value-of select="'ASSET_ETD'"/>
              </xsl:with-param>
              <xsl:with-param name="pTableAlias">
                <xsl:value-of select="'a'"/>
              </xsl:with-param>
              <xsl:with-param name="pJoinId">
                <xsl:value-of select="'a.IDASSET'"/>
              </xsl:with-param>
              <xsl:with-param name="pValueColumn">
                <xsl:value-of select="$pAssetColumn"/>
              </xsl:with-param>
              <xsl:with-param name="pValue">
                <xsl:value-of select="$pAssetCode"/>
              </xsl:with-param>
            </xsl:call-template>
            <xsl:if test="string-length($pMarket) > 0 and string-length($pMarketColumn) > 0">
              <xsl:call-template name="XMLJoinExtlId">
                <xsl:with-param name="pTableName">
                  <xsl:value-of select="'MARKET'"/>
                </xsl:with-param>
                <xsl:with-param name="pTableAlias">
                  <xsl:value-of select="'m'"/>
                </xsl:with-param>
                <xsl:with-param name="pJoinId">
                  <xsl:value-of select="'m.IDM'"/>
                </xsl:with-param>
                <xsl:with-param name="pValueColumn">
                  <xsl:value-of select="$pMarketColumn"/>
                </xsl:with-param>
                <xsl:with-param name="pValue">
                  <xsl:value-of select="$pMarket"/>
                </xsl:with-param>
              </xsl:call-template>
            </xsl:if>
          </xsl:variable>
          <xsl:call-template name="SQLJoinExtlId">
            <xsl:with-param name="pXMLExtlIdJoins">
              <xsl:copy-of select="$vXMLExtlIdJoins"/>
            </xsl:with-param>
            <xsl:with-param name="pAdditionalCondition"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>

      <Param name="EXTIDENTIFIER" datatype="string">
        <xsl:value-of select="$pExtIdentifier"/>
      </Param>
      <Param name="EXTDETAIL" datatype="string">
        <xsl:value-of select="$pExtDet"/>
      </Param>
      <Param name="TYPEINSTR" datatype="string">
        <xsl:text>Instr</xsl:text>
      </Param>
      <Param name="TYPEGINSTR" datatype="string">
        <xsl:text>GrpInstr</xsl:text>
      </Param>
      <Param name="TYPEPRODUCT" datatype="string">
        <xsl:text>Product</xsl:text>
      </Param>
      <Param name="DT" datatype="date">
        <xsl:value-of select="$pClearingBusinessDate"/>
      </Param>
    </SQL>
  </xsl:template>

  <!-- FI 20131122 [19233] add template LogRowInfo -->
  <xsl:template name ="LogRowInfo">
    <xsl:param name ="pRecordTypeLog"/>
    <xsl:param name ="pTradeIdLeg1"/>
    <xsl:param name ="pActionTypeLog"/>
    <xsl:param name ="pStrategyTypeLeg1"/>
    <xsl:param name ="pTNL"/>

    <xsl:value-of select ="concat('Trade Id: ',$pTradeIdLeg1,', Trade type: ',$pRecordTypeLog)"/>
    <xsl:if test="string-length($pStrategyTypeLeg1) > 0">
      <xsl:value-of select ="concat(', Strategy type: ',$pStrategyTypeLeg1,', Leg total number: ',$pTNL)"/>
    </xsl:if>
    <xsl:value-of select ="concat(', Action type: ',$pActionTypeLog)"/>
  </xsl:template>

  <!-- FI 20131122 [19233] add template LogRowDesc -->
  <xsl:template name="LogRowDesc">
    <xsl:param name ="pRowId"/>
    <xsl:param name ="pRowSrc"/>
    <xsl:value-of select ="concat('Row (id:&lt;b&gt;',$pRowId,'&lt;/b&gt;, src:',$pRowSrc,')')"/>
    <!--the result is => Row (id:<b>{1}</b>, src:{2})-->
  </xsl:template>

</xsl:stylesheet>