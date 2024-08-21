<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>
  <!-- FI 20120215 use parameter for debug only-->
  <!--<xsl:param name="pCOAcro"  select ="'CME'"/>-->
  
  <xsl:template match="/">
    <file>
      <xsl:attribute name="name"></xsl:attribute>
      <xsl:attribute name="folder"></xsl:attribute>
      <xsl:attribute name="date"></xsl:attribute>
      <xsl:attribute name="size"></xsl:attribute>
      <xsl:attribute name="status">success</xsl:attribute>
      <!-- FI 20120215 use parameter for debug only-->
      <!--<xsl:apply-templates select="spanOrgMastFile/ClearingOrgMaster/r[COAcro/text()=$pCOAcro]/ExchangeMaster"/>-->
      <xsl:apply-templates select="spanOrgMastFile/ClearingOrgMaster/r/ExchangeMaster"/>
    </file>
  </xsl:template>

  <xsl:template match="ExchangeMaster">
    <xsl:apply-templates select="./r/ProductFamilyMaster/r"/>
  </xsl:template>

  <xsl:template match="r">
    <xsl:variable name="vCOAcro">
      <xsl:value-of select ="../../../../COAcro/text()"/>
    </xsl:variable>
    <xsl:variable name="vExchAcro">
      <xsl:value-of select ="../../ExchAcro/text()"/>
    </xsl:variable>
    <xsl:variable name="vPosition" select="position()"/>
    <xsl:element name="row">
      <xsl:attribute name="id">
        <xsl:value-of select="'r'"/>_<xsl:value-of select="$vCOAcro"/>_<xsl:value-of select="$vExchAcro"/>_<xsl:value-of select="$vPosition"/>
      </xsl:attribute>
      <xsl:attribute name="status">
        <xsl:value-of select="'success'"/>
      </xsl:attribute>

      <xsl:element name="data">
        <xsl:attribute name="name">COAcro</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select ="$vCOAcro"/>
      </xsl:element>

      <xsl:element name="data">
        <xsl:attribute name="name">ExchAcro</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select ="$vExchAcro"/>
      </xsl:element>

      <xsl:element name="data">
        <xsl:attribute name="name">PFCode</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select ="PFCode/text()"/>
      </xsl:element>

      <xsl:element name="data">
        <xsl:attribute name="name">Type</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select ="Type/text()"/>
      </xsl:element>
      
      <xsl:element name="data">
        <xsl:attribute name="name">PFName</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select ="PFName/text()"/>
      </xsl:element>
     

      <xsl:element name="data">
        <xsl:attribute name="name">CVM</xsl:attribute>
        <xsl:attribute name="datatype">decimal</xsl:attribute>
        <xsl:value-of select ="CVM/text()"/>
      </xsl:element>
      
      <xsl:element name="data">
        <xsl:attribute name="name">SettleDecLoc</xsl:attribute>
        <xsl:attribute name="datatype">int</xsl:attribute>
        <xsl:value-of select ="SettleDecLoc/text()"/>
      </xsl:element>

      
      <xsl:element name="data">
        <xsl:attribute name="name">StrikeDecLoc</xsl:attribute>
        <xsl:attribute name="datatype">int</xsl:attribute>
        <xsl:value-of select ="StrikeDecLoc/text()"/>
      </xsl:element>

      <xsl:element name="data">
        <xsl:attribute name="name">SettleAlignCode</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select ="SettleAlignCode/text()"/>
      </xsl:element>


      <xsl:element name="data">
        <xsl:attribute name="name">StrikeAlignCode</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select ="StrikeAlignCode/text()"/>
      </xsl:element>


      <xsl:element name="data">
        <xsl:attribute name="name">CabinetOptionValue</xsl:attribute>
        <xsl:attribute name="datatype">decimal</xsl:attribute>
        <xsl:value-of select ="CabinetOptionValue/text()"/>
      </xsl:element>

      <xsl:element name="data">
        <xsl:attribute name="name">SkipOnLoad</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select ="SkipOnLoad/text()"/>
      </xsl:element>

      <xsl:element name="data">
        <xsl:attribute name="name">CurrentlyActive</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select ="CurrentlyActive/text()"/>
      </xsl:element>

      <xsl:element name="data">
        <xsl:attribute name="name">PricingModel</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select ="PricingModel/text()"/>
      </xsl:element>
      
      <xsl:element name="data">
        <xsl:attribute name="name">PriceQuotationMethod</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select ="PriceQuotationMethod/text()"/>
      </xsl:element>
      
      <xsl:element name="data">
        <xsl:attribute name="name">ValuationMethod</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select ="ValuationMethod/text()"/>
      </xsl:element>

      <xsl:element name="data">
        <xsl:attribute name="name">SettlementMethod</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select ="SettlementMethod/text()"/>
      </xsl:element>
      
      <xsl:element name="data">
        <xsl:attribute name="name">SettleCurrencyCode</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select ="SettleCurrencyCode/text()"/>
      </xsl:element>

      <xsl:element name="data">
        <xsl:attribute name="name">CountryCode</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select ="CountryCode/text()"/>
      </xsl:element>

      <xsl:element name="data">
        <xsl:attribute name="name">ExerciseStyle</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select ="ExerciseStyle/text()"/>
      </xsl:element>

    </xsl:element>

  </xsl:template>
</xsl:stylesheet>