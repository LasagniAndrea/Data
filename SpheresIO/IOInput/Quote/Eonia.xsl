<?xml version="1.0" encoding="utf-8"?>
<!-- FI 20200901 [25468] use of GetUTCDateTimeSys -->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>
	
	<xsl:decimal-format name="decimalFormat" decimal-separator="." />
	
	<xsl:template match="/iotask">
		<iotask>
			<xsl:attribute name="id"><xsl:value-of select="@id"/></xsl:attribute>
			<xsl:attribute name="name"><xsl:value-of select="@name"/></xsl:attribute>
			<xsl:attribute name="displayname"><xsl:value-of select="@displayname"/></xsl:attribute>
			<xsl:attribute name="loglevel"><xsl:value-of select="@loglevel"/></xsl:attribute>
			<xsl:attribute name="commitmode"><xsl:value-of select="@commitmode"/></xsl:attribute>
			<xsl:apply-templates select="parameters"/>
			<xsl:apply-templates select="iotaskdet"/>
		</iotask>
	</xsl:template>
	
	<xsl:template match="parameters">
		<parameters>
			<xsl:for-each select="parameter" >
				<parameter>
					<xsl:attribute name="id"><xsl:value-of select="@id"/></xsl:attribute>
					<xsl:attribute name="name"><xsl:value-of select="@name"/></xsl:attribute>
					<xsl:attribute name="displayname"><xsl:value-of select="@displayname"/></xsl:attribute>
					<xsl:attribute name="direction"><xsl:value-of select="@direction"/></xsl:attribute>
					<xsl:attribute name="datatype"><xsl:value-of select="@datatype"/></xsl:attribute>
					<xsl:value-of select="."/>
				</parameter>
			</xsl:for-each>
		</parameters>
	</xsl:template>
	
	<xsl:template match="iotaskdet">
		<iotaskdet>
			<xsl:attribute name="id"><xsl:value-of select="@id"/></xsl:attribute>
			<xsl:attribute name="loglevel"><xsl:value-of select="@loglevel"/></xsl:attribute>
			<xsl:attribute name="commitmode"><xsl:value-of select="@commitmode"/></xsl:attribute>
			<xsl:apply-templates select="ioinput"/>
		</iotaskdet>
	</xsl:template>
	
	<xsl:template match="ioinput">
		<ioinput>
			<xsl:attribute name="id"><xsl:value-of select="@id"/></xsl:attribute>
			<xsl:attribute name="name"><xsl:value-of select="@name"/></xsl:attribute>
			<xsl:attribute name="displayname"><xsl:value-of select="@displayname"/></xsl:attribute>
			<xsl:attribute name="loglevel"><xsl:value-of select="@loglevel"/></xsl:attribute>
			<xsl:attribute name="commitmode"><xsl:value-of select="@commitmode"/></xsl:attribute>
			<xsl:apply-templates select="file"/>
		</ioinput>
	</xsl:template>
	
	<xsl:template match="file">
		<file>
			<xsl:attribute name="name"><xsl:value-of select="@name"/></xsl:attribute>
			<xsl:attribute name="folder"><xsl:value-of select="@folder"/></xsl:attribute>
			<xsl:attribute name="date"><xsl:value-of select="@date"/></xsl:attribute>
			<xsl:attribute name="size"><xsl:value-of select="@size"/></xsl:attribute>
			<xsl:apply-templates select="row"/>
		</file>
	</xsl:template>
	
	<xsl:template match="row">
		<row>
			<xsl:attribute name="id"><xsl:value-of select="@id"/></xsl:attribute>
			<xsl:attribute name="src"><xsl:value-of select="@src"/></xsl:attribute>
			<table name="QUOTE_RATEINDEX_H" action="IU" sequenceno="1">
				<mqueue name="QuotationHandlingMQueue" action="IU" />

				<column name="IDMARKETENV" datakey="true" datakeyupd="false" datatype="string"> 
					<SQL command="select" result="IDMARKETENV">
            select IDMARKETENV
            from MARKETENV
            where ISDEFAULT = 1
          </SQL>
				</column>
				<column name="IDVALSCENARIO" datakey="true" datakeyupd="false" datatype="string">
					<SQL command="select" result="IDVALSCENARIO">
            select v.IDVALSCENARIO, 1 as colorder
            from VALSCENARIO v
            inner join MARKETENV m on (m.IDMARKETENV = v.IDMARKETENV and m.ISDEFAULT = 1)
            where v.ISDEFAULT = 1
            union
            select v.IDVALSCENARIO, 2 as colorder
            from VALSCENARIO v
            where v.ISDEFAULT = 1 and v.IDMARKETENV is null
            order by colorder asc
          </SQL>
				</column>
				<column name="IDASSET" datakey="true" datakeyupd="false" datatype="integer">
					<SQL command="select" result="IDASSET">	
					select IDASSET
				    from ASSET_RATEINDEX 
				    where IDENTIFIER = @ASSET
					<Param name="ASSET" datatype="string">EUR-EONIA</Param>
					</SQL>
				</column>				
				<column name="IDC" datakey="false" datakeyupd="true" datatype="string" >EUR</column>
				<column name="IDBC" datakey="true" datakeyupd="true" datatype="string">EUTA</column>
				<column name="IDM" datakey="false" datakeyupd="true" datatype="integer">null</column>
				<column name="TIME" datakey="true" datakeyupd="false" datatype="date" dataformat="yyyy-MM-dd" >
					<xsl:value-of select="data[@name='Date']"/>
					<controls>
						<control action="RejectRow" return="false" >
							<SpheresLib function="IsDate()" />
							<logInfo status="ERROR" isexception="true"> 
								<message>Invalid Type</message>
							</logInfo>
						</control>
					</controls>
				</column>
				<column name="VALUE" datakey="false" datakeyupd="true" datatype="decimal">
					<xsl:variable name="num-value">
						<xsl:value-of select="data[@name='Close']"/>
					</xsl:variable>
					<xsl:value-of select="format-number(number($num-value) div 100, '0.00#######', 'decimalFormat')" />
				</column>
				<column name="SPREADVALUE" datakey="false" datakeyupd="true" datatype="decimal">0</column>				
				<column name="QUOTEUNIT" datakey="false" datakeyupd="true" datatype="string">Rate</column>
				<column name="QUOTESIDE" datakey="true" datakeyupd="true" datatype="string">null</column>
				<column name="QUOTETIMING" datakey="false" datakeyupd="true" datatype="string">Close</column>
				<!-- Bug Spheres IO -->
				<!-- column name="EXPIRITYTIME" datakeyupd="true" datatype="date">null</column -->
				<column name="ASSETMEASURE" datakey="false" datakeyupd="false" datatype="string">MarketQuote</column>
				<column name="CASHFLOWTYPE" datakey="true" datakeyupd="false" datatype="string">null</column>
				<column name="ISENABLED" datakey="false" datakeyupd="true" datatype="bool">1</column>
				<column name="SOURCE" datakey="false" datakeyupd="false" datatype="string">European Central Bank (ECB)</column>
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
				<column name="EXTLLINK" datakey="false" datakeyupd="false" datatype="string">null</column>
			</table>
		</row>
	</xsl:template >
	
</xsl:stylesheet>
