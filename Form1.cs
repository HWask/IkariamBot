using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace IkariamBot
{
    public partial class Form1 : Form
    {
        Ikariam ikariam;
        Thread worker;

        public Form1()
        {
            InitializeComponent();
            textBox4.Enabled = false;
            comboBox1.SelectedIndex = 0;
            numericUpDown1.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Stop")
            {
                worker.Abort();
                textBox1.Enabled = true;
                textBox2.Enabled = true;
                numericUpDown2.Enabled = true;
                numericUpDown3.Enabled = true;
                numericUpDown4.Enabled = true;
                textBox4.Enabled = true;
                button1.Text = "Start";
                checkBox1.Enabled = true;
                comboBox1.Enabled = true;
                textBox3.Enabled = true;
            }
            else
            {
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                numericUpDown2.Enabled = false;
                numericUpDown3.Enabled = false;
                numericUpDown4.Enabled = false;
                textBox4.Enabled = false;
                button1.Text = "Stop";
                checkBox1.Enabled = false;
                comboBox1.Enabled = false;
                var index = comboBox1.SelectedIndex;
                textBox3.Enabled = false;
                _9kw.apikey = textBox3.Text;

                worker = new Thread(() =>
                {
                    var pos = ((int)numericUpDown2.Value).ToString();
                    var level = ((int)numericUpDown1.Value).ToString();
                    ikariam = new Ikariam(textBox1.Text, textBox2.Text, (Ikariam.Server)index, pos, level, (int)numericUpDown4.Value, (int)numericUpDown3.Value);
                    ikariam.mainForm = this;
                    if (checkBox1.Checked)
                    {
                        ikariam.Proxy = textBox4.Text;
                        ikariam.Log("Proxy ist aktiv");
                    }
                    ikariam.Log("Bot gestartet");

                    //logic here
                START_LOGIN:
                    int attempts = 1;
                    bool login = false;
                    while(!login)
                    {
                        if (attempts > 5)
                        {
                            ikariam.Log("5 Login-Versuche in Folge nicht erfolgreich");
                            attempts = 1;
                        }

                        login = ikariam.Login();
                        ikariam.Log("Logge ein");

                        attempts++;
                    }
                    ikariam.Log("Login erfolgreich");

                    attempts = 1;
                    string response = "";
                    while (true)
                    {
                        if (attempts > 5)
                        {
                            ikariam.Log("5 Missionstart-Versuche in Folge nicht erfolgreich");
                            attempts = 1;
                        }

                        if (ikariam.StartMission(ref response))
                        {
                            attempts = 1;

                            if (ikariam.isSessionExpired(response))
                            {
                                ikariam.Log("Session abgelaufen");
                                goto START_LOGIN;
                            }
                            else
                            {
                                ikariam.Log("StartMission erfolgreich");
                            }
                        }
                        else
                        {
                            ikariam.Log("StartMission erneuter Versuch");
                            attempts++;


                            if (ikariam.isSessionExpired(response))
                            {
                                ikariam.Log("Session abgelaufen");
                                goto START_LOGIN;
                            }
                        }
                    }
                });
                worker.Start();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                textBox4.Enabled = true;
            else
                textBox4.Enabled = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (File.Exists("config.json"))
                File.Delete("config.json");

            var config = new Config();
            config.apikey = textBox3.Text;
            config.buildinglevel = (int)numericUpDown1.Value;
            config.max = (int)numericUpDown3.Value;
            config.min = (int)numericUpDown4.Value;
            config.password = xorEnc(textBox2.Text);
            config.position = (int)numericUpDown2.Value;
            config.proxy = textBox4.Text;
            config.server = comboBox1.SelectedIndex;
            config.username = textBox1.Text;
            config.proxyactive = checkBox1.Checked;

            var json = new System.Web.Script.Serialization.JavaScriptSerializer();
            string jsonencoded = json.Serialize(config);
            File.WriteAllText("config.json", jsonencoded);


            if (File.Exists("captcha.png"))
                File.Delete("captcha.png");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Icon = Properties.Resources.Sensibleworld_Starwars_Darth_Vader;

            if (File.Exists("config.json"))
            {
                var jsonencdoed = File.ReadAllText("config.json");
                var json = new System.Web.Script.Serialization.JavaScriptSerializer();
                var config = json.Deserialize<Config>(jsonencdoed);

                textBox3.Text = config.apikey;
                numericUpDown1.Value = config.buildinglevel;
                numericUpDown3.Value = config.max;
                numericUpDown4.Value = config.min;
                textBox2.Text = xorDec(config.password);
                numericUpDown2.Value = config.position;
                textBox4.Text = config.proxy;
                comboBox1.SelectedIndex = config.server;
                textBox1.Text = config.username;
                checkBox1.Checked = config.proxyactive;
            }
        }

        const byte xorKey = 45;

        private string xorEnc(string text)
        {
            string enc = "";
            foreach(var c in text)
            {
                enc += (char)(c^xorKey);
            }

            return enc;
        }

        private string xorDec(string text)
        {
            string enc = "";
            foreach (var c in text)
            {
                enc += (char)(c ^ xorKey);
            }

            return enc;
        }
    }
}
