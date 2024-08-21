<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

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
			<xsl:apply-templates select="iooutput"/>
		</iotaskdet>
	</xsl:template>
	<xsl:template match="iooutput">
		<iooutput>
			<xsl:attribute name="id"><xsl:value-of select="@id"/></xsl:attribute>
			<xsl:attribute name="name"><xsl:value-of select="@name"/></xsl:attribute>
			<xsl:attribute name="displayname"><xsl:value-of select="@displayname"/></xsl:attribute>
			<xsl:attribute name="loglevel"><xsl:value-of select="@loglevel"/></xsl:attribute>
			<xsl:attribute name="commitmode"><xsl:value-of select="@commitmode"/></xsl:attribute>
			<xsl:apply-templates select="file"/>
		</iooutput>
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
			<xsl:apply-templates select="table"/>
		</row>
	</xsl:template>
	<xsl:template match="table">
		<xsl:apply-templates select="column"/>		
	</xsl:template>
	<xsl:template match="column">		
			<data>
				<xsl:attribute name="name">
					<xsl:value-of select="@name"/>
				</xsl:attribute>
				<xsl:attribute name="datatype">
					<xsl:value-of select="@datatype"/>
				</xsl:attribute>
				<xsl:if test="@datatype!='xml' and @datatype!='text'">
					<xsl:value-of disable-output-escaping="no" select="text()"/>
				</xsl:if>
				<xsl:if test="@datatype='text'">
					<xsl:variable name="vCdata1"><![CDATA[<![]]>CDATA[</xsl:variable>
					<xsl:variable name="vCdata2">]]<![CDATA[>]]></xsl:variable>
					<xsl:value-of disable-output-escaping="yes" select="$vCdata1"/>
					<xsl:value-of disable-output-escaping="yes" select="."/>
					<xsl:value-of disable-output-escaping="yes" select="$vCdata2"/>
				</xsl:if>
				<xsl:if test="@datatype='xml'">
					<xsl:copy-of select="ValueXML"/>
				</xsl:if>
			</data>
	</xsl:template>
</xsl:stylesheet>
