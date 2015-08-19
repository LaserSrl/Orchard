using System;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.UI.Resources;
using Laser.Orchard.SEO.Models;


namespace Laser.Orchard.SEO.Drivers {


  public class SeoDriver : ContentPartDriver<SeoPart> {


    private readonly IWorkContextAccessor _workContextAccessor;


    public SeoDriver(IWorkContextAccessor workContextAccessor) {
        _workContextAccessor = workContextAccessor;
    }


    /// <summary>
    /// GET Display.
    /// </summary>
    protected override DriverResult Display(SeoPart part, string displayType, dynamic shapeHelper) {

      if (displayType != "Detail")
        return null;

      var resourceManager = _workContextAccessor.GetContext().Resolve<IResourceManager>();

      if (!string.IsNullOrWhiteSpace(part.Description)) {
        resourceManager.SetMeta(new MetaEntry {
          Name = "description",
          Content = part.Description
        });
      }

      if (!string.IsNullOrWhiteSpace(part.Keywords)) {
        resourceManager.SetMeta(new MetaEntry {
          Name = "keywords",
          Content = part.Keywords
        });
      }

      if (!string.IsNullOrWhiteSpace(part.TitleOverride)) {
        return ContentShape("Parts_SEO", () => shapeHelper.Parts_SEO(
          TitleOverride: part.TitleOverride
        ));
      }

      return null;
    }


    /// <summary>
    /// GET Editor.
    /// </summary>
    protected override DriverResult Editor(SeoPart part, dynamic shapeHelper) {

      return ContentShape("Parts_SEO_Edit",
                          () => shapeHelper.EditorTemplate(
                            TemplateName: "Parts/SEO",
                            Model: part,
                            Prefix: Prefix));
    }


    /// <summary>
    /// POST Editor.
    /// </summary>
    protected override DriverResult Editor(SeoPart part, IUpdateModel updater, dynamic shapeHelper) {

      updater.TryUpdateModel(part, Prefix, null, null);
      return Editor(part, shapeHelper);
    }


  }
}