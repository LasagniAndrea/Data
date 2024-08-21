<?xml version="1.0" encoding="utf-8"?>
<!--
=================================================================================================
Summary : CCeG - RISKDATA
File    : RiskData_CCeG_FuturesClosingPrices_Map.xsl
=================================================================================================
Version : v3.2.0.0                                           
Date    : 20130430
Author  : CC
Comment : File FuturesPricesImport-IDEM.xsl renamed to RiskData_CCeG_FuturesClosingPrices_Map.xsl
=================================================================================================
Version : v3.2.0.0                                           
Date    : 20130215
Author  : PL
Comment : Management of Market-Id='5' and Market-Id='8'
=================================================================================================
Date    : 20121029
Author  : BD                                           
Description: 
=================================================================================================
-->
<!-- FI 20200901 [25468] use of GetUTCDateTimeSys -->

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1" />

  <xsl:include href="Quote_Common_SQL.xsl"/>

  <!-- iotask -->
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

  <!-- parameters -->
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

  <!-- iotaskdet -->
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

  <!-- ioinput -->
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

  <!-- file -->
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

  <!-- row -->
  <xsl:template match="row">
    <row>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>

      <xsl:attribute name="src">
        <xsl:value-of select="@src"/>
      </xsl:attribute>

      <table name="QUOTE_ETD_H" action="IU" sequenceno="1">

        <!-- IDMARKETENV -->
        <column name="IDMARKETENV" datakey="true">
          <SQL command="select" result="IDMARKETENV" cache="true">
            select IDMARKETENV
            from dbo.MARKETENV
            where ISDEFAULT = 1
          </SQL>
          <controls>
            <control action="RejectRow" return="true">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
        </column>

        <!-- IDVALSCENARIO -->
        <column name="IDVALSCENARIO" datakey="true">
          <SQL command="select" result="IDVALSCENARIO" cache="true">
            select v.IDVALSCENARIO, 1 as colorder
            from dbo.VALSCENARIO v
            inner join dbo.MARKETENV m on (m.IDMARKETENV = v.IDMARKETENV and m.ISDEFAULT = 1)
            where v.ISDEFAULT = 1
            union all
            select v.IDVALSCENARIO, 2 as colorder
            from dbo.VALSCENARIO v
            where v.ISDEFAULT = 1 and v.IDMARKETENV is null
            order by colorder asc
          </SQL>
          <controls>
            <control action="RejectRow" return="true">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
        </column>

        <!-- IDASSET -->
        <column name="IDASSET" datakey="true" datatype="integer">
          <!-- BD 20130520 : Appel du template SQLDTENABLEDDTDISABLED pour vérifier la validité du DC sélectionné -->
          <SQL command="select" result="IDASSET" cache="true">
            select distinct tblmain.IDASSET
            from (
            select distinct a.IDASSET
            from dbo.ASSET_ETD a
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB=a.IDDERIVATIVEATTRIB)
            and ( (da.DTDISABLED is null) or (da.DTDISABLED &gt;= @DT) )
            inner join dbo.MATURITY ma on (ma.IDMATURITY=da.IDMATURITY)
            and ( (ma.DTDISABLED is null) or (ma.DTDISABLED &gt;= @DT) )
            inner join dbo.MATURITYRULE mr on ( mr.IDMATURITYRULE = ma.IDMATURITYRULE )
            and ( (mr.DTDISABLED is null) or (mr.DTDISABLED &gt;= @DT) )
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC=da.IDDC) and (dc.CONTRACTSYMBOL=@CONTRACTSYMBOL) and (dc.CATEGORY=@CATEGORY)
            <xsl:call-template name="SQLDTENABLEDDTDISABLED">
              <xsl:with-param name="pTable" select="'dc'"/>
            </xsl:call-template>
            inner join dbo.MARKET m on (m.IDM=dc.IDM) and (m.ISO10383_ALPHA4='XDMI') and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL)
            and ( (m.DTDISABLED is null) or (m.DTDISABLED &gt;= @DT) )
            inner join dbo.TRADE tr on (tr.IDASSET=a.IDASSET)
            where (
            ( (mr.MMYFMT=0) and (ma.MATURITYMONTHYEAR=substring(@MATURITYMONTHYEAR,1,6)) )
            or
            (ma.MATURITYMONTHYEAR=@MATURITYMONTHYEAR)
            )
            and ( (a.DTDISABLED is null) or (a.DTDISABLED &gt;= @DT) )
            union all
            select distinct a.IDASSET
            from dbo.ASSET_ETD a
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB=a.IDDERIVATIVEATTRIB)
            and ( (da.DTDISABLED is null) or (da.DTDISABLED &gt;= @DT) )
            inner join dbo.MATURITY ma on (ma.IDMATURITY=da.IDMATURITY) and (ma.MATURITYMONTHYEAR=@MATURITYMONTHYEAR)
            and ( (ma.DTDISABLED is null) or (ma.DTDISABLED &gt;= @DT) )
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC=da.IDDC) and (dc.CONTRACTSYMBOL=@CONTRACTSYMBOL) and (dc.CATEGORY=@CATEGORY)
            and ( (dc.DTDISABLED is null) or (dc.DTDISABLED &gt;= @DT) )
            inner join dbo.MARKET m on (m.IDM=dc.IDM) and (m.ISO10383_ALPHA4='XDMI') and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL)
            and ( (m.DTDISABLED is null) or (m.DTDISABLED &gt;= @DT) )
            inner join dbo.DERIVATIVEATTRIB da_opt on ( da_opt.IDASSET = a.IDASSET )
            and ( (da_opt.DTDISABLED is null) or (da_opt.DTDISABLED &gt;= @DT) )
            inner join dbo.DERIVATIVECONTRACT dc_opt on (dc_opt.IDDC=da_opt.IDDC) and (dc_opt.CATEGORY='O') and (dc_opt.ASSETCATEGORY='Future')
            and ( (dc_opt.DTDISABLED is null) or (dc_opt.DTDISABLED &gt;= @DT) )
            inner join dbo.ASSET_ETD a_opt on (a_opt.IDDERIVATIVEATTRIB=da_opt.IDDERIVATIVEATTRIB)
            and ( (a_opt.DTDISABLED is null) or (a_opt.DTDISABLED &gt;= @DT) )
            inner join dbo.TRADE tr on (tr.IDASSET=a_opt.IDASSET)
            where ( (a.DTDISABLED is null) or (a.DTDISABLED &gt;= @DT) )
            ) tblmain
            <Param name="DT" datatype="date">
              <xsl:value-of select="$gParamDtBusiness"/>
            </Param>
            <Param name="EXCHANGESYMBOL">
              <xsl:value-of select="data[@name='Market']"/>
            </Param>
            <Param name="CONTRACTSYMBOL">
              <xsl:value-of select="data[@name='Symbol']"/>
            </Param>
            <Param name="CATEGORY">F</Param>
            <Param name="MATURITYMONTHYEAR">
              <xsl:value-of select="data[@name='Expiry']"/>
            </Param>
          </SQL>
          <controls>
            <control action="RejectRow" return="true">
              <SpheresLib function="ISNULL()"/>
            </control>
          </controls>
        </column>

        <!-- IDC -->
        <column name="IDC" datakeyupd="true">
          <!-- BD 20130520 : Appel du template SQLDTENABLEDDTDISABLED pour vérifier la validité du DC sélectionné -->
          <SQL command="select" result="IDC" cache="true">
            select dc.IDC_PRICE
            from dbo.DERIVATIVECONTRACT dc
            inner join dbo.MARKET m on (m.IDM=dc.IDM) and (m.ISO10383_ALPHA4='XDMI') and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL)
            where (dc.CONTRACTSYMBOL=@CONTRACTSYMBOL) and (dc.CATEGORY='O')
            <xsl:call-template name="SQLDTENABLEDDTDISABLED">
              <xsl:with-param name="pTable" select="'dc'"/>
            </xsl:call-template>
            <Param name="DT" datatype="date">
              <xsl:value-of select="$gParamDtBusiness"/>
            </Param>
            <Param name="EXCHANGESYMBOL">
              <xsl:value-of select="data[@name='Market']"/>
            </Param>
            <Param name="CONTRACTSYMBOL">
              <xsl:value-of select="data[@name='Symbol']"/>
            </Param>
          </SQL>
        </column>

        <!-- IDM -->
        <column name="IDM" datakeyupd="true" datatype="integer">
          <SQL command="select" result="IDM" cache="true">
            select m.IDM
            from dbo.MARKET m
            where (m.ISO10383_ALPHA4='XDMI') and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL)
            <Param name="EXCHANGESYMBOL">
              <xsl:value-of select="data[@name='Market']"/>
            </Param>
          </SQL>
        </column>

        <!-- TIME -->
        <column name="TIME" datakey="true" datatype="datetime" dataformat="yyyy-MM-dd">
          <xsl:value-of select="data[@name='Date']"/>
          <controls>
            <control action="RejectRow" return="false">
              <SpheresLib function="IsDate()" format="YYYY-MM-DD" />
              <logInfo status="err" isException="true">
                <message>
                  Date: Invalid Date (<xsl:value-of select="data[@name='Date']"/>)
                </message>
              </logInfo>
            </control>
          </controls>
        </column>

        <column name="VALUE" datakeyupd="true" datatype="decimal">
          <xsl:value-of select="data[@name='MarkPrice']"/>
        </column>
        <column name="SPREADVALUE" datakey="false" datakeyupd="true" datatype="decimal">0</column>
        <column name="QUOTEUNIT" datakey="false" datakeyupd="true" datatype="string">Price</column>
        <column name="QUOTESIDE" datakey="true" datakeyupd="true" datatype="string">OfficialClose</column>
        <column name="QUOTETIMING" datakey="false" datakeyupd="true" datatype="string">Close</column>
        <column name="ASSETMEASURE" datakey="false" datakeyupd="false" datatype="string">MarketQuote</column>
        <column name="ISENABLED" datakey="false" datakeyupd="true" datatype="bool">1</column>
        <column name="SOURCE" datakey="false" datakeyupd="false" datatype="string">ClearingOrganization</column>

        <!-- Infos Importation -->
        <column name="DTUPD" datakey="false" datakeyupd="false" datatype="datetime">
          <SpheresLib function="GetUTCDateTimeSys()" />
          <controls>
            <control action="RejectColumn" return="true" >
              <SpheresLib function="IsInsert()" />
            </control>
          </controls>
        </column>
        <column name="IDAUPD" datakey="false" datakeyupd="false" datatype="integer">
          <SpheresLib function="GetUserId()" />
          <controls>
            <control action="RejectColumn" return="true" >
              <SpheresLib function="IsInsert()" />
            </control>
          </controls>
        </column>
        <column name="DTINS" datakey="false" datakeyupd="false" datatype="datetime">
          <SpheresLib function="GetUTCDateTimeSys()" />
          <controls>
            <control action="RejectColumn" return="true" >
              <SpheresLib function="IsUpdate()" />
            </control>
          </controls>
        </column>
        <column name="IDAINS" datakey="false" datakeyupd="false" datatype="integer">
          <SpheresLib function="GetUserId()" />
          <controls>
            <control action="RejectColumn" return="true" >
              <SpheresLib function="IsUpdate()" />
            </control>
          </controls>
        </column>

        <!-- VOLATILITY -->
        <column name="VOLATILITY" datakeyupd="true" datatype="decimal">
          <xsl:value-of select="data[@name='Volatility']"/>
        </column>

      </table>
    </row>
  </xsl:template>

</xsl:stylesheet>