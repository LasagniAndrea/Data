<?xml version="1.0" encoding="utf-8"?>
<!--
===================================================================================================================================
Summary : CCeG - RISKDATA [Voir Ticket TRIM 18674]
          Intégration des cours de settlementde tout les sous-jacents (Equity et Index) contenue dans le Fichier ExpInf.txt
          (ExpInf.txt: Underlying Prices, calculated by the Exchanges, and used by CC&G in the Expiring process)
          
          des options sur indice sur le CCeG
File    : RiskData_CCeG_UnderlyingExpiryPrices_Map.xsl
===================================================================================================================================
Version : v12.0.8544                                           
Date    : 20230524
Author  : MP/FL
Comment : [26281] - Official settlement on Equity STLAM since 02/03/2023 (NL00150001Q9) mancante

          Modification de la requête de recherche de l'identifiant de l'equity sur lequel porte la cotation,
		  car depuis le 02/03/2023 dans le fichier des cotations des Equities de la CCeG (fichier: EXPINF.TXT),
		  le symbole de l'equity ne correspond pas forcément au symbole du DC.
		  
          Exemple: L'equity STELLANTIS-NL00150001Q9 ayant pour symbole "STLAM" est sous-jacent d'un DC ayant pour symbole "STLA".
==================================================================================================================================     
Version : v3.5.0.0                                           
Date    : 20130705
Author  : BD/FL
Comment : File RiskData_CCeG_UnderlyingIndexPrices_Map.xsl renamed to RiskData_CCeG_UnderlyingExpiryPrices_Map.xsl

          Dorénavant, l'élément suivant CCeG - RISKDATA - UNDERLYINGEXPIRYPRICES (CCeG - RISKDATA - UNDERLYINGINDEXPRICES) 
          qui initialement n'importait que les cours des sous-jacents index,  importe désormais les cours de tout les sous-jacents
          (Equity et Index) contenue dans le Fichier (ExpInf.txt: Underlying Prices, calculated by the Exchanges, and used by CC&G
          in the Expiring process)

          Ces cours sont intégrés avec le type cotations : OfficialSettlement
          
          Pour rappel ce fichier contient des informations aux échéances suivantes : 
          
	         - Le 1er, 2ème, 4ème et 5ème  Vendredi du mois, ce fichier contient le cours du sous jacent 
              de l'index (FTSE MIB Index) nécessaire à l'expiration des index weeklyoptions expiry.
              
	         - Le jeudi qui précède le 3ème Vendredi du mois, ce fichier contient le cours des sous jacent 
              des equities nécessaire à l'expiration des options sur equities qui a lieu 3ème Vendredi du mois.
              
	        - Le 3ème Vendredi du mois, ce fichier contient le cours du sous jacent de l'index (FTSE MIB Index)
          nécessaire à l'expiration des options sur index. 
          
          Pour plus d'info ce référé au ticket  N° 18800
