<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="yes" encoding="UTF-8" 
              indent="yes" 
              media-type="text/xml" />

  <xsl:variable name="vMyRow" select="iotask/iotaskdet/iooutput/file/row"/>

  <xsl:template match="/">
    <ClosingReopeningInstructions>
      <xsl:copy-of select="$vMyRow"/>
    </ClosingReopeningInstructions>
  </xsl:template>

</xsl:stylesheet>
