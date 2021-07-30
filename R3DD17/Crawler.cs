using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace R3DD17
{
    public class Crawler
    {
        private string authToken = null;

        public Crawler()
        {
            this.GetAuthToken();
        }

        public List<string> GetTop(string sr, int count)
        {
            var result = new List<string>();
            var url = $"https://oauth.reddit.com/r/{sr}/hot/.json?count={count}&limit={count}";
            
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + this.authToken);
            client.DefaultRequestHeaders.Add("User-Agent", "test42");
            var req = new HttpRequestMessage(HttpMethod.Get, url);
            var res = client.SendAsync(req).Result;
            if (res.IsSuccessStatusCode)
            {
                try
                {
                    var content = res.Content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrEmpty(content))
                    {
                        var resp = JsonConvert.DeserializeObject<ContentResponse>(content);
                        foreach (var c in resp?.data?.children)
                        {
                            var title = c.data.title.Trim();
                            result.Add(title);
                            var body = c.data.selftext.Trim();
                            var sentences = Regex.Split(body, @"(?<=[\.!\?])\s+");
                            result.AddRange(sentences.Where(x => !string.IsNullOrWhiteSpace(x)));
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Parse error from : {sr}");
                }
                
            }
            else
            {
                if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized || res.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    this.GetAuthToken();
                    return this.GetTop(sr, count);
                }
                else
                {
                    Console.WriteLine($"API exception : {res.StatusCode}/{res.ReasonPhrase}");
                }
            }


            var cleaned = CleanUp(sr, result);
            
            return cleaned;
        }

        public List<string> CleanUp(string sr, List<string> input)
        {
            var cleaned = input.Select(x => x.Replace("\n", " ").Replace("\r", " ")).ToList();
            var junkEntriesCount = cleaned.RemoveAll(x => string.IsNullOrEmpty(x) || x.ToLower().Contains("sticky") || x.ToLower().Contains("https://") || x.Length > 1200 || x.Length < 24 );

            var cleaned2 = new List<string>();
            foreach(var item in cleaned)
            {
                //check for too much crap
                var count = item.Length;
                var capsCount = item.Count(x => char.IsUpper(x));
                var symCount = item.Count(x => char.IsSymbol(x) || char.IsPunctuation(x));
                if ((symCount*1.0)/count > 0.3 || (capsCount * 1.0) / count > 0.6)
                {
                    Console.WriteLine("Junk: " +  item);
                    junkEntriesCount++;
                }
                else
                {
                    cleaned2.Add(item.Replace("&amp;#x200B;", "").Replace("&#x200B;", ""));
                }
            }
            Console.WriteLine($"Crawled results for {sr} : junkEntriesCount : {junkEntriesCount}, Final count : {cleaned2.Count}");
            return cleaned2;
        }

        private void GetAuthToken()
        {
            var url = "https://www.reddit.com/api/v1/access_token";

            var nvc = new List<KeyValuePair<string, string>>();
            nvc.Add(new KeyValuePair<string, string>("grant_type", "password"));
            nvc.Add(new KeyValuePair<string, string>("username", ConfigurationManager.AppSettings["R_UserName"]));
            nvc.Add(new KeyValuePair<string, string>("password", ConfigurationManager.AppSettings["R_Password"]));
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Basic " + ConfigurationManager.AppSettings["R_Auth"]);
            
            var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(nvc) };
            var res = client.SendAsync(req).Result;
            if (res.IsSuccessStatusCode)
            {
                var authRes = JsonConvert.DeserializeObject<AuthResponse>(res.Content.ReadAsStringAsync().Result);
                if (authRes != null)
                {
                    authToken = authRes.access_token;
                }

                Console.WriteLine("Auth token : Success");
            }
            else
            {
                Console.WriteLine(res.Content.ReadAsStringAsync().Result);
            }
            
        }
    }

    class AuthResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
    }

    class ContentResponse
    {
        public string kind { get; set; }
        public Data2 data { get; set; }

        public class Data2
        {
            public object approved_at_utc { get; set; }
            public string subreddit { get; set; }
            public string selftext { get; set; }
            public string author_fullname { get; set; }
            public bool saved { get; set; }
            public object mod_reason_title { get; set; }
            public int gilded { get; set; }
            public bool clicked { get; set; }
            public string title { get; set; }
            public List<object> link_flair_richtext { get; set; }
            public string subreddit_name_prefixed { get; set; }
            public bool hidden { get; set; }
            public int pwls { get; set; }
            public object link_flair_css_class { get; set; }
            public int downs { get; set; }
            public int? thumbnail_height { get; set; }
            public object top_awarded_type { get; set; }
            public bool hide_score { get; set; }
            public string name { get; set; }
            public bool quarantine { get; set; }
            public string link_flair_text_color { get; set; }
            public double upvote_ratio { get; set; }
            public string author_flair_background_color { get; set; }
            public string subreddit_type { get; set; }
            public int ups { get; set; }
            public int total_awards_received { get; set; }
            public int? thumbnail_width { get; set; }
            public string author_flair_template_id { get; set; }
            public bool is_original_content { get; set; }
            public List<object> user_reports { get; set; }
            public bool is_reddit_media_domain { get; set; }
            public bool is_meta { get; set; }
            public object category { get; set; }
            public object link_flair_text { get; set; }
            public bool can_mod_post { get; set; }
            public int score { get; set; }
            public object approved_by { get; set; }
            public bool author_premium { get; set; }
            public string thumbnail { get; set; }
            public object edited { get; set; }
            public string author_flair_css_class { get; set; }
            public string post_hint { get; set; }
            public object content_categories { get; set; }
            public bool is_self { get; set; }
            public object mod_note { get; set; }
            public double created { get; set; }
            public string link_flair_type { get; set; }
            public int wls { get; set; }
            public object removed_by_category { get; set; }
            public object banned_by { get; set; }
            public string author_flair_type { get; set; }
            public string domain { get; set; }
            public bool allow_live_comments { get; set; }
            public string selftext_html { get; set; }
            public object likes { get; set; }
            public string suggested_sort { get; set; }
            public object banned_at_utc { get; set; }
            public object view_count { get; set; }
            public bool archived { get; set; }
            public bool no_follow { get; set; }
            public bool is_crosspostable { get; set; }
            public bool pinned { get; set; }
            public bool over_18 { get; set; }
            public List<object> awarders { get; set; }
            public bool media_only { get; set; }
            public bool can_gild { get; set; }
            public bool spoiler { get; set; }
            public bool locked { get; set; }
            public string author_flair_text { get; set; }
            public List<object> treatment_tags { get; set; }
            public bool visited { get; set; }
            public object removed_by { get; set; }
            public object num_reports { get; set; }
            public string distinguished { get; set; }
            public string subreddit_id { get; set; }
            public object mod_reason_by { get; set; }
            public object removal_reason { get; set; }
            public string link_flair_background_color { get; set; }
            public string id { get; set; }
            public bool is_robot_indexable { get; set; }
            public object report_reasons { get; set; }
            public string author { get; set; }
            public object discussion_type { get; set; }
            public int num_comments { get; set; }
            public bool send_replies { get; set; }
            public string whitelist_status { get; set; }
            public bool contest_mode { get; set; }
            public List<object> mod_reports { get; set; }
            public bool author_patreon_flair { get; set; }
            public string author_flair_text_color { get; set; }
            public string permalink { get; set; }
            public string parent_whitelist_status { get; set; }
            public bool stickied { get; set; }
            public string url { get; set; }
            public int subreddit_subscribers { get; set; }
            public double created_utc { get; set; }
            public int num_crossposts { get; set; }
            public bool is_video { get; set; }
            public string url_overridden_by_dest { get; set; }
            public object modhash { get; set; }
            public int dist { get; set; }
            public List<ContentResponse> children { get; set; }
            public string after { get; set; }
            public string before { get; set; }
        }
    }
}
