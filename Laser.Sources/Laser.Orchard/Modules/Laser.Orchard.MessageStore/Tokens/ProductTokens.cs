using System;
using Laser.Orchard.MessageStore.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Laser.Orchard.MessageStore.Tokens {
    [OrchardFeature("Laser.Orchard.MessageStore")]
    public class StoreTokens : ITokenProvider {
        private readonly IContentManager _contentManager;

        public StoreTokens(IContentManager contentManager) {
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(dynamic context) {
            context.For("Content", T("Content Items"), T("Content Items"))
                .Token("MessageTextHtml", T("Message html"), T("The text of the message."))
                .Token("MessageFrom", T("MessageFrom"), T("Message from."))
                .Token("MessageTo", T("MessageTo"), T("Message to."))
                .Token("MessageObject", T("MessageObject"), T("Message Object."))
                ;
        }

        public void Evaluate(dynamic context) {
            context.For<IContent>("Content")
                .Token("MessageTextHtml", (Func<IContent, object>)(content => content.As<MessageStorePart>().MessageTextHtml))
                .Token("MessageFrom", (Func<IContent, object>)(content => content.As<MessageStorePart>().MessageFrom))
                .Token("MessageTo", (Func<IContent, object>)(content => content.As<MessageStorePart>().MessageTo))
                 .Token("MessageObject", (Func<IContent, object>)(content => content.As<MessageStorePart>().MessageObject))
                ;
        }
    }
}
