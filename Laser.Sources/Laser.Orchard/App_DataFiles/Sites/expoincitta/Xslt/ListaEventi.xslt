<xsl:stylesheet version="1.0" xmlns:opf="http://www.e015.expo2015.org/schema/events/v1" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"   xmlns:msxsl="urn:schemas-microsoft-com:xslt"   xmlns:user="urn:my-scripts" xmlns:HttpUtility="my:HttpUtility" >
  <xsl:output
    method="xml"
    encoding="UTF-8"
    omit-xml-declaration="yes"
    indent="yes" />
  <xsl:param name="LinguaParameter"/>
  <xsl:param name="datainizio"/>
  <xsl:param name="termids"/>
  <msxsl:script language="C#" implements-prefix="user">
    <![CDATA[     
Int32 numero=0;
        public string correctTime(DateTime dt)
         {
            return dt.ToString("HH:mm:ss");
         }
	    public string correctNumber(string numb)
         {
            return numb.Replace(',','.');
         }	
	
		 public bool StessaData(DateTime t1,DateTime t2)
		 {
		 if (t1==t2)
			return true;
		 else
			return false;
		 }
		 public string GenerateId(string datas){
		 numero=numero+1;
		 return datas.Replace('-','0').ToString()+"0000"+numero.ToString();
		 }
	
public string UnixTicks( string dtstring)
  {
      DateTime d1 = new DateTime(1970, 1, 1);
    DateTime d2 = Convert.ToDateTime(dtstring);//.ToUniversalTime();
	//	 if (d2.Second==59){
		//		d2=d2.AddSeconds(1);
		//	 }
    TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);
    return  ts.TotalMilliseconds.ToString();
  }		 
    ]]>
  </msxsl:script>
  <xsl:template match="opf:Description">
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
  <xsl:template match="/opf:Events/opf:IssueDateTime">
  </xsl:template>
  <xsl:template match="/opf:Events/opf:Name">
  </xsl:template>
  <xsl:template match="/opf:Events/opf:Description">
  </xsl:template>
  <xsl:template match="opf:Event//opf:Name">
    <xsl:call-template name="FiltroLingua"/>
  </xsl:template>
  <xsl:template match="opf:Event/opf:Category">
    <xsl:call-template name="FiltroLingua"/>
  </xsl:template>
  <xsl:template match="opf:Scope">
    <xsl:if test="not(preceding-sibling::opf:Scope)">
      <xsl:text disable-output-escaping="yes"><![CDATA[<Scope>]]></xsl:text>
      <xsl:if test="not(@xml:lang)">
        <xsl:value-of select="."/>
      </xsl:if>
      <xsl:if test="(@xml:lang=$LinguaParameter)">
        <xsl:value-of select="."/>
      </xsl:if>
    </xsl:if>
    <xsl:if test="(preceding-sibling::opf:Scope)">
      <xsl:if test="not(@xml:lang)">
        <xsl:value-of select="concat(',',.)"/>
      </xsl:if>
      <xsl:if test="(@xml:lang=$LinguaParameter)">
        <xsl:value-of select="concat(',',.)"/>
      </xsl:if>
    </xsl:if>
    <xsl:if test="not(following-sibling::opf:Scope)">
      <xsl:text disable-output-escaping="yes"><![CDATA[</Scope>]]></xsl:text>
    </xsl:if>
  </xsl:template>

  <xsl:template match="opf:Event/opf:Contact">
    <xsl:if test="not(preceding-sibling::opf:Contact)">
      <xsl:text disable-output-escaping="yes">	<![CDATA[
		<Contacts>
		]]></xsl:text>
    </xsl:if>
    <xsl:if test="(descendant::*[local-name()='Type']!='phone') and (descendant::*[local-name()='Type']!='email')">
      <xsl:element name="Contact" namespace="">
        <xsl:apply-templates select="node()"/>
      </xsl:element>
    </xsl:if>
    <xsl:if test="not(following-sibling::opf:Contact)">
      <xsl:text disable-output-escaping="yes"><![CDATA[
		<Contact></Contact></Contacts>
		]]></xsl:text>
    </xsl:if>
  </xsl:template>

  <xsl:template match="opf:PriceInfo">
    <xsl:if test="$LinguaParameter='it'  and count(preceding-sibling::opf:PriceInfo)=0" >
      <xsl:element name="{local-name()}" namespace="">
        <xsl:value-of select="node()"/>
      </xsl:element>
    </xsl:if>
    <xsl:if test="$LinguaParameter='en'  and count(preceding-sibling::opf:PriceInfo)=1" >
      <xsl:element name="{local-name()}" namespace="">
        <xsl:value-of select="node()"/>
      </xsl:element>
    </xsl:if>
  </xsl:template>
  <xsl:template match="opf:Event/opf:MoreInfo">
    <xsl:if test="$LinguaParameter='it'  and count(preceding-sibling::opf:MoreInfo)=0" >
      <xsl:element name="{local-name()}" namespace="">
        <xsl:element name="Type" namespace="">
          <xsl:value-of select="@type" />
        </xsl:element>
        <xsl:element name="Uri" namespace="">
          <xsl:value-of select="@uri" />
        </xsl:element>
        <xsl:element name="Info" namespace="">
          <xsl:value-of select="." />
        </xsl:element>
      </xsl:element>
    </xsl:if>
    <xsl:if test="$LinguaParameter='en'  and count(preceding-sibling::opf:MoreInfo)=1" >
      <xsl:element name="{local-name()}" namespace="">
        <xsl:element name="Type" namespace="">
          <xsl:value-of select="@type" />
        </xsl:element>
        <xsl:element name="Uri" namespace="">
          <xsl:value-of select="@uri" />
        </xsl:element>
        <xsl:element name="Info" namespace="">
          <xsl:value-of select="." />
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>
  <xsl:template match="opf:Event/opf:MediaResource">
    <xsl:if test="not(preceding-sibling::opf:MediaResource)">
      <xsl:text disable-output-escaping="yes">	<![CDATA[
		<MediaResources>
		]]></xsl:text>
    </xsl:if>
    <xsl:element name="MediaResource" namespace="">
      <xsl:apply-templates select="node()"/>
    </xsl:element>
    <xsl:if test="not(following-sibling::opf:MediaResource)">
      <xsl:text disable-output-escaping="yes"><![CDATA[
		<MediaResource></MediaResource></MediaResources>
		]]></xsl:text>
    </xsl:if>
  </xsl:template>

  <xsl:template match="opf:Schedule">
    <xsl:element name="Schedule">
      <xsl:if test="count(opf:Occurrence) &gt; 0">
        <xsl:text disable-output-escaping="yes"> <![CDATA[<Occurrences>]]></xsl:text>
        <xsl:for-each select="opf:Occurrence">
          <xsl:sort select="opf:Occurrence"/>

          <xsl:variable name="date" select="substring-before(opf:StartEnd/opf:StartDateTime/text(), 'T')" />
          <xsl:variable name="year" select="substring-before($date, '-')" />
          <xsl:variable name="month" select="substring-before(substring-after($date, '-'), '-')" />
          <xsl:variable name="day" select="substring-after(substring-after($date, '-'), '-')" />

          <xsl:if test="concat($day,'/',$month,'/',$year) = $datainizio">
            <xsl:element name="{local-name()}" namespace="">
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>

        </xsl:for-each>
        <xsl:text disable-output-escaping="yes"><![CDATA[<Occurrence></Occurrence></Occurrences>]]></xsl:text>
      </xsl:if>
      <xsl:if test="count(opf:Recurrence) &gt; 0">
        <xsl:text disable-output-escaping="yes"> <![CDATA[<Recurrences>]]></xsl:text>
        <xsl:for-each select="opf:Recurrence">
          <xsl:sort select="opf:Recurrence"/>
          <xsl:element name="{local-name()}" namespace="">
            <xsl:apply-templates select="node()"/>
          </xsl:element>
        </xsl:for-each>
        <xsl:text disable-output-escaping="yes"><![CDATA[<Recurrence></Recurrence></Recurrences>]]></xsl:text>
      </xsl:if>
    </xsl:element>
  </xsl:template>

  <xsl:template match="opf:Recurrence/opf:RecurOnDayNr">
    <xsl:if test="not(preceding-sibling::opf:RecurOnDayNr)">
      <xsl:text disable-output-escaping="yes">	<![CDATA[<RecurOnDayNr>]]></xsl:text>
      <xsl:value-of select="."/>
    </xsl:if>
    <xsl:if test="(preceding-sibling::opf:RecurOnDayNr)">
      <xsl:value-of select="concat(',',.)"/>
    </xsl:if>
    <xsl:if test="not(following-sibling::opf:RecurOnDayNr)">
      <xsl:text disable-output-escaping="yes"><![CDATA[</RecurOnDayNr>]]></xsl:text>
    </xsl:if>
  </xsl:template>


  <xsl:template match="opf:Event/opf:Tag">
    <xsl:if test="not(preceding-sibling::opf:Tag)">
      <xsl:text disable-output-escaping="yes">	<![CDATA[<Tag>]]></xsl:text>
      <xsl:value-of select="."/>
    </xsl:if>
    <xsl:if test="(preceding-sibling::opf:Tag)">
      <xsl:value-of select="concat(',',.)"/>
    </xsl:if>
    <xsl:if test="not(following-sibling::opf:Tag)">
      <xsl:text disable-output-escaping="yes"><![CDATA[</Tag>]]></xsl:text>
    </xsl:if>
  </xsl:template>



  <xsl:template match="opf:AvailableService">
    <!--
    <xsl:if test="not(preceding-sibling::opf:AvailableService)">
      <xsl:text disable-output-escaping="yes"><![CDATA[<AvailableService>]]></xsl:text>
    </xsl:if>
    <xsl:if test="not(@xml:lang)">
      <xsl:value-of select="concat(HttpUtility:HtmlDecode(.),',')"/>
    </xsl:if>
    <xsl:if test="(@xml:lang=$LinguaParameter)">
      <xsl:value-of select="concat(HttpUtility:HtmlDecode(.),',')"/>
    </xsl:if>
    <xsl:if test="not(following-sibling::opf:AvailableService)">

      <xsl:text disable-output-escaping="yes"><![CDATA[</AvailableService>]]></xsl:text>
    </xsl:if>
    -->
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



  <xsl:template match="opf:Abstract">
    <xsl:if test="not(@xml:lang)">
      <xsl:element name="AbstractText" namespace="">
        <xsl:value-of select="HttpUtility:HtmlDecode(.)"  />
      </xsl:element>
    </xsl:if>
    <xsl:if test="(@xml:lang=$LinguaParameter)">
      <xsl:element name="AbstractText" namespace="">
        <xsl:value-of select="HttpUtility:HtmlDecode(.)"/>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template match="opf:Venue/opf:Category">
    <xsl:call-template name="FiltroLingua"/>
  </xsl:template>

  <xsl:template match="opf:Event/opf:Id">
    <xsl:element name="Id">
      <xsl:value-of select="concat(':lasernumeric',$termids,user:GenerateId($datainizio),'lasernumeric:')"/>
    </xsl:element>
  </xsl:template>
  <xsl:template match="opf:XCoord">
    <xsl:call-template name="Numeric"/>
  </xsl:template>
  <xsl:template match="opf:YCoord">
    <xsl:call-template name="Numeric"/>
  </xsl:template>
  <xsl:template match="opf:SRSCode">
    <xsl:call-template name="Numeric"/>
  </xsl:template>
  <xsl:template match="opf:StartDateTime">
    <xsl:call-template name="TipoDateTime"/>
  </xsl:template>
  <xsl:template match="opf:EndDateTime">
    <xsl:call-template name="TipoDateTime"/>
  </xsl:template>
  <xsl:template match="opf:StartTime">
    <xsl:call-template name="TipoTime"/>
  </xsl:template>
  <xsl:template match="opf:EndTime">
    <xsl:call-template name="TipoTime"/>
  </xsl:template>
  <xsl:template match="opf:Event/opf:Venue">
    <xsl:if test="not(preceding-sibling::opf:Venue)">
      <xsl:text disable-output-escaping="yes">	<![CDATA[<EventVenues>]]></xsl:text>
    </xsl:if>
    <xsl:element name="EventVenue" namespace="">
      <xsl:apply-templates select="node()"/>
    </xsl:element>
    <xsl:if test="not(following-sibling::opf:Venue)">
      <xsl:text disable-output-escaping="yes"><![CDATA[<EventVenue></EventVenue></EventVenues>]]></xsl:text>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Event/EventId">
    <xsl:if test="not(preceding-sibling::*[1][self::EventId])">
      <xsl:text disable-output-escaping="yes"><![CDATA[<EventId>]]></xsl:text>
      <xsl:value-of select="normalize-space(.)"/>
    </xsl:if>
    <xsl:if test="(preceding-sibling::*[1][self::EventId])">
      <xsl:value-of select="concat(',',.)"/>
    </xsl:if>
    <xsl:if test="not(following-sibling::*[1][self::EventId])">
      <xsl:text disable-output-escaping="yes"><![CDATA[</EventId>]]></xsl:text>
    </xsl:if>
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
      <xsl:value-of select="substring(substring-before(., '.'),1,5)" />
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
  <xsl:template match="Venue/Contact">
    <xsl:if test="not(preceding-sibling::*[1][self::Contact])">
      <xsl:text disable-output-escaping="yes">	<![CDATA[
		<Contacts>]]></xsl:text>
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

</xsl:stylesheet>