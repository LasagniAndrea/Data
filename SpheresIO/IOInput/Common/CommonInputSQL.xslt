<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
    <xsl:output method="xml" indent="yes"/>

<!--
/*
==============================================================
 Summary : Common templates to build Spheres XSL/SQL request                                          
==============================================================
 Version : v1.0.1.0  
	- Template TCPutCall gère les valeurs Fix (0, 1)
	- Template TCAcctTyp gère les valeurs Fix 1, 2 et 8
	- Template TCInstrumentIdentifier gère les valeurs Fix pour PutCall
 Date    : 20110829 
 Author  : GP 
 
 Version : v1.0.0.0  - Including 'Build SQl' 'Build XML' 'Transcode' templates                                        
 Date    : 20100220                                           
 Author  : MF                                                 
==============================================================
*/
-->
<!-- FI 20200901 [25468] use of GetUTCDateTimeSys -->
 

  <!--
  *******************************
  HELPER BUILD SQL TEMPLATES
  *******************************
  -->

  <!-- SQL Null constant declaration -->
  <xsl:variable name="NULL">null</xsl:variable>

  <!-- Not Found constant -->
  <xsl:variable name="cNOTFOUND">
    <xsl:value-of select="'!NotFound'"/>
  </xsl:variable>

  <!-- Not unique constant -->
  <xsl:variable name="cNOTUNIQUE">
    <xsl:value-of select="'!NotUnique'"/>
  </xsl:variable>
  
  <!-- Common template to build the SQL insertion  instructions for the fields DTINS and IDAINS -->
  <xsl:template name="SysIns">
    <xsl:param name="pIsWithControl" select="true()"/>
    <xsl:param name="pNoIdaIns" select="false()"/>

    <xsl:if test="$pIsWithControl = true()">

      <column name="DTINS" datakey="false" datakeyupd="false" datatype="datetime">
        <SpheresLib function="GetUTCDateTimeSys()"/>
          <controls>
            <control action="RejectColumn" return="true" logtype="None">
              <SpheresLib function="IsUpdate()"/>
            </control>
          </controls>
      </column>

      <xsl:if test="$pNoIdaIns = false()">

        <column name="IDAINS" datakey="false" datakeyupd="false" datatype="int">
          <SpheresLib function="GetUserID()"/>
            <controls>
              <control action="RejectColumn" return="true" logtype="None">
                <SpheresLib function="IsUpdate()"/>
              </control>
            </controls>
        </column>

      </xsl:if>

    </xsl:if>

  </xsl:template>

  <!--
  *******************************
  HELPER BUILD SQL/XML TEMPLATES
  *******************************
  -->

  <!-- 
  Build the SQL parameters list.
  <Param datatype="string" name="pNames[0]">pValues[0]</Param>
  ...
  <Param datatype="string" name="pNames[n]">pValues[n]</Param>
  -->
  <xsl:template name="SQLParamNodesBuilder">
    <!-- list of parameters values 'NAME1,NAME2, ... ,NAMEn' -->
    <xsl:param name="pValues"/>
    <!-- list of parameters names 'KEY1,KEY2, ... ,KEYn' -->
    <xsl:param name="pNames"/>
    <!-- If pKeyName is not empty then the template will try to return the unique record pValues[<pKeyName>]-->
    <xsl:param name="pKeyName"/>

    <xsl:choose>
      <xsl:when test="string-length($pNames) > 0">

        <xsl:variable name="vActualName">
          <xsl:choose>
            <xsl:when test="contains($pNames, ',')">
              <xsl:value-of select="substring-before($pNames, ',')"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$pNames"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="vActualValue">
          <xsl:choose>
            <xsl:when test="contains($pValues, ',')">
              <xsl:value-of select="substring-before($pValues, ',')"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$pValues"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="vFollowingValues">
          <xsl:value-of select="substring-after($pValues, ',')"/>
        </xsl:variable>

        <xsl:variable name="vFollowingNames">
          <xsl:value-of select="substring-after($pNames, ',')"/>
        </xsl:variable>

        <xsl:call-template name="SQLParamNodesBuilder">
          <xsl:with-param name="pValues" select="$vFollowingValues"/>
          <xsl:with-param name="pNames" select="$vFollowingNames"/>
          <xsl:with-param name="pKeyName" select="$pKeyName"/>
        </xsl:call-template>

        <xsl:choose>
          <!-- RD 20100506 Bug Oracle qui n'accèpte pas des paramétres vides -->
          <!-- PL 20101208 -->
          <!-- xsl:when test="$pKeyName = '' and string-length($vActualName) > 0 and string-length($vActualValue) > 0" -->
          <xsl:when test="$pKeyName = '' and string-length($vActualName) > 0">
            <Param datatype="string">
              <xsl:attribute name="name">
                <xsl:value-of select="$vActualName"/>
              </xsl:attribute>
              <xsl:value-of select="$vActualValue"/>
            </Param>
          </xsl:when>

          <xsl:when test="$pKeyName != '' and $vActualName = $pKeyName">
            <xsl:value-of select="$vActualValue"/>
          </xsl:when>
        </xsl:choose>

      </xsl:when>
      <xsl:otherwise/>
    </xsl:choose>

  </xsl:template>

  <!--
  *******************************
  HELPER TRANSCODE TEMPLATES
  *******************************
  -->
  
  <!-- Get the Sphere® instrument identifier -->
  <xsl:template name="TCInstrumentIdentifier">
    <!-- External values [P*,C*,0*, 1*, *] -->
    <xsl:param name="pPutCall"/>
    <xsl:variable name="vPC" select="substring($pPutCall, 1, 1)"/>
    <xsl:choose>
      <!-- GP20110829: valeurs Fix ajoutées (0/1) -->
		  <xsl:when test="$vPC='P' or $vPC='C' or $vPC = '0' or $vPC = '1'">
        <xsl:value-of select="'ExchangeTradedOption'"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'ExchangeTradedFuture'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

	<!-- Get the FIXml® PutCall value (FIX Tag 201) -->
	<xsl:template name="TCPutCall">
		<!-- External values [P*,C*,*] -->
		<xsl:param name="pPutCall"/>
		<xsl:variable name="vPC" select="substring($pPutCall, 1, 1)"/>
		<xsl:choose>
			<xsl:when test="$vPC='P'">
				<xsl:value-of select="'0'"/>
			</xsl:when>
			<xsl:when test="$vPC='C'">
				<xsl:value-of select="'1'"/>
			</xsl:when>
			<!-- GP20110829: if the external value is Fix, no transcodification to do -->
			<xsl:when test="$vPC='0' or $vPC='1'">
				<xsl:value-of select="$vPC"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$NULL"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

  <!-- Get the normalised StrikePrice value, if NaN -> empty else pStrkPx -->
  <xsl:template name="TCStrkPx">
    <xsl:param name="pStrkPx"/>
    <xsl:choose>
      <xsl:when test="pStrkPx = 'NaN'">
        <!--PL 20100908 xsl:value-of select="'0'"/ -->
        <xsl:value-of select="' '"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pStrkPx"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 
  Get the transcoded contract date starting from the fix date code of a future contract 
  -->
  <xsl:template name="TCMMY">
    <!--
    1. if the date length is 2 (eg Z0) then 
        date[0] -> month code
          * January = F
          * February = G
          * March = H
          * April = J
          * May = K
          * June = M
          * July = N
          * August = Q
          * September = U
          * October = V
          * November = X
          * December = Z
       date[1] -> last digit of the year
    2. date length is 6 (eg 201012)  
    -->
    <xsl:param name="pMMY"/>
    <!-- A date formatted as YYYY-MM-DD  -->
    <xsl:param name="pBizDt"/>
    <xsl:choose>
      <xsl:when test="string-length($pMMY) = 2">
        <xsl:variable name="MonthCode" select="substring($pMMY, 1, 1)"/>
        <xsl:variable name="LastYearDigit" select="substring($pMMY, 2)"/>
        <!-- UNDONE - XSLT 1.0 do not have function to get the current date -->
        <xsl:variable name="CurrentDecade" select="substring(string($pBizDt), 1, 3)"/>
        <xsl:variable name="TCMonthCode">
          <xsl:choose>
            <xsl:when test="$MonthCode = 'F'">
              <xsl:value-of select="01"/>
            </xsl:when>
            <xsl:when test="$MonthCode = 'G'">
              <xsl:value-of select="02"/>
            </xsl:when>
            <xsl:when test="$MonthCode = 'H'">
              <xsl:value-of select="03"/>
            </xsl:when>
            <xsl:when test="$MonthCode = 'J'">
              <xsl:value-of select="04"/>
            </xsl:when>
            <xsl:when test="$MonthCode = 'K'">
              <xsl:value-of select="05"/>
            </xsl:when>
            <xsl:when test="$MonthCode = 'M'">
              <xsl:value-of select="06"/>
            </xsl:when>
            <xsl:when test="$MonthCode = 'N'">
              <xsl:value-of select="07"/>
            </xsl:when>
            <xsl:when test="$MonthCode = 'Q'">
              <xsl:value-of select="08"/>
            </xsl:when>
            <xsl:when test="$MonthCode = 'U'">
              <xsl:value-of select="09"/>
            </xsl:when>
            <xsl:when test="$MonthCode = 'V'">
              <xsl:value-of select="10"/>
            </xsl:when>
            <xsl:when test="$MonthCode = 'X'">
              <xsl:value-of select="11"/>
            </xsl:when>
            <xsl:when test="$MonthCode = 'Z'">
              <xsl:value-of select="12"/>
            </xsl:when>
          </xsl:choose>
        </xsl:variable>
        <!-- return the transcoded date -->
        <xsl:value-of select="concat($CurrentDecade,$LastYearDigit,$TCMonthCode)"/>
      </xsl:when>
      <xsl:when test="string-length($pMMY) = 6">
        <!-- return the date as it is (Monthly maturity) -->
        <xsl:value-of select="$pMMY"/>
      </xsl:when>
      <xsl:when test="string-length($pMMY) = 8">
        <!-- return the date as it is (Weekly or Daily maturity) -->
        <xsl:value-of select="$pMMY"/>
      </xsl:when>
      <!-- PL 20111124 TODO -->
      <xsl:when test="string-length($pMMY) = 10">
        <xsl:value-of select="concat(substring($pMMY, 1, 4), substring($pMMY, 6, 2))"/>
      </xsl:when>
      <xsl:otherwise>
        <!-- return Incorrect format -->
        <xsl:value-of select="concat($pMMY,'ERRFORMAT')"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Get the FIXml® Side value (FIX Tag 54) -->
  <xsl:template name="TCSide">
    <xsl:param name="pSide"/>
    <xsl:variable name="vS" select="substring($pSide, 1, 1)"/>
    <xsl:choose>
      <xsl:when test="$vS='B'">
        <xsl:value-of select="'1'"/>
      </xsl:when>
      <xsl:when test="$vS='A'">
        <xsl:value-of select="'1'"/>
      </xsl:when>
      <xsl:when test="$vS='S'">
        <xsl:value-of select="'2'"/>
      </xsl:when>
      <xsl:when test="$vS='V'">
        <xsl:value-of select="'2'"/>
      </xsl:when>
      <xsl:otherwise>
        <!-- Already translated (1 or 2) or undefined value -->
        <xsl:value-of select="$pSide"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

	<!-- Get the FIXml® AcctTyp value (FIX Tag 581) -->
	<xsl:template name="TCAcctTyp">
		<xsl:param name="pAcctTyp"/>
		<xsl:variable name="vT" select="substring($pAcctTyp, 1, 1)"/>
		<xsl:choose>
			<xsl:when test="$vT = 'C'">
				<!-- Account is carried on customer side of the books ([C]lient)-->
				<xsl:value-of select="'1'"/>
			</xsl:when>
			<xsl:when test="$vT = 'M'">
				<!-- Account is carried on non-customer side of books ([M]aison)-->
				<xsl:value-of select="'2'"/>
			</xsl:when>
			<xsl:when test="$vT = 'H'">
				<!-- Account is carried on non-customer side of books ([H]ouse)-->
				<xsl:value-of select="'2'"/>
			</xsl:when>
			<xsl:when test="$vT = 'F'">
				<!-- Account is carried on non-customer side of books ([F]irm)-->
				<xsl:value-of select="'2'"/>
			</xsl:when>
			<!-- GP20110829: If the external value is FIX we do nothing -->
			<xsl:when test="$vT = '1' or $vT = '2' or $vT = '8'">
				<xsl:value-of select="$vT"/>
			</xsl:when>
			<xsl:otherwise>
				<!-- Joint back office account (JBO) -->
				<xsl:value-of select="'8'"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
  
</xsl:stylesheet>
