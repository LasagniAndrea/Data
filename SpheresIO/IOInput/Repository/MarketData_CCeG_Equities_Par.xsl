<?xml version="1.0" encoding="iso-8859-1" ?>
<!--
================================================================================================
Summary : CCeG - REPOSITORY
File    : MarketData_CCeG_Equities_Par.xsl
================================================================================================
Version : v3.2.0.0                                           
Date    : 20130429
Author  : CC
Comment : File MarketData_Idem_Equity_Import_Par.xsl renamed to MarketData_CCeG_Equities_Par.xsl
================================================================================================
Version : v1.0.0.0
Date    : 20110406
Author  : Guido
Description: It handles Market-Id = 3 (Equity)

A vérifier:
1. Comment alimenter l'attribut id avec la position réelle de la ligne parsée dans le fichier?
================================================================================================
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
		<!-- The ClassFile contains many data (Equity, Bonds, Derivative Contracts) -->
		<!-- We want to parse the Equities -->
		<xsl:if test="Market-Id='3' and Class-Type='C' and Product-Type='E'">
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
				<data name="PS" datatype="string">
					<xsl:value-of select="Product-Style"/>
				</data>
				<data name="UC" datatype="string">
					<xsl:value-of select="Underlying-Code"/>
				</data>
				<data name="UI" datatype="string">
					<xsl:value-of select="Underlying-Isin-Code"/>
				</data>
				<data name="CU" datatype="string">
					<xsl:value-of select="Currency"/>
				</data>
				<data name="DE" datatype="string">
					<xsl:value-of select="Description"/>
				</data>
				<data name="PT" datatype="string">
					<xsl:value-of select="Product-Type"/>
				</data>
				<data name="MU" datatype="decimal">
					<xsl:value-of select="Multiplier"/>
				</data>
			</row>
		</xsl:if>
	</xsl:template>
</xsl:stylesheet>
