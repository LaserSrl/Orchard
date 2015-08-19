<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"   xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:atom="http://www.w3.org/2005/Atom" xmlns:content="http://purl.org/rss/1.0/modules/content/"  xmlns:user="urn:my-scripts" xmlns:HttpUtility="my:HttpUtility" >
  <xsl:output
    method="xml"
    encoding="UTF-8"
    omit-xml-declaration="yes"
    indent="yes" />
  <xsl:param name="LinguaParameter"/>
  <xsl:param name="startdate"/>
  <msxsl:script language="C#" implements-prefix="user">
    <![CDATA[      
 			public string GenerateID(string numb) {
        try {
          return (Convert.ToInt64(numb.Replace("http://www.montagnebiellesi.com/","")) + 10000000).ToString();
        } catch {
          var resultId = "";
          var sourceId = numb.Replace("http://www.montagnebiellesi.com/","");
          foreach( char c in sourceId) {
              resultId += System.Convert.ToInt32(c).ToString();
          }
          if (resultId.Length<=18){
            resultId = resultId.Substring(0, Math.Min(resultId.Length, 18));
          } else {
            resultId = resultId.Substring(resultId.Length-18, 18);
          }
          return (Convert.ToInt64(resultId) + 20000000).ToString();
        }
      }

    ]]>
  </msxsl:script>
  <!-- 
  Le lingue in questo xslt non sono gestite
  -->
  <xsl:template match="rss">
    <xsl:element name="ToRemove">
      <xsl:apply-templates select="node()"/>
    </xsl:element>
  </xsl:template>
  <xsl:template match="rss/channel">
    <xsl:element name="ToRemove">
      <xsl:apply-templates select="node()"/>
    </xsl:element>
  </xsl:template>
  <xsl:template match="rss/channel/item">
    <xsl:element name="News">
      <xsl:apply-templates select="node()"/>
    </xsl:element>
  </xsl:template>
  <xsl:template match="rss/channel/item/description">
    <xsl:element name="Descriptiontext" namespace="">
      <xsl:value-of select="HttpUtility:HtmlDecode(.)"  />
    </xsl:element>
  </xsl:template>
  <xsl:template match="rss/channel/item/enclosure">
    <xsl:element name="MimeType" namespace="">
      <xsl:value-of select="@type"  />
    </xsl:element>
    <xsl:element name="MediaUrl" namespace="">
      <xsl:value-of select="@url"  />
    </xsl:element>
  </xsl:template>
  <xsl:template match="rss/channel/item/guid">
    <xsl:element name="Id">
      <xsl:value-of select="concat(':lasernumeric',user:GenerateID(.),'lasernumeric:')"/>
    </xsl:element>
    <xsl:element name="Sid">
      <xsl:value-of select="concat('news:',.)"/>
    </xsl:element>
  </xsl:template>
  <xsl:template match="channel/language|channel/title|channel/description|channel/pubDate|channel/generator|channel/link|channel/copyright|channel/atom:link"/>
  <xsl:template match="*">
    <xsl:element name="{local-name()}" namespace="">
      <xsl:apply-templates select="node()"/>
    </xsl:element>
  </xsl:template>

</xsl:stylesheet>