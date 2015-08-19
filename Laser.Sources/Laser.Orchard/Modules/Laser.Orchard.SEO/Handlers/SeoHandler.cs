﻿using Laser.Orchard.SEO.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.SEO.Handlers {

  public class SeoHandler : ContentHandler {

    public SeoHandler(IRepository<SeoRecord> repository) {
      Filters.Add(StorageFilter.For(repository));
    }
  }
}