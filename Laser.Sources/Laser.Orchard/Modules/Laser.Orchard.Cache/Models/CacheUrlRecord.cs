namespace Laser.Orchard.Cache.Models {

    public class CacheUrlRecord {
        public virtual int Id { get; set; }
        public virtual int Priority { get; set; }
        public virtual int CacheDuration { get; set; }
        public virtual int CacheGraceTime { get; set; }
        public virtual string CacheURL { get; set; }
        public virtual string CacheToken { get; set; }
    }
}