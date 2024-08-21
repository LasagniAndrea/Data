<?xml version="1.0" encoding="utf-8"?>

<!-- 
=========================================================================================================
 Summary   : - Import LIFFE Market Data
             - Import des DC du ICE FUTURES EUROPE - FINANCIAL PRODUCTS DIVISION(IFLL)       
 File      : MarketData_Liffe_Import_Map.xsl
=========================================================================================================
 Version   : v3.2.0.0                                              
 Date      : 20130314                                          
 Author(s) : BD & FL                                                 
 Comment   : Création
=========================================================================================================
 Version : v5.0.5807
 Date    : 20151125
 Author  : FL
 Comment : [21570] ICE-EU - Repository error : Row is rejected by controls
     - Pour cette importation il ne faut pas considérer les DC suivants, car il ne font pas partie
       du marché ICE FUTURES EUROPE - FINANCIAL PRODUCTS DIVISION(IFLL) (c'est une exeception)
	        EVL	(Euro Float 3 Month)
          GCL	(GBP Float 3 Month)
                  
=========================================================================================================Version : v4.5.5626
  Date    : 20150703
  Author  : FL
  Comment : [21164] ICE-EU - Repository error : Row is rejected by controls
     - Pour cette importation il ne faut pas considérer les DC suivants, car il ne font pas partie
       du marché ICE FUTURES EUROPE - FINANCIAL PRODUCTS DIVISION(IFLL) (c'est une exeception)
				  EUL	(FEUL-Euro Float 6 Mon)
		      EUX	(FEUX-Euro Fix Annual)
		      GBL	(FGBL-GBP Float 6 Mont)
		      GBX	(FGBX-GBP Fix 6 Month)
          
=========================================================================================================
-->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- ================================================== -->
  <!--        include(s)                                  -->
  <!-- ================================================== -->
  <xsl:include href="MarketData_LiffeCommon_Import_Map.xsl"/>

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

      <xsl:apply-templates select="file">
        <xsl:with-param name="pImportName" select="@name"/>
      </xsl:apply-templates>

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

      <!-- FL 20150703: [21164] ICE-EU - Repository error : Row is rejected by controls -->
      <!-- FL 20151125: [21570] ICE-EU - Repository error : Row is rejected by controls -->
      <!-- <xsl:apply-templates select="row"> -->
      <xsl:apply-templates select="row[contains(',EUL,EUX,GBL,GBX,EVL,GCL,',concat(',',data[@name='C'],','))=false()]">
        <xsl:with-param name="pImportName" select="$pImportName"/>
        <xsl:with-param name="pFileName" select="@name"/>
      </xsl:apply-templates>

    </file>

  </xsl:template>

  <!-- ================================================== -->
  <!--        <row>                                       -->
  <!-- ================================================== -->
  <xsl:template match="row">
    <xsl:param name="pImportName"/>
    <xsl:param name="pFileName"/>
    <row>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="src">
        <xsl:value-of select="@src"/>
      </xsl:attribute>

      <xsl:call-template name="rowStreamCommon_LIFFE">
        <xsl:with-param name="pExchangeSymbol" select="'L'"/>
      </xsl:call-template>
    </row>
  </xsl:template>

</xsl:stylesheet>