using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;

namespace SteamMarketBot
{

    public delegate void eventDelegate(object sender, object data, int searchId, flag myflag, bool isMain);

    [Flags]
    public enum flag : byte
    {
        Already_logged = 0,
        Login_success = 1,
        Login_cancel = 2,
        Login_error = 3,
        Logout_ = 4,
        Price_text = 5,
        Price_htext = 6,
        Rep_progress = 7,
        Success_buy = 8,
        Scan_cancel = 9,
        Scan_progress = 10,
        Search_success = 11,
        Price_btext = 12,
        Inventory_Loaded = 13,
        Items_Sold = 14,
        Sell_progress = 15,
        Error_buy = 16,
        StripImg = 17,
        Send_cancel = 18,
        Error_scan = 19,
        Lang_Changed = 20,
        InvPrice = 21,
        Resold = 22,
        SetHeadName = 23,
        ReLogin = 24,
        ResellErr = 25,
        ActPrice = 26,
        Save_progress = 27,
        Track_progress = 28,
        Load_progress = 29,
        Add_track = 30,
        Advance_search_success = 31
    }

    [Flags]
    public enum status : byte
    {
        Ready = 0,
        Warning = 1,
        InProcess = 2,
    }

    class SteamLibrary
    {
        /// <summary>
        /// Provide database constants and methods for Steam Wep API.
        /// </summary>

        #region Url constant

        public const string _host = "steamcommunity.com";
        public const string _mainsite = "http://" + _host + "/";
        public const string _mainsiteS = "https://" + _host + "/";

        public const string _comlog = "https://" + _host + "/login/";
        public const string _ref = _comlog + "home/?goto=market%2F";
        public const string _getrsa = _comlog + "getrsakey/";
        public const string _dologin = _comlog + "dologin/";
        public const string _logout = _comlog + "logout/";

        public const string _market = _mainsite + "market/";

        public const string _blist = _mainsiteS + "market/buylisting/";
        public const string _lists = _market + "listings/";

        //Todo: JSON
        public const string _search = _market + "search/render/?query={0}&start={1}&count={2}";
        public const string _adsearch = _market + "search/render/?appid={0}&query={1}&start={2}&count={3}";

        public const string _capcha = "https://" + _host + "/public/captcha.php?gid=";
        public const string _refrcap = "https://" + _host + "/actions/RefreshCaptcha/?count=1";

        public const string loginReq = "password={0}&username={1}&twofactorcode=&emailauth={2}&loginfriendlyname={3}&captchagid={4}&captcha_text={5}&emailsteamid={6}&rsatimestamp={7}&remember_login=true";
        public const string loginStr = "steamid={0}&token={1}&remember_login=false&webcookie={2}";

        public const string buyReq = "sessionid={0}&currency={4}&subtotal={1}&fee={2}&total={3}&quantity={5}";

        public const string _jsonInv = _mainsite + "profiles/{0}/inventory/json/{1}";

        public const string imgUri = "http://steamcommunity-a.akamaihd.net/economy/image/";

        public const string fndImgUrl = imgUri + "{0}/62fx62f";
        public const string _sellitem = _mainsiteS + "market/sellitem/";
        public const string sellReq = "sessionid={0}&appid={1}&contextid={2}&assetid={3}&amount=1&price={4}";
        public const string removeSell = _market + "removelisting/";

        public const string searchPageReq = "{0}&start={1}0";

        public const string recentMarket = _market + "recent/";

        public const string priceOverview = _market + "priceoverview/?currency={0}&appid={1}&market_hash_name={2}";
        public const string jsonAddonUrl = "?country={0}&language={1}&currency={2}";

        //For html parsing, bulding own json!
        public const string buildJson = "\"success\":true,\"results_html\":\"\",\"listinginfo\":{0},\"assets\":{1}";

        #endregion

        #region Local contanst

        public const string logPath = "logfile.txt";
        public const string hostsPath = "hosts.txt";

        public const string cockPath = "coockies.dat";
        public const string steamUA = "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.1916.153 Safari/537.36 OPR/22.0.1471.70";

