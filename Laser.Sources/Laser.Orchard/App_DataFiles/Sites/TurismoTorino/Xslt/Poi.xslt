<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"   xmlns:msxsl="urn:schemas-microsoft-com:xslt"   xmlns:user="urn:my-scripts"  xmlns:HttpUtility="my:HttpUtility" >
<xsl:output 
  method="xml"
  encoding="UTF-8"
  omit-xml-declaration="yes"
  indent="yes" /> 
  <xsl:param name="LinguaParameter"/>
<msxsl:script language="C#" implements-prefix="user">
    <![CDATA[              
        public string correctTime(DateTime dt)
         {
            return dt.ToLocalTime().ToString("HH:mm");
         }
	    public string correctNumber(string numb)
         {
            return numb.Replace(',','.');
         }	
		public string GenerateID(string numb)
         {
            return (Convert.ToInt32(numb)+1000000).ToString();
         }	
public string UnixTicks( string dtstring)
  {
      DateTime d1 = new DateTime(1970, 1, 1);
    DateTime d2 = Convert.ToDateTime(dtstring);//.ToUniversalTime();
    TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);
    return  ts.TotalMilliseconds.ToString();
  }		 
    ]]>
</msxsl:script>
<xsl:template match="Description">
	<xsl:if test="not(@xml:lang)">
	    <xsl:element name="Descriptiontext" namespace="">
			<xsl:value-of select="HttpUtility:HtmlDecode(.)"  />
		</xsl:element>
	</xsl:if>
	<xsl:if test="(@xml:lang=$LinguaParameter)">
	    <xsl:element name="Descriptiontext" namespace="">
			<xsl:value-of select="HttpUtility:HtmlDecode(.)"  />
		</xsl:element>
	</xsl:if>
</xsl:template>
<xsl:template match="Venue/Contact">
	<xsl:if test="not(preceding-sibling::*[1][self::Contact])">
 <xsl:text disable-output-escaping="yes">	<![CDATA[
		<Contacts>
		]]></xsl:text>
	</xsl:if>
	<xsl:element name="Contact" namespace="">
			<xsl:apply-templates select="node()"/>
		</xsl:element>
	<xsl:if test="not(following-sibling::*[1][self::Contact])">
	 <xsl:text disable-output-escaping="yes"><![CDATA[
		<Contact></Contact></Contacts>
		]]></xsl:text>
	</xsl:if>
</xsl:template>
<xsl:template match="*">
    <xsl:element name="{local-name()}" namespace="">
		<xsl:apply-templates select="node()"/>
    </xsl:element>
</xsl:template>
<xsl:template match="Venue">
	<xsl:element name="Venue">
		<xsl:apply-templates select="node()"/>	
	</xsl:element>
</xsl:template>
<xsl:template match="/*">
	<xsl:element name="ToRemove" namespace="">
		<xsl:apply-templates select="node()"/>	
	</xsl:element>
</xsl:template>

<xsl:template match="Venue/AvailableService">
	<xsl:if test="not(preceding-sibling::*[1][self::AvailableService])">
		<xsl:text disable-output-escaping="yes"><![CDATA[<AvailableService>]]></xsl:text>
		<xsl:if test="not(@xml:lang)">
			<xsl:value-of select="."/>
		</xsl:if>
		<xsl:if test="(@xml:lang=$LinguaParameter)">
			<xsl:value-of select="."/>
		</xsl:if>
	</xsl:if>
	<xsl:if test="(preceding-sibling::*[1][self::AvailableService])">
		<xsl:if test="not(@xml:lang)">
			<xsl:value-of select="concat(',',.)"/>
		</xsl:if>
		<xsl:if test="(@xml:lang=$LinguaParameter)">
			<xsl:value-of select="concat(',',.)"/>
		</xsl:if>
	</xsl:if>	
	<xsl:if test="not(following-sibling::*[1][self::AvailableService])">
		<xsl:text disable-output-escaping="yes"><![CDATA[</AvailableService>]]></xsl:text>
	</xsl:if>
</xsl:template>
<xsl:template match="OpeningTimes">
	<xsl:element name="OpeningTimes">
		<xsl:if test="count(Occurrence) &gt; 0">
			<xsl:text disable-output-escaping="yes"> <![CDATA[<Occurrences>]]></xsl:text>
			<xsl:for-each select="Occurrence">
				<xsl:sort select="Occurrence"/>
				<xsl:element name="{local-name()}" namespace="">
					<xsl:apply-templates select="node()"/>
				</xsl:element>
			</xsl:for-each>
			<xsl:text disable-output-escaping="yes"><![CDATA[<Occurrence></Occurrence></Occurrences>]]></xsl:text>
		</xsl:if>	
		<xsl:if test="count(Recurrence) &gt; 0">
			<xsl:text disable-output-escaping="yes"> <![CDATA[<Recurrences>]]></xsl:text>
			<xsl:for-each select="Recurrence">
				<xsl:sort select="Recurrence"/>
				<xsl:element name="{local-name()}" namespace="">
					<xsl:apply-templates select="node()"/>
				</xsl:element>
			</xsl:for-each>
			<xsl:text disable-output-escaping="yes"><![CDATA[<Recurrence></Recurrence></Recurrences>]]></xsl:text>
		</xsl:if>
	</xsl:element>
</xsl:template>

<xsl:template match="Abstract">
	<xsl:if test="not(@xml:lang)">
		<xsl:element name="AbstractText" namespace="">
		<xsl:value-of select="HttpUtility:HtmlDecode(.)"  />
		</xsl:element>
	</xsl:if>
	<xsl:if test="(@xml:lang=$LinguaParameter)">
		<xsl:element name="AbstractText" namespace="">
		<xsl:value-of select="HttpUtility:HtmlDecode(.)"  />
		</xsl:element>
	</xsl:if>
