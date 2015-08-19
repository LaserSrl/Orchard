using Laser.Orchard.Queues.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Laser.Orchard.Queues.ViewModels
{
    public class QueuesSettingsViewModel
    {
        [Required(ErrorMessage = "The Queue Service URL field is required.")]
        public string EndpointUrl { get; set; }

        [Required(ErrorMessage = "The Polling Interval field is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "The Polling Interval field should be an integer number greater than or equal to 0.")]
        public int PollingInterval { get; set; }

        [Required(ErrorMessage = "The Notifications to Send field is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "The Notifications to Send field should be an integer number greater than or equal to 0.")]
        public int MaxPushToSend { get; set; }

        public IEnumerable<QueueEdit> Queues { get; set; }
    }
}