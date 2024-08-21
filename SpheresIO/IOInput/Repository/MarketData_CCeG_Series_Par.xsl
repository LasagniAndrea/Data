<?xml version="1.0" encoding="iso-8859-1" ?>
<!--
==============================================================================================
Summary : CCeG - REPOSITORY
File    : MarketData_CCeG_Series_Par.xsl
==============================================================================================
Version : v3.2.0.0                                           
Date    : 20130429
Author  : CC
Comment : File MarketData_Idem_Series_Import_Par.xsl renamed to MarketData_CCeG_Series_Par.xsl
==============================================================================================
Version : v3.2.0.0                                           
Date    : 20130214
Author  : PL
Comment : Management of Market-Id='5' and Market-Id='8'
==============================================================================================
Version : v1.0.0.0
Date    : 20110406
Author  : GP
Description: It handles Market-Id = 3 (Equity)

A vérifier:
1. Comment alimenter l'attribut id avec la position réelle de la ligne parsée dans le fichier?
==============================================================================================
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

	<xsl:template match="/">
		<file>
			<!-- Copy the attributes of the node <file> -->
			<xsl:attribute name="name"></xsl:attribute>
			<xsl:attribute name="folder"></xsl:attribute>
			<xsl:attribute name="date"></xsl:attribute>
			<xsl:attribute name="size"></xsl:attribute>
			<xsl:attribute name="status">success</xsl:attribute>
			<xsl:apply-templates select="Flow"/>
		</file>
	</xsl:template>

	<xsl:template match="Flow">
		<xsl:apply-templates select="Data"/>
	</xsl:template>

	<xsl:template match="Data">
    <xsl:if test="Market-Id='2' or Market-Id='5' or Market-Id='8'">
			<xsl:variable name="idprefix" select="'r'"/>
			<xsl:variable name="idposition" select="position()"/>
			<row>
				<!-- Copy the attributes of the node <row> -->
				<xsl:attribute name="id">
					<xsl:value-of select="$idprefix"/>
					<xsl:value-of select="$idposition"/>
				</xsl:attribute>
				<xsl:attribute name="src">
					<xsl:value-of select="$idposition"/>
				</xsl:attribute>
				<xsl:attribute name="status">success</xsl:attribute>

				<data name="MI" datatype="string">
					<xsl:value-of select="Market-Id"/>
				</data>
				<data name="SY" datatype="string">
					<xsl:value-of select="Symbol"/>
				</data>
				<data name="CT" datatype="string">
					<xsl:value-of select="Class-Type"/>
				</data>
				<data name="IC" datatype="string">
					<xsl:value-of select="Isin-Code"/>
				</data>
				<data name="EX" datatype="string">
					<xsl:value-of select="Expiry"/>
				</data>

				<!-- For Option only: Strike Price and PutCall -->
				<xsl:if test="Class-Type='O'">
					<data name="SP" datatype="decimal">
						<xsl:value-of select="Strike-Price"/>
					</data>
					<data name="PC" datatype="string">
						<xsl:value-of select="P-C"/>
					</data>
				</xsl:if>

				<data name="ED" datatype="date" dataformat="yyyyMMdd">
					<xsl:value-of select="Expiry-Date"/>
				</data>
				<data name="LS" datatype="date" dataformat="yyyyMMdd">
					<xsl:value-of select="Last-Trade-Day"/>
				</data>

				<!-- Code Used By Market: Warning, frequently not filled -->
				<xsl:if test="string-length(Code-Used-By-Market) > 0">
					<data name="CD" datatype="string">
						<xsl:value-of select="Code-Used-By-Market"/>
					</data>
				</xsl:if>

        <!-- PL 20130221 New -->
        <!-- For IDEX only: Contract Multiplier -->
        <xsl:if test="Market-Id='5'">
          <data name="MU" datatype="decimal">
            <xsl:value-of select="Multiplier"/>
          </data>
        </xsl:if>
			</row>
		</xsl:if>
	</xsl:template>
</xsl:stylesheet>
