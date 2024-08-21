<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:dt="http://xsltsl.org/date-time"
  xmlns:fo="http://www.w3.org/1999/XSL/Format"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt">

  <!--  
================================================================================================================
  Date: 28.08.2009                                                                              
	Author: Guido Poerio/Marcello Faga
	Modifications:
	1. Bank's name modified (from PARIS ENT. to BPRP PARIS ENT.)
	2. Border for instrument header of detailed fees added 
	3. [MF] Making dynamique the header text (e.g. Enterprise d'investissement HPC)
	4. [MF] Fax entity added
	5. [MF] ISIN code added
	6. [MF] Trade Date updated
	7. [MF] New text format for not numeric table fields
	8. [MF] Hidden trade line with 0 amount (see also varShowZeroValue variable)
	9. [MF] Invoice dates added
	10.[MF] Invoice receiver identifier deleted - marked the receiver name instead
	11.[MF] Added Credit Note/Additional Invoice parsing
	11.[MF] Added Amended invoice parsing
	12.[MF] Rate 0 value well formatted
	13.[MF] Hard-coded "Au capital de"  
	14.[MF] Added flag varUseTradeSorting. When true() the xsl will use the sorting stream features
	15.[MF] Refactoring code
	========================================================================================== 
	Date: 03.11.2009                                                                              
	Author: Eric Goux
	Add:
	1.  [EG] varPageHeaderEntityAdressWidth = varPageHeaderWidth - varPageHeaderLogoWidth -	varPageInvoiceWidth
	Modifications:
	1.  [EG] varPageA4VerticalHeaderLogoWidth     : 8   -> 3
	2.  [EG] varPageA4VerticalInvoiceWidth        : 8   -> 3
	3.  [EG] varPageA4VerticalFooterExtent        : 1.8 -> 0.8
	4.  [EG] varPageA4LandscapePageHeight         : 27  -> 21
	5.  [EG] varPageA4LandscapeBodyMarginBottom   : 2.1 -> 0.8 
	6.  [EG] varPageA4LandscapeBodyMarginTop      : 2.8 -> 2.3
	7.  [EG] varPageA4LandscapeHeaderExtent	      : 2.8 -> 2
	8.  [EG] varPageA4LandscapeHeaderLogoWidth    : 8   -> 6
	9.  [EG] varPageA4LandscapeInvoiceWidth       : 8   -> 6
	10. [EG] varPageA4LandscapeInvoiceFontSize    : 11  -> 8
	11. [EG] varPageA4LandscapeInvoiceMarginRight : 0.5 -> 0
	10. [EG] varPageA4LandscapeFooterExtent       : 1.8 -> 0.5
	11. [EG] varPageA4VerticalInvoicePaddingTop   : 1   -> 0.8
	12. [EG] varDetFeesTableCol6Width             : 18  -> 14 (Rate)
	13. [EG] varDetFeesTableCol12Width            : 19  -> 25 (Fee Schedule)
	14. [EG] varDetFeesTableCol11Width            : 25  -> 23 (ISINCode)
	
	15. [EG] Header structure  : changed for table with 3 columns (1st:LOGO, 2nd:ADDRESS, 3rd:INVOICETITLE)
	16. [EG] Bold Style        : InvoiceNumber, CreditNoteNumber and AdditionalInvoiceNumber
	17. [EG] Bold Style        : GrossTurnOverAmount,RebateAmount and all NetAmount on summary page
	18. [EG] Text Align Left   : Counterparty,ISIN Code
	19. [EG] Text Align center : Buyer/Seller
  ========================================================================================== 
	Date: 25.11.2009                                                                              
	Author: Marcello Faga
	Add:
	1.  [MF] Client name on the lesft side of the header for detailed fees pages
    2   [MF] Total amount at the end of the detailed fees table
    
	Modifications:
	1.  [MF] Logo table cell width changed
  ========================================================================================== 
	Date: 26.11.2009                                                                              
	Author: Marcello Faga
	Add:
	1.  [MF] wrong ISIN replaced by empty strings (check on hard coded  string "EN ATTENTE" and string length different by 12 )
	========================================================================================== 
  Date: 07.12.2009                                                                              
	Author: Marcello Faga
	Modifications:
	1.  Bug: Wrong total amounts for amended invoice / Status: closed, DisplayTotalValues was called without mandatory 
            pCreditNoteVisualization parameter
    1.  Bug: Credit note not shown / Status: closed, varEnableCreditNoteVisualization was not correctly assigned inside DocumentContent
            template
            
    Remarcks: problems lied to resources could be issued by wrong resourtce file versions, check Message\Custom and Functions folders     
	========================================================================================== 
  Date: 30.12.2009                                                                              
	Author: Domenico Rotta
	Modifications:
	1) Reduced the size of almost all fields
	2)	Added the field « Nominal » after the field « Fee basis ».
		if the instrument is « REPO », then « Nominal » is set to "invoiceTrade/asset/notionalAmount/amount",
		else "invoiceTrade/notionalAmount/amount"
	3)	Duration
		If the element "invoiceFees/feeSchedule/duration" does not exist (this element is currently used now), then « Duration » it is set to
		"invoiceTrade/periodNumberOfDays" 
	4)	Fee basis
		« Money » format of its value
	5)	ISIN code
		If the « ISIN code » is not set, then « ISIN code » is set to the description of its underlyer
		"invoiceTrade/asset/description"
	========================================================================================== 
  Date: 04.01.2010                                                                              
	Author: Domenico Rotta
	Modifications:
	1) update of the scheme of the actor identification 
	   "http://www.euro-finance-systems.fr/otcml/actorBic" replaces "http://www.euro-finance-systems.fr/otcml/actorIdentifier"
	2) update of the size of fields "ISIN code" and "Fee schedule"	
	3) update of the field Fee basis, the value 'NaN' is no more displayed when the element does not exist
 	========================================================================================== 
   
    <xsl:call-template name="DisplayTotalValues">
                        <xsl:with-param name="pCreditNoteVisualization" select="$varEnableCreditNoteVisualization"/>

	To do:
	1. Données manquantes dans le flux xml:
	- [MF] changer de format (de vertical à horizontal) pour la page entête
	- [MF] passer comme parametre du FOP processeur les valeurs pour les variables varShowZeroValue et varUseTradeSorting
	
	========================================================================================== 
  Date: 08.06.2010                                                                              
	Author: Eric GOUX
	Modifications:
	1. Tax
  ========================================================================================== 
  Date: 22.09.2010                                                                              
	Author: RD
	Modifications:
	1.  Use new template "format-shortdate2" to formate Date for display according tu culture  
  ================================================================================================================
  Date: 23.10.2014                                                                              
	Author: PL
	Modifications:
  - Element "feeSchedule/assessmentBasisValue" is renamed to "feeSchedule/assessmentBasisValue1"
  - New element avalaible "feeSchedule/assessmentBasisValue2"
