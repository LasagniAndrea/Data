<!--
/*==============================================================
/* Summary : Import TIMS IDEM parameters                        
/*==============================================================
/* File    : FxRateImport-EUREX.xsl                   
/* Version : v0.0.0.1                                           
/* Date    : 20111019                                           
/* Author  : MF                                                 
/* Description: mapping SQl commands for the FPHCPA file data. The FPHCPA (file-name: hcAAAAMMDD.txt) file contains
haircut and adjusted exchange rate for the EUREX market.
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
  <xsl:include href="..\Common\CommonInput.xslt"/>
  <xsl:include href="Quote_Common_SQL.xsl"/>

  <xsl:variable name="vVersionRiskDataImport">v0.0.0.1</xsl:variable>
  <xsl:variable name="vFileNameRiskDataImport">FxRateImport-EUREX.xsl</xsl:variable>

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
      <!-- file date vs market date, when the file date is different the import raise an error -->
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

      <!-- fx rate insert/update (the referecne asset will be inserted too when it does not exist)-->
      <xsl:when test="data[contains(@status, 'success')] and data[contains(@name, 'IDC')]">

        <row>
          <xsl:call-template name="IORowAtt"/>

          <!--FL le 20121212 
          <xsl:variable name="vAssetIdentifier" select="concat(data[@name='IDC'],'/',data[@name='IDC_XEUR'],' EUREX RMHCPA')"/>-->
          <xsl:variable name="vAssetIdentifier" select="concat(data[@name='IDC_XEUR'],'/',data[@name='IDC'],' EUREX RMHCPA')"/>-->
          
          <xsl:call-template name="ASSET_FXRATE_IU">
            <xsl:with-param name="pSequenceNumber" select="'1'"/>
            <xsl:with-param name="pCurrency1" select="data[@name='IDC']"/>
            <xsl:with-param name="pCurrency2" select="data[@name='IDC_XEUR']"/>
            <xsl:with-param name="pIdentifier" select="$vAssetIdentifier"/>
            <!-- BD 20130524 : Correction de la Description du Taux de change = "'EUREX Risk...' Currency1 'per one' Currency2" -->
            <xsl:with-param name="pDescription" select="concat('EUREX Risk Parameters Exchange Rate ',data[@name='IDC'],' per one ',data[@name='IDC_XEUR'])"/>            
            <!-- value of the BUSINESSCENTER table -->
            <xsl:with-param name="pIDBC" select="'EUTA'"/>
            <!-- value of the InformationProvider Spheres enum -->
            <xsl:with-param name="pPrimaryRateSource" select="'ClearingOrganization'"/>
            <xsl:with-param name="pPrimaryRateSourcePage" select="'http://www.eurexchange.com'"/>
            <xsl:with-param name="pIsWithControl" select="$gTrue"/>
          </xsl:call-template>

          <xsl:call-template name="QUOTE_H_IU">
            <xsl:with-param name="pTableName" select="'QUOTE_FXRATE_H'"/>
            <xsl:with-param name="pSequenceNumber" select="'1'"/>
            <xsl:with-param name="pContractSymbol" select="$vAssetIdentifier"/>
            <xsl:with-param name="pCategory" select="'FX'"/>
            <xsl:with-param name="pBusinessDate" select="/iotask/parameters/parameter[@id='DTBUSINESS']"/>
            <xsl:with-param name="pValue" select="data[@name='Adj_CredValue']"/>
            <xsl:with-param name="pQuoteUnit" select="'ExchangeRate'"/>
            <!-- When the exchange rate is side credit(seller) then we use QuoteSide Bid -->
            <xsl:with-param name="pQuoteSide" select="'Bid'"/>
            <xsl:with-param name="pRateSource" select="'ClearingOrganization'"/>
            <xsl:with-param name="pContractSymbolSuffix" select="$gNull"/>
            <xsl:with-param name="pMaturityMonthYear" select="$gNull"/>
            <xsl:with-param name="pPutCall" select="$gNull"/>
            <xsl:with-param name="pStrike" select="$gNull" />
            <xsl:with-param name="pCurrency" select="$gNull"/>
            <xsl:with-param name="pExchangeSymbol" select="$gNull"/>
            <xsl:with-param name="pIsWithControl" select="$gTrue"/>
          </xsl:call-template>

          <xsl:call-template name="QUOTE_H_IU">
            <xsl:with-param name="pTableName" select="'QUOTE_FXRATE_H'"/>
            <xsl:with-param name="pSequenceNumber" select="'1'"/>
            <xsl:with-param name="pContractSymbol" select="$vAssetIdentifier"/>
            <xsl:with-param name="pCategory" select="'FX'"/>
            <xsl:with-param name="pBusinessDate" select="/iotask/parameters/parameter[@id='DTBUSINESS']"/>
            <xsl:with-param name="pValue" select="data[@name='Adj_DebValue']"/>
            <xsl:with-param name="pQuoteUnit" select="'ExchangeRate'"/>
            <!-- When the exchange rate is side debit(buyer) then we use QuoteSide Ask -->
            <xsl:with-param name="pQuoteSide" select="'Ask'"/>
            <xsl:with-param name="pRateSource" select="'ClearingOrganization'"/>
            <xsl:with-param name="pContractSymbolSuffix" select="$gNull"/>
            <xsl:with-param name="pMaturityMonthYear" select="$gNull"/>
            <xsl:with-param name="pPutCall" select="$gNull"/>
            <xsl:with-param name="pStrike" select="$gNull" />
            <xsl:with-param name="pCurrency" select="$gNull"/>
            <xsl:with-param name="pExchangeSymbol" select="$gNull"/>
            <xsl:with-param name="pIsWithControl" select="$gTrue"/>
          </xsl:call-template>

        </row>

      </xsl:when>

      <xsl:otherwise>

        <!-- no-op -->

      </xsl:otherwise>

    </xsl:choose>



  </xsl:template>

</xsl:stylesheet>