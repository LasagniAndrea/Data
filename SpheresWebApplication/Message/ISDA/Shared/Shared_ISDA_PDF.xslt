<?xml version="1.0" encoding="UTF-8" ?>

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:dt="http://xsltsl.org/date-time"
				xmlns:fo="http://www.w3.org/1999/XSL/Format" version="1.0">
  <!--
	==============================================================================================================================
	Summary : Shared ISDA PDF
	==============================================================================================================================
  EG 20150519 : Complete Refactoring for ISDA Bond Option transaction confirmation
  =============================================================================================================================  
	Revision        : 8
	Date            : 12.05.2015
	Author          : Pony LA
	Spheres Version : 4.6.0
  Update          : Add the Region (General Terms,settlement Terms) of the Bond Option Statement 
                    
  =============================================================================================================================  
	Revision        : 7
	Date            : 20.03.2015
	Author          : Pony LA
	Spheres Version : 4.6.0
  Update          : Add the Region (General Terms,settlement Terms) of the Equity Statement 
                    
  =============================================================================================================================  
	Revision        : 6
	Date            : 08.12.2011
	Author          : Gianmario SERIO
	Spheres Version : 2.6.0
  Update          : Bug fixed into displayExtendFields template (see GS 20111208 comments)
                    ExtendDet displayname value was not displayed for the last specific culture value 
  =============================================================================================================================  
  Revision        : 5
	Date            : 29/11/2011
	Author          : Gianmario SERIO
	Spheres Version : 2.6
  Update          : Swap confirmation update: Handle Offset into fixed and floating streams [OIS swap] see GS 20111129 comments
  ==============================================================================================================================
  Revision: 4
	Date    : 14/02/2011
	Author  : Gianmario SERIO
	Version : 2.5
  Update  : 1) Handle stub: 
               the stubs are asynchronous when 
               - only one stream has a stub 
               - two or more streams have stubs where the dates are different
               - in this case we display the stub informations for each specific stream
               the stubs are synchronous when
               - two or more streams have stubs where the dates are equal 
             2) Rename isCalculationPeriodDatesDifferent variable (changed in isTermDifferent)
             3) Improved code to handle zero coupon swap (see GS 20110214 comments)
             4) Improved code to handle basis swap with EONIA and EURIBOR floating rate (see GS 20110214 comments) 
  =================================================================================================================
  Revision    : 3	
	Version     : 2.3.0.9_3
	Date        : 09.10.2009
	Author      : Gianmario SERIO
	Updates     : Bug fix: It returns right stream position in events XML flux
				  Add create1Column template
				  Add Add displayNotepad and displaySplitNotepad templates: it displays carriage return in notepad text box
	==================================================================================================================
	Revision: 2
	Version : 2.3.0.3_3
	Date    : 31.08.2009
	Author  : Gianmario SERIO
	Update  : Add template create3Columns (draw annex columns) 
            Add template displayAnnex
	          - it displays scheduled nominal in annex 
            Update template displayAdditionalInformation
            - It show tradeExtend information (from extend and extendDet table)	
            Add Provisions region: it menages cancelation or early termination provisions
	          It contains:
            - Template displayOptionalEarlyTermination: it displays optional early termination information
            - [eg. option style, early termination date, buyer, seller, calculation agent]
            - displayCancelableProvision : it displays cancelable provision information
            - [eg. option style, early termination date, buyer, seller, calculation agent]
            - N.B. in ISDA confirmations the FPML 'Cancelable provision' corresponds to 'Early termination provisions with cashsettlement inapplicable'
            - Template displayMandatoryEarlyTermination : it displays mandatory early termination information 
            - Template displayRelevantUnderlyngDate: it displays Early Termination Date
            - Template displayProvisionProcedureForExercise: it displays provision procedure for exercise (not available for mandatory early termination)
            - For european exercise: Expiration Date, Earliest Exercise Time, Expiration Time and Partial Exercise
            - For american exercise: Commencement Date, Expiration Date, Earliest Exercise Time, Latest Exercise Time Expiration Time and MultipleExercise.   
            - For bermuda exercise: Bermuda Exercise Dates, Earliest Exercise Time, Latest Exercise Time, Expiration Time and MultipleExercise. 
            -  Template displayBermudaExerciseDates: it displays Bermuda Exercise Dates
            -  Template displayProvisionSettlementTerms: it displays Provision Settlement Terms (not availbale for cancelable provision)
            - it contains: CashSettlement, CashS ettlement Valuation Time, Cash Settlement Valuation Date, Valuation Business Days, Cash Settlement Payment Date, Cash Settlement Method, Cash Settlement Currency, Quotation Rate.   
            Update template displayAdditionalPayment: 
            - Rename [paymentType = 'UPFront']
            - in     [paymentType = 'Upfront']
            - PaymentType=Upfront is an OTCml default
            Update template displayFloatingRateCalculation
            - Add the section that shows Floating Rate For Initial or Final Calculation Period (stubs)
            - Add 'pPosition' parameter (it is define in handleIrdStreams template - it is used from varFloatingStreamNo and varFixedStreamNo variable)
            - Add parameter: pIsCalculationPeriodDatesDifferent
            Update template displayFixedRateSchedule
            - Add the section that shows the Fixed Rate For Initial or Final Calculation Period (stubs)
            - Add parameter: pIsCalculationPeriodDatesDifferent
            Add template displayFxAmericanOrEuropeanTriggers
            - This template manages one, two or more triggers
            Add template displayFxBarriers
            - This template manages one, two or more barriers
            Add template displayTriggerRate
            - It returns the appropriate barrier level (for one or more triggers) 
            Rename template displayFxDigitalTerms in displayFxDigitalOptionTerms
            Update template displayFxDigitalOptionsTerms
            - It displays triggers and barriers in a FxDigital			  
            Update template displayIrdProductTerms
            - Add new role for buyer an seller for CapFloor product:
            - The buyer of a CAP is the receiver of capFloorStream
            - The seller of a CAP is the payer of capFloorStream
            Update template displayFloatingRateCalculation  in Floating rate Payer section. 
            - Add new roles for Collar-Straddle-Strangle-Corridor instruments
            Update template displayRateSchedule
            - Add rate schedule for Corridor (Cap rate1 and Cap rate2)
	==========================================================================================================
	Revision: 1
	Version : 2.3.0.3_2
	Date    : 03.07.2009
	Author  : Domenico Rotta
	Description: (FRA) Add business days for payments into "displayFRATerms" template
	             (FxSwap) Update of template "displayExchangedCurrency": handle of FxSwap
	============================================================================================================
	Date    : 20.05.2009
	Author  : Gianmario SERIO
	Version : 2.3.0.3_1
	Description:
	==============================================================================================================
	-->
  <!-- 
	===============================================
	===============================================
	== All products: BEGIN REGION     =============
	===============================================
	===============================================
	-->
  <xsl:variable name="varLinefeed">&#x0d;</xsl:variable>
  <xsl:variable name="varSpaceCharacter">&#160;</xsl:variable>
  <!--
	===========================================================
	this parameter come from Spheres
	==========================================================
	<xsl:param name="pInstrument" select="//productType"/>
	-->
  <!-- ========================================= -->
  <!-- Variables for fonts                       -->
  <!-- ========================================= -->
  <xsl:variable name="varFontFamilyCourier">
    <xsl:value-of select="$gVarFontFamilyCourier"/>
  </xsl:variable>

  <!-- source logo in debug mode -->
  <xsl:variable name="varImgLogoDebug">
    <xsl:text>url(..\\HPC-Logo.jpg)</xsl:text>
  </xsl:variable>
  <!-- source logo in default mode: to be testing!!! -->
  <xsl:variable name="varImgLogo">
    <xsl:value-of select="concat('sql(select IDENTIFIER, LOLOGO from dbo.ACTOR where IDA=', //header/sendBy/routingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/Actorid'], ')')"/>
  </xsl:variable>
  <!-- ================================================================================================================ -->
  <!-- Begin region: template define vertical A4 page                                                                   -->
  <!-- ================================================================================================================ -->
  <xsl:template name="definePage">
    <fo:layout-master-set>
      <fo:simple-page-master master-name="A4" page-height="{$gVarA4VerticalPageHeightCM}cm" page-width="{$gVarA4VerticalPageWidthCM}cm" margin-left="{$varA4LeftMarginCM}cm" margin-right="{$varA4RightMarginCM}cm" margin-bottom="{$varA4BottomMarginCM}cm" margin-top="{$varA4TopMarginCM}cm">
        <!-- The top margin of the body is egual to header height -->
        <fo:region-body   region-name="A4-body"   background-color="{$gVarPageBodyColor}"   margin-left="0cm" margin-right="0cm" margin-bottom="{$varPageFooterHeightCM}cm" margin-top="{$varPageHeaderHeightCM}cm"></fo:region-body>
        <fo:region-before region-name="A4-header" background-color="{$gVarPageHeaderColor}" extent="{$varPageHeaderHeightCM}cm" precedence="true"></fo:region-before>
        <fo:region-after  region-name="A4-footer" background-color="{$gVarPageFooterColor}" extent="{$varPageFooterHeightCM}cm" precedence="true"></fo:region-after>
        <fo:region-start  region-name="A4-left"   background-color="{$gVarPageLeftColor}"   extent="0cm"></fo:region-start>
        <fo:region-end    region-name="A4-right"  background-color="{$gVarPageRightColor}"  extent="0cm"></fo:region-end>
      </fo:simple-page-master>
    </fo:layout-master-set>
  </xsl:template>
  <!-- ================================================================================================================ -->
  <!-- End region: template define vertical A4 page                                                                     -->
  <!-- ================================================================================================================ -->

  <!-- ================================================================================================================ -->
  <!-- Begin region: template display page header                                                                       -->
  <!-- ================================================================================================================ -->
  <xsl:template name="displayPageHeader">
    <fo:block border="{$gVarBorder}" margin-top="{$varLogoTopMarginCM}cm" margin-left="{$varLogoLeftMarginCM}cm" line-height="1" font-size="0pt">
      <fo:external-graphic src="{$varImgLogo}" text-align="left" display-align="before" height="{$varLogoHeightCM}cm"/>
    </fo:block>
  </xsl:template>
  <!-- ================================================================================================================ -->
  <!-- End region: template display page header                                                                         -->
  <!-- ================================================================================================================ -->

  <!-- ================================================================================================================ -->
  <!-- Begin region: template display page footer                                                                       -->
  <!-- ================================================================================================================ -->
  <xsl:template name="displayPageFooter">
    <xsl:variable name="varLineWidthCM">
      <xsl:value-of select="$gVarA4VerticalPageWidthCM - $varA4LeftMarginCM - $varA4RightMarginCM"/>
    </xsl:variable>

    <!-- Display a line -->
    <fo:block border="{$gVarBorder}"  text-align="right" border-bottom="0.8pt solid black" font-size="{$varPageFooterBankAddressFontSize}">
      <fo:page-number/>/<fo:page-number-citation ref-id="EndOfDoc"/>
    </fo:block>
    <!--
		<fo:block border="{$gVarBorder}">
			<fo:leader leader-pattern="rule" leader-length="{$varLineWidthCM}cm"/>
		</fo:block>
		-->
    <fo:block text-align="{$varPageFooterBankNameTextAlign}" font-size="{$varPageFooterBankNameFontSize}">
      <xsl:value-of select="$footer1" />
    </fo:block>
    <!-- Bank address 1 -->
    <fo:block text-align="{$varPageFooterBankAddressTextAlign}" font-size="{$varPageFooterBankAddressFontSize}">
      <xsl:value-of select="$footer2" />
    </fo:block>
    <!-- Bank address 2 -->
    <fo:block text-align="{$varPageFooterBankAddressTextAlign}" font-size="{$varPageFooterBankAddressFontSize}">
      <xsl:value-of select="$footer3" />
    </fo:block>
  </xsl:template>
  <!-- ================================================================================================================ -->
  <!-- End region: template display page footer                                                                         -->
  <!-- ================================================================================================================ -->

  <!-- ================================================================================================================ -->
  <!-- Begin region: template confirmation title                                                                        -->
  <!-- ================================================================================================================ -->
  <xsl:template name="displayConfirmationTitle">
    <fo:block text-align="{$varConfirmationTitleTextAlign}" margin-left="{$varConfirmationTitleLeftMarginCM}cm" margin-top="{$varConfirmationTitleTopMarginCM}cm" font-size="{$varConfirmationTitleFontSize}" color="{$varConfirmationTitleColor}" font-weight="{$varConfirmationFontWeight}">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'ConfirmationOf'"/>
      </xsl:call-template>
      <xsl:choose>
        <xsl:when test="//productType='EuropeanSwaption' or //productType='AmericanSwaption' or //productType='BermudaSwaption'">
          <xsl:text>Swaption</xsl:text>
        </xsl:when>
        <xsl:when test="//productType='debtSecurityOption' or //productType='debtSecurityAmericanOption' or //productType='debtSecurityBermudaOption' or //productType='debtSecurityEuropeanOption'">
          <xsl:text>Bond option</xsl:text>
        </xsl:when>
        <xsl:when test ="//dataDocument/trade/swap/node()">
          <xsl:text>Swap</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="//productType"/>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'Transaction'"/>
      </xsl:call-template>
      <xsl:call-template name="displayBlockBreaklines">
        <xsl:with-param name="min" select="1"/>
        <xsl:with-param name="max" select="5"/>
      </xsl:call-template>
    </fo:block>
  </xsl:template>
  <!-- ================================================================================================================ -->
  <!-- End region: template confirmation title                                                                          -->
  <!-- ================================================================================================================ -->

  <!-- ================================================================================================================ -->
  <!-- Begin region: template confirmation statement title                                                                        -->
  <!-- ================================================================================================================ -->
  <xsl:template name="displayConfirmationStatementTitle">
    <fo:block text-align="{$varConfirmationTitleTextAlign}" margin-left="{$varConfirmationTitleLeftMarginCM}cm" margin-top="{$varConfirmationTitleTopMarginCM}cm" font-size="{$varConfirmationTitleFontSize}" color="{$varConfirmationTitleColor}" font-weight="{$varConfirmationFontWeight}">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'ConfirmationOf'"/>
      </xsl:call-template>

      <xsl:value-of select="//events/Event/Event/DISPLAYNAME_INSTR"/>

      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'Transaction'"/>
      </xsl:call-template>

      <xsl:call-template name="displayBlockBreaklines">
        <xsl:with-param name="min" select="1"/>
        <xsl:with-param name="max" select="5"/>
      </xsl:call-template>
    </fo:block>
  </xsl:template>
  <!-- ================================================================================================================ -->
  <!-- End region: template confirmation statement title                                                                          -->
  <!-- ================================================================================================================ -->

  <!-- ================================================================================================================ -->
  <!-- Begin region: display Annex																					  -->
  <!-- ================================================================================================================ -->

  <xsl:template name="displayAnnex">
    <xsl:param name="pStream"/>
    <xsl:param name="pIsNotionalDifferent"/>

    <xsl:choose>
      <!-- when notional amount is the same for all streams (pIsNotionalDifferent = false) 
			     - it scans the first stream and it returns first stream amortizing -->
      <xsl:when test ="$pIsNotionalDifferent != 'true' ">

        <xsl:for-each select ="$pStream[1]">

          <xsl:variable name ="varScheduledNominalAnnexNumber">
            <xsl:call-template name ="getScheduledNominalAnnexNumber">
              <xsl:with-param name="pIsNotionalDifferent" select="$pIsNotionalDifferent"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:choose>
            <xsl:when test ="$varScheduledNominalAnnexNumber &lt; 1"/>

            <xsl:when test ="$varScheduledNominalAnnexNumber &gt;= 1">

              <!-- varAnnexAText -->
              <xsl:variable name="varAnnexAText">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'Annex_'"/>
                </xsl:call-template>
                <xsl:text> A </xsl:text>
                <xsl:text> ( </xsl:text>
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'NotionalSchedule'"/>
                </xsl:call-template>
                <xsl:text> )</xsl:text>
              </xsl:variable>

              <fo:block keep-together.within-page="always"  break-before="{'page'}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="displayTitle">
                    <xsl:with-param name="pText" select="$varAnnexAText"/>
                  </xsl:call-template>
                </fo:block>

                <fo:block border="{$gVarBorder}" margin-left="1.3cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
                  <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
                    <xsl:call-template name="create3Columns"/>
                    <fo:table-body>
                      <fo:table-row>
                        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                          <fo:block border="{$gVarBorder}" font-weight="{$varBoldFontWeidth}" text-align="{$varAnnexSectionColumnLeftAlign}">
                            <xsl:call-template name="getTranslation">
                              <xsl:with-param name="pResourceName" select="'FromAndIncluding'"/>
                            </xsl:call-template>
                          </fo:block>
                        </fo:table-cell>
                        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                          <fo:block border="{$gVarBorder}" font-weight="{$varBoldFontWeidth}" text-align="{$varAnnexSectionColumnLeftAlign}">
                            <xsl:call-template name="getTranslation">
                              <xsl:with-param name="pResourceName" select="'ToButExcluding'"/>
                            </xsl:call-template>
                          </fo:block>
                        </fo:table-cell>
                        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                          <fo:block border="{$gVarBorder}" font-weight="{$varBoldFontWeidth}" text-align="{$varAnnexSectionColumnRightAlign}" >
                            <xsl:call-template name="getTranslation">
                              <xsl:with-param name="pResourceName" select="'NotionalAmount_'"/>
                            </xsl:call-template>
                            <xsl:text>(</xsl:text>
                            <xsl:call-template name="getCurrency">
                              <xsl:with-param name="currency" select="$pStream[1]/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/currency"/>
                            </xsl:call-template>
                            <xsl:text>)</xsl:text>
                          </fo:block>
                        </fo:table-cell>
                      </fo:table-row>
                      <fo:table-row>
                        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                          <fo:block border="{$gVarBorder}" font-weight="{$varBoldFontWeidth}" text-align="{$varAnnexSectionColumnLeftAlign}">

                          </fo:block>
                        </fo:table-cell>
                        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                          <fo:block border="{$gVarBorder}" font-weight="{$varBoldFontWeidth}" text-align="{$varAnnexSectionColumnLeftAlign}">

                          </fo:block>
                        </fo:table-cell>
                        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                          <fo:block border="{$gVarBorder}" font-weight="{$varBoldFontWeidth}" text-align="{$varAnnexSectionColumnRightAlign}" >

                          </fo:block>
                        </fo:table-cell>
                      </fo:table-row>
                      <!--	
												Notional schedule (as annex) is available only if the events exist in XML
												-->
                      <xsl:for-each select="//Event[STREAMNO= '1' and EVENTCODE='NOS' and EVENTTYPE='NOM']/EventClass[EVENTCLASS='DAT']">
                        <fo:table-row>
                          <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                            <fo:block border="{$gVarBorder}" text-align="{$varAnnexSectionColumnLeftAlign}">
                              <xsl:call-template name="format-date">
                                <xsl:with-param name="xsd-date-time" select="../DTSTARTADJ"/>
                              </xsl:call-template>
                            </fo:block>
                          </fo:table-cell>
                          <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                            <fo:block border="{$gVarBorder}" text-align="{$varAnnexSectionColumnLeftAlign}">
                              <xsl:call-template name="format-date">
                                <xsl:with-param name="xsd-date-time" select="../DTENDADJ"/>
                              </xsl:call-template>
                            </fo:block>
                          </fo:table-cell>
                          <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                            <fo:block border="{$gVarBorder}" text-align="{$varAnnexSectionColumnRightAlign}">
                              <xsl:call-template name="getAmountWithoutCurrency">
                                <xsl:with-param name="amount" select="../VALORISATION"/>
                              </xsl:call-template>
                            </fo:block>
                          </fo:table-cell>
                        </fo:table-row>
                      </xsl:for-each>
                    </fo:table-body>
                  </fo:table>
                </fo:block>
              </fo:block>
            </xsl:when>
          </xsl:choose>
        </xsl:for-each>
      </xsl:when>
      <!-- when notional amount is different for all streams (pIsNotionalDifferent = true) 
			     - it scans all streams and it returns each stream amortizing -->
      <xsl:otherwise>
        <xsl:for-each select ="$pStream">

          <!-- varPosition -->
          <xsl:variable name="varPosition" select="position()" />

          <!-- varStreamNo -->
          <xsl:variable name="varStreamNo">
            <xsl:value-of select="$varPosition"/>
          </xsl:variable>

          <xsl:variable name ="varScheduledNominalAnnexNumber">
            <xsl:call-template name ="getScheduledNominalAnnexNumber">
              <xsl:with-param name="pIsNotionalDifferent" select="$pIsNotionalDifferent"/>
            </xsl:call-template>
          </xsl:variable>

          <!-- varAnnexText -->
          <xsl:variable name="varAnnexText">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'Annex_'"/>
            </xsl:call-template>
            <xsl:text> </xsl:text>
            <xsl:call-template name ="getLetterFromNumber">
              <xsl:with-param name="pNumber" select="$varScheduledNominalAnnexNumber"/>
            </xsl:call-template>
            <xsl:text> ( </xsl:text>
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'NotionalSchedule'"/>
            </xsl:call-template>
            <xsl:text> )</xsl:text>
          </xsl:variable>

          <!-- It displays period and amount only for scheduled nominal streams  -->
          <xsl:if test ="$varScheduledNominalAnnexNumber &gt;= 1">

            <fo:block keep-together.within-page="always"  break-before="{'page'}">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="displayTitle">
                  <xsl:with-param name="pText" select="$varAnnexText"/>
                </xsl:call-template>
              </fo:block>

              <fo:block border="{$gVarBorder}" margin-left="1.3cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
                <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
                  <xsl:call-template name="create3Columns"/>
                  <fo:table-body>
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}" font-weight="{$varBoldFontWeidth}" text-align="{$varAnnexSectionColumnLeftAlign}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'FromAndIncluding'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}" font-weight="{$varBoldFontWeidth}" text-align="{$varAnnexSectionColumnLeftAlign}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'ToButExcluding'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}" font-weight="{$varBoldFontWeidth}" text-align="{$varAnnexSectionColumnRightAlign}" >
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'NotionalAmount_'"/>
                          </xsl:call-template>
                          <xsl:text>(</xsl:text>
                          <xsl:call-template name="getCurrency">
                            <xsl:with-param name="currency" select="calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/currency"/>
                          </xsl:call-template>
                          <xsl:text>)</xsl:text>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}" font-weight="{$varBoldFontWeidth}" text-align="{$varAnnexSectionColumnLeftAlign}">

                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}" font-weight="{$varBoldFontWeidth}" text-align="{$varAnnexSectionColumnLeftAlign}">

                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}" font-weight="{$varBoldFontWeidth}" text-align="{$varAnnexSectionColumnRightAlign}" >

                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                    <!--	
										Notional schedule (as annex) is available only if the events exist in XML
										-->
                    <xsl:for-each select="//Event[INSTRUMENTNO='1'and STREAMNO=$varStreamNo and EVENTCODE='NOS' and EVENTTYPE='NOM']/EventClass[EVENTCLASS='DAT']">
                      <fo:table-row>
                        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                          <fo:block border="{$gVarBorder}" text-align="{$varAnnexSectionColumnLeftAlign}">
                            <xsl:call-template name="format-date">
                              <xsl:with-param name="xsd-date-time" select="../DTSTARTADJ"/>
                            </xsl:call-template>
                          </fo:block>
                        </fo:table-cell>
                        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                          <fo:block border="{$gVarBorder}" text-align="{$varAnnexSectionColumnLeftAlign}">
                            <xsl:call-template name="format-date">
                              <xsl:with-param name="xsd-date-time" select="../DTENDADJ"/>
                            </xsl:call-template>
                          </fo:block>
                        </fo:table-cell>
                        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                          <fo:block border="{$gVarBorder}" text-align="{$varAnnexSectionColumnRightAlign}">
                            <xsl:call-template name="getAmountWithoutCurrency">
                              <xsl:with-param name="amount" select="../VALORISATION"/>
                            </xsl:call-template>
                          </fo:block>
                        </fo:table-cell>
                      </fo:table-row>
                    </xsl:for-each>
                  </fo:table-body>
                </fo:table>
              </fo:block>
            </fo:block>
          </xsl:if>
        </xsl:for-each>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- ================================================================================================================ -->
  <!-- End region: display Annex                                                                                        -->
  <!-- ================================================================================================================ -->

  <!-- ================================================================================================================ -->
  <!-- Begin region: display receiver adress                                                                            -->
  <!-- ================================================================================================================ -->
  <xsl:template name="displayReceiverAddress">
    <xsl:variable name="varTableWidthCM">
      <xsl:value-of select="$gVarA4VerticalPageWidthCM - $varA4LeftMarginCM - $varA4RightMarginCM - $varReceiverAdressLeftMarginCM"/>
    </xsl:variable>

    <fo:block margin-left="0cm" margin-top="{$varReceiverAdressTopMarginCM}cm" font-size="{$varReceiverAdressFontSize}">
      <fo:table margin-left="0cm" border="{$gVarBorder}" text-align="{$varReceiverAdressTextAlign}" table-layout="fixed">
        <fo:table-column column-width="{$varReceiverAdressLeftMarginCM}cm" column-number="01"></fo:table-column>
        <fo:table-column column-width="{$varTableWidthCM}cm" column-number="02"></fo:table-column>
        <fo:table-body>
          <!-- First address line: we use the bold (usually it is the client name) -->
          <fo:table-row>
            <fo:table-cell border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:value-of select="$gVarSpace"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$gVarBorder}" font-weight="normal" font-family="{$varFontFamilyCourier}">
              <fo:block border="{$gVarBorder}">
                <xsl:value-of select="$address1"/>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
          <!-- Others address lines: we use normal characters -->
          <xsl:for-each select="//header/sendTo[1]/routingIdsAndExplicitDetails/routingAddress/streetAddress/streetLine[position()>1]">
            <fo:table-row>
              <fo:table-cell border="{$gVarBorder}" font-family="{$varFontFamilyCourier}">
                <fo:block border="{$gVarBorder}"></fo:block>
              </fo:table-cell>
              <fo:table-cell border="{$gVarBorder}" font-weight="normal" font-family="{$varFontFamilyCourier}">
                <fo:block border="{$gVarBorder}">
                  <xsl:value-of select="."/>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
          </xsl:for-each>
        </fo:table-body>
      </fo:table>
    </fo:block>

    <xsl:call-template name="displayBlockBreaklines">
      <xsl:with-param name="min" select="1"/>
      <xsl:with-param name="max" select="3"/>
    </xsl:call-template>
  </xsl:template>
  <!-- ================================================================================================================ -->
  <!-- End region: display receiver adress                                                                              -->
  <!-- ================================================================================================================ -->


  <!-- ================================================================================================================ -->
  <!-- Begin region: display date                                                                                       -->
  <!-- ================================================================================================================ -->
  <xsl:template name="displayDate">
    <xsl:variable name="formattedDate">
      <xsl:call-template name="format-date">
        <xsl:with-param name="xsd-date-time" select="$varCreationTimestamp"/>
      </xsl:call-template>
    </xsl:variable>
    <fo:block border="{$gVarBorder}" margin-left="{$varDisplayDateLeftMarginCM}cm" margin-top="{$varDisplayDateTopMarginCM}cm" font-size="{$varDisplayDateFontSize}" font-weight="{$varDisplayDateArticleFontWeight}">
      <fo:inline>
        <xsl:if test = "$pCurrentCulture != 'en-GB'">
          <xsl:call-template name="getSenderCity"/>
        </xsl:if>
      </fo:inline>
      <fo:inline>
        <xsl:value-of select="$formattedDate"/>
      </fo:inline>

      <xsl:call-template name="displayBlockBreaklines">
        <xsl:with-param name="min" select="1"/>
        <xsl:with-param name="max" select="3"/>
      </xsl:call-template>

    </fo:block>
  </xsl:template>
  <!-- ================================================================================================================ -->
  <!-- End region: display display date                                                                                 -->
  <!-- ================================================================================================================ -->


  <!-- ================================================================================================================ -->
  <!-- Begin region: display receiver, sender and facture number                                                        -->
  <!-- Ex.                                                                                                              -->
  <!-- To : SOCIETE GENERALE                                                                                            -->
  <!-- From : BNP Paribas                                                                                               -->
  <!-- F/Ref. : 1234567890                                                                                              -->
  <!-- ================================================================================================================ -->
  <xsl:template name="displaySendBy">
    <xsl:param name="pSubject"/>

    <xsl:variable name="varTableWidthCM">
      <xsl:value-of select="$gVarA4VerticalPageWidthCM - $varA4LeftMarginCM - $varA4RightMarginCM - $varSendByLeftMarginCM"/>
    </xsl:variable>
    <xsl:variable name="varDynamicDataColumnWidthCM">
      <xsl:value-of select="$varTableWidthCM - $varSendByStaticDataWidthCM"/>
    </xsl:variable>
    <fo:block margin-left="{$varSendByLeftMarginCM}cm" margin-top="{$varSendByTopMarginCM}cm" font-size="{$varSendByFontSize}">
      <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varTableWidthCM}cm" table-layout="fixed">
        <fo:table-column column-width="{$varSendByStaticDataWidthCM}cm"  column-number="01"></fo:table-column>
        <fo:table-column column-width="{$varDynamicDataColumnWidthCM}cm" column-number="02"></fo:table-column>
        <fo:table-body>

          <!-- First row. To: -->
          <fo:table-row>
            <fo:table-cell margin-left="0cm" border="{$gVarBorder}" font-weight="{$varSendByStaticSectionFontWeight}">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'To'"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell margin-left="0cm" border="{$gVarBorder}" font-weight="{$varSendByDynamicSectionFontWeight}">
              <fo:block text-align="{$varSendByDataAlign}">
                <xsl:call-template name="getSendTo"/>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>

          <!-- To/Ref and F/Ref: when do we display them? -->

          <!-- Case 1: To/Ref != From/Ref, To/Ref != 0, To/Ref not null, From/Ref!=0 and From/Ref not null: we display To/Ref and F/Ref -->
          <!-- Example: To/Ref = 1204, From/Ref: 1251. It displays To/Ref=1204 and F/Ref=1251 -->

          <!-- Case 2: To/Ref = From/Ref and To/Ref != 0 and To/Ref not null: we do not display To/Ref but only F/Ref -->
          <!-- Example: To/Ref = 1204, From/Ref: 1024. It displays F/Ref = 1204 and does not display T/Ref -->

          <!-- Case 3: To/Ref is 0 or null: we do not display To/Ref -->
          <!-- Example: To/Ref = 0. It does not display To/Ref -->

          <!-- Case 4: To/Ref != 0, To/Ref not null, F/Ref = 0 or null: it displays F/Ref with the value of T/Ref -->
          <!-- Example: To/Ref = 1204, F/Ref = 0. It displays F/Ref = 1204 -->

          <!-- 2° row (not always displayed). To/Ref.: -->
          <xsl:if test="($TradeIdTo != $TradeIdFrom) and (string-length($TradeIdTo) > 0) and ($TradeIdTo != 0) and ($TradeIdFrom != 0)">
            <fo:table-row>
              <fo:table-cell margin-left="0cm" border="{$gVarBorder}" font-weight="{$varSendByStaticSectionFontWeight}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'T/Ref'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell margin-left="0cm" border="{$gVarBorder}" font-weight="{$varSendByDynamicSectionFontWeight}">
                <fo:block text-align="{$varSendByDataAlign}">
                  <xsl:call-template name="getTradeIdTo"/>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
          </xsl:if>
          <!-- 3° row. From: -->
          <fo:table-row>
            <fo:table-cell margin-left="0cm" border="{$gVarBorder}" font-weight="{$varSendByStaticSectionFontWeight}">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'From'"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell margin-left="0cm" border="{$gVarBorder}" font-weight="{$varSendByDynamicSectionFontWeight}">
              <fo:block text-align="{$varSendByDataAlign}">
                <xsl:call-template name="getSendBy" />
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
          <!-- 4° row: F/Ref: -->
          <fo:table-row>
            <fo:table-cell margin-left="0cm" border="{$gVarBorder}" font-weight="{$varSendByStaticSectionFontWeight}">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'F/Ref'"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell margin-left="0cm" border="{$gVarBorder}" font-weight="{$varSendByDynamicSectionFontWeight}">
              <fo:block text-align="{$varSendByDataAlign}">
                <xsl:choose>
                  <xsl:when test="(string-length($TradeIdFrom) = 0) or ($TradeIdFrom = 0)">
                    <xsl:call-template name="getTradeIdTo" />
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:call-template name="getTradeIdFrom" />
                  </xsl:otherwise>
                </xsl:choose>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>

          <!-- 5° row: [Re:] = Subject -->
          <xsl:if test ="$pSubject">
            <fo:table-row height="0.5cm"/>
            <fo:table-row>
              <fo:table-cell margin-left="0cm" border="{$gVarBorder}" font-weight="{$varSendByStaticSectionFontWeight}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Subject'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell margin-left="0cm" border="{$gVarBorder}" font-weight="{$varSendByDynamicSectionFontWeight}">
                <fo:block text-align="{$varSendByDataAlign}">
                  <xsl:value-of select="$pSubject"/>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
          </xsl:if>
        </fo:table-body>
      </fo:table>
    </fo:block>
    <xsl:call-template name="displayBlockBreaklines">
      <xsl:with-param name="min" select="1"/>
      <xsl:with-param name="max" select="3"/>
    </xsl:call-template>
  </xsl:template>
  <!-- ================================================================================================================ -->
  <!-- End region: display receiver, sender and facture number                                                          -->
  <!-- ================================================================================================================ -->

  <!-- ================================================================================================================ -->
  <!-- Begin region: display title																					  -->
  <!-- ================================================================================================================ -->
  <xsl:template name="displayTitle">

    <xsl:param name="pTopMarginCM"          select="$varProductSectionPanelTopMarginCM"/>
    <xsl:param name="pLeftMarginCM"         select="$varProductSectionPanelLeftMargin"/>
    <xsl:param name="pRightMarginCM"        select="$varProductSectionPanelRightMargin"/>
    <xsl:param name="pFontSize"             select="$varProductSectionPanelFontSize"/>
    <xsl:param name="pTextColor"            select="$varProductSectionPanelTextColor"/>
    <xsl:param name="pBackgroundColor"      select="$varProductSectionPanelBackgColor"/>
    <xsl:param name="pFontWeight"           select="$varProductSectionPanelFontWeigth"/>
    <xsl:param name="pTextAlign"            select="$varProductSectionPanelTextAlign"/>
    <xsl:param name="pText"/>

    <xsl:call-template name="displayBlockBreakline"/>
    <fo:block margin-top="{$pTopMarginCM}cm" margin-left="{$pLeftMarginCM}cm" margin-right="{$pRightMarginCM}cm"
				  font-size="{$pFontSize}" font-weight="{$pFontWeight}" text-align="{$pTextAlign}" color="{$pTextColor}"
			      border-bottom="1pt solid {$pTextColor}"  >
      <fo:list-block>
        <fo:list-item>

          <fo:list-item-label end-indent="label-end()">
            <fo:block font-weight="bold" font-size="12pt">
              <!--<fo:character character="&#x2022;"/>-->
            </fo:block>
          </fo:list-item-label>

          <fo:list-item-body start-indent="0mm">
            <fo:block>
              <xsl:value-of select="$pText"/>
            </fo:block>
          </fo:list-item-body>
        </fo:list-item>
      </fo:list-block>
    </fo:block>
    <xsl:call-template name="displayBlockBreakline"/>

  </xsl:template>
  <!-- ================================================================================================================ -->
  <!-- End region: display Title                                                                                        -->
  <!-- ================================================================================================================ -->

  <!-- ================================================================================================================ -->
  <!-- Begin region: display round panel                                                                                -->
  <!-- ================================================================================================================ -->
  <xsl:template name="displayRoundPanel">
    <xsl:param name="pTopMarginCM"/>
    <xsl:param name="pLeftMarginCM"/>
    <xsl:param name="pRightMarginCM"/>
    <xsl:param name="pFontSize"/>
    <xsl:param name="pTextColor"/>
    <xsl:param name="pBackgroundColor"/>
    <xsl:param name="pFontWeight"/>
    <xsl:param name="pTextAlign"/>
    <xsl:param name="pText"/>

    <fo:block padding="0.1cm" margin-top="{$pTopMarginCM}cm" margin-left="{$pLeftMarginCM}cm" margin-right="{$pRightMarginCM}cm" font-size="{$pFontSize}" font-weight="{$pFontWeight}" text-align="{$pTextAlign}" color="{$pTextColor}" background-color="{$pBackgroundColor}">
      <xsl:value-of select="$pText"/>
    </fo:block>
  </xsl:template>
  <!-- ================================================================================================================ -->
  <!-- End region: display round panel                                                                                  -->
  <!-- ================================================================================================================ -->

  <!-- ================================================================================================================ -->
  <!-- BEGIN REGION: create columns                                                                                     -->
  <!-- ================================================================================================================ -->
  <xsl:template name="create1Column">
    <fo:table-column column-width="{$varProductSectionTextColumnWidthCM}cm" column-number="01"></fo:table-column>
  </xsl:template>

  <xsl:template name="createColumns">
    <fo:table-column column-width="{$varProductSectionTextColumnWidthCM}cm" column-number="01"></fo:table-column>
    <fo:table-column column-width="{$varProductSectionDataColumnWidthCM}cm" column-number="02"></fo:table-column>
  </xsl:template>

  <xsl:template name="create3Columns">
    <fo:table-column column-width="{$varAnnexSectionTextLongColumnWidthCM}cm" column-number="01"></fo:table-column>
    <fo:table-column column-width="{$varAnnexSectionTextShortColumnWidthCM}cm" column-number="02"></fo:table-column>
    <fo:table-column column-width="{$varAnnexSectionTextShortColumnWidthCM}cm" column-number="03"></fo:table-column>
  </xsl:template>

  <xsl:template name="create3ColumnsForBonds">
    <fo:table-column column-width="{$gVarAnnexSectionTextLongColumnWidth5MM}cm" column-number="01"></fo:table-column>
    <fo:table-column column-width="{$gVarAnnexSectionTextLongColumnWidth15MM}cm" column-number="02"></fo:table-column>
    <fo:table-column column-width="{$gVarAnnexSectionTextLongColumnWidth100MM}cm" column-number="03"></fo:table-column>
  </xsl:template>
  <!-- ================================================================================================================ -->
  <!-- END REGION: create columns                                                                                       -->
  <!-- ================================================================================================================ -->

  <!-- ================================================================================================================ -->
  <!-- BEGIN REGION: template display IRD calculation agent                                                             -->
  <!-- ================================================================================================================ -->
  <xsl:template name="displayIrdCalculationAgent">

    <xsl:variable name="calculation">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'Calculation'"/>
      </xsl:call-template>
    </xsl:variable>

    <fo:block keep-together.within-page="always">
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$calculation"/>
        </xsl:call-template>
      </fo:block>

      <fo:block margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>
            <!-- 
						==================
						Calculation Agent
						==================
						-->
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'CalculationAgent'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getCalculationAgent">
                    <xsl:with-param name="pTrade" select="//trade"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>
  </xsl:template>
  <!-- ================================================================================================================ -->
  <!-- END REGION: template display IRD calculation agent                                                               -->
  <!-- ================================================================================================================ -->

  <!-- ================================================================================================================ -->
  <!-- BEGIN REGION: template display brokerage                                                                         -->
  <!--Display Brokerage only for the party to which we are sending the confirmation                                     -->
  <!-- ================================================================================================================ -->
  <xsl:variable name="sendToID">
    <xsl:call-template name="getSendToID" />
  </xsl:variable>

  <xsl:template name="displayBrokerage">

    <xsl:variable name="fees">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'Fees'"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:if test="//dataDocument/trade/otherPartyPayment[paymentType = 'Brokerage']">
      <fo:block keep-together.within-page="always">
        <fo:block border="{$gVarBorder}">
          <xsl:call-template name="displayTitle">
            <xsl:with-param name="pText" select="$fees"/>
          </xsl:call-template>
        </fo:block>
        <fo:block margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
          <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
            <xsl:call-template name="createColumns"/>
            <fo:table-body>
              <xsl:choose>
                <!-- 
								If trade XML contains Brokerage events, the code uses them prioritily 
								-->
                <xsl:when test ="//Event[EVENTCODE='OPP' and EVENTTYPE='BRO' and IDA_PAY = $varSendToPartyID]">
                  <xsl:for-each select="//Event[EVENTCODE='OPP' and EVENTTYPE='BRO' and IDA_PAY = $varSendToPartyID]">
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:value-of select="./paymentType"/>
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'Brokerage_Amount'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="format-money2">
                            <xsl:with-param name="currency" select="./UNIT"/>
                            <xsl:with-param name="amount" select="./VALORISATION"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                    <!--
										These variables return OTCml Actor identifier for Brokerage Payer and Receiver
										-->
                    <xsl:variable name="varBrokeragePayerName">
                      <xsl:value-of select="./IDA_PAY"/>
                    </xsl:variable>
                    <xsl:variable name="varBrokerageReceiverName">
                      <xsl:value-of select="./IDA_REC"/>
                    </xsl:variable>

                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:value-of select="./paymentType"/>
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'Brokerage_Payer'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:value-of select="//party[@OTCmlId=$varBrokeragePayerName]/partyName"/>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:value-of select="./paymentType"/>
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'Brokerage_Receiver'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:value-of select="//party[@OTCmlId=$varBrokerageReceiverName]/partyName"/>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                  </xsl:for-each>
                </xsl:when>
                <!-- 
								If trade XML not contains Brokerage events, the code uses //otherPartyPayment node (old script)
								-->
                <xsl:otherwise>
                  <xsl:for-each select="//dataDocument/trade/otherPartyPayment[paymentType = 'Brokerage' and payerPartyReference/@href = $sendToID]">
                    <xsl:if test="position() != 1">
                      <fo:table-row>
                        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                          <fo:block border="{$gVarBorder}">
                          </fo:block>
                        </fo:table-cell>
                      </fo:table-row>
                    </xsl:if>
                    <xsl:if test="./paymentQuote">
                      <fo:table-row>
                        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                          <fo:block border="{$gVarBorder}">
                            <xsl:value-of select="./paymentType"/>:
                          </fo:block>
                        </fo:table-cell>
                        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                          <fo:block border="{$gVarBorder}">
                            <xsl:choose>
                              <xsl:when test="./paymentQuote/percentageRateFraction">
                                <xsl:value-of select="./paymentQuote/percentageRateFraction"/>
                              </xsl:when>
                              <xsl:otherwise>
                                <xsl:value-of select="./paymentQuote/percentageRate"/>
                              </xsl:otherwise>
                            </xsl:choose>
                          </fo:block>
                        </fo:table-cell>
                      </fo:table-row>
                    </xsl:if>
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:value-of select="./paymentType"/>
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'_Amount'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="formatMoney">
                            <xsl:with-param name="pMoney" select="./paymentAmount"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:value-of select="./paymentType"/>
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'_Payer'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getPartyNameOrID">
                            <xsl:with-param name="pPartyID" select="./payerPartyReference/@href"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:value-of select="./paymentType"/>
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'_Receiver'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getPartyNameOrID">
                            <xsl:with-param name="pPartyID" select="./receiverPartyReference/@href"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                  </xsl:for-each>
                </xsl:otherwise>
              </xsl:choose>
            </fo:table-body>
          </fo:table>
        </fo:block>
      </fo:block>
    </xsl:if>
  </xsl:template>
  <!-- ================================================================================================================ -->
  <!-- END REGION: template display brokerage                                                                           -->
  <!-- ================================================================================================================ -->

  <!-- ================================================================================================================ -->
  <!-- Begin region: display Formula	-->
  <!-- GS 20100106 Add Formula region -->
  <!-- ================================================================================================================ -->

  <xsl:template name="displayFormula">

    <!-- it extract the trade identifier from XML flow-->
    <xsl:variable name ="varTradeIdentifier">
      <xsl:value-of select ="//dataDocument/trade/tradeHeader/partyTradeIdentifier/tradeId[@tradeIdScheme='http://www.euro-finance-systems.fr/otcml/tradeid']"></xsl:value-of>
    </xsl:variable>

    <!-- it builds SQL query (table ATTACHEDDOC)-->
    <xsl:variable name="varQueryATTACHEDDOC">
      <xsl:text>select ID, LODOC from ATTACHEDDOC where TABLENAME = 'TRADE' and DOCNAME like 'Formul%'and ID = (select IDT from TRADE where IDENTIFIER = '</xsl:text>
      <xsl:value-of select ="$varTradeIdentifier"/>
      <xsl:text>'))</xsl:text>
    </xsl:variable>

    <!-- varImgFormula variable: it builds the concat syntax -->
    <xsl:variable name="varImgFormula">
      <xsl:value-of select="concat( 'sql(', $varQueryATTACHEDDOC )"/>
    </xsl:variable>

    <!--<xsl:variable name="varFormula">
			<xsl:call-template name="getTranslation">
				<xsl:with-param name="pResourceName" select="'Formula'"/>
			</xsl:call-template>
		</xsl:variable>-->

    <fo:block keep-together.within-page="always">
      <!--PL 20150324 Remove displayBlockBreakline-->
      <!--<fo:block border="{$gVarBorder}">
				<xsl:call-template name="displayBlockBreakline"/>
				-->
      <!--<xsl:call-template name="displayTitle">
					<xsl:with-param name="pText" select="$varFormula"/>
				</xsl:call-template>-->
      <!--
			</fo:block>-->

      <fo:block margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <fo:external-graphic src="{$varImgFormula}" text-align="left" display-align="before" height="{$varImgFormulaHeightCM}cm"/>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>

    <!--PL 20150324 Remove displayBlockBreakline-->
    <!--<xsl:call-template name="displayBlockBreakline"/>-->
  </xsl:template>

  <!-- ================================================================================================================ -->
  <!-- End region: display Formula																					  -->
  <!-- ================================================================================================================ -->

  <!-- ================================================================================== -->
  <!-- BEGIN REGION: template display footer product                                      -->
  <!-- ================================================================================== -->

  <xsl:template name="displayFooter_Product">

    <!-- ==============================================================-->
    <!-- template display footer product: begin region account details -->
    <!-- ==============================================================-->
    <xsl:variable name="accountDetails">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'AccountDetails'"/>
      </xsl:call-template>
    </xsl:variable>

    <fo:block border="{$gVarBorder}">
      <xsl:call-template name="displayTitle">
        <xsl:with-param name="pText" select="$accountDetails"/>
      </xsl:call-template>
    </fo:block>

    <fo:block keep-together.within-page="always">

      <fo:block border="{$gVarBorder}" margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">

        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'AccountForPayments'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'AccountTo'"/>
                  </xsl:call-template>
                  <xsl:value-of select="$gVarSpace"/>
                  <xsl:value-of select="$varParty_1_name" />:
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'StandardSettlementInstructions'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'AccountForPayments'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'AccountTo'"/>
                  </xsl:call-template>
                  <xsl:value-of select="$gVarSpace"/>
                  <xsl:value-of select="$varParty_2_name" />:
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'StandardSettlementInstructions'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>
    <!-- ============================================================-->
    <!-- template display footer product: end region account details -->
    <!-- ============================================================-->

    <!-- ======================================================-->
    <!-- template display footer product: begin region offices -->
    <!-- ======================================================-->

    <xsl:call-template name="displayBlockBreaklines"/>

    <xsl:variable name="offices">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'Offices'"/>
      </xsl:call-template>
    </xsl:variable>

    <fo:block keep-together.within-page="always">
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$offices"/>
        </xsl:call-template>
      </fo:block>

      <fo:block border="{$gVarBorder}" margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>

            <!-- (a) office -->
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  (a)
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'TheOfficeOf'"/>
                  </xsl:call-template>
                  <xsl:call-template name="getSendTo"/>
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'forTheTransactionIs'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>

              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:value-of select="$sendToRoutingAddress"/>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- (b) office -->
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  (b)
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'TheOfficeOf'"/>
                  </xsl:call-template>
                  <xsl:call-template name="getSendBy"/>
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'forTheTransactionIs'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:value-of select="$sendByRoutingAddress"/>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>
    <!-- ===================================================-->
    <!-- template display footer product: end region offices-->
    <!-- ===================================================-->
  </xsl:template>
  <!-- ================================================================================== -->
  <!-- END REGION: template display footer product                                        -->
  <!-- ================================================================================== -->

  <!-- ================================================================================================================ -->
  <!-- BEGIN REGION: template display signature text                                                                    -->
  <!-- ================================================================================================================ -->
  <!-- Display Signature -->
  <xsl:template name="displaySignatureText">

    <!-- Variable: signature text -->
    <xsl:variable name="pSignatureText">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'Signature'"/>
      </xsl:call-template>
    </xsl:variable>

    <fo:block text-align="justify" margin-left="{$varSignatureTextLeftMarginCM}cm" margin-right="{$varSignatureTextRightMarginCM}cm" margin-top="{$varSignatureTextTopMarginCM}cm" font-size="{$varSignatureTextFontSize}">
      <xsl:value-of select="$pSignatureText"/>
    </fo:block>
  </xsl:template>
  <!-- ================================================================================================================ -->
  <!-- END REGION: template display signature text                                                                      -->
  <!-- ================================================================================================================ -->


  <!-- ================================================================================================================ -->
  <!-- BEGIN REGION: template display sender and receiver signature                                                     -->
  <!-- ================================================================================================================ -->
  <!-- Display Signature -->
  <xsl:template name="displaySenderReceiverSignatures">

    <!-- Text size for 'Back-Office Manager'-->
    <xsl:variable name="varBackOfficeTextFontSize">
      <!-- EG 20160404 Migration vs2013 -->
      <!--<xsl:value-of select="$varSenderReceiverSignaturesFontSize - 1"/>-->
      <xsl:value-of select="$varSenderReceiverSignaturesFontSize"/>
    </xsl:variable>

    <fo:block keep-together="always" text-align="left" margin-top="{$varSenderSignaturesTopMarginCM}cm" font-size="{$varSenderReceiverSignaturesFontSize}">

      <fo:block keep-together="always" margin-left="{$varSenderSignaturesLeftMarginCM}cm" margin-top="{$varSenderSignaturesTopMarginCM}cm">
        <fo:block border="{$gVarBorder}">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'YoursSincerely'"/>
          </xsl:call-template>
        </fo:block>
        <fo:block border="{$gVarBorder}">
          <xsl:call-template name="getSendBy" />
        </fo:block>
        <xsl:call-template name="displayBlockBreakline"/>
        <fo:block border="{$gVarBorder}">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'By'"/>
          </xsl:call-template>
        </fo:block>
        <fo:block border="{$gVarBorder}">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'Name'"/>
          </xsl:call-template>
        </fo:block>
        <fo:inline>
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'Title'"/>
          </xsl:call-template>
          <fo:inline text-decoration="underline" font-style="italic" font-size="{$varBackOfficeTextFontSize}">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'Back-OfficeManager'"/>
            </xsl:call-template>
          </fo:inline>
        </fo:inline>
      </fo:block>

      <fo:block keep-together="always" text-align="left" margin-left="{$varReceiverSignatureLeftMarginCM}cm" margin-top="{$varReceiverSignatureTopMarginCM}cm">
        <fo:block >
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'Confirmed'"/>
          </xsl:call-template>
        </fo:block>
        <fo:block border="{$gVarBorder}">
          <xsl:call-template name="getSendTo" />
        </fo:block>
        <xsl:call-template name="displayBlockBreakline"/>
        <fo:block border="{$gVarBorder}">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'By'"/>
          </xsl:call-template>
        </fo:block>
        <fo:block border="{$gVarBorder}">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'Name'"/>
          </xsl:call-template>
        </fo:block>
        <fo:block border="{$gVarBorder}">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'Title'"/>
          </xsl:call-template>
        </fo:block>
      </fo:block>
    </fo:block>
  </xsl:template>
  <!-- ================================================================================================================ -->
  <!-- END REGION: template display sender signature                                                                    -->
  <!-- ================================================================================================================ -->



  <!-- ================================================================================================================ -->
  <!-- BEGIN REGION: breakline between blocks                                                                           -->
  <!-- ================================================================================================================ -->
  <xsl:template name="displayBlockBreakline">
    <xsl:param name="lineHeight" select="0.6"/>
    <fo:block border="{$gVarBorder}" line-height="{$lineHeight}">&#xA0;</fo:block>
  </xsl:template>
  <!-- ================================================================================================================ -->
  <!-- END REGION: breaklineS between blocks                                                                             -->
  <!-- ================================================================================================================ -->

  <!-- ================================================================================================================ -->
  <!-- BEGIN REGION: breaklineS between blocks                                                                           -->
  <!-- ================================================================================================================ -->

  <xsl:template name="displayBlockBreaklines">
    <xsl:param name="min" select="0"/>
    <xsl:param name="max" select="2"/>
    <xsl:call-template name="displayBlockBreakline"/>
    <xsl:if test="number($min) &lt; number($max - 1)">
      <xsl:call-template name="displayBlockBreaklines">
        <xsl:with-param name="min" select="$min + 1"/>
        <xsl:with-param name="max" select="$max"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <!-- ================================================================================================================ -->
  <!-- END REGION: breakline between blocks                                                                             -->
  <!-- ================================================================================================================ -->


  <!-- ================================================================================================================ -->
  <!-- BEGIN REGION: empty row for two columns table                                                                    -->
  <!-- ================================================================================================================ -->
  <xsl:template name="insertEmptyRow">
    <xsl:param name="pHeightCM"/>
    <fo:table-row height ="{$pHeightCM}cm">
      <fo:table-cell border="{$gVarBorder}">
        <fo:block border="{$gVarBorder}"></fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$gVarBorder}">
        <fo:block border="{$gVarBorder}"></fo:block>
      </fo:table-cell>
    </fo:table-row>
  </xsl:template>
  <!-- ================================================================================================================ -->
  <!-- END REGION: breakline between blocks                                                                             -->
  <!-- ================================================================================================================ -->

  <!-- 
	**************************************************************************************
	GS 15102009
	Symptom: Not Display carriage return in notepad text box
	Add displayNotepad and displaySplitNotepad templates
	When the notepad tag contains text with breakline, these templates split the texts. 	
	displaySplitNotepad template is a recursive function
	***************************************************************************************
	-->
  <xsl:template name="displayNotepad">
    <fo:block margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
      <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
        <xsl:call-template name="create1Column"/>
        <fo:table-body>
          <xsl:call-template name="displaySplitNotepad">
            <xsl:with-param name="pNotepad">
              <xsl:value-of select="//notepad"/>
            </xsl:with-param>
          </xsl:call-template>
        </fo:table-body>
      </fo:table>
    </fo:block>
  </xsl:template>

  <xsl:template name="displaySplitNotepad">
    <xsl:param name="pNotepad"/>
    <xsl:choose>
      <xsl:when test="contains($pNotepad,'&#x0A;')">
        <fo:table-row>
          <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0.2cm" border="{$gVarBorder}">
            <fo:block border="{$gVarBorder}">
              <xsl:value-of select="substring-before($pNotepad,'&#x0A;')"/>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
        <xsl:call-template name="displaySplitNotepad">
          <xsl:with-param name="pNotepad">
            <xsl:value-of select="substring-after($pNotepad,'&#x0A;')"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <fo:table-row>
          <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0.2cm" border="{$gVarBorder}">
            <fo:block border="{$gVarBorder}">
              <xsl:value-of select="$pNotepad"/>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- ==========================================================================================-->
  <!-- Display Additional Information (Notepad) -->
  <!-- GS 06012009 -->
  <!-- Update region: Delete extend fields / Now this region contains notepad texts if it exists -->
  <!--===========================================================================================-->
  <xsl:template name="displayAdditionalInformation">

    <xsl:variable name="varAdditionalInformation">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'AdditionalInformation'"/>
      </xsl:call-template>
    </xsl:variable>

    <fo:block keep-together.within-page="always">
      <fo:block border="{$gVarBorder}">

        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$varAdditionalInformation"/>
        </xsl:call-template>
      </fo:block>

      <xsl:if test ="normalize-space(//notepad)">
        <fo:block border="{$gVarBorder}" margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
          <fo:block border="{$gVarBorder}">
            <!-- EFS 15102009: call template displayNotepad -->
            <!-- display carriage return in notepad text box -->
            <xsl:call-template name ="displayNotepad"/>
            <xsl:call-template name="displayBlockBreakline"/>
          </fo:block>
        </fo:block>
      </xsl:if>
    </fo:block>
  </xsl:template>

  <!--=================================================================================================== -->
  <!-- Extend fields Region  -->
  <!-- GS 06012009: Add region          -->
  <!-- It contains extend fields if exists -->
  <!-- It handles several cultures if you set the displayname as follows "fr-FR:Titre;en-GB:Security;"-->
  <!--===================================================================================================-->

  <xsl:template name="displayExtendFields">

    <xsl:if test ="normalize-space(//dataDocument/trade/tradeExtends)">

      <xsl:for-each select="//dataDocument/repository/extend">
        <xsl:sort select="./identifier"/>

        <xsl:variable name="varExtendFieldName">
          <xsl:value-of select="./displayname"/>
        </xsl:variable>

        <fo:block keep-together.within-page="always">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="displayTitle">
              <xsl:with-param name="pText" select="$varExtendFieldName"/>
            </xsl:call-template>
          </fo:block>

          <fo:block margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
            <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
              <xsl:call-template name="createColumns"/>
              <fo:table-body>
                <xsl:for-each select="./extendDet">
                  <xsl:sort select="./identifier"/>
                  <xsl:variable name="varIdExtendDet" select="./@id"/>
                  <fo:table-row>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0.2cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                        <xsl:choose>
                          <xsl:when test="contains( ./displayname, concat( $pCurrentCulture, ':'))">

                            <xsl:variable name="vExtendDetDisplayName">
                              <xsl:value-of select="substring-before( substring-after( ./displayname, concat( $pCurrentCulture, ':') ), ';')"/>
                            </xsl:variable>

                            <xsl:choose>
                              <!-- !!!!!!! only two resources are handled in this section !!!!!!!-->
                              <!-- to do: handle three or more resources -->

                              <!-- GS 20111208 -->
                              <!-- ******************************************************************** -->
                              <!-- Bug fixed: ExtendDet displayname value was not displayed for the last specific culture value -->
                              <!-- Cause: the last character of the last resorce is not a semicolon(;)  -->
                              <!-- ******************************************************************** -->

                              <!-- this condiction is true in the last culture (eg. en-GB)    -->
                              <!-- <displayname>fr-FR: SSI du prêteur;en-GB:Lender's SSI</displayname>  -->
                              <xsl:when test ="string-length($vExtendDetDisplayName = 0)">
                                <xsl:value-of select=" substring-after( ./displayname, concat( $pCurrentCulture, ':') )"/>
                              </xsl:when>
                              <xsl:otherwise>
                                <xsl:value-of select="$vExtendDetDisplayName"/>
                              </xsl:otherwise>
                            </xsl:choose>
                          </xsl:when>
                          <!-- no specific culture value -->
                          <xsl:otherwise>
                            <xsl:value-of select="./displayname"/>
                          </xsl:otherwise>
                        </xsl:choose>
                        <xsl:text>:</xsl:text>
                      </fo:block>
                    </fo:table-cell>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                        <xsl:choose>
                          <!--to do: if dataType='bool' we must use a check-box-->
                          <xsl:when test ="./dataType='date'">
                            <xsl:call-template name="format-date">
                              <xsl:with-param name="xsd-date-time" select="//dataDocument/trade/tradeExtends/tradeExtend[@href=$varIdExtendDet]"/>
                            </xsl:call-template>
                          </xsl:when>
                          <xsl:otherwise>
                            <xsl:value-of select="//dataDocument/trade/tradeExtends/tradeExtend[@href=$varIdExtendDet]"/>
                          </xsl:otherwise>
                        </xsl:choose>
                      </fo:block>
                    </fo:table-cell>
                  </fo:table-row>
                </xsl:for-each>
              </fo:table-body>
            </fo:table>
          </fo:block>
        </fo:block>
      </xsl:for-each>
    </xsl:if>
  </xsl:template>
  <!-- 
	===============================================
	===============================================
	== All products: END REGION     =============
	===============================================
	===============================================
	-->

  <!-- 
	===============================================
	===============================================
	== IRD products: BEGIN REGION     =============
	===============================================
	===============================================
	-->
  <!-- =========================================================================== -->
  <!-- Begin region Header Product Text                                            -->
  <!-- Used by all IRD products, it displays the text of the letter for IRD        -->
  <!-- =========================================================================== -->

  <!-- &#xA; linefeed-treatment="preserve" -->
  <xsl:template name="displayIrdConfirmationText">

    <xsl:variable name="varDatedAsOf">
      <xsl:if test="$isValidMasterAgreementDate">
        <xsl:call-template name="format-date">
          <xsl:with-param name="xsd-date-time" select="$masterAgreementDtSignature"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>

    <xsl:call-template name="displayBlockBreakline"/>
    <fo:block border="{$gVarBorder}" text-align="justify" margin-left="{$varConfirmationTextLeftMarginCM}cm" margin-top="{$varConfirmationTextTopMarginCM}cm" font-size="{$varConfirmationTextFontSize}">
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'IRDConfText1'"/>
        </xsl:call-template>
      </fo:block>
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'IRDConfText2'"/>
        </xsl:call-template>
      </fo:block>
      <xsl:call-template name="displayBlockBreakline"/>
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'IRDConfText3'"/>
        </xsl:call-template>
      </fo:block>
      <xsl:call-template name="displayBlockBreakline"/>
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'IRDConfText4'"/>
        </xsl:call-template>
        <xsl:value-of select="$varDatedAsOf"/>
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'IRDConfText5'"/>
        </xsl:call-template>
        <xsl:value-of select="$varParty_1_name" />
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'IRDConfText6'"/>
        </xsl:call-template>
        <xsl:value-of select="$varParty_2_name" />.
      </fo:block>
      <xsl:call-template name="displayBlockBreakline"/>
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'IRDConfText7'"/>
        </xsl:call-template>
      </fo:block>
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayBlockBreakline"/>
      </fo:block>
    </fo:block>
  </xsl:template>

  <!-- ================================================================================== -->
  <!-- BEGIN of Region Display Swaption Terms                                  			-->
  <!-- ================================================================================== -->
  <!-- Templates Display Swaption Terms -->
  <xsl:template name="displaySwaptionTerms">
    <xsl:param name="pStreams"/>
    <xsl:param name="pIsCrossCurrency"/>
    <xsl:param name="pIsNotionalDifferent"/>

    <xsl:variable name="varSwaptionTerms">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'SwaptionTerms'"/>
      </xsl:call-template>
    </xsl:variable>

    <fo:block keep-together.within-page="always">

      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$varSwaptionTerms"/>
        </xsl:call-template>
      </fo:block>

      <fo:block border="{$gVarBorder}" margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'TradeDate'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTradeDate"/>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'OptionStyle'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <!-- 
									this is an intentionally not translate section 
									-->
                  <xsl:if test="//dataDocument/trade/swaption/americanExercise">American</xsl:if>
                  <xsl:if test="//dataDocument/trade/swaption/bermudaExercise">Bermuda</xsl:if>
                  <xsl:if test="//dataDocument/trade/swaption/europeanExercise">European</xsl:if>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Seller'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getPartyNameAndID" >
                    <xsl:with-param name="pParty" select="//swaption/sellerPartyReference/@href" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Buyer'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getPartyNameAndID" >
                    <xsl:with-param name="pParty" select="//swaption/buyerPartyReference/@href" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <xsl:call-template name="displayPremium" >
              <xsl:with-param name="pPremium" select="//dataDocument/trade/swaption/premium"/>
            </xsl:call-template>
          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>
  </xsl:template>
  <!-- ================================================================================== -->
  <!-- END of Region Swaption Terms   USED BY IRD PRODUCTS: SWAPTION						-->
  <!-- ================================================================================== -->

  <!-- ================================================================================== -->
  <!-- BEGIN of Region Premium  USED BY IRD PRODUCTS: SWAPTION							-->
  <!-- ================================================================================== -->
  <!--Display Premium -->
  <xsl:template name="displayPremium">
    <xsl:param name="pPremium" />
    <xsl:call-template name="debugXsl">
      <xsl:with-param name="pCurrentNode" select="$pPremium"/>
    </xsl:call-template>
    <xsl:for-each select="$pPremium">
      <fo:table-row>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'Premium'"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="debugXsl">
              <xsl:with-param name="pCurrentNode" select="paymentAmount/currency"/>
            </xsl:call-template>
            <xsl:value-of select="paymentAmount/currency" />
            <xsl:value-of select="$gVarSpace"/>
            <xsl:value-of select="format-number(paymentAmount/amount, $amountPattern, $defaultCulture)" />
          </fo:block>
        </fo:table-cell>
      </fo:table-row>

      <fo:table-row>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'PremiumPaymentDate'"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="getPremiumPaymentDate"/>
          </fo:block>
        </fo:table-cell>
      </fo:table-row>
      <xsl:if test="paymentDate">
        <fo:table-row>
          <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
            <fo:block border="{$gVarBorder}">
              <xsl:call-template name="getTranslation">
                <xsl:with-param name="pResourceName" select="'BusinessDayConventionFor'"/>
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
            <fo:block border="{$gVarBorder}">
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
        <fo:table-row>
          <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
            <fo:block border="{$gVarBorder}">
              <xsl:call-template name="getTranslation">
                <xsl:with-param name="pResourceName" select="'PremiumPaymentDate'"/>
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
            <fo:block border="{$gVarBorder}">
              <xsl:call-template name="debugXsl">
                <xsl:with-param name="pCurrentNode" select="paymentDate/dateAdjustments/businessDayConvention"/>
              </xsl:call-template>
              <xsl:value-of select="paymentDate/dateAdjustments/businessDayConvention"/>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
        <!--
				when the Business Center is missing don't show this row
				-->
        <xsl:if test ="//paymentDate/dateAdjustments/businessCenters=true()">
          <fo:table-row>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'BusinessDaysForPayments'"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="getBCFullNames">
                  <xsl:with-param name="pBCs" select="//paymentDate/dateAdjustments/businessCenters"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </xsl:if>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>
  <!-- ================================================================================== -->
  <!-- END of Region Premium                                                              -->
  <!-- ================================================================================== -->

  <!-- ================================================================================== -->
  <!-- BEGIN of Region display Procedure Exercise  USED BY IRD PRODUCTS: SWAPTION			-->
  <!-- ================================================================================== -->

  <xsl:variable name="varProcedureForExercise">
    <xsl:call-template name="getTranslation">
      <xsl:with-param name="pResourceName" select="'ProcedureForExercise'"/>
    </xsl:call-template>
  </xsl:variable>


  <!-- display Procedure Exercise Template -->
  <xsl:template name="displayExerciseProcedure">
    <xsl:param name="pProduct" />
    <!-- define product variable-->
    <xsl:variable name="exercise">
      <xsl:if test="$pProduct/americanExercise">
        <xsl:value-of select="$pProduct/americanExercise"/>
      </xsl:if>
      <xsl:if test="$pProduct/europeanExercise">
        <xsl:value-of select="$pProduct/europeanExercise"/>
      </xsl:if>
      <xsl:if test="$pProduct/bermudaExercise">
        <xsl:value-of select="$pProduct/bermudaExercise"/>
      </xsl:if>
    </xsl:variable>

    <fo:block keep-together.within-page="always">

      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$varProcedureForExercise"/>
        </xsl:call-template>
      </fo:block>
      <fo:block margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>
            <!-- Procedure for Bermuda Exercise -->
            <xsl:if test="$pProduct/bermudaExercise">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'BermudaOptionExerciseDates'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getAdjustableOrRelativeDates" >
                      <xsl:with-param name="pDate" select="$pProduct/bermudaExercise/bermudaExerciseDates"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'EarliestExerciseTime'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:value-of select="//bermudaExercise/earliestExerciseTime/hourMinuteTime"/>
                    <xsl:value-of select="$gVarSpace"/>
                    <!-- commented section. Now the gotten data is the Full Name of Business Center -->
                    <xsl:call-template name="getFullNameBC">
                      <xsl:with-param name="pBC" select="//bermudaExercise/earliestExerciseTime/businessCenter"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>

              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'LatestExerciseTime'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:value-of select="//bermudaExercise/latestExerciseTime/hourMinuteTime"/>
                    <xsl:value-of select="$gVarSpace"/>
                    <!-- commented section. Now the gotten data is the Full Name of Business Center -->
                    <xsl:call-template name="getFullNameBC">
                      <xsl:with-param name="pBC" select="//bermudaExercise/latestExerciseTime/businessCenter"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>
            <!--Procedure for American Exercise: to testing with an american swaption xml  -->
            <xsl:if test="$pProduct/americanExercise">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'CommencementDate'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getAdjustableOrRelativeDateSequence" >
                      <xsl:with-param name="pDate" select="$pProduct/americanExercise/commencementDate"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'ExpirationDate'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getAdjustableOrRelativeDateSequence" >
                      <xsl:with-param name="pDate" select="$pProduct/americanExercise/expirationDate"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'ExpirationTime'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:value-of select="//americanExercise/expirationTime/hourMinuteTime"/>
                    <xsl:value-of select="$gVarSpace"/>
                    <!-- commented section. Now the gotten data is the Full Name of Business Center -->
                    <xsl:call-template name="getFullNameBC">
                      <xsl:with-param name="pBC" select="//americanExercise/expirationTime/businessCenter"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>
            <!--Procedure for European Exercise -->
            <xsl:if test="$pProduct/europeanExercise">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'ExpirationDate'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getAdjustableOrRelativeDateSequence" >
                      <xsl:with-param name="pDate" select="$pProduct/europeanExercise/expirationDate"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'ExpirationTime'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:value-of select="//europeanExercise/expirationTime/hourMinuteTime"/>
                    <xsl:value-of select="$gVarSpace"/>
                    <!-- commented section. Now the gotten data is the Full Name of Business Center -->
                    <xsl:call-template name="getFullNameBC">
                      <xsl:with-param name="pBC" select="//europeanExercise/expirationTime/businessCenter"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>
          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>
  </xsl:template>
  <!-- ================================================================================== -->
  <!-- END of Region Procedure Exercise    USED BY IRD PRODUCTS: SWAPTION					-->
  <!-- ================================================================================== -->

  <!-- ================================================================================== -->
  <!-- BEGIN of Region Settlement Terms  USED BY IRD PRODUCTS: SWAPTION					-->
  <!-- ================================================================================== -->
  <xsl:variable name="varSettlementTerms">
    <xsl:call-template name="getTranslation">
      <xsl:with-param name="pResourceName" select="'SettlementTerms'"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:template name="displaySettlementTerms">

    <fo:block keep-together.within-page="always">

      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$varSettlementTerms"/>
        </xsl:call-template>
      </fo:block>

      <fo:block margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>
            <xsl:choose>
              <xsl:when test="//cashSettlement/node()=true()">

                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'Settlement'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'Cash'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>

                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'CashSettlementValuationTime'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:value-of select="//cashSettlement/cashSettlementValuationTime/hourMinuteTime"/>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>

                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'CashSettlementValuationDate'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:value-of select="$getCashSettlementValuationDate"/>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>

                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'ValuationBusinessDays'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getFullNameBC">
                        <xsl:with-param name="pBC" select="//cashSettlement/cashSettlementValuationDate/businessCenters"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'CashSettlementPaymentDate'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:value-of select="$getCashSettlementAdjustableOrRelativeDates"/>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'BusinessDayConventionForCashSettlementPaymentDate'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:value-of select="$getCashSettlementPaymentDateBusinessDayConvention"/>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'CashSettlementMethod'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:value-of select="$getCashSettlementMethod"/>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'CashSettlementCurrency'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:value-of select="//cashSettlement/cashPriceMethod/cashSettlementCurrency"/>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'QuotationRate'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:value-of select="//cashSettlement/cashPriceMethod/quotationRateType"/>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:when>
              <xsl:when test="//cashSettlement/node()=false()">
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'Settlement'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'Physical'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:when>
              <xsl:otherwise>
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'Settlement'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'Error'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:otherwise>
            </xsl:choose>
          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>
  </xsl:template>
  <!-- ================================================================================== -->
  <!-- END of Region Settlement Terms  USED BY IRD PRODUCTS: SWAPTION						-->
  <!-- ================================================================================== -->

  <!-- =================================================================================================	-->
  <!-- BEGIN of Region Display FRA Terms  USED BY IRD PRODUCTS: FRA		-->
  <!-- =================================================================================================	-->
  <xsl:variable name ="varTerms">
    <xsl:call-template name="getTranslation">
      <xsl:with-param name="pResourceName" select="'Terms'"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:template name="displayFRATerms">
    <xsl:param name="pFRA"/>

    <fo:block keep-together.within-page="always">

      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$varTerms"/>
        </xsl:call-template>
      </fo:block>

      <fo:block margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'TradeDate'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="//dataDocument/trade/tradeHeader/tradeDate"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'NotionalAmount'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-money2">
                    <xsl:with-param name="currency" select="$pFRA/notional/currency"/>
                    <xsl:with-param name="amount" select="//dataDocument/trade/fra/notional/amount"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'EffectiveDate'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="$pFRA/adjustedEffectiveDate"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'TerminationDate'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="$pFRA/adjustedTerminationDate"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'FixedRatePayer'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getPartyNameAndID" >
                    <xsl:with-param name="pParty" select="$pFRA/buyerPartyReference/@href" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'FixedRate'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-fixed-rate">
                    <xsl:with-param name="fixed-rate" select="$pFRA/fixedRate"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'FloatingRatePayer'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getPartyNameAndID" >
                    <xsl:with-param name="pParty" select="$pFRA/sellerPartyReference/@href" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'PaymentDate'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="$pFRA/paymentDate/unadjustedDate"/>
                  </xsl:call-template>
                  <xsl:call-template name="getBusinessDayConvention" >
                    <xsl:with-param name="pBusinessDayConvention" select="$pFRA/paymentDate/dateAdjustments/businessDayConvention" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <!-- DR20090702 -->
            <!--<xsl:if test ="count( $pFRA/paymentDates/paymentDatesAdjustments/businessCenters/businessCenter ) &gt; 0"> -->
            <xsl:if test ="normalize-space($pFRA/paymentDate/dateAdjustments/businessCenters)">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'BusinessDaysForPayment'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getBCFullNames">
                      <xsl:with-param name="pBCs" select="$pFRA/paymentDate/dateAdjustments/businessCenters"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'FloatingRateOption'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getFRAFloatingRateIndex">
                    <xsl:with-param name="pCalculation" select="$pFRA"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <xsl:if test ="normalize-space(//dataDocument/repository/rateIndex/informationSource)">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'Source'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getFRAFloatingRateSource">
                      <xsl:with-param name="pCalculation" select="$pFRA"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'DesignatedMaturity'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getFrequency" >
                    <xsl:with-param name="pFrequency" select="$pFRA/indexTenor"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Spread'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'None'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'FloatingRateDayCountFraction'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="debugXsl">
                    <xsl:with-param name="pCurrentNode" select="$pFRA/dayCountFraction"/>
                  </xsl:call-template>
                  <xsl:value-of select="$pFRA/dayCountFraction"/>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'ResetDates'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="$pFRA/adjustedEffectiveDate"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'FRADiscounting'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="FRADiscounting">
                    <xsl:with-param name="pStream" select="$pFRA/fraDiscounting"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- DR20090702: I put this section within remarks because the business day convention for
						                 payment dates has been already displayed above near the payment date -->
            <!--
						<fo:table-row>
							<fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
								<fo:block border="{$gVarBorder}">
									<xsl:call-template name="getTranslation">
										<xsl:with-param name="pResourceName" select="'BusinessDayConvention'"/>
									</xsl:call-template>
								</fo:block>
							</fo:table-cell>
							<fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
								<fo:block border="{$gVarBorder}">
									<xsl:call-template name="debugXsl">
										<xsl:with-param name="pCurrentNode" select="$pFRA/paymentDate/dateAdjustments/businessDayConvention"/>
									</xsl:call-template>
									<xsl:value-of select="$pFRA/paymentDate/dateAdjustments/businessDayConvention"/>
								</fo:block>
							</fo:table-cell>
						</fo:table-row>
						-->
          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>
  </xsl:template>

  <xsl:variable name ="varNotionalAmount">
    <xsl:call-template name="getTranslation">
      <xsl:with-param name="pResourceName" select="'NotionalAmount'"/>
    </xsl:call-template>
  </xsl:variable>

  <!-- ================================================================================================================ -->
  <!-- Begin region: display product terms                                                                              -->
  <!-- ================================================================================================================ -->
  <xsl:template name="displayIrdProductTerms">
    <xsl:param name="pIsSwaption"/>
    <xsl:param name="pStreams"/>
    <xsl:param name="pIsCrossCurrency"/>
    <xsl:param name="pIsNotionalDifferent"/>
    <xsl:param name="pIsScheduledNominalDisplayInAnnex"/>
    <!-- GS 20110214: stub handle-->
    <xsl:param name="pIsTermDifferent"/>
    <xsl:param name="pIsStub"/>
    <xsl:param name="pIsAsynchronousStub"/>

    <xsl:variable name="varUnderlyingOrTermsTitle">
      <xsl:choose>
        <xsl:when test="$pIsSwaption=true()">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'UnderlyingSwap'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pIsSwaption=false()">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'Terms'"/>
          </xsl:call-template>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <fo:block keep-together.within-page="always">

      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$varUnderlyingOrTermsTitle"/>
        </xsl:call-template>
      </fo:block>

      <xsl:variable name ="varNotionalAmount">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'NotionalAmount'"/>
        </xsl:call-template>
      </xsl:variable>

      <!-- begin of the section for swaption straddle product -->
      <!-- Display the specific terms of the underlying swap if the transaction is a Swaption straddle-->
      <xsl:choose>
        <xsl:when test="//dataDocument/trade/swaption[swaptionStraddle and swaptionStraddle='true']">
          <fo:block border="{$gVarBorder}" margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
            <fo:block border="{$gVarBorder}">
              <xsl:call-template name="getTranslation">
                <xsl:with-param name="pResourceName" select="'TheParticularTermsOfUnderlyingSwap'"/>
              </xsl:call-template>
              <xsl:call-template name="displayBlockBreakline"/>
            </fo:block>
            <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
              <xsl:call-template name="createColumns"/>
              <fo:table-body>
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}" font-weight="{$gVarDisplayDateDateFontWieght}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'SpecificTermsForUnderlyingPayerSwap'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'FixedRatePayer'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'Buyer_'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'FloatingRatePayer'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'Seller_'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}" font-weight="{$gVarDisplayDateDateFontWieght}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'SpecificTermsForUnderlyingReceiverSwap'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'FixedRatePayer'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'Seller'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'FloatingRatePayer'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'Buyer'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>

                <xsl:call-template name="displayAmountSchedule">
                  <xsl:with-param name="pName"     select="$varNotionalAmount"/>
                  <xsl:with-param name="pIsScheduledNominalDisplayInAnnex"     select="$pIsScheduledNominalDisplayInAnnex"/>
                  <xsl:with-param name="pIsNotionalDifferent"     select="$pIsNotionalDifferent"/>
                  <xsl:with-param name="pCurrency" select="$pStreams[1]/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/currency"/>
                  <xsl:with-param name="pSchedule" select="$pStreams[1]/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule"/>
                </xsl:call-template>
                <!-- 
								================
								Trade date 
								================
								-->
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'TradeDate'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTradeDate"/>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
                <!-- 
								================
								Effective date 
								================
								-->
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'EffectiveDate'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getEffectiveDate">
                        <xsl:with-param name="pStreams" select="$pStreams"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
                <!-- 
								====================
								Termination date 
								====================
								-->
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'TerminationDate'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTerminationDate">
                        <xsl:with-param name="pStreams" select="$pStreams"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </fo:table-body>
            </fo:table>
          </fo:block>
        </xsl:when>
        <!-- end of the section for swaption straddle product -->


        <!-- Begin of the section for all Ird products -->
        <xsl:otherwise>
          <fo:block border="{$gVarBorder}" margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
            <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
              <xsl:call-template name="createColumns"/>
              <fo:table-body>
                <!-- 
								=============================================
								Only for Cap/Floor 
								display Buyer and Seller
								31/08/2009 new role:
								the buyer of a CAP is the receiver of capFloorStream
								the seller of a CAP is the payer of capFloorStream
								============================================
								-->
                <xsl:if test="//dataDocument/trade/capFloor/capFloorStream/node()=true()">
                  <fo:table-row>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                        <xsl:call-template name="getTranslation">
                          <xsl:with-param name="pResourceName" select="'Buyer'"/>
                        </xsl:call-template>
                      </fo:block>
                    </fo:table-cell>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                        <xsl:call-template name="getPartyNameAndID" >
                          <xsl:with-param name="pParty" select="//capFloorStream/receiverPartyReference/@href" />
                        </xsl:call-template>
                      </fo:block>
                    </fo:table-cell>
                  </fo:table-row>

                  <fo:table-row>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                        <xsl:call-template name="getTranslation">
                          <xsl:with-param name="pResourceName" select="'Seller'"/>
                        </xsl:call-template>
                      </fo:block>
                    </fo:table-cell>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                        <xsl:call-template name="getPartyNameAndID" >
                          <xsl:with-param name="pParty" select="//capFloorStream/payerPartyReference/@href" />
                        </xsl:call-template>
                      </fo:block>
                    </fo:table-cell>
                  </fo:table-row>
                </xsl:if>

                <!-- 
								========================
								Notional amounts 
								========================
								-->
                <xsl:if test="$pIsCrossCurrency=false() and $pIsNotionalDifferent != 'true' and $pStreams/calculationPeriodAmount/calculation">
                  <xsl:call-template name="displayAmountSchedule">
                    <xsl:with-param name="pName"     select="$varNotionalAmount"/>
                    <xsl:with-param name="pIsScheduledNominalDisplayInAnnex"     select="$pIsScheduledNominalDisplayInAnnex"/>
                    <xsl:with-param name="pIsNotionalDifferent"     select="$pIsNotionalDifferent"/>
                    <xsl:with-param name="pCurrency" select="$pStreams[1]/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/currency"/>
                    <xsl:with-param name="pSchedule" select="$pStreams[1]/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule"/>
                  </xsl:call-template>
                </xsl:if>
                <!-- 
								================
								Trade date 
								================
								-->
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'TradeDate'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTradeDate"/>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
                <!--  GS 20110214: stub handle -->
                <xsl:if test ="$pIsTermDifferent != 'true'">
                  <!-- Effective date -->
                  <fo:table-row>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                        <xsl:call-template name="getTranslation">
                          <xsl:with-param name="pResourceName" select="'EffectiveDate'"/>
                        </xsl:call-template>
                      </fo:block>
                    </fo:table-cell>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                        <xsl:call-template name="getEffectiveDate">
                          <xsl:with-param name="pStreams" select="$pStreams"/>
                        </xsl:call-template>
                      </fo:block>
                    </fo:table-cell>
                  </fo:table-row>
                  <!-- Termination date -->
                  <fo:table-row>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                        <xsl:call-template name="getTranslation">
                          <xsl:with-param name="pResourceName" select="'TerminationDate'"/>
                        </xsl:call-template>
                      </fo:block>
                    </fo:table-cell>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                        <xsl:call-template name="getTerminationDate">
                          <xsl:with-param name="pStreams" select="$pStreams"/>
                        </xsl:call-template>
                      </fo:block>
                    </fo:table-cell>
                  </fo:table-row>
                </xsl:if>
                <!--  **************************************************  -->
                <!--  GS 20110214: stub handle                            -->
                <!--  Stub dates are displayed in the Term section if they are synchronous for all streams  -->
                <!--  Stub dates are displayed within each stream if either they are asynchronous (not the same for each stream) or there is only one stub -->
                <!--  **************************************************  -->
                <xsl:if test="$pIsStub=true() and $pIsAsynchronousStub=false()">
                  <!--  First Regular Period Date  -->
                  <xsl:if test="$pStreams/calculationPeriodDates/firstRegularPeriodStartDate/node()=true()">
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'FirstRegularPeriodDate'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getFirstRegularPeriodDate">
                            <xsl:with-param name="pStreams" select="$pStreams"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                  </xsl:if>
                  <!-- Last Regular Period Date	-->
                  <xsl:if test="$pStreams/calculationPeriodDates/lastRegularPeriodEndDate/node()=true()">
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'LastRegularPeriodDate'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getLastRegularPeriodDate">
                            <xsl:with-param name="pStreams" select="$pStreams"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                  </xsl:if>
                </xsl:if>
                <!--  **************************************************  -->
              </fo:table-body>
            </fo:table>
          </fo:block>
        </xsl:otherwise>
      </xsl:choose>
    </fo:block>
  </xsl:template>

  <!-- Template display amount schedule  -->
  <xsl:template name="displayAmountSchedule">
    <xsl:param name="pName"/>
    <xsl:param name="pCurrency"/>
    <xsl:param name="pSchedule"/>
    <xsl:param name="pStreamNo"/>
    <xsl:param name="pIsNotionalDifferent"/>
    <xsl:param name="pIsScheduledNominalDisplayInAnnex"/>

    <!-- varScheduledNominalAnnexNumber: it returns the annex number -->
    <xsl:variable name ="varScheduledNominalAnnexNumber">
      <xsl:call-template name ="getScheduledNominalAnnexNumber">
        <xsl:with-param name="pIsNotionalDifferent" select="$pIsNotionalDifferent"/>
      </xsl:call-template>
    </xsl:variable>

    <!-- GS 20110214: zero coupon handle: add variable-->
    <!-- true if exists notional steps (amortizing/accreting/roller coaster) -->
    <xsl:variable name ="isNotionalSteps">
      <xsl:choose>
        <xsl:when test="$pSchedule/step/node()">
          <xsl:value-of select="true()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="false()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- 
    when notional amount is the same for all streams
    displayed the first stream notional amount in Term section
    it can be:
    with single amount: display the initial value of the first stream
    with steps (amortizing/accreting/rollercoaster): 
    - if available we use the XML information events (NOS/NOM) 
    - else we use XML informations (NotionalStep node)
    -->
    <!--
    when notional amount are different
    displayed each notional amount in specific section (Floating or Fixed Amounts)
    it can be:
    with single amount: display the initial value of the specific stream
    with steps (amortizing/accreting/rollercoaster): 
    - if available we use the XML information events (NOS/NOM) 
    - else we use XML informations (NotionalStep node)   
    -->
    <!--
		for all types of notional amount with steps (amortizing/accreting/rollercoaster)
		when required (IsScheduledNominalDisplayInAnnex = true) 
    the notional amount will be displayed in annex section 
    the reference "See Annex + [number of annex]" will be displayed into specific section (Term, Floating Amount or Fixed Amounts)    
		-->
    <!-- 
		To define streamNO we use the pStreamNo variable - it is defined in 'handleIrdStreams' 
		template and it contains the swapStream position (eg. 1/2/3 or 4)
		
		Les id des streams dans le document XML sont une chose.
		- Ils représentent un petit nom du stream. 
		  Idéalement c’est mieux s’il représente aussi sa « position », mais cela reste facultatif, d’ailleurs le nom peut-être composé 
		  uniquement de caractères (ex. : TOTO)
		  Ils « peuvent » le cas échéant être  référencer via un attribut href
      
		Les STREAMNO dans les tables SQL en sont une autre
		- Ils représentent eux la position « réelle » du stream dans le document. Il s’agit d’un « integer ».

		-->

    <!--******************************************** -->
    <!-- notional amount is the same for all streams -->
    <xsl:if test="$pIsNotionalDifferent != 'true'">

      <!--single amount: display the initial value of the first stream-->
      <xsl:if test="$isNotionalSteps != 'true'">
        <fo:table-row>
          <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
            <fo:block border="{$gVarBorder}">
              <xsl:value-of select="$pName"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
            <fo:block border="{$gVarBorder}" text-align="{$varSendByDataAlign}">
              <xsl:value-of select="$pCurrency" />
              <xsl:value-of select="$gVarSpace"/>
              <xsl:value-of select="format-number($pSchedule/initialValue, $amountPattern, $defaultCulture)" />
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
      </xsl:if>

      <!--steps (amortizing/accreting/rollercoaster)-->
      <xsl:if test="$isNotionalSteps = 'true'">

        <!--notional amount displayed in annex section-->
        <xsl:if test="$pIsScheduledNominalDisplayInAnnex = 'true' and $varScheduledNominalAnnexNumber &gt;= 1">
          <fo:table-row>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:value-of select="$pName"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'SeeAnnex_'"/>
                </xsl:call-template>
                <xsl:call-template name ="getLetterFromNumber">
                  <xsl:with-param name="pNumber" select="$varScheduledNominalAnnexNumber"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </xsl:if>

        <!--notional amount displayed in specific section (Term, Floating Amount or Fixed Amounts) -->
        <xsl:if test="$pIsScheduledNominalDisplayInAnnex != 'true' or $varScheduledNominalAnnexNumber &lt; 1">
          <xsl:choose>
            <!-- when exists we use the Amortizing events ('NOS/'NOM') -->
            <xsl:when test ="//Event[INSTRUMENTNO='1' and STREAMNO='1' and EVENTCODE='NOS' and EVENTTYPE='NOM']">
              <xsl:for-each select="//Event[INSTRUMENTNO='1'and STREAMNO='1' and EVENTCODE='NOS' and EVENTTYPE='NOM']/EventClass[EVENTCLASS='DAT']">
                <xsl:if test="position ()&gt; 1">
                  <fo:table-row>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                      </fo:block>
                    </fo:table-cell>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                        <xsl:call-template name="format-money2">
                          <xsl:with-param name="currency" select="../UNIT"/>
                          <xsl:with-param name="amount" select="../VALORISATION"/>
                        </xsl:call-template>
                        <xsl:call-template name="getTranslation">
                          <xsl:with-param name="pResourceName" select="'asFrom'"/>
                        </xsl:call-template>
                        <xsl:call-template name="format-date">
                          <xsl:with-param name="xsd-date-time" select="current()/DTEVENT"/>
                        </xsl:call-template>
                      </fo:block>
                    </fo:table-cell>
                  </fo:table-row>
                </xsl:if>
              </xsl:for-each>
            </xsl:when>
            <!--  else we use the steps -->
            <xsl:otherwise>
              <xsl:if test="$isStream1NotionalStepSchedule=true()">
                <xsl:for-each select="//swapStream[1]/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/step">
                  <fo:table-row>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                      </fo:block>
                    </fo:table-cell>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                        <xsl:call-template name="format-money2">
                          <xsl:with-param name="currency" select="$pCurrency"/>
                          <xsl:with-param name="amount" select="stepValue"/>
                        </xsl:call-template>
                        <xsl:call-template name="getTranslation">
                          <xsl:with-param name="pResourceName" select="'asFrom'"/>
                        </xsl:call-template>
                        <xsl:call-template name="format-date">
                          <xsl:with-param name="xsd-date-time" select="stepDate"/>
                        </xsl:call-template>
                      </fo:block>
                    </fo:table-cell>
                  </fo:table-row>
                </xsl:for-each>
              </xsl:if>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>

      </xsl:if>

    </xsl:if>

    <!--*********************************-->
    <!-- Notional amounts are differents -->
    <xsl:if test="$pIsNotionalDifferent = 'true'">

      <!--single amount: display the initial value for each specific stream -->
      <xsl:if test="$isNotionalSteps != 'true'">
        <fo:table-row>
          <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
            <fo:block border="{$gVarBorder}">
              <xsl:value-of select="$pName"/>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
            <fo:block border="{$gVarBorder}" text-align="{$varSendByDataAlign}">
              <xsl:value-of select="$pCurrency" />
              <xsl:value-of select="$gVarSpace"/>
              <xsl:value-of select="format-number($pSchedule/initialValue, $amountPattern, $defaultCulture)" />
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
      </xsl:if>

      <!--steps (amortizing/accreting/rollercoaster)-->
      <xsl:if test="$isNotionalSteps = 'true'">

        <!--notional amount displayed in annex section-->
        <xsl:if test="$pIsScheduledNominalDisplayInAnnex = 'true' and $varScheduledNominalAnnexNumber &gt;= 1">
          <fo:table-row>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:value-of select="$pName"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'SeeAnnex_'"/>
                </xsl:call-template>
                <xsl:call-template name ="getLetterFromNumber">
                  <xsl:with-param name="pNumber" select="$varScheduledNominalAnnexNumber"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </xsl:if>

        <!--notional amount displayed in specific section (Term, Floating Amount or Fixed Amounts) -->
        <xsl:if test="$pIsScheduledNominalDisplayInAnnex != 'true' or $varScheduledNominalAnnexNumber &lt; 1">
          <xsl:choose>
            <!-- when exists we use the Amortizing events ('NOS/'NOM') -->
            <xsl:when test ="//Event[INSTRUMENTNO='1' and STREAMNO=$pStreamNo and EVENTCODE='NOS' and EVENTTYPE='NOM']">
              <xsl:for-each select="//Event[INSTRUMENTNO='1'and STREAMNO=$pStreamNo and EVENTCODE='NOS' and EVENTTYPE='NOM']/EventClass[EVENTCLASS='DAT']">
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:if test="position () = 1">
                        <xsl:value-of select="$pName"/>
                      </xsl:if>
                      <xsl:if test="position ()&gt; 1"/>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="format-money2">
                        <xsl:with-param name="currency" select="../UNIT"/>
                        <xsl:with-param name="amount" select="../VALORISATION"/>
                      </xsl:call-template>
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'asFrom'"/>
                      </xsl:call-template>
                      <xsl:call-template name="format-date">
                        <xsl:with-param name="xsd-date-time" select="current()/DTEVENT"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:for-each>
            </xsl:when>
            <!--  else we use the steps -->
            <xsl:otherwise>
              <xsl:if test="$isStream1NotionalStepSchedule = true()">
                <xsl:for-each select="//swapStream[$pStreamNo]/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/step">
                  <fo:table-row>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                      </fo:block>
                    </fo:table-cell>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                        <xsl:call-template name="format-money2">
                          <xsl:with-param name="currency" select="$pCurrency"/>
                          <xsl:with-param name="amount" select="stepValue"/>
                        </xsl:call-template>
                        <xsl:call-template name="getTranslation">
                          <xsl:with-param name="pResourceName" select="'asFrom'"/>
                        </xsl:call-template>
                        <xsl:call-template name="format-date">
                          <xsl:with-param name="xsd-date-time" select="stepDate"/>
                        </xsl:call-template>
                      </fo:block>
                    </fo:table-cell>
                  </fo:table-row>
                </xsl:for-each>
              </xsl:if>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>

      </xsl:if>

    </xsl:if>

  </xsl:template>

  <!-- returns Notional steps unit, valorization and date from events NOS/NOM -->
  <xsl:template name="getIntermediaryNominal">
    <xsl:param name="pStreamNo"/>
    <xsl:param name="pName"/>
    <xsl:param name="pCurrency"/>

    <xsl:for-each select="//Event[INSTRUMENTNO='1'and STREAMNO=$pStreamNo and EVENTCODE='NOS' and EVENTTYPE='NOM']/EventClass[EVENTCLASS='DAT']">
      <!--
			if you want display the "Initial Notional Amount" add in comment this condition(<xsl:if test="position ()&gt; 1">)  
			TO DO: add a parameter 'isAmortizingSwap'. to display the first node only for amortizing swap
			-->

      <xsl:if test="position ()&gt; 1">
        <fo:table-row>
          <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
            <fo:block border="{$gVarBorder}">
              <!--
						<xsl:if test="position () = 1">
							<xsl:value-of select="$pName"/>
						</xsl:if>
						<xsl:if test="position ()&gt; 1">
						</xsl:if>
						-->
            </fo:block>
          </fo:table-cell>
          <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
            <fo:block border="{$gVarBorder}">
              <xsl:call-template name="format-money2">
                <xsl:with-param name="currency" select="../UNIT"/>
                <xsl:with-param name="amount" select="../VALORISATION"/>
              </xsl:call-template>
              <xsl:call-template name="getTranslation">
                <xsl:with-param name="pResourceName" select="'asFrom'"/>
              </xsl:call-template>
              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="current()/DTEVENT"/>
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <!-- Display RateSchedule Template  -->
  <xsl:template name="displayRateSchedule">
    <xsl:param name="pName"/>
    <xsl:param name="pSchedule"/>

    <xsl:choose>
      <!--
			Add Rate schedule for Corridor: 
			It displays cap rate 1 and cap rate 2
			-->
      <xsl:when test ="$varCapFloorType = 'Corridor'">
        <xsl:if test ="position() = 1">
          <fo:table-row>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:value-of select="$pName"/>
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'_Rate1'"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}" text-align="{$varSendByDataAlign}">
                <xsl:call-template name="format-fixed-rate">
                  <xsl:with-param name="fixed-rate" select="$pSchedule[1]/initialValue" />
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </xsl:if>
        <xsl:if test ="position() = 2">
          <fo:table-row>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:value-of select="$pName"/>
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'_Rate2'"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}" text-align="{$varSendByDataAlign}">
                <xsl:call-template name="format-fixed-rate">
                  <xsl:with-param name="fixed-rate" select="$pSchedule[2]/initialValue" />
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </xsl:if>
      </xsl:when>
      <xsl:otherwise>
        <fo:table-row>
          <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
            <fo:block border="{$gVarBorder}">
              <xsl:value-of select="$pName"/>

              <xsl:call-template name="getTranslation">
                <xsl:with-param name="pResourceName" select="'_Rate'"/>
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
            <fo:block border="{$gVarBorder}" text-align="{$varSendByDataAlign}">
              <xsl:call-template name="format-fixed-rate">
                <xsl:with-param name="fixed-rate" select="$pSchedule/initialValue" />
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
      </xsl:otherwise>
    </xsl:choose>

    <xsl:if test="$pSchedule/step/node()=true()">
      <xsl:for-each select="$pSchedule/step">
        <fo:table-row>
          <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
            <fo:block border="{$gVarBorder}">
            </fo:block>
          </fo:table-cell>
          <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
            <fo:block border="{$gVarBorder}">
              <xsl:call-template name="format-fixed-rate">
                <xsl:with-param name="fixed-rate" select="stepValue" />
              </xsl:call-template>

              <xsl:call-template name="getTranslation">
                <xsl:with-param name="pResourceName" select="'asFrom'"/>
              </xsl:call-template>

              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="stepDate"/>
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
      </xsl:for-each>
    </xsl:if>

  </xsl:template>

  <!-- ================================================================================================================ -->
  <!-- Begin region: display floating calculation                                                                       -->
  <!-- Floating rate option, designated maturity, spread, floating rate day count fraction                              -->
  <!-- ================================================================================================================ -->
  <xsl:template name="displayFloatingCalculation">
    <xsl:param name="pCalculation"/>
    <fo:table-row>
      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
        <fo:block border="{$gVarBorder}">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'FloatingRateOption'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
        <fo:block border="{$gVarBorder}">
          <xsl:call-template name="getFloatingRateIndex">
            <xsl:with-param name="pCalculation" select="$pCalculation"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
    </fo:table-row>
    <xsl:if test ="normalize-space(//dataDocument/repository/rateIndex/informationSource)">
      <fo:table-row>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'Source'"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="getFloatingRateSource">
              <xsl:with-param name="pCalculation" select="$pCalculation"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
      </fo:table-row>
    </xsl:if>
    <!-- 
		==================================================================================
		Designated Maturity
		When getFrequency template returns "not applicable" this row is not displayed
		es. for the instrument OIS-Swap this information is not applicable
		==================================================================================
		-->
    <xsl:variable name ="varGetFrequency">
      <xsl:call-template name="getFrequency" >
        <xsl:with-param name="pFrequency" select="$pCalculation/floatingRateCalculation/indexTenor"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:if test ="$varGetFrequency != 'not applicable'">
      <!--
			Designate Maturity is not displayed when the index tenor is missing
			-->
      <xsl:if test ="$pCalculation/floatingRateCalculation/indexTenor/node()=true()">
        <fo:table-row>
          <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
            <fo:block border="{$gVarBorder}">
              <xsl:call-template name="getTranslation">
                <xsl:with-param name="pResourceName" select="'DesignatedMaturity'"/>
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
          <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
            <fo:block border="{$gVarBorder}">
              <xsl:call-template name="getFrequency" >
                <xsl:with-param name="pFrequency" select="$pCalculation/floatingRateCalculation/indexTenor"/>
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
      </xsl:if>
    </xsl:if>

    <!-- 
		==========
		Spread 
		==========
		-->
    <xsl:call-template name="displaySpread">
      <xsl:with-param name="pSpread" select="$pCalculation/floatingRateCalculation/spreadSchedule"/>
    </xsl:call-template>
    <!-- 
		==================================
		Floating Rate Day Count Fraction 
		==================================
		-->
    <fo:table-row>
      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
        <fo:block border="{$gVarBorder}">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'FloatingRateDayCountFraction'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm"  margin-left="0cm" border="{$gVarBorder}">
        <fo:block border="{$gVarBorder}">
          <xsl:value-of select="$pCalculation/dayCountFraction"/>
        </fo:block>
      </fo:table-cell>
    </fo:table-row>
  </xsl:template>


  <!-- 
	==================================
	Display Spread template
	==================================
	-->
  <xsl:template name="displaySpread">
    <xsl:param name="pSpread" />

    <xsl:if test="$pSpread=true()">
      <xsl:variable name="m_spread">
        <xsl:call-template name="format-fixed-rate">
          <xsl:with-param name="fixed-rate" select="$pSpread/initialValue" />
        </xsl:call-template>
      </xsl:variable>


      <fo:table-row>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'Spread'"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="ReplacePlusMinus">
              <xsl:with-param name="pString" select="$m_spread"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
      </fo:table-row>

      <xsl:if test="$pSpread/step/node()=true()">
        <xsl:for-each select="$pSpread/step">
          <fo:table-row>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
              </fo:block>
            </fo:table-cell>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="getSpread"/>
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'asFrom'"/>
                </xsl:call-template>
                <xsl:call-template name="format-date">
                  <xsl:with-param name="xsd-date-time" select="stepDate"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </xsl:for-each>
      </xsl:if>
    </xsl:if>

    <xsl:if test="$pSpread=false()">
      <xsl:value-of select="'None'"/>
    </xsl:if>
  </xsl:template>
  <!-- ================================================================================================================ -->
  <!-- End region: display floating calculation                                                                         -->
  <!-- ================================================================================================================ -->


  <!-- ======================================================================-->
  <!-- BEGIN REGION: template display floating rate calculation              -->
  <!-- ======================================================================-->
  <xsl:template name="displayFloatingRateCalculation">
    <xsl:param name="pStream"/>
    <xsl:param name="pStreamPosition"/>
    <xsl:param name="pIsCrossCurrency" select="false()"/>
    <xsl:param name="pIsNotionalDifferent"/>
    <xsl:param name="pIsScheduledNominalDisplayInAnnex"/>
    <xsl:param name="pIsCapOnly" select="false()"/>
    <xsl:param name="pIsFloorOnly" select="false()"/>
    <xsl:param name="pIsDisplayTitle" />
    <xsl:param name="pIsDisplayDetail" />
    <!-- GS 20110214: stub handle-->
    <xsl:param name="pIsTermDifferent"/>
    <xsl:param name="pIsStub"/>
    <xsl:param name="pIsAsynchronousStub"/>

    <xsl:variable name="varFloatingAmounts">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'FloatingAmounts'"/>
      </xsl:call-template>
    </xsl:variable>
    <fo:block keep-together.within-page="auto">

      <!-- Display title Floating amounts -->
      <xsl:if test="$pIsDisplayTitle=true()">
        <fo:block border="{$gVarBorder}">
          <xsl:call-template name="displayTitle">
            <xsl:with-param name="pText" select="$varFloatingAmounts"/>
          </xsl:call-template>
        </fo:block>
      </xsl:if>

      <fo:block border="{$gVarBorder}" margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>
            <!--
						Add the condition: show the payer when the swaption straddle node is false
						-->
            <xsl:choose>
              <xsl:when test ="//dataDocument/trade/swaption[swaptionStraddle and swaptionStraddle='true']"/>
              <xsl:when test ="//dataDocument/trade/capFloor/capFloorStream/node()=true()">
                <xsl:choose>
                  <xsl:when test ="$varCapFloorType = 'Collar'">
                    <!-- 
										The Buyer of a CapFloor Collar = Buy a Cap and sell a Floor
										Receive capRate and Pay floorRate
										- The FloatingCapRatePayer is the payer of CapFloorStream (the seller of the trade [Vendeur])
										- The FloatingFloorRatePayer is the receiver of CapFloorStream (the buyer of the trade [Acheteur])
										-->
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'FloatingCapRatePayer'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getPartyNameAndID" >
                            <xsl:with-param name="pParty" select="//capFloorStream/payerPartyReference/@href" />
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>

                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'FloatingFloorRatePayer'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getPartyNameAndID" >
                            <xsl:with-param name="pParty" select="//capFloorStream/receiverPartyReference/@href" />
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                  </xsl:when>
                  <!-- 
									The Buyer of a CapFloor straddle/strangle = Buy a Cap and Buy a Floor
									Receive capRate and Receive floorRate
									- The FloatingCapRatePayer is the payer of CapFloorStream (the seller of the trade [Vendeur])
									- The FloatingFloorRatePayer is the payer of CapFloorStream (the seller of the trade [Vendeur])
									-->
                  <!-- Todo: For Straddle just one item "FloatingRatePayer" -->
                  <xsl:when test ="$varCapFloorType = 'Straddle' or $varCapFloorType = 'Strangle'">
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'FloatingCapRatePayer'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getPartyNameAndID" >
                            <xsl:with-param name="pParty" select="//capFloorStream/payerPartyReference/@href" />
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>

                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'FloatingFloorRatePayer'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getPartyNameAndID" >
                            <xsl:with-param name="pParty" select="//capFloorStream/payerPartyReference/@href" />
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                  </xsl:when>
                  <!-- 
									The Buyer of a CapFloor Corridor = Buy a Cap1 and Sell a Cap2
									Receive capRate1 and Pay capRate2
									- The FloatingCapRatePayer1 is the payer of CapFloorStream (the seller of the trade [Vendeur])
									- The FloatingCapRatePayer is the receiver of CapFloorStream (the buyer of the trade [Acheteur])
									-->
                  <xsl:when test ="$varCapFloorType = 'Corridor'">
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'FloatingCapRate1Payer'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getPartyNameAndID" >
                            <xsl:with-param name="pParty" select="//capFloorStream/payerPartyReference/@href" />
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>

                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'FloatingCapRate2Payer'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getPartyNameAndID" >
                            <xsl:with-param name="pParty" select="//capFloorStream/receiverPartyReference/@href" />
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                  </xsl:when>
                  <!-- This section manage: Cap, Floor and all IRS products-->
                  <xsl:otherwise>
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'FloatingRatePayer'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="handleFloatingRatePayer">
                            <xsl:with-param name="pStream" select="$pStream"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                  </xsl:otherwise>
                </xsl:choose>

                <xsl:call-template name="handleCapOrFloorRate">
                  <xsl:with-param name="pStream" select="$pStream"/>
                </xsl:call-template>

              </xsl:when>
              <xsl:otherwise>
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'FloatingRatePayer'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="handleFloatingRatePayer">
                        <xsl:with-param name="pStream" select="$pStream"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:otherwise>
            </xsl:choose>

            <!-- Floating Rate Payer Currency Amount -->
            <xsl:choose>
              <!-- only for cross currency: display fixed rate payer currency amount -->
              <xsl:when test ="$pIsCrossCurrency=true()">
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'FloatingRatePayerCurrencyAmount'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getFloatingRatePayerCurrencyAmount">
                        <xsl:with-param name="pStream" select="$pStream"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:when>
              <!-- Notional amounts -->
              <xsl:otherwise>
                <xsl:if test="$pIsNotionalDifferent= 'true'">
                  <xsl:call-template name="displayAmountSchedule">
                    <xsl:with-param name="pName"     select="$varNotionalAmount"/>
                    <xsl:with-param name="pStreamNo"     select="$pStreamPosition"/>
                    <xsl:with-param name="pIsNotionalDifferent"     select="$pIsNotionalDifferent"/>
                    <xsl:with-param name="pIsScheduledNominalDisplayInAnnex"     select="$pIsScheduledNominalDisplayInAnnex"/>
                    <!-- GS 20110214: zero coupon handle  -->
                    <!--<xsl:with-param name="pCurrency" select="$pStream[$pStreamPosition]/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/currency"/>
										<xsl:with-param name="pSchedule" select="$pStream[$pStreamPosition]/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule"/>-->
                    <xsl:with-param name="pCurrency" select="$pStream/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/currency"/>
                    <xsl:with-param name="pSchedule" select="$pStream/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule"/>
                  </xsl:call-template>
                </xsl:if>
              </xsl:otherwise>
            </xsl:choose>

            <!-- GS 20110214: stub handle -->
            <!-- if the streams have calculation period date different -->
            <!-- it displays effective date and termination date in each stream -->
            <xsl:if test ="$pIsTermDifferent = 'true'">
              <!-- Effective date	-->
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'EffectiveDate'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getEffectiveDate">
                      <xsl:with-param name="pStreams" select="$pStream"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <!-- Termination date -->
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'TerminationDate'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTerminationDate">
                      <xsl:with-param name="pStreams" select="$pStream"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>
            <!--  **************************************************  -->
            <!-- EG 20220523 Corrections diverses liées à la saisie   -->
            <!-- suite querstionnaire OMIGRADE                        -->
            <!--  GS 20110214: stub handle                            -->
            <!--  Stub dates are displayed in the floating rate section if they are  asynchronous -->
            <!--  Stub dates are displayed in the Term rate section if they are synchronous -->
            <!--  **************************************************  -->
            <xsl:if test ="$pIsStub= true() and $pIsAsynchronousStub = true()">
              <!-- First Regular Period Date -->
              <xsl:if test="$pStream/calculationPeriodDates/firstRegularPeriodStartDate/node()=true()">
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'FirstRegularPeriodDate'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getFirstRegularPeriodDate">
                        <xsl:with-param name="pStreams" select="$pStream"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:if>
              <!-- Last Regular Period Date -->
              <xsl:if test="$pStream/calculationPeriodDates/lastRegularPeriodEndDate/node()=true()">
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'LastRegularPeriodDate'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getLastRegularPeriodDate">
                        <xsl:with-param name="pStreams" select="$pStream"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:if>
            </xsl:if>

            <!-- Floating rate option, designated maturity, spread, floating rate day count fraction -->
            <xsl:call-template name="displayFloatingCalculation" >
              <xsl:with-param name="pCalculation" select="$pStream/calculationPeriodAmount/calculation" />
            </xsl:call-template>

            <!--
						It displayed floating Stub Rate for Initial Calculation Period if exist
						When floating stub rate is determined it displays the stub rate else it display the text 'ToBeDetermined'
						This rule is valid only for floating stub rate
						-->
            <xsl:if test ="$pStream/calculationPeriodDates/firstRegularPeriodStartDate/node()=true()">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'FloatingRateForInitialCalculationPeriod'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:choose>
                      <xsl:when test="$pStream/stubCalculationPeriodAmount/initialStub/floatingRate/node()=true()">
                        <xsl:call-template name="getFloatingStubRate">
                          <xsl:with-param name="pStream" select="$pStream"/>
                          <xsl:with-param name="pTotalFloatingStubRate" select="count($pStream/stubCalculationPeriodAmount/initialStub/floatingRate)" />
                        </xsl:call-template>
                      </xsl:when>
                      <!-- GS 20110214: stub handle-->
                      <!-- a floating stub rate can be a fixed rate-->
                      <xsl:when test="$pStream/stubCalculationPeriodAmount/initialStub/stubRate/node()=true()">
                        <xsl:call-template name="getFixedStubRate">
                          <xsl:with-param name="pStream" select="$pStream"/>
                        </xsl:call-template>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:call-template name="getTranslation">
                          <xsl:with-param name="pResourceName" select="'SameLikeRegular'"/>
                        </xsl:call-template>
                      </xsl:otherwise>
                    </xsl:choose>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>
            <!--
						It displayed floating Stub Rate for Final Calculation Period if exist
						When floating stub rate is determined it displays the stub rate else it display the text 'ToBeDetermined'
						This rule is valid only for floating stub rate
						-->
            <xsl:if test ="$pStream/calculationPeriodDates/lastRegularPeriodEndDate/node()=true()">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'FloatingRateForFinalCalculationPeriod'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:choose>
                      <xsl:when test="$pStream/stubCalculationPeriodAmount/finalStub/floatingRate/node()=true()">
                        <xsl:call-template name="getFloatingStubRate">
                          <xsl:with-param name="pStream" select="$pStream"/>
                          <xsl:with-param name="pTotalFloatingStubRate" select="count($pStream/stubCalculationPeriodAmount/finalStub/floatingRate)" />
                        </xsl:call-template>
                      </xsl:when>
                      <!-- GS 20110214: stub handle-->
                      <!-- a floating stub rate can be a fixed rate-->
                      <xsl:when test="$pStream/stubCalculationPeriodAmount/finalStub/stubRate/node()=true()">
                        <xsl:call-template name="getFixedStubRate">
                          <xsl:with-param name="pStream" select="$pStream"/>
                        </xsl:call-template>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:call-template name="getTranslation">
                          <xsl:with-param name="pResourceName" select="'SameLikeRegular'"/>
                        </xsl:call-template>
                      </xsl:otherwise>
                    </xsl:choose>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>

            <!-- ================================================================================================================================================ -->
            <!-- BEGIN REGION: Detail of floating rate                                                                                                           -->
            <!-- when the condition is in "true" it Display all the informations (Rate Payer/Rate Option/Spread/Day Count/Payment Dates/Reset Dates/Compounding)  -->
            <!-- when the condition is in "false" it Display (Rate Payer/Rate Option/Maturity/Spread/Day Count/Compounding)-->
            <!-- this section add the detail about payment dates for swap-->
            <!-- ================================================================================================================================================= -->
            <xsl:if test="$pIsDisplayDetail=true()">

              <!--
							IsBusinessDayConventionDifferent: "true" if the Business Day Convention for Payment and Period Calculation are different
							-->
              <xsl:variable
									name ="IsBusinessDayConventionDifferent"
									select ="$pStream/calculationPeriodDates/calculationPeriodDatesAdjustments/businessDayConvention 
									  != $pStream/paymentDates/paymentDatesAdjustments/businessDayConvention">
              </xsl:variable>

              <!-- 
							If Delayed Payment or early Payment Applies (payment days Offset [OIS]): display the "Period End Dates"
							If not Delayed Payment or early Payment Applies: display the "Floating Rate Payment Dates Frequency:"
							-->
              <!--
							If the stream contains payment days offset or the Business day convention is different for calculation period and payment dates
							returns:
							- Period End Date +
								- If payment days offset is true returns payment convention (Offset + Calculation period BDC)
								- if payment days offset is false returns payment convention (realtive to [eg. calculation Period End Date] + Calculation period BDC)
							
							If the stream not contains payment days offset and  the Business day convention is egual
							returns:
							- the Payment Dates Frequency
							-->
              <xsl:if test="$pStream/paymentDates/paymentDaysOffset=true() or $IsBusinessDayConventionDifferent=true()">
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'PeriodEndDates'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getFloatingRateCalculationPeriodDatesFrequency">
                        <xsl:with-param name="pStream" select="$pStream"/>
                        <xsl:with-param name="pStreamPosition" select="$pStreamPosition"/>
                      </xsl:call-template>
                      <xsl:call-template name="getBusinessDayConvention" >
                        <xsl:with-param name="pBusinessDayConvention" select="$pStream/calculationPeriodDates/calculationPeriodDatesAdjustments/businessDayConvention" />
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
                <xsl:choose>
                  <xsl:when test ="$pStream/paymentDates/paymentDaysOffset=true()">
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <!-- GS 20111129: replace PaymentConvention with PaymentLag label-->
                            <!--<xsl:with-param name="pResourceName" select="'PaymentConvention'"/>-->
                            <xsl:with-param name="pResourceName" select="'PaymentLag'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">

                          <xsl:call-template name="getPaymentDatesOffSet" >
                            <xsl:with-param name="pStream" select="$pStream/paymentDates"/>
                          </xsl:call-template>

                          <xsl:call-template name="getBusinessDayConvention" >
                            <xsl:with-param name="pBusinessDayConvention" select="$pStream/paymentDates/paymentDatesAdjustments/businessDayConvention" />
                          </xsl:call-template>

                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                  </xsl:when>
                  <xsl:when test ="$pStream/paymentDates/paymentDaysOffset=false()">
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'PaymentConvention'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'RelativeTo'"/>
                          </xsl:call-template>
                          <xsl:call-template name="GetPaymentRelativeTo">
                            <xsl:with-param name="pStream" select="$pStream/paymentDates"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                  </xsl:when>
                </xsl:choose>
              </xsl:if>

              <xsl:if test="$pStream/paymentDates/paymentDaysOffset=false() and $IsBusinessDayConventionDifferent=false()">
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'FloatingRatePaymentDatesFrequency'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getFloatingRatePaymentDatesFrequency">
                        <xsl:with-param name="pStream" select="$pStream"/>
                        <xsl:with-param name="pStreamPosition" select="$pStreamPosition"/>
                      </xsl:call-template>
                      <xsl:call-template name="getBusinessDayConvention" >
                        <xsl:with-param name="pBusinessDayConvention" select="$pStream/paymentDates/paymentDatesAdjustments/businessDayConvention" />
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:if>

              <!--
							Display the list of the payment dates for the floating stream when Confirmation type is 'Full'
							-->
              <xsl:choose>
                <xsl:when test ="$varConfirmationType = 'Full'">
                  <xsl:if test="//Event[STREAMNO=$pStreamPosition and EVENTCODE='TER' and EVENTTYPE='INT']/EventClass[EVENTCLASS = 'STL']">
                    <xsl:call-template name="displayPaymentDatesList">
                      <xsl:with-param name="pStreamNo" select="$pStreamPosition"/>
                      <xsl:with-param name="pStream" select="$pStream"/>
                    </xsl:call-template>
                  </xsl:if>
                </xsl:when>
              </xsl:choose>
              <!-- 
							if confirmation Type is 'Full': display Business Day for 'Fixing' and Business Day for 'Payment'
							if confirmation Type is 'ISDA': display Business Days for Floating Amounts
							-->
              <xsl:choose>
                <xsl:when test ="$varConfirmationType = 'Full'">
                  <xsl:if test ="normalize-space($pStream/paymentDates/paymentDatesAdjustments/businessCenters)">
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}cm">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'BusinessDaysForPayment'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}cm">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getBCFullNames">
                            <xsl:with-param name="pBCs" select="$pStream/paymentDates/paymentDatesAdjustments/businessCenters"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                  </xsl:if>
                  <xsl:if test ="normalize-space($pStream/resetDates/fixingDates/businessCenters)">
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}cm">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'BusinessDaysForFixing'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}cm">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getBCFullNames">
                            <xsl:with-param name="pBCs" select="$pStream/resetDates/fixingDates/businessCenters"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                  </xsl:if>
                </xsl:when>
                <xsl:otherwise>
                  <!--
								display the business days for Floating amount
								-->
                  <xsl:if test ="normalize-space($pStream/paymentDates/paymentDatesAdjustments/businessCenters)">
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}cm">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'BusinessDaysforFloatingAmounts'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}cm">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getBCFullNames">
                            <xsl:with-param name="pBCs" select="$pStream/paymentDates/paymentDatesAdjustments/businessCenters"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                  </xsl:if>
                </xsl:otherwise>
              </xsl:choose>

              <!-- Display fixing dates only for cap/floor -->
              <xsl:for-each select="//dataDocument/trade/capFloor/capFloorStream[resetDates/node()]">
                <!-- 
							Set the CapFloor stream number variable 
							-->
                <xsl:variable name="StreamNo">
                  <xsl:value-of select="position()"/>
                </xsl:variable>
                <!-- 
								Display only if the fixing dates exist and Confirmation type is 'Full' 
								-->
                <xsl:choose>
                  <xsl:when test ="$varConfirmationType = 'Full'">
                    <xsl:if test="//Event[INSTRUMENTNO='1' and STREAMNO=$StreamNo and EVENTCODE='RES' and EVENTTYPE='FLO']/EventClass[EVENTCLASS='FXG']">
                      <xsl:call-template name="displayFixingDatesList">
                        <xsl:with-param name="pStreamNo" select="$StreamNo"/>
                      </xsl:call-template>
                    </xsl:if>
                  </xsl:when>
                </xsl:choose>
              </xsl:for-each>

              <!-- Fixing dates: -->
              <!-- GS 20110214: Bug fixed: handle a basis swap with EONIA floating rate index and EURIBOR floating rate index -->
              <xsl:variable name="varIsOvernightInterestSwap">
                <xsl:call-template name ="isOvernight">
                  <xsl:with-param name="pStream" select="$pStream"/>
                </xsl:call-template>
              </xsl:variable>
              <!-- DR20090701: Display the fixing date only if the swap is not an OIS -->
              <xsl:if test ="$varIsOvernightInterestSwap = 'false'">
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'FixingDates'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getFixingDates">
                        <xsl:with-param name="pStream" select="$pStream"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:if>

              <!-- 
							Begin region: fixing date for swap
							-->
              <!-- 
							Set the swap stream identifier 
							-->
              <xsl:variable name="swapStreamId">
                <xsl:value-of select="./@id"/>
              </xsl:variable>

              <!--
							Set the swap stream number (eg. 1 for swapstream1 - 2 for swapstream2) 
							-->
              <xsl:variable name="swapStreamNo">
                <xsl:value-of select="substring( $swapStreamId, 11, 2 )"/>
              </xsl:variable>

              <!-- 
							Display only if the fixing dates exist and Confirmation Type is Full
							-->
              <xsl:choose>
                <xsl:when test ="$varConfirmationType = 'Full'">
                  <!-- 
									the Fixing dates list is missing for EONIA OIS SWAP
									-->
                  <xsl:if test="$pStream/paymentDates/paymentDaysOffset=false()">
                    <xsl:if test="//Event[INSTRUMENTNO='1' and STREAMNO=$swapStreamNo and EVENTCODE='RES' and EVENTTYPE='FLO']/EventClass[EVENTCLASS='FXG']">
                      <xsl:call-template name="displayFixingDatesList">
                        <xsl:with-param name="pStreamNo" select="$swapStreamNo"/>
                      </xsl:call-template>
                    </xsl:if>
                  </xsl:if>
                </xsl:when>
              </xsl:choose>

              <!-- Begin region: reset dates  -->

              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'ResetDates'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getResetDatesType">
                      <xsl:with-param name="pStream" select="$pStream"/>
                    </xsl:call-template>
                    <xsl:call-template name="getBusinessDayConvention" >
                      <xsl:with-param name="pBusinessDayConvention" select="$pStream/resetDates/resetDatesAdjustments/businessDayConvention" />
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>

              <!-- Begin region: rate cut-off dates -->
              <xsl:if test="$pStream/resetDates/rateCutOffDaysOffset/node()">
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'RateCutOffDates'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getRateCutOffDaysOffset">
                        <xsl:with-param name="pStream" select="$pStream"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:if>
              <!-- End region: rate cut-off dates -->

              <!-- Begin region: averaging method -->
              <xsl:if test="$pStream/calculationPeriodAmount/calculation/floatingRateCalculation/averagingMethod/node()">
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'MethodOfAveraging'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getMethodOfAveraging">
                        <xsl:with-param name="pStream" select="$pStream"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:if>
              <!-- End region: averaging method -->
            </xsl:if>
            <!-- ==================================================-->
            <!-- END REGION: Detail of floating rate               -->
            <!-- ==================================================-->
            <!-- Begin region: compounding -->
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Compounding'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getCompounding">
                    <xsl:with-param name="pStream" select="$pStream"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <!-- End region: compounding -->
          </fo:table-body>
        </fo:table>
      </fo:block>
      <!-- END REGION: all products -->
    </fo:block>
  </xsl:template>
  <!-- ======================================================================-->
  <!-- END REGION: template display floating rate calculation                -->
  <!-- ======================================================================-->

  <!-- ================================================================================== -->
  <!-- BEGIN REGION: Exchange                                       -->
  <!-- ================================================================================== -->
  <xsl:template name="displayExchange">
    <xsl:param name="pExchange"/>

    <xsl:variable name="varExchangeTitle">
      <xsl:value-of select="$pExchange"/>
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'_Exchange'"/>
      </xsl:call-template>
    </xsl:variable>

    <fo:block keep-together.within-page="always">
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$varExchangeTitle"/>
        </xsl:call-template>
      </fo:block>

      <fo:block margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>
            <xsl:for-each select="//dataDocument/trade/swap/swapStream/principalExchanges[initialExchange='true']">
              <xsl:variable name="pPos" select="position()" />
              <xsl:variable name="curr" select="current()" />
              <xsl:variable name="Stream" select="//dataDocument/trade/swap/swapStream[$pPos]" />

              <xsl:if test="$pPos=1">
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:value-of select="$pExchange"/>
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'_ExchangeDate'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getExchangeDate">
                        <xsl:with-param name="pExchange" select="$pExchange" />
                        <xsl:with-param name="pStream" select="$Stream"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:if>

              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getPartyNameAndID" >
                      <xsl:with-param name="pParty" select="$Stream/payerPartyReference/@href" />
                    </xsl:call-template>
                    <xsl:text> </xsl:text>
                    <xsl:value-of select="$pExchange"/>
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'_ExchangeAmount'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getExchangeAmount">
                      <xsl:with-param name="pExchange" select="$pExchange" />
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:for-each>
          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>
  </xsl:template>

  <!-- ================================================================================================================ -->
  <!-- BEGIN REGION: template display payment dates list                                                                -->
  <!-- ================================================================================================================ -->
  <!-- Displays into a xsl:fo table the list of the payment dates of the fixed/floating rate stream (according to the "pStreamNo") -->
  <xsl:template name="displayPaymentDatesList">
    <xsl:param name="pStreamNo"/>
    <xsl:param name="pStream"/>
    <xsl:for-each select ="//Event[STREAMNO=$pStreamNo and EVENTCODE='INT' and EVENTTYPE='INT']/EventClass[EVENTCLASS = 'STL']">
      <fo:table-row>
        <fo:table-cell margin-left="0cm" border="{$gVarBorder}cm">
          <xsl:choose>
            <!--
						We write the text 'Payment Dates List:' on the left column only for the first row 
						If Delayed Payment or early Payment Applies (payment days Offset [OIS]): display the "Period End Dates List"
						If not Delayed Payment or early Payment Applies: display the "Floating Rate Payment Dates List:"
						-->
            <xsl:when test="position()=1">
              <fo:block border="{$gVarBorder}">
                <xsl:if test="$pStream/paymentDates/paymentDaysOffset=true()">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'PeriodEndDates'"/>
                  </xsl:call-template>
                </xsl:if>
                <xsl:if test="$pStream/paymentDates/paymentDaysOffset=false()">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'PaymentDates'"/>
                  </xsl:call-template>
                </xsl:if>
              </fo:block>
            </xsl:when>
            <xsl:otherwise>
              <fo:block border="{$gVarBorder}"></fo:block>
            </xsl:otherwise>
          </xsl:choose>
        </fo:table-cell>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="format-date">
              <xsl:with-param name="xsd-date-time" select="current()/DTEVENT"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
      </fo:table-row>
    </xsl:for-each>
    <xsl:for-each select ="//Event[STREAMNO=$pStreamNo and EVENTCODE='TER' and EVENTTYPE='INT']/EventClass[EVENTCLASS = 'STL']">
      <fo:table-row>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:if test="$pStream/paymentDates/paymentFrequency/period='T'">
              <xsl:call-template name="getTranslation">
                <xsl:with-param name="pResourceName" select="'PaymentDates'"/>
              </xsl:call-template>
            </xsl:if>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="format-date">
              <xsl:with-param name="xsd-date-time" select="current()/DTEVENT"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
      </fo:table-row>
    </xsl:for-each>
  </xsl:template>
  <!-- ================================================================================================================ -->
  <!-- END REGION: template display payment dates list                                                                  -->
  <!-- ================================================================================================================ -->

  <!-- ================================================================================================================ -->
  <!-- BEGIN REGION: template display payment dates list                                                                -->
  <!-- ================================================================================================================ -->
  <!-- Displays into a table the list of the payment dates of the fixed rate stream (according to the "pStreamNo") -->
  <xsl:template name="displayFixingDatesList">
    <xsl:param name="pStreamNo"/>

    <!-- Empty row 
		<xsl:call-template name="insertEmptyRow">
			<xsl:with-param name="pHeightCM" select="'0.4'"/>
		</xsl:call-template>
		-->

    <xsl:for-each select="//Event[INSTRUMENTNO='1' and STREAMNO=$pStreamNo and EVENTCODE='RES' and EVENTTYPE='FLO']/EventClass[EVENTCLASS='FXG']">
      <!-- Fixing dates: -->
      <fo:table-row>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <xsl:choose>
            <!-- We write the text 'Fixing Dates List:' on the left column only for the first row -->
            <xsl:when test="position()=1">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'FixingDates'"/>
                </xsl:call-template>
              </fo:block>
            </xsl:when>
            <xsl:otherwise>
              <fo:block border="{$gVarBorder}"></fo:block>
            </xsl:otherwise>
          </xsl:choose>
        </fo:table-cell>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="format-date">
              <xsl:with-param name="xsd-date-time" select="current()/DTEVENT"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
      </fo:table-row>
    </xsl:for-each>
  </xsl:template>
  <!-- ================================================================================================================ -->
  <!-- END REGION: template display payment dates list                                                                  -->
  <!-- ================================================================================================================ -->

  <!-- ================================================================================================================ -->
  <!-- BEGIN REGION: template display fixed rate payment dates list                                                     -->
  <!-- ================================================================================================================ -->
  <!-- Displays into a table the list of the payment dates for the fixed rate stream -->
  <xsl:template name="displayFixedRatePaymentDatesList">
    <xsl:param name="pStreamNo"/>
    <xsl:param name="pStream"/>
    <xsl:for-each select ="//Event[STREAMNO=$pStreamNo and EVENTCODE='INT' and EVENTTYPE='INT']/EventClass[EVENTCLASS = 'STL']">
      <fo:table-row>
        <fo:table-cell margin-left="0cm" border="{$gVarBorder}cm">
          <xsl:choose>
            <!-- We write the text 'Fixed Rate Payment Dates List:' on the left column only for the first row -->
            <xsl:when test="position()=1">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'FixedRatePaymentDates'"/>
                </xsl:call-template>
              </fo:block>
            </xsl:when>
            <xsl:otherwise>
              <fo:block border="{$gVarBorder}"></fo:block>
            </xsl:otherwise>
          </xsl:choose>
        </fo:table-cell>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="format-date">
              <xsl:with-param name="xsd-date-time" select="current()/DTEVENT"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
      </fo:table-row>
    </xsl:for-each>
    <xsl:for-each select ="//Event[STREAMNO=$pStreamNo and EVENTCODE='TER' and EVENTTYPE='INT']/EventClass[EVENTCLASS = 'STL']">
      <fo:table-row>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:if test="$pStream/paymentDates/paymentFrequency/period='T'">
              <xsl:call-template name="getTranslation">
                <xsl:with-param name="pResourceName" select="'PaymentDates'"/>
              </xsl:call-template>
            </xsl:if>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="format-date">
              <xsl:with-param name="xsd-date-time" select="current()/DTEVENT"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
      </fo:table-row>
    </xsl:for-each>
  </xsl:template>
  <!-- ================================================================================================================ -->
  <!-- END REGION: template display payment dates list                                                                  -->
  <!-- ================================================================================================================ -->

  <!-- ================================================================================== -->
  <!-- BEGIN REGION: template display discounting                                         -->
  <!-- ================================================================================== -->
  <xsl:template name="displayDiscounting">
    <xsl:param name="pStream"/>

    <xsl:variable name="varDiscounting">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'Discounting'"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:if test="$pStream/calculationPeriodAmount/calculation/discounting/node()">

      <fo:block keep-together.within-page="always">

        <fo:block border="{$gVarBorder}">
          <xsl:call-template name="displayTitle">
            <xsl:with-param name="pText" select="$varDiscounting"/>
          </xsl:call-template>
        </fo:block>

        <fo:block border="{$gVarBorder}" margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
          <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
            <xsl:call-template name="createColumns"/>
            <fo:table-body>
              <!-- 
							==============
							Discount rate
							==============
							-->
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'DiscountRate'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getDiscountRate">
                      <xsl:with-param name="pStream" select="$pStream"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <!-- 
							=================================
							Discount rate day count fraction
							=================================
							-->
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'DiscountRateDayCountFraction'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getDiscountRateDayCountFraction">
                      <xsl:with-param name="pStream" select="$pStream"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </fo:table-body>
          </fo:table>
        </fo:block>
      </fo:block>
    </xsl:if>
  </xsl:template>
  <!-- ================================================================================== -->
  <!-- BEGIN REGION: template display discounting                                         -->
  <!-- ================================================================================== -->

  <!-- ================================================================================== -->
  <!-- BEGIN REGION: template display fixed rate                                          -->
  <!-- ================================================================================== -->
  <xsl:template name="displayFixedRate">
    <xsl:param name="pName"/>
    <xsl:param name="pStream"/>
    <fo:table-row>
      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
        <fo:block border="{$gVarBorder}">
          <xsl:value-of select="$pName"/>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
        <fo:block border="{$gVarBorder}" text-align="{$varSendByDataAlign}">
          <xsl:call-template name="getFixedRate">
            <xsl:with-param name="pStream" select="$pStream"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
    </fo:table-row>

    <!-- if stream1 contains fixed rate steps schedule -->
    <xsl:if test="$varIsStream1FixedRateStepSchedule = true()">
      <xsl:for-each select="$varGetStream1FixedRateStepSchedule">
        <fo:table-row>
          <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
            <fo:block border="{$gVarBorder}"></fo:block>
          </fo:table-cell>
          <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
            <fo:block border="{$gVarBorder}">
              <xsl:call-template name="format-fixed-rate">
                <xsl:with-param name="fixed-rate" select="stepValue" />
              </xsl:call-template>
              <xsl:call-template name="getTranslation">
                <xsl:with-param name="pResourceName" select="'asFrom'"/>
              </xsl:call-template>
              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="stepDate"/>
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
      </xsl:for-each>
    </xsl:if>

    <!-- if stream2 contains fixed rate steps schedule -->
    <xsl:if test="$varIsStream2FixedRateStepSchedule = true()">
      <xsl:for-each select="$varGetStream2FixedRateStepSchedule">
        <fo:table-row>
          <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
            <fo:block border="{$gVarBorder}"></fo:block>
          </fo:table-cell>
          <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
            <fo:block border="{$gVarBorder}">
              <xsl:call-template name="format-fixed-rate">
                <xsl:with-param name="fixed-rate" select="stepValue" />
              </xsl:call-template>
              <xsl:call-template name="getTranslation">
                <xsl:with-param name="pResourceName" select="'asFrom'"/>
              </xsl:call-template>
              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="stepDate"/>
              </xsl:call-template>
            </fo:block>
          </fo:table-cell>
        </fo:table-row>
      </xsl:for-each>
    </xsl:if>
  </xsl:template>

  <!-- ================================================================================== -->
  <!-- BEGIN REGION: template display fixed rate schedule                                 -->
  <!-- ================================================================================== -->
  <xsl:template name="displayFixedRateSchedule">
    <xsl:param name="pStream"/>
    <xsl:param name="pStreamPosition"/>
    <xsl:param name="pIsCrossCurrency"/>
    <xsl:param name="pIsNotionalDifferent"/>
    <xsl:param name="pIsScheduledNominalDisplayInAnnex"/>
    <xsl:param name="pIsDisplayTitle" />
    <xsl:param name="pIsDisplayDetail" />
    <!-- GS 20110214: stub handle-->
    <xsl:param name="pIsTermDifferent"/>
    <xsl:param name="pIsStub"/>
    <xsl:param name="pIsAsynchronousStub"/>

    <xsl:variable name="varFixedAmounts">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'FixedAmounts'"/>
      </xsl:call-template>
    </xsl:variable>

    <fo:block keep-together.within-page="auto">

      <xsl:if test="$pIsDisplayTitle=true()">
        <fo:block border="{$gVarBorder}">
          <xsl:call-template name="displayTitle">
            <xsl:with-param name="pText" select="$varFixedAmounts"/>
          </xsl:call-template>
        </fo:block>
      </xsl:if>

      <fo:block border="{$gVarBorder}" margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>

            <xsl:choose>

              <xsl:when test ="//dataDocument/trade/capFloor/capFloorStream/node()=true()">

                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'FixedRatePayer'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">

                      <xsl:call-template name="getCapFloorPremiumRatePayer">
                        <xsl:with-param name="pPremium" select="//capFloor/premium[1]"/>
                      </xsl:call-template>

                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>

                <xsl:if test ="//premium/paymentQuote/percentageRate/node()=true()">
                  <fo:table-row>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                        <xsl:call-template name="getTranslation">
                          <xsl:with-param name="pResourceName" select="'FixedRate'"/>
                        </xsl:call-template>
                      </fo:block>
                    </fo:table-cell>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">

                        <xsl:call-template name="getCapFloorPremiumRate">
                          <xsl:with-param name="pPremium" select="//capFloor/premium[1]"/>
                        </xsl:call-template>

                      </fo:block>
                    </fo:table-cell>
                  </fo:table-row>
                </xsl:if>

                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'FixedAmount'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">

                      <xsl:call-template name="getCapFloorPremiumAmount">
                        <xsl:with-param name="pPremium" select="//capFloor/premium[1]"/>
                      </xsl:call-template>

                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>

                <!-- GS 20110328: handle cap/floor with more than one premium -->
                <!-- This version displayes: same payer, same amount and  differents dates -->
                <!-- To verified!: Can we have different amounts?-->

                <xsl:for-each select ="//capFloor/premium">

                  <xsl:variable name="curr" select="current()" />

                  <fo:table-row>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <xsl:if test ="position()=1">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'FixedRatePayerPaymentDate'"/>
                          </xsl:call-template>
                        </fo:block>
                      </xsl:if>
                      <xsl:if test ="position()&gt;1">
                        <fo:block border="{$gVarBorder}">
                        </fo:block>
                      </xsl:if>

                    </fo:table-cell>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">

                        <xsl:call-template name="getCapFloorPremiumPaymentDate">
                          <xsl:with-param name="pPremium" select="$curr"/>
                        </xsl:call-template>

                      </fo:block>
                    </fo:table-cell>
                  </fo:table-row>

                </xsl:for-each>

              </xsl:when>
              <xsl:otherwise>
                <!-- 
								include provisiones form of confirmation for the type of swap transaction 
								omitting fixed rate payer and floating rate payer.
								Add the condiction: show the payer when the swaption straddle node is false
								-->
                <xsl:choose>
                  <!-- Fixed rate payer -->
                  <xsl:when test ="//dataDocument/trade/swaption[swaptionStraddle and swaptionStraddle='true']"/>
                  <xsl:otherwise>
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'FixedRatePayer'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getPartyNameAndID" >
                            <xsl:with-param name="pParty" select="$pStream/payerPartyReference/@href" />
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                  </xsl:otherwise>
                </xsl:choose>

                <!-- Fixed rate payer currency amount -->
                <xsl:choose>
                  <!-- only for croos currency: display fixed rate payer currency amount -->
                  <xsl:when test ="$pIsCrossCurrency=true()">
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'FixedRatePayerCurrencyAmount'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getFixedRatePayerCurrencyAmount">
                            <xsl:with-param name="pStream" select="$pStream"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                  </xsl:when>
                  <xsl:otherwise>
                    <!-- GS 20110214: zero coupon handle-->
                    <xsl:choose>
                      <!-- when stream is zero coupon display the fixed amount -->
                      <xsl:when test ="$pStream/calculationPeriodAmount/knownAmountSchedule=true()">
                        <xsl:call-template name="handleFixedAmount">
                          <xsl:with-param name="pStream" select="$pStream"/>
                          <xsl:with-param name="pIsNotionalDifferent" select="$pIsNotionalDifferent"/>
                        </xsl:call-template>
                      </xsl:when>
                      <!-- for all other cases display notional amount -->
                      <xsl:otherwise>
                        <xsl:if test="$pIsNotionalDifferent = 'true'">
                          <xsl:call-template name="displayAmountSchedule">
                            <xsl:with-param name="pName"     select="$varNotionalAmount"/>
                            <xsl:with-param name="pStreamNo"     select="$pStreamPosition"/>
                            <xsl:with-param name="pIsNotionalDifferent"     select="$pIsNotionalDifferent"/>
                            <xsl:with-param name="pIsScheduledNominalDisplayInAnnex"     select="$pIsScheduledNominalDisplayInAnnex"/>
                            <!-- GS 20110214: zero coupon handle-->
                            <xsl:with-param name="pCurrency" select="$pStream/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/currency"/>
                            <xsl:with-param name="pSchedule" select="$pStream/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule"/>
                          </xsl:call-template>
                        </xsl:if>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:otherwise>
                </xsl:choose>
                <!-- GS 20110214: stub handle -->
                <!-- if the streams have calculation period date different -->
                <!-- it displays effective date and termination date in each stream -->
                <xsl:if test ="$pIsTermDifferent = 'true'">
                  <!-- Effective date -->
                  <fo:table-row>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                        <xsl:call-template name="getTranslation">
                          <xsl:with-param name="pResourceName" select="'EffectiveDate'"/>
                        </xsl:call-template>
                      </fo:block>
                    </fo:table-cell>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                        <xsl:call-template name="getEffectiveDate">
                          <xsl:with-param name="pStreams" select="$pStream"/>
                        </xsl:call-template>
                      </fo:block>
                    </fo:table-cell>
                  </fo:table-row>
                  <!-- Termination date -->
                  <fo:table-row>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                        <xsl:call-template name="getTranslation">
                          <xsl:with-param name="pResourceName" select="'TerminationDate'"/>
                        </xsl:call-template>
                      </fo:block>
                    </fo:table-cell>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                        <xsl:call-template name="getTerminationDate">
                          <xsl:with-param name="pStreams" select="$pStream"/>
                        </xsl:call-template>
                      </fo:block>
                    </fo:table-cell>
                  </fo:table-row>
                </xsl:if>
                <!--  **************************************************  -->
                <!--  GS 20110214: stub handle                          -->
                <!--  Stub dates are displayed in the floating rate section if they are  asynchronous -->
                <!--  Stub dates are displayed in the Term rate section if they are synchronous -->
                <!--  **************************************************  -->
                <xsl:if test="$pIsStub = true() and $pIsAsynchronousStub = true()">
                  <!-- First Regular Period Date -->
                  <xsl:if test="$pStream/calculationPeriodDates/firstRegularPeriodStartDate/node()=true()">
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'FirstRegularPeriodDate'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getFirstRegularPeriodDate">
                            <xsl:with-param name="pStreams" select="$pStream"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                  </xsl:if>
                  <!-- Last Regular Period Date -->
                  <xsl:if test="$pStream/calculationPeriodDates/lastRegularPeriodEndDate/node()=true()">
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'LastRegularPeriodDate'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getLastRegularPeriodDate">
                            <xsl:with-param name="pStreams" select="$pStream"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                  </xsl:if>
                </xsl:if>

                <!-- GS 20110214: zero coupon swap handle-->
                <!-- zero coupon swap: fixed rate and day count fraction is not displayed -->
                <xsl:if test="$pStream/calculationPeriodAmount/knownAmountSchedule=false()">
                  <!-- Fixed rate -->
                  <xsl:variable name="resFixedRate">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'FixedRate'"/>
                    </xsl:call-template>
                  </xsl:variable>
                  <xsl:call-template name="displayFixedRate">
                    <!--<xsl:with-param name="pName" select="'Fixed Rate:'"/>-->
                    <xsl:with-param name="pName" select="$resFixedRate"/>
                    <xsl:with-param name="pStream" select="$pStream"/>
                  </xsl:call-template>
                  <!-- Fixed rate day count fraction (ex. ACT/360) -->
                  <fo:table-row>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                        <xsl:call-template name="getTranslation">
                          <xsl:with-param name="pResourceName" select="'FixedRateDayCountFraction'"/>
                        </xsl:call-template>
                      </fo:block>
                    </fo:table-cell>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                        <xsl:value-of select="$pStream/calculationPeriodAmount/calculation/dayCountFraction"/>
                      </fo:block>
                    </fo:table-cell>
                  </fo:table-row>
                </xsl:if>

                <!--
								It displayed Fixed Stub Rate for Initial or Final Calculation Period
								When fixed stub rate is determined it displays the stub rate else it display nothing
								This rule is valid only for fixed stub rate
								-->
                <xsl:if test ="$pStream/calculationPeriodDates/firstRegularPeriodStartDate/node()=true() and $pStream/stubCalculationPeriodAmount/initialStub/node()=true()">
                  <fo:table-row>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                        <xsl:call-template name="getTranslation">
                          <xsl:with-param name="pResourceName" select="'FixedRateForInitialCalculationPeriod'"/>
                        </xsl:call-template>
                      </fo:block>
                    </fo:table-cell>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                        <xsl:call-template name="getFixedStubRate">
                          <xsl:with-param name="pStream" select="$pStream"/>
                        </xsl:call-template>
                      </fo:block>
                    </fo:table-cell>
                  </fo:table-row>
                </xsl:if>

                <xsl:if test ="$pStream/calculationPeriodDates/lastRegularPeriodEndDate/node()=true() and $pStream/stubCalculationPeriodAmount/finalStub/node()=true()">
                  <fo:table-row>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                        <xsl:call-template name="getTranslation">
                          <xsl:with-param name="pResourceName" select="'FixedRateForFinalCalculationPeriod'"/>
                        </xsl:call-template>
                      </fo:block>
                    </fo:table-cell>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                        <xsl:call-template name="getFixedStubRate">
                          <xsl:with-param name="pStream" select="$pStream"/>
                        </xsl:call-template>
                      </fo:block>
                    </fo:table-cell>
                  </fo:table-row>
                </xsl:if>

                <!-- =========================================================================================-->
                <!-- BEGIN REGION: detail of the fixing rate =================================================-->
                <!-- =========================================================================================-->
                <xsl:if test="$pIsDisplayDetail=true()">

                  <!-- 
									=======================================================================================================
									If Delayed Payment or early Payment Applies (payment days Offset [OIS]): display the "Period End Dates"
									If not Delayed Payment or early Payment Applies: display the "Fixed Rate Payment Dates Frequency"
									========================================================================================================
									-->
                  <xsl:if test="$pStream/paymentDates/paymentDaysOffset=true()">
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'PeriodEndDates'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">

                          <!--<xsl:call-template name="getPaymentDatesFrequencyWithOffset">
                            <xsl:with-param name="pStream" select="$pStream"/>
                            <xsl:with-param name ="pFrequency" select ="$pStream/paymentDates/paymentFrequency"/>
                          </xsl:call-template>-->

                          <xsl:call-template name="getFixedRateCalculationPeriodDatesFrequency">
                            <xsl:with-param name="pStream" select="$pStream"/>
                            <xsl:with-param name="pStreamPosition" select="$pStreamPosition"/>
                          </xsl:call-template>

                          <xsl:call-template name="getBusinessDayConvention" >
                            <xsl:with-param name="pBusinessDayConvention" select="$pStream/calculationPeriodDates/calculationPeriodDatesAdjustments/businessDayConvention" />
                          </xsl:call-template>

                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>

                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <!-- GS 20111129: replace PaymentConvention with PaymentLag label-->
                            <!--<xsl:with-param name="pResourceName" select="'PaymentConvention'"/>-->
                            <xsl:with-param name="pResourceName" select="'PaymentLag'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <!--<xsl:call-template name="getPaymentLag"/>-->

                          <xsl:call-template name="getPaymentDatesOffSet" >
                            <xsl:with-param name="pStream" select="$pStream/paymentDates"/>
                          </xsl:call-template>

                          <xsl:call-template name="getBusinessDayConvention" >
                            <xsl:with-param name="pBusinessDayConvention" select="$pStream/paymentDates/paymentDatesAdjustments/businessDayConvention" />
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                  </xsl:if>

                  <xsl:if test="$pStream/paymentDates/paymentDaysOffset=false()">
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'FixedRatePaymentDatesFrequency'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getFixedRatePaymentDatesFrequency">
                            <xsl:with-param name="pStream" select="$pStream"/>
                            <xsl:with-param name="pStreamPosition" select ="$pStreamPosition"/>
                          </xsl:call-template>
                          <xsl:call-template name="getBusinessDayConvention" >
                            <xsl:with-param name="pBusinessDayConvention" select="$pStream/paymentDates/paymentDatesAdjustments/businessDayConvention" />
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                  </xsl:if>

                  <!--
									Display the list of the payment dates for the fixed rate stream  when Confirmation type is Full
									-->
                  <xsl:choose>
                    <xsl:when test ="$varConfirmationType = 'Full'">
                      <xsl:if test="//Event[STREAMNO=$pStreamPosition and EVENTCODE='TER' and EVENTTYPE='INT']/EventClass[EVENTCLASS = 'STL']">
                        <xsl:call-template name="displayFixedRatePaymentDatesList">
                          <xsl:with-param name="pStreamNo" select="$pStreamPosition"/>
                          <xsl:with-param name="pStream" select="$pStream"/>
                        </xsl:call-template>
                      </xsl:if>
                    </xsl:when>
                  </xsl:choose>

                  <!--
									display the business days for fixed amount 
									-->
                  <xsl:if test ="normalize-space($pStream/paymentDates/paymentDatesAdjustments/businessCenters)">
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}cm">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'BusinessDaysforFixedAmounts'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}cm">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getBCFullNames">
                            <xsl:with-param name="pBCs" select="$pStream/paymentDates/paymentDatesAdjustments/businessCenters"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>
                  </xsl:if>



                </xsl:if>
                <!-- =========================================================================================-->
                <!-- END REGION: detail of the fixing rate ===================================================-->
                <!-- =========================================================================================-->
              </xsl:otherwise>
            </xsl:choose>
          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>
  </xsl:template>
  <!-- ================================================================================== -->
  <!-- END REGION: template display fixed rate schedule                                   -->
  <!-- ================================================================================== -->


  <!--
	================================================================================
	== Provisions BEGIN REGION
	=================================================================================
	-->

  <!-- varEarlyTerminationProvision variable -->
  <xsl:variable name="varEarlyTerminationProvision">
    <xsl:call-template name="getTranslation">
      <xsl:with-param name="pResourceName" select="'EarlyTermination'"/>
    </xsl:call-template>
  </xsl:variable>

  <!-- displayOptionalEarlyTermination template -->
  <xsl:template name="displayOptionalEarlyTermination">
    <xsl:param name="pInstrument"/>

    <fo:block keep-together.within-page="always">
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$varEarlyTerminationProvision"/>
        </xsl:call-template>
      </fo:block>
      <fo:block margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'OptionalEarlyTermination'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Applicable'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'OptionStyle'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getProvisionOptionStyle">
                    <xsl:with-param name="pPathProvision" select="$pInstrument/earlyTerminationProvision/optionalEarlyTermination"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <!-- 
						Optional Early termination Date 
						Exclude if the Optional early termination Date is the same as the cash Settlement payment date
						-->
            <xsl:call-template name ="displayRelevantUnderlyngDate">
              <xsl:with-param name ="pPathProvision" select="$pInstrument/earlyTerminationProvision/optionalEarlyTermination"/>
            </xsl:call-template>

            <!-- seller party reference -->
            <xsl:if test ="$pInstrument/earlyTerminationProvision/optionalEarlyTermination/singlePartyOption/sellerPartyReference/node()=true()">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'Seller'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getPartyNameAndID" >
                      <xsl:with-param name="pParty" select="$pInstrument/earlyTerminationProvision/optionalEarlyTermination/singlePartyOption/sellerPartyReference/@href" />
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>

            <!-- buyer party reference -->
            <xsl:if test ="$pInstrument/earlyTerminationProvision/optionalEarlyTermination/singlePartyOption/buyerPartyReference/node()=true()">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'Buyer'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getPartyNameAndID" >
                      <xsl:with-param name="pParty" select="$pInstrument/earlyTerminationProvision/optionalEarlyTermination/singlePartyOption/buyerPartyReference/@href" />
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>

            <!-- Calculation Agent -->
            <xsl:if test ="$pInstrument/earlyTerminationProvision/optionalEarlyTermination/calculationAgent/node()=true()">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'CalculationAgent'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getProvisionCalculationAgent">
                      <xsl:with-param name="pPathProvision" select="$pInstrument/earlyTerminationProvision/optionalEarlyTermination"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>
          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>
  </xsl:template>

  <!-- display Cancelable Provision  template -->
  <xsl:template name="displayCancelableProvision">
    <xsl:param name="pInstrument"/>
    <fo:block keep-together.within-page="always">
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$varEarlyTerminationProvision"/>
        </xsl:call-template>
      </fo:block>
      <fo:block margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'OptionalEarlyTermination'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Applicable'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'OptionStyle'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getProvisionOptionStyle">
                    <xsl:with-param name="pPathProvision" select="$pInstrument/cancelableProvision"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <!-- 
						Optional early termination Date 
						Exclude if the Optional early termination Date is the same as the cash Settlement payment date
						-->
            <xsl:call-template name ="displayRelevantUnderlyngDate">
              <xsl:with-param name ="pPathProvision" select="$pInstrument/cancelableProvision"/>
            </xsl:call-template>
            <!-- seller party reference -->
            <xsl:if test ="$pInstrument/cancelableProvision/sellerPartyReference/node()=true()">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'Seller'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getPartyNameAndID" >
                      <xsl:with-param name="pParty" select="$pInstrument/cancelableProvision/sellerPartyReference/@href" />
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>
            <!-- buyer party reference -->
            <xsl:if test ="$pInstrument/cancelableProvision/buyerPartyReference/node()=true()">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'Buyer'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getPartyNameAndID" >
                      <xsl:with-param name="pParty" select="$pInstrument/cancelableProvision/buyerPartyReference/@href" />
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>
            <!-- 
						Calculation Agent
						-->
            <xsl:if test ="$pInstrument/cancelableProvision/calculationAgent/node()=true()">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'CalculationAgent'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getProvisionCalculationAgent">
                      <xsl:with-param name="pPathProvision" select="$pInstrument/cancelableProvision"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>
          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>
  </xsl:template>

  <!-- display Mandatory Early Termination template -->
  <xsl:template name="displayMandatoryEarlyTermination">
    <xsl:param name="pInstrument"/>
    <fo:block keep-together.within-page="always">
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$varEarlyTerminationProvision"/>
        </xsl:call-template>
      </fo:block>
      <fo:block margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'MandatoryEarlyTermination'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Applicable'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>
  </xsl:template>

  <!-- displayRelevantUnderlyngDate template -->
  <xsl:template name="displayRelevantUnderlyngDate">
    <xsl:param name="pPathProvision"/>

    <xsl:if test ="//relevantUnderlyingDate/relativeDates/periodMultiplier!=0 and //relevantUnderlyingDate/relativeDates/period !='D'">
      <fo:table-row>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'OptionalEarlyTerminationDate'"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:choose>
              <xsl:when test ="$pPathProvision/europeanExercise/node()=true()">
                <xsl:call-template name="getRelevantUnderlyngAdjustableOrRelativeDates">
                  <xsl:with-param name="pExerciseType" select="$pPathProvision/europeanExercise"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test ="$pPathProvision/americanExercise/node()=true()">
                <xsl:call-template name="getRelevantUnderlyngAdjustableOrRelativeDates">
                  <xsl:with-param name="pExerciseType" select="$pPathProvision/americanExercise"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test ="$pPathProvision/bermudaExercise/node()=true()">
                <xsl:call-template name="getRelevantUnderlyngAdjustableOrRelativeDates">
                  <xsl:with-param name="pExerciseType" select="$pPathProvision/bermudaExercise"/>
                </xsl:call-template>
              </xsl:when>
            </xsl:choose>
          </fo:block>
        </fo:table-cell>
      </fo:table-row>
    </xsl:if>
  </xsl:template>

  <!-- displayProvisionProcedureForExercise template -->
  <xsl:template name="displayProvisionProcedureForExercise">
    <xsl:param name="pPathProvision"/>

    <fo:block keep-together.within-page="always">

      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$varProcedureForExercise"/>
        </xsl:call-template>
      </fo:block>
      <fo:block margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>
            <!-- 
						*********************
						european exercise
						*********************
						-->
            <xsl:if test="$pPathProvision/europeanExercise">
              <!-- Expiration Date -->
              <xsl:if test ="$pPathProvision/europeanExercise/expirationDate/relativeDate/periodMultiplier!=0">
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'ExpirationDate'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getProvisionsAdjustableOrRelativeOrRangeDate" >
                        <xsl:with-param name="pDate" select="$pPathProvision/europeanExercise/expirationDate"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:if>
              <!-- Earliest Exercise Time -->
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'EarliestExerciseTime'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:value-of select="$pPathProvision/europeanExercise/earliestExerciseTime/hourMinuteTime"/>
                    <xsl:value-of select="$gVarSpace"/>
                    <xsl:call-template name="getFullNameBC">
                      <xsl:with-param name="pBC" select="$pPathProvision/europeanExercise/earliestExerciseTime/businessCenter"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <!-- Expiration Time -->
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'ExpirationTime'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:value-of select="$pPathProvision/europeanExercise/expirationTime/hourMinuteTime"/>
                    <xsl:value-of select="$gVarSpace"/>
                    <xsl:call-template name="getFullNameBC">
                      <xsl:with-param name="pBC" select="$pPathProvision/europeanExercise/expirationTime/businessCenter"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <!-- 
							Partial Exercise 
							TO DO: test this section
							the node partialExercise is missing
							-->
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'PartialExercise'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:choose>
                      <xsl:when test ="$pPathProvision/europeanExercise/partialExercise/node()=true()">
                        <xsl:call-template name="getTranslation">
                          <xsl:with-param name="pResourceName" select="'Applicable'"/>
                        </xsl:call-template>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:call-template name="getTranslation">
                          <xsl:with-param name="pResourceName" select="'Inapplicable'"/>
                        </xsl:call-template>
                      </xsl:otherwise>
                    </xsl:choose>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>
            <!-- 
						**************************
						american exercise 
						**************************
						-->
            <xsl:if test="$pPathProvision/americanExercise">
              <!-- Commencement Date -->
              <xsl:if test ="$pPathProvision/americanExercise/commencementDate/relativeDate/periodMultiplier!=0">
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'CommencementDate'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getProvisionsAdjustableOrRelativeOrRangeDate">
                        <xsl:with-param name="pDate" select="$pPathProvision/americanExercise/commencementDate"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:if>
              <!-- Expiration Date -->
              <xsl:if test ="$pPathProvision/americanExercise/expirationDate/relativeDate/periodMultiplier!=0">
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'ExpirationDate'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getProvisionsAdjustableOrRelativeOrRangeDate" >
                        <xsl:with-param name="pDate" select="$pPathProvision/americanExercise/expirationDate"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:if>
              <!-- Earliest Exercise Time -->
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'EarliestExerciseTime'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:value-of select="$pPathProvision/americanExercise/earliestExerciseTime/hourMinuteTime"/>
                    <xsl:value-of select="$gVarSpace"/>
                    <xsl:call-template name="getFullNameBC">
                      <xsl:with-param name="pBC" select="$pPathProvision/americanExercise/earliestExerciseTime/businessCenter"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <!-- Latest Exercise Time -->
              <xsl:if test ="$pPathProvision/americanExercise/latestExerciseTime/node()=true()">
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'LatestExerciseTime'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:value-of select="$pPathProvision/americanExercise/latestExerciseTime/hourMinuteTime"/>
                      <xsl:value-of select="$gVarSpace"/>
                      <xsl:call-template name="getFullNameBC">
                        <xsl:with-param name="pBC" select="$pPathProvision/americanExercise/latestExerciseTime/businessCenter"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:if>
              <!-- Expiration Time -->
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'ExpirationTime'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:value-of select="$pPathProvision/americanExercise/expirationTime/hourMinuteTime"/>
                    <xsl:value-of select="$gVarSpace"/>
                    <xsl:call-template name="getFullNameBC">
                      <xsl:with-param name="pBC" select="$pPathProvision/americanExercise/expirationTime/businessCenter"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <!-- Multiple Exercise -->
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'MultipleExercise'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:choose>
                      <xsl:when test ="$pPathProvision/americanExercise/multipleExercise/node()=true()">
                        <xsl:call-template name="getTranslation">
                          <xsl:with-param name="pResourceName" select="'Applicable'"/>
                        </xsl:call-template>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:call-template name="getTranslation">
                          <xsl:with-param name="pResourceName" select="'Inapplicable'"/>
                        </xsl:call-template>
                      </xsl:otherwise>
                    </xsl:choose>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>
            <!-- 
						********************
						bermuda exercise 
						********************
						-->
            <xsl:if test="$pPathProvision/bermudaExercise">
              <xsl:call-template name="displayBermudaExerciseDates">
                <xsl:with-param name="pExerciseType" select="'$pPathProvision/bermudaExercise'"/>
              </xsl:call-template>
              <!-- Earliest Exercise Time -->
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'EarliestExerciseTime'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:value-of select="$pPathProvision/bermudaExercise/earliestExerciseTime/hourMinuteTime"/>
                    <xsl:value-of select="$gVarSpace"/>
                    <xsl:call-template name="getFullNameBC">
                      <xsl:with-param name="pBC" select="$pPathProvision/bermudaExercise/earliestExerciseTime/businessCenter"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <!-- Latest Exercise Time -->
              <xsl:if test ="$pPathProvision/bermudaExercise/latestExerciseTime/node()=true()">
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'LatestExerciseTime'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:value-of select="$pPathProvision/bermudaExercise/latestExerciseTime/hourMinuteTime"/>
                      <xsl:value-of select="$gVarSpace"/>
                      <xsl:call-template name="getFullNameBC">
                        <xsl:with-param name="pBC" select="$pPathProvision/bermudaExercise/latestExerciseTime/businessCenter"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:if>
              <!-- Expiration Time -->
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'ExpirationTime'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:value-of select="$pPathProvision/bermudaExercise/expirationTime/hourMinuteTime"/>
                    <xsl:value-of select="$gVarSpace"/>
                    <xsl:call-template name="getFullNameBC">
                      <xsl:with-param name="pBC" select="$pPathProvision/bermudaExercise/expirationTime/businessCenter"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <!-- Multiple Exercise -->
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'MultipleExercise'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:choose>
                      <xsl:when test ="$pPathProvision/bermudaExercise/multipleExercise/node()=true()">
                        <xsl:call-template name="getTranslation">
                          <xsl:with-param name="pResourceName" select="'Applicable'"/>
                        </xsl:call-template>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:call-template name="getTranslation">
                          <xsl:with-param name="pResourceName" select="'Inapplicable'"/>
                        </xsl:call-template>
                      </xsl:otherwise>
                    </xsl:choose>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>
          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>
  </xsl:template>

  <!-- 
	*************************************************
	display Bermuda Exercise Dates template
	Bermuda can contains two or more exercise dates
	to testing
	**************************************************
	-->
  <xsl:template name="displayBermudaExerciseDates">
    <xsl:param name="pExerciseType"/>

    <xsl:for-each select ="$pExerciseType/bermudaExerciseDates">
      <fo:table-row>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <xsl:choose>
            <xsl:when test="position()=1">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'BermudaExerciseDates'"/>
                </xsl:call-template>
              </fo:block>
            </xsl:when>
            <xsl:otherwise>
              <fo:block border="{$gVarBorder}"></fo:block>
            </xsl:otherwise>
          </xsl:choose>
        </fo:table-cell>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="getAdjustableOrRelativeDate">
              <xsl:with-param name="pDate" select="current()"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
      </fo:table-row>
    </xsl:for-each>
  </xsl:template>

  <!-- 
	*************************************************
	display Provision Settlement Terms
	**************************************************
	-->
  <xsl:template name="displayProvisionSettlementTerms">
    <xsl:param name="pPathProvision"/>


    <fo:block keep-together.within-page="always">

      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$varSettlementTerms"/>
        </xsl:call-template>
      </fo:block>

      <fo:block margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'CashSettlement'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:choose>
                    <xsl:when test="$pPathProvision/cashSettlement/node()=true()">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'Applicable'"/>
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'Inapplicable'"/>
                      </xsl:call-template>
                    </xsl:otherwise>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <xsl:if test="$pPathProvision/cashSettlement/node()=true()">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'CashSettlementValuationTime'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:value-of select="$pPathProvision/cashSettlement/cashSettlementValuationTime/hourMinuteTime"/>
                    <xsl:value-of select="$gVarSpace"/>
                    <xsl:call-template name="getFullNameBC">
                      <xsl:with-param name="pBC" select="$pPathProvision/cashSettlement/cashSettlementValuationTime/businessCenter"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'CashSettlementValuationDate'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getRelativeDate">
                      <xsl:with-param name="pRelativeDate" select="$pPathProvision/cashSettlement/cashSettlementValuationDate"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'ValuationBusinessDays'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getBCFullNames">
                      <xsl:with-param name="pBCs" select="$pPathProvision/cashSettlement/cashSettlementValuationDate/businessCenters"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'CashSettlementPaymentDate'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getProvisionsAdjustableOrRelativeOrRangeDate" >
                      <xsl:with-param name="pDate" select="$pPathProvision/cashSettlement/cashSettlementPaymentDate"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'CashSettlementMethod'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:value-of select="$getCashSettlementMethod"/>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'CashSettlementCurrency'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:value-of select="//cashSettlement/cashPriceMethod/cashSettlementCurrency"/>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'QuotationRate'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:value-of select="//cashSettlement/cashPriceMethod/quotationRateType"/>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>
          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>
  </xsl:template>
  <!--
	================================================================================
	== Provisions END REGION
	=================================================================================
	-->

  <!--
	================================================================================
	Display Additional Payment 
	=================================================================================
	-->
  <xsl:variable name="varAdditionalPayment">
    <xsl:call-template name="getTranslation">
      <xsl:with-param name="pResourceName" select="'AdditionalPayment'"/>
    </xsl:call-template>
  </xsl:variable>


  <xsl:template name="displayAdditionalPayment">
    <xsl:param name="pInstrument"/>

    <xsl:variable name="varLowerLetters">abcdefghijklmnopqrstuvwxyz</xsl:variable>
    <xsl:variable name="varUpperLetters">ABCDEFGHIJKLMNOPQRSTUVWXYZ</xsl:variable>

    <xsl:variable name ="varPaymentType">
      <xsl:value-of select="$pInstrument/additionalPayment/paymentType"/>
    </xsl:variable>

    <xsl:variable name="varUpperPaymentType">
      <xsl:value-of select="translate($varPaymentType, $varLowerLetters, $varUpperLetters)" />
    </xsl:variable>

    <xsl:if test="$pInstrument/additionalPayment">
      <fo:block keep-together.within-page="always">
        <fo:block border="{$gVarBorder}">
          <xsl:call-template name="displayTitle">
            <xsl:with-param name="pText" select="$varAdditionalPayment"/>
          </xsl:call-template>
        </fo:block>
        <fo:block margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
          <xsl:for-each select="$pInstrument/additionalPayment">
            <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
              <xsl:call-template name="createColumns"/>
              <fo:table-body>
                <!-- UpFront Amount -->
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="$varUpperPaymentType"/>
                      </xsl:call-template>
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'_Amount'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="formatMoney">
                        <xsl:with-param name="pMoney" select="./paymentAmount"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
                <!-- UpFront Payer -->
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="$varUpperPaymentType"/>
                      </xsl:call-template>
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'_Payer'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getPartyNameOrID">
                        <xsl:with-param name="pPartyID" select="./payerPartyReference/@href"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
                <!-- UpFront Receiver -->
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="$varUpperPaymentType"/>
                      </xsl:call-template>
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'_Receiver'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getPartyNameOrID">
                        <xsl:with-param name="pPartyID" select="./receiverPartyReference/@href"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
                <!-- UpFront Payment Date -->
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="$varUpperPaymentType"/>
                      </xsl:call-template>
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'_PaymentDate'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="format-date">
                        <xsl:with-param name="xsd-date-time" select="./paymentDate/unadjustedDate"/>
                      </xsl:call-template>
                      <xsl:call-template name="getBusinessDayConvention" >
                        <xsl:with-param name="pBusinessDayConvention" select="./paymentDate/unadjustedDate/businessDayConvention" />
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </fo:table-body>
            </fo:table>
          </xsl:for-each>
        </fo:block>
      </fo:block>
    </xsl:if>
  </xsl:template>
  <!-- ================================================================================================================ -->
  <!-- END REGION: template display Additional payment                                                                  -->
  <!-- ================================================================================================================ -->
  <!-- 
	===============================================
	===============================================
	== IRD products: END REGION     =============
	===============================================
	===============================================
	-->

  <!-- 
	===============================================
	===============================================
	== FX products: BEGIN REGION     =============
	===============================================
	===============================================
	-->
  <!-- ================================================================== -->
  <!-- BEGIN region display Confirmation Text USED BY ALL FX PRODUCTS		-->
  <!-- ================================================================== -->
  <!-- &#xA; linefeed-treatment="preserve" -->
  <xsl:template name="displayFxConfirmationText">
    <xsl:param name="pIsBullion"/>

    <xsl:variable name="varDatedAsOf">
      <xsl:if test="$isValidMasterAgreementDate">
        <xsl:call-template name="format-date">
          <xsl:with-param name="xsd-date-time" select="$masterAgreementDtSignature"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>

    <fo:block border="{$gVarBorder}" text-align="justify" margin-left="{$varConfirmationTextLeftMarginCM}cm" margin-top="{$varConfirmationTextTopMarginCM}cm" font-size="{$varConfirmationTextFontSize}">
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'FXConfText1'"/>
        </xsl:call-template>
      </fo:block>
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'FXConfText2'"/>
        </xsl:call-template>
      </fo:block>
      <xsl:call-template name="displayBlockBreakline"/>
      <fo:block border="{$gVarBorder}">
        <xsl:if test ="$pIsBullion=true()">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'FXConfText3b'"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:if test ="$pIsBullion=false()">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'FXConfText3'"/>
          </xsl:call-template>
        </xsl:if>
      </fo:block>
      <xsl:call-template name="displayBlockBreakline"/>
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'FXConfText4'"/>
        </xsl:call-template>
        <xsl:if test="$isValidMasterAgreementDate">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'FXConfText5'"/>
          </xsl:call-template>
          <xsl:call-template name="format-date">
            <xsl:with-param name="xsd-date-time" select="$masterAgreementDtSignature"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'FXConfText6'"/>
        </xsl:call-template>
        <xsl:value-of select="$varParty_1_name" />
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'FXConfText7'"/>
        </xsl:call-template>
        <xsl:value-of select="$varParty_2_name" />.
      </fo:block>
      <xsl:call-template name="displayBlockBreakline"/>
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'FXConfText8'"/>
        </xsl:call-template>
      </fo:block>
      <xsl:call-template name="displayBlockBreakline"/>
    </fo:block>
  </xsl:template>
  <!-- ============================================================== -->
  <!-- END region display Confirmation Text USED BY ALL FX PRODUCTS   -->
  <!-- ============================================================== -->

  <!-- =================================================================================================	-->
  <!-- BEGIN  of Region Display Terms  USED BY FX PRODUCTS: FxSwap, FxLeg 	-->
  <!-- =================================================================================================	-->
  <xsl:template name="displayFxSwapLegTerms">

    <xsl:param name="pFxLegContainer"/>
    <xsl:param name="pIsNonDeliverable"/>

    <fo:block keep-together.within-page="always">
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$varTerms"/>
        </xsl:call-template>
      </fo:block>

      <fo:block border="{$gVarBorder}" margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'TradeDate'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="//trade/tradeHeader/tradeDate"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>

    <!-- Scan Leg -->
    <xsl:for-each select="$pFxLegContainer/fxSingleLeg">
      <xsl:variable name="pos" select="position()" />
      <xsl:variable name="curr" select="current()" />
      <xsl:choose>
        <xsl:when test="$pIsNonDeliverable=true()">
          <!-- Non Deliverable -->
          <xsl:call-template name="displayNonDeliverableInformation">
            <xsl:with-param name="pFxSingleLeg" select="$curr"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <!-- Deliverable -->
          <xsl:call-template name="displayDeliverableInformation">
            <xsl:with-param name="pFxSingleLeg" select="$curr"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>

  </xsl:template>
  <!-- =================================================================================================	-->
  <!-- END  of Region Display Terms  USED BY FX PRODUCTS: FxSwap, FxLeg	-->
  <!-- =================================================================================================	-->

  <!-- ==========================================================================================================	-->
  <!-- BEGIN of Region Display FxOption Terms  USED BY FX PRODUCTS: FxDigitalOption                               -->
  <!-- =========================================================================================================	-->
  <xsl:template name="displayFxDigitalOptionTerms">
    <xsl:param name="pFxOption"/>

    <xsl:variable name="IsAmerican" select="$pFxOption/fxAmericanTrigger=true()" />
    <xsl:variable name="IsEuropean" select="$pFxOption/fxEuropeanTrigger=true()" />

    <fo:block keep-together.within-page="always">
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$varTerms"/>
        </xsl:call-template>
      </fo:block>

      <fo:block border="{$gVarBorder}" margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'TradeDate'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="//trade/tradeHeader/tradeDate"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Buyer'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getPartyNameAndID" >
                    <xsl:with-param name="pParty" select="$pFxOption/buyerPartyReference/@href" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Seller'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getPartyNameAndID" >
                    <xsl:with-param name="pParty" select="$pFxOption/sellerPartyReference/@href" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'CurrencyOptionType'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Binary'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'ExpirationDate'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="$pFxOption/expiryDateTime/expiryDate"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'ExpirationTime'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-shorttime">
                    <xsl:with-param name="xsd-date-time" select="$pFxOption/expiryDateTime/expiryTime/hourMinuteTime"/>
                  </xsl:call-template>
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'_at_'"/>
                  </xsl:call-template>
                  <xsl:call-template name="getFullNameBC">
                    <xsl:with-param name="pBC" select="$pFxOption/expiryDateTime/expiryTime/businessCenter" />
                  </xsl:call-template>
                  <xsl:if test="$pFxOption/expiryDateTime/cutName=true()">
                    <xsl:value-of select="' ( '"/>
                    <xsl:value-of select="$pFxOption/expiryDateTime/cutName"/>
                    <xsl:value-of select="' ) '"/>
                  </xsl:if>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Settlement'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'NonDeliverable'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'SettlementAmount'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-money2">
                    <xsl:with-param name="currency" select="$pFxOption/triggerPayout/currency"/>
                    <xsl:with-param name="amount" select="$pFxOption/triggerPayout/amount"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'SettlementDate'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:if test="$pFxOption/triggerPayout/payoutStyle='Immediate' and $IsAmerican=true()">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'TwoBusinessDaysAfterTheDateThatTheBinaryEventOccurs'"/>
                    </xsl:call-template>
                  </xsl:if>
                  <xsl:if test="$pFxOption/triggerPayout/payoutStyle='Deferred' or $IsEuropean=true()">
                    <xsl:call-template name="format-date">
                      <xsl:with-param name="xsd-date-time" select="$pFxOption/valueDate"/>
                    </xsl:call-template>
                  </xsl:if>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Premium'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-money2">
                    <xsl:with-param name="currency" select="$pFxOption/fxOptionPremium/premiumAmount/currency"/>
                    <xsl:with-param name="amount" select="$pFxOption/fxOptionPremium/premiumAmount/amount"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'PremiumPaymentDate'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="$pFxOption/fxOptionPremium/premiumSettlementDate"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <!--
						It displays trigger section (displayFxAmericanOrEuropeanTriggers)if the digital contains an or more triggers
						It displays barrier section (displayFxBarriers) if the digital contains an or more barriers
						-->
            <xsl:if test="$pFxOption/fxAmericanTrigger=true() or $pFxOption/fxEuropeanTrigger=true()">
              <xsl:call-template name="displayFxAmericanOrEuropeanTriggers">
                <xsl:with-param name="pFxOption" select="$pFxOption"/>
              </xsl:call-template>
            </xsl:if>

            <xsl:if test="$pFxOption/fxBarrier=true()">
              <xsl:call-template name="displayFxBarriers">
                <xsl:with-param name="pFxOption" select="$pFxOption"/>
              </xsl:call-template>
            </xsl:if>

            <!-- 
						**********************************************************************************************************************
						============================================================================================
						== add this region in comment
						== The information about "Currency pair" not exist in ISDA confirmation
						============================================================================================
						<fo:table-row>
							<fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
								<fo:block border="{$gVarBorder}">
									<xsl:value-of select="'Currency pair:'"/>
								</fo:block>
							</fo:table-cell>
							<fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
								<fo:block border="{$gVarBorder}">
									<xsl:value-of select="$pFxOption/quotedCurrencyPair/currency1"/>/<xsl:value-of select="$pFxOption/quotedCurrencyPair/currency2"/>
								</fo:block>
							</fo:table-cell>
						</fo:table-row>
						***********************************************************************************************************************
						-->
          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>
  </xsl:template>
  <!-- =================================================================================================	-->
  <!-- END  of Region Display FxOption Terms  USED BY FX PRODUCTS FxDigitalOption 	-->
  <!-- =================================================================================================	-->

  <!-- =================================================================================================	-->
  <!-- BEGIN  of Region display fxAmericanOrEuropeanTrigger USED BY FxDigitalOption  	                    -->
  <!-- =================================================================================================	-->
  <!-- 
	Create displayFxAmericanOrEuropeanTriggers template 
	Add the roles to manage two or more triggers
	-->
  <xsl:template name="displayFxAmericanOrEuropeanTriggers">
    <xsl:param name="pFxOption"/>

    <fo:table-row>
      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
        <fo:block border="{$gVarBorder}">
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
        <fo:block border="{$gVarBorder}">
        </fo:block>
      </fo:table-cell>
    </fo:table-row>

    <!-- It displays "barrier event = applicable" if triggers exists -->
    <fo:table-row>
      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
        <fo:block border="{$gVarBorder}">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'TriggerEvent'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
        <fo:block border="{$gVarBorder}">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'Applicable'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
    </fo:table-row>
    <!-- 
		Event Type displays: 
		- When the trade contains one trigger (One Touch or No Touch)
		- When the trade contains two triggers (Double One Touch or Double No Touch)
		And add the text: 
		- Binary (for an AmericanTrigger) 
		- "At expiration binary" (for an EuropeanTrigger) 
		-->
    <fo:table-row>
      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
        <fo:block border="{$gVarBorder}">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'EventType'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
        <fo:block border="{$gVarBorder}">

          <xsl:if test="$pFxOption/fxAmericanTrigger=true()">
            <xsl:call-template name="getTriggerEventType">
              <xsl:with-param name="pTouchCondition" select="$pFxOption/fxAmericanTrigger/touchCondition" />
              <xsl:with-param name="pTotalTrigger" select="count($pFxOption/fxAmericanTrigger)" />
            </xsl:call-template>
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'_Binary'"/>
            </xsl:call-template>
          </xsl:if>
          <xsl:if test="$pFxOption/fxEuropeanTrigger=true()">
            <xsl:call-template name="getTriggerEventType">
              <xsl:with-param name="pTouchCondition" select="$pFxOption/fxEuropeanTrigger/touchCondition" />
              <xsl:with-param name="pTotalTrigger" select="count($pFxOption/fxEuropeanTrigger)" />
            </xsl:call-template>
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'_AtExpirationBinary'"/>
            </xsl:call-template>
          </xsl:if>
        </fo:block>
      </fo:table-cell>
    </fo:table-row>

    <fo:table-row>
      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
        <fo:block border="{$gVarBorder}">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'InitialSpotRate'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
        <fo:block border="{$gVarBorder}">
          <xsl:if test="$pFxOption/spotRate=true()">
            <xsl:call-template name="format-fxrate">
              <xsl:with-param name="fxrate" select="$pFxOption/spotRate"/>
            </xsl:call-template>
            <xsl:value-of select="$gVarSpace"/>
            <xsl:call-template name="format-currency-pair">
              <xsl:with-param name="quotedCurrencyPair" select="$pFxOption/quotedCurrencyPair"/>
            </xsl:call-template>
          </xsl:if>
          <xsl:if test="$pFxOption/spotRate=false()">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'NotAvailable'"/>
            </xsl:call-template>
          </xsl:if>
        </fo:block>
      </fo:table-cell>
    </fo:table-row>

    <xsl:if test="$pFxOption/fxAmericanTrigger=true()">
      <xsl:call-template name="displayTriggerRate">
        <xsl:with-param name="pFxOption" select="$pFxOption/fxAmericanTrigger" />
        <xsl:with-param name="pTotalTrigger" select="count($pFxOption/fxAmericanTrigger)" />
      </xsl:call-template>
    </xsl:if>

    <xsl:if test="$pFxOption/fxEuropeanTrigger=true()">
      <xsl:call-template name="displayTriggerRate">
        <xsl:with-param name="pFxOption" select="$pFxOption/fxEuropeanTrigger" />
        <xsl:with-param name="pTotalTrigger" select="count($pFxOption/fxEuropeanTrigger)" />
      </xsl:call-template>
    </xsl:if>

    <!-- 
		It displays the rate source if the node exist
		-->
    <xsl:if test="$pFxOption/fxAmericanTrigger/informationSource=true()">
      <fo:table-row>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'TriggerEventRateSource'"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>

        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:value-of select="$pFxOption/fxAmericanTrigger/informationSource/rateSource"/>
            <xsl:text> - </xsl:text>
            <xsl:value-of select="$pFxOption/fxAmericanTrigger/informationSource/rateSourcePage"/>
            <xsl:text> - </xsl:text>
            <xsl:value-of select="$pFxOption/fxAmericanTrigger/informationSource/rateSourcePageHeading"/>
          </fo:block>
        </fo:table-cell>
      </fo:table-row>
    </xsl:if>

    <xsl:if test="$pFxOption/fxEuropeanTrigger/informationSource=true()">
      <fo:table-row>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'TriggerEventRateSource'"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>

        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:value-of select="$pFxOption/fxEuropeanTrigger/informationSource/rateSource"/>
            <xsl:text> - </xsl:text>
            <xsl:value-of select="$pFxOption/fxEuropeanTrigger/informationSource/rateSourcePage"/>
            <xsl:text> - </xsl:text>
            <xsl:value-of select="$pFxOption/fxEuropeanTrigger/informationSource/rateSourcePageHeading"/>
          </fo:block>
        </fo:table-cell>
      </fo:table-row>
    </xsl:if>
    <!-- 
		It displays Observation Start Date and Observation End Date only for American Trigger
		-->
    <xsl:if test="$pFxOption/fxAmericanTrigger/observationStartDate=true()">
      <fo:table-row>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'TriggerObservationStartDate'"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="format-date">
              <xsl:with-param name="xsd-date-time" select="$pFxOption/fxAmericanTrigger/observationStartDate"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
      </fo:table-row>
    </xsl:if>

    <xsl:if test="$pFxOption/fxAmericanTrigger/observationEndDate=true()">
      <fo:table-row>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'TriggerObservationEndDate'"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="format-date">
              <xsl:with-param name="xsd-date-time" select="$pFxOption/fxAmericanTrigger/observationEndDate"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
      </fo:table-row>
    </xsl:if>

  </xsl:template>

  <!-- =================================================================================================	-->
  <!-- END  of Region display fxAmericanOrEuropeanTrigger USED BY FxDigitalOption  	-->
  <!-- =================================================================================================	-->


  <!-- =================================================================================================	-->
  <!-- BEGIN  of Region display Trigger Rate  USED BY FxDigitalOption  	-->
  <!-- =================================================================================================	-->

  <xsl:template name="displayTriggerRate">
    <xsl:param name="pFxOption"/>
    <xsl:param name="pTotalTrigger"/>
    <!-- 
		If total trigger > 2 (=1) 
		it returns the barrier level information of the trigger 
		[eg. Barrier Level: 1.45 USD/EUR])
		-->
    <xsl:if test="$pTotalTrigger &lt; 2">
      <fo:table-row>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'TriggerLevel'"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <!-- Affichage du montant de la barrier-level -->
            <xsl:call-template name="format-fxrate">
              <xsl:with-param name="fxrate" select="$pFxOption/triggerRate" />
            </xsl:call-template>
            <xsl:value-of select="$gVarSpace"/>
            <!-- Affichage du couple de la devise -->
            <xsl:call-template name="format-currency-pair">
              <xsl:with-param name="quotedCurrencyPair" select="$pFxOption/quotedCurrencyPair" />
              <xsl:with-param name="tradeName" select="$pFxOption" />
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
      </fo:table-row>
    </xsl:if>
    <!-- 
		If total trigger > 1 it returns two Barrier Level: Upper and Lower 
		[eg. UpperBarrierLevel: 1.45 USD/EUR -  LowerBarrierLevel: 1.29 USD/EUR]
		-->
    <xsl:if test="$pTotalTrigger &gt; 1">
      <xsl:for-each select="$pFxOption">
        <xsl:sort select="current()/triggerRate" order="descending" />
        <!-- Affichage de la Barrière la plus haute -->
        <xsl:if test="position() = 1 ">
          <fo:table-row>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'UpperTriggerLevel'"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <!-- Affichage du montant de la barrier-level -->
                <xsl:call-template name="format-fxrate">
                  <xsl:with-param name="fxrate" select="current()/triggerRate" />
                </xsl:call-template>
                <xsl:value-of select="$gVarSpace"/>
                <!-- Affichage du couple de la devise -->
                <xsl:call-template name="format-currency-pair">
                  <xsl:with-param name="quotedCurrencyPair" select="current()/quotedCurrencyPair" />
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </xsl:if>
        <!-- Affichage de la Barrière la plus basse -->
        <xsl:if test="position() = last() ">
          <fo:table-row>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'LowerTriggerLevel'"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <!-- Affichage du montant de la barrier-level -->
                <xsl:call-template name="format-fxrate">
                  <xsl:with-param name="fxrate" select="current()/triggerRate" />
                </xsl:call-template>
                <xsl:value-of select="$gVarSpace"/>
                <!-- Affichage du couple de la devise -->
                <xsl:call-template name="format-currency-pair">
                  <xsl:with-param name="quotedCurrencyPair" select="current()/quotedCurrencyPair" />
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </xsl:if>
      </xsl:for-each>
    </xsl:if>
  </xsl:template>
  <!-- =================================================================================================	-->
  <!-- END  of Region display Trigger Rate  USED BY FxDigitalOption  	-->
  <!-- =================================================================================================	-->







  <!-- =================================================================================================	-->
  <!-- BEGIN  of Region Display FxBarrierOption Terms  USED BY FX PRODUCTS: FXBarrierOption 	-->
  <!-- =================================================================================================	-->
  <!--Template display Terms -->
  <xsl:template name="displayFxBarrierOptionTerms">
    <xsl:param name="pFxOption"/>
    <xsl:param name="pIsNonDeliverable"/>
    <xsl:variable name="IsAmerican" select="$pFxOption/valueDate='American'" />
    <xsl:variable name="IsEuropean" select="$pFxOption/valueDate='European'" />
    <xsl:variable name="IsBermuda" select="$pFxOption/valueDate='Bermuda'" />

    <fo:block keep-together.within-page="always">
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$varTerms"/>
        </xsl:call-template>
      </fo:block>

      <fo:block border="{$gVarBorder}" margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'TradeDate'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>

              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="//trade/tradeHeader/tradeDate"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Buyer'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getPartyNameAndID" >
                    <xsl:with-param name="pParty" select="$pFxOption/buyerPartyReference/@href" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Seller'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getPartyNameAndID" >
                    <xsl:with-param name="pParty" select="$pFxOption/sellerPartyReference/@href" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>


            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'CurrencyOptionStyle'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:value-of select="$pFxOption/exerciseStyle"/>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'CurrencyOptionType'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>

              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:value-of select="$pFxOption/putCurrencyAmount/currency"/>
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Put'"/>
                  </xsl:call-template>
                  <xsl:value-of select="' / '"/>
                  <xsl:value-of select="$pFxOption/callCurrencyAmount/currency"/>
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Call'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>

            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'CallCurrencyCallCurrencyAmount'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-money2">
                    <xsl:with-param name="currency" select="$pFxOption/callCurrencyAmount/currency"/>
                    <xsl:with-param name="amount" select="$pFxOption/callCurrencyAmount/amount"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'PutCurrencyPutCurrencyAmount'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-money2">
                    <xsl:with-param name="currency" select="$pFxOption/putCurrencyAmount/currency"/>
                    <xsl:with-param name="amount" select="$pFxOption/putCurrencyAmount/amount"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'StrikePrice'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-fxrate">
                    <xsl:with-param name="fxrate" select="$pFxOption/fxStrikePrice/rate" />
                  </xsl:call-template>
                  <xsl:value-of select="$gVarSpace"/>
                  <xsl:call-template name="format-currency-pair">
                    <xsl:with-param name="quotedCurrencyPair" select="$pFxOption/fxStrikePrice/strikeQuoteBasis" />
                    <xsl:with-param name="tradeName" select="$pFxOption" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'ExpirationDate'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="$pFxOption/expiryDateTime/expiryDate"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'ExpirationTime'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-shorttime">
                    <xsl:with-param name="xsd-date-time" select="$pFxOption/expiryDateTime/expiryTime/hourMinuteTime"/>
                  </xsl:call-template>
                  <xsl:value-of select="' ('"/>
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'localTimeIn_'"/>
                  </xsl:call-template>
                  <xsl:call-template name="getFullNameBC">
                    <xsl:with-param name="pBC" select="$pFxOption/expiryDateTime/expiryTime/businessCenter" />
                  </xsl:call-template>
                  <xsl:value-of select="')'"/>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Settlement'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:if test="$pFxOption/cashSettlementTerms/node()=true()">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'NonDeliverable'"/>
                    </xsl:call-template>
                  </xsl:if>
                  <xsl:if test="$pFxOption/cashSettlementTerms/node()=false()">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'Deliverable'"/>
                    </xsl:call-template>
                  </xsl:if>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <xsl:if test="$pFxOption/cashSettlementTerms/node()=true()">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'SettlementCurrency'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:value-of select="$pFxOption/cashSettlementTerms/settlementCurrency"/>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <!-- 
							===========================================================================================
							== Settlement rate or Settlement rate Option: To do this section for non delivery option
							==========================================================================================
							-->
              <!--
							<fo:table-row>
							<fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
							<fo:block border="{$gVarBorder}">
							<xsl:value-of select="'Settlement Rate:'"/>
							</fo:block>
							</fo:table-cell>
							<fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
							<fo:block border="{$gVarBorder}">
							
							</fo:block>
							</fo:table-cell>
							</fo:table-row>
							-->
            </xsl:if>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'SettlementDate'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="$pFxOption/valueDate"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <xsl:if test="$pFxOption/bermudanExerciseDates/date=true()">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'SpecifiedExerciseDate'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <xsl:for-each select="$pFxOption/bermudanExerciseDates/date">
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="format-date">
                        <xsl:with-param name="xsd-date-time" select="current()"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </xsl:for-each>
              </fo:table-row>
            </xsl:if>

            <xsl:if test="$pFxOption/fxOptionPremium/premiumAmount/amount=true()">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'Premium'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="format-money2">
                      <xsl:with-param name="currency" select="$pFxOption/fxOptionPremium/premiumAmount/currency"/>
                      <xsl:with-param name="amount" select="$pFxOption/fxOptionPremium/premiumAmount/amount"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'PremiumPaymentDate'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="$pFxOption/fxOptionPremium/premiumSettlementDate"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!--
						*******************************************************************************************************************
						Add "call template" to manage the barriers 
						*******************************************************************************************************************
						-->

            <xsl:if test="$pFxOption/fxBarrier=true()">
              <xsl:call-template name="displayFxBarriers">
                <xsl:with-param name="pFxOption" select="$pFxOption"/>
              </xsl:call-template>
            </xsl:if>

          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>
  </xsl:template>
  <!-- =================================================================================================	-->
  <!-- END of Region Display FxBarrierOption Terms  USED BY FX PRODUCTS: FXBarrierOption 	-->
  <!-- =================================================================================================	-->

  <!-- =====================================================================================================	-->
  <!-- BEGIN of Region Display displayFxBarriers USED BY FX PRODUCTS: FXBarrierOptions and FxDigitalOptions	-->
  <!-- ====================================================================================================	-->
  <!-- 
	Create displayFxBarriersNew template 
	-->
  <xsl:template name="displayFxBarriers">
    <xsl:param name="pFxOption"/>

    <fo:table-row>
      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
        <fo:block border="{$gVarBorder}">
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
        <fo:block border="{$gVarBorder}">
        </fo:block>
      </fo:table-cell>
    </fo:table-row>

    <fo:table-row>
      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
        <fo:block border="{$gVarBorder}">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'BarrierEvent'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
        <fo:block border="{$gVarBorder}">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'Applicable'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
    </fo:table-row>

    <fo:table-row>
      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
        <fo:block border="{$gVarBorder}">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'EventType'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
        <fo:block border="{$gVarBorder}">
          <xsl:call-template name="getBarrierEventType">
            <xsl:with-param name="pEventType" select="$pFxOption/fxBarrier/fxBarrierType" />
            <xsl:with-param name="pTotalBarrier" select="count($pFxOption/fxBarrier)" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
    </fo:table-row>

    <xsl:if test="count($pFxOption/fxBarrier) = 1 ">
      <fo:table-row>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'SpotExchangeRateDirection'"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="getBarrierSpotExchangeRateDirection">
              <xsl:with-param name="pEventType" select="$pFxOption/fxBarrier/fxBarrierType" />
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
      </fo:table-row>
    </xsl:if>

    <fo:table-row>
      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
        <fo:block border="{$gVarBorder}">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'InitialSpotPrice'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
        <fo:block border="{$gVarBorder}">
          <xsl:call-template name="format-fxrate">
            <xsl:with-param name="fxrate" select="$pFxOption/spotRate" />
          </xsl:call-template>
          <xsl:value-of select="$gVarSpace"/>
          <xsl:call-template name="format-currency-pair">
            <xsl:with-param name="quotedCurrencyPair" select="$pFxOption/fxStrikePrice/strikeQuoteBasis" />
            <xsl:with-param name="tradeName" select="$pFxOption" />
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
    </fo:table-row>

    <xsl:call-template name="displayBarrierLevel">
      <xsl:with-param name="pFxOption" select="$pFxOption" />
      <xsl:with-param name="pTotalBarrier" select="count($pFxOption/fxBarrier)" />
    </xsl:call-template>

    <xsl:if test="$pFxOption/fxBarrier/informationSource/node() = true() ">
      <fo:table-row>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'BarrierEventRateSource'"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:value-of select="$pFxOption/fxBarrier/informationSource/rateSource"/>
            <xsl:text> - </xsl:text>
            <xsl:value-of select="$pFxOption/fxBarrier/informationSource/rateSourcePage"/>
            <xsl:text> - </xsl:text>
            <xsl:value-of select="$pFxOption/fxBarrier/informationSource/rateSourcePageHeading"/>
          </fo:block>
        </fo:table-cell>
      </fo:table-row>
    </xsl:if>

    <xsl:if test="$pFxOption/fxBarrier/observationStartDate=true()">
      <fo:table-row>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'EventPeriodStartDateAndTime'"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="format-date">
              <xsl:with-param name="xsd-date-time" select="$pFxOption/fxBarrier/observationStartDate"/>
            </xsl:call-template>
            <xsl:value-of select="' 00:00 pm ('"/>
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'localTimeIn_'"/>
            </xsl:call-template>
            <xsl:call-template name="getFullNameBC">
              <xsl:with-param name="pBC" select="$pFxOption/expiryDateTime/expiryTime/businessCenter" />
            </xsl:call-template>
            <xsl:value-of select="')'"/>
          </fo:block>
        </fo:table-cell>
      </fo:table-row>
    </xsl:if>
    <xsl:if test="$pFxOption/fxBarrier/observationEndDate=true()">
      <fo:table-row>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'EventPeriodEndDateAndTime'"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="format-date">
              <xsl:with-param name="xsd-date-time" select="$pFxOption/fxBarrier/observationEndDate"/>
            </xsl:call-template>
            <xsl:value-of select="' 00:00 pm ('"/>
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'localTimeIn_'"/>
            </xsl:call-template>
            <xsl:call-template name="getFullNameBC">
              <xsl:with-param name="pBC" select="$pFxOption/expiryDateTime/expiryTime/businessCenter" />
            </xsl:call-template>
            <xsl:value-of select="')'"/>
          </fo:block>
        </fo:table-cell>
      </fo:table-row>
    </xsl:if>

  </xsl:template>

  <!-- =====================================================================================================	-->
  <!-- END of Region Display displayFxBarriers USED BY FX PRODUCTS: FXBarrierOptions and FxDigitalOptions	-->
  <!-- ====================================================================================================	-->


  <!-- =================================================================================================	-->
  <!-- BEGIN  of Region Display FxOptionLeg Terms  USED BY FX PRODUCTS: FxOptionLeg		-->
  <!-- =================================================================================================	-->
  <xsl:template name="displayFxOptionLegTerms">
    <xsl:param name="pFxOption"/>
    <xsl:param name="pIsNonDeliverable"/>
    <xsl:variable name="IsAmerican" select="$pFxOption/valueDate='American'" />
    <xsl:variable name="IsEuropean" select="$pFxOption/valueDate='European'" />
    <xsl:variable name="IsBermuda" select="$pFxOption/valueDate='Bermuda'" />

    <fo:block keep-together.within-page="always">
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$varTerms"/>
        </xsl:call-template>
      </fo:block>

      <fo:block border="{$gVarBorder}" margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'TradeDate'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="//trade/tradeHeader/tradeDate"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Buyer'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getPartyNameAndID" >
                    <xsl:with-param name="pParty" select="$pFxOption/buyerPartyReference/@href" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Seller'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getPartyNameAndID" >
                    <xsl:with-param name="pParty" select="$pFxOption/sellerPartyReference/@href" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'CurrencyOptionStyle'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:value-of select="$pFxOption/exerciseStyle"/>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'CurrencyOptionType'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:value-of select="$pFxOption/callCurrencyAmount/currency"/>
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Call'"/>
                  </xsl:call-template>
                  <xsl:value-of select="' / '"/>
                  <xsl:value-of select="$pFxOption/putCurrencyAmount/currency"/>
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Put'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'CallCurrencyCallCurrencyAmount'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-money2">
                    <xsl:with-param name="currency" select="$pFxOption/callCurrencyAmount/currency"/>
                    <xsl:with-param name="amount" select="$pFxOption/callCurrencyAmount/amount"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'PutCurrencyPutCurrencyAmount'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-money2">
                    <xsl:with-param name="currency" select="$pFxOption/putCurrencyAmount/currency"/>
                    <xsl:with-param name="amount" select="$pFxOption/putCurrencyAmount/amount"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'StrikePrice'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-fxrate">
                    <xsl:with-param name="fxrate" select="$pFxOption/fxStrikePrice/rate" />
                  </xsl:call-template>
                  <xsl:value-of select="$gVarSpace"/>
                  <xsl:call-template name="format-currency-pair">
                    <xsl:with-param name="quotedCurrencyPair" select="$pFxOption/fxStrikePrice/strikeQuoteBasis" />
                    <xsl:with-param name="tradeName" select="$pFxOption" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <xsl:if test="//cashSettlementTerms/node()=true()">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'Settlement'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'NonDeliverable'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>

              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'SettlementCurrency'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:value-of select="$pFxOption/cashSettlementTerms/settlementCurrency"/>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'ExpirationDate'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="$pFxOption/expiryDateTime/expiryDate"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'ExpirationTime'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-shorttime">
                    <xsl:with-param name="xsd-date-time" select="$pFxOption/expiryDateTime/expiryTime/hourMinuteTime"/>
                  </xsl:call-template>
                  <xsl:value-of select="' ('"/>
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'localTimeIn_'"/>
                  </xsl:call-template>
                  <xsl:call-template name="getFullNameBC">
                    <xsl:with-param name="pBC" select="$pFxOption/expiryDateTime/expiryTime/businessCenter" />
                  </xsl:call-template>
                  <xsl:value-of select="')'"/>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- 
							===============================================================================
							== To do this section
							== open point: use Spheres 2.3 to create a "Non Deliverable Currency Option"
							== verify XML
							===============================================================================
							-->
            <!--
							<fo:table-row>
							<fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
							<fo:block border="{$gVarBorder}">
							<xsl:value-of select="'Settlement Rate:'"/>
							</fo:block>
							</fo:table-cell>
							<fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
							<fo:block border="{$gVarBorder}">
							
							</fo:block>
							</fo:table-cell>
							</fo:table-row>
							-->

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'SettlementDate'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="$pFxOption/valueDate"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <xsl:if test="$pFxOption/bermudanExerciseDates/date=true()">
              <xsl:for-each select="$pFxOption/bermudanExerciseDates/date">
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'SpecifiedExerciseDate'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="format-date">
                        <xsl:with-param name="xsd-date-time" select="current()"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:for-each>
            </xsl:if>

            <xsl:if test="$pFxOption/fxOptionPremium/premiumAmount/amount=true() and $pFxOption/fxOptionPremium/premiumAmount/amount &gt; 1">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'Premium'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="format-money2">
                      <xsl:with-param name="currency" select="$pFxOption/fxOptionPremium/premiumAmount/currency"/>
                      <xsl:with-param name="amount" select="$pFxOption/fxOptionPremium/premiumAmount/amount"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>

            <xsl:if test="$pFxOption/fxOptionPremium/premiumQuote/premiumValue=true()">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'Price'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:if test="not( $pFxOption/fxOptionPremium/premiumQuote/premiumQuoteBasis = 'Explicit' )">
                      <xsl:call-template name="format-fixed-rate">
                        <xsl:with-param name="fixed-rate" select="$pFxOption/fxOptionPremium/premiumQuote/premiumValue" />
                      </xsl:call-template>

                      <xsl:value-of select="' ('"/>
                      <xsl:call-template name="getPremiumQuoteBasis">
                        <xsl:with-param name="CurrencyPairLabel" select="$pFxOption/fxOptionPremium/premiumQuote/premiumQuoteBasis" />
                      </xsl:call-template>
                      <xsl:value-of select="')'"/>
                    </xsl:if>

                    <xsl:if test="$pFxOption/fxOptionPremium/premiumQuote/premiumQuoteBasis = 'Explicit' ">
                      <xsl:call-template name="format-fxrate">
                        <xsl:with-param name="fxrate" select="$pFxOption/fxOptionPremium/premiumQuote/premiumValue" />
                      </xsl:call-template>
                    </xsl:if>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'PremiumPaymentDate'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}c">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="$pFxOption/fxOptionPremium/premiumSettlementDate"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>
  </xsl:template>
  <!-- =================================================================================================	-->
  <!-- END  of Region Display FxOptionLeg Terms  USED BY FX PRODUCTS: FxOptionLeg	 	-->
  <!-- =================================================================================================	-->

  <!-- =================================================================================================	-->
  <!-- BEGIN  of Region Display delivery information USED BY FX PRODUCTS: FxSwap, 	-->
  <!-- =================================================================================================	-->

  <!-- Display Deliverable Information -->
  <xsl:template name="displayDeliverableInformation">
    <xsl:param name="pFxSingleLeg"/>
    <xsl:call-template name="displayExchangedCurrency">
      <xsl:with-param name="pFxSingleLeg" select="$pFxSingleLeg"/>
    </xsl:call-template>
  </xsl:template>

  <!-- NonDeliverableInformation -->
  <xsl:template name="displayNonDeliverableInformation">
    <xsl:param name="pFxSingleLeg"/>

    <xsl:variable name="ReferenceCurrency">
      <xsl:call-template name="getReferenceCurrency">
        <xsl:with-param name="pFxSingleLeg" select="$pFxSingleLeg"/>
      </xsl:call-template>
    </xsl:variable>

    <fo:block border="{$gVarBorder}" margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
      <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
        <xsl:call-template name="createColumns"/>
        <fo:table-body>
          <fo:table-row>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'ReferenceCurrency'"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:value-of select="$ReferenceCurrency" />
              </fo:block>
            </fo:table-cell>
          </fo:table-row>

          <fo:table-row>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'ReferenceCurrencyNotionalAmount'"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:if test="$pFxSingleLeg/exchangedCurrency1/paymentAmount/currency=$ReferenceCurrency" >
                  <xsl:call-template name="format-money2">
                    <xsl:with-param name="currency" select="$pFxSingleLeg/exchangedCurrency1/paymentAmount/currency"/>
                    <xsl:with-param name="amount" select="$pFxSingleLeg/exchangedCurrency1/paymentAmount/amount"/>
                  </xsl:call-template>
                </xsl:if>
                <xsl:if test="$pFxSingleLeg/exchangedCurrency2/paymentAmount/currency=$ReferenceCurrency" >
                  <xsl:call-template name="format-money2">
                    <xsl:with-param name="currency" select="$pFxSingleLeg/exchangedCurrency2/paymentAmount/currency"/>
                    <xsl:with-param name="amount" select="$pFxSingleLeg/exchangedCurrency2/paymentAmount/amount"/>
                  </xsl:call-template>
                </xsl:if>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>

          <fo:table-row>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'NotionalAmount'"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:if test="$pFxSingleLeg/exchangedCurrency1/paymentAmount/currency!=$ReferenceCurrency" >
                  <xsl:call-template name="format-money2">
                    <xsl:with-param name="currency" select="$pFxSingleLeg/exchangedCurrency1/paymentAmount/currency"/>
                    <xsl:with-param name="amount" select="$pFxSingleLeg/exchangedCurrency1/paymentAmount/amount"/>
                  </xsl:call-template>
                </xsl:if>
                <xsl:if test="$pFxSingleLeg/exchangedCurrency2/paymentAmount/currency!=$ReferenceCurrency" >
                  <xsl:call-template name="format-money2">
                    <xsl:with-param name="currency" select="$pFxSingleLeg/exchangedCurrency2/paymentAmount/currency"/>
                    <xsl:with-param name="amount" select="$pFxSingleLeg/exchangedCurrency2/paymentAmount/amount"/>
                  </xsl:call-template>
                </xsl:if>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>

          <!--
					old valorization
					<fo:table-row>
						<fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
							<fo:block border="{$gVarBorder}">
								<xsl:call-template name="getTranslation">
									<xsl:with-param name="pResourceName" select="'ForwardRate'"/>
								</xsl:call-template>
							</fo:block>
						</fo:table-cell>
						<fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
							<fo:block border="{$gVarBorder}">
								<xsl:call-template name="GetExchangeRate">
									<xsl:with-param name="pFxSingleLeg" select="$pFxSingleLeg"/>
								</xsl:call-template>
							</fo:block>
						</fo:table-cell>
					</fo:table-row>
					-->
          <!-- 
					**************************************************************
					Add new condition
					For FpML FxSpot and FxForward are the same element: FxSingleLeg

					XML extract from FxSpot:
					 <exchangeRate>
						<quotedCurrencyPair>
						  <currency1>GBP</currency1>
						  <currency2>USD</currency2>
						  <quoteBasis>Currency2PerCurrency1</quoteBasis>
						</quotedCurrencyPair>
						<rate>1.48</rate>
					  </exchangeRate>
					  
					 XML extract from FxForward:
					  <exchangeRate>
						<quotedCurrencyPair>
						  <currency1>EUR</currency1>
						  <currency2>USD</currency2>
						  <quoteBasis>Currency2PerCurrency1</quoteBasis>
						</quotedCurrencyPair>
						<rate>0.9175</rate>
						<spotRate>0.9130</spotRate>
						<forwardPoints>0.0045</forwardPoints>
					  </exchangeRate>
					  
					(From PL mail)
					En FpML aucune distinction
					Si SpotRate est renseigné et s’il est différent de Rate 
					 - Utiliser le terme Forward rate
					Sinon 
					 - Utiliser le terme Exchange rate
					**************************************************************
					-->
          <xsl:if test ="normalize-space($pFxSingleLeg/exchangeRate/rate)">
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:choose>
                    <xsl:when test ="normalize-space($pFxSingleLeg/exchangeRate/spotRate) and $pFxSingleLeg/exchangeRate/spotRate != $pFxSingleLeg/exchangeRate/rate">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'ForwardRate'"/>
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'ExchangeRate'"/>
                      </xsl:call-template>
                    </xsl:otherwise>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="GetExchangeRate">
                    <xsl:with-param name="pFxSingleLeg" select="$pFxSingleLeg"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
          </xsl:if>

          <xsl:variable name="ReferenceCurrencyBuyer">
            <xsl:if test="$pFxSingleLeg/exchangedCurrency1/paymentAmount/currency=$ReferenceCurrency" >
              <xsl:value-of select="$pFxSingleLeg/exchangedCurrency1/receiverPartyReference/@href" />
            </xsl:if>
            <xsl:if test="$pFxSingleLeg/exchangedCurrency2/paymentAmount/currency=$ReferenceCurrency" >
              <xsl:value-of select="$pFxSingleLeg/exchangedCurrency2/receiverPartyReference/@href" />
            </xsl:if>
          </xsl:variable>

          <fo:table-row>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'ReferenceCurrencyBuyer'"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="getPartyNameAndID" >
                  <xsl:with-param name="pParty" select="$ReferenceCurrencyBuyer" />
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>

          <xsl:variable name="ReferenceCurrencySeller">
            <xsl:if test="$pFxSingleLeg/exchangedCurrency1/paymentAmount/currency=$ReferenceCurrency" >
              <xsl:value-of select="$pFxSingleLeg/exchangedCurrency1/payerPartyReference/@href" />
            </xsl:if>
            <xsl:if test="$pFxSingleLeg/exchangedCurrency2/paymentAmount/currency=$ReferenceCurrency" >
              <xsl:value-of select="$pFxSingleLeg/exchangedCurrency2/payerPartyReference/@href" />
            </xsl:if>
          </xsl:variable>

          <fo:table-row>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'ReferenceCurrencySeller'"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="getPartyNameAndID" >
                  <xsl:with-param name="pParty" select="$ReferenceCurrencySeller" />
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
          <fo:table-row>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'SettlementCurrency'"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:value-of select="$pFxSingleLeg/nonDeliverableForward/settlementCurrency" />
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
          <fo:table-row>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'SettlementDate'"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="format-date">
                  <xsl:with-param name="xsd-date-time" select="$pFxSingleLeg/valueDate"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
          <fo:table-row>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'Settlement'"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'NonDeliverable'"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>

          <xsl:if test ="$pFxSingleLeg/nonDeliverableForward/fixing/fixingDate= true()">
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'FixingDates'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="GetFxFixingDate">
                    <xsl:with-param name="pFxSingleLeg" select="$pFxSingleLeg"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'FixingTime'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:value-of  select ="$pFxSingleLeg/nonDeliverableForward/fixing/fixingTime/hourMinuteTime"/>
                  <xsl:value-of select="' ('"/>
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'localTimeIn_'"/>
                  </xsl:call-template>
                  <xsl:call-template name="getFullNameBC">
                    <xsl:with-param name="pBC" select="$pFxSingleLeg/nonDeliverableForward/fixing/fixingTime/businessCenter" />
                  </xsl:call-template>
                  <xsl:value-of select="')'"/>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
          </xsl:if>
        </fo:table-body>
      </fo:table>
    </fo:block>
  </xsl:template>
  <!-- =================================================================================================	-->
  <!-- END  of Region Display delivery information USED BY FX PRODUCTS: FxSwap, 	-->
  <!-- =================================================================================================	-->

  <!-- =================================================================================================	-->
  <!-- BEGIN  of Region Display Exchanged Currency USED BY FX PRODUCTS: FxSwap, 	-->
  <!-- =================================================================================================	-->
  <xsl:template name="displayExchangedCurrency">
    <xsl:param name="pFxSingleLeg"/>
    <!-- 
			varFxSingleLegRoundpanelTitle is only for FxSwap
			It displays the right title depending on the leg we are:
				- the first fxSingleLeg is always the spot rate
				- the second fxSingleLeg is always the forward rate
		-->
    <xsl:variable name="varFxSingleLegRoundpanelTitle">
      <xsl:choose>
        <xsl:when test="$varIsFxSwap=true() and position() = 1">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'SpotAmounts'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$varIsFxSwap=true() and position() = last()">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'ForwardAmounts'"/>
          </xsl:call-template>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="varCU1">
      <xsl:value-of select="$pFxSingleLeg/exchangedCurrency1/paymentAmount/currency"/>
    </xsl:variable>
    <!--<xsl:variable name="varIsBullion">
      <xsl:choose>
        <xsl:when test ="$varCU1='XAU' or $varCU1='XAG' or $varCU1='XPT' or $varCU1='XPD'">
          <xsl:value-of select="true()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="false()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>-->
    <xsl:variable name="varIsBullion" select="$varCU1='XAU' or $varCU1='XAG' or $varCU1='XPT' or $varCU1='XPD'" />

    <xsl:if test ="$varIsFxSwap=true()">
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$varFxSingleLegRoundpanelTitle"/>
        </xsl:call-template>
      </fo:block>
    </xsl:if>

    <fo:block border="{$gVarBorder}" margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
      <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
        <xsl:call-template name="createColumns"/>
        <fo:table-body>
          <!-- PL 20150323 TBD -->
          <xsl:if test ="$varIsBullion=true()">
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'BullionPurchaser'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getPartyNameAndID" >
                    <xsl:with-param name="pParty" select="$pFxSingleLeg/exchangedCurrency1/receiverPartyReference/@href" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'BullionSeller'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getPartyNameAndID" >
                    <xsl:with-param name="pParty" select="$pFxSingleLeg/exchangedCurrency1/payerPartyReference/@href" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Bullion'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:choose>
                    <xsl:when test="$pFxSingleLeg/exchangedCurrency1/paymentAmount/currency = 'XAU'">
                      <xsl:value-of select="'Gold  (XAU)'"/>
                    </xsl:when>
                    <xsl:when test="$pFxSingleLeg/exchangedCurrency1/paymentAmount/currency = 'XAG'">
                      <xsl:value-of select="'Silver  (XAG)'"/>
                    </xsl:when>
                    <xsl:when test="$pFxSingleLeg/exchangedCurrency1/paymentAmount/currency = 'XPT'">
                      <xsl:value-of select="'Platinum  (XPT)'"/>
                    </xsl:when>
                    <xsl:when test="$pFxSingleLeg/exchangedCurrency1/paymentAmount/currency = 'XPD'">
                      <xsl:value-of select="'Palladium  (XPD)'"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="$pFxSingleLeg/exchangedCurrency1/paymentAmount/currency"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'OuncesNumber'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:value-of select="format-number($pFxSingleLeg/exchangedCurrency1/paymentAmount/amount, $integerPattern, $defaultCulture)" />
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'ContractPrice'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:value-of select="$pFxSingleLeg/exchangedCurrency2/paymentAmount/currency" />
                  <xsl:text> </xsl:text>
                  <xsl:call-template name="GetExchangeRate">
                    <xsl:with-param name="pFxSingleLeg" select="$pFxSingleLeg"/>
                  </xsl:call-template>
                  <xsl:text>/oz</xsl:text>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'ValueDate'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="$pFxSingleLeg/valueDate"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Settlement'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'SettlementByDelivery'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
          </xsl:if>

          <xsl:if test ="$varIsBullion=false()">
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'AmountAndCurrencyPayableBy_'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getPartyNameAndID" >
                    <xsl:with-param name="pParty" select="$pFxSingleLeg/exchangedCurrency1/payerPartyReference/@href" />
                  </xsl:call-template>
                  <xsl:value-of select="': '"/>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:value-of select="$pFxSingleLeg/exchangedCurrency1/paymentAmount/currency" />
                  <xsl:text> </xsl:text>
                  <xsl:value-of select="format-number($pFxSingleLeg/exchangedCurrency1/paymentAmount/amount, $amountPattern, $defaultCulture)" />
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'AmountAndCurrencyPayableBy_'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getPartyNameAndID" >
                    <xsl:with-param name="pParty" select="$pFxSingleLeg/exchangedCurrency2/payerPartyReference/@href" />
                  </xsl:call-template>
                  <xsl:value-of select="': '"/>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:value-of select="$pFxSingleLeg/exchangedCurrency2/paymentAmount/currency" />
                  <xsl:text> </xsl:text>
                  <xsl:value-of select="format-number($pFxSingleLeg/exchangedCurrency2/paymentAmount/amount, $amountPattern, $defaultCulture)" />
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- If fxSwap and first leg -->
            <xsl:if test ="position() = 1 and $varIsFxSwap=true()">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'SpotRate'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="GetExchangeRate">
                      <xsl:with-param name="pFxSingleLeg" select="$pFxSingleLeg"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>
            <!-- If fxSwap and second leg -->
            <xsl:if test ="position() = 2 and $varIsFxSwap=true()">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'ForwardRate'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="GetExchangeRate">
                      <xsl:with-param name="pFxSingleLeg" select="$pFxSingleLeg"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>
            <!-- If not fxSwap -->
            <xsl:if test ="$varIsFxSwap=false()">
              <xsl:if test ="normalize-space($pFxSingleLeg/exchangeRate/spotRate)">
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'SpotRate'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="GetSpotRate">
                        <xsl:with-param name="pFxSingleLeg" select="$pFxSingleLeg"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:if>
              <!-- 
						  **************************************************************
						  Add new condition
						  For FpML FxSpot and FxForward are the same element: FxSingleLeg

						  XML extract from FxSpot:
						   <exchangeRate>
							  <quotedCurrencyPair>
							    <currency1>GBP</currency1>
							    <currency2>USD</currency2>
							    <quoteBasis>Currency2PerCurrency1</quoteBasis>
							  </quotedCurrencyPair>
							  <rate>1.48</rate>
						    </exchangeRate>
  						  
						   XML extract from FxForward:
						    <exchangeRate>
							  <quotedCurrencyPair>
							    <currency1>EUR</currency1>
							    <currency2>USD</currency2>
							    <quoteBasis>Currency2PerCurrency1</quoteBasis>
							  </quotedCurrencyPair>
							  <rate>0.9175</rate>
							  <spotRate>0.9130</spotRate>
							  <forwardPoints>0.0045</forwardPoints>
						    </exchangeRate>
  						  
						  (From PL mail)
						  En FpML aucune distinction
						  Si SpotRate est renseigné et s’il est différent de Rate 
						   - Utiliser le terme Forward rate
						  Sinon 
						   - Utiliser le terme Exchange rate
						  **************************************************************
						  -->
              <xsl:if test ="normalize-space($pFxSingleLeg/exchangeRate/rate)">
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:choose>
                        <xsl:when test ="normalize-space($pFxSingleLeg/exchangeRate/spotRate) and $pFxSingleLeg/exchangeRate/spotRate != $pFxSingleLeg/exchangeRate/rate">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'ForwardRate'"/>
                          </xsl:call-template>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'ExchangeRate'"/>
                          </xsl:call-template>
                        </xsl:otherwise>
                      </xsl:choose>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="GetExchangeRate">
                        <xsl:with-param name="pFxSingleLeg" select="$pFxSingleLeg"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:if>

              <!--
						  <xsl:if test ="normalize-space($pFxSingleLeg/exchangeRate/spotRate)">
							  <fo:table-row>
								  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
									  <fo:block border="{$gVarBorder}">
										  <xsl:call-template name="getTranslation">
											  <xsl:with-param name="pResourceName" select="'SpotRate'"/>
										  </xsl:call-template>
									  </fo:block>
								  </fo:table-cell>
								  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
									  <fo:block border="{$gVarBorder}">
										  <xsl:call-template name="GetSpotRate">
											  <xsl:with-param name="pFxSingleLeg" select="$pFxSingleLeg"/>
										  </xsl:call-template>
									  </fo:block>
								  </fo:table-cell>
							  </fo:table-row>
						  </xsl:if>
						  <xsl:if test ="normalize-space($pFxSingleLeg/exchangeRate/rate)">
							  <fo:table-row>
								  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
									  <fo:block border="{$gVarBorder}">
										  <xsl:call-template name="getTranslation">
											  <xsl:with-param name="pResourceName" select="'ForwardRate'"/>
										  </xsl:call-template>
									  </fo:block>
								  </fo:table-cell>
								  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
									  <fo:block border="{$gVarBorder}">
										  <xsl:call-template name="GetExchangeRate">
											  <xsl:with-param name="pFxSingleLeg" select="$pFxSingleLeg"/>
										  </xsl:call-template>
									  </fo:block>
								  </fo:table-cell>
							  </fo:table-row>
						  </xsl:if>
						  -->
            </xsl:if>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'SettlementDate'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="$pFxSingleLeg/valueDate"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
          </xsl:if>
        </fo:table-body>
      </fo:table>
    </fo:block>
  </xsl:template>

  <!-- =================================================================================================	-->
  <!-- END  of Region Display Exchanged Currency USED BY FX PRODUCTS: FxSwap, 	-->
  <!-- =================================================================================================	-->

  <!-- =================================================================================================	-->
  <!-- BEGIN  of Region Display Exchanged Rate: not used 	-->
  <!-- =================================================================================================	-->
  <!-- template DisplayExchangedRate -->
  <xsl:template name="displayExchangedRate">
    <xsl:param name="pFxSingleLeg"/>
    <fo:table-row>
      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
        <fo:block border="{$gVarBorder}">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'Rate'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
        <fo:block border="{$gVarBorder}">
          <xsl:choose>
            <xsl:when test="//trade/fxSingleLeg/exchangeRate/quotedCurrencyPair/quoteBasis='Currency2PerCurrency1'">
              <xsl:value-of select="//trade/fxSingleLeg/exchangeRate/quotedCurrencyPair/currency2" />
              <xsl:value-of select="'/'"/>
              <xsl:value-of select="//trade/fxSingleLeg/exchangeRate/quotedCurrencyPair/currency1" />
            </xsl:when>
            <xsl:when test="//trade/fxSingleLeg/exchangeRate/quotedCurrencyPair/quoteBasis='Currency1PerCurrency2'">
              <xsl:value-of select="//trade/fxSingleLeg/exchangeRate/quotedCurrencyPair/currency1" />
              <xsl:value-of select="'/'"/>
              <xsl:value-of select="//trade/fxSingleLeg/exchangeRate/quotedCurrencyPair/currency2" />
            </xsl:when>
            <xsl:otherwise>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:value-of select="$gVarSpace"/>
          <xsl:value-of select="format-number(//trade/fxSingleLeg/exchangeRate/rate, $amountPattern, $defaultCulture)" />
        </fo:block>
      </fo:table-cell>
    </fo:table-row>
  </xsl:template>
  <!-- =================================================================================================	-->
  <!-- END  of Region Display Exchanged Rate 	-->
  <!-- =================================================================================================	-->

  <!-- =================================================================================================	-->
  <!-- BEGIN  of Region Display BarrierLevel  USED BY FX PRODUCTS: FXBarrierOption 	-->
  <!-- =================================================================================================	-->
  <!-- Template Display Barrier Level -->
  <xsl:template name="displayBarrierLevel">
    <xsl:param name="pFxOption"/>
    <xsl:param name="pTotalBarrier"/>
    <!-- ************************************************************* -->
    <!-- Traitement du cas ou il existe qu'une seule Barrière          -->
    <!-- ************************************************************* -->
    <xsl:if test="$pTotalBarrier &lt; 2">
      <fo:table-row>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'BarrierLevel'"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <!-- Affichage du montant de la barrier-level -->
            <xsl:call-template name="format-fxrate">
              <xsl:with-param name="fxrate" select="$pFxOption/fxBarrier/triggerRate" />
            </xsl:call-template>
            <xsl:value-of select="$gVarSpace"/>
            <!-- Affichage du couple de la devise -->
            <xsl:call-template name="format-currency-pair">
              <xsl:with-param name="quotedCurrencyPair" select="$pFxOption/fxBarrier/quotedCurrencyPair" />
              <xsl:with-param name="tradeName" select="$pFxOption" />
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
      </fo:table-row>
    </xsl:if>
    <!-- ************************************************************* -->
    <!-- Traitement du cas ou il existe plusieurs niveaux de Barrières -->
    <!-- ************************************************************* -->
    <xsl:if test="$pTotalBarrier &gt; 1">
      <xsl:for-each select="$pFxOption/fxBarrier">
        <xsl:sort select="current()/triggerRate" order="descending" />
        <!-- Affichage de la Barrière la plus haute -->
        <xsl:if test="position() = 1 ">
          <fo:table-row>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'UpperBarrierLevel'"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <!-- Affichage du montant de la barrier-level -->
                <xsl:call-template name="format-fxrate">
                  <xsl:with-param name="fxrate" select="current()/triggerRate" />
                </xsl:call-template>
                <xsl:value-of select="$gVarSpace"/>
                <!-- Affichage du couple de la devise -->
                <xsl:call-template name="format-currency-pair">
                  <xsl:with-param name="quotedCurrencyPair" select="current()/quotedCurrencyPair" />
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </xsl:if>
        <!-- Affichage de la Barrière la plus basse -->
        <xsl:if test="position() = last() ">
          <fo:table-row>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'LowerBarrierLevel'"/>
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}">
                <!-- Affichage du montant de la barrier-level -->
                <xsl:call-template name="format-fxrate">
                  <xsl:with-param name="fxrate" select="current()/triggerRate" />
                </xsl:call-template>
                <xsl:value-of select="$gVarSpace"/>
                <!-- Affichage du couple de la devise -->
                <xsl:call-template name="format-currency-pair">
                  <xsl:with-param name="quotedCurrencyPair" select="current()/quotedCurrencyPair" />
                </xsl:call-template>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </xsl:if>
      </xsl:for-each>
    </xsl:if>
  </xsl:template>
  <!-- =================================================================================================	-->
  <!-- END of Region Display BarrierLevel  USED BY FX PRODUCTS: FXBarrierOption 	-->
  <!-- =================================================================================================	-->

  <!-- =================================================================================================	-->
  <!-- BEGIN  of Region Display Additional Definition  USED BY FX PRODUCTS: FXDigitalOption 	-->
  <!-- =================================================================================================	-->
  <xsl:template name="displayAdditionalDefinitions">
    <fo:block border="{$gVarBorder}" margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
      <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
        <fo:table-body>
          <fo:table-row>
            <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
              <fo:block border="{$gVarBorder}" text-align="justify" margin-left="{$varConfirmationTextLeftMarginCM}cm" margin-top="{$varConfirmationTextTopMarginCM}cm" font-size="{$varConfirmationTextFontSize}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'AdditionalDefinitions'"/>
                  </xsl:call-template>
                </fo:block>
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'EventPeriodMeans'"/>
                  </xsl:call-template>
                </fo:block>
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'BinaryEventMeans'"/>
                  </xsl:call-template>
                </fo:block>
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'SpotExchangeMeans'"/>
                  </xsl:call-template>
                </fo:block>
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'SpotMarketMeans'"/>
                  </xsl:call-template>
                </fo:block>
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'NotificationOfEvent'"/>
                  </xsl:call-template>
                </fo:block>
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Settlement:IfBinaryEventHasOccurred'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </fo:block>
  </xsl:template>
  <!-- =================================================================================================	-->
  <!-- END  of Region Display Additional Definition  USED BY FX PRODUCTS: FXDigitalOption 	-->
  <!-- =================================================================================================	-->

  <!-- 
	===============================================
	===============================================
	== FX products: END REGION     =============
	===============================================
	===============================================
	-->

  <!-- 
	===============================================
	===============================================
	== EQD products: BEGIN REGION     =============
	===============================================
	===============================================
	-->
  <!-- =========================================================================== -->
  <!-- Begin region Header Product Text                                            -->
  <!-- Used by all IRD products, it displays the text of the letter for IRD        -->
  <!-- =========================================================================== -->
  <!-- &#xA; linefeed-treatment="preserve" -->
  <xsl:template name="displayEqdConfirmationText">
    <xsl:variable name="varDatedAsOf">
      <xsl:if test="$isValidMasterAgreementDate">
        <xsl:call-template name="format-date">
          <xsl:with-param name="xsd-date-time" select="$masterAgreementDtSignature"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>

    <fo:block border="{$gVarBorder}" text-align="justify" margin-left="{$varConfirmationTextLeftMarginCM}cm" margin-top="{$varConfirmationTextTopMarginCM}cm" font-size="{$varConfirmationTextFontSize}">
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'EQDConfText1'"/>
        </xsl:call-template>
      </fo:block>
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'EQDConfText2'"/>
        </xsl:call-template>
      </fo:block>
      <xsl:call-template name="displayBlockBreakline"/>
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'EQDConfText3'"/>
        </xsl:call-template>
      </fo:block>
      <xsl:call-template name="displayBlockBreakline"/>
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'EQDConfText4'"/>
        </xsl:call-template>
      </fo:block>
      <xsl:call-template name="displayBlockBreakline"/>
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'EQDConfText5'"/>
        </xsl:call-template>
      </fo:block>
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'EQDConfText6'"/>
        </xsl:call-template>
        <xsl:if test="$isValidMasterAgreementDate">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'EQDConfText7'"/>
          </xsl:call-template>
          <xsl:call-template name="format-date">
            <xsl:with-param name="xsd-date-time" select="$masterAgreementDtSignature"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'EQDConfText8'"/>
        </xsl:call-template>
        <xsl:value-of select="$varParty_1_name" />
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'EQDConfText9'"/>
        </xsl:call-template>
        <xsl:value-of select="$varParty_2_name" />.
      </fo:block>
      <xsl:call-template name="displayBlockBreakline"/>
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'EQDConfText10'"/>
        </xsl:call-template>
      </fo:block>
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayBlockBreakline"/>
      </fo:block>
    </fo:block>
  </xsl:template>

  <!-- ================================================================================== -->
  <!-- BEGIN of Region Display Terms in use by Equity Option                     			-->
  <!-- ================================================================================== -->
  <xsl:template name="displayEqdTerms">
    <xsl:param name="pEquityOption"/>

    <fo:block keep-together.within-page="always">

      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$varTerms"/>
        </xsl:call-template>
      </fo:block>

      <fo:block border="{$gVarBorder}" margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'TradeDate'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="//trade/tradeHeader/tradeDate"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'OptionStyle'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:if test="//dataDocument/trade/equityOption/equityExercise/equityAmericanExercise">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'American'"/>
                    </xsl:call-template>
                  </xsl:if>
                  <xsl:if test="//dataDocument/trade/equityOption/equityExercise/equityBermudaExercise">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'Bermuda'"/>
                    </xsl:call-template>
                  </xsl:if>
                  <xsl:if test="//dataDocument/trade/equityOption/equityExercise/equityEuropeanExercise">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'European'"/>
                    </xsl:call-template>
                  </xsl:if>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'OptionType'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:value-of select="//dataDocument/trade/equityOption/optionType"/>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Seller'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getPartyNameAndID" >
                    <xsl:with-param name="pParty" select="$pEquityOption/sellerPartyReference/@href" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Buyer'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getPartyNameAndID" >
                    <xsl:with-param name="pParty" select="$pEquityOption/buyerPartyReference/@href" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <xsl:if test="//dataDocument/trade/equityOption/underlyer/singleUnderlyer/equity">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'Shares'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:value-of select="//dataDocument/trade/equityOption/underlyer/singleUnderlyer/equity/description"/>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>
            <xsl:if test="//dataDocument/trade/equityOption/underlyer/singleUnderlyer/index">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'Index'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:value-of select="//dataDocument/trade/equityOption/underlyer/singleUnderlyer/index/description"/>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'NumberOfOptions'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:value-of select="//dataDocument/trade/equityOption/numberOfOptions"/>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <xsl:if test="//dataDocument/trade/equityOption/underlyer/singleUnderlyer/equity">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'OptionEntitlement'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:value-of select="//dataDocument/trade/equityOption/optionEntitlement"/>
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'_SharePerOption'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'StrikePrice'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getEquityStrike"/>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <xsl:call-template name="displayEqdPremium" >
              <xsl:with-param name="pPremium" select="//dataDocument/trade/equityOption/equityPremium"/>
            </xsl:call-template>

            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Exchange'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getFullNameMIC">
                    <xsl:with-param name="pMIC" select="//exchangeId"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>
  </xsl:template>
  <!-- ================================================================================== -->
  <!-- END of Region Display Terms in use by Equity Option            					-->
  <!-- ================================================================================== -->

  <!-- ================================================================================== -->
  <!-- BEGIN of Region Premium  in use by Equity Option       							-->
  <!-- ================================================================================== -->
  <!--Display Premium -->
  <xsl:template name="displayEqdPremium">
    <xsl:param name="pPremium" />

    <xsl:for-each select="$pPremium">
      <fo:table-row>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'Premium'"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:value-of select="paymentAmount/currency" />
            <xsl:text> </xsl:text>
            <xsl:value-of select="format-number(paymentAmount/amount, $amountPattern, $defaultCulture)" />
          </fo:block>
        </fo:table-cell>
      </fo:table-row>

      <fo:table-row>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'PremiumPaymentDate'"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
          <fo:block border="{$gVarBorder}">
            <xsl:call-template name="getPremiumPaymentDate"/>
          </fo:block>
        </fo:table-cell>
      </fo:table-row>
    </xsl:for-each>
  </xsl:template>
  <!-- ================================================================================== -->
  <!-- END of Region Premium  in use by Equity Option                                     -->
  <!-- ================================================================================== -->

  <!-- ================================================================================== -->
  <!-- BEGIN of Region display Procedure Exercise  USED BY EQD PRODUCTS:  Equity Option 	-->
  <!-- ================================================================================== -->
  <!-- display Procedure Exercise Template -->
  <xsl:template name="displayEqdExerciseProcedure">
    <xsl:param name="pProduct" />

    <!-- define product variable-->
    <xsl:variable name="exercise">
      <xsl:if test="$pProduct/equityAmericanExercise">
        <xsl:value-of select="$pProduct/americanExercise"/>
      </xsl:if>
      <xsl:if test="$pProduct/europeanExercise">
        <xsl:value-of select="$pProduct/equityEuropeanExercise"/>
      </xsl:if>
      <xsl:if test="$pProduct/bermudaExercise">
        <xsl:value-of select="$pProduct/equityBermudaExercise"/>
      </xsl:if>
    </xsl:variable>

    <xsl:variable name ="varProceduresForExercise">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'ProceduresForExercise'"/>
      </xsl:call-template>
    </xsl:variable>

    <fo:block keep-together.within-page="always">

      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$varProceduresForExercise"/>
        </xsl:call-template>
      </fo:block>
      <fo:block margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>
            <!-- Procedure for Bermuda Exercise -->
            <xsl:if test="$pProduct/equityBermudaExercise">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'BermudaOptionExerciseDates'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getAdjustableOrRelativeDates" >
                      <xsl:with-param name="pDate" select="$pProduct/equityBermudaExercise/bermudaExerciseDates"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'EarliestExerciseTime'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:value-of select="//equityBermudaExercise/earliestExerciseTime/hourMinuteTime"/>
                    <xsl:value-of select="$gVarSpace"/>
                    <!-- commented section. Now the gotten data is the Full Name of Business Center -->
                    <xsl:call-template name="getFullNameBC">
                      <xsl:with-param name="pBC" select="//equityBermudaExercise/earliestExerciseTime/businessCenter"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>

              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'LatestExerciseTime'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:value-of select="//equityBermudaExercise/latestExerciseTime/hourMinuteTime"/>
                    <xsl:value-of select="$gVarSpace"/>
                    <!-- commented section. Now the gotten data is the Full Name of Business Center -->
                    <xsl:call-template name="getFullNameBC">
                      <xsl:with-param name="pBC" select="//equityBermudaExercise/latestExerciseTime/businessCenter"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>

            <!--Procedure for American Exercise: to testing with an american swaption xml  -->
            <xsl:if test="$pProduct/equityAmericanExercise">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'CommencementDate'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getAdjustableOrRelativeDateSequence" >
                      <xsl:with-param name="pDate" select="$pProduct/equityAmericanExercise/commencementDate"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'ExpirationDate'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getAdjustableOrRelativeDateSequence" >
                      <xsl:with-param name="pDate" select="$pProduct/equityAmericanExercise/expirationDate"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'ExpirationTime'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:value-of select="//equityAmericanExercise/equityExpirationTime/hourMinuteTime"/>
                    <xsl:value-of select="$gVarSpace"/>
                    <!-- commented section. Now the gotten data is the Full Name of Business Center -->
                    <xsl:call-template name="getFullNameBC">
                      <xsl:with-param name="pBC" select="//equityAmericanExercise/equityExpirationTime/businessCenter"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>

            <!--Procedure for European Exercise -->
            <xsl:if test="$pProduct/equityEuropeanExercise">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'ExpirationDate'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getAdjustableOrRelativeDateSequence" >
                      <xsl:with-param name="pDate" select="$pProduct/equityEuropeanExercise/expirationDate"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'ExpirationTime'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:value-of select="//equityEuropeanExercise/equityExpirationTime/hourMinuteTime"/>
                    <xsl:value-of select="$gVarSpace"/>
                    <!-- commented section. Now the gotten data is the Full Name of Business Center -->
                    <xsl:call-template name="getFullNameBC">
                      <xsl:with-param name="pBC" select="//equityEuropeanExercise/equityExpirationTime/businessCenter"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>
          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>
  </xsl:template>
  <!-- ================================================================================== -->
  <!-- END of Region Procedure Exercise   USED BY EQD PRODUCTS:  Equity Option    		-->
  <!-- ================================================================================== -->

  <!-- ================================================================================== -->
  <!-- BEGIN of Region Settlement Terms  USED BY EQD PRODUCTS:  Equity Option				-->
  <!-- ================================================================================== -->
  <!-- display SettlementTerms   -->
  <xsl:template name="displayEqdSettlementTerms">

    <fo:block keep-together.within-page="always">

      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$varSettlementTerms"/>
        </xsl:call-template>
      </fo:block>
      <fo:block margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>
            <xsl:choose>
              <xsl:when test="//settlementType ='Cash'">
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'CashSettlement'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'Applicable'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>

                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'SettlementCurrency'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:value-of select="//equityExercise/settlementCurrency"/>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>

                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'SettlementPrice'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:value-of select="//equityExercise/settlementPriceSource"/>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>

                <!-- GS 20100106 Add condiction: It display nothing if settlement date is missing in XML flow -->
                <xsl:if test ="normalize-space(//equityExercise/settlementDate)">
                  <fo:table-row>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                        <xsl:call-template name="getTranslation">
                          <xsl:with-param name="pResourceName" select="'CashSettlementPaymentDate'"/>
                        </xsl:call-template>
                      </fo:block>
                    </fo:table-cell>
                    <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                      <fo:block border="{$gVarBorder}">
                        <xsl:value-of select="$getEqdCashSettlementPaymentDate"/>
                      </fo:block>
                    </fo:table-cell>
                  </fo:table-row>
                </xsl:if>
              </xsl:when>

              <xsl:when test="//settlementType ='Physical'">
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'PhysicalSettlement'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'Applicable'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>

                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'SettlementCurrency'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:value-of select="//equityExercise/settlementCurrency"/>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:when>
              <xsl:otherwise>
                <fo:table-row>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'Settlement'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                  <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                    <fo:block border="{$gVarBorder}">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'Error'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:otherwise>
            </xsl:choose>
          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>
  </xsl:template>
  <!-- ================================================================================== -->
  <!-- END of Region Settlement Terms  USED BY EQD PRODUCTS:  Equity Option				-->
  <!-- ================================================================================== -->

  <!-- ================================================================================== -->
  <!-- BEGIN of Region Statement General Terms  USED BY EQD STATEMENT:  Equity Option			-->
  <!-- ================================================================================== -->

  <!-- display StatementText   -->
  <xsl:template name="displayEqdStatementText">
    <fo:block border="{$gVarBorder}" text-align="justify" margin-left="{$varConfirmationTextLeftMarginCM}cm" margin-top="{$varConfirmationTextTopMarginCM}cm" font-size="{$varConfirmationTextFontSize}">
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'EQDStatementText1'"/>
        </xsl:call-template>
      </fo:block>
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayBlockBreakline"/>
      </fo:block>
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'EQDStatementText2'"/>
        </xsl:call-template>
      </fo:block>
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayBlockBreakline"/>
      </fo:block>
    </fo:block>
  </xsl:template>

  <!-- display GeneralTerms -->
  <xsl:template name="displayEqdStatementGeneralTerms">
    <xsl:param name="pEquityOption"/>

    <!-- variables declaration (BEGIN) -->
    <xsl:variable name ="varGeneralTerms">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'GeneralTerms'"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name ="varReceiverAccout">
      <xsl:value-of select="//header/sendTo/@href"/>
    </xsl:variable>
    <!-- variables declaration (END) -->

    <fo:block keep-together.within-page="always">

      <!-- Display General Terms: Title -->
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$varGeneralTerms"/>
        </xsl:call-template>
      </fo:block>

      <fo:block border="{$gVarBorder}" margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>

            <!-- Display General Terms: Transaction Type -->
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'TransactionType'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <!-- Display Value Buy or Sell -->
                  <xsl:variable name ="varBuySell">
                    <xsl:value-of select="//trade/equitySecurityTransaction/FIXML/TrdCaptRpt/RptSide/@Side"/>
                  </xsl:variable>
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="concat('Side',$varBuySell)"/>
                  </xsl:call-template>
                  <xsl:value-of select="$gVarSpace"/>
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'TransactionTypeValue'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- Display General Terms: Transaction Reference -->
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'TransactionReference'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:value-of select="//trade/@tradeId"/>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- Display General Terms: Execution date and time -->
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'ExecutionDateTime'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="//trade/tradeHeader/tradeDate"/>
                  </xsl:call-template>
                  <xsl:value-of select="$gVarSpace"/>
                  <xsl:call-template name="format-time">
                    <xsl:with-param name="xsd-date-time" select="//trade/tradeHeader/tradeDate/@timeStamp"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- Display General Terms: Account -->
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Account'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:for-each select="//trade/tradeHeader/partyTradeIdentifier">
                    <xsl:if test="./partyReference/@href=$varReceiverAccout">
                      <xsl:value-of select="./bookId" />
                    </xsl:if>
                  </xsl:for-each>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- Display General Terms: Shares -->
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Shares'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:value-of select="substring-before(//repository/equity/displayname,'-')"/>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- Display General Terms: Number of Shares -->
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'NumberOfShares'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:value-of select="format-number(//trade/equitySecurityTransaction/FIXML/TrdCaptRpt/@LastQty, $integerPattern, $defaultCulture)"/>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- Display General Terms: Price -->
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Price'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:value-of select="//trade/equitySecurityTransaction/FIXML/TrdCaptRpt/@Ccy"/>
                  <xsl:value-of select="$gVarSpace"/>
                  <xsl:value-of select="format-number(//trade/equitySecurityTransaction/FIXML/TrdCaptRpt/@LastPx, $amountPattern, $defaultCulture)" />
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- Display General Terms: Exchange -->
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Exchange'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:value-of select="//repository/market/description"/>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>
  </xsl:template>

  <!-- Display SettlementTerms   -->
  <xsl:template name="displayEqdStatementSettlementTerms">
    <xsl:param name="pEquityOption"/>

    <!-- variables declaration (BEGIN) -->
    <xsl:variable name ="varSettlementTerms">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'SettlementTerms'"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name ="varGrossAmount">
      <xsl:choose>
        <xsl:when test ="//trade/equitySecurityTransaction/grossAmount/paymentAmount/amount">
          <xsl:value-of select="//trade/equitySecurityTransaction/grossAmount/paymentAmount/amount"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>0</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name ="varGrossAmountCurrency">
      <xsl:choose>
        <xsl:when test ="//trade/equitySecurityTransaction/grossAmount/paymentAmount/currency">
          <xsl:value-of select="//trade/equitySecurityTransaction/grossAmount/paymentAmount/currency"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text></xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name ="varCommissionAmount">
      <xsl:choose>
        <xsl:when test ="//trade/otherPartyPayment[paymentType = 'Commission']">
          <xsl:value-of select="//trade/otherPartyPayment/paymentAmount/amount"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>0</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name ="varCommissionAmountCurrency">
      <xsl:choose>
        <xsl:when test ="//trade/otherPartyPayment[paymentType = 'Commission']">
          <xsl:value-of select="//trade/otherPartyPayment/paymentAmount/currency"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text></xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name ="varTotalAmount">
      <xsl:value-of select="number($varGrossAmount)+number($varCommissionAmount)"/>
    </xsl:variable>
    <!-- variables declaration (END) -->

    <fo:block keep-together.within-page="always">

      <!-- Display Settlement Terms: Title -->
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$varSettlementTerms"/>
        </xsl:call-template>
      </fo:block>

      <fo:block border="{$gVarBorder}" margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>

            <!-- Display Settlement Terms: Settlement Date -->
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'SettlementDate'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="//trade/equitySecurityTransaction/grossAmount/paymentDate/unadjustedDate"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- Display Settlement Terms: Transaction Gross Amount -->
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'TransactionGrossAmount'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:value-of select="$varGrossAmountCurrency"/>
                  <xsl:value-of select="$gVarSpace"/>
                  <xsl:value-of select="format-number($varGrossAmount, $amountPattern, $defaultCulture)" />
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- Display Settlement Terms: Commissions -->
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Commissions'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:value-of select="$varCommissionAmountCurrency"/>
                  <xsl:value-of select="$gVarSpace"/>
                  <xsl:value-of select="format-number($varCommissionAmount, $amountPattern, $defaultCulture)" />
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- Display Settlement Terms : Transaction Total Amount -->
            <xsl:if test="$varGrossAmountCurrency = $varCommissionAmountCurrency">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'TransactionTotalAmount'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:value-of select="$varGrossAmountCurrency"/>
                    <xsl:value-of select="$gVarSpace"/>
                    <xsl:value-of select="format-number($varTotalAmount, $amountPattern, $defaultCulture)" />
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>

          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>
  </xsl:template>
  <!-- ================================================================================== -->
  <!-- END of Region General Terms  USED BY EQD STATEMENT:  Equity Option                 -->
  <!-- ================================================================================== -->

  <!-- 
	===============================================
	===============================================
	== EQD products: END REGION     =============
	===============================================
	===============================================
	-->

  <!-- ================================================================================== -->
  <!-- BEGIN of Region Statement (Account Details) Used By Statement:  Bond Option        -->
  <!-- ================================================================================== -->
  <xsl:template name="displayBondOptionStatementAccountDetails">
    <xsl:param name="pBondOption"/>

    <!-- variables declaration (BEGIN) -->
    <xsl:variable name ="varAccountDetails">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'AccountDetails'"/>
      </xsl:call-template>
    </xsl:variable>
    <!-- variables declaration (END) -->

    <fo:block keep-together.within-page="always">

      <!-- Display Account Details: Title -->
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$varAccountDetails"/>
        </xsl:call-template>
      </fo:block>

      <fo:block border="{$gVarBorder}" margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>

            <!-- Display Account Details: Account Details for Seller: -->
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'AccountsDetails'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>

              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Seller'"/>
                  </xsl:call-template>
                  <xsl:value-of select="$gVarSpace"/>
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'StandardSettlementInstructions'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- Display Account Details: Account Details for Buyer: -->
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:value-of select="$gVarSpace"/>
                </fo:block>
              </fo:table-cell>

              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Buyer'"/>
                  </xsl:call-template>
                  <xsl:value-of select="$gVarSpace"/>
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'StandardSettlementInstructions'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>
  </xsl:template>
  <!-- ================================================================================== -->
  <!-- END of Region Statement (Account Details) Used By Statement:  Bond Option  			  -->
  <!-- ================================================================================== -->

  <!-- ================================================================================== -->
  <!-- BEGIN of Region Statement (Offices) Used By Statement:  Bond Option                -->
  <!-- ================================================================================== -->
  <xsl:template name="displayBondOptionStatementOffices">
    <xsl:param name="pBondOption"/>

    <!-- variables declaration (BEGIN) -->
    <xsl:variable name ="varOffices">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'Offices'"/>
      </xsl:call-template>
      <xsl:value-of select="':'"/>
    </xsl:variable>
    <!-- variables declaration (END) -->

    <fo:block keep-together.within-page="always">

      <!-- Display Account Details: Title -->
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$varOffices"/>
        </xsl:call-template>
      </fo:block>

      <fo:block border="{$gVarBorder}" margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>

            <!-- (a) office -->
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  (a)
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'TheOfficeOf'"/>
                  </xsl:call-template>
                  <xsl:call-template name="getSendTo"/>
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'forTheTransactionIs'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>

              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0.5cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:value-of select="$sendToRoutingAddress"/>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- (b) office -->
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  (b)
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'TheOfficeOf'"/>
                  </xsl:call-template>
                  <xsl:call-template name="getSendBy"/>
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'forTheTransactionIs'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0.5cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:value-of select="$sendByRoutingAddress"/>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>

    <xsl:call-template name="displayBlockBreakline"/>
    <xsl:call-template name="displayBlockBreakline"/>

  </xsl:template>
  <!-- ================================================================================== -->
  <!-- END of Region Statement (Offices) Used By Statement:  Bond Option  	         		  -->
  <!-- ================================================================================== -->


</xsl:stylesheet>
