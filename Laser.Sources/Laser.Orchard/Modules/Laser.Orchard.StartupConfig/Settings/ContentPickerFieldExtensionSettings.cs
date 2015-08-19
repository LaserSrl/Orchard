
namespace Laser.Orchard.StartupConfig.Settings
{
    public class ContentPickerFieldExtensionSettings
    {
        public bool CascadePublish { get; set; }
        public bool TranslateContents { get; set; }

        public ContentPickerFieldExtensionSettings()
        {
            this.TranslateContents = true;
        }
    }
}

