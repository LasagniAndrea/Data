<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt" version="1.0">
  
	<xsl:output method="html" indent="yes" encoding="UTF-8" />
  <xsl:param name="pCurrentCulture" select="'en-GB'"/>
  
	<!-- xslt includes -->
  <xsl:include href="..\Library\Resource.xslt"/>
  <xsl:include href="..\Library\StrFunc.xslt"/>
	
	<!-- Table -->	
	<xsl:template match="table">
		<table id="tblCompare" border="1" cellpadding="2" cellspacing="0" rules="cols" style="height:100%;width:100%;border-collapse:collapse;">
			<xsl:apply-templates select="tr"/>
		</table>
	</xsl:template>

	<!-- TableRow -->	
	<xsl:template match="tr">
		<tr class="XMLDataGridGray_ItemStyle">
			<td class="XmlDiff_Counter"><xsl:value-of select="position()"/></td>
			<xsl:apply-templates select="td">
				<xsl:with-param name="pRow" select="position()"/>
			</xsl:apply-templates>
		</tr>
	</xsl:template>
	
	<!-- TableCell -->	
	<xsl:template match="td">
		<xsl:param name="pRow"/>
		<td class="XmlDiff">
			<xsl:call-template name="SetStyle"/>
			<xsl:for-each select="child::font">
				<xsl:variable name="previousPosition" select="position()-1"/>
				<xsl:variable name="previous" select="../child::font[position()=$previousPosition]/@*/."/>
				<xsl:variable name="isAnchor">
					<xsl:choose>
						<xsl:when test="$previous=false()"><xsl:value-of select="'1'"/></xsl:when>
						<xsl:when test="$previous=@*/."><xsl:value-of select="'0'"/></xsl:when>
						<xsl:otherwise><xsl:value-of select="'1'"/></xsl:otherwise>
					</xsl:choose>
				</xsl:variable>	
				<xsl:call-template name="font">
					<xsl:with-param name="pRow" select="$pRow"/>
					<xsl:with-param name="pCell" select="position()"/>
					<xsl:with-param name="pIsAnchor" select="$isAnchor"/>					
				</xsl:call-template>
			</xsl:for-each>
		</td>
	</xsl:template>

	<!-- Font element of each TableCell -->	
	<xsl:template name="font">
		<xsl:param name="pRow"/>
		<xsl:param name="pCell"/>
		<xsl:param name="pIsAnchor"/>
		<span>
			<!-- Set Css Style Font equivalent -->	
			<xsl:variable name = "DiffClassName">
				<xsl:call-template name="SetStyleFont"/>
			</xsl:variable>	
			<xsl:if test="$DiffClassName=true()">
				<xsl:attribute name="class"><xsl:value-of select="$DiffClassName"/></xsl:attribute>	
			</xsl:if>
			
			<xsl:choose>
				<xsl:when test="$DiffClassName!='XmlDiff_None'">
					<xsl:variable name="DiffType" select="substring-after($DiffClassName,'XmlDiff')"/>
					<a id="_anchor_{$pRow}_{$pCell}" name="{$DiffType}"><xsl:value-of select="."/></a>
					<!--<xsl:if test="$pIsAnchor='1'">
						<xsl:variable name="DiffType" select="substring-after($DiffClassName,'XmlDiff')"/>
						<a id="_anchor_{$pRow}_{$pCell}" name="{$DiffType}"><xsl:value-of select="."/></a>
					</xsl:if>
					<xsl:if test="$pIsAnchor='0'">
						<a id="_anchor_{$pRow}_{$pCell}"><xsl:value-of select="."/></a>
					</xsl:if>-->
				</xsl:when>
				<xsl:otherwise><xsl:value-of select="."/></xsl:otherwise>
			</xsl:choose>
		</span>
	</xsl:template>


	<!-- Set Attribute to element -->
	<xsl:template name="SetStyle">
		<xsl:for-each select="@*">
			<xsl:attribute name="{name()}"><xsl:value-of select="."/></xsl:attribute>		
		</xsl:for-each>
	</xsl:template>

	<!-- Set Css Style Font equivalent -->	
	<xsl:template name="SetStyleFont">
		<!-- Get BackColor font -->
		<xsl:variable name="varBackColor">
			<xsl:if test="@style=true()">
				<xsl:call-template name="Replace">
					<xsl:with-param name="source"   select="@style"/>
					<xsl:with-param name="oldValue" select="'background-color: '"/>
					<xsl:with-param name="newValue" select="''"/>
				</xsl:call-template>
			</xsl:if>
		</xsl:variable>
		<!--Get ForeColor font -->		
		<xsl:variable name="varForeColor">
			<xsl:if test="@color=true()"><xsl:value-of select="@color"/></xsl:if>
		</xsl:variable>

		<!-- Transform Difference Cell Color with OTCml Css style -->
		<xsl:variable name="DiffClassName">
			<xsl:value-of select="'XmlDiff_'"/>
			<xsl:call-template name="GetDiffClass">
				<xsl:with-param name="BackColor"   select="$varBackColor"/>
				<xsl:with-param name="ForeColor" select="$varForeColor"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:value-of select="$DiffClassName"/>
	</xsl:template>
	
	<!-- ********************** -->
	<!-- TranslateFontAttribute -->
	<!-- ********************** -->
	<xsl:template name="GetDiffClass">
		<xsl:param name="BackColor"/>
		<xsl:param name="ForeColor"/>
		<xsl:choose>
			<xsl:when test="$BackColor='yellow'     and $ForeColor='black'"><xsl:value-of   select="'Added'"/></xsl:when>
			<xsl:when test="$BackColor='red'        and $ForeColor='black'"><xsl:value-of   select="'Removed'"/></xsl:when>
			<xsl:when test="$BackColor='lightgreen' and $ForeColor='black'"><xsl:value-of   select="'Changed'"/></xsl:when>
			<xsl:when test="$BackColor='red'        and $ForeColor='blue'"><xsl:value-of    select="'MovedFrom'"/></xsl:when>
			<xsl:when test="$BackColor='yellow'     and $ForeColor='blue'"><xsl:value-of    select="'MovedTo'"/></xsl:when>
			<xsl:when test="$BackColor='white'      and $ForeColor='#AAAAAA'"><xsl:value-of select="'Ignored'"/></xsl:when>
			<xsl:otherwise><xsl:value-of select="'None'"/></xsl:otherwise>
		</xsl:choose>
	</xsl:template>


</xsl:stylesheet>
