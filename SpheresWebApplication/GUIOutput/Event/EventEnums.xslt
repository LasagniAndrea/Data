<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:output method="xml" indent="yes" encoding="UTF-8" omit-xml-declaration="yes"></xsl:output>

	<xsl:template match="/">
		<div style="display:grid;">
			<table id="tblCodeEvent">
				<tr>
					<xsl:apply-templates select="ExtendEnums"/>
				</tr>
			</table>
		</div>
	</xsl:template>

  <xsl:template match="ExtendEnums">
    <xsl:apply-templates select="ExtendEnum"/>
  </xsl:template>

  <xsl:template match="ExtendEnum">
    <xsl:variable name="code" select="./@CODE"/>
    <xsl:variable name="currentExtendEnum"   select="position()"/>
    <td class="event-extend-enums-container">
      <div class="input">
        <div class="headh">
          <span class="size4"><xsl:value-of select="$code"/></span>
          <br/>
          <span class="size5" style="font-weight:lighter;"><xsl:value-of select="./DEFINITION"/></span>
        </div>
        <div class="contenth tableHolder" style="min-height:600px;height:800px;">
					<table cellpadding="5" cellspacing="0" rules="none" style="vertical-align:top;width:100%;border-collapse:collapse;">
            <tr>
              <td xcolspan="4">
                <table id="{$code}_TBL" border="1" cellpadding="5" cellspacing="0" rules="none" style="border-collapse:collapse;width:100%;">
                  <xsl:apply-templates select="ExtendEnumValue"/>
                </table>
              </td>
            </tr>
          </table>
        </div>
      </div>
    </td>
  </xsl:template>

  <xsl:template match="ExtendEnumValue">
		<xsl:variable name="current"   select="position()"/>
		<xsl:variable name="precedingNode">
			<xsl:choose>
				<xsl:when test = "$current!=1">
					<xsl:value-of select="preceding-sibling::*[position()=1]/@VALUE"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="./@VALUE"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="parentId">
			<xsl:value-of select="..//@CODE"/>
		</xsl:variable>
		<xsl:variable name="letterId">
			<xsl:value-of select="substring(./@VALUE,1,1)"/>
		</xsl:variable>

		<xsl:if test ="(substring($precedingNode,1,1) != $letterId) or $current='1'">
			<tr id="{$letterId}_{$parentId}_MAIN" style="border: 1pt solid lightgray;background-color:darkgray">
				<td>
          <div id="{$letterId}_{$parentId}_DIV" class="fa-icon fa fa-minus-square" onclick="javascript:ClickOnLetter(this);return false;" />
				</td>
				<td colspan="3" style="font-size:10pt;font-weight:bold;xwidth:100%;">
					<xsl:value-of select="$letterId"/>
				</td>
			</tr>
		</xsl:if>

		<tr id="{$letterId}_{$parentId}_{$current}" style="border-left: 1pt solid lightgray;border-right: 1pt solid lightgray;">
			<td style="width:16px;">&#xa0;</td>
			<td style="width:50px;">
				<xsl:value-of select="./@VALUE"/>
			</td>
			<td style="width:300px;">
				<xsl:value-of select="./DOCUMENTATION"/>
			</td>
			<td style="width:100%;">&#xa0;</td>
		</tr>
	</xsl:template>

</xsl:stylesheet>