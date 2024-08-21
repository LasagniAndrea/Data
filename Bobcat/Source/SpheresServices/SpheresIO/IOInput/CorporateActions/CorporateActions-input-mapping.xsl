<?xml version="1.0" encoding="utf-8" ?>
<!-- FI 20200901 [25468] use of GetUTCDateTimeSys -->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1" />
  <xsl:template match="/iotask">
    <iotask>
      <xsl:attribute name="id">
        <xsl:value-of select="@id" />
      </xsl:attribute>
      <xsl:attribute name="name">
        <xsl:value-of select="@name" />
      </xsl:attribute>
      <xsl:attribute name="displayname">
        <xsl:value-of select="@displayname" />
      </xsl:attribute>
      <xsl:attribute name="loglevel">
        <xsl:value-of select="@loglevel" />
      </xsl:attribute>
      <xsl:attribute name="commitmode">
        <xsl:value-of select="@commitmode" />
      </xsl:attribute>
      <xsl:apply-templates select="parameters" />
      <xsl:apply-templates select="iotaskdet" />
    </iotask>
  </xsl:template>
  <xsl:template match="parameters">
    <parameters>
      <xsl:for-each select="parameter">
        <parameter>
          <xsl:attribute name="id">
            <xsl:value-of select="@id" />
          </xsl:attribute>
          <xsl:attribute name="name">
            <xsl:value-of select="@name" />
          </xsl:attribute>
          <xsl:attribute name="displayname">
            <xsl:value-of select="@displayname" />
          </xsl:attribute>
          <xsl:attribute name="direction">
            <xsl:value-of select="@direction" />
          </xsl:attribute>
          <xsl:attribute name="datatype">
            <xsl:value-of select="@datatype" />
          </xsl:attribute>
          <xsl:value-of select="." />
        </parameter>
      </xsl:for-each>
    </parameters>
  </xsl:template>
  <xsl:template match="iotaskdet">
    <iotaskdet>
      <xsl:attribute name="id">
        <xsl:value-of select="@id" />
      </xsl:attribute>
      <xsl:attribute name="loglevel">
        <xsl:value-of select="@loglevel" />
      </xsl:attribute>
      <xsl:attribute name="commitmode">
        <xsl:value-of select="@commitmode" />
      </xsl:attribute>
      <xsl:apply-templates select="ioinput" />
    </iotaskdet>
  </xsl:template>
  <xsl:template match="ioinput">
    <ioinput>
      <xsl:attribute name="id">
        <xsl:value-of select="@id" />
      </xsl:attribute>
      <xsl:attribute name="name">
        <xsl:value-of select="@name" />
      </xsl:attribute>
      <xsl:attribute name="displayname">
        <xsl:value-of select="@displayname" />
      </xsl:attribute>
      <xsl:attribute name="loglevel">
        <xsl:value-of select="@loglevel" />
      </xsl:attribute>
      <xsl:attribute name="commitmode">
        <xsl:value-of select="@commitmode" />
      </xsl:attribute>
      <xsl:apply-templates select="file" />
    </ioinput>
  </xsl:template>
  <xsl:template match="file">
    <file>
      <!-- Copy the attributes of the node <file> -->
      <xsl:attribute name="name">
        <xsl:value-of select="@name" />
      </xsl:attribute>
      <xsl:attribute name="folder">
        <xsl:value-of select="@folder" />
      </xsl:attribute>
      <xsl:attribute name="date">
        <xsl:value-of select="@date" />
      </xsl:attribute>
      <xsl:attribute name="size">
        <xsl:value-of select="@size" />
      </xsl:attribute>
      <!--<xsl:variable name="date_of_parameter_p1" select="/iotask/parameters/parameter[@id='p1']" />-->
      <!-- we take into account only the rows with the same date of the parameter p1 -->
      <!-- <xsl:apply-templates select="row[data[@name='time'] = $date_of_parameter_p1]"/> -->
      <xsl:apply-templates select="row" />
    </file>
  </xsl:template>
  <xsl:template match="row">
    <row>
      <!-- Copy the attributes of the node <row> -->
      <xsl:attribute name="id">
        <xsl:value-of select="@id" />
      </xsl:attribute>
      <xsl:attribute name="src">
        <xsl:value-of select="@src" />
      </xsl:attribute>
      <xsl:attribute name="status">
        <xsl:value-of select="@status" />
      </xsl:attribute>

      <xsl:variable name ="normMsgFactory" select="data[@name='BUILDINFO']/ValueXML/normMsgFactory"/>
      <xsl:variable name ="buildingInfo" select="$normMsgFactory/buildingInfo"/>
      <xsl:variable name ="buildingInfoParameters" select="$buildingInfo/parameters"/>

      <!--<parameters>
        <parameter name="ID">
          <SpheresLib function="GETNEWID()">
            <Param name="IDGETID" datatype="string">
              <xsl:value-of select="'CORPOACTIONISSUE'"/>
            </Param>
          </SpheresLib>
        </parameter>
      </parameters>-->
      
      <table name="CORPOACTIONISSUE" action="IU" sequenceno="1">
        <mqueue name="NormMsgFactoryMQueue" action="IU" />

        <column name="IDCAISSUE" datakey="false" datakeyupd="false" datatype="integer">
          <!--<xsl:text>parameters.ID</xsl:text>-->
          <SpheresLib function="GETNEWID()">
            <Param name="IDGETID" datatype="string">
              <xsl:value-of select="'CORPOACTIONISSUE'"/>
            </Param>
          </SpheresLib>
          <controls>
            <control action="RejectColumn" return="true" >
              <SpheresLib function="IsUpdate()" />
              <logInfo status="NONE"/>
            </control>
          </controls>
        </column>
        <column name="CAMARKET" datakey="true" datakeyupd="false" datatype="string">
          <xsl:value-of select="$buildingInfoParameters/parameter[@id='CAMARKET']"/>
        </column>
        <column name="CFICODE" datakey="true" datakeyupd="false" datatype="string">
          <xsl:value-of select="$buildingInfoParameters/parameter[@id='CFICODE']"/>
        </column>
        <column name="REFNOTICE" datakey="true" datakeyupd="false" datatype="string">
          <xsl:value-of select="$buildingInfoParameters/parameter[@id='REFNOTICE']"/>
        </column>
        <column name="EFFECTIVEDATE" datakey="false" datakeyupd="false" datatype="date" dataformat="yyyy-MM-dd">
          <xsl:value-of select="$buildingInfoParameters/parameter[@id='EFFECTIVEDATE']"/>
        </column>
        <column name="CEGROUP" datakey="false" datakeyupd="false" datatype="string">
          <xsl:value-of select="$buildingInfoParameters/parameter[@id='CEGROUP']"/>
        </column>
        <column name="CETYPE" datakey="false" datakeyupd="false" datatype="string">
          <xsl:value-of select="$buildingInfoParameters/parameter[@id='CETYPE']"/>
        </column>
        <column name="CECOMBINEDTYPE" datakey="false" datakeyupd="false" datatype="string">
          <xsl:value-of select="$buildingInfoParameters/parameter[@id='CECOMBINEDTYPE']"/>
        </column>
        <column name="CECOMBINEDOPER" datakey="false" datakeyupd="false" datatype="string">
          <xsl:value-of select="$buildingInfoParameters/parameter[@id='CECOMBINEDOPER']"/>
        </column>
        <column name="IDENTIFIER" datakey="false" datakeyupd="false" datatype="string">
          <xsl:value-of select="$buildingInfoParameters/parameter[@id='REFNOTICE']/@name" />
        </column>
        <column name="NOTICEFILENAME" datakey="false" datakeyupd="false" datatype="string">
          <xsl:value-of select="$buildingInfoParameters/parameter[@id='REFNOTICE']/@exvalue" />
        </column>
        <column name="PUBDATE" datakey="false" datakeyupd="false" datatype="datetime" dataformat="yyyy-MM-dd">
          <xsl:value-of select="$buildingInfoParameters/parameter[@id='PUBDATE']"/>
        </column>
        <column name="ADJMETHOD" datakey="false" datakeyupd="false" datatype="string">
          <xsl:value-of select="$buildingInfoParameters/parameter[@id='ADJMETHOD']"/>
        </column>
        <column name="BUILDINFO" datakey="false" datakeyupd="false" datatype="xml" isColumnMQueue="true" >
          <ValueXML>
            <xsl:copy-of select="data[@name='BUILDINFO']/ValueXML/*"/>
          </ValueXML>
        </column>
        <column name="EMBEDDEDSTATE" datakey="false" datakeyupd="false" datatype="string">
          <xsl:value-of select="'REQUESTED'" />
        </column>
        <column name="READYSTATE" datakey="false" datakeyupd="false" datatype="string">
          <xsl:value-of select="$buildingInfoParameters/parameter[@id='READYSTATE']"/>
        </column>
        <column name="DTINS" datakey="false" datakeyupd="false" datatype="datetime">
          <SpheresLib function="GetUTCDateTimeSys()" />
          <controls>
            <control action="RejectColumn" return="true" >
              <SpheresLib function="IsUpdate()" />
              <logInfo status="NONE"/>
            </control>
          </controls>
        </column>
        <column name="IDAINS" datakey="false" datakeyupd="false" datatype="integer">
          <SpheresLib function="GetUserId()" />
          <controls>
            <control action="RejectColumn" return="true" >
              <SpheresLib function="IsUpdate()" />
              <logInfo status="NONE"/>
            </control>
          </controls>
        </column>
        <column name="DTUPD" datakey="false" datakeyupd="false" datatype="datetime">
          <SpheresLib function="GetUTCDateTimeSys()" />
          <controls>
            <control action="RejectColumn" return="true" >
              <SpheresLib function="IsInsert()" />
              <logInfo status="NONE"/>
            </control>
          </controls>
        </column>
        <column name="IDAUPD" datakey="false" datakeyupd="false" datatype="integer">
          <SpheresLib function="GetUserId()" />
          <controls>
            <control action="RejectColumn" return="true" >
              <SpheresLib function="IsInsert()" />
              <logInfo status="NONE"/>
            </control>
          </controls>
        </column>
        <column name="EXTLLINK" datakey="false" datakeyupd="false" datatype="string">null</column>
        <column name="ROWATTRIBUT" datakey="false" datakeyupd="false" datatype="string">S</column>
      </table>
    </row>
  </xsl:template>
</xsl:stylesheet>
