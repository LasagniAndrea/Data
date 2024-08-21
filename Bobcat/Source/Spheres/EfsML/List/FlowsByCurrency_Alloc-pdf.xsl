<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  xmlns:dt="http://xsltsl.org/date-time"
  xmlns:fo="http://www.w3.org/1999/XSL/Format"
  version="1.0">

  <xsl:param name="pCurrentCulture" select="'en-GB'"/>

  <!-- xslt includes -->
  <xsl:include href="..\..\Library\Resource.xslt"/>
  <xsl:include href="..\..\Library\DtFunc.xslt"/>
  <xsl:include href="..\..\Library\NbrFunc.xslt" />
  <xsl:include href="..\..\Library\xsltsl\date-time.xsl"/>

  <xsl:variable name="vIsDebug">0</xsl:variable>

  <xsl:variable name="vBorder">
    <xsl:choose>
      <xsl:when test="$vIsDebug=1">0.5pt solid black</xsl:when>
      <xsl:otherwise>0pt solid black</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name ="vLineHeight">0.6</xsl:variable>

  <xsl:variable name ="vNewLine">
    <xsl:text>&#10;</xsl:text>
  </xsl:variable>

  <xsl:variable name ="vCarriageReturn">
    <xsl:text>&#13;</xsl:text>
  </xsl:variable>

  <xsl:variable name ="vBreakLine">
    <fo:block> </fo:block>
  </xsl:variable>

  <xsl:variable name="vNonBreakingSpace">
    <xsl:text>&#xA0;</xsl:text>
  </xsl:variable>

  <xsl:variable name ="vEntity">
    <xsl:text>ENTITY</xsl:text>
  </xsl:variable>

  <xsl:variable name ="vApos">'</xsl:variable>

  <xsl:variable name="varImgLogo">
    <xsl:value-of select="concat('sql(select top(1) IDENTIFIER, LOLOGO from dbo.ACTOR a inner join dbo.ACTORROLE ar on ar.IDA = a.IDA where IDROLEACTOR =', $vApos, $vEntity, $vApos, 'and a.IDENTIFIER !=', $vApos, 'EFSLIC', $vApos,  ')')" />
  </xsl:variable>

  <!-- font ================================================================================= -->
  <!-- sans-serif/Helvetica/Courier/Times New Roman/New York/serif/Gill/-->
  <xsl:variable name="vFontFamily">sans-serif</xsl:variable>
  <xsl:variable name="vFontSizeBody">7.5pt</xsl:variable>
  <xsl:variable name="vFontSizeHeader">9pt</xsl:variable>
  <xsl:variable name="vFontSizeColumnTitles">8pt</xsl:variable>

  <!-- A4 page caracteristics ============================================================== -->
  <xsl:variable name="vPageA4LandscapePageHeight">21</xsl:variable>
  <xsl:variable name="vPageA4LandscapePageWidth">29.7</xsl:variable>
  <xsl:variable name="vPageA4LandscapeMargin">0.5</xsl:variable>

  <!-- Variables for A4 page format ====================================================== -->
  <xsl:variable name="vPageA4LandscapeLeftMargin">0.5</xsl:variable>
  <xsl:variable name="vPageA4LandscapeRightMargin">0.5</xsl:variable>
  <xsl:variable name="vPageA4LandscapeTopMargin">0.1</xsl:variable>
  <xsl:variable name="vPageA4LandscapeBottomMargin">0.1</xsl:variable>

  <!-- Variables for Footer ====================================================== -->
  <xsl:variable name ="vFooterExtent">1.5</xsl:variable>
  <xsl:variable name ="vFooterBorderBottom">0.8pt solid black</xsl:variable>
  <xsl:variable name ="vFooterFontSize">7pt</xsl:variable>

  <!-- Variables for Body ====================================================== -->
  <xsl:variable name ="vBodyLeftMargin">0</xsl:variable>
  <xsl:variable name ="vBodyRightMargin">0</xsl:variable>
  <xsl:variable name ="vBodyTopMargin">
    <xsl:value-of select ="$vHeaderExtent"/>
  </xsl:variable>
  <xsl:variable name ="vBodyBottomMargin">
    <xsl:value-of select ="$vFooterExtent+$vPageA4LandscapeMargin"/>
  </xsl:variable>

  <!-- Variables for Block ====================================================== -->
  <xsl:variable name ="vBlockLeftMargin">0.1</xsl:variable>
  <xsl:variable name ="vBlockRightMargin">1</xsl:variable>
  <xsl:variable name ="vBlockTopMargin">0.2</xsl:variable>
  <xsl:variable name ="vBlockFontSize">8pt</xsl:variable>

  <!-- Variables for Table ====================================================== -->
  <xsl:variable name ="vTableLeftMargin">0</xsl:variable>
  <xsl:variable name="varTableBorder">
    0.1pt solid black
  </xsl:variable>

  <!-- Variables for table-cell ====================================================== -->
  <xsl:variable name ="vTableCellPadding">0.1</xsl:variable>
  <xsl:variable name ="vTableCellPaddingTop">0</xsl:variable>
  <xsl:variable name ="vTableCellMarginLeft">0</xsl:variable>
  <xsl:variable name ="vAmountColumnWidth">2.45</xsl:variable>
  <xsl:variable name ="vLogoHeight">1.35</xsl:variable>
  <xsl:variable name ="vRowHeightBreakline">0.4</xsl:variable>
  <xsl:variable name ="vRowHeight">0.6</xsl:variable>
  <xsl:variable name ="vRowHeightColumnTitles">1.2</xsl:variable>

  <xsl:variable name ="vRowBackgroundColor">
    GhostWhite
    <!--<xsl:choose>
      <xsl:when test="position() mod 2 = 0">white</xsl:when>
      <xsl:otherwise>gray</xsl:otherwise>
    </xsl:choose>-->
  </xsl:variable>

  <!-- Variables for Header ====================================================== -->
  <!--<xsl:variable name ="vHeaderExtent">4.25</xsl:variable>-->
  <xsl:variable name ="vHeaderExtent">
    <xsl:value-of select ="($vLogoHeight) + ($vRowHeight*4) + $vRowHeightBreakline + $vRowHeightColumnTitles + $vPageA4LandscapeTopMargin"/>
  </xsl:variable>
  <xsl:variable name ="vHeaderLeftMargin">1.5</xsl:variable>

  <!-- ================================================================ -->
  <!-- templates ====================================================== -->
  <!-- ================================================================ -->
  <xsl:template name="Referential">
    <fo:layout-master-set>
      <fo:simple-page-master master-name="Landscape" page-height="{$vPageA4LandscapePageHeight}cm" page-width="{$vPageA4LandscapePageWidth}cm" margin="{$vPageA4LandscapeMargin}cm">
        <fo:region-body region-name="PageBody" background-color="white" margin-left="{$vBodyLeftMargin}cm" margin-top="{$vBodyTopMargin}cm" margin-bottom="{$vBodyBottomMargin}cm" />
        <fo:region-before region-name="PageHeader" background-color="white" extent="{$vHeaderExtent}cm"  precedence="true" />
        <fo:region-after region-name="PageFooter" background-color="white" extent="{$vFooterExtent}cm" precedence="true" />
      </fo:simple-page-master>
    </fo:layout-master-set>
  </xsl:template>

  <xsl:template match="/">
    <fo:root xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <xsl:call-template name="Referential"/>
      <fo:page-sequence master-reference="Landscape" initial-page-number="1">

        <fo:static-content flow-name="PageHeader">
          <xsl:call-template name="displayHeader"/>
        </fo:static-content>

        <fo:static-content flow-name="PageFooter">
          <xsl:call-template name="displayFooter"/>
        </fo:static-content>

        <fo:flow flow-name="PageBody">
          <xsl:call-template name="displayBody"/>
          <fo:block id="EndOfDoc" />
        </fo:flow>

      </fo:page-sequence>
    </fo:root>

  </xsl:template>

  <!-- =========================================================  -->
  <!-- Common graphic templates section                           -->
  <!-- =========================================================  -->

  <xsl:template name="displayBreakline">
    <fo:table table-layout="fixed" >
      <xsl:call-template name ="create1Column"/>
      <fo:table-body>
        <fo:table-row height="{$vRowHeightBreakline}cm">
          <fo:table-cell border="{$vBorder}">
            <fo:block border="{$vBorder}"/>
          </fo:table-cell>
        </fo:table-row>
      </fo:table-body>
    </fo:table>
  </xsl:template>

  <xsl:template name ="displayBlankField">
    <fo:block border="{$vBorder}">
      <!-- This field is intentionally left blank -->
    </fo:block>
  </xsl:template>

  <!-- draw horizontal line -->
  <xsl:template name ="drawHorizontalLine">
    <fo:table table-layout="fixed" >
      <xsl:call-template name ="create1Column"/>
      <fo:table-body>        
        <fo:table-row height="{$vRowHeight}cm">
          <fo:table-cell border="{$vBorder}">
            <fo:block border="{$vBorder}" text-align="center" display-align="center">
              <fo:leader leader-length="100%" leader-pattern="rule" rule-style="solid" rule-thickness="1px" color="grey" />
            </fo:block>
          </fo:table-cell>
        </fo:table-row>        
      </fo:table-body>
    </fo:table>
  </xsl:template>

  <xsl:template name="createHeader3Columns">
    <xsl:variable name="vColumnWidth1">7</xsl:variable>
    <xsl:variable name="vColumnWidth2">14</xsl:variable>
    <xsl:variable name="vColumnWidth3">
      <xsl:value-of select =" $vPageA4LandscapePageWidth - $vColumnWidth1 - $vColumnWidth2 - $vPageA4LandscapeLeftMargin - $vPageA4LandscapeRightMargin"/>
    </xsl:variable>
    <fo:table-column column-width="{$vColumnWidth1}cm" column-number="01" />
    <fo:table-column column-width="{$vColumnWidth2}cm" column-number="02" />
    <fo:table-column column-width="{$vColumnWidth3}cm" column-number="03" />
  </xsl:template>

  <xsl:template name="createHeader2Columns">
    <xsl:variable name="vColumnWidth1">4</xsl:variable>
    <xsl:variable name="vColumnWidth2">
      <xsl:value-of select =" $vPageA4LandscapePageWidth - $vColumnWidth1 - $vPageA4LandscapeLeftMargin - $vPageA4LandscapeRightMargin"/>
    </xsl:variable>
    <fo:table-column column-width="{$vColumnWidth1}cm" column-number="01" />
    <fo:table-column column-width="{$vColumnWidth2}cm" column-number="02" />
  </xsl:template>

  <xsl:template name="createBodyColumns">
    <fo:table-column column-width="{2.5}cm" column-number="01"></fo:table-column>
    <fo:table-column column-width="{2.5}cm" column-number="02"></fo:table-column>
    <fo:table-column column-width="{1.65}cm" column-number="03"></fo:table-column>
    <fo:table-column column-width="{$vAmountColumnWidth}cm" column-number="04"></fo:table-column>
    <fo:table-column column-width="{$vAmountColumnWidth}cm" column-number="05"></fo:table-column>
    <fo:table-column column-width="{$vAmountColumnWidth}cm" column-number="06"></fo:table-column>
    <fo:table-column column-width="{$vAmountColumnWidth}cm" column-number="07"></fo:table-column>
    <fo:table-column column-width="{$vAmountColumnWidth}cm" column-number="08"></fo:table-column>
    <fo:table-column column-width="{$vAmountColumnWidth}cm" column-number="09"></fo:table-column>
    <fo:table-column column-width="{$vAmountColumnWidth}cm" column-number="10"></fo:table-column>
    <fo:table-column column-width="{$vAmountColumnWidth}cm" column-number="11"></fo:table-column>
    <fo:table-column column-width="{$vAmountColumnWidth}cm" column-number="12"></fo:table-column>
  </xsl:template>

  <xsl:template name="createBody2Columns">
    <fo:table-column column-width="{4}cm" column-number="01" />
    <fo:table-column column-width="{15}cm" column-number="02" />
  </xsl:template>

  <xsl:template name="create1Column">
    <!-- EG 20160404 Migration vs2013 -->
    <!--<fo:table-column column-width="100%" column-number="01"></fo:table-column>-->
    <fo:table-column column-width="proportional-column-width(100)" column-number="01"/>
  </xsl:template>

  <!-- =========================================================  -->
  <!-- Header section                                             -->
  <!-- =========================================================  -->
  <xsl:template name="displayHeader">
    
    <!-- Header 1° block -->
    <fo:block linefeed-treatment="preserve">
      <fo:table table-layout="fixed">
        <xsl:call-template name ="createHeader3Columns"/>
        <fo:table-body>
          <fo:table-row height="{$vLogoHeight}cm" color="black" font-family="{$vFontFamily}">
            <!-- Logo -->
            <fo:table-cell border="{$vBorder}" text-align="left">
              <fo:block border="{$vBorder}">
                <fo:external-graphic src="{$varImgLogo}" height="{$vLogoHeight}cm" />
              </fo:block>
            </fo:table-cell>

            <fo:table-cell border="{$vBorder}" display-align="after" font-weight="bold" text-align="center" font-size="14pt">
              <fo:block border="{$vBorder}">
                <xsl:text>Situation Financiere Detaillée</xsl:text>
              </fo:block>
            </fo:table-cell>

            <fo:table-cell border="{$vBorder}" display-align="before" text-align="right" font-size="{$vFontSizeHeader}">
              <fo:block border="{$vBorder}">
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'TS-Published'"/>
                </xsl:call-template>
                <xsl:text> </xsl:text>
                <xsl:call-template name ="getTimeStamp"/>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
          <fo:table-row height="{$vRowHeight}cm" font-size="10pt" color="grey" display-align="before" text-align="center" font-weight="bold" font-family="{$vFontFamily}">
            <fo:table-cell border="{$vBorder}" text-align="left">
              <xsl:call-template name="displayBlankField"/>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}">
                <xsl:variable name ="vTableDisplayName">
                  <xsl:call-template name="getTableDisplayName">
                    <xsl:with-param name="pTableName" select="Referential/@TableName"/>
                  </xsl:call-template>
                </xsl:variable>
                <xsl:if test ="substring-after($vTableDisplayName,';') != ''">
                  <xsl:value-of select ="substring-after($vTableDisplayName,';')"/>
                </xsl:if>
                <xsl:if test ="substring-after($vTableDisplayName,';') = ''">
                  <xsl:value-of select ="$vTableDisplayName"/>
                </xsl:if>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}">
                <xsl:call-template name="displayBlankField"/>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </fo:block>

    <!-- Header 2° block -->
    <fo:block linefeed-treatment="preserve">
      
      <fo:table table-layout="fixed" >
        <xsl:call-template name ="createHeader2Columns"/>
        <fo:table-body>
          <fo:table-row height="{$vRowHeight}cm"  text-align="left" font-size="{$vFontSizeHeader}" font-weight="bold" color="black" font-family="{$vFontFamily}">
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}">
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'TS-ClearingDate'"/>
                </xsl:call-template>
                <xsl:text>: </xsl:text>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}">
                <xsl:call-template name ="getClearingDate"/>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
          
          <fo:table-row height="{$vRowHeight}cm"  text-align="left" font-size="{$vFontSizeHeader}" font-weight="bold" color="black" font-family="{$vFontFamily}">
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}">
                <xsl:call-template name="getSpheresTranslation">
                  <xsl:with-param name="pResourceName" select="'lblActorSide'"/>
                </xsl:call-template>
                <xsl:text>: </xsl:text>
              </fo:block>
            </fo:table-cell>
            <fo:table-cell border="{$vBorder}">
              <fo:block border="{$vBorder}">
                <xsl:call-template name ="getConsultationType"/>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
          
        </fo:table-body>
      </fo:table>
    </fo:block>
    
    <!-- Header 3° block (drawHorizontalLine)-->
    <fo:block linefeed-treatment="preserve">
      <xsl:call-template name="drawHorizontalLine"/>
    </fo:block>
    <!-- Header 4° block (breakline)-->
    <fo:block linefeed-treatment="preserve">
      <xsl:call-template name="displayBreakline"/>    
    </fo:block>

    <!-- Header 5° block (Column Titles)-->
    <fo:block linefeed-treatment="preserve">
      <fo:table table-layout="fixed" >
        <xsl:call-template name="createBodyColumns"/>
        <fo:table-body>
          <xsl:call-template name ="displayColumnTitles"/>
        </fo:table-body>
      </fo:table>
    </fo:block>
  </xsl:template>

  <!-- draws the column titles of report-->
  <xsl:template name="displayColumnTitles">
    <fo:table-row height="{$vRowHeightColumnTitles}cm" background-color="#D7D6CB"  display-align="center" font-weight="bold" color="black" font-family="{$vFontFamily}" font-size="{$vFontSizeColumnTitles}" text-align="center">
      <fo:table-cell border="{$varTableBorder}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getColumnDisplayName">
            <xsl:with-param name ="pColumnName" select ="'A_DEALCLEARBRO_DISPLAYNAME'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getColumnDisplayName">
            <xsl:with-param name ="pColumnName" select ="'B_DEALCLEARBRO_IDENTIFIER'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getColumnDisplayName">
            <xsl:with-param name ="pColumnName" select ="'FLOWS_CUR_ALLOC_UNIT'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getColumnDisplayName">
            <xsl:with-param name ="pColumnName" select ="'FLOWS_CUR_ALLOC_VARMARGINAMOUNT'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getColumnDisplayName">
            <xsl:with-param name ="pColumnName" select ="'FLOWS_CUR_ALLOC_PREMIUMAMOUNT'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getColumnDisplayName">
            <xsl:with-param name ="pColumnName" select ="'FLOWS_CUR_ALLOC_CASHSETTLEMENT'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getColumnDisplayName">
            <xsl:with-param name ="pColumnName" select ="'FLOWS_CUR_ALLOC_FEE'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getColumnDisplayName">
            <xsl:with-param name ="pColumnName" select ="'FLOWS_CUR_ALLOC_INITIALMARGINREQ'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getColumnDisplayName">
            <xsl:with-param name ="pColumnName" select ="'FLOWS_CUR_ALLOC_DAILYBALANCE'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getColumnDisplayName">
            <xsl:with-param name ="pColumnName" select ="'FLOWS_CUR_ALLOC_PREVCASHBALANCE'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getColumnDisplayName">
            <xsl:with-param name ="pColumnName" select ="'FLOWS_CUR_ALLOC_CASHTRANSFER'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}">
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getColumnDisplayName">
            <xsl:with-param name ="pColumnName" select ="'FLOWS_CUR_ALLOC_CASHBALANCE'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
    </fo:table-row>
  </xsl:template>

  <!-- =========================================================  -->
  <!-- Footer section                                             -->
  <!-- =========================================================  -->
  <xsl:template name="displayFooter">
    <xsl:call-template name="drawHorizontalLine"/>
    <fo:block >
      <fo:table font-size="8pt" text-align="right" table-layout="fixed" border="{$vBorder}">
        <xsl:call-template name ="create1Column"/>
        <fo:table-body>
          <fo:table-row font-size="8pt" color="gray" font-weight="bold" font-family="{$vFontFamily}">
            <fo:table-cell padding="{$vTableCellPadding}cm" border="{$vBorder}">
              <fo:block border="{$vBorder}">
                <fo:page-number/>/<fo:page-number-citation ref-id="EndOfDoc"/>
              </fo:block>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </fo:block>
  </xsl:template>

  <!-- =========================================================  -->
  <!-- Body section                                               -->
  <!-- =========================================================  -->

  <xsl:template name="displayBody">
    <fo:block >
      <fo:table font-size="{$vFontSizeBody}" table-layout="fixed" >
        <xsl:call-template name="createBodyColumns"/>
        <xsl:for-each select="//Row">

          <!--<xsl:variable name="fratello" select ="preceding-sibling::Row[Column/@ColumnName='A_DEALCLEARBRO_DISPLAYNAME']"/>

          <xsl:variable name ="vActor">
            <xsl:value-of select ="Column[@ColumnName='A_DEALCLEARBRO_DISPLAYNAME']"/>
          </xsl:variable>
          
          <fo:table-body >
            <fo:table-row font-family="{$vFontFamily}" background-color="{$vRowBackgroundColor}" >
              <fo:table-cell padding="{$vTableCellPadding}cm" border="{$varTableBorder}" font-weight="bold" text-align="left">
                <fo:block>
                  vActor: <xsl:value-of select ="$vActor"/>
                  group by: <xsl:value-of select ="Column[@ColumnName='GroupByCount']"/>
                  
                  <xsl:if test ="$fratello != $vActor  and Column[@ColumnName='GroupByCount'] and Column/node()='0'">
                    ok             
                  </xsl:if>               

                </fo:block>
              </fo:table-cell>
            </fo:table-row>
          </fo:table-body>-->

          <xsl:if test ="Column[@ColumnName='GroupByCount'] = '0' ">

            <xsl:variable name ="vCurrentActor">
              <xsl:value-of select ="Column[@ColumnName='A_DEALCLEARBRO_DISPLAYNAME']"/>
            </xsl:variable>

            <xsl:variable name="vPrecedingActor" select ="preceding-sibling::Row[Column/@ColumnName='A_DEALCLEARBRO_DISPLAYNAME']"/>

            <xsl:variable name ="vCustomBackgroundColor">
              <xsl:if test ="$vCurrentActor != $vPrecedingActor">
                <xsl:text>white</xsl:text>
              </xsl:if>
              <xsl:if test ="$vCurrentActor = $vPrecedingActor">
                <xsl:text>white</xsl:text>
              </xsl:if>
            </xsl:variable>

            <fo:table-body border="{$vBorder}">
              <xsl:call-template name ="displayDetailedRow">
                <xsl:with-param name ="pNode" select="current()"/>
                <xsl:with-param name ="pRowBackgroundColor" select="$vCustomBackgroundColor"/>
              </xsl:call-template>
            </fo:table-body>
          </xsl:if>
          <!--<xsl:if test ="Column[@ColumnName='GroupByCount'] = '1' ">
            <fo:table-body keep-together="always">
              <xsl:call-template name ="displayGroupByRow">
                <xsl:with-param name ="pNode" select="current()"/>
              </xsl:call-template>
              <xsl:call-template name="displayBreakline"/>
              <xsl:call-template name="displayBreakline"/>
              <xsl:call-template name="displayBreakline"/>
            </fo:table-body >
          </xsl:if>-->
        </xsl:for-each>
      </fo:table>

    </fo:block>

    <fo:block>
      <fo:table font-size="{$vFontSizeBody}" table-layout="fixed" border="{$vBorder}">
        <xsl:call-template name ="createBodyColumns"/>
        <fo:table-body border="{$vBorder}">
          <xsl:call-template name="displayEmptyRow"/>
          <xsl:call-template name="displayEmptyRow"/>
          <xsl:call-template name="displayEmptyRow"/>
          <xsl:call-template name ="displayGlobalTotal"/>
        </fo:table-body>
      </fo:table>
    </fo:block>


  </xsl:template>


  <!-- Detailed rows - Drows the rows with attribute GroupByCount = 0 -->
  <xsl:template name ="displayDetailedRow">
    <xsl:param name ="pNode"/>
    <xsl:param name ="pRowBackgroundColor"/>
    <fo:table-row font-family="{$vFontFamily}" background-color="{$pRowBackgroundColor}" >
      <fo:table-cell padding="{$vTableCellPadding}cm" border="{$varTableBorder}" font-weight="bold" text-align="left">
        <fo:block border="{$vBorder}">
          <xsl:value-of select="Column[@ColumnName='A_DEALCLEARBRO_DISPLAYNAME']"/>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$vTableCellPadding}cm" border="{$varTableBorder}" text-align="left">
        <fo:block border="{$vBorder}">
          <xsl:value-of select="Column[@ColumnName='B_DEALCLEARBRO_IDENTIFIER']"/>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$vTableCellPadding}cm" border="{$varTableBorder}" text-align="left">
        <fo:block border="{$vBorder}">
          <xsl:value-of select="Column[@ColumnName='FLOWS_CUR_ALLOC_UNIT']"/>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$vTableCellPadding}cm" border="{$varTableBorder}" text-align="right">
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getAmount">
            <xsl:with-param name ="pNode" select="current()/Column[@ColumnName='FLOWS_CUR_ALLOC_VARMARGINAMOUNT']"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$vTableCellPadding}cm" border="{$varTableBorder}" text-align="right">
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getAmount">
            <xsl:with-param name ="pNode" select="current()/Column[@ColumnName='FLOWS_CUR_ALLOC_PREMIUMAMOUNT']"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$vTableCellPadding}cm" border="{$varTableBorder}" text-align="right">
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getAmount">
            <xsl:with-param name ="pNode" select="current()/Column[@ColumnName='FLOWS_CUR_ALLOC_CASHSETTLEMENT']"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$vTableCellPadding}cm" border="{$varTableBorder}" text-align="right">
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getAmount">
            <xsl:with-param name ="pNode" select="current()/Column[@ColumnName='FLOWS_CUR_ALLOC_FEE']"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$vTableCellPadding}cm" border="{$varTableBorder}" text-align="right">
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getAmount">
            <xsl:with-param name ="pNode" select="current()/Column[@ColumnName='FLOWS_CUR_ALLOC_INITIALMARGINREQ']"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$vTableCellPadding}cm" border="{$varTableBorder}" text-align="right">
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getAmount">
            <xsl:with-param name ="pNode" select="current()/Column[@ColumnName='FLOWS_CUR_ALLOC_DAILYBALANCE']"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$vTableCellPadding}cm" border="{$varTableBorder}" text-align="right">
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getAmount">
            <xsl:with-param name ="pNode" select="current()/Column[@ColumnName='FLOWS_CUR_ALLOC_PREVCASHBALANCE']"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$vTableCellPadding}cm" border="{$varTableBorder}" text-align="right">
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getAmount">
            <xsl:with-param name ="pNode" select="current()/Column[@ColumnName='FLOWS_CUR_ALLOC_CASHTRANSFER']"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell padding="{$vTableCellPadding}cm" border="{$varTableBorder}" text-align="right">
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getAmount">
            <xsl:with-param name ="pNode" select="current()/Column[@ColumnName='FLOWS_CUR_ALLOC_CASHBALANCE']"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
    </fo:table-row>
  </xsl:template>

  <!-- Groupby rows - Drows the rows with attribute GroupByCount = 1 -->
  <xsl:template name ="displayGroupByRow">
    <xsl:param name ="pNode"/>
    <fo:table-row font-weight="bold" font-family="{$vFontFamily}">
      <fo:table-cell border="{$varTableBorder}" padding="{$vTableCellPadding}cm" text-align="left">
        <xsl:call-template name="displayBlankField"/>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$vTableCellPadding}cm" text-align="left">
        <xsl:call-template name="displayBlankField"/>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
        <fo:block border="{$vBorder}">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'SUM'"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getAmount">
            <xsl:with-param name ="pNode" select="current()/Column[@ColumnName='FLOWS_CUR_ALLOC_VARMARGINAMOUNT']"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getAmount">
            <xsl:with-param name ="pNode" select="current()/Column[@ColumnName='FLOWS_CUR_ALLOC_PREMIUMAMOUNT']"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getAmount">
            <xsl:with-param name ="pNode" select="current()/Column[@ColumnName='FLOWS_CUR_ALLOC_CASHSETTLEMENT']"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getAmount">
            <xsl:with-param name ="pNode" select="current()/Column[@ColumnName='FLOWS_CUR_ALLOC_FEE']"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getAmount">
            <xsl:with-param name ="pNode" select="current()/Column[@ColumnName='FLOWS_CUR_ALLOC_INITIALMARGINREQ']"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getAmount">
            <xsl:with-param name ="pNode" select="current()/Column[@ColumnName='FLOWS_CUR_ALLOC_DAILYBALANCE']"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getAmount">
            <xsl:with-param name ="pNode" select="current()/Column[@ColumnName='FLOWS_CUR_ALLOC_PREVCASHBALANCE']"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getAmount">
            <xsl:with-param name ="pNode" select="current()/Column[@ColumnName='FLOWS_CUR_ALLOC_CASHTRANSFER']"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
      <fo:table-cell border="{$varTableBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
        <fo:block border="{$vBorder}">
          <xsl:call-template name ="getAmount">
            <xsl:with-param name ="pNode" select="current()/Column[@ColumnName='FLOWS_CUR_ALLOC_CASHBALANCE']"/>
          </xsl:call-template>
        </fo:block>
      </fo:table-cell>
    </fo:table-row>
  </xsl:template>


  <!-- draws a empty rows -->
  <xsl:template name ="displayEmptyRow">

    <fo:table-row font-weight="bold" font-family="{$vFontFamily}">
      <fo:table-cell border="{$vBorder}" padding="{$vTableCellPadding}cm" text-align="left">
        <xsl:call-template name="displayBlankField"/>
      </fo:table-cell>
      <fo:table-cell border="{$vBorder}" padding="{$vTableCellPadding}cm" text-align="left">
        <xsl:call-template name="displayBlankField"/>
      </fo:table-cell>
      <fo:table-cell border="{$vBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
        <xsl:call-template name="displayBlankField"/>
      </fo:table-cell>
      <fo:table-cell border="{$vBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
        <xsl:call-template name="displayBlankField"/>
      </fo:table-cell>
      <fo:table-cell border="{$vBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
        <xsl:call-template name="displayBlankField"/>
      </fo:table-cell>
      <fo:table-cell border="{$vBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
        <xsl:call-template name="displayBlankField"/>
      </fo:table-cell>
      <fo:table-cell border="{$vBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
        <xsl:call-template name="displayBlankField"/>
      </fo:table-cell>
      <fo:table-cell border="{$vBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
        <xsl:call-template name="displayBlankField"/>
      </fo:table-cell>
      <fo:table-cell border="{$vBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
        <xsl:call-template name="displayBlankField"/>
      </fo:table-cell>
      <fo:table-cell border="{$vBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
        <xsl:call-template name="displayBlankField"/>
      </fo:table-cell>
      <fo:table-cell border="{$vBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
        <xsl:call-template name="displayBlankField"/>
      </fo:table-cell>
      <fo:table-cell border="{$vBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
        <xsl:call-template name="displayBlankField"/>
      </fo:table-cell>
    </fo:table-row>

  </xsl:template>


  <!-- draws a row with total amount for each currency-->
  <xsl:template name ="displayGlobalTotal">

    <!-- returns all the currencies availables into the report (ColumnName=FLOWS_CUR_ALLOC_UNIT) -->
    <!-- like sql syntax for select distinct-->
    <xsl:for-each select="//Row/Column[@ColumnName='FLOWS_CUR_ALLOC_UNIT'][not(.=preceding::Column[@ColumnName='FLOWS_CUR_ALLOC_UNIT'])]">

      <xsl:variable name ="vIdc">
        <xsl:value-of select="."/>
      </xsl:variable>

      <fo:table-row font-weight="bold" font-family="{$vFontFamily}">
        <fo:table-cell border="{$vBorder}" padding="{$vTableCellPadding}cm" text-align="left">
          <xsl:call-template name="displayBlankField"/>
        </fo:table-cell>
        <fo:table-cell border="{$vBorder}" padding="{$vTableCellPadding}cm" text-align="left">
          <xsl:call-template name="displayBlankField"/>
        </fo:table-cell>
        <fo:table-cell border="{$varTableBorder}" padding="{$vTableCellPadding}cm" background-color="#D7D6CB" text-align="right" >
          <fo:block border="{$vBorder}">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'SUM'"/>
            </xsl:call-template>
            <xsl:text> </xsl:text>
            <xsl:value-of select ="$vIdc"/>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell border="{$varTableBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
          <fo:block border="{$vBorder}">
            <xsl:call-template name ="getTotalAmount">
              <xsl:with-param name ="pIdc" select ="$vIdc"/>
              <xsl:with-param name ="pColumnName" select ="'FLOWS_CUR_ALLOC_VARMARGINAMOUNT'"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell border="{$varTableBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
          <fo:block border="{$vBorder}">
            <xsl:call-template name ="getTotalAmount">
              <xsl:with-param name ="pIdc" select ="$vIdc"/>
              <xsl:with-param name ="pColumnName" select ="'FLOWS_CUR_ALLOC_PREMIUMAMOUNT'"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell border="{$varTableBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
          <fo:block border="{$vBorder}">
            <xsl:call-template name ="getTotalAmount">
              <xsl:with-param name ="pIdc" select ="$vIdc"/>
              <xsl:with-param name ="pColumnName" select ="'FLOWS_CUR_ALLOC_CASHSETTLEMENT'"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell border="{$varTableBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
          <fo:block border="{$vBorder}">
            <xsl:call-template name ="getTotalAmount">
              <xsl:with-param name ="pIdc" select ="$vIdc"/>
              <xsl:with-param name ="pColumnName" select ="'FLOWS_CUR_ALLOC_FEE'"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell border="{$varTableBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
          <fo:block border="{$vBorder}">
            <xsl:call-template name ="getTotalAmount">
              <xsl:with-param name ="pIdc" select ="$vIdc"/>
              <xsl:with-param name ="pColumnName" select ="'FLOWS_CUR_ALLOC_INITIALMARGINREQ'"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell border="{$varTableBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
          <fo:block border="{$vBorder}">
            <xsl:call-template name ="getTotalAmount">
              <xsl:with-param name ="pIdc" select ="$vIdc"/>
              <xsl:with-param name ="pColumnName" select ="'FLOWS_CUR_ALLOC_DAILYBALANCE'"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell border="{$varTableBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
          <fo:block border="{$vBorder}">
            <xsl:call-template name ="getTotalAmount">
              <xsl:with-param name ="pIdc" select ="$vIdc"/>
              <xsl:with-param name ="pColumnName" select ="'FLOWS_CUR_ALLOC_PREVCASHBALANCE'"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell border="{$varTableBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
          <fo:block border="{$vBorder}">
            <xsl:call-template name ="getTotalAmount">
              <xsl:with-param name ="pIdc" select ="$vIdc"/>
              <xsl:with-param name ="pColumnName" select ="'FLOWS_CUR_ALLOC_CASHTRANSFER'"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
        <fo:table-cell border="{$varTableBorder}" padding="{$vTableCellPadding}cm" text-align="right" >
          <fo:block border="{$vBorder}">
            <xsl:call-template name ="getTotalAmount">
              <xsl:with-param name ="pIdc" select ="$vIdc"/>
              <xsl:with-param name ="pColumnName" select ="'FLOWS_CUR_ALLOC_CASHBALANCE'"/>
            </xsl:call-template>
          </fo:block>
        </fo:table-cell>
      </fo:table-row>

    </xsl:for-each>

  </xsl:template>

  <!-- =========================================================  -->
  <!-- Get section                                                -->
  <!-- contains variables and templates                           -->
  <!-- =========================================================  -->

  <xsl:variable name ="vReferentialTableName">
    <xsl:value-of select ="Referential/@TableName"/>
  </xsl:variable>


  <xsl:template name ="getTableDisplayName">
    <xsl:param name ="pTableName"/>

    <xsl:choose>

      <xsl:when test ="$pTableName = 'FLOWSBYTRADE'">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'OTC_VIEW_FO_FLOW_BYTRADE'"/>
        </xsl:call-template>
      </xsl:when>

      <xsl:when test ="$pTableName = 'FLOWSBYASSET'">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'OTC_VIEW_FO_FLOW_BYASSET'"/>
        </xsl:call-template>
      </xsl:when>

      <xsl:when test ="$pTableName = 'FLOWSBYCURRENCY'">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'OTC_VIEW_FO_FLOW_BYCURRENCY'"/>
        </xsl:call-template>
      </xsl:when>

      <xsl:otherwise>
        <xsl:value-of select ="$pTableName"/>
      </xsl:otherwise>

    </xsl:choose>

  </xsl:template>

  <!-- returns display name of the columns -->
  <xsl:template name ="getColumnDisplayName">
    <xsl:param name ="pColumnName"/>

    <xsl:variable name ="vPrefixTable">
      <xsl:value-of select ="substring($pColumnName,1,2)"/>
    </xsl:variable>

    <xsl:variable name ="vTable">
      <xsl:choose>
        <xsl:when test ="$vPrefixTable='A_'">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'TBL_ACTOR_FLOWSBYCURRENCY'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test ="$vPrefixTable='B_'">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'TBL_BOOK_FLOWSBYCURRENCY'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test ="$vPrefixTable='FL'">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'TBL_CASHFLOWS'"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>Not Available</xsl:text>
        </xsl:otherwise>
      </xsl:choose>

    </xsl:variable>

    <xsl:variable name ="vColumn">
      <xsl:choose>

        <xsl:when test ="$pColumnName='A_DEALCLEARBRO_DISPLAYNAME'">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'TBL_ACTOR_FLOWSBYCURRENCY'"/>
          </xsl:call-template>
        </xsl:when>

        <xsl:when test ="$pColumnName='B_DEALCLEARBRO_IDENTIFIER'">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'TBL_BOOK_FLOWSBYCURRENCY'"/>
          </xsl:call-template>
        </xsl:when>

        <xsl:when test ="$pColumnName='FLOWS_CUR_ALLOC_UNIT'">

          <xsl:variable name ="vTempDev">
            <xsl:call-template name="getSpheresTranslation">
              <xsl:with-param name="pResourceName" select="'COL_Currency'"/>
            </xsl:call-template>
          </xsl:variable>

          <xsl:value-of select ="substring-after($vTempDev,';')"/>

        </xsl:when>

        <xsl:when test ="$pColumnName='FLOWS_CUR_ALLOC_VARMARGINAMOUNT'">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'COL_VARMARGIN1'"/>
          </xsl:call-template>
        </xsl:when>

        <xsl:when test ="$pColumnName='FLOWS_CUR_ALLOC_PREMIUMAMOUNT'">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'COL_PREMIUM2'"/>
          </xsl:call-template>
        </xsl:when>

        <xsl:when test ="$pColumnName='FLOWS_CUR_ALLOC_CASHSETTLEMENT'">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'COL_CASHSETTLEMENT3'"/>
          </xsl:call-template>
        </xsl:when>

        <xsl:when test ="$pColumnName='FLOWS_CUR_ALLOC_FEE'">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'COL_FEE6'"/>
          </xsl:call-template>
        </xsl:when>

        <xsl:when test ="$pColumnName='FLOWS_CUR_ALLOC_INITIALMARGINREQ'">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'COL_INITIALMARGINREQ7'"/>
          </xsl:call-template>
        </xsl:when>

        <xsl:when test ="$pColumnName='FLOWS_CUR_ALLOC_DAILYBALANCE'">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'COL_DAILYBALANCE'"/>
          </xsl:call-template>
        </xsl:when>

        <xsl:when test ="$pColumnName='FLOWS_CUR_ALLOC_PREVCASHBALANCE'">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'COL_PREVCASHBALANCE'"/>
          </xsl:call-template>
        </xsl:when>

        <xsl:when test ="$pColumnName='FLOWS_CUR_ALLOC_CASHTRANSFER'">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'COL_CASHTRANSFER'"/>
          </xsl:call-template>
        </xsl:when>

        <xsl:when test ="$pColumnName='FLOWS_CUR_ALLOC_CASHBALANCE'">
          <xsl:call-template name="getSpheresTranslation">
            <xsl:with-param name="pResourceName" select="'COL_CASHBALANCE'"/>
          </xsl:call-template>
        </xsl:when>

        <xsl:otherwise>
          <xsl:value-of select ="$pColumnName"/>
        </xsl:otherwise>
      </xsl:choose>

    </xsl:variable>

    <xsl:choose>
      <xsl:when test ="substring-before($vColumn,'&lt;br/&gt;') != ''">
        <xsl:value-of select ="substring-before($vColumn,'&lt;br/&gt;')"/>
        <fo:block> </fo:block>
        <xsl:value-of select ="substring-after($vColumn,'&lt;br/&gt;')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$vColumn"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!-- returns formatted number with 2 decimal pattern using current culture-->
  <xsl:template name ="formatMoney">
    <xsl:param name ="pAmount"/>

    <xsl:if test ="$pAmount != ''">
      <xsl:call-template name="format-number">
        <xsl:with-param name="pAmount" select="$pAmount" />
        <xsl:with-param name="pAmountPattern" select="$number2DecPattern" />
      </xsl:call-template>
    </xsl:if>

  </xsl:template>


  <xsl:template name ="getTotalAmount">
    <xsl:param name ="pIdc"/>
    <xsl:param name ="pColumnName"/>

    <xsl:variable name ="vTotalAmount">
      <xsl:value-of select="sum(/Referential/Row[Column/@ColumnName='FLOWS_CUR_ALLOC_UNIT' and Column/node()=$pIdc and Column/@ColumnName='GroupByCount' and Column/node()='1']/Column[@ColumnName=$pColumnName] )"/>
    </xsl:variable>

    <xsl:call-template name ="formatMoney">
      <xsl:with-param name ="pAmount" select ="$vTotalAmount"/>
    </xsl:call-template>
  </xsl:template>


  <!-- return amount value from a specific node-->
  <xsl:template name ="getAmount">
    <xsl:param name ="pNode"/>
    <xsl:call-template name ="formatMoney">
      <xsl:with-param name ="pAmount" select ="$pNode"/>
    </xsl:call-template>
  </xsl:template>

  <!-- returns report date time stamp-->
  <xsl:template name ="getTimeStamp">
    <xsl:variable name ="vDateTimeStamp">
      <xsl:value-of select ="Referential/@timestamp"/>
    </xsl:variable>

    <xsl:call-template name="format-date">
      <xsl:with-param name="xsd-date-time" select="$vDateTimeStamp"/>
    </xsl:call-template>

    <xsl:text> </xsl:text>

    <xsl:call-template name="format-time">
      <xsl:with-param name="xsd-date-time" select="substring-after($vDateTimeStamp,'T')"/>
    </xsl:call-template>

  </xsl:template>


  <xsl:template name ="getClearingDate">

    <xsl:call-template name="format-date">
      <xsl:with-param name="xsd-date-time" select="//Data[@name='DATE1']"/>
    </xsl:call-template>

  </xsl:template>

  <xsl:template name ="getConsultationType">

    <xsl:variable name ="vCustomObject">
      <xsl:value-of select ="//Data[@name='CUSTOMOBJECT0']"/>
    </xsl:variable>
    <xsl:choose>

      <xsl:when test ="$vCustomObject=0">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'All'"/>
        </xsl:call-template>
      </xsl:when>

      <xsl:when test ="$vCustomObject=1">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'ACTOR_TRADINGSIDE'"/>
        </xsl:call-template>
      </xsl:when>

      <xsl:when test ="$vCustomObject=2">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'POSITION_CLEARINGSIDE'"/>
        </xsl:call-template>
      </xsl:when>

      <xsl:when test ="$vCustomObject=3">
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'POSITION_EXECUTINGSIDE'"/>
        </xsl:call-template>
      </xsl:when>

      <xsl:otherwise>
        <xsl:call-template name="getSpheresTranslation">
          <xsl:with-param name="pResourceName" select="'NA'"/>
        </xsl:call-template>
      </xsl:otherwise>

    </xsl:choose>

  </xsl:template>

</xsl:stylesheet>