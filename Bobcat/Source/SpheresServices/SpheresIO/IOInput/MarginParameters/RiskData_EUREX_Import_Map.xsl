<!--
/*==============================================================
/* Summary : Import EUREX parameters - Main                       
/*==============================================================
/* File    : RiskData_EUREX_Import_Map.xsl                   
/* Version : v0.0.0.1                                           
/* Date    : 20111014                                           
/* Author  : MF                                                 
/* Description: 
    

/*==============================================================*/
/*==============================================================
/* Revision:                                           
/*                                                            
/* Date    :                                          
/* Author  :                                                 
/* Version :                                             	      
/* Comment : 
                    
/*==============================================================*/
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- Includes-->
  <xsl:include href=".\RiskData_EUREX_Import_TableTemplates.xsl"/>

  <xsl:variable name="vVersionRiskDataImport">v0.0.0.1</xsl:variable>
  <xsl:variable name="vFileNameRiskDataImport">RiskData_EUREX_Import_Map.xsl</xsl:variable>

  <!-- match the file node -->
  <xsl:template match="file">
    <file>
      <xsl:call-template name="IOFileAtt"/>

      <!-- match any row node inside the actual file node -->
      <xsl:apply-templates select="row">


      </xsl:apply-templates>

    </file>
  </xsl:template>

  <!-- match any row node -->
  <xsl:template match="row">

    <xsl:choose>
      
      <!-- fichier FPPARM - insert an ETD contract inside of the EUREX class/group hierarchy, any contract EUREX is bound to one pair class/group -->
      <xsl:when test="data[contains(@status, 'success')] and data[contains(@name, 'MgnGrp')] and data[contains(@name, 'MgnCls')]">

        <row>
          <xsl:call-template name="IORowAtt"/>

          <xsl:variable name="vOffset">
            <xsl:value-of select="number(data[@name='Offset']) div 100"/>
          </xsl:variable>

          <xsl:call-template name="PARAMSEUREX_CONTRACT_IU">
            <xsl:with-param name="pMgnGrp" select="data[@name='MgnGrp']"/>
            <xsl:with-param name="pMgnCls" select="data[@name='MgnCls']"/>
            <xsl:with-param name="pSym" select="data[@name='Sym']"/>
            <xsl:with-param name="pExch" select="'XEUR'"/>
            <xsl:with-param name="pMgnSwt" select="data[@name='MgnSwt']"/>
            <xsl:with-param name="pExpMthFtr" select="data[@name='ExpMthFtr']"/>
            <xsl:with-param name="pOffset" select="$vOffset"/>
            <xsl:with-param name="pBckSpdRat" select="data[@name='BckSpdRat']"/>
            <xsl:with-param name="pSptSpdRat" select="data[@name='SptSpdRat']"/>
            <xsl:with-param name="pOOM_MinRat" select="data[@name='OOM_MinRat']"/>
          </xsl:call-template>

        </row>

      </xsl:when>

      <!-- fichier FPPARA - file date vs market date, when the file date is different the import raise an error -->
      <xsl:when test="data[contains(@status, 'success')] and data[contains(@name, 'BusDt')]">

        <row>
          <xsl:call-template name="IORowAtt"/>

          <xsl:call-template name="FileDateCheck">
            <xsl:with-param name="pFileDate" select="data[@name='BusDt']"/>
            <xsl:with-param name="pControlDate" select="/iotask/parameters/parameter[@id='DTBUSINESS']"/>
            <xsl:with-param name="pFileName" select="/iotask/iotaskdet/ioinput/file/@name"/>
            <xsl:with-param name="pFilePath" select="/iotask/iotaskdet/ioinput/file/@folder"/>
            <xsl:with-param name="pIOElemId" select="/iotask/iotaskdet/ioinput/@id"/>
          </xsl:call-template>

        </row>

      </xsl:when>

      <!-- fichier FPPARA - maturity factor import - 
      insert the maturity factor for the inserted asset bound to the input derivative margin class -->
      <xsl:when test="data[contains(@status, 'success')] and data[contains(@name, 'YYMM')] and data[contains(@name, 'MgnCls')]">

        <row>
          <xsl:call-template name="IORowAtt"/>

          <parameters>
            <xsl:call-template name="BuildGlobalParameter">
              <xsl:with-param name="pParamName" select="'ContractSymbol'"/>
              <xsl:with-param name="pParamValue">
                <SQL command="select" result="CONTRACTSYMBOL">
                  select CONTRACTSYMBOL
                  from PARAMSEUREX_CONTRACT
                  where
                  MARGINCLASS = @MARGINCLASS
                  and MATURITY_SWITCH = 'Y'
                  <Param name="MARGINCLASS" datatype="string">
                    <xsl:value-of select="data[@name='MgnCls']"/>
                  </Param>
                </SQL>
              </xsl:with-param>
            </xsl:call-template>
          </parameters>

          <xsl:call-template name="PARAMSEUREX_MATURITY_U">
            <xsl:with-param name="pContractSymbol" select="'parameters.ContractSymbol'"/>
            <xsl:with-param name="pMaturityYearMonth" select="concat('20',data[@name='YYMM'])"/>
            <xsl:with-param name="pPutCall" select="'0'"/>
            <xsl:with-param name="pMatFtr" select="data[@name='MatFtr']"/>
          </xsl:call-template>

          <xsl:call-template name="PARAMSEUREX_MATURITY_U">
            <xsl:with-param name="pSequenceNumber" select="'2'"/>
            <xsl:with-param name="pContractSymbol" select="'parameters.ContractSymbol'"/>
            <xsl:with-param name="pMaturityYearMonth" select="concat('20',data[@name='YYMM'])"/>
            <xsl:with-param name="pPutCall" select="'1'"/>
            <xsl:with-param name="pMatFtr" select="data[@name='MatFtr']"/>
          </xsl:call-template>

          <xsl:call-template name="PARAMSEUREX_MATURITY_U">
            <xsl:with-param name="pSequenceNumber" select="'3'"/>
            <xsl:with-param name="pContractSymbol" select="'parameters.ContractSymbol'"/>
            <xsl:with-param name="pMaturityYearMonth" select="concat('20',data[@name='YYMM'])"/>
            <xsl:with-param name="pPutCall" select="$gNull"/>
            <xsl:with-param name="pMatFtr" select="data[@name='MatFtr']"/>
          </xsl:call-template>
          
        </row>

      </xsl:when>

      <xsl:otherwise>

        <!-- no-op -->

      </xsl:otherwise>

    </xsl:choose>
    
  </xsl:template>
  
</xsl:stylesheet>