-->
  <!-- ================================================== -->
  <!--        xslt includes                               -->
  <!-- ================================================== -->
  <xsl:include href="InvoiceCommon.xslt" />

  <!--==========================================================================================-->
  <!-- TEMPLATE : DocumentContent                                                               -->
  <!--            Summary: Document tree structure																							-->
  <!--            Document types: Original, Additional Amended Invoice or Credit Note           -->
  <!--            Parameters:	pDocumentType input the doc you want to format;						        -->
  <!--                        (permitted values: Main/Enclosure)                                -->
  <!--==========================================================================================-->
  <xsl:template name="DocumentContent">
    <xsl:param name="pDocumentType" />
    <!-- Actually the "credit note visualization format" is disabled except we are formatting the Additional Invoice document 
		     in all the other cases we can leave it enabled -->
    <xsl:variable name="varEnableCreditNoteVisualization">
      <xsl:choose>
        <xsl:when test="($pDocumentType = 'Main')">
          <xsl:value-of select="true()" />
        </xsl:when>
        <xsl:when test="($pDocumentType = 'Enclosure')">
          <xsl:value-of select="false()" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="false()" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- Actually the fees orientation is landscape except we are formatting the Additional Invoice document 
			 in all the other cases we can leave it enabled -->
    <xsl:variable name="vIsLandscape">
      <xsl:choose>
        <xsl:when test="($varIsAmendedInvoice = 'true') and ($varEnableCreditNoteVisualization = 'true')">
          <xsl:value-of select="false()" />
        </xsl:when>
        <xsl:when test="(pDocumentType = 'Main') and ($varIsInvoice = 'true')">
          <xsl:value-of select="true()" />
        </xsl:when>
        <xsl:when test="(pDocumentType = 'Main') and ($varIsAmendedInvoice = 'true')">
          <xsl:value-of select="false()" />
        </xsl:when>
        <xsl:when test="(pDocumentType = 'Enclosure')">
          <xsl:value-of select="true()" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="true()" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="varDetailedFeesOrientation">
      <xsl:choose>
        <xsl:when test="($vIsLandscape = 'true')">
          <xsl:value-of select="'A4-landscape'" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'Amended-A4-vertical'" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!--==========================================================================================-->
    <!-- Pages of detailed fees                                                                   -->
    <!--==========================================================================================-->
    <fo:page-sequence master-reference="{$varDetailedFeesOrientation}">
      <!--==========================================================================================-->
      <!-- Header of detailes pages                                                                 -->
      <!--==========================================================================================-->
      <fo:static-content flow-name="{$varDetailedFeesOrientation}-header">
        <xsl:call-template name="DisplayPageHeader">
          <xsl:with-param name="pLogo" select="$gImgLogo" />
          <xsl:with-param name="pIsFirstPage" select="false()" />
          <!-- Enable the credit note visualization, in case we are printing a credit note -->
          <xsl:with-param name="pIsCreditNoteVisualization" select="$varEnableCreditNoteVisualization" />
          <xsl:with-param name="pIsLandscape" select="$vIsLandscape"/>
        </xsl:call-template>
      </fo:static-content>
      <!--==========================================================================================-->
      <!-- Footer of detailes pages                                                                 -->
      <!--==========================================================================================-->
      <fo:static-content flow-name="{$varDetailedFeesOrientation}-footer">
        <xsl:call-template name="DisplayPageFooter">
          <xsl:with-param name="pIsLastPage" select="false()" />
          <xsl:with-param name="pIsLandscape" select="$vIsLandscape"/>
        </xsl:call-template>
      </fo:static-content>
      <!--==========================================================================================-->
      <!-- Body of detailes pages                                                                 -->
      <!--==========================================================================================-->
      <fo:flow flow-name="{$varDetailedFeesOrientation}-body">
        <xsl:variable name="vFormula">
          <xsl:choose>
            <xsl:when test="string-length($gReportHeaderFooter/hFormula/text())>0">
              <xsl:value-of select="$gReportHeaderFooter/hFormula/text()"/>
            </xsl:when>
          </xsl:choose>
        </xsl:variable>
        <xsl:if test="string-length($vFormula) >0">
          <xsl:variable name="vFormulaWidth">
            <xsl:choose>
              <xsl:when test="($vIsLandscape = 'true')">
                <xsl:value-of select="$gPageA4LandscapeFormulaWidth" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$gPageA4VerticalFormulaWidth" />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <!-- Display first paragraph ( introduction text) -->
          <xsl:call-template name="DisplayStatementTitle">
            <xsl:with-param name="pTitle" select="$vFormula" />
            <xsl:with-param name="pTitleFontSize" select="$gFormulaFontSize" />
            <xsl:with-param name="pTitleFontWeight" select="$gFormulaFontWeight" />
            <xsl:with-param name="pTitleTextAlign" select="$gFormulaTextAlign" />
            <xsl:with-param name="pTitleWidth" select="$vFormulaWidth" />
            <xsl:with-param name="pTitlePaddingTop" select="$gFormulaPaddingTop" />
            <xsl:with-param name="pTitleLeftMargin" select="$gFormulaLeftMargin" />
          </xsl:call-template>
        </xsl:if>

        <xsl:variable name="varDisplayCreditNoteFees">
          <xsl:choose>
            <xsl:when test="($varIsAmendedInvoice = 'true') and ($varEnableCreditNoteVisualization = 'true')">
              <xsl:value-of select="true()" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="false()" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <!-- The detailed fees use different data set and data format for producing 
					Invoice (original and amended) and Credit Note (credit note and additional invoice) -->
        <xsl:call-template name="displayDetailedFees">
          <xsl:with-param name="pCreditNoteVisualization"       select="$varDisplayCreditNoteFees" />
          <xsl:with-param name="pEnableCreditNoteVisualization" select="$varEnableCreditNoteVisualization" />
        </xsl:call-template>
        <!-- PLEASE DO NOT REMOVE THE FOLLOWING BLOCK -->
        <!-- It is used for getting last page number (cfr. <fo:page-number-citation ref-id="LastPage"/) -->
        <!-- Document serie structure:
					   when you are printing an Invoice the only doc you produce is the Invoice itself, but if you are printing
					   a credit note or an additional invoice, then you have to produce either the Credit Note either the relative
					   Amended Invoice
					   
					   1. Invoice
					   2. Credit Note -> Amended Invoice
					   3. Additional Invoice -> Amended Invoice 
					   
					   Conseauently we need to signal the last page when we print the Invoice and the Amended Invoice
				-->
        <xsl:if test="($varIsAmendedInvoice = 'false') or ($varEnableCreditNoteVisualization = 'false')">
          <fo:block id="LastPage" />
        </xsl:if>
      </fo:flow>
    </fo:page-sequence>
    <!--==========================================================================================-->
    <!--END REGION Pages detailed fees                                                            -->
    <!--==========================================================================================-->
  </xsl:template>

  <!--==========================================================================================-->
  <!-- TEMPLATE : displayInvoiceSummary                                                         -->
  <!--            display invoice summary (total amounts)                                       -->
  <!--==========================================================================================-->
  <xsl:template name="displayInvoiceSummary">
    <xsl:param name="pIsCreditNoteVisualization" />
    <!-- EG 20160404 Migration vs2013 -->
    <fo:block border="1pt solid black" space-before="2.5mm" space-after="2.5mm" font-size="{$varInvoiceSummaryFontSize}">
      <xsl:variable name="varInvoiceSummaryTableWidth">
        <xsl:value-of select="number ($varInvoiceSummaryTextWidth) + number ($varInvoiceSummaryAmountWidth) + number ($varInvoiceSummaryCurrencyWidth)" />
      </xsl:variable>
      <!-- EG 20160404 Migration vs2013 -->
      <fo:table margin-left="{$varInvoiceSummaryLeftMargin}" width="{$varInvoiceSummaryTableWidth}mm" table-layout="fixed">
        <fo:table-column column-width="{$varInvoiceSummaryTextWidth}mm" column-number="01" />
        <fo:table-column column-width="{$varInvoiceSummaryAmountWidth}mm" column-number="02" />
        <fo:table-column column-width="{$varInvoiceSummaryCurrencyWidth}mm" column-number="03" />
        <fo:table-body>
          <!-- Total gross -->
          <xsl:if test="($varIsAmendedInvoice = 'false') or ($pIsCreditNoteVisualization = false())">
            <xsl:call-template name="displaySummaryLineAmount">
              <xsl:with-param name="pIsCreditNoteVisualization" select="$pIsCreditNoteVisualization" />
              <xsl:with-param name="pResourceName" select="'INV-GrossTurnOverAmount'" />
              <xsl:with-param name="pAmount" select="$varGrossTurnOverAmount" />
              <xsl:with-param name="pTheoricAmount" select="$varTheoricGrossTurnOverAmount" />
              <xsl:with-param name="pCurrency" select="$varGrossTurnOverCurrency" />
            </xsl:call-template>
            <!-- Total rebate amount -->
            <xsl:if test="$varRebateAmount>0">
              <xsl:call-template name="displaySummaryLineAmount">
                <xsl:with-param name="pIsCreditNoteVisualization" select="$pIsCreditNoteVisualization" />
                <xsl:with-param name="pResourceName" select="'INV-RebateAmount'" />
                <xsl:with-param name="pAmount" select="$varRebateAmount" />
                <xsl:with-param name="pTheoricAmount" select="$varTheoricRebateAmount" />
                <xsl:with-param name="pCurrency" select="$varRebateCurrency" />
                <xsl:with-param name="pBorderBottom" select="'0.5pt solid black'" />
              </xsl:call-template>
              <xsl:variable name="varTotalBrokerageAmount">
                <xsl:value-of select="number($varGrossTurnOverAmount) - number($varRebateAmount)" />
              </xsl:variable>
              <xsl:variable name="varTotalTheoricBrokerageAmount">
                <xsl:value-of select="number($varTheoricGrossTurnOverAmount) - number($varTheoricRebateAmount)" />
              </xsl:variable>
              <xsl:call-template name="displaySummaryLineSpace">
                <xsl:with-param name="pBorderTop" select="'0.5pt solid black'" />
              </xsl:call-template>
              <xsl:call-template name="displaySummaryLineAmount">
                <xsl:with-param name="pIsCreditNoteVisualization" select="$pIsCreditNoteVisualization" />
                <xsl:with-param name="pResourceName" select="'INV-TotalAmount'" />
                <xsl:with-param name="pAmount" select="$varTotalBrokerageAmount" />
                <xsl:with-param name="pTheoricAmount" select="$varTotalTheoricBrokerageAmount" />
                <xsl:with-param name="pCurrency" select="$varGrossTurnOverCurrency" />
              </xsl:call-template>
            </xsl:if>
          </xsl:if>
          <!-- Total tax amount -->
          <xsl:if test="$varTaxAmount>0">
            <xsl:variable name="varAdditionalLabel">
              (<xsl:value-of select="normalize-space($varTaxRate)" />%)
            </xsl:variable>
            <xsl:call-template name="displaySummaryLineAmount">
              <xsl:with-param name="pIsCreditNoteVisualization" select="$pIsCreditNoteVisualization" />
              <xsl:with-param name="pResourceName" select="'INV-TaxAmount'" />
              <xsl:with-param name="pAmount" select="$varTaxAmount" />
              <xsl:with-param name="pTheoricAmount" select="$varTheoricTaxAmount" />
              <xsl:with-param name="pCurrency" select="$varTaxCurrency" />
              <xsl:with-param name="pAdditionalLabel" select="$varAdditionalLabel" />
              <!-- EG 20160404 Migration vs2013 -->
              <xsl:with-param name="pAdditionalSize" select="'7pt'" />
            </xsl:call-template>
          </xsl:if>
          <!-- Total net amount -->
          <xsl:call-template name="displaySummaryLineAmount">
            <xsl:with-param name="pIsCreditNoteVisualization" select="$pIsCreditNoteVisualization" />
            <xsl:with-param name="pResourceName" select="'INV-NetTurnOverAmount'" />
            <xsl:with-param name="pAmount" select="$varNetTurnOverAmount" />
            <xsl:with-param name="pTheoricAmount" select="$varTheoricNetTurnOverAmount" />
            <xsl:with-param name="pCurrency" select="$varNetTurnOverCurrency" />
            <xsl:with-param name="pBorderBottom" select="'.5pt solid black'" />
          </xsl:call-template>
          <!-- Total net issue amount-->
          <xsl:variable name="varLabelIssueRate">
            <xsl:choose>
              <xsl:when test="normalize-space($varNetTurnOverIssueCurrency) != normalize-space($varNetTurnOverCurrency)">
                <xsl:call-template name="displayExchangeRate">
                  <xsl:with-param name="prefix" select="'issue'" />
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="''" />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:call-template name="displaySummaryLineAmount">
            <xsl:with-param name="pIsCreditNoteVisualization" select="$pIsCreditNoteVisualization" />
            <xsl:with-param name="pResourceName" select="'INV-TotalToPay'" />
            <xsl:with-param name="pAmount" select="$varNetTurnOverIssueAmount" />
            <xsl:with-param name="pTheoricAmount" select="$varTheoricNetTurnOverIssueAmount" />
            <xsl:with-param name="pCurrency" select="$varNetTurnOverIssueCurrency" />
            <xsl:with-param name="pTextAlign" select="'right'" />
            <xsl:with-param name="pBackGroundColor" select="'#DCDCDC'" />
            <xsl:with-param name="pBorderTop" select="'.5pt solid black'" />
            <xsl:with-param name="pWeight" select="'bold'" />
            <xsl:with-param name="pAdditionalLabel" select="$varLabelIssueRate" />
            <!-- EG 20160404 Migration vs2013 -->
            <xsl:with-param name="pAdditionalSize" select="'7pt'" />
          </xsl:call-template>
        </fo:table-body>
      </fo:table>
    </fo:block>
  </xsl:template>
  <!--==========================================================================================-->
  <!-- TEMPLATE : displaySummaryLineAmount                                                      -->
  <!--            Label & Amount & Currency for each line on SUMMARY                            -->
  <!--==========================================================================================-->
  <xsl:template name="displaySummaryLineAmount">
    <xsl:param name="pResourceName" />
    <xsl:param name="pIsCreditNoteVisualization" />
    <xsl:param name="pAmount" />
    <xsl:param name="pTheoricAmount" />
    <xsl:param name="pCurrency" />
    <xsl:param name="pAdditionalLabel" />
    <xsl:param name="pAdditionalSize" select="$varInvoiceSummaryFontSize" />
    <xsl:param name="pAdditionalPos" select="'after'" />
    <xsl:param name="pTextAlign" select="$varTextAlign" />
    <xsl:param name="pBorderTop"    select="$varTableBorderDebug" />
    <xsl:param name="pBorderBottom" select="$varTableBorderDebug" />
    <xsl:param name="pBackGroundColor" select="'white'" />
    <!-- EG 20160404 Migration vs2013 -->
    <xsl:param name="pHeight" select="'5mm'" />
    <xsl:param name="pWeight" select="'normal'" />
    <!-- EG 20160404 Migration vs2013 -->
    <fo:table-row height="{$pHeight}" display-align="center">
      <!-- EG 20160404 Migration vs2013 -->
      <fo:table-cell margin-left="{$varInvoiceSummaryLeftMargin}" background-color="{$pBackGroundColor}" border-top="{$pBorderTop}" border-bottom="{$pBorderBottom}" text-align="{$pTextAlign}">
        <fo:block font-weight="{$pWeight}">
          <xsl:if test="$pAdditionalPos='before'">
            <!-- EG 20160404 Migration vs2013 -->
            <fo:inline font-weight="normal" font-size="{$pAdditionalSize}">
              <xsl:value-of select="$pAdditionalLabel" />
            </fo:inline>
          </xsl:if>
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="$pResourceName" />
          </xsl:call-template>
          <xsl:if test="$pAdditionalPos='after'">
            <!-- EG 20160404 Migration vs2013 -->
            <fo:inline font-weight="normal" font-size="{$pAdditionalSize}">
              <xsl:value-of select="$pAdditionalLabel" />
            </fo:inline>
          </xsl:if>
        </fo:block>
      </fo:table-cell>
      <!-- EG 20160404 Migration vs2013 -->
      <fo:table-cell margin-left="{$varInvoiceSummaryLeftMargin}" margin-right="{$varInvoiceSummaryRightMargin}" background-color="{$pBackGroundColor}" border-top="{$pBorderTop}" border-bottom="{$pBorderBottom}" text-align="{$varAmountTextAlign}">
        <fo:block font-weight="{$pWeight}">
          <!-- case invoice/credit note summary -->
          <xsl:if test="$varIsInvoice = 'true' or $pIsCreditNoteVisualization">
            <xsl:call-template name="format-money2">
              <xsl:with-param name="amount" select="normalize-space($pAmount)" />
            </xsl:call-template>
          </xsl:if>
          <!-- case amended invoice summary -->
          <xsl:if test="$varIsAmendedInvoice = 'true' and $pIsCreditNoteVisualization = false()">
            <xsl:call-template name="format-money2">
              <xsl:with-param name="amount" select="normalize-space($pTheoricAmount)" />
            </xsl:call-template>
          </xsl:if>
        </fo:block>
      </fo:table-cell>
      <xsl:call-template name="displaySummaryCurrency">
        <xsl:with-param name="pCurrency" select="$pCurrency" />
        <xsl:with-param name="pBorderTop" select="$pBorderTop" />
        <xsl:with-param name="pBorderBottom" select="$pBorderBottom" />
        <xsl:with-param name="pBackGroundColor" select="$pBackGroundColor" />
        <xsl:with-param name="pWeight" select="$pWeight" />
      </xsl:call-template>
    </fo:table-row>
  </xsl:template>
  <!--==========================================================================================-->
  <!-- TEMPLATE : displaySummaryCurrency                                                        -->
  <!--            Currency for each line on SUMMARY                                             -->
  <!--==========================================================================================-->
  <xsl:template name="displaySummaryCurrency">
    <xsl:param name="pCurrency" />
    <xsl:param name="pBorderTop"    select="$varTableBorderDebug" />
    <xsl:param name="pBorderBottom" select="$varTableBorderDebug" />
    <xsl:param name="pBackGroundColor" select="'white'" />
    <xsl:param name="pWeight"       select="'normal'" />
    <!-- EG 20160404 Migration vs2013 -->
    <fo:table-cell margin-left="{$varInvoiceSummaryLeftMargin}" background-color="{$pBackGroundColor}" border-top="{$pBorderTop}" border-bottom="{$pBorderBottom}" text-align="left">
      <fo:block font-weight="{$pWeight}">
        <xsl:value-of select="normalize-space($pCurrency)" />
      </fo:block>
    </fo:table-cell>
  </xsl:template>
  <!--==========================================================================================-->
  <!-- TEMPLATE : displaySummaryLineSpace                                                       -->
  <!--            Space Line on SUMMARY                                                         -->
  <!--==========================================================================================-->
  <xsl:template name="displaySummaryLineSpace">
    <xsl:param name="pBorderTop"    select="$varTableBorderDebug" />
    <xsl:param name="pBorderBottom" select="$varTableBorderDebug" />
    <xsl:param name="pBackGroundColor" select="'white'" />
    <fo:table-row>
      <fo:table-cell number-columns-spanned="3" background-color="{$pBackGroundColor}" border-top="{$pBorderTop}" border-bottom="{$pBorderBottom}">
        <fo:block text-align="{$varTextAlign}">
          <xsl:value-of select="$varEspace" />
        </fo:block>
      </fo:table-cell>
    </fo:table-row>
  </xsl:template>
  <!--==========================================================================================-->
  <!-- TEMPLATE : displayBankAccount                                                            -->
  <!--            display bank account information for settlement                               -->
  <!--==========================================================================================-->
  <xsl:template name="displayBankAccount">
    <xsl:variable name="varSettlementInstruction" select="msxsl:node-set($varSiReceiver)/SIXML/EfsSettlementInstruction" />

    <xsl:if test="$varSettlementInstruction">
      <!-- EG 20160404 Migration vs2013 -->
      <fo:block margin-left="{$varReglementTextLeftMargin}" padding-top="{$varReglementTextPaddingTop}" font-size="{$varReglementTextFontSize}">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'INV-ReglementText'" />
        </xsl:call-template>
      </fo:block>
      <xsl:variable name="varBankAccountTableWidth">
        <xsl:value-of select="number ($varBankAccountTextCellWidth) + number ($varBankAccountDataCellWidth)" />
      </xsl:variable>
      <!-- EG 20160404 Migration vs2013 -->
      <fo:table width="{$varBankAccountTableWidth}mm" padding-top="{$varBankAccountPaddingTop}" table-layout="fixed" font-size="{$varBankAccountFontSize}">
        <fo:table-column column-width="{$varBankAccountTextCellWidth}mm" column-number="01" />
        <fo:table-column column-width="{$varBankAccountDataCellWidth}mm" column-number="02" />

        <xsl:variable name="varSettlementMethod" select="$varSettlementInstruction/settlementMethod" />
        <xsl:variable name="varPaymentText">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="concat('INV-',$varSettlementMethod)" />
          </xsl:call-template>
        </xsl:variable>

        <!-- Banque -->
        <xsl:variable name="varSIBankRoutingName" select="$varSettlementInstruction/beneficiaryBank/routingIdsAndExplicitDetails/routingName"/>
        <xsl:variable name="varSIBankAddress1" select="$varSettlementInstruction/beneficiaryBank/routingIdsAndExplicitDetails/routingAddress/streetAddress/streetLine[1]"/>
        <xsl:variable name="varSIBankAddress2" select="$varSettlementInstruction/beneficiaryBank/routingIdsAndExplicitDetails/routingAddress/streetAddress/streetLine[2]"/>
        <xsl:variable name="varSIBankAddress3" select="$varSettlementInstruction/beneficiaryBank/routingIdsAndExplicitDetails/routingAddress/streetAddress/streetLine[3]"/>
        <xsl:variable name="varSIBankAddress4" select="$varSettlementInstruction/beneficiaryBank/routingIdsAndExplicitDetails/routingAddress/streetAddress/streetLine[4]"/>
        <xsl:variable name="varSIBankCity" select="$varSettlementInstruction/beneficiaryBank/routingIdsAndExplicitDetails/routingAddress/city"/>
        <xsl:variable name="varSIBankPostalCode" select="$varSettlementInstruction/beneficiaryBank/routingIdsAndExplicitDetails/routingAddress/postalCode"/>
        <xsl:variable name="varSIBankCountry" select="$varSettlementInstruction/beneficiaryBank/routingIdsAndExplicitDetails/routingAddress/country"/>
        <xsl:variable name="varSIRoutingAccountIdent" select="$varSettlementInstruction/beneficiaryBank/routingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/accountNumberIdent']"/>
        <!-- Account number -->
        <xsl:variable name="varSIRoutingAccountNumber" select="$varSettlementInstruction/beneficiaryBank/routingIdsAndExplicitDetails/routingAccountNumber"/>
        <!-- Swift Address -->
        <xsl:variable name="varSIAddressSWIFT" select="$varSettlementInstruction/beneficiaryBank/routingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/actorBic']"/>

        <!-- Bank -->
        <fo:table-body>
          <xsl:choose>
            <xsl:when test="$varPaymentText">
              <xsl:call-template name="displaySummaryLineAccount">
                <xsl:with-param name="pResourceName" select="concat('INV-',$varSettlementMethod)" />
                <xsl:with-param name="pValue" select="$varSIBankRoutingName" />
              </xsl:call-template>
              <xsl:if test="$varSIBankAddress1">
                <xsl:call-template name="displaySummaryLineAccount">
                  <xsl:with-param name="pValue" select="$varSIBankAddress1" />
                </xsl:call-template>
              </xsl:if>
              <xsl:if test="$varSIBankAddress2">
                <xsl:call-template name="displaySummaryLineAccount">
                  <xsl:with-param name="pValue" select="$varSIBankAddress2" />
                </xsl:call-template>
              </xsl:if>
              <xsl:if test="$varSIBankAddress3">
                <xsl:call-template name="displaySummaryLineAccount">
                  <xsl:with-param name="pValue" select="$varSIBankAddress3" />
                </xsl:call-template>
              </xsl:if>
              <xsl:if test="$varSIBankAddress4">
                <xsl:call-template name="displaySummaryLineAccount">
                  <xsl:with-param name="pValue" select="$varSIBankAddress4" />
                </xsl:call-template>
              </xsl:if>
              <fo:table-row>
                <fo:table-cell number-columns-spanned="2">
                  <fo:block text-align="{$varTextAlign}">
                    <xsl:value-of select="$varEspace" />
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <xsl:call-template name="displaySummaryLineAccount">
                <xsl:with-param name="pResourceName" select="$varSIRoutingAccountIdent" />
                <xsl:with-param name="pValue" select="$varSIRoutingAccountNumber" />
              </xsl:call-template>
              <xsl:if test="$varSIAddressSWIFT">
                <xsl:call-template name="displaySummaryLineAccount">
                  <xsl:with-param name="pResourceName" select="'INV-Swift'" />
                  <xsl:with-param name="pValue" select="$varSIAddressSWIFT" />
                </xsl:call-template>
              </xsl:if>

            </xsl:when>

            <xsl:otherwise>
              <xsl:call-template name="displaySummaryLineAccount">
                <xsl:with-param name="pResourceName" select="'INV-SettlementMethod'" />
                <xsl:with-param name="pValue" select="$varSettlementMethod" />
              </xsl:call-template>
              <xsl:call-template name="displaySummaryLineAccount">
                <xsl:with-param name="pResourceName" select="'INV-Bank'" />
                <xsl:with-param name="pValue" select="$varSIBankRoutingName" />
              </xsl:call-template>
              <xsl:call-template name="displaySummaryLineAccount">
                <xsl:with-param name="pResourceName" select="$varSIRoutingAccountIdent" />
                <xsl:with-param name="pValue" select="$varSIRoutingAccountNumber" />
              </xsl:call-template>
              <xsl:call-template name="displaySummaryLineAccount">
                <xsl:with-param name="pResourceName" select="'INV-Swift'" />
                <xsl:with-param name="pValue" select="$varSIAddressSWIFT" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </fo:table-body>
      </fo:table>
    </xsl:if>
  </xsl:template>
  <!--==========================================================================================-->
  <!-- TEMPLATE : displaySummaryLineAccount                                                     -->
  <!--            Label & Value for each line account on SUMMARY                                -->
  <!--==========================================================================================-->
  <xsl:template name="displaySummaryLineAccount">
    <xsl:param name="pResourceName" />
    <xsl:param name="pValue" />
    <xsl:param name="pWeight" select="'normal'" />
    <fo:table-row>
      <!-- EG 20160404 Migration vs2013 -->
      <fo:table-cell margin-left="{$varBankAccountTextLeftMargin}" border="{$varTableBorderDebug}">
        <xsl:if test="$pResourceName">
          <fo:block>
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="$pResourceName" />
            </xsl:call-template>
            <xsl:value-of select="$varEspace" />:
          </fo:block>
        </xsl:if>
      </fo:table-cell>
      <fo:table-cell text-align="{$varBankAccountDataTextAlign}" border="{$varTableBorderDebug}">
        <fo:block>
          <xsl:value-of select="$pValue" />
        </fo:block>
      </fo:table-cell>
    </fo:table-row>
  </xsl:template>
  <!--==========================================================================================-->
  <!-- TEMPLATE : createDetailedFeesTable                                                       -->
  <!--            create trade fee columns for Invoice and Amended Invoice                      -->
  <!--==========================================================================================-->
  <xsl:template name="createDetailedFeesTable">
    <!-- Trade identifier -->
    <fo:table-column column-width="{$varDetFeesTableCol1Width}mm" column-number="01" />
    <!-- Trade date -->
    <fo:table-column column-width="{$varDetFeesTableCol2Width}mm" column-number="02" />
    <!-- Expiry date -->
    <fo:table-column column-width="{$varDetFeesTableCol3Width}mm" column-number="03" />
    <!-- Value date -->
    <fo:table-column column-width="{$varDetFeesTableCol4Width}mm" column-number="04" />
    <!-- Trade currency -->
    <fo:table-column column-width="{$varDetFeesTableCol5Width}mm" column-number="05" />
    <!-- Rate -->
    <fo:table-column column-width="{$varDetFeesTableCol6Width}mm" column-number="06" />
    <!-- Buyer seller -->
    <fo:table-column column-width="{$varDetFeesTableCol7Width}mm" column-number="07" />
    <!-- Trader -->
    <fo:table-column column-width="{$varDetFeesTableCol8Width}mm" column-number="08" />
    <!-- Counterparty -->
    <fo:table-column column-width="{$varDetFeesTableCol9Width}mm" column-number="09" />
    <!-- Duration -->
    <fo:table-column column-width="{$varDetFeesTableCol10Width}mm" column-number="10" />
    <!-- ISIN code -->
    <fo:table-column column-width="{$varDetFeesTableCol11Width}mm" column-number="11" />
    <!-- Fee schedule -->
    <fo:table-column column-width="{$varDetFeesTableCol12Width}mm" column-number="12" />
    <!-- Fee basis -->
    <fo:table-column column-width="{$varDetFeesTableCol13Width}mm" column-number="13" />
    <!-- Nominal -->
    <fo:table-column column-width="{$varDetFeesTableCol14Width}mm" column-number="14" />
    <!-- Amount -->
    <fo:table-column column-width="{$varDetFeesTableCol15Width}mm" column-number="15" />
    <!-- Countervalue -->
    <fo:table-column column-width="{$varDetFeesTableCol16Width}mm" column-number="16" />
  </xsl:template>
  <!-- Define keys used to group elements without trade sorting -->
  <xsl:key name="keyByInstrumentAndCurrency" match="invoiceFee" use="concat(../../instrument, feeAmount/currency)" />
  <xsl:key name="keyByInstrument" match="invoiceFee" use="../../instrument" />
  <!-- Fees summary using trade values, used to format: 
	     - Credit Note and Additional Invoice 
		   - Invoice and Amended Invoice IF varUseTradeSorting = false()
    -->
  <!--==========================================================================================-->
  <!-- TEMPLATE : displayFeesSummaryWithoutTradeSorting                                         -->
  <!--==========================================================================================-->
  <xsl:template name="displayFeesSummaryWithoutTradeSorting">
    <xsl:param name="lstFeeParam" />
    <xsl:param name="pCreditNoteVisualization" />
    <!-- For each currency of the selected instrument ($lstFeeParam) -->
    <xsl:for-each select="$lstFeeParam[generate-id(.) = generate-id(key('keyByInstrumentAndCurrency', concat(../../instrument, feeAmount/currency))[1])]">
      <xsl:sort select="feeAmount/currency" />
      <xsl:variable name="lngCurrency" select="feeAmount/currency" />
      <xsl:variable name="lngInstrumentCurrency" select="concat(../../instrument, feeAmount/currency)" />
      <xsl:call-template name="Show">
        <xsl:with-param name="concatInstrumentCurrency" select="$lngInstrumentCurrency" />
        <xsl:with-param name="lstFeeParam" select="$lstFeeParam" />
        <xsl:with-param name="lngCurrency" select="$lngCurrency" />
        <xsl:with-param name="pCreditNoteVisualization" select="$pCreditNoteVisualization" />
      </xsl:call-template>
    </xsl:for-each>
  </xsl:template>
  <!--==========================================================================================-->
  <!-- TEMPLATE : displayFeesSummaryWithoutTradeSorting                                         -->
  <!--            Fees summary using trade sorting, used ONLY to format Invoice/Amended Invoice -->
  <!--==========================================================================================-->
  <xsl:template name="displayFeesSummaryUsingTradeSorting">
    <xsl:param name="plstInvoiceTradeReference" />
    <xsl:param name="pCreditNoteVisualization" />
    <xsl:variable name="varTotalAmount">
      <xsl:if test="$varIsInvoice = 'true'">
        <xsl:value-of select="./sum/feeInitialAmount/amount" />
      </xsl:if>
      <xsl:if test="$varIsAmendedInvoice = 'true'">
        <xsl:value-of select="./sum/feeAmount/amount" />
      </xsl:if>
    </xsl:variable>
    <xsl:if test="(($varTotalAmount != 0.0) or $varShowZeroValue)">
      <!-- EG 20160404 Migration vs2013 -->
      <fo:table width="{$varDetFeesTableTotalWidth}" table-layout="fixed">
        <xsl:call-template name="createDetailedFeesTable" />
        <xsl:call-template name="displayHeaderForDetailedFees">
          <xsl:with-param name="AmountCurrency" select="./sum/feeAmount/currency" />
          <xsl:with-param name="CountervalueCurrency" select="./sum/feeAccountingAmount/currency" />
          <xsl:with-param name="plstInvoiceTradeReferenceXHeader" select="$plstInvoiceTradeReference" />
        </xsl:call-template>
        <!-- ===== Scan fees ===== -->
        <xsl:for-each select="$plstInvoiceTradeReference">
          <xsl:variable name="lstNameTrade" select="key($tradeIdKeyName, ./@href)" />
          <xsl:for-each select="$lstNameTrade/invoiceFees/invoiceFee">
            <xsl:variable name="amount">
              <xsl:if test="$varIsInvoice = 'true'">
                <xsl:value-of select="./feeInitialAmount/amount" />
              </xsl:if>
              <xsl:if test="$varIsAmendedInvoice = 'true'">
                <xsl:value-of select="./feeAmount/amount" />
              </xsl:if>
            </xsl:variable>
            <xsl:variable name="accountingAmount">
              <xsl:if test="$varIsInvoice = 'true'">
                <xsl:value-of select="./feeInitialAccountingAmount/amount" />
              </xsl:if>
              <xsl:if test="$varIsAmendedInvoice = 'true'">
                <xsl:value-of select="./feeAccountingAmount/amount" />
              </xsl:if>
            </xsl:variable>
            <xsl:if test="(($amount != 0.0) or $varShowZeroValue)">
              <fo:table-body>
                <!-- EG 20160404 Migration vs2013 -->
                <fo:table-row font-size="{$varDetFeesFontSize}">
                  <!-- ===== Trade identifier ===== -->
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="right">
                    <fo:block>
                      <xsl:value-of select="$lstNameTrade/identifier" />
                    </fo:block>
                  </fo:table-cell>
                  <!-- ===== Trade date ===== -->
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="center">
                    <fo:block>
                      <xsl:call-template name="format-shortdate2">
                        <xsl:with-param name="xsd-date-time" select="$lstNameTrade/tradeDate" />
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <!-- ===== Expiry date (out date in XML file) ===== -->
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="center">
                    <fo:block>
                      <xsl:call-template name="format-shortdate2">
                        <xsl:with-param name="xsd-date-time" select="$lstNameTrade/outDate" />
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <!-- ===== Value date ===== -->
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="center">
                    <fo:block>
                      <xsl:call-template name="format-shortdate2">
                        <xsl:with-param name="xsd-date-time" select="$lstNameTrade/inDate" />
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <!-- ===== Trade currency ===== -->
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="center">
                    <fo:block>
                      <xsl:value-of select="$lstNameTrade/currency[@currencyScheme='http://www.fpml.org/ext/iso4217-2001-08-15']" />
                    </fo:block>
                  </fo:table-cell>
                  <!-- ===== Rate ===== -->
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="center">
                    <fo:block>
                      <xsl:call-template name="format-fixed-rate2">
                        <xsl:with-param name="fixed-rate" select="normalize-space($lstNameTrade/price)" />
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <!-- ===== Buyer seller ===== -->
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="center">
                    <xsl:variable name="varside">
                      <xsl:value-of select="'INV-'" />
                      <xsl:value-of select="normalize-space($lstNameTrade/side)" />
                    </xsl:variable>
                    <fo:block>
                      <xsl:call-template name="getSpheresTranslation">
                        <xsl:with-param name="pResourceName" select="$varside" />
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <!-- ===== Trader ===== -->
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="left">
                    <fo:block>
                      <xsl:variable name="varTraderName">
                        <xsl:value-of select="$lstNameTrade/trader[@traderScheme='http://www.euro-finance-systems.fr/otcml/actorIdentifier']/@name" />
                      </xsl:variable>
                      <xsl:choose>
                        <xsl:when test="string-length( normalize-space( $varTraderName )) &gt; 0">
                          <xsl:value-of select="$varTraderName" />
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="$lstNameTrade/trader[@traderScheme='http://www.euro-finance-systems.fr/otcml/actorIdentifier']" />
                        </xsl:otherwise>
                      </xsl:choose>
                    </fo:block>
                  </fo:table-cell>
                  <!-- ===== Counterparty ===== -->
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="left">
                    <fo:block>
                      <xsl:value-of select="//dataDocument/party[@id = $lstNameTrade/counterpartyPartyReference/@href]/partyName" />
                    </fo:block>
                  </fo:table-cell>
                  <!-- ===== Duration ===== -->
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="center">
                    <fo:block>
                      <xsl:choose>
                        <xsl:when test="string-length( normalize-space( ./feeSchedule/duration )) &gt; 0">
                          <xsl:value-of select="./feeSchedule/duration" />
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="$lstNameTrade/periodNumberOfDays" />
                        </xsl:otherwise>
                      </xsl:choose>
                    </fo:block>
                  </fo:table-cell>
                  <!-- ===== ISIN ===== -->
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="left">
                    <fo:block>
                      <xsl:variable name="varISIN">
                        <xsl:call-template name="GetIsinCode">
                          <xsl:with-param name="pISINCode" select="$lstNameTrade/asset/instrumentId[@instrumentIdScheme='http://www.euro-finance-systems.fr/spheres-enum/FIX/SecurityIDSource?V=4&amp;EV=ISIN']" />
                        </xsl:call-template>
                      </xsl:variable>
                      <xsl:variable name="varDescription" select="$lstNameTrade/asset/description" />
                      <xsl:choose>
                        <xsl:when test="string-length( normalize-space( $varISIN )) &gt; 0">
                          <xsl:value-of select="$varISIN" />
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="$varDescription" />
                        </xsl:otherwise>
                      </xsl:choose>
                    </fo:block>
                  </fo:table-cell>
                  <!-- ===== Fee schedule ===== -->
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="right">
                    <fo:block>
                      <xsl:value-of select="./feeSchedule/identifier" />
                    </fo:block>
                  </fo:table-cell>
                  <!-- ===== Fee basis ===== -->
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="right">
                    <fo:block>
                      <xsl:if test="string-length( normalize-space( ./feeSchedule/assessmentBasisValue1 )) &gt; 0">
                        <xsl:call-template name="format-money2">
                          <xsl:with-param name="amount" select="concat(./feeSchedule/assessmentBasisValue1,'')" />
                        </xsl:call-template>
                      </xsl:if>
                    </fo:block>
                  </fo:table-cell>
                  <!-- ===== Nominal ===== -->
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="right">
                    <fo:block>
                      <xsl:variable name="varNominal">
                        <xsl:choose>
                          <xsl:when test="$lstNameTrade/instrument = 'REPO'">
                            <xsl:value-of select="$lstNameTrade/asset/notionalAmount/amount" />
                          </xsl:when>
                          <xsl:otherwise>
                            <xsl:value-of select="$lstNameTrade/notionalAmount/amount" />
                          </xsl:otherwise>
                        </xsl:choose>
                      </xsl:variable>
                      <xsl:call-template name="format-money2">
                        <xsl:with-param name="amount" select="concat($varNominal,'')" />
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <!-- ===== Fee amount ===== -->
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="right">
                    <fo:block>
                      <xsl:call-template name="format-money2">
                        <xsl:with-param name="amount" select="concat($amount,'')" />
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <!-- ===== Countervalue amount ===== -->
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="right">
                    <fo:block>
                      <xsl:call-template name="format-money2">
                        <xsl:with-param name="amount" select="concat($accountingAmount,'')" />
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </fo:table-body>
            </xsl:if>
          </xsl:for-each>
        </xsl:for-each>
        <!-- ===== fees summary ===== -->
        <fo:table-body>
          <!-- ===== Subtotal for every instrument and fee currency ===== -->
          <fo:table-row font-size="7pt" text-align="right">
            <xsl:call-template name="createEmptyCellsForDetailedFees">
              <xsl:with-param name="pnNumberOfCells" select="13" />
            </xsl:call-template>
            <!-- ===== Fee basis ===== -->
            <!-- EG 20160404 Migration vs2013 -->
            <fo:table-cell font-weight="bold" padding="{$varDetailedFeesCellPadding}">
              <fo:block>
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'INV-SubTotal'" />
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <!-- ===== Fee amount ===== -->
            <!-- EG 20160404 Migration vs2013 -->
            <fo:table-cell font-weight="bold" padding="{$varDetailedFeesCellPadding}">
              <fo:block>
                <xsl:if test="$varIsInvoice = 'true'">
                  <xsl:call-template name="format-money2">
                    <xsl:with-param name="amount" select="./sum/feeInitialAmount/amount" />
                  </xsl:call-template>
                </xsl:if>
                <xsl:if test="$varIsAmendedInvoice = 'true'">
                  <xsl:call-template name="format-money2">
                    <xsl:with-param name="amount" select="./sum/feeAmount/amount" />
                  </xsl:call-template>
                </xsl:if>
              </fo:block>
            </fo:table-cell>
            <!-- ===== Countervalue ===== -->
            <!-- EG 20160404 Migration vs2013 -->
            <fo:table-cell font-weight="bold" padding="{$varDetailedFeesCellPadding}">
              <fo:block>
                <xsl:if test="$varIsInvoice = 'true'">
                  <xsl:call-template name="format-money2">
                    <xsl:with-param name="amount" select="./sum/feeInitialAccountingAmount/amount" />
                  </xsl:call-template>
                </xsl:if>
                <xsl:if test="$varIsAmendedInvoice = 'true'">
                  <xsl:call-template name="format-money2">
                    <xsl:with-param name="amount" select="./sum/feeAccountingAmount/amount" />
                  </xsl:call-template>
                </xsl:if>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
          <!-- Empty row after subtotal rows -->
          <!-- EG 20160404 Migration vs2013 -->
          <fo:table-row height="5mm">
            <xsl:call-template name="createEmptyCellsForDetailedFees">
              <xsl:with-param name="pnNumberOfCells" select="1" />
            </xsl:call-template>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </xsl:if>
  </xsl:template>

  <!--==========================================================================================-->
  <!-- TEMPLATE : Show                                                                          -->
  <!--            called for each instrument and currency                                       -->
  <!--==========================================================================================-->
  <xsl:template name="Show">
    <xsl:param name="concatInstrumentCurrency" />
    <xsl:param name="lstFeeParam" />
    <xsl:param name="lngCurrency" />
    <xsl:param name="pCreditNoteVisualization" />
    <!-- Scan fees for credit note additional invoice -->
    <xsl:if test="$pCreditNoteVisualization = 'true'">
      <fo:table table-layout="fixed" width="100%">
        <xsl:call-template name="createCreditNoteDetailedFeesTable" />
        <xsl:variable name="varTotalBaseAmount" select="sum($lstFeeParam[feeAmount/currency=$lngCurrency]/feeBaseAmount/amount)" />
        <xsl:variable name="varTotalAmount" select="sum($lstFeeParam[feeAmount/currency=$lngCurrency]/feeAmount/amount)" />
        <xsl:if test="($varTotalAmount - $varTotalBaseAmount)!=0">
          <xsl:call-template name="displayHeaderForCreditNoteDetailedFees">
            <xsl:with-param name="AmountCurrency" select="./feeAmount/currency" />
            <xsl:with-param name="AccountingAmountCurrency" select="./feeAccountingAmount/currency" />
          </xsl:call-template>
          <!-- ===== Scan fees ===== -->
          <xsl:for-each select="//invoiceFee">
            <xsl:variable name="currentInstrumentCurrency" select="concat(../../instrument, feeAmount/currency)" />
            <xsl:variable name="currentAmount" select="./feeAmount/amount" />
            <xsl:variable name="varFeeInitialAmount" select="./feeInitialAmount/amount" />
            <xsl:variable name="varFeeBaseAmount" select="./feeBaseAmount/amount" />
            <xsl:variable name="varFeeAmount" select="./feeAmount/amount" />
            <xsl:variable name="deltaAmount" select="$varFeeAmount - $varFeeBaseAmount" />
            <xsl:if test="($currentInstrumentCurrency = $concatInstrumentCurrency) and ($deltaAmount != 0.0)">
              <fo:table-body>
                <!-- EG 20160404 Migration vs2013 -->
                <fo:table-row font-size="{$varDetFeesFontSize}">
                  <!-- ===== Trade identifier ===== -->
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-cell column-number="2" padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="left">
                    <fo:block>
                      <xsl:value-of select="./../../identifier" />
                    </fo:block>
                  </fo:table-cell>
                  <!-- ===== Trade date ===== -->
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-cell column-number="3" padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="center">
                    <fo:block>
                      <xsl:call-template name="format-shortdate2">
                        <xsl:with-param name="xsd-date-time" select="./../../tradeDate" />
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <xsl:variable name="varFeeInitialAccountingAmount" select="./feeInitialAccountingAmount/amount" />
                  <xsl:variable name="varFeeBaseAccountingAmount" select="./feeBaseAccountingAmount/amount" />
                  <xsl:variable name="varFeeAccountingAmount" select="./feeAccountingAmount/amount" />
                  <!-- ===== Previous amount ===== -->
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-cell column-number="4" padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="right">
                    <fo:block>
                      <xsl:call-template name="format-money2">
                        <xsl:with-param name="amount" select="$varFeeBaseAmount" />
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <!-- ===== Previous accounting amount ===== -->
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-cell column-number="5" padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="right">
                    <fo:block>
                      <xsl:call-template name="format-money2">
                        <xsl:with-param name="amount" select="$varFeeBaseAccountingAmount" />
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <!-- ===== Actual or Amended amount ===== -->
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-cell column-number="6" padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="right">
                    <fo:block>
                      <xsl:call-template name="format-money2">
                        <xsl:with-param name="amount" select="$varFeeAmount" />
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <!-- ===== Actual or Amended accounting amount ===== -->
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-cell column-number="7" padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="right">
                    <fo:block>
                      <xsl:call-template name="format-money2">
                        <xsl:with-param name="amount" select="$varFeeAccountingAmount" />
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <!-- ===== Delta amount ===== -->
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-cell column-number="8" padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="right">
                    <fo:block>
                      <xsl:call-template name="format-money2">
                        <xsl:with-param name="amount" select="$varFeeAmount - $varFeeBaseAmount" />
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <!-- ===== Delta accounting amount ===== -->
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-cell column-number="9" padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="right">
                    <fo:block>
                      <xsl:call-template name="format-money2">
                        <xsl:with-param name="amount" select="$varFeeAccountingAmount - $varFeeBaseAccountingAmount" />
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </fo:table-body>
            </xsl:if>
          </xsl:for-each>
          <fo:table-body>
            <!-- ===== Subtotal for every instrument and fee currency ===== -->
            <fo:table-row font-size="7pt" text-align="right">
              <!--<xsl:call-template name="createEmptyCellsForDetailedFees">
								<xsl:with-param name="pnNumberOfCells" select="1" />
							</xsl:call-template>-->
              <!-- ===== Fee basis ===== -->
              <!-- EG 20160404 Migration vs2013 -->
              <fo:table-cell column-number="3" font-weight="bold" padding="{$varDetailedFeesCellPadding}">
                <fo:block>
                  <xsl:call-template name="getSpheresTranslation">
                    <xsl:with-param name="pResourceName" select="'INV-SubTotal'" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <xsl:variable name="varAmendedFees" select="$lstFeeParam[feeAmount/currency=$lngCurrency and feeAmount/amount!=feeBaseAmount/amount]" />
              <xsl:variable name="varTotAmendedBaseAmount" select="sum($varAmendedFees/feeBaseAmount/amount)" />
              <xsl:variable name="varTotAmendedBaseAccountingAmount" select="sum($varAmendedFees/feeBaseAccountingAmount/amount)" />
              <xsl:variable name="varTotAmendedAmount" select="sum($varAmendedFees/feeAmount/amount)" />
              <xsl:variable name="varTotAmendedAccountingAmount" select="sum($varAmendedFees/feeAccountingAmount/amount)" />
              <!-- ===== Previous amount ===== -->
              <!-- EG 20160404 Migration vs2013 -->
              <fo:table-cell column-number="4" font-weight="bold" padding="{$varDetailedFeesCellPadding}">
                <fo:block>
                  <xsl:call-template name="format-money2">
                    <xsl:with-param name="amount" select="$varTotAmendedBaseAmount" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <!-- ===== Previous accounting amount ===== -->
              <!-- EG 20160404 Migration vs2013 -->
              <fo:table-cell column-number="5" font-weight="bold" padding="{$varDetailedFeesCellPadding}">
                <fo:block>
                  <xsl:call-template name="format-money2">
                    <xsl:with-param name="amount" select="$varTotAmendedBaseAccountingAmount" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <!-- ===== Actual or Amended amount ===== -->
              <!-- EG 20160404 Migration vs2013 -->
              <fo:table-cell column-number="6" font-weight="bold" padding="{$varDetailedFeesCellPadding}">
                <fo:block>
                  <xsl:call-template name="format-money2">
                    <xsl:with-param name="amount" select="$varTotAmendedAmount" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <!-- ===== Actual or Amended accounting amount ===== -->
              <!-- EG 20160404 Migration vs2013 -->
              <fo:table-cell column-number="7" font-weight="bold" padding="{$varDetailedFeesCellPadding}">
                <fo:block>
                  <xsl:call-template name="format-money2">
                    <xsl:with-param name="amount" select="$varTotAmendedAccountingAmount" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <!-- ===== Delta amount ===== -->
              <!-- EG 20160404 Migration vs2013 -->
              <fo:table-cell column-number="8" font-weight="bold" padding="{$varDetailedFeesCellPadding}">
                <fo:block>
                  <xsl:call-template name="format-money2">
                    <xsl:with-param name="amount" select="$varTotAmendedAmount - $varTotAmendedBaseAmount" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <!-- ===== Delta accounting amount ===== -->
              <!-- EG 20160404 Migration vs2013 -->
              <fo:table-cell column-number="9" font-weight="bold" padding="{$varDetailedFeesCellPadding}">
                <fo:block>
                  <xsl:call-template name="format-money2">
                    <xsl:with-param name="amount" select="$varTotAmendedAccountingAmount - $varTotAmendedBaseAccountingAmount" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
          </fo:table-body>
        </xsl:if>
      </fo:table>
    </xsl:if>
    <!-- Scan fees for Invoice or Amended Invoice (called IFF varUseTradeSorting = false()) -->
    <xsl:if test="$pCreditNoteVisualization = 'false'">
      <!-- EG 20160404 Migration vs2013 -->
      <fo:table width="{$varDetFeesTableTotalWidth}" table-layout="fixed">
        <xsl:call-template name="createDetailedFeesTable" />
        <xsl:call-template name="displayHeaderForDetailedFees">
          <xsl:with-param name="AmountCurrency" select="./feeAmount/currency" />
          <xsl:with-param name="CountervalueCurrency" select="./feeAccountingAmount/currency" />
        </xsl:call-template>
        <!-- ===== Scan fees ===== -->
        <xsl:for-each select="//invoiceFee">
          <xsl:variable name="currentInstrumentCurrency" select="concat(../../instrument, feeAmount/currency)" />
          <xsl:variable name="currentAmount" select="./feeAmount/amount" />
          <fo:table-body>
            <!-- EG 20160404 Migration vs2013 -->
            <fo:table-row font-size="{$varDetFeesFontSize}">
              <!-- ===== Trade identifier ===== -->
              <!-- EG 20160404 Migration vs2013 -->
              <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="right">
                <fo:block>
                  <xsl:value-of select="./../../identifier" />
                </fo:block>
              </fo:table-cell>
              <!-- ===== Trade date ===== -->
              <!-- EG 20160404 Migration vs2013 -->
              <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="center">
                <fo:block>
                  <xsl:call-template name="format-shortdate2">
                    <xsl:with-param name="xsd-date-time" select="./../../tradeDate" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <!-- ===== Expiry date (out date in XML file) ===== -->
              <!-- EG 20160404 Migration vs2013 -->
              <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="center">
                <fo:block>
                  <xsl:call-template name="format-shortdate2">
                    <xsl:with-param name="xsd-date-time" select="./../../outDate" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <!-- ===== Value date ===== -->
              <!-- EG 20160404 Migration vs2013 -->
              <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="center">
                <fo:block>
                  <xsl:call-template name="format-shortdate2">
                    <xsl:with-param name="xsd-date-time" select="./../../inDate" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <!-- ===== Trade currency ===== -->
              <!-- EG 20160404 Migration vs2013 -->
              <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="center">
                <fo:block>
                  <xsl:value-of select="../../currency[@currencyScheme='http://www.fpml.org/ext/iso4217-2001-08-15']" />
                </fo:block>
              </fo:table-cell>
              <!-- ===== Rate ===== -->
              <!-- EG 20160404 Migration vs2013 -->
              <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="center">
                <xsl:call-template name="format-fixed-rate2">
                  <xsl:with-param name="fixed-rate" select="normalize-space(../../price)" />
                </xsl:call-template>
              </fo:table-cell>
              <!-- ===== Buyer seller ===== -->
              <!-- EG 20160404 Migration vs2013 -->
              <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="right">
                <xsl:variable name="varside">
                  <xsl:value-of select="normalize-space(../../side)" />
                </xsl:variable>
                <fo:block>
                  <xsl:if test="$varside='Buyer'">B</xsl:if>
                  <xsl:if test="$varside='Seller'">S</xsl:if>
                </fo:block>
              </fo:table-cell>
              <!-- ===== Trader ===== -->
              <!-- EG 20160404 Migration vs2013 -->
              <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="center">
                <fo:block>
                  <xsl:variable name="varTraderName">
                    <xsl:value-of select="../../trader[@traderScheme='http://www.euro-finance-systems.fr/otcml/actorIdentifier']/@name" />
                  </xsl:variable>
                  <xsl:choose>
                    <xsl:when test="string-length( normalize-space( $varTraderName )) &gt; 0">
                      <xsl:value-of select="$varTraderName" />
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="../../trader[@traderScheme='http://www.euro-finance-systems.fr/otcml/actorIdentifier']" />
                    </xsl:otherwise>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
              <!-- ===== Counterparty ===== -->
              <!-- EG 20160404 Migration vs2013 -->
              <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="center">
                <fo:block>
                  <xsl:value-of select="../../counterpartyPartyReference/@href" />
                </fo:block>
              </fo:table-cell>
              <!-- ===== Duration ===== -->
              <!-- EG 20160404 Migration vs2013 -->
              <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="center">
                <fo:block>
                  <xsl:choose>
                    <xsl:when test="string-length( normalize-space( ./feeSchedule/duration )) &gt; 0">
                      <xsl:value-of select="./feeSchedule/duration" />
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="./../../periodNumberOfDays" />
                    </xsl:otherwise>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
              <!-- ===== ISIN ===== -->
              <!-- EG 20160404 Migration vs2013 -->
              <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="center">
                <fo:block>
                  <xsl:variable name="varISIN">
                    <xsl:call-template name="GetIsinCode">
                      <xsl:with-param name="pISINCode" select="../../asset/instrumentId[@instrumentIdScheme='http://www.euro-finance-systems.fr/spheres-enum/FIX/SecurityIDSource?V=4&amp;EV=ISIN']" />
                    </xsl:call-template>
                  </xsl:variable>
                  <xsl:variable name="varDescription" select="../../asset/description" />
                  <xsl:choose>
                    <xsl:when test="string-length( normalize-space( $varISIN )) &gt; 0">
                      <xsl:value-of select="$varISIN" />
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="$varDescription" />
                    </xsl:otherwise>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
              <!-- ===== Fee schedule ===== -->
              <!-- EG 20160404 Migration vs2013 -->
              <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="right">
                <fo:block>
                  <xsl:value-of select="./feeSchedule/identifier" />
                </fo:block>
              </fo:table-cell>
              <!-- ===== Fee basis ===== -->
              <!-- EG 20160404 Migration vs2013 -->
              <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="right">
                <fo:block>
                  <xsl:if test="string-length( normalize-space( ./feeSchedule/assessmentBasisValue1 )) &gt; 0">
                    <xsl:call-template name="format-money2">
                      <xsl:with-param name="amount" select="concat(./feeSchedule/assessmentBasisValue1,'')" />
                    </xsl:call-template>
                  </xsl:if>
                </fo:block>
              </fo:table-cell>
              <!-- ===== Nominal ===== -->
              <!-- EG 20160404 Migration vs2013 -->
              <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="right">
                <fo:block>
                  <xsl:variable name="varNominal">
                    <xsl:choose>
                      <xsl:when test="../../instrument = 'REPO'">
                        <xsl:value-of select="../../asset/notionalAmount/amount" />
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="../../notionalAmount/amount" />
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:variable>
                  <xsl:call-template name="format-money2">
                    <xsl:with-param name="amount" select="concat($varNominal,'')" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <!-- ===== Fee amount ===== -->
              <!-- EG 20160404 Migration vs2013 -->
              <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="right">
                <fo:block>
                  <xsl:if test="$varIsInvoice = 'true'">
                    <xsl:call-template name="format-money2">
                      <xsl:with-param name="amount" select="./feeInitialAmount/amount" />
                    </xsl:call-template>
                  </xsl:if>
                  <xsl:if test="$varIsAmendedInvoice = 'true'">
                    <xsl:call-template name="format-money2">
                      <xsl:with-param name="amount" select="./feeAmount/amount" />
                    </xsl:call-template>
                  </xsl:if>
                </fo:block>
              </fo:table-cell>
              <!-- ===== Countervalue amount ===== -->
              <!-- EG 20160404 Migration vs2013 -->
              <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="right">
                <fo:block>
                  <xsl:if test="$varIsInvoice = 'true'">
                    <xsl:call-template name="format-money2">
                      <xsl:with-param name="amount" select="./feeInitialAccountingAmount/amount" />
                    </xsl:call-template>
                  </xsl:if>
                  <xsl:if test="$varIsAmendedInvoice = 'true'">
                    <xsl:call-template name="format-money2">
                      <xsl:with-param name="amount" select="./feeAccountingAmount/amount" />
                    </xsl:call-template>
                  </xsl:if>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
          </fo:table-body>
          <!--</xsl:if>-->
        </xsl:for-each>
        <fo:table-body>
          <!-- ===== Subtotal for every instrument and fee currency ===== -->
          <fo:table-row font-size="7pt" text-align="right">
            <xsl:call-template name="createEmptyCellsForDetailedFees">
              <xsl:with-param name="pnNumberOfCells" select="13" />
            </xsl:call-template>
            <!-- ===== Subtotal ===== -->
            <!-- EG 20160404 Migration vs2013 -->
            <fo:table-cell font-weight="bold" padding="{$varDetailedFeesCellPadding}">
              <fo:block>
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'INV-SubTotal'" />
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <!-- ===== Fee amount total ===== -->
            <!-- EG 20160404 Migration vs2013 -->
            <fo:table-cell font-weight="bold" padding="{$varDetailedFeesCellPadding}">
              <fo:block>
                <xsl:if test="$varIsInvoice = 'true'">
                  <xsl:call-template name="format-money2">
                    <xsl:with-param name="amount" select="sum($lstFeeParam[feeInitialAmount/currency=$lngCurrency]/feeInitialAmount/amount)" />
                  </xsl:call-template>
                </xsl:if>
                <xsl:if test="$varIsAmendedInvoice = 'true'">
                  <xsl:call-template name="format-money2">
                    <xsl:with-param name="amount" select="sum($lstFeeParam[feeAmount/currency=$lngCurrency]/feeAmount/amount)" />
                  </xsl:call-template>
                </xsl:if>
              </fo:block>
            </fo:table-cell>
            <!-- ===== fee amount countervalue total===== -->
            <!-- EG 20160404 Migration vs2013 -->
            <fo:table-cell font-weight="bold" padding="{$varDetailedFeesCellPadding}">
              <fo:block>
                <xsl:if test="$varIsInvoice = 'true'">
                  <xsl:call-template name="format-money2">
                    <xsl:with-param name="amount" select="sum($lstFeeParam[feeInitialAccountingAmount/currency=$lngCurrency]/feeInitialAccountingAmount/amount)" />
                  </xsl:call-template>
                </xsl:if>
                <xsl:if test="$varIsAmendedInvoice = 'true'">
                  <xsl:call-template name="format-money2">
                    <xsl:with-param name="amount" select="sum($lstFeeParam[feeAmount/currency=$lngCurrency]/feeAccountingAmount/amount)" />
                  </xsl:call-template>
                </xsl:if>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
          <!-- Empty row after subtotal rows -->
          <!-- EG 20160404 Migration vs2013 -->
          <fo:table-row height="5mm">
            <xsl:call-template name="createEmptyCellsForDetailedFees">
              <xsl:with-param name="pnNumberOfCells" select="1" />
            </xsl:call-template>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </xsl:if>
  </xsl:template>
  <!-- Define keys used to get the data of the sorting type inside the Trade Sorting element -->
  <xsl:key name="sortIdKey" match="//invoiceTradeSorting/keys/key" use="@id" />
  <!--==========================================================================================-->
  <!-- TEMPLATE : displayGroupValue                                                             -->
  <!--            helper for formatting the Trade Sorting Key on the detailed Fees Header       -->
  <!--==========================================================================================-->
  <xsl:template name="displayGroupValue">
    <xsl:param name="pLstNameTrade" />
    <xsl:param name="pLstNameSort" />
    <fo:block>
      <fo:block>
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="concat($pLstNameSort,'')" />
        </xsl:call-template>
      </fo:block>
      <xsl:value-of select="$varEspace" />
      <xsl:choose>
        <xsl:when test="$pLstNameSort = 'CLIENT_TRADER'">
          <xsl:value-of select="$pLstNameTrade/trader/@name" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="." />
        </xsl:otherwise>
      </xsl:choose>
    </fo:block>
  </xsl:template>
  <!--==========================================================================================-->
  <!-- TEMPLATE : displayHeaderForDetailedFees                                                  -->
  <!--            create trade fee Header for Invoice and Amended Invoice                       -->
  <!--==========================================================================================-->
  <xsl:template name="displayHeaderForDetailedFees">
    <xsl:param name="AmountCurrency" />
    <xsl:param name="CountervalueCurrency" />
    <xsl:param name="plstInvoiceTradeReferenceXHeader" />
    <!-- Table header for detailed fees -->
    <fo:table-header>
      <!-- Empty row not diplayed. It is necessary, otherwise the border of following row is not right displayed -->
      <!-- EG 20160404 Migration vs2013 -->
      <fo:table-row height="1mm">
        <fo:table-cell>
          <fo:block/>
        </fo:table-cell>
      </fo:table-row>
      <!-- First cell of the first row of the header: instrument's name -->
      <!-- EG 20160404 Migration vs2013 -->
      <fo:table-row height="{$varDetFeesTableHeaderInstrumentHeigth}" font-size="{$varDetFeesTableHeaderInstrumentFontSize}" font-weight="bold">
        <xsl:if test="$varUseTradeSorting = false()">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:table-cell border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderInstrumentTextAlign}" padding="{$varDetFeesTableHeaderInstrumentPadding}" background-color="{$varDetFeesTableHeaderBackgroundColor}">
            <fo:block>
              <fo:block>
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'INV-Instrument'" />
                </xsl:call-template>
              </fo:block>
              <xsl:value-of select="$varEspace" />
              <xsl:value-of select="../../instrument[@productTypeScheme='http://www.euro-finance-systems.fr/otcml/producttype']" />
            </fo:block>
          </fo:table-cell>
        </xsl:if>
        <xsl:variable name="lstNameTrade" select="key($tradeIdKeyName, $plstInvoiceTradeReferenceXHeader[1]/@href)" />
        <xsl:if test="$varUseTradeSorting = true()">
          <xsl:variable name="varLstKeys" select="./keyValue" />
          <xsl:for-each select="$varLstKeys">
            <xsl:variable name="lstNameSort" select="key('sortIdKey', ./@href)" />
            <xsl:variable name="varColSpanWidth" select="4" />
            <xsl:choose>
              <!-- EG 20160404 Migration vs2013 -->
              <xsl:when test="($lstNameSort = 'CLIENT_TRADER') or ($lstNameSort = 'UNDERLYER_INSTRUMENT') or ($lstNameSort = 'TRADE_INSTRUMENT')">
                <fo:table-cell number-columns-spanned="4" border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderInstrumentTextAlign}" padding="{$varDetFeesTableHeaderInstrumentPadding}" background-color="{$varDetFeesTableHeaderBackgroundColor}">
                  <xsl:call-template name="displayGroupValue">
                    <xsl:with-param name="pLstNameTrade" select="$lstNameTrade" />
                    <xsl:with-param name="pLstNameSort" select="$lstNameSort" />
                  </xsl:call-template>
                </fo:table-cell>
              </xsl:when>
              <!-- EG 20160404 Migration vs2013 -->
              <xsl:otherwise>
                <fo:table-cell number-columns-spanned="2" border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderInstrumentTextAlign}" padding="{$varDetFeesTableHeaderInstrumentPadding}" background-color="{$varDetFeesTableHeaderBackgroundColor}">
                  <xsl:call-template name="displayGroupValue">
                    <xsl:with-param name="pLstNameTrade" select="$lstNameTrade" />
                    <xsl:with-param name="pLstNameSort" select="$lstNameSort" />
                  </xsl:call-template>
                </fo:table-cell>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:for-each>
        </xsl:if>
      </fo:table-row>
      <!--2th to n rows of the header: the name of the fields-->
      <!-- EG 20160404 Migration vs2013 -->
      <fo:table-row background-color="{$varDetFeesTableHeaderBackgroundColor}" font-size="{$varDetFeesTableHeaderTextFontSize}" font-weight="bold">
        <fo:table-cell border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'INV-Trade'" />
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'INV-TradeDate'" />
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'INV-ExpiryDate'" />
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'INV-ValueDate'" />
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'INV-TradeCurrency'" />
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'INV-Rate'" />
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'INV-BuyerSeller'" />
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'INV-Trader'" />
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'INV-Counterparty'" />
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'INV-Duration'" />
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'INV-IsinCode'" />
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'INV-FeeSchedule'" />
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'INV-FeeBasis'" />
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'INV-Nominal'" />
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'INV-Amount'" />
            </xsl:call-template>
          </fo:block>
          <fo:block>
            <xsl:value-of select="$AmountCurrency" />
          </fo:block>
        </fo:table-cell>
        <fo:table-cell border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'INV-Countervalue'" />
            </xsl:call-template>
          </fo:block>
          <fo:block>
            <xsl:value-of select="$CountervalueCurrency" />
          </fo:block>
        </fo:table-cell>
      </fo:table-row>
    </fo:table-header>
  </xsl:template>
  <!--==========================================================================================-->
  <!-- TEMPLATE : createCreditNoteDetailedFeesTable                                             -->
  <!--            create trade fee columns for Credit note and Additional Invoice               -->
  <!--==========================================================================================-->
  <xsl:template name="createCreditNoteDetailedFeesTable">
    <fo:table-column column-width="proportional-column-width(1)" column-number="01" />
    <!-- Trade identifier -->
    <fo:table-column column-width="{$varDetFeesTableCol1Width}mm" column-number="02" />
    <!-- Trade date -->
    <fo:table-column column-width="{$varDetFeesTableCol2Width}mm" column-number="03" />
    <!-- Original Amount (to replace with base) -->
    <fo:table-column column-width="{$varDetFeesTableCol14Width}mm" column-number="04" />
    <!-- Original Accounting Amount (to replace with base) -->
    <fo:table-column column-width="{$varDetFeesTableCol14Width}mm" column-number="05" />
    <!-- Actual or Amended Amount -->
    <fo:table-column column-width="{$varDetFeesTableCol14Width}mm" column-number="06" />
    <!-- Actual or Amended Accounting Amount -->
    <fo:table-column column-width="{$varDetFeesTableCol14Width}mm" column-number="07" />
    <!-- Delta Amount -->
    <fo:table-column column-width="{$varDetFeesTableCol14Width}mm" column-number="08" />
    <!-- Delta Accounting Amount -->
    <fo:table-column column-width="{$varDetFeesTableCol14Width}mm" column-number="09" />

    <fo:table-column column-width="proportional-column-width(1)" column-number="10" />
  </xsl:template>
  <!--==========================================================================================-->
  <!-- TEMPLATE : displayHeaderForCreditNoteDetailedFees                                        -->
  <!--            create trade fee Header for Credit Note and Additional Invoice                -->
  <!--==========================================================================================-->
  <xsl:template name="displayHeaderForCreditNoteDetailedFees">
    <xsl:param name="AmountCurrency" />
    <xsl:param name="AccountingAmountCurrency" />
    <!-- Table header for detailed fees -->
    <fo:table-header>
      <!-- Empty row not diplayed. It is necessary, otherwise the border of following row is not right displayed -->
      <!-- EG 20160404 Migration vs2013 -->
      <fo:table-row height="1mm">
        <fo:table-cell>
          <fo:block />
        </fo:table-cell>
      </fo:table-row>
      <!-- First cell of the first row of the header: instrument's name -->
      <!-- EG 20160404 Migration vs2013 -->
      <fo:table-row height="{$varDetFeesTableHeaderInstrumentHeigth}" font-size="{$varDetFeesTableHeaderInstrumentFontSize}" font-weight="bold">
        <!-- EG 20160404 Migration vs2013 -->
        <fo:table-cell column-number="2" number-columns-spanned="8" xborder="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderInstrumentTextAlign}" padding="{$varDetFeesTableHeaderInstrumentPadding}" xbackground-color="{$varDetFeesTableHeaderBackgroundColor}">
          <fo:block>
            <fo:block>
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'INV-Instrument'" />
              </xsl:call-template>
            </fo:block>
            <xsl:value-of select="$varEspace" />
            <xsl:value-of select="../../instrument[@productTypeScheme='http://www.euro-finance-systems.fr/otcml/producttype']" />
          </fo:block>
        </fo:table-cell>
      </fo:table-row>
      <!--2th to n rows of the header: the name of the fields-->
      <!-- EG 20160404 Migration vs2013 -->
      <fo:table-row font-size="{$varDetFeesTableHeaderTextFontSize}" font-weight="bold">
        <fo:table-cell background-color="{$varDetFeesTableHeaderBackgroundColor}" column-number="2" number-columns-spanned="2" border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'INV-Characteristics'" />
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell background-color="{$varDetFeesTableHeaderBackgroundColor}" column-number="4" number-columns-spanned="2" border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'INV-PreviousAmount'" />
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell background-color="{$varDetFeesTableHeaderBackgroundColor}" column-number="6" number-columns-spanned="2" border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'INV-AmendedAmount'" />
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell background-color="{$varDetFeesTableHeaderBackgroundColor}" column-number="8" number-columns-spanned="2" border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'INV-Delta'" />
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
      </fo:table-row>
      <!-- EG 20160404 Migration vs2013 -->
      <fo:table-row font-size="{$varDetFeesTableHeaderTextFontSize}" font-weight="bold">
        <fo:table-cell background-color="{$varDetFeesTableHeaderBackgroundColor}" column-number="2" border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'INV-Trade'" />
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell background-color="{$varDetFeesTableHeaderBackgroundColor}" column-number="3" border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'INV-TradeDate'" />
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell background-color="{$varDetFeesTableHeaderBackgroundColor}" column-number="4"  border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:value-of select="$AmountCurrency" />
          </fo:block>
        </fo:table-cell>
        <fo:table-cell background-color="{$varDetFeesTableHeaderBackgroundColor}" column-number="5" border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:value-of select="$AccountingAmountCurrency" />
          </fo:block>
        </fo:table-cell>
        <fo:table-cell background-color="{$varDetFeesTableHeaderBackgroundColor}" column-number="6" border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:value-of select="$AmountCurrency" />
          </fo:block>
        </fo:table-cell>
        <fo:table-cell background-color="{$varDetFeesTableHeaderBackgroundColor}" column-number="7" border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:value-of select="$AccountingAmountCurrency" />
          </fo:block>
        </fo:table-cell>
        <fo:table-cell background-color="{$varDetFeesTableHeaderBackgroundColor}" column-number="8" border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:value-of select="$AmountCurrency" />
          </fo:block>
        </fo:table-cell>
        <fo:table-cell background-color="{$varDetFeesTableHeaderBackgroundColor}" column-number="9" border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:value-of select="$AccountingAmountCurrency" />
          </fo:block>
        </fo:table-cell>
      </fo:table-row>
    </fo:table-header>
  </xsl:template>
  <!--==========================================================================================-->
  <!-- TEMPLATE : createEmptyCellsForDetailedFees                                               -->
  <!--            for creating an empty row for detailed fees table                             -->
  <!--==========================================================================================-->
  <xsl:template name="createEmptyCellsForDetailedFees">
    <xsl:param name="pnNumberOfCells" />
    <xsl:if test="number($pnNumberOfCells) &gt;= 1">
      <fo:table-cell>
        <fo:block />
      </fo:table-cell>
      <xsl:call-template name="createEmptyCellsForDetailedFees">
        <xsl:with-param name="pnNumberOfCells" select="$pnNumberOfCells - 1" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
  <!-- ====================================================================== -->
  <!--                       SECTION detailed fees                            -->
  <!-- ====================================================================== -->
  <xsl:key name="invoiceTradeIdKey" match="//invoice/invoiceDetails/invoiceTrade" use="@id" />
  <xsl:key name="creditNoteTradeIdKey" match="//creditNote/invoiceDetails/invoiceTrade" use="@id" />
  <xsl:key name="additionalInvoiceTradeidKey" match="//additionalInvoice/invoiceDetails/invoiceTrade" use="@id" />
  <!--==========================================================================================-->
  <!-- TEMPLATE : displayDetailedFees                                                           -->
  <!--            called in detailed fees pages                                                 -->
  <!--==========================================================================================-->
  <xsl:template name="displayDetailedFees">
    <xsl:param name="pCreditNoteVisualization" />
    <xsl:param name="pEnableCreditNoteVisualization" />
    <!-- EG 20160404 Migration vs2013 -->
    <fo:block margin-top="0mm">
      <!-- Fees visualization without the trade sorting strategy -->
      <xsl:if test="$pCreditNoteVisualization = 'true' or $varUseTradeSorting = false()">
        <!-- For each instrument (invoiceTrade/instrument) of trades having fees (invoiceFee) -->
        <xsl:for-each select="//invoiceFee[generate-id(.) = generate-id(key('keyByInstrument', ../../instrument)[1])]">
          <xsl:sort select="../../instrument" />
          <xsl:variable name="instrument">
            <xsl:value-of select="../../instrument" />
          </xsl:variable>
          <!-- Select all the fees belonging to the instrument -->
          <xsl:variable name="lstFee" select="//invoiceFee[../../instrument=$instrument]" />
          <!-- Show details for fees belonging to the instrument -->
          <xsl:call-template name="displayFeesSummaryWithoutTradeSorting">
            <xsl:with-param name="lstFeeParam" select="$lstFee" />
            <xsl:with-param name="pCreditNoteVisualization" select="$pCreditNoteVisualization" />
          </xsl:call-template>
        </xsl:for-each>
      </xsl:if>
      <!-- Fees visualization using the invoice trade sorting strategy -->
      <xsl:if test="$pCreditNoteVisualization = 'false' and $varUseTradeSorting = true()">

        <xsl:for-each select="//invoiceTradeSorting/groups/group">
          <xsl:variable name="varLstInvoiceTradeReference" select="./invoiceTradeReference" />

          <!-- BEGIN DEBUG BLOCK, please dont delete-->
          <!--<fo:table width="{$varDetFeesTableTotalWidth}" table-layout="fixed">
						<fo:table-column column-width="300mm"  column-number="01"></fo:table-column>
						<fo:table-header>

							<fo:table-row height="1mm" >
								<fo:table-cell>
									<fo:block>
									</fo:block>
								</fo:table-cell>
							</fo:table-row>
						</fo:table-header>
						<fo:table-body>
							<fo:table-row font-size="{$varDetFeesFontSize}">

								<fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="center">

									<xsl:for-each select="$varLstInvoiceTradeReference">

										<xsl:variable name="lstNameTrade" select="key($tradeIdKeyName, ./@href)" />

										<fo:block>
											<xsl:value-of select="$lstNameTrade/identifier"/>
										</fo:block>
									</xsl:for-each>
								</fo:table-cell>
							</fo:table-row>
						</fo:table-body>
					</fo:table>-->
          <!-- END DEBUG BLOCK, please dont delete-->

          <xsl:choose>
            <xsl:when test="$IsDerivative = 'true'">
              <!--<xsl:call-template name="DisplayDetailedTradesForGroupFO">
                <xsl:with-param name="pGroup">
                  <xsl:copy-of select="."/>
                </xsl:with-param>
              </xsl:call-template>-->
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="displayFeesSummaryUsingTradeSorting">
                <xsl:with-param name="plstInvoiceTradeReference" select="$varLstInvoiceTradeReference" />
                <xsl:with-param name="pCreditNoteVisualization" select="$pCreditNoteVisualization" />
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>

        </xsl:for-each>

        <xsl:if test="($varIsAmendedInvoice = 'false') or ($pEnableCreditNoteVisualization = 'false')">
          <xsl:call-template name="DisplayTotalValues">
            <xsl:with-param name="pCreditNoteVisualization" select="$pEnableCreditNoteVisualization" />
          </xsl:call-template>
        </xsl:if>

      </xsl:if>
    </fo:block>
  </xsl:template>
  <!-- ====================================================================== -->
  <!--                       END SECTION detailed fees                        -->
  <!-- ====================================================================== -->
  <!--==========================================================================================-->
  <!-- TEMPLATE : createTotalValuesTable                                                        -->
  <!--            to show the total values of the invoice at the end of the detailed fees table -->
  <!--==========================================================================================-->
  <xsl:template name="createTotalValuesTable">
    <!-- Total label -->
    <fo:table-column column-width="{normalize-space($varDetFeesTableCol1Width + $varDetFeesTableCol2Width + $varDetFeesTableCol3Width + $varDetFeesTableCol4Width + $varDetFeesTableCol5Width + $varDetFeesTableCol6Width + $varDetFeesTableCol7Width + $varDetFeesTableCol8Width + $varDetFeesTableCol9Width + $varDetFeesTableCol10Width + $varDetFeesTableCol11Width + $varDetFeesTableCol12Width + $varDetFeesTableCol13Width + $varDetFeesTableCol14Width)}mm" column-number="01" />
    <!-- Totla amount -->
    <fo:table-column column-width="{$varDetFeesTableCol15Width}mm" column-number="02" />
    <!-- Total countervalue amount -->
    <fo:table-column column-width="{$varDetFeesTableCol16Width}mm" column-number="03" />
  </xsl:template>
  <!--==========================================================================================-->
  <!-- TEMPLATE : displayHeaderForTotalValues                                                   -->
  <!--            to show the total values of the invoice at the end of the detailed fees table -->
  <!--==========================================================================================-->
  <xsl:template name="displayHeaderForTotalValues">
    <xsl:param name="pAmountCurrency" />
    <xsl:param name="pAccountingAmountCurrency" />
    <!-- Table header for detailed fees -->
    <fo:table-header>
      <!-- Empty row not diplayed. It is necessary, otherwise the border of following row is not right displayed -->
      <!-- EG 20160404 Migration vs2013 -->
      <fo:table-row height="1mm">
        <fo:table-cell>
          <fo:block />
        </fo:table-cell>
      </fo:table-row>
      <!-- First cell of the first row of the header: instrument's name -->
      <!-- EG 20160404 Migration vs2013 -->
      <fo:table-row background-color="{$varDetFeesTableHeaderBackgroundColor}" font-size="{$varDetFeesTableHeaderTextFontSize}" font-weight="bold">
        <!-- EG 20160404 Migration vs2013 -->
        <fo:table-cell border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderInstrumentTextAlign}" padding="{$varDetFeesTableHeaderInstrumentPadding}" background-color="{$varDetFeesTableHeaderBackgroundColor}">
          <fo:block>
            <fo:block>
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'INV-Total'" />
              </xsl:call-template>
            </fo:block>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'INV-Amount'" />
            </xsl:call-template>
          </fo:block>
          <fo:block>
            <xsl:value-of select="$pAmountCurrency" />
          </fo:block>
        </fo:table-cell>
        <fo:table-cell border="{$varDetFeesTableHeaderCellBorder}" text-align="{$varDetFeesTableHeaderTextAlign}">
          <!-- EG 20160404 Migration vs2013 -->
          <fo:block padding-top="{$varDetFeesTableHeaderTextPaddingTop}">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'INV-Countervalue'" />
            </xsl:call-template>
          </fo:block>
          <fo:block>
            <xsl:value-of select="$pAccountingAmountCurrency" />
          </fo:block>
        </fo:table-cell>
      </fo:table-row>
    </fo:table-header>
  </xsl:template>
  <!--==========================================================================================-->
  <!-- TEMPLATE : DisplayTotalValues                                                            -->
  <!--            to show the total values of the invoice at the end of the detailed fees table -->
  <!--==========================================================================================-->
  <xsl:template name="DisplayTotalValues">
    <xsl:param name="pCreditNoteVisualization" />

    <xsl:variable name="varTempNetTurnOverAmount">
      <!-- case invoice/credit note -->
      <xsl:if test="($varIsInvoice = 'true') or ($pCreditNoteVisualization = 'true')">
        <xsl:value-of select="normalize-space($varNetTurnOverAmount)" />
      </xsl:if>
      <!-- case amended invoice -->
      <xsl:if test="($varIsAmendedInvoice = 'true') and ($pCreditNoteVisualization = 'false')">
        <xsl:value-of select="normalize-space($varTheoricNetTurnOverAmount)" />
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="varTempNetTurnOverAccountingAmount">
      <xsl:if test="($varIsInvoice = 'true') or ($pCreditNoteVisualization = 'true')">
        <xsl:value-of select="normalize-space($varNetTurnOverAccountingAmount)" />
      </xsl:if>
      <xsl:if test="($varIsAmendedInvoice = 'true') and ($pCreditNoteVisualization = 'false')">
        <xsl:value-of select="normalize-space($varTheoricNetTurnOverAccountingAmount)" />
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="varTempTaxAmount">
      <!-- case invoice/credit note -->
      <xsl:if test="($varIsInvoice = 'true') or ($pCreditNoteVisualization = 'true')">
        <xsl:value-of select="normalize-space($varTaxAmount)" />
      </xsl:if>
      <!-- case amended invoice -->
      <xsl:if test="($varIsAmendedInvoice = 'true') and ($pCreditNoteVisualization = 'false')">
        <xsl:value-of select="normalize-space($varTheoricTaxAmount)" />
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="varTempTaxAccountingAmount">
      <xsl:if test="($varIsInvoice = 'true') or ($pCreditNoteVisualization = 'true')">
        <xsl:value-of select="normalize-space($varTaxAccountingAmount)" />
      </xsl:if>
      <xsl:if test="($varIsAmendedInvoice = 'true') and ($pCreditNoteVisualization = 'false')">
        <xsl:value-of select="normalize-space($varTheoricTaxAccountingAmount)" />
      </xsl:if>
    </xsl:variable>

    <xsl:if test="($varTempNetTurnOverAmount != 0.0 or $varTempNetTurnOverAccountingAmount != 0.0 or $varShowZeroValue)">
      <xsl:choose>
        <xsl:when test="$IsDerivative = 'true'">

          <!--<xsl:call-template name="DisplayTotalGen_FO">
            <xsl:with-param name="pTaxAmount" select="$varTaxAmount" />
            <xsl:with-param name="pTempTaxAmount" select="$varTempTaxAmount" />
            <xsl:with-param name="pTempTaxAccountingAmount" select="$varTempTaxAccountingAmount" />
            <xsl:with-param name="pTempNetTurnOverAmount" select="$varTempNetTurnOverAmount" />
            <xsl:with-param name="pTempNetTurnOverAccountingAmount" select="$varTempNetTurnOverAccountingAmount" />
            <xsl:with-param name="pAmountCurrency" select="$varNetTurnOverCurrency" />
            <xsl:with-param name="pAccountingAmountCurrency" select="$varNetTurnOverAccountingCurrency" />
          </xsl:call-template>-->


        </xsl:when>
        <xsl:otherwise>
          <!-- EG 20160404 Migration vs2013 -->
          <fo:table width="{$varDetFeesTableTotalWidth}" table-layout="fixed">
            <xsl:call-template name="createTotalValuesTable" />
            <xsl:call-template name="displayHeaderForTotalValues">
              <xsl:with-param name="pAmountCurrency" select="normalize-space($varNetTurnOverIssueCurrency)" />
              <xsl:with-param name="pAccountingAmountCurrency" select="normalize-space($varNetTurnOverAccountingCurrency)" />
            </xsl:call-template>
            <fo:table-body>
              <xsl:if test="$varTaxAmount&gt;0">
                <!-- EG 20160404 Migration vs2013 -->
                <fo:table-row font-size="{$varDetFeesFontSize}" text-align="right">
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-cell font-weight="bold" padding="{$varDetailedFeesCellPadding}">
                    <fo:block>
                      <xsl:call-template name="getSpheresTranslation">
                        <xsl:with-param name="pResourceName" select="'INV-TaxAmount'" />
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <!-- ===== Tax amount ===== -->
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="right">
                    <fo:block>
                      <xsl:call-template name="format-money2">
                        <xsl:with-param name="amount" select="$varTempTaxAmount" />
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <!-- ===== Countervalue amount ===== -->
                  <!-- EG 20160404 Migration vs2013 -->
                  <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="right">
                    <fo:block>
                      <xsl:call-template name="format-money2">
                        <xsl:with-param name="amount" select="$varTempTaxAccountingAmount" />
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:if>
              <!-- EG 20160404 Migration vs2013 -->
              <fo:table-row font-size="{$varDetFeesFontSize}" text-align="right">
                <!-- EG 20160404 Migration vs2013 -->
                <fo:table-cell font-weight="bold" padding="{$varDetailedFeesCellPadding}">
                  <fo:block>
                    <xsl:call-template name="getSpheresTranslation">
                      <xsl:with-param name="pResourceName" select="'INV-NetTurnOverAmount'" />
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <!-- ===== Net amount ===== -->
                <!-- EG 20160404 Migration vs2013 -->
                <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="right">
                  <fo:block>
                    <xsl:call-template name="format-money2">
                      <xsl:with-param name="amount" select="$varTempNetTurnOverAmount" />
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <!-- ===== Countervalue amount ===== -->
                <!-- EG 20160404 Migration vs2013 -->
                <fo:table-cell padding="{$varDetailedFeesCellPadding}" border="{$varDetFeesTableBorder}" text-align="right">
                  <fo:block>
                    <xsl:call-template name="format-money2">
                      <xsl:with-param name="amount" select="$varTempNetTurnOverAccountingAmount" />
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </fo:table-body>
          </fo:table>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>