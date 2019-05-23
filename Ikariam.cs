using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading;
using System.Drawing;
using System.IO;

namespace IkariamBot
{
    public class Ikariam
    {
        public enum Server
        {
            Alpha,
            Beta,
            Gamma,
            Delta,
            Epsilon,
            Pi,
            Rho,
            Sigma,
            Boreas
        }

        string[] ServersUrls = {
                                   "s1-de.ikariam.gameforge.com",
                                   "s2-de.ikariam.gameforge.com",
                                   "s3-de.ikariam.gameforge.com",
                                   "s4-de.ikariam.gameforge.com",
                                   "s5-de.ikariam.gameforge.com",
                                   "s16-de.ikariam.gameforge.com",
                                   "s17-de.ikariam.gameforge.com",
                                   "s18-de.ikariam.gameforge.com",
                                   "s27-de.ikariam.gameforge.com"
                               };

        public Form1 mainForm;
        Random rnd = new Random();
        CookieCollection cookies = null;
        public string Proxy = null;
        Server m_Server;
        string m_user, m_pass, cityID, position, token, buildingLevel;
        int min, max;
        bool isfirst = true;

        public Ikariam(string user, string pass, Server server, string sposition, string sbuildingLevel, int mmin, int mmax)
        {
            m_Server = server;
            m_user = user;
            m_pass = pass;
            position = sposition;
            buildingLevel = sbuildingLevel;
            min = mmin;
            max = mmax;
        }

        public void Reset()
        {
            cookies = null;
        }

