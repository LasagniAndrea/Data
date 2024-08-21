<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
    <xsl:output method="xml" indent="yes"/>
  <!--  
    ================================================================================================================
                                                       HISTORY OF THE MODIFICATIONS

    Version 3.4.0.0_4
    Date: 18/06/2013
    Author: RD
    Description: Use Market in templates "SelectEnrichedSymOrIdentifier" and "SelectEnrichedSym"
    
    Version 3.3.0.0_3
    Date: 13/05/2013
    Author: RD
    Description: Manage task parameter "EXTLLINK_COLUMN" in SelectEnrichedPty.ID.27 template
    
    Version 2.6.0.0_2
    Date: 13/02/2012
    Author: MF
    Description: 
    
    Version 2.6.0.0_1
    Date: 17/03/2011
    Author: MF
    Description: first version

    ================================================================================================================
  -->

  <xsl:variable name="vVersionCommonExtlData">v1.0.0.0</xsl:variable>
  <xsl:variable name="vFileNameCommonExtlData">CommonExtldata.xslt</xsl:variable>
  <xsl:variable name="vProcessingFileName">
    <!--<xsl:value-of select="tokenize(document-uri(.), '/')[last()]"/>-->
  </xsl:variable>
<!--  
==============================================================
-->

  <xsl:include href="..\Common\CommonInputSQL.xslt"/>

  <!-- UNDONE : Temporary hardcoded value for the AcctIDSrcTyp attribute -->
  <xsl:variable name="vAcctIDSrcTyp">
    <xsl:value-of select="'99'"/>
  </xsl:variable>

  <!-- global parameter name containing the EXTLDATADET.IDEXTLDATA field -->
  <xsl:variable name="gbnIDExtlData">
    <xsl:value-of select="'IDExtlData'"/>
  </xsl:variable>

  <!-- global parameter name containing the EXTLDATADET.IDI field -->
  <xsl:variable name="gbnIDI">
    <xsl:value-of select="'IDI'"/>
  </xsl:variable>

  <!-- global parameter name containing the EXTLDATADET.IDM field -->
  <xsl:variable name="gbnIDM">
    <xsl:value-of select="'IDM'"/>
  </xsl:variable>

  <!-- global parameter name containing the enriched exchange code -->
  <xsl:variable name="gbnEnrichedExch">
    <xsl:value-of select="'EnrichedExch'"/>
  </xsl:variable>

  <!-- global parameter name containing the enriched acct/book identifier -->
  <xsl:variable name="gbnEnrichedAcct">
    <xsl:value-of select="'EnrichedAcct'"/>
  </xsl:variable>

  <!-- global parameter name containing the enriched buyer/seller identifier -->
  <xsl:variable name="gbnEnrichedPty.ID.27">
    <xsl:value-of select="'EnrichedPtyID27'"/>
  </xsl:variable>

  <!-- global parameter name containing the enriched clearer identifier -->
  <xsl:variable name="gbnEnrichedPty.ID.4">
    <xsl:value-of select="'EnrichedPtyID4'"/>
  </xsl:variable>
  
  <!-- global parameter name containing the enriched sym (today it is the contract identifier) -->
  <xsl:variable name="gbnEnrichedSym">
    <xsl:value-of select="'EnrichedSym'"/>
  </xsl:variable>

