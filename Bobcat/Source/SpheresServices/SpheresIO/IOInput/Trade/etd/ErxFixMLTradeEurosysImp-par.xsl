<?xml version="1.0" encoding="UTF-8"?>
<!-- bcsmessage-par.xsl version2.0 -->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"  xmlns:erx="www.eurexchange.com/technology" version="1.0">
	<xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

	<xsl:template match="/">
		<xsl:element name="file">
			<!-- Copy the attributes of the node <file> -->
			<xsl:attribute name="name"/>
			<xsl:attribute name="folder"/>
			<xsl:attribute name="date"/>
			<xsl:attribute name="size"/>
			<xsl:attribute name="status">success</xsl:attribute>
			<xsl:apply-templates select="erx:FIXML"/>
		</xsl:element>
	</xsl:template>

	<xsl:template match="erx:FIXML">
		<xsl:variable name="idprefix" select="'r'"/>
		<xsl:variable name="idposition" select="position()"/>
		<xsl:element name="row">
			<!-- Copy the attributes of the node <row> -->
			<xsl:attribute name="id"><xsl:value-of select="$idprefix"/><xsl:value-of select="$idposition"/></xsl:attribute>
			<xsl:attribute name="src"><xsl:value-of select="$idposition"/></xsl:attribute>
			<xsl:attribute name="status">success</xsl:attribute>
			<!-- Create <data> node -->
			<xsl:apply-templates select="./erx:TrdCaptRpt"/>
			<xsl:apply-templates select="./erx:TrdCaptRpt/erx:Pty"/>
			<xsl:apply-templates select="./erx:TrdCaptRpt/erx:RptSide"/>
			<xsl:apply-templates select="./erx:TrdCaptRpt/erx:Instrmt"/>
			<xsl:apply-templates select="./erx:TrdCaptRpt/erx:TrdRegTS"/>
		</xsl:element>
	</xsl:template>
		
	<xsl:template match="erx:TrdCaptRpt">
		<xsl:element name="data">
			<xsl:attribute name="name">BizDt</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="./@BizDt"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">OrdDat</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="./@TrdDt"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">LastMkt</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="./@LastMkt"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">Ccy</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="./@Ccy"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">LastQty</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="./@LastQty"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">TrnsfrRsn</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="./@TrnsfrRsn"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">LastPx</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="./@LastPx"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">RptTyp</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="./@RptTyp"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">TransTyp</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="./@TransTyp"/>
		</xsl:element>		
		<xsl:element name="data">
			<xsl:attribute name="name">RptID</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="./@RptID"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">RptRefID</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="./@RptRefID"/>
		</xsl:element>		
	</xsl:template>
	<xsl:template match="erx:TrdCaptRpt/erx:Pty">
		<xsl:choose>
			<xsl:when test="@R='1'">
				<xsl:element name="data">
					<xsl:attribute name="name">ID_R01</xsl:attribute>
					<xsl:attribute name="datatype">string</xsl:attribute>
					<xsl:value-of select="./@ID"/>
				</xsl:element>
			</xsl:when>
			<xsl:when test="@R='4'">
				<xsl:element name="data">
					<xsl:attribute name="name">ID_R04</xsl:attribute>
					<xsl:attribute name="datatype">string</xsl:attribute>
					<xsl:value-of select="./@ID"/>
				</xsl:element>
			</xsl:when>
			<xsl:when test="@R='12'">
				<xsl:element name="data">
					<xsl:attribute name="name">ID_R12</xsl:attribute>
					<xsl:attribute name="datatype">string</xsl:attribute>
					<xsl:value-of select="./@ID"/>
				</xsl:element>
			</xsl:when>
			<xsl:when test="@R='38'">
				<xsl:element name="data">
					<xsl:attribute name="name">ID_R38</xsl:attribute>
					<xsl:attribute name="datatype">string</xsl:attribute>
					<xsl:value-of select="./@ID"/>
				</xsl:element>
			</xsl:when>
			<xsl:when test="@R='95'">
				<xsl:element name="data">
					<xsl:attribute name="name">ID_R95</xsl:attribute>
					<xsl:attribute name="datatype">string</xsl:attribute>
					<xsl:value-of select="./@ID"/>
				</xsl:element>
			</xsl:when>
			<xsl:when test="@R='96'">
				<xsl:element name="data">
					<xsl:attribute name="name">ID_R96</xsl:attribute>
					<xsl:attribute name="datatype">string</xsl:attribute>
					<xsl:value-of select="./@ID"/>
				</xsl:element>
			</xsl:when>
		</xsl:choose>
	</xsl:template>
	<xsl:template match="erx:TrdCaptRpt/erx:RptSide">
		<xsl:element name="data">
			<xsl:attribute name="name">Side</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="./@Side"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">PosEfct</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="./@PosEfct"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">Txt1</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="./@Txt1"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">Txt2</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="./@Txt2"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">OrdID</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="./erx:TrdRptOrdDetl/@OrdID"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">OrdTyp</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>			
			<xsl:value-of select="./erx:TrdRptOrdDetl/@OrdTyp"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">OrdQty</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="./erx:TrdRptOrdDetl/erx:OrdQty/@Qty"/>
		</xsl:element>
	</xsl:template>
	<xsl:template match="erx:TrdCaptRpt/erx:Instrmt">
		<xsl:element name="data">
			<xsl:attribute name="name">PutCall</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="./@PutCall"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">StrkPx</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="./@StrkPx"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">MMY</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="./@MMY"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">OptAt</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="./@OptAt"/>
		</xsl:element>
		<xsl:element name="data">
			<xsl:attribute name="name">Sym</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="./@Sym"/>
		</xsl:element>
	</xsl:template>
	<xsl:template match="erx:TrdCaptRpt/erx:TrdRegTS">
		<xsl:element name="data">
			<xsl:attribute name="name">TS</xsl:attribute>
			<xsl:attribute name="datatype">string</xsl:attribute>
			<xsl:value-of select="./@TS"/>			
		</xsl:element>
	</xsl:template>
			
</xsl:stylesheet>
