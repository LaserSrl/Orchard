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
            return (Convert.ToInt64(numb.Replace(".","")) + 10000000).ToString();
            } catch {
            var resultId = "";
            var sourceId = numb;
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
    <xsl:template match="root">
        <xsl:element name="ToRemove" namespace="">
            <xsl:apply-templates select="node()"/>
        </xsl:element>
    </xsl:template>
    <xsl:template match="feedurldetailList">
        <xsl:element name="NewsDetail" namespace="">
            
            <xsl:variable name="count" select="user:GenerateID(.)" />
            <xsl:element name="Id" namespace="">
	     <xsl:value-of select="concat(':lasernumeric',user:GenerateID(polopolyId),'lasernumeric:')"/>
    </xsl:element>
            
            <xsl:element name="Title" namespace="">
		
			  <xsl:value-of select="HttpUtility:HtmlDecode(title)"  />
		
            </xsl:element>
            <xsl:element name="Summary" namespace="">
                <xsl:value-of select="summary"/>
            </xsl:element>
            <xsl:element name="Content" namespace="">
                <xsl:value-of select="content"/>
            </xsl:element>
			<xsl:element name="Category" namespace="">
			<xsl:for-each select="categories">
                <xsl:value-of select="concat(name,', ')"/>
            </xsl:for-each >
			</xsl:element>
            <xsl:element name="UpdatedDate" namespace="">
                <xsl:value-of select="updated"/>
            </xsl:element>
            
            <xsl:element name="Medias" namespace="">
                <xsl:for-each select="media">
                    <xsl:element name="Media" namespace="">
                        <xsl:element name="MediaUrl" namespace="">
                            <xsl:value-of select="substring-before(concat(thumb, '&quot;'), concat(substring-after(thumb, '.jpg'), '&quot;'))  "/>
                        </xsl:element>
                        <xsl:element name="Id" namespace="">
							<xsl:value-of select="concat(':lasernumeric',user:GenerateID(polopolyId),'lasernumeric:')"/>                           
					    </xsl:element>
                    </xsl:element>
					 <xsl:element name="Media" namespace="">
					 </xsl:element>
            </xsl:for-each>
            </xsl:element>
        </xsl:element>
        <xsl:element name="NewsDetail" namespace="">
        </xsl:element>
    </xsl:template>
    
</xsl:stylesheet>