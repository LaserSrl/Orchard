<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:user="urn:my-scripts"
                xmlns:HttpUtility="my:HttpUtility" >
<xsl:output 
  method="xml"
  encoding="UTF-8"
  omit-xml-declaration="yes"
  indent="yes" /> 
<xsl:param name="LinguaParameter"/>
<xsl:param name="pagesize"/>
<xsl:param name="page"/>
<xsl:param name="datainizio"/>
<msxsl:script language="C#" implements-prefix="user">
<msxsl:assembly name="System.Text.RegularExpressions" />
<![CDATA[

public Int32 numero=0;

public string GeneraID(string pagesize,string page)
{
  numero=numero+1;
  return (1000000+Convert.ToInt32(pagesize)*Convert.ToInt32(page)+numero).ToString();
}

public string correctNumber(string numb) { return numb.Replace(',','.'); }

public string GenerateID(string numb) { return (Convert.ToInt32(numb)+5000000).ToString(); }

public string CompletaLinkRelativi(string testo)
{
  return Regex.Replace(testo, "href=\\\"(?!mailto:|http://)/*([^\\\"]*)\\\"", "href=\"http://www.grand-paradis.it/$1\"");
}

public bool StessaData(DateTime t1,DateTime t2)
{
  if (t1==t2)
    return true;
  else
    return false;
}
	
public string UnixTicks( string dtstring)
{
  DateTime d1 = new DateTime(1970, 1, 1);
  DateTime d2 = Convert.ToDateTime(dtstring);
  TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);
  return  ts.TotalMilliseconds.ToString();
}
  
]]>
</msxsl:script>
<xsl:template match="*">
    <xsl:element name="{local-name()}" namespace="">
		<xsl:apply-templates select="node()"/>
    </xsl:element>
</xsl:template>
<xsl:template match="root">
	<xsl:element name="ToRemove" namespace="">
		<xsl:apply-templates select="node()"/>	
	</xsl:element>
</xsl:template>
<xsl:template match="numeroDiElementiTotale">
</xsl:template>
<xsl:template match="numeroDellaPagina">
</xsl:template>
<xsl:template match="elementi">
<xsl:element name="Elementi" namespace="">
			<xsl:apply-templates select="node()"/>
		</xsl:element>
</xsl:template>
<xsl:template match="abstract">
		<xsl:element name="AbstractText" namespace="">
			<xsl:apply-templates select="node()"/>
		</xsl:element>
</xsl:template>
<xsl:template match="testo">
		<xsl:element name="testo" namespace="">
			<xsl:value-of select="user:CompletaLinkRelativi(.)"/>
		</xsl:element>
</xsl:template>
<xsl:template match="datadipubblicazione">
	<xsl:call-template name="TipoDateTime"/>
</xsl:template>	
<xsl:template name="TipoDateTime">
	<xsl:variable name="date" select="substring-before(., ' ')" />
	<xsl:variable name="year" select="substring-before($date, '/')" />
	<xsl:variable name="month" select="substring-before(substring-after($date, '/'), '/')" />
	<xsl:variable name="day" select="substring-after(substring-after($date, '/'), '/')" />
	<xsl:variable name="timeutc" select="substring-after(., ' ')" />
	<xsl:element name="{local-name()}" namespace="">
		<xsl:if test="$year!=''">
		<xsl:value-of select="concat(':laserDate',user:UnixTicks(concat($year,'-',$month,'-',$day,' ',$timeutc)),'laserDate:')" />
		</xsl:if>
	</xsl:element>
</xsl:template>
<xsl:template match="url">
	<xsl:element name="url" namespace="">
			<xsl:apply-templates select="node()"/>
		</xsl:element>
<xsl:element name="Id" namespace="">
<xsl:value-of select="concat(':lasernumeric',user:GeneraID($pagesize,$page),'lasernumeric:')" />
</xsl:element>
</xsl:template>
</xsl:stylesheet>