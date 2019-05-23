using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Drawing;

namespace IkariamBot
{
    public class HTTP
    {
        public static bool Request(string url, CookieCollection reqcookies, Dictionary<string, string> getParams,
            Dictionary<string, string> postParams, ref CookieCollection respcookies, ref string html, string proxy)
        {
            try 
            {
                if (getParams != null)
                {
                    url += "?";
                    foreach (var pair in getParams)
                    {
                        url += pair.Key + "=" + pair.Value + "&";
                    }
                }
                url = url.Substring(0, url.Length-1);

                var req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "POST";
                req.ContentLength = 0;
                req.ContentType = "application/x-www-form-urlencoded";
                req.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.115 Safari/537.36";
                req.Accept = "*/*";
                req.Timeout = 15*1000;

                if(proxy != null)
                {
                    var s = proxy.Split(':');
                    req.Proxy = new WebProxy(s[0], int.Parse(s[1]));
                }

                if (reqcookies != null)
                {
                    req.CookieContainer = new CookieContainer();
                    req.CookieContainer.Add(reqcookies);
                }
                else
                    req.CookieContainer = new CookieContainer();

                if (postParams != null)
                {
                    string post = "";
                    post += "?";
                    foreach (var pair in postParams)
                    {
                        post += pair.Key + "=" + pair.Value + "&";
                    }
                    post = post.Substring(0, post.Length - 1);
                    byte[] buffer = Encoding.ASCII.GetBytes(post);

                    req.ContentLength = buffer.Length;
                    var reqstream = req.GetRequestStream();
                    reqstream.Write(buffer, 0, buffer.Length);
                    reqstream.Close();
                }

                var resp = (HttpWebResponse)req.GetResponse();
                var stream = resp.GetResponseStream();

                var reader = new StreamReader(stream);
                html = reader.ReadToEnd();

                respcookies = resp.Cookies;

                reader.Close();
                stream.Close();
                resp.Close();

                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public static bool RequestImage(string url, CookieCollection reqcookies, Dictionary<string, string> getParams,
            Dictionary<string, string> postParams, ref CookieCollection respcookies, ref Image img, string proxy)
        {
            try
            {
                if (getParams != null)
                {
                    url += "?";
                    foreach (var pair in getParams)
                    {
                        url += pair.Key + "=" + pair.Value + "&";
                    }
                }
                url = url.Substring(0, url.Length - 1);

                var req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "POST";
                req.ContentLength = 0;
                req.ContentType = "application/x-www-form-urlencoded";
                req.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.115 Safari/537.36";
                req.Accept = "*/*";
                req.Timeout = 15 * 1000;

                if (proxy != null)
                {
                    var s = proxy.Split(':');
                    req.Proxy = new WebProxy(s[0], int.Parse(s[1]));
                }

                if (reqcookies != null)
                {
                    req.CookieContainer = new CookieContainer();
                    req.CookieContainer.Add(reqcookies);
                }
                else
                    req.CookieContainer = new CookieContainer();

                if (postParams != null)
                {
                    string post = "";
                    post += "?";
                    foreach (var pair in postParams)
                    {
                        post += pair.Key + "=" + pair.Value + "&";
                    }
                    post = post.Substring(0, post.Length - 1);
                    byte[] buffer = Encoding.ASCII.GetBytes(post);

                    req.ContentLength = buffer.Length;
                    var reqstream = req.GetRequestStream();
                    reqstream.Write(buffer, 0, buffer.Length);
                    reqstream.Close();
                }

                var resp = (HttpWebResponse)req.GetResponse();
                var stream = resp.GetResponseStream();

                img = Image.FromStream(stream);

                respcookies = resp.Cookies;

                stream.Close();
                resp.Close();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
