using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.TranslationChecker.Models {
    public class TranslationRecord {
        public int Id { get; set; }
        public string ContainerName { get; set; }
        public string ContainerType { get; set; }
        public string Context { get; set; }
        public string Message { get; set; }
        public string TranslatedMessage { get; set; }
        public string Language { get; set; }
    }
}