===================================================================================================================================
Version : v3.4.0.0                                           
Date    : 20130521
Author  : BD
Comment : Création
===================================================================================================================================
-->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
	<xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1" />

	<xsl:include href="Quote_Common_SQL.xsl"/>

	<!-- iotask -->
	<xsl:template match="iotask">
		<iotask>
			<xsl:attribute name="id">
				<xsl:value-of select="@id"/>
			</xsl:attribute>
			<xsl:attribute name="name">
				<xsl:value-of select="@name"/>
			</xsl:attribute>
			<xsl:attribute name="displayname">
				<xsl:value-of select="@displayname"/>
			</xsl:attribute>
			<xsl:attribute name="loglevel">
				<xsl:value-of select="@loglevel"/>
			</xsl:attribute>
			<xsl:attribute name="commitmode">
				<xsl:value-of select="@commitmode"/>
			</xsl:attribute>
			<xsl:apply-templates select="parameters"/>
			<xsl:apply-templates select="iotaskdet"/>
		</iotask>
	</xsl:template>

	<!-- parameters -->
	<xsl:template match="parameters">
		<parameters>
			<xsl:for-each select="parameter">
				<parameter>
					<xsl:attribute name="id">
						<xsl:value-of select="@id"/>
					</xsl:attribute>
					<xsl:attribute name="name">
						<xsl:value-of select="@name"/>
					</xsl:attribute>
					<xsl:attribute name="displayname">
						<xsl:value-of select="@displayname"/>
					</xsl:attribute>
					<xsl:attribute name="direction">
						<xsl:value-of select="@direction"/>
					</xsl:attribute>
					<xsl:attribute name="datatype">
						<xsl:value-of select="@datatype"/>
					</xsl:attribute>
					<xsl:value-of select="."/>
				</parameter>
			</xsl:for-each>
		</parameters>
	</xsl:template>

	<!-- iotaskdet -->
	<xsl:template match="iotaskdet">
		<iotaskdet>
			<xsl:attribute name="id">
				<xsl:value-of select="@id"/>
			</xsl:attribute>
			<xsl:attribute name="loglevel">
				<xsl:value-of select="@loglevel"/>
			</xsl:attribute>
			<xsl:attribute name="commitmode">
				<xsl:value-of select="@commitmode"/>
			</xsl:attribute>
			<xsl:apply-templates select="ioinput"/>
		</iotaskdet>
	</xsl:template>

	<!-- ioinput -->
	<xsl:template match="ioinput">
		<ioinput>
			<xsl:attribute name="id">
				<xsl:value-of select="@id"/>
			</xsl:attribute>
			<xsl:attribute name="name">
				<xsl:value-of select="@name"/>
			</xsl:attribute>
			<xsl:attribute name="displayname">
				<xsl:value-of select="@displayname"/>
			</xsl:attribute>
			<xsl:attribute name="loglevel">
				<xsl:value-of select="@loglevel"/>
			</xsl:attribute>
			<xsl:attribute name="commitmode">
				<xsl:value-of select="@commitmode"/>
			</xsl:attribute>
			<xsl:apply-templates select="file"/>
		</ioinput>
	</xsl:template>

	<!-- file -->
	<xsl:template match="file">
		<file>
			<xsl:attribute name="name">
				<xsl:value-of select="@name"/>
			</xsl:attribute>
			<xsl:attribute name="folder">
				<xsl:value-of select="@folder"/>
			</xsl:attribute>
			<xsl:attribute name="date">
				<xsl:value-of select="@date"/>
			</xsl:attribute>
			<xsl:attribute name="size">
				<xsl:value-of select="@size"/>
			</xsl:attribute>

			<xsl:apply-templates select="row" />
		</file>
	</xsl:template>

	<!-- row -->
	<xsl:template match="row">
		<row>
			<xsl:attribute name="id">
				<xsl:value-of select="@id"/>
			</xsl:attribute>
			<xsl:attribute name="src">
				<xsl:value-of select="@src"/>
			</xsl:attribute>

			<!-- VARIABLES -->
			<xsl:variable name="vDate">
				<xsl:value-of select="data[@name='Date']"/>
			</xsl:variable>
			<xsl:variable name="vContractSymbol">
				<xsl:value-of select="normalize-space(data[@name='Symbol'])"/>
			</xsl:variable>
			<xsl:variable name="vIsin">
				<xsl:value-of select="data[@name='Isin']"/>
			</xsl:variable>
			<xsl:variable name="vPrice">
				<xsl:value-of select="data[@name='Price']"/>
			</xsl:variable>
			<xsl:variable name="vCurrency">
				<xsl:value-of select="data[@name='Currency']"/>
			</xsl:variable>
			<xsl:variable name="vCategory">
				<xsl:value-of select="'O'"/>
			</xsl:variable>
			<xsl:variable name="vISO10383">
				<xsl:value-of select="'XDMI'"/>
			</xsl:variable>

			<!-- En fonction de la Phase -->
			<xsl:choose>

				<!-- Phase 1 ou 2 : QUOTE_EQUITY_H -->
				<xsl:when test="data[@name='Phase'] = '1' or data[@name='Phase'] = '2'">
					<!-- Select IDASSET -->
					<parameters>
						<parameter name="IdAssetIndex">
							<SQL command="select" result="IDASSET" cache="true">
								<!-- MP/FL 20230524 [26281]  -->
								<!-- select a.IDASSET from dbo.ASSET_EQUITY a
                                     inner join dbo.DERIVATIVECONTRACT dc on dc.IDASSET_UNL=a.IDASSET and dc.CONTRACTSYMBOL=@CONTRACTSYMBOL and dc.CATEGORY=@CATEGORY and dc.ASSETCATEGORY='EquityAsset'
                                     inner join dbo.MARKET m on m.IDM=dc.IDM and m.ISO10383_ALPHA4=@ISO10383
                                     where a.ISINCODE=@ISINCODE -->
								select aerd.IDASSET from dbo.ASSET_EQUITY_RDCMK aerd
								inner join dbo.ASSET_EQUITY a on a.IDASSET = aerd.IDASSET
								inner join dbo.MARKET m on m.IDM = aerd.IDM_RELATED and m.ISO10383_ALPHA4=@ISO10383
								where  a.ISINCODE=@ISINCODE and aerd.SYMBOL = @CONTRACTSYMBOL
								<xsl:call-template name="SQLDTENABLEDDTDISABLED">
									<xsl:with-param name="pTable" select="'a'"/>
								</xsl:call-template>
								<Param name="DT" datatype="date">
									<xsl:value-of select="$gParamDtBusiness"/>
								</Param>
								<Param name="CONTRACTSYMBOL" datatype="string">
									<xsl:value-of select="$vContractSymbol"/>
								</Param>
								<Param name="ISINCODE" datatype="string">
									<xsl:value-of select="data[@name='Isin']"/>
								</Param>
								<Param name="ISO10383" datatype="string">
									<xsl:value-of select="$vISO10383"/>
								</Param>
								<Param name="CATEGORY" datatype="string">
									<xsl:value-of select="$vCategory"/>
								</Param>
							</SQL>
						</parameter>
					</parameters>
					<!-- Insert/Update QUOTE_INDEX_H -->
					<xsl:call-template name="QUOTE_H_IU">
						<xsl:with-param name="pTableName" select="'QUOTE_EQUITY_H'"/>
						<xsl:with-param name="pISO10383" select="$vISO10383"/>
						<xsl:with-param name="pContractSymbol" select="$vContractSymbol"/>
						<xsl:with-param name="pIdAsset" select="'parameters.IdAssetIndex'"/>
						<xsl:with-param name="pBusinessDate" select="$vDate"/>
						<xsl:with-param name="pCategory" select="$vCategory"/>
						<xsl:with-param name="pCurrency" select="$vCurrency"/>
						<xsl:with-param name="pValue" select="$vPrice"/>
						<xsl:with-param name="pQuoteSide" select="'OfficialSettlement'"/>
					</xsl:call-template>
				</xsl:when>

				<!-- Phase 3 ou 4 : QUOTE_INDEX_H -->
				<xsl:when test="data[@name='Phase'] = '3' or data[@name='Phase'] = '4'">
					<!-- Select IDASSET -->
					<parameters>
						<parameter name="IdAssetIndex">
							<SQL command="select" result="IDASSET" cache="true">
								select a.IDASSET from dbo.ASSET_INDEX a
								inner join dbo.DERIVATIVECONTRACT dc on dc.IDASSET_UNL=a.IDASSET and dc.CONTRACTSYMBOL=@CONTRACTSYMBOL and dc.CATEGORY=@CATEGORY and dc.ASSETCATEGORY='Index'
								inner join dbo.MARKET m on m.IDM=dc.IDM and m.ISO10383_ALPHA4=@ISO10383
								where a.ISINCODE=@ISINCODE
								<xsl:call-template name="SQLDTENABLEDDTDISABLED">
									<xsl:with-param name="pTable" select="'dc'"/>
								</xsl:call-template>
								<Param name="DT" datatype="date">
									<xsl:value-of select="$gParamDtBusiness"/>
								</Param>
								<Param name="CONTRACTSYMBOL" datatype="string">
									<xsl:value-of select="$vContractSymbol"/>
								</Param>
								<Param name="ISINCODE" datatype="string">
									<xsl:value-of select="data[@name='Isin']"/>
								</Param>
								<Param name="ISO10383" datatype="string">
									<xsl:value-of select="$vISO10383"/>
								</Param>
								<Param name="CATEGORY" datatype="string">
									<xsl:value-of select="$vCategory"/>
								</Param>
							</SQL>
						</parameter>
					</parameters>

					<!-- Insert/Update QUOTE_INDEX_H -->
					<xsl:call-template name="QUOTE_H_IU">
						<xsl:with-param name="pTableName" select="'QUOTE_INDEX_H'"/>
						<xsl:with-param name="pISO10383" select="$vISO10383"/>
						<xsl:with-param name="pContractSymbol" select="$vContractSymbol"/>
						<xsl:with-param name="pIdAsset" select="'parameters.IdAssetIndex'"/>
						<xsl:with-param name="pBusinessDate" select="$vDate"/>
						<xsl:with-param name="pCategory" select="$vCategory"/>
						<xsl:with-param name="pCurrency" select="$vCurrency"/>
						<xsl:with-param name="pValue" select="$vPrice"/>
						<xsl:with-param name="pQuoteSide" select="'OfficialSettlement'"/>
					</xsl:call-template>
				</xsl:when>

			</xsl:choose>

		</row>
	</xsl:template>

</xsl:stylesheet>