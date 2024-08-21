<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

  <!-- output -->
  <xsl:output method="html" indent="yes" encoding="UTF-8"/>
  <!-- include -->
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
    <table id="tblEvents" border="0" width="100%">
      <tr>
        <td>
		  <table class="DataGrid" cellpadding="1" cellspacing="0" rules="none" style="vertical-align:top;width:100%;border-collapse:collapse;">
            <xsl:apply-templates select="Reset">
              <xsl:with-param name="pCssClass" select="'SubEventTransparent'" />
            </xsl:apply-templates>
          </table>
          <div class="tableHolder DataGrid" style="clear:both;height:370px">
            <xsl:if test="SelfAverage=true()">
              <table class="DataGrid" cellpadding="1" cellspacing="0" rules="none" style="width:100%;border-collapse:collapse;">
                <xsl:apply-templates select="SelfAverage"/>
              </table>
            </xsl:if>
            <xsl:if test="SelfAverage=false()">
              <table class="DataGrid" cellpadding="1" cellspacing="0" rules="none" style="width:100%;border-collapse:collapse;">
                <tr>
                  <td align="center">
                    <xsl:value-of select="$varResource[@name='NoComplementaryInformation']/value" />
                  </td>
                </tr>
              </table>
            </xsl:if>
          </div>
        </td>
      </tr>
    </table>
	</xsl:template>
	
	<!-- SelfAverage -->
	<xsl:template match="SelfAverage">
		<xsl:if test="position()=1"><xsl:call-template name="SelfHeader"/></xsl:if>
		<xsl:call-template name="SelfBody">
			<xsl:with-param name="pName" select="'SelfAverage'" />
			<xsl:with-param name="pAsset" select="AssetSelfAverage" />
			<xsl:with-param name="pClassEvent" select="ClassSelfAverage" />
			<xsl:with-param name="pCssClass" select="'DataGrid_AlternatingItemStyle'" />
		</xsl:call-template>
		<xsl:apply-templates select="SelfReset"/>
	</xsl:template>

  <!-- SelfReset -->
	<xsl:template match="SelfReset">
		<xsl:call-template name="SelfBody">
			<xsl:with-param name="pName" select="'SelfReset'" />
			<xsl:with-param name="pAsset" select="AssetSelfReset" />
			<xsl:with-param name="pClassEvent" select="ClassSelfReset" />
			<xsl:with-param name="pCssClass" select="'DataGrid_ItemStyle'" />
		</xsl:call-template>
	</xsl:template>
	
	<!-- SelfHeader -->
	<xsl:template name="SelfHeader">
		<thead>
			<!-- DataGrid_HeaderStyle-->
			<tr class="rowHeader DataGrid_HeaderStyle">
				<td>
					<table class="hgSubDataGrid" width="100%" cellpadding="0" cellspacing="0" >
						<xsl:apply-templates select="AssetSelfAverage">
							<xsl:with-param name="pCssClass" select="'hgSubDataGrid_AlternatingItemStyle'" />
						</xsl:apply-templates>
					</table>
				</td>
				<td width="120px"><xsl:value-of select="$varResource[@name='EventDates']/value" /></td>
				<td colspan="2"><xsl:value-of select="$varResource[@name='Valorisation']/value" /></td>
				<td width="95px"><xsl:value-of select="$varResource[@name='StartPeriod']/value" /></td>
				<td width="95px"><xsl:value-of select="$varResource[@name='EndPeriod']/value" /></td>
			</tr>
		</thead>
	</xsl:template>
	
	<!-- SelfBody -->
	<xsl:template name="SelfBody">
		<xsl:param name="pName"/>
		<xsl:param name="pAsset"/>
		<xsl:param name="pClassEvent"/>
		<xsl:param name="pCssClass"/>
		
		<xsl:variable name="Self_ToolTip">
			<xsl:value-of select="EVENTCODE" />-<xsl:value-of select="EVENTTYPE" />
		</xsl:variable>

		<xsl:variable name="SubCssClass">hgSub<xsl:value-of select="$pCssClass" /></xsl:variable>

		<tr class="{$pCssClass}">
			<td width="200px" title="{$Self_ToolTip}" style="cursor:pointer;">
				<xsl:value-of select="$varResource[@name=$pName]/value" />
			</td>
			<td>
				<table class="hgSubDataGrid" width="100%" cellpadding="0" cellspacing="0" >
					<xsl:apply-templates select="$pClassEvent">
						<xsl:with-param name="pCssClass" select="$SubCssClass" />
					</xsl:apply-templates>
				</table>
			</td>
			<td align="right" >
				<xsl:if test="VALORISATION=true()"><xsl:value-of select="VALORISATION" /></xsl:if>
				<xsl:if test="VALORISATION=false()">&#xa0;</xsl:if>
			</td>
			<td>
				<table class="hgSubDataGrid" width="100%" cellpadding="0" cellspacing="0" >
					<xsl:apply-templates select="$pAsset">
						<xsl:with-param name="pCssClass" select="$SubCssClass" />
					</xsl:apply-templates>
				</table>
			</td>
			
			<td width="95px" align="center">
				<xsl:call-template name="format-shortdate">
					<xsl:with-param name="xsd-date-time" select="DTSTARTADJ"/>
				</xsl:call-template>
			</td>
			<td width="95px" align="center">
				<xsl:call-template name="format-shortdate">
					<xsl:with-param name="xsd-date-time" select="DTENDADJ"/>
				</xsl:call-template>
			</td>
		</tr>
	</xsl:template>
</xsl:stylesheet>

  