        public bool Login()
        {
            var getParams = new Dictionary<string, string>{
                { "action", "loginAvatar" },
                { "function", "login" }
            };

             var postParams = new Dictionary<string, string>{
                { "uni_url", ServersUrls[(int)m_Server] },
                { "name", m_user },
                { "password", m_pass },
                { "kid", "" },
                { "pwat_uid", "" },
                { "pwat_checksum", "" },
                { "startPageShown", "1" },
                { "detectedDevice", "1" }
            };

            string html = "";
            if (HTTP.Request("http://" + ServersUrls[(int)m_Server] + "/index.php", null, getParams, postParams, ref cookies, ref html, Proxy == null ? null : Proxy))
            {
                if (cookies.Count < 2)
                    return false;

                if (parse(html))
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        private bool parse(string html)
        {
            var regCityId = new Regex("currentCityId: (.*?),");
            var regToken = new Regex("actionRequest: \"(.*?)\",");
            var matches = regCityId.Match(html);
            if (matches.Success)
                cityID = matches.Groups[1].Value;
            else
                return false;

            var matche = regToken.Match(html);
            if (matche.Success)
                token = matche.Groups[1].Value;
            else
                return false;

            return true;
        }

        public bool isSessionExpired(string html)
        {
            var reg = new Regex("Deine Sitzung ist abgelaufen, bitte logge dich");
            var match = reg.Match(html);
            if (match.Success)
            {
                isfirst = true;
                return true;
            }
            else
                return false;
        }

        private string getNewToken(string html)
        {
            var reg = new Regex("{\"actionRequest\":\"(.*?)\"");
            var m = reg.Match(html);
            if (m.Success)
                return m.Groups[1].Value;
            else
                return null;
        }

        private string getCaptchaURL(string html)
        {
            html = html.Replace("\\", "");
            var reg = new Regex("<img class=\"captchaImage\" src=\"(.*?)\"");
            var m = reg.Match(html);
            if (m.Success)
                return "http://" + ServersUrls[(int)m_Server] + "/index.php" + m.Groups[1].Value;
            else
                return null;
        }

        public bool StartMission(ref string response)
        {
            if (!isfirst)
            {
                var randomDelay = rnd.Next(Math.Min(min, max), Math.Max(min, max) + 1);
                //0s - 2min 30s = 150s
                Thread.Sleep((150 + randomDelay) * 1000);
            }
            isfirst = false;

            var getParams = new Dictionary<string, string>{
                { "action", "PiracyScreen" },
                { "function", "capture" },
                { "buildingLevel", buildingLevel },
                { "view", "pirateFortress" },
                { "cityId", cityID },
                { "position", position },
                { "backgroundView", "city" },
                { "currentCityId", cityID },
                { "templateView", "pirateFortress" },
                { "currentTab", "tabBootyQuest" },
                { "actionRequest", token },
                { "ajax", "1" }
            };

            string html = "";
            CookieCollection t = new CookieCollection();
            if (HTTP.Request("http://" + ServersUrls[(int)m_Server] + "/index.php",
                cookies, getParams, null, ref t, ref html, Proxy == null ? null : Proxy))
            {
                response = html;
                token = getNewToken(html);

                //we have a captcha to solve
                string src = getCaptchaURL(html);
                if(src != null)
                {
                    Log("Captcha muss gelöst werden: " + src);

                    if (_9kw.isServerOnline())
                    {
                        int guthaben = int.Parse(_9kw.guthaben());
                        if (guthaben < 10)
                        {
                            Log("Zu wenig Guthaben: " + guthaben);
                            return false;
                        }
                        else
                        {
                            CookieCollection tempCookie = null;
                            Image img = null;
                            if (HTTP.RequestImage(src, cookies, null, null, ref tempCookie, ref img, Proxy))
                            {
                                saveImage(img);
                                string id = _9kw.captchaEinsenden("captcha.png");
                                //wait 60sec until captcha is solved by someone
                                //Thread.Sleep(30 * 1000);

                                string sol = _9kw.getCaptchaSolution(id);
                                Log("Uebermittelte Captcha Loesung: " + sol);
                                int attempts = 1;
                                while(attempts < 5)
                                {
                                    if(SendCaptchaResponse(sol, ref src))
                                    {
                                        _9kw.captchaIsCorrect(id);
                                        isfirst = true;
                                        return true;
                                    }
                                    else
                                    {
                                        _9kw.captchaInCorrect(id);
                                        if (HTTP.RequestImage(src, cookies, null, null, ref tempCookie, ref img, Proxy))
                                            saveImage(img);
                                        else
                                        {
                                            Log("Konnte Captcha nicht runterladen");
                                            isfirst = true;
                                            return false;
                                        }
                                    }

                                    id = _9kw.captchaEinsenden("captcha.png");
                                    //Thread.Sleep(30 * 1000);
                                    sol = _9kw.getCaptchaSolution(id);
                                    Log("Uebermittelte Captcha Loesung: " + sol);
                                    attempts++;
                                }
                                Log("Captcha wurde nach 5 Versuchen nicht gelöst. Hole neues Captcha");
                                isfirst = true;
                                return false;
                            }
                            else
                            {
                                Log("Konnte Captcha nicht runterladen");
                                isfirst = true;
                                return false;
                            }
                        }
                    }
                    else
                    {
                        Log("9kw Server ist down");
                        isfirst = true;
                        return false;
                    }
                }

                return true;
            }
            else
                return false;
        }

        public bool SendCaptchaResponse(string captchaResponse, ref string captchaUrl)
        {
            var getParams = new Dictionary<string, string>{
                { "action", "PiracyScreen" },
                { "function", "capture" },
                { "buildingLevel", buildingLevel },
                { "view", "pirateFortress" },
                { "cityId", cityID },
                { "position", position },
                { "captcha", captchaResponse },
                { "backgroundView", "city" },
                { "currentCityId", cityID },
                { "templateView", "pirateFortress" },
                { "currentTab", "tabBootyQuest" },
                { "actionRequest", token },
                { "ajax", "1" }
            };

            string html = "";
            CookieCollection t = new CookieCollection();
            if (HTTP.Request("http://" + ServersUrls[(int)m_Server] + "/index.php",
                cookies, getParams, null, ref t, ref html, Proxy == null ? null : Proxy))
            {
                token = getNewToken(html);
                //is captcha solved?
                string src = getCaptchaURL(html);
                if (src != null)
                {
                    Log("Captcha nicht korrekt");
                    captchaUrl = src;
                    return false;
                }

                if(isSessionExpired(html))
                {
                    Log("Sesison expired");
                    return false;
                }
                else
                {
                    Log("Captcha korrekt");
                    return true;
                }
            }
            else
                return false;
        }

        public void saveImage(Image img)
        {
            if (File.Exists("captcha.png"))
                File.Delete("captcha.png");

            img.Save("captcha.png", System.Drawing.Imaging.ImageFormat.Png);
        }

        public void Log(string text)
        {
            mainForm.Invoke((MethodInvoker)delegate
            {
                var dt = DateTime.Now;
                string time = "[" + dt.ToString("HH:mm:ss") + "]: ";
                mainForm.richTextBox1.AppendText(time + text + System.Environment.NewLine);
            });
        }
    }
}
