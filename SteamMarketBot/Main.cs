using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.ComponentModel;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;
using System.Timers;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace SteamMarketBot
{
    public partial class Main : Form
    {
                  
        static bool isLog = true;

        static int lastSelec = -1;

        private static SteamAutoFunction SteamAuto = new SteamAutoFunction();

        public static SettingFrm settingfrm = new SettingFrm();

        public static int ReqDelay = 100;

        public static string JsonAddon;

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            AddListView();
            
            settingfrm.Loadacclist();

            //Loading currency list
            SteamParsing.Currencies.Load("currency.txt");

            LoadingCookie();            

            LoadConfigs();

            button2.Enabled = false;
            button3.Enabled = false;
            button7.Enabled = false;
            button15.Enabled = false;
            nextpageBT.Visible = false;
            prepageBT.Visible = false;
            button16.Visible = false;
            button17.Visible = false;

            comboBox1.SelectedIndex = 0;

            SteamAuto.delegMessage += new eventDelegate(Event_Message);
        }

        public void LoadingCookie()
        {           
            if (settingfrm.Visible == false)
            {
                settingfrm.Show();

                SteamAuto.UserName = settingfrm.comboBox1.SelectedItem.ToString();

                settingfrm.Hide();
            }
            else
            {
                SteamAuto.UserName = settingfrm.comboBox1.SelectedItem.ToString();
            }

            var cook = new CookieContainer();

            if (!SteamAuto.UserName.Equals("New Account"))
            {
                cook = (CookieContainer)SteamLibrary.LoadBinary(SteamAuto.UserName + ".dat");
                if (cook == null)
                {
                    cook = new CookieContainer();
                }                  
            }

            SteamAuto.cookieCont = cook;

        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!SteamAuto.UserName.Equals("New Account"))
            {
                SteamLibrary.SaveBinary(SteamAuto.UserName + ".dat", SteamAuto.cookieCont);                
            }

            StatusPB.Image = Properties.Resources.icon1;
            StatusLabel1.Text = "Saving";
            SaveTrackList();
            StatusPB.Image = null;
        }

        private void AddListView()
        {
            ScanLV.View = View.Details;
            ScanLV.Columns.Add("Game", 46, HorizontalAlignment.Left);
            ScanLV.Columns.Add("Name", 190, HorizontalAlignment.Left);
            ScanLV.Columns.Add("Lowest price", 60, HorizontalAlignment.Left);
            ScanLV.Columns.Add("Quantity", 60, HorizontalAlignment.Left);
            ScanLV.Columns.Add("Median price", 60, HorizontalAlignment.Left);
            ScanLV.Columns.Add("Hash name", 0, HorizontalAlignment.Left);

            TraItemLV.View = View.Details;
            TraItemLV.Columns.Add("Game", 43, HorizontalAlignment.Left);
            TraItemLV.Columns.Add("Name", 135, HorizontalAlignment.Left);          
            TraItemLV.Columns.Add("Lowest price", 53, HorizontalAlignment.Left);
            TraItemLV.Columns.Add("Median Price", 53, HorizontalAlignment.Left);
            TraItemLV.Columns.Add("Buy", 20, HorizontalAlignment.Left);
            TraItemLV.Columns.Add("Sell", 20, HorizontalAlignment.Left);
            TraItemLV.Columns.Add("% B", 37, HorizontalAlignment.Left);
            TraItemLV.Columns.Add("% S", 37, HorizontalAlignment.Left);
            TraItemLV.Columns.Add("Purchased", 20, HorizontalAlignment.Left);
            TraItemLV.Columns.Add("Link", 0, HorizontalAlignment.Left);
            TraItemLV.Columns.Add("Pur. Price", 53, HorizontalAlignment.Left);
            TraItemLV.Columns.Add("Trad. Date", 70, HorizontalAlignment.Left);
            TraItemLV.Columns.Add("Hash Name", 0, HorizontalAlignment.Left);
            TraItemLV.Columns.Add("Ping", 36, HorizontalAlignment.Left);

            InvenLV.View = View.Details;
            InvenLV.Columns.Add("Type", 68, HorizontalAlignment.Left);
            InvenLV.Columns.Add("Name", 180, HorizontalAlignment.Left);
            InvenLV.Columns.Add("Buy Price", 78, HorizontalAlignment.Left);
            InvenLV.Columns.Add("Median Price", 78, HorizontalAlignment.Left);
        }

        public void Event_Message(object sender, object data, int searchId, flag myflag, bool isMain)
        {
            if (data == null)
                return;

            string message = data.ToString();

            switch (myflag)
            {
                case flag.Already_logged:

                    StatusLabel1.Text = "Ready";
                    StatusPB.Image = null;
                    SteamLibrary.AddtoLog("Already logged");
                    GetAccInfo((SteamLibrary.StrParam)data);
                    break;

                case flag.Login_success:

                    StatusLabel1.Text = "Ready";
                    StatusPB.Image = null;
                    SteamLibrary.AddtoLog("Login succesful");
                    GetAccInfo((SteamLibrary.StrParam)data);
                    break;

                case flag.Login_cancel:

                    MessageBox.Show("Error logging in! " + message + "!", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    StatusPB.Image = null;
                    StatusLabel1.Text = message;                    
                    break;

                case flag.Logout_:

                    StatusLabel1.Text = "Ready";
                    StatusPB.Image = null;
                    LoginB.Text = "Login";
                    break;

                case flag.Search_success:

                    ScanLV.Enabled = true;
                    button15.Enabled = true;

                    if (SteamAutoFunction.sppos.CurrentPos == 1)
                    {
                        int found = Convert.ToInt32(message);
                        SteamAutoFunction.sppos.PageCount = found / SteamAutoFunction.SearchResCount;
                        if (found % SteamAutoFunction.SearchResCount != 0)
                            SteamAutoFunction.sppos.PageCount++;
                    }

                    label5.Text = string.Format("{0}/{1}", SteamAutoFunction.sppos.CurrentPos.ToString(), 
                        SteamAutoFunction.sppos.PageCount.ToString());

                    if (SteamAutoFunction.sppos.PageCount > 1)
                    {
                        prepageBT.Visible = true;
                        nextpageBT.Visible = true;
                        button16.Visible = false;
                        button17.Visible = false;
                    }
                    else
                    {
                        prepageBT.Visible = false;
                        nextpageBT.Visible = false;
                        button16.Visible = false;
                        button17.Visible = false;
                    }

                    ScanLV.Items.Clear();

                    for (int i = 0; i < SteamAutoFunction.SearchList.Count; i++)
                    {
                        var ourItem = SteamAutoFunction.SearchList[i];

                        string[] row = { ourItem.Game, ourItem.Name, SteamParsing.Currencies.GetName() + " " + ourItem.StartPrice, 
                                           ourItem.Quantity, SteamParsing.Currencies.GetName() + " " + ourItem.MedianPrice, ourItem.Hashname };
                        var lstItem = new ListViewItem(row);
                        ScanLV.Items.Add(lstItem);
                    }

                    button2.Enabled = true;
                    StatusPB.Image = null;
                    StatusLabel1.Text = "Ready";

                    break;

                case flag.Advance_search_success:

                    ScanLV.Enabled = true;
                    button15.Enabled = true;
                    SteamAutoFunction.searchDone = true;

                    if (SteamAutoFunction.sppos.CurrentPos == 1)
                    {
                        int found = Convert.ToInt32(message);
                        SteamAutoFunction.sppos.PageCount = found / SteamAutoFunction.SearchResCount;
                        if (found % SteamAutoFunction.SearchResCount != 0)
                            SteamAutoFunction.sppos.PageCount++;
                    }

                    label5.Text = string.Format("{0}/{1}", SteamAutoFunction.sppos.CurrentPos.ToString(),
                        SteamAutoFunction.sppos.PageCount.ToString());

                    if (SteamAutoFunction.sppos.PageCount > 1)
                    {
                        prepageBT.Visible = false;
                        nextpageBT.Visible = false;
                        button16.Visible = true;
                        button17.Visible = true;
                    }
                    else
                    {
                        prepageBT.Visible = false;
                        nextpageBT.Visible = false;
                        button16.Visible = false;
                        button17.Visible = false;
                    }

                    ScanLV.Items.Clear();

                    for (int i = 0; i < SteamAutoFunction.SearchList.Count; i++)
                    {
                        var ourItem = SteamAutoFunction.SearchList[i];

                        string[] row = { ourItem.Game, ourItem.Name, SteamParsing.Currencies.GetName() + " " + ourItem.StartPrice, 
                                           ourItem.Quantity, SteamParsing.Currencies.GetName() + " " + ourItem.MedianPrice, ourItem.Hashname };
                        var lstItem = new ListViewItem(row);
                        ScanLV.Items.Add(lstItem);
                    }

                    button2.Enabled = true;
                    StatusPB.Image = null;
                    StatusLabel1.Text = "Ready";           

                    break;
                   
                case flag.Add_track:

                    SteamLibrary.TrackingItem item = (SteamLibrary.TrackingItem)data;

                    string lp = SteamParsing.Currencies.GetName() + " " + string.Format("{0:N2}", item.StartPrice);
                    string mp = SteamParsing.Currencies.GetName() + " " + string.Format("{0:N2}", item.MedianPrice);

                    string[] row1 = { item.Game, item.Name, lp, mp, "", "",  item.BuyPercent.ToString(), 
                                    item.SellPercent.ToString(), "", item.Link, "", "", item.hashname, "" };
                    var lst1 = new ListViewItem(row1);
                    TraItemLV.Items.Add(lst1);

                    break;

                case flag.Track_progress:

                    SteamLibrary.TrackingItem item1 = (SteamLibrary.TrackingItem)data;

                    string lp1 = SteamParsing.Currencies.GetName() + " " + string.Format("{0:N2}", item1.StartPrice);
                    string mp1 = SteamParsing.Currencies.GetName() + " " + string.Format("{0:N2}", item1.MedianPrice);                    

                    TraItemLV.Items[item1.index].SubItems[2].Text = lp1;
                    TraItemLV.Items[item1.index].SubItems[3].Text = mp1;
                    TraItemLV.Items[item1.index].SubItems[13].Text = searchId.ToString();

                    break;

                /*case flag.Error_scan:

                    string mess = GetScanErrMess(message);
                    button12.Enabled = true;
                    button3.Enabled = true;
                    break;*/

                case flag.Success_buy:

                    string[] price = message.Split(';');

                    double itemcost = Math.Round(Convert.ToSingle(WalletL.Text.Remove(0, 2)) - Convert.ToSingle(price[0]), 2);

                    FlashWindow.Flash(this);                                       

                    WalletL.Text = SteamParsing.Currencies.GetName() + " " + price[0];
                    button3.Enabled = true;
                    button12.Enabled = true;

                    for (int i = 0; i < SteamAutoFunction.TrackingList.Count; i++)
                    {
                        if (SteamAutoFunction.TrackingList[i].Name.Equals(price[1]))
                        {
                            SteamAutoFunction.TrackingList[i].Purchased = true;
                            TraItemLV.Items[i].SubItems[8].Text = "x";
                            TraItemLV.Items[i].SubItems[10].Text = SteamParsing.Currencies.GetName() + " " + itemcost;
                            TraItemLV.Items[i].SubItems[11].Text = DateTime.Today.AddDays(7).ToString();
                            if (SteamAutoFunction.TrackingList[i].Buy == true)
                            {
                                SteamAutoFunction.TrackingList[i].Buy = false;
                                TraItemLV.Items[i].SubItems[4].Text = "";    
                            }
                            if (SteamAutoFunction.TrackingList[i].Buying == true)
                            {
                                SteamAutoFunction.TrackingList[i].Buying = false;
                            }
                            SteamAutoFunction.TrackingList[i].Refresh.Stop();
                            break;
                        }
                    }                  

                    break;
                    
                case flag.Error_buy:

                    button3.Enabled = true;
                    break;

 
                case flag.Load_progress:

                    for (int a = 0; a < SteamAutoFunction.TrackingList.Count; a++)
                    {
                        SteamAutoFunction.TrackingList[a].index = a;

                        string lp2 = SteamParsing.Currencies.GetName() + " " + string.Format("{0:N2}", SteamAutoFunction.TrackingList[a].StartPrice);
                        string mp2 = SteamParsing.Currencies.GetName() + " " + string.Format("{0:N2}", SteamAutoFunction.TrackingList[a].MedianPrice);

                        string buy = ""; string sell = ""; string pured = ""; string pp = "";

                        if (SteamAutoFunction.TrackingList[a].Buy)
                        {
                            buy = "x";
                        }
                        if (SteamAutoFunction.TrackingList[a].Sell)
                        {
                            sell = "x";
                        }
                        if (SteamAutoFunction.TrackingList[a].Purchased)
                        {
                            pured = "x";
                            pp = SteamParsing.Currencies.GetName() + " " + string.Format("{0:N2}", SteamAutoFunction.TrackingList[a].PurchasePrice);
                        }

                        string[] row2 = {   SteamAutoFunction.TrackingList[a].Game,
                                            SteamAutoFunction.TrackingList[a].Name, lp2, mp2, buy, sell,  
                                            SteamAutoFunction.TrackingList[a].BuyPercent.ToString(), 
                                            SteamAutoFunction.TrackingList[a].SellPercent.ToString(), pured, SteamAutoFunction.TrackingList[a].Link, 
                                            pp, SteamAutoFunction.TrackingList[a].purchasedate ,
                                            SteamAutoFunction.TrackingList[a].hashname, "" };
                        var lst2 = new ListViewItem(row2);
                        TraItemLV.Items.Add(lst2);
                    }

                    break;

                case flag.Inventory_Loaded:

                    button9.Enabled = true;
                    InvenLV.Enabled = true;
                    StatusPB.Image = null;
                    StatusLabel1.Text = "Ready";
                    //SetInvFilter();
                   
                    //label4.Text = filteredInvList.Count.ToString();
                    button1.Enabled = true;
                    if (searchId == 0)
                    {
                        FillInventoryList();

                        button7.Enabled = true;
                        if (comboBox1.SelectedIndex != 8)
                        {
                            button10.Enabled = true;
                        }
                        else
                        {
                            button10.Enabled = false;
                        }
                    }
                    else
                    {
                        InvenLV.Items.Clear();
                        button10.Enabled = false;
                        MessageBox.Show("Inventory section is empty!", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    break;

                case flag.InvPrice:

                    SteamLibrary.InventItem ourItem1 = (SteamLibrary.InventItem)data;

                    string priceRes = SteamParsing.Currencies.GetName() + " " + SteamLibrary.DoFracture(ourItem1.BuyPrice);
                    string priceMes = SteamParsing.Currencies.GetName() + " " + ourItem1.MedianPrice.ToString();

                    InvenLV.Items[searchId].SubItems[2].Text = priceRes;
                    InvenLV.Items[searchId].SubItems[3].Text = priceMes;

                    textBox2.Text = SteamLibrary.DoFracture(ourItem1.BuyPrice);
                    textBox2_KeyUp(this, null);

                    textBox2.ReadOnly = false;

                    button11.Enabled = true;
                    InvenLV.Enabled = true;

                    break;

                case flag.Items_Sold:
                    if (searchId != 1)
                    {
                        StatusLabel1.Text = "Ready";
                        StatusPB.Image = null;

                        button10.Text = "Set price";

                        int app = comboBox1.SelectedIndex;

                        SteamAuto.InventoryThreadExcute(app);
                    }
                    break;
            }
        }

        #region Load Settings

        public void LoadConfigs()
        {
            if (File.Exists(Application.StartupPath + @"\Configs\" + SteamAuto.UserName + ".ini"))
            {
                TextReader config_reader = new StreamReader(Application.StartupPath + @"\Configs\" + SteamAuto.UserName + ".ini");
                string input;
                while ((input = config_reader.ReadLine()) != "//")
                {
                    SteamAuto.UserName = input;
                    settingfrm.UserNamTB.Text = SteamAuto.UserName;
                    SteamAuto.Password = SteamLibrary.Decrypt(config_reader.ReadLine());
                    settingfrm.PassTB.Text = SteamAuto.Password;
                    settingfrm.searchResTB.Text = config_reader.ReadLine();
                }
                config_reader.Close();
                AccNameLB.Text = "(Acc: " + SteamAuto.UserName + ")";
            }
            else
            {
                MessageBox.Show("Error loading configs!", "Error");
                AccNameLB.Text = "(Acc: None)";
            }
        }      

        public void SaveConfigs()
        {
            if (!Directory.Exists(System.AppDomain.CurrentDomain.BaseDirectory + @"\Configs"))
            {
                Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + @"\Configs");
            }
            TextWriter config_writer = new StreamWriter(Application.StartupPath + @"\Configs\" + SteamAuto.UserName + ".ini");
            {
                config_writer.WriteLine(settingfrm.comboBox1.SelectedItem.ToString());
                config_writer.WriteLine(SteamLibrary.Encrypt(settingfrm.PassTB.Text));
                config_writer.WriteLine(settingfrm.searchResTB.Text);
                config_writer.WriteLine("//");
                config_writer.Close();
            }
        }

        public void SaveTrackList()
        {
            try
            {
                if (!Directory.Exists(System.AppDomain.CurrentDomain.BaseDirectory + @"\Tracks"))
                {
                    Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + @"\\Tracks");
                }
                if (SteamAutoFunction.Logged)
                {
                    TextWriter config_writer = new StreamWriter(Application.StartupPath + @"\Tracks\" + SteamAuto.UserName + ".ini");
                    {
                        for (int i = 0; i < TraItemLV.Items.Count; i++)
                        {
                            config_writer.WriteLine("[Item {0}]", i);
                            for (int a = 0; a < 13; a++)
                            {
                                config_writer.WriteLine(TraItemLV.Items[i].SubItems[a].Text);
                            }
                        }
                        config_writer.WriteLine("//");
                        config_writer.Close();
                    }
                }              
            }
            catch
            {
                MessageBox.Show("Error in saving data! Data will not be saved!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        private void LoginB_Click(object sender, EventArgs e)
        {
            if (!SteamAutoFunction.Logged)
            {
                if (SteamAutoFunction.LoginProcess)
                {
                    //TODO: adequate login cancellation!
                    SteamAuto.CancelLogin();
                }
                else
                {
                    SteamAuto.LoginThreadExcute();
                    StatusPB.Image = Properties.Resources.icon1;
                    StatusLabel1.Text = "Logging in";
                }
            }
            else
            {
                SteamAuto.Logout();

                StatusPB.Image = Properties.Resources.icon1;
                StatusLabel1.Text = "Logging out";
                button2.Enabled = false;
                button3.Enabled = false;
                LoginB.Text = "Login";
                AccNameL.Text = "None";
                WalletL.Text = "None";
                AvatarPB.Image = null;
            }
        }

        private void GetAccInfo(SteamLibrary.StrParam mess)
        {
            if (mess != null)
            {
                SteamParsing.SteamSite.StartLoadImgTread(mess.P4, AvatarPB);
                WalletL.Text = mess.P3.Insert(1, " ");
                JsonAddon = mess.P4;

                //walletVal = Convert.ToInt32(SteamSite.GetSweetPrice(Regex.Replace(mess.P2, @"[^\d.,]+", string.Empty)));

                AccNameL.Text = mess.P1;
                LoginB.Text = "Logout";
                button2.Enabled = true;
                button3.Enabled = true;
                button15.Enabled = true;

                SteamAuto.LoadThreadExcute(SteamAuto.UserName, 3000);

                /*ProgressBar1.Visible = false;             
                addtoScan.Enabled = true;

                setNotifyText(Strings.NotLogged);*/
            }
            else
                MessageBox.Show("ErrAccInfo", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void SettingB_Click(object sender, EventArgs e)
        {
            settingfrm.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FlashWindow.Flash(this);

            SteamAuto.DoSearch(0, 20, SeaNameTB.Text);
            ScanLV.Enabled = false;
            button2.Enabled = false;
            button15.Enabled = false;
            StatusPB.Image = Properties.Resources.icon1;
            StatusLabel1.Text = "Searching";
        }

        private void nextpageBT_Click(object sender, EventArgs e)
        {
            ScanLV.Enabled = false;
            button2.Enabled = false;
            button15.Enabled = false;
            SteamAuto.DoSearch(1, 20, SeaNameTB.Text);
            StatusPB.Image = Properties.Resources.icon1;
            StatusLabel1.Text = "Searching";
            nextpageBT.Visible = false;
            prepageBT.Visible = false;
        }

        private void prepageBT_Click(object sender, EventArgs e)
        {
            ScanLV.Enabled = false;
            button2.Enabled = false;
            button15.Enabled = false;
            SteamAuto.DoSearch(2, 20, SeaNameTB.Text);
            StatusPB.Image = Properties.Resources.icon1;
            StatusLabel1.Text = "Searching";
            nextpageBT.Visible = false;
            prepageBT.Visible = false;
        }

        private void ScanLV_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ScanLV.SelectedItems.Count == 1)
            {
                var ourItem = SteamAutoFunction.SearchList[ScanLV.SelectedIndices[0]];
                SteamParsing.SteamSite.StartLoadImgTread(string.Format(SteamLibrary.fndImgUrl, ourItem.ImgLink), pictureBox1);
            }
            else
                pictureBox1.Image = null;
        }

        private void AddTracB_Click(object sender, EventArgs e)
        {
            try
            {
                if (ScanLV.SelectedIndices.Count > 0)
                {
                    string name = ScanLV.SelectedItems[0].SubItems[1].Text;
                    string game = ScanLV.SelectedItems[0].SubItems[0].Text;
                    string hashname = ScanLV.SelectedItems[0].SubItems[5].Text;
                    int count = 0;
                    for (int i = 0; i < TraItemLV.Items.Count; i++)
                    {
                        if (TraItemLV.Items[i].SubItems[0].Text.Equals(game) && TraItemLV.Items[i].SubItems[1].Text.Equals(name))
                        {
                            count++;
                        }
                    }
                    if (count == 0)
                    {
                        SteamAuto.DoTrack(name, game, hashname, Convert.ToInt32(BuyPricTB.Text), Convert.ToInt32(SelPricTB.Text), 3000);
                    }
                }
            }
            catch
            {
                MessageBox.Show("Settings for tracking is incorrect!", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetBuyB_Click(object sender, EventArgs e)
        {
            if (TraItemLV.SelectedIndices.Count > 0)
            {
                for (int i = 0; i < TraItemLV.SelectedItems.Count; i++)
                {
                    int a = TraItemLV.SelectedItems[i].Index;
                    TraItemLV.SelectedItems[i].SubItems[4].Text = "x";
                    SteamAutoFunction.TrackingList[a].Buy = true;
                    SteamAutoFunction.TrackingList[a].Refresh.Stop();
                    SteamAutoFunction.TrackingList[a].Refresh.Start();                    
                }
            }         
        }

        private void SetSellB_Click(object sender, EventArgs e)
        {
            if (TraItemLV.SelectedIndices.Count > 0)
            {
                for (int i = 0; i < TraItemLV.SelectedItems.Count; i++)
                {
                    int a = TraItemLV.SelectedItems[i].Index;
                    TraItemLV.SelectedItems[i].SubItems[5].Text = "x";
                    SteamAutoFunction.TrackingList[a].Sell = true;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (TraItemLV.SelectedIndices.Count > 0)
            {
                for (int i = 0; i < TraItemLV.SelectedItems.Count; i++)
                {
                    int a = TraItemLV.SelectedItems[i].Index;
                    TraItemLV.SelectedItems[i].SubItems[4].Text = "";
                    SteamAutoFunction.TrackingList[a].Refresh.Stop();
                    SteamAutoFunction.TrackingList[a].Buy = false;
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (TraItemLV.SelectedIndices.Count > 0)
            {
                for (int i = 0; i < TraItemLV.SelectedItems.Count; i++)
                {
                    int a = TraItemLV.SelectedItems[i].Index;
                    TraItemLV.SelectedItems[i].SubItems[5].Text = "";
                    SteamAutoFunction.TrackingList[a].Sell = false;
                }
            }         
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (SearchRight())
            {
                var ourItem = SteamAutoFunction.SearchList[ScanLV.SelectedItems[0].Index];
                DialogResult result = MessageBox.Show("Do you want to buy " + ourItem.Name + "?", "Attention", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    SteamAuto.BuyThreadExcute(ourItem.Link, true, true, false);
                    button3.Enabled = false;
                }
            }
            else
            {
                MessageBox.Show("No item selected or found! Please select one or search for new items!", "Attention",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }            
        }

        bool SearchRight()
        {
            return (ScanLV.Items.Count != 0 && SteamAutoFunction.SearchList.Count != 0 && ScanLV.SelectedItems.Count > -1);
        }

        private void button6_Click(object sender, EventArgs e)
        {
           if (TraItemLV.SelectedIndices.Count > 0)
            {                
                foreach(ListViewItem lvitem in TraItemLV.SelectedItems)
                {
                    try
                    {
                        SteamAutoFunction.TrackingList[lvitem.Index].Refresh.Stop();
                    }
                    catch { }
                    SteamAutoFunction.TrackingList.RemoveAt(lvitem.Index);
                    TraItemLV.Items.Remove(lvitem);
                }
                for (int j = 0; j < TraItemLV.Items.Count; j++)
                {
                    SteamAutoFunction.TrackingList[j].index = j;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (TraItemLV.SelectedIndices.Count > 0)
                {
                    for (int i = 0; i < TraItemLV.SelectedItems.Count; i++)
                    {
                        int a = TraItemLV.SelectedItems[i].Index;
                        SteamAutoFunction.TrackingList[a].BuyPercent = Convert.ToInt32(BuyPricTB.Text);
                        SteamAutoFunction.TrackingList[a].SellPercent = Convert.ToInt32(SelPricTB.Text);        
                        TraItemLV.SelectedItems[i].SubItems[6].Text = BuyPricTB.Text;
                        TraItemLV.SelectedItems[i].SubItems[7].Text = SelPricTB.Text;
                    }               
                }
            }
            catch
            {
                MessageBox.Show("Please enter the correct number in boxes!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DefaultB_Click(object sender, EventArgs e)
        {
            BuyPricTB.Text = "20";
            SelPricTB.Text = "150";
        }

        private void button9_Click(object sender, EventArgs e)
        {
            int inventory;

            if (SteamAutoFunction.Logged)
            {
                button9.Enabled = false;
                InvenLV.Enabled = false;               
                StatusPB.Image = Properties.Resources.icon1;
                StatusLabel1.Text = "Loading";

                lastSelec = -1;

                button1.Enabled = false;
                if (comboBox1.SelectedIndex == 8)
                {
                    inventory = 8;
                    SteamAutoFunction.LoadOnSale = true;
                    button7.Text = "Remove sell";
                    textBox1.Clear();
                    textBox2.Clear();
                    SteamAutoFunction.IsRemove = true;
                    textBox1.ReadOnly = true;
                    textBox2.ReadOnly = true;
                }
                else
                {
                    SteamAutoFunction.LoadOnSale = false;
                    inventory = comboBox1.SelectedIndex;
                    button7.Text = "Sell now";
                    SteamAutoFunction.IsRemove = false;
                    textBox1.ReadOnly = false;
                    textBox2.ReadOnly = false;
                }
                button7.Enabled = false;
                InvenLV.Items.Clear();
                SteamAuto.InventoryThreadExcute(inventory);
            }
            else
            {
                MessageBox.Show("Please log in first!", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void FillInventoryList()
        {
            InvenLV.Items.Clear();

            for (int i = 0; i < SteamParsing.InventList.Count; i++)
            {
                var ourItem = SteamParsing.InventList[i];

                string priceRes; string pricemes;
                if (ourItem.MedianPrice.Equals("0"))
                {
                    pricemes = "Not yer Refreshed";
                }
                else
                {
                    pricemes = SteamParsing.Currencies.GetName() + " " + ourItem.MedianPrice;
                }
                if (ourItem.BuyPrice == "0")
                {
                    priceRes = "Not yet Refreshed";
                }
                else if (ourItem.BuyPrice == "1")
                {
                    priceRes = "Not Tradable";
                }
                else
                {
                    priceRes = SteamParsing.Currencies.GetName() + " " + SteamLibrary.DoFracture(ourItem.BuyPrice);
                }                    

                string[] row = { ourItem.Type, ourItem.Name, priceRes, pricemes };
                var lstItem = new ListViewItem(row);
                InvenLV.Items.Add(lstItem);
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (InvenLV.Items.Count > 0)
            {
                button11.Enabled = false;
                InvenLV.Enabled = false;

                for (int i = 0; i < InvenLV.Items.Count; i++ )
                {
                    if (!InvenLV.Items[i].SubItems[2].Text.Contains("Not Tradable"))
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(SteamAuto.RefreshProgress), new object[] 
                        { SteamParsing.InventList[i], i });                                  
                    }
                }                          
            }
            else
            {
                MessageBox.Show("No item to refresh!", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (TraItemLV.SelectedIndices.Count != 0)
            {
                var item = SteamAutoFunction.TrackingList[TraItemLV.SelectedIndices[0]];
                DialogResult result = MessageBox.Show("Do you want to buy " + item.Name + "?", "Attention", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    SteamAuto.BuyThreadExcute(item.Link, true, true, false);
                    button12.Enabled = false;
                }                                                      
            }             
        }

        private void InvenLV_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (InvenLV.SelectedItems.Count == 1)
            {

                var lit = InvenLV.SelectedItems[0];

                if (lastSelec != lit.Index)
                {
                    var ourItem = SteamParsing.InventList[InvenLV.SelectedIndices[0]];
                    SteamParsing.SteamSite.StartLoadImgTread(string.Format(SteamLibrary.fndImgUrl, ourItem.ImgLink), pictureBox2);

                    lastSelec = lit.Index;

                    if (!InvenLV.SelectedItems[0].SubItems[2].Text.Contains("Not Tradable") && (comboBox1.SelectedIndex != 8))
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(SteamAuto.RefreshProgress), new object[] 
                        { SteamParsing.InventList[InvenLV.SelectedIndices[0]], InvenLV.SelectedIndices[0] });
                    }
                }

                textBox2_KeyUp(this, null);
            }
            else
            {
                pictureBox2.Image = null;
            } 
        }

        private void textBox2_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (textBox2.Text != string.Empty)
                {
                    textBox1.Text = SteamLibrary.DoFracture(SteamLibrary.CalcWithFee(SteamLibrary.GetSweetPrice(textBox2.Text)));
                }
            }
            catch (Exception)
            {

            }
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (textBox1.Text != string.Empty)
                {
                    textBox2.Text = SteamLibrary.DoFracture(SteamLibrary.AddFee(SteamLibrary.GetSweetPrice(textBox1.Text)));
                }
            }
            catch (Exception)
            {

            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (TraItemLV.SelectedIndices.Count > 0)
            {
                for (int i = 0; i < TraItemLV.SelectedItems.Count; i++)
                {
                    int a = TraItemLV.SelectedItems[i].Index;
                    SteamAutoFunction.TrackingList[a].Refresh.Stop();
                    SteamAutoFunction.TrackingList[a].Refresh.Start();
                }
            }         
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (TraItemLV.SelectedIndices.Count > 0)
            {
                for (int i = 0; i < TraItemLV.SelectedItems.Count; i++)
                {
                    int a = TraItemLV.SelectedItems[i].Index;
                    TraItemLV.SelectedItems[i].SubItems[13].Text = "";
                    SteamAutoFunction.TrackingList[a].Refresh.Stop();
                }                
            }         
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != 8)
            {
                string truePrice = string.Empty;
                if (textBox2.Text == string.Empty)
                {
                    MessageBox.Show("Wrong Price!", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else
                {
                    truePrice = SteamLibrary.GetSweetPrice(textBox2.Text);
                    if (truePrice == string.Empty)
                    {
                        MessageBox.Show("WrongPrice", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                for (int i = 0; i < InvenLV.SelectedIndices.Count; i++)
                {
                    var ourItem = SteamParsing.InventList[InvenLV.SelectedIndices[i]];
                    ourItem.BuyPrice = truePrice;
                    InvenLV.SelectedItems[i].SubItems[2].Text = SteamParsing.Currencies.GetName() + " " + textBox2.Text;
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (SteamParsing.InventList.Count == 0)
            {
                MessageBox.Show("Please load inventory first!", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (InvenLV.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select one item!", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                int app = comboBox1.SelectedIndex;

                SteamAutoFunction.SellingList.Clear();

                for (int i = 0; i < InvenLV.SelectedItems.Count; i++)
                {
                    var ouritem = SteamParsing.InventList[InvenLV.SelectedItems[i].Index];
                    if ((ouritem.Marketable) && (!ouritem.BuyPrice.Equals("0")))
                    {
                        SteamAutoFunction.SellingList.Add(new SteamLibrary.ItemToSell(ouritem.AssetId,
                            SteamLibrary.CalcWithFee(ouritem.BuyPrice), SteamLibrary.GetAppFromIndex(app, false)));
                    }
                }

                if (SteamAutoFunction.SellingList.Count != 0)
                {
                    if (SteamAutoFunction.IsRemove)
                    {
                        StatusLabel1.Text = "Removing";
                        StatusPB.Image = Properties.Resources.icon1;
                    }
                    else
                    {
                        StatusLabel1.Text = "Add to sell";
                        StatusPB.Image = Properties.Resources.icon1;
                    }

                    //steam_srch.sellDelay = Convert.ToInt32(sellDelayBox.Text);
                    //steam_srch.isDelayRand = randomDelayBox.Checked;
                    SteamAuto.SellThreadExcute();
                }
                else
                {
                    MessageBox.Show("Select items to sell first!", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }              
            }
        }

        private string itemName;
        private bool isGame;
        private string Game;
        private bool isMin;
        private string min;
        private bool isMax;
        private string max;
        public int searchCount;

        private void button15_Click(object sender, EventArgs e)
        {
            Advance AdvanceBox = new Advance();

            if (AdvanceBox.ShowDialog() == DialogResult.OK)
            {
                itemName = AdvanceBox.SearchName;
                isGame = AdvanceBox.IsGame;
                Game = AdvanceBox.Game;
                isMin = AdvanceBox.IsMin;
                min = AdvanceBox.Minimum;
                isMax = AdvanceBox.IsMax;
                max = AdvanceBox.Maximum;
                searchCount = AdvanceBox.searchCount;
                AdvanceBox.Dispose();
                SteamAuto.DoAdvanceSearch(0, searchCount, itemName, isGame, Game, isMin, min, isMax, max);
                ScanLV.Enabled = false;
                button2.Enabled = false;
                StatusPB.Image = Properties.Resources.icon1;
                StatusLabel1.Text = "Searching";
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            ScanLV.Enabled = false;
            button2.Enabled = false;
            button15.Enabled = false;
            SteamAuto.DoAdvanceSearch(1, searchCount, Name, isGame, Game, isMin, min, isMax, max);
            StatusPB.Image = Properties.Resources.icon1;
            StatusLabel1.Text = "Searching";
            prepageBT.Visible = false;
            nextpageBT.Visible = false;
            button16.Visible = false;
            button17.Visible = false;
        }

        private void button16_Click(object sender, EventArgs e)
        {
            ScanLV.Enabled = false;
            button2.Enabled = false;
            button15.Enabled = false;
            SteamAuto.DoAdvanceSearch(2, searchCount, Name, isGame, Game, isMin, min, isMax, max);
            StatusPB.Image = Properties.Resources.icon1;
            StatusLabel1.Text = "Searching";
            prepageBT.Visible = false;
            nextpageBT.Visible = false;
            button16.Visible = false;
            button17.Visible = false;
        }
    }

    public static class FlashWindow
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        public const uint FLASHW_ALL = 3;
        public const uint FLASHW_TIMERNOFG = 12;

        public static bool Flash(System.Windows.Forms.Form form)
        {
            if (Win2000OrLater)
            {
                FLASHWINFO fi = Create_FLASHWINFO(form.Handle, FLASHW_ALL | FLASHW_TIMERNOFG, uint.MaxValue, 0);
                return FlashWindowEx(ref fi);
            }
            return false;
        }

        private static FLASHWINFO Create_FLASHWINFO(IntPtr handle, uint flags, uint count, uint timeout)
        {
            FLASHWINFO fi = new FLASHWINFO();
            fi.cbSize = Convert.ToUInt32(Marshal.SizeOf(fi));
            fi.hwnd = handle;
            fi.dwFlags = flags;
            fi.uCount = count;
            fi.dwTimeout = timeout;
            return fi;
        }

        private static bool Win2000OrLater
        {
            get { return System.Environment.OSVersion.Version.Major >= 5; }
        }
    }
}
