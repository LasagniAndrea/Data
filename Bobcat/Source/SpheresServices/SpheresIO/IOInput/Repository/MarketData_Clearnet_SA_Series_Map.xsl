<!--
=========================================================================================================
Summary : LCH CLEARNET SA - REPOSITORY
File    : MarketData_Clearnet_SA_Series_Map.xsl
=========================================================================================================
=========================================================================================================
Version : v1.0.0.0                                           
Date    : 20171006                                           
Author  : PLA
=========================================================================================================
Version: v6.0.6507   Date: 20171025   Author: PLA
Comment: [23491] - Import: MiFID II: ISIN codes for series [LCH.CLEARNET-SA] 
         (Add category contract on clause where for the table ASSET_ETD)
Version: v6.0.6488   Date: 20171006   Author: PLA
Comment: [23491] - Import: MiFID II: ISIN codes for series [LCH.CLEARNET-SA] (New feature)
=========================================================================================================
-->
<!-- FI 20200901 [25468] use of GetUTCDateTimeSys -->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- Includes-->
  <xsl:include href="MarketData_Common.xsl"/>
  <xsl:include href="MarketData_Common_SQL.xsl"/>

  <!--Main template  -->
  <xsl:template match="/iotask">
    <iotask>
      <xsl:call-template name="IOTaskAtt"/>
      <xsl:apply-templates select="parameters"/>
      <xsl:apply-templates select="iotaskdet"/>
    </iotask>
  </xsl:template>

  <!-- Specific template-->
  <xsl:template match="file">
    <file>
      <xsl:call-template name="IOFileAtt"/>
      <xsl:apply-templates select="row"/>
    </file>
  </xsl:template>

  <xsl:template match="row">
    <row useCache="false">
      <xsl:call-template name="IORowAtt"/>
      <xsl:call-template name="rowStream"/>
    </row>
  </xsl:template>

  <!-- Spécifique à chaque Import -->
  <xsl:template name="rowStream">
    <!-- ================================================================================ -->
    <!--                              Shared variables                                    -->
    <!-- These variables are used both by Derivative Contract section and Series section  -->
    <!-- ================================================================================ -->
    <xsl:variable name="vExchangeSymbol">
      <xsl:value-of select="normalize-space(data[@name='MC'])"/>
    </xsl:variable>

    <xsl:variable name="vContractSymbol">
      <xsl:value-of select="normalize-space(data[@name='FMC'])"/>
    </xsl:variable>

    <!-- F=Future;O=Option -->
    <xsl:variable name="vCategory">
      <xsl:value-of select="normalize-space(data[@name='FT'])"/>
    </xsl:variable>

    <xsl:variable name="vMaturityMonthYear">
      <xsl:value-of select="normalize-space(data[@name='EC'])"/>
    </xsl:variable>

    <!-- ================================================================================ -->
    <!--                              Series variables II                                 -->
    <!-- For passing data to template SQLTableASSET_ETD                                   -->
    <!-- ================================================================================ -->
    <xsl:variable name="vAssetISINCode">
      <xsl:value-of select="normalize-space(data[@name='IC'])"/>
    </xsl:variable>

    <xsl:variable name="vPutCall">
      <xsl:call-template name="CallPut">
        <xsl:with-param name="pCallPut">
          <xsl:value-of select="normalize-space(data[@name='OS'])"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vStrike">
      <xsl:value-of select="normalize-space(data[@name='SP'])"/>
    </xsl:variable>

    <!-- PLA 20171006 [23491] MiFID II: ISIN codes for series [LCH.CLEARNET-SA]-->
    <!-- PLA 20171025 [23491] MiFID II: Add category contract on clause where for the table ASSET_ETD-->
    <xsl:if test="string-length($vAssetISINCode) > 0">

      <table name="ASSET_ETD" action="U">
        <column name="IDASSET" datakey="true" datatype="integer">
          <SQL command="select" cache="true" result="IDASSET">
            <![CDATA[select asset.IDASSET
            from dbo.ASSET_ETD asset
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB=asset.IDDERIVATIVEATTRIB)
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC=da.IDDC) and (dc.CONTRACTSYMBOL=@CONTRACTSYMBOL)
            inner join dbo.MARKET m on m.IDM=dc.IDM and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL)
            inner join dbo.MATURITY ma on ((ma.IDMATURITY=da.IDMATURITY) and (ma.IDMATURITYRULE=dc.IDMATURITYRULE)) and (ma.MATURITYMONTHYEAR=@MATURITYMONTHYEAR)
            inner join dbo.MATURITYRULE mr on ((mr.IDMATURITYRULE=dc.IDMATURITYRULE) and (mr.IDMATURITYRULE=ma.IDMATURITYRULE))
            where ((dc.CATEGORY=@CATEGORYFUTURE and @CATEGORY='F') or (dc.CATEGORY=@CATEGORYOPTION and @CATEGORY='O' and asset.STRIKEPRICE=@STRIKEPRICE and asset.PUTCALL=@PUTCALL))
            and (dc.DTENABLED<=@DT and (dc.DTDISABLED is null or dc.DTDISABLED>@DT))]]>
            <Param name="CONTRACTSYMBOL" datatype="string">
              <xsl:value-of select="$vContractSymbol"/>
            </Param>
            <Param name="EXCHANGESYMBOL" datatype="string">
              <xsl:value-of select="$vExchangeSymbol"/>
            </Param>
            <Param name="MATURITYMONTHYEAR" datatype="string">
              <xsl:value-of select="$vMaturityMonthYear"/>
            </Param>
            <Param name="CATEGORY" datatype="string">
              <xsl:value-of select="$vCategory"/>
            </Param>
            <Param name="CATEGORYFUTURE" datatype="string">F</Param>
            <Param name="CATEGORYOPTION" datatype="string">O</Param>
            <Param name="STRIKEPRICE" datatype="decimal">
              <xsl:value-of select="$vStrike"/>
            </Param>
            <Param name="PUTCALL" datatype="string">
              <xsl:value-of select="$vPutCall"/>
            </Param>
            <Param name="DT" datatype="date">
              <xsl:value-of select="/iotask/parameters/parameter[@id='DTBUSINESS']"/>
            </Param>
          </SQL>
        </column>
        <column name="ISINCODE" datakeyupd="true" >
          <xsl:value-of select="$vAssetISINCode"/>
        </column>
        <column name="DTUPD" datakey="false" datakeyupd="false" datatype="datetime">
          <SpheresLib function="GetUTCDateTimeSys()" />
        </column>
        <column name="IDAUPD" datakey="false" datakeyupd="false" datatype="int">
          <SpheresLib function="GetUserID()" />
        </column>
      </table>
    </xsl:if>

  </xsl:template>

</xsl:stylesheet>
