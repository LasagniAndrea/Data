<?xml version="1.0" encoding="utf-8"?>

<!-- 
=============================================================================================
 Summary  : Import de la feuille "Individual Equity Options" du fichier Excel bclear.       
 File     : MarketData_ICE-EUEquityOptions_ICE-EUProduct_Import_Map.xsl
=============================================================================================
 Version  : v3.2.0.0                                              
 Date     : 20130314                                          
 File     : BD & FL                                                 
 Comment  : Création
=============================================================================================
-->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <!-- ================================================== -->
  <!--        import(s)                                   -->
  <!-- ================================================== -->
  <xsl:import href="MarketData_Common_SQL.xsl"/>
  <xsl:import href="MarketData_ICE-EUCommon_ICE-EUProduct_Import_Map.xsl"/>

  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- ================================================== -->
  <!--        <iotask>                                    -->
  <!-- ================================================== -->
  <xsl:template match="iotask">
    <iotask>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="name">
        <xsl:value-of select="@name"/>
      </xsl:attribute>
      <xsl:attribute name="displayname">
        <xsl:value-of select="@displayname"/>
      </xsl:attribute>
      <xsl:attribute name="loglevel">
        <xsl:value-of select="@loglevel"/>
      </xsl:attribute>
      <xsl:attribute name="commitmode">
        <xsl:value-of select="@commitmode"/>
      </xsl:attribute>
      <xsl:apply-templates select="pms"/>
      <xsl:apply-templates select="iotaskdet"/>
    </iotask>
  </xsl:template>

  <!-- ================================================== -->
  <!--        <parameters>                                -->
  <!-- ================================================== -->
  <xsl:template match="parameters">
    <parameters>
      <xsl:for-each select="parameter">
        <parameter>
          <xsl:attribute name="id">
            <xsl:value-of select="@id"/>
          </xsl:attribute>
          <xsl:attribute name="name">
            <xsl:value-of select="@name"/>
          </xsl:attribute>
          <xsl:attribute name="displayname">
            <xsl:value-of select="@displayname"/>
          </xsl:attribute>
          <xsl:attribute name="direction">
            <xsl:value-of select="@direction"/>
          </xsl:attribute>
          <xsl:attribute name="datatype">
            <xsl:value-of select="@datatype"/>
          </xsl:attribute>
          <xsl:value-of select="."/>
        </parameter>
      </xsl:for-each>
    </parameters>
  </xsl:template>

  <!-- ================================================== -->
  <!--        <iotaskdet>                                 -->
  <!-- ================================================== -->
  <xsl:template match="iotaskdet">
    <iotaskdet>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="loglevel">
        <xsl:value-of select="@loglevel"/>
      </xsl:attribute>
      <xsl:attribute name="commitmode">
        <xsl:value-of select="@commitmode"/>
      </xsl:attribute>
      <xsl:apply-templates select="ioinput"/>
    </iotaskdet>
  </xsl:template>

  <!-- ================================================== -->
  <!--        <ioinput>                                   -->
  <!-- ================================================== -->
  <xsl:template match="ioinput">
    <ioinput>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="name">
        <xsl:value-of select="@name"/>
      </xsl:attribute>
      <xsl:attribute name="displayname">
        <xsl:value-of select="@displayname"/>
      </xsl:attribute>
      <xsl:attribute name="loglevel">
        <xsl:value-of select="@loglevel"/>
      </xsl:attribute>
      <xsl:attribute name="commitmode">
        <xsl:value-of select="@commitmode"/>
      </xsl:attribute>
      <xsl:apply-templates select="file" />
    </ioinput>
  </xsl:template>

  <!-- ================================================== -->
  <!--        <file>                                      -->
  <!-- ================================================== -->
  <xsl:template match="file">
    <xsl:param name="pImportName"/>
    <file>
      <xsl:attribute name="name">
        <xsl:value-of select="@name"/>
      </xsl:attribute>
      <xsl:attribute name="folder">
        <xsl:value-of select="@folder"/>
      </xsl:attribute>
      <xsl:attribute name="date">
        <xsl:value-of select="@date"/>
      </xsl:attribute>
      <xsl:attribute name="size">
        <xsl:value-of select="@size"/>
      </xsl:attribute>
      <xsl:apply-templates select="row" />
    </file>
  </xsl:template>

  <!-- ================================================== -->
  <!--        <row>                                       -->
  <!-- ================================================== -->
  <xsl:template match="row">

    <!-- On génére le <row> uniquement si AssetName ne débute pas par :
          "*The Exchange Delivery Settlement Price"
         Il s'agit de la dernière ligne de la feuille "Index Contracts" :
          "The Reuters Instrument Code for these contracts begins with ".dP" Preliminary Closing Index Value, and with ".dM" for the Official Closing Index Value " -->
    <xsl:if test="starts-with(data[@name='AssetName'],'The Reuters Instrument Code')=false()">

      <row>
        <xsl:attribute name="id">
          <xsl:value-of select="@id"/>
        </xsl:attribute>
        <xsl:attribute name="src">
          <xsl:value-of select="@src"/>
        </xsl:attribute>


        <xsl:call-template name="rowStreamCommon_ICE-EU">
          <xsl:with-param name="pSheetName" select="'IndexContracts'"/>
        </xsl:call-template>

      </row>

    </xsl:if>

  </xsl:template>

</xsl:stylesheet>