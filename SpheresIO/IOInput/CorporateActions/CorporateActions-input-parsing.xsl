<?xml version="1.0" encoding="ISO-8859-1" ?>
<!--  FI 20130527 [] Spheres® filtre les données 
      seuls les enregistrements où normMsgFactory/buildingInfo est renseigné sont considérés
-->

<xsl:stylesheet
xmlns:tt="http://www.ecb.int/vocabulary/2002-08-01/eurofxref"
xmlns:gesmes="http://www.gesmes.org/xml/2002-08-01"
xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
version="1.0">
  
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8"
              indent="yes" media-type="text/xml"/>

  <xsl:template match="/">
    <file>
      <!-- Copy the attributes of the node <file> -->
      <xsl:attribute name="name"></xsl:attribute>
      <xsl:attribute name="folder"></xsl:attribute>
      <xsl:attribute name="date"></xsl:attribute>
      <xsl:attribute name="size"></xsl:attribute>
      <xsl:attribute name="status">success</xsl:attribute>

      <xsl:apply-templates select="CorporateActions"/>

    </file>
  </xsl:template>

  <xsl:template match="CorporateActions">
    <xsl:apply-templates select="row[data[@name='BUILDINFO']/ValueXML/normMsgFactory/buildingInfo]"/>
  </xsl:template>

  <xsl:template match="row">
    <row>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="src">
        <xsl:value-of select="@src"/>
      </xsl:attribute>
      
      

      <xsl:apply-templates select="data"/>
    </row>
  </xsl:template>

  <xsl:template match="data">
    <data>
      <xsl:attribute name="name">
        <xsl:value-of select="@name"/>
      </xsl:attribute>
      <xsl:attribute name="datatype">
        <xsl:value-of select="@datatype"/>
      </xsl:attribute>
      <xsl:if test="@datatype!='xml' and @datatype!='text'">
        <xsl:value-of disable-output-escaping="no" select="text()"/>
      </xsl:if>
      <xsl:if test="((@datatype='xml') and (@name='BUILDINFO'))">
        <ValueXML>
          <xsl:text disable-output-escaping="yes">&lt;![CDATA[</xsl:text>
          <xsl:copy-of select="ValueXML/*"/>
          <xsl:text disable-output-escaping="yes">]]&gt;</xsl:text>
        </ValueXML>
      </xsl:if>
    </data>
  </xsl:template>

</xsl:stylesheet>
