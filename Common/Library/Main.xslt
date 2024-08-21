<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:output method="xml" version="1.0" indent="no"/>
	<xsl:strip-space elements="*"/>
	<xsl:preserve-space elements="xsl:text"/>
	
	<xsl:template match="/|*|@*|processing-instruction()">
		<xsl:copy>
			<xsl:apply-templates select="@*|node()[not(self::comment())]"/>
		</xsl:copy>
	</xsl:template>
</xsl:stylesheet>

  