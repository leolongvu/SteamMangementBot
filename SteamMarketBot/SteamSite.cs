/*using System;
using System.ComponentModel;
using System.Net;
using System.Windows.Forms;
using System.Threading;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SteamMarketBot
{
    partial class SteamSite
    {
        public string UserName { set; get; }
        public string Password { set; get; }

        public int scanID { set; get; }
        public int mainDelay { get; set; }
        public int resellDelay { get; set; }

        public string linkTxt { set; get; }

        public bool Logged { set; get; }
        public bool LoginProcess { set; get; }
        public bool scaninProg { set; get; }

        public bool BuyNow { set; get; }
        public bool LoadOnSale { get; set; }
        public bool isRemove { get; set; }

        public bool IgnoreWarn { get; set; }

        public int invApp { get; set; }

        public bool NotSetHead { get; set; }

        public int sellDelay { get; set; }
        public bool isDelayRand { get; set; }

        public static string myUserId;

        public CookieContainer cookieCont { set; get; }

        //public BindingList<MainScanItem.LogItem> logContainer = new BindingList<MainScanItem.LogItem>();

        public saveTab scanInput { set; get; }

        public Semaphore Sem = new Semaphore(0, 1);

        private BackgroundWorker loginThread = new BackgroundWorker();
        private BackgroundWorker scanThread = new BackgroundWorker();

        private BackgroundWorker reqThread = new BackgroundWorker();
        private BackgroundWorker sellThread = new BackgroundWorker();

        private BackgroundWorker getInventory = new BackgroundWorker();
        private BackgroundWorker loadThread = new BackgroundWorker();
        private BackgroundWorker refreshThread = new BackgroundWorker();
        public List<ItemToSell> toSellList = new List<ItemToSell>();
        public List<string> toUnSellList = new List<string>();

        public SteamSite()
        {
            loginThread.WorkerSupportsCancellation = true;
            loginThread.DoWork += new DoWorkEventHandler(loginThread_DoWork);

            scanThread.WorkerSupportsCancellation = true;
            scanThread.DoWork += new DoWorkEventHandler(scanThread_DoWork);

            reqThread.WorkerSupportsCancellation = true;
            reqThread.DoWork += new DoWorkEventHandler(reqThread_DoWork);

            getInventory.WorkerSupportsCancellation = true;
            getInventory.DoWork += new DoWorkEventHandler(getInventory_DoWork);

            loadThread.WorkerSupportsCancellation = true;
            loadThread.DoWork += new DoWorkEventHandler(LoadTrackList);

            refreshThread.WorkerSupportsCancellation = true;
            refreshThread.DoWork += new DoWorkEventHandler(refreshThreadGUI);

            /*sellThread.WorkerSupportsCancellation = true;
            sellThread.DoWork += new DoWorkEventHandler(sellThread_DoWork);
        }

        public class AppType
        {
            public string App { set; get; }
            public string Context { set; get; }

            public AppType(string app, string context)
            {
                this.App = app;
                this.Context = context;
            }            
        }

        public class ItemToSell
        {
            public ItemToSell(string assetid, string price)
            {
                this.AssetId = assetid;
                this.Price = price;
            }

            public string AssetId { set; get; }
            public string Price { set; get; }
        }

        public void Login()
        {
            if (loginThread.IsBusy != true)
            {
                loginThread.RunWorkerAsync();
            }
        }

        public void CancelLogin()
        {
            if (loginThread.IsBusy == true)
            {
                loginThread.CancelAsync();
            }
        }

        public void Logout()
        {
            ThreadStart threadStart = delegate()
            {
                SendGet(_logout, cookieCont, false, true);
                doMessage(flag.Logout_, 0, string.Empty, true);
                Logged = false;

                trackinglist.Clear();

            };
            Thread pTh = new Thread(threadStart);
            pTh.IsBackground = true;
            pTh.Start();
        }

        public void ChangeLng(string lang)
        {
            ThreadStart threadStart = delegate()
            {
                SendGet(_lang_chg + lang, cookieCont, false, true);
                doMessage(flag.Lang_Changed, 0, lang, true);
            };
            Thread pTh = new Thread(threadStart);
            pTh.IsBackground = true;
            pTh.Start();
        }

        public void LoadTracks()
        {
            if (loadThread.IsBusy != true)
            {
                loadThread.RunWorkerAsync();
            }
        }

        public void RefeshPrice()
        {
            if (refreshThread.IsBusy != true)
            {
                refreshThread.RunWorkerAsync();
            }
        }

        public void ScanPrices()
        {
            if (scanThread.IsBusy != true)
            {
                scaninProg = true;
                scanThread.RunWorkerAsync();
            }
        }

        public void CancelScan()
        {
            if (scanThread.WorkerSupportsCancellation == true && scanThread.IsBusy)
            {
                scaninProg = false;
                Sem.Release();
                scanThread.CancelAsync();
            }
        }

        public void reqLoad()
        {
            if (reqThread.IsBusy != true)
            {
                reqThread.RunWorkerAsync();
            }
        }

        public void loadInventory()
        {
            if (getInventory.IsBusy != true)
            {
                getInventory.RunWorkerAsync();
            }
        }

        public void ItemSell()
        {
            if (sellThread.IsBusy != true)
            {
                sellThread.RunWorkerAsync();
            }
        }

        public static TrackingItem UpdateItemPrice (TrackingItem item, SteamSite steam)
        {
            try
            {
                string link = string.Format(priceOverview, GetCurrenyID(Main.currencies.GetName()), GetApp(item.Game), item.Name);
                var priceOver = JsonConvert.DeserializeObject<PriceOverview>(steam.SendGet(link, steam.cookieCont, false, true));
                if (priceOver.Success)
                {
                    item.StartPrice = Convert.ToSingle(priceOver.Lowest.Split(';')[1]);
                    item.MedianPrice = Convert.ToSingle(priceOver.Median.Split(';')[1]);
                    item.link = "http://steamcommunity.com/market/listings/" + GetApp(item.Game) + "/" + item.Name;        
                }              
            }
            catch { }

            return item;
        }

        private void getInventory_DoWork(object sender, DoWorkEventArgs e)
        {
            int invCount = 0;

            if (!LoadOnSale)
            {
                invCount = ParseInventory(SendGet(string.Format(_jsonInv, myUserId, GetUrlApp(invApp, true).App),
              cookieCont, false, true));
            }
            else
            {
                invCount = ParseOnSale(SendGet(_market, cookieCont, false, true));
            }

            if (invCount > 0)
                doMessage(flag.Inventory_Loaded, 0, string.Empty, true);
            else
            {
                doMessage(flag.Inventory_Loaded, 1, string.Empty, true);
            }
                
        }

        /*private void sellThread_DoWork(object sender, DoWorkEventArgs e)
        {
            var cunt = toSellList.Count;

            if (cunt != 0)
            {
                var appReq = GetUrlApp(invApp, false);

                int incr = (100 / cunt);

                bool isSleep = false;

                if (cunt > 0)
                    isSleep = true;

                Random random = new Random();
                int min = sellDelay / 2;
                int max = sellDelay * 2;

                for (int i = 0; i < cunt; i++)
                {

                    if (isRemove)
                    {
                        var req = "sessionid=" + GetSessId(cookieCont);
                        SendPost(req, removeSell + toSellList[i].AssetId, _market, false);
                    }
                    else
                    {
                        if (toSellList[i].Price != Strings.None)
                        {
                            var req = string.Format(sellReq, GetSessId(cookieCont), appReq.App, appReq.Context, toSellList[i].AssetId, toSellList[i].Price);

                            SendPost(req, _sellitem, _market, false);
                        }
                    }

                    doMessage(flag.Sell_progress, 0, (incr * (i + 1)).ToString(), true);

                    if ((isSleep) && (i != cunt - 1))
                    {
                        if (isDelayRand)
                        {
                            Thread.Sleep(random.Next(min, max));
                        }
                        else
                            Thread.Sleep(sellDelay);
                    }
                }

                doMessage(flag.Items_Sold, 0, string.Empty, true);
            }
            else
            {
                doMessage(flag.Items_Sold, 1, string.Empty, true);
            }
        }

        private void reqThread_DoWork(object sender, DoWorkEventArgs e)
        {
            doMessage(flag.Search_success, 0, ParseSearchRes(SendGet(linkTxt, cookieCont, false, true), searchList, Main.steam_srch), true);
        }

        public void scanTimerThreadGUI(TrackingItem item)
        {
            Thread.Sleep(1);
            doMessage(flag.Scan_progress, 0, UpdateItemPrice(item, Main.steam_srch), true);
        }

        public void refreshThreadGUI(object sender, DoWorkEventArgs e)
        {
            foreach (InventItem item in inventList)
            {
                if (item.Price == "0")
                {
                    string[] split = item.PageLnk.Split('/');

                    var priceOver = JsonConvert.DeserializeObject<PriceOverview>(SendGet(string.Format(priceOverview, GetCurrenyID(Main.currencies.GetName()), split[6], split[7]), cookieCont, false, true));

                    if (priceOver.Success)
                    {
                        try
                        {
                            item.Price = priceOver.Median.Split(';')[1];
                        }
                        catch { }
                    }
                }                
            }
            
            doMessage(flag.InvPrice, 0, 0, true);
        }

        private RespRSA GetRSA()
        {
            return JsonConvert.DeserializeObject<RespRSA>(SendPost("username=" + UserName, _getrsa, _ref, true));         
        }

        private void LoginProgr(string value)
        {
            doMessage(flag.Rep_progress, 0, value, true);
        }

        private void loginThread_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            LoginProcess = true;
            Logged = false;

            var accInfo = GetNameBalance(cookieCont);

            if (accInfo != null)
            {
                doMessage(flag.Already_logged, 0, accInfo, true);
                doMessage(flag.Rep_progress, 0, "100", true);
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

            if (worker.CancellationPending == true)
                return;

            var rRSA = GetRSA();

            if (rRSA == null)
            {
                Main.AddtoLog("Network Problem");
                doMessage(flag.Login_cancel, 0, "Network Problem", true);
                e.Cancel = true;
                LoginProcess = false;
                return;
            }

            string finalpass = EncryptPassword(Password, rRSA.Module, rRSA.Exponent);

            string MainReq = string.Format(loginReq, finalpass, UserName, mailCode, guardDesc, capchaId,
                                                                          capchaTxt, mailId, rRSA.TimeStamp);
            string BodyResp = SendPost(MainReq, _dologin, _ref, true);

            //Checking login problem
            if (BodyResp.Contains("message"))
            {
                var rProcess = JsonConvert.DeserializeObject<RespProcess>(BodyResp);

                //Checking Incorrect Login
                if (rProcess.Message == "Incorrect login")
                {
                    Main.AddtoLog("Incorrect login");
                    doMessage(flag.Login_cancel, 0, "Incorrect login", true);
                    e.Cancel = true;
                    LoginProcess = false;
                    return;
                }
                else
                {
                    //Login correct, checking message type...
                    Dialog guardCheckForm = new Dialog();

                    if (rProcess.isCaptcha)
                    {
                        //Verifying humanity, loading capcha
                        guardCheckForm.capchgroupEnab = true;
                        guardCheckForm.codgroupEnab = false;

                        string newcap = _capcha + rProcess.Captcha_Id;
                        Main.loadImg(newcap, guardCheckForm.capchImg, false, false);
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
                            //Again!
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
                        Main.AddtoLog("Dialog has been cancelled");
                        doMessage(flag.Login_cancel, 0, "Dialog has been cancelled", true);
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
                var rFinal = JsonConvert.DeserializeObject<RespFinal>(BodyResp);

                if (rFinal.Success && rFinal.isComplete)
                {
                    //Okay
                    var accInfo2 = GetNameBalance(cookieCont);

                    doMessage(flag.Login_success, 0, accInfo2, true);

                    Logged = true;
                    Main.AddtoLog("Login Success");
                }
                else
                {
                    //Fail
                    goto begin;
                }
            }
            LoginProcess = false;
        }       

        public string UrlForRender(string input)
        {
            string url = input;

            int fint = url.IndexOf('?');

            if (fint == -1)
            {
                url += "/render" + Main.jsonAddon;
            }
            else
            {
                //fix
                url = url.Replace("?", "/render" + Main.jsonAddon + "&");
            }
            return url;
        }

        public void scanThread_DoWork(object sender, DoWorkEventArgs e)
        {            
            try
            {
                BackgroundWorker worker = sender as BackgroundWorker;

                string sessid = GetSessId(cookieCont);

                string url = scanInput.Link;

                if (!Main.isHTML)
                {
                    url = UrlForRender(scanInput.Link);
                }               
                if (BuyNow)
                {
                    ParseLotList(SendGet(url, cookieCont, false, true), lotList, false, true);

                    if (lotList.Count == 0)
                    {                       
                        doMessage(flag.Error_scan, scanID, "0", true);
                    }
                    else
                    {
                        string totalStr = Convert.ToString(lotList[0].Price + lotList[0].Fee);                        
                        var buyresp = BuyItem(cookieCont, sessid, lotList[0].ListringId, scanInput.Link, lotList[0].Price.ToString(), lotList[0].Fee.ToString(), totalStr);
                        BuyNow = false;

                        if (buyresp.Succsess)
                        {
                            doMessage(flag.Success_buy, scanID, buyresp.Mess + ";" + lotList[0].ItemName , true);
                        }
                        else
                        {
                            doMessage(flag.Error_buy, scanID, buyresp.Mess, true);
                        }
                    }
                    return;
                }               
            }
            catch (Exception exc)
            {
                Main.AddtoLog(exc.Message);
            }
            finally
            {
                CancelScan();
            }
        }

        public void LoadTrackList(object sender, DoWorkEventArgs e)
        {
            doMessage(flag.Load_progress, 0, 0, true);
        }

        public void SaveTrackList(object sender, DoWorkEventArgs e)
        {
            doMessage(flag.Save_progress, 0, 0, true);
        }

        private BuyResponse BuyItem(CookieContainer cock, string sessid, string itemId, string link, string subtotal, string fee, string total)
        {
            string data = string.Format(buyReq, sessid, subtotal, fee, total, Main.currencies.GetCode(), "1");
     
            //buy
            //29.08.2013 Steam Update Issue!
            //FIX: using SSL - https:// in url
            string buyres = SendPost(data, _blist + itemId, link, true);

            //testing purposes
            //string buyres = File.ReadAllText(@"C:\x.txt");

            try
            {
                if (buyres.Contains("message"))
                {
                    //Already buyed!
                    var ErrBuy = JsonConvert.DeserializeObject<InfoMessage>(buyres);
                    return new BuyResponse(false, ErrBuy.Message);
                }
                else

                    if (buyres != string.Empty)
                    {
                        var AfterBuy = JsonConvert.DeserializeObject<WalletInfo>(buyres);

                        if (AfterBuy.WalletRes.Success == 1)
                        {
                            string balance = AfterBuy.WalletRes.Balance;
                            balance = balance.Insert(balance.Length - 2, ".");
                            return new BuyResponse(true, balance);
                        }
                        else return new BuyResponse(false, "UnknownErr");
                    }
                    else return new BuyResponse(false, "UnknownErr");
            }
            catch (Exception)
            {
                return new BuyResponse(false, "UnknownErr");
            }
        }       

        private void StartResellThread(string lotPrice, string resellPrice, AppType appType, string markName, int resellType)
        {
            ThreadStart threadStart = delegate()
            {
                try
                {
                    if (resellDelay > 0)
                        Thread.Sleep(resellDelay);

                    int sellPrice = Convert.ToInt32(lotPrice);
                    int resell = Convert.ToInt32(GetSweetPrice(resellPrice));

                    switch (resellType)
                    {
                        case 1: sellPrice += resell;
                            break;
                        case 2: sellPrice = resell;
                            break;
                    }

                    //You get the point!
                    ParseInventory(SendGet(string.Format(_jsonInv, myUserId, appType.App + "/" + appType.Context), cookieCont, false, true));

                    var req = string.Format(sellReq, GetSessId(cookieCont), appType.App, appType.Context, inventList.Find(p => p.Name == markName).AssetId, sellPrice.ToString());

                    SendPost(req, _sellitem, _market, false);
                    doMessage(flag.Resold, 0, markName, true);
                }
                catch (Exception exc)
                {
                    Main.AddtoLog("Resell error: " + exc.Message);
                    //To Error
                    doMessage(flag.ResellErr, 0, markName, true);
                }
            };
            Thread pTh = new Thread(threadStart);
            pTh.IsBackground = true;
            pTh.Start();
        }
    }
}*/


