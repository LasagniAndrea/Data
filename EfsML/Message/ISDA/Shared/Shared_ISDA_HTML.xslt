<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:fo="http://www.w3.org/1999/XSL/Format"
xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" version="1.0">

  <!--
	==============================================================
	Summary : Shared ISDA HTML
	==============================================================
	File    : Shared_Isda_HTML.xslt
	Date    : 20.05.2009
	Author  : Gianmario SERIO
	Version : 2.3.0.3_1
	Description:
	==============================================================
	Revision:
	Date    :
	Author  :
	Version :
	Comment :
	==============================================================
	-->

  <!-- 
	===============================================
	===============================================
	== All products: BEGIN REGION     =============
	===============================================
	===============================================
	-->
  <!-- Display the image of the Logo -->
  <xsl:template name="displayLogo">
    <xsl:if test="$varLogo = true()">
      <table style="width: 780px;vertical-align:top;" cellpadding="0" cellspacing="0" align="center" border="0">
        <tr>
          <td>
            <img align="left" src="{$varLogo}" endelement="true"/>
          </td>
        </tr>
      </table>
    </xsl:if>
  </xsl:template>

  <!-- displayCSS: set the CSS for the web page -->
  <xsl:template name="displayCSS">
    <xsl:if test="$outputMethod ='html'">
      body {
      margin: 1px;
      padding:0px;
      font-family:Verdana;
      font-size:11pt;
      }
      td {font-size:9pt;}
      a {	text-decoration: underline }
      a:hover { color: #ff0000; background: #ffff00; text-decoration: underline; }
      thead {display: table-header-group;}
      tfoot {display: table-footer-group;}
      .label {text-indent:20px;width:330px;vertical-align:top;}
      .label2 {text-indent:40px;width:280px;vertical-align:top;}
      .label_nowidth {padding-left:20px; padding-right:10px;width:310px; vertical-align:top;}
      .label_nowidth_right{padding-right:20px; vertical-align:top;}
      .value {width:100%;}
      .underlinelabel {border-bottom: 1pt solid;}
      .puce {list-style-type:disc;text-align:left;}
      .pnlRounded {border: royalblue 1px solid;background-color: lavender;}
      .titleRounded {color:royalblue;font-size:11pt;font-weight:bold;}
      .roundcont {padding: 0px;margin: 4px 25px 4px 5px;background-color: Transparent;}
    </xsl:if>
    <!--
	xsl:if test="$outputMethod ='html'">
	thead {display: table-header-group;}
	tfoot {display: table-footer-group;}
	</xsl:if
	-->
  </xsl:template>

  <!-- Template that generates a BreakLine -->
  <xsl:template name="BreakLine">
    <xsl:text>&#xa;</xsl:text>
    <tr>
      <td>
        <br/>
      </td>
    </tr>
  </xsl:template>

  <!-- Debug  -->
  <xsl:template name="Debug">
    <xsl:param name="Comment" select="'Test'"/>
    <tr>
      <td>
        <span style="color:Red; font-weight:bold">
          DEBUG: <xsl:value-of select="$Comment"/>
        </span>
      </td>
    </tr>
  </xsl:template>

  <!-- Display the Title of the ISDA -->
  <xsl:template name="displayTitle">
    <xsl:text>&#xa;</xsl:text>
    <tr style="line-height:18pt" >
      <td align="center" style="FONT-WEIGHT:bold;FONT-SIZE:18;COLOR: DarkBlue;">
        <xsl:call-template name="getTitle"/>
      </td>
    </tr>
  </xsl:template>

  <!-- RoundedPanel -->
  <xsl:template name="RoundedPanel">
    <xsl:param name="pWidth" select="'100%'" />
    <xsl:param name="pNumberTitle" />
    <xsl:param name="pTitle" select="'Title'" />
    <span style="font-family: Verdana; color: royalblue; font-size: 11pt; font-weight: bold;" class="titleRounded">
      &#160;
      <xsl:if test="$pNumberTitle">
        <xsl:value-of select="$pNumberTitle"/>.&#160;
      </xsl:if>
      <xsl:value-of select="$pTitle"/>
      <xsl:text>&#xa;</xsl:text>
      <xsl:text>&#xa;</xsl:text>
    </span>
  </xsl:template>
  <!-- 
	==============================================
	== Header: BEGIN REGION 
	== contains (Address, date, To, From, Ref )
	==============================================
	-->
  <!-- Display the Header of the web page-->
  <xsl:template name="displayHeader">
    <xsl:param name="pTDCSS"/>
    <xsl:text>&#xa;</xsl:text>
    <tr>
      <td>
        <table cellpadding="0" cellspacing="0" align="left" width="100%" border="0">
          <tr>
            <td>&#160;</td>
            <td width="300">
              <table cellpadding="0" cellspacing="0" align="left" width="100%" border="0">
                <tr>
                  <td style="color:White;line-height:11pt">
                    <br />
                  </td>
                </tr>
                <tr>
                  <td style="{$pTDCSS}">
                    <xsl:value-of select="$address1" />
                  </td>
                </tr>
                <tr>
                  <td style="{$pTDCSS}">
                    <xsl:value-of select="$address2" />
                  </td>
                </tr>
                <tr>
                  <td style="{$pTDCSS}">
                    <xsl:value-of select="$address3" />
                  </td>
                </tr>
                <tr>
                  <td style="{$pTDCSS}">
                    <xsl:value-of select="$address4" />
                  </td>
                </tr>
                <tr>
                  <td style="{$pTDCSS}">
                    <xsl:value-of select="$address5" />
                  </td>
                </tr>
                <tr>
                  <td style="{$pTDCSS}">
                    <b>
                      <xsl:value-of select="$address6" />
                    </b>
                  </td>
                </tr>
                <tr>
                  <td style="color:White;line-height:11pt">
                    <br />
                  </td>
                </tr>
              </table>
            </td>
          </tr>
          <tr>
            <td colspan="2">
              <br />
            </td>
          </tr>
          <tr>
            <td width="60"></td>
            <td style="{$pTDCSS}" align="right">
              <xsl:call-template name="getSenderCity"/>
              <xsl:text>&#xa;</xsl:text>
              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="$varCreationTimestamp"/>
              </xsl:call-template>
              <!-- translation test
							<xsl:call-template name="getTranslation">
								<xsl:with-param name="pResourceName" select="'THE'"/>
							</xsl:call-template>
							<xsl:call-template name="format-date">
								<xsl:with-param name="xsd-date-time" select="$varCreationTimestamp"/>
							</xsl:call-template>
							-->
            </td>
          </tr>
          <tr>
            <td colspan="2">
              <br />
            </td>
          </tr>
          <tr>
            <xsl:text>&#xa;</xsl:text>
            <td>
              <table cellpadding="0" cellspacing="0" align="left" width="100%" border="0">
                <tr>
                  <td width="50%">
                    <table cellpadding="0" cellspacing="0" align="left" width="100%" border="0">
                      <tr>
                        <td style="{$pTDCSS}" width="60">To :</td>
                        <td style="{$pTDCSS}">
                          <xsl:call-template name="getSendTo" />
                        </td>
                      </tr>
                    </table>
                  </td>
                  <td></td>
                </tr>
              </table>
            </td>
            <td></td>
          </tr>
          <tr>
            <xsl:text>&#xa;</xsl:text>
            <td>
              <table cellpadding="0" cellspacing="0" align="left" width="100%" border="0">
                <tr>
                  <td width="50%">
                    <table cellpadding="0" cellspacing="0" align="left" width="100%" border="0">
                      <!-- It Display the To/Ref when: it exist, the string length is great of zero and its value is different from zero -->
                      <!-- case 1) when REF is equal (T/REF: 1070 not displayed) (F/Ref: 1070 displayed)-->
                      <!-- case 2) when REF is different and T/Ref is zero (T/REF: 0 not displayed) (F/Ref: 1070 displayed)-->
                      <!-- case 3) when REF is different and F/Ref is zero (T/REF: 1070 not displayed) (F/Ref: 0 displayed with the value of T/Ref)-->
                      <xsl:if test="($TradeIdTo != $TradeIdFrom) and (string-length($TradeIdTo) > 0) and ($TradeIdTo != 0) and ($TradeIdFrom != 0)">
                        <tr>
                          <td style="{$pTDCSS}" width="60">
                            <xsl:text>T/Ref. :</xsl:text>
                          </td>
                          <td style="{$pTDCSS}">
                            <xsl:call-template name="getTradeIdTo" />
                          </td>
                        </tr>
                      </xsl:if>
                    </table>
                  </td>
                  <td></td>
                </tr>
              </table>
            </td>
            <td></td>
          </tr>
          <tr>
            <xsl:text>&#xa;</xsl:text>
            <td>
              <table cellpadding="0" cellspacing="0" align="left" width="100%" border="0">
                <tr>
                  <td width="50%">
                    <table cellpadding="0" cellspacing="0" align="left" width="100%" border="0">
                      <tr>
                        <td style="{$pTDCSS}" width="60">From :</td>
                        <td style="{$pTDCSS}">
                          <xsl:call-template name="getSendBy" />
                        </td>
                      </tr>
                    </table>
                  </td>
                  <td></td>
                </tr>
              </table>
            </td>
            <td></td>
          </tr>
          <tr>
            <xsl:text>&#xa;</xsl:text>
            <td>
              <table cellpadding="0" cellspacing="0" align="left" width="100%" border="0">
                <tr>
                  <td width="50%">
                    <table cellpadding="0" cellspacing="0" align="left" width="100%" border="0">
                      <tr>
                        <td style="{$pTDCSS}" width="60">
                          <xsl:text>F/Ref. :</xsl:text>
                        </td>
                        <td style="{$pTDCSS}">
                          <!-- It Display the From/Ref in all cases. When the TradeIdFrom net exist or = '0' it use the value of TradeIdTo -->
                          <xsl:choose>
                            <xsl:when test="(string-length($TradeIdFrom) = 0) or ($TradeIdFrom = 0)">
                              <xsl:call-template name="getTradeIdTo" />
                            </xsl:when>
                            <xsl:otherwise>
                              <xsl:call-template name="getTradeIdFrom" />
                            </xsl:otherwise>
                          </xsl:choose>
                        </td>
                      </tr>
                    </table>
                  </td>
                  <td></td>
                </tr>
              </table>
            </td>
            <td></td>
          </tr>
        </table>
      </td>
    </tr>
  </xsl:template>
  <!-- 
	====================================
	== Header: END REGION   
	====================================
	-->
  <!-- 
	=====================================
	== Floating Amount: BEGIN REGION 
	=====================================
	-->
  <!-- Display the Floating Rate Calculation -->
  <!-- 
	contains (Floating Rate Payer, Floating Rate Payer Currency Amount, Floating Rate Payment Dates Frequency, Fixing Dates
	Reset Dates, Rate Cut-off Dates, Method of Averaging,Compounding)
	it calls the templates: (displayFloatingCalculation [Floating Rate Option, Designated Maturity, Spread, Floating Rate Day Count Fraction])
	-->
  <xsl:template name="displayFloatingRateCalculation">
    <xsl:param name="pStream"/>
    <xsl:param name="pIsCrossCurrency" select="false()"/>
    <xsl:param name="pIsCapOnly" select="false()"/>
    <xsl:param name="pIsFloorOnly" select="false()"/>
    <xsl:param name="pIsDisplayTitle" />
    <xsl:param name="pIsDisplayDetail" />
    <xsl:param name="pTDCSS"/>

    <xsl:if test="$pIsDisplayTitle=true()">
      <tr>
        <td valign="middle" align="left" bgcolor="lavender" colspan="2">
          <xsl:call-template name="RoundedPanel">
            <xsl:with-param name="pTitle" select="'Floating Amounts'"/>
          </xsl:call-template>
        </td>
      </tr>
    </xsl:if>
    <tr>
      <td>
        <br/>
        <table style="line-height:11pt" cellpadding="0" cellspacing="0" align="left" border="0">
          <!--
					===================================================================================
					include provisiones from of confirmation for the type of swap transaction 
					omitting fixed rate payer and floating rate payer.
					Add the condition: show the payer when the swaption straddle node is false
					================================================================================
					-->
          <xsl:choose>
            <xsl:when test ="//dataDocument/trade/swaption[swaptionStraddle and swaptionStraddle='true']">
            </xsl:when>

            <xsl:when test ="//dataDocument/trade/capFloor/capFloorStream/node()=true()">
              <xsl:choose>
                <xsl:when test ="$varCapFloorType = 'Vanilla'">
                  <tr>
                    <td style="{$pTDCSS}" class="label">
                      <xsl:text>Floating Rate Payer: </xsl:text>
                    </td>
                    <td style="{$pTDCSS}">
                      <xsl:call-template name="handleFloatingRatePayer">
                        <xsl:with-param name="pStream" select="$pStream"/>
                      </xsl:call-template>
                      <xsl:text>&#xa;</xsl:text>
                    </td>
                  </tr>
                </xsl:when>
                <xsl:when test ="$varCapFloorType = 'Collar'">
                  <tr>
                    <td style="{$pTDCSS}" class="label">
                      <xsl:text>Floating Cap Rate Payer: </xsl:text>
                    </td>
                    <td style="{$pTDCSS}">
                      <xsl:call-template name="getPartyNameAndID" >
                        <xsl:with-param name="pParty" select="$pStream/payerPartyReference/@href" />
                      </xsl:call-template>
                    </td>
                  </tr>

                  <tr>
                    <td style="{$pTDCSS}" class="label">
                      <xsl:text>Floating Floor Rate Payer: </xsl:text>
                    </td>
                    <td style="{$pTDCSS}">
                      <xsl:call-template name="getPartyNameAndID" >
                        <xsl:with-param name="pParty" select="$pStream/receiverPartyReference/@href" />
                      </xsl:call-template>
                    </td>
                  </tr>
                </xsl:when>

                <xsl:when test ="$varCapFloorType = 'Straddle' or $varCapFloorType = 'Strangle'">
                  <tr>
                    <td style="{$pTDCSS}" class="label">
                      <xsl:text>Floating Cap Rate Payer: </xsl:text>
                    </td>
                    <td style="{$pTDCSS}">
                      <xsl:call-template name="getPartyNameAndID" >
                        <xsl:with-param name="pParty" select="$pStream/payerPartyReference/@href" />
                      </xsl:call-template>
                    </td>
                  </tr>

                  <tr>
                    <td style="{$pTDCSS}" class="label">
                      <xsl:text>Floating Floor Rate Payer: </xsl:text>
                    </td>
                    <td style="{$pTDCSS}">
                      <xsl:call-template name="getPartyNameAndID" >
                        <xsl:with-param name="pParty" select="$pStream/payerPartyReference/@href" />
                      </xsl:call-template>
                    </td>
                  </tr>
                </xsl:when>
              </xsl:choose>

              <xsl:call-template name="handleCapOrFloorRate">
                <xsl:with-param name="pStream" select="$pStream"/>
                <xsl:with-param name="pTDCSS" select="$pTDCSS" />
              </xsl:call-template>
            </xsl:when>

            <xsl:otherwise>
              <tr>
                <td style="{$pTDCSS}" class="label">
                  <xsl:text>Floating Rate Payer: </xsl:text>
                </td>
                <td style="{$pTDCSS}">
                  <xsl:call-template name="handleFloatingRatePayer">
                    <xsl:with-param name="pStream" select="$pStream"/>
                  </xsl:call-template>
                  <xsl:text>&#xa;</xsl:text>
                </td>
              </tr>
            </xsl:otherwise>
          </xsl:choose>


          <xsl:if test="$pIsCrossCurrency=true()">
            <tr>
              <td style="{$pTDCSS}" class="label">Floating Rate Payer Currency Amount:</td>
              <td style="{$pTDCSS}">
                <xsl:call-template name="getFloatingRatePayerCurrencyAmount">
                  <xsl:with-param name="pStream" select="$pStream"/>
                </xsl:call-template>
              </td>
            </tr>
          </xsl:if>

          <xsl:call-template name="displayFloatingCalculation" >
            <xsl:with-param name="pCalculation" select="$pStream/calculationPeriodAmount/calculation" />
            <xsl:with-param name="pTDCSS" select="$pTDCSS" />
          </xsl:call-template>
          <!-- 
					========================================================================================================================================================
					Begin Region "Detail" of Floating Rate
					when the condition is in "true" it Display all the informations (Rate Payer/Rate Option/Spread/Day Count/Payment Dates/Reset Dates/Compounding)
					when the condition is in "false" it Display (Rate Payer/Rate Option/Maturity/Spread/Day Count/Compounding)
					this section add the detail about payment dates for swap
					=========================================================================================================================================================
					-->
          <xsl:if test="$pIsDisplayDetail=true()">
            <tr>
              <!-- 
							============================================================================================================
							If Delayed Payment or early Payment Applies (payment days Offset [OIS]): display the "Period End Dates"
							If not Delayed Payment or early Payment Applies: display the "Floating Rate Payment Dates Frequency:"
							=============================================================================================================
							-->
              <xsl:if test="$pStream/paymentDates/paymentDaysOffset=true()">
                <td style="{$pTDCSS}" class="label">Period End Dates: </td>
                <td style="{$pTDCSS}">
                  <xsl:call-template name="getFloatingRateCalculationPeriodDatesFrequency">
                    <xsl:with-param name="pStream" select="$pStream"/>
                  </xsl:call-template>
                  <xsl:call-template name="getBusinessDayConvention" >
                    <xsl:with-param name="pBusinessDayConvention" select="$pStream/calculationPeriodDates/calculationPeriodDatesAdjustments/businessDayConvention" />
                  </xsl:call-template>
                </td>
              </xsl:if>

              <xsl:if test="$pStream/paymentDates/paymentDaysOffset=false()">
                <td style="{$pTDCSS}" class="label">Floating Rate Payment Dates Frequency:</td>
                <td style="{$pTDCSS}">
                  <xsl:call-template name="getFloatingRatePaymentDatesFrequency">
                    <xsl:with-param name="pStream" select="$pStream"/>
                  </xsl:call-template>
                  <xsl:call-template name="getBusinessDayConvention" >
                    <xsl:with-param name="pBusinessDayConvention" select="$pStream/paymentDates/paymentDatesAdjustments/businessDayConvention" />
                  </xsl:call-template>
                </td>
              </xsl:if>
            </tr>
            <!--
						==========================================================
						display the business days for Floating amount 
						========================================================== 
						-->
            <xsl:if test ="normalize-space($pStream/paymentDates/paymentDatesAdjustments/businessCenters)">
              <tr>
                <td style="{$pTDCSS}" class="label">Business Days for Floating Amounts: </td>
                <td style="{$pTDCSS}">
                  <xsl:call-template name="getBCFullNames">
                    <xsl:with-param name="pBCs" select="$pStream/paymentDates/paymentDatesAdjustments/businessCenters"/>
                  </xsl:call-template>
                </td>
              </tr>
            </xsl:if>
            <!-- 
						===============================================================
						Set the stream number corresponding to the floating stream 
						===============================================================
						-->
            <xsl:variable name="streamNoIRSFLO">
              <xsl:value-of select="//Event[EVENTCODE='IRS' and EVENTTYPE='FLO']/STREAMNO"/>
            </xsl:variable>
            <!-- 
						===============================================================
						Floating Rate Payment Dates: section in comment
						Display the list of the payment dates for the floating stream
						if you need this information cut the comment
						===============================================================		
						<xsl:if test="//Event[STREAMNO=$streamNoIRSFLO and EVENTCODE='TER' and EVENTTYPE='INT']/EventClass[EVENTCLASS = 'STL']">
							<xsl:call-template name="BreakLine"/>
							<tr>
							<td style="{$pTDCSS}" class="label">
							</td>
							<td style="{$pTDCSS}">
							<xsl:call-template name="getPaymentDatesList">
							<xsl:with-param name="pStreamNo" select="$streamNoIRSFLO"/>
							<xsl:with-param name="pSeparator">
							<br/>
							<xsl:text>&#xa;</xsl:text>
							</xsl:with-param>
							<xsl:with-param name="pDelimiter">
							<xsl:text>&#x20;&#x20;&#x20;</xsl:text>
							</xsl:with-param>
							</xsl:call-template>
							</td>
							</tr>					
						</xsl:if>	
						-->

            <!-- 
						=========================================================================
						only for cap/floor
						Display fixing dates						
						=========================================================================
						<xsl:for-each select="//dataDocument/trade/capFloor/capFloorStream[resetDates/node()]">
							<xsl:variable name="streamNo">
								<xsl:value-of select="position()"/>
							</xsl:variable>
						<xsl:if test="//Event[INSTRUMENTNO='1' and STREAMNO=$streamNo and EVENTCODE='RES' and EVENTTYPE='FLO']/EventClass[EVENTCLASS='FXG']">
							<xsl:call-template name="BreakLine"/>
							<tr>
								<td style="{$pTDCSS}" class="label">
									<xsl:text>Fixing Dates:</xsl:text>
								</td>
								<td style="{$pTDCSS}">
								<xsl:call-template name="getFixingDatesList">
									<xsl:with-param name="pStreamNo" select="$streamNo"/>
									<xsl:with-param name="pSeparator">
									<br/>
									<xsl:text>&#xa;</xsl:text>
									</xsl:with-param>
									<xsl:with-param name="pDelimiter">
									<xsl:text>&#x20;&#x20;&#x20;</xsl:text>
									</xsl:with-param>
								</xsl:call-template>										
								<xsl:call-template name="getFixingDates">
									<xsl:with-param name="pStream" select="$pStream"/>
								</xsl:call-template>
								</td>
							</tr>
						</xsl:if>
						</xsl:for-each>
						-->

            <!-- 
						========================================================================================
						only for swap
						Display fixing dates 
						========================================================================================
						<xsl:for-each select="//dataDocument/trade/swap/swapStream[resetDates/node()]">
							<xsl:variable name="swapStreamId">
								<xsl:value-of select="./@id"/>
							</xsl:variable>
						=============================					
						set the swap stream number 
						=============================
						<xsl:variable name="streamNo">
							<xsl:value-of select="substring( $swapStreamId, 11, 2 )"/>
						</xsl:variable>						
						=========================================
						Display only if the fixing dates exist
						=========================================
						<xsl:if test="//Event[INSTRUMENTNO='1' and STREAMNO=$streamNo and EVENTCODE='RES' and EVENTTYPE='FLO']/EventClass[EVENTCLASS='FXG']">
							<xsl:call-template name="BreakLine"/>
								<tr>
									<td style="{$pTDCSS}" class="label">
										<xsl:text>Fixing Dates:</xsl:text>
									</td>
									<td style="{$pTDCSS}">
										<xsl:call-template name="getFixingDatesList">
											<xsl:with-param name="pStreamNo" select="$streamNo"/>
											<xsl:with-param name="pSeparator">
											<br/>
											<xsl:text>&#xa;</xsl:text>
											</xsl:with-param>
											<xsl:with-param name="pDelimiter">
											<xsl:text>&#x20;&#x20;&#x20;</xsl:text>
											</xsl:with-param>
										</xsl:call-template>	
									</td>
								</tr>
									<xsl:call-template name="BreakLine"/>
							</xsl:if>
							</xsl:for-each>
							-->
            <tr>
              <td style="{$pTDCSS}" class="label">
                <xsl:text>Fixing Dates: </xsl:text>
              </td>
              <td style="{$pTDCSS}">
                <xsl:call-template name="getFixingDates">
                  <xsl:with-param name="pStream" select="$pStream"/>
                </xsl:call-template>
              </td>
            </tr>

            <tr>
              <td style="{$pTDCSS}" class="label">
                <xsl:text>Reset Dates: </xsl:text>
              </td>
              <td style="{$pTDCSS}">
                <xsl:call-template name="getResetDatesType">
                  <xsl:with-param name="pStream" select="$pStream"/>
                </xsl:call-template>
                <xsl:call-template name="getBusinessDayConvention" >
                  <xsl:with-param name="pBusinessDayConvention" select="$pStream/resetDates/resetDatesAdjustments/businessDayConvention" />
                </xsl:call-template>
              </td>
            </tr>

            <xsl:if test="$pStream/resetDates/rateCutOffDaysOffset/node()">
              <tr>
                <td style="{$pTDCSS}" class="label">Rate Cut-off Dates:</td>
                <td>
                  <xsl:call-template name="getRateCutOffDaysOffset">
                    <xsl:with-param name="pStream" select="$pStream"/>
                  </xsl:call-template>
                </td>
              </tr>
            </xsl:if>

            <xsl:if test="$pStream/calculationPeriodAmount/calculation/floatingRateCalculation/averagingMethod/node()">
              <tr>
                <td style="{$pTDCSS}" class="label">Method of Averaging:</td>
                <td>
                  <xsl:call-template name="getMethodOfAveraging">
                    <xsl:with-param name="pStream" select="$pStream"/>
                  </xsl:call-template>
                  <xsl:text>&#xa;</xsl:text>
                </td>
              </tr>
            </xsl:if>
          </xsl:if>
          <!-- End Region "Detail" of Floating Rate-->
          <tr>
            <td valign="top"  style="{$pTDCSS};text-indent:20px;">Compounding:</td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="getCompounding">
                <xsl:with-param name="pStream" select="$pStream"/>
              </xsl:call-template>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </xsl:template>
  <!-- end of the section in use by all Ird products-->



  <!-- Display RateSchedule Template  -->
  <xsl:template name="displayRateSchedule">
    <xsl:param name="pName"/>
    <xsl:param name="pSchedule"/>
    <xsl:param name="pTDCSS"/>
    <tr>
      <td style="{$pTDCSS}" class="label">
        <xsl:value-of select="$pName"/> Rate:
      </td>
      <td style="{$pTDCSS}">
        <xsl:call-template name="format-fixed-rate">
          <xsl:with-param name="fixed-rate" select="$pSchedule/initialValue" />
        </xsl:call-template>
      </td>
    </tr>

    <xsl:if test="$pSchedule/step/node()=true()">
      <tr>
        <td style="{$pTDCSS}" class="label" rowspan="count($pSchedule/step)"></td>
        <td style="{$pTDCSS}">
          <table cellpadding="0" cellspacing="0" border="0" align="left">
            <xsl:for-each select="$pSchedule/step">
              <tr>
                <td style="{$pTDCSS}">
                  <xsl:call-template name="format-fixed-rate">
                    <xsl:with-param name="fixed-rate" select="stepValue" />
                  </xsl:call-template>
                </td>
                <td style="{$pTDCSS}">&#xa0;as from&#xa0;</td>
                <td style="{$pTDCSS}">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="stepDate"/>
                  </xsl:call-template>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </td>
      </tr>
    </xsl:if>
  </xsl:template>

  <!-- Display Floating Calculation  -->
  <!-- contains (Floating Rate Option, Designated Maturity, Spread, Floating Rate Day Count Fraction)-->
  <xsl:template name="displayFloatingCalculation">
    <xsl:param name="pCalculation"/>
    <xsl:param name="pTDCSS"/>
    <tr>
      <td style="{$pTDCSS}" class="label">Floating Rate Option:</td>
      <td style="{$pTDCSS}">
        <xsl:call-template name="getFloatingRateIndex">
          <xsl:with-param name="pCalculation" select="$pCalculation"/>
        </xsl:call-template>
      </td>
    </tr>
    <xsl:if test ="normalize-space(//dataDocument/repository/rateIndex/informationSource)">
      <tr>
        <td style="{$pTDCSS}" class="label">Source:</td>
        <td style="{$pTDCSS}">
          <xsl:call-template name="getFloatingRateSource">
            <xsl:with-param name="pCalculation" select="$pCalculation"/>
          </xsl:call-template>
        </td>
      </tr>
    </xsl:if>
    <!-- 
		================================================================================
		When getFrequency template returns "not applicable" this row is not displayed
		es. for the instrument OIS-Swap this information is not applicable
		================================================================================
		-->
    <xsl:variable name ="varGetFrequency">
      <xsl:call-template name="getFrequency" >
        <xsl:with-param name="pFrequency" select="$pCalculation/floatingRateCalculation/indexTenor"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:if test ="$varGetFrequency != 'not applicable'">
      <tr>
        <td style="{$pTDCSS}" class="label">Designated Maturity:</td>
        <td style="{$pTDCSS}">
          <xsl:call-template name="getFrequency" >
            <xsl:with-param name="pFrequency" select="$pCalculation/floatingRateCalculation/indexTenor"/>
          </xsl:call-template>
          <xsl:text>&#xa;</xsl:text>
        </td>
      </tr>
    </xsl:if>

    <tr>
      <td style="{$pTDCSS}" class="label">Spread:</td>
      <td style="{$pTDCSS}">
        <xsl:call-template name="displaySpread">
          <xsl:with-param name="pSpread" select="$pCalculation/floatingRateCalculation/spreadSchedule"/>
        </xsl:call-template>
        <xsl:text>&#xa;</xsl:text>
      </td>
    </tr>

    <tr>
      <td style="{$pTDCSS}" class="label">Floating Rate Day Count Fraction:</td>
      <td style="{$pTDCSS}">
        <xsl:call-template name="debugXsl">
          <xsl:with-param name="pCurrentNode" select="$pCalculation/dayCountFraction"/>
        </xsl:call-template>

        <xsl:value-of select="$pCalculation/dayCountFraction"/>
        <xsl:text>&#xa;</xsl:text>
      </td>
    </tr>
  </xsl:template>

  <!-- Display Spread Template -->
  <xsl:template name="displaySpread">
    <xsl:param name="pSpread" />
    <xsl:call-template name="debugXsl">
      <xsl:with-param name="pCurrentNode" select="$pSpread"/>
    </xsl:call-template>

    <xsl:if test="$pSpread=true()">
      <xsl:variable name="m_spread">
        <xsl:call-template name="format-fixed-rate">
          <xsl:with-param name="fixed-rate" select="$pSpread/initialValue" />
        </xsl:call-template>
      </xsl:variable>
      <xsl:call-template name="ReplacePlusMinus">
        <xsl:with-param name="pString" select="$m_spread"/>
      </xsl:call-template>

      <xsl:if test="$pSpread/step/node()=true()">
        <tr>
          <td class="label" rowspan="count($pSpread/step)"></td>
          <td>
            <table cellpadding="0" cellspacing="0" border="0" align="left">
              <xsl:for-each select="$pSpread/step">
                <tr>
                  <td>
                    <xsl:call-template name="getSpread"/>
                  </td>
                  <td>&#xa0;as from&#xa0;</td>
                  <td>
                    <xsl:call-template name="format-date">
                      <xsl:with-param name="xsd-date-time" select="stepDate"/>
                    </xsl:call-template>
                  </td>
                </tr>
              </xsl:for-each>
            </table>
          </td>
        </tr>
      </xsl:if>

    </xsl:if>
    <xsl:if test="$pSpread=false()">
      <xsl:value-of select="'None'"/>
    </xsl:if>
  </xsl:template>
  <!-- 
	========================================
	== Floating Amount: END REGION   
	========================================
	-->
  <!-- 
	========================================
	== Calculation: BEGIN REGION   
	========================================
	-->
  <!-- Templates displayIrdCalculationAgent -->
  <xsl:template name="displayIrdCalculationAgent">
    <xsl:param name="pTDCSS"/>
    <tr style="line-height:15pt">
      <td valign="middle" align="left" bgcolor="lavender" colspan="2">
        <xsl:call-template name="RoundedPanel">
          <xsl:with-param name="pTitle" select="'Calculation'"/>
        </xsl:call-template>
      </td>
    </tr>
    <tr>
      <td>
        <br/>
        <table style="line-height:11pt" cellpadding="0" cellspacing="0" align="left" border="0">
          <tr>
            <td style="{$pTDCSS}" class="label">Calculation Agent:</td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="getCalculationAgent">
                <xsl:with-param name="pTrade" select="//trade"/>
              </xsl:call-template>
              <xsl:text>&#xa;</xsl:text>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </xsl:template>
  <!-- 
	========================================
	== Calculation: END REGION   
	========================================
	-->
  <!-- 
	========================================
	== Discounting: BEGIN REGION   
	========================================
	-->
  <!-- Template displayDiscounting -->
  <xsl:template name="displayDiscounting">
    <xsl:param name="pStream"/>

    <xsl:if test="$pStream/calculationPeriodAmount/calculation/discounting/node()">
      <tr>
        <td colspan="2">
          <xsl:call-template name="RoundedPanel">
            <xsl:with-param name="pTitle" select="'Discounting'"/>
          </xsl:call-template>
        </td>
      </tr>
      <tr>
        <td>
          <table cellpadding="1" align="left">
            <xsl:call-template name="BreakLine"/>
            <tr>
              <td class="label">Discount Rate:</td>
              <td>
                <xsl:call-template name="getDiscountRate">
                  <xsl:with-param name="pStream" select="$pStream"/>
                </xsl:call-template>
                <xsl:text>&#xa;</xsl:text>
              </td>
            </tr>
            <tr>
              <td class="label">Discount Rate Day Count Fraction:</td>
              <td>
                <xsl:call-template name="getDiscountRateDayCountFraction">
                  <xsl:with-param name="pStream" select="$pStream"/>
                </xsl:call-template>
                <xsl:text>&#xa;</xsl:text>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </xsl:if>
  </xsl:template>
  <!-- 
	========================================
	== Discounting: END REGION   
	========================================
	-->
  <!-- 
	========================================
	== Fees: BEGIN REGION   
	========================================
	-->
  <!-- Display Brokerage only for the party to which we are sending the confirmation -->
  <xsl:variable name="sendToID">
    <xsl:call-template name="getSendToID" />
  </xsl:variable>

  <xsl:template name="displayBrokerage">
    <xsl:param name="pTDCSS" />
    <xsl:if test="//dataDocument/trade/otherPartyPayment[paymentType = 'Brokerage']">
      <tr style="line-height:15pt">
        <td valign="middle" align="left" bgcolor="lavender" colspan="2">
          <xsl:call-template name="RoundedPanel">
            <xsl:with-param name="pTitle" select="'Fees'"/>
          </xsl:call-template>
        </td>
      </tr>
      <tr>
        <td>
          <br/>
          <table style="line-height:11pt" cellpadding="0" cellspacing="0" align="left" border="0">
            <xsl:for-each select="//dataDocument/trade/otherPartyPayment[paymentType = 'Brokerage' and payerPartyReference/@href = $sendToID]">
              <xsl:if test="position() != 1">
                <tr>
                  <td colspan="2">
                    <br/>
                  </td>
                </tr>
              </xsl:if>
              <xsl:if test="./paymentQuote">
                <tr>
                  <td style="{$pTDCSS}" class="label">
                    <xsl:value-of select="./paymentType"/>
                    <xsl:text>:</xsl:text>
                  </td>
                  <td style="{$pTDCSS}">
                    <xsl:choose>
                      <xsl:when test="./paymentQuote/percentageRateFraction">
                        <xsl:value-of select="./paymentQuote/percentageRateFraction"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="./paymentQuote/percentageRate"/>
                      </xsl:otherwise>
                    </xsl:choose>
                    <xsl:text>&#xa;</xsl:text>
                  </td>
                </tr>
              </xsl:if>
              <tr>
                <td style="{$pTDCSS}" class="label">
                  <xsl:value-of select="./paymentType"/>
                  <xsl:text> Amount:</xsl:text>
                </td>
                <td style="{$pTDCSS}">
                  <xsl:call-template name="formatMoney">
                    <xsl:with-param name="pMoney" select="./paymentAmount"/>
                  </xsl:call-template>
                  <xsl:text>&#xa;</xsl:text>
                </td>
              </tr>
              <tr>
                <td style="{$pTDCSS}" class="label">
                  <xsl:value-of select="./paymentType"/>
                  <xsl:text> Payer:</xsl:text>
                </td>
                <td style="{$pTDCSS}">
                  <xsl:call-template name="getPartyNameOrID">
                    <xsl:with-param name="pPartyID" select="./payerPartyReference/@href"/>
                  </xsl:call-template>
                  <xsl:text>&#xa;</xsl:text>
                </td>
              </tr>
              <tr>
                <td style="{$pTDCSS}" class="label">
                  <xsl:value-of select="./paymentType"/>
                  <xsl:text> Receiver:</xsl:text>
                </td>
                <td style="{$pTDCSS}">
                  <xsl:call-template name="getPartyNameOrID">
                    <xsl:with-param name="pPartyID" select="./receiverPartyReference/@href"/>
                  </xsl:call-template>
                  <xsl:text>&#xa;</xsl:text>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </td>
      </tr>
    </xsl:if>
  </xsl:template>
  <!-- 
	========================================
	== Fees: END REGION   
	========================================
	-->
  <!-- 
	=================================================
	== Account Details and Offices: BEGIN REGION   
	================================================
	-->
  <!-- 
	=========================================
	translation test
	=========================================
	<xsl:variable name="vAccountDetails">
		<xsl:call-template name="getTranslation">
			<xsl:with-param name="pResourceName" select="'ACCOUNT DETAILS'"/>
		</xsl:call-template>
	</xsl:variable>
	-->
  <!--Display Footer_Product -->
  <xsl:template name="displayFooter_Product">
    <xsl:param name="pTDCSS"/>
    <!--
		=================================================
		Add in comment the section Account details
		=================================================
		<tr style="line-height:15pt">
			<td valign="middle" align="left" bgcolor="lavender" colspan="2">
				<xsl:call-template name="RoundedPanel">
					<xsl:with-param name="pTitle" select="'Account Details'"/>
				</xsl:call-template>
			</td>
		</tr>		
		<tr>
			<td>
				<br/>
				<table style="line-height:11pt" cellpadding="0" cellspacing="0" align="left" border="0">
					<tr>
						<td style="{$pTDCSS}" class="label_nowidth" >
							<xsl:text>Account for payment to </xsl:text>
							<xsl:value-of select="$varParty_1_name" />
							<xsl:text>: </xsl:text>
						</td>
						<td style="{$pTDCSS}">
							<xsl:text>[...]</xsl:text>
							<xsl:text>&#xa;</xsl:text>
						</td>
					</tr>
					<tr>
						<td style="{$pTDCSS}" class="label_nowidth">
							<xsl:text>Account for payment to </xsl:text>
							<xsl:value-of select="$varParty_2_name" />
							<xsl:text>: </xsl:text>

						</td>
						<td style="{$pTDCSS}">
							<xsl:text>[...]</xsl:text>
							<xsl:text>&#xa;</xsl:text>
						</td>
					</tr>
				</table>
			</td>
		</tr>
		<xsl:call-template name="BreakLine"/>
		-->
    <tr style="line-height:15pt">
      <td valign="middle" align="left" bgcolor="lavender" colspan="2">
        <xsl:call-template name="RoundedPanel">
          <xsl:with-param name="pTitle" select="'Offices'"/>
        </xsl:call-template>
      </td>
    </tr>
    <tr>
      <td>
        <br/>
        <table style="line-height:11pt" cellpadding="0" cellspacing="0" align="left" border="0">
          <tr>
            <td style="{$pTDCSS}" class="label_nowidth">
              <xsl:text>(a) The office(s) of </xsl:text>
              <xsl:call-template name="getSendTo" />
              <xsl:text> for the Transaction is (are): </xsl:text>
            </td>
            <td style="{$pTDCSS}" class="label_nowidth_right">
              <xsl:value-of select="$sendToRoutingAddress"/>
              <xsl:text>&#xa;</xsl:text>
              <xsl:text>&#xa;</xsl:text>

            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label_nowidth">
              <xsl:text>(b) The office(s) of </xsl:text>
              <xsl:call-template name="getSendBy" />
              <xsl:text> for the Transaction is (are): </xsl:text>
            </td>
            <td style="{$pTDCSS}" class="label_nowidth_right">
              <xsl:value-of select="$sendByRoutingAddress"/>
              <xsl:text>&#xa;</xsl:text>
            </td>
          </tr>
          <xsl:if test="false()">
            <tr>
              <td style="{$pTDCSS}" class="label">
                <xsl:text>Broker/Arranger: </xsl:text>
              </td>
              <td></td>
            </tr>
          </xsl:if>
        </table>
      </td>
    </tr>
  </xsl:template>
  <!-- 
	============================================
	== Account Details and Offices: END REGION   
	============================================
	-->
  <!-- 
	====================================
	== Signature: BEGIN REGION   
	====================================
	-->
  <!-- Signature Variable Text -->
  <xsl:variable name="pSignatureText">
    <xsl:text>&#xa;</xsl:text>
    <xsl:text>Please confirm that the foregoing correctly sets forth the terms of our agreement by executing the&#xa;</xsl:text>
    <xsl:text>copy of this Confirmation enclosed for that purpose and returning it to us or by sending to us a letter&#xa;</xsl:text>
    <xsl:text>substantially similar to this letter, which letter sets forth the material terms of the Transaction to which this&#xa;</xsl:text>
    <xsl:text>Confirmation relates and indicates your agreement to those terms.&#xa;</xsl:text>
  </xsl:variable>

  <!-- Display Signature -->
  <xsl:template name="displaySignature">
    <xsl:param name="pTDCSS"/>
    <tr>
      <td>
        <table style="line-height:11pt" cellpadding="0" cellspacing="0" align="left" border="0">
          <tr>
            <td style="{$pTDCSS}">
              <p align="justify"  style="line-height:10pt">
                <xsl:value-of select="$pSignatureText"/>
              </p>
            </td>
          </tr>
          <xsl:call-template name="BreakLine"/>
        </table>
      </td>
    </tr>
    <tr>
      <td>
        <table style="line-height:11pt" cellpadding="0" cellspacing="0" width="100%" border="0">
          <tr>
            <td width="50%"></td>
            <td width="50%">
              <table cellpadding="0" cellspacing="0" width="100%" border="0">
                <tr>
                  <td align="left" style="font-size: 9pt;" colspan="2" width="50%">
                    <xsl:text>Yours sincerely, &#xa;</xsl:text>
                  </td>
                </tr>
                <tr>
                  <td align="left" style="font-size: 9pt;" colspan="2">
                    <xsl:call-template name="getSendBy" />
                    <br />
                  </td>
                </tr>
                <tr>
                  <td align="left" colspan="2">
                    <br />
                  </td>
                </tr>
                <tr>
                  <td align="left" colspan="2" style="font-size: 9pt;">By: &#xa;__________________________________</td>
                </tr>
                <tr>
                  <td align="left" colspan="2" style="font-size: 9pt;">
                    <xsl:text>Name: &#xa;</xsl:text>
                  </td>
                </tr>
                <tr>
                  <td align="left" colspan="2" style="font-size: 9pt;">
                    <xsl:text>Title: </xsl:text>
                    <a style="font-size: 9pt; font-style: italic;">
                      <xsl:text>Back-Office Manager &#xa;</xsl:text>
                    </a>
                  </td>
                </tr>
              </table>
            </td>
          </tr>
          <tr>
            <td colspan="2" style="color:White; line-height:9pt;">
              <br />
            </td>
          </tr>
          <tr>
            <td width="50%">
              <table cellpadding="0" cellspacing="0" width="100%" border="0">
                <tr>
                  <td align="left" style="font-size: 9pt;" colspan="2" width="50%">
                    <xsl:text>Confirmed as of the date first above written: &#xa;</xsl:text>
                  </td>
                </tr>
                <tr>
                  <td align="left" style="font-size: 9pt;" colspan="2">
                    <xsl:call-template name="getSendTo" />
                    <br />
                  </td>
                </tr>
                <tr>
                  <td colspan="2">
                    <br />
                  </td>
                </tr>
                <tr>
                  <td align="left" colspan="2" style="font-size: 9pt;">
                    <xsl:text>By: __________________________________</xsl:text>
                  </td>
                </tr>
                <tr>
                  <td align="left" colspan="2" style="font-size: 9pt;">
                    <xsl:text>Name: &#xa;</xsl:text>
                  </td>
                </tr>
                <tr>
                  <td align="left" colspan="2" style="font-size: 9pt;">
                    <xsl:text>Title: &#xa;</xsl:text>
                  </td>
                </tr>
              </table>
            </td>
            <td width="50%"></td>

            <td width="50%">
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </xsl:template>
  <!-- 
	====================================
	== Signature: END REGION   
	====================================
	-->
  <!-- 
	====================================
	== Footer: BEGIN REGION   
	====================================
	-->

  <!-- 
	=======================================
	Display Footer 
	=======================================
	-->
  <xsl:template name="displayFooter">
    <tr>
      <td>
        <br/>
        <hr color="SteelBlue" endelement="true"/>
      </td>
    </tr>
    <tr>
      <td>
        <table style="line-height:9pt;" width="100%" cellpadding="0" cellspacing="0" border="0">
          <tr>
            <td align="center" valign="top" style="font-size: 9pt;">
              <xsl:value-of select="$footer1" />
            </td>
          </tr>
          <tr>
            <td align="center" valign="top" style="font-size: 7pt;">
              <xsl:value-of select="$footer2" />
            </td>
          </tr>
          <tr>
            <td align="center" valign="top" style="font-size: 7pt;">
              <xsl:value-of select="$footer3" />
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </xsl:template>

  <!-- 
	====================================
	== Footer: END REGION   
	====================================
	-->
  <!-- 
	========================================
	== Exchange: BEGIN REGION  
	========================================
	-->
  <!--
	============================================================================================
	== Exchange Amount (è il nozionale scambiato e non è il nozionale del payer su ogni gamba) 
	== this version manages 'initial' and 'final' exchange
	== this version doesn't manage 'interim' exchange
	== to do: use event (STA/NOM - INT/NOM - TER/NOM) to manage the exchange
	=============================================================================================
	-->
  <xsl:template name="displayExchange">
    <xsl:param name="pExchange"/>
    <tr>
      <td valign="middle" align="left" bgcolor="lavender" colspan="2">
        <xsl:variable name="title">
          <xsl:value-of select="$pExchange"/>&#160;Exchange
        </xsl:variable>
        <xsl:call-template name="RoundedPanel">
          <xsl:with-param name="pTitle" select="$title"/>
        </xsl:call-template>
      </td>
    </tr>

    <tr>
      <td>
        <br/>
        <table style="line-height:11pt" cellpadding="0" cellspacing="0" align="left" border="0">
          <xsl:for-each select="//dataDocument/trade/swap/swapStream/principalExchanges[initialExchange='true']">
            <xsl:variable name="pPos" select="position()" />
            <xsl:variable name="curr" select="current()" />
            <xsl:variable name="Stream" select="//dataDocument/trade/swap/swapStream[$pPos]" />

            <xsl:if test="$pPos=1">
              <tr>
                <td class="label">
                  <xsl:value-of select="$pExchange"/> Exchange Date :
                </td>
                <td>
                  <xsl:call-template name="getExchangeDate">
                    <xsl:with-param name="pExchange" select="$pExchange" />
                    <xsl:with-param name="pStream" select="$Stream"/>
                  </xsl:call-template>
                </td>
              </tr>
            </xsl:if>
            <tr>
              <td class="label">
                <!-- selon la doc ISDA, c'est l'emetteur FROM... qui sera affiché en premier -->
                <!-- quelque soit le type Exchange ('Initial', 'Interim', 'Final')-->
                <!-- old call template -->
                <!--
								<xsl:call-template name="getPartyID">
									<xsl:with-param name="pPos" select="$pPos" />
								</xsl:call-template>
								-->
                <xsl:call-template name="getPartyNameAndID" >
                  <xsl:with-param name="pParty" select="$Stream/payerPartyReference/@href" />
                </xsl:call-template>

                <xsl:text> </xsl:text>
                <xsl:value-of select="$pExchange"/> Exchange Amount :
              </td>
              <td>
                <xsl:call-template name="getExchangeAmount">
                  <xsl:with-param name="pExchange" select="$pExchange" />
                </xsl:call-template>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </td>
    </tr>
  </xsl:template>
  <!-- 
	========================================
	== Exchange: END REGION   
	========================================
	-->
  <!-- 
	===============================================
	===============================================
	== All products: END REGION     ===============
	===============================================
	===============================================


	===============================================
	===============================================
	== IRD product's family: BEGIN REGION     =====
	===============================================
	===============================================
	-->
  <!-- Display the text of the letter for all IRD product's family -->
  <xsl:template name="displayIrdConfirmationText">
    <xsl:param name="pTDCSS"/>
    <tr>
      <td>
        <table cellpadding="0" cellspacing="0" align="left" border="0">
          <tr>
            <td style="{$pTDCSS}">
              <p align="justify" style="line-height:10pt">
                <br/>
                <xsl:text>Dear Sir or Madam,&#xa;&#xa;</xsl:text>
                <br/>
                <xsl:text>The purpose of this letter (this "Confirmation") is to confirm the terms and conditions&#xa;</xsl:text>
                <xsl:text>of the Swap Transaction entered into between us on the Trade Date specified below.&#xa;&#xa;</xsl:text>
                <br/>
                <br/>
                <xsl:text>The definitions and provisions contained in the 2006 ISDA Definitions, as published by the&#xa;</xsl:text>
                <xsl:text>International Swaps and Derivatives Association, Inc., are incorporated into this Confirmation.&#xa;</xsl:text>
                <xsl:text>In the event of any inconsistency between those definitions and provisions and this Confirmation, this&#xa;</xsl:text>
                <xsl:text>Confirmation will govern.&#xa;&#xa;</xsl:text>
                <br/>
                <br/>
                <xsl:text>This Confirmation constitutes a "Confirmation" as referred to in, and supplements, forms part of and is subject to,&#xa;</xsl:text>
                <xsl:text>the ISDA Master Agreement </xsl:text>
                <xsl:if test="$isValidMasterAgreementDate">
                  <xsl:text>dated as of </xsl:text>
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="$masterAgreementDtSignature"/>
                  </xsl:call-template>
                </xsl:if>
                <xsl:text> as amended and supplemented from time to time (the "Agreement"), &#xa;between </xsl:text>
                <xsl:value-of select="$varParty_1_name" />
                <xsl:text> and </xsl:text>
                <xsl:value-of select="$varParty_2_name" />
                <xsl:text>.&#xa;&#xa;</xsl:text>
                <br/>
                <br/>
                <xsl:text>All provisions contained in the Agreement govern this Confirmation except as expressly modified below.</xsl:text>
              </p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </xsl:template>

  <!-- 
	=================================================================
	== Terms: BEGIN REGION  used by (Swap, Cap/Floor, LoanDeposit)
	== Underlying Swap : BEGIN REGION used by (Swaption, Straddle)
	==================================================================
	-->
  <!-- Templates Display Ird Terms -->
  <xsl:template name="displayIrdTerms">
    <xsl:param name="pStreams"/>
    <xsl:param name="pIsCrossCurrency"/>
    <xsl:param name="pIsNotionalDifferent"/>
    <xsl:param name="pIsSwaption" />
    <xsl:param name="pTDCSS"/>
    <!-- 
		================================================================================
		Test to traslate into a param 		
		MEMO: when the translation is ready put the variable $tUnderlyingSwap into the 
		template Roundpanel in substitution of the text "Underlying Swap"
		==================================================================================
		<xsl:variable name="tUnderlyingSwap">
			<xsl:call-template name="getTranslation">
				<xsl:with-param name="pResourceName" select="'UNDERLYINGID'"/>
			</xsl:call-template>
		</xsl:variable>
		-->
    <tr style="line-height:15pt;">
      <td valign="middle" align="left" bgcolor="lavender" colspan="2">
        <xsl:if test="$pIsSwaption=true()">
          <xsl:call-template name="RoundedPanel">
            <xsl:with-param name="pTitle" select="'Underlying Swap'"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:if test="$pIsSwaption=false()">
          <xsl:call-template name="RoundedPanel">
            <xsl:with-param name="pTitle" select="'Terms'"/>
          </xsl:call-template>
        </xsl:if>
      </td>
    </tr>

    <tr style="line-height:11pt">
      <td style="{$pTDCSS}"  align="left">
        <!-- 
				=================================================================================================
				begin of the section for swaption straddle product
				Display the specific terms of the underlying swap if the transaction is a Swaption straddle
				=================================================================================================
				-->
        <xsl:choose>
          <xsl:when test="//dataDocument/trade/swaption[swaptionStraddle and swaptionStraddle='true']">
            <br/>
            <xsl:text>The particular terms of the underlying swap transactions to which this Swaption Straddle relates are as follows:</xsl:text>
            <table style="line-height:11pt" cellpadding="0" cellspacing="0" align="left" border="0">
              <tr>
                <td colspan="2">
                  <br/>
                </td>
              </tr>
              <tr>
                <td colspan="2" style="{$pTDCSS}" class="label">Specific terms for the Underlying Payer Swap</td>
              </tr>
              <!-- 
							==========================================================
							Modify the section underlying swap for swaption straddle
							==========================================================
							-->
              <tr>
                <td style="{$pTDCSS}" class="label">
                  <xsl:text>Fixed Rate Payer </xsl:text>
                </td>
                <td style="{$pTDCSS}">
                  <xsl:text>Buyer</xsl:text>
                </td>
              </tr>
              <tr>
                <td style="{$pTDCSS}" class="label">
                  <xsl:text>Floating Rate Payer </xsl:text>
                </td>
                <td style="{$pTDCSS}">
                  <xsl:text>Seller</xsl:text>
                </td>
              </tr>
              <tr>
                <td colspan="2">
                  <br />
                </td>
              </tr>
              <tr>
                <td colspan="2" style="{$pTDCSS}" class="label">Specific terms for the Underlying Receiver Swap</td>
              </tr>
              <tr>
                <td style="{$pTDCSS}" class="label">
                  <xsl:text>Fixed Rate Payer </xsl:text>
                </td>
                <td style="{$pTDCSS}">
                  <xsl:text>Seller</xsl:text>
                </td>
              </tr>
              <tr>
                <td style="{$pTDCSS}" class="label">
                  <xsl:text>Floating Rate Payer </xsl:text>
                </td>
                <td style="{$pTDCSS}">
                  <xsl:text>Buyer</xsl:text>
                </td>
              </tr>
              <tr>
                <td colspan="2">
                  <br />
                </td>
              </tr>
              <xsl:call-template name="displayAmountSchedule">
                <xsl:with-param name="pName" select="'Notional Amount:'"/>
                <xsl:with-param name="pCurrency" select="$pStreams[1]/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/currency"/>
                <xsl:with-param name="pSchedule" select="$pStreams[1]/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule"/>
                <xsl:with-param name="pTDCSS" select="$pTDCSS"/>
              </xsl:call-template>

              <!-- Trade date -->
              <tr>
                <td style="{$pTDCSS}" class="label">
                  <xsl:text>Trade Date:</xsl:text>
                </td>
                <td style="{$pTDCSS}"  >
                  <xsl:call-template name="getTradeDate"/>
                  <xsl:text>&#xa;</xsl:text>
                </td>
              </tr>
              <tr>
                <!-- Effective date -->
                <td style="{$pTDCSS}" valign="top" class="label">
                  <xsl:text>Effective Date:</xsl:text>
                </td>
                <td style="{$pTDCSS}"  >
                  <xsl:call-template name="getEffectiveDate">
                    <xsl:with-param name="pStreams" select="$pStreams"/>
                  </xsl:call-template>
                  <xsl:text>&#xa;</xsl:text>
                </td>
              </tr>
              <!-- Termination Date -->
              <tr>
                <td style="{$pTDCSS}" valign="top" class="label">Termination Date:</td>
                <td style="{$pTDCSS}">
                  <xsl:call-template name="getTerminationDate">
                    <xsl:with-param name="pStreams" select="$pStreams"/>
                  </xsl:call-template>
                  <xsl:text>&#xa;</xsl:text>
                </td>
              </tr>
            </table>
          </xsl:when>
          <!-- end of the section for swaption straddle product -->

          <!-- begin of the section for all Ird products -->
          <xsl:otherwise>
            <br/>
            <table style="line-height:11pt" cellpadding="0" cellspacing="0" align="left" border="0">
              <!--
							=============================================
							Only for Cap/Floor 
							display Buyer and Seller
							to verified: 
							the buyer is the payer of the capstream
							the seller is the receiver of the capstream
							==============================================
							-->
              <xsl:if test="//dataDocument/trade/capFloor/capFloorStream/node()=true()">
                <tr>
                  <td style="{$pTDCSS}" class="label">
                    <xsl:text>Buyer:</xsl:text>
                  </td>
                  <td style="{$pTDCSS}">
                    <xsl:call-template name="getPartyNameAndID" >
                      <xsl:with-param name="pParty" select="//capFloorStream/payerPartyReference/@href" />
                    </xsl:call-template>
                  </td>
                </tr>
                <tr>
                  <td style="{$pTDCSS}" class="label">
                    <xsl:text>Seller:</xsl:text>
                  </td>
                  <td style="{$pTDCSS}">
                    <xsl:call-template name="getPartyNameAndID" >
                      <xsl:with-param name="pParty" select="//capFloorStream/receiverPartyReference/@href" />
                    </xsl:call-template>
                  </td>
                </tr>
              </xsl:if>
              <!-- Notional amounts -->
              <xsl:if test="$pIsCrossCurrency=false() and $pIsNotionalDifferent=false()and $pStreams/calculationPeriodAmount/calculation">
                <!-- Only one notional in the same currency -->
                <xsl:call-template name="displayAmountSchedule">
                  <xsl:with-param name="pName" select="'Notional Amount:'"/>
                  <xsl:with-param name="pCurrency" select="$pStreams[1]/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/currency"/>
                  <xsl:with-param name="pSchedule" select="$pStreams[1]/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule"/>
                  <xsl:with-param name="pTDCSS" select="$pTDCSS"/>
                </xsl:call-template>
              </xsl:if>
              <!-- Trade date -->
              <tr>
                <td style="{$pTDCSS}" class="label">
                  <xsl:text>Trade Date:        </xsl:text>
                </td>
                <td style="{$pTDCSS}"  >
                  <xsl:call-template name="getTradeDate"/>
                  <xsl:text>&#xa;</xsl:text>
                </td>
              </tr>
              <tr>
                <!-- Effective date -->
                <td style="{$pTDCSS}" valign="top" class="label">
                  <xsl:text>Effective Date:    </xsl:text>
                </td>
                <td style="{$pTDCSS}"  >
                  <xsl:call-template name="getEffectiveDate">
                    <xsl:with-param name="pStreams" select="$pStreams"/>
                  </xsl:call-template>
                  <xsl:text>&#xa;</xsl:text>
                </td>
              </tr>
              <!-- First Regular Period Date -->
              <xsl:if test="$pStreams/calculationPeriodDates/firstRegularPeriodStartDate/node()=true()">
                <tr>
                  <td style="{$pTDCSS}" valign="top" class="label">First Regular Period Date:</td>
                  <td style="{$pTDCSS}">
                    <xsl:call-template name="getFirstRegularPeriodDate">
                      <xsl:with-param name="pStreams" select="$pStreams"/>
                    </xsl:call-template>
                    <xsl:text>&#xa;</xsl:text>
                  </td>
                </tr>
              </xsl:if>
              <!-- Last Regular Period Date -->
              <xsl:if test="$pStreams/calculationPeriodDates/lastRegularPeriodStartDate/node()=true()">
                <tr>
                  <td style="{$pTDCSS}" valign="top" class="label">Last Regular Period Date:</td>
                  <td style="{$pTDCSS}">
                    <xsl:call-template name="getLastRegularPeriodDate">
                      <xsl:with-param name="pStreams" select="$pStreams"/>
                    </xsl:call-template>
                    <xsl:text>&#xa;</xsl:text>
                  </td>
                </tr>
              </xsl:if>
              <!-- Termination Date -->
              <tr>
                <td style="{$pTDCSS}" valign="top" class="label">Termination Date:</td>
                <td style="{$pTDCSS}">
                  <xsl:call-template name="getTerminationDate">
                    <xsl:with-param name="pStreams" select="$pStreams"/>
                  </xsl:call-template>
                  <xsl:text>&#xa;</xsl:text>
                </td>
              </tr>
            </table>
          </xsl:otherwise>
          <!-- end of the section for all Ird products -->
        </xsl:choose>
        <br/>
      </td>
    </tr>
    <xsl:call-template name="BreakLine"/>
  </xsl:template>

  <!-- Template "Display Amount Schedule"  -->
  <xsl:template name="displayAmountSchedule">
    <xsl:param name="pName"/>
    <xsl:param name="pCurrency"/>
    <xsl:param name="pSchedule"/>
    <xsl:param name="pTDCSS"/>

    <tr>
      <td style="{$pTDCSS}" class="label">
        <xsl:value-of select="$pName"/>
        <xsl:text>&#x20;&#x20;&#x20;</xsl:text>
      </td>
      <td style="{$pTDCSS}">
        <xsl:value-of select="$pCurrency" />&#160;
        <xsl:value-of select="format-number($getStream1NotionalStepScheduleInitialValue, $amountPattern, $defaultCulture)" />
      </td>
    </tr>
    <!-- Are there notional steps? -->
    <xsl:if test="$IsStream1NotionalStepSchedule = true()">
      <tr>
        <td class="label" rowspan="count($getStream1NotionalSteps)"></td>
        <td>
          <table cellpadding="0" cellspacing="0" border="0" align="left">
            <xsl:for-each select="$getStream1NotionalSteps">
              <tr>
                <td>
                  <xsl:call-template name="format-money">
                    <xsl:with-param name="currency" select="$pCurrency"/>
                    <xsl:with-param name="amount" select="stepValue"/>
                  </xsl:call-template>
                </td>

                <td>&#xa0;as from&#xa0;</td>

                <td>
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="stepDate"/>
                  </xsl:call-template>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </td>
      </tr>
    </xsl:if>
  </xsl:template>

  <!-- 
	=================================================================
	== Terms: END REGION  used by (Swap, Cap/Floor, LoanDeposit)
	== Underlying Swap : END REGION used by (Swaption, Straddle)
	==================================================================
	-->
  <!-- 
	=================================================================
	== Swaption Terms: BEGIN REGION  used by (Swaption)
	==================================================================
	-->
  <!-- Templates Display Swaption Terms -->
  <xsl:template name="displaySwaptionTerms">
    <xsl:param name="pStreams"/>
    <xsl:param name="pIsCrossCurrency"/>
    <xsl:param name="pIsNotionalDifferent"/>
    <xsl:param name="pTDCSS"/>
    <tr style="line-height:15pt">
      <td valign="middle" align="left" bgcolor="lavender" colspan="2">
        <xsl:call-template name="RoundedPanel">
          <xsl:with-param name="pTitle" select="'Swaption Terms'"/>
        </xsl:call-template>
      </td>
    </tr>
    <tr style="line-height:11pt">
      <td>
        <br />
        <table style="line-height:11pt" cellpadding="0" cellspacing="0" align="left" border="0">
          <!-- Trade date -->
          <tr>
            <td style="{$pTDCSS}" class="label">Trade Date:</td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="//dataDocument/trade/tradeHeader/tradeDate"/>
              </xsl:call-template>
            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Option Style :</td>
            <td style="{$pTDCSS}">
              <xsl:if test="//dataDocument/trade/swaption/americanExercise">American</xsl:if>
              <xsl:if test="//dataDocument/trade/swaption/bermudaExercise">Bermuda</xsl:if>
              <xsl:if test="//dataDocument/trade/swaption/europeanExercise">European</xsl:if>
            </td>
          </tr>
          <!-- 
					====================================================================================
					section in comment: to define if the swaption ISDA contain the element Option Type
					=====================================================================================
					<tr>
						<td class="label">Option Type :</td>
						<td>
							<xsl:for-each select="//dataDocument/trade/swaption/swap/swapStream">
								<xsl:variable name="payer" select="payerPartyReference/@href"/>
								<xsl:variable name="receiver" select="receiverPartyReference/@href"/>
								<xsl:if test="calculationPeriodAmount/calculation/fixedRateSchedule">
									<xsl:if test="$partyID_1=$payer">Put</xsl:if>
									<xsl:if test="$partyID_1=$receiver">Call</xsl:if>
								</xsl:if>
							</xsl:for-each>
						</td>
					</tr>
					-->
          <tr>
            <td style="{$pTDCSS}" class="label">Seller :</td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="getPartyNameAndID" >
                <xsl:with-param name="pParty" select="$partyID_2" />
              </xsl:call-template>
            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Buyer :</td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="getPartyNameAndID" >
                <xsl:with-param name="pParty" select="$partyID_1" />
              </xsl:call-template>
            </td>
          </tr>
          <xsl:call-template name="displayPremium" >
            <xsl:with-param name="pPremium" select="//dataDocument/trade/swaption/premium"/>
            <xsl:with-param name="pTDCSS" select="$pTDCSS"/>
          </xsl:call-template>
        </table>
      </td>
    </tr>
    <xsl:call-template name="BreakLine"/>
  </xsl:template>
  <!-- 
	===================================================
	== Swaption Terms: END REGION  used by (Swaption)
	===================================================
	-->
  <!-- 
	==========================================
	== FRA Terms: BEGIN REGION  used by (FRA)
	==========================================
	-->
  <!-- display FraTerms-->
  <xsl:template name="displayFRATerms">
    <xsl:param name="pFRA"/>
    <xsl:param name="pTDCSS"/>
    <tr style="line-height:15pt">
      <td valign="middle" align="left" bgcolor="lavender" colspan="2">
        <xsl:call-template name="RoundedPanel">
          <xsl:with-param name="pTitle" select="'Terms'"/>
        </xsl:call-template>
      </td>
    </tr>
    <tr>
      <td>
        <br/>
        <table style="line-height:11pt" cellpadding="0" cellspacing="0" align="left" border="0">
          <tr>
            <td style="{$pTDCSS}" class="label">Trade Date :</td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="//dataDocument/trade/tradeHeader/tradeDate"/>
              </xsl:call-template>
            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Notional Amount:</td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="format-money">
                <xsl:with-param name="currency" select="$pFRA/notional/currency"/>
                <xsl:with-param name="amount" select="//dataDocument/trade/fra/notional/amount"/>
              </xsl:call-template>
            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Effective Date:</td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="$pFRA/adjustedEffectiveDate"/>
              </xsl:call-template>
            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Termination Date:</td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="$pFRA/adjustedTerminationDate"/>
              </xsl:call-template>
            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Fixed Rate Payer:</td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="getPartyNameAndID" >
                <xsl:with-param name="pParty" select="$pFRA/buyerPartyReference/@href" />
              </xsl:call-template>
            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Fixed Rate:</td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="format-fixed-rate">
                <xsl:with-param name="fixed-rate" select="$pFRA/fixedRate"/>
              </xsl:call-template>
            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Floating Rate Payer:</td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="getPartyNameAndID" >
                <xsl:with-param name="pParty" select="$pFRA/sellerPartyReference/@href" />
              </xsl:call-template>
            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Payment Date:</td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="$pFRA/paymentDate/unadjustedDate"/>
              </xsl:call-template>
              <xsl:call-template name="getBusinessDayConvention" >
                <xsl:with-param name="pBusinessDayConvention" select="$pFRA/paymentDate/dateAdjustments/businessDayConvention" />
              </xsl:call-template>
            </td>
          </tr>
          <!--
					=============================================
					get FRA floating rate option displayname
					==============================================
					-->
          <tr>
            <td style="{$pTDCSS}" class="label">Floating Rate Option:</td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="getFRAFloatingRateIndex">
                <xsl:with-param name="pCalculation" select="$pFRA"/>
              </xsl:call-template>
            </td>
          </tr>
          <xsl:if test ="normalize-space(//dataDocument/repository/rateIndex/informationSource)">
            <tr>
              <td style="{$pTDCSS}" class="label">Source:</td>
              <td style="{$pTDCSS}">
                <xsl:call-template name="getFRAFloatingRateSource">
                  <xsl:with-param name="pCalculation" select="$pFRA"/>
                </xsl:call-template>
              </td>
            </tr>
          </xsl:if>

          <tr>
            <td style="{$pTDCSS}" class="label">Designated Maturity:</td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="getFrequency" >
                <xsl:with-param name="pFrequency" select="$pFRA/indexTenor"/>
              </xsl:call-template>
            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Spread:</td>
            <td style="{$pTDCSS}">None</td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Floating Rate Day Count Fraction:</td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="debugXsl">
                <xsl:with-param name="pCurrentNode" select="$pFRA/dayCountFraction"/>
              </xsl:call-template>
              <xsl:value-of select="$pFRA/dayCountFraction"/>
            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Reset Dates:</td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="$pFRA/adjustedEffectiveDate"/>
              </xsl:call-template>
            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">FRA Discounting:</td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="FRADiscounting">
                <xsl:with-param name="pStream" select="$pFRA/fraDiscounting"/>
              </xsl:call-template>
            </td>
          </tr>
          <!-- 
					===========================================================
					get the business day convention:
					Following, Modified Following, Preceding for payment date
					============================================================
					-->
          <tr>
            <td style="{$pTDCSS}" class="label">Business Day Convention:</td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="debugXsl">
                <xsl:with-param name="pCurrentNode" select="$pFRA/paymentDate/dateAdjustments/businessDayConvention"/>
              </xsl:call-template>
              <xsl:value-of select="$pFRA/paymentDate/dateAdjustments/businessDayConvention"/>
            </td>
          </tr>
        </table>
        <br/>
      </td>
    </tr>
  </xsl:template>
  <!-- 
	========================================
	== FRA Terms: END REGION  used by (FRA)
	========================================
	-->
  <!-- 
	========================================
	== Fixed Rate Schedule: BEGIN REGION  
	========================================
	-->
  <!--
	======================================================================
	template get fixed rate
	it returns the fixed rate for swap if contains steps schedule also 
	======================================================================
	-->
  <xsl:template name="displayFixedRate">
    <xsl:param name="pName"/>
    <xsl:param name="pStream"/>
    <xsl:param name="pTDCSS"/>

    <tr>
      <td style="{$pTDCSS}" class="label">
        <xsl:value-of select="$pName"/>
      </td>
      <td style="{$pTDCSS}">
        <xsl:call-template name="getFixedRate">
          <xsl:with-param name="pStream" select="$pStream"/>
        </xsl:call-template>
      </td>
    </tr>

    <!-- if the stream1 ot Stream2 contains fixed rate steps schedule -->
    <xsl:if test="$varIsStream1FixedRateStepSchedule = true()">
      <tr>
        <td class="label" rowspan="count($varGetStream1FixedRateStepSchedule)"></td>
        <td>
          <table cellpadding="0" cellspacing="0" border="0" align="left">
            <xsl:for-each select="$varGetStream1FixedRateStepSchedule">
              <tr>
                <td>
                  <xsl:call-template name="format-fixed-rate">
                    <xsl:with-param name="fixed-rate" select="stepValue" />
                  </xsl:call-template>
                  <xsl:text> as from </xsl:text>
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="stepDate"/>
                  </xsl:call-template>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </td>
      </tr>
    </xsl:if>

    <!-- if the stream2 contains fixed rate steps schedule -->
    <xsl:if test="$varIsStream2FixedRateStepSchedule = true()">
      <tr>
        <td class="label" rowspan="count($varGetStream2FixedRateStepSchedule)"></td>
        <td>
          <table cellpadding="0" cellspacing="0" border="0" align="left">
            <xsl:for-each select="$varGetStream2FixedRateStepSchedule">
              <tr>
                <td>
                  <xsl:call-template name="format-fixed-rate">
                    <xsl:with-param name="fixed-rate" select="stepValue" />
                  </xsl:call-template>
                  <xsl:text> as from </xsl:text>
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="stepDate"/>
                  </xsl:call-template>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </td>
      </tr>
    </xsl:if>
  </xsl:template>


  <!-- Display Fixed Rate Schedule Template -->
  <xsl:template name="displayFixedRateSchedule">
    <xsl:param name="pStream"/>
    <xsl:param name="pIsCrossCurrency"/>
    <xsl:param name="pIsDisplayTitle" />
    <xsl:param name="pIsDisplayDetail" />
    <xsl:param name="pTDCSS" />

    <!-- begin of the section in use by all IRD products-->
    <xsl:if test="$pIsDisplayTitle=true()">
      <tr>
        <td valign="middle" align="left" bgcolor="lavender" colspan="2">
          <xsl:call-template name="RoundedPanel">
            <xsl:with-param name="pTitle" select="'Fixed Amounts'"/>
          </xsl:call-template>
        </td>
      </tr>
    </xsl:if>
    <tr>
      <td>
        <br/>
        <table style="line-height:11pt" cellpadding="0" cellspacing="0" align="left" border="0">
          <!--
					===========================================================
					== only for cap/floor
					== display fixed amount for cap/floor
					== Fixed Rate Payer = premium rate payer
					== Fixed Rate = premium rate
					== Fixed Rate Payer Payment date = premium payment date
					== Fixed Amount = Premium amount
					===========================================================					
					-->
          <xsl:choose>
            <xsl:when test ="//dataDocument/trade/capFloor/capFloorStream/node()=true()">
              <tr>
                <td style="{$pTDCSS}" class="label">Fixed Rate Payer:</td>
                <td style="{$pTDCSS}">
                  <xsl:call-template name="getCapFloorPremiumRatePayer"/>
                </td>
              </tr>

              <tr>
                <td style="{$pTDCSS}" class="label">Fixed Rate:</td>
                <td style="{$pTDCSS}">
                  <xsl:call-template name="getCapFloorPremiumRate"/>
                </td>
              </tr>

              <tr>
                <td style="{$pTDCSS}" class="label">Fixed Amount:</td>
                <td style="{$pTDCSS}">
                  <xsl:call-template name="getCapFloorPremiumAmount"/>
                </td>
              </tr>
              <tr>
                <td style="{$pTDCSS}" class="label">Fixed Rate Payer Payment Date(s):</td>
                <td style="{$pTDCSS}">
                  <xsl:call-template name="getCapFloorPremiumPaymentDate"/>
                </td>
              </tr>
            </xsl:when>
            <xsl:otherwise>

              <!--
						=============================================================================
						include provisiones form of confirmation for the type of swap transaction 
						omitting fixed rate payer and floating rate payer.
						Add the condiction: show the payer when the swaption straddle node is false
						==============================================================================
						-->
              <xsl:choose>
                <xsl:when test ="//dataDocument/trade/swaption[swaptionStraddle and swaptionStraddle='true']">
                </xsl:when>
                <xsl:otherwise>
                  <tr>
                    <td style="{$pTDCSS}" class="label">
                      <xsl:text>Fixed Rate Payer: </xsl:text>
                    </td>
                    <td style="{$pTDCSS}">
                      <xsl:call-template name="getPartyNameAndID" >
                        <xsl:with-param name="pParty" select="$pStream/payerPartyReference/@href" />
                      </xsl:call-template>
                      <xsl:text>&#xa;</xsl:text>
                    </td>
                  </tr>
                </xsl:otherwise>
              </xsl:choose>

              <xsl:if test="$pStream/calculationPeriodAmount/knownAmountSchedule=true()">
                <tr>
                  <td style="{$pTDCSS}">
                    <xsl:call-template name="handleFixedAmount">
                      <xsl:with-param name="pStream" select="$pStream"/>
                    </xsl:call-template>
                  </td>
                </tr>
              </xsl:if>

              <xsl:if test="$pIsCrossCurrency=true()">
                <tr>
                  <td style="{$pTDCSS}" class="label">Fixed Rate Payer Currency Amount:</td>
                  <td style="{$pTDCSS}">
                    <xsl:call-template name="getFixedRatePayerCurrencyAmount">
                      <xsl:with-param name="pStream" select="$pStream"/>
                    </xsl:call-template>
                    <xsl:text>&#xa;</xsl:text>
                  </td>
                </tr>
              </xsl:if>


              <xsl:if test="$pStream/calculationPeriodAmount/knownAmountSchedule=false()">
                <!--
							==============================================================
							If existes, it displayes fixed rate steps schedule
							In use only for the swap
							==============================================================
							-->
                <xsl:call-template name="displayFixedRate">
                  <xsl:with-param name="pName" select="'Fixed Rate:'"/>
                  <xsl:with-param name="pStream" select="$pStream"/>
                  <xsl:with-param name="pTDCSS" select="$pTDCSS"/>
                </xsl:call-template>

                <tr>
                  <td style="{$pTDCSS}" class="label">Fixed Rate Day Count Fraction:</td>
                  <td style="{$pTDCSS}">
                    <xsl:value-of select="$pStream/calculationPeriodAmount/calculation/dayCountFraction"/>
                    <xsl:text>&#xa;</xsl:text>
                  </td>
                </tr>

                <!-- Begin Region "Detail" of Fixing Rate-->
                <!--
								=================================================================================================================================================
								 when the condition is in "true" it Display all the informations (Rate Payer/Rate Option/Spread/Day Count/Payment Dates/Reset Dates/Compounding)
								 when the condition is in "false" it Display (Rate Payer/Rate Option/Maturity/Spread/Day Count/Compounding)
								 this section add the detail about payment dates for swap
								 ==================================================================================================================================================
								 -->
                <xsl:if test="$pIsDisplayDetail=true()">
                  <tr>

                    <!-- 
								=============================================================================================================		
								If Delayed Payment or early Payment Applies (payment days Offset [OIS]): display the "Period End Dates"
								If not Delayed Payment or early Payment Applies: display the "Fixed Rate Payment Dates Frequency"
								=============================================================================================================
								-->
                    <xsl:if test="$pStream/paymentDates/paymentDaysOffset=true()">
                      <td style="{$pTDCSS}" class="label">Period End Dates: </td>
                      <td style="{$pTDCSS}">
                        <xsl:call-template name="getFixedRateCalculationPeriodDatesFrequency">
                          <xsl:with-param name="pStream" select="$pStream"/>
                        </xsl:call-template>
                        <xsl:text>&#xa;</xsl:text>
                        <xsl:call-template name="getBusinessDayConvention" >
                          <xsl:with-param name="pBusinessDayConvention" select="$pStream/calculationPeriodDates/calculationPeriodDatesAdjustments/businessDayConvention" />
                        </xsl:call-template>
                      </td>
                    </xsl:if>
                    <xsl:if test="$pStream/paymentDates/paymentDaysOffset=false()">
                      <td style="{$pTDCSS}" class="label">Fixed Rate Payment Dates Frequency: </td>
                      <td style="{$pTDCSS}">
                        <xsl:call-template name="getFixedRatePaymentDatesFrequency">
                          <xsl:with-param name="pStream" select="$pStream"/>
                        </xsl:call-template>
                        <xsl:text>&#xa;</xsl:text>
                        <xsl:call-template name="getBusinessDayConvention" >
                          <xsl:with-param name="pBusinessDayConvention" select="$pStream/paymentDates/paymentDatesAdjustments/businessDayConvention" />
                        </xsl:call-template>
                      </td>
                    </xsl:if>
                  </tr>
                  <!--
									=============================================
									display the business days for fixed amount 
									=============================================
									-->
                  <xsl:if test ="normalize-space($pStream/paymentDates/paymentDatesAdjustments/businessCenters)">
                    <tr>
                      <td style="{$pTDCSS}" class="label">Business Days for Fixed Amounts: </td>
                      <td style="{$pTDCSS}">
                        <xsl:call-template name="getBCFullNames">
                          <xsl:with-param name="pBCs" select="$pStream/paymentDates/paymentDatesAdjustments/businessCenters"/>
                        </xsl:call-template>
                      </td>
                    </tr>
                  </xsl:if>
                  <!-- 
									==========================================================
									Set the stream number corresponding to the floating stream 
									===========================================================
									-->
                  <xsl:variable name="streamNoIRSFIX">
                    <xsl:value-of select="//Event[EVENTCODE='IRS' and EVENTTYPE='FIX']/STREAMNO"/>
                  </xsl:variable>

                  <!-- 
									====================================================================================
									Display the list of the payment dates for the fixed rate stream 
									if you need this information cut the comment in this section 
									====================================================================================
									<xsl:if test="//Event[STREAMNO=$streamNoIRSFIX and EVENTCODE='TER' and EVENTTYPE='INT']/EventClass[EVENTCLASS = 'STL']">
										<tr>
											<td style="{$pTDCSS}" class="label">
												Fixed Rate Payment Dates:
											</td>
											<td style="{$pTDCSS}">
												<xsl:call-template name="getFixedRatePaymentDatesList">
													<xsl:with-param name="pStreamNo" select="$streamNoIRSFIX"/>
													<xsl:with-param name="pSeparator">
														<br/>
														<xsl:text>&#xa;</xsl:text>
													</xsl:with-param>
													<xsl:with-param name="pDelimiter">
														<xsl:text>&#x20;&#x20;&#x20;</xsl:text>
													</xsl:with-param>
												</xsl:call-template>
											</td>
										</tr>
									</xsl:if>
									-->
                </xsl:if>
              </xsl:if>
            </xsl:otherwise>
          </xsl:choose>
        </table>
      </td>
    </tr>
    <xsl:call-template name="BreakLine"/>
  </xsl:template>
  <!-- 
	========================================
	== Fixed Rate Schedule: END REGION  
	========================================
	-->

  <!-- 
	========================================
	== Premium: BEGIN REGION  
	========================================
	-->
  <xsl:template name="displayPremium">
    <xsl:param name="pPremium" />
    <xsl:param name="pTDCSS"/>

    <xsl:call-template name="debugXsl">
      <xsl:with-param name="pCurrentNode" select="$pPremium"/>
    </xsl:call-template>

    <xsl:for-each select="$pPremium">
      <tr>
        <td style="{$pTDCSS}" class="label">Premium :</td>
        <td style="{$pTDCSS}">
          <xsl:call-template name="debugXsl">
            <xsl:with-param name="pCurrentNode" select="paymentAmount/currency"/>
          </xsl:call-template>
          <xsl:value-of select="paymentAmount/currency" />&#160;
          <xsl:value-of select="format-number(paymentAmount/amount, $amountPattern, $defaultCulture)" />
        </td>
      </tr>
      <tr>
        <td style="{$pTDCSS}" class="label">Premium Payment Date :</td>
        <td style="{$pTDCSS}">
          <xsl:call-template name="getPremiumPaymentDate"/>

        </td>
      </tr>
      <xsl:if test="paymentDate">
        <xsl:if test="//paymentDate/dateAdjustments/businessDayConvention != 'NONE'">
          <tr>
            <td style="{$pTDCSS}" class="label">Business Day Convention </td>
            <td style="{$pTDCSS}">
            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">for Premium Payment Date :</td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="debugXsl">
                <xsl:with-param name="pCurrentNode" select="paymentDate/dateAdjustments/businessDayConvention"/>
              </xsl:call-template>
              <xsl:value-of select="paymentDate/dateAdjustments/businessDayConvention"/>
            </td>
          </tr>
        </xsl:if>
      </xsl:if>
      <!--
			============================================================
			when the Business Center is missing don't show this row
			============================================================
			-->
      <xsl:if test ="normalize-space(//swaption/premium/paymentDate/dateAdjustments/businessCenters)">
        <tr>
          <td style="{$pTDCSS}" class="label">Business Days for Payments:</td>
          <td style="{$pTDCSS}">
            <xsl:call-template name="getBCFullNames">
              <xsl:with-param name="pBCs" select="//swaption/premium/paymentDate/dateAdjustments/businessCenters"/>
            </xsl:call-template>
          </td>
        </tr>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>
  <!-- 
	========================================
	== Premium: END REGION  
	========================================
	-->
  <!-- 
	========================================
	== Settlement Terms: BEGIN REGION  
	========================================
	-->
  <xsl:template name="displaySettlementTerms">
    <xsl:param name="pTDCSS"/>
    <tr style="line-height:15pt">
      <td valign="middle" align="left" bgcolor="lavender" colspan="2">
        <xsl:call-template name="RoundedPanel">
          <xsl:with-param name="pTitle" select="'Settlement Terms'"/>
        </xsl:call-template>
      </td>
    </tr>
    <tr>
      <td>
        <br/>
        <table style="line-height:11pt" cellpadding="0" cellspacing="0" align="left" border="0">
          <!-- 
					=======================================================================================================
					Change the node's name in "cashSettlement". old name "cashSettlementTerms": 
					========================================================================================================
					-->
          <xsl:choose>
            <xsl:when test="//cashSettlement/node()=true()">
              <tr>
                <td style="{$pTDCSS}" class="label">Settlement:</td>
                <td style="{$pTDCSS}">
                  Cash
                </td>
              </tr>
              <tr>
                <td style="{$pTDCSS}" class="label">Cash Settlement Valuation Time: </td>
                <td style="{$pTDCSS}">
                  <xsl:value-of select="//cashSettlement/cashSettlementValuationTime/hourMinuteTime"/>
                </td>
              </tr>
              <tr>
                <td style="{$pTDCSS}" class="label">Cash Settlement Valuation Date: </td>
                <td style="{$pTDCSS}">
                  <xsl:value-of select="$getCashSettlementValuationDate"/>
                </td>
              </tr>
              <tr>
                <td style="{$pTDCSS}" class="label">Valuation Business Days: </td>
                <td style="{$pTDCSS}">
                  <xsl:call-template name="getFullNameBC">
                    <xsl:with-param name="pBC" select="//cashSettlement/cashSettlementValuationDate/businessCenters"/>
                  </xsl:call-template>
                </td>
              </tr>
              <tr>
                <td style="{$pTDCSS}" class="label">Cash Settlement Payment Date: </td>
                <td style="{$pTDCSS}">
                  <xsl:value-of select="$getCashSettlementAdjustableOrRelativeDates"/>
                </td>
              </tr>
              <tr>
                <td style="{$pTDCSS}" class="label_nowidth">Business Day Convention for Cash Settlement Payment Date: </td>
                <td style="{$pTDCSS}">
                  <xsl:value-of select="$getCashSettlementPaymentDateBusinessDayConvention"/>
                </td>
              </tr>
              <tr>
                <td style="{$pTDCSS}" class="label">Cash Settlement Method: </td>
                <td style="{$pTDCSS}">
                  <xsl:value-of select="$getCashSettlementMethod"/>
                </td>
              </tr>
              <tr>
                <td style="{$pTDCSS}" class="label">Cash Settlement Currency: </td>
                <td style="{$pTDCSS}">
                  <xsl:value-of select="//cashSettlement/cashPriceMethod/cashSettlementCurrency"/>
                </td>
              </tr>
              <tr>
                <td style="{$pTDCSS}" class="label">Quotation Rate: </td>
                <td style="{$pTDCSS}">
                  <xsl:value-of select="//cashSettlement/cashPriceMethod/quotationRateType"/>
                </td>
              </tr>
            </xsl:when>
            <xsl:when test="//cashSettlement/node()=false()">
              <tr>
                <td style="{$pTDCSS}" class="label">Settlement:</td>
                <td style="{$pTDCSS}">Physical</td>
              </tr>
            </xsl:when>
            <xsl:otherwise>
              <tr>
                <td style="{$pTDCSS}" class="label">Settlement:</td>
                <td style="{$pTDCSS}">Error</td>
              </tr>
            </xsl:otherwise>
          </xsl:choose>
        </table>
      </td>
    </tr>
  </xsl:template>
  <!-- 
	========================================
	== Settlement Terms: END REGION  
	========================================
	-->
  <!-- 
	=============================================================
	== Procedure for Exercise: BEGIN REGION  used by (SWAPTION)  
	=============================================================
	-->
  <xsl:template name="displayExerciseProcedure">
    <xsl:param name="pProduct" />
    <xsl:param name="pTDCSS"/>

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

    <tr style="line-height:15pt">
      <td valign="middle" align="left" bgcolor="lavender" colspan="2">
        <xsl:call-template name="RoundedPanel">
          <xsl:with-param name="pTitle" select="'Procedure for Exercise'"/>
        </xsl:call-template>
      </td>
    </tr>
    <tr style="line-height:11pt">
      <td>
        <br />
        <table style="line-height:11pt" cellpadding="0" cellspacing="0" align="left" border="0">

          <!-- Procedure for Bermuda Exercise -->
          <xsl:if test="$pProduct/bermudaExercise">
            <tr>
              <td style="{$pTDCSS}" valign="top" class="label">Bermuda Option Exercise Dates:</td>
              <td style="{$pTDCSS}">
                <xsl:call-template name="getAdjustableOrRelativeDates" >
                  <xsl:with-param name="pSeparator">
                    <br/>
                    <xsl:text>&#xa;</xsl:text>
                  </xsl:with-param>
                  <xsl:with-param name="pDate" select="$pProduct/bermudaExercise/bermudaExerciseDates"/>
                </xsl:call-template>
              </td>
            </tr>
            <tr>
              <td style="{$pTDCSS}" class="label">Earliest Exercise Time:</td>
              <td style="{$pTDCSS}">
                <xsl:value-of select="//bermudaExercise/earliestExerciseTime/hourMinuteTime"/>
                <xsl:text>&#xa;</xsl:text>
                <!-- commented section. Now the gotten data is the Full Name of Business Center -->
                <xsl:call-template name="getFullNameBC">
                  <xsl:with-param name="pBC" select="//bermudaExercise/earliestExerciseTime/businessCenter"/>
                </xsl:call-template>
              </td>
            </tr>
            <tr>
              <td style="{$pTDCSS}" class="label">Latest Exercise Time:</td>
              <td style="{$pTDCSS}">
                <xsl:value-of select="//bermudaExercise/latestExerciseTime/hourMinuteTime"/>
                <xsl:text>&#xa;</xsl:text>
                <!-- commented section. Now the gotten data is the Full Name of Business Center -->
                <xsl:call-template name="getFullNameBC">
                  <xsl:with-param name="pBC" select="//bermudaExercise/latestExerciseTime/businessCenter"/>
                </xsl:call-template>
              </td>
            </tr>
          </xsl:if>

          <!--Procedure for American Exercise: to testing with an american swaption xml  -->
          <xsl:if test="$pProduct/americanExercise">
            <tr>
              <td style="{$pTDCSS}" class="label">Commencement Date:</td>
              <td style="{$pTDCSS}">
                <xsl:call-template name="getAdjustableOrRelativeDateSequence" >
                  <xsl:with-param name="pSeparator">
                    <br/>
                    <xsl:text>&#xa;</xsl:text>
                  </xsl:with-param>
                  <xsl:with-param name="pDate" select="$pProduct/americanExercise/commencementDate"/>
                </xsl:call-template>
              </td>
            </tr>
            <tr>
              <td style="{$pTDCSS}" class="label">Expiration Date:</td>
              <td style="{$pTDCSS}">
                <xsl:call-template name="getAdjustableOrRelativeDateSequence" >
                  <xsl:with-param name="pSeparator">
                    <br/>
                    <xsl:text>&#xa;</xsl:text>
                  </xsl:with-param>
                  <xsl:with-param name="pDate" select="$pProduct/americanExercise/expirationDate"/>
                </xsl:call-template>
              </td>
            </tr>
            <tr>
              <td style="{$pTDCSS}" class="label">Expiration Time:</td>
              <td style="{$pTDCSS}">
                <xsl:value-of select="//americanExercise/expirationTime/hourMinuteTime"/>
                <xsl:text>&#xa;</xsl:text>
                <!-- commented section. Now the gotten data is the Full Name of Business Center -->
                <xsl:call-template name="getFullNameBC">
                  <xsl:with-param name="pBC" select="//americanExercise/expirationTime/businessCenter"/>
                </xsl:call-template>
              </td>
            </tr>
          </xsl:if>

          <!--Procedure for European Exercise -->
          <xsl:if test="$pProduct/europeanExercise">
            <tr>
              <td style="{$pTDCSS}" class="label">Expiration Date:</td>
              <td style="{$pTDCSS}">
                <xsl:call-template name="getAdjustableOrRelativeDateSequence" >
                  <xsl:with-param name="pDate" select="$pProduct/europeanExercise/expirationDate"/>
                  <xsl:with-param name="pSeparator">
                    <br/>
                    <xsl:text>&#xa;</xsl:text>
                  </xsl:with-param>
                </xsl:call-template>
              </td>
            </tr>
            <tr>
              <td style="{$pTDCSS}" class="label">Expiration Time:</td>
              <td style="{$pTDCSS}">
                <xsl:value-of select="//europeanExercise/expirationTime/hourMinuteTime"/>
                <xsl:text>&#xa;</xsl:text>
                <!-- commented section. Now the gotten data is the Full Name of Business Center -->
                <xsl:call-template name="getFullNameBC">
                  <xsl:with-param name="pBC" select="//europeanExercise/expirationTime/businessCenter"/>
                </xsl:call-template>
              </td>
            </tr>
          </xsl:if>
        </table>
      </td>
    </tr>
    <xsl:call-template name="BreakLine"/>
  </xsl:template>
  <!-- 
	=============================================================
	== Procedure for Exercise: END REGION used by (SWAPTION)  
	=============================================================
	-->
  <!-- 
	===============================================
	===============================================
	== IRD product's family: END REGION       =====
	===============================================
	===============================================
	-->





  <!-- 
	===============================================
	===============================================
	== FX product's family: BEGIN REGION      =====
	===============================================
	===============================================
	-->
  <!-- Display the text of the letter for all IRD product's family -->
  <xsl:template name="displayFxConfirmationText">
    <xsl:param name="pTDCSS"/>
    <tr>
      <td>
        <table cellpadding="0" cellspacing="0" align="left" border="0">
          <tr>
            <td style="{$pTDCSS}">
              <p align="justify" style="line-height:10pt">
                <br/>
                <xsl:text>Dear Sir or Madam,</xsl:text>
                <br/>
                <xsl:text>The purpose of this letter agreement (this "Confirmation") is to confirm the terms and conditions&#xa;</xsl:text>
                <xsl:text>of the Transaction entered into between us on the Trade Date specified below (the "Transaction").&#xa;</xsl:text>
                <xsl:text>This Confirmation constitutes a "Confirmation" as referred to in the Agreement specified below.&#xa;</xsl:text>
                <br/>
                <br/>
                <xsl:text>The definitions and provisions contained in the 1998 FX and Currency Option Definitions (as published &#xa;</xsl:text>
                <xsl:text>by the International Swaps and Derivatives Association, Inc., the Emerging Markets Traders Association &#xa;</xsl:text>
                <xsl:text>and The Foreign Exchange Committee) are incorporated into this Confirmation. In the event of any inconsistency&#xa;</xsl:text>
                <xsl:text>between those definitions and provisions and this Confirmation, this Confirmation will govern.</xsl:text>
                <br/>
                <br/>
                <xsl:text>This Confirmation supplements, forms a part of, and is subject to, the ISDA Master Agreement</xsl:text>
                <xsl:if test="$isValidMasterAgreementDate">
                  <xsl:text>dated as of</xsl:text>
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="$masterAgreementDtSignature"/>
                  </xsl:call-template>
                </xsl:if>,
                <xsl:text>as amended and supplemented from time to time (the "Agreement"), between &#xa;</xsl:text>
                <xsl:value-of select="$varParty_1_name" /><xsl:text> and </xsl:text> <xsl:value-of select="$varParty_2_name" />
                <xsl:text>.&#xa;&#xa;</xsl:text>
                <xsl:text>All provisions contained in the Agreement govern this Confirmation except as expressly modified below.</xsl:text>
              </p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </xsl:template>
  <!-- 
	===================================================================
	== FX Swap and Fx Leg Terms: BEGIN REGION used by ( FxSwap, FxLeg)  
	===================================================================
	-->
  <xsl:template name="displayFxSwapLegTerms">
    <xsl:param name="pFxLegContainer"/>
    <xsl:param name="pIsNonDeliverable"/>
    <xsl:param name="pTDCSS"/>

    <tr style="line-height:15pt;">
      <td valign="middle" align="left" bgcolor="lavender" colspan="2">
        <xsl:call-template name="RoundedPanel">
          <xsl:with-param name="pTitle" select="'Terms'"/>
        </xsl:call-template>
      </td>
    </tr>
    <tr style="line-height:11pt">
      <td style="{$pTDCSS}"  align="left">
        <!-- General Term -->
        <br/>
        <table style="line-height:11pt" cellpadding="0" cellspacing="0" align="left" border="0">
          <tr style="line-height:11pt">
            <td style="{$pTDCSS}" class="label_nowidth">
              Trade Date :
            </td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="//trade/tradeHeader/tradeDate"/>
              </xsl:call-template>
            </td>
          </tr>
          <!-- Scan Leg -->
          <!-- 
					========================================
					Add breakline for each FxsingleLeg
					========================================
					-->
          <xsl:for-each select="$pFxLegContainer/fxSingleLeg">
            <xsl:variable name="pos" select="position()" />
            <xsl:variable name="curr" select="current()" />
            <xsl:choose>
              <xsl:when test="$pIsNonDeliverable=true()">
                <!-- Non Deliverable -->
                <xsl:call-template name="displayNonDeliverableInformation">
                  <xsl:with-param name="pFxSingleLeg" select="$curr"/>
                  <xsl:with-param name="pTDCSS" select="$pTDCSS"/>
                </xsl:call-template>

              </xsl:when>
              <xsl:otherwise>
                <!-- Deliverable -->
                <xsl:call-template name="displayDeliverableInformation">
                  <xsl:with-param name="pFxSingleLeg" select="$curr"/>
                  <xsl:with-param name="pTDCSS" select="$pTDCSS"/>
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:for-each>

        </table>
        <br/>
      </td>
    </tr>

  </xsl:template>
  <!-- 
	====================================================================
	== FX swap and Fx Leg Terms: END REGION used by (FxSwap, FxLeg)  
	====================================================================
	-->
  <!-- 
	==============================================================
	== Delivery information: BEGIN REGION used by (FxSwap) 
	==============================================================
	-->
  <xsl:template name="displayDeliverableInformation">
    <xsl:param name="pFxSingleLeg"/>
    <xsl:param name="pTDCSS"/>
    <xsl:call-template name="displayExchangedCurrency">
      <xsl:with-param name="pFxSingleLeg" select="$pFxSingleLeg"/>
      <xsl:with-param name="pTDCSS" select="$pTDCSS"/>
    </xsl:call-template>
  </xsl:template>

  <!-- NonDeliverableInformation -->
  <xsl:template name="displayNonDeliverableInformation">
    <xsl:param name="pFxSingleLeg"/>
    <xsl:param name="pTDCSS"/>
    <xsl:variable name="ReferenceCurrency">
      <xsl:call-template name="getReferenceCurrency">
        <xsl:with-param name="pFxSingleLeg" select="$pFxSingleLeg"/>
        <xsl:with-param name="pTDCSS" select="$pTDCSS"/>
      </xsl:call-template>
    </xsl:variable>
    <tr style="line-height:11pt">
      <td style="{$pTDCSS}" class="label">Reference Currency:</td>
      <td style="{$pTDCSS}">
        <xsl:value-of select="$ReferenceCurrency" />
      </td>
    </tr>
    <tr>
      <td style="{$pTDCSS}" class="label">Reference Currency Notional Amount:</td>
      <td style="{$pTDCSS}">
        <xsl:if test="$pFxSingleLeg/exchangedCurrency1/paymentAmount/currency=$ReferenceCurrency" >
          <xsl:call-template name="format-money">
            <xsl:with-param name="currency" select="$pFxSingleLeg/exchangedCurrency1/paymentAmount/currency"/>
            <xsl:with-param name="amount" select="$pFxSingleLeg/exchangedCurrency1/paymentAmount/amount"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:if test="$pFxSingleLeg/exchangedCurrency2/paymentAmount/currency=$ReferenceCurrency" >
          <xsl:call-template name="format-money">
            <xsl:with-param name="currency" select="$pFxSingleLeg/exchangedCurrency2/paymentAmount/currency"/>
            <xsl:with-param name="amount" select="$pFxSingleLeg/exchangedCurrency2/paymentAmount/amount"/>
          </xsl:call-template>
        </xsl:if>
      </td>
    </tr>
    <tr>
      <td style="{$pTDCSS}" class="label">Notional Amount:</td>
      <td style="{$pTDCSS}">
        <xsl:if test="$pFxSingleLeg/exchangedCurrency1/paymentAmount/currency!=$ReferenceCurrency" >
          <xsl:call-template name="format-money">
            <xsl:with-param name="currency" select="$pFxSingleLeg/exchangedCurrency1/paymentAmount/currency"/>
            <xsl:with-param name="amount" select="$pFxSingleLeg/exchangedCurrency1/paymentAmount/amount"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:if test="$pFxSingleLeg/exchangedCurrency2/paymentAmount/currency!=$ReferenceCurrency" >
          <xsl:call-template name="format-money">
            <xsl:with-param name="currency" select="$pFxSingleLeg/exchangedCurrency2/paymentAmount/currency"/>
            <xsl:with-param name="amount" select="$pFxSingleLeg/exchangedCurrency2/paymentAmount/amount"/>
          </xsl:call-template>
        </xsl:if>
      </td>
    </tr>
    <tr>
      <xsl:variable name="ReferenceCurrencyBuyer">
        <xsl:if test="$pFxSingleLeg/exchangedCurrency1/paymentAmount/currency=$ReferenceCurrency" >
          <xsl:value-of select="$pFxSingleLeg/exchangedCurrency1/receiverPartyReference/@href" />
        </xsl:if>
        <xsl:if test="$pFxSingleLeg/exchangedCurrency2/paymentAmount/currency=$ReferenceCurrency" >
          <xsl:value-of select="$pFxSingleLeg/exchangedCurrency2/receiverPartyReference/@href" />
        </xsl:if>
      </xsl:variable>
      <td style="{$pTDCSS}" class="label">Reference Currency Buyer:</td>
      <td style="{$pTDCSS}">
        <xsl:call-template name="getPartyNameAndID" >
          <xsl:with-param name="pParty" select="$ReferenceCurrencyBuyer" />
        </xsl:call-template>
      </td>
    </tr>
    <tr>
      <xsl:variable name="ReferenceCurrencySeller">
        <xsl:if test="$pFxSingleLeg/exchangedCurrency1/paymentAmount/currency=$ReferenceCurrency" >
          <xsl:value-of select="$pFxSingleLeg/exchangedCurrency1/payerPartyReference/@href" />
        </xsl:if>
        <xsl:if test="$pFxSingleLeg/exchangedCurrency2/paymentAmount/currency=$ReferenceCurrency" >
          <xsl:value-of select="$pFxSingleLeg/exchangedCurrency2/payerPartyReference/@href" />
        </xsl:if>
      </xsl:variable>
      <td style="{$pTDCSS}" class="label">Reference Currency Seller:</td>
      <td style="{$pTDCSS}">
        <xsl:call-template name="getPartyNameAndID" >
          <xsl:with-param name="pParty" select="$ReferenceCurrencySeller" />
        </xsl:call-template>
      </td>
    </tr>
    <tr>
      <td style="{$pTDCSS}" class="label">Settlement Currency:</td>
      <td style="{$pTDCSS}">
        <xsl:value-of select="$pFxSingleLeg/nonDeliverableForward/settlementCurrency" />
      </td>
    </tr>
    <tr>
      <td style="{$pTDCSS}" class="label">Settlement Date:</td>
      <td style="{$pTDCSS}">
        <xsl:call-template name="format-date">
          <xsl:with-param name="xsd-date-time" select="$pFxSingleLeg/valueDate"/>
        </xsl:call-template>
      </td>
    </tr>
    <tr>
      <td style="{$pTDCSS}" class="label">Settlement :</td>
      <td style="{$pTDCSS}">
        <xsl:value-of select="'Non Deliverable'" />
      </td>
    </tr>
  </xsl:template>
  <!-- 
	==============================================================
	== Delivery information: END REGION used by (FxSwap) 
	==============================================================
	-->
  <!-- 
	=====================================================================
	== Exchanged Currency: BEGIN REGION used by (FxSwap and FxSingleLeg) 
	=====================================================================
	-->
  <!-- 
	====================================================================================================
	Modify template: replace "Exchanged Currency Amounts" with "Amount and Currency payable by Party A"
	====================================================================================================
	-->
  <xsl:template name="displayExchangedCurrency">
    <xsl:param name="pFxSingleLeg"/>
    <xsl:param name="pTDCSS"/>
    <xsl:if test ="//fxSwap=true() and position() = 1">
      <xsl:call-template name="BreakLine"/>
      <tr>
        <td style="{$pTDCSS}" class="label" >
          Spot Rate
        </td>
        <td style="{$pTDCSS}">

        </td>
      </tr>
    </xsl:if>

    <xsl:if test ="//fxSwap=true() and position() = last()" >
      <xsl:call-template name="BreakLine"/>
      <tr>
        <td style="{$pTDCSS}" class="label" >
          Forward Rate
        </td>
        <td style="{$pTDCSS}">

        </td>
      </tr>
    </xsl:if>

    <tr>
      <td style="{$pTDCSS}" class="label" >
        Amount and Currency payable by
      </td>
      <td style="{$pTDCSS}"></td>
    </tr>

    <tr>
      <td style="{$pTDCSS}" class="label" >
        <xsl:call-template name="getPartyNameAndID" >
          <xsl:with-param name="pParty" select="$pFxSingleLeg/exchangedCurrency1/payerPartyReference/@href" />
        </xsl:call-template>:
      </td>
      <td style="{$pTDCSS}">
        <xsl:value-of select="$pFxSingleLeg/exchangedCurrency1/paymentAmount/currency" />
        <xsl:text> </xsl:text>
        <xsl:value-of select="format-number($pFxSingleLeg/exchangedCurrency1/paymentAmount/amount, $amountPattern, $defaultCulture)" />
      </td>
    </tr>

    <tr>
      <td style="{$pTDCSS}" class="label">
        Amount and Currency payable by
      </td>
      <td style="{$pTDCSS}"></td>
    </tr>

    <tr>
      <td style="{$pTDCSS}" class="label">
        <xsl:call-template name="getPartyNameAndID" >
          <xsl:with-param name="pParty" select="$pFxSingleLeg/exchangedCurrency2/payerPartyReference/@href" />
        </xsl:call-template>:
      </td>
      <td style="{$pTDCSS}">
        <xsl:value-of select="$pFxSingleLeg/exchangedCurrency2/paymentAmount/currency" />
        <xsl:text> </xsl:text>
        <xsl:value-of select="format-number($pFxSingleLeg/exchangedCurrency2/paymentAmount/amount, $amountPattern, $defaultCulture)" />
      </td>
    </tr>

    <tr>
      <td style="{$pTDCSS}" class="label">Settlement Date :</td>
      <td style="{$pTDCSS}">
        <xsl:call-template name="format-date">
          <xsl:with-param name="xsd-date-time" select="$pFxSingleLeg/valueDate"/>
        </xsl:call-template>
      </td>
    </tr>
  </xsl:template>
  <!-- 
	==============================================================
	== Exchanged Currency: END REGION used by (FxSwap) 
	==============================================================
	-->
  <!-- 
	=========================================================================================================
	== FX Digital and Fx Option Leg Terms: BEGIN REGION used by (FxDigitalOption) 
	==========================================================================================================
	-->
  <xsl:template name="displayFxDigitalTerms">
    <xsl:param name="pFxOption"/>
    <xsl:param name="pTDCSS"/>
    <xsl:variable name="IsAmerican" select="$pFxOption/fxAmericanTrigger=true()" />
    <xsl:variable name="IsEuropean" select="$pFxOption/fxEuropeanTrigger=true()" />
    <tr style="line-height:15pt;">
      <td valign="middle" align="left" bgcolor="lavender" colspan="2">
        <xsl:call-template name="RoundedPanel">
          <xsl:with-param name="pTitle" select="'Terms'"/>
        </xsl:call-template>
      </td>
    </tr>
    <tr style="line-height:11pt">
      <td style="{$pTDCSS}"  align="left">
        <!-- General Term -->
        <br/>
        <table style="line-height:11pt" cellpadding="0" cellspacing="0" align="left" border="0">
          <tr style="line-height:11pt">
            <td style="{$pTDCSS}" class="label">Trade Date :</td>
            <td style="{$pTDCSS}">

              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="//trade/tradeHeader/tradeDate"/>
              </xsl:call-template>

            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Buyer :</td>
            <td style="{$pTDCSS}">

              <xsl:call-template name="getPartyNameAndID" >
                <xsl:with-param name="pParty" select="$pFxOption/buyerPartyReference/@href" />
              </xsl:call-template>

            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Seller :</td>
            <td style="{$pTDCSS}">

              <xsl:call-template name="getPartyNameAndID" >
                <xsl:with-param name="pParty" select="$pFxOption/sellerPartyReference/@href" />
              </xsl:call-template>

            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Currency Option Style :</td>
            <td style="{$pTDCSS}">
              <xsl:if test="$IsEuropean=true()">
                <xsl:value-of select="$pFxOption/fxEuropeanTrigger/triggerCondition" /> At Expiration Binary Option
              </xsl:if>
              <xsl:if test="$IsAmerican=true()">
                <xsl:call-template name="getTouchCondition">
                  <xsl:with-param name="pTouchCondition" select="$pFxOption/fxAmericanTrigger/touchCondition"/>
                </xsl:call-template>
                Binary Option
              </xsl:if>

            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Currency pair :</td>
            <td style="{$pTDCSS}">

              <xsl:value-of select="$pFxOption/quotedCurrencyPair/currency1"/>/<xsl:value-of select="$pFxOption/quotedCurrencyPair/currency2"/>

            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Binary Payout :</td>
            <td style="{$pTDCSS}">

              <xsl:call-template name="format-money">
                <xsl:with-param name="currency" select="$pFxOption/triggerPayout/currency"/>
                <xsl:with-param name="amount" select="$pFxOption/triggerPayout/amount"/>
              </xsl:call-template>

            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Binary Level :</td>
            <td style="{$pTDCSS}">
              <table cellpadding="0" cellspacing="0" align="left" border="0" bgcolor="white">
                <tr>
                  <td style="{$pTDCSS}" colspan="2">
                    <xsl:if test="$IsEuropean=true()">
                      <xsl:call-template name="getTrigger">
                        <xsl:with-param name="pFxOptionTrigger" select="$pFxOption/fxEuropeanTrigger"/>
                      </xsl:call-template>
                      <br/>
                      <xsl:if test="$pFxOption/fxEuropeanTrigger/informationSource=true()">
                        <xsl:call-template name="getSource">
                          <xsl:with-param name="pFxOptionTrigger" select="$pFxOption/fxEuropeanTrigger"/>
                        </xsl:call-template>
                        <br/>
                        <xsl:if test="$pFxOption/fxEuropeanTrigger/informationSource/rateSourcePage=true()">
                          <xsl:call-template name="getPage">
                            <xsl:with-param name="pFxOptionTrigger" select="$pFxOption/fxEuropeanTrigger"/>
                          </xsl:call-template>
                        </xsl:if>
                        <br/>
                        <xsl:if test="$pFxOption/fxEuropeanTrigger/informationSource/rateSourcePageHeading=true()">
                          <xsl:call-template name="getHeading">
                            <xsl:with-param name="pFxOptionTrigger" select="$pFxOption/fxEuropeanTrigger"/>
                          </xsl:call-template>
                        </xsl:if>
                      </xsl:if>
                    </xsl:if>

                    <xsl:if test="$IsAmerican=true()">
                      <xsl:call-template name="getTrigger">
                        <xsl:with-param name="pFxOptionTrigger" select="$pFxOption/fxAmericanTrigger"/>
                      </xsl:call-template>
                      <br/>
                      <xsl:if test="$pFxOption/fxAmericanTrigger/informationSource=true()">
                        <xsl:call-template name="getSource">
                          <xsl:with-param name="pFxOptionTrigger" select="$pFxOption/fxAmericanTrigger"/>
                        </xsl:call-template>
                        <br/>
                        <xsl:if test="$pFxOption/fxAmericanTrigger/informationSource/rateSourcePage=true()">
                          <xsl:call-template name="getPage">
                            <xsl:with-param name="pFxOptionTrigger" select="$pFxOption/fxAmericanTrigger"/>
                          </xsl:call-template>
                        </xsl:if>
                        <br/>
                        <xsl:if test="$pFxOption/fxAmericanTrigger/informationSource/rateSourcePageHeading=true()">
                          <xsl:call-template name="getHeading">
                            <xsl:with-param name="pFxOptionTrigger" select="$pFxOption/fxAmericanTrigger"/>
                          </xsl:call-template>
                        </xsl:if>
                      </xsl:if>
                    </xsl:if>
                  </td>
                </tr>

                <xsl:if test="$IsAmerican=true()">
                  <xsl:if test="$pFxOption/fxAmericanTrigger/observationStartDate=true()">
                    <tr>
                      <td style="{$pTDCSS}">
                        Observation Start Date:
                      </td>
                      <td style="{$pTDCSS}">
                        <xsl:call-template name="format-date">
                          <xsl:with-param name="xsd-date-time" select="$pFxOption/fxAmericanTrigger/observationStartDate"/>
                        </xsl:call-template>
                      </td>
                    </tr>
                  </xsl:if>
                </xsl:if>

                <xsl:if test="$IsAmerican=true()">
                  <xsl:if test="$pFxOption/fxAmericanTrigger/observationEndDate=true()">
                    <tr>
                      <td style="{$pTDCSS}">
                        Observation End Date:
                      </td>
                      <td style="{$pTDCSS}">
                        <xsl:call-template name="format-date">
                          <xsl:with-param name="xsd-date-time" select="$pFxOption/fxAmericanTrigger/observationEndDate"/>
                        </xsl:call-template>
                      </td>
                    </tr>
                  </xsl:if>
                </xsl:if>
              </table>
            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Initial Spot Rate :</td>
            <td style="{$pTDCSS}">

              <xsl:if test="$pFxOption/spotRate=true()">
                <xsl:call-template name="format-fxrate">
                  <xsl:with-param name="fxrate" select="$pFxOption/spotRate"/>
                </xsl:call-template>
                <xsl:text>&#160;&#160;</xsl:text>
                <xsl:call-template name="format-currency-pair">
                  <xsl:with-param name="quotedCurrencyPair" select="$pFxOption/quotedCurrencyPair"/>
                </xsl:call-template>
              </xsl:if>
              <xsl:if test="$pFxOption/spotRate=false()">
                N/A
              </xsl:if>

            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Expiration Date :</td>
            <td style="{$pTDCSS}">

              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="$pFxOption/expiryDateTime/expiryDate"/>
              </xsl:call-template>

            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Expiration Time :</td>
            <td style="{$pTDCSS}">

              <xsl:call-template name="format-time">
                <xsl:with-param name="xsd-date-time" select="$pFxOption/expiryDateTime/expiryTime/hourMinuteTime"/>
              </xsl:call-template>
              <xsl:text> in </xsl:text>
              <xsl:call-template name="getFullNameBC">
                <xsl:with-param name="pBC" select="$pFxOption/expiryDateTime/expiryTime/businessCenter" />
              </xsl:call-template>
              <xsl:if test="$pFxOption/expiryDateTime/cutName=true()">
                <xsl:text>&#160;&#160;</xsl:text>
                (<xsl:value-of select="$pFxOption/expiryDateTime/cutName"/>)
              </xsl:if>

            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Settlement Date :</td>
            <td style="{$pTDCSS}">

              <xsl:if test="$pFxOption/triggerPayout/payoutStyle='Immediate' and $IsAmerican=true()">
                Two Business Days after the date that the Binary Event (if any) occurs.
              </xsl:if>
              <xsl:if test="$pFxOption/triggerPayout/payoutStyle='Deferred' or $IsEuropean=true()">
                <xsl:call-template name="format-date">
                  <xsl:with-param name="xsd-date-time" select="$pFxOption/valueDate"/>
                </xsl:call-template>
              </xsl:if>

            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Premium :</td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="format-money">
                <xsl:with-param name="currency" select="$pFxOption/fxOptionPremium/premiumAmount/currency"/>
                <xsl:with-param name="amount" select="$pFxOption/fxOptionPremium/premiumAmount/amount"/>
              </xsl:call-template>
            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Premium Payment Date:</td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="$pFxOption/fxOptionPremium/premiumSettlementDate"/>
              </xsl:call-template>
            </td>
          </tr>
        </table>
        <br/>
      </td>
    </tr>
  </xsl:template>
  <!-- 
	=========================================================================================================
	== FX Digital and Fx Option Leg Terms: END REGION used by (FxDigitalOption, FxOptionLeg [FxSimpleOption]) 
	==========================================================================================================
	-->
  <!-- 
	=========================================================================================================
	== Additional Definition: BEGIN REGION used by (FxDigitalOption) 
	==========================================================================================================
	-->
  <xsl:template name="displayAdditionalDefinitions">
    <tr>
      <td>
        <table cellpadding="1" align="left" border="0">
          <tr>
            <td>
              <p align="justify">
                <b>Additional definitions for the Transaction to which this Confirmation relates are as follows:</b>
                <br /><br />
                "Event Period" means the period commencing on the date and at the time this Currency Option is entered into and ending at the Expiration Time on the Expiration Date.
                <br /><br />
                "Binary Event" means that, at any time during the Event Period , the Spot Exchange Rate (in comparison to the Initial Spot Rate) is equal to or beyond the Binary Level, as determined by the Calculation Agent.
                <br /><br />
                "Spot Exchange Rate" means the price in the Spot Market for one or more actual foreign exchange transactions involving the Currency Pair (or cross-rates constituting such Currency Pair) which is the subject of this Currency Option as determined by the  Calculation Agent.
                <br /><br />
                "Spot Market" means the global spot foreign exchange market, open continuously from 5:00 a.m. Sydney time on a Monday in any week to 5:00 p.m. New York time on the Friday of that week.
              </p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
    <xsl:call-template name="BreakLine"/>
    <tr>
      <td>
        <table cellpadding="1" align="left" border="0">
          <tr>
            <td>
              <p align="justify">
                <b>Additional terms of the Transaction to which this Confirmation relates are as follows:</b>
                <br /><br />
                Notification of event: The Calculation Agent shall promptly notify the other Party (or Parties if the Calculation Agent is not a Party) of the occurrence of an event relating to this Currency Option.  A failure to give such notice shall not prejudice or invalidate the occurrence or effect of an event.
                <br /><br />
                Settlement: If a Binary Event has occurred, the Seller will pay the Buyer the Binary Payout on the Settlement Date.
              </p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </xsl:template>
  <!-- 
	=========================================================================================================
	== Additional Definition: END REGION used by (FxDigitalOption) 
	==========================================================================================================
	-->
  <!-- 
	==============================================================
	== FX Barrier Terms: BEGIN REGION used by (FxBarrierOption) 
	==============================================================
	-->
  <xsl:template name="displayFxBarrierOptionTerms">
    <xsl:param name="pFxOption"/>
    <xsl:param name="pIsNonDeliverable"/>
    <xsl:param name="pTDCSS"/>

    <xsl:variable name="IsAmerican" select="$pFxOption/valueDate='American'" />
    <xsl:variable name="IsEuropean" select="$pFxOption/valueDate='European'" />
    <xsl:variable name="IsBermuda" select="$pFxOption/valueDate='Bermuda'" />

    <tr style="line-height:15pt;">
      <td valign="middle" align="left" bgcolor="lavender" colspan="2">
        <xsl:call-template name="RoundedPanel">
          <xsl:with-param name="pTitle" select="'Terms'"/>
        </xsl:call-template>
      </td>
    </tr>
    <tr style="line-height:11pt">
      <td style="{$pTDCSS}"  align="left">
        <!-- General Term -->
        <br/>
        <table style="line-height:11pt" cellpadding="0" cellspacing="0" align="left" border="0">
          <tr style="line-height:11pt">
            <td style="{$pTDCSS}" class="label_nowidth">
              Trade Date :
            </td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="//trade/tradeHeader/tradeDate"/>
              </xsl:call-template>
            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Buyer :</td>
            <td style="{$pTDCSS}">

              <xsl:call-template name="getPartyNameAndID" >
                <xsl:with-param name="pParty" select="$pFxOption/buyerPartyReference/@href" />
              </xsl:call-template>

            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Seller :</td>
            <td style="{$pTDCSS}">

              <xsl:call-template name="getPartyNameAndID" >
                <xsl:with-param name="pParty" select="$pFxOption/sellerPartyReference/@href" />
              </xsl:call-template>

            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Currency Option Style :</td>
            <td style="{$pTDCSS}">

              <xsl:value-of select="$pFxOption/exerciseStyle"/>

            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Currency Option Type :</td>
            <td style="{$pTDCSS}">

              <xsl:value-of select="$pFxOption/putCurrencyAmount/currency"/> Put / <xsl:value-of select="$pFxOption/callCurrencyAmount/currency"/> Call

            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Call Currency :</td>
            <td style="{$pTDCSS}">

              <xsl:call-template name="format-money">
                <xsl:with-param name="currency" select="$pFxOption/callCurrencyAmount/currency"/>
                <xsl:with-param name="amount" select="$pFxOption/callCurrencyAmount/amount"/>
              </xsl:call-template>

            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Put Currency :</td>
            <td style="{$pTDCSS}">

              <xsl:call-template name="format-money">
                <xsl:with-param name="currency" select="$pFxOption/putCurrencyAmount/currency"/>
                <xsl:with-param name="amount" select="$pFxOption/putCurrencyAmount/amount"/>
              </xsl:call-template>

            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Strike Price :</td>
            <td style="{$pTDCSS}">

              <xsl:call-template name="format-fxrate">
                <xsl:with-param name="fxrate" select="$pFxOption/fxStrikePrice/rate" />
              </xsl:call-template>
              <xsl:text>&#160;&#160;</xsl:text>
              <xsl:call-template name="format-currency-pair">
                <xsl:with-param name="quotedCurrencyPair" select="$pFxOption/fxStrikePrice/strikeQuoteBasis" />
                <xsl:with-param name="tradeName" select="$pFxOption" />
              </xsl:call-template>

            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Expiration Date :</td>
            <td style="{$pTDCSS}">

              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="$pFxOption/expiryDateTime/expiryDate"/>
              </xsl:call-template>

            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Expiration Time :</td>
            <td style="{$pTDCSS}">

              <xsl:call-template name="format-shorttime">
                <xsl:with-param name="xsd-date-time" select="$pFxOption/expiryDateTime/expiryTime/hourMinuteTime"/>
              </xsl:call-template>
              <xsl:text> ( local time in </xsl:text>
              <xsl:call-template name="getFullNameBC">
                <xsl:with-param name="pBC" select="$pFxOption/expiryDateTime/expiryTime/businessCenter" />
              </xsl:call-template>
              <xsl:text> )</xsl:text>

            </td>
          </tr>
          <tr>
            <xsl:if test="$pFxOption/cashSettlementTerms=true()">
              <td style="{$pTDCSS}" class="label">Settlement :</td>
              <td style="{$pTDCSS}">

                <xsl:text>Non-Deliverable</xsl:text>

              </td>
            </xsl:if>
            <xsl:if test="$pFxOption/cashSettlementTerms=false()">
              <td style="{$pTDCSS}" class="label">Settlement :</td>
              <td style="{$pTDCSS}">

                <xsl:text>Deliverable</xsl:text>

              </td>
            </xsl:if>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Settlement Date :</td>
            <td style="{$pTDCSS}">

              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="$pFxOption/valueDate"/>
              </xsl:call-template>

            </td>
          </tr>
          <tr>
            <xsl:if test="$pFxOption/bermudanExerciseDates/date=true()">
              <td style="{$pTDCSS}" class="label">Specified Exercise Date(s) :</td>
              <xsl:for-each select="$pFxOption/bermudanExerciseDates/date">
                <tr>
                  <td style="{$pTDCSS}" class="label"> </td>
                  <td style="{$pTDCSS}">

                    <xsl:call-template name="format-date">
                      <xsl:with-param name="xsd-date-time" select="current()"/>
                    </xsl:call-template>

                  </td>
                </tr>
              </xsl:for-each>
            </xsl:if>
          </tr>
          <tr>
            <xsl:if test="$pFxOption/fxOptionPremium/premiumAmount/amount=true()">
              <td style="{$pTDCSS}" class="label">Premium :</td>
              <td style="{$pTDCSS}">

                <xsl:call-template name="format-money">
                  <xsl:with-param name="currency" select="$pFxOption/fxOptionPremium/premiumAmount/currency"/>
                  <xsl:with-param name="amount" select="$pFxOption/fxOptionPremium/premiumAmount/amount"/>
                </xsl:call-template>

              </td>
            </xsl:if>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Premium Payment Date:</td>
            <td style="{$pTDCSS}">

              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="$pFxOption/fxOptionPremium/premiumSettlementDate"/>
              </xsl:call-template>

            </td>
          </tr>
          <tr>
            <xsl:if test="$pFxOption/fxBarrier=true()">
              <td style="{$pTDCSS}" class="label">Barrier Event: </td>
              <td style="{$pTDCSS}">

                <xsl:text>Applicable</xsl:text>

              </td>

              <tr>
                <td style="{$pTDCSS}" class="label">Event Type: </td>
                <td style="{$pTDCSS}">
                  <xsl:call-template name="getBarrierEventType">
                    <xsl:with-param name="pEventType" select="$pFxOption/fxBarrier/fxBarrierType" />
                    <xsl:with-param name="pTotalBarrier" select="count($pFxOption/fxBarrier)" />
                  </xsl:call-template>
                </td>
              </tr>
              <xsl:if test="count($pFxOption/fxBarrier) = 1 ">
                <tr>
                  <td style="{$pTDCSS}" class="label">Spot Exchange Rate Direction: </td>
                  <td style="{$pTDCSS}">

                    <xsl:call-template name="getBarrierSpotExchangeRateDirection">
                      <xsl:with-param name="pEventType" select="$pFxOption/fxBarrier/fxBarrierType" />
                    </xsl:call-template>

                  </td>
                </tr>
              </xsl:if>
              <tr>
                <td style="{$pTDCSS}" class="label">Initial Spot Price: </td>
                <td style="{$pTDCSS}">

                  <xsl:call-template name="format-fxrate">
                    <xsl:with-param name="fxrate" select="$pFxOption/spotRate" />
                  </xsl:call-template>
                  <xsl:text>&#160;&#160;</xsl:text>
                  <xsl:call-template name="format-currency-pair">
                    <xsl:with-param name="quotedCurrencyPair" select="$pFxOption/fxStrikePrice/strikeQuoteBasis" />
                    <xsl:with-param name="tradeName" select="$pFxOption" />
                  </xsl:call-template>

                </td>
              </tr>
              <tr>
                <xsl:call-template name="displayBarrierLevel">
                  <xsl:with-param name="pFxOption" select="$pFxOption" />
                  <xsl:with-param name="pTotalBarrier" select="count($pFxOption/fxBarrier)" />
                </xsl:call-template>
              </tr>
              <xsl:if test="$pFxOption/fxBarrier/observationStartDate=true()">
                <tr>
                  <td style="{$pTDCSS}" class="label">Event Period Start Date and Time:</td>
                  <td style="{$pTDCSS}">

                    <xsl:call-template name="format-date">
                      <xsl:with-param name="xsd-date-time" select="$pFxOption/fxBarrier/observationStartDate"/>
                    </xsl:call-template>
                    <xsl:text> 00:00:00 pm ( local time in </xsl:text>
                    <xsl:call-template name="getFullNameBC">
                      <xsl:with-param name="pBC" select="$pFxOption/expiryDateTime/expiryTime/businessCenter" />
                    </xsl:call-template>
                    <xsl:text> )</xsl:text>

                  </td>
                </tr>
              </xsl:if>
              <xsl:if test="$pFxOption/fxBarrier/observationEndDate=true()">
                <tr>
                  <td style="{$pTDCSS}" class="label">Event Period End Date and Time:</td>
                  <td style="{$pTDCSS}">

                    <xsl:call-template name="format-date">
                      <xsl:with-param name="xsd-date-time" select="$pFxOption/fxBarrier/observationEndDate"/>
                    </xsl:call-template>

                    <xsl:text> 00:00:00 pm ( local time in </xsl:text>
                    <xsl:call-template name="getFullNameBC">
                      <xsl:with-param name="pBC" select="$pFxOption/expiryDateTime/expiryTime/businessCenter" />
                    </xsl:call-template>
                    <xsl:text> )</xsl:text>
                  </td>
                </tr>
              </xsl:if>
            </xsl:if>
          </tr>
        </table>
      </td>
    </tr>
  </xsl:template>
  <!-- 
	============================================================
	== FX Barrier Terms: END REGION used by (FxBarrierOption) 
	============================================================
	-->
  <!-- 
	============================================================
	== Barrier Level: BEGIN REGION used by (FxBarrierOption)
	== call from template:displayFxBarrierOptionTerms
	============================================================
	-->
  <xsl:template name="displayBarrierLevel">
    <xsl:param name="pFxOption"/>
    <xsl:param name="pTotalBarrier"/>
    <xsl:param name="pTDCSS"/>
    <!-- Traitement du cas ou il existe qu'une seule Barrière -->
    <xsl:if test="$pTotalBarrier &lt; 2">
      <td style="{$pTDCSS}" class="label">Barrier Level: </td>
      <td style="{$pTDCSS}">
        <!-- Affichage du montant de la barrier-level -->
        <xsl:call-template name="format-fxrate">
          <xsl:with-param name="fxrate" select="$pFxOption/fxBarrier/triggerRate" />
        </xsl:call-template>
        <xsl:text>&#160;&#160;</xsl:text>
        <!-- Affichage du couple de la devise -->
        <xsl:call-template name="format-currency-pair">
          <xsl:with-param name="quotedCurrencyPair" select="$pFxOption/fxBarrier/quotedCurrencyPair" />
          <xsl:with-param name="tradeName" select="$pFxOption" />
        </xsl:call-template>
      </td>
    </xsl:if>
    <!-- Traitement du cas ou il existe plusieurs niveaux de Barrières -->
    <xsl:if test="$pTotalBarrier &gt; 1">
      <xsl:for-each select="$pFxOption/fxBarrier">
        <xsl:sort select="current()/triggerRate" order="descending" />

        <!-- Affichage de la Barrière la plus haute -->
        <xsl:if test="position() = 1 ">
          <tr>
            <td style="{$pTDCSS}" class="label">Upper Barrier Level: </td>
            <td style="{$pTDCSS}">
              <!-- Affichage du montant de la barrier-level -->
              <xsl:call-template name="format-fxrate">
                <xsl:with-param name="fxrate" select="current()/triggerRate" />
              </xsl:call-template>
              <xsl:text>&#160;&#160;</xsl:text>
              <!-- Affichage du couple de la devise -->
              <xsl:call-template name="format-currency-pair">
                <xsl:with-param name="quotedCurrencyPair" select="current()/quotedCurrencyPair" />
              </xsl:call-template>
            </td>
          </tr>
        </xsl:if>

        <!-- Affichage de la Barrière la plus basse -->
        <xsl:if test="position() = last() ">
          <td style="{$pTDCSS}" class="label">Lower Barrier Level: </td>
          <td style="{$pTDCSS}">
            <!-- Affichage du montant de la barrier-level -->
            <xsl:call-template name="format-fxrate">
              <xsl:with-param name="fxrate" select="current()/triggerRate" />
            </xsl:call-template>
            <xsl:text>&#160;&#160;</xsl:text>
            <!-- Affichage du couple de la devise -->
            <xsl:call-template name="format-currency-pair">
              <xsl:with-param name="quotedCurrencyPair" select="current()/quotedCurrencyPair" />
            </xsl:call-template>
          </td>
        </xsl:if>

      </xsl:for-each>
    </xsl:if>
  </xsl:template>
  <!-- 
	============================================================
	== Barrier Level: END REGION used by (FxBarrierOption)
	============================================================
	-->
  <!-- 
	==============================================================
	== FX OptionLeg Terms: BEGIN REGION used by (FxOptionLeg) 
	==============================================================
	-->
  <xsl:template name="displayFxOptionLegTerms">
    <xsl:param name="pFxOption"/>
    <xsl:param name="pIsNonDeliverable"/>
    <xsl:param name="pTDCSS"/>

    <xsl:variable name="IsAmerican" select="$pFxOption/valueDate='American'" />
    <xsl:variable name="IsEuropean" select="$pFxOption/valueDate='European'" />
    <xsl:variable name="IsBermuda" select="$pFxOption/valueDate='Bermuda'" />
    <tr style="line-height:15pt;">
      <td valign="middle" align="left" bgcolor="lavender" colspan="2">
        <xsl:call-template name="RoundedPanel">
          <xsl:with-param name="pTitle" select="'Terms'"/>
        </xsl:call-template>
      </td>
    </tr>
    <tr>
      <td>
        <br/>
        <!-- General Term -->
        <table style="line-height:11pt" cellpadding="0" cellspacing="0" align="left" border="0">
          <tr>
            <td style="{$pTDCSS}" class="label">Trade Date :</td>
            <td style="{$pTDCSS}">

              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="//trade/tradeHeader/tradeDate"/>
              </xsl:call-template>

            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Buyer :</td>
            <td style="{$pTDCSS}">

              <xsl:call-template name="getPartyNameAndID" >
                <xsl:with-param name="pParty" select="$pFxOption/buyerPartyReference/@href" />
              </xsl:call-template>

            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Seller :</td>
            <td style="{$pTDCSS}" >

              <xsl:call-template name="getPartyNameAndID" >
                <xsl:with-param name="pParty" select="$pFxOption/sellerPartyReference/@href" />
              </xsl:call-template>

            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Currency Option Style :</td>
            <td style="{$pTDCSS}" >
              <xsl:value-of select="$pFxOption/exerciseStyle"/>
            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Currency Option Type :</td>
            <td style="{$pTDCSS}">

              <xsl:value-of select="$pFxOption/putCurrencyAmount/currency"/> Put / <xsl:value-of select="$pFxOption/callCurrencyAmount/currency"/> Call

            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Call Currency :</td>
            <td style="{$pTDCSS}">

              <xsl:call-template name="format-money">
                <xsl:with-param name="currency" select="$pFxOption/callCurrencyAmount/currency"/>
                <xsl:with-param name="amount" select="$pFxOption/callCurrencyAmount/amount"/>
              </xsl:call-template>

            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Put Currency :</td>
            <td style="{$pTDCSS}" >

              <xsl:call-template name="format-money">
                <xsl:with-param name="currency" select="$pFxOption/putCurrencyAmount/currency"/>
                <xsl:with-param name="amount" select="$pFxOption/putCurrencyAmount/amount"/>
              </xsl:call-template>

            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}"  class="label">Strike Price :</td>
            <td style="{$pTDCSS}" >

              <xsl:call-template name="format-fxrate">
                <xsl:with-param name="fxrate" select="$pFxOption/fxStrikePrice/rate" />
              </xsl:call-template>
              <xsl:text>&#160;&#160;</xsl:text>
              <xsl:call-template name="format-currency-pair">
                <xsl:with-param name="quotedCurrencyPair" select="$pFxOption/fxStrikePrice/strikeQuoteBasis" />
                <xsl:with-param name="tradeName" select="$pFxOption" />
              </xsl:call-template>

            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}"  class="label">Expiration Date :</td>
            <td style="{$pTDCSS}" >

              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="$pFxOption/expiryDateTime/expiryDate"/>
              </xsl:call-template>

            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}"  class="label">Expiration Time :</td>
            <td style="{$pTDCSS}" >

              <xsl:call-template name="format-shorttime">
                <xsl:with-param name="xsd-date-time" select="$pFxOption/expiryDateTime/expiryTime/hourMinuteTime"/>
              </xsl:call-template>
              <xsl:text> ( local time in </xsl:text>
              <xsl:call-template name="getFullNameBC">
                <xsl:with-param name="pBC" select="$pFxOption/expiryDateTime/expiryTime/businessCenter" />
              </xsl:call-template>
              <xsl:text> )</xsl:text>

            </td>
          </tr>
          <tr>
            <xsl:if test="$pFxOption/cashSettlementTerms=true()">
              <td style="{$pTDCSS}"  class="label">Settlement :</td>
              <td style="{$pTDCSS}" >

                <xsl:text>Non-Deliverable</xsl:text>

              </td>
            </xsl:if>
          </tr>
          <tr>
            <td style="{$pTDCSS}"  class="label">Settlement Date :</td>
            <td style="{$pTDCSS}" >

              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="$pFxOption/valueDate"/>
              </xsl:call-template>

            </td>
          </tr>
          <tr>
            <xsl:if test="$pFxOption/bermudanExerciseDates/date=true()">
              <td style="{$pTDCSS}" class="label">Specified Exercise Date(s) :</td>
              <xsl:for-each select="$pFxOption/bermudanExerciseDates/date">
                <tr>
                  <td style="{$pTDCSS}" class="label"> </td>
                  <td style="{$pTDCSS}">

                    <xsl:call-template name="format-date">
                      <xsl:with-param name="xsd-date-time" select="current()"/>
                    </xsl:call-template>

                  </td>
                </tr>
              </xsl:for-each>
            </xsl:if>
          </tr>
          <tr>
            <xsl:if test="$pFxOption/fxOptionPremium/premiumAmount/amount=true()">
              <td style="{$pTDCSS}" class="label">Premium :</td>
              <td style="{$pTDCSS}" >

                <xsl:call-template name="format-money">
                  <xsl:with-param name="currency" select="$pFxOption/fxOptionPremium/premiumAmount/currency"/>
                  <xsl:with-param name="amount" select="$pFxOption/fxOptionPremium/premiumAmount/amount"/>
                </xsl:call-template>

              </td>
            </xsl:if>
          </tr>
          <tr>
            <xsl:if test="$pFxOption/fxOptionPremium/premiumQuote/premiumValue=true()">
              <td style="{$pTDCSS}" class="label">Price :</td>
              <td style="{$pTDCSS}">
                <xsl:if test="not( $pFxOption/fxOptionPremium/premiumQuote/premiumQuoteBasis = 'Explicit' )">
                  <xsl:call-template name="format-fixed-rate">
                    <xsl:with-param name="fixed-rate" select="$pFxOption/fxOptionPremium/premiumQuote/premiumValue" />
                  </xsl:call-template>

                  <xsl:text> ( </xsl:text>
                  <xsl:call-template name="getPremiumQuoteBasis">
                    <xsl:with-param name="CurrencyPairLabel" select="$pFxOption/fxOptionPremium/premiumQuote/premiumQuoteBasis" />
                  </xsl:call-template>
                  <xsl:text> )</xsl:text>
                </xsl:if>

                <xsl:if test="$pFxOption/fxOptionPremium/premiumQuote/premiumQuoteBasis = 'Explicit' ">
                  <xsl:call-template name="format-fxrate">
                    <xsl:with-param name="fxrate" select="$pFxOption/fxOptionPremium/premiumQuote/premiumValue" />
                  </xsl:call-template>
                </xsl:if>
              </td>
            </xsl:if>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Premium Payment Date:</td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="$pFxOption/fxOptionPremium/premiumSettlementDate"/>
              </xsl:call-template>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </xsl:template>
  <!-- 
	==============================================================
	== FX OptionLeg Terms: END REGION used by (FxOptionLeg) 
	==============================================================
	-->
  <!-- 
	==============================================================
	== Exchanged Rate: BEGIN REGION not used  
	==============================================================

	<xsl:template name="displayExchangedRate">
		<xsl:param name="pFxSingleLeg"/>
		<xsl:param name="pTDCSS"/>
		<tr>
			<td style="{$pTDCSS}" class="label">Rate :</td>
			<td style="{$pTDCSS}">
				<xsl:choose>
					<xsl:when test="//trade/fxSingleLeg/exchangeRate/quotedCurrencyPair/quoteBasis='Currency2PerCurrency1'">
						<xsl:value-of select="//trade/fxSingleLeg/exchangeRate/quotedCurrencyPair/currency2" />
						/
						<xsl:value-of select="//trade/fxSingleLeg/exchangeRate/quotedCurrencyPair/currency1" />
					</xsl:when>
					<xsl:when test="//trade/fxSingleLeg/exchangeRate/quotedCurrencyPair/quoteBasis='Currency1PerCurrency2'">
						<xsl:value-of select="//trade/fxSingleLeg/exchangeRate/quotedCurrencyPair/currency1" />
						/
						<xsl:value-of select="//trade/fxSingleLeg/exchangeRate/quotedCurrencyPair/currency2" />
					</xsl:when>
					<xsl:otherwise>
					</xsl:otherwise>
				</xsl:choose>
				&#160;
				<xsl:value-of select="format-number(//trade/fxSingleLeg/exchangeRate/rate, $amountPattern, $defaultCulture)" />
			</td>
		</tr>
	</xsl:template>
	
	==============================================================
	== Exchanged Rate: END REGION not used  
	==============================================================
	-->
  <!-- 
	===============================================
	===============================================
	== FX product's family: END REGION        =====
	===============================================
	===============================================
	-->








  <!-- 
	===============================================
	===============================================
	== EQD product's family: BEGIN REGION     =====
	===============================================
	===============================================
	-->
  <!-- Display the text of the letter for all EQD product's family -->
  <xsl:template name="displayEqdConfirmationText">
    <xsl:param name="pTDCSS"/>
    <tr>
      <td>
        <table cellpadding="0" cellspacing="0" align="left" border="0">
          <tr>
            <td style="{$pTDCSS}">
              <p align="justify" style="line-height:10pt">
                <br/>
                <xsl:text>Dear Sir or Madam,</xsl:text>
                <br/>
                <xsl:text>The purpose of this letter agreement (this "Confirmation") is to confirm the terms and conditions </xsl:text>
                <xsl:text>of the Transaction entered into between us on the Trade Date specified below (the "Transaction").</xsl:text>
                <xsl:text>This Confirmation constitutes a "Confirmation" as referred to in the Agreement specified below.</xsl:text>
                <br/>
                <br/>
                <xsl:text>The definitions and provisions contained in the 2002 ISDA Equity Derivatives Definitions (the "Equity </xsl:text>
                <xsl:text>Definitions"), as published by the International Swaps and Derivatives Association, Inc., are </xsl:text>
                <xsl:text>incorporated into this Confirmation. </xsl:text>
                <xsl:text>In the event of any inconsistency between the Equity Definitions and this Confirmation, this Confirmation will govern. </xsl:text>
                <br/>
                <br/>
                <xsl:text>This Confirmation supplements, forms a part of, and is subject to, the ISDA Master Agreement </xsl:text>
                <xsl:if test="$isValidMasterAgreementDate">
                  <xsl:text>dated as of</xsl:text>
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="xsd-date-time" select="$masterAgreementDtSignature"/>
                  </xsl:call-template>
                </xsl:if>,
                <xsl:text>as amended and supplemented from time to time (the "Agreement"), between &#xa;</xsl:text>
                <xsl:value-of select="$varParty_1_name" /><xsl:text> and </xsl:text><xsl:value-of select="$varParty_2_name" />.
                <br/>
                <br/>
                <xsl:text>All provisions contained in the Agreement govern this Confirmation except as expressly modified below.</xsl:text>
              </p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </xsl:template>
  <!-- 
	==============================================================
	== Equity Option Terms: BEGIN REGION used by (Equity Option) 
	==============================================================
	-->
  <xsl:template name="displayEqdTerms">
    <xsl:param name="pEquityOption"/>
    <xsl:param name="pTDCSS"/>

    <tr style="line-height:15pt;">
      <td valign="middle" align="left" bgcolor="lavender" colspan="2">
        <xsl:call-template name="RoundedPanel">
          <xsl:with-param name="pTitle" select="'Terms'"/>
        </xsl:call-template>
      </td>
    </tr>
    <tr style="line-height:11pt">
      <td style="{$pTDCSS}"  align="left">
        <!-- General Term -->
        <br/>
        <table style="line-height:11pt" cellpadding="0" cellspacing="0" align="left" border="0">
          <tr style="line-height:11pt">
            <td style="{$pTDCSS}" class="label_nowidth">
              Trade Date :
            </td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="format-date">
                <xsl:with-param name="xsd-date-time" select="//trade/tradeHeader/tradeDate"/>
              </xsl:call-template>
            </td>
          </tr>

          <tr>
            <td style="{$pTDCSS}" class="label">Option Style :</td>
            <td style="{$pTDCSS}">
              <xsl:if test="//dataDocument/trade/equityOption/equityExercise/equityAmericanExercise">American</xsl:if>
              <xsl:if test="//dataDocument/trade/equityOption/equityExercise/equityBermudaExercise">Bermuda</xsl:if>
              <xsl:if test="//dataDocument/trade/equityOption/equityExercise/equityEuropeanExercise">European</xsl:if>
            </td>
          </tr>
          <tr>
            <td class="label">Option Type :</td>
            <td>
              <xsl:value-of select="//dataDocument/trade/equityOption/optionType"/>
            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Seller :</td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="getPartyNameAndID" >
                <xsl:with-param name="pParty" select="$pEquityOption/sellerPartyReference/@href" />
              </xsl:call-template>
            </td>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Buyer :</td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="getPartyNameAndID" >
                <xsl:with-param name="pParty" select="$pEquityOption/buyerPartyReference/@href" />
              </xsl:call-template>
            </td>
          </tr>
          <xsl:if test="//dataDocument/trade/equityOption/underlyer/singleUnderlyer/equity">
            <tr>
              <td style="{$pTDCSS}" class="label">Shares :</td>
              <td style="{$pTDCSS}">
                <xsl:value-of select="//dataDocument/trade/equityOption/underlyer/singleUnderlyer/equity/description"/>
              </td>
            </tr>
          </xsl:if>
          <xsl:if test="//dataDocument/trade/equityOption/underlyer/singleUnderlyer/index">
            <tr>
              <td style="{$pTDCSS}" class="label">Index :</td>
              <td style="{$pTDCSS}">
                <xsl:value-of select="//dataDocument/trade/equityOption/underlyer/singleUnderlyer/index/description"/>
              </td>
            </tr>
          </xsl:if>
          <tr>
            <td style="{$pTDCSS}" class="label">Number of Options :</td>
            <td style="{$pTDCSS}">
              <xsl:value-of select="//dataDocument/trade/equityOption/numberOfOptions"/>
            </td>
          </tr>
          <xsl:if test="//dataDocument/trade/equityOption/underlyer/singleUnderlyer/equity">
            <tr>
              <td style="{$pTDCSS}" class="label">Option Entitlement :</td>
              <td style="{$pTDCSS}">
                <xsl:value-of select="//dataDocument/trade/equityOption/optionEntitlement"/>&#160;
                <xsl:text>Share(s) per Option</xsl:text>
              </td>
            </tr>
          </xsl:if>
          <tr>
            <td style="{$pTDCSS}" class="label">Strike Price :</td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="getEquityStrike"/>
            </td>
          </tr>
          <tr>
            <xsl:call-template name="displayEqdPremium" >
              <xsl:with-param name="pPremium" select="//dataDocument/trade/equityOption/equityPremium"/>
              <xsl:with-param name="pTDCSS" select="$pTDCSS"/>
            </xsl:call-template>
          </tr>
          <tr>
            <td style="{$pTDCSS}" class="label">Exchange :</td>
            <td style="{$pTDCSS}">
              <xsl:call-template name="getFullNameMIC">
                <xsl:with-param name="pMIC" select="//exchangeId"/>
              </xsl:call-template>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </xsl:template>
  <!-- 
	==============================================================
	== Equity Option Terms: END REGION used by (Equity Option) 
	==============================================================
	-->
  <!-- 
	==============================================================
	== Equity Option Premium: BEGIN REGION used by (Equity Option) 
	==============================================================
	-->
  <xsl:template name="displayEqdPremium">
    <xsl:param name="pPremium" />
    <xsl:param name="pTDCSS"/>

    <xsl:call-template name="debugXsl">
      <xsl:with-param name="pCurrentNode" select="$pPremium"/>
    </xsl:call-template>

    <xsl:for-each select="$pPremium">
      <tr>
        <td style="{$pTDCSS}" class="label">Premium :</td>
        <td style="{$pTDCSS}">
          <xsl:call-template name="debugXsl">
            <xsl:with-param name="pCurrentNode" select="paymentAmount/currency"/>
          </xsl:call-template>
          <xsl:value-of select="paymentAmount/currency" />&#160;
          <xsl:value-of select="format-number(paymentAmount/amount, $amountPattern, $defaultCulture)" />
        </td>
      </tr>
      <tr>
        <td style="{$pTDCSS}" class="label">Premium Payment Date :</td>
        <td style="{$pTDCSS}">
          <xsl:call-template name="getEqdPremiumPaymentDate"/>

        </td>
      </tr>
    </xsl:for-each>
  </xsl:template>
  <!-- 
	==============================================================
	== Equity Option Premium: END REGION used by (Equity Option) 
	==============================================================
	-->
  <!-- 
	===========================================================================
	== Equity Option Procedure Exercise: BEGIN REGION used by (Equity Option) 
	===========================================================================
	-->
  <xsl:template name="displayEqdExerciseProcedure">
    <xsl:param name="pProduct" />
    <xsl:param name="pTDCSS"/>

    <tr style="line-height:15pt">
      <td valign="middle" align="left" bgcolor="lavender" colspan="2">
        <xsl:call-template name="RoundedPanel">
          <xsl:with-param name="pTitle" select="'Procedure for Exercise'"/>
        </xsl:call-template>
      </td>
    </tr>
    <tr style="line-height:11pt">
      <td>
        <br />
        <table style="line-height:11pt" cellpadding="0" cellspacing="0" align="left" border="0">

          <!-- Procedure for Bermuda Exercise -->
          <xsl:if test="$pProduct/equityBermudaExercise">
            <tr>
              <td style="{$pTDCSS}" valign="top" class="label">Bermuda Option Exercise Dates:</td>
              <td style="{$pTDCSS}">
                <xsl:call-template name="getAdjustableOrRelativeDates" >
                  <xsl:with-param name="pSeparator">
                    <br/>
                    <xsl:text>&#xa;</xsl:text>
                  </xsl:with-param>
                  <xsl:with-param name="pDate" select="$pProduct/equityBermudaExercise/bermudaExerciseDates"/>
                </xsl:call-template>
              </td>
            </tr>
            <tr>
              <td style="{$pTDCSS}" class="label">Earliest Exercise Time:</td>
              <td style="{$pTDCSS}">
                <xsl:value-of select="//equityBermudaExercise/earliestExerciseTime/hourMinuteTime"/>
                <xsl:text>&#xa;</xsl:text>
                <xsl:call-template name="getFullNameBC">
                  <xsl:with-param name="pBC" select="//equityBermudaExercise/earliestExerciseTime/businessCenter"/>
                </xsl:call-template>
                <!--
								<xsl:call-template name="getBusinessCenterList">
									<xsl:with-param name="pBusinessCenters" select="//equityBermudaExercise/earliestExerciseTime"/>
								</xsl:call-template>
								-->
              </td>
            </tr>
            <tr>
              <td style="{$pTDCSS}" class="label">Latest Exercise Time:</td>
              <td style="{$pTDCSS}">
                <xsl:value-of select="//equityBermudaExercise/latestExerciseTime/hourMinuteTime"/>
                <xsl:text>&#xa;</xsl:text>
                <xsl:call-template name="getFullNameBC">
                  <xsl:with-param name="pBC" select="//equityBermudaExercise/latestExerciseTime/businessCenter"/>
                </xsl:call-template>
                <!--
								<xsl:call-template name="getBusinessCenterList">
									<xsl:with-param name="pBusinessCenters" select="//equityBermudaExercise/latestExerciseTime"/>
								</xsl:call-template>
								-->
              </td>
            </tr>
          </xsl:if>

          <!--Procedure for American Exercise: to testing with an american swaption xml  -->
          <xsl:if test="$pProduct/equityAmericanExercise">
            <tr>
              <td style="{$pTDCSS}" class="label">Commencement Date:</td>
              <td style="{$pTDCSS}">
                <xsl:call-template name="getAdjustableOrRelativeDateSequence" >
                  <xsl:with-param name="pSeparator">
                    <br/>
                    <xsl:text>&#xa;</xsl:text>
                  </xsl:with-param>
                  <xsl:with-param name="pDate" select="$pProduct/equityAmericanExercise/commencementDate"/>
                </xsl:call-template>
              </td>
            </tr>
            <tr>
              <td style="{$pTDCSS}" class="label">Expiration Date:</td>
              <td style="{$pTDCSS}">
                <xsl:call-template name="getAdjustableOrRelativeDateSequence" >
                  <xsl:with-param name="pDate" select="$pProduct/equityAmericanExercise/expirationDate"/>
                  <xsl:with-param name="pSeparator">
                    <br/>
                    <xsl:text>&#xa;</xsl:text>
                  </xsl:with-param>
                </xsl:call-template>
              </td>
            </tr>
            <tr>
              <td style="{$pTDCSS}" class="label">Expiration Time:</td>
              <td style="{$pTDCSS}">
                <xsl:value-of select="//equityAmericanExercise/equityExpirationTime/hourMinuteTime"/>
                <xsl:text>&#xa;</xsl:text>
                <xsl:call-template name="getFullNameBC">
                  <xsl:with-param name="pBC" select="//equityAmericanExercise/equityExpirationTime/businessCenter"/>
                </xsl:call-template>
                <!--
								<xsl:call-template name="getBusinessCenterList">
									<xsl:with-param name="pBusinessCenters" select="//equityAmericanExercise/expirationTime"/>
								</xsl:call-template>
								-->
              </td>
            </tr>
          </xsl:if>

          <!--Procedure for European Exercise -->
          <xsl:if test="$pProduct/equityEuropeanExercise">
            <tr>
              <td style="{$pTDCSS}" class="label">Expiration Date:</td>
              <td style="{$pTDCSS}">
                <xsl:call-template name="getAdjustableOrRelativeDateSequence" >
                  <xsl:with-param name="pDate" select="$pProduct/equityEuropeanExercise/expirationDate"/>
                  <xsl:with-param name="pSeparator">
                    <br/>
                    <xsl:text>&#xa;</xsl:text>
                  </xsl:with-param>
                </xsl:call-template>
              </td>
            </tr>
            <tr>
              <td style="{$pTDCSS}" class="label">Expiration Time:</td>
              <td style="{$pTDCSS}">
                <xsl:value-of select="//equityEuropeanExercise/equityExpirationTime/hourMinuteTime"/>
                <xsl:text>&#xa;</xsl:text>
                <xsl:call-template name="getFullNameBC">
                  <xsl:with-param name="pBC" select="//equityEuropeanExercise/equityExpirationTime/businessCenter"/>
                </xsl:call-template>
                <!--
								<xsl:call-template name="getBusinessCenterList">
									<xsl:with-param name="pBusinessCenters" select="//equityEuropeanExercise/equityExpirationTime/businessCenter"/>
								</xsl:call-template>
								-->
              </td>
            </tr>
          </xsl:if>
        </table>
        <br/>
      </td>
    </tr>
  </xsl:template>
  <!-- 
	===========================================================================
	== Equity Option Procedure Exercise: END REGION used by (Equity Option) 
	===========================================================================
	-->
  <!-- 
	===========================================================================
	== Equity Option Settlement Terms: BEGIN REGION used by (Equity Option) 
	===========================================================================
	-->
  <xsl:template name="displayEqdSettlementTerms">
    <xsl:param name="pTDCSS"/>
    <tr style="line-height:15pt">
      <td valign="middle" align="left" bgcolor="lavender" colspan="2">
        <xsl:call-template name="RoundedPanel">
          <xsl:with-param name="pTitle" select="'Settlement Terms'"/>
        </xsl:call-template>
      </td>
    </tr>
    <tr>
      <td>
        <br/>
        <table style="line-height:11pt" cellpadding="0" cellspacing="0" align="left" border="0">

          <xsl:choose>
            <xsl:when test="//settlementType ='Cash'">
              <tr>
                <td style="{$pTDCSS}" class="label">Cash Settlement :</td>
                <td style="{$pTDCSS}">
                  Applicable
                </td>
              </tr>
              <tr>
                <td style="{$pTDCSS}" class="label">Settlement Currency : </td>
                <td style="{$pTDCSS}">
                  <xsl:value-of select="//equityExercise/settlementCurrency"/>
                </td>
              </tr>
              <tr>
                <td style="{$pTDCSS}" class="label">Settlement Price : </td>
                <td style="{$pTDCSS}">
                  <xsl:value-of select="//equityExercise/settlementPriceSource"/>
                </td>
              </tr>

              <tr>
                <td style="{$pTDCSS}" class="label">Cash Settlement Payment Date : </td>
                <td style="{$pTDCSS}">
                  <xsl:value-of select="$getEqdCashSettlementAdjustableOrRelativeDates"/>
                </td>
              </tr>
            </xsl:when>

            <xsl:when test="//settlementType ='Physical'">
              <tr>
                <td style="{$pTDCSS}" class="label">Physical Settlement :</td>
                <td style="{$pTDCSS}">
                  Applicable
                </td>
              </tr>
              <tr>
                <td style="{$pTDCSS}" class="label">Settlement Currency : </td>
                <td style="{$pTDCSS}">
                  <xsl:value-of select="//equityExercise/settlementCurrency"/>
                </td>
              </tr>
            </xsl:when>
            <xsl:otherwise>
              <tr>
                <td style="{$pTDCSS}" class="label">Settlement:</td>
                <td style="{$pTDCSS}">Error</td>
              </tr>
            </xsl:otherwise>
          </xsl:choose>
        </table>
      </td>
    </tr>
  </xsl:template>
  <!-- 
	===========================================================================
	== Equity Option Settlement Terms: END REGION used by (Equity Option) 
	===========================================================================
	-->
  <!-- 
	===============================================
	===============================================
	== EQD product's family: END REGION       =====
	===============================================
	===============================================
	-->
</xsl:stylesheet>