﻿using Laser.Orchard.UserProfiler.ViewModels;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserProfiler.Service {
    public interface IUserProfilingService : IDependency {
        Dictionary<string, int> UpdateProfile(int UserId, string text, TextSourceTypeOptions sourceType, int count);
        Dictionary<string, int> UpdateProfile(int UserId, UpdateVM update);
    }
}