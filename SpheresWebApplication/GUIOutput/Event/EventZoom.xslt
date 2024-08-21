<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

  <xsl:param name="pCurrentCulture" select="'en-GB'"/>
	
  <xsl:include href="..\..\Library\Resource.xslt"/>
  <xsl:include href="..\..\Library\DtFunc.xslt"/>
  <xsl:include href="..\..\Library\xsltsl\date-time.xsl"/>

  <!-- UnderlyerValuationDates | ExerciseDates | Reset | GrossTurnOver -->
  <xsl:template match="UnderlyerValuationDates|ExerciseDates|Reset|GrossTurnOver">
    <xsl:param name="pCssClass" select="'SubEventTransparent'"/>
      <tr class="DataGrid_HeaderStyle">
        <td colspan="4"><xsl:value-of select="$varResource[@name='Trade']/value" /></td>
        <td colspan="2"><xsl:value-of select="$varResource[@name='Instrument']/value" /></td>
      </tr>
      <tr class="DataGrid_ItemStyle">
        <td colspan="4"><xsl:value-of select="TRADE_IDENTIFIER"/> [<xsl:value-of select="IDT" />]</td>
        <td colspan="2"><xsl:value-of select="INSTR_IDENTIFIER" /></td>
      </tr>
      <tr class="DataGrid_AlternatingItemStyle">
        <td width="100px" colspan="2"><xsl:value-of select="$varResource[@name='EventCode']/value" /></td>
        <td width="120px"><xsl:value-of select="$varResource[@name='EventDates']/value" /></td>
        <td><xsl:value-of select="$varResource[@name='Valorisation']/value" /></td>
        <td width="100px"><xsl:value-of select="$varResource[@name='StartPeriod']/value" /></td>
        <td width="100px"><xsl:value-of select="$varResource[@name='EndPeriod']/value" /></td>
      </tr>
      <tr class="DataGrid_ItemStyle">
        <td width="50px"><xsl:value-of select="EVENTCODE" /></td>
        <td width="50px"><xsl:value-of select="EVENTTYPE" /></td>
        <td width="120px">
          <table width="100%" cellpadding="0" cellspacing="0" >
            <xsl:apply-templates select="ClassEvent">
              <xsl:with-param name="pCssClass" select="$pCssClass" />
            </xsl:apply-templates>
          </table>
        </td>
        <td align="right">
          <xsl:if test="VALORISATION=true()">
						<xsl:value-of select="VALORISATION" />&#xa0;<xsl:value-of select="UNIT" />
					</xsl:if>
          <xsl:if test="VALORISATION=false()">&#xa0;</xsl:if>
        </td>
        <td width="100px">
          <xsl:call-template name="format-shortdate">
            <xsl:with-param name="xsd-date-time" select="DTSTARTADJ"/>
          </xsl:call-template>
        </td>
        <td width="100px">
          <xsl:call-template name="format-shortdate">
            <xsl:with-param name="xsd-date-time" select="DTENDADJ"/>
          </xsl:call-template>
        </td>
      </tr>
      <tr class="Event_TitleLevel">
        <td colspan="6">&#xa0;</td>
      </tr>
  </xsl:template>

  <!-- ClassExerciseDates | ClassUnderlyer | ClassConstituent | ClassSelfAverage | ClassSelfReset | ClassDetailFee -->
  <xsl:template match="ClassEvent|ClassUnderlyer|ClassConstituent|ClassSelfAverage|ClassSelfReset|ClassDetailFee">
    <xsl:param name="pCssClass" select="'SubEventTransparent'"/>
    <tr class="{$pCssClass}">
      <td><xsl:value-of select="EVENTCLASS" /></td>
      <td>
        <xsl:call-template name="format-shortdate">
          <xsl:with-param name="xsd-date-time" select="DTEVENT"/>
        </xsl:call-template>
      </td>
    </tr>
  </xsl:template>

  <!-- AssetUnderlyer | AssetConstituent | AssetReset | AssetSelfAverage | AssetSelfReset -->
  <xsl:template match="AssetUnderlyer|AssetConstituent|AssetReset|AssetSelfAverage|AssetSelfReset">
    <xsl:param name="pCssClass" select="'SubEventTransparent'"/>
    <xsl:param name="pSize" select="'50px'"/>
    <tr class="{$pCssClass}">
      <td>
        <xsl:variable name="Node">
          <xsl:value-of select="name(current())" />
        </xsl:variable>
        <xsl:if test="$Node='AssetReset' or $Node='AssetSelfAverage' or $Node='AssetSelfReset'">
          <xsl:value-of select="AST_IDENTIFIER" />
        </xsl:if>
        
        <xsl:if test="IDC=true() or CLEARANCESYSTEM=true() or IDM=true() or 
                      IDMARKETENV=true() or IDVALSCENARIO=true() or QUOTESIDE=true() or 
                      QUOTETIMING=true() or IDBC=true() or PRIMARYRATESRC=true()">
          <div style="overflow:auto;height:{$pSize};font-size:smaller;color:DimGray;">
            <xsl:if test="IDMARKETENV=true()">
              <xsl:value-of select="$varResource[@name='IDMARKETENV']/value" />:&#xa0;
              <xsl:value-of select="IDMARKETENV" /><br/>
            </xsl:if>
            <xsl:if test="IDVALSCENARIO=true()">
              <xsl:value-of select="$varResource[@name='IDVALSCENARIO']/value" />:&#xa0;
              <xsl:value-of select="IDVALSCENARIO" /><br/>
            </xsl:if>
            <xsl:if test="QUOTESIDE=true()">
              <xsl:value-of select="$varResource[@name='QUOTESIDE']/value" />:&#xa0;
              <xsl:value-of select="QUOTESIDE" /><br/>
            </xsl:if>
            <xsl:if test="QUOTETIMING=true()">
              <xsl:value-of select="$varResource[@name='QUOTETIMING']/value" />:&#xa0;
              <xsl:value-of select="QUOTETIMING" /><br/>
            </xsl:if>
            <xsl:if test="IDBC=true()">
              <xsl:value-of select="$varResource[@name='IDBC']/value" />:&#xa0;
              <xsl:value-of select="IDBC" /><br/>
            </xsl:if>
            <xsl:if test="PRIMARYRATESRC=true()">
              <xsl:value-of select="$varResource[@name='PRIMARYRATESRC']/value" />:&#xa0;
              <xsl:value-of select="PRIMARYRATESRC" /><br/>
            </xsl:if>
            <xsl:if test="IDC=true()">
              <xsl:value-of select="$varResource[@name='IDC']/value" />:&#xa0;
              <xsl:value-of select="IDC" /><br/>
            </xsl:if>
            <xsl:if test="CLEARANCESYSTEM=true()">
              <xsl:value-of select="$varResource[@name='CLEARANCESYSTEM']/value" />:&#xa0;
              <xsl:value-of select="CLEARANCESYSTEM" /><br/>
            </xsl:if>
            <xsl:if test="IDM=true()">
              <xsl:value-of select="$varResource[@name='IDM']/value" />:&#xa0;<xsl:value-of select="IDM" /><br/>
            </xsl:if>
            <xsl:if test="WEIGHT=true()">
              <xsl:value-of select="$varResource[@name='WEIGHT']/value" />:&#xa0;<xsl:value-of select="WEIGHT" />
              <xsl:choose>
                <xsl:when test = "UNITWEIGHT=true()">
                  &#xa0;<xsl:value-of select="UNITWEIGHT" /><br/>
                </xsl:when>
                <xsl:when test = "UNITTYPEWEIGHT='Percentage'">&#xa0;%</xsl:when>
              </xsl:choose>
            </xsl:if>
          </div>
        </xsl:if>
      </td>
    </tr>
  </xsl:template>

</xsl:stylesheet>
