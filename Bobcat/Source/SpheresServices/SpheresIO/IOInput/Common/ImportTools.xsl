<?xml version="1.0" encoding="utf-8" ?>
<!-- FI 20170404 [23039] usage de xmlns:msxsl="urn:schemas-microsoft-com:xslt" -->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt">
<!--  
=============================================================================================
Summary : Spheres standard tools for ETD imports (Trades, PosRequest, ...)
          Some templates of this xsl are overrided by:
            - specific BCS's xsl files
            - specific customer's xsl files
          
File    : ImportTools.xsl
=============================================================================================

FI 20170824 [23339] Modify

FI 20161005 [XXXXX] Remove ovrETD_SQLMarket and ovrSQLDerivativeContract

FI 20160929 [22507] Modify

RD 20140409 [19816]
  - Add ovrSQLDealerActorAndBook, ovrSQLClearingOrganization, ovrSQLClearingOrganizationBook
  - Modify SQLJoinExtlId: Manage the case that EXTLLINK contains multiple values. Example: "Codice1";"Codice2";"Codice3"

RD 20140324 [19704] 
  - Add ovrETD_SQLMarket and ovrSQLDerivativeContract
  - Add new parameter for existing templates: ETD_SQLMarket and SQLDerivativeContract 

RD 20140120 [19513] 
 - ajout de saut de lignes sur la requête du template ETD_SQLInstrumentAndAsset
==============================================================================================
-->

  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml;"/>
  <xsl:decimal-format name="decimalFormat" decimal-separator="."/>

  <!-- FI 20160929 [22507] Include pour usage du template Replace -->
  <xsl:include href ="..\..\Library\StrFunc.xslt"/>

  <!-- ================================================== -->
  <!--        Global Constantes                           -->
  <!-- ================================================== -->
  <xsl:variable name ="ConstTrue" select ="'true'"/>
  <xsl:variable name ="ConstFalse" select ="'false'"/>
  <xsl:variable name ="ConstUnknown" select ="'{unknown}'"/>
  <xsl:variable name ="ConstUseSQL" select ="'{UseSQL}'"/>
  <xsl:variable name ="ConstActorSYSTEM" select ="'SYSTEM'"/>
  <xsl:variable name ="ConstSpheresExtlId" select ="'SpheresExtlId-'"/>
  <!-- FI 20170404 [23039] Add  -->
  <xsl:variable name ="ConstMemberId" select ="'MemberId-'"/>
  <xsl:variable name ="ConstNA" select ="'N/A'"/>
  <!-- FI 20160929 [22507] Add -->
  <xsl:variable name ="s-quote">'</xsl:variable>

  <xsl:variable name ="ConstBuyerOrSeller" select ="'BuyerOrSeller'"/>
  <xsl:variable name ="ConstBuyer" select ="'Buyer'"/>
  <xsl:variable name ="ConstSeller" select ="'Seller'"/>
  <xsl:variable name ="ConstBuyerOrSellerEntity" select ="'BuyerOrSellerEntity'"/>
  <xsl:variable name ="ConstNegociationBroker" select ="'NegociationBroker'"/>
  <xsl:variable name ="ConstBuyerNegociationBroker" select ="'BuyerNegociationBroker'"/>
  <xsl:variable name ="ConstSellerNegociationBroker" select ="'SellerNegociationBroker'"/>
  <xsl:variable name ="ConstExecutingBroker" select ="'ExecutingBroker'"/>
  <!-- FI 20170404 [23039] Add ConstClearingBroker -->
  <xsl:variable name ="ConstClearingBroker" select ="'ClearingBroker'"/>
  <xsl:variable name ="ConstBuyerExecutingBroker" select ="'BuyerExecutingBroker'"/>
  <xsl:variable name ="ConstSellerExecutingBroker" select ="'SellerExecutingBroker'"/>
  <xsl:variable name ="ConstBuyerClearingBroker" select ="'BuyerClearingBroker'"/>
  <xsl:variable name ="ConstSellerClearingBroker" select ="'SellerClearingBroker'"/>
  <xsl:variable name ="ConstClearingOrganization" select ="'ClearingOrganization'"/>

  <xsl:variable name ="gIsExistParameters">
    <xsl:choose>
      <xsl:when test="/iotask/parameters">
        <xsl:value-of select="$ConstTrue"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$ConstFalse"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name ="gAllRows" select ="/iotask/iotaskdet/ioinput/file"/>

  <!-- ================================================== -->
  <!--               Templates                            -->
  <!-- ================================================== -->
  
  <!-- FI 20170404 [23039] Add GetRecordVersion  -->
  <xsl:template name="GetRecordVersion">
    <xsl:param name="pRow"/>

    <xsl:variable name="vRecordVersion" select ="normalize-space($pRow/data[@name='RVR'])"/>
    <xsl:choose>
      <xsl:when test="string-length($vRecordVersion)>0">
        <xsl:value-of select="concat('v',$vRecordVersion)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'v1'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- FI 20220823 [XXXXX] Add Retourne ContractType selon les versions d'import  
  v1 : ETD => "" (COMS non géré).
  v2 : ETD => "". COMS => duration (lecture de CTRT)
  v3 : ETD => 'STD' ou 'Flex' (lecture de CTRT). COMS => duration (lecture de CTRT)
  -->
  <xsl:template name="GetContractType">
    <xsl:param name="pRow"/>
    <xsl:param name="pFamily"/>

    <xsl:variable name="vRecordVersion">
      <xsl:call-template name="GetRecordVersion">
        <xsl:with-param name="pRow" select="."/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test= "$pFamily='ETD'">
        <!-- v1 (only ETD) and v2 => vContractType info does not exist. 
          v1=>data[@name='CTRT'] does not exist 
          v2=>data[@name='CTRT'] contains PutCall information -->
        <xsl:if test="$vRecordVersion='v3'">
          <xsl:value-of select ="normalize-space($pRow/data[@name='CTRT'])"/>
        </xsl:if>
      </xsl:when>
      <xsl:otherwise>
        <!-- COMS  v2, v3  => data[@name='CTRT'] contains duration like 15Minutes, 30minutes -->
        <xsl:value-of select ="normalize-space($pRow/data[@name='CTRT'])"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template name="IsToApplyOrToIgnore">
    <xsl:param name="pApplyIgnore"/>
    <xsl:choose>
      <xsl:when test="$pApplyIgnore='Apply'">
        <xsl:value-of select="'true'"/>
      </xsl:when>
      <xsl:when test="$pApplyIgnore='Ignore'">
        <xsl:value-of select="'false'"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="GetCommonIdentColumn">
    <xsl:param name="pIdent"/>
    <xsl:choose>
      <xsl:when test="$pIdent = 'SpheresIdentifier'">
        <xsl:value-of select="'IDENTIFIER'"/>
      </xsl:when>
      <xsl:when test="$pIdent = 'SpheresDisplayname'">
        <xsl:value-of select="'DISPLAYNAME'"/>
      </xsl:when>
      <xsl:when test="$pIdent = 'SpheresExtlLink'">
        <xsl:value-of select="'EXTLLINK'"/>
      </xsl:when>
      <xsl:when test="$pIdent = 'SpheresExtlLink2'">
        <xsl:value-of select="'EXTLLINK2'"/>
      </xsl:when>
      <!--SpheresExtlId-XXX-->
      <xsl:when test="starts-with($pIdent,$ConstSpheresExtlId)">
        <xsl:value-of select="$pIdent"/>
      </xsl:when>
      <!--RD Glop -->
      <!--xsl:when test="$pIdent = 'Proprietary'">                        
          </xsl:when-->
      <xsl:otherwise>
        <xsl:value-of select="'IDENTNOTFOUND'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="GetMarketIdentColumn">
    <xsl:param name="pIdent"/>
    <xsl:choose>
      <xsl:when test="$pIdent = 'SpheresShortIdentifier'">
        <xsl:value-of select="'SHORTIDENTIFIER'"/>
      </xsl:when>
      <xsl:when test="$pIdent = 'SpheresExchSymbol'">
        <xsl:value-of select="'EXCHANGESYMBOL'"/>
      </xsl:when>
      <xsl:when test="$pIdent = 'SpheresExchAcronym'">
        <xsl:value-of select="'EXCHANGEACRONYM'"/>
      </xsl:when>
      <xsl:when test="$pIdent = 'SpheresISO10383'">
        <xsl:value-of select="'ISO10383_ALPHA4'"/>
      </xsl:when>
      <xsl:when test="$pIdent = 'SpheresId'">
        <xsl:value-of select="'IDM'"/>
      </xsl:when>
      <xsl:when test="$pIdent = 'SpheresSecurityExchange'">
        <xsl:value-of select="'FIXML_SECURITYEXCHANGE'"/>
      </xsl:when>

      <xsl:otherwise>
        <xsl:call-template name="GetCommonIdentColumn">
          <xsl:with-param name="pIdent">
            <xsl:value-of select="$pIdent"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <xsl:template name="GetAssetIdentColumn">
    <xsl:param name="pIdent"/>
    <xsl:choose>
      <xsl:when test="$pIdent = 'MARKET'">
        <xsl:value-of select="'ASSETSYMBOL'"/>
      </xsl:when>
      <!-- FI 20220218 [XXXXX] Add -->
      <xsl:when test="$pIdent = 'MARKETASSIGNEDID'">
        <xsl:value-of select="'MARKETASSIGNEDID'"/>
      </xsl:when>
      <xsl:when test="$pIdent = 'ISIN'">
        <xsl:value-of select="'ISINCODE'"/>
      </xsl:when>
      <xsl:when test="$pIdent = 'CFI'">
        <xsl:value-of select="'CFICODE'"/>
      </xsl:when>
      <xsl:when test="$pIdent = 'AII'">
        <xsl:value-of select="'AII'"/>
      </xsl:when>
      <xsl:when test="$pIdent = 'SpheresId'">
        <xsl:value-of select="'IDASSET'"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="GetCommonIdentColumn">
          <xsl:with-param name="pIdent">
            <xsl:value-of select="$pIdent"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <!-- FI 20170404 [23039] Modify  -->
  <xsl:template name="GetDerivativeContractIdentColumn">
    <xsl:param name="pIdent"/>
    <!-- FI 20170404 [23039] call GetContractIdentColumn-->
    <xsl:call-template name="GetContractIdentColumn">
      <xsl:with-param name="pIdent">
        <xsl:value-of select="$pIdent"/>
      </xsl:with-param>
      <xsl:with-param name="pFamily">
        <xsl:value-of select="'ETD'"/>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <!-- FI 20170404 [23039] Add  -->
  <xsl:template name="GetCommodityContractIdentColumn">
    <xsl:param name="pIdent"/>
    <xsl:call-template name="GetContractIdentColumn">
      <xsl:with-param name="pIdent">
        <xsl:value-of select="$pIdent"/>
      </xsl:with-param>
      <xsl:with-param name="pFamily">
        <xsl:value-of select="'COMS'"/>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>
  
  <!-- FI 20170404 [23039] Add Template -->
  <xsl:template name="GetContractIdentColumn">
    <xsl:param name = "pIdent" />
    <xsl:param name = "pFamily" />
    <xsl:choose>
      <xsl:when test="$pIdent = 'SpheresContractCode'">
        <xsl:value-of select="'CONTRACTSYMBOL'"/>
      </xsl:when>
      <xsl:when test="$pIdent = 'SpheresId'">
        <xsl:choose>
          <xsl:when test="$pFamily = 'ETD'">
            <xsl:value-of select="'IDDC'"/>
          </xsl:when>
          <xsl:when test="$pFamily = 'COMS'">
            <xsl:value-of select="'IDCC'"/>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="GetCommonIdentColumn">
          <xsl:with-param name="pIdent">
            <xsl:value-of select="$pIdent"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="GetBookIdentColumn">
    <xsl:param name="pIdent"/>
    <xsl:choose>
      <xsl:when test="$pIdent = 'SpheresId'">
        <xsl:value-of select="'IDB'"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="GetCommonIdentColumn">
          <xsl:with-param name="pIdent">
            <xsl:value-of select="$pIdent"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- FI 20170404 [23039] Modify Method -->
  <xsl:template name="GetActorIdentColumn">
    <xsl:param name="pIdent"/>
    <xsl:choose>
      <xsl:when test="$pIdent = 'BIC'">
        <xsl:value-of select="'BIC'"/>
      </xsl:when>
      <!-- FI 20170404 [23039] add LEI, IGI -->
      <xsl:when test="$pIdent = 'LEI'">
        <xsl:value-of select="'ISO17442'"/>
      </xsl:when>
      <xsl:when test="$pIdent = 'IGI'">
        <xsl:value-of select="'IBEI'"/>
      </xsl:when>
      <xsl:when test="$pIdent = 'NCB'">
        <xsl:value-of select="'NCBNUMBER'"/>
      </xsl:when>
      <!-- FI 20220217 [XXXXX] Add -->
      <xsl:when test="$pIdent = 'SpheresISO10383' or $pIdent = 'MIC'">
        <xsl:value-of select="'ISO10383_ALPHA4'"/>
      </xsl:when>
      <xsl:when test="$pIdent = 'SpheresId'">
        <xsl:value-of select="'IDA'"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="GetCommonIdentColumn">
          <xsl:with-param name="pIdent">
            <xsl:value-of select="$pIdent"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- FI 20170404 [23039] Add GetInstrIdentColumn Template -->
  <xsl:template name="GetInstrIdentColumn">
    <xsl:param name="pIdent"/>
    <xsl:choose>
      <xsl:when test="$pIdent = 'SpheresId'">
        <xsl:value-of select="'IDI'"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="GetCommonIdentColumn">
          <xsl:with-param name="pIdent">
            <xsl:value-of select="$pIdent"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template name="GetActionTypeLog">
    <xsl:param name="pActionType"/>
    <xsl:choose>
      <xsl:when test="string-length($pActionType) > 0">

        <xsl:variable name ="labelActionType">
          <xsl:call-template name="GetLabelActionType">
            <xsl:with-param name="pActionType" select="$pActionType"/>
          </xsl:call-template>
        </xsl:variable>

        <xsl:value-of select="$pActionType"/>
        <xsl:value-of select ="concat(' (',$labelActionType,')')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$ConstUnknown"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- FI 20160907 [21831] Modify -->
  <xsl:template name="GetLabelActionType">
    <xsl:param name="pActionType"/>
    <xsl:choose>
      <xsl:when test="$pActionType='N'">
        <xsl:value-of select="'New'"/>
      </xsl:when>
      <xsl:when test="$pActionType='M'">
        <xsl:value-of select="'Modify'"/>
      </xsl:when>
      <xsl:when test="$pActionType='G'">
        <xsl:value-of select="'Give-up'"/>
      </xsl:when>
      <xsl:when test="$pActionType='U'">
        <xsl:value-of select="'Update'"/>
      </xsl:when>
      <!-- FI 20160907 [21831] Add 'F' -->
      <xsl:when test="$pActionType='F'">
        <xsl:value-of select="'Update Fees'"/>
      </xsl:when>
      <xsl:when test="$pActionType='S'">
        <xsl:value-of select="'Suppress'"/>
      </xsl:when>
      <!-- FI 20170824 [23339] Add 'T' -->      
      <xsl:when test="$pActionType='T'">
        <xsl:value-of select="'Update Trader'"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$ConstUnknown"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Recherche de l'asset (ou de l'instrument) à partir de l'asset -->
  <!-- FI 20170404 [23039] Modify -->
  <!-- FI 20220304 [25699] Add pClearingHouse, pClearingHouseColumn -->
  <xsl:template name="ETD_SQLInstrumentAndAsset">
    <xsl:param name="pBusinessDate"/>
    <xsl:param name="pAssetCode"/>
    <xsl:param name="pAssetColumn"/>
    <xsl:param name="pMarket"/>
    <xsl:param name="pMarketColumn"/>
    <xsl:param name="pClearingHouse"/>
    <xsl:param name="pClearingHouseColumn"/>
    <xsl:param name="pResultColumn" select="'IDENTIFIER'"/>
    <xsl:param name="pIsMandatory" select="$ConstFalse"/>
    <xsl:choose>
      <xsl:when test="string-length($pAssetCode) > 0 and string-length($pAssetColumn) > 0">
        <xsl:choose>
          <xsl:when test="($pResultColumn = 'ASSET_IDENTIFIER' and $pAssetColumn = 'IDENTIFIER')">
            <xsl:value-of select="$pAssetCode"/>
          </xsl:when>
          <xsl:otherwise>
            <SQL command="select" result="{$pResultColumn}" cache="true">
              <![CDATA[
              select '0' as LEVELSORT,i.IDENTIFIER,asset.IDENTIFIER as ASSET_IDENTIFIER
              from dbo.INSTRUMENT i
              inner join dbo.DERIVATIVECONTRACT dc on dc.IDI=i.IDI
              inner join dbo.DERIVATIVEATTRIB da on da.IDDC=dc.IDDC
              inner join dbo.ASSET_ETD asset on asset.IDDERIVATIVEATTRIB=da.IDDERIVATIVEATTRIB
              ]]>
              <xsl:call-template name="SQLDTENABLEDDTDISABLED">
                <xsl:with-param name="pTable" select="'dc'"/>
              </xsl:call-template>
              <xsl:call-template name="SQLDTENABLEDDTDISABLED">
                <xsl:with-param name="pTable" select="'da'"/>
              </xsl:call-template>
              <xsl:call-template name="SQLDTENABLEDDTDISABLED">
                <xsl:with-param name="pTable" select="'asset'"/>
              </xsl:call-template>
              <xsl:if test="(string-length($pMarket) > 0 and string-length($pMarketColumn) > 0) or (string-length($pClearingHouse) > 0 and string-length($pClearingHouseColumn) > 0)">
                <![CDATA[inner join dbo.VW_MARKET_IDENTIFIER m on m.IDM=dc.IDM]]>
                <xsl:call-template name="SQLDTENABLEDDTDISABLED">
                  <xsl:with-param name="pTable" select="'m'"/>
                </xsl:call-template>
              </xsl:if>
              <xsl:if test="(string-length($pClearingHouse) > 0 and string-length($pClearingHouseColumn) > 0)">              
                <![CDATA[inner join dbo.ACTOR css on css.IDA=m.IDA]]>
              </xsl:if>
              
              <xsl:variable name ="vXMLExtlIdJoins">
                <xsl:call-template name="XMLJoinExtlId">
                  <xsl:with-param name="pTableName" select="'ASSET_ETD'"/>
                  <xsl:with-param name="pTableAlias" select="'asset'"/>
                  <xsl:with-param name="pJoinId" select="'asset.IDASSET'"/>
                  <xsl:with-param name="pValueColumn" select="$pAssetColumn"/>
                  <xsl:with-param name="pValue" select="$pAssetCode"/>
                </xsl:call-template>
                <xsl:if test="string-length($pMarket) > 0 and string-length($pMarketColumn) > 0">
                  <xsl:call-template name="XMLJoinExtlId">
                    <xsl:with-param name="pTableName" select="'MARKET'"/>
                    <xsl:with-param name="pTableAlias" select="'m'"/>
                    <xsl:with-param name="pJoinId" select="'m.IDM'"/>
                    <xsl:with-param name="pValueColumn" select="$pMarketColumn"/>
                    <xsl:with-param name="pValue" select="$pMarket"/>
                  </xsl:call-template>
                </xsl:if>
                 <!-- FI 20220304 [25699] Add -->
                <xsl:if test="string-length($pClearingHouse) > 0 and string-length($pClearingHouseColumn) > 0">
                    <xsl:call-template name="XMLJoinExtlId">
                      <xsl:with-param name="pTableName" select="'ACTOR'"/>
                      <xsl:with-param name="pTableAlias" select="'css'"/>
                      <xsl:with-param name="pJoinId" select="'css.IDA'"/>
                      <xsl:with-param name="pValueColumn" select="$pClearingHouseColumn"/>
                      <xsl:with-param name="pValue" select="$pClearingHouse"/>
                    </xsl:call-template>
                </xsl:if>
              </xsl:variable>
              <xsl:variable name="vAdditionalParameter">
                <Param name="UNKNOWN" datatype="string">
                  <xsl:value-of select="$ConstUnknown"/>
                </Param>
                <Param name="ASSETVALUE" datatype="string">
                  <xsl:value-of select="concat($pAssetCode,' [',$pAssetColumn,']')"/>
                </Param>
              </xsl:variable>
              <xsl:call-template name="SQLJoinExtlId">
                <xsl:with-param name="pXMLExtlIdJoins">
                  <xsl:copy-of select="$vXMLExtlIdJoins"/>
                </xsl:with-param>
                <xsl:with-param name="pAdditionalSQL">
                 <![CDATA[union all
                  select '1' as LEVELSORT,@UNKNOWN as IDENTIFIER,@ASSETVALUE as ASSET_IDENTIFIER
                  from DUAL
                  order by LEVELSORT]]>
                </xsl:with-param>
                <xsl:with-param name="pAdditionalParameter">
                  <xsl:copy-of select="$vAdditionalParameter"/>
                </xsl:with-param>
              </xsl:call-template>

              <Param name="DT" datatype="date">
                <xsl:value-of select="$pBusinessDate"/>
              </Param>
            </SQL>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="string-length($pAssetCode) > 0">
        '<xsl:value-of select="concat($pAssetCode,' [',$ConstUnknown,']')"/>'
      </xsl:when>
      <xsl:when test="$pIsMandatory=$ConstTrue">
        <xsl:value-of select="$ConstUnknown"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="XMLJoinExtlId">
    <xsl:param name="pTableName"/>
    <xsl:param name="pTableAlias"/>
    <xsl:param name="pJoinId"/>
    <xsl:param name="pValueColumn"/>
    <xsl:param name="pValue"/>
    <extlIdJoin tableName="{$pTableName}" tableAlias="{$pTableAlias}" joinId="{$pJoinId}" valueColumn="{$pValueColumn}">
      <xsl:value-of select="$pValue"/>
    </extlIdJoin>
  </xsl:template>

  <xsl:template name="SQLJoinExtlId">
    <xsl:param name="pXMLExtlIdJoins"/>
    <xsl:param name="pAdditionalCondition"/>
    <xsl:param name="pAdditionalSQL"/>
    <xsl:param name="pAdditionalParameter"/>

    <!-- D'abord les jointures pour Ident = SpheresExtlId-XXX-->
    <!-- FI 20170404 [23039] Utilisation de msxsl:node-set -->
    <xsl:for-each select="msxsl:node-set($pXMLExtlIdJoins)/extlIdJoin">
      <xsl:variable name ="vJoinPos" select="position()"/>
      <xsl:if test="string-length(text()) > 0 and string-length(@valueColumn) > 0 and starts-with(@valueColumn,$ConstSpheresExtlId)">
        <xsl:variable name="vExtlIdIdentifier" select="substring-after(@valueColumn, $ConstSpheresExtlId)"/>
        <xsl:if test="string-length($vExtlIdIdentifier) > 0">
          <xsl:text>
