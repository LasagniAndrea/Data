<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
				        xmlns:dt="http://xsltsl.org/date-time"
				        xmlns:msxsl="urn:schemas-microsoft-com:xslt">

  <xsl:param name="pPartyIDFrom"/>
  <xsl:param name="pPartyIDTo"/>
  <xsl:param name="pSendersLTAddress"/>
  <xsl:param name="pReceiversLTAddress"/>
  <xsl:param name="pSwiftCommonReferenceBankLeft"/>
  <xsl:param name="pSwiftCommonReferenceBankRight"/>
  <xsl:param name="pMasterAgreementAgreementType"/>


  <xsl:include href="../Lib_SWIFT.xslt"/>

  <!-- ******************* -->
  <!-- MAIN                -->
  <!-- ******************* -->
  <xsl:template match="/">
    <xsl:call-template name="Swift-BasicHeader">
      <xsl:with-param name="pSendersLTAddress" select="$pSendersLTAddress" />
    </xsl:call-template>

    <xsl:call-template name="Swift-ApplicationHeader">
      <xsl:with-param name="pReceiversLTAddress" select="$pReceiversLTAddress" />
      <xsl:with-param name="pMessageType" select="'360'" />
    </xsl:call-template>

    <xsl:call-template name="MT360Fields">
      <xsl:with-param name="pPartyIDFrom" select="$pPartyIDFrom"/>
      <xsl:with-param name="pPartyIDTo" select="$pPartyIDTo"/>
    </xsl:call-template>
  </xsl:template>


  <!-- ******************* -->
  <!-- Message SWIFT MT360 -->
  <!-- ******************* -->
  <xsl:template name="MT360Fields">

    <xsl:param name="pPartyIDFrom"/>
    <xsl:param name="pPartyIDTo"/>

    <!-- *************************************************************************** -->
    <!-- Récupération d'un noeud dynamiquement suivant si on est dans un SWAP ou pas -->
    <!-- Si c'est un SWAP on obtient le noeud: //dataDocument/trade/swap/swapStream[0]/*		     -->
    <!-- Sinon            on obtient le noeud: //dataDocument/trade/capFloor/capFloorStream/*     -->
    <!-- *************************************************************************** -->
    <xsl:variable name="Var_Stream">
      <xsl:call-template name="Swift-GetStream"/>
    </xsl:variable>

    <xsl:text>&#xA;</xsl:text>
    <xsl:text>{4:</xsl:text>

    <!-- Mandatory Sequence A General Information -->
    <!--:15A: New Sequence-->
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>:15A:</xsl:text>

    <!--:20: Sender's Reference-->
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>:20:</xsl:text>
    <xsl:call-template name="Swift-GetReferenceNumber">
      <xsl:with-param name="pPartyID" select="$pPartyIDFrom"/>
    </xsl:call-template>

    <!--:22A: Type of Operation-->
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>:22A:</xsl:text>
    <xsl:text>NEWT</xsl:text>

    <!--:22C: Common Reference-->
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>:22C:</xsl:text>
    <xsl:call-template name="Swift-CommonReference">
      <xsl:with-param name="pBicCodeBank1" select="$pSwiftCommonReferenceBankLeft"/>
      <xsl:with-param name="pBicCodeBank2" select="$pSwiftCommonReferenceBankRight"/>
      <xsl:with-param name="pSeparatorCode" select="msxsl:node-set($Var_Stream)/calculationPeriodDates/terminationDate/unadjustedDate"/>
      <xsl:with-param name="pCommonReferenceType" select="'DATE'"/>
    </xsl:call-template>

    <!-- :23A: Identification of the the Swap -->
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>:23A:</xsl:text>

    <!-- ************** -->
    <!-- is SWAP        -->
    <!-- ************** -->
    <xsl:if test="$isSwap">

      <xsl:variable name="Var_isPartyAFloatFixed">
        <xsl:call-template name="Swift-GetSwapFloatFixed"/>
      </xsl:variable>

      <xsl:if test="contains( $Var_isPartyAFloatFixed,'Fixed') and not( contains( $Var_isPartyAFloatFixed,'Float') )">
        <xsl:text>FIXEDFIXED</xsl:text>
      </xsl:if>
      <xsl:if test="contains( $Var_isPartyAFloatFixed,'Float') and not( contains( $Var_isPartyAFloatFixed,'Fixed') )">
        <xsl:text>FLOATFLOAT</xsl:text>
      </xsl:if>

      <xsl:if test="contains( $Var_isPartyAFloatFixed,'PartyAPayerFixed') and contains( $Var_isPartyAFloatFixed,'PartyAReceiverFloat' )">
        <xsl:text>FIXEDFLOAD</xsl:text>
      </xsl:if>

      <xsl:if test="contains( $Var_isPartyAFloatFixed,'PartyAPayerFloat') and contains( $Var_isPartyAFloatFixed,'PartyAReceiverFixed' )">
        <xsl:text>FLOATFIXED</xsl:text>
      </xsl:if>

    </xsl:if>

    <!-- ************************* -->
    <!-- is CAP or FLOOR or COLLAR -->
    <!-- ************************* -->
    <xsl:if test="$isCap or $isFloor or $isCollar">
      <xsl:call-template name="Swift-GetCapFloorIdentification"/>
    </xsl:if>


    <!--:21N: Contract Number Party A-->
    <!--:21B: Contract Number Party B-->

    <!--:30T: Trade Date-->
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>:30T:</xsl:text>
    <!--Formatage de la date yyyy-mm-jj se transforme en yyyymmjj-->
    <xsl:call-template name="Swift-Date">
      <xsl:with-param name="pDate" select="//dataDocument/trade/tradeHeader/tradeDate"/>
      <xsl:with-param name="pDatePatternIn" select="'yyyy-mm-jj'"/>
      <xsl:with-param name="pDatePatternOut" select="'yyyymmjj'"/>
    </xsl:call-template>

    <!--:30V: Effective Date-->
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>:30V:</xsl:text>
    <!--Formatage de la date yyyy-mm-jj se transforme en yyyymmjj-->
    <xsl:call-template name="Swift-Date">
      <xsl:with-param name="pDate" select="msxsl:node-set($Var_Stream)/calculationPeriodDates/effectiveDate/unadjustedDate"/>
      <xsl:with-param name="pDatePatternIn" select="'yyyy-mm-jj'"/>
      <xsl:with-param name="pDatePatternOut" select="'yyyymmjj'"/>
    </xsl:call-template>

    <!--:30P: Termination Date-->
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>:30P:</xsl:text>
    <!--Formatage de la date yyyy-mm-jj se transforme en yyyymmjj-->
    <xsl:call-template name="Swift-Date">
      <xsl:with-param name="pDate" select="msxsl:node-set($Var_Stream)/calculationPeriodDates/terminationDate/unadjustedDate"/>
      <xsl:with-param name="pDatePatternIn" select="'yyyy-mm-jj'"/>
      <xsl:with-param name="pDatePatternOut" select="'yyyymmjj'"/>
    </xsl:call-template>

    <!--:32B: Currency, Notional Amount-->
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>:32B:</xsl:text>
    <xsl:value-of select="msxsl:node-set($Var_Stream)/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/currency" />
    <xsl:call-template name="Swift-Amount">
      <xsl:with-param name="pSource" select="msxsl:node-set($Var_Stream)/calculationPeriodAmount/calculation/notionalSchedule/notionalStepSchedule/initialValue" />
    </xsl:call-template>

    <!--:82a: Party A-->
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>:82A:</xsl:text>
    <xsl:value-of select="$pPartyIDFrom"/>

    <!--:87a: Party B-->
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>:87A:</xsl:text>
    <xsl:value-of select="$pPartyIDTo"/>

    <!--:77H: Type, Date, Version of the Agreement-->
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>:77H:</xsl:text>
    <xsl:variable name="Var_MasterAgreementType">
      <xsl:call-template name="Swift-GetMasterAgreementType">
        <xsl:with-param name="pMasterAgreementAgreementType" select="$pMasterAgreementAgreementType" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:value-of select="$Var_MasterAgreementType"/>

    <!--:14C: Year of Definitions-->
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>:14C:</xsl:text>
    <xsl:if test="$Var_MasterAgreementType='ISDA'">
      <xsl:value-of select="substring($pMasterAgreementAgreementType,5,4)"/>
    </xsl:if>
    <xsl:if test="$Var_MasterAgreementType!='ISDA'">
      <xsl:text>0000</xsl:text>
    </xsl:if>

    <!-- Mandatory Sequence D Payment Instructions for Interest Payable by Party B -->
    <!--:15D: New Sequence-->
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>:15D:</xsl:text>

    <!--:57a: Receiving Agent-->
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>:57a:</xsl:text>
    <xsl:value-of select="$pPartyIDFrom"/>

    <!-- Mandatory Sequence G Payment Instructions for Interest Payable by Party A -->
    <!--:15G: New Sequence-->
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>:15G:</xsl:text>

    <!--:57a: Receiving Agent-->
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>:57a:</xsl:text>
    <xsl:value-of select="$pPartyIDTo"/>

    <xsl:text>&#xA;</xsl:text>
    <xsl:text>-}</xsl:text>

  </xsl:template>

</xsl:stylesheet>