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
using PrivateLessonMS.Resources;

namespace NileThink.Framework.PrivateLessonManagementSystem.Web.Controllers.Api
{
    [RoutePrefix("api/v1/BankAccountData")]
    public class BankAccountDataController : BaseController
    {



        BankAccountBLL _bankAccountBll = new BankAccountBLL();
        TeacherRequestRefunedBll _teacherRequestRefunedBll = new TeacherRequestRefunedBll();
        [AllowAnonymous]


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
        [ResponseType(typeof(TeacherBankAccount))]
        public IHttpActionResult GetTeacherBankAccountData(int TeacherId)
        {
            string Lang = lang;
            try
            {
                var obj = _bankAccountBll.GetTeacherBankBalanceDataApi(TeacherId);
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




        [Authorize]
        [Route("GetWalletTransactionStudent")]
        [HttpGet]
        [ResponseType(typeof(List<sp_wallet_transaction_Result>))]
        public IHttpActionResult GetWalletTransactionStudent(int StudentId)
        {
            string Lang = lang;
            try
            {
                List<sp_wallet_transaction_Result> obj = _teacherRequestRefunedBll.GetWalletTransaction(0, StudentId, 0);
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, "", true, obj));

            }
            catch (Exception ex)
            {
                return this.ResponseOK(new ResponseViewModel(HttpStatusCode.OK, Resource.ErrorOccure, false, null));
            }

        }

    }
}