<?xml version="1.0" encoding="UTF-8" ?>

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:dt="http://xsltsl.org/date-time" xmlns:fo="http://www.w3.org/1999/XSL/Format" version="1.0">

  <!--
	==============================================================
	Summary : Shared Variables 
	==============================================================
	Revision: 1
	Date    : 22.09.2009
	Author  : Gianmario SERIO
	Version : 2.3RTM_2
	Update  : Update variable gVarProductSectionPanelTextColor: change the product section panel text color [from royalblue to darkblue]
	          Add variable gVarAnnexSectionTextLongColumnWidthCM: it defines the width more great of the column  (section annex)
			  Add variable gVarAnnexSectionTextShortColumnWidthCM : it defines the width more small of the column (section annex)
		      Add variable gVarBoldFontWeidth
	          Add variable gVarAnnexSectionColumnLeftAlign
	          Add variable gVarAnnexSectionColumnRightAlign
	==============================================================	
	File    : Shared_Variables.xslt
	Date    : 20.05.2009
	Author  : Guido POERIO
	Version : 2.3.0.0_1
	==============================================================
	-->


  <!-- ======================================================================================================================= -->
  <!--                                                                                                                         -->
  <!--                                       BEGIN REGION: variables for all confirmations                                     -->
  <!--                                                                                                                         -->
  <!--                                                                                                                         -->
  <!-- ======================================================================================================================= -->

  <!-- ====================================================== -->
  <!-- Please, Add new variables in this section              -->
  <!-- ====================================================== -->
  <xsl:variable name="gVarBoldFontWeidth">bold</xsl:variable>
  <xsl:variable name="gVarAnnexSectionTextLongColumnWidthCM">5</xsl:variable>
  <xsl:variable name="gVarAnnexSectionTextShortColumnWidthCM">4</xsl:variable>
  <xsl:variable name="gVarAnnexSectionColumnLeftAlign">left</xsl:variable>
  <xsl:variable name="gVarAnnexSectionColumnRightAlign">right</xsl:variable>

  <!--Pony 20150513-->
  <xsl:variable name="gVarAnnexSectionTextLongColumnWidth5MM">0.5</xsl:variable>
  <xsl:variable name="gVarAnnexSectionTextLongColumnWidth15MM">1.5</xsl:variable>
  <xsl:variable name="gVarAnnexSectionTextLongColumnWidth100MM">10</xsl:variable>

  <!-- ========================================= -->
  <!-- Variables for page A4 dimensions          -->
  <!-- ========================================= -->
  <!-- PLEASE BE CAREFUL. MODIFY THIS REGION ONLY IF YOU WANT TO MODIFY A4 PAGE SIZE -->
  <xsl:variable name="gVarA4VerticalPageWidthCM">
    <xsl:value-of select='21'/>
  </xsl:variable>

  <!-- Page A4 width -->
  <xsl:variable name="gVarA4VerticalPageHeightCM">
    <xsl:value-of select='29.7'/>
  </xsl:variable>

  <!-- ========================================= -->
  <!-- Variables for fonts                       -->
  <!-- ========================================= -->
  <!-- Admitted values: Helvetica, Times, Courier -->
  <xsl:variable name="gVarFontFamily">Helvetica</xsl:variable>
  <xsl:variable name="gVarFontFamilyCourier">Courier</xsl:variable>

  <!-- ========================================= -->
  <!-- Variables for A4 page format              -->
  <!-- ========================================= -->
  <xsl:variable name="gVarA4LeftMarginCM">0.5</xsl:variable>
  <xsl:variable name="gVarA4RightMarginCM">0.5</xsl:variable>
  <xsl:variable name="gVarA4TopMarginCM">0.5</xsl:variable>
  <xsl:variable name="gVarA4BottomMarginCM">0.5</xsl:variable>

  <!-- ========================================= -->
  <!-- Variables for page header                 -->
  <!-- ========================================= -->
  <xsl:variable name="gVarPageHeaderHeightCM">2</xsl:variable>
  <xsl:variable name="gVarLogoLeftMarginCM">0</xsl:variable>
  <xsl:variable name="gVarLogoTopMarginCM">0</xsl:variable>
  <!-- In order to avoid overflow problems the variable varLogoHeightCM must be at least 0.4cm smaller than (varPageHeaderHeightCM - varLogoTopMarginCM )-->
  <!-- Exemple: varPageHeaderHeightCM=2, varLogoTopMarginCM=0.2. varLogoHeightCM must be greater than 1.4 -->
  <xsl:variable name="gVarLogoHeightCM">0.8</xsl:variable>
  <!-- GS 06012010-->
  <xsl:variable name="gVarImgFormulaHeightCM">2.0</xsl:variable>

  <!-- ========================================= -->
  <!-- Variables for page footer                 -->
  <!-- ========================================= -->
  <xsl:variable name="gVarPageFooterHeightCM">1.5</xsl:variable>
  <xsl:variable name="gVarPageFooterBankNameFontSize">9pt</xsl:variable>
  <xsl:variable name="gVarPageFooterBankNameTextAlign">center</xsl:variable>
  <xsl:variable name="gVarPageFooterBankAddressFontSize">7pt</xsl:variable>
  <xsl:variable name="gVarPageFooterBankAddressTextAlign">center</xsl:variable>

  <!-- ========================================= -->
  <!-- Variables for template confirmation title -->
  <!-- ========================================= -->
  <xsl:variable name="gVarConfirmationTitleTopMarginCM">0.5</xsl:variable>
  <xsl:variable name="gVarConfirmationTitleLeftMarginCM">0</xsl:variable>
  <xsl:variable name="gVarConfirmationTitleTextAlign">center</xsl:variable>
  <xsl:variable name="gVarConfirmationTitleFontSize">16pt</xsl:variable>
  <xsl:variable name="gVarConfirmationTitleColor">darkblue</xsl:variable>
  <xsl:variable name="gVarConfirmationFontWeight">bold</xsl:variable>

  <!-- ========================================= -->
  <!-- Variables for template client adress      -->
  <!-- ========================================= -->
  <xsl:variable name="gVarReceiverAdressLeftMarginCM">13.5</xsl:variable>
  <xsl:variable name="gVarReceiverAdressTopMarginCM">1</xsl:variable>
  <xsl:variable name="gVarReceiverAdressFontSize">11pt</xsl:variable>
  <!-- The alignement of the text -->
  <xsl:variable name="gVarReceiverAdressTextAlign">left</xsl:variable>
  <!-- Font weidth for the first address line (usually is the receiver name) -->
  <xsl:variable name="gVarReceiverAddressFirstLineFontWeidth">bold</xsl:variable>

  <!-- ========================================= -->
  <!-- Variables for template display date       -->
  <!-- ========================================= -->
  <xsl:variable name="gVarDisplayDateLeftMarginCM">13.5</xsl:variable>
  <xsl:variable name="gVarDisplayDateTopMarginCM">0.5</xsl:variable>
  <xsl:variable name="gVarDisplayDateFontSize">9pt</xsl:variable>
  <xsl:variable name="gVarDisplayDateArticleFontWeight">normal</xsl:variable>
  <xsl:variable name="gVarDisplayDateDateFontWieght">bold</xsl:variable>

  <!-- ========================================= -->
  <!-- Variables for template send by section    -->
  <!-- ========================================= -->
  <xsl:variable name="gVarSendByLeftMarginCM">0</xsl:variable>
  <xsl:variable name="gVarSendByTopMarginCM">0.5</xsl:variable>
  <xsl:variable name="gVarSendByFontSize">9pt</xsl:variable>
  <!-- Static section (To:, From:, F/Ref:) -->
  <xsl:variable name="gVarSendByStaticDataWidthCM">1.5</xsl:variable>
  <xsl:variable name="gVarSendByStaticSectionFontWeight">normal</xsl:variable>
  <!-- Dynamic data (ex. SOCIETE GENERALE, MLBANK, 12346) -->
  <xsl:variable name="gVarSendByDynamicSectionFontWeight">normal</xsl:variable>
  <xsl:variable name="gVarSendByDataAlign">left</xsl:variable>

  <!-- ========================================= -->
  <!-- Variables for confirmation text           -->
  <!-- ========================================= -->
  <xsl:variable name="gVarConfirmationTextLeftMarginCM">0</xsl:variable>
  <xsl:variable name="gVarConfirmationTextTopMarginCM">0.8</xsl:variable>
  <xsl:variable name="gVarConfirmationTextFontSize">9pt</xsl:variable>

  <!-- ========================================= -->
  <!-- Variables for product section             -->
  <!-- ========================================= -->
  <!-- Panel -->
  <xsl:variable name="gVarProductSectionPanelTopMarginCM">0.5</xsl:variable>
  <xsl:variable name="gVarProductSectionPanelLeftMargin">0</xsl:variable>
  <xsl:variable name="gVarProductSectionPanelRightMargin">1</xsl:variable>
  <!-- GS 22092009
	change the product section panel text color
	<xsl:variable name="gVarProductSectionPanelTextColor">royalblue</xsl:variable>
	-->
  <xsl:variable name="gVarProductSectionPanelTextColor">darkblue</xsl:variable>
  <xsl:variable name="gVarProductSectionPanelBackgColor">lavender</xsl:variable>
  <xsl:variable name="gVarProductSectionPanelFontSize">12pt</xsl:variable>
  <xsl:variable name="gVarProductSectionPanelFontWeigth">bold</xsl:variable>
  <xsl:variable name="gVarProductSectionPanelTextAlign">left</xsl:variable>
  <!-- Text -->
  <xsl:variable name="gVarProductSectionTextTopMarginCM">0.2</xsl:variable>
  <xsl:variable name="gVarProductSectionTextLeftMarginCM">0.5</xsl:variable>
  <xsl:variable name="gVarProductSectionTextRightMarginCM">1</xsl:variable>
  <xsl:variable name="gVarProductSectionTextFontSize">9pt</xsl:variable>
  <xsl:variable name="gVarProductSectionTextPaddingTop">0.1</xsl:variable>
  <!-- Table -->
  <xsl:variable name="gVarProductSectionTextColumnWidthCM">6.5</xsl:variable>
  <!-- blank space-->
  <xsl:variable name="gVarSpace">
    <xsl:value-of select="' '"/>
  </xsl:variable>
  <!-- ========================================= -->
  <!-- Variables for signature text              -->
  <!-- ========================================= -->
  <xsl:variable name="gVarSignatureTextLeftMarginCM">0</xsl:variable>
  <xsl:variable name="gVarSignatureTextRightMarginCM">0</xsl:variable>
  <xsl:variable name="gVarSignatureTextTopMarginCM">1</xsl:variable>
  <xsl:variable name="gVarSignatureTextFontSize">9pt</xsl:variable>

  <!-- ==================================================== -->
  <!-- Variables for sender and receiver signature template -->
  <!-- ==================================================== -->
  <!-- Font size for both signatures -->
  <xsl:variable name="gVarSenderReceiverSignaturesFontSize">9pt</xsl:variable>
  <!-- Sender signature margins -->
  <xsl:variable name="gVarSenderSignaturesLeftMarginCM">12</xsl:variable>
  <xsl:variable name="gVarSenderSignaturesTopMarginCM">0.5</xsl:variable>
  <!-- Receiver signature margins -->
  <xsl:variable name="gVarReceiverSignatureLeftMarginCM">0</xsl:variable>
  <xsl:variable name="gVarReceiverSignatureTopMarginCM">0.2</xsl:variable>

  <!-- ======================================================================== -->
  <!-- Calculated variables for product section                                 -->
  <!--                  PLEASE DO NOT MODIFY OR REMOVE                          -->
  <!-- ======================================================================== -->
  <!-- Table total width -->
  <!-- We have to calculate it dinamically -->
  <xsl:variable name="gVarProductSectionTableWidthCM">
    <xsl:value-of select="$gVarA4VerticalPageWidthCM - $gVarA4LeftMarginCM - $gVarA4RightMarginCM - $gVarProductSectionTextLeftMarginCM - $gVarProductSectionTextRightMarginCM" />
  </xsl:variable>

  <!-- Data column (ex. EUR 1,000,000.000) width -->
  <xsl:variable name="gVarProductSectionDataColumnWidthCM">
    <xsl:value-of select="$gVarProductSectionTableWidthCM - $gVarProductSectionTextColumnWidthCM"/>
  </xsl:variable>

  <!-- ====================================================================================== -->
  <!-- BEGIN REGION: variables for debug                                                      -->
  <!-- ====================================================================================== -->
  <!-- Set 1 for debug all files, 0 otherwise-->
  <xsl:variable name="gVarIsDebug">
    0
  </xsl:variable>

  <!-- PLEASE DO NOT MODIFY OR REMOVE THIS REGION -->
  <xsl:variable name="gVarBorder">
    <xsl:choose>
      <xsl:when test="$varIsDebug=1">0.8pt solid black</xsl:when>
      <xsl:otherwise>0pt solid black</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="gVarPageHeaderColor">
    <xsl:choose>
      <xsl:when test="$varIsDebug=1">gray</xsl:when>
      <xsl:otherwise>white</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="gVarPageFooterColor">
    <xsl:choose>
      <xsl:when test="$varIsDebug=1">gray</xsl:when>
      <xsl:otherwise>white</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="gVarPageBodyColor">
    <xsl:choose>
      <xsl:when test="$varIsDebug=1">yellow</xsl:when>
      <xsl:otherwise>white</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="gVarPageLeftColor">
    <xsl:choose>
      <xsl:when test="$varIsDebug=1">red</xsl:when>
      <xsl:otherwise>white</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="gVarPageRightColor">
    <xsl:choose>
      <xsl:when test="$varIsDebug=1">red</xsl:when>
      <xsl:otherwise>white</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <!-- ====================================================================================== -->
  <!-- END REGION: variables for debug                                                        -->
  <!-- ====================================================================================== -->


  <!-- ======================================================================================================================= -->
  <!--                                                                                                                         -->
  <!--                                       END REGION: variables for all confirmations                                       -->
  <!--                                                                                                                         -->
  <!--                                                                                                                         -->
  <!-- ======================================================================================================================= -->

</xsl:stylesheet>