using Laser.Orchard.Queues.Models;
using Laser.Orchard.Queues.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.Queues.Drivers
{
    public class QueuesSettingsDriver : ContentPartDriver<QueuesSettingsPart>
    {
        private readonly IQueuesService _queuesService;
        private readonly IControllerContextAccessor _controllerContextAccessor;

        private const string TemplateName = "Parts/QueuesSettings";

        public Localizer T { get; set; }

        public QueuesSettingsDriver(IQueuesService queuesService, IControllerContextAccessor controllerContextAccessor)
        {
            _queuesService = queuesService;
            _controllerContextAccessor = controllerContextAccessor;
            T = NullLocalizer.Instance;
        }

        protected override string Prefix { get { return "QueuesSettings"; } }

        //GET
        protected override DriverResult Editor(QueuesSettingsPart part, dynamic shapeHelper)
        {

            return ContentShape("Parts_QueuesSettings_Edit",
                () =>
                {
                    IEnumerable<QueueEdit> queues = null;
                    var queueWithErrors = _controllerContextAccessor.Context.Controller.TempData[Prefix + "QueueWithErrors"];
                    if (queueWithErrors == null)
                        queues = _queuesService.GetQueues().Select(s => new QueueEdit
                                                                            {
                                                                                Id = s.Id,
                                                                                QueueName = s.QueueName,
                                                                                TicketGap = s.TicketGap,
                                                                                MaxTicketNumber = s.MaxTicketNumber,
                                                                                Delete = false
                                                                            });
                    else
                        queues = ((IEnumerable<QueueEdit>)queueWithErrors).Where(x => x.Delete == false);

                    var model = new QueuesSettingsViewModel
                    {
                        EndpointUrl = part.EndpointUrl,
                        PollingInterval = part.PollingInterval,
                        MaxPushToSend = part.MaxPushToSend,
                        Queues = queues
                    };

                    return shapeHelper.EditorTemplate(
                    TemplateName: TemplateName,
                    Model: model,
                    Prefix: Prefix);
                }).OnGroup("Queues");
        }

        //POST
        protected override DriverResult Editor(QueuesSettingsPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            QueuesSettingsViewModel queuesSettingsVM = new QueuesSettingsViewModel();

            if (updater.TryUpdateModel(queuesSettingsVM, Prefix, null, null))
            {
                //Trattandosi di settings aggiorno solo se un parametro obbligatorio è presente, altrimenti significa che sto salvando altre impostazioni e non devo aggiornare queste.
                //Senza questo controllo causerei l'azzeramento delle impostazioni delle code.
                if (!string.IsNullOrEmpty(queuesSettingsVM.EndpointUrl))
                {
                    var queuesByName = queuesSettingsVM.Queues.GroupBy(q => new { q.QueueName }).Select(q => new { q.Key.QueueName, Occurrences = q.Count() });
                    queuesByName = queuesByName.Where(q => q.Occurrences > 1);

                    if (queuesByName.ToList().Count() > 0)
                    {
                        _controllerContextAccessor.Context.Controller.TempData[Prefix + "QueueWithErrors"] = queuesSettingsVM.Queues;
                        updater.AddModelError("QueueUpdateError", T("Cannot have multiple queues with the same name ({0})", string.Join(", ", queuesByName.Select(q => q.QueueName))));
                    }
                    else
                    {
                        part.EndpointUrl = queuesSettingsVM.EndpointUrl;
                        part.PollingInterval = queuesSettingsVM.PollingInterval;
                        part.MaxPushToSend = queuesSettingsVM.MaxPushToSend;

                        _queuesService.UpdateQueues(queuesSettingsVM.Queues);

                        if (queuesSettingsVM.PollingInterval > 0)
                            _queuesService.ScheduleStartTask(queuesSettingsVM.PollingInterval);
                    }
                }
            }
            else
            {
                _controllerContextAccessor.Context.Controller.TempData[Prefix + "QueueWithErrors"] = queuesSettingsVM.Queues;
            }

            return Editor(part, shapeHelper);
        }
    }
}