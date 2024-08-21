<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:doc="http://www.fpml.org/coding-scheme/documentation"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
    xmlns:gcl="http://xml.genericode.org/2004/ns/CodeList/0.2/">

  <xsl:output method="text"/>

  <xsl:template match="gcl:CodeList">

    <!-- Déclaration de l'enum -->
    <xsl:call-template name ="ToCSharpAnnotation">
      <xsl:with-param name="pAnnotation" select ="Annotation"/>
      <xsl:with-param name="pIdentification" select ="Identification/ShortName"/>
    </xsl:call-template>

    <!-- liste des valeurs de l'enum-->
    <xsl:for-each select="SimpleCodeList/Row">
      <xsl:call-template name ="ToCSharpRow">
        <xsl:with-param name="pRow" select ="."/>
      </xsl:call-template>
    </xsl:for-each>

    <!-- FIN -->
    <xsl:text>}</xsl:text>

  </xsl:template>

  <xsl:template name ="ToCSharpAnnotation">
    <xsl:param name ="pAnnotation" />
    <xsl:param name ="pIdentification" />

    <!-- Ajout summary lié à la déclaration de l'enum  -->
    <xsl:call-template name ="AddSummary">
      <xsl:with-param name="pData" select="$pAnnotation/Description/doc:definition"/>
    </xsl:call-template>
    <!-- Ajout 
     public enum (Scheme)  
     {  
    -->
    <xsl:text>&#10;</xsl:text>
    <xsl:text>public enum </xsl:text>
    <xsl:value-of select ="$pIdentification"/>
    <xsl:text>&#10;{&#10;</xsl:text>
  </xsl:template>

  <xsl:template name ="ToCSharpRow">
    <xsl:param name ="pRow" />
    <!-- Ajout summary lié à la valeur  -->
    <xsl:call-template name ="AddSummary">
      <xsl:with-param name="pData" select="$pRow/Value[3]/SimpleValue"/>
    </xsl:call-template>
    <!-- Ajout de la valeur  -->
    <xsl:text>&#10;</xsl:text>
    <xsl:value-of select ="$pRow/Value[1]/SimpleValue"/>
    <xsl:text>,&#10;</xsl:text>
  </xsl:template>

  <!-- Retourne un summary c# -->
  <xsl:template name ="AddSummary">
    <xsl:param name ="pData" />

    <xsl:text>/// &lt;summary &gt;</xsl:text>
    <xsl:text>&#10;</xsl:text>
    <xsl:text>/// </xsl:text>
    <xsl:value-of select ="$pData"/>
    <xsl:text>&#10;</xsl:text>
    <xsl:text>/// &lt;/summary&gt;</xsl:text>
  </xsl:template>

</xsl:stylesheet>
