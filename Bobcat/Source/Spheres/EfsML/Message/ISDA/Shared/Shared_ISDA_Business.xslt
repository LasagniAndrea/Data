<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"  version="1.0">
  <!--
	=============================================================================================================================
	Summary : Shared ISDA Business
	================================================================================================================
  Version : v3.7.5155                                           
  Date    : 20140220
  Author  : RD
  Comment : [19612] Add scheme name for "tradeId" and "bookId" XPath checking  
  =============================================================================================================================
	Revision        : 5
	Date            : 29/11/2011
	Author          : Gianmario SERIO
	Spheres Version : 2.6
  Update          : Swap confirmation update: Handle Offset into fixed and floating streams [OIS] see GS 20111129 comments
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
  ==================================================================================================================== 
  Revision    : 3	
	Version     : 2.3.0.9_3
	Date        : 09.10.2009
	Author      : Gianmario SERIO
	Updates     : Bug fix: It returns right stream position in events XML flux
	=====================================================================================================================
	Revision    : 2
	Version     : 2.3.0.3_3
	Date        : 31.08.2009
	Author      : Gianmario SERIO
	Updates     : Add Annex region: it contains
	              - Template getScheduledNominalAnnexNumber: it returns annex number
				        into this template there are:
					      - Variable varNotionalStepParametersCount: notionalStepParameters count value for current stream (if the node exists = 1)
	              - Variable varNotionalStepCount: NotionalStep count value for current stream (to do: test when it returns more steps)
					      - Variable varIsScheduledNominalCurrentStream: It returns 1 if there are scheduled nominal in the current stream
					      - Variable varScheduledNominalPrecedingStreamNumber: It returns how many preceding streams contain scheduled nominal
                - Template getLetterFromNumber: it converts digits to letters (1 to 9) -> (A to I)
                Update template handleIrdStreams:
	              - Add parameter pIsScheduledNominalDisplayInAnnex
				        - Add parameter pIsScheduledNominalDisplayInAnnex in displayFixedRateSchedule call template
				        - Add parameter pIsScheduledNominalDisplayInAnnex in displayFloatingRateCalculation call template
                Update template getDateRange:
                - the definition 'through and including' has been replaced by the definition 'up to and including'				 
				        Update template getPaymentDatesFrequency:
				        - the definition 'through and including the termination date' has been replaced by the definition 'up to and including the termination date'
				        Update template getCalculationPeriodDatesFrequency:
				        - the definition 'through and including the termination date' has been replaced by the definition 'up to and including the termination date'
	              Update template handleIrdStreams:
	              - Add parameter pIsCalculationPeriodDatesDifferent
				        - Add parameter pIsCalculationPeriodDatesDifferent in displayFixedRateSchedule call template
				        - Add parameter pIsCalculationPeriodDatesDifferent in displayFloatingRateCalculation call template
				        - Rename parameter 'pIsCollar' in 'pCapFloorType'
				        - Add variable 'varPosition': it returns the position of the stream       	
	              Add Provisions region: it menages cancelation or early termination provisions
			          It contains: 
				        - Template handleProvisions: it handle provisione region calling the correct templates for each provision.
				        - Template getProvisionOptionStyle: it returns provision option style (european, american, bermuda)
				        - Template getProvisionCalculationAgent: it returns Provision Calculation Agent
				        - Template getRelevantUnderlyngAdjustableOrRelativeDates: 
				          it returns the early termination date (when it is adjustable or relative type)
				        - Template getProvisionsAdjustableOrRelativeOrRangeDate: 
				          it returns either the adjustable or the relative  or daterange date only when the node contains the path adjustableDate or relativeDate (without 's') or businessDateRange
				        - Template getDateRange: it returns the date range [eg.Commencing on 'unadjustedFirstDate' through and including 'unadjustedLastDate' + business day convention]
				        - Template getDateRelativeToDisplayName: it returns the correct display name in 'Date Relative To'
				          [eg. when the path <DateRelativeTo> contains 'OET_CashSettlementPaymentDate' it returns the name 'Cash Settlement Payment Date']      
				        Add template getFloatingStubRate: it displays the floating stub rate (for initialstub or finalstub)
	              - This template manages floating rate interpolations      
				        Add template getFixedStubRate: it displays the fixed stub rate (for initialstub or finalstub) 
				        Rename template getTouchCondition in getTriggerEventType
	              - Now it manages two triggers (only for fxdigitaloptions)	  
				        Update template "handleFloatingRatePayer"  for Cap and Floor
				        Update variable "varCapFloorType"  (Corridor section)
				        Add variable "varCapRate2" (only for Corridor): it returns 2° cap rate (initial value)
				        Update template handleCapOrFloorRate
				        - Add rate schedule for Corridor (Cap rate1 and Cap rate2)
	===================================================================================
	Revision    : 1	
	Version     : 2.3.0.3_2
	Date        : 03.07.2009
	Author      : Domenico Rotta
	Updates     : add variable "varIsFxSwap"
	===================================================================================
	Version     : 2.3.0.3_1
	Date        : 20.05.2009
	Author      : Gianmario SERIO
	==============================================================
	-->

  <!-- 
	===============================================
	===============================================
	== All products: BEGIN REGION     =============
	===============================================
	===============================================
	-->
  <!-- 
	========================
	== Variables         === 
	========================
	-->
  <xsl:variable name="varCreationTimestamp">
    <xsl:value-of select="//header/creationTimestamp"/>
  </xsl:variable>

  <xsl:variable name="varLogo">
    <xsl:value-of select="concat('sql(select IDENTIFIER, LOLOGO from dbo.ACTOR where IDA=', //header/sendBy/routingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/Actorid'], ')')"/>
  </xsl:variable>

  <!--  xsl:variable name="isValidMasterAgreementDate" select="not( contains( $pMasterAgreementDtSignature, '0001-01-01T00:00:00') )"/ -->
  <xsl:variable name="isValidMasterAgreementDate" select="not( $masterAgreementDtSignature = '' )"/>

  <!-- 
	==================================================================================
	== Begin Test of Path Variable in Display Terms Region							
	================================================================================== 
	-->
  <xsl:variable name="getCurrency" select="calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/currency"/>
  <xsl:variable name="getNotionalStepSchedule" select="calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule"/>

  <!-- 
	================================================================================== 
	== End Test of Path Variable in Display Terms Region					
	================================================================================== 
	-->
  <!-- 
	=================================================== 
	== BEGIN region about parties and related offices
	== Template getPartyNameOrID: 
	== return the name of the party having the ID "pPartyID"; if the name is not provided return the ID.
	== The "pPartyID" is referred to the attribute "id" of the node "/confirmationMessage/datadocument/party",
	== which correponds to the attribute "href" of the parties' references. 
	== E.g.
	== /dataDocument/trade/tradeHeader/partyTradeIdentifier/partyReference
	== /confirmationMessage/header/sendBy
	== /confirmationMessage/header/sendTo
	== used by varParty_1_name, varParty_2_name, displayBrokerage (fees payer and receiver)
	=================================================== 

	==========================================================================================================
    == getPartyNameOrID
	==========================================================================================================
	-->
  <xsl:template name="getPartyNameOrID">
    <xsl:param name="pPartyID"/>

    <xsl:for-each select="//party">
      <xsl:variable name="curr" select="current()"/>
      <xsl:if test="$curr/@id=$pPartyID">
        <xsl:choose>
          <xsl:when test="$curr/partyName/node()=true()">
            <xsl:value-of select="$curr/partyName"/>
          </xsl:when>
          <xsl:otherwise>
            <!-- 20090416 PL Multiple parties -->
            <!-- xsl:value-of select="$curr/partyId"/ -->
            <xsl:value-of select="$curr/partyId[1]"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <!-- 
	=====================================================================================================
	Template getPartyNameAndID 
	return the name and the ID of the party having the ID (HREF) "pParty" 
	used by getPartyID, getPartyTo, getPartyFrom, getCalculationAgent, displayTerms (Seller, Buyer) 
	=====================================================================================================
	-->
  <xsl:template name="getPartyNameAndID">
    <xsl:param name="pParty"/>
    <xsl:param name="pIsDisplayIdAndName" select="false()"/>

    <xsl:if test="$pParty = $partyID_1">
      <xsl:value-of select="$varParty_1_name" />
      <xsl:if test="$pIsDisplayIdAndName and $partyID_1!=$varParty_1_name">
        <xsl:text>(</xsl:text>
        <xsl:value-of select="$partyID_1" />
        <xsl:text>)</xsl:text>
      </xsl:if>
    </xsl:if>

    <xsl:if test="$pParty = $partyID_2">
      <xsl:value-of select="$varParty_2_name" />
      <xsl:if test="$pIsDisplayIdAndName and $partyID_2!=$varParty_2_name">
        <xsl:text>(</xsl:text>
        <xsl:value-of select="$partyID_2" />
        <xsl:text>)</xsl:text>
      </xsl:if>
    </xsl:if>
  </xsl:template>
  <!-- 
	=====================================================================================================
	getPartyID template
	used by displayExchange, getPartyIDTo, getPartyIDTo
	=====================================================================================================
	-->
  <xsl:template name="getPartyID">
    <xsl:param name="pSeparator"/>
    <xsl:param name="pDelimiter"/>
    <xsl:param name="pPos"/>
    <xsl:if test="$pPos=1">
      <xsl:call-template name="getPartyNameAndID" >
        <xsl:with-param name="pParty" select="$partyIDFrom" />
        <xsl:with-param name="pIsDisplayIdAndName" select="false()" />
      </xsl:call-template>
    </xsl:if>
    <xsl:if test="$pPos &gt; 1">
      <xsl:call-template name="getPartyNameAndID" >
        <xsl:with-param name="pParty" select="$partyIDTo" />
        <xsl:with-param name="pIsDisplayIdAndName" select="false()" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <!-- Footer_Product SendByPartyID_1-->
  <xsl:variable name="SendByPartyID_1">
    <xsl:choose>
      <xsl:when test="//header/sendBy/partyRelativeTo/@href = $partyID_1">
        <xsl:call-template name="getContactOfficeAddress">
          <xsl:with-param name="pContactOffice" select="//header/sendBy"/>
          <xsl:with-param name="pIsCrLf" select="true()"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="getContactOfficeAddress">
          <xsl:with-param name="pContactOffice" select="//header/sendTo"/>
          <xsl:with-param name="pIsCrLf" select="true()"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- Footer_Product SendByPartyID_2-->
  <xsl:variable name="SendByPartyID_2">
    <xsl:choose>
      <xsl:when test="//header/sendBy/partyRelativeTo/@href = $partyID_2">
        <xsl:call-template name="getContactOfficeAddress">
          <xsl:with-param name="pContactOffice" select="//header/sendBy"/>
          <xsl:with-param name="pIsCrLf" select="true()"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="getContactOfficeAddress">
          <xsl:with-param name="pContactOffice" select="//header/sendTo"/>
          <xsl:with-param name="pIsCrLf" select="true()"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- sendByRoutingAddress -->
  <xsl:variable name="sendByRoutingAddress">
    <xsl:call-template name="getContactOfficeAddress">
      <xsl:with-param name="pContactOffice" select="//header/sendBy"/>
      <xsl:with-param name="pIsCrLf" select="true()"/>
    </xsl:call-template>
  </xsl:variable>

  <!-- sendToRoutingAddress -->
  <xsl:variable name="sendToRoutingAddress">
    <xsl:call-template name="getContactOfficeAddress">
      <xsl:with-param name="pContactOffice" select="//header/sendTo"/>
      <xsl:with-param name="pIsCrLf" select="true()"/>
    </xsl:call-template>
  </xsl:variable>

  <!-- Get the ID of the SendTo -->
  <xsl:template name="getSendToID">
    <xsl:value-of select="/confirmationMessage/header/sendTo[1]/@href"/>
  </xsl:template>

  <!-- 
	it returns the recipient of confirmation partyID 
	-->
  <xsl:variable name="varSendToPartyID">
    <xsl:value-of select="//sendTo[1]/routingIdsAndExplicitDetails/routingIds/routingId[1]"/>
  </xsl:variable>

  <!-- Get the name of the SendTo -->
  <!-- 
	new version (to conform of preceding 2.3 OTCmL version)
	(@href) in //header/sendTo node is available from 2.3 OTCmL version
	Before (eg. in OTCML version 2.2) the (@href)don't exist
	This template verifies if the node //header/sendTo contains (@href)
	when the condition is true it return the standard partyName 
	otherwise (eg. in OTCmL version 2.2) it use the variable address1
	-->
  <xsl:template name="getSendTo">
    <xsl:choose>
      <xsl:when test ="//header/sendTo[1]/@href">
        <xsl:value-of select="//dataDocument/party[@id=//header/sendTo[1]/@href]/partyName"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$address1"/>
      </xsl:otherwise>
    </xsl:choose>
    <!--
		<xsl:value-of select="//header/sendTo/routingIdsAndExplicitDetails/routingName"/>
		<xsl:if test="(//dataDocument/party[@id=//header/sendTo/@href]/node()=true()) and (//dataDocument/party[@id=//header/sendTo/@href]/partyName != //header/sendTo/routingIdsAndExplicitDetails/routingName)">
			<xsl:text> - </xsl:text>
		</xsl:if>
		-->
  </xsl:template>

  <!-- Get the ID of the SendBy -->
  <xsl:template name="getSendByID">
    <xsl:value-of select="/confirmationMessage/header/sendBy/@href"/>
  </xsl:template>

  <!-- Get the name of the SendBy -->
  <xsl:template name="getSendBy">
    <!--
		<xsl:value-of select="//header/sendBy/routingIdsAndExplicitDetails/routingName"/>
		<xsl:if test="(//dataDocument/party[@id=//header/sendBy/@href]/node()=true()) and (//dataDocument/party[@id=//header/sendBy/@href]/partyName != //header/sendBy/routingIdsAndExplicitDetails/routingName)">
			<xsl:text> - </xsl:text>
			<xsl:value-of select="//dataDocument/party[@id=//header/sendBy/@href]/partyName"/>
		</xsl:if>
		-->
    <xsl:value-of select="//dataDocument/party[@id=//header/sendBy/@href]/partyName"/>
  </xsl:template>

  <!-- set the name of the party_1 -->
  <xsl:variable name="varParty_1_name">
    <xsl:call-template name="getPartyNameOrID">
      <xsl:with-param name="pPartyID" select="$partyID_1"/>
    </xsl:call-template>
  </xsl:variable>

  <!-- set the name of the party_2 -->
  <xsl:variable name="varParty_2_name">
    <xsl:call-template name="getPartyNameOrID">
      <xsl:with-param name="pPartyID" select="$partyID_2"/>
    </xsl:call-template>
  </xsl:variable>

  <!-- Return the party id (HREF) of the actor who receives the confirmation -->
  <xsl:template name="getPartyIDTo">
    <xsl:value-of select="$partyIDTo" />
  </xsl:template>

  <!-- Define the party id (HREF) of the actor who receives the confirmation -->
  <xsl:variable name="partyIDTo">
    <xsl:choose>
      <xsl:when test="$partyIDFrom = $partyID_1">
        <!-- 
		the variables partyID_1 and partyID_2 are defined in the product-specific XSL file (e.g. SWAP_ISDA_Business.xslt)
		and they define the parties id (HREF) of the actors who pay and receive the financial flows 
		-->
        <xsl:value-of select="$partyID_2"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$partyID_1"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- getPartyTo -->
  <xsl:template name="getPartyTo">
    <xsl:call-template name="getPartyNameAndID" >
      <xsl:with-param name="pParty" select="$partyIDTo" />
      <xsl:with-param name="pIsDisplayIdAndName" select="true()" />
    </xsl:call-template>
  </xsl:template>

  <!-- Return the party id (HREF) of the actor who sends the confirmation -->
  <xsl:template name="getPartyIDFrom">
    <xsl:value-of select="$partyIDFrom" />
  </xsl:template>

  <!-- Define the party id (HREF) of the actor who sends the confirmation -->
  <xsl:variable name="partyIDFrom">
    <xsl:value-of select="//header/sendBy/partyRelativeTo/@href"/>
  </xsl:variable>

  <!-- getPartyFrom -->
  <xsl:template name="getPartyFrom">
    <xsl:call-template name="getPartyNameAndID" >
      <xsl:with-param name="pParty" select="$partyIDFrom" />
      <xsl:with-param name="pIsDisplayIdAndName" select="true()" />
    </xsl:call-template>
  </xsl:template>

  <!-- set the book ID of the party having the HREF equal to the "partyID_1" -->
  <xsl:variable name="bookID_1">
    <xsl:value-of select="//dataDocument/trade/tradeHeader/partyTradeIdentifier/partyReference[@href=$partyID_1]/../bookId[@bookIdScheme='http://www.euro-finance-systems.fr/otcml/bookid']"/>
  </xsl:variable>

  <!-- set the book ID of the party having the HREF equal to the "partyID_2" -->
  <xsl:variable name="bookID_2">
    <xsl:value-of select="//dataDocument/trade//tradeHeader/partyTradeIdentifier/partyReference[@href=$partyID_2]/../bookId[@bookIdScheme='http://www.euro-finance-systems.fr/otcml/bookid']"/>
  </xsl:variable>

  <!-- footer1 variable -->
  <xsl:variable name="footer1">
    <xsl:choose>
      <xsl:when test="(//dataDocument/party[@id=//header/sendBy/@href]/node()=true()) and 
				(//dataDocument/party[@id=//header/sendBy/@href]/partyName != //header/sendBy/routingIdsAndExplicitDetails/routingName)">
        <xsl:value-of select="//header/sendBy/routingIdsAndExplicitDetails/routingName"/>
        <!--
				add in comment: 
				OTCml 2.2.0 version generate XML node:<partyName>MYBANK </partyName>(blank space after the partyname)
				and the condition 
				//dataDocument/party[@id=//header/sendBy/@href]/partyName != //header/sendBy/routingIdsAndExplicitDetails/routingName)" is missing
				check the bug: to do
				<xsl:text> - </xsl:text>
				<xsl:value-of select="//dataDocument/party[@id=//header/sendBy/@href]/partyName"/>
				-->
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="//header/sendBy/routingIdsAndExplicitDetails/routingName"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- footer2 variable -->
  <xsl:variable name="footer2">
    <xsl:choose>
      <xsl:when test="//header/sendBy/routingIdsAndExplicitDetails/routingAddress/streetAddress/streetLine[1] = //header/sendBy/routingIdsAndExplicitDetails/routingName">
        <xsl:call-template name="getFooterOfficeAddress">
          <xsl:with-param name="pStreetAddress" select="//header/sendBy/routingIdsAndExplicitDetails/routingAddress/streetAddress"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="getContactOfficeAddress">
          <xsl:with-param name="pContactOffice" select="//header/sendBy"/>
          <xsl:with-param name="pIsCrLf" select="false()"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- footer3 variable -->
  <xsl:variable name="footer3">
    <xsl:if test="//header/sendBy/routingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/telephoneNumber']">
      <xsl:text>Phone </xsl:text>
      <xsl:value-of select="//header/sendBy/routingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/telephoneNumber']"/>
    </xsl:if>
    <xsl:if test="//header/sendBy/routingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/faxNumber']">
      <xsl:text> Fax </xsl:text>
      <xsl:value-of select="//header/sendBy/routingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/faxNumber']"/>
    </xsl:if>
    <xsl:if test="//header/sendBy/routingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/telex']">
      <xsl:text> Telex </xsl:text>
      <xsl:value-of select="//header/sendBy/routingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/telex']"/>
    </xsl:if>
    <xsl:if test="//header/sendBy/routingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/email']">
      <!-- Email -->
      <xsl:text> - </xsl:text>
      <xsl:value-of select="//header/sendBy/routingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/email']"/>
    </xsl:if>
    <xsl:if test="//header/sendBy/routingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/web']">
      <!-- Web -->
      <xsl:text> - </xsl:text>
      <xsl:value-of select="//header/sendBy/routingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/web']"/>
    </xsl:if>
  </xsl:variable>

  <!-- address1 -->
  <xsl:variable name="address1">
    <xsl:value-of select="//header/sendTo[1]/routingIdsAndExplicitDetails/routingAddress/streetAddress/streetLine[1]"/>
  </xsl:variable>

  <!-- address2 -->
  <xsl:variable name="address2">
    <xsl:value-of select="//header/sendTo[1]/routingIdsAndExplicitDetails/routingAddress/streetAddress/streetLine[2]"/>
  </xsl:variable>

  <!-- address3 -->
  <xsl:variable name="address3">
    <xsl:value-of select="//header/sendTo[1]/routingIdsAndExplicitDetails/routingAddress/streetAddress/streetLine[3]"/>
  </xsl:variable>

  <!-- address4 -->
  <xsl:variable name="address4">
    <xsl:value-of select="//header/sendTo[1]/routingIdsAndExplicitDetails/routingAddress/streetAddress/streetLine[4]"/>
  </xsl:variable>

  <!-- address5 -->
  <xsl:variable name="address5">
    <xsl:value-of select="//header/sendTo[1]/routingIdsAndExplicitDetails/routingAddress/streetAddress/streetLine[5]"/>
  </xsl:variable>

  <!-- address6 -->
  <xsl:variable name="address6">
    <xsl:value-of select="//header/sendTo[1]/routingIdsAndExplicitDetails/routingAddress/streetAddress/streetLine[6]"/>
  </xsl:variable>

  <!-- Get the city name of sender-->
  <xsl:template name="getSenderCity">
    <xsl:value-of select="//header/sendBy/routingIdsAndExplicitDetails/routingAddress/city"/>
  </xsl:template>

  <!-- set the date of the signature of the master agreement -->
  <xsl:variable name="masterAgreementDtSignature">
    <xsl:value-of select="//dataDocument/trade/documentation/masterAgreement/masterAgreementDate"/>
  </xsl:variable>

  <!-- Get the calculation agent -->
  <xsl:template name="getCalculationAgent">
    <xsl:param name="pTrade" />

    <xsl:call-template name="debugXsl">
      <xsl:with-param name="pCurrentNode" select="$pTrade/calculationAgent/calculationAgentPartyReference"/>
    </xsl:call-template>

    <xsl:if test="$pTrade/calculationAgent/calculationAgentPartyReference=true()">
      <xsl:call-template name="getPartyNameAndID" >
        <xsl:with-param name="pParty" select="//dataDocument/trade/calculationAgent/calculationAgentPartyReference/@href" />
      </xsl:call-template>
    </xsl:if>
    <xsl:if test="$pTrade/calculationAgent/calculationAgentPartyReference=false()">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'NotAvailable'"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <!-- Get the contact office address -->
  <xsl:template name="getContactOfficeAddress">
    <xsl:param name="pContactOffice"/>
    <xsl:param name="pIsCrLf"/>
    <xsl:for-each select="$pContactOffice/routingIdsAndExplicitDetails/routingAddress/streetAddress/streetLine">
      <xsl:variable name="curr" select="current()"/>
      <xsl:if test="position() != 1">
        <xsl:if test="$pIsCrLf = true()">
        </xsl:if>
        <xsl:if test="$pIsCrLf = false()">
        </xsl:if>
        <xsl:text> - </xsl:text>
      </xsl:if>
      <xsl:value-of select="$curr"/>
    </xsl:for-each>
  </xsl:template>

  <!--
	======================================
	Get the footer office address
	in use only in confirmation footer 
	======================================
	-->
  <xsl:template name="getFooterOfficeAddress">
    <xsl:param name="pStreetAddress"/>
    <xsl:value-of select="$pStreetAddress/streetLine[2]"/>
    <xsl:text> - </xsl:text>
    <xsl:value-of select="$pStreetAddress/streetLine[3]"/>
    <xsl:text> - </xsl:text>
    <xsl:value-of select="$pStreetAddress/streetLine[4]"/>
  </xsl:template>

  <!-- =================================================== -->
  <!-- END region about parties and related offices   -->
  <!-- =================================================== -->

  <!-- =================================================== -->
  <!-- BEGIN   region about trades                      -->
  <!-- =================================================== -->
  <!-- Variables for display the T/Ref text if the value exist -->
  <xsl:variable name="TradeIdTo">
    <xsl:call-template name="getTradeIdTo" />
  </xsl:variable>
  <!-- Variables that display the "T/Ref" text if the value exist-->
  <xsl:variable name="TradeIdFrom">
    <xsl:call-template name="getTradeIdFrom" />
  </xsl:variable>

  <!-- Get the trade date -->
  <xsl:template name="getTradeDate">
    <xsl:param name="pSeparator"/>
    <xsl:param name="pDelimiter"/>
    <xsl:call-template name="format-date">
      <xsl:with-param name="xsd-date-time" select="//dataDocument/trade/tradeHeader/tradeDate"/>
    </xsl:call-template>
  </xsl:template>

  <!-- Get the effective date -->
  <xsl:template name="getEffectiveDate">
    <xsl:param name="pStreams"/>
    <xsl:call-template name="format-date">
      <xsl:with-param name="xsd-date-time" select="$pStreams[1]/calculationPeriodDates/effectiveDate/unadjustedDate"/>
    </xsl:call-template>
    <xsl:call-template name="getBusinessDayConvention" >
      <xsl:with-param name="pBusinessDayConvention" select="$pStreams[1]/calculationPeriodDates/effectiveDate/dateAdjustments/businessDayConvention" />
    </xsl:call-template>
  </xsl:template>

  <!-- Get the first regular period date -->
  <xsl:template name="getFirstRegularPeriodDate">
    <xsl:param name="pStreams"/>
    <xsl:call-template name="format-date">
      <xsl:with-param name="xsd-date-time" select="$pStreams[1]/calculationPeriodDates/firstRegularPeriodStartDate"/>
    </xsl:call-template>
    <xsl:call-template name="getBusinessDayConvention" >
      <xsl:with-param name="pBusinessDayConvention" select="$pStreams[1]/calculationPeriodDates/calculationPeriodDatesAdjustments/businessDayConvention" />
    </xsl:call-template>
  </xsl:template>

  <!--
	<xsl:template name="getFirstRegularPeriodDate">
		<xsl:param name="pStreams"/>
		<xsl:call-template name="format-date">
			<xsl:with-param name="xsd-date-time" select="$pStreams/calculationPeriodDates/firstRegularPeriodStartDate"/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template name="getBDCofFirstRegularPeriodDate">
		<xsl:param name="pStreams"/>
		<xsl:call-template name="getBusinessDayConvention" >
			<xsl:with-param name="pBusinessDayConvention" select="$pStreams/calculationPeriodDates/calculationPeriodDatesAdjustments/businessDayConvention" />
		</xsl:call-template>
	</xsl:template>
	-->

  <!-- Get Last Regular Period Date -->
  <xsl:template name="getLastRegularPeriodDate">
    <xsl:param name="pStreams"/>
    <xsl:call-template name="format-date">
      <xsl:with-param name="xsd-date-time" select="$pStreams[1]/calculationPeriodDates/lastRegularPeriodEndDate"/>
    </xsl:call-template>
    <xsl:call-template name="getBusinessDayConvention" >
      <xsl:with-param name="pBusinessDayConvention" select="$pStreams[1]/calculationPeriodDates/calculationPeriodDatesAdjustments/businessDayConvention" />
    </xsl:call-template>
  </xsl:template>

  <!-- 
	================================================================================================================
	Get Termination Date
	if the first swapstream not contains business day convention it use the second swapstream 
	if the first and the second stream not contains business day convention it display none business day convention
	=================================================================================================================
	-->
  <xsl:template name="getTerminationDate">
    <xsl:param name="pStreams"/>
    <xsl:choose>
      <xsl:when test ="$pStreams[1]/calculationPeriodDates/terminationDate/dateAdjustments/businessDayConvention = 'NONE' and normalize-space( $pStreams[2]/calculationPeriodDates/terminationDate/unadjustedDate)">
        <xsl:call-template name="format-date">
          <xsl:with-param name="xsd-date-time" select="$pStreams[2]/calculationPeriodDates/terminationDate/unadjustedDate"/>
        </xsl:call-template>
        <xsl:call-template name="getBusinessDayConvention" >
          <xsl:with-param name="pBusinessDayConvention" select="$pStreams[2]/calculationPeriodDates/terminationDate/dateAdjustments/businessDayConvention" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="format-date">
          <xsl:with-param name="xsd-date-time" select="$pStreams[1]/calculationPeriodDates/terminationDate/unadjustedDate"/>
        </xsl:call-template>
        <xsl:call-template name="getBusinessDayConvention" >
          <xsl:with-param name="pBusinessDayConvention" select="$pStreams[1]/calculationPeriodDates/terminationDate/dateAdjustments/businessDayConvention" />
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Get the trade ID with respect to the counter-party of the trade who receives the confirmation (TO which the confirmation must be sent)-->
  <xsl:template name="getTradeIdTo">
    <xsl:variable name="partyIDTo">
      <xsl:call-template name="getPartyIDTo"/>
    </xsl:variable>
    <xsl:value-of select="//dataDocument/trade/tradeHeader/partyTradeIdentifier/partyReference[@href=$partyIDTo]/../tradeId[@tradeIdScheme='http://www.euro-finance-systems.fr/otcml/tradeid']"/>
  </xsl:template>

  <!-- Get the trade ID with respect to the counter-party of the trade who sends the confirmation (FROM which the confirmation will be sent)-->
  <xsl:template name="getTradeIdFrom">
    <xsl:variable name="partyIDFrom">
      <xsl:call-template name="getPartyIDFrom"/>
    </xsl:variable>
    <xsl:value-of select="//dataDocument/trade/tradeHeader/partyTradeIdentifier/partyReference[@href=$partyIDFrom]/../tradeId[@tradeIdScheme='http://www.euro-finance-systems.fr/otcml/tradeid']"/>
  </xsl:template>

  <!-- =================================================== -->
  <!-- END region about trades                             -->
  <!-- =================================================== -->
  <!-- 
	==================================================================================
	Return the Initial or Final Exchange Date of the notional for a given stream 
	===================================================================================
	-->
  <xsl:template name="getExchangeDate">
    <xsl:param name="pExchange"/>
    <xsl:param name="pStream"/>
    <xsl:param name="pSeparator"/>
    <xsl:param name="pDelimiter"/>
    <xsl:if test="$pExchange='Initial'">
      <xsl:call-template name="format-date">
        <xsl:with-param name="xsd-date-time" select="$pStream/calculationPeriodDates/effectiveDate/unadjustedDate"/>
      </xsl:call-template>
      <xsl:call-template name="getBusinessDayConvention" >
        <xsl:with-param name="pBusinessDayConvention" select="$pStream/calculationPeriodDates/effectiveDate/dateAdjustments/businessDayConvention" />
      </xsl:call-template>
    </xsl:if>
    <xsl:if test="$pExchange='Final'">
      <xsl:call-template name="format-date">
        <xsl:with-param name="xsd-date-time" select="$pStream/calculationPeriodDates/terminationDate/unadjustedDate"/>
      </xsl:call-template>
      <xsl:call-template name="getBusinessDayConvention" >
        <xsl:with-param name="pBusinessDayConvention" select="$pStream/calculationPeriodDates/terminationDate/dateAdjustments/businessDayConvention" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <!-- Get Exchange Amount -->
  <xsl:template name="getExchangeAmount">
    <xsl:param name="pSeparator"/>
    <xsl:param name="pDelimiter"/>
    <xsl:param name="pExchange"/>
    <xsl:variable name="pPos" select="position()" />
    <xsl:variable name="curr" select="current()" />
    <xsl:variable name="Stream" select="//dataDocument/trade/swap/swapStream[$pPos]" />

    <xsl:if test="$pExchange='Initial'">
      <xsl:if test="$Stream/calculationPeriodAmount/knownAmountSchedule">
        <xsl:call-template name="format-money2">
          <xsl:with-param name="currency" select="$Stream/calculationPeriodAmount/knownAmountSchedule/currency"/>
          <xsl:with-param name="amount" select="$Stream/calculationPeriodAmount/knownAmountSchedule/initialValue"/>
        </xsl:call-template>
      </xsl:if>

      <xsl:if test="$Stream/calculationPeriodAmount/calculation">
        <xsl:call-template name="format-money2">
          <xsl:with-param name="currency" select="$Stream/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/currency"/>
          <xsl:with-param name="amount" select="$Stream/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/initialValue"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:if>

    <xsl:if test="$pExchange='Final'">
      <xsl:if test="$Stream/calculationPeriodAmount/knownAmountSchedule">
        <xsl:if test="$Stream/calculationPeriodAmount/knownAmountSchedule/step">
          <xsl:call-template name="format-money2">
            <xsl:with-param name="currency" select="$Stream/calculationPeriodAmount/knownAmountSchedule/currency"/>
            <xsl:with-param name="amount" select="$Stream/calculationPeriodAmount/knownAmountSchedule/step[count($Stream/calculationPeriodAmount/knownAmountSchedule/step)]/stepValue"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:if test="not( $Stream/calculationPeriodAmount/knownAmountSchedule )">
          <xsl:call-template name="format-money2">
            <xsl:with-param name="currency" select="$Stream/calculationPeriodAmount/knownAmountSchedule/currency"/>
            <xsl:with-param name="amount" select="$Stream/calculationPeriodAmount/knownAmountSchedule/initialValue"/>
          </xsl:call-template>
        </xsl:if>
      </xsl:if>

      <xsl:if test="$Stream/calculationPeriodAmount/calculation">
        <xsl:call-template name="format-money2">
          <xsl:with-param name="currency" select="$Stream/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/currency"/>
          <xsl:with-param name="amount" select="$Stream/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/initialValue"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <!-- =================================================== -->
  <!-- BEGIN region about dates                            -->
  <!-- =================================================== -->
  <!-- Get either the adjustable or the relative date -->
  <xsl:template name="getAdjustableOrRelativeDates">
    <xsl:param name="pDate" />

    <xsl:choose>
      <xsl:when test="$pDate/adjustableDates/node()=true()">
        <xsl:call-template name="getAdjustableDates">
          <xsl:with-param name="pAdjustableDates" select="$pDate/adjustableDates"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pDate/relativeDate/node()=true()">
        <xsl:call-template name="getRelativeDate">
          <xsl:with-param name="pRelativeDate" select="$pDate/relativeDates"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'Error'"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="getAdjustableOrRelativeDate">
    <xsl:param name="pDate" />

    <xsl:choose>
      <xsl:when test="$pDate/adjustableDate/node()=true()">
        <xsl:call-template name="getAdjustableDate">
          <xsl:with-param name="pAdjustableDate" select="$pDate/adjustableDate"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pDate/relativeDate/node()=true()">
        <xsl:call-template name="getRelativeDate">
          <xsl:with-param name="pRelativeDate" select="$pDate/relativeDate"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'Error'"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- AdjustableOrRelativeDateSequence   -->
  <xsl:template name="getAdjustableOrRelativeDateSequence">
    <xsl:param name="pDate" />
    <xsl:choose>
      <xsl:when test="$pDate/adjustableDate/node()=true()">
        <xsl:call-template name="getAdjustableDate">
          <xsl:with-param name="pAdjustableDate" select="$pDate/adjustableDate"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pDate/relativeDateSequence/node()=true()">
        <xsl:call-template name="getRelativeDateSequence">
          <xsl:with-param name="pRelativeDateSequence" select="$pDate/relativeDateSequence"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'Error'"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- get AdjustableDate Template  -->
  <xsl:template name="getAdjustableDate">
    <xsl:param name="pAdjustableDate" />

    <xsl:call-template name="format-date">
      <xsl:with-param name="xsd-date-time" select="$pAdjustableDate/unadjustedDate"/>
    </xsl:call-template>
    <xsl:call-template name="getBusinessDayConvention" >
      <xsl:with-param name="pBusinessDayConvention" select="$pAdjustableDate/dateAdjustments/businessDayConvention" />
    </xsl:call-template>
  </xsl:template>

  <!-- get Adjustable Dates Template   -->
  <xsl:template name="getAdjustableDates">
    <xsl:param name="pAdjustableDates" />
    <xsl:param name="pSeparator" select="' - '"/>

    <xsl:for-each select="$pAdjustableDates/unadjustedDate">
      <xsl:call-template name="format-date">
        <xsl:with-param name="xsd-date-time" select="."/>
      </xsl:call-template>
      <xsl:copy-of select="$pSeparator"/>
      <xsl:variable name="totDate" select="count($pAdjustableDates/unadjustedDate)"/>
      <xsl:if test="(0&lt;$totDate) and (position() != $totDate)"></xsl:if>
    </xsl:for-each>
    <xsl:call-template name="getBusinessDayConvention" >
      <xsl:with-param name="pBusinessDayConvention" select="$pAdjustableDates/dateAdjustments/businessDayConvention" />
    </xsl:call-template>
  </xsl:template>

  <!-- AdjustableDate2  -->
  <xsl:template name="getAdjustableDate2">
    <xsl:param name="pAdjustableDate2" />

    <xsl:call-template name="debugXsl">
      <xsl:with-param name="pCurrentNode" select="$pAdjustableDate2/unadjustedDate"/>
    </xsl:call-template>

    <xsl:call-template name="format-date">
      <xsl:with-param name="xsd-date-time" select="$pAdjustableDate2/unadjustedDate"/>
    </xsl:call-template>
    <xsl:choose>
      <xsl:when test="$pAdjustableDate2/dateAdjustments/node()=true()">
        <xsl:call-template name="getBusinessDayConvention" >
          <xsl:with-param name="pBusinessDayConvention" select="$pAdjustableDate2/dateAdjustments/businessDayConvention" />
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pAdjustableDate2/dateAdjustmentsReference/@href">
        <xsl:variable name="eltReference" select="//*[@id=$pAdjustableDate2/dateAdjustmentsReference/@href]"/>
        <xsl:call-template name="getBusinessDayConvention" >
          <xsl:with-param name="pBusinessDayConvention" select="$eltReference/businessDayConvention" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'Error'"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 
	========================================
	Get the first payment date  
	========================================
	-->
  <xsl:template name="getFirstPaymentDate">
    <xsl:param name="pStreamPosition"/>
    <xsl:call-template name="format-date">
      <xsl:with-param name="xsd-date-time" select="//Event[STREAMNO=$pStreamPosition and EVENTCODE='INT' and EVENTTYPE='INT']/EventClass[EVENTCLASS = 'STL']/DTEVENT[1]"/>
    </xsl:call-template>
  </xsl:template>

  <!-- 
	==========================================================
	== Get first Period End date 
	== the fist period end date is the PER/FLO or PER/FIX event  
	===========================================================
	-->
  <xsl:template name="getFirstPeriodEndDate">
    <xsl:param name="pStreamPosition"/>

    <xsl:choose>
      <!-- floating stream (PER/FLO event)-->
      <xsl:when test ="//Event[STREAMNO=$pStreamPosition and EVENTCODE='PER' and EVENTTYPE='FLO']">
        <xsl:call-template name="format-date">
          <xsl:with-param name="xsd-date-time" select="//Event[STREAMNO=$pStreamPosition and EVENTCODE='PER' and EVENTTYPE='FLO']/DTENDADJ[1]"/>
        </xsl:call-template>
      </xsl:when>
      <!--GS 20111129 handle Last period end date for fixed stream -->
      <!-- Fixed stream (PER/FIX event)-->
      <xsl:when test ="//Event[STREAMNO=$pStreamPosition and EVENTCODE='PER' and EVENTTYPE='FIX']">
        <xsl:call-template name="format-date">
          <xsl:with-param name="xsd-date-time" select="//Event[STREAMNO=$pStreamPosition and EVENTCODE='PER' and EVENTTYPE='FIX']/DTENDADJ[1]"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>[Error]</xsl:text>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!-- 
	=====================================
	Get last payment date
  used by Fixed and Floating streams
	=====================================
	-->
  <xsl:template name="getLastPaymentDate">
    <xsl:param name="pStreamPosition"/>
    <xsl:call-template name="format-date">
      <xsl:with-param name="xsd-date-time" select="//Event[STREAMNO=$pStreamPosition and EVENTCODE='TER' and EVENTTYPE='INT']/EventClass[EVENTCLASS = 'STL']/DTEVENT"/>
    </xsl:call-template>
  </xsl:template>

  <!-- 
	===============================
	== Get last Period End date  
	== 
	===============================
	-->
  <xsl:template name="getLastPeriodEndDate">
    <xsl:param name="pStreamPosition"/>
    <xsl:choose>
      <!-- floating stream (PER/FLO event)-->
      <xsl:when test ="//Event[STREAMNO=$pStreamPosition and EVENTCODE='TER' and EVENTTYPE='INT']/Event[EVENTCODE='PER' and EVENTTYPE='FLO']">
        <xsl:call-template name="format-date">
          <xsl:with-param name="xsd-date-time" select="//Event[STREAMNO=$pStreamPosition and EVENTCODE='TER' and EVENTTYPE='INT']/Event[EVENTCODE='PER' and EVENTTYPE='FLO']/DTENDADJ"/>
        </xsl:call-template>
      </xsl:when>
      <!--GS 20111129 handle Last period end date for fixed stream -->
      <!-- Fixed stream (PER/FIX event)-->
      <xsl:when test ="//Event[STREAMNO=$pStreamPosition and EVENTCODE='TER' and EVENTTYPE='INT']/Event[EVENTCODE='PER' and EVENTTYPE='FIX']">
        <xsl:call-template name="format-date">
          <xsl:with-param name="xsd-date-time" select="//Event[STREAMNO=$pStreamPosition and EVENTCODE='TER' and EVENTTYPE='INT']/Event[EVENTCODE='PER' and EVENTTYPE='FIX']/DTENDADJ"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>[Error]</xsl:text>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!--
	====================================
	RelativeDate 
	====================================
	-->
  <xsl:template name="getRelativeDate">
    <xsl:param name="pRelativeDate" />

    <xsl:call-template name="getOffset">
      <xsl:with-param name="pOffset" select="$pRelativeDate" />
    </xsl:call-template>

    <xsl:call-template name="getBusinessDayConvention" >
      <xsl:with-param name="pBusinessDayConvention" select="$pRelativeDate/businessDayConvention" />
    </xsl:call-template>

    <xsl:call-template name="getTranslation">
      <xsl:with-param name="pResourceName" select="'determinedWithRespectTo'"/>
    </xsl:call-template>

    <xsl:call-template name ="getDateRelativeToDisplayName">
      <xsl:with-param name="pDateRelativeTo" select="$pRelativeDate/dateRelativeTo/@href"/>
    </xsl:call-template>
    <!--
		<xsl:value-of select="$pRelativeDate/dateRelativeTo/@href" />
		-->

  </xsl:template>

  <!-- RelativeDateSequence -->
  <xsl:template name="getRelativeDateSequence">
    <xsl:param name="pRelativeDateSequence" />
    <xsl:param name="pSeparator"/>

    <xsl:call-template name="getOffset">
      <xsl:with-param name="pOffset" select="$pRelativeDateSequence/dateOffset" />
    </xsl:call-template>

    <xsl:call-template name="getBusinessDayConvention" >
      <xsl:with-param name="pBusinessDayConvention" select="$pRelativeDateSequence/dateOffset/businessDayConvention" />
    </xsl:call-template>

    <xsl:call-template name="getTranslation">
      <xsl:with-param name="pResourceName" select="'determinedWithRespectTo'"/>
    </xsl:call-template>

    <xsl:value-of select="$pRelativeDateSequence/dateRelativeTo/@href" />

    <xsl:copy-of select="$pSeparator"/>

  </xsl:template>


  <!-- 
	==========================
	Get roll convention 
	==========================
	-->
  <xsl:template name="getRollConvention">
    <xsl:param name="pRollConvention" />
    <xsl:choose>
      <xsl:when test="$pRollConvention = 'EOM'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'EndOfMonth'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pRollConvention = '1'">
        <xsl:text> 1st </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '2'">
        <xsl:text> 2nd </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '3'">
        <xsl:text> 3rd </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '4'">
        <xsl:text> 4th </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '5'">
        <xsl:text> 5th </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '6'">
        <xsl:text> 6th </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '7'">
        <xsl:text> 7th </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '8'">
        <xsl:text> 8th </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '9'">
        <xsl:text> 9th </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '10'">
        <xsl:text> 10th </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '11'">
        <xsl:text> 11th </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '12'">
        <xsl:text> 12th </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '13'">
        <xsl:text> 13th </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '14'">
        <xsl:text> 14th </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '15'">
        <xsl:text> 15th </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '16'">
        <xsl:text> 16th </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '17'">
        <xsl:text> 17th </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '18'">
        <xsl:text> 18th </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '19'">
        <xsl:text> 19th </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '20'">
        <xsl:text> 20th </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '21'">
        <xsl:text> 21th </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '22'">
        <xsl:text> 22th </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '23'">
        <xsl:text> 23th </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '24'">
        <xsl:text> 24th </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '25'">
        <xsl:text> 25th </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '26'">
        <xsl:text> 26th </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '27'">
        <xsl:text> 27th </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '28'">
        <xsl:text> 28th </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '29'">
        <xsl:text> 29th </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '30'">
        <xsl:text> 30th </xsl:text>
      </xsl:when>
      <xsl:when test="$pRollConvention = '31'">
        <xsl:text> 31th </xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pRollConvention"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!-- It returns the payment lag available into the paymentDaysOffset node (eg. 1 Business Day)-->
  <!-- Case not handled: offset defined for each stream -->
  <!-- This template returns the offset informations available into the first paymentDaysOffset node-->
  <xsl:template name="getPaymentLag">

    <xsl:variable name="vCurrentNode" select="//paymentDaysOffset[1]" />

    <xsl:variable name ="vOffSetPeriodMultiplier">
      <xsl:value-of select ="$vCurrentNode/periodMultiplier"/>
    </xsl:variable>

    <xsl:variable name ="vOffSetDayType">
      <xsl:value-of select ="$vCurrentNode/dayType"/>
    </xsl:variable>

    <xsl:variable name ="vOffSetPeriod">

      <xsl:choose>
        <xsl:when test="$vCurrentNode/period = 'D' and $vCurrentNode/periodMultiplier &gt; 1">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'Days'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'Day'"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>

    </xsl:variable>

    <xsl:value-of select ="concat($vOffSetPeriodMultiplier,' ',$vOffSetDayType,' ',$vOffSetPeriod)"/>

  </xsl:template>



  <!-- 
	==========================================
	this template returns Frequency on payment 
  dates without payments dates and roll convention
  not in use 
	==========================================
	-->
  <xsl:template name="getPaymentDatesFrequencyWithOffset">
    <xsl:param name="pStreamPosition" select="position()"/>
    <xsl:param name="pFrequency" />

    <!-- display frequency period (eg. D=Daily; W=Weekly;)-->
    <xsl:if test="$pFrequency/period != 'T'">
      <xsl:if test="$pFrequency/periodMultiplier = 1">
        <xsl:choose>
          <xsl:when test="$pFrequency/period = 'D'">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'Daily'"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pFrequency/period = 'W'">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'Weekly'"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pFrequency/period = 'M'">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'Monthly'"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pFrequency/period = 'Y'">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'Yearly'"/>
            </xsl:call-template>
          </xsl:when>
          <!-- If we are here there is an error -->
          <xsl:otherwise>
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'Error'"/>
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>

    </xsl:if>

    <xsl:if test="$pFrequency/period = 'T'">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'OneTerm'"/>
      </xsl:call-template>
    </xsl:if>

  </xsl:template>

  <!-- 
	==========================================
	Get frequency of the payment dates 
	==========================================
	-->
  <xsl:template name="getPaymentDatesFrequency">
    <xsl:param name="pStreamPosition" />
    <xsl:param name="pFrequency" />
    <xsl:param name="pRollConvention" />

    <xsl:if test="$pFrequency/period != 'T'">
      <xsl:if test="$pFrequency/periodMultiplier = 1">
        <xsl:choose>
          <xsl:when test="$pFrequency/period = 'D'">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'Daily'"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pFrequency/period = 'W'">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'Weekly'"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pFrequency/period = 'M'">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'Monthly'"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pFrequency/period = 'Y'">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'Yearly'"/>
            </xsl:call-template>
          </xsl:when>
          <!-- If we are here there is an error -->
          <xsl:otherwise>
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'Error'"/>
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>
      <xsl:if test="$pFrequency/periodMultiplier != 1">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'Every'"/>
        </xsl:call-template>
        <xsl:call-template name="getFrequency" >
          <xsl:with-param name="pFrequency" select="$pFrequency" />
        </xsl:call-template>
      </xsl:if>
      <!--  when frequency = "1 day" the roll convention is missing-->
      <xsl:choose>
        <xsl:when test="$pFrequency/period = 'D' and $pFrequency/periodMultiplier = 1">

        </xsl:when>
        <xsl:otherwise>
          <xsl:choose>
            <xsl:when test="$pFrequency/period = 'W'">
              <xsl:call-template name="getTranslation">
                <xsl:with-param name="pResourceName" select="'on'"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="getTranslation">
                <xsl:with-param name="pResourceName" select="'onThe'"/>
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:call-template name="getRollConvention" >
            <xsl:with-param name="pRollConvention" select="$pRollConvention"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
      <!-- add the start payment date-->
      <!-- DR20090701 only if it exists -->
      <xsl:if test="//Event[STREAMNO=$pStreamPosition and EVENTCODE='INT' and EVENTTYPE='INT']/EventClass[EVENTCLASS = 'STL']/DTEVENT[1] = true()">
        <xsl:call-template name="getTranslation">
          <!-- EG 20160404 Migration vs2013 -->
          <xsl:with-param name="pResourceName" select="'commencingOn_'"/>
        </xsl:call-template>
        <xsl:call-template name="getFirstPaymentDate">
          <xsl:with-param name="pStreamPosition" select="$pStreamPosition"/>
        </xsl:call-template>
      </xsl:if>
      <!-- the definition 'through and including the termination date' has been replaced by the definition 'up to and including the termination date'-->
      <!-- from ISDA 2006 definitions-->
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'UpToAndIncludingTheTerminationDate'"/>
      </xsl:call-template>
    </xsl:if>
    <xsl:if test="$pFrequency/period = 'T'">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'OneTermOn'"/>
      </xsl:call-template>
      <xsl:call-template name="getLastPaymentDate" >
        <xsl:with-param name="pStreamPosition" select="$pStreamPosition"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <!-- 
	==============================================
	Get frequency of the Calculation Period 
	==============================================
	-->
  <xsl:template name="getCalculationPeriodDatesFrequency">
    <xsl:param name="pStreamPosition" />
    <xsl:param name="pFrequency" />
    <xsl:param name="pRollConvention" />


    <xsl:if test="$pFrequency/period != 'T'">
      <xsl:if test="$pFrequency/periodMultiplier = 1">
        <xsl:choose>
          <xsl:when test="$pFrequency/period = 'D'">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'Daily'"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pFrequency/period = 'W'">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'Weekly'"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pFrequency/period = 'M'">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'Monthly'"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pFrequency/period = 'Y'">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'Yearly'"/>
            </xsl:call-template>
          </xsl:when>
          <!-- If we are here there is an error -->
          <xsl:otherwise>
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'Error'"/>
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>
      <xsl:if test="$pFrequency/periodMultiplier != 1">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'Every'"/>
        </xsl:call-template>
        <xsl:call-template name="getFrequency" >
          <xsl:with-param name="pFrequency" select="$pFrequency" />
        </xsl:call-template>
      </xsl:if>
      <!--  when frequency = "1 day" the roll convention is missing-->
      <xsl:choose>
        <xsl:when test="$pFrequency/period = 'D' and $pFrequency/periodMultiplier = 1">

        </xsl:when>
        <xsl:otherwise>
          <xsl:choose>
            <xsl:when test="$pFrequency/period = 'W'">
              <xsl:call-template name="getTranslation">
                <xsl:with-param name="pResourceName" select="'on'"/>
              </xsl:call-template>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="getTranslation">
                <xsl:with-param name="pResourceName" select="'onThe'"/>
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:call-template name="getRollConvention" >
            <xsl:with-param name="pRollConvention" select="$pRollConvention"/>
          </xsl:call-template>
        </xsl:otherwise>

      </xsl:choose>
      <!-- add the start period end date-->
      <!-- EG 20160405 Migration vs2013 -->
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'commencingOn_'"/>
      </xsl:call-template>
      <xsl:call-template name="getFirstPeriodEndDate" >
        <xsl:with-param name="pStreamPosition" select="$pStreamPosition"/>
      </xsl:call-template>
      <!-- the definition 'through and including the termination date' has been replaced by the definition 'up to and including the termination date'-->
      <!-- from ISDA 2006 definitions-->
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'UpToAndIncludingTheTerminationDate'"/>
      </xsl:call-template>
    </xsl:if>
    <xsl:if test="$pFrequency/period = 'T'">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'OneTermOn'"/>
      </xsl:call-template>
      <xsl:call-template name="getLastPeriodEndDate" >
        <xsl:with-param name="pStreamPosition" select="$pStreamPosition"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <!-- 
	==============================================
	Get the floating rate index displayname
	==============================================
	-->
  <!-- 
	(Available from 2.3 OTCmL version )
	Floating rate index is available from node[//dataDocument/repository/rateIndex/displayname]
	when the node is missing (eg.OTCML version 2.2) the Floating rate index is available from node [//floatingRateCalculation/floatingRateIndex]
	-->
  <!-- FI 20161114 [RATP] GLOP il faut revoir pour l'ensemble des xsl la lecture dans repository-->
  <xsl:template name="getFloatingRateIndex">
    <xsl:param name="pCalculation"/>
    <xsl:choose>
      <xsl:when test ="//dataDocument/repository/rateIndex=true()">
        <xsl:variable name ="OTCmlId" select ="$pCalculation/floatingRateCalculation/floatingRateIndex/@OTCmlId"/>
        <xsl:value-of select="//dataDocument/repository/rateIndex[@OTCmlId=$OTCmlId]/displayname"/>
        <!--<xsl:value-of select="'LA MISERE'"/>-->
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pCalculation/floatingRateCalculation/floatingRateIndex"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 
	=============================================
	Get FRA floating rate index displayname
	=============================================
	-->
  <xsl:template name="getFRAFloatingRateIndex">
    <xsl:param name="pCalculation"/>
    <xsl:value-of select="//dataDocument/repository/rateIndex[$pCalculation/floatingRateIndex/@href = @id]/displayname"/>
  </xsl:template>

  <!-- 
	=================================
	Get floating rate source 
	=================================
	-->
  <xsl:template name="getFloatingRateSource">
    <xsl:param name="pCalculation"/>
    <xsl:call-template name="debugXsl">
      <xsl:with-param name="pCurrentNode" select="$pCalculation/floatingRateCalculation/floatingRateIndex"/>
    </xsl:call-template>
    <xsl:value-of select="//dataDocument/repository/rateIndex[$pCalculation/floatingRateCalculation/floatingRateIndex/@href = @id]/informationSource/rateSource"/>
    <xsl:text> - </xsl:text>
    <xsl:value-of select="//dataDocument/repository/rateIndex[$pCalculation/floatingRateCalculation/floatingRateIndex/@href = @id]/informationSource/rateSourcePage"/>
  </xsl:template>

  <!-- 
	===================================
	Get FRA floating rate source 
	===================================
	-->
  <xsl:template name="getFRAFloatingRateSource">
    <xsl:param name="pCalculation"/>
    <xsl:call-template name="debugXsl">
      <xsl:with-param name="pCurrentNode" select="$pCalculation/floatingRateIndex"/>
    </xsl:call-template>
    <xsl:value-of select="//dataDocument/repository/rateIndex[$pCalculation/floatingRateIndex/@href = @id]/informationSource/rateSource"/>
    <xsl:text> - </xsl:text>
    <xsl:value-of select="//dataDocument/repository/rateIndex[$pCalculation/floatingRateIndex/@href = @id]/informationSource/rateSourcePage"/>
  </xsl:template>

  <!-- =================================================== -->
  <!-- END region about dates                              -->
  <!-- =================================================== -->

  <!-- 
	==========================================
	BusinessDayConvention 
	==========================================
	-->
  <xsl:template name="getBusinessDayConvention">
    <xsl:param name="pBusinessDayConvention" />

    <xsl:call-template name="debugXsl">
      <xsl:with-param name="pCurrentNode" select="$pBusinessDayConvention"/>
    </xsl:call-template>

    <xsl:if test="$pBusinessDayConvention != 'NONE'">
      <xsl:choose>
        <xsl:when test="$pBusinessDayConvention = 'MODFOLLOWING'">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'BusinessDayConventionMODF'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pBusinessDayConvention = 'FOLLOWING'">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'BusinessDayConventionFOL'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pBusinessDayConvention = 'MODPRECEDING'">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'BusinessDayConventionMODP'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pBusinessDayConvention = 'PRECEDING'">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'BusinessDayConventionPREC'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pBusinessDayConvention = 'FRN'">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'BusinessDayConventionFRN'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pBusinessDayConvention = 'NotApplicable'"></xsl:when>
      </xsl:choose>
    </xsl:if>
  </xsl:template>

  <!-- 
	=====================================================================================
	Return the correct display text from the abbreviation of the period and frequency 
	=====================================================================================
	-->
  <xsl:template name="getFrequency">
    <xsl:param name="pFrequency" />
    <xsl:param name="pIsBold" />

    <xsl:variable name="ret">
      <xsl:value-of select="$pFrequency/periodMultiplier" />
      <xsl:choose>
        <xsl:when test="$pFrequency/period = 'D' and $pFrequency/periodMultiplier  = 1">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'_day'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pFrequency/period = 'D' and $pFrequency/periodMultiplier &gt; 1">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'_days'"/>
          </xsl:call-template>
        </xsl:when>
        <!-- eg. = '0' -->
        <xsl:when test="$pFrequency/period = 'D' and $pFrequency/periodMultiplier &lt; 1">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'_days'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pFrequency/period = 'W' and $pFrequency/periodMultiplier  = 1">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'_week'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pFrequency/period = 'W' and $pFrequency/periodMultiplier  &gt; 1">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'_weeks'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pFrequency/period = 'M' and $pFrequency/periodMultiplier = 1">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'_month'"/>
          </xsl:call-template>

        </xsl:when>
        <xsl:when test="$pFrequency/period = 'M' and $pFrequency/periodMultiplier &gt; 1">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'_months'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pFrequency/period = 'Y' and $pFrequency/periodMultiplier = 1">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'_year'"/>
          </xsl:call-template>

        </xsl:when>
        <xsl:when test="$pFrequency/period = 'Y' and $pFrequency/periodMultiplier &gt; 1">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'_years'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pFrequency/period = 'T'">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'_Term'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'NotApplicable'"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:if test="$pIsBold=false()">
      <xsl:value-of select="$ret" />
    </xsl:if>
    <xsl:if test="$pIsBold!=false()">
      <xsl:value-of select="$ret" />
    </xsl:if>
  </xsl:template>

  <!-- 
	==================================================================================================================================
	Return the correct display text from the abbreviation of the period node: in use by Straddle swaption in cash Settlement section
	===================================================================================================================================
	-->
  <xsl:template name="getTextPeriodByPeriodMultiplier">
    <xsl:param name="pPeriodFrequency" />

    <xsl:choose>
      <xsl:when test="$pPeriodFrequency/period = 'D' and $pPeriodFrequency/periodMultiplier  = 1">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'_day'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pPeriodFrequency/period = 'D' and $pPeriodFrequency/periodMultiplier &gt; 1">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'_days'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pPeriodFrequency/period = 'D' and $pPeriodFrequency/periodMultiplier &lt; 0">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'_days'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pPeriodFrequency/period = 'W' and $pPeriodFrequency/periodMultiplier  = 1">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'_week'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pPeriodFrequency/period = 'W' and $pPeriodFrequency/periodMultiplier  &gt; 1">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'_weeks'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pPeriodFrequency/period = 'W' and $pPeriodFrequency/periodMultiplier  &lt; 0">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'_weeks'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pPeriodFrequency/period = 'M' and $pPeriodFrequency/periodMultiplier = 1">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'_month'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pPeriodFrequency/period = 'M' and $pPeriodFrequency/periodMultiplier &gt; 1">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'_months'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pPeriodFrequency/period = 'M' and $pPeriodFrequency/periodMultiplier &lt; 0">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'_months'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pPeriodFrequency/period = 'Y' and $pPeriodFrequency/periodMultiplier = 1">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'_year'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pPeriodFrequency/period = 'Y' and $pPeriodFrequency/periodMultiplier &gt; 1">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'_years'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pPeriodFrequency/period = 'Y' and $pPeriodFrequency/periodMultiplier &lt; 0">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'_years'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pPeriodFrequency/period = 'T'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'_Term'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'NotApplicable'"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>
  <!-- 
	===========================================
	get English Month Name 
	===========================================
	-->
  <xsl:template name="getMonthName">
    <xsl:param name="pMonth" />

    <!-- ***************************************************** -->
    <!-- Call Mode DEBUG                                       -->
    <!-- ***************************************************** -->
    <xsl:call-template name="debugXsl">
      <xsl:with-param name="pCurrentNode" select="$pMonth"/>
    </xsl:call-template>
    <xsl:choose>
      <xsl:when test="$pMonth = 1">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'January'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pMonth = 2">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'February'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pMonth = 3">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'March'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pMonth = 4">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'April'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pMonth = 5">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'May'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pMonth = 6">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'June'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pMonth = 7">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'July'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pMonth = 8">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'August'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pMonth = 9">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'September'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pMonth = 10">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'October'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pMonth = 11">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'November'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pMonth = 12">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'December'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'Error'"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 
	==============================================
	get all business center 
	==============================================
	-->
  <xsl:template name="getBusinessCenterList">
    <xsl:param name="pBusinessCenters" />

    <xsl:if test="$pBusinessCenters/businessCenter/node()=true()">
      <xsl:for-each select="$pBusinessCenters/businessCenter">

        <xsl:if test="position() &gt; 1">
          ,
        </xsl:if>

        <xsl:value-of select="current()"/>
      </xsl:for-each>
    </xsl:if>
  </xsl:template>

  <!-- 
	==================================
	Get Spread 
	==================================
	-->
  <xsl:template name="getSpread">
    <xsl:variable name="m_spread2">
      <xsl:call-template name="format-fixed-rate">
        <xsl:with-param name="fixed-rate" select="stepValue" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:call-template name="ReplacePlusMinus">
      <xsl:with-param name="pString" select="$m_spread2"/>
    </xsl:call-template>
  </xsl:template>

  <!-- 
	===============
	Get Offset  
	===============
	-->
  <xsl:template name="getOffset">
    <xsl:param name="pOffset" />

    <xsl:variable name="m_offset">
      <xsl:call-template name="getFrequency" >
        <xsl:with-param name="pFrequency" select="$pOffset"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:call-template name="ReplacePlusMinus">
      <xsl:with-param name="pString" select="$m_offset"/>
    </xsl:call-template>
    &#160;<xsl:value-of select="$pOffset/dayType"/>
  </xsl:template>

  <!-- =================================================== -->
  <!-- BEGIN region General Roles                          -->
  <!-- =================================================== -->

  <!-- 
	=================================================
	Return a absolute numeric value 
	=================================================
	-->
  <xsl:template name="getAbsoluteNumericValue">
    <xsl:param name="pNumericValue" />
    <xsl:choose>
      <xsl:when test="$pNumericValue &gt;= 0">
        <xsl:value-of select="$pNumericValue" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="0 - $pNumericValue" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 
	===================================
	return Applicable/Not Applicable 
	===================================
	-->
  <xsl:template name="getApplicable">
    <xsl:param name="pValue" />
    <xsl:choose>
      <xsl:when test="$pValue = 'true'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'Applicable'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pValue = 'false'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'NotApplicable'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'Error'"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Format the Money node according to the currency and culture of the user (pCurrentCulture)-->
  <xsl:template name="formatMoney">
    <xsl:param name="pMoney" />
    <xsl:value-of select="$pMoney/currency" />
    <xsl:text>&#160;</xsl:text>
    <xsl:value-of select="format-number($pMoney/amount, $amountPattern, $defaultCulture)" />
  </xsl:template>

  <!-- =================================================== -->
  <!-- END region General Roles                          -->
  <!-- =================================================== -->

  <!-- Get the Title -->
  <xsl:template name="getTitle">
    <xsl:call-template name="getTranslation">
      <xsl:with-param name="pResourceName" select="'ConfirmationOf'"/>
    </xsl:call-template>
    <!-- when the product is a Straddle include in the title the text 'Swaption' whithout the Option Style (American, Bermuda and European) -->
    <xsl:choose>
      <xsl:when test="//productType='EuropeanSwaption' or //productType='AmericanSwaption' or //productType='BermudaSwaption'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'Swaption'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="//productType='debtSecurityOption' or //productType='debtSecurityAmericanOption' or //productType='debtSecurityBermudaOption' or //productType='debtSecurityEuropeanOption'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'BondOption'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pInstrument"/>
      </xsl:otherwise>
    </xsl:choose>
    <!-- 
		Remove title text straddle rule:   
		display the Straddle word into the Title when the Swaption is a Straddle in other case display a blank space 
		<xsl:choose>
		<xsl:when test="//dataDocument/trade/swaption[swaptionStraddle and swaptionStraddle='true']">
			<xsl:text>&#160;Straddle&#160;</xsl:text>
		</xsl:when>
		<xsl:otherwise>
			<xsl:text>&#160;</xsl:text>
		</xsl:otherwise>
		</xsl:choose>
		-->
    <xsl:call-template name="getTranslation">
      <xsl:with-param name="pResourceName" select="'Transaction'"/>
    </xsl:call-template>
  </xsl:template>

  <!-- GetNCs -->
  <xsl:template name="getNCS">
    <xsl:value-of select="//header/notificationConfirmationSystemIds/notificationConfirmationSystemId[@notificationConfirmationSystemIdScheme='http://www.euro-finance-systems.fr/otcml/notificationConfirmationSystemIdentifier']"/>
  </xsl:template>

  <!-- Display PremiumQuoteBasis -->
  <xsl:template name="getPremiumQuoteBasis">
    <xsl:param name="CurrencyPairLabel" />
    <xsl:choose>
      <xsl:when test="$CurrencyPairLabel = 'PercentageOfCallCurrencyAmount'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'CallCurrencyAmount'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$CurrencyPairLabel = 'PercentageOfPutCurrencyAmount'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'PutCurrencyAmount'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$CurrencyPairLabel = 'CallCurrencyPerPutCurrency'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'PutCurrencyAmountInCallCurrencyAmount'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$CurrencyPairLabel = 'PutCurrencyPerCallCurrency'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'CallCurrencyAmountInPutCurrencyAmount'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$CurrencyPairLabel = 'Explicit'">
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'Error'"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
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
  <!-- 
	==================================================================================
	only for swap: variables begin region
	varIsStream1FixedRateStepSchedule variable: "true" if the Fixed rate has some steps 
	===================================================================================
	-->
  <xsl:variable name="varIsStream1FixedRateStepSchedule" select="(//dataDocument/trade/swap/swapStream[1]/calculationPeriodAmount/calculation/fixedRateSchedule/step/node())"/>
  <xsl:variable name="varGetStream1FixedRateStepSchedule" select="//dataDocument/trade/swap/swapStream[1]/calculationPeriodAmount/calculation/fixedRateSchedule/step"/>
  <xsl:variable name="varIsStream2FixedRateStepSchedule" select="(//dataDocument/trade/swap/swapStream[2]/calculationPeriodAmount/calculation/fixedRateSchedule/step/node())"/>
  <xsl:variable name="varGetStream2FixedRateStepSchedule" select="//dataDocument/trade/swap/swapStream[2]/calculationPeriodAmount/calculation/fixedRateSchedule/step"/>
  <!-- 
	=================================================================
	== Handle IRD Stream: BEGIN REGION  
	==================================================================
	-->
  <!-- Handle floating/fixed streams of the IRD calling appropriate "display" templates to display -->
  <!-- the floating and fixed rate calculation section of the confirmation for each stream         -->

  <!-- Called by the "handleProduct" template -->
  <xsl:template name="handleIrdStreams">
    <xsl:param name="pStreams"/>
    <xsl:param name="pIsCrossCurrency" select="false()"/>
    <!-- GS 20110214: zero coupon handle-->
    <!--<xsl:param name="pIsNotionalDifferent" select="false()"/>-->
    <xsl:param name="pIsNotionalDifferent"/>
    <xsl:param name="pIsScheduledNominalDisplayInAnnex"/>
    <xsl:param name="pCapFloorType"/>
    <xsl:param name="pIsDisplayTitle" />
    <xsl:param name="pIsDisplayDetail" />
    <!-- GS 20110214: stub handle-->
    <xsl:param name="pIsTermDifferent"/>
    <xsl:param name="pIsStub"/>
    <xsl:param name="pIsAsynchronousStub"/>

    <!-- Scan Stream -->

    <xsl:for-each select="$pStreams">

      <xsl:variable name="curr" select="current()" />
      <!-- it returns the position of the stream -->
      <xsl:variable name="varStreamPosition" select="position()" />

      <xsl:if test="$curr/calculationPeriodAmount/calculation/fixedRateSchedule or $curr/calculationPeriodAmount/knownAmountSchedule or (//dataDocument/trade/capFloor/capFloorStream/node() and //dataDocument/trade/capFloor/premium/node())">
        <xsl:call-template name="displayFixedRateSchedule">
          <xsl:with-param name="pStream" select="$curr"/>
          <xsl:with-param name="pStreamPosition" select="$varStreamPosition"/>
          <xsl:with-param name="pIsCrossCurrency" select="$pIsCrossCurrency"/>
          <xsl:with-param name="pIsNotionalDifferent" select="$pIsNotionalDifferent"/>
          <xsl:with-param name="pIsScheduledNominalDisplayInAnnex" select="$pIsScheduledNominalDisplayInAnnex"/>
          <xsl:with-param name="pIsDisplayTitle" select="$pIsDisplayTitle"/>
          <xsl:with-param name="pIsDisplayDetail" select="$pIsDisplayDetail"/>
          <!-- GS 20110214: stub handle-->
          <xsl:with-param name="pIsTermDifferent" select="$pIsTermDifferent"/>
          <xsl:with-param name="pIsStub" select="$pIsStub"/>
          <xsl:with-param name="pIsAsynchronousStub" select="$pIsAsynchronousStub"/>
        </xsl:call-template>
      </xsl:if>

      <xsl:if test="$curr/calculationPeriodAmount/calculation/floatingRateCalculation">
        <xsl:choose>
          <xsl:when test="$pCapFloorType='Collar'">
            <xsl:call-template name="displayFloatingRateCalculation">
              <xsl:with-param name="pStream" select="$curr"/>
              <xsl:with-param name="pStreamPosition" select="$varStreamPosition"/>
              <xsl:with-param name="pIsCapOnly" select="true()"/>
              <xsl:with-param name="pIsFloorOnly" select="true()"/>
              <xsl:with-param name="pIsDisplayTitle" select="$pIsDisplayTitle"/>
              <xsl:with-param name="pIsDisplayDetail" select="$pIsDisplayDetail"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:if test="$curr/calculationPeriodAmount/calculation/floatingRateCalculation">
              <xsl:call-template name="displayFloatingRateCalculation">
                <xsl:with-param name="pStream" select="$curr"/>
                <xsl:with-param name="pStreamPosition" select="$varStreamPosition"/>
                <xsl:with-param name="pIsCrossCurrency" select="$pIsCrossCurrency"/>
                <xsl:with-param name="pIsNotionalDifferent" select="$pIsNotionalDifferent"/>
                <xsl:with-param name="pIsScheduledNominalDisplayInAnnex" select="$pIsScheduledNominalDisplayInAnnex"/>
                <xsl:with-param name="pIsDisplayTitle" select="$pIsDisplayTitle"/>
                <xsl:with-param name="pIsDisplayDetail" select="$pIsDisplayDetail"/>
                <!-- GS 20110214: stub handle-->
                <xsl:with-param name="pIsTermDifferent" select="$pIsTermDifferent"/>
                <xsl:with-param name="pIsStub" select="$pIsStub"/>
                <xsl:with-param name="pIsAsynchronousStub" select="$pIsAsynchronousStub"/>
              </xsl:call-template>
            </xsl:if>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:call-template name="displayDiscounting">
          <xsl:with-param name="pStream" select="$curr"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>
  <!-- 
	=================================================================
	== Handle IRD Stream: END REGION  
	==================================================================
	-->

  <!-- 
	=================================================================
	== Floating Rate IRD: BEGIN REGION  
	==================================================================
	-->
  <!-- Handle floating rate payer -->
  <xsl:template name="handleFloatingRatePayer">
    <xsl:param name="pStream"/>
    <xsl:param name="pIsCrossCurrency" select="false()"/>
    <xsl:param name="pIsCapOnly" select="false()"/>
    <xsl:param name="pIsFloorOnly" select="false()"/>
    <!--
		=======================================================================================
		In a Cap is active when floatingRate > capStrike
		the Cap Transaction Buyer Pay the CapRate
		from XML
		<capRateSchedule>
		<buyer>Receiver</buyer> 
		<seller>Payer</seller>
		=======================================================================================
		In a Floor is active when floatingRate < floorStrike
		the Floor Transaction Buyer Receive the FloorRate
		from XML
		<floorRateSchedule>
		<buyer>Receiver</buyer>
		<seller>Payer</seller> 
		=======================================================================================
		The Buyer: Pay the Premium and Pay the floatingRate
		The Seller: Pay the floorStrike
		=======================================================================================
		Acheteur Cap 
		Receiver du Stream (ReceiverCapFloorStream)
		Buyer du capRateSchedule
		FloatingRatePayer = payerCapFloorStream
		
		Acheteur Floor
		Receiver du Stream (ReceiverCapFloorStream)
		Buyer du floorRateSchedule
		FloatingRatePayer = payerCapFloorStream			
		-->
    <xsl:choose>
      <xsl:when test="$varCapFloorType = 'Cap' or $varCapFloorType = 'Floor'">
        <!-- Add in comment
				<xsl:choose>
					<xsl:when test ="//capFloor/premium/node()">
						==========================================================================
						2009-06-08 
						Modif pour pallier le pb du payer/receiver du taux flottant dans l'XML 
						des cap/floor (en attendant la correction de EFS)
						==========================================================================
						<xsl:call-template name="getPartyNameAndID" >
							<xsl:with-param name="pParty" select="//capFloor/premium/receiverPartyReference/@href"/>
						</xsl:call-template>						
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="getPartyNameAndID" >
							<xsl:with-param name="pParty" select="//capFloor/capFloorStream/receiverPartyReference/@href"/>
						</xsl:call-template>
					</xsl:otherwise>					
				</xsl:choose>
				-->
        <xsl:call-template name="getPartyNameAndID" >
          <xsl:with-param name="pParty" select="//capFloor/capFloorStream/payerPartyReference/@href"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <!--
				Please don't modify the section
				It gets the FloatingRate Payer for all IRS products
				-->
        <xsl:call-template name="getPartyNameAndID" >
          <xsl:with-param name="pParty" select="$pStream/payerPartyReference/@href" />
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 
	=====================================================================================
	== only for cap/floor
	== For Vanilla, Strangle or Collar CapFloor: it displays the Cap and the Floor rate
	== For Straddle CapFloor: it displays only one rate (strike rate)
	======================================================================================
	-->
  <xsl:template name="handleCapOrFloorRate">
    <xsl:param name="pIsCapOnly" />
    <xsl:param name="pIsFloorOnly" />
    <xsl:param name="pStream"/>

    <xsl:choose>

      <xsl:when test ="$varCapFloorType = 'Cap' or $varCapFloorType = 'Floor' or $varCapFloorType = 'Strangle' or $varCapFloorType = 'Collar'">
        <xsl:if test="$pIsFloorOnly=false() and $pStream/calculationPeriodAmount/calculation/floatingRateCalculation/capRateSchedule">
          <xsl:call-template name="displayRateSchedule">
            <xsl:with-param name="pName" select="'Cap'"/>
            <xsl:with-param name="pSchedule" select="$pStream/calculationPeriodAmount/calculation/floatingRateCalculation/capRateSchedule"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:if test="$pIsCapOnly=false() and $pStream/calculationPeriodAmount/calculation/floatingRateCalculation/floorRateSchedule">
          <xsl:call-template name="displayRateSchedule">
            <xsl:with-param name="pName" select="'Floor'"/>
            <xsl:with-param name="pSchedule" select="$pStream/calculationPeriodAmount/calculation/floatingRateCalculation/floorRateSchedule"/>
          </xsl:call-template>
        </xsl:if>
      </xsl:when>
      <xsl:when test ="$varCapFloorType = 'Straddle'">
        <xsl:call-template name="displayRateSchedule">
          <xsl:with-param name="pName" select="'Strike'"/>
          <xsl:with-param name="pSchedule" select="$pStream/calculationPeriodAmount/calculation/floatingRateCalculation/capRateSchedule"/>
        </xsl:call-template>
      </xsl:when>
      <!--
			Add Rate schedule for Corridor
			It displays cap rate 1 and cap rate 2
			-->
      <xsl:when test ="$varCapFloorType = 'Corridor'">
        <xsl:for-each select ="$pStream/calculationPeriodAmount/calculation/floatingRateCalculation/capRateSchedule">
          <xsl:call-template name="displayRateSchedule">
            <xsl:with-param name="pName" select="'Cap'"/>
            <xsl:with-param name="pSchedule" select="$pStream/calculationPeriodAmount/calculation/floatingRateCalculation/capRateSchedule"/>
          </xsl:call-template>
        </xsl:for-each>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- Get Floating Rate Payer Currency Amount -->
  <xsl:template name="getFloatingRatePayerCurrencyAmount">
    <xsl:param name="pStream"/>
    <xsl:param name="pIsCrossCurrency" select="false()"/>
    <xsl:param name="pIsCapOnly" select="false()"/>
    <xsl:param name="pIsFloorOnly" select="false()"/>
    <xsl:call-template name="format-money2">
      <xsl:with-param name="currency" select="$pStream/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/currency"/>
      <xsl:with-param name="amount" select="$pStream/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/initialValue"/>
    </xsl:call-template>
  </xsl:template>

  <!-- Get floating rate frequency for payment dates  -->
  <xsl:template name="getFloatingRatePaymentDatesFrequency">
    <xsl:param name="pStream"/>
    <xsl:param name="pStreamPosition"/>
    <xsl:call-template name="getPaymentDatesFrequency" >
      <xsl:with-param name="pStreamPosition" select="$pStreamPosition"/>
      <xsl:with-param name="pFrequency" select="$pStream/paymentDates/paymentFrequency" />
      <xsl:with-param name="pRollConvention" select="$pStream/calculationPeriodDates/calculationPeriodFrequency/rollConvention" />
    </xsl:call-template>
  </xsl:template>

  <!-- Get floating rate frequency for calculation period -->
  <xsl:template name="getFloatingRateCalculationPeriodDatesFrequency">
    <xsl:param name="pStream"/>
    <xsl:param name="pStreamPosition"/>
    <xsl:call-template name="getCalculationPeriodDatesFrequency" >
      <xsl:with-param name="pStreamPosition" select="$pStreamPosition"/>
      <xsl:with-param name="pFrequency" select="$pStream/calculationPeriodDates/calculationPeriodFrequency" />
      <xsl:with-param name="pRollConvention" select="$pStream/calculationPeriodDates/calculationPeriodFrequency/rollConvention" />
    </xsl:call-template>
  </xsl:template>

  <!-- Get the list of the fixing dates of the floating rate stream "pStreamNo" -->
  <xsl:template name="getFixingDatesList">
    <xsl:param name="pStreamNo"/>
    <xsl:param name="pStreamPosition"/>
    <xsl:for-each select="//Event[INSTRUMENTNO='1' and STREAMNO=$pStreamPosition and EVENTCODE='RES' and EVENTTYPE='FLO']/EventClass[EVENTCLASS='FXG']">
      <xsl:call-template name="format-date">
        <xsl:with-param name="xsd-date-time" select="current()/DTEVENT"/>
      </xsl:call-template>
    </xsl:for-each>
  </xsl:template>

  <!-- 
	===========================
	Get the fixing dates type 
	===========================
  FI 20151020 suppression de &#xa; et usage de <xsl:text> </xsl:text> 
  -->
  <xsl:template name="getFixingDates">
    <xsl:param name="pStream"/>
    <xsl:choose>
      <xsl:when test="$pStream/resetDates/fixingDates/periodMultiplier='0'and $pStream/resetDates/fixingDates/period='D'" >
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'EachResetDate'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <!--
				get the absolute numeric value from fixing date period multiplier
				before: -2 Business days prior to each reset date
				after :  2 Business days prior to each reset date
				-->
        <xsl:call-template name="getAbsoluteNumericValue">
          <xsl:with-param name ="pNumericValue" select="$pStream/resetDates/fixingDates/periodMultiplier"/>
        </xsl:call-template>
        <xsl:text> </xsl:text>
        <!-- FI 20161114 [GLOP] A revoir codage en dur pour démo -->
        <xsl:choose>
          <xsl:when test = "$pCurrentCulture = 'fr-FR'">
            <xsl:choose>
              <xsl:when test="$pStream/resetDates/fixingDates/period='D'">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'Days'"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="$pStream/resetDates/fixingDates/period='M'">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'Months'"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="$pStream/resetDates/fixingDates/period='W'">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'Weeks'"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="$pStream/resetDates/fixingDates/period='Y'">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'Years'"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'None'"/>
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
            <xsl:text> </xsl:text>
            <xsl:choose>
              <xsl:when test ="$pStream/resetDates/fixingDates/dayType = 'Business'">ouvrés</xsl:when>
              <xsl:otherwise>
                <xsl:value-of select ="$pStream/resetDates/fixingDates/dayType"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select ="$pStream/resetDates/fixingDates/dayType"/>
            <xsl:text> </xsl:text>
            <xsl:choose>
              <xsl:when test="$pStream/resetDates/fixingDates/period='D'">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'Days'"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="$pStream/resetDates/fixingDates/period='M'">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'Months'"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="$pStream/resetDates/fixingDates/period='W'">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'Weeks'"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:when test="$pStream/resetDates/fixingDates/period='Y'">
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'Years'"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name="getTranslation">
                  <xsl:with-param name="pResourceName" select="'None'"/>
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text> </xsl:text>
        <!-- 
				=======================================================
				Add the rule for "prior" or "after" days
				The fixing date is relative to reset date: 
				========================================================
				-->
        <xsl:choose>
          <xsl:when test="$pStream/resetDates/fixingDates/periodMultiplier &lt; 0">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'PriorToEachResetDate'"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pStream/resetDates/fixingDates/periodMultiplier &gt; 0">
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'AfterToEachResetDate'"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="getTranslation">
              <xsl:with-param name="pResourceName" select="'Error'"/>
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!-- 
	===========================
	Get  OffSet
	===========================
  FI 20151020 suppression de &#xa; et usage de <xsl:text> </xsl:text>
	-->
  <xsl:template name="getPaymentDatesOffSet">
    <xsl:param name="pStream"/>
    <!--
		get the absolute numeric value from fixing date period multiplier
		before: -2 Business days prior to each reset date
		after :  2 Business days prior to each reset date
		-->
    <xsl:call-template name="getAbsoluteNumericValue">
      <xsl:with-param name ="pNumericValue" select="$pStream/paymentDaysOffset/periodMultiplier"/>
    </xsl:call-template>
    <xsl:text> </xsl:text>
    <xsl:value-of select ="$pStream/paymentDaysOffset/dayType"/>
    <xsl:text> </xsl:text>
    <xsl:choose>
      <xsl:when test="$pStream/paymentDaysOffset/period='D'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'Days'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pStream/paymentDaysOffset/period='M'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'Months'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pStream/paymentDaysOffset/period='W'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'Weeks'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pStream/paymentDaysOffset/period='Y'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'Years'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'None'"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:text> </xsl:text>
    <!-- 
		=======================================================
		Add the rule for "prior" or "after" days
		========================================================
		-->
    <xsl:choose>
      <xsl:when test="$pStream/paymentDaysOffset/periodMultiplier &lt; 0">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'PriorToEach'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pStream/paymentDaysOffset/periodMultiplier &gt; 0">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'AfterToEach'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'Error'"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:call-template name="GetPaymentRelativeTo">
      <xsl:with-param name="pStream" select="$pStream"/>
    </xsl:call-template>
  </xsl:template>

  <!-- 
	===============================================
	== GetPaymentRelativeTo
	== returns the reference to payment date
	== relative to
	== - Calculation Period End date
	== - Calculation Period Start date
	== - Reset Date
	================================================
	-->
  <xsl:template name ="GetPaymentRelativeTo">
    <xsl:param name="pStream"/>
    <xsl:choose>
      <xsl:when test="$pStream/payRelativeTo ='CalculationPeriodEndDate'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'CalculationPeriodEndDate'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pStream/payRelativeTo ='CalculationPeriodStartDate'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'CalculationPeriodStartDate'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pStream/payRelativeTo ='ResetDate'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'ResetDate'"/>
        </xsl:call-template>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- GS 20110214: Bug fixed: handle a basis swap with EONIA floating rate index and EURIBOR floating rate index -->
  <!-- true if rate index contains substring 'OIS' (OvernightInterestSwap) - false if rate index don't contain substring 'OIS' (eg. EURIBOR)-->
  <!--old value:
  <xsl:variable name="varIsOvernightInterestSwap">
    <xsl:value-of select="contains(//dataDocument/repository/rateIndex/displayname,'OIS')"/>
  </xsl:variable>
  -->
  <xsl:template name="isOvernight">
    <xsl:param name="pStream"/>
    <xsl:value-of select="contains($pStream/calculationPeriodAmount/calculation/floatingRateCalculation/floatingRateIndex,'OIS')"/>
  </xsl:template>

  <!-- 
	=================================================================================
	Get reset dates type (the first day of each calculation period, the last day...) 
	=================================================================================
	-->
  <xsl:template name="getResetDatesType">
    <xsl:param name="pStream"/>

    <!-- GS 20110214: Bug fixed: handle a basis swap with EONIA floating rate index and EURIBOR floating rate index -->
    <xsl:variable name="varIsOvernightInterestSwap">
      <xsl:call-template name ="isOvernight">
        <xsl:with-param name="pStream" select="$pStream"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:if test ="$varIsOvernightInterestSwap = 'true'">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'TheLastDayEachCalcPeriod'"/>
      </xsl:call-template>
    </xsl:if>

    <xsl:if test ="$varIsOvernightInterestSwap = 'false'">
      <xsl:call-template name="debugXsl">
        <xsl:with-param name="pCurrentNode" select="$pStream/resetDates/resetRelativeTo"/>
      </xsl:call-template>
      <xsl:choose>
        <xsl:when test="$pStream/resetDates/resetRelativeTo='CalculationPeriodStartDate'">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'FirstDayOfEachCalPeriod'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pStream/resetDates/resetRelativeTo='CalculationPeriodEndDate'">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'TheLastDayEachCalcPeriod'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'None'"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
  </xsl:template>

  <!-- Get if Reset Dates is Rate Cut Off Days Offset-->
  <xsl:template name="getRateCutOffDaysOffset">
    <xsl:param name="pStream"/>
    <xsl:param name="pSeparator"/>
    <xsl:param name="pDelimiter"/>
    <xsl:call-template name="getOffset" >
      <xsl:with-param name="pOffset" select="$pStream/resetDates/rateCutOffDaysOffset"/>
    </xsl:call-template>
  </xsl:template>

  <!-- 
	====================================================================
	== Only for CAP/Floor
	== Get CapFloor type
	== Collar:   BUY CAP - SELL FLOOR
	== Strangle: BUY CAP - BUY FLOOR and CAP RATE != FLOOR RATE
	== Straddle: BUY CAP - BUY FLOOR and CAP RATE = FLOOR RATE
	== Corridor: BUY CAP1 - SELL CAP2 and CAP1 RATE != CAP2 RATE
	== This section is intentionally not translate
	=====================================================================
	-->
  <xsl:variable name ="varCapRate">
    <xsl:value-of select="//capFloor/capFloorStream/calculationPeriodAmount/calculation/floatingRateCalculation/capRateSchedule[1]/initialValue"/>
  </xsl:variable>

  <!-- 
	only for corridor: this variable returns 2° cap rate (initial value)
	-->
  <xsl:variable name ="varCapRate2">
    <xsl:value-of select="//capFloor/capFloorStream/calculationPeriodAmount/calculation/floatingRateCalculation/capRateSchedule[2]/initialValue"/>
  </xsl:variable>

  <xsl:variable name ="varFloorRate">
    <xsl:value-of select="//capFloor/capFloorStream/calculationPeriodAmount/calculation/floatingRateCalculation/floorRateSchedule/initialValue"/>
  </xsl:variable>

  <xsl:variable name="varCapFloorFloatingRateCalculation" select="//capFloor/capFloorStream/calculationPeriodAmount/calculation/floatingRateCalculation"/>

  <xsl:variable name="varCapFloorType">
    <xsl:choose>
      <xsl:when test ="$varCapFloorFloatingRateCalculation/capRateSchedule/buyer = 'Receiver' and $varCapFloorFloatingRateCalculation/capRateSchedule/seller = 'Payer'
					  and $varCapFloorFloatingRateCalculation/floorRateSchedule/buyer = 'Payer' and $varCapFloorFloatingRateCalculation/floorRateSchedule/seller = 'Receiver'">
        <xsl:text>Collar</xsl:text>
      </xsl:when>

      <xsl:when test ="$varCapFloorFloatingRateCalculation/capRateSchedule/buyer = 'Receiver' and $varCapFloorFloatingRateCalculation/capRateSchedule/seller = 'Payer'
					  and $varCapFloorFloatingRateCalculation/floorRateSchedule/buyer = 'Receiver' and $varCapFloorFloatingRateCalculation/floorRateSchedule/seller = 'Payer'
					  and $varCapRate != $varFloorRate">
        <xsl:text>Strangle</xsl:text>
      </xsl:when>

      <xsl:when test ="$varCapFloorFloatingRateCalculation/capRateSchedule/buyer = 'Receiver' and $varCapFloorFloatingRateCalculation/capRateSchedule/seller = 'Payer'
					  and $varCapFloorFloatingRateCalculation/floorRateSchedule/buyer = 'Receiver' and $varCapFloorFloatingRateCalculation/floorRateSchedule/seller = 'Payer'
					  and $varCapRate = $varFloorRate">
        <xsl:text>Straddle</xsl:text>
      </xsl:when>
      <!-- 
			Modified Corridor definition (Buy cap1 and Sell cap2)
			+ Add condition caprate1 != caprate2
			-->
      <xsl:when test ="$varCapFloorFloatingRateCalculation/capRateSchedule[1]/buyer = 'Receiver' and $varCapFloorFloatingRateCalculation/capRateSchedule[1]/seller = 'Payer'
					     and $varCapFloorFloatingRateCalculation/capRateSchedule[2]/buyer = 'Payer' and $varCapFloorFloatingRateCalculation/capRateSchedule[2]/seller = 'Receiver'
					     and $varCapRate != $varCapRate2">
        <xsl:text>Corridor</xsl:text>
      </xsl:when>

      <xsl:when test ="$varCapFloorFloatingRateCalculation/capRateSchedule/node()= true() and $varCapFloorFloatingRateCalculation/floorRateSchedule/node()=false()">
        <xsl:text>Cap</xsl:text>
      </xsl:when>

      <xsl:when test ="$varCapFloorFloatingRateCalculation/floorRateSchedule/node()= true() and $varCapFloorFloatingRateCalculation/capRateSchedule/node()=false()">
        <xsl:text>Floor</xsl:text>
      </xsl:when>

    </xsl:choose>

  </xsl:variable>


  <!-- 
	=====================================
	Only for CAP/Floor
	Get CapFloor premium rate Payer
	======================================
	-->
  <xsl:template name="getCapFloorPremiumRatePayer">
    <xsl:param name="pPremium"/>
    <xsl:call-template name="getPartyNameAndID" >
      <xsl:with-param name="pParty" select="$pPremium/payerPartyReference/@href" />
    </xsl:call-template>
  </xsl:template>

  <!-- 
	=================================
	Only for CAP/Floor
	Get CapFloor premium rate
	=================================
	-->
  <xsl:template name="getCapFloorPremiumRate">
    <xsl:param name="pPremium"/>
    <xsl:call-template name="format-fixed-rate">
      <xsl:with-param name="fixed-rate" select="$pPremium/paymentQuote/percentageRate" />
    </xsl:call-template>
  </xsl:template>

  <!-- 
	==============================
	Only for CAP/Floor
	Get CapFloor premium amount
	==============================
	-->
  <xsl:template name="getCapFloorPremiumAmount">
    <xsl:param name="pPremium"/>
    <xsl:call-template name="format-money2">
      <xsl:with-param name="currency" select="$pPremium/paymentAmount/currency"/>
      <xsl:with-param name="amount" select="$pPremium/paymentAmount/amount"/>
    </xsl:call-template>
  </xsl:template>

  <!-- 
	==================================
	Only for CAP/Floor
	Get CapFloor premium payment date
	===================================
	-->
  <xsl:template name="getCapFloorPremiumPaymentDate">
    <xsl:param name="pPremium"/>
    <xsl:call-template name="format-date">
      <xsl:with-param name="xsd-date-time" select="$pPremium/paymentDate/unadjustedDate"/>
    </xsl:call-template>
    <xsl:call-template name="getBusinessDayConvention" >
      <xsl:with-param name="pBusinessDayConvention" select="$pPremium/paymentDate/dateAdjustments/businessDayConvention" />
    </xsl:call-template>
  </xsl:template>

  <!-- 
	=================================================================
	== Floating Rate IRD: END REGION  
	==================================================================
	-->
  <!-- 
	=================================================================
	== Fixed Rate IRD: BEGIN REGION  
	==================================================================
	-->
  <!-- Handle Fixed Amount -->
  <xsl:template name="handleFixedAmount">
    <xsl:param name="pStream"/>
    <!-- GS 20110214: zero coupon handle: add parameter-->
    <xsl:param name="pIsNotionalDifferent"/>

    <xsl:call-template name="displayAmountSchedule">
      <xsl:with-param name="pName" select="'Fixed Amount:'"/>
      <xsl:with-param name="pIsNotionalDifferent" select="$pIsNotionalDifferent"/>
      <xsl:with-param name="pCurrency" select="$pStream/calculationPeriodAmount/knownAmountSchedule/currency"/>
      <xsl:with-param name="pSchedule" select="$pStream/calculationPeriodAmount/knownAmountSchedule"/>
    </xsl:call-template>
  </xsl:template>

  <!--Get the rate of the fixed rate stream -->
  <xsl:template name="getFixedRate">
    <xsl:param name="pStream"/>
    <xsl:call-template name="format-fixed-rate">
      <xsl:with-param name="fixed-rate" select="$pStream/calculationPeriodAmount/calculation/fixedRateSchedule/initialValue" />
    </xsl:call-template>
  </xsl:template>

  <!--
	Get the rate of the stub fixed rate stream 
	-->
  <xsl:template name="getFixedStubRate">
    <xsl:param name="pStream"/>

    <xsl:if test ="$pStream/stubCalculationPeriodAmount/initialStub/node()=true()">
      <xsl:call-template name="format-fixed-rate">
        <xsl:with-param name="fixed-rate" select="$pStream/stubCalculationPeriodAmount/initialStub/stubRate" />
      </xsl:call-template>
    </xsl:if>

    <xsl:if test ="$pStream/stubCalculationPeriodAmount/finalStub/node()=true()">
      <xsl:call-template name="format-fixed-rate">
        <xsl:with-param name="fixed-rate" select="$pStream/stubCalculationPeriodAmount/finalStub/stubRate" />
      </xsl:call-template>
    </xsl:if>

  </xsl:template>

  <!--
	Get the rate of the stub floating rate stream
	if the stub countains 1 floating rate it returns the asset (es. EUR/EURIBOR-telerate 3M)
	if the stub countains 2 floating rate it returns the information for asset interpolation 
	(es. interpolation between EUR/EURIBOR-telerate 3M and EUR/EURIBOR-telerate 1M)
	to do:
	Add the floating rate index name from the node (//repository/rateIndex/displayname)
	-->
  <xsl:template name="getFloatingStubRate">
    <xsl:param name="pStream"/>
    <xsl:param name="pTotalFloatingStubRate"/>

    <!-- one floating stub rate -->
    <xsl:if test="$pTotalFloatingStubRate &lt; 2">

      <xsl:if test ="$pStream/stubCalculationPeriodAmount/initialStub/floatingRate/node()=true()">
        <xsl:value-of select ="$pStream/stubCalculationPeriodAmount/initialStub/floatingRate/floatingRateIndex"/>
        <xsl:text> </xsl:text>
        <xsl:call-template name="getFrequency" >
          <xsl:with-param name="pFrequency" select="$pStream/stubCalculationPeriodAmount/initialStub/floatingRate/indexTenor"/>
        </xsl:call-template>
      </xsl:if>

      <xsl:if test ="$pStream/stubCalculationPeriodAmount/finalStub/floatingRate/node()=true()">
        <xsl:value-of select ="$pStream/stubCalculationPeriodAmount/finalStub/floatingRate/floatingRateIndex"/>
        <xsl:text> </xsl:text>
        <xsl:call-template name="getFrequency" >
          <xsl:with-param name="pFrequency" select="$pStream/stubCalculationPeriodAmount/finalStub/floatingRate/indexTenor"/>
        </xsl:call-template>
      </xsl:if>

    </xsl:if>

    <!-- two floating stub rates -->
    <xsl:if test="$pTotalFloatingStubRate &gt; 1">

      <xsl:if test ="$pStream/stubCalculationPeriodAmount/initialStub/node()=true()">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'InterpolationBetween_'"/>
        </xsl:call-template>
        <xsl:value-of select ="$pStream/stubCalculationPeriodAmount/initialStub/floatingRate[1]/floatingRateIndex"/>
        <xsl:text> </xsl:text>
        <xsl:call-template name="getFrequency" >
          <xsl:with-param name="pFrequency" select="$pStream/stubCalculationPeriodAmount/initialStub/floatingRate[1]/indexTenor"/>
        </xsl:call-template>
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'_And_'"/>
        </xsl:call-template>
        <xsl:value-of select ="$pStream/stubCalculationPeriodAmount/initialStub/floatingRate[2]/floatingRateIndex"/>
        <xsl:text> </xsl:text>
        <xsl:call-template name="getFrequency" >
          <xsl:with-param name="pFrequency" select="$pStream/stubCalculationPeriodAmount/initialStub/floatingRate[2]/indexTenor"/>
        </xsl:call-template>
      </xsl:if>

      <xsl:if test ="$pStream/stubCalculationPeriodAmount/finalStub/node()=true()">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'InterpolationBetween_'"/>
        </xsl:call-template>
        <xsl:value-of select ="$pStream/stubCalculationPeriodAmount/finalStub/floatingRate[1]/floatingRateIndex"/>
        <xsl:text> </xsl:text>
        <xsl:call-template name="getFrequency" >
          <xsl:with-param name="pFrequency" select="$pStream/stubCalculationPeriodAmount/finalStub/floatingRate[1]/indexTenor"/>
        </xsl:call-template>
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'_And_'"/>
        </xsl:call-template>
        <xsl:value-of select ="$pStream/stubCalculationPeriodAmount/finalStub/floatingRate[2]/floatingRateIndex"/>
        <xsl:text> </xsl:text>
        <xsl:call-template name="getFrequency" >
          <xsl:with-param name="pFrequency" select="$pStream/stubCalculationPeriodAmount/finalStub/floatingRate[2]/indexTenor"/>
        </xsl:call-template>
      </xsl:if>

    </xsl:if>

  </xsl:template>

  <!-- Get the list of the payment dates for the fixed rate stream -->
  <xsl:template name="getFixedRatePaymentDatesList">
    <xsl:param name="pStreamNo"/>
    <xsl:param name="pSeparator"/>
    <xsl:param name="pDelimiter"/>
    <xsl:for-each select ="//Event[STREAMNO=$pStreamNo and EVENTCODE='INT' and EVENTTYPE='INT']/EventClass[EVENTCLASS = 'STL']">
      <xsl:copy-of select="$pDelimiter"/>
      <xsl:call-template name="format-date">
        <xsl:with-param name="xsd-date-time" select="current()/DTEVENT"/>
      </xsl:call-template>
      <xsl:copy-of select="$pDelimiter"/>
      <xsl:copy-of select="$pSeparator"/>
    </xsl:for-each>
    <xsl:for-each select ="//Event[STREAMNO=$pStreamNo and EVENTCODE='TER' and EVENTTYPE='INT']/EventClass[EVENTCLASS = 'STL']">
      <xsl:copy-of select="$pDelimiter"/>
      <xsl:call-template name="format-date">
        <xsl:with-param name="xsd-date-time" select="current()/DTEVENT"/>
      </xsl:call-template>
      <xsl:copy-of select="$pDelimiter"/>
      <xsl:copy-of select="$pSeparator"/>
    </xsl:for-each>
  </xsl:template>

  <!-- Get Fixed Rate Payer Currency Amount -->
  <xsl:template name="getFixedRatePayerCurrencyAmount">
    <xsl:param name="pStream"/>
    <xsl:param name="pSeparator"/>
    <xsl:param name="pDelimiter"/>
    <xsl:call-template name="format-money2">
      <xsl:with-param name="currency" select="$pStream/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/currency"/>
      <xsl:with-param name="amount" select="$pStream/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/initialValue"/>
    </xsl:call-template>
  </xsl:template>

  <!-- Get the frequency of the payment dates for fixed rate stream -->
  <xsl:template name="getFixedRatePaymentDatesFrequency">
    <xsl:param name="pStream"/>
    <xsl:param name="pStreamPosition"/>
    <xsl:call-template name="getPaymentDatesFrequency" >
      <!-- DR -->
      <xsl:with-param name="pStreamPosition" select="$pStreamPosition"/>
      <xsl:with-param name="pFrequency" select="$pStream/paymentDates/paymentFrequency" />
      <xsl:with-param name="pRollConvention" select="$pStream/calculationPeriodDates/calculationPeriodFrequency/rollConvention" />
    </xsl:call-template>
  </xsl:template>

  <!-- Get floating rate frequency for calculation period -->
  <xsl:template name="getFixedRateCalculationPeriodDatesFrequency">
    <xsl:param name="pStream"/>
    <xsl:param name="pStreamPosition"/>
    <xsl:call-template name="getCalculationPeriodDatesFrequency" >
      <xsl:with-param name="pStreamPosition" select="$pStreamPosition"/>
      <xsl:with-param name="pFrequency" select="$pStream/calculationPeriodDates/calculationPeriodFrequency" />
      <xsl:with-param name="pRollConvention" select="$pStream/calculationPeriodDates/calculationPeriodFrequency/rollConvention" />
    </xsl:call-template>
  </xsl:template>

  <!-- 
	=================================================================
	== Fixed Rate IRD: END REGION  
	==================================================================
	-->
  <!-- 
	=================================================================
	== Premium Payment Date: BEGIN REGION  
	==================================================================
	-->
  <!-- Get Premium Payment Date -->
  <xsl:template name="getPremiumPaymentDate">
    <xsl:if test="paymentDate">
      <xsl:call-template name="format-date">
        <xsl:with-param name="xsd-date-time" select="paymentDate/unadjustedDate"/>
      </xsl:call-template>
    </xsl:if>
    <xsl:if test="adjustedPaymentDate">
      <xsl:call-template name="format-date">
        <xsl:with-param name="xsd-date-time" select="adjustedPaymentDate"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
  <!-- 
	=================================================================
	== Premium Payment Date: END REGION  
	==================================================================
	-->
  <!-- 
	=================================================================
	== Method of Averaging: BEGIN REGION  
	==================================================================
	-->
  <!-- Get Method of Averaging -->
  <xsl:template name="getMethodOfAveraging">
    <xsl:param name="pStream"/>
    <xsl:param name="pSeparator"/>
    <xsl:param name="pDelimiter"/>
    <xsl:value-of select="$pStream/calculationPeriodAmount/calculation/floatingRateCalculation/averagingMethod"/> Average
  </xsl:template>
  <!-- 
	=================================================================
	== Method of Averaging: END REGION  
	==================================================================
	-->

  <!-- 
	=================================================================
	== Compounding: BEGIN REGION  
	==================================================================
	-->
  <!-- Get Compounding -->
  <xsl:template name="getCompounding">
    <xsl:param name="pStream"/>
    <xsl:param name="pSeparator"/>
    <xsl:param name="pDelimiter"/>
    <xsl:call-template name="debugXsl">
      <xsl:with-param name="pCurrentNode" select="$pStream/calculationPeriodAmount/calculation/compoundingMethod"/>
    </xsl:call-template>
    <xsl:choose>
      <xsl:when test="$pStream/calculationPeriodAmount/calculation/compoundingMethod/node()">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'Applicable'"/>
        </xsl:call-template>
        <xsl:text> (</xsl:text>
        <xsl:value-of select="$pStream/calculationPeriodAmount/calculation/compoundingMethod"/>
        <xsl:text>) </xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'Inapplicable'"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- 
	=================================================================
	== Compounding: END REGION  
	==================================================================
	-->
  <!-- 
	=================================================================
	== Discount Rate: BEGIN REGION  
	==================================================================
	-->
  <!-- Get Discount Rate -->
  <xsl:template name="getDiscountRate">
    <xsl:param name="pStream"/>
    <xsl:param name="pSeparator"/>
    <xsl:param name="pDelimiter"/>
    <xsl:call-template name="format-fixed-rate">
      <xsl:with-param name="fixed-rate" select="$pStream/calculationPeriodAmount/calculation/discounting/discountRate" />
    </xsl:call-template>
  </xsl:template>
  <!-- 
	=================================================================
	== Discount Rate: END REGION  
	==================================================================
	-->
  <!-- 
	=================================================================
	== Discount Rate Day Count Fraction: BEGIN REGION  
	==================================================================
	-->
  <!-- Get Discount Rate Day Count Fraction -->
  <xsl:template name="getDiscountRateDayCountFraction">
    <xsl:param name="pStream"/>
    <xsl:param name="pSeparator"/>
    <xsl:param name="pDelimiter"/>
    <xsl:value-of select="$pStream/calculationPeriodAmount/calculation/discounting/discountRateDayCountFraction"/>
  </xsl:template>
  <!-- 
	=================================================================
	== Discount Rate Day Count Fraction: END REGION  
	==================================================================
	-->
  <!-- 
	=================================================================
	== FRA Discounting: BEGIN REGION  
	==================================================================
	-->
  <!-- FRA Discounting -->
  <xsl:template name="FRADiscounting">
    <xsl:param name="pStream"/>

    <xsl:call-template name="debugXsl">
      <xsl:with-param name="pCurrentNode" select="$pStream"/>
    </xsl:call-template>

    <xsl:choose>
      <xsl:when test="contains($pStream,'ISDA')">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'Applicable'"/>
        </xsl:call-template>

      </xsl:when>
      <xsl:when test="contains($pStream,'AFMA')">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'Applicable_AFMA'"/>
        </xsl:call-template>
        Applicable (AFMA)
      </xsl:when>
      <xsl:when test="contains($pStream,'NONE')">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'None'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pStream"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- 
	=================================================================
	== FRA Discounting: END REGION  
	==================================================================
	-->

  <!-- 
	=================================================================
	== Provisions: BEGIN REGION  
	==================================================================
	-->
  <!-- Handle provisions template -->

  <xsl:template name="handleProvisions">
    <xsl:param name="pInstrument"/>

    <xsl:choose>
      <!-- optional early termination-->
      <xsl:when test ="$pInstrument/earlyTerminationProvision/optionalEarlyTermination=true()">
        <xsl:choose>
          <!-- european exercise -->
          <xsl:when test ="$pInstrument/earlyTerminationProvision/optionalEarlyTermination/europeanExercise/node()=true()">
            <xsl:call-template name ="displayOptionalEarlyTermination">
              <xsl:with-param name="pExerciseType" select ="$pInstrument/earlyTerminationProvision/optionalEarlyTermination/europeanExercise"/>
              <xsl:with-param name="pInstrument" select ="$pInstrument"/>
            </xsl:call-template>
            <xsl:call-template name="displayProvisionProcedureForExercise">
              <xsl:with-param name="pPathProvision" select="$pInstrument/earlyTerminationProvision/optionalEarlyTermination"/>
            </xsl:call-template>
            <xsl:call-template name="displayProvisionSettlementTerms">
              <xsl:with-param name="pPathProvision" select="$pInstrument/earlyTerminationProvision/optionalEarlyTermination"/>
            </xsl:call-template>
          </xsl:when>
          <!-- american exercise -->
          <xsl:when test ="$pInstrument/earlyTerminationProvision/optionalEarlyTermination/americanExercise/node()=true()">
            <xsl:call-template name ="displayOptionalEarlyTermination">
              <xsl:with-param name="pExerciseType" select ="$pInstrument/earlyTerminationProvision/optionalEarlyTermination/americanExercise"/>
              <xsl:with-param name="pInstrument" select ="$pInstrument"/>
            </xsl:call-template>
            <xsl:call-template name="displayProvisionProcedureForExercise">
              <xsl:with-param name="pPathProvision" select="$pInstrument/earlyTerminationProvision/optionalEarlyTermination"/>
            </xsl:call-template>
            <xsl:call-template name="displayProvisionSettlementTerms">
              <xsl:with-param name="pPathProvision" select="$pInstrument/earlyTerminationProvision/optionalEarlyTermination"/>
            </xsl:call-template>
          </xsl:when>
          <!-- bermuda exercise -->
          <xsl:when test ="$pInstrument/earlyTerminationProvision/optionalEarlyTermination/bermudaExercise/node()=true()">
            <xsl:call-template name ="displayOptionalEarlyTermination">
              <xsl:with-param name="pExerciseType" select ="$pInstrument/earlyTerminationProvision/optionalEarlyTermination/bermudaExercise"/>
              <xsl:with-param name="pInstrument" select ="$pInstrument"/>
            </xsl:call-template>
            <xsl:call-template name="displayProvisionProcedureForExercise">
              <xsl:with-param name="pPathProvision" select="$pInstrument/earlyTerminationProvision/optionalEarlyTermination"/>
            </xsl:call-template>
            <xsl:call-template name="displayProvisionSettlementTerms">
              <xsl:with-param name="pPathProvision" select="$pInstrument/earlyTerminationProvision/optionalEarlyTermination"/>
            </xsl:call-template>
          </xsl:when>

        </xsl:choose>
      </xsl:when>
      <!-- mandatory early termination-->
      <xsl:when test ="$pInstrument/earlyTerminationProvision/mandatoryEarlyTermination=true()">
        <xsl:choose>
          <!-- european exercise -->
          <xsl:when test ="$pInstrument/earlyTerminationProvision/mandatoryEarlyTermination/europeanExercise">
            <xsl:call-template name ="displayMandatoryEarlyTermination">
              <xsl:with-param name="pPathProvision" select ="$pInstrument/earlyTerminationProvision/mandatoryEarlyTermination/europeanExercise"/>
            </xsl:call-template>
            <xsl:call-template name="displayProvisionSettlementTerms">
              <xsl:with-param name="pPathProvision" select ="$pInstrument/earlyTerminationProvision/mandatoryEarlyTermination"/>
            </xsl:call-template>
          </xsl:when>
          <!-- american exercise -->
          <xsl:when test ="$pInstrument/earlyTerminationProvision/mandatoryEarlyTermination/americanExercise">
            <xsl:call-template name ="displayMandatoryEarlyTermination">
              <xsl:with-param name="pPathProvision" select ="$pInstrument/earlyTerminationProvision/mandatoryEarlyTermination/americanExercise"/>
            </xsl:call-template>
            <xsl:call-template name="displayProvisionSettlementTerms">
              <xsl:with-param name="pPathProvision" select ="$pInstrument/earlyTerminationProvision/mandatoryEarlyTermination"/>
            </xsl:call-template>
          </xsl:when>
          <!-- bermuda exercise -->
          <xsl:when test ="$pInstrument/earlyTerminationProvision/mandatoryEarlyTermination/bermudaExercise">
            <xsl:call-template name ="displayMandatoryEarlyTermination">
              <xsl:with-param name="pPathProvision" select ="$pInstrument/earlyTerminationProvision/mandatoryEarlyTermination/bermudaExercise"/>
            </xsl:call-template>
            <xsl:call-template name="displayProvisionSettlementTerms">
              <xsl:with-param name="pPathProvision" select ="$pInstrument/earlyTerminationProvision/mandatoryEarlyTermination"/>
            </xsl:call-template>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <!-- optional cancelable provision-->
      <xsl:when test ="$pInstrument/cancelableProvision=true()">
        <xsl:choose>
          <!-- european exercise -->
          <xsl:when test ="$pInstrument/cancelableProvision/europeanExercise">
            <xsl:call-template name ="displayCancelableProvision">
              <xsl:with-param name="pExerciseType" select ="$pInstrument/cancelableProvision/europeanExercise"/>
              <xsl:with-param name="pInstrument" select ="$pInstrument"/>
            </xsl:call-template>
            <xsl:call-template name="displayProvisionProcedureForExercise">
              <xsl:with-param name="pPathProvision" select="$pInstrument/cancelableProvision"/>
            </xsl:call-template>
          </xsl:when>
          <!-- american exercise -->
          <xsl:when test ="$pInstrument/cancelableProvision/americanExercise">
            <xsl:call-template name ="displayCancelableProvision">
              <xsl:with-param name="pExerciseType" select ="$pInstrument/cancelableProvision/americanExercise"/>
              <xsl:with-param name="pInstrument" select ="$pInstrument"/>
            </xsl:call-template>
            <xsl:call-template name="displayProvisionProcedureForExercise">
              <xsl:with-param name="pPathProvision" select="$pInstrument/cancelableProvision"/>
            </xsl:call-template>
          </xsl:when>
          <!-- bermuda exercise -->
          <xsl:when test ="$pInstrument/cancelableProvision/bermudaExercise">
            <xsl:call-template name ="displayCancelableProvision">
              <xsl:with-param name="pExerciseType" select ="$pInstrument/cancelableProvision/bermudaExercise"/>
              <xsl:with-param name="pInstrument" select ="$pInstrument"/>
            </xsl:call-template>
            <xsl:call-template name="displayProvisionProcedureForExercise">
              <xsl:with-param name="pPathProvision" select="$pInstrument/cancelableProvision"/>
            </xsl:call-template>
          </xsl:when>

        </xsl:choose>

      </xsl:when>
    </xsl:choose>

  </xsl:template>

  <!-- 
	getProvisionOptionStyle template
	it returns provision Option style
	-->
  <xsl:template name="getProvisionOptionStyle">
    <xsl:param name="pPathProvision"/>

    <xsl:choose>
      <xsl:when test ="$pPathProvision/americanExercise/node()=true()">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'American'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test ="$pPathProvision/europeanExercise/node()=true()">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'European'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test ="$pPathProvision/bermudaExercise/node()=true()">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'Bermuda'"/>
        </xsl:call-template>
      </xsl:when>
    </xsl:choose>

  </xsl:template>

  <!-- 
	getProvisionCalculationAgent template
	it returns The calculation Agent for the provision
	-->
  <xsl:template name="getProvisionCalculationAgent">
    <xsl:param name="pPathProvision"/>

    <xsl:call-template name="getPartyNameAndID" >
      <xsl:with-param name="pParty" select="$pPathProvision/calculationAgent/calculationAgentPartyReference/@href" />
    </xsl:call-template>

  </xsl:template>


  <!-- 
	getRelevantUnderlyngDate template 
	It returns the early termination date 
	-->
  <xsl:template name="getRelevantUnderlyngAdjustableOrRelativeDates">
    <xsl:param name="pExerciseType"/>
    <xsl:choose>
      <xsl:when test="$pExerciseType/relevantUnderlyingDate/adjustableDates/node()=true()">
        <xsl:call-template name="getAdjustableDates">
          <xsl:with-param name="pAdjustableDates" select="$pExerciseType/relevantUnderlyingDate/adjustableDate"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pExerciseType/relevantUnderlyingDate/relativeDates/node()=true()">
        <xsl:call-template name="getRelativeDate">
          <xsl:with-param name="pRelativeDate" select="$pExerciseType/relevantUnderlyingDate/relativeDates"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'Error'"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!-- 
	get either the adjustable or the relative  or daterange date only when the node contains the path adjustableDate or relativeDate (without 's') or businessDateRange
	different: 
	getAdjustableOrRelativeDate = select="$pDate/adjustableDates"
	getProvisionsAdjustableOrRelativeOrRangeDate = select="$pDate/adjustableDate"
	-->
  <!-- 
	old name: getProvisionsAdjustableOrRelativeDate 
	new name: getProvisionsAdjustableOrRelativeOrRangeDate
	to do replace in all sections
	-->

  <xsl:template name="getProvisionsAdjustableOrRelativeOrRangeDate">
    <xsl:param name="pDate" />
    <xsl:choose>
      <xsl:when test="$pDate/adjustableDates/node()=true()">
        <xsl:call-template name="getAdjustableDates">
          <xsl:with-param name="pAdjustableDates" select="$pDate/adjustableDate"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pDate/relativeDate/node()=true()">
        <xsl:call-template name="getRelativeDate">
          <xsl:with-param name="pRelativeDate" select="$pDate/relativeDate"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$pDate/businessDateRange/node()=true()">
        <xsl:call-template name="getDateRange">
          <xsl:with-param name="pDateRange" select="$pDate/businessDateRange"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'Error'"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 
	It returns a date range
	unadjustedFirstDate
	unadjustedLastDate
	-->

  <xsl:template name="getDateRange">
    <xsl:param name="pDateRange" />
    <xsl:call-template name="getTranslation">
      <xsl:with-param name="pResourceName" select="'CommencingOn'"/>
    </xsl:call-template>
    <xsl:call-template name="format-date">
      <xsl:with-param name="xsd-date-time" select="$pDateRange/unadjustedFirstDate"/>
    </xsl:call-template>
    <!-- 
			the definition 'through and including' has been replaced by the definition 'up to and including'
			(from ISDA 2006 definition)
			<xsl:call-template name="getTranslation">
				<xsl:with-param name="pResourceName" select="'_throughAndIncluding_'"/>
			</xsl:call-template>
			-->
    <xsl:call-template name="getTranslation">
      <xsl:with-param name="pResourceName" select="'_UpToAndIncluding_'"/>
    </xsl:call-template>

    <xsl:call-template name="format-date">
      <xsl:with-param name="xsd-date-time" select="$pDateRange/unadjustedLastDate"/>
    </xsl:call-template>
    <xsl:call-template name="getBusinessDayConvention" >
      <xsl:with-param name="pBusinessDayConvention" select="$pDateRange/businessDayConvention" />
    </xsl:call-template>
  </xsl:template>

  <!-- 
	Template 
	it returns the display name in Date Relative To
	eg. 
	Before: Minus 2 days business determinated respect to OET_CashSettlementPaymentDate 
	After: Minus 2 days business determinated respect to Cash Settlement Payment Date 
	-->

  <xsl:template name="getDateRelativeToDisplayName">
    <xsl:param name="pDateRelativeTo" />
    <xsl:choose>
      <xsl:when test ="$pDateRelativeTo='OET_ExerciseDates'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'OET_ExerciseDates'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test ="$pDateRelativeTo='OET_CashSettlementPaymentDate'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'OET_CashSettlementPaymentDate'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test ="$pDateRelativeTo='calculationPeriodDates' or $pDateRelativeTo='calculationPeriodDates2'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'calculationPeriodDates'"/>
        </xsl:call-template>
      </xsl:when>
      <!-- EG 20160405 Migration vs2013 -->
      <xsl:when test ="$pDateRelativeTo='effectiveDate' or $pDateRelativeTo='effectiveDate2'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'effectiveDate_'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test ="$pDateRelativeTo='floatingCalcPeriodDates'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'floatingCalcPeriodDates'"/>
        </xsl:call-template>
      </xsl:when>
      <!-- EG 20160405 Migration vs2013 -->
      <xsl:when test ="$pDateRelativeTo='paymentDates' or $pDateRelativeTo='paymentDates1' or $pDateRelativeTo='paymentDates2'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'paymentDates_'"/>
        </xsl:call-template>
      </xsl:when>
      <!-- EG 20160405 Migration vs2013 -->
      <xsl:when test ="$pDateRelativeTo='resetDates' or $pDateRelativeTo='resetDates1' or $pDateRelativeTo='resetDates2'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'resetDates_'"/>
        </xsl:call-template>
      </xsl:when>
      <!-- EG 20160405 Migration vs2013 -->
      <xsl:when test ="$pDateRelativeTo='terminationDate' or $pDateRelativeTo='terminationDate1' or $pDateRelativeTo='terminationDate2'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'terminationDate_'"/>
        </xsl:call-template>
      </xsl:when>
      <!-- EG 20160405 Migration vs2013 -->
      <xsl:when test ="$pDateRelativeTo='tradeDate'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'tradeDate_'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test ="$pDateRelativeTo='RelevantUnderlyingDate' or $pDateRelativeTo='EarlyTerminationDate'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'earlyTerminationDate'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test ="$pDateRelativeTo='expirationDate' or $pDateRelativeTo='bermudaDates'">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'exerciseDate'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pDateRelativeTo"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>
  <!-- 
	=================================================================
	== Provisions: END REGION  
	==================================================================
	-->

  <!--
	=================================================================
	== Annex: BEGIN REGION  
	==================================================================
	-->

  <!-- getScheduledNominalAnnexNumber template-->
  <!-- it returns the annex number  -->
  <xsl:template name ="getScheduledNominalAnnexNumber">
    <xsl:param name="pIsNotionalDifferent"/>

    <!-- varNotionalStepParametersCount -->
    <!-- notionalStepParameters count value for current stream (if the node exists = 1) -->
    <!-- when notional amount is different for all streams (pIsNotionalDifferent = true)it scans all streams
			   when notional amount is the same for all streams (pIsNotionalDifferent = false)it scans the first stream-->
    <xsl:variable name ="varNotionalStepParametersCount">
      <xsl:if test ="$pIsNotionalDifferent = 'true'">
        <xsl:value-of select ="count( calculationPeriodAmount/calculation/notionalSchedule/notionalStepParameters )"/>
      </xsl:if>
      <xsl:if test ="$pIsNotionalDifferent != 'true'">
        <xsl:choose>
          <xsl:when test ="//dataDocument/trade/capFloor/capFloorStream/node()">
            <xsl:value-of select ="count( //capFloorStream/calculationPeriodAmount/calculation/notionalSchedule/notionalStepParameters )"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select ="count( //swapStream[1]/calculationPeriodAmount/calculation/notionalSchedule/notionalStepParameters )"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>
    </xsl:variable>

    <!-- varNotionalStepCount -->
    <!-- NotionalStep count value for current stream (to do: test when it returns more steps) -->
    <!-- when notional amount is different for all streams (pIsNotionalDifferent = true)it scans all streams
			   when notional amount is the same for all streams (pIsNotionalDifferent = false)it scans the first stream-->
    <xsl:variable name ="varNotionalStepCount">
      <xsl:if test ="$pIsNotionalDifferent = 'true'">
        <xsl:value-of select ="count( calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/step )"/>
      </xsl:if>
      <xsl:if test ="$pIsNotionalDifferent != 'true'">
        <xsl:choose>
          <xsl:when test ="//dataDocument/trade/capFloor/capFloorStream/node()">
            <xsl:value-of select ="count( //capFloorStream/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/step )"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select ="count( //swapStream[1]/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/step )"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>
    </xsl:variable>

    <!-- It returns 1 if there are scheduled nominal in the current stream -->
    <!-- when notional amount is different for all streams (pIsNotionalDifferent = true)it scans all streams
			   when notional amount is the same for all streams (pIsNotionalDifferent = false)it scans the first stream-->
    <xsl:variable name="varIsScheduledNominalCurrentStream">
      <xsl:if test ="$pIsNotionalDifferent = 'true'">
        <xsl:value-of select ="count( calculationPeriodAmount[($varNotionalStepParametersCount = 1) or ($varNotionalStepCount  &gt;= 1)] )"/>
      </xsl:if>
      <xsl:if test ="$pIsNotionalDifferent != 'true'">
        <xsl:choose>
          <xsl:when test ="//dataDocument/trade/capFloor/capFloorStream/node()">
            <xsl:value-of select ="count( //capFloorStream/calculationPeriodAmount[($varNotionalStepParametersCount = 1) or ($varNotionalStepCount  &gt;= 1)] )"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select ="count( //swapStream[1]/calculationPeriodAmount[($varNotionalStepParametersCount = 1) or ($varNotionalStepCount  &gt;= 1)] )"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>
    </xsl:variable>

    <!-- varScheduledNominalPrecedingStreamNumber-->
    <!-- It returns how many preceding streams contain scheduled nominal -->
    <xsl:variable name ="varScheduledNominalPrecedingStreamNumber" select="count(preceding-sibling::*[(count( calculationPeriodAmount/calculation/notionalSchedule/notionalStepParameters ) = 1)   or  (count( calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/step ) &gt;= 1) ])"/>

    <!-- It returns sheduled nominal stream position -->
    <xsl:value-of select="$varIsScheduledNominalCurrentStream + $varScheduledNominalPrecedingStreamNumber"/>

  </xsl:template>


  <!-- getLetterFromNumber template -->
  <!-- it converts digits to letters (1 to 9) -> (A to I)	-->
  <xsl:template name ="getLetterFromNumber">
    <xsl:param name="pNumber"/>

    <!-- varAnnexNumbers -->
    <xsl:variable name="varNumbers">123456789</xsl:variable>

    <!-- varAnnexLetters -->
    <xsl:variable name="varLetters">ABCDEFGHI</xsl:variable>

    <!-- it returns A if pNumber = 1 / b if pNumber = 2]-->
    <xsl:value-of select="translate($pNumber, $varNumbers, $varLetters)" />

  </xsl:template>

  <!-- 
	=================================================================
	== Annex: END REGION  
	==================================================================
	-->

  <!-- 
	=================================================================
	== Settlement Terms: BEGIN REGION  
	==================================================================
	-->
  <!-- Display the right text about the cash settlement Valuation Date -->
  <xsl:variable name="getCashSettlementValuationDate">
    <xsl:call-template name="getAbsoluteNumericValue">
      <xsl:with-param name ="pNumericValue" select="//cashSettlement/cashSettlementValuationDate/periodMultiplier"/>
    </xsl:call-template>
    <xsl:text>&#160;</xsl:text>
    <xsl:value-of select="//cashSettlement/cashSettlementValuationDate/dayType"/>
    <xsl:text>&#160;</xsl:text>
    <xsl:call-template name="getTextPeriodByPeriodMultiplier" >
      <xsl:with-param name="pPeriodFrequency" select="//cashSettlement/cashSettlementValuationDate"/>
    </xsl:call-template>
    <xsl:text>&#160;</xsl:text>
    <xsl:if test="//cashSettlement/cashSettlementValuationDate/periodMultiplier &lt; 0">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'priorThe'"/>
      </xsl:call-template>
    </xsl:if>
    <xsl:if test="//cashSettlement/cashSettlementValuationDate/periodMultiplier &gt; 1">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'afterThe'"/>
      </xsl:call-template>
    </xsl:if>
    <xsl:call-template name="getTranslation">
      <xsl:with-param name="pResourceName" select="'ExerciseDate'"/>
    </xsl:call-template>
  </xsl:variable>

  <!-- Display the right text about the cash settlement payment date: if it is an adjustableDates or a relativeDate -->
  <xsl:variable name="getCashSettlementAdjustableOrRelativeDates">
    <xsl:if test="//cashSettlement/cashSettlementPaymentDate/adjustableDates/node()=true()">
      <xsl:call-template name="getAdjustableOrRelativeDate">
        <xsl:with-param name="pDate" select="//cashSettlement/cashSettlementPaymentDate"/>
      </xsl:call-template>
    </xsl:if>

    <xsl:if test="//cashSettlement/cashSettlementPaymentDate/relativeDate/node()=true()">
      <xsl:call-template name="getAbsoluteNumericValue">
        <xsl:with-param name ="pNumericValue" select="//cashSettlement/cashSettlementPaymentDate/relativeDate/periodMultiplier"/>
      </xsl:call-template>
      <xsl:text>&#160;</xsl:text>
      <xsl:value-of select="//cashSettlement/cashSettlementPaymentDate/relativeDate/dayType"/>
      <xsl:call-template name="getTextPeriodByPeriodMultiplier" >
        <xsl:with-param name="pPeriodFrequency" select="//cashSettlement/cashSettlementPaymentDate/relativeDate"/>
      </xsl:call-template>
      <xsl:text>&#160;</xsl:text>
      <xsl:if test="//cashSettlement/cashSettlementPaymentDate/relativeDate/periodMultiplier &lt; 0">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'priorThe'"/>
        </xsl:call-template>
      </xsl:if>
      <xsl:if test="//cashSettlement/cashSettlementPaymentDate/relativeDate/periodMultiplier &gt; 1">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'afterThe'"/>
        </xsl:call-template>
      </xsl:if>
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'ExerciseDate'"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:variable>

  <!-- Display the right text about the cash settlement payment date Business Date Convention -->
  <xsl:variable name="getCashSettlementPaymentDateBusinessDayConvention">
    <xsl:if test="//cashSettlement/cashSettlementPaymentDate/relativeDate/node()=true()">
      <xsl:value-of select="//cashSettlementPaymentDate/relativeDate/businessDayConvention"/>
    </xsl:if>
    <xsl:if test="//cashSettlement/cashSettlementPaymentDate/adjustableDates/node()=true()">
      <xsl:value-of select="//cashSettlementPaymentDate/adjustableDates//dateAdjustments/businessDayConvention"/>
    </xsl:if>
  </xsl:variable>


  <!-- display the right text about the cash settlement method if the xml node exist-->
  <xsl:variable name="getCashSettlementMethod">
    <xsl:if test="//cashPriceMethod/node()=true()">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'CashPrice'"/>
      </xsl:call-template>
    </xsl:if>
    <xsl:if test="//parYieldCurveUnadjustedMethod/node()=true()">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'ParYieldCurveUnadjusted'"/>
      </xsl:call-template>
    </xsl:if>
    <xsl:if test="//zeroCouponYiealdAdjustedMethod/node()=true()">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'ZeroCouponCurveAdjusted'"/>
      </xsl:call-template>
    </xsl:if>
    <xsl:if test="//cashPriceAlternateMethod/node()=true()">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'CashPriceAlternatedMethod'"/>
      </xsl:call-template>
    </xsl:if>
    <xsl:if test="//parYieldCurveAdjustedMethod/node()=true()">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'ParYieldCurveAdjusted'"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:variable>
  <!-- 
	=================================================================
	== Settlement Terms: END REGION  
	==================================================================
	-->

  <!-- 
	===============================================
	===============================================
	== IRD products: END REGION     ===============
	===============================================
	===============================================





	===============================================
	===============================================
	== FX product's family: BEGIN REGION     =====
	===============================================
	===============================================
	-->

  <!-- 
	=================================================================
	== Exchange Rate Region
	== For FxSingle and FxSwap instrument
	== Spot rate: available exchange rate 
	== SideRate: end rate (forward rate)	
	==================================================================
	-->

  <xsl:variable name="varIsFxSwap" select="boolean(//fxSwap/node()=true())"/>

  <!-- template GetSpotRate -->
  <xsl:template name="GetSpotRate">
    <xsl:param name="pFxSingleLeg"/>
    <!--<xsl:value-of select="$pFxSingleLeg/exchangeRate/spotRate" />-->
    <!--PL 20150324 format-number-->
    <xsl:value-of select="format-number($pFxSingleLeg/exchangeRate/spotRate, $ratePattern, $defaultCulture)"/>
  </xsl:template>

  <!-- template GetExchangeRate -->
  <xsl:template name="GetExchangeRate">
    <xsl:param name="pFxSingleLeg"/>
    <!--<xsl:value-of select="$pFxSingleLeg/exchangeRate/rate" />-->
    <!--PL 20150324 format-number-->
    <xsl:value-of select="format-number($pFxSingleLeg/exchangeRate/rate, $ratePattern, $defaultCulture)"/>
  </xsl:template>
  <!--
	=================================================
	== FxFixingDate
	=================================================
	-->
  <!-- template GetFxFixingDate -->
  <xsl:template name="GetFxFixingDate">
    <xsl:param name="pFxSingleLeg"/>
    <xsl:call-template name="format-date">
      <xsl:with-param name="xsd-date-time" select="$pFxSingleLeg/nonDeliverableForward/fixing/fixingDate"/>
    </xsl:call-template>
  </xsl:template>

  <!-- 
	=================================================================
	== Reference Currency: BEGIN REGION  
	==================================================================
	-->
  <!-- template GetReferenceCurrency -->
  <xsl:template name="getReferenceCurrency">
    <xsl:param name="pFxSingleLeg"/>
    <xsl:variable name="currency1" select="$pFxSingleLeg/exchangeRate/quotedCurrencyPair/currency1" />
    <xsl:variable name="currency2" select="$pFxSingleLeg/exchangeRate/quotedCurrencyPair/currency2" />
    <xsl:variable name="settlementCurrency" select="$pFxSingleLeg/nonDeliverableForward/settlementCurrency" />

    <xsl:choose>
      <xsl:when test="$settlementCurrency=$currency1">
        <xsl:value-of select="$currency2" />
      </xsl:when>
      <xsl:when test="$settlementCurrency=$currency2">
        <xsl:value-of select="$currency1" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:for-each select="$pFxSingleLeg/nonDeliverableForward/fixing">
          <xsl:variable name="pos" select="position()" />
          <xsl:variable name="curr" select="current()" />
          <xsl:choose>
            <xsl:when test="$settlementCurrency=$curr/quotedCurrencyPair/currency1">
              <xsl:value-of select="$curr/quotedCurrencyPair/currency2" />
            </xsl:when>
            <xsl:when test="$settlementCurrency=$curr/quotedCurrencyPair/currency2">
              <xsl:value-of select="$curr/quotedCurrencyPair/currency1" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="'***'" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:for-each>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- 
	=================================================================
	== Reference Currency: END REGION  
	==================================================================
	-->
  <!-- 
	=================================================================
	== Trigger: BEGIN REGION  
	==================================================================
	-->
  <!-- template getTrigger -->
  <xsl:template name="getTrigger">
    <xsl:param name="pFxOptionTrigger"/>
    <xsl:call-template name="format-fxrate">
      <xsl:with-param name="fxrate" select="$pFxOptionTrigger/triggerRate" />
    </xsl:call-template>
    <xsl:text>&#160;</xsl:text>
    <xsl:call-template name="format-currency-pair">
      <xsl:with-param name="quotedCurrencyPair" select="$pFxOptionTrigger/quotedCurrencyPair"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="getSource">
    <xsl:param name="pFxOptionTrigger"/>
    <xsl:call-template name="getTranslation">
      <xsl:with-param name="pResourceName" select="'Source'"/>
    </xsl:call-template>
    <xsl:text> : </xsl:text>
    <xsl:value-of select="$pFxOptionTrigger/informationSource/rateSource" />
  </xsl:template>

  <xsl:template name="getPage">
    <xsl:param name="pFxOptionTrigger"/>
    <xsl:call-template name="getTranslation">
      <xsl:with-param name="pResourceName" select="'Page'"/>
    </xsl:call-template>
    <xsl:text> : </xsl:text>
    <xsl:value-of select="$pFxOptionTrigger/informationSource/rateSourcePage" />
  </xsl:template>

  <xsl:template name="getHeading">
    <xsl:param name="pFxOptionTrigger"/>
    <xsl:call-template name="getTranslation">
      <xsl:with-param name="pResourceName" select="'Heading'"/>
    </xsl:call-template>
    <xsl:text> : </xsl:text>
    <xsl:value-of select="$pFxOptionTrigger/informationSource/rateSourcePageHeading" />
  </xsl:template>

  <!-- 
	=================================================================
	== Trigger: END REGION  
	==================================================================
	-->
  <!-- 
	===========================================================================================
	== Barrier Event Type: BEGIN REGION
	== Valeurs d'affichage Possibles:Knock-In, Knock-Out, No-Touch Binary, One-Touch Binary
	===========================================================================================
	-->
  <!-- template getBarrierEventType -->
  <xsl:template name="getBarrierEventType">
    <xsl:param name="pEventType" />
    <xsl:param name="pTotalBarrier" />

    <!-- On considère que l'evènement est double s'il existe plusieurs niveau de Barrière -->

    <!-- Barrier number > 1 (2 or plus barriers)-->
    <xsl:if test="$pTotalBarrier &gt; 1">
      <xsl:choose>
        <xsl:when test="$pEventType = 'Knockout'">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'DoubleKnock-Out'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pEventType = 'Knockin'">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'DoubleKnock-In'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pEventType = 'ReverseKnockout'">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'DoubleKnock-Out'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pEventType = 'ReverseKnockin'">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'DoubleKnock-In'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pEventType = 'NoTouchBinary'">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'DoubleNo-TouchBinary'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pEventType = 'OneTouchBinary'">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'DoubleOne-TouchBinary'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'Error'"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>

    <!-- Barrier number < 2  (one barrier)-->
    <xsl:if test="$pTotalBarrier &lt; 2">
      <xsl:choose>
        <xsl:when test="$pEventType = 'Knockout'">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'Knock-Out'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pEventType = 'Knockin'">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'Knock-In'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pEventType = 'ReverseKnockout'">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'Knock-Out'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pEventType = 'ReverseKnockin'">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'Knock-In'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pEventType = 'NoTouchBinary'">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'No-TouchBinary'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pEventType = 'OneTouchBinary'">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'One-TouchBinary'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'Error'"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>

  </xsl:template>
  <!-- 
	=================================================================
	== Barrier Event Type: END REGION  
	==================================================================
	-->
  <!-- 
	===========================================================================================
	== Trigger Event Type: BEGIN REGION
	== Touch - NoTouch - Double Touch - Double NoTouch
	== When the trade contains one trigger (One Touch or No Touch)
    == When the trade contains two triggers (Double One Touch or Double No Touch)
	== <xsl:with-param name="pTotalTrigger" select="count($pFxOption/fxAmericanTrigger)" />
	== old name getTouchCondition
	===========================================================================================
	-->
  <xsl:template name="getTriggerEventType">
    <xsl:param name="pTouchCondition" />
    <xsl:param name="pTotalTrigger" />

    <xsl:if test="$pTotalTrigger &gt; 1">
      <xsl:choose>
        <xsl:when test="$pTouchCondition = 'Touch'">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'DoubleOne-Touch'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pTouchCondition = 'Notouch'">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'DoubleNo-Touch'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$pTouchCondition"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>

    <xsl:if test="$pTotalTrigger &lt; 2">
      <xsl:choose>
        <xsl:when test="$pTouchCondition = 'Touch'">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'One-Touch'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pTouchCondition = 'Notouch'">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'No-Touch'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$pTouchCondition"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
  </xsl:template>

  <!-- 
	=================================================================
	== Barrier Spot Exchange Rate Direction: BEGIN REGION 
    == ps: On affiche l'indicateur de message                                   
	== Greater than or equal / Less than or equal to the Barrier Level ) que si
	== on est en présence d'une trade n'ayant qu'une seule Barrière             
	==================================================================
	-->
  <xsl:template name="getBarrierSpotExchangeRateDirection">
    <xsl:param name="pEventType" />

    <xsl:if test="contains( $pEventType,'Reverse')">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'GreaterThanOrEqualToTheBarrierLevel'"/>
      </xsl:call-template>
    </xsl:if>

    <xsl:if test="not( contains( $pEventType,'Reverse' ) )">
      <xsl:call-template name="getTranslation">
        <xsl:with-param name="pResourceName" select="'LessThanOrEqualToTheBarrierLevel'"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
  <!-- 
	=================================================================
	== Barrier Spot Exchange Rate Direction: END REGION  
	==================================================================
	-->
  <!-- 
	===============================================
	===============================================
	== FX products: END REGION     ===============
	===============================================
	===============================================

	===============================================
	===============================================
	== EQD product's family: BEGIN REGION     =====
	===============================================
	===============================================
	-->
  <!-- GS 20100106: Add cash settlement payment dates for equity Option -->
  <!-- To Do: test adjustable or relative date -->
  <!-- To Do: cash settlement payment dates is always relative to Exercise Date? -->

  <xsl:variable name="getEqdCashSettlementPaymentDate">

    <xsl:variable name ="varSettementDateRelativeTo">
      <xsl:value-of select="//equityExercise/settlementDate/relativeDate/dateRelativeTo/@href"/>
    </xsl:variable>

    <xsl:if test="//equityExercise/settlementDate/adjustableDates/node()=true()">
      <xsl:call-template name="getAdjustableOrRelativeDate">
        <xsl:with-param name="pDate" select="//equityExercise/settlementDate"/>
      </xsl:call-template>
    </xsl:if>

    <xsl:if test="//equityExercise/settlementDate/relativeDate/node()=true()">
      <xsl:call-template name="getAbsoluteNumericValue">
        <xsl:with-param name ="pNumericValue" select="//equityExercise/settlementDate/relativeDate/periodMultiplier"/>
      </xsl:call-template>
      <xsl:text>&#160;</xsl:text>
      <xsl:value-of select="//equityExercise/settlementDate/relativeDate/dayType"/>
      <xsl:call-template name="getTextPeriodByPeriodMultiplier" >
        <xsl:with-param name="pPeriodFrequency" select="//equityExercise/settlementDate/relativeDate"/>
      </xsl:call-template>
      <xsl:if test="//equityExercise/settlementDate/relativeDate/periodMultiplier &lt; 0">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'priorThe'"/>
        </xsl:call-template>
      </xsl:if>
      <xsl:if test="//equityExercise/settlementDate/relativeDate/periodMultiplier &gt; 1">
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'afterThe'"/>
        </xsl:call-template>
      </xsl:if>

      <xsl:choose>
        <!-- EG 20160405 Migration vs2013 -->
        <xsl:when test="$varSettementDateRelativeTo = 'expirationDate'">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'expirationDate_'"/>
          </xsl:call-template>
        </xsl:when>
        <!-- EG 20160405 Migration vs2013 -->
        <xsl:when test="$varSettementDateRelativeTo = 'exerciseDate'">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'exerciseDate_'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$varSettementDateRelativeTo = 'valuationDate'">
          <xsl:call-template name="getTranslation">
            <xsl:with-param name="pResourceName" select="'valuationDate'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="$varSettementDateRelativeTo"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
  </xsl:variable>


  <!-- 
	==========================================================================================================================
	Get Equity Strike
	get the strike price for an equity: when the currency is declare use the currency in the tag <strike><currency> 
	when the currency isn't declare use the underlyer currency 
	===========================================================================================================================
	-->
  <xsl:template name="getEquityStrike">
    <xsl:choose>
      <xsl:when test="//dataDocument/trade/equityOption/strike/currency">
        <xsl:call-template name="format-money2">
          <xsl:with-param name="currency" select="//dataDocument/trade/equityOption/strike/currency"/>
          <xsl:with-param name="amount" select="//dataDocument/trade/equityOption/strike/strikePrice"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="//underlyer/singleUnderlyer/index/currency">
        <xsl:call-template name="format-money2">
          <xsl:with-param name="currency" select="//underlyer/singleUnderlyer/index/currency"/>
          <xsl:with-param name="amount" select="//dataDocument/trade/equityOption/strike/strikePrice"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="//underlyer/singleUnderlyer/equity/currency">
        <xsl:call-template name="format-money2">
          <xsl:with-param name="currency" select="//underlyer/singleUnderlyer/equity/currency"/>
          <xsl:with-param name="amount" select="//dataDocument/trade/equityOption/strike/strikePrice"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="format-money2">
          <xsl:with-param name="amount" select="//dataDocument/trade/equityOption/strike/strikePrice"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Get Premium Payment Date -->
  <xsl:template name="getEqdPremiumPaymentDate">
    <xsl:if test="paymentDate">
      <xsl:call-template name="format-date">
        <xsl:with-param name="xsd-date-time" select="//equityPremium/paymentDate/unadjustedDate"/>
      </xsl:call-template>
    </xsl:if>
    <xsl:if test="adjustedPaymentDate">
      <xsl:call-template name="format-date">
        <xsl:with-param name="xsd-date-time" select="//equityPremium/adjustedPaymentDate"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <!-- 
	===============================================
	===============================================
	== EQD products: END REGION     ===============
	===============================================
	===============================================
	-->

</xsl:stylesheet>