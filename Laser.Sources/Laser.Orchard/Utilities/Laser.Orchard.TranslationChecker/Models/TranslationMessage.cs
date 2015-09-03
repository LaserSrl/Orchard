using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Laser.Orchard.TranslationChecker.Models {
    public class TranslationMessage {
        public string ContainerName { get; set; }
        public TranslationArea ContainerType { get; set; }
        public string TinyContainerType {
            get {
                return ContainerType.ToString().Substring(0, 1);
            }
        }
        public string MessageContext { get; set; }
        public string MessageId { get; set; }
        public object MessageLanguage { get; set; }
    }
}
