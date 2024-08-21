<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
xmlns:dt="http://xsltsl.org/date-time">

  <!-- ************************************************ -->
  <!-- DateTime to String DD/MM/YYYY (Not use culture)  -->
  <!-- ************************************************ -->
  <xsl:template name="format-shortdate">
    <xsl:param name="xsd-date-time"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <xsl:call-template name="dt:format-date-time">
      <xsl:with-param name="xsd-date-time" select="$xsd-date-time"/>
      <xsl:with-param name="format" select="'%d/%m/%Y'"/>
      <xsl:with-param name="pCulture" select="$pCulture"/>
    </xsl:call-template>

  </xsl:template>
  <!-- ************************************************ -->
  <!-- Date to String DD/MM/YYYY  (Use culture)         -->
  <!-- ************************************************ -->
  <!--Formate Date for display according to culture-->
  <xsl:template name="format-shortdate2">
    <xsl:param name="xsd-date-time"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <!-- Set the correct date format according to the culture -->
    <xsl:variable name="dateFormat">
      <xsl:choose>
        <xsl:when test = "$pCulture = 'fr-FR'">%d/%m/%Y</xsl:when>
        <xsl:when test = "$pCulture = 'en-GB'">%d/%m/%Y</xsl:when>
        <xsl:when test = "$pCulture = 'en-US'">%m/%d/%Y</xsl:when>
        <xsl:when test = "$pCulture = 'it-IT'">%d/%m/%Y</xsl:when>
        <!-- Par défaut on retient le format 'en-GB'-->
        <xsl:otherwise>%d/%m/%Y</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- Format the date according to the specified date format and culture -->
    <xsl:call-template name="dt:format-date-time">
      <xsl:with-param name="xsd-date-time" select="$xsd-date-time"/>
      <xsl:with-param name="format" select="$dateFormat"/>
      <xsl:with-param name="pCulture" select="$pCulture"/>
    </xsl:call-template>

  </xsl:template>
  <!-- ************************************************ -->
  <!-- Date to String DD MMM (Use culture)              -->
  <!-- ************************************************ -->
  <xsl:template name="format-shortdate_ddMMM">
    <xsl:param name="month"/>
    <xsl:param name="day"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <xsl:call-template name="format-shortdate_ddMMMyy">
      <xsl:with-param name="month" select="$month"/>
      <xsl:with-param name="day" select="$day"/>
      <xsl:with-param name="pCulture" select="$pCulture"/>
    </xsl:call-template>
  </xsl:template>
  <!-- ************************************************ -->
  <!-- Date to String DD MMM YY (Use culture)           -->
  <!-- ************************************************ -->
  <xsl:template name="format-shortdate_ddMMMyy">
    <xsl:param name="year"/>
    <xsl:param name="month"/>
    <xsl:param name="day"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <xsl:variable name="vDay">
      <xsl:if test="string-length($day) > 0">
        <xsl:value-of select="number($day)"/>
      </xsl:if>
    </xsl:variable>
    <!-- Abbreviated month name-->
    <xsl:variable name="vMonth">
      <xsl:call-template name="dt:format-date-time">
        <xsl:with-param name="month" select="number($month)"/>
        <xsl:with-param name="format" select="'%b'"/>
        <xsl:with-param name="pCulture" select="$pCulture"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vShortdate_ddMMMyy">
      <xsl:if test="string-length($vDay) > 0">
        <xsl:value-of select="concat($vDay,' ')"/>
      </xsl:if>
      <xsl:value-of select="$vMonth"/>
      <xsl:if test="string-length($year) > 0">
        <xsl:value-of select="concat(' ',substring($year,3,2))"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="vShortdate_MMMddyy">
      <xsl:value-of select="$vMonth"/>
      <xsl:choose>
        <xsl:when test="$vDay and $year">
          <!--Feb 11, 2011-->
          <xsl:value-of select="concat(' ', $vDay,', ', $year)"/>
        </xsl:when>
        <xsl:when test="$vDay">
          <!--Feb 11-->
          <xsl:value-of select="concat(' ', $vDay)"/>
        </xsl:when>
        <xsl:when test="$year">
          <!--Feb 2011-->
          <xsl:value-of select="concat(' ', $year)"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <!-- Set the correct date format according to the culture -->
    <xsl:choose>
      <xsl:when test = "$pCulture = 'fr-FR'">
        <!--%d %mmm %Y-->
        <xsl:value-of select="$vShortdate_ddMMMyy"/>
      </xsl:when>
      <xsl:when test = "$pCulture = 'en-GB'">
        <!--%d %mmm %Y-->
        <xsl:value-of select="$vShortdate_ddMMMyy"/>
      </xsl:when>
      <xsl:when test = "$pCulture = 'en-US'">
        <!--%mmm %d, %Y-->
        <xsl:value-of select="$vShortdate_MMMddyy"/>
      </xsl:when>
      <xsl:when test = "$pCulture = 'it-IT'">
        <!--%d %mmm %Y-->
        <xsl:value-of select="$vShortdate_ddMMMyy"/>
      </xsl:when>
      <!-- Par défaut on retient le format 'en-GB'-->
      <xsl:otherwise>
        <!--%d %mmm %Y-->
        <xsl:value-of select="$vShortdate_ddMMMyy"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="format-shortdate_ddMMMyyyy">
    <xsl:param name="year"/>
    <xsl:param name="month"/>
    <xsl:param name="day"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <xsl:variable name="vMonth">
      <xsl:call-template name="dt:format-date-time">
        <xsl:with-param name="month" select="number($month)"/>
        <xsl:with-param name="format" select="'%b'"/>
        <xsl:with-param name="pCulture" select="$pCulture"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vShortdate_ddMMMyyyy">
      <xsl:if test="$day">
        <xsl:value-of select="concat($day,' ')"/>
      </xsl:if>
      <xsl:value-of select="$vMonth"/>
      <xsl:if test="$year">
        <xsl:value-of select="concat(' ',$year)"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="vShortdate_MMMddyyyy">
      <xsl:value-of select="$vMonth"/>
      <xsl:choose>
        <xsl:when test="$day and $year">
          <!--Feb 11, 2011-->
          <xsl:value-of select="concat(' ', $day,', ', $year)"/>
        </xsl:when>
        <xsl:when test="$day">
          <!--Feb 11-->
          <xsl:value-of select="concat(' ', $day)"/>
        </xsl:when>
        <xsl:when test="$year">
          <!--Feb 2011-->
          <xsl:value-of select="concat(' ', $year)"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <!-- Set the correct date format according to the culture -->
    <xsl:choose>
      <xsl:when test = "$pCulture = 'fr-FR'">
        <!--%d %mmm %Y-->
        <xsl:value-of select="$vShortdate_ddMMMyyyy"/>
      </xsl:when>
      <xsl:when test = "$pCulture = 'en-GB'">
        <!--%d %mmm %Y-->
        <xsl:value-of select="$vShortdate_ddMMMyyyy"/>
      </xsl:when>
      <xsl:when test = "$pCulture = 'en-US'">
        <!--%mmm %d, %Y-->
        <xsl:value-of select="$vShortdate_MMMddyyyy"/>
      </xsl:when>
      <xsl:when test = "$pCulture = 'it-IT'">
        <!--%d %mmm %Y-->
        <xsl:value-of select="$vShortdate_ddMMMyyyy"/>
      </xsl:when>
      <!-- Par défaut on retient le format 'en-GB'-->
      <xsl:otherwise>
        <!--%d %mmm %Y-->
        <xsl:value-of select="$vShortdate_ddMMMyyyy"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- ************************************************ -->
  <!-- Date to String DD MMM YYYY (Use culture)         -->
  <!-- ************************************************ -->
  <xsl:template name="format-date">
    <xsl:param name="xsd-date-time"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <!-- Set the correct date format according to the culture -->
    <xsl:variable name="dateFormat">
      <xsl:choose>
        <xsl:when test = "$pCulture = 'fr-FR'">
          %e %B %Y
        </xsl:when>
        <xsl:when test = "$pCulture = 'en-GB'">
          %e %B %Y
        </xsl:when>
        <xsl:when test = "$pCulture = 'en-US'">
          %B %e, %Y
        </xsl:when>
        <xsl:when test = "$pCulture = 'it-IT'">
          %e %B %Y
        </xsl:when>
        <!-- Par défaut on retient le format 'en-GB'-->
        <xsl:otherwise>
          %e %B %Y
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- Format the date according to the specified date format and culture -->
    <xsl:call-template name="dt:format-date-time">
      <xsl:with-param name="xsd-date-time" select="$xsd-date-time"/>
      <xsl:with-param name="format" select="$dateFormat"/>
      <xsl:with-param name="pCulture" select="$pCulture"/>
    </xsl:call-template>

  </xsl:template>
  <!-- ************************************************ -->
  <!-- Time to String hh:mm:ss (Not use culture)        -->
  <!-- ************************************************ -->
  <xsl:template name="format-time">
    <xsl:param name="xsd-date-time"/>

    <xsl:variable name="format">
      %H
      <xsl:value-of select="$separatorTime" />
      %M
      <xsl:value-of select="$separatorTime" />
      %S
    </xsl:variable>
    <xsl:value-of select="$xsd-date-time" />
  </xsl:template>
  <!-- ************************************************ -->
  <!-- Time to String hh:mm:ss (Use culture)            -->
  <!-- ************************************************ -->
  <xsl:template name="format-time2">
    <xsl:param name="xsd-date-time"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <!-- Set the correct date format according to the culture -->
    <xsl:variable name="format">
      <xsl:choose>
        <xsl:when test = "$pCulture = 'en-US'">
          <!--Hour in 12-hour format (01 - 12)-->
          <xsl:value-of select="concat('%I', $separatorTime,'%M', $separatorTime,'%S',' %p')" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="concat('%H', $separatorTime,'%M', $separatorTime,'%S')" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:call-template name="dt:format-date-time">
      <xsl:with-param name="xsd-date-time" select="$xsd-date-time"/>
      <xsl:with-param name="format" select="$format"/>
      <xsl:with-param name="pCulture" select="$pCulture"/>
    </xsl:call-template>
  </xsl:template>
  <!-- ************************************************ -->
  <!-- Time to String hh:mm   (Not use culture)         -->
  <!-- ************************************************ -->
  <xsl:template name="format-shorttime">
    <xsl:param name="xsd-date-time"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <xsl:variable name="format">
      %I
      <xsl:value-of select="$separatorTime" />
      %M %p
    </xsl:variable>

    <xsl:call-template name="dt:format-date-time">
      <xsl:with-param name="xsd-date-time" select="$xsd-date-time"/>
      <xsl:with-param name="format" select="$format"/>
      <xsl:with-param name="pCulture" select="$pCulture"/>
    </xsl:call-template>
  </xsl:template>
  <!-- ************************************************ -->
  <!-- Time to String hh:mm (Use culture)               -->
  <!-- ************************************************ -->
  <xsl:template name="format-shorttime2">
    <xsl:param name="xsd-date-time"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <!-- Set the correct date format according to the culture -->
    <xsl:variable name="format">
      <xsl:choose>
        <xsl:when test = "$pCulture = 'en-US'">
          <!--Hour in 12-hour format (01 - 12)-->
          <xsl:value-of select="concat('%I', $separatorTime,'%M',' %p')" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="concat('%H', $separatorTime,'%M')" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:call-template name="dt:format-date-time">
      <xsl:with-param name="xsd-date-time" select="$xsd-date-time"/>
      <xsl:with-param name="format" select="$format"/>
      <xsl:with-param name="pCulture" select="$pCulture"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ************************************************ -->
  <!-- Time to String hh:mm AM/PM   (Not use culture)   -->
  <!-- ************************************************ -->
  <xsl:template name="format-shorttime3">
    <xsl:param name="xsd-date-time"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <xsl:variable name="format" select="concat('%I', $separatorTime,'%M',' %P')" />
    <xsl:call-template name="dt:format-date-time">
      <xsl:with-param name="xsd-date-time" select="$xsd-date-time"/>
      <xsl:with-param name="format" select="$format"/>
      <xsl:with-param name="pCulture" select="$pCulture"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ********************************************************************* -->
  <!-- Time to String hh:mm:ss with Business Center (Use culture)            -->
  <!-- remarque: si la seconde est 00 on n'affiche pas les secondesà zéro    -->
  <!--           si la culture est en-GB on affiche l'heure en (hh:mm AM/PM) -->
  <!-- ********************************************************************* -->
  <xsl:template name="format-time-bc">
    <xsl:param name="pTime"/>
    <xsl:param name="pBusinessCenter"/>
    <xsl:param name="pCulture" select="$pCurrentCulture"/>

    <!-- Set the correct Business Center format according to the culture -->
    <xsl:choose>

      <xsl:when test = "$pCulture = 'fr-FR'">
        <!-- time 12:15:10 -->
        <xsl:variable name="varTime">
          <xsl:call-template name="format-time">
            <xsl:with-param name="xsd-date-time" select="$pTime"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:choose>
          <xsl:when test="substring($varTime,7,2) = '00'">
            <xsl:value-of select="concat(substring($varTime,1,5),' ')"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="concat($varTime,' ')"/>
          </xsl:otherwise>
        </xsl:choose>

        <!--Heure de Milan-->
        <xsl:value-of select="'Heure de '"/>
        <xsl:call-template name="getFullNameBC">
          <xsl:with-param name="pBC" select="$pBusinessCenter"/>
        </xsl:call-template>
      </xsl:when>

      <xsl:when test = "$pCulture = 'en-GB'">
        <!-- time 12:15 AM/PM -->
        <xsl:call-template name="format-shorttime3">
          <xsl:with-param name="xsd-date-time" select="$pTime"/>
          <xsl:with-param name="pCulture" select="$pCulture"/>
        </xsl:call-template>
        <xsl:value-of select="' '"/>
        <!--Milan Time-->
        <xsl:call-template name="getFullNameBC">
          <xsl:with-param name="pBC" select="$pBusinessCenter"/>
        </xsl:call-template>
        <xsl:value-of select="' Time'"/>
      </xsl:when>

      <xsl:when test = "$pCulture = 'en-US'">
        <!-- time 12:15:10 -->
        <xsl:variable name="varTime">
          <xsl:call-template name="format-time">
            <xsl:with-param name="xsd-date-time" select="$pTime"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:choose>
          <xsl:when test="substring($varTime,7,2) = '00'">
            <xsl:value-of select="concat(substring($varTime,1,5),' ')"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="concat($varTime,' ')"/>
          </xsl:otherwise>
        </xsl:choose>
        <!--Milan Time-->
        <xsl:call-template name="getFullNameBC">
          <xsl:with-param name="pBC" select="$pBusinessCenter"/>
        </xsl:call-template>
        <xsl:value-of select="' Time'"/>
      </xsl:when>

      <xsl:when test = "$pCulture = 'it-IT'">
        <!-- time 12:15:10 -->
        <xsl:variable name="varTime">
          <xsl:call-template name="format-time">
            <xsl:with-param name="xsd-date-time" select="$pTime"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:choose>
          <xsl:when test="substring($varTime,7,2) = '00'">
            <xsl:value-of select="concat(substring($varTime,1,5),' ')"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="concat($varTime,' ')"/>
          </xsl:otherwise>
        </xsl:choose>
        <!--Orario di Milano-->
        <xsl:call-template name="getTranslation">
          <xsl:with-param name="pResourceName" select="'Orario di'"/>
        </xsl:call-template>
        <xsl:value-of select="' '"/>
        <xsl:call-template name="getFullNameBC">
          <xsl:with-param name="pBC" select="$pBusinessCenter"/>
        </xsl:call-template>
      </xsl:when>

      <!-- Par défaut on retient le format 'en-GB'-->
      <xsl:otherwise>
        <!-- time 12:15 AM/PM -->
        <xsl:call-template name="format-shorttime3">
          <xsl:with-param name="xsd-date-time" select="$pTime"/>
          <xsl:with-param name="pCulture" select="$pCulture"/>
        </xsl:call-template>
        <xsl:value-of select="' '"/>
        <!--Milan Time-->
        <xsl:call-template name="getFullNameBC">
          <xsl:with-param name="pBC" select="$pBusinessCenter"/>
        </xsl:call-template>
        <xsl:value-of select="' Time'"/>
      </xsl:otherwise>

    </xsl:choose>
  </xsl:template>

  <!-- ************************************************ -->
  <!-- Fonction CompareDate -->
  <!-- ************************************************ -->
  <xsl:template name="CompareDate">
    <xsl:param name="pDate1"/>
    <xsl:param name="pDate2"/>

    <xsl:variable name="d1" select="number(concat(substring($pDate1, 1, 4),substring($pDate1, 6, 2),substring($pDate1, 9, 2)))"/>
    <xsl:variable name="d2" select="number(concat(substring($pDate2, 1, 4),substring($pDate2, 6, 2),substring($pDate2, 9, 2)))"/>

    <xsl:choose>
      <xsl:when test="$d1>$d2">1</xsl:when>
      <xsl:when test="$d2>$d1">-1</xsl:when>
      <xsl:otherwise>0</xsl:otherwise>
    </xsl:choose>

  </xsl:template>

</xsl:stylesheet>

