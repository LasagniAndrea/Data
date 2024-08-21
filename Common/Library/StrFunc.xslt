<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	
	<!--
	<xsl:output method="html" encoding="UTF-8"></xsl:output>
	-->

	<xsl:variable name="lcletters">abcdefghijklmnopqrstuvwxyz</xsl:variable>
	<xsl:variable name="ucletters">ABCDEFGHIJKLMNOPQRSTUVWXYZ</xsl:variable>

  <!-- ************* -->
  <!-- Ucase         -->
  <!-- ************* -->
  <xsl:template name="FirstUCase">
    <xsl:param name="source"/>
    <xsl:value-of select="concat(translate(substring($source,1,1),$lcletters,$ucletters), substring($source,2))"/>
  </xsl:template>

  <!-- ************* -->
	<!-- StringReplace -->
	<!-- ************* -->
	<xsl:template name="Replace">
		<xsl:param name="source"/>
		<xsl:param name="oldValue"/>
		<xsl:param name="newValue"/>
		<xsl:choose>
			<xsl:when test="contains($source,$oldValue)">
				<xsl:value-of select="concat(substring-before($source,$oldValue),$newValue)"/>
				<xsl:call-template name="Replace">
					<xsl:with-param name="source"   select="substring-after($source,$oldValue)"/>
					<xsl:with-param name="oldValue" select="$oldValue"/>
					<xsl:with-param name="newValue" select="$newValue"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$source"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- ************* --> 
	<!-- Upper         -->
	<!-- ************* -->
	<xsl:template name="Upper">
		<xsl:param name="source"/>
		<xsl:value-of select="translate($source,$lcletters,$ucletters)"/>
	</xsl:template>

	<!-- ************* -->
	<!-- Lower         -->
	<!-- ************* -->
	<xsl:template name="Lower">
		<xsl:param name="source"/>
		<xsl:value-of select="translate($source,$ucletters,$lcletters)"/>
	</xsl:template>

	<!-- ******************** -->
	<!-- format-currency-pair -->
	<!-- ******************** -->
	<xsl:template name="format-currency-pair">
		<xsl:param name="quotedCurrencyPair"/>
		<xsl:param name="tradeName"/>
		
		<xsl:if test="$quotedCurrencyPair/quoteBasis = 'Currency2PerCurrency1'">
			<xsl:value-of select="$quotedCurrencyPair/currency2" />/<xsl:value-of select="$quotedCurrencyPair/currency1" />
		</xsl:if>
		<xsl:if test="$quotedCurrencyPair/quoteBasis = 'Currency1PerCurrency2'">
			<xsl:value-of select="$quotedCurrencyPair/currency1" />/<xsl:value-of select="$quotedCurrencyPair/currency2" />
		</xsl:if>
		
		<xsl:if test="$quotedCurrencyPair = 'PutCurrencyPerCallCurrency'">
			<xsl:value-of select="$tradeName/callCurrencyAmount/currency" />/<xsl:value-of select="$tradeName/putCurrencyAmount/currency" />
		</xsl:if>
		<xsl:if test="$quotedCurrencyPair = 'CallCurrencyPerPutCurrency'">
			<xsl:value-of select="$tradeName/putCurrencyAmount/currency" />/<xsl:value-of select="$tradeName/callCurrencyAmount/currency" />
		</xsl:if>					
	
	</xsl:template>

	<!-- ****************** -->
	<!-- ReplacePlusMinus   -->
	<!-- ****************** -->
	<xsl:template name="ReplacePlusMinus">
		<xsl:param name="pString" />
			<xsl:choose>
				<xsl:when test="starts-with($pString, '-')">
					<xsl:value-of select="concat('Minus ', substring-after($pString, '-'))"/>
				</xsl:when>
				<xsl:when test="starts-with($pString, '+')">
					<xsl:value-of select="concat('Plus ', substring-after($pString, '+'))"/>
				</xsl:when>
				<xsl:when test="$pString">
					<xsl:value-of select="concat('Plus ', $pString)"/>
				</xsl:when>
				<xsl:otherwise>
					None
				</xsl:otherwise>
			</xsl:choose> 
	</xsl:template>

  <!-- EG 20160404 Migration vs2013 -->
  <xsl:template name="DisplayBorderAttributes">
    <xsl:param name="pLeftStyle" />
    <xsl:param name="pLeftWidth" />
    <xsl:param name="pRightStyle" />
    <xsl:param name="pRightWidth" />
    <xsl:param name="pTopStyle" />
    <xsl:param name="pTopWidth" />
    <xsl:param name="pBottomStyle" />
    <xsl:param name="pBottomWidth" />

    <xsl:call-template name="DisplayBorderAttribute">
      <xsl:with-param name="pStyleName" select="'border-left-style'"/>
      <xsl:with-param name="pStyleValue" select="$pLeftStyle"/>
      <xsl:with-param name="pWidthName" select="'border-left-width'"/>
      <xsl:with-param name="pWidthValue" select="$pLeftWidth"/>
    </xsl:call-template>
    <xsl:call-template name="DisplayBorderAttribute">
      <xsl:with-param name="pStyleName" select="'border-right-style'"/>
      <xsl:with-param name="pStyleValue" select="$pRightStyle"/>
      <xsl:with-param name="pWidthName" select="'border-right-width'"/>
      <xsl:with-param name="pWidthValue" select="$pRightWidth"/>
    </xsl:call-template>
    <xsl:call-template name="DisplayBorderAttribute">
      <xsl:with-param name="pStyleName" select="'border-top-style'"/>
      <xsl:with-param name="pStyleValue" select="$pTopStyle"/>
      <xsl:with-param name="pWidthName" select="'border-top-width'"/>
      <xsl:with-param name="pWidthValue" select="$pTopWidth"/>
    </xsl:call-template>
    <xsl:call-template name="DisplayBorderAttribute">
      <xsl:with-param name="pStyleName" select="'border-bottom-style'"/>
      <xsl:with-param name="pStyleValue" select="$pBottomStyle"/>
      <xsl:with-param name="pWidthName" select="'border-bottom-width'"/>
      <xsl:with-param name="pWidthValue" select="$pBottomWidth"/>
    </xsl:call-template>

  </xsl:template>

  <xsl:template name="DisplayBorderAttribute">
    <xsl:param name="pStyleName" />
    <xsl:param name="pStyleValue" />
    <xsl:param name="pWidthName" />
    <xsl:param name="pWidthValue" />

    <xsl:call-template name="DisplayAttribute">
      <xsl:with-param name="pName" select="$pStyleName"/>
      <xsl:with-param name="pValue" select="$pStyleValue"/>
    </xsl:call-template>
    <xsl:call-template name="DisplayAttribute">
      <xsl:with-param name="pName" select="$pWidthName"/>
      <xsl:with-param name="pValue" select="$pWidthValue"/>
    </xsl:call-template>
  </xsl:template>

  <!-- EG 20160404 Migration vs2013 -->
  <xsl:template name="DisplayAttribute">
    <xsl:param name="pName" />
    <xsl:param name="pValue" />
    <xsl:if test="string-length($pName) > 0 and string-length($pValue) > 0">
      <xsl:attribute name="{$pName}">
        <xsl:choose>
          <xsl:when test="(substring($pName, string-length($pName) - string-length('width') + 1) = 'width')">
            <xsl:call-template name="GetBorderWidthValue">
              <xsl:with-param name="pValue" select="$pValue"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$pValue"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
    </xsl:if>
  </xsl:template>

  <!-- EG 20160404 Migration vs2013 -->
  <xsl:template name="GetBorderWidthValue">
    <xsl:param name="pValue" select="0"/>
    <xsl:choose>
      <xsl:when test ="($pValue='thin') or ($pValue='medium') or ($pValue='thick')">
        <xsl:value-of select="$pValue"/>
      </xsl:when>
      <xsl:when test="(substring($pValue, string-length($pValue) - string-length('pt') + 1) = 'pt')">
        <xsl:value-of select="$pValue"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select ="concat($pValue,'pt')"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

 
</xsl:stylesheet>

  