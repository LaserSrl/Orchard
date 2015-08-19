using Orchard.ContentManagement;
using Orchard.DisplayManagement.Implementation;
using Orchard.Widgets.Models;


namespace Laser.Orchard.ZoneAlternates {


    public class ZoneAlternatesFactory : ShapeDisplayEvents {

        private string lastZone = "";
        public override void Displaying(ShapeDisplayingContext context) {

            context.ShapeMetadata
              .OnDisplaying(displayedContext => {

                  // We don't want the widget itself, 
                  // but the content item that consists of the Widget part (e.g. Parts.Blogs.RecentBlogPosts)
                  if (displayedContext.ShapeMetadata.Type != "Widget") {
                      if (displayedContext.ShapeMetadata.Type == "Zone") {
                          lastZone = displayedContext.Shape.ZoneName;
                      } else {
                          ContentItem contentItem = displayedContext.Shape.ContentItem;
                          ContentPart contentPart = displayedContext.Shape.ContentPart is ContentPart ? displayedContext.Shape.ContentPart : null;
                          ContentField contentField = displayedContext.Shape.ContentField is ContentField ? displayedContext.Shape.ContentField : null;
                          var displayType = displayedContext.ShapeMetadata.DisplayType;

                          if (contentItem != null && lastZone != "") {

                              // contentItem è un Widget?
                              var zoneName = lastZone;
                              var shapeName = displayedContext.ShapeMetadata.Type;
                              // [ShapeName]-[ZoneName].cshtml: "Parts.Blogs.RecentBlogPosts-myZoneName.cshtml"
                              // [ContentTypeName]-[ZoneName].cshtml: "RecentBlogPosts-myZoneName.cshtml"
                              if (!displayedContext.ShapeMetadata.Alternates.Contains(shapeName + "__" + zoneName)) {
                                  displayedContext.ShapeMetadata.Alternates.Add(shapeName + "__" + zoneName);
                                  if (!string.IsNullOrWhiteSpace(displayType) && displayType != "Detail") {
                                      displayedContext.ShapeMetadata.Alternates.Add(shapeName + "__" + zoneName + "__" + displayType);
                                  }

                              }
                              if (!displayedContext.ShapeMetadata.Alternates.Contains(shapeName + "__" + contentItem.ContentType + "__" + zoneName)) {
                                  displayedContext.ShapeMetadata.Alternates.Add(shapeName + "__" + contentItem.ContentType + "__" + zoneName);
                                  if (!string.IsNullOrWhiteSpace(displayType) && displayType != "Detail") {
                                      displayedContext.ShapeMetadata.Alternates.Add(shapeName + "__" + contentItem.ContentType + "__" + zoneName + "__" + displayType);
                                  }
                              }
                              if (contentField != null) {
                                  if (!displayedContext.ShapeMetadata.Alternates.Contains(shapeName + "__" + contentField.Name + "__" + zoneName)) {
                                      displayedContext.ShapeMetadata.Alternates.Add(shapeName + "__" + contentField.Name + "__" + zoneName);
                                      if (!string.IsNullOrWhiteSpace(displayType) && displayType != "Detail") {
                                          displayedContext.ShapeMetadata.Alternates.Add(shapeName + "__" + contentField.Name + "__" + zoneName + "__" + displayType);
                                      }

                                  }
                                  if (!displayedContext.ShapeMetadata.Alternates.Contains(shapeName + "__" + contentItem.ContentType + "__" + contentField.Name + "__" + zoneName)) {
                                      displayedContext.ShapeMetadata.Alternates.Add(shapeName + "__" + contentItem.ContentType + "__" + contentField.Name + "__" + zoneName);
                                      if (!string.IsNullOrWhiteSpace(displayType) && displayType != "Detail") {
                                          displayedContext.ShapeMetadata.Alternates.Add(shapeName + "__" + contentItem.ContentType + "__" + contentField.Name + "__" + zoneName + "__" + displayType);
                                      }
                                  }

                              }

                          }
                      }
                  }
              });
        }


    }
}