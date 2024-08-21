<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="xml" indent="yes"/>

  <!--
==============================================================
 Summary : Default Spheres F&Oml mapping for files containing financial fluxes ('CashFlow' and extentions) for compare purpose
==============================================================
  Version 3.4.0.0_4
  Date: 18/06/2013
  Author: RD
  Description: Use Market in templates "SelectEnrichedSymOrIdentifier" and "SelectEnrichedSym"

  Version : v1.0.0.0                                           
  Date    : 201202...                                           
  Author  : MF                                                   
==============================================================
-->

  <xsl:include href="..\Common\CommonInput.xslt"/>
  <xsl:include href="CommonExtldata.xslt"/>

  <!-- IO Parameter InternalDataTypeEnum -->
  <xsl:variable name="vBusinessType">
    <xsl:choose>
      <xsl:when test="/iotask/parameters/parameter[@id='InternalDataTypeEnum']">
        <xsl:value-of select="/iotask/parameters/parameter[@id='InternalDataTypeEnum']"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'CashFlows'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- IO Parameter IDA entity (needed by Spheres F&O only) -->
  <xsl:variable name="vEntityString">
    <xsl:choose>
      <xsl:when test="/iotask/parameters/parameter[@id='ENTITYIDENTIFIER']">
        <xsl:value-of select="/iotask/parameters/parameter[@id='ENTITYIDENTIFIER']"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$NULL"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- IO Parameter External link, with the name of the column used to perform transcodes -->
  <xsl:variable name="vExternalLinkColumn">
    <xsl:choose>
      <xsl:when test="/iotask/parameters/parameter[@id='EXTLLINK_COLUMN']">
        <xsl:value-of select="/iotask/parameters/parameter[@id='EXTLLINK_COLUMN']"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'EXTLLINK'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- IO Parameter External Symbol, with the name of the column used to perform transcodes on the derivative contract table -->
  <xsl:variable name="vExternalSymbolColumn">
    <xsl:choose>
      <xsl:when test="/iotask/parameters/parameter[@id='CONTRACTSYMBOL_COLUMN']">
        <xsl:value-of select="/iotask/parameters/parameter[@id='CONTRACTSYMBOL_COLUMN']"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'CONTRACTSYMBOL'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- match the "file" node, where we create the main ETLDATA element -->
  <xsl:template match="file">
    <file>
      <xsl:call-template name="IOFileAtt"/>

      <xsl:variable name="IsResetParameter" select="/iotask/parameters/parameter[@id='ISRESET']"/>

      <row id="r0" src="0">

        <xsl:if test="$IsResetParameter = 'true'">

          <xsl:call-template name="Reset">
            <xsl:with-param name="pIoTaskDet" select="../../@id"/>
            <xsl:with-param name="pBusinessType" select="$vBusinessType"/>
          </xsl:call-template>

        </xsl:if>

        <!-- write the EFS XSL/SQL insert request for the EXTLDATA table -->
        <xsl:call-template name="EXTLDATA">
          <xsl:with-param name="pIsWithControl" select="true()"/>
          <xsl:with-param name="pIdIoTaskDet" select="../../@id"/>
          <xsl:with-param name="pFileName" select="concat(@folder, '\', @name)"/>
          <xsl:with-param name="pDtFile" select="@date"/>
          <xsl:with-param name="pBusinessType" select="$vBusinessType"/>
          <xsl:with-param name="pMessage" select="$NULL"/>
          <xsl:with-param name="pLoFileContent" select="$NULL"/>
          <xsl:with-param name="pEntityString" select="$vEntityString"/>
          <xsl:with-param name="pSize" select="@size"/>
        </xsl:call-template>
      </row>

      <xsl:variable name="vSelectIDExtlData">
        <xsl:call-template name="SelectIDExtlData">
          <xsl:with-param name="pIoTaskDet" select="../../@id"/>
        </xsl:call-template>
      </xsl:variable>

      <!-- match any row node inside the current file node, in order to get all the parsed rows -->
      <xsl:apply-templates select="row[@status='success']">
        <xsl:with-param name="pSelectIDExtlData">
          <xsl:copy-of select="$vSelectIDExtlData"/>
        </xsl:with-param>
        <xsl:with-param name="pDtBusinessParam" select="/iotask/parameters/parameter[@id='DTBUSINESS']"/>
      </xsl:apply-templates>

    </file>
  </xsl:template>

  <!-- 
  Get the activation state of the amount for the current comparison process.
  Returns 'true' when the amount is activated.
  -->
  <xsl:template name="IsValidAmount">
    <!-- amount type -->
    <xsl:param name="pNameAmount"/>

    <!-- type of the modality we use to figure out when an amount has to be compared -->
    <xsl:variable name="vValidAmounts">
      <xsl:choose>
        <xsl:when test="/iotask/parameters/parameter[@id='VALIDAMOUNTS']">
          <xsl:value-of select="/iotask/parameters/parameter[@id='VALIDAMOUNTS']"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$NULL"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="$vValidAmounts = 'SIGMA_TOOLKIT'">
        <xsl:variable name="vAcctTyp">
          <xsl:value-of select="data[@name='AcctTyp']"/>
        </xsl:variable>
        <xsl:choose>
          <xsl:when test="$vAcctTyp = 'COMPTE' or $vAcctTyp = 'BROKER' or $vAcctTyp = 'COMPENS'">
            <xsl:choose>
              <xsl:when test="
                        $pNameAmount = 'PrmAmt' 
                        or $pNameAmount = 'UMgAmt' 
                        or $pNameAmount = 'VrMrgnAmt'
                        or $pNameAmount = 'CollAmt'
                        or $pNameAmount = 'TaxComBrkAmt'
                        or $pNameAmount = 'RMgAmt' 
                        or $pNameAmount = 'CallAmt'
                        or $pNameAmount = 'LovAmt'
                        ">
                <xsl:value-of select="'1'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'0'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <!-- $vAcctTyp = 'GROUPE'  -->
          <xsl:otherwise>
            <xsl:choose>
              <xsl:when test="
                        $pNameAmount = 'CollAmt'
                        or $pNameAmount = 'CallAmt'
                        or $pNameAmount = 'RptAmt'
                        ">
                <xsl:value-of select="'1'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'0'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <!-- by default an amount is valid and will be compared -->
      <xsl:otherwise>
        <xsl:value-of select="'1'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- match any "row" node, where we create an EXTLDATADET element for each parsed row  -->
  <xsl:template match="row">
    <xsl:param name="pSelectIDExtlData" select="$NULL"/>
    <xsl:param name="pDtBusinessParam" select="$NULL"/>

    <row>
      <xsl:call-template name="IORowAtt"/>

      <!-- Transcode the put call  -->
      <xsl:variable name="vTCPutCall">
        <xsl:choose>
          <xsl:when test="data[@name='PutCall']">
            <xsl:call-template name="TCPutCall">
              <xsl:with-param name="pPutCall" select="data[@name='PutCall']"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$NULL"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <!-- Transcode the instrument identifier : P,C -> 'ExchangeTradedOption' ; F -> 'ExchangeTradedFuture'; then get the IDI -->
      <xsl:variable name="vTCInstrumentIdentifier">
        <xsl:choose>
          <xsl:when test="data[@name='PutCall']">
            <xsl:call-template name="TCInstrumentIdentifier">
              <xsl:with-param name="pPutCall" select="data[@name='PutCall']"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$NULL"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vSelectIDI">
        <xsl:choose>
          <xsl:when test="data[@name='PutCall']">
            <xsl:call-template name="SelectIDIFromIDENTIFIER">
              <xsl:with-param name="pNames" select="'INSTRUMENT'"/>
              <xsl:with-param name="pValues" select="$vTCInstrumentIdentifier"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$NULL"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <!-- Transcode the symbol  -->
      <xsl:variable name="vTCSym">
        <xsl:choose>
          <xsl:when test="data[@name='Sym']">
            <!--RD 20140717 [19928] Gérer le cas des DC annulés. Ajout du paramètre pDtBusiness-->
            <xsl:call-template name="SelectEnrichedSymOrIdentifier">
              <xsl:with-param name="pExternalLink" select="$vExternalLinkColumn"/>
              <xsl:with-param name="pExternalSymbol" select="$vExternalSymbolColumn"/>
              <xsl:with-param name="pNames" select="concat('EXTL_Exch',',','SYMBOL',',','INSTRUMENT',',','NOTFOUND',',','NOTUNIQUE')"/>
              <xsl:with-param name="pValues" select="concat(data[@name='Mrk'],',',data[@name='Sym'],',',$vTCInstrumentIdentifier,',',$cNOTFOUND,',',$cNOTUNIQUE)"/>
              <xsl:with-param name="pDtBusiness" select="$pDtBusinessParam"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$NULL"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <!-- Get the market IDM -->
      <xsl:variable name="vSelectIDM">
        <xsl:choose>
          <xsl:when test="data[@name='Mrk']">
            <xsl:call-template name="SelectIDM">
              <xsl:with-param name="pExternalLink" select="$vExternalLinkColumn"/>
              <xsl:with-param name="pResult" select="$gbnIDM"/>
              <xsl:with-param name="pNames" select="'EXTL_Exch'"/>
              <xsl:with-param name="pValues" select="data[@name='Mrk']"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$NULL"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <!-- Count how many market matches the given EXTL_Exch -->
      <xsl:variable name="vEnrichedExch">
        <xsl:choose>
          <xsl:when test="data[@name='Mrk']">
            <xsl:call-template name="SelectEnrichedExch">
              <xsl:with-param name="pExternalLink" select="$vExternalLinkColumn"/>
              <xsl:with-param name="pNames" select="concat('EXTL_Exch',',','NOTFOUND',',','NOTUNIQUE')"/>
              <xsl:with-param name="pValues" select="concat(data[@name='Mrk'],',',$cNOTFOUND,',',$cNOTUNIQUE)"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$NULL"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <!-- Count how many book/actor matches the given EXTL_Acct/EXTL_PtyID27  -->
      <xsl:variable name="vEnrichedAcct">
        <xsl:choose>
          <xsl:when test="data[@name='Pty.ID.27']">
            <xsl:call-template name="SelectEnrichedAcct">
              <xsl:with-param name="pExternalLink" select="$vExternalLinkColumn"/>
              <xsl:with-param name="pNames" select="concat('EXTL_Acct',',','EXTL_PtyID27',',','NOTFOUND',',','NOTUNIQUE')"/>
              <xsl:with-param name="pValues" select="concat(data[@name='Acct'],',',data[@name='Pty.ID.27'],',',$cNOTFOUND,',',$cNOTUNIQUE)"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="SelectEnrichedAcctOnBookOnly">
              <xsl:with-param name="pExternalLink" select="$vExternalLinkColumn"/>
              <xsl:with-param name="pNames" select="concat('EXTL_Acct',',','NOTFOUND',',','NOTUNIQUE')"/>
              <xsl:with-param name="pValues" select="concat(data[@name='Acct'],',',$cNOTFOUND,',',$cNOTUNIQUE)"/>
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <!-- Transcode the Side value : ... -->
      <xsl:variable name="vTCAcctTyp">
        <xsl:call-template name="TCAcctTyp">
          <xsl:with-param name="pAcctTyp">
            <xsl:value-of select="data[@name='AcctTyp']"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:variable>

      <!-- Build the global parameters list for the actual row node -->
      <parameters>
        <xsl:call-template name="BuildGlobalParameter">
          <xsl:with-param name="pParamName" select="$gbnIDExtlData"/>
          <xsl:with-param name="pParamValue">
            <xsl:copy-of select="$pSelectIDExtlData"/>
          </xsl:with-param>
        </xsl:call-template>
        <xsl:call-template name="BuildGlobalParameter">
          <xsl:with-param name="pParamName" select="$gbnIDI"/>
          <xsl:with-param name="pParamValue">
            <xsl:copy-of select="$vSelectIDI"/>
          </xsl:with-param>
        </xsl:call-template>
        <xsl:call-template name="BuildGlobalParameter">
          <xsl:with-param name="pParamName" select="$gbnIDM"/>
          <xsl:with-param name="pParamValue">
            <xsl:copy-of select="$vSelectIDM"/>
          </xsl:with-param>
        </xsl:call-template>
        <xsl:call-template name="BuildGlobalParameter">
          <xsl:with-param name="pParamName" select="$gbnEnrichedExch"/>
          <xsl:with-param name="pParamValue">
            <xsl:copy-of select="$vEnrichedExch"/>
          </xsl:with-param>
        </xsl:call-template>
        <xsl:call-template name="BuildGlobalParameter">
          <xsl:with-param name="pParamName" select="$gbnEnrichedAcct"/>
          <xsl:with-param name="pParamValue">
            <xsl:copy-of select="$vEnrichedAcct"/>
          </xsl:with-param>
        </xsl:call-template>
        <xsl:call-template name="BuildGlobalParameter">
          <xsl:with-param name="pParamName" select="$gbnEnrichedSym"/>
          <xsl:with-param name="pParamValue">
            <xsl:copy-of select="$vTCSym"/>
          </xsl:with-param>
        </xsl:call-template>
      </parameters>

      <!-- Currencies amounts  -->

      <xsl:variable name="vPrmCcy">
        <xsl:choose>
          <xsl:when test="data[@name='PrmCcy']">
            <xsl:value-of select="data[@name='PrmCcy']"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="data[@name='Ccy']"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vPrmEnable">
        <xsl:call-template name="IsValidAmount">
          <xsl:with-param name="pNameAmount" select="'PrmAmt'"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name="vCallCcy">
        <xsl:choose>
          <xsl:when test="data[@name='CallCcy']">
            <xsl:value-of select="data[@name='CallCcy']"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="data[@name='Ccy']"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vCallEnable">
        <xsl:call-template name="IsValidAmount">
          <xsl:with-param name="pNameAmount" select="'CallAmt'"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name="vFaceCcy">
        <xsl:choose>
          <xsl:when test="data[@name='FaceCcy']">
            <xsl:value-of select="data[@name='FaceCcy']"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="data[@name='Ccy']"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vFaceEnable">
        <xsl:call-template name="IsValidAmount">
          <xsl:with-param name="pNameAmount" select="'FaceAmt'"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name="vVrMrgnCcy">
        <xsl:choose>
          <xsl:when test="data[@name='VrMrgnCcy']">
            <xsl:value-of select="data[@name='VrMrgnCcy']"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="data[@name='Ccy']"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vVrMrgnEnable">
        <xsl:call-template name="IsValidAmount">
          <xsl:with-param name="pNameAmount" select="'VrMrgnAmt'"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name="vUMgCcy">
        <xsl:choose>
          <xsl:when test="data[@name='UMgCcy']">
            <xsl:value-of select="data[@name='UMgCcy']"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="data[@name='Ccy']"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vUMgEnable">
        <xsl:call-template name="IsValidAmount">
          <xsl:with-param name="pNameAmount" select="'UMgAmt'"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name="vCollCcy">
        <xsl:choose>
          <xsl:when test="data[@name='CollCcy']">
            <xsl:value-of select="data[@name='CollCcy']"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="data[@name='Ccy']"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vCollEnable">
        <xsl:call-template name="IsValidAmount">
          <xsl:with-param name="pNameAmount" select="'CollAmt'"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name="vOPPCcy">
        <xsl:choose>
          <xsl:when test="data[@name='OPPCcy']">
            <xsl:value-of select="data[@name='OPPCcy']"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="data[@name='Ccy']"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vOPPEnable">
        <xsl:call-template name="IsValidAmount">
          <xsl:with-param name="pNameAmount" select="'OPPAmt'"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name="vRptCcy">
        <xsl:choose>
          <xsl:when test="data[@name='RptCcy']">
            <xsl:value-of select="data[@name='RptCcy']"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="data[@name='Ccy']"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vRptEnable">
        <xsl:call-template name="IsValidAmount">
          <xsl:with-param name="pNameAmount" select="'RptAmt'"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name="vPmtCcy">
        <xsl:choose>
          <xsl:when test="data[@name='PmtCcy']">
            <xsl:value-of select="data[@name='PmtCcy']"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="data[@name='Ccy']"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vPmtEnable">
        <xsl:call-template name="IsValidAmount">
          <xsl:with-param name="pNameAmount" select="'PmtAmt'"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name="vTaxComCcy">
        <xsl:choose>
          <xsl:when test="data[@name='TaxComCcy']">
            <xsl:value-of select="data[@name='TaxComCcy']"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="data[@name='Ccy']"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vTaxComEnable">
        <xsl:call-template name="IsValidAmount">
          <xsl:with-param name="pNameAmount" select="'TaxComAmt'"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name="vTaxBrkCcy">
        <xsl:choose>
          <xsl:when test="data[@name='TaxBrkCcy']">
            <xsl:value-of select="data[@name='TaxBrkCcy']"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="data[@name='Ccy']"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vTaxBrkEnable">
        <xsl:call-template name="IsValidAmount">
          <xsl:with-param name="pNameAmount" select="'TaxBrkAmt'"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name="vStlCcy">
        <xsl:choose>
          <xsl:when test="data[@name='StlCcy']">
            <xsl:value-of select="data[@name='StlCcy']"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="data[@name='Ccy']"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vStlEnable">
        <xsl:call-template name="IsValidAmount">
          <xsl:with-param name="pNameAmount" select="'StlAmt'"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name="vTaxComBrkCcy">
        <xsl:choose>
          <xsl:when test="data[@name='TaxComBrkCcy']">
            <xsl:value-of select="data[@name='TaxComBrkCcy']"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="data[@name='Ccy']"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vTaxComBrkEnable">
        <xsl:call-template name="IsValidAmount">
          <xsl:with-param name="pNameAmount" select="'TaxComBrkAmt'"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name="vRMgCcy">
        <xsl:choose>
          <xsl:when test="data[@name='RMgCcy']">
            <xsl:value-of select="data[@name='RMgCcy']"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="data[@name='Ccy']"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vRMgEnable">
        <xsl:call-template name="IsValidAmount">
          <xsl:with-param name="pNameAmount" select="'RMgAmt'"/>
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name="vLovCcy">
        <xsl:choose>
          <xsl:when test="data[@name='LovCcy']">
            <xsl:value-of select="data[@name='LovCcy']"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="data[@name='Ccy']"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vLovEnable">
        <xsl:call-template name="IsValidAmount">
          <xsl:with-param name="pNameAmount" select="'LovAmt'"/>
        </xsl:call-template>
      </xsl:variable>

      <!-- Build a generic Rpt node for the specific CashFlow type -->
      <xsl:variable name="vRpt">
        <xsl:call-template name="BuildAcctRpt">
          <xsl:with-param name="pDtAndTm" select="data[@name='DtAndTm']"/>
          <xsl:with-param name="pExistsMrk" select="$vEnrichedExch"/>
          <xsl:with-param name="pMrk" select="concat('parameters.', $gbnEnrichedExch)"/>
          <xsl:with-param name="pAcct" select="concat('parameters.', $gbnEnrichedAcct)"/>
          <xsl:with-param name="pAcctTyp" select="$vTCAcctTyp"/>
          <xsl:with-param name="pPutCall" select="$vTCPutCall"/>
          <xsl:with-param name="pExistsSym" select="$vTCSym"/>
          <xsl:with-param name="pSym" select="concat('parameters.', $gbnEnrichedSym)"/>

          <xsl:with-param name="pPrmAmt" select="data[@name='PrmAmt']"/>
          <xsl:with-param name="pPrmCcy" select="$vPrmCcy"/>
          <xsl:with-param name="pPrmEnable" select="$vPrmEnable"/>

          <xsl:with-param name="pCallAmt" select="data[@name='CallAmt']"/>
          <xsl:with-param name="pCallCcy" select="$vCallCcy"/>
          <xsl:with-param name="pCallEnable" select="$vCallEnable"/>

          <xsl:with-param name="pFaceAmt" select="data[@name='FaceAmt']"/>
          <xsl:with-param name="pFaceCcy" select="$vFaceCcy"/>
          <xsl:with-param name="pFaceEnable" select="$vFaceEnable"/>

          <xsl:with-param name="pVrMrgnAmt" select="data[@name='VrMrgnAmt']"/>
          <xsl:with-param name="pVrMrgnCcy" select="$vVrMrgnCcy"/>
          <xsl:with-param name="pVrMrgnEnable" select="$vVrMrgnEnable"/>

          <xsl:with-param name="pUMgAmt" select="data[@name='UMgAmt']"/>
          <xsl:with-param name="pUMgCcy" select="$vUMgCcy"/>
          <xsl:with-param name="pUMgEnable" select="$vUMgEnable"/>

          <xsl:with-param name="pCollAmt" select="data[@name='CollAmt']"/>
          <xsl:with-param name="pCollCcy" select="$vCollCcy"/>
          <xsl:with-param name="pCollEnable" select="$vCollEnable"/>

          <xsl:with-param name="pOPPAmt" select="data[@name='OPPAmt']"/>
          <xsl:with-param name="pOPPCcy" select="$vOPPCcy"/>
          <xsl:with-param name="pOPPEnable" select="$vOPPEnable"/>

          <xsl:with-param name="pRptAmt" select="data[@name='RptAmt']"/>
          <xsl:with-param name="pRptCcy" select="$vRptCcy"/>
          <xsl:with-param name="pRptEnable" select="$vRptEnable"/>

          <xsl:with-param name="pPmtAmt" select="data[@name='PmtAmt']"/>
          <xsl:with-param name="pPmtCcy" select="$vPmtCcy"/>
          <xsl:with-param name="pPmtEnable" select="$vPmtEnable"/>

          <xsl:with-param name="pTaxComAmt" select="data[@name='TaxComAmt']"/>
          <xsl:with-param name="pTaxComCcy" select="$vTaxComCcy"/>
          <xsl:with-param name="pTaxComEnable" select="$vTaxComEnable"/>

          <xsl:with-param name="pTaxBrkAmt" select="data[@name='TaxBrkAmt']"/>
          <xsl:with-param name="pTaxBrkCcy" select="$vTaxBrkCcy"/>
          <xsl:with-param name="pTaxBrkEnable" select="$vTaxBrkEnable"/>

          <xsl:with-param name="pStlAmt" select="data[@name='StlAmt']"/>
          <xsl:with-param name="pStlCcy" select="$vStlCcy"/>
          <xsl:with-param name="pStlEnable" select="$vStlEnable"/>

          <xsl:with-param name="pTaxComBrkAmt" select="data[@name='TaxComBrkAmt']"/>
          <xsl:with-param name="pTaxComBrkCcy" select="$vTaxComBrkCcy"/>
          <xsl:with-param name="pTaxComBrkEnable" select="$vTaxComBrkEnable"/>

          <xsl:with-param name="pRMgAmt" select="data[@name='RMgAmt']"/>
          <xsl:with-param name="pRMgCcy" select="$vRMgCcy"/>
          <xsl:with-param name="pRMgEnable" select="$vRMgEnable"/>

          <xsl:with-param name="pLovAmt" select="data[@name='LovAmt']"/>
          <xsl:with-param name="pLovCcy" select="$vLovCcy"/>
          <xsl:with-param name="pLovEnable" select="$vLovEnable"/>

        </xsl:call-template>
      </xsl:variable>

      <xsl:call-template name="EXTLDATADET">
        <xsl:with-param name="pIsWithControl" select="true()"/>
        <xsl:with-param name="pMessage" select="$NULL"/>
        <xsl:with-param name="pDataRowNumber" select="@src"/>
        <xsl:with-param name="pDataInput" select="$NULL"/>
        <xsl:with-param name="pDataXML">

          <xsl:copy-of select="$vRpt"/>

        </xsl:with-param>
        <xsl:with-param name="pDtData" select="data[@name='DtAndTm']"/>
        <xsl:with-param name="pDtBusinessParam" select="$pDtBusinessParam"/>
        <xsl:with-param name="pDtBusinessTrade" select="data[@name='DtAndTm']"/>

      </xsl:call-template>

    </row>

  </xsl:template>

</xsl:stylesheet>