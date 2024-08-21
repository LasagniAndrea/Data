<?xml version="1.0" encoding="utf-8"?>
<!--
FI 20240208 [WI856] Add TVTIC
FI 20220914 [XXXXX] Adaptation au format d'import V3 (dispo avec Spheres V12)
FI 20190425 [24801] SpheresIdentifier for FACI
PM 20171026 [23566] Adaptation au format d'import V2 et Spheres V7
FI 20170824 [23339] Mod Trader => Trader se trouve sous BSTR ou BSNBTR
FI 20170512 [23165] Trade aggregation, Reverse trade aggregation, Reverse trade split
FI 20170510 [23039] Mod Trader => soit il se trouve sous BSTR ou BSNBTR ou BSEBTR
PL 20170504 [23121] Add [1] on following values: marketid, abicode, contractdate, accounttype, side
FI 20170428 [23039] Add Trader (BSTR and BSTRC)
RD 20170320 [22761] Exclure les messages NotifySubContracts avec "ContractState = R"
RD 20170110 [22745] Gestion des Extends
RD 20161020 [22553] Gestion du timestamping pour les trades issus des flux 'NotifyContractTransfers/NotifySubContractTransfers'
PM 20161014 [22402] Ajout vérification du marché
PM 20160818 [22402] Gestion des flux NotifySubContracts et NotifySubContractTransfers
FI 20140106 [19445] utilisation de transferdate
FI 20130729 [18841] Dans le cas des Give-up le book côté clearing est suffixé par '.GU'  
BD 20130123 [18250] Create File
-->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>
  <!-- ================================================== -->
  <!--        Noeud Root                                  -->
  <!-- ================================================== -->
  <xsl:template match="/">
    <file>
      <xsl:attribute name="name"/>
      <xsl:attribute name="folder"/>
      <xsl:attribute name="date"/>
      <xsl:attribute name="size"/>
      <xsl:attribute name="status">success</xsl:attribute>
      <xsl:variable name="idprefix" select="'r'"/>
      <xsl:variable name="idposition" select="position()"/>

      <xsl:choose>
        <!-- 2 à n <BCSMessage>, on passe par <BCSMessageList>  -->
        <xsl:when test="BCSMessageList">
          <xsl:apply-templates select="BCSMessageList"/>
        </xsl:when>
        <!-- 1 seul <BCSMessage> -->
        <xsl:otherwise>
          <xsl:apply-templates select="BCSMessage"/>
        </xsl:otherwise>
      </xsl:choose>
    </file>
  </xsl:template>


  <!-- ================================================== -->
  <!--        <BCSMessageList>                            -->
  <!-- ================================================== -->
  <!-- On match d'abord avec <BCSMessageList> -->
  <xsl:template match="BCSMessageList">
    <xsl:apply-templates select="BCSMessage"/>
  </xsl:template>


  <!-- ================================================== -->
  <!--        <BCSMessage>                                -->
  <!-- ================================================== -->
  <xsl:template match="BCSMessage">

    <!-- Variables -->
    <!-- <messageclass> -->
    <xsl:variable name="vMessageclass">
      <xsl:value-of select="messageclass"/>
    </xsl:variable>
    <!-- contractnumber -->
    <xsl:variable name="vContractnumber">
      <xsl:value-of select="datafields/data[@name='contractnumber']"/>
    </xsl:variable>
    <!-- origcontractnumber (Existe uniquement sur NotifyContractTransfers ou NotifySubContractTransfers, NotifySubContracts)  -->
    <xsl:variable name="vOrigcontractnumber">
      <xsl:value-of select="datafields/data[@name='origcontractnumber']"/>
    </xsl:variable>
    <!-- transfertype (existe uniquement sur NotifyContractTransfers, NotifySubContractTransfers) -->
    <xsl:variable name="vTransfertype">
      <xsl:value-of select="datafields/data[@name='transfertype']"/>
    </xsl:variable>
    <!-- isNotifyContractsOk -->
    <!-- Pour savoir si le trade doit être importé ou non -->
    <xsl:variable name="isNotifyContractsOk">
      <xsl:choose>
        <!-- NotifyContracts ou NotifyContractsByTime -->
        <!-- PM 20160818 [22402] Ajout NotifySubContracts -->
        <!-- RD 20170320 [22761] Exclure les messages NotifySubContracts avec "ContractState = R"-->
        <!-- FI 20170512 [23165] Considération des messages NotifySubContracts avec "ContractState = R" sauf si Reversing de type give-up -->
        <xsl:when test="($vMessageclass='NotifyContracts') or ($vMessageclass='NotifyContractsByTime')  or ($vMessageclass='NotifySubContracts')">
          <!-- FI 20170824 [23339] Les NotifySubContracts de type Reverse suite à un give-up sont désormais considéré (pour Mise à jour du trader en cas de give-up automatique) -->
          <!--<xsl:choose>
            <xsl:when test=" $vMessageclass='NotifySubContracts' and datafields/data[@name='contractstate']='R' and datafields/data[@name='repoindex']='G'">false</xsl:when>
            <xsl:otherwise>true</xsl:otherwise>
          </xsl:choose>-->
          <xsl:value-of select="'true'"/>
        </xsl:when>
        <!-- NotifyContractTransfers et transferstate=P (processed) -->
        <!-- PM 20160818 [22402] Ajout NotifySubContractTransfers -->
        <xsl:when test="($vMessageclass='NotifyContractTransfers') or ($vMessageclass='NotifySubContractTransfers') and datafields/data[@name='transferstate']='P'">true</xsl:when>
        <xsl:otherwise>false</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- isMarketOk -->
    <!-- Pour savoir si le trade doit être importé ou non -->
    <!-- PM 20161014 [22402] Ajout vérification du marché -->
    <xsl:variable name="isMarketOk">
      <xsl:choose>
        <!-- Idem -->
        <xsl:when test="'02'=datafields/data[@name='marketid'][1]">true</xsl:when>
        <!-- Idex -->
        <xsl:when test="'05'=datafields/data[@name='marketid'][1]">true</xsl:when>
        <!-- Agrex -->
        <xsl:when test="'08'=datafields/data[@name='marketid'][1]">true</xsl:when>
        <xsl:otherwise>false</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- Si isNotifyContractsOk = true -->
    <!-- PM 20161014 [22402] Ajout vérification du marché (si isMarketOk = true)-->
    <!--<xsl:if test="$isNotifyContractsOk = 'true'">-->
    <xsl:if test="($isNotifyContractsOk = 'true') and ($isMarketOk =  'true')">
      <!-- FI 20170512 [23165] Add variable vIsReversing -->
      <xsl:variable name ="vIsReversing">
        <xsl:choose>
          <xsl:when test="$vMessageclass='NotifySubContracts' and datafields/data[@name='contractstate']='R'">
            <xsl:value-of select ="'true'"/>
          </xsl:when>
          <xsl:otherwise>false</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <!-- FI 20170824 [23339] add vIsReversingGiveUP -->
      <xsl:variable name ="vIsReversingGiveUP">
        <xsl:choose>
          <xsl:when test="$vIsReversing='true' and datafields/data[@name='repoindex']='G'">
            <xsl:value-of select ="'true'"/>
          </xsl:when>
          <xsl:otherwise>false</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <!-- Récupérer ActionType -->
      <!-- FI 20170512 [23165] passage du paramètre pIsReversing -->
      <!-- FI 20170824 [23339] passage du paramètre pIsReversingGiveUP -->
      <xsl:variable name="vActionType">
        <xsl:call-template name="GetActionType">
          <xsl:with-param name="pMessageclass" select="$vMessageclass"/>
          <xsl:with-param name="pContractnumber" select="$vContractnumber"/>
          <xsl:with-param name="pOrigcontractnumber" select="$vOrigcontractnumber"/>
          <xsl:with-param name="pTransfertype" select="$vTransfertype"/>
          <xsl:with-param name="pIsReversing" select="$vIsReversing"/>
          <xsl:with-param name="pIsReversingGiveUP" select="$vIsReversingGiveUP"/>
        </xsl:call-template>
      </xsl:variable>

      <!-- FI 20170512 [23165] pas d'action particulière si action de type ='S' -->
      <xsl:call-template name="MatchDatafields">
        <xsl:with-param name="pATP" select="$vActionType"/>
        <xsl:with-param name="pMessageclass" select="$vMessageclass"/>
      </xsl:call-template>

    </xsl:if>
  </xsl:template>

 
  <!-- ================================================== -->
  <!--        GetActionType                               -->
  <!-- ================================================== -->
  <!-- Récupère l'ActionType suivant messageclass, contractnumber, origcontractnumber et transfertype -->
  <!-- FI 20170512 [23165] Add pIsReversing -->
  <!-- FI 20170824 [23339] Add pIsReversingGiveUP -->
  <xsl:template name="GetActionType">
    <xsl:param name="pMessageclass"/>
    <xsl:param name="pContractnumber"/>
    <xsl:param name="pOrigcontractnumber"/>
    <xsl:param name="pTransfertype"/>
    <xsl:param name="pIsReversing"/>
    <xsl:param name="pIsReversingGiveUP"/>

    <xsl:choose>
      <!-- NotifyContractsByTime -> N -->
      <xsl:when test="$pMessageclass='NotifyContractsByTime'">N</xsl:when>

      <!-- NotifyContracts -->
      <!-- PM 20160818 [22402] Ajout NotifySubContracts -->
      <!-- FI 20170512 [23165] Nouvelles règles  pour la valeur retour (Utilisation de pIsReversing)   -->
      <xsl:when test="$pMessageclass='NotifyContracts' or $pMessageclass='NotifySubContracts'">
        <xsl:choose>
          <!-- FI 20170824 [23339] si $pIsReversingGiveUP Action => T (Ajout du trader s'il n'existe pas)  -->
          <xsl:when test="$pIsReversingGiveUP = 'true'">T</xsl:when>
          <xsl:when test="$pIsReversing = 'true'">S</xsl:when>
          <xsl:when test="$pContractnumber = $pOrigcontractnumber">M</xsl:when>
          <xsl:otherwise>N</xsl:otherwise>
        </xsl:choose>
      </xsl:when>

      <!-- NotifyContractTransfers -->
      <!-- PM 20160818 [22402] Ajout NotifySubContractTransfers -->
      <xsl:when test="$pMessageclass='NotifyContractTransfers' or $pMessageclass='NotifySubContractTransfers'">
        <xsl:choose>
          <!-- transfertype = D (delivered) -> G (give-up) -->
          <xsl:when test="$pTransfertype = 'D'">G</xsl:when>
          <!-- transfertype = R (receveid) -> N (take-up) -->
          <xsl:when test="$pTransfertype = 'R'">N</xsl:when>
        </xsl:choose>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  
  <!-- ================================================== -->
  <!--        MatchDatafields                             -->
  <!-- ================================================== -->
  <xsl:template name="MatchDatafields">
    <!-- ActionType (ATP) -->
    <xsl:param name="pATP"/>
    <!-- messageclass -->
    <xsl:param name="pMessageclass"/>

    <!-- Récupérer TradeID (TRID) -->
    <!-- FI 20170512 [23165] Lecture sytématique de contractnumber et side -->
    <xsl:variable name="vTradeID">
      <!--<xsl:call-template name="GetTradeID">
        <xsl:with-param name="pActionType" select="$ATP"/>
      </xsl:call-template>-->
      <xsl:value-of select="concat(datafields/data[@name='contractnumber'],'-',datafields/data[@name='side'][1])"/>
    </xsl:variable>

    <row>
      <xsl:variable name="idprefix" select="'r'"/>
      <xsl:variable name="idposition" select="position()"/>
      <xsl:attribute name="id" >
        <xsl:value-of select="concat($idprefix,$idposition)"/>
      </xsl:attribute>
      <xsl:attribute name="src">
        <xsl:value-of select="$idposition"/>
      </xsl:attribute>
      
      
      <xsl:attribute name="status">success</xsl:attribute>

      <xsl:apply-templates select="datafields">
        <xsl:with-param name="pATP" select="$pATP"/>
        <xsl:with-param name="pTRID" select="$vTradeID"/>
        <xsl:with-param name="pMessageclass" select="$pMessageclass"/>
      </xsl:apply-templates>
    </row>
  </xsl:template>


  <!-- ================================================== -->
  <!--        <datafields>                                -->
  <!-- ================================================== -->
  <xsl:template match="datafields">
    <xsl:param name="pATP"/>
    <xsl:param name="pTRID"/>
    <xsl:param name="pMessageclass"/>

    <!-- FI 20170510 [23039] add memberID -->
    <!-- MemberID (Membre connecté qui recoit le flux) -->
    <xsl:variable name ="memberID" select ="../@AbiCode"/>

    <!-- tradeClass -->
    <!-- FI 20170510 [23039] la variable tradeClass est désormais renseignée systématiquement. -->
    <xsl:variable name="tradeClass">
      <xsl:choose>
        <!-- FI 20170510 [23039] add NotifyContracts, NotifyContractsByTime, NotifySubContracts  -->
        <xsl:when test="($pMessageclass='NotifyContracts' or $pMessageclass='NotifyContractsByTime' or $pMessageclass='NotifySubContracts') 
                        and data[@name='giveupabicode']!='00000'">
          <xsl:choose>
            <xsl:when test="data[@name='abicode'][1]=data[@name='giveupabicode']">
              <!-- data[@name='abicode'] est l'acteur ayant exécuté le trade (il ne se charge pas de la compensation) 
                   Remarque => on ne sait pas quel est l'acteur en charge de la compensation
              -->
              <!-- Définition de giveupabicode:  In case of a local give up trade, it is the member ABI code of the company that executed the contract -->
              <!-- Ce cas se produit sur les messages de type Reverse lorsqu'il y a give-up -->
              <xsl:value-of select ="'give-up'"/>
            </xsl:when>
            <xsl:otherwise>
              <!-- data[@name='abicode'] est l'acteur en charge de la compensation -->
              <xsl:value-of select ="'take-up'"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <!-- PM 20160818 [22402] Ajout NotifySubContractTransfers -->
        <xsl:when test="$pMessageclass='NotifyContractTransfers' or $pMessageclass='NotifySubContractTransfers'">
          <xsl:choose>
            <!-- transfertype = D (delivered) -> give-up -->
            <xsl:when test="data[@name='transfertype']='D'">give-up</xsl:when>
            <!-- transfertype = R (received) -> take-up -->
            <xsl:when test="data[@name='transfertype']='R'">take-up</xsl:when>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>normal</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- isNCMOfGCM => true si la gateway est installée chez un GCM et elle reçoit le flux concernant un NCM -->
    <!-- FI 20170510 [23039] add variable -->
    <xsl:variable name ="isNCMOfGCM">
      <xsl:choose>
        <xsl:when test="$pMessageclass='NotifyContracts' or $pMessageclass='NotifyContractsByTime' or $pMessageclass='NotifySubContracts'">
          <xsl:choose>
            <xsl:when test ="data[@name='abicode'][1] != $memberID">true</xsl:when>
            <xsl:otherwise>false</xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="($pMessageclass='NotifyContractTransfers' or $pMessageclass='NotifySubContractTransfers') and $tradeClass='give-up' ">
          <xsl:choose>
            <xsl:when test ="data[@name='deliverabicode'] != $memberID">true</xsl:when>
            <xsl:otherwise>false</xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>false</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- contractdate-->
    <xsl:variable name="year">
      <xsl:value-of select="substring(data[@name='contractdate'][1],1,4)"/>
    </xsl:variable>
    <xsl:variable name="month">
      <xsl:value-of select="substring(data[@name='contractdate'][1],5,2)"/>
    </xsl:variable>
    <xsl:variable name="day">
      <xsl:value-of select="substring(data[@name='contractdate'][1],7,2)"/>
    </xsl:variable>

    <!-- transferdate -->
    <!-- FI 20140106 [19445] add variable transferYear,transferMonth,transferDay  -->
    <!-- PM 20160818 [22402] Ajout NotifySubContractTransfers -->
    <xsl:variable name="transferYear">
      <xsl:if test="$pMessageclass='NotifyContractTransfers' or $pMessageclass='NotifySubContractTransfers'">
        <xsl:value-of select="substring(data[@name='transferdate'],1,4)"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="transferMonth">
      <xsl:if test="$pMessageclass='NotifyContractTransfers' or $pMessageclass='NotifySubContractTransfers'">
        <xsl:value-of select="substring(data[@name='transferdate'],5,2)"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="transferDay">
      <xsl:if test="$pMessageclass='NotifyContractTransfers' or $pMessageclass='NotifySubContractTransfers'">
        <xsl:value-of select="substring(data[@name='transferdate'],7,2)"/>
      </xsl:if>
    </xsl:variable>

    <!-- category -->
    <!-- PM 20171026 [23566] Add variable category -->
    <xsl:variable name="category">
      <xsl:choose>
        <xsl:when test="normalize-space(data[@name='putcall']) != ''">O</xsl:when>
        <xsl:otherwise>F</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- ALLOWDATAMISSDEALER -->
    <data>
      <xsl:attribute name="name">ALLOWDATAMISSDEALER</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'Allowed'"/>
    </data>

    <!-- RecordVersion -->
    <!-- PM 20171026 [23566] Add RecordVersion -->
    <data>
      <xsl:attribute name="name">RVR</xsl:attribute>
      <xsl:attribute name="datatype">integer</xsl:attribute>
      <xsl:value-of select="3"/>
    </data>

    <!-- RecordType -->
    <data>
      <xsl:attribute name="name">RTP</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'A'"/>
    </data>

    <!-- ActionType -->
    <data>
      <xsl:attribute name="name">ATP</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="$pATP"/>
    </data>

    <!-- TradeID -->
    <data>
      <xsl:attribute name="name">TRID</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="$pTRID"/>
    </data>

    <!-- SpheresProduct -->
    <!-- PM 20171026 [23566] Add SpheresProduct -->
    <data>
      <xsl:attribute name="name">PRD</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'exchangeTradedDerivative'"/>
    </data>

    <!-- SpheresInstrument -->
    <!-- PM 20171026 [23566] Add SpheresInstrument -->
    <data>
      <xsl:attribute name="name">INS</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:choose>
        <xsl:when test="$category='F'">
          <xsl:value-of select="'ExchangeTradedFuture'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'ExchangeTradedOption'"/>
        </xsl:otherwise>
      </xsl:choose>
    </data>

    <!-- SpheresInstrumentIdent -->
    <!-- PM 20171026 [23566] Add SpheresInstrumentIdent -->
    <data>
      <xsl:attribute name="name">INSI</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'SpheresIdentifier'"/>
    </data>

    <!-- SpheresFeeCalculation -->
    <data>
      <xsl:attribute name="name">SFC</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'Apply'"/>
    </data>

    <!-- SpheresPartyTemplate -->
    <!-- PM 20160818 [22402] Ajout NotifySubContractTransfers (et NotifySubContracts)-->
    <!-- FI 20170510 [23039] Mise en commentaire PARTYTEMPLATE n'est jamais exploité (ce comportement est celui de la gateway Eurex )-->
    <!--<data>
      <xsl:attribute name="name">SPT</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:choose>
        <xsl:when test="$pMessageclass='NotifyContracts' or $pMessageclass='NotifyContractsByTime' or $pMessageclass='NotifySubContracts'">
          <xsl:choose>
            <xsl:when test="data[@name='abicode'][1] != $memberID">Apply</xsl:when>
            <xsl:otherwise>Ignore</xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="$pMessageclass='NotifyContractTransfers' or $pMessageclass='NotifySubContractTransfers'">
          <xsl:choose>
            <xsl:when test="$tradeClass = 'give-up'">
              <xsl:choose>
                <xsl:when test="data[@name='deliverabicode'] != $memberID">Apply</xsl:when>
                <xsl:otherwise>Ignore</xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:when test="$tradeClass = 'take-up'">
              <xsl:choose>
                <xsl:when test="data[@name='receiverabicode'] != $memberID">Apply</xsl:when>
                <xsl:otherwise>Ignore</xsl:otherwise>
              </xsl:choose>
            </xsl:when>
          </xsl:choose>
        </xsl:when>
      </xsl:choose>
    </data>-->

    <!-- SpheresPartyTemplate -->
    <!-- FI 20170510 [23039] SPT systématiquement valorisé à Ignore -->
    <data>
      <xsl:attribute name="name">SPT</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'Ignore'"/>
    </data>

    <!-- Facility -->
    <!-- PM 20171026 [23566] Add FAC (XDMI en dur) -->
    <data>
      <xsl:attribute name="name">FAC</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'XDMI'"/>
    </data>

    <!-- FacilityIdent -->
    <!-- PM 20171026 [23566] Add FACI -->
    <data>
      <xsl:attribute name="name">FACI</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'SpheresIdentifier'"/>
    </data>

    <!-- ExecutionDateTime -->
    <!-- RD 20161020 [22553] Use of 'executiontime' for 'NotifyContractTransfers/NotifySubContractTransfers' -->
    <!-- PM 20171026 [23566] Rename TDT (TransactionDate) in EDT (ExecutionDateTime) et ajustement du format -->
    <data>
      <!--<xsl:attribute name="name">TDT</xsl:attribute>-->
      <xsl:attribute name="name">EDT</xsl:attribute>
      <xsl:attribute name="datatype">datetime</xsl:attribute>
      <!-- contracttime -->
      <xsl:variable name="contracttime">
        <xsl:choose>
          <xsl:when test ="$pMessageclass='NotifyContractTransfers' or $pMessageclass='NotifySubContractTransfers'">
            <xsl:choose>
              <!-- Si executiontime existe -->
              <xsl:when test="data[@name='executiontime']">
                <xsl:variable name="hour">
                  <xsl:value-of select="substring(data[@name='executiontime'],9,2)"/>
                </xsl:variable>
                <xsl:variable name="minut">
                  <xsl:value-of select="substring(data[@name='executiontime'],11,2)"/>
                </xsl:variable>
                <xsl:variable name="second">
                  <xsl:value-of select="substring(data[@name='executiontime'],13,2)"/>
                </xsl:variable>
                <xsl:value-of select="concat($hour,':',$minut,':',$second)"/>
              </xsl:when>
              <!-- Si executiontime n'existe pas -->
              <xsl:otherwise>00:00:00</xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:otherwise>
            <xsl:choose>
              <!-- Si contracttime existe -->
              <xsl:when test="data[@name='contracttime']">
                <xsl:variable name="hour">
                  <xsl:value-of select="substring(data[@name='contracttime'],1,2)"/>
                </xsl:variable>
                <xsl:variable name="minute">
                  <xsl:value-of select="substring(data[@name='contracttime'],3,2)"/>
                </xsl:variable>
                <xsl:variable name="second">
                  <xsl:value-of select="substring(data[@name='contracttime'],5,2)"/>
                </xsl:variable>
                <xsl:value-of select="concat($hour,':',$minute,':',$second)"/>
              </xsl:when>
              <!-- Si contracttime n'existe pas -->
              <xsl:otherwise>00:00:00</xsl:otherwise>
            </xsl:choose>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <!-- datetime format ISO 8601 étendu yyyy-MM-ddTHH:mm:ss -->
      <xsl:value-of select="concat($year,'-',$month,'-',$day,'T',$contracttime)"/>
    </data>

    <!-- ExecutionTimeZone -->
    <!-- PM 20171026 [23566] Add ExecutionTimeZone -->
    <data>
      <xsl:attribute name="name">EDTTZ</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'Europe/Rome'"/>
    </data>    
    
    <!-- BusinessDate -->
    <!-- FI 20140106 [19445] use of transferYear,transferMonth,transferDay -->
    <!-- PM 20160818 [22402] Ajout NotifySubContractTransfers -->
    <!-- PM 20171026 [23566] Rename CDT in BDT -->
    <data>
      <!--<xsl:attribute name="name">CDT</xsl:attribute>-->
      <xsl:attribute name="name">BDT</xsl:attribute>
      <xsl:attribute name="datatype">date</xsl:attribute>
      <xsl:choose>
        <xsl:when test ="$pMessageclass='NotifyContractTransfers' or $pMessageclass='NotifySubContractTransfers'">
          <xsl:value-of select="concat($transferYear,'-',$transferMonth,'-',$transferDay)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="concat($year,'-',$month,'-',$day)"/>
        </xsl:otherwise>
      </xsl:choose>
    </data>

    <!-- Comment -->
    <!-- PM 20160818 [22402] Ajout NotifySubContractTransfers (et NotifySubContracts) -->
    <data>
      <xsl:attribute name="name">CMT</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:choose>
        <xsl:when test="$pMessageclass='NotifyContracts' or $pMessageclass='NotifyContractsByTime' or $pMessageclass='NotifySubContracts'">
          <xsl:value-of select="data[@name='clientinfo']"/>
        </xsl:when>
        <xsl:when test="$pMessageclass='NotifyContractTransfers' or $pMessageclass='NotifySubContractTransfers'">
          <xsl:choose>
            <!-- give-up -->
            <xsl:when test="$tradeClass='give-up'">
              <xsl:value-of select="data[@name='deliverinfo']"/>
            </xsl:when>
            <!-- take-up -->
            <xsl:when test="$tradeClass='take-up'">
              <xsl:value-of select="data[@name='receiverinfo']"/>
            </xsl:when>
          </xsl:choose>
        </xsl:when>
      </xsl:choose>
    </data>

    <xsl:if test="$pMessageclass='NotifyContracts' or $pMessageclass='NotifyContractsByTime' or $pMessageclass='NotifySubContracts'">
      <!-- TVTIC -->
      <data>
        <xsl:attribute name="name">TVTIC</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="data[@name='tvtic']"/>
      </data>
    </xsl:if>
    
    <!-- SpheresValidationXSD -->
    <data>
      <xsl:attribute name="name">SVX</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'Ignore'"/>
    </data>

    <!-- SpheresValidationRule -->
    <data>
      <xsl:attribute name="name">SVR</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'Apply'"/>
    </data>

    <!-- Market -->
    <data>
      <xsl:attribute name="name">MKT</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:choose>
        <xsl:when test="data[@name='marketid'][1]='02'">XDMI</xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="concat('XDMI-',number(data[@name='marketid'][1]))"/>
        </xsl:otherwise>
      </xsl:choose>
    </data>

    <!-- MarketIdent -->
    <data>
      <xsl:attribute name="name">MKTI</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'SpheresSecurityExchange'"/>
    </data>

    <!-- Contract -->
    <!-- PM 20171026 [23566] Rename DVT in CTR -->
    <data>
      <!--<xsl:attribute name="name">DVT</xsl:attribute>-->
      <xsl:attribute name="name">CTR</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="data[@name='symbol']"/>
    </data>

    <!-- ContractIdent -->
    <!-- PM 20171026 [23566] Rename DVTI in CTRI -->
    <data>
      <xsl:attribute name="name">CTRI</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'SpheresContractCode'"/>
    </data>

    <!-- ContractCategory -->
    <!-- PM 20171026 [23566] Use of $category and Rename DVTC in CTRC-->
    <data>
      <!--<xsl:attribute name="name">DVTC</xsl:attribute>-->
      <xsl:attribute name="name">CTRC</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <!--<xsl:choose>
        <xsl:when test="number(data[@name='strikeprice']) != 0">O</xsl:when>
        <xsl:otherwise>F</xsl:otherwise>
      </xsl:choose>-->
      <xsl:value-of select="$category"/>
    </data>

    <!-- ContractPutCall -->
    <!-- PM 20171026 [23566] Rename DVTO in CTRT -->
    <data>
      <!--<xsl:attribute name="name">DVTO</xsl:attribute>-->
      <!--<xsl:attribute name="name">CTRT</xsl:attribute>-->
      <xsl:attribute name="name">CTRPC</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="data[@name='putcall']"/>
    </data>

    <!-- ContractMaturity -->
    <!-- PM 20171026 [23566] Rename DVTM in CTRM -->
    <data>
      <!--<xsl:attribute name="name">DVTM</xsl:attribute>-->
      <xsl:attribute name="name">CTRM</xsl:attribute>
      <xsl:attribute name="datatype">date</xsl:attribute>
      <xsl:value-of select="data[@name='expirationmonth']"/>
    </data>

    <!-- ContractStrike -->
    <!-- PM 20171026 [23566] Rename DVTS in CTRS -->
    <data>
      <!--<xsl:attribute name="name">DVTS</xsl:attribute>-->
      <xsl:attribute name="name">CTRS</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="data[@name='strikeprice']"/>
    </data>

    <!-- BuyerOrSellerAccount -->
    <!-- PM 20160818 [22402] Ajout NotifySubContractTransfers (et NotifySubContracts) -->
    <data>
      <xsl:attribute name="name">BSA</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:choose>
        <xsl:when test="$pMessageclass='NotifyContracts' or $pMessageclass='NotifyContractsByTime' or $pMessageclass='NotifySubContracts'">
          <xsl:choose>
            <xsl:when test="string-length(data[@name='clientcode'])>0">
              <xsl:value-of select="data[@name='clientcode']"/>
            </xsl:when>
            <xsl:otherwise>{NotSpecified}</xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="$pMessageclass='NotifyContractTransfers' or $pMessageclass='NotifySubContractTransfers'">
          <xsl:choose>
            <!-- give-up -->
            <xsl:when test="$tradeClass='give-up'">
              <xsl:choose>
                <xsl:when test="string-length(data[@name='delivercode'])>0">
                  <xsl:value-of select="data[@name='delivercode']"/>
                </xsl:when>
                <xsl:otherwise>{NotSpecified}</xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <!-- take-up -->
            <xsl:when test="$tradeClass='take-up'">
              <xsl:choose>
                <xsl:when test="string-length(data[@name='receivercode'])>0">
                  <xsl:value-of select="data[@name='receivercode']"/>
                </xsl:when>
                <xsl:otherwise>{NotSpecified}</xsl:otherwise>
              </xsl:choose>
            </xsl:when>
          </xsl:choose>
        </xsl:when>
      </xsl:choose>
    </data>
    <!-- BuyerOrSellerAccountIDent -->
    <data>
      <xsl:attribute name="name">BSAI</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'SpheresExtlLink'"/>
    </data>
    
    <!-- BuyerOrSellerTrader -->
    <!-- FI 20170510 [23039] Alimentation du trader sur l'acteur dealer s'il s'agit de l'activé d'un NCM et que l'entité connectée est GCM 
        => C'est le NCM (à travers son trader) qui s'est chargé de l'exécution 
        Remarque : traderid existe uniquement sur les message NotifyContracts, NotifyContractsByTime, NotifySubContracts
        Remarque : même nomenclature que pour pour les flux issus de FIXMLEurex ({Membre}+'_'+{Trader})
        -->
    <xsl:if test="$pMessageclass='NotifyContracts' or $pMessageclass='NotifyContractsByTime' or $pMessageclass='NotifySubContracts'">
      <xsl:if test="$isNCMOfGCM='true' and $tradeClass!='take-up' ">
        <xsl:call-template name ="ovrGetBuyerOrSellerTraderData">
          <xsl:with-param name="pData" select="concat(data[@name='abicode'][1],'_',data[@name='traderid'])"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:if>

    <!-- BuyerOrSellerNegociationBrokerTrader -->
    <!-- FI 20170510 [23039] Alimentation du trader sur l'acteur NegociationBroker 
        (sauf s'il s'agit de l'activé d'un NCM et que l'entité connectée est GCM, sauf s'il s'agit d'un take-up) 
        Remarque : traderid existe uniquement sur les message NotifyContracts, NotifyContractsByTime, NotifySubContracts
        -->
    <xsl:if test="$pMessageclass='NotifyContracts' or $pMessageclass='NotifyContractsByTime' or $pMessageclass='NotifySubContracts'">
      <xsl:choose>
        <xsl:when test="$isNCMOfGCM='true'"/>
        <xsl:when test="$tradeClass='take-up'"/>
        <xsl:otherwise>
          <xsl:call-template name ="ovrGetBuyerOrSellerNegociationBrokerTraderData">
            <xsl:with-param name="pData" select="concat(data[@name='abicode'][1],'_',data[@name='traderid'])"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>

    <!-- BuyerOrSellerExecutingBroker -->
    <!-- PM 20160818 [22402] Ajout NotifySubContractTransfers (et NotifySubContracts) -->
    <xsl:if test="$tradeClass='take-up'">
      <data>
        <xsl:attribute name="name">BSEB</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:choose>
          <xsl:when test="($pMessageclass='NotifyContracts' or $pMessageclass='NotifyContractsByTime' or $pMessageclass='NotifySubContracts')">
            <xsl:value-of select="data[@name='giveupabicode']"/>
          </xsl:when>
          <xsl:when test="$pMessageclass='NotifyContractTransfers' or $pMessageclass='NotifySubContractTransfers'">
            <xsl:value-of select="data[@name='deliverabicode']"/>
          </xsl:when>
        </xsl:choose>
      </data>
      <!-- BuyerOrSellerExecutingBrokerIdent -->
      <data>
        <xsl:attribute name="name">BSEBI</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="'SpheresExtlLink'"/>
      </data>
      <!-- BuyerOrSellerExecutingBrokerTrader -->
      <!-- FI 20170510 [23039] Alimentation du trader sur l'acteur ExecutingBroker (Si take-up)
        Remarque : traderid existe uniquement sur les message NotifyContracts, NotifyContractsByTime, NotifySubContracts
        -->
      <!-- FI 20170824 [23339] pas d'alimentation du trader -->
      <!--<xsl:if test="$pMessageclass='NotifyContracts' or $pMessageclass='NotifyContractsByTime' or $pMessageclass='NotifySubContracts'">
        <xsl:call-template name ="ovrGetBuyerOrSellerExecutingBrokerTraderData">
          <xsl:with-param name="pData" select="data[@name='traderid']"/>
        </xsl:call-template>
      </xsl:if>-->
    </xsl:if>

    <!-- BuyerOrSellerClearingOrganisation -->
    <data>
      <xsl:attribute name="name">BSCO</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'CCEGITRR'"/>
    </data>
    <!-- BuyerOrSellerClearingOrganisationIdent -->
    <data>
      <xsl:attribute name="name">BSCOI</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'BIC'"/>
    </data>

    <!-- BuyerOrSellerClearingOrganisationAccount -->
    <!-- PM 20160818 [22402] Ajout NotifySubContractTransfers (et NotifySubContracts) -->
    <!-- PM 20171026 [23566] Rename BSCA in BSCOA -->
    <data>
      <!--<xsl:attribute name="name">BSCA</xsl:attribute>-->
      <xsl:attribute name="name">BSCOA</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:choose>
        <!-- NotifyContracts ou NotifyContractsByTime ou NotifySubContracts-->
        <xsl:when test="($pMessageclass='NotifyContracts' or $pMessageclass='NotifyContractsByTime' or $pMessageclass='NotifySubContracts')">
          <xsl:choose>
            <!-- FI 20130729 [18841] test que len(subaccount>0) -->
            <xsl:when test="string-length(data[@name='subaccount'])>0 and data[@name='subaccount'] != '*OMN'">
              <xsl:value-of select="data[@name='subaccount']"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="data[@name='accounttype'][1]"/>
              <!-- FI 20130729 [18841] si Give-Up local => compte avec suffixe .GU-->
              <xsl:choose>
                <xsl:when test ="$tradeClass='give-up'">.GU</xsl:when>
                <xsl:otherwise/>
              </xsl:choose>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <!-- NotifyContractTransfers ou NotifySubContractTransfers -->
        <xsl:when test="$pMessageclass='NotifyContractTransfers' or $pMessageclass='NotifySubContractTransfers'">
          <xsl:choose>
            <!-- give-up -->
            <xsl:when test="$tradeClass='give-up'">
              <xsl:choose>
                <!-- FI 20130729 [18841] test que len(subaccount>0) -->
                <xsl:when test="string-length(data[@name='subaccount'])>0 and data[@name='subaccount'] != '*OMN'">
                  <xsl:value-of select="data[@name='subaccount']"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="data[@name='deliveraccounttype']"/>
                  <!-- FI 20130729 [18841] si Give-Up => compte avec suffixe .GU-->
                  <xsl:value-of select="'.GU'"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <!-- take-up -->
            <xsl:when test="$tradeClass='take-up'">
              <xsl:choose>
                <!-- FI 20130729 [18841] test que len(subaccount>0) -->
                <xsl:when test="string-length(data[@name='subaccount'])>0 and data[@name='subaccount'] != '*OMN'">
                  <xsl:value-of select="data[@name='subaccount']"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="data[@name='receiveraccounttype']"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
          </xsl:choose>
        </xsl:when>
      </xsl:choose>
    </data>

    <!-- BuyerOrSellerClearingOrganisationAccountIdent -->
    <!-- PM 20171026 [23566] Rename BSCAI in BSCOAI -->
    <data>
      <!--<xsl:attribute name="name">BSCAI</xsl:attribute>-->
      <xsl:attribute name="name">BSCAI</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'SpheresExtlLink'"/>
    </data>

    <!-- BuyerOrSellerClearingBroker -->
    <!-- FI 20171215 [23652] Modify receiverabicode à la place de receiverAbiCode-->
    <!-- FI 20190115 [24432] BSCB doit être alimenté uniquement sur les messages de transfert car sinon (sur les message NotifySubContracts de type Reverse) le BSCB est non renseigné
         provocant au final l'enregistrement du trade avec la statut missing du fait de l'absence de clearing broker   
    -->
    <!--<xsl:if test="$tradeClass='give-up'">-->
    <xsl:if test="($pMessageclass='NotifyContractTransfers' or $pMessageclass='NotifySubContractTransfers') and $tradeClass='give-up'">
      <data>
        <xsl:attribute name="name">BSCB</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <!--<xsl:choose>
          <xsl:when test="$pMessageclass='NotifyContractTransfers' or $pMessageclass='NotifySubContractTransfers'">
            <xsl:value-of select="data[@name='receiverabicode']"/>
          </xsl:when>
        </xsl:choose>-->
        <xsl:value-of select="data[@name='receiverabicode']"/>
      </data>
      <!-- BuyerOrSellerClearingBrokerIdent -->
      <data>
        <xsl:attribute name="name">BSCBI</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="'SpheresExtlLink'"/>
      </data>
    </xsl:if>

    <!-- ExecutionId -->
    <data>
      <xsl:attribute name="name">EXID</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="data[@name='contractnumber']"/>
    </data>

    <!-- AllocationId -->
    <!-- PM 20171026 [23566] Add ALID (marketcontractnumber) -->
    <data>
      <xsl:attribute name="name">ALID</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="data[@name='marketcontractnumber']"/>
    </data>
    
    <!-- AllocationSide -->
    <data>
      <xsl:attribute name="name">ALS</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:choose>
        <xsl:when test="data[@name='side'][1]='B'">1</xsl:when>
        <xsl:when test="data[@name='side'][1]='S'">2</xsl:when>
      </xsl:choose>
    </data>

    <!-- AllocationQuantity -->
    <data>
      <xsl:attribute name="name">ALQ</xsl:attribute>
      <xsl:attribute name="datatype">integer</xsl:attribute>
      <xsl:value-of select="data[@name='quantity']"/>
    </data>

    <!-- AllocationPrice -->
    <!-- PM 20171026 [23566] Rename PRI in ALP -->
    <data>
      <!--<xsl:attribute name="name">PRI</xsl:attribute>-->
      <xsl:attribute name="name">ALP</xsl:attribute>
      <xsl:attribute name="datatype">decimal</xsl:attribute>
      <xsl:value-of select="data[@name='price']"/>
    </data>

    <!-- PositionEffect -->
    <data>
      <xsl:attribute name="name">ALPE</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:choose>
        <xsl:when test="data[@name='openclose']='1'">O</xsl:when>
        <xsl:when test="data[@name='openclose']='2'">C</xsl:when>
      </xsl:choose>
    </data>

    <!-- OrderElectId -->
    <data>
      <xsl:attribute name="name">OEID</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="data[@name='ordernumber']"/>
    </data>

    <!-- OrderElectQuantity -->
    <!-- PM 20171026 [23566] Rename OEQT in OEQ. Commented because not filled  -->
    <!--<data>
      --><!--<xsl:attribute name="name">OEQT</xsl:attribute>--><!--
      <xsl:attribute name="name">OEQ</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'TBD'"/>
    </data>-->

    <!-- AllocationType -->
    <data>
      <xsl:attribute name="name">ALT</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:choose>
        <!-- Late Trade -->
        <xsl:when test="data[@name='marketsource'] = '4'">4</xsl:when>
        <!-- ExchangeGranted Trade -->
        <xsl:when test="data[@name='marketsource'] = '6' or data[@name='marketsource'] = '7'">52</xsl:when>
        <!-- Regular Trade par défaut  faute de correspondance -->
        <xsl:otherwise>0</xsl:otherwise>
      </xsl:choose>
    </data>

    <!-- MemberId (Identifiant chambre du membre connecté) -->
    <data>
      <xsl:attribute name="name">MID</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="$memberID"/>
    </data>

    <!-- Extends -->
    <xsl:call-template name="AddExtendsData">
      <xsl:with-param name="pATP" select="$pATP"/>
      <xsl:with-param name="pTRID" select="$pTRID"/>
      <xsl:with-param name="pMessageclass" select="$pMessageclass"/>
    </xsl:call-template>

  </xsl:template>


  <!-- ============================================================== -->
  <!--        ovrGetBuyerOrSellerTraderData                           -->
  <!-- ============================================================== -->
  <!-- FI  20170428 [23039] add -->
  <xsl:template name="ovrGetBuyerOrSellerTraderData">
    <xsl:param name="pData"/>

    <!-- BuyerOrSellerTrader -->
    <data>
      <xsl:attribute name="name">BSTR</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <!-- FI 20170824 [23339] Alimentation de BSTR avec $pData-->
      <xsl:value-of select="$pData"/>
    </data>

    <!-- BuyerOrSellerTraderIdent -->
    <data>
      <xsl:attribute name="name">BSTRI</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'SpheresExtlLink'"/>
    </data>

    <!-- BuyerOrSellerTraderAutoCreate -->
    <data>
      <xsl:attribute name="name">BSTRC</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <!-- FI 20170824 [23339] autocréation du trader OUI -->
      <xsl:value-of select="'Y'"/>
    </data>

  </xsl:template>

  <!-- ============================================================== -->
  <!--        ovrGetBuyerOrSellerExecutingBrokerTraderData            -->
  <!-- ============================================================== -->
  <!-- FI 20170510 [23039] add -->
  <xsl:template name="ovrGetBuyerOrSellerExecutingBrokerTraderData">
    <xsl:param name="pData"/>
    <!-- BuyerOrSellerExecutingBrokerTrader -->
    <data>
      <xsl:attribute name="name">BSEBTR</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="$pData"/>
    </data>

    <!-- BuyerOrSellerExecutingBrokerTraderIdent -->
    <data>
      <xsl:attribute name="name">BSEBTRI</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'SpheresExtlLink'"/>
    </data>

    <!-- BuyerOrSellerExecutingBrokerTraderAutoCreate -->
    <data>
      <xsl:attribute name="name">BSEBTRC</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'Y'"/>
    </data>
  </xsl:template>

  <!-- ============================================================== -->
  <!--        ovrGetBuyerOrSellerNegociationBrokerTraderData         -->
  <!-- ============================================================== -->
  <!-- FI 20170510 [23039] add -->
  <xsl:template name="ovrGetBuyerOrSellerNegociationBrokerTraderData">
    <xsl:param name="pData"/>
    
    <!-- BuyerOrSellerNegociationBrokerTrader -->
    <data>
      <xsl:attribute name="name">BSNBTR</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="$pData"/>
    </data>
    
    <!-- BuyerOrSellerNegociationBrokerTraderIdent -->
    <data>
      <xsl:attribute name="name">BSNBTRI</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'SpheresExtlLink'"/>
    </data>

    <!-- BuyerOrSellerNegociationBrokerTraderAutoCreate -->
    <data>
      <xsl:attribute name="name">BSNBTRC</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'Y'"/>
    </data>
  </xsl:template>

  <!-- ================================================== -->
  <!--        AddExtendsData                              -->
  <!-- ================================================== -->
  <xsl:template name="AddExtendsData">
    <!-- ATP -->
    <xsl:param name="pATP"/>
    <!-- TRID -->
    <xsl:param name="pTRID"/>
    <!-- messageclass -->
    <xsl:param name="pMessageclass"/>
  </xsl:template>

</xsl:stylesheet>