<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:output omit-xml-declaration="yes" method="html" indent="yes" encoding="UTF-8"></xsl:output> 
	<xsl:param name="pWithTitle"       select="1"/>
	<xsl:param name="pWithLabel"       select="1"/>    
	<xsl:param name="pWithAllExchange" select="1"/>      
	<xsl:param name="pWithAllClosing"  select="1"/> 
	<xsl:param name="pWithAllFlows"    select="1"/>
	<xsl:param name="pCurrentCulture" select="'en-GB'"/>


  <!-- xslt includes -->
	<xsl:include href="..\..\Library\Resource.xslt"/>
	<xsl:include href="..\..\Library\DtFunc.xslt"/>
	<xsl:include href="..\..\Library\xsltsl\date-time.xsl"/>
	
	<!-- Ear ressource file -->
	<xsl:variable name="varEarEnumFileResource">
		<xsl:value-of select="$PathResource"/>
		<xsl:text>\EarEnumsRessource.xml</xsl:text>
	</xsl:variable>
	
	<xsl:variable name="vEarEnumRes"         select="document($varEarEnumFileResource)//EarEnums"/>	
	
	<!-- Ear enums file -->
	<xsl:variable name="varEnumsFile">
		<xsl:text>EarEnums.xml</xsl:text>
	</xsl:variable>

	<xsl:variable name="vExchangeTypeEnum"	 select="document($varEnumsFile)//EarEnums//ExchangeType"/>
	<xsl:variable name="vSideEnum"			 select="document($varEnumsFile)//EarEnums//Side"/>
	
	<xsl:variable name="vExchangeTypeEnumCount" select="count($vExchangeTypeEnum)"/>
	<xsl:variable name="vSideEnumCount"		 select="count($vSideEnum)"/>
	
  	<!-- Keys -->
	<xsl:key      name="kEarDet"             match="EARDET"    use="@IDEAR"/>
  	<xsl:key      name="kEarDetInstr"        match="EARDET"    use="@INSTR_IDENTIFIER"/>  	
  	<xsl:key      name="kEventClass"         match="EARCLASS"  use="@CLASS"/>
  	
  	<!-- Used for rising compatibility with version before v1.1.9.0-->
  	<xsl:key      name="kEventClass_v1.1.9"  match="EARCLASS"  use="@TYPE"/>
  	
  	<xsl:key      name="kInstrumentNO"       match="EARCLASS"  use="@INSTRUMENTNO"/>
  	<xsl:key      name="kStreamNO"           match="EARCLASS"  use="@STREAMNO"/>
  	
  	<!-- Global variables -->
  	<xsl:variable name="vEarEventClass"      select="*/EARBOOK/EAR/EARXML/EARDATE"/>
  	
  	<xsl:variable name="vAllEventClass"      select="$vEarEventClass/EARCLASS[generate-id()=generate-id(key('kEventClass',@CLASS)) 
  														and ($pWithAllClosing!=0 or (@CLASS!='LIN' and @CLASS!='CMP'))]"/>
  	
  	<!-- Used for rising compatibility with version before v1.1.9.0 -->
  	<xsl:variable name="vAllEventClass_v1.1.9"   select="$vEarEventClass/EARCLASS[generate-id()=generate-id(key('kEventClass_v1.1.9',@TYPE)) 
  														and ($pWithAllClosing!=0 or (@TYPE!='LIN' and @TYPE!='CMP'))]"/>													
  														
	<xsl:variable name="vAllInstrumentNO"    select="$vEarEventClass/EARCLASS[generate-id()=generate-id(key('kInstrumentNO',@INSTRUMENTNO))]"/>
  	<xsl:variable name="vAllStreamNO"        select="$vEarEventClass/EARCLASS[generate-id()=generate-id(key('kStreamNO',@STREAMNO))]"/>
  	
  	<xsl:variable name="vEarAmount"          select="*/EARBOOK/EAR/EARXML/EARDATE/EARCLASS/EARAMOUNT[$pWithAllFlows!=0 or (@IDEARDAY>0 and not(@IDEARCOMMON))]"/>
  	<xsl:variable name="vEarNomAmount"       select="*/EARBOOK/EAR/EARXML/EARDATE/EARCLASS/EARNOMAMOUNT"/>
	<xsl:variable name="vAllInstrIdent"      select="*/EARBOOK/EARDET[generate-id()=generate-id(key('kEarDetInstr',@INSTR_IDENTIFIER))]"/>
	
	<xsl:variable name="vNbEar"              select="count(*/EARBOOK/EAR)"/>
		
	<!-- Width Varibles -->
	<xsl:variable name="vWidthUnit"          select="px"/>
	<xsl:variable name="vAmountWidth"        select="110"/>
	<xsl:variable name="vCurrencyWidth"      select="35"/>
	
	<xsl:template match="/">	
		<script type="text/javascript">

      function GUI_ShowHide()
      {
				var divbody  = document.getElementById('divbody');
				divbody.style.display='none';

				var butDailyEar  = document.getElementById('BUTDAILYEAR');
				var butAllEar    = document.getElementById('BUTALLEAR');
				var butSelectEar = document.getElementById('BUTSELECTEAR');

				if (butDailyEar!=null)
				{
					if (butAllEar!=null)
					{
						if (butSelectEar!=null)
						{
							var objDtStartEar    = document.getElementById('TXTDTSTARTEAR');
							var objDtEndEar      = document.getElementById('TXTDTENDEAR');
							var objPnlEventDates = document.getElementById('divtbear3');

							if (objDtStartEar!=null)
							{
								if (objDtEndEar!=null)
								{
									if (butDailyEar.checked || butAllEar.checked )
									{
										objPnlEventDates.style.display = 'none';
									}
									else
									{
										objPnlEventDates.style.display = 'inline';
									}
								}
							}
						}
					}
				}
      }
    </script>
		<xsl:apply-templates select="EARS"/>
	</xsl:template>
	
	<!-- Title -->	
	<xsl:template match="EARS">
		<xsl:if test="0&lt;$vExchangeTypeEnumCount">
			<xsl:if test="0&lt;$vNbEar">	
				<xsl:call-template name="DisplayEARBOOK"/>				
			</xsl:if>
		</xsl:if>
	</xsl:template>
	<!-- EarBook -->
	<xsl:template name="DisplayEARBOOK">
		<table width="100%" border="0" cellspacing="0" cellpadding="0">
			<xsl:for-each select="EARBOOK">
				<xsl:variable name="vCurrentIDB"><xsl:value-of select="@IDB"/></xsl:variable>													
				<xsl:if test="count(EAR/EARXML/EARDATE/EARCLASS/EARAMOUNT[$pWithAllFlows!=0 or (@IDEARDAY>0 and not(@IDEARCOMMON))])!=0">
					<xsl:variable name="vPreviousIDB" select="preceding::EARBOOK[1]/@IDB"/>
					<xsl:variable name="vBookHeaderToDisplay">
						<xsl:choose>
							<xsl:when test="$vPreviousIDB">
								<xsl:choose>
									<xsl:when test="$vPreviousIDB = @IDB">
										0
									</xsl:when>
									<xsl:otherwise>
										1
									</xsl:otherwise>
								</xsl:choose>
							</xsl:when>
							<xsl:otherwise>
								1
							</xsl:otherwise>
						</xsl:choose>
					</xsl:variable>	
					<xsl:if test="$vBookHeaderToDisplay =1">
						<!--hr/-->													
						<tr><td colspan="3">
						<table class="DataGrid" border="0" cellspacing="0" cellpadding="0" rules="All" style="background-color:Transparent;width:100%;border:outset 1pt ridge;border-collapse:collapse;">			
							<tr class="DataGrid_HeaderStyle">
								<td width="20px;" nowrap="nowrap">
                  <img id="{$vCurrentIDB}_IMG" name="{$vCurrentIDB}" src="images/png/expand.png" align="middle" onclick="ExpandCollapse(event);" style="cursor:hand"/>
								</td>			
								<td class="Header_EarSubTitleLeft" width="150px;"><xsl:value-of select="$varResource[@name='Book']/value" /></td>
								<td class="Header_EarTitleLeft">
									<xsl:value-of select="@BOOK_IDENTIFIER"/>
									<xsl:if test="$pWithLabel!=0">
										<span class="SmallerCode">(id:<xsl:value-of select="$vCurrentIDB"/>)</span>
									</xsl:if>
								</td>
							</tr>
							<tr class="DataGrid_ItemStyle">			
								<td class="Header_EarSubTitleLeft" width="150px;" colspan="2"><xsl:value-of select="$varResource[@name='IDA_ENTITY']/value" /></td>
								<td class="Header_EarTitleLeft">
									<xsl:value-of select="@ENTITY_IDENTIFIER"/>
									<xsl:if test="$pWithLabel!=0">
										<span class="SmallerCode">(id:<xsl:value-of select="@IDA_ENTITY"/>)</span>
									</xsl:if>
								</td>
							</tr>
							<tr class="DataGrid_ItemStyle">
								<td class="Header_EarSubTitleLeft" width="150px;" colspan="2"><xsl:value-of select="$varResource[@name='Trade']/value" /></td>
								<td class="Header_EarTitleLeft">
									<xsl:value-of select="@TRADE_IDENTIFIER"/>
									<xsl:if test="$pWithLabel!=0">
										<span class="SmallerCode">(id:<xsl:value-of select="@IDT"/>)</span>
									</xsl:if>
								</td>
							</tr>							
						</table></td></tr>
						<hr/>			
					</xsl:if>		
					<xsl:apply-templates select="EAR"/>
				</xsl:if>		
			</xsl:for-each>							
		</table>
	</xsl:template>
	
	<!-- Ear by date -->	
	<xsl:template match="EAR">
		<xsl:variable name="vCurrentIDB"><xsl:value-of select="../@IDB"/></xsl:variable>
		<xsl:variable name="vIDEAR"><xsl:value-of select="../@IDEAR"/></xsl:variable>
				
		<xsl:variable name="vPreviousEAR"><xsl:value-of select="preceding::EAR[1]/@IDEAR"/></xsl:variable> 
		<xsl:variable name="vFollowEAR" select="following::EAR[1]/@IDEAR"/>	
		<tr><td colspan="3"><A name="{$vIDEAR}_POS"></A></td></tr>		
		<tr><td width="3pt;"></td><td>
		<table id="{$vCurrentIDB}" class="DataGrid" border="0" cellspacing="0" cellpadding="1" rules="All" style="background-color:Transparent;width:100%;border:outset 1pt groove;border-collapse:collapse;display:none">				
			<THEAD>
			<tr class="DataGrid_ItemStyle">
				<!-- OTC_PROCESS_ACC -->
				<xsl:variable name="vWithAllExchangeStyle">
					<xsl:choose>
						<xsl:when test="$pWithAllExchange">
							background-position-y:0;
						</xsl:when>
						<xsl:otherwise>
							background-position-y:center;
						</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				<xsl:variable name="vDeactivStyle">
					<xsl:choose>
						<xsl:when test="@IDSTACTIVATION='DEACTIV'">
							text-decoration: line-through;
						</xsl:when>
						<xsl:otherwise>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				<td class="DataGrid_HeaderStyle" style="{$vWithAllExchangeStyle}" colspan="3" nowrap="1">
          <img id="{$vIDEAR}_IMG" name="{$vIDEAR}" src="images/png/expand.png" align="middle" onclick="ExpandCollapse(event);" style="cursor:hand"/>
          <span style="{$vDeactivStyle}+ font-weight:bolder;">
						<xsl:call-template name="format-shortdate">
							<xsl:with-param name="xsd-date-time" select="../@DTEVENT"/>
						</xsl:call-template>
						<xsl:if test="@IDSTACTIVATION!='REGULAR'">
							&#xa0;<xsl:value-of select="$vEarEnumRes/StActivation[@code=current()/@IDSTACTIVATION]/Description[@resource=$pCurrentCulture]"/>							
						</xsl:if>
					</span>
					<xsl:if test="$pWithLabel!=0">
						<span class="SmallerCode">(EVENT)</span>
					</xsl:if>
					&#xa0;
					<span style="{$vDeactivStyle}+ font-weight: lighter;">				
						<xsl:call-template name="format-shortdate">
							<xsl:with-param name="xsd-date-time" select="../@DTEAR"/>
						</xsl:call-template>
					</span>
					<xsl:if test="$pWithLabel!=0">
						<span class="SmallerCode">(EAR id: <xsl:value-of select="../@IDEAR"/>)</span>
					</xsl:if>										
					&#xa0;&#xa0;&#xa0;
					<xsl:if test="$vPreviousEAR>0">
						<A  href="#{$vPreviousEAR}_POS" title="Preceding EAR">
							<span id="{$vIDEAR}" style="font-family:Wingdings 3;font-size:15pt;font-weight:lighter;display:none">#</span>
						</A>
					</xsl:if>
					<xsl:if test="$vFollowEAR>0">
						<A href="#{$vFollowEAR}_POS" title="Following EAR">
							<span id="{$vIDEAR}" style="font-family:Wingdings 3;font-size:15pt;font-weight:lighter;display:none">$</span>
						</A>
					</xsl:if>&#xa0;
					<A href="#toppage" title="Top">
						<span style="font-family:Wingdings 3;font-size:15pt;font-weight:lighter">+</span>
					</A>
					<A href="#bottom" title="Down">
						<span style="font-family:Wingdings 3;font-size:15pt;font-weight:lighter">,</span>
					</A>
				</td>
				<xsl:call-template name="DisplayHeaderExchangeType">
					<xsl:with-param name="pCurrentEar" select="$vIDEAR"/>
				</xsl:call-template>
			</tr>
			</THEAD>
			<xsl:call-template name="DisplayNominalResult">
					<xsl:with-param name="pCurrentEar" select="$vIDEAR"/>
				</xsl:call-template>
			<xsl:call-template name="DisplayResult">
					<xsl:with-param name="pCurrentEar" select="$vIDEAR"/>
				</xsl:call-template>
		</table></td><td width="3pt"></td></tr>
		<!--br/-->
	</xsl:template>

	<!-- EarClass for Nominal only -->
	<xsl:template name="DisplayNominalResult">
		<xsl:param name="pCurrentEar"/>	
		<xsl:for-each select="EARXML/EARDATE/EARCLASS">
			<xsl:sort select="@CODE" data-type="text"/>
			<xsl:sort select="@CLASS" data-type="text"/>
			
			<xsl:if test="@CODE='NOS'">
				<xsl:if test="preceding-sibling::EARCLASS[@CODE='NOS']=false()">
					<tr id="{$pCurrentEar}" style="display:none">
						<th class="DataGrid_AlternatingItemStyle" colspan="3"><xsl:value-of select="$varResource[@name='NOMINAL']/value" /></th>
						<xsl:call-template name="DisplayHeaderNominal"/>
					</tr>
				</xsl:if>
				<xsl:apply-templates select="EARNOMAMOUNT"/>				
			</xsl:if>
		</xsl:for-each>
	</xsl:template>	

	<!-- EarClass for amount excepted Nominal -->
	<xsl:template name="DisplayResult">
		<xsl:param name="pCurrentEar"/>	
		<tr id="{$pCurrentEar}" style="display:none">
			<th class="DataGrid_AlternatingItemStyle" colspan="3"><xsl:value-of select="$varResource[@name='Amounts']/value" /></th>
			<xsl:call-template name="DisplayHeaderSide"/>
		</tr>
		
		<xsl:variable name="vEarClassNotNOS" select="EARXML/EARDATE/EARCLASS[@CODE!='NOS']"/>
		<xsl:variable name="vCurentEAR_v1.1.9" select="EARXML/EARDATE/EARCLASS[@TYPE]"/>
		
		<xsl:for-each select="$vAllInstrumentNO">			
			<xsl:sort select="@INSTRUMENTNO" data-type="number"/>	
							
			<xsl:variable name="vInstrumentNo" select="@INSTRUMENTNO"/>
			
			<xsl:for-each select="$vAllStreamNO">						
				<xsl:sort select="@STREAMNO" data-type="number"/>
					
				<xsl:variable name="vStreamNo" select="@STREAMNO"/>					
								
  				<xsl:choose>
  					<!-- For rising compatibility with version before v1.1.9.0-->
					<xsl:when test="count($vCurentEAR_v1.1.9) > 0">
						<xsl:call-template name="DisplayEventClass_v1.1.9">
							<xsl:with-param name="pCurrentEar" select="$pCurrentEar"/>
							<xsl:with-param name="pEarClassNotNOS" select="$vEarClassNotNOS"/>
							<xsl:with-param name="pAllEventClass" select="$vAllEventClass_v1.1.9"/>
							<xsl:with-param name="pInstrumentNo" select="$vInstrumentNo"/>
							<xsl:with-param name="pStreamNo" select="$vStreamNo"/>
						</xsl:call-template>
  					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="DisplayEventClass">
							<xsl:with-param name="pCurrentEar" select="$pCurrentEar"/>
							<xsl:with-param name="pEarClassNotNOS" select="$vEarClassNotNOS"/>
							<xsl:with-param name="pAllEventClass" select="$vAllEventClass"/>
							<xsl:with-param name="pInstrumentNo" select="$vInstrumentNo"/>
							<xsl:with-param name="pStreamNo" select="$vStreamNo"/>
						</xsl:call-template>
					</xsl:otherwise>
				</xsl:choose>
				
			</xsl:for-each>
		</xsl:for-each>				
	</xsl:template>	
		
	<!-- DisplayEventClass_v1.1.9 - For rising compatibility with version before v1.1.9.0-->
	<xsl:template name="DisplayEventClass_v1.1.9">
		<xsl:param name="pCurrentEar"/>			
		<xsl:param name="pEarClassNotNOS"/>
		<xsl:param name="pAllEventClass"/>
		<xsl:param name="pInstrumentNo"/>
		<xsl:param name="pStreamNo"/>
		
		<xsl:for-each select="$pAllEventClass">	
			<xsl:sort select="@TYPE" data-type="text"/>
			
			<xsl:variable name="vEarClassType" select="@TYPE"/>							
			<xsl:variable name="vEarClass" select="$pEarClassNotNOS[(@TYPE=$vEarClassType) and (@INSTRUMENTNO=$pInstrumentNo) and (@STREAMNO=$pStreamNo)]"/>
						
			<xsl:if test="count($vEarClass/EARAMOUNT[$pWithAllFlows!=0 or (@IDEARDAY>0 and not(@IDEARCOMMON))])>0">		
				<tr id="{$pCurrentEar}" style="display:none">					
					<xsl:call-template name="DisplayEarKey">
						<xsl:with-param name="pEarClassType" select="$vEarClass/@TYPE"/>
						<xsl:with-param name="pInstrumentNO" select="$vEarClass/@INSTRUMENTNO"/>
						<xsl:with-param name="pStreamNO"     select="$vEarClass/@STREAMNO"/>
					</xsl:call-template>
					
					<xsl:call-template name="DisplayHeaderSide">
						<xsl:with-param name="pIsFilled" select="number(1)"/>
					</xsl:call-template>	
							
					<xsl:for-each select="$vEarClass">
						<xsl:apply-templates select="EARAMOUNT[$pWithAllFlows!=0 or (@IDEARDAY>0 and not(@IDEARCOMMON))]">
							<xsl:sort select="@FLOWTYPE" data-type="text"/>
						</xsl:apply-templates>
					</xsl:for-each>
				</tr>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>
	
	<!-- DisplayEventClass -->
	<xsl:template name="DisplayEventClass">
		<xsl:param name="pCurrentEar"/>			
		<xsl:param name="pEarClassNotNOS"/>
		<xsl:param name="pAllEventClass"/>
		<xsl:param name="pInstrumentNo"/>
		<xsl:param name="pStreamNo"/>
		
		<xsl:for-each select="$pAllEventClass">	
			<xsl:sort select="@CLASS" data-type="text"/>
			
			<xsl:variable name="vEarClassType" select="@CLASS"/>							
			<xsl:variable name="vEarClass" select="$pEarClassNotNOS[(@CLASS=$vEarClassType) and (@INSTRUMENTNO=$pInstrumentNo) and (@STREAMNO=$pStreamNo)]"/>
						
			<xsl:if test="count($vEarClass/EARAMOUNT[$pWithAllFlows!=0 or (@IDEARDAY>0 and not(@IDEARCOMMON))])>0">		
				<tr id="{$pCurrentEar}" style="display:none">					
					<xsl:call-template name="DisplayEarKey">
						<xsl:with-param name="pEarClassType" select="$vEarClass/@CLASS"/>
						<xsl:with-param name="pInstrumentNO" select="$vEarClass/@INSTRUMENTNO"/>
						<xsl:with-param name="pStreamNO"     select="$vEarClass/@STREAMNO"/>
					</xsl:call-template>
					
					<xsl:call-template name="DisplayHeaderSide">
						<xsl:with-param name="pIsFilled" select="number(1)"/>
					</xsl:call-template>	
							
					<xsl:for-each select="$vEarClass">
						<xsl:apply-templates select="EARAMOUNT[$pWithAllFlows!=0 or (@IDEARDAY>0 and not(@IDEARCOMMON))]">
							<xsl:sort select="@SIDE" data-type="text"/>
						</xsl:apply-templates>
					</xsl:for-each>
				</tr>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>
		
	<!-- DisplayHeaderExchangeType -->
	<xsl:template name="DisplayHeaderExchangeType">
		<xsl:param name="pCurrentEar"/>			
		<xsl:for-each select="$vExchangeTypeEnum">
			<xsl:sort select="@ORDER" data-type="number"/>
			<xsl:variable name="vExchangeTypeEnumCode"><xsl:value-of select="@EXCHANGETYPE"/></xsl:variable>
			
			<xsl:if test="($pWithAllExchange!=0 or $vExchangeTypeEnumCode='FCU' or $vExchangeTypeEnumCode='ACU_EVENTDATE')">
				<th id="{$pCurrentEar}" class="DataGrid_AlternatingItemStyle" colspan="{$vSideEnumCount*2}" style="display:none">
					<xsl:value-of select="$vExchangeTypeEnumCode"/>
					<xsl:if test="$pWithLabel!=0">
						<span class="SmallerCode"><br/>(<xsl:value-of select="$vEarEnumRes/ExchangeType[@code=$vExchangeTypeEnumCode]/Description[@resource=$pCurrentCulture]"/>)</span>
					</xsl:if>
				</th>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>

	<!-- DisplayHeaderSide -->
	<xsl:template name="DisplayHeaderSide">
		<xsl:param name="pIsFilled" select="0"/>
		<xsl:for-each select="$vExchangeTypeEnum">
			<xsl:sort select="@ORDER" data-type="number"/>
			<xsl:variable name="vExchangeTypeEnumCode"><xsl:value-of select="@EXCHANGETYPE"/></xsl:variable>		
			<xsl:if test="($pWithAllExchange!=0 or $vExchangeTypeEnumCode='FCU' or $vExchangeTypeEnumCode='ACU_EVENTDATE')">
				<xsl:for-each select="$vSideEnum">
					<xsl:sort select="@ORDER" data-type="number"/>					
					<xsl:variable name="vSideClassName">
						<xsl:choose>
							<xsl:when test ="$pIsFilled=0">
								<xsl:choose>
									<xsl:when test ="(@SIDE = 'PAY') or (@SIDE = 'PAID')"><xsl:value-of select="'Ear_HeaderPaid'"/></xsl:when>
									<xsl:when test ="(@SIDE = 'REC') or (@SIDE = 'RECEIVED')"><xsl:value-of select="'Ear_HeaderReceived'"/></xsl:when>
									<xsl:otherwise><xsl:value-of select="'DataGrid_ItemStyle'"/></xsl:otherwise>
								</xsl:choose>
							</xsl:when>
							<xsl:otherwise>
								<xsl:choose>
									<xsl:when test ="(@SIDE = 'PAY') or (@SIDE = 'PAID')"><xsl:value-of select="'Ear_Paid'"/></xsl:when>
									<xsl:when test ="(@SIDE = 'REC') or (@SIDE = 'RECEIVED')"><xsl:value-of select="'Ear_Received'"/></xsl:when>
									<xsl:otherwise><xsl:value-of select="'DataGrid_ItemStyle'"/></xsl:otherwise>
								</xsl:choose>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:variable>					
					<th class="{$vSideClassName}" colspan="2">
						<xsl:if test="$pIsFilled=0">
							<xsl:value-of select="$vEarEnumRes/Side[@code=current()/@SIDE]/Description[@resource=$pCurrentCulture]"/>
						</xsl:if>
					</th>
				</xsl:for-each>
			</xsl:if>				
		</xsl:for-each>
	</xsl:template>

	<!-- DisplayHeaderNominal -->
	<xsl:template name="DisplayHeaderNominal">
		<xsl:variable name="vColSpan">
			<xsl:choose>
				<xsl:when test ="$pWithAllExchange!=0"><xsl:value-of select="$vSideEnumCount*2*$vExchangeTypeEnumCount"/></xsl:when>
				<xsl:otherwise><xsl:value-of select="$vSideEnumCount*4"/></xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<th class="Ear_HeaderNotional" colspan="{$vColSpan}">
			<xsl:value-of select="'NOS'"/>
			<xsl:if test="$pWithLabel!=0">
				<span class="SmallerCode">(<xsl:value-of select="$vEarEnumRes/EventCodeType[@code='NOS']/Description[@resource=$pCurrentCulture]"/>)</span>
			</xsl:if>							
		</th>
	</xsl:template>
	
	<!-- DisplayEarKey -->
	<xsl:template name="DisplayEarKey">
		<xsl:param name="pEarClassType"/>
		<xsl:param name="pInstrumentNO"/>
		<xsl:param name="pStreamNO"/>
		
		<xsl:variable name="vClassByAmountType">
			<xsl:choose>
				<xsl:when test ="@IDEARCALC"><xsl:value-of select="'Ear_Key'"/></xsl:when>
				<xsl:otherwise><xsl:value-of select="'Ear_Key'"/></xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<!-- EventClass -->
		<xsl:variable name="vColSpan">
			<xsl:choose>
				<xsl:when test ="$pStreamNO = 0 and $pInstrumentNO = 0">2</xsl:when>
				<xsl:otherwise>1</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<th class="{$vClassByAmountType}" width="150pt;" nowrap="0" ></th>
		<!--th class="{$vClassByAmountType}" colspan="{$vColSpan}" width="150pt;" style="text-align: left;" nowrap="1" >
			<xsl:value-of select="$pEarClassType"/>
			<xsl:if test="$pWithLabel!=0">
				<span class="SmallerCode">(<xsl:value-of select="$vEarEnumRes/EventClassType[@code=$pEarClassType]/Description[@resource=$pCurrentCulture]"/>)</span>
			</xsl:if>			
		</th-->	
		<th width="200pt;" class="{$vClassByAmountType}" style="text-align: left;" colspan="{$vColSpan}" nowrap="1" >	
			<xsl:value-of select="$pEarClassType"/>
			<xsl:if test="$pWithLabel!=0">
				<span class="SmallerCode">(<xsl:value-of select="$vEarEnumRes/EventClassType[@code=$pEarClassType]/Description[@resource=$pCurrentCulture]"/>)</span>
			</xsl:if>
		</th>			
		<xsl:if test = "not($pStreamNO = 0 and $pInstrumentNO = 0)">	
			<!-- Instrument -->
			<th width="300pt;" class="{$vClassByAmountType}" style="text-align: center;" nowrap="1">
				<!-- Instrument Identifier-->
				<xsl:for-each select="$vAllInstrIdent">
					<xsl:if test = "(@INSTRUMENTNO = $pInstrumentNO)">
						&#xa0;<xsl:value-of select="@INSTR_IDENTIFIER"/>							
					</xsl:if>
				</xsl:for-each>
				<xsl:choose>
					<xsl:when test = "$pStreamNO = 0">
						<!-- Instrument ID -->	
						<xsl:for-each select="$vAllInstrIdent">
							<xsl:if test = "(@INSTRUMENTNO = $pInstrumentNO) and $pWithLabel!=0">
								<span class="SmallerCode" >(id:<xsl:value-of select="@IDI"/>)</span>						
							</xsl:if>
						</xsl:for-each>
					</xsl:when>
					<xsl:otherwise>
						<xsl:for-each select="key('kEarDet',./ancestor::EARBOOK/@IDEAR)">
							<xsl:if test = "(@INSTRUMENTNO = $pInstrumentNO) and (@STREAMNO = $pStreamNO)">
								<!-- Instrument ID -->	
								<xsl:if test = "$pInstrumentNO != 0 and $pWithLabel!=0">
									<span class="SmallerCode" >(id: <xsl:value-of select="@IDI"/>)</span>
								</xsl:if>
								<!-- Stream -->	
								<!--xsl:if test = "$pStreamNO != 0">
									<span class="SmallerCode" ><br/>&#xa0;Stream&#xa0;<xsl:value-of select="@STREAMNO"/></span>
								</xsl:if-->
								&#xa0;Stream&#xa0;<xsl:value-of select="@STREAMNO"/>							
							</xsl:if>
						</xsl:for-each>
					</xsl:otherwise>
				</xsl:choose>
			</th>
		</xsl:if>
	</xsl:template>

	<!-- Get Money value for an exchangeType and side Columns - For rising compatibility with version before v1.1.9.0-->
	<xsl:template name="GetMoneyByExchangeTypeAndSide_v1.1.9">
		<xsl:param name="pMoney"/>
		<xsl:param name="pCurrentExchangeType"/>
		<xsl:param name="pCurrentSide"/>	
		<xsl:for-each select="$pMoney">
			<xsl:variable name="vPos" select="position()"/>			
			<xsl:if test="(../@FLOWTYPE = $pCurrentSide) and (@EXCHANGETYPE = $pCurrentExchangeType) and ((@STATUS = 'SUCCESS') or not (@STATUS) )">
				<xsl:value-of select="$vPos"/>
			</xsl:if>				
		</xsl:for-each>
	</xsl:template>	
	
	<!-- Get Money value for an exchangeType and side Columns -->
	<xsl:template name="GetMoneyByExchangeTypeAndSide">
		<xsl:param name="pMoney"/>
		<xsl:param name="pCurrentExchangeType"/>
		<xsl:param name="pCurrentSide"/>	
		<xsl:for-each select="$pMoney">
			<xsl:variable name="vPos" select="position()"/>			
			<xsl:if test="(../@SIDE = $pCurrentSide) and (@EXCHANGETYPE = $pCurrentExchangeType) and ((@STATUS = 'SUCCESS') or not (@STATUS) )">
				<xsl:value-of select="$vPos"/>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>	
	
	<!-- Get Nominal Money value for an exchangeType Columns -->
	<xsl:template name="GetNominalMoneyByExchangeType">
		<xsl:param name="pMoney"/>
		<xsl:param name="pCurrentExchangeType"/>
		<xsl:for-each select="$pMoney">
			<xsl:variable name="vPos" select="position()"/>
			<xsl:if test="(@EXCHANGETYPE = $pCurrentExchangeType) and ((@STATUS = 'SUCCESS') or not (@STATUS) )">
				<xsl:value-of select="$vPos"/>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>
	
	<!-- Display Nominal Amounts -->
	<xsl:template match="EARNOMAMOUNT">
		<xsl:variable name="vIDEAR"><xsl:value-of select="../../../../@IDEAR"/></xsl:variable>
		<xsl:variable name="vInstrumentNo" select="../@INSTRUMENTNO"/>
		<xsl:variable name="vStreamNo"     select="../@STREAMNO"/>
		<xsl:variable name="vIDEARNOM"     select="@IDEARNOM"/>
		<xsl:variable name="vTYPE"         select="@TYPE"/>
		<xsl:variable name="vClassByAmountType">
			<xsl:choose>
				<xsl:when test ="@IDEARCALC"><xsl:value-of select="'Ear_Key'"/></xsl:when>
				<xsl:otherwise><xsl:value-of select="'Ear_Key'"/></xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<tr id="{$vIDEAR}" style="display:none">
			<!-- Instrument -->
			<xsl:for-each select="key('kEarDet',./ancestor::EARBOOK/@IDEAR)">
					<xsl:if test = "(@INSTRUMENTNO = $vInstrumentNo) and (@STREAMNO = $vStreamNo)">
						<!-- Instrument -->			
						<th class="{$vClassByAmountType}" colspan="3">
							<xsl:choose>
								<xsl:when test="$vIDEARNOM">
									<span style="color:DarkGray"><xsl:value-of select="'Nom'"/></span>
									<xsl:if test="$pWithLabel!=0">
										<span class="SmallerCode" style="color:DarkGray">(id: <xsl:value-of select="$vIDEARNOM"/>)</span>
									</xsl:if>
								</xsl:when>
							</xsl:choose>&#xa0;		
							<xsl:value-of select="@INSTR_IDENTIFIER"/>							
							<xsl:if test = "$vInstrumentNo != 0 and $pWithLabel!=0">
								<span class="SmallerCode" >(id:<xsl:value-of select="@IDI"/>)</span>
							</xsl:if>
							<xsl:if test = "$vTYPE = 'CCU' or  $vTYPE = 'PCU'">
								&#xa0;<xsl:value-of select="$vTYPE"/>
								<xsl:if test="$pWithLabel!=0">
									<span class="SmallerCode">(<xsl:value-of select="$vEarEnumRes/AmountType[@code=$vTYPE]/Description[@resource=$pCurrentCulture]"/>)</span>
								</xsl:if>
							</xsl:if>
							<!--xsl:if test = "$vStreamNo != 0">
								<span class="SmallerCode"><br/>&#xa0;Stream&#xa0;<xsl:value-of select="@STREAMNO"/></span>
							</xsl:if-->
							&#xa0;Stream&#xa0;<xsl:value-of select="@STREAMNO"/>
						</th>
					</xsl:if>
			</xsl:for-each>
			
			<!-- Amount value by exchange type -->
			<xsl:variable name="vMoney" select="EARMONEY"/>			
			<xsl:variable name="vMoneyFCUExists" select="EARMONEY[@EXCHANGETYPE='FCU']"/>		
			<xsl:if test="(count($vMoney) > 0) and ($vMoneyFCUExists)">			
				<xsl:for-each select="$vExchangeTypeEnum">
					<xsl:sort select="@ORDER" data-type="number"/>
					<xsl:variable name="vExchangeTypeEnumCode"><xsl:value-of select="@EXCHANGETYPE"/></xsl:variable>
					
					<xsl:if test="($pWithAllExchange!=0 or $vExchangeTypeEnumCode='FCU' or $vExchangeTypeEnumCode='ACU_EVENTDATE')">
						<xsl:variable name="vCurrentExchangeType" select="$vMoney[@EXCHANGETYPE=$vExchangeTypeEnumCode]"/>
						<xsl:choose>
								<xsl:when test="$vCurrentExchangeType">						
									<xsl:variable name="vResult">
										<xsl:call-template name="GetNominalMoneyByExchangeType">
											<xsl:with-param name="pMoney"               select="$vMoney"/>
											<xsl:with-param name="pCurrentExchangeType" select="$vCurrentExchangeType/@EXCHANGETYPE"/>
										</xsl:call-template>
									</xsl:variable> 
									<xsl:choose>
										<xsl:when test ="$vResult>0">
											<td nowrap="1" class="Ear_Notional" align="right" colspan="{($vSideEnumCount*2)-1}" width="{($vSideEnumCount*$vAmountWidth)+(($vSideEnumCount - 1)*$vCurrencyWidth)}px;">
												<xsl:value-of select="format-number($vMoney[position()=$vResult],$amountPattern,$pCurrentCulture)"/>
											</td>
											<td nowrap="1" class="Ear_Notional" align="center" width="{$vCurrencyWidth}px;"><xsl:value-of select="$vMoney[position()=$vResult]/@CURRENCY"/></td>
										</xsl:when>
										<xsl:when test ="$vCurrentExchangeType/@EXCHANGETYPE='ACU_EARDATE'">
											<xsl:call-template name="EarNominalAmountToday">
												<xsl:with-param name="pMoney"               select="$vMoney"/>
												<xsl:with-param name="pCurrentExchangeType" select="'ACU_TODAY'"/>
												<xsl:with-param name="pamountPattern"       select="$amountPattern"/>
												<xsl:with-param name="pCurrentCulture"      select="$pCurrentCulture"/>
											</xsl:call-template>							
										</xsl:when>
										<xsl:when test ="$vCurrentExchangeType/@EXCHANGETYPE='CU1_EARDATE'">
											<xsl:call-template name="EarNominalAmountToday">
												<xsl:with-param name="pMoney"               select="$vMoney"/>
												<xsl:with-param name="pCurrentExchangeType" select="'CU1_TODAY'"/>
												<xsl:with-param name="pamountPattern"       select="$amountPattern"/>
												<xsl:with-param name="pCurrentCulture"      select="$pCurrentCulture"/>
											</xsl:call-template>							
										</xsl:when>
										<xsl:when test ="$vCurrentExchangeType/@EXCHANGETYPE='CU2_EARDATE'">
											<xsl:call-template name="EarNominalAmountToday">
												<xsl:with-param name="pMoney"               select="$vMoney"/>
												<xsl:with-param name="pCurrentExchangeType" select="'CU2_TODAY'"/>
												<xsl:with-param name="pamountPattern"       select="$amountPattern"/>
												<xsl:with-param name="pCurrentCulture"      select="$pCurrentCulture"/>
											</xsl:call-template>							
										</xsl:when>
										<xsl:otherwise>
											<td nowrap="1" class="Ear_Notional" colspan="{($vSideEnumCount*2)-1}" width="{($vSideEnumCount*$vAmountWidth)+(($vSideEnumCount - 1)*$vCurrencyWidth)}px;" align="center">N/A</td>
											<td nowrap="1" class="Ear_Notional" width="{$vCurrencyWidth}px;">&#xa0;</td>
											</xsl:otherwise>
									</xsl:choose>
								</xsl:when>
								<xsl:otherwise>
									<td nowrap="1" class="Ear_Notional" colspan="{($vSideEnumCount*2)-1}" width="{($vSideEnumCount*$vAmountWidth)+(($vSideEnumCount - 1)*$vCurrencyWidth)}px;" align="center">N/A</td>
									<td nowrap="1" class="Ear_Notional" width="{$vCurrencyWidth}px;">&#xa0;</td>
								</xsl:otherwise>
							</xsl:choose>
					</xsl:if>
				</xsl:for-each>
			</xsl:if>
		</tr>
	</xsl:template>	
	
	<!-- Display no Nominal Amounts -->
	<xsl:template match="EARAMOUNT">
			
		<xsl:variable name="vIDEAR"><xsl:value-of select="../../../../@IDEAR"/></xsl:variable>
		<xsl:variable name="vMoney" select="EARMONEY"/>
		<xsl:variable name="vMoneyFCUExists" select="EARMONEY[@EXCHANGETYPE='FCU']"/>
		
		<xsl:variable name="vCurentEAR_v1.1.9" select="../../EARCLASS[@TYPE]"/>
		
		<xsl:if test="(count($vMoney) > 0) and ($vMoneyFCUExists)">			
			<tr id="{$vIDEAR}" style="display:none">	
				<xsl:variable name="vEarCode"	  select="../@CODE"/>
				<xsl:variable name="vCurrentSide">
					<xsl:choose>
						<!-- For resumption of old version before v1.1.9.0-->
						<xsl:when test="count($vCurentEAR_v1.1.9) > 0"><xsl:value-of select="@FLOWTYPE"/></xsl:when>
						<xsl:otherwise><xsl:value-of select="@SIDE"/></xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				<xsl:variable name="vIsEarCommon">
					<xsl:choose>
						<xsl:when test ="(@IDEARCOMMON > 0 )"><xsl:value-of select="1"/></xsl:when>
						<xsl:otherwise><xsl:value-of select="0"/></xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				<xsl:variable name="vIsEarCalc">
					<xsl:choose>
						<xsl:when test ="(@IDEARCALC > 0 )"><xsl:value-of select="1"/></xsl:when>
						<xsl:otherwise><xsl:value-of select="0"/></xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				
				<!-- AmountType -->
				<xsl:variable name="vClassAmount">
					<xsl:choose>
						<xsl:when test ="($vCurrentSide = 'PAY') or ($vCurrentSide = 'PAID')">
							<xsl:choose>
								<xsl:when test ="$vIsEarCommon=1 or $vIsEarCalc=1"><xsl:value-of select="'Ear_OtherPaid'"/></xsl:when>
								<xsl:otherwise><xsl:value-of select="'Ear_Paid'"/></xsl:otherwise>
							</xsl:choose>
						</xsl:when>
						<xsl:when test ="($vCurrentSide = 'REC') or ($vCurrentSide = 'RECEIVED')">
							<xsl:choose>
								<xsl:when test ="$vIsEarCommon=1 or $vIsEarCalc=1"><xsl:value-of select="'Ear_OtherReceived'"/></xsl:when>
								<xsl:otherwise><xsl:value-of select="'Ear_Received'"/></xsl:otherwise>
							</xsl:choose>						
						</xsl:when>
						<xsl:otherwise><xsl:value-of select="'DataGrid_ItemStyle'"/></xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				<td class="{$vClassAmount}" colspan="3" nowrap="1">					
					<xsl:choose>
						<xsl:when test="@IDEARCOMMON">
							<span style="color:DarkGray"><xsl:value-of select="'Common'"/></span>
							<xsl:if test="$pWithLabel!=0">
								<span class="SmallerCode" style="color:DarkGray">(id: <xsl:value-of select="@IDEARCOMMON"/>)</span>
							</xsl:if>
						</xsl:when>
						<xsl:when test="@IDEARCALC">
							<span style="color:DarkGray"><xsl:value-of select="'Calc'"/></span>
							<xsl:if test="$pWithLabel!=0">
								<span class="SmallerCode" style="color:DarkGray">(id: <xsl:value-of select="@IDEARCALC"/>)</span>
							</xsl:if>
						</xsl:when>
						<xsl:when test="@IDEARDAY">
							<span style="color:DarkGray"><xsl:value-of select="'Day'"/></span>
							<xsl:if test="$pWithLabel!=0">
								<span class="SmallerCode" style="color:DarkGray">(id: <xsl:value-of select="@IDEARDAY"/>)</span>
							</xsl:if>	
						</xsl:when>						
					</xsl:choose>
					&#xa0;<xsl:value-of select="$vEarCode"/>
					<xsl:if test="$pWithLabel!=0">
						<span class="SmallerCode">(<xsl:value-of select="$vEarEnumRes/EventCodeType[@code=$vEarCode]/Description[@resource=$pCurrentCulture]"/>)</span>			
					</xsl:if>&#xa0;
					<xsl:variable name="vInfoBulle">
						<xsl:choose>
							<xsl:when test="$vIsEarCalc=1 and @AGFUNC"><xsl:value-of select="@AGFUNC"/>(<xsl:value-of select="@AGAMOUNTS"/>)</xsl:when>
							<xsl:otherwise></xsl:otherwise>
						</xsl:choose>
					</xsl:variable>
					<span title="{$vInfoBulle}"><xsl:value-of select="@TYPE"/></span>
					<xsl:if test="$pWithLabel!=0">
						<span class="SmallerCode">(<xsl:value-of select="$vEarEnumRes/AmountType[@code=current()/@TYPE]/Description[@resource=$pCurrentCulture]"/>)</span>
					</xsl:if>		
				</td>				
			
				<!-- Amount value by exchange type -->				
				<xsl:for-each select="$vExchangeTypeEnum">
					<xsl:sort select="@ORDER" data-type="number"/>
					<xsl:variable name="vExchangeTypeEnumCode"><xsl:value-of select="@EXCHANGETYPE"/></xsl:variable>
					
					<xsl:if test="($pWithAllExchange!=0 or $vExchangeTypeEnumCode='FCU' or $vExchangeTypeEnumCode='ACU_EVENTDATE')">
						<xsl:for-each select="$vSideEnum">
							<xsl:sort select="@ORDER" data-type="number"/>		
							<xsl:variable name="vSideClassName">
								<xsl:choose>
									<xsl:when test ="(@SIDE = 'PAY') or (@SIDE = 'PAID')">
										<xsl:choose>
											<xsl:when test ="$vIsEarCommon=1 or $vIsEarCalc=1"><xsl:value-of select="'Ear_OtherPaid'"/></xsl:when>
											<xsl:otherwise><xsl:value-of select="'Ear_Paid'"/></xsl:otherwise>
										</xsl:choose>									
									</xsl:when>
									<xsl:when test ="(@SIDE = 'REC') or (@SIDE = 'RECEIVED')">
										<xsl:choose>
											<xsl:when test ="$vIsEarCommon=1 or $vIsEarCalc=1"><xsl:value-of select="'Ear_OtherReceived'"/></xsl:when>
											<xsl:otherwise><xsl:value-of select="'Ear_Received'"/></xsl:otherwise>
										</xsl:choose>										
									</xsl:when>
									<xsl:otherwise><xsl:value-of select="'DataGrid_ItemStyle'"/></xsl:otherwise>
								</xsl:choose>
							</xsl:variable>
							<xsl:variable name="vCurrentExchangeType" select="$vMoney[@EXCHANGETYPE=$vExchangeTypeEnumCode]"/>
							<xsl:choose>	
								<xsl:when test="$vCurrentExchangeType">
									<xsl:choose>	
										<xsl:when test="@SIDE=$vCurrentSide">
										
											<xsl:variable name="vResult">
												<xsl:choose>
  													<!-- For rising compatibility with version before v1.1.9.0-->
													<xsl:when test="count($vCurentEAR_v1.1.9) > 0">
														<xsl:call-template name="GetMoneyByExchangeTypeAndSide_v1.1.9">
															<xsl:with-param name="pMoney"               select="$vMoney"/>
															<xsl:with-param name="pCurrentExchangeType" select="$vCurrentExchangeType/@EXCHANGETYPE"/>
															<xsl:with-param name="pCurrentSide"         select="@SIDE"/>
														</xsl:call-template>
  													</xsl:when>
													<xsl:otherwise>
														<xsl:call-template name="GetMoneyByExchangeTypeAndSide">
															<xsl:with-param name="pMoney"               select="$vMoney"/>
															<xsl:with-param name="pCurrentExchangeType" select="$vCurrentExchangeType/@EXCHANGETYPE"/>
															<xsl:with-param name="pCurrentSide"         select="@SIDE"/>
														</xsl:call-template>
													</xsl:otherwise>
												</xsl:choose>												
											</xsl:variable>	
																
											<xsl:choose>
												<xsl:when test ="$vResult>0">
													<td nowrap="1" class="{$vSideClassName}" align="right"  width="{$vAmountWidth}px;">
														<xsl:value-of select="format-number($vMoney[position()=$vResult],$amountPattern,$pCurrentCulture)"/>
													</td>
													<td nowrap="1" class="{$vSideClassName}" align="center" width="{$vCurrencyWidth}px;"><xsl:value-of select="$vMoney[position()=$vResult]/@CURRENCY"/></td>
												</xsl:when>
												<xsl:when test ="$vCurrentExchangeType/@EXCHANGETYPE='ACU_EARDATE'">
													<xsl:call-template name="EarAmountToday">
														<xsl:with-param name="pMoney"               select="$vMoney"/>
														<xsl:with-param name="pCurrentExchangeType" select="'ACU_TODAY'"/>
														<xsl:with-param name="pCurrentSide"         select="@SIDE"/>
														<xsl:with-param name="pSideClassName"		select="$vSideClassName"/>
														<xsl:with-param name="pamountPattern"       select="$amountPattern"/>
														<xsl:with-param name="pCurrentCulture"      select="$pCurrentCulture"/>
													</xsl:call-template>							
												</xsl:when>
												<xsl:when test ="$vCurrentExchangeType/@EXCHANGETYPE='CU1_EARDATE'">
													<xsl:call-template name="EarAmountToday">
														<xsl:with-param name="pMoney"               select="$vMoney"/>
														<xsl:with-param name="pCurrentExchangeType" select="'CU1_TODAY'"/>
														<xsl:with-param name="pCurrentSide"			select="@SIDE"/>
														<xsl:with-param name="pSideClassName"		select="$vSideClassName"/>
														<xsl:with-param name="pamountPattern"       select="$amountPattern"/>
														<xsl:with-param name="pCurrentCulture"      select="$pCurrentCulture"/>
													</xsl:call-template>							
												</xsl:when>
												<xsl:when test ="$vCurrentExchangeType/@EXCHANGETYPE='CU2_EARDATE'">
													<xsl:call-template name="EarAmountToday">
														<xsl:with-param name="pMoney"               select="$vMoney"/>
														<xsl:with-param name="pCurrentExchangeType" select="'CU2_TODAY'"/>
														<xsl:with-param name="pCurrentSide"			select="@SIDE"/>
														<xsl:with-param name="pSideClassName"		select="$vSideClassName"/>
														<xsl:with-param name="pamountPattern"       select="$amountPattern"/>
														<xsl:with-param name="pCurrentCulture"      select="$pCurrentCulture"/>
													</xsl:call-template>							
												</xsl:when>
												<xsl:otherwise>
													<td nowrap="1" class="{$vSideClassName}" align="center" width="{$vAmountWidth}px;">N/A</td>
													<td nowrap="1" class="{$vSideClassName}" width="{$vCurrencyWidth}px;">&#xa0;</td>
												</xsl:otherwise>
											</xsl:choose>
										</xsl:when>
										<xsl:otherwise>
											<td nowrap="1" class="{$vSideClassName}" width="{$vAmountWidth}px;">&#xa0;</td>
											<td nowrap="1" class="{$vSideClassName}" width="{$vCurrencyWidth}px;">&#xa0;</td>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:when>
								<xsl:otherwise>
									<xsl:choose>	
										<xsl:when test="@SIDE=$vCurrentSide">
											<td nowrap="1" class="{$vSideClassName}" align="center" width="{$vAmountWidth}px;">N/A</td>
											<td nowrap="1" class="{$vSideClassName}" width="{$vCurrencyWidth}px;">&#xa0;</td>
										</xsl:when>
										<xsl:otherwise>
											<td nowrap="1" class="{$vSideClassName}" width="{$vAmountWidth}px;">&#xa0;</td>
											<td nowrap="1" class="{$vSideClassName}" width="{$vCurrencyWidth}px;">&#xa0;</td>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:for-each>
					</xsl:if>
				</xsl:for-each>
			</tr>		
		</xsl:if>
	</xsl:template>
	
	<!-- Display ACUToday amount -->
	<xsl:template name="EarAmountToday">
		<xsl:param name="pMoney"/>
		<xsl:param name="pCurrentExchangeType"/>
		<xsl:param name="pCurrentSide"/>
		<xsl:param name="pSideClassName"/>
		<xsl:param name="pamountPattern"/>
		<xsl:param name="pCurrentCulture"/>	
		<xsl:variable name="vResult2">
			<xsl:call-template name="GetMoneyByExchangeTypeAndSide">
				<xsl:with-param name="pMoney"               select="$pMoney"/>
				<xsl:with-param name="pCurrentExchangeType" select="$pCurrentExchangeType"/>
				<xsl:with-param name="pCurrentSide"     select="$pCurrentSide"/>
			</xsl:call-template>
		</xsl:variable> 
		<xsl:choose>
			<xsl:when test ="$vResult2>0">
				<td nowrap="1" class="{$pSideClassName}" align="right"  width="{$vAmountWidth}px;">
					<xsl:value-of select="format-number($pMoney[position()=$vResult2],$pamountPattern,$pCurrentCulture)"/>
				</td>
				<td nowrap="1" class="{$pSideClassName}" align="center" width="{$vCurrencyWidth}px;"><xsl:value-of select="$pMoney[position()=$vResult2]/@CURRENCY"/></td>
			</xsl:when>
			<xsl:otherwise>
				<td nowrap="1" class="{$pSideClassName}" align="center" width="{$vAmountWidth}px;">N/A</td>
				<td nowrap="1" class="{$pSideClassName}" width="{$vCurrencyWidth}px;">&#xa0;</td>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>	
	
	<!-- Display ACUToday Nominal amount -->
	<xsl:template name="EarNominalAmountToday">
		<xsl:param name="pMoney"/>
		<xsl:param name="pCurrentExchangeType"/>
		<xsl:param name="pamountPattern"/>
		<xsl:param name="pCurrentCulture"/>	
		<xsl:variable name="vResult2">
			<xsl:call-template name="GetNominalMoneyByExchangeType">
				<xsl:with-param name="pMoney"               select="$pMoney"/>
				<xsl:with-param name="pCurrentExchangeType" select="$pCurrentExchangeType"/>
			</xsl:call-template>
		</xsl:variable> 
		<xsl:choose>
			<xsl:when test ="$vResult2>0">
				<td nowrap="1" class="Ear_Notional" align="right" colspan="{($vSideEnumCount*2)-1}" width="{($vSideEnumCount*$vAmountWidth)+(($vSideEnumCount - 1)*$vCurrencyWidth)}px;">
					<xsl:value-of select="format-number($pMoney[position()=$vResult2],$pamountPattern,$pCurrentCulture)"/>
				</td>
				<td nowrap="1" class="Ear_Notional" align="center" width="{$vCurrencyWidth}px;"><xsl:value-of select="$pMoney[position()=$vResult2]/@CURRENCY"/></td>
			</xsl:when>
			<xsl:otherwise>
				<td nowrap="1" class="Ear_Notional" colspan="{($vSideEnumCount*2)-1}" width="{($vSideEnumCount*$vAmountWidth)+(($vSideEnumCount - 1)*$vCurrencyWidth)}px;" align="center">N/A</td>
				<td nowrap="1" class="Ear_Notional" width="{$vCurrencyWidth}px;">&#xa0;</td>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>	
</xsl:stylesheet>