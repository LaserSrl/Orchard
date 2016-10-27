using System;
using System.ComponentModel.DataAnnotations;
using Laser.Orchard.Events.Models;

namespace Laser.Orchard.Events.ViewModels
{
    public class ActivityViewModel
    {
        public string DateStart { get; set; }

        public string DateEnd { get; set; }

        public string TimeStart { get; set; }

        public string TimeEnd { get; set; }

        public bool AllDay { get; set; }

        public bool Repeat { get; set; }

        public string RepeatType { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage="The periodicity value must be greater than 0.")]
        public int RepeatValue { get; set; }

        public string RepeatDetails { get; set; }

        public bool RepeatEnd { get; set; }

        [Required]
        public string RepeatEndDate { get; set; }

        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
        public bool Sunday { get; set; }

        public bool RepeatByDayNumber { get; set; }
        public ActivityPartSettings Settings { get; set; }
    }
}