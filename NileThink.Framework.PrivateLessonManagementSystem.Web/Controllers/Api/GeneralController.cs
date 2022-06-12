using Newtonsoft.Json;
using NileThink.Framework.PrivateLessonManagementSystem.Web.Models;
using NileThink.Framework.PrivateLessonManagementSystem.Web.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.BussinessLayer;
using System.IO;
using NileThink.Framework.PrivateLessonManagementSystem.DAL.Models;
using NileThink.Framework.PrivateLessonManagementSystem.BLL.ViewModels;
using Microsoft.AspNet.Identity;
using PrivateLessonMS.Resources;
using Twilio;
using Twilio.Rest.Api.V2010.Account;



namespace NileThink.Framework.PrivateLessonManagementSystem.Web.Controllers.Api
{
    [RoutePrefix("api/v1/General")]
    public class GeneralController : BaseController
    {
        SettingBLL _settings = new SettingBLL();
        EducationLevelBLL _EducationLevelBLL = new EducationLevelBLL();
        SpecializationBLL _SpecializationBLL = new SpecializationBLL();
        CertificateTypesBLL _certificateTypesBLL = new CertificateTypesBLL();
        MaterialsBLL _materialBll = new MaterialsBLL();
        ComplainTypesBLL _complainTypes = new ComplainTypesBLL();
        BankAccountBLL _bankAccountBll = new BankAccountBLL();
        TeacherRequestRefunedBll _teacherRequestRefunedBll = new TeacherRequestRefunedBll();
        AdvertiseBannersBLL _adBannersBll = new AdvertiseBannersBLL();

        EducationLevelTreeBLL _educationLevelTreeBll = new EducationLevelTreeBLL();

