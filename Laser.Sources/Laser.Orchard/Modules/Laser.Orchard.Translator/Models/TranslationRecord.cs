using System.ComponentModel.DataAnnotations;

namespace Laser.Orchard.Translator.Models
{
    public class TranslationRecord
    {
        public virtual int Id { get; set; }

        public virtual string ContainerName { get; set; }

        public virtual string ContainerType { get; set; }

        public virtual string Context { get; set; }

        public virtual string Message { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public virtual string TranslatedMessage { get; set; }

        public virtual string Language { get; set; }
    }
}