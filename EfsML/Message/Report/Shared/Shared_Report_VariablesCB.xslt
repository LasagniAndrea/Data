<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
					xmlns:dt="http://xsltsl.org/date-time"
					xmlns:fo="http://www.w3.org/1999/XSL/Format"
					xmlns:msxsl="urn:schemas-microsoft-com:xslt"
					version="1.0">

  <!--  
================================================================================================================
                                                     HISTORY OF THE MODIFICATIONS
Version 1.0.0.0 (Spheres 2.5.0.0)
 Date: 17/09/2010
 Author: RD
 Description: first version  
================================================================================================================
  -->

  <!-- ================================================== -->
  <!--        Global Constantes                           -->
  <!-- ================================================== -->
  <xsl:variable name="gDataDocumentRepository" select="//dataDocument/repository"/>
  <xsl:variable name="gBusinessBooks">
    <xsl:copy-of select="msxsl:node-set($gBusinessTrades)/trade/BOOK[(generate-id()=generate-id(key('kBookKey',@data)))]"/>
  </xsl:variable>

  <!--============================================================================================================ -->
  <!--                                                                                                             -->
  <!--                                         Global Variables                                                    -->
  <!--                                                                                                             -->
  <!--============================================================================================================ -->

  <!-- ===== Columns of common table settings===== -->
  <xsl:variable name="gDetTrdTableTotalWidth">197mm</xsl:variable>

  <!--RD 20130616 [18320] Correction du calcul du Solde-->
  <!--<xsl:variable name="gUseAvailableCash" select="normalize-space(./cashBalance/settings/useAvailableCash/text())"/>-->
  <xsl:variable name="gUseAvailableCash" select="'true'"/>
  <!-- ================================================== -->
  <!--   Statement page (A4 vertical) variables           -->
  <!-- ================================================== -->

  <!--Table Header-->


  <!--<xsl:variable name="gCBHdrFontSize">8</xsl:variable>-->

  <!-- .................................. -->
  <!--   page body: Columns of the table  -->
  <!-- .................................. -->

</xsl:stylesheet>