</xsl:template>
<xsl:template match="Category">
	<xsl:call-template name="FiltroLingua"/>
</xsl:template>

<xsl:template match="Id">
	<xsl:element name="Id">
		<xsl:value-of select="concat(':lasernumeric',user:GenerateID(.),'lasernumeric:')"/>
	</xsl:element>
</xsl:template>
  <xsl:template match="Break">
    <xsl:element name="BreakTime">
      <xsl:apply-templates select="node()"/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="Venue/MediaResource">
	<xsl:if test="not(preceding-sibling::*[1][self::MediaResource])">
 <xsl:text disable-output-escaping="yes">	<![CDATA[
		<MediaResources>
		]]></xsl:text>
	</xsl:if>
	<xsl:element name="MediaResource" namespace="">
			<xsl:apply-templates select="node()"/>
		</xsl:element>
	<xsl:if test="not(following-sibling::*[1][self::MediaResource])">
	 <xsl:text disable-output-escaping="yes"><![CDATA[
		<MediaResource></MediaResource></MediaResources>
		]]></xsl:text>
	</xsl:if>
</xsl:template>
<xsl:template match="Venue/Tag">
	<xsl:if test="not(preceding-sibling::*[1][self::Tag])">
 <xsl:text disable-output-escaping="yes">	<![CDATA[<Tag>]]></xsl:text>
		<xsl:value-of select="."/>
	</xsl:if>
	<xsl:if test="(preceding-sibling::*[1][self::Tag])">
		<xsl:value-of select="concat(',',.)"/>
	</xsl:if>	
		<xsl:if test="not(following-sibling::*[1][self::Tag])">
	 <xsl:text disable-output-escaping="yes"><![CDATA[</Tag>]]></xsl:text>
	</xsl:if>
</xsl:template>
<xsl:template match="Recurrence/RecurOnDayNr">
	<xsl:if test="not(preceding-sibling::*[1][self::RecurOnDayNr])">
 <xsl:text disable-output-escaping="yes">	<![CDATA[<RecurOnDayNr>]]></xsl:text>
		<xsl:value-of select="."/>
	</xsl:if>
	<xsl:if test="(preceding-sibling::*[1][self::RecurOnDayNr])">
		<xsl:value-of select="concat(',',.)"/>
	</xsl:if>	
		<xsl:if test="not(following-sibling::*[1][self::RecurOnDayNr])">
	 <xsl:text disable-output-escaping="yes"><![CDATA[</RecurOnDayNr>]]></xsl:text>
	</xsl:if>
</xsl:template>
<xsl:template match="Venue/EventId">
	<xsl:if test="not(preceding-sibling::*[1][self::EventId])"><xsl:text disable-output-escaping="yes"><![CDATA[<EventId>]]></xsl:text><xsl:value-of select="normalize-space(.)"/></xsl:if>
	<xsl:if test="(preceding-sibling::*[1][self::EventId])"><xsl:value-of select="concat(',',.)"/></xsl:if>
	<xsl:if test="not(following-sibling::*[1][self::EventId])"><xsl:text disable-output-escaping="yes"><![CDATA[</EventId>]]></xsl:text></xsl:if>

</xsl:template>
<xsl:template match="XCoord">
	<xsl:call-template name="Numeric"/>
</xsl:template>
<xsl:template match="YCoord">
	<xsl:call-template name="Numeric"/>
</xsl:template>
<xsl:template match="StartDateTime">
	<xsl:call-template name="TipoDateTime"/>
</xsl:template>	
<xsl:template match="EndDateTime">
	<xsl:call-template name="TipoDateTime"/>
</xsl:template>	
<xsl:template match="StartTime">
	<xsl:call-template name="TipoTime"/>
</xsl:template>	
<xsl:template match="EndTime">
	<xsl:call-template name="TipoTime"/>
</xsl:template>
<xsl:template name="TipoDateTime">
	<xsl:variable name="date" select="substring-before(., 'T')" />
	<xsl:variable name="year" select="substring-before($date, '-')" />
	<xsl:variable name="month" select="substring-before(substring-after($date, '-'), '-')" />
	<xsl:variable name="day" select="substring-after(substring-after($date, '-'), '-')" />
	<xsl:variable name="timeutc" select="substring-after(., 'T')" />
	<xsl:element name="{local-name()}" namespace="">
		<xsl:value-of select="concat(':laserDate',user:UnixTicks(concat($year,'-',$month,'-',$day,' ',user:correctTime(.))),'laserDate:')" />
	</xsl:element>
	
</xsl:template>
<xsl:template name="TipoTime">
	<xsl:element name="{local-name()}" namespace="">
		<xsl:value-of select="substring-before(., '+')" />
	</xsl:element>
</xsl:template>			
<xsl:template name="FiltroLingua">
	<xsl:if test="not(@xml:lang)">
		<xsl:element name="{local-name()}" namespace="">
			<xsl:apply-templates select="node()"/>
		</xsl:element>
	</xsl:if>
	<xsl:if test="(@xml:lang=$LinguaParameter)">
		<xsl:element name="{local-name()}" namespace="">
			<xsl:apply-templates select="node()"/>
		</xsl:element>
	</xsl:if>
</xsl:template>
<xsl:template name="Numeric2">
	<xsl:element name="{local-name()}" namespace="">
		<xsl:value-of select="concat('',user:correctNumber(.),'')"/>
	</xsl:element>
</xsl:template>
<xsl:template name="Numeric">
	<xsl:element name="{local-name()}" namespace="">
		<xsl:value-of select="concat(':lasernumeric',.,'lasernumeric:')"/>
	</xsl:element>
</xsl:template>
</xsl:stylesheet>