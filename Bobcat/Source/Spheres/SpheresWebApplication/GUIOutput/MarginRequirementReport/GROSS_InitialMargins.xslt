<?xml version="1.0" encoding="UTF-8" ?>

<xsl:stylesheet version="1.0"
                xmlns:xsl  ="http://www.w3.org/1999/XSL/Transform"
                xmlns:dt   ="http://xsltsl.org/date-time"
                xmlns:fo   ="http://www.w3.org/1999/XSL/Format"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:efs  ="http://www.efs.org/2007/EFSmL-3-0"
                xmlns:fixml="http://www.fixprotocol.org/FIXML-5-0-SP1"
                xmlns:fpml ="http://www.fpml.org/2007/FpML-4-4">

  <xsl:param name="pCurrentCulture" select="'fr-FR'"/>

  <!-- xslt includes -->
  <xsl:include href="..\..\Library\Resource.xslt" />
  <xsl:include href="..\..\Library\DtFunc.xslt" />
  <xsl:include href="..\..\Library\NbrFunc.xslt" />
  <xsl:include href="..\..\Library\xsltsl/date-time.xsl" />
  <xsl:include href="..\..\Library\StrFunc.xslt" />

  <!-- *************************************************** -->


  <!--! ================================================================================ -->
  <!--!                      Page characteristics variables                              -->
  <!--! ================================================================================ -->
  
  <!-- A4 Vertical characteristics  ================================================== -->
  <xsl:variable name="vPageA4VerticalPageHeight">29.7</xsl:variable>
  <xsl:variable name="vPageA4VerticalPageWidth">21</xsl:variable>
  <xsl:variable name="vPageA4VerticalMargin">0.5</xsl:variable>
  <!-- A4 Vertical Header  ====================================================== -->
  <xsl:variable name ="vPageA4VerticalHeaderExtent">
    <xsl:value-of select ="$vPageA4VerticalMargin + $vLogoHeight + ($vRowHeight * 6) "/>
  </xsl:variable>
  <xsl:variable name ="vPageA4VerticalHeaderExtentFirst">
    <xsl:value-of select ="$vPageA4VerticalMargin + ($vRowHeight * 9)"/>
  </xsl:variable>
  <xsl:variable name ="vPageA4VerticalHeaderExtentRest">
    <xsl:value-of select ="$vPageA4VerticalMargin + ($vRowHeight * 2)"/>
  </xsl:variable>
  <xsl:variable name ="vPageA4VerticalHeaderLeftMargin">1.5</xsl:variable>
  <!-- A4 Vertical Footer ====================================================== -->
  <xsl:variable name ="vPageA4VerticalFooterExtent">
    <xsl:value-of select ="$vPageA4VerticalMargin + ($vRowHeight * 2)"/>
  </xsl:variable>
  <xsl:variable name ="vPageA4VerticalFooterBorderBottom">0.8pt solid black</xsl:variable>
  <xsl:variable name ="vPageA4VerticalFooterFontSize">7pt</xsl:variable>
  <!-- A4 Vertical Body  ====================================================== -->
  <xsl:variable name ="vPageA4VerticalBodyLeftMargin">0</xsl:variable>
  <xsl:variable name ="vPageA4VerticalBodyRightMargin">0</xsl:variable>
  <xsl:variable name ="vPageA4VerticalBodyTopMargin">
    <xsl:value-of select ="$vPageA4VerticalHeaderExtent"/>
  </xsl:variable>
  <xsl:variable name ="vPageA4VerticalBodyBottomMargin">
    <xsl:value-of select ="$vPageA4VerticalFooterExtent+$vPageA4VerticalMargin"/>
  </xsl:variable>

  <!-- A4 Landscape characteristics  ================================================== -->
  <xsl:variable name="vPageA4LandscapePageHeight">21</xsl:variable>
  <xsl:variable name="vPageA4LandscapePageWidth">29.7</xsl:variable>
  <xsl:variable name="vPageA4LandscapeMargin">0.5</xsl:variable>
  <!-- A4 Landscape Header ====================================================== -->
  <xsl:variable name ="vPageA4LandscapeHeaderExtent">
    <xsl:value-of select ="$vPageA4LandscapeMargin + $vLogoHeight + ($vRowHeight * 6) "/>
  </xsl:variable>
  <xsl:variable name ="vPageA4LandscapeHeaderExtentFirst">
    <xsl:value-of select ="$vPageA4LandscapeMargin + ($vRowHeight * 9)"/>
  </xsl:variable>
  <xsl:variable name ="vPageA4LandscapeHeaderExtentRest">
    <xsl:value-of select ="$vPageA4LandscapeMargin + ($vRowHeight * 2)"/>
  </xsl:variable>
  <xsl:variable name ="vPageA4LandscapeHeaderLeftMargin">1.5</xsl:variable>
  <!-- A4 Landscape Footer ====================================================== -->
  <xsl:variable name ="vPageA4LandscapeFooterExtent">
    <xsl:value-of select ="$vPageA4LandscapeMargin + ($vRowHeight * 2)"/>
  </xsl:variable>
  <xsl:variable name ="vPageA4LandscapeFooterBorderBottom">0.8pt solid black</xsl:variable>
  <xsl:variable name ="vPageA4LandscapeFooterFontSize">7pt</xsl:variable>
  <!-- A4 Landscape Body ====================================================== -->
  <xsl:variable name ="vPageA4LandscapeBodyLeftMargin">0</xsl:variable>
  <xsl:variable name ="vPageA4LandscapeBodyRightMargin">0</xsl:variable>
  <xsl:variable name ="vPageA4LandscapeBodyTopMargin">
    <xsl:value-of select ="$vPageA4LandscapeHeaderExtent+$vPageA4LandscapeMargin"/>
  </xsl:variable>
  <xsl:variable name ="vPageA4LandscapeBodyBottomMargin">
    <xsl:value-of select ="$vPageA4LandscapeFooterExtent+$vPageA4LandscapeMargin"/>
  </xsl:variable>


  <!--! =================================================================================== -->
  <!--!                      Layout variables                                               -->
  <!--! =================================================================================== -->
  <xsl:variable name ="vIsA4VerticalReport" select ="false()"/>
  
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

  <xsl:variable name="vLogo">
    <xsl:value-of select="concat('sql(select top(1) IDENTIFIER, LOLOGO from dbo.ACTOR a inner join dbo.ACTORROLE ar on ar.IDA = a.IDA where IDROLEACTOR =', $vApos, $vEntity, $vApos,  ')')" />
  </xsl:variable>

  <xsl:variable name ="vLogoHeight">1.35</xsl:variable>

  <xsl:variable name ="vApos">'</xsl:variable>

  <xsl:variable name="vSpaceCharacter">&#160;</xsl:variable>

  <xsl:variable name ="vFillCharacter">-</xsl:variable>

  <xsl:variable name ="vPageWidth">
    <xsl:choose>
      <xsl:when test ="$vIsA4VerticalReport=true()">
        <xsl:value-of select ="$vPageA4VerticalPageWidth"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select ="$vPageA4LandscapePageWidth"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name ="vPageMargin">
    <xsl:choose>
      <xsl:when test ="$vIsA4VerticalReport=true()">
        <xsl:value-of select ="$vPageA4VerticalMargin"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select ="$vPageA4LandscapeMargin"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- font  -->
  <xsl:variable name="vFontFamily">sans-serif</xsl:variable> <!--(sans-serif/Helvetica/Courier/Times New Roman/New York/serif/Gill)-->
  <xsl:variable name="vFontSizeBody">7.5pt</xsl:variable>
  <xsl:variable name="vFontSizeLarge">14pt</xsl:variable>
  <xsl:variable name="vFontSizeMedium">12pt</xsl:variable>
  <xsl:variable name="vFontSizeSmall">9pt</xsl:variable>
  <xsl:variable name="vFontSizeFooter">7pt</xsl:variable>

  <xsl:variable name="vFontColorGrey">
    <xsl:text>#D7D6CB</xsl:text> 
  </xsl:variable>
  <xsl:variable name="vFontColorDarkGrey">
    <xsl:text>#A9A9A9</xsl:text>
  </xsl:variable>
  <xsl:variable name="vFontColorGreen">Green</xsl:variable>
  <xsl:variable name="vFontColorRed">Red</xsl:variable>

  <xsl:variable name ="vFontWeightBold">
    <xsl:text>bold</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vFontWeightNormal">
    <xsl:text>normal</xsl:text>
  </xsl:variable>

  <xsl:variable name ="vDarkBackgroundColor">
    <!-- DarkGrey -->
    <xsl:text>#A9A9A9</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLightBackgroundColor">
    <!-- LightGrey -->
    <xsl:text>#D7D6CB</xsl:text>
  </xsl:variable>

  <xsl:variable name ="vPadding">
    <xsl:text>0.05</xsl:text>
  </xsl:variable>

  <xsl:variable name="vDisplayAlignCenter">center</xsl:variable>
  <xsl:variable name="vDisplayAlignBefore">before</xsl:variable>
  <xsl:variable name="vDisplayAlignAfter">after</xsl:variable>

  <xsl:variable name="vTextAlignCenter">center</xsl:variable>
  <xsl:variable name="vTextAlignLeft">left</xsl:variable>
  <xsl:variable name="vTextAlignRight">right</xsl:variable>

  <xsl:variable name ="vRowHeight">0.5</xsl:variable>
  <xsl:variable name ="vRowHeightBreakline">0.4</xsl:variable>

  <xsl:variable name="vTableCellBorder">
    <xsl:text>0.2pt solid black</xsl:text>
  </xsl:variable>

  <xsl:variable name="vBorderCollapse">
    <xsl:text>separate</xsl:text>
  </xsl:variable>

  <xsl:variable name="vBorderSeparation">
    <xsl:text>0.5pt</xsl:text>
  </xsl:variable>
  
  <!--! =================================================================================== -->
  <!--!                      Label variables                                                -->
  <!--! =================================================================================== -->
  <xsl:variable name ="vLabelReportName">
    <xsl:call-template name="getSpheresTranslation">
      <xsl:with-param name="pResourceName" select="'OTC_VIEW_MARGINTRACK'"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:variable name ="vLabelSubTitle">

    <!--<xsl:with-param name="pResourceName" select="'GlobalInitialMargin_Title'"/>-->
    <xsl:variable name="vResourceValue">
      <xsl:call-template name="getSpheresTranslation">
        <xsl:with-param name="pResourceName" select="'ISGROSSMARGINING'"/>
      </xsl:call-template>
    </xsl:variable>
    
    <!--Gross Management|Net Management;Gross Management-->
    <xsl:variable name="vAvailableResources">
      <xsl:value-of select="substring-after($vResourceValue,'|')"/>
    </xsl:variable>

    <xsl:variable name="vGross">
      <xsl:value-of select="substring-after($vResourceValue,';')"/>
    </xsl:variable>

    <xsl:variable name="vNet">
      <xsl:value-of select="substring-before($vResourceValue,';')"/>
    </xsl:variable>

    <xsl:value-of select="$vGross"/>

  </xsl:variable>

  <xsl:variable name="vLabelClearingDate">
    <xsl:call-template name="getSpheresTranslation">
      <xsl:with-param name="pResourceName" select="'TS-ClearingDate'"/>
    </xsl:call-template>
    <xsl:text> : </xsl:text>
  </xsl:variable>

  <xsl:variable name ="vLabelAccount">
    <xsl:call-template name="getSpheresTranslation">
      <xsl:with-param name="pResourceName" select="'TS-Account'"/>
    </xsl:call-template>
    <xsl:text> : </xsl:text>
  </xsl:variable>

  <xsl:variable name ="vLabelTiming">
    <xsl:call-template name="getSpheresTranslation">
      <xsl:with-param name="pResourceName" select="'marginRequirement_timing'"/>
    </xsl:call-template>
    <xsl:text> : </xsl:text>
  </xsl:variable>

  <xsl:variable name ="vLabelClearingOrganization">
    <xsl:call-template name="getSpheresTranslation">
      <xsl:with-param name="pResourceName" select="'marginRequirement_clearingOrganizationPartyReference'"/>
    </xsl:call-template>
    <xsl:text> : </xsl:text>
  </xsl:variable>

  <xsl:variable name ="vLabelPayer">
    <xsl:call-template name="getSpheresTranslation">
      <xsl:with-param name="pResourceName" select="'marginRequirement_payment_payer'"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:variable name ="vLabelReceiver">
    <xsl:call-template name="getSpheresTranslation">
      <xsl:with-param name="pResourceName" select="'marginRequirement_payment_receiver'"/>
    </xsl:call-template>
  </xsl:variable>
  
  <xsl:variable name ="vLabelWeightingRatio">
    <xsl:call-template name="getSpheresTranslation">
      <xsl:with-param name="pResourceName" select="'WeightingRatio_Title'"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:variable name ="vLabelClearingCurrency">
    <xsl:call-template name="getSpheresTranslation">
      <xsl:with-param name="pResourceName" select="'Currency'"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:variable name ="vLabelMarginAmount">
    <xsl:call-template name="getSpheresTranslation">
      <xsl:with-param name="pResourceName" select="'marginRequirement_payment_paymentAmount_amount'"/>
    </xsl:call-template>
  </xsl:variable>


  <!--! =================================================================================== -->
  <!--!                      Business variables                                               -->
  <!--! =================================================================================== -->
  <!-- true is gross margin / false is net margin-->
  <xsl:variable name ="vIsGross">
    <xsl:value-of select ="efs:EfsML/efs:marginRequirementOffice/@isGross"/>
  </xsl:variable>

  <!-- this variable returns value from getTradeDate template (it is a Clearing date)-->
  <xsl:variable name ="vTradeDate">
    <xsl:call-template name ="getTradeDate"/>
  </xsl:variable>

  <!-- this variable returns value from getDateTime template -->
  <xsl:variable name ="vDateTime">
    <xsl:call-template name ="getDateTime"/>
  </xsl:variable>

  <!-- this variable returns value from getTiming template -->
  <xsl:variable name ="vTiming">
    <xsl:call-template name ="getTiming"/>
  </xsl:variable>

  <!--  -->
  <xsl:variable name ="vMarginPayer">
    <xsl:call-template name ="getMarginPayer"/>
  </xsl:variable>

  <!--  -->
  <xsl:variable name ="vMarginReceiver">
    <xsl:call-template name ="getMarginReceiver"/>
  </xsl:variable>

  <!-- this variable returns value from getBook template -->
  <xsl:variable name ="vBook">
    <xsl:call-template name ="getBook"/>
  </xsl:variable>

  <!-- this variable returns value from getBook (account) template -->
  <!-- not in use -->
  <!--<xsl:variable name ="vAccount">
    <xsl:call-template name ="getAccount"/>
  </xsl:variable>-->

  <!-- this variable returns value from getClearingOrganization template -->
  <xsl:variable name ="vClearingOrganization">
    <xsl:call-template name ="getClearingOrganization"/>
  </xsl:variable>

  <xsl:variable name ="vMarginRequirementCurrency">
    <xsl:call-template name ="getMarginCurrency">
      <xsl:with-param name ="pNode" select ="efs:EfsML/efs:marginRequirementOffice/efs:marginAmounts/efs:Money"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:variable name="vWeightingRatio">
    <xsl:call-template name="getWeightingRatio"/>
  </xsl:variable>

  <xsl:variable name="vMarginAmount">
    <xsl:call-template name="getMarginAmount"/>
  </xsl:variable>

  <!--! =================================================================================== -->
  <!--!                      Layout templates                                               -->
  <!--! =================================================================================== -->

  <xsl:template name ="setPageCharacteristics">

    <fo:layout-master-set>
      <!-- A4 VERTICAL -->
      <!-- Spheres layout report-->
      <!-- EG 20160404 Migration vs2013 -->
      <!--<fo:simple-page-master master-name="A4VerticalPage" page-height="{$vPageA4VerticalPageHeight}cm" page-width="{$vPageA4VerticalPageWidth}cm" margin="{$vPageA4VerticalMargin}cm">-->
        <!--<fo:region-body region-name="A4VerticalBody" background-color="white" margin-left="{$vPageA4VerticalBodyLeftMargin}cm" margin-top="{$vPageA4VerticalBodyTopMargin}cm" margin-bottom="{$vPageA4VerticalBodyBottomMargin}cm" />-->
      <fo:simple-page-master master-name="A4VerticalPage" page-height="{$vPageA4VerticalPageHeight}cm" page-width="{$vPageA4VerticalPageWidth}cm" margin-left="{$vPageA4VerticalMargin}cm"  margin-top="{$vPageA4VerticalMargin}cm"  margin-right="{$vPageA4VerticalMargin}cm"  margin-bottom="{$vPageA4VerticalMargin}cm">
        <fo:region-body region-name="A4VerticalBody" background-color="white" margin-left="{$vPageA4VerticalBodyLeftMargin}cm" margin-top="{$vPageA4VerticalBodyTopMargin}cm" margin-right="{$vPageA4VerticalBodyRightMargin}cm" margin-bottom="{$vPageA4VerticalBodyBottomMargin}cm" />
        <fo:region-before region-name="A4VerticalHeader" background-color="white" extent="{$vPageA4VerticalHeaderExtent}cm"  precedence="true" />
        <fo:region-after region-name="A4VerticalFooter" background-color="white" extent="{$vPageA4VerticalFooterExtent}cm" precedence="true" />
      </fo:simple-page-master>
      <!-- A4 LANDSCAPE -->
      <!-- Spheres layout report-->
      <!-- EG 20160404 Migration vs2013 -->
      <!--<fo:simple-page-master master-name="A4LandscapePage" page-height="{$vPageA4LandscapePageHeight}cm" page-width="{$vPageA4LandscapePageWidth}cm" margin="{$vPageA4LandscapeMargin}cm">-->
        <!--<fo:region-body region-name="A4LandscapeBody" background-color="white" margin-left="{$vPageA4LandscapeBodyLeftMargin}cm" margin-top="{$vPageA4LandscapeBodyTopMargin}cm" margin-bottom="{$vPageA4LandscapeBodyBottomMargin}cm" />-->
      <fo:simple-page-master master-name="A4LandscapePage" page-height="{$vPageA4LandscapePageHeight}cm" page-width="{$vPageA4LandscapePageWidth}cm" margin-left="{$vPageA4LandscapeMargin}cm" margin-top="{$vPageA4LandscapeMargin}cm" margin-right="{$vPageA4LandscapeMargin}cm" margin-bottom="{$vPageA4LandscapeMargin}cm">
        <fo:region-body region-name="A4LandscapeBody" background-color="white" margin-left="{$vPageA4LandscapeBodyLeftMargin}cm" margin-top="{$vPageA4LandscapeBodyTopMargin}cm" margin-right="{$vPageA4LandscapeBodyRightMargin}cm" margin-bottom="{$vPageA4LandscapeBodyBottomMargin}cm" />
        <fo:region-before region-name="A4LandscapeHeader" background-color="white" extent="{$vPageA4LandscapeHeaderExtent}cm"  precedence="true" />
        <fo:region-after region-name="A4LandscapeFooter" background-color="white" extent="{$vPageA4LandscapeFooterExtent}cm" precedence="true" />
      </fo:simple-page-master>
    </fo:layout-master-set>

  </xsl:template>

  <xsl:template match="/">
    <fo:root xmlns:fo="http://www.w3.org/1999/XSL/Format">

      <xsl:call-template name="setPageCharacteristics"/>

      <xsl:choose>
        <xsl:when test ="$vIsA4VerticalReport=true()">

          <fo:page-sequence master-reference="A4VerticalPage" initial-page-number="1">
            <fo:static-content flow-name="A4VerticalHeader">
              <xsl:call-template name="displayHeader"/>
            </fo:static-content>
            <fo:static-content flow-name="A4VerticalFooter">
              <xsl:call-template name="displayFooter"/>
            </fo:static-content>
            <fo:flow flow-name="A4VerticalBody">
              <xsl:call-template name="displayBody"/>
              <fo:block id="EndOfDoc" />
            </fo:flow>
          </fo:page-sequence>

        </xsl:when>
        <xsl:otherwise>

          <fo:page-sequence master-reference="A4LandscapePage" initial-page-number="1">
            <fo:static-content flow-name="A4LandscapeHeader">
              <xsl:call-template name="displayHeader"/>
            </fo:static-content>
            <fo:static-content flow-name="A4LandscapeFooter">
              <xsl:call-template name="displayFooter"/>
            </fo:static-content>
            <fo:flow flow-name="A4LandscapeBody">
              <xsl:call-template name="displayBody"/>
              <fo:block id="EndOfDoc" />
            </fo:flow>
          </fo:page-sequence>

        </xsl:otherwise>
      </xsl:choose>

    </fo:root>

  </xsl:template>

  <!-- *************************************************** -->
  <!-- 1 columns (for Landscape and Vertical page) -->
  <xsl:template name="create1Column">
    <!-- EG 20160404 Migration vs2013 -->
    <!--<fo:table-column column-width="100%" column-number="01"/>-->
    <fo:table-column column-width="proportional-column-width(100)" column-number="01"/>
  </xsl:template>

  <!-- *************************************************** -->
  <!-- 2 columns (for Landscape and Vertical page) -->
  <xsl:template name="create2Columns">
    <xsl:param name ="pColumnWidth1"/>
    <xsl:param name ="pColumnWidth2" select ="0"/>

    <xsl:variable name ="vColumnWidth1">
      <xsl:value-of select ="$pColumnWidth1"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth2">

      <xsl:choose>
        <xsl:when test ="$pColumnWidth2=0">
          <xsl:value-of select =" $vPageWidth - $vColumnWidth1 - ($vPageMargin * 2)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$pColumnWidth2"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <fo:table-column column-width="{$vColumnWidth1}cm" column-number="01" />
    <fo:table-column column-width="{$vColumnWidth2}cm" column-number="02" />
  </xsl:template>

  <!-- *************************************************** -->
  <!-- 3 columns (for Landscape and Vertical page)-->
  <xsl:template name="create3Columns">
    <xsl:param name ="pColumnWidth1" select ="0"/>
    <xsl:param name ="pColumnWidth2" select ="0"/>
    <xsl:param name ="pColumnWidth3" select ="0"/>

    <xsl:variable name ="vColumnWidth1">
      <xsl:value-of select ="$pColumnWidth1"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth2">
      <xsl:choose>
        <xsl:when test ="$pColumnWidth2=0">
          <xsl:value-of select =" $vPageWidth - $vColumnWidth1 - $pColumnWidth3 - ($vPageMargin * 2)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$pColumnWidth2"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth3">
      <xsl:choose>
        <xsl:when test ="$pColumnWidth3=0">
          <xsl:value-of select =" $vPageWidth - $vColumnWidth1 - $pColumnWidth2 - ($vPageMargin * 2)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$pColumnWidth3"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <fo:table-column column-width="{$vColumnWidth1}cm" column-number="01" />
    <fo:table-column column-width="{$vColumnWidth2}cm" column-number="02" />
    <fo:table-column column-width="{$vColumnWidth3}cm" column-number="03" />
  </xsl:template>

  <!-- *************************************************** -->
  <!-- 4 columns (for Landscape and Vertical page)-->
  <xsl:template name="create4Columns">
    <xsl:param name ="pColumnWidth1" select ="0"/>
    <xsl:param name ="pColumnWidth2" select ="0"/>
    <xsl:param name ="pColumnWidth3" select ="0"/>
    <xsl:param name ="pColumnWidth4" select ="0"/>

    <xsl:variable name ="vColumnWidth1">
      <xsl:value-of select ="$pColumnWidth1"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth2">
      <xsl:value-of select ="$pColumnWidth2"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth3">
      <xsl:value-of select ="$pColumnWidth3"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth4">
      <xsl:choose>
        <xsl:when test ="$pColumnWidth4=0">
          <xsl:value-of select =" $vPageWidth - $vColumnWidth1 -  $vColumnWidth2 - $vColumnWidth3 - ($vPageMargin * 2)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$pColumnWidth4"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <fo:table-column column-width="{$vColumnWidth1}cm" column-number="01" />
    <fo:table-column column-width="{$vColumnWidth2}cm" column-number="02" />
    <fo:table-column column-width="{$vColumnWidth3}cm" column-number="03" />
    <fo:table-column column-width="{$vColumnWidth4}cm" column-number="04" />
  </xsl:template>

  <!-- *************************************************** -->
  <!-- 5 columns (for Landscape and Vertical page)-->
  <xsl:template name="create5Columns">
    <xsl:param name ="pColumnWidth1" select ="0"/>
    <xsl:param name ="pColumnWidth2" select ="0"/>
    <xsl:param name ="pColumnWidth3" select ="0"/>
    <xsl:param name ="pColumnWidth4" select ="0"/>
    <xsl:param name ="pColumnWidth5" select ="0"/>

    <xsl:variable name ="vColumnWidth1">
      <xsl:value-of select="$pColumnWidth1"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth2">
      <xsl:value-of select="$pColumnWidth2"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth3">
      <xsl:choose>
        <!-- middle dynamic field -->
        <xsl:when test ="$pColumnWidth3=0 and $pColumnWidth4=0 and $pColumnWidth5=0">
          <xsl:value-of select =" $vPageWidth - ($vColumnWidth1 * 2) -  ($vColumnWidth2 * 2) - ($vPageMargin * 2)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$pColumnWidth3"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth4">
      <xsl:choose>
        <xsl:when test ="$pColumnWidth3=0 and $pColumnWidth4=0 and $pColumnWidth5=0">
          <!-- middle dynamic field (ColumnWidth4 = ColumnWidth2) -->
          <xsl:value-of select="$vColumnWidth2"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$pColumnWidth4"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth5">
      <xsl:choose>
        <!-- middle dynamic field (ColumnWidth5 = ColumnWidth1) -->
        <xsl:when test ="$pColumnWidth3=0 and $pColumnWidth4=0 and $pColumnWidth5=0">
          <xsl:value-of select="$pColumnWidth1"/>
        </xsl:when>
        <!--last dynamic field-->
        <!-- all others parameters must be valorized-->
        <xsl:when test ="$pColumnWidth3!=0 and $pColumnWidth4=0 and $pColumnWidth5=0">
          <xsl:value-of select =" $vPageWidth - $vColumnWidth1 - $vColumnWidth2 - $vColumnWidth3 - $vColumnWidth4 - ($vPageMargin * 2)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$pColumnWidth5"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <fo:table-column column-width="{$vColumnWidth1}cm" column-number="01" />
    <fo:table-column column-width="{$vColumnWidth2}cm" column-number="02" />
    <fo:table-column column-width="{$vColumnWidth3}cm" column-number="03" />
    <fo:table-column column-width="{$vColumnWidth4}cm" column-number="04" />
    <fo:table-column column-width="{$vColumnWidth5}cm" column-number="05" />
  </xsl:template>

  <!-- *************************************************** -->
  <!-- 6 columns (for Landscape and Vertical page)-->
  <xsl:template name="create6Columns">
    <xsl:param name ="pColumnWidth1" select ="0"/>
    <xsl:param name ="pColumnWidth2" select ="0"/>
    <xsl:param name ="pColumnWidth3" select ="0"/>
    <xsl:param name ="pColumnWidth4" select ="0"/>
    <xsl:param name ="pColumnWidth5" select ="0"/>
    <xsl:param name ="pColumnWidth6" select ="0"/>

    <xsl:variable name ="vColumnWidth1">
      <xsl:value-of select="$pColumnWidth1"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth2">
      <xsl:value-of select="$pColumnWidth2"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth3">
      <xsl:value-of select ="$pColumnWidth3"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth4">
      <xsl:value-of select ="$pColumnWidth4"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth5">
      <xsl:value-of select ="$pColumnWidth5"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth6">
      <xsl:choose>
        <xsl:when test ="$pColumnWidth6=0">
          <xsl:value-of select ="$vPageWidth - $vColumnWidth1 -  $vColumnWidth2 - $vColumnWidth3 - $vColumnWidth4 - $vColumnWidth5 - ($vPageMargin * 2)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$pColumnWidth6"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <fo:table-column column-width="{$vColumnWidth1}cm" column-number="01" />
    <fo:table-column column-width="{$vColumnWidth2}cm" column-number="02" />
    <fo:table-column column-width="{$vColumnWidth3}cm" column-number="03" />
    <fo:table-column column-width="{$vColumnWidth4}cm" column-number="04" />
    <fo:table-column column-width="{$vColumnWidth5}cm" column-number="05" />
    <fo:table-column column-width="{$vColumnWidth6}cm" column-number="06" />

  </xsl:template>
  
  <!-- *************************************************** -->
  <!-- 7 columns (for Landscape and Vertical page)-->
  <xsl:template name="create7Columns">
    <xsl:param name ="pColumnWidth1" select ="0"/>
    <xsl:param name ="pColumnWidth2" select ="0"/>
    <xsl:param name ="pColumnWidth3" select ="0"/>
    <xsl:param name ="pColumnWidth4" select ="0"/>
    <xsl:param name ="pColumnWidth5" select ="0"/>
    <xsl:param name ="pColumnWidth6" select ="0"/>
    <xsl:param name ="pColumnWidth7" select ="0"/>

    <xsl:variable name ="vColumnWidth1">
      <xsl:value-of select="$pColumnWidth1"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth2">
      <xsl:value-of select="$pColumnWidth2"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth3">
      <xsl:value-of select ="$pColumnWidth3"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth4">
      <xsl:value-of select ="$pColumnWidth4"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth5">
      <xsl:value-of select ="$pColumnWidth5"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth6">
      <xsl:value-of select ="$pColumnWidth6"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth7">
      <xsl:choose>
        <xsl:when test ="$pColumnWidth7=0">
          <xsl:value-of select ="$vPageWidth - $vColumnWidth1 -  $vColumnWidth2 - $vColumnWidth3 - $vColumnWidth4 - $vColumnWidth5 - $vColumnWidth6 - ($vPageMargin * 2)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$pColumnWidth7"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <fo:table-column column-width="{$vColumnWidth1}cm" column-number="01" />
    <fo:table-column column-width="{$vColumnWidth2}cm" column-number="02" />
    <fo:table-column column-width="{$vColumnWidth3}cm" column-number="03" />
    <fo:table-column column-width="{$vColumnWidth4}cm" column-number="04" />
    <fo:table-column column-width="{$vColumnWidth5}cm" column-number="05" />
    <fo:table-column column-width="{$vColumnWidth6}cm" column-number="06" />
    <fo:table-column column-width="{$vColumnWidth7}cm" column-number="07" />

  </xsl:template>

  <!-- *************************************************** -->
  <!-- 8 columns (for Landscape and Vertical page)-->
  <xsl:template name="create8Columns">
    <xsl:param name ="pColumnWidth1" select ="0"/>
    <xsl:param name ="pColumnWidth2" select ="0"/>
    <xsl:param name ="pColumnWidth3" select ="0"/>
    <xsl:param name ="pColumnWidth4" select ="0"/>
    <xsl:param name ="pColumnWidth5" select ="0"/>
    <xsl:param name ="pColumnWidth6" select ="0"/>
    <xsl:param name ="pColumnWidth7" select ="0"/>
    <xsl:param name ="pColumnWidth8" select ="0"/>


    <xsl:variable name ="vColumnWidth1">
      <xsl:value-of select ="$pColumnWidth1"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth2">
      <xsl:value-of select ="$pColumnWidth2"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth3">
      <xsl:value-of select ="$pColumnWidth3"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth4">
      <xsl:value-of select ="$pColumnWidth4"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth5">
      <xsl:value-of select ="$pColumnWidth5"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth6">
      <xsl:value-of select ="$pColumnWidth6"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth7">
      <xsl:value-of select ="$pColumnWidth7"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth8">
      <xsl:choose>
        <xsl:when test ="$pColumnWidth8=0">
          <xsl:value-of select ="$vPageWidth - $vColumnWidth1 -  $vColumnWidth2 - $vColumnWidth3 - $vColumnWidth4 - $vColumnWidth5 - $vColumnWidth6 - $vColumnWidth7 - ($vPageMargin * 2)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$pColumnWidth8"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <fo:table-column column-width="{$vColumnWidth1}cm" column-number="01" />
    <fo:table-column column-width="{$vColumnWidth2}cm" column-number="02" />
    <fo:table-column column-width="{$vColumnWidth3}cm" column-number="03" />
    <fo:table-column column-width="{$vColumnWidth4}cm" column-number="04" />
    <fo:table-column column-width="{$vColumnWidth5}cm" column-number="05" />
    <fo:table-column column-width="{$vColumnWidth6}cm" column-number="06" />
    <fo:table-column column-width="{$vColumnWidth7}cm" column-number="07" />
    <fo:table-column column-width="{$vColumnWidth8}cm" column-number="08" />
  </xsl:template>

  <!-- *************************************************** -->
  <!-- 9 columns (for Landscape and Vertical page)-->
  <xsl:template name="create9Columns">
    <xsl:param name ="pColumnWidth1" select ="0"/>
    <xsl:param name ="pColumnWidth2" select ="0"/>
    <xsl:param name ="pColumnWidth3" select ="0"/>
    <xsl:param name ="pColumnWidth4" select ="0"/>
    <xsl:param name ="pColumnWidth5" select ="0"/>
    <xsl:param name ="pColumnWidth6" select ="0"/>
    <xsl:param name ="pColumnWidth7" select ="0"/>
    <xsl:param name ="pColumnWidth8" select ="0"/>
    <xsl:param name ="pColumnWidth9" select ="0"/>

    <xsl:variable name ="vColumnWidth1">
      <xsl:value-of select ="$pColumnWidth1"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth2">
      <xsl:value-of select ="$pColumnWidth2"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth3">
      <xsl:value-of select ="$pColumnWidth3"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth4">
      <xsl:value-of select ="$pColumnWidth4"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth5">
      <xsl:value-of select ="$pColumnWidth5"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth6">
      <xsl:value-of select ="$pColumnWidth6"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth7">
      <xsl:value-of select ="$pColumnWidth7"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth8">
      <xsl:value-of select ="$pColumnWidth8"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth9">
      <xsl:choose>
        <xsl:when test ="$pColumnWidth9=0">
          <xsl:value-of select ="$vPageWidth - $vColumnWidth1 -  $vColumnWidth2 - $vColumnWidth3 - $vColumnWidth4 - $vColumnWidth5 - $vColumnWidth6 - $vColumnWidth7 - $vColumnWidth8 - ($vPageMargin * 2)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$pColumnWidth9"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <fo:table-column column-width="{$vColumnWidth1}cm" column-number="01" />
    <fo:table-column column-width="{$vColumnWidth2}cm" column-number="02" />
    <fo:table-column column-width="{$vColumnWidth3}cm" column-number="03" />
    <fo:table-column column-width="{$vColumnWidth4}cm" column-number="04" />
    <fo:table-column column-width="{$vColumnWidth5}cm" column-number="05" />
    <fo:table-column column-width="{$vColumnWidth6}cm" column-number="06" />
    <fo:table-column column-width="{$vColumnWidth7}cm" column-number="07" />
    <fo:table-column column-width="{$vColumnWidth8}cm" column-number="08" />
    <fo:table-column column-width="{$vColumnWidth9}cm" column-number="09" />
  </xsl:template>

  <!-- *************************************************** -->
  <!-- 11 columns (for Landscape and Vertical page)-->
  <xsl:template name="create11Columns">
    <xsl:param name ="pColumnWidth1" select ="0"/>
    <xsl:param name ="pColumnWidth2" select ="0"/>
    <xsl:param name ="pColumnWidth3" select ="0"/>
    <xsl:param name ="pColumnWidth4" select ="0"/>
    <xsl:param name ="pColumnWidth5" select ="0"/>
    <xsl:param name ="pColumnWidth6" select ="0"/>
    <xsl:param name ="pColumnWidth7" select ="0"/>
    <xsl:param name ="pColumnWidth8" select ="0"/>
    <xsl:param name ="pColumnWidth9" select ="0"/>
    <xsl:param name ="pColumnWidth10" select ="0"/>
    <xsl:param name ="pColumnWidth11" select ="0"/>

    <xsl:variable name ="vColumnWidth1">
      <xsl:value-of select ="$pColumnWidth1"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth2">
      <xsl:value-of select ="$pColumnWidth2"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth3">
      <xsl:value-of select ="$pColumnWidth3"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth4">
      <xsl:value-of select ="$pColumnWidth4"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth5">
      <xsl:value-of select ="$pColumnWidth5"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth6">
      <xsl:value-of select ="$pColumnWidth6"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth7">
      <xsl:value-of select ="$pColumnWidth7"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth8">
      <xsl:value-of select ="$pColumnWidth8"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth9">
      <xsl:value-of select ="$pColumnWidth9"/>
    </xsl:variable>
    <xsl:variable name ="vColumnWidth10">
      <xsl:value-of select ="$pColumnWidth10"/>
    </xsl:variable>

    <xsl:variable name ="vColumnWidth11">
      <xsl:choose>
        <xsl:when test ="$pColumnWidth11=0">
          <xsl:value-of select ="$vPageWidth - $vColumnWidth1 -  $vColumnWidth2 - $vColumnWidth3 - $vColumnWidth4 - $vColumnWidth5 - $vColumnWidth6 - $vColumnWidth7 - $vColumnWidth8 - $vColumnWidth9 - $vColumnWidth10 -($vPageMargin * 2)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$pColumnWidth11"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <fo:table-column column-width="{$vColumnWidth1}cm" column-number="01" />
    <fo:table-column column-width="{$vColumnWidth2}cm" column-number="02" />
    <fo:table-column column-width="{$vColumnWidth3}cm" column-number="03" />
    <fo:table-column column-width="{$vColumnWidth4}cm" column-number="04" />
    <fo:table-column column-width="{$vColumnWidth5}cm" column-number="05" />
    <fo:table-column column-width="{$vColumnWidth6}cm" column-number="06" />
    <fo:table-column column-width="{$vColumnWidth7}cm" column-number="07" />
    <fo:table-column column-width="{$vColumnWidth8}cm" column-number="08" />
    <fo:table-column column-width="{$vColumnWidth9}cm" column-number="09" />
    <fo:table-column column-width="{$vColumnWidth10}cm" column-number="10" />
    <fo:table-column column-width="{$vColumnWidth11}cm" column-number="11" />
  </xsl:template>

  <!-- *************************************************** -->
  <!-- it returns a string consisting of a given number of copies of a string concatenated together.-->
  <!-- pString = string to be concatenated -->
  <!-- pLoops =  number of copies -->
  <!-- this template replaces leader with custom content pattern syntax unhandled in Apache fop installed in Spheres -->
  <!--
  <fo:leader leader-length="100%" rule-style="solid" rule-thickness="1px" color="black" leader-pattern="use-content">
   <xsl:value-of select="$vFillCharacter"/>
  fo:leader>
  -->
  <xsl:template name="repeaterString">
    <xsl:param name="pString" select="''"/>
    <xsl:param name="pLoops" select="0"/>
    <xsl:if test="$pLoops > 0">
      <xsl:value-of select="$pString"/>
      <xsl:call-template name="repeaterString">
        <xsl:with-param name="pString" select="$pString"/>
        <xsl:with-param name="pLoops" select="$pLoops - 1"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name="displayBreakline">
    <fo:block linefeed-treatment="preserve" >
      <fo:table table-layout="fixed" width="100%">
        <xsl:call-template name ="create1Column"/>
        <fo:table-body>
          <fo:table-row height="{$vRowHeightBreakline}cm">
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}"/>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </fo:block>
  </xsl:template>

  <!-- draw horizontal line -->
  <xsl:template name ="drawHorizontalLine">
    <fo:table table-layout="fixed" width="100%">
      <xsl:call-template name ="create1Column"/>
      <fo:table-body>
        <fo:table-row height="{$vRowHeight}cm">
          <fo:table-cell border="{$vBorder}" display-align="center">
            <fo:block border="{$vBorder}" text-align="center">
              <fo:leader leader-length="100%" leader-pattern="rule" rule-style="solid" rule-thickness="1px" color="grey" />
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
      </fo:table-body>
    </fo:table>
  </xsl:template>

  <!-- draw table cell for body-->
  <xsl:template name ="drawTableCell">
    <xsl:param name ="pValue" select ="''"/>
    <xsl:param name ="pBorder" select ="$vBorder"/>  
    <xsl:param name ="pPadding" select ="$vPadding"/>
    <xsl:param name ="pDisplayAlign" select ="$vDisplayAlignCenter"/>
    <xsl:param name ="pBackgroundColor" select ="'transparent'"/>
    <xsl:param name ="pFontSize" select ="$vFontSizeBody"/>
    <xsl:param name ="pFontWeight" select ="'normal'"/>
    <xsl:param name ="pColor" select ="'black'"/>
    <xsl:param name ="pTextAlign" select ="$vTextAlignLeft"/>

    <fo:table-cell border="{$pBorder}" padding="{$pPadding}cm" display-align="{$pDisplayAlign}" background-color="{$pBackgroundColor}" >
      <fo:block font-size="{$pFontSize}" color="{$pColor}" text-align="{$pTextAlign}" font-weight="{$pFontWeight}" >
        <xsl:value-of select ="$pValue"/>
      </fo:block>
    </fo:table-cell>

  </xsl:template>

  <!-- *************************************************** -->
  <xsl:template name="displayHeader">

    <fo:block linefeed-treatment="preserve" font-family="{$vFontFamily}">
      <fo:table table-layout="fixed" width="100%">
        <xsl:call-template name ="create3Columns">
          <xsl:with-param name="pColumnWidth1" select ="7"/>
          <xsl:with-param name="pColumnWidth3" select ="7"/>
        </xsl:call-template>
        <fo:table-body>
          <fo:table-row height="{$vLogoHeight}cm">

            <fo:table-cell border="{$vBorder}" text-align="left">
              <fo:block border="{$vBorder}">
                <fo:external-graphic src="{$vLogo}" height="{$vLogoHeight}cm" />
              </fo:block>
            </fo:table-cell>

            <xsl:call-template name ="drawTableCell">
              <xsl:with-param name ="pValue" select ="$vLabelReportName"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSizeLarge"/>
              <xsl:with-param name ="pFontWeight" select ="$vFontWeightBold"/>
              <xsl:with-param name ="pTextAlign" select ="$vTextAlignCenter"/>
              <xsl:with-param name ="pDisplayAlign" select ="$vDisplayAlignAfter"/>
            </xsl:call-template>

          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </fo:block>

    <fo:block linefeed-treatment="preserve" font-family="{$vFontFamily}">
      <fo:table table-layout="fixed" width="100%">
        <xsl:call-template name ="create1Column"/>
        <fo:table-body>
          <fo:table-row height="{$vRowHeight}cm">
            <xsl:call-template name ="drawTableCell">
              <xsl:with-param name ="pValue" select ="$vLabelSubTitle"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSizeMedium"/>
              <xsl:with-param name ="pColor" select ="$vFontColorDarkGrey"/>              
              <xsl:with-param name ="pFontWeight" select ="$vFontWeightBold"/>
              <xsl:with-param name ="pTextAlign" select ="$vTextAlignCenter"/>
            </xsl:call-template>
          </fo:table-row>          
        </fo:table-body>
      </fo:table>
    </fo:block>

    <fo:block linefeed-treatment="preserve" font-family="{$vFontFamily}" >
      <!--<fo:table table-layout="fixed" width="100%" border-collapse="{$vBorderCollapse}" border-separation="{$vBorderSeparation}">-->
      <fo:table table-layout="fixed" width="100%">
        <xsl:call-template name ="create2Columns">
          <xsl:with-param name="pColumnWidth1" select ="4.5"/>
          <xsl:with-param name="pColumnWidth2" select ="8"/>      
        </xsl:call-template>
        
        <fo:table-body>
          
          <fo:table-row height="{$vRowHeight}cm">
            <xsl:call-template name ="drawTableCell">
              <xsl:with-param name ="pValue" select ="$vLabelClearingDate"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSizeSmall"/>
              <xsl:with-param name ="pFontWeight" select ="$vFontWeightBold"/>
              <xsl:with-param name ="pTextAlign" select ="$vTextAlignLeft"/>
            </xsl:call-template>

            <xsl:call-template name ="drawTableCell">
              <xsl:with-param name ="pValue" select ="$vTradeDate"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSizeSmall"/>
              <xsl:with-param name ="pFontWeight" select ="$vFontWeightBold"/>
              <xsl:with-param name ="pTextAlign" select ="$vTextAlignLeft"/>
            </xsl:call-template>      
          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">
            <xsl:call-template name ="drawTableCell">
              <xsl:with-param name ="pValue" select ="$vLabelTiming"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSizeSmall"/>
              <xsl:with-param name ="pFontWeight" select ="$vFontWeightBold"/>
              <xsl:with-param name ="pTextAlign" select ="$vTextAlignLeft"/>
            </xsl:call-template>

            <xsl:call-template name ="drawTableCell">
              <xsl:with-param name ="pValue" select ="$vTiming"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSizeSmall"/>
              <xsl:with-param name ="pFontWeight" select ="$vFontWeightBold"/>
              <xsl:with-param name ="pTextAlign" select ="$vTextAlignLeft"/>
            </xsl:call-template>        
          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">
            <xsl:call-template name ="drawTableCell">
              <xsl:with-param name ="pValue" select ="$vLabelClearingOrganization"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSizeSmall"/>
              <xsl:with-param name ="pFontWeight" select ="$vFontWeightBold"/>
              <xsl:with-param name ="pTextAlign" select ="$vTextAlignLeft"/>
            </xsl:call-template>

            <xsl:call-template name ="drawTableCell">
              <xsl:with-param name ="pValue" select ="$vClearingOrganization"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSizeSmall"/>
              <xsl:with-param name ="pFontWeight" select ="$vFontWeightBold"/>
              <xsl:with-param name ="pTextAlign" select ="$vTextAlignLeft"/>
            </xsl:call-template>
          </fo:table-row>

          <fo:table-row height="{$vRowHeight}cm">
            <xsl:call-template name ="drawTableCell">
              <xsl:with-param name ="pValue" select ="$vLabelAccount"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSizeSmall"/>
              <xsl:with-param name ="pFontWeight" select ="$vFontWeightBold"/>
              <xsl:with-param name ="pTextAlign" select ="$vTextAlignLeft"/>
            </xsl:call-template>

            <xsl:call-template name ="drawTableCell">
              <xsl:with-param name ="pValue" select ="$vBook"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSizeSmall"/>
              <xsl:with-param name ="pFontWeight" select ="$vFontWeightBold"/>
              <xsl:with-param name ="pTextAlign" select ="$vTextAlignLeft"/>
            </xsl:call-template>
          </fo:table-row>
          
        </fo:table-body>
      </fo:table>
    </fo:block>
    
    <xsl:call-template name ="drawHorizontalLine"/>

  </xsl:template>

  <!-- =========================================================  -->
  <!-- Footer section                                             -->
  <!-- =========================================================  -->

  <!-- Display footer  -->
  <xsl:template name="displayFooter">

   <xsl:call-template name ="drawHorizontalLine"/>

    <fo:block border="{$vBorder}" linefeed-treatment="preserve">
      <fo:table table-layout="fixed" width="100%">
        <xsl:call-template name ="create1Column"/>
        <fo:table-body>
          <fo:table-row height="{$vRowHeight}cm">
            <fo:table-cell border="{$vBorder}" display-align="{$vDisplayAlignCenter}">
              <fo:block border="{$vBorder}" text-align="{$vTextAlignRight}" font-family="{$vFontFamily}" font-size="{$vFontSizeFooter}" font-weight="{$vFontWeightBold}" color="{$vFontColorDarkGrey}">
                 <fo:page-number/>/<fo:page-number-citation ref-id="EndOfDoc"/>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </fo:block>

  </xsl:template>
  
  <!-- =========================================================  -->
  <!-- Body section                                               -->
  <!-- =========================================================  -->

  <xsl:template name="displayBody">

    <fo:block linefeed-treatment="preserve" font-family="{$vFontFamily}">
     
      <fo:table table-layout="fixed" width="100%">
        <xsl:call-template name ="create6Columns">
          <xsl:with-param name="pColumnWidth1" select ="2"/>
          <xsl:with-param name="pColumnWidth2" select ="6"/>
          <xsl:with-param name="pColumnWidth3" select ="6"/>
          <xsl:with-param name="pColumnWidth4" select ="5"/>
          <xsl:with-param name="pColumnWidth5" select ="2"/>
          <xsl:with-param name="pColumnWidth6" select ="4"/>          
        </xsl:call-template>
        <fo:table-body>
          
          <fo:table-row height="{$vRowHeight}cm">

            <!-- draw empty table cell-->
            <xsl:call-template name ="drawTableCell"/>
            
            <xsl:call-template name ="drawTableCell">
              <xsl:with-param name ="pValue" select ="$vLabelPayer"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSizeSmall"/>
              <xsl:with-param name ="pFontWeight" select ="$vFontWeightBold"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vLightBackgroundColor"/>
              <xsl:with-param name ="pTextAlign" select ="$vTextAlignCenter"/>
            </xsl:call-template>

            <xsl:call-template name ="drawTableCell">
              <xsl:with-param name ="pValue" select ="$vLabelReceiver"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSizeSmall"/>
              <xsl:with-param name ="pFontWeight" select ="$vFontWeightBold"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vLightBackgroundColor"/>
              <xsl:with-param name ="pTextAlign" select ="$vTextAlignCenter"/>
            </xsl:call-template>

            <xsl:call-template name ="drawTableCell">
              <xsl:with-param name ="pValue" select ="$vLabelWeightingRatio"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSizeSmall"/>
              <xsl:with-param name ="pFontWeight" select ="$vFontWeightBold"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vLightBackgroundColor"/>
              <xsl:with-param name ="pTextAlign" select ="$vTextAlignCenter"/>
            </xsl:call-template>
            
            <xsl:call-template name ="drawTableCell">
              <xsl:with-param name ="pValue" select ="$vLabelClearingCurrency"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSizeSmall"/>
              <xsl:with-param name ="pFontWeight" select ="$vFontWeightBold"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vLightBackgroundColor"/>
              <xsl:with-param name ="pTextAlign" select ="$vTextAlignCenter"/>
            </xsl:call-template>            

            <xsl:call-template name ="drawTableCell">
              <xsl:with-param name ="pValue" select ="$vLabelMarginAmount"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSizeSmall"/>
              <xsl:with-param name ="pFontWeight" select ="$vFontWeightBold"/>
              <xsl:with-param name ="pBackgroundColor" select ="$vLightBackgroundColor"/>
              <xsl:with-param name ="pTextAlign" select ="$vTextAlignCenter"/>
            </xsl:call-template>
            
          </fo:table-row>


          <fo:table-row height="{$vRowHeight}cm">
            
             <!-- draw empty table cell-->
            <xsl:call-template name ="drawTableCell"/>

            <xsl:call-template name ="drawTableCell">
              <xsl:with-param name ="pValue" select ="$vMarginPayer"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSizeBody"/>
              <xsl:with-param name ="pTextAlign" select ="$vTextAlignCenter"/>
            </xsl:call-template>

            <xsl:call-template name ="drawTableCell">
              <xsl:with-param name ="pValue" select ="$vMarginReceiver"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSizeBody"/>
              <xsl:with-param name ="pTextAlign" select ="$vTextAlignCenter"/>
            </xsl:call-template>

            <xsl:call-template name ="drawTableCell">
              <xsl:with-param name ="pValue" select ="$vWeightingRatio"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSizeBody"/>
              <xsl:with-param name ="pTextAlign" select ="$vTextAlignCenter"/>
            </xsl:call-template>

            <xsl:call-template name ="drawTableCell">
              <xsl:with-param name ="pValue" select ="$vMarginRequirementCurrency"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSizeBody"/>
              <xsl:with-param name ="pTextAlign" select ="$vTextAlignCenter"/>
            </xsl:call-template>

            <xsl:call-template name ="drawTableCell">
              <xsl:with-param name ="pValue" select ="$vMarginAmount"/>
              <xsl:with-param name ="pBorder" select ="$vTableCellBorder"/>
              <xsl:with-param name ="pFontSize" select ="$vFontSizeBody"/>
              <xsl:with-param name ="pTextAlign" select ="$vTextAlignRight"/>
            </xsl:call-template>

          </fo:table-row>

        </fo:table-body>
      </fo:table>
       
    </fo:block>

  </xsl:template>

  <!--! =================================================================================== -->
  <!--!   BUSINESS VARIABLES (PREFIX V) AND TEMPLATES(PREFIX GET)                 -->
  <!--! =================================================================================== -->

  <!-- return trade date -->
  <!-- when parameter pCurrentCulture= 'en-GB' it returns: 6 JUL 2001-->
  <!-- when parameter pCurrentCulture= 'it-IT' it returns: 6 Luglio 2001-->
  <xsl:template name ="getTradeDate">
    <xsl:variable name ="vTradedate">
      <xsl:call-template name="format-date">
        <xsl:with-param name="xsd-date-time" select="//efs:EfsML/efs:trade/efs:Trade/fpml:tradeHeader/fpml:tradeDate"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:value-of select="normalize-space($vTradedate)"/>
  </xsl:template>

  <!-- return today date time (eg: 6 JUL 2001 + 14:45)-->
  <xsl:template name ="getDateTime">
    <xsl:variable name ="vFormatDate">
      <xsl:call-template name ="getTradeDate"/>
    </xsl:variable>
    <xsl:variable name ="vFormatTime">
      <xsl:value-of select="//efs:EfsML/efs:trade/efs:Trade/fpml:tradeHeader/fpml:tradeDate/@timeStamp"/>
    </xsl:variable>
    <xsl:value-of select="concat (normalize-space($vFormatDate), ' ', $vFormatTime)"/>
  </xsl:template>

  <!--  -->
  <xsl:template name ="getMarginCurrency">
    <xsl:param name ="pNode"/>

    <xsl:choose>
      <xsl:when test ="$pNode/fpml:currency">
        <xsl:value-of select ="$pNode/fpml:currency"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select ="$pNode/@curr"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- return Weighting Factor -->
  <xsl:template name="getWeightingFactor">
    <xsl:value-of select="//efs:marginRequirementOffice/@wratio"/>
  </xsl:template>

  <!-- return Clearing Organization -->
  <!-- This version returns the BIC code (displayname not available into XML)   -->
  <xsl:template name ="getClearingOrganization">
    <xsl:value-of select ="efs:EfsML/efs:trade/efs:Trade/efs:marginRequirement/efs:clearingOrganizationPartyReference/@href"/>
  </xsl:template>

  <!-- return margin payer (Exchange member) -->
  <xsl:template name ="getMarginPayer">
    <xsl:value-of select ="efs:EfsML/efs:marginRequirementOffice/efs:payerPartyReference/@href"/>
  </xsl:template>

  <!-- returns margin receiver (Clearing member) -->
  <xsl:template name ="getMarginReceiver">
    <xsl:value-of select ="efs:EfsML/efs:marginRequirementOffice/efs:receiverPartyReference/@href"/>
  </xsl:template>

  <!-- returns Book of Exchange member (marginRequirementOffice) -->
  <xsl:template name ="getBook">
    <xsl:value-of select ="efs:EfsML/efs:marginRequirementOffice/efs:bookId/@bookName"/>
  </xsl:template>

  <!-- returns timing definition from Spheres resources -->
  <!-- (EOD)= End of Day / (ITD)= Intra-day / (SOD)= Start of Day -->
  <xsl:template name ="getTiming">
    
    <xsl:variable name ="vValue">
      <xsl:value-of select ="efs:EfsML/efs:trade/efs:Trade/efs:marginRequirement/efs:timing"/>      
    </xsl:variable>

    <xsl:call-template name="getSpheresTranslation">
      <xsl:with-param name="pResourceName" select="$vValue"/>
    </xsl:call-template>

  </xsl:template>

  <!-- returns weighting ratio -->
  <xsl:template name ="getWeightingRatio">
    <xsl:value-of select ="efs:EfsML/efs:marginRequirementOffice/@wratio"/>
    <!--<xsl:value-of select="//efs:marginRequirementOffice/@wratio"/>-->
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

  <!-- return amount value from marginRequirementOffice/marginAmounts node-->
  <xsl:template name ="getMarginAmount">

    <xsl:variable name ="vValue">
      <xsl:value-of select ="efs:EfsML/efs:marginRequirementOffice/efs:marginAmounts/efs:Money/fpml:amount"/>
    </xsl:variable>    
    
    <xsl:call-template name ="formatMoney">
      <xsl:with-param name ="pAmount" select ="$vValue"/>
    </xsl:call-template>
  </xsl:template>

</xsl:stylesheet>