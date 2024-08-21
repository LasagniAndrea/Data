<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:param name="pTrackSelected"  select="'0'"/>
  <xsl:param name="pTrackToCompare" select="'0'"/>
  <xsl:param name="pCurrentCulture" select="'en-GB'"/>
  
  <xsl:output method="xml" omit-xml-declaration="yes" indent="yes" encoding="UTF-8"></xsl:output>


	<!-- xslt includes -->
  <xsl:include href="..\..\Library\Resource.xslt"/>
  <xsl:include href="..\..\Library\strFunc.xslt"/>

	<xsl:template match="/">
		<xsl:apply-templates select="TradeTrack"/>
		<script type="text/javascript">
			//<![CDATA[
			function ChangeTrack(demand)
			{
				var index = 0;
				var ddl   = document.getElementById("DDLTracks");
				if (null != ddl)
				{
					var id;
					if (null!=demand)
						id = demand.id;
					index = ddl.selectedIndex;
					if ("TrackFirst" == id)
						index = 0;
					else if ("TrackLast" == id)
						index = ddl.options.length - 1;
					else if ("TrackPrevious" == id)
						index = Math.max(index - 1,0);
					else if	("TrackNext" == id)
						index = Math.min(ddl.options.length-1,index + 1);
					ddl.selectedIndex = index;
					GoToTrack(ddl);
				}
				else
				{
					DisplayTrack("track1");
				}
				var counter = document.getElementById("CurrentTrack");
				if (null != counter)
					counter.innerHTML = index + 1;
			}

			function GoToTrack(ddl)
			{
				DisplayTrack(ddl.options[ddl.selectedIndex].value.substr(1));
				document.forms[0].__TRACKSELECTED.value = ddl.selectedIndex;
				ddl.focus();
			}

			function DisplayTrack(trackNo)
			{
				var divTracks = document.getElementsByTagName("div");
				for(var i = divTracks.length; i--;)
				{
					if (0 == divTracks[i].id.indexOf("track"))
					{
						if (divTracks[i].id == trackNo)
							divTracks[i].style.display = "block";
						else
							divTracks[i].style.display = "none";
					}
				}
			}
			ChangeTrack('');
			//]]>
		</script>

	</xsl:template>	
	
	<xsl:template match="TradeTrack">
		<table style="height:100%;width:100%;">				
			<tr style="height:73px;">
				<td>
					<xsl:call-template name="trade"/>
				</td>
			</tr>				
			<tr style="height:40px"><td>
				<div>
					<table id="trackselection" style="width:100%;">				
						<tr>
							<td style="font-size:10pt;font-weight:bold;width:120px;">Tracks selection</td>
							<td style="text-align:center;width:50px;">
								<select id="DDLTracks" class="ddlCapture" onchange="GoToTrack(this);">
									<xsl:apply-templates select="track" mode="DDL">
										<xsl:with-param name="pTrack" select="$pTrackSelected"/>
									</xsl:apply-templates>
								</select>
							</td>
							<td style="width:16px"><a type="button" id="TrackFirst"    class ="fa-icon fas fa-angle-double-left"  align="middle" style="cursor:pointer;" onclick="ChangeTrack(this);" alt="First"/></td>
							<td style="width:16px"><a type="button" id="TrackPrevious" class ="fa-icon fas fa-angle-left"   align="middle" style="cursor:pointer;" onclick="ChangeTrack(this);" alt="Previous"/></td>
							<td style="width:16px"><a type="button" id="TrackNext"     class ="fa-icon fas fa-angle-right"   align="middle" style="cursor:pointer;" onclick="ChangeTrack(this);" alt="Next"/></td>
							<td style="width:16px"><a type="button" id="TrackLast"     class ="fa-icon fas fa-angle-double-right"  align="middle" style="cursor:pointer;" onclick="ChangeTrack(this);" alt="Last"/></td>
							<td style="width:50px;">&#xa0;</td>
							<td style="font-size:10pt;font-weight:bold;width:90px;">Compare To</td>
							<td style="text-align:center;width:50px;">
								<select name="DDLTracksCompare" id="DDLTracksCompare" class="ddlCapture" onchange="document.forms[0].__TRACKTOCOMPARE.value = this.selectedIndex;">
									<xsl:apply-templates select="track" mode="DDL">
										<xsl:with-param name="pTrack"   select="$pTrackToCompare"/>
									</xsl:apply-templates>
								</select>
							</td>
							<td>&#xa0;</td>
							<td style="cursor:default;text-align:right;width:100px;">
								Track <span id="CurrentTrack"><xsl:value-of select="$pTrackSelected"/></span> / <xsl:value-of select="count(track)"/>
							</td>
						</tr>
					</table>
				</div>
			</td></tr>
			<tr>
				<td>
					<xsl:apply-templates select="track" />
				</td>
			</tr>
		</table>		
	</xsl:template>

	<!-- Track -->
  <!-- EG 20210324 [25562] Gestion de la piste d'audit post UPGV10 (absence TRADE_P, TRADEXML_P) -->
	<xsl:template match="track" mode="DDL">
		<xsl:param name="pTrack"/>
		<xsl:variable name="anchor" select="position()"/>
    <xsl:variable name="star">
      <xsl:if test="data/execution=true()"></xsl:if>
      <xsl:if test="data/execution=false()">*</xsl:if>
    </xsl:variable>
		<xsl:if test="$anchor=$pTrack">
			<option selected="selected" value="#track{$anchor}"><xsl:value-of select="$star"/><xsl:value-of select="action"/> - <xsl:value-of select="date"/></option>		
		</xsl:if>
		<xsl:if test="$anchor!=$pTrack">
			<option value="#track{$anchor}">
        <xsl:value-of select="$star"/><xsl:value-of select="action"/> - <xsl:value-of select="date"/></option>
		</xsl:if>
	</xsl:template>


	<xsl:template match="track">
		<xsl:variable name="anchor" select="position()"/>
		<div id="track{$anchor}" style="display:none;overflow:hidden;height:280px;">
			<table cellpadding="2" cellspacing="1" rules="none" style="height:100%;width:100%;border-collapse:collapse;">
				<tr class="input">
          <td style="white-space:nowrap;width:300px">
						<xsl:value-of select="$varResource[@name='Msg_ACTIONForActionTuning']/value"/> - <xsl:value-of select="$varResource[@name='DTSYS_']/value"/>
					</td>
          <td style="white-space:nowrap;width:160px">
            <xsl:value-of select="$varResource[@name='tradeHeader_market_orderEntered']/value"/>
          </td>
          <td style="white-space:nowrap;width:160px">
            <xsl:value-of select="$varResource[@name='tradeHeader_market_executionDateTime']/value"/>
          </td>
          <td style="white-space:nowrap;width:120px">
            <xsl:value-of select="$varResource[@name='tradeHeader_market_clearedDate']/value"/>
          </td>
          <td style="white-space:nowrap;width:200px">
						<xsl:value-of select="$varResource[@name='USER_']/value"/>
					</td>
					<td style="white-space:nowrap">
						<xsl:value-of select="$varResource[@name='COL_SCREENNAME']/value"/>
					</td>
					<td style="white-space:nowrap">
						<xsl:value-of select="$varResource[@name='HOSTNAME_']/value"/>
					</td>
				</tr>
				<tr>
					<td>
						<xsl:value-of select="action"/> - <xsl:value-of select="date"/>
					</td>
          <td>
            <xsl:value-of select="data/orderEntered"/>
          </td>
          <td>
            <xsl:value-of select="data/execution"/>
          </td>
          <td>
            <xsl:value-of select="data/businessDate"/>
          </td>
          <td>
						<xsl:value-of select="user/identifier"/>
					</td>
					<td>
						<xsl:if test="screenName=true()">
							<xsl:value-of select="screenName"/>
						</xsl:if>
						<xsl:if test="screenName=false()">N/A</xsl:if>
					</td>
					<td>
						<xsl:if test="hostName=true()">
							<xsl:value-of select="hostName"/>
						</xsl:if>
						<xsl:if test="hostName=false()">N/A</xsl:if>
					</td>
				</tr>

				<tr class="input">
					<td colspan="7" >
						<xsl:value-of select="$varResource[@name='APPNAME']/value"/>
					</td>
				</tr>
        
				<tr>
					<td colspan="5" >
						<xsl:if test="application=true()">
							<xsl:value-of select="application"/>&#xa0;<xsl:value-of select="$varResource[@name='APPVERSION']/value"/>&#xa0;<xsl:value-of select="application/@version"/>
						</xsl:if>
						<xsl:if test="application=false()">N/A</xsl:if>
					</td>
					<td colspan="2" >
						<xsl:if test="application/@browser=true()">
							<xsl:value-of select="$varResource[@name='APPBROWSER']/value"/>&#xa0;<xsl:value-of select="application/@browser"/>
						</xsl:if>
					</td>
				</tr>
        
				<tr>
					<td colspan="7" style="text-align:top">
						<xsl:apply-templates select="data" mode="status"/>
					</td>
				</tr>
        
        <!--<tr class="input">
					<td colspan="5" style="text-align:center;white-space:nowrap;font-weight:bold;">
						<xsl:value-of select="$varResource[@name='lblMailSend']/value"/>
					</td>
				</tr>-->
				<tr>
					<td colspan="7" >
						<xsl:apply-templates select="data" mode="confirm"/>
					</td>
				</tr>
			</table>
		</div>
	</xsl:template>

	<!-- Status -->
	<xsl:template match="data" mode="status">
		<xsl:if test="status">
			<div style="clear:both;overflow-y: auto;overflow-x: hidden;height:80px">
				<table border="0" cellpadding="0" cellspacing="0" rules="none" style="width:100%;border-collapse:collapse;">
					<xsl:call-template name="StatusHeader"/>
					<xsl:apply-templates select="status"/>
				</table>
			</div>
		</xsl:if>
		<xsl:if test="status=false()">&#xa0;</xsl:if>
	</xsl:template>
	<xsl:template name="StatusHeader">
		<thead>
			<tr class="input">
				<td style="width:20%">
					<xsl:value-of select="$varResource[@name='TradeStatus']/value"/>
				</td>
				<td style="width:25%">
					<xsl:value-of select="$varResource[@name='VALUE']/value"/>
				</td>
				<td style="width:25%">
					<xsl:value-of select="$varResource[@name='DESCRIPTION']/value"/>
				</td>
				<td style="width:25%">
					<xsl:value-of select="$varResource[@name='Date']/value"/>
				</td>
			</tr>
		</thead>
	</xsl:template>
	<xsl:template match="status">
		<xsl:variable name="statutLabel">
			<xsl:value-of select="'IDST'"/>
			<xsl:call-template name="Upper">
				<xsl:with-param name="source" select="@type"/>
			</xsl:call-template>
		</xsl:variable>
		<tr>
			<td>
				<xsl:value-of select="$varResource[@name=$statutLabel]/value"/>
			</td>
			<td style="background-color:{design/@backColor};color:{design/@foreColor}">
				<xsl:value-of select="@OTCml-Id"/>
			</td>
			<td>
				<xsl:value-of select="displayName"/>
			</td>
			<td>
				<xsl:value-of select="date"/>
			</td>
		</tr>
	</xsl:template>

	<!-- Confirm -->
	<xsl:template match="data" mode="confirm">
		<table border="0" cellpadding="1" cellspacing="1" align="center" rules="none" style="width:100%;border-collapse:collapse;">
      <tr class="input">
				<td colspan="5" style="text-align:center;white-space:nowrap;font-weight:bold;">
					<xsl:value-of select="$varResource[@name='lblMailSend']/value"/>
				</td>
			</tr>
			<xsl:apply-templates select="confirm"/>
		</table>
	</xsl:template>
	<xsl:template match="confirm">
		<xsl:apply-templates select="*"/>
	</xsl:template>
	<xsl:template match="initial | interim | final">
		<xsl:variable name="confirmLabel">
			<xsl:call-template name="Upper">
				<xsl:with-param name="source" select="name(.)"/>
			</xsl:call-template>
		</xsl:variable>
		<tr>
			<td><xsl:value-of select="$varResource[@name=$confirmLabel]/value"/></td>
			<td style="text-align:center;width:110px;">
				<xsl:if test=".='true'">
          <div class="fa-icon fas fa-circle green"></div>
				</xsl:if>
				<xsl:if test=".='false'">&#xa0;</xsl:if>
			</td>
		</tr>
	</xsl:template>

	<!-- Trade -->
	<xsl:template name="trade">
		<table border="0" cellpadding="2" cellspacing="1" rules="none" style="width:100%;border-collapse:collapse;">	
      <tr class="input">
        <td>General Information</td>
				<td><xsl:value-of select="$varResource[@name='IDENTIFIER']/value"/> (Id)</td>
				<td><xsl:value-of select="$varResource[@name='DISPLAYNAME']/value"/></td>
			</tr>
			<tr>
				<td><xsl:value-of select="$varResource[@name='Trade']/value"/></td>
				<td><xsl:value-of select="trade/identifier"/> (<xsl:value-of select="trade/@OTCml-Id"/>)</td>
				<td><xsl:value-of select="trade/displayName"/></td>
			</tr>
			<tr>
				<td><xsl:value-of select="$varResource[@name='Instrument']/value"/></td>
				<td><xsl:value-of select="trade/instrument/identifier"/> (<xsl:value-of select="trade/instrument/@OTCml-Id"/>)</td>
				<td><xsl:value-of select="trade/instrument/displayName"/></td>
			</tr>
		</table>
	</xsl:template>
</xsl:stylesheet>

  