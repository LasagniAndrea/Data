<?xml version="1.0" encoding="UTF-8" ?>
<!--
=============================================================================================
Summary : Shared XSL to display data in PDF format 
File    : Shared_DataGrid_PDFCommon.xslt
=============================================================================================
Version : v4.0.5239                                           
Date    : 20140603
Author  : RD
Comment : Mise en place du spécifique Sigma:
- Ajout des sous totaux par acteur de niveau 2     
=============================================================================================
Version : v3.7.5155                                           
Date    : 20140417
Author  : RD
Comment : First version     
=============================================================================================
-->
<xsl:stylesheet
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  xmlns:dt="http://xsltsl.org/date-time"
  xmlns:fo="http://www.w3.org/1999/XSL/Format"
  version="1.0">

  <xsl:param name="pCurrentCulture" select="'en-GB'"/>

  <!-- xslt includes -->
  <xsl:include href="..\..\..\Library\Resource.xslt"/>
  <xsl:include href="..\..\..\Library\DtFunc.xslt"/>
  <xsl:include href="..\..\..\Library\NbrFunc.xslt" />
  <xsl:include href="..\..\..\Library\StrFunc.xslt" />
  <xsl:include href="..\..\..\Library\xsltsl\date-time.xsl"/>

  <xsl:variable name="vLowerLetters">abcdeéèfghijklmnopqrstuvwxyz</xsl:variable>
  <xsl:variable name="vUpperLetters">ABCDEEEFGHIJKLMNOPQRSTUVWXYZ</xsl:variable>

  <!-- Keys -->
  <xsl:key name="kMarketKey" match="Column[@ColumnName='MKTIDENT_SHORTIDENTIFIER']" use="text()"/>
  <xsl:key name="kCurrencyKey" match="Column[@ColumnName='FLOWS_CUR_UNIT']" use="text()"/>
  <xsl:key name="kBookKey" match="Column[@ColumnName='B_ALLOC_P_IDENTIFIER']" use="text()"/>
  <!-- ================================================================ -->
  <!-- Global Report settings                                           -->
  <!-- ================================================================ -->
  <xsl:variable name="gReportTitle" select="'Report-FinStatMember-title'"/>
  <xsl:variable name="gReportNoInformationDet" select="'Report-NoInformationDet'"/>

  <xsl:variable name="gReportReferential" select="Referential"/>
  <xsl:variable name="gReportTableName" select="$gReportReferential/@TableName"/>
  <xsl:variable name="gReportRows" select="$gReportReferential/Row[Column[@ColumnName='CM_CLEARINGMEMBERTYPE' and (text()='GCM' or text()='DCM' or text()='NCM')]]"/>
  <xsl:variable name="gReportData" select="$gReportReferential/Datas/Data"/>

  <xsl:variable name="gDtBusiness">
    <xsl:call-template name ="getClearingDate"/>
  </xsl:variable>
  <xsl:variable name="gCreationTimestamp">
    <xsl:value-of select="$gReportReferential/@timestamp"/>
  </xsl:variable>
  <xsl:variable name="gIsReferential" select="$gReportReferential=true()" />
  <!-- Display -->
  <xsl:variable name ="vHeaderExtent">
    <xsl:value-of select ="$vPageA4LandscapeTopMargin"/>
  </xsl:variable>

  <xsl:variable name="gHdrCellBackgroundColor">#e3e3e3</xsl:variable>
  <xsl:variable name="gCellPadding">2pt</xsl:variable>
  <xsl:variable name="gTotalBackgroundColor">#c3c3c3</xsl:variable>

  <xsl:variable name="vIsDebug">0</xsl:variable>

  <xsl:variable name="vBorder">
    <xsl:choose>
      <xsl:when test="$vIsDebug=1">0.5pt solid black</xsl:when>
      <xsl:otherwise>0pt solid black</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name ="vEntity">
    <xsl:text>ENTITY</xsl:text>
  </xsl:variable>

  <xsl:variable name ="vApos">'</xsl:variable>

  <xsl:variable name="varImgLogo">
    <xsl:value-of select="concat('sql(select top(1) IDENTIFIER, LOLOGO from dbo.ACTOR a inner join dbo.ACTORROLE ar on ar.IDA = a.IDA where IDROLEACTOR =', $vApos, $vEntity, $vApos, 'and a.IDENTIFIER !=', $vApos, 'EFSLIC', $vApos,  ')')" />
  </xsl:variable>

  <!-- font ================================================================================= -->
  <!-- sans-serif/Helvetica/Courier/Times New Roman/New York/serif/Gill/-->
  <xsl:variable name="vFontFamily">sans-serif</xsl:variable>
  <xsl:variable name="vFontSizeHeader">9pt</xsl:variable>
  <xsl:variable name="vFontSizeColumnTitles">8pt</xsl:variable>
  <xsl:variable name="vFontSizeFormula">9pt</xsl:variable>
  <xsl:variable name="vFontSizeDet">5pt</xsl:variable>
  <xsl:variable name="vRuleSize">0.5pt</xsl:variable>

  <!-- A4 page caracteristics ============================================================== -->
  <xsl:variable name="vPageA4LandscapePageHeight">21</xsl:variable>
  <xsl:variable name="vPageA4LandscapePageWidth">29.7</xsl:variable>
  <xsl:variable name="vPageA4LandscapeMargin">0.5</xsl:variable>

  <!-- Variables for A4 page format ====================================================== -->
  <xsl:variable name="vPageA4LandscapeLeftMargin">0.5</xsl:variable>
  <xsl:variable name="vPageA4LandscapeRightMargin">0.5</xsl:variable>
  <xsl:variable name="vPageA4LandscapeTopMargin">0.1</xsl:variable>

  <!-- Variables for Footer ====================================================== -->
  <xsl:variable name ="vFooterExtent">1</xsl:variable>

  <!-- Variables for Body ====================================================== -->
  <xsl:variable name ="vBodyLeftMargin">0</xsl:variable>
  <xsl:variable name ="vBodyTopMargin">
    <xsl:value-of select ="$vHeaderExtent"/>
  </xsl:variable>
  <xsl:variable name ="vBodyBottomMargin">
    <xsl:value-of select ="$vFooterExtent+$vPageA4LandscapeMargin"/>
  </xsl:variable>

  <!-- Variables for Table ====================================================== -->
  <xsl:variable name="varTableBorder">
    0.1pt solid black
  </xsl:variable>

  <!-- Variables for table-cell ====================================================== -->
  <xsl:variable name ="vTableCellPadding">0.1</xsl:variable>
  <xsl:variable name ="vAmountColumnWidth">3.5</xsl:variable>
  <xsl:variable name ="vLogoHeight">1.35</xsl:variable>
  <xsl:variable name ="vRowHeightBreakline">0.4</xsl:variable>
  <xsl:variable name ="vRowHeight">0.6</xsl:variable>

  <xsl:variable name ="vRowBackgroundColor">
    GhostWhite
  </xsl:variable>

  <!-- ================================================================ -->
  <!-- templates ====================================================== -->
  <!-- ================================================================ -->

  <xsl:template match="/Referential">
    <fo:root xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <xsl:call-template name="SetPagesCaracteristics"/>
      <xsl:call-template name="DisplayDocumentContent"/>
    </fo:root>
  </xsl:template>

  <!-- =========================================================  -->
  <!-- Common graphic templates section                           -->
  <!-- =========================================================  -->


  <xsl:template name="SetPagesCaracteristics">
    <fo:layout-master-set>
      <fo:simple-page-master master-name="Landscape" page-height="{$vPageA4LandscapePageHeight}cm" page-width="{$vPageA4LandscapePageWidth}cm" margin="{$vPageA4LandscapeMargin}cm">
        <fo:region-body region-name="PageBody" background-color="white" margin-left="{$vBodyLeftMargin}cm" margin-top="{$vBodyTopMargin}cm" margin-bottom="{$vBodyBottomMargin}cm" />
        <fo:region-before region-name="PageHeader" background-color="white" extent="{$vHeaderExtent}cm"  precedence="true" />
        <fo:region-after region-name="PageFooter" background-color="white" extent="{$vFooterExtent}cm" precedence="true" />
      </fo:simple-page-master>
    </fo:layout-master-set>
  </xsl:template>

  <xsl:template name="DisplayDocumentContent">
    <xsl:call-template name="DisplayDocumentContentCommon"/>
  </xsl:template>
  <xsl:template name="DisplayDocumentContentCommon">
    <xsl:param name="pData"/>
    <xsl:param name="pRows"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <fo:page-sequence master-reference="Landscape" initial-page-number="1" force-page-count="no-force">

      <fo:static-content flow-name="PageHeader">
        <xsl:call-template name="displayHeader">
          <xsl:with-param name="pData" select="$pData"/>
          <xsl:with-param name="pCulture" select="$pCulture"/>
        </xsl:call-template>
      </fo:static-content>

      <fo:static-content flow-name="PageFooter">
        <xsl:call-template name="displayFooter">
          <xsl:with-param name="pData" select="$pData"/>
          <xsl:with-param name="pCulture" select="$pCulture"/>
        </xsl:call-template>
      </fo:static-content>

      <fo:flow flow-name="PageBody">
        <xsl:call-template name="displayPageBody">
          <xsl:with-param name="pData" select="$pData"/>
          <xsl:with-param name="pRows" select="$pRows"/>
          <xsl:with-param name="pCulture" select="$pCulture"/>
        </xsl:call-template>
        <xsl:call-template name="AddEndOfDoc">
          <xsl:with-param name="pData" select="$pData"/>
        </xsl:call-template>
      </fo:flow>
    </fo:page-sequence>
  </xsl:template>

  <xsl:template name="displayBreakline">
    <fo:table table-layout="fixed" >
      <xsl:call-template name ="create1Column"/>
      <fo:table-body>
        <fo:table-row height="{$vRowHeightBreakline}cm">
          <fo:table-cell border="{$vBorder}">
            <fo:block border="{$vBorder}"/>
          </fo:table-cell>
        </fo:table-row>
      </fo:table-body>
    </fo:table>
  </xsl:template>
  <xsl:template name ="displayBlankField">
    <fo:block border="{$vBorder}">
      <!-- This field is intentionally left blank -->
    </fo:block>
  </xsl:template>

  <!-- draw horizontal line -->
  <xsl:template name ="drawHorizontalLine">
    <xsl:param name="pRowHeight" select="$vRowHeight"/>
    <xsl:param name="pRowColor" select="'grey'"/>

    <fo:table table-layout="fixed" >
      <xsl:call-template name ="create1Column"/>
      <fo:table-body>
        <fo:table-row height="{$pRowHeight}cm">
          <fo:table-cell border="{$vBorder}">
            <fo:block border="{$vBorder}" text-align="center" display-align="center">
              <fo:leader leader-length="100%" leader-pattern="rule" rule-style="solid" rule-thickness="1px" color="{$pRowColor}" />
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
      </fo:table-body>
    </fo:table>
  </xsl:template>
  <xsl:template name="createHeader3Columns">
    <xsl:variable name="vColumnWidth1">7</xsl:variable>
    <xsl:variable name="vColumnWidth2">14</xsl:variable>
    <xsl:variable name="vColumnWidth3">
      <xsl:value-of select =" $vPageA4LandscapePageWidth - $vColumnWidth1 - $vColumnWidth2 - $vPageA4LandscapeLeftMargin - $vPageA4LandscapeRightMargin"/>
    </xsl:variable>
    <fo:table-column column-width="{$vColumnWidth1}cm" column-number="01" />
    <fo:table-column column-width="{$vColumnWidth2}cm" column-number="02" />
    <fo:table-column column-width="{$vColumnWidth3}cm" column-number="03" />
  </xsl:template>
  <xsl:template name="createHeader1Columns">
    <xsl:variable name="vColumnWidth1">
      <xsl:value-of select =" $vPageA4LandscapePageWidth - $vPageA4LandscapeLeftMargin - $vPageA4LandscapeRightMargin"/>
    </xsl:variable>
    <fo:table-column column-width="{$vColumnWidth1}cm" column-number="01" />
  </xsl:template>
  <xsl:template name="createHeader2Columns">
    <xsl:variable name="vColumnWidth1">4</xsl:variable>
    <xsl:variable name="vColumnWidth2">
      <xsl:value-of select =" $vPageA4LandscapePageWidth - $vColumnWidth1 - $vPageA4LandscapeLeftMargin - $vPageA4LandscapeRightMargin"/>
    </xsl:variable>
    <fo:table-column column-width="{$vColumnWidth1}cm" column-number="01" />
    <fo:table-column column-width="{$vColumnWidth2}cm" column-number="02" />
  </xsl:template>
  <xsl:template name="createBodyColumns">
    <fo:table-column column-width="{4.7}cm" column-number="01"></fo:table-column>
    <fo:table-column column-width="{3}cm" column-number="02"></fo:table-column>
    <fo:table-column column-width="{$vAmountColumnWidth}cm" column-number="03"></fo:table-column>
    <fo:table-column column-width="{$vAmountColumnWidth}cm" column-number="04"></fo:table-column>
    <fo:table-column column-width="{$vAmountColumnWidth}cm" column-number="05"></fo:table-column>
    <fo:table-column column-width="{$vAmountColumnWidth}cm" column-number="06"></fo:table-column>
    <fo:table-column column-width="{$vAmountColumnWidth}cm" column-number="07"></fo:table-column>
    <fo:table-column column-width="{$vAmountColumnWidth}cm" column-number="08"></fo:table-column>
  </xsl:template>
  <xsl:template name="createBody2Columns">
    <fo:table-column column-width="{4}cm" column-number="01" />
    <fo:table-column column-width="{15}cm" column-number="02" />
  </xsl:template>
  <xsl:template name="create1Column">
    <!-- EG 20160404 Migration vs2013 -->
    <!--<fo:table-column column-width="100%" column-number="01"></fo:table-column>-->
    <fo:table-column column-width="proportional-column-width(100)" column-number="01"/>
  </xsl:template>

  <!-- =========================================================  -->
  <!-- Header section                                             -->
  <!-- =========================================================  -->
  <xsl:template name="displayHeader">
    <xsl:param name="pData"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <!-- Header 1° block -->
    <fo:block linefeed-treatment="preserve">
      <fo:table table-layout="fixed">
        <xsl:call-template name ="createHeader3Columns"/>
        <fo:table-body>
          <fo:table-row height="{$vLogoHeight}cm" color="black" font-family="{$vFontFamily}">
            <!-- Logo -->
            <fo:table-cell border="{$vBorder}" text-align="left">
              <fo:block border="{$vBorder}">
                <!--<fo:external-graphic src="{$varImgLogo}" height="{$vLogoHeight}cm" />-->
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}">
                <xsl:call-template name="displayBlankField"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}">
                <xsl:call-template name="displayBlankField"/>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </fo:block>
  </xsl:template>

  <!-- =========================================================  -->
  <!-- Footer section                                             -->
  <!-- =========================================================  -->
  <xsl:template name="displayFooter">
    <xsl:param name="pData"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <fo:block >
      <fo:table font-size="8pt" text-align="right" table-layout="fixed" border="{$vBorder}">
        <xsl:call-template name ="create1Column"/>
        <fo:table-body>
          <fo:table-row font-weight="normal">
            <fo:table-cell padding="0mm" border="0pt" text-align="right" display-align="center" font-size="5pt">
              <fo:block linefeed-treatment="preserve" >
                <xsl:text>Eurosys statement powered by Spheres - © 2024 EFS</xsl:text>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </fo:block>
    <fo:block >
      <fo:table table-layout="fixed" border-top-style="solid" border="{$vRuleSize}">
        <xsl:call-template name ="create1Column"/>
        <fo:table-body>
          <fo:table-row font-family="{$vFontFamily}">
            <!-- PageNumber -->
            <fo:table-cell padding="1mm"
                           border="{$vBorder}"
                           text-align="right"
                           display-align="before"
                           font-size="8pt">
              <fo:block>
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'Page'" />
                  <xsl:with-param name="pCulture" select="$pCulture" />
                </xsl:call-template>
                <xsl:text> </xsl:text>
                <xsl:call-template name="displayPageNumber">
                  <xsl:with-param name="pData" select="$pData"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </fo:block>
  </xsl:template>

  <xsl:template name="AddEndOfDoc">
    <xsl:param name="pData"/>
    <fo:block id="EndOfDoc" />
  </xsl:template>
  <xsl:template name="displayPageNumber">
    <xsl:param name="pData"/>
    <fo:page-number />/<fo:page-number-citation ref-id="EndOfDoc" />
  </xsl:template>

  <!-- =========================================================  -->
  <!-- Body section                                               -->
  <!-- =========================================================  -->
  <xsl:template name="displayPageBody">
    <xsl:param name="pData"/>
    <xsl:param name="pRows"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <fo:block linefeed-treatment="preserve">
      <fo:table table-layout="fixed" >
        <xsl:call-template name ="createHeader1Columns"/>
        <fo:table-header>
          <fo:table-row height="{$vRowHeight}cm" text-align="left"
                        font-size="{$vFontSizeHeader}" font-weight="bold" color="black" font-family="{$vFontFamily}">
            <fo:table-cell border="{$vBorder}" display-align="after" font-weight="bold" text-align="center" font-size="14pt">
              <fo:block border="{$vBorder}">
                <xsl:variable name ="vTitle">
                  <xsl:call-template name="getSpheresTranslation">
                    <xsl:with-param name="pResourceName" select="$gReportTitle" />
                    <xsl:with-param name="pCulture" select="$pCulture" />
                  </xsl:call-template>
                </xsl:variable>
                <xsl:value-of select="translate($vTitle, $vLowerLetters, $vUpperLetters)"/>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
          <fo:table-row height="{$vRowHeight}cm" text-align="center"
                        font-size="8pt" display-align="before" font-weight="bold" color="grey" font-family="{$vFontFamily}">
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}">
                <xsl:variable name ="vTableDisplayName">
                  <xsl:call-template name="getTableDisplayName">
                    <xsl:with-param name="pTableName" select="$gReportTableName"/>
                    <xsl:with-param name="pCulture" select="$pCulture" />
                  </xsl:call-template>
                </xsl:variable>
                <xsl:if test ="string-length($vTableDisplayName)>0">
                  <xsl:text>(</xsl:text>
                  <xsl:if test ="substring-after($vTableDisplayName,';') != ''">
                    <xsl:value-of select ="substring-after($vTableDisplayName,';')"/>
                  </xsl:if>
                  <xsl:if test ="substring-after($vTableDisplayName,';') = ''">
                    <xsl:value-of select ="$vTableDisplayName"/>
                  </xsl:if>
                  <xsl:text>)</xsl:text>
                </xsl:if>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
          <xsl:call-template name="displayPageBodyHeader">
            <xsl:with-param name="pData" select="$pData"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </fo:table-header>
        <fo:table-body>
          <fo:table-row>
            <fo:table-cell border="{$vBorder}">
              <xsl:call-template name ="displayPageBodyDetail">
                <xsl:with-param name="pData" select="$pData"/>
                <xsl:with-param name="pRows" select="$pRows"/>
                <xsl:with-param name="pCulture" select="$pCulture" />
              </xsl:call-template>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </fo:block>
  </xsl:template>

  <xsl:template name="displayPageBodyHeader">
    <xsl:param name="pData"/>

  </xsl:template>

  <xsl:template name="displayPageBodyDetail">
    <xsl:param name="pData"/>

    <xsl:variable name="vColumnsCopy">
      <xsl:copy-of select="$gReportRows/Column"/>
    </xsl:variable>
    <xsl:variable name="vMarkets">
      <xsl:copy-of select="msxsl:node-set($vColumnsCopy)/Column[(generate-id()=generate-id(key('kMarketKey',text())))]"/>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="msxsl:node-set($vMarkets)/Column">
        <xsl:for-each select="msxsl:node-set($vMarkets)/Column">
          <xsl:sort select="text()" data-type="text"/>

          <xsl:variable name="vMarketName" select="text()"/>

          <xsl:call-template name="displayMarket">
            <xsl:with-param name="pMarketName" select="$vMarketName" />
          </xsl:call-template>

          <xsl:if test="position() != last()">
            <fo:block break-after='page'/>
          </xsl:if>

        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name ="displayNoData"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="displayNoData">
    <fo:block linefeed-treatment="preserve">
      <xsl:call-template name="displayBreakline"/>
      <xsl:call-template name="displayBreakline"/>
    </fo:block>
    <fo:block linefeed-treatment="preserve">
      <fo:table table-layout="fixed" >
        <xsl:call-template name ="createHeader2Columns"/>
        <fo:table-body>
          <xsl:call-template name ="displayDates"/>
        </fo:table-body>
      </fo:table>
    </fo:block>
    <fo:block linefeed-treatment="preserve">
      <xsl:call-template name="displayBreakline"/>
      <xsl:call-template name="displayBreakline"/>
    </fo:block>
    <fo:block linefeed-treatment="preserve" text-align="center"
              font-size="{$vFontSizeFormula}" font-weight="bold" font-family="{$vFontFamily}">
      <xsl:call-template name="getSpheresTranslation">
        <xsl:with-param name="pResourceName" select="'Msg_NoInformation'"/>
      </xsl:call-template>
    </fo:block>
    <xsl:call-template name="displayBreakline"/>
    <fo:block linefeed-treatment="preserve" text-align="left"
              font-size="{$vFontSizeFormula}" font-family="{$vFontFamily}">
      <xsl:call-template name="getSpheresTranslation">
        <xsl:with-param name="pResourceName" select="$gReportNoInformationDet"/>
      </xsl:call-template>
    </fo:block>
  </xsl:template>
  <xsl:template name="displayMarket">
    <xsl:param name ="pMarketName"/>

    <xsl:variable name="vMarketRows" select="$gReportRows[Column[@ColumnName='MKTIDENT_SHORTIDENTIFIER']/text()=$pMarketName]"/>

    <xsl:variable name="vMarketRowsCopy1">
      <xsl:copy-of select="$vMarketRows"/>
    </xsl:variable>

    <xsl:variable name="vCurrencies">
      <xsl:copy-of select="msxsl:node-set($vMarketRowsCopy1)/Row/Column[(generate-id()=generate-id(key('kCurrencyKey',text())))]"/>
    </xsl:variable>

    <xsl:variable name="vMarketRowsCopy2">
      <xsl:copy-of select="$vMarketRows"/>
    </xsl:variable>

    <xsl:variable name="vBooks">
      <xsl:copy-of select="msxsl:node-set($vMarketRowsCopy2)/Row/Column[(generate-id()=generate-id(key('kBookKey',text())))]"/>
    </xsl:variable>

    <xsl:for-each select="msxsl:node-set($vCurrencies)/Column">
      <xsl:sort select="text()" data-type="text"/>

      <xsl:variable name="vCurrency" select="text()"/>

      <xsl:call-template name="displayCurrency">
        <xsl:with-param name="pMarketName" select="$pMarketName" />
        <xsl:with-param name="pCurrency" select="$vCurrency" />
        <xsl:with-param name="pBooks" select="$vBooks" />
      </xsl:call-template>

      <xsl:if test="position() != last()">
        <fo:block break-after='page'/>
      </xsl:if>

    </xsl:for-each>

  </xsl:template>
  <xsl:template name="displayCurrency">
    <xsl:param name ="pMarketName"/>
    <xsl:param name ="pCurrency"/>
    <xsl:param name ="pBooks"/>

    <fo:block linefeed-treatment="preserve">
      <xsl:call-template name="displayBreakline"/>
      <xsl:call-template name="displayBreakline"/>
    </fo:block>

    <fo:block linefeed-treatment="preserve">
      <fo:table table-layout="fixed" >
        <xsl:call-template name ="createHeader2Columns"/>
        <fo:table-body>
          <fo:table-row height="{$vRowHeight}cm"  text-align="left"
                        font-size="{$vFontSizeHeader}" color="black" font-family="{$vFontFamily}">
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}">
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'TS-MARKET'"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}" font-weight="bold" >
              <fo:block border="{$vBorder}">
                <xsl:value-of select="$pMarketName"/>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
          <fo:table-row height="{$vRowHeight}cm"  text-align="left" font-size="{$vFontSizeHeader}" color="black" font-family="{$vFontFamily}">
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}">
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'Report-Currency'"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}" font-weight="bold" >
              <fo:block border="{$vBorder}">
                <xsl:value-of select="$pCurrency"/>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>

          <xsl:call-template name ="displayDates"/>
        </fo:table-body>
      </fo:table>
    </fo:block>

    <fo:block linefeed-treatment="preserve">
      <xsl:call-template name="displayBreakline"/>
    </fo:block>

    <xsl:variable name="vCurrencyRow" select="$gReportRows[Column[@ColumnName='MKTIDENT_SHORTIDENTIFIER' and text()=$pMarketName] and Column[@ColumnName='FLOWS_CUR_UNIT' and text()=$pCurrency]]"/>
    <xsl:variable name="vBook1Row" select="$vCurrencyRow[Column[@ColumnName='B_ALLOC_P_IDENTIFIER' and text()=msxsl:node-set($pBooks)/Column[1]/text()]]"/>
    <xsl:variable name="vBook2Row" select="$vCurrencyRow[Column[@ColumnName='B_ALLOC_P_IDENTIFIER' and text()=msxsl:node-set($pBooks)/Column[2]/text()]]"/>
    <xsl:variable name="vBook3Row" select="$vCurrencyRow[Column[@ColumnName='B_ALLOC_P_IDENTIFIER' and text()=msxsl:node-set($pBooks)/Column[3]/text()]]"/>
    <xsl:variable name="vBook4Row" select="$vCurrencyRow[Column[@ColumnName='B_ALLOC_P_IDENTIFIER' and text()=msxsl:node-set($pBooks)/Column[4]/text()]]"/>
    <xsl:variable name="vBook5Row" select="$vCurrencyRow[Column[@ColumnName='B_ALLOC_P_IDENTIFIER' and text()=msxsl:node-set($pBooks)/Column[5]/text()]]"/>

    <!-- Header 5° block (Column Titles)-->
    <fo:block linefeed-treatment="preserve">
      <fo:table table-layout="fixed" >
        <xsl:call-template name="createBodyColumns"/>
        <fo:table-header>
          <xsl:call-template name ="displayColumnTitles">
            <xsl:with-param name="pBook1Row" select="$vBook1Row"/>
            <xsl:with-param name="pBook2Row" select="$vBook2Row"/>
            <xsl:with-param name="pBook3Row" select="$vBook3Row"/>
            <xsl:with-param name="pBook4Row" select="$vBook4Row"/>
            <xsl:with-param name="pBook5Row" select="$vBook5Row"/>
          </xsl:call-template>
        </fo:table-header>
        <fo:table-body>
          <xsl:call-template name ="displayDetailedRow">
            <xsl:with-param name="pMarketName" select="$pMarketName" />
            <xsl:with-param name="pBook1Row" select="$vBook1Row"/>
            <xsl:with-param name="pBook2Row" select="$vBook2Row"/>
            <xsl:with-param name="pBook3Row" select="$vBook3Row"/>
            <xsl:with-param name="pBook4Row" select="$vBook4Row"/>
            <xsl:with-param name="pBook5Row" select="$vBook5Row"/>
          </xsl:call-template>
        </fo:table-body>
      </fo:table>
    </fo:block>
  </xsl:template>

  <!-- draws the column titles of report-->
  <xsl:template name="displayDates">
    <fo:table-row height="{$vRowHeight}cm" text-align="left"
                  font-size="{$vFontSizeHeader}" color="black" font-family="{$vFontFamily}">
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'TS-ClearingDate'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$vBorder}" font-weight="bold" >
        <fo:block border="{$vBorder}">
          <xsl:value-of select="$gDtBusiness"/>
        </fo:block>
      </fo:table-cell>
    </fo:table-row>
    <xsl:call-template name="displayPublished"/>
  </xsl:template>
  <xsl:template name="displayPublished">
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <fo:table-row height="{$vRowHeight}cm"  text-align="left" font-size="{$vFontSizeHeader}" color="black" font-family="{$vFontFamily}">
      <fo:table-cell border="{$vBorder}" display-align="before" text-align="left" font-size="{$vFontSizeHeader}">
        <fo:block border="{$vBorder}">
          <!--Affichage de le ressource <Edité le >-->
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'TS-Published'" />
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$vBorder}" display-align="before" text-align="left" font-size="{$vFontSizeHeader}" font-weight="bold" >
        <fo:block border="{$vBorder}">
          <!--Affichage de la date de création du report-->
          <xsl:call-template name="format-date">
            <xsl:with-param name="xsd-date-time" select="$gCreationTimestamp" />
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
          <!--//-->
          <xsl:text> </xsl:text>
          <fo:inline font-weight="normal">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'TS-PublishedAt'" />
              <xsl:with-param name="pCulture" select="$pCulture" />
            </xsl:call-template>
          </fo:inline>
          <xsl:text> </xsl:text>
          <xsl:call-template name="format-time2">
            <xsl:with-param name="xsd-date-time" select="substring-after($gCreationTimestamp, 'T')" />
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
    </fo:table-row>
  </xsl:template>

  <!-- draws the column titles of report-->
  <xsl:template name="displayColumnTitles">
    <xsl:param name="pBook1Row"/>
    <xsl:param name="pBook2Row"/>
    <xsl:param name="pBook3Row"/>
    <xsl:param name="pBook4Row"/>
    <xsl:param name="pBook5Row"/>

    <fo:table-row background-color="{$gHdrCellBackgroundColor}"
                  display-align="center" font-weight="bold" color="black" font-family="{$vFontFamily}"
                  font-size="{$vFontSizeColumnTitles}" text-align="center">

      <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" text-align="left">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'CSHBAL-Designation'" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-Product'" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="DisplayBookTitle">
            <xsl:with-param name="pBookRow" select="$pBook1Row" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="DisplayBookTitle">
            <xsl:with-param name="pBookRow" select="$pBook2Row" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="DisplayBookTitle">
            <xsl:with-param name="pBookRow" select="$pBook3Row" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="DisplayBookTitle">
            <xsl:with-param name="pBookRow" select="$pBook4Row" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="DisplayBookTitle">
            <xsl:with-param name="pBookRow" select="$pBook5Row" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'TS-Total'" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
    </fo:table-row>
  </xsl:template>
  <xsl:template name="DisplayBookTitle">
    <xsl:param name ="pBookRow"/>

    <xsl:if test="$pBookRow">
      <xsl:choose>
        <xsl:when test="string-length($pBookRow/Column[@ColumnName='B_ALLOC_P_DISPLAYNAME']/text()) > 0">
          <xsl:value-of select="$pBookRow/Column[@ColumnName='B_ALLOC_P_DISPLAYNAME']/text()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$pBookRow/Column[@ColumnName='B_ALLOC_P_IDENTIFIER']/text()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
  </xsl:template>
  <xsl:template name ="displayDetailedRow">
    <xsl:param name="pBook1Row"/>
    <xsl:param name="pBook2Row"/>
    <xsl:param name="pBook3Row"/>
    <xsl:param name="pBook4Row"/>
    <xsl:param name="pBook5Row"/>

    <xsl:variable name="vBook1Amounts">
      <xsl:call-template name ="GetBookAmounts">
        <xsl:with-param name="pBookRow" select="$pBook1Row" />
        <xsl:with-param name="pNumber" select="'1'" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vBook2Amounts">
      <xsl:call-template name ="GetBookAmounts">
        <xsl:with-param name="pBookRow" select="$pBook2Row" />
        <xsl:with-param name="pNumber" select="'2'" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vBook3Amounts">
      <xsl:call-template name ="GetBookAmounts">
        <xsl:with-param name="pBookRow" select="$pBook3Row" />
        <xsl:with-param name="pNumber" select="'3'" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vBook4Amounts">
      <xsl:call-template name ="GetBookAmounts">
        <xsl:with-param name="pBookRow" select="$pBook4Row" />
        <xsl:with-param name="pNumber" select="'4'" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vBook5Amounts">
      <xsl:call-template name ="GetBookAmounts">
        <xsl:with-param name="pBookRow" select="$pBook5Row" />
        <xsl:with-param name="pNumber" select="'5'" />
      </xsl:call-template>
    </xsl:variable>

    <!-- Solde espèce précédent -->
    <xsl:call-template name ="displayColumnData">
      <xsl:with-param name="pAmountDesignation">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'CSHBAL-cashDayBefore'" />
        </xsl:call-template>
      </xsl:with-param>
      <xsl:with-param name="pProduct" select="''" />
      <xsl:with-param name="pAmountBook1" select="msxsl:node-set($vBook1Amounts)/Book/@PREVCASHBALANCE" />
      <xsl:with-param name="pAmountBook2" select="msxsl:node-set($vBook2Amounts)/Book/@PREVCASHBALANCE" />
      <xsl:with-param name="pAmountBook3" select="msxsl:node-set($vBook3Amounts)/Book/@PREVCASHBALANCE" />
      <xsl:with-param name="pAmountBook4" select="msxsl:node-set($vBook4Amounts)/Book/@PREVCASHBALANCE" />
      <xsl:with-param name="pAmountBook5" select="msxsl:node-set($vBook5Amounts)/Book/@PREVCASHBALANCE" />
    </xsl:call-template>
    <!-- Mouvements espèces -->
    <xsl:call-template name ="displayColumnData">
      <xsl:with-param name="pAmountDesignation">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'CSHBAL-cashMouvements'" />
        </xsl:call-template>
      </xsl:with-param>
      <xsl:with-param name="pProduct" select="''" />
      <xsl:with-param name="pAmountBook1" select="msxsl:node-set($vBook1Amounts)/Book/@CASHTRANSFER" />
      <xsl:with-param name="pAmountBook2" select="msxsl:node-set($vBook2Amounts)/Book/@CASHTRANSFER" />
      <xsl:with-param name="pAmountBook3" select="msxsl:node-set($vBook3Amounts)/Book/@CASHTRANSFER" />
      <xsl:with-param name="pAmountBook4" select="msxsl:node-set($vBook4Amounts)/Book/@CASHTRANSFER" />
      <xsl:with-param name="pAmountBook5" select="msxsl:node-set($vBook5Amounts)/Book/@CASHTRANSFER" />

    </xsl:call-template>
    <!-- Solde à Nouveau -->
    <xsl:call-template name ="displayColumnData">
      <xsl:with-param name="pAmountDesignation">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'Report-TodayCashBalance'" />
        </xsl:call-template>
      </xsl:with-param>
      <xsl:with-param name="pProduct" select="''" />
      <xsl:with-param name="pAmountBook1" select="msxsl:node-set($vBook1Amounts)/Book/@TODAYCASHBALANCE" />
      <xsl:with-param name="pAmountBook2" select="msxsl:node-set($vBook2Amounts)/Book/@TODAYCASHBALANCE" />
      <xsl:with-param name="pAmountBook3" select="msxsl:node-set($vBook3Amounts)/Book/@TODAYCASHBALANCE" />
      <xsl:with-param name="pAmountBook4" select="msxsl:node-set($vBook4Amounts)/Book/@TODAYCASHBALANCE" />
      <xsl:with-param name="pAmountBook5" select="msxsl:node-set($vBook5Amounts)/Book/@TODAYCASHBALANCE" />
      <xsl:with-param name ="pIsSubTotal" select="true()"/>
    </xsl:call-template>
    <!-- Appels de marge -->
    <xsl:call-template name ="displayColumnData">
      <xsl:with-param name="pAmountDesignation">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'CSHBAL-variationMargin'" />
        </xsl:call-template>
      </xsl:with-param>
      <xsl:with-param name="pProduct" select="''" />
      <xsl:with-param name="pAmountBook1" select="msxsl:node-set($vBook1Amounts)/Book/@VARMARGINAMOUNT" />
      <xsl:with-param name="pAmountBook2" select="msxsl:node-set($vBook2Amounts)/Book/@VARMARGINAMOUNT" />
      <xsl:with-param name="pAmountBook3" select="msxsl:node-set($vBook3Amounts)/Book/@VARMARGINAMOUNT" />
      <xsl:with-param name="pAmountBook4" select="msxsl:node-set($vBook4Amounts)/Book/@VARMARGINAMOUNT" />
      <xsl:with-param name="pAmountBook5" select="msxsl:node-set($vBook5Amounts)/Book/@VARMARGINAMOUNT" />
    </xsl:call-template>
    <!-- Primes -->
    <xsl:call-template name ="displayColumnData">
      <xsl:with-param name="pAmountDesignation">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'CSHBAL-optionPremium'" />
        </xsl:call-template>
      </xsl:with-param>
      <xsl:with-param name="pProduct" select="''" />
      <xsl:with-param name="pAmountBook1" select="msxsl:node-set($vBook1Amounts)/Book/@PREMIUMAMOUNT" />
      <xsl:with-param name="pAmountBook2" select="msxsl:node-set($vBook2Amounts)/Book/@PREMIUMAMOUNT" />
      <xsl:with-param name="pAmountBook3" select="msxsl:node-set($vBook3Amounts)/Book/@PREMIUMAMOUNT" />
      <xsl:with-param name="pAmountBook4" select="msxsl:node-set($vBook4Amounts)/Book/@PREMIUMAMOUNT" />
      <xsl:with-param name="pAmountBook5" select="msxsl:node-set($vBook5Amounts)/Book/@PREMIUMAMOUNT" />
    </xsl:call-template>
    <!-- Cash settlement -->
    <xsl:call-template name ="displayColumnData">
      <xsl:with-param name="pAmountDesignation">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'CSHBAL-cashSettlement'" />
        </xsl:call-template>
      </xsl:with-param>
      <xsl:with-param name="pProduct" select="''" />
      <xsl:with-param name="pAmountBook1" select="msxsl:node-set($vBook1Amounts)/Book/@CASHSETTLEMENT" />
      <xsl:with-param name="pAmountBook2" select="msxsl:node-set($vBook2Amounts)/Book/@CASHSETTLEMENT" />
      <xsl:with-param name="pAmountBook3" select="msxsl:node-set($vBook3Amounts)/Book/@CASHSETTLEMENT" />
      <xsl:with-param name="pAmountBook4" select="msxsl:node-set($vBook4Amounts)/Book/@CASHSETTLEMENT" />
      <xsl:with-param name="pAmountBook5" select="msxsl:node-set($vBook5Amounts)/Book/@CASHSETTLEMENT" />
    </xsl:call-template>
    <!-- Total Liquidation/Prime -->
    <xsl:call-template name ="displayColumnData">
      <xsl:with-param name="pAmountDesignation">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'Report-VariationMarginPremium'" />
        </xsl:call-template>
      </xsl:with-param>
      <xsl:with-param name="pProduct" select="''" />
      <xsl:with-param name="pAmountBook1" select="msxsl:node-set($vBook1Amounts)/Book/@OFFSETTINGPREMIUM" />
      <xsl:with-param name="pAmountBook2" select="msxsl:node-set($vBook2Amounts)/Book/@OFFSETTINGPREMIUM" />
      <xsl:with-param name="pAmountBook3" select="msxsl:node-set($vBook3Amounts)/Book/@OFFSETTINGPREMIUM" />
      <xsl:with-param name="pAmountBook4" select="msxsl:node-set($vBook4Amounts)/Book/@OFFSETTINGPREMIUM" />
      <xsl:with-param name="pAmountBook5" select="msxsl:node-set($vBook5Amounts)/Book/@OFFSETTINGPREMIUM" />
      <xsl:with-param name ="pIsSubTotal" select="true()"/>
    </xsl:call-template>
    <!-- Garanties titres précédent -->
    <xsl:call-template name ="displayColumnData">
      <xsl:with-param name="pAmountDesignation">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'Report-PreviousCollateralUsed'" />
        </xsl:call-template>
      </xsl:with-param>
      <xsl:with-param name="pProduct" select="''" />
      <xsl:with-param name="pAmountBook1" select="msxsl:node-set($vBook1Amounts)/Book/@COLLATERAL_U_PREV" />
      <xsl:with-param name="pAmountBook2" select="msxsl:node-set($vBook2Amounts)/Book/@COLLATERAL_U_PREV" />
      <xsl:with-param name="pAmountBook3" select="msxsl:node-set($vBook3Amounts)/Book/@COLLATERAL_U_PREV" />
      <xsl:with-param name="pAmountBook4" select="msxsl:node-set($vBook4Amounts)/Book/@COLLATERAL_U_PREV" />
      <xsl:with-param name="pAmountBook5" select="msxsl:node-set($vBook5Amounts)/Book/@COLLATERAL_U_PREV" />
    </xsl:call-template>
    <!-- Encours déposit précédent -->
    <xsl:call-template name ="displayColumnData">
      <xsl:with-param name="pAmountDesignation">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'Report-PreviousMarginRequirement'" />
        </xsl:call-template>
      </xsl:with-param>
      <xsl:with-param name="pProduct" select="''" />
      <xsl:with-param name="pAmountBook1" select="msxsl:node-set($vBook1Amounts)/Book/@MARGINREQ_PREV" />
      <xsl:with-param name="pAmountBook2" select="msxsl:node-set($vBook2Amounts)/Book/@MARGINREQ_PREV" />
      <xsl:with-param name="pAmountBook3" select="msxsl:node-set($vBook3Amounts)/Book/@MARGINREQ_PREV" />
      <xsl:with-param name="pAmountBook4" select="msxsl:node-set($vBook4Amounts)/Book/@MARGINREQ_PREV" />
      <xsl:with-param name="pAmountBook5" select="msxsl:node-set($vBook5Amounts)/Book/@MARGINREQ_PREV" />
    </xsl:call-template>
    <!-- Garanties titres jour -->
    <xsl:call-template name ="displayColumnData">
      <xsl:with-param name="pAmountDesignation">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'Report-CollateralUsed'" />
        </xsl:call-template>
      </xsl:with-param>
      <xsl:with-param name="pProduct" select="''" />
      <xsl:with-param name="pAmountBook1" select="msxsl:node-set($vBook1Amounts)/Book/@COLLATERAL_U" />
      <xsl:with-param name="pAmountBook2" select="msxsl:node-set($vBook2Amounts)/Book/@COLLATERAL_U" />
      <xsl:with-param name="pAmountBook3" select="msxsl:node-set($vBook3Amounts)/Book/@COLLATERAL_U" />
      <xsl:with-param name="pAmountBook4" select="msxsl:node-set($vBook4Amounts)/Book/@COLLATERAL_U" />
      <xsl:with-param name="pAmountBook5" select="msxsl:node-set($vBook5Amounts)/Book/@COLLATERAL_U" />
    </xsl:call-template>
    <!-- Encours déposit jour -->
    <xsl:call-template name ="displayColumnData">
      <xsl:with-param name="pAmountDesignation">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'Report-MarginRequirement'" />
        </xsl:call-template>
      </xsl:with-param>
      <xsl:with-param name="pProduct" select="''" />
      <xsl:with-param name="pAmountBook1" select="msxsl:node-set($vBook1Amounts)/Book/@MARGINREQ" />
      <xsl:with-param name="pAmountBook2" select="msxsl:node-set($vBook2Amounts)/Book/@MARGINREQ" />
      <xsl:with-param name="pAmountBook3" select="msxsl:node-set($vBook3Amounts)/Book/@MARGINREQ" />
      <xsl:with-param name="pAmountBook4" select="msxsl:node-set($vBook4Amounts)/Book/@MARGINREQ" />
      <xsl:with-param name="pAmountBook5" select="msxsl:node-set($vBook5Amounts)/Book/@MARGINREQ" />
    </xsl:call-template>
    <!-- Appel/Restitution de déposit -->
    <xsl:call-template name ="displayColumnData">
      <xsl:with-param name="pAmountDesignation">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'CSHBAL-returnCallDeposit'" />
        </xsl:call-template>
      </xsl:with-param>
      <xsl:with-param name="pProduct" select="''" />
      <xsl:with-param name="pAmountBook1" select="msxsl:node-set($vBook1Amounts)/Book/@MARGINCALL" />
      <xsl:with-param name="pAmountBook2" select="msxsl:node-set($vBook2Amounts)/Book/@MARGINCALL" />
      <xsl:with-param name="pAmountBook3" select="msxsl:node-set($vBook3Amounts)/Book/@MARGINCALL" />
      <xsl:with-param name="pAmountBook4" select="msxsl:node-set($vBook4Amounts)/Book/@MARGINCALL" />
      <xsl:with-param name="pAmountBook5" select="msxsl:node-set($vBook5Amounts)/Book/@MARGINCALL" />
      <xsl:with-param name ="pIsSubTotal" select="true()"/>
    </xsl:call-template>
    <!-- Mouvement à Prévoir -->
    <xsl:call-template name ="displayColumnData">
      <xsl:with-param name="pAmountDesignation">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'Report-Movement'" />
        </xsl:call-template>
      </xsl:with-param>
      <xsl:with-param name="pProduct" select="''" />
      <xsl:with-param name="pAmountBook1" select="msxsl:node-set($vBook1Amounts)/Book/@DAILYBALANCE" />
      <xsl:with-param name="pAmountBook2" select="msxsl:node-set($vBook2Amounts)/Book/@DAILYBALANCE" />
      <xsl:with-param name="pAmountBook3" select="msxsl:node-set($vBook3Amounts)/Book/@DAILYBALANCE" />
      <xsl:with-param name="pAmountBook4" select="msxsl:node-set($vBook4Amounts)/Book/@DAILYBALANCE" />
      <xsl:with-param name="pAmountBook5" select="msxsl:node-set($vBook5Amounts)/Book/@DAILYBALANCE" />
      <xsl:with-param name ="pIsSubTotal" select="true()"/>
    </xsl:call-template>
    <!-- Solde à Reporter -->
    <xsl:call-template name ="displayColumnData">
      <xsl:with-param name="pAmountDesignation">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'Report-BalanceCarriedForward'" />
        </xsl:call-template>
      </xsl:with-param>
      <xsl:with-param name="pProduct" select="''" />
      <xsl:with-param name="pAmountBook1" select="msxsl:node-set($vBook1Amounts)/Book/@CASHBALANCE" />
      <xsl:with-param name="pAmountBook2" select="msxsl:node-set($vBook2Amounts)/Book/@CASHBALANCE" />
      <xsl:with-param name="pAmountBook3" select="msxsl:node-set($vBook3Amounts)/Book/@CASHBALANCE" />
      <xsl:with-param name="pAmountBook4" select="msxsl:node-set($vBook4Amounts)/Book/@CASHBALANCE" />
      <xsl:with-param name="pAmountBook5" select="msxsl:node-set($vBook5Amounts)/Book/@CASHBALANCE" />
      <xsl:with-param name ="pIsTotal" select="true()"/>
    </xsl:call-template>

  </xsl:template>
  <xsl:template name="GetBookAmounts">
    <xsl:param name ="pBookRow"/>
    <xsl:param name ="pNumber"/>

    <xsl:variable name="vPREVCASHBALANCE">
      <xsl:call-template name ="GetAmount">
        <xsl:with-param name="pBookRow" select="$pBookRow" />
        <xsl:with-param name="pAmountName" select="'FLOWS_CUR_PREVCASHBALANCE'" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vCASHTRANSFER">
      <xsl:call-template name ="GetAmount">
        <xsl:with-param name="pBookRow" select="$pBookRow" />
        <xsl:with-param name="pAmountName" select="'FLOWS_CUR_CASHTRANSFER'" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vTODAYCASHBALANCE">
      <xsl:choose>
        <xsl:when test="$pBookRow">
          <xsl:value-of select="$vPREVCASHBALANCE + $vCASHTRANSFER" />
        </xsl:when>
        <xsl:otherwise></xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vVARMARGINAMOUNT">
      <xsl:call-template name ="GetAmount">
        <xsl:with-param name="pBookRow" select="$pBookRow" />
        <xsl:with-param name="pAmountName" select="'FLOWS_CUR_VARMARGINAMOUNT'" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vPREMIUMAMOUNT">
      <xsl:call-template name ="GetAmount">
        <xsl:with-param name="pBookRow" select="$pBookRow" />
        <xsl:with-param name="pAmountName" select="'FLOWS_CUR_PREMIUMAMOUNT'" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vCASHSETTLEMENT">
      <xsl:call-template name ="GetAmount">
        <xsl:with-param name="pBookRow" select="$pBookRow" />
        <xsl:with-param name="pAmountName" select="'FLOWS_CUR_CASHSETTLEMENT'" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vOFFSETTINGPREMIUM">
      <xsl:choose>
        <xsl:when test="$pBookRow">
          <xsl:value-of select="$vVARMARGINAMOUNT + $vPREMIUMAMOUNT + $vCASHSETTLEMENT" />
        </xsl:when>
        <xsl:otherwise></xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vMARGINREQ_PREV">
      <xsl:call-template name ="GetAmount">
        <xsl:with-param name="pBookRow" select="$pBookRow" />
        <xsl:with-param name="pAmountName" select="'FLOWS_CUR_MARGINREQ_PREV'" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vMARGINREQ">
      <xsl:call-template name ="GetAmount">
        <xsl:with-param name="pBookRow" select="$pBookRow" />
        <xsl:with-param name="pAmountName" select="'FLOWS_CUR_MARGINREQ'" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vMARGINCALL">
      <xsl:call-template name ="GetAmount">
        <xsl:with-param name="pBookRow" select="$pBookRow" />
        <xsl:with-param name="pAmountName" select="'FLOWS_CUR_MARGINCALL'" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vDAILYBALANCE">
      <xsl:call-template name ="GetAmount">
        <xsl:with-param name="pBookRow" select="$pBookRow" />
        <xsl:with-param name="pAmountName" select="'FLOWS_CUR_DAILYBALANCE'" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vCASHBALANCE">
      <xsl:call-template name ="GetAmount">
        <xsl:with-param name="pBookRow" select="$pBookRow" />
        <xsl:with-param name="pAmountName" select="'FLOWS_CUR_CASHBALANCE'" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vCOLLATERAL_U">
      <xsl:call-template name ="GetAmount">
        <xsl:with-param name="pBookRow" select="$pBookRow" />
        <xsl:with-param name="pAmountName" select="'FLOWS_CUR_COLLATERAL_U'" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vCOLLATERAL_U_PREV">
      <xsl:call-template name ="GetAmount">
        <xsl:with-param name="pBookRow" select="$pBookRow" />
        <xsl:with-param name="pAmountName" select="'FLOWS_CUR_COLLATERAL_U_PREV'" />
      </xsl:call-template>
    </xsl:variable>

    <Book PREVCASHBALANCE="{$vPREVCASHBALANCE}"
          CASHTRANSFER="{$vCASHTRANSFER}"
          TODAYCASHBALANCE="{$vTODAYCASHBALANCE}"
          VARMARGINAMOUNT="{$vVARMARGINAMOUNT}"
          PREMIUMAMOUNT="{$vPREMIUMAMOUNT}"
          CASHSETTLEMENT="{$vCASHSETTLEMENT}"
          OFFSETTINGPREMIUM="{$vOFFSETTINGPREMIUM}"
          MARGINREQ_PREV="{$vMARGINREQ_PREV}"
          MARGINREQ="{$vMARGINREQ}"
          MARGINCALL="{$vMARGINCALL}"
          DAILYBALANCE="{$vDAILYBALANCE}"
          CASHBALANCE="{$vCASHBALANCE}"
          COLLATERAL_U="{$vCOLLATERAL_U}"
          COLLATERAL_U_PREV="{$vCOLLATERAL_U_PREV}"/>

  </xsl:template>
  <xsl:template name="GetAmount">
    <xsl:param name ="pBookRow"/>
    <xsl:param name ="pAmountName"/>

    <xsl:choose>
      <xsl:when test="$pBookRow">
        <xsl:value-of select="$pBookRow/Column[@ColumnName=$pAmountName]/text()" />
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!-- draws the column titles of report-->
  <xsl:template name="displayColumnData">
    <xsl:param name ="pAmountDesignation"/>
    <xsl:param name ="pProduct"/>
    <xsl:param name ="pAmountBook1"/>
    <xsl:param name ="pAmountBook2"/>
    <xsl:param name ="pAmountBook3"/>
    <xsl:param name ="pAmountBook4"/>
    <xsl:param name ="pAmountBook5"/>
    <xsl:param name ="pIsSubTotal" select="false()"/>
    <xsl:param name ="pIsTotal" select="false()"/>

    <xsl:variable name="vAmountBook1">
      <xsl:choose>
        <xsl:when test="string-length($pAmountBook1) > 0">
          <xsl:value-of select="$pAmountBook1"/>
        </xsl:when>
        <xsl:otherwise>0</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vAmountBook2">
      <xsl:choose>
        <xsl:when test="string-length($pAmountBook2) > 0">
          <xsl:value-of select="$pAmountBook2"/>
        </xsl:when>
        <xsl:otherwise>0</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vAmountBook3">
      <xsl:choose>
        <xsl:when test="string-length($pAmountBook3) > 0">
          <xsl:value-of select="$pAmountBook3"/>
        </xsl:when>
        <xsl:otherwise>0</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vAmountBook4">
      <xsl:choose>
        <xsl:when test="string-length($pAmountBook4) > 0">
          <xsl:value-of select="$pAmountBook4"/>
        </xsl:when>
        <xsl:otherwise>0</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vAmountBook5">
      <xsl:choose>
        <xsl:when test="string-length($pAmountBook5) > 0">
          <xsl:value-of select="$pAmountBook5"/>
        </xsl:when>
        <xsl:otherwise>0</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vTotal">
      <xsl:value-of select="$vAmountBook1 + $vAmountBook2 + $vAmountBook3 + $vAmountBook4 + $vAmountBook5"/>
    </xsl:variable>

    <fo:table-row display-align="center" color="black"
                  font-family="{$vFontFamily}" font-size="{$vFontSizeColumnTitles}">

      <xsl:if test="$pIsSubTotal or $pIsTotal">
        <xsl:attribute name="font-weight">bold</xsl:attribute>
      </xsl:if>

      <xsl:if test="$pIsTotal">
        <xsl:attribute name="background-color">
          <xsl:value-of select="$gTotalBackgroundColor"/>
        </xsl:attribute>
      </xsl:if>

      <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}">
        <xsl:if test="$pIsSubTotal or $pIsTotal">
          <xsl:attribute name="text-align">center</xsl:attribute>
        </xsl:if>

        <xsl:if test="$pIsTotal">
          <xsl:attribute name="font-style">italic</xsl:attribute>
          <xsl:attribute name="number-columns-spanned">2</xsl:attribute>
        </xsl:if>

        <fo:block border="{$vBorder}">
          <xsl:value-of select="$pAmountDesignation"/>
        </fo:block>
      </fo:table-cell>
      <xsl:if test="$pIsTotal = false()">
        <fo:table-cell border="{$varTableBorder}" text-align="center" padding="{$gCellPadding}">
          <fo:block border="{$vBorder}">
            <xsl:value-of select="$pProduct"/>
          </fo:block>
        </fo:table-cell>
      </xsl:if>
      <fo:table-cell border="{$varTableBorder}" text-align="right" padding="{$gCellPadding}">
        <xsl:call-template name ="DisplayAmount">
          <xsl:with-param name ="pAmount" select="$pAmountBook1"/>
        </xsl:call-template>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" text-align="right" padding="{$gCellPadding}">
        <xsl:call-template name ="DisplayAmount">
          <xsl:with-param name ="pAmount" select="$pAmountBook2"/>
        </xsl:call-template>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" text-align="right" padding="{$gCellPadding}">
        <xsl:call-template name ="DisplayAmount">
          <xsl:with-param name ="pAmount" select="$pAmountBook3"/>
        </xsl:call-template>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" text-align="right" padding="{$gCellPadding}">
        <xsl:call-template name ="DisplayAmount">
          <xsl:with-param name ="pAmount" select="$pAmountBook4"/>
        </xsl:call-template>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" text-align="right" padding="{$gCellPadding}">
        <xsl:call-template name ="DisplayAmount">
          <xsl:with-param name ="pAmount" select="$pAmountBook5"/>
        </xsl:call-template>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" text-align="right" padding="{$gCellPadding}">
        <xsl:call-template name ="DisplayAmount">
          <xsl:with-param name ="pAmount" select="$vTotal"/>
        </xsl:call-template>
      </fo:table-cell>
    </fo:table-row>
  </xsl:template>
  <xsl:template name="DisplayAmount">
    <xsl:param name ="pAmount"/>

    <xsl:if test="string-length($pAmount) > 0">
      <fo:block border="{$vBorder}">
        <xsl:call-template name ="getAmount">
          <xsl:with-param name ="pNode" select="$pAmount"/>
        </xsl:call-template>
      </fo:block>
    </xsl:if>
  </xsl:template>

  <!-- =========================================================  -->
  <!-- Get section                                                -->
  <!-- contains variables and templates                           -->
  <!-- =========================================================  -->

  <xsl:template name ="getTableDisplayName">
    <xsl:param name ="pTableName"/>

    <xsl:choose>

      <xsl:when test ="$pTableName = 'FLOWSBYTRADE'">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'OTC_VIEW_FO_FLOW_BYTRADE'"/>
        </xsl:call-template>
      </xsl:when>

      <xsl:when test ="$pTableName = 'FLOWSBYASSET'">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'OTC_VIEW_FO_FLOW_BYASSET'"/>
        </xsl:call-template>
      </xsl:when>

      <xsl:when test ="$pTableName = 'FLOWSBYCURRENCY'">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'OTC_VIEW_FO_FLOW_BYCURRENCY'"/>
        </xsl:call-template>
      </xsl:when>

      <xsl:otherwise>
        <xsl:value-of select ="$pTableName"/>
      </xsl:otherwise>

    </xsl:choose>

  </xsl:template>

  <!-- returns formatted number with 2 decimal pattern using current culture-->
  <xsl:template name ="formatMoney">
    <xsl:param name ="pAmount"/>

    <xsl:if test ="$pAmount != ''">
      <xsl:call-template name="format-number">
        <xsl:with-param name="pAmount" select="$pAmount" />
        <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
      </xsl:call-template>
    </xsl:if>

  </xsl:template>

  <!-- return amount value from a specific node-->
  <xsl:template name ="getAmount">
    <xsl:param name ="pNode"/>
    <xsl:call-template name ="formatMoney">
      <xsl:with-param name ="pAmount" select ="$pNode"/>
    </xsl:call-template>
  </xsl:template>

  <!-- returns report date time stamp-->
  <xsl:template name ="getTimeStamp">
    <xsl:variable name ="vDateTimeStamp">
      <xsl:value-of select ="$gReportReferential/@timestamp"/>
    </xsl:variable>

    <xsl:call-template name="format-date">
      <xsl:with-param name="xsd-date-time" select="$vDateTimeStamp"/>
    </xsl:call-template>
    <xsl:text> </xsl:text>
    <xsl:call-template name="getSpheresTranslation">
      <xsl:with-param name="pResourceName" select="'TS-PublishedAt'" />
    </xsl:call-template>
    <xsl:text> </xsl:text>
    <xsl:call-template name="format-time">
      <xsl:with-param name="xsd-date-time" select="substring-after($vDateTimeStamp,'T')"/>
    </xsl:call-template>

  </xsl:template>

  <xsl:template name ="getClearingDate">
    <xsl:call-template name="format-date">
      <xsl:with-param name="xsd-date-time" select="//Data[@name='DTBUSINESS']"/>
    </xsl:call-template>
  </xsl:template>
  <xsl:template name ="getDate1">

    <xsl:call-template name="format-date">
      <xsl:with-param name="xsd-date-time" select="$gReportData[@name='DATE1']"/>
    </xsl:call-template>

  </xsl:template>
  <xsl:template name ="getDate2">

    <xsl:call-template name="format-date">
      <xsl:with-param name="xsd-date-time" select="$gReportData[@name='DATE2']"/>
    </xsl:call-template>

  </xsl:template>

  <!-- .......................................................................... -->
  <!--              Display Tools                                                 -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- Display_Legend                                   -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Report Legend                           
       ................................................ -->
  <xsl:template name="Display_Legend">
    <xsl:param name="pLegend"/>
    <xsl:param name="pResource"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <fo:block keep-with-previous="always">
      <fo:inline font-size="{$vFontSizeDet}" vertical-align="super" font-weight="bold" >
        <xsl:value-of select="$pLegend"/>
      </fo:inline>
      <fo:inline>
        <xsl:value-of select="' '"/>
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="$pResource" />
          <xsl:with-param name="pCulture" select="$pCulture" />
        </xsl:call-template>
      </fo:inline>
    </fo:block>
  </xsl:template>

  <!-- ................................................ -->
  <!-- DisplayData_Truncate                             -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Truncate date according to length 
       ................................................ -->
  <xsl:template name="DisplayData_Truncate">
    <xsl:param name="pData"/>
    <xsl:param name="pDataLength"/>

    <xsl:choose>
      <xsl:when test="($pDataLength > number('0')) and (string-length($pData) > $pDataLength)">
        <xsl:choose>
          <xsl:when test="$pDataLength > number('1')">
            <xsl:value-of select="substring(normalize-space($pData),1,$pDataLength - number('1'))"/>
            <xsl:value-of select="'...'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="substring(normalize-space($pData),1,1)"/>
            <xsl:value-of select="'...'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:copy-of select="$pData"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>