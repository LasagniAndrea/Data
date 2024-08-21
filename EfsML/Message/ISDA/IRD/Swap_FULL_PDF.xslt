<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:dt="http://xsltsl.org/date-time" xmlns:fo="http://www.w3.org/1999/XSL/Format" version="1.0">

  <!--
	==============================================================
	Summary : Swap FULL PDF
	==============================================================
	File    : Swap_FULL_PDF.xslt
	Date    : 10.06.2009
	Author  : Gianmario SERIO
	Version : 2.3.0.3_1
	Description:
	==============================================================
	Revision:
	Date    :
	Author  :
	Version :
	Comment :
	==============================================================
	-->
  <xsl:import href="Swap_ISDA_PDF.xslt"/>
  <!-- varConfirmationType
	     Admitted values: "Full", "ISDA" -->
  <xsl:variable name ="varConfirmationType">
    <xsl:text>Full</xsl:text>
  </xsl:variable>

</xsl:stylesheet>