        //Just put here your random values
        public const string initVector = "tu89geji340t89u2";
        public const string passPhrase = "o6806642kbM7c5";
        public const int keysize = 256;

        #endregion

        #region Item classes

        public class SearchItem
        {
            /// <summary>
            /// Constuctor for items from search list.
            /// </summary>

            public string Name { set; get; }
            public string Game { set; get; }
            public string Link { set; get; }
            public string ImgLink { set; get; }
            public string Quantity { set; get; }
            public string StartPrice { set; get; }
            public string MedianPrice { set; get; }

            public string Hashname;

            public SearchItem(string name, string game, string link, string quantity, string startPrice,
                string imgLink, string medianPrice)
            {
                this.Name = name;
                this.Game = game;
                this.Link = link;
                this.Quantity = quantity;
                this.StartPrice = startPrice;
                this.ImgLink = imgLink;
                this.MedianPrice = medianPrice;
            }
        }

        public class InventItem
        {
            /// <summary>
            /// Constuctor for items from inventory list.
            /// </summary>

            public string Name { set; get; }
            public string ImgLink { set; get; }
            public string BuyPrice { set; get; }
            public string MedianPrice { get; set; }
            public string Type { set; get; }
            public string AssetId { set; get; }
            public string MarketName { set; get; }
            public bool OnSale { set; get; }
            public string PageLink { set; get; }
            public bool Marketable { set; get; }

            public InventItem(string assetId, string name, string type, 
                string buyPrice, string medianPrice, string imgLink, string marketName, bool onSale, bool marketable, string pageLink)
            {
                this.Name = name;
                this.AssetId = assetId;
                this.Type = type;
                this.BuyPrice = buyPrice;
                this.MedianPrice = medianPrice;
                this.ImgLink = imgLink;
                this.OnSale = onSale;
                this.MarketName = marketName;
                this.Marketable = marketable;
                this.PageLink = pageLink;
            }           
        }

        public class ItemToSell
        {
            public string AssetId { set; get; }
            public string Price { set; get; }

            public SteamLibrary.AppType App;

            public ItemToSell(string assetid, string price, SteamLibrary.AppType app)
            {
                this.AssetId = assetid;
                this.Price = price;
                this.App = app;
            }
        }

        public class TrackingItem
        {
            /// <summary>
            /// Constuctor for items from tracking list.
            /// </summary>

            public string Name { set; get; }
            public string Game { set; get; }
            public double StartPrice { set; get; }
            public double MedianPrice { set; get; }
            public double PreMedianPrice { get; set; }
            public int BuyPercent { set; get; }
            public int SellPercent { set; get; }
            public bool Buy { set; get; }
            public bool Sell { set; get; }
            public bool Purchased { set; get; }
            public string Link { get; set; }
            public double PurchasePrice { get; set; }

            public int index = 0;
            public string hashname = "";
            public string purchasedate = "";
            public System.Timers.Timer Refresh;
            public bool Buying = false;

            public TrackingItem(string name, string game, double startPrice, double medianPrice, int buyPercent,
                int sellPercent, bool buy, bool sell, bool purchased, string link, double purchasePrice)
            {
                this.Name = name;
                this.Game = game;
                this.StartPrice = startPrice;
                this.MedianPrice = medianPrice;
                this.BuyPercent = buyPercent;
                this.SellPercent = sellPercent;
                this.Buy = buy;
                this.Sell = sell;
                this.Purchased = purchased;
                this.Link = link;
                this.PurchasePrice = purchasePrice;
            }
        }

        public class BuyItem
        {
            /// <summary>
            /// Constuctor for items from buying list.
            /// </summary>

            public string ListingId { set; get; }
            public int Price { set; get; }
            public int Fee { set; get; }
            public AppType Type { set; get; }
            public string ItemName { set; get; }

            public BuyItem(string listingId, int price, int fee, AppType appType, string itemName)
            {
                this.ListingId = listingId;
                this.Price = price;
                this.Fee = fee;
                this.Type = appType;
                this.ItemName = itemName;
            }            
        }

