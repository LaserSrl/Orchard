using Orchard.ContentManagement;

namespace Laser.Orchard.ChartaWS.Models
{
    public class ChartaSiteSettingsPart : ContentPart
    {
        public string ChartaId
        {
            get { return this.Retrieve(x => x.ChartaId); }
            set { this.Store(x => x.ChartaId, value); }
        }
        public string RemoteUrl
        {
            get { return this.Retrieve(x => x.RemoteUrl); }
            set { this.Store(x => x.RemoteUrl, value); }
        }
        public string SaveFilePath
        {
            get { return this.Retrieve(x => x.SaveFilePath); }
            set { this.Store(x => x.SaveFilePath, value); }
        }
        public string ReplyToPaypalUrl
        {
            get { return this.Retrieve(x => x.ReplyToPaypalUrl); }
            set { this.Store(x => x.ReplyToPaypalUrl, value); }
        }
        public string PaypalMerchantMail
        {
            get { return this.Retrieve(x => x.PaypalMerchantMail); }
            set { this.Store(x => x.PaypalMerchantMail, value); }
        }
        public int ServiceRetryCount
        {
            get { return this.Retrieve(x => x.ServiceRetryCount); }
            set { this.Store(x => x.ServiceRetryCount, value); }
        }
        public string ChartaTranCommitUrl
        {
            get { return this.Retrieve(x => x.ChartaTranCommitUrl); }
            set { this.Store(x => x.ChartaTranCommitUrl, value); }
        }
        public string MailTo
        {
            get { return this.Retrieve(x => x.MailTo); }
            set { this.Store(x => x.MailTo, value); }
        }
        public string MailCc
        {
            get { return this.Retrieve(x => x.MailCc); }
            set { this.Store(x => x.MailCc, value); }
        }
        public string MailSubject
        {
            get { return this.Retrieve(x => x.MailSubject); }
            set { this.Store(x => x.MailSubject, value); }
        }
        public string MailBody
        {
            get { return this.Retrieve(x => x.MailBody); }
            set { this.Store(x => x.MailBody, value); }
        }
        public int Timeout
        {
            get { return this.Retrieve(x => x.Timeout); }
            set { this.Store(x => x.Timeout, value); }
        }
        public int MetersAroundMe
        {
            get { return this.Retrieve(x => x.MetersAroundMe); }
            set { this.Store(x => x.MetersAroundMe, value); }
        }
    }
}