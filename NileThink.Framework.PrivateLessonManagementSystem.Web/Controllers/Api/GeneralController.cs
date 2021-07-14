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

namespace NileThink.Framework.PrivateLessonManagementSystem.Web.Controllers.Api
{
    [RoutePrefix("api/v1/General")]
    public class GeneralController : ApiController
    {
        SettingBLL _settings = new SettingBLL();
        EducationLevelBLL _EducationLevelBLL = new EducationLevelBLL();
        SpecializationBLL _SpecializationBLL = new SpecializationBLL();
        CertificateTypesBLL _certificateTypesBLL = new CertificateTypesBLL();
        MaterialsBLL _materialBll = new MaterialsBLL();
        ComplainTypesBLL _complainTypes = new ComplainTypesBLL();
        [AllowAnonymous]
        [Route("GetAllCity")]
        [ResponseType(typeof(List<string>))]
        [HttpGet]
        // [CacheFilterAttribute(Duration = 60)]
        public IHttpActionResult GetAllCity()
        {
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
            var Levels = _EducationLevelBLL.GetEducationLevels().Select(vm => new EducationLevel()
            {
                id = vm.id,
                name = vm.name,
                subLevels=vm.subLevels.Select(x => new EducationSuBLevel()
                {
                    id = x.id,
                    name = x. name,
                   


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
        //[CacheFilterAttribute(Duration = 60)]
        public IHttpActionResult GetPages()
        {
            
           
                term_policy about = new term_policy()
                {
                    about = _settings.AboutUs(),
                   
                };
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true, about));
            

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
            var Specializations = _SpecializationBLL.GetSpecializations().Select(vm => new EducationLevel()
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
            try
            {
                if (string.IsNullOrEmpty(image.imageBase)) return this.ResponseBadRequest(new ResponseViewModel(HttpStatusCode.BadRequest, null, false, "من فضلك ادخل ملف "));
                if (string.IsNullOrEmpty(image.imageExtenstion)) return this.ResponseBadRequest(new ResponseViewModel(HttpStatusCode.BadRequest, null, false, "من فضلك ادخل بيانات صحيحه "));
                var bytes = Convert.FromBase64String(image.imageBase);// a.base64image

                string filedir = System.Web.Hosting.HostingEnvironment.MapPath( "~/resources/users");
                
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
                return this.ResponseBadRequest(new ResponseViewModel(HttpStatusCode.BadRequest, null, false, "ملف غير صحيح"));
            }
        }
        [AllowAnonymous]
        [Route("UploadImageFile")]
        [HttpPost]
        public IHttpActionResult UploadImageFile()
        {
            string fileName, path,folderName;
            folderName = "/resources/users";
            var file = HttpContext.Current.Request.Files.Count > 0 ?
                HttpContext.Current.Request.Files[0] : null;
            var newGuid = Guid.NewGuid();
            var filedir = System.Web.Hosting.HostingEnvironment.MapPath("~"+ folderName);
            if (!Directory.Exists(filedir))
            { //check if the folder exists;
                Directory.CreateDirectory(filedir);
            }
            if (file != null && file.ContentLength > 0)
            {
                 var Exten = Path.GetExtension(file.FileName);
                 fileName = Path.GetFileName(string.Format("{0}{1}",newGuid,Exten));

                 path = Path.Combine(
                    System.Web.Hosting.HostingEnvironment.MapPath("~"+ folderName),
                    fileName
                );

                file.SaveAs(path);
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, null, true, fileName));
            }

            return this.ResponseBadRequest(new ResponseViewModel(HttpStatusCode.BadRequest, null, false, "ملف غير صحيح"));
        }

    }
}