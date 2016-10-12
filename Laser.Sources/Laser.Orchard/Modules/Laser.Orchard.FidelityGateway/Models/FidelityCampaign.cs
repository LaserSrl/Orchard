using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.FidelityGateway.Models
{
    public class FidelityCampaign
    {
        public string Name { set; get; }
        public string Id { set; get; }
        public Dictionary<FidelityReward, double> Catalog {set; get;}
        public Dictionary<string, string> Data { set; get; }

        public bool AddReward(FidelityReward reward, double points)
        {
            try
            {
                Catalog.Add(reward, points);
            }
            catch (ArgumentException ex)
            {
                return false;
            }
            return true;
        }

        public bool AddData(string k, string v)
        {
            try
            {
                Data.Add(k, v);
            }
            catch (ArgumentException ex)
            {
                return false;
            }
            return true;
        } 
    }
}