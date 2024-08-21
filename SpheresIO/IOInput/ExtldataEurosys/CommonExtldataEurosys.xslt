<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="xml" indent="yes"/>

  <!--
==============================================================
 Summary : Common "External Data" SQL request on EUROSYS®
==============================================================
-->
  <xsl:variable name="vVersionCommonExtlData">v1.0.0.0</xsl:variable>
  <xsl:variable name="vFileNameCommonExtlData">CommonExtldataEurosys.xslt</xsl:variable>
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

  <!-- global parameter name containing the enriched exchange code -->
  <xsl:variable name="gbnEnrichedExch">
    <xsl:value-of select="'EnrichedExch'"/>
  </xsl:variable>

  <!-- global parameter name containing the enriched symbol code -->
  <xsl:variable name="gbnEnrichedSym">
    <xsl:value-of select="'EnrichedSym'"/>
  </xsl:variable>

  <!-- global parameter name containing the enriched acct/book identifier -->
  <xsl:variable name="gbnEnrichedAcct">
    <xsl:value-of select="'EnrichedAcct'"/>
  </xsl:variable>

  <!-- global parameter name containing the enriched clearer identifier -->
  <xsl:variable name="gbnEnrichedPty.ID.4">
    <xsl:value-of select="'EnrichedPtyID4'"/>
  </xsl:variable>

  <!-- PL 20101208 Newness -->
  <!-- global parameter name containing the enriched MMY identifier -->
  <xsl:variable name="gbnEnrichedMMY">
    <xsl:value-of select="'EnrichedMMY'"/>
  </xsl:variable>

  <!-- Sql Select IDA from table entity where the input string matches with the relative actor identifier  -->
  <xsl:template name="SelectIDAEntityFromIDENTIFIER">
    <xsl:param name="pEntityString"/>
    <SQL command="select" result="RETDATA">
      select 1 as OB, ent.IDA as RETDATA
      from dbo.ENTITY ent
      inner join dbo.ACTOR a on (a.IDA = ent.IDA) and (a.IDENTIFIER = @ENTITYSTRING)
      union all
      select 2 as OB, min(ent.IDA) as RETDATA
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
    <SQL command="select" result="RETDATA">
      select IDEXTLDATA as RETDATA
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

  <!-- Return the Eurosys® Market identifier, according with the value of the input variable "EXTL_Exch" 
  (EXTL_Acct is declared inside the input parameter "pNames" and its own value has been stocked inside "pValues").
  The input parameter "EXTL_Exch" contains the parsed value for the market, this is an external code we need OPTIONALLY to translate in 
  some internal value. To get the details of specific transcode see the when/case block.
  For the MARKET_TRANSCO mode: the internal value is stocked inside the table MARKET_TRANSCO, column EFS_MARKET -->
  <xsl:template name="SelectEnrichedExch">
    <xsl:param name="pMode"/>
    <xsl:param name="pSourceDefined"/>
    <xsl:param name="pNames"/>
    <xsl:param name="pValues"/>
    <SQL command="select" result="RETDATA">
      <xsl:choose>
        <!-- MARKET_TRANSCO mode, the external value is compared with the EXTERNAL_MARKET column of the MARKET_TRANSCO table. 
        when a match is found the relative internal value EFS_MARKET is returned.-->
        <xsl:when test="$pMode = 'MARKET_TRANSCO'">
          select
          case count(m.MARCHE)
          when 0 then @EXTL_Exch
          else min(m.MARCHE)
          end
          ||
          case count(m.MARCHE)
          when 0 then @NOTFOUND
          when 1 then ''
          else @NOTUNIQUE
          end
          as RETDATA
          from dbo.MARCHE m
          left outer join dbo.MARKET_TRANSCO mt on (mt.EFS_MARKET = m.MARCHE)
          where (
          (m.MARCHE = @EXTL_Exch)
          or (mt.EXTERNAL_MARKET = @EXTL_Exch <xsl:if test="$pSourceDefined = 'true'">and mt.SOURCE = @SOURCE</xsl:if>)
          <xsl:if test="$pSourceDefined = 'true'">
            or (mt.EXTERNAL_MARKET = @EXTL_Exch)
          </xsl:if>
          )
          group by mt.EXTERNAL_MARKET, mt.SOURCE
          order by
          case mt.EXTERNAL_MARKET when @EXTL_Exch then 1 else 2 end
          <xsl:if test="$pSourceDefined = 'true'">
            ,case mt.SOURCE when @SOURCE then 1 else 2 end
          </xsl:if>
        </xsl:when>
        <!-- Standard mode, the default transcode. 
        No transcode is performed, the external value is directly compared to the internal values -->
        <xsl:otherwise>
          select
          case count(m.MARCHE)
          when 0 then @EXTL_Exch
          else min(m.MARCHE)
          end
          ||
          case count(m.MARCHE)
          when 0 then @NOTFOUND
          when 1 then ''
          else @NOTUNIQUE
          end
          as RETDATA
          from dbo.MARCHE m
          where (m.MARCHE = @EXTL_Exch)
        </xsl:otherwise>
      </xsl:choose>
      <xsl:call-template name="SQLParamNodesBuilder">
        <xsl:with-param name="pValues" select="$pValues"/>
        <xsl:with-param name="pNames" select="$pNames"/>
      </xsl:call-template>
    </SQL>
  </xsl:template>

  <!-- PL 20101214 Update -->
  <!-- Return the Eurosys® Product identifier together with existence informations -->
  <xsl:template name="SelectEnrichedSym">
    <xsl:param name="pNames"/>
    <xsl:param name="pValues"/>
    <SQL command="select" result="RETDATA">
      select	case count(p.PRODUIT)
      when 0 then @EXTL_Sym
      else min( deco.PRODUIT )
      end
      ||
      case count(p.PRODUIT)
      when 0 then @NOTFOUND
      when 1 then ''
      else @NOTUNIQUE end as RETDATA
      from dbo.PRODUIT p
      inner join
      (
      select pbcs1.NOM_PRODUIT as PRODUIT, pbcs1.SYMBOL
      from dbo.PRODUIT_BCS_CLASSE pbcs1
      where ( pbcs1.SYMBOL = @EXTL_Sym ) and ( pbcs1.PRODUCTTYPE=case when @PUTCALL in ('0','1') then 'O' else 'F' end )
      union all
      select p1.PRODUIT, p1.PRODUIT as SYMBOL
      from dbo.PRODUIT p1
      where not exists (
      select 1
      from dbo.PRODUIT_BCS_CLASSE pbcs2
      where ( pbcs2.SYMBOL = @EXTL_Sym ) and ( pbcs2.PRODUCTTYPE=case when @PUTCALL in ('0','1') then 'O' else 'F' end )
      )
      ) deco on (deco.PRODUIT=p.PRODUIT)
      where (deco.SYMBOL = @EXTL_Sym)
      <xsl:call-template name="SQLParamNodesBuilder">
        <xsl:with-param name="pValues" select="$pValues"/>
        <xsl:with-param name="pNames" select="$pNames"/>
      </xsl:call-template>
    </SQL>
  </xsl:template>

  <!-- Return an Eurosys® Buyer/Seller Account identifier, according with the value of the input variable "EXTL_Acct" 
  (EXTL_Acct is declared inside the input parameter "pNames" and its own value has been stocked inside "pValues").
  The input parameter "EXTL_Acct" contains the parsed value for the account, this is an external code we need OPTIONALLY to translate in 
  some internal value. To get the details of specific transcode see the when/case block. 
  For the BANQUE mode: the internal value is stocked inside the table COMPTE, column NUM_COMPTE-->
  <xsl:template name="SelectEnrichedAcct">
    <xsl:param name="pMode"/>
    <xsl:param name="pNames"/>
    <xsl:param name="pValues"/>
    <SQL command="select" result="RETDATA">
      <xsl:choose>
        <!-- BANQUE mode, the external value is compared with the NUM_CPT_BDF column of the BANQUE table. 
        when a match is found the relative internal value is returned.-->
        <xsl:when test="$pMode = 'BANQUE'">
          select
          case count(c.NUM_COMPTE)
          when 0 then @EXTL_Acct
          else min(c.NUM_COMPTE)
          end
          ||
          case count(c.NUM_COMPTE)
          when 0 then @NOTFOUND
          when 1 then ''
          else @NOTUNIQUE
          end
          as RETDATA
          from dbo.COMPTE c
          left outer join dbo.BANQUE b on (c.NUM_COMPTE = b.NUM_PROPRIET)
          where (c.NUM_COMPTE = @EXTL_Acct or b.NUM_CPT_BDF = @EXTL_Acct)
          and (c.TYP_COMPTE not in ('BROKER','INTER'))
        </xsl:when>
        <!-- Standard mode, the default transcode. 
        No transcode is performed, the external value is directly compared to the internal values -->
        <xsl:otherwise>
          select
          case count(c.NUM_COMPTE)
          when 0 then @EXTL_Acct
          else min(c.NUM_COMPTE)
          end
          ||
          case count(c.NUM_COMPTE)
          when 0 then @NOTFOUND
          when 1 then ''
          else @NOTUNIQUE
          end
          as RETDATA
          from dbo.COMPTE c
          where (c.NUM_COMPTE = @EXTL_Acct) and (c.TYP_COMPTE not in ('BROKER','INTER'))
        </xsl:otherwise>
      </xsl:choose>
      <xsl:call-template name="SQLParamNodesBuilder">
        <xsl:with-param name="pValues" select="$pValues"/>
        <xsl:with-param name="pNames" select="$pNames"/>
      </xsl:call-template>
    </SQL>
  </xsl:template>

  <!-- return the Eurosys® Clearing Broker Account identifier together with existence informations -->
  <xsl:template name="SelectEnrichedPty.ID.4">
    <xsl:param name="pNames"/>
    <xsl:param name="pValues"/>
    <SQL command="select" result="RETDATA">
      select
      case count(c.NUM_COMPTE)
      when 0 then @EXTL_PtyID4
      else min(c.NUM_COMPTE)
      end
      ||
      case count(c.NUM_COMPTE)
      when 0 then @NOTFOUND
      when 1 then ''
      else @NOTUNIQUE
      end
      as RETDATA
      from dbo.COMPTE c
      where (c.NUM_COMPTE = @EXTL_PtyID4) and (c.TYP_COMPTE = 'BROKER')
      <xsl:call-template name="SQLParamNodesBuilder">
        <xsl:with-param name="pValues" select="$pValues"/>
        <xsl:with-param name="pNames" select="$pNames"/>
      </xsl:call-template>
    </SQL>
  </xsl:template>

  <!-- PL 20101213 Newness -->
  <xsl:template name="SelectEnrichedMMY">
    <xsl:param name="pNames"/>
    <xsl:param name="pValues"/>
    <!-- SQL command="select" result="RETDATA">
      select
      case count(e.NOM_ECHE)
      when 0 then @EXTL_MMY
      else '20' || substring(min(e.NOM_ECHE),4,2) || substring(min(e.NOM_ECHE),1,2)
      end
      ||
      case count(e.NOM_ECHE)
      when 0 then @NOTFOUND
      when 1 then ''
      else @NOTUNIQUE
      end
      as RETDATA
      from dbo.ECHEANCE e
      left outer join PRODUIT_BCS_CLASSE pbcs on (pbcs.NOM_PRODUIT=e.PRODUIT) and (pbcs.PRODUCTTYPE=case when @PUTCALL in ('0','1') then 'O' else 'F' end)
      where (e.DATE_CLT_SYS = @EXTL_MMY) and (case when pbcs.SYMBOL is null then e.PRODUIT else pbcs.SYMBOL end = @SYM)
      <xsl:call-template name="SQLParamNodesBuilder">
        <xsl:with-param name="pValues" select="$pValues"/>
        <xsl:with-param name="pNames" select="$pNames"/>
      </xsl:call-template>
    </SQL -->
    <SQL command="select" result="RETDATA">
      select
      case count(e.NOM_ECHE)
      when 0 then @EXTL_MMY
      else '20' + substring(min(e.NOM_ECHE),4,2) + substring(min(e.NOM_ECHE),1,2)
      end
      ||
      case count(e.NOM_ECHE)
      when 0 then @NOTFOUND
      when 1 then ''
      else @NOTUNIQUE
      end
      as RETDATA
      from dbo.ECHEANCE e
      left outer join dbo.PRODUIT_BCS_CLASSE pbcs on (pbcs.NOM_PRODUIT=e.PRODUIT) and (pbcs.PRODUCTTYPE=case when @PUTCALL in ('0','1') then 'O' else 'F' end)
      where (convert(varchar(10),e.DATE_CLT_SYS,126) = @EXTL_MMY)
      and ((pbcs.SYMBOL = @SYM)
      or ((e.PRODUIT = @SYM) and not exists( select 1 from dbo.PRODUIT_BCS_CLASSE pbcs1 where (pbcs1.SYMBOL = @SYM) and (pbcs1.PRODUCTTYPE=case when @PUTCALL in ('0','1') then 'O' else 'F' end))))
      <xsl:call-template name="SQLParamNodesBuilder">
        <xsl:with-param name="pValues" select="$pValues"/>
        <xsl:with-param name="pNames" select="$pNames"/>
      </xsl:call-template>
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
                &lt;b&gt;EXTLDATA reset rejected, NEW datas.&lt;/b&gt;
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
