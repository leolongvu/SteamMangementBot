using System;
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
    public class Config
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

        public int searchRes { get; set; }

        public static string myUserId;

        public CookieContainer cookieCont { set; get; }       
    }
}
