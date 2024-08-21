<?xml version="1.0" encoding="utf-8"?>
<!-- Transformation SQL dans le but de produire un fichier SQL ScriptSystemMsg_Sql2k -->

<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="text" indent="no"/>

  <xsl:param name ="pDbSvrType" select="'dbSQL'"> </xsl:param>

  <xsl:template match="/root">
    <xsl:choose>
      <xsl:when test ="$pDbSvrType='dbORA'">
SPOOL &amp;1
SET ECHO OFF
SET ESCAPE ON
SET VERIFY ON
SET FEEDBACK OFF
SET TERMOUT OFF
SET SERVEROUTPUT ON

begin
  dbms_output.put_line('==============================================================');
  dbms_output.put_line(' File ScriptSystemMsg_Ora9i2.sql                ');
  dbms_output.put_line('==============================================================');
end;
/

begin

/*==============================================================*/
/* Copyright © 2023 EFS							                            */
/*==============================================================*/
/****************************************************************/
/* File ScriptSystemMsg.sql                                     */
/****************************************************************/

      </xsl:when>
      <xsl:when test ="$pDbSvrType='dbSQL'">
/*==============================================================*/
/* Copyright © 2023 EFS							                            */
/*==============================================================*/
/****************************************************************/
/* File ScriptSystemMsg.sql                                     */
/****************************************************************/
      </xsl:when>
    </xsl:choose> 

delete from SYSTEMMSGDET where SYSCODE!='EXT';
delete from SYSTEMMSG where SYSCODE!='EXT';

    <xsl:apply-templates select="data">
      <xsl:with-param name ="pDbSvrType" select ="$pDbSvrType"/>
   </xsl:apply-templates>

     <xsl:choose>
      <xsl:when test ="$pDbSvrType='dbORA'">
end;
/

SET SERVEROUTPUT OFF
SPOOL OFF
EXIT
        </xsl:when>
       </xsl:choose>
  </xsl:template>

  <xsl:template match="data" >
    <xsl:param name ="pDbSvrType"/>
    
    <xsl:variable name ="messageFr">
      <xsl:call-template name="GetMessage">
        <xsl:with-param name="pMessage" select="message/fr-FR"/>
        <xsl:with-param name="pDbSvrType" select ="$pDbSvrType"/>
      </xsl:call-template>
   </xsl:variable>
   
    <xsl:variable name ="messageEn">
      <xsl:call-template name="GetMessage">
        <xsl:with-param name="pMessage" select="message/en-GB"/>
        <xsl:with-param name="pDbSvrType" select ="$pDbSvrType"/>
      </xsl:call-template>
   </xsl:variable>
   
    <xsl:variable name ="messageIt">
      <xsl:call-template name="GetMessage">
        <xsl:with-param name ="pMessage" select="message/it-IT"/>
        <xsl:with-param name="pDbSvrType" select ="$pDbSvrType"/>
      </xsl:call-template>
    </xsl:variable>
   
    <xsl:variable name ="shortMessageFr">
      <xsl:call-template name="GetMessage">
        <xsl:with-param name="pMessage" select="shortMessage/fr-FR"/>
        <xsl:with-param name="pDbSvrType" select ="$pDbSvrType"/>
      </xsl:call-template>
   </xsl:variable>
    
    <xsl:variable name ="shortMessageEn">
      <xsl:call-template name="GetMessage">
        <xsl:with-param name="pMessage" select="shortMessage/en-GB"/>
        <xsl:with-param name="pDbSvrType" select ="$pDbSvrType"/>
      </xsl:call-template>
   </xsl:variable>
    
    <xsl:variable name ="shortMessageIt">
      <xsl:call-template name="GetMessage">
        <xsl:with-param name="pMessage" select="shortMessage/it-IT"/>
        <xsl:with-param name="pDbSvrType" select ="$pDbSvrType"/>
      </xsl:call-template>
   </xsl:variable>

  insert into SYSTEMMSG (SYSCODE,SYSNUMBER,CONTACTCODE,ROWATTRIBUT) values ('<xsl:value-of select ="@syscode"/>',<xsl:value-of select ="@sysnumber"/>,'EFS','S');
<xsl:if test ="string-length($messageFr)>0">
  insert into SYSTEMMSGDET (SYSCODE,SYSNUMBER,CULTURE,ROWATTRIBUT,MESSAGE,SHORTMESSAGE) values ('<xsl:value-of select ="@syscode"/>',<xsl:value-of select ="@sysnumber"/>,'fr','S','<xsl:value-of  select ="$messageFr"/>','<xsl:value-of  select ="$shortMessageFr"/>');
</xsl:if>
<xsl:if test ="string-length($messageEn)>0">
  insert into SYSTEMMSGDET (SYSCODE,SYSNUMBER,CULTURE,ROWATTRIBUT,MESSAGE,SHORTMESSAGE) values ('<xsl:value-of select ="@syscode"/>',<xsl:value-of select ="@sysnumber"/>,'en','S','<xsl:value-of  select ="$messageEn"/>','<xsl:value-of  select ="$shortMessageEn"/>');
</xsl:if>
  <xsl:if test ="string-length($messageIt)>0">
  insert into SYSTEMMSGDET (SYSCODE,SYSNUMBER,CULTURE,ROWATTRIBUT,MESSAGE,SHORTMESSAGE) values ('<xsl:value-of select ="@syscode"/>',<xsl:value-of select ="@sysnumber"/>,'it','S','<xsl:value-of  select ="$messageIt"/>','<xsl:value-of  select ="$shortMessageIt"/>');
</xsl:if>
  </xsl:template>
  
  <!-- ************* -->
  <!-- StringReplace -->
  <!-- ************* -->
  <xsl:template name="Replace">
    <xsl:param name="source"/>
    <xsl:param name="oldValue"/>
    <xsl:param name="newValue"/>
    <xsl:choose>
      <xsl:when test="contains($source,$oldValue)">
        <xsl:value-of select="concat(substring-before($source,$oldValue),$newValue)"/>
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="substring-after($source,$oldValue)"/>
          <xsl:with-param name="oldValue" select="$oldValue"/>
          <xsl:with-param name="newValue" select="$newValue"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$source"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!-- ************* -->
  <!-- GetMessage -->
  <!-- ************* -->
  <xsl:template name ="GetMessage">
    <xsl:param name="pMessage"/>
    <xsl:param name="pDbSvrType"/>

    <xsl:variable name="quote">
      <xsl:text>'</xsl:text>
    </xsl:variable>
    <xsl:variable name="double_quote">
      <xsl:text>''</xsl:text>
    </xsl:variable>

    <!-- ' est remplacé par '' -->
    <xsl:variable name ="vMessage">
      <xsl:call-template name="Replace">
        <xsl:with-param name="source"   select="$pMessage"/>
        <xsl:with-param name="oldValue" select="$quote"/>
        <xsl:with-param name="newValue" select="$double_quote"/>
      </xsl:call-template>
    </xsl:variable>
    
    <!-- Sous Oracle uniquement & est remlplacé par \& pour que le script ScriptSystemMsg_Ora9i2.sql puisse être exécuté via sqlplus -->
    <xsl:variable name ="vMessageOra">
      <xsl:if test ="$pDbSvrType='dbORA'">
        <xsl:call-template name="Replace">
          <xsl:with-param name="source"   select="$vMessage"/>
          <xsl:with-param name="oldValue" select="'&amp;'"/>
          <xsl:with-param name="newValue" select="concat('\','&amp;')"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test ="$pDbSvrType='dbORA'">
        <xsl:value-of select ="$vMessageOra"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select ="$vMessage"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>
