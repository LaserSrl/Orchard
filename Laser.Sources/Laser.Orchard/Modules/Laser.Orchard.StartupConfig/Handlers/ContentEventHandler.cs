using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.ContentManagement.FieldStorage;
using Orchard;
using Orchard.Fields.Fields;

namespace Laser.Orchard.StartupConfig.Handlers {
    public class FieldCreatortHandler : ContentHandler {//IFieldStorageEvents {
        private readonly IOrchardServices _orchardServices;
        public FieldCreatortHandler(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            OnUpdated<CommonPart>((context, CommonPart) => {
                if (context.ContentItem.As<CommonPart>() != null) {
                    var currentUser = _orchardServices.WorkContext.CurrentUser;
                    if (currentUser != null) {
                        ((dynamic)context.ContentItem.As<CommonPart>()).LastModifier.Value = currentUser.Id;
                        if (((dynamic)context.ContentItem.As<CommonPart>()).Creator.Value == null)
                            //  ((NumericField) CommonPart.Fields.Where(x=>x.Name=="Creator").FirstOrDefault()).Value = currentUser.Id;
                            ((dynamic)context.ContentItem.As<CommonPart>()).Creator.Value = currentUser.Id;
                    }
                }

            });
            OnCreated<CommonPart>((context, CommonPart) => {
                if (context.ContentItem.As<CommonPart>() != null) {
                    var currentUser = _orchardServices.WorkContext.CurrentUser;
                    if (currentUser != null)
                        if (((dynamic)context.ContentItem.As<CommonPart>()).Creator.Value == null)
                            //  ((NumericField) CommonPart.Fields.Where(x=>x.Name=="Creator").FirstOrDefault()).Value = currentUser.Id;
                            ((dynamic)context.ContentItem.As<CommonPart>()).Creator.Value = currentUser.Id;

                }

            });
            //OnCreated<CommonPart>((context, CommonPart) => {
            //    if (context.ContentItem.As<CommonPart>() != null) {
            //        ((dynamic)context.ContentItem.As<CommonPart>()).Creator.Value = ((dynamic)context.ContentItem.As<CommonPart>()).Owner.Id;

            //    }
            //});

        }
        //protected override void Created(CreateContentContext context) {
        //    if (context.ContentItem.As<CommonPart>() != null) {
        //     }
        //}

    }
}