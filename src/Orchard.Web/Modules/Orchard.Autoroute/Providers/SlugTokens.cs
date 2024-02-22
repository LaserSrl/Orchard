﻿using System;
using Orchard.Autoroute.Services;
using Orchard.Tokens;
using Orchard.Localization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Autoroute.Models;
using Orchard.Core.Common.Models;

namespace Orchard.Autoroute.Providers {
    public class SlugTokens : ITokenProvider {
        private readonly ISlugService _slugService;
        private readonly IHomeAliasService _homeAliasService;

        public SlugTokens(ISlugService slugService, IHomeAliasService homeAliasService) {
            T = NullLocalizer.Instance;
            _slugService = slugService;
            _homeAliasService = homeAliasService;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            context.For("Content")
                // /my-item
                .Token("Slug", T("Slug"), T("A slugified version of the item title appropriate for content Urls"))
                // /path/to/my-item
                .Token("Path", T("Path"), T("The full path of an item as already generated by Autoroute"))
                // /path/to/parent-item/
                .Token("ParentPath", T("Parent Path"), T("The parent item's path and slug with an appended forward slash if non-empty"));

            context.For("TypeDefinition")
                // /blog-post
                .Token("Slug", T("Slug"), T("Slugified version of content type display name."));

            context.For("Text")
                .Token("Slug", T("Slug"), T("Slugify the text"));
        }

        public void Evaluate(EvaluateContext context) {
            context.For<IContent>("Content")
                // {Content.Slug}
                .Token("Slug", (content => content == null ? String.Empty : _slugService.Slugify(content)))
                .Chain("Slug", "Text", (content => content == null ? String.Empty : _slugService.Slugify(content)))
                .Token("Path", (content => {
                    var autoroutePart = content.As<AutoroutePart>();
                    if (autoroutePart == null) {
                        return String.Empty;
                    }
                    var isHomePage = _homeAliasService.IsHomePage(autoroutePart);
                    return isHomePage ? String.Empty : autoroutePart.DisplayAlias;
                }))
                // {Content.ParentPath}
                .Token("ParentPath", (content => {
                    var common = content.As<CommonPart>();
                    if (common == null || common.Container == null) {
                        return String.Empty;
                    }
                    var containerAutoroutePart = common.Container.As<AutoroutePart>();
                    if (containerAutoroutePart == null) {
                        return String.Empty;
                    }
                    if (String.IsNullOrEmpty(containerAutoroutePart.DisplayAlias))
                        return String.Empty;

                    var isHomePage = _homeAliasService.IsHomePage(containerAutoroutePart);
                    return isHomePage ? "/" : containerAutoroutePart.DisplayAlias + "/";
                }));

            context.For<ContentTypeDefinition>("TypeDefinition")
                // {Content.ContentType.Slug}
                .Token("Slug", (ctd => _slugService.Slugify(ctd.DisplayName)));

            context.For<String>("Text")
                .Token("Slug", text => _slugService.Slugify(text));
        }
    }
}
