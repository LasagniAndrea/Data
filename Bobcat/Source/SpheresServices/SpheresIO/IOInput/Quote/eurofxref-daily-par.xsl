<?xml version="1.0" encoding="ISO-8859-1" ?> 
<xsl:stylesheet  
xmlns:tt="http://www.ecb.int/vocabulary/2002-08-01/eurofxref" 
xmlns:gesmes="http://www.gesmes.org/xml/2002-08-01" 
xmlns:xsl="http://www.w3.org/1999/XSL/Transform"  
version="1.0">
<xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" 
            indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

	<xsl:template match="/">
		<file> 
			<!-- Copy the attributes of the node <file> -->
			<xsl:attribute name="name"></xsl:attribute> 
			<xsl:attribute name="folder"></xsl:attribute>
			<xsl:attribute name="date"></xsl:attribute>
			<xsl:attribute name="size"></xsl:attribute>
			<xsl:attribute name="status">success</xsl:attribute>
			
			<xsl:apply-templates select="gesmes:Envelope"/>
			
		</file>
	</xsl:template>

	<xsl:template match="gesmes:Envelope">
		<xsl:apply-templates select="gesmes:subject"/>
		<xsl:apply-templates select="gesmes:Sender"/>
		<xsl:apply-templates select="tt:Cube"/>
	</xsl:template>   

	<xsl:template match="gesmes:subject">
		<!--
		<subject>
			 <xsl:value-of select="."/>
		</subject>
		-->
	</xsl:template>   

	<xsl:template match="gesmes:Sender">
		<!--
		<senderName>
			<xsl:value-of select="gesmes:name"/>
		</senderName>	
		-->
	</xsl:template>   

	<xsl:template match="tt:Cube">

		<!--
		<Time>
		<xsl:value-of select="tt:Cube/@time"/>
		</Time>
		-->
		<xsl:apply-templates select="tt:Cube/tt:Cube" mode="Child"/>
	</xsl:template>   

	<xsl:template match="tt:Cube/tt:Cube" mode="Child">
			<xsl:variable name="idprefix" select="'r'"/>
			<xsl:variable name="idposition" select="position()"/>
			<row>
				<!-- Copy the attributes of the node <row> --> 
				<xsl:attribute name="id">
					<xsl:value-of select="$idprefix"/><xsl:value-of select="$idposition"/>
				</xsl:attribute>
				<xsl:attribute name="src">
					<xsl:value-of select="$idposition"/>
				</xsl:attribute>
				<xsl:attribute name="status">success</xsl:attribute>
					<data name="time" datatype="date" dataformat="yyyy-MM-dd"><xsl:value-of select="../@time"/></data>
					<data name="currency" datatype="string"><xsl:value-of select="@currency"/></data>
					<data name="rate" datatype="decimal"><xsl:value-of select="@rate"/></data>
			</row>		
	</xsl:template>
</xsl:stylesheet>
