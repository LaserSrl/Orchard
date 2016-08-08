using Orchard.ContentManagement.Handlers;
using Orchard.MediaLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Laser.Orchard.StartupConfig.Handlers {
    public class ExternalMediaHandler : ContentHandler {

        public ExternalMediaHandler() {

            OnLoaded<MediaPart>((context, part) => {
                if (!String.IsNullOrEmpty(part.FileName)) {
                    //regex pattern for http or https
                    //^(https?):\/\/\S+[.]\S+
                    //check whether the filename points to a remote resource
                    if (Regex.IsMatch(part.FileName, @"^(https?):\/\/\S+[.]\S+")) {
                        part._publicUrl.Loader(x => part.FileName);
                    }
                }
            });
        }
    }
}