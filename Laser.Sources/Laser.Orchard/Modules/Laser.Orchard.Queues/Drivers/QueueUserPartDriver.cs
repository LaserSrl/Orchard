using Laser.Orchard.Queues.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using System;
using System.Xml.Linq;

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



        protected override void Importing(QueueUserPart part, ImportContentContext context) {

            var root = context.Data.Element(part.PartDefinition.Name);           

            foreach (QueueUserRecord recQueueUser in part.UserQueues) 
            {
                if(int.Parse(root.Element("Id").Value)!=null)
                    recQueueUser.Id=int.Parse(root.Element("Id").Value);
                
                
                recQueueUser.QueueNumber = int.Parse(root.Element("QueueNumber").Value);
                recQueueUser.NumNotifications = int.Parse(root.Element("NumNotifications").Value);
                recQueueUser.RegistrationDate = DateTime.Parse(root.Element("RegistrationDate").Value);

                recQueueUser.QueueRecord.Id = int.Parse(root.Attribute("QueueRecord").Parent.Element("Id").Value);
                recQueueUser.QueueRecord.QueueName = root.Attribute("QueueRecord").Parent.Element("QueueName").Value;
                recQueueUser.QueueRecord.TicketGap = int.Parse(root.Attribute("QueueRecord").Parent.Element("TicketGap").Value);
                recQueueUser.QueueRecord.MaxTicketNumber = int.Parse(root.Attribute("QueueRecord").Parent.Element("MaxTicketNumber").Value);

            }
        }


        protected override void Exporting(QueueUserPart part, ExportContentContext context) {

            var root = context.Element(part.PartDefinition.Name);

            if(part.UserQueues!=null)
            {
                foreach (QueueUserRecord receq in part.UserQueues) 
                {
                    root.SetAttributeValue("Id", receq.Id);
                    root.SetAttributeValue("QueueNumber", receq.QueueNumber);
                    root.SetAttributeValue("NumNotifications", receq.NumNotifications);
                    root.SetAttributeValue("RegistrationDate", receq.RegistrationDate);

                    XElement QueueRec = new XElement("QueueRecord");
                    
                    QueueRec.SetAttributeValue("Id", receq.QueueRecord.Id);
                    QueueRec.SetAttributeValue("QueueName", receq.QueueRecord.Id);
                    QueueRec.SetAttributeValue("TicketGap", receq.QueueRecord.TicketGap);
                    QueueRec.SetAttributeValue("MaxTicketNumber", receq.QueueRecord.MaxTicketNumber);

                }                            

            }

        }





    }
}