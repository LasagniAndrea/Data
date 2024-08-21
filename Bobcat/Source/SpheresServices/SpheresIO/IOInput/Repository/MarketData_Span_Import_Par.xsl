<!-- FI 20130222 [18419] new integration of derivative Contracts -->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8"
              indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- Includes-->
  <xsl:include href="MarketData_Common.xsl"/>

  <xsl:key name="futPf" match="futPf" use="pfId"/>

  <xsl:template match="/">
    <xsl:element name="file">
      <!-- Copy the attributes of the node <file> -->
      <xsl:attribute name="name"/>
      <xsl:attribute name="folder"/>
      <xsl:attribute name="date"/>
      <xsl:attribute name="size"/>
      <xsl:attribute name="status">success</xsl:attribute>
      <xsl:apply-templates select="spanFile/pointInTime/clearingOrg"/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="clearingOrg">
    <xsl:apply-templates select="exchange">
      <xsl:with-param name="pClearingOrgCode">
        <xsl:value-of select="ec"/>
      </xsl:with-param>
      <xsl:with-param name="pClearingOrgName">
        <xsl:value-of select="name"/>
      </xsl:with-param>
    </xsl:apply-templates>
  </xsl:template>

  <!-- Market -->
  <xsl:template match="exchange">
    <xsl:param name="pClearingOrgCode"/>
    <xsl:param name="pClearingOrgName"/>

    <xsl:variable name="vExchangeCode" select="exch"/>
    <xsl:variable name="vElementType" select="'exchange'"/>
    <xsl:variable name="vPosition" select="position()"/>

    <!-- Market Row -->
    <xsl:element name="row">
      <xsl:call-template name="IORowAtt">
        <xsl:with-param name="pId">
          <xsl:value-of select="$vDataTypeMarket"/>
        </xsl:with-param>
        <xsl:with-param name="pSrc">
          <xsl:value-of select="$vElementType"/>_<xsl:value-of select="$vPosition"/>
        </xsl:with-param>
      </xsl:call-template>

      <xsl:element name="data">
        <!--xsl:attribute name="name">ClearingOrgCode</xsl:attribute-->
        <xsl:attribute name="name">COC</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="$pClearingOrgCode"/>
      </xsl:element>
      <xsl:element name="data">
        <!--xsl:attribute name="name">ClearingOrgName</xsl:attribute-->
        <xsl:attribute name="name">CON</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="$pClearingOrgName"/>
      </xsl:element>
      <xsl:element name="data">
        <!--xsl:attribute name="name">ExchangeCode</xsl:attribute-->
        <xsl:attribute name="name">EC</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="$vExchangeCode"/>
      </xsl:element>
      <xsl:element name="data">
        <!--xsl:attribute name="name">ExchangeName</xsl:attribute-->
        <xsl:attribute name="name">EN</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="name"/>
      </xsl:element>
    </xsl:element>

    <!-- Futures Rows -->
    <xsl:apply-templates select="futPf"/>/

    <!-- Options Rows -->
    <xsl:apply-templates select="ooePf"/>
    <xsl:apply-templates select="oofPf"/>
    <xsl:apply-templates select="oopPf"/>
  </xsl:template>

  <!-- Futures Contract-->
  <!-- http://www.cme-ch.com/span/span4_xml_dtddesc.htm#futPf -->
  <xsl:template match="futPf">
    <xsl:variable name="vContractId">
      <xsl:value-of select="pfId"/>
    </xsl:variable>

    <xsl:element name="row">
      <xsl:call-template name="RowContractAtt">
        <xsl:with-param name="pContractId">
          <xsl:value-of select="$vContractId"/>
        </xsl:with-param>
        <xsl:with-param name="pElementType">
          <xsl:value-of select="'futPf'"/>
        </xsl:with-param>
      </xsl:call-template>

      <xsl:call-template name="ProductFamilyData">
        <xsl:with-param name="pCategory" select="'F'"/>
      </xsl:call-template>
    </xsl:element>

    <!-- Futures series -->
    <!-- FI 20130222 [18419] Mise en commentaire car seuls les DCs son chargés -->
    <!--<xsl:apply-templates select="fut">
      <xsl:with-param name="pContractId">
        <xsl:value-of select="$vContractId"/>
      </xsl:with-param>
      <xsl:with-param name="pContractSymbol">
        <xsl:value-of select="pfCode"/>
      </xsl:with-param>
    </xsl:apply-templates>-->

  </xsl:template>

  <xsl:template match="fut">
    <xsl:param name="pContractId"/>
    <xsl:param name="pContractSymbol"/>

    <xsl:element name="row">
      <xsl:call-template name="RowExpiryAtt">
        <xsl:with-param name="pAttribId">
          <xsl:value-of select="pe"/>_<xsl:value-of select="$pContractId"/>
        </xsl:with-param>
        <xsl:with-param name="pElementType">
          <xsl:value-of select="'fut'"/>
        </xsl:with-param>
      </xsl:call-template>

      <xsl:call-template name="ContractAttribData">
        <xsl:with-param name="pContractId">
          <xsl:value-of select="$pContractId"/>
        </xsl:with-param>
      </xsl:call-template>

      <xsl:element name="data">
        <!--xsl:attribute name="name">SettlementDate</xsl:attribute-->
        <xsl:attribute name="name">SD</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="setlDate"/>
      </xsl:element>
      <xsl:element name="data">
        <!--xsl:attribute name="name">AssetSymbol</xsl:attribute-->
        <xsl:attribute name="name">AS</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="cId"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <!-- Options Contract / UnderlyingGroup: F / UnderlyingAsset: FS-->
  <!-- ooePf => Options On Equities Product Family (option sur action)-->
  <!-- http://www.cme-ch.com/span/span4_xml_dtddesc.htm#ooePf -->
  <xsl:template match="ooePf">
    <xsl:variable name="vContractId">
      <xsl:value-of select="pfId"/>
    </xsl:variable>

    <xsl:element name="row">
      <xsl:call-template name="RowContractAtt">
        <xsl:with-param name="pContractId">
          <xsl:value-of select="$vContractId"/>
        </xsl:with-param>
        <xsl:with-param name="pElementType">
          <xsl:value-of select="'ooePf'"/>
        </xsl:with-param>
      </xsl:call-template>

      <xsl:call-template name="Option"/>

      <xsl:call-template name="dataUnderlyingGroup">
        <xsl:with-param name="pUnderlyingGroup">
          <xsl:value-of select="'F'"/>
        </xsl:with-param>
      </xsl:call-template>
      <xsl:call-template name="dataUnderlyingAsset">
        <xsl:with-param name="pUnderlyingAsset">
          <xsl:value-of select="'FS'"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:element>

    <!-- Options series-->
    <!-- FI 20130222 [18419] Mise en commentaire car seuls les DCs son chargés -->
    <!--<xsl:apply-templates select="series">
      <xsl:with-param name="pContractId">
        <xsl:value-of select="$vContractId"/>
      </xsl:with-param>
      <xsl:with-param name="pContractSymbol">
        <xsl:value-of select="pfCode"/>
      </xsl:with-param>
    </xsl:apply-templates>-->
  </xsl:template>

  <!-- Options Contract / UnderlyingGroup: F -->
  <!-- oofPf => Options On Futures Product Family (option sur future)-->
  <!-- http://www.cme-ch.com/span/span4_xml_dtddesc.htm#oofPf -->
  <xsl:template match="oofPf">
    <xsl:variable name="vContractId">
      <xsl:value-of select="pfId"/>
    </xsl:variable>

    <xsl:element name="row">
      <xsl:call-template name="RowContractAtt">
        <xsl:with-param name="pContractId">
          <xsl:value-of select="$vContractId"/>
        </xsl:with-param>
        <xsl:with-param name="pElementType">
          <xsl:value-of select="'oofPf'"/>
        </xsl:with-param>
      </xsl:call-template>

      <xsl:call-template name="Option"/>

      <xsl:call-template name="dataUnderlyingGroup">
        <xsl:with-param name="pUnderlyingGroup">
          <xsl:value-of select="'F'"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:element>
    
    <!-- Options series-->
    <!-- FI 20130222 [18419] Mise en commentaire car seuls les DCs son chargés -->
    <!--<xsl:apply-templates select="series">
      <xsl:with-param name="pContractId">
        <xsl:value-of select="$vContractId"/>
      </xsl:with-param>
      <xsl:with-param name="pContractSymbol">
        <xsl:value-of select="pfCode"/>
      </xsl:with-param>
    </xsl:apply-templates>-->
    
  </xsl:template>

  <!-- Options Contract / UnderlyingGroup: C-->
  <!-- oopPf => Options On Physicals Product Family (option sur matières premieres) -->
  <!-- http://www.cme-ch.com/span/span4_xml_dtddesc.htm#oopPf -->
  <xsl:template match="oopPf">
    <xsl:variable name="vContractId">
      <xsl:value-of select="pfId"/>
    </xsl:variable>

    <xsl:element name="row">
      <xsl:call-template name="RowContractAtt">
        <xsl:with-param name="pContractId">
          <xsl:value-of select="$vContractId"/>
        </xsl:with-param>
        <xsl:with-param name="pElementType">
          <xsl:value-of select="'oopPf'"/>
        </xsl:with-param>
      </xsl:call-template>

      <xsl:call-template name="Option"/>

      <xsl:call-template name="dataUnderlyingGroup">
        <xsl:with-param name="pUnderlyingGroup">
          <xsl:value-of select="'C'"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:element>

    <!-- Options series-->
    <!-- FI 20130222 [18419] Mise en commentaire car seuls les DCs son chargés -->
    <!--<xsl:apply-templates select="series">
      <xsl:with-param name="pContractId">
        <xsl:value-of select="$vContractId"/>
      </xsl:with-param>
      <xsl:with-param name="pContractSymbol">
        <xsl:value-of select="pfCode"/>
      </xsl:with-param>
    </xsl:apply-templates>-->
  </xsl:template>

  <!-- Option templates-->
  <xsl:template match="series">
    <xsl:param name="pContractId"/>
    <xsl:param name="pContractSymbol"/>

    <xsl:variable name="vExpiryId">
      <xsl:value-of select="pe"/>_<xsl:value-of select="$pContractId"/>
    </xsl:variable>

    <xsl:element name="row">
      <xsl:call-template name="RowExpiryAtt">
        <xsl:with-param name="pAttribId">
          <xsl:value-of select="$vExpiryId"/>
        </xsl:with-param>
        <xsl:with-param name="pElementType">
          <xsl:value-of select="'series'"/>
        </xsl:with-param>
      </xsl:call-template>

      <xsl:call-template name="ContractAttribData">
        <xsl:with-param name="pContractId">
          <xsl:value-of select="$pContractId"/>
        </xsl:with-param>
      </xsl:call-template>

      <xsl:variable name="vCId" select="undC/cId"/>
      <xsl:variable name="futProductFamily" select="key('futPf',../undPf/pfId)"/>

      <xsl:for-each select="$futProductFamily/fut">
        <xsl:if test="cId=$vCId">
          <xsl:element name="data">
            <!--xsl:attribute name="name">UnderlyingContractMaturity</xsl:attribute-->
            <xsl:attribute name="name">UCM</xsl:attribute>
            <xsl:attribute name="datatype">string</xsl:attribute>
            <xsl:value-of select="pe"/>_<xsl:value-of select="cId"/>
          </xsl:element>
        </xsl:if>
      </xsl:for-each>
    </xsl:element>

    <!-- FI 20130222 [18419] Mise en commentaire, L'importation ne charge pas les assets  -->
    <!--<xsl:apply-templates select="opt">
      <xsl:with-param name="pExpiryId">
        <xsl:value-of select="$vExpiryId"/>
      </xsl:with-param>
    </xsl:apply-templates>-->
    
  </xsl:template>

  <xsl:template match="opt">
    <xsl:param name="pExpiryId"/>

    <xsl:variable name="vAssetId">
      <xsl:value-of select="cId"/>_<xsl:value-of select="$pExpiryId"/>
    </xsl:variable>

    
    <xsl:element name="row">
			<xsl:call-template name="RowAssetAtt">
				<xsl:with-param name="pAssetId"><xsl:value-of select="$vAssetId"/></xsl:with-param>
				<xsl:with-param name="pElementType"><xsl:value-of select="'opt'"/></xsl:with-param>
			</xsl:call-template>
			
			<xsl:element name="data">
        <xsl:attribute name="name">PutCall</xsl:attribute>
				<xsl:attribute name="name">PC</xsl:attribute>
				<xsl:attribute name="datatype">string</xsl:attribute>
				<xsl:value-of select="o"/>
			</xsl:element>
			<xsl:element name="data">
        <xsl:attribute name="name">StrikePrice</xsl:attribute>
				<xsl:attribute name="name">SP</xsl:attribute>
				<xsl:attribute name="datatype">decimal</xsl:attribute>
				<xsl:value-of select="k"/>
			</xsl:element>
      <xsl:element name="data">
        <xsl:attribute name="name">AssetSymbol</xsl:attribute>
        <xsl:attribute name="name">AS</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="cId"/>
      </xsl:element>
		</xsl:element>
  </xsl:template>

  <xsl:template name="Option">
    <xsl:call-template name="ProductFamilyData">
      <xsl:with-param name="pCategory" select="'O'"/>
    </xsl:call-template>

    <xsl:element name="data">
      <!--xsl:attribute name="name">ExerciseStyle</xsl:attribute-->
      <xsl:attribute name="name">ES</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="exercise"/>
    </xsl:element>
    <!--xsl:element name="data">
			<xsl:attribute name="name">StrikeDecLocator</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="strikeDl"/>
		</xsl:element-->
    <!--xsl:element name="data">
      <xsl:attribute name="name">CabinetOptValue</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="cab"/>
		</xsl:element-->
    <xsl:element name="data">
      <!--xsl:attribute name="name">NominalValue</xsl:attribute-->
      <xsl:attribute name="name">NV</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="notionAmt"/>
    </xsl:element>
    <!--xsl:element name="data">
      <xsl:attribute name="name">UnderlyingExchangeCode</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="undPf/exch"/>
		</xsl:element>-->
    <xsl:element name="data">
      <!--http://www.cme-ch.com/span/span4_xml_dtddesc.htm#pfType-->
      <!--<xsl:attribute name="name">UnderlyingContractType</xsl:attribute>-->
      <xsl:attribute name="name">UCT</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="undPf/pfType"/>
    </xsl:element>
    <xsl:element name="data">
      <!--xsl:attribute name="name">UnderlyingContractSymbol</xsl:attribute-->
      <xsl:attribute name="name">UCS</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="undPf/pfCode"/>
    </xsl:element>
  </xsl:template>

  <!-- **********************************************************************-->
  <!-- Common templates 	                                                   -->
  <!-- **********************************************************************-->

  <xsl:template name="RowContractAtt">
    <xsl:param name="pContractId"/>
    <xsl:param name="pElementType"/>

    <xsl:call-template name="IORowAtt">
      <xsl:with-param name="pId">
        <xsl:value-of select="$vDataTypeContract"/>_<xsl:value-of select="$pContractId"/>
      </xsl:with-param>
      <xsl:with-param name="pSrc">
        <xsl:value-of select="$pElementType"/>_<xsl:value-of select="position()"/>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="RowExpiryAtt">
    <xsl:param name="pAttribId"/>
    <xsl:param name="pElementType"/>
    <xsl:call-template name="IORowAtt">
      <xsl:with-param name="pId">
        <xsl:value-of select="$vDataTypeExpiry"/>_<xsl:value-of select="$pAttribId"/>
      </xsl:with-param>
      <xsl:with-param name="pSrc">
        <xsl:value-of select="$pElementType"/>_<xsl:value-of select="position()"/>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="RowAssetAtt">
    <xsl:param name="pAssetId"/>
    <xsl:param name="pElementType"/>

    <xsl:call-template name="IORowAtt">
      <xsl:with-param name="pId">
        <xsl:value-of select="$vDataTypeAsset"/>_<xsl:value-of select="$pAssetId"/>
      </xsl:with-param>
      <xsl:with-param name="pSrc">
        <xsl:value-of select="$pElementType"/>_<xsl:value-of select="position()"/>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="dataUnderlyingGroup">
    <xsl:param name="pUnderlyingGroup"/>

    <xsl:element name="data">
      <!--xsl:attribute name="name">UnderlyingGroup</xsl:attribute-->
      <xsl:attribute name="name">UG</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="$pUnderlyingGroup"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="dataUnderlyingAsset">
    <xsl:param name="pUnderlyingAsset"/>

    <xsl:element name="data">
      <!--xsl:attribute name="name">UnderlyingAsset</xsl:attribute-->
      <xsl:attribute name="name">UA</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="$pUnderlyingAsset"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="ProductFamilyData">
    <xsl:param name="pCategory"/>

    <xsl:element name="data">
      <!--xsl:attribute name="name">Category</xsl:attribute-->
      <xsl:attribute name="name">CAT</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:copy-of select="$pCategory"/>
    </xsl:element>
    <xsl:element name="data">
      <!--xsl:attribute name="name">ContractSymbol</xsl:attribute-->
      <xsl:attribute name="name">CS</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="pfCode"/>
    </xsl:element>
    <xsl:element name="data">
      <!--xsl:attribute name="name">ContractName</xsl:attribute-->
      <xsl:attribute name="name">CN</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="name"/>
    </xsl:element>

    <!--xsl:element name="data">
			<xsl:attribute name="name">ContractShortName</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="alias[@aType='CNAME' and position()=1]/@aVal"/>
		</xsl:element-->

    <xsl:element name="data">
      <!--xsl:attribute name="name">Currency</xsl:attribute-->
      <xsl:attribute name="name">CU</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="currency"/>
    </xsl:element>
    <xsl:element name="data">
      <!--xsl:attribute name="name">Multiplier</xsl:attribute-->
      <xsl:attribute name="name">MLT</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="contractsize"/>
    </xsl:element>
    <!--xsl:element name="data">
			<xsl:attribute name="name">uomQty</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="uomQty"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">uom</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="uom"/>
		</xsl:element-->
    <xsl:element name="data">
      <!--xsl:attribute name="name">Factor</xsl:attribute-->
      <xsl:attribute name="name">FA</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="cvf"/>
    </xsl:element>
    <!--xsl:element name="data">
			<xsl:attribute name="name">PriceDecLocator</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="priceDl"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">FutValuationMethod</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="valueMeth"/>
		</xsl:element-->
    <xsl:element name="data">
      <!--xsl:attribute name="name">SettltMethod</xsl:attribute-->
      <xsl:attribute name="name">SM</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="setlMeth"/>
    </xsl:element>
    <!--xsl:element name="data">
			<xsl:attribute name="name">PricingMethod</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="priceMeth"/>
		</xsl:element-->
  </xsl:template>
  <xsl:template match="alias">
    <xsl:if test="'CNAME' = aType">
      <xsl:element name="data">
        <!--xsl:attribute name="name">ContractShortName</xsl:attribute-->
        <xsl:attribute name="name">CSN</xsl:attribute>
        <xsl:attribute name="datatype">string</xsl:attribute>
        <xsl:value-of select="aVal"/>
      </xsl:element>
    </xsl:if>
  </xsl:template>
  <xsl:template name="ContractAttribData">
    <xsl:param name="pContractId"/>

    <xsl:element name="data">
      <!--xsl:attribute name="name">MaturityMonthYear</xsl:attribute-->
      <xsl:attribute name="name">MMY</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="pe"/>
    </xsl:element>
    <xsl:element name="data">
      <!--xsl:attribute name="name">MaturityDate</xsl:attribute-->
      <xsl:attribute name="name">MD</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="setlDate"/>
    </xsl:element>
    <xsl:element name="data">
      <!--xsl:attribute name="name">FirstTradingDate</xsl:attribute-->
      <xsl:attribute name="name">FTD</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="fdot"/>
    </xsl:element>
    <xsl:element name="data">
      <!--xsl:attribute name="name">LastTradingDate</xsl:attribute-->
      <xsl:attribute name="name">LTD</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="ldot"/>
    </xsl:element>
    <xsl:element name="data">
      <!--xsl:attribute name="name">Multiplier</xsl:attribute-->
      <xsl:attribute name="name">MLT</xsl:attribute>
      <xsl:attribute name="datatype">string</xsl:attribute>
      <xsl:value-of select="cvf"/>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
