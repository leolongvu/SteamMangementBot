using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;

namespace SteamMarketBot
{
    class SteamAutoFunction
    {
        /// <summary>
        /// Provide function to communicats with Steam from client.
        /// </summary>

        string lastSrch = "";

        string linkTxt = "";

        string advancelinkTxt = "";

        public static string MyUserID = "";

        public string UserName = "";

        public string Password = "";

        public static bool LoginProcess { get; set; }

        public static bool Logged { get; set; }

        public static bool IsRemove { get; set; }

        public static bool LoadOnSale { get; set; }

        private SteamParsing.SteamSite SteamParseSite = new SteamParsing.SteamSite();

        public CookieContainer cookieCont = new CookieContainer();
        
        public event eventDelegate delegMessage;

        public static SteamLibrary.SearchPagePos sppos;

        //List constructors:

        public static List<SteamLibrary.BuyItem> BuyList = new List<SteamLibrary.BuyItem>();
        public static List<SteamLibrary.SearchItem> SearchList = new List<SteamLibrary.SearchItem>();
        public static List<SteamLibrary.TrackingItem> TrackingList = new List<SteamLibrary.TrackingItem>();
        public static List<SteamLibrary.ItemToSell> SellingList = new List<SteamLibrary.ItemToSell>();

        //End.

        private BackgroundWorker LoginThread = new BackgroundWorker();
        private BackgroundWorker SearchThread = new BackgroundWorker();
        private BackgroundWorker BuyThread = new BackgroundWorker();
        private BackgroundWorker InventoryThread = new BackgroundWorker();
        private BackgroundWorker LoadThread = new BackgroundWorker();
        private BackgroundWorker SellThread = new BackgroundWorker();

        public SteamAutoFunction()
        {
            LoginThread.WorkerSupportsCancellation = true;
            LoginThread.DoWork += new DoWorkEventHandler(LoginProgress);

            SearchThread.WorkerSupportsCancellation = true;
            SearchThread.DoWork += new DoWorkEventHandler(SearchProgress);

            BuyThread.WorkerSupportsCancellation = true;
            BuyThread.DoWork += new DoWorkEventHandler(BuyProgress);

            InventoryThread.WorkerSupportsCancellation = true;
            InventoryThread.DoWork += new DoWorkEventHandler(InventoryProgress);

            LoadThread.WorkerSupportsCancellation = true;
            LoadThread.DoWork += new DoWorkEventHandler(LoadTrackList);

            SellThread.WorkerSupportsCancellation = true;
            SellThread.DoWork += new DoWorkEventHandler(SellProgress);
        }

        #region Run Async

        public void LoginThreadExcute()
        {
            if (LoginThread.IsBusy != true)
            {
                LoginThread.RunWorkerAsync();
            }
        }

        public void SearchThreadExcute()
        {
            if (SearchThread.IsBusy != true)
            {
                SearchThread.RunWorkerAsync();
            }
        }

        public void BuyThreadExcute(string link, bool BuyNow, bool IgnoreWarn, bool isTrack)
        {
            object[] parameters = new object[] { link, BuyNow, IgnoreWarn, isTrack };

            if (BuyThread.IsBusy != true)
            {
                BuyThread.RunWorkerAsync(parameters);
            }
        }

        public void InventoryThreadExcute(int invApp)
        {
            int[] parameter = new int[] { invApp };

            if (InventoryThread.IsBusy != true)
            {
                InventoryThread.RunWorkerAsync(parameter);
            }
        }

        public void LoadThreadExcute(string userName, int interval)
        {
            object[] parameter = new object[] { userName, interval };

            if (LoadThread.IsBusy != true)
            {
                LoadThread.RunWorkerAsync(parameter);
            }
        }

        public void SellThreadExcute()
        {
            if (SellThread.IsBusy != true)
            {
                SellThread.RunWorkerAsync();
            }
        }

        #endregion

        #region Delegate methods

