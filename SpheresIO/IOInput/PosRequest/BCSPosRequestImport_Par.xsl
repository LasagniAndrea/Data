<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes"
               media-type="text/xml; charset=ISO-8859-1"/>

<!--
=============================================================================================
Summary : PosRequest input - BCS Gateway parsing
          
File    : BCSPosRequestImport_Par.xsl
=============================================================================================

RD 20140324 [19704] include only the xsl "../Common/BCSTools.xsl" because it also included the 
                  xsl "../Common/ImportTools.xsl"
FI 20190924 [18990] gestion des assignations
FI 20220914 [XXXXX] Adaptation au format d'import V3 (dispo avec Spheres V12)
=============================================================================================
  -->

  <!-- ================================================== -->
  <!--        include(s)                                  -->
  <!-- ================================================== -->
  <xsl:include href="..\Common\BCSTools.xsl"/>

  <!-- ================================================== -->
  <!--        Noeud Root                                  -->
  <!-- ================================================== -->
  <xsl:template match="/">
    <xsl:call-template name="BuildFile"/>
  </xsl:template>

  <!-- ================================================== -->
  <!--        <BCSMessageList>                            -->
  <!-- ================================================== -->
  <!-- Match before with <BCSMessageList> -->
  <xsl:template match="BCSMessageList">
    <xsl:apply-templates select="BCSMessage"/>
  </xsl:template>

  <!-- ================================================== -->
  <!--        <BCSMessage>                                -->
  <!-- ================================================== -->
  <xsl:template match="BCSMessage">

    <xsl:variable name="vMessageclass">
      <xsl:value-of select="messageclass"/>
    </xsl:variable>

    <!-- FI 20130923 [18990] Seules les assignations sont gérées -->
    <xsl:variable name="vIsOk">
      <xsl:choose>
        <xsl:when test="$vMessageclass='NotifyAssignments'">
          <xsl:value-of select ="$ConstTrue"/>
        </xsl:when>
        <!-- FI 20130923 [18990] code suivant à activer lors de la gestion des exercices -->
        <!--<xsl:when test="($vMessageclass='NotifyEarlyExercises' or $vMessageclass='NotifyExByEx') 
                      and datafields/data[@name='RequestState']='P'">$ConstTrue</xsl:when>-->
        <xsl:otherwise>
          <xsl:value-of select ="$ConstFalse"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:if test="$vIsOk = $ConstTrue">
      <xsl:call-template name="MatchDatafields">
        <xsl:with-param name="pMessageclass" select="$vMessageclass"/>
      </xsl:call-template>
    </xsl:if>

  </xsl:template>


  <!-- ================================================== -->
  <!--        MatchDatafields                             -->
  <!-- ================================================== -->
  <xsl:template name="MatchDatafields">
    <xsl:param name="pMessageclass"/>
    <row>
      <xsl:variable name="idprefix" select="'r'"/>
      <xsl:variable name="idposition" select="position()"/>
      <xsl:attribute name="id">
        <xsl:value-of select="$idprefix"/>
        <xsl:value-of select="$idposition"/>
      </xsl:attribute>
      <xsl:attribute name="src">
        <xsl:value-of select="$idposition"/>
      </xsl:attribute>
      <xsl:attribute name="status">success</xsl:attribute>

      <xsl:apply-templates select="datafields">
        <xsl:with-param name="pMessageclass" select="$pMessageclass"/>
      </xsl:apply-templates>
    </row>
  </xsl:template>


  <!-- ================================================== -->
  <!--        <datafields>                                -->
  <!-- ================================================== -->
  <xsl:template match="datafields">
    <!-- messageclass -->
    <xsl:param name="pMessageclass"/>

    <xsl:variable name ="vRequestType">
      <xsl:choose>
        <xsl:when test="$pMessageclass='NotifyEarlyExercises' or 
                        $pMessageclass='NotifyExByEx' or 
                        $pMessageclass='NotifyExerciseAtExpiry'">
          <xsl:value-of select ="'EXE'"/>
        </xsl:when>
        <xsl:when test="$pMessageclass='NotifyAssignments'">
          <xsl:value-of select ="'ASS'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="ConstNA"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    
    <!-- RecordVersion -->
    <data name="RVR" datatype="integer">
      <xsl:value-of select="3"/>
    </data>

    <!-- RecordType (B for BODY)-->
    <data name="RTP" datatype="string" >
      <xsl:value-of select="'B'"/>
    </data>

    <!-- ActionType -->
    <data name="ATP" datatype="string">
      <xsl:value-of select="'M'"/>
    </data>

    <!-- PosRequestId -->
    <!-- identifier -->
    <data name="PRID" datatype="string">
      <xsl:call-template name ="GetTradeID">
        <xsl:with-param name ="pRequestType" select ="$vRequestType"/>
      </xsl:call-template>
    </data>

    <!-- SpheresProduct -->
    <data name="PRD" datatype="string">exchangeTradedDerivative</data>
    <!-- SpheresInstrument -->
    <data name="INS" datatype="string">ExchangeTradedOption</data>
    <!-- SpheresInstrumentIdent -->
    <data name="INSI" datatype="string">SpheresIdentifier</data>


    <!-- RD 20200117 [25114] Add SpheresFeeCalculation=Apply -->
    <!-- SpheresFeeCalculation -->
    <data>
      <xsl:attribute name="name">SFC</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'Apply'"/>
    </data>

    <!-- exercisedate/assignmentdate -->
    <xsl:choose>
      <xsl:when test ="$vRequestType='EXE'">
        <xsl:if test ="string-length(data[@name='exercisedate'])>0">
          <xsl:call-template name ="DataBDT">
            <xsl:with-param name="pDateYYYYMMDD">
              <xsl:value-of select="data[@name='exercisedate']"/>
            </xsl:with-param>
          </xsl:call-template>
        </xsl:if>
      </xsl:when>
      <xsl:when test ="$vRequestType='ASS'">
        <xsl:call-template name ="DataBDT">
          <xsl:with-param name="pDateYYYYMMDD">
            <xsl:value-of select="data[@name='assignmentdate']"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise/>
    </xsl:choose>


    <!-- RequestType -->
    <data>
      <xsl:attribute name="name">RQT</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="$vRequestType"/>
    </data>

    <!-- RequestMode -->
    <data>
      <xsl:attribute name="name">RQM</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="'EOD'"/>
    </data>

    <!-- RequestQty -->
    <data>
      <xsl:attribute name="name">QTY</xsl:attribute>
      <xsl:attribute name="datatype">integer</xsl:attribute>
      <xsl:choose>
        <xsl:when test ="$vRequestType='ASS'">
          <xsl:value-of select="data[@name='assignedquantity']"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="0"/>
        </xsl:otherwise>
      </xsl:choose>
    </data>

    <!--Market-->
    <xsl:call-template name="DataMKT">
      <xsl:with-param name ="pMarketID">
        <xsl:value-of select="data[@name='marketid']"/>
      </xsl:with-param>
    </xsl:call-template>

    <!-- ContractAsset -->
    <xsl:call-template name="DataCTR">
      <xsl:with-param name ="pIsOptionAction">
        <xsl:value-of select="'true'"/>
      </xsl:with-param>
    </xsl:call-template>

    <!-- Code à décommenter qd spheres® sera en mesure de déterminer un acteur à partir de L'ABICODE -->
    <!-- ABI code doit être renseigné dans la table CSMID avec le code ITIT (Italian Domestic Identification Code)  -->
    <!-- DealerEntity -->
    <!-- FI 20190904 [24882] Mise en place de BSE et BSEI -->
    <xsl:if test="$vRequestType = 'ASS'">
      <data>
        <xsl:attribute name="name">BSE</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="../@AbiCode"/>
      </data>
      <data>
        <xsl:attribute name="name">BSEI</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="'MemberId-ITIT'"/>
      </data>
    </xsl:if>


    <!-- ClearingOrganisation -->
    <xsl:call-template name="DataBSCO"/>

    <!-- ClearingOrganisationAccount-->
    <xsl:call-template name="DataBSCOA"/>

  </xsl:template>


  <!-- ================================================== -->
  <!--        GetTradeID                                  -->
  <!-- ================================================== -->
  <xsl:template name="GetTradeID">
    <xsl:param name ="pRequestType"/>
    <xsl:if test ="string-length(data[@name='isincode'])>0"  >
      <xsl:value-of select ="concat($pRequestType,'-',data[@name='abicode'],'-',data[@name='isincode'])"/>
      <xsl:choose>
        <xsl:when test="string-length(data[@name='subaccount'])>0 and data[@name='subaccount'] != '*OMN'">
          <xsl:value-of select="concat('-',data[@name='subaccount'])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="concat('-',data[@name='accounttype'])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>