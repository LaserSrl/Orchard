using System.ComponentModel.DataAnnotations;

namespace Laser.Orchard.Queues.ViewModels
{
    [ValidateQueueTicketData]
    public class QueueEdit
    {
        public int Id { get; set; }

        public string QueueName { get; set; }

        public int? TicketGap { get; set; }

        public int? MaxTicketNumber { get; set; }

        public bool Delete { get; set; }
    }
}