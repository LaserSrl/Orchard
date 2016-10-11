using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.FidelityGateway.Models
{
    public class FidelityCustumer
    {
        public string email { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string id { get; set; }
        //TODO da espandere in qualche standard?
        public Dictionary<string, string> data { get; set; }
        public Dictionary<string, double> pointsInCampaign { get; set; }

        public FidelityCustumer(string email, string username, string password)
        {
            this.email = email;
            this.username = username;
            this.password = password;
        }

        public void setPointsCampaign(string campaign_id, double points)
        {
            pointsInCampaign[campaign_id] = points;
        }

        public bool addData(string k, string v)
        {
            try
            {
                data.Add(k, v);
            }
            catch (ArgumentException ex)
            {
                return false;
            }
            return true;

        }
    }
}