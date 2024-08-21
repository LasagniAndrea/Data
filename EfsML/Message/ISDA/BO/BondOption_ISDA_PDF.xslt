<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:dt="http://xsltsl.org/date-time" xmlns:fo="http://www.w3.org/1999/XSL/Format" version="1.0">

  <!--
	==============================================================
	Summary : Bond Option ISDA PDF
	==============================================================
  EG 20150519 : Complete Refactoring for ISDA Bond Option transaction confirmation
	==============================================================
	File    : BondOption_ISDA_PDF.xslt
	Date    : 22.04.2015
	Author  : Pony LA
	Version : 4.6.0.0
	Description: 
	==============================================================
	-->

  <xsl:import href="..\Shared\Shared_Variables.xslt"/>
  <xsl:import href="..\Shared\Shared_ISDA_Business.xslt"/>
  <xsl:import href="..\Shared\Shared_ISDA_PDF.xslt"/>
  <xsl:import href="BondOption_ISDA_Business.xslt"/>
  <xsl:import href="..\..\Custom\Custom_Message_HTML.xslt"/>

  <xsl:output method="xml" encoding="UTF-8"/>
  
  <!-- xslt includes -->
  <xsl:include href="..\..\..\Library\Resource.xslt"/>
  <xsl:include href="..\..\..\Library\DtFunc.xslt"/>
  <xsl:include href="..\..\..\Library\NbrFunc.xslt"/>
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

          <!-- Confirmation title (ex.: Confirmation of a Share Transaction) -->
          <xsl:call-template name="displayConfirmationTitle"/>
          <xsl:call-template name="displayBlockBreakline"/>

          <!-- Bank address (ex.: SOCIETE GENERALE 29 , BOULEVARD HAUSMANN....) -->
          <xsl:call-template name="displayReceiverAddress" />
          <xsl:call-template name="displayBlockBreakline"/>

          <!-- Display date (ex. The June 30, 2008) -->
          <xsl:call-template name="displayDate" />
          <xsl:call-template name="displayBlockBreakline"/>

          <!-- Display send by section (ex.: To:MLBANK From:SOCIETE GENERALE F/Ref.:123456) -->
          <xsl:call-template name="displaySendBy">
            <xsl:with-param name="pSubject" select="'Government Bond Option Transaction'" />            
          </xsl:call-template>
          <xsl:call-template name="displayBlockBreakline"/>

          <!-- Display confirmation text (ex.: Dear Sir or Madam...) -->
          <xsl:call-template name="displayBondOptionText"/>
          <xsl:call-template name="displayBlockBreakline"/>

          <!-- Call template to Display Statement General Terms (Trade Date, Option Style, etc.) -->
          <xsl:call-template name="displayBondOptionGeneralTerms">
            <xsl:with-param name="pProduct" select="//dataDocument/trade/bondOption" />
          </xsl:call-template>
          <xsl:call-template name="displayBlockBreakline"/>

          <!-- Call template Display Exercise and ExerciseProcedure  -->
          <xsl:call-template name="displayBondOptionExerciseProcedure">
            <xsl:with-param name="pProduct" select="//dataDocument/trade/bondOption"/>
          </xsl:call-template>

          <!-- Call template to Display Settlement Terms (Settlement, Settlement Date, SplitTickets, ...) -->
          <xsl:call-template name="displayBondOptionSettlementTerms">
            <xsl:with-param name="pProduct" select="//dataDocument/trade/bondOption" />
          </xsl:call-template>
          <xsl:call-template name="displayBlockBreaklines"/>

          <!-- Call template to Display Account Details and Offices -->
          <xsl:call-template name="displayFooter_Product" />
          <xsl:call-template name="displayBlockBreaklines">
            <xsl:with-param name="min" select="1"/>
            <xsl:with-param name="max" select="5"/>
          </xsl:call-template>

          <!-- Call template to Display text signature (ex. Please confirm that...) -->
          <xsl:call-template name="displaySignatureText"/>
          <xsl:call-template name="displayBlockBreaklines">
            <xsl:with-param name="min" select="1"/>
            <xsl:with-param name="max" select="5"/>
          </xsl:call-template>

          <!-- Call template to Sender and receiver signatures -->
          <xsl:call-template name="displaySenderReceiverSignatures"/>
          <xsl:call-template name="displayBlockBreakline"/>
          <fo:block id="EndOfDoc" />
        </fo:flow>
      </fo:page-sequence>
    </fo:root>
  </xsl:template>

  <!-- ================================================================================== -->
  <!-- BEGIN of Region Text                                                               -->
  <!-- ================================================================================== -->

  <xsl:template name="displayBondOptionText">

    <fo:block border="{$gVarBorder}" text-align="justify" margin-left="{$varConfirmationTextLeftMarginCM}cm" 
              margin-top="{$varConfirmationTextTopMarginCM}cm" font-size="{$varConfirmationTextFontSize}">

      <!-- Dear Sir or Madam, -->
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'BondOptionParagraph1'"/>
        </xsl:call-template>
      </fo:block>
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayBlockBreaklines"/>
      </fo:block>

      <!-- The purpose of this letter... -->
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'BondOptionParagraph2'"/>
        </xsl:call-template>
      </fo:block>
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayBlockBreaklines"/>
      </fo:block>

      <!-- display: The definitions and provisions... -->
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'BondOptionParagraph3'"/>
        </xsl:call-template>
      </fo:block>
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayBlockBreaklines"/>
      </fo:block>

      <!-- display: This Confirmation constitutes... -->
      <fo:block border="{$gVarBorder}">
        <!-- display: This Confirmation constitutes... -->
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'BondOptionParagraph4'"/>
        </xsl:call-template>

        <!-- Trade Date -->
        <xsl:call-template name="getTradeDate"/>

        <!-- display: , as amended and supplemented... -->
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'BondOptionParagraph4_1'"/>
        </xsl:call-template>

        <!-- PartyA -->
        <xsl:value-of select="//dataDocument/party[@id=//header/sendBy/@href]/partyName"/>

        <!-- display:  ("Party A") and ... -->
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'BondOptionParagraph4_2'"/>
        </xsl:call-template>

        <!-- PartyB -->
        <xsl:value-of select="//dataDocument/party[@id=//header/sendTo[1]/@href]/partyName"/>

        <!-- display: ("Party B"). All provisions... -->
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'BondOptionParagraph4_3'"/>
        </xsl:call-template>
      </fo:block>
      
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayBlockBreaklines"/>
      </fo:block>

      <!-- display: This Confirmation constitutes... -->
      <fo:block border="{$gVarBorder}">
        <!-- display: The terms of the Government Bond... -->
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'BondOptionParagraph5'"/>
        </xsl:call-template>
      </fo:block>
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayBlockBreaklines"/>
      </fo:block>

    </fo:block>

  </xsl:template>
  <!-- END of Region Text                                                                 -->


  <!-- ================================================================================== -->
  <!-- BEGIN of Region General Terms                                                      -->
  <!-- ================================================================================== -->

  <xsl:template name="displayBondOptionGeneralTerms">
    <xsl:param name="pProduct"/>

    <!-- variables declaration (BEGIN) -->
    <xsl:variable name ="varGeneralTerms">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'GeneralTerms'"/>
      </xsl:call-template>
    </xsl:variable>
    <!-- variables declaration (END) -->

    <fo:block keep-together.within-page="always">

      <!-- Title -->
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$varGeneralTerms"/>
        </xsl:call-template>
      </fo:block>

      <fo:block border="{$gVarBorder}" margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>

            <!-- TradeDate -->
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

            <!-- OptionStyle -->
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
                  <xsl:choose>
                    <xsl:when test="$pProduct/americanExercise">
                      <xsl:value-of select="'American'"/>
                    </xsl:when>
                    <xsl:when test="$pProduct/bermudaExercise">
                      <xsl:value-of select="'Bermuda'"/>
                    </xsl:when>
                    <xsl:when test="$pProduct/europeanExercise">
                      <xsl:value-of select="'European'"/>
                    </xsl:when>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- OptionType -->
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
                  <xsl:value-of select="$pProduct/optionType"/>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- Seller -->
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
                    <xsl:with-param name="pParty" select="$pProduct/sellerPartyReference/@href" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- Buyer -->
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
                    <xsl:with-param name="pParty" select="$pProduct/buyerPartyReference/@href" />
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <fo:table-row height="0.5cm"/>

            <!-- Bonds -->
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'Bonds'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:value-of select="'Identified as follows:'"/>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- Bonds Description: (begin) -->
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:value-of select="$gVarSpace"/>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell>
                <fo:table>

                  <fo:table-column column-width="2cm" column-number="01"></fo:table-column>
                  <fo:table-column column-width="9cm" column-number="02"></fo:table-column>

                  <fo:table-body>

                    <!-- DisplayName -->
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0.5cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'BondDisplayName'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:value-of select="$pProduct/securityAsset/securityName"/>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>

                    <!-- Issuer -->
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0.5cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'Issuer'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:variable name="varIssuerHref">
                            <xsl:value-of select="$pProduct/bond/issuerPartyReference/@href"/>
                          </xsl:variable>
                          <xsl:value-of select="//party[@id=$varIssuerHref]/partyName"/>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>

                    <!-- Isin -->
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0.5cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'IsinCode'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:value-of select="$pProduct/bond/instrumentId[contains(@instrumentIdScheme,'ISIN') ]"/>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>

                    <!-- CouponRate -->
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0.5cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'CouponRate'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:choose>
                            <xsl:when test="$pProduct/bond/couponType = 'Fixed'">
                              <xsl:value-of select="format-number( $pProduct/securityAsset/debtSecurity/debtSecurityStream/calculationPeriodAmount/calculation/fixedRateSchedule/initialValue * 100, $amountPattern, $defaultCulture)"/>%
                            </xsl:when>
                            <xsl:otherwise>
                              <xsl:value-of select="'TBD'"/>
                            </xsl:otherwise>
                          </xsl:choose>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>

                    <!-- Maturity -->
                    <fo:table-row>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0.5cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'Maturity'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                      <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                        <fo:block border="{$gVarBorder}">
                          <xsl:call-template name="format-date">
                            <xsl:with-param name="xsd-date-time" select="$pProduct/bond/maturity"/>
                          </xsl:call-template>
                          <xsl:call-template name="getTranslation">
                            <xsl:with-param name="pResourceName" select="'MaturityComplement'"/>
                          </xsl:call-template>
                        </fo:block>
                      </fo:table-cell>
                    </fo:table-row>

                  </fo:table-body>
                </fo:table>
              </fo:table-cell>
            </fo:table-row>
            <!-- Bonds Description: (end) -->

            <fo:table-row height="0.5cm"/>

            <!-- NumberOfOptions -->
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
                  <xsl:value-of select="$pProduct/numberOfOptions"/>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- OptionEntitlement -->
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
                  <xsl:value-of select="$pProduct/entitlementCurrency" />
                  <xsl:value-of select="$gVarSpace"/>
                  <xsl:value-of select="format-number($pProduct/optionEntitlement, $amountPattern, $defaultCulture)" />
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'OptionEntitlementDescription'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- StrikePrice -->
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
                  <xsl:choose>
                    <xsl:when test="$pProduct/strike/price/strikePercentage">
                      <xsl:call-template name="format-fixed-rate2">
                        <xsl:with-param name="fixed-rate" select="normalize-space($pProduct/strike/price/strikePercentage)" />
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="'TBD'"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- PremiumPricePerOption -->
            <xsl:if test="$pProduct/premium/pricePerOption">
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'PremiumPerOption'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:value-of select="$pProduct/premium/pricePerOption/currency" />
                    <xsl:value-of select="$gVarSpace"/>
                    <xsl:value-of select="format-number($pProduct/premium/pricePerOption/amount, $amountPattern, $defaultCulture)" />
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </xsl:if>

            <!-- PremiumPaymentAmount -->
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
                  <xsl:value-of select="$pProduct/premium/paymentAmount/currency" />
                  <xsl:value-of select="$gVarSpace"/>
                  <xsl:value-of select="format-number($pProduct/premium/paymentAmount/amount, $amountPattern, $defaultCulture)" />
                  <xsl:if test="$pProduct/premium/percentageOfNotional">
                    <xsl:text> (</xsl:text>
                    <xsl:call-template name="format-fixed-rate2">
                      <xsl:with-param name="fixed-rate" select="normalize-space($pProduct/premium/percentageOfNotional)" />
                    </xsl:call-template>
                    <xsl:text> )</xsl:text>
                  </xsl:if>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- PremiumPaymentDate (with BusinessDayConvention if != NONE) -->
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
                  <xsl:call-template name="getAdjustableOrRelativeDate">
                    <xsl:with-param name="pDate" select="$pProduct/premium/paymentDate"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- Business Days -->
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'BusinessDays'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getBCFullNames">
                    <xsl:with-param name="pBCs" select="$pProduct/securityAsset/debtSecurity/debtSecurityStream/paymentDates/paymentDatesAdjustments/businessCenters"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- SellerBusinessDay (TBD) -->
            <!-- fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'SellerBusinessDay'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">                                   
                </fo:block>
              </fo:table-cell>
            </fo:table-row -->

            <!-- ExchangeId -->
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
                    <xsl:with-param name="pMIC" select="$pProduct/bond/exchangeId"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- CalculationAgent -->
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
  <!-- END of Region General Terms                                                        -->


  <!-- ================================================================================== -->
  <!-- BEGIN of Region Exercise | ExerciseProcedure                                       -->
  <!-- ================================================================================== -->

  <xsl:template name="displayBondOptionExerciseProcedure">
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

    <xsl:variable name="varProcedureForExercise">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'ProcedureForExercise'"/>
      </xsl:call-template>
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
            <!-- Bermuda Exercise -->
            <xsl:if test="$pProduct/bermudaExercise">

              <!-- ExerciseDates -->
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

              <!-- LatestExerciseTime -->
              <xsl:if test="$pProduct/bermudaExercise/latestExerciseTime">
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
                      <xsl:call-template name="format-time-bc">
                        <xsl:with-param name="pTime" select="$pProduct/bermudaExercise/latestExerciseTime/hourMinuteTime"/>
                        <xsl:with-param name="pBusinessCenter" select="$pProduct/bermudaExercise/latestExerciseTime/businessCenter"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:if>

              <!-- MultipleExercise -->
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:if test="$pProduct/bermudaExercise/multipleExercise">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'MultipleExercise'"/>
                      </xsl:call-template>
                    </xsl:if>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:if test ="$pProduct/bermudaExercise/multipleExercise">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'Applicable'"/>
                      </xsl:call-template>
                    </xsl:if>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>

            </xsl:if>

            <!-- American Exercise -->
            <xsl:if test="$pProduct/americanExercise">


              <!-- CommencementDate -->
              <!-- Set Formated TradeDate -->
              <xsl:variable name="varTradeDate">
                <xsl:call-template name="getTradeDate"/>
              </xsl:variable>

              <!-- Set Formated CommencementDate -->
              <xsl:variable name="varCommencementDate">
                <xsl:call-template name="getAdjustableOrRelativeDateSequence" >
                  <xsl:with-param name="pDate" select="$pProduct/americanExercise/commencementDate"/>
                </xsl:call-template>
              </xsl:variable>

              <!-- Include CommencementDate is not the TradeDate -->
              <xsl:if test="$varTradeDate != $varCommencementDate">
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
              </xsl:if>

              <!-- ExpirationDate -->
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


              <!-- Set Formated ExpirationTime -->
              <xsl:variable name="varExpirationTime">
                <xsl:call-template name="format-time-bc">
                  <xsl:with-param name="pTime" select="$pProduct/americanExercise/expirationTime/hourMinuteTime"/>
                  <xsl:with-param name="pBusinessCenter" select="$pProduct/americanExercise/expirationTime/businessCenter"/>
                </xsl:call-template>
              </xsl:variable>

              <!-- LatestExerciseTime -->
              <xsl:if test="$pProduct/americanExercise/latestExerciseTime">

                <xsl:variable name="varLatestExerciseTime">
                  <xsl:call-template name="format-time-bc">
                    <xsl:with-param name="pTime" select="$pProduct/americanExercise/latestExerciseTime/hourMinuteTime"/>
                    <xsl:with-param name="pBusinessCenter" select="$pProduct/americanExercise/latestExerciseTime/businessCenter"/>
                  </xsl:call-template>
                </xsl:variable>

                <!-- Include if American style option and LatestExerciseTime is not the ExpirationTime  -->
                <xsl:if test="$varLatestExerciseTime != $varExpirationTime">
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
                        <xsl:value-of select="$varLatestExerciseTime"/>
                      </fo:block>
                    </fo:table-cell>
                  </fo:table-row>
                </xsl:if>
              </xsl:if>

              <!-- ExpirationTime -->
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
                    <xsl:value-of select="$varExpirationTime"/>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>

              <!-- MultipleExercise -->
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
                    <xsl:if test ="$pProduct/americanExercise/multipleExercise">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'Applicable'"/>
                      </xsl:call-template>
                    </xsl:if>
                    <xsl:if test="not($pProduct/americanExercise/multipleExercise)">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'Inapplicable'"/>
                      </xsl:call-template>
                    </xsl:if>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>

            </xsl:if>

            <!-- European Exercise -->
            <xsl:if test="$pProduct/europeanExercise">

              <!-- ExpirationDate -->
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

              <!-- ExpirationTime -->
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
                    <xsl:call-template name="format-time-bc">
                      <xsl:with-param name="pTime" select="$pProduct/europeanExercise/expirationTime/hourMinuteTime"/>
                      <xsl:with-param name="pBusinessCenter" select="$pProduct/europeanExercise/expirationTime/businessCenter"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>

              <!-- PartialExercise -->
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
                    <xsl:if test ="$pProduct/europeanExercise/partialExercise">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'Applicable'"/>
                      </xsl:call-template>
                    </xsl:if>
                    <xsl:if test="not($pProduct/europeanExercise/partialExercise)">
                      <xsl:call-template name="getTranslation">
                        <xsl:with-param name="pResourceName" select="'Inapplicable'"/>
                      </xsl:call-template>
                    </xsl:if>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>

            </xsl:if>

            <!-- Exercise Procedure  -->

            <!-- FollowUpConfirmation -->
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'WrittenConfirmationOfExercise'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <!-- Written notice of exercise is presumed to be required unless the parties specify otherwise -->
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:choose>
                    <xsl:when test="not($pProduct/exerciseProcedure) or $pProduct/exerciseProcedure/followUpConfirmation = 'true'">
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

            <!-- Limited Right to Confirm Exercise -->
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'LimitedRightToConfirmExercise'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:choose>
                    <xsl:when test="($pProduct/exerciseProcedure/limitedRightToConfirm = 'true')">
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

            <!-- Automatic Exercise -->
            <fo:table-row>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'AutomaticExercise'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:choose>
                    <xsl:when test="($pProduct/exerciseProcedure/automaticExercise)">
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

            <!--  Contact Details for Purpose of Giving -->
            <fo:table-row>
              <!-- Seller Info -->
              <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                <fo:block border="{$gVarBorder}">
                  <xsl:call-template name="getTranslation">
                    <xsl:with-param name="pResourceName" select="'ContactDetailsForPurposeOfGiving'"/>
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
                    <xsl:with-param name="pResourceName" select="'PleaseAdvise'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- Buyer Info -->
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
                    <xsl:with-param name="pResourceName" select="'ToBeAdvised'"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>
  </xsl:template>
  <!-- END of Region Exercise | ExerciseProcedure                                         -->


  <!-- ================================================================================== -->
  <!-- BEGIN of Region Settlement Terms                                                   -->
  <!-- ================================================================================== -->

  <xsl:template name="displayBondOptionSettlementTerms">
    <xsl:param name="pProduct"/>

    <!-- variables declaration (BEGIN) -->
    <xsl:variable name ="varSettlementTerms">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'SettlementTerms'"/>
      </xsl:call-template>
    </xsl:variable>
    <!-- variables declaration (END) -->

    <fo:block keep-together.within-page="always">

      <!-- Display General Terms: Title -->
      <fo:block border="{$gVarBorder}">
        <xsl:call-template name="displayTitle">
          <xsl:with-param name="pText" select="$varSettlementTerms"/>
        </xsl:call-template>
      </fo:block>

      <fo:block border="{$gVarBorder}" margin-left="{$varProductSectionTextLeftMarginCM}cm" margin-top="{$varProductSectionTextTopMarginCM}cm" font-size="{$varProductSectionTextFontSize}">
        <fo:table margin-left="0cm" border="{$gVarBorder}" width="{$varProductSectionTableWidthCM}cm" table-layout="fixed">
          <xsl:call-template name="createColumns"/>
          <fo:table-body>

            <!-- SettlementType -->
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
                  <xsl:value-of select="$pProduct/settlementType"/>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- SettlementDate -->
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
                  <xsl:call-template name="getAdjustableOrRelativeDate">
                    <xsl:with-param name="pDate" select="$pProduct/settlementDate"/>
                  </xsl:call-template>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>

            <!-- Physical Exercise only -->
            <xsl:if test="$pProduct/settlementType = 'Physical'">

              <!-- SplitTickets : Exclude if Cash Settlement is specified to be applicable -->
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'SplitTickets'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>

                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:choose>
                      <xsl:when test="($pProduct/exerciseProcedure/splitTicket = 'true')">
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

              <!-- ClearanceSystem : Exclude if Cash Settlement is specified to be applicable -->
              <fo:table-row>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:call-template name="getTranslation">
                      <xsl:with-param name="pResourceName" select="'ClearanceSystem'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding-top="{$varProductSectionTextPaddingTop}cm" margin-left="0cm" border="{$gVarBorder}">
                  <fo:block border="{$gVarBorder}">
                    <xsl:if test="$pProduct/securityAsset/debtSecurity/security/clearanceSystem">
                      <xsl:value-of select="$pProduct/securityAsset/debtSecurity/security/clearanceSystem"/>
                    </xsl:if>
                    <xsl:if test="not($pProduct/securityAsset/debtSecurity/security/clearanceSystem)">
                      <xsl:value-of select="' '"/>
                    </xsl:if>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>

            </xsl:if>

          </fo:table-body>
        </fo:table>
      </fo:block>
    </fo:block>
  </xsl:template>
  <!-- END of Region Settlement Terms                                                   -->



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
  <xsl:variable name="varA4LeftMarginCM">1.5</xsl:variable>
  <xsl:variable name="varA4RightMarginCM">1.5</xsl:variable>
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

</xsl:stylesheet>



