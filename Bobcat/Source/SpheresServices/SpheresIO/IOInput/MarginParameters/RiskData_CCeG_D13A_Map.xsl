<?xml version="1.0" encoding="ISO-8859-1"?>
<!--
=============================================================================================
Summary : CCeG - RISKDATA
File    : RiskData_CCeG_D13A_Map.xsl
=============================================================================================
Version : v3.2.0.0                                           
Date    : 20130430
Author  : CC
Comment : File CCeG-D13A-EquitiesForRiskPerformance.xsl renamed to RiskData_CCeG_D13A_Map.xsl
=============================================================================================
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <xsl:decimal-format name="decimalFormat" decimal-separator="." />

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
      <xsl:if test="//iotask/parameters/parameter[@id='PRICEONLY']='false'">
        <xsl:apply-templates select="row"/>
      </xsl:if>
    </file>
  </xsl:template>

  <xsl:template match="row">
    <row>
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:attribute name="src">
        <xsl:value-of select="@src"/>
      </xsl:attribute>
      <table name="CCG_D13A" action="IU" sequenceno="1">
        <column name="MEMBERCLEARINGCODE" datakey="true" datakeyupd="false" datatype="integer">
          <xsl:variable name="MembersClearingCode">
            <xsl:text>Member's clearing code</xsl:text>
          </xsl:variable>
          <xsl:value-of select="data[@name=$MembersClearingCode]"/>
        </column>
        <column name="DATAFILECODE" datakey="true" datatype="string">
          <xsl:value-of select="data[@name='Data File code']"/>
        </column>
        <column name="RECORDNUMBER" datakey="true" datatype="integer">
          <xsl:value-of select="data[@name='Record number']"/>
        </column>
        <column name="DTBUSINESS" datakey="true" datatype="date" dataformat="yyyy-MM-dd" >
          <xsl:value-of select="data[@name='BizDt']"/>
        </column>
        <column name="MEMBERABICODE" datakey="true" datatype="integer">
          <xsl:value-of select="data[@name='Member ABI Code']"/>
        </column>
        <column name="MARKETID" datakey="true" datatype="integer">
          <xsl:value-of select="data[@name='Exch']"/>
        </column>
        <column name="ACCOUNT" datakey="true" datatype="string">
          <xsl:value-of select="data[@name='AcctTyp']"/>
        </column>
        <column name="POSITIONTYPE" datakey="true" datatype="string">
          <xsl:value-of select="data[@name='Position Type']"/>
        </column>
        <column name="SYMBOL" datakey="true"  datatype="string">
          <xsl:value-of select="data[@name='Sym']"/>
        </column>
        <column name="PRODUCTTYPE" datakey="true" datatype="string">
          <xsl:value-of select="data[@name='Product Type']"/>
        </column>
        <column name="EXPIRY" datakey="true" datatype="date" dataformat="yyyy-MM-dd" >
          <xsl:value-of select="data[@name='MMY']"/>
        </column>
        <column name="OPTIONTYPE" datatype="string">
          <xsl:value-of select="data[@name='PutCall']"/>
        </column>
        <column name="REPOTYPE" datatype="string">
          <xsl:value-of select="data[@name='Repo Type']"/>
        </column>
        <column name="STRIKEPRICE" datatype="decimal">
          <xsl:value-of select="data[@name='StrkPx']"/>
        </column>
        <column name="ISINCODE" datatype="string">
          <xsl:value-of select="data[@name='Isin Code']"/>
        </column>
        <column name="DESCRIPTION" datatype="string">
          <xsl:value-of select="data[@name='Description']"/>
        </column>
        <column name="LONGPOSITION" datatype="integer">
          <xsl:value-of select="data[@name='Long']"/>
        </column>
        <column name="SHORTPOSITION" datatype="integer">
          <xsl:value-of select="data[@name='Short']"/>
        </column>
        <column name="LONGPOSITIONCTRVAL" datatype="decimal">
          <xsl:value-of select="data[@name='Long Position Countervalue']"/>
        </column>
        <column name="SHORTPOSITIONCTRVAL" datatype="decimal">
          <xsl:value-of select="data[@name='Short Position Countervalue']"/>
        </column>
        <column name="LONGACCRUEDCOUPON" datatype="decimal">
          <xsl:value-of select="data[@name='Long Accrued Coupon']"/>
        </column>
        <column name="SHORTACCRUEDCOUPON" datatype="decimal">
          <xsl:value-of select="data[@name='Short Accrued Coupon']"/>
        </column>
        <column name="CURRENCY" datatype="string">
          <xsl:value-of select="data[@name='Currency']"/>
        </column>
        <column name="UNDERLYINGPRICE" datatype="decimal">
          <xsl:value-of select="data[@name='Underling Price']"/>
        </column>
        <column name="GENERALABICODE" datatype="integer">
          <xsl:value-of select="data[@name='General Abi Code']"/>
        </column>
        <column name="DELIVERYABICODE" datatype="integer">
          <xsl:value-of select="data[@name='Delivery Abi Code']"/>
        </column>
        <column name="DELIVERYACCOUNT" datatype="integer">
          <xsl:value-of select="data[@name='Delivery Account']"/>
        </column>
        <column name="POSITIONALREADYDLIV" datatype="integer">
          <xsl:value-of select="data[@name='Position already delivered']"/>
        </column>
        <column name="VALORESOTTOSTANTE" datatype="decimal">
          <xsl:value-of select="data[@name='Valore sottostante']"/>
        </column>
        <column name="FAILEXECUTION" datatype="string">
          <xsl:value-of select="data[@name='Fail/Execution']"/>
        </column>
        <column name="BONDSHARECASH" datatype="string">
          <xsl:value-of select="data[@name='BondShare/Cash']"/>
        </column>
        <column name="BONISMALIS" datatype="string">
          <xsl:value-of select="data[@name='Bonis/Malis']"/>
        </column>
        <column name="MULTIPLIER" datatype="decimal">
          <xsl:value-of select="data[@name='Multiplier']"/>
        </column>
        <column name="SUBACCOUNT" datatype="string">
          <xsl:value-of select="data[@name='SubAccount']"/>
        </column>
        <column name="SETTLEMENTPRICE" datatype="decimal">
          <xsl:value-of select="data[@name='Settlement Price']"/>
        </column>
      </table>
    </row>
  </xsl:template >

</xsl:stylesheet>
