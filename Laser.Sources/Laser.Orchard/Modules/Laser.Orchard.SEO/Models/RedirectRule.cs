using Orchard.Environment.Extensions;
using System;
using System.ComponentModel.DataAnnotations;

namespace Laser.Orchard.SEO.Models {
    [OrchardFeature("Laser.Orchard.Redirects")]
    public class RedirectRule {

        public RedirectRule() {
            CreatedDateTime = DateTime.Now;
        }

        public virtual int Id { get; set; }

        public virtual DateTime CreatedDateTime { get; set; }

        [Required]
        [RegularExpression(ValidRelativeUrlPattern, ErrorMessage = "Do not start with '~/'")]
        [Display(Name = "Source URL")]
        public virtual string SourceUrl { get; set; }

        [Required]
        [RegularExpression(ValidRelativeUrlPattern, ErrorMessage = "Do not start with '~/'")]
        [Display(Name = "Destination URL")]
        public virtual string DestinationUrl { get; set; }

        public virtual bool IsPermanent { get; set; }

        public const string ValidRelativeUrlPattern = @"^[^\~\/\\].*";
    }
}