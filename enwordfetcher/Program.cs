﻿#define AC_DEBUG

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace enwordfetcher
{
    internal class WordResult
    {
        internal string WordString { get; set; }
        internal string WordPronUK { get; set; }
        internal string WordPronUS { get; set; }
        internal string WordPronUKFile { get; set; }
        internal string WordPronUSFile { get; set; }
        internal List<String> WordExplains { get; private set; }

        internal WordResult()
        {
            this.WordExplains = new List<string>();
        }

        internal string WriteToString()
        {
            String rststr = this.WordString;
            rststr += Environment.NewLine;
            rststr += "Pron [UK]:" + this.WordPronUK;
            rststr += Environment.NewLine;
            rststr += "Pron [US]:" + this.WordPronUS;
            rststr += Environment.NewLine;
            rststr += "File for Pron [UK]:" + this.WordPronUKFile;
            rststr += Environment.NewLine;
            rststr += "File for Pron [US]:" + this.WordPronUSFile;
            rststr += Environment.NewLine;
            rststr += "Explains:";
            rststr += Environment.NewLine;
            foreach(string str in this.WordExplains)
            {
                rststr += str;
                rststr += Environment.NewLine;
            }
            rststr += Environment.NewLine;

            return rststr;
        }
    }

    class Program
    {
        private static String[] WordStrings;
        private static List<WordResult> Results = new List<WordResult>();
        private static String ResultFile;

        public static void Main(string[] args)
        {
            Console.WriteLine("The program try to fetch out pronounce for all words!");
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

#if AC_DEBUG
            WordStrings = File.ReadAllLines("words.txt");

            var backgroundTasks = new List<Task>();
            for (Int32 i = 0; i < 20; i++)
            {
                String strword = Program.WordStrings[i];
                backgroundTasks.Add(Task.Run(() => TestFetchWordAsync(strword)));
            }
            Task.WaitAll(backgroundTasks.ToArray());

            Program.ResultFile = Path.GetTempFileName();
            foreach (var rst in Program.Results)
            {
                File.AppendAllText(Program.ResultFile, rst.WriteToString());
            }
            //File.WriteAllLines(Program.ResultFile, Program.Results);

            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            if (!String.IsNullOrEmpty(Program.ResultFile))
            {
                Console.WriteLine("Result file generated: " + Program.ResultFile);
                Console.WriteLine("Press ANY key to open that file");
                Console.Read();
                Process.Start("notepad.exe", Program.ResultFile);
            }
            else
            {
                Console.WriteLine("Press ANY key to exit");
                Console.Read();
            }
#else
            var wordLink = @"https://github.com/dwyl/english-words/raw/master/words_alpha.txt";
            Console.WriteLine("Step 1. Fetch all words via " + wordLink);
            try
            {
                FetchAllWords(wordLink).Wait();
            }
            catch (Exception exp)
            {
                Console.WriteLine("Failed to fetch all words: " + exp.Message);
                Console.Read();
                return;
            }
            Console.WriteLine("Words fetched successfully");

            // Now analyze the strings
            String[] arWords = Program.WordStrings.Split(Environment.NewLine);
            Int32 nCount = arWords.Length;
            if (nCount <= 0)
            {
                Console.WriteLine("Word fetched as empty!");
                Console.Read();
                return;
            }

            // Step 2. Get a target folder
            //Console.WriteLine("Step 2, pick a target folder.");
            //String strTargetFolder = Console.ReadLine();

            // Split into 10 tasks 
            var backgroundTasks = new List<Task>();
            const Int32 TOTALCOUNT = 10;
            for (Int32 ibatch = 0; ibatch < TOTALCOUNT; ibatch++)
            {
                Int32 iStart = ibatch * (nCount / TOTALCOUNT);
                Int32 iEnd = ibatch * (nCount / TOTALCOUNT) + TOTALCOUNT - 1;
                backgroundTasks.Add(Task.Run(() => FetchWordPron(arWords, iStart, iEnd)));
            }

            await Task.WhenAll(backgroundTasks);
#endif

        }

        private static async Task FetchAllWords(String wordLink)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("text/html"));

            var stringTask = client.GetStringAsync(wordLink);

            //Program.WordStrings = await stringTask;
        }

        private static async Task FetchWordPron(String[] arWords, Int32 nStart, Int32 nEnd)
        {
            for (Int32 i = nStart; i < nEnd; i++)
            {
                String curword = arWords[i];
                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("text/html"));
                var dictTask = client.GetStringAsync("https://www.bing.com/dict/search?q=" + curword);

                var resString = await dictTask;

            }
        }

        private static async Task TestFetchWordAsync(String strword)
        {
            Console.WriteLine("Processing: " + strword);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("text/html"));

            Boolean useBing = false; // https://cn.bing.com/dict/search?q=university&qs=n&form=Z9LH5&sp=-1&pq=university&sc=8-10&sk=
            Boolean useIciba = true; // http://www.iciba.com/
            Boolean useYoudao = false; // US pron: https://dict.youdao.com/dictvoice?audio=take+part+in&type=2
            String resString = String.Empty;

            try
            {
                if (useBing)
                {
                    resString = await client.GetStringAsync("https://www.bing.com/dict/search?q=" + strword);
                    // US&nbsp;[.junɪ'vɜrsəti] </div><div class="hd_tf"><a class="bigaud" onmouseover="this.className = 'bigaud_f'; javascript: BilingualDict.Click(this, 'https://dictionary.blob.core.chinacloudapi.cn/media/audio/tom/37/e5/37E503EEA1A86E57383615D6E9E5FBCC.mp3', 'akicon.png', false, 'dictionaryvoiceid')" onmouseout="this.className = 'bigaud'" title="Click to listen" onclick="javascript: BilingualDict.Click(this, 'https://dictionary.blob.core.chinacloudapi.cn/media/audio/tom/37/e5/37E503EEA1A86E57383615D6E9E5FBCC.mp3', 'akicon.png', false, 'dictionaryvoiceid')" href="javascript: void(0); " h="ID = Dictionary,5187.1"></a></div><div class="hd_pr">UK&nbsp;[.juːnɪ'vɜː(r)səti] </div><div class="hd_tf"><a class="bigaud" onmouseover="this.className = 'bigaud_f'; javascript: BilingualDict.Click(this, 'https://dictionary.blob.core.chinacloudapi.cn/media/audio/george/37/e5/37E503EEA1A86E57383615D6E9E5FBCC.mp3', 'akicon.png', false, 'dictionaryvoiceid')" onmouseout="this.className = 'bigaud'" title="Click to listen" onclick="javascript: BilingualDict.Click(this, 'https://dictionary.blob.core.chinacloudapi.cn/media/audio/george/37/e5/37E503EEA1A86E57383615D6E9E5FBCC.mp3', 'akicon.png', false, 'dictionaryvoiceid')" href="javascript: void(0); " h="ID = Dictionary,5188.1"></a></div></div>
                    Int32 iPos = resString.IndexOf("hd_prUS\">");
                    Int32 iPos2 = resString.IndexOf('<', iPos);
                    String usPron = resString.Substring(iPos, iPos2 - iPos);
                    Console.WriteLine("US Pron: " + usPron);

                    iPos = resString.IndexOf("https:", iPos2);
                    iPos2 = resString.IndexOf(".mp3'", iPos);
                    usPron = resString.Substring(iPos, iPos2 - iPos);
                    Console.WriteLine("MP3: " + usPron);
                }
                else if (useIciba)
                {
                    // Check whether there is a SPACE inside
                    String strword2 = strword.Trim();
                    strword2 = strword2.Replace(' ', '+');
                    resString = await client.GetStringAsync("http://www.iciba.com/" + strword2);

                    Boolean bfailed = false;
                    WordResult wr = new WordResult();

                    // Pron.
                    Int32 iPos = resString.IndexOf("<div class=\"base-speak\">");
                    Int32 iPos2 = resString.IndexOf("</div>", iPos) + "</div>".Length;
                    String usPron = resString.Substring(iPos, iPos2 - iPos);

                    if (!String.IsNullOrEmpty(usPron))
                    {
                        iPos = usPron.IndexOf("<span>");
                        if (iPos != -1)
                            iPos += "<span>".Length;
                        if (iPos != -1)
                        {
                            usPron = usPron.Remove(0, iPos);
                            iPos = usPron.IndexOf("<span>");
                            iPos2 = usPron.IndexOf("</span>");

                            wr.WordString = strword;
                            wr.WordPronUK = usPron.Substring(iPos + 6, iPos2 - iPos - 6);

                            usPron = usPron.Remove(0, iPos2 + "</span>".Length);
                            iPos = usPron.IndexOf("('");
                            iPos2 = usPron.IndexOf("')");
                            wr.WordPronUKFile = usPron.Substring(iPos + 2, iPos2 - iPos - 2);

                            usPron = usPron.Remove(0, iPos2 + 2);
                            iPos = usPron.IndexOf("<span>");
                            usPron = usPron.Remove(0, iPos + "<span>".Length);

                            iPos = usPron.IndexOf("<span>");
                            iPos2 = usPron.IndexOf("</span>");
                            wr.WordPronUS = usPron.Substring(iPos + 6, iPos2 - iPos - 6);
                            usPron = usPron.Remove(0, iPos2 + "</span>".Length);
                            iPos = usPron.IndexOf("('");
                            iPos2 = usPron.IndexOf("')");
                            wr.WordPronUSFile = usPron.Substring(iPos + 2, iPos2 - iPos - 2);
                        }
                    }
                    else
                    {
                        bfailed = true;
                    }

                    if (!bfailed)
                    {
                        // Explain
                        iPos = resString.IndexOf("<ul class=\"base-list switch_part\" class=\"\">");
                        iPos2 = resString.IndexOf("</ul>", iPos) + "</ul>".Length;
                        String expStr = resString.Substring(iPos, iPos2 - iPos);

                        if (!String.IsNullOrEmpty(expStr))
                        {
                            iPos = expStr.IndexOf("<li");
                            while (iPos != -1)
                            {
                                iPos2 = expStr.IndexOf("</li>", iPos);

                                String expitem = expStr.Substring(iPos + 3, iPos2 - iPos);
                                Int32 j1 = expitem.IndexOf("<span");
                                Int32 j2 = -1;
                                String strexp = "";
                                while (j1 != -1)
                                {
                                    j2 = expitem.IndexOf("</span>", j1);
                                    j1 = expitem.IndexOf(">", j1);
                                    strexp += expitem.Substring(j1 + 1, j2 - j1 - 1) + " ;";

                                    j1 = expitem.IndexOf("<span", j2);
                                }
                                if (!String.IsNullOrEmpty(strexp))
                                    wr.WordExplains.Add(strexp);

                                iPos = expStr.IndexOf("<li", iPos2);
                            }
                        }
                        else
                        {
                            bfailed = true;
                        }
                    }

                    if (!bfailed)
                    {
                        Program.Results.Add(wr);
                    }

                    //// Transforms
                    //iPos = resString.IndexOf("<h1 class=\"base-word abbr chinese change-base\">变形</h1>");
                    //if (iPos != -1)
                    //{
                    //    iPos = resString.IndexOf("<p>", iPos);
                    //    iPos2 = resString.IndexOf("</p>", iPos) + "<p>".Length;
                    //    String strForms = resString.Substring(iPos, iPos2 - iPos);
                    //    Console.WriteLine("Forms: " + strForms);
                    //}

                    //// Sentences
                    //iPos = resString.IndexOf("<div class=\"collins-section\">");
                }
                else if (useYoudao)
                {
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine("Error: " + exp.Message);
            }
        }
    }
}
