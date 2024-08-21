<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:dt="http://xsltsl.org/date-time"
  xmlns:fo="http://www.w3.org/1999/XSL/Format"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt">

  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml-fo;"/>


  <!--  
================================================================================================================
                                                     HISTORY OF THE MODIFICATIONS
Version 1.0.0.0 (Spheres 2.5.0.0)
 Date: 17/09/2010
 Author: RD
 Description: first version  
 
Version 1.0.0.1 (Spheres 2.6.3.0)
 Date: 26/06/2012
 Author: MF
 Description: 
 Enhancements
 - Grouped amounts, any group prefix by an header (using a row spanned table cell)
 - Subordinate tables, recongized details tables rendered differently
 - New display on screen for composed amounts using an available value together with an used one
 - Pagination, for each group we put a page break
 - ....
 
 Version 1.0.0.2 (Spheres 2.6.3.0)
  Date: 03/09/2012 [Ticket 18048]
  Author: MF
  Description: 
  Modifications
  - the account statement needs a larger width for the header containing the account and the account period, introducing vReceiverBookWidth
 
================================================================================================================
  -->

  <!--============================================================================================================ -->
  <!--                                                                                                             -->
  <!--                                       Display Templates                                                     -->
  <!--                                                                                                             -->
  <!--============================================================================================================ -->
  <!-- .................................. -->
  <!--   Page Caracteristics              -->
  <!-- .................................. -->
  <!--DisplaySetPagesCaracteristics template-->
  <xsl:template name="SetPagesCaracteristics">
    <fo:layout-master-set>
      <xsl:variable name="vBodyMarginBottom">
        <xsl:choose>
          <xsl:when test="$gIsReportWithOrderTypeLegend and $gIsReportWithTradeTypeLegend">
            <xsl:choose>
              <xsl:when test="$gIsReportWithSpecificLegend">
                <xsl:value-of select="$gPageA4VerticalBodyMarginBottomLarge3"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$gPageA4VerticalBodyMarginBottomLarge2"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$gIsReportWithOrderTypeLegend or $gIsReportWithTradeTypeLegend">
            <xsl:choose>
              <xsl:when test="$gIsReportWithSpecificLegend">
                <xsl:value-of select="$gPageA4VerticalBodyMarginBottomLarge2"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$gPageA4VerticalBodyMarginBottomLarge1"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$gIsReportWithPriceTypeLegend">
            <xsl:choose>
              <xsl:when test="$gIsReportWithSpecificLegend">
                <xsl:value-of select="$gPageA4VerticalBodyMarginBottomLarge2"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$gPageA4VerticalBodyMarginBottomLarge1"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$gPageA4VerticalBodyMarginBottom"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="vBodyMarginBottomRest">
        <xsl:choose>
          <xsl:when test="($gReportHeaderFooter/fLegend/text()='AllPages')">
            <xsl:value-of select="$vBodyMarginBottom"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$gPageA4VerticalBodyMarginBottom"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="vBodyMarginBottomLast">
        <xsl:choose>
          <xsl:when test="($gReportHeaderFooter=false()) or 
                      ($gReportHeaderFooter/fLegend/text()='AllPages') or 
                      ($gReportHeaderFooter/fLegend/text()='LastPageOnly')">
            <xsl:value-of select="$vBodyMarginBottom"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$gPageA4VerticalBodyMarginBottom"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vFooterExtent">
        <xsl:choose>
          <xsl:when test="$gIsReportWithOrderTypeLegend and $gIsReportWithTradeTypeLegend">
            <xsl:choose>
              <xsl:when test="$gIsReportWithSpecificLegend">
                <xsl:value-of select="$gPageA4VerticalFooterExtentLarge3"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$gPageA4VerticalFooterExtentLarge2"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$gIsReportWithOrderTypeLegend or $gIsReportWithTradeTypeLegend">
            <xsl:choose>
              <xsl:when test="$gIsReportWithSpecificLegend">
                <xsl:value-of select="$gPageA4VerticalFooterExtentLarge2"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$gPageA4VerticalFooterExtentLarge1"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$gIsReportWithPriceTypeLegend">
            <xsl:choose>
              <xsl:when test="$gIsReportWithSpecificLegend">
                <xsl:value-of select="$gPageA4VerticalFooterExtentLarge2"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$gPageA4VerticalFooterExtentLarge1"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$gPageA4VerticalFooterExtent"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="vFooterExtentRest">
        <xsl:choose>
          <xsl:when test="($gReportHeaderFooter/fLegend/text()='AllPages')">
            <xsl:value-of select="$vFooterExtent"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$gPageA4VerticalFooterExtent"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="vFooterExtentLast">
        <xsl:choose>
          <xsl:when test="($gReportHeaderFooter=false()) or 
                      ($gReportHeaderFooter/fLegend/text()='AllPages') or 
                      ($gReportHeaderFooter/fLegend/text()='LastPageOnly')">
            <xsl:value-of select="$vFooterExtent"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$gPageA4VerticalFooterExtent"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vBodyMarginTopFirst">
        <xsl:choose>
          <xsl:when test="$gReportHeaderFooter=false() or
            $gReportHeaderFooter/hLFirstPg/text() != 'None' or 
            $gReportHeaderFooter/hCFirstPg/text() != 'None' or 
            $gReportHeaderFooter/hRFirstPg/text() != 'None'">
            <xsl:value-of select="$gPageA4VerticalBodyMarginTop"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="gPageA4VerticalBodyMarginTop0"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="vBodyMarginTopRest">
        <xsl:choose>
          <xsl:when test="$gReportHeaderFooter=false() or
            $gReportHeaderFooter/hLAllPg/text() != 'None' or 
            $gReportHeaderFooter/hCAllPg/text() != 'None' or 
            $gReportHeaderFooter/hRAllPg/text() != 'None'">
            <xsl:value-of select="$gPageA4VerticalBodyMarginTop"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$gPageA4VerticalBodyMarginTop0"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vHeaderExtentFirst">
        <xsl:choose>
          <xsl:when test="$gReportHeaderFooter=false() or
            $gReportHeaderFooter/hLFirstPg/text() != 'None' or 
            $gReportHeaderFooter/hCFirstPg/text() != 'None' or 
            $gReportHeaderFooter/hRFirstPg/text() != 'None'">
            <xsl:value-of select="$gPageA4VerticalHeaderExtent"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$gPageA4VerticalHeaderExtent0"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="vHeaderExtentRest">
        <xsl:choose>
          <xsl:when test="$gReportHeaderFooter=false() or
            $gReportHeaderFooter/hLAllPg/text() != 'None' or 
            $gReportHeaderFooter/hCAllPg/text() != 'None' or 
            $gReportHeaderFooter/hRAllPg/text() != 'None'">
            <xsl:value-of select="$gPageA4VerticalHeaderExtent"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$gPageA4VerticalHeaderExtent0"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <!--<xsl:call-template name="SetPagesCaracteristics_SimplePage">
        <xsl:with-param name="pName" select="'A4-vertical-first'"/>
        <xsl:with-param name="pHeader" select="'A4-vertical-header-first'"/>
        <xsl:with-param name="pHeaderExtent" select="$vHeaderExtentFirst"/>
        <xsl:with-param name="pBodyMarginTop" select="$vBodyMarginTopFirst"/>
        <xsl:with-param name="pFooter" select="'A4-vertical-footer-first'"/>
        <xsl:with-param name="pFooterExtent" select="$vFooterExtentRest"/>
        <xsl:with-param name="pBodyMarginBottom" select="$vBodyMarginBottomRest"/>
      </xsl:call-template>-->
      <!--<xsl:call-template name="SetPagesCaracteristics_SimplePage">
        <xsl:with-param name="pName" select="'A4-vertical-last'"/>
        <xsl:with-param name="pHeader" select="'A4-vertical-header-last'"/>
        <xsl:with-param name="pHeaderExtent" select="$vHeaderExtentRest"/>
        <xsl:with-param name="pBodyMarginTop" select="$vBodyMarginTopRest"/>
        <xsl:with-param name="pFooter" select="'A4-vertical-footer-last'"/>
        <xsl:with-param name="pFooterExtent" select="$vFooterExtentLast"/>
        <xsl:with-param name="pBodyMarginBottom" select="$vBodyMarginBottomLast"/>
      </xsl:call-template>-->
      <!--<xsl:call-template name="SetPagesCaracteristics_SimplePage">
        <xsl:with-param name="pName" select="'A4-vertical-only'"/>
        <xsl:with-param name="pHeader" select="'A4-vertical-header-only'"/>
        <xsl:with-param name="pHeaderExtent" select="$vHeaderExtentFirst"/>
        <xsl:with-param name="pBodyMarginTop" select="$vBodyMarginTopFirst"/>
        <xsl:with-param name="pFooter" select="'A4-vertical-footer-only'"/>
        <xsl:with-param name="pFooterExtent" select="$vFooterExtentLast"/>
        <xsl:with-param name="pBodyMarginBottom" select="$vBodyMarginBottomLast"/>
      </xsl:call-template>-->
      <xsl:call-template name="SetPagesCaracteristics_SimplePage">
        <xsl:with-param name="pName" select="'A4-vertical-rest'"/>
        <xsl:with-param name="pHeader" select="'A4-vertical-header-rest'"/>
        <xsl:with-param name="pBodyMarginTop" select="$vBodyMarginTopRest"/>
        <xsl:with-param name="pHeaderExtent" select="$vHeaderExtentRest"/>
        <xsl:with-param name="pFooter" select="'A4-vertical-footer-rest'"/>
        <xsl:with-param name="pFooterExtent" select="$vFooterExtentRest"/>
        <xsl:with-param name="pBodyMarginBottom" select="$vBodyMarginBottomRest"/>
      </xsl:call-template>
      <xsl:call-template name="SetPagesCaracteristicsSpecific">
        <xsl:with-param name="pBodyMarginTop" select="$vBodyMarginTopRest"/>
        <xsl:with-param name="pHeaderExtent" select="$vHeaderExtentRest"/>
        <xsl:with-param name="pFooterExtent" select="$vFooterExtentRest"/>
        <xsl:with-param name="pBodyMarginBottom" select="$vBodyMarginBottomRest"/>
      </xsl:call-template>
      <fo:page-sequence-master master-name="A4-vertical">
        <fo:repeatable-page-master-alternatives>
          <!--<fo:conditional-page-master-reference page-position="last" master-reference="A4-vertical-last"/>-->
          <!--<fo:conditional-page-master-reference page-position="first" master-reference="A4-vertical-first"/>
          <fo:conditional-page-master-reference page-position="only" master-reference="A4-vertical-only"/>
          <fo:conditional-page-master-reference page-position="rest" master-reference="A4-vertical-rest"/>-->
          <fo:conditional-page-master-reference master-reference="A4-vertical-rest"/>
        </fo:repeatable-page-master-alternatives>
      </fo:page-sequence-master>
    </fo:layout-master-set>
  </xsl:template>
  <!--SetPagesCaracteristics_SimplePage template-->
  <xsl:template name="SetPagesCaracteristics_SimplePage">
    <xsl:param name="pName"/>
    <xsl:param name="pHeader"/>
    <xsl:param name="pHeaderExtent"/>
    <xsl:param name="pBodyMarginTop"/>
    <xsl:param name="pFooter"/>
    <xsl:param name="pFooterExtent"/>
    <xsl:param name="pBodyMarginBottom"/>

    <!--<fo:simple-page-master master-name="{$pName}"
                           page-height="{$gPageA4VerticalPageHeight}"
                           page-width="{$gPageA4VerticalPageWidth}"
                           margin="{$gPageA4VerticalMargin}"
                           padding="0"
                           border-width="0mm">-->

    <fo:simple-page-master master-name="{$pName}"
                           page-height="{$gPageA4VerticalPageHeight}"
                           page-width="{$gPageA4VerticalPageWidth}"
                           margin-left="{$gPageA4VerticalMarginLeft}"
                           margin-right="{$gPageA4VerticalMarginRight}"
                           margin-bottom="{$gPageA4VerticalMarginBottom}"
                           margin-top="{$gPageA4VerticalMarginTop}"
                           padding="0"
                           border-width="0mm">

      <fo:region-body region-name="A4-vertical-body"
                      margin-left="{$gPageA4VerticalBodyMargin}"
                      margin-right="{$gPageA4VerticalBodyMargin}"
                      margin-bottom="{$pBodyMarginBottom}"
                      margin-top="{$pBodyMarginTop}"
                      border="{$gcPageBorderDebug}"
                      padding="0"
                      border-width="0mm"
                      overflow="scroll">

        <xsl:attribute name="background-color">
          <xsl:choose>
            <xsl:when test="$gcIsDisplayDebugMode=true()">
              <xsl:value-of select="'yellow'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'white'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </fo:region-body>

      <fo:region-before region-name="{$pHeader}"
                        extent="{$pHeaderExtent}"
                        precedence="true">
        <xsl:attribute name="background-color">
          <xsl:choose>
            <xsl:when test="$gcIsDisplayDebugMode=true()">
              <xsl:value-of select="'green'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'white'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </fo:region-before>
      <fo:region-after region-name="{$pFooter}"
                       extent="{$pFooterExtent}"
                       display-align="after"
                       precedence="true"
                       padding="0"
                       border-width="0mm">

        <xsl:attribute name="background-color">
          <xsl:choose>
            <xsl:when test="$gcIsDisplayDebugMode=true()">
              <xsl:value-of select="'green'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'white'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </fo:region-after>

      <fo:region-start region-name="A4-vertical-start-rest"
                       extent="{$gPageA4VerticalBodyMargin}">
        <xsl:attribute name="background-color">
          <xsl:choose>
            <xsl:when test="$gcIsDisplayDebugMode=true()">
              <xsl:value-of select="'green'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'white'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </fo:region-start>
      <fo:region-end region-name="A4-vertical-end-rest"
                     extent="{$gPageA4VerticalBodyMargin}">
        <xsl:attribute name="background-color">
          <xsl:choose>
            <xsl:when test="$gcIsDisplayDebugMode=true()">
              <xsl:value-of select="'green'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'white'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </fo:region-end>
    </fo:simple-page-master>
  </xsl:template>

  <!-- .................................. -->
  <!--   Document Content                 -->
  <!-- .................................. -->
  <!--DisplayDocumentContent template-->
  <xsl:template name="DisplayDocumentContent">
    <xsl:choose>
      <xsl:when test="$gIsMonoBook = 'true'">
        <xsl:variable name="vCountBook" select="count(msxsl:node-set($gBusinessBooks)/BOOK)"/>
        <!--//-->
        <xsl:for-each select="msxsl:node-set($gBusinessBooks)/BOOK">
          <xsl:sort select="@data" data-type="text"/>

          <xsl:variable name="vBook" select="@data"/>

          <xsl:call-template name="DisplayDocumentPageSequence">
            <xsl:with-param name="pBook" select="$vBook" />
            <xsl:with-param name="pBookPosition" select="position()" />
            <xsl:with-param name="pCountBook" select="$vCountBook" />
          </xsl:call-template>
        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="DisplayDocumentPageSequence">
          <xsl:with-param name="pBookPosition" select="1" />
          <xsl:with-param name="pCountBook" select="1" />
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!--DisplayDocumentPageSequence template-->
  <xsl:template name="DisplayDocumentPageSequence">
    <xsl:param name="pBook" />
    <xsl:param name="pBookPosition" />
    <xsl:param name="pCountBook" />
    <xsl:param name="pSetLastPage" select="true()" />
    <xsl:param name="pIsSummuryPage" select="false()" />
    <xsl:param name="pIsCreditNoteVisualization" select="false()" />
    <xsl:param name="pGroups" />
    <xsl:param name="pIsHFormulaMandatory" select="true()" />

    <fo:page-sequence master-reference="A4-vertical" font-family="{$gFontFamily}">
      <!--==========================================================================================-->
      <!-- Header of summary page                                                                   -->
      <!--==========================================================================================-->
      <!--<fo:static-content flow-name="A4-vertical-header-first" 
                         border="{$gcPageBorderDebug}" 
                         display-align="before" >
        <fo:block>
          <xsl:call-template name="DisplayPageHeader">
            <xsl:with-param name="pLogo" select="$gImgLogo" />
            <xsl:with-param name="pBook" select="$pBook" />
            <xsl:with-param name="pIsFirstPage" select="true()" />
          </xsl:call-template>
        </fo:block>
      </fo:static-content>-->
      <!--<fo:static-content flow-name="A4-vertical-header-last" 
                         border="{$gcPageBorderDebug}" 
                         display-align="before" >
        <fo:block>
          <xsl:call-template name="DisplayPageHeader">
            <xsl:with-param name="pLogo" select="$gImgLogo" />
            <xsl:with-param name="pBook" select="$pBook" />
            <xsl:with-param name="pIsFirstPage" select="false()" />
          </xsl:call-template>
        </fo:block>
      </fo:static-content>-->
      <!--<fo:static-content flow-name="A4-vertical-header-only" 
                         border="{$gcPageBorderDebug}" 
                         display-align="before" >
        <fo:block>
          <xsl:call-template name="DisplayPageHeader">
            <xsl:with-param name="pLogo" select="$gImgLogo" />
            <xsl:with-param name="pBook" select="$pBook" />
            <xsl:with-param name="pIsFirstPage" select="true()" />
          </xsl:call-template>
        </fo:block>
      </fo:static-content>-->
      <fo:static-content flow-name="A4-vertical-header-rest"
                         border="{$gcPageBorderDebug}"
                         display-align="before" >
        <fo:block>
          <xsl:call-template name="DisplayPageHeader">
            <xsl:with-param name="pLogo" select="$gImgLogo" />
            <xsl:with-param name="pBook" select="$pBook" />
            <xsl:with-param name="pIsFirstPage" select="false()" />
            <xsl:with-param name="pIsCreditNoteVisualization" select="$pIsCreditNoteVisualization" />
          </xsl:call-template>
        </fo:block>
      </fo:static-content>
      <!--//-->
      <xsl:variable name="vGroups">
        <xsl:choose>
          <xsl:when test="$pIsSummuryPage"/>
          <xsl:when test="$pGroups">
            <xsl:copy-of select="$pGroups"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="CreateGroups">
              <xsl:with-param name="pBook" select="$pBook" />
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="vIsDisplayStarLegend">
        <xsl:choose>
          <xsl:when test="msxsl:node-set($vGroups)/groups[@isSpecific=false()]/group/sum[@status=$gcNOkStatus] or 
                    msxsl:node-set($vGroups)/groups[@isSpecific=false()]/group/serie/trade//fees/payment[@status=$gcNOkStatus]">
            <xsl:value-of select="1"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="0"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <!--==========================================================================================-->
      <!-- Footer of summary page                                                                   -->
      <!--==========================================================================================-->
      <!--<fo:static-content flow-name="A4-vertical-footer-first"
                         border="{$gcPageBorderDebug}"
                         display-align="after">
        <fo:block padding-before="0mm" display-align="after">
          <xsl:call-template name="DisplayPageFooter">
            <xsl:with-param name="pIsDisplayStarLegend" select="$vIsDisplayStarLegend"/>
            <xsl:with-param name="pIsLastPage" select="false()" />
          </xsl:call-template>
        </fo:block>
      </fo:static-content>-->
      <!--<fo:static-content flow-name="A4-vertical-footer-last"
                         border="{$gcPageBorderDebug}"
                         display-align="after">
        <fo:block padding-before="0mm" display-align="after">
          <xsl:call-template name="DisplayPageFooter">
            <xsl:with-param name="pIsDisplayStarLegend" select="$vIsDisplayStarLegend"/>
            <xsl:with-param name="pIsLastPage" select="true()" />
          </xsl:call-template>
        </fo:block>
      </fo:static-content>-->
      <!--<fo:static-content flow-name="A4-vertical-footer-only"
                         border="{$gcPageBorderDebug}"
                         display-align="after">
        <fo:block padding-before="0mm" display-align="after">
          <xsl:call-template name="DisplayPageFooter">
            <xsl:with-param name="pIsDisplayStarLegend" select="$vIsDisplayStarLegend"/>
            <xsl:with-param name="pIsLastPage" select="true()" />
          </xsl:call-template>
        </fo:block>
      </fo:static-content>-->
      <fo:static-content flow-name="A4-vertical-footer-rest"
                         border="{$gcPageBorderDebug}"
                         display-align="after">
        <fo:block padding-before="0mm" display-align="after">
          <xsl:call-template name="DisplayPageFooter">
            <xsl:with-param name="pIsDisplayStarLegend" select="$vIsDisplayStarLegend"/>
            <xsl:with-param name="pIsLastPage" select="false()" />
          </xsl:call-template>
        </fo:block>
      </fo:static-content>
      <!--==========================================================================================-->
      <!-- Body of summary page                                                                     -->
      <!--==========================================================================================-->
      <fo:flow flow-name="A4-vertical-body" border="{$gcPageBorderDebug}">
        <xsl:choose>
          <xsl:when test="$pIsSummuryPage">
            <xsl:call-template name="DisplaySummaryPageBody">
              <xsl:with-param name="pBook" select="$pBook" />
              <xsl:with-param name="pBookPosition" select="$pBookPosition" />
              <xsl:with-param name="pCountBook" select="$pCountBook" />
              <xsl:with-param name="pIsCreditNoteVisualization" select="$pIsCreditNoteVisualization" />
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="DisplayPageBody">
              <xsl:with-param name="pBook" select="$pBook" />
              <xsl:with-param name="pBookPosition" select="$pBookPosition" />
              <xsl:with-param name="pCountBook" select="$pCountBook" />
              <xsl:with-param name="pSetLastPage" select="$pSetLastPage" />
              <xsl:with-param name="pGroups" select="$vGroups" />
              <xsl:with-param name="pIsDisplayStarLegend" select="$vIsDisplayStarLegend"/>
              <xsl:with-param name="pIsHFormulaMandatory" select="$pIsHFormulaMandatory"/>
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </fo:flow>
    </fo:page-sequence>
  </xsl:template>

  <!-- .................................. -->
  <!--   Header                           -->
  <!-- .................................. -->
  <!--DisplayPageHeader template-->
  <xsl:template name="DisplayPageHeader">
    <xsl:param name="pLogo" />
    <xsl:param name="pBook" />
    <xsl:param name="pIsFirstPage" select="false" />
    <xsl:param name="pIsCreditNoteVisualization" select="false()" />
    <xsl:param name="pIsLandscape" select="false()"/>

    <xsl:if test="$gReportHeaderFooter=false() or
            ($pIsFirstPage=true() and 
            ($gReportHeaderFooter/hLFirstPg/text() != 'None' or 
            $gReportHeaderFooter/hCFirstPg/text() != 'None' or 
            $gReportHeaderFooter/hRFirstPg/text() != 'None')) or
            ($pIsFirstPage=false() and 
            ($gReportHeaderFooter/hLAllPg/text() != 'None' or 
            $gReportHeaderFooter/hCAllPg/text() != 'None' or 
            $gReportHeaderFooter/hRAllPg/text() != 'None'))">

      <xsl:variable name="vRuleSize">
        <xsl:choose>
          <xsl:when test="string-length($gReportHeaderFooter/hRuleSize/text())>0">
            <xsl:value-of select="concat($gReportHeaderFooter/hRuleSize/text(),'pt')"/>
          </xsl:when>
          <xsl:when test="$gReportHeaderFooter">
            <xsl:value-of select="'0pt'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'0.5pt'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vPageA4HeaderWidth">
        <xsl:choose>
          <xsl:when test="$pIsLandscape = 'true'">
            <xsl:value-of select="$gPageA4LandscapeHeaderWidth"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$gPageA4VerticalHeaderWidth"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="vPageA4HeaderLeftWidth">
        <xsl:choose>
          <xsl:when test="$pIsLandscape = 'true'">
            <xsl:value-of select="$gPageA4LandscapeHeaderLeftWidth"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$gPageA4VerticalHeaderLeftWidth"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="vPageA4HeaderCenterWidth">
        <xsl:choose>
          <xsl:when test="$pIsLandscape = 'true'">
            <xsl:value-of select="$gPageA4LandscapeHeaderCenterWidth"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$gPageA4VerticalHeaderCenterWidth"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="vPageA4HeaderRightWidth">
        <xsl:choose>
          <xsl:when test="$pIsLandscape = 'true'">
            <xsl:value-of select="$gPageA4LandscapeHeaderRightWidth"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$gPageA4VerticalHeaderRightWidth"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <!-- Display logo, Entity and Published Date -->
      <fo:block linefeed-treatment="preserve">
        <fo:table table-layout="fixed"
                  border-bottom-style="solid"
                  border="{$vRuleSize}"
                  width="{$vPageA4HeaderWidth}"
                  font-weight="normal">

          <xsl:if test="string-length($gReportHeaderFooter/hRuleColor/text())>0">
            <xsl:attribute name="border-bottom-color">
              <xsl:call-template name="Lower">
                <xsl:with-param name="source" select="$gReportHeaderFooter/hRuleColor/text()"/>
              </xsl:call-template>
            </xsl:attribute>
          </xsl:if>

          <fo:table-column border="{$gcTableBorderDebug}" column-width="{$vPageA4HeaderLeftWidth}" column-number="01" />
          <fo:table-column border="{$gcTableBorderDebug}" column-width="{$vPageA4HeaderCenterWidth}" column-number="02" />
          <fo:table-column border="{$gcTableBorderDebug}" column-width="{$vPageA4HeaderRightWidth}" column-number="03" />

          <fo:table-body>
            <fo:table-row font-weight="normal">

              <xsl:variable name="vHeaderColor">
                <xsl:choose>
                  <xsl:when test="string-length($gReportHeaderFooter/hColor/text())>0">
                    <xsl:call-template name="Lower">
                      <xsl:with-param name="source" select="$gReportHeaderFooter/hColor/text()"/>
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="$gPageA4VerticalHeaderTextColor"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:variable>

              <xsl:choose>
                <xsl:when test="$gReportHeaderFooter">
                  <xsl:choose>
                    <xsl:when test="$pIsFirstPage=true()">
                      <!--hLFirstPg-->
                      <xsl:call-template name="DisplayPageHeaderType">
                        <xsl:with-param name="pType" select="$gReportHeaderFooter/hLFirstPg/text()" />
                        <xsl:with-param name="pLogo" select="$pLogo" />
                        <xsl:with-param name="pBook" select="$pBook" />
                        <xsl:with-param name="pCustom" select="$gReportHeaderFooter/hLFirstPgCustom/text()" />
                        <xsl:with-param name="pColor" select="$vHeaderColor" />
                        <xsl:with-param name="pAlign" select="'left'" />
                        <xsl:with-param name="pIsCreditNoteVisualization" select="$pIsCreditNoteVisualization" />
                      </xsl:call-template>
                      <!--hCFirstPg-->
                      <xsl:call-template name="DisplayPageHeaderType">
                        <xsl:with-param name="pType" select="$gReportHeaderFooter/hCFirstPg/text()" />
                        <xsl:with-param name="pLogo" select="$pLogo" />
                        <xsl:with-param name="pBook" select="$pBook" />
                        <xsl:with-param name="pCustom" select="$gReportHeaderFooter/hCFirstPgCustom/text()" />
                        <xsl:with-param name="pColor" select="$vHeaderColor" />
                        <xsl:with-param name="pAlign" select="'center'" />
                        <xsl:with-param name="pIsCreditNoteVisualization" select="$pIsCreditNoteVisualization" />
                      </xsl:call-template>
                      <!--hRFirstPg-->
                      <xsl:call-template name="DisplayPageHeaderType">
                        <xsl:with-param name="pType" select="$gReportHeaderFooter/hRFirstPg/text()" />
                        <xsl:with-param name="pLogo" select="$pLogo" />
                        <xsl:with-param name="pBook" select="$pBook" />
                        <xsl:with-param name="pCustom" select="$gReportHeaderFooter/hRFirstPgCustom/text()" />
                        <xsl:with-param name="pColor" select="$vHeaderColor" />
                        <xsl:with-param name="pAlign" select="'right'" />
                        <xsl:with-param name="pIsCreditNoteVisualization" select="$pIsCreditNoteVisualization" />
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise>
                      <!--hLAllPg-->
                      <xsl:call-template name="DisplayPageHeaderType">
                        <xsl:with-param name="pType" select="$gReportHeaderFooter/hLAllPg/text()" />
                        <xsl:with-param name="pLogo" select="$pLogo" />
                        <xsl:with-param name="pBook" select="$pBook" />
                        <xsl:with-param name="pCustom" select="$gReportHeaderFooter/hLAllPgCustom/text()" />
                        <xsl:with-param name="pColor" select="$vHeaderColor" />
                        <xsl:with-param name="pAlign" select="'left'" />
                        <xsl:with-param name="pIsCreditNoteVisualization" select="$pIsCreditNoteVisualization" />
                      </xsl:call-template>
                      <!--hCAllPg-->
                      <xsl:call-template name="DisplayPageHeaderType">
                        <xsl:with-param name="pType" select="$gReportHeaderFooter/hCAllPg/text()" />
                        <xsl:with-param name="pLogo" select="$pLogo" />
                        <xsl:with-param name="pBook" select="$pBook" />
                        <xsl:with-param name="pCustom" select="$gReportHeaderFooter/hCAllPgCustom/text()" />
                        <xsl:with-param name="pColor" select="$vHeaderColor" />
                        <xsl:with-param name="pAlign" select="'center'" />
                        <xsl:with-param name="pIsCreditNoteVisualization" select="$pIsCreditNoteVisualization" />
                      </xsl:call-template>
                      <!--hRAllPg-->
                      <xsl:call-template name="DisplayPageHeaderType">
                        <xsl:with-param name="pType" select="$gReportHeaderFooter/hRAllPg/text()" />
                        <xsl:with-param name="pLogo" select="$pLogo" />
                        <xsl:with-param name="pBook" select="$pBook" />
                        <xsl:with-param name="pCustom" select="$gReportHeaderFooter/hRAllPgCustom/text()" />
                        <xsl:with-param name="pColor" select="$vHeaderColor" />
                        <xsl:with-param name="pAlign" select="'right'" />
                        <xsl:with-param name="pIsCreditNoteVisualization" select="$pIsCreditNoteVisualization" />
                      </xsl:call-template>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:when>
                <xsl:otherwise>
                  <!--Logo-->
                  <xsl:call-template name="DisplayPageHeaderType">
                    <xsl:with-param name="pType" select="'Logo'" />
                    <xsl:with-param name="pLogo" select="$pLogo" />
                    <xsl:with-param name="pAlign" select="'left'" />
                    <xsl:with-param name="pIsCreditNoteVisualization" select="$pIsCreditNoteVisualization" />
                  </xsl:call-template>
                  <!--Entity-->
                  <xsl:call-template name="DisplayPageHeaderType">
                    <xsl:with-param name="pType" select="'CompanyName'" />
                    <xsl:with-param name="pColor" select="$vHeaderColor" />
                    <xsl:with-param name="pAlign" select="'center'" />
                    <xsl:with-param name="pIsCreditNoteVisualization" select="$pIsCreditNoteVisualization" />
                  </xsl:call-template>
                  <!--Book and Published Date-->
                  <xsl:call-template name="DisplayPageHeaderType">
                    <xsl:with-param name="pType" select="'Published_Model1'" />
                    <xsl:with-param name="pBook" select="$pBook" />
                    <xsl:with-param name="pColor" select="$vHeaderColor" />
                    <xsl:with-param name="pAlign" select="'right'" />
                    <xsl:with-param name="pIsCreditNoteVisualization" select="$pIsCreditNoteVisualization" />
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </fo:table-row>
          </fo:table-body>
        </fo:table>
      </fo:block>
    </xsl:if>
  </xsl:template>
  <!--DisplayPageHeaderType template-->
  <xsl:template name="DisplayPageHeaderType">
    <xsl:param name="pType" />
    <xsl:param name="pLogo" />
    <xsl:param name="pBook" />
    <xsl:param name="pCustom" />
    <xsl:param name="pColor" />
    <xsl:param name="pAlign" />
    <xsl:param name="pIsCreditNoteVisualization" select="false()" />

    <xsl:choose>
      <!-- Logo -->
      <xsl:when test="$pType='Logo'">
        <fo:table-cell border="{$gcTableBorderDebug}" text-align="{$pAlign}">
          <fo:block>
            <fo:external-graphic src="{$gImgLogo}" height="{$gPageA4VerticalHeaderLogoHeight}" vertical-align="bottom"/>
          </fo:block>
        </fo:table-cell>
      </xsl:when>
      <!-- Entity -->
      <xsl:when test="$pType='CompanyName'">
        <fo:table-cell font-size="{$gPageA4VerticalHeaderTextFontSize}" font-weight="normal" border="{$gcTableBorderDebug}" text-align="{$pAlign}" display-align="after">
          <fo:block color="{$pColor}">
            <xsl:choose>
              <xsl:when test="string-length($gEntityDisplayname)>0">
                <xsl:value-of select="$gEntityDisplayname" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$gEntityIdentifier" />
              </xsl:otherwise>
            </xsl:choose>
          </fo:block>
        </fo:table-cell>
      </xsl:when>
      <!-- Book and Published Date -->
      <xsl:when test="starts-with($pType,'Published_Model')">
        <fo:table-cell font-size="{$gPageA4VerticalHeaderDateFontSize}" border="{$gcTableBorderDebug}" font-weight="normal" text-align="{$pAlign}" display-align="after">
          <xsl:call-template name="DisplayPageHeaderType_PublishedModel">
            <xsl:with-param name="pType" select="$pType" />
            <xsl:with-param name="pBook" select="$pBook" />
            <xsl:with-param name="pIsCreditNoteVisualization" select="$pIsCreditNoteVisualization" />
          </xsl:call-template>
        </fo:table-cell>
      </xsl:when>
      <!-- Custom -->
      <xsl:when test="$pType='Custom'">
        <fo:table-cell font-size="{$gPageA4VerticalHeaderDateFontSize}" border="{$gcTableBorderDebug}" font-weight="normal" text-align="{$pAlign}" display-align="after">
          <fo:block color="{$pColor}">
            <xsl:value-of select="$pCustom" />
          </fo:block>
        </fo:table-cell>
      </xsl:when>
      <!-- Default -->
      <xsl:otherwise>
        <fo:table-cell border="{$gcTableBorderDebug}">
          <fo:block/>
        </fo:table-cell>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!--DisplayPageHeaderType_PublishedModel template-->
  <xsl:template name="DisplayPageHeaderType_PublishedModel">
    <xsl:param name="pType" />
    <xsl:param name="pBook" />
    <xsl:param name="pIsCreditNoteVisualization" select="false()" />

    <!--Affichage de <Book-Identifier> ou de <Actor-Identifier> selon le modèle-->
    <fo:block>
      <xsl:choose>
        <xsl:when test="($pType='Published_Model1' or $pType='Published_Model2') and $gIsMonoBook = 'true'">
          <xsl:value-of select="$pBook" />
        </xsl:when>
        <xsl:when test="$pType='Published_Model4'">
          <xsl:value-of select="normalize-space($gSendToRoutingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/actorIdentifier'])" />
        </xsl:when>
      </xsl:choose>
    </fo:block>
    <!--Affichage de <Invoice-Identifier>-->
    <xsl:if test="$pType='Published_Model4'">
      <xsl:variable name="vTradeTradeHeader">
        <xsl:call-template name="GetTradeHeader">
          <xsl:with-param name="pIsCreditNoteVisualization" select="$pIsCreditNoteVisualization" />
        </xsl:call-template>
      </xsl:variable>
      <xsl:if test="$vTradeTradeHeader">
        <fo:block>
          <xsl:copy-of select="$vTradeTradeHeader" />
        </fo:block>
      </xsl:if>
    </xsl:if>
    <!--Affichage de <Edité le > date et heure-->
    <fo:block>
      <!--Affichage de le ressource <Edité le >-->
      <xsl:call-template name="getSpheresTranslation">
        <xsl:with-param name="pResourceName" select="'TS-Published'" />
      </xsl:call-template>
      <!--Affichage de la date de création du report-->
      <xsl:call-template name="format-date">
        <xsl:with-param name="xsd-date-time" select="$gCreationTimestamp" />
      </xsl:call-template>
      <!--//-->
      <!--Affichage de l'heure de création du report selon le modèle-->
      <xsl:if test="$pType='Published_Model1'">
        <xsl:value-of select="$gcEspace" />
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'TS-PublishedAt'" />
        </xsl:call-template>
        <xsl:value-of select="$gcEspace" />
        <xsl:call-template name="format-time2">
          <xsl:with-param name="xsd-date-time" select="substring-after($gCreationTimestamp, 'T')" />
        </xsl:call-template>
      </xsl:if>
    </fo:block>
  </xsl:template>

  <!--DisplayLegend_LastPage template-->
  <!-- 20120522 MF Ticket 17788-->
  <xsl:template name="DisplayLegend_LastPage">
    <xsl:param name="pIsDisplayStarLegend"/>
    <!--StarLegend-->
    <xsl:if test="$pIsDisplayStarLegend = 1">
      <fo:table table-layout="fixed"
                border="{$gcTableBorderDebug}"
                width="{$gPageA4VerticalFooterWidth}">
        <fo:table-column column-width="{$gPageA4VerticalFooterWidth}" column-number="01" />
        <fo:table-body>
          <fo:table-row font-weight="normal">
            <fo:table-cell text-align="justify" display-align="before">
              <fo:block linefeed-treatment="preserve">
                <fo:inline font-size="{$gDetTrdStarFontSize}" font-weight="bold">
                  <xsl:value-of select="$gcNOkStatus" />
                </fo:inline>
                <fo:inline font-size="{$gFormulaFontSize}">
                  <xsl:call-template name="getSpheresTranslation">
                    <xsl:with-param name="pResourceName" select="concat($gReportType,'-TradeLegend')" />
                  </xsl:call-template>
                </fo:inline>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </xsl:if>
    <!--SpecificLegend-->
    <xsl:if test="$gIsReportWithSpecificLegend or $gReportHeaderFooter/fFormula">
      <xsl:variable name="vFormula">
        <xsl:choose>
          <xsl:when test="string-length($gReportHeaderFooter/fFormula/text())>0">
            <xsl:value-of select="$gReportHeaderFooter/fFormula/text()"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="concat($gReportType,'-SpecificLegend')" />
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="vFormulaBlock">
        <xsl:if test="$gReportHeaderFooter/fFormula/block">
          <xsl:copy-of select="$gReportHeaderFooter/fFormula/block"/>
        </xsl:if>
      </xsl:variable>
      <xsl:call-template name="DisplayStatementTitle">
        <xsl:with-param name="pTitle" select="$vFormula" />
        <xsl:with-param name="pTitleBlock" select="$vFormulaBlock" />
        <xsl:with-param name="pTitleFontSize" select="$gFormulaFontSize" />
        <xsl:with-param name="pTitleFontWeight" select="$gFormulaFontWeight" />
        <xsl:with-param name="pTitleTextAlign" select="'left'" />
        <xsl:with-param name="pTitleWidth" select="$gPageA4VerticalFormulaWidth" />
        <xsl:with-param name="pTitlePaddingTop" select="$gFormulaPaddingTop" />
        <xsl:with-param name="pTitlePaddingBottom" select="$gFormulaPaddingTop" />
        <xsl:with-param name="pTitleLeftMargin" select="$gFormulaLeftMargin" />
      </xsl:call-template>
    </xsl:if>
    <!-- RD 20141120 [20504] Afficher un Message personnalisé en dernière page du document indépendamment de l'affichage ou pas des Legendes.-->
    <xsl:if test="($gReportHeaderFooter=false()) or ($gReportHeaderFooter/fLegend/text()='LastPageOnly')">
      <!--OrderTypeLegend-->
      <xsl:if test="$gIsReportWithOrderTypeLegend">
        <xsl:variable name="vRepositoryOrdTypeEnums" select="$gDataDocumentRepository/enums[@id='ENUMS.CODE.OrdTypeEnum']/enum"/>
        <xsl:if test="$vRepositoryOrdTypeEnums">
          <xsl:variable name="vOrdTypeEnums">
            <xsl:for-each select="$vRepositoryOrdTypeEnums">
              <xsl:sort select="extattrb/text()" data-type="text" />
              <xsl:sort select="value/text()" data-type="text" />
              <xsl:copy-of select="." />
            </xsl:for-each>
          </xsl:variable>
          <xsl:call-template name="DisplayPageFooterEnums">
            <xsl:with-param name="pEnums" select="msxsl:node-set($vOrdTypeEnums)/enum" />
            <xsl:with-param name="pEnumsResource" select="'TS-OrderTypeLong'" />
          </xsl:call-template>
        </xsl:if>
      </xsl:if>
      <!--TradeTypeLegend-->
      <xsl:if test="$gIsReportWithTradeTypeLegend">
        <xsl:variable name="vRepositoryTrdTypeEnums" select="$gDataDocumentRepository/enums[@id='ENUMS.CODE.TrdTypeEnum']/enum"/>
        <xsl:if test="$vRepositoryTrdTypeEnums">
          <xsl:variable name="vTrdTypeEnums">
            <xsl:for-each select="$vRepositoryTrdTypeEnums">
              <xsl:sort select="extattrb/text()" data-type="text" />
              <xsl:sort select="value/text()" data-type="number" />
              <xsl:copy-of select="." />
            </xsl:for-each>
          </xsl:variable>
          <xsl:call-template name="DisplayPageFooterEnums">
            <xsl:with-param name="pEnums" select="msxsl:node-set($vTrdTypeEnums)/enum" />
            <xsl:with-param name="pEnumsResource" select="'TS-TradeTypeLong'" />
          </xsl:call-template>
        </xsl:if>
      </xsl:if>
      <!--PriceTypeLegend-->
      <xsl:if test="$gIsReportWithPriceTypeLegend">
        <xsl:variable name="vPriceTypeEnums">
          <!--Afficher "Prix de référence"-->
          <xsl:if test="$gPosActions[@requestType='MOF']">
            <enum id="ENUM.VALUE.1">
              <value>(1)</value>
              <extvalue>
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'TS-PriceType1'" />
                </xsl:call-template>
              </extvalue>
            </enum>
          </xsl:if>
          <!--Afficher "Prix du Sous-jacent"-->
          <!--RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)-->
          <xsl:if test="$gPosActions[contains(',EXE,AUTOEXE,ABN,NEX,NAS,AUTOABN,ASS,AUTOASS,',concat(',',@requestType,','))]">
            <enum id="ENUM.VALUE.2">
              <value>(2)</value>
              <extvalue>
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'TS-PriceType2'" />
                </xsl:call-template>
              </extvalue>
            </enum>
          </xsl:if>
        </xsl:variable>
        <xsl:if test="msxsl:node-set($vPriceTypeEnums)/enum">
          <xsl:call-template name="DisplayPageFooterEnums">
            <xsl:with-param name="pEnums" select="msxsl:node-set($vPriceTypeEnums)/enum" />
            <xsl:with-param name="pEnumsResource" select="'TS-PriceType'" />
          </xsl:call-template>
        </xsl:if>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <!-- .................................. -->
  <!--   Footer                           -->
  <!-- .................................. -->
  <!--DisplayPageFooter template-->
  <xsl:template name="DisplayPageFooter">
    <xsl:param name="pIsDisplayStarLegend" />
    <xsl:param name="pIsLastPage" />
    <xsl:param name="pIsLandscape" select="false()"/>

    <!--A revoir: Problème « FirstPage » et « LastPage» -->
    <!--<xsl:if test="($gReportHeaderFooter=false() and $pIsLastPage=true()) or 
            ($gReportHeaderFooter/fLegend/text()='AllPages') or 
            ($gReportHeaderFooter/fLegend/text()='LastPageOnly' and $pIsLastPage=true())">-->

    <!-- 20120522 MF Ticket 17788 -->
    <!-- 20120523 MF afficher la légende dans le footer amène à des incogruences, voir ticket 17788  -->
    <!--<xsl:if test="($gReportHeaderFooter/fLegend/text()='AllPages')">
      <xsl:call-template name="DisplayLegend">
        <xsl:with-param name="pIsDisplayStarLegend" select="$pIsDisplayStarLegend"/>
      </xsl:call-template>
    </xsl:if>-->

    <xsl:variable name="vPageA4FooterWidth">
      <xsl:choose>
        <xsl:when test="$pIsLandscape = 'true'">
          <xsl:value-of select="$gPageA4LandscapeFooterWidth"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$gPageA4VerticalFooterWidth"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vPageA4FooterLeftWidth">
      <xsl:choose>
        <xsl:when test="$pIsLandscape = 'true'">
          <xsl:value-of select="$gPageA4LandscapeFooterLeftWidth"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$gPageA4VerticalFooterLeftWidth"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vPageA4FooterCenterWidth">
      <xsl:choose>
        <xsl:when test="$pIsLandscape = 'true'">
          <xsl:value-of select="$gPageA4LandscapeFooterCenterWidth"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$gPageA4VerticalFooterCenterWidth"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vPageA4FooterRightWidth">
      <xsl:choose>
        <xsl:when test="$pIsLandscape = 'true'">
          <xsl:value-of select="$gPageA4LandscapeFooterRightWidth"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$gPageA4VerticalFooterRightWidth"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!--//-->
    <xsl:if test="$gcIsModelMode">
      <fo:table table-layout="fixed" border-top-style="none" width="{$vPageA4FooterWidth}">
        <fo:table-column border="{$gcTableBorderDebug}" column-width="{$vPageA4FooterLeftWidth}" column-number="01" />
        <fo:table-body>
          <fo:table-row font-weight="normal">
            <fo:table-cell padding="0mm" border="0pt" text-align="right" display-align="center" font-size="{$gPageA4VerticalFooterLegendTextFontSize}pt">
              <fo:block linefeed-treatment="preserve" >
                <xsl:value-of select="$gCopyright" />
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </xsl:if>
    <!--//-->
    <xsl:if test="$gReportHeaderFooter=false() or
            ($pIsLastPage=true() and 
            ($gReportHeaderFooter/fLLastPg/text() != 'None' or 
            $gReportHeaderFooter/fCLastPg/text() != 'None' or 
            $gReportHeaderFooter/fRLastPg/text() != 'None')) or
            ($pIsLastPage=false() and 
            ($gReportHeaderFooter/fLAllPg/text() != 'None' or 
            $gReportHeaderFooter/fCAllPg/text() != 'None' or 
            $gReportHeaderFooter/fRAllPg/text() != 'None'))">

      <xsl:variable name="vRuleSize">
        <xsl:choose>
          <xsl:when test="string-length($gReportHeaderFooter/fRuleSize/text())>0">
            <xsl:value-of select="concat($gReportHeaderFooter/fRuleSize/text(),'pt')"/>
          </xsl:when>
          <xsl:when test="$gReportHeaderFooter">
            <xsl:value-of select="'0pt'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'0.5pt'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <!--//-->
      <fo:table table-layout="fixed"
                border-top-style="solid"
                border="{$vRuleSize}"
                width="{$vPageA4FooterWidth}">

        <xsl:if test="string-length($gReportHeaderFooter/fRuleColor/text())>0">
          <xsl:attribute name="border-top-color">
            <xsl:call-template name="Lower">
              <xsl:with-param name="source" select="$gReportHeaderFooter/fRuleColor/text()"/>
            </xsl:call-template>
          </xsl:attribute>
        </xsl:if>

        <xsl:choose>
          <xsl:when test="$gReportHeaderFooter">
            <xsl:choose>
              <xsl:when test="$pIsLastPage=true()">
                <fo:table-column border="{$gcTableBorderDebug}" column-number="01">
                  <xsl:attribute name="column-width">
                    <xsl:call-template name="GetPageFooterColumnWidth">
                      <xsl:with-param name="pType" select="$gReportHeaderFooter/fLLastPg/text()" />
                      <xsl:with-param name="pDefaultWidth" select="$vPageA4FooterLeftWidth" />
                    </xsl:call-template>
                  </xsl:attribute>
                </fo:table-column>
                <fo:table-column border="{$gcTableBorderDebug}" column-number="02">
                  <xsl:attribute name="column-width">
                    <xsl:call-template name="GetPageFooterColumnWidth">
                      <xsl:with-param name="pType" select="$gReportHeaderFooter/fCLastPg/text()" />
                      <xsl:with-param name="pDefaultWidth" select="$vPageA4FooterCenterWidth" />
                    </xsl:call-template>
                  </xsl:attribute>
                </fo:table-column>
                <fo:table-column border="{$gcTableBorderDebug}" column-number="03">
                  <xsl:attribute name="column-width">
                    <xsl:call-template name="GetPageFooterColumnWidth">
                      <xsl:with-param name="pType" select="$gReportHeaderFooter/fRLastPg/text()" />
                      <xsl:with-param name="pDefaultWidth" select="$vPageA4FooterRightWidth" />
                    </xsl:call-template>
                  </xsl:attribute>
                </fo:table-column>
              </xsl:when>
              <xsl:otherwise>
                <fo:table-column border="{$gcTableBorderDebug}" column-number="01">
                  <xsl:attribute name="column-width">
                    <xsl:call-template name="GetPageFooterColumnWidth">
                      <xsl:with-param name="pType" select="$gReportHeaderFooter/fLAllPg/text()" />
                      <xsl:with-param name="pDefaultWidth" select="$vPageA4FooterLeftWidth" />
                    </xsl:call-template>
                  </xsl:attribute>
                </fo:table-column>
                <fo:table-column border="{$gcTableBorderDebug}" column-number="02">
                  <xsl:attribute name="column-width">
                    <xsl:call-template name="GetPageFooterColumnWidth">
                      <xsl:with-param name="pType" select="$gReportHeaderFooter/fCAllPg/text()" />
                      <xsl:with-param name="pDefaultWidth" select="$vPageA4FooterCenterWidth" />
                    </xsl:call-template>
                  </xsl:attribute>
                </fo:table-column>
                <fo:table-column border="{$gcTableBorderDebug}" column-number="03">
                  <xsl:attribute name="column-width">
                    <xsl:call-template name="GetPageFooterColumnWidth">
                      <xsl:with-param name="pType" select="$gReportHeaderFooter/fRAllPg/text()" />
                      <xsl:with-param name="pDefaultWidth" select="$vPageA4FooterRightWidth" />
                    </xsl:call-template>
                  </xsl:attribute>
                </fo:table-column>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:otherwise>
            <fo:table-column border="{$gcTableBorderDebug}" column-width="{$vPageA4FooterLeftWidth}" column-number="01" />
            <fo:table-column border="{$gcTableBorderDebug}" column-width="{$vPageA4FooterCenterWidth}" column-number="02" />
            <fo:table-column border="{$gcTableBorderDebug}" column-width="{$gPageA4VerticalFooterPageNumberWidth}" column-number="03" />
          </xsl:otherwise>
        </xsl:choose>

        <fo:table-body>
          <fo:table-row font-weight="normal">
            <xsl:choose>
              <xsl:when test="$gReportHeaderFooter">
                <xsl:choose>
                  <xsl:when test="$pIsLastPage=true()">

                    <xsl:variable name="vModelSpan">
                      <xsl:call-template name="GetPageFooterSpanModel">
                        <xsl:with-param name="pLPg" select="$gReportHeaderFooter/fLLastPg/text()" />
                        <xsl:with-param name="pCPg" select="$gReportHeaderFooter/fCLastPg/text()" />
                        <xsl:with-param name="pRPg" select="$gReportHeaderFooter/fRLastPg/text()" />
                      </xsl:call-template>
                    </xsl:variable>

                    <!--fLLastPg-->
                    <xsl:call-template name="DisplayPageFooterType">
                      <xsl:with-param name="pType" select="$gReportHeaderFooter/fLLastPg/text()" />
                      <xsl:with-param name="pCustom" select="$gReportHeaderFooter/fLLastPgCustom/text()" />
                      <xsl:with-param name="pAlign" select="'left'" />
                      <xsl:with-param name="pModelSpan" select="$vModelSpan" />
                    </xsl:call-template>
                    <!--fCLastPg-->
                    <xsl:call-template name="DisplayPageFooterType">
                      <xsl:with-param name="pType" select="$gReportHeaderFooter/fCLastPg/text()" />
                      <xsl:with-param name="pCustom" select="$gReportHeaderFooter/fCLastPgCustom/text()" />
                      <xsl:with-param name="pAlign" select="'center'" />
                      <xsl:with-param name="pModelSpan" select="$vModelSpan" />
                    </xsl:call-template>
                    <!--fRLastPg-->
                    <xsl:call-template name="DisplayPageFooterType">
                      <xsl:with-param name="pType" select="$gReportHeaderFooter/fRLastPg/text()" />
                      <xsl:with-param name="pCustom" select="$gReportHeaderFooter/fRLastPgCustom/text()" />
                      <xsl:with-param name="pAlign" select="'right'" />
                      <xsl:with-param name="pModelSpan" select="$vModelSpan" />
                    </xsl:call-template>
                  </xsl:when>
                  <xsl:otherwise>

                    <xsl:variable name="vModelSpan">
                      <xsl:call-template name="GetPageFooterSpanModel">
                        <xsl:with-param name="pLPg" select="$gReportHeaderFooter/fLAllPg/text()" />
                        <xsl:with-param name="pCPg" select="$gReportHeaderFooter/fCAllPg/text()" />
                        <xsl:with-param name="pRPg" select="$gReportHeaderFooter/fRAllPg/text()" />
                      </xsl:call-template>
                    </xsl:variable>

                    <!--fLAllPg-->
                    <xsl:call-template name="DisplayPageFooterType">
                      <xsl:with-param name="pType" select="$gReportHeaderFooter/fLAllPg/text()" />
                      <xsl:with-param name="pCustom" select="$gReportHeaderFooter/fLAllPgCustom/text()" />
                      <xsl:with-param name="pAlign" select="'left'" />
                      <xsl:with-param name="pModelSpan" select="$vModelSpan" />
                    </xsl:call-template>
                    <!--fCAllPg-->
                    <xsl:call-template name="DisplayPageFooterType">
                      <xsl:with-param name="pType" select="$gReportHeaderFooter/fCAllPg/text()" />
                      <xsl:with-param name="pCustom" select="$gReportHeaderFooter/fCAllPgCustom/text()" />
                      <xsl:with-param name="pAlign" select="'center'" />
                      <xsl:with-param name="pModelSpan" select="$vModelSpan" />
                    </xsl:call-template>
                    <!--fRAllPg-->
                    <xsl:call-template name="DisplayPageFooterType">
                      <xsl:with-param name="pType" select="$gReportHeaderFooter/fRAllPg/text()" />
                      <xsl:with-param name="pCustom" select="$gReportHeaderFooter/fRAllPgCustom/text()" />
                      <xsl:with-param name="pAlign" select="'right'" />
                      <xsl:with-param name="pModelSpan" select="$vModelSpan" />
                    </xsl:call-template>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>
              <xsl:otherwise>
                <!-- Entity's adress (ex. HPC 22 rue des Capucines 75005 Paris) -->
                <xsl:call-template name="DisplayPageFooterType">
                  <xsl:with-param name="pType" select="'LegalInfo_Model2'" />
                  <xsl:with-param name="pAlign" select="'center'" />
                  <xsl:with-param name="pModelSpan" select="'2'" />
                </xsl:call-template>
                <!-- PageNumber -->
                <xsl:call-template name="DisplayPageFooterType">
                  <xsl:with-param name="pType" select="'PageNumber'" />
                  <xsl:with-param name="pAlign" select="'right'" />
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </xsl:if>
  </xsl:template>
  <!--DisplayPageFooterType template-->
  <xsl:template name="DisplayPageFooterType">
    <xsl:param name="pType" />
    <xsl:param name="pCustom" />
    <xsl:param name="pAlign" />
    <xsl:param name="pModelSpan" />

    <xsl:choose>
      <!-- Entity's adress (ex. HPC 22 rue des Capucines 75005 Paris) -->
      <xsl:when test="starts-with($pType,'LegalInfo_Model')">
        <fo:table-cell number-columns-spanned="{$pModelSpan}"
                       padding="1mm" border="{$gcTableBorderDebug}"
                       color="{$gPageA4VerticalFooterTextColor}"
                       text-align="{$pAlign}"
                       display-align="before">
          <xsl:call-template name="DisplayPageFooterType_LegalInfoModel">
            <xsl:with-param name="pType" select="$pType" />
          </xsl:call-template>
        </fo:table-cell>
      </xsl:when>
      <!-- PageNumber -->
      <xsl:when test="$pType='PageNumber'">
        <fo:table-cell padding="1mm"
                       border="{$gcTableBorderDebug}"
                       text-align="{$pAlign}"
                       display-align="before"
                       font-size="{$gPageA4VerticalFooterTextFontSize}pt">
          <fo:block>
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Page'" />
            </xsl:call-template>
            <xsl:value-of select="$gcEspace" />
            <fo:page-number />/<fo:page-number-citation ref-id="LastPage" />
          </fo:block>
        </fo:table-cell>
      </xsl:when>
      <!-- Custom -->
      <xsl:when test="$pType='Custom'">
        <fo:table-cell padding="1mm"
                       border="{$gcTableBorderDebug}"
                       text-align="{$pAlign}"
                       display-align="before"
                       font-size="{$gPageA4VerticalFooterTextFontSize}pt">
          <fo:block>
            <xsl:value-of select="$pCustom" />
          </fo:block>
        </fo:table-cell>
      </xsl:when>
      <xsl:when test="$pType='None' and $pModelSpan='1'">
        <fo:table-cell padding="1mm"
                       border="{$gcTableBorderDebug}">
          <fo:block/>
        </fo:table-cell>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  <!--DisplayPageFooterType_LegalInfoModel template-->
  <xsl:template name="DisplayPageFooterType_LegalInfoModel">
    <xsl:param name="pType" />

    <fo:block linefeed-treatment="preserve" font-size="{$gPageA4VerticalFooterTextFontSize}pt">
      <!-- Entity's adress (ex. HPC 22 rue des Capucines 75005 Paris) -->
      <xsl:for-each select="$gDataDocumentHeader/sendBy/routingIdsAndExplicitDetails/routingAddress/streetAddress/streetLine">
        <xsl:value-of select="." />
        <xsl:value-of select="$gcEspace" />
      </xsl:for-each>
    </fo:block>

    <xsl:if test="$pType='LegalInfo_Model2' or $pType='LegalInfo_Model3'">
      <fo:block linefeed-treatment="preserve" font-size="{$gPageA4VerticalFooterTextFontSize}pt">
        <!-- Phone -->
        <xsl:variable name="vPhone">
          <xsl:call-template name="PageFooterDisplay_Phone">
            <xsl:with-param name="pWithSeparator" select="false()"/>
          </xsl:call-template>
        </xsl:variable>
        <!-- Telex -->
        <xsl:variable name="vTelex">
          <xsl:call-template name="PageFooterDisplay_Telex">
            <xsl:with-param name="pWithSeparator" select="string-length($vPhone)>0"/>
          </xsl:call-template>
        </xsl:variable>
        <!-- Fax -->
        <xsl:variable name="vFax">
          <xsl:call-template name="PageFooterDisplay_Fax">
            <xsl:with-param name="pWithSeparator" select="string-length($vPhone)>0 or string-length($vTelex)>0"/>
          </xsl:call-template>
        </xsl:variable>

        <!-- Mail -->
        <xsl:variable name="vMail">
          <xsl:if test="$pType='LegalInfo_Model2'">
            <xsl:call-template name="PageFooterDisplay_Mail">
              <xsl:with-param name="pWithSeparator" select="string-length($vPhone)>0 or string-length($vTelex)>0 or string-length($vFax)>0"/>
            </xsl:call-template>
          </xsl:if>
        </xsl:variable>

        <xsl:value-of select="concat($vPhone,$vTelex,$vFax,$vMail)"/>
      </fo:block>
    </xsl:if>

    <xsl:if test="$pType='LegalInfo_Model3'">
      <fo:block linefeed-treatment="preserve" font-size="{$gPageA4VerticalFooterTextFontSize}pt">
        <!-- Mail -->
        <xsl:call-template name="PageFooterDisplay_Mail">
          <xsl:with-param name="pWithSeparator" select="false()"/>
        </xsl:call-template>
      </fo:block>
    </xsl:if>

  </xsl:template>

  <!--PageFooterDisplay_Phone-->
  <xsl:template name="PageFooterDisplay_Phone">
    <xsl:param name="pWithSeparator" select="true()"/>

    <xsl:call-template name="DisplayPageFooterType_LegalInfoModel_Element">
      <xsl:with-param name="pRessource" select="'INV-Phone'"/>
      <xsl:with-param name="pData" select="$gEntityPhone"/>
      <xsl:with-param name="pWithSeparator" select="$pWithSeparator"/>
    </xsl:call-template>
  </xsl:template>
  <!--PageFooterDisplay_Telex-->
  <xsl:template name="PageFooterDisplay_Telex">
    <xsl:param name="pWithSeparator" select="true()"/>
    <!-- Telex -->
    <xsl:call-template name="DisplayPageFooterType_LegalInfoModel_Element">
      <xsl:with-param name="pRessource" select="'INV-Telex'"/>
      <xsl:with-param name="pData" select="$gEntityTelex"/>
      <xsl:with-param name="pWithSeparator" select="$pWithSeparator"/>
    </xsl:call-template>
  </xsl:template>
  <!--PageFooterDisplay_Fax-->
  <xsl:template name="PageFooterDisplay_Fax">
    <xsl:param name="pWithSeparator" select="true()"/>
    <xsl:call-template name="DisplayPageFooterType_LegalInfoModel_Element">
      <xsl:with-param name="pRessource" select="'INV-Fax'"/>
      <xsl:with-param name="pData" select="$gEntityFax"/>
      <xsl:with-param name="pWithSeparator" select="$pWithSeparator"/>
    </xsl:call-template>
  </xsl:template>
  <!--PageFooterDisplay_Mail-->
  <xsl:template name="PageFooterDisplay_Mail">
    <xsl:param name="pWithSeparator" select="true()"/>
    <xsl:call-template name="DisplayPageFooterType_LegalInfoModel_Element">
      <xsl:with-param name="pRessource" select="'INV-Mail'"/>
      <xsl:with-param name="pData" select="$gEntityMail"/>
      <xsl:with-param name="pWithSeparator" select="$pWithSeparator"/>
    </xsl:call-template>
  </xsl:template>

  <!--DisplayPageFooterType_LegalInfoModel_Element-->
  <xsl:template name="DisplayPageFooterType_LegalInfoModel_Element">
    <xsl:param name="pRessource"/>
    <xsl:param name="pData"/>
    <xsl:param name="pWithSeparator" select="true()"/>

    <xsl:if test="string-length($pData) &gt; 0">
      <xsl:if test="$pWithSeparator = true()">
        <xsl:value-of select="concat($gcEspace,'-',$gcEspace)" />
      </xsl:if>
      <xsl:call-template name="getSpheresTranslation">
        <xsl:with-param name="pResourceName" select="$pRessource" />
      </xsl:call-template>
      <xsl:value-of select="concat(':',$gcEspace)" />
      <xsl:value-of select="$pData" />
    </xsl:if>
  </xsl:template>

  <!--GetPageFooterColumnWidth-->
  <xsl:template name="GetPageFooterColumnWidth">
    <xsl:param name="pType" />
    <xsl:param name="pDefaultWidth" />

    <xsl:choose>
      <xsl:when test="$pType='PageNumber'">
        <xsl:value-of select="$gPageA4VerticalFooterPageNumberWidth"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pDefaultWidth"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!--GetPageFooterSpanModel-->
  <xsl:template name="GetPageFooterSpanModel">
    <xsl:param name="pLPg" />
    <xsl:param name="pCPg" />
    <xsl:param name="pRPg" />

    <xsl:choose>
      <xsl:when test="starts-with($pLPg,'LegalInfo_Model') and $pCPg='None' and $pRPg='None'">3</xsl:when>
      <xsl:when test="starts-with($pLPg,'LegalInfo_Model') and $pCPg='None'">2</xsl:when>
      <xsl:when test="starts-with($pCPg,'LegalInfo_Model') and $pLPg='None' and $pRPg='None'">3</xsl:when>
      <xsl:when test="starts-with($pCPg,'LegalInfo_Model') and ($pLPg='None' or $pRPg='None')">2</xsl:when>
      <xsl:when test="starts-with($pRPg,'LegalInfo_Model') and $pCPg='None' and $pLPg='None'">3</xsl:when>
      <xsl:when test="starts-with($pRPg,'LegalInfo_Model') and $pCPg='None'">2</xsl:when>
      <xsl:otherwise>1</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!--DisplayPageFooterEnums template-->
  <xsl:template name="DisplayPageFooterEnums">
    <xsl:param name="pEnums" />
    <xsl:param name="pEnumsResource" />

    <fo:table table-layout="fixed"
              border="{$gcTableBorderLegend}"
              width="{$gPageA4VerticalFooterWidth}"
              font-size="{$gDetTrdDetFontSize}">
      <fo:table-column column-width="29mm" column-number="01" />
      <fo:table-column column-width="24mm" column-number="02" />
      <fo:table-column column-width="24mm" column-number="03" />
      <fo:table-column column-width="24mm" column-number="04" />
      <fo:table-column column-width="24mm" column-number="05" />
      <fo:table-column column-width="24mm" column-number="06" />
      <fo:table-column column-width="24mm" column-number="07" />
      <fo:table-column column-width="24mm" column-number="08" />
      <fo:table-body>
        <fo:table-row font-weight="normal">
          <fo:table-cell padding="{$gDetTrdCellPadding}" text-align="justify" display-align="before">
            <fo:block linefeed-treatment="preserve">
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="$pEnumsResource" />
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
          <xsl:for-each select="$pEnums[7 >= position()]">
            <xsl:call-template name="DisplayPageFooterEnumsCell">
              <xsl:with-param name="pEnum" select="." />
            </xsl:call-template>
          </xsl:for-each>
        </fo:table-row>

        <xsl:variable name="vRowCount" select="(number($pEnums) div 7)"/>

        <xsl:if test="vRowCount > 0">
          <xsl:variable name="vRowCountToDisplay">
            <xsl:choose>
              <xsl:when test="number($pEnums) > ($vRowCount*7)">
                <xsl:value-of select="$vRowCount+1"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$vRowCount"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:if test="$vRowCountToDisplay > 1">
            <xsl:call-template name="DisplayPageFooterEnumsRow">
              <xsl:with-param name="pEnums" select="$pEnums" />
              <xsl:with-param name="pRowCountToDisplay" select="$vRowCountToDisplay" />
              <xsl:with-param name="pRowNumber" select="number('2')" />
            </xsl:call-template>
          </xsl:if>
        </xsl:if>
      </fo:table-body>
    </fo:table>
    <!--Space-->
    <fo:table table-layout="fixed"
              border="{$gcTableBorderDebug}"
              width="{$gPageA4VerticalFooterWidth}"
              font-size="{$gDetTrdDetFontSize}">
      <fo:table-column column-width="{$gPageA4VerticalFooterWidth}" column-number="01" />
      <fo:table-body>
        <fo:table-row font-weight="normal" height="1mm">
          <fo:table-cell text-align="left" display-align="center">
            <fo:block />
          </fo:table-cell>
        </fo:table-row>
      </fo:table-body>
    </fo:table>
  </xsl:template>
  <!--DisplayPageFooterEnumsRow template-->
  <xsl:template name="DisplayPageFooterEnumsRow">
    <xsl:param name="pEnums" />
    <xsl:param name="pRowCountToDisplay" />
    <xsl:param name="pRowNumber" />

    <xsl:if test="$pRowCountToDisplay >= $pRowNumber">

      <fo:table-row font-weight="normal">
        <xsl:call-template name="DisplayPageFooterEnumsCell"/>

        <xsl:for-each select="$pEnums[(number($pRowNumber)*7 >= position()) and (position() > (number($pRowNumber)-1)*7)]">
          <xsl:call-template name="DisplayPageFooterEnumsCell">
            <xsl:with-param name="pEnum" select="." />
          </xsl:call-template>
        </xsl:for-each>
      </fo:table-row>

      <xsl:call-template name="DisplayPageFooterEnumsRow">
        <xsl:with-param name="pEnums" select="$pEnums" />
        <xsl:with-param name="pRowCountToDisplay" select="$pRowCountToDisplay" />
        <xsl:with-param name="pRowNumber" select="$pRowNumber+1" />
      </xsl:call-template>
    </xsl:if>

  </xsl:template>
  <!--DisplayPageFooterEnumsCell template-->
  <xsl:template name="DisplayPageFooterEnumsCell">
    <xsl:param name="pEnum" />

    <fo:table-cell padding="{$gDetTrdCellPadding}" text-align="justify" display-align="before">
      <fo:block linefeed-treatment="preserve">
        <fo:inline font-weight="bold">
          <xsl:choose>
            <xsl:when test="string-length($pEnum/extattrb/text())>0">
              <xsl:value-of select="$pEnum/extattrb/text()" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$pEnum/value/text()" />
            </xsl:otherwise>
          </xsl:choose>
        </fo:inline>
        <xsl:value-of select="$gcEspace" />
        <xsl:value-of select="$gcEspace" />
        <xsl:value-of select="$pEnum/extvalue/text()" />
      </fo:block>
    </fo:table-cell>
  </xsl:template>

  <!-- .................................. -->
  <!--   Body                             -->
  <!-- .................................. -->
  <!--DisplayPageBodyCommon template-->
  <xsl:template name="DisplayPageBodyCommon">
    <xsl:param name="pBook" />
    <xsl:param name="pBookPosition" />
    <xsl:param name="pCountBook" />
    <xsl:param name="pSetLastPage" select="true()" />
    <xsl:param name="pGroups" />
    <xsl:param name="pIsDisplayStarLegend" />
    <xsl:param name="pIsHFormulaMandatory" select="true()" />

    <!--Affichage du début de la page: Titre, Adresse, Book, ...-->
    <xsl:call-template name="DisplayPageBodyStart">
      <xsl:with-param name="pBook" select="$pBook"/>
    </xsl:call-template>
    <!--Display first paragraph ( introduction text)-->
    <xsl:variable name="vFormula">
      <xsl:choose>
        <xsl:when test="string-length($gReportHeaderFooter/hFormula/text())>0">
          <xsl:value-of select="$gReportHeaderFooter/hFormula/text()"/>
        </xsl:when>
        <xsl:when test="$pIsHFormulaMandatory">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="concat($gReportType,'-TitleDetailed')" />
          </xsl:call-template>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:call-template name="DisplayStatementTitle">
      <xsl:with-param name="pTitle" select="$vFormula" />
      <xsl:with-param name="pTitleFontSize" select="$gFormulaFontSize" />
      <xsl:with-param name="pTitleFontWeight" select="$gFormulaFontWeight" />
      <xsl:with-param name="pTitleTextAlign" select="$gFormulaTextAlign" />
      <xsl:with-param name="pTitleWidth" select="$gPageA4VerticalFormulaWidth" />
      <xsl:with-param name="pTitlePaddingTop" select="$gFormulaPaddingTop" />
      <xsl:with-param name="pTitleLeftMargin" select="$gFormulaLeftMargin" />
    </xsl:call-template>

    <!--==========================================================================================-->
    <!-- Display Book's Trades using sorting keys and subtotals                                   -->
    <!--==========================================================================================-->
    <fo:block>
      <xsl:apply-templates select="msxsl:node-set($pGroups)/groups/group" mode="display">
        <xsl:sort select="@sort" data-type="number"/>
      </xsl:apply-templates>
    </fo:block>

    <xsl:if test="$pSetLastPage">
      <!-- 20120522 MF Ticket 17788-->
      <!-- RD 20141120 [20504] Afficher un Message personnalisé en dernière page du document indépendamment de l'affichage ou pas des Legendes.-->
      <!--<xsl:if test="($gReportHeaderFooter=false()) or ($gReportHeaderFooter/fLegend/text()='LastPageOnly')">-->
      <fo:block display-align="after" padding-top="5mm">
        <xsl:call-template name="DisplayLegend_LastPage">
          <xsl:with-param name="pIsDisplayStarLegend" select="$pIsDisplayStarLegend"/>
        </xsl:call-template>
      </fo:block>
      <!--</xsl:if>-->
      <!-- 20120522 MF Ticket 17788-->

      <!--//-->
      <!--If is LastBook-->
      <xsl:if test="$pCountBook = $pBookPosition">
        <fo:block id="LastPage"/>
      </xsl:if>
    </xsl:if>
  </xsl:template>
  <!--DisplayPageBodyStartCommon template-->
  <xsl:template name="DisplayPageBodyStartCommon">
    <xsl:param name="pBook" />

    <xsl:variable name="vTitle">
      <xsl:choose>
        <xsl:when test="string-length($gReportHeaderFooter/hTitle/text())>0">
          <xsl:value-of select="$gReportHeaderFooter/hTitle/text()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="concat($gReportType,'-Title')" />
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vTitlePosition">
      <xsl:choose>
        <xsl:when test="string-length($gReportHeaderFooter/hTitlePosition/text())>0">
          <xsl:call-template name="Lower">
            <xsl:with-param name="source" select="$gReportHeaderFooter/hTitlePosition/text()"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$gMainTitleTextAlign"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!--//-->
    <!-- Display Statement title -->
    <xsl:call-template name="DisplayStatementTitle">
      <xsl:with-param name="pTitle" select="$vTitle" />
      <xsl:with-param name="pTitleFontSize" select="$gMainTitleFontSize" />
      <xsl:with-param name="pTitleFontWeight" select="$gMainTitleFontWeight" />
      <xsl:with-param name="pTitleTextAlign" select="$vTitlePosition" />
      <xsl:with-param name="pTitleWidth" select="$gMainTitleWidth" />
      <xsl:with-param name="pTitlePaddingTop" select="$gMainTitlePaddingTop" />
      <xsl:with-param name="pTitleLeftMargin" select="$gMainTitleLeftMargin" />
    </xsl:call-template>
    <!-- Display receiver name and address, Book and Clearing Date -->
    <xsl:call-template name="DisplayReceiver">
      <xsl:with-param name="pBook" select="$pBook" />
    </xsl:call-template>

  </xsl:template>
  <!--DisplayReceiver template-->
  <xsl:template name="DisplayReceiver">
    <xsl:param name="pBook" />
    <xsl:param name="pReceiverPaddingTop" select="$gReceiverPaddingTop" />

    <fo:table table-layout="fixed" padding-top="{$pReceiverPaddingTop}" border="{$gcTableBorderDebug}" width="{$gReceiverWidth}">
      <fo:table-column column-width="{$gReceiverBookWidth}" column-number="01" />
      <fo:table-column column-width="{$gReceiverAdressWidth}" column-number="02" />
      <fo:table-body>
        <fo:table-row font-weight="normal">
          <fo:table-cell/>
          <fo:table-cell>
            <fo:block>
              <xsl:call-template name="DisplayReceiverAddress" />
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
        <fo:table-row font-weight="normal">
          <fo:table-cell>
            <fo:block>
              <xsl:call-template name="DisplayReceiverDetailSpecific">
                <xsl:with-param name="pBook" select="$pBook" />
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell/>
        </fo:table-row>
      </fo:table-body>
    </fo:table>
  </xsl:template>
  <!--DisplayReceiverAddress template-->
  <xsl:template name="DisplayReceiverAddress">
    <fo:table table-layout="fixed" border="{$gcTableBorderDebug}" font-family="{$gReceiverAdressFontFamily}" font-size="{$gReceiverAdressFontSize}" width="{$gReceiverAdressWidth}">
      <fo:table-column column-width="{$gReceiverAdressWidth}" column-number="01" />
      <fo:table-body>
        <xsl:for-each select="$gDataDocumentHeader/sendTo/routingIdsAndExplicitDetails/routingAddress/streetAddress/streetLine">
          <fo:table-row font-weight="normal">
            <fo:table-cell margin-left="{$gReceiverAdressLeftMargin}" border="{$gcTableBorderDebug}" font-weight="normal" text-align="{$gReceiverAdressTextAlign}">
              <fo:block>
                <xsl:value-of select="." />
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </xsl:for-each>
      </fo:table-body>
    </fo:table>
  </xsl:template>
  <!--DisplayReceiverDetailCommon template-->
  <xsl:template name="DisplayReceiverDetailCommon">
    <xsl:param name="pBook" />
    <xsl:call-template name="DisplayBookDetail">
      <xsl:with-param name="pBook" select="$pBook" />
    </xsl:call-template>
  </xsl:template>
  <!--DisplayBookDetail template-->
  <xsl:template name="DisplayBookDetail">
    <xsl:param name="pBook" />
    <!-- 20120903 MF Ticket 18048 - the account statement needs a larger width -->
    <!--<xsl:variable name="vReceiverBookWidth">
    <xsl:choose>
      <xsl:when test="$gReportType = $gcReportTypeACCSTAT">
        <xsl:value-of select="$gReceiverBookWidthAccountStatement"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$gReceiverBookWidth"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>-->
    <fo:table table-layout="fixed"
                padding-top="{$gReceiverBookPaddingTop}"
                border="{$gcTableBorderDebug}"
                font-size="{$gReceiverBookFontSize}"
                width="{$gReceiverWidth}">
      <fo:table-column column-width="{$gReceiverBookIdentWidth}" column-number="01" />

      <!-- 20120903 MF Ticket 18048 - the account statement needs a larger width -->
      <!--<xsl:variable name="vReceiverBookNameWidth">
        <xsl:choose>
          <xsl:when test="$gReportType = $gcReportTypeACCSTAT">
            <xsl:value-of select="$gReceiverBookNameWidthAccountStatement"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$gReceiverBookNameWidth"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>-->
      <fo:table-column column-width="{$gReceiverBookNameWidth}" column-number="02" />
      <fo:table-body>
        <xsl:if test="$gIsMonoBook = 'true'">
          <xsl:variable name="vBookRepository" select="$gDataDocumentRepository/book[identifier=$pBook]"/>
          <xsl:variable name="vBookDisplayName" select="$vBookRepository/displayname"/>
          <xsl:variable name="vBookCurrency" select="$vBookRepository/idc"/>
          <!--//-->
          <fo:table-row font-weight="normal">
            <fo:table-cell margin-left="{$gReceiverBookLeftMargin}"
                           border="{$gcTableBorderDebug}"
                           text-align="{$gReceiverBookTextAlign}">
              <fo:block>
                <xsl:choose>
                  <xsl:when test="string-length($gReportHeaderFooter/hBookIdLabel/text())>0">
                    <xsl:value-of select="$gReportHeaderFooter/hBookIdLabel/text()"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:call-template name="getSpheresTranslation">
                      <xsl:with-param name="pResourceName" select="'TS-Account'" />
                    </xsl:call-template>
                    <xsl:value-of select ="string(' : ')"/>
                  </xsl:otherwise>
                </xsl:choose>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell margin-left="{$gReceiverBookLeftMargin}"
                           border="{$gcTableBorderDebug}"
                           text-align="{$gReceiverBookTextAlign}">
              <fo:block>
                <fo:inline font-size="{$gReceiverBookFontSizeAccount}" font-weight="bold">
                  <xsl:value-of select="$pBook" />
                </fo:inline>
                <xsl:if test="string-length($vBookCurrency) > 0">
                  <fo:inline font-size="{$gReceiverBookFontSizeCurrency}" font-weight="normal">
                    <xsl:value-of select="concat($gcEspace,'(',$vBookCurrency,')')" />
                  </fo:inline>
                </xsl:if>
              </fo:block>
              <xsl:if test="(string-length($vBookDisplayName) > 0) and  (string($vBookDisplayName) != string($pBook))">
                <fo:block font-weight="normal" font-size="{$gReceiverBookFontSizeDisplayname}">
                  <xsl:value-of select="$vBookDisplayName" />
                </fo:block>
              </xsl:if>
            </fo:table-cell>
          </fo:table-row>
        </xsl:if>
        <xsl:call-template name="DisplayBookAdditionalDetail" />
      </fo:table-body>
    </fo:table>
  </xsl:template>
  <!--CommonDisplayBookAdditionalDetail template-->
  <xsl:template name="CommonDisplayBookAdditionalDetail">

    <fo:table-row font-weight="normal">
      <fo:table-cell margin-left="{$gReceiverAdressLeftMargin}" border="{$gcTableBorderDebug}" text-align="{$gReceiverBookTextAlign}">
        <fo:block>
          <!-- 20120903 MF Ticket 18048 - the account statement report is related to a specific reource -->
          <xsl:choose>
            <xsl:when test="$gReportType = $gcReportTypeACCSTAT">
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'TS-PeriodFrom'" />
              </xsl:call-template>
              <xsl:value-of select ="string(' ')"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:choose>
                <xsl:when test="string-length($gReportHeaderFooter/hDtBusinessLabel/text())>0">
                  <xsl:value-of select="$gReportHeaderFooter/hDtBusinessLabel/text()"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="getSpheresTranslation">
                    <xsl:with-param name="pResourceName" select="'TS-ClearingDate'" />
                  </xsl:call-template>
                  <xsl:value-of select ="string(' : ')"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:otherwise>
          </xsl:choose>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell margin-left="{$gReceiverAdressLeftMargin}" border="{$gcTableBorderDebug}" text-align="{$gReceiverBookTextAlign}">
        <!-- 20120903 MF Ticket 18048 - Adding the end date of the account statement period (for account statement report only) -->
        <xsl:choose>
          <xsl:when test="$gReportType = $gcReportTypeACCSTAT">
            <fo:block font-weight="bold">
              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="$gClearingDate" />
              </xsl:call-template>
              <fo:inline font-weight="normal">
                <xsl:value-of select ="string(' ')"/>
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'INV-To'" />
                </xsl:call-template>
                <xsl:value-of select ="string(' ')"/>
              </fo:inline>
              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="$gEndPeriodDate" />
              </xsl:call-template>
            </fo:block>
          </xsl:when>
          <xsl:otherwise>
            <fo:block font-weight="bold">
              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="$gClearingDate" />
              </xsl:call-template>
            </fo:block>
          </xsl:otherwise>
        </xsl:choose>
      </fo:table-cell>
    </fo:table-row>
  </xsl:template>

  <!-- .................................. -->
  <!--   Other                            -->
  <!-- .................................. -->
  <!--DisplayStatementTitle template-->
  <xsl:template name="DisplayStatementTitle">
    <xsl:param name="pTitle" />
    <xsl:param name="pTitleFontSize" />
    <xsl:param name="pTitleFontWeight" select="'normal'" />
    <xsl:param name="pTitleTextAlign" />
    <xsl:param name="pTitleWidth" />
    <xsl:param name="pTitlePaddingTop" />
    <xsl:param name="pTitlePaddingBottom"/>
    <xsl:param name="pTitleLeftMargin" />

    <fo:table table-layout="fixed" padding-top="{$pTitlePaddingTop}" border="{$gcTableBorderDebug}" font-size="{$pTitleFontSize}" width="{$pTitleWidth}">
      <xsl:if test="$pTitlePaddingBottom">
        <xsl:attribute name="padding-bottom">
          <xsl:value-of select="$pTitlePaddingBottom"/>
        </xsl:attribute>
      </xsl:if>
      <fo:table-column column-width="{$pTitleWidth}" column-number="01" />
      <fo:table-body>
        <fo:table-row font-weight="normal">
          <fo:table-cell margin-left="{$pTitleLeftMargin}" border="{$gcTableBorderDebug}" font-weight="{$pTitleFontWeight}" text-align="{$pTitleTextAlign}">
            <fo:block linefeed-treatment='preserve' white-space-treatment='preserve' white-space-collapse='false'>
              <xsl:value-of select="$pTitle"/>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
      </fo:table-body>
    </fo:table>
  </xsl:template>

</xsl:stylesheet>
