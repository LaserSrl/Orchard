<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:user="urn:my-scripts" xmlns:HttpUtility="my:HttpUtility" >
  <xsl:output
    method="xml"
    encoding="UTF-8"
    omit-xml-declaration="yes"
    indent="yes"
/>
  <xsl:param name="LinguaParameter"/>
  <xsl:param name="seed"/>
  <xsl:param name="luogo"/>

  <msxsl:script language="C#" implements-prefix="user">
    <![CDATA[              
		public string GenerateID(string originalID, string seed)
         {
            var nSeed = 1;
            int.TryParse(seed, out nSeed);
            return (Convert.ToInt64(originalID)+(nSeed*10000000)).ToString();
         }
    ]]>
  </msxsl:script>
  <xsl:template match="/">
    <ToRemove>
      <xsl:if test="$luogo!=''">
        <xsl:for-each select="Eventi/Evento[FkLuogo=$luogo]">
          <Evento>
            <xsl:apply-templates />
          </Evento>
        </xsl:for-each>
      </xsl:if>
      <xsl:if test="$luogo=''">
        <xsl:for-each select="Eventi/Evento">
          <Evento>
            <xsl:apply-templates />
          </Evento>
        </xsl:for-each>
      </xsl:if>
    </ToRemove>
  </xsl:template>
  <xsl:template match="Id">
    <xsl:element name="Id">
      <xsl:value-of select="concat(':lasernumeric',user:GenerateID(., $seed),'lasernumeric:')"/>
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