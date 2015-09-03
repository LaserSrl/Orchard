﻿using System.Collections.Generic;

namespace Laser.Orchard.Translator.ViewModels
{
    public class TranslationTreeNodeViewModel
    {
        public string text;
        public Dictionary<string, string> data;
        public List<TranslationTreeNodeViewModel> children;
    }
}