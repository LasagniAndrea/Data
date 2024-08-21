<?xml version="1.0" encoding="utf-8"?>
<!-- 
======================================================================================================================================== 
 Sommaire     : Importation des données nécessaires au calcul de risque du marché suédois                                                
======================================================================================================================================== 
FI 20130522 [18382]  Ajout des colonnes DTINS,IDAINS, DTUPD,IDAUPD,etc..

FI 20130516 [18382] Modifications diverses 
  - ajout datatype lorsque non renseignés
  - la colonne POINT est datakey 
  - les colonnes COUNTRY,MARKET,INSTRUMENTGROUP, MODIFIER,COMMODITY  sont  datakeyupd="false" 

BD 20130416 [18382] New XSL
======================================================================================================================================== 
-->
<!-- FI 20200901 [25468] use of GetUTCDateTimeSys -->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>
  
  <!-- ==================================================================== -->
  <!--            <iotask>                                                  -->
  <!-- ==================================================================== -->
  <xsl:template match="/iotask">
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

  <!-- ==================================================================== -->
  <!--            <parameters>                                              -->
  <!-- ==================================================================== -->
  <xsl:template match="parameters">
    <parameters>
      <xsl:for-each select="parameter" >
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

  <!-- ==================================================================== -->
  <!--            <iotaskdet>                                               -->
  <!-- ==================================================================== -->
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

  <!-- ==================================================================== -->
  <!--            <ioinput>                                                 -->
  <!-- ==================================================================== -->
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

  <!-- ==================================================================== -->
  <!--            <file>                                                    -->
  <!-- ==================================================================== -->
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
      <xsl:apply-templates select="row" />
    </file>
  </xsl:template>

  <!-- ==================================================================== -->
  <!--            <row>                                                     -->
  <!-- ==================================================================== -->
  <xsl:template match="row">
    <row>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="src">
        <xsl:value-of select="@src"/>
      </xsl:attribute>

      <!-- ================= -->
      <!--    VARIABLES      -->
      <!-- ================= -->
      <!-- IDASSET -->
      <xsl:variable name="vIdAsset">
        <xsl:value-of select="data[@name='IDASSET']"/>
      </xsl:variable>
      <!-- DTBUSINESS -->
      <xsl:variable name="vDtBusinnes">
        <xsl:value-of select="/iotask/parameters/parameter[@id='DTBUSINESS']"/>
      </xsl:variable>
      <!-- Country -->
      <xsl:variable name="vCountry">
        <xsl:value-of select="data[@name='Country']"/>
      </xsl:variable>
      <!-- Market -->
      <xsl:variable name="vMarket">
        <xsl:value-of select="data[@name='Market']"/>
      </xsl:variable>
      <!-- InstrumentGroup -->
      <xsl:variable name="vInstrumentGroup">
        <xsl:value-of select="data[@name='Instrument']"/>
      </xsl:variable>
      <!-- Modifier -->
      <xsl:variable name="vModifier">
        <xsl:value-of select="data[@name='Modifier']"/>
      </xsl:variable>
      <!-- Commodity -->
      <xsl:variable name="vCommodity">
        <xsl:value-of select="data[@name='Commodity']"/>
      </xsl:variable>
      <!-- ExpirationDate -->
      <xsl:variable name="vExpirationDate">
        <xsl:value-of select="data[@name='Expiration']"/>
      </xsl:variable>
      <!-- StrikePrice -->
      <xsl:variable name="vStrikePrice">
        <xsl:value-of select="data[@name='Strike']"/>
      </xsl:variable>
      <!-- Point -->
      <xsl:variable name="vPoint">
        <xsl:value-of select="data[@name='Point']"/>
      </xsl:variable>
      <!-- Spot -->
      <xsl:variable name="vSpot">
        <xsl:value-of select="data[@name='Spot']"/>
      </xsl:variable>
      <!-- HeldLow -->
      <xsl:variable name="vHeldLow">
        <xsl:value-of select="data[@name='LowH']"/>
      </xsl:variable>
      <!-- WrittenLow -->
      <xsl:variable name="vWrittenLow">
        <xsl:value-of select="data[@name='LowW']"/>
      </xsl:variable>
      <!-- HeldMiddle -->
      <xsl:variable name="vHeldMiddle">
        <xsl:value-of select="data[@name='MidH']"/>
      </xsl:variable>
      <!-- WrittenMiddle -->
      <xsl:variable name="vWrittenMiddle">
        <xsl:value-of select="data[@name='MidW']"/>
      </xsl:variable>
      <!-- HeldHigh -->
      <xsl:variable name="vHeldHigh">
        <xsl:value-of select="data[@name='HigH']"/>
      </xsl:variable>
      <!-- WrittenHigh -->
      <xsl:variable name="vWrittenHigh">
        <xsl:value-of select="data[@name='HigW']"/>
      </xsl:variable>

      <!-- ================= -->
      <!--    SQL            -->
      <!-- ================= -->
      <table name="IMOMXVCTFILE_H" action="IU">
        <!-- IDASSET -->
        <column name="IDASSET" datakey="true" datatype="integer">
          <xsl:value-of select="$vIdAsset"/>
          <controls>
            <control action="RejectRow" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
        </column>
        <!-- DTBUSINESS -->
        <column name="DTBUSINESS" datakey="true"  datatype="datetime" dataformat="yyyy-MM-dd">
          <xsl:value-of select="$vDtBusinnes"/>
          <controls>
            <control action="RejectRow" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
        </column>
        <!-- COUNTRY -->
        <column name="COUNTRY" datakeyupd="false" datatype="string">
          <xsl:value-of select="$vCountry"/>
          <controls>
            <control action="RejectRow" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
        </column>
        <!-- MARKET -->
        <column name="MARKET" datakeyupd="false" datatype="string">
          <xsl:value-of select="$vMarket"/>
          <controls>
            <control action="RejectRow" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
        </column>
        <!-- INSTRUMENTGROUP -->
        <column name="INSTRUMENTGROUP" datakeyupd="false" datatype="string">
          <xsl:value-of select="$vInstrumentGroup"/>
          <controls>
            <control action="RejectRow" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
        </column>
        <!-- MODIFIER -->
        <column name="MODIFIER" datakeyupd="false" datatype="string">
          <xsl:value-of select="$vModifier"/>
          <controls>
            <control action="RejectRow" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
        </column>
        <!-- COMMODITY -->
        <column name="COMMODITY" datakeyupd="false" datatype="string">
          <xsl:value-of select="$vCommodity"/>
          <controls>
            <control action="RejectRow" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
        </column>
        <!-- EXPIRATIONDATE -->
        <column name="EXPIRATIONDATE" datakeyupd="false" datatype="string" >
          <xsl:value-of select="$vExpirationDate"/>
          <controls>
            <control action="RejectRow" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
        </column>
        <!-- STRIKEPRICE -->
        <column name="STRIKEPRICE" datakeyupd="false" datatype="string" >
          <xsl:value-of select="$vStrikePrice"/>
          <controls>
            <control action="RejectRow" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
        </column>
        <!-- POINT -->
        <column name="POINT" datakey="true" datatype="integer">
          <xsl:value-of select="$vPoint"/>
          <controls>
            <control action="RejectRow" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
        </column>
        <!-- SPOT -->
        <column name="SPOT" datakeyupd="true" datatype="integer">
          <xsl:value-of select="$vSpot"/>
          <controls>
            <control action="RejectRow" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
        </column>
        <!-- HELDLOW -->
        <column name="HELDLOW" datakeyupd="true" datatype="integer">
          <xsl:value-of select="$vHeldLow"/>
          <controls>
            <control action="RejectRow" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
        </column>
        <!-- WRITTENLOW -->
        <column name="WRITTENLOW" datakeyupd="true" datatype="integer">
          <xsl:value-of select="$vWrittenLow"/>
          <controls>
            <control action="RejectRow" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
        </column>
        <!-- HELDMIDDLE -->
        <column name="HELDMIDDLE" datakeyupd="true" datatype="integer">
          <xsl:value-of select="$vHeldMiddle"/>
          <controls>
            <control action="RejectRow" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
        </column>
        <!-- WRITTENMIDDLE -->
        <column name="WRITTENMIDDLE" datakeyupd="true" datatype="integer">
          <xsl:value-of select="$vWrittenMiddle"/>
          <controls>
            <control action="RejectRow" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
        </column>
        <!-- HELDHIGH -->
        <column name="HELDHIGH" datakeyupd="true" datatype="integer">
          <xsl:value-of select="$vHeldHigh"/>
          <controls>
            <control action="RejectRow" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
        </column>
        <!-- WRITTENHIGH -->
        <column name="WRITTENHIGH" datakeyupd="true" datatype="integer">
          <xsl:value-of select="$vWrittenHigh"/>
          <controls>
            <control action="RejectRow" return="true" logtype="None">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
        </column>

        <column name="DTINS" datakey="false" datakeyupd="false" datatype="datetime">
          <SpheresLib function="GetUTCDateTimeSys()" />
          <controls>
            <control action="RejectColumn" return="true" >
              <SpheresLib function="IsUpdate()" />
              <logInfo status="NONE"/>
            </control>
          </controls>
        </column>
        <column name="IDAINS" datakey="false" datakeyupd="false" datatype="integer">
          <SpheresLib function="GetUserId()" />
          <controls>
            <control action="RejectColumn" return="true" >
              <SpheresLib function="IsUpdate()" />
              <logInfo status="NONE"/>
            </control>
          </controls>
        </column>
        <column name="DTUPD" datakey="false" datakeyupd="false" datatype="datetime">
          <SpheresLib function="GetDateSys()" />
          <controls>
            <control action="RejectColumn" return="true" >
              <SpheresLib function="IsInsert()" />
              <logInfo status="NONE"/>
            </control>
          </controls>
        </column>
        <column name="IDAUPD" datakey="false" datakeyupd="false" datatype="integer">
          <SpheresLib function="GetUserId()" />
          <controls>
            <control action="RejectColumn" return="true" >
              <SpheresLib function="IsInsert()" />
              <logInfo status="NONE"/>
            </control>
          </controls>
        </column>
        <column name="EXTLLINK" datakey="false" datakeyupd="false" datatype="string">null</column>
        <column name="ROWATTRIBUT" datakey="false" datakeyupd="false" datatype="string">S</column>

      </table>

      


    </row>
  </xsl:template>

</xsl:stylesheet>