using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace NileThink.Framework.PrivateLessonManagementSystem.Web.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string fullname { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string gender { get; set; }
        public string city { get; set; }
        public string region { get; set; }
        public DateTime? dob { get; set; }
        public string country { get; set; }
        public int? education_level { get; set; }
        public string education_level_text { get; set; }
        public int? status { get; set; }
        public DateTime? cdate { get; set; }
        public DateTime? last_login { get; set; }
        public string address { get; set; }
        public bool? allow_notify { get; set; }
        public string ip { get; set; }
        public string photo { get; set; }
        public string details { get; set; }
        public string certificate { get; set; }
        public string university { get; set; }
        public int? specialization { get; set; }
        public int? branch_specialization { get; set; }
        public string teaching_mechanism { get; set; }
        public string online_cost { get; set; }
        public string site_cost { get; set; }
        public string graduate_year { get; set; }
        public double? total_income { get; set; }
        public double? total_remain { get; set; }
        public int? total_course { get; set; }
        public string fcm { get; set; }
        public int? is_complete { get; set; }
        public int? absher { get; set; }
        public string absher_no { get; set; }
        public string user_state { get; set; }
        public bool? online { get; set; }
        public string certificate_photo { get; set; }
        public double? user_site_rate { get; set; } = 0;

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}