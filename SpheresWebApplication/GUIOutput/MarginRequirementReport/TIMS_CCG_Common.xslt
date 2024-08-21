<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="1.0"
                xmlns:xsl  ="http://www.w3.org/1999/XSL/Transform"
                xmlns:dt   ="http://xsltsl.org/date-time"
                xmlns:fo   ="http://www.w3.org/1999/XSL/Format"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:efs  ="http://www.efs.org/2007/EFSmL-3-0"
                xmlns:fixml="http://www.fixprotocol.org/FIXML-5-0-SP1"
                xmlns:fpml ="http://www.fpml.org/2007/FpML-4-4">

  <!-- ==================== -->
  <!-- xslt includes        -->
  <!-- ==================== -->
  <xsl:include href="..\..\Library\Resource.xslt" />
  <xsl:include href="..\..\Library\DtFunc.xslt" />
  <xsl:include href="..\..\Library\xsltsl/date-time.xsl" />
  

  <!-- ==================== -->
  <!-- Global variables     -->
  <!-- ==================== -->
  <xsl:decimal-format name="decimal" decimal-separator="," grouping-separator="."/>
  <xsl:variable name="vDescribedMethod" select ="'TIMS IDEM'"/>
  <xsl:variable name="vContractOriginalNameDisplayed">false</xsl:variable>
  <xsl:variable name="vLinefeed">&#xA0;</xsl:variable>
  <xsl:variable name="vSpaceCharacter">&#160;</xsl:variable>
  <xsl:variable name="vBoldFontWeidth">bold</xsl:variable>
  <!-- Set 1 for debug all files, 0 otherwise-->
  <xsl:variable name="vIsDebug">0</xsl:variable>
  <xsl:variable name="vBorder">
    <xsl:choose>
      <xsl:when test="$vIsDebug=1">0.5pt solid black</xsl:when>
      <xsl:otherwise>0pt solid black</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name ="vLineHeight">0.6</xsl:variable>
  <!-- A4 page caracteristics  -->
  <xsl:variable name="vA4LandscapePageHeight">21</xsl:variable>
  <xsl:variable name="vA4LandscapePageWidth">29.7</xsl:variable>
  <xsl:variable name="vA4LandscapePageMargin">0.5</xsl:variable>
  <!-- Font  -->
  <xsl:variable name="vFontFamily">Courier</xsl:variable>
  <!-- A4 page format  -->
  <xsl:variable name="vA4LandscapePageLeftMargin">0.5</xsl:variable>
  <xsl:variable name="vA4LandscapePageRightMargin">0.5</xsl:variable>
  <xsl:variable name="vA4LandscapePageTopMargin">0.5</xsl:variable>
  <xsl:variable name="vA4LandscapePageBottomMargin">0.5</xsl:variable>
  <!-- Block -->
  <xsl:variable name ="vBlockLeftMargin">0</xsl:variable>
  <xsl:variable name ="vBlockRightMargin">1</xsl:variable>
  <xsl:variable name ="vBlockTopMargin">0.2</xsl:variable>
  <xsl:variable name ="vBlockFontSize">8pt</xsl:variable>
  <xsl:variable name ="vRowHeightBreakSpace">0.5</xsl:variable>
  <xsl:variable name ="vRowHeightBreakSpaceProducts">1</xsl:variable>
  <xsl:variable name ="vRowHeight">0.3</xsl:variable>
  <xsl:variable name ="vRowHeightColumnTitles">0.6</xsl:variable>
  <!-- Body -->
  <xsl:variable name ="vBodyLeftMargin">1</xsl:variable>
  <xsl:variable name ="vBodyRightMargin">1</xsl:variable>
  <xsl:variable name ="vBodyBottomMargin">2</xsl:variable>
  <!-- EG 20160404 Migration vs2013 -->
  <xsl:variable name ="vRowHeightBreakline">0.4</xsl:variable>
  <xsl:variable name ="vBodyTopMargin">
    <xsl:value-of select ="$vHeaderExtent"/>
  </xsl:variable>
  <!-- Header -->
  <xsl:variable name ="vHeaderExtent">
    <xsl:value-of select ="($vRowHeight*3) + $vRowHeightBreakSpace + $vRowHeightColumnTitles + $vA4LandscapePageTopMargin"/>
  </xsl:variable>
  <xsl:variable name ="vHeaderTopMargin">0.5</xsl:variable>
  <xsl:variable name ="vHeaderLeftMargin">1</xsl:variable>
  <!-- Footer -->
  <xsl:variable name ="vFooterExtent">1</xsl:variable>
  <xsl:variable name ="vFooterBorderBottom">0.8pt solid black</xsl:variable>
  <xsl:variable name ="vFooterFontSize">7pt</xsl:variable>
  <!-- Table  -->
  <xsl:variable name ="vTableLeftMargin">0</xsl:variable>
  <!-- We have to calculate it dinamically -->
  <xsl:variable name ="vTableHeight">
    <xsl:value-of select="$vA4LandscapePageHeight - $vA4LandscapePageLeftMargin - $vA4LandscapePageRightMargin - $vBlockLeftMargin - $vBlockRightMargin" />
  </xsl:variable>
  <!-- Variables for table-cell -->
  <xsl:variable name ="vTableCellPaddingTop">0</xsl:variable>
  <xsl:variable name ="vTableCellMarginLeft">0</xsl:variable>


  <!-- ============================ -->
  <!-- Graphic Templates            -->
  <!-- ============================ -->

  <!-- ********************************* -->
  <!--  Draw Landscape A4 page           -->
  <!-- ********************************* -->
  <xsl:template name="setPagesCaracteristics">
    <fo:layout-master-set>
      <!-- EG 20160404 Migration vs2013 -->
      <fo:simple-page-master master-name="A4LandscapePage" page-height="{$vA4LandscapePageHeight}cm" page-width="{$vA4LandscapePageWidth}cm" margin-left="{$vA4LandscapePageMargin}cm" margin-top="{$vA4LandscapePageMargin}cm" margin-right="{$vA4LandscapePageMargin}cm" margin-bottom="{$vA4LandscapePageMargin}cm">
        <fo:region-body region-name="A4LandscapeBody" background-color="white" margin-left="{$vBodyLeftMargin}cm" margin-bottom="{$vBodyBottomMargin}cm" margin-top="{$vBodyTopMargin}cm" margin-right="{$vBodyTopMargin}cm"/>
        <fo:region-before region-name="A4LandscapeHeader" background-color="white" extent="{$vHeaderExtent}cm" precedence="true" />
        <fo:region-after region-name="A4LandscapeFooter" background-color="white" extent="{$vFooterExtent}cm" precedence="true" />
      </fo:simple-page-master>
    </fo:layout-master-set>
  </xsl:template>

  <!-- ********************************* -->
  <!--  Draw Footer                       -->
  <!-- ********************************* -->
  <xsl:template name="displayFooter">
    <fo:block border="{$vBorder}"  text-align="right" font-size="{$vFooterFontSize}" font-family ="sans-serif">
      <!-- EG 20160404 Migration vs2013 -->
      <xsl:text>Powered by Spheres - © 2024 EFS</xsl:text>
    </fo:block>
  </xsl:template>

  <!-- ************************* -->
  <!-- draw header title columns -->
  <!-- ************************* -->
  <xsl:template name="createHeaderTitleColumns">
    <fo:table-column column-width="{1}cm" column-number="01"/>
    <fo:table-column column-width="{8}cm" column-number="02"/>
    <fo:table-column column-width="{2}cm" column-number="03"/>
    <fo:table-column column-width="{2}cm" column-number="04"/>
    <fo:table-column column-width="{2}cm" column-number="05"/>
    <fo:table-column column-width="{3}cm" column-number="06"/>
    <fo:table-column column-width="{0.25}cm" column-number="07"/>
    <fo:table-column column-width="{3}cm" column-number="08"/>
    <fo:table-column column-width="{3}cm" column-number="09"/>
    <fo:table-column column-width="{3}cm" column-number="10"/>
  </xsl:template>

  <!-- ************************* -->
  <!-- draw 10 columns -->
  <!-- ************************* -->
  <xsl:template name="createBody11Columns">
    <fo:table-column column-width="{4}cm" column-number="01"/>
    <fo:table-column column-width="{4}cm" column-number="02"/>
    <fo:table-column column-width="{2}cm" column-number="03"/>
    <fo:table-column column-width="{2}cm" column-number="04"/>
    <fo:table-column column-width="{2}cm" column-number="05"/>
    <fo:table-column column-width="{3}cm" column-number="06"/>
    <fo:table-column column-width="{0.25}cm" column-number="07"/>
    <fo:table-column column-width="{3}cm" column-number="08"/>
    <fo:table-column column-width="{3}cm" column-number="09"/>
    <fo:table-column column-width="{3}cm" column-number="10"/>
    <fo:table-column column-width="{0.25}cm" column-number="11"/>
  </xsl:template>

  <!-- ************************* -->
  <!-- draw 6 columns -->
  <!-- ************************* -->
  <xsl:template name="createBody6Columns">
    <fo:table-column column-width="{3}cm" column-number="01"/>
    <fo:table-column column-width="{4}cm" column-number="02"/>
    <fo:table-column column-width="{3}cm" column-number="03"/>
    <fo:table-column column-width="{4.5}cm" column-number="04"/>
    <fo:table-column column-width="{2.5}cm" column-number="05"/>
    <fo:table-column column-width="{9.25}cm" column-number="06"/>
  </xsl:template>

  <!-- EG 20160404 Migration vs2013 -->
  <xsl:template name="create1Column">
    <fo:table-column column-width="proportional-column-width(100)" column-number="01"/>
  </xsl:template>

  <!-- ************************* -->
  <!-- draw a break line row     -->
  <!-- ************************* -->
  <!-- EG 20160404 Migration vs2013 -->
  <!--<xsl:template name="displayBreakline">
    <xsl:param name ="pRowHeight" />
    <fo:table-row height="{$pRowHeight}cm">
      <fo:table-cell>
        <fo:block border="{$vBorder}"/>
      </fo:table-cell>
    </fo:table-row>
  </xsl:template>-->
  <xsl:template name="displayBreakline">
    <fo:table-row>
      <fo:table-cell>
        <fo:block linefeed-treatment="preserve" >
          <fo:table table-layout="fixed" width="100%">
            <xsl:call-template name ="create1Column"/>
            <fo:table-body>
              <fo:table-row height="{$vRowHeightBreakline}cm">
                <fo:table-cell border="{$vBorder}">
                  <fo:block border="{$vBorder}"/>
                </fo:table-cell>
              </fo:table-row>
            </fo:table-body>
          </fo:table>
        </fo:block>
      </fo:table-cell>
    </fo:table-row>
  </xsl:template>

  <!-- ************************* -->
  <!-- Label variables           -->
  <!-- ************************* -->
  <xsl:variable name ="vLabelAder">
    <xsl:text>Ader.:</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelComp">
    <xsl:text>M.I. Comparti Azionari e Derivati</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelReportName">
    <xsl:text>RP-MS22</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelValuta">
    <xsl:text>Valuta:</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelCassa">
    <xsl:text>CASSA DI COMPENSAZIONE E GARANZIA</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelConto">
    <xsl:text>Conto:</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelOmn">
    <xsl:text>*OMN</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelGruppoMercati">
    <xsl:text>Gruppo Mercati:</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelContrattazioni">
    <xsl:text>Contrattazioni del:  </xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelDescrizioneContratto">
    <xsl:text>Descrizione Contratto</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelPrice">
    <xsl:text>Prezzo/
