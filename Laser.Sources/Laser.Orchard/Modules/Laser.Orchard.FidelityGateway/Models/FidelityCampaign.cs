using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.FidelityGateway.Models
{
    public class FidelityCampaign
    {
        public string name { set; get; }
        public string id { set; get; }
        public Dictionary<FidelityReward, double> catalog {set; get;}
        public Dictionary<string, string> data { set; get; }

        public bool addReward(FidelityReward reward, double points)
        {
            try
            {
                catalog.Add(reward, points);
            }
            catch (ArgumentException ex)
            {
                return false;
            }
            return true;
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