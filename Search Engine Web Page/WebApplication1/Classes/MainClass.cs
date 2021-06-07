using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Net;
using mshtml;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace WebApplication1
{
    public class MainClass
    {
        public HashSet<String> stop_words = new HashSet<String>()
            {
                "","a","able","about","above","according","accordingly","across","actually","after","afterwards","again","against","ain't","all",
                "allow","allows","almost","alone","along","already","also", "although","always","am","among","amongst","an","and", "another",
                "any","anybody","anyhow","anyone","anything","anyway","anyways","anywhere","apart","appear","appreciate","appropriate","are",
                "aren't","around","as","a's","aside","ask", "asking","associated","at","available","away","awfully","be","became","because","become",
                "becomes","becoming","been","before","beforehand","behind","being","believe","below","beside","besides","best","better","between",
                "beyond","both","brief","but","by","came","can","cannot","cant","can't","cause","causes","certain","certainly","changes","clearly","c'mon","co",
                "com","come","comes","concerning","consequently","consider","considering","contain","containing","contains","corresponding",
                "could","couldn't","course","c's","currently","definitely","described","despite","did","didn't","different","do","does","doesn't","doing","done",
                "don't","down","downwards","during","each","edu","eg","eight","either","else","elsewhere","enough","entirely","especially","et","etc","even","ever",
                "every","everybody","everyone","everything","everywhere","ex","exactly","example","except","far","few","fifth","first","five","followed","following",
                "follows","for","former","formerly","forth","four","from","further","furthermore","get","gets","getting","given","gives","go","goes","going","gone",
                "got","gotten","greetings","had","hadn't","happens","hardly","has","hasn't","have","haven't","having","he","he'd","he'll","hello","help","hence","her",
                "here","hereafter","hereby","herein","here's","hereupon","hers","herself","he's","hi","him","himself","his","hither","hopefully","how","howbeit",
                "however","how's","i","i'd","ie","if","ignored","i'll","i'm","immediate","in","inasmuch","inc","indeed","indicate","indicated","indicates","inner","insofar",
                "instead","into","inward","is","isn't","it","it'd","it'll","its","it's","itself","i've","just","keep","keeps","kept","know","known","knows","last","lately","later",
                "latter","latterly","least","less","lest","let","let's","like","liked","likely","little","look","looking","looks","ltd","mainly","many","may","maybe","me","mean",
                "meanwhile","merely","might","more","moreover","most","mostly","much","must","mustn't","my","myself","name",
                "namely","nd","near","nearly","necessary","need","needs","neither","never","nevertheless","new","next","nine","no",
                "nobody","non","none","noone","nor","normally","not","nothing","novel","now","nowhere","obviously","of","off","often",
                "oh","ok","okay","old","on","once","one","ones","only","onto","or","other","others","otherwise","ought","our","ours","ourselves",
                "out","outside","over","overall","own","particular","particularly","per","perhaps","placed","please","plus","possible","presumably",
                "probably","provides","que","quite","qv","rather","rd","re","really","reasonably","regarding","regardless","regards","relatively","respectively",
                "right","said","same","saw","say","saying","says","second","secondly","see","seeing","seem","seemed","seeming","seems","seen","self","selves",
                "sensible","sent","serious","seriously","seven","several","shall","shan't","she","she'd","she'll","she's","should","shouldn't","since","six","so","some",
                "somebody","somehow","someone","something","sometime","sometimes","somewhat","somewhere","soon","sorry","specified","specify","specifying",
                "still","sub","such","sup","sure","take","taken","tell","tends","th","than","thank","thanks","thanx","that","thats","that's","the","their","theirs","them",
                "themselves","then","thence","there","thereafter","thereby","therefore","therein","theres","there's","thereupon","these","they","they'd","they'll","they're",
                "they've","think","third","this","thorough","thoroughly","those","though","three","through","throughout","thru","thus","to","together","too","took","toward",
                "towards","tried","tries","truly","try","trying","t's","twice","two","un","under","unfortunately","unless","unlikely","until","unto","up","upon","us","use",
                "used","useful","uses","using","usually","value","various","very","via","viz","vs","want","wants","was","wasn't","way","we","we'd","welcome","well",
                "we'll","went","were","we're","weren't","we've","what","whatever","what's","when","whence","whenever","when's","where","whereafter","whereas",
                "whereby","wherein","where's","whereupon","wherever","whether","which","while","whither","who","whoever","whole","whom","who's","whose",
                "why","why's","will","willing","wish","with","within","without","wonder","won't","would","wouldn't","yes","yet","you","you'd","you'll","your",
                "you're","yours","yourself","yourselves","you've","zero"
            };

        static bool AllwaysGoodCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            return true;
        }

        static void cont(Queue<String> ToVisitURLS, Queue<String> VisitedURLS)
        {
            String Connection = "Data Source = (local); Initial Catalog = CrawlerDatabase; Integrated Security = True";
            SqlConnection con = new SqlConnection(Connection);
            SqlCommand cmd;
            con.Open();
            cmd = new SqlCommand("SELECT * FROM ToVisitQ", con);
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                ToVisitURLS.Enqueue(Convert.ToString(rdr.GetValue(1)));
            }
            rdr.Close();
            SqlCommand cmd2;
            cmd2 = new SqlCommand("SELECT * FROM VisitedQ", con);
            SqlDataReader rdr2 = cmd.ExecuteReader();
            while (rdr2.Read())
            {
                VisitedURLS.Enqueue(Convert.ToString(rdr2.GetValue(1)));
            }
            rdr2.Close();
            con.Close();
        }

        public void GetPages()
        {
            String Connection = "Data Source = (local); Initial Catalog = CrawlerDatabase; Integrated Security = True";
            SqlConnection con = new SqlConnection(Connection);
            SqlCommand cmd;
            Queue<String> ToVisitURLS = new Queue<String>();
            Stack<String> tempVisitedURLS = new Stack<String>();
            Queue<String> VisitedURLS = new Queue<String>();
            //ToVisitURLS.Enqueue("https://www.wikipedia.com/"); //Seed
            cont(ToVisitURLS, VisitedURLS);
            con.Open();
            Uri uriResult;
            bool result, kfaya = false;
            int downloaded = 0, required_pages = 4000, MaxForToVisit = 6500;
            while (ToVisitURLS.Count > 0 /*&& downloaded < required_pages*/)
            {
                Console.WriteLine("\n-------------------------------------------------------------------------");
                Console.WriteLine("To Visit URLs = " + ToVisitURLS.Count);
                Console.WriteLine("Visited URLs = " + VisitedURLS.Count + "(" + downloaded + ")");
                Console.WriteLine("temp Visited URLs = " + tempVisitedURLS.Count);

                String URI = ToVisitURLS.Dequeue();
                cmd = new SqlCommand("DELETE TOP (1) FROM ToVisitQ;", con);
                cmd.ExecuteNonQuery();

                result = Uri.TryCreate(URI, UriKind.RelativeOrAbsolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                if (result)
                {
                    try
                    {
                        tempVisitedURLS.Push(URI);
                        HttpWebRequest myWebRequest = (HttpWebRequest)WebRequest.Create(URI);
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(AllwaysGoodCertificate);
                        HttpWebResponse myWebResponse = (HttpWebResponse)myWebRequest.GetResponse();
                        if (((HttpWebResponse)myWebRequest.GetResponse()).StatusCode != HttpStatusCode.OK) continue;
                        if (!Uri.IsWellFormedUriString(myWebResponse.ResponseUri.ToString(), UriKind.RelativeOrAbsolute)) { continue; }
                        Stream streamResponse = myWebResponse.GetResponseStream();
                        StreamReader sReader = new StreamReader(streamResponse);
                        String rString = sReader.ReadToEnd();
                        streamResponse.Close();
                        sReader.Close();
                        String[] fStrings = rString.Split(new String[] { "\n" }, StringSplitOptions.None);
                        if (fStrings[1].Contains("lang=\"en") || fStrings[1].Contains("lang=\"mul"))
                        {
                            Console.WriteLine(URI);
                            Console.Write("1");
                            HTMLDocument y = new HTMLDocument();
                            Console.Write("2");
                            IHTMLDocument2 doc = (IHTMLDocument2)y;
                            Console.Write("3");
                            doc.write(rString);
                            Console.Write("4");
                            String body = doc.body.innerText;
                            Console.Write("5");
                            if (body != null)
                            {
                                Console.Write("6");
                                downloaded++;
                                cmd = new SqlCommand("insert into CrawlingDB Values('" + URI + "',@PageContent)", con);
                                cmd.Parameters.AddWithValue("@PageContent", body);
                                cmd.ExecuteNonQuery();
                            }
                            if (!kfaya)
                            {
                                Console.WriteLine("--thirsty");
                                IHTMLElementCollection elements = doc.links;
                                foreach (IHTMLElement el in elements)
                                {
                                    String link = (String)el.getAttribute("href", 0);
                                    if (link[link.Length - 1] != '/')
                                        link += "/";
                                    if (link == "https://creativecommons.org/2012/06/29/2012-paris-oer-declaration/")
                                        continue;
                                    result = Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                                    if (result && !tempVisitedURLS.Contains(link) && !VisitedURLS.Contains(link) && !ToVisitURLS.Contains(link))
                                    {
                                        ToVisitURLS.Enqueue(link);
                                        cmd = new SqlCommand("insert into ToVisitQ Values('" + link + "')", con);
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                                if (ToVisitURLS.Count > MaxForToVisit)
                                    kfaya = true;
                            }
                            else
                                Console.WriteLine("--enough");

                            VisitedURLS.Enqueue(URI);
                            cmd = new SqlCommand("insert into VisitedQ Values('" + URI + "')", con);
                            cmd.ExecuteNonQuery();
                            tempVisitedURLS.Pop();
                        }
                    }
                    catch (Exception e)
                    {
                        continue;
                    }
                }
                else continue;
            }
        }

        public List<string> Tokenize(String Word)
        {
            String[] delimiters = new String[] {"\r","'","", "ད", "’", "–", " ", "", "\n", "!", "%", "&", ",", "*", "*!", "**", ";", "+=", "~=", "`", ":", "&=", "/=", "(", ")",
                "'", "  ", "**=", "+", "-", ".", "/", "/!", "//", "{", "}", "^", "-=", "<",">", "<>", "==", ">=", "<=", "?", "@", "|=", "*=", "[", "]", "|", "~","་"};
            string[] words_list = Word.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            List<String> doc = words_list.ToList();
            for (int k = 0; k < doc.Count; k++)
            {
                String str = doc[k];
                String str2 = "";
                for (int w = 0; w < str.Length; w++)
                {
                    if (char.IsPunctuation(str[w]))
                    {
                        str2 = str.Remove(w, 1);
                        str = str2;
                    }
                }
                if (Regex.IsMatch(str, "^[a-zA-Z]*$")) { doc[k] = str.ToLower(); continue; }
                doc.Remove(doc[k]);
                k--;
            }
            words_list = doc.ToArray();
            return words_list.ToList();
        }

        public List<List<String>> GetWords()
        {
            String Connection = "Data Source = (local); Initial Catalog = CrawlerDatabase; Integrated Security = True";
            SqlConnection con = new SqlConnection(Connection);
            SqlDataReader rdr;
            SqlCommand cmd;
            con.Open();
            List<String> html = new List<String>();
            //First 1505 recrods
            cmd = new SqlCommand("SELECT Top 1505 PageContent from CrawlingDB order by ID", con);
            rdr = cmd.ExecuteReader();
            List<String> words_list = new List<String>();
            List<List<String>> pos = new List<List<String>>();
            while (rdr.Read())
            {
                html.Add((String)rdr[0]);
            }
            con.Close();
            for (int j = 0; j < html.Count; j++)
            {
                words_list.Clear();
                words_list = Tokenize(html[j]).ToList();
                pos.Add(words_list.ToList());
            }
            return pos;
        }

        public List<string> StopWordsRemoval(List<String> Words)
        {
            List<string> newWords = Words;
            for (int i = 0; i < newWords.Count; i++)
            {
                if (stop_words.Contains(newWords[i]) || newWords[i] == String.Empty)
                {
                    newWords.RemoveAt(i);
                    i--;
                }
            }
            return newWords;
        }

        public List<List<String>> RemoveStopWords(List<List<String>> WordsList)
        {
            List<List<String>> pos2 = new List<List<String>>();
            List<String> copy_pos = new List<String>();

            for (int m = 0; m < WordsList.Count; m++)
            {
                copy_pos = StopWordsRemoval(WordsList[m].ToList());
                pos2.Add(copy_pos);
            }
            return pos2;
        }

        public String PorterStemmer(String Word)
        {
            return new PorterStem().stem(Word);
        }

        public String Soundex(String Word)
        {
            Word = Word.ToLower();
            String newWord = "";
            newWord += Word[0];
            Word = Word.ToUpper();
            for (int i = 1; i < Word.Length; i++)
            {
                if (Word[i] == 'B' || Word[i] == 'F' || Word[i] == 'P' || Word[i] == 'V') newWord += '1';
                else if (Word[i] == 'C' || Word[i] == 'G' || Word[i] == 'J'
                    || Word[i] == 'K' || Word[i] == 'Q' || Word[i] == 'S'
                    || Word[i] == 'X' || Word[i] == 'Z') newWord += '2';
                else if (Word[i] == 'D' || Word[i] == 'T') newWord += '3';
                else if (Word[i] == 'L') newWord += '4';
                else if (Word[i] == 'M' || Word[i] == 'N') newWord += '5';
                else if (Word[i] == 'R') newWord += '6';
                else newWord += '0';
                if (i >= 2)
                {
                    if (newWord[newWord.Length - 1] == newWord[newWord.Length - 2])
                        newWord = newWord.Remove(newWord.Length - 1, 1);
                }
            }
            Word = newWord;
            newWord = "";
            foreach (var cha in Word)
            {
                if (cha != '0') newWord += cha;
            }
            if (newWord.Length > 4) newWord = newWord.Remove(4, newWord.Length - 4);
            while (newWord.Length < 4) newWord += '0';
            return newWord;
        }

        public List<string> BiGram(String Word)
        {
            List<string> bigramsList = new List<string>();
            Word = Word.ToLower();
            for (int i = -1; i < Word.Length; i++)
            {
                String aux;
                if (i == -1)
                    aux = "$" + Word[0];
                else if (i == Word.Length - 1)
                    aux = Word[Word.Length - 1] + "$";
                else
                    aux = Word[i].ToString() + Word[i + 1];
                bigramsList.Add(aux);
            }
            return bigramsList;
        }

        public Dictionary<String, List<String>> get_BiGram(List<String> Distinct_noSWords)
        {
            Dictionary<String, List<String>> Bigram = new Dictionary<string, List<string>>();
            foreach (var str_iter in Distinct_noSWords)
            {
                var str = str_iter;
                str = str.ToLower();
                for (int i = -1; i < str.Length; i++)
                {
                    String aux;
                    if (i == -1)
                        aux = "$" + str[0];
                    else if (i == str.Length - 1)
                        aux = str[str.Length - 1] + "$";
                    else
                        aux = str[i].ToString() + str[i + 1];
                    if (Bigram.Keys.Contains(aux))
                        Bigram[aux].Add(str);
                    else
                    {
                        List<string> tmplist = new List<string>();
                        tmplist.Add(str);
                        Bigram.Add(aux, tmplist);
                    }
                }
            }
            return Bigram;
        }

        public Dictionary<String, List<String>> get_Soundex(List<String> Distinct_noSWords)
        {
            Dictionary<String, List<String>> words_sound = new Dictionary<String, List<String>>();
            for (int j = 0; j < Distinct_noSWords.Count; j++)
            {
                String output = Soundex(Distinct_noSWords[j]);
                if (!words_sound.Keys.Contains(output))
                {
                    List<string> tempValues = new List<string>();
                    tempValues.Add(Distinct_noSWords[j]);
                    words_sound.Add(output, tempValues);
                }
                else
                    words_sound[output].Add(Distinct_noSWords[j]);
            }
            return words_sound;
        }

        public SortedDictionary<String, SortedDictionary<int, List<int>>> get_InvertedIndex(List<List<String>> WordsList)
        {
            SortedDictionary<String, SortedDictionary<int, List<int>>> InvIndex
                = new SortedDictionary<string, SortedDictionary<int, List<int>>>();
            for (int doc_id = 0; doc_id < WordsList.Count(); doc_id++)
            {
                for (int word_pos = 0; word_pos < WordsList[doc_id].Count(); word_pos++)
                {
                    if (stop_words.Contains(WordsList[doc_id][word_pos]))
                        continue;
                    var Term = PorterStemmer(WordsList[doc_id][word_pos]);
                    if (!InvIndex.ContainsKey(Term))
                        InvIndex.Add(Term, new SortedDictionary<int, List<int>>());
                    if (!InvIndex[Term].ContainsKey(doc_id + 1))
                        InvIndex[Term].Add(doc_id + 1, new List<int>());
                    InvIndex[Term][doc_id + 1].Add(word_pos + 1);
                }
            }
            return InvIndex;
        }

        public void BiGramInDB(Dictionary<String, List<String>> Words_BiGram)
        {
            String Connection = "Data Source = (local); Initial Catalog = CrawlerDatabase; Integrated Security = True";
            SqlConnection con = new SqlConnection(Connection);
            con.Open();
            SqlCommand cmd = new SqlCommand("DELETE FROM BigramIndex;", con);
            cmd.ExecuteNonQuery();
            foreach (var kgram in Words_BiGram)
            {
                String terms = "";
                foreach (var term in kgram.Value)
                    terms += term + ",";
                terms = terms.Remove(terms.Length - 1);
                cmd = new SqlCommand("insert into BigramIndex Values('" + kgram.Key + "',@terms)", con);
                cmd.Parameters.AddWithValue("@terms", terms);
                cmd.ExecuteNonQuery();
            }
            con.Close();
        }

        public void SoundexInDB(Dictionary<String, List<String>> Words_Soundex)
        {
            String Connection = "Data Source = (local); Initial Catalog = CrawlerDatabase; Integrated Security = True";
            SqlConnection con = new SqlConnection(Connection);
            con.Open();
            SqlCommand cmd = new SqlCommand("DELETE FROM SoundexIndex;", con);
            cmd.ExecuteNonQuery();
            foreach (var soundex in Words_Soundex)
            {
                String terms = "";
                foreach (var term in soundex.Value)
                    terms += term + ",";
                terms = terms.Remove(terms.Length - 1);
                cmd = new SqlCommand("insert into SoundexIndex Values('" + soundex.Key + "',@term)", con);
                cmd.Parameters.AddWithValue("@term", terms);
                cmd.ExecuteNonQuery();
            }
            con.Close();
        }

        public void InvertedIndexInDB(SortedDictionary<String, SortedDictionary<int, List<int>>> InvIndex)
        {

            String Connection = "Data Source = (local); Initial Catalog = CrawlerDatabase; Integrated Security = True";
            SqlConnection con = new SqlConnection(Connection);
            con.Open();
            SqlCommand cmd = new SqlCommand("DELETE FROM InvertedIndex;", con);
            cmd.ExecuteNonQuery();
            foreach (var Term in InvIndex)
            {
                String docIDs = "", frequency = "", positions = "";
                foreach (var doc in Term.Value)
                {
                    docIDs += doc.Key.ToString() + " , ";
                    frequency += doc.Key.ToString() + ":" + doc.Value.Count().ToString() + " , ";
                    foreach (var curr_pos in doc.Value)
                        positions += doc.Key.ToString() + ":" + curr_pos.ToString() + " , ";
                }
                docIDs = docIDs.Remove(docIDs.Length - 3);
                frequency = frequency.Remove(frequency.Length - 3);
                positions = positions.Remove(positions.Length - 3);
                cmd = new SqlCommand("insert into InvertedIndex Values('" + Term.Key + "',@DocIDs,@Frequency,@Positions)", con);
                cmd.Parameters.AddWithValue("@DocIDs", docIDs);
                cmd.Parameters.AddWithValue("@Frequency", frequency);
                cmd.Parameters.AddWithValue("@Positions", positions);
                cmd.ExecuteNonQuery();
            }
            con.Close();


        }

        public Dictionary<string, Dictionary<int, List<int>>> GetDocAndPos(List<string> queryWords)
        {
            Dictionary<string, Dictionary<int, List<int>>> dm = new Dictionary<string, Dictionary<int, List<int>>>();
            string query;
            String Connection = "Data Source = (local); Initial Catalog = CrawlerDatabase; Integrated Security = True";
            SqlConnection con = new SqlConnection(Connection);
            con.Open();
            SqlCommand cmd;
            SqlDataReader rdr;
            for (int i = 0; i < queryWords.Count; i++)
            {
                query = "Select Term, Positions from InvertedIndex Where Term = '" + queryWords[i] + "'";
                cmd = new SqlCommand(query, con);
                rdr = cmd.ExecuteReader();
                if (rdr.Read())
                {

                    string Term = rdr.GetString(0), pos_str = rdr.GetString(1);
                    if (!dm.ContainsKey(Term))
                        dm.Add(Term, new Dictionary<int, List<int>>());

                    var z1 = pos_str.Split(new string[] { " , " }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var z2 in z1)
                    {
                        var z3 = z2.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                        int doc_id = int.Parse(z3[0]), word_pos = int.Parse(z3[1]);
                        if (!dm[Term].ContainsKey(doc_id))
                            dm[Term].Add(doc_id, new List<int>());
                        dm[Term][doc_id].Add(word_pos);
                    }

                }
                rdr.Close();
            }
            con.Close();
            return dm;
        }

        // Jaccard coefficient   
        static double JaccardCofficient(string s1, string s2)
        {
            int common = 0;
            foreach (var c1 in s1)
                foreach (var c2 in s2)
                    if (c1 == c2) common++;
            return (double)(2 * common) / (s1.Length + s2.Length);
        }

        //edit Distance
        static int EditDistance(String str1, String str2)
        {
            int[,] dp = new int[str1.Length + 1, str2.Length + 1];

            for (int i = 0; i <= str1.Length; i++)
            {
                for (int j = 0; j <= str2.Length; j++)
                {
                    if (i == 0)
                        dp[i, j] = j;
                    else if (j == 0)
                        dp[i, j] = i;
                    else if (str1[i - 1] == str2[j - 1])
                        dp[i, j] = dp[i - 1, j - 1];
                    else
                        dp[i, j] = 1 + Math.Min(Math.Min(dp[i, j - 1], // Insert 
                                        dp[i - 1, j]), // Remove 
                                        dp[i - 1, j - 1]); // Replace 
                }
            }

            return dp[str1.Length, str2.Length];
        }

        public Dictionary<string, List<string>> Spell_Checker(List<string> wongWords)
        {
            Dictionary<string, List<string>> res = new Dictionary<string, List<string>>();

            foreach (var word in wongWords)
            {
                var word_bigram = BiGram(word).Distinct();
                Dictionary<string, int> edit_dist_res = new Dictionary<string, int>();
                foreach (var bigr in word_bigram)
                {
                    String Connection = "Data Source = (local); Initial Catalog = CrawlerDatabase; Integrated Security = True";
                    SqlConnection con = new SqlConnection(Connection);
                    SqlDataReader rdr;
                    SqlCommand cmd;
                    con.Open();
                    cmd = new SqlCommand("SELECT terms from BigramIndex where k_gram = '" + bigr + "'", con);
                    rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        var woos = rdr[0].ToString().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var wo in woos)
                        {
                            if (!edit_dist_res.ContainsKey(wo) && JaccardCofficient(wo, word) >= 0.45)
                                edit_dist_res.Add(wo, EditDistance(wo, word));
                        }
                    }
                    rdr.Close();
                    con.Close();
                }
                List<string> matchedWords = new List<string>();
                foreach (var wo in edit_dist_res)
                {
                    if (wo.Value == edit_dist_res.Values.Min())
                    {
                        matchedWords.Add(wo.Key);
                    }
                }
                res.Add(word, matchedWords.ToList());
            }

            return res;
        }


        public List<string> GetDocsURLS()
        {
            List<string> Ret = new List<string>();
            Ret.Add("");
            String Connection = "Data Source = (local); Initial Catalog = CrawlerDatabase; Integrated Security = True";
            SqlConnection con = new SqlConnection(Connection);
            SqlDataReader rdr;
            SqlCommand cmd;
            con.Open();
            cmd = new SqlCommand("SELECT Top 1505 URL from CrawlingDB order by ID", con);
            rdr = cmd.ExecuteReader();
            while (rdr.Read())
                Ret.Add(rdr[0].ToString());
            rdr.Close();
            con.Close();

            return Ret;
        }


        //nour


        public SortedDictionary<String, Dictionary<int, int>> OriginalTerm_DocID(List<List<String>> WordsList)
        {
            SortedDictionary<String, Dictionary<int, int>> OriginalTerms = new SortedDictionary<String, Dictionary<int, int>>();
            for (int doc_id = 0; doc_id < WordsList.Count(); doc_id++)
            {
                for (int word_pos = 0; word_pos < WordsList[doc_id].Count(); word_pos++)
                {
                    if (stop_words.Contains(WordsList[doc_id][word_pos]))
                        continue;
                    else if (!OriginalTerms.ContainsKey(WordsList[doc_id][word_pos]))
                        OriginalTerms.Add(WordsList[doc_id][word_pos], new Dictionary<int, int>());
                    if (OriginalTerms[WordsList[doc_id][word_pos]].ContainsKey(doc_id + 1))
                        OriginalTerms[WordsList[doc_id][word_pos]][doc_id + 1]++;
                    else OriginalTerms[WordsList[doc_id][word_pos]].Add(doc_id + 1, 1);
                }
            }
            return OriginalTerms;
        }

        public void OriginalTermsInDB(SortedDictionary<String, Dictionary<int, int>> Oterms_doc)
        {
            String Connection = "Data Source = (local); Initial Catalog = CrawlerDatabase; Integrated Security = True";
            SqlConnection con = new SqlConnection(Connection);
            con.Open();
            SqlCommand cmd = new SqlCommand("DELETE FROM [dbo].[OriginalTerms];", con);
            cmd.ExecuteNonQuery();
            for (int i = 0; i < Oterms_doc.Count; i++)
            {
                String docIDs = "";
                for (int j = 0; j < Oterms_doc.ElementAt(i).Value.Count; j++)
                {
                    docIDs += Oterms_doc.ElementAt(i).Value.ElementAt(j).Key + ":" + Oterms_doc.ElementAt(i).Value.ElementAt(j).Value + ",";
                }
                docIDs = docIDs.Remove(docIDs.Length - 1);
                cmd = new SqlCommand("insert into [dbo].[OriginalTerms] Values('" + Oterms_doc.ElementAt(i).Key + "',@DocIDs)", con);
                cmd.Parameters.AddWithValue("@DocIDs", docIDs);
                cmd.ExecuteNonQuery();
            }
        }

        public Dictionary<string, Dictionary<int, int>> SoundexModule(String queryWord)
        {
            String wordSound = Soundex(queryWord);
            String similarSoundTerms = "";
            Dictionary<string, Dictionary<int, int>> result = new Dictionary<string, Dictionary<int, int>>();
            String Connection = "Data Source = (local); Initial Catalog = CrawlerDatabase; Integrated Security = True";
            SqlConnection con = new SqlConnection(Connection);
            con.Open();
            SqlCommand cmd;
            SqlDataReader rdr;
            cmd = new SqlCommand("Select [term] from [dbo].[SoundexIndex] Where [soundex] ='" + wordSound + "'", con);
            rdr = cmd.ExecuteReader();
            if (rdr.Read()) similarSoundTerms = rdr.GetString(0);
            rdr.Close();
            con.Close();
            if (similarSoundTerms != "")
            {
                string[] similarTerms = similarSoundTerms.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                con.Open();
                for (int i = 0; i < similarTerms.Count(); i++)
                {
                    cmd = new SqlCommand("Select [DocID] from [dbo].[OriginalTerms] Where [Term] = '" + similarTerms[i] + "'", con);
                    rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        string[] docIDs_freqs = rdr.GetString(0).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        Dictionary<int, int> doc_freqS = new Dictionary<int, int>();
                        for (int j = 0; j < docIDs_freqs.Count(); j++)
                        {
                            string[] docID_freq = docIDs_freqs[j].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            doc_freqS.Add(Int32.Parse(docID_freq[0]), Int32.Parse(docID_freq[1]));
                        }
                        var sortedDict = from entry in doc_freqS orderby entry.Value descending select entry;
                        int x = sortedDict.ElementAt(0).Key;
                        int y = sortedDict.ElementAt(0).Value;
                        Dictionary<int, int> sortedDocs = sortedDict.ToDictionary(pair => pair.Key, pair => pair.Value);
                        result.Add(similarTerms[i], sortedDocs);
                    }
                    rdr.Close();
                }
                con.Close();
            }
            return result;
        }




    }
}
