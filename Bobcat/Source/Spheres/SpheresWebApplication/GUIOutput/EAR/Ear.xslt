<?xml version="1.0" encoding="UTF-8" ?>

<!--RD 20121002 Trier les EAR par Book et Date Event -->
<!--RD 20121026 [18201] Gestion du statut d'activation d'un EAR (affichage d'une image) -->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" version="1.0">
  
 
  <xsl:output omit-xml-declaration="yes" method="xml" indent="yes" encoding="UTF-8"/>
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
    <xsl:text>..\..\Resource\EarEnumsResource.xml</xsl:text>
  </xsl:variable>

  <xsl:variable name="vEarEnumRes" select="document($varEarEnumFileResource)//EarEnums"/>

  <!-- Ear enums file -->
  <xsl:variable name="varEnumsFile">
    <xsl:text>EarEnums.xml</xsl:text>
  </xsl:variable>

  <xsl:variable name="vExchangeTypeEnum"  select="document($varEnumsFile)//EarEnums//ExchangeType"/>
  <xsl:variable name="vSideEnum"			    select="document($varEnumsFile)//EarEnums//Side"/>

  <xsl:variable name="vExchangeTypeEnumCount" select="count($vExchangeTypeEnum)"/>
  <xsl:variable name="vSideEnumCount"     select="count($vSideEnum)"/>

  <!-- Keys -->
  <xsl:key      name="kEarDet"            match="EARDET"    use="@IDEAR"/>
  <xsl:key      name="kEarDetInstr"       match="EARDET"    use="@INSTR_IDENTIFIER"/>
  <xsl:key      name="kEventClass"        match="EARCLASS"  use="@CLASS"/>

  <xsl:key      name="kInstrumentNO"      match="EARCLASS"  use="@INSTRUMENTNO"/>
  <xsl:key      name="kStreamNO"          match="EARCLASS"  use="@STREAMNO"/>

  <!-- Global variables -->
  <xsl:variable name="vEarEventClass"     select="*/EARBOOK"/>

  <xsl:variable name="vAllEventClass"     select="$vEarEventClass/EARCLASS[generate-id()=generate-id(key('kEventClass',@CLASS)) 
														and ($pWithAllClosing!=0 or (@CLASS!='LIN' and @CLASS!='CMP'))]"/>

  <xsl:variable name="vAllInstrumentNO"   select="$vEarEventClass/EARCLASS[generate-id()=generate-id(key('kInstrumentNO',@INSTRUMENTNO))]"/>
  <xsl:variable name="vAllStreamNO"       select="$vEarEventClass/EARCLASS[generate-id()=generate-id(key('kStreamNO',@STREAMNO))]"/>

  <xsl:variable name="vEarNomAmount"      select="*/EARBOOK/EARCLASS/EARNOMAMOUNT"/>
  <xsl:variable name="vAllInstrIdent"     select="*/EARBOOK/EARDET[generate-id()=generate-id(key('kEarDetInstr',@INSTR_IDENTIFIER))]"/>

  <xsl:variable name="vNbEar"              select="count(*/EARBOOK)"/>

  <!-- Width Varibles -->
  <xsl:variable name="vWidthUnit"          select="px"/>
  <xsl:variable name="vAmountWidth"        select="110"/>
  <xsl:variable name="vCurrencyWidth"      select="35"/>

  <xsl:template match="/">
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
    <table style="width:100%" border="0" cellspacing="0" cellpadding="0">
      <!-- Trier d'abord les EAR par Book et Date Event-->
      <xsl:variable name="vEARBOOK">
        <xsl:for-each select="EARBOOK">
          <xsl:sort select="@BOOK_IDENTIFIER" data-type="text"/>
          <xsl:sort select="@DTEVENT" data-type="text"/>
          <xsl:copy-of select="."/>
        </xsl:for-each>
      </xsl:variable>

      <xsl:for-each select="msxsl:node-set($vEARBOOK)/EARBOOK">

        <xsl:variable name="vCurrentIDB">
          <xsl:value-of select="@IDB"/>
        </xsl:variable>

        <xsl:if test="((count(EARCLASS/EARDAY)>0) or (count(EARCLASS/EARCOMMON[$pWithAllFlows!=0])>0) or (count(EARCLASS/EARCALC[$pWithAllFlows!=0])>0) )">

          <xsl:variable name="vPreviousIDB" select="preceding::EARBOOK[1]/@IDB"/>
          <xsl:variable name="vBookHeaderToDisplay">
            <xsl:choose>
              <xsl:when test="$vPreviousIDB">
                <xsl:choose>
                  <xsl:when test="$vPreviousIDB = @IDB">0</xsl:when>
                  <xsl:otherwise>1</xsl:otherwise>
                </xsl:choose>
              </xsl:when>
              <xsl:otherwise>1</xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:if test="$vBookHeaderToDisplay =1">
            <tr>
              <td colspan="3">&#xa0;</td>
            </tr>
            <tr>
              <td colspan="3">
                <table class="EarGrid" border="0" cellspacing="0" cellpadding="0" rules="all">
                  <tr class="DataGrid_HeaderStyle">
                    <td style="white-space:nowrap;width:20px;">
                      <div id="EARBOOK_{$vCurrentIDB}" class="fa-icon fa fa-plus-square" onclick="javascript:EAR_ExpandCollapse(this);"/>
                    </td>
                    <td class="Header_EarSubTitleLeft" width="150px;">
                      <xsl:value-of select="$varResource[@name='Book']/value" />
                    </td>
                    <td class="Header_EarTitleLeft">
                      <xsl:value-of select="@BOOK_IDENTIFIER"/>
                      <xsl:if test="$pWithLabel!=0">
                        <span class="SmallerCode">
                          (id:<xsl:value-of select="$vCurrentIDB"/>)
                        </span>
                      </xsl:if>
                    </td>
                  </tr>
                  <tr class="DataGrid_ItemStyle">
                    <td class="Header_EarSubTitleLeft" colspan="2" style="width:150px;">
                      <xsl:value-of select="$varResource[@name='IDA_ENTITY_']/value" />
                    </td>
                    <td class="Header_EarTitleLeft">
                      <xsl:value-of select="@ENTITY_IDENTIFIER"/>
                      <xsl:if test="$pWithLabel!=0">
                        <span class="SmallerCode">
                          (id:<xsl:value-of select="@IDA_ENTITY"/>)
                        </span>
                      </xsl:if>
                    </td>
                  </tr>
                  <tr class="DataGrid_ItemStyle">
                    <td class="Header_EarSubTitleLeft" colspan="2" style="width:150px;">
                      <xsl:value-of select="$varResource[@name='Trade']/value" />
                    </td>
                    <td class="Header_EarTitleLeft">
                      <xsl:value-of select="@TRADE_IDENTIFIER"/>
                      <xsl:if test="$pWithLabel!=0">
                        <span class="SmallerCode">
                          (id:<xsl:value-of select="@IDT"/>)
                        </span>
                      </xsl:if>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
            <tr>
              <td colspan="3">&#xa0;</td>
            </tr>
          </xsl:if>

          <!-- Display EarClass-->
          <xsl:variable name="vIDEAR">
            <xsl:value-of select="@IDEAR"/>
          </xsl:variable>
          <xsl:variable name="vPreviousEAR">
            <xsl:value-of select="preceding::EARBOOK[1]/@IDEAR"/>
          </xsl:variable>
          <xsl:variable name="vFollowEAR" select="following::EARBOOK[1]/@IDEAR"/>
          <tr>
            <td colspan="3">
              <a name="POS_{$vIDEAR}"></a>
            </td>
          </tr>
          <tr>
            <td style="width:10px;"></td>
            <td>
              <table id="EARBOOK_{$vCurrentIDB}_{$vIDEAR}" class="EarGrid" border="0" cellspacing="0" cellpadding="1" rules="all" style="background-color:Transparent;width:100%;border:outset 1pt groove;border-collapse:collapse;display:none">
                <thead>
                  <tr class="earitemstyle">
                    <td style="white-space:nowrap;" colspan="3">
                      <div id="EAR_{$vIDEAR}" class="fa-icon fa fa-plus-square" onclick="javascript:EAR_ExpandCollapse(this);">&#xa0;</div>
                      <div class="eardetail">
                        <span style="font-weight:bolder;">
                          <xsl:call-template name="format-shortdate">
                            <xsl:with-param name="xsd-date-time" select="@DTEVENT"/>
                          </xsl:call-template>
                        </span>
                        <xsl:if test="$pWithLabel!=0">
                          <span>(EVENT)</span>
                        </xsl:if>
                      </div>
                      <div class="eardetail">
                        <span>
                          <xsl:call-template name="format-shortdate">
                            <xsl:with-param name="xsd-date-time" select="@DTEAR"/>
                          </xsl:call-template>
                        </span>
                        <xsl:if test="$pWithLabel!=0">
                          <span>(EAR id: <xsl:value-of select="$vIDEAR"/>)</span>
                        </xsl:if>
                      </div>
                      <div class="eardetail">
                        <xsl:choose>
                          <xsl:when test="@IDSTACTIVATION='DEACTIV'">
                            <xsl:variable name="vTitle">
                              <xsl:call-template name="getSpheresTranslation">
                                <xsl:with-param name="pResourceName" select="'EARStatus_Deactiv'" />
                              </xsl:call-template>
                            </xsl:variable>
                            <div class="deactiv" title="{$vTitle}">&#xa0;</div>
                          </xsl:when>
                          <xsl:when test="@IDSTACTIVATION='REMOVED'">
                            <xsl:variable name="vTitle">
                              <xsl:call-template name="getSpheresTranslation">
                                <xsl:with-param name="pResourceName" select="'EARStatus_Removed'" />
                              </xsl:call-template>
                            </xsl:variable>
                            <div class="removed" title="{$vTitle}">&#xa0;</div>
                          </xsl:when>
                          <xsl:otherwise>&#xa0;</xsl:otherwise>
                        </xsl:choose>
                      </div>
                      <div class="eartb">
                        <xsl:if test="$vPreviousEAR>0">
                          <a class="fa-icon fa fa-chevron-circle-left" href="#POS_{$vPreviousEAR}">&#xa0;</a>
                        </xsl:if>
                        <xsl:if test="$vFollowEAR>0">
                          <a class="fa-icon fa fa-chevron-circle-right" href="#POS_{$vFollowEAR}">&#xa0;</a>
                        </xsl:if>
                        <a class="fa-icon fa fa-chevron-circle-up" title="top" href="#toppage">&#xa0;</a>
                        <a class="fa-icon fa fa-chevron-circle-down" title="bottom" href="#bottom">&#xa0;</a>
                      </div>
                    </td>
                    
                    <xsl:call-template name="DisplayHeaderExchangeType">
                      <xsl:with-param name="pCurrentEar" select="$vIDEAR"/>
                    </xsl:call-template>
                  </tr>
                </thead>
                <xsl:call-template name="DisplayNominalResult">
                  <xsl:with-param name="pCurrentEar" select="$vIDEAR"/>
                </xsl:call-template>
                <xsl:call-template name="DisplayResult">
                  <xsl:with-param name="pCurrentEar" select="$vIDEAR"/>
                </xsl:call-template>
              </table>
            </td>
            <td style="width:10px;"></td>
          </tr>
        </xsl:if>
      </xsl:for-each>
    </table>
  </xsl:template>

  <!-- EarClass for Nominal only -->
  <xsl:template name="DisplayNominalResult">
    <xsl:param name="pCurrentEar"/>
    <xsl:for-each select="EARCLASS">
      <xsl:sort select="@CODE" data-type="text"/>
      <xsl:sort select="@CLASS" data-type="text"/>

      <xsl:variable name="vPos" select="position()"/>
      <xsl:if test="@CODE='NOS'">
        <xsl:if test="preceding-sibling::EARCLASS[@CODE='NOS']=false()">
          <tr class="earitemstyle" style="display:none">
            <th class="DataGrid_AlternatingItemStyle" colspan="3">
              <xsl:value-of select="$varResource[@name='NOMINAL']/value" />
            </th>
            <xsl:call-template name="DisplayHeaderNominal"/>
          </tr>
        </xsl:if>
        <xsl:apply-templates select="EARNOM"/>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <!-- EarClass for amount excepted Nominal -->
  <xsl:template name="DisplayResult">
    <xsl:param name="pCurrentEar"/>
    <tr class="earitemstyle" style="display:none">
      <th class="DataGrid_AlternatingItemStyle" colspan="3">
        <xsl:value-of select="$varResource[@name='Amounts']/value" />
      </th>
      <xsl:call-template name="DisplayHeaderSide"/>
    </tr>

    <xsl:variable name="vEarClassNotNOS" select="EARCLASS[@CODE!='NOS']"/>

    <xsl:for-each select="$vAllInstrumentNO">
      <xsl:sort select="@INSTRUMENTNO" data-type="number"/>

      <xsl:variable name="vInstrumentNo" select="@INSTRUMENTNO"/>

      <xsl:for-each select="$vAllStreamNO">
        <xsl:sort select="@STREAMNO" data-type="number"/>

        <xsl:variable name="vStreamNo" select="@STREAMNO"/>

        <xsl:call-template name="DisplayEventClass">
          <xsl:with-param name="pCurrentEar" select="$pCurrentEar"/>
          <xsl:with-param name="pEarClassNotNOS" select="$vEarClassNotNOS"/>
          <xsl:with-param name="pAllEventClass" select="$vAllEventClass"/>
          <xsl:with-param name="pInstrumentNo" select="$vInstrumentNo"/>
          <xsl:with-param name="pStreamNo" select="$vStreamNo"/>
        </xsl:call-template>

      </xsl:for-each>
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
      <xsl:if test="( (count($vEarClass/EARDAY) > 0) or (count($vEarClass/EARCOMMON[$pWithAllFlows!=0]) > 0) or (count($vEarClass/EARCALC[$pWithAllFlows!=0]) > 0))">
        <tr class="earitemstyle" style="display:none">
          <xsl:call-template name="DisplayEarKey">
            <xsl:with-param name="pEarClassType" select="$vEarClass/@CLASS"/>
            <xsl:with-param name="pInstrumentNO" select="$vEarClass/@INSTRUMENTNO"/>
            <xsl:with-param name="pStreamNO"     select="$vEarClass/@STREAMNO"/>
          </xsl:call-template>

          <xsl:call-template name="DisplayHeaderSide">
            <xsl:with-param name="pIsFilled" select="number(1)"/>
          </xsl:call-template>
        </tr>

        <xsl:for-each select="$vEarClass">
          <xsl:sort select="@CODE" data-type="text"/>
          <xsl:call-template name="DisplayEarAmount">
            <xsl:with-param name="pEarType" select="EARDAY"/>
          </xsl:call-template>
          <xsl:call-template name="DisplayEarAmount">
            <xsl:with-param name="pEarType" select="EARCOMMON[$pWithAllFlows!=0]"/>
          </xsl:call-template>
          <xsl:call-template name="DisplayEarAmount">
            <xsl:with-param name="pEarType" select="EARCALC[$pWithAllFlows!=0]"/>
          </xsl:call-template>
        </xsl:for-each>

      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <!-- DisplayHeaderExchangeType -->
  <xsl:template name="DisplayHeaderExchangeType">
    <xsl:param name="pCurrentEar"/>
    <xsl:for-each select="$vExchangeTypeEnum">
      <xsl:sort select="@ORDER" data-type="number"/>
      <xsl:variable name="vExchangeTypeEnumCode">
        <xsl:value-of select="@EXCHANGETYPE"/>
      </xsl:variable>

      <xsl:if test="($pWithAllExchange!=0 or $vExchangeTypeEnumCode='FCU' or $vExchangeTypeEnumCode='ACU_EVENTDATE')">
        <th class="earheader" colspan="{$vSideEnumCount*2}" style="display:none">
          <xsl:value-of select="$vExchangeTypeEnumCode"/>
          <xsl:if test="$pWithLabel!=0">
            <span class="SmallerCode">(<xsl:value-of select="$vEarEnumRes/ExchangeType[@code=$vExchangeTypeEnumCode]/Description[@resource=$pCurrentCulture]"/>)</span>
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
      <xsl:variable name="vExchangeTypeEnumCode">
        <xsl:value-of select="@EXCHANGETYPE"/>
      </xsl:variable>
      <xsl:if test="($pWithAllExchange!=0 or $vExchangeTypeEnumCode='FCU' or $vExchangeTypeEnumCode='ACU_EVENTDATE')">
        <xsl:for-each select="$vSideEnum">
          <xsl:sort select="@ORDER" data-type="number"/>
          <xsl:variable name="vSideClassName">
            <xsl:choose>
              <xsl:when test ="$pIsFilled=0">
                <xsl:choose>
                  <xsl:when test ="(@SIDE = 'PAY') or (@SIDE = 'PAID')">
                    <xsl:value-of select="'Ear_HeaderPaid'"/>
                  </xsl:when>
                  <xsl:when test ="(@SIDE = 'REC') or (@SIDE = 'RECEIVED')">
                    <xsl:value-of select="'Ear_HeaderReceived'"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="'xDataGrid_ItemStyle'"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>
              <xsl:otherwise>
                <xsl:choose>
                  <xsl:when test ="(@SIDE = 'PAY') or (@SIDE = 'PAID')">
                    <xsl:value-of select="'Ear_Paid'"/>
                  </xsl:when>
                  <xsl:when test ="(@SIDE = 'REC') or (@SIDE = 'RECEIVED')">
                    <xsl:value-of select="'Ear_Received'"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="'xDataGrid_ItemStyle'"/>
                  </xsl:otherwise>
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
        <xsl:when test ="$pWithAllExchange!=0">
          <xsl:value-of select="$vSideEnumCount*2*$vExchangeTypeEnumCount"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$vSideEnumCount*4"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <th class="Ear_HeaderNotional" colspan="{$vColSpan}">
      <xsl:value-of select="'NOS'"/>
      <xsl:if test="$pWithLabel!=0">
        <span class="SmallerCode">
          (<xsl:value-of select="$vEarEnumRes/EventCodeType[@code='NOS']/Description[@resource=$pCurrentCulture]"/>)
        </span>
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
        <xsl:when test ="@IDEARCALC">
          <xsl:value-of select="'Ear_Key'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'Ear_Key'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- EventClass -->
    <xsl:variable name="vColSpan">
      <xsl:choose>
        <xsl:when test ="$pStreamNO = 0 and $pInstrumentNO = 0">3</xsl:when>
        <xsl:otherwise>2</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <th class="{$vClassByAmountType}" style="width:350px;white-space:nowrap" colspan="{$vColSpan}">
      <span style="white-space:nowrap;">
        <xsl:value-of select="$pEarClassType"/>
      </span>
      <xsl:if test="$pWithLabel!=0">
        <span class="SmallerCode" style="white-space:nowrap;">
          (<xsl:value-of select="$vEarEnumRes/EventClassType[@code=$pEarClassType]/Description[@resource=$pCurrentCulture]"/>)
        </span>
      </xsl:if>
    </th>
    <xsl:if test = "not($pStreamNO = 0 and $pInstrumentNO = 0)">
      <!-- Instrument -->
      <th class="{$vClassByAmountType}" style="width:300px;text-align: center;">
        <!-- Instrument Identifier-->
        <xsl:for-each select="$vAllInstrIdent">
          <xsl:if test = "(@INSTRUMENTNO = $pInstrumentNO)">
            <span style="white-space:nowrap">
              &#xa0;<xsl:value-of select="@INSTR_IDENTIFIER"/>
            </span>
          </xsl:if>
        </xsl:for-each>
        <xsl:choose>
          <xsl:when test = "$pStreamNO = 0">
            <!-- Instrument ID -->
            <xsl:for-each select="$vAllInstrIdent">
              <xsl:if test = "(@INSTRUMENTNO = $pInstrumentNO) and $pWithLabel!=0">
                <span class="SmallerCode" style="white-space:nowrap">
                  (id:<xsl:value-of select="@IDI"/>)
                </span>
              </xsl:if>
            </xsl:for-each>
          </xsl:when>
          <xsl:otherwise>
            <xsl:for-each select="key('kEarDet',./ancestor::EARBOOK/@IDEAR)">
              <xsl:if test = "(@INSTRUMENTNO = $pInstrumentNO) and (@STREAMNO = $pStreamNO)">
                <!-- Instrument ID -->
                <xsl:if test = "$pInstrumentNO != 0 and $pWithLabel!=0">
                  <span class="SmallerCode" style="white-space:nowrap">
                    (id: <xsl:value-of select="@IDI"/>)
                  </span>
                </xsl:if>
                <!-- Stream -->
                <span style="white-space:nowrap">
                  &#xa0;Stream&#xa0;<xsl:value-of select="@STREAMNO"/>
                </span>
              </xsl:if>
            </xsl:for-each>
          </xsl:otherwise>
        </xsl:choose>
      </th>
    </xsl:if>
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
  <xsl:template match="EARNOM">
    <xsl:variable name="vIDEAR">
      <xsl:value-of select="../../@IDEAR"/>
    </xsl:variable>
    <xsl:variable name="vInstrumentNo" select="../@INSTRUMENTNO"/>
    <xsl:variable name="vStreamNo"     select="../@STREAMNO"/>
    <xsl:variable name="vIDEARNOM"     select="@IDEARNOM"/>
    <xsl:variable name="vTYPE"         select="@TYPE"/>
    <xsl:variable name="vClassByAmountType">
      <xsl:choose>
        <xsl:when test ="@IDEARCALC">
          <xsl:value-of select="'Ear_Key'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="'Ear_Key'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <tr style="display:none">
      <!-- Instrument -->
      <xsl:for-each select="key('kEarDet',./ancestor::EARBOOK/@IDEAR)">
        <xsl:if test = "(@INSTRUMENTNO = $vInstrumentNo) and (@STREAMNO = $vStreamNo)">
          <!-- Instrument -->
          <th class="{$vClassByAmountType}" colspan="3">
            <xsl:choose>
              <xsl:when test="$vIDEARNOM">
                <span style="color:#fff">
                  <xsl:value-of select="'Nom'"/>
                </span>
                <xsl:if test="$pWithLabel!=0">
                  <span class="SmallerCode" style="color:#fff">
                    (id: <xsl:value-of select="$vIDEARNOM"/>)
                  </span>
                </xsl:if>
              </xsl:when>
            </xsl:choose>&#xa0;
            <xsl:value-of select="@INSTR_IDENTIFIER"/>
            <xsl:if test = "$vInstrumentNo != 0 and $pWithLabel!=0">
              <span class="SmallerCode" >
                (id:<xsl:value-of select="@IDI"/>)
              </span>
            </xsl:if>
            <xsl:if test = "$vTYPE = 'CCU' or  $vTYPE = 'PCU'">
              &#xa0;<xsl:value-of select="$vTYPE"/>
              <xsl:if test="$pWithLabel!=0">
                <span class="SmallerCode">
                  (<xsl:value-of select="$vEarEnumRes/AmountType[@code=$vTYPE]/Description[@resource=$pCurrentCulture]"/>)
                </span>
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
      <xsl:variable name="vMoney" select="EARNOMAMOUNT"/>
      <xsl:variable name="vMoneyFCUExists" select="EARNOMAMOUNT[@EXCHANGETYPE='FCU']"/>
      <xsl:if test="(count($vMoney) > 0) and ($vMoneyFCUExists)">
        <xsl:for-each select="$vExchangeTypeEnum">
          <xsl:sort select="@ORDER" data-type="number"/>
          <xsl:variable name="vExchangeTypeEnumCode">
            <xsl:value-of select="@EXCHANGETYPE"/>
          </xsl:variable>

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
                    <td class="Ear_Notional" colspan="{($vSideEnumCount*2)-1}" style="text-align:right;width:{($vSideEnumCount*$vAmountWidth)+(($vSideEnumCount - 1)*$vCurrencyWidth)}px;">
                      <span style="white-space:nowrap" >
                        <xsl:value-of select="format-number($vMoney[position()=$vResult]/AMOUNT,$amountPattern,$defaultCulture)"/>
                      </span>
                    </td>
                    <td class="Ear_Notional" style="text-align:center;width:{$vCurrencyWidth}px;">
                      <span style="white-space:nowrap" >
                        <xsl:value-of select="$vMoney[position()=$vResult]/@CURRENCY"/>
                      </span>
                    </td>
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
                    <td nowrap="true" class="Ear_Notional" colspan="{($vSideEnumCount*2)-1}" style="text-align:center;width:{($vSideEnumCount*$vAmountWidth)+(($vSideEnumCount - 1)*$vCurrencyWidth)}px;">
                      <span style="white-space:nowrap">N/A</span>
                    </td>
                    <td class="Ear_Notional" style="width:{$vCurrencyWidth}px;">
                      <span style="white-space:nowrap">&#xa0;</span>
                    </td>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>
              <xsl:otherwise>
                <td class="Ear_Notional" colspan="{($vSideEnumCount*2)-1}" style="text-align:center;width:{($vSideEnumCount*$vAmountWidth)+(($vSideEnumCount - 1)*$vCurrencyWidth)}px;">
                  <span style="white-space:nowrap">N/A</span>
                </td>
                <td class="Ear_Notional" width="{$vCurrencyWidth}px;">
                  <span style="white-space:nowrap">&#xa0;</span>
                </td>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:if>
        </xsl:for-each>
      </xsl:if>
    </tr>
  </xsl:template>

  <!-- Display no Nominal Amounts -->
  <xsl:template name="DisplayEarAmount">
    <xsl:param name="pEarType"/>
    <xsl:for-each select="$pEarType">
      <xsl:sort select="@SIDE" data-type="text"/>
      <xsl:sort select="@TYPE" data-type="text"/>

      <xsl:variable name="vIDEAR">
        <xsl:value-of select="../../@IDEAR"/>
      </xsl:variable>
      <xsl:variable name="vEarDayAmount" select="EARDAYAMOUNT"/>
      <xsl:variable name="vEarCommonAmount" select="EARCOMMONAMOUNT"/>
      <xsl:variable name="vEarCalcAmount" select="EARCALCAMOUNT"/>
      <xsl:variable name="vEarDayAmountFCUExists" select="EARDAYAMOUNT[@EXCHANGETYPE='FCU']"/>
      <xsl:variable name="vEarCommonAmountFCUExists" select="EARCOMMONAMOUNT[@EXCHANGETYPE='FCU']"/>
      <xsl:variable name="vEarCalcAmountFCUExists" select="EARCALCAMOUNT[@EXCHANGETYPE='FCU']"/>
      <xsl:variable name="vIsEarDayOk" select="(count($vEarDayAmount) > 0) and $vEarDayAmountFCUExists"/>
      <xsl:variable name="vIsEarCommonOk" select="(count($vEarCommonAmount) > 0) and $vEarCommonAmountFCUExists"/>
      <xsl:variable name="vIsEarCalcOk" select="(count($vEarCalcAmount) > 0) and $vEarCalcAmountFCUExists"/>
      <xsl:if test="($vIsEarDayOk or $vIsEarCommonOk or $vIsEarCalcOk)">
        <tr style="display:none">
          <xsl:variable name="vEarCode"	  select="../@CODE"/>
          <xsl:variable name="vCurrentSide">
            <xsl:value-of select="@SIDE"/>
          </xsl:variable>
          <xsl:variable name="vIsEarCommon">
            <xsl:choose>
              <xsl:when test ="(@IDEARCOMMON > 0 )">
                <xsl:value-of select="1"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="0"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:variable name="vIsEarCalc">
            <xsl:choose>
              <xsl:when test ="(@IDEARCALC > 0 )">
                <xsl:value-of select="1"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="0"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <!-- AmountType -->
          <xsl:variable name="vClassAmount">
            <xsl:choose>
              <xsl:when test ="($vCurrentSide = 'PAY')">
                <xsl:choose>
                  <xsl:when test ="$vIsEarCommon > 0 or $vIsEarCalc > 0">
                    <xsl:value-of select="'Ear_OtherPaid'"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="'Ear_Paid'"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>
              <xsl:when test ="($vCurrentSide = 'REC')">
                <xsl:choose>
                  <xsl:when test ="$vIsEarCommon > 0 or $vIsEarCalc > 0">
                    <xsl:value-of select="'Ear_OtherReceived'"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="'Ear_Received'"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="'xDataGrid_ItemStyle'"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <td class="{$vClassAmount}" colspan="3" style="white-space:nowrap">
            <xsl:choose>
              <xsl:when test="$vIsEarCommon > 0">
                <span style="color:#fff">
                  <xsl:value-of select="'Common'"/>
                </span>
                <xsl:if test="$pWithLabel!=0">
                  <span class="SmallerCode" style="color:#fff">
                    (id: <xsl:value-of select="@IDEARCOMMON"/>)
                  </span>
                </xsl:if>
              </xsl:when>
              <xsl:when test="$vIsEarCalc > 0">
                <span style="color:#fff">
                  <xsl:value-of select="'Calc'"/>
                </span>
                <xsl:if test="$pWithLabel!=0">
                  <span class="SmallerCode" style="color:#fff">
                    (id: <xsl:value-of select="@IDEARCALC"/>)
                  </span>
                </xsl:if>
              </xsl:when>
              <xsl:otherwise>
                <span style="color:#fff">
                  <xsl:value-of select="'Day'"/>
                </span>
                <xsl:if test="$pWithLabel!=0">
                  <span class="SmallerCode" style="color:#fff">
                    (id: <xsl:value-of select="@IDEARDAY"/>)
                  </span>
                </xsl:if>
              </xsl:otherwise>
            </xsl:choose>
            &#xa0;<xsl:value-of select="$vEarCode"/>
            <xsl:if test="$pWithLabel!=0">
              <span class="SmallerCode">
                (<xsl:value-of select="$vEarEnumRes/EventCodeType[@code=$vEarCode]/Description[@resource=$pCurrentCulture]"/>)
              </span>
            </xsl:if>&#xa0;
            <xsl:variable name="vInfoBulle">
              <xsl:choose>
                <xsl:when test="$vIsEarCalc and @AGFUNC">
                  <xsl:value-of select="@AGFUNC"/>(<xsl:value-of select="@AGAMOUNTS"/>)
                </xsl:when>
                <xsl:otherwise></xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <span title="{$vInfoBulle}">
              <xsl:value-of select="@TYPE"/>
            </span>
            <xsl:if test="$pWithLabel!=0">
              <span class="SmallerCode">
                (<xsl:value-of select="$vEarEnumRes/AmountType[@code=current()/@TYPE]/Description[@resource=$pCurrentCulture]"/>)
              </span>
            </xsl:if>
          </td>

          <!-- Amount value by exchange type -->
          <xsl:for-each select="$vExchangeTypeEnum">
            <xsl:sort select="@ORDER" data-type="number"/>
            <xsl:variable name="vExchangeTypeEnumCode">
              <xsl:value-of select="@EXCHANGETYPE"/>
            </xsl:variable>

            <xsl:if test="($pWithAllExchange!=0 or $vExchangeTypeEnumCode='FCU' or $vExchangeTypeEnumCode='ACU_EVENTDATE')">
              <xsl:for-each select="$vSideEnum">
                <xsl:sort select="@ORDER" data-type="number"/>
                <xsl:variable name="vSideClassName">
                  <xsl:choose>
                    <xsl:when test ="(@SIDE = 'PAY')">
                      <xsl:choose>
                        <xsl:when test ="($vIsEarCommon > 0 or $vIsEarCalc > 0)">
                          <xsl:value-of select="'Ear_OtherPaid'"/>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="'Ear_Paid'"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:when>
                    <xsl:when test ="(@SIDE = 'REC')">
                      <xsl:choose>
                        <xsl:when test ="($vIsEarCommon > 0 or $vIsEarCalc > 0)">
                          <xsl:value-of select="'Ear_OtherReceived'"/>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="'Ear_Received'"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="'xDataGrid_ItemStyle'"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:variable>
                <xsl:variable name="vCurrentEarDayExchangeType" select="$vEarDayAmount[@EXCHANGETYPE=$vExchangeTypeEnumCode]"/>
                <xsl:variable name="vCurrentEarCommonExchangeType" select="$vEarCommonAmount[@EXCHANGETYPE=$vExchangeTypeEnumCode]"/>
                <xsl:variable name="vCurrentEarCalcExchangeType" select="$vEarCalcAmount[@EXCHANGETYPE=$vExchangeTypeEnumCode]"/>
                <xsl:choose>
                  <xsl:when test="$vCurrentEarDayExchangeType or $vCurrentEarCommonExchangeType or $vCurrentEarCalcExchangeType">
                    <xsl:choose>
                      <xsl:when test="@SIDE=$vCurrentSide">

                        <xsl:choose>
                          <xsl:when test="$vIsEarDayOk">
                            <xsl:variable name="vResultEarDay">
                              <xsl:call-template name="GetMoneyByExchangeTypeAndSide">
                                <xsl:with-param name="pMoney"               select="$vEarDayAmount"/>
                                <xsl:with-param name="pCurrentExchangeType" select="$vCurrentEarDayExchangeType/@EXCHANGETYPE"/>
                                <xsl:with-param name="pCurrentSide"         select="@SIDE"/>
                              </xsl:call-template>
                            </xsl:variable>
                            <xsl:call-template name="DisplayEarAmountToday">
                              <xsl:with-param name="pMoney"               select="$vEarDayAmount"/>
                              <xsl:with-param name="pResult"              select="$vResultEarDay"/>
                              <xsl:with-param name="pCurrentExchangeType" select="$vCurrentEarDayExchangeType/@EXCHANGETYPE"/>
                              <xsl:with-param name="pSideClassName"       select="$vSideClassName"/>
                              <xsl:with-param name="pAmountWidth"         select="$vAmountWidth"/>
                            </xsl:call-template>
                          </xsl:when>
                          <xsl:when test="$vIsEarCommonOk">
                            <xsl:variable name="vResultEarCommon">
                              <xsl:call-template name="GetMoneyByExchangeTypeAndSide">
                                <xsl:with-param name="pMoney"               select="$vEarCommonAmount"/>
                                <xsl:with-param name="pCurrentExchangeType" select="$vCurrentEarCommonExchangeType/@EXCHANGETYPE"/>
                                <xsl:with-param name="pCurrentSide"         select="@SIDE"/>
                              </xsl:call-template>
                            </xsl:variable>
                            <xsl:call-template name="DisplayEarAmountToday">
                              <xsl:with-param name="pMoney"               select="$vEarCommonAmount"/>
                              <xsl:with-param name="pResult"              select="$vResultEarCommon"/>
                              <xsl:with-param name="pCurrentExchangeType" select="$vCurrentEarCommonExchangeType/@EXCHANGETYPE"/>
                              <xsl:with-param name="pSideClassName"       select="$vSideClassName"/>
                              <xsl:with-param name="pAmountWidth"         select="$vAmountWidth"/>
                            </xsl:call-template>
                          </xsl:when>
                          <xsl:when test="$vIsEarCalcOk">
                            <xsl:variable name="vResultEarCalc">
                              <xsl:call-template name="GetMoneyByExchangeTypeAndSide">
                                <xsl:with-param name="pMoney"               select="$vEarCalcAmount"/>
                                <xsl:with-param name="pCurrentExchangeType" select="$vCurrentEarCalcExchangeType/@EXCHANGETYPE"/>
                                <xsl:with-param name="pCurrentSide"         select="@SIDE"/>
                              </xsl:call-template>
                            </xsl:variable>
                            <xsl:call-template name="DisplayEarAmountToday">
                              <xsl:with-param name="pMoney"               select="$vEarCalcAmount"/>
                              <xsl:with-param name="pResult"              select="$vResultEarCalc"/>
                              <xsl:with-param name="pCurrentExchangeType" select="$vCurrentEarCalcExchangeType/@EXCHANGETYPE"/>
                              <xsl:with-param name="pSideClassName"       select="$vSideClassName"/>
                              <xsl:with-param name="pAmountWidth"         select="$vAmountWidth"/>
                            </xsl:call-template>
                          </xsl:when>
                        </xsl:choose>

                        <!--xsl:choose>
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
											</xsl:choose-->

                      </xsl:when>
                      <xsl:otherwise>
                        <td class="{$vSideClassName}" width="{$vAmountWidth}px;">
                          <span style="white-space:nowrap">&#xa0;</span>
                        </td>
                        <td class="{$vSideClassName}" width="{$vCurrencyWidth}px;">
                          <span style="white-space:nowrap">&#xa0;</span>
                        </td>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:choose>
                      <xsl:when test="@SIDE=$vCurrentSide">
                        <td class="{$vSideClassName}" align="center" width="{$vAmountWidth}px;">
                          <span style="white-space:nowrap">N/A</span>
                        </td>
                        <td class="{$vSideClassName}" width="{$vCurrencyWidth}px;">
                          <span style="white-space:nowrap">&#xa0;</span>
                        </td>
                      </xsl:when>
                      <xsl:otherwise>
                        <td class="{$vSideClassName}" width="{$vAmountWidth}px;">
                          <span style="white-space:nowrap">&#xa0;</span>
                        </td>
                        <td class="{$vSideClassName}" width="{$vCurrencyWidth}px;">
                          <span style="white-space:nowrap">&#xa0;</span>
                        </td>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:for-each>
            </xsl:if>
          </xsl:for-each>
        </tr>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <!-- Display amount -->
  <xsl:template name="DisplayEarAmountToday">
    <xsl:param name="pMoney"/>
    <xsl:param name="pResult"/>
    <xsl:param name="pCurrentExchangeType"/>
    <xsl:param name="pSideClassName"/>
    <xsl:param name="pAmountWidth"/>
    <xsl:choose>
      <xsl:when test ="$pResult>0">
        <td class="{$pSideClassName}" align="right"  width="{$pAmountWidth}px;">
          <span style="white-space:nowrap">
            <xsl:value-of select="format-number($pMoney[position()=$pResult]/AMOUNT,$amountPattern,$defaultCulture)"/>
          </span>
        </td>
        <td class="{$pSideClassName}" align="center" width="{$vCurrencyWidth}px;">
          <span style="white-space:nowrap">
            <xsl:value-of select="$pMoney[position()=$pResult]/@CURRENCY"/>
          </span>
        </td>
      </xsl:when>
      <xsl:when test ="$pCurrentExchangeType/@EXCHANGETYPE='ACU_EARDATE'">
        <xsl:call-template name="EarAmountToday">
          <xsl:with-param name="pMoney"               select="$pMoney"/>
          <xsl:with-param name="pCurrentExchangeType" select="'ACU_TODAY'"/>
          <xsl:with-param name="pCurrentSide"         select="@SIDE"/>
          <xsl:with-param name="pSideClassName"		    select="$pSideClassName"/>
          <xsl:with-param name="pamountPattern"       select="$amountPattern"/>
          <xsl:with-param name="pCurrentCulture"      select="$pCurrentCulture"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test ="$pCurrentExchangeType/@EXCHANGETYPE='CU1_EARDATE'">
        <xsl:call-template name="EarAmountToday">
          <xsl:with-param name="pMoney"               select="$pMoney"/>
          <xsl:with-param name="pCurrentExchangeType" select="'CU1_TODAY'"/>
          <xsl:with-param name="pCurrentSide"			    select="@SIDE"/>
          <xsl:with-param name="pSideClassName"		    select="$pSideClassName"/>
          <xsl:with-param name="pamountPattern"       select="$amountPattern"/>
          <xsl:with-param name="pCurrentCulture"      select="$pCurrentCulture"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test ="$pCurrentExchangeType/@EXCHANGETYPE='CU2_EARDATE'">
        <xsl:call-template name="EarAmountToday">
          <xsl:with-param name="pMoney"               select="$pMoney"/>
          <xsl:with-param name="pCurrentExchangeType" select="'CU2_TODAY'"/>
          <xsl:with-param name="pCurrentSide"			    select="@SIDE"/>
          <xsl:with-param name="pSideClassName"		    select="$pSideClassName"/>
          <xsl:with-param name="pamountPattern"       select="$amountPattern"/>
          <xsl:with-param name="pCurrentCulture"      select="$pCurrentCulture"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <td class="{$pSideClassName}" width="{$pAmountWidth}px;" align="center" >
          <span style="white-space:nowrap">N/A</span>
        </td>
        <td class="{$pSideClassName}" width="{$vCurrencyWidth}px;">
          <span style="white-space:nowrap">&#xa0;</span>
        </td>
      </xsl:otherwise>
    </xsl:choose>
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
        <td class="{$pSideClassName}" align="right"  width="{$vAmountWidth}px;">
          <span style="white-space:nowrap">
            <xsl:value-of select="format-number($pMoney[position()=$vResult2]/AMOUNT,$pamountPattern,$defaultCulture)"/>
          </span>
        </td>
        <td class="{$pSideClassName}" align="center" width="{$vCurrencyWidth}px;">
          <span style="white-space:nowrap">
            <xsl:value-of select="$pMoney[position()=$vResult2]/@CURRENCY"/>
          </span>
        </td>
      </xsl:when>
      <xsl:otherwise>
        <td class="{$pSideClassName}" align="center" width="{$vAmountWidth}px;">
          <span style="white-space:nowrap">N/A</span>
        </td>
        <td class="{$pSideClassName}" width="{$vCurrencyWidth}px;">
          <span style="white-space:nowrap">&#xa0;</span>
        </td>
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
        <td class="Ear_Notional" align="right" colspan="{($vSideEnumCount*2)-1}" width="{($vSideEnumCount*$vAmountWidth)+(($vSideEnumCount - 1)*$vCurrencyWidth)}px;">
          <span style="white-space:nowrap">
            <xsl:value-of select="format-number($pMoney[position()=$vResult2]/AMOUNT,$pamountPattern,$defaultCulture)"/>
          </span>
        </td>
        <td class="Ear_Notional" align="center" width="{$vCurrencyWidth}px;">
          <span style="white-space:nowrap">
            <xsl:value-of select="$pMoney[position()=$vResult2]/@CURRENCY"/>
          </span>
        </td>
      </xsl:when>
      <xsl:otherwise>
        <td class="Ear_Notional" colspan="{($vSideEnumCount*2)-1}" width="{($vSideEnumCount*$vAmountWidth)+(($vSideEnumCount - 1)*$vCurrencyWidth)}px;" align="center">
          <span style="white-space:nowrap">N/A</span>
        </td>
        <td class="Ear_Notional" width="{$vCurrencyWidth}px;">
          <span style="white-space:nowrap">&#xa0;</span>
        </td>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>