        public void doMessage(flag myflag, int searchId, object message, bool isMain)
        {
            try
            {
                if (delegMessage != null)
                {
                    Control target = delegMessage.Target as Control;

                    if (target != null && target.InvokeRequired)
                    {
                        target.Invoke(delegMessage, new object[] { this, message, searchId, myflag, isMain });
                    }
                    else
                    {
                        delegMessage(this, message, searchId, myflag, isMain);
                    }
                }
            }
            catch (Exception e)
            {
                SteamLibrary.AddtoLog(e.Message);
            }
        }

        #endregion

        #region Login
        
        //"RSA SecurID" for Steam
        public SteamLibrary.RespRSA GetRSA(string userName, CookieContainer cookieCont)
        {
            return JsonConvert.DeserializeObject<SteamLibrary.RespRSA>(SteamParsing.SendPost("username=" + userName,
                SteamLibrary._getrsa, SteamLibrary._ref, cookieCont, true));
        }

        public void LoginProgress(object sender, DoWorkEventArgs e)
        {
            LoginProcess = true;
            Logged = false;

            var accInfo = SteamParseSite.GetNameBalance(cookieCont);

            if (accInfo != null)
            {
                doMessage(flag.Already_logged, 0, accInfo, true);
                LoginProcess = false;
                Logged = true;
                return;
            }

            string mailCode = string.Empty;
            string guardDesc = string.Empty;
            string capchaId = string.Empty;
            string capchaTxt = string.Empty;
            string mailId = string.Empty;

            //Login cycle
        begin:

            var rRSA = GetRSA("toshigawa_tori", cookieCont);

            if (rRSA == null)
            {
                SteamLibrary.AddtoLog("Network Problem");
                doMessage(flag.Login_cancel, 0, "Network Problem", true);
                LoginProcess = false;
                return;
            }

            string finalpass = SteamLibrary.EncryptPassword(Password, rRSA.Module, rRSA.Exponent);

            string MainReq = string.Format(SteamLibrary.loginReq, finalpass, UserName, mailCode, guardDesc, capchaId,
                                                                          capchaTxt, mailId, rRSA.TimeStamp);

            string BodyResp = SteamParsing.SendPost(MainReq, SteamLibrary._dologin, SteamLibrary._ref, cookieCont, true);

            //Checking login problem
            if (BodyResp.Contains("message"))
            {
                var rProcess = JsonConvert.DeserializeObject<SteamLibrary.RespProcess>(BodyResp);

                //Checking Incorrect Login
                if (rProcess.Message.Contains("Incorrect"))
                {
                    SteamLibrary.AddtoLog("Incorrect login");
                    doMessage(flag.Login_cancel, 0, "Incorrect login", true);
                    LoginProcess = false;
                    return;
                }
                else
                {
                    //Login correct, checking message type...
                    Dialog guardCheckForm = new Dialog();

                    if ((rProcess.isCaptcha) && (rProcess.Message.Contains("humanity")))
                    {
                        //Verifying humanity, loading capcha
                        guardCheckForm.capchgroupEnab = true;
                        guardCheckForm.codgroupEnab = false;

                        string newcap = SteamLibrary._capcha + rProcess.Captcha_Id;
                        SteamParsing.SteamSite.loadImg(newcap, guardCheckForm.capchImg, false, false);
                    }
                    else
                        if (rProcess.isEmail)
                        {
                            //Steam guard wants email code
                            guardCheckForm.capchgroupEnab = false;
                            guardCheckForm.codgroupEnab = true;
                        }
                        else
                        {
                            //Whoops!
                            goto begin;
                        }

                    //Re-assign main request values
                    if (guardCheckForm.ShowDialog() == DialogResult.OK)
                    {
                        mailCode = guardCheckForm.MailCode;
                        guardDesc = guardCheckForm.GuardDesc;
                        capchaId = rProcess.Captcha_Id;
                        capchaTxt = guardCheckForm.capchaText;
                        mailId = rProcess.Email_Id;
                        guardCheckForm.Dispose();
                    }
                    else
                    {
                        SteamLibrary.AddtoLog("Dialog has been cancelled!");
                        doMessage(flag.Login_cancel, 0, "Dialog has been cancelled!", true);
                        e.Cancel = true;
                        Logged = false;
                        LoginProcess = false;
                        guardCheckForm.Dispose();
                        return;

                    }

                    goto begin;
                }
            }
            else
            {
                //No Messages, Success!
                var rFinal = JsonConvert.DeserializeObject<SteamLibrary.RespFinal>(BodyResp);

                if (rFinal.Success && rFinal.isComplete)
                {
                    //Okay
                    var accInfo2 = SteamParseSite.GetNameBalance(cookieCont);

                    doMessage(flag.Login_success, 0, accInfo2, true);

                    Logged = true;
                    SteamLibrary.AddtoLog("Login Success");
                }
                else
                {
                    //Fail
                    goto begin;
                }
            }

            LoginProcess = false;          
        }

