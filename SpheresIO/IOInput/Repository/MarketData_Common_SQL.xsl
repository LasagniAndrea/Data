<?xml version="1.0" encoding="utf-16"?>
<!-- FI 20200901 [25468] use of GetUTCDateTimeSys -->
<!--
=============================================================================================
 Summary: SQL Template common for LSD Marketdata import
 File   : MarketData_Common_SQL.xsl
=============================================================================================
Version : v12.1.8465   Date    : 20230306	Author  : PM
Comment : [WI592] Add test on Category and control to discard null values
=============================================================================================
Version : v8.1.8040   Date    : 20220105	Author  : FL/RD
Comment : 25920 - ICE-EU - Repository error : Row is rejected by controls. [EFS]
 - Add parameter pIsCheckAssetCategory and display Warning if Asset category is missing
    on SQLTableDERIVATIVECONTRACT template
=============================================================================================
Version : v8.1.7656   Date    : 20201217	Author  : FL
Comment : 25606 - ICE-EU - Repository error : Row is rejected by controls. [EFS]
 - Fixed a Bug generating Maturiy rules to Null instead of the Default Rule 
    on SQLTableDERIVATIVECONTRACT template
============================================================================================  
Version: v6.0.6488   Date: 20171006   Author: FL
Comment: [23495] - Import: BMECLEARING - REPOSITORY (New feature)
 - Fixed various problems following recipe for setting up this import into production.
 - Add pPhysettltamount parameter
    on template SQLTableDERIVATIVECONTRACT and ovrSQLGetDerivativeContractDefaultValue
 - Add pSQLIdMaturityRule2 parameter
    on template SQLTableMATURITYRULE_DERIVATIVECONTRACT, SQLTableDERIVATIVECONTRACT
 - Add pSQLUnderlyingIdAsset parameter on template SQLTableDERIVATIVECONTRACT
 - New Template SQLIdMaturityRule2
 - Manage mew colunm IDXC, CONTRACTCATEGORY in Table CONTRACTG
============================================================================================
 Version: v6.0.0.0    Date: 20160106    Author: FL
 Comment: [22084] - Derivative Contracts: Exercise Rules
  - Add pExerciseRule parameter on SQLTableDERIVATIVECONTRACT template
