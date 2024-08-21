<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:dt="http://xsltsl.org/date-time"
  xmlns:fo="http://www.w3.org/1999/XSL/Format"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt">

  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml-fo"/>

  <!--  
================================================================================================================
Summary : Spheres report - Shared - Main template for all reports
                    
File    : Shared_Report_Main.xslt
================================================================================================================
Version : v3.7.5155                                           
Date    : 20140220
Author  : RD
Comment : [19612] Add scheme name for "tradeId" and "bookId" XPath checking  
================================================================================================================
Version : v1.0.0.1 (Spheres 2.6.3.0)
Date    : 20120626
Author  : MF
Comment :  
 Enhancements
 - Grouped amounts, any group prefix by an header (using a row spanned table cell)
 - Subordinate tables, recongized details tables rendered differently
 - New display on screen for composed amounts using an available value together with an used one
 - Pagination, for each group we put a page break
 - ....
================================================================================================================
Version : v1.0.0.0 (Spheres 2.5.0.0)
Date    : 20100917
Author  : RD
Comment : First version
================================================================================================================    
  -->

  <!-- ================================================== -->
  <!--        xslt includes                               -->
  <!-- ================================================== -->
  <xsl:include href="..\..\..\Library\Resource.xslt" />
  <xsl:include href="..\..\..\Library\DtFunc.xslt" />
  <xsl:include href="..\..\..\Library\NbrFunc.xslt" />
  <xsl:include href="..\..\..\Library\StrFunc.xslt" />
  <xsl:include href="..\..\..\Library\xsltsl/date-time.xsl" />
  <!--//-->
  <xsl:include href="Shared_Report_BusinessCommon.xslt" />
  <xsl:include href="Shared_Report_PDFCommon.xslt" />

  <!-- ================================================== -->
  <!--        Main Template                               -->
  <!-- ================================================== -->
  <xsl:template name="Main">
    <fo:root xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <xsl:choose>
        <xsl:when test="$gcIsBusinessDebugMode=false()">
          <!--Create PDF Document-->
          <xsl:call-template name="SetPagesCaracteristics" />
          <xsl:call-template name="DisplayDocumentContent"/>
        </xsl:when>
        <xsl:when test="$gcIsBusinessDebugMode">
          <statement creationTimestamp="{$gCreationTimestamp}" clearingDate="{$gClearingDate}">
            <tradeSorting>
              <xsl:copy-of select="$gSortingKeys"/>
            </tradeSorting>
            <!--//-->
            <businessTrades>
              <xsl:copy-of select="msxsl:node-set($gBusinessTrades)/trade"/>
            </businessTrades>
            <!--//-->
            <xsl:choose>
              <xsl:when test="$gIsMonoBook = 'true'">
                <xsl:for-each select="msxsl:node-set($gBusinessBooks)/BOOK">
                  <xsl:sort select="@data" data-type="text"/>

                  <xsl:call-template name="CreateGroups">
                    <xsl:with-param name="pBook" select="@data" />
                  </xsl:call-template>
                </xsl:for-each>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name="CreateGroups"/>
              </xsl:otherwise>
            </xsl:choose>
          </statement>
        </xsl:when>
      </xsl:choose>
    </fo:root>
  </xsl:template>
</xsl:stylesheet>
