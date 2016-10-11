using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.FidelityGateway.Models
{
    public class FidelityReward
    {
        public string id { set; get; }
        public string description { set; get; }
        public Dictionary<string, string> data { set; get; }

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