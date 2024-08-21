<!--
=========================================================================================
 Summary : Import Eurex Referential                           
 File    : MarketData_Eurex_Import_Map.xsl              
=========================================================================================
 Version : v13.0.0.0                                           
 Date    : 20230726
 Author  : PL
 Comment : [XXXXX] - Use new file ProductList
=========================================================================================
 Version : v3.2.0.0                                           
 Date    : 20130222
 Author  : FL
 Comment : Exclusion de l’importation des DC ayant comme ContractSymbol
           FEUH, FEUM, FEUU, FEUZ, DOUF ou SINF
=========================================================================================
 Version : v1.0.1.0                                           
 Date    : 20100127                                           
 Author  : MF                                                 
 Comment : Import Eurex Referential File                   
=========================================================================================
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- Includes-->
  <xsl:include href="MarketData_EurexCommon_Import_Map.xsl"/>

  <!--Main template  -->
  <xsl:template match="/iotask">
    <iotask>
      <xsl:call-template name="IOTaskAtt"/>
      <xsl:apply-templates select="parameters"/>
      <xsl:apply-templates select="iotaskdet"/>
    </iotask>
  </xsl:template>

  <!-- Specific template-->
  <xsl:template match="file">
    <file>
      <xsl:call-template name="IOFileAtt"/>

      <xsl:variable name="vTradingStartsDateTime">
        <xsl:value-of select="normalize-space(row[@status='success']/data[@name='TSD'])"/>
      </xsl:variable>

      <xsl:apply-templates select="row">
        <!--<xsl:with-param name="pTradingStartsDateTime" select="$vTradingStartsDateTime"/>-->
      </xsl:apply-templates>

    </file>
  </xsl:template>

  <xsl:template match="row">
    <!--<xsl:param name="pTradingStartsDateTime"/>-->

    <!-- FL 20130221 : Exclusion des DC ayant comme ContractSymbol FEUH, FEUM, FEUU, FEUZ, DOUF, ou SINF car 
                        - Ces DC ne sont pas dans le fichier alimentant EUREXPRODUCT
                        - Aucun info sur ces DC sur le site de l'Eurex -->
    <!-- xsl:if test="data[contains(@status, 'success')] and data[contains(@name, 'MC')]
                  and data[@name='FMC']!='FEUH' and data[@name='FMC']!='FEUM'
                  and data[@name='FMC']!='FEUU' and data[@name='FMC']!='FEUZ'
                  and data[@name='FMC']!='DOUF' and data[@name='FMC']!='SINF'" -->
    <!-- PL 20231009 Refactoring, ces 6 symboles n'existent plus -->
      <xsl:if test="data[contains(@status, 'success')] and data[contains(@name, 'MC')]">
        <row>
        <xsl:call-template name="IORowAtt"/>
        <xsl:call-template name="rowStream">
          <!--<xsl:with-param name="pTradingStartsDateTime" select="$pTradingStartsDateTime"/>-->
        </xsl:call-template>
      </row>

    </xsl:if>

  </xsl:template>

  <!-- Spécifique à chaque Import -->
  <xsl:template name="rowStream">
    <!--<xsl:param name="pTradingStartsDateTime"/>-->

    <xsl:call-template name="rowStreamCommon">
      <!--<xsl:with-param name="pTradingStartsDateTime" select="$pTradingStartsDateTime"/>-->
    </xsl:call-template>

  </xsl:template>

</xsl:stylesheet>
