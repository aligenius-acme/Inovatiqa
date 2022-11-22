using System.Collections.Generic;
using Newtonsoft.Json;
using Inovatiqa.Core;
namespace Inovatiqa.Web.Models.Common
{
    public class GoogleCaptcha
    {
        public GoogleCaptcha(string response)
        {
            Validate(response);
        }
        public static string Validate(string EncodedResponse)
        {
            return "";
            //var client = new System.Net.WebClient();s
            //var GoogleReply = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", InovatiqaDefaults.CaptchaPrivateKey, EncodedResponse));
            //var captchaResponse = JsonConvert.DeserializeObject<GoogleCaptcha>(GoogleReply, Encodin);
            //return captchaResponse.Success.ToLower();
        }

        [JsonProperty("success")]
        public string Success
        {
            get { return m_Success; }
            set { m_Success = value; }
        }

        public string m_Success;
        [JsonProperty("error-codes")]
        public List<string> ErrorCodes
        {
            get { return m_ErrorCodes; }
            set { m_ErrorCodes = value; }
        }


        public List<string> m_ErrorCodes;
    }
}
