using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PlacementInfo {
    class Program {

        private static string rootPath;
        private static Dictionary<FileInfo, string> allPlacementFile;
        private static List<FileInfo> allPlacementFileInfo;
        private static List<PlaceElement> placeElements;

        static void Main(string[] args) {

            allPlacementFile = new Dictionary<FileInfo, string>();
            allPlacementFileInfo = new List<FileInfo>();
            placeElements = new List<PlaceElement>();

            startProgram();
            createDictionary();
            generateElementList();
            createCsv();

            foreach (PlaceElement pe in placeElements) {
                Console.WriteLine(pe.ToString());
            }

            Console.WriteLine("\n\nPremere Invio per uscire");
            Console.ReadLine();

        }

        private static void createCsv() {
            try {
                string path = rootPath + "\\PlacementCsv.csv";
                if (File.Exists(path)) {
                    File.Delete(path);
                }

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(path)) {
                    file.WriteLine("FILE; ELEMENT; POSITION; DISPLAY TYPE; CONTENT TYPE;");
                    foreach (var line in placeElements) {                
                            file.WriteLine(line.ToString());
                    }
                }
                // Open the stream and read it back.
                using (StreamReader sr = File.OpenText(path)) {
                    string s = "";
                    while ((s = sr.ReadLine()) != null) {
                        Console.WriteLine(s);
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void startProgram() {
            Console.WriteLine("Inserire la root della cartella contenente i Placement.info e premere Invio per generare il csv");
            rootPath = Console.ReadLine();

            DirectoryInfo root = new DirectoryInfo(rootPath);
            try {
                allPlacementFileInfo = root.GetFiles("placement.info", SearchOption.AllDirectories).ToList<FileInfo>();
            } catch (DirectoryNotFoundException ex) {
                Console.WriteLine("path non valido, riprovare");
                startProgram();
                return;
            }
        }

        private static void createDictionary() {
            foreach (FileInfo fi in allPlacementFileInfo) {
                allPlacementFile[fi] = fi.OpenText().ReadToEnd().ToString();
            }
        }

        private static void generateElementList() {
            foreach (FileInfo fi in allPlacementFileInfo) {
                int filenamesize = fi.FullName.Length - rootPath.Length - "Placement.info".Length -2;
                try {
                    using (XmlReader reader = XmlReader.Create(new StringReader(allPlacementFile[fi]))) {

                        reader.Read();
                        //leggo il placemnet
                        while (!reader.Name.Equals("Placement")) {
                            reader.Read();
                        }
                        
                        parseChilds(reader, new PlaceElement(fi.FullName.Substring(rootPath.Length + 1, filenamesize)));
                    }
                }catch(Exception ex) {
                    PlaceElement placeError;
                    if (!(filenamesize <0)) {
                        placeError = new PlaceElement(fi.FullName.Substring(rootPath.Length + 1, filenamesize));
                    }else {
                        placeError = new PlaceElement(fi.FullName);
                    }
                    placeError.ContentType = ex.ToString().Replace(System.Environment.NewLine, "replacement text");
                    placeElements.Add(placeError);
                }
            }
        }

        private static void parseChilds(XmlReader reader, PlaceElement pe) {
            while (!reader.NodeType.Equals(XmlNodeType.Element)) {
                if (!reader.Read()) {
                    return;
                }
            }
            using (var subtree = reader.ReadSubtree()) {
                while (subtree.Read()) {
                    if (reader.NodeType.Equals(XmlNodeType.Element)) {
                        if (reader.Name.Equals("Match")) {
                            parseMatch(reader, pe);
                        } else if (reader.Name.Equals("Place")) {
                            parsePlace(reader, pe);
                        }
                    }
                }
            }
        }

        private static void parseMatch(XmlReader reader, PlaceElement pe) {
            PlaceElement newPe = PlaceElement.dupplica(pe);
            for (int i = 0; i < reader.AttributeCount; i++) {
                reader.MoveToNextAttribute();
                if (reader.Name.Equals("ContentType")) {
                    newPe.ContentType = reader.Value;
                } else if (reader.Name.Equals("DisplayType")) {
                    newPe.DisplayType = reader.Value;
                }
            }

            parseChilds(reader, newPe);
        }

        private static void parsePlace(XmlReader reader, PlaceElement pe) {
            for (int i = 0; i < reader.AttributeCount; i++) {
                reader.MoveToNextAttribute();
                pe.Element = reader.Name;
                pe.Position = reader.Value;
                placeElements.Add(PlaceElement.dupplica(pe));
            }
        }
    }

    class PlaceElement {
        public string FileName { set; get; }
        public string DisplayType { set; get; }
        public string ContentType { set; get; }
        public string Element { set; get; }
        public string Position { set; get; }

        public static PlaceElement dupplica(PlaceElement pe) {
            return new PlaceElement(pe.FileName, pe.DisplayType,
                pe.ContentType, pe.Element, pe.Position);
        }

        public PlaceElement(string fileName, string displayType, string contentType, string element, string position) {
            this.FileName = fileName;
            this.DisplayType = displayType;
            this.ContentType = contentType;
            this.Element = element;
            this.Position = position;
        }

        public PlaceElement(string fileName) {
            this.FileName = fileName;
            this.DisplayType = "";
            this.Element = "";
            this.ContentType = "";
            this.Position = "";
        }

        public override string ToString() {
            return FileName.Replace(';','|') + ";" +  Element.Replace(';', '|') + ";" + Position.Replace(';', '|') + ";"
                +  DisplayType.Replace(';', '|') + ";" + ContentType.Replace(';', '|') + ";";
        }

    }

}
