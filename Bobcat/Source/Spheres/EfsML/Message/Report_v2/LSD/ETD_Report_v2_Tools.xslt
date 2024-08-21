<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:fo="http://www.w3.org/1999/XSL/Format"
                xmlns:fn="http://www.w3.org/2005/xpath-functions"
                xmlns:dt="http://xsltsl.org/date-time"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                version="1.0">

  <!-- ============================================================================================== -->
  <!-- Summary : Spheres report - Shared - Common variables for all reports                           -->
  <!-- File    : Shared_Report_v2_PDF.xslt                                                            -->
  <!-- ============================================================================================== -->
  <!-- Version : v4.2.5358                                                                            -->
  <!-- Date    : 20140905                                                                             -->
  <!-- Author  : RD                                                                                   -->
  <!-- Comment : First version                                                                        -->
  <!-- ============================================================================================== -->

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
  <!-- DisplayData_RepositoryEtd                        -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Data in Repository ETD flow          
       ................................................ -->
  <xsl:template name="DisplayData_RepositoryEtd">
    <xsl:param name="pDataName"/>
    <xsl:param name="pAsset"/>
    <xsl:param name="pNumberPattern" select="$gDefaultPricePattern"/>
    <xsl:param name="pDataLength" select ="number('0')"/>

    <xsl:variable name="vData">
      <xsl:choose>
        <xsl:when test="$pDataName='TradeType'" >
          <xsl:variable name="vCategory" select="$gRepository/derivativeContract[@OTCmlId=@pAsset/idDC]/category"/>
          <xsl:choose>
            <xsl:when test="$vCategory = 'O'">
              <xsl:choose>
                <xsl:when test="$pAsset/putCall = '0'">
                  <xsl:value-of select="'Put'"/>
                </xsl:when>
                <xsl:when test="$pAsset/putCall = '1'">
                  <xsl:value-of select="'Call'"/>
                </xsl:when>
              </xsl:choose>
              <xsl:value-of select="'-'"/>
              <xsl:call-template name="format-number">
                <xsl:with-param name="pAmount" select="$pAsset/strikePrice"/>
                <xsl:with-param name="pAmountPattern" select="$pNumberPattern"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'Future'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="$pDataName='Maturity'" >
          <xsl:variable name="vDay" select="substring($pAsset/maturityMonthYear , 7)" />
          <xsl:variable name="vMaturity">
            <xsl:call-template name="format-shortdate_ddMMMyy">
              <xsl:with-param name="year" select="substring($pAsset/maturityMonthYear , 1, 4)" />
              <xsl:with-param name="month" select="substring($pAsset/maturityMonthYear , 5, 2)" />
              <xsl:with-param name="day" select="$vDay" />
            </xsl:call-template>
          </xsl:variable>
          <xsl:choose>
            <xsl:when test="string-length($vDay)=0">
              <xsl:call-template name="FirstUCase">
                <xsl:with-param name="source" select="$vMaturity" />
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$vMaturity"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <!--<xsl:when test="$pDataName='Expiry'" >
          <xsl:if test="string-length($pAsset/maturityDate/text()) > 0">
            <xsl:call-template name="format-shortdate_ddMMMyy">
              <xsl:with-param name="year" select="substring($pAsset/maturityDate/text() , 1, 4)" />
              <xsl:with-param name="month" select="substring($pAsset/maturityDate/text() , 6, 2)" />
              <xsl:with-param name="day" select="substring($pAsset/maturityDate/text() , 9, 2)" />
            </xsl:call-template>
          </xsl:if>
        </xsl:when>-->
        <xsl:when test="$pDataName='PC'" >
          <xsl:choose>
            <xsl:when test="$pAsset/putCall = '0'">
              <xsl:value-of select="'Put'"/>
            </xsl:when>
            <xsl:when test="$pAsset/putCall = '1'">
              <xsl:value-of select="'Call'"/>
            </xsl:when>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="$pDataName='FmtStrike'" >
          <xsl:call-template name="DisplayFmtNumber">
            <xsl:with-param name="pFmtNumber" select="$pAsset/strikePrice/@fmtPrice"/>
            <xsl:with-param name="pPattern" select="$pNumberPattern" />
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