Ammont.ITM</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelPosiz">
    <xsl:text>Posiz.
Cop./Str</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelNette">
    <xsl:text>Nette/
Ordin.</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelPremium">
    <xsl:text>Margini su
Premio/MTM</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelStraddle">
    <xsl:text>Straddle
Consegna</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelAdditional">
    <xsl:text>Margini
Addizionali</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelTotal">
    <xsl:text>Margini
Totali</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelOpen">
    <xsl:text>OPEN E/A</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelGrpProd">
    <xsl:text>Grp. Prod</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelOffset">
    <xsl:text>Offset</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelMrgMin">
    <xsl:text>Mrg.Minimo:</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelTotConto">
    <xsl:text>Totale Conto:</xsl:text>
  </xsl:variable>
  <xsl:variable name ="vLabelFattPond">
    <xsl:text>Fattore di ponderazione:</xsl:text>
  </xsl:variable>

  <!-- ================================================================================================== -->
  <!-- Business variables                                                                                 -->
  <!-- ================================================================================================== -->

  <!-- **************************** -->
  <!-- true is gross margin         -->
  <!-- false is net margin          -->
  <!-- **************************** -->
  <xsl:variable name ="vIsGross">
    <xsl:value-of select ="efs:EfsML/efs:marginRequirementOffice/@isGross"/>
  </xsl:variable>

  <!-- =================================================================================================== -->
  <!-- Business Templates                                                                                  -->
  <!-- =================================================================================================== -->

  <!-- **************************************************************** -->
  <!-- return trade date -->
  <!-- when parameter pCurrentCulture= 'en-GB' it returns: 6 JUL 2001-->
  <!-- when parameter pCurrentCulture= 'it-IT' it returns: 6 Luglio 2001-->
  <!-- **************************************************************** -->
  <xsl:template name ="getTradeDate">
    <xsl:variable name ="vTradedate">
      <xsl:call-template name="format-date">
        <xsl:with-param name="xsd-date-time" select="//efs:EfsML/efs:trade/efs:Trade/fpml:tradeHeader/fpml:tradeDate"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:value-of select="normalize-space($vTradedate)"/>
  </xsl:template>

  <!-- **************************************************************** -->
  <!-- return today date time (eg: 6 JUL 2001 + 14:45)-->
  <!-- **************************************************************** -->
  <xsl:template name ="getDateTime">
    <xsl:variable name ="vFormatDate">
      <xsl:call-template name ="getTradeDate"/>
    </xsl:variable>
    <xsl:variable name ="vFormatTime">
      <xsl:value-of select="//efs:EfsML/efs:trade/efs:Trade/fpml:tradeHeader/fpml:tradeDate/@timeStamp"/>
    </xsl:variable>
    <xsl:value-of select="concat (normalize-space($vFormatDate), ' ', $vFormatTime)"/>
  </xsl:template>

  <!-- **************************************************************** -->
  <!-- returns margin amount currency from marginRequirementOffice node -->
  <!-- **************************************************************** -->
  <xsl:template name ="getMarginAmountCurrency">
    <xsl:value-of select ="efs:EfsML/efs:marginRequirementOffice/efs:marginAmounts/efs:Money/fpml:currency"/>
  </xsl:template>

  <!-- **************************************************************** -->
  <!-- return formatted margin amount from marginRequirementOffice node -->
  <!-- **************************************************************** -->
  <xsl:template name ="getMROMarginAmount">
    <xsl:variable name ="vMarginAmount">
      <xsl:choose>
        <xsl:when test ="//efs:marginRequirementOffice/efs:marginAmounts">
          <xsl:value-of select="format-number(//efs:marginRequirementOffice/efs:marginAmounts/efs:Money/fpml:amount, '#.##0,00', 'decimal' )"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="format-number(0, '#.##0,00', 'decimal' )"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select ="$vMarginAmount"/>
  </xsl:template>

  <!-- **************************************************************** -->
  <!-- Returns unformatted amount from all types of amount nodes        -->
  <!-- **************************************************************** -->
  <xsl:template name ="getUnformattedAmount">
    <xsl:param name ="pNode"/>
    <xsl:variable name ="vAmount">
      <xsl:choose>
        <xsl:when test ="$pNode/efs:marginAmount/fpml:amount">
          <xsl:value-of select ="$pNode/efs:marginAmount/fpml:amount"/>
        </xsl:when>
        <xsl:when test ="$pNode/efs:amount/@amount">
          <xsl:value-of select ="$pNode/efs:amount/@amount"/>
        </xsl:when>
        <xsl:when test ="$pNode/efs:marginAmount/@amount">
          <xsl:value-of select ="$pNode/efs:marginAmount/@amount"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select ="$vAmount"/>
  </xsl:template>

  <!-- **************************** -->
  <!-- Returns Gruppo mercati       -->
  <!-- cross margin true = NET      -->
  <!-- cross margin false = DER/MTA -->
  <!-- *****************************-->
  <xsl:template name ="getMarketsGroup">
    <xsl:variable name="vCrossMarginFlag">
      <xsl:value-of select="//efs:marginRequirementOffice/efs:marginCalculation/efs:netMargin/efs:marginCalculationMethod[@name = $vDescribedMethod]/@crossMarging"/>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$vCrossMarginFlag='true'">
        <xsl:text>NET</xsl:text>
      </xsl:when>
      <xsl:when test="$vCrossMarginFlag='false'">
        <xsl:text>DER/MTA</xsl:text>
      </xsl:when>
      <!-- deposit brut-->
      <xsl:otherwise>
        <xsl:text>NA</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- **************************** -->
  <!-- Returns Entity               -->
  <!-- IT: Aderente mercato         -->
  <!-- **************************** -->
  <xsl:template name ="getEntity">
    <xsl:value-of select ="efs:EfsML/efs:marginRequirementOffice/efs:payerPartyReference/@href"/>
  </xsl:template>

  <!-- *********************************** -->
  <!-- Returns Margin Requirement office   -->
  <!-- Clearing Compartment                -->
  <!-- *********************************** -->
  <xsl:template name ="getMarginRequirementParty">
    <xsl:value-of select ="efs:EfsML/efs:marginRequirementOffice/efs:receiverPartyReference/@href"/>
  </xsl:template>

  <!-- *********************************** -->
  <!-- Returns Book                        -->
  <!-- *********************************** -->
  <xsl:template name ="getBook">
    <xsl:value-of select ="efs:EfsML/efs:marginRequirementOffice/efs:bookId/@bookName"/>
  </xsl:template>

  <!-- *********************************** -->
  <!-- Returns Class name                  -->
  <!-- eg. FTMIB                           -->
  <!-- *********************************** -->
  <xsl:template name ="getClassName">
    <xsl:param name ="pName"/>
    <xsl:value-of select ="normalize-space ($pName)"/>
  </xsl:template>

  <!-- ************************************************ -->
  <!-- Returns instrument                               -->
  <!-- if strike exists is a option else is a future    -->
  <!-- ************************************************ -->
  <xsl:template name ="getInstrument">
    <xsl:param name ="pStrkPx" />
    <xsl:param name ="pPutCall"/>
    <xsl:choose>
      <xsl:when test ="count ($pStrkPx)=0 ">Future</xsl:when>
      <xsl:when test ="count ($pStrkPx) &gt;= 1 ">Option</xsl:when>
      <xsl:otherwise>ERROR</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- ***********************************************************************************  -->
  <!-- Returns ContractType                                                                 -->
  <!-- IT: tipo contratto (comparto di appartenenza)                                        -->
  <!-- Source: manuale tecnico TIMS                                                         -->
  <!-- The symbol in the first column indicates the type of contract, therefore the Section -->
  <!-- F=futures or O=option                                                                -->
  <!-- ***********************************************************************************  -->
  <xsl:template name="getContractType">
    <xsl:param name="pInstrument"/>
    <xsl:choose>
      <xsl:when test ="$pInstrument='Future'">F </xsl:when>
      <xsl:when test ="$pInstrument='Option'">O </xsl:when>
      <xsl:otherwise>ERROR </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- **************************************  -->
  <!-- Returns option style Put or Call        -->
  <!-- PutCall=0 (Put)                         -->
  <!-- PutCall=1 (Call)                        -->
  <!-- **************************************  -->
  <xsl:template name="getOptionStyle">
    <xsl:param name ="pInstrument"/>
    <xsl:param name ="pPutCall"/>
    <xsl:choose>
      <xsl:when test ="$pInstrument = 'Option' and $pPutCall=0">Put</xsl:when>
      <xsl:when test ="$pInstrument = 'Option' and $pPutCall=1">Call</xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- ************************************************** -->
  <!-- Returns naming convention for IDEM instrument      -->
  <!-- [root][year][month][strike][corporate action flag] -->
  <!-- *************************************************  -->
  <xsl:template name="getSeriesCode">
    <xsl:param name ="pDisplayTranscodedSerie"/>
    <xsl:param name ="pInstrmtId"/>
    <xsl:param name ="pInstrument"/>
    <xsl:param name ="pOptionStyle"/>
    <xsl:param name ="pStrike"/>
    <xsl:param name ="pMMY"/>
    <xsl:param name ="pSym"/>
    <!-- eg.201208 returns 2 -->
    <xsl:variable name ="vLastNumberOfTheYear">
      <xsl:value-of select ="substring($pMMY,4,1)"/>
    </xsl:variable>
    <!-- eg.201208 returns 08 -->
    <xsl:variable name ="vMonth">
      <xsl:value-of select ="substring($pMMY,5,2)"/>
    </xsl:variable>
    <xsl:variable name ="vMonthCode">
      <xsl:choose>
        <!-- put option: M...X -->
        <xsl:when test ="($pInstrument = 'Option' and  $pOptionStyle='Put')">
          <xsl:choose>
            <xsl:when test ="$vMonth='01'">M</xsl:when>
            <xsl:when test ="$vMonth='02'">N</xsl:when>
            <xsl:when test ="$vMonth='03'">O</xsl:when>
            <xsl:when test ="$vMonth='04'">P</xsl:when>
            <xsl:when test ="$vMonth='05'">Q</xsl:when>
            <xsl:when test ="$vMonth='06'">R</xsl:when>
            <xsl:when test ="$vMonth='07'">S</xsl:when>
            <xsl:when test ="$vMonth='08'">T</xsl:when>
            <xsl:when test ="$vMonth='09'">U</xsl:when>
            <xsl:when test ="$vMonth='10'">V</xsl:when>
            <xsl:when test ="$vMonth='11'">W</xsl:when>
            <xsl:when test ="$vMonth='12'">X</xsl:when>
            <xsl:otherwise>ERROR</xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <!-- call option and future: A..L -->
        <xsl:otherwise>
          <xsl:choose>
            <xsl:when test ="$vMonth='01'">A</xsl:when>
            <xsl:when test ="$vMonth='02'">B</xsl:when>
            <xsl:when test ="$vMonth='03'">C</xsl:when>
            <xsl:when test ="$vMonth='04'">D</xsl:when>
            <xsl:when test ="$vMonth='05'">E</xsl:when>
            <xsl:when test ="$vMonth='06'">F</xsl:when>
            <xsl:when test ="$vMonth='07'">G</xsl:when>
            <xsl:when test ="$vMonth='08'">H</xsl:when>
            <xsl:when test ="$vMonth='09'">I</xsl:when>
            <xsl:when test ="$vMonth='10'">J</xsl:when>
            <xsl:when test ="$vMonth='11'">K</xsl:when>
            <xsl:when test ="$vMonth='12'">L</xsl:when>
            <xsl:otherwise>ERROR</xsl:otherwise>
          </xsl:choose>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- the corporate action is out of the scope in first release-->
    <xsl:variable name ="vCorporateActionFlag">
      <xsl:value-of select ="''"/>
    </xsl:variable>
    <!-- Build Serie Code from IDEM naming convention -->
    <xsl:choose>
      <xsl:when test ="$pDisplayTranscodedSerie=true()">
        <xsl:value-of select="concat($pSym,$vLastNumberOfTheYear,$vMonthCode,$pStrike,$vCorporateActionFlag)"/>
      </xsl:when>
      <!-- if parameter pDisplayTranscodedSerie is false display original serie name  -->
      <xsl:otherwise>
        <xsl:value-of select="$pInstrmtId"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- ************************************************** -->
  <!-- Returns full istrument id (with ISINCODE)          -->
  <!-- ************************************************** -->
  <xsl:template name ="getInstrumentId">
    <xsl:param name ="pNode"/>
    <xsl:value-of select ="$pNode/fixml:Instrmt/@ID"/>
  </xsl:template>

  <!-- ************************************************** -->
  <!-- Returns short istrument id (without ISINCODE)      -->
  <!-- ************************************************** -->
  <xsl:template name ="getShortInstrumentId">
    <xsl:param name ="pNode"/>

    <xsl:variable name="vInstrmtId">
      <xsl:value-of select ="$pNode/fixml:Instrmt/@ID"/>
    </xsl:variable>

    <xsl:variable name="vIsinCode">
      <xsl:value-of select ="concat(' ', $pNode/fixml:Instrmt/fixml:AID[@AltIDSrc='4']/@AltID)"/>
    </xsl:variable>

    <xsl:variable name="vShortInstrumentId">
      <xsl:value-of select ="substring-before($vInstrmtId,$vIsinCode)"/>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="$vShortInstrumentId=''">
        <xsl:value-of select="$vInstrmtId"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$vShortInstrumentId"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!-- ************************************************** -->
  <!-- Returns format Quote                               -->
  <!-- #.##0,0000                                         -->
  <!-- ************************************************** -->
  <xsl:template name ="formatQuote">
    <xsl:param name ="pQuote"/>
    <xsl:if test ="$pQuote != ''">
      <xsl:value-of select="format-number($pQuote, '#.##0,0000', 'decimal' )"/>
    </xsl:if>
  </xsl:template>

  <!-- ************************************************** -->
  <!-- Returns Quote from every margin nodes              -->
  <!-- ************************************************** -->
  <xsl:template name="getQuote">
    <xsl:param name ="pNode"/>
    <xsl:param name ="pInstrId"/>
    <xsl:call-template name ="formatQuote">
      <xsl:with-param name ="pQuote" select="$pNode/efs:factors/efs:factor[@ID=$pInstrId]/efs:Quote"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ************************************************************ -->
  <!-- Returns Quantity from every margin nodes excepts spread node -->
  <!-- ************************************************************ -->
  <xsl:template name ="getQty">
    <xsl:param name ="pNode"/>
    <xsl:if test ="$pNode/fixml:Qty[not(@Typ)]/@Long!=0">
      <xsl:value-of select ="concat($pNode/fixml:Qty[not(@Typ)]/@Long,'L')"/>
    </xsl:if>
    <xsl:if test ="$pNode/fixml:Qty[not(@Typ)]/@Short!=0">
      <xsl:value-of select ="concat($pNode/fixml:Qty[not(@Typ)]/@Short,'C')"/>
    </xsl:if>
  </xsl:template>

  <!-- ****************************************** -->
  <!-- Returns Quantity specific from spread node -->
  <!-- ****************************************** -->
  <xsl:template name ="getSpreadQuantity">
    <xsl:param name ="pNode"/>
    <xsl:param name ="pInstrId"/>
    <xsl:if test ="$pNode/efs:factors/efs:factor[@ID=$pInstrId]/efs:Qty/@Long!=0">
      <xsl:value-of select ="concat($pNode/efs:factors/efs:factor[@ID=$pInstrId]/efs:Qty/@Long,'L')"/>
    </xsl:if>
    <xsl:if test ="$pNode/efs:factors/efs:factor[@ID=$pInstrId]/efs:Qty/@Short!=0">
      <xsl:value-of select ="concat($pNode/efs:factors/efs:factor[@ID=$pInstrId]/efs:Qty/@Short,'C')"/>
    </xsl:if>
  </xsl:template>

  <!-- ****************************************** -->
  <!-- Returns Net Quantity specific for futures  -->
  <!-- Position quantity - Spread quantity        -->
  <!-- ****************************************** -->
  <xsl:template name ="getNetQuantity">
    <xsl:param name ="pNode"/>
    <xsl:param name ="pSpreadNode"/>
    <xsl:param name ="pInstrId"/>

    <xsl:variable name="vLongSpreadQuantity">
      <xsl:choose>
        <xsl:when test ="$pSpreadNode/efs:factors/efs:factor[@ID=$pInstrId]/efs:Qty">
          <xsl:value-of select ="$pSpreadNode/efs:factors/efs:factor[@ID=$pInstrId]/efs:Qty/@Long"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vShortSpreadQuantity">
      <xsl:choose>
        <xsl:when test ="$pSpreadNode/efs:factors/efs:factor[@ID=$pInstrId]/efs:Qty">
          <xsl:value-of select ="$pSpreadNode/efs:factors/efs:factor[@ID=$pInstrId]/efs:Qty/@Short"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vLongQuantity">
      <xsl:choose>
        <xsl:when test ="$pNode/fixml:Qty">
          <xsl:value-of select ="$pNode/fixml:Qty[not(@Typ)]/@Long"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vShortQuantity">
      <xsl:choose>
        <xsl:when test ="$pNode/fixml:Qty">
          <xsl:value-of select ="$pNode/fixml:Qty[not(@Typ)]/@Short"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vNetQuantity">
      <!-- GS 20131008: Improved futures net quantity (Position quantity - Spread quantity)-->
      <xsl:value-of select ="($vLongQuantity - $vShortQuantity) - ($vLongSpreadQuantity - $vShortSpreadQuantity)"/>
    </xsl:variable>

    <xsl:choose>
      <!-- net quantity greater than zero -->
      <xsl:when test ="$vNetQuantity &gt; 0">
        <xsl:value-of select ="concat($vNetQuantity,'L')"/>
      </xsl:when>
      <!-- net quantity less than zero -->
      <xsl:when test ="$vNetQuantity &lt; 0">
        <xsl:value-of select ="concat(($vNetQuantity * -1),'C')"/>
      </xsl:when>
      <xsl:otherwise>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!-- *********************************************************************** -->
  <!-- Returns quantity from every nodes without @Typ attribut                 -->
  <!-- *********************************************************************** -->
  <xsl:template name ="getQtyWithoutTyp">
    <xsl:param name ="pNode"/>
    <xsl:param name ="pInstrId"/>
    <xsl:if test ="$pNode/efs:factors/efs:factor[@ID=$pInstrId]/efs:Qty[not(@Typ)]/@Long!=0">
      <xsl:value-of select ="concat($pNode/efs:factors/efs:factor[@ID=$pInstrId]/efs:Qty[not(@Typ)]/@Long,'L')"/>
    </xsl:if>
    <xsl:if test ="$pNode/efs:factors/efs:factor[@ID=$pInstrId]/efs:Qty[not(@Typ)]/@Short!=0">
      <xsl:value-of select ="concat($pNode/efs:factors/efs:factor[@ID=$pInstrId]/efs:Qty[not(@Typ)]/@Short,'C')"/>
    </xsl:if>
  </xsl:template>

  <!-- *********************************************************************** -->
  <!-- Returns quantity from every nodes with specific @Typ attribut           -->
  <!-- *********************************************************************** -->
  <xsl:template name ="getQtyWithTyp">
    <xsl:param name ="pNode"/>
    <xsl:param name ="pInstrId"/>
    <xsl:param name ="pTyp"/>
    <xsl:choose>
      <xsl:when test ="$pTyp= 'EX'">
        <xsl:value-of select ="concat($pNode/efs:factors/efs:factor[@ID=$pInstrId]/efs:Qty[@Typ=$pTyp]/@Long,'E')"/>
      </xsl:when>
      <xsl:when test ="$pTyp= 'AS'">
        <xsl:value-of select ="concat($pNode/efs:factors/efs:factor[@ID=$pInstrId]/efs:Qty[@Typ=$pTyp]/@Short,'A')"/>
      </xsl:when>
      <!-- @Typ different to EX or AS-->
      <xsl:otherwise>
        <xsl:if test ="$pNode/efs:factors/efs:factor[@ID=$pInstrId]/efs:Qty[@Typ=$pTyp]/@Long!=0">
          <xsl:value-of select ="concat($pNode/efs:factors/efs:factor[@ID=$pInstrId]/efs:Qty[@Typ=$pTyp]/@Long,'L')"/>
        </xsl:if>
        <xsl:if test ="$pNode/efs:factors/efs:factor[@ID=$pInstrId]/efs:Qty[@Typ=$pTyp]/@Short!=0">
          <xsl:value-of select ="concat($pNode/efs:factors/efs:factor[@ID=$pInstrId]/efs:Qty[@Typ=$pTyp]/@Short,'C')"/>
        </xsl:if>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!-- *********************************************************************** -->
  <!-- Returns amount from every nodes with specific @Typ attribut             -->
  <!-- *********************************************************************** -->
  <xsl:template name ="getMarginAmountWithoutTyp">
    <xsl:param name ="pNode"/>
    <xsl:param name ="pInstrId"/>
    <xsl:param name ="pTyp"/>
    <xsl:variable name ="vAmount">
      <xsl:call-template name="getUnformattedAmount">
        <xsl:with-param name="pNode" select="$pNode/efs:factors/efs:factor[@ID=$pInstrId]"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:call-template name ="formatAmount">
      <xsl:with-param name ="pAmount" select="$vAmount"/>
    </xsl:call-template>
  </xsl:template>

  <!-- *********************************************************************** -->
  <!-- Returns amount from every nodes with specific @Typ attribut             -->
  <!-- *********************************************************************** -->
  <xsl:template name ="getMarginAmountWithTyp">
    <xsl:param name ="pNode"/>
    <xsl:param name ="pInstrId"/>
    <xsl:param name ="pTyp"/>
    <xsl:variable name ="vAmount">
      <xsl:call-template name="getUnformattedAmount">
        <xsl:with-param name="pNode" select="$pNode/efs:factors/efs:factor[@ID=$pInstrId and efs:Qty[@Typ=$pTyp]]"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:call-template name ="formatAmount">
      <xsl:with-param name ="pAmount" select="$vAmount"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ************************************************** -->
  <!-- Returns C symbol for negative amount (else null)   -->
  <!-- ************************************************** -->
  <xsl:template name="AmountSymbol">
    <xsl:param name ="pNode"/>
    <xsl:variable name="vAmount">
      <xsl:call-template name="getUnformattedAmount">
        <xsl:with-param name="pNode" select="$pNode"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:if test ="substring($vAmount,1,1)='-'">
      <xsl:text>C</xsl:text>
    </xsl:if>

  </xsl:template>

  <!-- **************************** -->
  <!-- Return amount in TIMS format -->
  <!-- all amounts are positive     -->
  <!-- **************************** -->
  <xsl:template name ="formatAmount">
    <xsl:param name ="pAmount"/>
    <xsl:if test ="$pAmount != ''">
      <xsl:if test ="substring($pAmount,1,1)='-'">
        <xsl:value-of select="format-number(substring-after($pAmount,'-'), '#.##0,00', 'decimal' )"/>
      </xsl:if>
      <xsl:if test ="substring($pAmount,1,1)!='-'">
        <xsl:value-of select="format-number($pAmount, '#.##0,00', 'decimal' )"/>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <!-- **************************** -->
  <!-- Return Minimun Margin Group  -->
  <!-- **************************** -->
  <xsl:template name ="getMinimumMarginGroup">
    <xsl:param name ="pCurrent"/>
    <xsl:variable name ="vMinimum">
      <xsl:choose>
        <xsl:when test ="$pCurrent/efs:minimum">
          <xsl:call-template name="getUnformattedAmount">
            <xsl:with-param name="pNode" select="$pCurrent/efs:minimum"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="format-number($vMinimum, '#.##0,00', 'decimal' )"/>
  </xsl:template>

  <!-- **************************** -->
  <!-- Return total premium amount  -->
  <!-- **************************** -->
  <xsl:template name ="getTotalPremium">
    <xsl:param name ="pCurrent"/>
    <xsl:variable name ="vTotalPremium">
      <xsl:choose>
        <xsl:when test ="$pCurrent/efs:premium">
          <xsl:call-template name="getUnformattedAmount">
            <xsl:with-param name="pNode" select="$pCurrent/efs:premium"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!--<xsl:value-of select="format-number($vTotalPremium, '#.##0,00', 'decimal' )"/>-->
    <xsl:call-template name ="formatAmount">
      <xsl:with-param name ="pAmount" select="$vTotalPremium"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ***************************************** -->
  <!-- Returns total straddle for product        -->
  <!-- ***************************************** -->
  <xsl:template name ="getTotalStraddle">
    <xsl:param name ="pCurrent"/>
    <xsl:variable name ="vTotalStraddle">
      <xsl:choose>
        <xsl:when test ="$pCurrent/efs:spread">
          <xsl:call-template name="getUnformattedAmount">
            <xsl:with-param name="pNode" select="$pCurrent/efs:spread"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="format-number($vTotalStraddle, '#.##0,00', 'decimal' )"/>
  </xsl:template>

  <!-- ***************************************** -->
  <!-- Returns total additional for product      -->
  <!-- ***************************************** -->
  <xsl:template name ="getTotalAdditional">
    <xsl:param name ="pCurrent"/>
    <xsl:variable name ="vTotalAdditional">
      <xsl:choose>
        <xsl:when test ="$pCurrent/efs:additional">
          <xsl:call-template name="getUnformattedAmount">
            <xsl:with-param name="pNode" select="$pCurrent/efs:additional"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="format-number($vTotalAdditional, '#.##0,00', 'decimal' )"/>
  </xsl:template>

  <!-- ***************************************** -->
  <!-- Returns margin amount for product         -->
  <!-- ***************************************** -->
  <xsl:template name ="getMarginAmount">
    <xsl:param name ="pNode"/>
    <xsl:variable name ="vMarginAmount">
      <xsl:choose>
        <xsl:when test ="$pNode/efs:marginAmount">
          <xsl:choose>
            <xsl:when test ="$pNode/efs:marginAmount/fpml:amount">
              <xsl:value-of select ="$pNode/efs:marginAmount/fpml:amount"/>
            </xsl:when>
            <xsl:when test ="$pNode/efs:marginAmount/efs:amount/@amount">
              <xsl:value-of select ="$pNode/efs:marginAmount/efs:amount/@amount"/>
            </xsl:when>
            <xsl:when test ="$pNode/efs:marginAmount/@amount">
              <xsl:value-of select ="$pNode/efs:marginAmount/@amount"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select ="0"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!--<xsl:value-of select="format-number($vMarginAmount, '#.##0,00', 'decimal' )"/>-->
    <xsl:call-template name ="formatAmount">
      <xsl:with-param name ="pAmount" select="$vMarginAmount"/>
    </xsl:call-template>
  </xsl:template>

</xsl:stylesheet>