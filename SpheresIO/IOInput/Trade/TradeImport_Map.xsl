<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt">
<!--  
=============================================================================================
Summary : Trade input - Spheres standard mapping
=============================================================================================
FI 20170824 [23339] Gestion de l'action T
FI 20170829 [XXXXX] Mise en place d'un warning lorsque Spheres®IO détecte que le flux est issu d'une gateway où Il manque l'identifiant du membre connecté
FI 20171103 [23326] Format v2bis (gestion de MiFID/MIFIR)
FI 20181022 [24210] correction pb sur auto creation de trader
FI 20240227 [WI856] Add TVTIC
FI 20240229 [WI860] Import fom gateway BCS : call ConvertToLongFromBase64String for Order ElectronicID 
FI 20240229 [WI859] use ALID (Market Transaction ID)
-->

  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml;"/>
  <!--<xsl:param name="pDebugMode" select="'SectionParty'"/>-->
  <xsl:param name="pDebugMode" select="'None'"/>
  <!-- FI 20170718 [23326] add key -->
  <xsl:key name="kTradePartiesAndBrokersNodeName" match="party|book|trader|frontId|folder" use="@name"/>

  <!-- ================================================== -->
  <!--        include(s)                                  -->
  <!-- ================================================== -->
  <xsl:include href="..\Common\ImportTools.xsl"/>

  <!-- ================================================== -->
  <!--        Global Constantes                           -->
  <!-- ================================================== -->
  <xsl:variable name ="ConstPartyNumber1">1</xsl:variable>
  <xsl:variable name ="ConstPartyNumber2">2</xsl:variable>
  <xsl:variable name ="ConstStrategy" select ="'strategy_'"/>

  <!-- ================================================== -->
  <!--        Global Variables                            -->
  <!-- ================================================== -->
  <!-- FI 20130701 [18798]
      S'il existe des paramètres => la tâche IO d'importation de trade est exécutée suite à une demande manuellement (RunIo.aspx)
      S'il n'existe pas de paramètre => la tâche IO d'importation de trade est exécutée suite à une demande gateway
  -->
  <xsl:variable name ="gIsModegateway">
    <xsl:choose>
      <!-- FI 20240118 [WI817] use of parameter ISCREATEDBY_GATEWAY -->
      <xsl:when test="/iotask/parameters/parameter[@id='ISCREATEDBY_GATEWAY']=$ConstTrue">
        <xsl:value-of select="$ConstTrue"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$ConstFalse"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- FI 20130301 [18465] 
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


  <!-- FI 20130301 [18465]
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

  <!-- FI 20130301 [18465] 
    Mode de détermination de l'identifier du trade 
    "Default" => L'identifier du trade est calculé par Spheres®  
    "TradeId" => L'identifier du trade est TradeId 
  -->
  <xsl:variable name ="gIdentifierSource">
    <xsl:value-of select="/iotask/parameters/parameter[@id='IDENTIFIERSRC']"/>
  </xsl:variable>

  <!--RD 20190718 Use Task parameter ISPOSTTOEVENTSGEN--> 
  <xsl:variable name ="gIsPostToEventsGen">
    <xsl:choose>
      <xsl:when test="string-length(/iotask/parameters/parameter[@id='ISPOSTTOEVENTSGEN']) > 0">
        <xsl:value-of select="/iotask/parameters/parameter[@id='ISPOSTTOEVENTSGEN']"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$ConstTrue"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- ================================================== -->
  <!--        Templates match                             -->
  <!-- ================================================== -->
  <xsl:template match="/iotask">
    <iotask id="{@id}" name="{@name}" displayname="{@displayname}" loglevel="{@loglevel}" commitmode="{@commitmode}">
      <xsl:apply-templates select="parameters"/>
      <xsl:apply-templates select="iotaskdet"/>
    </iotask>
  </xsl:template>

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
      <!-- RD GLOP : Il faut traiter les Row de type Header et Footer -->

      <!-- Spheres® traite d'abord tous les Row de type Allocation -->
      <xsl:apply-templates select="row[@status='success' and data[@name='RTP']='A']"/>
      <!-- Ensuite, Spheres® traite tous les Row de type NON Allocation et avec LegSequenceNumber = 1 -->
      <xsl:apply-templates select="row[@status='success' and data[@name='RTP']!='A' and data[@name='LSN']=1]"/>
    </file>
  </xsl:template>

  <!-- ================================================================= 
        In Strategy case: Process each Row with SequenceLegNumber = 1 
                          and his attached Rows with SequenceLegNumber>1 
       ================================================================= -->
  <xsl:template match="row">
    <xsl:variable name ="vId" select ="normalize-space(@id)"/>
    <xsl:variable name ="vSrc" select ="normalize-space(@src)"/>

    <!-- FI 20170404 [23039] Add vRecordVersion1
    - v1 => version 1 du parsing
    - v2 => version 2 du parsing (standard v6.0)
    -->
    <xsl:variable name ="vRecordVersion1">
      <xsl:call-template name="GetRecordVersion">
        <xsl:with-param name="pRow" select="."/>
      </xsl:call-template>
    </xsl:variable>

    <!-- RTP: Valeurs possibles (E)xecution ou (I)ntermediation ou (A)lloc -->
    <xsl:variable name ="vRecordType" select ="normalize-space(data[@name='RTP'])"/>
    <xsl:variable name ="vRecordTypeLog">
      <xsl:call-template name="GetRecordTypeLog">
        <xsl:with-param name="pRecordType" select="$vRecordType"/>
      </xsl:call-template>
    </xsl:variable>

    <!-- FI 20130702 [18798] add variable vStBusiness, vStActivation, vStEnvironment, vStPriority -->
    <xsl:variable name ="vStBusiness" >
      <xsl:call-template name="GetStBusiness">
        <xsl:with-param name="pRecordType">
          <xsl:value-of select="$vRecordType" />
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    <!-- FI 20130703 [18798] GLOP 
      Il faudra modifier le fichier standard pour y introduire ces données 
      Pour l'instant ces données sont renseignées à partir du template 
    -->
    <xsl:variable name ="vStActivation" select ="normalize-space(data[@name='STACT'])"/>
    <xsl:variable name ="vStEnvironment" select ="normalize-space(data[@name='STENV'])"/>
    <xsl:variable name ="vStPriority" select ="normalize-space(data[@name='STPRY'])"/>

    <!-- FI 20170718 [23326] call template GetIsAllowDataMissDealer -->
    <xsl:variable name ="isAllowDataMissDealer">
      <xsl:call-template name="GetIsAllowDataMissDealer">
        <xsl:with-param name="pRow" select="."/>
      </xsl:call-template>
    </xsl:variable>

    <!-- FI 20170718 [23326] call template GetIsAllowDataMissing -->
    <xsl:variable name ="isAllowDataMissing">
      <xsl:call-template name="GetIsAllowDataMissing">
        <xsl:with-param name="pRow" select="."/>
      </xsl:call-template>
    </xsl:variable>


    <!-- vStrategyType => exemple Box, Butterfly, Calendar-Spread, Combo,Condor, Conversion/Reversal,... None -->
    <xsl:variable name ="vStrategyType">
      <!-- FI 20180214 [23774] StrategyType exists in ALLOCATION
      <xsl:if test="$vRecordType != 'A'">
        <xsl:value-of select ="normalize-space(data[@name='STA'])"/>
      </xsl:if>-->
      <xsl:value-of select ="normalize-space(data[@name='STA'])"/>
    </xsl:variable>
    
    <!-- FI 20180214 [23774] add vLegSequenceNumber -->
    <xsl:variable name ="vLegSequenceNumber">
      <xsl:value-of select ="normalize-space(data[@name='LSN'])"/>
    </xsl:variable>

    <xsl:variable name ="vTotalNumberOfLegs">
      <!-- FI 20180214 [23774] TotalNumberOfLegs exists in ALLOCATION
      <xsl:if test="$vRecordType != 'A'">
        <xsl:value-of select ="normalize-space(data[@name='TNL'])"/>
      </xsl:if>-->
      <xsl:value-of select ="normalize-space(data[@name='TNL'])"/>
    </xsl:variable>

    <xsl:variable name ="vIsStrategy">
      <xsl:choose>
        <!-- FI 20170718 [23326] 'None' es la valeur précisée dans la doc lorsqu'il n'y a pas de stratégie  -->
        <xsl:when test="$vRecordType != 'A' and (string-length($vStrategyType) > 0) and $vStrategyType != 'None'">
          <xsl:value-of select ="$ConstTrue"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$ConstFalse"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name ="vCciPrefixStrategy">
      <xsl:if test="$vIsStrategy = $ConstTrue">
        <xsl:value-of select="$ConstStrategy"/>
      </xsl:if>
    </xsl:variable>

    <xsl:variable name ="vTradeId" select ="normalize-space(data[@name='TRID'])"/>
    <xsl:variable name ="vAllTradeRows" select ="$gAllRows/row[data[@name='TRID']=$vTradeId]"/>

    <xsl:variable name ="vActionType" select ="normalize-space(data[@name='ATP'])"/>
    <xsl:variable name ="vActionTypeLog">
      <xsl:call-template name="GetActionTypeLog">
        <xsl:with-param name="pActionType" select="$vActionType"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name ="vSpheresTemplateIdentifier" select ="normalize-space(data[@name='TPID'])"/>
    <xsl:variable name ="vSpheresScreen" select ="normalize-space(data[@name='SCR'])"/>
    <xsl:variable name ="vSpheresFeeCalculation" select="normalize-space(data[@name='SFC'])" />

    <!-- FI 20160829 [22412] Apply, ApplyParty or ApplyAll  -->
    <xsl:variable name ="vSpheresPartyTemplate">
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

    <!-- FI 20160829 [22412] ApplyBroker, ApplyAll  -->
    <xsl:variable name ="vSpheresClearingTemplate">
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

    <xsl:variable name ="vSpheresValidationXSD">
      <xsl:call-template name="IsToApplyOrToIgnore">
        <xsl:with-param name="pApplyIgnore">
          <xsl:value-of select="normalize-space(data[@name='SVX'])" />
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vSpheresValidationRules">
      <xsl:call-template name="IsToApplyOrToIgnore">
        <xsl:with-param name="pApplyIgnore">
          <xsl:value-of select="normalize-space(data[@name='SVR'])" />
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vSpheresDisplayname" select ="normalize-space(data[@name='DSP'])"/>
    <xsl:variable name ="vSpheresDescription" select ="normalize-space(data[@name='DSC'])"/>
    <xsl:variable name ="vSession" select ="normalize-space(data[@name='SSS'])"/>

    <!-- FI 20171103 [23326] Mise en commentaire
    <xsl:variable name ="vTransactionDate" select ="normalize-space(data[@name='TDT'])"/>
    <xsl:variable name ="vClearingBusinessDate" select ="normalize-space(data[@name='CDT'])"/>
    -->
    <!-- FI 20171103 [23326] -->
    <xsl:variable name ="vBusinessDate">
      <xsl:choose>
        <xsl:when test ="$vRecordVersion1='v1'">
          <xsl:value-of select="data[@name='CDT']"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="data[@name='BDT']"/>  
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    

    <!-- FI 20220215 [25699] Add vClearingHouse and vClearingHouseColumn (available only in v3 record) -->    
    <!-- vClearingHouse contient la valeur qui identifie la chmabre de compensation -->
    <xsl:variable name ="vClearingHouse" select ="normalize-space(data[@name='CLH'])"/>
    <!-- vClearingHouseColumn contient la colonne CLEARINGHOUSE en fonction du type d'identification-->
    <xsl:variable name ="vClearingHouseColumn">
      <xsl:call-template name="GetActorIdentColumn">
        <xsl:with-param name="pIdent" select="normalize-space(data[@name='CLHI'])"/>
      </xsl:call-template>
    </xsl:variable>
    
    <!-- vMarket contient la valeur qui identifie le marché -->
    <xsl:variable name ="vMarket" select ="normalize-space(data[@name='MKT'])"/>
    <!-- vMarketColumn contient la colonne MARKET en fonction du type d'identification-->
    <xsl:variable name ="vMarketColumn">
      <xsl:call-template name="GetMarketIdentColumn">
        <xsl:with-param name="pIdent" select="normalize-space(data[@name='MKTI'])"/>
      </xsl:call-template>
    </xsl:variable>

    <!-- FI 20171103 [23326] Add vFacility -->
    <xsl:variable name ="vFacility" select ="data[@name='FAC']"/>
    <xsl:variable name ="vFacilityColumn">
      <xsl:call-template name="GetMarketIdentColumn">
        <xsl:with-param name="pIdent" select="data[@name='FACI']"/>
      </xsl:call-template>
    </xsl:variable>
   
    <!-- FI 20170404 [23039] Gestion commoditySpot -->
    <!-- FIL1 et FIL2 peuvent être alimenté lorsque vRecordVersion1 = 'v1' -->
    <xsl:variable name ="vProduct" select ="normalize-space(data[@name='FIL1' or @name='PRD'])"/>
    <xsl:variable name ="vInstrument" select ="normalize-space(data[@name='FIL2' or @name='INS'])"/>
    <xsl:variable name ="vFamily">
      <xsl:choose>
        <xsl:when test="$vProduct='equitySecurityTransaction' or $vProduct='STGequitySecurityTransaction'">
          <xsl:value-of select="'ESE'"/>
        </xsl:when>
        <xsl:when test="$vProduct='commoditySpot'">
          <xsl:value-of select="'COMS'"/>
        </xsl:when>
        <xsl:otherwise>
          <!--Default value -->
          <xsl:value-of select="'ETD'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    
    <xsl:variable name ="vAssetCode" select ="normalize-space(data[@name='ASC'])"/>
    <xsl:variable name ="vAssetCodeIdent" select ="normalize-space(data[@name='ASCI'])"/>
    <xsl:variable name ="vAssetCodeColumn">
      <xsl:call-template name="GetAssetIdentColumn">
        <xsl:with-param name="pIdent" select="$vAssetCodeIdent"/>
      </xsl:call-template>
    </xsl:variable>
      
    <!-- FI 20170404 [23039] compatibilité lorsque vRecordVersion1 = 'v1' (ex DVT ou CTR) -->
    <xsl:variable name ="vContract" select ="normalize-space(data[@name='DVT' or @name='CTR'])"/>
    <xsl:variable name ="vContractColumn">
      <xsl:call-template name="GetDerivativeContractIdentColumn">
        <xsl:with-param name="pIdent" select="normalize-space(data[@name='DVTI' or @name='CTRI'])"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vContractCategory" select ="normalize-space(data[@name='DVTC' or @name='CTRC'])"/>
    <!-- FI 20220823 [XXXXX] Call Template GetContractType -->
    <xsl:variable name="vContractType"> 
      <xsl:call-template name="GetContractType">
        <xsl:with-param name="pRow" select="." />
        <xsl:with-param name="pFamily" select ="$vFamily"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vContractVersion" select ="normalize-space(data[@name='DVTV' or @name='CTRV' ])"/>
    <!-- FI 20220225 [25699] Add vContractSettltMethod and vContractExerciseStyle -->
    <xsl:variable name ="vContractSettltMethod" select ="normalize-space(data[@name='CTRSM'])"/>
    <xsl:variable name ="vContractExerciseStyle" select ="normalize-space(data[@name='CTRES'])"/>

    <!-- FI 20170404 [23039] add vInstr -->
    <xsl:variable name ="vInstr">
      <xsl:choose>
        <xsl:when test ="$vRecordVersion1='v1'">
          <xsl:choose>
            <!-- si Instrument est renseigné -->
            <xsl:when test="string-length($vInstrument) > 0">
              <xsl:value-of select="$vInstrument"/>
            </xsl:when>
            <!-- si Instrument de type Strategy -->
            <xsl:when test="$vIsStrategy = $ConstTrue">
              <xsl:value-of select="$vStrategyType"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:if test ="$vFamily = 'ETD'">
                <xsl:choose>
                  <xsl:when test="string-length($vAssetCode) = 0">
                    <xsl:choose>
                      <xsl:when test="$vContractCategory = 'F'">
                        <xsl:value-of select="'ExchangeTradedFuture'"/>
                      </xsl:when>
                      <xsl:when test="$vContractCategory = 'O'">
                        <xsl:value-of select="'ExchangeTradedOption'"/>
                      </xsl:when>
                    </xsl:choose>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="$ConstUseSQL"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:if>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$vInstrument"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- FI 20170404 [23039] add vInstrColumn -->
    <xsl:variable name ="vInstrColumn">
      <xsl:choose>
        <xsl:when test ="$vRecordVersion1='v1'">
          <xsl:value-of select="'IDENTIFIER'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="GetInstrIdentColumn">
            <xsl:with-param name="pIdent" select="normalize-space(data[@name='INSI'])"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    
    <!-- FI 20170404 [23039] gestion commoditySpot -->
    <xsl:variable name ="vCciPrefixFamily">
      <xsl:choose>
        <xsl:when test="$vFamily='ETD'">
          <xsl:value-of select="'exchangeTradedDerivative'"/>
        </xsl:when>
        <xsl:when test="$vFamily='ESE'">
          <xsl:value-of select="'equitySecurityTransaction'"/>
        </xsl:when>
        <xsl:when test="$vFamily='COMS'">
          <xsl:value-of select="'commoditySpot'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'ERROR'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- RD 20121204 [18240] Import: Trades with data for extensions -->
    <xsl:variable name ="vXMLExtends">
      <xsl:choose>
        <!-- Instrument is Strategy -->
        <xsl:when test="$vIsStrategy = $ConstTrue">
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
      <xsl:choose>
        <xsl:when test="$vIsStrategy = $ConstTrue">
      <xsl:call-template name ="LogRowInfo">
        <xsl:with-param name ="pActionTypeLog" select ="$vActionTypeLog"/>
        <xsl:with-param name ="pRecordTypeLog"  select ="$vRecordTypeLog"/>
            <xsl:with-param name ="pTradeId" select ="$vTradeId"/>
            <xsl:with-param name ="pStrategyType" select ="$vStrategyType"/>
            <xsl:with-param name ="pTNL" select ="$vTotalNumberOfLegs"/>
      </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name ="LogRowInfo">
            <xsl:with-param name ="pActionTypeLog" select ="$vActionTypeLog"/>
            <xsl:with-param name ="pRecordTypeLog"  select ="$vRecordTypeLog"/>
            <xsl:with-param name ="pTradeId" select ="$vTradeId"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
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
              <xsl:value-of select ="$vActionType"/>
            </xsl:attribute>
            <xsl:choose>
              <!-- FI 20170824 [23339] Add actionTypeLeg 'T'  -->
              <xsl:when test="($vActionType='N') or ($vActionType='M') or ($vActionType='G') or ($vActionType='T')">
                <!-- l'action [N]ew peut peut générer un Update ou New -->
                <!-- l'action [M]odify peut peut générer un Update ou New -->
                <!-- l'action [G]ive-up peut peut générer un Update ou New -->
                <!-- l'action [T]rader peut peut générer un Update uniquement -->
                <!-- FI 20170404 [23039] Add pTransactionDate, pBusinessDate parameters -->
                <xsl:call-template name="SQLCheckTrade">
                  <xsl:with-param name="pActionType" select="$vActionType"/>
                  <xsl:with-param name="pRecordType" select="$vRecordType"/>
                  <xsl:with-param name="pTradeId" select="$vTradeId"/>
                  <xsl:with-param name="pResultColumn" select="'IMPORTMODE'"/>
                  <xsl:with-param name="pBusinessDate" select="$vBusinessDate"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name="GetDefaultImportMode">
                  <xsl:with-param name="pActionType" select="$vActionType"/>
                  <xsl:with-param name="pRecordType" select="$vRecordType"/>
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </importMode>
          <!-- ================================================== -->
          <!--                CONDITIONS SECTION                  -->
          <!-- ================================================== -->

          <conditions>
            <!-- Si le tradeId est non spécifié, l'importation termine en erreur -->
            <condition name="IsTradeIdSpecified" datatype="bool">
              <logInfo status="ERROR">
                <code>LOG</code>
                <number>06019</number>
                <data1>
                  <xsl:value-of select="$vLogRowDesc"/>
                </data1>
              </logInfo>
              <xsl:choose>
                <xsl:when test="string-length($vTradeId) > 0">
                  <xsl:text>true</xsl:text>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:text>false</xsl:text>
                </xsl:otherwise>
              </xsl:choose>
            </condition>
            <xsl:choose>
              <!-- FI 20160907 [21831] add vActionType = 'F'-->
              <!-- FI 20170824 [23339] add vActionType = 'T'-->
              <xsl:when test="$vActionType='N' or $vActionType='G' or 
                              $vActionType='U' or $vActionType='M' or $vActionType='F' or $vActionType='T' or
                              $vActionType='S'">
                <xsl:variable name ="logNumber">
                  <xsl:choose>
                    <xsl:when test="$vActionType='N' or $vActionType='G'">
                      <!-- 6020 => Le trade a déjà été importé. -->
                      <xsl:value-of select ="06020" />
                    </xsl:when>
                    <xsl:when test="$vActionType='U' or $vActionType='M' or $vActionType='F'">
                      <!-- 6021 => Le trade n'existe pas ou il est désactivé. -->
                      <xsl:value-of select ="06021" />
                    </xsl:when>
                    <xsl:when test="$vActionType='S'">
                      <!-- 6026 => Le trade n'existe pas, ou il a déjà été désactivé.-->
                      <xsl:value-of select ="06026" />
                    </xsl:when>
                    <xsl:when test="$vActionType='T'">
                      <!-- 6038 => Le trade n'existe pas ou il est désactivé, ou la mise à jour du trader a déjà été effectuée-->
                      <xsl:value-of select ="06038" />
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
                      <xsl:value-of select="$vTradeId"/>
                    </data2>
                  </logInfo>
                  <!-- FI 20170404 [23039] Add  pBusinessDate parameters -->
                  <xsl:call-template name="SQLCheckTrade">
                    <xsl:with-param name="pActionType" select="$vActionType"/>
                    <xsl:with-param name="pRecordType" select="$vRecordType"/>
                    <xsl:with-param name="pTradeId" select="$vTradeId"/>
                    <xsl:with-param name="pResultColumn" select="'ISOK'"/>
                    <xsl:with-param name="pBusinessDate" select="$vBusinessDate"/>
                  </xsl:call-template>
                </condition>
              </xsl:when>
              <xsl:when test="string-length($vActionType) > 0">
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
                      <xsl:value-of select="$vActionType"/>
                    </data2>
                  </logInfo>
                  <xsl:text>false</xsl:text>
                </condition>
              </xsl:otherwise>
            </xsl:choose>
            <!-- FI 20170829 [XXXXX] -->
            <xsl:if test="$gIsModegateway=$ConstTrue">
              <condition name="IsMIDSpecified" datatype="bool">
                <logInfo status="WARNING">
                  <code>LOG</code>
                  <number>06039</number>
                  <data1>
                    <xsl:value-of select="$vLogRowDesc"/>
                  </data1>
                </logInfo>
                <xsl:choose>
                  <xsl:when test ="string-length(normalize-space(data[@name='MID']))>0">true</xsl:when>
                  <xsl:otherwise>false</xsl:otherwise>
                </xsl:choose>
              </condition>
            </xsl:if>
            <xsl:for-each select='msxsl:node-set($vXMLExtends)/extend'>
              <xsl:variable name ="vExtendPosition" select="position()"/>
              <condition name="IsExtendExist" datatype="bool">
                <logInfo status="ERROR">
                  <xsl:choose>
                    <xsl:when test="$vInstr = $ConstUseSQL">
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
                        <xsl:value-of select="$vAssetCode"/>
                        <xsl:if test="$vAssetCodeColumn != 'IDENTIFIER'">
                          <xsl:value-of select="concat(' (',$vAssetCodeColumn,')')"/>
                        </xsl:if>
                      </data4>
                      <data5>
                        <xsl:value-of select="$vTradeId"/>
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
                        <xsl:value-of select="$vInstr"/>
                      </data4>
                      <data5>
                        <xsl:value-of select="$vTradeId"/>
                      </data5>
                    </xsl:otherwise>
                  </xsl:choose>
                </logInfo>
                <!-- FI 20170404 [23039] parameters pInstr et pInstrColumn -->
                <xsl:call-template name="SQLExtendAttribute">
                  <xsl:with-param name="pExtIdentifier" select="@identifier"/>
                  <xsl:with-param name="pExtDet" select="@detail"/>
                  <xsl:with-param name="pResultColumn" select="'ISEXTENDEXIST'"/>
                  <xsl:with-param name="pInstr" select="$vInstr"/>
                  <xsl:with-param name="pInstrColumn" select="$vInstrColumn"/>
                  <xsl:with-param name="pAssetCode" select="$vAssetCode"/>
                  <xsl:with-param name="pAssetColumn" select="$vAssetCodeColumn"/>
                  <xsl:with-param name="pMarket" select="$vMarket"/>
                  <xsl:with-param name="pMarketColumn" select="$vMarketColumn"/>
                  <xsl:with-param name="pClearingBusinessDate" select="$vBusinessDate"/>
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
                <!-- FI 20170404 [23039] comportement spécifique si version v1 
                Lecture potentielle de l'instrument à partir de l'asset
                -->
                <xsl:when test ="$vRecordVersion1='v1'">
                  <xsl:choose>
                    <xsl:when test="$vInstr = $ConstUseSQL">
                      <xsl:call-template name="ETD_SQLInstrumentAndAsset">
                        <xsl:with-param name="pBusinessDate" select="$vBusinessDate"/>
                        <xsl:with-param name="pAssetCode" select="$vAssetCode"/>
                        <xsl:with-param name="pAssetColumn" select="$vAssetCodeColumn"/>
                        <xsl:with-param name="pMarket" select="$vMarket"/>
                        <xsl:with-param name="pMarketColumn" select="$vMarketColumn"/>
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="$vInstr"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:when>
                <xsl:otherwise>
                  <!-- FI 20170404 [23039] template SQLInstr si version v2 -->
                  <xsl:call-template name="SQLInstr">
                    <xsl:with-param name="pInstr" select="$vInstr"/>
                    <xsl:with-param name="pInstrColumn" select="$vInstrColumn"/>
                    <xsl:with-param name="pIsMandatory" select="$ConstTrue"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </parameter>
            <!-- template from instrument default -->
            <parameter name="http://www.efs.org/otcml/tradeImport/templateIdentifier" datatype="string">
              <xsl:value-of select="$vSpheresTemplateIdentifier"/>
            </parameter>
            <parameter name="http://www.efs.org/otcml/tradeImport/screen" datatype="string">
              <xsl:value-of select="$vSpheresScreen"/>
            </parameter>
            <parameter name="http://www.efs.org/otcml/tradeImport/identifier" datatype="string">
              <!-- FI 20130301 [18465] appel systématique à SQLTradeIdentifier  -->
              <!-- FI 20170404 [23039] pBusinessDate parameters -->
              <xsl:call-template name="SQLTradeIdentifier">
                <xsl:with-param name="pActionType" select="$vActionType"/>
                <xsl:with-param name="pTradeId" select="$vTradeId"/>
                <xsl:with-param name="pResultColumn" select="'IDENTIFIER'"/>
                <xsl:with-param name="pBusinessDate" select="$vBusinessDate"/>
              </xsl:call-template>
            </parameter>
            <parameter name="http://www.efs.org/otcml/tradeImport/displayname" datatype="string">
              <xsl:value-of select="$vSpheresDisplayname"/>
            </parameter>
            <parameter name="http://www.efs.org/otcml/tradeImport/description" datatype="string">
              <xsl:value-of select="$vSpheresDescription"/>
            </parameter>
            <parameter name="http://www.efs.org/otcml/tradeImport/extllink" datatype="string">
              <xsl:value-of select="$vTradeId"/>
            </parameter>
            <parameter name="http://www.efs.org/otcml/tradeImport/feeCalculation" datatype="string">
              <xsl:value-of select="$vSpheresFeeCalculation"/>
            </parameter>
            <parameter name="http://www.efs.org/otcml/tradeImport/isApplyPartyTemplate" datatype="bool">
              <xsl:value-of select="$vSpheresPartyTemplate"/>
            </parameter>
            <!-- FI 20160829 [22412] Add parameter isApplyClearingTemplate -->
            <parameter name="http://www.efs.org/otcml/tradeImport/isApplyClearingTemplate" datatype="bool">
              <xsl:value-of select="$vSpheresClearingTemplate"/>
            </parameter>
            <!--RD 20190718 Use Task parameter ISPOSTTOEVENTSGEN--> 
            <parameter name="http://www.efs.org/otcml/tradeImport/isPostToEventsGen" datatype="bool">
              <xsl:value-of select="$gIsPostToEventsGen"/>
            </parameter>
            <xsl:if test="string-length($vSpheresValidationXSD) > 0">
              <parameter name="http://www.efs.org/otcml/tradeImport/isApplyValidationXSD" datatype="bool">
                <xsl:value-of select="$vSpheresValidationXSD"/>
              </parameter>
            </xsl:if>
            <xsl:if test="string-length($vSpheresValidationRules) > 0">
              <parameter name="http://www.efs.org/otcml/tradeImport/isApplyValidationRules" datatype="bool">
                <xsl:value-of select="$vSpheresValidationRules"/>
              </parameter>
            </xsl:if>
            <!-- FI 20160907 [21831] Add UpdateMode Parameter -->
            <xsl:if test ="$vActionType = 'F'">
              <parameter name="http://www.efs.org/otcml/tradeImport/UpdateMode" datatype="string">UpdateFeesOnly</parameter>
            </xsl:if>
            <!-- FI 20170824 [23339] Add UpdateMode Parameter -->
            <xsl:if test ="$vActionType = 'T'">
              <parameter name="http://www.efs.org/otcml/tradeImport/UpdateMode" datatype="string">UpdateTraderOnly</parameter>
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

            <!-- FI 20170404 [23039] add variable vDynamicDataMarketIdentifier de type StringDynamicData 
             vDynamicDataMarketIdentifier permet d'obtenir le marché
            -->
            <xsl:variable name ="vDynamicDataMarketIdentifier">
              <xsl:call-template name ="GetDynamicDataMarketIdentifier">
                <xsl:with-param name ="pFamily" select="$vFamily"/>
                <xsl:with-param name ="pBusinessDate" select="$vBusinessDate"/>
                <xsl:with-param name ="pMarket" select="$vMarket"/>
                <xsl:with-param name ="pMarketColumn" select="$vMarketColumn"/>
                <xsl:with-param name ="pAssetCode" select="$vAssetCode"/>
                <xsl:with-param name ="pAssetColumn" select="$vAssetCodeColumn"/>
                <xsl:with-param name ="pContrat" select="$vContract"/>
                <xsl:with-param name ="pContractColumn" select="$vContractColumn"/>
                <xsl:with-param name ="pContractVersion" select="$vContractVersion"/>
                <xsl:with-param name ="pContractCategory" select="$vContractCategory"/>
                <xsl:with-param name ="pContractType" select="$vContractType"/>
                <xsl:with-param name ="pContractSettltMethod" select="$vContractSettltMethod"/>
                <xsl:with-param name ="pContractExerciseStyle" select="$vContractExerciseStyle"/>
                <xsl:with-param name ="pClearingHouse" select="$vClearingHouse"/>
                <xsl:with-param name ="pClearingHouseColumn" select="$vClearingHouseColumn"/>
              </xsl:call-template>
            </xsl:variable>

            <!-- FI 20170404 [23039] add variable vDynamicDataCssIdentifier de type StringDynamicData 
              vDynamicDataCssIdentifier permet d'obtenir le css à partir du marché 
            -->
            <xsl:variable name ="vDynamicDataCssIdentifier">
              <xsl:call-template name ="GetDynamicDataCssIdentifier">
                <xsl:with-param name="pDynamicDataMarketIdentifier">
                  <xsl:copy-of select ="$vDynamicDataMarketIdentifier"/>
                </xsl:with-param>
              </xsl:call-template>
            </xsl:variable>

            <!-- Facility Cci-->
            <!-- FI 20171103 [23326] Add Facility Cci -->
            <xsl:variable name ="vDynamicDataFacilityIdentifier">
              <xsl:choose>
                <xsl:when test ="$vRecordVersion1='v1'">
                  <dynamicValue>
                    <xsl:call-template name="GetSQLFacilityFromMarket">
                      <xsl:with-param name="pResultColumn" select="'FACILITY'"/>
                      <xsl:with-param name="pDynamicDataMarketIdentifier">
                        <xsl:copy-of select ="$vDynamicDataMarketIdentifier"/>
                      </xsl:with-param>
                    </xsl:call-template>
                  </dynamicValue>
                </xsl:when>
                <xsl:otherwise>
                  <dynamicValue>
                    <xsl:call-template name="SQLMarket">
                      <xsl:with-param name="pMarket" select="$vFacility"/>
                      <xsl:with-param name="pMarketColumn" select="$vFacilityColumn"/>
                       <!--<xsl:with-param name="pResultColumn" select="'FIXML_SECURITYEXCHANGE'"/>-->
                    </xsl:call-template>
                  </dynamicValue>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId" select ="'tradeHeader_market1_identifier'"/>
              <xsl:with-param name="pDataType" select ="'string'"/>
              <xsl:with-param name="pDynamicValue" select ="msxsl:node-set($vDynamicDataFacilityIdentifier)/dynamicValue"/>
              <xsl:with-param name="pIsMissingMode" select ="$isAllowDataMissing"/>
              <xsl:with-param name="pIsMissingModeSpecified" select ="$ConstTrue"/>
              <xsl:with-param name="pIsMandatory" select ="$ConstTrue"/>
            </xsl:call-template>
            
            <!-- Market Cci-->
            <!-- FI 20170404 [23039] add vClientIdExch -->
            <xsl:variable name ="vClientIdExch">
              <xsl:choose >
                <xsl:when test ="$vFamily = 'ETD' or $vFamily = 'ESE'">
                  <xsl:value-of select ="concat($vCciPrefixStrategy,$vCciPrefixFamily,'_FIXML_TrdCapRpt_Instrmt_Exch')"/>
                </xsl:when>
                <xsl:when test ="$vFamily='COMS'">
                  <!-- FI 20180228 [23814] -->
                  <xsl:value-of select ="concat($vCciPrefixStrategy, $vCciPrefixFamily, '_{commodityPhysicalLeg}_commodityAsset_exchangeId')"/>
                </xsl:when>
              </xsl:choose>
            </xsl:variable>
            <xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId" select ="$vClientIdExch"/>
              <xsl:with-param name="pDataType" select ="'string'"/>
              <!-- FI 20170718 [23326] utilisation du paramètre pDynamicValue -->
              <!--<xsl:with-param name="pXMLDynamicValue">
                <xsl:copy-of select ="msxsl:node-set($vDynamicDataMarketIdentifier)/dynamicValue"/>
              </xsl:with-param>-->
              <xsl:with-param name="pDynamicValue" select ="msxsl:node-set($vDynamicDataMarketIdentifier)/dynamicValue"/>
              <xsl:with-param name="pIsMissingMode" select ="$isAllowDataMissing"/>
              <xsl:with-param name="pIsMissingModeSpecified" select ="$ConstTrue"/>
              <xsl:with-param name="pIsMandatory" select ="$ConstTrue"/>
            </xsl:call-template>

            <!-- Comment Cci-->
            <!-- FI 20170718 [23326] cci ajouté uniquement si data[@name='CMT'] existe -->
            <xsl:if test ="data[@name='CMT']">
              <xsl:variable name ="vComment" select ="normalize-space(data[@name='CMT'])"/>
              <xsl:variable name ="vClientIdTxt">
                <xsl:choose >
                  <xsl:when test ="$vFamily='ETD' or $vFamily='ESE'">
                    <xsl:value-of select ="concat($vCciPrefixStrategy,$vCciPrefixFamily,'_FIXML_TrdCapRpt_RptSide_Txt')"/>
                  </xsl:when>
                  <xsl:when test ="$vFamily='COMS'">
                    <xsl:value-of select ="concat($vCciPrefixStrategy,$vCciPrefixFamily,'_RptSide_Txt')"/>
                  </xsl:when>
                </xsl:choose>
              </xsl:variable>
              <xsl:call-template name="CustomCaptureInfo">
                <xsl:with-param name="pClientId" select="$vClientIdTxt"/>
                <xsl:with-param name="pDataType" select ="'string'"/>
                <xsl:with-param name="pValue" select="$vComment"/>
                <xsl:with-param name="pIsMandatory" select ="$ConstFalse"/>
              </xsl:call-template>
            </xsl:if>

            <!-- ======================================================================================= -->
            <!--                                   PARTIES SECTION                                       -->
            <!-- ======================================================================================= -->
            <xsl:variable name ="vXMLTradePartiesAndBrokers">
              <xsl:choose>
                <!-- Allocation -->
                <xsl:when test ="$vRecordType='A'">
                  <!-- FI 20170718 [23326] call GetTradePartiesAndBrokersAlloc -->
                  <xsl:call-template name="GetTradePartiesAndBrokersAlloc">
                    <xsl:with-param name="pRow" select ="."/>
                    <xsl:with-param name="pBusinessDate" select ="$vBusinessDate"/>
                    <xsl:with-param name="pFamily" select ="$vFamily"/>
                    <xsl:with-param name="pMarket" select ="$vMarket"/>
                    <xsl:with-param name="pMarketColumn" select ="$vMarketColumn"/>
                    <xsl:with-param name="pAssetCode" select ="$vAssetCode"/>
                    <xsl:with-param name="pAssetColumn" select ="$vAssetCodeColumn"/>
                    <xsl:with-param name="pContract" select ="$vContract"/>
                    <xsl:with-param name="pContractColumn" select ="$vContractColumn"/>
                    <xsl:with-param name="pContractVersion" select ="$vContractVersion"/>
                    <xsl:with-param name="pContractCategory" select ="$vContractCategory"/>
                    <xsl:with-param name="pContractType" select ="$vContractType"/>
                    <xsl:with-param name="pContractSettltMethod" select="$vContractSettltMethod"/>
                    <xsl:with-param name="pContractExerciseStyle" select="$vContractExerciseStyle"/>
                    <xsl:with-param name="pClearingHouseColumn" select="$vClearingHouseColumn"/>
                    <xsl:with-param name="pDynamicDataMarketIdentifier" select="$vDynamicDataMarketIdentifier"/>
                    <xsl:with-param name="pDynamicDataCssIdentifier" select="$vDynamicDataCssIdentifier"/>
                  </xsl:call-template>
                </xsl:when>
                <!-- Execution/Intermediation-->
                <xsl:otherwise>
                  <!-- FI 20170718 [23326] call GetTradePartiesAndBrokersExecution -->
                  <xsl:call-template name="GetTradePartiesAndBrokersExecution">
                    <xsl:with-param name="pRow" select ="."/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:variable name ="vTradePartiesAndBrokersNode" select ="msxsl:node-set($vXMLTradePartiesAndBrokers)"/>
            <!-- FI 20170726 [23326] add variable  vXMLCcisTradePartiesAndBrokers -->
            <xsl:variable name ="vXMLCcisTradePartiesAndBrokers">
              <xsl:choose>
                <!-- Allocation-->
                <xsl:when test ="$vRecordType='A'">
                  <!-- FI 20170718 [23326] Call GetCcisTradePartiesAlloc -->
                  <xsl:call-template name ="GetCcisTradePartiesAlloc">
                    <xsl:with-param name="pRow" select ="."/>
                    <xsl:with-param name="pTradePartiesAndBrokersNode" select="$vTradePartiesAndBrokersNode"/>
                  </xsl:call-template>
                </xsl:when>
                <!-- Execution/Intermediation-->
                <xsl:otherwise>
                  <!-- FI 20170718 [23326] Call GetCcisTradePartiesExecution -->
                  <xsl:call-template name ="GetCcisTradePartiesExecution">
                    <xsl:with-param name="pRow" select ="."/>
                    <xsl:with-param name="pTradePartiesAndBrokersNode" select="$vTradePartiesAndBrokersNode"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <!-- FI 20170726 [23326] copy-of vXMLCcisTradePartiesAndBrokers -->
            <xsl:copy-of select ="$vXMLCcisTradePartiesAndBrokers"/>
            
            <!-- FI 20170718 [23326] si  mode Debug = 'SectionParty' 
            affichage de la variable vXMLTradePartiesAndBrokers
            affichage des ccis spécifiques aux parties -->
            <xsl:if test ="$pDebugMode = 'SectionParty'">
              <sectionParty>
                <vXMLTradePartiesAndBrokers>
                  <xsl:copy-of select ="$vXMLTradePartiesAndBrokers"/>
                </vXMLTradePartiesAndBrokers>
                <ccisParties>
                  <!-- FI 20170726 [23326] copy-of vXMLCcisTradePartiesAndBrokers -->
                  <xsl:copy-of select ="$vXMLCcisTradePartiesAndBrokers"/>
                </ccisParties>
              </sectionParty>
            </xsl:if>


            <!-- ======================================================================================= -->
            <!--                                   DATES SECTION                                         -->
            <!-- ======================================================================================= -->

            <!-- FI 20171103 [23326] Alimentation de orderEntered et executionDateTime à la place de tradeDate et timeStamping -->
            <!-- TransactionDate Cci-->
            <!--<xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId" select ="'tradeHeader_tradeDate'"/>
              <xsl:with-param name="pDataType" select ="'date'"/>
              <xsl:with-param name="pValue" select="$vTransactionDate"/>
            </xsl:call-template>-->
            
            <!-- Horodatage Cci-->
            <!--<xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId" select ="'tradeHeader_timeStamping'"/>
              <xsl:with-param name="pDataType" select ="'time'"/>
              <xsl:with-param name="pRegex" select ="'RegexLongTime'"/>
              <xsl:with-param name="pValue" select="data[@name='TDT']"/>
            </xsl:call-template>-->

            <!-- FI 20171103 [23326] Add recherche du TIMEZONE asoscié au Facility (lui même déterminé à partir du market )-->
            <xsl:variable name="vDynamicDataTimeZoneV1">
              <xsl:if test ="$vRecordVersion1='v1'">
                <xsl:call-template name="GetDynamicDataTimeZoneV1">
                  <xsl:with-param name ="pDynamicDataFacilityIdentifier" select="$vDynamicDataFacilityIdentifier"/>
                </xsl:call-template>
              </xsl:if>
            </xsl:variable>

            <!-- FI 20171103 [23326] Add -->
            <xsl:variable name ="vExecutionDateTime">
              <xsl:choose>
                <xsl:when test ="$vRecordVersion1='v1'">
                  <xsl:value-of select="data[@name='TDT']"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="data[@name='EDT']"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:variable name ="vDynamicDataExecutionDateTimeTimeZone">
              <xsl:choose>
                <xsl:when test ="$vRecordVersion1='v1'">
                  <xsl:copy-of select="$vDynamicDataTimeZoneV1"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="GetDynamicDataTimeZoneV2">
                    <xsl:with-param name ="pName" select="'EDTTZ'"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            
            <!-- FI 20171103 [23326] Add 
            vOrderEntered = vExecutionDateTime s'il est non renseigné
            -->
            <xsl:variable name ="vOrderEntered">
              <xsl:choose>
                <xsl:when test ="string-length(data[@name='ODT']) > 0">
                  <xsl:value-of select ="data[@name='ODT']"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:copy-of select="$vExecutionDateTime"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            
            <!-- FI 20171103 [23326] Add 
            vDynamicDataOrderEnteredTimeZone = $vDynamicDataExecutionDateTimeTimeZone s'il est non renseigné
            -->
            <xsl:variable name ="vDynamicDataOrderEnteredTimeZone">
              <xsl:choose>
                <xsl:when test ="string-length(data[@name='ODT']) > 0">
                  <xsl:call-template name="GetDynamicDataTimeZoneV2">
                    <xsl:with-param name ="pName" select="'ODTTZ'"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:copy-of select="$vDynamicDataExecutionDateTimeTimeZone"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            

            <!-- orderEntered Cci => Convert en UTC de $vOrderEntered -->
            <xsl:variable name="vDynamicDataOrderEntered">
              <xsl:call-template name ="GetDynamicDataConvertTimestampToUtc">
                <xsl:with-param name="pTimestamp" select="$vOrderEntered"/>
                <xsl:with-param name="pDynamicDataTimeZone" select="$vDynamicDataOrderEnteredTimeZone"/>
              </xsl:call-template>
            </xsl:variable>
            <xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId" select ="'tradeHeader_market1_orderEntered'"/>
              <xsl:with-param name="pDataType" select ="'datetimeoffset'"/>
              <xsl:with-param name="pIsMandatory" select ="$ConstTrue"/>
              <xsl:with-param name="pDynamicValue" select ="msxsl:node-set($vDynamicDataOrderEntered)/dynamicValue"/>
            </xsl:call-template>

            <!-- executionDateTime Cci => Convert en UTC de $vExecutionDateTime-->
            <xsl:variable name="vDynamicDataExecutionDatetime">
              <xsl:call-template name ="GetDynamicDataConvertTimestampToUtc">
                <xsl:with-param name="pTimestamp" select="$vExecutionDateTime"/>
                <xsl:with-param name="pDynamicDataTimeZone" select="$vDynamicDataExecutionDateTimeTimeZone"/>
              </xsl:call-template>
            </xsl:variable>
            <xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId" select ="'tradeHeader_market1_executionDateTime'"/>
              <xsl:with-param name="pDataType" select ="'datetimeoffset'"/>
              <xsl:with-param name="pIsMandatory" select ="$ConstTrue"/>
              <xsl:with-param name="pDynamicValue" select ="msxsl:node-set($vDynamicDataExecutionDatetime)/dynamicValue"/>
            </xsl:call-template>
            
            <!-- TVTIC -->
            <xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId" select ="'tradeHeader_market1_tvtic'"/>
              <xsl:with-param name="pDataType" select ="'string'"/>
              <xsl:with-param name="pIsMandatory" select ="$ConstFalse"/>
              <xsl:with-param name="pValue" select ="normalize-space(data[@name='TVTIC'])"/>
            </xsl:call-template>

            <!-- ClearingBusinessDate Cci-->
            <!-- FI 20171103 [23326] le cci se nomme désormais tradeHeader_market1_clearedDate -->
            <!--<xsl:variable name ="vClientIdBizDt">
              <xsl:choose >
                <xsl:when test ="$vFamily = 'ETD' or $vFamily = 'ESE'">
                  <xsl:value-of select ="concat($vCciPrefixStrategy,$vCciPrefixFamily,'_FIXML_TrdCapRpt_BizDt')"/>
                </xsl:when>
                <xsl:when test ="$vFamily = 'COMS'">
                  <xsl:value-of select ="tradeHeader_businessDate"/>
                </xsl:when>
              </xsl:choose>
            </xsl:variable>-->
            <xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId" select ="'tradeHeader_market1_clearedDate'"/>
              <xsl:with-param name="pDataType" select ="'date'"/>
              <xsl:with-param name="pValue" select="$vBusinessDate"/>
              <xsl:with-param name="pIsMandatory" select ="$ConstTrue"/>
            </xsl:call-template>
            

            <!-- ======================================================================================= -->
            <!--                                   LEGS SECTION                                          -->
            <!-- ======================================================================================= -->
            <xsl:choose>
              <!-- Instrument is Strategy -->
              <xsl:when test="$vIsStrategy = $ConstTrue">
                <xsl:for-each select ="$vAllTradeRows">
                  <xsl:sort select="data[@name='LSN']"/>
                  <xsl:variable name ="vLegNumber" select="position()"/>

                  <!-- ....................................................................................... -->
                  <!--                      Strategy Specific Ccis SECTION                                     -->
                  <!-- ....................................................................................... -->
                  <!-- Buyer Cci-->
                  <xsl:variable name ="vBuyer" select ="data[@name='BYA']"/>
                  <xsl:call-template name="CustomCaptureInfo">
                    <xsl:with-param name="pClientId" select ="concat($vCciPrefixStrategy,$vCciPrefixFamily,$vLegNumber,'_buyer')"/>
                    <xsl:with-param name="pDataType" select ="'string'"/>
                    <xsl:with-param name="pIsMandatory" select ="$ConstTrue"/>
                    <xsl:with-param name="pIsMissingMode" select ="$isAllowDataMissDealer"/>
                    <xsl:with-param name="pIsMissingModeSpecified" select ="$ConstTrue"/>
                    <xsl:with-param name="pXMLDynamicValue">
                      <!-- FI 20180119 [23732] parsingValue[1] or parsingValue[2]-->
                      <xsl:copy-of select ="$vTradePartiesAndBrokersNode/party[parsingValue[1]=$vBuyer or parsingValue[2]=$vBuyer][1]/dynamicValue"/>
                    </xsl:with-param>
                  </xsl:call-template>
                  <!-- Seller Cci-->
                  <xsl:variable name ="vSeller" select ="data[@name='SLA']"/>
                  <xsl:call-template name="CustomCaptureInfo">
                    <xsl:with-param name="pClientId" select ="concat($vCciPrefixStrategy,$vCciPrefixFamily,$vLegNumber,'_seller')"/>
                    <xsl:with-param name="pDataType" select ="'string'"/>
                    <xsl:with-param name="pIsMandatory" select ="$ConstTrue"/>
                    <xsl:with-param name="pIsMissingMode" select ="$isAllowDataMissDealer"/>
                    <xsl:with-param name="pIsMissingModeSpecified" select ="$ConstTrue"/>
                    <xsl:with-param name="pXMLDynamicValue">
                      <!-- FI 20180119 [23732] parsingValue[1] or parsingValue[2]-->
                      <xsl:copy-of select ="$vTradePartiesAndBrokersNode/party[parsingValue[1]=$vSeller or parsingValue[2]=$vSeller][1]/dynamicValue"/>
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
                      <xsl:copy-of select ="exsl:node-set($vXMLTradePartiesAndBrokers)/party[normalize-space(parsingValue)=$vSeller][1]/dynamicValue"/>
                    </xsl:with-param>
                  </xsl:call-template>-->
                  <!-- ....................................................................................... -->
                  <!--                             Common Leg Ccis SECTION                                     -->
                  <!-- ....................................................................................... -->
                  <xsl:call-template name="CcisLeg">
                    <xsl:with-param name ="pLegNumber" select="$vLegNumber"/>
                    <!-- FI 20170718 [23326] Usage d'un select pour alimenter pLegRow -->
                    <xsl:with-param name ="pLegRow" select="." />
                    <xsl:with-param name ="pCciPrefix" select="$vCciPrefixStrategy"/>
                    <xsl:with-param name ="pCciPrefixFamily" select="$vCciPrefixFamily"/>
                    <xsl:with-param name ="pRecordType" select="$vRecordType"/>
                    <xsl:with-param name ="pMarket" select="$vMarket"/>
                    <xsl:with-param name ="pMarketColumn" select="$vMarketColumn"/>
                    <xsl:with-param name ="pProduct" select="$vProduct"/>
                    <xsl:with-param name ="pFamily" select="$vFamily"/>
                    <xsl:with-param name ="pBusinessDate" select="$vBusinessDate"/>
                    <xsl:with-param name ="pIsAllowDataMissing" select="$isAllowDataMissing"/>
                  </xsl:call-template>
                </xsl:for-each >
              </xsl:when>
              <!-- Instrument is NOT Strategy -->
              <xsl:otherwise>
                <xsl:choose>
                  <!-- Allocation-->
                  <xsl:when test ="$vRecordType='A'">
                    <!-- ....................................................................................... -->
                    <!--                      Allocation Specific Ccis SECTION                                   -->
                    <!-- ....................................................................................... -->
                    <!-- Side Cci-->
                    <!-- FI 20170404 [23039] Add vClientIdSide -->
                    <xsl:variable name ="vClientIdSide">
                      <xsl:choose >
                        <xsl:when test ="$vFamily='ETD' or $vFamily='ESE'">
                          <xsl:value-of select ="concat($vCciPrefixStrategy,$vCciPrefixFamily,'_FIXML_TrdCapRpt_RptSide_Side')"/>
                        </xsl:when>
                        <xsl:when test ="$vFamily='COMS'">
                          <xsl:value-of select ="concat($vCciPrefixStrategy,$vCciPrefixFamily,'_RptSide_Side')"/>
                        </xsl:when>
                      </xsl:choose>
                    </xsl:variable>
                    <xsl:call-template name="CustomCaptureInfo">
                      <xsl:with-param name="pClientId" select ="$vClientIdSide"/>
                      <xsl:with-param name="pDataType" select ="'string'"/>
                      <xsl:with-param name="pValue" select="normalize-space(data[@name='ALS'])"/>
                    </xsl:call-template>

                    <!-- ....................................................................................... -->
                    <!--                             Common Leg Ccis SECTION                                     -->
                    <!-- ....................................................................................... -->
                    <xsl:call-template name="CcisLeg">
                      <xsl:with-param name ="pLegNumber"/>
                      <!-- FI 20170718 [23326] Usage d'un select pour alimenter pLegRow -->
                      <xsl:with-param name ="pLegRow" select="." />
                      <xsl:with-param name ="pCciPrefix" select="$vCciPrefixStrategy"/>
                      <xsl:with-param name ="pCciPrefixFamily" select="$vCciPrefixFamily"/>
                      <xsl:with-param name ="pRecordType" select="$vRecordType"/>
                      <xsl:with-param name ="pMarket" select="$vMarket"/>
                      <xsl:with-param name ="pMarketColumn" select="$vMarketColumn"/>
                      <xsl:with-param name ="pProduct" select="$vProduct"/>
                      <xsl:with-param name ="pFamily" select="$vFamily"/>
                      <xsl:with-param name ="pBusinessDate" select="$vBusinessDate"/>
                      <xsl:with-param name ="pIsAllowDataMissing" select="$isAllowDataMissing"/>
                    </xsl:call-template>

                    <!-- ....................................................................................... -->
                    <!--                    Electronic Trade and Electronic Order Ccis SECTION                   -->
                    <!-- ....................................................................................... -->
                    <!-- FI 20180214 [23774] call template CcisElectronicTradeElectronicOrder -->
                    <xsl:call-template name="CcisElectronicTradeElectronicOrder">
                      <xsl:with-param name ="pLegRow" select="." />
                      <xsl:with-param name ="pCciPrefix" select="$vCciPrefixStrategy"/>
                      <xsl:with-param name ="pCciPrefixFamily" select="$vCciPrefixFamily"/>
                      <xsl:with-param name ="pFamily" select="$vFamily"/>
                    </xsl:call-template>

                    <!-- ....................................................................................... -->
                    <!--                    Strategy on trade ALLOC Ccis SECTION                                 -->
                    <!-- ....................................................................................... -->
                    <!-- FI 20180214 [23774] call template CcisAllocStrategy -->
                    <xsl:if test ="$vFamily='ETD'">
                      <xsl:call-template name="CcisAllocStrategy">
                        <xsl:with-param name ="pLegRow" select="." />
                        <xsl:with-param name ="pCciPrefix" select="$vCciPrefixStrategy"/>
                        <xsl:with-param name ="pCciPrefixFamily" select="$vCciPrefixFamily"/>
                          </xsl:call-template>
                    </xsl:if>
                        </xsl:when>
                  
                  <!-- Execution/Intermediation-->
                  <xsl:otherwise>
                    <!-- ....................................................................................... -->
                    <!--                             Common Leg Ccis SECTION                                     -->
                    <!-- ....................................................................................... -->
                    <xsl:call-template name ="CcisLeg">
                      <xsl:with-param name ="pLegNumber"/>
                      <!-- FI 20170718 [23326] Usage d'un select pour alimenter pLegRow -->
                      <xsl:with-param name ="pLegRow" select="." />
                      <xsl:with-param name ="pCciPrefix" select="$vCciPrefixStrategy"/>
                      <xsl:with-param name ="pCciPrefixFamily" select="$vCciPrefixFamily"/>
                      <xsl:with-param name ="pRecordType" select="$vRecordType"/>
                      <xsl:with-param name ="pMarket" select="$vMarket"/>
                      <xsl:with-param name ="pMarketColumn" select="$vMarketColumn"/>
                      <xsl:with-param name ="pProduct" select="$vProduct"/>
                      <xsl:with-param name ="pFamily" select="$vFamily"/>
                      <xsl:with-param name ="pBusinessDate" select="$vBusinessDate"/>
                      <xsl:with-param name ="pIsAllowDataMissing" select="$isAllowDataMissing"/>
                    </xsl:call-template>

                    <!-- ....................................................................................... -->
                    <!--                      Order Specific Ccis SECTION                                        -->
                    <!-- ....................................................................................... -->
                    <!--Buyer Cci-->
                    <xsl:call-template name="CustomCaptureInfo">
                      <xsl:with-param name="pClientId" select ="concat($vCciPrefixFamily,'_buyer')"/>
                      <xsl:with-param name="pDataType" select ="'string'"/>
                      <xsl:with-param name="pIsMissingMode" select ="$isAllowDataMissDealer"/>
                      <xsl:with-param name="pIsMissingModeSpecified" select ="$ConstTrue"/>
                      <!-- FI 20170718 [23326] Usage du paramètre pDynamicValue -->
                      <xsl:with-param name="pDynamicValue" select ="$vTradePartiesAndBrokersNode/party[@name=$ConstBuyer]/dynamicValue"/>
                    </xsl:call-template>
                    <!--Seller Cci-->
                    <xsl:call-template name="CustomCaptureInfo">
                      <xsl:with-param name="pClientId" select ="concat($vCciPrefixFamily,'_seller')"/>
                      <xsl:with-param name="pDataType" select ="'string'"/>
                      <xsl:with-param name="pIsMissingMode" select ="$isAllowDataMissDealer"/>
                      <xsl:with-param name="pIsMissingModeSpecified" select ="$ConstTrue"/>
                      <!-- FI 20170718 [23326] Usage du paramètre pDynamicValue -->
                      <xsl:with-param name="pDynamicValue" select ="$vTradePartiesAndBrokersNode/party[@name=$ConstSeller]/dynamicValue"/>
                    </xsl:call-template>
                    <!--Side Cci-->
                    <xsl:call-template name="CustomCaptureInfo">
                      <xsl:with-param name="pClientId" select ="concat($vCciPrefixFamily,'_FIXML_TrdCapRpt_RptSide_Side')"/>
                      <xsl:with-param name="pDataType" select ="'string'"/>
                      <xsl:with-param name="pValue" select="'1'"/>
                    </xsl:call-template>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:otherwise>
            </xsl:choose>

            <!-- ======================================================================================= -->
            <!--                                   EXTEND SECTION                                        -->
            <!-- ======================================================================================= -->
            <!-- RD 20121204 [18240] Import: Trades with data for extensions -->
            <!-- FI 20170404 [23039] rem pInstrumentIdentifier, add pInstr et pInstrColumn -->
            <xsl:call-template name="CcisExtends">
              <xsl:with-param name="pXMLExtends">
                <xsl:copy-of select="$vXMLExtends"/>
              </xsl:with-param>
              <xsl:with-param name="pInstr" select="$vInstr"/>
              <xsl:with-param name="pInstrColumn" select="$vInstrColumn"/>
              <xsl:with-param name="pAssetCode" select="$vAssetCode"/>
              <xsl:with-param name="pAssetColumn" select="$vAssetCodeColumn"/>
              <xsl:with-param name="pMarket" select="$vMarket"/>
              <xsl:with-param name="pMarketColumn" select="$vMarketColumn"/>
              <xsl:with-param name="pClearingBusinessDate" select="$vBusinessDate"/>
            </xsl:call-template>

            <!-- ======================================================================================= -->
            <!--                                   FEES SECTION                                          -->
            <!-- ======================================================================================= -->
            <xsl:variable name ="vXMLFees">
              <xsl:choose>
                <!-- Instrument is Strategy -->
                <xsl:when test="$vIsStrategy = $ConstTrue">
                  <xsl:for-each select ="$vAllTradeRows">
                    <xsl:call-template name="XMLFees">
                      <xsl:with-param name="pXMLRow">
                        <xsl:copy-of select ="."/>
                      </xsl:with-param>
                      <xsl:with-param name="pXMLTradePartiesAndBrokers">
                        <xsl:copy-of select ="$vXMLTradePartiesAndBrokers"/>
                      </xsl:with-param>
                      <xsl:with-param name="pRecordType" select ="$vRecordType"/>
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
                    <xsl:with-param name="pRecordType" select ="$vRecordType"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            
            <!-- FI [23732] 20180119 add sectionFees -->
            <xsl:if test ="$pDebugMode = 'SectionFees'">
              <SectionFees>
                <xsl:copy-of select ="$vXMLFees"/>
              </SectionFees>
            </xsl:if>
            
            <!-- RD 20130822 FeeCalculation Project / Gestion da la valeur Businessdate / Ajout parametre pBusinessDate-->
            <!-- FI 20171103 [23326] suppression des paramètres pTransactionDate et pBusinessDate -->
            <xsl:call-template name="CcisFees">
              <!--<xsl:with-param name="pTransactionDate" select="$vTransactionDate"/>
              <xsl:with-param name="pBusinessDate" select="$vBusinessDate"/>-->
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
  <!-- FI 20170824 [23339] Modify-->
  <xsl:template name="GetDefaultImportMode">
    <xsl:param name="pActionType"/>
    <xsl:param name="pRecordType"/>
    <xsl:choose>
      <xsl:when test="$pActionType='N' or $pActionType='G'">
        <xsl:value-of select="'New'"/>
      </xsl:when>
      <!-- FI 20160907 [21831] Add ActionType='F' -->
      <!-- FI 20170824 [23339] Mod ActionType='T' -->
      <xsl:when test="$pActionType='U' or $pActionType='M' or $pActionType='F' or $pActionType='T'">
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

  <!-- FI 20120704 add parameter pCciPrefixFamily and pFamily-->
  <!-- FI 20130703 [18798] add parameter pIsAllowDataMissing-->
  <!-- FI 20170404 [23039] add pRecordVersion -->
  <!-- FI 20170404 [23039] rem pRecordVersion -->
  <!-- FI 20170718 [23326] paramètre pLegRow à la place de pXMLLegRow -->
  <xsl:template name="CcisLeg">
    <xsl:param name ="pLegNumber"/>
    <xsl:param name ="pLegRow"/>
    <xsl:param name ="pCciPrefix"/>
    <xsl:param name ="pCciPrefixFamily"/>
    <xsl:param name ="pRecordType"/>
    <xsl:param name ="pMarket"/>
    <xsl:param name ="pMarketColumn"/>
    <xsl:param name ="pProduct"/>
    <xsl:param name ="pFamily"/>
    <xsl:param name ="pBusinessDate"/>
    <xsl:param name ="pIsAllowDataMissing"/>


    <xsl:variable name="vRow" select="$pLegRow"/>

    <xsl:variable name ="vRecordVersion1">
      <xsl:call-template name="GetRecordVersion">
        <xsl:with-param name="pRow" select="$vRow"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vLegPrefix" select ="concat($pCciPrefix,$pCciPrefixFamily,$pLegNumber)"/>

    <xsl:variable name ="vAssetCode" select ="normalize-space($vRow/data[@name='ASC'])"/>
    <xsl:variable name ="vAssetCodeIdent" select ="normalize-space($vRow/data[@name='ASCI'])"/>
    <xsl:variable name ="vAssetCodeColumn">
      <xsl:call-template name="GetAssetIdentColumn">
        <xsl:with-param name="pIdent" select="$vAssetCodeIdent"/>
      </xsl:call-template>
    </xsl:variable>

    <!-- FI 20170404 [23039] Modify Lecture des champs CTR, CTRI, CTRV, CTRC -->
    <xsl:variable name ="vContract" select ="normalize-space($vRow/data[@name='DVT' or @name='CTR'])"/>
    <xsl:variable name ="vContractColumn">
      <xsl:call-template name="GetContractIdentColumn">
        <xsl:with-param name="pIdent" select="normalize-space($vRow/data[@name='DVTI' or @name='CTRI'])"/>
        <xsl:with-param name="pFamily" select="$pFamily"/>
      </xsl:call-template>
    </xsl:variable>
    <!-- ETD  : (F)uture ou (O)ption
         COMS : (S)Spot ou (I)ntraday    -->
    <xsl:variable name ="vContractCategory" select ="normalize-space($vRow/data[@name='DVTC' or @name='CTRC'])"/>
    <!-- FI 20220823 [XXXXX] Call Template GetContractType -->
    <xsl:variable name="vContractType"> 
      <xsl:call-template name="GetContractType">
        <xsl:with-param name="pRow" select="$vRow" />
        <xsl:with-param name="pFamily" select ="$pFamily"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vContractVersion" select ="normalize-space($vRow/data[@name='DVTV' or @name='CTRV'])"/>
    <!-- FI 20220225 [25699] Add vContractSettltMethod and vContractExerciseStyle -->
    <xsl:variable name ="vContractSettltMethod" select ="normalize-space($vRow/data[@name='CTRSM'])"/>
    <xsl:variable name ="vContractExerciseStyle" select ="normalize-space($vRow/data[@name='CTRES'])"/>
    
    <!-- FI 20220628 [25699] add vContractPutCall-->
   <xsl:variable name="vContractPutCall">
     <xsl:choose>
       <xsl:when test ="$vRecordVersion1='v3'">
         <xsl:value-of select ="normalize-space($vRow/data[@name='CTRPC'])"/>
       </xsl:when>
       <xsl:otherwise>
         <xsl:value-of select ="normalize-space($vRow/data[@name='DVTO' or @name='CTRT'])"/>
       </xsl:otherwise>
     </xsl:choose>
    </xsl:variable>

    
    <!-- FI 20220214 [25699] vContractKind replace vContractType --> 
    <!-- FI 20220628 [25699] vContractKind removed -->
    <!--<xsl:variable name ="vContractKind">
      <xsl:choose>
        <xsl:when test ="$vRecordVersion1='v1'">
          <xsl:value-of select ="normalize-space($vRow/data[@name='DVTO'])"/>
        </xsl:when>
        <xsl:when test ="$vRecordVersion1='v2'">
          <xsl:value-of select ="normalize-space($vRow/data[@name='CTRT'])"/>
        </xsl:when>
        <xsl:when test ="$vRecordVersion1='v3'">
          <xsl:value-of select ="normalize-space($vRow/data[@name='CTRK'])"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>-->
    <!-- FI 20220214 [25699] vContractType available only on v3 Record -->
    <!--<xsl:variable name="vContractType">
      <xsl:choose>
        <xsl:when test ="$vRecordVersion1='v3'">
          <xsl:value-of select ="normalize-space($vRow/data[@name='CTRT'])"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>-->
    
    

    <!-- vClearingHouse contient la valeur qui identifie la chmabre de compensation -->
    <xsl:variable name ="vClearingHouse" select ="normalize-space($vRow/data[@name='CLH'])"/>
    <!-- vClearingHouseColumn contient la colonne CLEARINGHOUSE en fonction du type d'identification-->
    <xsl:variable name ="vClearingHouseColumn">
      <xsl:call-template name="GetActorIdentColumn">
        <xsl:with-param name="pIdent" select="normalize-space($vRow/data[@name='CLHI'])"/>
      </xsl:call-template>
    </xsl:variable>
    
    <xsl:variable name="vPhysicalLegPrefix">
      <xsl:if test= "$pFamily='COMS'">
        <xsl:value-of select ="'_{commodityPhysicalLeg}'"/>
      </xsl:if>
    </xsl:variable>

    <!-- Asset Cci-->
    <xsl:variable name="vClientIdAsset">
      <xsl:choose >
        <xsl:when test= "$pFamily='ETD' or $pFamily='ESE'">
          <xsl:value-of select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_Instrmt_ID')"/>
        </xsl:when>
        <xsl:when test= "$pFamily='COMS'">
          <xsl:value-of select ="concat($vLegPrefix, $vPhysicalLegPrefix,'_commodityAsset')"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name ="vAssetIsMandatory">
      <xsl:choose>
        <!-- FI 20170718 [23326] vAssetIsMandatory => true si COMS -->
        <xsl:when test ="$pFamily='ESE' or $pFamily='COMS'">
          <xsl:value-of select ="$ConstTrue"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$ConstFalse"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:if test="string-length($vAssetCode)>0 or ($vAssetIsMandatory = $ConstTrue)">
      <xsl:variable name="vDynamicDataAssetIdentifier">
        <xsl:call-template name="GetDynamicDataAssetIdentifier">
          <xsl:with-param name ="pFamily" select="$pFamily"/>
          <xsl:with-param name ="pBusinessDate" select="$pBusinessDate"/>
          <xsl:with-param name ="pMarket" select="$pMarket"/>
          <xsl:with-param name ="pMarketColumn" select="$pMarketColumn"/>
          <xsl:with-param name ="pAssetCode" select="$vAssetCode"/>
          <xsl:with-param name ="pAssetColumn" select="$vAssetCodeColumn"/>
          <xsl:with-param name ="pContract" select ="$vContract"/>
          <xsl:with-param name ="pContractColumn" select="$vContractColumn"/>
          <xsl:with-param name ="pContractVersion" select="$vContractVersion"/>
          <xsl:with-param name ="pContractCategory" select="$vContractCategory"/>
          <xsl:with-param name ="pContractType" select="$vContractType"/>
          <xsl:with-param name ="pClearingHouse" select="$vClearingHouse"/>
          <xsl:with-param name ="pClearingHouseColumn" select="$vClearingHouseColumn"/>
          <xsl:with-param name ="pIsMandatory" select ="$vAssetIsMandatory" />
        </xsl:call-template>
      </xsl:variable>

      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="$vClientIdAsset"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <!-- FI 20170718 [23326] utilisation du paramètre pDynamicValue -->
        <!--<xsl:with-param name="pXMLDynamicValue">
          <xsl:copy-of select ="msxsl:node-set($vDynamicDataAssetIdentifier)/dynamicValue"/>
        </xsl:with-param>-->
        <xsl:with-param name="pDynamicValue" select ="msxsl:node-set($vDynamicDataAssetIdentifier)/dynamicValue"/>
        <xsl:with-param name="pIsMissingMode" select ="$pIsAllowDataMissing"/>
        <xsl:with-param name="pIsMissingModeSpecified" select ="$ConstTrue"/>
      </xsl:call-template>
    </xsl:if>


    <!-- Si l'Asset est renseigné, on ne renseigne pas les cci : Contract, Maturity, PutCall et Strike-->
    <!-- FI 20120704 [17991] only on ETD -->
    <xsl:if test="string-length($vAssetCode)=0 and $pFamily='ETD' ">
      <xsl:variable name ="vContractMaturity" select ="normalize-space($vRow/data[@name='DVTM' or @name='CTRM'])"/>
      <xsl:variable name ="vContractStrike" select ="normalize-space($vRow/data[@name='DVTS' or @name='CTRS'])"/>
      <!-- Contract Cci-->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_Instrmt_Sym')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pXMLDynamicValue">
          <xsl:call-template name="SQLDerivativeContract">
            <xsl:with-param name="pBusinessDate" select="$pBusinessDate"/>
            <xsl:with-param name="pDerivativeContract" select="$vContract"/>
            <xsl:with-param name="pDerivativeContractColumn" select="$vContractColumn"/>
            <xsl:with-param name="pDerivativeContractVersion" select="$vContractVersion"/>
            <xsl:with-param name="pDerivativeContractCategory" select="$vContractCategory"/>
            <xsl:with-param name="pDerivativeContractType" select="$vContractType"/>
            <xsl:with-param name="pDerivativeContractSettltMethod" select="$vContractSettltMethod"/>
            <xsl:with-param name="pDerivativeContractExerciseStyle" select="$vContractExerciseStyle"/>
            <xsl:with-param name="pMarket" select="$pMarket"/>
            <xsl:with-param name="pMarketColumn" select="$pMarketColumn"/>
            <xsl:with-param name="pClearingHouse" select="$vClearingHouse"/>
            <xsl:with-param name="pClearingHouseColumn" select="$vClearingHouseColumn"/>
          </xsl:call-template>
        </xsl:with-param>
        <!--FI 20130702 [18798] add pIsMandatory à true de manière à intégrer les trades lorsque le DC est non renseigné
        Si la donnée est vide => cela génère un message d'erreur dans le cci => le trade passe en mode Incomplet
        Ce comportement s'applique si $pIsAllowDataMissing
        -->
        <xsl:with-param name="pIsMandatory" select ="$ConstTrue"/>
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
        <xsl:with-param name="pClientId" select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_Instrmt_MMY')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pValue">
          <xsl:choose>
            <xsl:when test ="contains($vContractMaturity,'@')">
              <xsl:value-of select="substring-before($vContractMaturity,'@')"/>
            </xsl:when>
            <!-- FI 20240417 [26751] Next Month sur certains contrat Eurex lorsque échéance au format YYYYMM (CODAGE EN DUR POUR HPC|MAREX] -->
            <xsl:when test ="($pMarket = 'EUREX' and $pMarketColumn='SHORTIDENTIFIER')  
                      and (string-length($vContractMaturity)=6) 
                      and ($vContractColumn='CONTRACTSYMBOL') 
                      and ($vContract='OGBL' or $vContract='OGBM' or $vContract='OGBS' or $vContract='OGBX')">
              <xsl:call-template name ="NextMonth" >
                <xsl:with-param name="pYYYYMM" select="$vContractMaturity"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$vContractMaturity"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:with-param>
      </xsl:call-template>
      <!-- MaturityDate Cci-->
      <!-- FI [19271] add cci MatDt -->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_Instrmt_MatDt')"/>
        <xsl:with-param name="pDataType" select ="'date'"/>
        <xsl:with-param name="pValue">
          <xsl:choose>
            <xsl:when test ="contains($vContractMaturity,'@')">
              <xsl:value-of select="substring-after($vContractMaturity,'@')"/>
            </xsl:when>
          </xsl:choose>
        </xsl:with-param>
      </xsl:call-template>
      <!-- For Option only-->
      <!-- FI 20170404 [23039] Remarque $vContractCategory n'est pas renseigné si l'asset est renseigné -->
      <xsl:if test="$vContractCategory = 'O'">
        <!-- Type Cci-->
        <xsl:call-template name="CustomCaptureInfo">
          <xsl:with-param name="pClientId" select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_Instrmt_PutCall')"/>
          <xsl:with-param name="pDataType" select ="'string'"/>
          <xsl:with-param name="pXMLDynamicValue">
            <xsl:call-template name="SQLEnumValue2">
              <xsl:with-param name="pCode" select="'PutOrCallEnum'"/>
              <xsl:with-param name="pExtValue" select="$vContractPutCall"/>
            </xsl:call-template>
          </xsl:with-param>
        </xsl:call-template>
        <!-- Strike Cci-->
        <xsl:call-template name="CustomCaptureInfo">
          <xsl:with-param name="pClientId" select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_Instrmt_StrkPx')"/>
          <xsl:with-param name="pDataType" select ="'string'"/>
          <xsl:with-param name="pValue" select="$vContractStrike"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:if>
    <!-- Qty Cci-->
    <xsl:variable name="vClientIdQty">
      <xsl:choose >
        <xsl:when test= "$pFamily='ETD' or $pFamily='ESE'">
          <xsl:value-of select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_LastQty')"/>
        </xsl:when>
        <xsl:when test= "$pFamily='COMS'">
          <xsl:value-of select ="concat($vLegPrefix, $vPhysicalLegPrefix,'_deliveryQuantity_totalPhysicalQuantity_quantity')"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vDataTypeQty">
      <xsl:choose >
        <xsl:when test= "$pFamily='ETD' or $pFamily='ESE'">
          <xsl:value-of select ="'integer'"/>
        </xsl:when>
        <xsl:when test= "$pFamily='COMS'">
          <xsl:value-of select ="'decimal'"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <xsl:call-template name="CustomCaptureInfo">
      <xsl:with-param name="pClientId" select ="$vClientIdQty"/>
      <xsl:with-param name="pDataType" select ="$vDataTypeQty"/>
      <xsl:with-param name="pRegex" select ="'RegexPositiveInteger'"/>
      <xsl:with-param name="pValue">
        <xsl:choose>
          <xsl:when test="$pRecordType = 'A'">
            <!--AllocationQuantity-->
            <xsl:value-of select="normalize-space($vRow/data[@name='ALQ'])"/>
          </xsl:when>
          <xsl:otherwise>
            <!--LegQuantity-->
            <xsl:value-of select="normalize-space($vRow/data[@name='LQTY'])"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:with-param>
      <xsl:with-param name="pIsMandatory" select ="$ConstTrue"/>
    </xsl:call-template>
    <!-- Price Cci-->
    <xsl:variable name="vClientIdPrice">
      <xsl:choose >
        <xsl:when test= "$pFamily='ETD' or $pFamily='ESE'">
          <xsl:value-of select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_LastPx')"/>
        </xsl:when>
        <xsl:when test= "$pFamily='COMS'">
          <xsl:value-of select ="concat($vLegPrefix,'_fixedLeg_fixedPrice_price')"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:call-template name="CustomCaptureInfo">
      <xsl:with-param name="pClientId" select= "$vClientIdPrice"/>
      <xsl:with-param name="pDataType" select= "'decimal'"/>
      <xsl:with-param name="pValue">
        <!-- FI 20170404 [23039] Price -->
        <!-- FI 20170428 [23039] lecture de la donnée PRI (compatibilité avec l'ancien standard)  -->
        <xsl:choose>
          <xsl:when test="$pRecordType = 'A'">
            <!-- AllocationPrice -->
            <xsl:value-of select="normalize-space($vRow/data[@name='ALP' or @name='PRI'])"/>
          </xsl:when>
          <xsl:otherwise>
            <!-- LegPrice -->
            <xsl:value-of select="normalize-space($vRow/data[@name='LPRI' or @name='PRI'])"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:with-param>
      <xsl:with-param name="pIsMandatory" select ="$ConstTrue"/>
    </xsl:call-template>
    <!-- PosEfct For Allocation only-->
    <xsl:if test="$pRecordType = 'A' and ($pFamily='ETD' or $pFamily='ESE')">
      <!--RD 20150610 [21099] Alimenter le Cci FIXML_TrdCapRpt_RptSide_PosEfct uniquement si la donnée ALPE existe dans le fichier d'entrée-->
      <xsl:variable name="posEfct" select ="normalize-space($vRow/data[@name='ALPE'])"/>
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
            <xsl:call-template name="SQLEnumValue2">
              <xsl:with-param name="pCode" select="'PositionEffectEnum'"/>
              <xsl:with-param name="pExtValue" select="$posEfct"/>
            </xsl:call-template>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:if>
    </xsl:if>
    <!-- FI 20170404 [23039] Add COMS ccis -->
    <xsl:if test ="$pFamily = 'COMS'">
      <!-- Livraison Debut -->
      <!--commoditySpot_effectiveDate_adjustableDate_unadjustedDate -->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat($vLegPrefix,'_effectiveDate_adjustableDate_unadjustedDate')"/>
        <xsl:with-param name="pDataType" select ="'date'"/>
        <xsl:with-param name="pValue" select="substring-before(normalize-space($vRow/data[@name='CTRTS']),'T')"/>
        <xsl:with-param name="pIsMandatory" select ="$ConstTrue"/>
      </xsl:call-template>
      <!-- commoditySpot_electricityPhysicalLeg_settlementPeriods_startTime_hourMinuteTime 
           commoditySpot_gasPhysicalLeg_deliveryPeriods_supplyStartTime_hourMinuteTime -->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat($vLegPrefix, $vPhysicalLegPrefix,'_{settlementPeriods_startTime}_hourMinuteTime')"/>
        <xsl:with-param name="pDataType" select ="'time'"/>
        <xsl:with-param name="pRegEx" select ="'RegexLongTime'"/>
        <xsl:with-param name="pValue" select="substring-after(normalize-space($vRow/data[@name='CTRTS']),'T')"/>
        <xsl:with-param name="pIsMandatory" select ="$ConstTrue"/>
      </xsl:call-template>
      <!-- Fuseau horaire -->
      <!-- commoditySpot_electricityPhysicalLeg_settlementPeriods_startTime_location 
           commoditySpot_gasPhysicalLeg_deliveryPeriods_supplyStartTime_location -->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat($vLegPrefix, $vPhysicalLegPrefix,'_{settlementPeriods_startTime}_location')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pValue" select ="normalize-space($vRow/data[@name='CTRTZ'])"/>
        <xsl:with-param name="pIsMandatory" select ="$ConstTrue"/>
      </xsl:call-template>
      <!-- Livraison Fin -->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat($vLegPrefix,'_terminationDate_adjustableDate_unadjustedDate')"/>
        <xsl:with-param name="pDataType" select ="'date'"/>
        <xsl:with-param name="pValue" select="substring-before(normalize-space($vRow/data[@name='CTRTE']),'T')"/>
        <xsl:with-param name="pIsMandatory" select ="$ConstTrue"/>
      </xsl:call-template>
      <!-- commoditySpot_electricityPhysicalLeg_settlementPeriods_endTime_hourMinuteTime
           commoditySpot_gasPhysicalLeg_deliveryPeriods_supplyEndTime_hourMinuteTime -->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat($vLegPrefix, $vPhysicalLegPrefix,'_{settlementPeriods_endTime}_hourMinuteTime')"/>
        <xsl:with-param name="pDataType" select ="'time'"/>
        <xsl:with-param name="pRegEx" select ="'RegexLongTime'"/>
        <xsl:with-param name="pValue" select="substring-after(normalize-space($vRow/data[@name='CTRTE']),'T')"/>
        <xsl:with-param name="pIsMandatory" select ="$ConstTrue"/>
      </xsl:call-template>
    </xsl:if>
    <!-- FI 20170404 [23039] Add GrossAmount ccis -->
    <xsl:if test="($pFamily='ESE' or $pFamily='COMS')">
      <xsl:variable name ="vGrossAmountPrefix">
        <xsl:choose >
          <xsl:when test= "$pFamily='ESE'">
            <xsl:value-of select ="concat($vLegPrefix,'_grossAmount')"/>
          </xsl:when>
          <xsl:when test= "$pFamily='COMS'">
            <xsl:value-of select ="concat($vLegPrefix, '_fixedLeg_grossAmount')"/>
          </xsl:when>
        </xsl:choose>
      </xsl:variable>
      <!-- Date Cci-->
      <xsl:if test ="string-length(normalize-space($vRow/data[@name='STLD']))>0">
        <xsl:call-template name="CustomCaptureInfo">
          <xsl:with-param name="pClientId" select ="concat($vGrossAmountPrefix,'_paymentDate_unadjustedDate')"/>
          <xsl:with-param name="pDataType" select ="'date'"/>
          <xsl:with-param name="pValue" select="normalize-space($vRow/data[@name='STLD'])"/>
        </xsl:call-template>
      </xsl:if>
      <!-- Amount Cci-->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat($vGrossAmountPrefix,'_paymentAmount_amount')"/>
        <xsl:with-param name="pDataType" select ="'decimal'"/>
        <xsl:with-param name="pValue" select="normalize-space($vRow/data[@name='STLA'])"/>
      </xsl:call-template>
      <!-- Currency Cci-->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat($vGrossAmountPrefix,'_paymentAmount_currency')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pValue" select="normalize-space($vRow/data[@name='STLC'])"/>
      </xsl:call-template>
      <!-- SettlementInformation Cci-->
      <xsl:variable name="vStlInformation">
        <xsl:choose>
          <xsl:when test="contains(normalize-space($vRow/data[@name='STLI']),'-stlinf')">
            <xsl:value-of select="normalize-space(substring-before(substring-after(normalize-space($vRow/data[@name='STLI']),'-stlinf'),'-'))"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="normalize-space($vRow/data[@name='STLI'])"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat($vGrossAmountPrefix,'_settlementInformation')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pValue" select="$vStlInformation"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <!-- FI 20180214 [23774] Add template CcisElectronicTradeElectronicOrder -->
  <xsl:template name ="CcisElectronicTradeElectronicOrder">
    <xsl:param name ="pLegRow"/>
    <xsl:param name ="pCciPrefix"/>
    <xsl:param name ="pCciPrefixFamily"/>
    <xsl:param name ="pFamily"/>

    <xsl:variable name ="vLegPrefix" select ="concat($pCciPrefix,$pCciPrefixFamily)"/>
    <xsl:variable name="vRow" select="$pLegRow"/>

    <!-- ExecutionId Cci-->
    <!-- FI 20170404 [23039] Add vClientIdExecId -->
    <xsl:variable name ="vClientIdExecId">
      <xsl:choose >
        <xsl:when test ="$pFamily='ETD' or $pFamily='ESE'">
          <xsl:value-of select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_ExecID')"/>
        </xsl:when>
        <xsl:when test ="$pFamily='COMS'">
          <xsl:value-of select ="concat($vLegPrefix,'_RptSide_ExecRefID')"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:call-template name="CustomCaptureInfo">
      <xsl:with-param name="pClientId" select ="$vClientIdExecId"/>
      <xsl:with-param name="pDataType" select ="'string'"/>
      <xsl:with-param name="pValue" select="normalize-space($vRow/data[@name='EXID'])"/>
    </xsl:call-template>
    <!-- ExecType Cci-->
    <!-- FI 20170404 [23039] Add vClientIdExecType -->
    <xsl:variable name ="vClientIdExecType">
      <xsl:choose >
        <xsl:when test ="$pFamily='ETD' or $pFamily='ESE'">
          <xsl:value-of select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_ExecType')"/>
        </xsl:when>
        <xsl:when test ="$pFamily='COMS'">
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:if test ="string-length($vClientIdExecType) > 0">
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="$vClientIdExecType"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pXMLDynamicValue">
          <xsl:call-template name="SQLEnumValue2">
            <xsl:with-param name="pCode" select="'ExecTypeEnum'"/>
            <xsl:with-param name="pExtValue" select="normalize-space($vRow/data[@name='EXT'])"/>
          </xsl:call-template>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:if>
    <!-- TrdType Cci-->
    <!-- FI 20170404 [23039] Add vClientIdTrdTyp -->
    <xsl:variable name ="vClientIdTrdTyp">
      <xsl:choose >
        <xsl:when test ="$pFamily='ETD' or $pFamily='ESE'">
          <xsl:value-of select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_TrdTyp')"/>
        </xsl:when>
        <xsl:when test ="$pFamily='COMS'">
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:if test ="string-length($vClientIdTrdTyp) > 0">
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="$vClientIdTrdTyp"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pXMLDynamicValue">
          <xsl:call-template name="SQLEnumValue2">
            <xsl:with-param name="pCode" select="'TrdTypeEnum'"/>
            <xsl:with-param name="pExtValue" select="normalize-space($vRow/data[@name='ALT'])"/>
          </xsl:call-template>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:if>
    <!-- TrdSubType Cci-->
    <!-- FI 20170404 [23039] Add vClientIdTrdSubTyp -->
    <xsl:variable name ="vClientIdTrdSubTyp">
      <xsl:choose >
        <xsl:when test ="$pFamily='ETD' or $pFamily='ESE'">
          <xsl:value-of select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_TrdSubTyp')"/>
        </xsl:when>
        <xsl:when test ="$pFamily='COMS'">
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:if test ="string-length($vClientIdTrdSubTyp) > 0">
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId"  select ="$vClientIdTrdSubTyp"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pXMLDynamicValue">
          <xsl:call-template name="SQLEnumValue2">
            <xsl:with-param name="pCode" select="'TrdSubTypeEnum'"/>
            <xsl:with-param name="pExtValue" select="normalize-space($vRow/data[@name='ALST'])"/>
          </xsl:call-template>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:if>
    <!-- TradeInputSource Cci-->
    <!-- FI 20170404 [23039] Add vClientIdInptSrc -->
    <xsl:variable name ="vClientIdInptSrc">
      <xsl:choose >
        <xsl:when test ="$pFamily='ETD' or $pFamily='ESE'">
          <xsl:value-of select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_RptSide_InptSrc')"/>
        </xsl:when>
        <xsl:when test ="$pFamily='COMS'">
          <xsl:value-of select ="concat($vLegPrefix,'_RptSide_InptSrc')"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:call-template name="CustomCaptureInfo">
      <xsl:with-param name="pClientId" select ="$vClientIdInptSrc"/>
      <xsl:with-param name="pDataType" select ="'string'"/>
      <xsl:with-param name="pValue" select="normalize-space($vRow/data[@name='ALSRC'])"/>
    </xsl:call-template>
    <!-- TrdTyp2 Cci-->
    <!-- FI 20170404 [23039] Add vClientIdTrdTyp2 -->
    <xsl:variable name ="vClientIdTrdTyp2">
      <xsl:choose >
        <xsl:when test ="$pFamily='ETD' or $pFamily='ESE'">
          <xsl:value-of select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_TrdTyp2')"/>
        </xsl:when>
        <xsl:when test ="$pFamily='COMS'">
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:if test ="string-length($vClientIdTrdTyp2) > 0">
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="$vClientIdTrdTyp2"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pXMLDynamicValue">
          <xsl:call-template name="SQLEnumValue2">
            <xsl:with-param name="pCode" select="'SecondaryTrdTypeEnum'"/>
            <xsl:with-param name="pExtValue" select="normalize-space($vRow/data[@name='ALT2'])"/>
          </xsl:call-template>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:if>
    <!-- AllocationID Cci-->
    <!-- FI 20240229 [WI859] ALID used now for tradeHeader_market1_trdId -->
    <!-- FI 20170404 [23039] Add vClientIdTrdID -->
    <!--<xsl:variable name ="vClientIdTrdID">
      <xsl:choose >
        <xsl:when test ="$pFamily='ETD' or $pFamily='ESE'">
          <xsl:value-of select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_TrdID')"/>
        </xsl:when>
        <xsl:when test ="$pFamily='COMS'">
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:if test ="string-length($vClientIdTrdID) > 0">
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="$vClientIdTrdID"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pValue" select="normalize-space($vRow/data[@name='ALID'])"/>
      </xsl:call-template>
    </xsl:if>-->

    <xsl:call-template name="CustomCaptureInfo">
      <xsl:with-param name="pClientId" select ="'tradeHeader_market1_trdId'"/>
      <xsl:with-param name="pDataType" select ="'string'"/>
      <xsl:with-param name="pValue" select="normalize-space($vRow/data[@name='ALID'])"/>
    </xsl:call-template>

    <!-- AllocationInputDevice Cci-->
    <!-- FI 20170404 [23039] Add vClientIdInptDev -->
    <xsl:variable name ="vClientIdInptDev">
      <xsl:choose >
        <xsl:when test ="$pFamily='ETD' or $pFamily='ESE'">
          <xsl:value-of select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_RptSide_InptDev')"/>
        </xsl:when>
        <xsl:when test ="$pFamily='COMS'">
          <xsl:value-of select ="concat($vLegPrefix,'_RptSide_InptDev')"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:call-template name="CustomCaptureInfo">
      <xsl:with-param name="pClientId" select ="$vClientIdInptDev"/>
      <xsl:with-param name="pDataType" select ="'string'"/>
      <xsl:with-param name="pValue" select="normalize-space($vRow/data[@name='ALD'])"/>
    </xsl:call-template>
    <!-- RelatedPositionID Cci-->
    <!-- FI 20170404 [23039] Add vClientIdReltdPosID -->
    <xsl:variable name ="vClientIdReltdPosID">
      <xsl:choose >
        <xsl:when test ="$pFamily='ETD' or $pFamily='ESE'">
          <xsl:value-of select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_RptSide_ReltdPosID')"/>
        </xsl:when>
        <xsl:when test ="$pFamily='COMS'">
          <xsl:value-of select ="concat($vLegPrefix,'_RptSide_ReltdPosID')"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:call-template name="CustomCaptureInfo">
      <xsl:with-param name="pClientId" select ="$vClientIdReltdPosID"/>
      <xsl:with-param name="pDataType" select ="'string'"/>
      <xsl:with-param name="pValue" select="normalize-space($vRow/data[@name='RPID'])"/>
    </xsl:call-template>

    <!-- ///-->
    <!-- OrderElectId Cci-->
    <!-- FI 20170404 [23039] Add vClientIdOrdID -->
    <xsl:variable name ="vClientIdOrdID">
      <xsl:choose >
        <xsl:when test ="$pFamily='ETD' or $pFamily='ESE'">
          <xsl:value-of select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_RptSide_OrdID')"/>
        </xsl:when>
        <xsl:when test ="$pFamily='COMS'">
          <xsl:value-of select ="concat($vLegPrefix,'_RptSide_OrdID')"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <!-- 20240229 [WI860] ConvertToLongFromBase64String from gateway BCS -->
    <xsl:choose>
      <xsl:when test="$gIsModegateway=$ConstTrue and $vRow/data[@name='FAC']='XDMI'">
        <xsl:call-template name="CustomCaptureInfo">
          <xsl:with-param name="pClientId" select ="$vClientIdOrdID"/>
          <xsl:with-param name="pDataType" select ="'string'"/>
          <xsl:with-param name="pXMLDynamicValue">
          <dynamicValue>
            <SpheresLib function="ConvertToLongFromBase64String()">
              <Param name="BASE64STRING" datatype="string">
                <xsl:value-of select="normalize-space($vRow/data[@name='OEID'])"/>
              </Param>
            </SpheresLib>
          </dynamicValue>
        </xsl:with-param>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="CustomCaptureInfo">
          <xsl:with-param name="pClientId" select ="$vClientIdOrdID"/>
          <xsl:with-param name="pDataType" select ="'string'"/>
          <xsl:with-param name="pValue" select="normalize-space($vRow/data[@name='OEID'])"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
    
    <!-- OrderElectCategory Cci -->
    <!-- FI 20170404 [23039] Add vClientIdOrdCat -->
    <xsl:variable name ="vClientIdOrdCat">
      <xsl:choose >
        <xsl:when test ="$pFamily='ETD' or $pFamily='ESE'">
          <xsl:value-of select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_OrdCat')"/>
        </xsl:when>
        <xsl:when test ="$pFamily='COMS'">
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:if test ="string-length($vClientIdOrdCat) > 0">
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="$vClientIdOrdCat"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pXMLDynamicValue">
          <xsl:call-template name="SQLEnumValue2">
            <xsl:with-param name="pCode" select="'OrderCategoryEnum'"/>
            <xsl:with-param name="pExtValue" select="normalize-space($vRow/data[@name='OEC'])"/>
          </xsl:call-template>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:if>
    <!-- OrderElectType Cci-->
    <!-- FI 20170404 [23039] Add vClientIdOrdTyp -->
    <xsl:variable name ="vClientIdOrdTyp">
      <xsl:choose >
        <xsl:when test ="$pFamily='ETD' or $pFamily='ESE'">
          <xsl:value-of select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_RptSide_OrdTyp')"/>
        </xsl:when>
        <xsl:when test ="$pFamily='COMS'">
          <xsl:value-of select ="concat($vLegPrefix,'_RptSide_OrdTyp')"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:call-template name="CustomCaptureInfo">
      <xsl:with-param name="pClientId" select ="$vClientIdOrdTyp"/>
      <xsl:with-param name="pDataType" select ="'string'"/>
      <xsl:with-param name="pXMLDynamicValue">
        <xsl:call-template name="SQLEnumValue2">
          <xsl:with-param name="pCode" select="'OrdTypeEnum'"/>
          <xsl:with-param name="pExtValue" select="normalize-space($vRow/data[@name='OET'])"/>
        </xsl:call-template>
      </xsl:with-param>
    </xsl:call-template>
    <!-- OrderElectStatus Cci-->
    <!-- FI 20170404 [23039] Add vClientIdOrdStat -->
    <xsl:variable name ="vClientIdOrdStat">
      <xsl:choose >
        <xsl:when test ="$pFamily='ETD' or $pFamily='ESE'">
          <xsl:value-of select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_OrdStat')"/>
        </xsl:when>
        <xsl:when test ="$pFamily='COMS'">
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:if test ="string-length($vClientIdOrdStat) > 0">
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="$vClientIdOrdStat"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pXMLDynamicValue">
          <xsl:call-template name="SQLEnumValue2">
            <xsl:with-param name="pCode" select="'OrdStatusEnum'"/>
            <xsl:with-param name="pExtValue" select="normalize-space($vRow/data[@name='OES'])"/>
          </xsl:call-template>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:if>
    <!-- OrderElectInputDevice Cci-->
    <!-- FI 20170404 [23039] Add vClientIdOrdInptDev -->
    <xsl:variable name ="vClientIdOrdInptDev">
      <xsl:choose >
        <xsl:when test ="$pFamily='ETD' or $pFamily='ESE'">
          <xsl:value-of select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_RptSide_OrdInptDev')"/>
        </xsl:when>
        <xsl:when test ="$pFamily='COMS'">
          <xsl:value-of select ="concat($vLegPrefix,'_RptSide_OrdInptDev')"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:call-template name="CustomCaptureInfo">
      <xsl:with-param name="pClientId" select ="$vClientIdOrdInptDev"/>
      <xsl:with-param name="pDataType" select ="'string'"/>
      <xsl:with-param name="pValue" select="normalize-space($vRow/data[@name='OED'])"/>
    </xsl:call-template>
  </xsl:template>

  <!-- FI 20180214 [23774] Add template CcisAllocStrategy -->
  <xsl:template name ="CcisAllocStrategy">
    <xsl:param name ="pLegRow"/>
    <xsl:param name ="pCciPrefix"/>
    <xsl:param name ="pCciPrefixFamily"/>

    <xsl:variable name ="vLegPrefix" select ="concat($pCciPrefix,$pCciPrefixFamily)"/>
    <xsl:variable name="vRow" select="$pLegRow"/>
    
    <!-- StrategyType Cci-->
    <xsl:variable name ="vClientIdStrategyType" select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_TrdLeg_SecSubTyp')"/>
    <xsl:call-template name="CustomCaptureInfo">
      <xsl:with-param name="pClientId" select ="$vClientIdStrategyType"/>
      <xsl:with-param name="pDataType" select ="'string'"/>
      <xsl:with-param name="pXMLDynamicValue">
        <xsl:call-template name="SQLEnumValue2">
          <xsl:with-param name="pCode" select="'StrategyTypeScheme'"/>
          <xsl:with-param name="pExtValue" select="normalize-space($vRow/data[@name='STA'])"/>
        </xsl:call-template>
      </xsl:with-param>
    </xsl:call-template>

    <!-- Strategy Leg Num Cci-->
    <xsl:variable name ="vClientIdLegNum" select ="concat($vLegPrefix,'_FIXML_TrdCapRpt_TrdLeg_LegNo')"/>
    <xsl:call-template name="CustomCaptureInfo">
      <xsl:with-param name="pClientId" select ="$vClientIdLegNum"/>
      <xsl:with-param name="pDataType" select ="'integer'"/>
      <xsl:with-param name="pValue" select="normalize-space($vRow/data[@name='LSN'])"/>
    </xsl:call-template>
  </xsl:template>

  
  
  
  
  <!--RD 20130822 FeeCalculation Project / Gestion da la valeur Businessdate
    - Ajout du parametre pBusinessDate
    - Renommer le parametre pFeesDate en pTransactionDate-->
  <!-- FI 20171103 [23326] suppression des paramètres pTransactionDate pBusinessDate 
       Afin d'éviter des convertions d'horodatage d'un timezone à un autre (cas de la date d'exécution qui est à la source de date de transaction)
    -->
  <xsl:template name="CcisFees">
    <!--<xsl:param name ="pTransactionDate"/>
    <xsl:param name ="pBusinessDate"/>-->
    <xsl:param name ="pXMLFees"/>

    <xsl:for-each select="msxsl:node-set($pXMLFees)/fee">
      <xsl:variable name ="vFeePosition" select="position()"/>

      <!-- FeeSchedule Cci-->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('otherPartyPayment',$vFeePosition,'_paymentSource_feeSchedule')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
      </xsl:call-template>
      <!-- Payer Cci-->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('otherPartyPayment',$vFeePosition,'_payer')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pXMLDynamicValue">
          <xsl:copy-of select="payer/dynamicValue"/>
        </xsl:with-param>
        <xsl:with-param name="pIsMandatory" select="$ConstTrue"/>
      </xsl:call-template>
      <!-- Receiver Cci-->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('otherPartyPayment',$vFeePosition,'_receiver')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pXMLDynamicValue">
          <xsl:copy-of select="receiver/dynamicValue"/>
        </xsl:with-param>
        <xsl:with-param name="pIsMandatory" select="$ConstTrue"/>
      </xsl:call-template>
      <!-- Date Cci-->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('otherPartyPayment',$vFeePosition,'_paymentDate_unadjustedDate')"/>
        <xsl:with-param name="pDataType" select ="'date'"/>
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
              <xsl:value-of select="'TransactDate'"/>
            </xsl:when>
            <!--RD 20130822 FeeCalculation Project / Gestion da la valeur BusinessDate-->
            <xsl:when test="@relativeto='BusinessDate'">
              <xsl:value-of select="'BusinessDate'"/>
            </xsl:when>
            <!--RD 20130822 FeeCalculation Project / Gestion da la valeur NextBusinessDate-->
            <xsl:when test="@relativeto='NextBusinessDate'">
              <xsl:value-of select="'default'"/>
            </xsl:when>
            <xsl:otherwise>
              <!-- 20171103 [23326] par défaut default-->
              <xsl:value-of select="'default'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:with-param>
      </xsl:call-template>
      <!-- Ajustement Cci-->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('otherPartyPayment',$vFeePosition,'_paymentDate_dateAdjustments_bDC')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pValue"/>
      </xsl:call-template>
      <!-- Type Cci-->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('otherPartyPayment',$vFeePosition,'_paymentType')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pValue" select="normalize-space(@type)"/>
      </xsl:call-template>
      <!-- Amount Cci-->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('otherPartyPayment',$vFeePosition,'_paymentAmount_amount')"/>
        <xsl:with-param name="pDataType" select ="'decimal'"/>
        <xsl:with-param name="pValue" select="normalize-space(@amount)"/>
      </xsl:call-template>
      <!-- Currency Cci-->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('otherPartyPayment',$vFeePosition,'_paymentAmount_currency')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pValue" select="normalize-space(@currency)"/>
      </xsl:call-template>
      <!-- SettlementInformation Cci-->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('otherPartyPayment',$vFeePosition,'_settlementInformation')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pValue" select="normalize-space(@information)"/>
      </xsl:call-template>
      <!-- FeeInvoicing Cci-->
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('otherPartyPayment',$vFeePosition,'_paymentSource_feeInvoicing')"/>
        <xsl:with-param name="pDataType" select ="'bool'"/>
        <xsl:with-param name="pValue" select="normalize-space(@invoicing)"/>
      </xsl:call-template>
    </xsl:for-each>
  </xsl:template>

  <!-- FI 20170404 [23039] 
    add parameter pInstr et pInstrColumn 
    rem parameter pInstrumentIdentifier
    -->
  <xsl:template name="CcisExtends">
    <xsl:param name="pXMLExtends"/>
    <xsl:param name="pInstr"/>
    <xsl:param name="pInstrColumn"/>
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
            <!-- FI 20170404 [23039] parameters pInstr et pInstrColumn -->
            <xsl:call-template name="SQLExtendAttribute">
              <xsl:with-param name="pExtIdentifier" select="@identifier"/>
              <xsl:with-param name="pExtDet" select="@detail"/>
              <xsl:with-param name="pResultColumn" select="'CLIENTID'"/>
              <xsl:with-param name="pInstr" select="$pInstr"/>
              <xsl:with-param name="pInstrColumn" select="$pInstrColumn"/>
              <xsl:with-param name="pAssetCode" select="$pAssetCode"/>
              <xsl:with-param name="pAssetColumn" select="$pAssetColumn"/>
              <xsl:with-param name="pMarket" select="$pMarket"/>
              <xsl:with-param name="pMarketColumn" select="$pMarketColumn"/>
              <xsl:with-param name="pClearingBusinessDate" select="$pClearingBusinessDate"/>
            </xsl:call-template>
          </dynamicAttrib >
          <dynamicAttrib name="dataType">
            <!-- FI 20170404 [23039] parameters pInstr et pInstrColumn -->
            <xsl:call-template name="SQLExtendAttribute">
              <xsl:with-param name="pExtIdentifier" select="@identifier"/>
              <xsl:with-param name="pExtDet" select="@detail"/>
              <xsl:with-param name="pResultColumn" select="'DATATYPE'"/>
              <xsl:with-param name="pInstr" select="$pInstr"/>
              <xsl:with-param name="pInstrColumn" select="$pInstrColumn"/>
              <xsl:with-param name="pAssetCode" select="$pAssetCode"/>
              <xsl:with-param name="pAssetColumn" select="$pAssetColumn"/>
              <xsl:with-param name="pMarket" select="$pMarket"/>
              <xsl:with-param name="pMarketColumn" select="$pMarketColumn"/>
              <xsl:with-param name="pClearingBusinessDate" select="$pClearingBusinessDate"/>
            </xsl:call-template>
          </dynamicAttrib >
          <dynamicAttrib name="mandatory">
            <!-- FI 20170404 [23039] parameters pInstr et pInstrColumn -->
            <xsl:call-template name="SQLExtendAttribute">
              <xsl:with-param name="pExtIdentifier" select="@identifier"/>
              <xsl:with-param name="pExtDet" select="@detail"/>
              <xsl:with-param name="pResultColumn" select="'ISMANDATORY'"/>
              <xsl:with-param name="pInstr" select="$pInstr"/>
              <xsl:with-param name="pInstrColumn" select="$pInstrColumn"/>
              <xsl:with-param name="pAssetCode" select="$pAssetCode"/>
              <xsl:with-param name="pAssetColumn" select="$pAssetColumn"/>
              <xsl:with-param name="pMarket" select="$pMarket"/>
              <xsl:with-param name="pMarketColumn" select="$pMarketColumn"/>
              <xsl:with-param name="pClearingBusinessDate" select="$pClearingBusinessDate"/>
            </xsl:call-template>
          </dynamicAttrib >
          <dynamicAttrib name="regex">
            <!-- FI 20170404 [23039] parameters pInstr et pInstrColumn -->
            <xsl:call-template name="SQLExtendAttribute">
              <xsl:with-param name="pExtIdentifier" select="@identifier"/>
              <xsl:with-param name="pExtDet" select="@detail"/>
              <xsl:with-param name="pResultColumn" select="'REGULAREXPRESSION'"/>
              <xsl:with-param name="pInstr" select="$pInstr"/>
              <xsl:with-param name="pInstrColumn" select="$pInstrColumn"/>
              <xsl:with-param name="pAssetCode" select="$pAssetCode"/>
              <xsl:with-param name="pAssetColumn" select="$pAssetColumn"/>
              <xsl:with-param name="pMarket" select="$pMarket"/>
              <xsl:with-param name="pMarketColumn" select="$pMarketColumn"/>
              <xsl:with-param name="pClearingBusinessDate" select="$pClearingBusinessDate"/>
            </xsl:call-template>
          </dynamicAttrib >
          <dynamicAttrib name="defaultValue">
            <!-- FI 20170404 [23039] parameters pInstr et pInstrColumn -->
            <xsl:call-template name="SQLExtendAttribute">
              <xsl:with-param name="pExtIdentifier" select="@identifier"/>
              <xsl:with-param name="pExtDet" select="@detail"/>
              <xsl:with-param name="pResultColumn" select="'DEFAULTVALUE'"/>
              <xsl:with-param name="pInstr" select="$pInstr"/>
              <xsl:with-param name="pInstrColumn" select="$pInstrColumn"/>
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

  <!-- FI 20170718 [23326] Refactoring -->
  <xsl:template name="XMLSale">
    <xsl:param name="pSale"/>
    <xsl:param name="pSaleIdent"/>
    <xsl:param name="pCoeff"/>

    <xsl:if test="$pSale">
      <sale ident="{$pSaleIdent}">
        <xsl:attribute name="coeff">
          <xsl:value-of select="number($pCoeff) div 100"/>
        </xsl:attribute>
        <xsl:value-of select="normalize-space($pSale)"/>
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
      <!-- FI 20180119 [23732] parsingValue[1] or parsingValue[2]-->
      <fee amount="{$pAmount}" currency="{$pCurrency}" type="{$pType}" information="{$vInformation}" relativeto="{$vRelativeTo}" invoicing="{$vInvoicing}">
        <payer>
          <xsl:copy-of select ="msxsl:node-set($pXMLTradePartiesAndBrokers)/party[parsingValue[1]=$pPayer or parsingValue[2]=$pPayer][1]/dynamicValue"/>
        </payer>
        <receiver>
          <xsl:copy-of select ="msxsl:node-set($pXMLTradePartiesAndBrokers)/party[parsingValue[1]=$pReceiver or parsingValue[2]=$pReceiver][1]/dynamicValue"/>
        </receiver>
      </fee>
    </xsl:if>
  </xsl:template>

  <xsl:template name="XMLFees">
    <xsl:param name ="pXMLRow"/>
    <xsl:param name ="pXMLTradePartiesAndBrokers"/>
    <xsl:param name ="pRecordType"/>

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

    <xsl:if test ="$pRecordType!='A'">
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

  <!-- FI 20170404 [23039] re-ecriture -->
  <xsl:template name="XMLExtends">
    <xsl:param name ="pXMLRow"/>

    <xsl:variable name="vRow" select="msxsl:node-set($pXMLRow)/row"/>

    <xsl:if test="$vRow/data[starts-with(@name,'XT')]">
      <xsl:call-template name="XMLExtend">
        <xsl:with-param name="pExtendIdentifier" select ="$vRow/data[@name='XTI1']"/>
        <xsl:with-param name="pExtendDetIdentifier" select ="$vRow/data[@name='XTDI1']"/>
        <xsl:with-param name="pExtendValue" select ="$vRow/data[@name='XTVL1']"/>
      </xsl:call-template>
      <xsl:call-template name="XMLExtend">
        <xsl:with-param name="pExtendIdentifier" select ="$vRow/data[@name='XTI2']"/>
        <xsl:with-param name="pExtendDetIdentifier" select ="$vRow/data[@name='XTDI2']"/>
        <xsl:with-param name="pExtendValue" select ="$vRow/data[@name='XTVL2']"/>
      </xsl:call-template>
      <xsl:call-template name="XMLExtend">
        <xsl:with-param name="pExtendIdentifier" select ="$vRow/data[@name='XTI3']"/>
        <xsl:with-param name="pExtendDetIdentifier" select ="$vRow/data[@name='XTDI3']"/>
        <xsl:with-param name="pExtendValue" select ="$vRow/data[@name='XTVL3']"/>
      </xsl:call-template>
      <xsl:call-template name="XMLExtend">
        <xsl:with-param name="pExtendIdentifier" select ="$vRow/data[@name='XTI4']"/>
        <xsl:with-param name="pExtendDetIdentifier" select ="$vRow/data[@name='XTDI4']"/>
        <xsl:with-param name="pExtendValue" select ="$vRow/data[@name='XTVL4']"/>
      </xsl:call-template>
      <xsl:call-template name="XMLExtend">
        <xsl:with-param name="pExtendIdentifier" select ="$vRow/data[@name='XTI5']"/>
        <xsl:with-param name="pExtendDetIdentifier" select ="$vRow/data[@name='XTDI5']"/>
        <xsl:with-param name="pExtendValue" select ="$vRow/data[@name='XTVL5']"/>
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

  <!-- FI 20170404 [23039] Add Template  -->
  <!-- FI 20190904 [24882] Appel à SQLActor2 -->
  <xsl:template name="SQLTrader">
    <xsl:param name="pValue"/>
    <xsl:param name="pValueIdent"/>
    <xsl:param name="pDynamicDataCssIdentifier"/>
    <xsl:param name="pDynamicDataMarketIdentifier"/>
    <xsl:param name="pClearingBusinessDate"/>
    <xsl:call-template name="SQLActor2">
          <xsl:with-param name="pValue" select="$pValue"/>
          <xsl:with-param name="pValueIdent" select="$pValueIdent"/>
      <xsl:with-param name="pDynamicDataCssIdentifier" select="$pDynamicDataCssIdentifier"/>
      <xsl:with-param name="pDynamicDataMarketIdentifier" select="$pDynamicDataMarketIdentifier"/>
      <xsl:with-param name="pClearingBusinessDate" select="$pClearingBusinessDate"/>
      <xsl:with-param name="pClearingMemberType" select="'MET'"/>
        </xsl:call-template>
  </xsl:template>


  <!-- BD 20120120 [18363] Gestion des Trades Modify et Update -->
  <!-- FI 20130521 [18672] Suppression du paramètre date de transaction -->
  <!-- FI 20140103 [19433] Modification de la requête associée à la colonne IDTGIVEUP (utilisation de la colonne FIXPARTYROLE) -->
  <!-- FI 20170404 [23039] Add pBusinessDate parameters -->
  <!-- FI 20170824 [23339] Modify -->
  <!-- RD 20171214 [23316] Modify -->
  <!-- PL 20180907 Action N: new query without cross join and without upper() -->
  <!-- PL 20180907 Set "cache" true with cacheinterval 3 sec. -->
  <!-- EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML) -->
  <xsl:template name="SQLCheckTrade">
    <xsl:param name="pActionType"/>
    <xsl:param name="pRecordType"/>
    <xsl:param name="pTradeId"/>
    <xsl:param name="pResultColumn"/>
    <xsl:param name="pBusinessDate"/>

    <xsl:variable name ="query">
      <xsl:choose>
        <!--
        Important: les éventuels Trades DEACTIV ne sont pas considérés.
        ISOK       => True  : le Trade n'existe pas ou il existe seul un Trade MISSING
                      False : dans les autres cas (ex. il existe un Trade REGULAR, il existe 2 Trades MISSING, ...) 
        IMPORTMODE => New   : le Trade n'existe pas 
                      Update: dans les autres cas (ex. le Trade existant est MISSING) 
        -->
        <xsl:when test="$pActionType='N'">
          <xsl:text>
          <![CDATA[/* Action N */
select case when (count(IDT) = 0) or ((count(IDT) = 1) and (count(case when IDSTACTIVATION='MISSING' then IDT else null end) = 1)) then 1 else 0 end as ISOK,
       case when (count(IDT) = 0) then 'New' else 'Update' end as IMPORTMODE
from (
    select t.IDT, t.IDSTACTIVATION
    from dbo.TRADE t
    where (t.EXTLLINK=@TRADEID) and (t.DTBUSINESS=@DT) and (t.IDSTACTIVATION!='DEACTIV')
     ) x]]> 
          </xsl:text>
        </xsl:when>

        <!--
        Important: les éventuels Trades DEACTIV ne sont pas considérés.
        ISOK       => True  : le Trade n'existe pas ou il existe seul un Trade
                      False : dans les autres cas (ex. il existe 2 Trades MISSING, ...) 
        IMPORTMODE => New   : le Trade n'existe pas 
                      Update: dans les autres cas (ex. le Trade existant est REGULAR ou MISSING ou LOCKED) 
        -->
        <xsl:when test="$pActionType='M'">
          <xsl:text>
          <![CDATA[/* Action M */
select case when (count(IDT) <= 1) then 1 else 0 end as ISOK,
       case when (count(IDT) = 0) then 'New' else 'Update' end as IMPORTMODE
from (
    select t.IDT
    from dbo.TRADE t
    where (t.EXTLLINK=@TRADEID) and (t.DTBUSINESS=@DT) and (t.IDSTACTIVATION!='DEACTIV')
     ) x]]> 
          </xsl:text>
        </xsl:when>

        <!--
        Important: les éventuels Trades DEACTIV ne sont pas considérés.
        ISOK       => True  : le Trade n'existe pas ou il existe seul un Trade et celui-ci n'est pas un Give-Up
                      False : dans les autres cas (ex. il existe 2 Trades MISSING, il existe 1 Trade mais c'est un Give-Up, ...) 
        IMPORTMODE => New   : le Trade n'existe pas 
                      Update: dans les autres cas (ex. le Trade existant est REGULAR ou MISSING ou LOCKED et celui-ci n'est pas un Give-Up) 
                      
        NB: count opéré dans le sous-select nécessaire à PL/SQL lors de l'utilisation avec un "not exist"
        -->
        <xsl:when test="$pActionType='G'">
          <xsl:text>
          <![CDATA[/* Action G */
select case when (CountIDT = 0) or ((CountIDT = 1) and not exists (
              select 1 
              from dbo.TRADE tgu
              inner join dbo.TRADEACTOR ta on ta.IDT=tgu.IDT and ta.FIXPARTYROLE='14'
              where (tgu.EXTLLINK=@TRADEID) and (tgu.DTBUSINESS=@DT) and (tgu.IDSTACTIVATION='REGULAR')
              )) 
            then 1 else 0 end as ISOK,
       case when (CountIDT = 0) then 'New' else 'Update' end as IMPORTMODE
from (
    select count(t.IDT) as CountIDT
    from dbo.TRADE t
    where (t.EXTLLINK=@TRADEID) and (t.DTBUSINESS=@DT) and (t.IDSTACTIVATION!='DEACTIV')
     ) x]]> 
          </xsl:text>
        </xsl:when>
        <!--
        FI 20170824 [23339] 
        Important: les éventuels Trades DEACTIV ne sont pas considérés.
        ISOK       => True  : il existe seul un Trade et celui-ci ne comporte aucun Trader
                      False : dans les autres cas (ex. il existe 2 Trades MISSING, , il existe 1 Trade mais il comporte un Trader, ...) 
        IMPORTMODE => "Update"
        
        NB: count opéré dans le sous-select nécessaire à PL/SQL lors de l'utilisation avec un "not exist"
        
        FI 20190115 [24432] Nouvelle écriture de la requête puisque les transfert peuvent être effectués une journée de bourse postérieure à la journée de bourse d'entrée du trade
        -->
        <xsl:when test="$pActionType='T'">
          <xsl:text>
          <![CDATA[/* Action T */
select case when (CountIDT = 1) and not exists (
              select 1 
              from dbo.TRADE tt
              inner join dbo.TRADEACTOR ta on ta.IDT=tt.IDT and ta.IDROLEACTOR='TRADER' and (ta.IDA_ACTOR=tt.IDA_DEALER or ta.IDA_ACTOR=tt.IDA_ENTITY)  
              where (tt.EXTLLINK=@TRADEID) and (tt.DTBUSINESS=x.DTBUSINESS) and (tt.IDSTACTIVATION='REGULAR')
              ) 
            then 1 else 0 end as ISOK,
       'Update' as IMPORTMODE
from (
    select count(t.IDT) as CountIDT, t.DTBUSINESS
    from dbo.TRADE t
    where (t.EXTLLINK=@TRADEID) and (t.IDSTACTIVATION!='DEACTIV') and 
    (t.DTBUSINESS in (select MAX(tmax.DTBUSINESS) from dbo.TRADE tmax where tmax.DTBUSINESS >=@DT and tmax.EXTLLINK=@TRADEID))
    group by t.DTBUSINESS
     ) x]]> 
          </xsl:text>
        </xsl:when>
        
        <!--
        Important: les éventuels Trades DEACTIV ne sont pas considérés.
        ISOK       => True  : il existe seul un Trade
                      False : dans les autres cas (ex. il existe 2 Trades MISSING, ...) 
        IMPORTMODE => "Update"
        -->
        <!-- FI 20160907 [21831] Add $pActionType='F'-->
        <xsl:when test="$pActionType='U' or $pActionType='F'">
          <xsl:text>
          <![CDATA[/* Action U/F */
select case when (count(IDT) = 1) then 1 else 0 end as ISOK,
       'Update' as IMPORTMODE
from (
    select t.IDT
    from dbo.TRADE t
    where (t.EXTLLINK=@TRADEID) and (t.DTBUSINESS=@DT) and (t.IDSTACTIVATION!='DEACTIV')
     ) x]]> 
          </xsl:text>
        </xsl:when>

        <!--
        Important: ATTENTION, les éventuels Trades DEACTIV sont CONSIDERES.
        ISOK       => True  : il existe seul un Trade non désactivé
                      False : dans les autres cas (ex. il existe un trade mais il est déjà désactivé, ...) 
        IMPORTMODE => RemoveOnly OU RemoveAllocation: géré via un SQL parameter
        -->
        <xsl:when test="$pActionType='S'">
          <xsl:text>
          <![CDATA[/* Action S */
select case when (count(IDT)=count(case when IDSTACTIVATION='DEACTIV' then IDT else null end) + 1) then 1 else 0 end as ISOK,
       @IMPORTMODE as IMPORTMODE
from (
    select t.IDT, t.IDSTACTIVATION
    from dbo.TRADE t
    where (t.EXTLLINK=@TRADEID) and (t.DTBUSINESS=@DT)
     ) x]]> 
          </xsl:text>
          <!-- RD 20171214 [23316] Comment-->
          <!--<Param name="IMPORTMODE" datatype="string">
            <xsl:call-template name="GetDefaultImportMode">
              <xsl:with-param name="pActionType" select="$pActionType"/>
              <xsl:with-param name="pRecordType" select="$pRecordType"/>
            </xsl:call-template>
          </Param>-->
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <!-- FI 20181221 [24408] cache => false  -->
    <SQL command="select" result="{$pResultColumn}" cache="false" cacheinterval="2">
      <xsl:value-of select="$query"/>
      <Param name="TRADEID" datatype="string" >
        <xsl:value-of select="$pTradeId"/>
      </Param>
      <Param name="DT" datatype="date">
        <xsl:value-of select="$pBusinessDate"/>
      </Param>
      <!-- RD 20171214 [23316] Add -->
      <xsl:if test="$pActionType='S'">
        <Param name="IMPORTMODE" datatype="string">
          <xsl:call-template name="GetDefaultImportMode">
            <xsl:with-param name="pActionType" select="$pActionType"/>
            <xsl:with-param name="pRecordType" select="$pRecordType"/>
          </xsl:call-template>
        </Param>
      </xsl:if>
    </SQL>
  </xsl:template>

  <!-- FI 20130301 [18465] Refactoring En mode création de trade il est possible de spécifier un identifier -->
  <!-- FI 20130521 [18672] suppression du paramètre date de transaction et recordType -->
  <!-- FI 20170404 [23039] pBusinessDate parameters -->
  <!-- EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML) -->
  <xsl:template name="SQLTradeIdentifier">
    <xsl:param name="pActionType"/>
    <xsl:param name="pTradeId"/>
    <xsl:param name="pResultColumn"/>
    <xsl:param name="pBusinessDate"/>

    <xsl:variable name ="query">
      <xsl:choose>
        <!-- 'N' peut donner lieu à une modification de trade MISSING => Recherche du trade MISSING -->
        <!-- 'N' peut donner lieu à mise en place d'un identifier fourni en entrée -->
        <xsl:when test="$pActionType='N'">
          <xsl:text>
            <![CDATA[
            select t.IDENTIFIER, 0 as SORT
            from dbo.TRADE t
            where upper(t.EXTLLINK)=upper(@TRADEID) and t.DTBUSINESS = @DT and (t.IDSTACTIVATION='MISSING')
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
        <!-- FI 20170824 [23339] Add $pActionType='T'-->
        <!-- FI 20190115 [24432] Rem $pActionType='T'-->
        <xsl:when test="$pActionType = 'M' or $pActionType = 'G' or $pActionType = 'U' or $pActionType='F' or $pActionType = 'S'">
          <xsl:text>
          <![CDATA[
            select t.IDENTIFIER
            from dbo.TRADE t
            where upper(t.EXTLLINK)=upper(@TRADEID) and t.DTBUSINESS = @DT
            order by case when t.IDSTACTIVATION='REGULAR' then 1 
                          when t.IDSTACTIVATION='MISSING' then 2  
                          else 3 end
          ]]>
      
          </xsl:text>
        </xsl:when>
        <!-- FI 20190115 [24432] Nouvelle écriture de la requête puisque les transfert peuvent être effectués une journée de bourse postérieure à la journée de bourse d'entrée du trade -->
        <xsl:when test="$pActionType = 'T'">
          <xsl:text>
          <![CDATA[
            select t.IDENTIFIER
            from dbo.TRADE t
            where upper(t.EXTLLINK)=upper(@TRADEID) and t.DTBUSINESS >= @DT
            order by  t.DTBUSINESS desc,  
                      case when t.IDSTACTIVATION='REGULAR' then 1 
                           when t.IDSTACTIVATION='MISSING' then 2  
                           else 3 end asc
          ]]>
          </xsl:text>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <SQL command="select" result="{$pResultColumn}">
      <xsl:value-of select="$query"/>
      <Param name="TRADEID" datatype="string" >
        <xsl:value-of select="$pTradeId"/>
      </Param>
      <Param name="DT" datatype="date" >
        <xsl:value-of select="$pBusinessDate"/>
      </Param>
    </SQL>
  </xsl:template>

  <!-- RD 20121204 [18240] Import: Trades with data for extensions -->
  <!-- FI 20170404 [23039] 
       Refactoring puisque pClearingBusinessDate n'est pas forcément renseigné (COMS Family)
       add parameter pInstr et pInstrColumn 
       rem parameter pInstrumentIdentifier
  -->
  <xsl:template name="SQLExtendAttribute">
    <xsl:param name="pExtIdentifier"/>
    <xsl:param name="pExtDet"/>
    <xsl:param name="pResultColumn"/>
    <xsl:param name="pInstr"/>
    <xsl:param name="pInstrColumn"/>
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
        inner join dbo.DEFINEEXTEND dext on (dext.IDENTIFIER=@EXTIDENTIFIER) and (dext.DTENABLED<=getdate() and (dext.DTDISABLED is null or dext.DTDISABLED>getdate()))
        and 
        (
          (dext.TYPEINSTR is null)
          or ((dext.TYPEINSTR='Product') and (dext.IDINSTR=i.IDP))
          or ((dext.TYPEINSTR='Instr') and (dext.IDINSTR=i.IDI))
          or ((dext.TYPEINSTR='GrpInstr') and (dext.IDINSTR in (
						                                  select ig.IDGINSTR 
						                                  from dbo.INSTRG ig
						                                  where (ig.IDI=i.IDI)  and (ig.DTENABLED<=getdate() and (ig.DTDISABLED is null or ig.DTDISABLED>getdate())))))
        )
        inner join dbo.DEFINEEXTENDDET dextd on (dextd.IDDEFINEEXTEND=dext.IDDEFINEEXTEND) and (dextd.IDENTIFIER=@EXTDETAIL)
        ]]>
      </xsl:text>
      <xsl:choose>
        <xsl:when test="$pInstr != $ConstUseSQL">
          <xsl:variable name ="vXMLExtlIdJoins">
            <xsl:call-template name="XMLJoinExtlId">
              <xsl:with-param name="pTableName" select="'INSTRUMENT'"/>
              <xsl:with-param name="pTableAlias" select="'i'"/>
              <xsl:with-param name="pJoinId" select="'i.IDI'"/>
              <xsl:with-param name="pValueColumn" select="$pInstrColumn"/>
              <xsl:with-param name="pValue" select="$pInstr"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:call-template name="SQLJoinExtlId">
            <xsl:with-param name="pXMLExtlIdJoins">
              <xsl:copy-of select="$vXMLExtlIdJoins"/>
            </xsl:with-param>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <!-- cas où instrument non renseigné => Restriction sur instrument à partir de l'asset ETD -->
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
          <Param name="DT" datatype="date">
            <xsl:value-of select="$pClearingBusinessDate"/>
          </Param>
        </xsl:otherwise>
      </xsl:choose>
      <Param name="EXTIDENTIFIER" datatype="string">
        <xsl:value-of select="$pExtIdentifier"/>
      </Param>
      <Param name="EXTDETAIL" datatype="string">
        <xsl:value-of select="$pExtDet"/>
      </Param>
    </SQL>
  </xsl:template>

  <!-- FI 20131122 [19233] add template LogRowInfo -->
  <xsl:template name ="LogRowInfo">
    <xsl:param name ="pRecordTypeLog"/>
    <xsl:param name ="pTradeId"/>
    <xsl:param name ="pActionTypeLog"/>
    <xsl:param name ="pStrategyType"/>
    <xsl:param name ="pTNL"/>

    <xsl:value-of select ="concat('Trade Id: ',$pTradeId,', Trade type: ',$pRecordTypeLog)"/>
    <xsl:if test="string-length($pStrategyType) > 0">
      <xsl:value-of select ="concat(', Strategy type: ',$pStrategyType,', Leg total number: ',$pTNL)"/>
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


  

  <!-- FI 20170718 [23326] Add GetIsAllowDataMissDealer -->
  <xsl:template name="GetIsAllowDataMissDealer">
    <xsl:param name="pRow"/>

    <xsl:choose>
      <xsl:when test="$pRow/data[@name='ALLOWDATAMISSDEALER']">
        <!-- S'il existe une data ALLOWDATAMISSDEALER Spheres® prend en considération sa valeur -->
        <xsl:choose>
          <xsl:when test="$pRow/data[@name='ALLOWDATAMISSDEALER']='Allowed'">
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
  </xsl:template>

  <!-- FI 20170718 [23326] Add GetIsAllowDataMissing -->
  <xsl:template name ="GetIsAllowDataMissing">
    <xsl:param name="pRow"/>

    <xsl:choose>
      <xsl:when test="$pRow/data[@name='ALLOWDATAMISSING']">
        <!-- S'il existe une data ALLOWDATAMISSING Spheres® prend en considération sa valeur -->
        <!-- sinon Spheres® utilise la valeur par défaut présente dans $gIsAllowDataMissing -->
        <xsl:choose>
          <xsl:when test="$pRow/data[@name='ALLOWDATAMISSING']='Allowed'">
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
  </xsl:template>


  <!-- FI 20170718 [23326] Add Template -->
  <!-- FI 20220214 [25699] Add pContractType -->
  <!-- FI 20220215 [25699] Add pClearingHouse and pClearingHouseColumn -->
  <!-- FI 20220225 [25699] Add pDerivativeContractSettltMethod and pDerivativeContractExerciseStyle -->  
  <xsl:template name="GetTradePartiesAndBrokersAlloc">
    <xsl:param name="pRow"/>
    <xsl:param name="pBusinessDate"/>
    <xsl:param name="pFamily"/>
    <xsl:param name="pMarket"/>
    <xsl:param name="pMarketColumn"/>
    <xsl:param name="pAssetCode"/>
    <xsl:param name="pAssetColumn"/>
    <xsl:param name="pContract"/>
    <xsl:param name="pContractColumn" />
    <xsl:param name="pContractVersion"/>
    <xsl:param name="pContractCategory"/>
    <xsl:param name="pContractType"/>
    <xsl:param name="pContractSettltMethod"/>
    <xsl:param name="pContractExerciseStyle"/>
    <xsl:param name="pClearingHouse"/>
    <xsl:param name="pClearingHouseColumn"/>
    <xsl:param name="pDynamicDataCssIdentifier"/>
    <xsl:param name="pDynamicDataMarketIdentifier"/>

    <xsl:variable name ="vRecordVersion1">
      <xsl:call-template name="GetRecordVersion">
        <xsl:with-param name="pRow" select="."/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="isAllowDataMissDealer">
      <xsl:call-template name="GetIsAllowDataMissDealer">
        <xsl:with-param name="pRow" select="."/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="isAllowDataMissing">
      <xsl:call-template name="GetIsAllowDataMissing">
        <xsl:with-param name="pRow" select="."/>
      </xsl:call-template>
    </xsl:variable>

    <!-- Dealer -->
    <xsl:variable name ="vBuyerOrSellerNode" select ="$pRow/data[@name='BSD']"/>
    <xsl:variable name ="vBuyerOrSellerIdentNode" select ="$pRow/data[@name='BSDI']"/>
    <xsl:variable name ="vBuyerOrSellerAccountNode" select ="$pRow/data[@name='BSA']"/>
    <xsl:variable name ="vBuyerOrSellerAccountIdentNode" select ="$pRow/data[@name='BSAI']"/>
    <xsl:variable name ="vBuyerOrSellerActor">
      <xsl:call-template name ="SQLDealer">
        <xsl:with-param name ="pBuyerOrSellerActorNode" select="$vBuyerOrSellerNode"/>
        <xsl:with-param name ="pBuyerOrSellerActorIdentNode" select="$vBuyerOrSellerIdentNode"/>
        <xsl:with-param name ="pBuyerOrSellerAccountNode" select="$vBuyerOrSellerAccountNode"/>
        <xsl:with-param name ="pBuyerOrSellerAccountIdentNode" select="$vBuyerOrSellerAccountIdentNode"/>
        <xsl:with-param name ="pRecordVersion" select="$vRecordVersion1"/>
        <xsl:with-param name ="pIsAllowDataMissDealer" select ="$isAllowDataMissDealer"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vBuyerOrSellerBook">
      <xsl:call-template name="ovrSQLDealerActorAndBook">
        <xsl:with-param name="pBuyerOrSellerDealer" select="$vBuyerOrSellerNode/text()"/>
        <xsl:with-param name="pBuyerOrSellerDealerIdent" select="$vBuyerOrSellerIdentNode/text()"/>
        <xsl:with-param name="pBuyerOrSellerAccount" select="$vBuyerOrSellerAccountNode/text()"/>
        <xsl:with-param name="pBuyerOrSellerAccountIdent" select="$vBuyerOrSellerAccountIdentNode/text()"/>
        <xsl:with-param name="pResultColumn" select="'BOOK_IDENTIFIER'"/>
        <xsl:with-param name="pIsAllowDataMissDealer" select ="$isAllowDataMissDealer"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vBuyerOrSellerTraderNode" select ="$pRow/data[@name='BSTR']"/>
    <xsl:variable name ="vBuyerOrSellerTraderIdentNode" select ="$pRow/data[@name='BSTRI']"/>
    <xsl:variable name ="vBuyerOrSellerTraderIsAutoCreateNode" select ="$pRow/data[@name='BSTRC']"/>
    <xsl:variable name ="vBuyerOrSellerTraderActor">
      <xsl:if test ="$vBuyerOrSellerTraderNode">
        <xsl:call-template name="SQLTrader">
          <xsl:with-param name="pValue" select="$vBuyerOrSellerTraderNode/text()"/>
          <xsl:with-param name="pValueIdent" select="$vBuyerOrSellerTraderIdentNode/text()"/>
          <xsl:with-param name="pDynamicDataCssIdentifier" select="$pDynamicDataCssIdentifier"/>
          <xsl:with-param name="pDynamicDataMarketIdentifier" select="$pDynamicDataMarketIdentifier"/>
          <xsl:with-param name="pClearingBusinessDate" select="$pBusinessDate"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name ="vBuyerOrSellerFrontIdNode" select ="$pRow/data[@name='BSFRI']"/>
    <xsl:variable name ="vBuyerOrSellerFolderNode" select ="$pRow/data[@name='BSFLI']"/>
    <xsl:variable name ="vBuyerOrSellerUTINode" select ="$pRow/data[@name='DLUTI']"/>
    <xsl:variable name ="vBuyerOrSellerInvestmentDecisionNode" select ="$pRow/data[@name='DLID']"/>
    <xsl:variable name ="vBuyerOrSellerInvestmentDecisionIdentNode" select ="$pRow/data[@name='DLIDI']"/>
    <xsl:variable name ="vBuyerOrSellerInvestmentDecisionActor">
      <xsl:if test ="$vBuyerOrSellerInvestmentDecisionNode">
        <xsl:call-template name="SQLActor">
          <xsl:with-param name="pValue" select="$vBuyerOrSellerInvestmentDecisionNode/text()"/>
          <xsl:with-param name="pValueIdent" select="$vBuyerOrSellerInvestmentDecisionIdentNode/text()"/>
          <xsl:with-param name="pResultColumn" select="IDENTIFIER"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name ="vBuyerOrSellerExecutionNode" select ="$pRow/data[@name='DLEX']"/>
    <xsl:variable name ="vBuyerOrSellerExecutionIdentNode" select ="$pRow/data[@name='DLEXI']"/>
    <xsl:variable name ="vBuyerOrSellerExecutionActor">
      <xsl:if test ="$vBuyerOrSellerExecutionNode">
        <xsl:call-template name="SQLActor">
          <xsl:with-param name="pValue" select="$vBuyerOrSellerExecutionNode/text()"/>
          <xsl:with-param name="pValueIdent" select="$vBuyerOrSellerExecutionIdentNode/text()"/>
          <xsl:with-param name="pResultColumn" select="IDENTIFIER"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>
    <xsl:call-template name="XMLParty2">
      <xsl:with-param name="pName" select="$ConstBuyerOrSeller"/>
      <xsl:with-param name="pActorParsingNode" select="$vBuyerOrSellerNode"/>
      <xsl:with-param name="pActor" select="$vBuyerOrSellerActor"/>
      <xsl:with-param name="pBookParsingNode" select="$vBuyerOrSellerAccountNode"/>
      <xsl:with-param name="pBook" select="$vBuyerOrSellerBook"/>
      <xsl:with-param name="pTraderParsingNode" select="$vBuyerOrSellerTraderNode"/>
      <xsl:with-param name="pTrader" select="$vBuyerOrSellerTraderActor"/>
      <xsl:with-param name="pTraderIsAutoCreate" select="$vBuyerOrSellerTraderIsAutoCreateNode/text()"/>
      <xsl:with-param name="pFrontIdParsingNode" select="$vBuyerOrSellerFrontIdNode"/>
      <xsl:with-param name="pFrontId" select="$vBuyerOrSellerFrontIdNode/text()"/>
      <xsl:with-param name="pFolderParsingNode" select="$vBuyerOrSellerFolderNode"/>
      <xsl:with-param name="pFolder" select="$vBuyerOrSellerFolderNode/text()"/>
      <xsl:with-param name="pUTINode" select="$vBuyerOrSellerUTINode"/>
      <xsl:with-param name="pInvestmentDecisionParsingNode" select="$vBuyerOrSellerInvestmentDecisionNode"/>
      <xsl:with-param name="pInvestmentDecisionActor" select="$vBuyerOrSellerInvestmentDecisionActor"/>
      <xsl:with-param name="pExecutionParsingNode" select="$vBuyerOrSellerExecutionNode"/>
      <xsl:with-param name="pExecutionActor" select="$vBuyerOrSellerExecutionActor"/>
      <xsl:with-param name="pTradingCapacityNode" select ="$pRow/data[@name='DLTC']"/>
      <xsl:with-param name="pWaivorIndicatorNode" select ="$pRow/data[@name='DLWI']"/>
      <xsl:with-param name="pShortSailingIndicatorNode" select ="$pRow/data[@name='DLSSI']"/>
      <xsl:with-param name="pOTCPostTradeIndicatorNode" select ="$pRow/data[@name='DLOPI']"/>
      <xsl:with-param name="pCommodityDerivativeIndicatorNode" select ="$pRow/data[@name='DLCDI']"/>
      <xsl:with-param name="pSecuritiesFinancingIndicatorNode"  select ="$pRow/data[@name='DLSFI']"/>
    </xsl:call-template>

    
    

    <!-- NegociationBroker of Dealer (il s'agit du gestionnaire du dealer) -->
    <xsl:variable name ="vNegociationBrokerNode" select ="$pRow/data[@name='BSNB']"/>
    <xsl:variable name ="vNegociationBrokerIdentNode" select ="$pRow/data[@name='BSNBI']"/>
    <xsl:variable name ="vNegociationBrokerActor">
      <xsl:choose>
        <xsl:when test ="$vNegociationBrokerNode">
          <xsl:call-template name="SQLActor">
            <xsl:with-param name="pValue" select="$vNegociationBrokerNode/text()"/>
            <xsl:with-param name="pValueIdent" select="$vNegociationBrokerIdentNode/text()"/>
          </xsl:call-template>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name ="vNegociationBrokerTraderNode" select ="$pRow/data[@name='BSNBTR']"/>
    <xsl:variable name ="vNegociationBrokerTraderIdentNode" select ="$pRow/data[@name='BSNBTRI']"/>
    <xsl:variable name ="vNegociationBrokerTraderIsAutoCreateNode" select ="$pRow/data[@name='BSNBTRC']"/>
    <xsl:variable name ="vNegociationBrokerTraderActor">
      <xsl:if test ="$vNegociationBrokerTraderNode">
        <xsl:call-template name="SQLTrader">
          <xsl:with-param name="pValue" select="$vNegociationBrokerTraderNode/text()"/>
          <xsl:with-param name="pValueIdent" select="$vNegociationBrokerTraderIdentNode/text()"/>
          <xsl:with-param name="pDynamicDataCssIdentifier" select="$pDynamicDataCssIdentifier"/>
          <xsl:with-param name="pDynamicDataMarketIdentifier" select="$pDynamicDataMarketIdentifier"/>
          <xsl:with-param name="pClearingBusinessDate" select="$pBusinessDate"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name ="vNegociationBrokerFrontIdNode" select ="$pRow/data[@name='BSNBFRI']"/>
    <xsl:call-template name="XMLParty2">
      <xsl:with-param name="pName" select="$ConstNegociationBroker"/>
      <xsl:with-param name="pActorParsingNode" select="$vNegociationBrokerNode"/>
      <xsl:with-param name="pActor" select="$vNegociationBrokerActor"/>
      <xsl:with-param name="pTraderParsingNode" select="$vNegociationBrokerTraderNode"/>
      <xsl:with-param name="pTrader" select="$vNegociationBrokerTraderActor"/>
      <xsl:with-param name="pTraderIsAutoCreate" select="$vNegociationBrokerTraderIsAutoCreateNode/text()"/>
      <xsl:with-param name="pFrontIdParsingNode" select="$vNegociationBrokerFrontIdNode"/>
      <xsl:with-param name="pFrontId" select="$vNegociationBrokerFrontIdNode/text()"/>
    </xsl:call-template>

    <!-- ClearingOrganization -->
    <xsl:variable name ="vClearingOrganizationNode" select ="$pRow/data[@name='BSCO']"/>
    <xsl:variable name ="vClearingOrganizationIdentNode" select ="$pRow/data[@name='BSCOI']"/>
    <xsl:variable name ="vClearingOrganizationAccountNode" select ="$pRow/data[@name='BSCA' or @name='BSCOA']"/>
    <xsl:variable name ="vClearingOrganizationAccountIdentNode" select ="$pRow/data[@name='BSCAI' or @name='BSCOAI']"/>
    <xsl:variable name ="vClearingOrganizationUTINode" select ="$pRow/data[@name='COUTI']"/>


    <!-- RD 20140409 [19816] use template ovrSQLClearingOrganization-->
    <xsl:variable name ="vClearingOrganizationActor">
      <!-- FI 20160929 [22507] Chgt de signature du template ovrSQLClearingOrganization -->
      <xsl:call-template name="ovrSQLClearingOrganization">
        <xsl:with-param name="pClearingOrganization" select="$vClearingOrganizationNode/text()"/>
        <xsl:with-param name="pClearingOrganizationIdent" select="$vClearingOrganizationIdentNode/text()"/>
        <xsl:with-param name="pClearingOrganizationAccount" select="$vClearingOrganizationAccountNode/text()"/>
        <xsl:with-param name="pClearingOrganizationAccountIdent" select="$vClearingOrganizationAccountIdentNode/text()"/>
        <xsl:with-param name="pClearingBusinessDate" select="$pBusinessDate"/>
        <xsl:with-param name="pFamily" select ="$pFamily"/>
        <xsl:with-param name="pMarket" select="$pMarket"/>
        <xsl:with-param name="pMarketColumn" select="$pMarketColumn"/>
        <xsl:with-param name="pAssetCode" select="$pAssetCode"/>
        <xsl:with-param name="pAssetColumn" select="$pAssetColumn"/>
        <xsl:with-param name="pDerivativeContract" select="$pContract"/>
        <xsl:with-param name="pDerivativeContractColumn" select="$pContractColumn"/>
        <xsl:with-param name="pDerivativeContractVersion" select="$pContractVersion"/>
        <xsl:with-param name="pDerivativeContractCategory" select="$pContractCategory"/>
        <xsl:with-param name="pDerivativeContractType" select="$pContractType"/>
        <xsl:with-param name="pDerivativeContractSettltMethod" select="$pContractSettltMethod"/>
        <xsl:with-param name="pDerivativeContractExerciseStyle" select="$pContractExerciseStyle"/>
        <xsl:with-param name="pClearingHouse" select="$pClearingHouse"/>
        <xsl:with-param name="pClearingHouseColumn" select="$pClearingHouseColumn"/>
        <xsl:with-param name="pBuyerOrSellerAccount" select="$vBuyerOrSellerAccountNode/text()"/>
        <xsl:with-param name="pDefaultMode">
          <xsl:choose>
            <xsl:when test ="string-length($pRow/data[@name='MID'])>0 and $pRow/data[@name='MIDTYP'] = 'GCM'">
              <!-- Recherche de la chambre de compensation à partir de l'asset -->
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
    <!-- RD 20140409 [19816] use template ovrSQLClearingOrganizationBook -->
    <xsl:variable name ="vClearingOrganizationBook">
      <!-- FI 20160929 [22507] Chgt de signature template ovrSQLClearingOrganizationBook -->
      <xsl:call-template name="ovrSQLClearingOrganizationBook">
        <xsl:with-param name="pClearingOrganizationAccount" select="$vClearingOrganizationAccountNode/text()"/>
        <xsl:with-param name="pClearingOrganizationAccountIdent" select="$vClearingOrganizationAccountIdentNode/text()"/>
        <xsl:with-param name="pBuyerOrSellerAccount" select="$vBuyerOrSellerAccountNode/text()"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:call-template name="XMLParty2">
      <xsl:with-param name="pName" select="$ConstClearingOrganization"/>
      <xsl:with-param name="pActorParsingNode" select="$vClearingOrganizationNode"/>
      <xsl:with-param name="pActor" select="$vClearingOrganizationActor"/>
      <xsl:with-param name="pBookParsingNode" select="$vClearingOrganizationAccountNode"/>
      <xsl:with-param name="pBook" select="$vClearingOrganizationBook"/>
      <!-- FI 20171103 [23326] Alimentation de pUTINode -->
      <xsl:with-param name="pUTINode" select="$vClearingOrganizationUTINode"/>
    </xsl:call-template>

    <!-- ExecutingBroker -->
    <xsl:variable name ="vExecutingBrokerNode" select ="$pRow/data[@name='BSEB']"/>
    <xsl:variable name ="vExecutingBrokerIdentNode" select ="$pRow/data[@name='BSEBI']"/>
    <xsl:variable name ="vExecutingBrokerAccountNode" select ="$pRow/data[@name='BSEBA']"/>
    <xsl:variable name ="vExecutingBrokerAccountIdentNode" select ="$pRow/data[@name='BSEBAI']"/>
    <xsl:variable name ="vExecutingBrokerActor">
      <xsl:call-template name="SQLActor">
        <xsl:with-param name="pValue" select="$vExecutingBrokerNode/text()"/>
        <xsl:with-param name="pValueIdent" select="$vExecutingBrokerIdentNode/text()"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vExecutingBrokerBook">
      <xsl:call-template name="SQLActorAndBook">
        <xsl:with-param name="pValue" select="$vExecutingBrokerAccountNode/text()"/>
        <xsl:with-param name="pValueIdent" select="$vExecutingBrokerAccountIdentNode/text()"/>
        <xsl:with-param name="pResultColumn" select="'BOOK_IDENTIFIER'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vExecutingBrokerTraderNode" select ="$pRow/data[@name='BSEBTR']"/>
    <xsl:variable name ="vExecutingBrokerTraderIdentNode" select ="$pRow/data[@name='BSEBTRI']"/>
    <xsl:variable name ="vExecutingBrokerTraderIsAutoCreateNode" select ="$pRow/data[@name='BSEBTRC']"/>
    <xsl:variable name ="vExecutingBrokerTraderActor">
      <xsl:if test ="$vExecutingBrokerTraderNode">
        <xsl:call-template name="SQLTrader">
          <xsl:with-param name="pValue" select="$vExecutingBrokerTraderNode/text()"/>
          <xsl:with-param name="pValueIdent" select="$vExecutingBrokerTraderIdentNode/text()"/>
          <xsl:with-param name="pDynamicDataCssIdentifier" select="$pDynamicDataCssIdentifier"/>
          <xsl:with-param name="pDynamicDataMarketIdentifier" select="$pDynamicDataMarketIdentifier"/>
          <xsl:with-param name="pClearingBusinessDate" select="$pBusinessDate"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name ="vExecutingBrokerFrontIdNode" select ="$pRow/data[@name='BSEBFRI']"/>
    <xsl:call-template name="XMLParty2">
      <xsl:with-param name="pName" select="$ConstExecutingBroker"/>
      <xsl:with-param name="pActorParsingNode" select="$vExecutingBrokerNode"/>
      <xsl:with-param name="pActor"  select="$vExecutingBrokerActor"/>
      <xsl:with-param name="pBookParsingNode" select="$vExecutingBrokerAccountNode"/>
      <xsl:with-param name="pBook" select="$vExecutingBrokerBook"/>
      <xsl:with-param name="pTraderParsingNode" select="$vExecutingBrokerTraderNode"/>
      <xsl:with-param name="pTrader" select="$vExecutingBrokerTraderActor"/>
      <xsl:with-param name="pTraderIsAutoCreate" select="$vExecutingBrokerTraderIsAutoCreateNode/text()"/>
      <xsl:with-param name="pFrontIdParsingNode" select="$vExecutingBrokerFrontIdNode"/>
      <xsl:with-param name="pFrontId" select="$vExecutingBrokerFrontIdNode/text()"/>
    </xsl:call-template>

    <!-- ClearingBroker -->
    <xsl:variable name ="vClearingBrokerNode" select ="$pRow/data[@name='BSCB']"/>
    <xsl:variable name ="vClearingBrokerIdentNode" select ="$pRow/data[@name='BSCBI']"/>
    <xsl:variable name ="vClearingBrokerAccountNode" select ="$pRow/data[@name='BSCBA']"/>
    <xsl:variable name ="vClearingBrokerAccountIdentNode" select ="$pRow/data[@name='BSCBAI']"/>
    <xsl:variable name ="vClearingBrokerActor">
      <xsl:call-template name="SQLActor">
        <xsl:with-param name="pValue" select="$vClearingBrokerNode/text()"/>
        <xsl:with-param name="pValueIdent" select="$vClearingBrokerIdentNode/text()"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vClearingBrokerBook">
      <xsl:call-template name="SQLActorAndBook">
        <xsl:with-param name="pValue" select="$vClearingBrokerAccountNode/text()"/>
        <xsl:with-param name="pValueIdent" select="$vClearingBrokerAccountIdentNode/text()"/>
        <xsl:with-param name="pResultColumn" select="'BOOK_IDENTIFIER'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vClearingBrokerTraderNode" select ="$pRow/data[@name='BSCBTR']"/>
    <xsl:variable name ="vClearingBrokerTraderIdentNode" select ="$pRow/data[@name='BSCBTRI']"/>
    <xsl:variable name ="vClearingBrokerTraderIsAutoCreateNode" select ="$pRow/data[@name='BSCBTRC']"/>
    <xsl:variable name ="vClearingBrokerTraderActor">
      <xsl:if test ="$vClearingBrokerTraderNode">
        <xsl:call-template name="SQLTrader">
          <xsl:with-param name="pValue" select="$vClearingBrokerTraderNode/text()"/>
          <xsl:with-param name="pValueIdent" select="$vClearingBrokerTraderIdentNode/text()"/>
          <xsl:with-param name="pDynamicDataCssIdentifier" select="$pDynamicDataCssIdentifier"/>
          <xsl:with-param name="pDynamicDataMarketIdentifier" select="$pDynamicDataMarketIdentifier"/>
          <xsl:with-param name="pClearingBusinessDate" select="$pBusinessDate"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name ="vClearingBrokerFrontIdNode" select ="$pRow/data[@name='BSEBFRI']"/>
    <xsl:call-template name="XMLParty2">
      <xsl:with-param name="pName" select="$ConstClearingBroker"/>
      <xsl:with-param name="pActorParsingNode" select="$vClearingBrokerNode"/>
      <xsl:with-param name="pActor" select="$vClearingBrokerActor"/>
      <xsl:with-param name="pBookParsingNode" select="$vClearingBrokerAccountNode"/>
      <xsl:with-param name="pBook" select="$vClearingBrokerBook"/>
      <xsl:with-param name="pTraderParsingNode" select="$vClearingBrokerTraderNode"/>
      <xsl:with-param name="pTrader" select="$vClearingBrokerTraderActor"/>
      <xsl:with-param name="pTraderIsAutoCreate" select="$vClearingBrokerTraderIsAutoCreateNode/text()"/>
      <xsl:with-param name="pFrontIdParsingNode" select="$vClearingBrokerFrontIdNode"/>
      <xsl:with-param name="pFrontId" select="$vClearingBrokerFrontIdNode/text()"/>
    </xsl:call-template>
  </xsl:template>

  <!-- FI 20170718 [23326] Add Template  -->
  <xsl:template name="GetTradePartiesAndBrokersExecution">
    <xsl:param name="pRow"/>

    <xsl:variable name ="vRecordVersion1">
      <xsl:call-template name="GetRecordVersion">
        <xsl:with-param name="pRow" select="."/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="isAllowDataMissDealer">
      <xsl:call-template name="GetIsAllowDataMissDealer">
        <xsl:with-param name="pRow" select="."/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="isAllowDataMissing">
      <xsl:call-template name="GetIsAllowDataMissing">
        <xsl:with-param name="pRow" select="."/>
      </xsl:call-template>
    </xsl:variable>

    <!-- Buyer -->
    <xsl:variable name ="vBuyerNode" select ="$pRow/data[@name='BYD']"/>
    <xsl:variable name ="vBuyerIdentNode" select ="$pRow/data[@name='BYDI']"/>
    <xsl:variable name ="vBuyerAccountNode" select ="$pRow/data[@name='BYA']"/>
    <xsl:variable name ="vBuyerAccountIdentNode" select ="$pRow/data[@name='BYAI']"/>
    <xsl:variable name ="vBuyerActor">
      <xsl:call-template name ="SQLDealer">
        <xsl:with-param name ="pBuyerOrSellerActorNode" select="$vBuyerNode"/>
        <xsl:with-param name ="pBuyerOrSellerActorIdentNode" select="$vBuyerIdentNode"/>
        <xsl:with-param name ="pBuyerOrSellerAccountNode" select="$vBuyerAccountNode"/>
        <xsl:with-param name ="pBuyerOrSellerAccountIdentNode" select="$vBuyerAccountIdentNode"/>
        <xsl:with-param name ="pRecordVersion" select="$vRecordVersion1"/>
        <xsl:with-param name ="pIsAllowDataMissDealer" select ="$isAllowDataMissDealer"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vBuyerBook">
      <xsl:call-template name="SQLActorAndBook">
        <xsl:with-param name="pValue" select="$vBuyerAccountNode/text()"/>
        <xsl:with-param name="pValueIdent" select="$vBuyerAccountIdentNode/text()"/>
        <xsl:with-param name="pResultColumn" select="'BOOK_IDENTIFIER'"/>
        <xsl:with-param name="pUseDefaultValue" select ="$isAllowDataMissDealer"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vBuyerTraderNode" select ="$pRow/data[@name='BYTR']"/>
    <xsl:variable name ="vBuyerTraderIdentNode" select ="$pRow/data[@name='BYTRI']"/>
    <xsl:variable name ="vBuyerTraderIsAutoCreateNode" select ="$pRow/data[@name='BYTRC']"/>
    <xsl:variable name ="vBuyerTraderActor">
      <xsl:if test ="$vBuyerTraderNode">
        <xsl:call-template name="SQLTrader">
          <xsl:with-param name="pValue" select="$vBuyerTraderNode/text()"/>
          <xsl:with-param name="pValueIdent" select="$vBuyerTraderIdentNode/text()"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name ="vBuyerFrontIdNode" select ="$pRow/data[@name='BYFRI']"/>
    <xsl:variable name ="vBuyerFolderNode" select ="$pRow/data[@name='BYFLI']"/>
    <xsl:variable name ="vBuyerUTINode" select ="$pRow/data[@name='BYUTI']"/>
    <xsl:variable name ="vBuyerInvestmentDecisionNode" select ="$pRow/data[@name='BYID']"/>
    <xsl:variable name ="vBuyerInvestmentDecisionIdentNode" select ="$pRow/data[@name='BYIDI']"/>
    <xsl:variable name ="vBuyerInvestmentDecisionActor">
      <xsl:if test ="$vBuyerInvestmentDecisionNode">
        <xsl:call-template name="SQLActor">
          <xsl:with-param name="pValue" select="$vBuyerInvestmentDecisionNode/text()"/>
          <xsl:with-param name="pValueIdent" select="$vBuyerInvestmentDecisionIdentNode/text()"/>
          <xsl:with-param name="pResultColumn" select="IDENTIFIER"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name ="vBuyerExecutionNode" select ="$pRow/data[@name='BYEX']"/>
    <xsl:variable name ="vBuyerExecutionIdentNode" select ="$pRow/data[@name='BYEXI']"/>
    <xsl:variable name ="vBuyerExecutionActor">
      <xsl:if test ="$vBuyerExecutionNode">
        <xsl:call-template name="SQLActor">
          <xsl:with-param name="pValue" select="$vBuyerExecutionNode/text()"/>
          <xsl:with-param name="pValueIdent" select="$vBuyerExecutionIdentNode/text()"/>
          <xsl:with-param name="pResultColumn" select="IDENTIFIER"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>
    <!-- FI 20171103 [23326] Add EMIR/MiFIR -->
    <xsl:call-template name="XMLParty2">
      <xsl:with-param name="pName" select="$ConstBuyer"/>
      <xsl:with-param name="pActorParsingNode" select ="$vBuyerNode" />
      <xsl:with-param name="pActor" select ="$vBuyerActor" />
      <xsl:with-param name="pBookParsingNode" select ="$vBuyerAccountNode" />
      <xsl:with-param name="pBook" select ="$vBuyerBook"/>
      <xsl:with-param name="pTraderParsingNode" select="$vBuyerTraderNode"/>
      <xsl:with-param name="pTrader" select="$vBuyerTraderActor"/>
      <xsl:with-param name="pTraderIsAutoCreate" select="$vBuyerTraderIsAutoCreateNode/text()"/>
      <xsl:with-param name="pFrontIdParsingNode" select="$vBuyerFrontIdNode"/>
      <xsl:with-param name="pFrontId" select="$vBuyerFrontIdNode/text()"/>
      <xsl:with-param name="pFolderParsingNode" select="$vBuyerFolderNode"/>
      <xsl:with-param name="pFolder" select="$vBuyerFolderNode/text()"/>
      <xsl:with-param name="pUTINode" select="$vBuyerUTINode"/>
      <xsl:with-param name="pInvestmentDecisionParsingNode" select="$vBuyerInvestmentDecisionNode"/>
      <xsl:with-param name="pInvestmentDecisionActor" select="$vBuyerInvestmentDecisionActor"/>
      <xsl:with-param name="pExecutionParsingNode" select="$vBuyerExecutionNode"/>
      <xsl:with-param name="pExecutionActor" select="$vBuyerExecutionActor"/>
      <xsl:with-param name="pTradingCapacityNode" select ="$pRow/data[@name='BYTC']"/>
      <xsl:with-param name="pWaivorIndicatorNode" select ="$pRow/data[@name='BYWI']"/>
      <xsl:with-param name="pShortSailingIndicatorNode" select ="$pRow/data[@name='BYSSI']"/>
      <xsl:with-param name="pOTCPostTradeIndicatorNode" select ="$pRow/data[@name='BYOPI']"/>
      <xsl:with-param name="pCommodityDerivativeIndicatorNode" select ="$pRow/data[@name='BYCDI']"/>
      <xsl:with-param name="pSecuritiesFinancingIndicatorNode"  select ="$pRow/data[@name='BYSFI']"/>
    </xsl:call-template>

    <!-- BuyerNegociationBroker -->
    <xsl:variable name ="vBuyerNegociationBrokerNode" select ="data[@name='BYNB']"/>
    <xsl:variable name ="vBuyerNegociationBrokerIdentNode" select ="data[@name='BYNBI']"/>
    <xsl:variable name ="vBuyerNegociationBrokerActor">
      <xsl:call-template name="SQLActor">
        <xsl:with-param name="pValue" select="$vBuyerNegociationBrokerNode/text()"/>
        <xsl:with-param name="pValueIdent" select="$vBuyerNegociationBrokerIdentNode/text()"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vBuyerNegociationBrokerTraderNode" select ="$pRow/data[@name='BYNBTR']"/>
    <xsl:variable name ="vBuyerNegociationBrokerTraderIdentNode" select ="$pRow/data[@name='BYNBTRI']"/>
    <xsl:variable name ="vBuyerNegociationBrokerTraderIsAutoCreateNode" select ="$pRow/data[@name='BYNBTRC']"/>
    <xsl:variable name ="vBuyerNegociationBrokerTraderActor">
      <xsl:if test ="$vBuyerNegociationBrokerTraderNode">
        <xsl:call-template name="SQLTrader">
          <xsl:with-param name="pValue" select="$vBuyerNegociationBrokerTraderNode/text()"/>
          <xsl:with-param name="pValueIdent" select="$vBuyerNegociationBrokerTraderIdentNode/text()"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name ="vBuyerNegociationBrokerFrontIdNode" select ="$pRow/data[@name='BYNBFRI']"/>
    <xsl:call-template name="XMLParty2">
      <xsl:with-param name="pName" select="$ConstBuyerNegociationBroker"/>
      <xsl:with-param name="pActorParsingNode" select ="$vBuyerNegociationBrokerNode" />
      <xsl:with-param name="pActor" select ="$vBuyerNegociationBrokerActor" />
      <xsl:with-param name="pTraderParsingNode" select="$vBuyerNegociationBrokerTraderNode"/>
      <xsl:with-param name="pTrader" select="$vBuyerNegociationBrokerTraderActor"/>
      <xsl:with-param name="pTraderIsAutoCreate" select="$vBuyerNegociationBrokerTraderIsAutoCreateNode/text()"/>
      <xsl:with-param name="pFrontIdParsingNode" select="$vBuyerNegociationBrokerFrontIdNode"/>
      <xsl:with-param name="pFrontId" select="$vBuyerNegociationBrokerFrontIdNode/text()"/>
    </xsl:call-template>

    <!-- BuyerExecutingBroker -->
    <xsl:variable name ="vBuyerExecutingBrokerNode" select ="data[@name='BYEB']"/>
    <xsl:variable name ="vBuyerExecutingBrokerIdentNode" select ="data[@name='BYEBI']"/>
    <xsl:variable name ="vBuyerExecutingBrokerActor">
      <xsl:call-template name="SQLActor">
        <xsl:with-param name="pValue" select="$vBuyerExecutingBrokerNode/text()"/>
        <xsl:with-param name="pValueIdent" select="$vBuyerExecutingBrokerIdentNode/text()"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vBuyerExecutingBrokerTraderNode" select ="$pRow/data[@name='BYEBTR']"/>
    <xsl:variable name ="vBuyerExecutingBrokerTraderIdentNode" select ="$pRow/data[@name='BYEBTRI']"/>
    <xsl:variable name ="vBuyerExecutingBrokerTraderIsAutoCreateNode" select ="$pRow/data[@name='BYEBTRC']"/>
    <xsl:variable name ="vBuyerExecutingBrokerTraderActor">
      <xsl:if test ="$vBuyerExecutingBrokerTraderNode">
        <xsl:call-template name="SQLTrader">
          <xsl:with-param name="pValue" select="$vBuyerExecutingBrokerTraderNode/text()"/>
          <xsl:with-param name="pValueIdent" select="$vBuyerExecutingBrokerTraderIdentNode/text()"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name ="vBuyerExecutingBrokerFrontIdNode" select ="$pRow/data[@name='BYEBFRI']"/>
    <xsl:call-template name="XMLParty2">
      <xsl:with-param name="pName" select="$ConstBuyerExecutingBroker"/>
      <xsl:with-param name="pActorParsingNode" select ="$vBuyerExecutingBrokerNode" />
      <xsl:with-param name="pActor" select ="$vBuyerExecutingBrokerActor" />
      <xsl:with-param name="pTraderParsingNode" select="$vBuyerExecutingBrokerTraderNode"/>
      <xsl:with-param name="pTrader" select="$vBuyerExecutingBrokerTraderActor"/>
      <xsl:with-param name="pTraderIsAutoCreate" select="$vBuyerExecutingBrokerTraderIsAutoCreateNode/text()"/>
      <xsl:with-param name="pFrontIdParsingNode" select="$vBuyerExecutingBrokerFrontIdNode"/>
      <xsl:with-param name="pFrontId" select="$vBuyerExecutingBrokerFrontIdNode/text()"/>
    </xsl:call-template>

    <!-- BuyerClearingBroker -->
    <xsl:variable name ="vBuyerClearingBrokerNode" select ="data[@name='BYCB']"/>
    <xsl:variable name ="vBuyerClearingBrokerIdentNode" select ="data[@name='BYCBI']"/>
    <xsl:variable name ="vBuyerClearingBrokerActor">
      <xsl:call-template name="SQLActor">
        <xsl:with-param name="pValue" select="$vBuyerClearingBrokerNode/text()"/>
        <xsl:with-param name="pValueIdent" select="$vBuyerClearingBrokerIdentNode/text()"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vBuyerClearingBrokerTraderNode" select ="$pRow/data[@name='BYCBTR']"/>
    <xsl:variable name ="vBuyerClearingBrokerTraderIdentNode" select ="$pRow/data[@name='BYCBTRI']"/>
    <xsl:variable name ="vBuyerClearingBrokerTraderIsAutoCreateNode" select ="$pRow/data[@name='BYCBTRC']"/>
    <xsl:variable name ="vBuyerClearingBrokerTraderActor">
      <xsl:if test ="$vBuyerClearingBrokerTraderNode">
        <xsl:call-template name="SQLTrader">
          <xsl:with-param name="pValue" select="$vBuyerClearingBrokerTraderNode/text()"/>
          <xsl:with-param name="pValueIdent" select="$vBuyerClearingBrokerTraderIdentNode/text()"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name ="vBuyerClearingBrokerFrontIdNode" select ="$pRow/data[@name='BYCBFRI']"/>
    <xsl:call-template name="XMLParty2">
      <xsl:with-param name="pName" select="$ConstBuyerClearingBroker"/>
      <xsl:with-param name="pActorParsingNode" select ="$vBuyerClearingBrokerNode" />
      <xsl:with-param name="pActor" select ="$vBuyerClearingBrokerActor" />
      <xsl:with-param name="pTraderParsingNode" select="$vBuyerClearingBrokerTraderNode"/>
      <xsl:with-param name="pTrader" select="$vBuyerClearingBrokerTraderActor"/>
      <xsl:with-param name="pTraderIsAutoCreate" select="$vBuyerClearingBrokerTraderIsAutoCreateNode/text()"/>
      <xsl:with-param name="pFrontIdParsingNode" select="$vBuyerClearingBrokerFrontIdNode"/>
      <xsl:with-param name="pFrontId" select="$vBuyerClearingBrokerFrontIdNode/text()"/>
    </xsl:call-template>

    <!-- Seller -->
    <xsl:variable name ="vSellerNode" select ="$pRow/data[@name='SLD']"/>
    <xsl:variable name ="vSellerIdentNode" select ="$pRow/data[@name='SLDI']"/>
    <xsl:variable name ="vSellerAccountNode" select ="$pRow/data[@name='SLA']"/>
    <xsl:variable name ="vSellerAccountIdentNode" select ="$pRow/data[@name='SLAI']"/>
    <xsl:variable name ="vSellerActor">
      <xsl:call-template name ="SQLDealer">
        <xsl:with-param name ="pBuyerOrSellerActorNode" select="$vSellerNode"/>
        <xsl:with-param name ="pBuyerOrSellerActorIdentNode" select="$vSellerIdentNode"/>
        <xsl:with-param name ="pBuyerOrSellerAccountNode" select="$vSellerAccountNode"/>
        <xsl:with-param name ="pBuyerOrSellerAccountIdentNode" select="$vSellerAccountIdentNode"/>
        <xsl:with-param name ="pRecordVersion" select="$vRecordVersion1"/>
        <xsl:with-param name ="pIsAllowDataMissDealer" select ="$isAllowDataMissDealer"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vSellerBook">
      <xsl:call-template name="SQLActorAndBook">
        <xsl:with-param name="pValue" select="$vSellerAccountNode/text()"/>
        <xsl:with-param name="pValueIdent" select="$vSellerAccountIdentNode/text()"/>
        <xsl:with-param name="pResultColumn" select="'BOOK_IDENTIFIER'"/>
        <xsl:with-param name="pUseDefaultValue" select ="$isAllowDataMissDealer"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vSellerTraderNode" select ="$pRow/data[@name='SLTR']"/>
    <xsl:variable name ="vSellerTraderIdentNode" select ="$pRow/data[@name='SLTRI']"/>
    <xsl:variable name ="vSellerTraderIsAutoCreateNode" select ="$pRow/data[@name='SLTRC']"/>
    <xsl:variable name ="vSellerTraderActor">
      <xsl:if test ="$vSellerTraderNode">
        <xsl:call-template name="SQLTrader">
          <xsl:with-param name="pValue" select="$vSellerTraderNode/text()"/>
          <xsl:with-param name="pValueIdent" select="$vSellerTraderIdentNode/text()"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name ="vSellerFrontIdNode" select ="$pRow/data[@name='SLFRI']"/>
    <xsl:variable name ="vSellerFolderNode" select ="$pRow/data[@name='SLFLI']"/>
    <xsl:variable name ="vSellerUTINode" select ="$pRow/data[@name='SLUTI']"/>
    <xsl:variable name ="vSellerInvestmentDecisionNode" select ="$pRow/data[@name='SLID']"/>
    <xsl:variable name ="vSellerInvestmentDecisionIdentNode" select ="$pRow/data[@name='SLIDI']"/>
    <xsl:variable name ="vSellerInvestmentDecisionActor">
      <xsl:if test ="$vSellerInvestmentDecisionNode">
        <xsl:call-template name="SQLActor">
          <xsl:with-param name="pValue" select="$vSellerInvestmentDecisionNode/text()"/>
          <xsl:with-param name="pValueIdent" select="$vSellerInvestmentDecisionIdentNode/text()"/>
          <xsl:with-param name="pResultColumn" select="IDENTIFIER"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name ="vSellerExecutionNode" select ="$pRow/data[@name='SLEX']"/>
    <xsl:variable name ="vSellerExecutionIdentNode" select ="$pRow/data[@name='SLEXI']"/>
    <xsl:variable name ="vSellerExecutionActor">
      <xsl:if test ="$vSellerExecutionNode">
        <xsl:call-template name="SQLActor">
          <xsl:with-param name="pValue" select="$vSellerExecutionNode/text()"/>
          <xsl:with-param name="pValueIdent" select="$vSellerExecutionIdentNode/text()"/>
          <xsl:with-param name="pResultColumn" select="IDENTIFIER"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>
    <!-- FI 20171103 [23326] Add EMIR/MiFIR info -->
    <xsl:call-template name="XMLParty2">
      <xsl:with-param name="pName" select="$ConstSeller"/>
      <xsl:with-param name="pActorParsingNode" select ="$vSellerNode" />
      <xsl:with-param name="pActor" select ="$vSellerActor" />
      <xsl:with-param name="pBookParsingNode" select ="$vSellerAccountNode" />
      <xsl:with-param name="pBook" select ="$vSellerBook"/>
      <xsl:with-param name="pTraderParsingNode" select="$vSellerTraderNode"/>
      <xsl:with-param name="pTrader" select="$vSellerTraderActor"/>
      <xsl:with-param name="pTraderIsAutoCreate" select="$vSellerTraderIsAutoCreateNode/text()"/>
      <xsl:with-param name="pFrontIdParsingNode" select="$vSellerFrontIdNode"/>
      <xsl:with-param name="pFrontId" select="$vSellerFrontIdNode/text()"/>
      <xsl:with-param name="pFolderParsingNode" select="$vSellerFolderNode"/>
      <xsl:with-param name="pFolder" select="$vSellerFolderNode/text()"/>
      <xsl:with-param name="pUTINode" select="$vSellerUTINode"/>
      <xsl:with-param name="pInvestmentDecisionParsingNode" select="$vSellerInvestmentDecisionNode"/>
      <xsl:with-param name="pInvestmentDecisionActor" select="$vSellerInvestmentDecisionActor"/>
      <xsl:with-param name="pExecutionParsingNode" select="$vSellerExecutionNode"/>
      <xsl:with-param name="pExecutionActor" select="$vSellerExecutionActor"/>
      <xsl:with-param name="pTradingCapacityNode" select ="$pRow/data[@name='SLTC']"/>
      <xsl:with-param name="pWaivorIndicatorNode" select ="$pRow/data[@name='SLWI']"/>
      <xsl:with-param name="pShortSailingIndicatorNode" select ="$pRow/data[@name='SLSSI']"/>
      <xsl:with-param name="pOTCPostTradeIndicatorNode" select ="$pRow/data[@name='SLOPI']"/>
      <xsl:with-param name="pCommodityDerivativeIndicatorNode" select ="$pRow/data[@name='SLCDI']"/>
      <xsl:with-param name="pSecuritiesFinancingIndicatorNode"  select ="$pRow/data[@name='SLSFI']"/>
    </xsl:call-template>

    <!-- SellerNegociationBroker -->
    <xsl:variable name ="vSellerNegociationBrokerNode" select ="data[@name='SLNB']"/>
    <xsl:variable name ="vSellerNegociationBrokerIdentNode" select ="data[@name='SLNBI']"/>
    <xsl:variable name ="vSellerNegociationBrokerActor">
      <xsl:call-template name="SQLActor">
        <xsl:with-param name="pValue" select="$vSellerNegociationBrokerNode/text()"/>
        <xsl:with-param name="pValueIdent" select="$vSellerNegociationBrokerIdentNode/text()"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vSellerNegociationBrokerTraderNode" select ="$pRow/data[@name='SLNBTR']"/>
    <xsl:variable name ="vSellerNegociationBrokerTraderIdentNode" select ="$pRow/data[@name='SLNBTRI']"/>
    <xsl:variable name ="vSellerNegociationBrokerTraderIsAutoCreateNode" select ="$pRow/data[@name='SLNBTRC']"/>
    <xsl:variable name ="vSellerNegociationBrokerTraderActor">
      <xsl:if test ="$vSellerNegociationBrokerTraderNode">
        <xsl:call-template name="SQLTrader">
          <xsl:with-param name="pValue" select="$vSellerNegociationBrokerTraderNode/text()"/>
          <xsl:with-param name="pValueIdent" select="$vSellerNegociationBrokerTraderIdentNode/text()"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name ="vSellerNegociationBrokerFrontIdNode" select ="$pRow/data[@name='SLNBFRI']"/>
    <xsl:call-template name="XMLParty2">
      <xsl:with-param name="pName" select="$ConstSellerNegociationBroker"/>
      <xsl:with-param name="pActorParsingNode" select ="$vSellerNegociationBrokerNode" />
      <xsl:with-param name="pActor" select ="$vSellerNegociationBrokerActor" />
      <xsl:with-param name="pTraderParsingNode" select="$vSellerNegociationBrokerTraderNode"/>
      <xsl:with-param name="pTrader" select="$vSellerNegociationBrokerTraderActor"/>
      <xsl:with-param name="pTraderIsAutoCreate" select="$vSellerNegociationBrokerTraderIsAutoCreateNode/text()"/>
      <xsl:with-param name="pFrontIdParsingNode" select="$vSellerNegociationBrokerFrontIdNode"/>
      <xsl:with-param name="pFrontId" select="$vSellerNegociationBrokerFrontIdNode/text()"/>
    </xsl:call-template>

    <!-- SellerExecutingBroker -->
    <xsl:variable name ="vSellerExecutingBrokerNode" select ="data[@name='SLEB']"/>
    <xsl:variable name ="vSellerExecutingBrokerIdentNode" select ="data[@name='SLEBI']"/>
    <xsl:variable name ="vSellerExecutingBrokerActor">
      <xsl:call-template name="SQLActor">
        <xsl:with-param name="pValue" select="$vSellerExecutingBrokerNode/text()"/>
        <xsl:with-param name="pValueIdent" select="$vSellerExecutingBrokerIdentNode/text()"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vSellerExecutingBrokerTraderNode" select ="$pRow/data[@name='SLEBTR']"/>
    <xsl:variable name ="vSellerExecutingBrokerTraderIdentNode" select ="$pRow/data[@name='SLEBTRI']"/>
    <xsl:variable name ="vSellerExecutingBrokerTraderIsAutoCreateNode" select ="$pRow/data[@name='SLEBTRC']"/>
    <xsl:variable name ="vSellerExecutingBrokerTraderActor">
      <xsl:if test ="$vSellerExecutingBrokerTraderNode">
        <xsl:call-template name="SQLTrader">
          <xsl:with-param name="pValue" select="$vSellerNegociationBrokerTraderNode/text()"/>
          <xsl:with-param name="pValueIdent" select="$vSellerNegociationBrokerTraderIdentNode/text()"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name ="vSellerExecutingBrokerFrontIdNode" select ="$pRow/data[@name='SLEBFRI']"/>
    <xsl:call-template name="XMLParty2">
      <xsl:with-param name="pName" select="$ConstSellerExecutingBroker"/>
      <xsl:with-param name="pActorParsingNode" select ="$vSellerExecutingBrokerNode" />
      <xsl:with-param name="pActor" select ="$vSellerExecutingBrokerActor" />
      <xsl:with-param name="pTraderParsingNode" select="$vSellerExecutingBrokerTraderNode"/>
      <xsl:with-param name="pTrader" select="$vSellerExecutingBrokerTraderActor"/>
      <xsl:with-param name="pTraderIsAutoCreate" select="$vSellerExecutingBrokerTraderIsAutoCreateNode/text()"/>
      <xsl:with-param name="pFrontIdParsingNode" select="$vSellerExecutingBrokerFrontIdNode"/>
      <xsl:with-param name="pFrontId" select="$vSellerExecutingBrokerFrontIdNode/text()"/>
    </xsl:call-template>

    <!-- SellerClearingBroker -->
    <xsl:variable name ="vSellerClearingBrokerNode" select ="data[@name='SLCB']"/>
    <xsl:variable name ="vSellerClearingBrokerIdentNode" select ="data[@name='SLCBI']"/>
    <xsl:variable name ="vSellerClearingBrokerActor">
      <xsl:call-template name="SQLActor">
        <xsl:with-param name="pValue" select="$vSellerClearingBrokerNode/text()"/>
        <xsl:with-param name="pValueIdent" select="$vSellerClearingBrokerIdentNode/text()"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vSellerClearingBrokerTraderNode" select ="$pRow/data[@name='SLCBTR']"/>
    <xsl:variable name ="vSellerClearingBrokerTraderIdentNode" select ="$pRow/data[@name='SLCBTRI']"/>
    <xsl:variable name ="vSellerClearingBrokerTraderIsAutoCreateNode" select ="$pRow/data[@name='SLCBTRC']"/>
    <xsl:variable name ="vSellerClearingBrokerTraderActor">
      <xsl:if test ="$vSellerClearingBrokerTraderNode">
        <xsl:call-template name="SQLTrader">
          <xsl:with-param name="pValue" select="$vSellerClearingBrokerTraderNode/text()"/>
          <xsl:with-param name="pValueIdent" select="$vSellerClearingBrokerTraderIdentNode/text()"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name ="vSellerClearingBrokerFrontIdNode" select ="$pRow/data[@name='SLCBFRI']"/>
    <xsl:call-template name="XMLParty2">
      <xsl:with-param name="pName" select="$ConstSellerClearingBroker"/>
      <xsl:with-param name="pActorParsingNode" select ="$vSellerClearingBrokerNode" />
      <xsl:with-param name="pActor" select ="$vSellerClearingBrokerActor" />
      <xsl:with-param name="pTraderParsingNode" select="$vSellerClearingBrokerTraderNode"/>
      <xsl:with-param name="pTrader" select="$vSellerClearingBrokerTraderActor"/>
      <xsl:with-param name="pTraderIsAutoCreate" select="$vSellerClearingBrokerTraderIsAutoCreateNode/text()"/>
      <xsl:with-param name="pFrontIdParsingNode" select="$vSellerClearingBrokerFrontIdNode"/>
      <xsl:with-param name="pFrontId" select="$vSellerClearingBrokerFrontIdNode/text()"/>
    </xsl:call-template>
  </xsl:template>

  <!-- FI 20170718 [23326] Add Template
  Génération des ccis spécifiques aux parties (Dealer, Clearing, Executuing Broker, Clearing Broker)
  à partir de la variable pTradePartiesAndBrokersNode
  -->
  <xsl:template name ="GetCcisTradePartiesAlloc">
    <xsl:param name ="pRow"/>
    <xsl:param name ="pTradePartiesAndBrokersNode"/>

    <xsl:variable name ="isAllowDataMissDealer">
      <xsl:call-template name="GetIsAllowDataMissDealer">
        <xsl:with-param name="pRow" select="."/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="isAllowDataMissing">
      <xsl:call-template name="GetIsAllowDataMissing">
        <xsl:with-param name="pRow" select="."/>
      </xsl:call-template>
    </xsl:variable>

    <!-- côté Dealer -->
    <!-- Dealer (actor) -->
    <xsl:if test ="$pTradePartiesAndBrokersNode/party[@name=$ConstBuyerOrSeller]">
      <xsl:variable name ="vParty" select ="$pTradePartiesAndBrokersNode/party[@name=$ConstBuyerOrSeller]"/>
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="'tradeHeader_party1_actor'"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pIsMissingMode" select="$isAllowDataMissDealer"/>
        <xsl:with-param name="pIsMissingModeSpecified" select="$ConstTrue"/>
        <xsl:with-param name="pDynamicValue" select ="$vParty/dynamicValue"/>
        <xsl:with-param name="pIsMandatory" select="$ConstTrue"/>
      </xsl:call-template>
    </xsl:if>
    <!-- Dealer (book) -->
    <xsl:if test ="$pTradePartiesAndBrokersNode/book[@name=$ConstBuyerOrSeller]">
      <xsl:variable name ="vBook" select ="$pTradePartiesAndBrokersNode/book[@name=$ConstBuyerOrSeller]"/>
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="'tradeHeader_party1_book'"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pIsMissingMode" select="$isAllowDataMissDealer"/>
        <xsl:with-param name="pIsMissingModeSpecified" select="$ConstTrue" />
        <xsl:with-param name="pDynamicValue" select ="$vBook/dynamicValue"/>
        <xsl:with-param name="pIsMandatory" select="$ConstTrue"/>
      </xsl:call-template>
    </xsl:if>
    <!-- Dealer (frontId) -->
    <xsl:if test ="$pTradePartiesAndBrokersNode/frontId[@name=$ConstBuyerOrSeller]">
      <xsl:variable name ="vFront" select ="$pTradePartiesAndBrokersNode/frontId[@name=$ConstBuyerOrSeller]"/>
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="'tradeHeader_party1_frontId'"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pDynamicValue" select ="$vFront/dynamicValue"/>
      </xsl:call-template>
    </xsl:if>
    <!-- Dealer (folder) -->
    <xsl:if test ="$pTradePartiesAndBrokersNode/folder[@name=$ConstBuyerOrSeller]">
      <xsl:variable name ="vFolder" select ="$pTradePartiesAndBrokersNode/folder[@name=$ConstBuyerOrSeller]"/>
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="'tradeHeader_party1_folder'"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pDynamicValue" select ="$vFolder/dynamicValue"/>
      </xsl:call-template>
    </xsl:if>
    <!-- Dealer (trader) -->
    <xsl:if test ="$pTradePartiesAndBrokersNode/trader[@name=$ConstBuyerOrSeller]">
      <xsl:variable name ="vTrader" select ="$pTradePartiesAndBrokersNode/trader[@name=$ConstBuyerOrSeller]"/>
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="'tradeHeader_party1_trader1_identifier'"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pDynamicValue" select ="$vTrader/dynamicValue"/>
        <xsl:with-param name="pIsAutoCreate" select="$vTrader/@isAutoCreate"/>
        <xsl:with-param name="pIsMissingMode" select="$isAllowDataMissing"/>
        <xsl:with-param name="pIsMissingModeSpecified" select="$ConstTrue" />
      </xsl:call-template>
    </xsl:if>
    <!-- Dealer (sales) -->
    <xsl:variable name ="vXMLBuyerOrSellerSales">
      <sales>
        <xsl:call-template name="XMLSale">
          <xsl:with-param name="pSale" select ="$pRow/data[@name='BSSL1']"/>
          <xsl:with-param name="pSaleIdent" select ="$pRow/data[@name='BSSLI1']"/>
          <xsl:with-param name="pCoeff" select ="$pRow/data[@name='BSSLC1']"/>
        </xsl:call-template>
        <xsl:call-template name="XMLSale">
          <xsl:with-param name="pSale" select ="$pRow/data[@name='BSSL2']"/>
          <xsl:with-param name="pSaleIdent" select ="$pRow/data[@name='BSSLI2']"/>
          <xsl:with-param name="pCoeff" select ="$pRow/data[@name='BSSLC2']"/>
        </xsl:call-template>
        <xsl:call-template name="XMLSale">
          <xsl:with-param name="pSale" select ="$pRow/data[@name='BSSL3']"/>
          <xsl:with-param name="pSaleIdent" select ="$pRow/data[@name='BSSLI3']"/>
          <xsl:with-param name="pCoeff" select ="$pRow/data[@name='BSSLC3']"/>
        </xsl:call-template>
        <xsl:call-template name="XMLSale">
          <xsl:with-param name="pSale" select ="$pRow/data[@name='BSSL4']"/>
          <xsl:with-param name="pSaleIdent" select ="$pRow/data[@name='BSSLI4']"/>
          <xsl:with-param name="pCoeff" select ="$pRow/data[@name='BSSLC4']"/>
        </xsl:call-template>
        <xsl:call-template name="XMLSale">
          <xsl:with-param name="pSale" select ="$pRow/data[@name='BSSL5']"/>
          <xsl:with-param name="pSaleIdent" select ="$pRow/data[@name='BSSLI5']"/>
          <xsl:with-param name="pCoeff" select ="$pRow/data[@name='BSSLC5']"/>
        </xsl:call-template>
      </sales>
    </xsl:variable>
    <xsl:for-each select="msxsl:node-set($vXMLBuyerOrSellerSales)/sales/sale">
      <xsl:variable name ="vSalePosition" select="position()"/>
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('tradeHeader_party1_sales',$vSalePosition,'_identifier')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pXMLDynamicValue">
          <xsl:call-template name="SQLTraderSales">
            <xsl:with-param name="pValue" >
              <xsl:value-of select="text()"/>
            </xsl:with-param>
            <xsl:with-param name="pValueIdent">
              <xsl:value-of select="@ident"/>
            </xsl:with-param>
          </xsl:call-template>
        </xsl:with-param>
      </xsl:call-template>
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('tradeHeader_party1_sales',$vSalePosition,'_factor')"/>
        <xsl:with-param name="pDataType" select ="'decimal'"/>
        <xsl:with-param name="pValue" select="@coeff"/>
      </xsl:call-template>
    </xsl:for-each>
    
    <!-- FI 20171103 [23326] Call GetCcis_EMIR_MiFIR -->
    <xsl:call-template name ="GetCcis_EMIR_MiFIR">
      <xsl:with-param name ="pTradePartiesAndBrokersNode" select="$pTradePartiesAndBrokersNode"/>
      <xsl:with-param name ="pBuyerSeller" select="$ConstBuyerOrSeller"/>
    </xsl:call-template>

    <!-- NegociationBroker (actor) -->
    <xsl:if test ="$pTradePartiesAndBrokersNode/party[@name=$ConstNegociationBroker]">
      <xsl:variable name ="vParty" select ="$pTradePartiesAndBrokersNode/party[@name=$ConstNegociationBroker]"/>
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="'tradeHeader_party1_broker1_actor'"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pDynamicValue" select ="$vParty/dynamicValue"/>
        <xsl:with-param name="pIsMandatory" select="$ConstTrue"/>
      </xsl:call-template>
    </xsl:if>
    <!-- NegociationBroker (frontId) -->
    <xsl:if test ="$pTradePartiesAndBrokersNode/frontId[@name=$ConstNegociationBroker]">
      <xsl:variable name ="vFront" select ="$pTradePartiesAndBrokersNode/frontId[@name=$ConstNegociationBroker]"/>
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="'tradeHeader_party1_broker1_frontId'"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pDynamicValue" select ="$vFront/dynamicValue"/>
      </xsl:call-template>
    </xsl:if>
    <!-- NegociationBroker (trader) -->
    <xsl:if test ="$pTradePartiesAndBrokersNode/trader[@name=$ConstNegociationBroker]">
      <xsl:variable name ="vTrader" select ="$pTradePartiesAndBrokersNode/trader[@name=$ConstNegociationBroker]"/>
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="'tradeHeader_party1_broker1_trader1_identifier'"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pDynamicValue" select ="$vTrader/dynamicValue"/>
        <xsl:with-param name="pIsAutoCreate" select="$vTrader/@isAutoCreate"/>
        <xsl:with-param name="pIsMissingMode" select="$isAllowDataMissing"/>
        <xsl:with-param name="pIsMissingModeSpecified" select="$ConstTrue" />
      </xsl:call-template>
    </xsl:if>

    <!-- Côté Clearing -->
    <!-- Clearing (actor) -->
    <xsl:if test ="$pTradePartiesAndBrokersNode/party[@name=$ConstClearingOrganization]">
      <xsl:variable name ="vParty" select ="$pTradePartiesAndBrokersNode/party[@name=$ConstClearingOrganization]"/>
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="'tradeHeader_party2_actor'"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pDynamicValue" select ="$vParty/dynamicValue"/>
        <xsl:with-param name="pIsMandatory" select="$ConstTrue"/>
      </xsl:call-template>
    </xsl:if>
    <!-- Clearing (book) -->
    <xsl:if test ="$pTradePartiesAndBrokersNode/book[@name=$ConstClearingOrganization]">
      <xsl:variable name ="vBook" select ="$pTradePartiesAndBrokersNode/book[@name=$ConstClearingOrganization]"/>
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="'tradeHeader_party2_book'"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pDynamicValue" select ="$vBook/dynamicValue"/>
        <xsl:with-param name="pIsMandatory" select="$ConstTrue"/>
      </xsl:call-template>
    </xsl:if>

    <!-- RD 20171227 [23684] Call GetCcis_EMIR_MiFIR -->
    <xsl:call-template name ="GetCcis_EMIR_MiFIR">
      <xsl:with-param name ="pTradePartiesAndBrokersNode" select="$pTradePartiesAndBrokersNode"/>
      <xsl:with-param name ="pBuyerSeller" select="$ConstClearingOrganization"/>
    </xsl:call-template>
    
    <!-- Clearing (ExecutingBroker/ ClearingBroker) -->
    <!-- Utilisation de la méthode Muenchian pour obtenir un select distinct (voir $/Spheres/Tools_XSLT/xslt_MuenchianMethod/xslt_MuenchianMethod/TradeImportSample)-->
    <xsl:variable name="vName_List" select="$pTradePartiesAndBrokersNode/child::node()
                  [@name=$ConstBuyerNegociationBroker or @name=$ConstExecutingBroker or @name=$ConstClearingBroker]
                  [generate-id()=generate-id(key('kTradePartiesAndBrokersNodeName',@name)[1])]"/>
    <xsl:for-each select ="$vName_List">
      <xsl:variable name="vBrokerPosition" select="position()"/>
      <xsl:variable name="vBrokerName" select="current()/@name"/>
      <!-- Broker (actor) -->
      <xsl:if test ="$pTradePartiesAndBrokersNode/party[@name=$vBrokerName]">
        <xsl:variable name ="vParty" select ="$pTradePartiesAndBrokersNode/party[@name=$vBrokerName]"/>
        <xsl:call-template name="CustomCaptureInfo">
          <xsl:with-param name="pClientId" select ="concat('tradeHeader_party2_broker',$vBrokerPosition,'_actor')"/>
          <xsl:with-param name="pDataType" select ="'string'"/>
          <xsl:with-param name="pDynamicValue" select ="$vParty/dynamicValue"/>
          <xsl:with-param name="pIsMandatory" select="$ConstTrue"/>
        </xsl:call-template>
      </xsl:if>
      <!-- Broker (book) -->
      <xsl:if test ="$pTradePartiesAndBrokersNode/book[@name=$vBrokerName]">
        <xsl:variable name ="vBook" select ="$pTradePartiesAndBrokersNode/book[@name=$vBrokerName]"/>
        <xsl:call-template name="CustomCaptureInfo">
          <xsl:with-param name="pClientId" select ="concat('tradeHeader_party2_broker',$vBrokerPosition,'_book')"/>
          <xsl:with-param name="pDataType" select ="'string'"/>
          <xsl:with-param name="pDynamicValue" select ="$vBook/dynamicValue"/>
        </xsl:call-template>
      </xsl:if>
      <!-- Broker (frontId) -->
      <xsl:if test ="$pTradePartiesAndBrokersNode/frontId[@name=$vBrokerName]">
        <xsl:variable name ="vFront" select ="$pTradePartiesAndBrokersNode/frontId[@name=$vBrokerName]"/>
        <xsl:call-template name="CustomCaptureInfo">
          <xsl:with-param name="pClientId" select ="concat('tradeHeader_party2_broker',$vBrokerPosition,'_frontId')"/>
          <xsl:with-param name="pDataType" select ="'string'"/>
          <xsl:with-param name="pDynamicValue" select ="$vFront/dynamicValue"/>
        </xsl:call-template>
      </xsl:if>
      <!-- Broker (trader) -->
      <xsl:if test ="$pTradePartiesAndBrokersNode/trader[@name=$vBrokerName]">
        <xsl:variable name ="vTrader" select ="$pTradePartiesAndBrokersNode/trader[@name=$vBrokerName]"/>
        <xsl:call-template name="CustomCaptureInfo">
          <xsl:with-param name="pClientId" select ="concat('tradeHeader_party2_broker',$vBrokerPosition,'_trader1_identifier')"/>
          <xsl:with-param name="pDataType" select ="'string'"/>
          <xsl:with-param name="pDynamicValue" select ="$vTrader/dynamicValue"/>
          <xsl:with-param name="pIsAutoCreate" select="$vTrader/@isAutoCreate"/>
          <xsl:with-param name="pIsMissingMode" select="$isAllowDataMissing"/>
          <xsl:with-param name="pIsMissingModeSpecified" select="$ConstTrue" />
        </xsl:call-template>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <!-- FI 20170718 [23326] Add Template
  Génération des ccis spécifiques aux parties (Buyer, buyer Negociation Broker, buyer Executing Broker,..., seller,..... )
  à partir de la variable pTradePartiesAndBrokersNode
  -->
  <xsl:template name ="GetCcisTradePartiesExecution">
    <xsl:param name ="pRow"/>
    <xsl:param name ="pTradePartiesAndBrokersNode"/>
    <!-- Côté Buyer (Party1) -->
    <xsl:call-template name ="GetCcisTradePartyExecution">
      <xsl:with-param name ="pRow" select ="$pRow"/>
      <xsl:with-param name ="pTradePartiesAndBrokersNode" select="$pTradePartiesAndBrokersNode"/>
      <xsl:with-param name ="pBuyerSeller" select="$ConstBuyer"/>
    </xsl:call-template>
    <!-- Côté Seller (Party2) -->
    <xsl:call-template name ="GetCcisTradePartyExecution">
      <xsl:with-param name ="pRow" select ="$pRow"/>
      <xsl:with-param name ="pTradePartiesAndBrokersNode" select="$pTradePartiesAndBrokersNode"/>
      <xsl:with-param name ="pBuyerSeller" select="$ConstSeller"/>
    </xsl:call-template>
  </xsl:template>

  <!-- FI 20170718 [23326] Add Template
  Génération des ccis spécifiques à une partie à partir de la variable pTradePartiesAndBrokersNode
  -->
  <xsl:template name ="GetCcisTradePartyExecution">
    <xsl:param name ="pRow"/>
    <xsl:param name ="pTradePartiesAndBrokersNode"/>
    <!-- Valeurs possible $ConstBuyer ou $ConstSeller -->
    <xsl:param name ="pBuyerSeller"/>

    <xsl:variable name ="vParty">
      <xsl:choose>
        <xsl:when test ="$pBuyerSeller = $ConstBuyer">
          <xsl:value-of select ="'party1'"/>
        </xsl:when>
        <xsl:when test ="$pBuyerSeller = $ConstSeller">
          <xsl:value-of select ="'party2'"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name ="vBYSL">
      <xsl:choose>
        <xsl:when test ="$pBuyerSeller = $ConstBuyer">
          <xsl:value-of select ="'BY'"/>
        </xsl:when>
        <xsl:when test ="$pBuyerSeller = $ConstSeller">
          <xsl:value-of select ="'SL'"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name ="isAllowDataMissDealer">
      <xsl:call-template name="GetIsAllowDataMissDealer">
        <xsl:with-param name="pRow" select="."/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="isAllowDataMissing">
      <xsl:call-template name="GetIsAllowDataMissing">
        <xsl:with-param name="pRow" select="."/>
      </xsl:call-template>
    </xsl:variable>

    <!-- Buyer/seller (actor) -->
    <xsl:if test ="$pTradePartiesAndBrokersNode/party[@name=$pBuyerSeller]">
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('tradeHeader_',$vParty,'_actor')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pIsMissingMode" select="$isAllowDataMissDealer"/>
        <xsl:with-param name="pIsMissingModeSpecified" select="$ConstTrue"/>
        <xsl:with-param name="pDynamicValue" select ="$pTradePartiesAndBrokersNode/party[@name=$pBuyerSeller]/dynamicValue"/>
        <xsl:with-param name="pIsMandatory" select="$ConstTrue"/>
      </xsl:call-template>
    </xsl:if>
    <!-- Buyer/seller (book) -->
    <xsl:if test ="$pTradePartiesAndBrokersNode/book[@name=$pBuyerSeller]">
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('tradeHeader_',$vParty,'_book')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pIsMissingMode" select="$isAllowDataMissDealer"/>
        <xsl:with-param name="pIsMissingModeSpecified" select="$ConstTrue" />
        <xsl:with-param name="pDynamicValue" select ="$pTradePartiesAndBrokersNode/book[@name=$pBuyerSeller]/dynamicValue"/>
        <xsl:with-param name="pIsMandatory" select="$ConstTrue"/>
      </xsl:call-template>
    </xsl:if>
    <!-- Buyer/seller (frontId) -->
    <xsl:if test ="$pTradePartiesAndBrokersNode/frontId[@name=$pBuyerSeller]">
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('tradeHeader_',$vParty,'_frontId')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pDynamicValue" select ="$pTradePartiesAndBrokersNode/frontId[@name=$pBuyerSeller]/dynamicValue"/>
      </xsl:call-template>
    </xsl:if>
    <!-- Buyer/seller (folder) -->
    <xsl:if test ="$pTradePartiesAndBrokersNode/folder[@name=$pBuyerSeller]">
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('tradeHeader_',$vParty,'_folder')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pDynamicValue" select ="$pTradePartiesAndBrokersNode/folder[@name=$pBuyerSeller]/dynamicValue"/>
      </xsl:call-template>
    </xsl:if>
    <!-- Buyer/seller (trader) -->
    <xsl:if test ="$pTradePartiesAndBrokersNode/trader[@name=$pBuyerSeller]">
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('tradeHeader_',$vParty,'_trader1_identifier')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pDynamicValue" select ="$pTradePartiesAndBrokersNode/trader[@name=$pBuyerSeller]/dynamicValue"/>
        <xsl:with-param name="pIsAutoCreate" select ="$pTradePartiesAndBrokersNode/trader[@name=$pBuyerSeller]/@isAutoCreate"/>
        <xsl:with-param name="pIsMissingMode" select="$isAllowDataMissing"/>
        <xsl:with-param name="pIsMissingModeSpecified" select="$ConstTrue" />
      </xsl:call-template>
    </xsl:if>
    <!-- Buyer/seller (sales) -->
    <xsl:variable name ="vXMLBuyerSales">
      <sales>
        <xsl:call-template name="XMLSale">
          <xsl:with-param name="pSale" select ="$pRow/data[@name=concat($vBYSL,'SL1')]"/>
          <xsl:with-param name="pSaleIdent" select ="$pRow/data[@name=concat($vBYSL,'SLI1')]"/>
          <xsl:with-param name="pCoeff" select ="$pRow/data[@name=concat($vBYSL,'SLC1')]"/>
        </xsl:call-template>
        <xsl:call-template name="XMLSale">
          <xsl:with-param name="pSale" select ="$pRow/data[@name=concat($vBYSL,'SL2')]"/>
          <xsl:with-param name="pSaleIdent" select ="$pRow/data[@name=concat($vBYSL,'SLI2')]"/>
          <xsl:with-param name="pCoeff" select ="$pRow/data[@name=concat($vBYSL,'SLC2')]"/>
        </xsl:call-template>
        <xsl:call-template name="XMLSale">
          <xsl:with-param name="pSale" select ="$pRow/data[@name=concat($vBYSL,'SL3')]"/>
          <xsl:with-param name="pSaleIdent" select ="$pRow/data[@name=concat($vBYSL,'SLI3')]"/>
          <xsl:with-param name="pCoeff" select ="$pRow/data[@name=concat($vBYSL,'SLC3')]"/>
        </xsl:call-template>
        <xsl:call-template name="XMLSale">
          <xsl:with-param name="pSale" select ="$pRow/data[@name=concat($vBYSL,'SL4')]"/>
          <xsl:with-param name="pSaleIdent" select ="$pRow/data[@name=concat($vBYSL,'SLI4')]"/>
          <xsl:with-param name="pCoeff" select ="$pRow/data[@name=concat($vBYSL,'SLC4')]"/>
        </xsl:call-template>
        <xsl:call-template name="XMLSale">
          <xsl:with-param name="pSale" select ="$pRow/data[@name=concat($vBYSL,'SL5')]"/>
          <xsl:with-param name="pSaleIdent" select ="$pRow/data[@name=concat($vBYSL,'SLI5')]"/>
          <xsl:with-param name="pCoeff" select ="$pRow/data[@name=concat($vBYSL,'SLC5')]"/>
        </xsl:call-template>
      </sales>
    </xsl:variable>
    <xsl:for-each select="msxsl:node-set($vXMLBuyerSales)/sales/sale">
      <xsl:variable name ="vSalePosition" select="position()"/>
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('tradeHeader_',$vParty,'_sales',$vSalePosition,'_identifier')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pXMLDynamicValue">
          <xsl:call-template name="SQLTraderSales">
            <xsl:with-param name="pValue" >
              <xsl:value-of select="text()"/>
            </xsl:with-param>
            <xsl:with-param name="pValueIdent">
              <xsl:value-of select="@ident"/>
            </xsl:with-param>
          </xsl:call-template>
        </xsl:with-param>
      </xsl:call-template>
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('tradeHeader_',$vParty,'_sales',$vSalePosition,'_factor')"/>
        <xsl:with-param name="pDataType" select ="'decimal'"/>
        <xsl:with-param name="pValue" select="@coeff"/>
      </xsl:call-template>
    </xsl:for-each>

    <!-- FI 20171103 [23326] -->
    <xsl:call-template name ="GetCcis_EMIR_MiFIR">
      <xsl:with-param name ="pTradePartiesAndBrokersNode" select="$pTradePartiesAndBrokersNode"/>
      <xsl:with-param name ="pBuyerSeller" select="$pBuyerSeller"/>
    </xsl:call-template>
    
    
    <!-- Buyer/seller (Brokers) -->
    <!-- Utilisation de la méthode Muenchian pour obtenir un select distinct (voir $/Spheres/Tools_XSLT/xslt_MuenchianMethod/xslt_MuenchianMethod/TradeImportSample)-->
    <xsl:variable name="vName_List" select="$pTradePartiesAndBrokersNode/child::node()
                  [@name=concat($pBuyerSeller,'NegociationBroker') or @name=concat($pBuyerSeller,'ExecutingBroker') or @name=concat($pBuyerSeller,'ClearingBroker')]
                  [generate-id()=generate-id(key('kTradePartiesAndBrokersNodeName',@name)[1])]"/>
    <xsl:for-each select ="$vName_List">
      <xsl:variable name="vBrokerPosition" select="position()"/>
      <xsl:variable name="vBrokerName" select="current()/@name"/>
      <!-- NegociationBroker (actor) -->
      <xsl:if test ="$pTradePartiesAndBrokersNode/party[@name=$vBrokerName]">
        <xsl:call-template name="CustomCaptureInfo">
          <xsl:with-param name="pClientId" select ="concat('tradeHeader_',$vParty,'_broker',$vBrokerPosition,'_actor')"/>
          <xsl:with-param name="pDataType" select ="'string'"/>
          <xsl:with-param name="pDynamicValue" select ="$pTradePartiesAndBrokersNode/party[@name=$vBrokerName]/dynamicValue"/>
          <xsl:with-param name="pIsMandatory" select="$ConstTrue"/>
        </xsl:call-template>
      </xsl:if>
      <!-- NegociationBroker (frontId) -->
      <xsl:if test ="$pTradePartiesAndBrokersNode/frontId[@name=$vBrokerName]">
        <xsl:call-template name="CustomCaptureInfo">
          <xsl:with-param name="pClientId" select ="concat('tradeHeader_',$vParty,'_broker',$vBrokerPosition,'_frontId')"/>
          <xsl:with-param name="pDataType" select ="'string'"/>
          <xsl:with-param name="pDynamicValue" select ="$pTradePartiesAndBrokersNode/frontId[@name=$vBrokerName]/dynamicValue"/>
        </xsl:call-template>
      </xsl:if>
      <!-- NegociationBroker (trader) -->
      <xsl:if test ="$pTradePartiesAndBrokersNode/trader[@name=$vBrokerName]">
        <xsl:call-template name="CustomCaptureInfo">
          <xsl:with-param name="pClientId" select ="concat('tradeHeader_',$vParty,'_broker',$vBrokerPosition,'_trader1_identifier')"/>
          <xsl:with-param name="pDataType" select ="'string'"/>
          <xsl:with-param name="pDynamicValue" select ="$pTradePartiesAndBrokersNode/trader[@name=$vBrokerName]/dynamicValue"/>
          <xsl:with-param name="pIsAutoCreate" select ="$pTradePartiesAndBrokersNode/trader[@name=$vBrokerName]/@isAutoCreate"/>
          <xsl:with-param name="pIsMissingMode" select ="$isAllowDataMissing"/>
          <xsl:with-param name="pIsMissingModeSpecified" select="$ConstTrue" />
        </xsl:call-template>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <!-- FI 20170718 [23326] Add Template -->
  <xsl:template name ="SQLDealer">
    <xsl:param name ="pBuyerOrSellerActorNode"/>
    <xsl:param name ="pBuyerOrSellerActorIdentNode"/>
    <xsl:param name ="pBuyerOrSellerAccountNode"/>
    <xsl:param name ="pBuyerOrSellerAccountIdentNode"/>
    <xsl:param name ="pRecordVersion"/>
    <xsl:param name ="pIsAllowDataMissDealer"/>

    <xsl:choose>
      <xsl:when test ="$pRecordVersion='v1'">
        <!-- Recherche du delaler à partir du book -->
        <xsl:call-template name="ovrSQLDealerActorAndBook">
          <xsl:with-param name="pBuyerOrSellerDealer" select="$pBuyerOrSellerActorNode/text()"/>
          <xsl:with-param name="pBuyerOrSellerDealerIdent" select="$pBuyerOrSellerActorIdentNode/text()"/>
          <xsl:with-param name="pBuyerOrSellerAccount" select="$pBuyerOrSellerAccountNode/text()"/>
          <xsl:with-param name="pBuyerOrSellerAccountIdent" select="$pBuyerOrSellerAccountIdentNode/text()"/>
          <xsl:with-param name="pResultColumn" select="'IDENTIFIER'"/>
          <xsl:with-param name="pIsAllowDataMissDealer" select ="$pIsAllowDataMissDealer"/>
          <xsl:with-param name="pDefaultValue" select ="$ConstActorSYSTEM"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <!-- FI 20170404 [23039] Recherche du dealer à partir du vBuyerOrSeller lorsqu'il est renseigné -->
          <xsl:when test ="$pBuyerOrSellerActorNode and string-length(normalize-space($pBuyerOrSellerActorNode))>0">
            <xsl:call-template name="ovrSQLDealerActor">
              <xsl:with-param name="pBuyerOrSeller" select="$pBuyerOrSellerActorNode/text()"/>
              <xsl:with-param name="pBuyerOrSellerIdent" select="$pBuyerOrSellerActorIdentNode/text()"/>
              <xsl:with-param name="pResultColumn" select="'IDENTIFIER'"/>
              <xsl:with-param name="pIsAllowDataMissDealer" select ="$pIsAllowDataMissDealer"/>
              <xsl:with-param name="pDefaultValue" select ="$ConstActorSYSTEM"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <!-- Recherche du dealer à partir du book -->
            <xsl:call-template name="ovrSQLDealerActorAndBook">
              <xsl:with-param name="pBuyerOrSellerDealer" select="$pBuyerOrSellerActorNode/text()"/>
              <xsl:with-param name="pBuyerOrSellerDealerIdent" select="$pBuyerOrSellerActorIdentNode/text()"/>
              <xsl:with-param name="pBuyerOrSellerAccount" select="$pBuyerOrSellerAccountNode/text()"/>
              <xsl:with-param name="pBuyerOrSellerAccountIdent" select="$pBuyerOrSellerAccountIdentNode/text()"/>
              <xsl:with-param name="pResultColumn" select="'IDENTIFIER'"/>
              <xsl:with-param name="pIsAllowDataMissDealer" select ="$pIsAllowDataMissDealer"/>
              <xsl:with-param name="pDefaultValue" select ="$ConstActorSYSTEM"/>
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- FI 20171103 [23326] Add 
    ajoute les ccis nécessaires à EMIR/ MiFIR -->
  <xsl:template name ="GetCcis_EMIR_MiFIR">
    <xsl:param name ="pTradePartiesAndBrokersNode"/>
    <!-- Valeurs possible $ConstBuyer ou $ConstSeller -->
    <xsl:param name ="pBuyerSeller"/>

    <xsl:variable name ="vParty">
      <xsl:choose>
        <xsl:when test ="$pBuyerSeller = $ConstBuyer or $pBuyerSeller = $ConstBuyerOrSeller ">
          <xsl:value-of select ="'party1'"/>
        </xsl:when>
        <xsl:when test ="$pBuyerSeller = $ConstSeller or $pBuyerSeller = $ConstClearingOrganization">
          <xsl:value-of select ="'party2'"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <!-- EMIR/UTI -->
    <xsl:if test ="$pTradePartiesAndBrokersNode/uti[@name=$pBuyerSeller]">
      <xsl:variable name ="vUTI" select ="$pTradePartiesAndBrokersNode/uti[@name=$pBuyerSeller]"/>
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('tradeHeader_',$vParty,'_partyTradeIdentifier_UTI_value')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pDynamicValue" select ="$vUTI/dynamicValue"/>
      </xsl:call-template>
    </xsl:if>

    <!-- MiFIR/investmentDecisionWithinFirm -->
    <xsl:if test ="$pTradePartiesAndBrokersNode/investmentDecisionWithinFirm[@name=$pBuyerSeller]">
      <xsl:variable name ="vInvestmentDecisionWithinFirm" select ="$pTradePartiesAndBrokersNode/investmentDecisionWithinFirm[@name=$pBuyerSeller]"/>
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('tradeHeader_',$vParty,'_partyTradeInformation_investmentDecisionWithinFirm')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pDynamicValue" select ="$vInvestmentDecisionWithinFirm/dynamicValue"/>
      </xsl:call-template>
    </xsl:if>

    <!-- MiFIR/executionWithinFirm -->
    <xsl:if test ="$pTradePartiesAndBrokersNode/executionWithinFirm[@name=$pBuyerSeller]">
      <xsl:variable name ="vExecutionWithinFirm" select ="$pTradePartiesAndBrokersNode/executionWithinFirm[@name=$pBuyerSeller]"/>
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('tradeHeader_',$vParty,'_partyTradeInformation_executionWithinFirm')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pDynamicValue" select ="$vExecutionWithinFirm/dynamicValue"/>
      </xsl:call-template>
    </xsl:if>

    <!-- MiFIR/tradingCapacity -->
    <xsl:if test ="$pTradePartiesAndBrokersNode/tradingCapacity[@name=$pBuyerSeller]">
      <xsl:variable name ="vtradingCapacity" select ="$pTradePartiesAndBrokersNode/tradingCapacity[@name=$pBuyerSeller]"/>
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('tradeHeader_',$vParty,'_partyTradeInformation_tradingCapacity')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pDynamicValue" select ="$vtradingCapacity/dynamicValue"/>
      </xsl:call-template>
    </xsl:if>

    <!-- MiFIR/tradingWaiver -->
    <xsl:if test ="$pTradePartiesAndBrokersNode/waiverIndicator[@name=$pBuyerSeller]">
      <xsl:variable name ="vWaiverIndicator" select ="$pTradePartiesAndBrokersNode/waiverIndicator[@name=$pBuyerSeller]"/>
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('tradeHeader_',$vParty,'_partyTradeInformation_tradingWaiver1')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pDynamicValue" select ="$vWaiverIndicator/dynamicValue"/>
      </xsl:call-template>
    </xsl:if>

    <!-- MiFIR/shortSailingIndicator -->
    <xsl:if test ="$pTradePartiesAndBrokersNode/waiverIndicator[@name=$pBuyerSeller]">
      <xsl:variable name ="vShortSailingIndicator" select ="$pTradePartiesAndBrokersNode/shortSailingIndicator[@name=$pBuyerSeller]"/>
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('tradeHeader_',$vParty,'_partyTradeInformation_shortSale')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pDynamicValue" select ="$vShortSailingIndicator/dynamicValue"/>
      </xsl:call-template>
    </xsl:if>

    <!-- MiFIR/OTCPostTradeIndicator -->
    <xsl:if test ="$pTradePartiesAndBrokersNode/otcPostTradeIndicator[@name=$pBuyerSeller]">
      <xsl:variable name ="votcPostTradeIndicator" select ="$pTradePartiesAndBrokersNode/otcPostTradeIndicator[@name=$pBuyerSeller]"/>
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('tradeHeader_',$vParty,'_partyTradeInformation_otcClassification1')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pDynamicValue" select ="$votcPostTradeIndicator/dynamicValue"/>
      </xsl:call-template>
    </xsl:if>

    <!-- MiFIR/commodityDerivativeIndicator -->
    <xsl:if test ="$pTradePartiesAndBrokersNode/commodityDerivativeIndicator[@name=$pBuyerSeller]">
      <xsl:variable name ="vcommodityDerivativeIndicator" select ="$pTradePartiesAndBrokersNode/commodityDerivativeIndicator[@name=$pBuyerSeller]"/>
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('tradeHeader_',$vParty,'_partyTradeInformation_isCommodityHedge')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pDynamicValue" select ="$vcommodityDerivativeIndicator/dynamicValue"/>
      </xsl:call-template>
    </xsl:if>

    <!-- MiFIR/commodityDerivativeIndicator -->
    <xsl:if test ="$pTradePartiesAndBrokersNode/commodityDerivativeIndicator[@name=$pBuyerSeller]">
      <xsl:variable name ="vSecuritiesFinancing" select ="$pTradePartiesAndBrokersNode/securitiesFinancing[@name=$pBuyerSeller]"/>
      <xsl:call-template name="CustomCaptureInfo">
        <xsl:with-param name="pClientId" select ="concat('tradeHeader_',$vParty,'_partyTradeInformation_isSecuritiesFinancing')"/>
        <xsl:with-param name="pDataType" select ="'string'"/>
        <xsl:with-param name="pDynamicValue" select ="$vSecuritiesFinancing/dynamicValue"/>
      </xsl:call-template>
    </xsl:if>
    
  </xsl:template>
  
  <!-- FI 20171103 [23326] Add
  Convertie un horodatage en UTC-->
  <xsl:template name="GetDynamicDataConvertTimestampToUtc">
    <xsl:param name="pTimestamp"/>
    <xsl:param name="pDynamicDataTimeZone"/>
    <dynamicValue>
      <SpheresLib function="ConvertToUTC()">
        <Param name="TIMESTAMP" datatype="datetime" dataformat="yyyy-MM-ddTHH:mm:ss.FFFFFF">
          <xsl:value-of select="$pTimestamp"/>
        </Param>
        <Param name="TIMEZONE" datatype="string">
          <xsl:copy-of select="msxsl:node-set($pDynamicDataTimeZone)/dynamicValue/child::node()"/>
        </Param>
      </SpheresLib>
    </dynamicValue>
  </xsl:template>

  <!-- FI 20171103 [23326] Add
  Détermination du facility à partir du market (règle en dur à amender si nécessaire) -->
  <xsl:template name ="GetSQLFacilityFromMarket">
    <xsl:param name="pResultColumn" select="'FACILITY'"/>
    <xsl:param name="pDynamicDataMarketIdentifier"/>

    <xsl:variable name ="vDynamicDataMarketIdentifier" select="msxsl:node-set($pDynamicDataMarketIdentifier)"/>
    <SQL command="select" result="{$pResultColumn}" cache="true">
      <xsl:text>
      <![CDATA[
        select case when m.FIXML_SECURITYEXCHANGE in ('XEUR-XHEX','XEEE') then 'XEUR'
                    when m.FIXML_SECURITYEXCHANGE in ('XDMI-5','XDMI-8') then 'XDMI'
                    else m.IDENTIFIER 
                    end as FACILITY
        from dbo.VW_MARKET_IDENTIFIER m
        where m.FIXML_SECURITYEXCHANGE=@MARKETIDENTIFIER
      ]]>
      </xsl:text>
      <Param datatype="string" name="MARKETIDENTIFIER">
        <xsl:copy-of select="$vDynamicDataMarketIdentifier/dynamicValue/child::node()"/>
      </Param>
    </SQL>
  </xsl:template>
  
  <!-- FI 20171103 [23326] Add
  Détermination du time zone à partir du facility  -->
  <xsl:template name="GetDynamicDataTimeZoneV1">
    <xsl:param name="pDynamicDataFacilityIdentifier"/>

    <xsl:variable name ="vDynamicDataFacilityIdentifier" select="msxsl:node-set($pDynamicDataFacilityIdentifier)"/>
    <dynamicValue>
      <SQL command="select" result="TIMEZONE" cache="true">
        <xsl:text>
      <![CDATA[
        select isnull(TIMEZONE,'Etc/UTC') as TIMEZONE, 0 as SORT
        from dbo.VW_MARKET_IDENTIFIER m
        where m.FIXML_SECURITYEXCHANGE=@MARKETIDENTIFIER
        union
        select 'Etc/UTC'  as TIMEZONE , 1 as SORT
        from DUAL
        order by SORT ASC
      ]]>
      </xsl:text>
        <Param datatype="string" name="MARKETIDENTIFIER">
          <xsl:copy-of select="$vDynamicDataFacilityIdentifier/dynamicValue/child::node()"/>
        </Param>
      </SQL>
    </dynamicValue>
  </xsl:template>

  <!-- FI 20171103 [23326] Add 
  Lecture du timezone en entrée (Retourne 'Etc/UTC' si timezone non renseigné -->
  <xsl:template name ="GetDynamicDataTimeZoneV2">
    <!-- ODTTZ (pour OrderEnteredTimeZone) or EDTTZ (pour ExecutionTimeZone) -->
    <xsl:param name ="pName"/>
    <dynamicValue>
      <xsl:choose>
        <xsl:when test ="string-length(data[@name=$pName])>0">
          <xsl:value-of select ="data[@name=$pName]"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="'Etc/UTC'"/>
        </xsl:otherwise>
      </xsl:choose>
    </dynamicValue>
  </xsl:template>

  <!-- FI 20240417 [26751] add template -->
  <xsl:template name ="NextMonth">
    <xsl:param name ="pYYYYMM"/>
    <!-- Extract year and month from the date -->
    <xsl:variable name="year" select="substring($pYYYYMM, 1, 4)" />
    <xsl:variable name="month" select="substring($pYYYYMM, 5, 2)" />

    <!-- Calculate the next month -->
    <xsl:variable name="nextMonth">
      <xsl:choose>
        <!-- If December, increment year and set month to January -->
        <xsl:when test="$month = 12">
          <xsl:value-of select="$year + 1" />
          <xsl:text>01</xsl:text>
        </xsl:when>
        <!-- Otherwise, increment month -->
        <xsl:otherwise>
          <xsl:value-of select="$year" />
          <!-- Add leading zero if necessary -->
          <xsl:choose>
            <xsl:when test="string-length($month + 1) = 1">
              <xsl:text>0</xsl:text>
            </xsl:when>
          </xsl:choose>
          <xsl:value-of select="$month + 1" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- Output the next month in YYYYMM format -->
    <xsl:value-of select="$nextMonth" />
  </xsl:template>
  
</xsl:stylesheet>