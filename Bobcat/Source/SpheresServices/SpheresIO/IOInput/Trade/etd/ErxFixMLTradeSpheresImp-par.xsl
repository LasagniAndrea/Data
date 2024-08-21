<?xml version="1.0" encoding="utf-8"?>
<!--
=============================================================================================
Summary : Eurex FixML Gateway - Trade input
File    : ErxFixMLTradeSpheresImp-par.xsl
=============================================================================================
REVISIONS
FI 20221018 [25699] prise en compte de ContractDate uniquement sur la chambre ECAG
FI 20221013 [25699] prise en compte de ContractDate
FI 20220628 [25699] Rename CTRK in CTRPC
FI 2022XXXX [25699] Adaptation au format d'import V3 (dispo avec Spheres V12) => RVR='3', Alimentation de CLH, CLHI, CTRT, CTRK, CTRSM, CTRES
SI 20220223 [25951] Use ProdCmplx FIXml Tag (FIX Tag 1227) on Flexible Trade (TrdTyp 54=OTC)
PM 20171026 [23567] Adaptation au format d'import V2 (dispo avec Spheres V7)
FI 20170727 [23340] Mod Trader => Trader se trouve sous BSTR ou BSNBTR
FI 20170428 [23039] Add BuyerOrSellerTraderAutoCreate (BSTRC)
RD 20170403 [23040] Modify
FI 20170403 [23037] Modify
FI 20160929 [22507] Modify 
FI 20160502 [22107] Add RelatedPositionID (FIX.5.0SP2 EP208 tag 1862)
PL 20150916 [20882] Use MatDt FIXml Tag (FIX Tag 541) on Flexible Trade (TrdTyp 54=OTC)
FI 20140505 [19851] Add template GetTrtTyp
RD 20131210 [19159] Add 2 templates: ovrGetBuyerOrSellerData and ovrGetBuyerOrSellerTraderData    
BD 20131210 First version     

