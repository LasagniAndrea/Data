<?xml version="1.0" encoding="UTF-8" ?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:dt="http://xsltsl.org/date-time" xmlns:fo="http://www.w3.org/1999/XSL/Format"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:efs="http://www.efs.org/2007/EFSmL-3-0" xmlns:fixml="http://www.fixprotocol.org/FIXML-5-0-SP1" 
                xmlns:fpml="http://www.fpml.org/2007/FpML-4-4">

  <xsl:output method="xml" encoding="UTF-8" />
  <!-- xslt includes -->
  <xsl:include href="TIMS_CCG_Common.xslt" />
  <!-- ***************************************** -->
  <!-- Global Parameters *********************** -->
  <!-- ***************************************** -->
  <xsl:param name="pCurrentCulture" select="'en-GB'" />

  <!-- ========================== -->
  <!-- Graphic Templates          -->
  <!-- ========================== -->

  <!-- *********************** -->
  <!-- Draw PDF page (Match)   -->
  <!-- *********************** -->
  <xsl:template match="/">
    <fo:root xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <xsl:call-template name="setPagesCaracteristics"/>
      <fo:page-sequence master-reference="A4LandscapePage" initial-page-number="1" font-family="{$vFontFamily}">
        <fo:static-content flow-name="A4LandscapeHeader">
          <xsl:call-template name="displayHeader"/>
        </fo:static-content>
        <fo:static-content flow-name="A4LandscapeFooter">
          <xsl:call-template name="displayFooter"/>
        </fo:static-content>
        <fo:flow flow-name="A4LandscapeBody">
          <xsl:call-template name="displayBody"/>
          <fo:block id="EndOfDoc" />
        </fo:flow>
      </fo:page-sequence>
    </fo:root>
  </xsl:template>

  <!-- *********************** -->
  <!-- Draw Header             -->
  <!-- *********************** -->
  <xsl:template name="displayHeader">
    <fo:block linefeed-treatment="preserve" font-size="{$vBlockFontSize}">
      <!-- ************************* -->
      <!-- *** 1° row ************** -->
      <!-- ************************* -->
      <fo:table table-layout="fixed">
        <fo:table-column column-width="{1}cm" column-number="01"/>
        <fo:table-column column-width="{1.3}cm" column-number="02"/>
        <fo:table-column column-width="{10}cm" column-number="03"/>
        <fo:table-column column-width="{7}cm" column-number="04"/>
        <fo:table-column column-width="{1.5}cm" column-number="05"/>
        <fo:table-column column-width="{5}cm" column-number="06"/>
        <fo:table-column column-width="{0.2}cm" column-number="07"/>
        <fo:table-column column-width="{2.5}cm" column-number="08"/>
        <fo:table-body>
          <fo:table-row height="{$vRowHeight}cm">
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}" text-align="right">
                <!--This field is intentionally left blank-->
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}" text-align="left">
                <xsl:value-of select ="$vLabelAder"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}" text-align="left">
                <xsl:call-template name ="getEntity"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}" text-align="left">
                <xsl:value-of select ="$vLabelComp"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}" text-align="right">
                <xsl:value-of select ="$vLabelReportName"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}" text-align="right">
                <xsl:call-template name ="getDateTime"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}" text-align="right">
                <xsl:text></xsl:text>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}" text-align="right">
                <fo:page-number/>/<fo:page-number-citation ref-id="EndOfDoc"/>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
      <!-- ************************* -->
      <!-- *** 2° row ************** -->
      <!-- ************************* -->
      <fo:table table-layout="fixed">
        <fo:table-column column-width="{1}cm" column-number="01"/>
        <fo:table-column column-width="{1.3}cm" column-number="02"/>
        <fo:table-column column-width="{10}cm" column-number="03"/>
        <fo:table-column column-width="{15.2}cm" column-number="04"/>
        <fo:table-body>
          <fo:table-row height="{$vRowHeight}cm" >
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}" text-align="right">
                <!-- This field is intentionally left blank -->
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}" text-align="left">
                <xsl:value-of select ="$vLabelValuta"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}" text-align="left">
                <xsl:call-template name="getMarginAmountCurrency"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}" text-align="left">
                <xsl:value-of select ="$vLabelCassa"/>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
      <!-- ************************* -->
      <!-- *** 3° row ************** -->
      <!-- ************************* -->
      <fo:table  table-layout="fixed">
        <fo:table-column column-width="{1}cm" column-number="01"/>
        <fo:table-column column-width="{1.3}cm" column-number="02"/>
        <fo:table-column column-width="{4.9}cm" column-number="03"/>
        <fo:table-column column-width="{1}cm" column-number="04"/>
        <fo:table-column column-width="{2.7}cm" column-number="05"/>
        <fo:table-column column-width="{1.4}cm" column-number="06"/>
        <fo:table-column column-width="{3.5}cm" column-number="07"/>
        <fo:table-column column-width="{11.7}cm" column-number="08"/>
        <fo:table-body>
          <fo:table-row height="{$vRowHeight}cm" >
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}" text-align="right">
                <!-- This field is intentionally left blank -->
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}" text-align="left">
                <xsl:value-of select ="$vLabelConto"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}" text-align="left">
                <xsl:call-template name ="getBook"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}" text-align="left">
                <xsl:value-of select ="$vLabelOmn"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}" text-align="left">
                <xsl:value-of select ="$vLabelGruppoMercati"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}" text-align="left">
                <xsl:call-template name="getMarketsGroup"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}" text-align="left">
                <xsl:value-of select ="$vLabelContrattazioni"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}" text-align="left">
                <xsl:call-template name ="getTradeDate"/>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>

      <xsl:if test="$vIsGross='false'">
        <!-- ************************* -->
        <!-- *** 4° row ************** -->
        <!-- ************************* -->
        <fo:table table-layout="fixed">
          <xsl:call-template name="createHeaderTitleColumns"/>
          <fo:table-body>
            <!-- EG 20160404 Migration vs2013 -->
            <!--<xsl:call-template name="displayBreakline">
              <xsl:with-param name ="pRowHeight" select ="$vRowHeightBreakSpace"/>
            </xsl:call-template>-->
            <xsl:call-template name="displayBreakline" />
            <fo:table-row height="{$vRowHeightColumnTitles}cm" >

              <fo:table-cell border="{$vBorder}">
                <fo:block border="{$vBorder}" text-align="right">
                  <!-- This field is intentionally left blank -->
                </fo:block>
              </fo:table-cell>

              <!-- Descrizione Contratto -->
              <fo:table-cell border="{$vBorder}">
                <fo:block border="{$vBorder}" text-align="left">
                  <xsl:value-of select ="$vLabelDescrizioneContratto"/>
                </fo:block>
              </fo:table-cell>

              <!-- Prezzo/Ammont.ITM -->
              <fo:table-cell border="{$vBorder}">
                <fo:block border="{$vBorder}" text-align="right">
                  <xsl:value-of select ="$vLabelPrice"/>
                </fo:block>
              </fo:table-cell>

              <!-- Posiz. Cop./Str -->
              <fo:table-cell border="{$vBorder}">
                <fo:block border="{$vBorder}" text-align="right">
                  <xsl:value-of select ="$vLabelPosiz"/>
                </fo:block>
              </fo:table-cell>

              <!-- Nette/Ordin. -->
              <fo:table-cell border="{$vBorder}">
                <fo:block border="{$vBorder}" text-align="right">
                  <xsl:value-of select ="$vLabelNette"/>
                </fo:block>
              </fo:table-cell>

              <!-- Margini su Premio/MTM -->
              <fo:table-cell border="{$vBorder}">
                <fo:block border="{$vBorder}" text-align="right">
                  <xsl:value-of select ="$vLabelPremium"/>
                </fo:block>
              </fo:table-cell>

              <!-- Margini su Premio/MTM Prefix -->
              <fo:table-cell border="{$vBorder}">
                <fo:block border="{$vBorder}" text-align="left">
                  <!-- This field is intentionally left blank -->
                </fo:block>
              </fo:table-cell>

              <!-- Straddle Consegna -->
              <fo:table-cell border="{$vBorder}">
                <fo:block border="{$vBorder}" text-align="right">
                  <xsl:value-of select ="$vLabelStraddle"/>
                </fo:block>
              </fo:table-cell>

              <!-- Margini Addizionali -->
              <fo:table-cell border="{$vBorder}">
                <fo:block border="{$vBorder}" text-align="right">
                  <xsl:value-of select ="$vLabelAdditional"/>
                </fo:block>
              </fo:table-cell>

              <!-- Margini Totali -->
              <fo:table-cell border="{$vBorder}">
                <fo:block border="{$vBorder}" text-align="right">
                  <xsl:value-of select ="$vLabelTotal"/>
                </fo:block>
              </fo:table-cell>

            </fo:table-row>
          </fo:table-body>
        </fo:table>
      </xsl:if>
    </fo:block>
  </xsl:template>

  <!-- **********************************************************  -->
  <!-- It draw the body of MS22 report -->
  <!-- the report body consists of:
        1. Adjustment or Cross Margin section (posizione azionaria)
        2. Option and Future positions section
        3. Total for Class section
        4. Total for Group section
        5. Total for Account section    -->
  <!-- **********************************************************  -->
  <xsl:template name="displayBody">
    <xsl:if test="$vIsGross='false'">
      <!-- PM 20160404 [22116] Gestion du cas de plusieurs méthodes dans le log -->
      <!--<xsl:if test ="//efs:marginRequirementOffice/efs:marginCalculation/efs:netMargin/efs:marginCalculationMethod/@name = $vDescribedMethod">-->
      <xsl:if test ="//efs:marginRequirementOffice/efs:marginCalculation/efs:netMargin/efs:marginCalculationMethod[@name=$vDescribedMethod]">
        <xsl:for-each select ="//efs:marginCalculationMethod[@name=$vDescribedMethod]/efs:products/efs:product">
          <fo:block keep-together="always" font-size="{$vBlockFontSize}" linefeed-treatment="preserve" white-space-collapse="false">
            <fo:table table-layout="fixed">
              <xsl:call-template name="createBody11Columns"/>
              <fo:table-body>
                <xsl:for-each select ="efs:classes/efs:class/efs:contracts/efs:contract/efs:mtm/efs:factors/efs:factor">
                  <xsl:if test ="efs:Qty[@Typ='PA' or @Typ='XM']">
                    <!-- ***************************************************************************** -->
                    <!-- Position actionnaire - PA:Adjustment Quantity or XM:Cross Margin Quantity     -->
                    <!-- ***************************************************************************** -->
                    <xsl:call-template name="displayAdjustmentOrCrossMarginPositionRow">
                      <xsl:with-param name ="pNode" select ="current()"/>
                    </xsl:call-template>
                  </xsl:if>
                </xsl:for-each>
                <xsl:if test ="efs:classes/efs:class/efs:contracts/efs:contract/efs:mtm/efs:factors/efs:factor/node()">
                  <xsl:call-template name="displayBreakline"/>
                </xsl:if>
                <!-- ************************* -->
                <!-- Options and Futures       -->
                <!-- ************************* -->
                <xsl:for-each select="efs:classes/efs:class">
                  <xsl:for-each select ="efs:contracts/efs:contract">
                    <!-- cicle for future positions-->
                    <xsl:for-each select ="efs:futureConvertedPositions/efs:PosRpt">
                      <xsl:sort select="fixml:Instrmt/@MMY"/>
                      <xsl:call-template name="displayPositionRow">
                        <xsl:with-param name="pNode" select="current()"/>
                        <xsl:with-param name="pClassNode" select="ancestor::efs:class"/>
                        <xsl:with-param name="pContractNode" select="ancestor::efs:contract"/>
                        <xsl:with-param name="pInstrument" select="'Future'"/>
                      </xsl:call-template>
                    </xsl:for-each>
                    <!-- cicle for option positions PutCall=1-->
                    <xsl:for-each select ="efs:positions/efs:PosRpt[fixml:Instrmt/@PutCall=1]">
                      <xsl:sort select="fixml:Instrmt/@MMY" />
                      <xsl:sort select="fixml:Instrmt/@StrkPx" />
                      <xsl:variable name ="vInstrument">
                        <xsl:call-template name ="getInstrument">
                          <xsl:with-param name ="pTyp" select ="fixml:Qty[1]/@Typ"/>
                          <xsl:with-param name ="pStrkPx" select ="fixml:Instrmt/@StrkPx"/>
                          <xsl:with-param name ="pPutCall" select ="fixml:Instrmt/@PutCall"/>
                        </xsl:call-template>
                      </xsl:variable>
                      <xsl:if test ="$vInstrument='Option'">
                        <xsl:call-template name="displayPositionRow">
                          <xsl:with-param name="pNode" select="current()"/>
                          <xsl:with-param name="pClassNode" select="ancestor::efs:class"/>
                          <xsl:with-param name="pContractNode" select="ancestor::efs:contract"/>
                          <xsl:with-param name="pInstrument" select="$vInstrument"/>
                        </xsl:call-template>
                      </xsl:if>
                    </xsl:for-each>

                    <xsl:call-template name="displayBreakline"/>

                    <!-- cicle for option positions PutCall=0-->
                    <xsl:for-each select ="efs:positions/efs:PosRpt[fixml:Instrmt/@PutCall=0]">
                      <xsl:sort select="fixml:Instrmt/@MMY" />
                      <xsl:sort select="fixml:Instrmt/@StrkPx" />
                      <xsl:variable name ="vInstrument">
                        <xsl:call-template name ="getInstrument">
                          <xsl:with-param name ="pTyp" select ="fixml:Qty[1]/@Typ"/>
                          <xsl:with-param name ="pStrkPx" select ="fixml:Instrmt/@StrkPx"/>
                          <xsl:with-param name ="pPutCall" select ="fixml:Instrmt/@PutCall"/>
                        </xsl:call-template>
                      </xsl:variable>
                      <xsl:if test ="$vInstrument='Option'">
                        <xsl:call-template name="displayPositionRow">
                          <xsl:with-param name="pNode" select="current()"/>
                          <xsl:with-param name="pClassNode" select="ancestor::efs:class"/>
                          <xsl:with-param name="pContractNode" select="ancestor::efs:contract"/>
                          <xsl:with-param name="pInstrument" select="$vInstrument"/>
                        </xsl:call-template>
                      </xsl:if>
                    </xsl:for-each>
                  </xsl:for-each>
                  <!-- ********************************* -->
                  <!-- Total for Class                   -->
                  <!-- ********************************* -->
                  <xsl:call-template name="displayTotalClassRow">
                    <xsl:with-param name="pNode" select="current()"/>
                  </xsl:call-template>
                  <xsl:call-template name="displayBreakline"/>
                </xsl:for-each>
                <!-- ********************************* -->
                <!-- Total for Group                  -->
                <!-- ********************************* -->
                <xsl:call-template name="displayTotalGroupRow">
                  <xsl:with-param name="pNode" select="current()"/>
                </xsl:call-template>
                <!-- EG 20160404 Migration vs2013 -->
                <!--<xsl:call-template name="displayBreakline">
                  <xsl:with-param name ="pRowHeight" select ="$vRowHeightBreakSpaceProducts"/>
                </xsl:call-template>-->
                <xsl:call-template name="displayBreakline" />
              </fo:table-body>
            </fo:table>
          </fo:block>
        </xsl:for-each>
      </xsl:if>
    </xsl:if>
    <fo:block linefeed-treatment="preserve" white-space-collapse="false" font-size="{$vBlockFontSize}">
      <fo:table table-layout="fixed">
        <xsl:call-template name="createBody6Columns"/>
        <fo:table-body>
          <!-- *************************** -->
          <!-- Total for Account           -->
          <!-- *************************** -->
          <xsl:call-template name ="displayTotalAccountRow"/>
        </fo:table-body>
      </fo:table>
    </fo:block>
  </xsl:template>

  <!-- ***************************************************************************** -->
  <!-- It draw a additional row for each Adjustment or Cross Margin position         -->
  <!-- ***************************************************************************** -->
  <xsl:template name="displayAdjustmentOrCrossMarginPositionRow">
    <xsl:param name="pNode"/>
    <fo:table-row>

      <!-- Descrizione Contratto -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}">
          <xsl:text>C </xsl:text>
          <xsl:value-of select="$pNode/@ID"/>
        </fo:block>
      </fo:table-cell>

      <!-- Descrizione Contratto -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}">
          <!-- This field is intentionally left blank -->
        </fo:block>
      </fo:table-cell>

      <!-- Prezzo/Ammont.ITM -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <xsl:if test ="efs:Quote != ''">
            <xsl:call-template name ="formatQuote">
              <xsl:with-param name ="pQuote" select="$pNode/efs:Quote"/>
            </xsl:call-template>
          </xsl:if>
        </fo:block>
      </fo:table-cell>

      <!-- Posiz. Cop./Str -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <xsl:value-of select ="$pNode/efs:Qty[@Typ='PA']/@Short"/>
        </fo:block>
      </fo:table-cell>

      <!-- Nette/Ordin. -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <xsl:if test ="$pNode/efs:Qty/@Long != 0">
            <xsl:value-of select="concat($pNode/efs:Qty/@Long,'L')"/>
          </xsl:if>
          <xsl:if test ="$pNode/efs:Qty/@Short != 0">
            <xsl:value-of select="concat($pNode/efs:Qty/@Short,'C')"/>
          </xsl:if>
        </fo:block>
      </fo:table-cell>

      <!-- Margini su Premio/MTM -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <xsl:variable name="vPremiumMTMMarginAmount">
            <xsl:call-template name="getUnformattedAmount">
              <xsl:with-param name="pNode" select="$pNode"/>
            </xsl:call-template>
          </xsl:variable>
          <xsl:call-template name ="formatAmount">
            <xsl:with-param name ="pAmount" select ="$vPremiumMTMMarginAmount"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>

      <!-- Margini su Premio/MTM suffix -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="left">
          <xsl:call-template name ="AmountSymbol">
            <xsl:with-param name ="pNode" select ="$pNode"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>

      <!-- Straddle Consegna -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <!-- This field is intentionally left blank -->
        </fo:block>
      </fo:table-cell>

      <!-- Margini Addizionali -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <!-- This field is intentionally left blank -->
        </fo:block>
      </fo:table-cell>

      <!-- Margini Totali -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <!-- This field is intentionally left blank -->
        </fo:block>
      </fo:table-cell>

      <!-- Margini Totali suffix -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}">
          <!-- This field is intentionally left blank -->
        </fo:block>
      </fo:table-cell>

    </fo:table-row>
  </xsl:template>

  <!-- ***************************************************************************** -->
  <!-- It draw a additional row for each Exercise/Assignation (only for Options)     -->
  <!-- ***************************************************************************** -->
  <xsl:template name="displayExerciseAssignationRow">
    <xsl:param name="pNode"/>
    <xsl:param name="pContractNode"/>
    <xsl:param name ="pInstrument"/>
    <xsl:param name ="pInstrId"/>

    <fo:table-row>

      <!-- Descrizione Contratto -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" start-indent="1.5cm">
          <xsl:value-of select ="$vLabelOpen"/>
        </fo:block>
      </fo:table-cell>

      <!-- original name  -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}">
          <xsl:if test ="$pNode/fixml:Qty[@Typ='AS']">
            <xsl:value-of select="$pNode/fixml:Qty[@Typ='AS']/@QtyDt"/>
          </xsl:if>
          <xsl:if test ="$pNode/fixml:Qty[@Typ='EX']">
            <xsl:value-of select="$pNode/fixml:Qty[@Typ='EX']/@QtyDt"/>
          </xsl:if>
        </fo:block>
      </fo:table-cell>

      <!-- Prezzo/Ammont.ITM -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <xsl:call-template name="getQuote">
            <xsl:with-param name ="pNode" select="$pContractNode/efs:premium"/>
            <xsl:with-param name ="pInstrId" select="$pInstrId"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>

      <!-- Posiz. Cop./Str -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <!-- This field is intentionally left blank -->
        </fo:block>
      </fo:table-cell>

      <!-- Nette/Ordin. -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <xsl:call-template name="getQtyWithTyp">
            <xsl:with-param name ="pNode" select="$pContractNode/efs:premium"/>
            <xsl:with-param name ="pInstrId" select="$pInstrId"/>
            <xsl:with-param name ="pTyp" select ="$pNode/fixml:Qty/@Typ"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>

      <!-- Margini su Premio/MTM -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <xsl:call-template name="getMarginAmountWithTyp">
            <xsl:with-param name ="pNode" select="$pContractNode/efs:premium"/>
            <xsl:with-param name ="pInstrId" select="$pInstrId"/>
            <xsl:with-param name ="pTyp" select ="$pNode/fixml:Qty/@Typ"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>

      <!-- Margini su Premio/MTM suffix-->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="left">
          <xsl:call-template name ="AmountSymbol">
            <xsl:with-param name ="pNode" select ="$pContractNode/efs:premium/efs:factors/efs:factor[@ID=$pInstrId and efs:Qty[@Typ=$pNode/fixml:Qty/@Typ]]"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>

      <!-- Straddle Consegna -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <!-- This field is intentionally left blank -->
        </fo:block>
      </fo:table-cell>

      <!-- Margini Addizionali -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <!-- This field is intentionally left blank -->
        </fo:block>
      </fo:table-cell>

      <!-- Margini Totali -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <!-- This field is intentionally left blank -->
        </fo:block>
      </fo:table-cell>

      <!-- Margini Totali suffix -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}">
          <!-- This field is intentionally left blank -->
        </fo:block>
      </fo:table-cell>

    </fo:table-row>
  </xsl:template>

  <!-- ***************************************************************************** -->
  <!-- It draw a row for each Option and Future positions                            -->
  <!-- and it draw a additional row for each Exercise/Assignation (only for Options) -->
  <!-- ***************************************************************************** -->
  <xsl:template name="displayPositionRow">
    <xsl:param name="pNode"/>
    <xsl:param name="pClassNode"/>
    <xsl:param name="pContractNode"/>
    <xsl:param name="pInstrument"/>

    <!--instrument ID-->
    <xsl:variable name ="vInstrmtId">
      <xsl:call-template name ="getInstrumentId">
        <xsl:with-param name ="pNode" select ="$pNode"/>
      </xsl:call-template>
    </xsl:variable>

    <!--instrument ID without Isincode-->
    <xsl:variable name ="vShortInstrmtId">
      <xsl:call-template name ="getShortInstrumentId">
        <xsl:with-param name ="pNode" select ="$pNode"/>
      </xsl:call-template>
    </xsl:variable>

    <!--contains option style (call ot put)-->
    <xsl:variable name ="vOptionStyle">
      <xsl:call-template name ="getOptionStyle">
        <xsl:with-param name ="pInstrument" select ="$pInstrument"/>
        <xsl:with-param name ="pPutCall" select ="$pNode/fixml:Instrmt/@PutCall"/>
      </xsl:call-template>
    </xsl:variable>

    <!--returns contract Type (F/O)-->
    <xsl:variable name ="vContractType">
      <xsl:call-template name ="getContractType">
        <xsl:with-param name ="pInstrument" select ="$pInstrument"/>
      </xsl:call-template>
    </xsl:variable>

    <!--return naming convention for IDEM instrument-->
    <xsl:variable name ="vSeriesCode">
      <xsl:call-template name ="getSeriesCode">
        <xsl:with-param name ="pDisplayTranscodedSerie" select="true()"/>
        <xsl:with-param name ="pInstrmtId" select="$vInstrmtId"/>
        <xsl:with-param name ="pInstrument" select="$pInstrument"/>
        <xsl:with-param name ="pOptionStyle" select="$vOptionStyle"/>
        <xsl:with-param name ="pStrike" select="$pNode/fixml:Instrmt/@StrkPx"/>
        <xsl:with-param name ="pMMY" select="$pNode/fixml:Instrmt/@MMY"/>
        <xsl:with-param name ="pSym" select="$pNode/fixml:Instrmt/@Sym"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vQtyType">
      <xsl:value-of select="$pNode/fixml:Qty/@Typ"/>
    </xsl:variable>

    <fo:table-row>

      <!-- Descrizione Contratto -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}">
          <xsl:value-of select="$vContractType"/>
          <xsl:value-of select="$vSeriesCode"/>
        </fo:block>
      </fo:table-cell>

      <!-- original name  -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}">
          <xsl:choose>
            <!--GS 20131008 (QtyType=DN) position Future sur actions en livraison-->
            <xsl:when test ="$pInstrument='Future' and $vQtyType='DN'">
              <xsl:value-of select="$pContractNode/efs:positions/efs:PosRpt[fixml:Instrmt/@ID=$vInstrmtId]/fixml:Qty/@QtyDt"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:if test="$vContractOriginalNameDisplayed='true'">
                <xsl:value-of select="concat('(' , $vShortInstrmtId, ')')"/>
              </xsl:if>
            </xsl:otherwise>
          </xsl:choose>
        </fo:block>
      </fo:table-cell>

      <!-- Prezzo/Ammont.ITM -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <xsl:choose>
            <xsl:when test ="$pInstrument='Future'">
              <xsl:choose>
                <!--  GS 20131008 (QtyType=DN) position Future sur actions en livraison -->
                <xsl:when test="$vQtyType='DN'">
                  <xsl:call-template name="getQuote">
                    <xsl:with-param name ="pNode" select="$pContractNode/efs:mtm"/>
                    <xsl:with-param name ="pInstrId" select="$vInstrmtId"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="getQuote">
                    <xsl:with-param name ="pNode" select="$pClassNode/efs:spread"/>
                    <xsl:with-param name ="pInstrId" select="$vInstrmtId"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:when test ="$pInstrument='Option'">
              <xsl:choose>
                <!--GS 20131008 This field is intentionally left blank for exercise/assignation -->
                <xsl:when test="fixml:Qty[1][@Typ='AS' or @Typ='EX']"/>
                <xsl:otherwise>
                  <xsl:call-template name="getQuote">
                    <xsl:with-param name ="pNode" select="$pContractNode/efs:premium"/>
                    <xsl:with-param name ="pInstrId" select="$vInstrmtId"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:otherwise/>
          </xsl:choose>
        </fo:block>
      </fo:table-cell>

      <!-- Posiz. Cop./Str -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <xsl:choose>
            <xsl:when test ="$pInstrument='Future'">
              <!--PM 20141114 [20491] Ajout du calcul des spreads au niveau Contract-->
              <xsl:choose>
                <xsl:when test="$pClassNode/efs:spread/efs:factors">
                  <xsl:call-template name ="getSpreadQuantity">
                    <xsl:with-param name ="pNode" select="$pClassNode/efs:spread"/>
                    <xsl:with-param name ="pInstrId" select="$vInstrmtId"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name ="getSpreadQuantity">
                    <xsl:with-param name ="pNode" select="$pContractNode/efs:spread"/>
                    <xsl:with-param name ="pInstrId" select="$vInstrmtId"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:when test ="$pInstrument='Option'">
              <xsl:choose>
                <!--GS 20131008 This field is intentionally left blank for exercise/assignation -->
                <xsl:when test="fixml:Qty[1][@Typ='AS' or @Typ='EX']"/>
                <xsl:otherwise>
                  <xsl:value-of select ="fixml:Qty[@Typ='PA']/@Short"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:otherwise/>
          </xsl:choose>
        </fo:block>
      </fo:table-cell>

      <!-- Nette/Ordin. -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <xsl:choose>
            <xsl:when test ="$pInstrument='Future'">
              <xsl:choose>
                <!--  GS 20131008 (QtyType=DN) position Future sur actions en livraison -->
                <xsl:when test="$vQtyType='DN'">
                  <xsl:call-template name="getQtyWithTyp">
                    <xsl:with-param name ="pNode" select="$pContractNode/efs:mtm"/>
                    <xsl:with-param name ="pInstrId" select="$vInstrmtId"/>
                    <xsl:with-param name ="pTyp" select ="$pNode/fixml:Qty/@Typ"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <!--PM 20141114 [20491] Ajout du calcul des spreads au niveau Contract-->
                  <xsl:choose>
                    <xsl:when test="$pClassNode/efs:spread/efs:factors">
                      <xsl:call-template name ="getNetQuantity">
                        <xsl:with-param name ="pNode" select="$pNode"/>
                        <xsl:with-param name ="pSpreadNode" select ="$pClassNode/efs:spread"/>
                        <xsl:with-param name ="pInstrId" select="$vInstrmtId"/>
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:call-template name ="getNetQuantity">
                        <xsl:with-param name ="pNode" select="$pNode"/>
                        <xsl:with-param name ="pSpreadNode" select ="$pContractNode/efs:spread"/>
                        <xsl:with-param name ="pInstrId" select="$vInstrmtId"/>
                      </xsl:call-template>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:when test ="$pInstrument='Option'">
              <xsl:choose>
                <!--GS 20131008 This field is intentionally left blank for exercise/assignation -->
                <xsl:when test="fixml:Qty[1][@Typ='AS' or @Typ='EX']"/>
                <xsl:otherwise>
                  <xsl:call-template name="getQtyWithoutTyp">
                    <xsl:with-param name ="pNode" select="$pContractNode/efs:premium"/>
                    <xsl:with-param name ="pInstrId" select="$vInstrmtId"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:otherwise/>
          </xsl:choose>
        </fo:block>
      </fo:table-cell>

      <!-- Margini su Premio/MTM -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <xsl:choose>
            <!--  GS 20131008 (QtyType=DN) position Future sur actions en livraison -->
            <xsl:when test ="$pInstrument='Future' and $vQtyType='DN'">
              <xsl:call-template name="getMarginAmountWithoutTyp">
                <xsl:with-param name ="pNode" select="$pContractNode/efs:mtm"/>
                <xsl:with-param name ="pInstrId" select="$vInstrmtId"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:when test ="$pInstrument='Option'">
              <xsl:choose>
                <!--GS 20131008 This field is intentionally left blank for exercise/assignation -->
                <xsl:when test="fixml:Qty[1][@Typ='AS' or @Typ='EX']"/>
                <xsl:otherwise>
                  <xsl:call-template name="getMarginAmountWithoutTyp">
                    <xsl:with-param name ="pNode" select="$pContractNode/efs:premium"/>
                    <xsl:with-param name ="pInstrId" select="$vInstrmtId"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:otherwise/>
          </xsl:choose>
        </fo:block>
      </fo:table-cell>

      <!-- Margini su Premio/MTM symbol -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="left">
          <!-- Return C for negative amount else null-->
          <xsl:choose>
            <!--  GS 20131008 (QtyType=DN) position Future sur actions en livraison -->
            <xsl:when test ="$pInstrument='Future' and $vQtyType='DN'">
              <xsl:call-template name ="AmountSymbol">
                <xsl:with-param name ="pNode" select ="$pContractNode/efs:mtm/efs:factors/efs:factor[@ID=$vInstrmtId]"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:when test ="$pInstrument='Option'">
              <xsl:choose>
                <!--GS 20131008 This field is intentionally left blank for exercise/assignation -->
                <xsl:when test="fixml:Qty[1][@Typ='AS' or @Typ='EX']"/>
                <xsl:otherwise>
                  <xsl:call-template name ="AmountSymbol">
                    <xsl:with-param name ="pNode" select ="$pContractNode/efs:premium/efs:factors/efs:factor[@ID=$vInstrmtId]"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:otherwise/>
          </xsl:choose>
        </fo:block>
      </fo:table-cell>

      <!-- Straddle Consegna -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <xsl:if test ="$pInstrument='Future' and $vQtyType!='DN'">
            <!--PM 20141114 [20491] Ajout du calcul des spreads au niveau Contract-->
            <xsl:choose>
              <xsl:when test="$pClassNode/efs:spread/efs:factors">
                <xsl:call-template name="getMarginAmountWithoutTyp">
                  <xsl:with-param name ="pNode" select="$pClassNode/efs:spread"/>
                  <xsl:with-param name ="pInstrId" select="$vInstrmtId"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name="getMarginAmountWithoutTyp">
                  <xsl:with-param name ="pNode" select="$pContractNode/efs:spread"/>
                  <xsl:with-param name ="pInstrId" select="$vInstrmtId"/>
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:if>
        </fo:block>
      </fo:table-cell>

      <!-- Margini Addizionali -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <!-- This field is intentionally left blank -->
        </fo:block>
      </fo:table-cell>

      <!-- Margini Totali -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <!-- This field is intentionally left blank -->
        </fo:block>
      </fo:table-cell>

      <!-- Margini Totali suffix -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}">
          <!-- This field is intentionally left blank -->
        </fo:block>
      </fo:table-cell>

    </fo:table-row>
    <xsl:if test ="fixml:Qty[@Typ='AS' or @Typ='EX'] and $pInstrument='Option'">
      <!-- ********************************************* -->
      <!-- Exercise Assignation                          -->
      <!-- ********************************************* -->
      <xsl:call-template name ="displayExerciseAssignationRow">
        <xsl:with-param name ="pNode" select ="$pNode"/>
        <xsl:with-param name ="pClassNode" select ="$pClassNode"/>
        <xsl:with-param name ="pContractNode" select ="$pContractNode"/>
        <xsl:with-param name ="pInstrument" select ="$pInstrument"/>
        <xsl:with-param name ="pInstrId" select ="$vInstrmtId"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <!-- **************************************** -->
  <!-- It draw a row contains the class total   -->
  <!-- from efs:classes/efs:class node          -->
  <!-- **************************************** -->
  <xsl:template name="displayTotalClassRow">
    <xsl:param name="pNode"/>
    <fo:table-row>
      
      <!-- Descrizione Contratto -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="left">
          <xsl:call-template name ="getClassName">
            <xsl:with-param name ="pName" select ="$pNode/@name"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>

      <!-- Original name  -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <!-- This field is intentionally left blank -->
        </fo:block>
      </fo:table-cell>

      <!-- Prezzo/Ammont.ITM -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <!-- This field is intentionally left blank -->
        </fo:block>
      </fo:table-cell>

      <!-- Posiz. Cop./Str -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <!-- This field is intentionally left blank -->
        </fo:block>
      </fo:table-cell>

      <!-- Nette/Ordin. -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <!-- This field is intentionally left blank -->
        </fo:block>
      </fo:table-cell>

      <!-- Margini su Premio/MTM -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <xsl:call-template name ="getTotalPremium">
            <xsl:with-param name ="pCurrent" select ="$pNode"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>

      <!-- Margini su Premio/MTM suffix -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="left">
          <xsl:call-template name ="AmountSymbol">
            <xsl:with-param name ="pNode" select ="$pNode/efs:premium"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>

      <!-- Straddle Consegna -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <xsl:call-template name ="getTotalStraddle">
            <xsl:with-param name ="pCurrent" select ="$pNode"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>

      <!-- Margini Addizionali -->
      <fo:table-cell border="{$vBorder}">
        <!--RD 20160114 [21722] Ne pas afficher Margini Addizionali par Class-->
        <!--<fo:block border="{$vBorder}" text-align="right">
          <xsl:call-template name ="getTotalAdditional">
            <xsl:with-param name ="pCurrent" select ="$pNode"/>
          </xsl:call-template>
        </fo:block>-->
        <fo:block border="{$vBorder}"/>
      </fo:table-cell>

      <!-- Margini Totali -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}">
          <!-- This field is intentionally left blank -->
        </fo:block>
      </fo:table-cell>

      <!-- Margini Totali suffix-->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}">
          <!-- This field is intentionally left blank -->
        </fo:block>
      </fo:table-cell>
    </fo:table-row>
  </xsl:template>

  <!-- **************************************************************** -->
  <!-- It draw a row contains the group total                           -->
  <!-- from //efs:marginCalculationMethod/efs:products/efs:product node -->
  <!-- **************************************************************** -->
  <xsl:template name ="displayTotalGroupRow">
    <xsl:param name ="pNode"/>
    <fo:table-row>
      
      <!-- Descrizione Contratto -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="left">
          <xsl:value-of select ="$vLabelGrpProd"/>
        </fo:block>
      </fo:table-cell>
      
      <!-- Original name  -->
      <fo:table-cell  border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="left">
          <xsl:value-of select ="$vLabelOffset"/>
        </fo:block>
      </fo:table-cell>
      
      <!-- Prezzo/Ammont.ITM -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <xsl:value-of select ="$vLabelMrgMin"/>
        </fo:block>
      </fo:table-cell>

      <!-- Posiz. Cop./Str -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <xsl:call-template name ="getMinimumMarginGroup">
            <xsl:with-param name ="pCurrent" select ="$pNode"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>

      <!-- Nette/Ordin. -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <!-- This field is intentionally left blank -->
        </fo:block>
      </fo:table-cell>

      <!-- Margini su Premio/MTM -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <xsl:call-template name ="getTotalPremium">
            <xsl:with-param name ="pCurrent" select ="$pNode"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>

      <!-- Margini su Premio/MTM suffix -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="left">
          <xsl:call-template name ="AmountSymbol">
            <xsl:with-param name ="pNode" select ="$pNode/efs:premium"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>

      <!-- Straddle Consegna -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <xsl:call-template name ="getTotalStraddle">
            <xsl:with-param name ="pCurrent" select ="$pNode"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>

      <!-- Margini Addizionali -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <xsl:call-template name ="getTotalAdditional">
            <xsl:with-param name ="pCurrent" select ="$pNode"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>

      <!-- Margini Totali -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <xsl:call-template name ="getMarginAmount">
            <xsl:with-param name ="pNode" select ="$pNode"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>

      <!-- Margini Totali suffix -->
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="left">
           <xsl:call-template name ="AmountSymbol">
            <xsl:with-param name ="pNode" select ="$pNode"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
    </fo:table-row>
  </xsl:template>

  <!-- ************************************** -->
  <!-- It draw a row contains account total   -->
  <!-- from efs:marginRequirementOffice node  -->
  <!-- ************************************** -->
  <xsl:template name ="displayTotalAccountRow">
    <fo:table-row>
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="left">
          <xsl:value-of select ="$vLabelTotConto"/>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="left">
          <xsl:call-template name ="getBook"/>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="left">
          <xsl:call-template name="getMarginAmountCurrency"/>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <xsl:value-of select ="$vLabelFattPond"/>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <xsl:value-of select="format-number(//efs:marginRequirementOffice/@wratio, '#.##0,00000000', 'decimal' )"/>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$vBorder}">
        <fo:block border="{$vBorder}" text-align="right">
          <xsl:call-template name ="getMROMarginAmount"/>
        </fo:block>
      </fo:table-cell>
    </fo:table-row>
  </xsl:template>

</xsl:stylesheet>