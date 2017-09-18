// Decompiled with JetBrains decompiler
// Type: Laser.Orchard.AppDirect.Models.LogEventsRecord
// Assembly: Laser.Orchard.AppDirect, Version=1.10.2.0, Culture=neutral, PublicKeyToken=null
// MVID: 99E0A220-C243-49E2-8282-9AD243A16957
// Assembly location: C:\Users\lorenzo\Desktop\Laser.Orchard.AppDirect.dll

using Orchard.Data.Conventions;
using System;

namespace Laser.Orchard.AppDirect.Models
{
  public class LogEventsRecord
  {
    public virtual int Id { get; set; }

    public virtual DateTime TimeStamp { get; set; }

    public virtual EventType EventType { get; set; }

    public virtual string Method { get; set; }

    [StringLengthMax]
    public virtual string Log { get; set; }

    public LogEventsRecord(EventType type, string log, string method)
    {
      this.TimeStamp = DateTime.UtcNow;
      this.EventType = type;
      this.Log = log;
      this.Method = method;
    }

    public LogEventsRecord()
    {
    }
  }
}