-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:erx="www.eurexchange.com/technology" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <xsl:template match="erx:FIXML">
    <file>
      <xsl:attribute name="name"/>
      <xsl:attribute name="folder"/>
      <xsl:attribute name="date"/>
      <xsl:attribute name="size"/>
      <xsl:attribute name="status">success</xsl:attribute>
      <xsl:apply-templates select="erx:TrdCaptRpt"/>
    </file>
  </xsl:template>

  <xsl:template match="erx:TrdCaptRpt">
    <xsl:variable name="idprefix" select="'r'"/>
    <xsl:variable name="idposition" select="position()"/>
    <row>
      <xsl:attribute name="id">
        <xsl:value-of select="$idprefix"/>
        <xsl:value-of select="$idposition"/>
      </xsl:attribute>
      <xsl:attribute name="src">
        <xsl:value-of select="$idposition"/>
      </xsl:attribute>
      <xsl:attribute name="status">success</xsl:attribute>

      <!-- FI 20160929 [22507] Add clearingFirm, executingFirm memberID, memberType-->
      <!-- FI 20170727 Add Commentaire 
           D'après la doc Eurex    erx:Pty[@R='1']/@ID =>  Exchange Member ID 
           D'après la doc Eurex    erx:Pty[@R='4']/@ID =>  Clearing Member ID 
           Exchange Member ID et Clearing Member ID  sont différents lorsqu'il s'agit  de l'activité d'un NCM
      -->

      <!-- clearingMemberID -->
      <xsl:variable name ="clearingFirm" select ="erx:Pty[@R='4']/@ID"/>
      <!-- FI 20231113 [24690] executingFirm doesn't have @Qual attribut -->
      <xsl:variable name ="executingFirm" select ="erx:Pty[@R='1' and not(@Qual)]/@ID"/>

      <!-- MemberID (Membre connecté qui recoit le flux) 
      erx:Hdr/@AcctMID est un attribut ajouté par la gateway Eurex FIXML 
      La gateway s'appuie sur l'élément AccountID présent dans le fichier de config (le compte déclaré pour la connexion vers le broker AMQP d'eurex) 
      Exemple: si le commpte est CCLXV_SPHERBOBBGATEWAY la gateway déduit que le membre connecté est CCLXV
      -->
      <xsl:variable name ="memberID" select ="erx:Hdr/@AcctMID"/>

      <!-- memberType (GCM = DCM or GCM, sinon NCM) -->
      <xsl:variable name ="memberType">
        <xsl:choose>
          <xsl:when test ="$memberID = $clearingFirm">
            <xsl:value-of select ="'GCM'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select ="'NCM'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <!-- FI 20170727 [23340] add variable isNCMOfGCM
      - true si la gateway est installée chez un GCM et elle reçoit le flux concernant un NCM -->
      <xsl:variable name ="isNCMOfGCM">
        <xsl:choose>
          <xsl:when test ="($memberType = 'GCM') and ($memberID != $executingFirm)">true</xsl:when>
          <xsl:otherwise>false</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <!-- FI 20170727 [23340] add variable IsNCMActivity
      - true si gateway installée chen un NCM 
      - true si gateway installée chez un GCM et que le flux concerne son NCM
      -->
      <xsl:variable name ="isNCMActivity">
        <xsl:choose>
          <xsl:when test ="($memberType = 'NCM') or ($isNCMOfGCM = 'true')">true</xsl:when>
          <xsl:otherwise>false</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <!-- Type du Trade (give-up, take-up ou normal) -->
      <xsl:variable name="tradeClass">
        <xsl:choose>
          <!-- give-up -->
          <xsl:when test="@TrnsfrRsn='020' or
                          @TrnsfrRsn='021' or
                          @TrnsfrRsn='043' or
                          @TrnsfrRsn='044'">give-up</xsl:when>
          <!-- take-up -->
          <xsl:when test="@TrnsfrRsn='030' or
                          @TrnsfrRsn='031' or
                          @TrnsfrRsn='035' or
                          @TrnsfrRsn='036' or
                          @TrnsfrRsn='046' or
                          @TrnsfrRsn='047' or
                          @TrnsfrRsn='048'">take-up</xsl:when>
          <!-- normal -->
          <xsl:otherwise>normal</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <!-- Trade Reverse -->
      <xsl:variable name="isReverse">
        <xsl:choose>
          <xsl:when test="@TransTyp='4'">true</xsl:when>
          <xsl:otherwise>false</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <!-- FI 20170727 [23340] Mise en commentaire da la variable $executingBroker -->
      <!-- ExecutingBroker -->
      <!--
      <xsl:variable name="executingBroker">
        <xsl:choose>
          <xsl:when test="$tradeClass='take-up'">
            <xsl:value-of select="erx:Pty[@R='95']/@ID"/>
          </xsl:when>
          <xsl:when test="$tradeClass='give-up'">
            <xsl:value-of select="erx:Pty[@R='96']/@ID"/>
          </xsl:when>
        </xsl:choose>
      </xsl:variable>-->

      <!-- category -->
      <!-- PM 20171026 [23567] Add variable category -->
      <xsl:variable name="category">
        <xsl:choose>
          <xsl:when test="erx:Instrmt/@OptAt">O</xsl:when>
          <xsl:otherwise>F</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <!-- Flex -->
      <!-- FI 20220228 [XXXXX] add-->
      <xsl:variable name="isFlex">
        <xsl:choose>
          <xsl:when test="erx:Instrmt/@FlexInd='Y'">true</xsl:when>
          <xsl:when test="erx:Instrmt/@FlexInd='N'">false</xsl:when>
          <xsl:when test="@TrdTyp='54'">true</xsl:when>
          <xsl:otherwise>false</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      
      <!-- ContractDate -->
      <!-- FI 20220113 [25699] add -->
      <xsl:variable name="isContractDate">
        <xsl:choose>
          <xsl:when test="string-length(erx:Instrmt/@ContractDate)>0 and erx:Hdr/@SID='ECAG'">true</xsl:when>
          <xsl:otherwise>false</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <!-- ALLOWDATAMISSDEALER -->
      <data>
        <xsl:attribute name="name">ALLOWDATAMISSDEALER</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="'Allowed'"/>
      </data>

      <!-- RecordVersion -->
      <!-- PM 20171026 [23567] Add RecordVersion -->
      <!-- FI 20220218 [XXXXX] use of version 3-->
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
        <xsl:choose>
          <!-- Give-up : Update -->
          <xsl:when test="$tradeClass='give-up'">
            <xsl:value-of select="'M'"/>
          </xsl:when>
          <xsl:when test="$isReverse='true'">
            <!-- Suppression -->
            <xsl:value-of select="'S'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'N'"/>
          </xsl:otherwise>
        </xsl:choose>
      </data>

      <!-- TradeID -->
      <data>
        <xsl:attribute name="name">TRID</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:choose>
          <xsl:when test="$isReverse='true'">
            <xsl:value-of select="concat(@RptRefID,'-',erx:RptSide/erx:TrdRptOrdDetl/@OrdID)"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:choose>
              <xsl:when test="$tradeClass='normal'">
                <xsl:value-of select="concat(@RptID,'-',erx:RptSide/erx:TrdRptOrdDetl/@OrdID)"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="concat(@RptRefID,'-',erx:RptSide/erx:TrdRptOrdDetl/@OrdID)"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:otherwise>
        </xsl:choose>
      </data>

      <!-- SpheresProduct -->
      <!-- PM 20171026 [23567] Add SpheresProduct -->
      <data>
        <xsl:attribute name="name">PRD</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="'exchangeTradedDerivative'"/>
      </data>

      <!-- SpheresInstrument -->
      <!-- PM 20171026 [23567] Add SpheresInstrument -->
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
      <!-- PM 20171026 [23567] Add SpheresInstrumentIdent -->
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
      <data>
        <xsl:attribute name="name">SPT</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="'Ignore'"/>
      </data>

      <!-- Facility -->
      <!-- PM 20171026 [23567] Add FAC -->
      <data>
        <xsl:attribute name="name">FAC</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="@LastMkt"/>
      </data>

      <!-- FacilityIdent -->
      <!-- PM 20171026 [23567] Add FACI -->
      <data>
        <xsl:attribute name="name">FACI</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="'SpheresISO10383'"/>
      </data>

      <!-- ExecutionDateTime -->
      <!-- PM 20171026 [23567] Rename TDT (TransactionDate) in EDT (ExecutionDateTime) et ajustement du format -->
      <data>
        <!--<xsl:attribute name="name">TDT</xsl:attribute>-->
        <xsl:attribute name="name">EDT</xsl:attribute>
        <xsl:attribute name="datatype">datetime</xsl:attribute>
        <!-- RD 20170403 [23040] Use of 'executiontime' as timestamp -->
        <!--<xsl:value-of select="@TrdDt"/>-->
        <!-- timestamp -->
        <!-- PM 20171026 [23567] Les dates et heures peuvent être considérées directement sans transformation car déjà au format ISO 8601 étendu-->
        <!--<xsl:variable name="timestamp">
          -->
        <!--  valeurs possibles pour erx:TrdRegTS/@Typ
                1=Execution Time, 
                2=Time In (Time when the transaction arrived on the clearing layer), 
                7=Submission to Clearing (Time when the transaction was successfully booked on the clearing layer)
          -->
        <!--
          <xsl:variable name="executiontime" select="erx:TrdRegTS[@Typ='1']/@TS"/>
          <xsl:choose>
            -->
        <!-- Si executiontime existe -->
        <!--
            <xsl:when test="$executiontime">
              <xsl:variable name="hour">
                <xsl:value-of select="substring($executiontime,12,2)"/>
              </xsl:variable>
              <xsl:variable name="minute">
                <xsl:value-of select="substring($executiontime,15,2)"/>
              </xsl:variable>
              <xsl:variable name="second">
                <xsl:value-of select="substring($executiontime,18,2)"/>
              </xsl:variable>
              <xsl:value-of select="concat($hour,':',$minute,':',$second)"/>
            </xsl:when>
            -->
        <!-- Si executiontime n'existe pas -->
        <!--
            <xsl:otherwise>00:00:00</xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        -->
        <!-- datetime format ISO 8601 étendu yyyy-MM-ddTHH:mm:ss -->
        <!--
        <xsl:value-of select="concat(@TrdDt,'T',$timestamp)"/>-->

        <!--  valeurs possibles pour erx:TrdRegTS/@Typ
                1=Execution Time, 
                2=Time In (Time when the transaction arrived on the clearing layer), 
                7=Submission to Clearing (Time when the transaction was successfully booked on the clearing layer)
          -->
        <xsl:variable name="executiontime" select="erx:TrdRegTS[@Typ='1']/@TS"/>
        <xsl:choose>
          <xsl:when test="$executiontime">
            <!-- datetime format ISO 8601 étendu yyyy-MM-ddTHH:mm:ss.sss -->
            <xsl:value-of select="substring-before($executiontime,'+')"/>
          </xsl:when>
          <!-- Si executiontime n'existe pas, prendre TradeDate -->
          <xsl:otherwise>
            <!-- datetime format ISO 8601 étendu yyyy-MM-ddTHH:mm:ss -->
            <xsl:value-of select="concat(@TrdDt,'T00:00:00')"/>
          </xsl:otherwise>
        </xsl:choose>
      </data>

      <!-- ExecutionTimeZone -->
      <!-- PM 20171026 [23567] Add ExecutionTimeZone -->
      <!-- FI 20210804 [XXXXX] Réécriture du choose et ajout de commentaires (aucun changement fonctionnel) -->
      <data>
        <xsl:attribute name="name">EDTTZ</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:choose>
          <!-- EUREX: From C7 the timestamps are in UTC -->
          <xsl:when test="erx:Hdr/@SID = 'ECAG'">Etc/UTC</xsl:when>
          <!-- ECC  : From C7 the timestamps are in UTC -->
          <xsl:when test="erx:Hdr/@SID = 'ECC'">Etc/UTC</xsl:when>
          <!-- CCP  : Under Release 14 the timestamps were on Berlin -->
          <xsl:when test="erx:Hdr/@SID = 'CCP'">Europe/Berlin</xsl:when>
          <xsl:otherwise>Etc/UTC</xsl:otherwise>
        </xsl:choose>
      </data>




      <!-- BusinessDate -->
      <!-- PM 20171026 [23567] Rename CDT in BDT -->
      <data>
        <!--<xsl:attribute name="name">CDT</xsl:attribute>-->
        <xsl:attribute name="name">BDT</xsl:attribute>
        <xsl:attribute name="datatype">date</xsl:attribute>
        <xsl:value-of select="@BizDt"/>
      </data>

      <!-- Comment -->
      <data>
        <xsl:attribute name="name">CMT</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="erx:RptSide/@Txt1"/>
      </data>

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
      <!-- FI 20220218 [25699] Add ClearingHouse,ClearingHouseIdent -->
      <!-- ClearingHouse,ClearingHouseIdent -->
      <data>
        <xsl:attribute name="name">CLH</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:call-template name="GetClearingHouseMicCode"/>
      </data>
      <data>
        <xsl:attribute name="name">CLHI</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="'SpheresISO10383'"/>
      </data>

      <!-- Market,MarketIdent -->
      <!-- FI 20160929 [22507] Remove MKT, MKTI
           Le marché est désormais tjs recherché à partir du Derivative Contract
           Cette recherche est effectuée dans le XSL de mapping
      <data>
        <xsl:attribute name="name">MKT</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="'XEUR'"/>
      </data>
      <data>
        <xsl:attribute name="name">MKTI</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="'SpheresISO10383'"/>
      </data>
      -->

      <!-- Contract -->
      <!-- PM 20171026 [23567] Rename DVT in CTR -->
      <data>
        <!--<xsl:attribute name="name">DVT</xsl:attribute>-->
        <xsl:attribute name="name">CTR</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <!-- SI 20220223 [25951] Use ProdCmplx instead of Sym on Flex-->
        <!--<xsl:value-of select="erx:Instrmt/@Sym"/>-->
        <xsl:choose>
          <xsl:when test="$isFlex='true'">
            <xsl:value-of select ="erx:Instrmt/@ProdCmplx"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="erx:Instrmt/@Sym"/>
          </xsl:otherwise>
        </xsl:choose>
      </data>

      <!-- ContractIdent -->
      <!-- PM 20171026 [23567] Rename DVTI in CTRI -->
      <data>
        <!--<xsl:attribute name="name">DVTI</xsl:attribute>-->
        <xsl:attribute name="name">CTRI</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="'SpheresContractCode'"/>
      </data>

      <!-- ContractVersion -->
      <!-- PM 20171026 [23567] Rename DVTV in CTRV -->
      <data>
        <!--<xsl:attribute name="name">DVTV</xsl:attribute>-->
        <xsl:attribute name="name">CTRV</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="erx:Instrmt/@OptAt"/>
      </data>

      <!-- ContractCategory -->
      <!-- PM 20171026 [23567] Use of $category and Rename DVTC in CTRC-->
      <data>
        <!--<xsl:attribute name="name">DVTC</xsl:attribute>-->
        <xsl:attribute name="name">CTRC</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <!--<xsl:choose>
          <xsl:when test="erx:Instrmt/@OptAt">O</xsl:when>
          <xsl:otherwise>F</xsl:otherwise>
        </xsl:choose>-->
        <xsl:value-of select="$category"/>
      </data>

      <!-- Contract Type-->
      <!-- FI 20220228 [25699] Alimentation de CTRT -->
      <data>
        <xsl:attribute name="name">CTRT</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:choose>
          <xsl:when test="$isFlex='true'">FLEX</xsl:when>
          <xsl:otherwise>STD</xsl:otherwise>
        </xsl:choose>
      </data>

      <!-- ContractPutCall -->
      <!-- PM 20171026 [23567] Rename DVTO in CTRT -->
      <!-- FI 20220218 [25699] Rename CTRT in CTRK -->
      <!-- FI 20220628 [25699] Rename CTRK in CTRPC -->
      <data>
        <!--<xsl:attribute name="name">DVTO</xsl:attribute>-->
        <xsl:attribute name="name">CTRPC</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="erx:Instrmt/@PutCall"/>
      </data>

      <!-- ContractSettlementMethod -->
      <!-- FI 20220215 [25699] Alimentation de CTRSM -->
      <data>
        <xsl:attribute name="name">CTRSM</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="erx:Instrmt/@SettlMeth"/>
      </data>

      <!-- ContractExerciseStyle -->
      <!-- FI 20220215 [25699] Alimentation de CTRES -->
      <data>
        <xsl:attribute name="name">CTRES</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="erx:Instrmt/@ExerStyle"/>
      </data>

      <!-- ContractMaturity -->
      <!-- PM 20171026 [23567] Rename DVTM in CTRM -->
      <data>
        <!--<xsl:attribute name="name">DVTM</xsl:attribute>-->
        <xsl:attribute name="name">CTRM</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <!-- FI 20220113 [25699] test su $isContractDate -->
        <xsl:choose>
          <xsl:when test="$isContractDate='true'">
            <xsl:variable name="contractDate" select="erx:Instrmt/@ContractDate"/>
            <xsl:value-of select="concat(substring($contractDate,1,4),substring($contractDate,6,2),substring($contractDate,9,2))"/>
          </xsl:when>
          <xsl:otherwise>
            <!-- PL 20150916 [20882] use MatDt MaturityDate (witch format is YYYY-MM-DD) on flex -->
            <!--<xsl:value-of select="erx:Instrmt/@MMY"/>-->
            <xsl:choose>
              <xsl:when test="$isFlex='true'">
                <xsl:variable name="MatDt" select="erx:Instrmt/@MatDt"/>
                <xsl:value-of select="concat(substring($MatDt,1,4),substring($MatDt,6,2),substring($MatDt,9,2))"/>
              </xsl:when>
              <xsl:otherwise>
                <!-- MMY MaturityMonthYear YYYYMM -->
                <xsl:value-of select="erx:Instrmt/@MMY"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:otherwise>
        </xsl:choose>
      </data>

      <!-- ContractStrike -->
      <!-- PM 20171026 [23567] Rename DVTS in CTRS -->
      <data>
        <!--<xsl:attribute name="name">DVTS</xsl:attribute>-->
        <xsl:attribute name="name">CTRS</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="erx:Instrmt/@StrkPx"/>
      </data>

      <!-- BuyerOrSellerAccount -->
      <xsl:call-template name="ovrGetBuyerOrSellerData">
        <xsl:with-param name="pData" select="erx:RptSide/@Txt2"/>
      </xsl:call-template>

      <!-- BuyerOrSellerTrader -->
      <!-- FI 20170727 [23340] Trader sur le dealer si activité d'un NCM chez un GCM (la gateway est installée chez le GCM)
           si trade nomal ou give-up (sauf si le give-up fait suite à un take-up) 
           erx:Pty[@R='95']/@ID => Give-up (Trading Firm) [le membre qui a alloué les lots]
      -->
      <xsl:if test="$isNCMOfGCM = 'true' and (($tradeClass='normal') or (($tradeClass='give-up' and not(erx:Pty[@R='95']/@ID))))">
        <xsl:if test ="erx:Pty[@R='11']">
          <xsl:call-template name="ovrGetBuyerOrSellerTraderData">
            <xsl:with-param name="pData">
              <xsl:call-template name="GetTraderId">
                <xsl:with-param name="pTraderPty" select ="erx:Pty[@R='11']/@ID"/>
              </xsl:call-template>
            </xsl:with-param>
          </xsl:call-template>
        </xsl:if>
      </xsl:if>

      <!-- BuyerOrSellerNegociationBrokerTrader -->
      <!-- FI 20170727 [23340] Trader sur entité si activité d'un GCM (gateway installée chez GCM)  ou si activité d'un NCM (gateway installée chez NCM)
           si trade nomal ou si give-up (sauf si le give-up fait suite à un take-up)  
      -->
      <xsl:choose>
        <xsl:when test="$isNCMOfGCM = 'true' and (($tradeClass='normal') or (($tradeClass='give-up' and not(erx:Pty[@R='95']/@ID))))"/>
        <!-- Ds cas le trader se trouve sur l'acteur dealer  => pas de trader sur l'entité de gestion  -->
        <xsl:when test="erx:Pty[@R='95']/@ID"/>
        <!-- Ds cas le trade a été alloué au membre détenteur de Spheres (take-up) 
        => pas de trader sur l'entité de gestion  -->
        <xsl:otherwise>
          <!-- otherwise => Activité maison/client (que Spheres® soit installé chez un NCM ou un GCM)  -->
          <xsl:if test ="erx:Pty[@R='11']">
            <xsl:call-template name ="ovrGetBuyerOrSellerNegociationBrokerTraderData">
              <xsl:with-param name="pData">
                <xsl:call-template name="GetTraderId">
                  <xsl:with-param name="pTraderPty" select ="erx:Pty[@R='11']/@ID"/>
                </xsl:call-template>
              </xsl:with-param>
            </xsl:call-template>
          </xsl:if>
        </xsl:otherwise>
      </xsl:choose>

      <xsl:choose>
        <!-- BuyerOrSellerExecutingBroker si take-up -->
        <xsl:when test="$tradeClass='take-up'">
          <!-- FI 20170727 [23340] Lecture de erx:Pty[@R='95']
          => L'acteur 95 Give-up (Trading) Firm, d'où proviennent les lots -->
          <data>
            <xsl:attribute name="name">BSEB</xsl:attribute>
            <xsl:attribute name="datatype">string</xsl:attribute>
            <!-- FI 20170727 [23340] use erx:Pty[@R='95'] -->
            <xsl:value-of select="erx:Pty[@R='95']/@ID"/>
          </data>
          <data>
            <xsl:attribute name="name">BSEBI</xsl:attribute>
            <xsl:attribute name="datatype">string</xsl:attribute>
            <xsl:value-of select="'SpheresExtlLink'"/>
          </data>
        </xsl:when>

        <!-- BuyerOrSellerClearingBroker si give-up -->
        <xsl:when test="$tradeClass='give-up'">
          <!-- Clearing Broker  -->
          <data>
            <xsl:attribute name="name">BSCB</xsl:attribute>
            <xsl:attribute name="datatype">string</xsl:attribute>
            <xsl:value-of select="erx:Pty[@R='96']/@ID"/>
          </data>
          <data>
            <xsl:attribute name="name">BSCBI</xsl:attribute>
            <xsl:attribute name="datatype">string</xsl:attribute>
            <xsl:value-of select="'SpheresExtlLink'"/>
          </data>
          <xsl:if test="erx:Pty[@R='95']/@ID">
            <!-- Executing Broker renseigé si le give-up fait suite à un take-up -->
            <data>
              <xsl:attribute name="name">BSEB</xsl:attribute>
              <xsl:attribute name="datatype">string</xsl:attribute>
              <xsl:value-of select="erx:Pty[@R='95']/@ID"/>
            </data>
            <data>
              <xsl:attribute name="name">BSEBI</xsl:attribute>
              <xsl:attribute name="datatype">string</xsl:attribute>
              <xsl:value-of select="'SpheresExtlLink'"/>
            </data>
          </xsl:if>
        </xsl:when>
      </xsl:choose>

      <!--<xsl:if test ="string-length($executingBroker)>0">
        <data>
          <xsl:attribute name="name">BSEB</xsl:attribute>
          <xsl:attribute name="datatype">string</xsl:attribute>
          <xsl:value-of select="erx:Pty[@R='95']/@ID"/>
        </data>
        <data>
          <xsl:attribute name="name">BSEBI</xsl:attribute>
          <xsl:attribute name="datatype">string</xsl:attribute>
          <xsl:value-of select="'SpheresExtlLink'"/>
        </data>
      </xsl:if>-->

      <!-- BuyerOrSellerClearingOrganisation -->
      <!-- RD 20171211 [23639] Déplacer le code dans le template ovrGetBuyerOrSellerClearingntOrganisationData-->
      <xsl:call-template name="ovrGetBuyerOrSellerClearingntOrganisationData">
        <xsl:with-param name="pMemberType" select="$memberType"/>
        <xsl:with-param name="pClearingFirm" select="$clearingFirm"/>
        <xsl:with-param name="pTradeClass" select="$tradeClass"/>
      </xsl:call-template>

      <!-- BuyerOrSellerTrader -->
      <!-- BuyerOrSellerTraderIdent -->
      <!-- FI 20170727 [23340] Mise en commentaire Le trader n'est plus systématiquement sur le dealer -->
      <!--<xsl:call-template name="ovrGetBuyerOrSellerTraderData">
        <xsl:with-param name="pTradeClass" select="$tradeClass"/>
        <xsl:with-param name="pData" select="erx:Pty[@R='12']/@ID"/>
      </xsl:call-template>-->

      <!-- ExecutionId -->
      <data>
        <xsl:attribute name="name">EXID</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="@RptID"/>
      </data>

      <!-- AllocationSide -->
      <data>
        <xsl:attribute name="name">ALS</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="erx:RptSide/@Side"/>
      </data>

      <!-- AllocationQuantity -->
      <data>
        <xsl:attribute name="name">ALQ</xsl:attribute>
        <xsl:attribute name="datatype">integer</xsl:attribute>
        <xsl:value-of select="@LastQty"/>
      </data>

      <!-- AllocationPrice -->
      <!-- PM 20171026 [23567] Rename PRI in ALP -->
      <data>
        <!--<xsl:attribute name="name">ALP</xsl:attribute>-->
        <xsl:attribute name="name">PRI</xsl:attribute>
        <xsl:attribute name="datatype">decimal</xsl:attribute>
        <xsl:value-of select="@LastPx"/>
      </data>

      <!-- AllocationInputDevice -->
      <data>
        <xsl:attribute name="name">ALD</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="erx:RptSide/@InptDev"/>
      </data>

      <!-- PositionEffect -->
      <data>
        <xsl:attribute name="name">ALPE</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="erx:RptSide/@PosEfct"/>
      </data>

      <!-- OrderElectId -->
      <data>
        <xsl:attribute name="name">OEID</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="erx:RptSide/erx:TrdRptOrdDetl/@OrdID"/>
      </data>

      <!-- OrderElectQuantity -->
      <!-- PM 20171026 [23567] Rename OEQT in OEQ and fill value -->
      <data>
        <!--<xsl:attribute name="name">OEQT</xsl:attribute>-->
        <xsl:attribute name="name">OEQ</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <!--<xsl:value-of select="'TBD'"/>-->
        <xsl:value-of select="erx:RptSide/erx:TrdRptOrdDetl/erx:OrdQty/@Qty"/>
      </data>

      <!-- OrderElectType -->
      <data>
        <xsl:attribute name="name">OET</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="erx:RptSide/erx:TrdRptOrdDetl/@OrdTyp"/>
      </data>

      <!-- OrderElecInputDevise -->
      <data>
        <xsl:attribute name="name">OED</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="erx:RptSide/erx:TrdRptOrdDetl/@OrdInptDev"/>
      </data>

      <!-- OrderElectCategory -->
      <data>
        <xsl:attribute name="name">OEC</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="erx:RptSide/@OrdCat"/>
      </data>

      <!-- OrderElectStatus -->
      <data>
        <xsl:attribute name="name">OES</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="erx:RptSide/erx:TrdRptOrdDetl/@OrdStat"/>
      </data>

      <!-- ExecutionType -->
      <data>
        <xsl:attribute name="name">EXT</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="@ExecTyp"/>
      </data>

      <!-- AllocationType -->
      <data>
        <xsl:attribute name="name">ALT</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <!--<xsl:value-of select="@TrdTyp"/>-->
        <xsl:call-template name ="GetTrtTyp">
          <xsl:with-param name="pTrtTyp">
            <xsl:value-of select="@TrdTyp"/>
          </xsl:with-param>
        </xsl:call-template>
      </data>

      <!-- AllocationSubType -->
      <data>
        <xsl:attribute name="name">ALST</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="@TrdSubTyp"/>
      </data>

      <!-- AllocationInputSource -->
      <data>
        <xsl:attribute name="name">ALSRC</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="erx:RptSide/@InptScr"/>
      </data>

      <!-- AllocationSecondaryType -->
      <data>
        <xsl:attribute name="name">ALT2</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="@TrdTyp2"/>
      </data>

      <!-- RelatedPositionID -->
      <data>
        <xsl:attribute name="name">RPID</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="erx:RptSide/erx:ReltdPos/@ID"/>
      </data>

      <!-- FI 20160929 [22507] Add-->
      <!-- MemberId (Identifiant chambre du membre connecté) -->
      <data>
        <xsl:attribute name="name">MID</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="$memberID"/>
      </data>

      <!-- FI 20160929 [22507] Add-->
      <!-- MemberId Type (Valeurs Possible GCM ou NCM)  -->
      <data>
        <xsl:attribute name="name">MIDTYP</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="$memberType"/>
      </data>

      <!-- FI 20160929 [22507] Add-->
      <!-- ISNCMACTIVITY -->
      <data>
        <xsl:attribute name="name">ISNCMACTIVITY</xsl:attribute>
        <xsl:attribute name="datatype">bool</xsl:attribute>
        <!-- FI 20170727 [23340] Lecture de le variable isNCMActivity -->
        <!--<xsl:choose>
          <xsl:when test ="$memberType = 'NCM'">true</xsl:when>
          <xsl:when test ="($memberType = 'GCM') and ($clearingFirm != $executingFirm)">true</xsl:when>
          <xsl:otherwise>false</xsl:otherwise>
        </xsl:choose>-->
        <xsl:value-of select="$isNCMActivity"/>
      </data>
    </row>

  </xsl:template>

  <!-- ============================================================== -->
  <!--        ovrGetBuyerOrSellerData                                 -->
  <!-- ============================================================== -->
  <xsl:template name="ovrGetBuyerOrSellerData">
    <xsl:param name="pData"/>

    <!-- BuyerOrSellerAccount -->
    <data>
      <xsl:attribute name="name">BSA</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:choose>
        <xsl:when test="string-length($pData)>0">
          <xsl:value-of select="$pData"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'{NotSpecified}'"/>
        </xsl:otherwise>
      </xsl:choose>
    </data>

    <!-- BuyerOrSellerAccountIDent -->
    <data>
      <xsl:attribute name="name">BSAI</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'SpheresExtlLink'"/>
    </data>
  </xsl:template>

  <!-- ============================================================== -->
  <!--        ovrGetBuyerOrSellerClearingntOrganisationData           -->
  <!-- ============================================================== -->
  <!-- RD 20171211 [23639] Add-->
  <xsl:template name="ovrGetBuyerOrSellerClearingntOrganisationData">
    <xsl:param name="pMemberType"/>
    <xsl:param name="pClearingFirm"/>
    <xsl:param name="pTradeClass"/>

    <!-- FI 20160929 [22507] Mise en commentaire
      <data>
        <xsl:attribute name="name">BSCO</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="'EUXCDEFF'"/>
      </data>
      <data>
        <xsl:attribute name="name">BSCOI</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="'BIC'"/>
      </data>
      -->

    <!-- FI 20160929 [22507] Modify BSCO, BSCOI
           Si le membre connecté est NCM 
           => Alimentation de BSCO, BSCOI avec le GCM  ($clearingMemberID)
           sinon (membre connecté est GCM ou DCM) 
           => pas d'alimentation de BSCO, BSCOI (recherche du CSS à partir du DC. Cette recherche est effectuée dans le XSL de mapping) 
      -->
    <xsl:if test ="$pMemberType = 'NCM'">
      <!-- BuyerOrSellerClearingOrganisation -->
      <data>
        <xsl:attribute name="name">BSCO</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="$pClearingFirm"/>
      </data>

      <!-- BuyerOrSellerClearingOrganisationIdent -->
      <data>
        <xsl:attribute name="name">BSCOI</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="'SpheresExtlLink'"/>
      </data>
    </xsl:if>


    <!-- BuyerOrSellerClearingOrganisationAccount -->
    <!-- FI 20160929 [22507] Modify 
           Book clearing : {ExecutingFirm}_{Position Account}
      -->
    <!--<data>
        <xsl:attribute name="name">BSCA</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="erx:Pty[@R='1']/@ID"/>
      </data>-->
    <!-- PM 20171026 [23567] Rename BSCA in BSCOA -->
    <data>
      <!--<xsl:attribute name="name">BSCA</xsl:attribute>-->
      <xsl:attribute name="name">BSCOA</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <!-- FI 20231113 [24690] executingFirm doesn't have @Qual attribut -->
      <xsl:value-of select="erx:Pty[@R='1' and not(@Qual)]/@ID"/>
      <xsl:value-of select="'_'"/>
      <xsl:value-of select="erx:Pty[@R='38']/@ID"/>
      <!-- FI 20170403 [23037] Dans le cas des Give-up le book côté clearing est suffixé par '.GU'-->
      <xsl:if test="$pTradeClass='give-up'">
        <xsl:value-of select="'.GU'"/>
      </xsl:if>
    </data>

    <!-- BuyerOrSellerClearingOrganisationAccountIdent -->
    <!-- PM 20171026 [23567] Rename BSCAI in BSCOAI -->
    <data>
      <!--<xsl:attribute name="name">BSCAI</xsl:attribute>-->
      <xsl:attribute name="name">BSCOAI</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'SpheresExtlLink'"/>
    </data>
  </xsl:template>

  <!-- ============================================================== -->
  <!--        ovrGetBuyerOrSellerTraderData                           -->
  <!-- ============================================================== -->
  <!-- FI 20170428 [23039] Modify -->
  <!-- FI 20170727 [23340] Modify -->
  <xsl:template name="ovrGetBuyerOrSellerTraderData">
    <!-- FI 20170727 [23340] paramètre pTradeClass abandonné -->
    <!--<xsl:param name="pTradeClass"/>-->
    <xsl:param name="pData"/>

    <!-- BuyerOrSellerTrader -->
    <data>
      <xsl:attribute name="name">BSTR</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <!-- FI 20170727 [23340] Mise en commentaire du if -->
      <!--<xsl:if test="$pTradeClass != 'take-up'">-->
      <xsl:value-of select="$pData"/>
      <!--</xsl:if>-->
    </data>

    <!-- BuyerOrSellerTraderIdent -->
    <data>
      <xsl:attribute name="name">BSTRI</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'SpheresExtlLink'"/>
    </data>

    <!-- FI  20170428 [23039] add BuyerOrSellerTraderAutoCreate -->
    <!-- BuyerOrSellerTraderAutoCreate -->
    <data>
      <xsl:attribute name="name">BSTRC</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <!-- FI 20170727 [23340] autocréation du trader OUI -->
      <xsl:value-of select="'Y'"/>
    </data>

  </xsl:template>

  <!-- ============================================================== -->
  <!--        ovrGetBuyerOrSellerNegociationBrokerTraderData         -->
  <!-- ============================================================== -->
  <!-- FI 20170727 [23340] add -->
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

  <!-- ============================================================== -->
  <!--        ovrGetBuyerOrSellerExecutingBrokerTraderData            -->
  <!-- ============================================================== -->
  <!-- FI 20170727 [23340] add -->
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
  <!--        ovrGetBuyerOrSellerClearingBrokerTraderData            -->
  <!-- ============================================================== -->
  <!-- FI 20170727 [23340] add -->
  <xsl:template name="ovrGetBuyerOrSellerClearingBrokerTraderData">
    <xsl:param name="pData"/>
    <!-- BuyerOrSellerClearingBrokerTrader -->
    <data>
      <xsl:attribute name="name">BSCBTR</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="$pData"/>
    </data>

    <!-- BuyerOrSellerClearingBrokerTraderIdent -->
    <data>
      <xsl:attribute name="name">BSCBTRI</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'SpheresExtlLink'"/>
    </data>

    <!-- BuyerOrSellerClearingBrokerTraderAutoCreate -->
    <data>
      <xsl:attribute name="name">BSCBTRC</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'Y'"/>
    </data>
  </xsl:template>


  <!-- FI 20170727 [23340] add Template 
  Retourne le trader selon le modèle suivant {MemberID}_{TRADERID}
  Dans le flux le moèle est le suivant {MemberID}{TRADERID} 
  Exemple ABCFRTRD001 devient ABCFR_TRD001 => cet acteur pourra être auto-alimenté dans Spheres®
  -->
  <xsl:template name ="GetTraderId">
    <xsl:param name="pTraderPty"/>
    <xsl:value-of select="concat(substring($pTraderPty,1,5),'_',substring($pTraderPty,6))"/>
  </xsl:template>

  <xsl:template name ="GetTrtTyp">
    <xsl:param name="pTrtTyp"/>
    <xsl:choose>
      <!-- TrdType Eurex 1000 devient 1100,TrdType Eurex 1001 devient 1101,etc -->
      <!-- Dans Spheres® les trdType à compter de 1100 sont reservés à Eurex -->
      <xsl:when test="number($pTrtTyp)>=1000">
        <xsl:value-of select="1100 + number($pTrtTyp) - 1000"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pTrtTyp"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- FI 20220304 [25699]  Add Template
  The list  available via the following direct link: https://www.iso20022.org/sites/default/files/ISO10383_MIC/ISO10383_MIC.xls
  -->
  <xsl:template name="GetClearingHouseMicCode">
    <xsl:choose>
      <xsl:when test="erx:Hdr/@SID ='ECC'">
        <!-- EUROPEAN COMMODITY CLEARING AG (Acronym : ECC, MIC: XECC) -->
        <xsl:value-of select="'XECC'"/>
      </xsl:when>
      <xsl:otherwise>
        <!-- EUREX CLEARING AG (MIC : ECAG) -->
        <xsl:value-of select="erx:Hdr/@SID"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>