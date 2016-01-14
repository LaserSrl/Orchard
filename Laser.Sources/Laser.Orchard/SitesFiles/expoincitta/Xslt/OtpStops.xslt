<xsl:stylesheet version="1.0" xmlns:opf="http://www.e015.expo2015.org/schema/events/v1" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"   xmlns:msxsl="urn:schemas-microsoft-com:xslt"   xmlns:user="urn:my-scripts" xmlns:HttpUtility="my:HttpUtility" >
  <xsl:output
    method="xml"
    encoding="UTF-8"
    omit-xml-declaration="yes"
    indent="yes" />
  <xsl:param name="LinguaParameter"/>
  <xsl:param name="datainizio"/>
  <msxsl:script language="C#" implements-prefix="user">
    <![CDATA[              
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
			public string GenerateID(string numb) {
        try {
          return (Convert.ToInt64(numb.Replace("AMAT_Milan_feed:","")) + 20000000).ToString();
        } catch {
          var resultId = "";
          var sourceId = numb.Replace("AMAT_Milan_feed:","");
          foreach( char c in sourceId) {
              resultId += System.Convert.ToInt32(c).ToString();
          }
          
          resultId = resultId.Substring(0, Math.Min(resultId.Length, 18));
          return (Convert.ToInt64(resultId) + 20000000).ToString();
        }
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

  <xsl:template match="OtpStopsList">
    <xsl:element name="OtpStopItem" namespace="">
      <xsl:apply-templates select="node()"/>
    </xsl:element>
  </xsl:template>
  <xsl:template match="id">
    <xsl:element name="Id" namespace="">
      <xsl:value-of select="concat(':lasernumeric',user:GenerateID(.),'lasernumeric:')"/>
    </xsl:element>
    <xsl:element name="OriginalId" namespace="">
      <xsl:value-of select="."/>
    </xsl:element>
    <xsl:element name="Sid" namespace="">
      <xsl:value-of select="concat('OtpStopItem:',.,'')"/>
    </xsl:element>
  </xsl:template>
  <xsl:template match="lat">
    <xsl:element name="Lat" namespace="">
      <xsl:value-of select="concat(':lasernumeric',.,'lasernumeric:')"/>
    </xsl:element>
  </xsl:template>
  <xsl:template match="lon">
    <xsl:element name="Lon" namespace="">
      <xsl:value-of select="concat(':lasernumeric',.,'lasernumeric:')"/>
    </xsl:element>
  </xsl:template>
  <xsl:template match="dist">
    <xsl:element name="Dist" namespace="">
      <xsl:value-of select="concat(':lasernumeric',.,'lasernumeric:')"/>
    </xsl:element>
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


</xsl:stylesheet>