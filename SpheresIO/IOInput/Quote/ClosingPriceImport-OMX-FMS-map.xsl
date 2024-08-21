<?xml version="1.0" encoding="utf-8"?>

<!-- 
======================================================================================================================================== 
Sommaire     : Importation des prix de clôtures des Futures et options OMX                        
======================================================================================================================================== 
FI 20130516 [18382] Margin Settlement Price /100 
    La documentation OMX  =>  marg_price_i : datatype INT32_T, defines the margin settlement price (rien de plus) 
    En absence de plus de commentaire division par 100 pour coller aux valeurs du marché

BD 20130416 [18382] New XSL
======================================================================================================================================== 
-->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- ==================================================================== -->
  <!--            include(s)                                                -->
  <!-- ==================================================================== -->
  <xsl:include href="Quote_Common_SQL.xsl"/>

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
      <xsl:attribute name="displayname">
        <xsl:value-of select="@displayname"/>
      </xsl:attribute>
      <xsl:attribute name="name">
        <xsl:value-of select="@name"/>
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

      <!-- ================= -->
      <!--    PARAMETERS     -->
      <!-- ================= -->      
      <parameters>
        <!-- IDM -->
        <parameter name="IdMarket">
          <!-- BD 20130520 : Appel du template SQLDTENABLEDDTDISABLED pour vérifier la validité du DC sélectionné -->
          <SQL command="select" result="IDM" cache="true">
            select m.IDM
            from dbo.MARKET m
            inner join dbo.ASSET_ETD a on ( a.IDASSET = @IDASSET )
            inner join dbo.DERIVATIVEATTRIB da on ( da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB )
            inner join dbo.DERIVATIVECONTRACT dc on ( dc.IDDC = da.IDDC )
            where ( m.IDM = dc.IDM )
            <xsl:call-template name="SQLDTENABLEDDTDISABLED">
              <xsl:with-param name="pTable" select="'dc'"/>
            </xsl:call-template>
            <Param name="DT" datatype="date">
              <xsl:value-of select="$vDtBusinnes"/>
            </Param>
            <Param name="IDASSET" datatype="integer">
              <xsl:value-of select="$vIdAsset" />
            </Param>
          </SQL>
        </parameter>
        <!-- IDC -->
        <parameter name="IdCurrency">
          <!-- BD 20130520 : Appel du template SQLDTENABLEDDTDISABLED pour vérifier la validité du DC sélectionné -->
          <SQL command="select" result="IDC" cache="true">
            select c.IDC
            from dbo.CURRENCY c
            inner join dbo.ASSET_ETD a on ( a.IDASSET = @IDASSET )
            inner join dbo.DERIVATIVEATTRIB da on ( da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB )
            inner join dbo.DERIVATIVECONTRACT dc on ( dc.IDDC = da.IDDC )
            where ( c.IDC = dc.IDC_PRICE )
            <xsl:call-template name="SQLDTENABLEDDTDISABLED">
              <xsl:with-param name="pTable" select="'dc'"/>
            </xsl:call-template>
            <Param name="DT" datatype="date">
              <xsl:value-of select="$vDtBusinnes"/>
            </Param>
            <Param name="IDASSET" datatype="integer">
              <xsl:value-of select="$vIdAsset" />
            </Param>
          </SQL>
        </parameter>
        <!-- Value -->
        <!-- BD FI 20130520 Utilisation du paramètre DERIVATIVECONTRAT.PRICEDECLOCATOR -->
        <parameter name="Value">
          <!-- BD 20130520 : Appel du template SQLDTENABLEDDTDISABLED pour vérifier la validité du DC sélectionné -->
          <!-- SI 20130822 : Ref ticket 18894 modification de la query pour la conversion du cours  -->
          <SQL command="select" result="VALUE" cache="true">
            select case when dc.PRICEDECLOCATOR is null then @PRICEVALUE else cast(cast(@PRICEVALUE as decimal(15,9)) / power( 10, dc.PRICEDECLOCATOR ) as decimal(15,9)) end VALUE
            from dbo.DERIVATIVECONTRACT dc
            inner join dbo.DERIVATIVEATTRIB da on ( da.IDDC = dc.IDDC )
            inner join dbo.ASSET_ETD a on ( a.IDASSET = @IDASSET ) and ( da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB )
            <xsl:call-template name="SQLDTENABLEDDTDISABLED">
              <xsl:with-param name="pTable" select="'dc'"/>
            </xsl:call-template>
            <Param name="DT" datatype="date">
              <xsl:value-of select="$vDtBusinnes"/>
            </Param>
            <Param name="IDASSET" datatype="integer">
              <xsl:value-of select="$vIdAsset" />
            </Param>
            <Param name="PRICEVALUE" datatype="integer">
              <xsl:value-of select="data[@name='MargP']" />
            </Param>
          </SQL>
        </parameter>
      </parameters>

      <!-- ================= -->
      <!--    SQL            -->
      <!-- ================= -->
      <xsl:call-template name="QUOTE_H_IU">
        <xsl:with-param name="pTableName" select="'QUOTE_ETD_H'"/>
        <xsl:with-param name="pIDM" select="'parameters.IdMarket'"/>
        <xsl:with-param name="pIdAsset" select="$vIdAsset"/>
        <xsl:with-param name="pBusinessDate" select="$vDtBusinnes"/>
        <xsl:with-param name="pCurrency" select="'parameters.IdCurrency'"/>
        <xsl:with-param name="pValue" select="'parameters.Value'"/>
      </xsl:call-template>

    </row>
  </xsl:template>

</xsl:stylesheet>