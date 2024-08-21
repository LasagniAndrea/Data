<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
                xmlns:exsl="http://exslt.org/common" extension-element-prefixes="exsl">

  <!--
=============================================================================================
Summary : Trade input - BCS Gateway mapping
          It's based on Spheres standard mapping of Trade input and overrides some templates
          Some templates of this xsl are overrided by specific customer's xsl mapping files
          
File    : BCSLSD_TradeImport_Map.xsl
=============================================================================================
Version : v3.7.5158                                           
Date    : 20140408
Author  : RD
Comment : [19816] First version
=============================================================================================
  -->

  <xsl:import href="LSD_TradeImport_Map.xsl" />
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml;"/>

  <!-- ================================================== -->
  <!--        include(s)                                  -->
  <!-- ================================================== -->
  <xsl:include href="..\..\Common\BCSTools.xsl"/>

</xsl:stylesheet>