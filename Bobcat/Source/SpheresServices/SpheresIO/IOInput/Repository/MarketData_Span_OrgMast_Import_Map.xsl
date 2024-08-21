<!-- 20130218 [18419] add vIsOption -->
<!-- 20230302 [WI589] Add test to insert/update on SPANPRODUCT only columns with values -->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes"
              media-type="text/xml; charset=ISO-8859-1"/>

  <xsl:variable name="gNull">null</xsl:variable> 
  
  <!-- Includes-->
  <xsl:include href="MarketData_Common.xsl"/>

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
      <xsl:apply-templates select="row"/>
    </file>
  </xsl:template>

  <xsl:template match="row">
    <row>
      <xsl:call-template name="IORowAtt"/>
      <xsl:call-template name="rowStream"/>
    </row>
  </xsl:template>

  <!-- Spécifique à chaque Import -->
  <xsl:template name="rowStream">

  <!--
  FL/PL 20221104 Type value
   1 : Physical
  11 : Future
  14 : ??????
  21 : Option on Physical
  22 : Option on Future
  23 : Option on Stock
  31 : Combination
  41 : Option on Combination
  -->
    <xsl:variable name="vIsOption">
      <xsl:choose>
        <xsl:when test ="data[@name='Type']='21' or 
                         data[@name='Type']='22' or 
                         data[@name='Type']='23' or 
                         data[@name='Type']='41'">
          <xsl:value-of select ="'True'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="'False'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="vCLEARINGORGACRONYM" select="data[@name='COAcro']"/>
    <xsl:variable name="vEXCHANGEACRONYM" select="data[@name='ExchAcro']"/>
    <xsl:variable name="vCONTRACTSYMBOL" select="data[@name='PFCode']"/>

    <xsl:variable name="vCATEGORY">
      <xsl:choose>
        <xsl:when test ="$vIsOption = 'True'">
          <xsl:value-of select ="'O'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="'F'"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vTYPE" select="data[@name='Type']"/>
    <xsl:variable name="vDESCRIPTION" select="data[@name='PFName']"/>
    <xsl:variable name="vCONTRACTMULTIPLIER" select="data[@name='CVM']"/>
    <xsl:variable name="vPRICEDECLOCATOR" select="data[@name='SettleDecLoc']"/>
    <xsl:variable name="vSTRIKEDECLOCATOR" select="data[@name='StrikeDecLoc']"/>
    <xsl:variable name="vPRICEALIGNCODE" select="data[@name='SettleAlignCode']"/>
    <xsl:variable name="vSTRIKEALIGNCODE" select="data[@name='StrikeAlignCode']"/>
    <xsl:variable name="vCABINETOPTVALUE" select="data[@name='CabinetOptionValue']"/>


    <xsl:variable name="vSETTLTMETHOD">
      <xsl:choose>
        <xsl:when test ="data[@name='SettlementMethod']='CASH'">
          <xsl:value-of select ="'C'"/>
        </xsl:when>
        <xsl:when test ="data[@name='SettlementMethod']='DELIV'">
          <xsl:value-of select ="'P'"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="vSKIPONLOAD" select="data[@name='SkipOnLoad']"/>
    <xsl:variable name="vCURRENTLYACTIVE" select="data[@name='CurrentlyActive']"/>
    <xsl:variable name="vPRICINGMODEL" select="data[@name='PricingModel']"/>
    
    <xsl:variable name="vFUTVALUATIONMETHOD" select="data[@name='ValuationMethod']"/>
    <xsl:variable name="vIDC" select="data[@name='SettleCurrencyCode']"/>
    <xsl:variable name="vIDCOUNTRY" select="data[@name='CountryCode']"/>

    <xsl:variable name="vEXERCISESTYLE">
      <xsl:if test ="$vIsOption = 'True'">
        <xsl:choose>
          <xsl:when test ="data[@name='ExerciseStyle']='AMER'">
            <xsl:value-of select ="'1'"/>
          </xsl:when>
          <xsl:when test ="data[@name='ExerciseStyle']='EURO'">
            <xsl:value-of select ="'0'"/>
          </xsl:when>
        </xsl:choose>
      </xsl:if>
    </xsl:variable>

    <xsl:variable name="vPRICEQUOTEMETHOD">
      <xsl:choose>
        <xsl:when test ="data[@name='PriceQuotationMethod']='PCT'">
          <xsl:value-of select ="'PCTPAR'"/>
        </xsl:when>
        <xsl:when test ="data[@name='PriceQuotationMethod']='IDX'">
          <xsl:value-of select ="'INX'"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select ="data[@name='PriceQuotationMethod']"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <table name="SPANPRODUCT" action="IU" sequenceno="1">

      <column name="CLEARINGORGACRONYM" datakey="true" datakeyupd="false" datatype="string">
        <xsl:value-of select= "$vCLEARINGORGACRONYM"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="IsNull()" />
            <logInfo status="REJECTED" isexception ="true">
              <message>
                Clearing Organisation is a mandatory data and it cannot be empty.
              </message>
            </logInfo>
          </control>
        </controls>
      </column>

      <column name="EXCHANGEACRONYM" datakey="true" datakeyupd="false" datatype="string">
        <xsl:value-of select= "$vEXCHANGEACRONYM"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="IsNull()" />
            <logInfo status="REJECTED" isexception ="true">
              <message>
                Exchange is a mandatory data and it cannot be empty.
              </message>
            </logInfo>
          </control>
        </controls>
      </column>

      <column name="CATEGORY" datakey="true" datakeyupd="false" datatype="string">
        <xsl:value-of select= "$vCATEGORY"/>
      </column>

      <column name="CONTRACTSYMBOL" datakey="true" datakeyupd="false" datatype="string">
        <xsl:value-of select= "$vCONTRACTSYMBOL"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="IsNull()" />
            <logInfo status="REJECTED" isexception ="true">
              <message>
                Contract Symbol is a mandatory data and it cannot be empty.
              </message>
            </logInfo>
          </control>
        </controls>
      </column>

      <column name="TYPE" datakey="true" datakeyupd="false" datatype="string">
        <xsl:value-of select= "$vTYPE"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="IsNull()" />
            <logInfo status="REJECTED" isexception ="true">
              <message>
                Type is a mandatory data and it cannot be empty.
              </message>
            </logInfo>
          </control>
        </controls>
      </column>

      <column name="DESCRIPTION" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select= "$vDESCRIPTION"/>
      </column>

      <xsl:if test="(normalize-space($vCONTRACTMULTIPLIER)!='')  and (number($vCONTRACTMULTIPLIER) = $vCONTRACTMULTIPLIER)">
        <column name="CONTRACTMULTIPLIER" datakey="false" datakeyupd="true" datatype="decimal">
          <xsl:value-of select= "$vCONTRACTMULTIPLIER"/>
        </column>
      </xsl:if>

      <xsl:if test="(normalize-space($vPRICEDECLOCATOR)!='')  and (number($vPRICEDECLOCATOR) = $vPRICEDECLOCATOR)">
        <column name="PRICEDECLOCATOR" datakey="false" datakeyupd="true" datatype="integer">
          <xsl:value-of select= "$vPRICEDECLOCATOR"/>
        </column>
      </xsl:if>

      <xsl:if test="(normalize-space($vSTRIKEDECLOCATOR)!='') and (number($vSTRIKEDECLOCATOR) = $vSTRIKEDECLOCATOR) and ($vCATEGORY = 'O')">
        <column name="STRIKEDECLOCATOR" datakey="false" datakeyupd="true" datatype="integer">
          <xsl:value-of select= "$vSTRIKEDECLOCATOR"/>
        </column>
      </xsl:if>

      <xsl:if test="(normalize-space($vPRICEALIGNCODE)!='')">
        <column name="PRICEALIGNCODE" datakey="false" datakeyupd="true" datatype="string">
          <xsl:value-of select= "$vPRICEALIGNCODE"/>
        </column>
      </xsl:if>

      <xsl:if test="(normalize-space($vSTRIKEALIGNCODE)!='') and ($vCATEGORY = 'O')">
        <column name="STRIKEALIGNCODE" datakey="false" datakeyupd="true" datatype="string">
          <xsl:value-of select= "$vSTRIKEALIGNCODE"/>
        </column>
      </xsl:if>

      <xsl:if test="(normalize-space($vCABINETOPTVALUE)!='') and (number($vCABINETOPTVALUE) = $vCABINETOPTVALUE) and ($vCATEGORY = 'O')">
        <column name="CABINETOPTVALUE" datakey="false" datakeyupd="true" datatype="decimal">
          <xsl:value-of select= "$vCABINETOPTVALUE"/>
        </column>
      </xsl:if>

      <xsl:if test="(normalize-space($vSETTLTMETHOD)!='')">
        <column name="SETTLTMETHOD" datakey="false" datakeyupd="true" datatype="string">
          <xsl:value-of select= "$vSETTLTMETHOD"/>
        </column>
      </xsl:if>

      <xsl:if test="(normalize-space($vSKIPONLOAD)!='')">
        <column name="SKIPONLOAD" datakey="false" datakeyupd="true" datatype="string">
          <xsl:value-of select= "$vSKIPONLOAD"/>
        </column>
      </xsl:if>

      <xsl:if test="(normalize-space($vCURRENTLYACTIVE)!='')">
        <column name="CURRENTLYACTIVE" datakey="false" datakeyupd="true" datatype="string">
          <xsl:value-of select= "$vCURRENTLYACTIVE"/>
        </column>
      </xsl:if>

      <xsl:if test="(normalize-space($vPRICINGMODEL)!='')">
        <column name="PRICINGMODEL" datakey="false" datakeyupd="true" datatype="string">
          <xsl:value-of select= "$vPRICINGMODEL"/>
        </column>
      </xsl:if>

      <xsl:if test="(normalize-space($vPRICEQUOTEMETHOD)!='')">
        <column name="PRICEQUOTEMETHOD" datakey="false" datakeyupd="true" datatype="string">
          <xsl:value-of select= "$vPRICEQUOTEMETHOD"/>
        </column>
      </xsl:if>

      <xsl:if test="(normalize-space($vFUTVALUATIONMETHOD)!='')">
        <column name="FUTVALUATIONMETHOD" datakey="false" datakeyupd="true" datatype="string">
          <xsl:value-of select= "$vFUTVALUATIONMETHOD"/>
        </column>
      </xsl:if>

      <column name="IDC" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select= "$vIDC"/>
      </column>

      <column name="IDCOUNTRY" datakey="false" datakeyupd="true" datatype="string">
        <xsl:value-of select= "$vIDCOUNTRY"/>
      </column>

      <xsl:if test="(normalize-space($vEXERCISESTYLE)!='') and ($vCATEGORY = 'O')">
        <column name="EXERCISESTYLE" datakey="false" datakeyupd="true" datatype="string">
          <xsl:value-of select= "$vEXERCISESTYLE"/>
        </column>
      </xsl:if>

      <!--
      <column name="DTINS" datakey="false" datakeyupd="false" datatype="datetime">
        <SpheresLib function="GetUTCDateTimeSys()" />
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="IsUpdate()" />
            <logInfo status="NONE"/>
          </control>
        </controls>
      </column>
      <column name="IDAINS" datakey="false" datakeyupd="false" datatype="integer">
        <SpheresLib function="GetUserId()" />
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="IsUpdate()" />
            <logInfo status="NONE"/>
          </control>
        </controls>
      </column>

      <column name="DTUPD" datakey="false" datakeyupd="false" datatype="datetime">
        <SpheresLib function="GetUTCDateTimeSys()" />
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="IsInsert()" />
            <logInfo status="NONE"/>
          </control>
        </controls>
      </column>
      <column name="IDAUPD" datakey="false" datakeyupd="false" datatype="integer">
        <SpheresLib function="GetUserId()" />
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            <SpheresLib function="IsInsert()" />
            <logInfo status="NONE"/>
          </control>
        </controls>
      </column>
-->
    </table>

  </xsl:template>
</xsl:stylesheet>