<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"   xmlns:msxsl="urn:schemas-microsoft-com:xslt"   xmlns:user="http://c1.composite.net/sample/csharp" xmlns:HttpUtility="my:HttpUtility">
  <xsl:output
    method="xml"
    encoding="UTF-8"
    omit-xml-declaration="yes"
    indent="yes" />
  <xsl:param name="LinguaParameter"/>
  <msxsl:script language="C#" implements-prefix="user">
    <msxsl:assembly name="System.Core" />
    <msxsl:assembly name="System.Xml.Linq" />
    <msxsl:using namespace="System.Collections.Generic" />
    <msxsl:using namespace="System.Linq" />
    <msxsl:using namespace="System.Xml.Linq" />

    <![CDATA[     

		  public string filtra(string elencotag, string linguaTag)
		  {
        // Definziione Dictionary in lingua
        // IT
        var dictionaryIT = new Dictionary<string, string>();
        dictionaryIT.Add("Il Carema","Il Carema");
        dictionaryIT.Add("Erbaluce di Caluso","Erbaluce di Caluso");
        dictionaryIT.Add("Freisa Chieri","Freisa Chieri");
        dictionaryIT.Add("Il Valsusa","Il Valsusa");
        dictionaryIT.Add("Il Pinerolese","Il Pinerolese");
        dictionaryIT.Add("Il Canavese","Il Canavese");
        dictionaryIT.Add("Il Collina Torinese","Il Collina Torinese");
        dictionaryIT.Add("Ramie","Ramie");
        dictionaryIT.Add("Doux DHenry","Doux D'Henry");
        dictionaryIT.Add("Birra artigianale","Birra artigianale");
        dictionaryIT.Add("Genepi","Genepi");
        dictionaryIT.Add("Miele di montagna","Miele di montagna");
        dictionaryIT.Add("Cevrin di Coazze","Cevrin di Coazze");
        dictionaryIT.Add("Seirass del Fen","Seirass del Fen");
        dictionaryIT.Add("Toma del lait brusc","Toma del lait brusc");
        dictionaryIT.Add("Toma di Lanzo","Toma di Lanzo");
        dictionaryIT.Add("Toma d’Trausela","Toma d’Trausela");
        dictionaryIT.Add("Civrin della Valchiusella","Civrin della Valchiusella");
        dictionaryIT.Add("Plaisentif","Plaisentif");
        dictionaryIT.Add("Antiche Mele Piemontesi","Antiche Mele Piemontesi");
        dictionaryIT.Add("Ciliegia di Pecetto e Ratafià","Ciliegia di Pecetto e Ratafià");
        dictionaryIT.Add("Marrone della Valle di Susa","Marrone della Valle di Susa");
        dictionaryIT.Add("Piccoli frutti","Piccoli frutti");
        dictionaryIT.Add("Asparago di Santena e delle Terre del Pianalto","Asparago di Santena e delle Terre del Pianalto");
        dictionaryIT.Add("Cavolo Verza di Montalto Dora","Cavolo Verza di Montalto Dora");
        dictionaryIT.Add("Cipolla Piatlina bionda di Andezeno","Cipolla Piatlina bionda di Andezeno");
        dictionaryIT.Add("Patata di montagna","Patata di montagna");
        dictionaryIT.Add("Peperone di Carmagnola","Peperone di Carmagnola");
        dictionaryIT.Add("Ravanello lungo Torino","Ravanello lungo Torino");
        dictionaryIT.Add("Cavolfiore di Moncalieri","Cavolfiore di Moncalieri");
        dictionaryIT.Add("Fagiolo bianco piattella di Cortereggio","Fagiolo bianco piattella di Cortereggio");
        dictionaryIT.Add("Ciapinabò o topinambur","Ciapinabò o topinambur");
        dictionaryIT.Add("Gianduiotto di Torino","Gianduiotto di Torino");
        dictionaryIT.Add("Cioccolato","Cioccolato");
        dictionaryIT.Add("Mustardela della Val Pellice","Mustardela della Val Pellice");
        dictionaryIT.Add("Salame di giora di Carmagnola","Salame di giora di Carmagnola");
        dictionaryIT.Add("Salame di Turgia","Salame di Turgia");
        dictionaryIT.Add("Salampatata del Canavese","Salampatata del Canavese");
        dictionaryIT.Add("Paste di meliga","Paste di meliga");
        dictionaryIT.Add("Gofri","Gofri");
        dictionaryIT.Add("Canestrelli","Canestrelli");
        dictionaryIT.Add("Baciaje di Cercenasco","Baciaje di Cercenasco");
        dictionaryIT.Add("Torcetto di Lanzo e del Canavese","Torcetto di Lanzo e del Canavese");
        dictionaryIT.Add("Gelato artigianale","Gelato artigianale");
        dictionaryIT.Add("Piccola pasticceria artigianale","Piccola pasticceria artigianale");
        dictionaryIT.Add("Rubatà del Chierese","Rubatà del Chierese");
        dictionaryIT.Add("Grissino stirato torinese","Grissino stirato torinese");
        dictionaryIT.Add("Tinca gobba di Poirino","Tinca gobba di Poirino");
        dictionaryIT.Add("Menta di Pancalieri","Menta di Pancalieri");
        dictionaryIT.Add("Panettone basso glassato di Pinerolo","Panettone basso glassato di Pinerolo");
        dictionaryIT.Add("Nocciolini di Chivasso","Nocciolini di Chivasso");
        dictionaryIT.Add("Torta Zurigo","Torta Zurigo");
        dictionaryIT.Add("Torta 900","Torta 900");
        dictionaryIT.Add("Focaccia di Susa","Focaccia di Susa");
        dictionaryIT.Add("Sedano rosso di Orbassano","Sedano rosso di Orbassano");
        dictionaryIT.Add("Prosciuttello della Valle di Susa","Prosciuttello della Valle di Susa");
        dictionaryIT.Add("Tomino del Talucco","Tomino del Talucco");
        dictionaryIT.Add("Tomino di Rivalta","Tomino di Rivalta");
        dictionaryIT.Add("Erbe spontanee e officinali della Valchiusella","Erbe spontanee e officinali della Valchiusella");
        dictionaryIT.Add("Pane di Giaveno (De.C.O.)","Pane di Giaveno (De.C.O.)");
        dictionaryIT.Add("Caffè","Caffè");
        dictionaryIT.Add("Funghi","Funghi");
        dictionaryIT.Add("Farine Antichi Mais","Farine Antichi Mais");
        //ENG
        var dictionaryEN = new Dictionary<string, string>();
        dictionaryEN.Add("Il Carema","Carema wine");
        dictionaryEN.Add("Erbaluce di Caluso","Erbaluce wine");
        dictionaryEN.Add("Freisa Chieri","Freisa wine");
        dictionaryEN.Add("Il Valsusa","Valsusa DOC wine");
        dictionaryEN.Add("Il Pinerolese","Pinerolese doc wine");
        dictionaryEN.Add("Il Canavese","Canavese rosso DOC wine");
        dictionaryEN.Add("Il Collina Torinese","Collina Torinese wine");
        dictionaryEN.Add("Ramie","Ramie wine");
        dictionaryEN.Add("Doux DHenry","Doux d'Henry wine");
        dictionaryEN.Add("Birra artigianale","Beer");
        dictionaryEN.Add("Genepi","Genepi");
        dictionaryEN.Add("Miele di montagna","Mountain honey");
        dictionaryEN.Add("Cevrin di Coazze","Cevrin di Coazze");
        dictionaryEN.Add("Seirass del Fen","Seirass del fen");
        dictionaryEN.Add("Toma del lait brusc","Toma del Lait Brusc");
        dictionaryEN.Add("Toma di Lanzo","Toma from Lanzo");
        dictionaryEN.Add("Toma d’Trausela","Toma d'Trausela");
        dictionaryEN.Add("Civrin della Valchiusella","Civrin della Valchiusella");
        dictionaryEN.Add("Plaisentif","Plaisentif");
        dictionaryEN.Add("Antiche Mele Piemontesi","Ancient Piedmontese Apples");
        dictionaryEN.Add("Ciliegia di Pecetto e Ratafià","Cherries from Pecetto");
        dictionaryEN.Add("Marrone della Valle di Susa","Susa Valley Chestnuts");
        dictionaryEN.Add("Piccoli frutti","Small fruits");
        dictionaryEN.Add("Asparago di Santena e delle Terre del Pianalto","Asparagus from Santena");
        dictionaryEN.Add("Cavolo Verza di Montalto Dora","Cabbage from Montalto Dora");
        dictionaryEN.Add("Cipolla Piatlina bionda di Andezeno","Golden piatlina onion from Andezeno");
        dictionaryEN.Add("Patata di montagna","Mountain potatoe");
        dictionaryEN.Add("Peperone di Carmagnola","Pepper from Carmagnola");
        dictionaryEN.Add("Ravanello lungo Torino","Turin radish");
        dictionaryEN.Add("Cavolfiore di Moncalieri","Couliflower of Moncalieri");
        dictionaryEN.Add("Fagiolo bianco piattella di Cortereggio","Piattella Canavesana di Cortereggio");
        dictionaryEN.Add("Ciapinabò o topinambur","Ciapinabò o topinambur");
        dictionaryEN.Add("Gianduiotto di Torino","Gianduiotto");
        dictionaryEN.Add("Cioccolato","Chocolate");
        dictionaryEN.Add("Mustardela della Val Pellice","Mustardela from Val Pellice");
        dictionaryEN.Add("Salame di giora di Carmagnola","Salame di Giora from Carmagnola");
        dictionaryEN.Add("Salame di Turgia","Salame di Turgia");
        dictionaryEN.Add("Salampatata del Canavese","Salampatata from Canavese");
        dictionaryEN.Add("Paste di meliga","Meliga biscuits");
        dictionaryEN.Add("Gofri","Gofri");
        dictionaryEN.Add("Canestrelli","Sweetmeats Canestrelli");
        dictionaryEN.Add("Baciaje di Cercenasco","Baciaje from Cercenasco");
        dictionaryEN.Add("Torcetto di Lanzo e del Canavese","Torcetti biscuits from Lanzo and Canavese area");
        dictionaryEN.Add("Gelato artigianale","Ice cream");
        dictionaryEN.Add("Piccola pasticceria artigianale","Small artisan patisserie");
        dictionaryEN.Add("Rubatà del Chierese","Rubatà from Chierese area");
        dictionaryEN.Add("Grissino stirato torinese","Bread-stick from Torinese area");
        dictionaryEN.Add("Tinca gobba di Poirino","Tench from Poirino");
        dictionaryEN.Add("Menta di Pancalieri","Peppermint and medicinal herbs");
        dictionaryEN.Add("Panettone basso glassato di Pinerolo","Short, glazed panettone with Piemonte hazelnuts of Pinerolo");
        dictionaryEN.Add("Nocciolini di Chivasso","Nocciolini from Chivasso");
        dictionaryEN.Add("Torta Zurigo","Torta Zurigo");
        dictionaryEN.Add("Torta 900","Torta '900 of Ivrea");
        dictionaryEN.Add("Focaccia di Susa","Focaccia di Susa");
        dictionaryEN.Add("Sedano rosso di Orbassano","Red celery of Orbassano");
        dictionaryEN.Add("Prosciuttello della Valle di Susa","Susa Valley ham");
        dictionaryEN.Add("Tomino del Talucco","Tomino del Talucco cheese");
        dictionaryEN.Add("Tomino di Rivalta","Tomino di Rivalta cheese");
        dictionaryEN.Add("Erbe spontanee e officinali della Valchiusella","Wild and medicinal herbs in Valchiusella");
        dictionaryEN.Add("Pane di Giaveno (De.C.O.)","Giaveno De.C.O. bread");
        dictionaryEN.Add("Caffè","Coffee");
        dictionaryEN.Add("Funghi","Mushrooms");
        dictionaryEN.Add("Farine Antichi Mais","Ancient corn flours");

        
        try {
			    List<string>moiconta=new List<string>();
			    string[] taginseriti=elencotag.Split(',');
			    string tagvalidi = "Il Carema,Erbaluce di Caluso,Freisa Chieri,Il Valsusa,Il Pinerolese,Il Canavese,Il Collina Torinese,Ramie,Doux DHenry,Birra artigianale,Genepi,Miele di montagna,Cevrin di Coazze,Seirass del Fen,Toma del lait brusc,Toma di Lanzo,Toma d’Trausela,Civrin della Valchiusella,Plaisentif,Antiche Mele Piemontesi,Ciliegia di Pecetto e Ratafià,Marrone della Valle di Susa,Piccoli frutti,Asparago di Santena e delle Terre del Pianalto,Cavolo Verza di Montalto Dora,Cipolla Piatlina bionda di Andezeno,Patata di montagna,Peperone di Carmagnola,Ravanello lungo Torino,Cavolfiore di Moncalieri,Fagiolo bianco piattella di Cortereggio,Ciapinabò o topinambur,Gianduiotto di Torino,Cioccolato,Mustardela della Val Pellice,Salame di giora di Carmagnola,Salame di Turgia,Salampatata del Canavese,Paste di meliga,Gofri,Canestrelli,Baciaje di Cercenasco,Torcetto di Lanzo e del Canavese,Gelato artigianale,Piccola pasticceria artigianale,Rubatà del Chierese,Grissino stirato torinese,Tinca gobba di Poirino,Menta di Pancalieri,Panettone basso glassato di Pinerolo,Nocciolini di Chivasso,Torta Zurigo,Torta 900,Focaccia di Susa,Sedano rosso di Orbassano,Prosciuttello della Valle di Susa,Tomino del Talucco,Tomino di Rivalta,Erbe spontanee e officinali della Valchiusella,Pane di Giaveno (De.C.O.),Caffè,Funghi,Farine Antichi Mais";
			    string[] tag=tagvalidi.Split(',');
			    foreach(string val in taginseriti){
				    if (tagvalidi.Contains(val)){
              if (linguaTag=="it" && dictionaryIT.ContainsKey(val)) {
					      moiconta.Add(dictionaryIT[val]);
				      } else if (linguaTag=="en" && dictionaryEN.ContainsKey(val)){
                moiconta.Add(dictionaryEN[val]);
              } else {
                moiconta.Add(val);
              }
            }
			    }
          if (moiconta.Count()>0){
			      return string.Join(", ",moiconta.ToArray());
          } else {
			      return ",";
          }
        } catch {
          return ",";
        }
		  }     	
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
    DateTime d2 = Convert.ToDateTime(dtstring).ToUniversalTime();
    TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);
    return  ts.TotalMilliseconds.ToString();
  }
    ]]>
  </msxsl:script>
  <xsl:template match="PriceInfo">
    <xsl:element name="{local-name()}" namespace="">
      <xsl:value-of select="HttpUtility:HtmlDecode(.)"  />
    </xsl:element>
  </xsl:template>
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
  <xsl:template match="Venue/Tag">
    <xsl:if test="not(preceding-sibling::*[1][self::Tag])">
      <xsl:text disable-output-escaping="yes">	<![CDATA[<Tag>]]></xsl:text>
      <xsl:value-of select="user:filtra(.,$LinguaParameter)"/>
    </xsl:if>
    <xsl:if test="(preceding-sibling::*[1][self::Tag])">
      <xsl:value-of select="user:filtra(concat(',',.),$LinguaParameter)"/>
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
  <xsl:template match="AvailableService">
    <xsl:call-template name="FiltroLingua"/>
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
    <xsl:variable name="timeutc" select="substring-before(substring-after(., 'T'),'+')" />
    <xsl:element name="{local-name()}" namespace="">
      <xsl:value-of select="concat(':laserDate',user:UnixTicks(concat($year,'-',$month,'-',$day,' ',$timeutc)),'laserDate:')" />
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