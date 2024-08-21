<?xml version="1.0" encoding="ISO-8859-1"?>
<!-- bcsmessage-map.xsl version2.0 -->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>
	<xsl:template match="/iotask">
		<iotask>
			<xsl:attribute name="id"><xsl:value-of select="@id"/></xsl:attribute>
			<xsl:attribute name="name"><xsl:value-of select="@name"/></xsl:attribute>
			<xsl:attribute name="displayname"><xsl:value-of select="@displayname"/></xsl:attribute>
			<xsl:attribute name="loglevel"><xsl:value-of select="@loglevel"/></xsl:attribute>
			<xsl:attribute name="commitmode"><xsl:value-of select="@commitmode"/></xsl:attribute>
			<xsl:apply-templates select="parameters"/>
			<xsl:apply-templates select="iotaskdet"/>
		</iotask>
	</xsl:template>
	<xsl:template match="parameters">
		<parameters>
			<xsl:for-each select="parameter">
				<parameter>
					<xsl:attribute name="id"><xsl:value-of select="@id"/></xsl:attribute>
					<xsl:attribute name="name"><xsl:value-of select="@name"/></xsl:attribute>
					<xsl:attribute name="displayname"><xsl:value-of select="@displayname"/></xsl:attribute>
					<xsl:attribute name="direction"><xsl:value-of select="@direction"/></xsl:attribute>
					<xsl:attribute name="datatype"><xsl:value-of select="@datatype"/></xsl:attribute>
					<xsl:value-of select="."/>
				</parameter>
			</xsl:for-each>
		</parameters>
	</xsl:template>
	<xsl:template match="iotaskdet">
		<iotaskdet>
			<xsl:attribute name="id"><xsl:value-of select="@id"/></xsl:attribute>
			<xsl:attribute name="loglevel"><xsl:value-of select="@loglevel"/></xsl:attribute>
			<xsl:attribute name="commitmode"><xsl:value-of select="@commitmode"/></xsl:attribute>
			<xsl:apply-templates select="ioinput"/>
		</iotaskdet>
	</xsl:template>
	<xsl:template match="ioinput">
		<ioinput>
			<xsl:attribute name="id"><xsl:value-of select="@id"/></xsl:attribute>
			<xsl:attribute name="name"><xsl:value-of select="@name"/></xsl:attribute>
			<xsl:attribute name="displayname"><xsl:value-of select="@displayname"/></xsl:attribute>
			<xsl:attribute name="loglevel"><xsl:value-of select="@loglevel"/></xsl:attribute>
			<xsl:attribute name="commitmode"><xsl:value-of select="@commitmode"/></xsl:attribute>
			<xsl:apply-templates select="file"/>
		</ioinput>
	</xsl:template>
	<xsl:template match="file">
		<file>
			<!-- Copy the attributes of the node <file> -->
			<xsl:attribute name="name"><xsl:value-of select="@name"/></xsl:attribute>
			<xsl:attribute name="folder"><xsl:value-of select="@folder"/></xsl:attribute>
			<xsl:attribute name="date"><xsl:value-of select="@date"/></xsl:attribute>
			<xsl:attribute name="size"><xsl:value-of select="@size"/></xsl:attribute>
			<xsl:apply-templates select="row"/>
		</file>
	</xsl:template>
	<xsl:template match="row">
		<row>
			<!-- Copy the attributes of the node <row> -->
			<xsl:attribute name="id"><xsl:value-of select="@id"/></xsl:attribute>
			<xsl:attribute name="src"><xsl:value-of select="@src"/></xsl:attribute>
			<xsl:attribute name="status"><xsl:value-of select="@status"/></xsl:attribute>
		    <xsl:call-template name="NotifyTrade"/>
		</row>
	</xsl:template>

	<xsl:template name="NotifyTrade">
		<table name="ERX_TRD_CONF" action="IU">
			<column name="DATE_JOUR" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="substring(data[@name='BizDt'],1,4)"/>
				<xsl:value-of select="substring(data[@name='BizDt'],6,2)"/>
				<xsl:value-of select="substring(data[@name='BizDt'],9,2)"/>
			</column>
			<column name="MEMB_EXCH_ID_COD" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='ID_R01']"/>
			</column>
			<column name="TRN_ID_NO" datakey="true" datakeyupd="false" datatype="string">				
				<xsl:value-of select="substring(data[@name='RptID'],1,6)"/>
			</column>
			<column name="TRN_ID_SFX_NO" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="substring(data[@name='RptID'],7,5)"/>
			</column>
			<column name="ORDR_NO" datakey="true" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='OrdID']"/>
			</column>
			<column name="EXCH_APPL_ID" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='LastMkt']"/>
			</column>
			<column name="CURR_TYP_COD" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='Ccy']"/>
			</column>
			<column name="LNG_QTY" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='LastQty']"/>
			</column>
			<column name="SHT_QTY" datakey="false" datakeyupd="false" datatype="string">
				<xsl:text>0</xsl:text>
			</column>
			<column name="POS_TRN_TYP" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='TrnsfrRsn']"/>
			</column>
			<column name="TRD_MTCH_PRC" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='LastPx']"/>
			</column>
			<column name="TRN_ADJ_STS_COD" datakey="false" datakeyupd="false" datatype="string">
				<xsl:choose>
					<xsl:when test="data[@name='TransTyp']='0' and data[@name='RptTyp']='0'">
						<xsl:text>N</xsl:text>
					</xsl:when>
					<xsl:when test="data[@name='TransTyp']='0' and data[@name='RptTyp']='6'">
						<xsl:text>N</xsl:text>
					</xsl:when>
					<xsl:when test="data[@name='TransTyp']='4' and data[@name='RptTyp']='6'">
						<xsl:text>R</xsl:text>
					</xsl:when>
					<xsl:when test="data[@name='TransTyp']='2' and data[@name='RptTyp']='0'">
						<xsl:text>A</xsl:text>
					</xsl:when>
				</xsl:choose>			
			</column>
			<column name="TRN_HIST_ADJ_IND" datakey="false" datakeyupd="false" datatype="string">
				<xsl:choose>
					<xsl:when test="substring(data[@name='RptID'],12,1)='C'">
						<xsl:text> </xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="substring(data[@name='RptID'],12,1)"/>
					</xsl:otherwise>
				</xsl:choose>			
			</column>
			<column name="TRN_ID_SFX_NO_PNT" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="substring(data[@name='RptRefID'],7,5)"/>
			</column>
			<column name="ACCT_TYP_COD" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="substring(data[@name='ID_R38'],1,1)"/>
			</column>
			<column name="ACCT_TYP_NO" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="substring(data[@name='ID_R38'],2,1)"/>
			</column>
			<column name="GUT_CTPY" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='ID_R95']"/>
				<xsl:value-of select="data[@name='ID_R96']"/> 
			</column>
			<column name="GUT_REF_CUST" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='ID_R95']"/>
				<xsl:value-of select="data[@name='ID_R96']"/> 
			</column>
			<column name="PART_SUB_GRP_COD" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="substring(data[@name='ID_R12'],1,3)"/>
			</column>
			<column name="PART_NO" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="substring(data[@name='ID_R12'],4,3)"/>
			</column>
			<column name="MEMB_CLG_ID_COD" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='ID_R04']"/>
			</column>
			<column name="BUY_COD" datakey="false" datakeyupd="false" datatype="string">
				<xsl:choose>
					<xsl:when test="data[@name='Side']='1'">
						<xsl:text>B</xsl:text>
					</xsl:when>
					<xsl:when test="data[@name='Side']='2'">
						<xsl:text>S</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text>?Side</xsl:text>
					</xsl:otherwise>
				</xsl:choose>				
			</column>
			<column name="OPN_CLS_COD" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='PosEfct']"/>
			</column>
			<column name="TEXT" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='Txt1']"/>
			</column>
			<column name="CUST" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='Txt2']"/>
			</column>
			<column name="ORDR_DAT" datakey="false" datakeyupd="false" datatype="string">				
				<xsl:value-of select="substring(data[@name='OrdDat'],1,4)"/>
				<xsl:value-of select="substring(data[@name='OrdDat'],6,2)"/>
				<xsl:value-of select="substring(data[@name='OrdDat'],9,2)"/>
			</column>
			<column name="ORDR_TYP_COD" datakey="false" datakeyupd="false" datatype="string">
				<xsl:choose>
					<xsl:when test="data[@name='OrdTyp']='1'">
						<xsl:text>M</xsl:text>
					</xsl:when>
					<xsl:when test="data[@name='OrdTyp']='2'">
						<xsl:text>L</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="data[@name='OrdTyp']"/>
					</xsl:otherwise>
				</xsl:choose>			
			</column>
			<column name="ORDR_QTY" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='OrdQty']"/>
			</column>
			<column name="CNTR_CLAS_COD" datakey="false" datakeyupd="false" datatype="string">
				<xsl:choose>
					<xsl:when test="data[@name='PutCall']='0'">
						<xsl:text>P</xsl:text>
					</xsl:when>
					<xsl:when test="data[@name='PutCall']='1'">
						<xsl:text>C</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text></xsl:text>
					</xsl:otherwise>
				</xsl:choose>			
			</column>
			<column name="PROD_LINE" datakey="false" datakeyupd="false" datatype="string">
				<xsl:choose>
					<xsl:when test="string-length(data[@name='PutCall'])=0">
						<xsl:text>F</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text>O</xsl:text>
					</xsl:otherwise>
				</xsl:choose>			
			</column>
			<column name="CNTR_EXER_PRC" datakey="false" datakeyupd="false" datatype="string">
				<xsl:choose>
					<xsl:when test="string-length(data[@name='StrkPx'])=0">
						<xsl:text>0</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="data[@name='StrkPx']"/>
					</xsl:otherwise>
				</xsl:choose>							
			</column>
			<column name="CNTR_EXP_MTH_DAT" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="substring(data[@name='MMY'],5,2)"/>
			</column>
			<column name="CNTR_EXP_YR_DAT" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="substring(data[@name='MMY'],1,4)"/>
			</column>
			<column name="CNTR_VERS_NO" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='OptAt']"/>
			</column>
			<column name="PROD_ID" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="data[@name='Sym']"/>
			</column>
			<column name="TRN_DAT" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="substring(data[@name='TS'],1,4)"/>
				<xsl:value-of select="substring(data[@name='TS'],6,2)"/>
				<xsl:value-of select="substring(data[@name='TS'],9,2)"/>
			</column>
			<column name="TRN_TIM" datakey="false" datakeyupd="false" datatype="string">
				<xsl:value-of select="substring(data[@name='TS'],12,2)"/>
				<xsl:value-of select="substring(data[@name='TS'],15,2)"/>
				<xsl:value-of select="substring(data[@name='TS'],18,2)"/>
				<xsl:value-of select="substring(data[@name='TS'],21,2)"/>			
			</column>
			<column name="EXER_PRC_DCML" datakey="false" datakeyupd="false" datatype="string">
				<xsl:text>0</xsl:text>
			</column>
			<column name="PROD_DISP_DCML" datakey="false" datakeyupd="false" datatype="string">
				<xsl:text>0</xsl:text>
			</column>
		</table>
	</xsl:template>
	
</xsl:stylesheet>
