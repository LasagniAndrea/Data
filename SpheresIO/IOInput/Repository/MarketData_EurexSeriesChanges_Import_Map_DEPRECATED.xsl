<!--
/*==============================================================*/
/* Summary : Import Eurex Referential                           */
/*==============================================================*/
/* File    : MarketData_EurexSeriesChanges_Import_Map.xsl       */
/* Version : v1.0.0.0                                           */
/* Date    : 20100127                                           */
/* Author  : MF                                                 */
/* Description: Import Eurex Referential File                   */
/*==============================================================*/
/* Revision:                                                    */
/*                                                              */
/* Date    : 20230726                                           */
/* Author  : PL                                                 */
/* Version : DEPRECATED                                    	    */
/* Comment : Unused                                             */
/*==============================================================*/
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- Includes-->
  <xsl:include href="MarketData_EurexCommon_Import_Map.xsl"/>

  <!--Main template  -->
  <xsl:template match="/iotask">
    <iotask>
      <xsl:call-template name="IOTaskAtt"/>
      <xsl:apply-templates select="parameters"/>
      <xsl:apply-templates select="iotaskdet"/>
    </iotask>
  </xsl:template>

  <!-- Specific template-->
  <xsl:template match="file">
    <file>
      <xsl:call-template name="IOFileAtt"/>

      <xsl:variable name="vTradingStartsDateTime">
        <xsl:value-of select="normalize-space(row[@status='success']/data[@name='TSDT'])"/>
      </xsl:variable>

      <xsl:apply-templates select="row">
        <xsl:with-param name="pTradingStartsDateTime" select="$vTradingStartsDateTime"/>
      </xsl:apply-templates>

    </file>
  </xsl:template>

  <xsl:template match="row">
    <xsl:param name="pTradingStartsDateTime"/>

    <xsl:if test="data[contains(@status, 'success')] and data[contains(@name, 'MC')]">

      <row>
        <xsl:call-template name="IORowAtt"/>
        <xsl:call-template name="rowStream">
          <xsl:with-param name="pTradingStartsDateTime" select="$pTradingStartsDateTime"/>
        </xsl:call-template>
      </row>

    </xsl:if>

  </xsl:template>

  <!-- Spécifique à chaque Import -->
  <xsl:template name="rowStream">
    <xsl:param name="pTradingStartsDateTime"/>

    <xsl:call-template name="rowStreamCommon">
      <xsl:with-param name="pTradingStartsDateTime" select="$pTradingStartsDateTime"/>
      <xsl:with-param name="pIsSeriesChange" select="true()"/>
    </xsl:call-template>

  </xsl:template>

</xsl:stylesheet>
