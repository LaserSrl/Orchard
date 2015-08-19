namespace Laser.Orchard.Queues.Models
{
    public class QueueRecord
    {
        public virtual int Id { get; set; }
        public virtual string QueueName { get; set; }
        public virtual int TicketGap { get; set; }
        public virtual int MaxTicketNumber { get; set; }
    }
}