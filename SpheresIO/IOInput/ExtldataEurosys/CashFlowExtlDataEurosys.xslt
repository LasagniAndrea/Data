<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="xml" indent="yes"/>

  <!--
==============================================================
 Summary : Default Spheres EUROSYS/Vision mapping for files containing financial fluxes ('CashFlow' and extentions) for compare purpose
==============================================================
 Version : v1.0.0.0                                           
 Date    : 201202...                                           
 Author  : MF                                                   
==============================================================
-->

  <xsl:include href="..\Common\CommonInput.xslt"/>
  <xsl:include href="CommonExtldataEurosys.xslt"/>

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

  <!-- IO Parameter transcode modality for external account values, 
  look at the SelectEnrichedAcct template to see as this parameter drives the transcode process  -->
  <xsl:variable name="vTCAcctMode">
    <xsl:choose>
      <xsl:when test="/iotask/parameters/parameter[@id='TCACCTMODE']">
        <xsl:value-of select="/iotask/parameters/parameter[@id='TCACCTMODE']"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$NULL"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- IO Parameter transcode modality for external market codes, 
  look at the SelectEnrichedExch template to see as this parameter drives the transcode process  -->
  <xsl:variable name="vTCExchMode">
    <xsl:choose>
      <xsl:when test="/iotask/parameters/parameter[@id='TCEXCHMODE']">
        <xsl:value-of select="/iotask/parameters/parameter[@id='TCEXCHMODE']"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$NULL"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- IO Parameter transcode priority source for external market codes, 
  look at the SelectEnrichedExch template to see as this parameter drives the transcode process  -->
  <xsl:variable name="vTCExchSource">
    <xsl:choose>
      <xsl:when test="/iotask/parameters/parameter[@id='TCEXCHSOURCE']">
        <xsl:value-of select="/iotask/parameters/parameter[@id='TCEXCHSOURCE']"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$NULL"/>
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

  <!-- match any "row" node, where we create an EXTLDATADET element for each parsed row  -->
  <xsl:template match="row">
    <xsl:param name="pSelectIDExtlData" select="$NULL"/>
    <xsl:param name="pDtBusinessParam" select="$NULL"/>

    <row>
      <xsl:call-template name="IORowAtt"/>

      <xsl:variable name="vSourceDefined">
        <xsl:choose>
          <xsl:when test="$vTCExchSource != $NULL">
            <xsl:value-of select="'true'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'false'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vNamesEnrichedExch">
        <xsl:choose>
          <xsl:when test="$vTCExchSource != $NULL">
            <xsl:value-of select="concat('SOURCE',',','EXTL_Exch',',','NOTFOUND',',','NOTUNIQUE')"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="concat('EXTL_Exch',',','NOTFOUND',',','NOTUNIQUE')"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vValuesEnrichedExch">
        <xsl:choose>
          <xsl:when test="$vTCExchSource != $NULL">
            <xsl:value-of select="concat($vTCExchSource,',',data[@name='Mrk'],',',$cNOTFOUND,',',$cNOTUNIQUE)"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="concat(data[@name='Mrk'],',',$cNOTFOUND,',',$cNOTUNIQUE)"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      
      <!-- Count how many market matches the given EXTL_Exch -->
      <xsl:variable name="vEnrichedExch">
        <xsl:choose>
          <xsl:when test="data[@name='Mrk']">
            <xsl:call-template name="SelectEnrichedExch">
              <xsl:with-param name="pMode" select="$vTCExchMode"/>
              <xsl:with-param name="pSourceDefined" select="$vSourceDefined"/>
              <xsl:with-param name="pNames" select="$vNamesEnrichedExch"/>
              <xsl:with-param name="pValues" select="$vValuesEnrichedExch"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$NULL"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <!-- Count how many books match the given EXTL_Acct  -->
      <xsl:variable name="vEnrichedAcct">
        <xsl:call-template name="SelectEnrichedAcct">
          <xsl:with-param name="pMode" select="$vTCAcctMode"/>
          <xsl:with-param name="pNames" select="concat('EXTL_Acct',',','NOTFOUND',',','NOTUNIQUE')"/>
          <xsl:with-param name="pValues" select="concat(data[@name='Acct'],',',$cNOTFOUND,',',$cNOTUNIQUE)"/>
        </xsl:call-template>
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
      
      <!-- Date -->
      <xsl:variable name="vDtAndTm">
        <!-- 20120209 MF replacez avec data[@name='DtAndTm'] à peine un template sera disponible pour traduire cette donnée en format ISO, 
        on force la date business pour ne pas rater le controle lors de l'insertion dans EXTLDATADET -->
        <!--<xsl:value-of select="data[@name='DtAndTm']"/>pDtBusinessParam-->
        <xsl:value-of select="$pDtBusinessParam"/>
      </xsl:variable>


      <!-- Build a generic Rpt node for the specific CashFlow type -->
      <xsl:variable name="vRpt">
        <xsl:call-template name="BuildAcctRpt">
          <xsl:with-param name="pDtAndTm" select="$vDtAndTm"/>
          <xsl:with-param name="pExistsMrk" select="$vEnrichedExch"/>
          <xsl:with-param name="pMrk" select="concat('parameters.', $gbnEnrichedExch)"/>
          <xsl:with-param name="pAcct" select="concat('parameters.', $gbnEnrichedAcct)"/>
          <xsl:with-param name="pAcctTyp" select="$vTCAcctTyp"/>
          <xsl:with-param name="pPutCall" select="$NULL"/>
          <xsl:with-param name="pExistsSym" select="$NULL"/>
          <xsl:with-param name="pSym" select="$NULL"/>

          <xsl:with-param name="pPrmAmt" select="data[@name='PrmAmt']"/>
          <xsl:with-param name="pPrmCcy" select="$vPrmCcy"/>

          <xsl:with-param name="pCallAmt" select="data[@name='CallAmt']"/>
          <xsl:with-param name="pCallCcy" select="$vCallCcy"/>

          <xsl:with-param name="pFaceAmt" select="data[@name='FaceAmt']"/>
          <xsl:with-param name="pFaceCcy" select="$vFaceCcy"/>

          <xsl:with-param name="pVrMrgnAmt" select="data[@name='VrMrgnAmt']"/>
          <xsl:with-param name="pVrMrgnCcy" select="$vVrMrgnCcy"/>

          <xsl:with-param name="pUMgAmt" select="data[@name='UMgAmt']"/>
          <xsl:with-param name="pUMgCcy" select="$vUMgCcy"/>

          <xsl:with-param name="pCollAmt" select="data[@name='CollAmt']"/>
          <xsl:with-param name="pCollCcy" select="$vCollCcy"/>

          <xsl:with-param name="pOPPAmt" select="data[@name='OPPAmt']"/>
          <xsl:with-param name="pOPPCcy" select="$vOPPCcy"/>

          <xsl:with-param name="pRptAmt" select="data[@name='RptAmt']"/>
          <xsl:with-param name="pRptCcy" select="$vRptCcy"/>

          <xsl:with-param name="pPmtAmt" select="data[@name='PmtAmt']"/>
          <xsl:with-param name="pPmtCcy" select="$vPmtCcy"/>

          <xsl:with-param name="pTaxComAmt" select="data[@name='TaxComAmt']"/>
          <xsl:with-param name="pTaxComCcy" select="$vTaxComCcy"/>

          <xsl:with-param name="pTaxBrkAmt" select="data[@name='TaxBrkAmt']"/>
          <xsl:with-param name="pTaxBrkCcy" select="$vTaxBrkCcy"/>

          <xsl:with-param name="pStlAmt" select="data[@name='StlAmt']"/>
          <xsl:with-param name="pStlCcy" select="$vStlCcy"/>

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
        <xsl:with-param name="pDtData" select="$vDtAndTm"/>
        <xsl:with-param name="pDtBusinessParam" select="$pDtBusinessParam"/>
        <xsl:with-param name="pDtBusinessTrade" select="$vDtAndTm"/>

      </xsl:call-template>

    </row>

  </xsl:template>

</xsl:stylesheet>