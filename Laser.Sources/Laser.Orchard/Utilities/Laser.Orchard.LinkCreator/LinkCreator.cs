using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace LinkCreator
{
    class LinkCreator
    {
        [DllImport("kernel32.dll")]
        static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        enum SymbolicLink
        {
            File = 0,
            Directory = 1
        }

        static void Main(string[] args)
        {
            try
            {
                // Leggo i parametri di configurazione

                string sourceFolder = ConfigurationManager.AppSettings["SourceFolder"];
                string destFolder = ConfigurationManager.AppSettings["DestFolder"];

                // Controllo i moduli

                Console.WriteLine("-- Verifica moduli --");
                Console.WriteLine("");

                bool missingModules = CheckAndCreateLinks(sourceFolder + @"\Modules\", destFolder + @"\Modules\");

                if (!missingModules)
                    Console.WriteLine("Tutti i moduli sono già presenti.");

                // Controllo i temi

                Console.WriteLine("");
                Console.WriteLine("-- Verifica temi --");
                Console.WriteLine("");

                bool missingThemes = CheckAndCreateLinks(sourceFolder + @"\Themes\", destFolder + @"\Themes\");

                if (!missingThemes)
                    Console.WriteLine("Tutti i temi sono già presenti.");

                // Concludo
                string NwazetFolder = ConfigurationManager.AppSettings["NwazetFolder"];
              
                bool missingNwazet = CheckAndCreateSingleLink(NwazetFolder, destFolder + @"\Modules\","Nwazet.Commerce");
                if (!missingNwazet)
                    Console.WriteLine("Nwazet presente.");


                Console.WriteLine("");
                Console.Write("-- Operazione terminata. Premere invio per chiudere la finestra. --");
            }
            catch (Exception e)
            {
                Console.WriteLine("");
                Console.WriteLine("-- Si è verificato un errore: " + e.Message + " --");
                Console.Write("-- Operazione interrotta. Premere invio per chiudere la finestra. --");
                Console.ReadLine();
            }
        }

        private static bool CheckAndCreateLinks(string sourcePath, string destPath)
        {
            string[] items = Directory.GetDirectories(sourcePath);
            string[] itemsLinks = Directory.GetDirectories(destPath);

            bool missingItems = false;

            foreach (string item in items)
            {
                string itemName = item.Remove(0, sourcePath.Length);
                if (!itemsLinks.Contains(destPath + itemName))
                {
                    Console.WriteLine("Collegamento a " + itemName + " mancante: lo aggiungo.");
                    Console.WriteLine("1" + destPath + itemName);
                    Console.WriteLine("2" + Path.GetFullPath(item));
                    CreateSymbolicLink(destPath + itemName, Path.GetFullPath(item), SymbolicLink.Directory);
                    missingItems = true;
                }
            }

            return missingItems;
        }

        public static bool CheckAndCreateSingleLink(string sourcePath, string destPath,string foldername) {
            string[] itemsLinks = Directory.GetDirectories(destPath);
            if (!itemsLinks.Contains(destPath + foldername)) {
                Console.WriteLine("Collegamento a " + foldername + " mancante: lo aggiungo.");
                Console.WriteLine("1" + destPath + foldername);
                Console.WriteLine("2" + Path.GetFullPath(sourcePath + foldername));
                CreateSymbolicLink(destPath + foldername, Path.GetFullPath(sourcePath + foldername), SymbolicLink.Directory);
                return true;
            }
            return false;
        }
    }
}
