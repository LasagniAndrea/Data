<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt">

  <!--
=============================================================================================
Summary : PosRequest input - Spheres standard mapping
          Some templates of this xsl are overrided by:
          - specific BCS's xsl mapping files
          - specific customer's xsl mapping files
          
File    : PosRequestImport_Map.xsl    
=============================================================================================
Version : v12.0
Date    : 20220823
Author  : FI
Comment : [25699] Format V3 (like TradeImport_Map.xsl)
=============================================================================================
Version : v3.7.5158                                           
Date    : 20140324
Author  : RD
Comment : [19704] Use templates: ovrETD_SQLMarket and ovrSQLDerivativeContract
                  Instead of: ETD_SQLMarket and SQLDerivativeContract
=============================================================================================
Version : v3.7.5127                                           
Date    : 20131210
Author  : RD
Comment : [19382] Add 2 templates: ovrGetConditions and GetStandardConditions
=============================================================================================
Version : v3.3.4878                                           
Date    : 20130322 
Author  : FI
Comment : First version 
=============================================================================================
-->

  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml;"/>

  <!-- ================================================== -->
  <!--        include(s)                                  -->
  <!-- ================================================== -->
  <xsl:include href="..\Common\ImportTools.xsl"/>

  <!-- ================================================== -->
  <!--        Global Variables                            -->
  <!-- ================================================== -->
  <!-- TODO FI 20130328 [1867] comment savoir que l'on est sur un traitement déclenché par une gateway ? -->
  <xsl:variable name ="gIsModegateway" select ="false"/>

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
      <xsl:apply-templates select="row[@status='success' and data[@name='RTP']='B']"/>
    </file>
  </xsl:template>

  <xsl:template match="row">
    <!-- FI 20220823 [25699] Add vRecordVersion1 -->
    <xsl:variable name ="vRecordVersion1">
      <xsl:call-template name="GetRecordVersion">
        <xsl:with-param name="pRow" select="."/>
      </xsl:call-template>
    </xsl:variable>
    
    <xsl:variable name ="vId" select ="normalize-space(@id)"/>
    <xsl:variable name ="vSrc" select ="normalize-space(@src)"/>

    <!--vActionType-->
    <xsl:variable name ="vActionType" select ="normalize-space(data[@name='ATP'])"/>
    <xsl:variable name ="vActionTypeLog">
      <xsl:call-template name="GetActionTypeLog">
        <xsl:with-param name="pActionType" select="$vActionType"/>
      </xsl:call-template>
    </xsl:variable>

    <!--vPosRequestId-->
    <xsl:variable name="vPosRequestId">
      <xsl:choose>
        <xsl:when test="string-length(normalize-space(data[@name='PRID'])) = 0">
          <xsl:value-of select="$ConstNA"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="normalize-space(data[@name='PRID'])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!--vBusinessDate-->
    <!-- FI 20220823 [25699] vBusinessDate Remplace vClearingBusinessDate-->
    <xsl:variable name ="vBusinessDate" select ="normalize-space(data[@name='BDT' or @name='CDT'])"/>
    
    <!-- FI 20220823 [25699] Add vClearingHouse and vClearingHouseColumn (available only in v3 record) -->
    <!-- vClearingHouse contient la valeur qui identifie la chmabre de compensation -->
    <xsl:variable name="vClearingHouse" select ="normalize-space(data[@name='CLH'])"/>
    <xsl:variable name="vClearingHouseColumn">
      <xsl:call-template name="GetActorIdentColumn">
        <xsl:with-param name="pIdent" select="normalize-space(data[@name='CLHI'])"/>
      </xsl:call-template>
    </xsl:variable>
    
    <!-- vMarket contient la valeur qui identifie le marché -->
    <xsl:variable name="vMarket" select ="normalize-space(data[@name='MKT'])"/>
    <xsl:variable name="vMarketColumn">
      <xsl:call-template name="GetMarketIdentColumn">
        <xsl:with-param name="pIdent" select="normalize-space(data[@name='MKTI'])"/>
      </xsl:call-template>
    </xsl:variable>
    
    <xsl:variable name ="vProduct" select ="normalize-space(data[@name='PRD'])"/>
    <xsl:variable name ="vInstrument" select ="normalize-space(data[@name='INSTR' or @name='INS'])"/>
    <xsl:variable name ="vFamily">
      <xsl:choose>
        <xsl:when test="$vProduct='equitySecurityTransaction'">
          <xsl:value-of select="'ESE'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'ETD'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!--Asset-->
    <xsl:variable name ="vAssetCode" select ="normalize-space(data[@name='ASC'])"/>
    <xsl:variable name ="vAssetCodeColumn">
      <xsl:call-template name="GetAssetIdentColumn">
        <xsl:with-param name="pIdent" select ="normalize-space(data[@name='ASCI'])"/>
      </xsl:call-template>
    </xsl:variable>

    <!-- FI 20220823 [25699] add vContract, vContractColumn, vContractCategory, vContractType, vContractVersion , vContractSettltMethod, vContractExerciseStyle, vContractPutCall, vContractStrike, vContractMaturity -->
    <xsl:variable name ="vContract" select ="normalize-space(data[@name='DVT' or @name='CTR'])"/>
    <xsl:variable name ="vContractColumn">
      <xsl:call-template name="GetDerivativeContractIdentColumn">
        <xsl:with-param name="pIdent" select="normalize-space(data[@name='DVTI' or @name='CTRI'])"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name ="vContractCategory" select ="normalize-space(data[@name='DVTC' or @name='CTRC'])"/>
    <xsl:variable name="vContractType">
      <xsl:call-template name="GetContractType">
        <xsl:with-param name="pRow" select="." />
        <xsl:with-param name="pFamily" select ="$vFamily"/>
      </xsl:call-template>
    </xsl:variable>
    
    <xsl:variable name ="vContractVersion" select ="normalize-space(data[@name='DVTV' or @name='CTRV' ])"/>
    <xsl:variable name ="vContractSettltMethod" select ="normalize-space(data[@name='CTRSM'])"/>
    <xsl:variable name ="vContractExerciseStyle" select ="normalize-space(data[@name='CTRES'])"/>
    <xsl:variable name ="vContractPutCall">
      <xsl:choose>
        <xsl:when test ="$vRecordVersion1='v3'">
          <xsl:value-of select ="normalize-space(data[@name='CTRPC'])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="normalize-space(data[@name='DVTO'])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name ="vContractStrike" select ="normalize-space(data[@name='DVTS' or @name='CTRS'])"/>
    <xsl:variable name ="vContractMaturity" select ="normalize-space(data[@name='DVTM' or @name='CTRM'])"/>

    <!--Instrument-->
    <!-- L'instrument doit être en entrée (cas normal) et exigé si ESE -->
    <!-- s'il est non renseigné et si famile ETD, Spheres® récupère l'instrument à partir des caractéristiques de l'asset -->
    <xsl:variable name ="vInstr">
      <xsl:choose>
        <xsl:when test ="$vRecordVersion1='v1'">
          <xsl:choose>
            <!-- si Instrument est renseigné -->
            <xsl:when test="string-length($vInstrument) > 0">
              <xsl:value-of select="$vInstrument"/>
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
    <!-- FI 20220823 [25699] Add -->
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
    
    <!--vRequestType-->
    <xsl:variable name ="vRequestType" select ="normalize-space(data[@name='RQT'])"/>
    <!--vRequestMode-->
    <xsl:variable name ="vRequestMode" select ="normalize-space(data[@name='RQM'])"/>
    <!--vQty-->
    <xsl:variable name ="vQty" select ="normalize-space(data[@name='QTY'])"/>

    <!--vSpheresFeeCalculation-->
    <xsl:variable name ="vSpheresFeeCalculation">
      <xsl:call-template name="IsToApplyOrToIgnore">
        <xsl:with-param name="pApplyIgnore" select="normalize-space(data[@name='SFC'])"/>
      </xsl:call-template>
    </xsl:variable>

    <!--vSpheresPartialExecution-->
    <xsl:variable name ="vSpheresPartialExecution">
      <xsl:call-template name="IsToApplyOrToIgnore">
        <xsl:with-param name="pApplyIgnore" select="normalize-space(data[@name='SPE'])"/>
      </xsl:call-template>
    </xsl:variable>

    <!--vSpheresAbandonRemainingQty-->
    <xsl:variable name ="vSpheresAbandonRemainingQty">
      <xsl:call-template name="IsToApplyOrToIgnore">
        <xsl:with-param name="pApplyIgnore" select="normalize-space(data[@name='SARQ'])"/>
      </xsl:call-template>
    </xsl:variable>

    <!--vSpheresDescription-->
    <xsl:variable name ="vSpheresDescription" select ="normalize-space(data[@name='DSC'])"/>


    <!-- Dealer-->
    <xsl:variable name ="vBuyerOrSellerAccount" select ="data[@name='BSA']"/>
    <xsl:variable name ="vBuyerOrSellerAccountIdent" select ="data[@name='BSAI']"/>

    <!--Clearer-->
    <!-- FI 20220823 [25699] BSCO, BSCOI, BSCOA, BSCOAI -->
    <xsl:variable name ="vClearingOrganization" select ="data[@name='CO' or @name='BSCO']"/>
    <xsl:variable name ="vClearingOrganizationIdent" select ="data[@name='COI' or @name='BSCOI']"/>
    <xsl:variable name ="vClearingOrganizationAccount" select ="data[@name='COA' or @name='BSCOA']"/>
    <xsl:variable name ="vClearingOrganizationAccountIdent" select ="data[@name='COAI' or @name='BSCOAI']"/> 

    <!--DealerEntity-->
    <xsl:variable name ="vBuyerOrSellerEntity" select ="data[@name='BSE']"/>
    <xsl:variable name ="vBuyerOrSellerEntityIdent" select ="data[@name='BSEI']"/>

    <row id="{$vId}" src="{$vSrc}">
      <logInfo>
        <message>
          <xsl:value-of select ="concat('PosRequest Id: ',$vPosRequestId,', Action type: ',$vActionTypeLog)"/>
        </message>
      </logInfo>
      <!--///-->
      <posRequestImport xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                        xmlns:xsd="http://www.w3.org/2001/XMLSchema">
        <settings>
          <importMode>
            <xsl:choose>
              <xsl:when test ="$vPosRequestId = $ConstNA">
                <xsl:value-of select="'New'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name="SQLCheckPosRequest">
                  <xsl:with-param name="pActionType" select="$vActionType"/>
                  <xsl:with-param name="pPosRequestId" select="$vPosRequestId"/>
                  <xsl:with-param name="pResultColumn" select="'IMPORTMODE'"/>
                  <xsl:with-param name="pBusinessDate" select="$vBusinessDate"/>
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </importMode>
          <!-- ================================================== -->
          <!--                CONDITIONS SECTION                  -->
          <!-- ================================================== -->
          <conditions>
            <!-- FI 20220823 [25699] add pContractType, pContractSettltMethod, pContractExerciseStyle, pContractVersion  -->
            <xsl:call-template name="ovrGetConditions">
              <xsl:with-param name="pActionType" select="$vActionType"/>
              <xsl:with-param name="pRequestType" select="$vRequestType"/>
              <xsl:with-param name="pPosRequestId" select="$vPosRequestId"/>
              <xsl:with-param name="pClearingBusinessDate" select="$vBusinessDate"/>
              <xsl:with-param name="pAssetCode" select="$vAssetCode"/>
              <xsl:with-param name="pAssetColumn" select="$vAssetCodeColumn"/>
              <xsl:with-param name="pMarket" select="$vMarket"/>
              <xsl:with-param name="pMarketColumn" select="$vMarketColumn"/>
              <xsl:with-param name="pDC" select="$vContract"/>
              <xsl:with-param name="pDCColumn" select="$vContractColumn"/>
              <xsl:with-param name="pCategory" select="$vContractCategory"/>
              <xsl:with-param name="pMaturityMonthYear" select="$vContractMaturity"/>
              <xsl:with-param name="pStrikePrice" select="$vContractStrike"/>
              <xsl:with-param name="pPutCall" select="$vContractPutCall"/>
              <xsl:with-param name="pContractType" select="$vContractType"/>
              <xsl:with-param name="pContractSettltMethod" select="$vContractSettltMethod"/>
              <xsl:with-param name="pContractExerciseStyle" select="$vContractExerciseStyle"/>
              <xsl:with-param name="pContractVersion" select="$vContractVersion"/>
            </xsl:call-template>
          </conditions>
          <!-- ================================================== -->
          <!--                User SECTION                        -->
          <!-- ================================================== -->
          <user>SYSADM</user>
          <!-- ================================================== -->
          <!--                PARAMETERS SECTION                  -->
          <!-- ================================================== -->
          <parameters>
            <parameter name="http://www.efs.org/Spheres/posRequestImport/instrumentIdentifier" datatype="string">
              <xsl:choose>
                <!-- FI 20220823 [25699] comportement spécifique si version v1 
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
                  <!-- FI 20220823 [25699] template SQLInstr si version v3 -->
                  <xsl:call-template name="SQLInstr">
                    <xsl:with-param name="pInstr" select="$vInstr"/>
                    <xsl:with-param name="pInstrColumn" select="$vInstrColumn"/>
                    <xsl:with-param name="pIsMandatory" select="$ConstTrue"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </parameter>
            <parameter name="http://www.efs.org/Spheres/posRequestImport/extllink" datatype="string">
              <xsl:choose>
                <xsl:when test ="$vPosRequestId != $ConstNA">
                  <xsl:value-of select="$vPosRequestId"/>
                </xsl:when>
                <xsl:otherwise>
                </xsl:otherwise>
              </xsl:choose>
            </parameter>
            <xsl:choose>
              <xsl:when test ="$vPosRequestId != $ConstNA">
                <parameter name="http://www.efs.org/Spheres/posRequestImport/idPosRequest" datatype="int">
                  <xsl:call-template name="SQLPosRequestIdPR">
                    <xsl:with-param name="pPosRequestId" select="$vPosRequestId"/>
                    <xsl:with-param name="pBusinessDate" select="$vBusinessDate"/>
                  </xsl:call-template>
                </parameter>
              </xsl:when>
              <xsl:otherwise>
              </xsl:otherwise>
            </xsl:choose>
          </parameters>
        </settings>
        <posRequestInput>
          <!-- ================================================== -->
          <!--                CCI SECTION                         -->
          <!-- ================================================== -->

          <!-- FI 20190904 [24882] add variable vDynamicDataMarketIdentifier de type StringDynamicData 
             vDynamicDataMarketIdentifier permet d'obtenir le marché
            -->
          <!-- FI 20220823 [25699] alimentation des paramètres pContractType, pContractSettltMethod, pContractExerciseStyle, pClearingHouse, pClearingHouseColumn   -->
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

          <!-- FI 20190904 [24882] add variable vDynamicDataCssIdentifier de type StringDynamicData 
              vDynamicDataCssIdentifier permet d'obtenir le css à partir du marché 
            -->
          <xsl:variable name ="vDynamicDataCssIdentifier">
            <xsl:call-template name ="GetDynamicDataCssIdentifier">
              <xsl:with-param name="pDynamicDataMarketIdentifier">
                <xsl:copy-of select ="$vDynamicDataMarketIdentifier"/>
              </xsl:with-param>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name ="vXMLParties">
            <!--Dealer-->
            <xsl:variable name ="vBuyerOrSellerActor">
              <xsl:call-template name="SQLActorAndBook">
                <xsl:with-param name="pValue" select="$vBuyerOrSellerAccount"/>
                <xsl:with-param name="pValueIdent" select="$vBuyerOrSellerAccountIdent"/>
                <xsl:with-param name="pResultColumn" select="'IDENTIFIER'"/>
                <xsl:with-param name="pIsMandatory" select="'true'"/>
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name ="vBuyerOrSellerBook">
              <xsl:call-template name="SQLActorAndBook">
                <xsl:with-param name="pValue" select="$vBuyerOrSellerAccount"/>
                <xsl:with-param name="pValueIdent" select="$vBuyerOrSellerAccountIdent"/>
                <xsl:with-param name="pResultColumn" select="'BOOK_IDENTIFIER'"/>
              </xsl:call-template>
            </xsl:variable>

            <!--Clearer-->
            <xsl:variable name ="vClearingOrganizationActor">
              <xsl:choose>
                <!-- Si la ClearingOrganisation est spécifiée, on la charge-->
                <xsl:when test="string-length($vClearingOrganization) > 0 and string-length($vClearingOrganizationIdent) > 0">
                  <xsl:call-template name="SQLActor">
                    <xsl:with-param name="pValue" select="$vClearingOrganization"/>
                    <xsl:with-param name="pValueIdent" select="$vClearingOrganizationIdent"/>
                  </xsl:call-template>
                </xsl:when>
                <!-- Chargement de la ClearingOrganisation à partir du book-->
                <xsl:otherwise>
                  <xsl:call-template name="SQLActorAndBook">
                    <xsl:with-param name="pValue" select="$vClearingOrganizationAccount"/>
                    <xsl:with-param name="pValueIdent" select="$vClearingOrganizationAccountIdent"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
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
            <xsl:variable name ="vClearingOrganizationBook">
              <xsl:call-template name="SQLActorAndBook">
                <xsl:with-param name="pValue" select="$vClearingOrganizationAccount"/>
                <xsl:with-param name="pValueIdent" select="$vClearingOrganizationAccountIdent"/>
                <xsl:with-param name="pResultColumn" select="'BOOK_IDENTIFIER'"/>
              </xsl:call-template>
            </xsl:variable>

            <!--DealerEntity-->
            <!-- FI 20190904 [24882] Appel au template SQLActor2--> 
            <xsl:variable name ="vBuyerOrSellerEntityActor">
              <xsl:call-template name="SQLActor2">
                <xsl:with-param name="pValue" select="$vBuyerOrSellerEntity"/>
                <xsl:with-param name="pValueIdent" select="$vBuyerOrSellerEntityIdent"/>
                <xsl:with-param name="pDynamicDataCssIdentifier" select="$vDynamicDataCssIdentifier"/>
                <xsl:with-param name="pDynamicDataMarketIdentifier" select="$vDynamicDataMarketIdentifier"/>
                <xsl:with-param name="pClearingBusinessDate" select="$vBusinessDate"/>
                <xsl:with-param name="pClearingMemberType" select="'DCM'"/>
                <xsl:with-param name="pClearingMemberType2" select="'GCM'"/>
                <xsl:with-param name="pClearingMemberType3" select="'NCM'"/>
              </xsl:call-template>
            </xsl:variable>

            <!--Dealer-->
            <xsl:call-template name="XMLParty">
              <xsl:with-param name="pName" select="$ConstBuyerOrSeller"/>
              <xsl:with-param name="pParsingValue" select ="$vBuyerOrSellerAccount"/>
              <xsl:with-param name="pActor">
                <xsl:copy-of select="$vBuyerOrSellerActor"/>
              </xsl:with-param>
              <xsl:with-param name="pBook">
                <xsl:copy-of select="$vBuyerOrSellerBook"/>
              </xsl:with-param>
            </xsl:call-template>

            <!--Clearer-->
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
            </xsl:call-template>

            <!--DealerEntity-->
            <xsl:call-template name="XMLParty">
              <xsl:with-param name="pName">
                <xsl:value-of select="$ConstBuyerOrSellerEntity"/>
              </xsl:with-param>
              <xsl:with-param name="pParsingValue">
                <xsl:value-of select ="$vBuyerOrSellerEntity"/>
              </xsl:with-param>
              <xsl:with-param name="pActor">
                <xsl:copy-of select="$vBuyerOrSellerEntityActor"/>
              </xsl:with-param>
            </xsl:call-template>

          </xsl:variable>
          <customCaptureInfos>
            <!--Market Cci-->
            <xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId" select ="'Instrmt_Exch'"/>
              <xsl:with-param name="pDataType" select ="'string'"/>
              <xsl:with-param name="pXMLDynamicValue">
                <xsl:choose>
                  <xsl:when test="$vFamily='ETD'">
                    <!-- FI 20220823 [25699] Alimentation de pDerivativeContractType, pDerivativeContractSettltMethod, pDerivativeContractExerciseStyle, pClearingHouse, pClearingHouseColumn  -->
                    <xsl:call-template name="ETD_SQLMarket">
                      <xsl:with-param name="pBusinessDate" select="$vBusinessDate"/>
                      <xsl:with-param name="pMarket" select="$vMarket"/>
                      <xsl:with-param name="pMarketColumn" select="$vMarketColumn"/>
                      <xsl:with-param name="pAssetCode" select="$vAssetCode"/>
                      <xsl:with-param name="pAssetColumn" select="$vAssetCodeColumn"/>
                      <xsl:with-param name="pDerivativeContract" select="$vContract"/>
                      <xsl:with-param name="pDerivativeContractColumn" select="$vContractColumn"/>
                      <xsl:with-param name="pDerivativeContractVersion" select="$vContractVersion"/>
                      <xsl:with-param name="pDerivativeContractCategory" select="$vContractCategory"/>
                      <xsl:with-param name="pDerivativeContractType" select="$vContractType"/>
                      <xsl:with-param name="pDerivativeContractSettltMethod" select="$vContractSettltMethod"/>
                      <xsl:with-param name="pDerivativeContractExerciseStyle" select="$vContractExerciseStyle"/>
                      <xsl:with-param name="pClearingHouse" select="$vClearingHouse"/>
                      <xsl:with-param name="pClearingHouseColumn" select="$vClearingHouseColumn"/>
                    </xsl:call-template>
                  </xsl:when>
                  <!-- FI 20120704 [17991] use template for ESE-->
                  <xsl:when test="$vFamily='ESE'">
                    <xsl:call-template name="SQLMarket">
                      <xsl:with-param name="pMarket" select="$vMarket"/>
                      <xsl:with-param name="pMarketColumn" select="$vMarketColumn"/>
                    </xsl:call-template>
                  </xsl:when>
                </xsl:choose>
              </xsl:with-param>
            </xsl:call-template>
            <!-- requestType -->
            <xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId" select ="'requestType'"/>
              <xsl:with-param name="pDataType" select ="'string'"/> 
              <xsl:with-param name="pValue" select="$vRequestType"/>
            </xsl:call-template>
            
            <!-- requestMode -->
            <xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId" select ="'requestMode'"/>
              <xsl:with-param name="pDataType" select ="'string'"/>
              <xsl:with-param name="pValue" select="$vRequestMode"/>              
            </xsl:call-template>
            <!-- qty -->
            <xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId" select ="'qty'"/>
              <xsl:with-param name="pDataType" select ="'integer'"/>
              <xsl:with-param name="pValue" select="$vQty"/>
            </xsl:call-template>
            <!--clearingBusinessDate -->
            <xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId" select ="'clearingBusinessDate'"/>
              <xsl:with-param name="pDataType" select ="'string'"/>
              <xsl:with-param name="pValue"  select="$vBusinessDate">
            </xsl:with-param>
            </xsl:call-template>
            <!-- actorDealer -->
            <xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId" select ="'actorDealer'"/>
              <xsl:with-param name="pDataType" select ="'string'"/>
              <xsl:with-param name="pXMLDynamicValue">
                <xsl:copy-of select="msxsl:node-set($vXMLParties)/party[@name=$ConstBuyerOrSeller]/dynamicValue"/>
              </xsl:with-param>
            </xsl:call-template>
            <!-- bookDealer -->
            <xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId" select ="'bookDealer'"/>
              <xsl:with-param name="pDataType" select ="'string'"/>
              <xsl:with-param name="pXMLDynamicValue">
                <xsl:copy-of select="msxsl:node-set($vXMLParties)/book[@name=$ConstBuyerOrSeller]/dynamicValue"/>
              </xsl:with-param>
            </xsl:call-template>
            <!-- actorClearer -->
            <xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId" select ="'actorClearer'"/>
              <xsl:with-param name="pDataType" select ="'string'"/>
              <xsl:with-param name="pXMLDynamicValue">
                <xsl:copy-of select="msxsl:node-set($vXMLParties)/party[@name=$ConstClearingOrganization]/dynamicValue"/>
              </xsl:with-param>
            </xsl:call-template>
            <!-- bookClearer -->
            <xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId" select ="'bookClearer'"/>
              <xsl:with-param name="pDataType" select ="'string'"/>
              <xsl:with-param name="pXMLDynamicValue">
                <xsl:copy-of select="msxsl:node-set($vXMLParties)/book[@name=$ConstClearingOrganization]/dynamicValue"/>
              </xsl:with-param>
            </xsl:call-template>

            <!-- actorEntityDealer -->
            <xsl:if test="string-length($vBuyerOrSellerEntity)>0">
              <xsl:call-template name="CustomCaptureInfo">
                <xsl:with-param name="pClientId" select ="'actorEntityDealer'"/>
                <xsl:with-param name="pDataType" select ="'string'"/>
                <xsl:with-param name="pXMLDynamicValue">
                  <xsl:copy-of select="msxsl:node-set($vXMLParties)/party[@name=$ConstBuyerOrSellerEntity]/dynamicValue"/>
                </xsl:with-param>
              </xsl:call-template>
            </xsl:if>

            <!--notes -->
            <xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId" select ="'notes'"/>
              <xsl:with-param name="pDataType" select ="'string'"/>
              <xsl:with-param name="pValue" select="$vSpheresDescription"/>
            </xsl:call-template>
            <!-- Asset Cci-->
            <xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId" select ="'Instrmt_ID'"/>
              <xsl:with-param name="pDataType" select ="'string'"/>
              <xsl:with-param name="pXMLDynamicValue">
                <xsl:choose>
                  <xsl:when test="$vFamily='ETD'">
                    <xsl:call-template name="ETD_SQLInstrumentAndAsset">
                      <xsl:with-param name="pBusinessDate" select="$vBusinessDate"/>
                      <xsl:with-param name="pAssetCode" select="$vAssetCode"/>
                      <xsl:with-param name="pAssetColumn" select="$vAssetCodeColumn"/>
                      <xsl:with-param name="pMarket" select="$vMarket"/>
                      <xsl:with-param name="pMarketColumn" select="$vMarketColumn"/>
                      <xsl:with-param name="pResultColumn" select="'ASSET_IDENTIFIER'"/>
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:when test="$vFamily='ESE'">
                    <xsl:call-template name="ESE_SQLAsset">
                      <xsl:with-param name="pAssetCode" select="$vAssetCode"/>
                      <xsl:with-param name="pAssetColumn" select="$vAssetCodeColumn"/>
                      <!-- FI 20170404 [23039] add pMarket et pMarketColumn -->
                      <xsl:with-param name="pMarket" select="$vMarket"/>
                      <xsl:with-param name="pMarketColumn" select="$vMarketColumn"/>
                      <xsl:with-param name="pIsMandatory" select="$ConstTrue"/>
                    </xsl:call-template>
                  </xsl:when>
                </xsl:choose>

              </xsl:with-param>
            </xsl:call-template>
            <xsl:if test="string-length($vAssetCode)=0 and $vFamily='ETD' ">
              <!-- Contract Cci-->
              <xsl:call-template name="CustomCaptureInfo">
                <xsl:with-param name="pClientId" select ="'Instrmt_Sym'"/>
                <xsl:with-param name="pDataType" select ="'string'"/>
                <xsl:with-param name="pXMLDynamicValue">
                  <xsl:call-template name="SQLDerivativeContract">
                    <xsl:with-param name="pBusinessDate" select="$vBusinessDate"/>
                    <xsl:with-param name="pDerivativeContract" select="$vContract"/>
                    <xsl:with-param name="pDerivativeContractColumn" select="$vContractColumn"/>
                    <xsl:with-param name="pDerivativeContractVersion" select="$vContractVersion"/>
                    <xsl:with-param name="pDerivativeContractCategory" select="$vContractCategory"/>
                    <xsl:with-param name="pDerivativeContractType" select="$vContractType"/>
                    <xsl:with-param name="pDerivativeContractSettltMethod" select="$vContractSettltMethod"/>
                    <xsl:with-param name="pDerivativeContractExerciseStyle" select="$vContractExerciseStyle"/>
                    <xsl:with-param name="pMarket" select="$vMarket"/>
                    <xsl:with-param name="pMarketColumn" select="$vMarketColumn"/>
                    <xsl:with-param name="pClearingHouse" select="$vClearingHouse"/>
                    <xsl:with-param name="pClearingHouseColumn" select="$vClearingHouseColumn"/>
                  </xsl:call-template>
                </xsl:with-param>
              </xsl:call-template>
              <!-- Maturity Cci-->
              <xsl:call-template name="CustomCaptureInfo">
                <xsl:with-param name="pClientId" select ="'Instrmt_MMY'"/>
                <xsl:with-param name="pDataType" select ="'string'"/>
                <xsl:with-param name="pValue" select="$vContractMaturity"/>
              </xsl:call-template>
              <!-- For Option only-->
              <xsl:if test="$vContractCategory = 'O'">
                <!-- Type Cci-->
                <xsl:call-template name="CustomCaptureInfo">
                  <xsl:with-param name="pClientId" select ="'Instrmt_PutCall'"/>
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
                  <xsl:with-param name="pClientId" select ="'Instrmt_StrkPx'"/>
                  <xsl:with-param name="pDataType" select ="'string'"/>
                  <xsl:with-param name="pValue" select="$vContractStrike"/>
                </xsl:call-template>
              </xsl:if>
            </xsl:if>

            <!-- isPartialExecutionAllowed -->
            <xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId"  select ="'isPartialExecutionAllowed'"/>
              <xsl:with-param name="pDataType" select ="'bool'"/>
              <xsl:with-param name="pValue" select="$vSpheresPartialExecution"/>
            </xsl:call-template>

            <!-- isFeeCalculation -->
            <xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId" select ="'isFeeCalculation'"/>
              <xsl:with-param name="pDataType" select ="'bool'"/>
              <xsl:with-param name="pValue" select="$vSpheresFeeCalculation"/>
            </xsl:call-template>

            <!-- isAbandonRemainingQty -->
            <xsl:call-template name="CustomCaptureInfo">
              <xsl:with-param name="pClientId" select ="'isAbandonRemainingQty'"/>
              <xsl:with-param name="pDataType" select ="'bool'"/>
              <xsl:with-param name="pValue" select="$vSpheresAbandonRemainingQty"/>
            </xsl:call-template>


          </customCaptureInfos>
        </posRequestInput>
      </posRequestImport>
    </row>
  </xsl:template>

  <!-- virtual Template -->
  <!-- FI 20220823 [25699] Add pContractType, pContractSettltMethod, pContractExerciseStyle, pContractVersion-->
  <xsl:template name="ovrGetConditions">
    <xsl:param name="pActionType"/>
    <xsl:param name="pRequestType"/>
    <xsl:param name="pPosRequestId"/>
    <xsl:param name="pClearingBusinessDate"/>
    <xsl:param name="pAssetCode"/>
    <xsl:param name="pAssetColumn"/>
    <xsl:param name="pMarket"/>
    <xsl:param name="pMarketColumn"/>
    <xsl:param name="pDC"/>
    <xsl:param name="pDCColumn"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pMaturityMonthYear"/>
    <xsl:param name="pStrikePrice"/>
    <xsl:param name="pPutCall"/>
    <xsl:param name="pContractType"/>
    <xsl:param name="pContractSettltMethod"/>
    <xsl:param name="pContractExerciseStyle"/>
    <xsl:param name="pContractVersion"/>

    <xsl:call-template name="GetStandardConditions">
      <xsl:with-param name="pActionType" select="$pActionType"/>
      <xsl:with-param name="pPosRequestId" select="$pPosRequestId"/>
      <xsl:with-param name="pBusinessDate" select="$pClearingBusinessDate"/>
    </xsl:call-template>

  </xsl:template>

  <xsl:template name="GetStandardConditions">
    <xsl:param name="pActionType"/>
    <xsl:param name="pPosRequestId"/>
    <xsl:param name="pBusinessDate"/>

    <xsl:choose>
      <xsl:when test="$pActionType = 'N' or 
                      $pActionType = 'U' or $pActionType = 'M' or 
                      $pActionType = 'S'">

        <!-- TODO FI 201303 il faut passer en mode ressource-->
        <xsl:variable name ="vMsg">
          <xsl:choose>
            <xsl:when test="$pActionType = 'N' ">
              <xsl:value-of  select="concat('PosRequest already imported (Identifier source:',$pPosRequestId,')')" />
            </xsl:when>
            <xsl:when test="$pActionType = 'U' or $pActionType = 'M' ">
              <xsl:value-of  select="concat('PosRequest to update does not exist (Identifier source:',$pPosRequestId,')')" />
            </xsl:when>
            <xsl:when test="$pActionType = 'S' ">
              <xsl:value-of  select="concat('PosRequest to delete does not exist (Identifier source:',$pPosRequestId,')')" />
            </xsl:when>
          </xsl:choose>
        </xsl:variable>

        <!--<xsl:variable name ="logNumber">
          <xsl:choose>
            <xsl:when test="$vActionType = 'N' ">
              <xsl:value-of select ="06020" />
            </xsl:when>
            <xsl:when test="$vActionType = 'U' or $vActionType = 'M' ">
              <xsl:value-of select ="06021" />
            </xsl:when>
            <xsl:when test="$vActionType = 'S' ">
              <xsl:value-of select ="06026" />
            </xsl:when>
          </xsl:choose>
        </xsl:variable>-->

        <condition name="IsActionOk" datatype="bool">
          <!--lors des intégrations gateway, l'importation termine en success si la condition est non respectée -->
          <!--lors des intégrations manuelle,l'importation termine en success/warning si la condition est non respectée -->
          <xsl:variable name="status">
            <xsl:choose>
              <xsl:when test="$gIsModegateway=$ConstTrue">INFO</xsl:when>
              <xsl:otherwise>WARNING</xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <logInfo status="{$status}">
            <!--<code>LOG</code>
            <number>
              <xsl:value-of select="$logNumber"/>
            </number>-->
            <message>
              <xsl:value-of select="$vMsg"/>
            </message>
            <!--<data1>
              <xsl:value-of select="$vPosRequestId"/>
            </data1>
            <data2>
              <xsl:value-of select="$vActionTypeLog"/>
            </data2>
            <data3>
              <xsl:value-of select="$vSrc"/>
            </data3>
            <data4>
              <xsl:value-of select="$vId"/>
            </data4>-->
          </logInfo>
          <xsl:call-template name="SQLCheckPosRequest">
            <xsl:with-param name="pActionType" select="$pActionType"/>
            <xsl:with-param name="pPosRequestId" select="$pPosRequestId"/>
            <xsl:with-param name="pResultColumn" select="'ISOK'"/>
            <xsl:with-param name="pBusinessDate" select="$pBusinessDate"/>
          </xsl:call-template>
        </condition>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="SQLCheckPosRequest">
    <xsl:param name="pActionType"/>
    <xsl:param name="pPosRequestId"/>
    <xsl:param name="pResultColumn"/>
    <xsl:param name="pBusinessDate"/>
    
    <SQL command="select" result="{$pResultColumn}">
      <xsl:choose>
        <!--
        ISOK=> Le POSREQUEST n'existe pas 
        IMPORTMODE => New
        -->
        <!-- FI 20220916 [XXXXX] Correction de la requête -->
        <xsl:when test="$pActionType='N'">
          <xsl:text>
          <![CDATA[
          select 
          case when
          tabAll.COUNTIDPR = 0 then 1 else 0 end as ISOK,
          'New' as IMPORTMODE
          from
          (
            select count(pr.IDPR) as COUNTIDPR
            from dbo.POSREQUEST pr
            where (upper(pr.EXTLLINK)=upper(@PRID)) and (pr.DTBUSINESS=@DT) and (pr.STATUS in ('SUCCESS','PENDING'))
          )tabAll
          ]]> 
          </xsl:text>
        </xsl:when>

        <!--
        ISOK=> 1 dans tous les cas  
        IMPORTMODE => Si le POSREQUEST n'existe pas alors New sinon Update
        -->
        <xsl:when test="$pActionType='M'">
          <xsl:text>
          <![CDATA[
          select
          1 as ISOK,
          case when (tabAll.COUNTIDPR = 0) then
          'New' else 'Update' end as IMPORTMODE
          from
          (
            select count(pr.IDPR) as COUNTIDPR
            from dbo.POSREQUEST pr
            where (upper(pr.EXTLLINK)=upper(@PRID)) and (pr.DTBUSINESS=@DT) and (pr.STATUS in ('SUCCESS','PENDING'))
          )tabAll          
          ]]> 
          </xsl:text>
        </xsl:when>

        <!--
        ISOK=> Si le POSREQUEST existe
        IMPORTMODE => Update
        -->
        <xsl:when test="$pActionType='U'">
          <xsl:text>
          <![CDATA[
          select 
          case when
          tabAll.COUNTIDPR = 1 then 1 else 0 end as ISOK,
          'Update' as IMPORTMODE
          from
          (
            select count(pr.IDPR) as COUNTIDPR
            from dbo.POSREQUEST pr
            where (upper(pr.EXTLLINK)=upper(@PRID)) and (pr.DTBUSINESS=@DT) and (pr.STATUS in ('SUCCESS','PENDING'))
          )tabAll      
          ]]> 
          </xsl:text>
        </xsl:when>

        <!--
        ISOK=> Si le POSREQUEST existe
        IMPORTMODE => RemoveOnly 
        -->
        <xsl:when test="$pActionType='S'">
          <xsl:text>
          <![CDATA[
          select 
          case when
          tabAll.COUNTIDPR = 1 then 1 else 0 end as ISOK,
          'RemoveOnly' as IMPORTMODE
          from
          (
            select count(pr.IDPR) as COUNTIDPR
            from dbo.POSREQUEST pr
            where (upper(pr.EXTLLINK)=upper(@PRID)) and (pr.DTBUSINESS=@DT) and (pr.STATUS in ('SUCCESS','PENDING'))
          )tabAll      
          ]]> 
          </xsl:text>
        </xsl:when>
      </xsl:choose>
      <Param name="PRID" datatype="string">
        <xsl:value-of select="$pPosRequestId" />
      </Param>
      <Param name="DT" datatype="date">
        <xsl:value-of select="$pBusinessDate"/>
      </Param>
    </SQL>
  </xsl:template>
  <xsl:template name="SQLPosRequestIdPR">
    <xsl:param name="pPosRequestId"/>
    <xsl:param name="pBusinessDate"/>
    <SQL command="select" result="IDPR">
      <!-- Recherche d'un posRequest existant -->
      <xsl:text>
        <![CDATA[
          select pr.IDPR
          from dbo.POSREQUEST pr
          where (upper(pr.EXTLLINK)=upper(@PRID)) and (pr.DTBUSINESS=@DT) and (pr.STATUS in ('SUCCESS','PENDING'))
        ]]>
      </xsl:text>

      <Param name="PRID" datatype="string">
        <xsl:value-of select="$pPosRequestId" />
      </Param>
      <Param name="DT" datatype="date">
        <xsl:value-of select="$pBusinessDate"/>
      </Param>
    </SQL>
  </xsl:template>

</xsl:stylesheet>
