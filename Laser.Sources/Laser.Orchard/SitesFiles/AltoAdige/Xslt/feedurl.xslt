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
public int val_selezionato=0;  
		public void selezionato(int num){
		val_selezionato=num;
		}  
		public bool HoSelezionato(){
		if (val_selezionato>0)
			return false;
		else
			return true;
		} 
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

 <!--     -->
<xsl:template match="root">
<xsl:element name="ToRemove" namespace="">
<xsl:apply-templates select="node()"/>
	</xsl:element>
</xsl:template>
<xsl:template match="root/layout|root/published">
    
</xsl:template>
<xsl:template match="root/layout|root/name">
    
</xsl:template>
<xsl:template match="root/layout|root/metaDescription">
    
</xsl:template>
<xsl:template match="*">
	<xsl:element name="{local-name()}" namespace="">
			<xsl:apply-templates select="node()"/>
	</xsl:element>
</xsl:template>
<xsl:template match="entries">
	<xsl:if test="type[contains(text(),'StandardArticle')] and (category/zone='opening' or category/zone='openings' or category/zone='sport' or category/zone='sport-auto' or category/zone='main' or category/zone='main-auto')">
		<xsl:element name="News" namespace="">
			<xsl:element name="Id" namespace="">
				<xsl:value-of select="concat(':lasernumeric',user:GenerateID(id),'lasernumeric:')"/>
			</xsl:element>
			<xsl:element name="Title" namespace="">
				<xsl:value-of select="title"/>
			</xsl:element>
            
            <xsl:choose>
                <xsl:when test="string-length(summary)>0">
                    <xsl:element name="Summary" namespace="">
                        <xsl:value-of select="summary"/>
                    </xsl:element>
                </xsl:when>
                <xsl:otherwise>
                </xsl:otherwise>
            </xsl:choose>
            
			<xsl:element name="UpdatedDate" namespace="">
				<xsl:value-of select="updated"/>
			</xsl:element>
		
				<xsl:element name="CategoryLabel" namespace="">
					<xsl:value-of select="category/label"/>
				</xsl:element>
				<xsl:element name="CategorySequence" namespace="">
					<xsl:value-of select="concat(':lasernumeric',category/sequence,'lasernumeric:')"/>
				</xsl:element>
				<xsl:element name="CategoryZone" namespace="">
					<xsl:if test="category/zone='opening'">
						<xsl:value-of select="concat(':lasernumeric','1','lasernumeric:')"/>
					</xsl:if>
					<xsl:if test="category/zone='openings'">
						<xsl:value-of select="concat(':lasernumeric','2','lasernumeric:')"/>
					</xsl:if>
					<xsl:if test="category/zone='sport'">
						<xsl:value-of select="concat(':lasernumeric','3','lasernumeric:')"/>
					</xsl:if>
					<xsl:if test="category/zone='sport-auto'">
						<xsl:value-of select="concat(':lasernumeric','4','lasernumeric:')"/>
					</xsl:if>
					<xsl:if test="category/zone='main'">
						<xsl:value-of select="concat(':lasernumeric','5','lasernumeric:')"/>
					</xsl:if>
					<xsl:if test="category/zone='main-auto'">
						<xsl:value-of select="concat(':lasernumeric','6','lasernumeric:')"/>
					</xsl:if>					
				</xsl:element>				
			
			<xsl:for-each select="links">
				<xsl:if test="rel[contains(text(),'detail-alternate')]">
					<xsl:element name="Link" namespace="">
						<xsl:value-of select="href"/>
					</xsl:element>
				</xsl:if>
			</xsl:for-each>
			<xsl:element name="ToRemove" namespace="">
			<xsl:value-of select="user:selezionato(0)"/>
			</xsl:element>
			
		
			
				<xsl:for-each select="links" >
					<xsl:if test="rel[contains(text(),'detail_558')] and user:HoSelezionato()">

						
						<xsl:element name="Mediaurl" namespace="">
							<xsl:value-of select="href"/>
						</xsl:element>
				
				
						<xsl:element name="ToRemove" namespace="">
						<xsl:value-of select="user:selezionato(1)"/>
						</xsl:element>
					</xsl:if>
				</xsl:for-each>	
		
		</xsl:element>
	</xsl:if>
    

</xsl:template>

	

</xsl:stylesheet>