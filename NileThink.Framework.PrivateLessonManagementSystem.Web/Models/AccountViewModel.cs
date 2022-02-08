using Newtonsoft.Json;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.ViewModels;
using NileThink.Framework.PrivateLessonManagementSystem.DAL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NileThink.Framework.PrivateLessonManagementSystem.Web.Models
{
    public class AccountViewModel
    {
    }
    public class ForgotPasswordViewModel
    {
        public string email { get; set; }
    }
    public class GeneralUser
    {
        public int id { get; set; }
        public string fullName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string gender { get; set; }
        public string city { get; set; }
        public string region { get; set; }
        public DateTime? birthDate { get; set; }
        [JsonIgnore]
        public string country { get; set; }
        public int? status { get; set; }
        public string photo { get; set; }
        public string mobile { get; set; }
        public string email { get; set; }
        public bool? isComplete { get; set; }
        public string nationalId { get; set; }


    }
    public class term_policy
    {

        public string about { get; set; }
        public string TermsAndCondition { get; set; }
        public string Mobile { get; set; }
    }
    public class GeneralUserResponseModel
    {
        public int id { get; set; }
        public string fullName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public float? rating { get; set; }
        public string gender { get; set; }
        public string city { get; set; }
        public string district { get; set; }
        public string birthDate { get; set; }
        public string AbsherUserId { get; set; }
 
        public string country { get; set; }
        public int? status { get; set; }
        public string photo { get; set; }
        public string mobile { get; set; }
        public string email { get; set; }
        public bool? isComplete { get; set; }
        public string nationalId { get; set; }
        public string postalCode { get; set; }
        public string streetNo { get; set; }
        public int teachingMechanism { get; set; }
        public List<EducationLevel> educationLevel { get; set; }


    }
    public class AvailableTeacher
    {


        public string firstName { get; set; }
        public string lastName { get; set; }
        public string city { get; set; }

        public int? rate { get; set; }


        public string gender { get; set; }


        public int? pageNo { get; set; }
        public int? recordsPerPage { get; set; }


        public string specialization { get; set; }
        public string branchSpecialization { get; set; }

        public Nullable<bool> online { get; set; }
        public string teachingMechanism { get; set; }


    }
    public class teacherAvailability
    {
        public int teacherId { get; set; }
        public string dayOfWeek { get; set; }
        public string fromTime { get; set; }
        public string toTime { get; set; }
        //public int FromMinutes { get; set; }
        //public int ToMinutes { get; set; }
        //public string FromTime { get; set; }
        //public string ToTime { get; set; }
    }
    public class EducationLevel
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<EducationSuBLevel> subLevels { get; set; }
    }
    public class GeneralList
    {
        public int id { get; set; }
        public string name { get; set; }

    }
    public class EducationSuBLevel
    {
        public int id { get; set; }
        public string name { get; set; }

    }
    public class Materials
    {
        public int id { get; set; }
        public string name { get; set; }
    }
    public class BookedTimes
    {
        [JsonIgnore]
        List<BookedDates> bookedDates { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
    }
    public class BookedDates
    {
        [JsonIgnore]
        public DateTime BookDate { get; set; }
        public string startDate { get; set; }

        public int dayOfWeek { get; set; }
        public List<BookedDayTimes> bookedDayTimes { get; set; }

    }
    public class BookedDayTimes
    {

        public string fromTime { get; set; }
        public string toTime { get; set; }
    }
    public class TeacherDetailsResponse
    {
        public int teacherId { get; set; }
        public int id { get; set; }
        public string postalCode { get; set; }
        public string streetNo { get; set; }
        public string fullName { get; set; }
        public string collage { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string gender { get; set; }
        public string bio { get; set; }
        public string certificate { get; set; }
        public double? onlineCost { get; set; }
        public double? siteCost { get; set; }
        public GeneralList branchSpecializations { get; set; }
        public GeneralList specializations { get; set; }
        public string specialization { get; set; }
        public string branchSpecialization { get; set; }
        public int? totalCourse { get; set; }
        public float? rating { get; set; }
        public string email { get; set; }
        public int teachingMechanism { get; set; }
        [JsonIgnore]
        public string createdDate { get; set; }

        public string photo { get; set; }

        public string country { get; set; }

        public int? absher { get; set; }
        public string absher_no { get; set; }
        public bool? online { get; set; }
        [JsonIgnore]
        public string period { get; set; }
        [JsonIgnore]
        public string material { get; set; }
        public string city { get; set; }
        public string birthDate { get; set; }
        public string mobile { get; set; }
        public string nationalId { get; set; }
        public string district { get; set; }
        public string AbsherUserId { get; set; }
        public bool isComplete { get; set; }
        public int status { get; set; }
        public List<EducationLevel> educationLevel { get; set; }
        public List<teacherAvailability> teacherAvailability { get; set; }
        public List<Materials> teacherMaterials { get; set; }
        public bool IsPackageAvailable { get; set; }
        public List<PackageDataLstVM> PackageLst { get; set; }
        public List<TeacherPackagesVM> TeacherPacks { get; set; }
        public string requireUpdateProfileAbsher { get; set; }
    }
    public class TeacherPackageDetailsResponse
    {

        public bool IsPackageAvailable { get; set; }
        public List<PackageDataLstVM> PackageLst { get; internal set; }
        public List<TeacherPackagesVM> TeacherPacks { get; internal set; }
    }
    public class UploadModel
    {
        public string imageBase { get; set; }
        public string imageExtenstion { get; set; }
    }
    public class NewRequestListViewModel
    {
        public int? EducationLevelId { get; set; }
        public int? SubLevelId { get; set; }
        public string EducationSublevelName { get; set; }
        public string EducationLevelName { get; set; }
        public int? id { get; set; }
        public string createdDate { get; set; }
        public int teacherId { get; set; }
        public int studentId { get; set; }
        /// <summary>
        /// (0 انتظارالدفع ) , 
        /// (1 تم الموافقة على الطلب ) , 
        /// (2 تم تأكيد الطلب والدفع),
        /// (3 تم تأكيد الدفع),
        /// (-1 تم رفض الطلب )
        /// </summary>
        public int statusId { get; set; }
        public string status { get; set; }
        public string teacherName { get; set; }
        public string photo { get; set; }
        public string subject { get; set; }
        public string comment { get; set; }
        public string studentName { get; set; }
        /// <summary>
        /// (0 online ) , 
        /// (1 onsite ) , 
        /// (2 both),
        /// (-1 none )
        /// </summary>

        public int teachingMechanismStatus { get; set; }

        public string teachingMechanism { get; set; }
        public string liveType { get; set; }
        /// <summary>
        /// (video), 
        /// (audio)
        /// </summary>

        // public List<IBan> teacher_bank_accounts { get; set; }

        public string terms { get; set; }

        public int? lessons { get; set; }
        public double onlinCost { get; set; }
        public double siteCost { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }

        public double? amount { get; set; }
        public double? pricePerHour { get; set; }
        public double? totalPrice { get; set; }
        public double? totalHours { get; set; }
        public List<RequestCourceDates> courseDates { get; set; }
        public string AddressName { get;  set; }
    }
    public class RequestCourceDates
    {
        [JsonIgnore]
        public int id { get; set; }

        public int requestId { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }

    }
    public class FileDesc
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }

        public FileDesc(string n, string p, long s)
        {
            Name = n;
            Path = p;
            Size = s;
        }
    }
    public class LessonModel
    {
        public int teacherId { get; set; }
        public int pageno { get; set; }
        public int records { get; set; }
        public DateTime? requestDate { get; set; }
        public DateTime? courseTime { get; set; }
        public string material { get; set; }
        public string studentName { get; set; }
        public int? TeachingMehod { get; set; }

    }
    public class LessonStudentModel
    {
        public int studentId { get; set; }
        public int pageno { get; set; }
        public int records { get; set; }
        public DateTime? requestDate { get; set; }
        public DateTime? courseTime { get; set; }
        public string material { get; set; }
        public string teacherName { get; set; }
        public int? TeachingMehod { get; set; }

    }
    public class CourseModel
    {
        public int? EducationLevelId { get; set; }
        public int? SubLevelId { get; set; }
        public string EducationSublevelName { get; set; }
        public string EducationLevelName { get; set; }
        public int lessonId { get; set; }
        public int requestId { get; set; }
        [JsonIgnore]
        public int requestDateId { get; set; }
        [JsonIgnore]
        public string conferanceWizqUrl { get; set; }
        [JsonProperty("zoomUrl")]
        public string conferanceZoom { get; set; }
        [JsonIgnore]
        public string studentZoom { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public NewRequestListViewModel requestDetails { get; set; }
        public object AddressName { get;  set; }
    }
    public class RateModel
    {

        public int studentId { get; set; }
        [JsonIgnore]
        public int? courseId { get; set; }
        public int? rate { get; set; }
        public int? rateType { get; set; }
        public int? complainId { get; set; }
        public string complain { get; set; }
        public string comment { get; set; }
        public int teacherId { get; set; }
        [JsonIgnore]
        public bool? online { get; set; }

        /// <summary>
        /// Just OutPut
        /// </summary>
        public int? id { get; set; }
        [JsonIgnore]
        public string courseTitle { get; set; }
        /// <summary>
        /// Just OutPut
        /// </summary>
        public string teacherName { get; set; }
        /// <summary>
        /// Just OutPut
        /// </summary>
        public string studentName { get; set; }
        /// <summary>
        /// Just OutPut
        /// </summary>
        public Nullable<System.DateTime> createdDate { get; set; }
        /// <summary>
        /// Just OutPut
        /// </summary>
        public string photo { get; set; }
    }
    public class ChangePasswordBindingModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string oldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string newPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string confirmPassword { get; set; }
    }
    public class LoginBindingModel
    {
        public string userName { get; set; }
        public string password { get; set; }
    }
    public class RegisterUserModel
    {
        public string email { get; set; }
        public string mobile { get; set; }
        public string fullName { get; set; }
        public string password { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }

        public string country { get; set; }

    }
    public class TeacherResponseModel : GeneralUserResponseModel
    {
        public int teacherId { get; set; }
        public string bio { get; set; }
        public string certificate { get; set; }
        public string certificatePhoto { get; set; }
        public string specialization { get; set; }
        public string branchSpecialization { get; set; }

        //public string specialization_obj { get; set; }
        //public string branch_specialization_obj { get; set; }
        /// <summary>
        /// (onsite)
        /// (online)
        /// (both)
        /// </summary>
        public int teachingMechanism { get; set; }
        public string university { get; set; }

        public double? onlineCost { get; set; }
        public double? siteCost { get; set; }
        public List<teacherAvailability> teacherTimes { get; set; }
        public List<Materials> teacherMaterials { get; set; }
        public int? rate { get; set; }
        public int? absher { get; set; }
    }
    public class TeacherTimes
    {
        public int? dayNumber { get; set; }
        public string from { get; set; }
        public string to { get; set; }
    }
    public class TeacherRegisterResponseModel : TeacherResponseModel
    {
        public int? expired { get; set; }
        public string token { get; set; }
        public string fcm { get; set; }


    }
    public class RegisterResponseModel
    {
        public string token { get; set; }
        public int expired { get; set; }
        public int id { get; set; }
        public string fullName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string mobile { get; set; }
        public bool? allowNotify { get; set; }
        public string role { get; set; }
        public bool? isComplete { get; set; }
        public int? absherNo { get; set; }
        public bool? online { get; set; }
        public string photo { get; set; }
        public string country { get; set; }
        public string requireUpdateProfileAbsher { get; set; }
        public string userId { get; set; }
        public string absher_id { get; internal set; }
        public string AbsherUserId { get; set; }
        //public string absher_no { get; set; }
    }
    public class RegisterEmail
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string type { get; set; }
        public string message { get; set; }
        public string createdDate { get; set; }
        public string to { get; set; }
    }
    public class ChangeUserModel
    {
        public string to { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }
    public class EmailContract
    {
        public string to { get; set; }
        public string title { get; set; }
        public string from { get; set; }
        public string msg { get; set; }
        public string createdDate { get; set; }
        public string period { get; set; }
        public DateTime? startDate { get; set; }
        public TimeSpan? startTime { get; set; }
        public string teachingMechanism { get; set; }
        public string material { get; set; }
        public string bio { get; set; }
        public string isTest { get; set; }
        public string cost { get; set; }
        public int? status { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }

    }
    public class EmailInviteSender
    {
        public string to { get; set; }
        public string title { get; set; }
        public string from { get; set; }
        public string message { get; set; }
        public string createdDate { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
    }
    public class InviteResponse
    {
        public string checkoutId { set; get; }
    }
    public class PaymentModelRequest
    {
        public int? requestId { get; set; }
        public float? amount { get; set; } = 20;
        public int? gateway { get; set; } = 0;
        public string customerEmail { get; set; }
        public string billingStreet { get; set; }
        public string billingCity { get; set; }
        public string billingState { get; set; }
        public string billingPostCode { get; set; }
        public string customerFirstName { get; set; }
        public string customerSurname { get; set; }
    }
    public class PaymentModelResult
    {
        public string description { get; set; }

        public string buildNumber { get; set; }
        public string timestamp { get; set; }
        public string ndc { get; set; }
        public string id { get; set; }
        public PaymentResult Result { get; set; }

        public string checkout_id { get; set; }
        public string code { get; set; }
        [JsonIgnore]
        public int? status { get; set; }
    }
    public class PaymentResult
    {
        public string code { get; set; }
        public string description { get; set; }

    }
    public class WebinarModel
    {

        public string topic { get; set; }
        public int type { get; set; }
        public DateTime start_time { get; set; }
        public string duration { get; set; }
        public string timezone { get; set; }
        public string password { get; set; }
        public string agenda { get; set; }
        public Recurrence recurrence { get; set; }
        public Settings settings { get; set; }


    }
    public class Recurrence
    {
        public int type { get; set; }
        public int repeat_interval { get; set; }
        public DateTime end_date_time { get; set; }
    }

    public class Settings
    {
        public string host_video { get; set; }
        public string panelists_video { get; set; }
        public string practice_session { get; set; }
        public string hd_video { get; set; }
        public int approval_type { get; set; }
        public int registration_type { get; set; }
        public string audio { get; set; }
        public string auto_recording { get; set; }
        public string enforce_login { get; set; }
        public string close_registration { get; set; }
        public string show_share_button { get; set; }
        public string allow_multiple_devices { get; set; }
        public string registrants_email_notification { get; set; }
    }
    public class ZoomWebinar
    {
        public string uuid { get; set; }
        public long id { get; set; }
        public string host_id { get; set; }
        public string host_email { get; set; }
        public string topic { get; set; }
        public int type { get; set; }
        public DateTime start_time { get; set; }
        public int duration { get; set; }
        public string timezone { get; set; }
        public string agenda { get; set; }
        public DateTime created_at { get; set; }
        public string start_url { get; set; }
        public string join_url { get; set; }
        public string registration_url { get; set; }
        public Settings settings { get; set; }
    }


}