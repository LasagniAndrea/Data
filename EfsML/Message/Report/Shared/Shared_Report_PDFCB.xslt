<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:dt="http://xsltsl.org/date-time"
  xmlns:fo="http://www.w3.org/1999/XSL/Format"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt">

  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml;"/>

  <!--  
================================================================================================================
                                                     HISTORY OF THE MODIFICATIONS
Version 1.0.0.0 (Spheres 2.5.0.0)
 Date: 17/09/2010
 Author: RD
 Description: first version  
================================================================================================================
  -->

  <!-- ================================================== -->
  <!--        xslt includes                               -->
  <!-- ================================================== -->
  <!--//-->
  <xsl:include href="Shared_Report_Main.xslt" />
  <xsl:include href="Shared_Report_BusinessCB.xslt" />

  <!-- ================================================== -->
  <!--        Parameters                                  -->
  <!-- ================================================== -->


  <!-- ================================================== -->
  <!--        Main Template                               -->
  <!-- ================================================== -->

  <!--============================================================================================================ -->
  <!--                                                                                                             -->
  <!--                                       Display Templates                                                     -->
  <!--                                                                                                             -->
  <!--============================================================================================================ -->
  <!--Affichage du trade dans le Header du report-->
  <xsl:template name="GetTradeHeader"/>
  <!--DisplayPageBody template-->
  <xsl:template name="DisplayPageBody">
    <xsl:param name="pBook" />
    <xsl:param name="pBookPosition" />
    <xsl:param name="pCountBook" />
    <xsl:param name="pGroups" />
    <xsl:param name="pIsDisplayStarLegend" />

    <xsl:call-template name="DisplayPageBodyCommon">
      <xsl:with-param name="pBook" select="$pBook" />
      <xsl:with-param name="pBookPosition" select="$pBookPosition" />
      <xsl:with-param name="pCountBook" select="$pCountBook" />
      <xsl:with-param name="pGroups" select="$pGroups" />
      <xsl:with-param name="pIsDisplayStarLegend" select="$pIsDisplayStarLegend"/>
    </xsl:call-template>

  </xsl:template>
  <!--DisplayPageBodyStart template-->
  <xsl:template name="DisplayPageBodyStart">
    <xsl:param name="pBook" />

    <xsl:call-template name="DisplayPageBodyStartCommon">
      <xsl:with-param name="pBook" select="$pBook"/>
    </xsl:call-template>

  </xsl:template>
  <!--DisplayReceiverDetailSpecific template-->
  <xsl:template name="DisplayReceiverDetailSpecific">
    <xsl:param name="pBook" />
    <xsl:call-template name="DisplayReceiverDetailCommon">
      <xsl:with-param name="pBook" select="$pBook" />
    </xsl:call-template>
  </xsl:template>
  <!--DisplayBookAdditionalDetail template-->
  <xsl:template name="DisplayBookAdditionalDetail">
    <xsl:call-template name="CommonDisplayBookAdditionalDetail" />
  </xsl:template>

  <xsl:template match="column" mode="display-getcolumn">
    <xsl:param name="pGroup"/>

    <xsl:call-template name="GetColumnToDisplay">
      <xsl:with-param name="pGroup" select="$pGroup" />
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="GetColumnDynamicData">
    <xsl:param name="pValue"/>
    <xsl:param name="pRowData"/>
    <xsl:param name="pParentRowData"/>

    <xsl:variable name="vValue">
      <xsl:call-template name="GetSpecificColumnDynamicData">
        <xsl:with-param name="pValue" select="$pValue"/>
        <xsl:with-param name="pRowData" select="$pRowData"/>
        <xsl:with-param name="pParentRowData" select="$pParentRowData"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:call-template name="GetComonColumnDynamicData">
      <xsl:with-param name="pValue" select="$vValue"/>
      <xsl:with-param name="pRowData" select="$pRowData"/>
      <xsl:with-param name="pParentRowData" select="$pParentRowData"/>
    </xsl:call-template>
  </xsl:template>

  <!--GetSpecificColumnDynamicData-->
  <xsl:template name="GetSpecificColumnDynamicData">
    <xsl:param name="pValue"/>
    <xsl:param name="pRowData"/>
    <xsl:param name="pParentRowData"/>

    <xsl:value-of select="$pValue"/>
  </xsl:template>

  <!-- ===== GetContractDataToDisplay ===== -->
  <xsl:template name="GetContractDataToDisplay"/>

  <!-- .................................. -->
  <!--   Other                            -->
  <!-- .................................. -->

  <!-- .................................. -->
  <!--   Common Column                    -->
  <!-- .................................. -->

</xsl:stylesheet>
