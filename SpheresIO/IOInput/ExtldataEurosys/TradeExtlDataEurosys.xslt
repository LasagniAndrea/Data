<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="xml" indent="yes"/>

  <!--
==============================================================
 Summary : Test mapping for example files containing Fix trades on EUROSYS®
==============================================================
 Version : v1.0.0.0                                           
 Date    : 20100220                                           
 Author  : MF                                                   
==============================================================
-->

  <xsl:include href="..\Common\CommonInput.xslt"/>
  <xsl:include href="CommonExtldataEurosys.xslt"/>

  <xsl:variable name="vBusinessType">
    <xsl:choose>
      <xsl:when test="/iotask/parameters/parameter[@id='InternalDataTypeEnum']">
        <xsl:value-of select="/iotask/parameters/parameter[@id='InternalDataTypeEnum']"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'TradeFIXml'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="vEntityString">
    <xsl:choose>
      <xsl:when test="/iotask/parameters/parameter[@id='EntityIdentifier']">
        <xsl:value-of select="/iotask/parameters/parameter[@id='EntityIdentifier']"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'N/A'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- match the file node -->
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

      <!-- match any row node inside the actual file node -->
      <xsl:apply-templates select="row">
        <xsl:with-param name="pSelectIDExtlData">
          <xsl:copy-of select="$vSelectIDExtlData"/>
        </xsl:with-param>
        <xsl:with-param name="pDtBusinessParam" select="/iotask/parameters/parameter[@id='DTBUSINESS']"/>
      </xsl:apply-templates>

    </file>
  </xsl:template>

  <!-- match any row node -->
  <xsl:template match="row">
    <xsl:param name="pSelectIDExtlData" select="$NULL"/>
    <xsl:param name="pDtBusinessParam" select="$NULL"/>

    <row>
      <xsl:call-template name="IORowAtt"/>

      <!-- Count how many market matches the given EXTL_Exch -->
      <xsl:variable name="vEnrichedExch">
        <xsl:call-template name="SelectEnrichedExch">
          <xsl:with-param name="pNames" select="concat('EXTL_Exch',',','NOTFOUND',',','NOTUNIQUE')"/>
          <xsl:with-param name="pValues" select="concat(data[@name='Exch'],',',$cNOTFOUND,',',$cNOTUNIQUE)"/>
        </xsl:call-template>
      </xsl:variable>

      <!-- PL 20101213 Update -->
      <!-- Count how many product matches the given EXTL_Sym -->
      <xsl:variable name="vEnrichedSym">
        <xsl:call-template name="SelectEnrichedSym">
          <xsl:with-param name="pNames" select="concat('EXTL_Sym',',','NOTFOUND',',','NOTUNIQUE',',','PUTCALL')"/>
          <xsl:with-param name="pValues" select="concat(data[@name='Sym'],',',$cNOTFOUND,',',$cNOTUNIQUE,',',data[@name='PutCall'])"/>
        </xsl:call-template>
      </xsl:variable>

      <!-- Transcode the PutCall external code : P -> 0 ; C -> 1 ; Other -> null -->
      <xsl:variable name="vTCPutCall">
        <xsl:call-template name="TCPutCall">
          <xsl:with-param name="pPutCall" select="data[@name='PutCall']"/>
        </xsl:call-template>
      </xsl:variable>

      <!-- Transcode the StrkPx value : (if empty) -> 0 ; (if >= 0) -> StrkPx -->
      <xsl:variable name="vTCStrkPx">
        <xsl:call-template name="TCStrkPx">
          <xsl:with-param name="pStrkPx">
            <xsl:value-of select="number(data[@name='StrkPx'])"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:variable>

      <!-- Transcode the MMY code  -->
      <xsl:variable name="vTCMMY">
        <xsl:call-template name="TCMMY">
          <xsl:with-param name="pMMY">
            <xsl:value-of select="data[@name='MMY']"/>
          </xsl:with-param>
          <xsl:with-param name="pBizDt">
            <xsl:value-of select="data[@name='BizDt']"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:variable>

      <!-- Transcode the Side value : 'B' -> 1 ; 'S' -> 2 -->
      <xsl:variable name="vTCSide">
        <xsl:call-template name="TCSide">
          <xsl:with-param name="pSide">
            <xsl:value-of select="data[@name='Side']"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:variable>

      <!-- Count how many Buyer/Seller Account matches the given EXTL_Acct -->
      <xsl:variable name="vEnrichedAcct">
        <xsl:call-template name="SelectEnrichedAcct">
          <xsl:with-param name="pNames" select="concat('EXTL_Acct',',','NOTFOUND',',','NOTUNIQUE')"/>
          <xsl:with-param name="pValues" select="concat(data[@name='Acct'],',',$cNOTFOUND,',',$cNOTUNIQUE)"/>
        </xsl:call-template>
      </xsl:variable>

      <!-- Transcode the Side value : 'B' -> 1 ; 'S' -> 2 -->
      <xsl:variable name="vTCAcctTyp">
        <xsl:call-template name="TCAcctTyp">
          <xsl:with-param name="pAcctTyp">
            <xsl:value-of select="data[@name='AcctTyp']"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:variable>

      <xsl:variable name="vExistsPty.ID.4">
        <xsl:choose>
          <xsl:when test="data[@name='Pty.ID.4'] != ''">
            <xsl:value-of select="true()"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="false()"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <!-- Count how many Clearing Broker Account/Pty.ID.4 matches the given EXTL_PtyID4 -->
      <xsl:variable name="vEnrichedPty.ID.4">
        <xsl:if test="$vExistsPty.ID.4 = 'true'">
          <xsl:call-template name="SelectEnrichedPty.ID.4">
            <xsl:with-param name="pNames" select="concat('EXTL_PtyID4',',','NOTFOUND',',','NOTUNIQUE')"/>
            <xsl:with-param name="pValues" select="concat(data[@name='Pty.ID.4'],',',$cNOTFOUND,',',$cNOTUNIQUE)"/>
          </xsl:call-template>
        </xsl:if>
      </xsl:variable>

      <!-- Build TradeCpt Fix Node -->
      <xsl:variable name="vTrdCaptRptNode">
        <xsl:call-template name="BuildTrdCaptRptNode">
          <xsl:with-param name="TxnTm" select="data[@name='TxnTm']"/>
          <xsl:with-param name="BizDt" select="data[@name='BizDt']"/>
          <xsl:with-param name="LastQty" select="data[@name='LastQty']"/>
          <xsl:with-param name="LastPx" select="data[@name='LastPx']"/>
          <!-- ref : SelectCountIDMFromEXTLLINK -->
          <xsl:with-param name="Exch" select="concat('parameters.', $gbnEnrichedExch)"/>
          <xsl:with-param name="Sym" select="concat('parameters.', $gbnEnrichedSym)"/>
          <xsl:with-param name="PutCall" select="$vTCPutCall"/>
          <xsl:with-param name="StrkPx" select="$vTCStrkPx"/>
          <xsl:with-param name="MMY" select="$vTCMMY"/>
          <xsl:with-param name="Side" select="$vTCSide"/>
          <xsl:with-param name="Acct" select="concat('parameters.', $gbnEnrichedAcct)"/>
          <xsl:with-param name="AcctTyp" select="$vTCAcctTyp"/>
          <xsl:with-param name="PosEfct" select="data[@name='PosEfct']"/>
          <xsl:with-param name="AcctIDSrcTyp" select="$vAcctIDSrcTyp"/>
          <!-- xsl:with-param name="Pty.ID.27" select="concat('parameters.', $gbnEnrichedPty.ID.27)"/ -->
          <xsl:with-param name="Pty.ID.4" select="concat('parameters.', $gbnEnrichedPty.ID.4)"/>
          <xsl:with-param name="ExistsPty.ID.4" select="$vExistsPty.ID.4"/>
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
          <xsl:with-param name="pParamName" select="$gbnEnrichedSym"/>
          <xsl:with-param name="pParamValue">
            <xsl:copy-of select="$vEnrichedSym"/>
          </xsl:with-param>
        </xsl:call-template>
        <xsl:call-template name="BuildGlobalParameter">
          <xsl:with-param name="pParamName" select="$gbnEnrichedAcct"/>
          <xsl:with-param name="pParamValue">
            <xsl:copy-of select="$vEnrichedAcct"/>
          </xsl:with-param>
        </xsl:call-template>
        <xsl:if test="$vExistsPty.ID.4 = 'true'">
          <xsl:call-template name="BuildGlobalParameter">
            <xsl:with-param name="pParamName" select="$gbnEnrichedPty.ID.4"/>
            <xsl:with-param name="pParamValue">
              <xsl:copy-of select="$vEnrichedPty.ID.4"/>
            </xsl:with-param>
          </xsl:call-template>
        </xsl:if>
      </parameters>

      <xsl:call-template name="EXTLDATADET">
        <xsl:with-param name="pIsWithControl" select="true()"/>
        <xsl:with-param name="pMessage" select="$NULL"/>
        <xsl:with-param name="pDataRowNumber" select="@src"/>
        <xsl:with-param name="pDataInput" select="$NULL"/>
        <xsl:with-param name="pDataXML">

          <xsl:copy-of select="$vTrdCaptRptNode"/>

        </xsl:with-param>
        <xsl:with-param name="pDtData" select="data[@name='TxnTm']"/>
        <!--<xsl:with-param name="pSelectIDExtlData">
          
          <xsl:copy-of select="$pSelectIDExtlData"/>
          
        </xsl:with-param>-->
        <xsl:with-param name="pDtBusinessParam" select="$pDtBusinessParam"/>
        <xsl:with-param name="pDtBusinessTrade" select="data[@name='BizDt']"/>

      </xsl:call-template>

    </row>

  </xsl:template>

</xsl:stylesheet>

