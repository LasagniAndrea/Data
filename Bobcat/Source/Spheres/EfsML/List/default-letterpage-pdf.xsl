<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:dt="http://xsltsl.org/date-time" xmlns:fo="http://www.w3.org/1999/XSL/Format" version="1.0">

	<xsl:param name="pCurrentCulture" select="'en-GB'"/>

	<!-- xslt includes -->
  <xsl:include href="..\..\Library\Resource.xslt"/>
  <xsl:include href="..\..\Library\DtFunc.xslt"/>
  <xsl:include href="..\..\Library\xsltsl\date-time.xsl"/>

  <xsl:key name="kColumnName" match="Column"  use="@ColumnName"/>
	<xsl:variable name="vAllColumnName" select="/Referential/Row/Column[generate-id()=generate-id(key('kColumnName',@ColumnName))]"/>
	<xsl:variable name="vColumnNameToProcess" select="$vAllColumnName[position()&lt;=5]"/>
	
	<xsl:variable name="varTableBorder">
		1pt solid blue
	</xsl:variable>

	<xsl:template match="/">
		<xsl:apply-templates select="Referential"/>
	</xsl:template>
	<xsl:template match="Referential">
		<fo:root xmlns:fo="http://www.w3.org/1999/XSL/Format" >
			<fo:layout-master-set>				
				<fo:simple-page-master master-name="LetterPage" page-width="8.5in" page-height="11in">					
					<fo:region-body region-name="PageBody" margin="0.7in"/>
				</fo:simple-page-master>
			</fo:layout-master-set>
			
			<fo:page-sequence master-reference="LetterPage">
				<fo:flow flow-name="PageBody">
					<!-- Titre -->
					<fo:block margin-top="1.5cm" font-size="12pt" color="blue">Référentiel: <xsl:value-of select="@TableName" /></fo:block>
					
					<fo:table  border="{$varTableBorder}" font-size="11pt" text-align="left" table-layout="fixed">
						<!-- Déclaration des colonnes-->
						<fo:table-column  column-width="1cm" ></fo:table-column>
						<xsl:for-each select="$vColumnNameToProcess">							
							<fo:table-column></fo:table-column>	
						</xsl:for-each>
						
						<fo:table-body >
							<fo:table-row border="3pt solid red" font-size="14pt" color="blue"  background-color="#D7D6CB" >
								
								<!-- La lignes Entête avec les noms de colonnes-->
								<fo:table-cell><fo:block/></fo:table-cell>
								<xsl:for-each select="$vColumnNameToProcess">									
									<fo:table-cell border="{$varTableBorder}" text-align="left">
										<fo:block>
											<xsl:value-of select="@ColumnName" />
										</fo:block>
									</fo:table-cell>									
								</xsl:for-each>
							</fo:table-row>

							<xsl:for-each select="Row">
								<fo:table-row>
									<!-- Les lignes de données-->
									<fo:table-cell border="{$varTableBorder}" text-align="left">
										<fo:block><xsl:value-of select="position()" /></fo:block>
									</fo:table-cell>
									<xsl:variable name="vRowColumns" select="Column"/>
									<xsl:for-each select="$vColumnNameToProcess">
										<xsl:variable name="vColumnName" select="@ColumnName"/>
										<fo:table-cell border="{$varTableBorder}" text-align="left">
											<fo:block>
												<xsl:variable name="vValue">
													<xsl:value-of select="$vRowColumns[@ColumnName=$vColumnName]" />
												</xsl:variable>
												<xsl:choose >
													<xsl:when test="$vColumnName = 'DTENABLED'">
														<xsl:call-template name="format-shortdate">
															<xsl:with-param name="xsd-date-time" select="$vValue"/>
														</xsl:call-template>
													</xsl:when>
													<xsl:otherwise>
														<xsl:value-of select="$vValue" />
													</xsl:otherwise>
												</xsl:choose>
											</fo:block>
										</fo:table-cell>
									</xsl:for-each>
								</fo:table-row>
							</xsl:for-each>
						</fo:table-body>
					</fo:table>
				</fo:flow>
			</fo:page-sequence>
		</fo:root>
	</xsl:template>
</xsl:stylesheet>