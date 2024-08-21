<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="xml" indent="yes"/>

<!--
  ==============================================================
   Summary : Common templates to import data from an external source                                           
  ==============================================================
   Version : v1.0.0.0                                           
   Date    : 20100220                                           
   Author  : MF                                                 
  ==============================================================
-->

<!--
  *******************************
  COMMON BUILDING SECTIONS TEMPLATES - 
  building the initial sections of the XSL mapping file (iotask/iotaskdet/ioinput)
  *******************************
  -->

  <!-- Create the Task attributes -->
  <xsl:template name="IOTaskAtt">
    <xsl:attribute name="id">
      <xsl:value-of select="@id"/>
    </xsl:attribute>
    <xsl:attribute name="name">
      <xsl:value-of select="@name"/>
    </xsl:attribute>
    <xsl:attribute name="displayname">
      <xsl:value-of select="@displayname"/>
    </xsl:attribute>
    <xsl:attribute name="loglevel">
      <xsl:value-of select="@loglevel"/>
    </xsl:attribute>
    <xsl:attribute name="commitmode">
      <xsl:value-of select="@commitmode"/>
    </xsl:attribute>
  </xsl:template>

  <!--Main template  -->
  <xsl:template match="/iotask">
    <iotask>
      <xsl:call-template name="IOTaskAtt"/>

      <!-- searching parameters node (could not exist)-->
      <xsl:apply-templates select="parameters"/>

      <!-- searching the task details node -->
      <xsl:apply-templates select="iotaskdet"/>

    </iotask>
  </xsl:template>

  <xsl:template match="parameters">
    <parameters>
      <xsl:for-each select="parameter">
        <parameter>
          <xsl:attribute name="id">
            <xsl:value-of select="@id"/>
          </xsl:attribute>
          <xsl:attribute name="name">
            <xsl:value-of select="@name"/>
          </xsl:attribute>
          <xsl:attribute name="displayname">
            <xsl:value-of select="@displayname"/>
          </xsl:attribute>
          <xsl:attribute name="direction">
            <xsl:value-of select="@direction"/>
          </xsl:attribute>
          <xsl:attribute name="datatype">
            <xsl:value-of select="@datatype"/>
          </xsl:attribute>
          <xsl:value-of select="."/>
        </parameter>
      </xsl:for-each>
    </parameters>
  </xsl:template>

  <!-- Create the Task details attributes -->
  <xsl:template name="IOTaskDetAtt">
    <xsl:attribute name="id">
      <xsl:value-of select="@id"/>
    </xsl:attribute>
    <xsl:attribute name="loglevel">
      <xsl:value-of select="@loglevel"/>
    </xsl:attribute>
    <xsl:attribute name="commitmode">
      <xsl:value-of select="@commitmode"/>
    </xsl:attribute>
  </xsl:template>

  <xsl:template match="iotaskdet">
    <iotaskdet>
      <xsl:call-template name="IOTaskDetAtt"/>

      <!-- searching the import information node -->
      <xsl:apply-templates select="ioinput"/>

    </iotaskdet>
  </xsl:template>

  <!-- Create the Import information attributes -->
  <xsl:template name="IOInputAtt">
    <xsl:attribute name="id">
      <xsl:value-of select="@id"/>
    </xsl:attribute>
    <xsl:attribute name="name">
      <xsl:value-of select="@name"/>
    </xsl:attribute>
    <xsl:attribute name="displayname">
      <xsl:value-of select="@displayname"/>
    </xsl:attribute>
    <xsl:attribute name="loglevel">
      <xsl:value-of select="@loglevel"/>
    </xsl:attribute>
    <xsl:attribute name="commitmode">
      <xsl:value-of select="@commitmode"/>
    </xsl:attribute>
  </xsl:template>

  <xsl:template match="ioinput">
    <ioinput>
      <xsl:call-template name="IOInputAtt"/>

      <!-- searching the file content node -->
      <xsl:apply-templates select="file"/>

    </ioinput>
  </xsl:template>

  <!-- Create the File attributes -->
  <xsl:template name="IOFileAtt">
    <xsl:attribute name="name">
      <xsl:value-of select="@name"/>
    </xsl:attribute>
    <xsl:attribute name="folder">
      <xsl:value-of select="@folder"/>
    </xsl:attribute>
    <xsl:attribute name="date">
      <xsl:value-of select="@date"/>
    </xsl:attribute>
    <xsl:attribute name="size">
      <xsl:value-of select="@size"/>
    </xsl:attribute>
  </xsl:template>

  <!-- Create the Row attributes (it is mandatory to call this template after the row node declaration in your specific importation) -->
  <xsl:template name="IORowAtt">
    <xsl:attribute name="id">
      <xsl:value-of select="@id"/>
    </xsl:attribute>
    <xsl:attribute name="src">
      <xsl:value-of select="@src"/>
    </xsl:attribute>
  </xsl:template>

  <!--
  *******************************
  HELPER BUILD XML TEMPLATES
  *******************************
  -->

  <!-- Build a parameter node to put inside the global parameters collection -->
  <xsl:template name="BuildGlobalParameter">
    <!-- parameter name - mandatory -->
    <xsl:param name="pParamName"/>
    <!-- parameter value - mandatory -->
    <xsl:param name="pParamValue"/>
    <!-- parameter format - optional (for datetime parameter value only, e.g. 'yyyyMMdd') -->
    <xsl:param name="pParamDataFormat"/>
    <parameter>
      <xsl:attribute name="name">
        <xsl:value-of select="$pParamName"/>
      </xsl:attribute>
      <!-- dataformat  -->
      <xsl:if test="$pParamDataFormat != ''">
        <xsl:attribute name="name">
          <xsl:value-of select="$pParamDataFormat"/>
        </xsl:attribute>
      </xsl:if>
      <!-- value -->
      <xsl:copy-of select="$pParamValue"/>
    </parameter>
  </xsl:template>

  <!-- Build a fixml:TrdCaptRpt node  -->
  <xsl:template name="BuildTrdCaptRptNode">
    <xsl:param name="TxnTm"/>
    <xsl:param name="BizDt"/>
    <xsl:param name="LastQty"/>
    <xsl:param name="LastPx"/>
    <xsl:param name="CountExch"/>
    <xsl:param name="Exch"/>
    <xsl:param name="Sym"/>
    <xsl:param name="PutCall"/>
    <xsl:param name="StrkPx"/>
    <xsl:param name="MMY"/>
    <xsl:param name="Side"/>
    <xsl:param name="Acct"/>
    <xsl:param name="AcctTyp"/>
    <xsl:param name="PosEfct"/>
    <xsl:param name="AcctIDSrcTyp"/>
    <xsl:param name="Pty.ID.27"/>
    <xsl:param name="Pty.ID.4"/>
    <xsl:param name="ExistsPty.ID.4"/>
    <ValueXML
        xmlns:fixml="http://www.fixprotocol.org/FIXML-5-0-SP1">
      <fixml:TrdCaptRpt>
        <xsl:attribute name="LastPx">
          <xsl:value-of select="$LastPx"/>
        </xsl:attribute>
        <xsl:attribute name="LastQty">
          <xsl:value-of select="$LastQty"/>
        </xsl:attribute>
        <xsl:attribute name="BizDt">
          <xsl:value-of select="$BizDt"/>
        </xsl:attribute>
        <xsl:attribute name="BizDt">
          <xsl:value-of select="$BizDt"/>
        </xsl:attribute>
        <xsl:attribute name="TxnTm">
          <xsl:value-of select="$TxnTm"/>
        </xsl:attribute>
        <fixml:Instrmt>
          <xsl:attribute name="MMY">
            <xsl:value-of select="$MMY"/>
          </xsl:attribute>
          <xsl:attribute name="Sym">
            <xsl:value-of select="$Sym"/>
          </xsl:attribute>
          <xsl:attribute name="Exch">
            <xsl:value-of select="$Exch"/>
          </xsl:attribute>
          <xsl:if test="number($StrkPx) > 0">
            <xsl:attribute name="StrkPx">
              <xsl:value-of select="$StrkPx"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="number($PutCall) = 0 or number($PutCall) = 1">
            <xsl:attribute name="PutCall">
              <xsl:value-of select="$PutCall"/>
            </xsl:attribute>
          </xsl:if>
        </fixml:Instrmt>
        <fixml:RptSide>
          <xsl:attribute name="Acct">
            <xsl:value-of select="$Acct"/>
          </xsl:attribute>
          <xsl:attribute name="Side">
            <xsl:value-of select="$Side"/>
          </xsl:attribute>
          <xsl:attribute name="AcctTyp">
            <xsl:value-of select="$AcctTyp"/>
          </xsl:attribute>
          <xsl:attribute name="PosEfct">
            <xsl:value-of select="$PosEfct"/>
          </xsl:attribute>
          <xsl:attribute name="AcctIDSrcTyp">
            <xsl:value-of select="$AcctIDSrcTyp"/>
          </xsl:attribute>
          <fixml:Pty Src="D" R="27">
            <xsl:attribute name="ID">
              <xsl:value-of select="$Pty.ID.27"/>
            </xsl:attribute>
          </fixml:Pty>
          <xsl:if test="$ExistsPty.ID.4 = 'true'">
            <fixml:Pty Src="D" R="4">
              <xsl:attribute name="ID">
                <xsl:value-of select="$Pty.ID.4"/>
              </xsl:attribute>
            </fixml:Pty>
          </xsl:if>
        </fixml:RptSide>
      </fixml:TrdCaptRpt>
    </ValueXML>
  </xsl:template>

  <!-- Build a fixml:PosRpt node  -->
  <xsl:template name="BuildPosRptNode">
    <xsl:param name="BizDt"/>
    <xsl:param name="Exch"/>
    <xsl:param name="Sym"/>
    <xsl:param name="PutCall"/>
    <xsl:param name="StrkPx"/>
    <xsl:param name="MMY"/>
    <xsl:param name="LongQty"/>
    <xsl:param name="ShortQty"/>
    <xsl:param name="QtyTyp" select="'FIN'"/>
    <xsl:param name="Acct"/>
    <xsl:param name="AcctTyp"/>
    <xsl:param name="AcctIDSrcTyp"/>
    <xsl:param name="Pty.ID.27"/>
    <xsl:param name="Pty.ID.4"/>
    <xsl:param name="ExistsPty.ID.4"/>
    <ValueXML
        xmlns:fixml="http://www.fixprotocol.org/FIXML-5-0-SP1">
      <fixml:PosRpt>
        <xsl:attribute name="BizDt">
          <xsl:value-of select="$BizDt"/>
        </xsl:attribute>
        <xsl:attribute name="Acct">
          <xsl:value-of select="$Acct"/>
        </xsl:attribute>
        <xsl:attribute name="AcctTyp">
          <xsl:value-of select="$AcctTyp"/>
        </xsl:attribute>
        <xsl:attribute name="AcctIDSrcTyp">
          <xsl:value-of select="$AcctIDSrcTyp"/>
        </xsl:attribute>
        <fixml:Pty Src="D" R="27">
          <xsl:attribute name="ID">
            <xsl:value-of select="$Pty.ID.27"/>
          </xsl:attribute>
        </fixml:Pty>
        <xsl:if test="$ExistsPty.ID.4 = 'true'">
          <fixml:Pty Src="D" R="4">
            <xsl:attribute name="ID">
              <xsl:value-of select="$Pty.ID.4"/>
            </xsl:attribute>
          </fixml:Pty>
        </xsl:if>
        <fixml:Instrmt>
          <xsl:if test="number($StrkPx) > 0">
            <xsl:attribute name="StrkPx">
              <xsl:value-of select="$StrkPx"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="number($PutCall) = 0 or number($PutCall) = 1">
            <xsl:attribute name="PutCall">
              <xsl:value-of select="$PutCall"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:attribute name="MMY">
            <xsl:value-of select="$MMY"/>
          </xsl:attribute>
          <xsl:attribute name="Sym">
            <xsl:value-of select="$Sym"/>
          </xsl:attribute>
          <xsl:attribute name="Exch">
            <xsl:value-of select="$Exch"/>
          </xsl:attribute>
        </fixml:Instrmt>
        <fixml:Qty>
          <xsl:attribute name="Typ">
            <xsl:value-of select="$QtyTyp"/>
          </xsl:attribute>
          <xsl:attribute name="Long">
            <xsl:value-of select="$LongQty"/>
          </xsl:attribute>
          <xsl:attribute name="Short">
            <xsl:value-of select="$ShortQty"/>
          </xsl:attribute>
        </fixml:Qty>
      </fixml:PosRpt>
    </ValueXML>
  </xsl:template>

  <!-- Build a AcctRpt node  -->
  <xsl:template name="BuildAcctRpt">
    <xsl:param name="pDtAndTm"/>
    <xsl:param name="pExistsMrk"/>
    <xsl:param name="pMrk"/>
    <xsl:param name="pAcct"/>
    <xsl:param name="pAcctTyp"/>
    <xsl:param name="pPutCall"/>
    <xsl:param name="pExistsSym" />
    <xsl:param name="pSym"/>
    
    <xsl:param name="pPrmAmt" select="'null'"/>
    <xsl:param name="pPrmCcy" />
    <xsl:param name="pPrmEnable" select="'true'"/>
    
    <xsl:param name="pFaceAmt" select="'null'"/>
    <xsl:param name="pFaceCcy" />
    <xsl:param name="pFaceEnable" select="'true'"/>
    
    <xsl:param name="pVrMrgnAmt" select="'null'"/>
    <xsl:param name="pVrMrgnCcy" />
    <xsl:param name="pVrMrgnEnable" select="'true'"/>
    
    <xsl:param name="pUMgAmt" select="'null'"/>
    <xsl:param name="pUMgCcy" />
    <xsl:param name="pUMgEnable" select="'true'"/>
    
    <xsl:param name="pCollAmt" select="'null'"/>
    <xsl:param name="pCollCcy" />
    <xsl:param name="pCollEnable" select="'true'"/>
    
    <xsl:param name="pOPPAmt" select="'null'"/>
    <xsl:param name="pOPPCcy" />
    <xsl:param name="pOPPEnable" select="'true'"/>
    
    <xsl:param name="pPmtAmt" select="'null'"/>
    <xsl:param name="pPmtCcy" />
    <xsl:param name="pPmtEnable" select="'true'"/>
    
    <xsl:param name="pStlAmt" select="'null'"/>
    <xsl:param name="pStlCcy" />
    <xsl:param name="pStlEnable" select="'true'"/>
    
    <xsl:param name="pTaxBrkAmt" select="'null'"/>
    <xsl:param name="pTaxBrkCcy" />
    <xsl:param name="pTaxBrkEnable" select="'true'"/>
    
    <xsl:param name="pTaxComAmt" select="'null'"/>
    <xsl:param name="pTaxComCcy" />
    <xsl:param name="pTaxComEnable" select="'true'"/>
    
    <xsl:param name="pTaxComBrkAmt" select="'null'"/>
    <xsl:param name="pTaxComBrkCcy" />
    <xsl:param name="pTaxComBrkEnable" select="'true'"/>
    
    <xsl:param name="pRMgAmt" select="'null'"/>
    <xsl:param name="pRMgCcy" />
    <xsl:param name="pRMgEnable" select="'true'"/>
    
    <xsl:param name="pCallAmt" select="'null'"/>
    <xsl:param name="pCallCcy" />
    <xsl:param name="pCallEnable" select="'true'"/>
    
    <xsl:param name="pLovAmt" select="'null'"/>
    <xsl:param name="pLovCcy" />
    <xsl:param name="pLovEnable" select="'true'"/>
    
    <xsl:param name="pRptAmt" select="'null'"/>
    <xsl:param name="pRptCcy" />
    <xsl:param name="pRptEnable" select="'true'"/>

    <ValueXML
        xmlns:fixml="http://www.fixprotocol.org/FIXML-5-0-SP1"
        xmlns:efs="http://www.efs.org/2007/EFSmL-3-0">
      <efs:AcctRpt>
        <xsl:if test="$pExistsMrk != 'null'">
          <xsl:attribute name="Mrk">
            <xsl:value-of select="$pMrk"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:attribute name="DtAndTm">
          <xsl:value-of select="$pDtAndTm"/>
        </xsl:attribute>
        <fixml:RptSide>
          <xsl:attribute name="Acct">
            <xsl:value-of select="$pAcct"/>
          </xsl:attribute>
          <xsl:attribute name="AcctTyp">
            <xsl:value-of select="$pAcctTyp"/>
          </xsl:attribute>
        </fixml:RptSide>
        <xsl:if test="$pExistsSym != 'null'">
          <fixml:Instrmt>
            <xsl:attribute name="Sym">
              <xsl:value-of select="$pSym"/>
            </xsl:attribute>
            <xsl:if test="$pPutCall != 'null'">
              <xsl:attribute name="PutCall">
                <xsl:value-of select="$pPutCall"/>
              </xsl:attribute>
            </xsl:if>
          </fixml:Instrmt>
        </xsl:if>
        <efs:Amts>
          <efs:PrmAmt>
            <xsl:attribute name="ccy">
              <xsl:value-of select="$pPrmCcy"/>
            </xsl:attribute>
            <xsl:attribute name="act">
              <xsl:value-of select="$pPrmEnable"/>
            </xsl:attribute>
            <xsl:if test="$pPrmAmt != 'null' and $pPrmAmt != ''">
              <xsl:attribute name="amt">
                <xsl:value-of select="$pPrmAmt"/>
              </xsl:attribute>
            </xsl:if>
          </efs:PrmAmt>

          <efs:FaceAmt>
            <xsl:attribute name="ccy">
              <xsl:value-of select="$pFaceCcy"/>
            </xsl:attribute>
            <xsl:attribute name="act">
              <xsl:value-of select="$pFaceEnable"/>
            </xsl:attribute>
            <xsl:if test="$pFaceAmt != 'null' and $pFaceAmt != ''">
              <xsl:attribute name="amt">
                <xsl:value-of select="$pFaceAmt"/>
              </xsl:attribute>
            </xsl:if>
          </efs:FaceAmt>

          <efs:UMgAmt>
            <xsl:attribute name="ccy">
              <xsl:value-of select="$pUMgCcy"/>
            </xsl:attribute>
            <xsl:attribute name="act">
              <xsl:value-of select="$pUMgEnable"/>
            </xsl:attribute>
            <xsl:if test="$pUMgAmt != 'null' and $pUMgAmt != ''">
              <xsl:attribute name="amt">
                <xsl:value-of select="$pUMgAmt"/>
              </xsl:attribute>
            </xsl:if>
          </efs:UMgAmt>

          <efs:VrMrgnAmt>
            <xsl:attribute name="ccy">
              <xsl:value-of select="$pVrMrgnCcy"/>
            </xsl:attribute>
            <xsl:attribute name="act">
              <xsl:value-of select="$pVrMrgnEnable"/>
            </xsl:attribute>
            <xsl:if test="$pVrMrgnAmt != 'null' and $pVrMrgnAmt != ''">
              <xsl:attribute name="amt">
                <xsl:value-of select="$pVrMrgnAmt"/>
              </xsl:attribute>
            </xsl:if>
          </efs:VrMrgnAmt>

          <efs:CollAmt>
            <xsl:attribute name="ccy">
              <xsl:value-of select="$pCollCcy"/>
            </xsl:attribute>
            <xsl:attribute name="act">
              <xsl:value-of select="$pCollEnable"/>
            </xsl:attribute>
            <xsl:if test="$pCollAmt != 'null' and $pCollAmt != ''">
              <xsl:attribute name="amt">
                <xsl:value-of select="$pCollAmt"/>
              </xsl:attribute>
            </xsl:if>
          </efs:CollAmt>

          <efs:OPPAmt>
            <xsl:attribute name="ccy">
              <xsl:value-of select="$pOPPCcy"/>
            </xsl:attribute>
            <xsl:attribute name="act">
              <xsl:value-of select="$pOPPEnable"/>
            </xsl:attribute>
            <xsl:if test="$pOPPAmt != 'null' and $pOPPAmt != ''">
              <xsl:attribute name="amt">
                <xsl:value-of select="$pOPPAmt"/>
              </xsl:attribute>
            </xsl:if>
          </efs:OPPAmt>

          <efs:PmtAmt>
            <xsl:attribute name="ccy">
              <xsl:value-of select="$pPmtCcy"/>
            </xsl:attribute>
          <xsl:attribute name="act">
              <xsl:value-of select="$pPmtEnable"/>
            </xsl:attribute>
            <xsl:if test="$pPmtAmt != 'null' and $pPmtAmt != ''">
              <xsl:attribute name="amt">
                <xsl:value-of select="$pPmtAmt"/>
              </xsl:attribute>
            </xsl:if>
          </efs:PmtAmt>

          <efs:StlAmt>
            <xsl:attribute name="ccy">
              <xsl:value-of select="$pStlCcy"/>
            </xsl:attribute>
              <xsl:attribute name="act">
              <xsl:value-of select="$pStlEnable"/>
            </xsl:attribute>
            <xsl:if test="$pStlAmt != 'null' and $pStlAmt != ''">
              <xsl:attribute name="amt">
                <xsl:value-of select="$pStlAmt"/>
              </xsl:attribute>
            </xsl:if>
          </efs:StlAmt>

          <efs:TaxBrkAmt>
            <xsl:attribute name="ccy">
              <xsl:value-of select="$pTaxBrkCcy"/>
            </xsl:attribute>
               <xsl:attribute name="act">
              <xsl:value-of select="$pTaxBrkEnable"/>
            </xsl:attribute>
            <xsl:if test="$pTaxBrkAmt != 'null' and $pTaxBrkAmt != ''">
              <xsl:attribute name="amt">
                <xsl:value-of select="$pTaxBrkAmt"/>
              </xsl:attribute>
            </xsl:if>
          </efs:TaxBrkAmt>

          <efs:TaxComAmt>
            <xsl:attribute name="ccy">
              <xsl:value-of select="$pTaxComCcy"/>
            </xsl:attribute>
              <xsl:attribute name="act">
              <xsl:value-of select="$pTaxComEnable"/>
            </xsl:attribute>
            <xsl:if test="$pTaxComAmt != 'null' and $pTaxComAmt != ''">
              <xsl:attribute name="amt">
                <xsl:value-of select="$pTaxComAmt"/>
              </xsl:attribute>
            </xsl:if>
          </efs:TaxComAmt>
          
          <efs:TaxComBrkAmt>
            <xsl:attribute name="ccy">
              <xsl:value-of select="$pTaxComBrkCcy"/>
            </xsl:attribute>
          <xsl:attribute name="act">
              <xsl:value-of select="$pTaxComBrkEnable"/>
            </xsl:attribute>
            <xsl:if test="$pTaxComBrkAmt != 'null' and $pTaxComBrkAmt != ''">
              <xsl:attribute name="amt">
                <xsl:value-of select="$pTaxComBrkAmt"/>
              </xsl:attribute>
            </xsl:if>
          </efs:TaxComBrkAmt>

          <efs:RMgAmt>
            <xsl:attribute name="ccy">
              <xsl:value-of select="$pRMgCcy"/>
            </xsl:attribute>
          <xsl:attribute name="act">
              <xsl:value-of select="$pRMgEnable"/>
            </xsl:attribute>
            <xsl:if test="$pRMgAmt != 'null' and $pRMgAmt != ''">
              <xsl:attribute name="amt">
                <xsl:value-of select="$pRMgAmt"/>
              </xsl:attribute>
            </xsl:if>
          </efs:RMgAmt>
          
          <efs:CallAmt>
            <xsl:attribute name="ccy">
              <xsl:value-of select="$pCallCcy"/>
            </xsl:attribute>
              <xsl:attribute name="act">
              <xsl:value-of select="$pCallEnable"/>
            </xsl:attribute>
            <xsl:if test="$pCallAmt != 'null' and $pCallAmt != ''">
              <xsl:attribute name="amt">
                <xsl:value-of select="$pCallAmt"/>
              </xsl:attribute>
            </xsl:if>
          </efs:CallAmt>
          
          <efs:LovAmt>
            <xsl:attribute name="ccy">
              <xsl:value-of select="$pLovCcy"/>
            </xsl:attribute>
              <xsl:attribute name="act">
              <xsl:value-of select="$pLovEnable"/>
            </xsl:attribute>
            <xsl:if test="$pLovAmt != 'null' and $pLovAmt != ''">
              <xsl:attribute name="amt">
                <xsl:value-of select="$pLovAmt"/>
              </xsl:attribute>
            </xsl:if>
          </efs:LovAmt>

          <efs:RptAmt>
            <xsl:attribute name="ccy">
              <xsl:value-of select="$pRptCcy"/>
            </xsl:attribute>
              <xsl:attribute name="act">
              <xsl:value-of select="$pRptEnable"/>
            </xsl:attribute>
            <xsl:if test="$pRptAmt != 'null' and $pRptAmt != ''">
              <xsl:attribute name="amt">
                <xsl:value-of select="$pRptAmt"/>
              </xsl:attribute>
            </xsl:if>
          </efs:RptAmt>

        </efs:Amts>
      </efs:AcctRpt>
    </ValueXML>
  </xsl:template>

</xsl:stylesheet>