============================================================================================
 Version: v4.0.0.0    Date: 20140619    Author: FL/RD
 Comment: [20113] the contract size (column (FACTOR) is not updated for all DCs
=============================================================================================
 Version: v4.1.0.0    Date: 20140513    Author: PM
 Comment: [19970][19259] Managing the case where the currency is provided by an SQL query
=============================================================================================
 Version: v4.0.0.0    Date: 20140326    Author: RD
 Comment: [19648]
  - Add and use a new optional parameter "pParamExchangeSymbol" to template "SQLIdMarket2"
     It contains a Spheres I/O "Param" element
  - Add a new optional parameter "pParamExchangeSymbol" to template "SQLTableDERIVATIVECONTRACT"
     It contains a Spheres I/O "Param" element
=============================================================================================
 FL 20140325 [19778]
 - Template SQLTableDERIVATIVECONTRACT
    - Column DERIVATIVECONTRACT.EXERCISESTYLE: On the DCs of category "Future" the column
      "EXERCISESTYLE" must always be NULL
=============================================================================================
 FL 20140220 [19648]
 - Template SQLTableDERIVATIVECONTRACT
    - Manage columns DERIVATIVECONTRACT.UNDERLYINGGROUP & DERIVATIVECONTRACT.UNDERLYINGASSET
    - Column DERIVATIVECONTRACT.EXERCISESTYLE: On the DCs of category "Future" the column
      "EXERCISESTYLE" must always be NULL
 - Template ovrSQLGetDerivativeContractDefaultValue
    - Managing Data Defaults UNDERLYINGGROUP & UNDERLYINGASSET
=============================================================================================
 FI 20131205 [19275]  
  - Update the template SQLTableDERIVATIVECONTRACT
=============================================================================================
 FI 20131129 [19284] 
  - Manage column DERIVATIVECONTRACT.FINALSETTLTSIDE
=============================================================================================
 FL 20131119 [19200]                                          
  - Maturity Rules: New available maturity rules with the same identifier for 2015
  - Modify query in Template : SQLIdMaturityRule
=============================================================================================
 BD 20130520
  - Global variable "gParamDtBusiness" (/ iotask / parameters / parameter [@ id = 'DTBUSINESS']).
  - Call the template SQLDTENABLEDDTDISABLED to verify the validity of the selected DCs.
=============================================================================================
 BD 20130219
  - Update of QLTableDERIVATIVECONTRACT and ovrSQLGetDerivativeContractDefaultValue
=============================================================================================
 PL 20130214
  - Management of IDEX/AGREX
=============================================================================================
 FL/PL 20130124
  - Add pMaturityRuleIdentifier parameter on SQLTableDERIVATIVECONTRACT template
============================================================================================= 
-->
<!-- 
/*==============================================================*/
                        OLD RELEASES NOTES
/*==============================================================*/
/* File    : MarketData_Common.xsl                              */
/* Version : v1.0.3.0                                           */
/* Date    : 20100407                                           */
/* Author  : GP                                                 */
/* Description: Template common for IDEM LSD Marketdata import  */
/* Comment :
    Variables
    - *NEW* gIdemSpecial
    - *NEW* gIdm
    - *NEW* gFuture
    - *NEW* gIDASSET
    Templates
    - *UPDATE* SQLTableMATURITYRULE_DERIVATIVECONTRACT
            Paramètre pAssetCategory ajouté pour alimenter le champ DERIVATIVECONTRACT.ASSETCATEGORY
            Paramètre pUnderlyingIdentifier ajouté pour alimenter le champ DERIVATIVECONTRACT.IDASSET_UNL
            Appel au template SQLTableDERIVATIVECONTRACT mis à jour: les paramètres pAssetCategory et pUnderlyingIdentifier ont été ajoutés
    - *UPDATE* SQLTableDERIVATIVECONTRACT  
            paramètres pAssetCategory et pUnderlyingIdentifier ajoutés
    - *UPDATE* SelectOrFilterColumn: IDEM Integration
    - *UPDATE* SQLWhereClauseExtension: IDEM Integration
    - *NEW* GetIdassetUnl    
    - *NEW* tSQLGetDataFromMarketTable
    - *NEW* SQLTableASSET_EQUITY
/*==============================================================*/
/* File    : MarketData_Common_SQL.xsl                          */
/* Version : v1.0.0.0                                           */
/* Date    : 20100220                                           */
/* Author  : PM/MF/RD                                           */
/* Description: SQL Template common for LSD Marketdata import   */
*==============================================================*/
 Version : v13.0.0.0                                           
 Date    : 20230726
 Author  : PL
 Comment : [XXXXX] - Use new file ProductList
===============================================================
/* Revision:   v1.1.0.1                                         */
/*                                                              */
/* Date    :   20100604                                         */
/* Author  :   RD                                               */
/* Version :                                             	      */
/* Comment :                                                    */
      - MATURITY: Load IdMaturityRule from DERIVATIVECONTRACT table 
*==============================================================*/
*==============================================================*/
/* Revision:   v1.1.0.0                                         */
/*                                                              */
/* Date    :   20100419                                         */
/* Author  :   MF                                               */
/* Version :                                             	      */
/* Comment :                                                    */
      - *NEW* Extended datas from EXCHANGEFIELD Table 
      
*==============================================================*/
/* Revision:   v1.0.9.0                                         */
/*                                                              */
/* Date    :   20100402                                         */
/* Author  :   RD                                               */
/* Version :                                             	      */
/* Comment :                                                    */
/*    Updates according to new Unique index in tables:          */
/*      - DERIVATIVECONTRACT : IDENTIFIER, IDM                  */
/*      - ASSET_ETD : IDENTIFIER, IDDERIVATIVEATTRIB            */
/*    Add warning in VRxxx columns                              */
*==============================================================*/
/* Revision:   v1.0.8.0                                         */
/*                                                              */
/* Date    :   20100322                                         */
/* Author  :   PM/MF                                               */
/* Version :                                             	      */
/* Comment :   
      - *UPD*  Insert ISAUTOSETTING, ISAUTOSETTINGASSET, ISAUTOCREATEASSET for DERIVATIVECONTRACT
      - *UPD*  IsAutoSettingAsset deprecated from SQLTableMATURITYRULE and SQLTableDERIVATIVECONTRACT_UpdateIDMATURITYRULE,
      in order to allow the insertion of MATURITYRULE elements when the autosetting property of the derivative contract is true. 
      The autosetting control has been replaced with a simple derivative contract existence check
*==============================================================*/
/* Revision:   v1.0.7.0                                         */
/*                                                              */
/* Date    :   20100312                                         */
/* Author  :   MF                                               */
/* Version :                                             	      */
/* Comment :   
      - *UPD*  gAutomaticComputeSQL for MEFF special cases
 
*==============================================================*/
*==============================================================*/
/* Revision:   v1.0.6.0                                         */
/*                                                              */
/* Date    :   20100312                                         */
/* Author  :    MF                                              */
/* Version :                                             	      */
/* Comment :   
      - *UPD* SQLAssetIdentifier - NO CONTRACTATTRIBUTE for DERIVATIVEATTRIB elements 
 
*==============================================================*/
*==============================================================*/
/* Revision:   v1.0.5.0                                         */
/*                                                              */
/* Date    :   20100311                                         */
/* Author  :     MF                                             */
/* Version :                                             	      */
/* Comment :   
      - *UPD* k.EXCHANGESYMBOL replaces k.ISO10383_ALPHA4 
      - *UPD* SQLTableCONTRACTG : removed derivaive contract identifier
      - *UPD* SqlAssetActivationUpdate: removed derivaive contract identifier
      - *UPD* SqlWhereClauseExtension/SQLSelectFilterColumn: added EUREX and SPAN special cases
 
*==============================================================*/
*==============================================================*/
/* Revision:   v1.0.4.0                                         */
/*                                                              */
/* Date    :   20100310                                         */
/* Author  :     MF                                             */
/* Version :                                             	      */
/* Comment :
- *DEL* queuing PTIDENTIFIER extended parameters removed; the parameters is passed at the specific import level (MEFF)

*==============================================================*/
/* Revision:   v1.0.3.0                                         */
/*                                                              */
/* Date    :   20100309                                         */
/* Author  :     MF                                             */
/* Version :                                             	      */
/* Comment :
- *UPD* SQLWhereClauseExtension : add the LIFFE_SPECIAL case
- *UPD* SelectOrFilterColumn : add the LIFFE_SPECIAL case

*==============================================================*/
/* Revision:   v1.0.2.0                                         */
/*                                                              */
/* Date    :   20100305                                         */
/* Author  :    MF                                              */
/* Version :                                             	      */
/* Comment :
- *UPD* All the SQL templates use "SQLWhereClauseExtension" to put additional AND filters to
original SQL requests
- *NEW* SQLWhereClauseExtension : used to add additional AND filters to original SQL requests
- *NEW* SelectOrFilterColumn : used to add additional OR filters, or to retrieve
SQL values when the file source values are unavailable
- *NEW* ParamNodesBuilder : used to build SQL node parameters (<Param/>) in a dynamic way, 
      or to retrieve a string value from a collection (in a .NET Dictionary style)
      
/*==============================================================*/
/* Revision:   v1.0.1.0                                         */
/*                                                              */
/* Date    :   20100220                                         */
/* Author  :     MF                                             */
/* Version :                                             	      */
/* Comment :   
      - *UPD* DERIVATIVECONTRACT - new fields : PriceIncr StrikeDecLocator MinPriceIncrAmount
      - *NEW* new templates : SQLTableGCONTRACT SQLTableGCONTRACTROLE SQLTableCONTRACTG
      
/*==============================================================*/
/*==============================================================*/
/* TODO :                                                       */
/*                                                              */
/* Author  :      MF                                            */
/* Version :                                             	      */
/* Comment :   
      - EUREX SqlAssetActivationUpdate - remove identifier filters and use symbol and similia
      - Change name SQLTableMATURITYRULE_DERIVATIVECONTRACT to SQLTableDERIVATIVECONTRACT_MATURITYRULE
      
/*==============================================================*/
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- Includes-->
  <xsl:include href="..\Common\ImportTools.xsl"/>

  <!-- Global variables-->
  <xsl:variable name="vFile">MarketData_Common_SQL.xsl</xsl:variable>
  <xsl:variable name="vVersion">v8.1.8040</xsl:variable>
  <xsl:variable name="gTrue">true</xsl:variable>
  <xsl:variable name="gFalse">false</xsl:variable>
  <xsl:variable name="gNull">null</xsl:variable>
  <xsl:variable name="gSqlConcat">||</xsl:variable>
  <xsl:variable name="gAutomaticComputeSQL">%%AUTOMATIC_COMPUTE%%</xsl:variable>
  <xsl:variable name="gIdemSpecial">IDEM_SPECIAL</xsl:variable>
  <xsl:variable name="gIdm" select="'IDM'"/>
  <xsl:variable name="gFuture" select="'Future'"/>
  <xsl:variable name="gIDASSET" select="'IDASSET'"/>

  <xsl:variable name="gParamDtBusiness">
    <xsl:value-of select="/iotask/parameters/parameter[@id='DTBUSINESS']"/>
  </xsl:variable>

  <xsl:variable name="gParamCategoryFuture">
    <Param name="CATEGORYFUTURE" datatype="string">F</Param>
  </xsl:variable>
  <xsl:variable name="gParamCategoryOption">
    <Param name="CATEGORYOPTION" datatype="string">O</Param>
  </xsl:variable>

  <!-- *************************************************************** -->
  <!-- Templates Ã  partager entre les diffÃ©rents imports LSD_MarketData -->
  <!-- *************************************************************** -->
  <xsl:template name="GetCategoryValue">
    <xsl:param name="pValue"/>
    <xsl:param name="pCategory"/>

    <xsl:choose>
      <xsl:when test="$pCategory = 'F'" >
        <xsl:value-of select="$gNull"/>
      </xsl:when>
      <xsl:when test="$pCategory = 'O'">
        <xsl:value-of select="$pValue"/>
      </xsl:when>
    </xsl:choose>

  </xsl:template>

  <!-- Templates communs pour toutes les tables -->
  <xsl:template name="SysIns">
    <xsl:param name="pIsWithControl" select="$gTrue"/>
    <!-- BD 20130513 Ajout du param pUseDtBusiness pour DTENABLED -->
    <xsl:param name="pUseDtBusiness" select="$gFalse"/>

    <column name="DTENABLED" datakey="false" datakeyupd="false" datatype="date">
      <xsl:choose>
        <xsl:when test="string-length($gParamDtBusiness) > 0">
          <xsl:value-of select="$gParamDtBusiness"/>
        </xsl:when>
        <xsl:otherwise>
          <SpheresLib function="GetDateRDBMS()"/>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:if test="$pIsWithControl = $gTrue">
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="IsUpdate()"/>
          </control>
        </controls>
      </xsl:if>
    </column>
    <column name="DTINS" datakey="false" datakeyupd="false" datatype="datetime">
      <SpheresLib function="GetUTCDateTimeSys()"/>
      <xsl:if test="$pIsWithControl = $gTrue">
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="IsUpdate()"/>
          </control>
        </controls>
      </xsl:if>
    </column>
    <column name="IDAINS" datakey="false" datakeyupd="false" datatype="int">
      <SpheresLib function="GetUserID()"/>
      <xsl:if test="$pIsWithControl = $gTrue">
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="IsUpdate()"/>
          </control>
        </controls>
      </xsl:if>
    </column>
  </xsl:template>
  <xsl:template name="SysUpd">
    <xsl:param name="pIsWithControl" select="$gTrue"/>

    <column name="DTUPD" datakey="false" datakeyupd="false" datatype="datetime">
      <SpheresLib function="GetUTCDateTimeSys()"/>
      <xsl:if test="$pIsWithControl = $gTrue">
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="IsInsert()"/>
          </control>
        </controls>
      </xsl:if>
    </column>
    <column name="IDAUPD" datakey="false" datakeyupd="false" datatype="int">
      <SpheresLib function="GetUserID()"/>
      <xsl:if test="$pIsWithControl = $gTrue">
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="IsInsert()"/>
          </control>
        </controls>
      </xsl:if>
    </column>
  </xsl:template>
  <xsl:template name="SysInsUpd">
    <xsl:call-template name="SysIns"/>
    <xsl:call-template name="SysUpd"/>
  </xsl:template>

  <!-- Sql Common -->
  <xsl:template name="SQLMaturityRuleIdentifierFromDerivativeContract">
    <xsl:param name="pParamExchangeSymbol"/>
    <xsl:param name="pParamContractSymbol"/>

    <xsl:param name="pExtSQLFilterNames"/>
    <xsl:param name="pExtSQLFilterValues"/>

    <!-- PL 20130215 Add cache="true" -->
    <SQL command="select" result="IDENTIFIER" cache="true">
      <![CDATA[
      select mr.IDENTIFIER
      from dbo.MATURITYRULE mr
      inner join dbo.DERIVATIVECONTRACT dc on dc.IDMATURITYRULE=mr.IDMATURITYRULE and (dc.CONTRACTSYMBOL=@CONTRACTSYMBOL)
      inner join dbo.MARKET m on m.IDM=dc.IDM and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL)
      where (1=1)
      ]]>
      <xsl:call-template name="SQLDTENABLEDDTDISABLED">
        <xsl:with-param name="pTable" select="'dc'"/>
      </xsl:call-template>
      <xsl:call-template name="SQLWhereClauseExtension">
        <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
      </xsl:call-template>
      <xsl:copy-of select="$pParamContractSymbol"/>
      <xsl:copy-of select="$pParamExchangeSymbol"/>
      <Param name="DT" datatype="date">
        <xsl:value-of select="$gParamDtBusiness"/>
      </Param>
    </SQL>

  </xsl:template>

  <xsl:template name="SQLDerivativeContractBefore">
    <xsl:param name="pResultColumn"/>
    <xsl:param name="pParamISO10383"/>
    <xsl:param name="pParamExchangeSymbol"/>
    <xsl:param name="pParamContractSymbol"/>

    <xsl:param name="pExtSQLFilterNames"/>
    <xsl:param name="pExtSQLFilterValues"/>

    <xsl:choose>
      <xsl:when test="$pParamISO10383 != $gNull and string-length($pParamISO10383) > 0">
        <!-- PL 20130215 Add cache="true" -->
        <SQL command="select" cache="true">
          <xsl:attribute name="result">
            <xsl:value-of select="$pResultColumn"/>
          </xsl:attribute>
          <xsl:text>  
            <![CDATA[        
            select dc.IDENTIFIER, case when dc.ISAUTOSETTING=1 then dc.ISAUTOSETTING else null end as ISAUTOSETTING
            from dbo.DERIVATIVECONTRACT dc
            inner join dbo.MARKET m on (m.IDM=dc.IDM) and (m.ISO10383_ALPHA4=@ISO10383_ALPHA4) and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL)
            where (dc.CONTRACTSYMBOL=@CONTRACTSYMBOL)
            ]]>
          </xsl:text>
          <xsl:call-template name="SQLDTENABLEDDTDISABLED">
            <xsl:with-param name="pTable" select="'dc'"/>
          </xsl:call-template>
          <xsl:call-template name="SQLWhereClauseExtension">
            <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
            <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
          </xsl:call-template>
          <xsl:copy-of select="$pParamISO10383"/>
          <xsl:copy-of select="$pParamExchangeSymbol"/>
          <xsl:copy-of select="$pParamContractSymbol"/>
          <Param name="DT" datatype="date">
            <xsl:value-of select="$gParamDtBusiness"/>
          </Param>
        </SQL>
      </xsl:when>
      <xsl:otherwise>
        <!-- PL 20130215 Add cache="true" -->
        <SQL command="select" cache="true">
          <xsl:attribute name="result">
            <xsl:value-of select="$pResultColumn"/>
          </xsl:attribute>
          <xsl:text>
          select dc.IDENTIFIER, case when dc.ISAUTOSETTING=1 then dc.ISAUTOSETTING else null end as ISAUTOSETTING
          from dbo.DERIVATIVECONTRACT dc
          inner join dbo.MARKET m on (m.IDM=dc.IDM) and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL)
          where (dc.CONTRACTSYMBOL=@CONTRACTSYMBOL)
          </xsl:text>
          <xsl:call-template name="SQLDTENABLEDDTDISABLED">
            <xsl:with-param name="pTable" select="'dc'"/>
          </xsl:call-template>
          <xsl:call-template name="SQLWhereClauseExtension">
            <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
            <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
          </xsl:call-template>
          <xsl:copy-of select="$pParamExchangeSymbol"/>
          <xsl:copy-of select="$pParamContractSymbol"/>
          <Param name="DT" datatype="date">
            <xsl:value-of select="$gParamDtBusiness"/>
          </Param>
        </SQL>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="SQLDerivativeContractAfter">
    <xsl:param name="pResultColumn"/>
    <xsl:param name="pParamExchangeSymbol"/>
    <xsl:param name="pParamContractSymbol"/>

    <xsl:param name="pExtSQLFilterNames"/>
    <xsl:param name="pExtSQLFilterValues"/>

    <!-- PL 20130215 Add cache="true" -->
    <SQL command="select" cache="true">
      <xsl:attribute name="result">
        <xsl:value-of select="$pResultColumn"/>
      </xsl:attribute>
      <xsl:text>
          select dc.IDENTIFIER, dc.IDDC, dc.IDMATURITYRULE, m.ISO10383_ALPHA4 || ' ' || dc.IDENTIFIER as IDENTIFIERMTR,
          case when dc.ISAUTOSETTINGASSET=1 then dc.ISAUTOSETTINGASSET else null end as ISAUTOSETTINGASSET
          from dbo.DERIVATIVECONTRACT dc
          inner join dbo.MARKET m on m.IDM=dc.IDM and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL)
          where (dc.CONTRACTSYMBOL=@CONTRACTSYMBOL)
      </xsl:text>
      <xsl:call-template name="SQLDTENABLEDDTDISABLED">
        <xsl:with-param name="pTable" select="'dc'"/>
      </xsl:call-template>
      <xsl:call-template name="SQLWhereClauseExtension">
        <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
      </xsl:call-template>
      <xsl:copy-of select="$pParamContractSymbol"/>
      <xsl:copy-of select="$pParamExchangeSymbol"/>
      <Param name="DT" datatype="date">
        <xsl:value-of select="$gParamDtBusiness"/>
      </Param>
    </SQL>


  </xsl:template>

  <!-- FI Template destiné à remplacer SQLDerivativeContractAfter -->
  <!-- Prend en considération de MarketISO10383 -->
  <xsl:template name="SQLDerivativeContractAfter2">
    <xsl:param name="pResultColumn"/>
    <xsl:param name="pParamISO10383"/>
    <xsl:param name="pParamExchangeSymbol"/>
    <xsl:param name="pParamContractSymbol"/>

    <xsl:param name="pExtSQLFilterNames"/>
    <xsl:param name="pExtSQLFilterValues"/>

    <SQL command="select" cache="true">
      <xsl:attribute name="result">
        <xsl:value-of select="$pResultColumn"/>
      </xsl:attribute>
      <xsl:text>
            select dc.IDENTIFIER, dc.IDDC, dc.IDMATURITYRULE, m.ISO10383_ALPHA4 || ' ' || dc.IDENTIFIER as IDENTIFIERMTR,
            case when dc.ISAUTOSETTINGASSET=1 then dc.ISAUTOSETTINGASSET else null end as ISAUTOSETTINGASSET
            from dbo.DERIVATIVECONTRACT dc
            inner join dbo.MARKET m on m.IDM=dc.IDM and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL) and (m.ISO10383_ALPHA4=@ISO10383_ALPHA4)
            where (dc.CONTRACTSYMBOL=@CONTRACTSYMBOL)
        </xsl:text>
      <xsl:call-template name="SQLDTENABLEDDTDISABLED">
        <xsl:with-param name="pTable" select="'dc'"/>
      </xsl:call-template>
      <xsl:call-template name="SQLWhereClauseExtension">
        <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
      </xsl:call-template>
      <xsl:copy-of select="$pParamISO10383"/>
      <xsl:copy-of select="$pParamExchangeSymbol"/>
      <xsl:copy-of select="$pParamContractSymbol"/>
      <Param name="DT" datatype="date">
        <xsl:value-of select="$gParamDtBusiness"/>
      </Param>
    </SQL>
  </xsl:template>


  <xsl:template name="SQLIdMaturityRuleBefor">
    <xsl:param name="pParamExchangeSymbol"/>
    <xsl:param name="pParamContractSymbol"/>

    <xsl:param name="pExtSQLFilterNames"/>
    <xsl:param name="pExtSQLFilterValues"/>
    <!-- PL 20130215 Add cache="true" -->
    <SQL command="select" result="IDMATURITYRULE" cache="true">
      select 1 as IDMATURITYRULE
      from dbo.MATURITYRULE mr
      inner join dbo.DERIVATIVECONTRACT dc on (dc.CONTRACTSYMBOL=@CONTRACTSYMBOL)
      inner join dbo.MARKET m on m.IDM=dc.IDM and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL)
      where (mr.IDENTIFIER=rtrim(m.ISO10383_ALPHA4) <xsl:value-of select="$gSqlConcat"/> ' ' <xsl:value-of select="$gSqlConcat"/> dc.IDENTIFIER)
      <xsl:call-template name="SQLDTENABLEDDTDISABLED">
        <xsl:with-param name="pTable" select="'dc'"/>
      </xsl:call-template>
      <xsl:call-template name="SQLWhereClauseExtension">
        <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
      </xsl:call-template>
      <xsl:copy-of select="$pParamContractSymbol"/>
      <xsl:copy-of select="$pParamExchangeSymbol"/>
      <Param name="DT" datatype="date">
        <xsl:value-of select="$gParamDtBusiness"/>
      </Param>
    </SQL>

  </xsl:template>
  <xsl:template name="SQLIdMaturityRuleAfter">
    <xsl:param name="pResultColumn" select="'IDMATURITYRULE'"/>
    <xsl:param name="pParamExchangeSymbol"/>
    <xsl:param name="pParamContractSymbol"/>

    <xsl:param name="pExtSQLFilterNames"/>
    <xsl:param name="pExtSQLFilterValues"/>

    <!-- PL 20130215 Add cache="true" -->
    <SQL command="select" cache="true">
      <xsl:attribute name="result">
        <xsl:value-of select="$pResultColumn"/>
      </xsl:attribute>
      select mr.IDMATURITYRULE, dc.IDDC
      from dbo.MATURITYRULE mr
      inner join dbo.DERIVATIVECONTRACT dc on (dc.CONTRACTSYMBOL=@CONTRACTSYMBOL)
      inner join dbo.MARKET m on m.IDM=dc.IDM and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL)
      where (mr.IDENTIFIER=rtrim(m.ISO10383_ALPHA4) || ' ' ||  dc.IDENTIFIER)
      <xsl:call-template name="SQLDTENABLEDDTDISABLED">
        <xsl:with-param name="pTable" select="'dc'"/>
      </xsl:call-template>
      <xsl:call-template name="SQLWhereClauseExtension">
        <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
      </xsl:call-template>
      <xsl:copy-of select="$pParamContractSymbol"/>
      <xsl:copy-of select="$pParamExchangeSymbol"/>
      <Param name="DT" datatype="date">
        <xsl:value-of select="$gParamDtBusiness"/>
      </Param>
    </SQL>

  </xsl:template>
  <xsl:template name="SQLIsAutoSetting">
    <xsl:param name="pParamExchangeSymbol"/>
    <xsl:param name="pParamContractSymbol"/>

    <xsl:param name="pExtSQLFilterNames"/>
    <xsl:param name="pExtSQLFilterValues"/>

    <!-- PL 20130215 Add cache="true" -->
    <SQL command="select" result="ISAUTOSETTING" cache="true">
      select dc.ISAUTOSETTING
      from dbo.DERIVATIVECONTRACT dc
      inner join dbo.MARKET m on m.IDM=dc.IDM and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL)
      where (dc.CONTRACTSYMBOL=@CONTRACTSYMBOL)
      <xsl:call-template name="SQLDTENABLEDDTDISABLED">
        <xsl:with-param name="pTable" select="'dc'"/>
      </xsl:call-template>
      <xsl:call-template name="SQLWhereClauseExtension">
        <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
      </xsl:call-template>
      <xsl:copy-of select="$pParamContractSymbol"/>
      <xsl:copy-of select="$pParamExchangeSymbol"/>
      <Param name="DT" datatype="date">
        <xsl:value-of select="$gParamDtBusiness"/>
      </Param>
    </SQL>

  </xsl:template>
  <xsl:template name="SQLIdDerivativeContractUnderlying">
    <xsl:param name="pParamISO10383"/>
    <xsl:param name="pParamExchangeSymbol"/>
    <xsl:param name="pParamUnderlyingContractSymbol"/>

    <xsl:param name="pExtSQLFilterNames"/>
    <xsl:param name="pExtSQLFilterValues"/>

    <xsl:choose>
      <xsl:when test="$pParamISO10383 != $gNull">
        <!-- PL 20130215 Add cache="true" -->
        <!-- RD 20160427 [22109] Add condition "dc.CATEGORY=@CATEGORYFUTURE"-->
        <SQL command="select" result="IDDC" cache="true">
          select dc.IDDC
          from dbo.DERIVATIVECONTRACT dc
          inner join dbo.MARKET m on (m.IDM=dc.IDM) and (m.ISO10383_ALPHA4=@ISO10383_ALPHA4) and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL)
          where (dc.CATEGORY=@CATEGORYFUTURE and dc.CONTRACTSYMBOL=@UNDERLYINGCONTRACTSYMBOL)
          <xsl:call-template name="SQLDTENABLEDDTDISABLED">
            <xsl:with-param name="pTable" select="'dc'"/>
          </xsl:call-template>
          <xsl:call-template name="SQLWhereClauseExtension">
            <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
            <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
          </xsl:call-template>
          <xsl:copy-of select="$pParamISO10383"/>
          <xsl:copy-of select="$pParamExchangeSymbol"/>
          <xsl:copy-of select="$pParamUnderlyingContractSymbol"/>
          <xsl:copy-of select="$gParamCategoryFuture"/>
          <Param name="DT" datatype="date">
            <xsl:value-of select="$gParamDtBusiness"/>
          </Param>
        </SQL>
      </xsl:when>
      <xsl:otherwise>
        <!-- PL 20130215 Add cache="true" -->
        <!-- RD 20160427 [22109] Add condition "dc.CATEGORY=@CATEGORYFUTURE"-->
        <SQL command="select" result="IDDC" cache="true">
          select dc.IDDC
          from dbo.DERIVATIVECONTRACT dc
          inner join dbo.MARKET m on (m.IDM=dc.IDM) and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL )
          where (dc.CATEGORY=@CATEGORYFUTURE and dc.CONTRACTSYMBOL=@UNDERLYINGCONTRACTSYMBOL)
          <xsl:call-template name="SQLDTENABLEDDTDISABLED">
            <xsl:with-param name="pTable" select="'dc'"/>
          </xsl:call-template>
          <xsl:call-template name="SQLWhereClauseExtension">
            <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
            <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
          </xsl:call-template>
          <xsl:copy-of select="$pParamExchangeSymbol"/>
          <xsl:copy-of select="$pParamUnderlyingContractSymbol"/>
          <xsl:copy-of select="$gParamCategoryFuture"/>
          <Param name="DT" datatype="date">
            <xsl:value-of select="$gParamDtBusiness"/>
          </Param>
        </SQL>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="SQLIdInstrument">
    <xsl:param name="pParamInstrumentIdentifier"/>
    <!-- PL 20130215 Add cache="true" -->
    <SQL command="select" result="IDI" cache="true">
      select i.IDI
      from dbo.INSTRUMENT i
      where (i.IDENTIFIER=@INSTRUMENTIDENTIFIER)
      <xsl:copy-of select="$pParamInstrumentIdentifier"/>
    </SQL>
  </xsl:template>
  <xsl:template name="SQLIdMarket">
    <!--FL 20171006 [23495]-->
    <!-- <xsl:param name="pParamISO10383" select="$gNull"/> -->
    <xsl:param name="pParamISO10383" select="$gNull"/>
    <xsl:param name="pParamExchangeSymbol"/>

    <xsl:choose>
      <xsl:when test="$pParamISO10383 != $gNull">
        <!-- PL 20130215 Add cache="true" -->
        <SQL command="select" result="IDM" cache="true">
          select IDM
          from dbo.MARKET
          where (ISO10383_ALPHA4=@ISO10383_ALPHA4) and (EXCHANGESYMBOL=@EXCHANGESYMBOL)
          <xsl:copy-of select="$pParamISO10383"/>
          <xsl:copy-of select="$pParamExchangeSymbol"/>
        </SQL>
      </xsl:when>
      <xsl:otherwise>
        <!-- PL 20130215 Add cache="true" -->
        <SQL command="select" result="IDM" cache="true">
          select IDM
          from dbo.MARKET
          where (EXCHANGESYMBOL=@EXCHANGESYMBOL)
          <xsl:copy-of select="$pParamExchangeSymbol"/>
        </SQL>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- SQLIdMarket2: Nouveau template destiné à remplacer SQLIdMarket -->
  <xsl:template name="SQLIdMarket2">
    <xsl:param name="pISO10383"/>
    <xsl:param name="pExchangeSymbol"/>
    <!--FL/RD 20140326 [19648] Add new optional parameter pParamExchangeSymbol-->
    <xsl:param name="pParamExchangeSymbol"/>
    <xsl:param name="pExchangeAcronym"/>

    <!--FL/RD 20140326 [19648] Use new optional parameter pParamExchangeSymbol-->
    <xsl:variable name="vParamExchangeSymbol">
      <xsl:choose>
        <xsl:when test="$pParamExchangeSymbol">
          <xsl:copy-of select="$pParamExchangeSymbol"/>
        </xsl:when>
        <xsl:otherwise>
          <Param name="EXCHANGESYMBOL" datatype="string">
            <xsl:value-of select="$pExchangeSymbol"/>
          </Param>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:choose>
      <!--<xsl:when test="$pISO10383 != $gNull">-->
      <xsl:when test="string-length($pISO10383) > 0">
        <xsl:choose>
          <xsl:when test="string-length($pExchangeSymbol) > 0">
            <SQL command="select" result="IDM" cache="true">
              select IDM
              from dbo.MARKET
              where (ISO10383_ALPHA4=@ISO10383_ALPHA4) and (EXCHANGESYMBOL=@EXCHANGESYMBOL)
              <Param name="ISO10383_ALPHA4" datatype="string">
                <xsl:value-of select="$pISO10383"/>
              </Param>
              <!--FL/RD 20140326 [19648] Use new optional parameter pParamExchangeSymbol-->
              <!--<Param name="EXCHANGESYMBOL" datatype="string">
                <xsl:value-of select="$pExchangeSymbol"/>
              </Param>-->
              <xsl:copy-of select="$vParamExchangeSymbol"/>
            </SQL>
          </xsl:when>
          <xsl:when test="string-length($pExchangeAcronym) > 0">
            <SQL command="select" result="IDM" cache="true">
              select IDM
              from dbo.MARKET
              where (ISO10383_ALPHA4=@ISO10383_ALPHA4) and (EXCHANGEACRONYM=@EXCHANGEACRONYM)
              <Param name="ISO10383_ALPHA4" datatype="string">
                <xsl:value-of select="$pISO10383"/>
              </Param>
              <Param name="EXCHANGEACRONYM" datatype="string">
                <xsl:value-of select="$pExchangeAcronym"/>
              </Param>
            </SQL>
          </xsl:when>
          <xsl:otherwise>
            <SQL command="select" result="IDM" cache="true">
              select IDM
              from dbo.MARKET
              where (ISO10383_ALPHA4=@ISO10383_ALPHA4)
              <Param name="ISO10383_ALPHA4" datatype="string">
                <xsl:value-of select="$pISO10383"/>
              </Param>
            </SQL>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <!-- otherwise: a supprimer plus tard -->
        <SQL command="select" result="IDM" cache="true">
          select IDM
          from dbo.MARKET
          where (EXCHANGESYMBOL=@EXCHANGESYMBOL)
          <!--FL/RD 20140326 [19648] Use new optional parameter pParamExchangeSymbol-->
          <!--<Param name="EXCHANGESYMBOL" datatype="string">
            <xsl:value-of select="$pExchangeSymbol"/>
          </Param>-->
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </SQL>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="SQLIdCurrency">
    <xsl:param name="pParamCurrency"/>

    <xsl:param name="pExtSQLFilterNames"/>
    <xsl:param name="pExtSQLFilterValues"/>

    <!-- PL 20130215 Add cache="true" -->
    <SQL command="select" result="IDC" cache="true">
      select c.IDC
      from dbo.CURRENCY c
      where (c.IDC=@CURRENCY)
      <xsl:call-template name="SelectOrFilterColumn">
        <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
        <xsl:with-param name="pResultField" select="'IDC_PRICE'"/>
        <xsl:with-param name="pFilterField" select="'c.IDC'"/>
      </xsl:call-template>
      <xsl:copy-of select="$pParamCurrency"/>
    </SQL>
  </xsl:template>

  <!--FL 20171006 [23495] BMECLEARING New Template "SQLIdMaturityRule2" -->
  <xsl:template name="SQLIdMaturityRule2">
    <xsl:param name="pExtSQLFilterNames"/>
    <xsl:param name="pExtSQLFilterValues"/>

    <SQL command="select" result="IDMATURITYRULE" cache="true">
      select min(mr.IDMATURITYRULE) as IDMATURITYRULE
      from dbo.MATURITYRULE mr
      where (mr.IDENTIFIER!=mr.IDENTIFIER)
      <xsl:call-template name="SelectOrFilterColumn">
        <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
        <xsl:with-param name="pResultField" select="'IDMATURITYRULE'"/>
        <xsl:with-param name="pFilterField" select="'mr.IDMATURITYRULE'"/>
      </xsl:call-template>
    </SQL>
  </xsl:template>

  <xsl:template name="SQLIdMaturityRule">
    <!-- FL/PL 20130124 IDEM: Nouveau template "SQLIdMaturityRule" -->
    <xsl:param name="pMaturityRuleIdentifier"/>

    <!-- PL 20130215 Add cache="true" -->
    <!-- FL 20131119 Modify Query "select IDMATURITYRULE" became "select min(IDMATURITYRULE) as IDMATURITYRULE" -->
    <SQL command="select" result="IDMATURITYRULE" cache="true">
      select min(IDMATURITYRULE) as IDMATURITYRULE
      from dbo.MATURITYRULE
      where (IDENTIFIER=@IDENTIFIER)
      <Param name="IDENTIFIER" datatype="string">
        <xsl:value-of select="$pMaturityRuleIdentifier"/>
      </Param>
    </SQL>
  </xsl:template>

  <!-- FI  Template destiné à remplacer SQLIdMaturity -->
  <!-- Prend en considération de MarketISO10383 -->
  <xsl:template name="SQLIdMaturity2">
    <xsl:param name="pParamISO10383"/>
    <xsl:param name="pParamExchangeSymbol"/>
    <xsl:param name="pParamContractSymbol"/>
    <xsl:param name="pParamMaturityMonthYear"/>

    <xsl:param name="pExtSQLFilterNames"/>
    <xsl:param name="pExtSQLFilterValues"/>

    <!-- PL 20130215 Add cache="true" -->
    <SQL command="select" result="IDMATURITY" cache="true">
      <xsl:text>
          select ma.IDMATURITY
          from dbo.MATURITY ma
          inner join dbo.DERIVATIVECONTRACT dc on dc.IDMATURITYRULE=ma.IDMATURITYRULE and (dc.CONTRACTSYMBOL=@CONTRACTSYMBOL)
          inner join dbo.MARKET m on m.IDM=dc.IDM and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL) and (m.ISO10383_ALPHA4=@ISO10383_ALPHA4)
          where (ma.MATURITYMONTHYEAR=@MATURITYMONTHYEAR)
      </xsl:text>
      <xsl:call-template name="SQLDTENABLEDDTDISABLED">
        <xsl:with-param name="pTable" select="'dc'"/>
      </xsl:call-template>
      <xsl:call-template name="SQLWhereClauseExtension">
        <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
      </xsl:call-template>
      <xsl:copy-of select="$pParamContractSymbol"/>
      <xsl:copy-of select="$pParamISO10383"/>
      <xsl:copy-of select="$pParamExchangeSymbol"/>
      <xsl:copy-of select="$pParamMaturityMonthYear"/>
      <Param name="DT" datatype="date">
        <xsl:value-of select="$gParamDtBusiness"/>
      </Param>
    </SQL>
  </xsl:template>
  <xsl:template name="SQLIdMaturity">
    <xsl:param name="pParamExchangeSymbol"/>
    <xsl:param name="pParamContractSymbol"/>
    <xsl:param name="pParamMaturityMonthYear"/>

    <xsl:param name="pExtSQLFilterNames"/>
    <xsl:param name="pExtSQLFilterValues"/>

    <!-- PL 20130215 Add cache="true" -->
    <SQL command="select" result="IDMATURITY" cache="true">
      select ma.IDMATURITY
      from dbo.MATURITY ma
      inner join dbo.DERIVATIVECONTRACT dc on dc.IDMATURITYRULE=ma.IDMATURITYRULE and (dc.CONTRACTSYMBOL=@CONTRACTSYMBOL)
      inner join dbo.MARKET m on m.IDM=dc.IDM and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL)
      where (ma.MATURITYMONTHYEAR=@MATURITYMONTHYEAR)
      <xsl:call-template name="SQLDTENABLEDDTDISABLED">
        <xsl:with-param name="pTable" select="'dc'"/>
      </xsl:call-template>
      <xsl:call-template name="SQLWhereClauseExtension">
        <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
      </xsl:call-template>
      <xsl:copy-of select="$pParamContractSymbol"/>
      <xsl:copy-of select="$pParamExchangeSymbol"/>
      <xsl:copy-of select="$pParamMaturityMonthYear"/>
      <Param name="DT" datatype="date">
        <xsl:value-of select="$gParamDtBusiness"/>
      </Param>
    </SQL>

  </xsl:template>


  <!-- FI  Template destiné à remplacer SQLIdAssetUnderlying -->
  <!-- Prend en considération de MarketISO10383 -->
  <xsl:template name="SQLIdAssetUnderlying2">
    <xsl:param name="pParamISO10383"/>
    <xsl:param name="pParamExchangeSymbol"/>
    <xsl:param name="pParamUnderlyingContractSymbol"/>
    <xsl:param name="pParamUnderlyingAssetSymbol"/>

    <xsl:param name="pExtSQLFilterNames"/>
    <xsl:param name="pExtSQLFilterValues"/>

    <!-- PL 20130215 Add cache="true" -->
    <SQL command="select" result="IDASSET" cache="true">
      <xsl:text>
        select asset.IDASSET
        from dbo.ASSET_ETD asset
        inner join dbo.DERIVATIVEATTRIB da on da.IDDERIVATIVEATTRIB=asset.IDDERIVATIVEATTRIB
        inner join dbo.DERIVATIVECONTRACT dc on dc.IDDC=da.IDDC and (dc.CONTRACTSYMBOL=@UNDERLYINGCONTRACTSYMBOL)
        inner join dbo.MARKET m on m.IDM=dc.IDM and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL) and (m.ISO10383_ALPHA4=@ISO10383_ALPHA4)
        where (asset.ASSETSYMBOL=@UNDERLYINGASSETSYMBOL)
      </xsl:text>
      <xsl:call-template name="SQLDTENABLEDDTDISABLED">
        <xsl:with-param name="pTable" select="'dc'"/>
      </xsl:call-template>
      <xsl:call-template name="SQLWhereClauseExtension">
        <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
      </xsl:call-template>
      <xsl:copy-of select="$pParamISO10383"/>
      <xsl:copy-of select="$pParamExchangeSymbol"/>
      <xsl:copy-of select="$pParamUnderlyingContractSymbol"/>
      <xsl:copy-of select="$pParamUnderlyingAssetSymbol"/>
      <Param name="DT" datatype="date">
        <xsl:value-of select="$gParamDtBusiness"/>
      </Param>
    </SQL>

  </xsl:template>
  <xsl:template name="SQLIdAssetUnderlying">
    <xsl:param name="pParamExchangeSymbol"/>
    <xsl:param name="pParamUnderlyingContractSymbol"/>
    <xsl:param name="pParamUnderlyingAssetSymbol"/>

    <xsl:param name="pExtSQLFilterNames"/>
    <xsl:param name="pExtSQLFilterValues"/>

    <!-- PL 20130215 Add cache="true" -->
    <SQL command="select" result="IDASSET" cache="true">
      select asset.IDASSET
      from dbo.ASSET_ETD asset
      inner join dbo.DERIVATIVEATTRIB da on da.IDDERIVATIVEATTRIB=asset.IDDERIVATIVEATTRIB
      inner join dbo.DERIVATIVECONTRACT dc on dc.IDDC=da.IDDC and (dc.CONTRACTSYMBOL=@UNDERLYINGCONTRACTSYMBOL)
      inner join dbo.MARKET m on m.IDM=dc.IDM and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL)
      where (asset.ASSETSYMBOL=@UNDERLYINGASSETSYMBOL)
      <xsl:call-template name="SQLDTENABLEDDTDISABLED">
        <xsl:with-param name="pTable" select="'dc'"/>
      </xsl:call-template>
      <xsl:call-template name="SQLWhereClauseExtension">
        <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
      </xsl:call-template>
      <xsl:copy-of select="$pParamUnderlyingContractSymbol"/>
      <xsl:copy-of select="$pParamUnderlyingAssetSymbol"/>
      <xsl:copy-of select="$pParamExchangeSymbol"/>
      <Param name="DT" datatype="date">
        <xsl:value-of select="$gParamDtBusiness"/>
      </Param>
    </SQL>

  </xsl:template>

  <xsl:template name="SQLIdAssetUnderlyingWithoutContractSymbol">
    <xsl:param name="pParamUnderlyingAssetSymbol"/>

    <!-- PL 20130215 Add cache="true" -->
    <SQL command="select" result="IDASSET" cache="true">
      select asset.IDASSET
      from dbo.ASSET_ETD asset
      where(asset.ASSETSYMBOL=@UNDERLYINGASSETSYMBOL)
      <xsl:copy-of select="$pParamUnderlyingAssetSymbol"/>
    </SQL>

  </xsl:template>

  <!-- FI  Template destiné à remplacer SQLAssetIdentifier -->
  <!-- Prend en considération de MarketISO10383 -->
  <xsl:template name="SQLAssetIdentifier2">
    <xsl:param name="pSource" select="$gNull"/>
    <xsl:param name="pResultColumn" select="'IDENTIFIER'"/>
    <xsl:param name="pParamISO10383"/>
    <xsl:param name="pParamExchangeSymbol"/>
    <xsl:param name="pParamContractSymbol"/>
    <xsl:param name="pParamMaturityMonthYear"/>
    <xsl:param name="pParamStrikePrice"/>
    <xsl:param name="pParamPutCall"/>
    <xsl:param name="pContractAttribute" select="$gNull"/>

    <xsl:param name="pExtSQLFilterNames"/>
    <xsl:param name="pExtSQLFilterValues"/>

    <!-- PL 20130215 Add cache="true" -->
    <SQL command="select" cache="true">
      <xsl:attribute name="result">
        <xsl:value-of select="$pResultColumn"/>
      </xsl:attribute>

      select <xsl:if test="$pSource != $gNull">
        <xsl:value-of select="$pSource"/> as RESULT,
      </xsl:if>
      asset.IDENTIFIER, asset.DISPLAYNAME, asset.ISINCODE, asset.AII, asset.ASSETSYMBOL
      from dbo.ASSET_ETD asset
      inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB=asset.IDDERIVATIVEATTRIB)
      inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC=da.IDDC) and (dc.CONTRACTSYMBOL=@CONTRACTSYMBOL)
      inner join dbo.MARKET m on m.IDM=dc.IDM and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL) and (m.ISO10383_ALPHA4=@ISO10383_ALPHA4)
      inner join dbo.MATURITY ma on ((ma.IDMATURITY=da.IDMATURITY) and (ma.IDMATURITYRULE=dc.IDMATURITYRULE)) and (ma.MATURITYMONTHYEAR=@MATURITYMONTHYEAR)
      inner join dbo.MATURITYRULE mr on ((mr.IDMATURITYRULE=dc.IDMATURITYRULE) and (mr.IDMATURITYRULE=ma.IDMATURITYRULE))
      where (dc.CATEGORY=@CATEGORYFUTURE or (dc.CATEGORY=@CATEGORYOPTION and asset.STRIKEPRICE=@STRIKEPRICE and asset.PUTCALL=@PUTCALL))
      <!-- Phil : the CONTRACTATTRIBUTE value is not used anymore for the DERIVATIVEATTRIB elements-->
      <!--<xsl:if test="$gNull != $pContractAttribute">
        and ( (da.CONTRACTATTRIBUTE is null and  dc.CONTRACTATTRIBUTE = @CONTRACTATTRIBUTE ) or (da.CONTRACTATTRIBUTE = @CONTRACTATTRIBUTE))
      </xsl:if>-->
      <xsl:call-template name="SQLDTENABLEDDTDISABLED">
        <xsl:with-param name="pTable" select="'dc'"/>
      </xsl:call-template>
      <xsl:call-template name="SQLWhereClauseExtension">
        <xsl:with-param name="pNames" select="concat($pExtSQLFilterNames,',','ALIASTBLDC')"/>
        <xsl:with-param name="pValues" select="concat($pExtSQLFilterValues, ',','dc')"/>
      </xsl:call-template>
      <xsl:copy-of select="$pParamContractSymbol"/>
      <xsl:copy-of select="$pParamISO10383"/>
      <xsl:copy-of select="$pParamExchangeSymbol"/>
      <xsl:copy-of select="$pParamMaturityMonthYear"/>
      <xsl:copy-of select="$gParamCategoryFuture"/>
      <xsl:copy-of select="$gParamCategoryOption"/>
      <xsl:copy-of select="$pParamStrikePrice"/>
      <xsl:copy-of select="$pParamPutCall"/>
      <xsl:if test="$gNull != $pContractAttribute">
        <xsl:variable name="vParamContractAttribute">
          <Param name="CONTRACTATTRIBUTE" datatype="string">
            <xsl:value-of select="$pContractAttribute"/>
          </Param>
        </xsl:variable>
        <xsl:copy-of select="$vParamContractAttribute"/>
      </xsl:if>
      <Param name="DT" datatype="date">
        <xsl:value-of select="$gParamDtBusiness"/>
      </Param>
    </SQL>
  </xsl:template>
  <xsl:template name="SQLAssetIdentifier">
    <xsl:param name="pSource" select="$gNull"/>
    <xsl:param name="pResultColumn" select="'IDENTIFIER'"/>
    <xsl:param name="pParamExchangeSymbol"/>
    <xsl:param name="pParamContractSymbol"/>
    <xsl:param name="pParamMaturityMonthYear"/>
    <xsl:param name="pParamStrikePrice"/>
    <xsl:param name="pParamPutCall"/>
    <xsl:param name="pContractAttribute" select="$gNull"/>

    <xsl:param name="pExtSQLFilterNames"/>
    <xsl:param name="pExtSQLFilterValues"/>

    <!-- PL 20130215 Add cache="true" -->
    <SQL command="select" cache="true">
      <xsl:attribute name="result">
        <xsl:value-of select="$pResultColumn"/>
      </xsl:attribute>

      select <xsl:if test="$pSource != $gNull">
        <xsl:value-of select="$pSource"/> as RESULT,
      </xsl:if>
      asset.IDENTIFIER, asset.DISPLAYNAME, asset.ISINCODE, asset.AII, asset.ASSETSYMBOL
      from dbo.ASSET_ETD asset
      inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB=asset.IDDERIVATIVEATTRIB)
      inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC=da.IDDC) and (dc.CONTRACTSYMBOL=@CONTRACTSYMBOL)
      inner join dbo.MARKET m on m.IDM=dc.IDM and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL)
      inner join dbo.MATURITY ma on ((ma.IDMATURITY=da.IDMATURITY) and (ma.IDMATURITYRULE=dc.IDMATURITYRULE)) and (ma.MATURITYMONTHYEAR=@MATURITYMONTHYEAR)
      inner join dbo.MATURITYRULE mr on ((mr.IDMATURITYRULE=dc.IDMATURITYRULE) and (mr.IDMATURITYRULE=ma.IDMATURITYRULE))
      where (dc.CATEGORY=@CATEGORYFUTURE or (dc.CATEGORY=@CATEGORYOPTION and asset.STRIKEPRICE=@STRIKEPRICE and asset.PUTCALL=@PUTCALL))
      <!-- Phil : the CONTRACTATTRIBUTE value is not used anymore for the DERIVATIVEATTRIB elements-->
      <!--<xsl:if test="$gNull != $pContractAttribute">
        and ( (da.CONTRACTATTRIBUTE is null and  dc.CONTRACTATTRIBUTE = @CONTRACTATTRIBUTE ) or (da.CONTRACTATTRIBUTE = @CONTRACTATTRIBUTE))
      </xsl:if>-->
      <xsl:call-template name="SQLDTENABLEDDTDISABLED">
        <xsl:with-param name="pTable" select="'dc'"/>
      </xsl:call-template>
      <xsl:call-template name="SQLWhereClauseExtension">
        <xsl:with-param name="pNames" select="concat($pExtSQLFilterNames,',','ALIASTBLDC')"/>
        <xsl:with-param name="pValues" select="concat($pExtSQLFilterValues, ',','dc')"/>
      </xsl:call-template>
      <xsl:copy-of select="$pParamContractSymbol"/>
      <xsl:copy-of select="$pParamExchangeSymbol"/>
      <xsl:copy-of select="$pParamMaturityMonthYear"/>
      <xsl:copy-of select="$gParamCategoryFuture"/>
      <xsl:copy-of select="$gParamCategoryOption"/>
      <xsl:copy-of select="$pParamStrikePrice"/>
      <xsl:copy-of select="$pParamPutCall"/>
      <xsl:if test="$gNull != $pContractAttribute">
        <xsl:variable name="vParamContractAttribute">
          <Param name="CONTRACTATTRIBUTE" datatype="string">
            <xsl:value-of select="$pContractAttribute"/>
          </Param>
        </xsl:variable>
        <xsl:copy-of select="$vParamContractAttribute"/>
      </xsl:if>
      <Param name="DT" datatype="date">
        <xsl:value-of select="$gParamDtBusiness"/>
      </Param>
    </SQL>
  </xsl:template>

  <!-- FI  Template destiné à remplacer SQLAssetIdentifierExistsAndRowChanged -->
  <!-- Prend en considération de MarketISO10383 -->
  <xsl:template name="SQLAssetIdentifierExistsAndRowChanged2">
    <xsl:param name="pResultColumn" select="'RESULT'"/>
    <xsl:param name="pParamISO10383"/>
    <xsl:param name="pParamExchangeSymbol"/>
    <xsl:param name="pParamContractSymbol"/>
    <xsl:param name="pParamMaturityMonthYear"/>
    <xsl:param name="pParamStrikePrice"/>
    <xsl:param name="pParamPutCall"/>
    <xsl:param name="pContractAttribute" select="$gNull"/>
    <xsl:param name="pAssetDisplayName"/>
    <xsl:param name="pAssetISINCode"/>
    <xsl:param name="pAssetSymbol"/>
    <xsl:param name="pAssetFirstQuotationDay"/>

    <xsl:param name="pExtSQLFilterNames"/>
    <xsl:param name="pExtSQLFilterValues"/>

    <!-- Constituer le SqlWhereUpdKey-->
    <xsl:variable name="vSqlWhereUpdKey1">
      <xsl:if test="string-length($pAssetDisplayName) > 0"> asset.DISPLAYNAME is null</xsl:if>
      <xsl:if test="string-length($pAssetISINCode) > 0"> or asset.ISINCODE is null</xsl:if>
      <xsl:if test="string-length($pAssetSymbol) > 0"> or asset.ASSETSYMBOL is null</xsl:if>
      <xsl:if test="string-length($pAssetFirstQuotationDay) > 0"> or asset.FIRSTQUOTATIONDAY is null</xsl:if>
    </xsl:variable>
    <!-- Enlever l'eventuel OR au début-->
    <xsl:variable name="vSqlWhereUpdKey2">
      <xsl:choose>
        <xsl:when test="normalize-space(substring($vSqlWhereUpdKey1, 1, 3)) = ' or'">
          <xsl:value-of select="substring-after($vSqlWhereUpdKey1,'or')"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$vSqlWhereUpdKey1"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- Constituer le case when-->
    <xsl:variable name="vSqlWhereUpdKey3">
      <xsl:choose>
        <xsl:when test="string-length($vSqlWhereUpdKey2) > 0">
          case when (<xsl:value-of select="$vSqlWhereUpdKey2"/>) then 1 else null end
        </xsl:when>
        <xsl:otherwise>
          1
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:call-template name="SQLAssetIdentifier2">
      <xsl:with-param name="pSource">
        <xsl:copy-of select="$vSqlWhereUpdKey3"/>
      </xsl:with-param>
      <xsl:with-param name="pResultColumn">
        <xsl:copy-of select="$pResultColumn"/>
      </xsl:with-param>
      <xsl:with-param name="pParamISO10383">
        <xsl:copy-of select="$pParamISO10383"/>
      </xsl:with-param>
      <xsl:with-param name="pParamExchangeSymbol">
        <xsl:copy-of select="$pParamExchangeSymbol"/>
      </xsl:with-param>
      <xsl:with-param name="pParamContractSymbol">
        <xsl:copy-of select="$pParamContractSymbol"/>
      </xsl:with-param>
      <xsl:with-param name="pParamMaturityMonthYear">
        <xsl:copy-of select="$pParamMaturityMonthYear"/>
      </xsl:with-param>
      <xsl:with-param name="pParamStrikePrice">
        <xsl:copy-of select="$pParamStrikePrice"/>
      </xsl:with-param>
      <xsl:with-param name="pParamPutCall">
        <xsl:copy-of select="$pParamPutCall"/>
      </xsl:with-param>
      <xsl:with-param name="pContractAttribute">
        <xsl:copy-of select="$pContractAttribute"/>
      </xsl:with-param>

      <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
      <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

    </xsl:call-template>
  </xsl:template>
  <xsl:template name="SQLAssetIdentifierExistsAndRowChanged">
    <xsl:param name="pResultColumn" select="'RESULT'"/>
    <xsl:param name="pParamExchangeSymbol"/>
    <xsl:param name="pParamContractSymbol"/>
    <xsl:param name="pParamMaturityMonthYear"/>
    <xsl:param name="pParamStrikePrice"/>
    <xsl:param name="pParamPutCall"/>
    <xsl:param name="pContractAttribute" select="$gNull"/>
    <xsl:param name="pAssetDisplayName"/>
    <xsl:param name="pAssetISINCode"/>
    <xsl:param name="pAssetSymbol"/>
    <xsl:param name="pAssetFirstQuotationDay"/>

    <xsl:param name="pExtSQLFilterNames"/>
    <xsl:param name="pExtSQLFilterValues"/>

    <!-- Constituer le SqlWhereUpdKey-->
    <xsl:variable name="vSqlWhereUpdKey1">
      <xsl:if test="string-length($pAssetDisplayName) > 0"> asset.DISPLAYNAME is null</xsl:if>
      <xsl:if test="string-length($pAssetISINCode) > 0"> or asset.ISINCODE is null</xsl:if>
      <xsl:if test="string-length($pAssetSymbol) > 0"> or asset.ASSETSYMBOL is null</xsl:if>
      <xsl:if test="string-length($pAssetFirstQuotationDay) > 0"> or asset.FIRSTQUOTATIONDAY is null</xsl:if>
    </xsl:variable>
    <!-- Enlever l'eventuel OR au début-->
    <xsl:variable name="vSqlWhereUpdKey2">
      <xsl:choose>
        <xsl:when test="normalize-space(substring($vSqlWhereUpdKey1, 1, 3)) = ' or'">
          <xsl:value-of select="substring-after($vSqlWhereUpdKey1,'or')"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$vSqlWhereUpdKey1"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- Constituer le case when-->
    <xsl:variable name="vSqlWhereUpdKey3">
      <xsl:choose>
        <xsl:when test="string-length($vSqlWhereUpdKey2) > 0">
          case when (<xsl:value-of select="$vSqlWhereUpdKey2"/>) then 1 else null end
        </xsl:when>
        <xsl:otherwise>
          1
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:call-template name="SQLAssetIdentifier">
      <xsl:with-param name="pSource">
        <xsl:copy-of select="$vSqlWhereUpdKey3"/>
      </xsl:with-param>
      <xsl:with-param name="pResultColumn">
        <xsl:copy-of select="$pResultColumn"/>
      </xsl:with-param>
      <xsl:with-param name="pParamExchangeSymbol">
        <xsl:copy-of select="$pParamExchangeSymbol"/>
      </xsl:with-param>
      <xsl:with-param name="pParamContractSymbol">
        <xsl:copy-of select="$pParamContractSymbol"/>
      </xsl:with-param>
      <xsl:with-param name="pParamMaturityMonthYear">
        <xsl:copy-of select="$pParamMaturityMonthYear"/>
      </xsl:with-param>
      <xsl:with-param name="pParamStrikePrice">
        <xsl:copy-of select="$pParamStrikePrice"/>
      </xsl:with-param>
      <xsl:with-param name="pParamPutCall">
        <xsl:copy-of select="$pParamPutCall"/>
      </xsl:with-param>
      <xsl:with-param name="pContractAttribute">
        <xsl:copy-of select="$pContractAttribute"/>
      </xsl:with-param>

      <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
      <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

    </xsl:call-template>
  </xsl:template>

  <!-- FI  Template destiné à remplacer SQLIdDerivativeAttrib -->
  <!-- Prend en considération de MarketISO10383 -->
  <xsl:template name="SQLIdDerivativeAttrib2">
    <xsl:param name="pParamISO10383"/>
    <xsl:param name="pParamExchangeSymbol"/>
    <xsl:param name="pParamContractSymbol"/>
    <xsl:param name="pParamMaturityMonthYear"/>

    <xsl:param name="pExtSQLFilterNames"/>
    <xsl:param name="pExtSQLFilterValues"/>

    <!-- PL 20130215 Add cache="true" -->
    <SQL command="select" result="IDDERIVATIVEATTRIB" cache="true">
      <xsl:text>
        select da.IDDERIVATIVEATTRIB
        from dbo.DERIVATIVEATTRIB da
        inner join dbo.DERIVATIVECONTRACT dc on dc.IDDC=da.IDDC and (dc.CONTRACTSYMBOL=@CONTRACTSYMBOL)
        inner join dbo.MARKET m on m.IDM=dc.IDM and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL) and (m.ISO10383_ALPHA4=@ISO10383_ALPHA4)
        inner join dbo.MATURITYRULE mr on mr.IDMATURITYRULE=dc.IDMATURITYRULE
        inner join dbo.MATURITY ma on ma.IDMATURITY=da.IDMATURITY and ma.IDMATURITYRULE=mr.IDMATURITYRULE and (ma.MATURITYMONTHYEAR=@MATURITYMONTHYEAR)
        where (1=1)
      </xsl:text>
      <xsl:call-template name="SQLDTENABLEDDTDISABLED">
        <xsl:with-param name="pTable" select="'dc'"/>
      </xsl:call-template>
      <xsl:call-template name="SQLWhereClauseExtension">
        <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
      </xsl:call-template>
      <xsl:copy-of select="$pParamISO10383"/>
      <xsl:copy-of select="$pParamContractSymbol"/>
      <xsl:copy-of select="$pParamExchangeSymbol"/>
      <xsl:copy-of select="$pParamMaturityMonthYear"/>
      <Param name="DT" datatype="date">
        <xsl:value-of select="$gParamDtBusiness"/>
      </Param>
    </SQL>

  </xsl:template>
  <xsl:template name="SQLIdDerivativeAttrib">
    <xsl:param name="pParamExchangeSymbol"/>
    <xsl:param name="pParamContractSymbol"/>
    <xsl:param name="pParamMaturityMonthYear"/>

    <xsl:param name="pExtSQLFilterNames"/>
    <xsl:param name="pExtSQLFilterValues"/>

    <!-- PL 20130215 Add cache="true" -->
    <SQL command="select" result="IDDERIVATIVEATTRIB" cache="true">
      select da.IDDERIVATIVEATTRIB
      from dbo.DERIVATIVEATTRIB da
      inner join dbo.DERIVATIVECONTRACT dc on dc.IDDC=da.IDDC and (dc.CONTRACTSYMBOL=@CONTRACTSYMBOL)
      inner join dbo.MARKET m on m.IDM=dc.IDM and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL)
      inner join dbo.MATURITYRULE mr on mr.IDMATURITYRULE=dc.IDMATURITYRULE
      inner join dbo.MATURITY ma on ma.IDMATURITY=da.IDMATURITY and ma.IDMATURITYRULE=mr.IDMATURITYRULE and (ma.MATURITYMONTHYEAR=@MATURITYMONTHYEAR)
      where (1=1)
      <xsl:call-template name="SQLDTENABLEDDTDISABLED">
        <xsl:with-param name="pTable" select="'dc'"/>
      </xsl:call-template>
      <xsl:call-template name="SQLWhereClauseExtension">
        <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
      </xsl:call-template>
      <xsl:copy-of select="$pParamContractSymbol"/>
      <xsl:copy-of select="$pParamExchangeSymbol"/>
      <xsl:copy-of select="$pParamMaturityMonthYear"/>
      <Param name="DT" datatype="date">
        <xsl:value-of select="$gParamDtBusiness"/>
      </Param>
    </SQL>

  </xsl:template>

  <!-- Virtual Template -->
  <!-- RD 20111110 -->
  <!-- Retourner des valeurs par défaut dans le cas où la contrat n'existe pas dans la table EXCHANGELIST -->
  <!-- FI 20120221 add INSTRUMENTDEN -->
  <!-- FI 20131121 [19216] add CONTRACTTYPE (CONTRACTTYPE retourne tjs 'STD' en attendant une éventuelle mise à jour de la table EXCHANGELIST)   -->
  <!-- FI 20131129 [19284] add FINALSETTLTSIDE (FINALSETTLTSIDE retourne tjs 'OfficialSettlement' sur les DC options en attendant une éventuelle -->
  <!--                      mise à jour de la table EXCHANGELIST)-->
  <!-- FL 20140220 [19648] add UNDERLYINGGROUP & UNDERLYINGASSET(Retourne tjs 'null' a ce niveau, la vrai mise à jour de ces données s'effectue  -->
  <!--  sur les Templates : ovrSQLGetDerivativeContractDefaultValue spécifiques à chaque importation -->
  <!-- FL/PLA 20170420 [23064] add column PHYSETTLTAMOUNT -->
  <xsl:template name="ovrSQLGetDerivativeContractDefaultValue">
    <xsl:param name="pResult"/>
    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>
    <SQL command="select" result="{$pResult}" cache="true">
      <![CDATA[
      select '0' as LEVELSORT, exl.CONTRACTMULTIPLIER, exl.IDC_PRICE,
      case when exl.SETTLTMETHOD is null then 'C' else exl.SETTLTMETHOD end as SETTLTMETHOD,
      case when (exl.SETTLTMETHOD is null or exl.SETTLTMETHOD = 'C') then 'NA' else 'None' end as PHYSETTLTAMOUNT,
      exl.PRICEQUOTEMETHOD, exl.FUTVALUATIONMETHOD, exl.PRICEDECLOCATOR, exl.STRIKEDECLOCATOR, exl.PRICEALIGNCODE,
      exl.STRIKEALIGNCODE, exl.CABINETOPTVALUE, exl.EXERCISESTYLE, null as IDMATURITYRULE, null as ASSETCATEGORY, null as IDASSET_UNL, null as IDDC_UNL, 100  as INSTRUMENTDEN,
      null as ISINCODE, 'STD' as CONTRACTTYPE, 
      case when @CATEGORYEXL='O' then 'OfficialSettlement' else null end as FINALSETTLTSIDE,
      null as UNDERLYINGGROUP,
      null as UNDERLYINGASSET
      from dbo.EXCHANGELIST exl
      inner join dbo.MARKET m on (m.EXCHANGESYMBOL=@EXCHANGESYMBOLEXL)
      where (exl.CONTRACTSYMBOL=@CONTRACTSYMBOLEXL) and (exl.EXCHANGEISO10383=m.ISO10383_ALPHA4)
      and (exl.CATEGORY=@CATEGORYEXL or exl.CATEGORY='X')
      ]]>
      <xsl:if test="
                $pResult = 'PRICEDECLOCATOR' or 
                $pResult = 'STRIKEDECLOCATOR' or 
                $pResult = 'PRICEALIGNCODE' or 
                $pResult = 'STRIKEALIGNCODE' or 
                $pResult = 'CABINETOPTVALUE'">
        <![CDATA[ and (exl.ORIGINEDATA='SPAN') ]]>
      </xsl:if>
      <xsl:if test="
                $pResult = 'EXERCISESTYLE'">
        <![CDATA[ and (@CATEGORYEXL='O') ]]>
      </xsl:if>
      <![CDATA[
      union all
      select '1' as LEVELSORT, null as CONTRACTMULTIPLIER, null as IDC_PRICE,
      'C' as SETTLTMETHOD,
      'NA' as PHYSETTLTAMOUNT,
      null as PRICEQUOTEMETHOD, null as FUTVALUATIONMETHOD, null as PRICEDECLOCATOR, null as STRIKEDECLOCATOR, null as PRICEALIGNCODE,
      null as STRIKEALIGNCODE, null as CABINETOPTVALUE, null as EXERCISESTYLE, null as IDMATURITYRULE, null as ASSETCATEGORY, null as IDASSET_UNL, null as IDDC_UNL, 100  as INSTRUMENTDEN,
      null as ISINCODE, 'STD' as CONTRACTTYPE,
      case when @CATEGORYEXL='O' then 'OfficialSettlement' else null end as FINALSETTLTSIDE,
      null as UNDERLYINGGROUP,
      null as UNDERLYINGASSET
      from DUAL
      order by LEVELSORT
      ]]>
      <xsl:call-template name="ParamNodesBuilder">
        <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
        <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
      </xsl:call-template>
    </SQL>
  </xsl:template>

  <!-- SqlBuilder, contient les scripts des cinq tables : MATURITYRULE,DERIVATIVECONTRACT,MATURITY,DERIVATIVEATTRIB et ASSET_ETD  -->
  <xsl:template name="SqlBuilder">
    <xsl:param name="pExchangeSymbol"/>
    <xsl:param name="pContractDisplayName"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pContractFactor"/>
    <xsl:param name="pContractMultiplier"/>
    <!--RD 20110629 [17435] Ne pas importer le ContractMultiplier, le cas échéant.-->
    <!--FI 20131205 [19275] Mise en commentaire du paramètre pContractMultiplierSpecified ce paramètre n'existe -->
    <!--<xsl:param name="pContractMultiplierSpecified" select="true()"/>-->
    <xsl:param name="pAssetMultiplier"/>
    <xsl:param name="pExerciseStyle"/>
    <xsl:param name="pUnderlyingGroup"/>
    <xsl:param name="pUnderlyingAsset"/>
    <xsl:param name="pUnderlyingContractSymbol"/>
    <xsl:param name="pUnderlyingAssetSymbol"/>
    <xsl:param name="pNominalValue"/>
    <xsl:param name="pSettlMethod"/>
    <xsl:param name="pCurrency"/>
    <xsl:param name="pMaturityMonthYear"/>
    <xsl:param name="pMaturityFormat" select="'0'"/>
    <xsl:param name="pMaturityDate"/>
    <xsl:param name="pDeliveryDate"/>
    <xsl:param name="pLastTradingDay"/>
    <xsl:param name="pAssetISINCode"/>
    <xsl:param name="pAssetDisplayName"/>
    <xsl:param name="pAssetSymbol"/>
    <xsl:param name="pAssetPutCall"/>
    <xsl:param name="pAssetStrikePrice"/>
    <xsl:param name="pAssetFirstQuotationDay"/>
    <xsl:param name="pAssetIdentifier"/>
    <xsl:param name="pInstrumentIdentifier"/>
    <xsl:param name="pFutValuationMethod"/>
    <xsl:param name="pAssignmentMethod"/>
    <!--<xsl:param name="pMaturityRuleIdentifier"/>-->
    <xsl:param name="pDerivativeContractIdentifier"/>
    <xsl:param name="pAIICode"/>
    <xsl:param name="pContractAttribute" select="$gNull"/>
    <xsl:param name="pStrikeDecLocator" select="$gNull"/>
    <!--RD&FL 20120110 Dans le cas de L'Eurex, mettre tous les DC avec ISAUTOSSETING à True.-->
    <xsl:param name="pDerivativeContractIsAutoSetting" select="$gFalse"/>

    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>

    <!--	*******************************************************************
						Tables MATURITYRULE and DERIVATIVECONTRACT
			*******************************************************************	-->
    <xsl:call-template name="SQLTableMATURITYRULE_DERIVATIVECONTRACT">
      <xsl:with-param name="pExchangeSymbol">
        <xsl:value-of select="$pExchangeSymbol"/>
      </xsl:with-param>
      <!--<xsl:with-param name="pMaturityRuleIdentifier">
        <xsl:value-of select="$pMaturityRuleIdentifier"/>
      </xsl:with-param>-->
      <xsl:with-param name="pContractSymbol" select="$pContractSymbol"/>
      <xsl:with-param name="pUnderlyingContractSymbol" select="$pUnderlyingContractSymbol"/>
      <xsl:with-param name="pDerivativeContractIdentifier" select="$pDerivativeContractIdentifier"/>
      <xsl:with-param name="pContractDisplayName" select="$pContractDisplayName"/>
      <xsl:with-param name="pInstrumentIdentifier" select="$pInstrumentIdentifier"/>
      <xsl:with-param name="pCurrency" select="$pCurrency"/>
      <xsl:with-param name="pCategory" select="$pCategory"/>
      <xsl:with-param name="pExerciseStyle" select="$pExerciseStyle"/>
      <xsl:with-param name="pSettlMethod" select="$pSettlMethod"/>
      <xsl:with-param name="pFutValuationMethod" select="$pFutValuationMethod"/>
      <xsl:with-param name="pContractFactor" select="$pContractFactor"/>
      <xsl:with-param name="pContractMultiplier" select="$pContractMultiplier"/>
      <!--RD 20110629 [17435] Ne pas importer le ContractMultiplier, le cas échéant.-->
      <!--FI 20131205 [19275] Mise en commentaire ce paramètre n'existe plus -->
      <!--<xsl:with-param name="pContractMultiplierSpecified" select="$pContractMultiplierSpecified"/>-->
      <xsl:with-param name="pNominalValue" select="$pNominalValue"/>
      <xsl:with-param name="pUnderlyingGroup" select="$pUnderlyingGroup"/>
      <xsl:with-param name="pUnderlyingAsset" select="$pUnderlyingAsset"/>
      <xsl:with-param name="pAssignmentMethod" select="$pAssignmentMethod"/>
      <xsl:with-param name="pContractAttribute" select="$pContractAttribute"/>
      <xsl:with-param name="pStrikeDecLocator" select="$pStrikeDecLocator"/>
      <xsl:with-param name="pMaturityFormat" select="$pMaturityFormat"/>
      <!--RD&FL 20120110 Dans le cas de L'Eurex, mettre tous les DC avec ISAUTOSSETING à True.-->
      <xsl:with-param name="pDerivativeContractIsAutoSetting" select="$pDerivativeContractIsAutoSetting"/>

      <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
      <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

    </xsl:call-template>
    <!--	*******************************************************************
						Tables MATURITY and DERIVATIVEATTRIB
			*******************************************************************	-->
    <xsl:call-template name="SQLTableMATURITY_DERIVATIVEATTRIB">
      <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
      <xsl:with-param name="pContractSymbol" select="$pContractSymbol"/>
      <xsl:with-param name="pMaturityMonthYear" select="$pMaturityMonthYear"/>
      <xsl:with-param name="pMaturityDate" select="$pMaturityDate"/>
      <xsl:with-param name="pDeliveryDate" select="$pDeliveryDate"/>
      <xsl:with-param name="pLastTradingDay" select="$pLastTradingDay"/>
      <xsl:with-param name="pUnderlyingAssetSymbol" select="$pUnderlyingAssetSymbol"/>
      <xsl:with-param name="pUnderlyingContractSymbol" select="$pUnderlyingContractSymbol"/>

      <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
      <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>
    </xsl:call-template>
    <!--	*******************************************************************
						Table ASSET_ETD
			*******************************************************************	-->
    <xsl:call-template name="SQLTableASSET_ETD" >
      <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
      <xsl:with-param name="pContractSymbol" select="$pContractSymbol"/>
      <xsl:with-param name="pMaturityMonthYear" select="$pMaturityMonthYear"/>
      <xsl:with-param name="pAssetIdentifier" select="$pAssetIdentifier"/>
      <xsl:with-param name="pAssetDisplayName" select="$pAssetDisplayName"/>
      <xsl:with-param name="pAssetSymbol" select="$pAssetSymbol"/>
      <xsl:with-param name="pAssetISINCode" select="$pAssetISINCode"/>
      <xsl:with-param name="pAIICode" select="$pAIICode"/>
      <xsl:with-param name="pAssetMultiplier" select="$pAssetMultiplier"/>
      <xsl:with-param name="pCategory" select="$pCategory"/>
      <xsl:with-param name="pAssetPutCall" select="$pAssetPutCall"/>
      <xsl:with-param name="pAssetStrikePrice" select="$pAssetStrikePrice"/>
      <xsl:with-param name="pAssetFirstQuotationDay" select="$pAssetFirstQuotationDay"/>
      <xsl:with-param name="pContractAttribute" select="$pContractAttribute"/>
      <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
      <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>
    </xsl:call-template>
  </xsl:template>

  <!-- SqlAssetActivationUpdate :  ASSET_ETD.ISSECURITYSTATUS update, EurexUpd file -->
  <xsl:template name="SqlAssetActivationUpdate">
    <!--<xsl:param name="pAssetIdentifier"/>-->
    <xsl:param name="pExchangeSymbol"/>
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pMaturityMonthYear"/>
    <xsl:param name="pAssetIdentifier"/>
    <xsl:param name="pAssetDisplayName"/>
    <xsl:param name="pAssetSymbol"/>
    <xsl:param name="pAssetISINCode"/>
    <xsl:param name="pAssetPutCall"/>
    <xsl:param name="pAssetStrikePrice"/>
    <xsl:param name="pAssetFirstQuotationDay"/>
    <xsl:param name="pContractAttribute" select="$gNull"/>
    <xsl:param name="pIsSecurityStatus"/>
    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>

    <xsl:variable name="vParamExchangeSymbol">
      <Param name="EXCHANGESYMBOL" datatype="string">
        <xsl:value-of select="$pExchangeSymbol"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamContractSymbol">
      <Param name="CONTRACTSYMBOL" datatype="string">
        <xsl:value-of select="$pContractSymbol"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamAssetIdentifier">
      <Param name="IDENTIFIER" datatype="string">
        <xsl:value-of select="$pAssetIdentifier"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamMaturityMonthYear">
      <Param name="MATURITYMONTHYEAR" datatype="string">
        <xsl:value-of select="$pMaturityMonthYear"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamAssetPutCall">
      <Param name="PUTCALL" datatype="string">
        <xsl:value-of select="$pAssetPutCall"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamAssetStrikePrice">
      <Param name="STRIKEPRICE" datatype="decimal">
        <xsl:value-of select="$pAssetStrikePrice"/>
      </Param>
    </xsl:variable>

    <xsl:variable name="vSQLIsAutoSettingAsset">
      <xsl:call-template name="SQLDerivativeContractAfter">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'ISAUTOSETTINGASSET'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLAssetIdentifier">
      <xsl:call-template name="SQLAssetIdentifierExistsAndRowChanged">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'IDENTIFIER'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamMaturityMonthYear">
          <xsl:copy-of select="$vParamMaturityMonthYear"/>
        </xsl:with-param>
        <xsl:with-param name="pParamStrikePrice">
          <xsl:copy-of select="$vParamAssetStrikePrice"/>
        </xsl:with-param>
        <xsl:with-param name="pParamPutCall">
          <xsl:copy-of select="$vParamAssetPutCall"/>
        </xsl:with-param>
        <xsl:with-param name="pContractAttribute">
          <xsl:copy-of select="$pContractAttribute"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetDisplayName">
          <xsl:copy-of select="$pAssetDisplayName"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetISINCode">
          <xsl:copy-of select="$pAssetISINCode"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetSymbol">
          <xsl:copy-of select="$pAssetSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetFirstQuotationDay">
          <xsl:copy-of select="$pAssetFirstQuotationDay"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLIdDerivativeAttrib">
      <xsl:call-template name="SQLIdDerivativeAttrib">
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamMaturityMonthYear">
          <xsl:copy-of select="$vParamMaturityMonthYear"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>

    <!--	*******************************************************************
						Table ASSET_ETD
			*******************************************************************	-->
    <table name="ASSET_ETD" action="U" sequenceno="7">
      <!--DERIVATIVECONTRACT Exists and ISAUTOSETTINGASSET-->
      <column name="ISAUTOSETTINGASSET" datakey="false" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLIsAutoSettingAsset"/>
        <controls>
          <control action="RejectRow" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            true
          </control>
        </controls>
      </column>
      <column name="IDENTIFIER" datakey="true" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLAssetIdentifier"/>
        <controls>
          <control action="RejectRow" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            true
          </control>
        </controls>
      </column>
      <!-- RD 20100402 / IDDERIVATIVEATTRIB is DataKey-->
      <column name="IDDERIVATIVEATTRIB" datakey="true" datakeyupd="false" datatype="int">
        <xsl:copy-of select="$vSQLIdDerivativeAttrib"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Incorrect Derivative Attribut.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The Derivative Attribut is not found for Maturity &lt;b&gt;<xsl:value-of select="$pMaturityMonthYear"/>&lt;/b&gt; of Contract &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt; on Market &lt;b&gt;<xsl:value-of select="$pExchangeSymbol"/>&lt;/b&gt;.
                &lt;b&gt;Action:&lt;/b&gt; Create the corresponding Derivative Attribut and restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>
      <column name="ISSECURITYSTATUS" datakey="false" datakeyupd="true" datatype="int">
        <xsl:value-of select="$pIsSecurityStatus"/>
      </column>
      <xsl:call-template name="SysUpd">
        <xsl:with-param name="pIsWithControl">
          <xsl:copy-of select="$gFalse"/>
        </xsl:with-param>
      </xsl:call-template>
    </table>
  </xsl:template>

  <!-- SQLTableMATURITYRULE_DERIVATIVECONTRACT -->
  <xsl:template name="SQLTableMATURITYRULE_DERIVATIVECONTRACT">
    <xsl:param name="pExchangeSymbol"/>
    <!--<xsl:param name="pMaturityRuleIdentifier"/>-->
    <xsl:param name="pSQLMaturityRuleID" select="$gNull"/>
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pUnderlyingContractSymbol"/>
    <xsl:param name="pDerivativeContractIdentifier"/>
    <xsl:param name="pContractDisplayName"/>
    <xsl:param name="pInstrumentIdentifier"/>
    <xsl:param name="pCurrency"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pExerciseStyle"/>
    <xsl:param name="pSettlMethod"/>
    <xsl:param name="pFutValuationMethod"/>
    <xsl:param name="pContractFactor"/>
    <xsl:param name="pContractMultiplier"/>
    <!--RD 20110629 [17435] Ne pas importer le ContractMultiplier, le cas échéant.-->
    <!--FI 20131205 [19275] Mise en commentaire, ce paramètre n'existe-->
    <!--<xsl:param name="pContractMultiplierSpecified" select="true()"/>-->
    <xsl:param name="pNominalValue"/>
    <xsl:param name="pUnderlyingGroup"/>
    <xsl:param name="pUnderlyingAsset"/>
    <xsl:param name="pAssignmentMethod"/>
    <xsl:param name="pContractAttribute" select="$gNull"/>
    <xsl:param name="pStrikeDecLocator" select="$gNull"/>
    <xsl:param name="pMinPriceIncr" select="$gNull"/>
    <xsl:param name="pMinPriceIncrAmount" select="$gNull"/>
    <xsl:param name="pMaturityFormat" select="'0'"/>
    <!--GS&GP 20111025 IDEM Alimentation des champs DERIVATIVECONTRACT.ASSETCATEGORY et DERIVATIVECONTRACT.IDASSET_UNL-->
    <xsl:param name="pAssetCategory" select="$gNull"/>
    <xsl:param name="pUnderlyingIdentifier"/>
    <xsl:param name="pUnderlyingIsinCode"/>
    <!--RD&FL 20120110 EUREX Mettre tous les DC avec ISAUTOSSETING à True.-->
    <xsl:param name="pDerivativeContractIsAutoSetting" select="$gFalse"/>
    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>
    <!--FL 20170725-->
    <xsl:param name="pPhysettltamount"/>

    <!-- 1/ Insert Contract without MaturityRule-->
    <xsl:call-template name="SQLTableDERIVATIVECONTRACT">
      <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
      <!--<xsl:with-param name="pMaturityRuleIdentifier" select="$pMaturityRuleIdentifier"/>-->
      <xsl:with-param name="pSQLMaturityRuleID">
        <xsl:copy-of select="$pSQLMaturityRuleID"/>
      </xsl:with-param>
      <xsl:with-param name="pContractSymbol" select="$pContractSymbol"/>
      <xsl:with-param name="pUnderlyingContractSymbol" select="$pUnderlyingContractSymbol"/>
      <xsl:with-param name="pDerivativeContractIdentifier" select="$pDerivativeContractIdentifier"/>
      <xsl:with-param name="pContractDisplayName" select="$pContractDisplayName"/>
      <xsl:with-param name="pInstrumentIdentifier" select="$pInstrumentIdentifier"/>
      <xsl:with-param name="pCurrency" select="$pCurrency"/>
      <xsl:with-param name="pCategory" select="$pCategory"/>
      <xsl:with-param name="pExerciseStyle" select="$pExerciseStyle"/>
      <xsl:with-param name="pSettlMethod" select="$pSettlMethod"/>
      <xsl:with-param name="pFutValuationMethod" select="$pFutValuationMethod"/>
      <xsl:with-param name="pContractFactor" select="$pContractFactor"/>
      <xsl:with-param name="pContractMultiplier" select="$pContractMultiplier"/>
      <!--RD 20110629 [17435] Ne pas importer le ContractMultiplier, le cas échéant.-->
      <!--FI 20131205 [19275] Mise en commentaire, ce paramètre n'existe pas-->
      <!--<xsl:with-param name="pContractMultiplierSpecified" select="$pContractMultiplierSpecified"/>-->
      <xsl:with-param name="pNominalValue" select="$pNominalValue"/>
      <xsl:with-param name="pUnderlyingGroup" select="$pUnderlyingGroup"/>
      <xsl:with-param name="pUnderlyingAsset" select="$pUnderlyingAsset"/>
      <xsl:with-param name="pAssignmentMethod" select="normalize-space($pAssignmentMethod)"/>
      <xsl:with-param name="pContractAttribute" select="$pContractAttribute"/>
      <xsl:with-param name="pStrikeDecLocator" select="$pStrikeDecLocator"/>
      <xsl:with-param name="pMinPriceIncr" select="$pMinPriceIncr"/>
      <xsl:with-param name="pMinPriceIncrAmount" select="$pMinPriceIncrAmount"/>
      <xsl:with-param name="pInsertMaturityRule" select="$gFalse"/>
      <!--GS&GP 20111025 IDEM Alimentation des champs DERIVATIVECONTRACT.ASSETCATEGORY et DERIVATIVECONTRACT.IDASSET_UNL-->
      <xsl:with-param name="pAssetCategory"        select="$pAssetCategory"/>
      <xsl:with-param name="pUnderlyingIdentifier" select="$pUnderlyingIdentifier"/>
      <xsl:with-param name="pUnderlyingIsinCode" select="$pUnderlyingIsinCode"/>
      <!--RD&FL 20120110 EUREX Mettre tous les DC avec ISAUTOSSETING à True.-->
      <xsl:with-param name="pDerivativeContractIsAutoSetting" select="$pDerivativeContractIsAutoSetting"/>

      <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>
      <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>

      <!--FL 20171006 [23495]-->
      <xsl:with-param name="pPhysettltamount" select="$pPhysettltamount"/>

    </xsl:call-template>

    <!-- 2/ Insert MaturityRule-->
    <xsl:call-template name="SQLTableMATURITYRULE">
      <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
      <!--<xsl:with-param name="pMaturityRuleIdentifier" select="$pMaturityRuleIdentifier"/>-->
      <xsl:with-param name="pContractSymbol" select="$pContractSymbol"/>
      <xsl:with-param name="pMaturityFormat" select="$pMaturityFormat"/>

      <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>
      <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
    </xsl:call-template>

    <!-- 3/ Update Contract with MaturityRule-->
    <xsl:call-template name="SQLTableDERIVATIVECONTRACT_UpdateIDMATURITYRULE">
      <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
      <!--<xsl:with-param name="pMaturityRuleIdentifier" select="$pMaturityRuleIdentifier"/>-->
      <xsl:with-param name="pContractSymbol" select="$pContractSymbol"/>
      <xsl:with-param name="pContractAttribute" select="$pContractAttribute"/>

      <xsl:with-param name="pExtSQLFilterValues" select="
                    $pExtSQLFilterValues"/>
      <xsl:with-param name="pExtSQLFilterNames" select="
                    $pExtSQLFilterNames"/>
    </xsl:call-template>
  </xsl:template>

  <!-- SQLTableMATURITY_DERIVATIVEATTRIB2 -->
  <!-- FI Template destiné à remplacer SQLTableMATURITY_DERIVATIVEATTRIB -->
  <!-- Prend en considération de MarketISO10383 -->
  <xsl:template name="SQLTableMATURITY_DERIVATIVEATTRIB2">
    <xsl:param name="pISO10383"/>
    <xsl:param name="pExchangeSymbol"/>
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pMaturityMonthYear"/>
    <xsl:param name="pMaturityDate"/>
    <xsl:param name="pDeliveryDate"/>
    <xsl:param name="pLastTradingDay"/>
    <xsl:param name="pUnderlyingAssetSymbol"/>
    <xsl:param name="pUnderlyingContractSymbol"/>

    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>

    <!-- Param variables-->
    <xsl:variable name="vParamISO10383">
      <Param name="ISO10383_ALPHA4" datatype="string">
        <xsl:value-of select="$pISO10383"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamExchangeSymbol">
      <Param name="EXCHANGESYMBOL" datatype="string">
        <xsl:value-of select="$pExchangeSymbol"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamContractSymbol">
      <Param name="CONTRACTSYMBOL" datatype="string">
        <xsl:value-of select="$pContractSymbol"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamMaturityMonthYear">
      <Param name="MATURITYMONTHYEAR" datatype="string">
        <xsl:value-of select="$pMaturityMonthYear"/>
      </Param>
    </xsl:variable>

    <!-- SQL variables-->
    <!-- RD 20100603 / Load IdMaturityRule from DERIVATIVECONTRACT table-->
    <xsl:variable name="vSQLIdMaturityRuleFromDerivativeContract">
      <xsl:call-template name="SQLDerivativeContractAfter2">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'IDMATURITYRULE'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamISO10383">
          <xsl:copy-of select="$vParamISO10383"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vSQLIdMaturity">
      <xsl:call-template name="SQLIdMaturity2">
        <xsl:with-param name="pParamISO10383">
          <xsl:copy-of select="$vParamISO10383"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamMaturityMonthYear">
          <xsl:copy-of select="$vParamMaturityMonthYear"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <!-- RD 20100603 / Load IDDC from DERIVATIVECONTRACT table-->
    <xsl:variable name="vSQLIdDerivativeContract">
      <xsl:call-template name="SQLDerivativeContractAfter2">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'IDDC'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamISO10383">
          <xsl:copy-of select="$vParamISO10383"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLIsAutoSettingAsset">
      <xsl:call-template name="SQLDerivativeContractAfter2">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'ISAUTOSETTINGASSET'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamISO10383">
          <xsl:copy-of select="$vParamISO10383"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <!--	*******************************************************************
						Table MATURITY
			*******************************************************************	-->
    <table name="MATURITY" action="IU" sequenceno="5">
      <!--DERIVATIVECONTRACT Exists and ISAUTOSETTINGASSET-->
      <column name="ISAUTOSETTINGASSET" datakey="false" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLIsAutoSettingAsset"/>
        <controls>
          <control action="RejectRow" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            true
          </control>
        </controls>
      </column>
      <column name="IDMATURITYRULE" datakey="true" datakeyupd="false" datatype="int">
        <xsl:copy-of select="$vSQLIdMaturityRuleFromDerivativeContract"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;No Maturity Rule.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; No Maturity Rule affected for Contract &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt; on Market &lt;b&gt;<xsl:value-of select="$pExchangeSymbol"/>&lt;/b&gt;.
                &lt;b&gt;Action:&lt;/b&gt; Create the corresponding Maturity Rule, affecte it to the Contract and restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>
      <column name="MATURITYMONTHYEAR" datakey="true" datakeyupd="false" datatype="string">
        <xsl:value-of select="$pMaturityMonthYear"/>
      </column>
      <column name="MATURITYDATE" datakey="false" datakeyupd="true" datatype="date" dataformat="yyyyMMdd">
        <xsl:value-of select="$pMaturityDate"/>
      </column>
      <!--column name="MATURITYTIME" datakey="false" datakeyupd="false" datatype="string">
        null
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="IsUpdate()"/>
          </control>
        </controls>
      </column-->
      <column name="DELIVERYDATE" datakey="false" datakeyupd="true" datatype="date" dataformat="yyyyMMdd">
        <xsl:value-of select="$pDeliveryDate"/>
      </column>
      <column name="LASTTRADINGDAY" datakey="false" datakeyupd="true" datatype="date" dataformat="yyyyMMdd">
        <xsl:value-of select="$pLastTradingDay"/>
      </column>
      <xsl:call-template name="SysInsUpd"/>
    </table>

    <!--	*******************************************************************
						Table DERIVATIVEATTRIB
		    *******************************************************************	-->
    <table name="DERIVATIVEATTRIB" action="IU" sequenceno="6">
      <!--DERIVATIVECONTRACT Exists and ISAUTOSETTINGASSET-->
      <column name="ISAUTOSETTINGASSET" datakey="false" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLIsAutoSettingAsset"/>
        <controls>
          <control action="RejectRow" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            true
          </control>
        </controls>
      </column>
      <column name="IDMATURITY" datakey="true" datakeyupd="false" datatype="int">
        <xsl:copy-of select="$vSQLIdMaturity"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Incorrect Maturity.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The Maturity &lt;b&gt;<xsl:value-of select="$pMaturityMonthYear"/>&lt;/b&gt; is not found for Contract &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt; on Market &lt;b&gt;<xsl:value-of select="$pExchangeSymbol"/>&lt;/b&gt;.
                &lt;b&gt;Action:&lt;/b&gt; Create the Maturity &lt;b&gt;<xsl:value-of select="$pMaturityMonthYear"/>&lt;/b&gt; or correct the input file, and restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>
      <column name="IDDC" datakey="true" datakeyupd="false" datatype="int">
        <xsl:copy-of select="$vSQLIdDerivativeContract"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Incorrect Derivative Contract.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The Derivative Contract is not found for Contract &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt; on Market &lt;b&gt;<xsl:value-of select="$pExchangeSymbol"/>&lt;/b&gt;.
                &lt;b&gt;Action:&lt;/b&gt; Create the Derivative Contract &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt; or correct the input file, and restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>

      <xsl:if test="$pUnderlyingContractSymbol != '' and $pUnderlyingAssetSymbol != ''">

        <xsl:variable name="vParamUnderlyingContractSymbol">
          <Param name="UNDERLYINGCONTRACTSYMBOL" datatype="string">
            <xsl:value-of select="$pUnderlyingContractSymbol"/>
          </Param>
        </xsl:variable>
        <xsl:variable name="vParamUnderlyingAssetSymbol">
          <Param name="UNDERLYINGASSETSYMBOL" datatype="string">
            <xsl:value-of select="$pUnderlyingAssetSymbol"/>
          </Param>
        </xsl:variable>

        <xsl:variable name="vSQLIdAssetUnderlying">
          <xsl:call-template name="SQLIdAssetUnderlying2">
            <xsl:with-param name="pParamISO10383">
              <xsl:copy-of select="$vParamISO10383"/>
            </xsl:with-param>
            <xsl:with-param name="pParamExchangeSymbol">
              <xsl:copy-of select="$vParamExchangeSymbol"/>
            </xsl:with-param>
            <xsl:with-param name="pParamUnderlyingContractSymbol">
              <xsl:copy-of select="$vParamUnderlyingContractSymbol"/>
            </xsl:with-param>
            <xsl:with-param name="pParamUnderlyingAssetSymbol">
              <xsl:copy-of select="$vParamUnderlyingAssetSymbol"/>
            </xsl:with-param>
          </xsl:call-template>
        </xsl:variable>

        <column name="IDASSET" datakey="false" datakeyupd="true" datatype="int">
          <xsl:copy-of select="$vSQLIdAssetUnderlying"/>
          <controls>
            <control action="RejectColumn" return="true">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
        </column>

      </xsl:if>

      <xsl:call-template name="SysInsUpd"/>
    </table>

  </xsl:template>
  <!-- SQLTableMATURITY_DERIVATIVEATTRIB -->
  <xsl:template name="SQLTableMATURITY_DERIVATIVEATTRIB">
    <xsl:param name="pExchangeSymbol"/>
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pMaturityMonthYear"/>
    <xsl:param name="pMaturityDate"/>
    <xsl:param name="pDeliveryDate"/>
    <xsl:param name="pLastTradingDay"/>
    <xsl:param name="pUnderlyingAssetSymbol"/>
    <xsl:param name="pUnderlyingContractSymbol"/>

    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>

    <!-- Param variables-->
    <xsl:variable name="vParamExchangeSymbol">
      <Param name="EXCHANGESYMBOL" datatype="string">
        <xsl:value-of select="$pExchangeSymbol"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamContractSymbol">
      <Param name="CONTRACTSYMBOL" datatype="string">
        <xsl:value-of select="$pContractSymbol"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamMaturityMonthYear">
      <Param name="MATURITYMONTHYEAR" datatype="string">
        <xsl:value-of select="$pMaturityMonthYear"/>
      </Param>
    </xsl:variable>

    <!-- SQL variables-->
    <!-- RD 20100603 / Load IdMaturityRule from DERIVATIVECONTRACT table-->
    <xsl:variable name="vSQLIdMaturityRuleFromDerivativeContract">
      <xsl:call-template name="SQLDerivativeContractAfter">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'IDMATURITYRULE'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLIdMaturity">
      <xsl:call-template name="SQLIdMaturity">
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamMaturityMonthYear">
          <xsl:copy-of select="$vParamMaturityMonthYear"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <!-- RD 20100603 / Load IDDC from DERIVATIVECONTRACT table-->
    <xsl:variable name="vSQLIdDerivativeContract">
      <xsl:call-template name="SQLDerivativeContractAfter">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'IDDC'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLIsAutoSettingAsset">
      <xsl:call-template name="SQLDerivativeContractAfter">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'ISAUTOSETTINGASSET'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <!--	*******************************************************************
						Table MATURITY
			*******************************************************************	-->
    <table name="MATURITY" action="IU" sequenceno="5">
      <!--DERIVATIVECONTRACT Exists and ISAUTOSETTINGASSET-->
      <column name="ISAUTOSETTINGASSET" datakey="false" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLIsAutoSettingAsset"/>
        <controls>
          <control action="RejectRow" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            true
          </control>
        </controls>
      </column>
      <column name="IDMATURITYRULE" datakey="true" datakeyupd="false" datatype="int">
        <xsl:copy-of select="$vSQLIdMaturityRuleFromDerivativeContract"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;No Maturity Rule.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; No Maturity Rule affected for Contract &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt; on Market &lt;b&gt;<xsl:value-of select="$pExchangeSymbol"/>&lt;/b&gt;.
                &lt;b&gt;Action:&lt;/b&gt; Create the corresponding Maturity Rule, affecte it to the Contract and restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>
      <column name="MATURITYMONTHYEAR" datakey="true" datakeyupd="false" datatype="string">
        <xsl:value-of select="$pMaturityMonthYear"/>
      </column>
      <column name="MATURITYDATE" datakey="false" datakeyupd="true" datatype="date" dataformat="yyyyMMdd">
        <xsl:value-of select="$pMaturityDate"/>
      </column>
      <!--column name="MATURITYTIME" datakey="false" datakeyupd="false" datatype="string">
        null
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="IsUpdate()"/>
          </control>
        </controls>
      </column-->
      <column name="DELIVERYDATE" datakey="false" datakeyupd="true" datatype="date" dataformat="yyyyMMdd">
        <xsl:value-of select="$pDeliveryDate"/>
      </column>
      <column name="LASTTRADINGDAY" datakey="false" datakeyupd="true" datatype="date" dataformat="yyyyMMdd">
        <xsl:value-of select="$pLastTradingDay"/>
      </column>
      <!--column name="FIRSTNOTICEDAY" datakey="false" datakeyupd="false" datatype="date">
        null
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="IsUpdate()"/>
          </control>
        </controls>
      </column-->
      <xsl:call-template name="SysInsUpd"/>
    </table>

    <!--	*******************************************************************
						Table DERIVATIVEATTRIB
		    *******************************************************************	-->
    <table name="DERIVATIVEATTRIB" action="IU" sequenceno="6">
      <!--DERIVATIVECONTRACT Exists and ISAUTOSETTINGASSET-->
      <column name="ISAUTOSETTINGASSET" datakey="false" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLIsAutoSettingAsset"/>
        <controls>
          <control action="RejectRow" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            true
          </control>
        </controls>
      </column>
      <column name="IDMATURITY" datakey="true" datakeyupd="false" datatype="int">
        <xsl:copy-of select="$vSQLIdMaturity"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Incorrect Maturity.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The Maturity &lt;b&gt;<xsl:value-of select="$pMaturityMonthYear"/>&lt;/b&gt; is not found for Contract &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt; on Market &lt;b&gt;<xsl:value-of select="$pExchangeSymbol"/>&lt;/b&gt;.
                &lt;b&gt;Action:&lt;/b&gt; Create the Maturity &lt;b&gt;<xsl:value-of select="$pMaturityMonthYear"/>&lt;/b&gt; or correct the input file, and restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>
      <column name="IDDC" datakey="true" datakeyupd="false" datatype="int">
        <xsl:copy-of select="$vSQLIdDerivativeContract"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Incorrect Derivative Contract.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The Derivative Contract is not found for Contract &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt; on Market &lt;b&gt;<xsl:value-of select="$pExchangeSymbol"/>&lt;/b&gt;.
                &lt;b&gt;Action:&lt;/b&gt; Create the Derivative Contract &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt; or correct the input file, and restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>

      <xsl:if test="$pUnderlyingContractSymbol != '' and $pUnderlyingAssetSymbol != ''">

        <xsl:variable name="vParamUnderlyingContractSymbol">
          <Param name="UNDERLYINGCONTRACTSYMBOL" datatype="string">
            <xsl:value-of select="$pUnderlyingContractSymbol"/>
          </Param>
        </xsl:variable>
        <xsl:variable name="vParamUnderlyingAssetSymbol">
          <Param name="UNDERLYINGASSETSYMBOL" datatype="string">
            <xsl:value-of select="$pUnderlyingAssetSymbol"/>
          </Param>
        </xsl:variable>

        <xsl:variable name="vSQLIdAssetUnderlying">
          <xsl:call-template name="SQLIdAssetUnderlying">
            <xsl:with-param name="pParamExchangeSymbol">
              <xsl:copy-of select="$vParamExchangeSymbol"/>
            </xsl:with-param>
            <xsl:with-param name="pParamUnderlyingContractSymbol">
              <xsl:copy-of select="$vParamUnderlyingContractSymbol"/>
            </xsl:with-param>
            <xsl:with-param name="pParamUnderlyingAssetSymbol">
              <xsl:copy-of select="$vParamUnderlyingAssetSymbol"/>
            </xsl:with-param>
          </xsl:call-template>
        </xsl:variable>

        <column name="IDASSET" datakey="false" datakeyupd="true" datatype="int">
          <xsl:copy-of select="$vSQLIdAssetUnderlying"/>
          <controls>
            <control action="RejectColumn" return="true">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
        </column>

      </xsl:if>

      <xsl:call-template name="SysInsUpd"/>
    </table>

  </xsl:template>

  <!-- SQLTableMATURITYRULE -->
  <xsl:template name="SQLTableMATURITYRULE">
    <xsl:param name="pExchangeSymbol"/>
    <!--<xsl:param name="pMaturityRuleIdentifier"/>-->
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pMaturityFormat" select="'0'"/>

    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>

    <xsl:param name="pCheckContratExist" select="true()"/>

    <!-- Param variables-->
    <xsl:variable name="vParamExchangeSymbol">
      <Param name="EXCHANGESYMBOL" datatype="string">
        <xsl:value-of select="$pExchangeSymbol"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamContractSymbol">
      <Param name="CONTRACTSYMBOL" datatype="string">
        <xsl:value-of select="$pContractSymbol"/>
      </Param>
    </xsl:variable>
    <!--<xsl:variable name="vParamMaturityRuleIdentifier">
      <Param name="MATURITYRULEIDENTIFIER" datatype="string">
        <xsl:value-of select="$pMaturityRuleIdentifier"/>
      </Param>
    </xsl:variable>-->

    <!-- SQL variables-->

    <xsl:variable name="vSQLIDMaturityRuleFromDerivativeContract">
      <xsl:call-template name="SQLDerivativeContractAfter">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'IDMATURITYRULE'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLIdMaturityRule">
      <xsl:call-template name="SQLIdMaturityRuleBefor">
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLIDENTIFIERMaturityRule">
      <xsl:call-template name="SQLDerivativeContractAfter">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'IDENTIFIERMTR'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <!--<xsl:variable name="vSQLIsAutoSettingAsset">
      <xsl:call-template name="SQLDerivativeContractAfter">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'ISAUTOSETTINGASSET'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>-->
    <xsl:variable name="vSQLDerivativeContractExists">
      <xsl:call-template name="SQLDerivativeContractAfter">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'IDENTIFIER'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>

    <!--	*******************************************************************
						Table MATURITYRULE
			    *******************************************************************	-->
    <table name="MATURITYRULE" action="I" sequenceno="3">
      <!-- DERIVATIVECONTRACT Exists and ISAUTOSETTINGASSET-->
      <!--<xsl:if test="$pCheckContratExist = true()">-->
      <!--<column name="ISAUTOSETTINGASSET" datakey="false" datakeyupd="false" datatype="string">
          <xsl:copy-of select="$vSQLIsAutoSettingAsset"/>
          <controls>
            <control action="RejectRow" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
            <control action="RejectColumn" return="true" logtype="None">
              true
            </control>
          </controls>
        </column>-->
      <column name="DCONTRACTEXISTS" datakey="false" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLDerivativeContractExists"/>
        <controls>
          <control action="RejectRow" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            true
          </control>
        </controls>
      </column>
      <!--</xsl:if>-->
      <!-- DERIVATIVECONTRACT.IDMATURITYRULE is NULL-->
      <column name="CHECKIDMATURITYRULEINDERIVATIVECONTRACT" datakey="false" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLIDMaturityRuleFromDerivativeContract"/>
        <controls>
          <control action="RejectRow" return="false" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            true
          </control>
        </controls>
      </column>
      <!-- MATURITYRULE.IDMATURITYRULE is NULL-->
      <column name="CHECKIDMATURITYRULEINMATURITYRULE" datakey="false" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLIdMaturityRule"/>
        <controls>
          <control action="RejectRow" return="false">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Maturity Rule already exists.&lt;/b&gt;
                <!--&lt;b&gt;Cause:&lt;/b&gt; The Maturity Rule &lt;b&gt;<xsl:value-of select="$pMaturityRuleIdentifier"/>&lt;/b&gt; is already exist.-->
                &lt;b&gt;Cause:&lt;/b&gt; The Maturity Rule for Contract &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt; on Market &lt;b&gt;<xsl:value-of select="$pExchangeSymbol"/>&lt;/b&gt; is already exist.
                &lt;b&gt;Action:&lt;/b&gt; Delete or rename the existing Maturity Rule <!--&lt;b&gt;<xsl:value-of select="$pMaturityRuleIdentifier"/>&lt;/b&gt;--> and restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            true
          </control>
        </controls>
      </column>
      <column name="IDENTIFIER" datakey="true" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLIDENTIFIERMaturityRule"/>
        <!--<controls>
          <control action="RejectRow" return="false">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Maturity Rule already exists.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The Maturity Rule &lt;b&gt;<xsl:value-of select="$pMaturityRuleIdentifier"/>&lt;/b&gt; is already exist.
                &lt;b&gt;Action:&lt;/b&gt; Delete or rename the existing Maturity Rule &lt;b&gt;<xsl:value-of select="$pMaturityRuleIdentifier"/>&lt;/b&gt; and restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
          <control action="ApplyDefault" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>

          <xsl:call-template name="SelectOrFilterColumn">
            <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
            <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
            <xsl:with-param name="pDefaultValue" select="$pMaturityRuleIdentifier"/>
          </xsl:call-template>

          <xsl:value-of select="$pMaturityRuleIdentifier"/>
        </default>-->
      </column>
      <column name="DISPLAYNAME" datakey="false" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLIDENTIFIERMaturityRule"/>
        <!--<xsl:call-template name="SelectOrFilterColumn">
          <xsl:with-param name="pNames" select="concat($pExtSQLFilterNames,',','BEAUTIFYRESULT')"/>
          <xsl:with-param name="pValues" select="concat($pExtSQLFilterValues,',',' Maturities')"/>
          <xsl:with-param name="pDefaultValue" select="concat($pMaturityRuleIdentifier,' Maturities')"/>
          <xsl:with-param name="pResultField" select="'DISPLAYNAME'"/>
        </xsl:call-template>

        <xsl:value-of select="concat($pMaturityRuleIdentifier,' Maturities')"/>-->
      </column>
      <column name="MMYFMT" datakey="false" datakeyupd="false" datatype="int">
        <xsl:value-of select="$pMaturityFormat"/>
      </column>

      <!-- FI 20120116 Rajout de la valeur N/A dans une nouvelle colonne non null de la table MATURITYRULE   -->
      <column name="SOURCE" datakey="false" datakeyupd="false" datatype="string">N/A</column>

      <xsl:call-template name="SysIns">
        <xsl:with-param name="pIsWithControl">
          <xsl:copy-of select="$gFalse"/>
        </xsl:with-param>
      </xsl:call-template>
    </table>

  </xsl:template>

  <!-- =================================== -->
  <!--       Template IDASSET_UNL          -->
  <!-- =================================== -->
  <xsl:template name="GetIdassetUnl">
    <xsl:param name="pAssetCategory"/>
    <xsl:param name="pUnderlyingIdentifier"/>
    <xsl:param name="pUnderlyingIsinCode"/>
    <xsl:param name="pUnderlyingISO10383"/>
    <xsl:param name="pResult"/>
    <xsl:choose>
      <xsl:when test="$pAssetCategory = 'EquityAsset'">
        <!-- PL 20130215 Add cache="true" -->
        <!-- BD 20130513 Ajout de la condition sur ISINCODE -->
        <!-- EG 20130717 Paramètres SQL en MAJUSCULE -->
        <SQL command="select" cache="true">
          <xsl:attribute name="result" >
            <xsl:value-of select="$pResult"/>
          </xsl:attribute>
          select asset.IDASSET, asset.IDENTIFIER
          from dbo.ASSET_EQUITY asset
          inner join dbo.ASSET_EQUITY_RDCMK ardcm on (ardcm.IDASSET=asset.IDASSET) and (ardcm.SYMBOL=@UNDERLYINGIDENTIFIER)
          and (asset.ISINCODE=@UNDERLYINGISINCODE)
          <Param name="UNDERLYINGIDENTIFIER" datatype="string">
            <xsl:value-of select="$pUnderlyingIdentifier"/>
          </Param>
          <Param name="UNDERLYINGISINCODE" datatype="string">
            <xsl:value-of select="$pUnderlyingIsinCode"/>
          </Param>
        </SQL>
      </xsl:when>
      <xsl:when test="$pAssetCategory = 'Index'">
        <!-- PL 20130215 Add cache="true" -->
        <SQL command="select" cache="true">
          <xsl:attribute name="result">
            <xsl:value-of select="$pResult"/>
          </xsl:attribute>
          select asset.IDASSET, asset.IDENTIFIER
          from dbo.ASSET_INDEX asset
          where (asset.ISINCODE=@PSQLUNDERLYINGISINCODE)
          <Param name="PSQLUNDERLYINGISINCODE" datatype="string">
            <xsl:value-of select="$pUnderlyingIsinCode"/>
          </Param>
        </SQL>
      </xsl:when>
      <xsl:when test="$pAssetCategory = 'Commodity'">
        <!-- PL 20130218 Newness Commodity TBD -->
        <SQL command="select" cache="true">
          <xsl:attribute name="result">
            <xsl:value-of select="$pResult"/>
          </xsl:attribute>
          select asset.IDASSET, asset.IDENTIFIER
          from dbo.ASSET_COMMODITY asset
          inner join dbo.MARKET m on m.IDM=asset.IDM and (m.ISO10383_ALPHA4=@ISO10383_ALPHA4)
          where (asset.IDENTIFIER=@IDENTIFIER)
          <Param name="IDENTIFIER" datatype="string">
            <xsl:value-of select="$pUnderlyingIdentifier"/>
          </Param>
          <Param name="ISO10383_ALPHA4" datatype="string">
            <xsl:value-of select="$pUnderlyingISO10383"/>
          </Param>
        </SQL>
      </xsl:when>
      <xsl:otherwise/>
    </xsl:choose>
  </xsl:template>

  <!-- SQLTableDERIVATIVECONTRACT -->
  <!-- RD 20110629 [17435] add parameter pContractMultiplierSpecified-->
  <!-- PL 20130212 Add pISO10383 -->
  <!-- FI 20131120 [19216] add column CONTRACTTYPE (Mode Insert only) -->
  <!-- FI 20131129 [19284] add column FINALSETTLTSIDE (Mode Insert only) -->
  <!-- FI 20131205 [19275] del parameter pContractMultiplierSpecified (use pInsContractMultiplier and pUpdContractMultiplier instead of) -->
  <!-- FI 20131205 [19275] add parameters pInsContractMultiplier, pUpdContractMultiplier, pInsContractFactor, pUpdContractFactor -->
  <!-- PM 20140512 [19970][19259] Ajout pNominalCurrency -->
  <!-- RD 20160219 [21937] Ne pas updater ASSETCATEGORY et NOMINALVALUE avec null -->
  <!-- FL 20160701 [22084] add column EXERCISERULE -->
  <!-- FL/PLA 20170420 [23064] add column PHYSETTLTAMOUNT -->
  <!-- FL 20201217 [25606] Fixed a Bug generating Maturiy rules to Null instead of the Default Rule -->
  <xsl:template name="SQLTableDERIVATIVECONTRACT">
    <xsl:param name="pISO10383"/>
    <xsl:param name="pExchangeSymbol"/>
    <!-- FL/RD 20140326 [19648] Add new optional parameter pParamExchangeSymbol -->
    <xsl:param name="pParamExchangeSymbol"/>
    <!-- FL/PL 20130124 IDEM: Remise en vigueur du paramètre "pMaturityRuleIdentifier", avec une default value à Null -->
    <xsl:param name="pMaturityRuleIdentifier" select="$gNull"/>
    <xsl:param name="pMaturityRuleID" select="0"/>
    <!-- FL 20171006 [23495] -->
    <xsl:param name="pSQLMaturityRuleID" select="$gNull"/>
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pUnderlyingContractSymbol"/>
    <xsl:param name="pDerivativeContractIdentifier"/>
    <xsl:param name="pContractDisplayName"/>
    <xsl:param name="pInstrumentIdentifier"/>
    <xsl:param name="pCurrency"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pExerciseStyle"/>
    <!-- FL 20160701 [22084] add  pExerciseRule -->
    <xsl:param name="pExerciseRule" select="$gNull"/>
    <xsl:param name="pSettlMethod"/>
    <xsl:param name="pFutValuationMethod"/>
    <xsl:param name="pMinPriceIncr" select="$gNull"/>
    <xsl:param name="pMinPriceIncrAmount" select="$gNull"/>
    <xsl:param name="pContractFactor"/>
    <xsl:param name="pInsContractFactor" select="$gTrue"/>
    <xsl:param name="pUpdContractFactor" select="$gTrue"/>
    <xsl:param name="pContractMultiplier"/>
    <xsl:param name="pInsContractMultiplier" select="$gTrue"/>
    <xsl:param name="pUpdContractMultiplier" select="$gTrue"/>
    <xsl:param name="pNominalValue"/>
    <xsl:param name="pUnitOfMeasure" select="$gNull"/>
    <xsl:param name="pUnitOfMeasureQty" select="$gNull"/>
    <xsl:param name="pUnderlyingGroup"/>
    <xsl:param name="pUnderlyingAsset"/>
    <xsl:param name="pSQLUnderlyingIdAsset" select="$gNull"/>
    <xsl:param name="pUnderlyingISO10383"/>
    <xsl:param name="pContractSymbol_Shift"/>
    <xsl:param name="pAssignmentMethod"/>
    <xsl:param name="pContractAttribute" select="$gNull"/>
    <xsl:param name="pStrikeDecLocator" select="$gNull"/>
    <xsl:param name="pInsertMaturityRule" select="$gTrue"/>
    <!-- CS&GP 20111025 IDEM paramètres pAssetCategory et pUnderlyingIdentifier ajoutés -->
    <xsl:param name="pAssetCategory" select="$gNull"/>
    <xsl:param name="pUnderlyingIdentifier"/>
    <xsl:param name="pUnderlyingIsinCode"/>
    <!-- BD 20130219 Rajout du parametre ISIN Code pour mettre à jour l'ISIN Code du DC -->
    <xsl:param name="pIsinCode" select="$gNull"/>
    <!-- FI 20130220 -->
    <xsl:param name="pClearingOrgSymbol" select="$gNull"/>
    <!-- RD&FL 20120110 Dans le cas de L'Eurex, mettre tous les DC avec ISAUTOSSETING à True. -->
    <xsl:param name="pDerivativeContractIsAutoSetting" select="$gFalse"/>
    <!-- FI 20130228 100 valeur par défaut pour base -->
    <xsl:param name="pInstumentDen" select="100"/>
    <!-- BD 20130305 Ajout de la description du DC afin de ne pas passer par le trigger -->
    <xsl:param name="pDescription"/>
    <!-- BD 20130318 Ajout du parametre pUseDerivativeContractDefaultValue
          S'il est à true(), le template "ovrSQLGetDerivativeContractDefaultValue" sera appelé -->
    <xsl:param name="pUseDerivativeContractDefaultValue" select="false()"/>
    <!-- FL/RD 20140619[20113] Ajout du parametre pUseFactorDefaultValue
          S'il est à true(), le template "ovrSQLGetDerivativeContractDefaultValue" sera appelé pour charger la valeur de la colonne FACTOR -->
    <xsl:param name="pUseFactorDefaultValue" select="false()"/>
    <!-- BD 20130521 Ajout du param pIsAutoSettingAsset -->
    <xsl:param name="pIsAutoSettingAsset" select="$gFalse"/>
    <!-- BD 20130604 Ajout du param pFinalSettltPrice - Valeur par défaut : 'ExpiryDate' -->
    <xsl:param name="pFinalSettltPrice" select="'ExpiryDate'"/>
    <!--PM 20140515 [19970][19259] Ajout pNominalCurrency-->
    <xsl:param name="pNominalCurrency" select="$gNull"/>
    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>
    <!-- FL/PLA 20170420 [23064] add column PHYSETTLTAMOUNT -->
    <xsl:param name="pPhysettltamount" select="$gNull"/>
    <!-- FL/RD 20220105 [25920] add Check AssetCategory -->
    <xsl:param name="pIsCheckAssetCategory" select="$gFalse"/>

    <!-- Param variables -->
    <xsl:variable name="vParamISO10383">
      <Param name="ISO10383_ALPHA4" datatype="string">
        <xsl:value-of select="$pISO10383"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamExchangeSymbol">
      <!--FL/RD 20140326 [19648] Use new optional parameter pParamExchangeSymbol-->
      <xsl:choose>
        <xsl:when test="$pParamExchangeSymbol">
          <xsl:copy-of select="$pParamExchangeSymbol"/>
        </xsl:when>
        <xsl:otherwise>
          <Param name="EXCHANGESYMBOL" datatype="string">
            <xsl:value-of select="$pExchangeSymbol"/>
          </Param>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vParamContractSymbol">
      <Param name="CONTRACTSYMBOL" datatype="string">
        <xsl:value-of select="$pContractSymbol"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamInstrumentIdentifier">
      <Param name="INSTRUMENTIDENTIFIER" datatype="string">
        <xsl:value-of select="$pInstrumentIdentifier"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamCurrency">
      <!--PM 20140513 [19970][19259] Gestion du cas où la devise est fournie par une requête SQL -->
      <!--<Param name="CURRENCY" datatype="string">
        <xsl:value-of select="$pCurrency"/>
      </Param>-->
      <Param name="CURRENCY" datatype="string">
        <xsl:copy-of select="$pCurrency"/>
      </Param>
    </xsl:variable>
    <!--PM 20140515 [19970][19259] Ajout vNominalCurrency-->
    <xsl:variable name="vNominalCurrency">
      <xsl:choose>
        <xsl:when test="$pNominalCurrency=$gNull">
          <xsl:copy-of select="$pCurrency"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:copy-of select="$pNominalCurrency"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!--PM 20140515 [19970][19259] Ajout vParamNominalCurrency-->
    <xsl:variable name="vParamNominalCurrency">
      <Param name="CURRENCY" datatype="string">
        <xsl:copy-of select="$vNominalCurrency"/>
      </Param>
    </xsl:variable>

    <!-- SQL variables -->
    <xsl:variable name="vSQLDerivativeContractIdentifier">
      <xsl:call-template name="SQLDerivativeContractBefore">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'IDENTIFIER'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamISO10383">
          <xsl:copy-of select="$vParamISO10383"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLIsAutoSetting">
      <xsl:call-template name="SQLDerivativeContractBefore">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'ISAUTOSETTING'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamISO10383">
          <xsl:copy-of select="$vParamISO10383"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLIdInstrument">
      <xsl:call-template name="SQLIdInstrument">
        <xsl:with-param name="pParamInstrumentIdentifier">
          <xsl:copy-of select="$vParamInstrumentIdentifier"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLIdMarket">
      <xsl:call-template name="SQLIdMarket2">
        <xsl:with-param name="pISO10383" select="$pISO10383"/>
        <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
        <!--FL/RD 20140326 [19648] Use new optional parameter pParamExchangeSymbol-->
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLIdCurrency">
      <xsl:call-template name="SQLIdCurrency">
        <xsl:with-param name="pParamCurrency">
          <xsl:copy-of select="$vParamCurrency"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>
      </xsl:call-template>
    </xsl:variable>
    <!--PM 20140515 [19970][19259] ajout vSQLIdNominalCurrency-->
    <xsl:variable name="vSQLIdNominalCurrency">
      <xsl:call-template name="SQLIdCurrency">
        <xsl:with-param name="pParamCurrency">
          <xsl:copy-of select="$vParamNominalCurrency"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLIdMaturityRule">
      <!-- FL/PL 20130124 IDEM: Nouvelle variable "vSQLIdMaturityRule" -->
      <xsl:call-template name="SQLIdMaturityRule">
        <xsl:with-param name="pMaturityRuleIdentifier" select="$pMaturityRuleIdentifier"/>
      </xsl:call-template>
    </xsl:variable>

    <!-- Table DERIVATIVECONTRACT -->
    <!-- Remarque: action="IU"
         Le mode action="IU" vérifie si la ligne (ici le Contrat dérivé) existe, en utilisant les colonnes avec datakey=true :
         -	Si la ligne existe, un update est opéré 
         -	Si la ligne n’existe pas, un insert est opéré 
         Dans le contexte des DC, il existe une contrainte supplémentaire pour efefctuer un update: il s’agit de la colonne DERIVATIVECONTYRACT.ISAUTOSETTING (Autoriser la mise à jour automatique du contrat).
         Pour cette raison on ne peut utiliser le mode action="IU" et on doit procéder en 2 temps: action="U" puis action="I"
    -->
    <table name="DERIVATIVECONTRACT" action="U" sequenceno="1">
      <column name="ISAUTOSETTING" datakey="false" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLIsAutoSetting"/>
        <controls>
          <control action="RejectRow" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            true
          </control>
        </controls>
      </column>
      <column name="IDENTIFIER" datakey="true" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLDerivativeContractIdentifier"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            true
          </control>
        </controls>
      </column>

      <!-- BD 20130305 Ajout de la description du DC -->
      <xsl:if test="string-length($pDescription)>0">
        <column name="DESCRIPTION" datakey="false" datakeyupd="true" datatype="string">
          <xsl:value-of select="$pDescription"/>
        </column>
      </xsl:if>

      <!-- RD 20100402 / IDM is DataKey-->
      <column name="IDM" datakey="true" datakeyupd="false" datatype="int">
        <xsl:copy-of select="$vSQLIdMarket"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Incorrect Market.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The Market &lt;b&gt;<xsl:value-of select="$pExchangeSymbol"/>&lt;/b&gt; is not found.
                &lt;b&gt;Action:&lt;/b&gt; Create the Market &lt;b&gt;<xsl:value-of select="$pExchangeSymbol"/>&lt;/b&gt; or correct the input file, and restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>

      <!-- PL / FL / BD 20130605 : Mise en commentaire de l'Update de la colonne IDMATURITYRULE -->
      <!--<xsl:choose>
        -->
      <!-- FL/PL 20130124 IDEM: Alimentation éventuelle de la colonne "IDMATURITYRULE" -->
      <!--
        <xsl:when test="number($pMaturityRuleID) > 0">
          <column name="IDMATURITYRULE" datakey="false" datakeyupd="true" datatype="int">
            <xsl:value-of select="$pMaturityRuleID"/>
          </column>
        </xsl:when>

        <xsl:when test="$pMaturityRuleIdentifier != $gNull and $pMaturityRuleIdentifier != ''">
          <column name="IDMATURITYRULE" datakey="false" datakeyupd="true" datatype="int">
            <xsl:copy-of select="$vSQLIdMaturityRule"/>
            <controls>
              <control action="RejectRow" return="true">
                <SpheresLib function="ISNULL()"/>
                <logInfo status="REJECT" isexception="true">
                  <message>
                    &lt;b&gt;Incorrect Maturity Rule.&lt;/b&gt;
                    &lt;b&gt;Cause:&lt;/b&gt; The Maturity Rule for Derivative Contract &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt; on Market &lt;b&gt;<xsl:value-of select="$pExchangeSymbol"/>&lt;/b&gt; is not found.
                    &lt;b&gt;Action:&lt;/b&gt; Create the Maturity Rule &lt;b&gt;<xsl:value-of select="$pMaturityRuleIdentifier"/>&lt;/b&gt; or correct the input file, and restart the import.
                    <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
                  </message>
                </logInfo>
              </control>
            </controls>
          </column>
        </xsl:when>

        <xsl:otherwise>
          <column name="IDMATURITYRULE" datakey="false" datakeyupd="true" datatype="int">
            <controls>
              <control action="ApplyDefault" return="true" logtype="None">
                <SpheresLib function="ISNULL()"/>
              </control>
            </controls>
            <default>
              <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
                <xsl:with-param name="pResult" select="'IDMATURITYRULE'"/>
                <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
                <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
              </xsl:call-template>
            </default>
          </column>
        </xsl:otherwise>
      </xsl:choose>-->

      <!-- BD 20130718 : Ne pas updater ASSETCATEGORY avec null dans le cas d'un DC Option -->
      <!-- RD 20160219 [21937] Ne pas updater ASSETCATEGORY avec null pour tous les DC -->
      <!-- FL/RD 20220105 [25920] add Check AssetCategory -->
      <column name="ASSETCATEGORY" datakey="false" datakeyupd="true" datatype="string">
        <xsl:choose>
          <xsl:when test="(string-length($pAssetCategory)>0) and ($pAssetCategory!=$gNull)">
            <xsl:value-of select="$pAssetCategory"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
              <xsl:with-param name="pResult" select="'ASSETCATEGORY'"/>
              <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
              <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
        <!--<xsl:if test="$pCategory = 'O'">-->
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
            <xsl:if test="$pIsCheckAssetCategory=$gTrue">
              <logInfo status="WARNING" isexception="false">
                <message>
                  &lt;b&gt;Asset category is missing.&lt;/b&gt;
                  &lt;b&gt;Details:&lt;/b&gt;
                  - Exchange symbol: &lt;b&gt;<xsl:value-of select="$pExchangeSymbol"/>&lt;/b&gt;
                  - Contract symbol: &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt;
                  - Category: &lt;b&gt;<xsl:value-of select="$pCategory"/>&lt;/b&gt;
                  - Description: &lt;b&gt;<xsl:value-of select="$pDescription"/>&lt;/b&gt;
                  - xsl-t: &lt;b&gt;<xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/>&lt;/b&gt;
                </message>
              </logInfo>
            </xsl:if>
          </control>
        </controls>
        <!--</xsl:if>-->
      </column>

      <column name="IDC_PRICE" datakey="false" datakeyupd="true" datatype="string">
        <xsl:copy-of select="$vSQLIdCurrency"/>
        <controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Incorrect Currency.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The Currency &lt;b&gt;<xsl:value-of select="$pCurrency"/>&lt;/b&gt; is not found.
                &lt;b&gt;Action:&lt;/b&gt; Create the Currency &lt;b&gt;<xsl:value-of select="$pCurrency"/>&lt;/b&gt; or correct the input file, and restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
        <default>
          <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
            <xsl:with-param name="pResult" select="'IDC_PRICE'"/>
            <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
            <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
          </xsl:call-template>
        </default>
      </column>
      <column name="IDC_NOMINAL" datakey="false" datakeyupd="true" datatype="string">
        <!--PM 20140515 [19970][19259] remplacement de vSQLIdCurrency par vSQLIdNominalCurrency -->
        <!--<xsl:copy-of select="$vSQLIdCurrency"/>-->
        <xsl:copy-of select="$vSQLIdNominalCurrency"/>
        <controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Incorrect Currency.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The Currency &lt;b&gt;<xsl:value-of select="$vNominalCurrency"/>&lt;/b&gt; is not found.
                &lt;b&gt;Action:&lt;/b&gt; Create the Currency &lt;b&gt;<xsl:value-of select="$vNominalCurrency"/>&lt;/b&gt; or correct the input file, and restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
        <default>
          <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
            <xsl:with-param name="pResult" select="'IDC_NOMINAL'"/>
            <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
            <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
          </xsl:call-template>
        </default>
      </column>

      <column name="CATEGORY" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pCategory"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
            <!-- BD 20130702 Afficher un warning si CATEGORY est null -->
            <logInfo status="WARNING" isexception="false">
              <message>
                &lt;b&gt;Warning! Unknown category :&lt;/b&gt;
                - &lt;u&gt;Contract symbol:&lt;/u&gt; <xsl:value-of select="$pContractSymbol"/>
                - &lt;u&gt;Market:&lt;/u&gt; <xsl:value-of select="$pISO10383"/>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>

      <!--<xsl:if  test="$pExerciseStyle != '' or contains($pExtSQLFilterNames, 'MEFF_SPECIAL')">-->
      <column name="EXERCISESTYLE" datakey="false" datakeyupd="true" datatype="string">
        <xsl:choose>
          <!--FL 20140325 [19778] Sur les DC de catégorie "Future" la colonne "EXERCISESTYLE" doit toujours être à NULL-->
          <xsl:when test="$pCategory='F'">
            null
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="SelectOrFilterColumn">
              <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
              <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
              <xsl:with-param name="pDefaultValue" select="$pExerciseStyle"/>
              <xsl:with-param name="pResultField" select="'EXERCISESTYLE'"/>
            </xsl:call-template>

            <!--<xsl:value-of select="$pExerciseStyle"/>-->
            <controls>
              <control action="ApplyDefault" return="true" logtype="None">
                <SpheresLib function="ISNULL()"/>
              </control>
              <control action="RejectColumn" return="true" logtype="None">
                <SpheresLib function="ISNULL()"/>
              </control>
            </controls>
            <default>
              <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
                <xsl:with-param name="pResult" select="'EXERCISESTYLE'"/>
                <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
                <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
              </xsl:call-template>
            </default>
          </xsl:otherwise>
        </xsl:choose>
      </column>
      <!--</xsl:if>-->

      <!-- FL 20160701 Alimentation de EXERCISERULE -->
      <xsl:if test="$pExerciseRule != $gNull">
        <column name="EXERCISERULE" datakey="false" datakeyupd="true" datatype="string">
          <xsl:value-of select="$pExerciseRule"/>
        </column>
      </xsl:if>

      <!-- PM 20230303 [WI592] Ajout test sur Category -->
      <xsl:if test="$pCategory = 'O'">
        <!--<xsl:if test="$pStrikeDecLocator != $gNull and $pStrikeDecLocator != ''">-->
        <column name="STRIKEDECLOCATOR" datakey="false" datakeyupd="true" datatype="int">
          <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
            <xsl:with-param name="pResult" select="'STRIKEDECLOCATOR'"/>
            <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                          ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
            <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                          ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
          </xsl:call-template>

          <controls>
            <control action="ApplyDefault" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
            <control action="RejectColumn" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
          <default>
            <xsl:value-of select="$pStrikeDecLocator"/>
          </default>
        </column>
      </xsl:if>

      <column name="SETTLTMETHOD" datakey="false" datakeyupd="true" datatype="string">
        <xsl:call-template name="SelectOrFilterColumn">
          <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
          <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
          <xsl:with-param name="pDefaultValue" select="$pSettlMethod"/>
          <xsl:with-param name="pResultField" select="'SETTLTMETHOD'"/>
        </xsl:call-template>

        <!--<xsl:value-of select="$pSettlMethod"/>-->

        <controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Incorrect SETTLTMETHOD.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The Contract Symbol &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt; is not found.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
        <default>
          <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
            <xsl:with-param name="pResult" select="'SETTLTMETHOD'"/>
            <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
            <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
          </xsl:call-template>
        </default>
      </column>

      <!-- FL/PLA 20170420 [23064] add column PHYSETTLTAMOUNT -->
      <column name="PHYSETTLTAMOUNT" datakey="false" datakeyupd="true" datatype="string">
        <xsl:call-template name="SelectOrFilterColumn">
          <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
          <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
          <xsl:with-param name="pDefaultValue" select="$pPhysettltamount"/>
          <xsl:with-param name="pResultField" select="'PHYSETTLTAMOUNT'"/>
        </xsl:call-template>

        <controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Incorrect PHYSETTLTAMOUNT.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The Contract Symbol &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt; is not found.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
        <default>
          <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
            <xsl:with-param name="pResult" select="'PHYSETTLTAMOUNT'"/>
            <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
            <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
          </xsl:call-template>
        </default>
      </column>

      <column name="FUTVALUATIONMETHOD" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pFutValuationMethod"/>
        <controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Incorrect FUTVALUATIONMETHOD.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The Contract Symbol &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt; is not found.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
        <default>
          <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
            <xsl:with-param name="pResult" select="'FUTVALUATIONMETHOD'"/>
            <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
            <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
          </xsl:call-template>
        </default>
      </column>

      <!-- PL FL BD 20130507 Ne pas mettre à jour cette colonne -->
      <!--<column name="INSTRUMENTDEN" datakey="false" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$pInstumentDen"/>
        <controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
            <xsl:with-param name="pResult" select="'INSTRUMENTDEN'"/>
            <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
            <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
          </xsl:call-template>
        </default>
      </column>-->


      <!-- BD 20130318 - Rajout de la condition "or $pUseDerivativeContractDefaultValue=true()" afin de passer par ovrSQLGetDerivativeContractDefaultValue  -->
      <xsl:if test="($pMinPriceIncr != $gNull and $pMinPriceIncr != '') or $pUseDerivativeContractDefaultValue=true()">
        <column name="MINPRICEINCR" datakey="false" datakeyupd="true" datatype="decimal">
          <xsl:value-of select="$pMinPriceIncr"/>
          <controls>
            <control action="ApplyDefault" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
          <default>
            <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
              <xsl:with-param name="pResult" select="'MINPRICEINCR'"/>
              <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
              <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
            </xsl:call-template>
          </default>
        </column>
      </xsl:if>
      <xsl:if test="($pMinPriceIncrAmount != $gNull and $pMinPriceIncrAmount != '') or $pUseDerivativeContractDefaultValue=true()">
        <column name="MINPRICEINCRAMOUNT" datakey="false" datakeyupd="true" datatype="decimal">
          <xsl:value-of select="$pMinPriceIncrAmount"/>
          <controls>
            <control action="ApplyDefault" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
          <default>
            <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
              <xsl:with-param name="pResult" select="'MINPRICEINCRAMOUNT'"/>
              <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
              <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
            </xsl:call-template>
          </default>
        </column>
      </xsl:if>

      <xsl:if test="$pUpdContractFactor=$gTrue">
        <column name="FACTOR" datakey="false" datakeyupd="true" datatype="decimal">
          <xsl:call-template name="SelectOrFilterColumn">
            <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
            <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
            <xsl:with-param name="pDefaultValue" select="$pContractFactor"/>
            <xsl:with-param name="pResultField" select="'CONTRACTMULTIPLIER'"/>
          </xsl:call-template>
          <xsl:choose>
            <!-- FL/RD 20140619 [20113] Peu importe la valeur de pContractFactor, 
                si pUseFactorDefaultValue=true, on appel le template ovrSQLGetDerivativeContractDefaultValue pour charger la valeur de la colonne FACTOR-->
            <xsl:when test="$pUseFactorDefaultValue=true()">
              <controls>
                <control action="ApplyDefault" return="true" logtype="None">
                  <SpheresLib function="ISNULL()"/>
                </control>
                <control action="ApplyDefault" return="true" logtype="None">
                  <SpheresLib function="ISNOTNULL()"/>
                </control>
              </controls>
              <default>
                <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
                  <xsl:with-param name="pResult" select="'FACTOR'"/>
                  <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                            ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
                  <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                            ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
                </xsl:call-template>
              </default>
            </xsl:when>
            <!-- Peu importe la valeur de pContractFactor, si pUseDerivativeContractDefaultValue=true, on appel le template ovrSQLGetDerivativeContractDefaultValue -->
            <xsl:when test="$pUseDerivativeContractDefaultValue=true()">
              <controls>
                <control action="ApplyDefault" return="true" logtype="None">
                  <SpheresLib function="ISNULL()"/>
                </control>
                <control action="ApplyDefault" return="true" logtype="None">
                  <SpheresLib function="ISNOTNULL()"/>
                </control>
              </controls>
              <default>
                <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
                  <xsl:with-param name="pResult" select="'CONTRACTMULTIPLIER'"/>
                  <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                            ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
                  <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                            ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
                </xsl:call-template>
              </default>
            </xsl:when>
          </xsl:choose>
          <!--<xsl:value-of select="$pContractFactor"/>-->
        </column>
      </xsl:if>

      <xsl:if test="$pUpdContractMultiplier=$gTrue">
        <column name="CONTRACTMULTIPLIER" datakey="false" datakeyupd="true" datatype="decimal">

          <xsl:call-template name="SelectOrFilterColumn">
            <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
            <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
            <xsl:with-param name="pDefaultValue" select="$pContractMultiplier"/>
            <xsl:with-param name="pResultField" select="'CONTRACTMULTIPLIER'"/>
          </xsl:call-template>

          <controls>
            <control action="ApplyDefault" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
            <control action="RejectColumn" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
          <default>
            <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
              <xsl:with-param name="pResult" select="'CONTRACTMULTIPLIER'"/>
              <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
              <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
            </xsl:call-template>
          </default>

          <!--<xsl:value-of select="$pContractMultiplier"/>-->
        </column>
      </xsl:if>

      <!-- PM 20230303 [WI592] Ajout controle pour écarter les valeurs null -->
      <column name="PRICEDECLOCATOR" datakey="false" datakeyupd="true" datatype="int">
        <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
          <xsl:with-param name="pResult" select="'PRICEDECLOCATOR'"/>
          <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
          <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
        </xsl:call-template>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <!-- PM 20230303 [WI592] Ajout controle pour écarter les valeurs null -->
      <column name="PRICEALIGNCODE" datakey="false" datatype="string">
        <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
          <xsl:with-param name="pResult" select="'PRICEALIGNCODE'"/>
          <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
          <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
        </xsl:call-template>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <!-- PM 20230303 [WI592] Ajout test sur Category et controle pour écarter les valeurs null -->
      <xsl:if test="$pCategory = 'O'">
        <column name="STRIKEALIGNCODE" datakey="false" datatype="string">
          <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
            <xsl:with-param name="pResult" select="'STRIKEALIGNCODE'"/>
            <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
            <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
          </xsl:call-template>
          <controls>
            <control action="RejectColumn" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
        </column>
      </xsl:if>

      <!-- PM 20230303 [WI592] Ajout test sur Category -->
      <xsl:if test="$pCategory = 'O'">
        <column name="CABINETOPTVALUE" datakey="false" datatype="decimal">
          <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
            <xsl:with-param name="pResult" select="'CABINETOPTVALUE'"/>
            <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
            <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
          </xsl:call-template>
          <controls>
            <control action="RejectColumn" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
        </column>
      </xsl:if>

      <!-- RD 20160219 [21937]
       - La colonne NOMINALVALUE n'est pas updatée avec la donnée contenue dans le fichier source
       - Ne pas updater NOMINALVALUE avec null
      -->
      <column name="NOMINALVALUE" datakey="false" datakeyupd="true" datatype="decimal">
        <xsl:choose>
          <xsl:when test="(string-length($pNominalValue)>0) and ($pNominalValue!=$gNull)">
            <xsl:value-of select="$pNominalValue"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
              <xsl:with-param name="pResult" select="'NOMINALVALUE'"/>
              <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
              <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <xsl:if test="$pUnitOfMeasure != $gNull and $pUnitOfMeasure != ''">
        <column name="UNITOFMEASURE" datakey="false" datakeyupd="true" datatype="string">
          <xsl:value-of select="$pUnitOfMeasure"/>
        </column>
      </xsl:if>
      <xsl:if test="$pUnitOfMeasureQty != $gNull and $pUnitOfMeasureQty != ''">
        <column name="UNITOFMEASUREQTY" datakey="false" datakeyupd="true" datatype="decimal">
          <xsl:value-of select="$pUnitOfMeasureQty"/>
        </column>
      </xsl:if>

      <!-- FL 20140220 [19648] Gestion de la mise à jour de la donnée UNDERLYINGGROUP en mode "Modification d'un DC" -->
      <xsl:choose>

        <xsl:when test="$pUnderlyingGroup != $gNull and $pUnderlyingGroup != '' or contains($pExtSQLFilterNames, 'MEFF_SPECIAL')">
          <column name="UNDERLYINGGROUP" datakey="false" datakeyupd="true" datatype="string">
            <xsl:call-template name="SelectOrFilterColumn">
              <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
              <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
              <xsl:with-param name="pDefaultValue" select="$pUnderlyingGroup"/>
              <xsl:with-param name="pResultField" select="'UNDERLYINGGROUP'"/>
            </xsl:call-template>
            <!--<xsl:value-of select="$pUnderlyingGroup"/>-->
          </column>
        </xsl:when>

        <xsl:otherwise>
          <!-- FL 20140220 [19648] Gestion de ovrSQLGetDerivativeContractDefaultValue pour la mise à jour de cette donnée -->
          <column name="UNDERLYINGGROUP" datakey="false" datakeyupd="true" datatype="string">
            <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
              <xsl:with-param name="pResult" select="'UNDERLYINGGROUP'"/>
              <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
              <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
            </xsl:call-template>
            <controls>
              <control action="RejectColumn" return="true" logtype="None">
                <SpheresLib function="ISNULL()"/>
              </control>
            </controls>
          </column>
        </xsl:otherwise>

      </xsl:choose>

      <!-- FL 20140220 [19648] Gestion de la mise à jour de la donnée UNDERLYINGASSET en mode "Modification d'un DC" -->
      <xsl:choose>

        <xsl:when test="$pUnderlyingAsset != $gNull and $pUnderlyingAsset != '' or contains($pExtSQLFilterNames, 'MEFF_SPECIAL')">
          <column name="UNDERLYINGASSET" datakey="false" datakeyupd="true" datatype="string">
            <xsl:call-template name="SelectOrFilterColumn">
              <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
              <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
              <xsl:with-param name="pDefaultValue" select="$pUnderlyingAsset"/>
              <xsl:with-param name="pResultField" select="'UNDERLYINGASSET'"/>
            </xsl:call-template>
          </column>
        </xsl:when>

        <xsl:otherwise>
          <!-- FL 20140220 [19648] Gestion de ovrSQLGetDerivativeContractDefaultValue pour la mise à jour de cette donnée -->
          <column name="UNDERLYINGASSET" datakey="false" datakeyupd="true" datatype="string">
            <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
              <xsl:with-param name="pResult" select="'UNDERLYINGASSET'"/>
              <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
              <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
            </xsl:call-template>
            <controls>
              <control action="RejectColumn" return="true" logtype="None">
                <SpheresLib function="ISNULL()"/>
              </control>
            </controls>
          </column>

        </xsl:otherwise>

      </xsl:choose>
      
      <!-- PM 20230303 [WI592] Ajout test sur Category -->
      <xsl:if test="($pAssignmentMethod != $gNull) and ($pCategory = 'O')">

        <column name="ASSIGNMENTMETHOD" datakey="false" datakeyupd="false" datatype="string">
          <xsl:value-of select="$pAssignmentMethod"/>
        </column>

      </xsl:if>

      <xsl:choose>
        <xsl:when test="string-length($pUnderlyingContractSymbol)>0">

          <xsl:variable name="vParamUnderlyingContractSymbol">
            <Param name="UNDERLYINGCONTRACTSYMBOL" datatype="string">
              <xsl:value-of select="$pUnderlyingContractSymbol"/>
            </Param>
          </xsl:variable>

          <xsl:variable name="vSQLIdDerivativeContractUnderlying">
            <xsl:call-template name="SQLIdDerivativeContractUnderlying">
              <xsl:with-param name="pParamISO10383">
                <xsl:copy-of select="$vParamISO10383"/>
              </xsl:with-param>
              <xsl:with-param name="pParamExchangeSymbol">
                <xsl:copy-of select="$vParamExchangeSymbol"/>
              </xsl:with-param>
              <xsl:with-param name="pParamUnderlyingContractSymbol">
                <xsl:copy-of select="$vParamUnderlyingContractSymbol"/>
              </xsl:with-param>
            </xsl:call-template>
          </xsl:variable>

          <column name="IDDC_UNL" datakey="false" datakeyupd="true" datatype="int">
            <xsl:copy-of select="$vSQLIdDerivativeContractUnderlying"/>
          </column>
        </xsl:when>
        <!-- BD 20130515 IDDC_UNL = 'null' quand pAssetCategory est renseigné et ne vaut pas 'Future' -->
        <xsl:when test="$pAssetCategory!=$gNull and $pAssetCategory!='Future'">
          <column name="IDDC_UNL" datakey="false" datakeyupd="true" datatype="int"></column>
        </xsl:when>
        <xsl:otherwise>
          <column name="IDDC_UNL" datakey="false" datakeyupd="true" datatype="int">
            <controls>
              <control action="ApplyDefault" return="true" logtype="None">
                <SpheresLib function="ISNULL()"/>
              </control>
            </controls>
            <default>
              <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
                <xsl:with-param name="pResult" select="'IDDC_UNL'"/>
                <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORDSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL',',','ISINCODE')"/>
                <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory,',',$pIsinCode)"/>
              </xsl:call-template>
            </default>
          </column>
        </xsl:otherwise>
      </xsl:choose>

      <column name="IDASSET_UNL" datakey="false" datakeyupd="true" datatype="int">
        <xsl:choose>
          <!-- BD 20130304 : Rajout de condition pour passer dans ovrSQLGetDerivativeContractDefaultValue 
                            quand les param ne sont pas renseignés -->
          <xsl:when test="$pSQLUnderlyingIdAsset!=$gNull">
            <xsl:copy-of select="$pSQLUnderlyingIdAsset"/>
          </xsl:when>
          <xsl:when test="$pAssetCategory!=$gNull and 
                        string-length($pUnderlyingIdentifier)>0 and
                        string-length($pUnderlyingIsinCode)>0 and
                        string-length($pUnderlyingISO10383)>0">
            <xsl:call-template name="GetIdassetUnl">
              <xsl:with-param name="pAssetCategory" select="$pAssetCategory"/>
              <xsl:with-param name="pUnderlyingIdentifier" select="$pUnderlyingIdentifier"/>
              <xsl:with-param name="pUnderlyingIsinCode" select="$pUnderlyingIsinCode"/>
              <xsl:with-param name="pUnderlyingISO10383" select="$pUnderlyingISO10383"/>
              <xsl:with-param name="pResult" select="$gIDASSET"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
              <xsl:with-param name="pResult" select="'IDASSET_UNL'"/>
              <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORDSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL',',','ISINCODE')"/>
              <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory,',',$pIsinCode)"/>
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
        <!--<xsl:if test="($pCategory = 'O') and ($pAssetCategory != $gNull) and ($pAssetCategory != 'Future')">
          <controls>
            <control action="RejectColumn" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
              -->
        <!-- BD 20130702 Afficher un warning si IDASSET_UNL est null -->
        <!--
              <logInfo status="WARNING" isexception="false">
                <message>
                  &lt;b&gt;Warning! Underlying asset not found :&lt;/b&gt;
                  - &lt;u&gt;Contract symbol:&lt;/u&gt; <xsl:value-of select="$pContractSymbol"/>
                  - &lt;u&gt;Category:&lt;/u&gt; <xsl:value-of select="$pCategory"/>
                  - &lt;u&gt;Market:&lt;/u&gt; <xsl:value-of select="$pISO10383"/>
                  <xsl:if test="$pAssetCategory != $gNull">
                    - &lt;u&gt;Asset category:&lt;/u&gt; <xsl:value-of select="$pAssetCategory"/>
                  </xsl:if>
                  <xsl:if test="string-length($pUnderlyingIdentifier) > 0">
                    - &lt;u&gt;Asset identifier (or symbol):&lt;/u&gt; <xsl:value-of select="$pUnderlyingIdentifier"/>
                  </xsl:if>
                  <xsl:if test="string-length($pUnderlyingIsinCode) > 0">
                    - &lt;u&gt;Asset ISIN code:&lt;/u&gt; <xsl:value-of select="$pUnderlyingIsinCode"/>
                  </xsl:if>
                  <xsl:if test="(string-length($pUnderlyingISO10383) > 0) and ($pUnderlyingISO10383 != $gNull)">
                    - &lt;u&gt;Asset market:&lt;/u&gt; <xsl:value-of select="$pUnderlyingISO10383"/>
                  </xsl:if>
                </message>
              </logInfo>
            </control>
          </controls>
        </xsl:if>-->
      </column>


      <xsl:if test="$pContractAttribute != $gNull and $pContractAttribute != ''">
        <column name="CONTRACTATTRIBUTE" datakey="false" datakeyupd="true" datatype="int">
          <xsl:copy-of select="$pContractAttribute"/>
        </column>
      </xsl:if>

      <!-- FL 20130221 Alimentation de l'INSINCODE avec une valeur par défaut si pIsinCode vaut null  -->
      <column name="ISINCODE" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pIsinCode"/>
        <controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
            <xsl:with-param name="pResult" select="'ISINCODE'"/>
            <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORDSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL',',','ISINCODE')"/>
            <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory,',',$pIsinCode)"/>
          </xsl:call-template>
        </default>
      </column>
      
      <column name="BBGCODE" datakey="false" datakeyupd="true" datatype="string">
        <xsl:text>null</xsl:text>
        <controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
            <xsl:with-param name="pResult" select="'BLOOMBERG_CODE'"/>
            <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
            <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
          </xsl:call-template>
        </default>
      </column>
      <column name="RICCODE" datakey="false" datakeyupd="true" datatype="string">
        <xsl:text>null</xsl:text>
        <controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
            <xsl:with-param name="pResult" select="'REUTERS_CODE'"/>
            <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
            <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
          </xsl:call-template>
        </default>
      </column>

      <!-- BD 20130515 : Mise à jour de IDDC_SHIFT -->
      <xsl:if test="string-length($pContractSymbol_Shift) > 0">
        <column name="IDDC_SHIFT" datakey="false" datakeyupd="true" datatype="integer">
          <SQL command="select" result="IDDC" cache="true">
            select dc.IDDC
            from dbo.DERIVATIVECONTRACT dc
            inner join dbo.MARKET m on (m.IDM=dc.IDM) and (m.ISO10383_ALPHA4=@ISO10383_ALPHA4) and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL)
            where (dc.CONTRACTSYMBOL=@CONTRACTSYMBOL)
            <xsl:call-template name="SQLDTENABLEDDTDISABLED">
              <xsl:with-param name="pTable" select="'dc'"/>
            </xsl:call-template>
            <Param name="DT" datatype="date">
              <xsl:value-of select="$gParamDtBusiness"/>
            </Param>
            <Param name="ISO10383_ALPHA4" datatype="string">
              <xsl:value-of select="$pISO10383"/>
            </Param>
            <!--FL/RD 20140326 [19648] Use new optional parameter pParamExchangeSymbol-->
            <!--<Param name="EXCHANGESYMBOL" datatype="string">
              <xsl:value-of select="$pExchangeSymbol"/>
            </Param>-->
            <xsl:copy-of select="$vParamExchangeSymbol"/>
            <Param name="CONTRACTSYMBOL" datatype="string">
              <xsl:value-of select="$pContractSymbol_Shift"/>
            </Param>
          </SQL>
        </column>
      </xsl:if>

      <!-- BD 20130604 Alimentation de FINALSETTLTPRICE -->
      <column name="FINALSETTLTPRICE" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pFinalSettltPrice"/>
      </column>

      <!-- BD 20130521 Insert avec la valeur de pIsAutoSettingAsset -->
      <column name="ISAUTOSETTINGASSET" datakey="false" datakeyupd="true" datatype="bool">
        <xsl:value-of select="$pIsAutoSettingAsset"/>
      </column>

      <!-- BD 20130429 Update de la colonne DTDISABLED -->
      <!-- DTDISABLED est utilisé comme datakey pour mettre à jour le DC vivant -->
      <column name="DTDISABLED" datakey="true" datakeyupd="false" datatype="date">
        <controls>
          <!-- Si le DC existe, et qu'il est vivant, on applique le <default> (ApplyDefault) -->
          <control action="ApplyDefault" return="1" logtype="None">
            <SQL command="select" result="IS_EXISTDCALIVE" cache="true">
              select count(1) as IS_EXISTDCALIVE
              from dbo.DERIVATIVECONTRACT dc
              where dc.CONTRACTSYMBOL = @CONTRACTSYMBOL
              and dc.IDM = @IDM
              <xsl:call-template name="SQLDTENABLEDDTDISABLED">
                <xsl:with-param name="pTable" select="'dc'"/>
              </xsl:call-template>
              <xsl:call-template name="SQLWhereClauseExtension">
                <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
                <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
              </xsl:call-template>
              <Param name="CONTRACTSYMBOL" datatype="string">
                <xsl:value-of select="$pContractSymbol"/>
              </Param>
              <Param name="IDM" datatype="integer">
                <xsl:copy-of select="$vSQLIdMarket"/>
              </Param>
              <Param name="DT" datatype="date">
                <xsl:value-of select="$gParamDtBusiness"/>
              </Param>
            </SQL>
          </control>
          <!-- Si le DC n'existe aucun DC en vie, on rejette la ligne (RejectRow) -->
          <control action="RejectRow" return="0" logtype="None">
            <SQL command="select" result="IS_EXISTDCALIVE" cache="true">
              select count(1) as IS_EXISTDCALIVE
              from dbo.DERIVATIVECONTRACT dc
              where dc.CONTRACTSYMBOL = @CONTRACTSYMBOL
              and dc.IDM = @IDM
              <xsl:call-template name="SQLDTENABLEDDTDISABLED">
                <xsl:with-param name="pTable" select="'dc'"/>
              </xsl:call-template>
              <xsl:call-template name="SQLWhereClauseExtension">
                <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
                <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
              </xsl:call-template>
              <Param name="CONTRACTSYMBOL" datatype="string">
                <xsl:value-of select="$pContractSymbol"/>
              </Param>
              <Param name="IDM" datatype="integer">
                <xsl:copy-of select="$vSQLIdMarket"/>
              </Param>
              <Param name="DT" datatype="date">
                <xsl:value-of select="$gParamDtBusiness"/>
              </Param>
            </SQL>
          </control>
        </controls>
        <default>
          <!-- On récupère la DTDISABLED du DC vivant, comme datakey, pour indiquer à IO de mettre à jour ce DC -->
          <SQL command="select" result="DTDISABLED" cache="true">
            select DTDISABLED
            from dbo.DERIVATIVECONTRACT dc
            where dc.CONTRACTSYMBOL = @CONTRACTSYMBOL
            and dc.IDM = @IDM
            <xsl:call-template name="SQLDTENABLEDDTDISABLED">
              <xsl:with-param name="pTable" select="'dc'"/>
            </xsl:call-template>
            <xsl:call-template name="SQLWhereClauseExtension">
              <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
              <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
            </xsl:call-template>
            <Param name="CONTRACTSYMBOL" datatype="string">
              <xsl:value-of select="$pContractSymbol"/>
            </Param>
            <Param name="IDM" datatype="integer">
              <xsl:copy-of select="$vSQLIdMarket"/>
            </Param>
            <Param name="DT" datatype="date">
              <xsl:value-of select="$gParamDtBusiness"/>
            </Param>
          </SQL>
        </default>
      </column>

      <xsl:call-template name="SysUpd">
        <xsl:with-param name="pIsWithControl">
          <xsl:copy-of select="$gFalse"/>
        </xsl:with-param>
      </xsl:call-template>

    </table>

    <table name="DERIVATIVECONTRACT" action="I" sequenceno="2">
      <column name="IDENTIFIER" datakey="true" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLDerivativeContractIdentifier"/>
        <controls>
          <!-- BD 20130429 Mise en commentaire du controle ci-dessous: on peut avoir plusieurs DC avec le même IDENTIFIER -->
          <control action="RejectRow" return="true" logtype="None">
            <SpheresLib function="ISNOTNULL()"/>
          </control>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:call-template name="SelectOrFilterColumn">
            <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
            <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
            <xsl:with-param name="pDefaultValue" select="$pDerivativeContractIdentifier"/>
          </xsl:call-template>
          <!--<xsl:value-of select="$pDerivativeContractIdentifier"/>-->
        </default>
      </column>

      <!-- BD 20130305 Ajout de la description du DC -->
      <xsl:if test="string-length($pDescription)>0">
        <column name="DESCRIPTION" datakey="false" datakeyupd="true" datatype="string">
          <xsl:value-of select="$pDescription"/>
        </column>
      </xsl:if>

      <!-- RD 20100402 / IDM is DataKey-->
      <column name="IDM" datakey="true" datakeyupd="false" datatype="int">
        <xsl:copy-of select="$vSQLIdMarket"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Incorrect Market.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The Market &lt;b&gt;<xsl:value-of select="$pExchangeSymbol"/>&lt;/b&gt; is not found.
                &lt;b&gt;Action:&lt;/b&gt; Create the Market &lt;b&gt;<xsl:value-of select="$pExchangeSymbol"/>&lt;/b&gt; or correct the input file, and restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>

      <xsl:choose>
        <!-- FL/PL 20130124 IDEM: Alimentation éventuelle de la colonne "IDMATURITYRULE" -->
        <xsl:when test="number($pMaturityRuleID) > 0">
          <column name="IDMATURITYRULE" datakey="false" datakeyupd="true" datatype="int">
            <xsl:value-of select="$pMaturityRuleID"/>
          </column>
        </xsl:when>

        <xsl:when test="$pMaturityRuleIdentifier != $gNull and $pMaturityRuleIdentifier != ''">
          <column name="IDMATURITYRULE" datakey="false" datakeyupd="true" datatype="int">
            <xsl:copy-of select="$vSQLIdMaturityRule"/>
            <controls>
              <control action="RejectRow" return="true">
                <SpheresLib function="ISNULL()"/>
                <logInfo status="REJECT" isexception="true">
                  <message>
                    &lt;b&gt;Incorrect Maturity Rule.&lt;/b&gt;
                    &lt;b&gt;Cause:&lt;/b&gt; The Maturity Rule for Derivative Contract &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt; on Market &lt;b&gt;<xsl:value-of select="$pExchangeSymbol"/>&lt;/b&gt; is not found.
                    &lt;b&gt;Action:&lt;/b&gt; Create the Maturity Rule &lt;b&gt;<xsl:value-of select="$pMaturityRuleIdentifier"/>&lt;/b&gt; or correct the input file, and restart the import.
                    <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
                  </message>
                </logInfo>
              </control>
            </controls>
          </column>
        </xsl:when>

        <!--FL 20201217 [25606]-->
        <!--FL 20171006 [23495]-->
        <!--<xsl:when test="number($pSQLMaturityRuleID) != $gNull">-->
        <xsl:when test="$pSQLMaturityRuleID != $gNull">
          <column name="IDMATURITYRULE" datakey="false" datakeyupd="true" datatype="int">
            <xsl:copy-of select="$pSQLMaturityRuleID"/>
          </column>
        </xsl:when>

        <xsl:otherwise>
          <column name="IDMATURITYRULE" datakey="false" datakeyupd="true" datatype="int">
            <controls>
              <control action="ApplyDefault" return="true" logtype="None">
                <SpheresLib function="ISNULL()"/>
              </control>
            </controls>
            <default>
              <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
                <xsl:with-param name="pResult" select="'IDMATURITYRULE'"/>
                <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
                <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
              </xsl:call-template>
            </default>
          </column>
        </xsl:otherwise>
      </xsl:choose>

      <column name="DISPLAYNAME" datakey="false" datakeyupd="false" datatype="string">

        <xsl:call-template name="SelectOrFilterColumn">
          <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
          <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
          <xsl:with-param name="pDefaultValue" select="$pContractDisplayName"/>
          <xsl:with-param name="pResultField" select="'DISPLAYNAME'"/>
        </xsl:call-template>

        <!--<xsl:value-of select="$pContractDisplayName"/>-->
      </column>
      <column name="IDI" datakey="false" datakeyupd="true" datatype="int">
        <xsl:copy-of select="$vSQLIdInstrument"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="false">
              <message>
                &lt;b&gt;Incorrect Instrument.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The Instrument &lt;b&gt;<xsl:value-of select="$pInstrumentIdentifier"/>&lt;/b&gt; is not found.
                &lt;b&gt;Action:&lt;/b&gt; Create the Instrument &lt;b&gt;<xsl:value-of select="$pInstrumentIdentifier"/>&lt;/b&gt; or correct the input file, and restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>

      <column name="IDC_PRICE" datakey="false" datakeyupd="true" datatype="string">
        <xsl:copy-of select="$vSQLIdCurrency"/>
        <controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Incorrect Currency.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The Currency &lt;b&gt;<xsl:value-of select="$pCurrency"/>&lt;/b&gt; is not found.
                &lt;b&gt;Action:&lt;/b&gt; Create the Currency &lt;b&gt;<xsl:value-of select="$pCurrency"/>&lt;/b&gt; or correct the input file, and restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
        <default>
          <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
            <xsl:with-param name="pResult" select="'IDC_PRICE'"/>
            <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
            <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
          </xsl:call-template>
        </default>
      </column>
      <column name="IDC_NOMINAL" datakey="false" datakeyupd="true" datatype="string">
        <!--PM 20140515 [19970][19259] remplacement de vSQLIdCurrency par vSQLIdNominalCurrency -->
        <!--<xsl:copy-of select="$vSQLIdCurrency"/>-->
        <xsl:copy-of select="$vSQLIdNominalCurrency"/>
        <controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Incorrect Currency.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The Currency &lt;b&gt;<xsl:value-of select="$vNominalCurrency"/>&lt;/b&gt; is not found.
                &lt;b&gt;Action:&lt;/b&gt; Create the Currency &lt;b&gt;<xsl:value-of select="$vNominalCurrency"/>&lt;/b&gt; or correct the input file, and restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
        <default>
          <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
            <xsl:with-param name="pResult" select="'IDC_NOMINAL'"/>
            <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
            <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
          </xsl:call-template>
        </default>
      </column>

      <column name="CATEGORY" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pCategory"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
            <!-- BD 20130702 Afficher un warning si CATEGORY est null -->
            <logInfo status="WARNING" isexception="false">
              <message>
                &lt;b&gt;Warning! Unknown category :&lt;/b&gt;
                - &lt;u&gt;Contract symbol:&lt;/u&gt; <xsl:value-of select="$pContractSymbol"/>
                - &lt;u&gt;Market:&lt;/u&gt; <xsl:value-of select="$pISO10383"/>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>

      <!-- PM 20230303 [WI592] Ajout test sur Category -->
      <xsl:if test="$pCategory = 'O'">
        <!--<xsl:if  test="$pExerciseStyle != '' or contains($pExtSQLFilterNames, 'MEFF_SPECIAL')">-->
        <column name="EXERCISESTYLE" datakey="false" datakeyupd="true" datatype="string">
          <xsl:choose>
            <!--RD/FL 20140226 [19648] Sur les DC de catégorie "Future" la colonne "EXERCISESTYLE" doit toujours être à NULL-->
            <xsl:when test="$pCategory='F'">
              null
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="SelectOrFilterColumn">
                <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
                <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
                <xsl:with-param name="pDefaultValue" select="$pExerciseStyle"/>
                <xsl:with-param name="pResultField" select="'EXERCISESTYLE'"/>
              </xsl:call-template>

              <!--<xsl:value-of select="$pExerciseStyle"/>-->
              <controls>
                <control action="ApplyDefault" return="true" logtype="None">
                  <SpheresLib function="ISNULL()"/>
                </control>
                <control action="RejectColumn" return="true" logtype="None">
                  <SpheresLib function="ISNULL()"/>
                </control>
              </controls>
              <default>
                <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
                  <xsl:with-param name="pResult" select="'EXERCISESTYLE'"/>
                  <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                          ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
                  <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                          ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
                </xsl:call-template>
              </default>
            </xsl:otherwise>
          </xsl:choose>
        </column>
      </xsl:if>

      <!-- FL 20160701 Alimentation de EXERCISERULE -->
      <!-- RD 20180515 Colonne EXERCISERULE en double, voir plus bas -->
      <!--<xsl:if test="$pExerciseRule != $gNull">
        <column name="EXERCISERULE" datakey="false" datakeyupd="true" datatype="string">
          <xsl:value-of select="$pExerciseRule"/>
        </column>
      </xsl:if>-->

      <!-- PM 20230303 [WI592] Ajout test sur Category -->
      <xsl:if test="$pCategory = 'O'">
        <column name="STRIKEDECLOCATOR" datakey="false" datakeyupd="true" datatype="int">
          <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
            <xsl:with-param name="pResult" select="'STRIKEDECLOCATOR'"/>
            <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                          ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
            <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                          ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
          </xsl:call-template>

          <controls>
            <control action="ApplyDefault" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
            <control action="RejectColumn" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
          <default>
            <xsl:value-of select="$pStrikeDecLocator"/>
          </default>
        </column>
      </xsl:if>

      <column name="CONTRACTSYMBOL" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pContractSymbol"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Incorrect ContractSymbol.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The ContractSymbol &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt; is not found.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>

      <column name="SETTLTMETHOD" datakey="false" datakeyupd="true" datatype="string">

        <xsl:call-template name="SelectOrFilterColumn">
          <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
          <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
          <xsl:with-param name="pDefaultValue" select="$pSettlMethod"/>
          <xsl:with-param name="pResultField" select="'SETTLTMETHOD'"/>
        </xsl:call-template>

        <!--<xsl:value-of select="$pSettlMethod"/>-->

        <controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Incorrect SETTLTMETHOD.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The Contract Symbol &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt; is not found.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
        <default>
          <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
            <xsl:with-param name="pResult" select="'SETTLTMETHOD'"/>
            <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
            <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
          </xsl:call-template>
        </default>
      </column>

      <!-- FL/PLA 20170420 [23064] add column PHYSETTLTAMOUNT -->
      <column name="PHYSETTLTAMOUNT" datakey="false" datakeyupd="true" datatype="string">
        <xsl:call-template name="SelectOrFilterColumn">
          <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
          <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
          <xsl:with-param name="pDefaultValue" select="$pPhysettltamount"/>
          <xsl:with-param name="pResultField" select="'PHYSETTLTAMOUNT'"/>
        </xsl:call-template>

        <controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Incorrect PHYSETTLTAMOUNT.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The Contract Symbol &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt; is not found.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
        <default>
          <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
            <xsl:with-param name="pResult" select="'PHYSETTLTAMOUNT'"/>
            <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
            <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
          </xsl:call-template>
        </default>
      </column>

      <column name="FUTVALUATIONMETHOD" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pFutValuationMethod"/>
        <controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Incorrect FUTVALUATIONMETHOD.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The Contract Symbol &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt; is not found.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
        <default>
          <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
            <xsl:with-param name="pResult" select="'FUTVALUATIONMETHOD'"/>
            <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
            <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
          </xsl:call-template>
        </default>
      </column>

      <column name="PRICETYPE" datakey="false" datakeyupd="false" datatype="string">10</column>
      <column name="INSTRUMENTDEN" datakey="false" datakeyupd="false" datatype="decimal">
        <xsl:value-of select="$pInstumentDen"/>
        <controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
            <xsl:with-param name="pResult" select="'INSTRUMENTDEN'"/>
            <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
            <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
          </xsl:call-template>
        </default>
      </column>

      <column name="INSTRUMENTNUM" datakey="false" datakeyupd="false" datatype="decimal">1</column>

      <!-- BD 20130318 - Rajout de la condition "or $pUseDerivativeContractDefaultValue=true()" afin de passer par ovrSQLGetDerivativeContractDefaultValue  -->
      <xsl:if test="($pMinPriceIncr != $gNull and $pMinPriceIncr != '') or $pUseDerivativeContractDefaultValue=true()">
        <column name="MINPRICEINCR" datakey="false" datakeyupd="true" datatype="decimal">
          <xsl:value-of select="$pMinPriceIncr"/>
          <controls>
            <control action="ApplyDefault" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
          <default>
            <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
              <xsl:with-param name="pResult" select="'MINPRICEINCR'"/>
              <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
              <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
            </xsl:call-template>
          </default>
        </column>
      </xsl:if>
      <xsl:if test="($pMinPriceIncrAmount != $gNull and $pMinPriceIncrAmount != '') or $pUseDerivativeContractDefaultValue=true()">
        <column name="MINPRICEINCRAMOUNT" datakey="false" datakeyupd="true" datatype="decimal">
          <xsl:value-of select="$pMinPriceIncrAmount"/>
          <controls>
            <control action="ApplyDefault" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
          <default>
            <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
              <xsl:with-param name="pResult" select="'MINPRICEINCRAMOUNT'"/>
              <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
              <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
            </xsl:call-template>
          </default>
        </column>
      </xsl:if>
      <xsl:if test="$pInsContractFactor=$gTrue">
        <column name="FACTOR" datakey="false" datakeyupd="true" datatype="decimal">
          <xsl:call-template name="SelectOrFilterColumn">
            <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
            <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
            <xsl:with-param name="pDefaultValue" select="$pContractFactor"/>
            <xsl:with-param name="pResultField" select="'CONTRACTMULTIPLIER'"/>
          </xsl:call-template>
          <controls>
            <control action="ApplyDefault" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
            <control action="RejectColumn" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
          <default>
            <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
              <xsl:with-param name="pResult" select="'FACTOR'"/>
              <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
              <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
            </xsl:call-template>
          </default>

          <!--<xsl:value-of select="$pContractFactor"/>-->
        </column>
      </xsl:if>

      <xsl:if test="$pInsContractMultiplier=$gTrue">
        <column name="CONTRACTMULTIPLIER" datakey="false" datakeyupd="true" datatype="decimal">

          <xsl:call-template name="SelectOrFilterColumn">
            <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
            <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
            <xsl:with-param name="pDefaultValue" select="$pContractMultiplier"/>
            <xsl:with-param name="pResultField" select="'CONTRACTMULTIPLIER'"/>
          </xsl:call-template>

          <controls>
            <control action="ApplyDefault" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
            <control action="RejectColumn" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
          <default>
            <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
              <xsl:with-param name="pResult" select="'CONTRACTMULTIPLIER'"/>
              <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
              <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
            </xsl:call-template>
          </default>

          <!--<xsl:value-of select="$pContractMultiplier"/>-->
        </column>
      </xsl:if>

      <column name="NOMINALVALUE" datakey="false" datakeyupd="true" datatype="decimal">
        <xsl:call-template name="SelectOrFilterColumn">
          <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
          <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
          <xsl:with-param name="pDefaultValue" select="$pNominalValue"/>
          <xsl:with-param name="pResultField" select="'NOMINALVALUE'"/>
        </xsl:call-template>
        <!--<xsl:value-of select="$pNominalValue"/>-->
      </column>

      <xsl:if test="$pUnitOfMeasure != $gNull and $pUnitOfMeasure != ''">
        <column name="UNITOFMEASURE" datakey="false" datakeyupd="true" datatype="string">
          <xsl:value-of select="$pUnitOfMeasure"/>
        </column>
      </xsl:if>
      <xsl:if test="$pUnitOfMeasureQty != $gNull and $pUnitOfMeasureQty != ''">
        <column name="UNITOFMEASUREQTY" datakey="false" datakeyupd="true" datatype="decimal">
          <xsl:value-of select="$pUnitOfMeasureQty"/>
        </column>
      </xsl:if>

      <column name="PRICEDECLOCATOR" datakey="false" datatype="int">
        <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
          <xsl:with-param name="pResult" select="'PRICEDECLOCATOR'"/>
          <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
          <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
        </xsl:call-template>
      </column>

      <column name="PRICEALIGNCODE" datakey="false" datatype="string">
        <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
          <xsl:with-param name="pResult" select="'PRICEALIGNCODE'"/>
          <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
          <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
        </xsl:call-template>
      </column>

      <!-- PM 20230303 [WI592] Ajout test sur Category -->
      <xsl:if test="$pCategory = 'O'">
        <column name="STRIKEALIGNCODE" datakey="false" datatype="string">
          <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
            <xsl:with-param name="pResult" select="'STRIKEALIGNCODE'"/>
            <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
            <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
          </xsl:call-template>
        </column>
      </xsl:if>

      <!-- PM 20230303 [WI592] Ajout test sur Category -->
      <xsl:if test="$pCategory = 'O'">
        <column name="CABINETOPTVALUE" datakey="false" datatype="decimal">
          <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
            <xsl:with-param name="pResult" select="'CABINETOPTVALUE'"/>
            <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
            <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
          </xsl:call-template>
        </column>
      </xsl:if>
      
      <column name="PRICEQUOTEMETHOD" datakey="false" datakeyupd="false" datatype="string">
        <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
          <xsl:with-param name="pResult" select="'PRICEQUOTEMETHOD'"/>
          <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
          <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
        </xsl:call-template>

        <controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectRow" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          STD
        </default>

      </column>

      <!-- BD 20130604 Alimentation de FINALSETTLTPRICE -->
      <column name="FINALSETTLTPRICE" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pFinalSettltPrice"/>
      </column>

      <!--RD&FL 20120110 Dans le cas de L'Eurex, mettre tous les DC avec ISAUTOSSETING à True.-->
      <column name="ISAUTOSETTING" datakey="false" datakeyupd="false" datatype="bool">
        <xsl:value-of select="$pDerivativeContractIsAutoSetting"/>
      </column>
      <!-- BD 20130521 Insert avec la valeur de pIsAutoSettingAsset -->
      <column name="ISAUTOSETTINGASSET" datakey="false" datakeyupd="false" datatype="bool">
        <xsl:value-of select="$pIsAutoSettingAsset"/>
      </column>
      <column name="ISAUTOCREATEASSET" datakey="false" datakeyupd="false" datatype="bool">true</column>
      <!-- RD 20130827 [18834] Gestion de la checkbox "Importation systématique des cours" -->
      <column name="ISMANDATORYIMPORTQUOTE" datakey="false" datakeyupd="false" datatype="bool">false</column>
      <!-- RD 20100402 / Warning by default-->
      <column name="VRSTRIKEPXRANGE" datakey="false" datakeyupd="false" datatype="string">Warning</column>
      <column name="VRSTRIKEINCREMENT" datakey="false" datakeyupd="false" datatype="string">Warning</column>
      <column name="VRMATURITYDATE" datakey="false" datakeyupd="false" datatype="string">Warning</column>
      <column name="VRLASTTRADINGDAY" datakey="false" datakeyupd="false" datatype="string">Warning</column>
      <column name="VRDELIVERYDATE" datakey="false" datakeyupd="false" datatype="string">Warning</column>

      <!-- FL 20140220 [19648] Gestion de la mise à jour de la donnée UNDERLYINGGROUP en mode "Creation d'un DC" -->
      <xsl:choose>

        <xsl:when test="$pUnderlyingGroup != $gNull and $pUnderlyingGroup != '' or contains($pExtSQLFilterNames, 'MEFF_SPECIAL')">
          <column name="UNDERLYINGGROUP" datakey="false" datakeyupd="true" datatype="string">
            <xsl:call-template name="SelectOrFilterColumn">
              <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
              <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
              <xsl:with-param name="pDefaultValue" select="$pUnderlyingGroup"/>
              <xsl:with-param name="pResultField" select="'UNDERLYINGGROUP'"/>
            </xsl:call-template>
            <!--<xsl:value-of select="$pUnderlyingGroup"/>-->
          </column>
        </xsl:when>

        <xsl:otherwise>
          <!-- FL 20140220 [19648] Gestion de ovrSQLGetDerivativeContractDefaultValue pour la mise à jour de cette donnée -->
          <column name="UNDERLYINGGROUP" datakey="false" datakeyupd="true" datatype="string">
            <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
              <xsl:with-param name="pResult" select="'UNDERLYINGGROUP'"/>
              <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
              <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
            </xsl:call-template>
          </column>
        </xsl:otherwise>

      </xsl:choose>

      <!-- FL 20140220 [19648] Gestion de la mise à jour de la donnée UNDERLYINGASSET en mode "Creation d'un DC" -->
      <xsl:choose>

        <xsl:when test="$pUnderlyingAsset != $gNull and $pUnderlyingAsset != '' or contains($pExtSQLFilterNames, 'MEFF_SPECIAL')">
          <column name="UNDERLYINGASSET" datakey="false" datakeyupd="true" datatype="string">
            <xsl:call-template name="SelectOrFilterColumn">
              <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
              <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
              <xsl:with-param name="pDefaultValue" select="$pUnderlyingAsset"/>
              <xsl:with-param name="pResultField" select="'UNDERLYINGASSET'"/>
            </xsl:call-template>
          </column>
        </xsl:when>

        <xsl:otherwise>
          <!-- FL 20140220 [19648] Gestion de ovrSQLGetDerivativeContractDefaultValue pour la mise à jour de cette donnée -->
          <column name="UNDERLYINGASSET" datakey="false" datakeyupd="true" datatype="string">
            <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
              <xsl:with-param name="pResult" select="'UNDERLYINGASSET'"/>
              <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
              <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
            </xsl:call-template>
          </column>
        </xsl:otherwise>

      </xsl:choose>

      <xsl:if test="string-length($pContractSymbol_Shift) > 0">
        <column name="IDDC_SHIFT" datakey="false" datakeyupd="true" datatype="integer">
          <SQL command="select" result="IDDC" cache="true">
            select dc.IDDC
            from dbo.DERIVATIVECONTRACT dc
            inner join dbo.MARKET m on (m.IDM=dc.IDM) and (m.ISO10383_ALPHA4=@ISO10383_ALPHA4) and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL)
            where (dc.CONTRACTSYMBOL=@CONTRACTSYMBOL)
            <xsl:call-template name="SQLDTENABLEDDTDISABLED">
              <xsl:with-param name="pTable" select="'dc'"/>
            </xsl:call-template>
            <Param name="DT" datatype="date">
              <xsl:value-of select="$gParamDtBusiness"/>
            </Param>
            <Param name="ISO10383_ALPHA4" datatype="string">
              <xsl:value-of select="$pISO10383"/>
            </Param>
            <!--FL/RD 20140326 [19648] Use new optional parameter pParamExchangeSymbol-->
            <!--<Param name="EXCHANGESYMBOL" datatype="string">
              <xsl:value-of select="$pExchangeSymbol"/>
            </Param>-->
            <xsl:copy-of select="$vParamExchangeSymbol"/>
            <Param name="CONTRACTSYMBOL" datatype="string">
              <xsl:value-of select="$pContractSymbol_Shift"/>
            </Param>
          </SQL>
        </column>
      </xsl:if>

      <!-- PM 20230303 [WI592] Ajout test sur Category -->
      <xsl:if test="($pAssignmentMethod != $gNull) and ($pCategory = 'O')">

        <column name="ASSIGNMENTMETHOD" datakey="false" datakeyupd="false" datatype="string">
          <xsl:value-of select="$pAssignmentMethod"/>
        </column>

      </xsl:if>

      <xsl:choose>
        <xsl:when test="string-length($pUnderlyingContractSymbol)>0">

          <xsl:variable name="vParamUnderlyingContractSymbol">
            <Param name="UNDERLYINGCONTRACTSYMBOL" datatype="string">
              <xsl:value-of select="$pUnderlyingContractSymbol"/>
            </Param>
          </xsl:variable>

          <xsl:variable name="vSQLIdDerivativeContractUnderlying">
            <xsl:call-template name="SQLIdDerivativeContractUnderlying">
              <xsl:with-param name="pParamISO10383">
                <xsl:copy-of select="$vParamISO10383"/>
              </xsl:with-param>
              <xsl:with-param name="pParamExchangeSymbol">
                <xsl:copy-of select="$vParamExchangeSymbol"/>
              </xsl:with-param>
              <xsl:with-param name="pParamUnderlyingContractSymbol">
                <xsl:copy-of select="$vParamUnderlyingContractSymbol"/>
              </xsl:with-param>
            </xsl:call-template>
          </xsl:variable>

          <column name="IDDC_UNL" datakey="false" datakeyupd="true" datatype="int">
            <xsl:copy-of select="$vSQLIdDerivativeContractUnderlying"/>
          </column>
        </xsl:when>
        <!-- BD 20130515 IDDC_UNL = 'null' quand pAssetCategory est renseigné et ne vaut pas 'Future' -->
        <xsl:when test="$pAssetCategory!=$gNull and $pAssetCategory!='Future'">
          <column name="IDDC_UNL" datakey="false" datakeyupd="true" datatype="int"></column>
        </xsl:when>
        <xsl:otherwise>
          <column name="IDDC_UNL" datakey="false" datakeyupd="true" datatype="int">
            <controls>
              <control action="ApplyDefault" return="true" logtype="None">
                <SpheresLib function="ISNULL()"/>
              </control>
            </controls>
            <default>
              <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
                <xsl:with-param name="pResult" select="'IDDC_UNL'"/>
                <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORDSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL',',','ISINCODE')"/>
                <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory,',',$pIsinCode)"/>
              </xsl:call-template>
            </default>
          </column>

        </xsl:otherwise>
      </xsl:choose>

      <xsl:if test="$pContractAttribute != $gNull and $pContractAttribute != ''">
        <column name="CONTRACTATTRIBUTE" datakey="false" datakeyupd="true" datatype="int">
          <xsl:copy-of select="$pContractAttribute"/>
        </column>
      </xsl:if>

      <!-- FL/RD 20220105 [25920] add Check AssetCategory -->
      <column name="ASSETCATEGORY" datakey="false" datakeyupd="true" datatype="string">
        <xsl:choose>
          <xsl:when test="($pAssetCategory!=$gNull) and (string-length($pAssetCategory)>0)">
            <xsl:value-of select="$pAssetCategory"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
              <xsl:with-param name="pResult" select="'ASSETCATEGORY'"/>
              <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORDSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
              <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
            <xsl:if test="$pIsCheckAssetCategory=$gTrue">
              <logInfo status="WARNING" isexception="false">
                <message>
                  &lt;b&gt;Asset category is missing.&lt;/b&gt;
                  &lt;b&gt;Details:&lt;/b&gt;
                  - Exchange symbol: &lt;b&gt;<xsl:value-of select="$pExchangeSymbol"/>&lt;/b&gt;
                  - Contract symbol: &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt;
                  - Category: &lt;b&gt;<xsl:value-of select="$pCategory"/>&lt;/b&gt;
                  - Description: &lt;b&gt;<xsl:value-of select="$pDescription"/>&lt;/b&gt;
                  - <xsl:text>&#xa;</xsl:text>xsl-t: &lt;b&gt;<xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>&lt;/b&gt;
                </message>
              </logInfo>
            </xsl:if>
          </control>
        </controls>
      </column>

      <column name="IDASSET_UNL" datakey="false" datakeyupd="true" datatype="int">
        <xsl:choose>
          <!-- BD 20130304 : Rajout de condition pour passer dans ovrSQLGetDerivativeContractDefaultValue 
                            quand les param ne sont pas renseignés -->
          <xsl:when test="$pSQLUnderlyingIdAsset!=$gNull">
            <xsl:copy-of select="$pSQLUnderlyingIdAsset"/>
          </xsl:when>
          <xsl:when test="$pAssetCategory!=$gNull and 
                        string-length($pUnderlyingIdentifier)>0 and
                        string-length($pUnderlyingIsinCode)>0 and
                        string-length($pUnderlyingISO10383)>0">
            <xsl:call-template name="GetIdassetUnl">
              <xsl:with-param name="pAssetCategory" select="$pAssetCategory"/>
              <xsl:with-param name="pUnderlyingIdentifier" select="$pUnderlyingIdentifier"/>
              <xsl:with-param name="pUnderlyingIsinCode" select="$pUnderlyingIsinCode"/>
              <xsl:with-param name="pUnderlyingISO10383" select="$pUnderlyingISO10383"/>
              <xsl:with-param name="pResult" select="$gIDASSET"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
              <xsl:with-param name="pResult" select="'IDASSET_UNL'"/>
              <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORDSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL',',','ISINCODE')"/>
              <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory,',',$pIsinCode)"/>
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:if test="($pCategory = 'O') and ($pAssetCategory != $gNull) and ($pAssetCategory != 'Future')">
          <controls>
            <control action="RejectColumn" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
              <!-- BD 20130702 Afficher un warning si IDASSET_UNL est null -->
              <logInfo status="WARNING" isexception="false">
                <message>
                  &lt;b&gt;Warning! Underlying asset not found :&lt;/b&gt;
                  - &lt;u&gt;Contract symbol:&lt;/u&gt; <xsl:value-of select="$pContractSymbol"/>
                  - &lt;u&gt;Category:&lt;/u&gt; <xsl:value-of select="$pCategory"/>
                  - &lt;u&gt;Market:&lt;/u&gt; <xsl:value-of select="$pISO10383"/>
                  <xsl:if test="$pAssetCategory != $gNull">
                    - &lt;u&gt;Asset category:&lt;/u&gt; <xsl:value-of select="$pAssetCategory"/>
                  </xsl:if>
                  <xsl:if test="string-length($pUnderlyingIdentifier) > 0">
                    - &lt;u&gt;Asset identifier (or symbol):&lt;/u&gt; <xsl:value-of select="$pUnderlyingIdentifier"/>
                  </xsl:if>
                  <xsl:if test="string-length($pUnderlyingIsinCode) > 0">
                    - &lt;u&gt;Asset ISIN code:&lt;/u&gt; <xsl:value-of select="$pUnderlyingIsinCode"/>
                  </xsl:if>
                  <xsl:if test="(string-length($pUnderlyingISO10383) > 0) and ($pUnderlyingISO10383 != $gNull)">
                    - &lt;u&gt;Asset market:&lt;/u&gt; <xsl:value-of select="$pUnderlyingISO10383"/>
                  </xsl:if>
                </message>
              </logInfo>
            </control>
          </controls>
        </xsl:if>
      </column>

      <!-- FL 20130221 Alimentation de l'INSINCODE avec une valeur par défaut si pIsinCode vaut null  -->
      <column name="ISINCODE" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pIsinCode"/>
        <controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
            <xsl:with-param name="pResult" select="'ISINCODE'"/>
            <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORDSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL',',','ISINCODE')"/>
            <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory,',',$pIsinCode)"/>
          </xsl:call-template>
        </default>

      </column>

      <column name="BBGCODE" datakey="false" datakeyupd="true" datatype="string">
        <xsl:text>null</xsl:text>
        <controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
            <xsl:with-param name="pResult" select="'BLOOMBERG_CODE'"/>
            <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
            <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
          </xsl:call-template>
        </default>
      </column>
      <column name="RICCODE" datakey="false" datakeyupd="true" datatype="string">
        <xsl:text>null</xsl:text>
        <controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
            <xsl:with-param name="pResult" select="'REUTERS_CODE'"/>
            <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
            <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
          </xsl:call-template>
        </default>
      </column>
      
      <!-- BD 20130704 Mise en commentaire du code ci-dessous -->
      <!-- BD 20130429 On (re)crée le DC s'il n'existe pas, ou alors qu'il est désactivé -->
      <!--<column name="DTDISABLED" datakey="true" datakeyupd="false" datatype="date">
        <controls>
          <control action="RejectRow" return="1" logtype="none">
            <SQL command="select" result="IS_ALREADYEXISTALIVE" cache="true">
              select count(1) as IS_ALREADYEXISTALIVE
              from dbo.DERIVATIVECONTRACT dc
              where dc.CONTRACTSYMBOL = @CONTRACTSYMBOL
              and dc.IDM = @IDM
              <xsl:call-template name="SQLDTENABLEDDTDISABLED">
                <xsl:with-param name="pTable" select="'dc'"/>
              </xsl:call-template>
              <xsl:call-template name="SQLWhereClauseExtension">
                <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
                <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
              </xsl:call-template>
              <Param name="CONTRACTSYMBOL" datatype="string">
                <xsl:value-of select="$pContractSymbol"/>
              </Param>
              <Param name="IDM" datatype="integer">
                <xsl:copy-of select="$vSQLIdMarket"/>
              </Param>
              <Param name="DT" datatype="date">
                <xsl:value-of select="$gParamDtBusiness"/>
              </Param>
            </SQL>
          </control>
        </controls>
      </column>-->

      <!-- FI 20131120 [19216] add column CONTRACTTYPE -->
      <column name="CONTRACTTYPE" datakey="false" datakeyupd="true" datatype="string">
        <xsl:text>null</xsl:text>
        <controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
            <xsl:with-param name="pResult" select="'CONTRACTTYPE'"/>
            <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
            <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
          </xsl:call-template>
        </default>
      </column>

      <!-- FI 20131129 [19284] add column FINALSETTLTSIDE -->
      <column name="FINALSETTLTSIDE" datakey="false" datakeyupd="true" datatype="string">
        <xsl:text>null</xsl:text>
        <controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
            <xsl:with-param name="pResult" select="'FINALSETTLTSIDE'"/>
            <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,
                        ',','CONTRACTSYMBOLEXL',',','CLEARINGORGSYMBOLEXL',',','EXCHANGESYMBOLEXL',',','CATEGORYEXL')"/>
            <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,
                        ',',$pContractSymbol,',',$pClearingOrgSymbol,',',$pExchangeSymbol,',',$pCategory)"/>
          </xsl:call-template>
        </default>
      </column>

      <!-- FL 20160701 Alimentation de EXERCISERULE -->
      <xsl:if test="$pExerciseRule != $gNull">
        <column name="EXERCISERULE" datakey="false" datakeyupd="false" datatype="string">
          <xsl:value-of select="$pExerciseRule"/>
        </column>
      </xsl:if>

      <xsl:call-template name="SysIns">
        <xsl:with-param name="pIsWithControl">
          <xsl:copy-of select="$gFalse"/>
        </xsl:with-param>
        <xsl:with-param name="pUseDtBusiness">
          <xsl:copy-of select="$gTrue"/>
        </xsl:with-param>
      </xsl:call-template>
    </table>

  </xsl:template>


  <!-- SQLTableDERIVATIVECONTRACT_UpdateIDMATURITYRULE -->
  <xsl:template name="SQLTableDERIVATIVECONTRACT_UpdateIDMATURITYRULE">
    <xsl:param name="pExchangeSymbol"/>
    <!--<xsl:param name="pMaturityRuleIdentifier"/>-->
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pContractAttribute" select="$gNull"/>

    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>

    <!-- Param variables-->
    <xsl:variable name="vParamExchangeSymbol">
      <Param name="EXCHANGESYMBOL" datatype="string">
        <xsl:value-of select="$pExchangeSymbol"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamContractSymbol">
      <Param name="CONTRACTSYMBOL" datatype="string">
        <xsl:value-of select="$pContractSymbol"/>
      </Param>
    </xsl:variable>
    <!--<xsl:variable name="vParamMaturityRuleIdentifier">
      <Param name="MATURITYRULEIDENTIFIER" datatype="string">
        <xsl:value-of select="$pMaturityRuleIdentifier"/>
      </Param>
    </xsl:variable>-->

    <!-- SQL variables-->
    <xsl:variable name="vSQLDerivativeContractIdentifier">
      <xsl:call-template name="SQLDerivativeContractAfter">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'IDENTIFIER'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLIdMaturityRule">
      <xsl:call-template name="SQLIdMaturityRuleAfter">
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLIDMaturityRuleFromDerivativeContract">
      <xsl:call-template name="SQLDerivativeContractAfter">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'IDMATURITYRULE'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <!--<xsl:variable name="vSQLIsAutoSettingAsset">
      <xsl:call-template name="SQLDerivativeContractAfter">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'ISAUTOSETTINGASSET'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>
        
      </xsl:call-template>
    </xsl:variable>-->
    <xsl:variable name="vSQLDerivativeContractExists">
      <xsl:call-template name="SQLDerivativeContractAfter">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'IDENTIFIER'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLIdMarket">
      <xsl:call-template name="SQLIdMarket">
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    <!--	*******************************************************************
						Table DERIVATIVECONTRACT
				*******************************************************************	-->
    <table name="DERIVATIVECONTRACT" action="U" sequenceno="4">

      <!--<column name="ISAUTOSETTINGASSET" datakey="false" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLIsAutoSettingAsset"/>
        <controls>
          <control action="RejectRow" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            true
          </control>
        </controls>
      </column>-->
      <column name="DCONTRACTEXISTS" datakey="false" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLDerivativeContractExists"/>
        <controls>
          <control action="RejectRow" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            true
          </control>
        </controls>
      </column>
      <!-- DERIVATIVECONTRACT.IDMATURITYRULE is NULL-->
      <column name="CHECKIDMATURITYRULEINDERIVATIVECONTRACT" datakey="false" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLIDMaturityRuleFromDerivativeContract"/>
        <controls>
          <control action="RejectRow" return="false" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            true
          </control>
        </controls>
      </column>
      <column name="IDENTIFIER" datakey="true" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLDerivativeContractIdentifier"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            true
          </control>
        </controls>
      </column>
      <!-- RD 20100402 / IDM is DataKey-->
      <column name="IDM" datakey="true" datakeyupd="false" datatype="int">
        <xsl:copy-of select="$vSQLIdMarket"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Incorrect Market.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The Market &lt;b&gt;<xsl:value-of select="$pExchangeSymbol"/>&lt;/b&gt; is not found.
                &lt;b&gt;Action:&lt;/b&gt; Create the Market &lt;b&gt;<xsl:value-of select="$pExchangeSymbol"/>&lt;/b&gt; or correct the input file, and restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>
      <column name="IDMATURITYRULE" datakey="false" datakeyupd="true" datatype="int">
        <xsl:copy-of select="$vSQLIdMaturityRule"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Incorrect Maturity Rule.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The Maturity Rule for Contract &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt; on Market &lt;b&gt;<xsl:value-of select="$pExchangeSymbol"/>&lt;/b&gt; is not found.
                &lt;b&gt;Action:&lt;/b&gt; Create the Maturity Rule or correct the input file, and restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>
      <xsl:call-template name="SysUpd">
        <xsl:with-param name="pIsWithControl">
          <xsl:copy-of select="$gFalse"/>
        </xsl:with-param>
      </xsl:call-template>
    </table>

  </xsl:template>

  <!-- FI  Template destiné à remplacer SQLTableASSET_ETD -->
  <!-- Prend en considération de MarketISO10383 -->
  <xsl:template name="SQLTableASSET_ETD2">
    <xsl:param name="pISO10383"/>
    <xsl:param name="pExchangeSymbol"/>
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pMaturityMonthYear"/>
    <xsl:param name="pAssetIdentifier"/>
    <xsl:param name="pAssetDisplayName"/>
    <xsl:param name="pAssetSymbol"/>
    <xsl:param name="pAssetISINCode"/>
    <xsl:param name="pAIICode"/>
    <xsl:param name="pAssetMultiplier"/>
    <xsl:param name="pAssetFactor"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pAssetPutCall"/>
    <xsl:param name="pAssetStrikePrice"/>
    <xsl:param name="pAssetFirstQuotationDay"/>
    <xsl:param name="pContractAttribute" select="$gNull"/>

    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>

    <!-- Param variables-->
    <xsl:variable name="vParamISO10383">
      <Param name="ISO10383_ALPHA4" datatype="string">
        <xsl:value-of select="$pISO10383"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamExchangeSymbol">
      <Param name="EXCHANGESYMBOL" datatype="string">
        <xsl:value-of select="$pExchangeSymbol"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamContractSymbol">
      <Param name="CONTRACTSYMBOL" datatype="string">
        <xsl:value-of select="$pContractSymbol"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamAssetIdentifier">
      <Param name="IDENTIFIER" datatype="string">
        <xsl:value-of select="$pAssetIdentifier"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamMaturityMonthYear">
      <Param name="MATURITYMONTHYEAR" datatype="string">
        <xsl:value-of select="$pMaturityMonthYear"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamAssetPutCall">
      <Param name="PUTCALL" datatype="string">
        <xsl:value-of select="$pAssetPutCall"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamAssetStrikePrice">
      <Param name="STRIKEPRICE" datatype="decimal">
        <xsl:value-of select="$pAssetStrikePrice"/>
      </Param>
    </xsl:variable>

    <!-- SQL variables-->
    <xsl:variable name="vSQLAssetIdentifier">
      <xsl:call-template name="SQLAssetIdentifierExistsAndRowChanged2">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'IDENTIFIER'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamISO10383">
          <xsl:copy-of select="$vParamISO10383"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamMaturityMonthYear">
          <xsl:copy-of select="$vParamMaturityMonthYear"/>
        </xsl:with-param>
        <xsl:with-param name="pParamStrikePrice">
          <xsl:copy-of select="$vParamAssetStrikePrice"/>
        </xsl:with-param>
        <xsl:with-param name="pParamPutCall">
          <xsl:copy-of select="$vParamAssetPutCall"/>
        </xsl:with-param>
        <xsl:with-param name="pContractAttribute">
          <xsl:copy-of select="$pContractAttribute"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetDisplayName">
          <xsl:copy-of select="$pAssetDisplayName"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetISINCode">
          <xsl:copy-of select="$pAssetISINCode"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetSymbol">
          <xsl:copy-of select="$pAssetSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetFirstQuotationDay">
          <xsl:copy-of select="$pAssetFirstQuotationDay"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vSQLAssetIdentifierExistsAndRowChanged">
      <xsl:call-template name="SQLAssetIdentifierExistsAndRowChanged2">
        <xsl:with-param name="pParamISO10383">
          <xsl:copy-of select="$vParamISO10383"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamMaturityMonthYear">
          <xsl:copy-of select="$vParamMaturityMonthYear"/>
        </xsl:with-param>
        <xsl:with-param name="pParamStrikePrice">
          <xsl:copy-of select="$vParamAssetStrikePrice"/>
        </xsl:with-param>
        <xsl:with-param name="pParamPutCall">
          <xsl:copy-of select="$vParamAssetPutCall"/>
        </xsl:with-param>
        <xsl:with-param name="pContractAttribute">
          <xsl:copy-of select="$pContractAttribute"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetDisplayName">
          <xsl:copy-of select="$pAssetDisplayName"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetISINCode">
          <xsl:copy-of select="$pAssetISINCode"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetSymbol">
          <xsl:copy-of select="$pAssetSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetFirstQuotationDay">
          <xsl:copy-of select="$pAssetFirstQuotationDay"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLAssetDisplayname">
      <xsl:call-template name="SQLAssetIdentifierExistsAndRowChanged2">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'DISPLAYNAME'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamISO10383">
          <xsl:copy-of select="$vParamISO10383"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamMaturityMonthYear">
          <xsl:copy-of select="$vParamMaturityMonthYear"/>
        </xsl:with-param>
        <xsl:with-param name="pParamStrikePrice">
          <xsl:copy-of select="$vParamAssetStrikePrice"/>
        </xsl:with-param>
        <xsl:with-param name="pParamPutCall">
          <xsl:copy-of select="$vParamAssetPutCall"/>
        </xsl:with-param>
        <xsl:with-param name="pContractAttribute">
          <xsl:copy-of select="$pContractAttribute"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetDisplayName">
          <xsl:copy-of select="$pAssetDisplayName"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetISINCode">
          <xsl:copy-of select="$pAssetISINCode"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetSymbol">
          <xsl:copy-of select="$pAssetSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetFirstQuotationDay">
          <xsl:copy-of select="$pAssetFirstQuotationDay"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLAssetAIICode">
      <xsl:call-template name="SQLAssetIdentifierExistsAndRowChanged2">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'AII'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamISO10383">
          <xsl:copy-of select="$vParamISO10383"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamMaturityMonthYear">
          <xsl:copy-of select="$vParamMaturityMonthYear"/>
        </xsl:with-param>
        <xsl:with-param name="pParamStrikePrice">
          <xsl:copy-of select="$vParamAssetStrikePrice"/>
        </xsl:with-param>
        <xsl:with-param name="pParamPutCall">
          <xsl:copy-of select="$vParamAssetPutCall"/>
        </xsl:with-param>
        <xsl:with-param name="pContractAttribute">
          <xsl:copy-of select="$pContractAttribute"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetDisplayName">
          <xsl:copy-of select="$pAssetDisplayName"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetISINCode">
          <xsl:copy-of select="$pAssetISINCode"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetSymbol">
          <xsl:copy-of select="$pAssetSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetFirstQuotationDay">
          <xsl:copy-of select="$pAssetFirstQuotationDay"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLAssetISINCode">
      <xsl:call-template name="SQLAssetIdentifierExistsAndRowChanged2">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'ISINCODE'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamISO10383">
          <xsl:copy-of select="$vParamISO10383"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamMaturityMonthYear">
          <xsl:copy-of select="$vParamMaturityMonthYear"/>
        </xsl:with-param>
        <xsl:with-param name="pParamStrikePrice">
          <xsl:copy-of select="$vParamAssetStrikePrice"/>
        </xsl:with-param>
        <xsl:with-param name="pParamPutCall">
          <xsl:copy-of select="$vParamAssetPutCall"/>
        </xsl:with-param>
        <xsl:with-param name="pContractAttribute">
          <xsl:copy-of select="$pContractAttribute"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetDisplayName">
          <xsl:copy-of select="$pAssetDisplayName"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetISINCode">
          <xsl:copy-of select="$pAssetISINCode"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetSymbol">
          <xsl:copy-of select="$pAssetSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetFirstQuotationDay">
          <xsl:copy-of select="$pAssetFirstQuotationDay"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLAssetSymbol">
      <xsl:call-template name="SQLAssetIdentifierExistsAndRowChanged2">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'ASSETSYMBOL'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamISO10383">
          <xsl:copy-of select="$vParamISO10383"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamMaturityMonthYear">
          <xsl:copy-of select="$vParamMaturityMonthYear"/>
        </xsl:with-param>
        <xsl:with-param name="pParamStrikePrice">
          <xsl:copy-of select="$vParamAssetStrikePrice"/>
        </xsl:with-param>
        <xsl:with-param name="pParamPutCall">
          <xsl:copy-of select="$vParamAssetPutCall"/>
        </xsl:with-param>
        <xsl:with-param name="pContractAttribute">
          <xsl:copy-of select="$pContractAttribute"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetDisplayName">
          <xsl:copy-of select="$pAssetDisplayName"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetISINCode">
          <xsl:copy-of select="$pAssetISINCode"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetSymbol">
          <xsl:copy-of select="$pAssetSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetFirstQuotationDay">
          <xsl:copy-of select="$pAssetFirstQuotationDay"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLIdDerivativeAttrib">
      <xsl:call-template name="SQLIdDerivativeAttrib2">
        <xsl:with-param name="pParamISO10383">
          <xsl:copy-of select="$vParamISO10383"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamMaturityMonthYear">
          <xsl:copy-of select="$vParamMaturityMonthYear"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLIsAutoSettingAsset">
      <xsl:call-template name="SQLDerivativeContractAfter2">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'ISAUTOSETTINGASSET'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamISO10383">
          <xsl:copy-of select="$vParamISO10383"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>

    <!--	*******************************************************************
						Table ASSET_ETD
		    *******************************************************************	-->
    <table name="ASSET_ETD" action="U" sequenceno="7">
      <!--DERIVATIVECONTRACT Exists and ISAUTOSETTINGASSET-->
      <column name="ISAUTOSETTINGASSET" datakey="false" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLIsAutoSettingAsset"/>
        <controls>
          <control action="RejectRow" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            true
          </control>
        </controls>
      </column>
      <column name="CHECKIDENTIFIEREXITSANDROWCHANGED" datakey="false" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLAssetIdentifierExistsAndRowChanged"/>
        <controls>
          <control action="RejectRow" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            true
          </control>
        </controls>
      </column>
      <column name="IDENTIFIER" datakey="true" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLAssetIdentifier"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            true
          </control>
        </controls>
      </column>
      <!-- RD 20100402 / IDDERIVATIVEATTRIB is DataKey-->
      <column name="IDDERIVATIVEATTRIB" datakey="true" datakeyupd="false" datatype="int">
        <xsl:copy-of select="$vSQLIdDerivativeAttrib"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Incorrect Derivative Attribut.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The Derivative Attribut is not found for Maturity &lt;b&gt;<xsl:value-of select="$pMaturityMonthYear"/>&lt;/b&gt; of Contract &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt; on Market &lt;b&gt;<xsl:value-of select="$pExchangeSymbol"/>&lt;/b&gt;.
                &lt;b&gt;Action:&lt;/b&gt; Create the corresponding Derivative Attribut and restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>
      <column name="DISPLAYNAME" datakey="false" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLAssetDisplayname"/>
        <controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="false" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:value-of select="$pAssetDisplayName"/>
        </default>
      </column>
      <!--column name="DESCRIPTION" datakey="false" datakeyupd="false" datatype="string">
        null
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="IsUpdate()"/>
          </control>
        </controls>
      </column-->
      <column name="ISINCODE" datakey="false" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLAssetISINCode"/>
        <controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="false" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:value-of select="$pAssetISINCode"/>
        </default>
      </column>
      <column name="AII" datakey="false" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLAssetAIICode"/>
        <controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="false" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:value-of select="$pAIICode"/>
        </default>
      </column>

      <column name="CFICODE" datakey="false" datakeyupd="true" datatype="string">

        <xsl:call-template name="SelectOrFilterColumn">
          <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
          <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
          <xsl:with-param name="pDefaultValue" select="$gNull"/>
          <xsl:with-param name="pResultField" select="'CFICODE'"/>
        </xsl:call-template>

      </column>

      <column name="ASSETSYMBOL" datakey="false" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLAssetSymbol"/>
        <controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="false" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:value-of select="$pAssetSymbol"/>
        </default>
      </column>
      <!--column name="ISOPENCLOSESETTLT" datakey="false" datakeyupd="false" datatype="int">
        0
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="IsUpdate()"/>
          </control>
        </controls>
      </column-->

      <!-- PL 20130221 Add CONTRACTMULTIPLIER and FACTOR -->
      <xsl:if test="($pAssetMultiplier != $gNull and $pAssetMultiplier != '') or (contains($pExtSQLFilterNames, 'MEFF_SPECIAL'))">
        <column name="CONTRACTMULTIPLIER" datakey="false" datakeyupd="true" datatype="decimal">
          <xsl:call-template name="SelectOrFilterColumn">
            <xsl:with-param name="pNames" select="concat($pExtSQLFilterNames,',','CONTRACTMULTIPLIERASSET')"/>
            <xsl:with-param name="pValues" select="concat($pExtSQLFilterValues,',','KEY_OP')"/>
            <xsl:with-param name="pDefaultValue" select="$pAssetMultiplier"/>
            <xsl:with-param name="pResultField" select="'CONTRACTMULTIPLIER'"/>
          </xsl:call-template>
        </column>
      </xsl:if>
      <xsl:if test="($pAssetFactor != $gNull and $pAssetFactor != '') or (contains($pExtSQLFilterNames, 'MEFF_SPECIAL'))">
        <column name="FACTOR" datakey="false" datakeyupd="true" datatype="decimal">
          <xsl:call-template name="SelectOrFilterColumn">
            <xsl:with-param name="pNames" select="concat($pExtSQLFilterNames,',','CONTRACTMULTIPLIERASSET')"/>
            <xsl:with-param name="pValues" select="concat($pExtSQLFilterValues,',','KEY_OP')"/>
            <xsl:with-param name="pDefaultValue" select="$pAssetFactor"/>
            <xsl:with-param name="pResultField" select="'CONTRACTMULTIPLIER'"/>
          </xsl:call-template>
        </column>
      </xsl:if>

      <!--column name="ISSECURITYSTATUS" datakey="false" datakeyupd="false" datatype="int">
        1
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="IsUpdate()"/>
          </control>
        </controls>
      </column-->
      <!--column name="STRIKEPRICE" datakey="true" datakeyupd="false" datatype="decimal">
        <xsl:value-of select="$pAssetStrikePrice"/>
      </column-->
      <!--column name="PUTCALL" datakey="true" datakeyupd="false" datatype="string">
        <xsl:value-of select="$pAssetPutCall"/>
      </column-->
      <column name="FIRSTQUOTATIONDAY" datakey="false" datakeyupd="false" datatype="date"  dataformat="yyyyMMdd">
        <xsl:value-of select="$pAssetFirstQuotationDay"/>
        <controls>
          <!--control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control-->
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <!--default>
          <xsl:value-of select="$pAssetFirstQuotationDay"/>
        </default-->
      </column>
      <xsl:call-template name="SysUpd">
        <xsl:with-param name="pIsWithControl">
          <xsl:copy-of select="$gFalse"/>
        </xsl:with-param>
      </xsl:call-template>
    </table>
    <table name="ASSET_ETD" action="I" sequenceno="8">
      <!--DERIVATIVECONTRACT Exists and ISAUTOSETTINGASSET-->
      <column name="ISAUTOSETTINGASSET" datakey="false" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLIsAutoSettingAsset"/>
        <controls>
          <control action="RejectRow" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            true
          </control>
        </controls>
      </column>
      <column name="IDENTIFIER" datakey="true" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLAssetIdentifier"/>
        <controls>
          <control action="RejectRow" return="false" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:value-of select="$pAssetIdentifier"/>
        </default>
      </column>
      <!-- RD 20100402 / IDDERIVATIVEATTRIB is DataKey-->
      <column name="IDDERIVATIVEATTRIB" datakey="true" datakeyupd="false" datatype="int">
        <xsl:copy-of select="$vSQLIdDerivativeAttrib"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Incorrect Derivative Attribut.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The Derivative Attribut is not found for Maturity &lt;b&gt;<xsl:value-of select="$pMaturityMonthYear"/>&lt;/b&gt; of Contract &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt; on Market &lt;b&gt;<xsl:value-of select="$pExchangeSymbol"/>&lt;/b&gt;.
                &lt;b&gt;Action:&lt;/b&gt; Create the corresponding Derivative Attribut and restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>
      <column name="DISPLAYNAME" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pAssetDisplayName"/>
        <!--controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:value-of select="$pAssetDisplayName"/>
        </default-->
      </column>
      <!--column name="DESCRIPTION" datakey="false" datakeyupd="false" datatype="string">
        null
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="IsUpdate()"/>
          </control>
        </controls>
      </column-->
      <column name="ISINCODE" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pAssetISINCode"/>
        <!--controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="false" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:value-of select="$pAssetISINCode"/>
        </default-->
      </column>
      <column name="AII" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pAIICode"/>
      </column>

      <column name="CFICODE" datakey="false" datakeyupd="true" datatype="string">
        <xsl:call-template name="SelectOrFilterColumn">
          <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
          <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
          <xsl:with-param name="pDefaultValue" select="$gNull"/>
          <xsl:with-param name="pResultField" select="'CFICODE'"/>
        </xsl:call-template>
      </column>

      <column name="ASSETSYMBOL" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pAssetSymbol"/>
        <!--controls>
          <control action="ApplyDefault" return="true" logtype="None">
					<SpheresLib function="ISNULL()"/>
				</control>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <<default>
				<xsl:value-of select="$pAssetSymbol"/>
			</default>-->
      </column>
      <column name="ISOPENCLOSESETTLT" datakey="false" datakeyupd="false" datatype="int">
        <xsl:text>0</xsl:text>
        <!--controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="IsUpdate()"/>
          </control>
        </controls-->
      </column>

      <xsl:if test="($pAssetMultiplier != $gNull and $pAssetMultiplier != '') or (contains($pExtSQLFilterNames, 'MEFF_SPECIAL'))">
        <column name="CONTRACTMULTIPLIER" datakey="false" datakeyupd="true" datatype="decimal">
          <xsl:call-template name="SelectOrFilterColumn">
            <xsl:with-param name="pNames" select="concat($pExtSQLFilterNames,',','CONTRACTMULTIPLIERASSET')"/>
            <xsl:with-param name="pValues" select="concat($pExtSQLFilterValues,',','KEY_OP')"/>
            <xsl:with-param name="pDefaultValue" select="$pAssetMultiplier"/>
            <xsl:with-param name="pResultField" select="'CONTRACTMULTIPLIER'"/>
          </xsl:call-template>
        </column>
      </xsl:if>
      <!-- PL 20130221 Add FACTOR -->
      <xsl:if test="($pAssetFactor != $gNull and $pAssetFactor != '') or (contains($pExtSQLFilterNames, 'MEFF_SPECIAL'))">
        <column name="FACTOR" datakey="false" datakeyupd="true" datatype="decimal">
          <xsl:call-template name="SelectOrFilterColumn">
            <xsl:with-param name="pNames" select="concat($pExtSQLFilterNames,',','CONTRACTMULTIPLIERASSET')"/>
            <xsl:with-param name="pValues" select="concat($pExtSQLFilterValues,',','KEY_OP')"/>
            <xsl:with-param name="pDefaultValue" select="$pAssetFactor"/>
            <xsl:with-param name="pResultField" select="'CONTRACTMULTIPLIER'"/>
          </xsl:call-template>
        </column>
      </xsl:if>

      <column name="ISSECURITYSTATUS" datakey="false" datakeyupd="false" datatype="int">
        <xsl:text>1</xsl:text>
        <!--controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="IsUpdate()"/>
          </control>
        </controls-->
      </column>
      <column name="STRIKEPRICE" datakey="true" datakeyupd="false" datatype="decimal">
        <!--RD 20111110-->
        <!--Pour un future, insérer null quelque soit la valeur du fichier source-->
        <xsl:call-template name="GetCategoryValue">
          <xsl:with-param name="pValue" select="$pAssetStrikePrice"/>
          <xsl:with-param name="pCategory" select="$pCategory"/>
        </xsl:call-template>
      </column>
      <column name="PUTCALL" datakey="true" datakeyupd="false" datatype="string">
        <!--RD 20111110-->
        <!--Pour un future, insérer null quelque soit la valeur du fichier source-->
        <xsl:call-template name="GetCategoryValue">
          <xsl:with-param name="pValue" select="$pAssetPutCall"/>
          <xsl:with-param name="pCategory" select="$pCategory"/>
        </xsl:call-template>
      </column>
      <column name="FIRSTQUOTATIONDAY" datakey="false" datakeyupd="true" datatype="date"  dataformat="yyyyMMdd">
        <xsl:value-of select="$pAssetFirstQuotationDay"/>
        <!--controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="false" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:value-of select="$pAssetFirstQuotationDay"/>
        </default-->
      </column>
      <xsl:call-template name="SysIns">
        <xsl:with-param name="pIsWithControl">
          <xsl:copy-of select="$gFalse"/>
        </xsl:with-param>
      </xsl:call-template>
    </table>

  </xsl:template>
  <!-- SQLTableASSET_ETD -->
  <xsl:template name="SQLTableASSET_ETD">
    <xsl:param name="pExchangeSymbol"/>
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pMaturityMonthYear"/>
    <xsl:param name="pAssetIdentifier"/>
    <xsl:param name="pAssetDisplayName"/>
    <xsl:param name="pAssetSymbol"/>
    <xsl:param name="pAssetISINCode"/>
    <xsl:param name="pAIICode"/>
    <xsl:param name="pAssetMultiplier"/>
    <xsl:param name="pAssetFactor"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pAssetPutCall"/>
    <xsl:param name="pAssetStrikePrice"/>
    <xsl:param name="pAssetFirstQuotationDay"/>
    <xsl:param name="pContractAttribute" select="$gNull"/>
    <!-- BD 20130521 Nouveau param: pUpdateOnly -->
    <xsl:param name="pUpdateOnly" select="$gFalse"/>

    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>

    <!-- Param variables-->
    <xsl:variable name="vParamExchangeSymbol">
      <Param name="EXCHANGESYMBOL" datatype="string">
        <xsl:value-of select="$pExchangeSymbol"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamContractSymbol">
      <Param name="CONTRACTSYMBOL" datatype="string">
        <xsl:value-of select="$pContractSymbol"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamAssetIdentifier">
      <Param name="IDENTIFIER" datatype="string">
        <xsl:value-of select="$pAssetIdentifier"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamMaturityMonthYear">
      <Param name="MATURITYMONTHYEAR" datatype="string">
        <xsl:value-of select="$pMaturityMonthYear"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamAssetPutCall">
      <Param name="PUTCALL" datatype="string">
        <xsl:value-of select="$pAssetPutCall"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamAssetStrikePrice">
      <Param name="STRIKEPRICE" datatype="decimal">
        <xsl:value-of select="$pAssetStrikePrice"/>
      </Param>
    </xsl:variable>

    <!-- SQL variables-->
    <xsl:variable name="vSQLAssetIdentifier">
      <xsl:call-template name="SQLAssetIdentifierExistsAndRowChanged">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'IDENTIFIER'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamMaturityMonthYear">
          <xsl:copy-of select="$vParamMaturityMonthYear"/>
        </xsl:with-param>
        <xsl:with-param name="pParamStrikePrice">
          <xsl:copy-of select="$vParamAssetStrikePrice"/>
        </xsl:with-param>
        <xsl:with-param name="pParamPutCall">
          <xsl:copy-of select="$vParamAssetPutCall"/>
        </xsl:with-param>
        <xsl:with-param name="pContractAttribute">
          <xsl:copy-of select="$pContractAttribute"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetDisplayName">
          <xsl:copy-of select="$pAssetDisplayName"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetISINCode">
          <xsl:copy-of select="$pAssetISINCode"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetSymbol">
          <xsl:copy-of select="$pAssetSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetFirstQuotationDay">
          <xsl:copy-of select="$pAssetFirstQuotationDay"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
      <!--xsl:call-template name="SQLAssetIdentifier">
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamMaturityMonthYear">
          <xsl:copy-of select="$vParamMaturityMonthYear"/>
        </xsl:with-param>
        <xsl:with-param name="pParamStrikePrice">
          <xsl:copy-of select="$vParamAssetStrikePrice"/>
        </xsl:with-param>
        <xsl:with-param name="pParamPutCall">
          <xsl:copy-of select="$vParamAssetPutCall"/>
        </xsl:with-param>
        <xsl:with-param name="pContractAttribute">
          <xsl:copy-of select="$pContractAttribute"/>
        </xsl:with-param>
      </xsl:call-template-->
    </xsl:variable>
    <xsl:variable name="vSQLAssetIdentifierExistsAndRowChanged">
      <xsl:call-template name="SQLAssetIdentifierExistsAndRowChanged">
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamMaturityMonthYear">
          <xsl:copy-of select="$vParamMaturityMonthYear"/>
        </xsl:with-param>
        <xsl:with-param name="pParamStrikePrice">
          <xsl:copy-of select="$vParamAssetStrikePrice"/>
        </xsl:with-param>
        <xsl:with-param name="pParamPutCall">
          <xsl:copy-of select="$vParamAssetPutCall"/>
        </xsl:with-param>
        <xsl:with-param name="pContractAttribute">
          <xsl:copy-of select="$pContractAttribute"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetDisplayName">
          <xsl:copy-of select="$pAssetDisplayName"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetISINCode">
          <xsl:copy-of select="$pAssetISINCode"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetSymbol">
          <xsl:copy-of select="$pAssetSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetFirstQuotationDay">
          <xsl:copy-of select="$pAssetFirstQuotationDay"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLAssetDisplayname">
      <xsl:call-template name="SQLAssetIdentifierExistsAndRowChanged">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'DISPLAYNAME'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamMaturityMonthYear">
          <xsl:copy-of select="$vParamMaturityMonthYear"/>
        </xsl:with-param>
        <xsl:with-param name="pParamStrikePrice">
          <xsl:copy-of select="$vParamAssetStrikePrice"/>
        </xsl:with-param>
        <xsl:with-param name="pParamPutCall">
          <xsl:copy-of select="$vParamAssetPutCall"/>
        </xsl:with-param>
        <xsl:with-param name="pContractAttribute">
          <xsl:copy-of select="$pContractAttribute"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetDisplayName">
          <xsl:copy-of select="$pAssetDisplayName"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetISINCode">
          <xsl:copy-of select="$pAssetISINCode"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetSymbol">
          <xsl:copy-of select="$pAssetSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetFirstQuotationDay">
          <xsl:copy-of select="$pAssetFirstQuotationDay"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLAssetAIICode">
      <xsl:call-template name="SQLAssetIdentifierExistsAndRowChanged">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'AII'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamMaturityMonthYear">
          <xsl:copy-of select="$vParamMaturityMonthYear"/>
        </xsl:with-param>
        <xsl:with-param name="pParamStrikePrice">
          <xsl:copy-of select="$vParamAssetStrikePrice"/>
        </xsl:with-param>
        <xsl:with-param name="pParamPutCall">
          <xsl:copy-of select="$vParamAssetPutCall"/>
        </xsl:with-param>
        <xsl:with-param name="pContractAttribute">
          <xsl:copy-of select="$pContractAttribute"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetDisplayName">
          <xsl:copy-of select="$pAssetDisplayName"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetISINCode">
          <xsl:copy-of select="$pAssetISINCode"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetSymbol">
          <xsl:copy-of select="$pAssetSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetFirstQuotationDay">
          <xsl:copy-of select="$pAssetFirstQuotationDay"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLAssetISINCode">
      <xsl:call-template name="SQLAssetIdentifierExistsAndRowChanged">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'ISINCODE'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamMaturityMonthYear">
          <xsl:copy-of select="$vParamMaturityMonthYear"/>
        </xsl:with-param>
        <xsl:with-param name="pParamStrikePrice">
          <xsl:copy-of select="$vParamAssetStrikePrice"/>
        </xsl:with-param>
        <xsl:with-param name="pParamPutCall">
          <xsl:copy-of select="$vParamAssetPutCall"/>
        </xsl:with-param>
        <xsl:with-param name="pContractAttribute">
          <xsl:copy-of select="$pContractAttribute"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetDisplayName">
          <xsl:copy-of select="$pAssetDisplayName"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetISINCode">
          <xsl:copy-of select="$pAssetISINCode"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetSymbol">
          <xsl:copy-of select="$pAssetSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetFirstQuotationDay">
          <xsl:copy-of select="$pAssetFirstQuotationDay"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLAssetSymbol">
      <xsl:call-template name="SQLAssetIdentifierExistsAndRowChanged">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'ASSETSYMBOL'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamMaturityMonthYear">
          <xsl:copy-of select="$vParamMaturityMonthYear"/>
        </xsl:with-param>
        <xsl:with-param name="pParamStrikePrice">
          <xsl:copy-of select="$vParamAssetStrikePrice"/>
        </xsl:with-param>
        <xsl:with-param name="pParamPutCall">
          <xsl:copy-of select="$vParamAssetPutCall"/>
        </xsl:with-param>
        <xsl:with-param name="pContractAttribute">
          <xsl:copy-of select="$pContractAttribute"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetDisplayName">
          <xsl:copy-of select="$pAssetDisplayName"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetISINCode">
          <xsl:copy-of select="$pAssetISINCode"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetSymbol">
          <xsl:copy-of select="$pAssetSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pAssetFirstQuotationDay">
          <xsl:copy-of select="$pAssetFirstQuotationDay"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLIdDerivativeAttrib">
      <xsl:call-template name="SQLIdDerivativeAttrib">
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamMaturityMonthYear">
          <xsl:copy-of select="$vParamMaturityMonthYear"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLIsAutoSettingAsset">
      <xsl:call-template name="SQLDerivativeContractAfter">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'ISAUTOSETTINGASSET'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>

    <!--	*******************************************************************
						Table ASSET_ETD
		    *******************************************************************	-->
    <table name="ASSET_ETD" action="U" sequenceno="7">
      <!--DERIVATIVECONTRACT Exists and ISAUTOSETTINGASSET-->
      <column name="ISAUTOSETTINGASSET" datakey="false" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLIsAutoSettingAsset"/>
        <controls>
          <control action="RejectRow" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            true
          </control>
        </controls>
      </column>
      <column name="CHECKIDENTIFIEREXITSANDROWCHANGED" datakey="false" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLAssetIdentifierExistsAndRowChanged"/>
        <controls>
          <control action="RejectRow" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            true
          </control>
        </controls>
      </column>
      <column name="IDENTIFIER" datakey="true" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLAssetIdentifier"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            true
          </control>
        </controls>
      </column>
      <!-- RD 20100402 / IDDERIVATIVEATTRIB is DataKey-->
      <column name="IDDERIVATIVEATTRIB" datakey="true" datakeyupd="false" datatype="int">
        <xsl:copy-of select="$vSQLIdDerivativeAttrib"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Incorrect Derivative Attribut.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The Derivative Attribut is not found for Maturity &lt;b&gt;<xsl:value-of select="$pMaturityMonthYear"/>&lt;/b&gt; of Contract &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt; on Market &lt;b&gt;<xsl:value-of select="$pExchangeSymbol"/>&lt;/b&gt;.
                &lt;b&gt;Action:&lt;/b&gt; Create the corresponding Derivative Attribut and restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>
      <column name="DISPLAYNAME" datakey="false" datakeyupd="true" datatype="string">
        <xsl:copy-of select="$vSQLAssetDisplayname"/>
        <controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="false" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:value-of select="$pAssetDisplayName"/>
        </default>
      </column>
      <!--column name="DESCRIPTION" datakey="false" datakeyupd="false" datatype="string">
        null
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="IsUpdate()"/>
          </control>
        </controls>
      </column-->
      <column name="ISINCODE" datakey="false" datakeyupd="true" datatype="string">
        <!-- BD 20130522 Quand pAssetISINCode est renseigné, on update ASSET_ETD avec -->
        <xsl:choose>
          <xsl:when test="string-length($pAssetISINCode)>0">
            <xsl:value-of select="$pAssetISINCode"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:copy-of select="$vSQLAssetISINCode"/>
            <controls>
              <control action="ApplyDefault" return="true" logtype="None">
                <SpheresLib function="ISNULL()"/>
              </control>
              <control action="RejectColumn" return="false" logtype="None">
                <SpheresLib function="ISNULL()"/>
              </control>
            </controls>
            <default>
              <xsl:value-of select="$pAssetISINCode"/>
            </default>
          </xsl:otherwise>
        </xsl:choose>
      </column>
      <column name="AII" datakey="false" datakeyupd="true" datatype="string">
        <xsl:copy-of select="$vSQLAssetAIICode"/>
        <controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="false" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:value-of select="$pAIICode"/>
        </default>
      </column>

      <column name="CFICODE" datakey="false" datakeyupd="true" datatype="string">

        <xsl:call-template name="SelectOrFilterColumn">
          <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
          <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
          <xsl:with-param name="pDefaultValue" select="$gNull"/>
          <xsl:with-param name="pResultField" select="'CFICODE'"/>
        </xsl:call-template>

      </column>

      <column name="ASSETSYMBOL" datakey="false" datakeyupd="true" datatype="string">
        <!-- BD 20130522 Quand pAssetSymbol est renseigné, on update ASSET_ETD avec -->
        <xsl:choose>
          <xsl:when test="string-length($pAssetSymbol)>0">
            <xsl:value-of select="$pAssetSymbol"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:copy-of select="$vSQLAssetSymbol"/>
            <controls>
              <control action="ApplyDefault" return="true" logtype="None">
                <SpheresLib function="ISNULL()"/>
              </control>
              <control action="RejectColumn" return="false" logtype="None">
                <SpheresLib function="ISNULL()"/>
              </control>
            </controls>
            <default>
              <xsl:value-of select="$pAssetSymbol"/>
            </default>
          </xsl:otherwise>
        </xsl:choose>
      </column>
      <!--column name="ISOPENCLOSESETTLT" datakey="false" datakeyupd="false" datatype="int">
        0
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="IsUpdate()"/>
          </control>
        </controls>
      </column-->

      <!-- PL 20130221 Add CONTRACTMULTIPLIER and FACTOR -->
      <xsl:if test="($pAssetMultiplier != $gNull and $pAssetMultiplier != '') or (contains($pExtSQLFilterNames, 'MEFF_SPECIAL'))">
        <column name="CONTRACTMULTIPLIER" datakey="false" datakeyupd="true" datatype="decimal">
          <xsl:call-template name="SelectOrFilterColumn">
            <xsl:with-param name="pNames" select="concat($pExtSQLFilterNames,',','CONTRACTMULTIPLIERASSET')"/>
            <xsl:with-param name="pValues" select="concat($pExtSQLFilterValues,',','KEY_OP')"/>
            <xsl:with-param name="pDefaultValue" select="$pAssetMultiplier"/>
            <xsl:with-param name="pResultField" select="'CONTRACTMULTIPLIER'"/>
          </xsl:call-template>
        </column>
      </xsl:if>
      <xsl:if test="($pAssetFactor != $gNull and $pAssetFactor != '') or (contains($pExtSQLFilterNames, 'MEFF_SPECIAL'))">
        <column name="FACTOR" datakey="false" datakeyupd="true" datatype="decimal">
          <xsl:call-template name="SelectOrFilterColumn">
            <xsl:with-param name="pNames" select="concat($pExtSQLFilterNames,',','CONTRACTMULTIPLIERASSET')"/>
            <xsl:with-param name="pValues" select="concat($pExtSQLFilterValues,',','KEY_OP')"/>
            <xsl:with-param name="pDefaultValue" select="$pAssetFactor"/>
            <xsl:with-param name="pResultField" select="'CONTRACTMULTIPLIER'"/>
          </xsl:call-template>
        </column>
      </xsl:if>

      <!--column name="ISSECURITYSTATUS" datakey="false" datakeyupd="false" datatype="int">
        1
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="IsUpdate()"/>
          </control>
        </controls>
      </column-->
      <!--column name="STRIKEPRICE" datakey="true" datakeyupd="false" datatype="decimal">
        <xsl:value-of select="$pAssetStrikePrice"/>
      </column-->
      <!--column name="PUTCALL" datakey="true" datakeyupd="false" datatype="string">
        <xsl:value-of select="$pAssetPutCall"/>
      </column-->
      <column name="FIRSTQUOTATIONDAY" datakey="false" datakeyupd="true" datatype="date"  dataformat="yyyyMMdd">
        <xsl:value-of select="$pAssetFirstQuotationDay"/>
        <controls>
          <!--control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control-->
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <!--default>
          <xsl:value-of select="$pAssetFirstQuotationDay"/>
        </default-->
      </column>
      <xsl:call-template name="SysUpd">
        <xsl:with-param name="pIsWithControl">
          <xsl:copy-of select="$gFalse"/>
        </xsl:with-param>
      </xsl:call-template>
    </table>

    <xsl:if test="$pUpdateOnly = $gFalse">
      <table name="ASSET_ETD" action="I" sequenceno="8">
        <!--DERIVATIVECONTRACT Exists and ISAUTOSETTINGASSET-->
        <column name="ISAUTOSETTINGASSET" datakey="false" datakeyupd="false" datatype="string">
          <xsl:copy-of select="$vSQLIsAutoSettingAsset"/>
          <controls>
            <control action="RejectRow" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
            <control action="RejectColumn" return="true" logtype="None">
              true
            </control>
          </controls>
        </column>
        <column name="IDENTIFIER" datakey="true" datakeyupd="false" datatype="string">
          <xsl:copy-of select="$vSQLAssetIdentifier"/>
          <controls>
            <control action="RejectRow" return="false" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
            <control action="ApplyDefault" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
          <default>
            <xsl:value-of select="$pAssetIdentifier"/>
          </default>
        </column>
        <!-- RD 20100402 / IDDERIVATIVEATTRIB is DataKey-->
        <column name="IDDERIVATIVEATTRIB" datakey="true" datakeyupd="false" datatype="int">
          <xsl:copy-of select="$vSQLIdDerivativeAttrib"/>
          <controls>
            <control action="RejectRow" return="true">
              <SpheresLib function="ISNULL()"/>
              <logInfo status="REJECT" isexception="true">
                <message>
                  &lt;b&gt;Incorrect Derivative Attribut.&lt;/b&gt;
                  &lt;b&gt;Cause:&lt;/b&gt; The Derivative Attribut is not found for Maturity &lt;b&gt;<xsl:value-of select="$pMaturityMonthYear"/>&lt;/b&gt; of Contract &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt; on Market &lt;b&gt;<xsl:value-of select="$pExchangeSymbol"/>&lt;/b&gt;.
                  &lt;b&gt;Action:&lt;/b&gt; Create the corresponding Derivative Attribut and restart the import.
                  <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
                </message>
              </logInfo>
            </control>
          </controls>
        </column>
        <column name="DISPLAYNAME" datakey="false" datakeyupd="true" datatype="string">
          <xsl:value-of select="$pAssetDisplayName"/>
          <!--controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:value-of select="$pAssetDisplayName"/>
        </default-->
        </column>
        <!--column name="DESCRIPTION" datakey="false" datakeyupd="false" datatype="string">
        null
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="IsUpdate()"/>
          </control>
        </controls>
      </column-->
        <column name="ISINCODE" datakey="false" datakeyupd="true" datatype="string">
          <xsl:value-of select="$pAssetISINCode"/>
          <!--controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="false" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:value-of select="$pAssetISINCode"/>
        </default-->
        </column>
        <column name="AII" datakey="false" datakeyupd="true" datatype="string">
          <xsl:value-of select="$pAIICode"/>
        </column>

        <column name="CFICODE" datakey="false" datakeyupd="true" datatype="string">
          <xsl:call-template name="SelectOrFilterColumn">
            <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
            <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
            <xsl:with-param name="pDefaultValue" select="$gNull"/>
            <xsl:with-param name="pResultField" select="'CFICODE'"/>
          </xsl:call-template>
        </column>

        <column name="ASSETSYMBOL" datakey="false" datakeyupd="true" datatype="string">
          <xsl:value-of select="$pAssetSymbol"/>
          <!--controls>
          <control action="ApplyDefault" return="true" logtype="None">
					<SpheresLib function="ISNULL()"/>
				</control>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <<default>
				<xsl:value-of select="$pAssetSymbol"/>
			</default>-->
        </column>
        <column name="ISOPENCLOSESETTLT" datakey="false" datakeyupd="false" datatype="int">
          0
          <!--controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="IsUpdate()"/>
          </control>
        </controls-->
        </column>

        <xsl:if test="($pAssetMultiplier != $gNull and $pAssetMultiplier != '') or (contains($pExtSQLFilterNames, 'MEFF_SPECIAL'))">
          <column name="CONTRACTMULTIPLIER" datakey="false" datakeyupd="true" datatype="decimal">
            <xsl:call-template name="SelectOrFilterColumn">
              <xsl:with-param name="pNames" select="concat($pExtSQLFilterNames,',','CONTRACTMULTIPLIERASSET')"/>
              <xsl:with-param name="pValues" select="concat($pExtSQLFilterValues,',','KEY_OP')"/>
              <xsl:with-param name="pDefaultValue" select="$pAssetMultiplier"/>
              <xsl:with-param name="pResultField" select="'CONTRACTMULTIPLIER'"/>
            </xsl:call-template>
          </column>
        </xsl:if>
        <!-- PL 20130221 Add FACTOR -->
        <xsl:if test="($pAssetFactor != $gNull and $pAssetFactor != '') or (contains($pExtSQLFilterNames, 'MEFF_SPECIAL'))">
          <column name="FACTOR" datakey="false" datakeyupd="true" datatype="decimal">
            <xsl:call-template name="SelectOrFilterColumn">
              <xsl:with-param name="pNames" select="concat($pExtSQLFilterNames,',','CONTRACTMULTIPLIERASSET')"/>
              <xsl:with-param name="pValues" select="concat($pExtSQLFilterValues,',','KEY_OP')"/>
              <xsl:with-param name="pDefaultValue" select="$pAssetFactor"/>
              <xsl:with-param name="pResultField" select="'CONTRACTMULTIPLIER'"/>
            </xsl:call-template>
          </column>
        </xsl:if>

        <column name="ISSECURITYSTATUS" datakey="false" datakeyupd="false" datatype="int">
          1
          <!--controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="IsUpdate()"/>
          </control>
        </controls-->
        </column>
        <column name="STRIKEPRICE" datakey="true" datakeyupd="false" datatype="decimal">
          <!--RD 20111110-->
          <!--Pour un future, insérer null quelque soit la valeur du fichier source-->
          <xsl:call-template name="GetCategoryValue">
            <xsl:with-param name="pValue" select="$pAssetStrikePrice"/>
            <xsl:with-param name="pCategory" select="$pCategory"/>
          </xsl:call-template>
        </column>
        <column name="PUTCALL" datakey="true" datakeyupd="false" datatype="string">
          <!--RD 20111110-->
          <!--Pour un future, insérer null quelque soit la valeur du fichier source-->
          <xsl:call-template name="GetCategoryValue">
            <xsl:with-param name="pValue" select="$pAssetPutCall"/>
            <xsl:with-param name="pCategory" select="$pCategory"/>
          </xsl:call-template>
        </column>
        <column name="FIRSTQUOTATIONDAY" datakey="false" datakeyupd="true" datatype="date"  dataformat="yyyyMMdd">
          <xsl:value-of select="$pAssetFirstQuotationDay"/>
          <!--controls>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="false" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:value-of select="$pAssetFirstQuotationDay"/>
        </default-->
        </column>
        <xsl:call-template name="SysIns">
          <xsl:with-param name="pIsWithControl">
            <xsl:copy-of select="$gFalse"/>
          </xsl:with-param>
        </xsl:call-template>
      </table>
    </xsl:if>
  </xsl:template>

  <!-- SQLTableUpdateDERIVATIVEATTRIB -->
  <xsl:template name="SQLIDDCUnderlyingWithoutContractSymbol">
    <xsl:param name="pParamUnderlyingAssetSymbol"/>

    <!-- PL 20130215 Add cache="true" -->
    <SQL command="select" result="IDDC" cache="true">
      select dc.IDDC
      from dbo.ASSET_ETD asset
      inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB=asset.IDDERIVATIVEATTRIB)
      inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC=da.IDDC)
      where(asset.ASSETSYMBOL=@UNDERLYINGASSETSYMBOL)
      <xsl:call-template name="SQLDTENABLEDDTDISABLED">
        <xsl:with-param name="pTable" select="'dc'"/>
      </xsl:call-template>
      <xsl:copy-of select="$pParamUnderlyingAssetSymbol"/>
      <Param name="DT" datatype="date">
        <xsl:value-of select="$gParamDtBusiness"/>
      </Param>
    </SQL>

  </xsl:template>

  <xsl:template name="SQLTableUpdateDERIVATIVEATTRIB_DERIVATIVECONTRACT">
    <xsl:param name="pExchangeSymbol"/>
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pMaturityMonthYear"/>
    <xsl:param name="pUnderlyingAssetSymbol"/>

    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>

    <xsl:variable name="vParamExchangeSymbol">
      <Param name="EXCHANGESYMBOL" datatype="string">
        <xsl:value-of select="$pExchangeSymbol"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamContractSymbol">
      <Param name="CONTRACTSYMBOL" datatype="string">
        <xsl:value-of select="$pContractSymbol"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamMaturityMonthYear">
      <Param name="MATURITYMONTHYEAR" datatype="string">
        <xsl:value-of select="$pMaturityMonthYear"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamUnderlyingAssetSymbol">
      <Param name="UNDERLYINGASSETSYMBOL" datatype="string">
        <xsl:value-of select="$pUnderlyingAssetSymbol"/>
      </Param>
    </xsl:variable>


    <xsl:variable name="vSQLDerivativeContractIdentifier">
      <xsl:call-template name="SQLDerivativeContractAfter">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'IDENTIFIER'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vSQLIdAssetUnderlyingWithoutContractSymbol">
      <xsl:call-template name="SQLIdAssetUnderlyingWithoutContractSymbol">
        <xsl:with-param name="pParamUnderlyingAssetSymbol">
          <xsl:copy-of select="$vParamUnderlyingAssetSymbol"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vSQLIDDCUnderlyingWithoutContractSymbol">
      <xsl:call-template name="SQLIDDCUnderlyingWithoutContractSymbol">
        <xsl:with-param name="pParamUnderlyingAssetSymbol">
          <xsl:copy-of select="$vParamUnderlyingAssetSymbol"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vSQLIdMaturity">
      <xsl:call-template name="SQLIdMaturity">
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamMaturityMonthYear">
          <xsl:copy-of select="$vParamMaturityMonthYear"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLIsAutoSettingAsset">
      <xsl:call-template name="SQLDerivativeContractAfter">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'ISAUTOSETTINGASSET'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSQLIdMarket">
      <xsl:call-template name="SQLIdMarket">
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <!-- RD 20100603 / Load IDDC from DERIVATIVECONTRACT table-->
    <xsl:variable name="vSQLIdDerivativeContract">
      <xsl:call-template name="SQLDerivativeContractAfter">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'IDDC'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>

    <!--*******************************************************************
						Table DERIVATIVEATTRIB
				*******************************************************************-->
    <table name="DERIVATIVEATTRIB" action="U" sequenceno="6">
      <!--DERIVATIVECONTRACT Exists and ISAUTOSETTINGASSET-->
      <column name="ISAUTOSETTINGASSET" datakey="false" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLIsAutoSettingAsset"/>
        <controls>
          <control action="RejectRow" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            true
          </control>
        </controls>
      </column>
      <column name="IDMATURITY" datakey="true" datakeyupd="false" datatype="int">
        <xsl:copy-of select="$vSQLIdMaturity"/>
        <controls>
          <control action="RejectRow" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="IsUpdate()"/>
          </control>
        </controls>
      </column>
      <!-- RD 20100402 / IDDC is DataKey-->
      <column name="IDDC" datakey="true" datakeyupd="false" datatype="int">
        <xsl:copy-of select="$vSQLIdDerivativeContract"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Incorrect Derivative Contract.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The Derivative Contract is not found for Contract &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt; on Market &lt;b&gt;<xsl:value-of select="$pExchangeSymbol"/>&lt;/b&gt;.
                &lt;b&gt;Action:&lt;/b&gt; Create the Derivative Contract &lt;b&gt;<xsl:value-of select="$pContractSymbol"/>&lt;/b&gt; or correct the input file, and restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>
      <column name="IDASSET" datakey="false" datakeyupd="true" datatype="int">
        <xsl:copy-of select="$vSQLIdAssetUnderlyingWithoutContractSymbol"/>
      </column>
      <xsl:call-template name="SysUpd"/>
    </table>

    <table name="DERIVATIVECONTRACT" action="U" sequenceno="1">
      <column name="ISAUTOSETTING" datakey="false" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLIsAutoSettingAsset"/>
        <controls>
          <control action="RejectRow" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            true
          </control>
        </controls>
      </column>
      <column name="IDENTIFIER" datakey="true" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLDerivativeContractIdentifier"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            true
          </control>
        </controls>
      </column>
      <!-- RD 20100402 / IDM is DataKey-->
      <column name="IDM" datakey="true" datakeyupd="false" datatype="int">
        <xsl:copy-of select="$vSQLIdMarket"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Incorrect Market.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The Market &lt;b&gt;<xsl:value-of select="$pExchangeSymbol"/>&lt;/b&gt; is not found.
                &lt;b&gt;Action:&lt;/b&gt; Create the Market &lt;b&gt;<xsl:value-of select="$pExchangeSymbol"/>&lt;/b&gt; or correct the input file, and restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>
      <column name="IDDC_UNL" datakey="false" datakeyupd="true" datatype="int">
        <xsl:copy-of select="$vSQLIDDCUnderlyingWithoutContractSymbol"/>
      </column>
      <xsl:call-template name="SysUpd"/>
    </table>

  </xsl:template>

  <!-- SQLTableGCONTRACT -->

  <xsl:template name="SQLSelectGContractIdentifier">
    <xsl:param name="pParamContractGroupIdentifier"/>
    <!-- PL 20130215 Add cache="true" -->
    <SQL command="select" result="IDENTIFIER" cache="true">
      select gc.IDENTIFIER
      from dbo.GCONTRACT gc
      where (gc.IDENTIFIER=@CONTRACTGROUPIDENTIFIER)
      <xsl:copy-of select="$pParamContractGroupIdentifier"/>
    </SQL>
  </xsl:template>

  <xsl:template name="SQLTableGCONTRACT">
    <xsl:param name="pExchangeSymbol"/>
    <xsl:param name="pContractGroupIdentifier"/>
    <xsl:param name="pContractGroupDisplayName"/>
    <xsl:param name="pContractGroupDescription"/>
    <xsl:param name="pContractGroupSymbol"/>

    <xsl:variable name="vParamContractgroupIdentifier">
      <Param name="CONTRACTGROUPIDENTIFIER" datatype="string">
        <xsl:value-of select="$pContractGroupIdentifier"/>
      </Param>
    </xsl:variable>

    <xsl:variable name="vSQLContractGroupIdentifier">
      <xsl:call-template name="SQLSelectGContractIdentifier">
        <xsl:with-param name="pParamContractGroupIdentifier">
          <xsl:copy-of select="$vParamContractgroupIdentifier"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    <table name="GCONTRACT" action="I" sequenceno="1">

      <column name="IDENTIFIER" datakey="true" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLContractGroupIdentifier"/>
        <controls>
          <control action="RejectRow" return="true" logtype="None">
            <SpheresLib function="ISNOTNULL()"/>
          </control>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:value-of select="$pContractGroupIdentifier"/>
        </default>
      </column>

      <column name="DISPLAYNAME" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pContractGroupDisplayName"/>
      </column>

      <xsl:if test="$pContractGroupDescription != '' and $pContractGroupDescription != $gNull">

        <column name="DESCRIPTION" datakey="false" datakeyupd="true" datatype="string">
          <xsl:value-of select="$pContractGroupDescription"/>
        </column>

      </xsl:if>

      <column name="GROUPSYMBOL" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pContractGroupSymbol"/>
      </column>

      <xsl:call-template name="SysIns">
        <xsl:with-param name="pIsWithControl">
          <xsl:copy-of select="$gFalse"/>
        </xsl:with-param>
      </xsl:call-template>

    </table>

  </xsl:template>

  <!-- SQLTableGCONTRACTROLE -->

  <xsl:template name="SQLSelectIDGContractFromGContractIdentifier">
    <xsl:param name="pParamContractGroupIdentifier"/>

    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>

    <!-- PL 20130215 Add cache="true" -->
    <SQL command="select" result="IDGCONTRACT" cache="true">
      select gc.IDGCONTRACT
      from dbo.GCONTRACT gc
      where (gc.IDENTIFIER=@CONTRACTGROUPIDENTIFIER)
      <xsl:call-template name="SelectOrFilterColumn">
        <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
        <xsl:with-param name="pFilterField" select="'gc.IDENTIFIER'"/>
      </xsl:call-template>
      <xsl:copy-of select="$pParamContractGroupIdentifier"/>
    </SQL>
  </xsl:template>

  <xsl:template name="SQLSelectIDRoleType">
    <xsl:param name="pParamContractGroupRoleType"/>
    <xsl:param name="pParamContractGroupIdentifier"/>
    <!-- PL 20130215 Add cache="true" -->
    <SQL command="select" result="IDROLEGCONTRACT" cache="true">
      select gcr.IDROLEGCONTRACT
      from dbo.GCONTRACTROLE gcr
      inner join dbo.GCONTRACT gc on (gc.IDGCONTRACT=gcr.IDGCONTRACT)
      where (rtrim(gcr.IDROLEGCONTRACT)=@ROLETYPE) and (gc.IDENTIFIER=@CONTRACTGROUPIDENTIFIER)
      <xsl:copy-of select="$pParamContractGroupRoleType"/>
      <xsl:copy-of select="$pParamContractGroupIdentifier"/>
    </SQL>
  </xsl:template>

  <xsl:template name="SQLTableGCONTRACTROLE">
    <xsl:param name="pExchangeSymbol"/>
    <xsl:param name="pContractGroupIdentifier"/>
    <xsl:param name="pContractGroupRoleType" select="'MARGIN'"/>

    <xsl:variable name="vParamContractgroupIdentifier">
      <Param name="CONTRACTGROUPIDENTIFIER" datatype="string">
        <xsl:value-of select="$pContractGroupIdentifier"/>
      </Param>
    </xsl:variable>

    <xsl:variable name="vParamContractgroupRoleType">
      <Param name="ROLETYPE" datatype="string">
        <xsl:value-of select="$pContractGroupRoleType"/>
      </Param>
    </xsl:variable>

    <xsl:variable name="vSQLContractGroupID">
      <xsl:call-template name="SQLSelectIDGContractFromGContractIdentifier">
        <xsl:with-param name="pParamContractGroupIdentifier">
          <xsl:copy-of select="$vParamContractgroupIdentifier"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vSQLContractGroupRoleType">
      <xsl:call-template name="SQLSelectIDRoleType">
        <xsl:with-param name="pParamContractGroupRoleType">
          <xsl:copy-of select="$vParamContractgroupRoleType"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractGroupIdentifier">
          <xsl:copy-of select="$vParamContractgroupIdentifier"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    <table name="GCONTRACTROLE" action="I" sequenceno="2">

      <column name="IDROLEGCONTRACT" datakey="true" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLContractGroupRoleType"/>
        <controls>
          <control action="RejectRow" return="true" logtype="None">
            <SpheresLib function="ISNOTNULL()"/>
            <!--<logInfo status="REJECT" isexception="false">
              <message>
                &lt;b&gt;The role has been already inserted.&lt;/b&gt;
                &lt;b&gt;Reason:&lt;/b&gt; The role &lt;b&gt;<xsl:value-of select="$pContractGroupRoleType"/>&lt;/b&gt; is already linked
                to the contract group &lt;b&gt;<xsl:value-of select="$pContractGroupIdentifier"/>&lt;/b&gt;.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text>
                <xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>-->
          </control>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:value-of select="$pContractGroupRoleType"/>
        </default>
      </column>

      <column name="IDGCONTRACT" datakey="true" datakeyupd="false" datatype="int">
        <xsl:copy-of select="$vSQLContractGroupID"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;No related contract.&lt;/b&gt;
                &lt;b&gt;Reason:&lt;/b&gt; The related contract &lt;b&gt;<xsl:value-of select="$pContractGroupIdentifier"/>&lt;/b&gt; has not been found.
                &lt;b&gt;Action:&lt;/b&gt; Correct the input file and start the import again.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>

      <xsl:call-template name="SysIns">
        <xsl:with-param name="pIsWithControl">
          <xsl:copy-of select="$gFalse"/>
        </xsl:with-param>
      </xsl:call-template>

    </table>

  </xsl:template>

  <!-- SQLTableCONTRACTG -->

  <!-- build a stand-alone SQL request to return a column value / queue a filter extention for an already existent SQL request-->
  <xsl:template name="SelectOrFilterColumn">
    <!-- string collection of the sql parameter names-->
    <xsl:param name="pNames"/>
    <!-- string collection of the sql parameter values -->
    <xsl:param name="pValues"/>
    <!-- straight string value to return when no sql request is needed -->
    <xsl:param name="pDefaultValue"/>
    <!-- filter column name (prefixed by table alias name). Default value : ''. 
    If the pFilterField values is empty (default case), then the template will build a stand alone request <SQL></SQL> 
      returning the value of the column specified in the pResultField parameter.
    If the pFilterField values is NOT empty, then the template will build a filter extention for the specified pFilterField column. -->
    <xsl:param name="pFilterField"/>
    <!-- result column name (not prefixed by table alias name)  -->
    <xsl:param name="pResultField"/>
    <!-- flag parameter; If true then we will queue to the SQL request the params collection (<Param/> node)  -->
    <xsl:param name="pParamNodesBuilderEnabled" select="true()"/>

    <xsl:choose>
      <xsl:when test="
                contains($pNames, 'MEFF_SPECIAL') or 
                contains($pNames, 'LIFFE_SPECIAL') or 
                contains($pNames, 'SPAN_SPECIAL') or
                contains($pNames, 'EUREX_SPECIAL')or
                contains($pNames, $gIdemSpecial)">

        <xsl:variable name="vResultFieldTranscoded">
          <xsl:choose>

            <xsl:when test="$pResultField = '' or $pResultField = 'IDENTIFIER'">
              <xsl:choose>

                <xsl:when test="contains($pNames, 'MEFF_SPECIAL')">

                  <xsl:choose>
                    <xsl:when test="contains($pNames, 'PTCNTRGRPSYMBOL')">
                      <xsl:variable name="vContractGroupIDPartial">
                        <xsl:call-template name="ParamNodesBuilder">
                          <xsl:with-param name="pValues" select="$pValues"/>
                          <xsl:with-param name="pNames" select="$pNames"/>
                          <xsl:with-param name="pKeyName" select="'PTCNTRGRPSYMBOL'"/>
                        </xsl:call-template>
                      </xsl:variable>
                      '<xsl:value-of select="$vContractGroupIDPartial"/>' <xsl:value-of select="$gSqlConcat"/> UNDERLYINGMNEMONIC
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:variable name="vPartialID">
                        <xsl:call-template name="ParamNodesBuilder">
                          <xsl:with-param name="pValues" select="$pValues"/>
                          <xsl:with-param name="pNames" select="$pNames"/>
                          <xsl:with-param name="pKeyName" select="'PTIDENTIFIER'"/>
                        </xsl:call-template>
                      </xsl:variable>

                      <xsl:choose>

                        <xsl:when test="$vPartialID = $gAutomaticComputeSQL">
                          <xsl:value-of select="$vPartialID"/>
                        </xsl:when>
                        <xsl:otherwise>
                          '<xsl:value-of select="$vPartialID"/>' <xsl:value-of select="$gSqlConcat"/> DCSUFFIX
                        </xsl:otherwise>

                      </xsl:choose>

                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:when>

                <xsl:when test="contains($pNames, 'LIFFE_SPECIAL')"/>

                <xsl:when test="contains($pNames, 'SPAN_SPECIAL')"/>

                <xsl:when test="contains($pNames, 'EUREX_SPECIAL')"/>

                <xsl:when test="contains($pNames, $gIdemSpecial)"/>

                <xsl:otherwise/>

              </xsl:choose>
            </xsl:when>

            <xsl:when test="$pResultField  = 'DISPLAYNAME'">
              <xsl:choose>

                <xsl:when test="contains($pNames, 'MEFF_SPECIAL')">

                  <xsl:variable name="vPartialDisplayName">
                    <xsl:call-template name="ParamNodesBuilder">
                      <xsl:with-param name="pValues" select="$pValues"/>
                      <xsl:with-param name="pNames" select="$pNames"/>
                      <xsl:with-param name="pKeyName" select="'PTDISPLAYNAME'"/>
                    </xsl:call-template>
                  </xsl:variable>

                  <xsl:choose>

                    <xsl:when test="$vPartialDisplayName = $gAutomaticComputeSQL">
                      <xsl:value-of select="$vPartialDisplayName"/>
                    </xsl:when>
                    <xsl:otherwise>
                      '<xsl:value-of select="$vPartialDisplayName"/>' <xsl:value-of select="$gSqlConcat"/> DCSUFFIX
                    </xsl:otherwise>

                  </xsl:choose>
                </xsl:when>

                <xsl:when test="contains($pNames, 'LIFFE_SPECIAL')"/>

                <xsl:when test="contains($pNames, 'SPAN_SPECIAL')"/>

                <xsl:when test="contains($pNames, 'EUREX_SPECIAL')"/>

                <xsl:when test="contains($pNames, $gIdemSpecial)"/>

                <xsl:otherwise/>

              </xsl:choose>
            </xsl:when>

            <xsl:otherwise>
              <xsl:value-of select="$pResultField"/>
            </xsl:otherwise>

          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="vAliasField">
          <xsl:choose>
            <xsl:when test="$pResultField = ''">
              <xsl:value-of select="'IDENTIFIER'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$pResultField"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="vTempSQL">
          <xsl:choose>

            <xsl:when test="contains($pNames, 'MEFF_SPECIAL')">

              <xsl:choose>
                <xsl:when test="contains($pNames, 'PTCNTRGRPSYMBOL')">
                  select <xsl:value-of select="$vResultFieldTranscoded"/> as
                  <xsl:value-of select="$vAliasField"/>
                  from dbo.MEFFCCONTRGRP
                  where (CONTRACTGROUPCODE=@CONTRACTGROUPCODE and CLEARINGHOUSECODE=@CLEARINGHOUSECODE)
                </xsl:when>
                <xsl:otherwise>
                  <xsl:choose>

                    <xsl:when test="$vResultFieldTranscoded = $gAutomaticComputeSQL"/>
                    <!--FL 20171006 [23495]-->
                    <xsl:when test="$vResultFieldTranscoded = 'PHYSETTLTAMOUNT'"/>
                    <xsl:otherwise>
                      select <xsl:value-of select="$vResultFieldTranscoded"/> as
                      <xsl:value-of select="$vAliasField"/>
                      from dbo.MEFFCCONTRTYP meffct
                      where (meffct.CLEARINGHOUSECODE=@CLEARINGHOUSECODE and meffct.CONTRACTGROUPCODE=@CONTRACTGROUPCODE and
                      <xsl:choose>
                        <xsl:when test="$pResultField = 'CONTRACTMULTIPLIER' and contains($pNames,'CONTRACTMULTIPLIERASSET') != true()">
                          meffct.CONTRACTTYPECODE = @FIRSTCONTRACTTYPECODE
                        </xsl:when>
                        <xsl:otherwise>
                          meffct.CONTRACTTYPECODE = @CONTRACTTYPECODE
                        </xsl:otherwise>
                      </xsl:choose>
                      and meffct.CATEGORY=@CATEGORY)
                    </xsl:otherwise>

                  </xsl:choose>

                </xsl:otherwise>
              </xsl:choose>

            </xsl:when>

            <xsl:when test="contains($pNames, 'LIFFE_SPECIAL')"/>

            <xsl:when test="contains($pNames, 'SPAN_SPECIAL')"/>

            <xsl:when test="contains($pNames, 'EUREX_SPECIAL')"/>

            <xsl:when test="contains($pNames, $gIdemSpecial)"/>

            <xsl:otherwise/>

          </xsl:choose>
        </xsl:variable>

        <xsl:choose>

          <xsl:when test="$pFilterField != ''">
            <xsl:choose>

              <xsl:when test="$vTempSQL != ''">
                or ( <xsl:value-of select="$pFilterField"/> = (<xsl:value-of select="$vTempSQL"/>))
                <xsl:if test="$pParamNodesBuilderEnabled = true()">
                  <xsl:call-template name="ParamNodesBuilder">
                    <xsl:with-param name="pValues" select="$pValues"/>
                    <xsl:with-param name="pNames" select="$pNames"/>
                  </xsl:call-template>
                </xsl:if>
              </xsl:when>

              <xsl:otherwise/>

            </xsl:choose>
          </xsl:when>
          <xsl:otherwise>

            <xsl:choose>

              <xsl:when test="$vTempSQL != ''">
                <SQL command="select">
                  <xsl:attribute name="result">
                    <xsl:value-of select="$vAliasField"/>
                  </xsl:attribute>
                  <xsl:value-of select="$vTempSQL"/>
                  <xsl:if test="$pParamNodesBuilderEnabled = true()">
                    <xsl:call-template name="ParamNodesBuilder">
                      <xsl:with-param name="pValues" select="$pValues"/>
                      <xsl:with-param name="pNames" select="$pNames"/>
                    </xsl:call-template>
                  </xsl:if>
                </SQL>
              </xsl:when>

              <xsl:otherwise>
                <xsl:value-of select="$pDefaultValue"/>
              </xsl:otherwise>

            </xsl:choose>
          </xsl:otherwise>
        </xsl:choose>

      </xsl:when>

      <xsl:otherwise>
        <xsl:value-of select="$pDefaultValue"/>
      </xsl:otherwise>

    </xsl:choose>

  </xsl:template>

  <xsl:template name="BuildPartialContractGroupIdentifier">
    <xsl:param name="pContractGroupSymbol"/>
    <xsl:param name="pContractGroupUnderlying"/>

    <xsl:value-of select="concat($pContractGroupSymbol, '-', $pContractGroupUnderlying)"/>

  </xsl:template>

  <xsl:template name="SQLCheckRelationCGroupDContract">

    <xsl:param name="pParamContractSymbol"/>
    <xsl:param name="pParamExchangeSymbol"/>
    <xsl:param name="pParamContractGroupIdentifier"/>

    <xsl:param name="pContractGroupIdentifier"/>

    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>

    <!-- PL 20130215 Add cache="true" -->
    <SQL command="select" IDENTIFIER="IDGCONTRACT" cache="true">
      select cg.IDGCONTRACT, cg.IDXC, dc.IDENTIFIER, gc.IDENTIFIER
      from dbo.CONTRACTG cg
      inner join dbo.GCONTRACT gc on (gc.IDGCONTRACT=cg.IDGCONTRACT)
      inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC=cg.IDXC) and (dc.CONTRACTSYMBOL=@CONTRACTSYMBOL )
      inner join dbo.MARKET m on m.IDM=dc.IDM and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL)
      where (cg.CONTRACTCATEGORY='DerivativeContract')
      <xsl:call-template name="SQLDTENABLEDDTDISABLED">
        <xsl:with-param name="pTable" select="'dc'"/>
      </xsl:call-template>
      <xsl:call-template name="SQLWhereClauseExtension">
        <xsl:with-param name="pNames" select="concat($pExtSQLFilterNames,',','ALIASTBLDC')"/>
        <xsl:with-param name="pValues" select="concat($pExtSQLFilterValues,',','dc')"/>
        <xsl:with-param name="pParamNodesBuilderEnabled" select="false()"/>
      </xsl:call-template>
      and (gc.IDENTIFIER=@CONTRACTGROUPIDENTIFIER
      <xsl:call-template name="SelectOrFilterColumn">
        <xsl:with-param name="pNames" select="concat($pExtSQLFilterNames,',','PTCNTRGRPSYMBOL')"/>
        <xsl:with-param name="pValues" select="concat($pExtSQLFilterValues,',',$pContractGroupIdentifier)"/>
        <xsl:with-param name="pFilterField" select="'gc.IDENTIFIER'"/>
        <xsl:with-param name="pParamNodesBuilderEnabled" select="false()"/>
      </xsl:call-template>
      )
      <xsl:call-template name="ParamNodesBuilder">
        <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
        <xsl:with-param name="pNames" select="concat($pExtSQLFilterNames,',','PTCNTRGRPSYMBOL')"/>
      </xsl:call-template>
      <xsl:copy-of select="$pParamContractSymbol"/>
      <xsl:copy-of select="$pParamExchangeSymbol"/>
      <xsl:copy-of select="$pParamContractGroupIdentifier"/>
      <Param name="DT" datatype="date">
        <xsl:value-of select="$gParamDtBusiness"/>
      </Param>
    </SQL>
  </xsl:template>

  <xsl:template name ="SQLTableCONTRACTG">
    <xsl:param name="pContractGroupIdentifier"/>
    <!--<xsl:param name="pDerivativeContractIdentifier"/>-->

    <xsl:param name="pExchangeSymbol"/>
    <xsl:param name="pContractSymbol"/>

    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>

    <xsl:variable name="vParamExchangeSymbol">
      <Param name="EXCHANGESYMBOL" datatype="string">
        <xsl:value-of select="$pExchangeSymbol"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamContractSymbol">
      <Param name="CONTRACTSYMBOL" datatype="string">
        <xsl:value-of select="$pContractSymbol"/>
      </Param>
    </xsl:variable>
    <xsl:variable name="vParamContractgroupIdentifier">
      <Param name="CONTRACTGROUPIDENTIFIER" datatype="string">
        <xsl:value-of select="$pContractGroupIdentifier"/>
      </Param>
    </xsl:variable>

    <xsl:variable name="vSQLContractGroupID">
      <xsl:call-template name="SQLSelectIDGContractFromGContractIdentifier">
        <xsl:with-param name="pParamContractGroupIdentifier">
          <xsl:copy-of select="$vParamContractgroupIdentifier"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterValues" select="concat($pExtSQLFilterValues,',',$pContractGroupIdentifier)"/>
        <xsl:with-param name="pExtSQLFilterNames" select="concat($pExtSQLFilterNames,',','PTCNTRGRPSYMBOL')"/>

      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vSQLDerivativeContractIdentifier">
      <xsl:call-template name="SQLDerivativeContractAfter">
        <xsl:with-param name="pResultColumn">
          <xsl:copy-of select="'IDDC'"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>

        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>

      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vSQLCheckGroupContractRelationExists">

      <xsl:call-template name="SQLCheckRelationCGroupDContract">

        <xsl:with-param name="pParamContractSymbol">
          <xsl:copy-of select="$vParamContractSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamExchangeSymbol">
          <xsl:copy-of select="$vParamExchangeSymbol"/>
        </xsl:with-param>
        <xsl:with-param name="pParamContractGroupIdentifier">
          <xsl:copy-of select="$vParamContractgroupIdentifier"/>
        </xsl:with-param>

        <xsl:with-param name="pContractGroupIdentifier" select="$pContractGroupIdentifier"/>

        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>
        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>

      </xsl:call-template>
    </xsl:variable>

    <table name="CONTRACTG" action="I" sequenceno="3">

      <column name="CHECKALREADYEXISTS" datakey="false" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLCheckGroupContractRelationExists"/>
        <controls>
          <control action="RejectRow" return="false" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <!-- FL 20171006 [23495] IDDC becomes IDXCC -->
      <!--<column name="IDDC" datakey="true" datakeyupd="false" datatype="int">-->
      <column name="IDXC" datakey="true" datakeyupd="false" datatype="int">
        <xsl:copy-of select="$vSQLDerivativeContractIdentifier"/>
        <control action="RejectRow" return="true">
          <SpheresLib function="ISNULL()"/>
          <logInfo status="REJECT" isexception="true">
            <message>
              &lt;b&gt;No related contract.&lt;/b&gt;
              &lt;b&gt;Reason:&lt;/b&gt; The related contract has not been found.
              &lt;b&gt;Action:&lt;/b&gt; Correct the input file and start the import again.
              <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
            </message>
          </logInfo>
        </control>
      </column>

      <!-- FL 20171006 [23495] New column CONTRACTCATEGORY -->
      <column name="CONTRACTCATEGORY" datakey="true" datakeyupd="false" datatype="string">
        <xsl:value-of select="'DerivativeContract'"/>
      </column>

      <column name="IDGCONTRACT" datakey="true" datakeyupd="false" datatype="int">
        <xsl:copy-of select="$vSQLContractGroupID"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;No related contract.&lt;/b&gt;
                &lt;b&gt;Reason:&lt;/b&gt; The related contract &lt;b&gt;<xsl:value-of select="$pContractGroupIdentifier"/>&lt;/b&gt; has not been found.
                &lt;b&gt;Action:&lt;/b&gt; Correct the input file and start the import again.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>

      <xsl:call-template name="SysIns">
        <xsl:with-param name="pIsWithControl">
          <xsl:copy-of select="$gFalse"/>
        </xsl:with-param>
      </xsl:call-template>

    </table>

  </xsl:template>

  <xsl:template name ="SQLTableEXCHANGELIST">
    <xsl:param name="pContractGroupSymbol"/>
    <xsl:param name="pExchangeISO10383"/>
    <xsl:param name="pContractSymbol"/>

    <xsl:variable name="vSQLDescription">
      <SQL command="select" result="DESCRIPTION" cache="true">
        <![CDATA[select gc.DISPLAYNAME as DESCRIPTION
              from dbo.GCONTRACT gc
              where (gc.GROUPSYMBOL=@GROUPSYMBOL)]]>
        <Param name="GROUPSYMBOL" datatype="string">
          <xsl:copy-of select="$pContractGroupSymbol"/>
        </Param>
      </SQL>
    </xsl:variable>

    <table name="EXCHANGELIST" action="IU" sequenceno="1">
      <column name="EXCHANGEISO10383" datakey="true" datakeyupd="false" datatype="string">
        <xsl:value-of select="$pExchangeISO10383"/>
      </column>
      <column name="CONTRACTSYMBOL" datakey="true" datakeyupd="false" datatype="string">
        <xsl:value-of select="$pContractSymbol"/>
      </column>
      <column name="CATEGORY" datakey="false" datakeyupd="false" datatype="string">
        <xsl:value-of select="'X'"/>
      </column>
      <column name="DESCRIPTION" datakey="false" datakeyupd="true" datatype="string">
        <xsl:copy-of select="$vSQLDescription"/>
      </column>
      <column name="ORIGINEDATA" datakey="false" datakeyupd="true" datatype="string">
        <xsl:copy-of select="'BMECLEARING'"/>
      </column>
    </table>

  </xsl:template>

  <!-- SQLTableMEFFCCONTRTYP-->

  <xsl:template name="ParamNodesBuilder">
    <xsl:param name="pValues"/>
    <xsl:param name="pNames"/>
    <!-- If pKeyName is != '' then the template will return a single record 
    of the pValues array if pKeyName is found in the pNames array
    -->
    <xsl:param name="pKeyName"/>

    <xsl:choose>
      <xsl:when test="string-length($pNames) > 0">

        <xsl:variable name="vActualName">
          <xsl:choose>
            <xsl:when test="contains($pNames, ',')">
              <xsl:value-of select="substring-before($pNames, ',')"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$pNames"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="vActualValue">
          <xsl:choose>
            <xsl:when test="contains($pValues, ',')">
              <xsl:value-of select="substring-before($pValues, ',')"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$pValues"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="vFollowingValues">
          <xsl:value-of select="substring-after($pValues, ',')"/>
        </xsl:variable>

        <xsl:variable name="vFollowingNames">
          <xsl:value-of select="substring-after($pNames, ',')"/>
        </xsl:variable>

        <xsl:call-template name="ParamNodesBuilder">
          <xsl:with-param name="pValues" select="$vFollowingValues"/>
          <xsl:with-param name="pNames" select="$vFollowingNames"/>
          <xsl:with-param name="pKeyName" select="$pKeyName"/>
        </xsl:call-template>

        <xsl:choose>
          <!-- RD 20100506 Bug Oracle qui n'accèpte pas des paramétres vides-->
          <xsl:when test="$pKeyName = ''  and string-length($vActualName) > 0 and string-length($vActualValue) > 0">

            <Param>
              <xsl:attribute name="name">
                <xsl:value-of select="$vActualName"/>
              </xsl:attribute>
              <xsl:attribute name="datatype">
                <xsl:value-of select="'string'"/>
              </xsl:attribute>
              <xsl:value-of select="$vActualValue"/>
            </Param>

          </xsl:when>

          <xsl:when test="$pKeyName != '' and $vActualName = $pKeyName">

            <xsl:value-of select="$vActualValue"/>

          </xsl:when>

        </xsl:choose>

      </xsl:when>
      <xsl:otherwise/>
    </xsl:choose>

  </xsl:template>

  <xsl:template name="SQLWhereClauseExtension">
    <xsl:param name="pNames"/>
    <xsl:param name="pValues"/>
    <xsl:param name="pParamNodesBuilderEnabled" select="true()"/>
    <xsl:param name="pExtensionCheck" select="true()"/>

    <xsl:if test="
            contains($pNames, 'MEFF_SPECIAL') or 
            contains($pNames, 'LIFFE_SPECIAL') or 
            contains($pNames, 'SPAN_SPECIAL') or 
            contains($pNames, 'EUREX_SPECIAL')or
            contains($pNames, $gIdemSpecial)">

      <xsl:variable name="vAliasField">
        <xsl:choose>
          <xsl:when test="contains($pNames,'ALIASTBLDC')">
            <xsl:call-template name="ParamNodesBuilder">
              <xsl:with-param name="pValues" select="$pValues"/>
              <xsl:with-param name="pNames" select="$pNames"/>
              <xsl:with-param name="pKeyName" select="'ALIASTBLDC'"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <!-- PL 20130213 replace de 'c' par 'dc' -->
            <xsl:value-of select="'dc'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>


      <xsl:choose>
        <xsl:when test="
            contains($pNames, 'MEFF_SPECIAL') and
            contains($pNames,'CLEARINGHOUSECODE') and 
            contains($pNames, 'CONTRACTGROUPCODE') and 
            contains($pNames, 'CONTRACTTYPECODE')">
          and
          exists (select 1 from dbo.MEFFCCONTRTYP meffct
          where (
          meffct.CLEARINGHOUSECODE=@CLEARINGHOUSECODE and meffct.CONTRACTGROUPCODE=@CONTRACTGROUPCODE and
          meffct.CONTRACTTYPECODE=@CONTRACTTYPECODE and meffct.CATEGORY = <xsl:value-of select="$vAliasField"/>.CATEGORY and
          meffct.SETTLTMETHOD = <xsl:value-of select="$vAliasField"/>.SETTLTMETHOD
          and
          (
          <xsl:value-of select="$vAliasField"/>.CATEGORY = 'F'
          or (<xsl:value-of select="$vAliasField"/>.CATEGORY = 'O' and meffct.EXERCISESTYLE = <xsl:value-of select="$vAliasField"/>.EXERCISESTYLE)
          )))
          <xsl:if test="$pParamNodesBuilderEnabled">
            <xsl:call-template name="ParamNodesBuilder">
              <xsl:with-param name="pValues" select="$pValues"/>
              <xsl:with-param name="pNames" select="$pNames"/>
            </xsl:call-template>
          </xsl:if>
        </xsl:when>

        <xsl:when test="
                  contains($pNames, 'LIFFE_SPECIAL') and
                  contains($pNames, 'CATEGORY')">

          and <xsl:value-of select="$vAliasField"/>.CATEGORY = @CATEGORY
          <xsl:if test="$pParamNodesBuilderEnabled">
            <xsl:call-template name="ParamNodesBuilder">
              <xsl:with-param name="pValues" select="$pValues"/>
              <xsl:with-param name="pNames" select="$pNames"/>
            </xsl:call-template>
          </xsl:if>
        </xsl:when>

        <xsl:when test="
                  contains($pNames, 'SPAN_SPECIAL') and
                  contains($pNames, 'CATEGORY')">

          and <xsl:value-of select="$vAliasField"/>.CATEGORY = @CATEGORY
          <xsl:if test="$pParamNodesBuilderEnabled">
            <xsl:call-template name="ParamNodesBuilder">
              <xsl:with-param name="pValues" select="$pValues"/>
              <xsl:with-param name="pNames" select="$pNames"/>
            </xsl:call-template>
          </xsl:if>
        </xsl:when>

        <!-- IDEM SPECIAL -->
        <xsl:when test="contains($pNames, $gIdemSpecial) and contains($pNames, 'CATEGORY')">
          and <xsl:value-of select="$vAliasField"/>.CATEGORY = @CATEGORY
          <xsl:if test="$pParamNodesBuilderEnabled">
            <xsl:call-template name="ParamNodesBuilder">
              <xsl:with-param name="pValues" select="$pValues"/>
              <xsl:with-param name="pNames" select="$pNames"/>
            </xsl:call-template>
          </xsl:if>
        </xsl:when>

        <xsl:when test="
                  contains($pNames, 'EUREX_SPECIAL') and
                  contains($pNames, 'CONTRACTATTRIBUTEEUREX') and
                  $pExtensionCheck = true()">

          and <xsl:value-of select="$vAliasField"/>.CONTRACTATTRIBUTE = @CONTRACTATTRIBUTEEUREX
          <xsl:if test="$pParamNodesBuilderEnabled">
            <xsl:call-template name="ParamNodesBuilder">
              <xsl:with-param name="pValues" select="$pValues"/>
              <xsl:with-param name="pNames" select="$pNames"/>
            </xsl:call-template>
          </xsl:if>
        </xsl:when>

        <xsl:otherwise/>

      </xsl:choose>

    </xsl:if>

  </xsl:template>

  <xsl:template name="SQLMEFFSelectContratType">
    <xsl:param name="pResult"/>
    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>
    <!-- PL 20130215 Add cache="true" -->
    <SQL command="select" cache="true">
      <xsl:attribute name="result">
        <xsl:value-of select="$pResult"/>
      </xsl:attribute>
      select CLEARINGHOUSECODE,CONTRACTGROUPCODE,CONTRACTTYPECODE,
      CONTRACTTYPEDESC, CONTRACTMULTIPLIER, NOMINALVALUE,
      IDC_PRICE, CALCMETHOD, CFICODE, CATEGORY, SETTLTMETHOD,
      UNDERLYINGASSET, EXERCISESTYLE, DCSUFFIX
      from dbo.MEFFCCONTRTYP
      where (CLEARINGHOUSECODE=@CLEARINGHOUSECODE and CONTRACTGROUPCODE=@CONTRACTGROUPCODE and CONTRACTTYPECODE=@CONTRACTTYPECODE)
      <xsl:call-template name="ParamNodesBuilder">
        <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
        <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
      </xsl:call-template>
    </SQL>

  </xsl:template>

  <xsl:template name="BuildUnderlyingAssetCode">
    <xsl:param name="pUnderlyingAssetGroup"/>
    <xsl:param name="pUnderlyingAsset"/>
    <xsl:param name="pCategory"/>

    <xsl:choose>
      <xsl:when test="$pCategory = 'F'">
        <xsl:choose>
          <xsl:when test="$pUnderlyingAsset != 'M'">
            <xsl:value-of select="concat($pUnderlyingAssetGroup, $pUnderlyingAsset)"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$pUnderlyingAsset"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>

      <xsl:when test="$pCategory = 'O'">
        <xsl:value-of select="concat($pUnderlyingAssetGroup, $pUnderlyingAsset)"/>
      </xsl:when>
    </xsl:choose>

  </xsl:template>

  <xsl:template name ="SQLTableMEFFCCONTRTYP">

    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>

    <xsl:param name="pClearingHouseCode"/>
    <xsl:param name="pContractGroupCode"/>
    <xsl:param name="pContractTypeCode"/>

    <xsl:param name="pCurrency"/>
    <xsl:param name="pPublication"/>
    <xsl:param name="pContractTypeDescription"/>
    <xsl:param name="pContractTypeMultiplier"/>
    <xsl:param name="pNominalValue"/>
    <xsl:param name="pCalcMethod"/>
    <xsl:param name="pCFICode"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pSettlMethod"/>
    <xsl:param name="pUnderlyingAssetGroup"/>
    <xsl:param name="pUnderlyingAsset"/>
    <xsl:param name="pExerciseStyle"/>
    <xsl:param name="pDCSuffix"/>
    <!-- FL 20171006 [23495] New Parameter -->
    <xsl:param name="pMaturityRule"/>"

    <xsl:variable name="vParamCurrency">
      <Param name="CURRENCY" datatype="string">
        <xsl:value-of select="$pCurrency"/>
      </Param>
    </xsl:variable>

    <xsl:variable name="vSQLIdCurrency">
      <xsl:call-template name="SQLIdCurrency">
        <xsl:with-param name="pParamCurrency">
          <xsl:copy-of select="$vParamCurrency"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vSQLCLEARINGHOUSECODE">
      <xsl:call-template name="SQLMEFFSelectContratType">
        <xsl:with-param name="pResult" select="'CLEARINGHOUSECODE'"/>
        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vSQLCONTRACTGROUPCODE">
      <xsl:call-template name="SQLMEFFSelectContratType">
        <xsl:with-param name="pResult" select="'CONTRACTGROUPCODE'"/>
        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vSQLCONTRACTTYPECODE">
      <xsl:call-template name="SQLMEFFSelectContratType">
        <xsl:with-param name="pResult" select="'CONTRACTTYPECODE'"/>
        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>
      </xsl:call-template>
    </xsl:variable>


    <table name="MEFFCCONTRTYP" action="IU" sequenceno="1">

      <column name="CLEARINGHOUSECODE" datakey="true" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLCLEARINGHOUSECODE"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNOTNULL()"/>
          </control>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:value-of select="$pClearingHouseCode"/>
        </default>
      </column>

      <column name="CONTRACTGROUPCODE" datakey="true" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLCONTRACTGROUPCODE"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNOTNULL()"/>
          </control>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:value-of select="$pContractGroupCode"/>
        </default>
      </column>

      <column name="CONTRACTTYPECODE" datakey="true" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLCONTRACTTYPECODE"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNOTNULL()"/>
          </control>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:value-of select="$pContractTypeCode"/>
        </default>
      </column>

      <column name="IDC_PRICE" datakey="false" datakeyupd="true" datatype="string">
        <xsl:copy-of select="$vSQLIdCurrency"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Incorrect Currency.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The Currency &lt;b&gt;<xsl:value-of select="$pCurrency"/>&lt;/b&gt; is not found.
                &lt;b&gt;Action:&lt;/b&gt; Create the Currency &lt;b&gt;<xsl:value-of select="$pCurrency"/>&lt;/b&gt; or correct the input file, and restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFile"/><xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersion"/><xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>

      <column name="DTPUBLICATION" datakey="false" datakeyupd="true" datatype="date" dataformat="yyyyMMdd">
        <xsl:value-of select="$pPublication"/>
      </column>

      <column name="CONTRACTTYPEDESC" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pContractTypeDescription"/>
      </column>

      <column name="CONTRACTMULTIPLIER" datakey="false" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$pContractTypeMultiplier"/>
      </column>

      <column name="NOMINALVALUE" datakey="false" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$pNominalValue"/>
      </column>

      <column name="CALCMETHOD" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pCalcMethod"/>
      </column>

      <column name="CFICODE" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pCFICode"/>
      </column>

      <column name="CATEGORY" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pCategory"/>
      </column>

      <column name="SETTLTMETHOD" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pSettlMethod"/>
      </column>

      <column name="UNDERLYINGGROUP" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pUnderlyingAssetGroup"/>
      </column>

      <column name="UNDERLYINGASSET" datakey="false" datakeyupd="true" datatype="string">
        <xsl:call-template name="BuildUnderlyingAssetCode">
          <xsl:with-param name="pUnderlyingAssetGroup" select="$pUnderlyingAssetGroup"/>
          <xsl:with-param name="pUnderlyingAsset" select="$pUnderlyingAsset"/>
          <xsl:with-param name="pCategory" select="$pCategory"/>
        </xsl:call-template>
      </column>

      <column name="EXERCISESTYLE" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pExerciseStyle"/>
      </column>

      <column name="DCSUFFIX" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pDCSuffix"/>
      </column>

      <!-- FL 20171006 [23495] New Column -->
      <column name="IDMATURITYRULE" datakey="false" datakeyupd="true" datatype="integer">
        <SQL command="select" result="IDMATURITYRULE" cache="true">
          <![CDATA[
          select IDMATURITYRULE 
          from MATURITYRULE
          where IDENTIFIER = @IDENTIFIER]]>
          <Param name="IDENTIFIER" datatype="string">
            <xsl:value-of select="$pMaturityRule"/>
          </Param>
        </SQL>
      </column>

    </table>
  </xsl:template>

  <!-- SQLTableMEFFCCONTRGRP-->

  <xsl:template name="SQLMEFFSelectContratGroup">
    <xsl:param name="pResult"/>

    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>

    <!-- PL 20130215 Add cache="true" -->
    <SQL command="select" cache="true">
      <xsl:attribute name="result">
        <xsl:value-of select="$pResult"/>
      </xsl:attribute>
      select CLEARINGHOUSECODE,CONTRACTGROUPCODE,UNDERLYINGMNEMONIC
      from dbo.MEFFCCONTRGRP
      where (CLEARINGHOUSECODE=@CLEARINGHOUSECODE and CONTRACTGROUPCODE=@CONTRACTGROUPCODE)
      <xsl:call-template name="ParamNodesBuilder">
        <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
        <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
      </xsl:call-template>
    </SQL>
  </xsl:template>

  <xsl:template name="SQLTableMEFFCCONTRGRP">

    <xsl:param name="pClearingHouseCode"/>
    <xsl:param name="pContractGroupCode"/>
    <xsl:param name="pContractTypeCode"/>

    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>

    <xsl:param name="pContractGroupUnderlying"/>

    <xsl:variable name="vSQLCLEARINGHOUSECODE">
      <xsl:call-template name="SQLMEFFSelectContratGroup">
        <xsl:with-param name="pResult" select="'CLEARINGHOUSECODE'"/>
        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vSQLCONTRACTGROUPCODE">
      <xsl:call-template name="SQLMEFFSelectContratGroup">
        <xsl:with-param name="pResult" select="'CONTRACTGROUPCODE'"/>
        <xsl:with-param name="pExtSQLFilterNames" select="$pExtSQLFilterNames"/>
        <xsl:with-param name="pExtSQLFilterValues" select="$pExtSQLFilterValues"/>
      </xsl:call-template>
    </xsl:variable>

    <table name="MEFFCCONTRGRP" action="IU" sequenceno="1">

      <column name="CLEARINGHOUSECODE" datakey="true" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLCLEARINGHOUSECODE"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNOTNULL()"/>
          </control>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:value-of select="$pClearingHouseCode"/>
        </default>
      </column>

      <column name="CONTRACTGROUPCODE" datakey="true" datakeyupd="false" datatype="string">
        <xsl:copy-of select="$vSQLCONTRACTGROUPCODE"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNOTNULL()"/>
          </control>
          <control action="ApplyDefault" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
        <default>
          <xsl:value-of select="$pContractGroupCode"/>
        </default>
      </column>

      <column name="UNDERLYINGMNEMONIC" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pContractGroupUnderlying"/>
      </column>

    </table>

  </xsl:template>

  <xsl:template name="tSQLGetDataFromMarketTable">
    <xsl:param name="pResult"/>
    <xsl:param name="pExchangeSymbol"/>
    <!-- PL 20130215 Add cache="true" -->
    <!-- EG 20130717 Paramètre SQL en majuscule-->
    <SQL command="select" cache="true">
      <xsl:attribute name="result">
        <xsl:value-of select="$pResult"/>
      </xsl:attribute>
      select m.IDM, m.IDENTIFIER
      from dbo.MARKET m
      where (m.EXCHANGESYMBOL=@PSQLEXCHANGESYMBOL)
      <Param name="PSQLEXCHANGESYMBOL" datatype="string">
        <xsl:value-of select="$pExchangeSymbol"/>
      </Param>
    </SQL>
  </xsl:template>

  <!-- ======================================================= -->
  <!--           Template for inserting Equities               -->
  <!-- ======================================================= -->
  <xsl:template name="SQLTableASSET_EQUITY">
    <xsl:param name="pIdentifier"/>
    <xsl:param name="pDescription"/>
    <xsl:param name="pDisplayname"/>
    <xsl:param name="pSymbol" select="$gNull"/>
    <xsl:param name="pSymbolSuffix" select="''"/>
    <xsl:param name="pIsincode"/>
    <xsl:param name="pIdc"/>
    <xsl:param name="pRICCode"/>
    <xsl:param name="pBBGCode"/>
    <xsl:param name="pExchangeSymbol"/>
    <xsl:param name="pExchangeSymbolRelated"/>
    <xsl:param name="pIso10383"/>
    <xsl:param name="pIso10383Related"/>
    <!-- BD 20130507 Ajout des params pour sélectionner le marché options lié -->
    <xsl:param name="pIso10383Options"/>
    <xsl:param name="pExchangeSymbolOptions"/>
    <xsl:param name="pAssetTitled" select="''"/>
    <!--FL 20171006 [23064] -->
    <xsl:param name="pSQLdescription" select="$gNull"/>

    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>

    <xsl:variable name="vIDM">
      <xsl:call-template name="SQLIdMarket2">
        <xsl:with-param name="pISO10383" select="$pIso10383"/>
        <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
      </xsl:call-template>
    </xsl:variable>

    <table name="ASSET_EQUITY" action="IU" sequenceno="1">
      <!--RD 20200616 [25395] set datakeyupd false-->
      <column name="IDENTIFIER" datakeyupd="false" datatype="string">
        <xsl:value-of select="$gAutomaticComputeSQL"/>
      </column>

      <!--RD 20200616 [25395] set datakeyupd false-->
      <column name="DISPLAYNAME" datakeyupd="false" datatype="string">
        <xsl:value-of select="$gAutomaticComputeSQL"/>
      </column>

      <!--RD 20200616 [25395] set datakeyupd false-->
      <column name="DESCRIPTION" datatype="string">
        <xsl:choose>
          <xsl:when test="(string-length($pSQLdescription)>0) and ($pSQLdescription!=$gNull)">
            <xsl:attribute name="datakeyupd">true</xsl:attribute>
            <xsl:copy-of select="$pSQLdescription"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="datakeyupd">false</xsl:attribute>
            <xsl:value-of select="concat($gAutomaticComputeSQL,$pAssetTitled)"/>
          </xsl:otherwise>
        </xsl:choose>
      </column>

      <xsl:choose>

        <!-- Quand l'ISIN est renseigné -->
        <xsl:when test="(string-length($pIsincode)>0) and ($pIsincode!=$gNull)">
          <!-- ISINCODE datakey -->
          <column name="ISINCODE" datakey="true" datatype="string">
            <xsl:copy-of select="normalize-space($pIsincode)"/>
            <!-- RejectRow dans le cas d'un ISIN invalide (!= 12 car.) -->
            <xsl:if test="string-length(normalize-space($pIsincode))!=12">
              <controls>
                <control action="RejectRow" return="true">
                  true
                  <logInfo status="REJECT" isexception="false">
                    <message>
                      Invalid ISIN Code (<xsl:value-of select="$pIsincode"/>)
                    </message>
                  </logInfo>
                </control>
              </controls>
            </xsl:if>
          </column>
        </xsl:when>

        <!-- Quand l'ISIN n'est pas renseigné -->
        <xsl:otherwise>
          <!--SYMBOL datakey-->
          <column name="SYMBOL" datakey="true" datatype="string">
            <xsl:copy-of select="$pSymbol"/>
            <controls>
              <!--RejectRow si SYMBOL et ISIN non renseigné-->
              <control action="RejectRow" return="true" logtype="None">
                <SpheresLib function="ISNULL()"/>
              </control>
            </controls>
          </column>
        </xsl:otherwise>

      </xsl:choose>

      <!-- BD 20130326 Ajout des codes Reuters et Bloomberg -->
      <column name="RICCODE" datakeyupd="true" datatype="string">
        <xsl:copy-of select="$pRICCode"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="BBGCODE" datakeyupd="true" datatype="string">
        <xsl:copy-of select="$pBBGCode"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="IDC" datakeyupd="true" datatype="string">
        <xsl:copy-of select="$pIdc"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="IDM" datakey="true" datatype="int">
        <xsl:copy-of select="$vIDM"/>
      </column>

      <xsl:call-template name="SysInsUpd"/>

    </table>

    <!-- BD 20130507 Insert dans ASSET_EQUITY_RDCMK en même temps que ASSET_EQUITY -->
    <table name="ASSET_EQUITY_RDCMK" action="IU" sequenceno="2">

      <column name="IDASSET" datakey="true" datatype="int">
        <SQL command="select" result="IDASSET" cache="true">
          select IDASSET
          from dbo.ASSET_EQUITY
          where IDM=@IDM
          <xsl:choose>
            <xsl:when test="string-length($pIsincode)>0 and ($pIsincode!=$gNull)">
              and ISINCODE=@ISINCODE
            </xsl:when>
            <xsl:otherwise>
              and SYMBOL=@SYMBOL
            </xsl:otherwise>
          </xsl:choose>
          <Param name="ISINCODE" datatype="string">
            <xsl:copy-of select="$pIsincode"/>
          </Param>
          <Param name="SYMBOL" datatype="string">
            <xsl:copy-of select="$pSymbol"/>
          </Param>
          <Param name="IDM" datatype="integer">
            <xsl:copy-of select="$vIDM"/>
          </Param>
        </SQL>
        <controls>
          <control action="RejectRow" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="IDM_RELATED" datakey="true" datatype="int">
        <xsl:call-template name="SQLIdMarket2">
          <xsl:with-param name="pISO10383" select="$pIso10383Related"/>
          <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbolRelated"/>
        </xsl:call-template>
        <controls>
          <control action="RejectRow" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="SYMBOL" datakeyupd="true" datatype="string">
        <xsl:copy-of select="$pSymbol"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="SYMBOLSUFFIX" datakeyupd="true" datatype="string">
        <xsl:copy-of select="$pSymbolSuffix"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="IDM_OPTIONS" datakeyupd="true" datatype="int">
        <xsl:call-template name="SQLIdMarket2">
          <xsl:with-param name="pISO10383" select="$pIso10383Options"/>
          <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbolOptions"/>
        </xsl:call-template>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <xsl:call-template name="SysInsUpd"/>

    </table>
  </xsl:template>


  <!-- ======================================================= -->
  <!--           Template for inserting Index                  -->
  <!-- ======================================================= -->
  <xsl:template name="SQLTableASSET_INDEX">
    <xsl:param name="pIdentifier"/>
    <xsl:param name="pDescription"/>
    <xsl:param name="pDisplayname"/>
    <xsl:param name="pIsincode"/>
    <xsl:param name="pIdc"/>
    <!-- BD 20130326 Ajout des param pour le Symbol et les codes Reuters et Bloomberg -->
    <xsl:param name="pRICCode"/>
    <xsl:param name="pBBGCode"/>
    <xsl:param name="pSymbol"/>
    <!-- BD 20130422 Ajout du param pSymbolSuffix -->
    <xsl:param name="pSymbolSuffix" select="''"/>
    <xsl:param name="pExchangeSymbol"/>
    <xsl:param name="pExchangeSymbolRelated"/>
    <!-- BD 20130429 Ajout des parametre Iso10383 et Iso10383Related -->
    <xsl:param name="pIso10383"/>
    <xsl:param name="pIso10383Related"/>
    <!-- BD 20130607 Nouveau param: pAssetTitled, libellé du SSJ -->
    <xsl:param name="pAssetTitled" select="''"/>
    <!--FL 20171006 [23064] -->
    <xsl:param name="pSQLdescription" select="$gNull"/>

    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>

    <xsl:variable name="vIDM">
      <xsl:call-template name="SQLIdMarket2">
        <xsl:with-param name="pISO10383" select="$pIso10383"/>
        <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
      </xsl:call-template>
    </xsl:variable>

    <table name="ASSET_INDEX" action="IU" sequenceno="1">

      <!-- INDENTIFIER doit être unique -->
      <!--RD 20200616 [25395] set datakeyupd false-->
      <column name="IDENTIFIER" datakeyupd="false" datatype="string">
        <xsl:value-of select="$gAutomaticComputeSQL"/>
      </column>

      <!--RD 20200616 [25395] set datakeyupd false-->
      <column name="DISPLAYNAME" datakeyupd="false" datatype="string">
        <xsl:value-of select="$gAutomaticComputeSQL"/>
      </column>

      <!--RD 20200616 [25395] set datakeyupd false-->
      <column name="DESCRIPTION" datatype="string">
        <xsl:choose>
          <xsl:when test="(string-length($pSQLdescription)>0) and ($pSQLdescription!=$gNull)">
            <xsl:attribute name="datakeyupd">true</xsl:attribute>
            <xsl:copy-of select="$pSQLdescription"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="datakeyupd">false</xsl:attribute>
            <xsl:value-of select="concat($gAutomaticComputeSQL,$pAssetTitled)"/>
          </xsl:otherwise>
        </xsl:choose>
      </column>

      <xsl:choose>

        <!-- Quand l'ISIN est renseigné -->
        <xsl:when test="(string-length($pIsincode)>0) and ($pIsincode!=$gNull)">

          <!-- ISINCODE datakey = true -->
          <column name="ISINCODE" datakey="true" datatype="string">
            <xsl:copy-of select="normalize-space($pIsincode)"/>
            <!-- RejectRow dans le cas d'un ISIN invalide (!= 12 car.) -->
            <xsl:if test="string-length(normalize-space($pIsincode))!=12">
              <controls>
                <control action="RejectRow" return="true">
                  true
                  <logInfo status="REJECT" isexception="false">
                    <message>
                      Invalid ISIN Code (<xsl:value-of select="$pIsincode"/>)
                    </message>
                  </logInfo>
                </control>
              </controls>
            </xsl:if>
          </column>

          <!-- SYMBOL datakeyupd = true -->
          <column name="SYMBOL" datakeyupd="true" datatype="string">
            <xsl:copy-of select="$pSymbol"/>
            <controls>
              <control action="RejectColumn" return="true" logtype="None">
                <SpheresLib function="ISNULL()"/>
              </control>
              <!--FL/RD On est dans un template ASSET_INDEX et on controle la table ASSET_EQUITY !? 
              Control incohérent empéchant la mise à jour de la colonne ASSET_INDEX.SYMBOL-->
              <!-- BD 20130520 Si le param SymbolSuffix n'est pas renseigné... -->
              <!--<xsl:if test="string-length($pSymbolSuffix)=0">
                <control action="RejectColumn" return="1" logtype="None">
                  -->
              <!-- ...on test si ASSET_EQUITY.SYMBOLSUFFIX est renseigné pour cet Equity
                        - Si oui, on rejète la colonne
                        - Sinon, on insert le Symbol -->
              <!--
                  <SQL command="select" result="IS_SYMBOLSUFFIX_EXIST" cache="true">
                    select count(1) as IS_SYMBOLSUFFIX_EXIST
                    from dbo.ASSET_EQUITY
                    where ISINCODE=@ISINCODE and SYMBOLSUFFIX is not null and IDM=@IDM
                    <Param name="ISINCODE" datatype="string">
                      <xsl:value-of select="$pIsincode"/>
                    </Param>
                    <Param name="IDM" datatype="integer">
                      <xsl:copy-of select="$vIDM"/>
                    </Param>
                  </SQL>
                </control>
              </xsl:if>-->
            </controls>
          </column>
        </xsl:when>

        <!-- Quand l'ISIN n'est pas renseigné -->
        <xsl:otherwise>

          <!-- SYMBOL datakey = true -->
          <column name="SYMBOL" datakey="true" datatype="string">
            <xsl:copy-of select="$pSymbol"/>
            <controls>
              <control action="RejectRow" return="true" logtype="None">
                <SpheresLib function="ISNULL()"/>
              </control>
              <!-- BD 20130520 Si le param SymbolSuffix n'est pas renseigné... -->
              <xsl:if test="string-length($pSymbolSuffix)=0">
                <control action="RejectColumn" return="1" logtype="None">
                  <!-- ...on test si ASSET_EQUITY.SYMBOLSUFFIX est renseigné pour cet Equity
                        - Si oui, on rejète la colonne
                        - Sinon, on insert le Symbol -->
                  <SQL command="select" result="IS_SYMBOLSUFFIX_EXIST" cache="true">
                    select count(1) as IS_SYMBOLSUFFIX_EXIST
                    from dbo.ASSET_EQUITY
                    where ISINCODE=@ISINCODE and SYMBOLSUFFIX is not null and IDM=@IDM
                    <Param name="ISINCODE" datatype="string">
                      <xsl:value-of select="$pIsincode"/>
                    </Param>
                    <Param name="IDM" datatype="integer">
                      <xsl:copy-of select="$vIDM"/>
                    </Param>
                  </SQL>
                </control>
              </xsl:if>
            </controls>
          </column>
        </xsl:otherwise>
      </xsl:choose>

      <!-- BD 20130422 Ajout du param pSymbolSuffix -->
      <column name="SYMBOLSUFFIX" datakeyupd="true" datatype="string">
        <xsl:copy-of select="$pSymbolSuffix"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="RICCODE" datakeyupd="true" datatype="string">
        <xsl:copy-of select="$pRICCode"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="BBGCODE" datakeyupd="true" datatype="string">
        <xsl:copy-of select="$pBBGCode"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="IDC" datakeyupd="true" datatype="string">
        <xsl:copy-of select="$pIdc"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="IDM" datakey="true" datatype="int">
        <xsl:copy-of select="$vIDM"/>
      </column>

      <column name="IDM_RELATED" datakeyupd="true" datatype="int">
        <xsl:call-template name="SQLIdMarket2">
          <xsl:with-param name="pISO10383" select="$pIso10383Related"/>
          <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbolRelated"/>
        </xsl:call-template>
      </column>

      <xsl:call-template name="SysInsUpd"/>

    </table>
  </xsl:template>

  <!-- Return the Spheres NULL constant when the input argument has the default value of the xsl types:
    - int
    - string
    - decimal
    -->
  <xsl:template name="Nullable">
    <xsl:param name="pCheckValue"/>

    <xsl:choose>

      <xsl:when test="$pCheckValue = ''">
        <xsl:value-of select="$gNull"/>
      </xsl:when>

      <xsl:when test="number($pCheckValue) = 0">
        <xsl:value-of select="$gNull"/>
      </xsl:when>

      <xsl:otherwise>
        <xsl:value-of select="$pCheckValue"/>
      </xsl:otherwise>

    </xsl:choose>

  </xsl:template>

  <!-- Return the Spheres NULL constant when the input argument has the default value of the xsl types:
    - string
    -->
  <xsl:template name="NullableString">
    <xsl:param name="pCheckValue"/>

    <xsl:choose>

      <xsl:when test="$pCheckValue = ''">
        <xsl:value-of select="$gNull"/>
      </xsl:when>

      <xsl:otherwise>
        <xsl:value-of select="$pCheckValue"/>
      </xsl:otherwise>

    </xsl:choose>

  </xsl:template>

</xsl:stylesheet>
