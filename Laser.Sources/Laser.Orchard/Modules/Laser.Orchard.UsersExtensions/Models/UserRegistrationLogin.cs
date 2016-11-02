using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using Laser.Orchard.Policy.Models;
using Orchard.ContentManagement;

namespace Laser.Orchard.UsersExtensions.Models {
    public class UserRegistration {
        public UserRegistration() {
            PolicyAnswers = new List<UserPolicyAnswer>();
        }

        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string PasswordAnswer { get; set; }
        public string PasswordQuestion { get; set; }
        public string Culture { get; set; }
        public List<UserPolicyAnswer> PolicyAnswers { get; set; }
    }

    public class UserLogin {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class UserPolicyAnswer {
        public int PolicyId { get; set; }
        public bool UserHaveToAccept { get; set; }
        public bool PolicyAnswer { get; set; }
    }

    public class UserPolicyAnswerWithContent : UserPolicyAnswer {
        [Bindable(false)] // never bind from view
        public ContentItem PolicyText { get; set; }
    }
}