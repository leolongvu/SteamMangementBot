using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace SteamMarketBot
{
    class SteamParsing
    {
        /// <summary>
        /// Provide parsing methods and connection for Steam API.
        /// </summary>

        #region Constructors

        public static SteamLibrary.CurrInfoLst Currencies = new SteamLibrary.CurrInfoLst();

        public static List<SteamLibrary.InventItem> InventList = new List<SteamLibrary.InventItem>();

        #endregion

        #region HttP protocol

        public static string SendPost(string data, string url, string refer, 
            CookieContainer cookieCont, bool tolog)
        {
            var res = SendPostRequest(data, url, refer, cookieCont, tolog);

            return res;
        }

        public static string SendGet(string url, CookieContainer cookieCont, bool UseProxy, bool keepAlive)
        {
            var res = SteamParsing.GetRequest(url, cookieCont, UseProxy, keepAlive);

            return res;
        }

        public static string SendPostRequest(string req, string url, string refer, CookieContainer cookie, bool tolog)
        {
            var requestData = Encoding.UTF8.GetBytes(req);
            string content = string.Empty;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                request.CookieContainer = cookie;
                request.Method = "POST";

                //New
                request.Proxy = null;
                request.Timeout = 30000;
                //KeepAlive is True by default
                request.KeepAlive = true;

                request.UserAgent = SteamLibrary.steamUA;

                request.Referer = refer;
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = requestData.Length;

                Stream s = request.GetRequestStream();
                {
                    s.Write(requestData, 0, requestData.Length);
                }
                s.Close();

                HttpWebResponse resp = (HttpWebResponse)request.GetResponse();

                StreamReader stream = new StreamReader(resp.GetResponseStream());
                content = stream.ReadToEnd();

                if (tolog)
                {
                    SteamLibrary.AddtoLog(content);
                }
               
                cookie = request.CookieContainer;

                resp.Close();
                stream.Close();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp = e.Response;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        content = sr.ReadToEnd();
                    }
                }
            }
            return content;
        }

        public static string GetRequest(string url, CookieContainer cookie, bool UseHost, bool keepAlive)
        {
            string content = string.Empty;

            /*int hostNum = 0;
            bool hostUsed = false;*/

            try
            {

                /*if (UseHost && (hostList.Count != 0))
                {
                    hostNum = GetFreeIndex();
                    if (hostNum != -1)
                    {
                        hostList[hostNum].InUsing = true;
                        hostList[hostNum].WorkLoad++;
                        url = url.Replace(SteamSite._host, hostList[hostNum].Host);
                        MessageBox.Show(url);
                        hostUsed = true;
                    }
                }*/

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";

                request.Proxy = null;
                request.Timeout = 30000;
                request.Host = SteamLibrary._host;

                request.KeepAlive = keepAlive;
                request.UserAgent = SteamLibrary.steamUA;

                request.Accept = "application/json";
                request.CookieContainer = cookie;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                var stream = new StreamReader(response.GetResponseStream());
                content = stream.ReadToEnd();

                response.Close();
                stream.Close();
            }

            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {

                    HttpWebResponse resp = (HttpWebResponse)e.Response;
                    int statCode = (int)resp.StatusCode;

                    if (statCode == 403)
                    {
                        content = "403";
                    }
                    else
                    {
                        using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                        {
                            content = sr.ReadToEnd();
                        }
                    }
                }

            }

            //Free host
            /*if (UseHost && (hostList.Count != 0) && hostUsed)
            {
                hostList[hostNum].InUsing = false;
            }*/

            return content;
        }

        #endregion

        #region Steam Parse

        public class SteamSite
        {
            /// <summary>
            /// Provide methods for parsing data from Steam packet.
            /// </summary>

            #region Image Processing

            private static Color backColor(Image img, bool doWhite)
            {
                Color back = SystemColors.Control;
                if (doWhite)
                    back = Color.White;
                Bitmap bitmap = new Bitmap(img);
                var colors = new List<Color>();
                for (int y = 0; y < bitmap.Size.Height; y++)
                {
                    //Let's grab bunch of pixels from center of image
                    colors.Add(bitmap.GetPixel(bitmap.Size.Width / 2, y));
                }

                float imageBrightness = colors.Average(color => color.GetBrightness());
                if (imageBrightness > 0.6)
                    back = Color.Black;
                return back;
            }

            public static void loadImg(string imgurl, PictureBox picbox, bool drawtext, bool doWhite)
            {
                try
                {
                    if (imgurl == string.Empty)
                        return;

                    WebClient wClient = new WebClient();
                    byte[] imageByte = wClient.DownloadData(imgurl);
                    using (MemoryStream ms = new MemoryStream(imageByte, 0, imageByte.Length))
                    {
                        ms.Write(imageByte, 0, imageByte.Length);
                        var resimg = Image.FromStream(ms, true);
                        picbox.BackColor = backColor(resimg, doWhite);
                        picbox.Image = resimg;
                    }
                }
                catch (Exception exc)
                {
                    SteamLibrary.AddtoLog(exc.Message);
                }
            }

            public static void StartLoadImgTread(string imgUrl, PictureBox picbox)
            {
                if (imgUrl.Contains("http://"))
                {
                    ThreadStart threadStart = delegate() { loadImg(imgUrl, picbox, true, false); };
                    Thread pTh = new Thread(threadStart);
                    pTh.IsBackground = true;
                    pTh.Start();
                }
            }

            #endregion

            #region Parsing

            public SteamLibrary.StrParam GetNameBalance(CookieContainer cookieCont)
            {
                SteamLibrary.AddtoLog("Getting account name and balance...");

                string markpage = SendGet(SteamLibrary._market, cookieCont, false, true);

                //For testing purposes!
                //string markpage = System.IO.File.ReadAllText(@"C:\sing.htm");

                //Fix to getting name regex
                string parseName = Regex.Match(markpage, "(?<=buynow_dialog_myaccountname\">)(.*)(?=</span>)").ToString().Trim();
                if (parseName == "")
                {
                    return null;
                }

                //Set profileId for old Url format
                SteamAutoFunction.MyUserID = Regex.Match(markpage, "(?<=g_steamID = \")(.*)(?=\";)").ToString();

                //30.05.14 Update
                string parseImg = Regex.Match(markpage, "(?<=avatarIcon\"><img src=\")(.*)(?=\" alt=\"\"></span>)", RegexOptions.Singleline).ToString();

                string parseAmount = Regex.Match(markpage, "(?<=marketWalletBalanceAmount\">)(.*)(?=</span>)").ToString();

                string country = Regex.Match(markpage, "(?<=g_strCountryCode = \")(.*)(?=\";)").ToString();
                string strlang = Regex.Match(markpage, "(?<=g_strLanguage = \")(.*)(?=\";)").ToString();

                Currencies.GetType(parseAmount);

                parseAmount = Currencies.ReplaceAscii(parseAmount);

                //?country=RU&language=russian&currency=5&count=20
                string Addon = string.Format(SteamLibrary.jsonAddonUrl, country, strlang, Currencies.GetCode());

                return new SteamLibrary.StrParam(parseName, SteamAutoFunction.MyUserID, parseAmount, parseImg, Addon);
            }          

            public string GetSessId(CookieContainer cookieCont)
            {
                //sessid sample MTMyMTg5MTk5Mw%3D%3D
                string resId = string.Empty;
                var stcook = cookieCont.GetCookies(new Uri(SteamLibrary._mainsite));

                for (int i = 0; i < stcook.Count; i++)
                {
                    string cookname = stcook[i].Name.ToString();

                    if (cookname == "sessionid")
                    {
                        resId = stcook[i].Value.ToString();
                        break;
                    }
                }
                return resId;
            }

            public bool IsStillLogged(CookieContainer cookieCont)
            {
                var stcook = cookieCont.GetCookies(new Uri(SteamLibrary._mainsite));

                for (int i = 0; i < stcook.Count; i++)
                {
                    if (stcook[i].Name.Contains("steamLogin"))
                    {
                        return true;
                    }
                }
                return false;
            }

            public SteamLibrary.StrParam ParseItemPrice(string name, string gameid, CookieContainer cookieCont)
            {
                string lowest = "0";
                string median = "0";

                try
                {
                    var priceOver = JsonConvert.DeserializeObject<SteamLibrary.PriceOverview>(SendGet
                        (string.Format(SteamLibrary.priceOverview, Currencies.GetCode(),
                        gameid, name), cookieCont, false, true));

                    if (priceOver.Success)
                    {
                        lowest = priceOver.Lowest.Split(';')[1];
                        median = priceOver.Median.Split(';')[1];
                    }
                }
                catch
                {
                    median = "Error";
                }

                return new SteamLibrary.StrParam(lowest, median);
            }

            public string ParseSearchRes(string content, List<SteamLibrary.SearchItem> lst, CookieContainer cookieCont)
            {
                lst.Clear();
                string totalFind = "0";

                try
                {
                    var searchJS = JsonConvert.DeserializeObject<SteamLibrary.SearchBody>(content);

                    if (searchJS.Success)
                    {
                        totalFind = searchJS.TotalCount;

                        //content = File.ReadAllText(@"C:\dollar2.html");
                        MatchCollection matches = Regex.Matches(searchJS.HtmlRes, 
                            "(?<=market_listing_row_link\" href)(.*?)(?<=</a>)", 
                            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);

                        if (matches.Count != 0)
                        {

                            foreach (Match match in matches)
                            {
                                string currmatch = match.Groups[1].Value;

                                //Fix for Steam update 5/01/14 4:00 PM PST
                                string ItemUrl = Regex.Match(currmatch, "(?<==\")(.*)(?=\" id)").ToString();

                                string[] input = ItemUrl.Split('/');
                                string hash = input[6];

                                string ItemQuan = Regex.Match(currmatch, "(?<=num_listings_qty\">)(.*)(?=</span>)").ToString();

                                //Fix for Steam update 3/26/14 4:00 PM PST
                                string ItemPrice = Regex.Match(currmatch, 
                                    "(?<=<span style=\"color:)(.*)(?=<div class=\"market_listing_right_cell)", 
                                    RegexOptions.Singleline).ToString();

                                if (Currencies.NotSet)
                                {
                                    Currencies.GetType(ItemPrice);
                                    //If not loggen in then
                                    ItemPrice = Regex.Replace(ItemPrice, Currencies.GetAscii(), string.Empty);
                                    //currLst.NotSet = true;
                                }
                                else
                                {
                                    ItemPrice = Regex.Replace(ItemPrice, Currencies.GetAscii(), string.Empty);
                                }

                                ItemPrice = Regex.Replace(ItemPrice, @"[^\d\,\.]+", string.Empty);

                                //Fix fot Steam update 3/26/14 4:00 PM PST
                                string ItemName = Regex.Match(currmatch, 
                                    "(?<=listing_item_name\" style=\"color:)(.*)(?=</span>)").ToString();

                                ItemName = ItemName.Remove(0, ItemName.IndexOf(">") + 1);

                                string ItemGame = Regex.Match(currmatch, "(?<=game_name\">)(.*)(?=</span>)").ToString();
                               
                                string Median = "0";

                                try
                                {
                                    string gameid = SteamLibrary.GetApp(ItemGame).AppID;
                                    Median = ParseItemPrice(hash, gameid, cookieCont).P2;

                                    if (Median.Equals("Error"))
                                    {
                                        Median = "0";
                                    }
                                }
                                catch { }

                                string ItemImg = Regex.Match(currmatch, "(?<=net/economy/image/)(.*)(/62fx62f)", 
                                    RegexOptions.Singleline).ToString();

                                SteamLibrary.SearchItem item = new SteamLibrary.SearchItem(ItemName, ItemGame, ItemUrl, ItemQuan,
                                    ItemPrice, ItemImg, Median);

                                item.Hashname = hash;

                                lst.Add(item);
                            }
                        }
                        else
                        {
                            MessageBox.Show("No item founded!", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }                        
                    }
                }
                catch (Exception e)
                {
                    SteamLibrary.AddtoLog(e.Message);
                    MessageBox.Show("Error parsing search results.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return totalFind;
            }            

            public int ParseInventory(string content)
            {
                InventList.Clear();

                try
                {
                    var rgDescr = JsonConvert.DeserializeObject<SteamLibrary.InventoryData>(content);

                    foreach (SteamLibrary.InvItem prop in rgDescr.myInvent.Values)
                    {
                        var ourItem = rgDescr.invDescr[prop.classid + "_" + prop.instanceid];

                        //parse cost by url (_lists + 753/ + ourItem.MarketName)
                        string price = "0";

                        if (!ourItem.Marketable)
                        {
                            price = "1";
                        }
                            
                        //fix for special symbols in Item Name
                        string markname = string.Empty;

                        if ((ourItem.MarketName == null) && (ourItem.Name == string.Empty))
                        {
                            ourItem.Name = ourItem.SimpleName;
                            ourItem.MarketName = ourItem.SimpleName;
                        }

                        //BattleBlock Theater Fix
                        markname = Uri.EscapeDataString(ourItem.MarketName);
                        string pageLnk = string.Format("{0}/{1}/{2}", SteamLibrary._lists, ourItem.AppId, markname);

                        InventList.Add(new SteamLibrary.InventItem(prop.assetid, ourItem.Name, ourItem.Type, 
                            price, "0", ourItem.IconUrl, ourItem.MarketName, false, ourItem.Marketable, pageLnk));
                    }
                }
                catch (Exception e)
                {
                    SteamLibrary.AddtoLog(e.Message);
                }

                return InventList.Count;
            }

            public byte ParseLotList(string content, string myUserId, List<SteamLibrary.BuyItem> lst, bool full, 
                bool ismain, bool IgnoreWarn)
            {
                lst.Clear();

                //Smart ass!
                if (/*Main.isHTML &&*/ismain)
                {
                    string jsonAssets = Regex.Match(content, @"(?<=g_rgAssets \= )(.*)(?=;
	var g_rgCurrency)", RegexOptions.Singleline).ToString();

                    if (jsonAssets == string.Empty)
                        return 6;

                    string jsonListInfo = Regex.Match(content, @"(?<=g_rgListingInfo \= )(.*)(?=;
	var g_plotPriceHistory)", RegexOptions.Singleline).ToString();

                    content = "{" + string.Format(SteamLibrary.buildJson, jsonListInfo, jsonAssets) + "}";
                }
                else
                {
                    if (content == string.Empty)
                    {
                        //Content empty
                        return 0;
                    }
                    else if (content == "403")
                    {
                        //403 Forbidden
                        return 5;
                    }
                    else if (content.Length < 40)
                    {
                        //Move along
                        return 8;
                    }
                    else if (content[0] != '{')
                    {
                        //Json is not valid
                        return 2;
                    }
                }
                try
                {
                    //"success":false
                    if (content.Substring(11, 1) == "f")
                        return 1;

                    var pageJS = JsonConvert.DeserializeObject<SteamLibrary.PageBody>(content);

                    if (pageJS.Listing.Count != 0)
                    {

                        foreach (SteamLibrary.ListingInfo ourItem in pageJS.Listing.Values)
                        {
                            var ourItemInfo = pageJS.Assets[ourItem.asset.appid][ourItem.asset.contextid][ourItem.asset.id];

                            bool isNull = false;
                            if (ourItem.userId == myUserId)
                            {
                                continue;
                            }
                            if ((IgnoreWarn) && (ourItemInfo.warnings != null))
                            {
                                //Renamed Item or Descriprtion
                                SteamLibrary.AddtoLog(string.Format("{0}: {1}", ourItemInfo.name, ourItemInfo.warnings.ToString()));
                                continue;
                            }
                            if (ourItem.price != 0)
                            {
                                lst.Add(new SteamLibrary.BuyItem(ourItem.listingid, ourItem.price, ourItem.fee, 
                                    new SteamLibrary.AppType(ourItem.asset.appid, ourItem.asset.contextid), ourItemInfo.name));
                                isNull = false;
                            }
                            else
                            {
                                isNull = true;
                            }
                            if (!full && !isNull)
                                return 7;
                        }
                    }
                    else return 1;
                }
                catch (Exception e)
                {
                    //Parsing fail
                    SteamLibrary.AddtoLog("Err Source: " + e.Message);
                    return 3;
                }

                if (lst.Count == 0)
                    return 0;
                else
                    //Fine!
                    return 7;
            }

            public int ParseOnSale(string content)
            {
                InventList.Clear();
                string parseBody = Regex.Match(content, "(?<=section market_home_listing_table\">)(.*)(?=<div id=\"tabContentsMyMarketHistory)", 
                    RegexOptions.Singleline).ToString();

                MatchCollection matches = Regex.Matches(parseBody, "(?<=market_recent_listing_row listing_)(.*?)(?=	</div>\r\n</div>)", 
                    RegexOptions.Singleline);

                if (matches.Count != 0)
                {
                    foreach (Match match in matches)
                    {
                        string currmatch = match.Groups[1].Value;

                        string ImgLink = Regex.Match(currmatch, "(?<=economy/image/)(.*)(?=/38fx38f)").ToString();

                        //If you need:
                        //string assetid = Regex.Match(currmatch, "(?<='mylisting', ')(.*)(?=\" class=\"item_market)").ToString();
                        //assetid = assetid.Substring(assetid.Length - 11, 9); 

                        string listId = Regex.Match(currmatch, "(?<=mylisting_)(.*)(?=_image\" src=)").ToString();

                        string appidRaw = Regex.Match(currmatch, "(?<=market_listing_item_name_link)(.*)(?=</a></span>)").ToString();
                        string pageLnk = Regex.Match(appidRaw, "(?<=href=\")(.*)(?=\">)").ToString();

                        string captainPrice = Regex.Match(currmatch, @"(?<=>
						)(.*)(?=					</span>
					<br>)", RegexOptions.Singleline).ToString();

                        captainPrice = SteamLibrary.GetSweetPrice(Regex.Replace(captainPrice, 
                            Currencies.GetAscii(), string.Empty).Trim());

                        captainPrice.Insert(captainPrice.Length - 2, ".");

                        string[] LinkName = Regex.Match(currmatch, "(?<=_name_link\" href=\")(.*)(?=</a></span><br/>)").
                            ToString().Split(new string[] { "\">" }, StringSplitOptions.None);

                        string ItemType = Regex.Match(currmatch, "(?<=_listing_game_name\">)(.*)(?=</span>)").ToString();

                        InventList.Add(new SteamLibrary.InventItem(listId, LinkName[1], ItemType, captainPrice, "0", ImgLink, 
                            string.Empty, true, true, pageLnk));
                    }
                }
                //else
                //TODO. Add correct error processing
                //MessageBox.Show(Strings.OnSaleErr, Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return matches.Count;
            }

            #endregion
        }

        #endregion

    }
}