        public class CurrencyInfo
        {
            /// <summary>
            /// Constructor for currency info.
            /// </summary>

            public string AsciiName { set; get; }
            public string TrueName { set; get; }
            public string Index { set; get; }

            public CurrencyInfo(string asciiName, string trueName, string index)
            {
                this.AsciiName = asciiName;
                this.TrueName = trueName;
                this.Index = index;
            }
        }

        public class CurrInfoLst : List<CurrencyInfo>
        {
            /// <summary>
            /// Provide info for list of currency.
            /// </summary>

            public void Load(string path)
            {
                if (File.Exists(path))
                {
                    var list = File.ReadAllLines(path);

                    for (int i = 0; i < list.Length; i++)
                    {
                        try
                        {
                            if (list[i] == string.Empty)
                            {
                                continue;
                            }                           
                            else if (list[i][0] == '/')
                            {
                                continue;
                            }
                            else
                            {
                                var currStr = list[i].Split(',');
                                if (currStr.Length == 3)
                                {
                                    this.Add(new CurrencyInfo(currStr[2], currStr[1], currStr[0]));
                                }
                                else if (currStr.Length == 2)
                                {
                                    this.Add(new CurrencyInfo(currStr[1], currStr[1], currStr[0]));
                                }
                                else continue;    
                            }    
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }

                    this.NotSet = true;
                    this.Current = 0;

                    if (this.Count == 0)
                    {
                        MessageBox.Show("Currency List is empty! Program will not work correctly.", 
                            "Attention", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Currency List is not exists! Program will not work correctly.", 
                        "Attention", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }                   
            }

            public int Current { set; get; }
            public bool NotSet { set; get; }

            public string GetName()
            {
                return this[Current].TrueName;
            }

            public string GetCode()
            {
                return this[Current].Index;
            }

            public string GetAscii()
            {
                return this[Current].AsciiName;
            }

            public void GetType(string input)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (input.Contains(this[i].AsciiName))
                    {
                        Current = i;
                        NotSet = false;
                        break;
                    }
                }
            }

            public string ReplaceAscii(string parseAmount)
            {
                return parseAmount.Replace(GetAscii(), GetName());
            }
        }

        #endregion 

        #region JSON parsing

        public class RespRSA
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("publickey_mod")]
            public string Module { get; set; }

            [JsonProperty("publickey_exp")]
            public string Exponent { get; set; }

            [JsonProperty("timestamp")]
            public string TimeStamp { get; set; }
        }

