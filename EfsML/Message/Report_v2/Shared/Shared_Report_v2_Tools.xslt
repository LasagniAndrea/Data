<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:fo="http://www.w3.org/1999/XSL/Format"
                xmlns:fn="http://www.w3.org/2005/xpath-functions"
                xmlns:dt="http://xsltsl.org/date-time"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                version="1.0">

  <!-- ============================================================================================== -->
  <!-- Summary : Spheres report - Shared - Common variables for all reports                           -->
  <!-- File    : Shared_Report_v2_PDF.xslt                                                            -->
  <!-- ============================================================================================== -->
  <!-- Version : v5.1.6024                                                                            -->
  <!-- Date    : 20160629                                                                             -->
  <!-- Author  : RD                                                                                   -->
  <!-- Comment : [22286] Gestion de la mention "Annule et Remplace"                                   -->
  <!-- ============================================================================================== -->
  <!-- Version : v5.0.5738                                                                            -->
  <!-- Date    : 20150917                                                                             -->
  <!-- Author  : RD                                                                                   -->
  <!-- Comment : [21376] Correction affichage indicateur Credit/Debit "Parenthesis"                   -->
  <!-- ============================================================================================== -->
  <!-- Version : v4.2.5358                                                                            -->
  <!-- Date    : 20140905                                                                             -->
  <!-- Author  : RD                                                                                   -->
  <!-- Comment : First version                                                                        -->
  <!-- ============================================================================================== -->

  <!-- ============================================================================================== -->
  <!--                                              Settings                                          -->
  <!-- ============================================================================================== -->
  <xsl:include href="..\..\..\Library\Resource.xslt" />
  <xsl:include href="..\..\..\Library\DtFunc.xslt" />
  <xsl:include href="..\..\..\Library\NbrFunc.xslt" />
  <xsl:include href="..\..\..\Library\StrFunc.xslt" />
  <xsl:include href="..\..\..\Library\xsltsl\date-time.xsl" />

  <!-- ============================================================================================== -->
  <!--                                              Variables                                         -->
  <!-- ============================================================================================== -->
  <xsl:variable name="gMainTrade"/>
  <xsl:variable name="gMainBook" select="$gCBTrade/cashBalanceReport/@Acct"/>
  <!--En sachant que cette date (//header/valueDate) représente la date d'envoi, qui est égale à: Date de compensation à laquelle le traitement est demandé + un éventuel offset-->
  <xsl:variable name="gValueDate" select="$gHeader/valueDate" />
  <xsl:variable name="gValueDate2" select="$gHeader/valueDate2" />
  <xsl:variable name="gHeaderTimezone"/>
  <xsl:variable name="gIsPeriodicReport" select="$gValueDate2 = true()"/>
  <xsl:variable name="gExchangeCcy">
    <xsl:choose>
      <xsl:when test="string-length($gCBTrade/cashBalanceReport/exchangeCashBalanceStream/@ccy)>0">
        <xsl:value-of select="$gCBTrade/cashBalanceReport/exchangeCashBalanceStream/@ccy"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$gReportingCcy"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="gExchangeCcyPattern">
    <xsl:call-template name="GetCcyPattern">
      <xsl:with-param name="pCcy" select="$gExchangeCcy"/>
    </xsl:call-template>
  </xsl:variable>

  <!-- .......................................................................... -->
  <!--              Header                                                        -->
  <!-- .......................................................................... -->
  <xsl:variable name="gIsHeaderNotNone" select="$gSettings-headerFooter/hLAllPg/text() != 'None' or
                $gSettings-headerFooter/hCAllPg/text() != 'None' or 
                $gSettings-headerFooter/hRAllPg/text() != 'None'"/>

  <xsl:variable name="gIsHeaderLeftLogoAndCustom" select="$gSettings-headerFooter/hLAllPg/text() = 'Logo' and string-length($gSettings-headerFooter/hLAllPgCustom/text()) > 0"/>
  <xsl:variable name="gIsHeaderCenterLogoAndCustom" select="$gSettings-headerFooter/hCAllPg/text() = 'Logo' and string-length($gSettings-headerFooter/hCAllPgCustom/text()) > 0"/>
  <xsl:variable name="gIsHeaderRightLogoAndCustom" select="$gSettings-headerFooter/hRAllPg/text() = 'Logo' and string-length($gSettings-headerFooter/hRAllPgCustom/text()) > 0"/>

  <!-- .......................................................................... -->
  <!--              Footer                                                        -->
  <!-- .......................................................................... -->
  <xsl:variable name="gIsFooterNotNone" select="$gSettings-headerFooter/fLAllPg/text() != 'None' or
                $gSettings-headerFooter/fCAllPg/text() != 'None' or 
                $gSettings-headerFooter/fRAllPg/text() != 'None'"/>

  <xsl:variable name="gIsFooterLeftCustomAdd" select="string-length($gSettings-headerFooter/fLAllPgCustom/text()) > 0 and $gSettings-headerFooter/fLAllPg/text() != 'Custom'"/>
  <xsl:variable name="gIsFooterCenterCustomAdd" select="string-length($gSettings-headerFooter/fCAllPgCustom/text()) > 0 and $gSettings-headerFooter/fCAllPg/text() != 'Custom'"/>
  <xsl:variable name="gIsFooterRightCustomAdd" select="string-length($gSettings-headerFooter/fRAllPgCustom/text()) > 0 and $gSettings-headerFooter/fRAllPg/text() != 'Custom'"/>

  <xsl:variable name="gIsFooterSecondRow" select="$gIsFooterLeftCustomAdd=true() or $gIsFooterCenterCustomAdd=true() or $gIsFooterRightCustomAdd=true()"/>
  <xsl:variable name="gIsFooterLegalInfo_Model3" select="$gSettings-headerFooter/fLAllPg/text() = 'LegalInfo_Model3' or 
                $gSettings-headerFooter/fCAllPg/text() = 'LegalInfo_Model3' or 
                $gSettings-headerFooter/fRAllPg/text() = 'LegalInfo_Model3'"/>
  <xsl:variable name="gIsFooterLegalInfo_Model2" select="$gSettings-headerFooter/fLAllPg/text() = 'LegalInfo_Model2' or 
                $gSettings-headerFooter/fCAllPg/text() = 'LegalInfo_Model2' or 
                $gSettings-headerFooter/fRAllPg/text() = 'LegalInfo_Model2'"/>
  <xsl:variable name="gIsFooterLegalInfo_Model1" select="$gSettings-headerFooter/fLAllPg/text() = 'LegalInfo_Model1' or 
                $gSettings-headerFooter/fCAllPg/text() = 'LegalInfo_Model1' or 
                $gSettings-headerFooter/fRAllPg/text() = 'LegalInfo_Model1'"/>
  <xsl:variable name="gIsFooterCustom" select="$gSettings-headerFooter/fLAllPg/text() = 'Custom' or 
                $gSettings-headerFooter/fCAllPg/text() = 'Custom' or 
                $gSettings-headerFooter/fRAllPg/text() = 'Custom'"/>

  <!-- .......................................................................... -->
  <!--              Body                                                          -->
  <!-- .......................................................................... -->
  <xsl:variable name="gTitle_Text">
    <xsl:choose>
      <xsl:when test="string-length($gSettings-headerFooter/hTitle/text())>0">
        <xsl:value-of select="$gSettings-headerFooter/hTitle/text()"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="concat($gReportType,'-Title')" />
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="gTitle_Position">
    <xsl:choose>
      <xsl:when test="string-length($gSettings-headerFooter/hTitlePosition/text())>0">
        <xsl:call-template name="Lower">
          <xsl:with-param name="source" select="$gSettings-headerFooter/hTitlePosition/text()"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$gTitle_text-align"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="gHeaderFormula">
    <xsl:choose>
      <xsl:when test="$gSettings-headerFooter">
        <xsl:value-of select="$gSettings-headerFooter/hFormula/text()"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="concat($gReportType,'-TitleDetailed')" />
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="gFooterFormula">
    <xsl:if test="string-length($gSettings-headerFooter/fFormula/text()) > 0">
      <xsl:value-of select="$gSettings-headerFooter/fFormula/text()"/>
    </xsl:if>
  </xsl:variable>
  <!-- ============================================================================================== -->
  <!--                                              Templates                                         -->
  <!-- ============================================================================================== -->

  <!-- .......................................................................... -->
  <!--              Debug Tools                                                   -->
  <!-- .......................................................................... -->
  <!-- .................................... -->
  <!-- Debug_border-red                     -->
  <!-- .................................... -->
  <xsl:template name="Debug_border-red">
    <xsl:if test="$gcIsDisplayDebugMode=true()">
      <xsl:attribute name="border">
        <xsl:value-of select="'1pt solid red'"/>
      </xsl:attribute>
    </xsl:if>
  </xsl:template>
  <!-- .................................... -->
  <!-- Debug_border-blue                    -->
  <!-- .................................... -->
  <xsl:template name="Debug_border-blue">
    <xsl:if test="$gcIsDisplayDebugMode=true()">
      <xsl:attribute name="border">
        <xsl:value-of select="'1pt solid blue'"/>
      </xsl:attribute>
    </xsl:if>
  </xsl:template>
  <!-- .................................... -->
  <!-- Debug_border-green                   -->
  <!-- .................................... -->
  <xsl:template name="Debug_border-green">
    <xsl:if test="$gcIsDisplayDebugMode=true()">
      <xsl:attribute name="border">
        <xsl:value-of select="'1pt solid green'"/>
      </xsl:attribute>
    </xsl:if>
  </xsl:template>
  <!-- .................................... -->
  <!-- Debug_background-color-yellow        -->
  <!-- .................................... -->
  <xsl:template name="Debug_background-color-yellow">
    <xsl:if test="$gcIsDisplayDebugMode=true()">
      <xsl:attribute name="background-color">
        <xsl:value-of select="'yellow'"/>
      </xsl:attribute>
    </xsl:if>
  </xsl:template>
  <!-- .................................... -->
  <!-- Debug_background-color-red           -->
  <!-- .................................... -->
  <xsl:template name="Debug_background-color-red">
    <xsl:if test="$gcIsDisplayDebugMode=true()">
      <xsl:attribute name="background-color">
        <xsl:value-of select="'red'"/>
      </xsl:attribute>
    </xsl:if>
  </xsl:template>
  <!-- .................................... -->
  <!-- Debug_background-color-green         -->
  <!-- .................................... -->
  <xsl:template name="Debug_background-color-green">
    <xsl:if test="$gcIsDisplayDebugMode=true()">
      <xsl:attribute name="background-color">
        <xsl:value-of select="'green'"/>
      </xsl:attribute>
    </xsl:if>
  </xsl:template>

  <!-- .......................................................................... -->
  <!--              Display Tools                                                 -->
  <!-- .......................................................................... -->

  <!-- .................................... -->
  <!-- Display_SpaceLine                    -->
  <!-- .................................... -->
  <xsl:template name="Display_SpaceLine">
    <fo:table-row height="{$gLineSpace_height}"
                  keep-with-previous="always">
      <fo:table-cell>
        <xsl:call-template name="Debug_border-green"/>
      </fo:table-cell>
    </fo:table-row>
  </xsl:template>

  <!-- .................................... -->
  <!-- Display_SubTotalSpace                -->
  <!-- .................................... -->
  <xsl:template name="Display_SubTotalSpace">
    <xsl:param name="pNumber-columns" select="20"/>
    <xsl:param name="pPosition"/>

    <fo:table-row height="{$gSubTotal_space-after}"
                  keep-with-previous="always">
      <fo:table-cell>
        <xsl:if test="$pNumber-columns != number('0')">
          <xsl:attribute name="number-columns-spanned">
            <xsl:value-of select="$pNumber-columns"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:call-template name="Debug_border-green"/>
        <xsl:if test="$pPosition != last()">
          <xsl:attribute name="border">
            <xsl:value-of select="$gTable_border"/>
          </xsl:attribute>
          <xsl:attribute name="border-left-style">
            <xsl:value-of select="'none'"/>
          </xsl:attribute>
          <xsl:attribute name="border-right-style">
            <xsl:value-of select="'none'"/>
          </xsl:attribute>
          <xsl:attribute name="border-top-style">
            <xsl:value-of select="'none'"/>
          </xsl:attribute>
        </xsl:if>
        <!--<fo:block>
          <xsl:call-template name="Debug_border-blue"/>
        </fo:block>-->
      </fo:table-cell>
    </fo:table-row>
  </xsl:template>

  <!-- .................................... -->
  <!-- Display_AddAttribute-color           -->
  <!-- .................................... -->
  <xsl:template name="Display_AddAttribute-color">
    <xsl:param name="pColor"/>
    <xsl:param name="pAttributeName" select="'color'"/>

    <xsl:if test="$gIsColorMode and string-length($pColor) > 0">
      <xsl:attribute name="{$pAttributeName}">
        <xsl:value-of select="$pColor"/>
      </xsl:attribute>
    </xsl:if>
  </xsl:template>

  <!-- .......................................................................... -->
  <!--              Header                                                        -->
  <!-- .......................................................................... -->

  <!-- .................................... -->
  <!-- HeaderFooterRuleSize                 -->
  <!-- .................................... -->
  <!-- Summary : 
        Get the header/footer rule size according to the report settings
       .................................... -->
  <xsl:template name="HeaderFooterRuleSize">
    <xsl:param name="pRuleSize"/>

    <xsl:choose>
      <xsl:when test="string-length($pRuleSize)>0">
        <xsl:value-of select="concat($pRuleSize,'pt')"/>
      </xsl:when>
      <xsl:when test="$gSettings-headerFooter">
        <xsl:value-of select="'0pt'"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'0.5pt'"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- .................................... -->
  <!-- DisplayPageHeaderType                -->
  <!-- .................................... -->
  <!-- Summary : Display the Page Header according to the report settings  
       .................................... -->
  <!--RD 20170602 [xxxxx] Add parameter pParent_Width-->
  <xsl:template name="DisplayPageHeaderType">
    <xsl:param name="pType" />
    <xsl:param name="pCustom" />
    <xsl:param name="pParent_Width" />

    <fo:block>
      <xsl:choose>
        <!-- Logo -->
        <xsl:when test="$pType='Logo'">
          <fo:external-graphic src="{$gImgLogo}" height="{$gHeaderLogo_Height}" vertical-align="bottom"/>
        </xsl:when>
        <!-- Entity -->
        <xsl:when test="$pType='CompanyName'">
          <xsl:variable name="vEntityDisplayname" select="$gParty[@OTCmlId=$gEntityId]/partyName/text()"/>
          <xsl:choose>
            <xsl:when test="string-length($vEntityDisplayname)>0">
              <xsl:value-of select="$vEntityDisplayname" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$gSendBy-routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/actorIdentifier']" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <!-- Book and Published Date -->
        <xsl:when test="starts-with($pType,'Published_Model')">
          <xsl:call-template name="DisplayPublishedModel">
            <xsl:with-param name="pModel" select="$pType" />
          </xsl:call-template>
        </xsl:when>
        <!-- Entity's adress (ex. HPC 22 rue des Capucines 75005 Paris) -->
        <!--RD 20170602 [xxxxx] Add parameter pParent_Width-->
        <xsl:when test="starts-with($pType,'LegalInfo_Model')">
          <xsl:call-template name="DisplayLegalInfoModel">
            <xsl:with-param name="pModel" select="$pType" />
            <xsl:with-param name="pParent_Width" select="$pParent_Width" />
          </xsl:call-template>
        </xsl:when>
      </xsl:choose>
    </fo:block>
    <xsl:if test="$pType != 'None' and string-length($pCustom) > 0">
      <fo:block font-size="{$gHeaderTextCustom_FontSize}"
                wrap-option="wrap" linefeed-treatment="preserve" white-space-collapse="false" white-space-treatment="preserve">
        <xsl:value-of select="$pCustom" />
      </fo:block>
    </xsl:if>
  </xsl:template>

  <!-- .......................................................................... -->
  <!--              Footer                                                        -->
  <!-- .......................................................................... -->
  <!-- .................................... -->
  <!-- DisplayPageFooterType                -->
  <!-- .................................... -->
  <!-- Summary : Display the Page Footer according to the report settings
       .................................... -->
  <!--RD 20170602 [xxxxx] Add parameter pParent_Width-->
  <xsl:template name="DisplayPageFooterType">
    <xsl:param name="pType" />
    <xsl:param name="pCustom" />
    <xsl:param name="pParent_Width" />

    <xsl:choose>
      <!-- Custom -->
      <xsl:when test="$pType='Custom'">
        <fo:block font-size="{$gFooterTextCustom_FontSize}"
                  wrap-option="wrap" linefeed-treatment="preserve" white-space-collapse="false" white-space-treatment="preserve">
          <xsl:value-of select="$pCustom" />
        </fo:block>
      </xsl:when>
      <xsl:otherwise>
        <fo:block linefeed-treatment="preserve">
          <xsl:choose>
            <!-- Entity's adress (ex. HPC 22 rue des Capucines 75005 Paris) -->
            <!--RD 20170602 [xxxxx] Add parameter pParent_Width-->
            <xsl:when test="starts-with($pType,'LegalInfo_Model')">
              <xsl:call-template name="DisplayLegalInfoModel">
                <xsl:with-param name="pModel" select="$pType" />
                <xsl:with-param name="pParent_Width" select="$pParent_Width" />
              </xsl:call-template>
            </xsl:when>
            <!-- PageNumber -->
            <xsl:when test="$pType='PageNumber'">
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Page'" />
              </xsl:call-template>
              <xsl:value-of select="$gcSpace" />
              <fo:page-number />/<fo:page-number-citation ref-id="LastPage" />
            </xsl:when>
          </xsl:choose>
        </fo:block>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- .................................... -->
  <!-- DisplayLegalInfoModel                -->
  <!-- .................................... -->
  <!-- Summary : Display LegalInfoModel
       .................................... -->
  <!--RD 20170602 [xxxxx] Add parameter pParent_Width-->
  <xsl:template name="DisplayLegalInfoModel">
    <xsl:param name="pModel" />
    <xsl:param name="pParent_Width" />

    <xsl:choose>
      <!--Affichage de l'Adresse de l'expéditeur sur deux blocks-->
      <xsl:when test="$pModel='LegalInfo_Model4'">
        <!--EG Set width=$gSection_width-->
        <!--RD 20170602 [xxxxx] Use parameter pParent_Width-->
        <fo:table table-layout="fixed" width="{$pParent_Width}">
          <fo:table-column column-number="01" width="{$gHeaderAdresse_Width}">
            <xsl:call-template name="Debug_border-red"/>
          </fo:table-column>
          <fo:table-column column-number="02" column-width="proportional-column-width(1)" >
            <xsl:call-template name="Debug_border-red"/>
          </fo:table-column>
          <fo:table-body>
            <fo:table-row>
              <fo:table-cell text-align="left" display-align="after" number-columns-spanned="2">
                <fo:block>
                  <xsl:value-of select="$gSendBy-routingAddress/streetAddress/streetLine[1]" />
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
            <fo:table-row>
              <fo:table-cell text-align="left" display-align="after">
                <xsl:for-each select="$gSendBy-routingAddress/streetAddress/streetLine[position()>1]">
                  <fo:block>
                    <xsl:value-of select="." />
                  </fo:block>
                </xsl:for-each>
              </fo:table-cell>
              <fo:table-cell text-align="right" display-align="after">
                <!-- Phone -->
                <fo:block>
                  <xsl:call-template name="DisplayLegalInfoModel_Element">
                    <xsl:with-param name="pResource" select="'INV-Phone'"/>
                    <xsl:with-param name="pData" select="$gSendBy-routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/telephoneNumber']"/>
                    <xsl:with-param name="pWithSeparator" select="false()"/>
                  </xsl:call-template>
                </fo:block>
                <!-- Telex -->
                <fo:block>
                  <xsl:call-template name="DisplayLegalInfoModel_Element">
                    <xsl:with-param name="pResource" select="'INV-Telex'"/>
                    <xsl:with-param name="pData" select="$gSendBy-routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/telexNumber']"/>
                    <xsl:with-param name="pWithSeparator" select="false()"/>
                  </xsl:call-template>
                </fo:block>
                <!-- Fax -->
                <fo:block>
                  <xsl:call-template name="DisplayLegalInfoModel_Element">
                    <xsl:with-param name="pResource" select="'INV-Fax'"/>
                    <xsl:with-param name="pData" select="$gSendBy-routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/faxNumber']"/>
                    <xsl:with-param name="pWithSeparator" select="false()"/>
                  </xsl:call-template>
                </fo:block>
                <!-- Mail -->
                <fo:block>
                  <xsl:value-of select="$gSendBy-routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/email']"/>
                </fo:block>
                <!-- Web -->
                <fo:block>
                  <xsl:value-of select="$gSendBy-routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/web']"/>
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
          </fo:table-body>
        </fo:table>
      </xsl:when>
      <xsl:otherwise>
        <!-- Entity's adress (ex. HPC 22 rue des Capucines 75005 Paris) -->
        <xsl:if test="$pModel='LegalInfo_Model1' or $pModel='LegalInfo_Model2' or $pModel='LegalInfo_Model3'">
          <fo:block>
            <xsl:for-each select="$gSendBy-routingAddress/streetAddress/streetLine">
              <xsl:value-of select="." />
              <xsl:value-of select="$gcSpace" />
            </xsl:for-each>
          </fo:block>
        </xsl:if>

        <!-- Phone / Telex / Fax / Mail sur une ligne-->
        <xsl:if test="$pModel='LegalInfo_Model2' or $pModel='LegalInfo_Model3'">
          <fo:block>
            <!-- Phone -->
            <xsl:variable name="vPhone">
              <xsl:call-template name="DisplayLegalInfoModel_Element">
                <xsl:with-param name="pResource" select="'INV-Phone'"/>
                <xsl:with-param name="pData" select="$gSendBy-routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/telephoneNumber']"/>
                <xsl:with-param name="pWithSeparator" select="false()"/>
              </xsl:call-template>
            </xsl:variable>
            <!-- Telex -->
            <xsl:variable name="vTelex">
              <xsl:call-template name="DisplayLegalInfoModel_Element">
                <xsl:with-param name="pResource" select="'INV-Telex'"/>
                <xsl:with-param name="pData" select="$gSendBy-routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/telexNumber']"/>
                <xsl:with-param name="pWithSeparator" select="string-length($vPhone)>0"/>
              </xsl:call-template>
            </xsl:variable>
            <!-- Fax -->
            <xsl:variable name="vFax">
              <xsl:call-template name="DisplayLegalInfoModel_Element">
                <xsl:with-param name="pResource" select="'INV-Fax'"/>
                <xsl:with-param name="pData" select="$gSendBy-routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/faxNumber']"/>
                <xsl:with-param name="pWithSeparator" select="string-length($vPhone)>0 or string-length($vTelex)>0"/>
              </xsl:call-template>
            </xsl:variable>
            <!-- Mail -->
            <xsl:variable name="vMail">
              <xsl:if test="$pModel='LegalInfo_Model2'">
                <xsl:call-template name="DisplayLegalInfoModel_Element">
                  <xsl:with-param name="pResource" select="'INV-Mail'"/>
                  <xsl:with-param name="pData" select="$gSendBy-routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/email']"/>
                  <xsl:with-param name="pWithSeparator" select="string-length($vPhone)>0 or string-length($vTelex)>0 or string-length($vFax)>0"/>
                </xsl:call-template>
              </xsl:if>
            </xsl:variable>

            <xsl:value-of select="concat($vPhone,$vTelex,$vFax,$vMail)"/>
          </fo:block>
        </xsl:if>

        <!-- Phone / Telex / Fax / Mailsur deux lignes-->
        <xsl:if test="$pModel='LegalInfo_Model3'">
          <fo:block>
            <!-- Mail -->
            <xsl:call-template name="DisplayLegalInfoModel_Element">
              <xsl:with-param name="pResource" select="'INV-Mail'"/>
              <xsl:with-param name="pData" select="$gSendBy-routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/email']"/>
              <xsl:with-param name="pWithSeparator" select="false()"/>
            </xsl:call-template>
          </fo:block>
        </xsl:if>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- .................................... -->
  <!-- DisplayLegalInfoModel_Element        -->
  <!-- .................................... -->
  <!-- Summary : Display LegalInfoModel details
  ....................................
  Parameters:
	  pResource: element label
    pData: element value
    pWithSeparator: add separator " - "
     .................................... -->
  <xsl:template name="DisplayLegalInfoModel_Element">
    <xsl:param name="pResource"/>
    <xsl:param name="pData"/>
    <xsl:param name="pWithSeparator" select="true()"/>

    <xsl:if test="string-length($pData) &gt; 0">
      <xsl:if test="$pWithSeparator = true()">
        <xsl:value-of select="concat($gcSpace,'-',$gcSpace)" />
      </xsl:if>
      <xsl:call-template name="getSpheresTranslation">
        <xsl:with-param name="pResourceName" select="$pResource" />
      </xsl:call-template>
      <xsl:value-of select="concat(':',$gcSpace)" />
      <xsl:value-of select="$pData" />
    </xsl:if>
  </xsl:template>

  <!-- .......................................................................... -->
  <!--              Body                                                          -->
  <!-- .......................................................................... -->
  <!-- .................................... -->
  <!-- ReportTitle                          -->
  <!-- .................................... -->
  <!-- Summary : Display Report Title
  ....................................
  Parameters:
	  None
  ....................................
  -->
  <xsl:template name="ReportTitle">
    <fo:block font-size="{$gTitle_font-size}" font-weight="{$gTitle_font-weight}" text-align="{$gTitle_Position}"
              space-before="{$gTitle_space-before}"
              wrap-option="wrap" linefeed-treatment="preserve" white-space-collapse="false" white-space-treatment="preserve">
      <xsl:call-template name="Debug_border-blue"/>
      <xsl:value-of select="$gTitle_Text"/>
    </fo:block>
  </xsl:template>
  <!-- .................................... -->
  <!-- ReportSendDetails                    -->
  <!-- .................................... -->
  <!-- Summary : Display Cancel message and Send To address
  ....................................
  Parameters:
	  None
  ....................................
  -->
  <xsl:template name="ReportSendDetails">
    <fo:block>
      <!-- FI 20160808 Migration vs2013 add width -->
      <fo:table table-layout="fixed" width="200mm">
        <fo:table-column column-number="01" column-width="{$gDisplaySettings-CancelMessage/@msg_column-width}">
          <xsl:call-template name="Debug_border-blue"/>
        </fo:table-column>
        <fo:table-column column-number="02" column-width="{$gDisplaySettings-CancelMessage/@space_column-width}">
          <xsl:call-template name="Debug_border-red"/>
        </fo:table-column>
        <!-- FI 20160808 Migration vs2013 add column-width -->
        <fo:table-column column-number="03" column-width="proportional-column-width(1)">
          <xsl:call-template name="Debug_border-blue" />
        </fo:table-column>
        <fo:table-body>
          <fo:table-row>
            <fo:table-cell display-align="{$gDisplaySettings-CancelMessage/@display-align}">
              <xsl:call-template name="ReportCancelMessage"/>
            </fo:table-cell>
            <fo:table-cell/>
            <fo:table-cell>
              <xsl:call-template name="ReportSendToAddress"/>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </fo:block>
  </xsl:template>
  <!-- .................................... -->
  <!-- ReportCancelMessage                  -->
  <!-- .................................... -->
  <!-- Summary : Display Cancel message
  ....................................
  Parameters:
	  None
  ....................................
  -->
  <xsl:template name="ReportCancelMessage">
    <xsl:if test="$gDisplaySettings-CancelMessage/@isPreviousCreationTimestamp = 'true' and string-length($gDisplaySettings-CancelMessage/text()) > 0">
      <fo:block font-size="{$gDisplaySettings-CancelMessage/@font-size}"
                color="{$gDisplaySettings-CancelMessage/@color}"
                border="{$gDisplaySettings-CancelMessage/@border}"
                font-weight="{$gDisplaySettings-CancelMessage/@font-weight}"
                text-align="{$gDisplaySettings-CancelMessage/@text-align}"
                display-align="{$gDisplaySettings-CancelMessage/@display-align}"
                padding="{$gDisplaySettings-CancelMessage/@padding}"
                space-before="{$gDisplaySettings-CancelMessage/@space-before}"
                wrap-option="wrap" linefeed-treatment="preserve" white-space-collapse="false" white-space-treatment="preserve">
        <xsl:call-template name="Debug_border-green"/>
        <xsl:value-of select="$gDisplaySettings-CancelMessage/text()" />
      </fo:block>
    </xsl:if>
  </xsl:template>
  <!-- .................................... -->
  <!-- ReportSendToAddress                  -->
  <!-- .................................... -->
  <!-- Summary : Display Send To address
  ....................................
  Parameters:
	  None
  ....................................
  -->
  <xsl:template name="ReportSendToAddress">
    <fo:block font-family="{$gReceiverAdress_font-family}"
              font-size="{$gReceiverAdress_font-size}"
              text-align="{$gReceiverAdress_text-align}"
              space-before="{$gReceiverAdress_space-before}"
              linefeed-treatment="preserve">
      <xsl:call-template name="Debug_border-green"/>
      <xsl:for-each select="$gSendTo-routingAddress/streetAddress/streetLine">
        <fo:block>
          <xsl:value-of select="." />
        </fo:block>
      </xsl:for-each>
    </fo:block>
  </xsl:template>
  <!-- .................................... -->
  <!-- ReportBusinessDetails                -->
  <!-- .................................... -->
  <!-- Summary : Display Business detail: Book, Business date, ....
  ....................................
  Parameters:
	  None
  ....................................
  -->
  <xsl:template name="ReportBusinessDetails">

    <!--Label-->
    <!-- CC 20160615 [22259] -->
    <!--<xsl:variable name="vHolderIdent">
      <xsl:call-template name="getSpheresTranslation">
        <xsl:with-param name="pResourceName" select="'Report-Holder'" />
      </xsl:call-template>
    </xsl:variable>-->
    <xsl:variable name="vHolderIdent">
      <xsl:choose>
        <xsl:when test="$gSettings-headerFooter/hBookOwnerIdLabel">
          <xsl:value-of select="$gSettings-headerFooter/hBookOwnerIdLabel/text()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-Holder'" />
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vAccountIdent">
      <xsl:choose>
        <xsl:when test="$gSettings-headerFooter/hBookIdLabel">
          <xsl:value-of select="$gSettings-headerFooter/hBookIdLabel/text()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'TS-Account'" />
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vClearingDateIdent">
      <xsl:choose>
        <xsl:when test="$gSettings-headerFooter/hDtBusinessLabel">
          <xsl:value-of select="$gSettings-headerFooter/hDtBusinessLabel/text()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:choose>
            <xsl:when test="$gIsPeriodicReport = true()">
              <xsl:value-of select="$gDisplaySettings-BusinessDetail/column[@name='Ident']/data[@name='Date']/@resource"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$gDisplaySettings-BusinessDetail/column[@name='Ident']/data[@name='Dates']/@resource"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vIdentMaxLength">
      <xsl:choose>
        <xsl:when test="string-length($vHolderIdent) > string-length($vAccountIdent) and string-length($vHolderIdent) > string-length($vClearingDateIdent)">
          <xsl:value-of select="string-length($vHolderIdent)"/>
        </xsl:when>
        <xsl:when test="string-length($vAccountIdent) > string-length($vHolderIdent) and string-length($vAccountIdent) > string-length($vClearingDateIdent)">
          <xsl:value-of select="string-length($vAccountIdent)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="string-length($vClearingDateIdent)"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!--Data-->
    <xsl:variable name="vBookRepository" select="$gRepository/book[identifier=$gMainBook]"/>
    <xsl:variable name="vBookOwnerActor" select="$gRepository/actor[@id=$vBookRepository/owner/@href]"/>

    <xsl:variable name="vBookOwnerName" select="$vBookOwnerActor/displayname"/>
    <xsl:variable name="vBookDisplayName" select="$vBookRepository/displayname"/>
    <xsl:variable name="vBookCcy" select="$vBookRepository/idc"/>
    <xsl:variable name="vClearingDate">
      <xsl:call-template name="format-date">
        <xsl:with-param name="xsd-date-time" select="$gValueDate" />
      </xsl:call-template>
    </xsl:variable>

    <fo:block linefeed-treatment="preserve" font-size="{$gDisplaySettings-BusinessDetail/@font-size}">
      <!--EG Set width=$gSection_width-->
      <fo:table table-layout="fixed" width="{$gSection_width}">
        <fo:table-column column-number="01" column-width="{$gDisplaySettings-BusinessDetail/column[@name='Ident']/@column-width}">
          <xsl:call-template name="Debug_border-red"/>
        </fo:table-column>
        <xsl:choose>
          <xsl:when test="$vIdentMaxLength > ($gDisplaySettings-BusinessDetail/column[@name='Ident']/data/@length + $gDisplaySettings-BusinessDetail/column[@name='Ident2']/data/@length)">
            <fo:table-column column-number="02" column-width="{2* $gDisplaySettings-BusinessDetail/column[@name='Ident2']/@column-width}">
              <xsl:call-template name="Debug_border-red"/>
            </fo:table-column>
            <!-- EG 20160308 Migration vs2013 ok -->
            <fo:table-column column-number="03" column-width="proportional-column-width(1)" >
              <xsl:call-template name="Debug_border-red"/>
            </fo:table-column>
          </xsl:when>
          <xsl:when test="$vIdentMaxLength > ($gDisplaySettings-BusinessDetail/column[@name='Ident']/data/@length)">
            <fo:table-column column-number="02" column-width="{$gDisplaySettings-BusinessDetail/column[@name='Ident2']/@column-width}">
              <xsl:call-template name="Debug_border-red"/>
            </fo:table-column>
            <!-- EG 20160308 Migration vs2013 ok -->
            <fo:table-column column-number="03" column-width="proportional-column-width(1)">
              <xsl:call-template name="Debug_border-red"/>
            </fo:table-column>
          </xsl:when>
          <xsl:otherwise>
            <!-- EG 20160308 Migration vs2013 ok -->
            <fo:table-column column-number="02" column-width="proportional-column-width(1)">
              <xsl:call-template name="Debug_border-red"/>
            </fo:table-column>
          </xsl:otherwise>
        </xsl:choose>
        <fo:table-body>
          <!--Business detail: Book owner-->
          <xsl:if test="string-length($vBookOwnerName) > 0">
            <fo:table-row>
              <fo:table-cell font-weight="{$gDisplaySettings-BusinessDetail/column[@name='Ident']/data/@font-weight}"
                             text-align="{$gDisplaySettings-BusinessDetail/column[@name='Ident']/data/@text-align}"
                             display-align="{$gDisplaySettings-BusinessDetail/@display-align}">
                <xsl:if test="$vIdentMaxLength > ($gDisplaySettings-BusinessDetail/column[@name='Ident']/data/@length)">
                  <xsl:attribute name="number-columns-spanned">
                    <xsl:value-of select="'2'"/>
                  </xsl:attribute>
                </xsl:if>
                <fo:block white-space-collapse="false">
                  <xsl:value-of select="$vHolderIdent"/>
                </fo:block>
              </fo:table-cell>
              <fo:table-cell font-weight="{$gDisplaySettings-BusinessDetail/column[@name='Data']/data/@font-weight}"
                             text-align="{$gDisplaySettings-BusinessDetail/column[@name='Data']/data/@text-align}"
                             display-align="{$gDisplaySettings-BusinessDetail/@display-align}">
                <fo:block>
                  <xsl:value-of select="$vBookOwnerName" />
                </fo:block>
              </fo:table-cell>
            </fo:table-row>
          </xsl:if>
          <!--Business detail: Book-->
          <fo:table-row>
            <fo:table-cell font-weight="{$gDisplaySettings-BusinessDetail/column[@name='Ident']/data/@font-weight}"
                           text-align="{$gDisplaySettings-BusinessDetail/column[@name='Ident']/data/@text-align}"
                           display-align="{$gDisplaySettings-BusinessDetail/@display-align}">
              <xsl:if test="$vIdentMaxLength > ($gDisplaySettings-BusinessDetail/column[@name='Ident']/data/@length)">
                <xsl:attribute name="number-columns-spanned">
                  <xsl:value-of select="'2'"/>
                </xsl:attribute>
              </xsl:if>
              <fo:block white-space-collapse="false">
                <xsl:value-of select="$vAccountIdent"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell font-weight="{$gDisplaySettings-BusinessDetail/column[@name='Data']/data/@font-weight}"
                           text-align="{$gDisplaySettings-BusinessDetail/column[@name='Data']/data/@text-align}"
                           display-align="{$gDisplaySettings-BusinessDetail/@display-align}">
              <fo:block>
                <xsl:value-of select="$gMainBook" />
                <xsl:if test="string-length($vBookCcy) > 0">
                  <fo:inline font-size="{$gDisplaySettings-BusinessDetail/column[@name='Data']/data[@name='Det']/@font-size}"
                             font-weight="{$gDisplaySettings-BusinessDetail/column[@name='Data']/data[@name='Det']/@font-weight}">
                    <xsl:value-of select="concat($gcSpace,'(',$vBookCcy,')')" />
                  </fo:inline>
                </xsl:if>
              </fo:block>
              <xsl:if test="(string-length($vBookDisplayName) > 0) and  (normalize-space($vBookDisplayName) != normalize-space($gMainBook))">
                <fo:block font-size="{$gDisplaySettings-BusinessDetail/column[@name='Data']/data[@name='Det']/@font-size}"
                          font-weight="{$gDisplaySettings-BusinessDetail/column[@name='Data']/data[@name='Det']/@font-weight}">
                  <xsl:value-of select="$vBookDisplayName" />
                </fo:block>
              </xsl:if>
            </fo:table-cell>
          </fo:table-row>
          <!--Business detail: Business date-->
          <fo:table-row>
            <fo:table-cell font-weight="{$gDisplaySettings-BusinessDetail/column[@name='Ident']/data/@font-weight}"
                           text-align="{$gDisplaySettings-BusinessDetail/column[@name='Ident']/data/@text-align}"
                           display-align="{$gDisplaySettings-BusinessDetail/@display-align}">
              <xsl:if test="$vIdentMaxLength > ($gDisplaySettings-BusinessDetail/column[@name='Ident']/data/@length)">
                <xsl:attribute name="number-columns-spanned">
                  <xsl:value-of select="'2'"/>
                </xsl:attribute>
              </xsl:if>
              <fo:block white-space-collapse="false">
                <xsl:value-of select="$vClearingDateIdent"/>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell font-weight="{$gDisplaySettings-BusinessDetail/column[@name='Data']/data/@font-weight}"
                           text-align="{$gDisplaySettings-BusinessDetail/column[@name='Data']/data/@text-align}"
                           display-align="{$gDisplaySettings-BusinessDetail/@display-align}">
              <fo:block>
                <xsl:value-of select="normalize-space($vClearingDate)" />
                <xsl:if test="$gIsPeriodicReport = true()">
                  <fo:inline font-weight="{$gDisplaySettings-BusinessDetail/column[@name='Data']/data[@name='To']/@font-weight}">
                    <xsl:value-of select="concat($gcSpace,$gDisplaySettings-BusinessDetail/column[@name='Data']/data[@name='To']/@resource,$gcSpace)"/>
                  </fo:inline>
                  <fo:inline font-weight="{$gDisplaySettings-BusinessDetail/column[@name='Data']/data/@font-weight}">
                    <xsl:call-template name="format-date">
                      <xsl:with-param name="xsd-date-time" select="normalize-space($gValueDate2)" />
                    </xsl:call-template>
                  </fo:inline>
                </xsl:if>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
          <!--Add more business details here-->
        </fo:table-body>
      </fo:table>
    </fo:block>
  </xsl:template>
  <!-- .................................... -->
  <!-- ReportIntroduction                   -->
  <!-- .................................... -->
  <!-- Summary : Display Report Introduction -->
  <xsl:template name="ReportIntroduction">
    <xsl:if test="string-length($gHeaderFormula) > 0">
      <fo:block font-size="{$gFormula_font-size}" font-weight="{$gFormula_font-weight}" text-align="{$gFormula_text-align}"
                space-before="{$gFormula_space-before}"
                wrap-option="wrap" linefeed-treatment="preserve" white-space-collapse="false" white-space-treatment="preserve">
        <xsl:call-template name="Debug_border-blue"/>
        <xsl:value-of select="$gHeaderFormula"/>
      </fo:block>
    </xsl:if>
  </xsl:template>


  <!-- .................................... -->
  <!-- ProcessedExchanges                   -->
  <!-- .................................... -->
  <!-- FI 20170213 [22151] Add Template -->
  <!-- FI 20170217 [22151] shortIdentifier or acronym or identifier -->
  <xsl:template name ="ProcessedExchanges">
    <xsl:if test="$gCBTrade/cashBalanceReport/endOfDayStatus/cssCustodianStatus" >
      <xsl:variable name="vEndOfDayStatus" select ="$gCBTrade/cashBalanceReport/endOfDayStatus"/>
      <xsl:variable name="vMarket" select ="$vEndOfDayStatus//exchStatus"/>
      <xsl:variable name ="vMarketNode">
        <xsl:for-each select ="$vMarket">
          <xsl:variable name="vRepository-market" select="$gRepository/market[@OTCmlId=current()/@OTCmlId]"/>
          <xsl:element name ="market">
            <xsl:attribute name ="identifier">
              <xsl:choose>
                <xsl:when test="$vRepository-market/shortIdentifier">
                  <xsl:value-of select ="$vRepository-market/shortIdentifier"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:choose>
                    <xsl:when test="$vRepository-market/acronym">
                      <xsl:value-of select ="$vRepository-market/acronym"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select ="$vRepository-market/identifier"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:attribute>
            <xsl:attribute name ="iso">
              <xsl:value-of select ="$vRepository-market/ISO10383_ALPHA4"/>
            </xsl:attribute>
            <xsl:attribute name ="status">
              <xsl:value-of select ="current()/@status"/>
            </xsl:attribute>
          </xsl:element>
        </xsl:for-each>
      </xsl:variable>

      <xsl:variable name ="vCountMarketUnprocessed" select ="count($vEndOfDayStatus//exchStatus[@status='Unperformed'])" />
      <xsl:variable name ="vCountMarketProcessed" select ="count($vEndOfDayStatus//exchStatus[@status='Performed'])" />

      <xsl:variable name="labelInfoNode">
        <labelInfo>
          <xsl:choose>
            <xsl:when test ="$vCountMarketUnprocessed=0">
              <xsl:attribute  name ="label">
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'Report-AllExchangesProcessed'"/>
                </xsl:call-template>
              </xsl:attribute>
              <xsl:attribute  name ="color">
                <xsl:value-of select="'black'" />
              </xsl:attribute>
              <xsl:attribute  name ="fontWeight">
                <xsl:value-of select="'normal'" />
              </xsl:attribute>
            </xsl:when>
            <xsl:when test ="$vCountMarketProcessed=0">
              <xsl:attribute  name ="label">
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'Report-AllExchangesUnprocessed'"/>
                </xsl:call-template>
              </xsl:attribute>
              <xsl:attribute  name ="color">
                <xsl:value-of select="'darkred'" />
              </xsl:attribute>
              <xsl:attribute  name ="fontWeight">
                <xsl:value-of select="'normal'" />
              </xsl:attribute>
            </xsl:when>
            <xsl:otherwise>
              <xsl:attribute  name ="label">
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'Report-SomeExchangesUnprocessed'"/>
                </xsl:call-template>
              </xsl:attribute>
              <xsl:attribute  name ="color">
                <xsl:value-of select="'darkred'" />
              </xsl:attribute>
              <xsl:attribute  name ="fontWeight">
                <xsl:value-of select="'bold'" />
              </xsl:attribute>
            </xsl:otherwise>
          </xsl:choose>
        </labelInfo>
      </xsl:variable>
      <xsl:variable name="vLabelInfo" select="msxsl:node-set($labelInfoNode)"/>

      <fo:table table-layout="fixed" table-omit-header-at-break="false" table-omit-footer-at-break="true" width="{$gSection_width}">
        <fo:table-column column-number="01" column-width="200mm" />
        <!-- space-before -->
        <fo:table-header>
          <fo:table-row>
            <fo:table-cell number-columns-spanned="1">
              <fo:block space-before="{$gSection_space-before}" />
            </fo:table-cell>
          </fo:table-row>
        </fo:table-header>
        <fo:table-body border="1pt solid black">
          <!-- 1er Row-->
          <fo:table-row>
            <fo:table-cell color="{$vLabelInfo/labelInfo/@color}"  font-weight="{$vLabelInfo/labelInfo/@fontWeight}" padding="0.5mm" >
              <fo:block font-size="7pt">
                <xsl:value-of select="$vLabelInfo/labelInfo/@label"/>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
          <xsl:choose>
            <xsl:when test ="$vCountMarketProcessed=0">
              <!-- Nothing -->
            </xsl:when>
            <xsl:otherwise>
              <!--2nd row-->
              <fo:table-row>
                <fo:table-cell color="black"  padding="0.5mm">
                  <fo:block font-size="7pt" >
                    <xsl:if test ="$vCountMarketUnprocessed>0">
                      <fo:inline>
                        <xsl:call-template name="getSpheresTranslation">
                          <xsl:with-param name="pResourceName" select="'Report-Processed'"/>
                        </xsl:call-template>
                      </fo:inline>
                    </xsl:if>
                    <xsl:call-template name ="BuildLstMarket">
                      <xsl:with-param  name ="pMarketNode" select="$vMarketNode"/>
                      <xsl:with-param  name ="pStatus" select="'Performed'"/>
                    </xsl:call-template>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <xsl:if test ="$vCountMarketUnprocessed>0">
                <!--3nd row-->
                <fo:table-row>
                  <fo:table-cell color="black"  padding="0.5mm">
                    <fo:block font-size="7pt" >
                      <fo:inline>
                        <xsl:call-template name="getSpheresTranslation">
                          <xsl:with-param name="pResourceName" select="'Report-Unprocessed'"/>
                        </xsl:call-template>
                      </fo:inline>
                      <xsl:call-template name ="BuildLstMarket">
                        <xsl:with-param  name ="pMarketNode" select="$vMarketNode"/>
                        <xsl:with-param  name ="pStatus" select="'Unperformed'"/>
                      </xsl:call-template>
                    </fo:block>
                  </fo:table-cell>
                </fo:table-row>
              </xsl:if>
            </xsl:otherwise>
          </xsl:choose>
        </fo:table-body>
      </fo:table>
    </xsl:if>
  </xsl:template>

  <!-- .................................... -->
  <!-- BuildLstMarket                       -->
  <!-- .................................... -->
  <!-- FI 20170213 [22151] Add Template -->
  <xsl:template name ="BuildLstMarket">
    <xsl:param name="pMarketNode"/>
    <xsl:param name="pStatus" select="'Performed'"/>
    <xsl:for-each select ="msxsl:node-set($pMarketNode)/market[@status=$pStatus]">
      <fo:inline>
        <xsl:value-of select="concat(current()/@identifier,' (',current()/@iso,')')"/>
      </fo:inline>
      <xsl:if test="position() != last()">
        <fo:inline>
          <xsl:value-of select ="', '"/>
        </fo:inline>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>


  <!-- .................................... -->
  <!-- ReportLastMessage                    -->
  <!-- .................................... -->
  <!-- Summary : Display Report last message
  ....................................
  Parameters:
	  None
  ....................................
  -->
  <xsl:template name="ReportLastMessage">
    <xsl:if test="string-length($gFooterFormula) > 0">
      <!--EG Set width=$gSection_width-->
      <!--EG Set column-width=$gSection_width-->
      <fo:table table-layout="fixed"
                table-omit-header-at-break="false"
                display-align="center"
                keep-together.within-page="always"
                width="{$gSection_width}">

        <fo:table-column column-number="01" column-width="{$gSection_width}">
          <xsl:call-template name="Debug_border-red"/>
        </fo:table-column>
        <fo:table-body>
          <fo:table-row>
            <fo:table-cell>
              <fo:block font-size="{$gFormula_font-size}"
                        font-weight="{$gFormula_font-weight}"
                        text-align="{$gFormula_text-align}"
                        space-before="{$gFormula_space-before}"
                        display-align="after"
                        wrap-option="wrap" linefeed-treatment="preserve" white-space-collapse="false" white-space-treatment="preserve">
                <xsl:call-template name="Debug_border-blue"/>
                <xsl:value-of select="$gFooterFormula"/>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </xsl:if>
  </xsl:template>

  <!-- .......................................................................... -->
  <!--              Display data                                                  -->
  <!-- .......................................................................... -->

  <!-- .................................... -->
  <!-- DisplayPublishedModel                -->
  <!-- .................................... -->
  <!-- Summary : Display PublishedModel
  ....................................
  Parameters:
	  pModel: model name
  ....................................
  -->
  <xsl:template name="DisplayPublishedModel">
    <xsl:param name="pModel" />

    <!--Affichage de <Book-Identifier>-->
    <xsl:if test="$pModel='Published_Model1' or $pModel='Published_Model1UTC' or $pModel='Published_Model2'">
      <fo:block>
        <xsl:value-of select="$gMainBook" />
      </fo:block>
    </xsl:if>
    <!--Affichage de <Actor-Identifier>-->
    <xsl:if test="$pModel='Published_Model4'">
      <fo:block>
        <xsl:value-of select="normalize-space($gSendBy-routingId[@routingIdCodeScheme='http://www.euro-finance-systems.fr/otcml/actorIdentifier'])" />
      </fo:block>
    </xsl:if>
    <!--Affichage de <Invoice-Identifier>-->
    <xsl:if test="$pModel='Published_Model4'">
      <fo:block>
        <xsl:copy-of select="$gMainTrade" />
      </fo:block>
    </xsl:if>
    <!--Affichage de <Edité le > date de création du report -->
    <xsl:if test="$pModel='Published_Model1' or $pModel='Published_Model1UTC' or
            $pModel='Published_Model2' or $pModel='Published_Model3' or $pModel='Published_Model4' or 
            $pModel='Published_Model5' or $pModel='Published_Model5UTC' or 
            $pModel='Published_Model6'">
      <fo:block>
        <!--Affichage de la ressource <Edité le >-->
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'TS-Published'" />
        </xsl:call-template>
        <!--Affichage de la date de création du report-->
        <xsl:call-template name="format-date">
          <xsl:with-param name="xsd-date-time" select="$gHeader/creationTimestamp" />
        </xsl:call-template>
        <!--Affichage de l'heure de création du report -->
        <xsl:if test="$pModel='Published_Model1' or $pModel='Published_Model1UTC' or 
                $pModel='Published_Model5' or $pModel='Published_Model5UTC' or 
                $pModel='Published_Model6'">
          <xsl:value-of select="$gcSpace"/>
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'TS-PublishedAt'" />
          </xsl:call-template>
          <xsl:value-of select="$gcSpace"/>
          <xsl:choose>
            <xsl:when test="$pModel='Published_Model1' or $pModel='Published_Model5' or $pModel='Published_Model6' ">
              <xsl:call-template name="format-time2">
                <xsl:with-param name="xsd-date-time" select="substring-after($gHeader/creationTimestamp, 'T')" />
              </xsl:call-template>
              <xsl:if test="$pModel='Published_Model6'">
                <xsl:variable name="vCity">
                  <xsl:call-template name="Lower">
                    <xsl:with-param name="source" select="normalize-space($gSendBy-routingAddress/city/text())"/>
                  </xsl:call-template>
                </xsl:variable>
                <xsl:value-of select="concat($gcSpace,'(')"/>
                <xsl:call-template name="FirstUCase">
                  <xsl:with-param name="source" select="$vCity" />
                </xsl:call-template>
                <xsl:value-of select="')'"/>
              </xsl:if>
            </xsl:when>
            <xsl:when test="$pModel='Published_Model1UTC' or $pModel='Published_Model5UTC'">
              <xsl:call-template name="format-time2">
                <xsl:with-param name="xsd-date-time" select="substring-after($gHeader/creationTimestamp/@utc, 'T')" />
              </xsl:call-template>
              <xsl:value-of select="concat($gcSpace,'(UTC Time)')"/>
            </xsl:when>
          </xsl:choose>
        </xsl:if>
      </fo:block>
    </xsl:if>
  </xsl:template>

  <!-- ................................................ -->
  <!-- DisplayData_Format                               -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Format Data (Date is always fomatted in shortdate)
       ................................................ -->
  <!-- FI 20160530 [21885] Modify -->
  <!-- RD 20160914 [22453] Affichage de la donnée avec une petite police en fonction de sa longueur-->
  <xsl:template name="DisplayData_Format">
    <!-- Représente la donnée à formater (Format ISO)-->
    <xsl:param name="pData"/>
    <!-- Représente le type de donnée -->
    <xsl:param name="pDataType"/>
    <!-- Représente le format attendu -->
    <xsl:param name="pDataFormat"/>
    <!-- Si renseigné => Représente la taille maximum -->
    <xsl:param name="pDataLength" select ="number('0')"/>
    <!-- Si renseigné => Représente les caractéristiques de la colonne -->
    <xsl:param name="pColumnSettings"/>

    <xsl:variable name="vFormatedData">
      <xsl:choose>
        <xsl:when test="$pDataType='Date'" >
          <xsl:call-template name="format-shortdate_ddMMMyy">
            <xsl:with-param name="year" select="substring($pData , 1, 4)" />
            <xsl:with-param name="month" select="substring($pData , 6, 2)" />
            <xsl:with-param name="day" select="substring($pData , 9, 2)" />
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pDataType='Time'" >
          <xsl:variable name="vFormat">
            <xsl:call-template name="GetTimeFormat">
              <xsl:with-param name="pTimeFormat" select="$pDataFormat" />
            </xsl:call-template>
          </xsl:variable>
          <xsl:call-template name="dt:format-date-time">
            <xsl:with-param name="xsd-date-time" select="$pData"/>
            <xsl:with-param name="format" select="$vFormat"/>
          </xsl:call-template>
        </xsl:when>
        <!-- FI 20160530 [21885] Gestion du type Integer -->
        <xsl:when test="$pDataType='Integer'">
          <!-- Affiche un number selon le format $integerPattern-->
          <xsl:call-template name="format-integer">
            <xsl:with-param name="integer" select="$pData"/>
          </xsl:call-template>
        </xsl:when>
        <!-- FI 20160530 [21885] Gestion du type Decimal -->
        <!-- Affiche un number selon le format $pDataFormat-->
        <xsl:when test="$pDataType='Decimal'">
          <xsl:call-template name="format-number">
            <xsl:with-param name="pAmount" select="$pData"/>
            <xsl:with-param name="pAmountPattern" select="$pDataFormat" />
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$pData"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="$pColumnSettings and $pColumnSettings/style">
        <xsl:for-each select="$pColumnSettings/style">
          <xsl:sort  select="@length" order="ascending" data-type="number"/>

          <xsl:variable name="vCurrentStyle" select="current()"/>
          <xsl:variable name="vPrevStyle" select="preceding-sibling::style[1]"/>

          <xsl:choose>
            <xsl:when test="($vCurrentStyle/@length >= string-length($vFormatedData)) and (($vPrevStyle = false()) or (string-length($vFormatedData) > $vPrevStyle/@length))">
              <fo:inline font-size="{$vCurrentStyle/@font-size}">
                <xsl:value-of select="$vFormatedData"/>
              </fo:inline>
            </xsl:when>
            <xsl:when test="boolean(position() = last()) and string-length($vFormatedData) > $vCurrentStyle/@length">
              <fo:inline font-size="{$vCurrentStyle/@font-size}">
                <xsl:call-template name="DisplayData_Truncate">
                  <xsl:with-param name="pData" select="$vFormatedData"/>
                  <xsl:with-param name="pDataLength" select="$vCurrentStyle/@length"/>
                </xsl:call-template>
              </fo:inline>
            </xsl:when>
          </xsl:choose>
        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>

        <xsl:variable name="vDataLength">
          <xsl:choose>
            <xsl:when test="$pColumnSettings">
              <xsl:value-of select="$pColumnSettings/data/@length"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$pDataLength"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:call-template name="DisplayData_Truncate">
          <xsl:with-param name="pData" select="$vFormatedData"/>
          <xsl:with-param name="pDataLength" select="$vDataLength"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- ................................................ -->
  <!-- GetTimeFormat                                    -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Get Time format using
       ................................................ -->
  <xsl:template name="GetTimeFormat">
    <xsl:param name="pTimeFormat"/>
    <xsl:param name="pDefaultFormat"/>
    <xsl:param name="pIsDisplay" select="true()"/>

    <xsl:choose>
      <xsl:when test="$pTimeFormat='HH24:MI:SS'">
        <xsl:choose>
          <xsl:when test="$pIsDisplay = true()">
            <xsl:value-of select="concat('%H', $separatorTime,'%M', $separatorTime,'%S')" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$pTimeFormat" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="$pTimeFormat='HH24:MI'">
        <xsl:choose>
          <xsl:when test="$pIsDisplay = true()">
            <xsl:value-of select="concat('%H', $separatorTime,'%M')" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$pTimeFormat" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="$pTimeFormat='H24:MI:SS'">
        <xsl:choose>
          <xsl:when test="$pIsDisplay = true()">
            <xsl:value-of select="concat('%h', $separatorTime,'%M', $separatorTime,'%S')" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$pTimeFormat" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="$pTimeFormat='H24:MI'">
        <xsl:choose>
          <xsl:when test="$pIsDisplay = true()">
            <xsl:value-of select="concat('%h', $separatorTime,'%M')" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$pTimeFormat" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="$pTimeFormat='HH12:MI:SS AM/PM'">
        <xsl:choose>
          <xsl:when test="$pIsDisplay = true()">
            <xsl:value-of select="concat('%I', $separatorTime,'%M', $separatorTime,'%S',' %P')" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$pTimeFormat" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="$pTimeFormat='HH12:MI AM/PM'">
        <xsl:choose>
          <xsl:when test="$pIsDisplay = true()">
            <xsl:value-of select="concat('%I', $separatorTime,'%M',' %P')" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$pTimeFormat" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="$pTimeFormat='H12:MI:SS AM/PM'">
        <xsl:choose>
          <xsl:when test="$pIsDisplay = true()">
            <xsl:value-of select="concat('%i', $separatorTime,'%M', $separatorTime,'%S',' %P')" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$pTimeFormat" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="$pTimeFormat='H12:MI AM/PM'">
        <xsl:choose>
          <xsl:when test="$pIsDisplay = true()">
            <xsl:value-of select="concat('%i', $separatorTime,'%M',' %P')" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$pTimeFormat" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="string-length($pDefaultFormat) > 0">
        <xsl:choose>
          <xsl:when test="$pIsDisplay = true()">
            <xsl:call-template name="GetTimeFormat">
              <xsl:with-param name="pTimeFormat" select="$pDefaultFormat" />
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$pDefaultFormat" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="GetTimeFormat">
          <xsl:with-param name="pTimeFormat" select="'HH24:MI:SS'" />
          <xsl:with-param name="pIsDisplay" select="$pIsDisplay" />
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- ................................................ -->
  <!-- DisplayFmtNumber                                 -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display formatted price          
       ................................................ -->
  <xsl:template name="DisplayFmtNumber">
    <xsl:param name="pFmtNumber"/>
    <xsl:param name="pPattern"/>

    <xsl:choose>
      <!--- "100'5" ou "100^5" ou "100-5" -->
      <xsl:when test="contains($pFmtNumber,&quot;&apos;&quot;) or contains($pFmtNumber,'^') or contains($pFmtNumber,'-')">
        <xsl:value-of select="$pFmtNumber"/>
      </xsl:when>
      <!--- "100 16/32"-->
      <xsl:when test="contains($pFmtNumber,'/')">
        <xsl:variable name="vData-before" select="substring-before($pFmtNumber,' ')"/>
        <xsl:variable name="vData-after" select="substring-after($pFmtNumber,' ')"/>
        <xsl:variable name="vSup" select="substring-before($vData-after,'/')"/>
        <xsl:variable name="vSub" select="substring-after($vData-after,'/')"/>

        <xsl:value-of select="$vData-before"/>
        <fo:inline vertical-align="super" font-size="60%">
          <xsl:value-of select="$vSup"/>
        </fo:inline>/<fo:inline font-size="60%">
          <xsl:value-of select="$vSub"/>
        </fo:inline>
      </xsl:when>
      <!--- "100.5"  (défaut)-->
      <xsl:when test="string-length($pFmtNumber) > 0">
        <xsl:call-template name="format-number">
          <xsl:with-param name="pAmount" select="$pFmtNumber" />
          <xsl:with-param name="pAmountPattern" select="$pPattern" />
        </xsl:call-template>
      </xsl:when>
    </xsl:choose>
  </xsl:template>


  <!-- ................................................ -->
  <!-- DisplaySubTotalColumnSettlPx                     -->
  <!-- Affiche la ressource puis le prix                -->
  <!-- ................................................ -->
  <!-- FI 20151007 [21311] Add Template -->
  <xsl:template name ="DisplaySubTotalColumnSettlPx">
    <xsl:param name="pFmtPrice"/>
    <xsl:param name="pPattern"/>
    <xsl:param name="pResName"/>
    <xsl:choose>
      <xsl:when test ="$pResName='TheoPrice'">
        <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='SettlPx']/data[@name='TheoPrice']/@resource"/>
      </xsl:when>
      <xsl:when test ="$pResName='FwdRate'">
        <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='SettlPx']/data[@name='FwdRate']/@resource"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='SettlPx']/data/@resource"/>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:value-of select="$gcSpace"/>
    <fo:inline font-size="7pt" font-weight="bold">
      <xsl:call-template name="DisplayFmtNumber">
        <xsl:with-param name="pFmtNumber" select="$pFmtPrice"/>
        <xsl:with-param name="pPattern" select="$pPattern" />
      </xsl:call-template>
    </fo:inline>
  </xsl:template>

  <!-- EG 20190730 New : Mesure du prix -->
  <xsl:template name ="DisplaySubTotalColumnAssetMeasureClosePrice">
    <xsl:param name="pFmtPrice"/>
    <xsl:param name="pPattern"/>
    <xsl:param name="pResName"/>
    <xsl:choose>
      <xsl:when test ="string-length($pResName)>0">
        <xsl:value-of select="$pResName"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='SettlPx']/data/@resource"/>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:value-of select="$gcSpace"/>
    <fo:inline font-size="7pt" font-weight="bold">
      <xsl:call-template name="DisplayFmtNumber">
        <xsl:with-param name="pFmtNumber" select="$pFmtPrice"/>
        <xsl:with-param name="pPattern" select="$pPattern" />
      </xsl:call-template>
    </fo:inline>
  </xsl:template>    

  <!-- ................................................ -->
  <!-- DisplaySubTotalColumnAccDays                     -->
  <!-- Affiche la "Acc. Days" puis le nbr de jour       -->
  <!-- ................................................ -->
  <!-- FI 20151019 [21317] Add Template -->
  <xsl:template name ="DisplaySubTotalColumnAccIntDays">
    <xsl:param name="pDays"/>
    <xsl:value-of select="$gBlockSettings_Data/subtotal/column[@name='AccIntDays']/data/@resource"/>
    <xsl:value-of select="$gcSpace"/>
    <fo:inline font-size="7pt" font-weight="bold">
      <xsl:call-template name="format-integer">
        <xsl:with-param name="integer" select="$pDays"/>
      </xsl:call-template>
    </fo:inline>
  </xsl:template>


  <!-- ................................................ -->
  <!-- IsFmtPrice                                       -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Return true if {pPrice} is formatted price          
       ................................................ -->
  <xsl:template name="IsFmtPrice">
    <xsl:param name="pPrice"/>

    <xsl:choose>
      <!--- "100'5" ou "100^5" ou "100-5" ou "100 16/32" -->
      <xsl:when test="contains($pPrice,&quot;&apos;&quot;) or contains($pPrice,'^') or contains($pPrice,'-') or contains($pPrice,'/')">
        <xsl:value-of select="true()"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="false()"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- ................................................ -->
  <!-- DisplayData_Efsml                                -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Data in Efsml flow          
       ................................................ -->
  <!-- RD 20190619 [23912] Modify -->
  <xsl:template name="DisplayData_Efsml">
    <!-- Represente la donnée demandée-->
    <xsl:param name="pDataName"/>
    <!-- Represente un noeud trades/trade ou posActions/posAction ou posTrades/trade-->
    <xsl:param name="pDataEfsml"/>
    <!-- Represente le fommat numérique demandé -->
    <xsl:param name="pNumberPattern" select="$gDefaultPricePattern"/>
    <!-- Si renseigné tronque(*) le résultat.  -->
    <xsl:param name="pDataLength" select ="number('0')"/>


    <xsl:variable name="vData">
      <xsl:choose>
        <xsl:when test="$pDataName='TrdDt'" >
          <xsl:call-template name="DisplayData_Format">
            <xsl:with-param name="pData" select="$gCommonData/trade[@tradeId=$pDataEfsml/@tradeId]/@trdDt"/>
            <xsl:with-param name="pDataType" select="'Date'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pDataName='BizDt'" >
          <xsl:call-template name="DisplayData_Format">
            <xsl:with-param name="pData" select="$gCommonData/trade[@tradeId=$pDataEfsml/@tradeId]/@bizDt"/>
            <xsl:with-param name="pDataType" select="'Date'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pDataName='TimeStamp'" >
          <xsl:if test="$gIsDisplayTimestamp=true()">
            <xsl:call-template name="DisplayData_Format">
              <xsl:with-param name="pData" select="$gCommonData/trade[@tradeId=$pDataEfsml/@tradeId]/@trdTm"/>
              <xsl:with-param name="pDataType" select="'Time'"/>
              <xsl:with-param name="pDataFormat" select="$gTimestamp_Type"/>
            </xsl:call-template>
          </xsl:if>
        </xsl:when>
        <!-- RD 20190619 [23912] Add order Timestamp -->
        <xsl:when test="$pDataName='TimeStampOrd'" >
          <xsl:if test="$gIsDisplayTimestamp=true()">
            <xsl:call-template name="DisplayData_Format">
              <xsl:with-param name="pData" select="$gCommonData/trade[@tradeId=$pDataEfsml/@tradeId]/@ordTm"/>
              <xsl:with-param name="pDataType" select="'Time'"/>
              <xsl:with-param name="pDataFormat" select="$gTimestamp_Type"/>
            </xsl:call-template>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$pDataName='PosResult'" >
          <xsl:choose>
            <xsl:when test="$pDataEfsml/@posResult='Partial Close'">
              <xsl:value-of select="'Cl./Op.'"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$pDataEfsml/@posResult"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="$pDataName='Qty'" >
          <xsl:call-template name="format-integer">
            <xsl:with-param name="integer" select="$pDataEfsml/@qty"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pDataName='FmtQty'" >
          <xsl:call-template name="DisplayFmtNumber">
            <xsl:with-param name="pFmtNumber" select="$pDataEfsml/@fmtQty"/>
            <xsl:with-param name="pPattern" select="$pNumberPattern" />
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pDataName='FmtPrice'" >
          <xsl:call-template name="DisplayFmtNumber">
            <xsl:with-param name="pFmtNumber" select="$gCommonData/trade[@tradeId=$pDataEfsml/@tradeId]/@fmtLastPx"/>
            <xsl:with-param name="pPattern" select="$pNumberPattern" />
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pDataName='FmtUnlPx'" >
          <xsl:call-template name="DisplayFmtNumber">
            <xsl:with-param name="pFmtNumber" select="$pDataEfsml/@fmtUnlPx"/>
            <xsl:with-param name="pPattern" select="$pNumberPattern" />
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pDataName='FmtClrPx'" >
          <xsl:call-template name="DisplayFmtNumber">
            <xsl:with-param name="pFmtNumber" select="$pDataEfsml/@fmtClrPx"/>
            <xsl:with-param name="pPattern" select="$pNumberPattern" />
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pDataName='PaymentType'" >
          <xsl:value-of select="$pDataEfsml/@paymentType"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <xsl:call-template name="DisplayData_Truncate">
      <xsl:with-param name="pData" select="$vData"/>
      <xsl:with-param name="pDataLength" select="$pDataLength"/>
    </xsl:call-template>
  </xsl:template>


  <!-- ................................................ -->
  <!-- RptSideGetValue                                  -->
  <!-- ................................................ -->
  <!-- FI 20151019 [21317] Add Template -->
  <xsl:template name="RptSideGetValue">
    <!-- Represente la donnée demandée-->
    <xsl:param name="pDataName"/>
    <!-- Represente un élément RptSide -->
    <xsl:param name ="pRptSide"/>

    <xsl:choose>
      <!-- Retourne le book -->
      <xsl:when test="$pDataName='Acct'" >
        <xsl:value-of select="$pRptSide/@Acct"/>
      </xsl:when>

      <!-- Retourne la ressource Achat/Vente -->
      <xsl:when test="$pDataName='Side'" >
        <xsl:choose>
          <xsl:when test="$pRptSide/@Side = '1'">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-BuyData'" />
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pRptSide/@Side = '2'">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'Report-SellData'" />
            </xsl:call-template>
          </xsl:when>
        </xsl:choose>
      </xsl:when>

      <!-- Retourne la ressource Long/Short -->
      <xsl:when test="$pDataName='PosSide'" >
        <xsl:choose>
          <xsl:when test="$pRptSide/@Side = '1'">
            <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='long']/@resource"/>
          </xsl:when>
          <xsl:when test="$pRptSide/@Side = '2'">
            <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='short']/@resource"/>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
    </xsl:choose>

  </xsl:template>



  <!-- ................................................ -->
  <!-- DisplayData_SubTotal                             -->
  <!-- ................................................ -->
  <!-- Summary :                                        
        Display Data in Subtotal flow          
       ................................................ -->
  <xsl:template name="DisplayData_SubTotal">
    <xsl:param name="pDataName"/>
    <xsl:param name="pSubTotal"/>
    <xsl:param name="pDataEfsml"/>
    <xsl:param name="pCcyPattern"/>
    <xsl:param name="pNumberPattern" select="$gDefaultPricePattern"/>
    <xsl:param name="pDataLength" select ="number('0')"/>
    <xsl:param name="pQtyModel" select="'Date2Qty'"/>
    <xsl:param name="pIsFmtQty" select="true()"/>

    <xsl:variable name="vData">
      <xsl:choose>
        <xsl:when test="$pDataName='LongSide'" >
          <xsl:choose>
            <xsl:when test="$pSubTotal/long/@fmtQty > 0">
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='long']/@resource"/>
            </xsl:when>
            <xsl:when test="$pSubTotal/short/@fmtQty > 0">
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='short']/@resource"/>
            </xsl:when>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="$pDataName='ShortSide'" >
          <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='short']/@resource"/>
        </xsl:when>
        <xsl:when test="$pDataName='NetSide'" >
          <xsl:choose>
            <xsl:when test="$pSubTotal/long/@fmtQty > $pSubTotal/short/@fmtQty">
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='long']/@resource"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$gBlockSettings_Data/column[@name='Qty']/header[@name='short']/@resource"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="$pDataName='BuySide'" >
          <xsl:choose>
            <xsl:when test="$pSubTotal/long/@fmtQty > 0">
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-BuyData'" />
              </xsl:call-template>
            </xsl:when>
            <xsl:when test="$pSubTotal/short/@fmtQty > 0">
              <xsl:call-template name="getSpheresTranslation">
                <xsl:with-param name="pResourceName" select="'Report-SellData'" />
              </xsl:call-template>
            </xsl:when>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="$pDataName='SellSide'" >
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'Report-SellData'" />
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$pDataName='LongQty'" >
          <xsl:choose>
            <xsl:when test="$pSubTotal/long/@fmtQty > 0">
              <xsl:choose>
                <xsl:when test="$pIsFmtQty = true()">
                  <xsl:call-template name="DisplayFmtNumber">
                    <xsl:with-param name="pFmtNumber" select="$pSubTotal/long/@fmtQty"/>
                    <xsl:with-param name="pPattern" select="$pNumberPattern" />
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="format-integer">
                    <xsl:with-param name="integer" select="$pSubTotal/long/@qty"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:when test="$pQtyModel!='Date2Qty' and $pSubTotal/short/@fmtQty >= 0">
              <xsl:choose>
                <xsl:when test="$pIsFmtQty = true()">
                  <xsl:call-template name="DisplayFmtNumber">
                    <xsl:with-param name="pFmtNumber" select="$pSubTotal/short/@fmtQty"/>
                    <xsl:with-param name="pPattern" select="$pNumberPattern" />
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="format-integer">
                    <xsl:with-param name="integer" select="$pSubTotal/short/@qty"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:when test="$pSubTotal/long/@fmtQty = 0">
              <xsl:choose>
                <xsl:when test="$pIsFmtQty = true()">
                  <xsl:call-template name="DisplayFmtNumber">
                    <xsl:with-param name="pFmtNumber" select="$pSubTotal/long/@fmtQty"/>
                    <xsl:with-param name="pPattern" select="$pNumberPattern" />
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="format-integer">
                    <xsl:with-param name="integer" select="$pSubTotal/long/@qty"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="$pDataName='ShortQty'" >
          <xsl:if test="$pSubTotal/short/@fmtQty >= 0">
            <xsl:choose>
              <xsl:when test="$pIsFmtQty = true()">
                <xsl:call-template name="DisplayFmtNumber">
                  <xsl:with-param name="pFmtNumber" select="$pSubTotal/short/@fmtQty"/>
                  <xsl:with-param name="pPattern" select="$pNumberPattern" />
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name="format-integer">
                  <xsl:with-param name="integer" select="$pSubTotal/short/@qty"/>
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$pDataName='NetQty'" >
          <xsl:choose>
            <xsl:when test="$pSubTotal/long/@fmtQty > $pSubTotal/short/@fmtQty">
              <xsl:choose>
                <xsl:when test="$pIsFmtQty = true()">
                  <xsl:call-template name="DisplayFmtNumber">
                    <xsl:with-param name="pFmtNumber" select="$pSubTotal/long/@fmtQty - $pSubTotal/short/@fmtQty"/>
                    <xsl:with-param name="pPattern" select="$pNumberPattern" />
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="format-integer">
                    <xsl:with-param name="integer" select="$pSubTotal/long/@qty - $pSubTotal/short/@qty"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:otherwise>
              <xsl:choose>
                <xsl:when test="$pIsFmtQty = true()">
                  <xsl:call-template name="DisplayFmtNumber">
                    <xsl:with-param name="pFmtNumber" select="$pSubTotal/short/@fmtQty - $pSubTotal/long/@fmtQty"/>
                    <xsl:with-param name="pPattern" select="$pNumberPattern" />
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="format-integer">
                    <xsl:with-param name="integer" select="$pSubTotal/short/@qty - $pSubTotal/long/@qty"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="$pDataName='FmtLongAvgPx'" >
          <xsl:if test="$pSubTotal/long/@fmtQty > 0">
            <xsl:call-template name="DisplayFmtNumber">
              <xsl:with-param name="pFmtNumber" select="$pSubTotal/long/@fmtAvgPx"/>
              <xsl:with-param name="pPattern" select="$pNumberPattern" />
            </xsl:call-template>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$pDataName='FmtShortAvgPx'" >
          <xsl:if test="$pSubTotal/short/@fmtQty > 0">
            <xsl:call-template name="DisplayFmtNumber">
              <xsl:with-param name="pFmtNumber" select="$pSubTotal/short/@fmtAvgPx"/>
              <xsl:with-param name="pPattern" select="$pNumberPattern" />
            </xsl:call-template>
          </xsl:if>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <xsl:call-template name="DisplayData_Truncate">
      <xsl:with-param name="pData" select="$vData"/>
      <xsl:with-param name="pDataLength" select="$pDataLength"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- Display various data                             -->
  <!-- ................................................ -->
  <xsl:template name="DisplayData_Amounts">
    <xsl:param name="pDataName"/>
    <xsl:param name="pAmount"/>
    <xsl:param name="pCcy"/>
    <xsl:param name="pAmountPattern"/>

    <xsl:variable name="vCcy">
      <xsl:call-template name="GetAmount-ccy">
        <xsl:with-param name="pAmount" select="$pAmount"/>
        <xsl:with-param name="pCcy" select="$pCcy"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="$pDataName = 'ccy'">
        <!--Ccy-->
        <xsl:value-of select="$vCcy"/>
      </xsl:when>
      <xsl:otherwise>
        <!--Amount-->
        <xsl:variable name="vAmount">
          <xsl:call-template name="GetAmount-amt">
            <xsl:with-param name="pAmount" select="$pAmount"/>
            <xsl:with-param name="pCcy" select="$vCcy"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:choose>
          <xsl:when test="$pDataName = 'amount'">
            <xsl:variable name="vCcyPattern">
              <xsl:choose>
                <xsl:when test="string-length($pAmountPattern) > 0">
                  <xsl:value-of select="$pAmountPattern"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="GetCcyPattern">
                    <xsl:with-param name="pCcy" select="$vCcy"/>
                  </xsl:call-template>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:variable name="vAbsAmount">
              <xsl:call-template name="AbsAmount">
                <xsl:with-param name="pAmount" select="$vAmount"/>
              </xsl:call-template>
            </xsl:variable>
            <xsl:variable name="vRoundedAmount">
              <xsl:choose>
                <xsl:when test="string-length($vCcy) > 0">
                  <xsl:call-template name="GetRoundAmount">
                    <xsl:with-param name="pAmount" select="$vAbsAmount" />
                    <xsl:with-param name="pCcy" select="$vCcy"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$vAbsAmount"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:call-template name="format-number">
              <xsl:with-param name="pAmount" select="$vRoundedAmount"/>
              <xsl:with-param name="pAmountPattern" select="$vCcyPattern" />
            </xsl:call-template>
          </xsl:when>
          <xsl:when test="$pDataName = 'side'">
            <!--Side-->
            <xsl:call-template name="GetAmount-side">
              <xsl:with-param name="pAmount" select="$vAmount"/>
            </xsl:call-template>
          </xsl:when>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="DisplayAmount">
    <xsl:param name="pAmount"/>
    <xsl:param name="pCcy"/>
    <xsl:param name="pAmountPattern"/>

    <xsl:variable name="vCcyPattern">
      <xsl:choose>
        <xsl:when test="string-length($pAmountPattern) > 0">
          <xsl:value-of select="$pAmountPattern"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="GetCcyPattern">
            <xsl:with-param name="pCcy" select="$pCcy"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vAbsAmount">
      <xsl:call-template name="AbsAmount">
        <xsl:with-param name="pAmount" select="$pAmount"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vRoundedAmount">
      <xsl:choose>
        <xsl:when test="string-length($pCcy) > 0">
          <xsl:call-template name="GetRoundAmount">
            <xsl:with-param name="pAmount" select="$vAbsAmount" />
            <xsl:with-param name="pCcy" select="$pCcy"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$vAbsAmount"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:call-template name="format-number">
      <xsl:with-param name="pAmount" select="$vRoundedAmount"/>
      <xsl:with-param name="pAmountPattern" select="$vCcyPattern" />
    </xsl:call-template>
  </xsl:template>
  <xsl:template name="DisplayCurrencyPair">
    <xsl:param name="pQuotedCurrencyPair"/>

    <xsl:variable name="vCcy1" select="$pQuotedCurrencyPair/currency1/text()"/>
    <xsl:variable name="vCcy2" select="$pQuotedCurrencyPair/currency2/text()"/>
    <xsl:variable name="vQuoteBasis" select="$pQuotedCurrencyPair/quoteBasis/text()"/>

    <xsl:choose>
      <xsl:when test="$vQuoteBasis='Currency1PerCurrency2'">
        <xsl:value-of select="concat($vCcy2,'/',$vCcy1)"/>
      </xsl:when>
      <xsl:when test="$vQuoteBasis='Currency2PerCurrency1'">
        <xsl:value-of select="concat($vCcy1,'/',$vCcy2)"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="DisplayeExchangeRate">
    <xsl:param name="pExchangeRate"/>
    <xsl:param name="pFlowCcy"/>
    <xsl:param name="pExCcy" select="$gExchangeCcy"/>

    <xsl:choose>
      <xsl:when test="$pFlowCcy = $pExCcy"/>
      <xsl:when test="$pExchangeRate/rate">
        <xsl:call-template name="DisplayCurrencyPair">
          <xsl:with-param name="pQuotedCurrencyPair" select="$pExchangeRate/quotedCurrencyPair"/>
        </xsl:call-template>
        <xsl:value-of select="': '"/>
        <xsl:call-template name="format-number">
          <xsl:with-param name="pAmount" select="number($pExchangeRate/rate)" />
          <xsl:with-param name="pAmountPattern" select="$numberPatternNoZero" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="concat($pExCcy,'/',$pFlowCcy,': ')"/>
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'TS-FxRateNA'" />
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="DisplayMarket">
    <xsl:param name="pRepository-market"/>

    <xsl:choose>
      <xsl:when test="string-length($pRepository-market/shortIdentifier) > 0">
        <xsl:value-of select="$pRepository-market/shortIdentifier"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pRepository-market/displayname"/>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:value-of select="concat(' - ',$pRepository-market/ISO10383_ALPHA4)"/>
  </xsl:template>
  <xsl:template name="DisplayDC">
    <xsl:param name="pRepository-derivativeContract"/>

    <xsl:value-of select="concat($pRepository-derivativeContract/displayname,' - ')"/>
    <xsl:choose>
      <xsl:when test="$pRepository-derivativeContract/category='F'">
        <xsl:value-of select="'Future'"/>
      </xsl:when>
      <xsl:when test="$pRepository-derivativeContract/category='O'">
        <xsl:value-of select="'Option'"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="DisplayCcySym">
    <xsl:param name="pCcy"/>

    <xsl:variable name="vCcyRepository" select="$gRepository/currency[identifier=$pCcy]"/>
    <xsl:choose>
      <xsl:when test="$vCcyRepository/symbol">
        <xsl:value-of select="$vCcyRepository/symbol"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pCcy"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="DisplaySide">
    <xsl:param name="pAmount"/>

    <xsl:if test="$pAmount">
      <xsl:choose>
        <xsl:when test="number($pAmount) > 0">
          <xsl:value-of select="$gcCredit"/>
        </xsl:when>
        <xsl:when test="0 > number($pAmount)">
          <xsl:value-of select="$gcDebit"/>
        </xsl:when>
      </xsl:choose>
    </xsl:if>
  </xsl:template>
  <xsl:template name="DisplayPaymentType">
    <xsl:param name="pPaymentType"/>

    <xsl:variable name="vResourceName" select="concat('Report-Legend',$pPaymentType)"/>
    <xsl:variable name="vResource">
      <xsl:call-template name="getSpheresTranslation">
        <xsl:with-param name="pResourceName" select="$vResourceName" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$vResource != $vResourceName">
        <xsl:value-of select="$vResourceName"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pPaymentType"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="DisplayInstr">
    <xsl:param name="pSectionKey"/>
    <xsl:param name="pIdI"/>

    <xsl:variable name="vInstr-Repository" select="$gRepository/instr[@OTCmlId=$pIdI]"/>
    <xsl:choose>
      <xsl:when test="contains(',ExchangeTradedFuture,ExchangeTradedOption,',concat(',',$vInstr-Repository/identifier/text(),','))">
        <xsl:choose>
          <xsl:when test="$pSectionKey=$gcReportSectionKey_DLV">
            <xsl:text disable-output-escaping="yes">Future</xsl:text>
          </xsl:when>
          <xsl:when test="contains(concat(',',$gcReportSectionKey_EXE,',',$gcReportSectionKey_ASS,',',$gcReportSectionKey_ABN,',',$gcReportSectionKey_EXA,','),concat(',',$pSectionKey,','))">
            <xsl:text disable-output-escaping="yes">Option</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text disable-output-escaping="yes">Future &amp; Option</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$vInstr-Repository/displayname/text()"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="DisplayType_Short">
    <xsl:param name="pType"/>

    <xsl:call-template name="Upper">
      <xsl:with-param name="source" select="concat('(',substring(normalize-space($pType),1,number('3')),')')"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="DisplayPercentage">
    <xsl:param name="pFactor"/>
    <xsl:param name="pPattern" select="$numberPatternNoZero"/>

    <xsl:call-template name="format-number">
      <xsl:with-param name="pAmount" select="number($pFactor * 100)" />
      <xsl:with-param name="pAmountPattern" select="$pPattern" />
    </xsl:call-template>
    <xsl:value-of select="'%'"/>
  </xsl:template>

  <!-- ................................................ -->
  <!-- DisplayData_Truncate                             -->
  <!-- ................................................ -->
  <!-- Summary : 
  Si la taille de {pData} dépasse {pDataLength}:
  - Enlève le dernier caractère et ajoute '...'
  ..................................................... -->
  <xsl:template name="DisplayData_Truncate">
    <xsl:param name="pData"/>
    <xsl:param name="pDataLength"/>

    <xsl:choose>
      <xsl:when test="($pDataLength > number('0')) and (string-length($pData) > $pDataLength)">
        <xsl:choose>
          <xsl:when test="$pDataLength > number('1')">
            <xsl:value-of select="substring(normalize-space($pData),1,$pDataLength - number('1'))"/>
            <xsl:value-of select="'...'"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="substring(normalize-space($pData),1,1)"/>
            <xsl:value-of select="'...'"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:copy-of select="$pData"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- .......................................................................... -->
  <!--              Tools                                                         -->
  <!-- .......................................................................... -->

  <!-- ...........  Amount Tools................................................. -->
  <xsl:template name="SignAmount">
    <xsl:param name="pAmount" />
    <xsl:param name="pSide" />

    <xsl:choose>
      <xsl:when test="$pSide = $gcCredit">
        <xsl:value-of select="number($pAmount)"/>
      </xsl:when>
      <xsl:when test="$pSide = $gcDebit">
        <xsl:value-of select="number(-1) * number($pAmount)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="0"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>
  <xsl:template name="GetCcyPattern">
    <xsl:param name="pCcy"/>

    <xsl:variable name="vCcyRepository" select="$gRepository/currency[identifier=$pCcy]"/>
    <xsl:call-template name="GetDecPattern">
      <xsl:with-param name="pRoundDir" select="$vCcyRepository/rounddir/text()"/>
      <xsl:with-param name="pRoundPrec" select="$vCcyRepository/roundprec/text()"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ................................................ -->
  <!-- GetRoundAmount                                   -->
  <!-- ................................................ -->
  <!-- Summary :
    Retourne le montant arrondi en fonction des paramètres de la devise
  ..................................................... -->
  <xsl:template name="GetRoundAmount">
    <xsl:param name="pAmount"/>
    <xsl:param name="pCcy"/>

    <xsl:variable name="vCcyRepository" select="$gRepository/currency[identifier=$pCcy]"/>
    <!--RD 20161206 [22665] Test $pAmount is zero and $gRepository/currency exists -->
    <xsl:choose>
      <xsl:when test="number($pAmount) != 0 and $vCcyRepository">
        <xsl:call-template name="RoundAmount">
          <xsl:with-param name="pAmount" select="$pAmount" />
          <xsl:with-param name="pRoundDir" select="$vCcyRepository/rounddir/text()"/>
          <xsl:with-param name="pRoundPrec" select="$vCcyRepository/roundprec/text()"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pAmount"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- ................................................ -->
  <!-- GetAmount-ccy                                    -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Retourne la devise d'un montant                 -->
  <!-- ................................................ -->
  <xsl:template name="GetAmount-ccy">
    <!--Un montant signé ou bien une liste d'éléments avec deux attributs:amt et side -->
    <xsl:param name="pAmount"/>
    <xsl:param name="pCcy"/>

    <xsl:choose>
      <xsl:when test="string-length($pCcy) > 0">
        <xsl:value-of select="$pCcy"/>
      </xsl:when>
      <xsl:when test="msxsl:node-set($pAmount)/@amt">
        <xsl:value-of select="$pAmount[1]/@ccy"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- ................................................ -->
  <!-- GetQuantity-qtyUnit                              -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Retourne l'unité de la quantité                 -->
  <!-- ................................................ -->
  <xsl:template name="GetQuantity-qtyUnit">
    <!--Une quantité non signé ou bien une liste d'éléments avec deux attributs:fmtQty et qtyUnit -->
    <xsl:param name="pQuantity"/>
    <xsl:param name="pQtyUnit"/>

    <xsl:choose>
      <xsl:when test="string-length($pQtyUnit) > 0">
        <xsl:value-of select="$pQtyUnit"/>
      </xsl:when>
      <xsl:when test="msxsl:node-set($pQuantity)/@qtyUnit">
        <xsl:value-of select="$pQuantity[1]/@qtyUnit"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- ................................................ -->
  <!-- GetAmount-amt                                    -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Retourne la valeur d'un montant                 -->
  <!-- ................................................ -->
  <xsl:template name="GetAmount-amt">
    <!--Un montant signé ou bien une liste d'éléments avec deux attributs:amt et side -->
    <xsl:param name="pAmount"/>
    <xsl:param name="pCcy"/>

    <xsl:choose>
      <xsl:when test="string-length($pAmount) > 0">
        <xsl:value-of select="$pAmount"/>
      </xsl:when>
      <xsl:when test="msxsl:node-set($pAmount)/@amt">
        <xsl:choose>
          <xsl:when test="count($pAmount) > 1">
            <xsl:choose>
              <xsl:when test="$pAmount/@side">
                <xsl:value-of select="number(sum($pAmount[@ccy=$pCcy and @side=$gcCredit]/@amt) - sum($pAmount[@ccy=$pCcy and @side=$gcDebit]/@amt))"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="number(sum($pAmount[@ccy=$pCcy]/@amt))"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:otherwise>
            <xsl:choose>
              <xsl:when test="$pAmount/@side">
                <xsl:call-template name="SignAmount">
                  <xsl:with-param name="pAmount" select="$pAmount[@ccy=$pCcy]/@amt"/>
                  <xsl:with-param name="pSide" select="$pAmount/@side" />
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$pAmount[@ccy=$pCcy]/@amt"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- ................................................ -->
  <!-- GetQuantity-fmtQty                               -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Retourne la valeur formatée de la quantité      -->
  <!-- ................................................ -->
  <xsl:template name="GetQuantity-fmtQty">
    <!--Une quantité non signé ou bien une liste d'éléments avec deux attributs:fmtQty et qtyUnit -->
    <xsl:param name="pQuantity"/>
    <xsl:param name="pQtyUnit"/>

    <xsl:choose>
      <xsl:when test="string-length($pQuantity) > 0">
        <xsl:value-of select="$pQuantity"/>
      </xsl:when>
      <xsl:when test="msxsl:node-set($pQuantity)/@fmtQty">
        <xsl:choose>
          <xsl:when test="count($pQuantity) > 1">
            <xsl:value-of select="number(sum($pQuantity[@qtyUnit=$pQtyUnit]/@fmtQty))"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$pQuantity[@qtyUnit=$pQtyUnit]/@fmtQty"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- ................................................ -->
  <!-- GetAmount-side                                   -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Retourne le sens (DR/CR) d'un montant           -->
  <!-- ................................................ -->
  <xsl:template name="GetAmount-side">
    <!--Un montant signé ou bien une liste d'éléments avec deux attributs:amt et side -->
    <xsl:param name="pAmount"/>

    <xsl:variable name="vAmount" select="msxsl:node-set($pAmount)"/>

    <xsl:variable name="vSide">
      <xsl:choose>
        <xsl:when test="$vAmount and $vAmount/@amt and count($vAmount) = 1">
          <xsl:if test="number($vAmount/@amt) != 0">
            <xsl:value-of select="$vAmount/@side"/>
          </xsl:if>
        </xsl:when>
        <xsl:when test="string-length($vAmount) > 0">
          <xsl:choose>
            <xsl:when test="number($vAmount) > 0">
              <xsl:value-of select="$gcCredit"/>
            </xsl:when>
            <xsl:when test="0 > number($vAmount)">
              <xsl:value-of select="$gcDebit"/>
            </xsl:when>
          </xsl:choose>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="contains(',Default,RightDRCR,LeftDRCR,',concat(',',$gAmount-format,','))">
        <xsl:value-of select="$vSide"/>
      </xsl:when>
      <xsl:when test="contains(',RightDR,LeftDR,',concat(',',$gAmount-format,','))">
        <xsl:if test="$vSide = $gcDebit">
          <xsl:value-of select="$vSide"/>
        </xsl:if>
      </xsl:when>
      <xsl:when test="contains(',RightMinusPlus,LeftMinusPlus,',concat(',',$gAmount-format,','))">
        <xsl:choose>
          <xsl:when test="$vSide = $gcDebit">
            <xsl:value-of select="'-'"/>
          </xsl:when>
          <xsl:when test="$vSide = $gcCredit">
            <xsl:value-of select="'+'"/>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="contains(',RightMinus,LeftMinus,',concat(',',$gAmount-format,','))">
        <xsl:if test="$vSide = $gcDebit">
          <xsl:value-of select="'-'"/>
        </xsl:if>
      </xsl:when>
      <xsl:when test="$gAmount-format='Parenthesis' and $vSide=$gcDebit">
        <xsl:value-of select="$vSide"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- ................................................ -->
  <!-- GetAmount-color                                  -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Retourne la couleur d'un montant                -->
  <!--  - en fonction du signe                          -->
  <!-- ................................................ -->
  <xsl:template name="GetAmount-color">
    <!--Un montant signé -->
    <xsl:param name="pAmount"/>

    <xsl:choose>
      <xsl:when test="number($pAmount) > 0">
        <xsl:value-of select="$gAmountCR-color"/>
      </xsl:when>
      <xsl:when test="0 > number($pAmount)">
        <xsl:value-of select="$gAmountDR-color"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- ................................................ -->
  <!-- GetAmount-background-color                       -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Retourne la couleur background d'un montant     -->
  <!--  - en fonction du signe                          -->
  <!-- ................................................ -->
  <xsl:template name="GetAmount-background-color">
    <!--Un montant signé -->
    <xsl:param name="pAmount"/>

    <xsl:choose>
      <xsl:when test="number($pAmount) > 0">
        <xsl:value-of select="$gAmountCR-background-color"/>
      </xsl:when>
      <xsl:when test="0 > number($pAmount)">
        <xsl:value-of select="$gAmountDR-background-color"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$gAmountZero-background-color"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- ................................................ -->
  <!-- GetExpiry-color                                  -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Retourne la couleur de l'échéance               -->
  <!-- ................................................ -->
  <xsl:template name="GetExpiry-color">
    <xsl:param name="pExpInd"/>
    <xsl:param name="pDefault-color"/>

    <xsl:if test="$gIsColorMode">
      <xsl:choose>
        <xsl:when test="$pExpInd">
          <xsl:choose>
            <xsl:when test="$gExpiryIndicator-color">
              <xsl:value-of select="$gExpiryIndicator-color"/>
            </xsl:when>
            <xsl:when test="$pDefault-color">
              <xsl:value-of select="$pDefault-color"/>
            </xsl:when>
          </xsl:choose>
        </xsl:when>
        <xsl:when test="$pDefault-color">
          <xsl:value-of select="$pDefault-color"/>
        </xsl:when>
      </xsl:choose>
    </xsl:if>
  </xsl:template>

  <!-- ...........  FxRate Tools................................................. -->
  <xsl:template name="GetExchangeRate_Repository">
    <xsl:param name="pFlowCcy"/>
    <xsl:param name="pExCcy" select="$gExchangeCcy"/>
    <xsl:param name="pFixingDate"/>
    <xsl:param name="pIsLastFxRate" select="false()"/>

    <xsl:variable name="idFxRate" select="concat($gcPrefixCcyId, $pFlowCcy)"/>
    <xsl:choose>
      <xsl:when test="$pFixingDate">
        <xsl:choose>
          <xsl:when test="$pIsLastFxRate = true()">
            <xsl:variable name="vAllFxRate">
              <xsl:for-each select="$gRepository/currency[@id=$idFxRate]/fxrate">
                <xsl:sort data-type="text" select="fixingDate/text()"/>

                <xsl:variable name="vCompare">
                  <xsl:call-template name="CompareDate">
                    <xsl:with-param name="pDate1" select="$pFixingDate"/>
                    <xsl:with-param name="pDate2" select="fixingDate/text()"/>
                  </xsl:call-template>
                </xsl:variable>
                <xsl:if test="$vCompare = 0 or $vCompare = 1">
                  <xsl:copy-of select="."/>
                </xsl:if>

              </xsl:for-each>
            </xsl:variable>
            <xsl:copy-of select="msxsl:node-set($vAllFxRate)/fxrate[last()]"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:copy-of select="$gRepository/currency[@id=$idFxRate]/fxrate[fixingDate/text() = $pFixingDate]"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:copy-of select="$gRepository/currency[@id=$idFxRate]/fxrate"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="GetExchangeAmount">
    <xsl:param name="pAmount"/>
    <xsl:param name="pExchangeRate"/>
    <xsl:param name="pFlowCcy"/>
    <xsl:param name="pExCcy" select="$gExchangeCcy"/>

    <xsl:choose>
      <xsl:when test="$pFlowCcy = $pExCcy"/>
      <xsl:when test="$pExchangeRate/rate">

        <xsl:variable name="vQuoteBasis" select="$pExchangeRate/quotedCurrencyPair/quoteBasis/text()"/>
        <xsl:variable name="vCcy1" select="$pExchangeRate/quotedCurrencyPair/currency1/text()"/>
        <xsl:variable name="vCcy2" select="$pExchangeRate/quotedCurrencyPair/currency2/text()"/>
        <xsl:variable name="vRate" select="$pExchangeRate/rate"/>

        <xsl:choose>
          <xsl:when test="$vQuoteBasis='Currency1PerCurrency2'">
            <xsl:choose>
              <xsl:when test="$pExCcy=$vCcy1">
                <xsl:value-of select="number($pAmount) * number($vRate)"/>
              </xsl:when>
              <xsl:when test="$pExCcy=$vCcy2">
                <xsl:value-of select="number($pAmount) div number($vRate)"/>
              </xsl:when>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="$vQuoteBasis='Currency2PerCurrency1'">
            <xsl:choose>
              <xsl:when test="$pExCcy=$vCcy1">
                <xsl:value-of select="number($pAmount) div number($vRate)"/>
              </xsl:when>
              <xsl:when test="$pExCcy=$vCcy2">
                <xsl:value-of select="number($pAmount) * number($vRate)"/>
              </xsl:when>
            </xsl:choose>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- ...........  Repository Tools............................................. -->
  <xsl:template name="GetUNLRepository">
    <xsl:param name="pRepository-derivativeContract"/>

    <xsl:choose>
      <xsl:when test="contains(',ExchangeTradedContract,Future,',concat(',',$pRepository-derivativeContract/assetCategory,','))">
        <xsl:copy-of select="$gRepository/derivativeContract[@OTCmlId=$pRepository-derivativeContract/idDC_Unl]"/>
      </xsl:when>
      <xsl:when test="$pRepository-derivativeContract/assetCategory='EquityAsset'">
        <xsl:copy-of select="$gRepository/equity[@OTCmlId=$pRepository-derivativeContract/idAsset_Unl]"/>
      </xsl:when>
      <xsl:when test="$pRepository-derivativeContract/assetCategory='Index'">
        <xsl:copy-of select="$gRepository/index[@OTCmlId=$pRepository-derivativeContract/idAsset_Unl]"/>
      </xsl:when>
      <xsl:when test="$pRepository-derivativeContract/assetCategory='RateIndex'">
        <xsl:copy-of select="$gRepository/rateIndex[@OTCmlId=$pRepository-derivativeContract/idAsset_Unl]"/>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- ...........  Prices Tools ................................................ -->
  <!-- ................................................ -->
  <!-- GetPricesPattern                                 -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Retourne le pattern à appliquer à la liste des prix, en considérant la précision max -->
  <!-- ................................................ -->
  <xsl:template name="GetPricesPattern">
    <xsl:param name="pPrices"/>
    <xsl:param name="pHighPrec" select="number(2)"/>

    <xsl:variable name="vPricesHighestPrec">
      <xsl:call-template name="GetHighestPrecision">
        <xsl:with-param name="pPrices" select="$pPrices"/>
        <xsl:with-param name="pHighPrec" select="$pHighPrec"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:call-template name="GetPricePattern">
      <xsl:with-param name="pPrec" select="$vPricesHighestPrec"/>
    </xsl:call-template>
  </xsl:template>
  <!-- ................................................ -->
  <!-- GetPricesPattern                                 -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Retourne le pattern à appliquer en fonction de la précision -->
  <!-- ................................................ -->
  <xsl:template name="GetPricePattern">
    <xsl:param name="pPrec"/>

    <xsl:choose>
      <xsl:when test="number($pPrec) = 0">
        <xsl:value-of select="$numberPatternNoZero"/>
      </xsl:when>
      <xsl:when test="number($pPrec) = 1">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$number2DecPattern"/>
          <xsl:with-param name="oldValue" select="'00'"/>
          <xsl:with-param name="newValue" select="'0'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="number($pPrec) = 3">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$number2DecPattern"/>
          <xsl:with-param name="oldValue" select="'00'"/>
          <xsl:with-param name="newValue" select="'000'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="number($pPrec) = 4">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$number2DecPattern"/>
          <xsl:with-param name="oldValue" select="'00'"/>
          <xsl:with-param name="newValue" select="'0000'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="number($pPrec) = 5">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$number2DecPattern"/>
          <xsl:with-param name="oldValue" select="'00'"/>
          <xsl:with-param name="newValue" select="'00000'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="number($pPrec) = 6">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$number2DecPattern"/>
          <xsl:with-param name="oldValue" select="'00'"/>
          <xsl:with-param name="newValue" select="'000000'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="number($pPrec) = 7">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$number2DecPattern"/>
          <xsl:with-param name="oldValue" select="'00'"/>
          <xsl:with-param name="newValue" select="'0000000'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="number($pPrec) = 8">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$number2DecPattern"/>
          <xsl:with-param name="oldValue" select="'00'"/>
          <xsl:with-param name="newValue" select="'00000000'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="number($pPrec) = 9">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$number2DecPattern"/>
          <xsl:with-param name="oldValue" select="'00'"/>
          <xsl:with-param name="newValue" select="'000000000'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$number2DecPattern"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>
  <!-- ................................................ -->
  <!-- GetPricesPattern                                 -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Retourne la précision max de la liste des prix-->
  <xsl:template name="GetHighestPrecision">
    <xsl:param name="pPrices"/>
    <xsl:param name="pHighPrec" select="number(2)"/>

    <xsl:choose>
      <xsl:when test="count($pPrices) > number(0)">
        <xsl:variable name="vFirstDecimalPortion" select="substring-after($pPrices[1],'.')"/>
        <xsl:variable name="vFirstPrec" select="string-length($vFirstDecimalPortion)"/>

        <xsl:variable name="vHighPrec">
          <xsl:choose>
            <xsl:when test="$vFirstPrec > $pHighPrec">
              <xsl:value-of select="$vFirstPrec"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$pHighPrec"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:call-template name="GetHighestPrecision">
          <xsl:with-param name="pPrices" select="$pPrices[position() > 1]"/>
          <xsl:with-param name="pHighPrec" select="number($vHighPrec)"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pHighPrec"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- ...........  Subtotals Tools.............................................. -->
  <xsl:template name="GetSubTotal_Amounts">
    <xsl:param name="pAmount"/>

    <xsl:if test="$pAmount/@amt">
      <xsl:value-of select="number(sum($pAmount[@side=$gcCredit]/@amt) - sum($pAmount[@side=$gcDebit]/@amt))"/>
    </xsl:if>

  </xsl:template>



  <!-- ................................................ -->
  <!-- Vertical                                         -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!--  Retourne autant de block qu'il existe de lettre -->
  <xsl:template name="Vertical">
    <xsl:param name="pVal"/>
    <xsl:param name="pLen"/>
    <xsl:param name="pPos" select="0"/>
    <xsl:choose>
      <xsl:when test="$pPos != $pLen">
        <fo:block>
          <xsl:value-of select="substring($pVal,$pPos + 1,1)"/>
        </fo:block>
        <xsl:call-template name="Vertical">
          <xsl:with-param name="pVal">
            <xsl:value-of select="$pVal"/>
          </xsl:with-param>
          <xsl:with-param name="pLen">
            <xsl:value-of select="$pLen"/>
          </xsl:with-param>
          <xsl:with-param name="pPos">
            <xsl:value-of select="$pPos + 1"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:when>
    </xsl:choose>
  </xsl:template>


  <!-- ................................................ -->
  <!-- SplitCrLf                                        -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!-- Mise en place d'une retour à la ligne ( <fo:block/>) à chaque présence des caractères &#xD;&#xA; (valeurs Hexa de 'carriage return'  et  'line feed') -->
  <!-- FI 20151016 [21458] Add -->
  <xsl:template name="SplitCrLf">
    <!--Text en entrée contenant potentiellement  &#xD;&#xA; (Représentant un retour chariot)  -->
    <xsl:param name="pText" select="."/>
    <!-- Si renseigné => Représente la taille maximum -->
    <xsl:param name="pDataLength" select ="number('0')"/>
    <xsl:if test="string-length($pText)">

      <xsl:if test="not($pText=.)">
        <fo:block>
          <xsl:call-template name="DisplayData_Format">
            <xsl:with-param name="pData" select="substring-before(concat($pText,'&#xD;&#xA;'),'&#xD;&#xA;')"/>
            <xsl:with-param name="pDataLength" select="$pDataLength"/>
          </xsl:call-template>
        </fo:block>
      </xsl:if>

      <xsl:call-template name="SplitCrLf">
        <xsl:with-param name="pText" select="substring-after($pText, '&#xD;&#xA;')"/>
        <xsl:with-param name="pDataLength" select="$pDataLength"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <!-- ................................................................ -->
  <!-- GetCellUnderlying                                                -->
  <!-- ................................................................ -->
  <!-- Summary :                                                        -->
  <!-- Retourne une cellule alimentée avec la description de l'asset    -->
  <!-- (number-columns-spanned 3)                                       -->
  <!-- .................................................................-->
  <!-- FI 20151019 [21317] Add -->
  <xsl:template name ="GetCellUnderlying">
    <xsl:param name ="pRepository_Asset"/>
    <xsl:param name ="pAdditionalInfo"/>
    <xsl:param name ="pBizTrade" />
    <xsl:param name ="pNumberRowsSpanned" select ="1" />

    <xsl:variable name ="vAsssetName" select ="name($pRepository_Asset)"/>

    <fo:table-cell padding-top="{$gData_padding}" padding-bottom="{$gData_padding}"
                   display-align="{$gBlockSettings_Data/column[@name='Underlying']/data/@display-align}"
                   text-align="{$gBlockSettings_Data/column[@name='Underlying']/data/@text-align}"
                   number-columns-spanned="3" number-rows-spanned="{$pNumberRowsSpanned}">
      <xsl:call-template name="Debug_border-blue"/>

      <xsl:choose>
        <xsl:when test ="$vAsssetName='debtSecurity'">
          <fo:block>
            <xsl:call-template name="DisplayData_Truncate">
              <xsl:with-param name="pData" select="normalize-space($pRepository_Asset/altIdentifier)"/>
              <xsl:with-param name="pDataLength" select="$gBlockSettings_Data/column[@name='Underlying']/data/@length"/>
            </xsl:call-template>
          </fo:block>
          <fo:block>
            <fo:inline font-weight="{$gBlockSettings_Data/column[@name='Underlying']/data[@name='det']/@font-weight}">
              <xsl:value-of select="$pRepository_Asset/isinCode"/>
              <xsl:if test ="string-length($pRepository_Asset/sedolCode) > 0">
                <xsl:value-of select="concat(' (SEDOL: ', normalize-space($pRepository_Asset/sedolCode),')')"/>
              </xsl:if>
            </fo:inline>
          </fo:block>
        </xsl:when>
        <xsl:when test ="$vAsssetName='commodity'">
          <fo:block>
            <xsl:call-template name="DisplayData_Truncate">
              <xsl:with-param name="pData" select="normalize-space($pRepository_Asset/contractSymbol)"/>
              <xsl:with-param name="pDataLength" select="$gBlockSettings_Data/column[@name='Underlying']/data/@length"/>
            </xsl:call-template>
            <xsl:if test="string-length($pRepository_Asset/duration) > 0">
              <xsl:variable name="vExtValue" select="$gRepository/enums[@id='ENUMS.CODE.SettlementPeriodDurationEnum']/enum[value/text()=$pRepository_Asset/duration]/extvalue/text()"/>
              <xsl:choose>
                <xsl:when test="string-length($vExtValue) > 0">
                  <xsl:value-of select="concat(' (',$vExtValue,')')"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="concat(' (',$pRepository_Asset/duration,')')"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:if>
          </fo:block>
          <fo:block>
            <fo:inline font-weight="{$gBlockSettings_Data/column[@name='Underlying']/data[@name='det']/@font-weight}">
              <xsl:variable name="vCommonTrade" select="$gCommonData/trade[@tradeId=$pBizTrade/@tradeId]"/>
              <xsl:variable name="vTimeFormat">
                <xsl:call-template name="GetTimeFormat">
                  <xsl:with-param name="pTimeFormat" select="$gBlockSettings_Data/column[@name='Underlying']/data[@name='det']/@time-format" />
                  <xsl:with-param name="pIsDisplay" select="false()" />
                </xsl:call-template>
              </xsl:variable>
              <xsl:value-of select="concat($gBlockSettings_Data/column[@name='Underlying']/data/@resource-Dlv,': ')"/>
              <xsl:call-template name="DisplayData_Format">
                <xsl:with-param name="pData" select="$vCommonTrade/@dlvStart"/>
                <xsl:with-param name="pDataType" select="'Date'"/>
              </xsl:call-template>
              <xsl:value-of select="concat(' ',$gBlockSettings_Data/column[@name='Underlying']/data/@resource-TimeFrom,' ')"/>
              <xsl:call-template name="DisplayData_Format">
                <xsl:with-param name="pData" select="$vCommonTrade/@dlvStart"/>
                <xsl:with-param name="pDataType" select="'Time'"/>
                <xsl:with-param name="pDataFormat" select="$vTimeFormat"/>
              </xsl:call-template>
              <xsl:value-of select="concat(' ',$gBlockSettings_Data/column[@name='Underlying']/data/@resource-TimeTo,' ')"/>
              <xsl:call-template name="DisplayData_Format">
                <xsl:with-param name="pData" select="$vCommonTrade/@dlvEnd"/>
                <xsl:with-param name="pDataType" select="'Time'"/>
                <xsl:with-param name="pDataFormat" select="$vTimeFormat"/>
              </xsl:call-template>
              <xsl:if test ="substring-before($vCommonTrade/@dlvStart,'T') != substring-before($vCommonTrade/@dlvEnd,'T')">
                <fo:inline font-size="{$gBlockSettings_Data/column[@name='Underlying']/data[@name='det']/@font-size}">
                  <xsl:value-of select="' (T+1)'"/>
                </fo:inline>
              </xsl:if>
            </fo:inline>
          </fo:block>
        </xsl:when>
        <xsl:otherwise>
          <fo:block>
            <xsl:call-template name="Display_Underlyer">
              <xsl:with-param name="pRepository-asset" select="$pRepository_Asset"/>
              <xsl:with-param name="pColumnSettings" select="$gBlockSettings_Data/column[@name='Underlying']"/>
            </xsl:call-template>
          </fo:block>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:if test="string-length($pAdditionalInfo)>0">
        <fo:block>
          <fo:inline font-weight="{$gBlockSettings_Data/column[@name='Underlying']/data[@name='det']/@font-weight}">
            <xsl:value-of select="$pAdditionalInfo"/>
          </fo:inline>
        </fo:block>
      </xsl:if>
    </fo:table-cell>
  </xsl:template>

  <!-- .......................................................................... -->
  <!--              XML Tools                                                     -->
  <!-- .......................................................................... -->

  <!-- ................................................ -->
  <!-- Delete                                           -->
  <!-- ................................................ -->
  <!-- Summary :                                        -->
  <!-- Supprime un ou plusieurs noeud d'un flux XML-->
  <!-- RD 20190619 [23912] Add -->
  <xsl:template match="node()" mode="delete">
    <xsl:param name="pDelete"/>

    <xsl:copy>
      <xsl:copy-of select="@*"/>
      <xsl:apply-templates select="node()[contains($pDelete,concat(',',name(),','))=false()]" mode="delete">
        <xsl:with-param name="pDelete" select="$pDelete" />
      </xsl:apply-templates>
    </xsl:copy>
  </xsl:template>


</xsl:stylesheet>
