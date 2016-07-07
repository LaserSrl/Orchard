﻿using Laser.Orchard.Vimeo.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo.Handlers {

    public class VimeoSettingsPartHandler : ContentHandler {
        public VimeoSettingsPartHandler(IRepository<VimeoSettingsPartRecord> repository) {
            Filters.Add(new ActivatingFilter<VimeoSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));

            //ondisplaying of a mediapart containing a vimeo video, check controller: if the request is coming from a mobile
            //app we need to send a crypted URL, rather than the oEmbed, because we cannot whitelist apps
        }
    }
}