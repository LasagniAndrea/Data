<?xml version="1.0" encoding="utf-8"?>
<!-- FI 20200901 [25468] use of GetUTCDateTimeSys -->
<!-- PL 20190109 Fichier 2019 sans maturités 2w, 2m et 9m (NB: dans l'URL Range=A1:HV9 devient Range=A1:HV6) --> 
<!-- PL 20190109 Add "dbo." --> 
<!-- FI 20131119 Importation only if price exists --> 
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
			
			<xsl:variable name ="Date" ><xsl:value-of select="data[@name='Date']"/></xsl:variable>
			<xsl:variable name ="DateType" ><xsl:value-of select="data/@datatype[../@name='Date']"/></xsl:variable>

      <!-- FI 20131119 add string-length(.)>0 -->
      <xsl:for-each select="data[@name!='Date' and (string-length(.)>0)]" >
					<table name="QUOTE_RATEINDEX_H" action="IU" >
						<xsl:attribute name="sequenceno"><xsl:value-of select="position()"/></xsl:attribute>	
						<mqueue name="QuotationHandlingMQueue" action="IU" />

						<column name="IDMARKETENV" datakey="true" datakeyupd="false" datatype="string"> 
							<SQL command="select" result="IDMARKETENV">
                select IDMARKETENV from dbo.MARKETENV where ISDEFAULT = 1
              </SQL>
						</column>
						<column name="IDVALSCENARIO" datakey="true" datakeyupd="false" datatype="string">
							<SQL command="select" result="IDVALSCENARIO">
                select v.IDVALSCENARIO, 1 as colorder
                from dbo.VALSCENARIO v
                inner join dbo.MARKETENV m on (m.IDMARKETENV = v.IDMARKETENV and m.ISDEFAULT = 1)
                where v.ISDEFAULT = 1
                union
                select v.IDVALSCENARIO, 2 as colorder
                from dbo.VALSCENARIO v
                where v.ISDEFAULT = 1 and v.IDMARKETENV is null
                order by colorder asc
              </SQL>
						</column>
						<column name="IDASSET" datakey="true" datakeyupd="false" datatype="integer">
							<SQL command="select" result="IDASSET">	
								select IDASSET from dbo.ASSET_RATEINDEX where IDENTIFIER = 
								<xsl:choose>
									<xsl:when test="'Week1' = ./@name">'EUR-EURIBOR-Reuters/1W'</xsl:when>
									<!--<xsl:when test="'Week2' = ./@name">'EUR-EURIBOR-Reuters/2W'</xsl:when>-->
									<!--<xsl:when test="'Week3' = ./@name">'EUR-EURIBOR-Reuters/3W'</xsl:when>-->
									<xsl:when test="'Month1' = ./@name">'EUR-EURIBOR-Reuters/1M'</xsl:when>
									<!--<xsl:when test="'Month2' = ./@name">'EUR-EURIBOR-Reuters/2M'</xsl:when>-->
									<xsl:when test="'Month3' = ./@name">'EUR-EURIBOR-Reuters/3M'</xsl:when>
									<!--<xsl:when test="'Month4' = ./@name">'EUR-EURIBOR-Reuters/4M'</xsl:when>-->
									<!--<xsl:when test="'Month5' = ./@name">'EUR-EURIBOR-Reuters/5M'</xsl:when>-->
									<xsl:when test="'Month6' = ./@name">'EUR-EURIBOR-Reuters/6M'</xsl:when>
									<!--<xsl:when test="'Month7' = ./@name">'EUR-EURIBOR-Reuters/7M'</xsl:when>-->
									<!--<xsl:when test="'Month8' = ./@name">'EUR-EURIBOR-Reuters/8M'</xsl:when>-->
									<!--<xsl:when test="'Month9' = ./@name">'EUR-EURIBOR-Reuters/9M'</xsl:when>-->
									<!--<xsl:when test="'Month10' = ./@name">'EUR-EURIBOR-Reuters/10M'</xsl:when>-->
									<!--<xsl:when test="'Month11' = ./@name">'EUR-EURIBOR-Reuters/11M'</xsl:when>-->
									<xsl:when test="'Month12' = ./@name">'EUR-EURIBOR-Reuters/12M'</xsl:when>
								</xsl:choose>
							</SQL>			
						</column>				
						<column name="IDC" datakey="false" datakeyupd="true" datatype="string" >EUR</column>
						<column name="IDBC" datakey="true" datakeyupd="true" datatype="string">EUTA</column>
						<column name="IDM" datakey="false" datakeyupd="true" datatype="integer">null</column>
						<column name="TIME" datakey="true" datakeyupd="false" datatype="date" dataformat="yyyy-MM-dd">
							<xsl:value-of select="$Date"/>
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
								<xsl:value-of select="."/>
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
			</xsl:for-each>
		</row>
	</xsl:template>	
</xsl:stylesheet>
