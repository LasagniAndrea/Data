<!--

=======================================================================================
Summary : CCeG - RISKDATA
File    : RiskData_CCeG_Map.xsl
=======================================================================================
Version : v10.0.0.0
Date    : 20200901
Author  : FI
Comment : FI 20200901 [25468] use of GetUTCDateTimeSys
=======================================================================================

Version : v6.0.0.0
Date    : 20170222
Author  : PM
Comment : Remove DTMARKET from PARAMSTIMSIDEM_RISKARRAY
Rename PARAMSTIMSIDEM_RISKARRAY to IMMARSRISKARRAY_H
Rename PARAMSTIMSIDEM_CLASS to IMMARSCLASS_H
=======================================================================================
Version : v5.1.0.0
Date    : 20170222
Author  : PM
Comment : Add IMMARS_H and PARAMSTIMSIDEM_CLASS by date
Add column ISINCODE
=======================================================================================
Version : v3.2.0.0
Date    : 20130430
Author  : CC
Comment : File RiskData_TimsIdem_Import_Map.xsl renamed to RiskData_CCeG_Map.xsl
=======================================================================================
Version : v3.2.0.0
Date    : 20130215
Author  : PL
Comment : Management of Market-Id='5' and Market-Id='8'
=======================================================================================
Version :
Date    : 20121102
Author  : BD
Comment : Executer ce XSL uniquement quand PRICEONLY=false
=======================================================================================
Date    : 20110704
Author  : MF
Description:
Import TIMS IDEM parameters for margin evaluation.
Sources: CLASSFILE txt file; RISKARRAY txt file
Destinations: PARAMSTIMSIDEM_CLASS table object; PARAMSTIMSIDEM_RISKARRAY table object
=======================================================================================
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- Includes-->
  <xsl:include href="..\Common\CommonInput.xslt"/>
  <xsl:include href="..\Quote\Quote_Common_SQL.xsl"/>

  <xsl:variable name="vVersionRiskDataImport">v6.0.0.0</xsl:variable>
  <xsl:variable name="vFileNameRiskDataImport">RiskData_CCeG_Map.xsl</xsl:variable>

  <!-- PM 20170222 [22881][22942] Add vAskedBusinessDate, vImportedFileName, vImportedFileFolder, vIOInputName -->
  <xsl:variable name ="vAskedBusinessDate" select="/iotask/parameters/parameter[@id='DTBUSINESS']"/>
  <xsl:variable name ="vImportedFileName" select="/iotask/iotaskdet/ioinput/file/@name"/>
  <xsl:variable name ="vImportedFileFolder" select="/iotask/iotaskdet/ioinput/file/@folder"/>
  <xsl:variable name ="vIOInputName" select="/iotask/iotaskdet/ioinput/@name"/>
  
  <!-- V1: IDEM import only, unique exchange symbol [02]  -->
  <!-- TODO V2: IDEX import, energy contracts, exchange symbol [05]  -->
  <!--<xsl:variable name="vExchangeSymbol">
    <xsl:value-of select="'2'"/>
  </xsl:variable>-->

  <!-- match the file node -->
  <xsl:template match="file">
    <file>
      <xsl:call-template name="IOFileAtt"/>

      <!-- Executer ce XSL uniquement quand PRICEONLY=false -->
      <xsl:if test="//iotask/parameters/parameter[@id='PRICEONLY']='false'">

        <!-- PM 20170222 [22881][22942] Add call template IMMARS_H -->
        <xsl:call-template name="IMMARS_H">
          <xsl:with-param name="pBusinessDate" select="$vAskedBusinessDate"/>
        </xsl:call-template>

        <!-- match any row node inside the actual file node -->
        <xsl:apply-templates select="row" />
      </xsl:if>
    </file>
  </xsl:template>

  <!-- match any row node -->
  <xsl:template match="row">
    <xsl:choose>

      <!-- CLASSFILE -->
      <xsl:when test="
                      data[contains(@status, 'success')] and 
                      data[contains(@name, 'Product')] and 
                      data[contains(@name, 'Class')]">
        <row>
          <xsl:call-template name="IORowAtt"/>

          <xsl:call-template name="IMMARSCLASS_H">
            <!-- PM 20170222 [22881][22942] Add parameter pBusinessDate -->
            <xsl:with-param name="pBusinessDate" select="$vAskedBusinessDate"/>
            <xsl:with-param name="pSymbol" select="data[@name='Symbol']"/>
            <xsl:with-param name="pClassType" select="data[@name='ClassType']"/>
            <!--<xsl:with-param name="pExchangeSymbol" select="$vExchangeSymbol"/>-->
            <xsl:with-param name="pExchangeSymbol" select="data[@name='MarketId']"/>
            <!-- PM 20170222 [22881][22942] Add pUnlIsinCode -->
            <xsl:with-param name="pUnlIsinCode" select="data[@name='Isin']"/>

            <xsl:with-param name="pProduct" select="data[@name='Product']"/>
            <xsl:with-param name="pClass" select="data[@name='Class']"/>
            <xsl:with-param name="pUnderlyingCode" select="data[@name='UnderlyingCode']"/>
            <xsl:with-param name="pUnderlyingCategory" select="data[@name='ProductType']"/>
            <xsl:with-param name="pOffset" select="data[@name='Offset']"/>

            <xsl:with-param name="pSpotSpreadRate" select="data[@name='SpotSpreadRate']"/>
            <xsl:with-param name="pNonSpotSpreadRate" select="data[@name='NonSpotSpreadRate']"/>
            <xsl:with-param name="pDeliveryRate" select="data[@name='DeliveryRate']"/>
            <xsl:with-param name="pMultiplier" select="data[@name='Multiplier']"/>
            <xsl:with-param name="pUnderlyingPrice" select="data[@name='UnderlyingPrice']"/>
            <xsl:with-param name="pMinimumMargin" select="data[@name='MinimumMargin']"/>
            <xsl:with-param name="pDaysToSettle" select="data[@name='DaysToSettle']"/>
            <xsl:with-param name="pExpiryTime" select="data[@name='ExpiryTime']"/>

            <xsl:with-param name="pCurrency" select="data[@name='Currency']"/>
          </xsl:call-template>
        </row>
      </xsl:when>

      <!-- RISKARRAY -->
      <xsl:otherwise>
        <row>
          <xsl:call-template name="IORowAtt"/>

          <xsl:variable name="vYM">
            <xsl:choose>
              <xsl:when test="data[@name='YM'] = '000000'">
                <xsl:value-of select="$gNull"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="data[@name='YM']"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:variable name="vPutCall">
            <xsl:choose>
              <xsl:when test="data[@name='ClassType'] = 'F'">
                <xsl:value-of select="$gNull"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:choose>
                  <xsl:when test="data[@name='PutCall'] = 'C'">
                    <xsl:value-of select="'1'"/>
                  </xsl:when>
                  <xsl:when test="data[@name='PutCall'] = 'P'">
                    <xsl:value-of select="'0'"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="$gNull"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:variable name="vDownSide5">
            <xsl:call-template name="AddSignToRiskValue">
              <xsl:with-param name="pSign" select="data[@name='D5Sign']"/>
              <xsl:with-param name="pRiskValue" select="data[@name='DownSide5']"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name="vDownSide4">
            <xsl:call-template name="AddSignToRiskValue">
              <xsl:with-param name="pSign" select="data[@name='D4Sign']"/>
              <xsl:with-param name="pRiskValue" select="data[@name='DownSide4']"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name="vDownSide3">
            <xsl:call-template name="AddSignToRiskValue">
              <xsl:with-param name="pSign" select="data[@name='D3Sign']"/>
              <xsl:with-param name="pRiskValue" select="data[@name='DownSide3']"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name="vDownSide2">
            <xsl:call-template name="AddSignToRiskValue">
              <xsl:with-param name="pSign" select="data[@name='D2Sign']"/>
              <xsl:with-param name="pRiskValue" select="data[@name='DownSide2']"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name="vDownSide1">
            <xsl:call-template name="AddSignToRiskValue">
              <xsl:with-param name="pSign" select="data[@name='D1Sign']"/>
              <xsl:with-param name="pRiskValue" select="data[@name='DownSide1']"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name="vUpSide1">
            <xsl:call-template name="AddSignToRiskValue">
              <xsl:with-param name="pSign" select="data[@name='U1Sign']"/>
              <xsl:with-param name="pRiskValue" select="data[@name='UpSide1']"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name="vUpSide2">
            <xsl:call-template name="AddSignToRiskValue">
              <xsl:with-param name="pSign" select="data[@name='U2Sign']"/>
              <xsl:with-param name="pRiskValue" select="data[@name='UpSide2']"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name="vUpSide3">
            <xsl:call-template name="AddSignToRiskValue">
              <xsl:with-param name="pSign" select="data[@name='U3Sign']"/>
              <xsl:with-param name="pRiskValue" select="data[@name='UpSide3']"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name="vUpSide4">
            <xsl:call-template name="AddSignToRiskValue">
              <xsl:with-param name="pSign" select="data[@name='U4Sign']"/>
              <xsl:with-param name="pRiskValue" select="data[@name='UpSide4']"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name="vUpSide5">
            <xsl:call-template name="AddSignToRiskValue">
              <xsl:with-param name="pSign" select="data[@name='U5Sign']"/>
              <xsl:with-param name="pRiskValue" select="data[@name='UpSide5']"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:variable name="vShortAdj">
            <xsl:call-template name="AddSignToRiskValue">
              <xsl:with-param name="pSign" select="data[@name='SASign']"/>
              <xsl:with-param name="pRiskValue" select="data[@name='ShortAdj']"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:call-template name="IMMARSRISKARRAY_H">
            <!-- PM 20170222 [22881][22942] Add parameter pBusinessDate -->
            <xsl:with-param name="pBusinessDate" select="$vAskedBusinessDate"/>
            <xsl:with-param name="pDate" select="data[@name='Date']"/>
            <xsl:with-param name="pSymbol" select="data[@name='Symbol']"/>
            <xsl:with-param name="pClassType" select="data[@name='ClassType']"/>

            <!--<xsl:with-param name="pExchangeSymbol" select="$vExchangeSymbol"/>-->
            <xsl:with-param name="pExchangeSymbol" select="data[@name='MarketId']"/>

            <!-- PM 20170222 [22881][22942] Add pUnlIsinCode -->
            <xsl:with-param name="pUnlIsinCode" select="data[@name='Isin']"/>
            
            <xsl:with-param name="pYM" select="$vYM"/>
            <xsl:with-param name="pPutCall" select="$vPutCall"/>
            <xsl:with-param name="pStrkPrx" select="data[@name='StrkPrx']"/>

            <xsl:with-param name="pMarkPrice" select="data[@name='MarkPrice']"/>

            <xsl:with-param name="pDownSide5" select="$vDownSide5"/>
            <xsl:with-param name="pDownSide4" select="$vDownSide4"/>
            <xsl:with-param name="pDownSide3" select="$vDownSide3"/>
            <xsl:with-param name="pDownSide2" select="$vDownSide2"/>
            <xsl:with-param name="pDownSide1" select="$vDownSide1"/>

            <xsl:with-param name="pUpSide1" select="$vUpSide1"/>
            <xsl:with-param name="pUpSide2" select="$vUpSide2"/>
            <xsl:with-param name="pUpSide3" select="$vUpSide3"/>
            <xsl:with-param name="pUpSide4" select="$vUpSide4"/>
            <xsl:with-param name="pUpSide5" select="$vUpSide5"/>

            <xsl:with-param name="pShortAdj" select="$vShortAdj"/>
          </xsl:call-template>

          <xsl:if test="data[@name='ClassType'] = 'F' and $vYM = $gNull">

            <xsl:call-template name="IMMARSRISKARRAY_H">
              <!-- PM 20170222 [22881][22942] Add parameter pBusinessDate -->
              <xsl:with-param name="pBusinessDate" select="$vAskedBusinessDate"/>
              <xsl:with-param name="pDate" select="data[@name='Date']"/>
              <xsl:with-param name="pSymbol" select="data[@name='Symbol']"/>

              <xsl:with-param name="pClassType" select="'O'"/>

              <!--<xsl:with-param name="pExchangeSymbol" select="$vExchangeSymbol"/>-->
              <xsl:with-param name="pExchangeSymbol" select="data[@name='MarketId']"/>

              <!-- PM 20170222 [22881][22942] Add pUnlIsinCode -->
              <xsl:with-param name="pUnlIsinCode" select="data[@name='Isin']"/>
              
              <xsl:with-param name="pYM" select="$vYM"/>
              <xsl:with-param name="pPutCall" select="$vPutCall"/>
              <xsl:with-param name="pStrkPrx" select="data[@name='StrkPrx']"/>

              <xsl:with-param name="pMarkPrice" select="data[@name='MarkPrice']"/>

              <xsl:with-param name="pDownSide5" select="$vDownSide5"/>
              <xsl:with-param name="pDownSide4" select="$vDownSide4"/>
              <xsl:with-param name="pDownSide3" select="$vDownSide3"/>
              <xsl:with-param name="pDownSide2" select="$vDownSide2"/>
              <xsl:with-param name="pDownSide1" select="$vDownSide1"/>

              <xsl:with-param name="pUpSide1" select="$vUpSide1"/>
              <xsl:with-param name="pUpSide2" select="$vUpSide2"/>
              <xsl:with-param name="pUpSide3" select="$vUpSide3"/>
              <xsl:with-param name="pUpSide4" select="$vUpSide4"/>
              <xsl:with-param name="pUpSide5" select="$vUpSide5"/>

              <xsl:with-param name="pShortAdj" select="$vShortAdj"/>

            </xsl:call-template>

          </xsl:if>
        </row>
      </xsl:otherwise>

    </xsl:choose>

  </xsl:template>

  <xsl:template name="SelectIDDC">
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pExchangeSymbol"/>

    <!-- BD 20130520 : Appel du template SQLDTENABLEDDTDISABLED pour vérifier la validité du DC sélectionné -->
    <SQL command="select" result="IDDC" cache="true">
      select dc.IDDC
      from dbo.DERIVATIVECONTRACT dc
      inner join dbo.MARKET m on m.IDM=dc.IDM and (m.ISO10383_ALPHA4='XDMI') and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL)
      where (dc.CONTRACTSYMBOL=@SYMBOL) and (dc.CATEGORY=@CATEGORY)
      <xsl:call-template name="SQLDTENABLEDDTDISABLED">
        <xsl:with-param name="pTable" select="'dc'"/>
      </xsl:call-template>
      <Param name="DT" datatype="date">
        <xsl:value-of select="$gParamDtBusiness"/>
      </Param>
      <Param name="SYMBOL" datatype="string">
        <xsl:value-of select="$pContractSymbol" />
      </Param>
      <Param name="CATEGORY" datatype="string">
        <xsl:value-of select="$pCategory" />
      </Param>
      <Param name="EXCHANGESYMBOL" datatype="string">
        <xsl:value-of select="$pExchangeSymbol" />
      </Param>
    </SQL>

  </xsl:template>

  <xsl:template name="ExistsContractSymbol">
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pExchangeSymbol"/>

    <!-- BD 20130520 : Appel du template SQLDTENABLEDDTDISABLED pour vérifier la validité du DC sélectionné -->
    <SQL command="select" result="IDDC" cache="true">
      select dc.IDDC
      from dbo.DERIVATIVECONTRACT dc
      inner join dbo.MARKET m on m.IDM=dc.IDM and (m.ISO10383_ALPHA4='XDMI') and (m.EXCHANGESYMBOL=@EXCHANGESYMBOL)
      where (dc.CONTRACTSYMBOL=@SYMBOL)
      <xsl:call-template name="SQLDTENABLEDDTDISABLED">
        <xsl:with-param name="pTable" select="'dc'"/>
      </xsl:call-template>
      
      <Param name="DT" datatype="date">
        <xsl:value-of select="$gParamDtBusiness"/>
      </Param>
      <Param name="SYMBOL" datatype="string">
        <xsl:value-of select="$pContractSymbol" />
      </Param>
      <Param name="EXCHANGESYMBOL" datatype="string">
        <xsl:value-of select="$pExchangeSymbol" />
      </Param>
    </SQL>

  </xsl:template>

  <!-- Insert a row in the IMMARSCLASS_H table -->
  <xsl:template name="IMMARSCLASS_H">
    <xsl:param name="pIsWithControl" select="true()"/>
    <!-- PM 20170222 [22881][22942] Add pBusinessDate -->
    <xsl:param name="pBusinessDate" select="$gNull"/>
    <xsl:param name="pSymbol" select="$gNull"/>
    <xsl:param name="pClassType" select="$gNull"/>
    <xsl:param name="pExchangeSymbol" select="$gNull"/>
    <!-- PM 20170222 [22881][22942] Add pUnlIsinCode -->
    <xsl:param name="pUnlIsinCode" select="$gNull"/>
    
    <xsl:param name="pContractAttribute" select="$gNull"/>
    <xsl:param name="pProduct" select="$gNull"/>
    <xsl:param name="pClass" select="$gNull"/>
    <xsl:param name="pUnderlyingCode" select="$gNull"/>
    <xsl:param name="pUnderlyingCategory" select="$gNull"/>
    <xsl:param name="pOffset" select="$gNull"/>

    <xsl:param name="pSpotSpreadRate" select="$gNull"/>
    <xsl:param name="pNonSpotSpreadRate" select="$gNull"/>
    <xsl:param name="pDeliveryRate" select="$gNull"/>
    <xsl:param name="pMultiplier" select="$gNull"/>
    <xsl:param name="pUnderlyingPrice" select="$gNull"/>
    <xsl:param name="pMinimumMargin" select="$gNull"/>
    <xsl:param name="pDaysToSettle" select="$gNull"/>
    <xsl:param name="pExpiryTime" select="$gNull"/>

    <xsl:param name="pCurrency" select="$gNull"/>


    <table name="IMMARSCLASS_H" action="IU" sequenceno="1">

      <!-- PM 20170222 [22881][22942] Call template Col_IDIMMARS_H -->
      <xsl:call-template name="Col_IDIMMARS_H">
        <xsl:with-param name="pBusinessDate" select="$pBusinessDate"/>
      </xsl:call-template>
      
      <column name="CONTRACTSYMBOL" datakey="true" datatype="string">
        <xsl:value-of select="$pSymbol"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <code>SYS</code>
              <number>2001</number>
              <data1>
                <xsl:value-of select="$vFileNameRiskDataImport"/>
              </data1>
              <data2>
                <xsl:value-of select="$vVersionRiskDataImport"/>
              </data2>
            </logInfo>
          </control>
        </controls>
      </column>

      <column name="CATEGORY" datakey="true" datatype="string">
        <xsl:value-of select="$pClassType"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <code>SYS</code>
              <number>2002</number>
              <data1>
                <xsl:value-of select="$vFileNameRiskDataImport"/>
              </data1>
              <data2>
                <xsl:value-of select="$vVersionRiskDataImport"/>
              </data2>
            </logInfo>
          </control>
        </controls>
      </column>

      <xsl:choose>

        <xsl:when test="$pClassType = 'F' or $pClassType = 'O'">

          <column name="IDDC" datatype="integer">
            <xsl:call-template name="SelectIDDC">
              <xsl:with-param name="pContractSymbol" select="$pSymbol"/>
              <xsl:with-param name="pCategory" select="$pClassType"/>
              <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
            </xsl:call-template>
            <!-- RD 20160518 [22175] dans le cas où le DC n'existe pas dans le référentiel Contrats Dérivés, afficher un message avec le satatut INFO au lieu de REJECT (Warning)-->
            <controls>
              <control action="RejectRow" return="true">
                <SpheresLib function="ISNULL()"/>
                <logInfo status="INFO" isexception="false">
                  <code>LOG</code>
                  <number>2006</number>
                  <data1>
                    <xsl:value-of select="$pExchangeSymbol"/>
                  </data1>
                  <data2>
                    <xsl:value-of select="$pSymbol"/>
                  </data2>
                  <data3>
                    <xsl:choose>
                      <xsl:when test="$pClassType='F'">
                        <xsl:value-of select="'Future'"/>
                      </xsl:when>
                      <xsl:when test="$pClassType='O'">
                        <xsl:value-of select="'Option'"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="$pClassType"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </data3>
                </logInfo>
              </control>
            </controls>
          </column>

        </xsl:when>

        <!-- PM 20170222 [22881][22942] Add case when ClassType = 'C' -->
        <xsl:when test="$pClassType = 'C'">
          <column name="CHECK_UNLISINCODE">
            <xsl:call-template name="ExistsUnderlyingEquity">
              <xsl:with-param name="pIsinCode" select="$pUnlIsinCode"/>
            </xsl:call-template>
            <controls>
              <control action="RejectRow" return="true" logtype="Full">
                <SpheresLib function="ISNULL()"/>
              </control>
              <control action="RejectColumn" return="true" logtype="Full">true</control>
            </controls>
          </column>
        </xsl:when>

        <xsl:otherwise>

          <column name="CHECK_CONTRACTSYMBOL">
            <xsl:call-template name="ExistsContractSymbol">
              <xsl:with-param name="pContractSymbol" select="$pSymbol"/>
              <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
            </xsl:call-template>
            <controls>
              <control action="RejectRow" return="true" logtype="Full">
                <SpheresLib function="ISNULL()"/>
              </control>
              <control action="RejectColumn" return="true" logtype="Full">
                true
              </control>
            </controls>
          </column>

        </xsl:otherwise>

      </xsl:choose>

      <column name="IDASSET_UNL" datakeyupd="true" datatype="integer">
        <xsl:call-template name="SelectIDASSET">
          <xsl:with-param name="pContractSymbol" select="$pUnderlyingCode"/>
          <xsl:with-param name="pCategory" select="$pClassType"/>
        </xsl:call-template>
      </column>

      <column name="PRODUCTGROUP" datatype="string">
        <xsl:value-of select="$pProduct"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <code>SYS</code>
              <number>2007</number>
              <data1>
                <xsl:value-of select="$vFileNameRiskDataImport"/>
              </data1>
              <data2>
                <xsl:value-of select="$vVersionRiskDataImport"/>
              </data2>
            </logInfo>
          </control>
        </controls>
      </column>

      <column name="CLASSGROUP" datatype="string">
        <xsl:value-of select="$pClass"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <code>SYS</code>
              <number>2008</number>
              <data1>
                <xsl:value-of select="$vFileNameRiskDataImport"/>
              </data1>
              <data2>
                <xsl:value-of select="$vVersionRiskDataImport"/>
              </data2>
            </logInfo>
          </control>
        </controls>
      </column>

      <column name="UNDERLYINGCONTRACT" datatype="string">
        <xsl:value-of select="$pUnderlyingCode"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <code>SYS</code>
              <number>2009</number>
              <data1>
                <xsl:value-of select="$vFileNameRiskDataImport"/>
              </data1>
              <data2>
                <xsl:value-of select="$vVersionRiskDataImport"/>
              </data2>
            </logInfo>
          </control>
        </controls>
      </column>


      <column name="OFFSET" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$pOffset"/>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="SPOTSPREADRATE" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$pSpotSpreadRate"/>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="NONSPOTSPREADRATE" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$pNonSpotSpreadRate"/>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="DELIVERYRATE" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$pDeliveryRate"/>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="CONTRACTMULTIPLIER" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$pMultiplier"/>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="UNDERLYINGPRICE" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$pUnderlyingPrice"/>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="MINIMUMMARGIN" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$pMinimumMargin"/>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="STLDAYSDELAY" datakeyupd="true" datatype="int">
        <xsl:value-of select="$pDaysToSettle"/>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="EXPIRYTIME" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pExpiryTime"/>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <!-- PM 20170222 [22881][22942] Add ISINCODE -->
      <column name="ISINCODE" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pUnlIsinCode"/>
      </column>
        
      <column name="DTINS" datakey="false" datakeyupd="false" datatype="datetime">
        <SpheresLib function="GetUTCDateTimeSys()"/>
        <xsl:if test="$pIsWithControl = $gTrue">
          <controls>
            <control action="RejectColumn" return="true" logtype="None">
              <SpheresLib function="IsUpdate()"/>
            </control>
          </controls>
        </xsl:if>
      </column>
      <column name="IDAINS" datakey="false" datakeyupd="false" datatype="int">
        <SpheresLib function="GetUserID()"/>
        <xsl:if test="$pIsWithControl = $gTrue">
          <controls>
            <control action="RejectColumn" return="true" logtype="None">
              <SpheresLib function="IsUpdate()"/>
            </control>
          </controls>
        </xsl:if>
      </column>

      <xsl:call-template name="SysUpd">
        <xsl:with-param name="pIsWithControl" select="$pIsWithControl"/>
      </xsl:call-template>

    </table>

  </xsl:template>

  <!-- Insert a row in the IMMARSRISKARRAY_H table -->
  <xsl:template name="IMMARSRISKARRAY_H">
    <xsl:param name="pIsWithControl" select="true()"/>
    <!-- PM 20170222 [22881][22942] Add pBusinessDate -->
    <xsl:param name="pBusinessDate" select="$gNull"/>
    <xsl:param name="pDate" select="$gNull"/>
    <xsl:param name="pSymbol" select="$gNull"/>
    <xsl:param name="pClassType" select="$gNull"/>

    <xsl:param name="pExchangeSymbol" select="$gNull"/>

    <!-- PM 20170222 [22881][22942] Add pUnlIsinCode -->
    <xsl:param name="pUnlIsinCode" select="$gNull"/>

    <xsl:param name="pYM" select="$gNull"/>
    <xsl:param name="pPutCall" select="$gNull"/>
    <xsl:param name="pStrkPrx" select="$gNull"/>

    <xsl:param name="pMarkPrice" select="$gNull"/>

    <xsl:param name="pDownSide5" select="$gNull"/>
    <xsl:param name="pDownSide4" select="$gNull"/>
    <xsl:param name="pDownSide3" select="$gNull"/>
    <xsl:param name="pDownSide2" select="$gNull"/>
    <xsl:param name="pDownSide1" select="$gNull"/>

    <xsl:param name="pUpSide1" select="$gNull"/>
    <xsl:param name="pUpSide2" select="$gNull"/>
    <xsl:param name="pUpSide3" select="$gNull"/>
    <xsl:param name="pUpSide4" select="$gNull"/>
    <xsl:param name="pUpSide5" select="$gNull"/>

    <xsl:param name="pShortAdj" select="$gNull"/>

    <table name="IMMARSRISKARRAY_H" action="IU" sequenceno="2">

      <xsl:choose>

        <xsl:when test="$pClassType = 'F' or $pClassType = 'O'">

          <column name="CHECK_IDDC">
            <xsl:call-template name="SelectIDDC">
              <xsl:with-param name="pContractSymbol" select="$pSymbol"/>
              <xsl:with-param name="pCategory" select="$pClassType"/>
              <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
            </xsl:call-template>
            <controls>
              <control action="RejectRow" return="true" logtype="Full">
                <SpheresLib function="ISNULL()"/>
              </control>
              <control action="RejectColumn" return="true" logtype="Full">
                true
              </control>
            </controls>
          </column>

        </xsl:when>

        <!-- PM 20170222 [22881][22942] Add case when ClassType = 'C' -->
        <xsl:when test="$pClassType = 'C'">
          <column name="CHECK_UNLISINCODE">
            <xsl:call-template name="ExistsUnderlyingEquity">
              <xsl:with-param name="pIsinCode" select="$pUnlIsinCode"/>
            </xsl:call-template>
            <controls>
              <control action="RejectRow" return="true" logtype="Full">
                <SpheresLib function="ISNULL()"/>
              </control>
              <control action="RejectColumn" return="true" logtype="Full">true</control>
            </controls>
          </column>
        </xsl:when>

        <xsl:otherwise>

          <column name="CHECK_CONTRACTSYMBOL">
            <xsl:call-template name="ExistsContractSymbol">
              <xsl:with-param name="pContractSymbol" select="$pSymbol"/>
              <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
            </xsl:call-template>
            <controls>
              <control action="RejectRow" return="true" logtype="Full">
                <SpheresLib function="ISNULL()"/>
              </control>
              <control action="RejectColumn" return="true" logtype="Full">
                true
              </control>
            </controls>
          </column>

        </xsl:otherwise>

      </xsl:choose>

      <!-- PM 20170222 [22881][22942] Call template Col_IDIMMARS_H -->
      <xsl:call-template name="Col_IDIMMARS_H">
        <xsl:with-param name="pBusinessDate" select="$pBusinessDate"/>
      </xsl:call-template>

      <!-- PM 20170222 [22881][22942] Add control on $pDate!=$pBusinessDate and Reject Column-->
      <column name="Check_DTBUSINESS" datatype="date" dataformat="yyyy-MM-dd">
        <xsl:value-of select="$pDate"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="NONE">true</control>
          <!--control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <code>SYS</code>
              <number>2000</number>
              <data1>
                <xsl:value-of select="$vFileNameRiskDataImport"/>
              </data1>
              <data2>
                <xsl:value-of select="$vVersionRiskDataImport"/>
              </data2>
            </logInfo>
          </control-->
          <control action="RejectRow" return="true">
            <xsl:choose>
              <xsl:when test="$pDate!=$pBusinessDate">
                true
                <logInfo status="ERROR" isexception="true">
                  <code>SYS</code>
                  <number>5301</number>
                  <data1>
                    <xsl:value-of select="$pDate"/>
                  </data1>
                  <data2>
                    <xsl:value-of select="$pBusinessDate"/>
                  </data2>
                  <data3>
                    <xsl:value-of select="$vImportedFileName"/>
                  </data3>
                  <data4>
                    <xsl:value-of select="$vImportedFileFolder"/>
                  </data4>
                  <data5>
                    <xsl:value-of select="$vIOInputName"/>
                  </data5>
                </logInfo>
              </xsl:when>
              <xsl:otherwise>
                false
                <logInfo status="NONE" />
              </xsl:otherwise>
            </xsl:choose>
          </control>
        </controls>
      </column>

      <column name="CONTRACTSYMBOL" datakey="true" datatype="string">
        <xsl:value-of select="$pSymbol"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <code>SYS</code>
              <number>2001</number>
              <data1>
                <xsl:value-of select="$vFileNameRiskDataImport"/>
              </data1>
              <data2>
                <xsl:value-of select="$vVersionRiskDataImport"/>
              </data2>
            </logInfo>
          </control>
        </controls>
      </column>

      <column name="CATEGORY" datakey="true" datatype="string">
        <xsl:value-of select="$pClassType"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <code>SYS</code>
              <number>2002</number>
              <data1>
                <xsl:value-of select="$vFileNameRiskDataImport"/>
              </data1>
              <data2>
                <xsl:value-of select="$vVersionRiskDataImport"/>
              </data2>
            </logInfo>
          </control>
        </controls>
      </column>

      <column name="MATURITYYEARMONTH" datakey="true" datatype="string">
        <xsl:value-of select="$pYM"/>
      </column>

      <column name="PUTCALL" datakey="true" datatype="string">
        <xsl:value-of select="$pPutCall"/>
        <!--<controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="false">
              <code>SYS</code>
              <number>2003</number>
              <data1>
                <xsl:value-of select="$vFileNameRiskDataImport"/>
              </data1>
              <data2>
                <xsl:value-of select="$vVersionRiskDataImport"/>
              </data2>
            </logInfo>
          </control>
        </controls>-->
      </column>

      <column name="STRIKEPRICE" datakey="true" datatype="decimal">
        <xsl:value-of select="$pStrkPrx"/>
        <!--<controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <code>SYS</code>
              <number>02004</number>
              <data1>
                <xsl:value-of select="$vFileNameRiskDataImport"/>
              </data1>
              <data2>
                <xsl:value-of select="$vVersionRiskDataImport"/>
              </data2>
            </logInfo>
          </control>
        </controls>-->
      </column>

      <xsl:if test="$pYM != $gNull">
        <column name="IDASSET" datakey="true" datatype="integer">
          <xsl:call-template name="SelectIDASSET">
            <xsl:with-param name="pISO10383" select="'XDMI'"/>
            <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
            <xsl:with-param name="pContractSymbol" select="$pSymbol"/>
            <xsl:with-param name="pContractSymbolSuffix" select="$gNull"/>
            <xsl:with-param name="pCategory" select="$pClassType"/>
            <xsl:with-param name="pMaturityMonthYear" select="$pYM"/>
            <xsl:with-param name="pPutCall" select="$pPutCall"/>
            <xsl:with-param name="pStrike" select="$pStrkPrx"/>
          </xsl:call-template>
          <controls>
            <control action="RejectRow" return="true" >
              <SpheresLib function="ISNULL()"/>
              <logInfo status="NONE" />
            </control>
          </controls>
        </column>
      </xsl:if>


      <column name="MARKPRICE" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$pMarkPrice"/>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="DOWNSIDE5" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$pDownSide5"/>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="DOWNSIDE4" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$pDownSide4"/>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="DOWNSIDE3" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$pDownSide3"/>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="DOWNSIDE2" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$pDownSide2"/>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="DOWNSIDE1" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$pDownSide1"/>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="UPSIDE1" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$pUpSide1"/>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="UPSIDE2" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$pUpSide2"/>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="UPSIDE3" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$pUpSide3"/>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="UPSIDE4" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$pUpSide4"/>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="UPSIDE5" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$pUpSide5"/>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="SHORTOPTADJUSTMENT" datakeyupd="true" datatype="decimal">
        <xsl:value-of select="$pShortAdj"/>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <!-- PM 20170222 [22881][22942] Add ISINCODE -->
      <column name="ISINCODE" datakeyupd="true" datatype="string">
        <xsl:value-of select="$pUnlIsinCode"/>
      </column>
          
      <column name="DTINS" datakey="false" datakeyupd="false" datatype="datetime">
        <SpheresLib function="GetUTCDateTimeSys()"/>
        <xsl:if test="$pIsWithControl = $gTrue">
          <controls>
            <control action="RejectColumn" return="true" logtype="None">
              <SpheresLib function="IsUpdate()"/>
            </control>
          </controls>
        </xsl:if>
      </column>
      <column name="IDAINS" datakey="false" datakeyupd="false" datatype="int">
        <SpheresLib function="GetUserID()"/>
        <xsl:if test="$pIsWithControl = $gTrue">
          <controls>
            <control action="RejectColumn" return="true" logtype="None">
              <SpheresLib function="IsUpdate()"/>
            </control>
          </controls>
        </xsl:if>
      </column>

      <xsl:call-template name="SysUpd">
        <xsl:with-param name="pIsWithControl" select="$pIsWithControl"/>
      </xsl:call-template>

    </table>

  </xsl:template>

  <xsl:template name="AddSignToRiskValue">
    <xsl:param name="pSign" />
    <xsl:param name="pRiskValue" />
    <xsl:choose>
      <xsl:when test="$pSign = '-'">
        <xsl:value-of select="concat($pSign,$pRiskValue)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$pRiskValue"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- ================================================================== -->
  <!-- ============== IMMARS_H ========================================== -->
  <!-- PM 20170222 [22881][22942] Add template IMMARS_H -->
  <xsl:template name="IMMARS_H">
    <!-- PARAMETRES -->
    <xsl:param name="pIsWithControl" select="true()"/>
    <xsl:param name="pBusinessDate"/>
    <!-- Row -->
    <row id="r0" src="0">
      <table name="IMMARS_H" action="IU">
        <!-- Business Date -->
        <column name="DTBUSINESS" datakey="true" datatype="date" format="YYYY-MM-DD">
          <xsl:value-of select="$pBusinessDate"/>
          <controls>
            <control action="RejectRow" return="false">
              <SpheresLib function="IsDate()" format="YYYY-MM-DD"/>
              <logInfo status="ERROR" isexception="true">
                <message>
                  BusinessDate: Invalid Date (<xsl:value-of select="$pBusinessDate"/>)
                </message>
              </logInfo>
            </control>
          </controls>
        </column>
        <column name="DTINS" datakey="false" datakeyupd="false" datatype="datetime">
          <SpheresLib function="GetUTCDateTimeSys()"/>
          <xsl:if test="$pIsWithControl = $gTrue">
            <controls>
              <control action="RejectColumn" return="true" logtype="None">
                <SpheresLib function="IsUpdate()"/>
              </control>
            </controls>
          </xsl:if>
        </column>
        <column name="IDAINS" datakey="false" datakeyupd="false" datatype="int">
          <SpheresLib function="GetUserID()"/>
          <xsl:if test="$pIsWithControl = $gTrue">
            <controls>
              <control action="RejectColumn" return="true" logtype="None">
                <SpheresLib function="IsUpdate()"/>
              </control>
            </controls>
          </xsl:if>
        </column>
        <xsl:call-template name="SysUpd">
          <xsl:with-param name="pIsWithControl" select="$pIsWithControl"/>
        </xsl:call-template>
      </table>
    </row>
  </xsl:template>

  <!-- Lire et Ajouter la Colonne IDIMMARS_H -->
  <!-- PM 20170222 [22881][22942] Add template Col_IDIMMARS_H -->
  <xsl:template name="Col_IDIMMARS_H">
    <xsl:param name="pBusinessDate"/>
    <column name="IDIMMARS_H" datakey="true" datatype="integer">
      <SQL command="select" result="IDIMMARS_H" cache="true">
        select m.IDIMMARS_H
        from dbo.IMMARS_H m
        where ( m.DTBUSINESS = @BUSINESSDATE )
        <Param name="BUSINESSDATE" datatype="date" format="yyyy-MM-dd">
          <xsl:value-of select="$pBusinessDate" />
        </Param>
      </SQL>
      <controls>
        <control action="RejectRow" return="true">
          <SpheresLib function="IsNull()"/>
          <logInfo status="REJECT" isexception="true">
            <code>SYS</code>
            <number>2006</number>
            <data1>
              <xsl:value-of select="$vFileNameRiskDataImport"/>
            </data1>
            <data2>
              <xsl:value-of select="$vVersionRiskDataImport"/>
            </data2>
          </logInfo>
        </control>
      </controls>
    </column>
  </xsl:template>

  <!-- Vérifier l'existance qu'il y est déjà eu au moins un DC portant sur l'Equity dont l'Isin est en paramètre -->
  <!-- PM 20170222 [22881][22942] Add template ExistsUnderlying -->
  <xsl:template name="ExistsUnderlyingEquity">
    <xsl:param name="pIsinCode"/>

    <SQL command="select" result="EXIST" cache="true">
      select 1 as EXIST
      from dbo.DERIVATIVECONTRACT dc
      inner join dbo.MARKET m on (m.IDM = dc.IDM) and (m.ISO10383_ALPHA4 = 'XDMI')
      inner join dbo.ASSET_EQUITY ae on (ae.IDASSET = dc.IDASSET_UNL) and (ae.ISINCODE = @ISINCODE)
      where (dc.ASSETCATEGORY = 'EquityAsset')
      <Param name="ISINCODE" datatype="string">
        <xsl:value-of select="$pIsinCode" />
      </Param>
    </SQL>
  </xsl:template>

</xsl:stylesheet>
