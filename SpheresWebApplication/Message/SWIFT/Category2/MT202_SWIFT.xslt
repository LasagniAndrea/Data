<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
				        xmlns:dt="http://xsltsl.org/date-time">

  <!-- 20080103 PL Add du paramètre suivant, car attendu par Lib_SWIFT.xslt, et donc pb avec .NET 2.0 -->
  <xsl:param name="pPartyIDFrom"/>

  <xsl:include href="..\Lib_SWIFT.xslt"/>

  <!-- ******************* -->
  <!-- MAIN                -->
  <!-- ******************* -->
  <xsl:template match="/">
    <xsl:call-template name="Swift-BasicHeader">
      <xsl:with-param name="pSendersLTAddress"
				select="//header/sender/routingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/actorLTAdress']"/>
    </xsl:call-template>

    <xsl:call-template name="Swift-ApplicationHeader">
      <xsl:with-param name="pReceiversLTAddress"
			select="//header/receiver/routingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/actorLTAdress']"/>
      <xsl:with-param name="pMessageType" select="'202'" />
    </xsl:call-template>

    <xsl:call-template name="MT202Fields"/>
  </xsl:template>

  <!-- ******************* -->
  <!-- Message SWIFT MT202 -->
  <!-- ******************* -->
  <xsl:template name="MT202Fields">
    <xsl:text>{4:</xsl:text>
    <!--:20:-->
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>:20:</xsl:text>
    <!--<xsl:value-of select="//efs:payment/efs:paymentId[@paymentIdScheme='http://www.euro-finance-systems.fr/otcml/eventid']"/>-->
    <xsl:value-of select="//header/@OTCmlId"/>
    <!--:21:-->
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>:21:</xsl:text>
    <xsl:value-of select="//payment/payer/tradeId[@tradeIdScheme='http://www.euro-finance-systems.fr/otcml/tradeid']"/>
    <!--:32A:-->
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>:32A:</xsl:text>
    <!--transaction valueDate-->
    <xsl:call-template name="Swift-Date">
      <xsl:with-param name="pDate" select="//payment/valueDate"/>
      <xsl:with-param name="pDatePatternIn" select="'yyyy-mm-jj'"/>
      <xsl:with-param name="pDatePatternOut" select="'yymmjj'"/>
    </xsl:call-template>
    <!--transaction currency-->
    <xsl:value-of select="//payment/paymentAmount/currency"/>
    <!--transaction amount-->
    <xsl:call-template name="Swift-Amount">
      <xsl:with-param name="pSource" select="//payment/paymentAmount/amount"/>
    </xsl:call-template>

    <!--:52A:-->
    <xsl:if test="//payment/payer/settlementInstruction/beneficiaryBank/routingIdsAndExplicitDetails/routingName !=  //header/sender/routingIdsAndExplicitDetails/routingName">
      <xsl:call-template name="Swift-DisplayActor">
        <xsl:with-param name="pIdFields" select="52" />
        <xsl:with-param name="pActorNode" select="//payment/payer/settlementInstruction/beneficiaryBank/routingIdsAndExplicitDetails"/>
      </xsl:call-template>
    </xsl:if>

    <!--:56A:-->
    <xsl:if test="count(//payment/receiver/settlementInstruction/intermediaryInformation) > 1">
      <xsl:call-template name="Swift-DisplayActor">
        <xsl:with-param name="pIdFields" select="56" />
        <xsl:with-param name="pActorNode" select="//payment/receiver/settlementInstruction/intermediaryInformation/routingIdsAndExplicitDetails"/>
      </xsl:call-template>
    </xsl:if>

    <!--:57A:-->
    <!--Si l'intermediaire-Receiver est identique à Corespondent-Payer ne pas renseigner le TAG 57-->
    <xsl:if test="//payment/receiver/settlementInstruction/intermediaryInformation/routingIdsAndExplicitDetails/routingName !=  //payment/payer/settlementInstruction/correspondentInformation/routingIdsAndExplicitDetails/routingName">
      <!--S'il existe qu'un seul intermédiaire-->
      <xsl:if test="count(//payment/receiver/settlementInstruction/intermediaryInformation)= 1 ">
        <xsl:call-template name="Swift-DisplayActor">
          <xsl:with-param name="pIdFields" select="57" />
          <xsl:with-param name="pActorNode" select="//payment/receiver/settlementInstruction/intermediaryInformation/routingIdsAndExplicitDetails"/>
        </xsl:call-template>
      </xsl:if>

      <!--S'il existe plusieurs intermédiaires on retiendra l'intermédiaire du receiver de niveau le plus haut-->
      <xsl:if test="count(//payment/receiver/settlementInstruction/intermediaryInformation)> 1">
        <xsl:for-each select="//payment/receiver/settlementInstruction/intermediaryInformation">
          <xsl:sort select="current()/intermediarySequenceNumber" order="descending"/>
          <xsl:if test="position()=1">
            <xsl:call-template name="Swift-DisplayActor">
              <xsl:with-param name="pIdFields" select="57" />
              <xsl:with-param name="pActorNode" select="current()/routingIdsAndExplicitDetails"/>
            </xsl:call-template>
          </xsl:if>
        </xsl:for-each>
      </xsl:if>
    </xsl:if>

    <!--:58A:-->
    <xsl:call-template name="Swift-DisplayActor">
      <xsl:with-param name="pIdFields" select="58" />
      <xsl:with-param name="pActorNode" select="//payment/receiver/settlementInstruction/beneficiary/routingIdsAndExplicitDetails"/>
    </xsl:call-template>
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>-}</xsl:text>

    <!-- PL 20161115 RATP -->
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-</xsl:text>
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>Ebauche Message AFB160</xsl:text>
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-</xsl:text>
    <!-- - - - - - - - - - - - - - - - - - - - -->
    <!-- RECORD 03 - Enregistrement "émetteur" -->
    <!-- - - - - - - - - - - - - - - - - - - - -->
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>03</xsl:text>
    <!-- Cope opération -->
    <xsl:text>02</xsl:text>
    <xsl:text>00000000</xsl:text>
    <!-- Numéro d'émetteur ou d'identification pour le virement particulier code 22 -->
    <xsl:text>______</xsl:text>
    <!-- Code CCD -->
    <xsl:text>0</xsl:text>
    <xsl:text>000000</xsl:text>
    <!-- Date (JJMMA) -->
    <xsl:value-of select="substring( //payment/valueDate, 9, 2 )"/>
    <xsl:value-of select="substring( //payment/valueDate, 6, 2 )"/>
    <xsl:value-of select="substring( //payment/valueDate, 4, 1 )"/>
    <!-- Nom/Raison sociale du donneur d'ordre -->
    <!--<xsl:value-of select="substring( concat(//payment/payer/settlementInstruction/beneficiary/routingIdsAndExplicitDetails/routingName, '________________'), 1, 16 )"/>-->
    <xsl:value-of select="substring( concat(//payment/payer/settlementInstruction/beneficiary/routingIdsAndExplicitDetails/routingIds/routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/actorDescription'], '________________________'), 1, 24 )"/>
    <!-- Référence de la remise (à blanc ou a zéro si non utilisée) -->
    <xsl:text>0000000</xsl:text>
    <xsl:text>00000000000000000</xsl:text>
    <xsl:text>00</xsl:text>
    <!-- Code monnaie -->
    <xsl:text>E</xsl:text>
    <xsl:text>00000</xsl:text>
    <!-- Code guichet de la banque du donneur d'ordre -->
    <xsl:value-of select="substring( //payment/payer/settlementInstruction/beneficiaryBank/routingIdsAndExplicitDetails/routingAccountNumber, 6, 5 )"/>
    <!-- Numéro de compte du donneur d'ordre -->
    <xsl:value-of select="substring( //payment/payer/settlementInstruction/beneficiaryBank/routingIdsAndExplicitDetails/routingAccountNumber, 11, 11 )"/>
    <!-- Identifiant du donneur d'ordre -->
    <xsl:value-of select="substring( concat(//payment/payer/settlementInstruction/beneficiaryBank/routingIdsAndExplicitDetails/routingReferenceText, '________________'), 1, 16 )"/>
    <xsl:text>0000000000000000000000000000000</xsl:text>
    <!-- Code établissement de la banque du donneur d'ordre -->
    <xsl:value-of select="substring( //payment/payer/settlementInstruction/beneficiaryBank/routingIdsAndExplicitDetails/routingAccountNumber, 1, 5 )"/>
    <xsl:text>000000</xsl:text>
    <!-- - - - - - - - - - - - - - - - - - - - - -  -->
    <!-- RECORD 06 - Enregistrement "destinataire"  -->
    <!-- - - - - - - - - - - - - - - - - - - - - -  -->
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>06</xsl:text>
    <!-- Cope opération -->
    <xsl:text>02</xsl:text>
    <xsl:text>00000000</xsl:text>
    <!-- Numéro d'émetteur ou d'identification pour le virement particulier code 22 -->
    <xsl:text>______</xsl:text>
    <!-- - - - - - - - - - - - - - - - - - - -->
    <!-- RECORD 08 - Enregistrement "total"  -->
    <!-- - - - - - - - - - - - - - - - - - - -->
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>08</xsl:text>
    <!-- Cope opération -->
    <xsl:text>02</xsl:text>
    <xsl:text>00000000</xsl:text>
    <!-- Numéro d'émetteur ou d'identification pour le virement particulier code 22 -->
    <xsl:text>______</xsl:text>
    <xsl:text>000000000000</xsl:text>
    <xsl:text>000000000000000000000000</xsl:text>
    <xsl:text>000000000000000000000000</xsl:text>
    <xsl:text>00000000</xsl:text>
    <xsl:text>00000</xsl:text>
    <xsl:text>00000000000</xsl:text>
    <!-- Montant de la remise -->
    <xsl:value-of select="format-number(//payment/paymentAmount/amount, '0000000000000.00')"/>
    <xsl:text>0000000000000000000000000000000</xsl:text>
    <xsl:text>00000</xsl:text>
    <xsl:text>000000</xsl:text>
    <xsl:text>&#xA;</xsl:text>
    <xsl:text>+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-</xsl:text>
  </xsl:template>
</xsl:stylesheet>

