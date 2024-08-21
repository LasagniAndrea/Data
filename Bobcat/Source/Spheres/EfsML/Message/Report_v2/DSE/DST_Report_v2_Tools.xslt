<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:fo="http://www.w3.org/1999/XSL/Format"
                xmlns:fn="http://www.w3.org/2005/xpath-functions"
                xmlns:dt="http://xsltsl.org/date-time"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                version="1.0">


  <!-- ============================================================================================== -->
  <!--                                              Settings                                          -->
  <!-- ============================================================================================== -->

  <!-- ============================================================================================== -->
  <!--                                              Variables                                         -->
  <!-- ============================================================================================== -->

  <!-- ============================================================================================== -->
  <!--                                              Templates                                         -->
  <!-- ============================================================================================== -->

  <!-- .......................................................................... -->
  <!--              Display data                                                  -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- DisplayData_DebtSecurityTransaction              -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Data in DebtSecurityTransaction Trade          
       ................................................ -->
  <!-- FI 20151019 [21317] Add -->
  <xsl:template name="DisplayData_DebtSecurityTransaction">
    <!-- Represente la donnée demandée-->
    <xsl:param name="pDataName"/>
    <!-- Represente un trade DebtSecurityTransaction -->
    <xsl:param name="pTrade"/>
    <!-- Represente un noeud trades/trade ou posActions/posAction ou posTrades/trade-->
    <xsl:param name="pDataEfsml"/>
    <!-- Represente le fommat numérique demandé -->
    <xsl:param name="pNumberPattern" select="$gDefaultPricePattern"/>
    <!-- Si renseigné tronque(*) le résultat.  -->
    <xsl:param name="pDataLength" select ="number('0')"/>

    <xsl:variable name ="vRptSide" select="$pTrade/debtSecurityTransaction/RptSide"/>

    <xsl:variable name="vData">
      <xsl:choose>
        <xsl:when test="$pDataName='Side' or $pDataName='PosSide' or $pDataName='Acct'">
          <xsl:call-template name="RptSideGetValue">
            <xsl:with-param name ="pDataName" select ="$pDataName"/>
            <xsl:with-param name ="pRptSide" select ="$vRptSide"/>
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$pDataName='BuyQty'" >
          <xsl:if test="$vRptSide/@Side = '1'">
            <xsl:call-template name="DisplayData_DebtSecurityTransaction">
              <xsl:with-param name="pDataName" select="'Qty'"/>
              <xsl:with-param name="pTrade" select="$pTrade"/>
              <xsl:with-param name="pNumberPattern" select="$pNumberPattern" />
            </xsl:call-template>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$pDataName='SellQty'" >
          <xsl:if test="$vRptSide/@Side = '2'">
            <xsl:call-template name="DisplayData_DebtSecurityTransaction">
              <xsl:with-param name="pDataName" select="'Qty'"/>
              <xsl:with-param name="pTrade" select="$pTrade"/>
              <xsl:with-param name="pNumberPattern" select="$pNumberPattern" />
            </xsl:call-template>
          </xsl:if>
        </xsl:when>

        <xsl:when test="$pDataName='Qty'" >
          <xsl:choose>
            <xsl:when test="$pDataEfsml">
              <xsl:call-template name="DisplayData_Efsml">
                <xsl:with-param name="pDataName" select="'FmtQty'"/>
                <xsl:with-param name="pDataEfsml" select="$pDataEfsml"/>
                <xsl:with-param name="pNumberPattern" select="$pNumberPattern" />
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="format-integer">
                <xsl:with-param name="integer" select="$pTrade/debtSecurityTransaction/quantity/numberOfUnits"/>
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>

        <xsl:when test="$pDataName='PosBuyQty'" >
          <xsl:if test="$vRptSide/@Side = '1'">
            <xsl:call-template name="DisplayData_DebtSecurityTransaction">
              <xsl:with-param name="pDataName" select="'PosQty'"/>
              <xsl:with-param name="pDataEfsml" select="$pDataEfsml"/>
              <xsl:with-param name="pTrade" select="$pTrade"/>
              <xsl:with-param name="pNumberPattern" select="$pNumberPattern" />
            </xsl:call-template>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$pDataName='PosSellQty'" >
          <xsl:if test="$vRptSide/@Side = '2'">
            <xsl:call-template name="DisplayData_DebtSecurityTransaction">
              <xsl:with-param name="pDataName" select="'PosQty'"/>
              <xsl:with-param name="pDataEfsml" select="$pDataEfsml"/>
              <xsl:with-param name="pTrade" select="$pTrade"/>
              <xsl:with-param name="pNumberPattern" select="$pNumberPattern" />
            </xsl:call-template>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$pDataName='PosQty'" >
          <xsl:choose>
            <xsl:when test="$pDataEfsml">
              <xsl:call-template name="DisplayData_Efsml">
                <xsl:with-param name="pDataName" select="'FmtQty'"/>
                <xsl:with-param name="pDataEfsml" select="$pDataEfsml"/>
                <xsl:with-param name="pNumberPattern" select="$pNumberPattern" />
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="DisplayData_DebtSecurityTransaction">
                <xsl:with-param name="pDataName" select="'Qty'"/>
                <xsl:with-param name="pTrade" select="$pTrade"/>
                <xsl:with-param name="pNumberPattern" select="$pNumberPattern" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>

        <xsl:when test="$pDataName='accruedInterestRate'" >
          <xsl:call-template name="format-number">
            <xsl:with-param name="pAmount" select="$pTrade/debtSecurityTransaction/price/accruedInterestRate/text()"/>
            <xsl:with-param name="pAmountPattern" select="$pNumberPattern"/>
          </xsl:call-template>
        </xsl:when>
        
      </xsl:choose>
    </xsl:variable>

    <xsl:call-template name="DisplayData_Truncate">
      <xsl:with-param name="pData" select="$vData"/>
      <xsl:with-param name="pDataLength" select="$pDataLength"/>
    </xsl:call-template>
    
  </xsl:template>


  
  
  
  

  
  
  
  
  
  
  
  
  
  
  
  
  
</xsl:stylesheet>