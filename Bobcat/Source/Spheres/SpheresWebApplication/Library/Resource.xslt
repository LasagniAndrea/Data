<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt" version="1.0">

	<!--
	-->
  <xsl:variable name="gcResourceHref" select="'HREF='"/>
  
	<xsl:variable name="PathResource">
		<xsl:value-of select="'..\Resource'"/>
	</xsl:variable>

  <xsl:variable name="varPathResource">
    <xsl:value-of select="'..\Resource'"/>
  </xsl:variable>

  <xsl:variable name="varPathIsdaResource">
		<xsl:value-of select="'..\Resource'"/>
	</xsl:variable>

	<xsl:variable name="varPathCustomResource">
		<xsl:value-of select="'..\Resource'"/>
	</xsl:variable>

	<!-- Variable spheresFileResource: it produce the correct name of the spheres file resource by the param CurrentCulture -->
	<xsl:variable name="varFileResource">
		<xsl:value-of select="$varPathResource"/>
		<xsl:text>\SpheresResource.</xsl:text>
		<xsl:choose>
			<xsl:when test = "$pCurrentCulture = 'fr-FR'">
				<xsl:text>fr.resx</xsl:text>
			</xsl:when>
			<xsl:when test = "$pCurrentCulture = 'fr-BE'"> 
				<xsl:text>fr.resx</xsl:text>
			</xsl:when>
      <xsl:when test = "$pCurrentCulture = 'en-GB'">
        <xsl:text>resx</xsl:text>
      </xsl:when>
      <xsl:otherwise>
				<xsl:value-of select="$pCurrentCulture"/>
        <xsl:text>.resx</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>

  <xsl:variable name="varResource" select="document($varFileResource)//root/data"/>
  
	<!-- Variable spheresFileResource: it produce the correct name of the ISDA spheres file resource by the param CurrentCulture -->
	<xsl:variable name="varISDAResourceFile">
		<xsl:value-of select="$varPathIsdaResource"/>
		<xsl:text>\ISDA_Resource.</xsl:text>
		<xsl:choose>
			<xsl:when test = "$pCurrentCulture = 'fr-FR'">
				<xsl:text>fr.resx</xsl:text>
			</xsl:when>
			<xsl:when test = "$pCurrentCulture = 'fr-BE'">
				<xsl:text>fr.resx</xsl:text>
			</xsl:when>
      <xsl:when test = "$pCurrentCulture = 'en-GB'">
        <xsl:text>resx</xsl:text>
      </xsl:when>
      <xsl:otherwise>
				<xsl:value-of select="$pCurrentCulture"/>
        <xsl:text>.resx</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>

	<!-- Variable customFileResource: it produce the correct name of the custom file resource by the param CurrentCulture -->
	<xsl:variable name="varCustomResourceFile">
		<xsl:value-of select="$varPathCustomResource"/>
		<xsl:text>\Custom_Resource.</xsl:text>
		<xsl:choose>
			<xsl:when test = "$pCurrentCulture = 'fr-FR'">
				<xsl:text>fr.resx</xsl:text>
			</xsl:when>
			<xsl:when test = "$pCurrentCulture = 'fr-BE'">
				<xsl:text>fr.resx</xsl:text>
			</xsl:when>
      <xsl:when test = "$pCurrentCulture = 'en-GB'">
        <xsl:text>resx</xsl:text>
      </xsl:when>
      <xsl:otherwise>
				<xsl:value-of select="$pCurrentCulture"/>
        <xsl:text>.resx</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>

   
	<!-- Template getSpheresTranslation: it selects the Custom Translation if Exist -->
	<xsl:template name="getSpheresTranslation">
		<xsl:param name="pResourceName"/>
		
    <!-- First look for the translation in the custom resource file -->
    <xsl:variable name="vCustomResValue">
      <xsl:value-of select="document($varCustomResourceFile)/root/data[@name=$pResourceName]/value"/>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test = "string-length($vCustomResValue) > 0 ">
        <xsl:choose>
          <xsl:when test = "substring( $vCustomResValue, 1, string-length($gcResourceHref) ) = $gcResourceHref">
            <!-- Look for the translation in the custom resource file of HREF resource name-->
            <xsl:variable name="vCustomResHref">
              <xsl:value-of select="substring( $vCustomResValue, string-length($gcResourceHref) + 1)"/>
            </xsl:variable>
            <xsl:variable name="vCustomResValueHref">
              <xsl:value-of select="document($varCustomResourceFile)/root/data[@name=$vCustomResHref]/value"/>
            </xsl:variable>
            <xsl:choose>
              <xsl:when test = "string-length($vCustomResValueHref) > 0 ">
                <xsl:value-of select="$vCustomResValueHref"/>
              </xsl:when>
              <!-- otherwise display the resource name -->
              <xsl:otherwise>
                <xsl:value-of select="$vCustomResValue"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <!-- otherwise display the resource name -->
          <xsl:otherwise>
            <xsl:value-of select="$vCustomResValue"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <!-- Second look for the translation in the common culture resource file -->
        <xsl:variable name="vFileResValue">
          <xsl:value-of select="document($varFileResource)/root/data[@name=$pResourceName]/value"/>
        </xsl:variable>
        <xsl:choose>
          <xsl:when test = "string-length($vFileResValue) > 0 ">
            <xsl:choose>
              <xsl:when test = "substring( $vFileResValue, 1, string-length($gcResourceHref)) = $gcResourceHref">
                <!-- Look for the translation in the common culture resource file of HREF resource name-->
                <xsl:variable name="vFileResHref">
                  <xsl:value-of select="substring( $vFileResValue, string-length($gcResourceHref) + 1)"/>
                </xsl:variable>
                <xsl:variable name="vFileResValueHref">
                  <xsl:value-of select="document($varFileResource)/root/data[@name=$vFileResHref]/value"/>
                </xsl:variable>
                <xsl:choose>
                  <xsl:when test = "string-length($vFileResValueHref) > 0 ">
                    <xsl:value-of select="$vFileResValueHref"/>
                  </xsl:when>
                  <!-- otherwise display the resource name -->
                  <xsl:otherwise>
                    <xsl:value-of select="$vFileResValue"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>
              <!-- otherwise display the resource name -->
              <xsl:otherwise>
                <xsl:value-of select="$vFileResValue"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <!-- otherwise display the resource name -->
          <xsl:otherwise>
            <xsl:value-of select="$pResourceName"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
	</xsl:template>

	<!-- Template getTranslation: it selects the Custom Translation if Exist -->
	<xsl:template name="getTranslation">
		<xsl:param name="pResourceName"/>
		<xsl:choose>
			<!-- First look for the translation in the custom resource file -->
			<xsl:when test = "document($varCustomResourceFile)/root/data[@name=$pResourceName]/value">
				<xsl:value-of select="document($varCustomResourceFile)/root/data[@name=$pResourceName]/value"/>
			</xsl:when>
			<!-- second look for the translation in the ISDA standard resource file -->
			<xsl:when test = "document($varISDAResourceFile)/root/data[@name=$pResourceName]/value">
				<xsl:value-of select="document($varISDAResourceFile)/root/data[@name=$pResourceName]/value"/>
			</xsl:when>
			<!-- otherwise display the resource name -->
			<xsl:otherwise>
				<xsl:value-of select="$pResourceName"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	

	<xsl:variable name="varGlobalFileResource">
		<xsl:value-of select="$varPathResource"/>
		<xsl:text>\SpheresResource.resx</xsl:text>
	</xsl:variable>


	<xsl:variable name="varGlobalResource" select="document($varGlobalFileResource)//root/data"/>

	<xsl:decimal-format name="de-DE" decimal-separator="," grouping-separator="." percent="%" />
	<xsl:decimal-format name="en-GB" decimal-separator="." grouping-separator="," percent="%" />
	<xsl:decimal-format name="es-ES" decimal-separator="," grouping-separator="." percent="%" />
	<xsl:decimal-format name="fr-BE" decimal-separator="," grouping-separator="." percent="%" />
	<xsl:decimal-format name="fr-FR" decimal-separator="," grouping-separator=" " percent="%" />
	<xsl:decimal-format name="it-IT" decimal-separator="," grouping-separator="." percent="%" />

	<xsl:variable name="defaultCulture">
		<xsl:choose>
			<xsl:when test = "$pCurrentCulture = 'de-DE'">
				<xsl:value-of select="$pCurrentCulture"/>
			</xsl:when>
			<xsl:when test = "$pCurrentCulture = 'en-GB'">
				<xsl:value-of select="$pCurrentCulture"/>
			</xsl:when>
			<xsl:when test = "$pCurrentCulture = 'es-ES'">
				<xsl:value-of select="$pCurrentCulture"/>
			</xsl:when>
			<xsl:when test = "$pCurrentCulture = 'fr-BE'">
				<xsl:value-of select="$pCurrentCulture"/>
			</xsl:when>
			<xsl:when test = "$pCurrentCulture = 'fr-FR'">
				<xsl:value-of select="$pCurrentCulture"/>
			</xsl:when>
			<xsl:when test = "$pCurrentCulture = 'it-IT'">
				<xsl:value-of select="$pCurrentCulture"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="'en-GB'"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>

	<xsl:variable name="amountPattern">
		<xsl:choose>
			<xsl:when test = "$pCurrentCulture = 'de-DE'">
				<xsl:value-of select="'#.###.##0,00'"/>
			</xsl:when>
			<xsl:when test = "$pCurrentCulture = 'en-GB'">
				<xsl:value-of select="'#,###,##0.00'"/>
			</xsl:when>
			<xsl:when test = "$pCurrentCulture = 'es-ES'">
				<xsl:value-of select="'#.###.##0,00'"/>
			</xsl:when>
			<xsl:when test = "$pCurrentCulture = 'fr-BE'">
				<xsl:value-of select="'#.###.##0,00'"/>
			</xsl:when>
			<xsl:when test = "$pCurrentCulture = 'fr-FR'">
				<xsl:value-of select="'# ### ##0,00'"/>
			</xsl:when>
			<xsl:when test = "$pCurrentCulture = 'it-IT'">
				<xsl:value-of select="'#.###.##0,00'"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="'#,###,##0.00'"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
  
	<xsl:variable name="ratePattern">
		<xsl:choose>
			<xsl:when test = "$pCurrentCulture = 'de-DE'">
				<xsl:value-of select="'#.###.##0,00########'"/>
			</xsl:when>
			<xsl:when test = "$pCurrentCulture = 'en-GB'">
				<xsl:value-of select="'#,###,##0.00########'"/>
			</xsl:when>
			<xsl:when test = "$pCurrentCulture = 'es-ES'">
				<xsl:value-of select="'#.###.##0,00########'"/>
			</xsl:when>
			<xsl:when test = "$pCurrentCulture = 'fr-BE'">
				<xsl:value-of select="'#.###.##0,00########'"/>
			</xsl:when>
			<xsl:when test = "$pCurrentCulture = 'fr-FR'">
				<xsl:value-of select="'# ### ##0,00########'"/>
			</xsl:when>
			<xsl:when test = "$pCurrentCulture = 'it-IT'">
				<xsl:value-of select="'#.###.##0,00########'"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="'#,###,##0.00########'"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>

	<!--<xsl:variable name="separatorTime">
		<xsl:choose>
			<xsl:when test = "$pCurrentCulture = 'it-IT'">
				<xsl:value-of select="'.'"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="':'"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>-->
  <xsl:variable name="separatorTime" select="':'"/>

  <!-- **************************************************************************** -->
	<!-- Template DEBUG                                                               -->
	<!--                                                                              -->
	<!-- **************************************************************************** -->
	<xsl:param name="pDebugXsl" select="'0'"/>

	<!-- **************************************************************************** -->
	<!-- Template DEBUG                                                               -->
	<!--                                                                              -->
	<!-- displayParentNode                                                            -->
	<!-- Template utilisant la récursivité pour afficher le parent du noeud courant   -->
	<!-- **************************************************************************** -->
	<xsl:template name="displayParentNode">
		<xsl:param name="pCurrentNode"/>
		<xsl:param name="pParentNodeName"/>

		<xsl:if test="string-length( name( $pCurrentNode/parent::node()) ) &gt; 1 ">
			<xsl:call-template name="displayParentNode">
				<xsl:with-param name="pCurrentNode" select="$pCurrentNode/parent::node()"/>
				<xsl:with-param name="pParentNodeName" select="concat( name( $pCurrentNode/parent::node()),'\', $pParentNodeName) "/>
			</xsl:call-template>
		</xsl:if>

		<xsl:if test="string-length( name( $pCurrentNode/parent::node()) ) &lt; 1 ">
			<xsl:value-of select="$pParentNodeName"/>
		</xsl:if>
	</xsl:template>

	<!-- **************************************************************************** -->
	<!-- Template DEBUG                                                               -->
	<!--                                                                              -->
	<!-- DebugXsl                                                                     -->
	<!-- Template Affichant le nom complet + la valeur du noeud courant               -->
	<!-- **************************************************************************** -->
	<xsl:template name="debugXsl">
		<xsl:param name="pCurrentNode"/>

		<xsl:if test="$pDebugXsl = '1' ">
			<span style="color:Blue; font-weight:bold">
				&lt;Node:
				<xsl:call-template name="displayParentNode">
					<xsl:with-param name="pCurrentNode" select="$pCurrentNode"/>
					<xsl:with-param name="pParentNodeName" select="name($pCurrentNode)"/>
				</xsl:call-template>&gt;
			</span>
			<span style="color:Red; font-weight:bold">
				&lt;Value:
				<xsl:value-of select="string($pCurrentNode)"/>&gt;
			</span>
		</xsl:if>
	</xsl:template>


	<!-- ************* -->
	<!-- format-fxrate -->
	<!-- ************* -->
	<xsl:template name="format-fxrate">
		<xsl:param name="fxrate"/>

		<!-- ***************************************************** -->
		<!-- Call Mode DEBUG                                       -->
		<!-- ***************************************************** -->
		<xsl:call-template name="debugXsl">
			<xsl:with-param name="pCurrentNode" select="$fxrate"/>
		</xsl:call-template>

		<xsl:value-of select="format-number(number($fxrate), $ratePattern, $defaultCulture)" />
	</xsl:template>

	<!-- *********************************** -->
	<!-- format-fixed-rate : 0.06 to 6.00 %  -->
	<!-- *********************************** -->
	<xsl:template name="format-fixed-rate">
		<xsl:param name="fixed-rate"/>

		<!-- ***************************************************** -->
		<!-- Call Mode DEBUG                                       -->
		<!-- ***************************************************** -->
		<xsl:call-template name="debugXsl">
			<xsl:with-param name="pCurrentNode" select="$fixed-rate"/>
		</xsl:call-template>

		<xsl:value-of select="format-number(number($fixed-rate)*100, $ratePattern, $defaultCulture)" />
		<xsl:text> %</xsl:text>
	</xsl:template>
	<!-- *********************************** -->
	<!-- format-fixed-rate2: 0.06 to 6.00%   -->
	<!-- note: the percent symbol is stuck ot the number -->
	<!-- *********************************** -->
  <xsl:template name="format-fixed-rate2">
    <xsl:param name="fixed-rate"/>

    <!-- ***************************************************** -->
    <!-- Call Mode DEBUG                                       -->
    <!-- ***************************************************** -->
    <xsl:call-template name="debugXsl">
      <xsl:with-param name="pCurrentNode" select="$fixed-rate"/>
    </xsl:call-template>

    <xsl:choose>
      <xsl:when test="string-length($fixed-rate)>0">
        <xsl:value-of select="format-number(number($fixed-rate)*100, $ratePattern, $defaultCulture)" />
        <xsl:text>%</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>N/A</xsl:text>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

	<!-- ******************************************************************************************* -->
	<!-- format-money - return the appropriate format money in reference to the specified culture    -->
	<!-- This template use HTML tags -->
	<!-- ******************************************************************************************* -->

	<xsl:template name="format-money">
		<xsl:param name="currency"/>
		<xsl:param name="amount"/>

		<xsl:call-template name="debugXsl">
			<xsl:with-param name="pCurrentNode" select="$currency"/>
		</xsl:call-template>
		<xsl:call-template name="debugXsl">
			<xsl:with-param name="pCurrentNode" select="$amount"/>
		</xsl:call-template>

		<table cellpadding="0" cellspacing="0" align="left" border="0" bgcolor="white">
			<tr>
				<td>
					<xsl:value-of select="$currency" />
				</td>
				<td>&#160;&#160;</td>
				<td align="right">
					<xsl:value-of select="format-number($amount, $amountPattern, $defaultCulture)" />
				</td>
			</tr>
		</table>
	</xsl:template>

	<!-- ******************************************************************************************* -->
	<!-- format-money - return the appropriate format money in reference to the specified culture    -->
	<!-- No HTML tags -->
	<!-- ******************************************************************************************* -->
  <xsl:template name="format-money2">
    <xsl:param name="currency"/>
    <xsl:param name="amount"/>

    <!-- EG 20160404 Migration vs2013 -->
    <!--<xsl:value-of select="$currency" />&#160;&#160;<xsl:value-of select="format-number($amount, $amountPattern, $defaultCulture)" />-->
    <xsl:choose>
      <xsl:when test="string-length($currency)>0">
        <xsl:value-of select="$currency" />&#160;&#160;<xsl:value-of select="format-number($amount, $amountPattern, $defaultCulture)" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="format-number($amount, $amountPattern, $defaultCulture)" />
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

	<!-- GS 22092009
	it returns the currency -->
	<xsl:template name="getCurrency">
		<xsl:param name="currency"/>
		<xsl:value-of select="$currency" />
	</xsl:template>

	<!-- GS 22092009
	it returns the amount without currency -->
	<xsl:template name="getAmountWithoutCurrency">
		<xsl:param name="amount"/>
		<xsl:value-of select="format-number($amount, $amountPattern, $defaultCulture)" />
	</xsl:template>
  
	<!-- ************************************************************************************************* -->
	<!-- getBCFullNames  - return all business centers                                                     -->
	<!-- ************************************************************************************************* -->
	<xsl:template name="getBCFullNames">
		<xsl:param name="pBCs"/>
		<xsl:call-template name="getFullNameBC">
			<xsl:with-param name="pBC" select="$pBCs/businessCenter[1]"/>
		</xsl:call-template>
		<xsl:for-each select="$pBCs/businessCenter">
			<xsl:if test="position()!=1 ">
				<!--
				GS 11/06/2009
				if the 2th businness center is blank don't dipslay the word 'and'
				-->
				<xsl:if test="$pBCs/businessCenter[2]!= '' ">
					<xsl:text> and </xsl:text>
					<xsl:call-template name="getFullNameBC">
						<xsl:with-param name="pBC" select="."/>
					</xsl:call-template>
				</xsl:if>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>

	<!-- *************************************************************************************************** -->
	<!-- getFullNameBC  - return the complete name of business center from the business center abbreviation  -->
	<!-- *************************************************************************************************** -->
	<xsl:template name="getFullNameBC">
		<xsl:param name="pBC"/>
		<xsl:param name="pIsGetFullName"/>

    
		<xsl:variable name="retValue">
			<xsl:choose>
				<xsl:when test="$pBC='ARBA'">Buenos Aires</xsl:when>
				<xsl:when test="$pBC='ATVI'">Vienna</xsl:when>
				<xsl:when test="$pBC='AUME'">Melbourne</xsl:when>
				<xsl:when test="$pBC='AUSY'">Sydney</xsl:when>
				<xsl:when test="$pBC='BEBR'">Brussels</xsl:when>
				<xsl:when test="$pBC='BGSO'">Sofia</xsl:when>
				<xsl:when test="$pBC='BRBR'">Brasilia</xsl:when>
				<xsl:when test="$pBC='BRRJ'">Rio de Janeiro</xsl:when>
				<xsl:when test="$pBC='BRSP'">Sao Paulo</xsl:when>
				<xsl:when test="$pBC='CAMO'">Montreal</xsl:when>
				<xsl:when test="$pBC='CATO'">Toronto</xsl:when>
				<xsl:when test="$pBC='CHGE'">Geneva</xsl:when>
				<xsl:when test="$pBC='CHZU'">Zurich</xsl:when>
				<xsl:when test="$pBC='CLSA'">Santiago</xsl:when>
				<xsl:when test="$pBC='CNBE'">Beijing</xsl:when>
				<xsl:when test="$pBC='COBG'">Bogota</xsl:when>
				<xsl:when test="$pBC='CZPR'">Prague</xsl:when>
				<xsl:when test="$pBC='DEFR'">Frankfurt</xsl:when>
				<xsl:when test="$pBC='DKCO'">Copenhagen</xsl:when>
				<xsl:when test="$pBC='ECGU'">Guayaquil</xsl:when>
				<xsl:when test="$pBC='EETA'">Tallinn</xsl:when>
				<xsl:when test="$pBC='EGCA'">Cairo</xsl:when>
				<xsl:when test="$pBC='ESMA'">Madrid</xsl:when>
				<xsl:when test="$pBC='EUTA'">TARGET</xsl:when>
				<xsl:when test="$pBC='FIHE'">Helsinki</xsl:when>
				<xsl:when test="$pBC='FRPA'">Paris</xsl:when>
				<xsl:when test="$pBC='GBLO'">London</xsl:when>
				<xsl:when test="$pBC='GRAT'">Athens</xsl:when>
				<xsl:when test="$pBC='HKHK'">Hong Kong</xsl:when>
				<xsl:when test="$pBC='HRZA'">Zagreb</xsl:when>
				<xsl:when test="$pBC='HUBU'">Budapest</xsl:when>
				<xsl:when test="$pBC='IDJA'">Jakarta</xsl:when>
				<xsl:when test="$pBC='ILTA'">Tel Aviv</xsl:when>
				<xsl:when test="$pBC='INMU'">Mumbai</xsl:when>
				<xsl:when test="$pBC='ITMI'">Milan</xsl:when>
				<xsl:when test="$pBC='ITRO'">Rome</xsl:when>
				<xsl:when test="$pBC='JPTO'">Tokyo</xsl:when>
				<xsl:when test="$pBC='KANA'">Nairobi</xsl:when>
				<xsl:when test="$pBC='KRSE'">Seoul</xsl:when>
				<xsl:when test="$pBC='KWKC'">Kuwait City</xsl:when>
				<xsl:when test="$pBC='KZAL'">Almaty</xsl:when>
				<xsl:when test="$pBC='LBBE'">Beirut</xsl:when>
				<xsl:when test="$pBC='LKCO'">Colombo</xsl:when>
				<xsl:when test="$pBC='LTVI'">Vilnius</xsl:when>
				<xsl:when test="$pBC='LULU'">Luxembourg</xsl:when>
				<xsl:when test="$pBC='LVRI'">Riga</xsl:when>
				<xsl:when test="$pBC='MARA'">Rabat</xsl:when>
				<xsl:when test="$pBC='MXMC'">Mexico</xsl:when>
				<xsl:when test="$pBC='MYKL'">Kuala Lumpur</xsl:when>
				<xsl:when test="$pBC='N/A'">N/A</xsl:when>
				<xsl:when test="$pBC='NLAM'">Amsterdam</xsl:when>
				<xsl:when test="$pBC='NOOS'">Oslo</xsl:when>
				<xsl:when test="$pBC='NSWE'">Wellington</xsl:when>
				<xsl:when test="$pBC='NZAU'">Auckland</xsl:when>
				<xsl:when test="$pBC='PAPC'">Panama City</xsl:when>
				<xsl:when test="$pBC='PELI'">Lima</xsl:when>
				<xsl:when test="$pBC='PHMA'">Manila</xsl:when>
				<xsl:when test="$pBC='PKKA'">Karachi</xsl:when>
				<xsl:when test="$pBC='PLWA'">Warsaw</xsl:when>
				<xsl:when test="$pBC='ROBU'">Bucharest</xsl:when>
				<xsl:when test="$pBC='RUMO'">Moscow</xsl:when>
				<xsl:when test="$pBC='SARI'">Riyadh</xsl:when>
				<xsl:when test="$pBC='SEST'">Stockholm</xsl:when>
				<xsl:when test="$pBC='SGSI'">Singapour</xsl:when>
				<xsl:when test="$pBC='SILJ'">Ljubljana</xsl:when>
				<xsl:when test="$pBC='SKBR'">Bratislava</xsl:when>
				<xsl:when test="$pBC='THBA'">Bangkok</xsl:when>
				<xsl:when test="$pBC='TRAN'">Ankara</xsl:when>
				<xsl:when test="$pBC='TWTA'">Taipei</xsl:when>
				<xsl:when test="$pBC='UAKI'">Kiev</xsl:when>
				<xsl:when test="$pBC='USCH'">Chicago</xsl:when>
				<xsl:when test="$pBC='USGS'">U.S. Government Securities</xsl:when>
				<xsl:when test="$pBC='USLA'">Los Angeles</xsl:when>
				<xsl:when test="$pBC='USNY'">New York</xsl:when>
				<xsl:when test="$pBC='USNYFED'">New York Fed</xsl:when>
				<xsl:when test="$pBC='USNYSE'">New York Stock Exchange</xsl:when>
				<xsl:when test="$pBC='VECA'">Caracas</xsl:when>
				<xsl:when test="$pBC='VNHA'">Hanoi</xsl:when>
				<xsl:when test="$pBC='ZAJO'">Johannesburg</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$pBC" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:if test="$pIsGetFullName = true()">
			<xsl:value-of select="$pBC" />
			<xsl:if test="$pBC != $retValue">
				<xsl:text> (</xsl:text>
				<xsl:value-of select="$retValue" />
				<xsl:text>)</xsl:text>
			</xsl:if>
		</xsl:if>
		<xsl:if test="$pIsGetFullName = false()">
			<xsl:value-of select="$retValue" />
		</xsl:if>
	</xsl:template>

	<!-- ******************************************************************************************************************** -->
	<!-- getFullNameMIC - return the complete name of the exchanges and market identification from the MIC codes abbreviation -->
	<!-- ******************************************************************************************************************** -->
	<xsl:template name="getFullNameMIC">
		<xsl:param name="pMIC"/>
		<xsl:param name="pIsGetFullName"/>
    
    <xsl:variable name="varResourceFile" select="'Resource.xml'"/>    
    <xsl:variable name="vResourceNameMIC"  select="document($varResourceFile)//root/market[@mic=$pMIC]/@displayname"/>
    
		<xsl:variable name="retValue">
			<xsl:choose>
        <xsl:when test="string-length($vResourceNameMIC) >0 ">
          <xsl:value-of select="$vResourceNameMIC"/>
        </xsl:when>        
        <xsl:otherwise>
					<xsl:value-of select="$pMIC" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:if test="$pIsGetFullName = true()">
			<xsl:value-of select="$pMIC" />
			<xsl:if test="$pMIC != $retValue">
				<xsl:text> (</xsl:text>
				<xsl:value-of select="$retValue" />
				<xsl:text>)</xsl:text>
			</xsl:if>
		</xsl:if>
		<xsl:if test="$pIsGetFullName = false()">
			<xsl:value-of select="$retValue" />
		</xsl:if>
	</xsl:template>

</xsl:stylesheet>

