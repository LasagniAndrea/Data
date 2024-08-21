<?xml version="1.0" encoding="UTF-8"?>
<!--  
=============================================================================================
Summary : Parsing du fichier Energy Identification Codes (EICs)
File    : EIC_Par.xsl
=============================================================================================
-->

<!--
Fichier de données disponible sur: 
  https://www.entsoe.eu/data/energy-identification-codes-eic/eic-code-lists/Pages/default.aspx
  https://www.entsoe.eu/fileadmin/user_upload/edi/library/eic/allocated-eic-codes.zip
-->

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
 xmlns:fo="http://www.w3.org/1999/XSL/Format"
 xmlns:eic="urn:iec62325.351:tc57wg16:451-n:eicdocument:1:0">
  
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <xsl:template match="/">
    <file>
      <!-- Copy the attributes of the node <file> -->
      <xsl:attribute name="name"/>
      <xsl:attribute name="folder"/>
      <xsl:attribute name="date"/>
      <xsl:attribute name="size"/>
      <xsl:attribute name="status">success</xsl:attribute>
      <xsl:apply-templates select="eic:EIC_MarketDocument"/>
    </file>
  </xsl:template>
  
  <xsl:template match="eic:EIC_MarketDocument">
    <xsl:apply-templates select="eic:EICCode_MarketDocument"/>
  </xsl:template>

  <xsl:template match="eic:EICCode_MarketDocument">
    <xsl:variable name="idprefix" select="'r'"/>
    <xsl:variable name="idposition" select="position()"/>
    <!-- Import only for Endpoint ('V') and Connection Point ('Z') functions code-->
    <xsl:if test="substring(eic:mRID,3,1)='V' or substring(eic:mRID,3,1)='Y' or substring(eic:mRID,3,1)='Z'">
      <row>
        <!-- Add the attributes of the node <row> -->
        <xsl:attribute name="id">
          <xsl:value-of select="$idprefix"/>
          <xsl:value-of select="$idposition"/>
        </xsl:attribute>
        <xsl:attribute name="src">
          <xsl:value-of select="$idposition"/>
        </xsl:attribute>
        <xsl:attribute name="status">success</xsl:attribute>
        
        <data name="mRID">
          <xsl:value-of select="eic:mRID"/>
        </data>
        <data name="longName">
          <xsl:value-of select="eic:long_Names.name"/>
        </data>
        <data name="display">
          <xsl:value-of select="eic:display_Names.name"/>
        </data>
        <data name="fnCode">
          <xsl:value-of select="substring(./eic:mRID,3,1)"/>
        </data>
        <data name="fnName">
          <xsl:value-of select="eic:Function_Names/eic:name"/>
        </data>
        <xsl:if test="normalize-space(eic:eICCode_MarketParticipant.streetAddress/eic:townDetail/eic:country)!=''">
          <!-- The country is not always filled -->
          <data name="country">
            <xsl:value-of select="eic:eICCode_MarketParticipant.streetAddress/eic:townDetail/eic:country"/>
          </data>
        </xsl:if>
        <xsl:if test="normalize-space(eic:lastRequest_DateAndOrTime.date)!=''">
          <data name="dtLastRequest" type="datetime">
            <xsl:value-of select="eic:lastRequest_DateAndOrTime.date"/>
          </data>
        </xsl:if>
        <xsl:if test="normalize-space(eic:deactivationRequested_DateAndOrTime.date)!=''">
          <data name="dtDeactivateRequest" type="datetime">
            <xsl:value-of select="eic:deactivationRequested_DateAndOrTime.date"/>
          </data>
        </xsl:if>
      </row>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>
