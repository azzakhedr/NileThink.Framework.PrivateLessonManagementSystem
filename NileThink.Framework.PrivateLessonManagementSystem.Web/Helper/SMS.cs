using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
namespace NileThink.Framework.PrivateLessonManagementSystem.Web.Helper
{
    public class SMS
    {
        string USER_NAME = "#";//"khalidhmmad";
        string PASSWRD = "#";//"khalid654321";
        string SENDER = "#";
        //public string RECIEVER = "966530408855";
        // public string MESSAGE = "";
        string UNICOD = "#";
        string response = "";
        string url = "http://sms.malath.net.sa/httpSmsProvider.aspx";
        string ReturnMessage = "";

        //public string sendmessage(string RECIEVER, string MESSAGE)
        //{
        //    //int temp = '0';
        //    try
        //    {
        //        //return "";
        //        //username = "";
        //        //password = "";
        //       string username = "966554442684";
        //       string password = "test123";
        //       string sender = "Rayyeh";
        //        HttpWebRequest req = (HttpWebRequest)
        //        WebRequest.Create("http://www.mobily.ws/api/msgSend.php");
        //        req.Method = "POST";
        //        req.ContentType = "application/x-www-form-urlencoded";
        //        string postData = "mobile=" + username + "&password=" + password + "&numbers=" + RECIEVER + "&sender=" + sender + "&msg=" + ConvertToUnicode(MESSAGE) + "&applicationType=24&msgId=0";
        //        req.ContentLength = postData.Length;

        //        StreamWriter stOut = new
        //        StreamWriter(req.GetRequestStream(),
        //        System.Text.Encoding.ASCII);
        //        stOut.Write(postData);
        //        stOut.Close();
        //        // Do the request to get the response
        //        string strResponse;
        //        StreamReader stIn = new StreamReader(req.GetResponse().GetResponseStream());
        //        strResponse = stIn.ReadToEnd();
        //        stIn.Close();
        //        return strResponse;
        //    }
        //    catch (Exception ex)
        //    {
        //        return "";
        //    }

        //}

        //private string ConvertToUnicode(string val)
        //{
        //    string msg2 = string.Empty;

        //    for (int i = 0; i < val.Length; i++)
        //    {
        //        msg2 += convertToUnicode(System.Convert.ToChar(val.Substring(i, 1)));
        //    }

        //    return msg2;
        //}

        //private string convertToUnicode(char ch)
        //{
        //    System.Text.UnicodeEncoding class1 = new System.Text.UnicodeEncoding();
        //    byte[] msg = class1.GetBytes(System.Convert.ToString(ch));

        //    return fourDigits(msg[1] + msg[0].ToString("X"));
        //}

        //private string fourDigits(string val)
        //{
        //    string result = string.Empty;

        //    switch (val.Length)
        //    {
        //        case 1: result = "000" + val; break;
        //        case 2: result = "00" + val; break;
        //        case 3: result = "0" + val; break;
        //        case 4: result = val; break;
        //    }

        //    return result;
        //}

        public string sendmessage(string RECIEVER, string MESSAGE)
        {
            USER_NAME = "";// Functions.getOption("mobile_user");
            PASSWRD = "";// Functions.getOption("mobile_password");
            string query = url + "?username=" + USER_NAME + "&password=" + PASSWRD + "&mobile=" +
           RECIEVER + "&unicode=" + UNICOD + "&message=" + Unicode(MESSAGE) + "&sender=" + SENDER;
            try
            {
                HttpWebRequest httpwc = (HttpWebRequest)WebRequest.Create(query);
                HttpWebResponse httpwr = (HttpWebResponse)httpwc.GetResponse();
                StreamReader streader = new StreamReader(httpwr.GetResponseStream());
                response = streader.ReadLine();
                streader.Close();
                // Label9.Text = response;
                switch (response)
                {
                    case "0":
                        ReturnMessage = "تم الارسال بنجاح";
                        break;
                    case "101":
                        ReturnMessage = "معاملات خاطئة";
                        break;
                    case "104":
                        ReturnMessage = "قد يكون اسم المستخدم او كلمة الملرور غير صحيح ";
                        break;
                    case "105":
                        ReturnMessage = "بطاقة غير متوفرة";
                        break;
                    case "106":
                        ReturnMessage = "unicode خاطئ";
                        break;
                    case "107":
                        ReturnMessage = "اسم المرسل غير مسموح";
                        break;
                    case "108":
                        ReturnMessage = "اسم المرسل غير موجود";
                        break;
                    default:
                        break;
                }

                return ReturnMessage;
            }
            catch (Exception ex)
            {

                return "خطأ في الارسال ";
            }

        }
        public string getpoints()
        {
            string url = "http://www.sms.malath.net.sa/api/getBalance.aspx";
            string str = url + "?username=" + USER_NAME + "&password=" + PASSWRD;
            try
            {
                HttpWebRequest httpwc = (HttpWebRequest)WebRequest.Create(str);
                HttpWebResponse httpwr = (HttpWebResponse)httpwc.GetResponse();
                StreamReader streader = new StreamReader(httpwr.GetResponseStream());
                string balancestr = streader.ReadToEnd().ToString();
                streader.Close();
                httpwr.Close();
                return balancestr;
            }
            catch (Exception EX) { return "خطأ في الارسال "; }
        }
        private string Unicode(string messg)
        {
            string arabicmessage = string.Empty;

            for (int i = 0; i < messg.Length; i++)
            {
                arabicmessage += ToChar(System.Convert.ToChar(messg.Substring(i, 1)));
            }

            return arabicmessage;
        }

        private string ToChar(char ch)
        {
            System.Text.UnicodeEncoding class1 = new System.Text.UnicodeEncoding();
            byte[] msg = class1.GetBytes(System.Convert.ToString(ch));

            return ToHexaDecimal(msg[1] + msg[0].ToString("X"));
        }

        private string ToHexaDecimal(string messg)
        {
            string temp = string.Empty;

            switch (messg.Length)
            {
                case 1: temp = "000" + messg; break;
                case 2: temp = "00" + messg; break;
                case 3: temp = "0" + messg; break;
                case 4: temp = messg; break;
            }

            return temp;
        }
    }
}