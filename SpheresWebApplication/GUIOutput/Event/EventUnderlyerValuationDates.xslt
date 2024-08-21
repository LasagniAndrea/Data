<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  
  <xsl:output method="html" indent="yes" encoding="UTF-8"/>

  <xsl:include href="EventZoom.xslt"/>

  <!-- Root -->
<xsl:template match="/">
	<xsl:apply-templates select="Events"/>
	<p>
		<a href="http://validator.w3.org/check?uri=referer">
			<img src="http://www.w3.org/Icons/valid-xhtml10-blue" alt="Valid XHTML 1.0 Transitional" height="31" width="88" />
		</a>
</p>
  </xsl:template>

  <!-- Events -->
  <xsl:template match="Events">
	  <table class="DataGrid" border="0" cellpadding="1" cellspacing="0" rules="rows" style="width:100%;border-collapse:collapse;">
		  <xsl:apply-templates select="UnderlyerValuationDates">
			  <xsl:with-param name="pCssClass" select="'SubEventTransparent'" />
		  </xsl:apply-templates>
		  <tr>
			  <td colspan="6">
				  <div class="DataGrid" style="clear:both;overflow:auto;height:320;width:100%">
					  <table class="DataGrid" cellpadding="1" cellspacing="0" rules="rows" style="width:100%;border-collapse:collapse;">
						  <xsl:apply-templates select="Underlyer"/>
					  </table>
				  </div>
			  </td>
		  </tr>
	  </table>
  </xsl:template>

  <!-- Underlyer -->
  <xsl:template match="Underlyer">
    <xsl:choose>
      <xsl:when test="Constituent=true()">
        <xsl:if test="position()=1">
          <xsl:call-template name="BasketHeader"/>
        </xsl:if>
        <xsl:call-template name="BasketBody"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:if test="position()=1">
          <xsl:call-template name="SingleUnderlyerHeader"/>
        </xsl:if>
        <xsl:call-template name="SingleUnderlyerBody"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Constituent -->
  <xsl:template match="Constituent">
    <xsl:if test="position()=1">
      <xsl:call-template name="ConstituentHeader"/>
    </xsl:if>
    <xsl:call-template name="ConstituentBody"/>
  </xsl:template>

  <!-- BasketHeader -->
  <xsl:template name="BasketHeader">
    <thead>
		<tr class="DataGrid_HeaderStyle">
			<td>
				<xsl:value-of select="$varResource[@name='Underlyer']/value" />
			</td>
			<td style="width:250px;">&#xa0;</td>
			<td>
				<xsl:value-of select="$varResource[@name='EventDates']/value" />
			</td>
			<td>
				<xsl:value-of select="$varResource[@name='Valorisation']/value" />
			</td>
			<td>
				<xsl:value-of select="$varResource[@name='StartPeriod']/value" />
			</td>
			<td>
				<xsl:value-of select="$varResource[@name='EndPeriod']/value" />
			</td>
		</tr>
    </thead>
  </xsl:template>

  <!-- BasketBody -->
  <xsl:template name="BasketBody">
    <thead>
      <tr class="DataGrid_ItemStyle">
		  <td>
			  <xsl:value-of select="$varResource[@name='Basket']/value" />
		</td>
		<td style="width:250px;">
          <table class="hgSubDataGrid" style="width:100%;" cellpadding="0" cellspacing="0" >
            <xsl:apply-templates select="AssetUnderlyer">
              <xsl:with-param name="pSize" select="'30px'" />
            </xsl:apply-templates>
          </table>
        </td>
		  <td style="width:120px;">
          <table class="hgSubDataGrid" style="width:100%;"  cellpadding="0" cellspacing="0" >
            <xsl:apply-templates select="ClassUnderlyer">
              <xsl:with-param name="pCssClass" select="'SubEventTransparent'" />
            </xsl:apply-templates>
          </table>
        </td>
        <td style="width:140px;text-align:right;">
          <xsl:if test="VALORISATION=true()">
            <xsl:value-of select="VALORISATION" />
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
    </thead>
    <xsl:if test="Constituent=true()">
		<tr>
			<td colspan="6">
				<div class="DataGrid" style="clear:both;overflow:auto;height:130px">
					<table class="DataGrid" cellpadding="1" cellspacing="0" rules="rows" style="width:100%;border-collapse:collapse;">
						<xsl:apply-templates select="Constituent"/>
					</table>
				</div>
			</td>
		</tr>
    </xsl:if>
  </xsl:template>

  <!-- SingleUnderlyerHeader -->
  <xsl:template name="SingleUnderlyerHeader">
    <xsl:variable name="UnderlyerType">
      <xsl:choose>
        <xsl:when test = "EVENTTYPE='SHR'">
          <xsl:value-of select="'Share'" />
        </xsl:when>
        <xsl:when test = "EVENTTYPE='IND'">
          <xsl:value-of select="'Index'" />
        </xsl:when>
        <xsl:when test = "EVENTTYPE='BND'">
          <xsl:value-of select="'Bond'" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="EVENTTYPE"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <thead>
      <tr class="DataGrid_HeaderStyle">
        <td>
          <xsl:value-of select="$varResource[@name='Underlyer']/value" />
        </td>
        <td colspan="3">
          <xsl:value-of select="$varResource[@name='Characteristics']/value" />
        </td>
      </tr>
      <tr class="DataGrid_ItemStyle">
		  <td style="width:140px;">
			  <xsl:value-of select="$varResource[@name=$UnderlyerType]/value" />
          <br/>
          <xsl:apply-templates select="AssetUnderlyer" mode="Identifier"/>
        </td>
        <td colspan="3">
          <table class="hgSubDataGrid" style="width:100%;" cellpadding="0" cellspacing="0" >
            <xsl:apply-templates select="AssetUnderlyer">
              <xsl:with-param name="pCssClass" select="'SubEventTransparent'" />
              <xsl:with-param name="pSize" select="'40px'" />
            </xsl:apply-templates>
          </table>
        </td>
      </tr>
      <tr class="DataGrid_AlternatingItemStyle">
        <td style="text-align:center;width:140px;">
          <xsl:value-of select="$varResource[@name='EventDates']/value" />
        </td>
		  <td style="text-align:center;width:140px;">
			  <xsl:value-of select="$varResource[@name='Valorisation']/value" />
        </td>
		  <td style="text-align:center;width:95px;">
			  <xsl:value-of select="$varResource[@name='StartPeriod']/value" />
        </td>
        <td style="text-align:center;" width="95px">
          <xsl:value-of select="$varResource[@name='EndPeriod']/value" />
        </td>
      </tr>
    </thead>
  </xsl:template>

  <!-- SingleUnderlyerBody -->
  <xsl:template name="SingleUnderlyerBody">
    <tr class="DataGrid_ItemStyle">
		<td style="width:120px;">
			<table class="hgSubDataGrid" width="100%" cellpadding="0" cellspacing="0" >
				<xsl:apply-templates select="ClassUnderlyer">
					<xsl:with-param name="pCssClass" select="'SubEventTransparent'" />
				</xsl:apply-templates>
			</table>
		</td>
		<td style="text-align:right;width:140px;">
			<xsl:if test="VALORISATION=true()">
				<xsl:value-of select="VALORISATION" />
			</xsl:if>
			<xsl:if test="VALORISATION=false()">&#xa0;</xsl:if>
		</td>
		<td style="text-align:center;width:95px;">
			<xsl:call-template name="format-shortdate">
			  <xsl:with-param name="xsd-date-time" select="DTSTARTADJ"/>
			</xsl:call-template>
		  </td>
		<td style="text-align:center;width:95px;">
			<xsl:call-template name="format-shortdate">
				<xsl:with-param name="xsd-date-time" select="DTENDADJ"/>
			</xsl:call-template>
		</td>
    </tr>
    <xsl:if test="Constituent=true()">
		<tr>
	        <td colspan="7">
				<div class="DataGrid" style="clear:both;overflow:auto;height:130px">
					<table class="DataGrid" cellpadding="1" cellspacing="0" rules="rows" style="width:100%;border-collapse:collapse;">
						<xsl:apply-templates select="Constituent"/>
					</table>
				</div>
			</td>
		</tr>
    </xsl:if>
  </xsl:template>

  <!-- ConstituentHeader -->
  <xsl:template name="ConstituentHeader">
	<thead>
		<tr class="DataGrid_AlternatingItemStyle" name ="scrollTopPosition" style="position:relative;top:expression(this.offsetParent.scrollTop);">
			<td style="width:140px;">
				<xsl:value-of select="$varResource[@name='Constituent']/value" />
			</td>
			<td style="width:250px;">
				<xsl:value-of select="$varResource[@name='Characteristics']/value" />
			</td>
			<td style="width:120px;">
				<xsl:value-of select="$varResource[@name='EventDates']/value" />
			</td>
			<td style="width:140px;">
				<xsl:value-of select="$varResource[@name='Valorisation']/value" />
			</td>
			<td style="width:100px;">
				<xsl:value-of select="$varResource[@name='StartPeriod']/value" />
			</td>
			<td style="width:100px;">
				<xsl:value-of select="$varResource[@name='EndPeriod']/value" />
			</td>
		</tr>
    </thead>
  </xsl:template>

  <!-- ConstituentBody -->
  <xsl:template name="ConstituentBody">
    <xsl:variable name="ConstituentType">
      <xsl:choose>
        <xsl:when test = "EVENTTYPE='SHR'">
          <xsl:value-of select="'Share'" />
        </xsl:when>
        <xsl:when test = "EVENTTYPE='IND'">
          <xsl:value-of select="'Index'" />
        </xsl:when>
        <xsl:when test = "EVENTTYPE='BND'">
          <xsl:value-of select="'Bond'" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="EVENTTYPE"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <tr class="DataGrid_ItemStyle">
		<td style="text-align:right;width:140px;">
	        <xsl:value-of select="$varResource[@name=$ConstituentType]/value" />
			<br/>
			<xsl:apply-templates select="AssetConstituent" mode="Identifier"/>
      </td>
		<td style="width:250px;">
			<table class="DataGrid" style="width:100%;" cellpadding="0" cellspacing="0" >
				<xsl:apply-templates select="AssetConstituent">
					<xsl:with-param name="pSize" select="'50px'" />
				</xsl:apply-templates>
			</table>
		</td>
		<td style="width:120px;">
			<table class="DataGrid" style="width:100%" cellpadding="0" cellspacing="0" >
				<xsl:apply-templates select="ClassConstituent">
					<xsl:with-param name="pCssClass" select="'ui-subevent ui-subevent-title'" />
				</xsl:apply-templates>
			</table>
		</td>
      <td align="right" width="140px">
        <xsl:if test="VALORISATION=true()">
          <xsl:value-of select="VALORISATION" />
        </xsl:if>
        <xsl:if test="VALORISATION=false()">&#xa0;</xsl:if>
      </td>
		<td style="width:100px;">
        <xsl:call-template name="format-shortdate">
          <xsl:with-param name="xsd-date-time" select="DTSTARTADJ"/>
        </xsl:call-template>
	      </td>
		<td style="width:100px;">
	        <xsl:call-template name="format-shortdate">
				<xsl:with-param name="xsd-date-time" select="DTENDADJ"/>
			</xsl:call-template>
		</td>
    </tr>
  </xsl:template>

  <!-- AssetUnderlyer | AssetConstituent -->
  <xsl:template match="AssetUnderlyer | AssetConstituent" mode="Identifier">
    <xsl:value-of select="AST_IDENTIFIER" />
  </xsl:template>
</xsl:stylesheet>

