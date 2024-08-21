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
 
Version 1.0.0.2 (Spheres 2.6.4.0)
 Date: 03/08/2012 Ticket 18065
 Author: MF
 Description: 
 Enhancements
 - New dynamic values for template GetColumnDynamicData 
  (nameMarket;nameDc;nameCss;rowspanAmount;rowspanMarketCss;hideEmptyCell;hideAmountName;hideMarketName)"/>)
 - New attribute "hide" on column/data node, in order to hide a column when that equals 'true'
 
Version 1.0.0.3 (Spheres 2.6.4.0)
 Date: 22/08/2012 [Ticket 18073]
 Author: MF
 Description: 
 Enhancements
 - New attribute "typenode" on column/data node, in order to copy all the node contents when equals to 'true', iow using copy-of instead value-of.
 
 
Version 1.0.0.3 (Spheres 2.6.4.0)
 Date: 22/08/2012 [Ticket 18073 Item 7]
 Author: MF
 Description: 
 Enhancements
 - New dynamic value for template GetColumnDynamicData : "showTotal"
 - the condition value can be evaluated, using GetColumnDynamicData
 
 
Version 1.0.0.4 (Spheres 2.6.4.0)
 Date: 03/09/2012 [Ticket 18048]
 Author: MF
 Description: 
 Enhancements
 - New dynamic value for template GetColumnDynamicData : "cbAmount", "cbCrDr", "cbDate", "border-bottom-width", "border-top-width"
 - the condition value can be evaluated, using GetColumnDynamicData
 
