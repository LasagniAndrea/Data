<?xml version="1.0" encoding="ISO-8859-1" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"  version="1.0">

  <!--
	==============================================================
	Summary : Swaption ISDA Business
	==============================================================
	Revision: 2
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
  ==============================================================================================
  Revision: 1
	Date    : 24/09/2009
	Author  : Gianmario SERIO
	Version : 2.3RTM_1
	Updates : Add Calculation Period Different section. This section contains:
	          - Variable varSwapStreamsCount: it returns swap streams number
			  - Variable getStream1UnadjustedEffectiveDate: it returns Stream1 unadjusted effective date
	          - Variable getStream1UnadjustedTerminationDate: it returns Stream1 unadjusted termination date
			  - Variable getIsStreamsTerminationDatesDifferent: it returns 'true' if the effective dates are different
			  - Variable getIsStreamsTerminationDatesDifferent: it returns 'true' if the termination dates are different
			  - Variable IsCalculationPeriodDatesDifferent: "true" if distinct streams have different CalculationPeriod
			  
			  Update variable IsNotionalDifferent: it returns "true" if distinct streams have different notional amounts	
	==============================================================	
	File    : Swaption_ISDA_Business.xslt
	Date    : 20.05.2009
	Author  : Gianmario SERIO
	Version : 2.3.0.3_1
	==============================================================
	-->

  <!-- Define the parties id (HREF) of the actors who pay and receive the financial flows -->

  <!-- Parties variables -->
  <xsl:variable name="partyID_1" select="//dataDocument/trade/swaption/buyerPartyReference/@href"/>
  <xsl:variable name="partyID_2" select="//dataDocument/trade/swaption/sellerPartyReference/@href"/>

  <!-- IsCrossCurrency variable: "true" if the swap is a cross currency swap -->
  <xsl:variable name="isCrossCurrency" select="(//dataDocument/trade/swaption/swap/swapStream[2]/node())
			and
			(//dataDocument/trade/swaption/swap/swapStream/calculationPeriodAmount/calculation/node())
			and
			(//dataDocument/trade/swaption/swap/swapStream[1]/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/currency
			!=//dataDocument/trade/swaption/swap/swapStream[2]/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/currency)">
  </xsl:variable>

  <!-- IsStream1NotionalStepSchedule variable: "true" if the notional has some steps -->
  <xsl:variable name="isStream1NotionalStepSchedule" select="//dataDocument/trade/swaption/swap/swapStream/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/step/node()"/>

  <!-- GS 20110214 handle zero coupon swap -->
  <!-- Gets the initial value of the notionalStepSchedule of the first stream for vanilla swap-->
  <!-- Gets the initial value of the knownAmountSchedule of the first stream for zero coupon swap 
       (in a zero coupon swap the first stream is always the stream zero coupon)-->
  <xsl:variable name="getStream1NotionalStepScheduleInitialValue">

    <xsl:if test="//dataDocument/trade/swaption/swap/swapStream[1]/calculationPeriodAmount/calculation/node()">
      <xsl:value-of select="//dataDocument/trade/swaption/swap/swapStream[1]/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/initialValue"/>
    </xsl:if>
    <xsl:if test="//dataDocument/trade/swaption/swap/swapStream[1]/calculationPeriodAmount/knownAmountSchedule/node()">
      <xsl:value-of select="//dataDocument/trade/swaption/swap/swapStream[1]/calculationPeriodAmount/knownAmountSchedule/initialValue"/>
    </xsl:if>
  </xsl:variable>

  <!-- GS 20110214 handle zero coupon swap -->
  <!-- "true" if the first stream initial value is different to the last stream initial value-->
  <xsl:variable name ="isNotionalDifferent">
    <xsl:for-each select ="//dataDocument">
      <xsl:choose>
        <xsl:when test ="$getStream1NotionalStepScheduleInitialValue != ./trade/swaption/swap/swapStream[last()]/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/initialValue">
          <xsl:value-of select="true()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="false()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </xsl:variable>

  <!-- varSwapStreamsCount variable: it returns swap streams number-->
  <xsl:variable name ="varSwapStreamsCount">
    <xsl:value-of select ="count(//dataDocument/trade/swaption/swap/swapStream)"/>
  </xsl:variable>

  <!-- getStream1UnadjustedEffectiveDate variable: it returns Stream1 unadjusted effective date -->
  <xsl:variable name ="getStream1UnadjustedEffectiveDate">
    <xsl:value-of select ="//dataDocument/trade/swaption/swap/swapStream[1]/calculationPeriodDates/effectiveDate/unadjustedDate"/>
  </xsl:variable>

  <!-- getStream1UnadjustedTerminationDate variable: it returns Stream1 unadjusted termination date -->
  <xsl:variable name ="getStream1UnadjustedTerminationDate">
    <xsl:value-of select ="//dataDocument/trade/swaption/swap/swapStream[1]/calculationPeriodDates/terminationDate/unadjustedDate"/>
  </xsl:variable>

  <!-- getIsStreamsTerminationDatesDifferent variable: it returns 'true' if the effective dates are different -->
  <xsl:variable name ="getIsStreamsEffectiveDatesDifferent">
    <xsl:for-each select ="//dataDocument">
      <xsl:choose>
        <xsl:when test ="$getStream1UnadjustedEffectiveDate != ./trade/swaption/swap/swapStream/calculationPeriodDates/effectiveDate/unadjustedDate">
          <!-- if one effective dates is different from the first one in the streams: it returns 'true'  -->
          <xsl:text>true</xsl:text>
        </xsl:when>
        <!-- if all effective dates are equal to the first one in the streams: it returns 'false' -->
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </xsl:variable>

  <!-- getIsStreamsTerminationDatesDifferent variable: it returns 'true' if the termination dates are different -->
  <xsl:variable name ="getIsStreamsTerminationDatesDifferent">
    <xsl:for-each select ="//dataDocument">
      <xsl:choose>
        <xsl:when test ="$getStream1UnadjustedTerminationDate != ./trade/swaption/swap/swapStream/calculationPeriodDates/terminationDate/unadjustedDate">
          <!-- if one termination dates is different from the first one in the streams: it returns 'true'  -->
          <xsl:text>true</xsl:text>
        </xsl:when>
        <!-- if all termination dates are equal to the first one in the streams: it returns 'false' -->
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </xsl:variable>

  <!-- IsTermDifferent variable: "true" if distinct streams have different CalculationPeriod  -->
  <xsl:variable name ="isTermDifferent">
    <xsl:choose>
      <xsl:when test ="$getIsStreamsEffectiveDatesDifferent = 'true' or $getIsStreamsTerminationDatesDifferent = 'true'">
        <xsl:text>true</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>false</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>


  <!-- GS 20110214 Handle stub (for two streams swap or multistreams swap) -->

  <!-- it returns the number of the firstRegularPeriodStartDate nodes -->
  <xsl:variable name ="countFirstRegularPeriodStartDate">
    <xsl:value-of select ="count(//dataDocument/trade/swaption/swap/swapStream/calculationPeriodDates/firstRegularPeriodStartDate)"/>
  </xsl:variable>
  <!-- it returns the number of the lastRegularPeriodEndDate nodes -->
  <xsl:variable name ="countLastRegularPeriodEndDate">
    <xsl:value-of select ="count(//dataDocument/trade/swaption/swap/swapStream/calculationPeriodDates/lastRegularPeriodEndDate)"/>
  </xsl:variable>

  <!-- true if at least one stub exists -->
  <xsl:variable name ="isStub">
    <xsl:choose>
      <xsl:when test="$countFirstRegularPeriodStartDate=0 and countLastRegularPeriodEndDate=0">
        <xsl:value-of select ="false()"/>
      </xsl:when>
      <xsl:when test="$countFirstRegularPeriodStartDate &gt; 0 or countLastRegularPeriodEndDate &gt; 0">
        <xsl:value-of select ="true()"/>
      </xsl:when>
    </xsl:choose>
  </xsl:variable>

  <!--it returns the first node that contains a first regular period start date-->
  <xsl:variable name ="getFirstRegularPeriodStartDate1">
    <xsl:value-of select ="//dataDocument/trade/swaption/swap/swapStream/calculationPeriodDates/firstRegularPeriodStartDate[1]"/>
  </xsl:variable>
  <!--it returns the first node that contains a last regular period end date-->
  <xsl:variable name ="getLastRegularPeriodEndDate1">
    <xsl:value-of select ="//dataDocument/trade/swaption/swap/swapStream/calculationPeriodDates/lastRegularPeriodEndDate[1]"/>
  </xsl:variable>

  <!-- compare the first node that contains a first regular period start date with the last node that contains a first regular period start date -->
  <!-- true if the dates are different-->
  <!-- false if the stub has the same dates -->
  <!-- false if only one stream is stub -->
  <xsl:variable name ="isAsynchronousFirstRegularPeriodStartDate">
    <xsl:for-each select ="//dataDocument">
      <xsl:choose>
        <xsl:when test ="$getFirstRegularPeriodStartDate1 != ./trade/swaption/swap/swapStream/calculationPeriodDates/firstRegularPeriodStartDate[last()]">
          <xsl:value-of select ="true()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="false()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </xsl:variable>

  <!-- compare the first node that contains a last regular period end date with the last node that contains a last regular period end date -->
  <!-- true if the dates are different-->
  <!-- false if the stub has the same dates -->
  <!-- false if only one stream is stub -->
  <xsl:variable name ="isAsynchronousLastRegularPeriodEndDate">
    <xsl:for-each select ="//dataDocument">
      <xsl:choose>
        <xsl:when test ="$getLastRegularPeriodEndDate1 != ./trade/swaption/swap/swapStream/calculationPeriodDates/lastRegularPeriodEndDate[last()]">
          <xsl:value-of select ="true()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="false()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </xsl:variable>

  <!-- Stubs are asynchronous when -->
  <!-- only one stream has a stub -->
  <!-- two or more streams have stubs where the dates are different -->
  <xsl:variable name ="isAsynchronousStub">
    <xsl:choose>
      <xsl:when test="$countFirstRegularPeriodStartDate = 1">
        <xsl:value-of select ="true()"/>
      </xsl:when>
      <xsl:when test="$countFirstRegularPeriodStartDate &gt; 1 and $isAsynchronousFirstRegularPeriodStartDate = true()">
        <xsl:value-of select ="true()"/>
      </xsl:when>
      <xsl:when test="$countLastRegularPeriodEndDate = 1">
        <xsl:value-of select ="true()"/>
      </xsl:when>
      <xsl:when test="$countLastRegularPeriodEndDate &gt; 1 and $isAsynchronousLastRegularPeriodEndDate = true()">
        <xsl:value-of select ="true()"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select ="false()"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

</xsl:stylesheet>