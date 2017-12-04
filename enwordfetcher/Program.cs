#define AC_DEBUG

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;

namespace enwordfetcher
{
    class Program
    {
        private static String[] WordStrings;
        private static List<String> Results = new List<string>();

        public static void Main(string[] args)
        {
            Console.WriteLine("The program try to fetch out pronounce for all words!");
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

#if AC_DEBUG
            WordStrings = File.ReadAllLines("words.txt");
            TestFetchAllWordsAsync();

            
            Console.Read();
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

        private static async Task TestFetchAllWordsAsync()
        {
            Int32 nAmt = Program.WordStrings.Length;

            var backgroundTasks = new List<Task>();
            // For testing purpose, just peek 10
            nAmt = 10;
            for (Int32 i = 0; i < nAmt; i++)
            {
                String strword = Program.WordStrings[i];
                backgroundTasks.Add(Task.Run(() => TestFetchWordAsync(strword)));
            }
            await Task.WhenAll(backgroundTasks);

            if (File.Exists("results.txt"))
            {
                File.Delete("results.txt");
            }
            File.WriteAllLines("results.txt", Program.Results);
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
                resString = await client.GetStringAsync("http://www.iciba.com/" + strword);

                // Pron.
                Int32 iPos = resString.IndexOf("<div class=\"base-speak\">");
                Int32 iPos2 = resString.IndexOf("</div>", iPos) + "</div>".Length;
                String usPron = resString.Substring(iPos, iPos2 - iPos);
                Program.Results.Add(usPron);

                //// Explain
                //iPos = resString.IndexOf("<ul class=\"base-list switch_part\" class=\"\">");
                //iPos2 = resString.IndexOf("</ul>", iPos) + "</ul>".Length;
                //String strExp = resString.Substring(iPos, iPos2 - iPos);
                //Console.WriteLine("Explain:" + strExp);

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
        }
    }
}