        [AllowAnonymous]
        [Route("GetAllCity")]
        [ResponseType(typeof(List<string>))]
        [HttpGet]
        // [CacheFilterAttribute(Duration = 60)]
        public IHttpActionResult GetAllCity()
        {
            string Lang = lang;
            string file = HttpContext.Current.Server.MapPath("~/JsonData/saudi_regions.json");
            //deserialize JSON from file  
            string Json = System.IO.File.ReadAllText(file);
            List<Cities> list = JsonConvert.DeserializeObject<List<Cities>>(Json);
            var items = list.OrderBy(o => o.city_name).Select(s => s.city_name).Distinct().ToList();
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true, items));
        }
        /// <summary>
        ///    طلب الأحياء لمدينة محددة.
        ///    من خلال تمرير المدينة تطلب احيا السعودية 
        /// </summary>
        [AllowAnonymous]
        [Route("GetDistricts")]
        [ResponseType(typeof(List<string>))]
        [HttpGet]
        // [CacheFilterAttribute(Duration = 60)]
        public IHttpActionResult GetDistricts(string city)
        {
            string Lang = lang;
            string file = HttpContext.Current.Server.MapPath("~/JsonData/saudi_regions.json");
            //deserialize JSON from file  
            string Json = System.IO.File.ReadAllText(file);
            List<Cities> list = JsonConvert.DeserializeObject<List<Cities>>(Json);
            var items = list.Where(w => w.city_name == city).OrderBy(o => o.district_name).Select(s => s.district_name).ToList();
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true, items));
        }


        [Route("GetEducationlevels")]

        [HttpGet]
        // [CacheFilterAttribute(Duration = 60)]
        public IHttpActionResult GetEducationlevels()
        {
            string Lang = lang;
            var Levels = _EducationLevelBLL.GetEducationLevels().Select(vm => new Models.EducationLevel()
            {
                id = vm.id,
                name = vm.name,
                subLevels = vm.subLevels.Select(x => new EducationSuBLevel()
                {
                    id = x.id,
                    name = x.name,



                }).ToList()
            }).ToList();
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true, Levels));
        }
        /// <summary>
        ///    المواد العليمة .
        /// </summary>
        [Route("GetMaterials")]
        [HttpGet]
        // [CacheFilterAttribute(Duration = 60)]
        public IHttpActionResult GetMaterials()
        {
            string Lang = lang;
            var Materials = _materialBll.GetAllMatreials().Select(vm => new Materials()
            {
                id = vm.id,
                name = vm.name
            }).ToList();
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true, Materials));
        }

        [Route("GetPages")]
        [ResponseType(typeof(term_policy))]
        [HttpGet]
        ////   [CacheFilterAttribute(Duration = 60)]
        public IHttpActionResult GetPages()
        {
            string Lang = lang;
            //return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true, "hhh"));

            term_policy about1 = new term_policy()
            {
                about = _settings.AboutUs(),
                TermsAndCondition = _settings.term(),
                Mobile = _settings.mobile()
            };
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true, about1));


        }
        /// <summary>
        ///    التخصصات الأساسية.
        /// </summary>
        [Route("GetSpecializations")]
        [ResponseType(typeof(List<string>))]
        [HttpGet]
        // [CacheFilterAttribute(Duration = 60)]
        public IHttpActionResult GetSpecializations()
        {
            string Lang = lang;
            var Specializations = _SpecializationBLL.GetSpecializations().Select(vm => new Models.EducationLevel()
            {
                id = vm.id,
                name = vm.name
            }).ToList();
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true, Specializations));
        }
        /// <summary>
        ///    التخصصات الفرعية.
        /// </summary>
        [Route("GetBranchSpecializations")]
        [ResponseType(typeof(List<string>))]
        [HttpGet]
        // [CacheFilterAttribute(Duration = 60)]
        public IHttpActionResult GetBranchSpecializations(int ID)
        {
            string Lang = lang;
            var BSpecializations = _SpecializationBLL.GetBranchSpecializations(ID).Select(vm => new GeneralList()
            {
                id = vm.id,
                name = vm.name
            }).ToList();
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true, BSpecializations));
        }
        /// <summary>
        ///    نوع الشهادة.
        /// </summary>
        [Route("GetCertificateTypes")]
        [ResponseType(typeof(List<string>))]
        [HttpGet]
        // [CacheFilterAttribute(Duration = 60)]
        public IHttpActionResult GetCertificateTypes()
        {
            string Lang = lang;
            var Specializations = _certificateTypesBLL.GetCertificateTypes().Select(vm => new GeneralList()
            {
                id = vm.id,
                name = vm.name
            }).ToList();
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true, Specializations));
        }
        [Route("GetComplainTypes")]
        [ResponseType(typeof(List<string>))]
        [HttpGet]
        // [CacheFilterAttribute(Duration = 60)]
        public IHttpActionResult GetComplainTypes()
        {
            string Lang = lang;
            var ComplainTypes = _complainTypes.GetComplainTypes().Select(vm => new GeneralList()
            {
                id = vm.id,
                name = vm.name
            }).ToList();
            return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true, ComplainTypes));
        }

        [AllowAnonymous]
        [Route("UploadImage")]
        [HttpPost]
        public IHttpActionResult UploadImage(UploadModel image)
        {
            string Lang = lang;
            try
            {
                if (string.IsNullOrEmpty(image.imageBase)) return this.ResponseBadRequest(new ResponseViewModel(HttpStatusCode.BadRequest, null, false, Resource.IncorrectFile));
                if (string.IsNullOrEmpty(image.imageExtenstion)) return this.ResponseBadRequest(new ResponseViewModel(HttpStatusCode.BadRequest, null, false, Resource.IncorrectData));
                var bytes = Convert.FromBase64String(image.imageBase);// a.base64image

                string filedir = System.Web.Hosting.HostingEnvironment.MapPath("~/resources/users");

                var newGuid = Guid.NewGuid();

                if (!Directory.Exists(filedir))
                { //check if the folder exists;
                    Directory.CreateDirectory(filedir);
                }
                string filename = string.Format("{0}.{1}", newGuid.ToString(), image.imageExtenstion);
                string file = Path.Combine(filedir, filename);

                //Debug.WriteLine(File.Exists(file));
                if (bytes.Length > 0)
                {
                    using (var stream = new FileStream(file, FileMode.Create))
                    {
                        stream.Write(bytes, 0, bytes.Length);
                        stream.Flush();
                    }
                }
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true, filename));
            }
            catch
            {
                return this.ResponseBadRequest(new ResponseViewModel(HttpStatusCode.BadRequest, null, false, Resource.IncorrectFile));
            }
        }
        [AllowAnonymous]
        [Route("UploadImageFile")]
        [HttpPost]
        public IHttpActionResult UploadImageFile()
        {
            string Lang = lang;
            string fileName, path, folderName;
            folderName = "/resources/users";
            var file = HttpContext.Current.Request.Files.Count > 0 ?
                HttpContext.Current.Request.Files[0] : null;
            var newGuid = Guid.NewGuid();
            var filedir = System.Web.Hosting.HostingEnvironment.MapPath("~" + folderName);
            if (!Directory.Exists(filedir))
            { //check if the folder exists;
                Directory.CreateDirectory(filedir);
            }
            if (file != null && file.ContentLength > 0)
            {
                var Exten = Path.GetExtension(file.FileName);
                fileName = Path.GetFileName(string.Format("{0}{1}", newGuid, Exten));

                path = Path.Combine(
                   System.Web.Hosting.HostingEnvironment.MapPath("~" + folderName),
                   fileName
               );

                file.SaveAs(path);
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true, fileName));
            }

            return this.ResponseBadRequest(new ResponseViewModel(HttpStatusCode.BadRequest, null, false, Resource.IncorrectFile));
        }



        [Authorize]
        [Route("InsertUpdateBankAccount")]
        [HttpPost]
        public IHttpActionResult InsertUpdateBankAccount(TeacherBankAccount data)
        {
            string Lang = lang;
            try
            {
                _bankAccountBll.InsertUpdateTeacherBankAccount(data);
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, null));
            }
            catch
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));
            }


        }

        [Authorize]
        [Route("GetTeacherBankAccountData")]
        [HttpGet]
        [ResponseType(typeof(sp_get_teacher_account_Result))]
        public IHttpActionResult GetTeacherBankAccountData(int TeacherId)
        {
            string Lang = lang;
            try
            {
                var obj = _bankAccountBll.GetTeacherBankAccount(TeacherId);
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, obj));

            }
            catch (Exception ex)
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));
            }

        }
        [AllowAnonymous]
        [Route("GetBankList")]
        [HttpGet]
        [ResponseType(typeof(List<tbl_banks>))]
        public IHttpActionResult GetBankList()
        {
            string Lang = lang;
            try
            {
                List<tbl_banks> obj = _bankAccountBll.GetBanks();
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, obj));

            }
            catch (Exception ex)
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));
            }

        }

        [Authorize]
        [Route("InsertRequestBallanceRefuned")]
        [HttpPost]
        public IHttpActionResult InsertRequestBallanceRefuned(TeacherReqData data)
        {
            string Lang = lang;
            try
            {
                _teacherRequestRefunedBll.InsertTeacherRequestRef(data);
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, null));
            }
            catch
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));
            }


        }


        [Authorize]
        [Route("GetTeacherRequests")]
        [HttpGet]
        [ResponseType(typeof(List<sp_get_teacher_requests_Result>))]
        public IHttpActionResult GetTeacherRequests(int TeacherId)
        {
            string Lang = lang;
            try
            {
                List<sp_get_teacher_requests_Result> obj = _teacherRequestRefunedBll.GetTeacherRequests(TeacherId, 0);
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, obj));

            }
            catch (Exception ex)
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));
            }

        }

        [Authorize]
        [Route("GetWalletTransaction")]
        [HttpGet]
        [ResponseType(typeof(List<sp_wallet_transaction_Result>))]
        public IHttpActionResult GetWalletTransaction(int TeacherId)
        {
            string Lang = lang;
            try
            {
                List<sp_wallet_transaction_Result> obj = _teacherRequestRefunedBll.GetWalletTransaction(TeacherId, 0, 0);
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, obj));

            }
            catch (Exception ex)
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));
            }

        }


        [Route("Contact")]
        [HttpPost]
        public IHttpActionResult Contact(ContactModelCompany data)
        {
            string Lang = lang;
            try
            {
                var aboutEmail = _settings.contact_email();
                if (!String.IsNullOrEmpty(aboutEmail))
                {
                    List<string> emails = new List<string>();
                    emails.Add(aboutEmail);
                    new MailerController().SendMail(emails, data.subject, data.message);
                    return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.EmailSendSuccessfully, true, ""));
                }
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.NoContactMailFound, false, ""));

            }
            catch (Exception ex)
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));
            }

        }



        #region Advertise Banners

        [Route("GetAdvertiseBanners")]
        [HttpPost]
        public IHttpActionResult GetAdvertiseBanners(AdvertiseBannersVM data)
        {
            string Lang = lang;
            try
            {
                var res = _adBannersBll.GetApiAdvertiseBanners(data);
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, res));
            }
            catch (Exception ex)
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));
            }
        }
        #endregion


        #region EducationLevelTree
        [Route("GetEducationLevelTree")]
        [HttpPost]
        public IHttpActionResult GetEducationLevelTree()
        {
            string Lang = lang;
            try
            {
                var res = _educationLevelTreeBll.GetEducationLevel();
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, res));
            }
            catch (Exception ex)
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));
            }
        }
        #endregion



    }
}