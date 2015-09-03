using System.Collections.Generic;

namespace Laser.Orchard.Translator.ViewModels
{
    public class TranslatorViewModel
    {
        public IEnumerable<string> CultureList { get; set; }
        public bool ShowAdvancedOperations { get; set; }
    }
}