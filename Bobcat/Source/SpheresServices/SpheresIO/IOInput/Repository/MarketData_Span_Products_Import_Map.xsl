<!-- 
==================================================================================================================
 Summary  : XSL propre à l'importation du fichier "ProductSlate.xls" fournit la chambre CME permettant 
            D’enrichir la table SPANPRODUCT.   
            
 File     : MarketData_Span_Products_Import_Map.xsl
==================================================================================================================
  Version  : v4.1.0.0                                              
  Date     : 20140516 [19933]
  File     : FL                                                 
  Comment  : Création

  Important!! : Ce XSL, n’est pour l’instant pas utilisé dans la version 4.1 de Spheres®, il sera 
                 utilisé dans une version ultérieure de spheres® lors de la gestion des données 
                 UNDERLYINGGROUP et UNDERLYINGASSET sur les DC de la chambre de compensation CME CLEARING HOUSE.
      
==================================================================================================================
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

  <!-- ================================================== -->
  <!--        import(s)                                   -->
  <!-- ================================================== -->
  <xsl:import href="MarketData_Common_SQL.xsl"/>

  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- ================================================== -->
  <!--        include(s)                                  -->
  <!-- ================================================== -->
  <xsl:include href="MarketData_Common.xsl"/>

  <!-- ================================================== -->
  <!--        <iotask>                                    -->
  <!-- ================================================== -->
  <xsl:template match="iotask">
    <iotask>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="name">
        <xsl:value-of select="@name"/>
      </xsl:attribute>
      <xsl:attribute name="displayname">
        <xsl:value-of select="@displayname"/>
      </xsl:attribute>
      <xsl:attribute name="loglevel">
        <xsl:value-of select="@loglevel"/>
      </xsl:attribute>
      <xsl:attribute name="commitmode">
        <xsl:value-of select="@commitmode"/>
      </xsl:attribute>
      <xsl:apply-templates select="parameters"/>
      <xsl:apply-templates select="iotaskdet"/>
    </iotask>
  </xsl:template>

  <!-- ================================================== -->
  <!--        <parameters>                                -->
  <!-- ================================================== -->
  <xsl:template match="parameters">
    <parameters>
      <xsl:for-each select="parameter">
        <parameter>
          <xsl:attribute name="id">
            <xsl:value-of select="@id"/>
          </xsl:attribute>
          <xsl:attribute name="name">
            <xsl:value-of select="@name"/>
          </xsl:attribute>
          <xsl:attribute name="displayname">
            <xsl:value-of select="@displayname"/>
          </xsl:attribute>
          <xsl:attribute name="direction">
            <xsl:value-of select="@direction"/>
          </xsl:attribute>
          <xsl:attribute name="datatype">
            <xsl:value-of select="@datatype"/>
          </xsl:attribute>
          <xsl:value-of select="."/>
        </parameter>
      </xsl:for-each>
    </parameters>
  </xsl:template>

  <!-- ================================================== -->
  <!--        <iotaskdet>                                 -->
  <!-- ================================================== -->
  <xsl:template match="iotaskdet">
    <iotaskdet>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="loglevel">
        <xsl:value-of select="@loglevel"/>
      </xsl:attribute>
      <xsl:attribute name="commitmode">
        <xsl:value-of select="@commitmode"/>
      </xsl:attribute>
      <xsl:apply-templates select="ioinput"/>
    </iotaskdet>
  </xsl:template>

  <!-- ================================================== -->
  <!--        <ioinput>                                   -->
  <!-- ================================================== -->
  <xsl:template match="ioinput">
    <ioinput>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="name">
        <xsl:value-of select="@name"/>
      </xsl:attribute>
      <xsl:attribute name="displayname">
        <xsl:value-of select="@displayname"/>
      </xsl:attribute>
      <xsl:attribute name="loglevel">
        <xsl:value-of select="@loglevel"/>
      </xsl:attribute>
      <xsl:attribute name="commitmode">
        <xsl:value-of select="@commitmode"/>
      </xsl:attribute>
      <xsl:apply-templates select="file"/>
    </ioinput>
  </xsl:template>

  <!-- ================================================== -->
  <!--        <file>                                      -->
  <!-- ================================================== -->
  <xsl:template match="file">
    <file>
      <xsl:attribute name="name">
        <xsl:value-of select="@name"/>
      </xsl:attribute>
      <xsl:attribute name="folder">
        <xsl:value-of select="@folder"/>
      </xsl:attribute>
      <xsl:attribute name="date">
        <xsl:value-of select="@date"/>
      </xsl:attribute>
      <xsl:attribute name="size">
        <xsl:value-of select="@size"/>
      </xsl:attribute>
      <xsl:apply-templates select="row"/>
    </file>
  </xsl:template>

  <!-- ================================================== -->
  <!--        <row>                                       -->
  <!-- ================================================== -->
  <xsl:template match="row">

    <row>
      <xsl:call-template name="rowStream"/>
    </row>
  </xsl:template>

  <!-- ============================================== -->
  <!--                 rowStream                      -->
  <!-- ============================================== -->
  <xsl:template name="rowStream">

    <!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~ -->
    <!-- RECUPERATION DES VARIABLES -->
    <!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~ -->
    <xsl:variable name="gNull">null</xsl:variable>

    <xsl:variable name="vCLEARINGORGACRONYM" select="'CME'"/>

    <xsl:variable name="vEXCHANGEACRONYM">
      <xsl:choose>
        <xsl:when test ="data[@name='Exchange']='CBOT'">CBT</xsl:when>
        <xsl:when test ="data[@name='Exchange']='CME'">CME</xsl:when>
        <xsl:when test ="data[@name='Exchange']='COMEX'">CMX</xsl:when>
        <xsl:when test ="data[@name='Exchange']='NYMEX'">NYM</xsl:when>
        <xsl:when test ="data[@name='Exchange']='CMED'">null</xsl:when>
        <xsl:when test ="data[@name='Exchange']='DUMX'">null</xsl:when>
        <xsl:otherwise>null</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vCONTRACTSYMBOL" select="data[@name='Clearing']"/>

    <xsl:variable name="vCATEGORY">
      <xsl:choose>
        <xsl:when test ="data[@name='Cleared As']='Options'">O</xsl:when>
        <xsl:otherwise>F</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!--FL 20140514 [19933]  -->
    <!-- UnderlyingAsset     -->
    <xsl:variable name="vUNDERLYINGASSET">
      <!-- Determiné en fonction du  Product Group & Sub Group -->

      <xsl:choose>

        <!-- Product Group = 'Agriculture' -->
        <xsl:when test ="data[@name='Product Group']='Agriculture'">
          <!--Valeur Possible pour : Sub Group
              Commodity Index, Dairy, Dairy Spot Markets, Fertilizer,
              Grain And Oilseed, Livestock, Lumber And Pulp, Softs -->
          <xsl:choose>
            <xsl:when test ="data[@name='Sub Group']='Commodity Index'">CA</xsl:when>
            <xsl:otherwise>CA</xsl:otherwise>
          </xsl:choose>
        </xsl:when>

        <!-- Product Group = 'Energy' -->
        <xsl:when test ="data[@name='Product Group']='Energy'">
          <!-- Valeur Possible pour : Sub Group
              Biofuels, Coal, Crude Oil, Electricity, Emissions, Freight
              Natural Gas, Petrochemicals, Refined Products -->
          <xsl:choose>
            <xsl:when test ="data[@name='Sub Group']='Commodity Index'">CE</xsl:when>
            <xsl:otherwise>CE</xsl:otherwise>
          </xsl:choose>
        </xsl:when>

        <!-- Product Group = 'Equities' -->
        <xsl:when test ="data[@name='Product Group']='Equities'">
          <!-- Valeur Possible pour : Sub Group
               International Index, Select Sector Index, US Index -->
          <xsl:choose>
            <xsl:when test ="data[@name='Sub Group']='International Index'">FI</xsl:when>
            <xsl:otherwise>FI</xsl:otherwise>
          </xsl:choose>
        </xsl:when>

        <!-- Product Group = 'FX' -->
        <xsl:when test ="data[@name='Product Group']='FX'">
          <!-- Valeur Possible pour : Sub Group
               Cross Rates, E Micros, Emerging Market, Majors, Other
               Realized Fx Volatilities -->
          <xsl:choose>
            <xsl:when test ="data[@name='Sub Group']='International Index'">FC</xsl:when>
            <xsl:otherwise>FC</xsl:otherwise>
          </xsl:choose>
        </xsl:when>

        <!-- Product Group = 'Interest Rate' -->
        <xsl:when test ="data[@name='Product Group']='Interest Rate'">
          <!-- Valeur Possible pour : Sub Group
               Deliverable Swaps, Interest Rate Index, Sovereign Yield Spreads
               Stirs, US Treasury -->
          <xsl:choose>
            <xsl:when test ="data[@name='Sub Group']='International Index'">FD</xsl:when>
            <xsl:otherwise>FD</xsl:otherwise>
          </xsl:choose>
        </xsl:when>

        <!-- Product Group = 'Metals' -->
        <xsl:when test ="data[@name='Product Group']='Metals'">
          <!-- Valeur Possible pour : Sub Group
                Base, Coking Coal, Ferrous, Other, Precious -->
          <xsl:choose>
            <xsl:when test ="data[@name='Sub Group']='International Index'">CE</xsl:when>
            <xsl:otherwise>CE</xsl:otherwise>
          </xsl:choose>
        </xsl:when>

        <xsl:otherwise>
        </xsl:otherwise>

      </xsl:choose>

    </xsl:variable>

    <!--FL 20140514 [19933]  -->
    <!-- UnderlyingGroup     -->
    <xsl:variable name="vUNDERLYINGGROUP">
      <!-- Determiné en fonction du  Product Group & Sub Group -->

      <xsl:choose>

        <!-- Product Group = 'Agriculture' -->
        <xsl:when test ="data[@name='Product Group']='Agriculture'">C</xsl:when>

        <!-- Product Group = 'Energy' -->
        <xsl:when test ="data[@name='Product Group']='Energy'">C</xsl:when>

        <!-- Product Group = 'Equities' -->
        <xsl:when test ="data[@name='Product Group']='Equities'">F</xsl:when>

        <!-- Product Group = 'FX' -->
        <xsl:when test ="data[@name='Product Group']='FX'">F</xsl:when>

        <!-- Product Group = 'Interest Rate' -->
        <xsl:when test ="data[@name='Product Group']='Interest Rate'">F</xsl:when>

        <!-- Product Group = 'Metals' -->
        <xsl:when test ="data[@name='Product Group']='Metals'">C</xsl:when>

        <xsl:otherwise>
        </xsl:otherwise>

      </xsl:choose>

    </xsl:variable>

    <!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ -->
    <!-- INSERT/UPDATE TABLES SPANPRODUCT -->
    <!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ -->
    <table name="SPANPRODUCT" action="U" sequenceno="1">

      <column name="CLEARINGORGACRONYM" datakey="true" datakeyupd="false" datatype="string">
        <xsl:value-of select= "$vCLEARINGORGACRONYM"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="IsNull()" />
            <logInfo status="REJECTED" isexception ="true">
              <message>
                Clearing Organisation is a mandatory data and it cannot be empty.
              </message>
            </logInfo>
          </control>
        </controls>
      </column>

      <column name="EXCHANGEACRONYM" datakey="true" datakeyupd="false" datatype="string">
        <xsl:value-of select= "$vEXCHANGEACRONYM"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="IsNull()" />
          </control>
        </controls>
      </column>

      <column name="CATEGORY" datakey="true" datakeyupd="false" datatype="string">
        <xsl:value-of select= "$vCATEGORY"/>
      </column>

      <column name="CONTRACTSYMBOL" datakey="true" datakeyupd="false" datatype="string">
        <xsl:value-of select= "$vCONTRACTSYMBOL"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="IsNull()" />
            <logInfo status="REJECTED" isexception ="true">
              <message>
                Contract Symbol is a mandatory data and it cannot be empty.
              </message>
            </logInfo>
          </control>
        </controls>
      </column>

      <column name="UNDERLYINGASSET" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select= "$vUNDERLYINGASSET"/>
      </column>

      <column name="UNDERLYINGGROUP" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select= "$vUNDERLYINGGROUP"/>
      </column>

    </table>

  </xsl:template>
</xsl:stylesheet>