<?xml version="1.0" encoding="UTF-8" ?>
<!--
=============================================================================================
Summary : Financial flows aggregated by asset (FLOWSBYASSET_ALLOC)
          Financial Flows by Actor
File    : FinFlowsByActor-pdf.xsl
=============================================================================================
Version : v3.7.5155                                           
Date    : 20140417
Author  : RD
Comment : [18685] First version     
=============================================================================================
-->
<xsl:stylesheet
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  xmlns:dt="http://xsltsl.org/date-time"
  xmlns:fo="http://www.w3.org/1999/XSL/Format"
  version="1.0">

  <!-- xslt import -->
  <xsl:import href="FinFlows-pdf.xsl"/>

  <!-- ================================================================ -->
  <!-- FinFlowsByActor Report settings                                  -->
  <!-- ================================================================ -->
  <xsl:variable name="gIsByActor" select="true()"/>
</xsl:stylesheet>