inner join dbo.EXTLID extlid</xsl:text>
          <xsl:value-of select="$vJoinPos"/>
          <xsl:text> on extlid</xsl:text>
          <xsl:value-of select="$vJoinPos"/>
          <xsl:text>.TABLENAME = @TABLENAME</xsl:text>
          <xsl:value-of select="$vJoinPos"/>
          <xsl:text>
and extlid</xsl:text>
          <xsl:value-of select="$vJoinPos"/>
          <xsl:text>.ID = </xsl:text>
          <xsl:value-of select="@joinId"/>
          <xsl:text>
and extlid</xsl:text>
          <xsl:value-of select="$vJoinPos"/>
          <xsl:text>.IDENTIFIER = @IDENTIFIER</xsl:text>
          <xsl:value-of select="$vJoinPos"/>
          <xsl:text>
and extlid</xsl:text>
          <xsl:value-of select="$vJoinPos"/>
          <xsl:text>.VALUE = @VALUE</xsl:text>
          <xsl:value-of select="$vJoinPos"/>
        </xsl:if>
      </xsl:if>
    </xsl:for-each>
    <!-- Ensuite les conditions pour Ident != SpheresExtlId-XXX-->
    <xsl:variable name ="vCondition">
      <!-- FI 20170404 [23039] Utilisation de msxsl:node-set -->
      <xsl:for-each select="msxsl:node-set($pXMLExtlIdJoins)/extlIdJoin">
        <xsl:variable name ="vJoinPos" select="position()"/>
        <xsl:if test="string-length(text()) > 0 and string-length(@valueColumn) > 0 and (false = starts-with(@valueColumn,$ConstSpheresExtlId))">
          <!--RD 20140409 [19816] Manage the case that EXTLLINK contains multiple values. Example: "Codice1";"Codice2";"Codice3"-->
          <xsl:value-of select="' and ('"/>
          <xsl:value-of select="@tableAlias"/>.<xsl:value-of select="@valueColumn"/> = @VALUE<xsl:value-of select="$vJoinPos"/>
          <xsl:if test="@valueColumn = 'EXTLLINK'">
            <xsl:value-of select="' or '"/>
            <xsl:value-of select="@tableAlias"/>.EXTLLINK like '%"'||@VALUE<xsl:value-of select="$vJoinPos"/>||'"%'
          </xsl:if>
          <xsl:value-of select="')'"/>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name ="vCondition2" >
      <xsl:if test="string-length($pAdditionalCondition) > 0">
        <xsl:value-of select="'('"/>
        <xsl:value-of select="$pAdditionalCondition"/>
        <xsl:value-of select="')'"/>
      </xsl:if>
      <xsl:value-of select="$vCondition"/>
    </xsl:variable>
    <xsl:variable name ="vCondition3" >
      <xsl:choose>
        <xsl:when test="starts-with($vCondition2, ' and ')">
          <xsl:value-of select="substring-after($vCondition2, ' and ')"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$vCondition2"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:if test="string-length($vCondition3) > 0">
      <xsl:value-of select="' where '"/>
      <xsl:value-of select="$vCondition3"/>
    </xsl:if>
    <!-- Ensuite le SQL Additionnel-->
    <xsl:if test="string-length($pAdditionalSQL) > 0">
      <xsl:value-of select="$pAdditionalSQL"/>
    </xsl:if>
    <!-- En fin les parametres-->
    <!-- FI 20170404 [23039] Utilisation de msxsl:node-set -->
    <xsl:for-each select="msxsl:node-set($pXMLExtlIdJoins)/extlIdJoin">
      <xsl:variable name ="vJoinPos" select="position()"/>
      <xsl:if test="string-length(text()) > 0 and string-length(@valueColumn) > 0">
        <!-- Ident = SpheresExtlId-XXX-->
        <xsl:if test="starts-with(@valueColumn,$ConstSpheresExtlId)">
          <xsl:variable name="vExtlIdIdentifier" select="substring-after(@valueColumn, $ConstSpheresExtlId)"/>
          <xsl:if test="string-length($vExtlIdIdentifier) > 0">
            <Param datatype="string">
              <xsl:attribute name="name">
                <xsl:value-of select="'TABLENAME'"/>
                <xsl:value-of select="$vJoinPos"/>
              </xsl:attribute>
              <xsl:value-of select="@tableName"/>
            </Param>
            <Param datatype="string">
              <xsl:attribute name="name">
                <xsl:value-of select="'IDENTIFIER'"/>
                <xsl:value-of select="$vJoinPos"/>
              </xsl:attribute>
              <xsl:value-of select="$vExtlIdIdentifier"/>
            </Param>
          </xsl:if>
        </xsl:if>
        <Param datatype="string">
          <xsl:attribute name="name">
            <xsl:value-of select="'VALUE'"/>
            <xsl:value-of select="$vJoinPos"/>
          </xsl:attribute>
          <xsl:value-of select="text()"/>
        </Param>
      </xsl:if>
    </xsl:for-each>
    <!-- FI 20170404 [23039] Utilisation de msxsl:node-set -->
    <xsl:for-each select="msxsl:node-set($pAdditionalParameter)/Param">
      <xsl:copy-of select='.'/>
    </xsl:for-each>
  </xsl:template>

  <!-- FI 20130702 [18798] add pIsMissingModeSpecified (l'attribut missingMode est alimenté uniquement si pIsMissingModeSpecified vaut true)-->
  <!-- FI 20170404 [23039] add pIsAutoCreate -->
  <!-- FI 20170718 [23326] add pDynamicValue -->
  <xsl:template name="CustomCaptureInfo">
    <xsl:param name ="pClientId"/>
    <xsl:param name ="pDataType"/>
    <xsl:param name ="pRegex"/>
    <xsl:param name ="pIsMandatory"/>
    <xsl:param name ="pIsMissingMode" select="$ConstFalse"/>
    <xsl:param name ="pValue"/>
    <xsl:param name ="pXMLDynamicValue"/>
    <xsl:param name ="pDynamicValue"/>
    <xsl:param name ="pIsMissingModeSpecified" select="$ConstFalse"/>
    <xsl:param name ="pIsAutoCreate"/>

    <customCaptureInfo clientId="{$pClientId}" dataType="{$pDataType}">
      <xsl:if test="string-length($pIsMandatory) > 0">
        <xsl:attribute name="mandatory">
          <xsl:value-of select ="$pIsMandatory"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="string-length($pRegex) > 0">
        <xsl:attribute name="regex">
          <xsl:value-of select ="$pRegex"/>
        </xsl:attribute>
      </xsl:if>
      <!-- FI 20130702 [18798]-->
      <xsl:if test="$pIsMissingModeSpecified=$ConstTrue">
        <xsl:attribute name="missingMode">
          <xsl:value-of select ="$pIsMissingMode"/>
        </xsl:attribute>
      </xsl:if>
      <!-- FI 20170404 [23039] -->
      <xsl:if test="string-length($pIsAutoCreate) > 0">
        <xsl:attribute name="isAutoCreate">
          <xsl:value-of select ="$pIsAutoCreate"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:choose>
        <xsl:when test="string-length($pValue) > 0">
          <value>
            <xsl:value-of select ="$pValue"/>
          </value >
        </xsl:when>
        <xsl:when test="string-length($pXMLDynamicValue) > 0">
          <xsl:choose>
            <!-- FI 20170404 [23039] Utilisation de msxsl:node-set -->
            <xsl:when test="msxsl:node-set($pXMLDynamicValue)/dynamicValue">
              <xsl:copy-of select ="$pXMLDynamicValue"/>
            </xsl:when>
            <xsl:otherwise>
              <dynamicValue>
                <xsl:copy-of select ="$pXMLDynamicValue"/>
              </dynamicValue >
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <!-- FI 20170718 [23326] add copy of $pDynamicValue lorsque renseigné -->
        <xsl:when test ="$pDynamicValue">
          <xsl:copy-of select ="$pDynamicValue"/>
        </xsl:when>
        <xsl:otherwise>
          <value/>
        </xsl:otherwise>
      </xsl:choose>
    </customCaptureInfo>
  </xsl:template>

  <!-- FI 20120704 [17991] Add template SQLMarket -->
  <!-- FI 20130222 [18439] Use FIXML_SECURITYEXCHANGE -->
  <!-- FI 20220304 [25699] Rem pResultColumn parameter -->
  <xsl:template name="SQLMarket">
    <xsl:param name="pMarket"/>
    <xsl:param name="pMarketColumn"/>
    <!--<xsl:param name="pResultColumn" select="'FIXML_SECURITYEXCHANGE'"/>-->
    <xsl:param name="pIsMandatory" select="$ConstFalse"/>

    <xsl:choose>
      <xsl:when test="string-length($pMarket) > 0 and string-length($pMarketColumn) > 0">
        <xsl:choose>
          <!--<xsl:when test="($pResultColumn = 'FIXML_SECURITYEXCHANGE' and $pMarketColumn = 'FIXML_SECURITYEXCHANGE')">-->
          <xsl:when test="$pMarketColumn = 'FIXML_SECURITYEXCHANGE'">
            <xsl:value-of select="$pMarket"/>
          </xsl:when>
          <xsl:otherwise>
            <!-- FI 20170404 [23039] call Template GetSQLMarket -->
            <xsl:call-template name="GetSQLMarket">
              <xsl:with-param name="pMarket" select ="$pMarket"/>
              <xsl:with-param name="pMarketColumn" select ="$pMarketColumn"/>
            <!--<xsl:with-param name="pResultColumn" select="'FIXML_SECURITYEXCHANGE'"/>-->
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="string-length($pMarket) > 0">
        <xsl:value-of select="concat($pMarket,' [',$ConstUnknown,']')"/>
      </xsl:when>
      <xsl:when test="$pIsMandatory=$ConstTrue">
        <xsl:value-of select="$ConstUnknown"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- FI 20170404 [23039] Add Template 
   Requête SQL qui permet d'obtenir le marché à partir de pMarket et pMarketColumn
  -->
  <!-- FI 20220304 [25699] Rem pResultColumn parameter -->
  <!-- FI 20240611 [XXXXX] Modification in LEVELSORT (verrue: si multiples valeurs sont retournées prendre en priorité les marchés Enable)  -->
  <xsl:template name ="GetSQLMarket">
    <xsl:param name="pMarket"/>
    <xsl:param name="pMarketColumn"/>
    <!--<xsl:param name="pResultColumn" select="'FIXML_SECURITYEXCHANGE'"/>-->

    <xsl:variable name ="vXMLExtlIdJoins">
      <xsl:call-template name="XMLJoinExtlId">
        <xsl:with-param name="pTableName" select="'MARKET'"/>
        <xsl:with-param name="pTableAlias" select="'m'"/>
        <xsl:with-param name="pJoinId" select="'m.IDM'"/>
        <xsl:with-param name="pValueColumn" select="$pMarketColumn"/>
        <xsl:with-param name="pValue" select="$pMarket"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name ="paramMarket">
      <Param name="MARKETVALUE" datatype="string">
        <xsl:value-of select="concat($pMarket,' [',$pMarketColumn,']')"/>
      </Param>
    </xsl:variable>

    <SQL command="select" result="FIXML_SECURITYEXCHANGE" cache="true">
      <xsl:text>
      <![CDATA[
      select isnull(DTDISABLED, convert(datetime,'19000101',112)) as LEVELSORT, m.FIXML_SECURITYEXCHANGE
      from dbo.VW_MARKET_IDENTIFIER m
      ]]>
      </xsl:text>
      <xsl:call-template name="SQLJoinExtlId">
        <xsl:with-param name="pXMLExtlIdJoins">
          <xsl:copy-of select="$vXMLExtlIdJoins"/>
        </xsl:with-param>
        <xsl:with-param name="pAdditionalSQL">
          <xsl:text>
            <![CDATA[
            union all
            select convert(datetime,'29990101',112) as LEVELSORT, @MARKETVALUE as FIXML_SECURITYEXCHANGE 
            from DUAL
            order by LEVELSORT
            ]]>
          </xsl:text>
        </xsl:with-param>
        <xsl:with-param name="pAdditionalParameter">
          <xsl:copy-of select="$paramMarket"/>
        </xsl:with-param>
      </xsl:call-template>
    </SQL>
  </xsl:template>

  <!-- Génère du code SQL pour récupérer l'identifier d'un marché à partir d'un marché en entrée, ou à partir d'un DC/Asset -->
  <!-- RD 20140324 [19704] Standard template -->
  <!-- FI 20120704 [17991] use template for ETD only -->
  <!-- FI 20130222 [18439] use FIXML_SECURITYEXCHANGE -->
  <!-- FI 20161005 [XXXXX] suppression du paramètre pIsDerivativeContractVersion CONTRACTATTRIBUTE est pris en considération uniquement sur Eurex -->
  <!-- FI 20170404 [23039] Modify -->
  <!-- FI 20220214 [25699] Add pDerivativeContractType-->
  <!-- FI 20220215 [25699] Add pClearingHouse and pClearingHouseColumn -->
  <!-- FI 20220225 [25699] Add pDerivativeContractSettltMethod and pDerivativeContractExerciseStyle -->
  <!-- FI 20220304 [25699] Rem pResultColumn parameter and Refactoring -->
  <xsl:template name="ETD_SQLMarket">
    <xsl:param name="pBusinessDate"/>
    <xsl:param name="pMarket"/>
    <xsl:param name="pMarketColumn"/>
    <xsl:param name="pAssetCode"/>
    <xsl:param name="pAssetColumn"/>
    <xsl:param name="pDerivativeContract"/>
    <xsl:param name="pDerivativeContractColumn"/>
    <xsl:param name="pDerivativeContractVersion"/>
    <!-- RD 20140324 [19704] To use ContractVersion or not (For BCS Gateway, not use it)-->
    <!-- FI 20161005 [XXXXX] Remove pIsDerivativeContractVersion parameter -->
    <!--<xsl:param name="pIsDerivativeContractVersion" select="$ConstTrue"/>-->
    <!-- FI 20170404 [23039] param name is pDerivativeContractCategory -->
    <xsl:param name="pDerivativeContractCategory"/>
    <xsl:param name="pDerivativeContractType"/>
    <xsl:param name="pDerivativeContractSettltMethod"/>
    <xsl:param name="pDerivativeContractExerciseStyle"/>
    <xsl:param name="pClearingHouse"/>
    <xsl:param name="pClearingHouseColumn"/>
    <!-- FI 20120213 [18439] pResultColumn FIXML_SECURITYEXCHANGE (le cci doit être alimenté avec FIXML_SECURITYEXCHANGE)-->
    <!--<xsl:param name="pResultColumn" select="'IDENTIFIER'"/>-->
    <!-- FI 20220304 [25699] rem pResultColumn parameter -->
    <!--<xsl:param name="pResultColumn" select="'FIXML_SECURITYEXCHANGE'"/>-->
    <xsl:param name="pIsMandatory" select="$ConstFalse"/>

    <xsl:choose>
      <xsl:when test="string-length($pMarket) > 0 and string-length($pMarketColumn) > 0">
        <!-- FI 20220304 [25699] call SQLMarket Template -->
        <xsl:call-template name="SQLMarket">
          <xsl:with-param name="pMarket" select="$pMarket"/>
          <xsl:with-param name="pMarketColumn" select="$pMarketColumn"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="string-length($pAssetCode) > 0 and string-length($pAssetColumn) > 0">
        <!-- FI 20220304 [25699] call ETD_SQLInstrumentAndAsset Template -->
          <xsl:variable name="vDynamicDataAssetIdentifier">
           <dynamicValue>
             <xsl:call-template name="ETD_SQLInstrumentAndAsset">
              <xsl:with-param name="pBusinessDate" select="$pBusinessDate"/>
              <xsl:with-param name="pAssetCode" select="$pAssetCode"/>
              <xsl:with-param name="pAssetColumn" select="$pAssetColumn"/>
              <xsl:with-param name="pClearingHouse" select="$pClearingHouse"/>
              <xsl:with-param name="pClearingHouseColumn" select="$pClearingHouseColumn"/>
              <xsl:with-param name="pResultColumn" select="'ASSET_IDENTIFIER'"/>
            </xsl:call-template>
           </dynamicValue>
        </xsl:variable>
        <SQL command="select" result="FIXML_SECURITYEXCHANGE" cache="true">
          <xsl:text>
          <![CDATA[
          select m.FIXML_SECURITYEXCHANGE
          from dbo.VW_MARKET_IDENTIFIER m
          inner join dbo.VW_ASSET_ETD_EXPANDED asset on asset.IDM = m.IDM and asset.IDENTIFIER=@ASSET_IDENTIFIER
          ]]>
          </xsl:text>
          <Param name="ASSET_IDENTIFIER" datatype="string">
            <xsl:copy-of select="msxsl:node-set($vDynamicDataAssetIdentifier)/dynamicValue/child::node()"/>
          </Param>
        </SQL>
      </xsl:when>
      <xsl:when test="string-length($pDerivativeContract) > 0 and string-length($pDerivativeContractColumn) > 0">
        <!-- FI 20220304 [25699] call SQLDerivativeContract Template -->
          <xsl:variable name="vDynamicDataDerivativeContractIdentifier">
          <dynamicValue>
            <xsl:call-template name="SQLDerivativeContract">
              <xsl:with-param name="pBusinessDate" select="$pBusinessDate"/>
              <xsl:with-param name="pDerivativeContract" select="$pDerivativeContract"/>
              <xsl:with-param name="pDerivativeContractColumn" select="$pDerivativeContractColumn"/>
              <xsl:with-param name="pDerivativeContractVersion" select="$pDerivativeContractVersion"/>
              <xsl:with-param name="pDerivativeContractCategory" select="$pDerivativeContractCategory"/>
              <xsl:with-param name="pDerivativeContractType" select="$pDerivativeContractType"/>
              <xsl:with-param name="pDerivativeContractSettltMethod" select="$pDerivativeContractSettltMethod"/>
              <xsl:with-param name="pDerivativeContractExerciseStyle" select="$pDerivativeContractExerciseStyle"/>
              <xsl:with-param name="pClearingHouse" select="$pClearingHouse"/>
              <xsl:with-param name="pClearingHouseColumn" select="$pClearingHouseColumn"/>
            </xsl:call-template>
          </dynamicValue>
        </xsl:variable>
        <SQL command="select" result="FIXML_SECURITYEXCHANGE" cache="true">
          <xsl:text>
          <![CDATA[
          select m.FIXML_SECURITYEXCHANGE
          from dbo.VW_MARKET_IDENTIFIER m
          inner join dbo.DERIVATIVECONTRACT dc on dc.IDM = m.IDM and dc.IDENTIFIER=@DC_IDENTIFIER
          ]]>
          </xsl:text>
          <Param name="DC_IDENTIFIER" datatype="string">
            <xsl:copy-of select="msxsl:node-set($vDynamicDataDerivativeContractIdentifier)/dynamicValue/child::node()"/>
          </Param>
        </SQL>
      </xsl:when>
      <xsl:when test="string-length($pMarket) > 0">
        <xsl:value-of select="concat($pMarket,' [',$ConstUnknown,']')"/>
      </xsl:when>
      <xsl:when test="$pIsMandatory=$ConstTrue">
        <xsl:value-of select="$ConstUnknown"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- Genere un noeud XML party et éventuellement un noued XML book-->
  <!-- FI add parameter pIsMissingModeSpecified-->
  <!-- FI 20170404 [23039] Modify -->
  <xsl:template name="XMLParty">
    <xsl:param name="pName"/>
    <xsl:param name="pIsMissingMode" select="$ConstFalse"/>
    <xsl:param name="pParsingValue"/>
    <xsl:param name="pXMLParsingValue"/>
    <xsl:param name="pActor"/>
    <xsl:param name="pBook" select="''"/>
    <xsl:param name="pIsMissingModeSpecified" select="$ConstFalse"/>
    <!--party Node -->
    <party name="{$pName}">
      <xsl:if test="$pIsMissingModeSpecified=$ConstTrue">
        <xsl:attribute name="isMissingMode">
          <xsl:value-of select ="$pIsMissingMode"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:choose>
        <xsl:when test ="string-length($pParsingValue) > 0">
          <parsingValue>
            <xsl:value-of select ="$pParsingValue"/>
          </parsingValue>
        </xsl:when>
        <xsl:otherwise>
          <xsl:copy-of select ="msxsl:node-set($pXMLParsingValue)/parsingValue"/>
        </xsl:otherwise>
      </xsl:choose>
      <dynamicValue>
        <xsl:copy-of select="$pActor"/>
      </dynamicValue>
    </party>
    <!--book Node -->
    <!-- FI 20170404 [23039] remove attribute parsingValue -->
    <xsl:if test="string-length($pBook) > 0">
      <book name="{$pName}" >
        <xsl:if test="$pIsMissingModeSpecified=$ConstTrue">
          <xsl:attribute name="isMissingMode">
            <xsl:value-of select ="$pIsMissingMode"/>
          </xsl:attribute>
        </xsl:if>
        <dynamicValue>
          <xsl:copy-of select ="$pBook"/>
        </dynamicValue>
      </book>
    </xsl:if>
  </xsl:template>

  <!-- FI 20170718 [23326] Add -->
  <!-- FI 20171103 [23039] Mod ajout des données MiFID/MIFIR-->
  <xsl:template name="XMLParty2">
    <xsl:param name="pName"/>
    <xsl:param name="pActorParsingNode" />
    <xsl:param name="pActor"/>
    <xsl:param name="pBookParsingNode" />
    <xsl:param name="pBook" />
    <xsl:param name="pTraderParsingNode" />
    <xsl:param name="pTrader"/>
    <xsl:param name="pTraderIsAutoCreate"/>
    <xsl:param name="pFolderParsingNode"/>
    <xsl:param name="pFolder"/>
    <xsl:param name="pFrontIdParsingNode" />
    <xsl:param name="pFrontId"/>
    <xsl:param name="pUTINode"/>
    <xsl:param name="pInvestmentDecisionParsingNode"/>
    <xsl:param name="pInvestmentDecisionActor"/>
    <xsl:param name="pExecutionParsingNode"/>
    <xsl:param name="pExecutionActor"/>
    <xsl:param name="pTradingCapacityNode"/>
    <xsl:param name="pWaivorIndicatorNode"/>
    <xsl:param name="pShortSailingIndicatorNode"/>
    <xsl:param name="pOTCPostTradeIndicatorNode"/>
    <xsl:param name="pCommodityDerivativeIndicatorNode"/>
    <xsl:param name="pSecuritiesFinancingIndicatorNode"/>


    <!-- party Node -->
    <xsl:if test="$pActorParsingNode or $pBookParsingNode">
      <party name="{$pName}">
        <xsl:if test ="$pActorParsingNode">
          <parsingValue>
            <xsl:value-of select ="$pActorParsingNode"/>
          </parsingValue>
        </xsl:if>
        <xsl:if test ="$pBookParsingNode">
          <parsingValue>
            <xsl:value-of select ="$pBookParsingNode"/>
          </parsingValue>
        </xsl:if>
        <dynamicValue>
          <xsl:copy-of select="$pActor"/>
        </dynamicValue>
      </party>
    </xsl:if>

    <!-- book Node -->
    <xsl:if test="$pBookParsingNode">
      <book name="{$pName}">
        <dynamicValue>
          <xsl:copy-of select ="$pBook"/>
        </dynamicValue>
      </book>
    </xsl:if>

    <!-- trader Node -->
    <xsl:if test="$pTraderParsingNode">
      <xsl:variable name ="vIsAutoCreate">
        <xsl:choose>
          <xsl:when test ="normalize-space($pTraderIsAutoCreate) = 'Y'">
            <xsl:value-of select="$ConstTrue"/>
          </xsl:when>
          <xsl:when test ="normalize-space($pTraderIsAutoCreate) = 'N'">
            <xsl:value-of select="$ConstFalse"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$ConstFalse"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <trader name="{$pName}" isAutoCreate="{$vIsAutoCreate}">
        <dynamicValue>
          <xsl:copy-of select ="$pTrader"/>
        </dynamicValue>
      </trader>
    </xsl:if>

    <!-- frontId Node -->
    <xsl:if test="$pFrontIdParsingNode">
      <frontId name="{$pName}">
        <dynamicValue>
          <xsl:copy-of select ="$pFrontId"/>
        </dynamicValue>
      </frontId>
    </xsl:if>

    <!-- folder Node -->
    <xsl:if test="$pFolderParsingNode">
      <folder name="{$pName}">
        <dynamicValue>
          <xsl:copy-of select ="$pFolder"/>
        </dynamicValue>
      </folder>
    </xsl:if>

    <!-- Emir-->
    <!-- Unique Trade Identifier -->
    <xsl:if test="$pUTINode">
      <uti name="{$pName}">
        <dynamicValue>
          <xsl:copy-of select ="$pUTINode/text()"/>
        </dynamicValue>
      </uti>
    </xsl:if>

    <!-- Mifir-->
    <xsl:if test="$pInvestmentDecisionParsingNode">
      <investmentDecisionWithinFirm name="{$pName}">
        <parsingValue>
          <xsl:value-of select ="$pInvestmentDecisionParsingNode"/>
        </parsingValue>
        <dynamicValue>
          <xsl:copy-of select ="$pInvestmentDecisionActor"/>
        </dynamicValue>
      </investmentDecisionWithinFirm>
    </xsl:if>
    <xsl:if test="$pExecutionParsingNode">
      <executionWithinFirm name="{$pName}">
        <parsingValue>
          <xsl:value-of select ="$pExecutionParsingNode"/>
        </parsingValue>
        <dynamicValue>
          <xsl:copy-of select ="$pExecutionActor"/>
        </dynamicValue>
      </executionWithinFirm>
    </xsl:if>
    <xsl:if test="$pTradingCapacityNode">
      <tradingCapacity name="{$pName}">
        <dynamicValue>
          <xsl:copy-of select ="$pTradingCapacityNode/text()"/>
        </dynamicValue>
      </tradingCapacity>
    </xsl:if>
    <xsl:if test="$pWaivorIndicatorNode">
      <waiverIndicator name="{$pName}">
        <dynamicValue>
          <xsl:copy-of select ="$pWaivorIndicatorNode/text()"/>
        </dynamicValue>
      </waiverIndicator>
    </xsl:if>
    <xsl:if test="$pShortSailingIndicatorNode">
      <shortSailingIndicator name="{$pName}">
        <dynamicValue>
          <xsl:copy-of select ="$pShortSailingIndicatorNode/text()"/>
        </dynamicValue>
      </shortSailingIndicator>
    </xsl:if>
    <xsl:if test="$pOTCPostTradeIndicatorNode">
      <otcPostTradeIndicator name="{$pName}">
        <dynamicValue>
          <xsl:copy-of select ="$pOTCPostTradeIndicatorNode/text()"/>
        </dynamicValue>
      </otcPostTradeIndicator>
    </xsl:if>
    <xsl:if test="$pCommodityDerivativeIndicatorNode">
      <commodityDerivativeIndicator name="{$pName}">
        <dynamicValue>
          <xsl:copy-of select ="$pCommodityDerivativeIndicatorNode/text()"/>
        </dynamicValue>
      </commodityDerivativeIndicator>
    </xsl:if>
    <xsl:if test="$pSecuritiesFinancingIndicatorNode">
      <securitiesFinancing name="{$pName}">
        <dynamicValue>
          <xsl:copy-of select ="$pSecuritiesFinancingIndicatorNode/text()"/>
        </dynamicValue>
      </securitiesFinancing>
    </xsl:if>


  </xsl:template>
  
  <!-- 
  RD 20140409 [19816] Template wich can be overrided by specific xsl
  -->
  <!-- Génère du code SQL pour récupérer un book -->
  <!-- Génère du code SQL pour récupérer un le propriétaire d'un book -->
  <!-- Lorsque la donnée en entrée est vide, permet d'appliquer une valeur par défaut -->
  <xsl:template name="ovrSQLDealerActorAndBook">
    <xsl:param name="pBuyerOrSellerAccount"/>
    <xsl:param name="pBuyerOrSellerAccountIdent"/>
    <xsl:param name="pResultColumn"/>
    <xsl:param name="pIsAllowDataMissDealer"/>
    <xsl:param name="pDefaultValue"/>

    <xsl:call-template name="SQLActorAndBook">
      <xsl:with-param name="pValue" select="$pBuyerOrSellerAccount"/>
      <xsl:with-param name="pValueIdent" select="$pBuyerOrSellerAccountIdent"/>
      <xsl:with-param name="pResultColumn" select="$pResultColumn"/>
      <xsl:with-param name="pUseDefaultValue" select ="$pIsAllowDataMissDealer"/>
      <xsl:with-param name="pDefaultValue" select ="$pDefaultValue"/>
    </xsl:call-template>
  </xsl:template>

  <!-- 
  FI 20170404 [23039] Add Template => Recherche du dealer 
  -->
  <!-- Génère du code SQL pour récupérer un Acteur -->
  <!-- Lorsque la donnée en entrée est vide, permet d'appliquer une valeur par défaut -->
  <xsl:template name="ovrSQLDealerActor">
    <xsl:param name="pBuyerOrSeller"/>
    <xsl:param name="pBuyerOrSellerIdent"/>
    <xsl:param name="pResultColumn"/>
    <xsl:param name="pIsAllowDataMissDealer"/>
    <xsl:param name="pDefaultValue"/>

    <xsl:variable name ="vActorColumnName">
      <xsl:call-template name="GetActorIdentColumn">
        <xsl:with-param name="pIdent" select="$pBuyerOrSellerIdent"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="string-length($pBuyerOrSeller) > 0 and string-length($vActorColumnName) > 0">
        <xsl:choose>
          <xsl:when test="($pResultColumn = 'IDENTIFIER' and $vActorColumnName = 'IDENTIFIER')">
            <xsl:value-of select="$pBuyerOrSeller"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:variable name ="vXMLExtlIdJoins">
              <xsl:call-template name="XMLJoinExtlId">
                <xsl:with-param name="pTableName" select="'ACTOR'"/>
                <xsl:with-param name="pTableAlias" select="'a'"/>
                <xsl:with-param name="pJoinId" select="'a.IDA'"/>
                <xsl:with-param name="pValueColumn" select="$vActorColumnName"/>
                <xsl:with-param name="pValue" select="$pBuyerOrSeller"/>
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="vAdditionalParameter">
              <xsl:choose>
                <xsl:when test="string-length($pDefaultValue) > 0">
                  <Param name="UNKNOWN" datatype="string">
                    <xsl:value-of select="$pDefaultValue"/>
                  </Param>
                </xsl:when>
                <xsl:otherwise>
                  <Param name="UNKNOWN" datatype="string">
                    <xsl:value-of select="$ConstUnknown"/>
                  </Param>
                </xsl:otherwise>
              </xsl:choose>
              <Param name="ACTORVALUE" datatype="string">
                <xsl:value-of select="concat($pBuyerOrSeller,' [',$vActorColumnName,']')"/>
              </Param>
            </xsl:variable>
            <SQL command="select" result="{$pResultColumn}" cache="true">
              <xsl:text>
              <![CDATA[
              select '0' as LEVELSORT,a.IDENTIFIER
              from dbo.ACTOR a
              ]]>
              </xsl:text>
              <xsl:call-template name="SQLJoinExtlId">
                <xsl:with-param name="pXMLExtlIdJoins">
                  <xsl:copy-of select="$vXMLExtlIdJoins"/>
                </xsl:with-param>
                <xsl:with-param name="pAdditionalSQL">
                  <xsl:text>
                  <![CDATA[
                  union all
                  select '1' as LEVELSORT,@UNKNOWN as IDENTIFIER
                  from DUAL
                  order by LEVELSORT
                  ]]>
                  </xsl:text>
                </xsl:with-param>
                <xsl:with-param name="pAdditionalParameter">
                  <xsl:copy-of select="$vAdditionalParameter"/>
                </xsl:with-param>
              </xsl:call-template>
            </SQL>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>

      <xsl:when test="string-length($pBuyerOrSeller) > 0">
        <!-- Spheres® rentre ici si l'identification est inconnue -->
        <xsl:value-of select="concat($pBuyerOrSeller,' [',$ConstUnknown,']')"/>
      </xsl:when>

      <xsl:when test="$pIsAllowDataMissDealer=$ConstTrue">
        <xsl:choose>
          <xsl:when test="string-length($pDefaultValue) > 0">
            <xsl:value-of select="$pDefaultValue"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$ConstUnknown"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
    </xsl:choose>

  </xsl:template>
  
  <!-- 
  RD 20140409 [19816] Template wich can be overrided by specific xsl
  -->
  <!-- Génère du code SQL pour récupérer la ClearingOrganization -->
  <!-- FI 20160929 [22507] Modify (Chgt de signature: Add pAssetXXX, pDerivativeContractXXX, pCategory, pDefaultMode and Remove pResultColumn) -->
  <!-- FI 20170404 [23039] Modify (Chgt de signature: pFamily, pMarket et pMarketColumn) -->
  <!-- FI 20220214 [25699] Add pDerivativeContractType -->
  <!-- FI 20220215 [25699] Add pClearingHouse and pClearingHouseColumn -->
  <!-- FI 20220225 [25699] Add pDerivativeContractSettltMethod and pDerivativeContractExerciseStyle -->
  <xsl:template name="ovrSQLClearingOrganization">
    <xsl:param name="pClearingOrganization"/>
    <xsl:param name="pClearingOrganizationIdent"/>
    <xsl:param name="pClearingOrganizationAccount"/>
    <xsl:param name="pClearingOrganizationAccountIdent"/>
    <xsl:param name="pClearingBusinessDate"/>
    <xsl:param name="pFamily"/>
    <xsl:param name="pMarket"/>
    <xsl:param name="pMarketColumn"/>
    <xsl:param name="pAssetCode"/>
    <xsl:param name="pAssetColumn"/>
    <xsl:param name="pDerivativeContract"/>
    <xsl:param name="pDerivativeContractColumn"/>
    <xsl:param name="pDerivativeContractVersion"/>
    <xsl:param name="pDerivativeContractCategory"/>
    <xsl:param name="pDerivativeContractType"/>
    <xsl:param name="pDerivativeContractSettltMethod"/>
    <xsl:param name="pDerivativeContractExerciseStyle"/>
    <xsl:param name="pClearingHouse"/>
    <xsl:param name="pClearingHouseColumn"/>
    <xsl:param name="pBuyerOrSellerAccount"/>
    <xsl:param name="pDefaultMode" select ="'DefaultFromBook'"/>
    <xsl:choose>
      <!-- Si la ClearingOrganisation est spécifiée, on la charge -->
      <xsl:when test="string-length($pClearingOrganization) > 0 and string-length($pClearingOrganizationIdent) > 0">
        <xsl:call-template name="SQLActor">
          <xsl:with-param name="pValue" select="$pClearingOrganization"/>
          <xsl:with-param name="pValueIdent" select="$pClearingOrganizationIdent"/>
          <!-- FI 20160929 [22507] "IDENTIFIER" -->
          <!--<xsl:with-param name="pResultColumn" select="$pResultColumn"/>-->
          <xsl:with-param name="pResultColumn" select="IDENTIFIER"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <xsl:when test ="$pDefaultMode = 'DefaultFromMarket'">
            <!-- Recherche de la chambre  à partir du marché -->
            <!-- FI 20170404 [23039] Appel à GetDynamicDataMarketIdentifier et GetDynamicDataCssIdentifier -->
            <xsl:variable name ="vDynamicDataMarketIdentifier">
              <xsl:call-template name="GetDynamicDataMarketIdentifier">
                <xsl:with-param name ="pFamily" select="$pFamily"/>
                <xsl:with-param name ="pBusinessDate" select="$pClearingBusinessDate"/>
                <xsl:with-param name ="pMarket" select="$pMarket"/>
                <xsl:with-param name ="pMarketColumn" select="$pMarketColumn"/>
                <xsl:with-param name ="pAssetCode" select="$pAssetCode"/>
                <xsl:with-param name ="pAssetColumn" select="$pAssetColumn"/>
                <xsl:with-param name ="pContrat" select="$pDerivativeContract"/>
                <xsl:with-param name ="pContractColumn" select="$pDerivativeContractColumn"/>
                <xsl:with-param name ="pContractVersion" select="$pDerivativeContractVersion"/>
                <xsl:with-param name ="pContractCategory" select="$pDerivativeContractCategory"/>
                <xsl:with-param name ="pContractType" select="$pDerivativeContractType"/>
                <xsl:with-param name ="pContractSettltMethod" select="$pDerivativeContractSettltMethod"/>
                <xsl:with-param name ="pContractExerciseStyle" select="$pDerivativeContractExerciseStyle"/>
                <xsl:with-param name ="pClearingHouse" select="$pClearingHouse"/>
                <xsl:with-param name ="pClearingHouseColumn" select="$pClearingHouseColumn"/>
              </xsl:call-template>
            </xsl:variable>
            <!-- 20170718 [23326] Utilisation de la variable vDynamicDataCssIdentifier -->
            <xsl:variable name ="vDynamicDataCssIdentifier">
              <xsl:call-template name ="GetDynamicDataCssIdentifier">
                <xsl:with-param name="pDynamicDataMarketIdentifier">
                  <xsl:copy-of select ="$vDynamicDataMarketIdentifier"/>
                </xsl:with-param>
              </xsl:call-template>
            </xsl:variable>
            <xsl:copy-of select="msxsl:node-set($vDynamicDataCssIdentifier)/dynamicValue/child::node()"/>
          </xsl:when>
          <xsl:when test ="$pDefaultMode = 'DefaultFromBook'">
            <!-- Recherche de l'acteur propriétiare du book -->
            <xsl:call-template name="SQLClearingActorAndBook">
              <xsl:with-param name="pValue" select="$pClearingOrganizationAccount"/>
              <xsl:with-param name="pValueIdent" select="$pClearingOrganizationAccountIdent"/>
              <xsl:with-param name="pResultColumn" select="IDENTIFIER"/>
            </xsl:call-template>
          </xsl:when>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Génère du code SQL pour récupérer le Book de la ClearingOrganization à partir d'un book en entrée-->
  <!-- RD 20140409 [19816] Template wich can be overrided by specific xsl-->
  <!-- FI 20160929 [22507] Modify (Chgt de signature : Remove pResultColumn)-->
  <xsl:template name="ovrSQLClearingOrganizationBook">
    <xsl:param name="pClearingOrganizationAccount"/>
    <xsl:param name="pClearingOrganizationAccountIdent"/>
    <xsl:param name="pBuyerOrSellerAccount"/>

    <!--<xsl:call-template name="SQLActorAndBook">
      <xsl:with-param name="pValue" select="$pClearingOrganizationAccount"/>
      <xsl:with-param name="pValueIdent" select="$pClearingOrganizationAccountIdent"/>
      <xsl:with-param name="pResultColumn" select="$pResultColumn"/>
    </xsl:call-template>-->

    <!-- FI 20160929 [22507] Call Template SQLClearingActorAndBook -->
    <xsl:call-template name="SQLClearingActorAndBook">
      <xsl:with-param name="pValue" select="$pClearingOrganizationAccount"/>
      <xsl:with-param name="pValueIdent" select="$pClearingOrganizationAccountIdent"/>
      <xsl:with-param name="pResultColumn" select="'BOOK_IDENTIFIER'"/>
    </xsl:call-template>

  </xsl:template>

  <!-- Génère du code SQL pour récupérer un book à partir d'un book en entrée-->
  <!-- Génère du code SQL pour récupérer le propriétaire d'un book à partir d'un book en entrée-->
  <!-- Lorsque la donnée en entrée est vide, permet d'appliquer une valeur par défaut -->
  <xsl:template name="SQLActorAndBook">
    <xsl:param name="pValue"/>
    <xsl:param name="pValueIdent"/>
    <xsl:param name="pResultColumn" select="'IDENTIFIER'"/>
    <xsl:param name="pUseDefaultValue" select="$ConstFalse"/>
    <xsl:param name="pDefaultValue"/>

    <xsl:variable name ="vBookColumnName">
      <xsl:call-template name="GetBookIdentColumn">
        <xsl:with-param name="pIdent">
          <xsl:copy-of select="$pValueIdent"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="string-length($pValue) > 0 and string-length($vBookColumnName) > 0">
        <xsl:choose>
          <xsl:when test="($pResultColumn = 'BOOK_IDENTIFIER' and $vBookColumnName = 'IDENTIFIER')">
            <xsl:value-of select="$pValue"/>
          </xsl:when>
          <xsl:otherwise>
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
                  <xsl:value-of select="$pValue"/>
                </xsl:with-param>
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="vAdditionalParameter">
              <xsl:choose>
                <xsl:when test="string-length($pDefaultValue) > 0">
                  <Param name="UNKNOWN" datatype="string">
                    <xsl:value-of select="$pDefaultValue"/>
                  </Param>
                </xsl:when>
                <xsl:otherwise>
                  <Param name="UNKNOWN" datatype="string">
                    <xsl:value-of select="$ConstUnknown"/>
                  </Param>
                </xsl:otherwise>
              </xsl:choose>
              <Param name="BOOKVALUE" datatype="string">
                <xsl:value-of select="concat($pValue,' [',$vBookColumnName,']')"/>
              </Param>
            </xsl:variable>
            <SQL command="select" result="{$pResultColumn}" cache="true">
              <xsl:text>
              <![CDATA[
              select '0' as LEVELSORT,a.IDENTIFIER,b.IDENTIFIER as BOOK_IDENTIFIER
              from dbo.ACTOR a
              inner join dbo.BOOK b on b.IDA=a.IDA
              ]]>
              </xsl:text>
              <xsl:call-template name="SQLJoinExtlId">
                <xsl:with-param name="pXMLExtlIdJoins">
                  <xsl:copy-of select="$vXMLExtlIdJoins"/>
                </xsl:with-param>
                <xsl:with-param name="pAdditionalSQL">
                  <xsl:text>
                  <![CDATA[
                  union all
                  select '1' as LEVELSORT,@UNKNOWN as IDENTIFIER,@BOOKVALUE as BOOK_IDENTIFIER 
                  from DUAL
                  order by LEVELSORT
                  ]]>
                  </xsl:text>
                </xsl:with-param>
                <xsl:with-param name="pAdditionalParameter">
                  <xsl:copy-of select="$vAdditionalParameter"/>
                </xsl:with-param>
              </xsl:call-template>
            </SQL>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>

      <xsl:when test="string-length($pValue) > 0">
        <!-- Spheres® rentre ici si l'identification est inconnue => vBookColumnName est vide -->
        <xsl:text>'</xsl:text>
        <xsl:value-of select="concat($pValue,' [',$ConstUnknown,']')"/>
        <xsl:text>'</xsl:text>
      </xsl:when>

      <xsl:when test="$pUseDefaultValue=$ConstTrue">
        <xsl:choose>
          <xsl:when test="string-length($pDefaultValue) > 0">
            <xsl:value-of select="$pDefaultValue"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$ConstUnknown"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- Génère du code SQL pour récupérer l'identifier d'un acteur -->
  <!-- Lorsque la donnée en entrée est vide, permet d'appliquer la valeur par défaut {unknown}-->
  <xsl:template name="SQLActor">
    <xsl:param name="pValue"/>
    <xsl:param name="pValueIdent"/>
    <xsl:param name="pUseDefaultValue" select="$ConstFalse"/>

    <xsl:variable name ="vActorColumn">
      <xsl:call-template name="GetActorIdentColumn">
        <xsl:with-param name="pIdent">
          <xsl:copy-of select="$pValueIdent"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    <xsl:call-template name="SQLActorBase">
      <xsl:with-param name="pValue" select="$pValue"/>
      <xsl:with-param name="pActorColumn" select="$vActorColumn"/>
      <xsl:with-param name="pUseDefaultValue" select="$pUseDefaultValue"/>
    </xsl:call-template>

  </xsl:template>

  <!-- FI 20190904 [24882] Add -->
  <!-- FI 20201015 [25532] Mod Add parameter @CSSMEMBERCODE --> 
  <!-- SQLActor2 gère MemberId-ZZZ -->
  <!-- Génère du code SQL pour récupérer l'identifier d'un acteur -->
  <xsl:template name="SQLActor2">
    <xsl:param name="pValue"/>
    <xsl:param name="pValueIdent"/>
    <xsl:param name="pDynamicDataCssIdentifier"/>
    <xsl:param name="pDynamicDataMarketIdentifier"/>
    <xsl:param name="pClearingBusinessDate"/>
    <xsl:param name="pClearingMemberType"/>
    <xsl:param name="pClearingMemberType2" select ="''"/>
    <xsl:param name="pClearingMemberType3" select ="''"/>
    <xsl:choose>
      <xsl:when test="starts-with($pValueIdent,$ConstMemberId)">
        <xsl:variable name ="vDynamicDataCssIdentifierNode" select="msxsl:node-set($pDynamicDataCssIdentifier)"/>
        <xsl:variable name ="vDynamicDataMarketIdentifierNode" select="msxsl:node-set($pDynamicDataMarketIdentifier)"/>
        <SQL command="select" result="IDENTIFIER" cache="true">
          <![CDATA[
              select '0' as LEVELSORT, a.IDENTIFIER, isnull(csmId.IDM,-1) as IDM
              from dbo.ACTOR a
              inner join dbo.CSMID csmId on csmId.IDA = a.IDA 
              inner join dbo.ACTOR acss on acss.IDA = csmId.IDA_CSS and acss.IDENTIFIER = @CSSIDENTIFIER
              left outer join dbo.VW_MARKET_IDENTIFIER m on m.IDM = csmId.IDM
              ]]>
              <xsl:call-template name="SQLDTENABLEDDTDISABLED">
                <xsl:with-param name="pTable" select="'m'"/>
              </xsl:call-template>
              <![CDATA[
              where (csmid.CSSMEMBERCODE = @CSSMEMBERCODE) and (csmId.CSSMEMBERIDENT = @CSSMEMBERIDENT) 
              and (csmId.IDM is null or m.FIXML_SECURITYEXCHANGE = @MARKETIDENTIFIER)
              and (csmId.CLEARINGMEMBERTYPE is null or csmId.CLEARINGMEMBERTYPE = @CLEARINGMEMBERTYPE
              ]]>
           <!-- FI 20191024 [25034] @CLEARINGMEMBERTYPE2-->  
           <xsl:if test="string-length($pClearingMemberType2) > 0">
              <![CDATA[ or csmId.CLEARINGMEMBERTYPE = @CLEARINGMEMBERTYPE2 ]]>
            </xsl:if>
            <!-- FI 20191024 [25034] @CLEARINGMEMBERTYPE3-->
            <xsl:if test="string-length($pClearingMemberType3) > 0">
              <![CDATA[ or csmId.CLEARINGMEMBERTYPE = @CLEARINGMEMBERTYPE3 ]]>
            </xsl:if>  
            <![CDATA[)]]>
            <xsl:call-template name="SQLDTENABLEDDTDISABLED">
                <xsl:with-param name="pTable" select="'a'"/>
            </xsl:call-template>
            <xsl:call-template name="SQLDTENABLEDDTDISABLED">
                <xsl:with-param name="pTable" select="'csmId'"/>
            </xsl:call-template>
          <![CDATA[
                union all
                select '1' as LEVELSORT,@ACTORVALUE as IDENTIFIER, -1 as IDM  
                from DUAL
                order by LEVELSORT asc, IDM desc
          ]]>
          <Param datatype="string" name="CSSIDENTIFIER">
            <xsl:copy-of select="$vDynamicDataCssIdentifierNode/dynamicValue/child::node()"/>
          </Param>    
          <Param datatype="string" name="MARKETIDENTIFIER">
            <xsl:copy-of select="$vDynamicDataMarketIdentifierNode/dynamicValue/child::node()"/>
          </Param>  
          <Param datatype="string" name="CLEARINGMEMBERTYPE">
            <xsl:value-of select="$pClearingMemberType"/>
          </Param> 
          <xsl:if test="string-length($pClearingMemberType2) > 0">
            <Param datatype="string" name="CLEARINGMEMBERTYPE2">
            <xsl:value-of select="$pClearingMemberType2"/>
            </Param> 
          </xsl:if>
          <xsl:if test="string-length($pClearingMemberType3) > 0">
            <Param datatype="string" name="CLEARINGMEMBERTYPE3">
            <xsl:value-of select="$pClearingMemberType3"/>
            </Param> 
          </xsl:if>
          <!-- RD 20220421 [25938] CSSMEMBERCODE-->
          <Param datatype="string" name="CSSMEMBERCODE">
            <xsl:value-of select="$pValue"/>
          </Param>
          <Param datatype="string" name="CSSMEMBERIDENT">
            <xsl:value-of select="substring-after($pValueIdent, $ConstMemberId)"/>
          </Param>    
          <Param name="DT" datatype="date">
            <xsl:value-of select="$pClearingBusinessDate"/>
          </Param>
          <Param name="ACTORVALUE" datatype="string">
               <xsl:value-of select="concat($pValue,' [',$pValueIdent,']')"/>
           </Param>
          </SQL>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="SQLActor">
          <xsl:with-param name="pValue" select="$pValue"/>
          <xsl:with-param name="pValueIdent" select="$pValueIdent"/>
          <xsl:with-param name="pUseDefaultValue" select="$ConstFalse"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <!-- Génère du code SQL pour récupérer l'identifier d'un acteur -->
  <!-- Lorsque la donnée en entrée est vide, permet d'appliquer la valeur par défaut {unknown}-->
  <xsl:template name="SQLActorBase">
    <xsl:param name="pValue"/>
    <xsl:param name="pActorColumn"/>
    <xsl:param name="pUseDefaultValue" select="$ConstFalse"/>

    <xsl:choose>
      <xsl:when test="string-length($pValue) > 0 and string-length($pActorColumn) > 0">
        <xsl:choose>
          <xsl:when test="($pActorColumn = 'IDENTIFIER')">
            <xsl:value-of select="$pValue"/>
          </xsl:when>
          <xsl:otherwise>
            <SQL command="select" result="IDENTIFIER" cache="true">
              <xsl:text>
              <![CDATA[
              select '0' as LEVELSORT,a.IDENTIFIER
              from dbo.ACTOR a
              ]]>
              </xsl:text>
              <xsl:variable name ="vXMLExtlIdJoins">
                <xsl:call-template name="XMLJoinExtlId">
                  <xsl:with-param name="pTableName" select="'ACTOR'"/>
                  <xsl:with-param name="pTableAlias" select="'a'"/>
                  <xsl:with-param name="pJoinId" select="'a.IDA'"/>
                  <xsl:with-param name="pValueColumn" select="$pActorColumn"/>
                  <xsl:with-param name="pValue" select="$pValue"/>
                </xsl:call-template>
              </xsl:variable>
              <xsl:variable name ="paramActor">
                <Param name="ACTORVALUE" datatype="string">
                  <xsl:value-of select="concat($pValue,' [',$pActorColumn,']')"/>
                </Param>
              </xsl:variable>
              <xsl:call-template name="SQLJoinExtlId">
                <xsl:with-param name="pXMLExtlIdJoins">
                  <xsl:copy-of select="$vXMLExtlIdJoins"/>
                </xsl:with-param>
                <xsl:with-param name="pAdditionalSQL">
                  <xsl:text>
                  <![CDATA[
                  union all
                  select '1' as LEVELSORT,@ACTORVALUE as IDENTIFIER 
                  from DUAL
                  order by LEVELSORT
                  ]]>
                  </xsl:text>
                </xsl:with-param>
                <xsl:with-param name="pAdditionalParameter">
                  <xsl:copy-of select='$paramActor'/>
                </xsl:with-param>
              </xsl:call-template>
            </SQL>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="string-length($pValue) > 0">
        <!-- Spheres® rentre ici si l'identification est inconnue => pActorColumn est vide -->
        <xsl:value-of select="concat($pValue,' [',$ConstUnknown,']')"/>
      </xsl:when>
      <xsl:when test="$pUseDefaultValue=$ConstTrue">
        <xsl:value-of select="$ConstUnknown"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  
  <!-- Génère du code SQL pour récupérer l'identifier d'un asset Equity -->
  <!-- FI 20170404 [23039] Add parameters pMarket, pMarketColumn   -->
  <!-- FI 20220304 [25699] rem pResultColumn parameter -->
  <xsl:template name="ESE_SQLAsset">
    <xsl:param name="pAssetCode"/>
    <xsl:param name="pAssetColumn"/>
    <xsl:param name="pMarket"/>
    <xsl:param name="pMarketColumn"/>
    <!--<xsl:param name="pResultColumn" select="'IDENTIFIER'"/>-->
    <xsl:param name="pIsMandatory" select="$ConstFalse"/>
    <xsl:choose>
      <xsl:when test="string-length($pAssetCode) > 0 and string-length($pAssetColumn) > 0">
        <xsl:choose>
          <!--<xsl:when test="($pResultColumn = 'IDENTIFIER' and $pAssetColumn = 'IDENTIFIER')">-->
          <xsl:when test="$pAssetColumn = 'IDENTIFIER'">
            <xsl:value-of select="$pAssetCode"/>
          </xsl:when>
          <xsl:otherwise>
            <SQL command="select" result="IDENTIFIER" cache="true">
              <!-- FI 20170404 [23039] Uilisation de VW_ASSET pour accéder à ASSETSYMBOL -->
              <xsl:text>
              <![CDATA[
                select '0' as LEVELSORT,a.IDENTIFIER
                from (select a.IDASSET, a.IDENTIFIER, a.DISPLAYNAME, a.EXTLLINK, a.IDM, a.ASSETSYMBOL, a.ISINCODE, a.AIICODE
                             from dbo.VW_ASSET a
                             where a.ASSETCATEGORY = 'EquityAsset') a
              ]]>
              </xsl:text>
              <xsl:if test="string-length($pMarket) > 0 and string-length($pMarketColumn) > 0">
                <xsl:text>
                <![CDATA[
                  inner join dbo.VW_MARKET_IDENTIFIER m on m.IDM=a.IDM
                ]]>
                </xsl:text>
              </xsl:if>
              <xsl:variable name ="vXMLExtlIdJoins">
                <xsl:call-template name="XMLJoinExtlId">
                  <xsl:with-param name="pTableName" select="'ASSET_EQUITY'"/>
                  <xsl:with-param name="pTableAlias" select="'a'"/>
                  <xsl:with-param name="pJoinId" select="'a.IDASSET'"/>
                  <xsl:with-param name="pValueColumn" select="$pAssetColumn"/>
                  <xsl:with-param name="pValue" select="$pAssetCode"/>
                </xsl:call-template>
                <xsl:if test="string-length($pMarket) > 0 and string-length($pMarketColumn) > 0">
                  <xsl:call-template name="XMLJoinExtlId">
                    <xsl:with-param name="pTableName" select="'MARKET'"/>
                    <xsl:with-param name="pTableAlias" select="'m'"/>
                    <xsl:with-param name="pJoinId" select="'m.IDM'"/>
                    <xsl:with-param name="pValueColumn" select="$pMarketColumn"/>
                    <xsl:with-param name="pValue" select="$pMarket"/>
                  </xsl:call-template>
                </xsl:if>
              </xsl:variable>
              <xsl:variable name ="paramAsset">
                <Param name="ASSETVALUE" datatype="string">
                  <xsl:value-of select="concat($pAssetCode,' [',$pAssetColumn,']')"/>
                </Param>
              </xsl:variable>
              <xsl:call-template name="SQLJoinExtlId">
                <xsl:with-param name="pXMLExtlIdJoins">
                  <xsl:copy-of select="$vXMLExtlIdJoins"/>
                </xsl:with-param>
                <xsl:with-param name="pAdditionalSQL">
                  <xsl:text>
                  <![CDATA[
                    union all
                    select '1' as LEVELSORT,@ASSETVALUE as IDENTIFIER 
                    from DUAL
                    order by LEVELSORT
                  ]]>
                  </xsl:text>
                </xsl:with-param>
                <xsl:with-param name="pAdditionalParameter">
                  <xsl:copy-of select='$paramAsset'/>
                </xsl:with-param>
              </xsl:call-template>
            </SQL>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="string-length($pAssetCode) > 0">
        <xsl:value-of select="concat($pAssetCode,' [',$ConstUnknown,']')"/>
      </xsl:when>
      <xsl:when test="$pIsMandatory=$ConstTrue">
        <xsl:value-of select="$ConstUnknown"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="SQLEnumValue2">
    <xsl:param name="pCode"/>
    <xsl:param name="pExtValue"/>
    <xsl:if test="string-length($pExtValue) > 0">
      <SpheresLib function="GetEnumValue()" cache="true">
        <Param name="CODE" datatype="string">
          <xsl:value-of select="$pCode"/>
        </Param>
        <Param name="DATA" datatype="string">
          <xsl:value-of select="$pExtValue"/>
        </Param>
      </SpheresLib>
    </xsl:if>
  </xsl:template>

  <!-- Génère du code SQL pour récupérer l'identifier d'un DC -->
  <!-- RD 20140324 [19704] Standard template-->
  <!-- FI 20161005 [XXXXX] suppression du paramètre pIsDerivativeContractVersion CONTRACTATTRIBUTE est pris en considération uniquement sur Eurex -->
  <!-- FI 20220214 [25699] Add pDerivativeContractType -->
  <!-- FI 20220215 [25699] Add pClearingHouse and pClearingHouseColumn -->
  <!-- FI 20220304 [25699] rem pResultColumn parameter -->
  <xsl:template name="SQLDerivativeContract">
    <xsl:param name="pBusinessDate"/>
    <xsl:param name="pDerivativeContract"/>
    <xsl:param name="pDerivativeContractColumn"/>
    <xsl:param name="pDerivativeContractVersion"/>
    <!-- RD 20140324 [19704] To use ContractVersion or not (For BCS Gateway, not use it)-->
    <!--<xsl:param name="pIsDerivativeContractVersion" select="$ConstTrue"/>-->
    <xsl:param name="pDerivativeContractCategory"/>
    <xsl:param name="pDerivativeContractType"/>
    <xsl:param name="pDerivativeContractSettltMethod"/>
    <xsl:param name="pDerivativeContractExerciseStyle"/>
    <xsl:param name="pMarket"/>
    <xsl:param name="pMarketColumn"/>
    <xsl:param name="pClearingHouse"/>
    <xsl:param name="pClearingHouseColumn"/>
    <!--<xsl:param name="pResultColumn" select="'IDENTIFIER'"/>-->
    <xsl:param name="pIsMandatory" select="$ConstFalse"/>

    <xsl:choose>
      <xsl:when test="string-length($pDerivativeContract) > 0 and string-length($pDerivativeContractColumn) > 0">
        <xsl:choose>
          <!--<xsl:when test="($pResultColumn = 'IDENTIFIER' and $pDerivativeContractColumn = 'IDENTIFIER')">-->
          <xsl:when test="$pDerivativeContractColumn = 'IDENTIFIER'">
            <xsl:value-of select="$pDerivativeContract"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:variable name="vAdditionalCondition1">
              <!-- FI 20161005 [XXXXX] CONTRACTATTRIBUTE est pris en considération uniquement sur Eurex  -->
              <!--<xsl:if test="$pIsDerivativeContractVersion = $ConstTrue">
                <xsl:choose>
                  <xsl:when test="string-length($pDerivativeContractVersion) > 0">
                    <xsl:value-of select="'(dc.CONTRACTATTRIBUTE = @CONTRACTATTRIBUTE)'"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="'(dc.CONTRACTATTRIBUTE is null or dc.CONTRACTATTRIBUTE = 0)'"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:if>-->
              <!-- FI 20161005 [23631] Ajout d'une restriction sur CONTRACTATTRIBUTE uniquement lorsque $pDerivativeContractVersion est renseigné -->
              <!--<xsl:choose>
                <xsl:when test="string-length($pDerivativeContractVersion) > 0">
                  <xsl:value-of select="concat('(dc.CONTRACTATTRIBUTE = @CONTRACTATTRIBUTE or css.BIC !=',$s-quote,'EUXCDEFF',$s-quote,')')"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="concat('((dc.CONTRACTATTRIBUTE is null or dc.CONTRACTATTRIBUTE = 0) or css.BIC !=',$s-quote,'EUXCDEFF',$s-quote,')')"/>
                </xsl:otherwise>
              </xsl:choose>-->
              <xsl:if test="string-length($pDerivativeContractVersion) > 0">
                <xsl:value-of select="' and (dc.CONTRACTATTRIBUTE=@CONTRACTATTRIBUTE)'"/>
              </xsl:if>
              <xsl:if test="string-length($pDerivativeContractCategory) > 0">
                <xsl:value-of select="' and (dc.CATEGORY = @CATEGORY)'"/>
              </xsl:if>
              <xsl:if test="string-length($pDerivativeContractType) > 0">
                <xsl:value-of select="' and (dc.CONTRACTTYPE = @CONTRACTTYPE)'"/>
              </xsl:if>
              <xsl:if test="string-length($pDerivativeContractSettltMethod) > 0">
                <xsl:value-of select="' and (dc.SETTLTMETHOD = @SETTLTMETHOD)'"/>
              </xsl:if>
              <xsl:if test="string-length($pDerivativeContractExerciseStyle) > 0">
                <xsl:value-of select="' and (dc.EXERCISESTYLE = @EXERCISESTYLE)'"/>
              </xsl:if>
            </xsl:variable>
            <xsl:variable name ="vAdditionalCondition" >
              <xsl:choose>
                <xsl:when test="starts-with($vAdditionalCondition1, ' and ')">
                  <xsl:value-of select="substring-after($vAdditionalCondition1, ' and ')"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$vAdditionalCondition1"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:variable name="vAdditionalParameter">
              <Param name="DCVALUE" datatype="string">
                <xsl:value-of select="concat($pDerivativeContract,' [',$pDerivativeContractColumn,']')"/>
              </Param>
              <!-- FI 20161005 [XXXXX] CONTRACTATTRIBUTE est pris en considération uniquement sur Eurex  -->
              <!--<xsl:if test="$pIsDerivativeContractVersion = $ConstTrue and string-length($pDerivativeContractVersion) > 0">
                <Param name="CONTRACTATTRIBUTE" datatype="string">
                  <xsl:value-of select="$pDerivativeContractVersion"/>
                </Param>
              </xsl:if>-->
              <xsl:if test="string-length($pDerivativeContractVersion) > 0">
                <Param name="CONTRACTATTRIBUTE" datatype="string">
                  <xsl:value-of select="$pDerivativeContractVersion"/>
                </Param>
              </xsl:if>
              <xsl:if test="string-length($pDerivativeContractCategory) > 0">
                <Param name="CATEGORY" datatype="string">
                  <xsl:value-of select="$pDerivativeContractCategory"/>
                </Param>
              </xsl:if>
              <xsl:if test="string-length($pDerivativeContractType) > 0">
                <Param name="CONTRACTTYPE" datatype="string">
                  <xsl:value-of select="$pDerivativeContractType"/>
                </Param>
              </xsl:if>
              <xsl:if test="string-length($pDerivativeContractSettltMethod) > 0">
                <Param name="SETTLTMETHOD" datatype="string">
                  <xsl:value-of select="$pDerivativeContractSettltMethod"/>
                </Param>
              </xsl:if>                    
              <xsl:if test="string-length($pDerivativeContractExerciseStyle) > 0">
                 <Param name="EXERCISESTYLE" datatype="string">
                  <xsl:call-template name="SQLEnumValue2">
                    <xsl:with-param name="pCode" select="'DerivativeExerciseStyleEnum'"/>
                    <xsl:with-param name="pExtValue" select="$pDerivativeContractExerciseStyle"/>
                  </xsl:call-template>  
                </Param>
              </xsl:if>
            </xsl:variable>

            <SQL command="select" result="IDENTIFIER" cache="true">
              <![CDATA[
              select '0' as LEVELSORT, dc.IDENTIFIER
              from dbo.DERIVATIVECONTRACT dc
              inner join dbo.VW_MARKET_IDENTIFIER m on m.IDM = dc.IDM
              inner join dbo.ACTOR css on css.IDA = m.IDA
              ]]>
              <xsl:call-template name="SQLDTENABLEDDTDISABLED">
                <xsl:with-param name="pTable" select="'m'"/>
              </xsl:call-template>
              <xsl:variable name ="vXMLExtlIdJoins">
                <xsl:call-template name="XMLJoinExtlId">
                  <xsl:with-param name="pTableName" select="'DERIVATIVECONTRACT'"/>
                  <xsl:with-param name="pTableAlias" select="'dc'"/>
                  <xsl:with-param name="pJoinId" select="'dc.IDDC'"/>
                  <xsl:with-param name="pValueColumn" select="$pDerivativeContractColumn"/>
                  <xsl:with-param name="pValue" select="$pDerivativeContract"/>
                </xsl:call-template>
                <xsl:if test="string-length($pMarket) > 0 and string-length($pMarketColumn) > 0">
                  <xsl:call-template name="XMLJoinExtlId">
                    <xsl:with-param name="pTableName" select="'MARKET'"/>
                    <xsl:with-param name="pTableAlias" select="'m'"/>
                    <xsl:with-param name="pJoinId" select="'m.IDM'"/>
                    <xsl:with-param name="pValueColumn" select="$pMarketColumn"/>
                    <xsl:with-param name="pValue" select="$pMarket"/>
                  </xsl:call-template>
                </xsl:if>
                <xsl:if test="string-length($pClearingHouse) > 0 and string-length($pClearingHouseColumn) > 0">
                    <xsl:call-template name="XMLJoinExtlId">
                      <xsl:with-param name="pTableName" select="'ACTOR'"/>
                      <xsl:with-param name="pTableAlias" select="'css'"/>
                      <xsl:with-param name="pJoinId" select="'css.IDA'"/>
                      <xsl:with-param name="pValueColumn" select="$pClearingHouseColumn"/>
                      <xsl:with-param name="pValue" select="$pClearingHouse"/>
                    </xsl:call-template>
                </xsl:if>
              </xsl:variable>
              <xsl:call-template name="SQLJoinExtlId">
                <xsl:with-param name="pXMLExtlIdJoins">
                  <xsl:copy-of select="$vXMLExtlIdJoins"/>
                </xsl:with-param>
                <xsl:with-param name="pAdditionalCondition">
                  <xsl:value-of select="$vAdditionalCondition"/>
                </xsl:with-param>
                <xsl:with-param name="pAdditionalSQL">
                  <xsl:call-template name="SQLDTENABLEDDTDISABLED">
                    <xsl:with-param name="pTable" select="'dc'"/>
                  </xsl:call-template>
                  <![CDATA[
                  union all
                  select '1' as LEVELSORT, @DCVALUE as IDENTIFIER
                  from DUAL
                  order by LEVELSORT
                  ]]>
                </xsl:with-param>
                <xsl:with-param name="pAdditionalParameter">
                  <xsl:copy-of select='$vAdditionalParameter'/>
                </xsl:with-param>
              </xsl:call-template>
              <Param name="DT" datatype="date">
                <xsl:value-of select="$pBusinessDate"/>
              </Param>
            </SQL>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="string-length($pDerivativeContract) > 0">
        <xsl:value-of select="concat($pDerivativeContract,' [',$ConstUnknown,']')"/>
      </xsl:when>
      <xsl:when test="$pIsMandatory=$ConstTrue">
        <xsl:value-of select="$ConstUnknown"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- Ajoute une restriction SQL exploitant les colonnes DTENABLED/DTDISABLED d'une table -->
  <!-- FI 20170214 [21916] Refactoring Call Template Replace-->
  <xsl:template name="SQLDTENABLEDDTDISABLED">
    <xsl:param name="pTable"/>
    <xsl:variable name ="sqlRestrict">
      <xsl:value-of select ="'and ({alias}.DTENABLED&lt;=@DT and ({alias}.DTDISABLED is null or {alias}.DTDISABLED>@DT))'"/>
    </xsl:variable>
    <xsl:call-template name ="Replace">
      <xsl:with-param name ="source" select ="$sqlRestrict"/>
      <xsl:with-param name ="oldValue" select="'{alias}'"/>
      <xsl:with-param name ="newValue" select="$pTable"/>
    </xsl:call-template>
  </xsl:template>

  <!-- Génère du code SQL pour récupérer un book côté Clearing à partir d'un book en entrée ou-->
  <!-- Génère du code SQL pour récupérer le propriétaire d'un book côté Clearing à partir d'un book en entrée-->
  <!-- Lorsque la donnée en entrée est vide, permet d'appliquer une valeur par défaut -->
  <!-- FI 20160929 [22507] Add SQLClearingActorAndBook -->
  <xsl:template name="SQLClearingActorAndBook">
    <!--Représente un book en entrée -->
    <xsl:param name="pValue"/>
    <!--Représente l'identification du book en entrée-->
    <xsl:param name="pValueIdent"/>
    <!-- BOOK_IDENTIFIER => Récupération du book -->
    <!-- IDENTIFIER => Récupération du propriétaire du book -->
    <xsl:param name="pResultColumn" select="'IDENTIFIER'"/>
    <xsl:param name="pUseDefaultValue" select="$ConstFalse"/>
    <xsl:param name="pDefaultValue"/>

    <xsl:variable name ="vBookColumnName">
      <xsl:call-template name="GetBookIdentColumn">
        <xsl:with-param name="pIdent">
          <xsl:copy-of select="$pValueIdent"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name ="vIsValueWithUnderscore">
      <xsl:choose>
        <xsl:when test ="contains($pValue,'_')">
          <xsl:value-of select ="$ConstTrue"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$ConstFalse"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- RD 20230102 [25847] UNKNOWN parameter : wrong value-->
    <xsl:choose>
      <xsl:when test="string-length($pValue) > 0 and string-length($vBookColumnName) > 0 and $vIsValueWithUnderscore = $ConstTrue">
        <xsl:variable name ="vXMLExtlIdJoins">
          <xsl:call-template name="XMLJoinExtlId">
            <xsl:with-param name="pTableName" select="'BOOK'"/>
            <xsl:with-param name="pTableAlias" select="'b'"/>
            <xsl:with-param name="pJoinId" select="'b.IDB'"/>
            <xsl:with-param name="pValueColumn" select="$vBookColumnName"/>
            <xsl:with-param name="pValue" select="$pValue">
            </xsl:with-param>
          </xsl:call-template>
        </xsl:variable>
        <xsl:variable name="vAdditionalParameter">
          <xsl:choose>
            <xsl:when test="string-length($pDefaultValue) > 0">
              <Param name="UNKNOWN" datatype="string">
                <xsl:value-of select="$pDefaultValue"/>
              </Param>
            </xsl:when>
            <xsl:otherwise>
              <Param name="UNKNOWN" datatype="string">
                <xsl:value-of select="$ConstUnknown"/>
              </Param>
            </xsl:otherwise>
          </xsl:choose>
          <Param name="BOOKVALUE" datatype="string">
            <xsl:value-of select="concat($pValue,' [',$vBookColumnName,']')"/>
          </Param>
        </xsl:variable>
        <!-- Recherche du book en considérant uniquement ce qui se trouve d'arrère le '_' -->
        <xsl:variable name ="vXMLExtlIdJoins2">
          <xsl:call-template name="XMLJoinExtlId">
            <xsl:with-param name="pTableName" select="'BOOK'"/>
            <xsl:with-param name="pTableAlias" select="'b'"/>
            <xsl:with-param name="pJoinId" select="'b.IDB'"/>
            <xsl:with-param name="pValueColumn" select="$vBookColumnName"/>
            <xsl:with-param name="pValue" select="substring-after($pValue,'_')">
            </xsl:with-param>
          </xsl:call-template>
        </xsl:variable>
        <!-- Liste des requêtes qui seront exécutée (Union All entre les 3 requêtes)-->
        <xsl:variable name ="SQLQuery">
          <SQL1>
            <xsl:text>
                    <![CDATA[
                    select '0' as LEVELSORT,a.IDENTIFIER,b.IDENTIFIER as BOOK_IDENTIFIER
                    from dbo.ACTOR a
                    inner join dbo.BOOK b on b.IDA=a.IDA
                    ]]>
                    </xsl:text>
            <xsl:call-template name="SQLJoinExtlId">
              <xsl:with-param name="pXMLExtlIdJoins" select="$vXMLExtlIdJoins"/>
            </xsl:call-template>
          </SQL1>
          <SQL2>
            <xsl:text>
                    <![CDATA[
                    select '1' as LEVELSORT,a.IDENTIFIER,b.IDENTIFIER as BOOK_IDENTIFIER
                    from dbo.ACTOR a
                    inner join dbo.BOOK b on b.IDA=a.IDA
                    ]]>
                    </xsl:text>
            <xsl:call-template name="SQLJoinExtlId">
              <xsl:with-param name="pXMLExtlIdJoins" select="$vXMLExtlIdJoins2"/>
            </xsl:call-template>
          </SQL2>
          <SQL3>
            <xsl:text>
                    <![CDATA[
                    select '2' as LEVELSORT,@UNKNOWN as IDENTIFIER,@BOOKVALUE as BOOK_IDENTIFIER 
                    from DUAL
                    order by LEVELSORT
                    ]]>
                    </xsl:text>
          </SQL3>
        </xsl:variable>

        <SQL command="select" result="{$pResultColumn}" cache="true">
          <!--SQL Query-->
          <!-- FI 20170404 [23039] Utilisation de msxsl:node-set -->
          <xsl:copy-of select="msxsl:node-set($SQLQuery)/SQL1/text()"/>
          <xsl:text> union all </xsl:text>
          <xsl:call-template name ="Replace">
            <xsl:with-param name ="source" select ="msxsl:node-set($SQLQuery)/SQL2/text()"/>
            <xsl:with-param name ="oldValue" select="'@VALUE1'"/>
            <xsl:with-param name ="newValue" select="'@VALUE2'"/>
          </xsl:call-template>
          <xsl:text> union all </xsl:text>
          <!-- FI 20170404 [23039] Utilisation de msxsl:node-set -->
          <xsl:copy-of select="msxsl:node-set($SQLQuery)/SQL3/text()"/>
          <!--Liste des paramètres-->
          <!-- FI 20170404 [23039] Utilisation de msxsl:node-set -->
          <xsl:copy-of select="msxsl:node-set($SQLQuery)/SQL1/Param"/>
          <Param datatype="string" name="VALUE2">
            <!-- FI 20170404 [23039] Utilisation de msxsl:node-set -->
            <xsl:value-of select="msxsl:node-set($SQLQuery)/SQL2/Param[@name='VALUE1']"/>
          </Param>
          <xsl:copy-of select="$vAdditionalParameter"/>
        </SQL>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="SQLActorAndBook">
          <xsl:with-param name="pValue" select="$pValue"/>
          <xsl:with-param name="pValueIdent" select="$pValueIdent"/>
          <xsl:with-param name="pResultColumn" select="$pResultColumn"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- FI 20170404 [23039] Add Template -->
  <xsl:template name="SQLInstr">
    <xsl:param name="pInstr"/>
    <xsl:param name="pInstrColumn"/>
    <xsl:param name="pIsMandatory" select="$ConstFalse"/>
    <xsl:choose>
      <xsl:when test="string-length($pInstr) > 0 and string-length($pInstrColumn) > 0">
        <xsl:choose>
          <xsl:when test="$pInstrColumn = 'IDENTIFIER'">
            <xsl:value-of select="$pInstr"/>
          </xsl:when>
          <xsl:otherwise>
            <SQL command="select" result="'IDENTIFIER'" cache="true">
              <xsl:text>
              <![CDATA[
              select '0' as LEVELSORT, i.IDENTIFIER
              from dbo.INSTRUMENT i
              ]]>
              </xsl:text>
              <xsl:variable name ="vXMLExtlIdJoins">
                <xsl:call-template name="XMLJoinExtlId">
                  <xsl:with-param name="pTableName" select="'INSTRUMENT'"/>
                  <xsl:with-param name="pTableAlias" select="'i'"/>
                  <xsl:with-param name="pJoinId" select="'i.IDI'"/>
                  <xsl:with-param name="pValueColumn" select="$pInstrColumn"/>
                  <xsl:with-param name="pValue" select="$pInstr"/>
                </xsl:call-template>
              </xsl:variable>
              <xsl:variable name ="paramInstr">
                <Param name="UNKNOWN" datatype="string">
                  <xsl:value-of select="$ConstUnknown"/>
                </Param>
              </xsl:variable>
              <xsl:call-template name="SQLJoinExtlId">
                <xsl:with-param name="pXMLExtlIdJoins">
                  <xsl:copy-of select="$vXMLExtlIdJoins"/>
                </xsl:with-param>
                <xsl:with-param name="pAdditionalSQL">
                  <xsl:text>
                  <![CDATA[
                  union all
                  select '1' as LEVELSORT,@UNKNOWN as IDENTIFIER
                  from DUAL
                  order by LEVELSORT
                  ]]>
                  </xsl:text>
                </xsl:with-param>
                <xsl:with-param name="pAdditionalParameter">
                  <xsl:copy-of select='$paramInstr'/>
                </xsl:with-param>
              </xsl:call-template>
            </SQL>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>

      <xsl:when test="string-length($pInstr) > 0">
        <xsl:value-of select="concat($pInstr,' [',$ConstUnknown,']')"/>
      </xsl:when>
      <xsl:when test="$pIsMandatory=$ConstTrue">
        <xsl:value-of select="$ConstUnknown"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- FI 20170404 [23039] Add Template 
       Recherche de l'asset à partir de l'asset ou à partir du commodity contrat 
  -->
  <!-- FI 20220214 [25699] pContractCategory replaced by pContractTradableType
                           pContractType replaced by pContractDuration -->
  <xsl:template name="COMS_SQLAsset">
    <xsl:param name="pAssetCode"/>
    <xsl:param name="pAssetColumn"/>
    <xsl:param name="pMarket"/>
    <xsl:param name="pMarketColumn"/>
    <xsl:param name="pContract"/>
    <xsl:param name="pContractColumn"/>
    <xsl:param name="pContractTradableType"/>
    <xsl:param name="pContractDuration"/>
    <xsl:param name="pResultColumn" select="'ASSET_IDENTIFIER'"/>
    <xsl:param name="pIsMandatory" select="$ConstFalse"/>
    <xsl:choose>
      <xsl:when test="string-length($pAssetCode) > 0 and string-length($pAssetColumn) > 0">
        <!-- Recherche de l'asset à partir de l'asset -->
        <xsl:choose>
          <xsl:when test="($pResultColumn = 'ASSET_IDENTIFIER' and $pAssetColumn = 'IDENTIFIER')">
            <xsl:value-of select="$pAssetCode"/>
          </xsl:when>
          <xsl:when test="string-length($pAssetCode) > 0 and string-length($pAssetColumn) > 0">
            <SQL command="select" result="{$pResultColumn}" cache="true">
              <xsl:text>
              <![CDATA[
              select '0' as LEVELSORT,cdc.IDENTIFIER,a.IDENTIFIER as ASSET_IDENTIFIER
              from dbo.ASSET_COMMODITY a
              inner join dbo.COMMODITYCONTRACT cc on cc.IDCC = a.IDCC
              ]]>
              </xsl:text>
              <xsl:if test="string-length($pMarket) > 0 and string-length($pMarketColumn) > 0">
                <xsl:text>inner join dbo.VW_MARKET_IDENTIFIER m on m.IDM=cc.IDM</xsl:text>
              </xsl:if>
              <xsl:variable name ="vXMLExtlIdJoins">
                <xsl:call-template name="XMLJoinExtlId">
                  <xsl:with-param name="pTableName" select="'ASSET_COMMODITY'"/>
                  <xsl:with-param name="pTableAlias" select="'a'"/>
                  <xsl:with-param name="pJoinId" select="'a.IDASSET'"/>
                  <xsl:with-param name="pValueColumn" select="$pAssetColumn"/>
                  <xsl:with-param name="pValue" select="$pAssetCode"/>
                </xsl:call-template>
                <xsl:if test="string-length($pMarket) > 0 and string-length($pMarketColumn) > 0">
                  <xsl:call-template name="XMLJoinExtlId">
                    <xsl:with-param name="pTableName" select="'MARKET'"/>
                    <xsl:with-param name="pTableAlias" select="'m'"/>
                    <xsl:with-param name="pJoinId" select="'m.IDM'"/>
                    <xsl:with-param name="pValueColumn" select="$pMarketColumn"/>
                    <xsl:with-param name="pValue" select="$pMarket"/>
                  </xsl:call-template>
                </xsl:if>
              </xsl:variable>
              <xsl:call-template name="SQLJoinExtlId">
                <xsl:with-param name="pXMLExtlIdJoins">
                  <xsl:copy-of select="$vXMLExtlIdJoins"/>
                </xsl:with-param>
                <xsl:with-param name="pAdditionalCondition"/>
              </xsl:call-template>
            </SQL>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="string-length($pContract) > 0 and string-length($pContractColumn) > 0">
        <!-- Recherche de l'asset à partir du contract -->
        <xsl:variable name="vAdditionalParameter">
          <xsl:if test="string-length($pContractTradableType) > 0">
            <Param name="TRADABLETYPE" datatype="string">
              <xsl:call-template name="SQLEnumValue2">
                <xsl:with-param name="pCode" select="'TradableType'"/>
                <xsl:with-param name="pExtValue" select="$pContractTradableType"/>
              </xsl:call-template>
            </Param>
          </xsl:if>
          <xsl:if test="string-length($pContractDuration) > 0">
            <Param name="DURATION" datatype="string">
              <xsl:call-template name="SQLEnumValue2">
                <xsl:with-param name="pCode" select="'SettlementPeriodDurationEnum'"/>
                <xsl:with-param name="pExtValue" select="$pContractDuration"/>
              </xsl:call-template>
            </Param>
          </xsl:if>
        </xsl:variable>

        <xsl:variable name ="vXMLExtlIdJoins">
          <xsl:call-template name="XMLJoinExtlId">
            <xsl:with-param name="pTableName" select="'COMMODITYCONTRACT'"/>
            <xsl:with-param name="pTableAlias" select="'cc'"/>
            <xsl:with-param name="pJoinId" select="'dc.IDCC'"/>
            <xsl:with-param name="pValueColumn" select="$pContractColumn"/>
            <xsl:with-param name="pValue" select="$pContract"/>
          </xsl:call-template>
          <xsl:if test="string-length($pMarket) > 0 and string-length($pMarketColumn) > 0 ">
            <xsl:call-template name="XMLJoinExtlId">
              <xsl:with-param name="pTableName" select="'MARKET'"/>
              <xsl:with-param name="pTableAlias" select="'m'"/>
              <xsl:with-param name="pJoinId" select="'m.IDM'"/>
              <xsl:with-param name="pValueColumn" select="$pMarketColumn"/>
              <xsl:with-param name="pValue" select="$pMarket"/>
            </xsl:call-template>
          </xsl:if>
        </xsl:variable>
        
        <SQL command="select" result="{$pResultColumn}" cache="true">
          <xsl:text>
            <![CDATA[
select '0' as LEVELSORT,cc.IDENTIFIER,a.IDENTIFIER as ASSET_IDENTIFIER
from dbo.ASSET_COMMODITY a
inner join dbo.COMMODITYCONTRACT cc on cc.IDCC = a.IDCC]]>
          </xsl:text>
          <xsl:if test="string-length($pContractTradableType) > 0">
            <xsl:value-of select="' and (cc.TRADABLETYPE = @TRADABLETYPE)'"/>
          </xsl:if>
          <xsl:if test="string-length($pContractDuration) > 0">
            <xsl:value-of select="' and (cc.DURATION = @DURATION)'"/>
          </xsl:if>
          <xsl:if test="string-length($pMarket) > 0 and string-length($pMarketColumn) > 0">
            <xsl:text>inner join dbo.VW_MARKET_IDENTIFIER m on m.IDM=cc.IDM</xsl:text>
          </xsl:if>
          <xsl:call-template name="SQLJoinExtlId">
            <xsl:with-param name="pXMLExtlIdJoins">
              <xsl:copy-of select="$vXMLExtlIdJoins"/>
            </xsl:with-param>
            <xsl:with-param name="pAdditionalCondition"/>
            <xsl:with-param name="pAdditionalParameter">
              <xsl:copy-of select='$vAdditionalParameter'/>
            </xsl:with-param>
          </xsl:call-template>
        </SQL>
      </xsl:when>
      <xsl:when test="string-length($pAssetCode) > 0">
        <xsl:value-of select="concat($pAssetCode,' [',$ConstUnknown,']')"/>
      </xsl:when>
      <xsl:when test="$pIsMandatory=$ConstTrue">
        <xsl:value-of select="$ConstUnknown"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- FI 20170404 [23039] add Template GetDynamicDataMarketIdentifier 
  Retourne le marché (de type StringDynamicData)
  -->
  <!-- FI 20220214 [25699] Add pContractType-->
  <!-- FI 20220215 [25699] Add pClearingHouse and pClearingHouseColumn -->
  <!-- FI 20220225 [25699] Add pDerivativeContractSettltMethod and pDerivativeContractExerciseStyle -->
  <xsl:template name ="GetDynamicDataMarketIdentifier">
    <xsl:param name ="pFamily"/>
    <xsl:param name ="pBusinessDate"/>
    <xsl:param name ="pMarket"/>
    <xsl:param name ="pMarketColumn"/>
    <xsl:param name ="pAssetCode"/>
    <xsl:param name ="pAssetColumn"/>
    <xsl:param name ="pContrat"/>
    <xsl:param name ="pContractColumn"/>
    <xsl:param name ="pContractVersion"/>
    <xsl:param name ="pContractCategory"/>
    <xsl:param name ="pContractType"/>
    <xsl:param name ="pContractSettltMethod"/>
    <xsl:param name ="pContractExerciseStyle"/>
    <xsl:param name ="pClearingHouse"/>
    <xsl:param name ="pClearingHouseColumn"/>

    <dynamicValue>
      <xsl:choose>
        <xsl:when  test="string-length($pMarket) > 0">
          <xsl:call-template name="SQLMarket">
            <xsl:with-param name="pMarket" select="$pMarket"/>
            <xsl:with-param name="pMarketColumn" select="$pMarketColumn"/>
            <!--<xsl:with-param name="pResultColumn" select="'FIXML_SECURITYEXCHANGE'"/>-->
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:choose>
            <xsl:when test="$pFamily='ETD'">
              <xsl:call-template name="ETD_SQLMarket">
                <xsl:with-param name="pBusinessDate" select="$pBusinessDate"/>
                <xsl:with-param name="pMarket" select="$pMarket"/>
                <xsl:with-param name="pMarketColumn" select="$pMarketColumn"/>
                <xsl:with-param name="pAssetCode" select="$pAssetCode"/>
                <xsl:with-param name="pAssetColumn" select="$pAssetColumn"/>
                <xsl:with-param name="pDerivativeContract" select="$pContrat"/>
                <xsl:with-param name="pDerivativeContractColumn" select="$pContractColumn"/>
                <xsl:with-param name="pDerivativeContractVersion" select="$pContractVersion"/>
                <xsl:with-param name="pDerivativeContractCategory" select="$pContractCategory"/>
                <xsl:with-param name="pDerivativeContractType" select="$pContractType"/>
                <xsl:with-param name="pDerivativeContractSettltMethod" select="$pContractSettltMethod"/>
                <xsl:with-param name="pDerivativeContractExerciseStyle" select="$pContractExerciseStyle"/>
                <xsl:with-param name="pClearingHouse" select="$pClearingHouse"/>
                <xsl:with-param name="pClearingHouseColumn" select="$pClearingHouseColumn"/>
                <!--<xsl:with-param name="pResultColumn" select="'FIXML_SECURITYEXCHANGE'"/>-->
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <!-- FI 20170404 [23039] A compléter -->
            </xsl:otherwise>
          </xsl:choose>
        </xsl:otherwise>
      </xsl:choose>
    </dynamicValue>
  </xsl:template>

  <!-- FI 20170404 [23039] add Template GetDynamicDataCssIdentifier 
  Retourne la CSS (de type StringDynamicData)
  -->
  <xsl:template name ="GetDynamicDataCssIdentifier">
    <xsl:param name="pDynamicDataMarketIdentifier"/>
    <dynamicValue>
      <SQL command="select" result="CSS" cache="true">
       <![CDATA[
         select '0' as LEVELSORT,a.IDENTIFIER as CSS
         from dbo.VW_MARKET_IDENTIFIER m
         inner join dbo.ACTOR a on a.IDA = m.IDA 
         where m.FIXML_SECURITYEXCHANGE = @MARKETIDENTIFIER
        ]]>
        <Param name="MARKETIDENTIFIER" datatype="string">
          <xsl:copy-of select="msxsl:node-set($pDynamicDataMarketIdentifier)/dynamicValue/child::node()"/>
        </Param>
      </SQL>
    </dynamicValue>
  </xsl:template>

  <!-- FI 20170404 [23039] add Template GetDynamicDataAssetIdentifier 
  Retourne l'asset (de type StringDynamicData)
  -->
  <!-- FI 20170718 [23326] Modify -->
  <!-- FI 20220214 [25699] pContractType replaced by pContractKind -->
  <!-- FI 20220304 [25699] Add pClearingHouse and pClearingHouseColumn -->
  <!-- FI 20220628 [25699] pContractKind replaced by pContractType -->
  <xsl:template name ="GetDynamicDataAssetIdentifier">
    <xsl:param name ="pFamily"/>
    <xsl:param name ="pBusinessDate"/>
    <xsl:param name ="pMarket"/>
    <xsl:param name ="pMarketColumn"/>
    <xsl:param name ="pAssetCode"/>
    <xsl:param name ="pAssetColumn"/>
    <xsl:param name ="pContract"/>
    <xsl:param name ="pContractColumn"/>
    <xsl:param name ="pContractVersion"/>
    <xsl:param name ="pContractCategory"/>
    <xsl:param name ="pContractType"/>
    <xsl:param name ="pClearingHouse"/>
    <xsl:param name ="pClearingHouseColumn"/>
    <xsl:param name ="pIsMandatory" />
    <!-- FI 20170718 [23326] Ajout du tag dynamicValue -->
    <dynamicValue>
      <xsl:choose>
        <xsl:when test="$pFamily='ETD'">
          <xsl:call-template name="ETD_SQLInstrumentAndAsset">
            <xsl:with-param name="pBusinessDate" select="$pBusinessDate"/>
            <xsl:with-param name="pAssetCode" select="$pAssetCode"/>
            <xsl:with-param name="pAssetColumn" select="$pAssetColumn"/>
            <xsl:with-param name="pMarket" select="$pMarket"/>
            <xsl:with-param name="pMarketColumn" select="$pMarketColumn"/>
            <xsl:with-param name="pClearingHouse" select="$pClearingHouse"/>
            <xsl:with-param name="pClearingHouseColumn" select="$pClearingHouseColumn"/>
            <xsl:with-param name="pResultColumn" select="'ASSET_IDENTIFIER'"/>
            <xsl:with-param name="pIsMandatory" select="$pIsMandatory"/>
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$pFamily='ESE'">
          <xsl:call-template name="ESE_SQLAsset">
            <xsl:with-param name="pAssetCode" select="$pAssetCode"/>
            <xsl:with-param name="pAssetColumn" select="$pAssetColumn"/>
            <xsl:with-param name="pMarket" select="$pMarket"/>
            <xsl:with-param name="pMarketColumn" select="$pMarketColumn"/>
            <!--<xsl:with-param name="pResultColumn" select="'IDENTIFIER'"/>-->
            <xsl:with-param name="pIsMandatory" select="$pIsMandatory"/>
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$pFamily='COMS'">
          <xsl:call-template name="COMS_SQLAsset">
            <xsl:with-param name="pAssetCode" select="$pAssetCode"/>
            <xsl:with-param name="pAssetColumn" select="$pAssetColumn"/>
            <xsl:with-param name="pMarket" select="$pMarket"/>
            <xsl:with-param name="pMarketColumn" select="$pMarketColumn"/>
            <xsl:with-param name="pContract" select="$pContract"/>
            <xsl:with-param name="pContractColumn" select ="$pContractColumn"/>
            <xsl:with-param name="pContractTradableType" select ="$pContractCategory"/>
            <xsl:with-param name="pContractDuration" select ="$pContractType"/>
            <xsl:with-param name="pResultColumn" select="'ASSET_IDENTIFIER'"/>
            <xsl:with-param name="pIsMandatory" select="$pIsMandatory"/>
          </xsl:call-template>
        </xsl:when>
      </xsl:choose>
    </dynamicValue>
  </xsl:template>
 
</xsl:stylesheet>
