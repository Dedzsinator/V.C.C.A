using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Speech.Recognition;
using System.Speech.Synthesis;
using AIMLbot;

namespace V.C.C.A
{
    public partial class Form1 : Form
    {
        private SpeechRecognitionEngine engine;
        private SpeechSynthesizer synthesizer;

        private Bot bot;
        private User user;

        private Dictionary<string, string> cmd = new Dictionary<string, string>;

        public void LoadSpeech()
        {
            try
            {
                #region Speech, Speak, Chatbot

                engine = new SpeechRecognitionEngine();
                engine.SetInputToDefaultAudioDevice();

                engine.LoadGrammar(new DictationGrammar());

                engine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(rec);

                engine.RecognizeAsync(RecognizeMode.Multiple);

                bot = new Bot();
                bot.loadSettings();

                user = new User("Nándor", bot);

                bot.isAcceptingUserInput = false;
                bot.loadAIMLFromFiles();
                bot.isAcceptingUserInput = true;

                synthesizer = new SpeechSynthesizer();
                #endregion

                #region Commands
                //Command hozzaadashoz parancs utana "," es a kulcs//
                cmd.Add("What time is it", "WhatTime");
                cmd.Add("Tell me the time", "WhatTime");
                cmd.Add("What date is right now", "WhatDate");

                string[] cmds = cmd.Keys.ToArray();

                Choices c_cmds = new Choices(cmds);

                GrammarBuilder gb_cmds = new GrammarBuilder();
                gb_cmds.Append(c_cmds);

                Grammar g_cmds = new Grammar(gb_cmds);
                g_cmds.Name = "cmds";

                engine.LoadGrammar(g_cmds);
                #endregion

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadSpeech();

            if (label2.Text == "Andrea")
            {
                synthesizer.SelectVoiceByHints(VoiceGender.Female);
            }
        }

        private void rec(object s, SpeechRecognizedEventArgs e)
        {
            string speech = e.Result.Text;
            string answer = string.Empty;

            if(e.Result.Confidence > 0.4f)
            {
                label1.Text = "You: " + speech;

                switch (e.Result.Grammar.Name)
                {
                    case "cmds":
                        try
                        {
                            string cmdType = cmd[speech];
                            answer = ProcessCMD(cmdType);
                        }
                        catch
                        {
                            break;
                        }
                        break;
                    default:
                        answer = GetResponse(speech);
                        
                        break;
                }
                label2.Text = "Andrea: " + answer;
                synthesizer.SpeakAsync(answer);
            }
        }

        private string ProcessCMD(string cmdType)
        {
            string answer = string.Empty;
            switch (cmdType)
            {
                //Ide kell hozzaadni mindegyik parancsot mint "case"//
                case "WhatTime":
                    answer = DateTime.Now.ToShortTimeString();
                    break;

                case "WhatDate":
                    answer = DateTime.Now.ToShortDateString();
                    break;
            }
            return answer;
        }

        private string GetResponse(string query)
        {
            Request request = new Request(query, user, bot);
            Result result = bot.Chat(request);
            return result.Output;
        }
    }
}