<!-- Sql Select IDA from table entity where the input string matches with the relative actor identifier  -->
  <xsl:template name="SelectIDAEntityFromIDENTIFIER">
    <xsl:param name="pEntityString"/>
    <SQL command="select" result="IDA">
      select 1 as OB, ent.IDA
      from dbo.ENTITY ent
      inner join dbo.ACTOR a on (a.IDA = ent.IDA) and (a.IDENTIFIER = @ENTITYSTRING)
      union all
      select 2 as OB, min(ent.IDA)
      from dbo.ENTITY ent
      order by OB asc
      <Param name="ENTITYSTRING" datatype="string">
        <xsl:value-of select="$pEntityString"/>
      </Param>
    </SQL>
  </xsl:template>

  <!-- Sql Select ID from table EXTLDATA according with the given process and task input parameters -->
  <xsl:template name="SelectIDExtlData">
    <xsl:param name="pIoTaskDet"/>
    <SQL command="select" result="IDEXTLDATA">
      select IDEXTLDATA
      from dbo.EXTLDATA
      where (IDIOTASKDET = @IDIOTASKDET) and (IDPROCESS_L = @IDPROCESSL)
      <Param name="IDPROCESSL" datatype="integer">
        <SpheresLib function="GetProcessLogID()"/>
      </Param>
      <Param name="IDIOTASKDET" datatype="integer">
        <xsl:value-of select="$pIoTaskDet"/>
      </Param>
    </SQL>
  </xsl:template>

  <!-- Sql Select the IDI where the instrument input string matches the transcoded instrument identifier -->
  <xsl:template name="SelectIDIFromIDENTIFIER">
    <xsl:param name="pNames"/>
    <xsl:param name="pValues"/>
    <SQL command="select" result="IDI">
      select i.IDI
      from dbo.INSTRUMENT i
      where (i.IDENTIFIER = @INSTRUMENT)
      <xsl:call-template name="SQLParamNodesBuilder">
        <xsl:with-param name="pValues" select="$pValues"/>
        <xsl:with-param name="pNames" select="$pNames"/>
      </xsl:call-template>
    </SQL>
  </xsl:template>

  <!-- Sql Select the IDM matching the external exchange symbol -->
  <xsl:template name="SelectIDM">
    <xsl:param name="pResult"/>
    <xsl:param name="pExternalLink" select="'EXTLLINK'"/>
    <xsl:param name="pNames"/>
    <xsl:param name="pValues"/>
    <SQL command="select">
      <xsl:attribute name="result">
        <xsl:value-of select="$pResult"/>
      </xsl:attribute>
      select m.IDM
      from dbo.MARKET m
      where (m.<xsl:value-of select="$pExternalLink"/> = @EXTL_Exch)
      <xsl:call-template name="SQLParamNodesBuilder">
        <xsl:with-param name="pValues" select="$pValues"/>
        <xsl:with-param name="pNames" select="$pNames"/>
      </xsl:call-template>
    </SQL>
  </xsl:template>

  <!-- Return the external exchange symbol together with existence informations -->
  <xsl:template name="SelectEnrichedExch">
    <xsl:param name="pExternalLink" select="'EXTLLINK'"/>
    <xsl:param name="pNames"/>
    <xsl:param name="pValues"/>
    <SQL command="select">
      <xsl:attribute name="result">
        <xsl:value-of select="$gbnEnrichedExch"/>
      </xsl:attribute>
      select
      case count(m.IDM)
      when 0 then @EXTL_Exch
      else min(m.ISO10383_ALPHA4)
      end
      ||
      case count(m.IDM)
      when 0 then @NOTFOUND
      when 1 then ''
      else @NOTUNIQUE
      end
      as <xsl:value-of select="$gbnEnrichedExch"/>
      from dbo.MARKET m
      where (m.<xsl:value-of select="$pExternalLink"/> = @EXTL_Exch)
      <xsl:call-template name="SQLParamNodesBuilder">
        <xsl:with-param name="pValues" select="$pValues"/>
        <xsl:with-param name="pNames" select="$pNames"/>
      </xsl:call-template>
    </SQL>
  </xsl:template>

  <!-- return the Acct/Bok identifier together with existence informations -->
  <xsl:template name="SelectEnrichedAcct">
    <xsl:param name="pExternalLink" select="'EXTLLINK'"/>
    <xsl:param name="pNames"/>
    <xsl:param name="pValues"/>
    <SQL command="select">
      <xsl:attribute name="result">
        <xsl:value-of select="$gbnEnrichedAcct"/>
      </xsl:attribute>
      select
      case count(b.IDB)
      when 0 then @EXTL_Acct
      else min(b.IDENTIFIER)
      end
      ||
      case count(b.IDB)
      when 0 then @NOTFOUND
      when 1 then ''
      else @NOTUNIQUE
      end
      as <xsl:value-of select="$gbnEnrichedAcct"/>
      from dbo.BOOK b
      inner join dbo.ACTOR a on (a.IDA=b.IDA) and (a.<xsl:value-of select="$pExternalLink"/> = @EXTL_PtyID27)
      where (b.<xsl:value-of select="$pExternalLink"/>= @EXTL_Acct)
      <xsl:call-template name="SQLParamNodesBuilder">
        <xsl:with-param name="pValues" select="$pValues"/>
        <xsl:with-param name="pNames" select="$pNames"/>
      </xsl:call-template>
    </SQL>
  </xsl:template>

  <!-- return the Book identifier together with existence informations -->
  <xsl:template name="SelectEnrichedAcctOnBookOnly">
    <xsl:param name="pExternalLink" select="'EXTLLINK'"/>
    <xsl:param name="pNames"/>
    <xsl:param name="pValues"/>
    <SQL command="select">
      <xsl:attribute name="result">
        <xsl:value-of select="$gbnEnrichedAcct"/>
      </xsl:attribute>
      select
      case count(b.IDB)
      when 0 then @EXTL_Acct
      else min(b.IDENTIFIER)
      end
      ||
      case count(b.IDB)
      when 0 then @NOTFOUND
      when 1 then ''
      else @NOTUNIQUE
      end
      as <xsl:value-of select="$gbnEnrichedAcct"/>
      from dbo.BOOK b
      where (b.<xsl:value-of select="$pExternalLink"/> = @EXTL_Acct)
      <xsl:call-template name="SQLParamNodesBuilder">
        <xsl:with-param name="pValues" select="$pValues"/>
        <xsl:with-param name="pNames" select="$pNames"/>
      </xsl:call-template>
    </SQL>
  </xsl:template>

  <!-- return the Buyer/Seller identifier together with existence informations -->
  <xsl:template name="SelectEnrichedPty.ID.27">
    <xsl:param name="pExternalLink" select="'EXTLLINK'"/>
    <xsl:param name="pNames"/>
    <xsl:param name="pValues"/>
    <SQL command="select">
      <xsl:attribute name="result">
        <xsl:value-of select="$gbnEnrichedPty.ID.27"/>
      </xsl:attribute>
      select
      case count(a.IDA)
      when 0 then @EXTL_PtyID27
      else min(a.IDENTIFIER)
      end
      ||
      case count(a.IDA)
      when 0 then @NOTFOUND
      when 1 then ''
      else @NOTUNIQUE
      end
      as <xsl:value-of select="$gbnEnrichedPty.ID.27"/>
      from dbo.ACTOR a
      where (a.<xsl:value-of select="$pExternalLink"/> = @EXTL_PtyID27)
      <xsl:call-template name="SQLParamNodesBuilder">
        <xsl:with-param name="pValues" select="$pValues"/>
        <xsl:with-param name="pNames" select="$pNames"/>
      </xsl:call-template>
    </SQL>
  </xsl:template>

  <!-- return the Clearer identifier together with existence informations -->
  <xsl:template name="SelectEnrichedPty.ID.4">
    <xsl:param name="pNames"/>
    <xsl:param name="pValues"/>
    <SQL command="select">
      <xsl:attribute name="result">
        <xsl:value-of select="$gbnEnrichedPty.ID.4"/>
      </xsl:attribute>
      select
      case count(a.IDA)
      when 0 then @EXTL_PtyID4
      else min(A.IDENTIFIER)
      end
      ||
      case count(a.IDA)
      when 0 then @NOTFOUND
      when 1 then ''
      else @NOTUNIQUE
      end
      as <xsl:value-of select="$gbnEnrichedPty.ID.4"/>
      from dbo.ACTOR a
      inner join dbo.ACTORROLE ar on (ar.IDA=a.IDA)
      inner join dbo.ROLEACTOR ra on (ra.IDROLEACTOR=ar.IDROLEACTOR) and (ra.IDROLEACTOR='CLEARER')
      where (a.EXTLLINK = @EXTL_PtyID4)
      <xsl:call-template name="SQLParamNodesBuilder">
        <xsl:with-param name="pValues" select="$pValues"/>
        <xsl:with-param name="pNames" select="$pNames"/>
      </xsl:call-template>
    </SQL>
  </xsl:template>

  <!-- Return the contract identifier starting form the input contract symbol/external link and instrument -->
  <xsl:template name="Deprecated_SelectEnrichedSym">
    <xsl:param name="pExternalSymbol" select="'CONTRACTSYMBOL'"/>
    <xsl:param name="pNames"/>
    <xsl:param name="pValues"/>
    
    <SQL command="select">
      <xsl:attribute name="result">
        <xsl:value-of select="$gbnEnrichedSym"/>
      </xsl:attribute>
      select dc.IDENTIFIER
      as <xsl:value-of select="$gbnEnrichedSym"/>
      from dbo.DERIVATIVECONTRACT dc
      inner join dbo.INSTRUMENT i on (i.IDI = dc.IDI)
      where
      dc.<xsl:value-of select="$pExternalSymbol"/> = @SYMBOL
      and i.IDENTIFIER = @INSTRUMENT
      <xsl:call-template name="SQLParamNodesBuilder">
        <xsl:with-param name="pValues" select="$pValues"/>
        <xsl:with-param name="pNames" select="$pNames"/>
      </xsl:call-template>
    </SQL>
  </xsl:template>

  <!--RD 20140717 [19928] Gérer le cas des DC annulés.-->
  <xsl:template name="SelectEnrichedSym">
    <xsl:param name="pExternalLink" select="'EXTLLINK'"/>
    <xsl:param name="pExternalSymbol" select="'CONTRACTSYMBOL'"/>
    <xsl:param name="pNames"/>
    <xsl:param name="pValues"/>
    <xsl:param name="pDtBusiness"/>
    
    <SQL command="select">
      <xsl:attribute name="result">
        <xsl:value-of select="$gbnEnrichedSym"/>
      </xsl:attribute>
      select dc.IDENTIFIER
      as <xsl:value-of select="$gbnEnrichedSym"/>
      from dbo.DERIVATIVECONTRACT dc
      inner join dbo.INSTRUMENT i on (i.IDI = dc.IDI)
      inner join dbo.MARKET m on (m.IDM = dc.IDM)
      where (dc.<xsl:value-of select="$pExternalSymbol"/> = @SYMBOL)
      and (i.IDENTIFIER = @INSTRUMENT)
      and (m.<xsl:value-of select="$pExternalLink"/> = @EXTL_Exch)
      and (dc.DTENABLED&lt;=@DT) and (dc.DTDISABLED is null or dc.DTDISABLED>@DT)
      <xsl:call-template name="SQLParamNodesBuilder">
        <xsl:with-param name="pValues" select="$pValues"/>
        <xsl:with-param name="pNames" select="$pNames"/>
      </xsl:call-template>
      <Param name="DT" datatype="date">
        <xsl:value-of select="$pDtBusiness"/>
      </Param>
    </SQL>
  </xsl:template>

  <!-- Return the contract identifier starting form the input contract symbol/external link and instrument, whether 
  the enriched symbol in case of the input symbol is not related to any contracts, either many contracts are related to-->
  <xsl:template name="Deprecated_SelectEnrichedSymOrIdentifier">
    <xsl:param name="pExternalSymbol" select="'CONTRACTSYMBOL'"/>
    <xsl:param name="pNames"/>
    <xsl:param name="pValues"/>
    <SQL command="select">
      <xsl:attribute name="result">
        <xsl:value-of select="$gbnEnrichedSym"/>
      </xsl:attribute>
      select
      case count(dc.IDDC)
      when 0 then @SYMBOL
      else min(dc.IDENTIFIER)
      end
      ||
      case count(dc.IDDC)
      when 0 then @NOTFOUND
      when 1 then ''
      else @NOTUNIQUE
      end
      as <xsl:value-of select="$gbnEnrichedSym"/>
      from dbo.DERIVATIVECONTRACT dc
      inner join dbo.INSTRUMENT i on (i.IDI = dc.IDI)
      where
      dc.<xsl:value-of select="$pExternalSymbol"/> = @SYMBOL
      and i.IDENTIFIER = @INSTRUMENT
      <xsl:call-template name="SQLParamNodesBuilder">
        <xsl:with-param name="pValues" select="$pValues"/>
        <xsl:with-param name="pNames" select="$pNames"/>
      </xsl:call-template>
    </SQL>
  </xsl:template>

  <!-- Return the contract identifier starting form the input contract symbol/external link and instrument, whether 
  the enriched symbol in case of the input symbol is not related to any contracts, either many contracts are related to-->
  <!--RD 20140717 [19928] Gérer le cas des DC annulés.-->
  <xsl:template name="SelectEnrichedSymOrIdentifier">
    <xsl:param name="pExternalLink" select="'EXTLLINK'"/>
    <xsl:param name="pExternalSymbol" select="'CONTRACTSYMBOL'"/>
    <xsl:param name="pNames"/>
    <xsl:param name="pValues"/>
    <xsl:param name="pDtBusiness"/>
    
    <SQL command="select">
      <xsl:attribute name="result">
        <xsl:value-of select="$gbnEnrichedSym"/>
      </xsl:attribute>
      select
      case count(dc.IDDC)
      when 0 then @SYMBOL
      else min(dc.IDENTIFIER)
      end
      ||
      case count(dc.IDDC)
      when 0 then @NOTFOUND
      when 1 then ''
      else @NOTUNIQUE
      end
      as <xsl:value-of select="$gbnEnrichedSym"/>
      from dbo.DERIVATIVECONTRACT dc
      inner join dbo.INSTRUMENT i on (i.IDI = dc.IDI)
      inner join dbo.MARKET m on (m.IDM = dc.IDM)
      where (dc.<xsl:value-of select="$pExternalSymbol"/> = @SYMBOL)
      and (i.IDENTIFIER = @INSTRUMENT)
      and (m.<xsl:value-of select="$pExternalLink"/> = @EXTL_Exch)
      and (dc.DTENABLED&lt;=@DT) and (dc.DTDISABLED is null or dc.DTDISABLED>@DT)
      <xsl:call-template name="SQLParamNodesBuilder">
        <xsl:with-param name="pValues" select="$pValues"/>
        <xsl:with-param name="pNames" select="$pNames"/>
      </xsl:call-template>
      <Param name="DT" datatype="date">
        <xsl:value-of select="$pDtBusiness"/>
      </Param>
    </SQL>
  </xsl:template>

  <!-- Insert a row in the EXTLDATA table -->
  <xsl:template name="EXTLDATA">
    <xsl:param name="pIsWithControl" select="true()"/>
    <xsl:param name="pIdIoTaskDet" select="$NULL"/>
    <xsl:param name="pFileName" select="$NULL"/>
    <xsl:param name="pDtFile" select="$NULL"/>
    <xsl:param name="pBusinessType" select="$vBusinessType"/>
    <xsl:param name="pMessage" select="$NULL"/>
    <xsl:param name="pLoFileContent" select="$NULL"/>
    <xsl:param name="pEntityString" select="$NULL"/>
    <xsl:param name="pSize"/>

    <table name="EXTLDATA" action="IU" sequenceno="1">

      <!--<column name="IDEXTLDATA" datakey="false" datatype="integer">
        <xsl:value-of select="$NULL"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="None">
            true
          </control>
        </controls>
      </column>-->

      <column name="IDPROCESS_L" datakey="true" datatype="integer">
        <SpheresLib function="GetProcessLogID()"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Process ID unknown.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The parsing process failed.
                &lt;b&gt;Action:&lt;/b&gt; Restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFileNameCommonExtlData"/>
                <xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersionCommonExtlData"/>
                <xsl:text>&#160;</xsl:text><xsl:value-of select="$vProcessingFileName"/>
                <xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>

      <column name="IDIOTASKDET" datakey="true" datatype="integer">
        <xsl:value-of select="$pIdIoTaskDet"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Task ID unknown.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The parsing process failed.
                &lt;b&gt;Action:&lt;/b&gt; Restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFileNameCommonExtlData"/>
                <xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersionCommonExtlData"/>
                <xsl:text>&#160;</xsl:text><xsl:value-of select="$vProcessingFileName"/>
                <xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>

      <column name="FILENAME" datakey="false" datatype="string">
        <xsl:value-of select="$pFileName"/>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="DTFILE" datakey="false" datatype="datetime">
        <xsl:value-of select="$pDtFile"/>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="SIZEFILE" datakey="false" datatype="string">
        <xsl:value-of select="$pSize"/>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="DTBUSINESS" datakey="false" datatype="datetime">
        <SpheresLib format="yyyy-MM-dd" function="GetParameter()">
          <Param>DTBUSINESS</Param>
        </SpheresLib>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="BUSINESSTYPE" datakey="false" datatype="string">
        <xsl:value-of select="$pBusinessType"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Null business type.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The parsing rule-set could be uncomplete.
                &lt;b&gt;Action:&lt;/b&gt; Patch the parsing rule-set and restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFileNameCommonExtlData"/>
                <xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersionCommonExtlData"/>
                <xsl:text>&#160;</xsl:text><xsl:value-of select="$vProcessingFileName"/>
                <xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>

      <column name="MESSAGE" datakey="false" datatype="string">
        <xsl:value-of select="$pMessage"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="NONE">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="NONE"></logInfo>
          </control>
        </controls>
      </column>

      <column name="LOFILECONTENT" datakey="false" datatype="string">
        <xsl:value-of select="$pLoFileContent"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="NONE">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="NONE"></logInfo>
          </control>
        </controls>
      </column>

      <xsl:variable name="vSelectIDAEntity">
        <xsl:call-template name="SelectIDAEntityFromIDENTIFIER">
          <xsl:with-param name="pEntityString" select="$pEntityString"/>
        </xsl:call-template>
      </xsl:variable>

      <column name="IDA_ENTITY" datakey="false" datatype="integer">
        <xsl:copy-of select="$vSelectIDAEntity"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;The owner's entity is unknown.&lt;/b&gt; 
                &lt;b&gt;Cause:&lt;/b&gt; Lacking parameter value.
                &lt;b&gt;Action:&lt;/b&gt; Check the task parameters set. Restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFileNameCommonExtlData"/>
                <xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersionCommonExtlData"/>
                <xsl:text>&#160;</xsl:text><xsl:value-of select="$vProcessingFileName"/>
                <xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>

      <xsl:call-template name="SysIns">
        <xsl:with-param name="pIsWithControl" select="$pIsWithControl"/>
      </xsl:call-template>
      
    </table>
    
  </xsl:template>

  <!-- Insert a row in the EXTLDATADET table -->
  <xsl:template name="EXTLDATADET">
    <xsl:param name="pIsWithControl" select="true()"/>
    <xsl:param name="pMessage" select="$NULL"/>
    <xsl:param name="pDataRowNumber" select="$NULL"/>
    <xsl:param name="pDataInput" select="$NULL"/>
    <xsl:param name="pDataXML" select="$NULL"/>
    <xsl:param name="pDtData" select="$NULL"/>
    <!--<xsl:param name="pSelectIDExtlData" select="$NULL"/>-->
    <xsl:param name="pDtBusinessParam" select="$NULL"/>
    <xsl:param name="pDtBusinessTrade" select="$NULL"/>

    <table name="EXTLDATADET" action="I" sequenceno="2">

      <!-- check when the importing element has a wrong business date according with the given business date parameter -->
      <xsl:if test="$pDtBusinessParam != $pDtBusinessTrade">
      
        <column name="ERRORBIZDATE" datakey="false" datatype="string">
          <xsl:value-of select="$NULL"/>
          <controls>
            <control action="RejectRow" return="true">
              true
              <logInfo status="REJECT" isexception="true">
                <message>
                  &lt;b&gt;Line has been rejected.&lt;/b&gt;
                  &lt;b&gt;Cause:&lt;/b&gt; The business date [<xsl:value-of select="$pDtBusinessTrade"/>]
                  does not match the given business date value [<xsl:value-of select="$pDtBusinessParam"/>] .
                  <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFileNameCommonExtlData"/>
                  <xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersionCommonExtlData"/>
                  <xsl:text>&#160;</xsl:text><xsl:value-of select="$vProcessingFileName"/>
                  <xsl:text>&#xa;</xsl:text>
                </message>
              </logInfo>
            </control>
          </controls>
        </column>

      </xsl:if>

      <column name="IDEXTLDATA" datakey="false" datatype="integer">
        parameters.<xsl:value-of select="$gbnIDExtlData"/>
        <!--<xsl:copy-of select="$pSelectIDExtlData"/>-->
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;File reference unknown.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The File information insertion failed.
                &lt;b&gt;Action:&lt;/b&gt; Restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFileNameCommonExtlData"/>
                <xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersionCommonExtlData"/>
                <xsl:text>&#160;</xsl:text><xsl:value-of select="$vProcessingFileName"/>
                <xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>

      <column name="MESSAGE" datakey="false" datatype="string">
        <xsl:value-of select="$pMessage"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="NONE">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="NONE"></logInfo>
          </control>
        </controls>
      </column>

      <column name="DATAROWNUMBER" datakey="false" datatype="integer">
        <xsl:value-of select="$pDataRowNumber"/>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="DATAINPUT" datakey="false" datatype="string">
        <xsl:value-of select="$pDataInput"/>
        <controls>
          <control action="RejectColumn" return="true" logtype="NONE">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="NONE"></logInfo>
          </control>
        </controls>
      </column>

      <column name="DATAXML" datakey="false" datatype="xml">
        <xsl:choose>
          <!-- test if pDataXML is a element node -->
          <xsl:when test="$pDataXML != 'null'">
            <xsl:copy-of select="$pDataXML"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$pDataXML"/>
          </xsl:otherwise>
        </xsl:choose>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
          </control>
        </controls>
      </column>

      <column name="DTDATA" datakey="false" datatype="datetime">
        <xsl:value-of select="$pDtData"/>
        <controls>
          <control action="RejectRow" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="true">
              <message>
                &lt;b&gt;Unknown trade datetime.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The parsing rule-set could be uncomplete.
                &lt;b&gt;Action:&lt;/b&gt; Patch the parsing rule-set and restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFileNameCommonExtlData"/>
                <xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersionCommonExtlData"/>
                <xsl:text>&#160;</xsl:text><xsl:value-of select="$vProcessingFileName"/>
                <xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>

      <column name="IDDATA" datakey="false" datatype="string">
        null
        <controls>
          <control action="RejectColumn" return="true" logtype="NONE">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="NONE"></logInfo>
          </control>
        </controls>
      </column>

      <column name="IDI" datakey="false" datatype="integer">
        parameters.<xsl:value-of select="$gbnIDI"/>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo old_status="REJECT" status="NONE" isexception="false">
              <message>
                &lt;b&gt;Unknown instrument identifier.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The parsing rule-set could be uncomplete.
                &lt;b&gt;Action:&lt;/b&gt; Patch the parsing rule-set and restart the import.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFileNameCommonExtlData"/>
                <xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersionCommonExtlData"/>
                <xsl:text>&#160;</xsl:text><xsl:value-of select="$vProcessingFileName"/>
                <xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>

      <column name="IDM" datakey="false" datatype="integer">
        parameters.<xsl:value-of select="$gbnIDM"/>
        <controls>
          <control action="RejectColumn" return="true">
            <SpheresLib function="ISNULL()"/>
            <logInfo old_status="REJECT" status="NONE" isexception="false">
              <message>
                &lt;b&gt;Unknown market identifier.&lt;/b&gt;
                &lt;b&gt;Cause:&lt;/b&gt; The spheres DB does not contain the imported exchange code.
                &lt;b&gt;Action:&lt;/b&gt; Check the exchange code at the row number <xsl:value-of select="$pDataRowNumber"/>. 
                Contact the EFS support to patch the Spheres DB.
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFileNameCommonExtlData"/>
                <xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersionCommonExtlData"/>
                <xsl:text>&#160;</xsl:text><xsl:value-of select="$vProcessingFileName"/>
                <xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>

      <xsl:call-template name="SysIns">
        <xsl:with-param name="pIsWithControl" select="$pIsWithControl"/>
        <xsl:with-param name="pNoIdaIns" select="true()"/>
      </xsl:call-template>

    </table>
  </xsl:template>

  <!-- Delete from EXTLDATA et EXTLDATADET -->
  <xsl:template name="Reset">
    <xsl:param name="pIoTaskDet" select="$NULL"/>
    <xsl:param name="pBusinessType" select="$NULL"/>

    <xsl:variable name="vSelectIDExtlData">
      <xsl:call-template name="SelectIDExtlData">
        <xsl:with-param name="pIoTaskDet" select="$pIoTaskDet"/>
      </xsl:call-template>
    </xsl:variable>

    <table name="EXTLDATA" action="D" sequenceno="0">

      <column name="IDEXTLDATA" datakey="false" datatype="string">
        <xsl:copy-of select="$vSelectIDExtlData"/>
        <controls>
          <!-- 
          The delete operation is rejected when the EXTLDATA element returned by SelectIDExtlData, related to
          the current process_l and current iotaskdet of the input xml file, is NOT NULL. The Id MUST reference a previously 
          imported dataset 
          -->
          <control action="RejectRow" return="false">
            <SpheresLib function="ISNULL()"/>
            <logInfo status="REJECT" isexception="false">
              <message>
                &lt;b&gt;EXTLDATA reset rejected, the target datas are newly imported.&lt;/b&gt;
                <xsl:text>&#xa;</xsl:text>xsl-t: <xsl:value-of select="$vFileNameCommonExtlData"/>
                <xsl:text>&#160;</xsl:text><xsl:value-of select="$vVersionCommonExtlData"/>
                <xsl:text>&#160;</xsl:text><xsl:value-of select="$vProcessingFileName"/>
                <xsl:text>&#xa;</xsl:text>
              </message>
            </logInfo>
          </control>
        </controls>
      </column>

      <column name="DTBUSINESS" datakey="true" datatype="datetime">
        <SpheresLib format="yyyy-MM-dd" function="GetParameter()">
          <Param>DTBUSINESS</Param>
        </SpheresLib>
      </column>

      <column name="BUSINESSTYPE" datakey="true" datatype="string">
        <xsl:value-of select="$pBusinessType"/>
      </column>

    </table>

  </xsl:template>
</xsl:stylesheet>
