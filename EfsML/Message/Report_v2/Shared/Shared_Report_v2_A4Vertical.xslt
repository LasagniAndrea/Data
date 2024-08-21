<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
					xmlns:dt="http://xsltsl.org/date-time"
					xmlns:fo="http://www.w3.org/1999/XSL/Format"
					xmlns:msxsl="urn:schemas-microsoft-com:xslt"
					version="1.0">

  <!--  
==================================================================================================================
Summary : Spheres report - Shared - Common variables for all reports          
File    : Shared_Report_v2_A4Vertical.xslt
==================================================================================================================
Version : v4.2.5358                                          
Date    : 20140905
Author  : RD
Comment : First version
==================================================================================================================
  -->

  <!-- 
  ================================================================================================================
                                                Settings
  ================================================================================================================
  -->
  <!--<xsl:include href="Shared_Report_v2_Tools.xslt" />-->

  <!-- 
  ================================================================================================================
                                                Variables  
  ================================================================================================================
  -->
  <!-- 
  ..........................................................................
                A4 vertical
  ..........................................................................
  -->
  <xsl:variable name="gA4Vertical-height">297mm</xsl:variable>
  <xsl:variable name="gA4Vertical-width">210mm</xsl:variable>

  <xsl:variable name="gA4Vertical-margin-top">5mm</xsl:variable>
  <xsl:variable name="gA4Vertical-margin-bottom">5mm</xsl:variable>
  <xsl:variable name="gA4Vertical-margin-left">5mm</xsl:variable>
  <xsl:variable name="gA4Vertical-margin-right">5mm</xsl:variable>
  <xsl:variable name="gA4Vertical-padding">1mm</xsl:variable>
  <xsl:variable name="gA4Vertical-border-width">0mm</xsl:variable>

  <!--Header-->
  <xsl:variable name="gA4VerticalHeader-extent-0">0mm</xsl:variable>
  <xsl:variable name="gA4VerticalHeader-extent-1">17mm</xsl:variable>
  <xsl:variable name="gA4VerticalHeader-extent-2">21mm</xsl:variable>

  <xsl:variable name="gA4VerticalHeader-width">200mm</xsl:variable>

  <xsl:variable name="gA4VerticalHeaderLeftWidth">70mm</xsl:variable>
  <xsl:variable name="gA4VerticalHeaderCenterWidth">60mm</xsl:variable>
  <xsl:variable name="gA4VerticalHeaderRightWidth">70mm</xsl:variable>

  <!--Footer-->
  <xsl:variable name="gA4VerticalFooter-extent-0">3mm</xsl:variable>
  <xsl:variable name="gA4VerticalFooter-extent-1">7mm</xsl:variable>
  <xsl:variable name="gA4VerticalFooter-extent-2">13mm</xsl:variable>
  <xsl:variable name="gA4VerticalFooter-extent-3">19mm</xsl:variable>
  <xsl:variable name="gA4VerticalFooter-extent-4">25mm</xsl:variable>

  <xsl:variable name="gA4VerticalFooter-width">200mm</xsl:variable>

  <xsl:variable name="gA4VerticalFooterLeftWidth">20mm</xsl:variable>
  <xsl:variable name="gA4VerticalFooterCenterWidth">160mm</xsl:variable>
  <xsl:variable name="gA4VerticalFooterRightWidth">20mm</xsl:variable>

  <xsl:variable name="gA4VerticalFooterLegendFontSize">5pt</xsl:variable>

  <!--Body-->
  <xsl:variable name="gA4VerticalBody-margin">0mm</xsl:variable>

  <xsl:variable name="gA4VerticalBody-margin-top-0">0mm</xsl:variable>
  <xsl:variable name="gA4VerticalBody-margin-top-1">20mm</xsl:variable>
  <xsl:variable name="gA4VerticalBody-margin-top-2">25mm</xsl:variable>

  <xsl:variable name="gA4VerticalBody-margin-bottom-0">6mm</xsl:variable>
  <xsl:variable name="gA4VerticalBody-margin-bottom-1">10mm</xsl:variable>
  <xsl:variable name="gA4VerticalBody-margin-bottom-2">16mm</xsl:variable>
  <xsl:variable name="gA4VerticalBody-margin-bottom-3">22mm</xsl:variable>
  <xsl:variable name="gA4VerticalBody-margin-bottom-4">28mm</xsl:variable>

  <!--Legend-->
  <xsl:variable name="gA4VerticalLegendEnumsByRow" select="6"/>

  <!-- 
  ..........................................................................
                Other
  ..........................................................................
  -->

  <!-- 
  ================================================================================================================
                                                Templates
  ================================================================================================================
  -->
  <!--
  ....................................
    A4Vertical
  ....................................
  Summary : 
    Define "A4Vertical" model
  ....................................
  Parameters:
	  None
  ....................................
  -->
  <xsl:template name="A4Vertical">

    <!--Get Page Header extent according to the Settings of the report-->
    <xsl:variable name="vA4VerticalHeader-extent">
      <xsl:call-template name="A4VerticalHeader-extent"/>
    </xsl:variable>
    <!--Get Page Footer extent according to the Settings of the report-->
    <xsl:variable name="vA4VerticalFooter-extent">
      <xsl:call-template name="A4VerticalFooter-extent"/>
    </xsl:variable>

    <!--Get Page Body margin-top according to the Settings of the report-->
    <xsl:variable name="vA4VerticalBody-margin-top">
      <xsl:call-template name="A4VerticalBody-margin-top"/>
    </xsl:variable>
    <!--Get Page Body margin-bottom according to the Settings of the report-->
    <xsl:variable name="vA4VerticalBody-margin-bottom">
      <xsl:call-template name="A4VerticalBody-margin-bottom"/>
    </xsl:variable>

    <fo:simple-page-master master-name="A4Vertical"
                           page-height="{$gA4Vertical-height}"
                           page-width="{$gA4Vertical-width}"
                           margin-left="{$gA4Vertical-margin-left}"
                           margin-right="{$gA4Vertical-margin-right}"
                           margin-bottom="{$gA4Vertical-margin-bottom}"
                           margin-top="{$gA4Vertical-margin-top}"
                           border-width="{$gA4Vertical-border-width}">

      <!--Top region (header)-->
      <fo:region-before region-name="A4VerticalHeader"
                        extent="{$vA4VerticalHeader-extent}"
                        border-width="{$gA4Vertical-border-width}"
                        display-align="before"
                        precedence="true">
        <xsl:call-template name="Debug_background-color-green"/>
      </fo:region-before>
      <!--Bottom region (footer)-->
      <fo:region-after region-name="A4VerticalFooter"
                       extent="{$vA4VerticalFooter-extent}"
                       border-width="{$gA4Vertical-border-width}"
                       display-align="after"
                       precedence="true">
        <xsl:call-template name="Debug_background-color-green"/>
      </fo:region-after>
      <!--Left region (left sidebar)-->
      <fo:region-start region-name="A4VerticalStart"
                       extent="{$gA4VerticalBody-margin}">
        <xsl:call-template name="Debug_background-color-red"/>
      </fo:region-start>
      <!--Right region (right sidebar)-->
      <fo:region-end region-name="A4VerticalEnd"
                     extent="{$gA4VerticalBody-margin}">
        <xsl:call-template name="Debug_background-color-red"/>
      </fo:region-end>
      <!--Body region-->
      <fo:region-body region-name="A4VerticalBody"
                      margin-left="{$gA4VerticalBody-margin}"
                      margin-right="{$gA4VerticalBody-margin}"
                      margin-bottom="{$vA4VerticalBody-margin-bottom}"
                      margin-top="{$vA4VerticalBody-margin-top}"
                      border-width="{$gA4Vertical-border-width}"
                      overflow="scroll">
        <xsl:call-template name="Debug_background-color-yellow"/>
        <xsl:call-template name="Debug_border-red"/>
      </fo:region-body>
    </fo:simple-page-master>
  </xsl:template>

  <!-- 
  ..........................................................................
                Header
  ..........................................................................
  -->
  <!--
  ....................................
    A4VerticalHeader-extent
  ....................................
  Summary : 
    Get A4Vertical Page Header extent according to the Settings of the report
  ....................................
  Parameters:
	  None
  ....................................
  -->
  <xsl:template name="A4VerticalHeader-extent">
    <xsl:choose>
      <xsl:when test="$gSettings-headerFooter=false() or $gIsHeaderNotNone=true()">
        <xsl:choose>
          <xsl:when test="$gIsHeaderLeftLogoAndCustom or $gIsHeaderCenterLogoAndCustom or $gIsHeaderRightLogoAndCustom">
            <xsl:value-of select="$gA4VerticalHeader-extent-2"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$gA4VerticalHeader-extent-1"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$gA4VerticalHeader-extent-0"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!--
  ....................................
    A4VerticalHeader
  ....................................
  Summary : 
    Display A4Vertical Page Header according to the Settings of the report
  ....................................
  Parameters:
	  None
  ....................................
  -->
  <xsl:template name="A4VerticalHeader">

    <xsl:if test="$gSettings-headerFooter=false() or $gIsHeaderNotNone=true()">

      <xsl:variable name="vRuleSize">
        <xsl:call-template name="HeaderFooterRuleSize">
          <xsl:with-param name="pRuleSize" select="$gSettings-headerFooter/hRuleSize/text()"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:variable name="vHeaderColor">
        <xsl:choose>
          <xsl:when test="string-length($gSettings-headerFooter/hColor/text())>0">
            <xsl:call-template name="Lower">
              <xsl:with-param name="source" select="$gSettings-headerFooter/hColor/text()"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$gHeaderText_Color"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <!-- Display logo, Entity and Published Date -->
      <fo:table table-layout="fixed"
                display-align="before"
                width="{$gA4VerticalHeader-width}"
                border="{$vRuleSize}"
                border-bottom-style="solid"
                font-size="{$gHeader_FontSize}"
                font-weight="normal"
                color="{$vHeaderColor}">

        <xsl:if test="string-length($gSettings-headerFooter/hRuleColor/text())>0">
          <xsl:attribute name="border-bottom-color">
            <xsl:call-template name="Lower">
              <xsl:with-param name="source" select="$gSettings-headerFooter/hRuleColor/text()"/>
            </xsl:call-template>
          </xsl:attribute>
        </xsl:if>

        <fo:table-column column-number="01" column-width="{$gA4VerticalHeaderLeftWidth}">
          <xsl:call-template name="Debug_border-red"/>
        </fo:table-column>
        <fo:table-column column-number="02" column-width="{$gA4VerticalHeaderCenterWidth}">
          <xsl:call-template name="Debug_border-red"/>
        </fo:table-column>
        <fo:table-column column-number="03" column-width="{$gA4VerticalHeaderRightWidth}">
          <xsl:call-template name="Debug_border-red"/>
        </fo:table-column>

        <fo:table-body>
          <fo:table-row>
            <!--hLAllPg-->
            <xsl:variable name="vHeaderLeftType">
              <xsl:choose>
                <xsl:when test="$gSettings-headerFooter">
                  <xsl:value-of select="$gSettings-headerFooter/hLAllPg/text()"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="'Logo'"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:variable name="vHeaderLeftCustom">
              <xsl:if test="$gSettings-headerFooter">
                <xsl:value-of select="$gSettings-headerFooter/hLAllPgCustom/text()"/>
              </xsl:if>
            </xsl:variable>

            <fo:table-cell text-align="left" display-align="after"
                           padding-bottom="{$gA4Vertical-padding}" padding-right="{$gA4Vertical-padding}">
              <xsl:if test="starts-with($vHeaderLeftType,'Published_Model') or starts-with($vHeaderLeftType,'LegalInfo_Model')">
                <xsl:attribute name="font-size">
                  <xsl:value-of select="$gHeaderDetail_FontSize"/>
                </xsl:attribute>
              </xsl:if>
              <!--RD 20170602 [xxxxx] Add parameter pParent_Width-->
              <xsl:call-template name="DisplayPageHeaderType">
                <xsl:with-param name="pType" select="$vHeaderLeftType" />
                <xsl:with-param name="pCustom" select="$vHeaderLeftCustom" />
                <xsl:with-param name="pParent_Width" select="$gA4VerticalHeaderLeftWidth" />
              </xsl:call-template>
            </fo:table-cell>

            <!--hCAllPg-->
            <xsl:variable name="vHeaderCenterType">
              <xsl:choose>
                <xsl:when test="$gSettings-headerFooter">
                  <xsl:value-of select="$gSettings-headerFooter/hCAllPg/text()"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="'CompanyName'"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:variable name="vHeaderCenterCustom">
              <xsl:if test="$gSettings-headerFooter">
                <xsl:value-of select="$gSettings-headerFooter/hCAllPgCustom/text()"/>
              </xsl:if>
            </xsl:variable>

            <fo:table-cell text-align="center" display-align="after"
                           padding-bottom="{$gA4Vertical-padding}">
              <xsl:if test="starts-with($vHeaderCenterType,'Published_Model') or starts-with($vHeaderCenterType,'LegalInfo_Model')">
                <xsl:attribute name="font-size">
                  <xsl:value-of select="$gHeaderDetail_FontSize"/>
                </xsl:attribute>
              </xsl:if>
              <!--RD 20170602 [xxxxx] Add parameter pParent_Width-->
              <xsl:call-template name="DisplayPageHeaderType">
                <xsl:with-param name="pType" select="$vHeaderCenterType" />
                <xsl:with-param name="pCustom" select="$vHeaderCenterCustom" />
                <xsl:with-param name="pParent_Width" select="$gA4VerticalHeaderCenterWidth" />
              </xsl:call-template>
            </fo:table-cell>

            <!--hRAllPg-->
            <!--<xsl:variable name="vHeadeRightType" select="'LegalInfo_Model4'"/>-->
            <xsl:variable name="vHeadeRightType">
              <xsl:choose>
                <xsl:when test="$gSettings-headerFooter">
                  <xsl:value-of select="$gSettings-headerFooter/hRAllPg/text()"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="'Published_Model4'"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:variable name="vHeaderRightCustom">
              <xsl:if test="$gSettings-headerFooter">
                <xsl:value-of select="$gSettings-headerFooter/hRAllPgCustom/text()"/>
              </xsl:if>
            </xsl:variable>

            <fo:table-cell text-align="right" display-align="after"
                           padding-bottom="{$gA4Vertical-padding}" padding-left="{$gA4Vertical-padding}">
              <xsl:if test="starts-with($vHeadeRightType,'Published_Model') or starts-with($vHeadeRightType,'LegalInfo_Model')">
                <xsl:attribute name="font-size">
                  <xsl:value-of select="$gHeaderDetail_FontSize"/>
                </xsl:attribute>
              </xsl:if>
              
              <!--RD 20170602 [xxxxx] Add parameter pParent_Width-->
              <xsl:call-template name="DisplayPageHeaderType">
                <xsl:with-param name="pType" select="$vHeadeRightType" />
                <xsl:with-param name="pCustom" select="$vHeaderRightCustom" />
                <xsl:with-param name="pParent_Width" select="$gA4VerticalHeaderRightWidth" />
              </xsl:call-template>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </xsl:if>
  </xsl:template>

  <!-- 
  ..........................................................................
                Footer
  ..........................................................................
  -->
  <!--
  ....................................
    A4VerticalFooter-extent
  ....................................
  Summary : 
    Get A4Vertical Page Footer extent according to the Settings of the report
  ....................................
  Parameters:
	  None
  ....................................
  -->
  <xsl:template name="A4VerticalFooter-extent">
    <xsl:choose>
      <xsl:when test="$gSettings-headerFooter=false() or $gIsFooterNotNone=true()">
        <xsl:choose>
          <xsl:when test="$gIsFooterSecondRow and $gIsFooterLegalInfo_Model3">
            <xsl:value-of select="$gA4VerticalFooter-extent-4"/>
          </xsl:when>
          <xsl:when test="($gIsFooterSecondRow and $gIsFooterLegalInfo_Model2) or ($gIsFooterSecondRow=false() and $gIsFooterLegalInfo_Model3) or $gIsFooterCustom=true()">
            <xsl:value-of select="$gA4VerticalFooter-extent-3"/>
          </xsl:when>
          <xsl:when test="($gIsFooterSecondRow and $gIsFooterLegalInfo_Model1) or ($gIsFooterSecondRow=false() and $gIsFooterLegalInfo_Model2)">
            <xsl:value-of select="$gA4VerticalFooter-extent-2"/>
          </xsl:when>
          <xsl:when test="($gIsFooterSecondRow=false() and $gIsFooterLegalInfo_Model1)">
            <xsl:value-of select="$gA4VerticalFooter-extent-1"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$gA4VerticalFooter-extent-2"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$gA4VerticalFooter-extent-0"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!--
  ....................................
    A4VerticalFooter
  ....................................
  Summary : 
    Display A4Vertical Page Footer according to the Settings of the report
  ....................................
  Parameters:
	  None
  ....................................
  -->
  <xsl:template name="A4VerticalFooter">

    <!--Copyright-->
    <fo:table table-layout="fixed"
              display-align="after"
              border-top-style="none"
              width="{$gA4VerticalFooter-width}">
      <fo:table-column column-number="01" column-width="{$gA4VerticalFooter-width}">
        <xsl:call-template name="Debug_border-red"/>
      </fo:table-column>
      <fo:table-body>
        <fo:table-row font-weight="normal">
          <fo:table-cell border="0pt" text-align="right" display-align="center" font-size="{$gA4VerticalFooterLegendFontSize}">
            <fo:block linefeed-treatment="preserve" >
              <xsl:value-of select="$gCopyright" />
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
      </fo:table-body>
    </fo:table>

    <xsl:if test="$gSettings-headerFooter=false() or $gIsFooterNotNone=true()">

      <xsl:variable name="vRuleSize">
        <xsl:call-template name="HeaderFooterRuleSize">
          <xsl:with-param name="pRuleSize" select="$gSettings-headerFooter/fRuleSize/text()"/>
        </xsl:call-template>
      </xsl:variable>

      <fo:table table-layout="fixed"
                display-align="after"
                border-top-style="solid"
                border="{$vRuleSize}"
                width="{$gA4VerticalFooter-width}"
                font-size="{$gFooterText_FontSize}"
                font-weight="normal"
                color="{$gFooterText_Color}">

        <xsl:if test="string-length($gSettings-headerFooter/fRuleColor/text())>0">
          <xsl:attribute name="border-top-color">
            <xsl:call-template name="Lower">
              <xsl:with-param name="source" select="$gSettings-headerFooter/fRuleColor/text()"/>
            </xsl:call-template>
          </xsl:attribute>
        </xsl:if>

        <fo:table-column column-number="01" column-width="{$gA4VerticalFooterLeftWidth}">
          <xsl:call-template name="Debug_border-red"/>
        </fo:table-column>
        <fo:table-column column-number="02" column-width="{$gA4VerticalFooterCenterWidth}">
          <xsl:call-template name="Debug_border-red"/>
        </fo:table-column>
        <fo:table-column column-number="03" column-width="{$gA4VerticalFooterRightWidth}">
          <xsl:call-template name="Debug_border-red"/>
        </fo:table-column>

        <fo:table-body>
          <fo:table-row >
            <!--fLAllPg-->
            <fo:table-cell text-align="left"
                           display-align="before"
                           padding-top="{$gA4Vertical-padding}" padding-right="{$gA4Vertical-padding}">

              <xsl:variable name="vFooterLeftType">
                <xsl:choose>
                  <xsl:when test="$gSettings-headerFooter">
                    <xsl:value-of select="$gSettings-headerFooter/fLAllPg/text()"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="'None'"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:variable>
              <xsl:variable name="vFooterLeftCustom">
                <xsl:if test="$gSettings-headerFooter">
                  <xsl:value-of select="$gSettings-headerFooter/fLAllPgCustom/text()"/>
                </xsl:if>
              </xsl:variable>
              <!--RD 20170602 [xxxxx] Add parameter pParent_Width-->
              <xsl:call-template name="DisplayPageFooterType">
                <xsl:with-param name="pType" select="$vFooterLeftType" />
                <xsl:with-param name="pCustom" select="$vFooterLeftCustom" />
                <xsl:with-param name="pParent_Width" select="$gA4VerticalFooterLeftWidth" />
              </xsl:call-template>
            </fo:table-cell>

            <!--fCAllPg-->
            <fo:table-cell text-align="center"
                           display-align="before"
                           padding-top="{$gA4Vertical-padding}">

              <xsl:variable name="vFooterCenterType">
                <xsl:choose>
                  <xsl:when test="$gSettings-headerFooter">
                    <xsl:value-of select="$gSettings-headerFooter/fCAllPg/text()"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="'LegalInfo_Model2'"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:variable>
              <xsl:variable name="vFooterCenterCustom">
                <xsl:if test="$gSettings-headerFooter">
                  <xsl:value-of select="$gSettings-headerFooter/fCAllPgCustom/text()"/>
                </xsl:if>
              </xsl:variable>
              <!--RD 20170602 [xxxxx] Add parameter pParent_Width-->
              <xsl:call-template name="DisplayPageFooterType">
                <xsl:with-param name="pType" select="$vFooterCenterType" />
                <xsl:with-param name="pCustom" select="$vFooterCenterCustom" />
                <xsl:with-param name="pParent_Width" select="$gA4VerticalFooterCenterWidth" />
              </xsl:call-template>
            </fo:table-cell>

            <!--fRAllPg-->
            <fo:table-cell text-align="right"
                           display-align="before"
                           padding-top="{$gA4Vertical-padding}" padding-left="{$gA4Vertical-padding}">

              <xsl:variable name="vFooterRightType">
                <xsl:choose>
                  <xsl:when test="$gSettings-headerFooter">
                    <xsl:value-of select="$gSettings-headerFooter/fRAllPg/text()"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="'PageNumber'"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:variable>
              <xsl:variable name="vFooterRightCustom">
                <xsl:if test="$gSettings-headerFooter">
                  <xsl:value-of select="$gSettings-headerFooter/fRAllPgCustom/text()"/>
                </xsl:if>
              </xsl:variable>
              <!--RD 20170602 [xxxxx] Add parameter pParent_Width-->
              <xsl:call-template name="DisplayPageFooterType">
                <xsl:with-param name="pType" select="$vFooterRightType" />
                <xsl:with-param name="pCustom" select="$vFooterRightCustom" />
                <xsl:with-param name="pParent_Width" select="$gA4VerticalFooterRightWidth" />
              </xsl:call-template>
            </fo:table-cell>
          </fo:table-row>

          <xsl:if test="$gIsFooterSecondRow">

            <xsl:variable name="vIsSpanCenterCustomAdd" select="$gIsFooterLeftCustomAdd=false() and $gIsFooterCenterCustomAdd=true() and $gIsFooterRightCustomAdd=false()"/>

            <fo:table-row font-size="{$gFooterTextCustom_FontSize}">
              <!--fLAllPg-->
              <xsl:if test="$vIsSpanCenterCustomAdd=false()">
                <xsl:choose>
                  <xsl:when test="$gIsFooterLeftCustomAdd">
                    <fo:table-cell text-align="left"
                                   display-align="before"
                                   padding-top="3mm" padding-right="{$gA4Vertical-padding}">
                      <!--RD 20170602 [xxxxx] Add parameter pParent_Width-->
                      <xsl:call-template name="DisplayPageFooterType">
                        <xsl:with-param name="pType" select="'Custom'" />
                        <xsl:with-param name="pCustom" select="$gSettings-headerFooter/fLAllPgCustom/text()" />
                        <xsl:with-param name="pParent_Width" select="$gA4VerticalFooterLeftWidth" />
                      </xsl:call-template>
                    </fo:table-cell>
                  </xsl:when>
                  <xsl:otherwise>
                    <fo:table-cell>
                      <xsl:call-template name="Debug_border-green"/>
                    </fo:table-cell>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:if>

              <!--fCAllPg-->
              <xsl:choose>
                <xsl:when test="$gIsFooterCenterCustomAdd">
                  <fo:table-cell text-align="center"
                                 display-align="before"
                                 padding-top="3mm">
                    <xsl:if test="$vIsSpanCenterCustomAdd=true()">
                      <xsl:attribute name="number-columns-spanned">3</xsl:attribute>
                    </xsl:if>
                    <!--RD 20170602 [xxxxx] Add parameter pParent_Width-->
                    <xsl:call-template name="DisplayPageFooterType">
                      <xsl:with-param name="pType" select="'Custom'" />
                      <xsl:with-param name="pCustom" select="$gSettings-headerFooter/fCAllPgCustom/text()" />
                      <xsl:with-param name="pParent_Width" select="$gA4VerticalFooterCenterWidth" />
                    </xsl:call-template>
                  </fo:table-cell>
                </xsl:when>
                <xsl:otherwise>
                  <fo:table-cell>
                    <xsl:call-template name="Debug_border-green"/>
                  </fo:table-cell>
                </xsl:otherwise>
              </xsl:choose>

              <!--fRAllPg-->
              <xsl:if test="$vIsSpanCenterCustomAdd=false()">
                <xsl:choose>
                  <xsl:when test="$gIsFooterRightCustomAdd">
                    <fo:table-cell text-align="right"
                                   display-align="before"
                                   padding-top="3mm" padding-left="{$gA4Vertical-padding}">
                      <!--RD 20170602 [xxxxx] Add parameter pParent_Width-->
                      <xsl:call-template name="DisplayPageFooterType">
                        <xsl:with-param name="pType" select="'Custom'" />
                        <xsl:with-param name="pCustom" select="$gSettings-headerFooter/fRAllPgCustom/text()" />
                        <xsl:with-param name="pParent_Width" select="$gA4VerticalFooterRightWidth" />
                      </xsl:call-template>
                    </fo:table-cell>
                  </xsl:when>
                  <xsl:otherwise>
                    <fo:table-cell>
                      <xsl:call-template name="Debug_border-green"/>
                    </fo:table-cell>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:if>
            </fo:table-row>
          </xsl:if>
        </fo:table-body>
      </fo:table>
    </xsl:if>
  </xsl:template>

  <!-- 
  ..........................................................................
                Body
  ..........................................................................
  -->
  <!--
  ....................................
    A4VerticalBody-margin-top
  ....................................
  Summary : 
    Get A4Vertical Page Body margin-top according to the Settings of the report
  ....................................
  Parameters:
	  None
  ....................................
  -->
  <xsl:template name="A4VerticalBody-margin-top">
    <xsl:choose>
      <xsl:when test="$gSettings-headerFooter=false() or $gIsHeaderNotNone=true()">
        <xsl:choose>
          <xsl:when test="$gIsHeaderLeftLogoAndCustom or $gIsHeaderCenterLogoAndCustom or $gIsHeaderRightLogoAndCustom">
            <xsl:value-of select="$gA4VerticalBody-margin-top-2"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$gA4VerticalBody-margin-top-1"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$gA4VerticalBody-margin-top-0"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!--
  ....................................
    A4VerticalBody-margin-bottom
  ....................................
  Summary : 
    Get A4Vertical Page Body margin-bottom according to the Settings of the report
  ....................................
  Parameters:
	  None
  ....................................
  -->
  <xsl:template name="A4VerticalBody-margin-bottom">
    <xsl:choose>
      <xsl:when test="$gSettings-headerFooter=false() or $gIsFooterNotNone=true()">
        <xsl:choose>
          <xsl:when test="$gIsFooterSecondRow and $gIsFooterLegalInfo_Model3">
            <xsl:value-of select="$gA4VerticalBody-margin-bottom-4"/>
          </xsl:when>
          <xsl:when test="($gIsFooterSecondRow and $gIsFooterLegalInfo_Model2) or ($gIsFooterSecondRow=false() and $gIsFooterLegalInfo_Model3) or $gIsFooterCustom=true()">
            <xsl:value-of select="$gA4VerticalBody-margin-bottom-3"/>
          </xsl:when>
          <xsl:when test="($gIsFooterSecondRow and $gIsFooterLegalInfo_Model1) or ($gIsFooterSecondRow=false() and $gIsFooterLegalInfo_Model2)">
            <xsl:value-of select="$gA4VerticalBody-margin-bottom-2"/>
          </xsl:when>
          <xsl:when test="($gIsFooterSecondRow=false() and $gIsFooterLegalInfo_Model1)">
            <xsl:value-of select="$gA4VerticalBody-margin-bottom-1"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$gA4VerticalBody-margin-bottom-2"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$gA4VerticalBody-margin-bottom-0"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 
  ..........................................................................
                Legend
  ..........................................................................
  -->
  <!--
  ....................................
    A4VerticalLegend
  ....................................
  Summary : 
    Display A4Vertical Legend
  ....................................
  Parameters:
	  pWithOrderType: display OrdTypeEnum
    pWithTradeType: display TrdTypeEnum
    pWithPriceType: display Price Type
  ....................................
  -->
  <xsl:template name="A4VerticalLegend">
    <xsl:param name="pWithOrderType" select="false()"/>
    <xsl:param name="pWithTradeType" select="false()"/>
    <xsl:param name="pWithPriceType" select="false()"/>

    <xsl:if test="($gSettings-headerFooter=false()) or ($gSettings-headerFooter/fLegend/text()='LastPageOnly')">
      <fo:block font-size="{$gLegend_font-size}" font-weight="{$gLegend_font-weight}" text-align="{$gLegend_text-align}"
                space-before="{$gLegend_space-before}" display-align="after" linefeed-treatment="preserve">
        <xsl:call-template name="Debug_border-blue"/>

        <xsl:if test="$pWithOrderType">
          <xsl:call-template name="A4VerticalLegend_OrderType"/>
        </xsl:if>
        <xsl:if test="$pWithTradeType">
          <xsl:call-template name="A4VerticalLegend_TradeType"/>
        </xsl:if>
        <xsl:if test="$pWithPriceType">
          <xsl:call-template name="A4VerticalLegend_PriceType"/>
        </xsl:if>
      </fo:block>
    </xsl:if>
  </xsl:template>

  <!--
  ....................................
    A4VerticalLegend_OrderType
  ....................................
  Summary : 
    Display A4Vertical Legend: Order Type
  ....................................
  Parameters:
	  None
  ....................................
  -->
  <xsl:template name="A4VerticalLegend_OrderType">

    <xsl:variable name="vRepositoryOrdTypeEnums" select="$gRepository/enums[@id='ENUMS.CODE.OrdTypeEnum']/enum"/>
    <xsl:if test="$vRepositoryOrdTypeEnums">
      <xsl:variable name="vOrdTypeEnums">
        <xsl:for-each select="$vRepositoryOrdTypeEnums">
          <xsl:sort select="extattrb/text()" data-type="text" />
          <xsl:sort select="value/text()" data-type="text" />
          <xsl:copy-of select="." />
        </xsl:for-each>
      </xsl:variable>
      <xsl:call-template name="A4VerticalLegend_Table">
        <xsl:with-param name="pEnums" select="msxsl:node-set($vOrdTypeEnums)/enum" />
        <xsl:with-param name="pEnumsResource" select="'TS-OrderTypeLong'" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
  <!--
  ....................................
    A4VerticalLegend_TradeType
  ....................................
  Summary : 
    Display A4Vertical Legend: Trade Type
  ....................................
  Parameters:
	  None
  ....................................
  -->
  <xsl:template name="A4VerticalLegend_TradeType">

    <xsl:variable name="vRepositoryTrdTypeEnums" select="$gRepository/enums[@id='ENUMS.CODE.TrdTypeEnum']/enum"/>
    <xsl:if test="$vRepositoryTrdTypeEnums">
      <xsl:variable name="vTrdTypeEnums">
        <xsl:for-each select="$vRepositoryTrdTypeEnums">
          <xsl:sort select="extattrb/text()" data-type="text" />
          <xsl:sort select="value/text()" data-type="number" />
          <xsl:copy-of select="." />
        </xsl:for-each>
      </xsl:variable>
      <xsl:call-template name="A4VerticalLegend_Table">
        <xsl:with-param name="pEnums" select="msxsl:node-set($vTrdTypeEnums)/enum" />
        <xsl:with-param name="pEnumsResource" select="'TS-TradeTypeLong'" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
  <!--
  ....................................
    A4VerticalLegend_PriceType
  ....................................
  Summary : 
    Display A4Vertical Legend: Price Type
      - Reference Price
      - Underlying price
  ....................................
  Parameters:
	  None
  ....................................
  -->
  <xsl:template name="A4VerticalLegend_PriceType">

    <xsl:variable name="vPriceTypeEnums">
      <!--Display "Reference Price"-->
      <xsl:if test="$gPosActions/posAction[@requestType='MOF']">
        <enum id="ENUM.VALUE.1">
          <value>(1)</value>
          <extvalue>
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'TS-PriceType1'" />
            </xsl:call-template>
          </extvalue>
        </enum>
      </xsl:if>
      <!--Display "Underlying price"-->
      <!-- RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)-->
      <xsl:if test="$gPosActions/posAction[contains(',EXE,AUTOEXE,ABN,NEX,NAS,AUTOABN,ASS,AUTOASS,',concat(',',@requestType,','))]">
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
      <xsl:call-template name="A4VerticalLegend_Table">
        <xsl:with-param name="pEnums" select="msxsl:node-set($vPriceTypeEnums)/enum" />
        <xsl:with-param name="pEnumsResource" select="'TS-PriceType'" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
  <!--
  ....................................
    A4VerticalLegend_Table
  ....................................
  Summary : 
    Display A4Vertical Legend: Table
  ....................................
  Parameters:
	  pEnums : //repository/enums
    pEnumsResource : Enum label
  ....................................
  -->
  <xsl:template name="A4VerticalLegend_Table">
    <xsl:param name="pEnums" />
    <xsl:param name="pEnumsResource" />

    <fo:table table-layout="fixed" display-align="center" border="{$gLegend_border}" padding="{$gLegendTable_padding}" space-after="{$gLegendTable_space-after}" >
      <fo:table-column column-number="01" column-width="2.90mm">
        <xsl:call-template name="Debug_border-red"/>
      </fo:table-column>
      <fo:table-column column-number="02" column-width="2.85mm">
        <xsl:call-template name="Debug_border-red"/>
      </fo:table-column>
      <fo:table-column column-number="03" column-width="2.85mm">
        <xsl:call-template name="Debug_border-red"/>
      </fo:table-column>
      <fo:table-column column-number="04" column-width="2.85mm">
        <xsl:call-template name="Debug_border-red"/>
      </fo:table-column>
      <fo:table-column column-number="05" column-width="2.85mm">
        <xsl:call-template name="Debug_border-red"/>
      </fo:table-column>
      <fo:table-column column-number="06" column-width="2.85mm">
        <xsl:call-template name="Debug_border-red"/>
      </fo:table-column>
      <fo:table-column column-number="07" column-width="2.85mm">
        <xsl:call-template name="Debug_border-red"/>
      </fo:table-column>

      <fo:table-body>
        <fo:table-row font-weight="normal">
          <fo:table-cell>
            <fo:block linefeed-treatment="preserve">
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="$pEnumsResource" />
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
          <xsl:for-each select="$pEnums[$gA4VerticalLegendEnumsByRow >= position()]">
            <xsl:call-template name="A4VerticalLegend_EnumsCell">
              <xsl:with-param name="pEnum" select="." />
            </xsl:call-template>
          </xsl:for-each>
        </fo:table-row>

        <xsl:variable name="vRowCount" select="round((count($pEnums) div number($gA4VerticalLegendEnumsByRow)) + 0.5)"/>

        <xsl:if test="$vRowCount > 0">
          <xsl:variable name="vRowCountToDisplay">
            <xsl:choose>
              <xsl:when test="count($pEnums) > ($vRowCount * number($gA4VerticalLegendEnumsByRow))">
                <xsl:value-of select="$vRowCount+1"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$vRowCount"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:if test="$vRowCountToDisplay > 1">
            <xsl:call-template name="A4VerticalLegend_EnumsRow">
              <xsl:with-param name="pEnums" select="$pEnums" />
              <xsl:with-param name="pRowCountToDisplay" select="$vRowCountToDisplay" />
              <xsl:with-param name="pRowNumber" select="number('2')" />
            </xsl:call-template>
          </xsl:if>
        </xsl:if>
      </fo:table-body>
    </fo:table>
  </xsl:template>
  <!--
  ....................................
    A4VerticalLegend_EnumsRow
  ....................................
  Summary : 
    Display A4Vertical Legend: Row
  ....................................
  Parameters:
	  pEnums : //repository/enums
    pRowCountToDisplay : total rows to display
    pRowNumber : current row number
  ....................................
  -->
  <xsl:template name="A4VerticalLegend_EnumsRow">
    <xsl:param name="pEnums" />
    <xsl:param name="pRowCountToDisplay" />
    <xsl:param name="pRowNumber" />

    <xsl:if test="$pRowCountToDisplay >= $pRowNumber">

      <fo:table-row font-weight="normal">
        <xsl:call-template name="A4VerticalLegend_EnumsCell"/>

        <xsl:for-each select="$pEnums[(number($pRowNumber) * number($gA4VerticalLegendEnumsByRow) >= position()) and (position() > (number($pRowNumber)-1) * $gA4VerticalLegendEnumsByRow)]">
          <xsl:call-template name="A4VerticalLegend_EnumsCell">
            <xsl:with-param name="pEnum" select="." />
          </xsl:call-template>
        </xsl:for-each>
      </fo:table-row>

      <xsl:call-template name="A4VerticalLegend_EnumsRow">
        <xsl:with-param name="pEnums" select="$pEnums" />
        <xsl:with-param name="pRowCountToDisplay" select="$pRowCountToDisplay" />
        <xsl:with-param name="pRowNumber" select="$pRowNumber+1" />
      </xsl:call-template>
    </xsl:if>

  </xsl:template>
  <!--
  ....................................
    A4VerticalLegend_EnumsCell
  ....................................
  Summary : 
    Display A4Vertical Legend: Cell
  ....................................
  Parameters:
	  pEnum : //repository/enums/.
  ....................................
  -->
  <xsl:template name="A4VerticalLegend_EnumsCell">
    <xsl:param name="pEnum" />

    <fo:table-cell>
      <fo:block linefeed-treatment="preserve">
        <xsl:if test="$pEnum">
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
          <xsl:value-of select="$gcSpace" />
          <xsl:value-of select="$gcSpace" />
          <xsl:value-of select="$pEnum/extvalue/text()" />
        </xsl:if>
      </fo:block>
    </fo:table-cell>
  </xsl:template>

</xsl:stylesheet>