        public class RespProcess
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("emailauth_needed")]
            public bool isEmail { get; set; }

            [JsonProperty("captcha_needed")]
            public bool isCaptcha { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }

            [JsonProperty("captcha_gid")]
            public string Captcha_Id { get; set; }

            [JsonProperty("emailsteamid")]
            public string Email_Id { get; set; }

            [JsonProperty("bad_captcha")]
            public bool isBadCap { get; set; }
        }

        public class RespFinal
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("login_complete")]
            public bool isComplete { get; set; }
        }

        public class InventoryData
        {
            [JsonProperty("rgInventory")]
            public IDictionary<string, InvItem> myInvent { get; set; }

            [JsonProperty("rgDescriptions")]
            public IDictionary<string, ItemDescr> invDescr { get; set; }
        }

        public class InvItem
        {
            [JsonProperty("id")]
            public string assetid { get; set; }

            [JsonProperty("classid")]
            public string classid { get; set; }

            //Fixed
            [JsonProperty("instanceid")]
            public string instanceid { get; set; }
        }

        public class ItemDescr
        {
            //Fix for Resell feature
            [JsonProperty("market_name")]
            public string Name { get; set; }

            [JsonProperty("name")]
            public string SimpleName { get; set; }

            [JsonProperty("icon_url")]
            public string IconUrl { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("market_hash_name")]
            public string MarketName { get; set; }

            [JsonProperty("marketable")]
            public bool Marketable { get; set; }

            //New
            [JsonProperty("appid")]
            public string AppId { get; set; }
        }

        public class InfoMessage
        {
            [JsonProperty("message")]
            public string Message { get; set; }
        }

        public class WalletInfo
        {
            [JsonProperty("wallet_info")]
            public Wallet WalletRes { get; set; }
        }

        public class Wallet
        {
            [JsonProperty("wallet_balance")]
            public string Balance { get; set; }
            [JsonProperty("success")]
            public int Success { get; set; }
        }

        public class PageBody
        {
            [JsonProperty("success")]
            public bool Success { get; set; }
            [JsonProperty("results_html")]
            public string HtmlRes { get; set; }

            [JsonProperty(PropertyName = "listinginfo", Required = Required.Default)]
            public IDictionary<string, ListingInfo> Listing { get; set; }

            [JsonProperty("assets")]
            public IDictionary<string, IDictionary<string, IDictionary<string, ItemInfo>>> Assets { get; set; }
        }

        public class ListingInfo
        {
            [JsonProperty("listingid")]
            public string listingid { get; set; }

            [JsonProperty("converted_price")]
            public int price { get; set; }

            [JsonProperty("converted_fee")]
            public int fee { get; set; }

            [JsonProperty("asset")]
            public ItemAsset asset { get; set; }

            [JsonProperty("steamid_lister")]
            public string userId { get; set; }
        }

        public class ItemAsset
        {
            [JsonProperty("appid")]
            public string appid { get; set; }

            [JsonProperty("contextid")]
            public string contextid { get; set; }

            [JsonProperty("id")]
            public string id { get; set; }
        }

        public class Assets
        {
            [JsonProperty(PropertyName = "listinginfo", Required = Required.Default)]
            public IDictionary<string, ListingInfo> Listing { get; set; }

            [JsonProperty("converted_fee")]
            public string fee { get; set; }

            [JsonProperty("asset")]
            public ItemAsset asset { get; set; }
        }

        public class ItemInfo
        {
            [JsonProperty("market_name")]
            public string name { get; set; }

            [JsonProperty(PropertyName = "fraudwarnings", Required = Required.Default)]
            public object warnings { get; set; }

            //Not Useful Yet...
            [JsonProperty("type")]
            public string type { get; set; }

            [JsonProperty("tradable")]
            public bool tradable { get; set; }

            [JsonProperty("icon_url")]
            public string icon_url { get; set; }
        }

        public class SearchBody
        {
            [JsonProperty("success")]
            public bool Success { get; set; }
            [JsonProperty("results_html")]
            public string HtmlRes { get; set; }
            [JsonProperty("total_count")]
            public string TotalCount { get; set; }
        }

        public class PriceOverview
        {
            [JsonProperty("success")]
            public bool Success { get; set; }
            [JsonProperty("lowest_price")]
            public string Lowest { get; set; }
            [JsonProperty("volume")]
            public string Volume { get; set; }
            [JsonProperty("median_price")]
            public string Median { get; set; }
        }

        #endregion

        #region DataCheck
        
        public class StrParam
        {
            public StrParam(string p1, string p2)
            {
                this.P1 = p1;
                this.P2 = p2;
            }

            public StrParam(string p1, string p2, string p3, string p4, string p5)
            {
                this.P1 = p1;
                this.P2 = p2;
                this.P3 = p3;
                this.P4 = p4;
                this.P5 = p5;
            }

            public string P1 { set; get; }
            public string P2 { set; get; }
            public string P3 { set; get; }
            public string P4 { set; get; }
            public string P5 { set; get; }
        }

        public class BuyResponse
        {
            public bool Succsess { set; get; }
            public string Mess { set; get; }

            public BuyResponse(bool succsess, string mess)
            {
                this.Succsess = succsess;
                this.Mess = mess;
            }            
        }

        public class AppType
        {
            public string AppID { set; get; }
            public string Context { set; get; }

            public AppType(string app, string context)
            {
                this.AppID = app;
                this.Context = context;
            }
        }

        public class SearchPagePos
        {
            public int PageCount { set; get; }
            public int CurrentPos { set; get; }

            public SearchPagePos(int pageCount, int currentPos)
            {
                this.PageCount = pageCount;
                this.CurrentPos = currentPos;
            }            
        }

        public static AppType GetApp(string appname)
        {
            string apptype = "753";
            string cont = "6";

            switch (appname)
            {
                case "Dota 2":
                    apptype = "570";
                    cont = "2";
                    break;
                case "Counter-Strike: Global Offensive":
                    apptype = "730";
                    cont = "2";
                    break;
                case "BattleBlock Theater":
                    apptype = "238460";
                    break;
                case "Team Fortress 2":
                    apptype = "440";
                    cont = "2";
                    break;
                case "Warframe":
                    apptype = "230410";
                    cont = "2";
                    break;
                case "The Mighty Quest For Epic Loot":
                    apptype = "239220";
                    break;
                case "Sins of a Dark Age":
                    apptype = "251970";
                    cont = "1";
                    break;
                case "Primal Carnage: Extinction":
                    apptype = "321360";
                    break;
                case "Path of Exile":
                    apptype = "238960";
                    cont = "1";
                    break;
                case "Minimum":
                    apptype = "214190";
                    break;
                case "Altitude0: Lower & Faster":
                    apptype = "308080";
                    break;
            }

            return new AppType(apptype, cont);
        }

        public static AppType GetAppFromIndex(int index, bool isInv)
        {
            string app = "753";
            string cont = "6";

            switch (index)
            {
                case 0: //Trading Cards
                    app = "753";
                    cont = "6";
                    break;
                case 1:  //TF2
                    app = "440";
                    cont = "2";
                    break;
                case 2:  //DOTA2
                    app = "570";
                    cont = "2";
                    break;
                case 3: //CS:GO
                    app = "730";
                    cont = "2";
                    break;
                case 4: //BattleBlock Theater
                    app = "238460";
                    cont = "2";
                    break;
                case 5: //Warframe
                    app = "230410";
                    cont = "2";
                    break;
                case 6: //Sins of a Dark Age
                    app = "251970";
                    cont = "1";
                    break;
                case 7: //Path of Exile
                    app = "238960";
                    cont = "1";
                    break;
            }
            if (!isInv)
            {
                return new AppType(app, cont);
            }
            else
            {
                return new AppType(string.Format("{0}/{1}", app, cont), string.Empty);
            }
        }

        public static string GetAppNameFromIndex(int index)
        {
            string app = "";

            switch (index)
            {
                case 0: //Trading Cards
                    app = "Trading Cards";
                    break;
                case 1:  //TF2
                    app = "Team Fortress 2";
                    break;
                case 2:  //DOTA2
                    app = "Dota 2";
                    break;
                case 3: //CS:GO
                    app = "Counter-Strike: Global Offensive";
                    break;
                case 4: //BattleBlock Theater
                    app = "BattleBlock Theater";
                    break;
                case 5: //Warframe
                    app = "Warframe";
                    break;
                case 6: //Sins of a Dark Age
                    app = "Sins of a Dark Age";
                    break;
                case 7: //Path of Exile
                    app = "Path of Exile";
                    break;
            }

            return app;
        }

        public static string GetSweetPrice(string input)
        {
            string res = string.Empty;

            var match = input.IndexOfAny(".,".ToCharArray());

            if ((match == -1) | (match == input.Length - 1))
            {
                res = input + "00";
            }
            else
            {
                if (input.Length > match + 3)
                {
                    res = input.Substring(0, match + 3);
                }
                else if (input.Length == match)
                {
                    res = input + "00";
                }
                else if (input.Length == match + 2)
                {
                    res = input + "0";
                }
                else res = input;
            }

            return Regex.Replace(res, @"[d\.\,]+", string.Empty);
        }

        //Acync file access
        public static void AddtoLog(string logstr)
        {
            try
            {
                using (FileStream fs = new FileStream(logPath, FileMode.OpenOrCreate, FileSystemRights.AppendData,
                FileShare.Write, 4096, FileOptions.None))
                {
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        writer.AutoFlush = true;
                        writer.WriteLine(DateTime.Now);
                        writer.WriteLine(logstr);
                        writer.WriteLine();
                        writer.Close();
                    }
                    fs.Close();
                }
            }
            catch (Exception)
            {
                //IO exception?
            }
        }

        public static void SaveBinary(string p, object o)
        {
            try
            {
                if (o != null)
                {
                    using (Stream stream = File.Create(p))
                    {
                        BinaryFormatter bin = new BinaryFormatter();
                        bin.Serialize(stream, o);
                    }
                }
            }
            catch (Exception e)
            {
                AddtoLog("Saving Binary Exception: " + e.Message);
            }
        }

        public static object LoadBinary(string p)
        {
            try
            {
                using (Stream stream = File.Open(p, FileMode.Open))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    var res = bin.Deserialize(stream);
                    return res;
                }
            }
            catch (Exception e)
            {
                AddtoLog("Error Opening " + p + ": " + e.Message);
                return null;
            }
        }

        public static string Encrypt(string plainText)
        {
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
            byte[] keyBytes = password.GetBytes(keysize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] cipherTextBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(cipherTextBytes);
        }

        public static string Decrypt(string cipherText)
        {
            try
            {
                byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
                byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
                PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
                byte[] keyBytes = password.GetBytes(keysize / 8);
                RijndaelManaged symmetricKey = new RijndaelManaged();
                symmetricKey.Mode = CipherMode.CBC;
                ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
                MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
                CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                memoryStream.Close();
                cryptoStream.Close();
                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
            }
            catch (Exception)
            {
                return "Password";
            }
        }

        public static string EncryptPassword(string password, string modval, string expval)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            RSAParameters rsaParams = new RSAParameters();
            rsaParams.Modulus = HexToByte(modval);
            rsaParams.Exponent = HexToByte(expval);
            rsa.ImportParameters(rsaParams);

            byte[] bytePassword = Encoding.ASCII.GetBytes(password);
            byte[] encodedPassword = rsa.Encrypt(bytePassword, false);
            string encryptedPass = Convert.ToBase64String(encodedPassword);

            return Uri.EscapeDataString(encryptedPass);
        }

        static byte[] HexToByte(string hex)
        {
            if (hex.Length % 2 == 1)
            {
                AddtoLog("HexToByte: The binary key cannot have an odd number of digits");
                return null;
            }

            byte[] arr = new byte[hex.Length >> 1];
            int l = hex.Length;

            for (int i = 0; i < (l >> 1); ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        static int GetHexVal(char hex)
        {
            int val = (int)hex;
            return val - (val < 58 ? 48 : 55);
        }

        public static string CalcWithFee(string strInput)
        {
            int intres = 0;
            int input = Convert.ToInt32(strInput);

            double temp = input / 1.15;

            if (input > 10)
            {
                intres = Convert.ToInt32(Math.Ceiling(temp));
            }
            else if (input < 4)
            {
                if (input == 3)
                {
                    intres = 1;
                }
                else
                {
                    intres = 0;
                }
            }
            else
            {
                intres = Convert.ToInt32(temp) - 1;
            }    

            return intres.ToString();
        }

        public static string AddFee(string strInput)
        {
            int intres = 0;
            int input = Convert.ToInt32(strInput);
            double temp = 0;

            if (input < 20)
            {
                temp = 2;
            }
            else
            {
                temp = input * 0.15;
            }

            intres = input + Convert.ToInt32(temp);

            return intres.ToString();
        }

        public static string DoFracture(string input)
        {
            string prtoTxt = "0.";

            switch (input.Length)
            {
                case 0:
                    prtoTxt = "0";
                    break;
                case 1:
                    prtoTxt += "0" + input;
                    break;
                case 2:
                    prtoTxt += input;
                    break;
                default:
                    prtoTxt = input.Insert(input.Length - 2, ".");
                    break;
            }
            return prtoTxt;
        }

        #endregion

    }
}
