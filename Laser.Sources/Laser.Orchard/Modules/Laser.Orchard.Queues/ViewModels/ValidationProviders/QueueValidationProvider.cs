using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Laser.Orchard.Queues.ViewModels
{
    public class ValidateQueueTicketData : ValidationAttribute
    {
        public Localizer T { get; set; }

        public ValidateQueueTicketData()
        {
            T = NullLocalizer.Instance;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            QueueEdit queueEditVM = (QueueEdit)value;

            if (queueEditVM.Delete)
                return ValidationResult.Success;
            else
            {
                if (String.IsNullOrEmpty(queueEditVM.QueueName))
                    return new ValidationResult(T("The Queue Name field is required.").Text);
                else if (queueEditVM.TicketGap == null || queueEditVM.MaxTicketNumber == null)
                    return new ValidationResult(T("Error on queue {0}: the Number Distance and Max Number fields are required.", queueEditVM.QueueName).Text);
                else if (queueEditVM.TicketGap <= 0 || queueEditVM.MaxTicketNumber <= 0)
                    return new ValidationResult(T("Error on queue {0}: the Number Distance and Max Number fields must be integer numbers greater than 0.", queueEditVM.QueueName).Text);
                else
                    return ValidationResult.Success;
            }
        }
    }
}