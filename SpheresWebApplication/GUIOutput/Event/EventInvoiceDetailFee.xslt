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
						<xsl:apply-templates select="GrossTurnOver">
							<xsl:with-param name="pCssClass" select="'hgSubDataGrid_ItemStyle'" />
						</xsl:apply-templates>
					</table>
					<div class="tableHolder" style="clear:both;height:180px">
						<table class="DataGrid"  cellpadding="0" cellspacing="0" rules="none" style="width:100%;border-collapse:collapse;">
							<xsl:apply-templates select="DetailFee"/>
						</table>
					</div>
				</td>
			</tr>
		</table>
	</xsl:template>

	<!-- SelfAverage -->
	<xsl:template match="DetailFee">
		<xsl:if test="position()=1">
			<xsl:call-template name="FeeHeader"/>
		</xsl:if>
		<xsl:call-template name="SelfBody">
			<xsl:with-param name="pClassEvent" select="ClassDetailFee" />
			<xsl:with-param name="pCssClass" select="'DataGrid_ItemStyle'" />
		</xsl:call-template>
	</xsl:template>

	<!-- FeeHeader -->
	<xsl:template name="FeeHeader">
		<thead>
			<!-- DataGrid_HeaderStyle-->
			<tr class="rowheader DataGrid_HeaderStyle">
				<td style="width:100px;" colspan="2">
					<xsl:value-of select="$varResource[@name='EventCode']/value" />
				</td>
				<td style="text-align:center;width:120px;">
					<xsl:value-of select="$varResource[@name='EventDates']/value" />
				</td>
				<td >
					<xsl:value-of select="$varResource[@name='IDFEESCHEDULE']/value" />
				</td>
				<td >
					<xsl:value-of select="$varResource[@name='DCF']/value" />
				</td>
				<td  colspan="2">
					<xsl:value-of select="$varResource[@name='EventFeeCurrency']/value" />
				</td>
				<td  colspan="2">
					<xsl:value-of select="$varResource[@name='EventFeeAccountingCurrency']/value" />
				</td>
				<td style="text-align:center;width:95px;">
					<xsl:value-of select="$varResource[@name='StartPeriod']/value" />
				</td>
				<td style="text-align:center;width:95px;">
					<xsl:value-of select="$varResource[@name='EndPeriod']/value" />
				</td>
			</tr>
		</thead>
	</xsl:template>

	<!-- SelfBody -->
	<xsl:template name="SelfBody">
		<xsl:param name="pClassEvent"/>
		<xsl:param name="pCssClass"/>

		<xsl:variable name="SubCssClass">hgSub<xsl:value-of select="$pCssClass" />
	</xsl:variable>
	<xsl:variable name="Self_ToolTip">
		<xsl:value-of select="EVENTCODE" />-<xsl:value-of select="EVENTTYPE" />
	</xsl:variable>

	<tr class="{$pCssClass}">
		<td style="width:50px;">
			<xsl:value-of select="EVENTCODE" />
		</td>
		<td style="width:50px;">
			<xsl:value-of select="EVENTTYPE" />
		</td>
		<td>
			<table class="hgSubDataGrid" style="width:100%" cellpadding="0" cellspacing="0" >
				<xsl:apply-templates select="$pClassEvent">
					<xsl:with-param name="pCssClass" select="$SubCssClass" />
				</xsl:apply-templates>
				</table>

			</td>
			<td><xsl:value-of select="FEESCHEDULE_IDENTIFIER" /></td>
			<td><xsl:value-of select="FORMULADCF" /></td>
			<td style="text-align:right;" ><xsl:value-of select="VALORISATION" /></td>
			<td><xsl:value-of select="UNIT" /></td>

			<xsl:apply-templates select="FeeAccountingAmount"/>			

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
	</xsl:template>

	<!-- FeeAccountingAmount -->
	<xsl:template match="FeeAccountingAmount">
		<td style="text-align:right;">
			<xsl:value-of select="VALORISATION" />
		</td>
		<td>
			<xsl:value-of select="UNIT" />
				</td>
	</xsl:template>

	</xsl:stylesheet>

