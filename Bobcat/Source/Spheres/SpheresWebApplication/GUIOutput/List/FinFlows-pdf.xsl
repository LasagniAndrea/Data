<?xml version="1.0" encoding="UTF-8"?>
<!--
=============================================================================================
Summary : Financial flows aggregated by asset (FLOWSBYASSET_ALLOC)
          Financial Flows
File    : FinFlows-pdf.xsl
=============================================================================================
Version : v5.1.5883                                           
Date    : 20160209
Author  : RD
Comment : [20680] 
- Ajout des colonnes Dev. CV (IDCREPORT), Cours spot (SPOTRATE) et Résultat Réalisé CV (REALMARGINAMT_CV)
=============================================================================================
Version : v4.5.5653                                           
Date    : 20150624
Author  : RD
Comment : [21067] 
- Supprimer le Résultat Réalisé du Total
- Numéroter les colonnes incluses dans le Total
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
Comment : [18685] First version     
=============================================================================================
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
                xmlns:msxsl="urn:schemas-microsoft-com:xslt" 
                xmlns:dt="http://xsltsl.org/date-time" 
                xmlns:fo="http://www.w3.org/1999/XSL/Format" version="1.0">
  
  <!-- xslt import -->
  <xsl:import href="Shared\List_Common-pdf.xslt"/>
  
  <!-- Keys declaration-->
  <xsl:key name="kParentActorKey" match="Column[@ColumnName='VW_B_AL2_IDENTIFIER']" use="text()"/>

  <xsl:key name="kActorKey" match="Column[@ColumnName='A_ALLOC_P_IDENTIFIER']" use="text()"/>
  <xsl:key name="kCurrencyKey" match="Column[@ColumnName='FLOWS_ASS_UNIT']" use="text()"/>
  <xsl:key name="kIDCREPORTKey" match="Column[@ColumnName='FLOWS_ASS_CV_IDCREPORT']" use="text()"/>
  <xsl:key name="kBookKey" match="Column[@ColumnName='B_ALLOC_P_IDENTIFIER']" use="text()"/>
  <xsl:key name="kDCKey" match="Column[@ColumnName='DC_IDENTIFIER']" use="text()"/>
  <xsl:key name="kPCKey" match="Column[@ColumnName='A_ETD_PUTCALL']" use="text()"/>

  <xsl:key name="kIOActorKey" match="column[@name='A_ALLOC_P_IDENTIFIER']" use="text()"/>
  <xsl:key name="kIOCurrencyKey" match="column[@name='FLOWS_ASS_UNIT']" use="text()"/>
  <xsl:key name="kIOIDCREPORTKey" match="column[@name='FLOWS_ASS_CV_IDCREPORT']" use="text()"/>
  <xsl:key name="kIOBookKey" match="column[@name='B_ALLOC_P_IDENTIFIER']" use="text()"/>
  <xsl:key name="kIODCKey" match="column[@name='DC_IDENTIFIER']" use="text()"/>
  <xsl:key name="kIOPCKey" match="column[@name='A_ETD_PUTCALL']" use="text()"/>

  <!-- ================================================================ -->
  <!-- FinFlows Report settings                                         -->
  <!-- ================================================================ -->
  <xsl:variable name="gReportTitle" select="'Report-FinFlows-title'"/>
  <xsl:variable name="gReportNoInformationDet" select="'Report-FinFlows-NoInformationDet'"/>
  <xsl:variable name="gColumnParentActor" select="'VW_B_AL2_IDENTIFIER'"/>

  <!--Exclude subtotal rows-->
  <!--<xsl:variable name="gReportRows" select="$gReportReferential/Row[Column[@ColumnName='GROUPBYNUMBER' and text()='0']]"/>-->
  <xsl:variable name="gReportRows" select="$gReportReferential/Row[Column[translate(@ColumnName, $vLowerLetters, $vUpperLetters)='GROUPBYNUMBER' and text()='0']]"/>
  <xsl:variable name="gReportSPOTRATEListNode">
    <xsl:call-template name="GetSPOTRATEList"/>
  </xsl:variable>
  <xsl:variable name="gReportSPOTRATEList" select="msxsl:node-set($gReportSPOTRATEListNode)/Row"/>

  <xsl:variable name="gIsByParentActor" select="$gReportRows[Column[@ColumnName=$gColumnParentActor]]"/>
  <xsl:variable name="gIsByActor" select="false()"/>
  <xsl:variable name="gColumnNumber" select="16"/>

  <xsl:variable name="gTotalSpace">0.05cm</xsl:variable>
  <!-- ================================================================ -->
  <!-- Override Templates                                               -->
  <!-- ================================================================ -->
  <xsl:template name="DisplayDocumentContent">
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <xsl:choose>
      <xsl:when test="$gIsByActor">
        <xsl:choose>
          <xsl:when test="$gIsByParentActor">
            <xsl:call-template name="DisplayDocumentContentByParentActor">
              <xsl:with-param name="pCulture" select="$pCulture" />
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="DisplayDocumentContentByActor">
              <xsl:with-param name="pCulture" select="$pCulture" />
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="DisplayDocumentContentCommon">
          <xsl:with-param name="pCulture" select="$pCulture" />
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="AddEndOfDoc">
    <xsl:param name="pData"/>

    <xsl:choose>
      <xsl:when test="$gIsByActor">
        <fo:block id="{concat('EndOfDoc_',$pData)}"/>
      </xsl:when>
      <xsl:otherwise>
        <fo:block id="'EndOfDoc'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="displayPageNumber">
    <xsl:param name="pData"/>
    <xsl:choose>
      <xsl:when test="$gIsByActor">
        <fo:page-number/>/<fo:page-number-citation ref-id="{concat('EndOfDoc_',$pData)}"/>
      </xsl:when>
      <xsl:otherwise>
        <fo:page-number/>/<fo:page-number-citation ref-id="'EndOfDoc'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="displayDates">
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <fo:table-row height="{$vRowHeight}cm" text-align="left" font-size="{$vFontSizeHeader}" color="black" font-family="{$vFontFamily}">
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'TS-PeriodFrom'"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$vBorder}" font-weight="bold">
        <fo:block border="{$vBorder}">
          <xsl:variable name="vDate1">
            <xsl:call-template name ="getDate1">
              <xsl:with-param name="pCulture" select="$pCulture" />
            </xsl:call-template>
          </xsl:variable>
          <xsl:value-of select="concat($vDate1,' ')"/>
          <fo:inline font-weight="normal">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'INV-To'"/>
              <xsl:with-param name="pCulture" select="$pCulture" />
            </xsl:call-template>
          </fo:inline>
          <xsl:variable name="vDate2">
            <xsl:call-template name ="getDate2">
              <xsl:with-param name="pCulture" select="$pCulture" />
            </xsl:call-template>
          </xsl:variable>
          <xsl:value-of select="concat(' ',$vDate2)"/>
        </fo:block>
      </fo:table-cell>
    </fo:table-row>
    <xsl:call-template name="displayPublished">
      <xsl:with-param name="pCulture" select="$pCulture" />
    </xsl:call-template>
  </xsl:template>
  <xsl:template name="displayPageBodyHeader">
    <xsl:param name="pData"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <fo:table-row height="{$vRowHeight}cm" text-align="left" font-size="{$vFontSizeHeader}" color="black" font-family="{$vFontFamily}">
      <fo:table-cell border="{$vBorder}" display-align="after" text-align="center" font-size="14pt">
        <fo:block linefeed-treatment="preserve">
          <xsl:call-template name="displayBreakline"/>
          <xsl:call-template name="displayBreakline"/>
        </fo:block>
        <fo:block linefeed-treatment="preserve">
          <fo:table table-layout="fixed">
            <xsl:call-template name="createHeader2Columns"/>
            <fo:table-body>
              <xsl:call-template name="displayDates">
                <xsl:with-param name="pCulture" select="$pCulture" />
              </xsl:call-template>
            </fo:table-body>
          </fo:table>
        </fo:block>
        <fo:block linefeed-treatment="preserve">
          <xsl:call-template name="displayBreakline"/>
        </fo:block>
      </fo:table-cell>
    </fo:table-row>
  </xsl:template>
  <xsl:template name="displayPageBodyDetail">
    <xsl:param name="pData"/>
    <xsl:param name="pRows"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <fo:block linefeed-treatment="preserve">
      <fo:table table-layout="fixed">
        <xsl:call-template name="createBodyColumns"/>
        <fo:table-header>
          <xsl:call-template name="displayColumnTitles">
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </fo:table-header>
        <fo:table-footer>
          <xsl:call-template name="displayColumnLegends">
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </fo:table-footer>
        <fo:table-body>
          <xsl:choose>
            <xsl:when test="$gIsByActor">
              <xsl:choose>
                <xsl:when test="$pRows">
                  <xsl:choose>
                    <xsl:when test="$gIsByParentActor">
                      <xsl:call-template name="DisplayDocumentContentByActor">
                        <xsl:with-param name="pParentActor" select="$pData"/>
                        <xsl:with-param name="pRows" select="$pRows"/>
                        <xsl:with-param name="pCulture" select="$pCulture" />
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:call-template name="displayActor">
                        <xsl:with-param name="pActor" select="$pData"/>
                        <xsl:with-param name="pRows" select="$pRows"/>
                        <xsl:with-param name="pPosition" select="1"/>
                        <xsl:with-param name="pCulture" select="$pCulture" />
                      </xsl:call-template>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="displayNoData">
                    <xsl:with-param name="pCulture" select="$pCulture" />
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:otherwise>
              <xsl:choose>
                <xsl:when test="$gIsByParentActor">
                  <xsl:call-template name="DisplayDocumentContentByParentActor">
                    <xsl:with-param name="pCulture" select="$pCulture" />
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="DisplayDocumentContentByActor">
                    <xsl:with-param name="pCulture" select="$pCulture" />
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:otherwise>
          </xsl:choose>
        </fo:table-body>
      </fo:table>
    </fo:block>
  </xsl:template>
  <xsl:template name="createBodyColumns">
    <fo:table-column column-width="2.50cm" column-number="01"/>
    <fo:table-column column-width="2.50cm" column-number="02"/>
    <fo:table-column column-width="1.25cm" column-number="03"/>
    <fo:table-column column-width="1.25cm" column-number="04"/>
    <fo:table-column column-width="0.90cm" column-number="05"/>
    <fo:table-column column-width="0.90cm" column-number="06"/>
    <fo:table-column column-width="1.50cm" column-number="07"/>
    <fo:table-column column-width="1.15cm" column-number="08"/>
    <fo:table-column column-width="1.15cm" column-number="09"/>
    <fo:table-column column-width="2.25cm" column-number="10"/>
    <fo:table-column column-width="2.25cm" column-number="11"/>
    <fo:table-column column-width="2.25cm" column-number="12"/>
    <fo:table-column column-width="2.25cm" column-number="13"/>
    <fo:table-column column-width="2.25cm" column-number="14"/>
    <fo:table-column column-width="2.25cm" column-number="15"/>
    <fo:table-column column-width="2.25cm" column-number="16"/>
  </xsl:template>
  <xsl:template name="displayNoData">
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <fo:table-row>
      <fo:table-cell padding="{$gCellPadding}" text-align="left" number-columns-spanned="{$gColumnNumber}">
        <fo:block linefeed-treatment="preserve">
          <xsl:call-template name="displayBreakline"/>
          <xsl:call-template name="displayBreakline"/>
        </fo:block>
        <fo:block linefeed-treatment="preserve" text-align="center" font-size="{$vFontSizeFormula}" font-weight="bold" font-family="{$vFontFamily}">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Msg_NoInformation'"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </fo:block>
        <xsl:call-template name="displayBreakline"/>
        <fo:block linefeed-treatment="preserve" text-align="left" font-size="{$vFontSizeFormula}" font-family="{$vFontFamily}">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="$gReportNoInformationDet"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
    </fo:table-row>
  </xsl:template>
  <xsl:template name="displayColumnTitles">
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <fo:table-row font-weight="bold" color="black" font-family="{$vFontFamily}" font-size="{$vFontSizeColumnTitles}"
                  text-align="center">
      <fo:table-cell padding="{$gCellPadding}"  border="{$varTableBorder}" border-bottom-style="none"
                     background-color="{$gHdrCellBackgroundColor}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-Actor'"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$gCellPadding}" border="{$varTableBorder}" border-bottom-style="none"
                     background-color="{$gHdrCellBackgroundColor}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-Book'"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$gCellPadding}" border="{$varTableBorder}" border-bottom-style="none"
                     background-color="{$gHdrCellBackgroundColor}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-Contract'"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$gCellPadding}" border="{$varTableBorder}" border-bottom-style="none"
                     background-color="{$gHdrCellBackgroundColor}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-Put/Call'"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$gCellPadding}" border="{$varTableBorder}" border-bottom-style="none"
                     background-color="{$gHdrCellBackgroundColor}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-Currency2'"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$gCellPadding}" border="{$varTableBorder}" border-bottom-style="none"
                     background-color="{$gHdrCellBackgroundColor}"
                     number-rows-spanned="2">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-Currency2'"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
          <xsl:text> CV </xsl:text>
          <fo:inline font-size="{$vFontSizeDet}" vertical-align="super" >
            <xsl:text>(*)</xsl:text>
          </fo:inline>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$gCellPadding}" border="{$varTableBorder}" border-bottom-style="none"
                     background-color="{$gHdrCellBackgroundColor}"
                     number-rows-spanned="2">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-SpotRate'"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$gCellPadding}" border="{$varTableBorder}"
                     background-color="{$gHdrCellBackgroundColor}"
                     number-columns-spanned="2">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-QtyHdr'"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$gCellPadding}" border="{$varTableBorder}" border-bottom-style="none"
                     background-color="{$gHdrCellBackgroundColor}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-VMG'"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$gCellPadding}" border="{$varTableBorder}" border-bottom-style="none"
                     background-color="{$gHdrCellBackgroundColor}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'TS-Premium'"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$gCellPadding}" border="{$varTableBorder}" border-bottom-style="none"
                     background-color="{$gHdrCellBackgroundColor}"
                     number-rows-spanned="2">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-RMG'"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$gCellPadding}" border="{$varTableBorder}" border-bottom-style="none"
                     background-color="{$gHdrCellBackgroundColor}"
                     number-rows-spanned="2">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-RMG'"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
          <xsl:text> CV </xsl:text>
          <fo:inline font-size="{$vFontSizeDet}" vertical-align="super">
            <xsl:text>(**)</xsl:text>
          </fo:inline>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$gCellPadding}" border="{$varTableBorder}" border-bottom-style="none"
                     background-color="{$gHdrCellBackgroundColor}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-FeesExclTax'"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$gCellPadding}" border="{$varTableBorder}" border-bottom-style="none"
                     background-color="{$gHdrCellBackgroundColor}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-Tax'"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$gCellPadding}" border="{$varTableBorder}" border-bottom-style="none"
                     background-color="{$gHdrCellBackgroundColor}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'TS-Total'"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
    </fo:table-row>
    <fo:table-row font-weight="bold" color="black" font-family="{$vFontFamily}" font-size="{$vFontSizeColumnTitles}" text-align="center">
      <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" border-top-style="none"
                     background-color="{$gHdrCellBackgroundColor}">
        <fo:block border="{$vBorder}"/>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" border-top-style="none"
                     background-color="{$gHdrCellBackgroundColor}">
        <fo:block border="{$vBorder}"/>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" border-top-style="none"
                     background-color="{$gHdrCellBackgroundColor}">
        <fo:block border="{$vBorder}"/>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" border-top-style="none"
                     background-color="{$gHdrCellBackgroundColor}">
        <fo:block border="{$vBorder}"/>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" border-top-style="none"
                     background-color="{$gHdrCellBackgroundColor}">
        <fo:block border="{$vBorder}"/>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}"
                     background-color="{$gHdrCellBackgroundColor}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-QtyBuy'"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}"
                     background-color="{$gHdrCellBackgroundColor}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-QtySell'"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" border-top-style="none"
                     background-color="{$gHdrCellBackgroundColor}">
        <fo:block border="{$vBorder}">
          <xsl:value-of select="'(1)'"/>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" border-top-style="none"
                     background-color="{$gHdrCellBackgroundColor}">
        <fo:block border="{$vBorder}">
          <xsl:value-of select="'(2)'"/>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" border-top-style="none"
                     background-color="{$gHdrCellBackgroundColor}">
        <fo:block border="{$vBorder}">
          <xsl:value-of select="'(3)'"/>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" border-top-style="none"
                     background-color="{$gHdrCellBackgroundColor}">
        <fo:block border="{$vBorder}">
          <xsl:value-of select="'(4)'"/>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" border-top-style="none"
                     background-color="{$gHdrCellBackgroundColor}">
        <fo:block border="{$vBorder}">
          <xsl:value-of select="'(1+2+3+4)'"/>
        </fo:block>
      </fo:table-cell>
    </fo:table-row>
    <!-- Space-->
    <fo:table-row height="0.05cm" keep-with-previous="always">
      <fo:table-cell border="{$varTableBorder}" border-left-style="none" border-right-style="none"
                     number-columns-spanned="{$gColumnNumber}"/>
    </fo:table-row>
  </xsl:template>
  <xsl:template name="displayColumnLegends">
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <fo:table-row>
      <fo:table-cell number-columns-spanned="{$gColumnNumber}" padding="{$gCellPadding}">
        <fo:block/>
      </fo:table-cell>
    </fo:table-row>
    <fo:table-row color="black" font-family="{$vFontFamily}" font-size="{$vFontSizeColumnTitles}" text-align="left">
      <fo:table-cell number-columns-spanned="{$gColumnNumber}" padding="{$gCellPadding}">
        <xsl:call-template name="Display_Legend">
          <xsl:with-param name="pLegend" select="'(*)'" />
          <xsl:with-param name="pResource" select="'Report-CountervalueCcy'" />
          <xsl:with-param name="pCulture" select="$pCulture" />
          <!--<xsl:with-param name="pResource" select="'Devise de contrevaleur'" />-->
        </xsl:call-template>
      </fo:table-cell>
    </fo:table-row>
    <fo:table-row color="black" font-family="{$vFontFamily}" font-size="{$vFontSizeColumnTitles}" text-align="left">
      <fo:table-cell number-columns-spanned="{$gColumnNumber}" padding="{$gCellPadding}">
        <xsl:call-template name="Display_Legend">
          <xsl:with-param name="pLegend" select="'(**)'" />
          <xsl:with-param name="pResource" select="'Report-AmountCountervalueCcy'" />
          <xsl:with-param name="pCulture" select="$pCulture" />
          <!--<xsl:with-param name="pResource" select="'Montant en devise de contrevaleur'" />-->
        </xsl:call-template>
      </fo:table-cell>
    </fo:table-row>
  </xsl:template>

  <!-- ================================================================ -->
  <!-- FinFlows Report Templates                                        -->
  <!-- ================================================================ -->
  <xsl:template name="DisplayDocumentContentByActor">
    <xsl:param name="pParentActor"/>
    <xsl:param name="pRows" select="$gReportRows"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <xsl:choose>
      <xsl:when test="$gIsReferential='true'">
        <xsl:variable name="vParentActorColumnsCopy">
          <xsl:copy-of select="$pRows/Column"/>
        </xsl:variable>
        <xsl:call-template name="DisplayActorList">
          <xsl:with-param name="pParentActor" select="$pParentActor"/>
          <xsl:with-param name="pActorList" select="msxsl:node-set($vParentActorColumnsCopy)/Column[(generate-id()=generate-id(key('kActorKey',text())))]"/>
          <xsl:with-param name="pRows" select="$pRows"/>
          <xsl:with-param name="pCulture" select="$pCulture" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="vParentActorColumnsCopy">
          <xsl:copy-of select="$pRows/table/column"/>
        </xsl:variable>
        <xsl:call-template name="DisplayActorList">
          <xsl:with-param name="pParentActor" select="$pParentActor"/>
          <xsl:with-param name="pActorList" select="msxsl:node-set($vParentActorColumnsCopy)/column[(generate-id()=generate-id(key('kIOActorKey',text())))]"/>
          <xsl:with-param name="pRows" select="$pRows"/>
          <xsl:with-param name="pCulture" select="$pCulture" />
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="DisplayActorList">
    <xsl:param name="pParentActor"/>
    <xsl:param name="pActorList"/>
    <xsl:param name="pRows"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <xsl:choose>
      <xsl:when test="$pActorList">
        <xsl:for-each select="$pActorList">
          <xsl:sort select="text()" data-type="text"/>

          <xsl:variable name="vCurrentActor" select="normalize-space(text())"/>
          <xsl:choose>
            <xsl:when test="$gIsByActor">
              <xsl:choose>
                <xsl:when test="$gIsByParentActor">
                  <xsl:choose>
                    <xsl:when test="$gIsReferential='true'">
                      <xsl:call-template name="displayActor">
                        <xsl:with-param name="pActor" select="$vCurrentActor"/>
                        <xsl:with-param name="pRows" select="$pRows[Column[@ColumnName='A_ALLOC_P_IDENTIFIER' and text()=$vCurrentActor]]"/>
                        <xsl:with-param name="pPosition" select="position()"/>
                        <xsl:with-param name="pCulture" select="$pCulture" />
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:call-template name="displayActor">
                        <xsl:with-param name="pActor" select="$vCurrentActor"/>
                        <xsl:with-param name="pRows" select="$pRows[table/column[@name='A_ALLOC_P_IDENTIFIER' and text()=$vCurrentActor]]"/>
                        <xsl:with-param name="pPosition" select="position()"/>
                        <xsl:with-param name="pCulture" select="$pCulture" />
                      </xsl:call-template>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:choose>
                    <xsl:when test="$gIsReferential='true'">
                      <xsl:call-template name="DisplayDocumentContentCommon">
                        <xsl:with-param name="pData" select="$vCurrentActor"/>
                        <xsl:with-param name="pRows" select="$pRows[Column[@ColumnName='A_ALLOC_P_IDENTIFIER' and text()=$vCurrentActor]]"/>
                        <xsl:with-param name="pCulture" select="$pCulture" />
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:call-template name="DisplayDocumentContentCommon">
                        <xsl:with-param name="pData" select="$vCurrentActor"/>
                        <xsl:with-param name="pRows" select="$pRows[table/column[@name='A_ALLOC_P_IDENTIFIER' and text()=$vCurrentActor]]"/>
                        <xsl:with-param name="pCulture" select="$pCulture" />
                      </xsl:call-template>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:otherwise>
              <xsl:choose>
                <xsl:when test="$gIsReferential='true'">
                  <xsl:call-template name="displayActor">
                    <xsl:with-param name="pActor" select="$vCurrentActor"/>
                    <xsl:with-param name="pRows" select="$pRows[Column[@ColumnName='A_ALLOC_P_IDENTIFIER' and text()=$vCurrentActor]]"/>
                    <xsl:with-param name="pPosition" select="position()"/>
                    <xsl:with-param name="pCulture" select="$pCulture" />
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="displayActor">
                    <xsl:with-param name="pActor" select="$vCurrentActor"/>
                    <xsl:with-param name="pRows" select="$pRows[table/column[@name='A_ALLOC_P_IDENTIFIER' and text()=$vCurrentActor]]"/>
                    <xsl:with-param name="pPosition" select="position()"/>
                    <xsl:with-param name="pCulture" select="$pCulture" />
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:for-each>
        <xsl:if test="string-length($pParentActor) >0">
          <!-- Space-->
          <fo:table-row height="{$gTotalSpace}" keep-with-previous="always">
            <fo:table-cell number-columns-spanned="{$gColumnNumber}"/>
          </fo:table-row>
          <xsl:choose>
            <xsl:when test="$gIsReferential='true'">
              <xsl:variable name="vParentActorColumnsCopy">
                <xsl:copy-of select="$pRows/Column"/>
              </xsl:variable>
              <xsl:variable name="vCurrencyList" select="msxsl:node-set($vParentActorColumnsCopy)/Column[(generate-id()=generate-id(key('kCurrencyKey',text())))]"/>
              <xsl:call-template name="DisplayParentActorCurrency">
                <xsl:with-param name="pParentActor" select="$pParentActor"/>
                <xsl:with-param name="pCurrencyList" select="$vCurrencyList"/>
                <xsl:with-param name="pRows" select="$pRows"/>
                <xsl:with-param name="pCulture" select="$pCulture" />
              </xsl:call-template>

              <xsl:variable name="vParentActorColumnsCVCopy">
                <xsl:copy-of select="$pRows/Column"/>
              </xsl:variable>
              <xsl:variable name="vCVCurrencyList" select="msxsl:node-set($vParentActorColumnsCVCopy)/Column[(generate-id()=generate-id(key('kIDCREPORTKey',text())))]"/>

              <xsl:if test="count($vCurrencyList) > 1 or count($vCVCurrencyList) > 1 or $vCurrencyList[1] != $vCVCurrencyList[1]">
                <xsl:call-template name="DisplayParentActorCVCurrency">
                  <xsl:with-param name="pParentActor" select="$pParentActor"/>
                  <xsl:with-param name="pCVCurrencyList" select="msxsl:node-set($vParentActorColumnsCVCopy)/Column[(generate-id()=generate-id(key('kIDCREPORTKey',text())))]"/>
                  <xsl:with-param name="pRows" select="$pRows"/>
                  <xsl:with-param name="pCulture" select="$pCulture" />
                </xsl:call-template>
              </xsl:if>

            </xsl:when>
            <xsl:otherwise>
              <xsl:variable name="vParentActorColumnsCopy">
                <xsl:copy-of select="$pRows/table/column"/>
              </xsl:variable>
              <xsl:variable name="vCurrencyList" select="msxsl:node-set($vParentActorColumnsCopy)/column[(generate-id()=generate-id(key('kIOCurrencyKey',text())))]"/>
              <xsl:call-template name="DisplayParentActorCurrency">
                <xsl:with-param name="pParentActor" select="$pParentActor"/>
                <xsl:with-param name="pCurrencyList" select="$vCurrencyList"/>
                <xsl:with-param name="pRows" select="$pRows"/>
                <xsl:with-param name="pCulture" select="$pCulture" />
              </xsl:call-template>

              <xsl:variable name="vParentActorColumnsCVCopy">
                <xsl:copy-of select="$pRows/table/column"/>
              </xsl:variable>
              <xsl:variable name="vCVCurrencyList" select="msxsl:node-set($vParentActorColumnsCopy)/column[(generate-id()=generate-id(key('kIOIDCREPORTKey',text())))]"/>

              <xsl:if test="count($vCurrencyList) > 1 or count($vCVCurrencyList) > 1 or $vCurrencyList[1] != $vCVCurrencyList[1]">
                <xsl:call-template name="DisplayParentActorCVCurrency">
                  <xsl:with-param name="pParentActor" select="$pParentActor"/>
                  <xsl:with-param name="pCurrencyList" select="$vCVCurrencyList"/>
                  <xsl:with-param name="pRows" select="$pRows"/>
                  <xsl:with-param name="pCulture" select="$pCulture" />
                </xsl:call-template>
              </xsl:if>
            </xsl:otherwise>
          </xsl:choose>
          <!-- Black line-->
          <fo:table-row keep-with-previous="always">
            <fo:table-cell number-columns-spanned="{$gColumnNumber}" border="0.5pt solid black" border-bottom-style="solid"/>
          </fo:table-row>
          <!-- Space-->
          <fo:table-row height="0.05cm" keep-with-previous="always">
            <fo:table-cell number-columns-spanned="{$gColumnNumber}"/>
          </fo:table-row>
        </xsl:if>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <xsl:when test="$gIsByActor">
            <xsl:call-template name="DisplayDocumentContentCommon">
              <xsl:with-param name="pCulture" select="$pCulture" />
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="displayNoData">
              <xsl:with-param name="pCulture" select="$pCulture" />
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="DisplayParentActorCurrency">
    <xsl:param name="pParentActor"/>
    <xsl:param name="pCurrencyList"/>
    <xsl:param name="pRows"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <xsl:for-each select="$pCurrencyList">
      <xsl:sort select="text()" data-type="text"/>

      <xsl:variable name="vCurrentCurrency" select="normalize-space(text())"/>
      <xsl:variable name="vCurrencyRows">
        <xsl:choose>
          <xsl:when test="$gIsReferential='true'">
            <xsl:call-template name="GetDataRow">
              <xsl:with-param name="pActor" select="$pParentActor"/>
              <xsl:with-param name="pCurrency" select="$vCurrentCurrency"/>
              <xsl:with-param name="pRows" select="$pRows[Column[@ColumnName='FLOWS_ASS_UNIT' and text()=$vCurrentCurrency]]"/>
              <xsl:with-param name="pCulture" select="$pCulture" />
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="GetDataRow">
              <xsl:with-param name="pActor" select="$pParentActor"/>
              <xsl:with-param name="pCurrency" select="$vCurrentCurrency"/>
              <xsl:with-param name="pRows" select="$pRows[table/column[@name='FLOWS_ASS_UNIT' and text()=$vCurrentCurrency]]"/>
              <xsl:with-param name="pCulture" select="$pCulture" />
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="vTotalBackgroundColor">
        <xsl:choose>
          <xsl:when test="$gIsByActor = false()">
            <xsl:value-of select="$gTotalBackgroundColor"/>
          </xsl:when>
          <xsl:otherwise/>
        </xsl:choose>
      </xsl:variable>
      <xsl:call-template name="displayColumnDatas">
        <xsl:with-param name="pActor" select="$pParentActor"/>
        <xsl:with-param name="pCurrency" select="$vCurrentCurrency"/>
        <xsl:with-param name="pRows" select="msxsl:node-set($vCurrencyRows)/Row"/>
        <xsl:with-param name="pWithDetails" select="false()"/>
        <xsl:with-param name="pTotalBackgroundColor" select="$vTotalBackgroundColor"/>
        <xsl:with-param name="pCulture" select="$pCulture" />
      </xsl:call-template>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="DisplayParentActorCVCurrency">
    <xsl:param name="pParentActor"/>
    <xsl:param name="pCVCurrencyList"/>
    <xsl:param name="pRows"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>
  </xsl:template>

  <xsl:template name="DisplayDocumentContentByParentActor">
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <xsl:variable name="vColumnsCopy">
      <xsl:copy-of select="$gReportRows/Column"/>
    </xsl:variable>
    <xsl:variable name="vParentActors">
      <xsl:copy-of select="msxsl:node-set($vColumnsCopy)/Column[(generate-id()=generate-id(key('kParentActorKey',text())))]"/>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="msxsl:node-set($vParentActors)/Column">
        <xsl:for-each select="msxsl:node-set($vParentActors)/Column">
          <xsl:sort select="text()" data-type="text"/>

          <xsl:variable name="vCurrentParentActor" select="normalize-space(text())"/>
          <xsl:variable name="vCurrentParentActorRows" select="$gReportRows[Column[@ColumnName=$gColumnParentActor and text()=$vCurrentParentActor]]"/>
          <xsl:choose>
            <xsl:when test="$gIsByActor">
              <xsl:call-template name="DisplayDocumentContentCommon">
                <xsl:with-param name="pData" select="$vCurrentParentActor"/>
                <xsl:with-param name="pRows" select="$vCurrentParentActorRows"/>
                <xsl:with-param name="pCulture" select="$pCulture" />
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="DisplayDocumentContentByActor">
                <xsl:with-param name="pParentActor" select="$vCurrentParentActor"/>
                <xsl:with-param name="pRows" select="$vCurrentParentActorRows"/>
                <xsl:with-param name="pCulture" select="$pCulture" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="DisplayDocumentContentByActor">
          <xsl:with-param name="pCulture" select="$pCulture" />
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!--
  Display actor data
  - Data rows
  - Subtotal by currency
  -->
  <xsl:template name="displayActor">
    <xsl:param name="pActor"/>
    <xsl:param name="pRows"/>
    <xsl:param name="pPosition"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <xsl:choose>
      <xsl:when test="$pRows">
        <xsl:if test="$pPosition > 1">
          <!-- Space-->
          <fo:table-row height="0.05cm" keep-with-previous="always">
            <fo:table-cell number-columns-spanned="{$gColumnNumber}"/>
          </fo:table-row>
        </xsl:if>

        <xsl:choose>
          <xsl:when test="$gIsReferential='true'">
            <xsl:variable name="vColumnsCopyNode">
              <xsl:copy-of select="$pRows/Column"/>
            </xsl:variable>
            <xsl:call-template name="displayActorCurrency">
              <xsl:with-param name="pActor" select="$pActor"/>
              <xsl:with-param name="pCurrencyList" select="msxsl:node-set($vColumnsCopyNode)/Column[(generate-id()=generate-id(key('kCurrencyKey',text())))]"/>
              <xsl:with-param name="pRows" select="$pRows"/>
              <xsl:with-param name="pCulture" select="$pCulture" />
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:variable name="vColumnsCopyNode">
              <xsl:copy-of select="$pRows/table/column"/>
            </xsl:variable>
            <xsl:call-template name="displayActorCurrency">
              <xsl:with-param name="pActor" select="$pActor"/>
              <xsl:with-param name="pCurrencyList" select="msxsl:node-set($vColumnsCopyNode)/column[(generate-id()=generate-id(key('kIOCurrencyKey',text())))]"/>
              <xsl:with-param name="pRows" select="$pRows"/>
              <xsl:with-param name="pCulture" select="$pCulture" />
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:if test="$gIsByActor = false() or $gIsByParentActor">
          <!-- Black line-->
          <fo:table-row keep-with-previous="always">
            <fo:table-cell number-columns-spanned="{$gColumnNumber}" border="0.5pt solid black" border-bottom-style="solid"/>
          </fo:table-row>
        </xsl:if>
      </xsl:when>
      <xsl:otherwise>
        <!-- No Data-->
        <fo:table-row>
          <fo:table-cell padding="{$gCellPadding}" text-align="left" number-columns-spanned="{$gColumnNumber}">
            <fo:block linefeed-treatment="preserve" text-align="left" font-size="{$vFontSizeFormula}" font-weight="bold" font-family="{$vFontFamily}">
              <xsl:value-of select="$pActor"/>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
        <xsl:call-template name="displayNoData">
          <xsl:with-param name="pCulture" select="$pCulture" />
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!--
  Display actor/currency data
  - Data rows
  - Subtotal by currency
  -->
  <xsl:template name="displayActorCurrency">
    <xsl:param name="pActor"/>
    <xsl:param name="pCurrencyList"/>
    <xsl:param name="pRows"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <xsl:for-each select="$pCurrencyList">
      <xsl:sort select="text()" data-type="text"/>

      <xsl:variable name="vCurrentCurrency" select="normalize-space(text())"/>
      <xsl:variable name="vCurrencyRowsNode">
        <xsl:choose>
          <xsl:when test="$gIsReferential='true'">
            <xsl:variable name="vCurrencyRows" select="$pRows[Column[@ColumnName='FLOWS_ASS_UNIT' and text()=$vCurrentCurrency]]"/>
            <xsl:variable name="vCurrencyColumnsCopyNode">
              <xsl:copy-of select="$vCurrencyRows/Column"/>
            </xsl:variable>
            <xsl:call-template name="GetActorCurrencyBookRow">
              <xsl:with-param name="pActor" select="$pActor"/>
              <xsl:with-param name="pCurrency" select="$vCurrentCurrency"/>
              <xsl:with-param name="pBookList" select="msxsl:node-set($vCurrencyColumnsCopyNode)/Column[(generate-id()=generate-id(key('kBookKey',text())))]"/>
              <xsl:with-param name="pRows" select="$vCurrencyRows"/>
              <xsl:with-param name="pCulture" select="$pCulture" />
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:variable name="vCurrencyRows" select="$pRows[table/column[@name='FLOWS_ASS_UNIT' and text()=$vCurrentCurrency]]"/>
            <xsl:variable name="vCurrencyColumnsCopyNode">
              <xsl:copy-of select="$vCurrencyRows/table/column"/>
            </xsl:variable>
            <xsl:call-template name="GetActorCurrencyBookRow">
              <xsl:with-param name="pActor" select="$pActor"/>
              <xsl:with-param name="pCurrency" select="$vCurrentCurrency"/>
              <xsl:with-param name="pBookList" select="msxsl:node-set($vCurrencyColumnsCopyNode)/column[(generate-id()=generate-id(key('kIOBookKey',text())))]"/>
              <xsl:with-param name="pRows" select="$vCurrencyRows"/>
              <xsl:with-param name="pCulture" select="$pCulture" />
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:call-template name="displayColumnDatas">
        <xsl:with-param name="pActor" select="$pActor"/>
        <xsl:with-param name="pCurrency" select="$vCurrentCurrency"/>
        <xsl:with-param name="pRows" select="msxsl:node-set($vCurrencyRowsNode)/Row"/>
        <xsl:with-param name="pCulture" select="$pCulture" />
      </xsl:call-template>
    </xsl:for-each>

    <xsl:variable name="vColumnsCopyNode">
      <xsl:copy-of select="$pRows/Column"/>
    </xsl:variable>
    <xsl:variable name="vCVCurrencyList" select="msxsl:node-set($vColumnsCopyNode)/Column[(generate-id()=generate-id(key('kIDCREPORTKey',text())))]"/>

    <xsl:if test="count($pCurrencyList) > 1 or count($vCVCurrencyList) > 1 or $pCurrencyList[1] != $vCVCurrencyList[1]">
      <xsl:call-template name="displayActorCVCurrency">
        <xsl:with-param name="pActor" select="$pActor"/>
        <xsl:with-param name="pCVCurrencyList" select="$vCVCurrencyList"/>
        <xsl:with-param name="pRows" select="$pRows"/>
        <xsl:with-param name="pCulture" select="$pCulture" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <!--
  Display actor/currency data
  - Subtotal by CV currency
  -->
  <xsl:template name="displayActorCVCurrency">
    <xsl:param name="pActor"/>
    <xsl:param name="pCVCurrencyList"/>
    <xsl:param name="pRows"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>
  </xsl:template>

  <!--
  Display actor/currency data
  - Data rows
  - Subtotal by currency
  -->
  <xsl:template name="GetActorCurrencyBookRow">
    <xsl:param name="pActor"/>
    <xsl:param name="pCurrency"/>
    <xsl:param name="pBookList"/>
    <xsl:param name="pRows"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <xsl:for-each select="$pBookList">
      <xsl:sort select="text()" data-type="text"/>

      <xsl:variable name="vCurrentBook" select="text()"/>
      <xsl:choose>
        <xsl:when test="$gIsReferential='true'">
          <xsl:variable name="vBookRows" select="$pRows[Column[@ColumnName='B_ALLOC_P_IDENTIFIER' and text()=$vCurrentBook]]"/>
          <xsl:variable name="vBookColumnsCopyNode">
            <xsl:copy-of select="$vBookRows/Column"/>
          </xsl:variable>
          <xsl:call-template name="GetActorCurrencyBookDCRow">
            <xsl:with-param name="pActor" select="$pActor"/>
            <xsl:with-param name="pCurrency" select="$pCurrency"/>
            <xsl:with-param name="pBook" select="$vCurrentBook"/>
            <xsl:with-param name="pDCList" select="msxsl:node-set($vBookColumnsCopyNode)/Column[(generate-id()=generate-id(key('kDCKey',text())))]"/>
            <xsl:with-param name="pRows" select="$vBookRows"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="vBookRows" select="$pRows[table/column[@name='B_ALLOC_P_IDENTIFIER' and text()=$vCurrentBook]]"/>
          <xsl:variable name="vBookColumnsCopyNode">
            <xsl:copy-of select="$vBookRows/table/column"/>
          </xsl:variable>
          <xsl:call-template name="GetActorCurrencyBookDCRow">
            <xsl:with-param name="pActor" select="$pActor"/>
            <xsl:with-param name="pCurrency" select="$pCurrency"/>
            <xsl:with-param name="pBook" select="$vCurrentBook"/>
            <xsl:with-param name="pDCList" select="msxsl:node-set($vBookColumnsCopyNode)/column[(generate-id()=generate-id(key('kIODCKey',text())))]"/>
            <xsl:with-param name="pRows" select="$vBookRows"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>

  <!--
  Display actor/currency data
  - Data rows
  - Subtotal by currency
  -->
  <xsl:template name="GetActorCurrencyBookDCRow">
    <xsl:param name="pActor"/>
    <xsl:param name="pCurrency"/>
    <xsl:param name="pBook"/>
    <xsl:param name="pDCList"/>
    <xsl:param name="pRows"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <xsl:for-each select="$pDCList">
      <xsl:sort select="text()" data-type="text"/>

      <xsl:variable name="vCurrentDC" select="normalize-space(text())"/>

      <xsl:choose>
        <xsl:when test="$gIsReferential='true'">
          <xsl:variable name="vDCRows" select="$pRows[Column[@ColumnName='DC_IDENTIFIER' and text()=$vCurrentDC]]"/>
          <xsl:call-template name="GetActorCurrencyBookDCAssetRow">
            <xsl:with-param name="pActor" select="$pActor"/>
            <xsl:with-param name="pCurrency" select="$pCurrency"/>
            <xsl:with-param name="pBook" select="$pBook"/>
            <xsl:with-param name="pDC" select="$vCurrentDC"/>
            <xsl:with-param name="pIsPutCall" select="string-length($vDCRows/Column[@ColumnName='A_ETD_PUTCALL']/text()) > 0"/>
            <xsl:with-param name="pRows" select="$vDCRows"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="vDCRows" select="$pRows[table/column[@name='DC_IDENTIFIER' and text()=$vCurrentDC]]"/>
          <xsl:call-template name="GetActorCurrencyBookDCAssetRow">
            <xsl:with-param name="pActor" select="$pActor"/>
            <xsl:with-param name="pCurrency" select="$pCurrency"/>
            <xsl:with-param name="pBook" select="$pBook"/>
            <xsl:with-param name="pDC" select="$vCurrentDC"/>
            <xsl:with-param name="pIsPutCall" select="string-length($vDCRows/table/column[@name='A_ETD_PUTCALL']/text()) > 0"/>
            <xsl:with-param name="pRows" select="$vDCRows"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>

  <!--
  Display actor/currency data
  - Data rows
  - Subtotal by currency
  -->
  <xsl:template name="GetActorCurrencyBookDCAssetRow">
    <xsl:param name="pActor"/>
    <xsl:param name="pCurrency"/>
    <xsl:param name="pBook"/>
    <xsl:param name="pDC"/>
    <xsl:param name="pIsPutCall"/>
    <xsl:param name="pRows"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <xsl:choose>
      <xsl:when test="$pIsPutCall='true'">
        <xsl:choose>
          <xsl:when test="$gIsReferential='true'">
            <xsl:variable name="vPutCallColumnsCopyNode">
              <xsl:copy-of select="$pRows/Column"/>
            </xsl:variable>
            <xsl:call-template name="GetActorCurrencyBookDCPutCallRow">
              <xsl:with-param name="pActor" select="$pActor"/>
              <xsl:with-param name="pCurrency" select="$pCurrency"/>
              <xsl:with-param name="pBook" select="$pBook"/>
              <xsl:with-param name="pDC" select="$pDC"/>
              <xsl:with-param name="pPutCallList" select="msxsl:node-set($vPutCallColumnsCopyNode)/Column[(generate-id()=generate-id(key('kPCKey',text())))]"/>
              <xsl:with-param name="pRows" select="$pRows"/>
              <xsl:with-param name="pCulture" select="$pCulture" />
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:variable name="vPutCallColumnsCopyNode">
              <xsl:copy-of select="$pRows/table/column"/>
            </xsl:variable>
            <xsl:call-template name="GetActorCurrencyBookDCPutCallRow">
              <xsl:with-param name="pActor" select="$pActor"/>
              <xsl:with-param name="pCurrency" select="$pCurrency"/>
              <xsl:with-param name="pBook" select="$pBook"/>
              <xsl:with-param name="pDC" select="$pDC"/>
              <xsl:with-param name="pPutCallList" select="msxsl:node-set($vPutCallColumnsCopyNode)/column[(generate-id()=generate-id(key('kIOPCKey',text())))]"/>
              <xsl:with-param name="pRows" select="$pRows"/>
              <xsl:with-param name="pCulture" select="$pCulture" />
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="GetDataRow">
          <xsl:with-param name="pActor" select="$pActor"/>
          <xsl:with-param name="pCurrency" select="$pCurrency"/>
          <xsl:with-param name="pBook" select="$pBook"/>
          <xsl:with-param name="pDC" select="$pDC"/>
          <xsl:with-param name="pRows" select="$pRows"/>
          <xsl:with-param name="pCulture" select="$pCulture" />
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!--
  Display actor/currency data
  - Data rows
  - Subtotal by currency
  -->
  <xsl:template name="GetActorCurrencyBookDCPutCallRow">
    <xsl:param name="pActor"/>
    <xsl:param name="pCurrency"/>
    <xsl:param name="pBook"/>
    <xsl:param name="pDC"/>
    <xsl:param name="pPutCallList"/>
    <xsl:param name="pRows"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <xsl:for-each select="$pPutCallList">
      <xsl:sort select="text()" data-type="text"/>

      <xsl:variable name="vCurrentPutCall" select="normalize-space(text())"/>

      <xsl:choose>
        <xsl:when test="$gIsReferential='true'">
          <xsl:call-template name="GetDataRow">
            <xsl:with-param name="pActor" select="$pActor"/>
            <xsl:with-param name="pCurrency" select="$pCurrency"/>
            <xsl:with-param name="pBook" select="$pBook"/>
            <xsl:with-param name="pDC" select="$pDC"/>
            <xsl:with-param name="pPutCall" select="$vCurrentPutCall"/>
            <xsl:with-param name="pRows" select="$pRows[Column[@ColumnName='A_ETD__PUTCALL' and text()=$vCurrentPutCall]]"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="GetDataRow">
            <xsl:with-param name="pActor" select="$pActor"/>
            <xsl:with-param name="pCurrency" select="$pCurrency"/>
            <xsl:with-param name="pBook" select="$pBook"/>
            <xsl:with-param name="pDC" select="$pDC"/>
            <xsl:with-param name="pPutCall" select="$vCurrentPutCall"/>
            <xsl:with-param name="pRows" select="$pRows[table/column[@name='A_ETD__PUTCALL' and text()=$vCurrentPutCall]]"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="GetSPOTRATEList">

    <!--<xsl:variable name="vRowsSPOTRATE" select="$gReportRows[Column[@ColumnName='FLOWS_ASS_CV_SPOTRATE' and string-length(text()) > 0]]"/>-->
    <xsl:variable name="vRowsSPOTRATENode">
      <xsl:choose>
        <xsl:when test="$gIsReferential='true'">
          <xsl:copy-of select="$gReportRows[Column[@ColumnName='FLOWS_ASS_CV_SPOTRATE' and string-length(text()) > 0]]"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:copy-of select="$gReportRows[table/column[@name='FLOWS_ASS_CV_SPOTRATE' and string-length(text()) > 0]]"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!--<xsl:variable name="vRowsSPOTRATE" select="$gReportRows[Column[@ColumnName='FLOWS_ASS_CV_SPOTRATE_LAST' and string-length(text()) > 0]]"/>-->
    <xsl:variable name="vRowsSPOTRATECopyNode">
      <xsl:copy-of select="$vRowsSPOTRATENode"/>
    </xsl:variable>
    <!--<xsl:variable name="vCurrencyList" select="msxsl:node-set($vRowsSPOTRATECopyNode)/Row/Column[(generate-id()=generate-id(key('kCurrencyKey',text())))]"/>-->
    <xsl:variable name="vCurrencyListNode">
      <xsl:choose>
        <xsl:when test="$gIsReferential='true'">
          <xsl:value-of select="msxsl:node-set($vRowsSPOTRATECopyNode)/Row/Column[(generate-id()=generate-id(key('kCurrencyKey',text())))]"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="msxsl:node-set($vRowsSPOTRATECopyNode)/row/table/column[(generate-id()=generate-id(key('kIOCurrencyKey',text())))]"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:for-each select="msxsl:node-set($vCurrencyListNode)">
      <xsl:sort select="text()" data-type="text"/>

      <xsl:variable name="vCurrentCurrency" select="text()"/>
      <!--<xsl:variable name="vRowsSPOTRATE_Currency" select="$vRowsSPOTRATE[Column[@ColumnName='FLOWS_ASS_UNIT' and text()=$vCurrentCurrency]]"/>-->
      <xsl:variable name="vRowsSPOTRATE_CurrencyNode">
        <xsl:choose>
          <xsl:when test="$gIsReferential='true'">
            <xsl:copy-of select="msxsl:node-set($vRowsSPOTRATENode)/Row[Column[@ColumnName='FLOWS_ASS_UNIT' and text()=$vCurrentCurrency]]"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:copy-of select="msxsl:node-set($vRowsSPOTRATENode)/row[table/column[@name='FLOWS_ASS_UNIT' and text()=$vCurrentCurrency]]"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name="vRowsSPOTRATE_CurrencyCopyNode">
        <xsl:copy-of select="$vRowsSPOTRATE_CurrencyNode"/>
      </xsl:variable>
      <!--<xsl:variable name="vIDCREPORTList" select="msxsl:node-set($vRowsSPOTRATE_CurrencyCopyNode)/Row/Column[(generate-id()=generate-id(key('kIDCREPORTKey',text())))]"/>-->
      <xsl:variable name="vIDCREPORTListNode">
        <xsl:choose>
          <xsl:when test="$gIsReferential='true'">
            <xsl:value-of select="msxsl:node-set($vRowsSPOTRATE_CurrencyCopyNode)/Row/Column[(generate-id()=generate-id(key('kIDCREPORTKey',text())))]"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="msxsl:node-set($vRowsSPOTRATE_CurrencyCopyNode)/row/table/column[(generate-id()=generate-id(key('kIOIDCREPORTKey',text())))]"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:for-each select="msxsl:node-set($vIDCREPORTListNode)">
        <xsl:sort select="text()" data-type="text"/>

        <xsl:variable name="vCurrentIDCREPORT" select="text()"/>
        <!--<xsl:variable name="vRowsSPOTRATE_IDCREPORT" select="$vRowsSPOTRATE_Currency[Column[@ColumnName='FLOWS_ASS_CV_IDCREPORT' and text()=$vCurrentIDCREPORT]]"/>-->
        <xsl:variable name="vRowsSPOTRATE_IDCREPORTNode">
          <xsl:choose>
            <xsl:when test="$gIsReferential='true'">
              <xsl:copy-of select="msxsl:node-set($vRowsSPOTRATE_CurrencyNode)/Row[Column[@ColumnName='FLOWS_ASS_CV_IDCREPORT' and text()=$vCurrentIDCREPORT]]"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:copy-of select="msxsl:node-set($vRowsSPOTRATE_CurrencyNode)/row[table/column[@name='FLOWS_ASS_CV_IDCREPORT' and text()=$vCurrentIDCREPORT]]"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="vRowsSPOTRATE_IDCREPORTSortNode">
          <xsl:for-each select="msxsl:node-set($vRowsSPOTRATE_IDCREPORTNode)">
            <xsl:sort select="Column[@ColumnName='FLOWS_ASS_DTBUSINESS']"/>
            <xsl:sort select="table/column[@name='FLOWS_ASS_DTBUSINESS']"/>

            <xsl:copy-of select="current()"/>

          </xsl:for-each>
        </xsl:variable>

        <!--<xsl:variable name="vRowsSPOTRATE_IDCREPORTLast" select="msxsl:node-set($vRowsSPOTRATE_IDCREPORTSortNode)/Row[last()]"/>-->
        <!--<xsl:variable name="vRowsSPOTRATE_IDCREPORTLast" select="$vRowsSPOTRATE_IDCREPORT[last()]"/>-->

        <Row>
          <Column ColumnName="FLOWS_ASS_UNIT">
            <xsl:value-of select="$vCurrentCurrency"/>
          </Column>
          <Column ColumnName="FLOWS_ASS_CV_IDCREPORT">
            <xsl:value-of select="$vCurrentIDCREPORT"/>
          </Column>
          <Column ColumnName="FLOWS_ASS_DTBUSINESS">
            <!--<xsl:value-of select="$vRowsSPOTRATE_IDCREPORTLast/Column[@ColumnName='FLOWS_ASS_DTBUSINESS']/text()"/>-->
            <xsl:choose>
              <xsl:when test="$gIsReferential='true'">
                <xsl:value-of select="msxsl:node-set($vRowsSPOTRATE_IDCREPORTSortNode)/Row[last()]/Column[@ColumnName='FLOWS_ASS_DTBUSINESS']/text()"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="msxsl:node-set($vRowsSPOTRATE_IDCREPORTSortNode)/row[last()]/table/column[@name='FLOWS_ASS_DTBUSINESS']/text()"/>
              </xsl:otherwise>
            </xsl:choose>
          </Column>
          <Column ColumnName="FLOWS_ASS_CV_SPOTRATE">
            <!--<xsl:value-of select="$vRowsSPOTRATE_IDCREPORTLast/Column[@ColumnName='FLOWS_ASS_CV_SPOTRATE']/text()"/>-->
            <!--<xsl:value-of select="$vRowsSPOTRATE_IDCREPORTLast/Column[@ColumnName='FLOWS_ASS_CV_SPOTRATE_LAST']/text()"/>-->
            <xsl:choose>
              <xsl:when test="$gIsReferential='true'">
                <xsl:value-of select="msxsl:node-set($vRowsSPOTRATE_IDCREPORTSortNode)/Row[last()]/Column[@ColumnName='FLOWS_ASS_CV_SPOTRATE']/text()"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="msxsl:node-set($vRowsSPOTRATE_IDCREPORTSortNode)/row[last()]/table/column[@name='FLOWS_ASS_CV_SPOTRATE_LAST']/text()"/>
              </xsl:otherwise>
            </xsl:choose>
          </Column>
        </Row>

      </xsl:for-each>
    </xsl:for-each>
  </xsl:template>

  <!--
  Display actor/currency data
  - Data rows
  - Subtotal by currency
  -->
  <xsl:template name="GetDataRow">
    <xsl:param name="pActor"/>
    <xsl:param name="pCurrency"/>
    <xsl:param name="pBook"/>
    <xsl:param name="pDC"/>
    <xsl:param name="pPutCall" select="''"/>
    <xsl:param name="pRows"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <Row>
      <Column ColumnName="A_ALLOC_P_IDENTIFIER">
        <xsl:value-of select="$pActor"/>
      </Column>
      <Column ColumnName="FLOWS_ASS_UNIT">
        <xsl:value-of select="$pCurrency"/>
      </Column>
      <xsl:variable name="vIDCREPORT">
        <xsl:choose>
          <xsl:when test="$gIsReferential='true'">
            <xsl:value-of select="$pRows/Column[@ColumnName='FLOWS_ASS_CV_IDCREPORT'][1]/text()"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$pRows/table/column[@name='FLOWS_ASS_CV_IDCREPORT'][1]/text()"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <Column ColumnName="FLOWS_ASS_CV_IDCREPORT">
        <xsl:value-of select="$vIDCREPORT"/>
      </Column>
      <Column ColumnName="FLOWS_ASS_CV_SPOTRATE">
        <xsl:value-of select="$gReportSPOTRATEList[Column[@ColumnName='FLOWS_ASS_UNIT' and text()=$pCurrency] and Column[@ColumnName='FLOWS_ASS_CV_IDCREPORT' and text()=$vIDCREPORT]]/Column[@ColumnName='FLOWS_ASS_CV_SPOTRATE']/text()"/>
      </Column>
      <Column ColumnName="B_ALLOC_P_IDENTIFIER">
        <xsl:value-of select="$pBook"/>
      </Column>
      <Column ColumnName="DC_IDENTIFIER">
        <xsl:value-of select="$pDC"/>
      </Column>
      <Column ColumnName="PUTCALL">
        <xsl:value-of select="$pPutCall"/>
      </Column>
      <xsl:variable name="vFLOWS_ASS_QTY_BUY">
        <xsl:choose>
          <xsl:when test="$gIsReferential='true'">
            <xsl:value-of select="sum($pRows/Column[@ColumnName='FLOWS_ASS_QTY_BUY']/text())"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="sum($pRows/table/column[@name='FLOWS_ASS_QTY_BUY']/text())"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="vFLOWS_ASS_QTY_SELL">
        <xsl:choose>
          <xsl:when test="$gIsReferential='true'">
            <xsl:value-of select="sum($pRows/Column[@ColumnName='FLOWS_ASS_QTY_SELL']/text())"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="sum($pRows/table/column[@name='FLOWS_ASS_QTY_SELL']/text())"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="vFLOWS_ASS_VARMARGINAMOUNT">
        <xsl:choose>
          <xsl:when test="$gIsReferential='true'">
            <xsl:value-of select="sum($pRows/Column[@ColumnName='FLOWS_ASS_VARMARGINAMOUNT']/text())"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="sum($pRows/table/column[@name='FLOWS_ASS_VARMARGINAMOUNT']/text())"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="vFLOWS_ASS_PREMIUMAMOUNT">
        <xsl:choose>
          <xsl:when test="$gIsReferential='true'">
            <xsl:value-of select="sum($pRows/Column[@ColumnName='FLOWS_ASS_PREMIUMAMOUNT']/text())"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="sum($pRows/table/column[@name='FLOWS_ASS_PREMIUMAMOUNT']/text())"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="vFLOWS_ASS_REALMARGINAMOUNT">
        <xsl:choose>
          <xsl:when test="$gIsReferential='true'">
            <xsl:value-of select="sum($pRows/Column[@ColumnName='FLOWS_ASS_REALMARGINAMOUNT']/text())"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="sum($pRows/table/column[@name='FLOWS_ASS_REALMARGINAMOUNT']/text())"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="vFLOWS_ASS_FEEEXCLTAXAMOUNT">
        <xsl:choose>
          <xsl:when test="$gIsReferential='true'">
            <xsl:value-of select="sum($pRows/Column[@ColumnName='FLOWS_ASS_FEEEXCLTAXAMOUNT']/text())"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="sum($pRows/table/column[@name='FLOWS_ASS_FEEEXCLTAXAMOUNT']/text())"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="vFLOWS_ASS_TAXAMOUNT">
        <xsl:choose>
          <xsl:when test="$gIsReferential='true'">
            <xsl:value-of select="sum($pRows/Column[@ColumnName='FLOWS_ASS_TAXAMOUNT']/text())"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="sum($pRows/table/column[@name='FLOWS_ASS_TAXAMOUNT']/text())"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <Column ColumnName="FLOWS_ASS_QTY_BUY">
        <xsl:value-of select="$vFLOWS_ASS_QTY_BUY"/>
      </Column>
      <Column ColumnName="FLOWS_ASS_QTY_SELL">
        <xsl:value-of select="$vFLOWS_ASS_QTY_SELL"/>
      </Column>
      <Column ColumnName="FLOWS_ASS_VARMARGINAMOUNT">
        <xsl:value-of select="$vFLOWS_ASS_VARMARGINAMOUNT"/>
      </Column>
      <Column ColumnName="FLOWS_ASS_PREMIUMAMOUNT">
        <xsl:value-of select="$vFLOWS_ASS_PREMIUMAMOUNT"/>
      </Column>
      <Column ColumnName="FLOWS_ASS_REALMARGINAMOUNT">
        <xsl:value-of select="$vFLOWS_ASS_REALMARGINAMOUNT"/>
      </Column>
      <Column ColumnName="FLOWS_ASS_CV_REALMARGINAMT_CV">
        <xsl:choose>
          <xsl:when test="$gIsReferential='true'">
            <xsl:if test="$pRows[Column[@ColumnName='FLOWS_ASS_CV_REALMARGINAMT_CV' and string-length(text()) > 0]]">
              <xsl:value-of select="sum($pRows/Column[@ColumnName='FLOWS_ASS_CV_REALMARGINAMT_CV' and string-length(text()) > 0]/text())"/>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:if test="$pRows[table/column[@name='FLOWS_ASS_CV_REALMARGINAMT_CV' and string-length(text()) > 0]]">
              <xsl:value-of select="sum($pRows/table/column[@name='FLOWS_ASS_CV_REALMARGINAMT_CV' and string-length(text()) > 0]/text())"/>
            </xsl:if>
          </xsl:otherwise>
        </xsl:choose>
      </Column>
      <Column ColumnName="FLOWS_ASS_FEEEXCLTAXAMOUNT">
        <xsl:value-of select="$vFLOWS_ASS_FEEEXCLTAXAMOUNT"/>
      </Column>
      <Column ColumnName="FLOWS_ASS_TAXAMOUNT">
        <xsl:value-of select="$vFLOWS_ASS_TAXAMOUNT"/>
      </Column>
      <Column ColumnName="TOTAL">
        <xsl:value-of select="$vFLOWS_ASS_VARMARGINAMOUNT + $vFLOWS_ASS_PREMIUMAMOUNT + $vFLOWS_ASS_FEEEXCLTAXAMOUNT + $vFLOWS_ASS_TAXAMOUNT"/>
      </Column>
    </Row>
  </xsl:template>

  <xsl:template name="displayColumnDatas">
    <xsl:param name="pActor"/>
    <xsl:param name="pCurrency"/>
    <xsl:param name="pRows"/>
    <xsl:param name="pWithDetails" select="true()"/>
    <xsl:param name="pWithCVTotal" select="true()"/>
    <xsl:param name="pTotalBackgroundColor"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <xsl:variable name="vNumberPatternNoZero">
      <xsl:call-template name="GetNumberPattern">
        <xsl:with-param name="pPatternName" select="'numberPatternNoZero'"/>
        <xsl:with-param name="pCulture" select="$pCulture"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vNumberDecPattern">
      <xsl:call-template name="GetNumberPattern">
        <xsl:with-param name="pPatternName" select="'number2DecPattern'"/>
        <xsl:with-param name="pCulture" select="$pCulture"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vIntegerPattern">
      <xsl:call-template name="GetNumberPattern">
        <xsl:with-param name="pPatternName" select="'integerPattern'"/>
        <xsl:with-param name="pCulture" select="$pCulture"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vDefaultCulture">
      <xsl:choose>
        <xsl:when test = "$pCulture = 'de-DE'">
          <xsl:value-of select="$pCulture"/>
        </xsl:when>
        <xsl:when test = "$pCulture = 'en-GB'">
          <xsl:value-of select="$pCulture"/>
        </xsl:when>
        <xsl:when test = "$pCulture = 'es-ES'">
          <xsl:value-of select="$pCulture"/>
        </xsl:when>
        <xsl:when test = "$pCulture = 'fr-BE'">
          <xsl:value-of select="$pCulture"/>
        </xsl:when>
        <xsl:when test = "$pCulture = 'fr-FR'">
          <xsl:value-of select="$pCulture"/>
        </xsl:when>
        <xsl:when test = "$pCulture = 'it-IT'">
          <xsl:value-of select="$pCulture"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'en-GB'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:if test="$pWithDetails = true()">
      <xsl:for-each select="$pRows">
        <fo:table-row display-align="center" color="black" font-family="{$vFontFamily}" font-size="{$vFontSizeColumnTitles}" text-align="center">
          <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" text-align="left">
            <fo:block border="{$vBorder}">
              <xsl:call-template name="DisplayData_Truncate">
                <xsl:with-param name="pData" select="Column[@ColumnName='A_ALLOC_P_IDENTIFIER']/text()"/>
                <xsl:with-param name="pDataLength" select="13"/>
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" text-align="left">
            <fo:block border="{$vBorder}">
              <xsl:call-template name="DisplayData_Truncate">
                <xsl:with-param name="pData" select="Column[@ColumnName='B_ALLOC_P_IDENTIFIER']/text()"/>
                <xsl:with-param name="pDataLength" select="13"/>
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}">
            <fo:block border="{$vBorder}">
              <xsl:call-template name="DisplayData_Truncate">
                <xsl:with-param name="pData" select="Column[@ColumnName='DC_IDENTIFIER']/text()"/>
                <xsl:with-param name="pDataLength" select="6"/>
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}">
            <fo:block border="{$vBorder}">
              <xsl:value-of select="Column[@ColumnName='PUTCALL']/text()"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}">
            <fo:block border="{$vBorder}">
              <xsl:value-of select="Column[@ColumnName='FLOWS_ASS_UNIT']/text()"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}">
            <fo:block border="{$vBorder}">
              <xsl:if test="Column[@ColumnName='FLOWS_ASS_CV_IDCREPORT' and string-length(text()) > 0]">
                <xsl:value-of select="Column[@ColumnName='FLOWS_ASS_CV_IDCREPORT']/text()"/>
              </xsl:if>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" text-align="right">
            <fo:block border="{$vBorder}">
              <xsl:if test="Column[@ColumnName='FLOWS_ASS_CV_SPOTRATE' and string-length(text()) > 0]">
                <xsl:call-template name="format-number">
                  <xsl:with-param name="pAmount" select="Column[@ColumnName='FLOWS_ASS_CV_SPOTRATE']/text()"/>
                  <xsl:with-param name="pAmountPattern" select="$vNumberPatternNoZero"/>
                  <xsl:with-param name="pDefaultCulture" select="$vDefaultCulture" />
                </xsl:call-template>
              </xsl:if>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" text-align="right">
            <fo:block border="{$vBorder}">
              <xsl:call-template name="format-integer">
                <xsl:with-param name="integer" select="Column[@ColumnName='FLOWS_ASS_QTY_BUY']/text()"/>
                <xsl:with-param name="pAmountPattern" select="$vIntegerPattern"/>
                <xsl:with-param name="pDefaultCulture" select="$vDefaultCulture" />
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" text-align="right">
            <fo:block border="{$vBorder}">
              <xsl:call-template name="format-integer">
                <xsl:with-param name="integer" select="Column[@ColumnName='FLOWS_ASS_QTY_SELL']/text()"/>
                <xsl:with-param name="pAmountPattern" select="$vIntegerPattern"/>
                <xsl:with-param name="pDefaultCulture" select="$vDefaultCulture" />
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" text-align="right">
            <fo:block border="{$vBorder}">
              <xsl:call-template name="format-number">
                <xsl:with-param name="pAmount" select="Column[@ColumnName='FLOWS_ASS_VARMARGINAMOUNT']/text()"/>
                <xsl:with-param name="pAmountPattern" select="$vNumberDecPattern"/>
                <xsl:with-param name="pDefaultCulture" select="$vDefaultCulture" />
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" text-align="right">
            <fo:block border="{$vBorder}">
              <xsl:call-template name="format-number">
                <xsl:with-param name="pAmount" select="Column[@ColumnName='FLOWS_ASS_PREMIUMAMOUNT']/text()"/>
                <xsl:with-param name="pAmountPattern" select="$vNumberDecPattern"/>
                <xsl:with-param name="pDefaultCulture" select="$vDefaultCulture" />
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" text-align="right">
            <fo:block border="{$vBorder}">
              <xsl:call-template name="format-number">
                <xsl:with-param name="pAmount" select="Column[@ColumnName='FLOWS_ASS_REALMARGINAMOUNT']/text()"/>
                <xsl:with-param name="pAmountPattern" select="$vNumberDecPattern"/>
                <xsl:with-param name="pDefaultCulture" select="$vDefaultCulture" />
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" text-align="right">
            <fo:block border="{$vBorder}">
              <xsl:if test="Column[@ColumnName='FLOWS_ASS_CV_REALMARGINAMT_CV' and string-length(text()) > 0]">
                <xsl:call-template name="format-number">
                  <xsl:with-param name="pAmount" select="Column[@ColumnName='FLOWS_ASS_CV_REALMARGINAMT_CV']/text()"/>
                  <xsl:with-param name="pAmountPattern" select="$vNumberDecPattern"/>
                  <xsl:with-param name="pDefaultCulture" select="$vDefaultCulture" />
                </xsl:call-template>
              </xsl:if>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" text-align="right">
            <fo:block border="{$vBorder}">
              <xsl:call-template name="format-number">
                <xsl:with-param name="pAmount" select="Column[@ColumnName='FLOWS_ASS_FEEEXCLTAXAMOUNT']/text()"/>
                <xsl:with-param name="pAmountPattern" select="$vNumberDecPattern"/>
                <xsl:with-param name="pDefaultCulture" select="$vDefaultCulture" />
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" text-align="right">
            <fo:block border="{$vBorder}">
              <xsl:call-template name="format-number">
                <xsl:with-param name="pAmount" select="Column[@ColumnName='FLOWS_ASS_TAXAMOUNT']/text()"/>
                <xsl:with-param name="pAmountPattern" select="$vNumberDecPattern"/>
                <xsl:with-param name="pDefaultCulture" select="$vDefaultCulture" />
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell border="{$varTableBorder}" padding="{$gCellPadding}" text-align="right">
            <fo:block border="{$vBorder}">
              <xsl:call-template name="format-number">
                <xsl:with-param name="pAmount" select="Column[@ColumnName='TOTAL']/text()"/>
                <xsl:with-param name="pAmountPattern" select="$vNumberDecPattern"/>
                <xsl:with-param name="pDefaultCulture" select="$vDefaultCulture" />
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
      </xsl:for-each>
    </xsl:if>
    <fo:table-row display-align="center" color="black" font-family="{$vFontFamily}" font-size="{$vFontSizeColumnTitles}" text-align="center" font-weight="bold" keep-with-previous="always">
      <xsl:if test="string-length($pTotalBackgroundColor) > 0">
        <xsl:attribute name="background-color">
          <xsl:value-of select="$gHdrCellBackgroundColor"/>
        </xsl:attribute>
      </xsl:if>
      <fo:table-cell padding="{$gCellPadding}" text-align="right" font-style="italic" number-columns-spanned="3">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'TS-Total'"/>
            <xsl:with-param name="pCulture" select="$pCulture" />
          </xsl:call-template>
          <xsl:value-of select="concat(' : ',$pActor, ' ', $pCurrency)"/>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$gCellPadding}" text-align="left" number-columns-spanned="4">
        <fo:block border="{$vBorder}"/>
      </fo:table-cell>
      <fo:table-cell padding="{$gCellPadding}" text-align="right">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="format-integer">
            <xsl:with-param name="integer" select="sum($pRows/Column[@ColumnName='FLOWS_ASS_QTY_BUY']/text())"/>
            <xsl:with-param name="pAmountPattern" select="$vIntegerPattern"/>
            <xsl:with-param name="pDefaultCulture" select="$vDefaultCulture" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$gCellPadding}" text-align="right">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="format-integer">
            <xsl:with-param name="integer" select="sum($pRows/Column[@ColumnName='FLOWS_ASS_QTY_SELL']/text())"/>
            <xsl:with-param name="pAmountPattern" select="$vIntegerPattern"/>
            <xsl:with-param name="pDefaultCulture" select="$vDefaultCulture" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$gCellPadding}" text-align="right">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="format-number">
            <xsl:with-param name="pAmount" select="sum($pRows/Column[@ColumnName='FLOWS_ASS_VARMARGINAMOUNT']/text())"/>
            <xsl:with-param name="pAmountPattern" select="$vNumberDecPattern"/>
            <xsl:with-param name="pDefaultCulture" select="$vDefaultCulture" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$gCellPadding}" text-align="right">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="format-number">
            <xsl:with-param name="pAmount" select="sum($pRows/Column[@ColumnName='FLOWS_ASS_PREMIUMAMOUNT']/text())"/>
            <xsl:with-param name="pAmountPattern" select="$vNumberDecPattern"/>
            <xsl:with-param name="pDefaultCulture" select="$vDefaultCulture" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$gCellPadding}" text-align="right">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="format-number">
            <xsl:with-param name="pAmount" select="sum($pRows/Column[@ColumnName='FLOWS_ASS_REALMARGINAMOUNT']/text())"/>
            <xsl:with-param name="pAmountPattern" select="$vNumberDecPattern"/>
            <xsl:with-param name="pDefaultCulture" select="$vDefaultCulture" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$gCellPadding}" text-align="right">
        <fo:block border="{$vBorder}">
          <xsl:if test="$pWithCVTotal = true() and $pRows/Column[@ColumnName='FLOWS_ASS_CV_REALMARGINAMT_CV' and string-length(text()) > 0]">
            <xsl:call-template name="format-number">
              <xsl:with-param name="pAmount" select="sum($pRows/Column[@ColumnName='FLOWS_ASS_CV_REALMARGINAMT_CV']/text())"/>
              <xsl:with-param name="pAmountPattern" select="$vNumberDecPattern"/>
              <xsl:with-param name="pDefaultCulture" select="$vDefaultCulture" />
            </xsl:call-template>
          </xsl:if>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$gCellPadding}" text-align="right">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="format-number">
            <xsl:with-param name="pAmount" select="sum($pRows/Column[@ColumnName='FLOWS_ASS_FEEEXCLTAXAMOUNT']/text())"/>
            <xsl:with-param name="pAmountPattern" select="$vNumberDecPattern"/>
            <xsl:with-param name="pDefaultCulture" select="$vDefaultCulture" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$gCellPadding}" text-align="right">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="format-number">
            <xsl:with-param name="pAmount" select="sum($pRows/Column[@ColumnName='FLOWS_ASS_TAXAMOUNT']/text())"/>
            <xsl:with-param name="pAmountPattern" select="$vNumberDecPattern"/>
            <xsl:with-param name="pDefaultCulture" select="$vDefaultCulture" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$gCellPadding}" text-align="right">
        <fo:block border="{$vBorder}">
          <xsl:call-template name="format-number">
            <xsl:with-param name="pAmount" select="sum($pRows/Column[@ColumnName='TOTAL']/text())"/>
            <xsl:with-param name="pAmountPattern" select="$vNumberDecPattern"/>
            <xsl:with-param name="pDefaultCulture" select="$vDefaultCulture" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
    </fo:table-row>
  </xsl:template>

</xsl:stylesheet>
