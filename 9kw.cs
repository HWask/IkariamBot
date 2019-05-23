using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using CodeScales.Http;//CodeScales.com
using CodeScales.Http.Entity;
using CodeScales.Http.Methods;
using CodeScales.Http.Entity.Mime;
using CodeScales.Http.Common;

namespace IkariamBot
{
    public static class _9kw
    {
        public static string apikey = "";

        public static bool isServerOnline()
        {
            string servercheck = get_9kw_api("userservercheck");
            Match queue = Regex.Match(servercheck, @"(?<=queue=)\d+");

            int ret;
            return int.TryParse(queue.Value, out ret) == true;
        }

        public static string guthaben()
        {
            return get_9kw_api("usercaptchaguthaben");
        }

        public static string captchaEinsenden(string fileName)
        {
            return get_9kw_api_upload(fileName);
        }

        public static string getCaptchaSolution(string NewCaptchaID)
        {
            Thread.Sleep(30 * 1000);
            string sol = get_9kw_api("usercaptchacorrectdata&id=" + NewCaptchaID).ToUpper();
            int attempts = 1;
            while(sol == "" && attempts < 6)
            {
                sol = get_9kw_api("usercaptchacorrectdata&id=" + NewCaptchaID).ToUpper();
                Thread.Sleep(30 * 1000);
                attempts++;
            }
            return sol;
        }

        public static string captchaIsCorrect(string NewCaptchaID)
        {
            return get_9kw_api("usercaptchacorrectback&id=" + NewCaptchaID + "&correct=1");
        }

        public static string captchaInCorrect(string NewCaptchaID)
        {
            return get_9kw_api("usercaptchacorrectback&id=" + NewCaptchaID + "&correct=2");
        }

        public static string get_9kw_api(string data)
        {
            return get_9kw_http("http://www.9kw.eu/index.cgi?source=csapi&debug=false&apikey=" + _9kw.apikey + "&action=" + data);
        }

        public static string get_9kw_api_upload(string data)
        {
            HttpClient client = new HttpClient();
            HttpPost postMethod = new HttpPost(new Uri("http://www.9kw.eu/index.cgi"));

            MultipartEntity multipartEntity = new MultipartEntity();
            postMethod.Entity = multipartEntity;

            StringBody stringBody = new StringBody(Encoding.UTF8, "apikey", _9kw.apikey);
            multipartEntity.AddBody(stringBody);

            StringBody stringBody3 = new StringBody(Encoding.UTF8, "source", "csapi");
            multipartEntity.AddBody(stringBody3);

            StringBody stringBody2 = new StringBody(Encoding.UTF8, "action", "usercaptchaupload");
            multipartEntity.AddBody(stringBody2);

            FileInfo fileInfo = new FileInfo(@data);
            FileBody fileBody = new FileBody("file-upload-01", data, fileInfo);
            multipartEntity.AddBody(fileBody);

            HttpResponse response = client.Execute(postMethod);
            return EntityUtils.ToString(response.Entity);
        }

        public static string get_9kw_http(string url)
        {
            HttpClient httpClient = new HttpClient();
            HttpGet httpGet = new HttpGet(new Uri(url));
            HttpResponse httpResponse = httpClient.Execute(httpGet);//httpResponse.ResponseCode
            return EntityUtils.ToString(httpResponse.Entity);
        }
    }
}