================================================================================================================
  -->

  <!-- ================================================== -->
  <!--        xslt includes                               -->
  <!-- ================================================== -->
  <xsl:include href="Shared_Report_PDFCommon_Page.xslt" />

  <xsl:variable name="zeroFormattedAmount">
    <xsl:call-template name="format-number">
      <xsl:with-param name="pAmount" select="number('0')" />
      <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
    </xsl:call-template>
  </xsl:variable>

  <!--============================================================================================================ -->
  <!--                                                                                                             -->
  <!--                                       Display Templates                                                     -->
  <!--                                                                                                             -->
  <!--============================================================================================================ -->

  <!-- .................................. -->
  <!--   DetailedTrades                   -->
  <!--( Break, Trade details, Subtotal, ...)-->
  <!-- .................................. -->
  <!--DisplayGroup-->
  <xsl:template match="group" mode="display">
    <xsl:variable name="vCurrentGroup" select="."/>
    <xsl:variable name="vCurrentGroupSettings" select="msxsl:node-set($gGroupDisplaySettings)/group[@name=$vCurrentGroup/@name]"/>
    <xsl:variable name="vCurrentGroupColumns" select="$vCurrentGroupSettings/column"/>

    <xsl:variable name="vGroupColumnsToDisplay">
      <xsl:apply-templates select="$vCurrentGroupColumns" mode="display-getcolumn">
        <xsl:sort select="@number" data-type="number"/>
        <xsl:with-param name="pGroup" select="." />
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="vGroupTableTotalWidth">
      <xsl:choose>
        <xsl:when test="string-length($vCurrentGroupSettings/@tableTotalWidth) > 0">
          <xsl:value-of select="$vCurrentGroupSettings/@tableTotalWidth"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$gDetTrdTableTotalWidth"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vCurrentGroupColumnsToDisplay" select="msxsl:node-set($vGroupColumnsToDisplay)/column"/>
    <!--//-->
    <fo:table table-layout="fixed" border="{$gcTableBorderDebug}" width="{$vGroupTableTotalWidth}" table-omit-header-at-break="false">

      <xsl:attribute name="padding-top">
        <xsl:choose>
          <xsl:when test="string-length($vCurrentGroupSettings/@padding-top) > 0">
            <xsl:value-of select="$vCurrentGroupSettings/@padding-top"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$gDetTrdTablePaddingTop"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <!--//-->
      <xsl:attribute name="table-omit-header-at-break">
        <xsl:choose>
          <xsl:when test="string-length($vCurrentGroupSettings/@table-omit-header-at-break) > 0">
            <xsl:value-of select="$vCurrentGroupSettings/@table-omit-header-at-break"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'false'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <!--//-->
      <xsl:call-template name="CreateDetailedTradesTable">
        <xsl:with-param name="pGroupColumnsToDisplay" select="$vCurrentGroupColumns" />
      </xsl:call-template>

      <!-- ===== Display Group Header and Detail Header ===== -->
      <xsl:variable name="vCurrentGroupTitlesToDisplay">
        <xsl:choose>
          <xsl:when test="$vCurrentGroup/@isFirst='true'">
            <xsl:copy-of select="$vCurrentGroupSettings/title[string-length(@isFirstOnly) = 0 or @isFirstOnly='true']"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:copy-of select="$vCurrentGroupSettings/title[string-length(@isFirstOnly) = 0 or @isFirstOnly='false']"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:call-template name="DisplayHeaderForGroup">
        <xsl:with-param name="pGroup" select="$vCurrentGroup" />
        <xsl:with-param name="pGroupTitle" select="msxsl:node-set($vCurrentGroupTitlesToDisplay)/title" />
        <xsl:with-param name="pGroupColumns" select="$vCurrentGroupColumnsToDisplay" />
        <xsl:with-param name="pIsWithColumnHeader" select="$vCurrentGroupSettings/@isWithColumnHeader" />
      </xsl:call-template>
      <fo:table-body>
        <xsl:choose>
          <xsl:when test="$vCurrentGroupSettings/@isDataGroup = 'true'">

            <xsl:variable name="vConditions">
              <xsl:apply-templates select="$vCurrentGroupSettings/condition" mode="valorize">
                <xsl:with-param name="pGroup" select="$vCurrentGroup" />
              </xsl:apply-templates>
            </xsl:variable>

            <!-- ===== Scan Group data ===== -->
            <xsl:apply-templates select="$vCurrentGroupSettings/row" mode="display">
              <xsl:sort select="@sort" data-type="number"/>
              <xsl:with-param name="pGroup" select="$vCurrentGroup" />
              <xsl:with-param name="pIsDataGroup" select="true()"/>
              <xsl:with-param name="pGroupConditionsValue" select="msxsl:node-set($vConditions)/condition"/>
              <xsl:with-param name="pGroupColumnsToDisplay" select="$vCurrentGroupColumnsToDisplay"/>
            </xsl:apply-templates>
          </xsl:when>
          <xsl:otherwise>
            <!-- ===== Scan Group elements ===== -->
            <xsl:apply-templates select="./*" mode="display">
              <xsl:with-param name="pGroup" select="$vCurrentGroup" />
              <xsl:with-param name="pGroupColumnsToDisplay" select="$vCurrentGroupColumnsToDisplay"/>
              <xsl:with-param name="pGroupName" select="$vCurrentGroupSettings/@name"/>
            </xsl:apply-templates>
          </xsl:otherwise>
        </xsl:choose>
      </fo:table-body>
    </fo:table>

    <!-- place a new page block after each group, bu the last one, the countervalue -->
    <xsl:if test="$vCurrentGroupSettings/@withNewPage = 'true' and position() != last()">
      <fo:block break-after='page'/>
    </xsl:if>
  </xsl:template>

  <xsl:template match="condition" mode="valorize">
    <xsl:param name="pGroup" />

    <xsl:variable name="vConditionValue" select="boolean($pGroup/*[@name=current()/@condition])"/>
    <condition name="{@name}" value="{$vConditionValue}"/>
  </xsl:template>

  <xsl:template match="column" mode="display-header">
    <xsl:param name="pIsPreviousSubHeader" select="false()" />
    <xsl:param name="pIsNextSubHeader" select="false()" />
    <xsl:param name="pIsDisplayData" select="false()" />
    <xsl:param name="pParentRowData"/>

    <xsl:variable name="vNumber-columns-spanned">
      <xsl:choose>
        <xsl:when test="$pIsDisplayData = false() and string-length(./header/@master-ressource)>0">
          <xsl:value-of select="./header/@master-ressource-colspan"/>
        </xsl:when>
        <xsl:when test="string-length(./header/@colspan)>0">
          <xsl:value-of select="./header/@colspan"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'1'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vNumber-rows-spanned">
      <xsl:choose>
        <xsl:when test="string-length(./header/@rowspan)>0">
          <xsl:value-of select="./header/@rowspan"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'1'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vText-align">
      <xsl:choose>
        <xsl:when test="string-length(./header/@text-align)>0">
          <xsl:value-of select="./header/@text-align"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'center'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <fo:table-cell number-rows-spanned="{$vNumber-rows-spanned}"
                   number-columns-spanned="{$vNumber-columns-spanned}"
                   text-align="{$vText-align}"
                   padding="{./@padding}">

      <xsl:if test="string-length(./header/@background-color)>0">
        <xsl:attribute name="background-color">
          <xsl:value-of select="./header/@background-color" />
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="string-length(./header/@border)>0">
        <xsl:attribute name="border">
          <xsl:value-of select="./header/@border" />
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="string-length(./header/@font-weight)>0">
        <xsl:attribute name="font-weight">
          <xsl:value-of select="./header/@font-weight" />
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="string-length(./header/@font-size)>0">
        <xsl:attribute name="font-size">
          <!--<xsl:value-of select="concat(./header/@font-size,'pt')" />-->
          <!--<xsl:value-of select="./header/@font-size" />-->
          <xsl:call-template name="GetFontSize">
            <xsl:with-param name="pFontSize" select="./header/@font-size"/>
          </xsl:call-template>
        </xsl:attribute>
      </xsl:if>

      <xsl:choose>
        <xsl:when test="$pIsDisplayData = false() or string-length(./@master-column) > 0 or string-length(./header/@master-ressource) > 0">
          <xsl:attribute name="border-top-style">
            <xsl:choose>
              <xsl:when test="string-length(./@master-column) > 0">
                <xsl:value-of select="'none'"/>
              </xsl:when>
              <xsl:when test="string-length(./header/@master-ressource) > 0 and $pIsDisplayData = false()">
                <xsl:value-of select="./header/@border-top-style"/>
              </xsl:when>
              <xsl:when test="string-length(./header/@master-ressource) > 0 and $pIsDisplayData = true()">
                <xsl:value-of select="'none'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="./header/@border-top-style"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
          <xsl:attribute name="border-bottom-style">
            <xsl:choose>
              <xsl:when test="(string-length(./@master-column) > 0 or number(./@rowspan)=2 or number(./header/@rowspan)=2) and ./header/@border-bottom-style = 'none'">
                <xsl:value-of select="'none'"/>
              </xsl:when>
              <xsl:when test="string-length(./@master-column) > 0 or number(./@rowspan)=2 or number(./header/@rowspan)=2">
                <xsl:value-of select="'solid'"/>
              </xsl:when>
              <xsl:when test="string-length(./header/@master-ressource) > 0 and $pIsDisplayData = false()">
                <xsl:value-of select="'none'"/>
              </xsl:when>
              <xsl:when test="string-length(./header/@master-ressource) > 0 and $pIsDisplayData = true() and ./header/@border-bottom-style = 'none'">
                <xsl:value-of select="'none'"/>
              </xsl:when>
              <xsl:when test="string-length(./header/@master-ressource) > 0 and $pIsDisplayData = true()">
                <xsl:value-of select="'solid'"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'none'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
          <xsl:attribute name="border-left-style">
            <xsl:choose>
              <xsl:when test="string-length(./@master-column) > 0 and $pIsPreviousSubHeader='true'">
                <xsl:value-of select="'none'"/>
              </xsl:when>
              <xsl:when test="string-length(./header/@master-ressource) > 0  and $pIsDisplayData = false() and ./header/@border-left-style = 'none'">
                <xsl:value-of select="'none'"/>
              </xsl:when>
              <xsl:when test="string-length(./header/@master-ressource) > 0  and $pIsDisplayData = false()">
                <xsl:value-of select="'solid'"/>
              </xsl:when>
              <xsl:when test="string-length(./header/@master-ressource) > 0 and $pIsDisplayData = true() and $pIsPreviousSubHeader='true'">
                <xsl:value-of select="'none'"/>
              </xsl:when>
              <xsl:when test="string-length(./header/@border-left-style)>0">
                <xsl:value-of select="./header/@border-left-style"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'solid'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
          <xsl:attribute name="border-right-style">
            <xsl:choose>
              <xsl:when test="string-length(./@master-column) > 0 and $pIsNextSubHeader='true'">
                <xsl:value-of select="'none'"/>
              </xsl:when>
              <xsl:when test="string-length(./header/@master-ressource) > 0 and $pIsDisplayData = false()">
                <xsl:value-of select="'solid'"/>
              </xsl:when>
              <xsl:when test="string-length(./header/@master-ressource) > 0 and $pIsDisplayData = true() and $pIsNextSubHeader='true'">
                <xsl:value-of select="'none'"/>
              </xsl:when>
              <xsl:when test="string-length(./header/@border-right-style)>0">
                <xsl:value-of select="./header/@border-right-style"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'solid'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>

          <xsl:if test="string-length(./header/@ressource1) > 0 ">
            <fo:block>
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="concat(./header/@ressource1,'')" />
              </xsl:call-template>
            </fo:block>
          </xsl:if>

          <fo:block>
            <xsl:variable name="vColumnHeader">
              <xsl:choose>
                <xsl:when test="string-length(./header/@master-ressource) > 0 and $pIsDisplayData = false()">
                  <xsl:call-template name="getSpheresTranslation">
                    <xsl:with-param name="pResourceName" select="concat(./header/@master-ressource,'')" />
                  </xsl:call-template>
                </xsl:when>
                <xsl:when test="string-length(./header/@ressource) > 0 ">
                  <xsl:choose>
                    <xsl:when test="contains(./header/@ressource,'%%')">
                      <xsl:call-template name="GetColumnDynamicData">
                        <xsl:with-param name="pValue" select="./header/@ressource" />
                        <xsl:with-param name="pParentRowData" select="$pParentRowData" />
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:call-template name="getSpheresTranslation">
                        <xsl:with-param name="pResourceName" select="concat(./header/@ressource,'')" />
                      </xsl:call-template>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="getSpheresTranslation">
                    <xsl:with-param name="pResourceName" select="concat(./@name,'')" />
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>

            <xsl:choose>
              <xsl:when test="number(./header/@length) > 0 and (string-length($vColumnHeader) > number(./header/@length))">
                <xsl:choose>
                  <xsl:when test="number(./header/@length) > number('1')">
                    <xsl:value-of select="substring(normalize-space($vColumnHeader),1,number(./header/@length) - 1)"/>
                    <xsl:value-of select="'...'"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="substring(normalize-space($vColumnHeader),1,1)"/>
                    <xsl:value-of select="'...'"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$vColumnHeader"/>
              </xsl:otherwise>
            </xsl:choose>

            <xsl:if test="(string-length(./@master-column) > 0 or 
                    string-length(./header/@ressource1) >0 or 
                    string-length(./header/@master-ressource) >0) 
                    and string-length(./header/@data) > 0">
              <fo:inline font-size="{$gDetTrdHdrDataFontSize}" font-weight="{$gDetTrdHdrDataFontWeight}">
                <xsl:value-of select="concat($gcEspace,./header/@data)" />
              </fo:inline>
            </xsl:if>

            <xsl:for-each select="./header/inline">
              <fo:inline>
                <xsl:copy-of select="./@*"/>
                <xsl:call-template name="GetColumnDynamicData">
                  <xsl:with-param name="pValue" select="./text()" />
                  <xsl:with-param name="pParentRowData" select="$pParentRowData" />
                </xsl:call-template>
              </fo:inline>
            </xsl:for-each>
          </fo:block>
        </xsl:when>
        <xsl:when test="$pIsDisplayData = true()">
          <xsl:attribute name="border-top-style">
            <xsl:value-of select="'none'"/>
          </xsl:attribute>
          <xsl:attribute name="border-bottom-style">
            <xsl:value-of select="'solid'"/>
          </xsl:attribute>
          <xsl:attribute name="border-right-style">
            <xsl:value-of select="'solid'"/>
          </xsl:attribute>
          <xsl:attribute name="border-left-style">
            <xsl:value-of select="'solid'"/>
          </xsl:attribute>

          <fo:block font-size="{$gDetTrdHdrDataFontSize}" font-weight="{$gDetTrdHdrDataFontWeight}">
            <xsl:value-of select="./header/@data" />
          </fo:block>
        </xsl:when>
      </xsl:choose>
    </fo:table-cell>
  </xsl:template>
  <xsl:template match="column" mode="display-header-2nd">
    <xsl:param name="pGroupColumnsToDisplay" />
    <xsl:param name="pParentRowData"/>

    <xsl:variable name="vIsColumnHeaderToDisplay">
      <xsl:choose>
        <!--Display header of Sub column if master column is not 2 rows spanned or without master-header or master header is not 2 rows spanned-->
        <xsl:when test="string-length(./@master-column) > 0">
          <xsl:variable name="vMasterColumnName" select="normalize-space(./@master-column)"/>
          <xsl:variable name="vMasterColumn" select="$pGroupColumnsToDisplay[@name = $vMasterColumnName]"/>
          <xsl:choose>
            <xsl:when test="count($vMasterColumn) > 0 and string-length($vMasterColumn/header/@master-ressource) >0">
              <xsl:value-of select="false()"/>
            </xsl:when>
            <xsl:when test="count($vMasterColumn) > 0 and $vMasterColumn/header/@rowspan = 2">
              <xsl:value-of select="false()"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="true()"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <!--Display header of column if master header is not 2 rows spanned -->
        <xsl:when test="(string-length(./header/@master-ressource) > 0) and 
                  ((string-length(./header/@master-ressource-rowspan) = 0) or 
                  (number(./header/@master-ressource-number-rowspan) = 1))">
          <xsl:value-of select="true()"/>
        </xsl:when>
        <xsl:otherwise>
          <!--Display header column detail for column without Sub column-->
          <xsl:variable name="vColumnName" select="normalize-space(./@name)"/>
          <xsl:variable name="vSubColumn" select="$pGroupColumnsToDisplay[@master-column = $vColumnName]"/>

          <xsl:choose>
            <xsl:when test="count($vSubColumn) > 0">
              <xsl:value-of select="false()"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="true()"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:if test="$vIsColumnHeaderToDisplay = 'true'">

      <xsl:variable name="vIsPreviousSubHeader">
        <xsl:choose>
          <xsl:when test="string-length(./@master-column) > 0">
            <xsl:variable name="vMasterColumnName" select="normalize-space(./@master-column)"/>
            <xsl:variable name="vColumnNumber" select="normalize-space(./@number)"/>
            <xsl:variable name="vPreviousSubColumn" select="$pGroupColumnsToDisplay[(@master-column = $vMasterColumnName) and (number($vColumnNumber) > number(@number))]"/>

            <xsl:choose>
              <xsl:when test="count($vPreviousSubColumn) >0">
                <xsl:value-of select="true()"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="false()"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="string-length(./header/@master-ressource) > 0">
            <xsl:variable name="vMasterRessource" select="normalize-space(./header/@master-ressource)"/>
            <xsl:variable name="vColumnNumber" select="normalize-space(./@number)"/>
            <xsl:variable name="vPreviousColumnMasterRessource" select="$pGroupColumnsToDisplay[(./header/@master-ressource = $vMasterRessource) and (number($vColumnNumber) > number(@number))]"/>

            <xsl:choose>
              <xsl:when test="count($vPreviousColumnMasterRessource) >0">
                <xsl:value-of select="true()"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="false()"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="false()"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="vIsNextSubHeader">
        <xsl:choose>
          <xsl:when test="string-length(@master-column) > 0">
            <xsl:variable name="vMasterColumnName" select="normalize-space(./@master-column)"/>
            <xsl:variable name="vColumnNumber" select="normalize-space(./@number)"/>
            <xsl:variable name="vNextSubColumn" select="$pGroupColumnsToDisplay[(@master-column = $vMasterColumnName) and (number(@number) > number($vColumnNumber))]"/>

            <xsl:choose>
              <xsl:when test="count($vNextSubColumn) >0  and (string-length($vNextSubColumn/header/@colspan) = 0 or number($vNextSubColumn/header/@colspan) > 0)">
                <xsl:value-of select="true()"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="false()"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="string-length(./header/@master-ressource) > 0">
            <xsl:variable name="vMasterRessource" select="normalize-space(./header/@master-ressource)"/>
            <xsl:variable name="vColumnNumber" select="normalize-space(./@number)"/>
            <xsl:variable name="vNextColumnMasterRessource" select="$pGroupColumnsToDisplay[(./header/@master-ressource = $vMasterRessource) and (number(@number) > number($vColumnNumber))]"/>

            <xsl:choose>
              <xsl:when test="count($vNextColumnMasterRessource) >0">
                <xsl:value-of select="true()"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="false()"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="false()"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:apply-templates select="." mode="display-header">
        <xsl:sort select="@number" data-type="number"/>
        <xsl:with-param name="pIsPreviousSubHeader" select="$vIsPreviousSubHeader" />
        <xsl:with-param name="pIsNextSubHeader" select="$vIsNextSubHeader" />
        <xsl:with-param name="pIsDisplayData" select="true()" />
        <xsl:with-param name="pParentRowData" select="$pParentRowData" />
      </xsl:apply-templates>
    </xsl:if>
  </xsl:template>

  <xsl:template match="row" mode="display">
    <xsl:param name="pGroup"/>
    <xsl:param name="pIsDataGroup" select="false()"/>
    <xsl:param name="pGroupColumnsToDisplay"/>
    <xsl:param name="pGroupConditionsValue"/>
    <xsl:param name="pParentDataLinkValue"/>

    <xsl:call-template name="row-display">
      <xsl:with-param name="pGroup" select="$pGroup"/>
      <xsl:with-param name="pIsDataGroup" select="$pIsDataGroup"/>
      <xsl:with-param name="pGroupColumnsToDisplay" select="$pGroupColumnsToDisplay"/>
      <xsl:with-param name="pGroupConditionsValue" select="$pGroupConditionsValue"/>
      <xsl:with-param name="pParentDataLinkValue" select="$pParentDataLinkValue"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="row-display">
    <xsl:param name="pGroup"/>
    <xsl:param name="pIsDataGroup" select="false()"/>
    <xsl:param name="pGroupColumnsToDisplay"/>
    <xsl:param name="pGroupConditionsValue"/>
    <xsl:param name="pParentDataLinkValue"/>

    <xsl:variable name="vCurrentRow" select="."/>

    <xsl:variable name="vIsConditionVerify">
      <xsl:call-template name="IsConditionVerify">
        <xsl:with-param name="pConditionName" select="$vCurrentRow/@condition"/>
        <xsl:with-param name="pConditionsValue" select="$pGroupConditionsValue"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:if test="$vIsConditionVerify = 'true'">
      <xsl:choose>
        <xsl:when test="$vCurrentRow/@name='%%EMPTY%%'">
          <xsl:call-template name="CreateEmptyRow">
            <xsl:with-param name="pHeight" select="$vCurrentRow/@height" />
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="vRowColumnsToDisplay">
            <xsl:for-each select="$vCurrentRow/column">
              <xsl:call-template name="GetColumnToDisplayForSpecificRow">
                <xsl:with-param name="pGroupy" select="$pGroup" />
                <xsl:with-param name="pGroupColumnsToDisplay" select="$pGroupColumnsToDisplay" />
              </xsl:call-template>
            </xsl:for-each>
          </xsl:variable>

          <xsl:choose>
            <xsl:when test="$vCurrentRow/@name='%%FREE%%'">
              <fo:table-row>

                <xsl:if test="string-length($vCurrentRow/@font-size)>0">
                  <xsl:attribute name="font-size">
                    <!--<xsl:value-of select="concat($vCurrentRow/@font-size,'pt')"/>-->
                    <!--<xsl:value-of select="$vCurrentRow/@font-size"/>-->
                    <xsl:call-template name="GetFontSize">
                      <xsl:with-param name="pFontSize" select="$vCurrentRow/@font-size"/>
                    </xsl:call-template>
                  </xsl:attribute>
                </xsl:if>
                <xsl:if test="string-length($vCurrentRow/@font-weight)>0">
                  <xsl:attribute name="font-weight">
                    <xsl:value-of select="$vCurrentRow/@font-weight"/>
                  </xsl:attribute>
                </xsl:if>
                <xsl:if test="string-length($vCurrentRow/@background-color)>0">
                  <xsl:attribute name="background-color">
                    <xsl:value-of select="$vCurrentRow/@background-color"/>
                  </xsl:attribute>
                </xsl:if>

                <xsl:apply-templates select="msxsl:node-set($vRowColumnsToDisplay)/column" mode="display-data">
                  <xsl:sort select="@number" data-type="number"/>
                  <xsl:with-param name="pParentRowData" select="$pGroup" />
                </xsl:apply-templates>
              </fo:table-row>
            </xsl:when>
            <xsl:otherwise>
              <xsl:variable name="vCurrentChildRow" select="$vCurrentRow/row"/>
              <xsl:variable name="vCurrentDataLink" select="$vCurrentRow/@dataLink"/>

              <xsl:for-each select="$pGroup/*[@name=$vCurrentRow/@name]">

                <xsl:variable name="vCurrentDataLinkValue">
                  <xsl:if test="string-length($vCurrentDataLink)>0">
                    <xsl:call-template name="GetColumnDynamicData">
                      <xsl:with-param name="pValue" select="$vCurrentDataLink" />
                      <xsl:with-param name="pRowData" select="." />
                    </xsl:call-template>
                  </xsl:if>
                </xsl:variable>

                <xsl:variable name="vIsRowToDisplay" select="(string-length($pParentDataLinkValue) = 0) or 
                            (contains($vCurrentDataLinkValue,$pParentDataLinkValue))"/>

                <xsl:if test="$vIsRowToDisplay = true()">
                  <fo:table-row>

                    <xsl:if test="string-length($vCurrentRow/@font-size)>0">
                      <xsl:attribute name="font-size">
                        <!--<xsl:value-of select="concat($vCurrentRow/@font-size,'pt')"/>-->
                        <!--<xsl:value-of select="$vCurrentRow/@font-size"/>-->
                        <xsl:call-template name="GetFontSize">
                          <xsl:with-param name="pFontSize" select="$vCurrentRow/@font-size"/>
                        </xsl:call-template>
                      </xsl:attribute>
                    </xsl:if>
                    <xsl:if test="string-length($vCurrentRow/@font-weight)>0">
                      <xsl:attribute name="font-weight">
                        <xsl:value-of select="$vCurrentRow/@font-weight"/>
                      </xsl:attribute>
                    </xsl:if>
                    <xsl:if test="string-length($vCurrentRow/@background-color)>0">
                      <xsl:attribute name="background-color">
                        <xsl:value-of select="$vCurrentRow/@background-color"/>
                      </xsl:attribute>
                    </xsl:if>

                    <xsl:apply-templates select="msxsl:node-set($vRowColumnsToDisplay)/column" mode="display-data">
                      <xsl:sort select="@number" data-type="number"/>
                      <xsl:with-param name="pRowData" select="." />
                      <xsl:with-param name="pGroupConditionsValue" select="$pGroupConditionsValue"/>
                      <xsl:with-param name="pParentRowData" select="$pGroup" />
                    </xsl:apply-templates>
                  </fo:table-row>

                  <xsl:if test="$vCurrentChildRow">
                    <!--Display Child Row-->
                    <xsl:apply-templates select="$vCurrentChildRow" mode="display">
                      <xsl:sort select="@sort" data-type="number"/>
                      <xsl:with-param name="pGroup" select="$pGroup" />
                      <xsl:with-param name="pIsDataGroup" select="$pIsDataGroup"/>
                      <xsl:with-param name="pGroupColumnsToDisplay" select="$pGroupColumnsToDisplay"/>
                      <xsl:with-param name="pGroupConditionsValue" select="$pGroupConditionsValue"/>
                      <xsl:with-param name="pParentDataLinkValue" select="$vCurrentDataLinkValue"/>
                    </xsl:apply-templates>
                  </xsl:if>
                </xsl:if>

              </xsl:for-each>

            </xsl:otherwise>
          </xsl:choose>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
  </xsl:template>

  <!--IsConditionVerify-->
  <xsl:template name="IsConditionVerify">
    <xsl:param name="pConditionName" />
    <xsl:param name="pConditionsValue" />

    <xsl:choose>
      <xsl:when test="string-length($pConditionName) > 0">
        <xsl:value-of select="$pConditionsValue[@name=$pConditionName]/@value"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="true()"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <xsl:template match="column" mode="display-data">
    <xsl:param name="pRowData" />
    <xsl:param name="pGroupConditionsValue"/>
    <xsl:param name="pParentRowData" />
    <xsl:param name="pTradeWithStatus" />
    <xsl:param name="pIsFirst" select="false()" />
    <xsl:param name="pIsLast" select="false()" />
    <xsl:param name="pStar" />
    <xsl:param name="pIsPreviousSubColumn" select="false()" />
    <xsl:param name="pIsNextSubColumn" select="false()" />

    <xsl:variable name="vIsDataToDisplay" select="(@name != '%%EMPTY%%') and
                  (($pIsFirst = true()) or (string-length(./data/@isonlyfirstrow) = 0) or (./data/@isonlyfirstrow = false()))"/>

    <xsl:variable name="vColumnDataBrut">
      <xsl:if test="$vIsDataToDisplay = true()">

        <!-- 20120803 MF Ticket 18065 Item 7 - attribute "condition" dynamic evaluation -->
        <xsl:variable name="vConditionEvaluated">
          <xsl:choose>
            <xsl:when test="contains(current()/data/@condition,'%%')">
              <xsl:call-template name="GetColumnDynamicData">
                <xsl:with-param name="pValue" select="current()/data/@condition" />
                <xsl:with-param name="pRowData" select="$pRowData" />
                <xsl:with-param name="pParentRowData" select="$pParentRowData" />
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="current()/data/@condition" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="vIsConditionVerify">
          <xsl:call-template name="IsConditionVerify">
            <!-- 20120803 MF Ticket 18065 Item 7 -->
            <!--<xsl:with-param name="pConditionName" select="current()/data/@condition"/>-->
            <xsl:with-param name="pConditionName" select="$vConditionEvaluated"/>
            <xsl:with-param name="pConditionsValue" select="$pGroupConditionsValue"/>
          </xsl:call-template>
        </xsl:variable>

        <xsl:if test="$vIsConditionVerify = 'true'">
          <xsl:choose>
            <xsl:when test="string-length(./data/@value)>0">
              <xsl:call-template name="GetColumnDynamicData">
                <xsl:with-param name="pValue" select="./data/@value" />
                <xsl:with-param name="pRowData" select="$pRowData" />
                <xsl:with-param name="pParentRowData" select="$pParentRowData" />
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="GetColumnDataToDisplay">
                <xsl:with-param name="pRowData" select="$pRowData" />
                <xsl:with-param name="pTradeWithStatus" select="$pTradeWithStatus" />
                <xsl:with-param name="pParentRowData" select="$pParentRowData" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>
      </xsl:if>
    </xsl:variable>

    <xsl:variable name="vColumnData">
      <xsl:if test="string-length($vColumnDataBrut)>0">
        <xsl:choose>
          <xsl:when test="string-length(./data/@decPattern)>0">
            <xsl:call-template name="format-number">
              <xsl:with-param name="pAmount" select="$vColumnDataBrut" />
              <xsl:with-param name="pAmountPattern" select="./data/@decPattern" />
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <!-- 20120821 MF Ticket 18073 typenode attribute check, to select all the node contents including tags (using copy-of) -->
            <xsl:choose>
              <xsl:when test="./data/@typenode = 'true'">
                <xsl:copy-of select="msxsl:node-set($vColumnDataBrut)/node()/node()"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$vColumnDataBrut"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>
    </xsl:variable>

    <!-- 20120803 MF Ticket 18065 - attribute "hide", when that is "true" the table cell is not rendered -->
    <xsl:variable name="vHideCell">
      <xsl:choose>
        <xsl:when test="contains(./data/@hide,'%%')">
          <xsl:call-template name="GetColumnDynamicData">
            <xsl:with-param name="pValue" select="./data/@hide" />
            <xsl:with-param name="pRowData" select="$pRowData" />
            <xsl:with-param name="pParentRowData" select="$pParentRowData" />
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'false'" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:if test="$vHideCell != 'true'">

      <fo:table-cell border="{$gDetTrdCellBorder}">
        <xsl:choose>
          <xsl:when test="string-length(./data/@background-color)>0">
            <xsl:attribute name="background-color">
              <xsl:value-of select="./data/@background-color"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="@name = '%%EMPTY%%'">
            <xsl:attribute name="background-color">
              <xsl:value-of select="'white'"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>
        <xsl:if test="string-length(./data/@rowspan)>0">
          <xsl:attribute name="number-rows-spanned">
            <xsl:choose>
              <xsl:when test="contains(./data/@rowspan,'%%')">
                <xsl:call-template name="GetColumnDynamicData">
                  <xsl:with-param name="pValue" select="./data/@rowspan" />
                  <xsl:with-param name="pRowData" select="$pRowData" />
                  <xsl:with-param name="pParentRowData" select="$pParentRowData" />
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="./data/@rowspan" />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="string-length(./data/@colspan)>0">
          <xsl:attribute name="number-columns-spanned">
            <xsl:value-of select="./data/@colspan"/>
          </xsl:attribute>
        </xsl:if>

        <xsl:attribute name="border-top-style">
          <xsl:choose>
            <xsl:when test="string-length(./@master-column) > 0 or $vIsDataToDisplay = false()">
              <xsl:value-of select="'none'"/>
            </xsl:when>
            <xsl:when test="string-length(./data/@border-top-style) > 0">
              <xsl:value-of select="./data/@border-top-style"/>
            </xsl:when>
            <xsl:when test="@name='%%EMPTY%%'">
              <xsl:value-of select="'none'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'solid'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <xsl:attribute name="border-bottom-style">
          <xsl:choose>
            <xsl:when test="$vIsDataToDisplay = false()">
              <xsl:choose>
                <xsl:when test="($pIsLast = true()) and (string-length(./@master-column) > 0 or number(./data/@rowspan)=2)">
                  <xsl:value-of select="'solid'"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="'none'"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:when test="string-length(./@master-column) > 0 or number(./data/@rowspan)=2">
              <xsl:value-of select="'solid'"/>
            </xsl:when>
            <xsl:when test="$pIsLast = true() and string-length(./last/@border-bottom-style) > 0">
              <xsl:value-of select="./last/@border-bottom-style"/>
            </xsl:when>
            <xsl:when test="string-length(./data/@border-bottom-style) > 0">
              <xsl:value-of select="./data/@border-bottom-style"/>
            </xsl:when>
            <xsl:when test="@name='%%EMPTY%%'">
              <xsl:value-of select="'none'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'none'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <xsl:attribute name="border-left-style">
          <xsl:choose>
            <xsl:when test="@name='%%EMPTY%%'">
              <xsl:choose>
                <xsl:when test="string-length(./data/@border-left-style) > 0">
                  <xsl:value-of select="./data/@border-left-style"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="'none'"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:when test="$vIsDataToDisplay = false()">
              <xsl:variable name="vIsPreviousOnlyFirst">
                <xsl:variable name="vPreviousColumn" select="preceding-sibling::column[1]"/>

                <xsl:choose>
                  <xsl:when test="$vPreviousColumn">
                    <xsl:value-of select="$vPreviousColumn/data/@isonlyfirstrow"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="false()"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:variable>

              <xsl:choose>
                <xsl:when test="$vIsPreviousOnlyFirst = 'true'">
                  <xsl:value-of select="'none'"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="'solid'"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:when test="string-length(./@master-column) > 0 and $pIsPreviousSubColumn='true'">
              <xsl:value-of select="'none'"/>
            </xsl:when>
            <xsl:when test="string-length(./data/@border-left-style) > 0">
              <xsl:value-of select="./data/@border-left-style"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'solid'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <xsl:attribute name="border-right-style">
          <xsl:choose>
            <xsl:when test="@name='%%EMPTY%%'">
              <xsl:choose>
                <xsl:when test="string-length(./data/@border-right-style) > 0">
                  <xsl:value-of select="./data/@border-right-style"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="'none'"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:when test="$vIsDataToDisplay = false()">
              <xsl:variable name="vIsNextOnlyFirst">
                <xsl:variable name="vNextColumn" select="following-sibling::column[1]"/>

                <xsl:choose>
                  <xsl:when test="$vNextColumn">
                    <xsl:value-of select="$vNextColumn/data/@isonlyfirstrow"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="false()"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:variable>
              <xsl:choose>
                <xsl:when test="$vIsNextOnlyFirst = 'true'">
                  <xsl:value-of select="'none'"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="'solid'"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:when test="string-length(./@master-column) > 0 and $pIsNextSubColumn='true'">
              <xsl:value-of select="'none'"/>
            </xsl:when>
            <xsl:when test="string-length(./data/@border-right-style) > 0">
              <xsl:value-of select="./data/@border-right-style"/>
            </xsl:when>
            <xsl:when test="@name='%%EMPTY%%'">
              <xsl:value-of select="'none'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'solid'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <!-- MF 20120906 Ticket 18048 - the account statement report requires a specific border -->
        <xsl:choose>
          <xsl:when test="string-length(./data/@border-bottom-width) > 0">
            <xsl:attribute name="border-bottom-width">
              <xsl:value-of select="./data/@border-bottom-width"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:otherwise/>
        </xsl:choose>
        <!-- MF 20120906 Ticket 18048 - the account statement report requires a specific border -->
        <xsl:choose>
          <xsl:when test="string-length(./data/@border-top-width) > 0">
            <xsl:attribute name="border-top-width">
              <xsl:value-of select="./data/@border-top-width"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:otherwise/>
        </xsl:choose>

        <xsl:if test="string-length($vColumnData) > 0 or string-length($pStar) > 0">

          <xsl:attribute name="padding">
            <xsl:choose>
              <xsl:when test="string-length($pStar) > 0">
                <xsl:value-of select="concat($pStar,'pt')"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$gDetTrdCellPadding"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
          <xsl:attribute name="text-align">
            <xsl:choose>
              <xsl:when test="string-length(./data/@text-align)>0">
                <xsl:value-of select="./data/@text-align"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'left'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
          <xsl:variable name="IsAltData">
            <xsl:call-template name="GetColumnIsAltData">
              <xsl:with-param name="pRowData" select="$pRowData"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:choose>
            <xsl:when test="$IsAltData = 'true'">
              <xsl:attribute name="font-size">
                <!--<xsl:value-of select="concat(./data/@font-size-alt,'pt')"/>-->
                <!--<xsl:value-of select="./data/@font-size-alt"/>-->
                <xsl:call-template name="GetFontSize">
                  <xsl:with-param name="pFontSize" select="./data/@font-size-alt"/>
                </xsl:call-template>
              </xsl:attribute>
            </xsl:when>
            <xsl:when test="string-length(./data/@font-size)>0">
              <xsl:attribute name="font-size">
                <!--<xsl:value-of select="concat(./data/@font-size,'pt')"/>-->
                <!--<xsl:value-of select="./data/@font-size"/>-->
                <xsl:call-template name="GetFontSize">
                  <xsl:with-param name="pFontSize" select="./data/@font-size"/>
                </xsl:call-template>
              </xsl:attribute>
            </xsl:when>
          </xsl:choose>
          <xsl:if test="string-length(./data/@font-style)>0">
            <xsl:attribute name="font-style">
              <xsl:value-of select="./data/@font-style" />
            </xsl:attribute>
          </xsl:if>

          <xsl:variable name="vSpecificDataFontWeight">
            <xsl:call-template name="GetSpecificDataFontWeight">
              <xsl:with-param name="pRowData" select="$pRowData" />
            </xsl:call-template>
          </xsl:variable>

          <xsl:choose>
            <xsl:when test="string-length($vSpecificDataFontWeight)>0">
              <xsl:attribute name="font-weight">
                <xsl:value-of select="$vSpecificDataFontWeight"/>
              </xsl:attribute>
            </xsl:when>
            <xsl:when test="string-length(./data/@font-weight)>0">
              <xsl:attribute name="font-weight">
                <xsl:value-of select="./data/@font-weight"/>
              </xsl:attribute>
            </xsl:when>
          </xsl:choose>

          <xsl:choose>
            <xsl:when test="string-length($pStar) > 0">
              <xsl:attribute name="display-align">
                <xsl:value-of select="'after'"/>
              </xsl:attribute>
              <xsl:attribute name="margin-right">
                <xsl:value-of select="$gDetTrdCellPadding"/>
              </xsl:attribute>
            </xsl:when>
            <xsl:when test="string-length(./data/@display-align) > 0">
              <xsl:attribute name="display-align">
                <xsl:value-of select="./data/@display-align"/>
              </xsl:attribute>
            </xsl:when>
          </xsl:choose>

          <xsl:variable name="vColumnData_text-decoration" select="./data/@text-decoration" />
          <fo:block>
            <xsl:if test="string-length($vColumnData) > 0">
              <fo:inline>
                <xsl:if test="string-length($vColumnData_text-decoration)>0">
                  <xsl:attribute name="text-decoration">
                    <xsl:value-of select="$vColumnData_text-decoration"/>
                  </xsl:attribute>
                </xsl:if>
                <xsl:choose>
                  <xsl:when test="number(./data/@length) > 0 and (string-length($vColumnData) > number(./data/@length))">
                    <xsl:choose>
                      <xsl:when test="number(./data/@length) > number('1')">
                        <xsl:value-of select="substring(normalize-space($vColumnData),1,number(./data/@length) - 1)"/>
                        <xsl:value-of select="'...'"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="substring(normalize-space($vColumnData),1,1)"/>
                        <xsl:value-of select="'...'"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:when>
                  <xsl:otherwise>
                    <!-- 20120821 MF Ticket 18073 typenode attribute check, to select all the node contents including tags (using copy-of) -->
                    <xsl:choose>
                      <xsl:when test="./data/@typenode = 'true'">
                        <xsl:copy-of select="msxsl:node-set($vColumnData)/node()"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="$vColumnData"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:otherwise>
                </xsl:choose>
              </fo:inline>
            </xsl:if>

            <!--<xsl:if test="string-length($vColumnDetail) > 0">
            <fo:inline font-size="{$gDetTrdDetFontSize}" display-align="center">
              <xsl:choose>
                <xsl:when test="($vColumnDetail = $gcRec or $vColumnDetail = $gcPay)">
                  <xsl:choose>
                    <xsl:when test="string-length($vColumnData) > 0 and number($vColumnData) > 0">
                      <xsl:value-of select="concat($gcEspace,$vColumnDetail)" />
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:call-template name="DisplayEmptyAmountSufix"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="concat($gcEspace,$vColumnDetail)" />
                </xsl:otherwise>
              </xsl:choose>
            </fo:inline>
          </xsl:if>-->

            <xsl:if test="string-length($pStar) > 0">
              <fo:inline font-size="{$gDetTrdStarFontSize}" font-weight="bold">
                <xsl:value-of select="concat($gcEspace,$pStar)" />
              </fo:inline>
            </xsl:if>

            <xsl:for-each select="./data/inline">
              <fo:inline>
                <xsl:copy-of select="./@*[name()!='decPattern']"/>

                <xsl:if test="./@*[name()='text-decoration'] = false() and string-length($vColumnData_text-decoration)>0">
                  <xsl:attribute name="text-decoration">
                    <xsl:value-of select="$vColumnData_text-decoration"/>
                  </xsl:attribute>
                </xsl:if>

                <xsl:variable name="vInlineData">
                  <xsl:call-template name="GetColumnDynamicData">
                    <xsl:with-param name="pValue" select="./text()" />
                    <xsl:with-param name="pRowData" select="$pRowData" />
                    <xsl:with-param name="pParentRowData" select="$pParentRowData" />
                  </xsl:call-template>
                </xsl:variable>

                <xsl:if test="string-length($vInlineData)>0">
                  <xsl:choose>
                    <xsl:when test="string-length(./@decPattern)>0">
                      <xsl:call-template name="format-number">
                        <xsl:with-param name="pAmount" select="$vInlineData" />
                        <xsl:with-param name="pAmountPattern" select="./@decPattern" />
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="$vInlineData"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:if>

              </fo:inline>
            </xsl:for-each>
          </fo:block>
        </xsl:if>
      </fo:table-cell>

    </xsl:if>
  </xsl:template>
  <xsl:template match="column" mode="display-data-2nd">
    <xsl:param name="pGroupColumnsToDisplay" />
    <xsl:param name="pRowData" />
    <xsl:param name="pTradeWithStatus" />
    <xsl:param name="pIsFirst" select="false()" />
    <xsl:param name="pIsLast" select="false()" />
    <xsl:param name="pStar" />

    <xsl:variable name="vIsPreviousSubColumn">
      <xsl:choose>
        <xsl:when test="string-length(./@master-column) > 0">
          <xsl:variable name="vMasterColumnName" select="normalize-space(./@master-column)"/>
          <xsl:variable name="vColumnNumber" select="normalize-space(./@number)"/>
          <xsl:variable name="vPreviousSubColumn" select="$pGroupColumnsToDisplay[(@master-column = $vMasterColumnName) and (number($vColumnNumber) > number(@number))]"/>

          <xsl:choose>
            <xsl:when test="count($vPreviousSubColumn) >0">
              <xsl:value-of select="true()"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="false()"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="false()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vIsNextSubColumn">
      <xsl:choose>
        <xsl:when test="string-length(./@master-column) > 0">
          <xsl:variable name="vMasterColumnName" select="normalize-space(./@master-column)"/>
          <xsl:variable name="vColumnNumber" select="normalize-space(./@number)"/>
          <xsl:variable name="vNextSubColumn" select="$pGroupColumnsToDisplay[(@master-column = $vMasterColumnName) and (number(@number) > number($vColumnNumber))]"/>

          <xsl:choose>
            <xsl:when test="count($vNextSubColumn) >0">
              <xsl:value-of select="true()"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="false()"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="false()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:apply-templates select="." mode="display-data">
      <xsl:with-param name="pGroupColumnsToDisplay" select="$pGroupColumnsToDisplay" />
      <xsl:with-param name="pRowData" select="$pRowData" />
      <xsl:with-param name="pTradeWithStatus" select="$pTradeWithStatus" />
      <xsl:with-param name="pIsFirst" select="$pIsFirst" />
      <xsl:with-param name="pIsLast" select="$pIsLast" />
      <xsl:with-param name="pIsPreviousSubColumn" select="$vIsPreviousSubColumn" />
      <xsl:with-param name="pIsNextSubColumn" select="$vIsNextSubColumn" />
    </xsl:apply-templates>
  </xsl:template>

  <!--CreateDetailedTradesTable template-->
  <xsl:template name="CreateDetailedTradesTable">
    <xsl:param name="pGroupColumnsToDisplay" />

    <xsl:for-each select="$pGroupColumnsToDisplay">
      <xsl:sort select="@number" data-type="number"/>

      <xsl:variable name="vIsColumnToCreate">
        <xsl:choose>
          <!--Create Sub column-->
          <xsl:when test="string-length(@master-column) > 0">
            <xsl:value-of select="true()"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:variable name="vColumnName">
              <xsl:value-of select="normalize-space(@name)"/>
            </xsl:variable>
            <!--Not Create Master Column-->
            <xsl:variable name="vSubColumn" select="$pGroupColumnsToDisplay[@master-column = $vColumnName]"/>
            <xsl:choose>
              <xsl:when test="count($vSubColumn) > 0">
                <xsl:value-of select="false()"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="true()"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:if test="$vIsColumnToCreate = 'true'">
        <fo:table-column column-width="{@width}" column-number="{@number}" />
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <!--DisplayHeaderForGroup-->
  <xsl:template name="DisplayHeaderForGroup">
    <xsl:param name="pGroup" />
    <xsl:param name="pGroupTitle" />
    <xsl:param name="pGroupColumns" />
    <xsl:param name="pIsWithColumnHeader" select="true()" />

    <fo:table-header>
      <xsl:for-each select="$pGroupTitle">
        <xsl:sort select="@sort" data-type="number"/>

        <xsl:variable name="vCurrentTitle" select="current()"/>

        <xsl:variable name="vTitleColumnsToDisplay">
          <xsl:apply-templates select="$vCurrentTitle/column" mode="display-getcolumn">
            <xsl:sort select="@number" data-type="number"/>
            <xsl:with-param name="pGroup" select="$pGroup" />
          </xsl:apply-templates>
        </xsl:variable>
        <xsl:variable name="vCurrentTitleColumnsToDisplay" select="msxsl:node-set($vTitleColumnsToDisplay)/column"/>

        <!--Column header-->
        <xsl:if test="$vCurrentTitle/@withColumnHdr = 'true'">
          <fo:table-row font-size="{$gDetTrdHdrFontSize}" font-weight="{$gDetTrdHdrFontWeight}">
            <xsl:apply-templates select="$vCurrentTitleColumnsToDisplay" mode="display-header">
              <xsl:sort select="@number" data-type="number"/>
              <xsl:with-param name="pParentRowData" select="$pGroup" />
            </xsl:apply-templates>
          </fo:table-row>
        </xsl:if>
        <!--Data-->
        <fo:table-row>
          <xsl:if test="string-length($vCurrentTitle/@font-size)>0">
            <xsl:attribute name="font-size">
              <!--<xsl:value-of select="concat($vCurrentTitle/@font-size,'pt')"/>-->
              <!--<xsl:value-of select="$vCurrentTitle/@font-size"/>-->
              <xsl:call-template name="GetFontSize">
                <xsl:with-param name="pFontSize" select="$vCurrentTitle/@font-size"/>
              </xsl:call-template>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="string-length($vCurrentTitle/@font-weight)>0">
            <xsl:attribute name="font-weight">
              <xsl:value-of select="$vCurrentTitle/@font-weight"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="string-length($vCurrentTitle/@background-color)>0">
            <xsl:attribute name="background-color">
              <xsl:value-of select="$vCurrentTitle/@background-color"/>
            </xsl:attribute>
          </xsl:if>

          <xsl:apply-templates select="$vCurrentTitleColumnsToDisplay" mode="display-data">
            <xsl:sort select="@number" data-type="number"/>
            <xsl:with-param name="pRowData" select="$pGroup" />
            <xsl:with-param name="pParentRowData" select="$pGroup" />
          </xsl:apply-templates>
        </fo:table-row>
        <!--Empty row-->
        <xsl:if test="string-length($vCurrentTitle/@empty-row) > 0">
          <xsl:call-template name="CreateEmptyRow">
            <xsl:with-param name="pHeight" select="$vCurrentTitle/@empty-row" />
          </xsl:call-template>
        </xsl:if>
      </xsl:for-each>

      <xsl:if test="string-length($pIsWithColumnHeader) = 0 or $pIsWithColumnHeader = true()">
        <!--No Sub columns Header-->
        <fo:table-row font-size="{$gDetTrdHdrFontSize}" font-weight="{$gDetTrdHdrFontWeight}">
          <xsl:apply-templates select="$pGroupColumns
                             [((string-length(header/@colspan) = 0) or (number(header/@colspan) > 0)) and
                             string-length(@master-column) = 0 and 
                             ((string-length(header/@master-ressource) = 0) or (number(header/@master-ressource-colspan) > 0))]"
                               mode="display-header">
            <xsl:sort select="@number" data-type="number"/>
            <xsl:with-param name="pParentRowData" select="$pGroup" />
          </xsl:apply-templates>
        </fo:table-row>
        <!--Sub columns and details Headers-->
        <xsl:if test="$pGroupTitle/@withSubColumn = 'true'">
          <fo:table-row font-size="{$gDetTrdHdrFontSize}" font-weight="{$gDetTrdHdrFontWeight}">
            <xsl:apply-templates select="$pGroupColumns
                             [((string-length(header/@colspan) = 0) or (number(header/@colspan) > 0)) and
                             (string-length(header/@rowspan) = 0 or number(header/@rowspan) = 1)]" mode="display-header-2nd">
              <xsl:sort select="@number" data-type="number"/>
              <xsl:with-param name="pGroupColumnsToDisplay" select="$pGroupColumns" />
              <xsl:with-param name="pParentRowData" select="$pGroup" />
            </xsl:apply-templates>
          </fo:table-row>
        </xsl:if>
      </xsl:if>
    </fo:table-header>
  </xsl:template>

  <!-- .................................. -->
  <!--   Common Column                    -->
  <!-- .................................. -->
  <!--CreateEmptyRow template-->
  <xsl:template name="CreateEmptyRow">
    <xsl:param name="pHeight" select="'0.2cm'" />

    <fo:table-row height="{$pHeight}">
      <xsl:call-template name="CreateEmptyCellsForDetailedTrades">
        <xsl:with-param name="pnNumberOfCells" select="1" />
      </xsl:call-template>
    </fo:table-row>
  </xsl:template>
  <!--CreateEmptyCellsForDetailedTrades template-->
  <xsl:template name="CreateEmptyCellsForDetailedTrades">
    <xsl:param name="pnNumberOfCells" />
    <xsl:if test="number($pnNumberOfCells) &gt;= 1">
      <fo:table-cell/>
      <xsl:call-template name="CreateEmptyCellsForDetailedTrades">
        <xsl:with-param name="pnNumberOfCells" select="$pnNumberOfCells - 1" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <!-- ===== GetColumnToDisplay ===== -->
  <xsl:template name="GetColumnToDisplay">
    <xsl:param name="pGroup"/>
    <xsl:param name="pHeaderData" />

    <column number="{./@number}"
            name="{./@name}"
            width="{./@width}"
            padding="{$gDetTrdCellPadding}">

      <xsl:if test="string-length(./@colspan)>0">
        <xsl:attribute name="colspan">
          <xsl:value-of select="./@colspan"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="string-length(./@master-column)>0">
        <xsl:attribute name="master-column">
          <xsl:value-of select="./@master-column"/>
        </xsl:attribute>
      </xsl:if>

      <header border="{$gDetTrdHdrCellBorder}">

        <xsl:if test="string-length(./header/@ressource)>0">
          <xsl:attribute name="ressource">
            <xsl:value-of select="./header/@ressource"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="string-length(./header/@ressource1)>0">
          <xsl:attribute name="ressource1">
            <xsl:value-of select="./header/@ressource1"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="string-length(./header/@master-ressource)>0">
          <xsl:attribute name="master-ressource">
            <xsl:value-of select="./header/@master-ressource"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="string-length(./header/@master-ressource-colspan)>0">
          <xsl:attribute name="master-ressource-colspan">
            <xsl:value-of select="./header/@master-ressource-colspan"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="string-length(./header/@length)>0">
          <xsl:attribute name="length">
            <xsl:value-of select="./header/@length"/>
          </xsl:attribute>
        </xsl:if>

        <xsl:if test="string-length(./header/@rowspan)>0">
          <xsl:attribute name="rowspan">
            <xsl:value-of select="./header/@rowspan"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:choose>
          <xsl:when test="string-length(./header/@colspan)>0">
            <xsl:attribute name="colspan">
              <xsl:value-of select="./header/@colspan"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length(./@colspan)>0">
            <xsl:attribute name="colspan">
              <xsl:value-of select="./@colspan"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>

        <xsl:attribute name="text-align">
          <xsl:choose>
            <xsl:when test="string-length(./header/@text-align)>0">
              <xsl:value-of select="./header/@text-align"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$gDetTrdHdrTextAlign"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <xsl:attribute name="background-color">
          <xsl:choose>
            <xsl:when test="string-length(./header/@background-color)>0">
              <xsl:value-of select="./header/@background-color"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$gDetTrdHdrCellBackgroundColor"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>

        <xsl:if test="string-length($pHeaderData)>0">
          <xsl:attribute name="data">
            <xsl:value-of select="concat('(',$pHeaderData,')')"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:attribute name="font-size">
          <xsl:choose>
            <xsl:when test="string-length(./header/@font-size)>0">
              <xsl:value-of select="./header/@font-size"/>
            </xsl:when>
            <xsl:when test="string-length(./@font-size)>0">
              <xsl:value-of select="./@font-size"/>
            </xsl:when>
          </xsl:choose>
        </xsl:attribute>
        <xsl:if test="string-length(./header/@font-weight)>0">
          <xsl:attribute name="font-weight">
            <xsl:value-of select="./header/@font-weight"/>
          </xsl:attribute>
        </xsl:if>

        <xsl:attribute name="border-top-style">
          <xsl:choose>
            <xsl:when test="string-length(./header/@border-top-style)>0">
              <xsl:value-of select="./header/@border-top-style"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$gDetTrdHdrCellBorderTopStyle"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <xsl:if test="string-length(./header/@border-bottom-style)>0">
          <xsl:attribute name="border-bottom-style">
            <xsl:value-of select="./header/@border-bottom-style"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="string-length(./header/@border-left-style)>0">
          <xsl:attribute name="border-left-style">
            <xsl:value-of select="./header/@border-left-style"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="string-length(./header/@border-right-style)>0">
          <xsl:attribute name="border-right-style">
            <xsl:value-of select="./header/@border-right-style"/>
          </xsl:attribute>
        </xsl:if>

        <xsl:copy-of select="./header/inline"/>

      </header>

      <data>
        <xsl:if test="string-length(./data/@rowspan)>0">
          <xsl:attribute name="rowspan">
            <xsl:value-of select="./data/@rowspan"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:choose>
          <xsl:when test="string-length(./data/@colspan)>0">
            <xsl:attribute name="colspan">
              <xsl:value-of select="./data/@colspan"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length(./@colspan)>0">
            <xsl:attribute name="colspan">
              <xsl:value-of select="./@colspan"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>
        <!-- 20120803 MF - copying "hide" attribute from template to physical column -->
        <xsl:if test="string-length(./data/@hide)>0">
          <xsl:attribute name="hide">
            <xsl:value-of select="./data/@hide"/>
          </xsl:attribute>
        </xsl:if>
        <!-- 20120822 MF - copying "typenode" attribute from template to physical column -->
        <xsl:if test="string-length(./data/@typenode)>0">
          <xsl:attribute name="typenode">
            <xsl:value-of select="./data/@typenode"/>
          </xsl:attribute>
        </xsl:if>

        <xsl:if test="string-length(./data/@background-color)>0">
          <xsl:attribute name="background-color">
            <xsl:value-of select="./data/@background-color"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="string-length(./data/@text-align)>0">
          <xsl:attribute name="text-align">
            <xsl:value-of select="./data/@text-align"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="string-length(./data/@display-align)>0">
          <xsl:attribute name="display-align">
            <xsl:value-of select="./data/@display-align"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="string-length(./data/@text-decoration)>0">
          <xsl:attribute name="text-decoration">
            <xsl:value-of select="./data/@text-decoration"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="string-length(./data/@length)>0">
          <xsl:attribute name="length">
            <xsl:value-of select="./data/@length"/>
          </xsl:attribute>
        </xsl:if>

        <xsl:choose>
          <xsl:when test="string-length(./data/@font-size)>0">
            <xsl:attribute name="font-size">
              <xsl:value-of select="./data/@font-size"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length(./@font-size)>0">
            <xsl:attribute name="font-size">
              <xsl:value-of select="./@font-size"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>
        <xsl:choose>
          <xsl:when test="string-length(./data/@font-weight)>0">
            <xsl:attribute name="font-weight">
              <xsl:value-of select="./data/@font-weight"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length(./@font-weight)>0">
            <xsl:attribute name="font-weight">
              <xsl:value-of select="./@font-weight"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>
        <xsl:choose>
          <xsl:when test="string-length(./data/@font-style)>0">
            <xsl:attribute name="font-style">
              <xsl:value-of select="./data/@font-style"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length(./@font-style)>0">
            <xsl:attribute name="font-style">
              <xsl:value-of select="./@font-style"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>
        <xsl:if test="string-length(./data/@font-size-alt)>0">
          <xsl:attribute name="font-size-alt">
            <xsl:value-of select="./data/@font-size-alt"/>
          </xsl:attribute>
        </xsl:if>

        <xsl:choose>
          <xsl:when test="string-length(./data/@value)>0">
            <xsl:attribute name="value">
              <xsl:value-of select="./data/@value"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length(./@value)>0">
            <xsl:attribute name="value">
              <xsl:value-of select="./@value"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>

        <xsl:if test="string-length(./data/@isonlyfirstrow)>0">
          <xsl:attribute name="isonlyfirstrow">
            <xsl:value-of select="./data/@isonlyfirstrow"/>
          </xsl:attribute>
        </xsl:if>

        <xsl:choose>
          <xsl:when test="string-length(./data/@border)>0">
            <xsl:attribute name="border">
              <xsl:value-of select="./data/@border"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length(./@border)>0">
            <xsl:attribute name="border">
              <xsl:value-of select="./@border"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>

        <xsl:if test="string-length(./data/@border-left-style)>0">
          <xsl:attribute name="border-left-style">
            <xsl:value-of select="./data/@border-left-style"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="string-length(./data/@border-right-style)>0">
          <xsl:attribute name="border-right-style">
            <xsl:value-of select="./data/@border-right-style"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="string-length(./data/@border-bottom-style)>0">
          <xsl:attribute name="border-bottom-style">
            <xsl:value-of select="./data/@border-bottom-style"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="string-length(./data/@border-top-style)>0">
          <xsl:attribute name="border-top-style">
            <xsl:value-of select="./data/@border-top-style"/>
          </xsl:attribute>
        </xsl:if>
        <!-- 20120906 MF - copying "border-bottom-width" attribute from template to physical column -->
        <xsl:if test="string-length(./data/@border-bottom-width)>0">
          <xsl:attribute name="typenode">
            <xsl:value-of select="./data/@border-bottom-width"/>
          </xsl:attribute>
        </xsl:if>
        <!-- 20120906 MF - copying "border-top-width" attribute from template to physical column -->
        <xsl:if test="string-length(./data/@border-top-width)>0">
          <xsl:attribute name="typenode">
            <xsl:value-of select="./data/@border-top-width"/>
          </xsl:attribute>
        </xsl:if>

        <xsl:if test="string-length(./data/@decPattern)>0">
          <xsl:attribute name="decPattern">
            <xsl:call-template name="GetColumnDynamicData">
              <xsl:with-param name="pValue" select="./data/@decPattern" />
              <xsl:with-param name="pRowData" select="$pGroup" />
              <xsl:with-param name="pParentRowData" select="$pGroup" />
            </xsl:call-template>
          </xsl:attribute>
        </xsl:if>

        <xsl:copy-of select="./data/inline"/>

      </data>

      <xsl:copy-of select="./last"/>
    </column>

  </xsl:template>
  <!-- ===== GetColumnToDisplayForSpecificData ===== -->
  <xsl:template name="GetColumnToDisplayForSpecificRow">
    <xsl:param name="pGroup" />
    <xsl:param name="pGroupColumnsToDisplay" />

    <xsl:variable name="vCurrentColumnToDisplay" select="$pGroupColumnsToDisplay[@name=current()/@name]"/>

    <column>
      <xsl:choose>
        <xsl:when test="$vCurrentColumnToDisplay">
          <xsl:copy-of select="$vCurrentColumnToDisplay/@*"/>
          <xsl:copy-of select="$vCurrentColumnToDisplay/header"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:copy-of select="./@*"/>
        </xsl:otherwise>
      </xsl:choose>

      <data>
        <xsl:choose>
          <xsl:when test="string-length(./data/@condition)>0">
            <xsl:copy-of select="./data/@condition"/>
          </xsl:when>
          <xsl:when test="string-length($vCurrentColumnToDisplay/data/@condition)>0">
            <xsl:copy-of select="$vCurrentColumnToDisplay/data/@condition"/>
          </xsl:when>
        </xsl:choose>

        <xsl:choose>
          <xsl:when test="string-length(./data/@rowspan)>0">
            <xsl:attribute name="rowspan">
              <xsl:value-of select="./data/@rowspan"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length($vCurrentColumnToDisplay/data/@rowspan)>0">
            <xsl:attribute name="rowspan">
              <xsl:value-of select="$vCurrentColumnToDisplay/data/@rowspan"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>
        <xsl:choose>
          <xsl:when test="string-length(./data/@colspan)>0">
            <xsl:attribute name="colspan">
              <xsl:value-of select="./data/@colspan"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length($vCurrentColumnToDisplay/data/@colspan)>0">
            <xsl:attribute name="colspan">
              <xsl:value-of select="$vCurrentColumnToDisplay/data/@colspan"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>

        <!-- 20120803 MF - copying "hide" attribute from template to physical column -->
        <xsl:choose>
          <xsl:when test="string-length(./data/@hide)>0">
            <xsl:attribute name="hide">
              <xsl:value-of select="./data/@hide"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length($vCurrentColumnToDisplay/data/@hide)>0">
            <xsl:attribute name="hide">
              <xsl:value-of select="$vCurrentColumnToDisplay/data/@hide"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>

        <!-- 20120822 MF - copying "typenode" attribute from template to physical column -->
        <xsl:choose>
          <xsl:when test="string-length(./data/@typenode)>0">
            <xsl:attribute name="typenode">
              <xsl:value-of select="./data/@typenode"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length($vCurrentColumnToDisplay/data/@typenode)>0">
            <xsl:attribute name="typenode">
              <xsl:value-of select="$vCurrentColumnToDisplay/data/@typenode"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>

        <xsl:choose>
          <xsl:when test="string-length(./data/@background-color)>0">
            <xsl:attribute name="background-color">
              <xsl:value-of select="./data/@background-color"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length($vCurrentColumnToDisplay/data/@background-color)>0">
            <xsl:attribute name="background-color">
              <xsl:value-of select="$vCurrentColumnToDisplay/data/@background-color"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>
        <xsl:choose>
          <xsl:when test="string-length(./data/@text-align)>0">
            <xsl:attribute name="text-align">
              <xsl:value-of select="./data/@text-align"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length($vCurrentColumnToDisplay/data/@text-align)>0">
            <xsl:attribute name="text-align">
              <xsl:value-of select="$vCurrentColumnToDisplay/data/@text-align"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>
        <xsl:choose>
          <xsl:when test="string-length(./data/@display-align)>0">
            <xsl:attribute name="display-align">
              <xsl:value-of select="./data/@display-align"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length($vCurrentColumnToDisplay/data/@display-align)>0">
            <xsl:attribute name="display-align">
              <xsl:value-of select="$vCurrentColumnToDisplay/data/@display-align"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>
        <xsl:choose>
          <xsl:when test="string-length(./data/@length)>0">
            <xsl:attribute name="length">
              <xsl:value-of select="./data/@length"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length($vCurrentColumnToDisplay/data/@length)>0">
            <xsl:attribute name="length">
              <xsl:value-of select="$vCurrentColumnToDisplay/data/@length"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>

        <xsl:choose>
          <xsl:when test="string-length(./data/@font-size)>0">
            <xsl:attribute name="font-size">
              <xsl:value-of select="./data/@font-size"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length($vCurrentColumnToDisplay/data/@font-size)>0">
            <xsl:attribute name="font-size">
              <xsl:value-of select="$vCurrentColumnToDisplay/data/@font-size"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>
        <xsl:choose>
          <xsl:when test="string-length(./data/@font-weight)>0">
            <xsl:attribute name="font-weight">
              <xsl:value-of select="./data/@font-weight"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length($vCurrentColumnToDisplay/data/@font-weight)>0">
            <xsl:attribute name="font-weight">
              <xsl:value-of select="$vCurrentColumnToDisplay/data/@font-weight"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>
        <xsl:choose>
          <xsl:when test="string-length(./data/@font-style)>0">
            <xsl:attribute name="font-style">
              <xsl:value-of select="./data/@font-style"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length($vCurrentColumnToDisplay/data/@font-style)>0">
            <xsl:attribute name="font-style">
              <xsl:value-of select="$vCurrentColumnToDisplay/data/@font-style"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>
        <xsl:choose>
          <xsl:when test="string-length(./data/@font-size-alt)>0">
            <xsl:attribute name="font-size-alt">
              <xsl:value-of select="./data/@font-size-alt"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length($vCurrentColumnToDisplay/data/@font-size-alt)>0">
            <xsl:attribute name="font-size-alt">
              <xsl:value-of select="$vCurrentColumnToDisplay/data/@font-size-alt"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>

        <xsl:choose>
          <xsl:when test="string-length(./data/@value)>0">
            <xsl:attribute name="value">
              <xsl:value-of select="./data/@value"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length($vCurrentColumnToDisplay/data/@value)>0">
            <xsl:attribute name="value">
              <xsl:value-of select="$vCurrentColumnToDisplay/data/@value"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>

        <xsl:choose>
          <xsl:when test="string-length(./data/@isonlyfirstrow)>0">
            <xsl:attribute name="isonlyfirstrow">
              <xsl:value-of select="./data/@isonlyfirstrow"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length($vCurrentColumnToDisplay/data/@isonlyfirstrow)>0">
            <xsl:attribute name="isonlyfirstrow">
              <xsl:value-of select="$vCurrentColumnToDisplay/data/@isonlyfirstrow"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>

        <xsl:choose>
          <xsl:when test="string-length(./data/@border)>0">
            <xsl:attribute name="border">
              <xsl:value-of select="./data/@border"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length($vCurrentColumnToDisplay/data/@border)>0">
            <xsl:attribute name="border">
              <xsl:value-of select="$vCurrentColumnToDisplay/data/@border"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>
        <xsl:choose>
          <xsl:when test="string-length(./data/@border-left-style)>0">
            <xsl:attribute name="border-left-style">
              <xsl:value-of select="./data/@border-left-style"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length($vCurrentColumnToDisplay/data/@border-left-style)>0">
            <xsl:attribute name="border-left-style">
              <xsl:value-of select="$vCurrentColumnToDisplay/data/@border-left-style"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>
        <xsl:choose>
          <xsl:when test="string-length(./data/@border-right-style)>0">
            <xsl:attribute name="border-right-style">
              <xsl:value-of select="./data/@border-right-style"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length($vCurrentColumnToDisplay/data/@border-right-style)>0">
            <xsl:attribute name="border-right-style">
              <xsl:value-of select="$vCurrentColumnToDisplay/data/@border-right-style"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>
        <xsl:choose>
          <xsl:when test="string-length(./data/@border-bottom-style)>0">
            <xsl:attribute name="border-bottom-style">
              <xsl:value-of select="./data/@border-bottom-style"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length($vCurrentColumnToDisplay/data/@border-bottom-style)>0">
            <xsl:attribute name="border-bottom-style">
              <xsl:value-of select="$vCurrentColumnToDisplay/data/@border-bottom-style"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>
        <xsl:choose>
          <xsl:when test="string-length(./data/@border-top-style)>0">
            <xsl:attribute name="border-top-style">
              <xsl:value-of select="./data/@border-top-style"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length($vCurrentColumnToDisplay/data/@border-top-style)>0">
            <xsl:attribute name="border-top-style">
              <xsl:value-of select="$vCurrentColumnToDisplay/data/@border-top-style"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>
        <!-- 20120906 MF Ticket 18048 - copying "border-bottom-width" attribute from template to physical column -->
        <xsl:choose>
          <xsl:when test="string-length(./data/@border-bottom-width)>0">
            <xsl:attribute name="border-bottom-width">
              <xsl:value-of select="./data/@border-bottom-width"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length($vCurrentColumnToDisplay/data/@border-bottom-width)>0">
            <xsl:attribute name="border-bottom-width">
              <xsl:value-of select="$vCurrentColumnToDisplay/data/@border-bottom-width"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>
        <!-- 20120906 MF Ticket 18048 - copying "border-top-width" attribute from template to physical column -->
        <xsl:choose>
          <xsl:when test="string-length(./data/@border-top-width)>0">
            <xsl:attribute name="border-top-width">
              <xsl:value-of select="./data/@border-top-width"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length($vCurrentColumnToDisplay/data/@border-top-width)>0">
            <xsl:attribute name="border-top-width">
              <xsl:value-of select="$vCurrentColumnToDisplay/data/@border-top-width"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>

        <xsl:choose>
          <xsl:when test="string-length(./data/@decPattern)>0">
            <xsl:attribute name="decPattern">
              <xsl:value-of select="./data/@decPattern"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="string-length($vCurrentColumnToDisplay/data/@decPattern)>0">
            <xsl:attribute name="decPattern">
              <xsl:value-of select="$vCurrentColumnToDisplay/data/@decPattern"/>
            </xsl:attribute>
          </xsl:when>
        </xsl:choose>

        <xsl:for-each select="./data/inline">
          <inline>
            <xsl:copy-of select="./@*[name()!='decPattern']"/>

            <xsl:if test="string-length(./@decPattern)>0">
              <xsl:attribute name="decPattern">
                <xsl:call-template name="GetColumnDynamicData">
                  <xsl:with-param name="pValue" select="./@decPattern" />
                  <xsl:with-param name="pRowData" select="$pGroup" />
                  <xsl:with-param name="pParentRowData" select="$pGroup" />
                </xsl:call-template>
              </xsl:attribute>
            </xsl:if>

            <xsl:value-of select="./text()"/>
          </inline>
        </xsl:for-each>

      </data>

      <xsl:choose>
        <xsl:when test="./last">
          <xsl:copy-of select="./last"/>
        </xsl:when>
        <xsl:when test="string-length($vCurrentColumnToDisplay/last)>0">
          <xsl:copy-of select="$vCurrentColumnToDisplay/last"/>
        </xsl:when>
      </xsl:choose>

    </column>

  </xsl:template>

  <!-- ................................................ -->
  <!-- DisplayFmtPrice                                  -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display formatted price          
       ................................................ -->
  <xsl:template name="DisplayFmtPrice">
    <xsl:param name="pFmtPrice"/>

    <fmtPrice>
      <xsl:choose>
        <!--- "100'5" ou "100^5" ou "100-5" -->
        <xsl:when test="contains($pFmtPrice,&quot;&apos;&quot;) or contains($pFmtPrice,'^') or contains($pFmtPrice,'-')">
          <xsl:value-of select="$pFmtPrice"/>
        </xsl:when>
        <!--- "100 16/32"-->
        <xsl:when test="contains($pFmtPrice,'/')">
          <xsl:variable name="vData-before" select="substring-before($pFmtPrice,' ')"/>
          <xsl:variable name="vData-after" select="substring-after($pFmtPrice,' ')"/>
          <xsl:variable name="vSup" select="substring-before($vData-after,'/')"/>
          <xsl:variable name="vSub" select="substring-after($vData-after,'/')"/>

          <xsl:value-of select="$vData-before"/>
          <fo:inline vertical-align="super" font-size="60%">
            <xsl:value-of select="$vSup"/>
          </fo:inline>/<fo:inline font-size="60%">
            <xsl:value-of select="$vSub"/>
          </fo:inline>
        </xsl:when>
        <!--- "100.5"  (défaut)-->
        <xsl:when test="string-length($pFmtPrice) > 0">
          <xsl:call-template name="format-number">
            <xsl:with-param name="pAmount" select="normalize-space($pFmtPrice)" />
            <xsl:with-param name="pAmountPattern" select="$number4DecPattern" />
          </xsl:call-template>
        </xsl:when>
      </xsl:choose>
    </fmtPrice>
  </xsl:template>

  <!--GetColumnDynamicData-->
  <xsl:template name="GetComonColumnDynamicData">
    <xsl:param name="pValue"/>
    <xsl:param name="pRowData"/>
    <xsl:param name="pParentRowData"/>

    <xsl:choose>
      <!--%%RES:XXX%%-->
      <xsl:when test="contains($pValue,'%%RES:')">
        <xsl:variable name="vDynamicdata" select="substring-before(substring-after($pValue,'%%RES:'),'%%')" />
        <xsl:variable name="vDynamicdataRes">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="$vDynamicdata" />
          </xsl:call-template>
        </xsl:variable>
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$pValue"/>
          <xsl:with-param name="oldValue" select="concat('%%RES:',$vDynamicdata, '%%')"/>
          <xsl:with-param name="newValue" select="$vDynamicdataRes"/>
        </xsl:call-template>
      </xsl:when>

      <!--%%DESC:DetFees%%-->
      <xsl:when test="contains($pValue,'%%DESC:DetFees%%')">
        <xsl:variable name="vDynamicdata">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'CSHBAL-Note1'" />
          </xsl:call-template>
          <!-- MF 20120905 shorter label for detail -->
          <!--<xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'CSHBAL-feesConstituents'" />
          </xsl:call-template>-->
        </xsl:variable>
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DESC:DetFees%%'"/>
          <xsl:with-param name="newValue" select="$vDynamicdata"/>
        </xsl:call-template>
      </xsl:when>
      <!--%%DESC:DetCashBalance%%-->
      <xsl:when test="contains($pValue,'%%DESC:DetCashBalance%%')">
        <xsl:variable name="vDynamicdata">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'CSHBAL-Note1'" />
          </xsl:call-template>
          <!-- MF 20120905 shorter label for detail -->
          <!--<xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'CSHBAL-returnCallDepositDet'" />
          </xsl:call-template>-->
        </xsl:variable>
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DESC:DetCashBalance%%'"/>
          <xsl:with-param name="newValue" select="$vDynamicdata"/>
        </xsl:call-template>
      </xsl:when>

      <!--%%DATA:name.RES%%-->
      <xsl:when test="contains($pValue,'%%DATA:name.RES%%')">
        <xsl:variable name="vDynamicdata">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="$pRowData/@name" />
          </xsl:call-template>
        </xsl:variable>
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DATA:name.RES%%'"/>
          <xsl:with-param name="newValue" select="$vDynamicdata"/>
        </xsl:call-template>
      </xsl:when>
      <!--%%DATA:taxDetailName.RES%%-->
      <xsl:when test="contains($pValue,'%%DATA:taxDetailName.RES%%')">
        <xsl:variable name="vDynamicdata">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="$pRowData/@taxDetailName" />
          </xsl:call-template>
        </xsl:variable>
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DATA:taxDetailName.RES%%'"/>
          <xsl:with-param name="newValue" select="$vDynamicdata"/>
        </xsl:call-template>
      </xsl:when>
      <!--%%DATA:keyForSum.RES%%-->
      <xsl:when test="contains($pValue,'%%DATA:keyForSum.RES%%')">
        <xsl:variable name="vDynamicdata">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="concat('CSHBAL-',$pRowData/@keyForSum)" />
          </xsl:call-template>
        </xsl:variable>
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DATA:keyForSum.RES%%'"/>
          <xsl:with-param name="newValue" select="$vDynamicdata"/>
        </xsl:call-template>
      </xsl:when>
      <!--%%DATA:crDrResource.RES%%-->
      <xsl:when test="contains($pValue,'%%DATA:crDrResource.RES%%')">
        <xsl:variable name="vDynamicdata">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="$pRowData/@crDrResource" />
          </xsl:call-template>
        </xsl:variable>
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DATA:crDrResource.RES%%'"/>
          <xsl:with-param name="newValue" select="$vDynamicdata"/>
        </xsl:call-template>
      </xsl:when>
      <!--%%DATA:currency.PARENT%%-->
      <xsl:when test="contains($pValue,'%%DATA:currency.PARENT%%')">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DATA:currency.PARENT%%'"/>
          <xsl:with-param name="newValue" select="$pParentRowData/@currency"/>
        </xsl:call-template>
      </xsl:when>
      <!--%%DATA:currency%%-->
      <xsl:when test="contains($pValue,'%%DATA:currency%%')">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DATA:currency%%'"/>
          <xsl:with-param name="newValue" select="$pRowData/@currency"/>
        </xsl:call-template>
      </xsl:when>
      <!--%%DATA:amount%%-->
      <xsl:when test="contains($pValue,'%%DATA:amount%%')">
        <xsl:variable name="vAmount">
          <xsl:if test="string-length($pRowData/@amount) > 0">
            <xsl:choose>
              <xsl:when test ="number($pRowData/@amount) >0">
                <xsl:value-of select="$pRowData/@amount"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'0'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:if>
        </xsl:variable>
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DATA:amount%%'"/>
          <xsl:with-param name="newValue" select="$vAmount"/>
        </xsl:call-template>
      </xsl:when>
      <!--%%DATA:crDr%%-->
      <xsl:when test="contains($pValue,'%%DATA:crDr%%')">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DATA:crDr%%'"/>
          <xsl:with-param name="newValue" select="$pRowData/@crDr"/>
        </xsl:call-template>
      </xsl:when>
      <!--%%DATA:exAmount%%-->
      <xsl:when test="contains($pValue,'%%DATA:exAmount%%')">
        <xsl:variable name="vExAmount">
          <xsl:if test="string-length($pRowData/@exAmount) > 0">
            <xsl:choose>
              <xsl:when test ="number($pRowData/@exAmount) >0">
                <xsl:value-of select="$pRowData/@exAmount"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'0'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:if>
        </xsl:variable>
        <xsl:call-template name="Replace">
          <xsl:with-param name="source" select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DATA:exAmount%%'"/>
          <xsl:with-param name="newValue" select="$vExAmount"/>
        </xsl:call-template>
      </xsl:when>
      <!--%%DATA:fxRate%%-->
      <!--<xsl:when test="contains($pValue,'%%DATA:fxRate%%')">
        <xsl:variable name="vFxRate">
          <xsl:if test="$pRowData/@fxRate">
            <xsl:value-of select="$pRowData/@fxRate"/>
          </xsl:if>
        </xsl:variable>
        <xsl:variable name="vFxRate">
          <xsl:choose>
            <xsl:when test="string-length($vFxRate) = 0 or $pParentRowData/@currency = $gReportingCurrency">
              <xsl:value-of select="''"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'TS-FxRateTitle'" />
              </xsl:call-template>
              <xsl:value-of select="concat($gcEspace,$gReportingCurrency,'/',$pParentRowData/@currency,':',$gcEspace)" />
              <xsl:choose>
                <xsl:when test="number($vFxRate) = number(-1)">
                  <xsl:call-template name="getSpheresTranslation">
                    <xsl:with-param name="pResourceName" select="'TS-FxRateNA'" />
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="format-number">
                    <xsl:with-param name="pAmount" select="number($vFxRate)" />
                    <xsl:with-param name="pAmountPattern" select="$numberPatternNoZero" />
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:call-template name="Replace">
          <xsl:with-param name="source" select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DATA:fxRate%%'"/>
          <xsl:with-param name="newValue" select="$vFxRate"/>
        </xsl:call-template>
      </xsl:when>-->

      <!--%%DATA:fxRate%%-->
      <xsl:when test="contains($pValue,'%%DATA:fxRate%%')">
        <xsl:variable name="vFxRate">
          <xsl:choose>
            <xsl:when test="$pRowData/@fxRate and number($pRowData/@fxRate) != number(-1)">
              <xsl:value-of select="$pRowData/@fxRate"/>
            </xsl:when>
            <xsl:when test="$pRowData/@lastFxRate and number($pRowData/@lastFxRate) != number(-1)">
              <xsl:value-of select="$pRowData/@lastFxRate"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="number(-1)"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:variable name="vDisplayFxRate">
          <xsl:choose>
            <xsl:when test="string-length($vFxRate) = 0 or $pParentRowData/@currency = $gReportingCurrency">
              <xsl:value-of select="''"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'TS-FxRateTitle'" />
              </xsl:call-template>
              <xsl:value-of select="concat($gcEspace,$gReportingCurrency,'/',$pParentRowData/@currency,':',$gcEspace)" />
              <xsl:choose>
                <xsl:when test="number($vFxRate) = number(-1)">
                  <xsl:call-template name="getSpheresTranslation">
                    <xsl:with-param name="pResourceName" select="'TS-FxRateNA'" />
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="format-number">
                    <xsl:with-param name="pAmount" select="number($vFxRate)" />
                    <xsl:with-param name="pAmountPattern" select="$numberPatternNoZero" />
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:call-template name="Replace">
          <xsl:with-param name="source" select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DATA:fxRate%%'"/>
          <xsl:with-param name="newValue" select="$vDisplayFxRate"/>
        </xsl:call-template>
      </xsl:when>

      <!--%%DATA:fxRateLastRes%%-->
      <xsl:when test="contains($pValue,'%%DATA:fxRateLastRes%%')">
        <xsl:variable name="vFxRate">
          <xsl:choose>
            <xsl:when test="$pRowData/@fxRate and number($pRowData/@fxRate) != number(-1)">
              <xsl:value-of select="$pRowData/@fxRate"/>
            </xsl:when>
            <xsl:when test="$pRowData/@lastFxRate and number($pRowData/@lastFxRate) != number(-1)">
              <xsl:value-of select="$pRowData/@lastFxRate"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="number(-1)"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:variable name="vDisplayRes">
          <xsl:choose>
            <xsl:when test="string-length($vFxRate) = 0 or $pParentRowData/@currency = $gReportingCurrency">
              <xsl:value-of select="''"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:if test="number($vFxRate) != number(-1) and number($vFxRate) = number($pRowData/@lastFxRate)">
                <xsl:text> (</xsl:text>
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'TS-FxRateLast'" />
                </xsl:call-template>
                <xsl:text>)</xsl:text>
              </xsl:if>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:call-template name="Replace">
          <xsl:with-param name="source" select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DATA:fxRateLastRes%%'"/>
          <xsl:with-param name="newValue" select="$vDisplayRes"/>
        </xsl:call-template>
      </xsl:when>

      <!--%%DATA:taxDetailRate%%-->
      <xsl:when test="contains($pValue,'%%DATA:taxDetailRate%%')">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source" select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DATA:taxDetailRate%%'"/>
          <xsl:with-param name="newValue" select="$pRowData/@taxDetailRate"/>
        </xsl:call-template>
      </xsl:when>
      <!--%%DATA:keyForSum%%-->
      <xsl:when test="contains($pValue,'%%DATA:keyForSum%%')">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source" select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DATA:keyForSum%%'"/>
          <xsl:with-param name="newValue" select="$pRowData/@keyForSum"/>
        </xsl:call-template>
      </xsl:when>
      <!--%%DATA:rowspan%%-->
      <xsl:when test="contains($pValue,'%%DATA:rowspan%%')">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source" select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DATA:rowspan%%'"/>
          <xsl:with-param name="newValue" select="$pRowData/@rowspan"/>
        </xsl:call-template>
      </xsl:when>

      <!-- 20120803 MF Ticket 18065 START -->

      <!--%%DATA:rowspanAmount%%-->
      <xsl:when test="contains($pValue,'%%DATA:rowspanAmount%%')">
        <xsl:variable name="vAmount">
          <xsl:if test="string-length($pRowData/@rowspanAmount) > 0">
            <xsl:choose>
              <xsl:when test ="number($pRowData/@rowspanAmount) >0">
                <xsl:value-of select="$pRowData/@rowspanAmount"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'0'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:if>
        </xsl:variable>
        <xsl:call-template name="Replace">
          <xsl:with-param name="source" select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DATA:rowspanAmount%%'"/>
          <xsl:with-param name="newValue" select="$vAmount"/>
        </xsl:call-template>
      </xsl:when>
      <!--%%DATA:rowspanMarketCss%%-->
      <xsl:when test="contains($pValue,'%%DATA:rowspanMarketCss%%')">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source" select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DATA:rowspanMarketCss%%'"/>
          <xsl:with-param name="newValue" select="$pRowData/@rowspanMarketCss"/>
        </xsl:call-template>
      </xsl:when>
      <!--%%DATA:nameMarket%%-->
      <xsl:when test="contains($pValue,'%%DATA:nameMarket%%')">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source" select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DATA:nameMarket%%'"/>
          <xsl:with-param name="newValue" select="$pRowData/@nameMarket"/>
        </xsl:call-template>
      </xsl:when>
      <!--%%DATA:nameDc%%-->
      <xsl:when test="contains($pValue,'%%DATA:nameDc%%')">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source" select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DATA:nameDc%%'"/>
          <xsl:with-param name="newValue" select="$pRowData/@nameDc"/>
        </xsl:call-template>
      </xsl:when>
      <!--%%DATA:nameCss%%-->
      <xsl:when test="contains($pValue,'%%DATA:nameCss%%')">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source" select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DATA:nameCss%%'"/>
          <xsl:with-param name="newValue" select="$pRowData/@nameCss"/>
        </xsl:call-template>
      </xsl:when>
      <!--%%DATA:hideEmptyCell%%-->
      <xsl:when test="contains($pValue,'%%DATA:hideEmptyCell%%')">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source" select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DATA:hideEmptyCell%%'"/>
          <xsl:with-param name="newValue" select="$pRowData/@hideEmptyCell"/>
        </xsl:call-template>
      </xsl:when>
      <!--%%DATA:hideAmountName%%-->
      <xsl:when test="contains($pValue,'%%DATA:hideAmountName%%')">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source" select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DATA:hideAmountName%%'"/>
          <xsl:with-param name="newValue" select="$pRowData/@hideAmountName"/>
        </xsl:call-template>
      </xsl:when>
      <!--%%DATA:hideAmountName%%-->
      <xsl:when test="contains($pValue,'%%DATA:hideMarketName%%')">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source" select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DATA:hideMarketName%%'"/>
          <xsl:with-param name="newValue" select="$pRowData/@hideMarketName"/>
        </xsl:call-template>
      </xsl:when>

      <!-- 20120803 MF Ticket 18065 END -->

      <!-- 20120803 MF Ticket 18065 Item 7 -->
      <xsl:when test="contains($pValue,'%%DATA:showTotal%%')">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source" select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DATA:showTotal%%'"/>
          <xsl:with-param name="newValue" select="$pRowData/@showTotal"/>
        </xsl:call-template>
      </xsl:when>

      <!-- 20120903 MF Ticket 18048 - START -->

      <!--%%DATA:cbCrDr%%-->
      <xsl:when test="contains($pValue,'%%DATA:cbCrDr%%')">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source" select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DATA:cbCrDr%%'"/>
          <xsl:with-param name="newValue" select="$pRowData/@cbCrDr"/>
        </xsl:call-template>
      </xsl:when>
      <!--%%DATA:cbDate%%-->
      <xsl:when test="contains($pValue,'%%DATA:cbDate%%')">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source" select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DATA:cbDate%%'"/>
          <xsl:with-param name="newValue" select="$pRowData/@cbDate"/>
        </xsl:call-template>
      </xsl:when>
      <!--%%DATA:cbAmount%%-->
      <xsl:when test="contains($pValue,'%%DATA:cbAmount%%')">
        <xsl:variable name="vAmount">
          <xsl:if test="string-length($pRowData/@cbAmount) > 0">
            <xsl:choose>
              <xsl:when test ="number($pRowData/@cbAmount) >0">
                <xsl:value-of select="$pRowData/@cbAmount"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'0'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:if>
        </xsl:variable>
        <xsl:call-template name="Replace">
          <xsl:with-param name="source" select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%DATA:cbAmount%%'"/>
          <xsl:with-param name="newValue" select="$vAmount"/>
        </xsl:call-template>
      </xsl:when>

      <!-- 20120903 MF Ticket 18048 - END -->

      <!--%%PAT:currency.PARENT%%-->
      <xsl:when test="contains($pValue,'%%PAT:currency.PARENT%%')">
        <xsl:variable name="vDecPatternCurrency">
          <xsl:choose>
            <xsl:when test="string-length($pParentRowData/@currencyPattern) >0">
              <xsl:value-of select="$pParentRowData/@currencyPattern"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="GetDecPattern">
                <xsl:with-param name="pRoundDir" select="$pParentRowData/@cuRoundDir"/>
                <xsl:with-param name="pRoundPrec" select="$pParentRowData/@cuRoundPrec"/>
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:call-template name="Replace">
          <xsl:with-param name="source" select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%PAT:currency.PARENT%%'"/>
          <xsl:with-param name="newValue" select="$vDecPatternCurrency"/>
        </xsl:call-template>
      </xsl:when>
      <!--%%PAT:RepCurrency%%-->
      <xsl:when test="contains($pValue,'%%PAT:RepCurrency%%')">
        <xsl:variable name="vDecPatternCurrency">
          <xsl:choose>
            <xsl:when test="string-length($pParentRowData/@exCurrencyPattern) >0">
              <xsl:value-of select="$pParentRowData/@exCurrencyPattern"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="GetDecPattern">
                <xsl:with-param name="pRoundDir" select="$pParentRowData/@repCuRoundDir"/>
                <xsl:with-param name="pRoundPrec" select="$pParentRowData/@repCuRoundPrec"/>
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:call-template name="Replace">
          <xsl:with-param name="source" select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%PAT:RepCurrency%%'"/>
          <xsl:with-param name="newValue" select="$vDecPatternCurrency"/>
        </xsl:call-template>
      </xsl:when>
      <!--%%PAT:2Dec%%-->
      <xsl:when test="contains($pValue,'%%PAT:2Dec%%')">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source" select="$pValue"/>
          <xsl:with-param name="oldValue" select="'%%PAT:2Dec%%'"/>
          <xsl:with-param name="newValue" select="$number2DecPattern"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pValue"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- ===== GetMarketDataToDisplay ===== -->
  <xsl:template name="GetMarketDataToDisplay">
    <xsl:param name="pMarket"/>
    <xsl:param name="pMarketRepository"/>
    <xsl:param name="pIsWithISO10383_ALPHA4" select="true()"/>

    <xsl:choose>
      <xsl:when test="$pMarketRepository">
        <xsl:choose>
          <xsl:when test="string-length($pMarketRepository/shortIdentifier) > 0 ">
            <xsl:value-of select="string($pMarketRepository/shortIdentifier)"/>
          </xsl:when>
          <xsl:when test="string-length($pMarketRepository/acronym) > 0 ">
            <xsl:value-of select="string($pMarketRepository/acronym)"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$pMarket"/>
          </xsl:otherwise>
        </xsl:choose>

        <xsl:if test="$pIsWithISO10383_ALPHA4 = true() and string-length($pMarketRepository/ISO10383_ALPHA4) > 0 ">
          <xsl:value-of select="concat(' - ',string($pMarketRepository/ISO10383_ALPHA4))"/>
        </xsl:if>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pMarket"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!-- ===== GetTradeNumDataToDisplay ===== -->
  <xsl:template name="GetTradeNumDataToDisplay">
    <xsl:param name="pExecID"/>
    <xsl:param name="pHref"/>

    <xsl:choose>
      <xsl:when test="($gReportHeaderFooter=false()) or $gReportHeaderFooter/tradeNumberIdent/text() = 'TradeId'">
        <!--Display "Spheres Trade Identifier"-->
        <xsl:value-of select="$pHref"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <!--Display "FIX Execution Id" if exist, otherwise "Spheres Trade Identifier"-->
          <xsl:when test="string-length($pExecID) > 0">
            <xsl:value-of select="$pExecID"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$pHref"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="GetColumnDataValue">
    <xsl:param name="pRowData" />
    <xsl:param name="pDataName" />
    <xsl:param name="pDataType" select="'string'" />
    <xsl:param name="pReturnValue" select="'value'" />

    <xsl:if test="$pRowData/data[@name=$pDataName]">
      <xsl:variable name="vDataValue">
        <xsl:choose>
          <xsl:when test="$pReturnValue='exValue'" >
            <xsl:value-of select="$pRowData/data[@name=$pDataName]/@exValue"/>
          </xsl:when>
          <xsl:when test="$pReturnValue='currency'" >
            <xsl:value-of select="$pRowData/data[@name=$pDataName]/@currency"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$pRowData/data[@name=$pDataName]/@value"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:if test="string-length($vDataValue) > 0">
        <xsl:choose>
          <xsl:when test="$pDataType='decimal'" >
            <xsl:call-template name="format-number">
              <xsl:with-param name="pAmount" select="$vDataValue" />
              <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$vDataValue" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <xsl:template name="GetExchangeRate_Repository">
    <xsl:param name="pFlowCcy"/>
    <xsl:param name="pExCcy" select="$gReportingCurrency"/>
    <xsl:param name="pFixingDate"/>
    <xsl:param name="pIsLastFxRate" select="false()"/>

    <xsl:variable name="idFxRate" select="concat('CURRENCY.ISO4217_ALPHA3.', $pFlowCcy)"/>
    <xsl:choose>
      <xsl:when test="$pFixingDate">
        <xsl:choose>
          <xsl:when test="$pIsLastFxRate = true()">
            <xsl:variable name="vAllFxRate">
              <xsl:for-each select="$gDataDocumentRepository/currency[@id=$idFxRate]/fxrate">
                <xsl:sort data-type="text" select="fixingDate/text()"/>

                <xsl:variable name="vCompare">
                  <xsl:call-template name="CompareDate">
                    <xsl:with-param name="pDate1" select="$pFixingDate"/>
                    <xsl:with-param name="pDate2" select="fixingDate/text()"/>
                  </xsl:call-template>
                </xsl:variable>
                <xsl:if test="$vCompare = 0 or $vCompare = 1">
                  <xsl:copy-of select="."/>
                </xsl:if>

              </xsl:for-each>
            </xsl:variable>
            <xsl:copy-of select="msxsl:node-set($vAllFxRate)/fxrate[last()]"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:copy-of select="$gDataDocumentRepository/currency[@id=$idFxRate]/fxrate[fixingDate/text() = $pFixingDate]"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:copy-of select="$gDataDocumentRepository/currency[@id=$idFxRate]/fxrate"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="DisplayCurrencyPair">
    <xsl:param name="pQuotedCurrencyPair"/>

    <xsl:variable name="vCcy1" select="$pQuotedCurrencyPair/currency1/text()"/>
    <xsl:variable name="vCcy2" select="$pQuotedCurrencyPair/currency2/text()"/>
    <xsl:variable name="vQuoteBasis" select="$pQuotedCurrencyPair/quoteBasis/text()"/>

    <xsl:choose>
      <xsl:when test="$vQuoteBasis='Currency1PerCurrency2'">
        <xsl:value-of select="concat($vCcy2,'/',$vCcy1)"/>
      </xsl:when>
      <xsl:when test="$vQuoteBasis='Currency2PerCurrency1'">
        <xsl:value-of select="concat($vCcy1,'/',$vCcy2)"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!--Prices Pattern -->
  <xsl:template name="GetPricesPattern">
    <xsl:param name="pPrices"/>
    <xsl:param name="pHighPrec" select="number(2)"/>

    <xsl:variable name="vPricesHighestPrec">
      <xsl:call-template name="GetHighestPrecision">
        <xsl:with-param name="pPrices" select="$pPrices"/>
        <xsl:with-param name="pHighPrec" select="$pHighPrec"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:call-template name="GetPricePattern">
      <xsl:with-param name="pPrec" select="$vPricesHighestPrec"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="GetHighestPrecision">
    <xsl:param name="pPrices"/>
    <xsl:param name="pHighPrec" select="number(2)"/>

    <xsl:choose>
      <xsl:when test="count($pPrices) > number(0)">
        <xsl:variable name="vLowestInteger" select="floor(number($pPrices[1]))"/>
        <xsl:variable name="vDecimalPortion" select="number($pPrices[1]) - $vLowestInteger"/>
        <xsl:variable name="vDecimalPortionString">
          <xsl:call-template name="format-number">
            <xsl:with-param name="pAmount" select="number($vDecimalPortion)" />
            <xsl:with-param name="pAmountPattern" select="$numberPatternNoZero" />
          </xsl:call-template>
        </xsl:variable>
        <xsl:variable name="vFirstPrec" select="string-length($vDecimalPortionString) - 2"/>

        <xsl:variable name="vHighPrec">
          <xsl:choose>
            <xsl:when test="$vFirstPrec > $pHighPrec">
              <xsl:value-of select="$vFirstPrec"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$pHighPrec"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:call-template name="GetHighestPrecision">
          <xsl:with-param name="pPrices" select="$pPrices[position() > 1]"/>
          <xsl:with-param name="pHighPrec" select="number($vHighPrec)"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pHighPrec"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!--RD 20171123 [23572] Load Bug: use pPrec instead pRoundPrec-->
  <xsl:template name="GetPricePattern">
    <xsl:param name="pPrec"/>

    <xsl:choose>
      <xsl:when test="number($pPrec) = 3">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$number2DecPattern"/>
          <xsl:with-param name="oldValue" select="'00'"/>
          <xsl:with-param name="newValue" select="'000'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="number($pPrec) = 4">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$number2DecPattern"/>
          <xsl:with-param name="oldValue" select="'00'"/>
          <xsl:with-param name="newValue" select="'0000'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="number($pPrec) = 5">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$number2DecPattern"/>
          <xsl:with-param name="oldValue" select="'00'"/>
          <xsl:with-param name="newValue" select="'00000'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="number($pPrec) = 6">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$number2DecPattern"/>
          <xsl:with-param name="oldValue" select="'00'"/>
          <xsl:with-param name="newValue" select="'000000'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="number($pPrec) = 7">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$number2DecPattern"/>
          <xsl:with-param name="oldValue" select="'00'"/>
          <xsl:with-param name="newValue" select="'0000000'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="number($pPrec) = 8">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$number2DecPattern"/>
          <xsl:with-param name="oldValue" select="'00'"/>
          <xsl:with-param name="newValue" select="'00000000'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="number($pPrec) = 9">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$number2DecPattern"/>
          <xsl:with-param name="oldValue" select="'00'"/>
          <xsl:with-param name="newValue" select="'000000000'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$number2DecPattern"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!-- ................................................ -->
  <!-- Tools                                            -->
  <!-- ................................................ -->
  <xsl:template name="GetFontSize">
    <xsl:param name="pFontSize"/>
    <xsl:param name="pUnit" select="'pt'"/>

    <xsl:choose>
      <xsl:when test="$pUnit = substring($pFontSize, string-length($pFontSize)- string-length($pUnit) +1)">
        <xsl:value-of select="$pFontSize"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="concat($pFontSize,$pUnit)" />
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>
</xsl:stylesheet>