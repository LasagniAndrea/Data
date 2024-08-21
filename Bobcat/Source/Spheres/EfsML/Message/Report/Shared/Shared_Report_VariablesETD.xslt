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
 
Version 1.0.0.1 (Spheres 2.6.3.0)
 Date: 26/06/2012
 Author: MF
 Description: removed gcFeeDisplaySettings , moved on Shared_Report_VariablesCommon.xslt
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
  <!--                                         Global Settings                                                    -->
  <!--                                                                                                             -->
  <!--============================================================================================================ -->
  <xsl:variable name="gHideBookForMonoBookReport" select="false()"/>

  <!--============================================================================================================ -->
  <!--                                                                                                             -->
  <!--                                         Global Variables                                                    -->
  <!--                                                                                                             -->
  <!--============================================================================================================ -->
  <xsl:variable name="gReportingFeeDisplay" select="$gDataDocumentHeader/reportingFeeDisplay"/>
  <xsl:variable name="gPaymentTypes" select="msxsl:node-set($gcFeeDisplaySettings)/rule[@name=$gReportingFeeDisplay]/paymentType"/>

</xsl:stylesheet>