<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

  <!-- Provision -->
  <xsl:template name="MandatoryEarlyTerminationProvision">
    <keyGroup name="MandatoryEarlyTerminationProvision" key="MEP"/>
    <itemGroup>
      <occurence to="Unique" isOptional="true" field="efs_MandatoryEarlyTerminationDates">earlyTerminationMandatory</occurence>
      <key>
        <eventCode>EfsML.Enum.Tools.EventCodeFunc|Provision</eventCode>
        <eventType>EfsML.Enum.Tools.EventTypeFunc|MandatoryEarlyTerminationProvision</eventType>
      </key>
      <idStCalcul>[CALC]</idStCalcul>
      <startDate>MandatoryDate</startDate>
      <endDate>MandatoryDate</endDate>
      <subItem>
        <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
        <eventDate>AdjustedMandatoryDate</eventDate>
      </subItem>
    </itemGroup>

    <xsl:call-template name="Exercise">
      <xsl:with-param name="pType" select="'European'"/>
    </xsl:call-template>

  </xsl:template>

  <!-- Provision -->
  <xsl:template name="Provision">
    <xsl:param name="pProvision"/>

    <xsl:variable name="occurence">
      <xsl:choose>
        <xsl:when test = "$pProvision = 'OptionalEarlyTermination'">earlyTerminationOptional</xsl:when>
        <xsl:when test = "$pProvision = 'Cancelable'">cancelableProvision</xsl:when>
        <xsl:when test = "$pProvision = 'Extendible'">extendibleProvision</xsl:when>
        <xsl:when test = "$pProvision = 'StepUp'">stepUpProvision</xsl:when>
        <xsl:otherwise>None</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <keyGroup name="{$pProvision}Provision" key="{$pProvision}"/>
    <itemGroup>
      <occurence to="Unique" isOptional="true">
        <xsl:value-of select="$occurence"/>
      </occurence>
      <key>
        <eventCode>EfsML.Enum.Tools.EventCodeFunc|Provision</eventCode>
        <eventType>EfsML.Enum.Tools.EventTypeFunc|<xsl:value-of select="$pProvision"/>Provision</eventType>
      </key>
      <idStCalcul>[CALC]</idStCalcul>
      <subItem>
        <eventClass>EfsML.Enum.Tools.EventClassFunc|GroupLevel</eventClass>
        <eventDateReference hRef="AdjustedTradeDate"/>
      </subItem>
    </itemGroup>

    <xsl:call-template name="Exercise">
      <xsl:with-param name="pType" select="'American'"/>
    </xsl:call-template>
    <xsl:call-template name="Exercise">
      <xsl:with-param name="pType" select="'Bermuda'"/>
    </xsl:call-template>
    <xsl:call-template name="Exercise">
      <xsl:with-param name="pType" select="'European'"/>
    </xsl:call-template>
  </xsl:template>

</xsl:stylesheet>