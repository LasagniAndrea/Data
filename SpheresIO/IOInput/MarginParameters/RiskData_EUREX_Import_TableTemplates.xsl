<!--
=======================================================================================
Summary : Import EUREX parameters - Table Templates
File    : RiskData_EUREX_Import_TableTemplates.xsl
=======================================================================================
Version : v3.2.0.0                                           
Date    : 20130320
Author  : BD/FL
Comment : Management of market segment 'XHEX'
          Add template : SQLGetIdDC_EUREX
          Replace call SQLDerivativeContractAfter by call SQLGetIdDC_EUREX
=======================================================================================
Version : v1.0.0.0  
Date    : 20110215                                          
Author  : MF                                                 
=======================================================================================
-->
<!-- FI 20200901 [25468] use of GetUTCDateTimeSys -->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>
  
  <xsl:include href="..\Common\CommonInput.xslt"/>
  <xsl:include href="..\Quote\Quote_Common_SQL.xsl"/>

  <xsl:variable name="vVersionRiskDataTableTemplates">v0.0.0.1</xsl:variable>
  <xsl:variable name="vFileNameRiskDataTableTemplates">RiskData_EUREX_Import_TableTemplates.xsl</xsl:variable>


  <!-- Insert/Update an ETD contract inside of the EUREX hierarchy (group/class) -->
  <xsl:template name="PARAMSEUREX_CONTRACT_IU">

    <!-- sequence number of the operation -->
    <xsl:param name="pSequenceNumber" select="'1'"/>
    <!-- First level of the EUREX hierarchy : the group 
    MGN-GRP-COD-RMPARM -->
    <xsl:param name="pMgnGrp"/>
    <!-- Second level of the EUREX hierarchy : the class 
    MGN-CLS-COD-RMPARM -->
    <xsl:param name="pMgnCls"/>
    <!-- Thirth level of the EUREX hierarchy : the contract 
    SECU-ID-COD-RMPARM 
    (inside Sym we have the contract symbol, but in the EUREX market it identifies one ETD contract only, 
    then the symbol has the same value than the contract identifier) -->
    <xsl:param name="pSym"/>
    <!-- Exchange symbol-->
    <xsl:param name="pExch"/>
    <!-- Maturity margining indicator 
    MATURITY-SWITCH-RMPARM, 
    possible values: Y -> Enabled, N -> Disabled  -->
    <xsl:param name="pMgnSwt"/>
    <!-- Factor for the expiry month 
    EXP-MONTH-FACTOR-RMPARM-->
    <xsl:param name="pExpMthFtr"/>
    <!-- group offset for the positions on the current ETD contract, 
    GRP-OFF-SET-RMPARM -->
    <xsl:param name="pOffset"/>
    <!-- back month spread rate -->
    <xsl:param name="pBckSpdRat"/>
    <!-- Spot month spread rate -->
    <xsl:param name="pSptSpdRat"/>
    <!-- Out of the money minimum rate amount -->
    <xsl:param name="pOOM_MinRat"/>
    <!-- sys columns controls enabled/disabled -->
    <xsl:param name="pIsWithControl" select="$gTrue"/>

    <table name="PARAMSEUREX_CONTRACT" action="IU" sequenceno="{$pSequenceNumber}">

      <!-- BD/FL 20130320 : Replace call SQLDerivativeContractAfter by call SQLGetIdDC_EUREX -->
      <!-- RD 20130416 [18588] Ne pas afficher de Warning si le DC n'existe pas-->
      <column name="IDDC_Control">
        <xsl:call-template name="SQLGetIdDC_EUREX">
          <xsl:with-param name="pParamISO10383" select="$pExch"/>
          <xsl:with-param name="pParamContractSymbol" select="$pSym"/>
        </xsl:call-template>
        <controls>
          <control action="RejectRow" return="true" >
            <SpheresLib function="ISNULL()"/>
            <!--<logInfo status="REJECT" isexception="false">
              <code>SYS</code>
              <number>5303</number>
              <data1>
                <xsl:value-of select="$pSym"/>
              </data1>
              <data2>
                <xsl:value-of select="$pExch"/>
              </data2>
              <data3>
                <xsl:value-of select="concat($pMgnCls, ' ', $pMgnGrp)"/>
              </data3>
            </logInfo>-->
          </control>
          <control action="RejectColumn" return="false" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="CONTRACTSYMBOL" datakey="true">
        <xsl:call-template name="Nullable">
          <xsl:with-param name="pCheckValue" select="$pSym"/>
        </xsl:call-template>
        <controls>
          <control action="RejectRow" return="true" >
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="false">
              <code>SYS</code>
              <number>5304</number>
              <data1>
                <xsl:value-of select="'Contract symbol'"/>
              </data1>
              <data2>
                <xsl:value-of select="'Sym'"/>
              </data2>
              <data3>
                <xsl:value-of select="$pSym"/>
              </data3>
            </logInfo>
          </control>
        </controls>
      </column>

      <column name="MARGINGROUP" datakeyupd="true">
        <xsl:call-template name="Nullable">
          <xsl:with-param name="pCheckValue" select="$pMgnGrp"/>
        </xsl:call-template>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="MARGINCLASS" datakeyupd="true">
        <xsl:call-template name="Nullable">
          <xsl:with-param name="pCheckValue" select="$pMgnCls"/>
        </xsl:call-template>
        <controls>
          <control action="RejectRow" return="true" >
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="false">
              <code>SYS</code>
              <number>5304</number>
              <data1>
                <xsl:value-of select="'Derivative margin class'"/>
              </data1>
              <data2>
                <xsl:value-of select="'MgnCls'"/>
              </data2>
              <data3>
                <xsl:value-of select="$pMgnCls"/>
              </data3>
            </logInfo>
          </control>
        </controls>
      </column>

      <column name="MATURITY_SWITCH" datakeyupd="true">
        <xsl:value-of select="$pMgnSwt"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="EXPIRYMONTH_FACTOR" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$pExpMthFtr"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="OFFSET" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$pOffset"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="BACKMONTH_SPREADRATE" datakeyupd="true" datatype="integer">
        <xsl:value-of select="$pBckSpdRat"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="SPOTMONTH_SPREADRATE" datakeyupd="true" datatype="integer">
        <xsl:value-of select="$pSptSpdRat"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="OOM_MINIMUMRATE" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$pOOM_MinRat"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <!--
      *********************************************
      Sys columns
      *********************************************
      -->

      <column name="DTINS" datatype="datetime">
        <SpheresLib function="GetUTCDateTimeSys()"/>
        <xsl:if test="$pIsWithControl = $gTrue">
          <controls>
            <control action="RejectColumn" return="true" logtype="None">
              <SpheresLib function="IsUpdate()"/>
            </control>
          </controls>
        </xsl:if>
      </column>
      <column name="IDAINS" datatype="int">
        <SpheresLib function="GetUserID()"/>
        <xsl:if test="$pIsWithControl = $gTrue">
          <controls>
            <control action="RejectColumn" return="true" logtype="None">
              <SpheresLib function="IsUpdate()"/>
            </control>
          </controls>
        </xsl:if>
      </column>

      <xsl:call-template name="SysUpd">
        <xsl:with-param name="pIsWithControl" select="$pIsWithControl"/>
      </xsl:call-template>

    </table>

  </xsl:template>

  <!-- Update an EUREX maturity parameter, entry of the PARAMSEUREX_MATURITY table -->
  <xsl:template name="PARAMSEUREX_MATURITY_U">

    <!-- sequence number of the operation -->
    <xsl:param name="pSequenceNumber" select="'1'"/>
    <!-- contract symbol/ contract identifier  -->
    <xsl:param name="pContractSymbol"/>
    <!-- putcall indicator, null for futures -->
    <xsl:param name="pPutCall"/>
    <!-- maturity code. Format: YYYYMM.
    Built by this concatenation: '20' + SERI-EXP-DAT-RMPARA or '20' + EXPI-YR-DAT-RMPARA + EXPI-MTH-DAT-RMPARA -->
    <xsl:param name="pMaturityYearMonth"/>
    <!-- maturity factor 
    MATURITY-FACTOR-RMPARA-->
    <xsl:param name="pMatFtr"/>
    <!-- sys columns controls enabled/disabled -->
    <xsl:param name="pIsWithControl" select="$gTrue"/>

    <table name="PARAMSEUREX_MATURITY" action="U" sequenceno="{$pSequenceNumber}">

      <column name="CONTRACTSYMBOL" datakey="true">
        <xsl:value-of select="$pContractSymbol"/>
        <controls>
          <control action="RejectRow" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="PUTCALL" datakey="true">
        <xsl:value-of select="$pPutCall"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="MATURITYYEARMONTH" datakey="true">
        <xsl:call-template name="Nullable">
          <xsl:with-param name="pCheckValue" select="$pMaturityYearMonth"/>
        </xsl:call-template>
        <controls>
          <control action="RejectRow" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="MATURITY_FACTOR" datatype="decimal" datakeyupd="true">
        <xsl:value-of select="$pMatFtr"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <!--
      *********************************************
      Sys columns
      *********************************************
      -->

      <xsl:call-template name="SysUpd">
        <xsl:with-param name="pIsWithControl" select="$pIsWithControl"/>
      </xsl:call-template>

    </table>

  </xsl:template>

  <!-- *************************************************************************************************** -->
  <!-- SQLGetIdDC_EUREX - return IDDC from an Derivative Contract                                           -->
  <!-- *************************************************************************************************** -->
  <xsl:template name="SQLGetIdDC_EUREX">

    <!-- BD/FL 20130320 : Création d'un nouveau template permettant de rechercher l’IDDC en fonction 
    du code ISO10383_ALPHA4 d’un marhé et du ContractSymbol afin de gérer les "segments de marché" sur l’EUREX.
    
    Pour info les segments de marché sur l’EUREX sont gérés de la façon suivante :
    
     IDENTIFIER	DISPLAYNAME			         MARKETTYPE	ISO10383_ALPHA4	EXCHANGESYMBOL
     XEUR		    EUREX DEUTSCHLAND		     OPERATING  XEUR			      XEUR
     XHEX		    HELSINKI STOCK EXCHANGE	 SEGMENT	  XEUR			      XHEX

    Ps. On voit bien dans cet exemple que le marché et son segment ont le même code ISO 10383 (colonne : ISO10383_ALPHA4) -->

    <xsl:param name="pParamISO10383"/>
    <xsl:param name="pParamContractSymbol"/>
    <!-- BD 20130520 : Appel du template SQLDTENABLEDDTDISABLED pour vérifier la validité du DC sélectionné -->
    <SQL command="select" cache="true" result="IDDC">
      select dc.IDDC
      from dbo.DERIVATIVECONTRACT dc
      inner join dbo.MARKET m on m.IDM=dc.IDM and (m.ISO10383_ALPHA4=@ISO10383_ALPHA4)
      where (dc.CONTRACTSYMBOL=@CONTRACTSYMBOL)
      <xsl:call-template name="SQLDTENABLEDDTDISABLED">
        <xsl:with-param name="pTable" select="'dc'"/>
      </xsl:call-template>
      <Param name="DT" datatype="date">
        <xsl:value-of select="$gParamDtBusiness"/>
      </Param>
      <Param name="ISO10383_ALPHA4" datatype="string">
        <xsl:value-of select="$pParamISO10383"/>
      </Param>
      <Param name="CONTRACTSYMBOL" datatype="string">
        <xsl:value-of select="$pParamContractSymbol"/>
      </Param>
    </SQL>

  </xsl:template>

</xsl:stylesheet>
