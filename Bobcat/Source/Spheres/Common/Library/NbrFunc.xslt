<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

  <!-- ================================================== -->
  <!--        Number Pattern variables                    -->
  <!-- ================================================== -->
  <xsl:variable name="number2DecPattern">
    <xsl:call-template name="GetNumberPattern">
      <xsl:with-param name="pPatternName" select="'number2DecPattern'"/>
    </xsl:call-template>
  </xsl:variable>
  <xsl:variable name="number3DecPattern">
    <xsl:call-template name="GetNumberPattern">
      <xsl:with-param name="pPatternName" select="'number3DecPattern'"/>
    </xsl:call-template>
  </xsl:variable>
  <xsl:variable name="number4DecPattern">
    <xsl:call-template name="GetNumberPattern">
      <xsl:with-param name="pPatternName" select="'number4DecPattern'"/>
    </xsl:call-template>
  </xsl:variable>
  <xsl:variable name="numberPatternNoZero">
    <xsl:call-template name="GetNumberPattern">
      <xsl:with-param name="pPatternName" select="'numberPatternNoZero'"/>
    </xsl:call-template>
  </xsl:variable>  
  <xsl:variable name="integerPattern">
    <xsl:call-template name="GetNumberPattern">
      <xsl:with-param name="pPatternName" select="'integerPattern'"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:template name="GetNumberPattern">
    <xsl:param name="pPatternName"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <xsl:choose>
      <xsl:when test="$pPatternName='number2DecPattern'">
        <xsl:choose>
          <xsl:when test = "$pCulture = 'de-DE'">
            <xsl:value-of select="'#.###.##0,00'"/>
          </xsl:when>
          <xsl:when test = "$pCulture = 'en-GB'">
            <xsl:value-of select="'#,###,##0.00'"/>
          </xsl:when>
          <xsl:when test = "$pCulture = 'es-ES'">
            <xsl:value-of select="'#.###.##0,00'"/>
          </xsl:when>
          <xsl:when test = "$pCulture = 'fr-BE'">
            <xsl:value-of select="'#.###.##0,00'"/>
          </xsl:when>
          <xsl:when test = "$pCulture = 'fr-FR'">
            <xsl:value-of select="'# ### ##0,00'"/>
          </xsl:when>
          <xsl:when test = "$pCulture = 'it-IT'">
            <xsl:value-of select="'#.###.##0,00'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'#,###,##0.00'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="$pPatternName='number3DecPattern'">
        <xsl:choose>
          <xsl:when test = "$pCulture = 'de-DE'">
            <xsl:value-of select="'#.###.##0,000'"/>
          </xsl:when>
          <xsl:when test = "$pCulture = 'en-GB'">
            <xsl:value-of select="'#,###,##0.000'"/>
          </xsl:when>
          <xsl:when test = "$pCulture = 'es-ES'">
            <xsl:value-of select="'#.###.##0,000'"/>
          </xsl:when>
          <xsl:when test = "$pCulture = 'fr-BE'">
            <xsl:value-of select="'#.###.##0,000'"/>
          </xsl:when>
          <xsl:when test = "$pCulture = 'fr-FR'">
            <xsl:value-of select="'# ### ##0,000'"/>
          </xsl:when>
          <xsl:when test = "$pCulture = 'it-IT'">
            <xsl:value-of select="'#.###.##0,000'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'#,###,##0.000'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="$pPatternName='number4DecPattern'">
        <xsl:choose>
          <xsl:when test = "$pCulture = 'de-DE'">
            <xsl:value-of select="'#.###.##0,0000'"/>
          </xsl:when>
          <xsl:when test = "$pCulture = 'en-GB'">
            <xsl:value-of select="'#,###,##0.0000'"/>
          </xsl:when>
          <xsl:when test = "$pCulture = 'es-ES'">
            <xsl:value-of select="'#.###.##0,0000'"/>
          </xsl:when>
          <xsl:when test = "$pCulture = 'fr-BE'">
            <xsl:value-of select="'#.###.##0,0000'"/>
          </xsl:when>
          <xsl:when test = "$pCulture = 'fr-FR'">
            <xsl:value-of select="'# ### ##0,0000'"/>
          </xsl:when>
          <xsl:when test = "$pCulture = 'it-IT'">
            <xsl:value-of select="'#.###.##0,0000'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'#,###,##0.0000'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="$pPatternName='numberPatternNoZero'">
        <xsl:choose>
          <xsl:when test = "$pCulture = 'de-DE'">
            <xsl:value-of select="'#.###.##0,#########'"/>
          </xsl:when>
          <xsl:when test = "$pCulture = 'en-GB'">
            <xsl:value-of select="'#,###,##0.#########'"/>
          </xsl:when>
          <xsl:when test = "$pCulture = 'es-ES'">
            <xsl:value-of select="'#.###.##0,#########'"/>
          </xsl:when>
          <xsl:when test = "$pCulture = 'fr-BE'">
            <xsl:value-of select="'#.###.##0,#########'"/>
          </xsl:when>
          <xsl:when test = "$pCulture = 'fr-FR'">
            <xsl:value-of select="'# ### ##0,#########'"/>
          </xsl:when>
          <xsl:when test = "$pCulture = 'it-IT'">
            <xsl:value-of select="'#.###.##0,#########'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'#,###,##0.#########'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="$pPatternName='integerPattern'">
        <xsl:choose>
          <xsl:when test = "$pCulture = 'de-DE'">
            <xsl:value-of select="'#.###.##0'"/>
          </xsl:when>
          <xsl:when test = "$pCulture = 'en-GB'">
            <xsl:value-of select="'#,###,##0'"/>
          </xsl:when>
          <xsl:when test = "$pCulture = 'es-ES'">
            <xsl:value-of select="'#.###.##0'"/>
          </xsl:when>
          <xsl:when test = "$pCulture = 'fr-BE'">
            <xsl:value-of select="'#.###.##0'"/>
          </xsl:when>
          <xsl:when test = "$pCulture = 'fr-FR'">
            <xsl:value-of select="'# ### ##0'"/>
          </xsl:when>
          <xsl:when test = "$pCulture = 'it-IT'">
            <xsl:value-of select="'#.###.##0'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="'#,###,##0'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  
  <!-- ************************************************ -->
  <!-- Decimal to String (Use culture)  -->
  <!-- ************************************************ -->
  <xsl:template name="format-number">
    <xsl:param name="pAmount"/>
    <xsl:param name="pAmountPattern"/>
    <xsl:param name="pDefaultCulture" select="$defaultCulture"/>
    
    <xsl:value-of select="format-number($pAmount, $pAmountPattern, $pDefaultCulture)" />
  </xsl:template>

  <!-- ************************************************ -->
  <!-- Integer to String (Use culture)  -->
  <!-- ************************************************ -->
  <xsl:template name="format-integer">
    <xsl:param name="integer"/>
    <xsl:param name="pAmountPattern" select="$integerPattern"/>
    <xsl:param name="pDefaultCulture" select="$defaultCulture"/>
    
    <xsl:value-of select="format-number($integer, $pAmountPattern, $pDefaultCulture)" />    
  </xsl:template>

  <xsl:template name="GetDecPattern">
    <xsl:param name="pRoundDir"/>
    <xsl:param name="pRoundPrec"/>

    <xsl:choose>
      <xsl:when test="number($pRoundPrec) = 0">
        <xsl:value-of select="$integerPattern"/>
      </xsl:when>
      <xsl:when test="number($pRoundPrec) = 3">
        <xsl:value-of select="$number3DecPattern"/>
      </xsl:when>
      <xsl:when test="number($pRoundPrec) = 4">
        <xsl:value-of select="$number4DecPattern"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$number2DecPattern"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!-- ************************************************ -->
  <!-- Round amount according to Precision and Direction -->
  <!-- ************************************************ -->
  <xsl:template name="RoundAmount">
    <xsl:param name="pAmount"/>
    <xsl:param name="pRoundDir"/>
    <xsl:param name="pRoundPrec"/>

    <xsl:variable name="amount">
      <xsl:choose>
        <xsl:when test="number($pAmount) >= 0">
          <xsl:value-of select="number($pAmount)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="number(-1 * $pAmount)"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vPower">
      <xsl:call-template name="Power">
        <xsl:with-param name="base" select="number(10)" />
        <xsl:with-param name="power" select="$pRoundPrec" />
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="amountRounded">
      <xsl:choose>
        <xsl:when test="$pRoundDir='Down'">
          <xsl:value-of select="round(($amount - (1 div (2 * $vPower))) * $vPower) div $vPower"/>
        </xsl:when>
        <xsl:when test="$pRoundDir='Up'">
          <xsl:value-of select="round(($amount + (1 div (2 * $vPower))) * $vPower) div $vPower"/>
        </xsl:when>
        <xsl:otherwise>
          <!--pRoundDir='Nearest'-->
          <xsl:value-of select="round($amount * $vPower) div $vPower"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="number($pAmount) >= 0">
        <xsl:value-of select="number($amountRounded)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="number(-1 * $amountRounded)"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- ************************************************ -->
  <!-- Fonction Power -->
  <!-- ************************************************ -->
  <xsl:template name="Power">
    <xsl:param name="base" select="0" />
    <xsl:param name="power" select="1" />

    <xsl:choose>
      <xsl:when test="$power &lt; 0 or contains(string($power), '.')">
        <xsl:message terminate="yes">
          The XSLT template Power doesn't support negative or fractional arguments.
        </xsl:message>
        <xsl:text>NaN</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <!--RD 20161206 [22665] Test $power not null -->
          <!--<xsl:when test="$power = 0">-->
          <xsl:when test="$power = false() or string-length($power) = 0 or $power = 0">
            <xsl:value-of select="number(1)" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:variable name="vPower1">
              <xsl:call-template name="Power">
                <xsl:with-param name="base" select="$base" />
                <xsl:with-param name="power" select="$power - 1" />
              </xsl:call-template>
            </xsl:variable>
            <xsl:value-of select="number($vPower1) * number($base)"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- ************************************************ -->
  <!-- Fonction AbsAmount -->
  <!-- ************************************************ -->
  <xsl:template name="AbsAmount">
    <xsl:param name="pAmount" />

    <xsl:if test="string-length($pAmount) > 0">
      <xsl:choose>
        <xsl:when test="number($pAmount) >= 0">
          <xsl:value-of select="number($pAmount)" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="number(-1) * number($pAmount)" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>

  </xsl:template>

  <!-- ************************************************ -->
  <!-- Fonction AbsAmount -->
  <!-- ************************************************ -->
  <xsl:template name="Maximum">
    <xsl:param name="pNbr1" />
    <xsl:param name="pNbr2" />

    <xsl:choose>
      <xsl:when test="string-length($pNbr1) > 0 and string-length($pNbr2) > 0">
        <xsl:choose>
          <xsl:when test="number($pNbr1) >= number($pNbr2)">
            <xsl:value-of select="number($pNbr1)" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="number($pNbr2)" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="string-length($pNbr1) > 0">
        <xsl:value-of select="number($pNbr1)" />
      </xsl:when>
      <xsl:when test="string-length($pNbr2) > 0">
        <xsl:value-of select="number($pNbr2)" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="number(0)" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>

