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

      
      <table name="ACTIONREQUEST" action="I" sequenceno="1">
        <mqueue name="NormMsgFactoryMQueue" action="I" />

        <column name="IDARQ" datakey="false" datakeyupd="false" datatype="integer">
          <SpheresLib function="GETNEWID()">
            <Param name="IDGETID" datatype="string">
              <xsl:value-of select="'ACTIONREQUEST'"/>
            </Param>
          </SpheresLib>
          <controls>
            <control action="RejectColumn" return="true" >
              <SpheresLib function="IsUpdate()" />
              <logInfo status="NONE"/>
            </control>
          </controls>
        </column>
        
        <column name="IDENTIFIER" datakey="true" datakeyupd="false" datatype="string">
          <xsl:value-of select="$buildingInfoParameters/parameter[@id='IDENTIFICATION']"/>
        </column>
        <column name="DISPLAYNAME" datakey="false" datakeyupd="false" datatype="string">
          <xsl:value-of select="$buildingInfoParameters/parameter[@id='IDENTIFICATION']/@displayname"/>
        </column>
        <column name="DESCRIPTION" datakey="false" datakeyupd="false" datatype="string">
          <xsl:value-of select="$buildingInfoParameters/parameter[@id='DOCUMENTATION']"/>
        </column>
        <column name="REQUESTTYPE" datakey="false" datakeyupd="false" datatype="string">
          <xsl:value-of select="$buildingInfoParameters/parameter[@id='REQUESTTYPE']"/>
        </column>
        <column name="TIMING" datakey="false" datakeyupd="false" datatype="string">
          <xsl:value-of select="$buildingInfoParameters/parameter[@id='TIMING']"/>
        </column>
        <column name="READYSTATE" datakey="false" datakeyupd="false" datatype="string">
          <xsl:value-of select="'TOCOMPLETE'"/>
        </column>
        <column name="EFFECTIVEDATE" datakey="false" datakeyupd="false" datatype="date" dataformat="yyyy-MM-dd">
          <xsl:value-of select="$buildingInfoParameters/parameter[@id='EFFECTIVEDATE']"/>
        </column>
        
        <column name="MODE_C" datakey="false" datakeyupd="false" datatype="string">
          <xsl:value-of select="$buildingInfoParameters/parameter[@id='MODE_C']" />
        </column>
        <column name="EQTYPRICE_C" datakey="false" datakeyupd="false" datatype="string">
          <xsl:value-of select="$buildingInfoParameters/parameter[@id='PREMIUMSTYLEPRICE_C']" />
        </column>
        <column name="FUTPRICE_C" datakey="false" datakeyupd="false" datatype="string">
          <xsl:value-of select="$buildingInfoParameters/parameter[@id='FUTURESTYLEMTMPRICE_C']" />
        </column>
        <column name="OTHERPRICE_C" datakey="false" datakeyupd="false" datatype="string">
          <xsl:value-of select="$buildingInfoParameters/parameter[@id='OTHERPRICE_C']" />
        </column>
        
        <column name="MODE_O" datakey="false" datakeyupd="false" datatype="string">
          <xsl:value-of select="$buildingInfoParameters/parameter[@id='MODE_O']" />
        </column>
        <column name="EQTYPRICE_O" datakey="false" datakeyupd="false" datatype="string">
          <xsl:value-of select="$buildingInfoParameters/parameter[@id='PREMIUMSTYLEPRICE_O']" />
        </column>
        <column name="FUTPRICE_O" datakey="false" datakeyupd="false" datatype="string">
          <xsl:value-of select="$buildingInfoParameters/parameter[@id='FUTURESTYLEMTMPRICE_O']" />
        </column>
        <column name="OTHERPRICE_O" datakey="false" datakeyupd="false" datatype="string">
          <xsl:value-of select="$buildingInfoParameters/parameter[@id='OTHERPRICE_O']" />
        </column>
        <column name="BUILDINFO" datakey="false" datakeyupd="false" datatype="xml" isColumnMQueue="true" >
          <ValueXML>
            <xsl:copy-of select="data[@name='BUILDINFO']/ValueXML/*"/>
          </ValueXML>
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