        public void CancelLogin()
        {
            if (LoginThread.IsBusy == true)
            {
                LoginThread.CancelAsync();
            }
        }

        public void Logout()
        {
            ThreadStart threadStart = delegate()
            {
                SteamParsing.SendGet(SteamLibrary._logout, cookieCont, false, true);
                doMessage(flag.Logout_, 0, string.Empty, true);
                Logged = false;
            };
            Thread pTh = new Thread(threadStart);
            pTh.IsBackground = true;
            pTh.Start();
        }

        #endregion

        private void LoadTrackList(object sender, DoWorkEventArgs e)
        {
            object[] parameters = e.Argument as object[];

            string username = Convert.ToString(parameters[0]);
            int interval = Convert.ToInt32(parameters[1]);

            if (File.Exists(Application.StartupPath + @"\Tracks\" + username + ".ini"))
            {
                TrackingList.Clear();

                TextReader config_reader = new StreamReader(Application.StartupPath + @"\Tracks\" + username + ".ini");
                string input;
                int i = 0;
                while ((input = config_reader.ReadLine()) != "//")
                {
                    if (input.Equals("[Item " + i + "]"))
                    {
                        string game = config_reader.ReadLine();
                        string name = config_reader.ReadLine();
                        string lp = config_reader.ReadLine();
                        string mp = config_reader.ReadLine();
                        string buy = config_reader.ReadLine();
                        string sell = config_reader.ReadLine();
                        string bper = config_reader.ReadLine();
                        string sper = config_reader.ReadLine();
                        string pur = config_reader.ReadLine();
                        string link = config_reader.ReadLine();
                        string purprice = config_reader.ReadLine();
                        string purdate = config_reader.ReadLine();
                        string hashname = config_reader.ReadLine();

                        double lowp1 = Convert.ToSingle(lp.Split(' ')[1]);
                        double medp1 = Convert.ToSingle(mp.Split(' ')[1]);
                        int bpe = Convert.ToInt32(bper);
                        int spe = Convert.ToInt32(sper);
                        double purp = 0;
                        if (!purprice.Equals(""))
                        {
                            purp = Convert.ToSingle(purprice.Split(' ')[1]);
                        }
                        bool b = false; bool s = false; bool p = false;

                        if (buy.Equals("x"))
                        {
                            b = true;
                        }
                        if (sell.Equals("x"))
                        {
                            s = true;
                        }
                        if (pur.Equals("x"))
                        {
                            p = true;
                        }

                        SteamLibrary.TrackingItem item = new SteamLibrary.TrackingItem(name, game, lowp1, 
                            medp1, bpe, spe, b, s, p, link, purp);

                        item.hashname = hashname;
                        item.purchasedate = purdate;

                        item.index = i;

                        TrackingList.Add(item);

                        item.Refresh = new System.Timers.Timer();
                        item.Refresh.Elapsed += new System.Timers.ElapsedEventHandler(delegate { TrackProgress(item); });
                        item.Refresh.Interval = interval;
                        item.Refresh.AutoReset = true;

                        if (p == false)
                        {                           
                            item.Refresh.Start();
                        }

                        i++;
                    }
                }
                config_reader.Close();

                doMessage(flag.Load_progress, 0, "Loading done!", true);
            }
            else
            {
                MessageBox.Show("Error loading tracks!", "Error");
            }
        }

        #region Search Items

        public static int SearchResCount = 20;

        public void DoSearch(byte type, int searchResCount, string searchName)
        {
            try
            {
                SearchResCount = searchResCount;
            }
            catch
            {
                SearchResCount = 20;
            }
            switch (type)
            {
                case 0:
                    sppos = new SteamLibrary.SearchPagePos(0, 1);
                    lastSrch = searchName;
                    break;
                case 1:
                    if (sppos.CurrentPos < sppos.PageCount)
                        sppos.CurrentPos += SearchResCount;
                    else sppos.CurrentPos = 1;

                    break;
                case 2:
                    if (sppos.CurrentPos > SearchResCount)
                        sppos.CurrentPos -= SearchResCount;
                    else sppos.CurrentPos = sppos.PageCount;
                    break;
                default:
                    break;
            }

            //search/render/?query={0}&start={1}&count={2}
            linkTxt = string.Format(SteamLibrary._search, lastSrch, sppos.CurrentPos - 1, SearchResCount);
            SearchThreadExcute();
        }

        private void SearchProgress(object sender, DoWorkEventArgs e)
        {
            doMessage(flag.Search_success, 0, SteamParseSite.ParseSearchRes(SteamParsing.SendGet(linkTxt, cookieCont, 
                false, true), SearchList, cookieCont), true);
        }

        public void DoAdvanceSearch(byte type, int searchResCount, string searchName, bool isGame, string game, 
            bool isMin, string min, bool isMax, string max)
        {            
            switch (type)
            {
                case 0:
                    sppos = new SteamLibrary.SearchPagePos(0, 1);
                    lastSrch = searchName;
                    break;
                case 1:
                    if (sppos.CurrentPos < sppos.PageCount)
                        sppos.CurrentPos += searchResCount;
                    else sppos.CurrentPos = 1;

                    break;
                case 2:
                    if (sppos.CurrentPos > searchResCount)
                        sppos.CurrentPos -= searchResCount;
                    else sppos.CurrentPos = sppos.PageCount;
                    break;
                default:
                    break;
            }

            //search/render/?appid={0}&query={0}&start={1}&count={2}
            advancelinkTxt = string.Format(SteamLibrary._adsearch, SteamLibrary.GetApp(game).AppID, lastSrch, sppos.CurrentPos - 1, searchResCount);
            ThreadPool.QueueUserWorkItem(new WaitCallback(AdvanceSearchProgress), new object[] 
            { isGame, game, isMin, min, isMax, max, searchResCount });
        }

        private void AdvanceSearchProgress(object state)
        {
            object[] parameters = state as object[]; 

            bool isGame = (bool)parameters[0];
            string game = (string)parameters[1];
            bool isMin = (bool)parameters[2];
            string min = (string)parameters[3];
            bool isMax = (bool)parameters[4];
            string max = (string)parameters[5];
            int countRes = (int)parameters[6];

            string found = ParseAdvanceSearchRes(SteamParsing.SendGet(advancelinkTxt, cookieCont,
                false, true), SearchList, cookieCont, isGame, game, isMin, min, isMax, max, countRes);
        }

        public static bool searchDone = true;

        public string ParseAdvanceSearchRes(string content, List<SteamLibrary.SearchItem> lst, CookieContainer cookieCont,
                bool isGame, string game, bool isMin, string min, bool isMax, string max, int countRes)
        {           
            if (searchDone == true)
            {
                lst.Clear();
            }
            searchDone = false;
            string totalFind = "0";

            try
            {
                var searchJS = JsonConvert.DeserializeObject<SteamLibrary.SearchBody>(content);

                if (searchJS.Success)
                {
                    totalFind = searchJS.TotalCount;
                    sppos.PageCount = Convert.ToInt32(totalFind) / SearchResCount; 

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

                            if (SteamParsing.Currencies.NotSet)
                            {
                                SteamParsing.Currencies.GetType(ItemPrice);
                                //If not loggen in then
                                ItemPrice = Regex.Replace(ItemPrice, SteamParsing.Currencies.GetAscii(), string.Empty);
                                //currLst.NotSet = true;
                            }
                            else
                            {
                                ItemPrice = Regex.Replace(ItemPrice, SteamParsing.Currencies.GetAscii(), string.Empty);
                            }

                            ItemPrice = Regex.Replace(ItemPrice, @"[^\d\,\.]+", string.Empty);

                            bool pass = false;

                            if (isMin || isMax)
                            {
                                if (isMin && isMax)
                                {
                                    if (Convert.ToSingle(ItemPrice) > Convert.ToSingle(min) && (Convert.ToSingle(ItemPrice) < Convert.ToSingle(max)))
                                    {
                                        pass = true;
                                    }
                                }
                                else if (isMin)
                                {
                                    if (Convert.ToSingle(ItemPrice) > Convert.ToSingle(min))
                                    {
                                        pass = true;
                                    }
                                }
                                else
                                {
                                    if (Convert.ToSingle(ItemPrice) < Convert.ToSingle(max))
                                    {
                                        pass = true;
                                    }
                                }
                            }
                            else
                            {
                                pass = true;
                            }

                            if (pass)
                            {
                                //Fix fot Steam update 3/26/14 4:00 PM PST
                                string ItemName = Regex.Match(currmatch,
                                    "(?<=listing_item_name\" style=\"color:)(.*)(?=</span>)").ToString();

                                ItemName = ItemName.Remove(0, ItemName.IndexOf(">") + 1);

                                string ItemGame = Regex.Match(currmatch, "(?<=game_name\">)(.*)(?=</span>)").ToString();

                                if (isGame)
                                {
                                    if (ItemGame.Equals(game))
                                    {
                                        string Median = "0";

                                        try
                                        {
                                            string gameid = SteamLibrary.GetApp(ItemGame).AppID;
                                            Median = SteamParseSite.ParseItemPrice(hash, gameid, cookieCont).P2;

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
                                    string Median = "0";

                                    try
                                    {
                                        string gameid = SteamLibrary.GetApp(ItemGame).AppID;
                                        Median = SteamParseSite.ParseItemPrice(hash, gameid, cookieCont).P2;

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

            if (lst.Count < countRes)
            {
                DoAdvanceSearch(1, countRes, lastSrch, isGame, game, isMin, min, isMax, max);
            }
            else
            {                
                doMessage(flag.Advance_search_success, 0, totalFind, true);
            }
           
            return totalFind;
        }

        #endregion

        #region Track Items

        public void DoTrack(string name, string game,string hashname, int buyPer, int sellPer, int interval)
        {
            SteamLibrary.TrackingItem item = new SteamLibrary.TrackingItem(name, game, 0, 0, buyPer, sellPer, false, false, false, "", 0);
           
            item.hashname = hashname;

            string gameid = SteamLibrary.GetApp(game).AppID;
            try
            {
                SteamLibrary.StrParam price = SteamParseSite.ParseItemPrice(hashname, gameid, cookieCont);

                item.StartPrice = Convert.ToSingle(price.P1);
                item.MedianPrice = Convert.ToSingle(price.P2);
            }
            catch { }

            item.Link = "http://steamcommunity.com/market/listings/" + SteamLibrary.GetApp(game).AppID + "/" + name;

            int index = TrackingList.Count;

            TrackingList.Add(item);

            item.index = index;

            doMessage(flag.Add_track, 0, item, true);

            item.Refresh = new System.Timers.Timer();
            item.Refresh.Elapsed += new System.Timers.ElapsedEventHandler(delegate { TrackProgress(item); });
            item.Refresh.Interval = interval;
            item.Refresh.AutoReset = true;
            item.Refresh.Start();
        }

        public static double pricecheck = 0;

        private void TrackProgress(SteamLibrary.TrackingItem item)
        {
            Stopwatch stop = new Stopwatch();

            string gameid = SteamLibrary.GetApp(item.Game).AppID;
            try
            {
                stop.Start();
                SteamLibrary.StrParam price = SteamParseSite.ParseItemPrice(item.hashname, gameid, cookieCont);
                item.StartPrice = Convert.ToSingle(price.P1);                
                item.PreMedianPrice = item.MedianPrice;
                item.MedianPrice = Convert.ToSingle(price.P2);
                stop.Stop();
            }
            catch { }
            try
            {
                if (((item.StartPrice / item.MedianPrice * 100) < item.BuyPercent) && (item.Buy) && (item.StartPrice > 0) && (item.MedianPrice / item.MedianPrice < 1.2))
                {
                    pricecheck = item.StartPrice;
                    BuyThreadExcute(item.Link, true, true, true);
                }
            }
            catch { }

            doMessage(flag.Track_progress, Convert.ToInt32(stop.ElapsedMilliseconds), item, true);
        }

        #endregion

        #region Buy Items

        public void BuyProgress(object sender, DoWorkEventArgs e)
        {
            object[] parameters = e.Argument as object[]; 

            string link = Convert.ToString(parameters[0]);
            bool BuyNow = Convert.ToBoolean(parameters[1]);
            bool IgnoreWarn = Convert.ToBoolean(parameters[2]);
            bool isTrack = Convert.ToBoolean(parameters[3]);

            try
            {
                string sessid = SteamParseSite.GetSessId(cookieCont);

                string url = link;

                if (BuyNow)
                {
                    SteamParseSite.ParseLotList(SteamParsing.SendGet(url, cookieCont, false, true), 
                        MyUserID, BuyList, false, true, IgnoreWarn);

                    if (BuyList.Count == 0)
                    {
                        doMessage(flag.Error_scan, 0, "0", true);
                    }
                    else
                    {
                        double totalPrice = BuyList[0].Price + BuyList[0].Fee;

                        if (isTrack)
                        {
                            if (totalPrice / 100 == pricecheck)
                            {
                                string totalStr = Convert.ToString(totalPrice);

                                var buyresp = BuyItem(cookieCont, sessid, BuyList[0].ListingId, link, BuyList[0].Price.ToString(),
                                    BuyList[0].Fee.ToString(), totalStr);

                                BuyNow = false;

                                if (buyresp.Succsess)
                                {
                                    doMessage(flag.Success_buy, 0, buyresp.Mess + ";" + BuyList[0].ItemName, true);
                                }
                                else
                                {
                                    doMessage(flag.Error_buy, 0, buyresp.Mess, true);
                                }
                            }            
                        }
                        else
                        {
                            string totalStr = Convert.ToString(totalPrice);

                            var buyresp = BuyItem(cookieCont, sessid, BuyList[0].ListingId, link, BuyList[0].Price.ToString(),
                                BuyList[0].Fee.ToString(), totalStr);

                            BuyNow = false;

                            if (buyresp.Succsess)
                            {
                                doMessage(flag.Success_buy, 0, buyresp.Mess + ";" + BuyList[0].ItemName, true);
                            }
                            else
                            {
                                doMessage(flag.Error_buy, 0, buyresp.Mess, true);
                            }
                        }
                    }
                    return;
                }
            }
            catch (Exception exc)
            {
                SteamLibrary.AddtoLog(exc.Message);
            }
            finally
            {
                //CancelScan();
            }
        }

        private SteamLibrary.BuyResponse BuyItem(CookieContainer cock, string sessid, string itemId, string link, 
            string subtotal, string fee, string total)
        {
            string data = string.Format(SteamLibrary.buyReq, sessid, subtotal, fee, total, SteamParsing.Currencies.GetCode(), "1");

            //buy
            //29.08.2013 Steam Update Issue!
            //FIX: using SSL - https:// in url
            string buyres = SteamParsing.SendPost(data, SteamLibrary._blist + itemId, link, cookieCont, true);

            //testing purposes
            //string buyres = File.ReadAllText(@"C:\x.txt");

            try
            {
                if (buyres.Contains("message"))
                {
                    //Already buyed!
                    var ErrBuy = JsonConvert.DeserializeObject<SteamLibrary.InfoMessage>(buyres);
                    return new SteamLibrary.BuyResponse(false, ErrBuy.Message);
                }
                else

                    if (buyres != string.Empty)
                    {
                        var AfterBuy = JsonConvert.DeserializeObject<SteamLibrary.WalletInfo>(buyres);

                        if (AfterBuy.WalletRes.Success == 1)
                        {
                            string balance = AfterBuy.WalletRes.Balance;
                            balance = balance.Insert(balance.Length - 2, ".");
                            return new SteamLibrary.BuyResponse(true, balance);
                        }
                        else return new SteamLibrary.BuyResponse(false, "UnknownErr");
                    }
                    else return new SteamLibrary.BuyResponse(false, "UnknownErr");
            }
            catch (Exception)
            {
                return new SteamLibrary.BuyResponse(false, "UnknownErr");
            }
        }  

        #endregion

        #region Inventory

        private void InventoryProgress(object sender, DoWorkEventArgs e)
        {
            int[] parameters = e.Argument as int[];

            int invApp = parameters[0];

            int invCount = 0;

            if (!LoadOnSale)
            {
                invCount = SteamParseSite.ParseInventory(SteamParsing.SendGet(string.Format(SteamLibrary._jsonInv, MyUserID, 
                    SteamLibrary.GetAppFromIndex(invApp, true).AppID), cookieCont, false, true));
            }
            else
            {
                invCount = SteamParseSite.ParseOnSale(SteamParsing.SendGet(SteamLibrary._market, cookieCont, false, true));
            }

            if (invCount > 0)
            {
                doMessage(flag.Inventory_Loaded, 0, string.Empty, true);
            }
            else
            {
                doMessage(flag.Inventory_Loaded, 1, string.Empty, true);
            }
        }

        public void RefreshProgress(object state)
        {
            object[] parameters = state as object[];

            SteamLibrary.InventItem item = (SteamLibrary.InventItem)parameters[0];
            int index = (int)parameters[1];

            string[] split = item.PageLink.Split('/');

            SteamLibrary.StrParam priceOver = SteamParseSite.ParseItemPrice(split[7], split[6], cookieCont);

            item.MedianPrice = priceOver.P2;

            if (item.BuyPrice == "0")
            {
                item.BuyPrice = SteamLibrary.GetSweetPrice(item.MedianPrice);
            }
                
            doMessage(flag.InvPrice, index, item, true);           
        }

        #endregion

        #region Sell Items

        public void SellProgress(object sender, DoWorkEventArgs e)
        {
            var count = SellingList.Count;

            if (count != 0)
            {
                int incr = (100 / count);

                /*bool isSleep = false;

                if (count > 0)
                {
                    isSleep = true;
                }

                Random random = new Random();
                int min = sellDelay / 2;
                int max = sellDelay * 2;*/

                for (int i = 0; i < count; i++)
                {
                    if (IsRemove)
                    {
                        var req = "sessionid=" + SteamParseSite.GetSessId(cookieCont);
                        SteamParsing.SendPost(req, SteamLibrary.removeSell + SellingList[i].AssetId, SteamLibrary._market, cookieCont, false);
                    }
                    else
                    {
                        if (!SellingList[i].Price.Equals("None"))
                        {
                            var req = string.Format(SteamLibrary.sellReq, SteamParseSite.GetSessId(cookieCont), SellingList[i].App.AppID,
                                SellingList[i].App.Context, SellingList[i].AssetId, SellingList[i].Price);

                            SteamParsing.SendPost(req, SteamLibrary._sellitem, SteamLibrary._market, cookieCont, false);
                        }
                    }

                    doMessage(flag.Sell_progress, 0, (incr * (i + 1)).ToString(), true);

                    /*if ((isSleep) && (i != count - 1))
                    {
                        if (isDelayRand)
                        {
                            Thread.Sleep(random.Next(min, max));
                        }
                        else
                        {
                            Thread.Sleep(sellDelay);
                        }
                    }*/
                }

                doMessage(flag.Items_Sold, 0, string.Empty, true);
            }
            else
            {
                doMessage(flag.Items_Sold, 1, string.Empty, true);
            }
        }

        #endregion
    }
}
