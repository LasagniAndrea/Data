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
  <xsl:include href="..\..\Library\Resource.xslt"/>
  <xsl:include href="..\..\Library\DtFunc.xslt"/>
  <xsl:include href="..\..\Library\NbrFunc.xslt" />
  <xsl:include href="..\..\Library\StrFunc.xslt" />
  <xsl:include href="..\..\Library\xsltsl\date-time.xsl"/>


  <xsl:variable name="vLowerLetters">abcdeéèfghijklmnopqrstuvwxyz</xsl:variable>
  <xsl:variable name="vUpperLetters">ABCDEEEFGHIJKLMNOPQRSTUVWXYZ</xsl:variable>

  <!-- ================================================================ -->
  <!-- Global Report settings                                           -->
  <!-- ================================================================ -->
  <xsl:variable name="gModel">
    <xsl:text>Opérations validées aujourd'hui par le BO</xsl:text>
  </xsl:variable>  
  <xsl:variable name="gReportTitle" select="'Liste des opérations validées Back-Office'"/>
  <xsl:variable name="gReportNoInformationDet" select="concat('Veuillez sélectionner le modèle de consultation ', &quot;&apos;&quot;, $gModel, &quot;&apos;&quot;, ' et disposer de données à afficher.')"/>

  <xsl:variable name="gReportReferential" select="Referential"/>
  <xsl:variable name="gReportRows" select="$gReportReferential/Row"/>
  <xsl:variable name="gCreationTimestamp">
    <xsl:value-of select="$gReportReferential/@timestamp"/>
  </xsl:variable>
  <!-- Display -->

  <xsl:variable name="gHdrCellBackgroundColor">#e3e3e3</xsl:variable>
  <xsl:variable name="gCellPadding">2pt</xsl:variable>
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

  <xsl:variable name="varImgLogo">
    <xsl:value-of select="concat('sql(select top(1) IDENTIFIER, LOLOGO from dbo.ACTOR a inner join dbo.ACTORROLE ar on ar.IDA = a.IDA where IDROLEACTOR =', &quot;&apos;&quot;, $vEntity, &quot;&apos;&quot;, 'and a.IDENTIFIER !=', &quot;&apos;&quot;, 'EFSLIC', &quot;&apos;&quot;,  ')')" />
  </xsl:variable>

  <!-- font ================================================================================= -->
  <!-- sans-serif/Helvetica/Courier/Times New Roman/New York/serif/Gill/-->
  <xsl:variable name="vFontFamily">sans-serif</xsl:variable>
  <xsl:variable name="vFontSizeHeader">9pt</xsl:variable>
  <xsl:variable name="vFontSizeFormula">9pt</xsl:variable>
  <xsl:variable name="vRuleSize">0.5pt</xsl:variable>

  <!-- A4 page caracteristics ============================================================== -->
  <xsl:variable name="vPageA4LandscapePageHeight">29.7</xsl:variable>
  <xsl:variable name="vPageA4LandscapePageWidth">21</xsl:variable>
  <xsl:variable name="vPageA4LandscapeMargin">0.5</xsl:variable>

  <!-- Variables for A4 page format ====================================================== -->
  <xsl:variable name="vPageA4LandscapeLeftMargin">0.5</xsl:variable>
  <xsl:variable name="vPageA4LandscapeRightMargin">0.5</xsl:variable>
  <xsl:variable name="vPageA4LandscapeTopMargin">0.1</xsl:variable>

  <!-- Variables for Footer ====================================================== -->
  <xsl:variable name ="vFooterExtent">1</xsl:variable>

  <!-- Variables for Body ====================================================== -->
  <xsl:variable name ="vBodyLeftMargin">0.5</xsl:variable>
  <xsl:variable name ="vBodyBottomMargin">
    <xsl:value-of select ="$vFooterExtent+$vPageA4LandscapeMargin"/>
  </xsl:variable>

  <!-- Variables for Table ====================================================== -->
  <xsl:variable name="varTableBorder">
    0.1pt solid black
  </xsl:variable>

  <!-- Variables for table-cell ====================================================== -->
  <xsl:variable name ="vLogoHeight">1.35</xsl:variable>
  <xsl:variable name ="vRowHeightBreakline">0.4</xsl:variable>
  <xsl:variable name ="vRowHeight">0.6</xsl:variable>

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
      <fo:simple-page-master border-width="0mm" padding="0" margin-top="0.5cm" margin-bottom="0.5cm" margin-right="0.5cm" margin-left="0.6cm" page-width="21cm" page-height="29.7cm" master-name="Vertical">
        <fo:region-body border-width="0mm" padding="0" margin-top="1.5cm" margin-bottom="1.6cm" margin-right="0cm" margin-left="0cm" background-color="white" overflow="scroll" border="" region-name="PageBody"/>
        <fo:region-before background-color="white" region-name="PageHeader" precedence="true" extent="1cm"/>
        <fo:region-after border-width="0mm" padding="0" background-color="white" region-name="PageFooter" precedence="true" extent="1cm" display-align="after"/>
        <fo:region-start background-color="white" region-name="A4-vertical-start-rest" extent="0cm"/>
        <fo:region-end background-color="white" region-name="A4-vertical-end-rest" extent="0cm"/>
      </fo:simple-page-master>
    </fo:layout-master-set>
  </xsl:template>

  <xsl:template name="DisplayDocumentContent">
    <xsl:param name="pData"/>
    <xsl:param name="pRows"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <fo:page-sequence master-reference="Vertical" initial-page-number="1" force-page-count="no-force">

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
  
  <xsl:template name="createHeader3Columns">
    <xsl:variable name="vColumnWidth1">6</xsl:variable>
    <xsl:variable name="vColumnWidth2">6</xsl:variable>
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
  <xsl:template name="createBody2Columns">
    <fo:table-column column-width="{4}cm" column-number="01" />
    <fo:table-column column-width="{15}cm" column-number="02" />
  </xsl:template>
  <xsl:template name="create1Column">
    <fo:table-column column-width="20cm" column-number="01"/>
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
            <fo:table-cell border="{$vBorder}" font-size="7pt" text-align="right">
              <fo:block border="{$vBorder}">
                <!--<xsl:call-template name="displayBlankField"/>-->
                <!--Affichage de le ressource <Edité le >-->
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'TS-Published'" />
                  <xsl:with-param name="pCulture" select="$pCulture" />
                </xsl:call-template>
                <xsl:text> </xsl:text>
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
                <xsl:text>Powered by Spheres - © 2024 EFS</xsl:text>
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
                <xsl:value-of select="translate($gReportTitle, $vLowerLetters, $vUpperLetters)"/>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
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

  <xsl:template name="displayPageBodyDetail">
    <xsl:param name="pData"/>

    <xsl:variable name="vReportSubTitle" select="$gReportReferential/SubTitle/text()"/>
    <xsl:choose>
      <xsl:when test="msxsl:node-set($gReportRows) and contains($vReportSubTitle,$gModel)">

        <fo:block linefeed-treatment="preserve">
          <xsl:call-template name="displayBreakline"/>
          <xsl:call-template name="displayBreakline"/>
        </fo:block>

        <fo:block linefeed-treatment="preserve">
          <fo:table table-layout="fixed" >
            <fo:table-column column-width="1.50cm" column-number="01"/>
            <fo:table-column column-width="1.30cm" column-number="02"/>
            <fo:table-column column-width="1.6cm" column-number="03"/>
            <fo:table-column column-width="2cm" column-number="04"/>
            <fo:table-column column-width="2cm" column-number="05"/>
            <fo:table-column column-width="5.70cm" column-number="06"/>
            <fo:table-column column-width="4.10cm" column-number="07"/>
            <fo:table-column column-width="1.50cm" column-number="08"/>
            <fo:table-header>
              <fo:table-row background-color="{$gHdrCellBackgroundColor}"
                            display-align="center" font-weight="bold" color="black" font-family="{$vFontFamily}"
                            font-size="7pt" text-align="center">

                <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}">
                  <fo:block border="{$vBorder}">
                    Date
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}">
                  <fo:block border="{$vBorder}">
                    Heure
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}">
                  <fo:block border="{$vBorder}">
                    Trade
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}">
                  <fo:block border="{$vBorder}">
                    Acheteur
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}">
                  <fo:block border="{$vBorder}">
                    Vendeur
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}">
                  <fo:block border="{$vBorder}">
                    Nom
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}">
                  <fo:block border="{$vBorder}">
                    Description
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}">
                  <fo:block border="{$vBorder}">
                    Date valid.
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </fo:table-header>
            <fo:table-body>
              <xsl:for-each select="$gReportRows">
                <xsl:sort select="Column[@ColumnName='T_DTTRADE']/text()" data-type="text"/>
                <xsl:sort select="Column[@ColumnName='T_DTTIMESTAMP']/text()" data-type="text"/>

                <fo:table-row display-align="center" font-weight="normal" color="black" font-family="{$vFontFamily}"
                              font-size="7pt" text-align="center">

                  <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" text-align="center">
                    <fo:block border="{$vBorder}">
                      <xsl:variable name="vDate" select="Column[@ColumnName='T_DTTRADE']/text()"/>
                      <xsl:call-template name="format-shortdate_ddMMMyy">
                        <xsl:with-param name="year" select="substring($vDate , 1, 4)" />
                        <xsl:with-param name="month" select="substring($vDate , 6, 2)" />
                        <xsl:with-param name="day" select="substring($vDate , 9, 2)" />
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}">
                    <fo:block border="{$vBorder}">
                      <xsl:variable name="vTime" select="Column[@ColumnName='T_DTTIMESTAMP']/text()"/>
                      <xsl:call-template name="dt:format-date-time">
                        <xsl:with-param name="xsd-date-time" select="$vTime"/>
                        <xsl:with-param name="format" select="concat('%h', $separatorTime,'%M', $separatorTime,'%S')"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}">
                    <fo:block border="{$vBorder}">
                      <xsl:value-of select="Column[@ColumnName='T_IDENTIFIER']/text()"/>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" text-align="left">
                    <fo:block border="{$vBorder}">
                      <xsl:value-of select="Column[@ColumnName='ATABUYER_IDENTIFIER']/text()"/>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" text-align="left">
                    <fo:block border="{$vBorder}">
                      <xsl:value-of select="Column[@ColumnName='ATASELLER_IDENTIFIER']/text()"/>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" text-align="left">
                    <fo:block border="{$vBorder}">
                      <xsl:value-of select="Column[@ColumnName='T_DISPLAYNAME']/text()"/>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" text-align="left">
                    <fo:block border="{$vBorder}">
                      <xsl:value-of select="Column[@ColumnName='T_DESCRIPTION']/text()"/>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}">
                    <fo:block border="{$vBorder}">
                      <xsl:variable name="vDate" select="Column[@ColumnName='TSTCHECK_DTEFFECT']/text()"/>
                      <xsl:if test="string-length($vDate) >0">
                        <xsl:call-template name="format-shortdate_ddMMMyy">
                          <xsl:with-param name="year" select="substring($vDate , 1, 4)" />
                          <xsl:with-param name="month" select="substring($vDate , 6, 2)" />
                          <xsl:with-param name="day" select="substring($vDate , 9, 2)" />
                        </xsl:call-template>
                      </xsl:if>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>

              </xsl:for-each>
            </fo:table-body>
          </fo:table>
        </fo:block>
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

</xsl:stylesheet>