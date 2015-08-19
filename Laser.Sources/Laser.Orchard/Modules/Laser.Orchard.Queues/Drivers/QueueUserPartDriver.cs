using Laser.Orchard.Queues.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Laser.Orchard.Queues.Drivers
{
    public class QueueUserPartDriver : ContentPartDriver<QueueUserPart>
    {
        private const string TemplateName = "Parts/QueueUserRelation";

        protected override string Prefix { get { return "QueueUserRelation"; } }

        protected override DriverResult Editor(QueueUserPart part, dynamic shapeHelper)
        {
	        return ContentShape("Parts_QueueUserRelation_Edit",
	                            () => shapeHelper.EditorTemplate(
	                                    TemplateName: TemplateName,
	                                    Model: part,
				                        Prefix: Prefix));
	    }

        protected override DriverResult Editor(QueueUserPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}