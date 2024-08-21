<?xml version="1.0" encoding="utf-8"?>

<!--
=============================================================================================
Summary : PosRequest input - BCS Gateway mapping
          It's based on Spheres standard mapping of PosRequest input and overrides some templates
          Some templates of this xsl are overrided by specific customer's xsl mapping files
          
File    : BCSPosRequestImport_Map.xsl
=============================================================================================
Version : v3.7.5158                                           
Date    : 20140324
Author  : RD
Comment : [19704] First version
=============================================================================================
  -->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
                xmlns:exsl="http://exslt.org/common" extension-element-prefixes="exsl">

  <xsl:import href="PosRequestImport_Map.xsl" />
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <xsl:include href="..\Common\BCSTools.xsl"/>

</xsl:stylesheet>