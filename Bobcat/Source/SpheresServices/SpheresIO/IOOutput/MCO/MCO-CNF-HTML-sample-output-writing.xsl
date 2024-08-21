<?xml version="1.0" encoding="ISO-8859-1" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="html" omit-xml-declaration="yes" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1" />

  <xsl:variable name="vMyRow" select="iotask/iotaskdet/iooutput/file/row"/>

  <xsl:template match="/">
    <xsl:value-of disable-output-escaping="yes" select="$vMyRow/data[@name='LOCNFMSGTXT']"/>
  </xsl:template>
</xsl:stylesheet>
