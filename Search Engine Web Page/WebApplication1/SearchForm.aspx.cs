using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplication1
{
    public partial class SearchForm : System.Web.UI.Page
    {
        MainClass ms = new MainClass();
        List<string> DocURL = new List<string>();
        protected void Page_Load(object sender, EventArgs e)
        {
            DocURL = ms.GetDocsURLS();
        }
        protected void Search_Btn(object sender, EventArgs e)
        {
            ListBox1.Items.Clear();

            if (queryText.Text != string.Empty)
            {

                List<string> queryWords = ms.Tokenize(queryText.Text);
                queryWords = ms.StopWordsRemoval(queryWords);
                List<string> orignqueryWords = new List<string>(queryWords);
                for (int i = 0; i < queryWords.Count; i++) queryWords[i] = ms.PorterStemmer(queryWords[i]);

                List<string> distinct_queryWords = new List<string>(queryWords.Distinct());
                Dictionary<string, Dictionary<int, List<int>>> Word_DocAndPos = ms.GetDocAndPos(distinct_queryWords);


                List<string> wrong_queryWords = new List<string>();
                for (int i = 0; i < queryWords.Count; i++)
                    if (!Word_DocAndPos.ContainsKey(queryWords[i]))
                        wrong_queryWords.Add(orignqueryWords[i]);


                if (RadioButtonList1.Items[1].Selected && wrong_queryWords.Count > 0) //Spell Checker 
                {
                    wrong_queryWords = wrong_queryWords.Distinct().ToList();

                    var corrected_queryWords = ms.Spell_Checker(wrong_queryWords);

                    ListBox1.Items.Add(" Did you mean? ");
                    ListBox1.Items.Add("");

                    foreach (var cor_word in corrected_queryWords)
                    {
                        foreach (var x in cor_word.Value)
                            ListBox1.Items.Add("(" + x + ")\tinstead of (" + cor_word.Key + ")");
                        ListBox1.Items.Add("______________________");
                        ListBox1.Items.Add("");
                    }
                    //Response.Write("<script>window.alert('Spell Checker')</script>");

                }

                else
                {

                    //              <doc_id ,   <   pos    , word_id >   >
                    SortedDictionary<int, SortedDictionary<int, int>> doc_aux = new SortedDictionary<int, SortedDictionary<int, int>>();

                    for (int i = 0; i < distinct_queryWords.Count; i++)
                    {
                        int word_id = i + 1;
                        var word = distinct_queryWords[i];
                        if (Word_DocAndPos.ContainsKey(word))
                        {
                            foreach (var doc in Word_DocAndPos[word])
                            {
                                int doc_id = doc.Key;
                                foreach (var pos in doc.Value)
                                {
                                    if (!doc_aux.ContainsKey(doc_id))
                                        doc_aux.Add(doc_id, new SortedDictionary<int, int>());

                                    if (!doc_aux[doc_id].ContainsKey(pos))
                                        doc_aux[doc_id].Add(pos, word_id);

                                }
                            }
                        }
                    }



                    if (queryText.Text.StartsWith("\"") && queryText.Text.EndsWith("\""))
                    {
                        if (wrong_queryWords.Count > 0)
                        {
                            ListBox1.Items.Add("Your search - " + queryText.Text + " - did not match any documents.");
                        }
                        else
                        {
                            Dictionary<int, int> DocRank_ExactMatch = new Dictionary<int, int>();
                            foreach (var doc in doc_aux)
                            {
                                int doc_id = doc.Key;
                                int contig = 0, perv_pos = -1, perv_val = 0, freq = 0;
                                foreach (var pos in doc.Value)
                                {
                                    int curr_pos = pos.Key, curr_val = pos.Value;
                                    if (perv_pos + 1 == curr_pos &&
                                      (perv_val + 1 == curr_val || queryWords[curr_val - 1] == queryWords[perv_val + 1 - 1]))
                                    {
                                        contig++;
                                        perv_pos = curr_pos;
                                        perv_val = curr_val;
                                    }
                                    else if (curr_val == 1)
                                    {
                                        contig = 1;
                                        perv_pos = curr_pos;
                                        perv_val = curr_val;
                                    }
                                    else
                                    {
                                        contig = 0;
                                        perv_pos = -1;
                                    }

                                    if (contig == queryWords.Count)
                                    {
                                        freq++;
                                        contig = 0;
                                        perv_pos = -1;
                                    }
                                }
                                if (freq > 0)
                                    DocRank_ExactMatch.Add(doc_id, freq);
                            }
                            var sortedExactMatchDocs = from entry in DocRank_ExactMatch orderby entry.Value descending select entry;
                            if (sortedExactMatchDocs.Count() > 0)
                                ListBox1.Items.Add(" --Exact Matching Results-- ");
                            else
                                ListBox1.Items.Add("Your search - " + queryText.Text + " - did not match any documents.");
                            ListBox1.Items.Add("");
                            foreach (var doc in sortedExactMatchDocs)
                            {
                                ListBox1.Items.Add("DocID : " + doc.Key.ToString() + " -> (" + doc.Value + " Occurrence)");
                                ListBox1.Items.Add(DocURL[doc.Key]);
                                ListBox1.Items.Add("______________________");
                                ListBox1.Items.Add("");
                            }
                        }
                        //Response.Write("<script>window.alert('Exact Match')</script>");

                    }

                    else if (RadioButtonList1.Items[0].Selected)
                    {
                        Dictionary<string, Dictionary<int, int>> docS = ms.SoundexModule(queryWords[0]);
                        foreach (var term in docS)
                        {
                            ListBox1.Items.Add("--Term: (" + term.Key.ToString() + ")--");
                            ListBox1.Items.Add("");
                            foreach (var ID_Freq in term.Value)
                            {
                                ListBox1.Items.Add("DocID: " + ID_Freq.Key.ToString() + "  ->  (" +
                                     ID_Freq.Value.ToString() + " Frequency)");
                                ListBox1.Items.Add(DocURL[ID_Freq.Key]);
                                ListBox1.Items.Add("______________________");
                            }
                            ListBox1.Items.Add("************************************************************************************");
                            ListBox1.Items.Add("************************************************************************************");
                            ListBox1.Items.Add("");
                        }
                        //Response.Write("<script>window.alert('Soundex')</script>");

                    }

                    else    // Normal Search
                    {
                        if (wrong_queryWords.Count == queryWords.Count)
                        {
                            ListBox1.Items.Add("Your search - " + queryText.Text + " - did not match any documents.");
                        }
                        else
                        {
                            if (distinct_queryWords.Count == 1)
                            {
                                Dictionary<int, int> res = new Dictionary<int, int>();
                                ListBox1.Items.Add(" --Common Results-- ");
                                ListBox1.Items.Add("");

                                foreach (var doc in Word_DocAndPos[distinct_queryWords[0]])
                                {
                                    res.Add(doc.Key, doc.Value.Count);
                                }
                                var sortedres = from entry in res orderby entry.Value descending select entry;

                                foreach (var doooc in sortedres)
                                {
                                    ListBox1.Items.Add("DocID : " + doooc.Key + "  ->  (" + doooc.Value + " Frequency)");
                                    ListBox1.Items.Add(DocURL[doooc.Key]);
                                    ListBox1.Items.Add("______________________");
                                    ListBox1.Items.Add("");
                                }
                            }
                            else if (distinct_queryWords.Count >= 2) //Multi word Search
                            {
                                List<int> common_docs = new List<int>();
                                List<int> uncommon_docs = new List<int>();
                                foreach (var doooc in doc_aux)
                                {
                                    var doooc_id = doooc.Key;
                                    HashSet<int> hlpr = new HashSet<int>();

                                    foreach (var pooos in doooc.Value)
                                        hlpr.Add(pooos.Value);

                                    if (hlpr.Count == distinct_queryWords.Count)
                                        common_docs.Add(doooc_id);
                                    else
                                        uncommon_docs.Add(doooc_id);
                                }

                                if (common_docs.Count > 0)
                                {
                                    Dictionary<int, int> PageRanking = new Dictionary<int, int>();

                                    //      doc_aux     =>        <doc_id ,   <   pos    , word_id >   >

                                    foreach (var doooc_id in common_docs)
                                    {
                                        int min_dist = 99999999;
                                        foreach (var pooos in doc_aux[doooc_id])
                                        {
                                            List<int> dist_aux = new List<int>();
                                            for (int i = 0; i < distinct_queryWords.Count; i++)
                                            {
                                                if (i == pooos.Value - 1)
                                                    dist_aux.Add(0);
                                                else
                                                    dist_aux.Add(99999999);
                                            }
                                            int dist = 0;
                                            foreach (var pooos2 in doc_aux[doooc_id])
                                            {
                                                if (pooos2.Value != pooos.Value)
                                                {
                                                    dist_aux[pooos2.Value - 1] = Math.Min(dist_aux[pooos2.Value - 1],
                                                        Math.Abs(pooos2.Key - pooos.Key));
                                                }
                                            }
                                            foreach (var x in dist_aux) dist += x;
                                            min_dist = Math.Min(min_dist, dist);

                                            //if (perv_pos != -1)
                                            //    min_dist = min_dist + Math.Abs(perv_pos - pooos.Key);
                                            //perv_pos = pooos.Key;
                                        }
                                        PageRanking.Add(doooc_id, min_dist);
                                    }


                                    var sortedres = from entry in PageRanking orderby entry.Value ascending select entry;

                                    ListBox1.Items.Add(" --Common Results-- ");
                                    ListBox1.Items.Add("");

                                    foreach (var doooc in sortedres)
                                    {
                                        ListBox1.Items.Add("DocID : " + doooc.Key + "  ->  (" + doooc.Value + " Min_dist)");
                                        ListBox1.Items.Add(DocURL[doooc.Key]);
                                        ListBox1.Items.Add("______________________");
                                        ListBox1.Items.Add("");
                                    }
                                    ListBox1.Items.Add("************************************************************************************");
                                    ListBox1.Items.Add("************************************************************************************");
                                    ListBox1.Items.Add("");

                                }

                                if (uncommon_docs.Count > 0)
                                {
                                    ListBox1.Items.Add(" --UnCommon Results-- ");
                                    ListBox1.Items.Add("");
                                    foreach (var doooc_id in uncommon_docs)
                                    {
                                        ListBox1.Items.Add("DocID : " + doooc_id);
                                        ListBox1.Items.Add(DocURL[doooc_id]);
                                        ListBox1.Items.Add("______________________");
                                        ListBox1.Items.Add("");
                                    }
                                }



                            }


                        }

                    }

                }

            }
            RadioButtonList1.ClearSelection();
        }
    }
}
