<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:user="urn:my-scripts" xmlns:HttpUtility="my:HttpUtility" >
<xsl:output 
  method="xml"
  encoding="UTF-8"
  omit-xml-declaration="yes"
  indent="yes"
/> 
<xsl:param name="LinguaParameter"/>
<msxsl:script language="C#" implements-prefix="user">
    <![CDATA[              
		public string GenerateID(string originalID)
         {
            return (Convert.ToInt32(originalID)+1000000).ToString();
         }
    ]]>
</msxsl:script>
	<xsl:template match="Id">
		<xsl:element name="Id">
			<xsl:value-of select="concat(':lasernumeric',user:GenerateID(.),'lasernumeric:')"/>
		</xsl:element>
		<xsl:element name="OriginalId">
			<xsl:value-of select="."/>
		</xsl:element>
	</xsl:template>
	<xsl:template match="Eventi">
		<xsl:element name="ToRemove">
			<xsl:apply-templates select="node()"/>
		</xsl:element>
	</xsl:template>
	<xsl:template match="*">
		<xsl:element name="{local-name()}" namespace="">
			<xsl:apply-templates select="node()"/>
		</xsl:element>
	</xsl:template>
</xsl:stylesheet>