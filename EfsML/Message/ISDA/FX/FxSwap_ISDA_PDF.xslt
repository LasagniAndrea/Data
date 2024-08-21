<?xml version="1.0" encoding="UTF-8" ?>

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:dt="http://xsltsl.org/date-time" xmlns:fo="http://www.w3.org/1999/XSL/Format" version="1.0">

  <!--
	==============================================================
	Summary : FxSwap ISDA PDF
	==============================================================
	Revision: 3
	Date    : 20100106
	Author  : Gianmario SERIO
	Version : 2.3.1.0_3
	Update  : Add DisplayFormula 
		      Add displayExtendFields
			  Add variable: varImgFormulaHeightCM
	==================================================================================
	Revision: 2
	Date    : 02092009
	Author  : Gianmario SERIO
	Version : 2.3.0.3_2
	Update  : Add variable varAnnexSectionTextLongColumnWidthCM in variables region
		      Add variable varAnnexSectionTextShortColumnWidthCM in variables region
		      Add variable varBoldFontWeidth in variables region
		      Add variable varAnnexSectionColumnLeftAlign in variables region
		      Add variable varAnnexSectionColumnRightAlign in variables region
	          
			  Add displayAdditionalInformation
	==============================================================	
	File    : FxSwap_ISDA_PDF.xslt
	Date    : 20.05.2009
	Author  : Gianmario SERIO
	Version : 2.3.0.3_1
	Description:
	==============================================================
	-->

  <xsl:import href="..\Shared\Shared_Variables.xslt"/>
  <xsl:import href="..\Shared\Shared_ISDA_Business.xslt"/>
  <xsl:import href="..\Shared\Shared_ISDA_PDF.xslt"/>
  <xsl:import href="FxSwap_ISDA_Business.xslt"/>
  <xsl:import href="..\..\Custom\Custom_Message_PDF.xslt"/>

  <xsl:output method="xml" encoding="UTF-8"/>

  <!-- xslt includes -->
  <xsl:include href="..\..\..\Library\Resource.xslt"/>
  <xsl:include href="..\..\..\Library\DtFunc.xslt"/>
  <xsl:include href="..\..\..\Library\NbrFunc.xslt" />
  <xsl:include href="..\..\..\Library\StrFunc.xslt"/>
  <xsl:include href="..\..\..\Library\xsltsl\date-time.xsl"/>

  <!-- Global Parameters -->
  <xsl:param name="pCurrentCulture" select="'en-GB'"/>
  <xsl:param name="pTradeExtlLink" select="''"/>
  <xsl:param name="pProduct" select="''"/>
  <xsl:param name="pInstrument" />

  <!--
	******************************************
	Updatable variable: Begin section
	******************************************
	-->
  <!--
	varConfirmationType
	when it is "ISDA" uses the "standard ISDA" confirmation
	whes it is "FULL" uses the "Full" Confirmation
	-->
  <xsl:variable name="varConfirmationType">
    <xsl:text>ISDA</xsl:text>
  </xsl:variable>
  <!--
	******************************************
	Updatable variable: End section
	******************************************
	-->

  <!-- Display Web Page -->
  <xsl:template match="/">
    <fo:root xmlns:fo="http://www.w3.org/1999/XSL/Format">

      <!-- Define A4 page and its margins -->
      <xsl:call-template name="definePage"/>

      <fo:page-sequence master-reference="A4" initial-page-number="1" font-family="{$varFontFamily}">

        <!-- ============================================================================== -->
        <!-- Page header                                                                    -->
        <!-- ============================================================================== -->
        <!-- This section must be before page body -->
        <fo:static-content flow-name="A4-header">
          <xsl:call-template name="displayPageHeader"/>
        </fo:static-content>

        <!-- ============================================================================== -->
        <!-- Page footer                                                                    -->
        <!-- ============================================================================== -->
        <!-- This section must be before page body -->
        <fo:static-content flow-name="A4-footer">
          <xsl:call-template name="displayPageFooter"/>
        </fo:static-content>

        <!-- ============================================================================== -->
        <!-- Page body                                                                      -->
        <!-- ============================================================================== -->
        <fo:flow flow-name="A4-body">

          <!-- Confirmation title (ex.: Confirmation of a VanillaSwap Transaction) -->
          <xsl:call-template name="displayConfirmationTitle"/>
          <xsl:call-template name="displayBlockBreakline"/>

          <!-- Bank address (ex.: SOCIETE GENERALE 29 , BOULEVARD HAUSMANN....) -->
          <xsl:call-template name="displayReceiverAddress"></xsl:call-template>
          <xsl:call-template name="displayBlockBreakline"/>

          <!-- Display date (ex. The June 30, 2008) -->
          <xsl:call-template name="displayDate"></xsl:call-template>

          <!-- Display send by section (ex.: From:SOCIETE GENERALE To:MLBANK F/Ref.:123456) -->
          <xsl:call-template name="displaySendBy"></xsl:call-template>
          <xsl:call-template name="displayBlockBreakline"/>

          <!-- Display confirmation text (ex.: Dear Sir or Madam...) -->
          <xsl:call-template name="displayFxConfirmationText"></xsl:call-template>

          <!-- Display terms (Notional amounts, Trade date, Termination Date...) -->
          <xsl:call-template name="displayFxSwapLegTerms">
            <xsl:with-param name="pFxLegContainer" select="//dataDocument/trade/fxSwap" />
            <!-- fx swap can be either deliverable or non deliverable: this parameter is just a default value-->
            <xsl:with-param name="pIsNonDeliverable" select="false()"/>
          </xsl:call-template>

          <!-- Call template Formula -->
          <xsl:call-template name="displayFormula"/>
          <xsl:call-template name="displayBlockBreakline"/>

          <!-- Display Ird calculation agent (ex. Calculation...) -->
          <xsl:call-template name="displayIrdCalculationAgent"/>

          <!-- Display brokerage (ex. Fees...) -->
          <xsl:if test="//dataDocument/trade/otherPartyPayment[paymentType = 'Brokerage' and payerPartyReference/@href = $sendToID]">
            <xsl:call-template name="displayBrokerage"/>
          </xsl:if>
          <xsl:call-template name="displayBlockBreakline"/>

          <!-- Display displayExtendFields (tradeExtends) -->
          <xsl:if test ="normalize-space(//dataDocument/trade/tradeExtends)">
            <xsl:call-template name="displayExtendFields"/>
          </xsl:if>
          <xsl:call-template name="displayBlockBreakline"/>

          <!-- Display AdditionInformation (notepad) -->
          <xsl:if test ="normalize-space(//notepad)">
            <xsl:call-template name="displayAdditionalInformation"/>
          </xsl:if>
          <xsl:call-template name="displayBlockBreakline"/>

          <!-- Display footer product (ex. Account Details...) -->
          <xsl:call-template name="displayFooter_Product"/>
          <xsl:call-template name="displayBlockBreakline"/>

          <!-- Display text signature (ex. Please confirm that...) -->
          <xsl:call-template name="displaySignatureText"/>
          <xsl:call-template name="displayBlockBreakline"/>

          <!-- Sender and receiver signatures -->
          <xsl:call-template name="displaySenderReceiverSignatures"/>
          <fo:block id="EndOfDoc" />
        </fo:flow>

      </fo:page-sequence>
    </fo:root>
  </xsl:template>

  <!-- =================================================================================================== -->
  <!--                                                                                                     -->
  <!--                                     BEGIN REGION: variables                                         -->
  <!--                                                                                                     -->
  <!-- Modify following variables only if you want for swap confirmation a specific behavior               -->
  <!-- =================================================================================================== -->

  <!-- ====================================================== -->
  <!-- Please, Add new variables in this section              -->
  <!-- ====================================================== -->

  <xsl:variable name="varAnnexSectionTextLongColumnWidthCM">
    <xsl:value-of select="$gVarAnnexSectionTextLongColumnWidthCM"/>
  </xsl:variable>
  <xsl:variable name="varAnnexSectionTextShortColumnWidthCM">
    <xsl:value-of select="$gVarAnnexSectionTextShortColumnWidthCM"/>
  </xsl:variable>
  <xsl:variable name="varBoldFontWeidth">
    <xsl:value-of select="$gVarBoldFontWeidth"/>
  </xsl:variable>
  <xsl:variable name="varAnnexSectionColumnLeftAlign">
    <xsl:value-of select="$gVarAnnexSectionColumnLeftAlign"/>
  </xsl:variable>
  <xsl:variable name="varAnnexSectionColumnRightAlign">
    <xsl:value-of select="$gVarAnnexSectionColumnRightAlign"/>
  </xsl:variable>

  <!-- ========================================= -->
  <!-- Variable for debug                        -->
  <!-- ========================================= -->
  <!-- Set 1 for debug Swap_ISDA_PDF.xslt file, 0 otherwise-->
  <xsl:variable name="varIsDebug">
    <xsl:value-of select="$gVarIsDebug"/>
  </xsl:variable>

  <!-- ========================================= -->
  <!-- Variables for fonts                       -->
  <!-- ========================================= -->
  <xsl:variable name="varFontFamily">
    <xsl:value-of select="$gVarFontFamily"/>
  </xsl:variable>

  <!-- ========================================= -->
  <!-- Variables for A4 page format              -->
  <!-- ========================================= -->
  <xsl:variable name="varA4LeftMarginCM">
    <xsl:value-of select="$gVarA4LeftMarginCM"/>
  </xsl:variable>
  <xsl:variable name="varA4RightMarginCM">
    <xsl:value-of select="$gVarA4RightMarginCM"/>
  </xsl:variable>
  <xsl:variable name="varA4TopMarginCM">
    <xsl:value-of select="$gVarA4TopMarginCM"/>
  </xsl:variable>
  <xsl:variable name="varA4BottomMarginCM">
    <xsl:value-of select="$gVarA4BottomMarginCM"/>
  </xsl:variable>

  <!-- ========================================= -->
  <!-- Variables for page header                 -->
  <!-- ========================================= -->
  <xsl:variable name="varPageHeaderHeightCM">
    <xsl:value-of select="$gVarPageHeaderHeightCM"/>
  </xsl:variable>

  <xsl:variable name="varLogoHeightCM">
    <xsl:value-of select="$gVarLogoHeightCM"/>
  </xsl:variable>
  <!-- To avoid overflow problems this variable varLogoHeightCM must be at least 0.4 cm more small than (varPageHeaderHeightCM - varLogoTopMarginCM )-->
  <!-- Exemple: varPageHeaderHeightCM=2, varLogoTopMarginCM=0.2. varLogoHeightCM must be greater than 1.4 -->

  <xsl:variable name="varLogoLeftMarginCM">
    <xsl:value-of select="$gVarLogoLeftMarginCM"/>
  </xsl:variable>
  <xsl:variable name="varLogoTopMarginCM">
    <xsl:value-of select="$gVarLogoTopMarginCM"/>
  </xsl:variable>

  <!-- ========================================= -->
  <!-- Variables for footer                      -->
  <!-- ========================================= -->
  <xsl:variable name="varPageFooterHeightCM">
    <xsl:value-of select="$gVarPageFooterHeightCM"/>
  </xsl:variable>
  <xsl:variable name="varPageFooterBankNameFontSize">
    <xsl:value-of select="$gVarPageFooterBankNameFontSize"/>
  </xsl:variable>
  <xsl:variable name="varPageFooterBankNameTextAlign">
    <xsl:value-of select="$gVarPageFooterBankNameTextAlign"/>
  </xsl:variable>
  <xsl:variable name="varPageFooterBankAddressFontSize">
    <xsl:value-of select="$gVarPageFooterBankAddressFontSize"/>
  </xsl:variable>
  <xsl:variable name="varPageFooterBankAddressTextAlign">
    <xsl:value-of select="$gVarPageFooterBankAddressTextAlign"/>
  </xsl:variable>

  <!-- ========================================= -->
  <!-- Variables for template confirmation title -->
  <!-- ========================================= -->
  <xsl:variable name="varConfirmationTitleTopMarginCM">
    <xsl:value-of select="$gVarConfirmationTitleTopMarginCM"/>
  </xsl:variable>
  <xsl:variable name="varConfirmationTitleLeftMarginCM">
    <xsl:value-of select="$gVarConfirmationTitleLeftMarginCM"/>
  </xsl:variable>
  <xsl:variable name="varConfirmationTitleTextAlign">
    <xsl:value-of select="$gVarConfirmationTitleTextAlign"/>
  </xsl:variable>
  <xsl:variable name="varConfirmationTitleFontSize">
    <xsl:value-of select="$gVarConfirmationTitleFontSize"/>
  </xsl:variable>
  <xsl:variable name="varConfirmationTitleColor">
    <xsl:value-of select="$gVarConfirmationTitleColor"/>
  </xsl:variable>
  <xsl:variable name="varConfirmationFontWeight">
    <xsl:value-of select="$gVarConfirmationFontWeight"/>
  </xsl:variable>

  <!-- ========================================= -->
  <!-- Variables for template client adress      -->
  <!-- ========================================= -->
  <xsl:variable name="varReceiverAdressLeftMarginCM">
    <xsl:value-of select="$gVarReceiverAdressLeftMarginCM"/>
  </xsl:variable>
  <xsl:variable name="varReceiverAdressTopMarginCM">
    <xsl:value-of select="$gVarReceiverAdressTopMarginCM"/>
  </xsl:variable>
  <xsl:variable name="varReceiverAdressFontSize">
    <xsl:value-of select="$gVarReceiverAdressFontSize"/>
  </xsl:variable>

  <!-- Font weidth for the first address line (usually it is the receiver name) -->
  <xsl:variable name="varReceiverAddressFirstLineFontWeidth">
    <xsl:value-of select="$gVarReceiverAddressFirstLineFontWeidth"/>
  </xsl:variable>

  <!-- The alignement of the text -->
  <xsl:variable name="varReceiverAdressTextAlign">
    <xsl:value-of select="$gVarReceiverAdressTextAlign"/>
  </xsl:variable>

  <!-- ========================================= -->
  <!-- Variables for template display date       -->
  <!-- ========================================= -->
  <xsl:variable name="varDisplayDateLeftMarginCM">
    <xsl:value-of select="$gVarDisplayDateLeftMarginCM"/>
  </xsl:variable>
  <xsl:variable name="varDisplayDateTopMarginCM">
    <xsl:value-of select="$gVarDisplayDateTopMarginCM"/>
  </xsl:variable>
  <xsl:variable name="varDisplayDateFontSize">
    <xsl:value-of select="$gVarDisplayDateFontSize"/>
  </xsl:variable>
  <xsl:variable name="varDisplayDateArticleFontWeight">
    <xsl:value-of select="$gVarDisplayDateArticleFontWeight"/>
  </xsl:variable>
  <xsl:variable name="varDisplayDateDateFontWieght">
    <xsl:value-of select="$gVarDisplayDateDateFontWieght"/>
  </xsl:variable>

  <!-- ========================================= -->
  <!-- Variables for template send by section    -->
  <!-- ========================================= -->
  <xsl:variable name="varSendByLeftMarginCM">
    <xsl:value-of select="$gVarSendByLeftMarginCM"/>
  </xsl:variable>
  <xsl:variable name="varSendByTopMarginCM">
    <xsl:value-of select="$gVarSendByTopMarginCM"/>
  </xsl:variable>
  <xsl:variable name="varSendByFontSize">
    <xsl:value-of select="$gVarSendByFontSize"/>
  </xsl:variable>
  <xsl:variable name="varSendByStaticSectionFontWeight">
    <xsl:value-of select="$gVarSendByStaticSectionFontWeight"/>
  </xsl:variable>
  <xsl:variable name="varSendByStaticDataWidthCM">
    <xsl:value-of select="$gVarSendByStaticDataWidthCM"/>
  </xsl:variable>
  <xsl:variable name="varSendByDynamicSectionFontWeight">
    <xsl:value-of select="$gVarSendByDynamicSectionFontWeight"/>
  </xsl:variable>
  <xsl:variable name="varSendByDataAlign">
    <xsl:value-of select="$gVarSendByDataAlign"/>
  </xsl:variable>

  <!-- ========================================= -->
  <!-- Variables for confirmation text           -->
  <!-- ========================================= -->
  <xsl:variable name="varConfirmationTextLeftMarginCM">
    <xsl:value-of select="$gVarConfirmationTextLeftMarginCM"/>
  </xsl:variable>
  <xsl:variable name="varConfirmationTextTopMarginCM">
    <xsl:value-of select="$gVarConfirmationTextTopMarginCM"/>
  </xsl:variable>
  <xsl:variable name="varConfirmationTextFontSize">
    <xsl:value-of select="$gVarConfirmationTextFontSize"/>
  </xsl:variable>

  <!-- ========================================= -->
  <!-- Variables for product section             -->
  <!-- ========================================= -->
  <!-- Panel -->
  <xsl:variable name="varProductSectionPanelTopMarginCM">
    <xsl:value-of select="$gVarProductSectionPanelTopMarginCM"/>
  </xsl:variable>
  <xsl:variable name="varProductSectionPanelLeftMargin">
    <xsl:value-of select="$gVarProductSectionPanelLeftMargin"/>
  </xsl:variable>
  <xsl:variable name="varProductSectionPanelRightMargin">
    <xsl:value-of select="$gVarProductSectionPanelRightMargin"/>
  </xsl:variable>
  <xsl:variable name="varProductSectionPanelTextColor">
    <xsl:value-of select="$gVarProductSectionPanelTextColor"/>
  </xsl:variable>
  <xsl:variable name="varProductSectionPanelBackgColor">
    <xsl:value-of select="$gVarProductSectionPanelBackgColor"/>
  </xsl:variable>
  <xsl:variable name="varProductSectionPanelFontSize">
    <xsl:value-of select="$gVarProductSectionPanelFontSize"/>
  </xsl:variable>
  <xsl:variable name="varProductSectionPanelFontWeigth">
    <xsl:value-of select="$gVarProductSectionPanelFontWeigth"/>
  </xsl:variable>
  <xsl:variable name="varProductSectionPanelTextAlign">
    <xsl:value-of select="$gVarProductSectionPanelTextAlign"/>
  </xsl:variable>
  <!-- Text -->
  <xsl:variable name="varProductSectionTextTopMarginCM">
    <xsl:value-of select="$gVarProductSectionTextTopMarginCM"/>
  </xsl:variable>
  <xsl:variable name="varProductSectionTextLeftMarginCM">
    <xsl:value-of select="$gVarProductSectionTextLeftMarginCM"/>
  </xsl:variable>
  <xsl:variable name="varProductSectionTextRightMarginCM">
    <xsl:value-of select="$gVarProductSectionTextRightMarginCM"/>
  </xsl:variable>
  <xsl:variable name="varProductSectionTextFontSize">
    <xsl:value-of select="$gVarProductSectionTextFontSize"/>
  </xsl:variable>
  <xsl:variable name="varProductSectionTextPaddingTop">
    <xsl:value-of select="$gVarProductSectionTextPaddingTop"/>
  </xsl:variable>
  <!-- Table -->
  <xsl:variable name="varProductSectionTextColumnWidthCM">
    <xsl:value-of select="$gVarProductSectionTextColumnWidthCM"/>
  </xsl:variable>

  <!-- ========================================= -->
  <!-- Variables for signature text              -->
  <!-- ========================================= -->
  <xsl:variable name="varSignatureTextLeftMarginCM">
    <xsl:value-of select="$gVarSignatureTextLeftMarginCM"/>
  </xsl:variable>
  <xsl:variable name="varSignatureTextRightMarginCM">
    <xsl:value-of select="$gVarSignatureTextRightMarginCM"/>
  </xsl:variable>
  <xsl:variable name="varSignatureTextTopMarginCM">
    <xsl:value-of select="$gVarSignatureTextTopMarginCM"/>
  </xsl:variable>
  <xsl:variable name="varSignatureTextFontSize">
    <xsl:value-of select="$gVarSignatureTextFontSize"/>
  </xsl:variable>

  <!-- ==================================================== -->
  <!-- Variables for sender and receiver signature template -->
  <!-- ==================================================== -->
  <!-- Font size for both signatures -->
  <xsl:variable name="varSenderReceiverSignaturesFontSize">
    <xsl:value-of select="$gVarSenderReceiverSignaturesFontSize"/>
  </xsl:variable>
  <!-- Sender signature margins -->
  <xsl:variable name="varSenderSignaturesLeftMarginCM">
    <xsl:value-of select="$gVarSenderSignaturesLeftMarginCM"/>
  </xsl:variable>
  <xsl:variable name="varSenderSignaturesTopMarginCM">
    <xsl:value-of select="$gVarSenderSignaturesTopMarginCM"/>
  </xsl:variable>
  <!-- Receiver signature margins -->
  <xsl:variable name="varReceiverSignatureLeftMarginCM">
    <xsl:value-of select="$gVarReceiverSignatureLeftMarginCM"/>
  </xsl:variable>
  <xsl:variable name="varReceiverSignatureTopMarginCM">
    <xsl:value-of select="$gVarReceiverSignatureTopMarginCM"/>
  </xsl:variable>

  <!-- ======================================================================== -->
  <!-- Calculated variables for product section  PLEASE DO NOT MODIFY OR REMOVE -->
  <!--                  PLEASE DO NOT MODIFY OR REMOVE                          -->
  <!-- ======================================================================== -->
  <!-- Table total width -->
  <!-- We have to calculate it dinamically -->
  <xsl:variable name="varProductSectionTableWidthCM">
    <xsl:value-of select="$gVarA4VerticalPageWidthCM - $varA4LeftMarginCM - $varA4RightMarginCM - $varProductSectionTextLeftMarginCM - $varProductSectionTextRightMarginCM" />
  </xsl:variable>

  <!-- Data column (ex. EUR 1,000,000.000) width -->
  <xsl:variable name="varProductSectionDataColumnWidthCM">
    <xsl:value-of select="$varProductSectionTableWidthCM - $varProductSectionTextColumnWidthCM"/>
  </xsl:variable>

  <!-- ======================================================================== -->
  <!-- Variables for product swap                                               -->
  <!--                  PLEASE DO NOT MODIFY OR REMOVE                          -->
  <!-- ======================================================================== -->
  <xsl:variable name="varIsSwaption">
    <xsl:value-of select="false()"/>
  </xsl:variable>

  <!-- ======================================================================== -->
  <!-- Please add new variables in this section                                 -->
  <!--                                                                          -->
  <!-- ======================================================================== -->
  <!-- GS 20100106-->
  <xsl:variable name="varImgFormulaHeightCM">
    <xsl:value-of select="$gVarImgFormulaHeightCM"/>
  </xsl:variable>

  <!-- =================================================================================================== -->
  <!--                                                                                                     -->
  <!--                                     END REGION: variables                                           -->
  <!--                                                                                                     -->
  <!-- =================================================================================================== -->


  <!-- =================================================================================================== -->
  <!--                                                                                                     -->
  <!--                                     BEGIN REGION: not used templates                                -->
  <!-- PLEASE DO NOT MODIFY OR REMOVE                                                                      -->
  <!-- =================================================================================================== -->
  <xsl:template name="displayTerms">
    {unused}
  </xsl:template>
  <!-- =================================================================================================== -->
  <!--                                                                                                     -->
  <!--                                     END REGION: not used templates                                  -->
  <!--                                                                                                     -->
  <!-- =================================================================================================== -->

</xsl:stylesheet>