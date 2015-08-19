<xsl:stylesheet version="1.0" xmlns:opf="http://www.e015.expo2015.org/schema/events/v1" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"   xmlns:msxsl="urn:schemas-microsoft-com:xslt"   xmlns:user="urn:my-scripts" xmlns:HttpUtility="my:HttpUtility" >
  <xsl:output
    method="xml"
    encoding="UTF-8"
    omit-xml-declaration="yes"
    indent="yes" />
  <xsl:param name="LinguaParameter"/>
  <msxsl:script language="C#" implements-prefix="user">
    <![CDATA[     
     public string GenerateID(string numb) {
        return (Convert.ToInt64(numb) + 990000000).ToString();
     }
    ]]>
  </msxsl:script>
  <xsl:template match="Category">
    <xsl:element name="ExternalCategory" namespace="">
      <xsl:apply-templates />
    </xsl:element>
  </xsl:template>

  <xsl:template match="ID">
    <xsl:element name="Id" namespace="">
      <xsl:value-of select="concat(':lasernumeric',user:GenerateID(.),'lasernumeric:')"/>
    </xsl:element>
    <xsl:element name="OriginalId" namespace="">
      <xsl:value-of select="concat(':lasernumeric',.,'lasernumeric:')"/>
    </xsl:element>
  </xsl:template>
  <xsl:template match="CategoryEn">
    <xsl:if test="$LinguaParameter='it'">
    </xsl:if>
    <xsl:if test="$LinguaParameter='en'">
      <xsl:element name="DescriptionText" namespace="">
        <xsl:value-of select="HttpUtility:HtmlDecode(.)"  />
      </xsl:element>
    </xsl:if>
  </xsl:template>
  <xsl:template match="CategoryIt">
    <xsl:if test="$LinguaParameter='en'">
    </xsl:if>
    <xsl:if test="$LinguaParameter='it'">
      <xsl:element name="DescriptionText" namespace="">
        <xsl:value-of select="HttpUtility:HtmlDecode(.)"  />
      </xsl:element>
    </xsl:if>
  </xsl:template>
  <xsl:template match="*">
    <xsl:element name="{local-name()}" namespace="">
      <xsl:apply-templates select="node()"/>
    </xsl:element>
  </xsl:template>
  <xsl:template match="/*">
    <xsl:element name="ToRemove" namespace="">
      <xsl:apply-templates select="node()"/>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>