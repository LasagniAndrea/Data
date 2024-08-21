<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:dt="http://xsltsl.org/date-time"
                xmlns:fo="http://www.w3.org/1999/XSL/Format"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                version="1.0">

  <!-- ============================================================================================== -->
  <!-- Summary : Spheres report - Shared - Common variables for all reports                           -->
  <!-- File    : \Report_v2\Shared\Shared_Report_v2_UK.xslt                                           -->
  <!-- ============================================================================================== -->
  <!-- Version : v5.0.5738                                                                            -->
  <!-- Date    : 20150917                                                                             -->
  <!-- Author  : RD                                                                                   -->
  <!-- Comment : [21309] Model dynamique d'affichage de la section Purchaes & Sales                   -->
  <!--           [21376] Correction affichage indicateur Credit/Debit "Parenthesis"                   -->
  <!--           [21376] Gestion indicateur Credit/Debit pour la section "Amount Summary"             -->
  <!-- ============================================================================================== -->
  <!-- Version : v4.2.5358                                                                            -->
  <!-- Date    : 20140905                                                                             -->
  <!-- Author  : RD                                                                                   -->
  <!-- Comment : First version                                                                        -->
  <!-- ============================================================================================== -->

  <!-- ============================================================================================== -->
  <!--                                              Variables                                         -->
  <!-- ============================================================================================== -->
  <xsl:variable name="gDefaultPricePattern" select="$numberPatternNoZero"/>
  <xsl:variable name="gDefaultBlock-number-columns" select="number('19')"/>

  <!-- .......................................................................... -->
  <!--              Display Settings                                              -->
  <!-- .......................................................................... -->
  <xsl:variable name="gSection_width">200mm</xsl:variable>
  <xsl:variable name="gSectionTab1_width">100mm</xsl:variable>
  <xsl:variable name="gSectionTab2_width">100mm</xsl:variable>
  <xsl:variable name="gSection_space-before">5mm</xsl:variable>

  <xsl:variable name="gBlock_space-before">2mm</xsl:variable>
  <xsl:variable name="gBlock_space-after">1mm</xsl:variable>

  <xsl:variable name="gSubTotal_space-after">2mm</xsl:variable>

  <xsl:variable name="gColumnSpace_width">1.5mm</xsl:variable>
  <xsl:variable name="gLineSpace_height">2pt</xsl:variable>

  <xsl:variable name="gTable_border">0.5pt solid black</xsl:variable>

  <!--Block Data -->
  <xsl:variable name="gData_font-size">7pt</xsl:variable>
  <xsl:variable name="gData_Det_font-size">5pt</xsl:variable>
  <xsl:variable name="gData_font-weight">normal</xsl:variable>
  <xsl:variable name="gData_Master_font-weight">bold</xsl:variable>
  <!-- EG 20160308 Migration vs2013 UNIT = mm -->
  <xsl:variable name="gData_padding">0.15mm</xsl:variable>
  <xsl:variable name="gData_display-align">center</xsl:variable>
  <xsl:variable name="gData_text-align">center</xsl:variable>
  <xsl:variable name="gData_Header-align">center</xsl:variable>
  <xsl:variable name="gData_Number_text-align">right</xsl:variable>
  <xsl:variable name="gData_Side_text-align">center</xsl:variable>

  <xsl:variable name="gBlockSettingsNode">
    <block name="Section" border="0.75pt solid" border-bottom-color="{$gSectionBanner-background-color}" border-bottom-width="1.25pt"
           font-size="{$gSectionBanner-font-size}"
           font-weight="{$gSectionBanner-font-weight}"
           text-align="{$gSectionBanner-text-align}"
           display-align="center"
           color="{$gSectionBanner-color}"
           background-color="{$gSectionBanner-background-color}"
           padding="0.5mm" padding-bottom="0.15mm">
      <data name="det" font-weight="normal"/>
      <!-- RD 20190619 [23912] Add order section -->
      <section key="{$gcReportSectionKey_ORD}">
        <xsl:attribute name="resource">
          <xsl:choose>
            <xsl:when test="string-length($gSettings-section[@key=$gcReportSectionKey_ORD])>0">
              <xsl:value-of select="$gSettings-section[@key=$gcReportSectionKey_ORD]"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-TradesConfirmations'" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <data name="NotAvailable">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-TradesConfirmations-NotAvailable'"/>
            </xsl:call-template>
          </xsl:attribute>
        </data>
      </section>
      <section key="{$gcReportSectionKey_TRD}">
        <xsl:attribute name="resource">
          <xsl:choose>
            <xsl:when test="string-length($gSettings-section[@key=$gcReportSectionKey_TRD])>0">
              <xsl:value-of select="$gSettings-section[@key=$gcReportSectionKey_TRD]"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-TradesConfirmations'" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <data name="NotAvailable">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-TradesConfirmations-NotAvailable'"/>
            </xsl:call-template>
          </xsl:attribute>
        </data>
      </section>
      <section key="{$gcReportSectionKey_AMT}">
        <xsl:attribute name="resource">
          <xsl:choose>
            <xsl:when test="string-length($gSettings-section[@key=$gcReportSectionKey_AMT])>0">
              <xsl:value-of select="$gSettings-section[@key=$gcReportSectionKey_AMT]"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-AmendmentTransfer'" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </section>
      <section key="{$gcReportSectionKey_CAS}">
        <xsl:attribute name="resource">
          <xsl:choose>
            <xsl:when test="string-length($gSettings-section[@key=$gcReportSectionKey_CAS])>0">
              <xsl:value-of select="$gSettings-section[@key=$gcReportSectionKey_CAS]"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-Cascading'" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </section>
      <section key="{$gcReportSectionKey_LIQ}">
        <xsl:attribute name="resource">
          <xsl:choose>
            <xsl:when test="string-length($gSettings-section[@key=$gcReportSectionKey_LIQ])>0">
              <xsl:value-of select="$gSettings-section[@key=$gcReportSectionKey_LIQ]"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-Liquidations'" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </section>
      <section key="{$gcReportSectionKey_DLV}">
        <xsl:attribute name="resource">
          <xsl:choose>
            <xsl:when test="string-length($gSettings-section[@key=$gcReportSectionKey_DLV])>0">
              <xsl:value-of select="$gSettings-section[@key=$gcReportSectionKey_DLV]"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-Deliveries'" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </section>
      <section key="{$gcReportSectionKey_CA}">
        <xsl:attribute name="resource">
          <xsl:choose>
            <xsl:when test="string-length($gSettings-section[@key=$gcReportSectionKey_CA])>0">
              <xsl:value-of select="$gSettings-section[@key=$gcReportSectionKey_CA]"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-CorporateAction'" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </section>
      <xsl:variable name="vResource_ASS">
        <xsl:choose>
          <xsl:when test="string-length($gSettings-section[@key=$gcReportSectionKey_ASS])>0">
            <xsl:value-of select="$gSettings-section[@key=$gcReportSectionKey_ASS]"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Assignements'" />
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="vResource_EXE">
        <xsl:choose>
          <xsl:when test="string-length($gSettings-section[@key=$gcReportSectionKey_EXE])>0">
            <xsl:value-of select="$gSettings-section[@key=$gcReportSectionKey_EXE]"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Exercises'" />
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <section key="{$gcReportSectionKey_EXE}" resource="{$vResource_EXE}"/>
      <section key="{$gcReportSectionKey_ASS}" resource="{$vResource_ASS}"/>
      <section key="{$gcReportSectionKey_EXA}" resource="{$vResource_EXE}"/>
      <section key="{$gcReportSectionKey_ABN}">
        <xsl:attribute name="resource">
          <xsl:choose>
            <xsl:when test="string-length($gSettings-section[@key=$gcReportSectionKey_ABN])>0">
              <xsl:value-of select="$gSettings-section[@key=$gcReportSectionKey_ABN]"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-AbandonmentsExpirations'" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </section>
      <section key="{$gcReportSectionKey_SET}">
        <xsl:attribute name="resource">
          <xsl:choose>
            <xsl:when test="string-length($gSettings-section[@key=$gcReportSectionKey_SET])>0">
              <xsl:value-of select="$gSettings-section[@key=$gcReportSectionKey_SET]"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-SettledTrades'" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </section>
      <section key="{$gcReportSectionKey_UNS}">
        <xsl:attribute name="resource">
          <xsl:choose>
            <xsl:when test="string-length($gSettings-section[@key=$gcReportSectionKey_UNS])>0">
              <xsl:value-of select="$gSettings-section[@key=$gcReportSectionKey_UNS]"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-UnsettledTrades'" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </section>
      <section key="{$gcReportSectionKey_PSS}">
        <xsl:attribute name="resource">
          <xsl:choose>
            <xsl:when test="string-length($gSettings-section[@key=$gcReportSectionKey_PSS])>0">
              <xsl:value-of select="$gSettings-section[@key=$gcReportSectionKey_PSS]"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-PurchaseSale'" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <product name="ETD" model="{$gcPurchaseSalePnLOnClosing}"/>
        <!--<product name="ETD" model="{$gcPurchaseSaleOverallQty}"/>-->
      </section>
      <section key="{$gcReportSectionKey_CLO}">
        <xsl:attribute name="resource">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-ClosedPositions'" />
          </xsl:call-template>
        </xsl:attribute>
        <data name="BizDate">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-BusinessDate'"/>
            </xsl:call-template>
          </xsl:attribute>
        </data>
      </section>
      <section key="{$gcReportSectionKey_CST}">
        <xsl:attribute name="resource">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-ClosedPositions'" />
          </xsl:call-template>
        </xsl:attribute>
        <data name="StlDate">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-SettlementDate'"/>
            </xsl:call-template>
          </xsl:attribute>
        </data>
      </section>
      <section key="{$gcReportSectionKey_POS}">
        <xsl:attribute name="resource">
          <xsl:choose>
            <xsl:when test="string-length($gSettings-section[@key=$gcReportSectionKey_POS])>0">
              <xsl:value-of select="$gSettings-section[@key=$gcReportSectionKey_POS]"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-OpenPosition'" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <data name="NotAvailable">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-OpenPosition-NotAvailable'"/>
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="BizDate">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-BusinessDate'"/>
            </xsl:call-template>
          </xsl:attribute>
        </data>
      </section>
      <section key="{$gcReportSectionKey_STL}">
        <xsl:attribute name="resource">
          <xsl:choose>
            <xsl:when test="string-length($gSettings-section[@key=$gcReportSectionKey_POS])>0">
              <xsl:value-of select="$gSettings-section[@key=$gcReportSectionKey_POS]"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-OpenPosition'" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <data name="NotAvailable">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-StlOpenPosition-NotAvailable'"/>
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="StlDate">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-SettlementDate'"/>
            </xsl:call-template>
          </xsl:attribute>
        </data>
      </section>
      <section key="{$gcReportSectionKey_JNL}">
        <xsl:attribute name="resource">
          <xsl:choose>
            <xsl:when test="string-length($gSettings-section[@key=$gcReportSectionKey_JNL])>0">
              <xsl:value-of select="$gSettings-section[@key=$gcReportSectionKey_JNL]"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-JournalEntries'" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </section>
      <!-- FI 20160530 [21885] Add -->
      <section key="{$gcReportSectionKey_SOD}">
        <xsl:attribute name="resource">
          <xsl:choose>
            <xsl:when test="string-length($gSettings-section[@key=$gcReportSectionKey_SOD])>0">
              <xsl:value-of select="$gSettings-section[@key=$gcReportSectionKey_SOD]"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-CollateralSection'" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </section>

      <!-- FI 20160613 [22256] Add -->
      <section key="{$gcReportSectionKey_UOD}">
        <xsl:attribute name="resource">
          <xsl:choose>
            <xsl:when test="string-length($gSettings-section[@key=$gcReportSectionKey_UOD])>0">
              <xsl:value-of select="$gSettings-section[@key=$gcReportSectionKey_UOD]"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-UnderlyingStockSection'" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <xsl:attribute name ="resourceAdditional">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-UnderlyingStockSection-Additional'" />
          </xsl:call-template>
        </xsl:attribute>
      </section>

      <section key="{$gcReportSectionKey_ACC}">
        <xsl:attribute name="resource">
          <xsl:choose>
            <xsl:when test="string-length($gSettings-section[@key=$gcReportSectionKey_ACC])>0">
              <xsl:value-of select="$gSettings-section[@key=$gcReportSectionKey_ACC]"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-AccountSummary'" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <data name="NotAvailable">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-AccountSummary-NotAvailable'"/>
            </xsl:call-template>
          </xsl:attribute>
        </data>
      </section>
    </block>

    <block name="Date">
      <column name="Label"
              font-size="{$gDateBanner-font-size}"
              font-weight="{$gDateBanner-font-weight}"
              text-align="{$gDateBanner-text-align}"
              display-align="center"
              color="{$gDateBanner-color}"
              background-color="{$gDateBanner-background-color}"
              padding="0.15mm">
        <data name="RecordedAsOf">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-RecordedAsOf'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="SettledAsOf">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-SettledAsOf'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="MadeAsOf">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-MadeAsOf'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
      </column>
    </block>
    <block name="MarketDC" border="0.75pt solid black" border-bottom-width="1.25pt">
      <column name="Market" column-width="80mm" length="50"
              font-size="{$gAssetBanner-font-size}"
              font-weight="{$gAssetBanner-font-weight}"
              text-align="{$gAssetBanner-text-align}"
              display-align="center"
              color="{$gAssetBanner-color}"
              background-color="{$gAssetBanner-background-color}"
              padding="0.5mm">
        <data name="det" font-weight="normal"/>
      </column>
      <column name="DC" column-width="120mm" length="76"
              font-size="7pt"
              text-align="left"
              display-align="center"
              font-weight="bold"
              color="{$gAssetBanner-color}"
              background-color="{$gAssetBanner-background-color}"
              padding="0.5mm">
        <data name="det" font-weight="normal"/>
      </column>
    </block>
    <block name="MarketCcy" border="0.75pt solid black" border-bottom-width="1.25pt">
      <column name="Market" column-width="80mm" length="50"
              font-size="{$gAssetBanner-font-size}"
              font-weight="{$gAssetBanner-font-weight}"
              text-align="{$gAssetBanner-text-align}"
              display-align="center"
              color="{$gAssetBanner-color}"
              background-color="{$gAssetBanner-background-color}"
              padding="0.5mm">
        <data name="det" font-weight="normal"/>
      </column>
      <column name="Ccy" column-width="120mm" length="76"
              font-size="7pt"
              text-align="left"
              display-align="center"
              font-weight="bold"
              color="{$gAssetBanner-color}"
              background-color="{$gAssetBanner-background-color}"
              padding="0.5mm">
        <data name="det" font-size="7pt" font-weight="normal"/>
      </column>
    </block>
    <block name="Book" border="0.75pt solid black">
      <column name="Label" column-width="15mm"
              font-size="{$gAssetBanner-font-size}"
              font-weight="normal"
              text-align="{$gAssetBanner-text-align}"
              display-align="center"
              color="{$gAssetBanner-color}"
              background-color="{$gAssetBanner-background-color}"
              padding="0.15mm">
        <data>
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Book'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
      </column>

      <column name="Desc"
              font-size="{$gAssetBanner-font-size}"
              font-weight="{$gAssetBanner-font-weight}"
              text-align="{$gAssetBanner-text-align}"
              display-align="center"
              color="{$gAssetBanner-color}"
              background-color="{$gAssetBanner-background-color}"
              padding="0.15mm">
        <data name="det" font-weight="normal"/>
      </column>
    </block>
    <!-- RD 20190619 [23912] Add Asset block -->
    <block name="Asset">
      <!--text-align="left" font-size="6pt" font-weight="bold"-->
      <column name="AssetDet"
              font-size="6pt"
              font-weight="normal"
              text-align="{$gAssetBanner-text-align}"
              display-align="center"
              color="{$gAssetBanner-color}"
              background-color="#f2f2f2"
              padding="0.5mm">
        <data name="det" font-weight="bold"/>
        <data name="ISINCode" resource="ISIN" sort="{$gISINCode_Type}"/>
        <data name="BBGCode" resource="Bloomberg" sort="{$gBBGCode_Type}"/>
        <data name="RICCode" resource="RIC" sort="{$gRICCode_Type}"/>
      </column>
    </block>
    <!-- RD 20190619 [23912] Add Trader block -->
    <block name="Trader">
      <!--text-align="left" font-size="6pt" font-weight="bold"-->
      <column name="TraderDet"
              font-size="6pt"
              font-weight="normal"
              text-align="{$gAssetBanner-text-align}"
              display-align="center"
              color="{$gAssetBanner-color}"
              background-color="#f2f2f2"
              padding="0.5mm">
        <data>
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-TraderIntro'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="Name" font-weight="bold">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Name'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
      </column>
    </block>
    <block name="Data">
      <!--Data Columns-->
      <column name="Date" column-width="13.5mm">
        <header>
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Date'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="ClearingDate">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-ClearDate'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="TradeDate">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-TradeDate'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="SettltDate">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-SettltDate'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="PaymentDate">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-PaymentDate'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="ValueDate">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-ValueDate'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="Expiry">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Expiry'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="EntryDate">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-EntryDate'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <data name="Time" font-size="5pt"/>
      </column>
      <column name="TrdNum" column-width="16mm">
        <header>
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-TrdNum'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="OrderNum">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-OrderNum'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="RefNum">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-RefNum'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <data length="12"/>
        <data name="trade2" font-style="italic" color="#8C8C8C"/>
        <style name="S1" length="12" font-size="7pt"         description="999999999999"/>
        <style name="S2" length="14" font-size="6pt"       description="99999999999999"/>
        <style name="S3" length="16" font-size="5pt"     description="9999999999999999"/>
        <style name="S4" length="18" font-size="4.5pt" description="999999999999999999"/>
      </column>
      <column name="Type" column-width="6mm">
        <data length="6" font-size="6pt" font-weight="normal" text-align="left"/>
        <data name="trade2" font-style="italic" color="#8C8C8C" text-align="left"/>
      </column>
      <column name="OrderType" column-width="20mm">
        <header>
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-OrderType'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <data length="21" font-size="7pt" font-weight="normal" text-align="center"/>
        <style name="S1" length="21" font-size="7pt"         description="999999999999"/>
        <style name="S2" length="24" font-size="6pt"       description="99999999999999"/>
        <style name="S3" length="27" font-size="5pt"     description="9999999999999999"/>
        <style name="S4" length="30" font-size="4.5pt" description="999999999999999999"/>
      </column>
      <column name="OrderStatus" column-width="20mm">
        <header>
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-OrderStatus'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <data length="21" font-size="7pt" font-weight="normal" text-align="center"/>
        <style name="S1" length="21" font-size="7pt"         description="999999999999"/>
        <style name="S2" length="24" font-size="6pt"       description="99999999999999"/>
        <style name="S3" length="27" font-size="5pt"     description="9999999999999999"/>
        <style name="S4" length="30" font-size="4.5pt" description="999999999999999999"/>
      </column>
      <column name="OrderCategory" column-width="20mm">
        <header>
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-OrderCategory'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <data length="21" font-size="7pt" font-weight="normal" text-align="center"/>
        <style name="S1" length="21" font-size="7pt"         description="999999999999"/>
        <style name="S2" length="24" font-size="6pt"       description="99999999999999"/>
        <style name="S3" length="27" font-size="5pt"     description="9999999999999999"/>
        <style name="S4" length="30" font-size="4.5pt" description="999999999999999999"/>
      </column>
      <column name="Qty" column-width="17mm">
        <header>
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Quantity'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="buy" text-align="{$gData_Header-align}">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Buy'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="sell" text-align="{$gData_Header-align}">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Sell'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="long" text-align="{$gData_Header-align}">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Long'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="short" text-align="{$gData_Header-align}">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Short'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <!-- FI 20150716 [20892] Add -->
        <header name="QtyAssessed" text-align="{$gData_Header-align}">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-QtyAssessed'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <!-- FI 20160613 [22256] Add-->
        <header name="QuantityAvailable" text-align="{$gData_Header-align}">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-QuantityAvailable'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <!-- FI 20160613 [22256] Add-->
        <header name="QuantityUsed" text-align="{$gData_Header-align}">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-QuantityUsed'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
      </column>
      <column name="Side" column-width="7mm">
        <header text-align="{$gData_Header-align}">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Side'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
      </column>
      <column name="SideQty1" column-width="27mm">
        <header text-align="{$gData_Header-align}">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Qty'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="Amount" text-align="{$gData_Header-align}">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Amount'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
      </column>
      <column name="SideQty2" column-width="14mm">
        <header text-align="{$gData_Header-align}">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Qty'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="Amount" text-align="{$gData_Header-align}">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Amount'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
      </column>
      <column name="Maturity" column-width="25mm">
        <header>
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Maturity'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <data text-align="center"/>
      </column>
      <column name="PC" column-width="5mm">
        <header>
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-PC'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
      </column>
      <column name="Strike" column-width="18mm">
        <header text-align="{$gData_Header-align}">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Strike'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
      </column>
      <column name="Underlying">
        <!--Cette colonne englobe les colonnes Maturity, PC et Strike pour les OTC,RTS,DSE,EST -->
        <header>
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Designation'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="StockDescription">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-StockDesignation'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="DebtSecurityDescription">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-DebtSecurityDesignation'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="ContractDescription">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-ContractDesignation'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <!-- 34 caractères max, alignement à gauche -->
        <!-- 31 caractères en 5.0 -->
        <data length="34" text-align="left" display-align="before">
          <xsl:attribute name="resource-Dlv">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Dlv'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resource-TimeFrom">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-TimeFrom'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resource-TimeTo">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-TimeTo'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <!-- pilote l'affichage du CODEISIN -->
        <data name="det" font-weight="normal" font-size="5pt" time-format="HH24:MI"/>
      </column>
      <column name="Rate">
        <!--Cette colonne englobe les colonnes Strike et Price pour les FxNDO-->
        <header>
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Rate'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
      </column>
      <column name="CashSettlt" >
        <data length="32" text-align="left" font-style="italic" color="#555555"/>
        <data name="Settlt">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Sett'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="Rate" length="23" >
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Rate'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="Fixing">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Fix'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
      </column>
      <column name="FxOption">
        <data length="32" text-align="left" font-style="italic">
          <buy color="blue"/>
          <sell color="darkred"/>
        </data>
        <data name="Expiry">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Exp'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="Style">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Style'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="dealt">
          <ccy font-weight="bold"/>
          <putCall font-weight="bold"/>
        </data>
        <data name="nonDealt">
          <ccy font-weight="bold"/>
          <putCall font-weight="normal"/>
        </data>
      </column>
      <!-- FI 20160623 [XXXXX] 22mm => 21mm  ( see I:\INST_SPHERES\Reports (exemples-maquettes)\EtatSynthetique -Taille des colonnes.xlsx)-->
      <column name="Payment" column-width="21mm">
        <header>
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Payment'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <data length="15" text-align="left"/>
        <data name="det" font-weight="normal"/>
        <data name="Funding">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Funding'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="Borrowing">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Borrowing'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="Safekeeping">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-CustodyFee'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
      </column>
      <!-- FI 20160623 [XXXXX] 110mm => 97.5 -->
      <column name="Designation" column-width="97.5mm">
        <header>
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Designation'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <data length="105" text-align="left"/>
        <data name="det" font-weight="normal"/>
        <data name="at">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-At'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="day">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Day'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="days">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Days'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="Withdrawal">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Withdrawal'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="Deposit">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Deposit'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="Debit">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Debit'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="Credit">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Credit'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
      </column>
      <!--FI 20160530 [21885] Add-->
      <column name="Designation2" column-width="65mm">
        <header>
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Designation'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <data length="60" text-align="left"/>
      </column>
      <!--FI 20160530 [21885] Add-->
      <column name="Designation2" column-width="65mm">
        <header>
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Designation'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <data length="60" text-align="left"/>
      </column>
      <column name="Price" column-width="17mm">
        <header text-align="{$gData_Header-align}">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Price'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
      </column>
      <!--Amounts Columns-->
      <column name="AmountDet" column-width="6mm">
        <data font-size="5pt" font-weight="normal" />
      </column>
      <column name="Ccy" column-width="4.5mm" font-size="5pt">
        <header name="Dealt">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Dealt'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
      </column>
      <column name="DrCr" column-width="3.5mm" text-align="center"/>
      <column name="Amount" column-width="17mm">
        <header>
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Amount'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="Fee">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Commiss.Fee'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="Commissions">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Fee'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="Margin">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Margin'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="Premium">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Premium'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="MarketRevaluation">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-MarketRevaluation'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <!-- FI 20160530 [21885] Add -->
        <header name="GrossMarketValue">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-GrossMarketValue'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <!-- FI 20160530 [21885] Add -->
        <header name="NetMarketValue">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-NetMarketValue'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="ContractValue">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-ContractValue'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="RealisedPnL">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-RealisedPnL'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="SettltQty">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-SettltQty'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="RefAmount">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-RefAmount'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <header name="NetAmount">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-NetAmount'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <data name="OTE">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-OpenTradeEqty'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="OPV">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-OpenPositionValue'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="GAM">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-GrossAmount'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="NET">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-NetConsideration'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="PAM">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-PrincipalAmount'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="AIN">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-AccruedInterestAmount'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="MKP">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-PrincipalAmount'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="MKA">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-AccruedInterestAmount'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="PRM">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Premium'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="CST">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-CashSettlementAmount'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <xsl:choose>
          <xsl:when test="$gcIsTestMode=true()">
            <style name="S1" length="6" font-size="7pt"      description="99.999.999,99"/>
            <style name="S2" length="8" font-size="6pt"   description="9.999.999.999,99"/>
            <style name="S3" length="9" font-size="5pt" description="999.999.999.999,99"/>
          </xsl:when>
          <xsl:otherwise>
            <style name="S1" length="13" font-size="7pt"      description="99.999.999,99"/>
            <style name="S2" length="16" font-size="6pt"   description="9.999.999.999,99"/>
            <style name="S3" length="18" font-size="5pt" description="999.999.999.999,99"/>
          </xsl:otherwise>
        </xsl:choose>
      </column>
      <!--FI 20160530 [21885] Add-->
      <column name="CssMIC" column-width="14mm">
        <header resource="CSS" text-align="{$gData_Header-align}"/>
        <data length="8" text-align="{$gData_text-align}"/>
        <data name="Others">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Others'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
      </column>
      <!--FI 20160530 [21885] Add -->
      <column name="Haircut" column-width="7mm" >
        <header resource="Haircut" text-align="{$gData_Header-align}"/>
        <data length="5" text-align="{$gData_Number_text-align}"/>
      </column>
      <!--FI 20160613 [22256] Add -->
      <column name="StockCover" column-width="21mm" >
        <header text-align="{$gData_Header-align}">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-StockCover'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <data length="17" text-align="left"/>
        <data name="StockOption">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-StockCover-StockOption'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="PriorityStockOption">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-StockCover-PriorityStockOption'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="StockFuture">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-StockCover-StockFuture'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="PriorityStockFuture">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-StockCover-PriorityStockFuture'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
      </column>
      <!--Title-->
      <title font-weight="bold" padding="0.15mm" background-color="#e6e6e6"/>
      <!--Subtotal-->
      <!-- EG 20160404 Migration vs2013 -->
      <subtotal font-weight="bold" padding="0.50mm" padding-bottom="0.15mm" display-align="center">
        <!--Data Columns-->
        <column name="Total" text-align="left">
          <data>
            <xsl:attribute name="resource">
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-Total'" />
              </xsl:call-template>
            </xsl:attribute>
          </data>
        </column>
        <column name="TotalInCV" text-align="left">
          <data>
            <xsl:attribute name="resource">
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-TotalInCounterValue'" />
              </xsl:call-template>
            </xsl:attribute>
          </data>
        </column>
        <column name="Qty">
          <data background-color="#e6e6e6"/>
        </column>
        <column name="Side">
          <data background-color="#e6e6e6"/>
        </column>
        <column name="Expiry" font-size="6pt" font-weight="normal" color="{$gAssetBanner-color}">
          <data>
            <xsl:attribute name="resource">
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-Exp'" />
              </xsl:call-template>
            </xsl:attribute>
          </data>
        </column>
        <column name="AvgPx" font-size="6pt" font-weight="normal">
          <data name="buy">
            <xsl:attribute name="resource">
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-AvgBuy'" />
              </xsl:call-template>
            </xsl:attribute>
          </data>
          <data name="sell">
            <xsl:attribute name="resource">
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-AvgSell'" />
              </xsl:call-template>
            </xsl:attribute>
          </data>
          <data name="buySell">
            <xsl:attribute name="resource">
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-AvgBuySell'" />
              </xsl:call-template>
            </xsl:attribute>
          </data>
          <data name="long">
            <xsl:attribute name="resource">
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-AvgLong'" />
              </xsl:call-template>
            </xsl:attribute>
          </data>
          <data name="short">
            <xsl:attribute name="resource">
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-AvgShort'" />
              </xsl:call-template>
            </xsl:attribute>
          </data>
          <data name="longShort">
            <xsl:attribute name="resource">
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-AvgLongShort'" />
              </xsl:call-template>
            </xsl:attribute>
          </data>
        </column>
        <column name="SettlPx" font-size="6pt" font-weight="normal" color="{$gAssetBanner-color}" text-align="left">
          <data>
            <xsl:attribute name="resource">
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-SettlPrice'" />
              </xsl:call-template>
            </xsl:attribute>
          </data>
          <data name="FwdRate">
            <xsl:attribute name="resource">
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-FwdRate'" />
              </xsl:call-template>
            </xsl:attribute>
          </data>
          <data name="TheoPrice">
            <xsl:attribute name="resource">
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-TheoPrice'" />
              </xsl:call-template>
            </xsl:attribute>
          </data>
        </column>
        <column name="UnlSettlPx" font-size="6pt" font-weight="bold">
          <data>
            <xsl:attribute name="resource">
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-UnlSettlPrice'" />
              </xsl:call-template>
            </xsl:attribute>
          </data>
        </column>
        <column name="SpotRate" font-size="6pt" font-weight="normal"/>
        <column name="DrCr" font-weight="bold" text-align="center"/>
        <column name="AccIntDays" font-size="6pt" font-weight="normal" color="{$gAssetBanner-color}">
          <data resource="Acc. Days:"/>
        </column>
        <!-- EG 20190730 New : Taux du CC (sur MKA) -->
        <column name="AccRate" font-size="6pt" font-weight="normal" color="{$gAssetBanner-color}">
          <data resource="Acc.Rate"/>
        </column>
      </subtotal>
    </block>
    <block name="Summary" space-height="3mm" background-color="#e6e6e6" background-color2="#d9d9d9" minor-color="#5c5c5c" number-ccy="4" >
      <column name="Designation1" column-width="5mm"/>
      <column name="Designation2" column-width="23mm" text-align="left">
        <header>
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Designation'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
        <data name="Det" font-size="5pt" font-weight="normal"/>
        <data name="SpotRate">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-SpotRate'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="PreviousCashBalance">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-OpeningCashBalance'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="RealisedProfitLoss">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-RealisedProfitLoss'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetail">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-RealisedProfitLossDetail'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailFutures">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-RealisedProfitLossFutures'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailOptions">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-RealisedProfitLossOptions'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailStocks">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-RealisedProfitLossStocks'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailOthers">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-RealisedProfitLossOthers'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="CashSettlementOptions">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-CashSettlementOptions'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailStocks">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-DetailStocks'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailDebtSecurities">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-DetailDebtSecurities'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailOthers">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-CashSettlementOthers'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="Premium">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-OptionPremium'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="Fee">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-CommissionsFees'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetail">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-DetailCommissionsFees'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="Funding">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Funding'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="Borrowing">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Borrowing'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="CashBalancePayment">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-CashActivity'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="EqualisationPayment">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-EqualisationPayment'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="CashBalance">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-ClosingCashBalance'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="CashBalance_CSBDEFAULT">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-ClosingCashBalance'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="OpenTradeEqty">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-OpenTradeEqty'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetail">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-DetailOpenTradeEqty'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailFutures">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Futures'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailOptions">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Options'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailStocks">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-DetailStocks'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailDebtSecurities">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-DetailDebtSecurities'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailOthers">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-DetailOthers'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="MarketRevaluation">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-MarketRevaluation'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetail">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-DetailMarketRevaluation'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailFutures">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Futures'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailOptions">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Options'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailStocks">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-DetailStocks'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailDebtSecurities">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-DetailDebtSecurities'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailOthers">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-DetailOthers'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="UnrealisedMargin">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-UnrealisedMargin'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetail">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-UnrealisedMarginDetail'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailFutures">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Futures'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailOptions">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-Options'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailStocks">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-DetailStocks'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailDebtSecurities">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-DetailDebtSecurities'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailOthers">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-DetailOthers'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="UnsettledTransactions">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-UnsettledTransactions'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetail">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-DetailUnsettledTransactions'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailStocks">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-DetailStocks'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailDebtSecurities">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-DetailDebtSecurities'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailOthers">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-DetailOthers'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <xsl:variable name="vTotalEqty_Resource">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-TotalEqty'" />
          </xsl:call-template>
        </xsl:variable>
        <data name="TotalEqty" resource="{$vTotalEqty_Resource}" />
        <data name="TotalEqtyWithUT" resource="{$vTotalEqty_Resource}" />
        <data name="NetOptionValue">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-NetOptionValue'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetail">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-DetailNetOptionValue'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="LongOptionValue">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-LongOptionValue'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="ShortOptionValue">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-ShortOptionValue'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="OpenPositionValue">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-OpenPositionValue'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetail">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-DetailOpenPositionValue'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailStocks">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-DetailStocks'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailDebtSecurities">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-DetailDebtSecurities'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetailOthers">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-DetailOthers'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <xsl:variable name="vTotalAccountValue_Resource">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-TotalAccountValue'" />
          </xsl:call-template>
        </xsl:variable>
        <data name="TotalAccountValue" resource="{$vTotalAccountValue_Resource}" />
        <data name="TotalAccountValueWithNOV" resource="{$vTotalAccountValue_Resource}" />
        <data name="TotalAccountValueWithOPV" resource="{$vTotalAccountValue_Resource}" />
        <data name="InitialMarginReq_CSBUK" >
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-InitialMarginReq'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="InitialMarginReq_CSBDEFAULT">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-InitialMarginReq'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="InitialMarginCall">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-InitialMarginCall'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="SecuritiesOnDeposit">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-SecuritiesOnDeposit'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <xsl:variable name="vMarginDeficitExcess_Resource">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-MarginDeficitExcess'" />
          </xsl:call-template>
        </xsl:variable>
        <data name="MarginDeficitExcess" resource="{$vMarginDeficitExcess_Resource}" />
        <data name="MarginDeficitExcessNoSOD" resource="{$vMarginDeficitExcess_Resource}" />
        <data name="MarginCall_CSBDEFAULT">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-CBMarginCall'" />
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute name="resourceDetail">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-CBMarginCallDetail'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="MarginCallDetail_CSBDEFAULT" resourceInline="S1+D1) - (S2+D2">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-CBMarginCall'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="Collateral">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-CBCollateral'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="Cash">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-CBCash'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="UncoveredMargin">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-CBUncoveredMargin'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="CashSettlement">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-CBCashSettlement'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
        <data name="VariationMargin">
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-CBVariationMargin'" />
            </xsl:call-template>
          </xsl:attribute>
        </data>
      </column>
      <column name ="Designation3" column-width="10mm"></column>
      <column name ="Designation4" column-width="12mm" text-align="left"></column>
      <column name ="Currency" column-width="34mm">
        <data name ="SpotRate" font-size="6pt" font-weight="normal" text-align="left"/>
        <data name ="det" font-size="5pt" font-weight="normal"/>
      </column>
      <column name="DrCr" column-width="3.5mm" font-weight="normal" text-align="center"/>
      <column name="BaseCurrency" column-width="34mm">
        <header>
          <xsl:attribute name="resource">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-BaseCcy'" />
            </xsl:call-template>
          </xsl:attribute>
        </header>
      </column>
    </block>
    <block name="Legend"
           space-before="2mm"
           padding="0.15mm"
           display-align="{$gData_display-align}">
      <column name="Value" column-width="6.5mm">
        <data font-size="6pt" font-weight="normal" />
      </column>
      <column name="ExtValue" column-width="22mm">
        <data font-size="6pt" font-weight="normal" />
      </column>
    </block>
  </xsl:variable>
  <xsl:variable name="gBlockSettings" select="msxsl:node-set($gBlockSettingsNode)"/>
  <xsl:variable name="gBlockSettings_Section" select="$gBlockSettings/block[@name='Section']"/>
  <xsl:variable name="gBlockSettings_Date" select="$gBlockSettings/block[@name='Date']"/>
  <xsl:variable name="gBlockSettings_MarketDC" select="$gBlockSettings/block[@name='MarketDC']"/>
  <xsl:variable name="gBlockSettings_MarketCcy" select="$gBlockSettings/block[@name='MarketCcy']"/>
  <xsl:variable name="gBlockSettings_Book" select="$gBlockSettings/block[@name='Book']"/>
  <xsl:variable name="gBlockSettings_Data" select="$gBlockSettings/block[@name='Data']"/>
  <xsl:variable name="gBlockSettings_Summary" select="$gBlockSettings/block[@name='Summary']"/>
  <xsl:variable name="gBlockSettings_Legend" select="$gBlockSettings/block[@name='Legend']"/>
  <xsl:variable name="gBlockSettings_Asset" select="$gBlockSettings/block[@name='Asset']"/>
  <xsl:variable name="gBlockSettings_Trader" select="$gBlockSettings/block[@name='Trader']"/>

  <!-- ============================================================================================== -->
  <!--                                              Template                                          -->
  <!-- ============================================================================================== -->

  <!-- .......................................................................... -->
  <!--              Common Section                                                -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- Synthesis_Section                                -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Synthesis Section                       -->
  <!-- ................................................ -->
  <xsl:template name="Synthesis_Section">
    <xsl:param name="pSectionKey"/>
    <xsl:param name="pBusiness_Section"/>
    <xsl:param name="pIsDisplayInstr" select="false()"/>
    <xsl:param name="pDetail"/>

    <fo:table table-layout="fixed" table-omit-header-at-break="false" table-omit-footer-at-break="true" width="{$gSection_width}">

      <fo:table-column column-number="01" column-width="{$gSectionTab1_width}">
        <xsl:call-template name="Debug_border-red"/>
      </fo:table-column>
      <fo:table-column column-number="02" column-width="{$gSectionTab1_width}">
        <xsl:call-template name="Debug_border-red"/>
      </fo:table-column>
      <!--Header: Section name and Instrument-->
      <xsl:variable name="vInstr">
        <xsl:if test="$pIsDisplayInstr = true()">
          <xsl:call-template name="DisplayInstr">
            <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
            <xsl:with-param name="pIdI" select="$pBusiness_Section//asset[1]/@idI | $pBusiness_Section//dealt[1]/@idI"/>
          </xsl:call-template>
        </xsl:if>
      </xsl:variable>
      <xsl:variable name="vValueDate">
        <xsl:if test="contains(concat(',',$gcReportSectionKey_POS,',',$gcReportSectionKey_STL,',',$gcReportSectionKey_UNS,','),concat(',',$pSectionKey,',')) and $gIsPeriodicReport = true()">
          <xsl:call-template name="format-date">
            <xsl:with-param name="xsd-date-time" select="normalize-space($gValueDate2)" />
          </xsl:call-template>
        </xsl:if>
      </xsl:variable>
      <xsl:call-template name="UKSection_Header">
        <xsl:with-param name="pResource" select="$gBlockSettings_Section/section[@key=$pSectionKey]/@resource"/>
        <xsl:with-param name="pInstr" select="$vInstr"/>
        <xsl:with-param name="pDetail" select="$pDetail"/>
        <xsl:with-param name="pValueDate" select="$vValueDate"/>
      </xsl:call-template>

      <xsl:if test="$pBusiness_Section">
        <!-- Footer: Legend-->
        <xsl:variable name="vLegendNode">

          <xsl:call-template name="UKSection_GetLegendFee">
            <xsl:with-param name="pSubTotalFees" select="$pBusiness_Section//asset/subTotal/fee | $pBusiness_Section//dealt/subTotal/fee"/>
          </xsl:call-template>
          <xsl:call-template name="UKSection_GetLegendCashPayment">
            <xsl:with-param name="pCashPayments" select="$pBusiness_Section//cashPayment"/>
          </xsl:call-template>
          <xsl:call-template name="UKSection_GetLegendFunding">
            <xsl:with-param name="pFundings" select="$pBusiness_Section//posSynthetic[@eventType='FDA']"/>
          </xsl:call-template>
          <xsl:call-template name="UKSection_GetLegendBorrowing">
            <xsl:with-param name="pBorrowings" select="$pBusiness_Section//posSynthetic[@eventType='BWA']"/>
          </xsl:call-template>
          <xsl:call-template name="UKSection_GetLegendSafekeeping">
            <xsl:with-param name="pSafekeepings" select="$pBusiness_Section//posSynthetic[@eventType='CUS']"/>
          </xsl:call-template>

          <!-- FI 20150716 [20892] OPV, MKP, MKA sur les positions en date de rglt -->
          <xsl:if test="($pBusiness_Section//asset/@family='ESE' or 
                         $pBusiness_Section//asset/@family='DSE') and
                         $pBusiness_Section//book/@sectionKey=$gcReportSectionKey_STL">

            <xsl:call-template name="UKSection_GetAmountLegend">
              <xsl:with-param name="pName" select="'marketRevaluation'"/>
              <xsl:with-param name="pType" select="'OPV'"/>
            </xsl:call-template>

            <!-- FI 20151019 [21317] MKP / MKA -->
            <xsl:if test ="$pBusiness_Section//asset/@family='DSE'">
              <xsl:if test ="$pBusiness_Section//asset/trade/amount//*[name()='pam']">

                <xsl:call-template name="UKSection_GetAmountLegend">
                  <xsl:with-param name="pName" select="'marketRevaluation'"/>
                  <xsl:with-param name="pType" select="'MKP'"/>
                </xsl:call-template>
              </xsl:if>
              <xsl:if test ="$pBusiness_Section//asset/trade/amount//*[name()='ain']">
                <xsl:call-template name="UKSection_GetAmountLegend">
                  <xsl:with-param name="pName" select="'marketRevaluation'"/>
                  <xsl:with-param name="pType" select="'MKA'"/>
                </xsl:call-template>
              </xsl:if>
            </xsl:if>

          </xsl:if>

          <!-- FI 20150827 [21287] GAM sur Amendments / transfers -->
          <xsl:if test="(($pBusiness_Section//asset/@family='ESE') or 
                         ($pBusiness_Section//asset/@family='DSE'))  and 
                          $pBusiness_Section//book/@sectionKey=$gcReportSectionKey_AMT">
            <xsl:call-template name="UKSection_GetAmountLegend">
              <xsl:with-param name="pName" select="'contractValue'"/>
              <xsl:with-param name="pType" select="'GAM'"/>
            </xsl:call-template>
          </xsl:if>

          <!-- FI 20150827 [21287] GAM ,NET ,PAM ,AIN sur Trades, unsettled Trades, settled Trades  
                                   OTE sur  unsettled Trades
          -->
          <xsl:if test="(contains(',ESE,DSE,COMS,',concat(',',$pBusiness_Section//asset/@family,',')))  and 
                  contains(concat(',',$gcReportSectionKey_TRD,',',$gcReportSectionKey_SET,',',$gcReportSectionKey_UNS,','),concat(',',$pBusiness_Section//book/@sectionKey,','))">

            <xsl:if test ="$pBusiness_Section//asset/trade/amount//*[name()='gam']">
              <xsl:call-template name="UKSection_GetAmountLegend">
                <xsl:with-param name="pName" select="'contractValue'"/>
                <xsl:with-param name="pType" select="'GAM'"/>
              </xsl:call-template>
            </xsl:if>

            <xsl:if test="$pBusiness_Section//asset/@family != 'COMS'">
              <xsl:call-template name="UKSection_GetAmountLegend">
                <xsl:with-param name="pName" select="'contractValue'"/>
                <xsl:with-param name="pType" select="'NET'"/>
              </xsl:call-template>
            </xsl:if>

            <xsl:if test ="$pBusiness_Section//asset/@family='DSE'">
              <xsl:if test ="$pBusiness_Section//asset/trade/amount//*[name()='pam']">
                <xsl:call-template name="UKSection_GetAmountLegend">
                  <xsl:with-param name="pName" select="'contractValue'"/>
                  <xsl:with-param name="pType" select="'PAM'"/>
                </xsl:call-template>
              </xsl:if>
              <xsl:if test ="$pBusiness_Section//asset/trade/amount//*[name()='ain']">
                <xsl:call-template name="UKSection_GetAmountLegend">
                  <xsl:with-param name="pName" select="'contractValue'"/>
                  <xsl:with-param name="pType" select="'AIN'"/>
                </xsl:call-template>
              </xsl:if>
            </xsl:if>

            <xsl:if test ="$pBusiness_Section//book/@sectionKey=$gcReportSectionKey_UNS">
              <xsl:if test ="$pBusiness_Section//asset/trade/amount//*[name()='umg']">
                <xsl:call-template name="UKSection_GetAmountLegend">
                  <xsl:with-param name="pName" select="'marketRevaluation'"/>
                  <xsl:with-param name="pType" select="'OTE'"/>
                </xsl:call-template>
              </xsl:if>
            </xsl:if>


          </xsl:if>
          <!-- FI 20160208 [21311] add Legeng on LSD (section Exe/ASS/ABN)         -->
          <xsl:if test="$pBusiness_Section//asset[@assetName='etd'] and
                        $pBusiness_Section//book/@sectionKey=$gcReportSectionKey_EXA">

            <xsl:if test ="$pBusiness_Section//asset[@assetName='etd']/posAction/amount//*[name()='rmg']">
              <xsl:call-template name="UKSection_GetAmountLegend">
                <xsl:with-param name="pName" select="'PnL'"/>
                <xsl:with-param name="pType" select="'PRM'"/>
              </xsl:call-template>
            </xsl:if>

            <xsl:if test ="$pBusiness_Section//asset[@assetName='etd']/posAction/amount//*[name()='scu']">
              <xsl:call-template name="UKSection_GetAmountLegend">
                <xsl:with-param name="pName" select="'PnL'"/>
                <xsl:with-param name="pType" select="'CST'"/>
              </xsl:call-template>
            </xsl:if>
          </xsl:if>
        </xsl:variable>


        <xsl:variable name="vLegendList" select="msxsl:node-set($vLegendNode)/legend"/>
        <xsl:call-template name="UKSection_Footer">
          <xsl:with-param name="pLegends" select="$vLegendList"/>
        </xsl:call-template>
      </xsl:if>

      <fo:table-body>
        <fo:table-row>
          <fo:table-cell number-columns-spanned="2">
            <xsl:call-template name="Synthesis_SectionBody">
              <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
              <xsl:with-param name="pBusiness_Section" select="$pBusiness_Section"/>
            </xsl:call-template>
          </fo:table-cell>
        </fo:table-row>
      </fo:table-body>
    </fo:table>
  </xsl:template>

  <!-- ................................................ -->
  <!-- Synthesis_SectionBody                            -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Synthesis Section Body                  -->
  <!-- ................................................ -->
  <xsl:template name="Synthesis_SectionBody">
    <xsl:param name="pSectionKey"/>
    <xsl:param name="pBusiness_Section"/>
    <xsl:param name="pBizDt"/>

    <xsl:choose>
      <xsl:when test="$pBusiness_Section = false()">
        <xsl:call-template name="UKDisplay_NoActivityMsg">
          <xsl:with-param name="pResource" select="$gBlockSettings_Section/section[@key=$pSectionKey]/data[@name='NotAvailable']/@resource"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pBusiness_Section[name()='date']">
        <xsl:call-template name="Synthesis_SectionDate">
          <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
          <xsl:with-param name="pBusiness_Section-date" select="$pBusiness_Section"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pBusiness_Section[name()='book']">
        <xsl:call-template name="Synthesis_SectionBook">
          <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
          <xsl:with-param name="pBusiness_Section-book" select="$pBusiness_Section"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pBusiness_Section[name()='group']">
        <xsl:call-template name="Synthesis_SectionSummary">
          <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
          <xsl:with-param name="pBusiness_Section-group" select="$pBusiness_Section"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pBusiness_Section[name()='market']/derivativeContract">
        <xsl:call-template name="Synthesis_SectionMarketDC">
          <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
          <xsl:with-param name="pBusiness_Section-market" select="$pBusiness_Section"/>
          <xsl:with-param name="pBizDt" select="$pBizDt"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pBusiness_Section[name()='market']/currency">
        <xsl:call-template name="Synthesis_SectionMarketCurrency">
          <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
          <xsl:with-param name="pBusiness_Section-market" select="$pBusiness_Section"/>
          <xsl:with-param name="pBizDt" select="$pBizDt"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="Synthesis_SectionMarket">
          <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
          <xsl:with-param name="pBusiness_Section-market" select="$pBusiness_Section"/>
          <xsl:with-param name="pBizDt" select="$pBizDt"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- ................................................ -->
  <!-- Synthesis_SectionDate                          -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Synthesis Date Section (bizDt, ...)     -->
  <!-- ................................................ -->
  <xsl:template name="Synthesis_SectionDate">
    <xsl:param name="pSectionKey"/>
    <xsl:param name="pBusiness_Section-date"/>

    <xsl:for-each select="$pBusiness_Section-date">
      <xsl:sort select="current()/@bizDt" data-type="text"/>

      <xsl:variable name="vBizDate" select="current()"/>

      <xsl:choose>
        <xsl:when test="contains(concat(',',$gcReportSectionKey_POS,',',$gcReportSectionKey_STL,',',$gcReportSectionKey_UNS,','),concat(',',$pSectionKey,',')) or $gIsPeriodicReport = false()">
          <!-- Content-->
          <xsl:call-template name="Synthesis_SectionBody">
            <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
            <xsl:with-param name="pBusiness_Section" select="$vBizDate/*"/>
            <xsl:with-param name="pBizDt" select="$vBizDate/@bizDt"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <!--Date-->
          <fo:table table-layout="fixed" table-omit-header-at-break="false" table-omit-footer-at-break="true"
                    keep-together.within-page="always" width="{$gSection_width}">

            <fo:table-column column-number="01" column-width="{$gSection_width}">
              <xsl:call-template name="Debug_border-red"/>
            </fo:table-column>

            <fo:table-header>
              <fo:table-row font-size="{$gBlockSettings_Date/column[@name='Label']/@font-size}">
                <fo:table-cell padding="{$gBlockSettings_Date/column[@name='Label']/@padding}"
                               font-weight="{$gBlockSettings_Date/column[@name='Label']/@font-weight}"
                               text-align="{$gBlockSettings_Date/column[@name='Label']/@text-align}"
                               display-align="{$gBlockSettings_Date/column[@name='Label']/@display-align}"
                               background-color="{$gBlockSettings_Date/column[@name='Label']/@background-color}"
                               border="{$gBlockSettings_Date/@border}" border-left-style="none" border-right-style="none" >
                  <xsl:if test="$gIsColorMode">
                    <xsl:attribute name="color">
                      <xsl:value-of select="$gBlockSettings_Date/column[@name='Label']/@color"/>
                    </xsl:attribute>
                  </xsl:if>
                  <!-- Header -->
                  <fo:block>
                    <xsl:variable name="vDate">
                      <xsl:call-template name="format-date">
                        <xsl:with-param name="xsd-date-time" select="normalize-space($vBizDate/@bizDt)" />
                      </xsl:call-template>
                    </xsl:variable>
                    <xsl:variable name="vSectionName">
                      <xsl:call-template name="Lower">
                        <xsl:with-param name="source" select="$gBlockSettings_Section/section[@key=$pSectionKey]/@resource"/>
                      </xsl:call-template>
                    </xsl:variable>
                    <xsl:variable name="vRessource">
                      <xsl:choose>
                        <xsl:when test="$pSectionKey = $gcReportSectionKey_TRD">
                          <xsl:value-of select="$gBlockSettings_Date/column[@name='Label']/data[@name='RecordedAsOf']/@resource"/>
                        </xsl:when>
                        <xsl:when test="$pSectionKey = $gcReportSectionKey_SET">
                          <xsl:value-of select="$gBlockSettings_Date/column[@name='Label']/data[@name='SettledAsOf']/@resource"/>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="$gBlockSettings_Date/column[@name='Label']/data[@name='MadeAsOf']/@resource"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:variable>
                    <xsl:variable name="vDateDesc">
                      <xsl:call-template name="Replace">
                        <xsl:with-param name="source" select="$vRessource"/>
                        <xsl:with-param name="oldValue" select="'{0}'"/>
                        <xsl:with-param name="newValue" select="$vSectionName"/>
                      </xsl:call-template>
                    </xsl:variable>
                    <xsl:call-template name="Replace">
                      <xsl:with-param name="source" select="$vDateDesc"/>
                      <xsl:with-param name="oldValue" select="'{1}'"/>
                      <xsl:with-param name="newValue" select="$vDate"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <fo:table-row>
                <fo:table-cell>
                  <!--Space-->
                  <fo:block space-before="{$gBlock_space-after}"/>
                </fo:table-cell>
              </fo:table-row>
            </fo:table-header>
            <fo:table-body>
              <fo:table-row>
                <fo:table-cell>
                  <!-- Content-->
                  <xsl:call-template name="Synthesis_SectionBody">
                    <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
                    <xsl:with-param name="pBusiness_Section" select="$vBizDate/*"/>
                    <xsl:with-param name="pBizDt" select="$vBizDate/@bizDt"/>
                  </xsl:call-template>
                </fo:table-cell>
              </fo:table-row>
            </fo:table-body>
          </fo:table>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>

  <!-- ................................................ -->
  <!-- Synthesis_SectionMarketDC                      -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Synthesis Market Section                -->
  <!-- ................................................ -->
  <xsl:template name="Synthesis_SectionMarketDC">
    <xsl:param name="pSectionKey"/>
    <xsl:param name="pBusiness_Section-market"/>
    <xsl:param name="pBizDt"/>

    <xsl:for-each select="$pBusiness_Section-market">
      <xsl:sort select="concat($gRepository/market[@OTCmlId=current()/@OTCmlId]/shortIdentifier,'-',$gRepository/market[@OTCmlId=current()/@OTCmlId]/displayname)" data-type="text" order="ascending"/>
      <!--<xsl:sort select="$gRepository/market[@OTCmlId=current()/@OTCmlId]/displayname" data-type="text" order="ascending"/>-->
      <xsl:sort select="$gRepository/derivativeContract[@OTCmlId=current()/derivativeContract/@OTCmlId]/displayname" data-type="text" order="ascending"/>

      <xsl:sort select="$gRepository/etd['etd'=current()/derivativeContract/book/asset/@assetName and @OTCmlId=current()/derivativeContract/book/asset/@OTCmlId]/maturityMonthYear" data-type="text"/>
      <xsl:sort select="$gRepository/etd['etd'=current()/derivativeContract/book/asset/@assetName and @OTCmlId=current()/derivativeContract/book/asset/@OTCmlId]/putCall" data-type="text"/>
      <xsl:sort select="$gRepository/etd['etd'=current()/derivativeContract/book/asset/@assetName and @OTCmlId=current()/derivativeContract/book/asset/@OTCmlId]/strikePrice" data-type="number"/>

      <xsl:variable name="vBizMarket" select="current()"/>
      <xsl:variable name="vRepository-market" select="$gRepository/market[@OTCmlId=$vBizMarket/@OTCmlId]"/>
      <xsl:variable name="vBizDC" select="$vBizMarket/derivativeContract"/>
      <xsl:variable name="vRepository-derivativeContract" select="$gRepository/derivativeContract[@OTCmlId=$vBizDC/@OTCmlId]"/>

      <fo:table table-layout="fixed" table-omit-header-at-break="false" table-omit-footer-at-break="true"
                keep-together.within-page="always" width="{$gSection_width}">

        <fo:table-column column-number="01" column-width="{$gSection_width}">
          <xsl:call-template name="Debug_border-red"/>
        </fo:table-column>
        <fo:table-header>
          <fo:table-row>
            <fo:table-cell>
              <!-- Header -->
              <xsl:call-template name="UKMarketDC_Header">
                <xsl:with-param name="pRepository-market" select="$vRepository-market"/>
                <xsl:with-param name="pBiz-derivativeContract" select="$vBizDC"/>
                <xsl:with-param name="pRepository-derivativeContract" select="$vRepository-derivativeContract"/>
              </xsl:call-template>
              <!--Space-->
              <fo:block space-before="{$gBlock_space-after}"/>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-header>
        <fo:table-body>
          <fo:table-row>
            <fo:table-cell>
              <xsl:call-template name="Synthesis_SectionBook">
                <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
                <xsl:with-param name="pBusiness_Section-book" select="$vBizDC/book"/>
                <xsl:with-param name="pBizDt" select="$pBizDt"/>
                <xsl:with-param name="pBizDC" select="$vBizDC"/>
              </xsl:call-template>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </xsl:for-each>
  </xsl:template>

  <!-- ................................................ -->
  <!-- Synthesis_SectionMarketCurrency                -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Synthesis Market Section                -->
  <!-- ................................................ -->
  <xsl:template name="Synthesis_SectionMarketCurrency">
    <xsl:param name="pSectionKey"/>
    <xsl:param name="pBusiness_Section-market"/>
    <xsl:param name="pBizDt"/>

    <xsl:for-each select="$pBusiness_Section-market">
      <xsl:sort select="$gRepository/market[@OTCmlId=current()/@OTCmlId]/shortIdentifier" data-type="text" order="ascending"/>
      <xsl:sort select="$gRepository/market[@OTCmlId=current()/@OTCmlId]/displayname" data-type="text" order="ascending"/>
      <xsl:sort select="current()/currency/@ccy" data-type="text" order="ascending"/>

      <xsl:variable name="vBizMarket" select="current()"/>
      <xsl:variable name="vRepository-market" select="$gRepository/market[@OTCmlId=$vBizMarket/@OTCmlId]"/>
      <xsl:variable name="vBizCurrency" select="$vBizMarket/currency"/>
      <xsl:variable name="vRepository-currency" select="$gRepository/currency[identifier=$vBizCurrency/@ccy]"/>

      <fo:table table-layout="fixed" table-omit-header-at-break="false" table-omit-footer-at-break="true"
                keep-together.within-page="always" width="{$gSection_width}">

        <fo:table-column column-number="01" column-width="{$gSection_width}">
          <xsl:call-template name="Debug_border-red"/>
        </fo:table-column>
        <fo:table-header>
          <fo:table-row>
            <fo:table-cell>
              <!-- Header -->
              <xsl:call-template name="UKMarketCurrency_Header">
                <xsl:with-param name="pRepository-market" select="$vRepository-market"/>
                <xsl:with-param name="pRepository-currency" select="$vRepository-currency"/>
              </xsl:call-template>
              <!--Space-->
              <fo:block space-before="{$gBlock_space-after}"/>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-header>
        <fo:table-body>
          <fo:table-row>
            <fo:table-cell>
              <xsl:call-template name="Synthesis_SectionBook">
                <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
                <xsl:with-param name="pBusiness_Section-book" select="$vBizMarket/book"/>
                <xsl:with-param name="pBizDt" select="$pBizDt"/>
                <xsl:with-param name="pBizCurrency" select="$vBizCurrency"/>
              </xsl:call-template>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </xsl:for-each>
  </xsl:template>

  <!-- ................................................ -->
  <!-- Synthesis_SectionMarket                        -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Synthesis Market Section                -->
  <!-- ................................................ -->
  <xsl:template name="Synthesis_SectionMarket">
    <xsl:param name="pSectionKey"/>
    <xsl:param name="pBusiness_Section-market"/>
    <xsl:param name="pBizDt"/>

    <xsl:for-each select="$pBusiness_Section-market">
      <xsl:sort select="$gRepository/market[@OTCmlId=current()/@OTCmlId]/shortIdentifier" data-type="text" order="ascending"/>
      <xsl:sort select="$gRepository/market[@OTCmlId=current()/@OTCmlId]/displayname" data-type="text" order="ascending"/>

      <xsl:variable name="vBizMarket" select="current()"/>
      <xsl:variable name="vRepository-market" select="$gRepository/market[@OTCmlId=$vBizMarket/@OTCmlId]"/>

      <fo:table table-layout="fixed" table-omit-header-at-break="false" table-omit-footer-at-break="true"
                keep-together.within-page="always" width="{$gSection_width}">

        <fo:table-column column-number="01" column-width="{$gSection_width}">
          <xsl:call-template name="Debug_border-red"/>
        </fo:table-column>
        <fo:table-header>
          <fo:table-row>
            <fo:table-cell>
              <!-- Header -->
              <xsl:call-template name="UKMarket_Header">
                <xsl:with-param name="pRepository-market" select="$vRepository-market"/>
              </xsl:call-template>
              <!-- Space -->
              <fo:block space-before="{$gBlock_space-after}"/>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-header>
        <fo:table-body>
          <fo:table-row>
            <fo:table-cell>
              <xsl:call-template name="Synthesis_SectionBook">
                <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
                <xsl:with-param name="pBusiness_Section-book" select="$vBizMarket/book"/>
                <xsl:with-param name="pBizDt" select="$pBizDt"/>
              </xsl:call-template>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </xsl:for-each>
  </xsl:template>

  <!-- ................................................ -->
  <!-- Synthesis_SectionBook                          -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Synthesis Book Section                  -->
  <!-- ................................................ -->
  <xsl:template name="Synthesis_SectionBook">
    <xsl:param name="pSectionKey"/>
    <xsl:param name="pBusiness_Section-book"/>
    <xsl:param name="pBizDt"/>
    <xsl:param name="pBizDC"/>
    <xsl:param name="pBizCurrency"/>

    <xsl:for-each select="$pBusiness_Section-book">
      <xsl:sort select="$gRepository/book[@OTCmlId=current()/@OTCmlId]/identifier" data-type="text"/>

      <xsl:variable name="vBizBook" select="current()"/>
      <xsl:variable name="vRepository-book" select="$gRepository/book[@OTCmlId=$vBizBook/@OTCmlId]"/>

      <xsl:choose>
        <xsl:when test="count($pBusiness_Section-book)=1 and $gMainBook = $vRepository-book/identifier">
          <!-- Content-->
          <xsl:call-template name="Synthesis_SectionData">
            <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
            <xsl:with-param name="pBizData" select="$vBizBook"/>
            <xsl:with-param name="pBizDt" select="$pBizDt"/>
            <xsl:with-param name="pBizDC" select="$pBizDC"/>
            <xsl:with-param name="pBizCurrency" select="$pBizCurrency"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <!--Book-->
          <fo:table table-layout="fixed" table-omit-header-at-break="false" table-omit-footer-at-break="true"
                    keep-together.within-page="always" width="{$gSection_width}">

            <fo:table-column column-number="01" column-width="{$gSection_width}">
              <xsl:call-template name="Debug_border-red"/>
            </fo:table-column>

            <fo:table-header>
              <fo:table-row>
                <fo:table-cell>
                  <!-- Header -->
                  <xsl:call-template name="UKBook_Header">
                    <xsl:with-param name="pRepository-book" select="$vRepository-book"/>
                  </xsl:call-template>
                  <!--Space-->
                  <fo:block space-before="{$gBlock_space-after}"/>
                </fo:table-cell>
              </fo:table-row>
            </fo:table-header>
            <fo:table-body>
              <fo:table-row>
                <fo:table-cell>
                  <!-- Content-->
                  <xsl:call-template name="Synthesis_SectionData">
                    <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
                    <xsl:with-param name="pBizData" select="$vBizBook"/>
                    <xsl:with-param name="pBizDt" select="$pBizDt"/>
                    <xsl:with-param name="pBizDC" select="$pBizDC"/>
                    <xsl:with-param name="pBizCurrency" select="$pBizCurrency"/>
                  </xsl:call-template>
                </fo:table-cell>
              </fo:table-row>
            </fo:table-body>
          </fo:table>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>

  <!-- ................................................ -->
  <!-- Synthesis_SectionSummary                       -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Synthesis Summary Section               -->
  <!-- ................................................ -->
  <!-- FI 20150921 [21311] Modify -->
  <xsl:template name="Synthesis_SectionSummary">
    <xsl:param name="pSectionKey"/>
    <xsl:param name="pBusiness_Section-group"/>

    <xsl:for-each select="$pBusiness_Section-group">
      <xsl:sort select="current()/@sort"/>

      <xsl:variable name="vBizGroup" select="current()"/>

      <!-- Content-->
      <xsl:call-template name="Synthesis_SectionData">
        <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
        <xsl:with-param name="pBizData" select="$vBizGroup"/>
      </xsl:call-template>

      <!--Space-->
      <fo:block space-before="{$gBlock_space-before}"/>
    </xsl:for-each>
  </xsl:template>

  <!-- ................................................ -->
  <!-- Synthesis_SectionData                          -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Synthesis Section Data                  -->
  <!-- ................................................ -->
  <!-- FI 20150921 [21311] Modify (supression paramètre pIsHideBaseCcy)-->
  <!-- FI 20160530 [21885] Modify -->
  <!-- RD 20190619 [23912] Modify -->
  <xsl:template name="Synthesis_SectionData">
    <xsl:param name="pSectionKey"/>
    <xsl:param name="pBizData"/>
    <xsl:param name="pBizDt"/>
    <xsl:param name="pBizDC"/>
    <xsl:param name="pBizCurrency"/>

    <xsl:choose>
      <!-- RD 20190619 [23912] Add order confirmation section -->
      <xsl:when test="contains(concat(',',$gcReportSectionKey_ORD,','),concat(',',$pSectionKey,','))">
        <xsl:choose>
          <xsl:when test="$pBizData[name()='book']/dealt"/>
          <xsl:otherwise>
            <xsl:call-template name="Synthesis_OrderConfirmationsData">
              <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
              <xsl:with-param name="pBizDt" select="$pBizDt"/>
              <xsl:with-param name="pBizDC" select="$pBizDC"/>
              <xsl:with-param name="pBizBook" select="$pBizData"/>
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="contains(concat(',',$gcReportSectionKey_TRD,',',$gcReportSectionKey_SET,',',$gcReportSectionKey_UNS,','),concat(',',$pSectionKey,','))">
        <xsl:choose>
          <xsl:when test="$pBizData[name()='book']/dealt">
            <xsl:call-template name="Synthesis_TradesConfirmationsFx">
              <xsl:with-param name="pBizDt" select="$pBizDt"/>
              <xsl:with-param name="pBizCurrency" select="$pBizCurrency"/>
              <xsl:with-param name="pBizBook" select="$pBizData"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <!-- FI 20150716 [20892] Add parameter pSectionKey-->
            <xsl:call-template name="Synthesis_TradesConfirmationsData">
              <xsl:with-param name="pSectionKey" select="$pSectionKey"/>
              <xsl:with-param name="pBizDt" select="$pBizDt"/>
              <xsl:with-param name="pBizDC" select="$pBizDC"/>
              <xsl:with-param name="pBizBook" select="$pBizData"/>
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="contains(concat(',',$gcReportSectionKey_AMT,',',$gcReportSectionKey_CAS,','),concat(',',$pSectionKey,','))">
        <xsl:call-template name="Synthesis_AmendmentTransfersCascadingData">
          <xsl:with-param name="pBizDt" select="$pBizDt"/>
          <xsl:with-param name="pBizDC" select="$pBizDC"/>
          <xsl:with-param name="pBizBook" select="$pBizData"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="contains(concat(',',$gcReportSectionKey_LIQ,',',$gcReportSectionKey_DLV,','),concat(',',$pSectionKey,','))">
        <xsl:call-template name="Synthesis_LiquidationsDeliveriesData">
          <xsl:with-param name="pBizDt" select="$pBizDt"/>
          <xsl:with-param name="pBizDC" select="$pBizDC"/>
          <xsl:with-param name="pBizBook" select="$pBizData"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="contains(concat(',',$gcReportSectionKey_EXA,',',$gcReportSectionKey_ABN,','),concat(',',$pSectionKey,','))">
        <xsl:call-template name="Synthesis_OptionSettlementData">
          <xsl:with-param name="pBizDt" select="$pBizDt"/>
          <xsl:with-param name="pBizDC" select="$pBizDC"/>
          <xsl:with-param name="pBizBook" select="$pBizData"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="contains(concat(',',$gcReportSectionKey_PSS,',',$gcReportSectionKey_CLO,',',$gcReportSectionKey_CST,$gcReportSectionKey_CA,','),concat(',',$pSectionKey,','))">
        <xsl:call-template name="Synthesis_PurchaseSaleCascadingData">
          <xsl:with-param name="pBizDt" select="$pBizDt"/>
          <xsl:with-param name="pBizDC" select="$pBizDC"/>
          <xsl:with-param name="pBizBook" select="$pBizData"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="contains(concat(',',$gcReportSectionKey_POS,',',$gcReportSectionKey_STL,','),concat(',',$pSectionKey,','))">
        <xsl:choose>
          <xsl:when test="$pBizData[name()='book']/dealt">
            <xsl:call-template name="Synthesis_OpenPositionFx">
              <xsl:with-param name="pBizDt" select="$pBizDt"/>
              <xsl:with-param name="pBizCurrency" select="$pBizCurrency"/>
              <xsl:with-param name="pBizBook" select="$pBizData"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="Synthesis_OpenPositionData">
              <xsl:with-param name="pBizDt" select="$pBizDt"/>
              <xsl:with-param name="pBizDC" select="$pBizDC"/>
              <xsl:with-param name="pBizBook" select="$pBizData"/>
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="$pSectionKey=$gcReportSectionKey_JNL">
        <xsl:call-template name="Synthesis_JournalEntriesData">
          <xsl:with-param name="pBizBook" select="$pBizData"/>
        </xsl:call-template>
      </xsl:when>

      <!-- FI 20160530 [21885] Add -->
      <xsl:when test="$pSectionKey=$gcReportSectionKey_SOD">
        <xsl:call-template name="Synthesis_CollateralData">
          <xsl:with-param name="pBizBook" select="$pBizData"/>
        </xsl:call-template>
      </xsl:when>

      <!-- FI 20160613 [22256] Add -->
      <xsl:when test="$pSectionKey=$gcReportSectionKey_UOD">
        <xsl:call-template name="Synthesis_UnderlyingStockData">
          <xsl:with-param name="pBizBook" select="$pBizData"/>
        </xsl:call-template>
      </xsl:when>

      <xsl:when test="$pSectionKey=$gcReportSectionKey_ACC">
        <xsl:call-template name="Synthesis_AccountSummaryData">
          <xsl:with-param name="pBizGroup" select="$pBizData"/>
        </xsl:call-template>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- .......................................................................... -->
  <!--              Data Section                                                  -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- Synthesis_TradesConfirmationsData              -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display TradesConfirmations Content             -->
  <!-- ................................................ -->
  <xsl:template name="Synthesis_TradesConfirmationsData">
    <xsl:param name="pSectionKey"/>
    <xsl:param name="pBizDt"/>
    <xsl:param name="pBizDC"/>
    <xsl:param name="pBizBook"/>

    <!--To define in XSL of each report-->
  </xsl:template>

  <!-- ................................................ -->
  <!-- Synthesis_OrderConfirmationsData                 -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display OrderConfirmations Content              -->
  <!-- ................................................ -->
  <xsl:template name="Synthesis_OrderConfirmationsData">
    <xsl:param name="pSectionKey"/>
    <xsl:param name="pBizDt"/>
    <xsl:param name="pBizDC"/>
    <xsl:param name="pBizBook"/>

    <!--To define in XSL of each report-->
  </xsl:template>

  <!-- ................................................ -->
  <!-- Synthesis_TradesConfirmationsFx                  -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Fx TradesConfirmations Content          -->
  <!-- ................................................ -->
  <xsl:template name="Synthesis_TradesConfirmationsFx">
    <xsl:param name="pBizDt"/>
    <xsl:param name="pBizCurrency"/>
    <xsl:param name="pBizBook"/>

    <!--To define in XSL of each report-->
  </xsl:template>

  <!-- ................................................ -->
  <!-- Synthesis_AmendmentTransfersCascadingData        -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display AmendmentTransfers Content              -->
  <!-- ................................................ -->
  <xsl:template name="Synthesis_AmendmentTransfersCascadingData">
    <xsl:param name="pBizDt"/>
    <xsl:param name="pBizDC"/>
    <xsl:param name="pBizBook"/>

    <!--To define in XSL of each report-->
  </xsl:template>

  <!-- ................................................ -->
  <!-- Synthesis_LiquidationsDeliveriesData             -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Liquidations and Deliveries Content     -->
  <!-- ................................................ -->
  <xsl:template name="Synthesis_LiquidationsDeliveriesData">
    <xsl:param name="pBizDt"/>
    <xsl:param name="pBizDC"/>
    <xsl:param name="pBizBook"/>

    <!--To define in XSL of each report-->
  </xsl:template>

  <!-- ................................................ -->
  <!-- Synthesis_OptionSettlementData                 -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display EXERCISES / ASSIGNMENTS / ABANDONMENTS Content -->
  <!-- ................................................ -->
  <xsl:template name="Synthesis_OptionSettlementData">
    <xsl:param name="pBizDt"/>
    <xsl:param name="pBizDC"/>
    <xsl:param name="pBizBook"/>

    <!--To define in XSL of each report-->
  </xsl:template>

  <!-- ................................................ -->
  <!-- Synthesis_PurchaseSaleCascadingData              -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display PurchaseSale and Cascading Content      -->
  <!-- ................................................ -->
  <xsl:template name="Synthesis_PurchaseSaleCascadingData">
    <xsl:param name="pBizDt"/>
    <xsl:param name="pBizDC"/>
    <xsl:param name="pBizBook"/>

    <!--To define in XSL of each report-->
  </xsl:template>

  <!-- ................................................ -->
  <!-- Synthesis_OpenPositionData                     -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display OpenPosition Content                    -->
  <!-- ................................................ -->
  <xsl:template name="Synthesis_OpenPositionData">
    <xsl:param name="pBizDt"/>
    <xsl:param name="pBizDC"/>
    <xsl:param name="pBizBook"/>

    <!--To define in XSL of each report-->
  </xsl:template>

  <!-- ................................................ -->
  <!-- Synthesis_OpenPositionFx                       -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Fx OpenPosition Content                 -->
  <!-- ................................................ -->
  <xsl:template name="Synthesis_OpenPositionFx">
    <xsl:param name="pBizDt"/>
    <xsl:param name="pBizCurrency"/>
    <xsl:param name="pBizBook"/>

    <!--To define in XSL of each report-->
  </xsl:template>

  <!-- ................................................ -->
  <!-- Synthesis_JournalEntriesData                     -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display JournalEntries Content                  -->
  <!-- ................................................ -->
  <!-- FI 20150914 [XXXXX] Modify -->
  <!-- FI 20160623 [XXXXX] Modify Date and Value Date -->
  <xsl:template name="Synthesis_JournalEntriesData">
    <xsl:param name="pBizBook"/>

    <fo:table table-layout="fixed" width="{$gSection_width}"
              table-omit-header-at-break="false"
              keep-together.within-page="always">
      <fo:table-column column-number="01" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}"/>
      <fo:table-column column-number="02" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}"/>
      <fo:table-column column-number="03" column-width="{$gBlockSettings_Data/column[@name='TrdNum']/@column-width}"/>
      <fo:table-column column-number="04" column-width="{$gBlockSettings_Data/column[@name='Type']/@column-width}"/>
      <fo:table-column column-number="05" column-width="{$gBlockSettings_Data/column[@name='Payment']/@column-width}"/>
      <fo:table-column column-number="06" column-width="{$gColumnSpace_width}"/>
      <fo:table-column column-number="07" column-width="{$gBlockSettings_Data/column[@name='Designation']/@column-width}"/>
      <fo:table-column column-number="08"/>
      <fo:table-column column-number="09" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}"/>
      <fo:table-column column-number="10" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}"/>
      <fo:table-column column-number="11" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}"/>
      <fo:table-column column-number="12" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}"/>

      <fo:table-header>
        <!--Column resources-->
        <fo:table-row font-size="{$gData_font-size}"
                      font-weight="{$gBlockSettings_Data/title/@font-weight}"
                      text-align="{$gData_text-align}"
                      background-color="{$gBlockSettings_Data/title/@background-color}">
          <!-- Date -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <!-- Value Date -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="0.4" display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column/header[@name='ValueDate']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <!-- RefNum -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='TrdNum']/header[@name='RefNum']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <!-- Type -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"/>
          <!-- Payment -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Payment']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <!-- Space -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"/>
          <!-- Designation -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Designation']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <!-- Space -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"/>
          <!-- Amount det -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
            </fo:block>
          </fo:table-cell>
          <!-- Amount -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}"
                         number-columns-spanned="3">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
        <!--Space line-->
        <xsl:call-template name="Display_SpaceLine"/>
      </fo:table-header>
      <fo:table-body>
        <xsl:for-each select="$pBizBook/currency">
          <xsl:sort select="@ccy"/>

          <xsl:variable name="vBizCurrency" select="current()"/>
          <xsl:variable name="vCurrencyPosSyntheticList" select="($gPosSynthetics | $gStlPosSynthetics)/posSynthetic[concat(@assetCategory,'+', @idAsset)=$vBizCurrency/posSynthetic/@keyCategory]"/>
          <xsl:variable name="vCurrencyCashPaymentList" select="$gCashPayments/cashPayment[@OTCmlId=$vBizCurrency/cashPayment/@OTCmlId]"/>

          <!-- Display Details-->
          <!-- FI 20150914 [XXXXX] tri on lbl -->
          <!-- FI 20160623 [XXXXX] use of attributes recDt  -->
          <xsl:for-each select="$vBizCurrency/posSynthetic | $vBizCurrency/cashPayment">
            <xsl:sort select="current()/@bizDt" data-type="text"/>
            <xsl:sort select="current()/@eventType" data-type="text"/>
            <xsl:sort select="current()/@lbl" data-type="text"/>

            <xsl:variable name="vBizJournalEntries" select="current()"/>
            <xsl:variable name="vPosSynthetic" select="($gPosSynthetics | $gStlPosSynthetics)[@bizDt=$vBizJournalEntries/@bizDt]/
                          posSynthetic[concat(@assetCategory, '+', @idAsset)=$vBizJournalEntries/@keyCategory]"/>
            <xsl:variable name="vCashPayment" select="$gCashPayments[@bizDt=$vBizJournalEntries/@bizDt]/
                          cashPayment[$vBizJournalEntries[name()='cashPayment'] and @OTCmlId=$vBizJournalEntries/@OTCmlId]"/>

            <fo:table-row font-size="{$gData_font-size}"
                          font-weight="{$gData_font-weight}"
                          text-align="{$gData_text-align}"
                          display-align="{$gData_display-align}">

              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Format">
                    <xsl:with-param name="pData" select="$vBizJournalEntries/@recDt"/>
                    <xsl:with-param name="pDataType" select="'Date'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <!-- Date Value-->
              <fo:table-cell padding="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayData_Format">
                    <xsl:with-param name="pData" select="$vBizJournalEntries/@bizDt"/>
                    <xsl:with-param name="pDataType" select="'Date'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <!-- RefNum -->
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:choose>
                    <xsl:when test="$vPosSynthetic">
                      <xsl:value-of select="'n/a'"/>
                    </xsl:when>
                    <xsl:when test="$vCashPayment">
                      <xsl:call-template name="DisplayData_Format">
                        <xsl:with-param name="pData" select="$vCashPayment/@tradeId"/>
                        <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='TrdNum']"/>
                      </xsl:call-template>
                    </xsl:when>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
              <!-- Type -->
              <fo:table-cell>
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                </fo:block>
              </fo:table-cell>
              <!-- Payment -->
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}"
                             text-align="{$gBlockSettings_Data/column[@name='Payment']/data/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:choose>
                    <xsl:when test="$vBizJournalEntries/@eventType='FDA'">
                      <xsl:call-template name="UKDisplay_JNPaymentDefault">
                        <xsl:with-param name="pType" select="'Funding'"/>
                        <xsl:with-param name="pDataEfsml" select="$vPosSynthetic"/>
                        <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='Payment']"/>
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:when test="$vBizJournalEntries/@eventType='BWA'">
                      <xsl:call-template name="UKDisplay_JNPaymentDefault">
                        <xsl:with-param name="pType" select="'Borrowing'"/>
                        <xsl:with-param name="pDataEfsml" select="$vPosSynthetic"/>
                        <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='Payment']"/>
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:when test="$vBizJournalEntries/@eventType='CUS'">
                      <xsl:call-template name="UKDisplay_JNPaymentDefault">
                        <xsl:with-param name ="pType" select ="'Safekeeping'"/>
                        <xsl:with-param name="pDataEfsml" select="$vPosSynthetic"/>
                        <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='Payment']"/>
                      </xsl:call-template>
                    </xsl:when>

                    <xsl:when test="$vCashPayment">
                      <xsl:call-template name="UKDisplay_CashPaymentType">
                        <xsl:with-param name="pCashPayment" select="$vCashPayment"/>
                        <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='Payment']"/>
                      </xsl:call-template>
                    </xsl:when>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
              <!-- Space -->
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <!-- Designation -->
              <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}"
                             text-align="{$gBlockSettings_Data/column[@name='Designation']/data/@text-align}">
                <fo:block>
                  <!-- FI 20150914 [XXXXX] Lecture de l'attribut lbl et appel à DisplayData_Format -->
                  <!-- FI 20151016 [21458] Apple à SplitCrLf -->
                  <xsl:call-template name ="SplitCrLf">
                    <xsl:with-param name = "pText" select ="$vBizJournalEntries/@lbl"></xsl:with-param>
                    <xsl:with-param name = "pDataLength" select="$gBlockSettings_Data/column[@name='Designation']/data/@length"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <!-- Space -->
              <fo:table-cell>
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                </fo:block>
              </fo:table-cell>
              <!-- AmountDet (eventType) -->
              <fo:table-cell font-size="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-size}"
                             font-weight="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-weight}"
                             padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayType_Short">
                    <xsl:with-param name="pType" select="$vBizJournalEntries/@eventType"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <!-- AmountDet -->
              <xsl:choose>
                <xsl:when test="$vBizJournalEntries/@eventType='FDA'">
                  <xsl:call-template name="UKDisplay_Amount">
                    <xsl:with-param name="pAmount" select="$vPosSynthetic/fda"/>
                    <xsl:with-param name="pCcy" select="$vBizCurrency/@ccy"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:when test="$vBizJournalEntries/@eventType='BWA'">
                  <xsl:call-template name="UKDisplay_Amount">
                    <xsl:with-param name="pAmount" select="$vPosSynthetic/bwa"/>
                    <xsl:with-param name="pCcy" select="$vBizCurrency/@ccy"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:when test="$vBizJournalEntries/@eventType='CUS'">
                  <xsl:call-template name="UKDisplay_Amount">
                    <xsl:with-param name="pAmount" select="$vPosSynthetic/skp"/>
                    <xsl:with-param name="pCcy" select="$vBizCurrency/@ccy"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:when test="$vCashPayment">
                  <xsl:call-template name="UKDisplay_Amount">
                    <xsl:with-param name="pAmount" select="$vCashPayment"/>
                    <xsl:with-param name="pCcy" select="$vBizCurrency/@ccy"/>
                  </xsl:call-template>
                </xsl:when>
              </xsl:choose>
            </fo:table-row>
          </xsl:for-each>

          <!--Display Subtotal-->
          <xsl:variable name="vTotal_Amount">
            <xsl:call-template name="GetAmount-amt">
              <xsl:with-param name="pAmount" select="$vCurrencyPosSyntheticList/fda | 
                              $vCurrencyPosSyntheticList/bwa | 
                              $vCurrencyPosSyntheticList/skp | 
                              $vCurrencyCashPaymentList"/>
              <xsl:with-param name="pCcy" select="$vBizCurrency/@ccy"/>
            </xsl:call-template>
          </xsl:variable>

          <!--Total Row-->
          <fo:table-row font-size="{$gData_font-size}"
                        font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                        text-align="{$gData_text-align}"
                        display-align="{$gBlockSettings_Data/subtotal/@display-align}"
                        keep-with-previous="always">

            <fo:table-cell text-align="{$gBlockSettings_Data/subtotal/column[@name='Total']/@text-align}"
                           padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='Total']/data/@resource"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell number-columns-spanned="8">
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
            <xsl:call-template name="UKDisplay_SubTotal_Amount">
              <xsl:with-param name="pAmount" select="$vTotal_Amount"/>
              <xsl:with-param name="pCcy" select="$vBizCurrency/@ccy"/>
              <xsl:with-param name="pAmountPattern" select="$vBizCurrency/pattern/@ccy" />
            </xsl:call-template>
          </fo:table-row>
          <!--Display underline-->
          <xsl:call-template name="Display_SubTotalSpace">
            <xsl:with-param name="pNumber-columns" select="number('12')"/>
            <xsl:with-param name="pPosition" select="position()"/>
          </xsl:call-template>
          <!--Space line-->
          <xsl:call-template name="Display_SpaceLine"/>
        </xsl:for-each>
      </fo:table-body>
    </fo:table>
  </xsl:template>

  <!-- ................................................ -->
  <!-- Synthesis_CollateralData                          -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display collteral Content                       -->
  <!-- ................................................ -->
  <!-- FI 20160530 [21885] Add -->
  <xsl:template name="Synthesis_CollateralData">
    <xsl:param name="pBizBook"/>

    <xsl:variable name="vCBMethod" select ="$gCBTrade/cashBalanceReport/settings/cashBalanceMethod"/>

    <fo:table table-layout="fixed" width="{$gSection_width}"
              table-omit-header-at-break="false"
              keep-together.within-page="always">

      <fo:table-column column-number="01" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}"/>
      <fo:table-column column-number="02" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}"/>
      <fo:table-column column-number="03" column-width="{$gBlockSettings_Data/column[@name='TrdNum']/@column-width}"/>
      <fo:table-column column-number="04" column-width="{$gBlockSettings_Data/column[@name='Type']/@column-width}"/>
      <fo:table-column column-number="05" column-width="{$gBlockSettings_Data/column[@name='CssMIC']/@column-width}"/>
      <fo:table-column column-number="06" column-width="{$gBlockSettings_Data/column[@name='Haircut']/@column-width}"/>
      <fo:table-column column-number="07" column-width="{$gColumnSpace_width}"/>
      <fo:table-column column-number="08" column-width="{$gBlockSettings_Data/column[@name='Designation2']/@column-width}"/>
      <fo:table-column column-number="09"/>
      <fo:table-column column-number="10" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}"/>
      <fo:table-column column-number="11" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}"/>
      <fo:table-column column-number="12" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}"/>
      <fo:table-column column-number="13" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}"/>
      <fo:table-column column-number="14" column-width="{$gColumnSpace_width}"/>
      <fo:table-column column-number="15" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}"/>
      <fo:table-column column-number="16" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}"/>
      <fo:table-column column-number="17" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}"/>
      <fo:table-column column-number="18" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}"/>

      <fo:table-header>
        <fo:table-row font-size="{$gData_font-size}"
                      font-weight="{$gBlockSettings_Data/title/@font-weight}"
                      text-align="{$gData_text-align}"
                      background-color="{$gBlockSettings_Data/title/@background-color}">
          <!--Date -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <!--Date Valeur -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}" display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column/header[@name='ValueDate']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <!--RefNo (2 columns) -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}"
                         number-columns-spanned="2">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='TrdNum']/header[@name='RefNum']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <!-- haircut (2 columns)-->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}"
                         number-columns-spanned="2">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Haircut']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <!-- Space column -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"/>
          <!-- Designation -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Designation2']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <!-- Space column -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"/>
          <!-- Qty Column -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         number-columns-spanned="4">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <!-- Space -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <fo:block>
            </fo:block>
          </fo:table-cell>
          <!-- Amount Column (4 columns) -->
          <xsl:variable name ="vColumnName">
            <xsl:choose>
              <xsl:when test ="$vCBMethod = 'CSBUK'">
                <xsl:value-of select ="'NetMarketValue'"/>
              </xsl:when>
              <xsl:when test ="$vCBMethod = 'CSBDEFAULT'">
                <xsl:value-of select ="'GrossMarketValue'"/>
              </xsl:when>
            </xsl:choose>
          </xsl:variable>

          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}"
                         number-columns-spanned="4">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Amount']/header[@name=$vColumnName]/@resource"/>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
        <!--Space line-->
        <xsl:call-template name="Display_SpaceLine"/>
      </fo:table-header>
      <fo:table-body>

        <xsl:for-each select="$pBizBook/currency">
          <xsl:sort select="@ccy"/>
          <xsl:variable name="vBizCurrency" select="current()"/>
          <xsl:variable name="vCurrencyCollateralList" select="$gCollaterals/collateral[@OTCmlId=$vBizCurrency/collateral/@OTCmlId]"/>

          <!-- Display Details-->
          <xsl:for-each select="$vBizCurrency/collateral/haircut">
            <xsl:variable name ="vBizHaircut" select="current()"/>
            <xsl:variable name ="vBizCollateral" select="current()/parent::node()"/>
            <xsl:variable name ="vPrevColId">
              <xsl:value-of select="current()/preceding-sibling::node()[1]/parent::node()/@colId"/>
            </xsl:variable>
            <xsl:variable name ="vCurrentColId" >
              <xsl:value-of select="$vBizCollateral/@colId"/>
            </xsl:variable>
            <xsl:variable name= "vIsNewCollateral">
              <xsl:choose>
                <xsl:when test ="$vPrevColId=$vCurrentColId ">
                  <xsl:value-of select ="false()"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select ="true()"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:variable name= "vIsDisplayLabelOthers">
              <xsl:choose>
                <xsl:when test ="string-length(current()/@cssCode)=0 and (current()/preceding-sibling::node()|current()/following-sibling::node())">
                  <xsl:value-of select ="true()"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select ="false()"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:variable name ="vCssCode">
              <xsl:choose>
                <xsl:when test ="$vIsDisplayLabelOthers='true'">
                  <xsl:value-of select="$gBlockSettings_Data/column[@name='CssMIC']/data[@name='Others']/@resource"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select ="$vBizHaircut/@cssCode"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <fo:table-row font-size="{$gData_font-size}"
                          font-weight="{$gData_font-weight}"
                          text-align="{$gData_text-align}"
                          display-align="{$gData_display-align}">

              <!-- Date -->
              <fo:table-cell padding="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:if test ="$vIsNewCollateral='true'">
                    <xsl:call-template name="DisplayData_Format">
                      <xsl:with-param name="pData" select="$vBizCollateral/@bizDt"/>
                      <xsl:with-param name="pDataType" select="'Date'"/>
                    </xsl:call-template>
                  </xsl:if>
                </fo:block>
              </fo:table-cell>
              <!-- Value Date -->
              <fo:table-cell padding="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:if test ="$vIsNewCollateral='true'">
                    <xsl:call-template name="DisplayData_Format">
                      <xsl:with-param name="pData" select="$vBizCollateral/@bizDt"/>
                      <xsl:with-param name="pDataType" select="'Date'"/>
                    </xsl:call-template>
                  </xsl:if>
                </fo:block>
              </fo:table-cell>
              <!-- Ref Num -->
              <fo:table-cell padding="{$gData_padding}"
                              number-columns-spanned="2">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:if test ="$vIsNewCollateral='true'">
                    <xsl:call-template name="DisplayData_Format">
                      <xsl:with-param name="pData" select="$vBizCollateral/@colId"/>
                      <!--<xsl:with-param name="pDataLength" select="$gBlockSettings_Data/column[@name='TrdNum']/data/@length"/>-->
                      <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='TrdNum']"/>
                    </xsl:call-template>
                  </xsl:if>
                </fo:block>
              </fo:table-cell>
              <!-- CssMIC -->
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gBlockSettings_Data/column[@name='CssMIC']/data/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name ="DisplayData_Format">
                    <xsl:with-param name ="pData" select="$vCssCode"/>
                    <xsl:with-param name ="pDataLength" select="$gBlockSettings_Data/column[@name='CssMIC']/data/@length"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <!-- haircut -->
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gBlockSettings_Data/column[@name='Haircut']/data/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name ="DisplayData_Format">
                    <xsl:with-param name = "pData" select ="concat(100 * $vBizHaircut/@value,' %')"/>
                    <xsl:with-param name = "pDataLength" select="$gBlockSettings_Data/column[@name='Haircut']/data/@length"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <!-- Space -->
              <fo:table-cell>
                <fo:block/>
              </fo:table-cell>
              <!-- Designation -->
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gBlockSettings_Data/column[@name='Designation2']/data/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:if test ="$vIsNewCollateral='true'">
                    <xsl:call-template name ="SplitCrLf">
                      <xsl:with-param name = "pText" select ="$vBizCollateral/@lbl"/>
                      <xsl:with-param name = "pDataLength" select="$gBlockSettings_Data/column[@name='Designation2']/data/@length"/>
                    </xsl:call-template>
                  </xsl:if>
                </fo:block>
              </fo:table-cell>
              <!-- Space -->
              <fo:table-cell>
                <fo:block/>
              </fo:table-cell>
              <!-- qty -->
              <fo:table-cell text-align="{$gData_Number_text-align}"
                             padding="{$gData_padding}" number-columns-spanned="4">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:if test ="$vIsNewCollateral='true' and $vBizCollateral/@fmtQty>0">
                    <!--<xsl:call-template name ="DisplayData_Format">
                      <xsl:with-param name ="pData" select ="$vBizCollateral/@qty"/>
                      <xsl:with-param name ="pDataType" select="'Integer'"/>
                    </xsl:call-template>-->
                    <xsl:call-template name="DisplayFmtNumber">
                      <xsl:with-param name="pFmtNumber" select="$vBizCollateral/@fmtQty"/>
                      <xsl:with-param name="pPattern" select="$vBizCurrency/pattern/@qty" />
                    </xsl:call-template>
                  </xsl:if>
                </fo:block>
              </fo:table-cell>
              <!-- Space -->
              <fo:table-cell>
                <fo:block/>
              </fo:table-cell>
              <fo:table-cell font-size="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-size}"
                             font-weight="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-weight}"
                             padding="{$gData_padding}">
                <fo:block>
                </fo:block>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test ="$vIsNewCollateral='true'">
                  <xsl:call-template name="UKDisplay_Amount">
                    <xsl:with-param name="pAmount" select="$vBizCollateral"/>
                    <xsl:with-param name="pCcy" select="$vBizCurrency/@ccy"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell number-columns-spanned="3">
                    <fo:block>
                      <xsl:call-template name="Debug_border-green"/>
                    </fo:block>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
            </fo:table-row>
          </xsl:for-each>


          <!--Display Subtotal-->
          <xsl:variable name="vTotal_Amount">
            <xsl:call-template name="GetAmount-amt">
              <xsl:with-param name="pAmount" select="$vCurrencyCollateralList"/>
              <xsl:with-param name="pCcy" select="@ccy"/>
            </xsl:call-template>
          </xsl:variable>


          <fo:table-row font-size="{$gData_font-size}"
                        font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                        text-align="{$gData_text-align}"
                        display-align="{$gBlockSettings_Data/subtotal/@display-align}"
                        keep-with-previous="always">

            <fo:table-cell text-align="{$gBlockSettings_Data/subtotal/column[@name='Total']/@text-align}"
                           padding="{$gBlockSettings_Data/subtotal/@padding}"
                           padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='Total']/data/@resource"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell number-columns-spanned="13">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell/>
            <xsl:call-template name="UKDisplay_SubTotal_Amount">
              <xsl:with-param name="pAmount" select="$vTotal_Amount"/>
              <xsl:with-param name="pCcy" select="$vBizCurrency/@ccy"/>
              <xsl:with-param name="pAmountPattern" select="$vBizCurrency/pattern/@ccy" />
            </xsl:call-template>
          </fo:table-row>
          <!--Display underline-->
          <xsl:call-template name="Display_SubTotalSpace">
            <xsl:with-param name="pNumber-columns" select="number('18')"/>
            <xsl:with-param name="pPosition" select="position()"/>
          </xsl:call-template>
          <!--Space line-->
          <!--<xsl:call-template name="Display_SpaceLine"/>-->

        </xsl:for-each>
      </fo:table-body>
    </fo:table>
  </xsl:template>

  <!-- ................................................ -->
  <!-- Synthesis_UnderlyingStockData                    -->
  <!-- ................................................ -->
  <!-- FI 20160613 [22256] Add -->
  <xsl:template name="Synthesis_UnderlyingStockData">
    <xsl:param name="pBizBook"/>
    <fo:table table-layout="fixed" width="{$gSection_width}"
              table-omit-header-at-break="false"
              keep-together.within-page="always">

      <fo:table-column column-number="01" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}"/>
      <fo:table-column column-number="02" column-width="{$gBlockSettings_Data/column[@name='Date']/@column-width}"/>
      <fo:table-column column-number="03" column-width="{$gBlockSettings_Data/column[@name='TrdNum']/@column-width}"/>
      <fo:table-column column-number="04" column-width="{$gBlockSettings_Data/column[@name='Type']/@column-width}"/>
      <fo:table-column column-number="05" column-width="{$gBlockSettings_Data/column[@name='StockCover']/@column-width}"/>
      <fo:table-column column-number="06" column-width="{$gColumnSpace_width}"/>
      <fo:table-column column-number="07" column-width="{$gBlockSettings_Data/column[@name='Designation2']/@column-width}"/>
      <fo:table-column column-number="08"/>
      <fo:table-column column-number="9" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}"/>
      <fo:table-column column-number="10" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}"/>
      <fo:table-column column-number="11" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}"/>
      <fo:table-column column-number="12" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}"/>
      <fo:table-column column-number="13" column-width="{$gColumnSpace_width}"/>
      <fo:table-column column-number="14" column-width="{$gBlockSettings_Data/column[@name='AmountDet']/@column-width}"/>
      <fo:table-column column-number="15" column-width="{$gBlockSettings_Data/column[@name='Ccy']/@column-width}"/>
      <fo:table-column column-number="16" column-width="{$gBlockSettings_Data/column[@name='Amount']/@column-width}"/>
      <fo:table-column column-number="17" column-width="{$gBlockSettings_Data/column[@name='DrCr']/@column-width}"/>

      <fo:table-header>
        <fo:table-row font-size="{$gData_font-size}"
                      font-weight="{$gBlockSettings_Data/title/@font-weight}"
                      text-align="{$gData_text-align}"
                      background-color="{$gBlockSettings_Data/title/@background-color}">
          <!--Date -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Date']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <!--Date Valeur -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}" display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column/header[@name='ValueDate']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <!--RefNo (2 columns) -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}"
                         number-columns-spanned="2">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='TrdNum']/header[@name='RefNum']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <!-- StockCover -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='StockCover']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <!-- Space column -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"/>
          <!-- Designation -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Designation2']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <!-- Space column -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"/>
          <!-- Qty Available Column -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         number-columns-spanned="4">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='QuantityAvailable']/@resource"/>
            </fo:block>
          </fo:table-cell>
          <!-- Space -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none">
            <fo:block>
            </fo:block>
          </fo:table-cell>
          <!-- Qty used Column -->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none"
                         padding="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}"
                         number-columns-spanned="4">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='QuantityUsed']/@resource"/>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
        <!--Space line-->
        <xsl:call-template name="Display_SpaceLine"/>
      </fo:table-header>
      <fo:table-body>
        <xsl:for-each select="$pBizBook/underlyingStock">
          <xsl:variable name ="vBizUnderlyingStock" select="current()"/>
          <!-- Display Details-->
          <xsl:for-each select="$vBizUnderlyingStock/child::node()[name()='opt' or name()='fut']">
            <xsl:variable name ="vBizCategory" select="current()"/>
            <xsl:variable name ="vLblBizCategory">
              <xsl:choose>
                <xsl:when test ="name($vBizCategory)='opt'">
                  <xsl:value-of  select="'OPT'"/>
                </xsl:when>
                <xsl:when test ="name($vBizCategory)='fut'">
                  <xsl:value-of  select="'FUT'"/>
                </xsl:when>
              </xsl:choose>
            </xsl:variable>
            <xsl:variable name ="vPrevUnsId">
              <xsl:value-of select="current()/preceding-sibling::node()[1]/parent::node()/@unsId"/>
            </xsl:variable>
            <xsl:variable name ="vCurrentUnsId" >
              <xsl:value-of select="$vBizUnderlyingStock/@unsId"/>
            </xsl:variable>
            <xsl:variable name= "vIsNewUnderlyingStock">
              <xsl:choose>
                <xsl:when test ="$vPrevUnsId=$vCurrentUnsId ">
                  <xsl:value-of select ="false()"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select ="true()"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <fo:table-row font-size="{$gData_font-size}"
                          font-weight="{$gData_font-weight}"
                          text-align="{$gData_text-align}"
                          display-align="{$gData_display-align}">

              <!-- Date -->
              <fo:table-cell padding="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:if test ="$vIsNewUnderlyingStock='true'">
                    <xsl:call-template name="DisplayData_Format">
                      <xsl:with-param name="pData" select="$vBizUnderlyingStock/@bizDt"/>
                      <xsl:with-param name="pDataType" select="'Date'"/>
                    </xsl:call-template>
                  </xsl:if>
                </fo:block>
              </fo:table-cell>
              <!-- Value Date -->
              <fo:table-cell padding="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:if test ="$vIsNewUnderlyingStock='true'">
                    <xsl:call-template name="DisplayData_Format">
                      <xsl:with-param name="pData" select="$vBizUnderlyingStock/@bizDt"/>
                      <xsl:with-param name="pDataType" select="'Date'"/>
                    </xsl:call-template>
                  </xsl:if>
                </fo:block>
              </fo:table-cell>
              <!-- Ref Num -->
              <fo:table-cell padding="{$gData_padding}"
                              number-columns-spanned="2">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:if test ="$vIsNewUnderlyingStock='true'">
                    <xsl:call-template name="DisplayData_Format">
                      <xsl:with-param name="pData" select="$vBizUnderlyingStock/@unsId"/>
                      <!--<xsl:with-param name="pDataLength" select="$gBlockSettings_Data/column[@name='TrdNum']/data/@length"/>-->
                      <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='TrdNum']"/>
                    </xsl:call-template>
                  </xsl:if>
                </fo:block>
              </fo:table-cell>
              <!-- StockCover -->
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gBlockSettings_Data/column[@name='StockCover']/data/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:if test ="$vIsNewUnderlyingStock='true'">
                    <xsl:variable name ="vlblStockCover">
                      <xsl:value-of select ="$gBlockSettings_Data/column[@name='StockCover']/data[@name=$vBizUnderlyingStock/@stockCover]/@resource"/>
                    </xsl:variable>
                    <xsl:call-template name ="DisplayData_Format">
                      <xsl:with-param name = "pData" select ="$vlblStockCover"/>
                      <xsl:with-param name = "pDataLength" select="$gBlockSettings_Data/column[@name='StockCover']/data/@length"/>
                    </xsl:call-template>
                  </xsl:if>
                </fo:block>
              </fo:table-cell>
              <!-- Space -->
              <fo:table-cell>
                <fo:block/>
              </fo:table-cell>
              <!-- Designation -->
              <fo:table-cell padding="{$gData_padding}"
                             text-align="{$gBlockSettings_Data/column[@name='Designation2']/data/@text-align}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:if test ="$vIsNewUnderlyingStock='true'">
                    <xsl:call-template name ="SplitCrLf">
                      <xsl:with-param name = "pText" select ="$vBizUnderlyingStock/@lbl"/>
                      <xsl:with-param name = "pDataLength" select="$gBlockSettings_Data/column[@name='Designation2']/data/@length"/>
                    </xsl:call-template>
                  </xsl:if>
                </fo:block>
              </fo:table-cell>
              <!-- Space -->
              <fo:table-cell>
                <fo:block/>
              </fo:table-cell>
              <!-- qty -->
              <fo:table-cell text-align="{$gData_Number_text-align}"
                             padding="{$gData_padding}" number-columns-spanned="4">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:if test ="$vIsNewUnderlyingStock='true'">
                    <xsl:call-template name ="DisplayData_Format">
                      <xsl:with-param name ="pData" select ="$vBizUnderlyingStock/@qtyAvailable"/>
                      <xsl:with-param name ="pDataType" select="'Integer'"/>
                    </xsl:call-template>
                  </xsl:if>
                </fo:block>
              </fo:table-cell>
              <!-- Space -->
              <fo:table-cell>
                <fo:block/>
              </fo:table-cell>
              <fo:table-cell font-size="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-size}"
                             font-weight="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-weight}"
                             padding="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:value-of select ="concat('(', $vLblBizCategory,')')"/>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell text-align="{$gData_Number_text-align}" number-columns-spanned="3">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name ="DisplayData_Format">
                    <xsl:with-param name ="pData" select ="$vBizCategory/@qtyUsed"/>
                    <xsl:with-param name ="pDataType" select="'Integer'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
          </xsl:for-each>
        </xsl:for-each>
      </fo:table-body>
    </fo:table>
  </xsl:template>

  <!-- ................................................ -->
  <!-- Synthesis_AccountSummaryData                     -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Account Summary Content                 -->
  <!-- ................................................ -->
  <!-- FI 20150921 [21311] Modify -->
  <xsl:template name="Synthesis_AccountSummaryData">
    <!-- groupe (contient n devises + éventuellement l'exchange devise) -->
    <xsl:param name="pBizGroup"/>

    <xsl:variable name="vCcyCount" select="count($pBizGroup/currency)"/>

    <fo:table table-layout="fixed" width="{$gSection_width}"
              table-omit-header-at-break="false"
              keep-together.within-page="always">

      <fo:table-column column-number="01" column-width="{$gBlockSettings_Summary/column[@name='Designation1']/@column-width}"/>
      <fo:table-column column-number="02" column-width="{$gBlockSettings_Summary/column[@name='Designation2']/@column-width}"/>
      <fo:table-column column-number="03" column-width="{$gBlockSettings_Summary/column[@name='Designation3']/@column-width}"/>
      <fo:table-column column-number="04" column-width="{$gBlockSettings_Summary/column[@name='Designation4']/@column-width}"/>
      <xsl:for-each select="$pBizGroup/currency">
        <fo:table-column column-number="{((position() - 1 ) * 2) + 5}" column-width="{$gBlockSettings_Summary/column[@name='Currency']/@column-width}"/>
        <fo:table-column column-number="{((position() - 1 ) * 2) + 6}" column-width="{$gBlockSettings_Summary/column[@name='DrCr']/@column-width}"/>
      </xsl:for-each>
      <xsl:choose>
        <xsl:when test="$pBizGroup/exchangeCurrency">
          <fo:table-column column-number="{(($vCcyCount - 1 ) * 2) + 7}" column-width="{$gBlockSettings_Summary/column[@name='BaseCurrency']/@column-width}"/>
          <fo:table-column column-number="{(($vCcyCount - 1 ) * 2) + 8}" column-width="{$gBlockSettings_Summary/column[@name='DrCr']/@column-width}"/>
          <xsl:if test="number ($pBizGroup/@empty-currency) > 0">
            <fo:table-column column-number="{(($vCcyCount - 1 ) * 2) + 9}"/>
          </xsl:if>
        </xsl:when>
        <xsl:otherwise>
          <xsl:if test="number ($pBizGroup/@empty-currency) > 0">
            <fo:table-column column-number="{(($vCcyCount - 1 ) * 2) + 7}"/>
          </xsl:if>
        </xsl:otherwise>
      </xsl:choose>
      <fo:table-header>
        <!--Row1: resources-->
        <fo:table-row font-size="{$gData_font-size}"
                      font-weight="{$gBlockSettings_Data/title/@font-weight}"
                      text-align="{$gData_text-align}">

          <!--Designation-->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none" border-bottom-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}"
                         background-color="{$gBlockSettings_Data/title/@background-color}"
                         number-columns-spanned="4">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Summary/column[@name='Designation2']/header/@resource"/>
            </fo:block>
          </fo:table-cell>
          <!--Currencies-->
          <xsl:for-each select="$pBizGroup/currency">
            <xsl:sort select="current()/@ccy" data-type="text" order="ascending"/>

            <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none" border-bottom-style="none"
                           padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
                           display-align="{$gData_display-align}"
                           background-color="{$gBlockSettings_Data/title/@background-color}"
                           number-columns-spanned="2">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="current()/@ccy"/>
              </fo:block>
            </fo:table-cell>
          </xsl:for-each>
          <!--Exchange Currency-->
          <xsl:if test="$pBizGroup/exchangeCurrency">
            <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none" border-bottom-style="none"
                           padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
                           display-align="{$gData_display-align}"
                           background-color="{$gBlockSettings_Data/title/@background-color}"
                           number-columns-spanned="2">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="concat($gBlockSettings_Summary/column[@name='BaseCurrency']/header/@resource,$gcSpace,'-',$gcSpace,$pBizGroup/exchangeCurrency/@ccy)"/>
              </fo:block>
            </fo:table-cell>
          </xsl:if>
          <!--Empty-->
          <xsl:if test="number($pBizGroup/@empty-currency) > 0">
            <fo:table-cell background-color="{$gBlockSettings_Data/title/@background-color}"
                           border="{$gTable_border}" border-left-style="none" border-right-style="none" border-bottom-style="none">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
              </fo:block>
            </fo:table-cell>
          </xsl:if>
        </fo:table-row>
        <!--Row2: currencies description-->
        <fo:table-row font-size="{$gBlockSettings_Summary/column[@name='Currency']/data[@name='det']/@font-size}"
                      font-weight="{$gBlockSettings_Summary/column[@name='Currency']/data[@name='det']/@font-weight}"
                      text-align="{$gData_text-align}">

          <!--Designation-->
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none" border-top-style="none"
                         padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
                         display-align="{$gData_display-align}"
                         background-color="{$gBlockSettings_Data/title/@background-color}"
                         number-columns-spanned="4">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
            </fo:block>
          </fo:table-cell>
          <!--Currencies-->
          <xsl:for-each select="$pBizGroup/currency">
            <xsl:sort select="current()/@ccy" data-type="text" order="ascending"/>

            <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none" border-top-style="none"
                           padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
                           display-align="{$gData_display-align}"
                           background-color="{$gBlockSettings_Data/title/@background-color}"
                           number-columns-spanned="2">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:choose>
                  <xsl:when test="string-length($gRepository/currency[identifier=current()/@ccy]/description) > 0">
                    <xsl:value-of select="$gRepository/currency[identifier=current()/@ccy]/description"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="$gRepository/currency[identifier=current()/@ccy]/displayname"/>
                  </xsl:otherwise>
                </xsl:choose>
              </fo:block>
            </fo:table-cell>
          </xsl:for-each>
          <!--Exchange Currency-->
          <xsl:if test="$pBizGroup/exchangeCurrency">
            <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none" border-top-style="none"
                           padding-top="{$gBlockSettings_Data/title/@padding}" padding-bottom="{$gBlockSettings_Data/title/@padding}"
                           display-align="{$gData_display-align}"
                           background-color="{$gBlockSettings_Data/title/@background-color}"
                           number-columns-spanned="2">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:choose>
                  <xsl:when test="string-length($gRepository/currency[identifier=$pBizGroup/exchangeCurrency/@ccy]/description) > 0">
                    <xsl:value-of select="$gRepository/currency[identifier=$pBizGroup/exchangeCurrency/@ccy]/description"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="$gRepository/currency[identifier=$pBizGroup/exchangeCurrency/@ccy]/displayname"/>
                  </xsl:otherwise>
                </xsl:choose>
              </fo:block>
            </fo:table-cell>
          </xsl:if>
          <!--Empty-->
          <xsl:if test="number($pBizGroup/@empty-currency) > 0">
            <fo:table-cell background-color="{$gBlockSettings_Data/title/@background-color}"
                           border="{$gTable_border}" border-left-style="none" border-right-style="none" border-top-style="none">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
              </fo:block>
            </fo:table-cell>
          </xsl:if>
        </fo:table-row>
        <!--Space line-->
        <xsl:call-template name="Display_SpaceLine"/>
      </fo:table-header>
      <fo:table-body>
        <xsl:call-template name="Synthesis_AccountSummaryBody">
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        </xsl:call-template>
      </fo:table-body>
    </fo:table>
  </xsl:template>

  <!-- ................. Tools: ACCOUNT SUMMARY ................................. -->

  <!-- .......................................................................... -->
  <!--              Section: ACCOUNT SUMMARY                                      -->
  <!-- .......................................................................... -->


  <!-- ................................................ -->
  <!-- Synthesis_AccountSummaryBody                      -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Account Summary Content                 -->
  <!-- ................................................ -->
  <xsl:template name="Synthesis_AccountSummaryBody">
    <xsl:param name="pBizGroup"/>


    <!--To define in XSL of each report-->
  </xsl:template>

  <!-- ................................................ -->
  <!-- Synthesis_SummaryFeeDetail                       -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Summary Fee details                     -->
  <!-- ................................................ -->
  <!-- FI 20150921 [21311] Modify -->
  <xsl:template name="Synthesis_SummaryFeeDetail">
    <xsl:param name="pBizGroup"/>

    <!--FeeDetail : title-->
    <xsl:call-template name="Synthesis_SummaryRowTitle">
      <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
      <xsl:with-param name="pTitleName" select="'Fee'"/>
    </xsl:call-template>

    <!--Fee Detail-->
    <xsl:variable name="vBizFee" select="$pBizGroup/fee | $pBizGroup/safekeeping"/>
    <xsl:variable name="vBizExchangeFee" select="$pBizGroup/exchangeCurrency/fee | $pBizGroup/exchangeCurrency/safekeeping"/>

    <xsl:for-each select="$vBizFee[$vBizExchangeFee = false()] | $vBizExchangeFee">
      <xsl:sort select="current()/@paymentType"/>

      <xsl:variable name="vCurrentFee" select="current()"/>
      <xsl:variable name="vFee" select="$vBizFee[@paymentType=$vCurrentFee/@paymentType]"/>
      <xsl:variable name="vExchangeFee" select="$vBizExchangeFee[@paymentType=$vCurrentFee/@paymentType]"/>

      <xsl:variable name="vDisplayPaymentType_Short">
        <xsl:call-template name="DisplayType_Short">
          <xsl:with-param name="pType" select="$vCurrentFee/@paymentType"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:variable name="vDisplayPaymentType">
        <xsl:call-template name="DisplayPaymentType">
          <xsl:with-param name="pPaymentType" select="$vCurrentFee/@paymentType"/>
        </xsl:call-template>
      </xsl:variable>
      <!-- FI 20150921 [21311] use variable labelFee-->
      <xsl:variable name ="labelFee">
        <fo:inline font-size="{$gBlockSettings_Summary/column[@name='Designation2']/data[@name='Det']/@font-size}"
                 font-weight="{$gBlockSettings_Summary/column[@name='Designation2']/data[@name='Det']/@font-weight}">
          <xsl:value-of select ="$vDisplayPaymentType_Short"/>
        </fo:inline>
        <xsl:value-of select ="concat($gcSpace,$vDisplayPaymentType)"/>
      </xsl:variable>

      <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
      <xsl:call-template name="Synthesis_SummaryRowAmount">
        <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
        <xsl:with-param name="pAmountName" select="$labelFee"/>
        <xsl:with-param name="pIsAmountNameLabel" select="true()"/>
        <xsl:with-param name="pAmount" select="$vFee/amount"/>
        <xsl:with-param name="pExchangeAmount" select="$vExchangeFee/amount"/>
        <xsl:with-param name="pIsAmountMandatory" select="true()"/>
        <xsl:with-param name="pIsDetail" select="true()"/>
      </xsl:call-template>


      <xsl:for-each select="$vFee/tax[$vBizExchangeFee = false()] | $vExchangeFee/tax">
        <xsl:sort select="current()/@taxType"/>
        <xsl:sort select="current()/@taxId"/>
        <xsl:sort select="current()/@rate"/>

        <xsl:variable name="vCurrentTax" select="current()"/>
        <xsl:variable name="vTaxAmount" select="$vFee/tax[@taxType=$vCurrentTax/@taxType and @taxId=$vCurrentTax/@taxId and @rate=$vCurrentTax/@rate]/amount"/>
        <xsl:variable name="vExchangeTaxAmount" select="$vExchangeFee/tax[@taxType=$vCurrentTax/@taxType and @taxId=$vCurrentTax/@taxId and @rate=$vCurrentTax/@rate]/amount"/>

        <xsl:variable name="vDisplayTaxType_Short">
          <xsl:call-template name="DisplayType_Short">
            <xsl:with-param name="pType" select="$vCurrentTax/@taxType"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:variable name="vDisplayRate">
          <xsl:call-template name="DisplayPercentage">
            <xsl:with-param name="pFactor" select="$vCurrentTax/@rate"/>
            <xsl:with-param name="pPattern" select="$number2DecPattern" />
          </xsl:call-template>
        </xsl:variable>
        <!-- FI 20150921 [21311] use variable labelTax-->
        <xsl:variable name ="labelTax">
          <fo:inline font-size="{$gBlockSettings_Summary/column[@name='Designation2']/data[@name='Det']/@font-size}"
                   font-weight="{$gBlockSettings_Summary/column[@name='Designation2']/data[@name='Det']/@font-weight}">
            <xsl:value-of select ="$vDisplayTaxType_Short"/>
          </fo:inline>
          <xsl:value-of select ="concat($gcSpace,$vCurrentTax/@taxId,$gcSpace,$vDisplayRate)"/>
        </xsl:variable>

        <!-- RD 20160406 [21284] Add parameter pIsAmountMandatory = true() -->
        <xsl:call-template name="Synthesis_SummaryRowAmount">
          <xsl:with-param name="pAmountName" select="$labelTax"/>
          <xsl:with-param name="pIsAmountNameLabel" select="true()"/>
          <xsl:with-param name="pAmount" select="$vTaxAmount"/>
          <xsl:with-param name="pExchangeAmount" select="$vExchangeTaxAmount"/>
          <xsl:with-param name="pIsAmountMandatory" select="true()"/>
          <xsl:with-param name="pBizGroup" select="$pBizGroup"/>
          <xsl:with-param name="pIsDetail" select="true()"/>
        </xsl:call-template>
      </xsl:for-each>
    </xsl:for-each>
  </xsl:template>

  <!-- ................................................ -->
  <!-- Synthesis_SummaryRowAmount                       -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Row Summary Amount                      -->
  <!-- ................................................ -->
  <!-- FI 20150921 [21311] Refactoring -->
  <xsl:template name="Synthesis_SummaryRowAmount">
    <!-- BizGroup -->
    <xsl:param name="pBizGroup"/>
    <!-- Represente le type de montant (notamment pour récupérer sa resource) -->
    <xsl:param name="pAmountName"/>
    <!-- si true()  signifie que le paramètre pAmountName represente déjà une désignation correctement formattée -->
    <xsl:param name="pIsAmountNameLabel" select ="false()"/>
    <!-- Represente une liste de montant (montant pour chaque stream) -->
    <xsl:param name="pAmount"/>
    <!-- Represente le montant en contrevaleur (pris en considération uniquement si pBizGroup/exchangeCurrency)  -->
    <xsl:param name="pExchangeAmount"/>
    <!-- Si true() => L'état affiche 0 si le montant n'existe pas-->
    <xsl:param name="pIsAmountMandatory" select="false()"/>
    <!-- Mode d'affichage  
    - normal
    - bold     => ligne: en gras
    - master   => ligne: en gras, montant: couleur fonction du sens,  background-color inchangé
    - subtotal => ligne: en gras, montant: couleur fonction du sens,  background-color du montant fonction du sens
    - minor    => ligne: normal,  montant: couleur en gris claire
    -->
    <xsl:param name="pMode" select="'normal'"/>
    <!-- Si true() => Row dans une section de type detail (ex: "Detail of Profit / Loss" ou "Detail of commissions / Fees")-->
    <xsl:param name="pIsDetail" select="false()"/>
    <!-- couleur de Background -->
    <xsl:param name="pDefaultBackground-color" select="'default'"/>


    <xsl:variable name ="vDefaultBackground-color">
      <xsl:choose>
        <xsl:when test="$pIsDetail=true() and $pDefaultBackground-color='default'">
          <xsl:value-of select ="$gBlockSettings_Summary/@background-color"/>
        </xsl:when>
        <xsl:otherwise>default</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <fo:table-row font-size="{$gData_font-size}"
                  text-align="{$gData_text-align}"
                  display-align="{$gData_display-align}"
                  keep-with-previous="always">

      <xsl:attribute name="font-weight">
        <xsl:choose>
          <xsl:when test="$pMode = 'master' or $pMode = 'subtotal' or $pMode = 'bold'">
            <xsl:value-of select="$gData_Master_font-weight"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$gData_font-weight"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>

      <!--Designation-->
      <xsl:if test="$pIsDetail = true()">
        <fo:table-cell>
          <xsl:if test="$vDefaultBackground-color != 'default'">
            <xsl:attribute name="background-color">
              <xsl:value-of select="$vDefaultBackground-color"/>
            </xsl:attribute>
          </xsl:if>
        </fo:table-cell>
      </xsl:if>

      <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}"
                     text-align="{$gBlockSettings_Summary/column[@name='Designation2']/@text-align}">
        <xsl:call-template name="Debug_border-green"/>

        <xsl:if test="$vDefaultBackground-color != 'default'">
          <xsl:attribute name="background-color">
            <xsl:value-of select="$vDefaultBackground-color"/>
          </xsl:attribute>
        </xsl:if>

        <xsl:choose>
          <xsl:when test ="$pIsDetail = true()">
            <xsl:attribute name="number-columns-spanned">
              <xsl:value-of select="3"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="number-columns-spanned">
              <xsl:value-of select="4"/>
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>

        <fo:block>
          <xsl:choose>
            <xsl:when test ="$pIsAmountNameLabel=true()">
              <xsl:copy-of select ="$pAmountName"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name ="Synthesis_GetAmountLabel">
                <xsl:with-param name ="pAmountName" select ="$pAmountName"/>
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </fo:block>
      </fo:table-cell>

      <!--Currencies-->
      <xsl:for-each select="$pBizGroup/currency">
        <xsl:sort select="current()/@ccy" data-type="text" order="ascending"/>
        <xsl:variable name="vCurrentCcy" select="current()/@ccy"/>

        <xsl:choose>
          <xsl:when test="$pAmountName = 'SpotRate'">
            <xsl:call-template name="Synthesis_SummaryFxRate">
              <xsl:with-param name="pFlowCcy" select="$vCurrentCcy" />
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="Synthesis_SummaryAmount">
              <xsl:with-param name="pAmount" select="$pAmount[@ccy=$vCurrentCcy]" />
              <xsl:with-param name="pCcy" select="current()/@ccy" />
              <xsl:with-param name="pAmountPattern" select="current()/@pattern" />
              <xsl:with-param name="pWithSide" select="true()"/>
              <xsl:with-param name="pIsAmountMandatory" select="$pIsAmountMandatory"/>
              <xsl:with-param name="pMode" select="$pMode"/>
              <xsl:with-param name="pDefaultBackground-color" select="$vDefaultBackground-color" />
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>

      <!--Exchange Currency-->
      <xsl:if test="$pBizGroup/exchangeCurrency">
        <xsl:choose>
          <xsl:when test="$pAmountName = 'SpotRate'"/>
          <xsl:otherwise>
            <xsl:call-template name="Synthesis_SummaryAmount">
              <xsl:with-param name="pAmount" select="$pExchangeAmount" />
              <xsl:with-param name="pCcy" select="$pBizGroup/exchangeCurrency/@ccy" />
              <xsl:with-param name="pAmountPattern" select="$pBizGroup/exchangeCurrency/@pattern" />
              <xsl:with-param name="pWithSide" select="true()"/>
              <xsl:with-param name="pIsAmountMandatory" select="$pIsAmountMandatory"/>
              <xsl:with-param name="pMode" select="$pMode"/>
              <xsl:with-param name="pDefaultBackground-color" select="$vDefaultBackground-color" />
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>

      <!--Empty-->
      <xsl:if test="number ($pBizGroup/@empty-currency) > 0">
        <fo:table-cell>
          <xsl:call-template name="Debug_border-green"/>
        </fo:table-cell>
      </xsl:if>
    </fo:table-row>
  </xsl:template>


  <!-- ................................................ -->
  <!-- Synthesis_SummaryRowTitle                        -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Summary Title                           -->
  <!-- ................................................ -->
  <xsl:template name="Synthesis_SummaryRowTitle">
    <xsl:param name="pBizGroup"/>
    <!-- Représente le titre -->
    <xsl:param name="pTitleName"/>
    <!-- Style appliqué -->
    <xsl:param name="pFont-style" select="'italic'"/>
    <!-- true() si dans le section détail  -->
    <xsl:param name="pIsDetail" select="true()"/>

    <fo:table-row height="{$gBlockSettings_Summary/@space-height}">
      <!--Designation-->
      <fo:table-cell font-size="{$gData_font-size}"
                     font-style="{$pFont-style}"
                     text-align="{$gBlockSettings_Summary/column[@name='Designation2']/@text-align}"
                     display-align="{$gData_display-align}"
                     keep-with-previous="always"
                     number-columns-spanned="4">

        <xsl:if test="$pIsDetail = true()">
          <xsl:attribute name="background-color">
            <xsl:value-of select="$gBlockSettings_Summary/@background-color"/>
          </xsl:attribute>
        </xsl:if>

        <fo:block>
          <xsl:variable name="vResource" select="$gBlockSettings_Summary/column[@name='Designation2']/data[@name=$pTitleName]/@resourceDetail"/>
          <xsl:choose>
            <xsl:when test="string-length($vResource) > 0">
              <xsl:value-of select="$vResource"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$pTitleName"/>
            </xsl:otherwise>
          </xsl:choose>
        </fo:block>

      </fo:table-cell>
      <!--Currencies-->
      <xsl:for-each select="$pBizGroup/currency">
        <fo:table-cell border="{$gTable_border}" border-right-style="none" border-top-style="none" border-bottom-style="none">
          <xsl:if test="$pIsDetail = true()">
            <xsl:attribute name="background-color">
              <xsl:value-of select="$gBlockSettings_Summary/@background-color"/>
            </xsl:attribute>
          </xsl:if>
        </fo:table-cell>
        <fo:table-cell border="{$gTable_border}" border-left-style="none" border-top-style="none" border-bottom-style="none">
          <xsl:if test="$pIsDetail = true()">
            <xsl:attribute name="background-color">
              <xsl:value-of select="$gBlockSettings_Summary/@background-color"/>
            </xsl:attribute>
          </xsl:if>
        </fo:table-cell>
      </xsl:for-each>
      <!--Exchange Currency-->
      <xsl:if test="$pBizGroup/exchangeCurrency">
        <fo:table-cell border="{$gTable_border}" border-right-style="none" border-top-style="none" border-bottom-style="none">
          <xsl:if test="$pIsDetail = true()">
            <xsl:attribute name="background-color">
              <xsl:value-of select="$gBlockSettings_Summary/@background-color"/>
            </xsl:attribute>
          </xsl:if>
        </fo:table-cell>
        <fo:table-cell border="{$gTable_border}" border-left-style="none" border-top-style="none" border-bottom-style="none">
          <xsl:if test="$pIsDetail = true()">
            <xsl:attribute name="background-color">
              <xsl:value-of select="$gBlockSettings_Summary/@background-color"/>
            </xsl:attribute>
          </xsl:if>
        </fo:table-cell>
      </xsl:if>
      <!--Empty-->
      <xsl:if test="number ($pBizGroup/@empty-currency) > 0">
        <fo:table-cell>
          <xsl:call-template name="Debug_border-green"/>
        </fo:table-cell>
      </xsl:if>
    </fo:table-row>
  </xsl:template>

  <!-- ................................................ -->
  <!-- Synthesis_SummaryAmount                        -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Summary Amount 
        1 cell for amount
        1 cell for cr/dr                                -->
  <!-- ................................................ -->
  <!-- FI 20150921 [21311] Refactoring -->
  <xsl:template name="Synthesis_SummaryAmount">
    <!-- Représente une liste de montant (le template en effectue la somme) (liste de noeuds avec attributs amt et side)-->
    <xsl:param name="pAmount"/>
    <!-- Formatage du montant -->
    <xsl:param name="pAmountPattern"/>
    <!-- Représente la devise -->
    <xsl:param name="pCcy"/>
    <!-- true() s'il faut afficher le sens (CR, DR, ...)-->
    <xsl:param name="pWithSide" select="true()"/>
    <!-- si true() Affiche 0 si pAmount est null, sinon Affiche blanc -->
    <xsl:param name="pIsAmountMandatory" select="false()"/>
    <!-- Mode d'affichage  
    - normal   => affichage classique
    - master   => color fonction du sens 
    - subtotal => color fonction du sens, background-color fonction du sens
    - minor    => color gris clair, font-style:italic
    -->
    <xsl:param name="pMode" select="'normal'"/>
    <!-- si différent de default Applique un back-grouf-->
    <xsl:param name="pDefaultBackground-color" select="'default'"/>


    <xsl:choose>
      <xsl:when test="$pAmount or $pIsAmountMandatory">
        <xsl:variable name="vAmount">
          <xsl:choose>
            <xsl:when test="$pAmount/@side">
              <xsl:value-of select="number(sum($pAmount[@side=$gcCredit]/@amt) - sum($pAmount[@side=$gcDebit]/@amt))"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="number(sum($pAmount/@amt))"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="vFormattedAmount">
          <xsl:call-template name="DisplayAmount">
            <xsl:with-param name="pAmount" select="$vAmount"/>
            <xsl:with-param name="pCcy" select="$pCcy"/>
            <xsl:with-param name="pAmountPattern" select="$pAmountPattern" />
          </xsl:call-template>
        </xsl:variable>

        <xsl:variable name="vSide">
          <xsl:call-template name="GetAmount-side">
            <xsl:with-param name="pAmount" select="$vAmount"/>
          </xsl:call-template>
        </xsl:variable>


        <!-- Cell Amount -->
        <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}"
                       text-align="{$gData_Number_text-align}"
                       border="{$gTable_border}" border-top-style="none" border-bottom-style="none" border-right-style="none" >

          <xsl:call-template name ="SummaryAmount_AddAttribute">
            <xsl:with-param name ="pAmount" select="$vAmount"/>
            <xsl:with-param name ="pMode" select="$pMode"/>
            <xsl:with-param name ="pDefaultBackground-color" select="$pDefaultBackground-color"/>
          </xsl:call-template>

          <fo:block>
            <xsl:call-template name="Debug_border-green"/>
            <xsl:choose>
              <xsl:when test="$pWithSide = false()">
                <xsl:value-of select="$vFormattedAmount"/>
              </xsl:when>
              <xsl:when test="contains(',LeftDRCR,LeftDR,LeftMinusPlus,LeftMinus,',concat(',',$gAmount-format,','))">
                <fo:inline font-size="{$gData_Det_font-size}">
                  <xsl:value-of select="concat($vSide,$gcSpace)"/>
                </fo:inline>
                <fo:inline>
                  <xsl:value-of select="$vFormattedAmount"/>
                </fo:inline>
              </xsl:when>
              <xsl:when test="$gAmount-format='Parenthesis' and string-length($vSide) > 0">
                <xsl:value-of select="concat('(',$vFormattedAmount,')')"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$vFormattedAmount"/>
              </xsl:otherwise>
            </xsl:choose>
          </fo:block>
        </fo:table-cell>

        <!-- Cell Side -->
        <fo:table-cell font-size="{$gData_Det_font-size}" font-weight="{$gBlockSettings_Summary/column[@name='DrCr']/@font-weight}"
											 padding-top="{$gData_padding}" padding-bottom="{$gData_padding}"
                       text-align="{$gBlockSettings_Summary/column[@name='DrCr']/@text-align}" display-align="{$gData_display-align}"
                       border="{$gTable_border}" border-left-style="none" border-top-style="none" border-bottom-style="none">

          <xsl:call-template name="SummaryAmount_AddAttribute">
            <xsl:with-param name="pAmount" select="$vAmount"/>
            <xsl:with-param name="pMode" select="$pMode"/>
            <xsl:with-param name="pDefaultBackground-color" select ="$pDefaultBackground-color"/>
          </xsl:call-template>

          <xsl:choose>
            <xsl:when test="contains(',Default,RightDRCR,RightDR,RightMinusPlus,RightMinus,',concat(',',$gAmount-format,','))
                and $pWithSide = true()">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <xsl:value-of select="$vSide"/>
              </fo:block>
            </xsl:when>
            <xsl:otherwise>
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
              </fo:block>
            </xsl:otherwise>
          </xsl:choose>
        </fo:table-cell>
      </xsl:when>

      <xsl:otherwise>
        <!-- Cell Amount -->
        <fo:table-cell border="{$gTable_border}" border-right-style="none" border-top-style="none" border-bottom-style="none">
          <xsl:if test="$pDefaultBackground-color != 'default'">
            <xsl:attribute name="background-color">
              <xsl:value-of select="$pDefaultBackground-color"/>
            </xsl:attribute>
          </xsl:if>
        </fo:table-cell>

        <!-- Cell Side -->
        <fo:table-cell border="{$gTable_border}" border-left-style="none" border-top-style="none" border-bottom-style="none">
          <xsl:if test="$pDefaultBackground-color != 'default'">
            <xsl:attribute name="background-color">
              <xsl:value-of select="$pDefaultBackground-color"/>
            </xsl:attribute>
          </xsl:if>
        </fo:table-cell>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!-- ................................................ -->
  <!-- SummaryAmount_AddAttribute                       -->
  <!-- ................................................ -->
  <!-- FI 20150921 [21311] Add -->
  <xsl:template name ="SummaryAmount_AddAttribute">
    <xsl:param name="pAmount"/>
    <xsl:param name="pMode" select="'normal'"/>
    <xsl:param name="pDefaultBackground-color" select="'default'"/>

    <!-- color attribute -->
    <xsl:variable name="vAmount-color">
      <xsl:choose>
        <xsl:when test="$pMode='subtotal' or $pMode='master'">
          <xsl:call-template name="GetAmount-color">
            <xsl:with-param name="pAmount" select="$pAmount"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pMode='minor'">
          <xsl:value-of select="$gBlockSettings_Summary/@minor-color"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:if test="$vAmount-color">
      <xsl:call-template name="Display_AddAttribute-color">
        <xsl:with-param name="pColor" select="$vAmount-color"/>
      </xsl:call-template>
    </xsl:if>

    <!-- background-color attribute -->
    <xsl:variable name="vAmount-background-color">
      <xsl:choose>
        <xsl:when test="$pMode='subtotal'">
          <xsl:call-template name="GetAmount-background-color">
            <xsl:with-param name="pAmount" select="$pAmount"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pDefaultBackground-color != 'default'">
          <xsl:value-of select="$pDefaultBackground-color"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <xsl:if test ="$vAmount-background-color">
      <xsl:call-template name="Display_AddAttribute-color">
        <xsl:with-param name="pColor" select="$vAmount-background-color"/>
        <xsl:with-param name="pAttributeName" select="'background-color'"/>
      </xsl:call-template>
    </xsl:if>


  </xsl:template>


  <!-- ................................................ -->
  <!-- Synthesis_SummaryFxRate                          -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Summary Fx Rate                         -->
  <!-- ................................................ -->
  <xsl:template name="Synthesis_SummaryFxRate">
    <xsl:param name="pFlowCcy"/>

    <xsl:variable name="vExchangeRate"
                  select="$gCBTrade/cashBalanceReport/fxRate[(quotedCurrencyPair/currency1=$pFlowCcy and quotedCurrencyPair/currency2=$gExchangeCcy)
                  or (quotedCurrencyPair/currency1=$gExchangeCcy and quotedCurrencyPair/currency2=$pFlowCcy)]"/>

    <fo:table-cell font-size="{$gBlockSettings_Summary/column[@name='Currency']/data[@name='SpotRate']/@font-size}"
                   font-weight="{$gBlockSettings_Summary/column[@name='Currency']/data[@name='SpotRate']/@font-weight}"
                   text-align="{$gBlockSettings_Summary/column[@name='Currency']/data[@name='SpotRate']/@text-align}"
                   padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
      <fo:block>
        <xsl:call-template name="DisplayeExchangeRate">
          <xsl:with-param name="pExchangeRate" select="$vExchangeRate"/>
          <xsl:with-param name="pFlowCcy" select="$pFlowCcy" />
        </xsl:call-template>
      </fo:block>
    </fo:table-cell>
    <fo:table-cell />
  </xsl:template>

  <!-- ................................................ -->
  <!-- Synthesis_SummaryRowEmpty                        -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Summary Empty row                       -->
  <!-- ................................................ -->
  <xsl:template name="Synthesis_SummaryRowEmpty">
    <xsl:param name="pBizGroup"/>
    <xsl:param name="pWithBorder" select="true()"/>

    <fo:table-row height="{$gBlockSettings_Summary/@space-height}">
      <!--Designation-->
      <fo:table-cell number-columns-spanned="4">
        <xsl:call-template name="Debug_border-green"/>
      </fo:table-cell>
      <xsl:if test="$pWithBorder">
        <!--Currencies-->
        <xsl:for-each select="$pBizGroup/currency">
          <fo:table-cell border="{$gTable_border}" border-right-style="none" border-top-style="none" border-bottom-style="none"/>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-top-style="none" border-bottom-style="none"/>
        </xsl:for-each>
        <!--Exchange Currency-->
        <xsl:if test="$pBizGroup/exchangeCurrency">
          <fo:table-cell border="{$gTable_border}" border-right-style="none" border-top-style="none" border-bottom-style="none"/>
          <fo:table-cell border="{$gTable_border}" border-left-style="none" border-top-style="none" border-bottom-style="none"/>
        </xsl:if>
        <!--Empty-->
        <xsl:if test="number ($pBizGroup/@empty-currency) > 0">
          <fo:table-cell>
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
        </xsl:if>
      </xsl:if>
    </fo:table-row>
  </xsl:template>

  <!-- ................................................ -->
  <!-- Synthesis_SummaryRowLine                       -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Display Summary Line                            -->
  <!-- ................................................ -->
  <xsl:template name="Synthesis_SummaryRowLine">
    <xsl:param name="pBizGroup"/>

    <fo:table-row>
      <!--Designation-->
      <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none" border-top-style="none"
                     number-columns-spanned="4"/>
      <!--Currencies-->
      <xsl:for-each select="$pBizGroup/currency">
        <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none" border-top-style="none"
                       number-columns-spanned="2"/>
      </xsl:for-each>
      <!--Exchange Currency-->
      <xsl:if test="$pBizGroup/exchangeCurrency">
        <fo:table-cell border="{$gTable_border}" border-left-style="none" border-right-style="none" border-top-style="none"
                       number-columns-spanned="2"/>
      </xsl:if>
      <!--Empty-->
      <xsl:if test="number ($pBizGroup/@empty-currency) > 0">
        <fo:table-cell>
          <xsl:call-template name="Debug_border-green"/>
        </fo:table-cell>
      </xsl:if>
    </fo:table-row>
  </xsl:template>

  <!-- .......................................................................... -->
  <!--              Tools: Block Header/Footer                                    -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- UKSection_Header                                 -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Section Header: Title          
       ................................................ -->
  <xsl:template name="UKSection_Header">
    <xsl:param name="pResource"/>
    <xsl:param name="pInstr"/>
    <xsl:param name="pDetail"/>
    <xsl:param name="pValueDate"/>

    <fo:table-header>
      <!--Space-->
      <fo:table-row>
        <fo:table-cell number-columns-spanned="2">
          <!--Space-->
          <fo:block space-before="{$gSection_space-before}"/>
        </fo:table-cell>
      </fo:table-row>
      <!-- Header -->
      <fo:table-row font-size="{$gBlockSettings_Section/@font-size}"
                    font-weight="{$gBlockSettings_Section/@font-weight}"
                    text-align="{$gBlockSettings_Section/@text-align}">

        <fo:table-cell padding="{$gBlockSettings_Section/@padding}" padding-bottom="{$gBlockSettings_Section/@padding-bottom}"
                       display-align="{$gBlockSettings_Section/@display-align}">
          <xsl:attribute name="color">
            <xsl:value-of select="$gBlockSettings_Section/@color"/>
          </xsl:attribute>
          <xsl:attribute name="background-color">
            <xsl:value-of select="$gBlockSettings_Section/@background-color"/>
          </xsl:attribute>
          <xsl:choose>
            <xsl:when test="$gSectionBanner-style = 'Banner'">
              <xsl:attribute name="number-columns-spanned">
                <xsl:value-of select="'2'"/>
              </xsl:attribute>
            </xsl:when>
            <xsl:when test="$gSectionBanner-style = 'Tab'">
              <xsl:attribute name="border">
                <xsl:value-of select="$gBlockSettings_Section/@border"/>
              </xsl:attribute>
              <xsl:attribute name="border-left-style">
                <xsl:value-of select="'none'"/>
              </xsl:attribute>
              <xsl:attribute name="border-right-style">
                <xsl:value-of select="'none'"/>
              </xsl:attribute>
              <xsl:attribute name="border-top-style">
                <xsl:value-of select="'none'"/>
              </xsl:attribute>
              <xsl:attribute name="border-bottom-width">
                <xsl:value-of select="$gBlockSettings_Section/@border-bottom-width"/>
              </xsl:attribute>
              <xsl:attribute name="border-bottom-color">
                <xsl:value-of select="$gBlockSettings_Section/@border-bottom-color"/>
              </xsl:attribute>
            </xsl:when>
          </xsl:choose>

          <fo:block>
            <xsl:value-of select="$pResource"/>
            <xsl:if test="string-length($pInstr) >0">
              <xsl:value-of select="concat(' - ',$pInstr)"/>
            </xsl:if>
            <xsl:if test="string-length($pValueDate) > 0">
              <xsl:value-of select="concat(' - ',$pValueDate)"/>
            </xsl:if>
            <xsl:if test="string-length($pDetail) >0">
              <fo:inline font-weight="{$gBlockSettings_Section/data[@name='det']/@font-weight}">
                <xsl:value-of select="concat(' - ',$pDetail)"/>
              </fo:inline>
            </xsl:if>
          </fo:block>
        </fo:table-cell>
        <xsl:if test="$gSectionBanner-style = 'Tab'">
          <fo:table-cell padding="{$gBlockSettings_Section/@padding}" padding-bottom="{$gBlockSettings_Section/@padding-bottom}"
                        display-align="{$gBlockSettings_Section/@display-align}">
            <xsl:attribute name="border">
              <xsl:value-of select="$gBlockSettings_Section/@border"/>
            </xsl:attribute>
            <xsl:attribute name="border-left-style">
              <xsl:value-of select="'none'"/>
            </xsl:attribute>
            <xsl:attribute name="border-right-style">
              <xsl:value-of select="'none'"/>
            </xsl:attribute>
            <xsl:attribute name="border-top-style">
              <xsl:value-of select="'none'"/>
            </xsl:attribute>
            <xsl:attribute name="border-bottom-width">
              <xsl:value-of select="$gBlockSettings_Section/@border-bottom-width"/>
            </xsl:attribute>
            <xsl:attribute name="border-bottom-color">
              <xsl:value-of select="$gBlockSettings_Section/@border-bottom-color"/>
            </xsl:attribute>
            <fo:block/>
          </fo:table-cell>
        </xsl:if>
      </fo:table-row>
      <!-- Space -->
      <fo:table-row>
        <fo:table-cell number-columns-spanned="2">
          <!--Space-->
          <fo:block space-before="{$gBlock_space-before}"/>
        </fo:table-cell>
      </fo:table-row>
    </fo:table-header>
  </xsl:template>

  <!-- ................................................ -->
  <!-- UKSection_Footer                                 -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Section Footer: Fee Legend          
       ................................................ -->
  <xsl:template name="UKSection_Footer">
    <xsl:param name="pLegends"/>
    <xsl:param name="pMode" select="'OneColumn'"/>

    <xsl:if test="$pLegends">
      <!--Report Legend-->
      <fo:table-footer>
        <fo:table-row>
          <fo:table-cell number-columns-spanned="2">
            <fo:block font-size="{$gData_font-size}"
                      padding-top="{$gData_padding}" padding-bottom="{$gData_padding}"
                      display-align="{$gData_display-align}"
                      keep-together.within-page="always"
                      space-before="{$gBlockSettings_Legend/@space-before}">

              <!--BreakLine-->
              <!--EG Set width=$gSection_width-->
              <!--EG Set column-width=$gLegendLine_width-->
              <fo:table table-layout="fixed" width="{$gSection_width}">
                <fo:table-column column-number="01" column-width="{$gLegendLine_width}"/>
                <fo:table-body>
                  <fo:table-row height="{$gBlock_space-before}">
                    <fo:table-cell>
                      <xsl:call-template name="Debug_border-green"/>
                    </fo:table-cell>
                  </fo:table-row>
                </fo:table-body>
              </fo:table>
              <xsl:choose>
                <xsl:when test="$pMode='OneColumn'">
                  <xsl:variable name="vLegendByLine" select="7"/>
                  <!--EG Set width=$gSection_width-->
                  <!--EG Set column-width=$gLegendLine_width-->
                  <fo:table table-layout="fixed" width="{$gSection_width}">
                    <fo:table-column column-number="01" column-width="{$gLegendLine_width}"/>
                    <fo:table-body>
                      <xsl:for-each select="$pLegends[position() mod $vLegendByLine = 1]">
                        <xsl:sort select="@value"/>

                        <xsl:variable name="vRowPosition" select="position()"/>
                        <xsl:variable name="vLegendRow" select="$pLegends[($vRowPosition * $vLegendByLine) >= position() and position() > (($vRowPosition - 1) * $vLegendByLine)]"/>

                        <fo:table-row>
                          <fo:table-cell font-size="{$gBlockSettings_Legend/column[@name='Value']/data/@font-size}"
                                       font-weight="{$gBlockSettings_Legend/column[@name='Value']/data/@font-weight}">
                            <fo:block>
                              <xsl:call-template name="Debug_border-green"/>
                              <xsl:for-each select="$vLegendRow">
                                <xsl:sort select="@value"/>

                                <xsl:variable name="vLegend" select="current()"/>
                                <xsl:value-of select="concat($vLegend/@value, $gcSpace, $vLegend/@extvalue)"/>
                                <xsl:value-of select="concat($gcSpace, $gcSpace,$gcSpace, $gcSpace,$gcSpace, $gcSpace,$gcSpace, $gcSpace)"/>
                              </xsl:for-each>
                            </fo:block>
                          </fo:table-cell>

                        </fo:table-row>
                      </xsl:for-each>
                    </fo:table-body>
                  </fo:table>
                </xsl:when>
                <xsl:when test="$pMode='TwoColumns'">
                  <!--EG Set width=$gSection_width-->
                  <fo:table table-layout="fixed" width="{$gSection_width}">
                    <fo:table-column column-number="01" column-width="{$gBlockSettings_Legend/column[@name='Value']/@column-width}"/>
                    <fo:table-column column-number="02" column-width="{$gBlockSettings_Legend/column[@name='ExtValue']/@column-width}"/>
                    <fo:table-body>
                      <xsl:for-each select="$pLegends">
                        <xsl:sort select="@value"/>

                        <xsl:variable name="vLegend" select="current()"/>
                        <fo:table-row>
                          <fo:table-cell font-size="{$gBlockSettings_Legend/column[@name='Value']/data/@font-size}"
                                         font-weight="{$gBlockSettings_Legend/column[@name='Value']/data/@font-weight}">
                            <fo:block>
                              <xsl:call-template name="Debug_border-green"/>
                              <xsl:value-of select="$vLegend/@value"/>
                            </fo:block>
                          </fo:table-cell>
                          <fo:table-cell font-size="{$gBlockSettings_Legend/column[@name='ExtValue']/data/@font-size}"
                                         font-weight="{$gBlockSettings_Legend/column[@name='ExtValue']/data/@font-weight}">
                            <fo:block>
                              <xsl:call-template name="Debug_border-green"/>
                              <xsl:value-of select="$vLegend/@extvalue"/>
                            </fo:block>
                          </fo:table-cell>
                        </fo:table-row>
                      </xsl:for-each>
                    </fo:table-body>
                  </fo:table>
                </xsl:when>
                <xsl:when test="$pMode='Table'">
                  <xsl:variable name="vLegendByLine" select="7"/>
                  <!--EG Set width=$gSection_width-->
                  <fo:table table-layout="fixed" width="{$gSection_width}">
                    <xsl:for-each select="$pLegends[$vLegendByLine >= position()]">
                      <fo:table-column column-number="{((position() - 1 )* 2 ) + 1}" column-width="{$gBlockSettings_Legend/column[@name='Value']/@column-width}"/>
                      <fo:table-column column-number="{((position() - 1 )* 2 ) + 2}" column-width="{$gBlockSettings_Legend/column[@name='ExtValue']/@column-width}"/>
                    </xsl:for-each>
                    <fo:table-body>
                      <xsl:for-each select="$pLegends[position() mod $vLegendByLine = 1]">
                        <xsl:sort select="@value"/>

                        <xsl:variable name="vRowPosition" select="position()"/>
                        <xsl:variable name="vLegendRow" select="$pLegends[($vRowPosition * $vLegendByLine) >= position() and position() > (($vRowPosition - 1) * $vLegendByLine)]"/>

                        <fo:table-row>
                          <xsl:for-each select="$vLegendRow">
                            <xsl:sort select="@value"/>

                            <xsl:variable name="vLegend" select="current()"/>

                            <fo:table-cell font-size="{$gBlockSettings_Legend/column[@name='Value']/data/@font-size}"
                                           font-weight="{$gBlockSettings_Legend/column[@name='Value']/data/@font-weight}">
                              <fo:block>
                                <xsl:call-template name="Debug_border-green"/>
                                <xsl:value-of select="$vLegend/@value"/>
                              </fo:block>
                            </fo:table-cell>
                            <fo:table-cell font-size="{$gBlockSettings_Legend/column[@name='ExtValue']/data/@font-size}"
                                           font-weight="{$gBlockSettings_Legend/column[@name='ExtValue']/data/@font-weight}">
                              <fo:block>
                                <xsl:call-template name="Debug_border-green"/>
                                <xsl:value-of select="$vLegend/@extvalue"/>
                              </fo:block>
                            </fo:table-cell>
                          </xsl:for-each>
                        </fo:table-row>
                      </xsl:for-each>
                    </fo:table-body>
                  </fo:table>
                </xsl:when>
              </xsl:choose>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
      </fo:table-footer>
    </xsl:if>
  </xsl:template>

  <!-- ................................................ -->
  <!-- UKSection_GetLegendFee                           -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Get Fee Legend          
       ................................................ -->
  <xsl:template name="UKSection_GetLegendFee">
    <xsl:param name="pSubTotalFees"/>

    <xsl:if test="$pSubTotalFees">

      <!--PaymentType Legend-->
      <xsl:variable name="vFeeCopyNode">
        <xsl:copy-of select="$pSubTotalFees"/>
      </xsl:variable>
      <xsl:variable name="vFeeCopy" select="msxsl:node-set($vFeeCopyNode)/fee"/>
      <xsl:variable name="vFee-paymentType" select="$vFeeCopy[generate-id()=generate-id(key('kFee-paymentType',@paymentType))]"/>

      <xsl:for-each select="$vFee-paymentType">

        <xsl:variable name="vPaymentType" select="current()"/>
        <xsl:variable name="vValue">
          <xsl:call-template name="DisplayType_Short">
            <xsl:with-param name="pType" select="$vPaymentType/@paymentType"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:variable name="vExtValue">
          <xsl:call-template name="DisplayPaymentType">
            <xsl:with-param name="pPaymentType" select="$vPaymentType/@paymentType"/>
          </xsl:call-template>
        </xsl:variable>

        <legend name="feePaymentType" value="{$vValue}" extvalue="{$vExtValue}"/>
      </xsl:for-each>
    </xsl:if>
  </xsl:template>

  <!-- ................................................ -->
  <!-- UKSection_GetLegendCashPayment                   -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Get CashPayment Legend          
       ................................................ -->
  <xsl:template name="UKSection_GetLegendCashPayment">
    <xsl:param name="pCashPayments"/>

    <xsl:if test="$pCashPayments">

      <!--PaymentType Legend-->
      <xsl:variable name="vCashPaymentsCopyNode">
        <xsl:copy-of select="$pCashPayments"/>
      </xsl:variable>
      <xsl:variable name="vCashPaymentsCopy" select="msxsl:node-set($vCashPaymentsCopyNode)/cashPayment"/>
      <xsl:variable name="vCashPayment-eventType" select="$vCashPaymentsCopy[generate-id()=generate-id(key('kCashPayment-eventType',@eventType))]"/>

      <xsl:for-each select="$vCashPayment-eventType">

        <xsl:variable name="vEventType" select="current()"/>
        <xsl:variable name="vValue">
          <xsl:call-template name="DisplayType_Short">
            <xsl:with-param name="pType" select="$vEventType/@eventType"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:variable name="vExtValue" select="$gRepository/enums[@id='ENUMS.CODE.PaymentType']/enum[value/text()=$vEventType/@paymentType]/extvalue/text()"/>

        <legend name="cashPayments" value="{$vValue}" extvalue="{$vExtValue}"/>
      </xsl:for-each>
    </xsl:if>
  </xsl:template>

  <!-- ................................................ -->
  <!-- UKSection_GetLegendFunding                       -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Get Funding Legend          
       ................................................ -->
  <xsl:template name="UKSection_GetLegendFunding">
    <xsl:param name="pFundings"/>

    <xsl:if test="$pFundings">
      <xsl:variable name="vValue">
        <xsl:call-template name="DisplayType_Short">
          <xsl:with-param name="pType" select="'FDA'"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:variable name="vExtValue">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'Report-FundingAmount'" />
        </xsl:call-template>
      </xsl:variable>

      <legend name="funding" value="{$vValue}" extvalue="{$vExtValue}"/>
    </xsl:if>
  </xsl:template>

  <!-- ................................................ -->
  <!-- UKSection_GetLegendBorrowing                     -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Get Borrowing Legend          
       ................................................ -->
  <xsl:template name="UKSection_GetLegendBorrowing">
    <xsl:param name="pBorrowings"/>

    <xsl:if test="$pBorrowings">
      <xsl:variable name="vValue">
        <xsl:call-template name="DisplayType_Short">
          <xsl:with-param name="pType" select="'BWA'"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:variable name="vExtValue">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'Report-BorrowingAmount'" />
        </xsl:call-template>
      </xsl:variable>

      <legend name="borrowing" value="{$vValue}" extvalue="{$vExtValue}"/>
    </xsl:if>
  </xsl:template>


  <!-- ................................................ -->
  <!-- UKSection_GetLegendBorrowing                     -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Get Borrowing Legend          
       ................................................ -->
  <xsl:template name="UKSection_GetLegendSafekeeping">
    <xsl:param name="pSafekeepings"/>

    <xsl:if test="$pSafekeepings">
      <xsl:variable name="vValue">
        <xsl:call-template name="DisplayType_Short">
          <xsl:with-param name="pType" select="'CUS'"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:variable name="vExtValue">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'Report-CustodyFee'" />
        </xsl:call-template>
      </xsl:variable>

      <legend name="borrowing" value="{$vValue}" extvalue="{$vExtValue}"/>
    </xsl:if>
  </xsl:template>

  <!-- ................................................ -->
  <!-- UKSection_GetAmountLegend                        -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Get Amount Legend          
       ................................................ -->
  <!-- FI 20160208 [21311] Add -->
  <xsl:template name="UKSection_GetAmountLegend">
    <xsl:param name="pName"/>
    <xsl:param name="pType"/>

    <xsl:variable name="vValue">
      <xsl:call-template name="DisplayType_Short">
        <xsl:with-param name="pType" select="$pType"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vExtValue" select="$gBlockSettings_Data/column[@name='Amount']/data[@name=$pType]/@resource"/>

    <legend name="{$pName}" value="{$vValue}" extvalue="{$vExtValue}"/>
  </xsl:template>


  <!-- ................................................ -->
  <!-- UKMarketDC_Header                                -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Title Block with Market and DC          
       ................................................ -->
  <!-- FI 20150126 [21825] Add pBiz-derivativeContract -->
  <xsl:template name="UKMarketDC_Header">
    <xsl:param name="pRepository-market" />
    <xsl:param name="pBiz-derivativeContract" />
    <xsl:param name="pRepository-derivativeContract" />

    <fo:table table-layout="fixed" width="{$gSection_width}"
              keep-together.within-page="always">

      <fo:table-column column-number="01" column-width="{$gBlockSettings_MarketDC/column[@name='Market']/@column-width}"/>
      <fo:table-column column-number="02" column-width="{$gBlockSettings_MarketDC/column[@name='DC']/@column-width}"/>

      <fo:table-body>
        <fo:table-row font-size="{$gBlockSettings_MarketDC/column[@name='Market']/@font-size}"
                      background-color="{$gBlockSettings_MarketDC/column[@name='Market']/@background-color}">
          <fo:table-cell padding="{$gBlockSettings_MarketDC/column[@name='Market']/@padding}"
                         font-weight="{$gBlockSettings_MarketDC/column[@name='Market']/@font-weight}"
                         text-align="{$gBlockSettings_MarketDC/column[@name='Market']/@text-align}"
                         display-align="{$gBlockSettings_MarketDC/column[@name='Market']/@display-align}"
                         border="{$gBlockSettings_MarketDC/@border}" border-left-style="none" border-right-style="none" border-bottom-width="{$gBlockSettings_MarketDC/@border-bottom-width}" >
            <xsl:call-template name="Display_AddAttribute-color">
              <xsl:with-param name="pColor" select="$gBlockSettings_MarketDC/column[@name='Market']/@color"/>
            </xsl:call-template>
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:call-template name="UKDisplayTitle_Market">
                <xsl:with-param name="pRepository-market" select="$pRepository-market" />
                <xsl:with-param name="pDataLength" select="$gBlockSettings_MarketDC/column[@name='Market']/@length"/>
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell padding="{$gBlockSettings_MarketDC/column[@name='DC']/@padding}"
                         font-weight="{$gBlockSettings_MarketDC/column[@name='DC']/@font-weight}"
                         text-align="{$gBlockSettings_MarketDC/column[@name='DC']/@text-align}"
                         display-align="{$gBlockSettings_MarketDC/column[@name='DC']/@display-align}"
                         border="{$gBlockSettings_MarketDC/@border}" border-left-style="none" border-right-style="none" border-bottom-width="{$gBlockSettings_MarketDC/@border-bottom-width}">
            <xsl:call-template name="Display_AddAttribute-color">
              <xsl:with-param name="pColor" select="$gBlockSettings_MarketDC/column[@name='DC']/@color"/>
            </xsl:call-template>
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:call-template name="UKDisplayTitle_DC">
                <xsl:with-param name="pBiz_derivativeContract" select="$pBiz-derivativeContract" />
                <xsl:with-param name="pRepository-derivativeContract" select="$pRepository-derivativeContract" />
                <xsl:with-param name="pColumnSettings" select="$gBlockSettings_MarketDC/column[@name='DC']"/>
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
      </fo:table-body>
    </fo:table>
  </xsl:template>

  <!-- ................................................ -->
  <!-- UKAsset_Header                                -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Asset Block          
       ................................................ -->
  <!-- RD 20190619 [23912] Add -->
  <xsl:template name="UKAsset_Header">
    <xsl:param name="pRepository-Asset" />

    <fo:table table-layout="fixed" width="{$gSection_width}"
              keep-together.within-page="always">

      <fo:table-column column-number="01" column-width="{$gSection_width}"/>

      <fo:table-body>
        <fo:table-row font-size="{$gBlockSettings_Asset/column[@name='AssetDet']/@font-size}"
                      background-color="{$gBlockSettings_Asset/column[@name='AssetDet']/@background-color}">
          <fo:table-cell padding="{$gBlockSettings_Asset/column[@name='AssetDet']/@padding}"
                         font-weight="{$gBlockSettings_Asset/column[@name='AssetDet']/@font-weight}"
                         text-align="{$gBlockSettings_Asset/column[@name='AssetDet']/@text-align}"
                         display-align="{$gBlockSettings_Asset/column[@name='AssetDet']/@display-align}">
            <xsl:call-template name="Display_AddAttribute-color">
              <xsl:with-param name="pColor" select="$gBlockSettings_Asset/column[@name='AssetDet']/@color"/>
            </xsl:call-template>
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>

              <xsl:variable name="vAssetDetNode">
                <data>
                  <xsl:copy-of select="$gBlockSettings_Asset/column[@name='AssetDet']/data[@name='ISINCode']/@*"/>
                  <xsl:value-of select="$pRepository-Asset/isinCode"/>
                </data>
                <data>
                  <xsl:copy-of select="$gBlockSettings_Asset/column[@name='AssetDet']/data[@name='BBGCode']/@*"/>
                  <xsl:value-of select="$pRepository-Asset/bbgCode"/>
                </data>
                <data>
                  <xsl:copy-of select="$gBlockSettings_Asset/column[@name='AssetDet']/data[@name='RICCode']/@*"/>
                  <xsl:value-of select="$pRepository-Asset/ricCode"/>
                </data>
              </xsl:variable>
              <xsl:variable name="vAssetDet" select="msxsl:node-set($vAssetDetNode)/data[@sort!='None' and string-length(text())>0]"/>

              <xsl:for-each select="$vAssetDet">
                <xsl:sort select="@sort"/>

                <xsl:variable name="vAssetDetData">
                  <xsl:choose>
                    <xsl:when test="current()/@name='ISINCode'">
                      <xsl:value-of select="$pRepository-Asset/isinCode"/>
                    </xsl:when>
                    <xsl:when test="current()/@name='BBGCode'">
                      <xsl:value-of select="$pRepository-Asset/bbgCode"/>
                    </xsl:when>
                    <xsl:when test="current()/@name='RICCode'">
                      <xsl:value-of select="$pRepository-Asset/ricCode"/>
                    </xsl:when>
                  </xsl:choose>
                </xsl:variable>

                <xsl:value-of select="concat($gBlockSettings_Asset/column[@name='AssetDet']/data[@name=current()/@name]/@resource,':',$gcSpace)"/>
                <fo:inline font-weight="{$gBlockSettings_Asset/column[@name='AssetDet']/data[@name='det']/@font-weight}">
                  <xsl:value-of select="$vAssetDetData"/>
                </fo:inline>
                <xsl:value-of select="concat($gcSpace,$gcSpace,$gcSpace,$gcSpace)"/>
              </xsl:for-each>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
      </fo:table-body>
    </fo:table>
  </xsl:template>

  <!-- ................................................ -->
  <!-- UKMarketCurrency_Header                          -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Title Block with Market and Currency          
       ................................................ -->
  <xsl:template name="UKMarketCurrency_Header">
    <xsl:param name="pRepository-market"/>
    <xsl:param name="pRepository-currency"/>

    <fo:table table-layout="fixed" width="{$gSection_width}"
              keep-together.within-page="always">

      <fo:table-column column-number="01" column-width="{$gBlockSettings_MarketCcy/column[@name='Market']/@column-width}"/>
      <fo:table-column column-number="02" column-width="{$gBlockSettings_MarketCcy/column[@name='Ccy']/@column-width}"/>

      <fo:table-body>
        <fo:table-row font-size="{$gBlockSettings_MarketCcy/column[@name='Market']/@font-size}"
                      background-color="{$gBlockSettings_MarketCcy/column[@name='Market']/@background-color}">
          <fo:table-cell padding="{$gBlockSettings_MarketCcy/column[@name='Market']/@padding}"
                         font-weight="{$gBlockSettings_MarketCcy/column[@name='Market']/@font-weight}"
                         text-align="{$gBlockSettings_MarketCcy/column[@name='Market']/@text-align}"
                         display-align="{$gBlockSettings_MarketCcy/column[@name='Market']/@display-align}"
                         border="{$gBlockSettings_MarketCcy/@border}" border-left-style="none" border-right-style="none" border-bottom-width="{$gBlockSettings_MarketCcy/@border-bottom-width}" >
            <xsl:call-template name="Display_AddAttribute-color">
              <xsl:with-param name="pColor" select="$gBlockSettings_MarketCcy/column[@name='Market']/@color"/>
            </xsl:call-template>
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:call-template name="UKDisplayTitle_Market">
                <xsl:with-param name="pRepository-market" select="$pRepository-market" />
                <xsl:with-param name="pDataLength" select="$gBlockSettings_MarketCcy/column[@name='Market']/@length"/>
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell padding="{$gBlockSettings_MarketCcy/column[@name='Ccy']/@padding}"
                         font-weight="{$gBlockSettings_MarketCcy/column[@name='Ccy']/@font-weight}"
                         text-align="{$gBlockSettings_MarketCcy/column[@name='Ccy']/@text-align}"
                         display-align="{$gBlockSettings_MarketCcy/column[@name='Ccy']/@display-align}"
                         border="{$gBlockSettings_MarketCcy/@border}" border-left-style="none" border-right-style="none" border-bottom-width="{$gBlockSettings_MarketCcy/@border-bottom-width}">
            <xsl:call-template name="Display_AddAttribute-color">
              <xsl:with-param name="pColor" select="$gBlockSettings_MarketCcy/column[@name='Ccy']/@color"/>
            </xsl:call-template>
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:call-template name="UKDisplayTitle_Ccy">
                <xsl:with-param name="pRepository-currency" select="$pRepository-currency" />
                <xsl:with-param name="pColumnSettings" select="$gBlockSettings_MarketCcy/column[@name='Ccy']"/>
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
      </fo:table-body>
    </fo:table>
  </xsl:template>

  <!-- ................................................ -->
  <!-- UKMarket_Header                                  -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Title Block with Market          
       ................................................ -->
  <xsl:template name="UKMarket_Header">
    <xsl:param name="pRepository-market"/>

    <fo:table table-layout="fixed" width="{$gSection_width}"
              keep-together.within-page="always">

      <fo:table-column column-number="01" column-width="{$gSection_width}"/>

      <fo:table-body>
        <fo:table-row font-size="{$gBlockSettings_MarketDC/column[@name='Market']/@font-size}"
                      background-color="{$gBlockSettings_MarketDC/column[@name='Market']/@background-color}">
          <fo:table-cell padding="{$gBlockSettings_MarketDC/column[@name='Market']/@padding}"
                         font-weight="{$gBlockSettings_MarketDC/column[@name='Market']/@font-weight}"
                         text-align="{$gBlockSettings_MarketDC/column[@name='Market']/@text-align}"
                         display-align="{$gBlockSettings_MarketDC/column[@name='Market']/@display-align}"
                         border="{$gBlockSettings_MarketDC/@border}" border-left-style="none" border-right-style="none" border-bottom-width="{$gBlockSettings_MarketDC/@border-bottom-width}" >
            <xsl:call-template name="Display_AddAttribute-color">
              <xsl:with-param name="pColor" select="$gBlockSettings_MarketDC/column[@name='Market']/@color"/>
            </xsl:call-template>
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:call-template name="UKDisplayTitle_Market">
                <xsl:with-param name="pRepository-market" select="$pRepository-market" />
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
      </fo:table-body>
    </fo:table>
  </xsl:template>

  <!-- ................................................ -->
  <!-- UKBook_Header                                    -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Title of Book Block
       ................................................ -->
  <xsl:template name="UKBook_Header">
    <xsl:param name="pRepository-book"/>

    <fo:table table-layout="fixed" width="{$gSection_width}"
              keep-together.within-page="always">

      <fo:table-column column-number="01" column-width="{$gBlockSettings_Book/column[@name='Label']/@column-width}"/>
      <fo:table-column column-number="02" column-width="proportional-column-width(1)" />

      <fo:table-body>
        <fo:table-row font-size="{$gBlockSettings_Book/column[@name='Label']/@font-size}">
          <fo:table-cell padding="{$gBlockSettings_Book/column[@name='Label']/@padding}"
                         font-weight="{$gBlockSettings_Book/column[@name='Label']/@font-weight}"
                         text-align="{$gBlockSettings_Book/column[@name='Label']/@text-align}"
                         display-align="{$gBlockSettings_Book/column[@name='Label']/@display-align}"
                         background-color="{$gBlockSettings_Book/column[@name='Label']/@background-color}"
                         border="{$gBlockSettings_Book/@border}" border-left-style="none" border-right-style="none" >
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$gBlockSettings_Book/column[@name='Label']/data/@resource"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell padding="{$gBlockSettings_Book/column[@name='Desc']/@padding}"
                         font-weight="{$gBlockSettings_Book/column[@name='Desc']/@font-weight}"
                         text-align="{$gBlockSettings_Book/column[@name='Desc']/@text-align}"
                         display-align="{$gBlockSettings_Book/column[@name='Desc']/@display-align}"
                         number-columns-spanned="3"
                         border="{$gBlockSettings_Book/@border}" border-left-style="none" border-right-style="none" >
            <xsl:call-template name="Display_AddAttribute-color">
              <xsl:with-param name="pColor" select="$gBlockSettings_Book/column[@name='Desc']/@color"/>
            </xsl:call-template>
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:call-template name="UKDisplayTitle_Book">
                <xsl:with-param name="pRepository-book" select="$pRepository-book" />
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
      </fo:table-body>
    </fo:table>
  </xsl:template>

  <!-- ................................................ -->
  <!-- UKTrader_Detail                                  -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Trader Block
       ................................................ -->
  <!-- RD 20190619 [23912] Add -->
  <xsl:template name="UKTrader_Detail">
    <xsl:param name="pRepository-Actor" />

    <fo:table table-layout="fixed" width="{$gSection_width}"
              keep-together.within-page="always">

      <fo:table-column column-number="01" column-width="{$gSection_width}"/>

      <fo:table-body>
        <xsl:if test="position()=1">
          <fo:table-row font-size="{$gBlockSettings_Trader/column[@name='TraderDet']/@font-size}">
            <fo:table-cell padding="{$gBlockSettings_Trader/column[@name='TraderDet']/@padding}"
                           font-weight="{$gBlockSettings_Trader/column[@name='TraderDet']/@font-weight}"
                           text-align="{$gBlockSettings_Trader/column[@name='TraderDet']/@text-align}"
                           display-align="{$gBlockSettings_Trader/column[@name='TraderDet']/@display-align}">
              <fo:block>
                <xsl:call-template name="Debug_border-green"/>
                <!--<xsl:value-of select="'Trader to contact:'"/>-->
                <xsl:value-of select="$gBlockSettings_Trader/column[@name='TraderDet']/data/@resource"/>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </xsl:if>
        <fo:table-row font-size="{$gBlockSettings_Trader/column[@name='TraderDet']/@font-size}">
          <fo:table-cell padding="{$gBlockSettings_Trader/column[@name='TraderDet']/@padding}"
                         font-weight="{$gBlockSettings_Trader/column[@name='TraderDet']/@font-weight}"
                         text-align="{$gBlockSettings_Trader/column[@name='TraderDet']/@text-align}"
                         display-align="{$gBlockSettings_Trader/column[@name='TraderDet']/@display-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <!--<xsl:value-of select="concat($gcSpace,$gcSpace,$gcSpace,'Name',':',$gcSpace)"/>-->
              <xsl:value-of select="concat($gcSpace,$gcSpace,$gcSpace,$gBlockSettings_Trader/column[@name='TraderDet']/data[@name='Name']/@resource,$gcSpace)"/>
              <fo:inline font-weight="{$gBlockSettings_Trader/column[@name='TraderDet']/data[@name='Name']/@font-weight}">
                <xsl:value-of select="$pRepository-Actor/displayname"/>
              </fo:inline>
              <!--Phone-->
              <xsl:variable name="vPhone">
                <xsl:call-template name="DisplayLegalInfoModel_Element">
                  <xsl:with-param name="pResource" select="'INV-Phone'"/>
                  <xsl:with-param name="pData" select="$pRepository-Actor/telNumber"/>
                  <xsl:with-param name="pWithSeparator" select="true()"/>
                </xsl:call-template>
              </xsl:variable>
              <!--Telex-->
              <xsl:variable name="vTelex">
                <xsl:call-template name="DisplayLegalInfoModel_Element">
                  <xsl:with-param name="pResource" select="'INV-Telex'"/>
                  <xsl:with-param name="pData" select="$pRepository-Actor/telexNumber"/>
                  <xsl:with-param name="pWithSeparator" select="true()"/>
                </xsl:call-template>
              </xsl:variable>
              <!--Fax-->
              <xsl:variable name="vFax">
                <xsl:call-template name="DisplayLegalInfoModel_Element">
                  <xsl:with-param name="pResource" select="'INV-Fax'"/>
                  <xsl:with-param name="pData" select="$pRepository-Actor/faxNumber"/>
                  <xsl:with-param name="pWithSeparator" select="true()"/>
                </xsl:call-template>
              </xsl:variable>
              <!--Mail-->
              <xsl:variable name="vMail">
                <xsl:call-template name="DisplayLegalInfoModel_Element">
                  <xsl:with-param name="pResource" select="'INV-Mail'"/>
                  <xsl:with-param name="pData" select="$pRepository-Actor/email"/>
                  <xsl:with-param name="pWithSeparator" select="true()"/>
                </xsl:call-template>
              </xsl:variable>

              <xsl:value-of select="concat($vPhone,$vTelex,$vFax,$vMail)"/>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
      </fo:table-body>
    </fo:table>
  </xsl:template>

  <!-- .......................................................................... -->
  <!--              Tools: Display Data                                           -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- UKDisplayTitle_Market                            -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Market informations in Title Block      
       ................................................ -->
  <!-- FI 20161027 [22151] Modify -->
  <xsl:template name="UKDisplayTitle_Market">
    <xsl:param name="pRepository-market"/>
    <xsl:param name="pDataLength" select ="number('0')"/>

    <!-- FI 20161027 [22151] Call GetMarketIdentifier -->
    <xsl:variable name="vData">
      <xsl:call-template name ="GetMarketIdentifier">
        <xsl:with-param name="pRepository-market" select ="$pRepository-market"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:choose>
      <!--ISO10383_ALPHA4-->
      <xsl:when test="string-length($pRepository-market/ISO10383_ALPHA4) > 0">
        <xsl:variable name="vDataDet">
          <xsl:value-of select="concat(' (',$pRepository-market/ISO10383_ALPHA4,')')"/>
        </xsl:variable>
        <xsl:call-template name="DisplayData_Truncate">
          <xsl:with-param name="pData" select="$vData"/>
          <xsl:with-param name="pDataLength" select="$pDataLength - string-length($vDataDet)"/>
        </xsl:call-template>
        <fo:inline font-weight="{$gBlockSettings_MarketDC/column[@name='Market']/data[@name='det']/@font-weight}">
          <xsl:value-of select="$vDataDet"/>
        </fo:inline>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="DisplayData_Truncate">
          <xsl:with-param name="pData" select="$vData"/>
          <xsl:with-param name="pDataLength" select="$pDataLength"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!-- ................................................ -->
  <!-- GetMarketIdentifier                              -->
  <!-- ................................................ -->
  <!-- FI 20161027 [22151] Modify -->
  <xsl:template name ="GetMarketIdentifier">
    <xsl:param name="pRepository-market"/>

    <!--shortIdentifier-->
    <xsl:if test="string-length($pRepository-market/shortIdentifier) > 0">
      <xsl:value-of select="concat($pRepository-market/shortIdentifier,' - ')"/>
    </xsl:if>
    <!--acronym/identifier/displayname + city-->
    <xsl:choose>
      <xsl:when test="string-length($pRepository-market/acronym) > 0 and $pRepository-market/acronym != $pRepository-market/shortIdentifier">
        <xsl:value-of select="concat($pRepository-market/acronym,' ',$pRepository-market/city)"/>
      </xsl:when>
      <xsl:when test="string-length($pRepository-market/identifier) > 0 and $pRepository-market/identifier != $pRepository-market/shortIdentifier and $pRepository-market/identifier != $pRepository-market/ISO10383_ALPHA4">
        <xsl:value-of select="concat($pRepository-market/identifier,' ',$pRepository-market/city)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pRepository-market/displayname"/>
        <xsl:if test="contains($pRepository-market/displayname,$pRepository-market/city) = false()">
          <xsl:value-of select="concat(' ',$pRepository-market/city)"/>
        </xsl:if>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!-- ................................................ -->
  <!-- UKDisplayTitle_DC                                -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display DC informations in Title Block          
       ................................................ -->
  <!-- FI 20150126 [21825] Add pBiz-derivativeContract -->
  <!-- FI 20220908 [XXXXX] Add vAttrib, vDataDet modifié -->
  <xsl:template name="UKDisplayTitle_DC">
    <xsl:param name="pBiz_derivativeContract"/>
    <xsl:param name="pRepository-derivativeContract"/>
    <xsl:param name="pColumnSettings"/>
    
    <xsl:variable name="vDescription">
      <xsl:choose>
        <xsl:when test="string-length($pRepository-derivativeContract/description) > 0">
          <xsl:value-of select="$pRepository-derivativeContract/description"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$pRepository-derivativeContract/displayname"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vExtlDesc" select="$pRepository-derivativeContract/extlDesc"/>
    <xsl:variable name="vContractSymbol" select="$pRepository-derivativeContract/contractSymbol"/>
    <xsl:variable name="vCategory">
      <xsl:choose>
        <xsl:when test="$pRepository-derivativeContract/category='F'">
          <xsl:value-of select="'Future'"/>
        </xsl:when>
        <xsl:when test="$pRepository-derivativeContract/category='O'">
          <xsl:value-of select="'Option'"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vExerciseStyle">
      <xsl:if test="$pRepository-derivativeContract/category='O'">
        <xsl:choose>
          <xsl:when test="$pRepository-derivativeContract/exerciseStyle='0'">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'European'" />
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pRepository-derivativeContract/exerciseStyle='1'">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'American'" />
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pRepository-derivativeContract/exerciseStyle='2'">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Bermuda'" />
            </xsl:call-template>
          </xsl:when>
        </xsl:choose>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="vSettltMethod">
      <xsl:choose>
        <xsl:when test="$pRepository-derivativeContract/settltMethod='C'">
          <xsl:value-of select="'Cash'"/>
        </xsl:when>
        <xsl:when test="$pRepository-derivativeContract/settltMethod='P'">
          <xsl:value-of select="'Physical'"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vAttrib" select="$pRepository-derivativeContract/attrib"/>

    <xsl:variable name ="vResCurrency2">
      <xsl:call-template name="getSpheresTranslation">
        <xsl:with-param name="pResourceName" select="'Report-Currency2'" />
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vDataDesc">
      <xsl:choose>
        <xsl:when test="string-length($vExtlDesc) > 0">
          <xsl:value-of select="$vExtlDesc"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="vPart-before" select="concat($vContractSymbol,' ',substring($vCategory,1,3))"/>
          <xsl:variable name="vPart-after">
            <xsl:variable name="vPart-after1" select="substring($vSettltMethod,1,4)"/>
            <xsl:choose>
              <xsl:when test="$pRepository-derivativeContract/category='O'">
                <xsl:value-of select="concat($vPart-after1, ' ',substring($vExerciseStyle,1,2))"/>
              </xsl:when>
              <xsl:when test="$pRepository-derivativeContract/category='F'">
                <xsl:value-of select="$vPart-after1"/>
              </xsl:when>
            </xsl:choose>
          </xsl:variable>
          <xsl:variable name="vMidData1" select="substring-after($vDescription, $vPart-before)"/>
          <xsl:variable name="vMidData2" select="substring-before($vMidData1, $vPart-after)"/>
          <xsl:variable name="vMidData3" select="normalize-space($vMidData2)"/>
          <xsl:variable name="vMidData4">
            <xsl:choose>
              <xsl:when test="substring($vMidData3,1,1)='('">
                <xsl:value-of select="substring($vMidData3,2)"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$vMidData3"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:variable name="vMidData5">
            <xsl:variable name="vData" select="normalize-space($vMidData4)"/>
            <xsl:variable name="vLength" select="string-length($vData)"/>
            <xsl:choose>
              <xsl:when test="substring($vData,$vLength)=')'">
                <xsl:value-of select="substring($vData,1,$vLength - 1)"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$vData"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:value-of select="normalize-space($vMidData5)"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    
    <xsl:variable name="vData">
      <!--contractSymbol-->
      <xsl:if test="string-length($vContractSymbol) > 0">
        <xsl:value-of select="$vContractSymbol"/>
      </xsl:if>
      <!--description-->
      <xsl:if test="string-length($vContractSymbol) = 0 or $vDataDesc != $vContractSymbol">
        <xsl:if test="string-length($vContractSymbol) > 0 and string-length($vDataDesc) > 0">
          <xsl:value-of select="' - '"/>
        </xsl:if>
        <xsl:value-of select="$vDataDesc"/>
      </xsl:if>
    </xsl:variable>
    
    <xsl:variable name="vDataDet">
      <xsl:value-of select="concat(' - ', $vSettltMethod)"/>
      <xsl:if test="$pRepository-derivativeContract/category='O'">
        <xsl:value-of select="concat(' / ', $vExerciseStyle,' ', $vCategory)"/>
      </xsl:if>
      <xsl:if test="$pRepository-derivativeContract/category='F'">
         <xsl:value-of select="concat(' / ', $vCategory)"/>
      </xsl:if>
      <xsl:if test="string-length($vAttrib) > 0">
        <xsl:value-of select="concat(' / ','V', $vAttrib)"/>
      </xsl:if>
      <!-- FI 20150126 [21825] add devise -->
      <!--Devise et ContractSize-->
      <xsl:value-of select="' ('"/>
      <!-- Devise de prix -->
      <xsl:if test ="string-length($pRepository-derivativeContract/idC_Price/text())">
        <xsl:value-of select="translate($vResCurrency2,'.','')"/>
        <xsl:value-of select="': '"/>
        <xsl:value-of select="$pRepository-derivativeContract/idC_Price/text()"/>
        <!-- 4 espaces comme séparateur-->
        <xsl:value-of select="'&#160;&#160;&#160;&#160;'"/>
      </xsl:if>
      <!-- Contract Multiplier -->
      <xsl:call-template name="getSpheresTranslation">
        <xsl:with-param name="pResourceName" select="'Report-ContractSize'" />
      </xsl:call-template>
      <xsl:value-of select="': '"/>
      <xsl:call-template name="format-number">
        <xsl:with-param name="pAmount" select="$pBiz_derivativeContract/@contractMultiplier" />
        <xsl:with-param name="pAmountPattern" select="$numberPatternNoZero" />
      </xsl:call-template>
      <xsl:value-of select="')'"/>
    </xsl:variable>

    <xsl:call-template name="DisplayData_Truncate">
      <xsl:with-param name="pData" select="$vData"/>
      <xsl:with-param name="pDataLength" select="$pColumnSettings/@length - string-length($vDataDet)"/>
    </xsl:call-template>

    <fo:inline font-weight="{$pColumnSettings/data[@name='det']/@font-weight}">
      <xsl:value-of select="$vDataDet"/>
    </fo:inline>
  </xsl:template>

  <!-- ................................................ -->
  <!-- UKDisplayTitle_Ccy                               -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Ccy informations in Title Block          
       ................................................ -->
  <xsl:template name="UKDisplayTitle_Ccy">
    <xsl:param name="pRepository-currency"/>
    <xsl:param name="pColumnSettings"/>

    <xsl:variable name="vDescription">
      <xsl:choose>
        <xsl:when test="string-length($pRepository-currency/description) > 0">
          <xsl:value-of select="$pRepository-currency/description"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$pRepository-currency/displayname"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vCcy" select="$pRepository-currency/identifier/text()"/>

    <xsl:variable name="vData" select="$vCcy"/>
    <xsl:variable name="vDataDet" select="concat(' - ',$vDescription)"/>

    <xsl:call-template name="DisplayData_Truncate">
      <xsl:with-param name="pData" select="$vData"/>
      <xsl:with-param name="pDataLength" select="$pColumnSettings/@length - string-length($vDataDet)"/>
    </xsl:call-template>

    <fo:inline font-size="{$pColumnSettings/data[@name='det']/@font-size}"
               font-weight="{$pColumnSettings/data[@name='det']/@font-weight}">
      <xsl:value-of select="$vDataDet"/>
    </fo:inline>
  </xsl:template>

  <!-- ................................................ -->
  <!-- UKDisplayTitle_Book                              -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Book informations in Title Block      
       ................................................ -->
  <xsl:template name="UKDisplayTitle_Book">
    <xsl:param name="pRepository-book"/>
    <xsl:param name="pDataLength" select ="number('0')"/>

    <xsl:variable name="vData">
      <xsl:value-of select="$pRepository-book/identifier"/>
      <xsl:if test="$pRepository-book/identifier != $pRepository-book/displayname">
        <xsl:value-of select="concat(' - ',$pRepository-book/displayname)"/>
      </xsl:if>
    </xsl:variable>

    <xsl:call-template name="DisplayData_Truncate">
      <xsl:with-param name="pData" select="$vData"/>
      <xsl:with-param name="pDataLength" select="$pDataLength"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- UKDisplay_NoActivityMsg                          -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display "No data available"                     
       ................................................ -->
  <xsl:template name="UKDisplay_NoActivityMsg">
    <xsl:param name="pResource"/>

    <fo:block space-before="{$gBlock_space-before}"
              font-weight="normal"
              font-style="italic"
              font-size="{$gNoActivityMsg-font-size}">
      <xsl:if test="string-length($gNoActivityMsg-color) > 0">
        <xsl:attribute name="color">
          <xsl:value-of select="$gNoActivityMsg-color"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:call-template name="Debug_border-green"/>
      <xsl:value-of select="$pResource"/>
    </fo:block>
  </xsl:template>

  <!-- ................................................ -->
  <!-- UKDisplay_Legend                                 -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Report Legend                           
       ................................................ -->
  <xsl:template name="UKDisplay_Legend">
    <xsl:param name="pLegend"/>
    <xsl:param name="pResource"/>

    <fo:block keep-with-previous="always">
      <xsl:call-template name="Debug_border-green"/>
      <fo:inline font-size="{$gData_Det_font-size}" vertical-align="super">
        <xsl:value-of select="$pLegend"/>
      </fo:inline>
      <fo:inline>
        <xsl:value-of select="$gcSpace"/>
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="$pResource" />
        </xsl:call-template>
      </fo:inline>
    </fo:block>
  </xsl:template>

  <!-- ................................................ -->
  <!-- UKDisplay_Timestamp                              -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Timestamp                    
       ................................................ -->
  <xsl:template name="UKDisplay_Timestamp">
    <xsl:param name="pTimestamp"/>

    <fo:table-cell font-size="{$gBlockSettings_Data/column[@name='Date']/data[@name='Time']/@font-size}"
                   padding-top="{$gData_padding}"
                   padding-bottom="{$gData_padding}">
      <xsl:call-template name="Debug_border-green"/>
      <xsl:if test="$gIsDisplayTimestamp=true()">
        <fo:block>
          <xsl:value-of select="$pTimestamp"/>
        </fo:block>
      </xsl:if>
    </fo:table-cell>
  </xsl:template>

  <!-- ................................................ -->
  <!-- UKDisplayTradeDet_FirstFee                       -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Fee in the first line          
       ................................................ -->
  <xsl:template name="UKDisplayTradeDet_FirstFee">
    <xsl:param name="pFee"/>

    <xsl:choose>
      <xsl:when test="$pFee">
        <!--On parcourt tous les frais pour le tri, et on affiche le premier-->
        <xsl:for-each select="$pFee">
          <xsl:sort select="@paymentType"/>
          <xsl:sort select="@ccy"/>
          <xsl:sort select="@side=$gcDebit"/>
          <xsl:sort select="@side=$gcCredit"/>

          <xsl:if test="position()=1">
            <xsl:call-template name="UKDisplay_Fee"/>
          </xsl:if>
        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>
        <fo:table-cell number-columns-spanned="4">
          <xsl:call-template name="Debug_border-green"/>
        </fo:table-cell>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!-- ................................................ -->
  <!-- UKDisplayRow_TradeDet                            -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display the second row and next 
        second row can contains pTimestamps, pTrade2, pOtherPrice
       ................................................ -->
  <!-- FI 20150716 [20892] Add pOtherAmount2, pOtherAmountDet2 -->
  <!-- FI 20150827 [21287] Modify -->
  <!-- FI 20151019 [21317] add (Reecriture de UKDisplayRow_TradeDet => ajout paramètre pOtherPrice ) -->
  <!-- RD 20190619 [23912] Add pColor -->
  <xsl:template name="UKDisplayRow_TradeDet">
    <xsl:param name="pTimestamp"/>
    <!-- Autre prix (ie Accrued Interest rate sur les DSE) -->
    <xsl:param name="pOtherPrice"/>
    <!--pTrade2 contient 2 noeuds fo:table-cell (1 cell pour TrdNum et 1 cell pour Type) -->
    <xsl:param name="pTrade2"/>
    <!-- Représente les frais -->
    <xsl:param name="pFee"/>
    <!-- pOtherAmount2 Représente le montant à afficher en ligne n°2 -->
    <xsl:param name="pOtherAmount2"/>
    <!-- pOtherAmount3 Représente le montant à afficher en ligne n°3 -->
    <xsl:param name="pOtherAmount3"/>
    <!-- pOtherAmount4 Représente le montant à afficher en ligne n°4 -->
    <xsl:param name="pOtherAmount4"/>

    <xsl:param name="pQtyPattern"/>
    <xsl:param name="pIsQtyModel" select="false()"/>
    <xsl:param name="pColor"/>

    <xsl:choose>
      <!--Le premier row Fee a été déjà affiché sur la première ligne-->
      <xsl:when test="count(msxsl:node-set($pFee))>1">
        <!--Other Fee rows-->
        <xsl:for-each select="$pFee">
          <xsl:sort select="@paymentType"/>
          <xsl:sort select="@ccy"/>
          <xsl:sort select="@side=$gcDebit"/>
          <xsl:sort select="@side=$gcCredit"/>

          <!--Le premier a été déjà affiché sur la première ligne-->
          <xsl:if test="position()>1">
            <fo:table-row font-size="{$gData_font-size}"
                          font-weight="{$gData_font-weight}"
                          text-align="{$gData_text-align}"
                          display-align="{$gData_display-align}"
                          keep-with-previous="always"
                          keep-together.within-page="always">

              <xsl:if test="string-length($pColor) > 0">
                <xsl:call-template name="Display_AddAttribute-color">
                  <xsl:with-param name="pColor" select="$pColor"/>
                </xsl:call-template>
              </xsl:if>

              <xsl:choose>
                <xsl:when test="position()=2">
                  <xsl:choose>
                    <xsl:when test="$gIsDisplayTimestamp=true()">
                      <xsl:call-template name="UKDisplay_Timestamp">
                        <xsl:with-param name="pTimestamp" select="$pTimestamp"/>
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise>
                      <fo:table-cell number-columns-spanned="1">
                        <xsl:call-template name="Debug_border-green"/>
                      </fo:table-cell>
                    </xsl:otherwise>
                  </xsl:choose>
                  <xsl:if test="$pIsQtyModel">
                    <fo:table-cell>
                      <xsl:call-template name="Debug_border-green"/>
                    </fo:table-cell>
                  </xsl:if>
                  <xsl:choose>
                    <!-- FI 20150827 [21287] Use msxsl:node-set($pTrade2)/* -->
                    <xsl:when test="msxsl:node-set($pTrade2)/*">
                      <xsl:copy-of select="$pTrade2"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <fo:table-cell number-columns-spanned="2">
                        <xsl:call-template name="Debug_border-green"/>
                      </fo:table-cell>
                    </xsl:otherwise>
                  </xsl:choose>
                  <!-- FI 20180110 [XXXXX] number-columns-spanned à 5 -->
                  <!-- FI 20180126 [23748] number-columns-spanned à nouveau à 6 correction bug introduit le 20180110   -->
                  <fo:table-cell number-columns-spanned="6"/>
                  <xsl:choose>
                    <!-- FI 20151019 [21317] Add pOtherPrice-->
                    <xsl:when test="string-length($pOtherPrice)>0">
                      <fo:table-cell text-align="{$gData_Number_text-align}">
                        <fo:block>
                          <xsl:copy-of select="$pOtherPrice"/>
                        </fo:block>
                      </fo:table-cell>
                    </xsl:when>
                    <xsl:otherwise>
                      <fo:table-cell>
                        <xsl:call-template name="Debug_border-green"/>
                      </fo:table-cell>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:if test="$pIsQtyModel">
                    <fo:table-cell>
                      <xsl:call-template name="Debug_border-green"/>
                    </fo:table-cell>
                  </xsl:if>
                  <fo:table-cell number-columns-spanned="10">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <xsl:call-template name="UKDisplay_Fee"/>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <!-- FI 20150716 [20892] Call UKDisplay_OtherAmount-->
              <xsl:choose>
                <!-- FI 20170110 [XXXXX] add test sur $pOtherAmount2 -->
                <xsl:when test="position()=2 and $pOtherAmount2">
                  <xsl:call-template name ="UKDisplay_OtherAmount" >
                    <xsl:with-param name ="pOtherAmount" select ="$pOtherAmount2"/>
                    <xsl:with-param name ="pQtyPattern" select ="$pQtyPattern" />
                  </xsl:call-template>
                </xsl:when>
                <!-- FI 20170110 [XXXXX] add test sur $pOtherAmount3 -->
                <xsl:when test="position()=3 and $pOtherAmount3">
                  <xsl:call-template name ="UKDisplay_OtherAmount" >
                    <xsl:with-param name ="pOtherAmount" select ="$pOtherAmount3"/>
                    <xsl:with-param name ="pQtyPattern" select ="$pQtyPattern" />
                  </xsl:call-template>
                </xsl:when>
                <!-- FI 20170110 [XXXXX] add test sur $pOtherAmount4 -->
                <xsl:when test="position()=4 and $pOtherAmount4">
                  <xsl:call-template name ="UKDisplay_OtherAmount" >
                    <xsl:with-param name ="pOtherAmount" select ="$pOtherAmount4"/>
                    <xsl:with-param name ="pQtyPattern" select ="$pQtyPattern" />
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name ="UKDisplay_OtherAmount" >
                    <xsl:with-param name ="pOtherAmount" select ="null"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </fo:table-row>
          </xsl:if>
        </xsl:for-each>
        <!--FI 20151125 [21590] Ajout d'une ligne avec le montant 2 -->
        <xsl:if test ="count(msxsl:node-set($pFee)) &lt;=2 and $pOtherAmount3">
          <xsl:call-template name ="UKDisplay_RowOtherAmount">
            <xsl:with-param name ="pOtherAmount" select ="$pOtherAmount3"/>
            <xsl:with-param name ="pQtyPattern" select ="$pQtyPattern" />
            <xsl:with-param name ="pIsQtyModel" select ="$pIsQtyModel"/>
            <xsl:with-param name ="pColor" select="$pColor"/>
          </xsl:call-template>
        </xsl:if>
        <!--FI 20151125 [21590] Ajout d'une ligne avec le montant 3 -->
        <xsl:if test ="count(msxsl:node-set($pFee))&lt;=3 and $pOtherAmount4">
          <xsl:call-template name ="UKDisplay_RowOtherAmount">
            <xsl:with-param name ="pOtherAmount" select ="$pOtherAmount4"/>
            <xsl:with-param name ="pQtyPattern" select ="$pQtyPattern" />
            <xsl:with-param name ="pIsQtyModel" select ="$pIsQtyModel"/>
            <xsl:with-param name ="pColor" select="$pColor"/>
          </xsl:call-template>
        </xsl:if>
      </xsl:when>

      <xsl:when test ="($gIsDisplayTimestamp=true()) or (msxsl:node-set($pTrade2)/*) or (string-length($pOtherPrice)>0)">
        <fo:table-row font-size="{$gData_font-size}"
                      font-weight="{$gData_font-weight}"
                      text-align="{$gData_text-align}"
                      display-align="{$gData_display-align}"
                      keep-with-previous="always"
                      keep-together.within-page="always">

          <xsl:if test="string-length($pColor) > 0">
            <xsl:call-template name="Display_AddAttribute-color">
              <xsl:with-param name="pColor" select="$pColor"/>
            </xsl:call-template>
          </xsl:if>

          <xsl:choose>
            <xsl:when test="$gIsDisplayTimestamp=true()">
              <xsl:call-template name="UKDisplay_Timestamp">
                <xsl:with-param name="pTimestamp" select="$pTimestamp"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell number-columns-spanned="1">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:if test="$pIsQtyModel">
            <fo:table-cell>
              <xsl:call-template name="Debug_border-green"/>
            </fo:table-cell>
          </xsl:if>
          <xsl:choose>
            <!-- FI 20150827 [21287] Use msxsl:node-set($pTrade2)/* -->
            <xsl:when test="msxsl:node-set($pTrade2)/*">
              <xsl:copy-of select="$pTrade2"/>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell number-columns-spanned="2">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>
          <fo:table-cell number-columns-spanned="6"/>
          <xsl:choose>
            <!-- FI 20151019 [21317] Add pOtherPrice-->
            <xsl:when test="string-length($pOtherPrice)>0">
              <fo:table-cell text-align="{$gData_Number_text-align}">
                <fo:block>
                  <xsl:copy-of select="$pOtherPrice"/>
                </fo:block>
              </fo:table-cell>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>

          <fo:table-cell>
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>

          <fo:table-cell number-columns-spanned="4">
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <fo:table-cell>
            <xsl:call-template name="Debug_border-green"/>
          </fo:table-cell>
          <xsl:choose>
            <!-- FI 20180126 [23748] add test sur $pOtherAmount2 -->
            <xsl:when test="$pOtherAmount2 and $pOtherAmount2/*[1] and string-length($pOtherAmount2/@det) > 0">
              <fo:table-cell font-size="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-size}"
                             font-weight="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-weight}"
                             padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
                <fo:block>
                  <xsl:call-template name="Debug_border-green"/>
                  <xsl:call-template name="DisplayType_Short">
                    <xsl:with-param name="pType" select="$pOtherAmount2/@det"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:choose>
            <xsl:when test="$pOtherAmount2">
              <xsl:choose>
                <xsl:when test="$pOtherAmount2/@isQty = '1'">
                  <xsl:call-template name="UKDisplay_Quantity">
                    <xsl:with-param name="pQuantity" select="$pOtherAmount2/*[1]"/>
                    <xsl:with-param name="pQtyPattern" select="$pQtyPattern"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="UKDisplay_Amount">
                    <xsl:with-param name="pAmount" select="$pOtherAmount2/*[1]"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:otherwise>
              <fo:table-cell number-columns-spanned="3">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
            </xsl:otherwise>
          </xsl:choose>
        </fo:table-row>
        <xsl:if test ="$pOtherAmount3">
          <xsl:call-template name ="UKDisplay_RowOtherAmount">
            <xsl:with-param name ="pOtherAmount" select ="$pOtherAmount3"/>
            <xsl:with-param name ="pQtyPattern" select ="$pQtyPattern" />
            <xsl:with-param name ="pIsQtyModel" select ="$pIsQtyModel"/>
            <xsl:with-param name ="pColor" select="$pColor"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:if test ="$pOtherAmount4">
          <xsl:call-template name ="UKDisplay_RowOtherAmount">
            <xsl:with-param name ="pOtherAmount" select ="$pOtherAmount4"/>
            <xsl:with-param name ="pQtyPattern" select ="$pQtyPattern" />
            <xsl:with-param name ="pIsQtyModel" select ="$pIsQtyModel"/>
            <xsl:with-param name ="pColor" select="$pColor"/>
          </xsl:call-template>
        </xsl:if>
      </xsl:when>

      <!-- FI 20150716 [20892] Call UKDisplay_RowOtherAmount for $pOtherAmount or $pOtherAmount2-->
      <xsl:when test="$pOtherAmount2 or $pOtherAmount3 or $pOtherAmount4">
        <xsl:if test ="$pOtherAmount2">
          <xsl:call-template name ="UKDisplay_RowOtherAmount">
            <xsl:with-param name ="pOtherAmount" select ="$pOtherAmount2"/>
            <xsl:with-param name ="pQtyPattern" select ="$pQtyPattern" />
            <xsl:with-param name ="pIsQtyModel" select ="$pIsQtyModel"/>
            <xsl:with-param name ="pColor" select="$pColor"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:if test ="$pOtherAmount3">
          <xsl:call-template name ="UKDisplay_RowOtherAmount">
            <xsl:with-param name ="pOtherAmount" select ="$pOtherAmount3"/>
            <xsl:with-param name ="pQtyPattern" select ="$pQtyPattern" />
            <xsl:with-param name ="pIsQtyModel" select ="$pIsQtyModel"/>
            <xsl:with-param name ="pColor" select="$pColor"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:if test ="$pOtherAmount4">
          <xsl:call-template name ="UKDisplay_RowOtherAmount">
            <xsl:with-param name ="pOtherAmount" select ="$pOtherAmount4"/>
            <xsl:with-param name ="pQtyPattern" select ="$pQtyPattern" />
            <xsl:with-param name ="pIsQtyModel" select ="$pIsQtyModel"/>
            <xsl:with-param name ="pColor" select="$pColor"/>
          </xsl:call-template>
        </xsl:if>
      </xsl:when>
    </xsl:choose>
  </xsl:template>


  <!-- ................................................ -->
  <!-- UKDisplay_OtherAmount                            -->
  <!-- ................................................ -->
  <!-- Summary :     
       Consomme 4 colonnes 
       ................................................ -->
  <!-- FI 20150716 [20892] Add UKDisplay_OtherAmount -->
  <xsl:template name ="UKDisplay_OtherAmount">
    <xsl:param name="pOtherAmount"/>
    <xsl:param name="pQtyPattern"/>

    <xsl:choose>
      <!-- SI/PL 20180719 [24095] Add test msxsl:node-set($pOtherAmount)/* (see also [23872]) -->
      <!-- <xsl:when test="$pOtherAmount/*[1] and string-length($pOtherAmount/@det) > 0"> -->
      <xsl:when test="msxsl:node-set($pOtherAmount)/* and $pOtherAmount/*[1] and string-length($pOtherAmount/@det) > 0">
        <fo:table-cell font-size="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-size}"
                       font-weight="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-weight}"
                       padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
          <fo:block>
            <xsl:call-template name="Debug_border-green"/>
            <xsl:call-template name="DisplayType_Short">
              <xsl:with-param name="pType" select="$pOtherAmount/@det"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
      </xsl:when>
      <xsl:otherwise>
        <fo:table-cell>
          <xsl:call-template name="Debug_border-green"/>
        </fo:table-cell>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:choose>
      <xsl:when test="$pOtherAmount">
        <xsl:choose>
          <xsl:when test="$pOtherAmount/@isQty = '1'">
            <xsl:call-template name="UKDisplay_Quantity">
              <xsl:with-param name="pQuantity" select="$pOtherAmount/*[1]"/>
              <xsl:with-param name="pQtyPattern" select="$pQtyPattern"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="UKDisplay_Amount">
              <xsl:with-param name="pAmount" select="$pOtherAmount/*[1]"/>
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <fo:table-cell number-columns-spanned="3">
          <xsl:call-template name="Debug_border-green"/>
        </fo:table-cell>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!-- ................................................ -->
  <!-- UKDisplay_RowOtherAmount                         -->
  <!-- ................................................ -->
  <!-- Summary :                                        
       ................................................ -->
  <!-- FI 20150716 [20892] Add UKDisplay_RowOtherAmount -->
  <!-- RD 20190619 [23912] Add pColor -->
  <xsl:template name ="UKDisplay_RowOtherAmount">
    <xsl:param name="pOtherAmount"/>
    <xsl:param name="pQtyPattern"/>
    <xsl:param name="pIsQtyModel"/>
    <xsl:param name="pColor"/>

    <fo:table-row font-size="{$gData_font-size}"
                  font-weight="{$gData_font-weight}"
                  text-align="{$gData_text-align}"
                  display-align="{$gData_display-align}"
                  keep-with-previous="always"
                  keep-together.within-page="always">

      <xsl:if test="string-length($pColor) > 0">
        <xsl:call-template name="Display_AddAttribute-color">
          <xsl:with-param name="pColor" select="$pColor"/>
        </xsl:call-template>
      </xsl:if>

      <fo:table-cell number-columns-spanned="1">
        <xsl:call-template name="Debug_border-green"/>
      </fo:table-cell>
      <xsl:if test="$pIsQtyModel">
        <fo:table-cell>
          <xsl:call-template name="Debug_border-green"/>
        </fo:table-cell>
      </xsl:if>
      <fo:table-cell number-columns-spanned="14">
        <xsl:call-template name="Debug_border-green"/>
      </fo:table-cell>
      <fo:table-cell>
        <xsl:call-template name="Debug_border-green"/>
      </fo:table-cell>
      <xsl:call-template name ="UKDisplay_OtherAmount" >
        <xsl:with-param name ="pOtherAmount" select ="$pOtherAmount"/>
        <xsl:with-param name ="pQtyPattern" select ="$pQtyPattern" />
      </xsl:call-template>
    </fo:table-row>
  </xsl:template>


  <!-- ................................................ -->
  <!-- UKDisplay_Fee                                    -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display PaymentType and Amount in one cell                   
       ................................................ -->
  <xsl:template name="UKDisplay_Fee">
    <fo:table-cell font-size="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-size}"
                   font-weight="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-weight}"
                   padding-top="{$gData_padding}" padding-bottom="{$gData_padding}">
      <fo:block>
        <xsl:call-template name="Debug_border-green"/>
        <xsl:call-template name="DisplayType_Short">
          <xsl:with-param name="pType" select="current()/@paymentType"/>
        </xsl:call-template>
      </fo:block>
    </fo:table-cell>
    <xsl:call-template name="UKDisplay_Amount">
      <xsl:with-param name="pAmount" select="current()"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- UKDisplay_Amount                                 -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Amount on 3 columns (ccy, amount, side)                    
       ................................................ -->
  <xsl:template name="UKDisplay_Amount">
    <xsl:param name="pAmount"/>
    <xsl:param name="pCcy"/>
    <xsl:param name="pWithSide" select="true()"/>
    <xsl:param name="pAmountPattern"/>

    <xsl:variable name="vCcy">
      <xsl:call-template name="GetAmount-ccy">
        <xsl:with-param name="pAmount" select="$pAmount"/>
        <xsl:with-param name="pCcy" select="$pCcy"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vAmount">
      <xsl:call-template name="GetAmount-amt">
        <xsl:with-param name="pAmount" select="$pAmount"/>
        <xsl:with-param name="pCcy" select="$vCcy"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSide">
      <xsl:call-template name="GetAmount-side">
        <xsl:with-param name="pAmount" select="$vAmount"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="string-length($vAmount) > 0">
        <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}"
                       font-size="{$gBlockSettings_Data/column[@name='Ccy']/@font-size}">
          <fo:block>
            <xsl:call-template name="Debug_border-green"/>
            <xsl:call-template name="DisplayData_Amounts">
              <xsl:with-param name="pDataName" select="'ccy'"/>
              <xsl:with-param name="pCcy" select="$vCcy"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}"
                       text-align="{$gData_Number_text-align}">
          <xsl:if test="contains(',Default,RightDRCR,RightDR,RightMinusPlus,RightMinus,',concat(',',$gAmount-format,',')) = false()
                  or $pWithSide = false()">
            <xsl:attribute name="number-columns-spanned">
              <xsl:value-of select="'2'"/>
            </xsl:attribute>
          </xsl:if>
          <fo:block>
            <xsl:call-template name="Debug_border-green"/>
            <xsl:variable name="vFormattedAmount">
              <xsl:call-template name="DisplayData_Amounts">
                <xsl:with-param name="pDataName" select="'amount'"/>
                <xsl:with-param name="pAmount" select="$vAmount"/>
                <xsl:with-param name="pCcy" select="$vCcy"/>
                <xsl:with-param name="pAmountPattern" select="$pAmountPattern" />
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="vAmountSettingColumn" select="$gBlockSettings_Data/column[@name='Amount']"/>
            <xsl:variable name="vAmountFontSize">
              <xsl:choose>
                <xsl:when test="$vAmountSettingColumn/style[@name='S1']/@length >= string-length($vFormattedAmount)">
                  <xsl:value-of select="$vAmountSettingColumn/style[@name='S1']/@font-size"/>
                </xsl:when>
                <xsl:when test="$vAmountSettingColumn/style[@name='S2']/@length >= string-length($vFormattedAmount)">
                  <xsl:value-of select="$vAmountSettingColumn/style[@name='S2']/@font-size"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$vAmountSettingColumn/style[@name='S3']/@font-size"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:variable name="vAmountToDisplay">
              <xsl:choose>
                <xsl:when test="$gcIsTestMode=true()">
                  <xsl:choose>
                    <xsl:when test="$vAmountSettingColumn/style[@name='S1']/@length >= string-length($vFormattedAmount)">
                      <xsl:value-of select="$vAmountSettingColumn/style[@name='S1']/@description"/>
                    </xsl:when>
                    <xsl:when test="$vAmountSettingColumn/style[@name='S2']/@length >= string-length($vFormattedAmount)">
                      <xsl:value-of select="$vAmountSettingColumn/style[@name='S2']/@description"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="$vAmountSettingColumn/style[@name='S3']/@description"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$vFormattedAmount"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>

            <xsl:choose>
              <xsl:when test="$pWithSide = false()">
                <fo:inline font-size="{$vAmountFontSize}">
                  <xsl:value-of select="$vAmountToDisplay"/>
                </fo:inline>
              </xsl:when>
              <xsl:when test="contains(',LeftDRCR,LeftDR,LeftMinusPlus,LeftMinus,',concat(',',$gAmount-format,','))">
                <fo:inline font-size="{$gData_Det_font-size}">
                  <xsl:value-of select="concat($vSide,$gcSpace)"/>
                </fo:inline>
                <fo:inline font-size="{$vAmountFontSize}">
                  <xsl:value-of select="$vAmountToDisplay"/>
                </fo:inline>
              </xsl:when>
              <xsl:when test="$gAmount-format='Parenthesis' and string-length($vSide) > 0">
                <fo:inline font-size="{$vAmountFontSize}">
                  <xsl:value-of select="concat('(',$vAmountToDisplay,')')"/>
                </fo:inline>
              </xsl:when>
              <xsl:otherwise>
                <fo:inline font-size="{$vAmountFontSize}">
                  <xsl:value-of select="$vAmountToDisplay"/>
                </fo:inline>
              </xsl:otherwise>
            </xsl:choose>
          </fo:block>
        </fo:table-cell>
        <xsl:if test="contains(',Default,RightDRCR,RightDR,RightMinusPlus,RightMinus,',concat(',',$gAmount-format,','))
                  and $pWithSide = true()">
          <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}"
                         font-size="{$gData_Det_font-size}"
                         text-align="{$gBlockSettings_Data/column[@name='DrCr']/@text-align}">
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$vSide"/>
            </fo:block>
          </fo:table-cell>
        </xsl:if>
      </xsl:when>
      <xsl:otherwise>
        <fo:table-cell number-columns-spanned="3">
          <xsl:call-template name="Debug_border-green"/>
        </fo:table-cell>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- ................................................ -->
  <!-- UKDisplay_Quantity                               -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Quantity on 3 columns (Quantity and qtyUnit)
       ................................................ -->
  <xsl:template name="UKDisplay_Quantity">
    <xsl:param name="pQuantity"/>
    <xsl:param name="pQtyUnit"/>
    <xsl:param name="pQtyPattern"/>

    <xsl:variable name="vQtyUnit">
      <xsl:call-template name="GetQuantity-qtyUnit">
        <xsl:with-param name="pQuantity" select="$pQuantity"/>
        <xsl:with-param name="pQtyUnit" select="$pQtyUnit"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vQuantity">
      <xsl:call-template name="GetQuantity-fmtQty">
        <xsl:with-param name="pQuantity" select="$pQuantity"/>
        <xsl:with-param name="pQtyUnit" select="$vQtyUnit"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="string-length($vQuantity) > 0">
        <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}"
                       text-align="{$gData_Number_text-align}"
                       number-columns-spanned="3">

          <fo:block>
            <xsl:call-template name="Debug_border-green"/>
            <xsl:variable name="vFormattedAmount">
              <xsl:call-template name="DisplayFmtNumber">
                <xsl:with-param name="pFmtNumber" select="$vQuantity"/>
                <xsl:with-param name="pPattern" select="$pQtyPattern" />
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="vAmountSettingColumn" select="$gBlockSettings_Data/column[@name='Amount']"/>
            <xsl:variable name="vAmountFontSize">
              <xsl:choose>
                <xsl:when test="$vAmountSettingColumn/style[@name='S1']/@length >= string-length($vFormattedAmount)">
                  <xsl:value-of select="$vAmountSettingColumn/style[@name='S1']/@font-size"/>
                </xsl:when>
                <xsl:when test="$vAmountSettingColumn/style[@name='S2']/@length >= string-length($vFormattedAmount)">
                  <xsl:value-of select="$vAmountSettingColumn/style[@name='S2']/@font-size"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$vAmountSettingColumn/style[@name='S3']/@font-size"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:variable name="vAmountToDisplay">
              <xsl:choose>
                <xsl:when test="$gcIsTestMode=true()">
                  <xsl:choose>
                    <xsl:when test="$vAmountSettingColumn/style[@name='S1']/@length >= string-length($vFormattedAmount)">
                      <xsl:value-of select="$vAmountSettingColumn/style[@name='S1']/@description"/>
                    </xsl:when>
                    <xsl:when test="$vAmountSettingColumn/style[@name='S2']/@length >= string-length($vFormattedAmount)">
                      <xsl:value-of select="$vAmountSettingColumn/style[@name='S2']/@description"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="$vAmountSettingColumn/style[@name='S3']/@description"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$vFormattedAmount"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>

            <fo:inline font-size="{$vAmountFontSize}">
              <xsl:value-of select="$vAmountToDisplay"/>
            </fo:inline>
            <fo:inline font-size="{$gData_Det_font-size}">
              <xsl:value-of select="concat($gcSpace,$vQtyUnit)"/>
            </fo:inline>
          </fo:block>
        </fo:table-cell>
      </xsl:when>
      <xsl:otherwise>
        <fo:table-cell number-columns-spanned="3">
          <xsl:call-template name="Debug_border-green"/>
        </fo:table-cell>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- ................................................ -->
  <!-- UKDisplay_SubTotal_AvgPx                               -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        
       ................................................ -->
  <!-- RD 20190619 [23912] Add pWithRes -->
  <xsl:template name="UKDisplay_SubTotal_AvgPx">
    <xsl:param name="pDataName"/>
    <xsl:param name="pSubTotal"/>
    <xsl:param name="pNumberPattern"/>
    <xsl:param name="pIsPos" select="true()"/>
    <xsl:param name="pWithRes" select="true()"/>
    <xsl:param name="pQtyModel" select="'Date2Qty'"/>

    <xsl:variable name="vIsBuySell" select="$pQtyModel='Date2Qty'"/>
    <xsl:variable name="vIsBuy" select="$pQtyModel='Date2Qty' or $pDataName='AvgBuy'"/>

    <xsl:choose>
      <xsl:when test="$vIsBuySell and $pSubTotal/long/@fmtQty > 0 and $pSubTotal/short/@fmtQty > 0">
        <xsl:if test="$pWithRes=true()">
          <xsl:choose>
            <xsl:when test="$pIsPos=true()">
              <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='AvgPx']/data[@name='longShort']/@resource"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='AvgPx']/data[@name='buySell']/@resource"/>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:value-of select="$gcSpace"/>
        </xsl:if>
        <xsl:call-template name="DisplayData_SubTotal">
          <xsl:with-param name="pDataName" select="'FmtLongAvgPx'"/>
          <xsl:with-param name="pSubTotal" select="$pSubTotal"/>
          <xsl:with-param name="pNumberPattern" select="$pNumberPattern" />
        </xsl:call-template>
        <xsl:value-of select="concat($gcSpace,'/',$gcSpace)"/>
        <xsl:call-template name="DisplayData_SubTotal">
          <xsl:with-param name="pDataName" select="'FmtShortAvgPx'"/>
          <xsl:with-param name="pSubTotal" select="$pSubTotal"/>
          <xsl:with-param name="pNumberPattern" select="$pNumberPattern" />
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$vIsBuy and $pSubTotal/long/@fmtQty > 0">
        <xsl:if test="$pWithRes=true()">
          <xsl:choose>
            <xsl:when test="$pIsPos=true()">
              <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='AvgPx']/data[@name='long']/@resource"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='AvgPx']/data[@name='buy']/@resource"/>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:value-of select="$gcSpace"/>
        </xsl:if>
        <xsl:call-template name="DisplayData_SubTotal">
          <xsl:with-param name="pDataName" select="'FmtLongAvgPx'"/>
          <xsl:with-param name="pSubTotal" select="$pSubTotal"/>
          <xsl:with-param name="pNumberPattern" select="$pNumberPattern" />
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pSubTotal/short/@fmtQty > 0">
        <xsl:if test="$pWithRes=true()">
          <xsl:choose>
            <xsl:when test="$pIsPos=true()">
              <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='AvgPx']/data[@name='short']/@resource"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='AvgPx']/data[@name='sell']/@resource"/>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:value-of select="$gcSpace"/>
        </xsl:if>
        <xsl:call-template name="DisplayData_SubTotal">
          <xsl:with-param name="pDataName" select="'FmtShortAvgPx'"/>
          <xsl:with-param name="pSubTotal" select="$pSubTotal"/>
          <xsl:with-param name="pNumberPattern" select="$pNumberPattern" />
        </xsl:call-template>
      </xsl:when>
    </xsl:choose>

  </xsl:template>

  <!-- ................................................ -->
  <!-- UKDisplay_SubTotal_OneFee                        -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display SubTotal Fee in the first line          
       ................................................ -->
  <xsl:template name="UKDisplay_SubTotal_OneFee">
    <xsl:param name="pFee"/>
    <xsl:param name="pFeeNumber" select="'1'"/>
    <xsl:choose>
      <!--On parcourt tous les frais pour le tri, et on affiche le premier-->
      <!-- FI 20150729 [20892] Mise en place du test sur le count 
           Pour corrigé le cas exemple suivant 
           SI pFeeNumber vaut 2 et il n'existe 1 frais => pas de cellule 
      -->
      <xsl:when test="count(msxsl:node-set($pFee))>= $pFeeNumber">
        <!--<xsl:when test="$pFee">-->
        <xsl:for-each select="$pFee">
          <xsl:sort select="@paymentType"/>
          <xsl:sort select="@ccy"/>
          <xsl:sort select="@side=$gcDebit"/>
          <xsl:sort select="@side=$gcCredit"/>

          <xsl:if test="position()=$pFeeNumber">
            <xsl:call-template name="UKDisplay_SubTotal_Fee"/>
          </xsl:if>
        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>
        <fo:table-cell number-columns-spanned="4">
          <xsl:call-template name="Debug_border-green"/>
        </fo:table-cell>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- ................................................ -->
  <!-- UKDisplayRow_SubTotalDet                         -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Affiche les lignes Subtotal à partir de la ligne n°2          
       ................................................ -->
  <xsl:template name="UKDisplayRow_SubTotalDet">
    <!-- Représente les frais -->
    <xsl:param name="pFee"/>
    <!-- Représente l'index de début à partir du quel l'élément pFee doit être pris en considération (numéroration commence par 1)
    Ce paramètre ne fois jamais être inférieur à 2 (valeur par défaut 2)
    -->
    <xsl:param name="pFeeStartNumber" select="2"/>
    <!-- Représent le montant affiché en ligne 2  (un élément avec les attributs det,amt,ccy,withside) -->
    <xsl:param name="pOtherAmount2"/>
    <!-- Représent le montant affiché en ligne 3  (un élément avec les attributs det,amt,ccy,withside) -->
    <xsl:param name="pOtherAmount3"/>
    <!-- Représent le montant affiché en ligne 4  (un élément avec les attributs det,amt,ccy,withside) -->
    <xsl:param name="pOtherAmount4"/>
    <xsl:param name="pColumns-spanned" select="'11'" />
    <xsl:param name="pQtyPattern"/>

    <xsl:choose>
      <!--Les autres row Fee ont été déjà affichés sur les premières lignes-->
      <xsl:when test="count(msxsl:node-set($pFee))>= $pFeeStartNumber">
        <!--Other Subtotal Fee rows-->
        <xsl:for-each select="$pFee">
          <xsl:sort select="@paymentType"/>
          <xsl:sort select="@ccy"/>
          <xsl:sort select="@side=$gcDebit"/>
          <xsl:sort select="@side=$gcCredit"/>
          <xsl:if test="position()>= $pFeeStartNumber">
            <fo:table-row font-size="{$gData_font-size}"
                          font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                          text-align="{$gData_text-align}"
                          display-align="{$gBlockSettings_Data/subtotal/@display-align}"
                          keep-with-previous="always">

              <fo:table-cell number-columns-spanned="{$pColumns-spanned}">
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <xsl:call-template name="UKDisplay_SubTotal_Fee"/>
              <fo:table-cell>
                <xsl:call-template name="Debug_border-green"/>
              </fo:table-cell>
              <xsl:choose>
                <xsl:when test="position()=2">
                  <!-- FI 20150716 [20892] call UKDisplay_SubTotal_OtherAmount -->
                  <xsl:call-template name ="UKDisplay_SubTotal_OtherAmount">
                    <xsl:with-param name ="pOtherAmount" select ="$pOtherAmount2"/>
                    <xsl:with-param name ="pQtyPattern" select ="$pQtyPattern" />
                  </xsl:call-template>
                </xsl:when>
                <xsl:when test="position()=3">
                  <!-- FI 20150716 [20892] call UKDisplay_SubTotal_OtherAmount -->
                  <xsl:call-template name ="UKDisplay_SubTotal_OtherAmount">
                    <xsl:with-param name ="pOtherAmount" select ="$pOtherAmount3"/>
                    <xsl:with-param name ="pQtyPattern" select ="$pQtyPattern" />
                  </xsl:call-template>
                </xsl:when>
                <xsl:when test="position()=4">
                  <!-- FI 20150716 [20892] call UKDisplay_SubTotal_OtherAmount -->
                  <xsl:call-template name ="UKDisplay_SubTotal_OtherAmount">
                    <xsl:with-param name ="pOtherAmount" select ="$pOtherAmount4"/>
                    <xsl:with-param name ="pQtyPattern" select ="$pQtyPattern" />
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell number-columns-spanned="4">
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>
            </fo:table-row>
          </xsl:if>
        </xsl:for-each>

        <xsl:if test ="pFeeStartNumber &lt;=2 and count(msxsl:node-set($pFee)) &lt;2 and $pOtherAmount2">
          <xsl:call-template name ="UKDisplay_SubTotal_RowOtherAmount">
            <xsl:with-param name="pOtherAmount" select ="$pOtherAmount2"/>
            <xsl:with-param name="pColumns-spanned" select="$pColumns-spanned" />
            <xsl:with-param name="pQtyPattern" select="$pQtyPattern" />
          </xsl:call-template>
        </xsl:if>

        <xsl:if test ="$pFeeStartNumber &lt;=3 and count(msxsl:node-set($pFee))&lt;3 and $pOtherAmount3">
          <xsl:call-template name ="UKDisplay_SubTotal_RowOtherAmount">
            <xsl:with-param name="pOtherAmount" select ="$pOtherAmount3"/>
            <xsl:with-param name="pColumns-spanned" select="$pColumns-spanned" />
            <xsl:with-param name="pQtyPattern" select="$pQtyPattern" />
          </xsl:call-template>
        </xsl:if>

        <xsl:if test ="$pFeeStartNumber &lt;=4 and count(msxsl:node-set($pFee))&lt;4 and $pOtherAmount4">
          <xsl:call-template name ="UKDisplay_SubTotal_RowOtherAmount">
            <xsl:with-param name="pOtherAmount" select ="$pOtherAmount4"/>
            <xsl:with-param name="pColumns-spanned" select="$pColumns-spanned" />
            <xsl:with-param name="pQtyPattern" select="$pQtyPattern" />
          </xsl:call-template>
        </xsl:if>

      </xsl:when>

      <!-- Si aucun frais à afficher Affichage de $pOtherAmount2 s'il existe, puis affichage de $pOtherAmount3 s'il existe, etc. -->
      <xsl:when test="$pOtherAmount2 or $pOtherAmount3 or $pOtherAmount4">
        <xsl:if test ="$pFeeStartNumber &lt;=2 and $pOtherAmount2">
          <xsl:call-template name ="UKDisplay_SubTotal_RowOtherAmount">
            <xsl:with-param name="pOtherAmount" select ="$pOtherAmount2"/>
            <xsl:with-param name="pColumns-spanned" select="$pColumns-spanned" />
            <xsl:with-param name="pQtyPattern" select="$pQtyPattern" />
          </xsl:call-template>
        </xsl:if>
        <xsl:if test ="$pFeeStartNumber &lt;=3 and $pOtherAmount3">
          <xsl:call-template name ="UKDisplay_SubTotal_RowOtherAmount">
            <xsl:with-param name="pOtherAmount" select ="$pOtherAmount3"/>
            <xsl:with-param name="pColumns-spanned" select="$pColumns-spanned" />
            <xsl:with-param name="pQtyPattern" select="$pQtyPattern" />
          </xsl:call-template>
        </xsl:if>
        <xsl:if test ="$pFeeStartNumber &lt;=4 and $pOtherAmount4">
          <xsl:call-template name ="UKDisplay_SubTotal_RowOtherAmount">
            <xsl:with-param name="pOtherAmount" select ="$pOtherAmount4"/>
            <xsl:with-param name="pColumns-spanned" select="$pColumns-spanned" />
            <xsl:with-param name="pQtyPattern" select="$pQtyPattern" />
          </xsl:call-template>
        </xsl:if>
      </xsl:when>
    </xsl:choose>
  </xsl:template>


  <!-- ................................................ -->
  <!-- UKDisplay_SubTotal_OtherAmount                   -->
  <!-- ................................................ -->
  <!-- Summary :                                        
       ................................................ -->
  <!-- FI 20150716 [20892] Add UKDisplay_SubTotal_OtherAmount -->
  <xsl:template name="UKDisplay_SubTotal_OtherAmount">
    <!-- Représent un montant (un élément avec les attributs det,amt,ccy,withside)-->
    <xsl:param name="pOtherAmount"/>
    <xsl:param name="pQtyPattern"/>

    <!-- Affichage de la colonne det (1 cell)-->
    <xsl:choose>
      <xsl:when test="$pOtherAmount and string-length($pOtherAmount/@det) > 0">
        <xsl:variable name="vColor">
          <xsl:choose>
            <xsl:when test="$gIsColorMode and ($pOtherAmount/@withside=1)">
              <xsl:call-template name="GetAmount-color">
                <xsl:with-param name="pAmount" select="$pOtherAmount/*[1]/@amt"/>
              </xsl:call-template>
            </xsl:when>
          </xsl:choose>
        </xsl:variable>
        <fo:table-cell font-size="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-size}"
                       font-weight="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-weight}"
                       padding-top="{$gBlockSettings_Data/subtotal/@padding}"
											 padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">

          <xsl:if test ="string-length($vColor)>0">
            <xsl:call-template name="Display_AddAttribute-color">
              <xsl:with-param name="pColor" select="$vColor"/>
            </xsl:call-template>
          </xsl:if>
          <fo:block>
            <xsl:call-template name="Debug_border-green"/>
            <xsl:call-template name="DisplayType_Short">
              <xsl:with-param name="pType" select="$pOtherAmount/@det"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
      </xsl:when>
      <xsl:otherwise>
        <fo:table-cell>
          <xsl:call-template name="Debug_border-green"/>
        </fo:table-cell>
      </xsl:otherwise>
    </xsl:choose>

    <!-- Affichage de la colonne montant (3 cell: montant, devise, sens)-->
    <xsl:choose>
      <xsl:when test="$pOtherAmount and $pOtherAmount/@isQty = '1'">
        <xsl:call-template name="UKDisplay_SubTotal_Quantity">
          <xsl:with-param name="pQuantity" select="$pOtherAmount/*[1]"/>
          <xsl:with-param name="pQtyPattern" select="$pQtyPattern"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pOtherAmount and string-length($pOtherAmount/*[1]/@amt) > 0">
        <xsl:call-template name="UKDisplay_SubTotal_Amount">
          <xsl:with-param name="pAmount" select="$pOtherAmount/*[1]/@amt" />
          <xsl:with-param name="pCcy" select="$pOtherAmount/*[1]/@ccy"/>
          <xsl:with-param name="pWithSide" select="$pOtherAmount/@withside=1"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <fo:table-cell number-columns-spanned="3">
          <xsl:call-template name="Debug_border-green"/>
        </fo:table-cell>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- ................................................ -->
  <!-- UKDisplay_SubTotal_RowOtherAmount                -->
  <!-- ................................................ -->
  <!-- Summary : Affiche une ligne Subtotal avec 1 seul montant                                       
       ................................................ -->
  <!-- FI 20150716 [20892] Add UKDisplay_SubTotal_RowOtherAmount -->
  <xsl:template name="UKDisplay_SubTotal_RowOtherAmount">
    <!-- Représent le montant à afficher (un élément avec les attributs det,amt,ccy,withside) -->
    <xsl:param name="pOtherAmount"/>
    <!-- Représente le nombre de colonne spanned sur la 1er cellule -->
    <xsl:param name="pColumns-spanned"/>
    <xsl:param name="pQtyPattern"/>

    <fo:table-row font-size="{$gData_font-size}"
                      font-weight="{$gBlockSettings_Data/subtotal/@font-weight}"
                      text-align="{$gData_text-align}"
                      display-align="{$gBlockSettings_Data/subtotal/@display-align}"
                      keep-with-previous="always">

      <fo:table-cell number-columns-spanned="{$pColumns-spanned}">
        <xsl:call-template name="Debug_border-green"/>
      </fo:table-cell>

      <!-- 4 cellules pour 1 étiquette, 1 devise, 1 montant et un sens (CR,DR) -->
      <fo:table-cell number-columns-spanned="4">
        <xsl:call-template name="Debug_border-green"/>
      </fo:table-cell>
      <!-- Espace entre les 2 zones dédiées aux montants-->
      <fo:table-cell>
        <xsl:call-template name="Debug_border-green"/>
      </fo:table-cell>
      <!-- -->
      <xsl:call-template name ="UKDisplay_SubTotal_OtherAmount">
        <xsl:with-param name ="pOtherAmount" select ="$pOtherAmount"/>
        <xsl:with-param name ="pQtyPattern" select ="$pQtyPattern" />
      </xsl:call-template>
    </fo:table-row>
  </xsl:template>


  <!-- ................................................ -->
  <!-- UKDisplay_SubTotal_Fee                           -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display PaymentType and Subtotal Amount  
       ................................................ -->
  <xsl:template name="UKDisplay_SubTotal_Fee">

    <xsl:variable name="vTotal_Fee">
      <xsl:call-template name="GetAmount-amt">
        <xsl:with-param name="pAmount" select="current()"/>
        <xsl:with-param name="pCcy" select="current()/@ccy"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vColor_Fee">
      <xsl:if test="$gIsColorMode">
        <xsl:call-template name="GetAmount-color">
          <xsl:with-param name="pAmount" select="$vTotal_Fee"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>

    <fo:table-cell font-size="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-size}"
                   font-weight="{$gBlockSettings_Data/column[@name='AmountDet']/data/@font-weight}"
                   padding-top="{$gBlockSettings_Data/subtotal/@padding}"
									 padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
      <xsl:call-template name="Display_AddAttribute-color">
        <xsl:with-param name="pColor" select="$vColor_Fee"/>
      </xsl:call-template>
      <fo:block>
        <xsl:call-template name="Debug_border-green"/>
        <xsl:call-template name="DisplayType_Short">
          <xsl:with-param name="pType" select="current()/@paymentType"/>
        </xsl:call-template>
      </fo:block>
    </fo:table-cell>
    <xsl:call-template name="UKDisplay_SubTotal_Amount">
      <xsl:with-param name="pAmount" select="$vTotal_Fee" />
      <xsl:with-param name="pCcy" select="current()/@ccy"/>
      <xsl:with-param name="pColor" select="$vColor_Fee"/>
    </xsl:call-template>

  </xsl:template>

  <!-- ................................................ -->
  <!-- UKDisplay_SubTotal_Amount                        -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Subtotal Amount on 3 columns            
       ................................................ -->
  <xsl:template name="UKDisplay_SubTotal_Amount">
    <xsl:param name="pAmount"/>
    <xsl:param name="pCcy"/>
    <xsl:param name="pWithSide" select="true()"/>
    <xsl:param name="pAmountPattern"/>
    <xsl:param name="pColor"/>
    <xsl:param name="pBackground-color"/>

    <xsl:variable name="vCcy">
      <xsl:call-template name="GetAmount-ccy">
        <xsl:with-param name="pAmount" select="$pAmount"/>
        <xsl:with-param name="pCcy" select="$pCcy"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vAmount">
      <xsl:call-template name="GetAmount-amt">
        <xsl:with-param name="pAmount" select="$pAmount"/>
        <xsl:with-param name="pCcy" select="$vCcy"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vSide">
      <xsl:call-template name="GetAmount-side">
        <xsl:with-param name="pAmount" select="$vAmount"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vColor">
      <xsl:choose>
        <xsl:when test="$gIsColorMode and $pWithSide = true() and string-length($pColor) = 0">
          <xsl:call-template name="GetAmount-color">
            <xsl:with-param name="pAmount" select="$vAmount"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$pColor"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vBackground-color">
      <xsl:choose>
        <xsl:when test="$gIsColorMode and  $pWithSide = true() and string-length($pBackground-color) = 0">
          <xsl:call-template name="GetAmount-background-color">
            <xsl:with-param name="pAmount" select="$vAmount"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$pBackground-color"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="string-length($vAmount) > 0">
        <fo:table-cell font-size="{$gBlockSettings_Data/column[@name='Ccy']/@font-size}"
                       padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                       padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
          <xsl:call-template name="Display_AddAttribute-color">
            <xsl:with-param name="pColor" select="$vColor"/>
          </xsl:call-template>
          <xsl:call-template name="Display_AddAttribute-color">
            <xsl:with-param name="pColor" select="$vBackground-color"/>
            <xsl:with-param name="pAttributeName" select="'background-color'"/>
          </xsl:call-template>
          <fo:block>
            <xsl:call-template name="Debug_border-green"/>
            <xsl:call-template name="DisplayData_Amounts">
              <xsl:with-param name="pDataName" select="'ccy'"/>
              <xsl:with-param name="pCcy" select="$vCcy"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell text-align="{$gData_Number_text-align}"
                       padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                       padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}">
          <xsl:if test="contains(',Default,RightDRCR,RightDR,RightMinusPlus,RightMinus,',concat(',',$gAmount-format,',')) = false()
                  or $pWithSide = false()">
            <xsl:attribute name="number-columns-spanned">
              <xsl:value-of select="'2'"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:call-template name="Display_AddAttribute-color">
            <xsl:with-param name="pColor" select="$vColor"/>
          </xsl:call-template>
          <xsl:call-template name="Display_AddAttribute-color">
            <xsl:with-param name="pColor" select="$vBackground-color"/>
            <xsl:with-param name="pAttributeName" select="'background-color'"/>
          </xsl:call-template>
          <fo:block>
            <xsl:call-template name="Debug_border-green"/>
            <xsl:variable name="vFormattedAmount">
              <xsl:call-template name="DisplayData_Amounts">
                <xsl:with-param name="pDataName" select="'amount'"/>
                <xsl:with-param name="pAmount" select="$vAmount"/>
                <xsl:with-param name="pCcy" select="$vCcy"/>
                <xsl:with-param name="pAmountPattern" select="$pAmountPattern" />
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="vAmountSettingColumn" select="$gBlockSettings_Data/column[@name='Amount']"/>
            <xsl:variable name="vAmountFontSize">
              <xsl:choose>
                <xsl:when test="$vAmountSettingColumn/style[@name='S1']/@length >= string-length($vFormattedAmount)">
                  <xsl:value-of select="$vAmountSettingColumn/style[@name='S1']/@font-size"/>
                </xsl:when>
                <xsl:when test="$vAmountSettingColumn/style[@name='S2']/@length >= string-length($vFormattedAmount)">
                  <xsl:value-of select="$vAmountSettingColumn/style[@name='S2']/@font-size"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$vAmountSettingColumn/style[@name='S3']/@font-size"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:variable name="vAmountToDisplay">
              <xsl:choose>
                <xsl:when test="$gcIsTestMode=true()">
                  <xsl:choose>
                    <xsl:when test="$vAmountSettingColumn/style[@name='S1']/@length >= string-length($vFormattedAmount)">
                      <xsl:value-of select="$vAmountSettingColumn/style[@name='S1']/@description"/>
                    </xsl:when>
                    <xsl:when test="$vAmountSettingColumn/style[@name='S2']/@length >= string-length($vFormattedAmount)">
                      <xsl:value-of select="$vAmountSettingColumn/style[@name='S2']/@description"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="$vAmountSettingColumn/style[@name='S3']/@description"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$vFormattedAmount"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>

            <xsl:choose>
              <xsl:when test="$pWithSide = false()">
                <fo:inline font-size="{$vAmountFontSize}">
                  <xsl:value-of select="$vAmountToDisplay"/>
                </fo:inline>
              </xsl:when>
              <xsl:when test="contains(',LeftDRCR,LeftDR,LeftMinusPlus,LeftMinus,',concat(',',$gAmount-format,','))">
                <fo:inline font-size="{$gData_Det_font-size}">
                  <xsl:value-of select="concat($vSide,$gcSpace)"/>
                </fo:inline>
                <fo:inline font-size="{$vAmountFontSize}">
                  <xsl:value-of select="$vAmountToDisplay"/>
                </fo:inline>
              </xsl:when>
              <xsl:when test="$gAmount-format='Parenthesis' and string-length($vSide) > 0">
                <fo:inline font-size="{$vAmountFontSize}">
                  <xsl:value-of select="concat('(',$vAmountToDisplay,')')"/>
                </fo:inline>
              </xsl:when>
              <xsl:otherwise>
                <fo:inline font-size="{$vAmountFontSize}">
                  <xsl:value-of select="$vAmountToDisplay"/>
                </fo:inline>
              </xsl:otherwise>
            </xsl:choose>
          </fo:block>
        </fo:table-cell>
        <xsl:if test="contains(',Default,RightDRCR,RightDR,RightMinusPlus,RightMinus,',concat(',',$gAmount-format,','))
                  and $pWithSide = true()">
          <fo:table-cell font-size="{$gData_Det_font-size}"
                         font-weight="{$gBlockSettings_Data/subtotal/column[@name='DrCr']/@font-weight}"
                         padding-top="{$gBlockSettings_Data/subtotal/@padding}"
												 padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                         text-align="{$gBlockSettings_Data/subtotal/column[@name='DrCr']/@text-align}">
            <xsl:call-template name="Display_AddAttribute-color">
              <xsl:with-param name="pColor" select="$vColor"/>
            </xsl:call-template>
            <xsl:call-template name="Display_AddAttribute-color">
              <xsl:with-param name="pColor" select="$vBackground-color"/>
              <xsl:with-param name="pAttributeName" select="'background-color'"/>
            </xsl:call-template>
            <fo:block>
              <xsl:call-template name="Debug_border-green"/>
              <xsl:value-of select="$vSide"/>
            </fo:block>
          </fo:table-cell>
        </xsl:if>
      </xsl:when>
      <xsl:otherwise>
        <fo:table-cell number-columns-spanned="3">
          <xsl:call-template name="Debug_border-green"/>
        </fo:table-cell>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!-- ................................................ -->
  <!-- UKDisplay_SubTotal_Quantity                      -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Quantity on 3 columns (Quantity and qtyUnit)
       ................................................ -->
  <xsl:template name="UKDisplay_SubTotal_Quantity">
    <xsl:param name="pQuantity"/>
    <xsl:param name="pQtyUnit"/>
    <xsl:param name="pQtyPattern"/>

    <xsl:variable name="vQtyUnit">
      <xsl:call-template name="GetQuantity-qtyUnit">
        <xsl:with-param name="pQuantity" select="$pQuantity"/>
        <xsl:with-param name="pQtyUnit" select="$pQtyUnit"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vQuantity">
      <xsl:call-template name="GetQuantity-fmtQty">
        <xsl:with-param name="pQuantity" select="$pQuantity"/>
        <xsl:with-param name="pQtyUnit" select="$vQtyUnit"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="string-length($vQuantity) > 0">
        <fo:table-cell text-align="{$gData_Number_text-align}"
                       padding-top="{$gBlockSettings_Data/subtotal/@padding}"
                       padding-bottom="{$gBlockSettings_Data/subtotal/@padding-bottom}"
                       number-columns-spanned="3">

          <fo:block>
            <xsl:call-template name="Debug_border-green"/>
            <xsl:variable name="vFormattedAmount">
              <xsl:call-template name="DisplayFmtNumber">
                <xsl:with-param name="pFmtNumber" select="$vQuantity"/>
                <xsl:with-param name="pPattern" select="$pQtyPattern" />
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="vAmountSettingColumn" select="$gBlockSettings_Data/column[@name='Amount']"/>
            <xsl:variable name="vAmountFontSize">
              <xsl:choose>
                <xsl:when test="$vAmountSettingColumn/style[@name='S1']/@length >= string-length($vFormattedAmount)">
                  <xsl:value-of select="$vAmountSettingColumn/style[@name='S1']/@font-size"/>
                </xsl:when>
                <xsl:when test="$vAmountSettingColumn/style[@name='S2']/@length >= string-length($vFormattedAmount)">
                  <xsl:value-of select="$vAmountSettingColumn/style[@name='S2']/@font-size"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$vAmountSettingColumn/style[@name='S3']/@font-size"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:variable name="vAmountToDisplay">
              <xsl:choose>
                <xsl:when test="$gcIsTestMode=true()">
                  <xsl:choose>
                    <xsl:when test="$vAmountSettingColumn/style[@name='S1']/@length >= string-length($vFormattedAmount)">
                      <xsl:value-of select="$vAmountSettingColumn/style[@name='S1']/@description"/>
                    </xsl:when>
                    <xsl:when test="$vAmountSettingColumn/style[@name='S2']/@length >= string-length($vFormattedAmount)">
                      <xsl:value-of select="$vAmountSettingColumn/style[@name='S2']/@description"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="$vAmountSettingColumn/style[@name='S3']/@description"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$vFormattedAmount"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>

            <fo:inline font-size="{$vAmountFontSize}">
              <xsl:value-of select="$vAmountToDisplay"/>
            </fo:inline>
            <fo:inline font-size="{$gData_Det_font-size}">
              <xsl:value-of select="concat($gcSpace,$vQtyUnit)"/>
            </fo:inline>
          </fo:block>
        </fo:table-cell>
      </xsl:when>
      <xsl:otherwise>
        <fo:table-cell number-columns-spanned="3">
          <xsl:call-template name="Debug_border-green"/>
        </fo:table-cell>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- ................................................ -->
  <!-- UKDisplay_Underlyer                              -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Underlyer Asset Description          
       ................................................ -->
  <xsl:template name="Display_Underlyer">
    <!-- Element repository de l'asset -->
    <xsl:param name="pRepository-asset"/>
    <!-- Element Setting [contient un élément data (avec attribut @length => nbr de caractère Max)]  -->
    <xsl:param name="pColumnSettings"/>

    <xsl:variable name="vDataDesc" select="normalize-space($pRepository-asset/altIdentifier)"/>
    <xsl:variable name="vDataDet" select="concat(' ', $pRepository-asset/isinCode)"/>

    <xsl:call-template name="DisplayData_Truncate">
      <xsl:with-param name="pData" select="$vDataDesc"/>
      <xsl:with-param name="pDataLength" select="$pColumnSettings/data/@length - string-length($vDataDet)"/>
    </xsl:call-template>

    <fo:inline font-weight="{$pColumnSettings/data[@name='det']/@font-weight}">
      <xsl:value-of select="$vDataDet"/>
    </fo:inline>
  </xsl:template>

  <!-- ................................................ -->
  <!-- UKDisplay_FundingPayment                         -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Funding Payment          
       ................................................ -->
  <!-- FI 20150727 [XXXXX] Obsolete-->
  <xsl:template name="UKDisplay_FundingPayment_Obsolete">
    <xsl:param name="pDataEfsml"/>
    <xsl:param name="pColumnSettings"/>
    <xsl:variable name="vDataDesc" select="$pColumnSettings/data[@name='Funding']/@resource"/>

    <xsl:variable name="vDataDet">
      <xsl:variable name="vPos">
        <xsl:choose>
          <xsl:when test="$pDataEfsml/@side = 1">
            <xsl:value-of select="'Long'"/>
          </xsl:when>
          <xsl:when test="$pDataEfsml/@side = 2">
            <xsl:value-of select="'Short'"/>
          </xsl:when>
        </xsl:choose>
      </xsl:variable>
      <xsl:value-of select="concat(' (',$vPos,')')"/>
    </xsl:variable>

    <xsl:call-template name="DisplayData_Truncate">
      <xsl:with-param name="pData" select="$vDataDesc"/>
      <xsl:with-param name="pDataLength" select="$pColumnSettings/data/@length - string-length($vDataDet)"/>
    </xsl:call-template>

    <fo:inline font-weight="{$pColumnSettings/data[@name='det']/@font-weight}">
      <xsl:value-of select="$vDataDet"/>
    </fo:inline>
  </xsl:template>


  <!-- ................................................ -->
  <!-- UKDisplay_BorrowingPayment                       -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Borrowing Payment          
       ................................................ -->
  <!-- FI 20150727 [XXXXX] Obsolete-->
  <xsl:template name="UKDisplay_BorrowingPayment_Obsolete">
    <xsl:param name="pDataEfsml"/>
    <xsl:param name="pColumnSettings"/>

    <xsl:variable name="vDataDesc">
      <xsl:value-of select="$pColumnSettings/data[@name='Borrowing']/@resource"/>
    </xsl:variable>
    <xsl:variable name="vDataDet"/>

    <xsl:call-template name="DisplayData_Truncate">
      <xsl:with-param name="pData" select="$vDataDesc"/>
      <xsl:with-param name="pDataLength" select="$pColumnSettings/data/@length - string-length($vDataDet)"/>
    </xsl:call-template>

    <fo:inline font-weight="{$pColumnSettings/data[@name='det']/@font-weight}">
      <xsl:value-of select="$vDataDet"/>
    </fo:inline>
  </xsl:template>

  <!-- ................................................ -->
  <!-- UKDisplay_SafekeepingPayment                     -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Safekeeping Payment          
       ................................................ -->
  <xsl:template name="UKDisplay_SafekeepingPayment_Obsolete">
    <xsl:param name="pDataEfsml"/>
    <xsl:param name="pColumnSettings"/>

    <xsl:variable name="vDataDesc">
      <xsl:value-of select="$pColumnSettings/data[@name='Safekeeping']/@resource"/>
    </xsl:variable>
    <xsl:variable name="vDataDet"/>

    <xsl:call-template name="DisplayData_Truncate">
      <xsl:with-param name="pData" select="$vDataDesc"/>
      <xsl:with-param name="pDataLength" select="$pColumnSettings/data/@length - string-length($vDataDet)"/>
    </xsl:call-template>

    <fo:inline font-weight="{$pColumnSettings/data[@name='det']/@font-weight}">
      <xsl:value-of select="$vDataDet"/>
    </fo:inline>
  </xsl:template>

  <!-- ................................................ -->
  <!-- UKDisplay_CashPaymentType                        -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display CashPayment PaymentType             
       ................................................ -->
  <xsl:template name="UKDisplay_CashPaymentType">
    <xsl:param name="pCashPayment"/>
    <xsl:param name="pColumnSettings"/>

    <xsl:variable name="vSeparator" select="': '"/>
    <xsl:variable name="vPaymentType" select="$pCashPayment/@paymentType"/>
    <xsl:variable name="vEventType" select="$pCashPayment/@eventType"/>
    <xsl:variable name="vPart-before" select="substring-before($vPaymentType, $vSeparator)"/>
    <xsl:variable name="vPart-after" select="substring-after($vPaymentType, $vSeparator)"/>

    <xsl:variable name="vDisplay">
      <xsl:choose>
        <xsl:when test="$vEventType = $vPart-before">
          <xsl:value-of select="$vPart-after"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$vPaymentType"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:call-template name="DisplayData_Truncate">
      <xsl:with-param name="pData" select="$vDisplay"/>
      <xsl:with-param name="pDataLength" select="$pColumnSettings/data/@length"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- UKDisplay_JNPaymentDefault                        -->
  <!-- ................................................ -->
  <!-- Summary :                                        
       ................................................ -->
  <xsl:template name="UKDisplay_JNPaymentDefault">
    <xsl:param name ="pType"/>
    <xsl:param name="pDataEfsml"/>
    <xsl:param name="pColumnSettings"/>

    <xsl:variable name="vDataRes" select="$pColumnSettings/data[@name=$pType]/@resource"/>

    <xsl:variable name="vPos">
      <xsl:if test ="($pType='Funding') or ($pType='Borrowing')">
        <xsl:choose>
          <xsl:when test="$pDataEfsml/@side = 1">
            <xsl:value-of select="'Long'"/>
          </xsl:when>
          <xsl:when test="$pDataEfsml/@side = 2">
            <xsl:value-of select="'Short'"/>
          </xsl:when>
        </xsl:choose>
      </xsl:if>
    </xsl:variable>

    <xsl:variable name="vData">
      <xsl:choose >
        <xsl:when test ="string-length($vPos)>0">
          <xsl:value-of select="concat($vDataRes,' (',$vPos,')')"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$vDataRes"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:call-template name="DisplayData_Truncate">
      <xsl:with-param name="pData" select="$vData"/>
      <xsl:with-param name="pDataLength" select="$pColumnSettings/data/@length"/>
    </xsl:call-template>

  </xsl:template>

  <!-- .......................................................  -->
  <!-- Synthesis_GetAmountLabel                                 -->
  <!-- Summary : Retourne la resource associée à  pAmountName   -->
  <!-- ........................................................ -->
  <!-- FI 20150921 [21311] Add -->
  <xsl:template name ="Synthesis_GetAmountLabel">
    <xsl:param name ="pAmountName"/>

    <xsl:variable name="vResource" select="$gBlockSettings_Summary/column[@name='Designation2']/data[@name=$pAmountName]/@resource"/>
    <xsl:choose>
      <xsl:when test="string-length($vResource) > 0">
        <xsl:value-of select="$vResource"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pAmountName"/>
      </xsl:otherwise>
    </xsl:choose>

    <xsl:if test="string-length($gBlockSettings_Summary/column[@name='Designation2']/data[@name=$pAmountName]/@resourceInline) > 0">
      <fo:inline font-size="{$gBlockSettings_Summary/column[@name='Designation2']/data[@name='Det']/@font-size}"
                 font-weight="{$gBlockSettings_Summary/column[@name='Designation2']/data[@name='Det']/@font-weight}">
        <xsl:value-of select="concat($gcSpace,'(',$gBlockSettings_Summary/column[@name='Designation2']/data[@name=$pAmountName]/@resourceInline,')')"/>
      </fo:inline>
    </xsl:if>
  </xsl:template>

  <!-- ...............................................................................  -->
  <!-- UnderlyingHeader                                                                 -->
  <!-- Summary : Retourne le titre de la colonne utilisée pour désignation de l'asset   -->
  <!-- ................................................................................ -->
  <!-- FI 20150921 [21311] Add -->
  <xsl:template name ="UnderlyingHeader">
    <xsl:param name ="pFamily"/>

    <xsl:variable name ="vName">
      <xsl:choose>
        <xsl:when test="$pFamily='ESE'">
          <xsl:value-of select ="'StockDescription'"/>
        </xsl:when>
        <xsl:when test="$pFamily='DSE'">
          <xsl:value-of select ="'DebtSecurityDescription'"/>
        </xsl:when>
        <xsl:when test="$pFamily='COMS'">
          <xsl:value-of select ="'ContractDescription'"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test ="string-length($vName)>0">
        <xsl:value-of select="$gBlockSettings_Data/column[@name='Underlying']/header[@name=$vName]/@resource"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$gBlockSettings_Data/column[@name='Underlying']/header/@resource"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>
