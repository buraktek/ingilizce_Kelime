using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ingilizce_Kelime
{
    public partial class TR_Anlam : Form
    {
        public TR_Anlam()
        {
            //this.Location = new System.Drawing.Point((Cursor.Position.X+10), (Cursor.Position.Y+10));
            string temp = "";
            bool basarili = false;
            InitializeComponent();
            Uri url = new Uri("https://tureng.com/tr/turkce-ingilizce/" + ingilizce_Kelime.global.word);
            //WebClient client = new WebClient();
            WebClient wc = new WebClientWithTimeout1();
            wc.Encoding = System.Text.Encoding.UTF8;
            string html = wc.DownloadString(url);
            // Adresten istek yapı html kodlarını indiriyoruz.     

            HtmlAgilityPack.HtmlDocument dokuman = new HtmlAgilityPack.HtmlDocument();
            dokuman.LoadHtml(html);
            // İndirdiğimiz html kodlarını bir HtmlDocment nesnesine yüklüyoruz.     
            HtmlNodeCollection tr = dokuman.DocumentNode.SelectNodes("//td[@class='en tm']");
            HtmlNodeCollection en = dokuman.DocumentNode.SelectNodes("//td[@class='tr ts']");
            int sayac = 0;
            foreach (var baslik in tr)
            {
                if (baslik.FirstChild.InnerText == ingilizce_Kelime.global.word)
                {
                    temp = baslik.FirstChild.InnerText + ":   " + baslik.ChildNodes["i"].InnerText + " ";

                    int temp_int = 0;
                    foreach(var turkce in en)
                    {
                        if (temp_int == sayac)
                        {
                            
                            temp += ReplaceText(turkce.FirstChild.InnerText);
                            listBox1.Items.Add(temp);
                            basarili = true;
                            break;
                        }
                        temp_int++;
                    }
                    sayac++;
                }
                else
                    break;
            }
            if (sayac == 0)
            {
                HtmlNodeCollection tr2 = dokuman.DocumentNode.SelectNodes("//td[@class='en tm']");
                HtmlNodeCollection en2 = dokuman.DocumentNode.SelectNodes("//td[@class='tr ts']");
                sayac = 0;
                foreach (var baslik2 in tr2)
                {
                    temp = ReplaceText(baslik2.FirstChild.InnerText) + ":   " + baslik2.ChildNodes["i"].InnerText + " ";

                    int temp_int = 0;
                    foreach (var turkce2 in en2)
                    {
                        if (temp_int == sayac)
                        {
                            basarili = true;
                            temp += ReplaceText(turkce2.FirstChild.InnerText);
                            listBox1.Items.Add(temp);
                            break;
                        }
                        temp_int++;
                    }
                    sayac++;
                    if (sayac == 20)
                        break;
                }
            }

            #region Pencere Boyutunu Ayarla

            int i, max_uzunluk_x = 0, max_uzunluk_y = 384 ;
            for(i = 0; i<listBox1.Items.Count; i++)
            {
                if (max_uzunluk_x < listBox1.Items[i].ToString().Length)
                    max_uzunluk_x = listBox1.Items[i].ToString().Length;
            }
            if (i < 8)
            {
                max_uzunluk_y = i * 17 + 120;
            }
            else if (i<15)
            {
                max_uzunluk_y = i * 17 + 100;
            }
            if(max_uzunluk_x>50)
            {
                max_uzunluk_x *= 6;
                max_uzunluk_x += 15;
                if (max_uzunluk_x > 800)
                    max_uzunluk_x = 800;
                this.Size = new System.Drawing.Size(max_uzunluk_x,max_uzunluk_y);
            }
            else
            {
                if (max_uzunluk_x < 350)
                    max_uzunluk_x = 350;

                this.Size = new System.Drawing.Size(max_uzunluk_x,max_uzunluk_y);
            }
            if(basarili)
            {
                if (!global.kaydetme)
                {
                    ingilizce_Kelime.Properties.Settings.Default.history += "#" + ingilizce_Kelime.global.word + "@" + turkce_5_anlamini_dondur(ingilizce_Kelime.global.word)/*+ "*\\"*/ ;
                    ingilizce_Kelime.Properties.Settings.Default.Save();
                    ingilizce_Kelime.global.yenile = true;
                    ((Form1)Application.OpenForms["Form1"]).gecmisi_goster();
                }
                global.kaydetme = false;
            }
            #endregion
        }
        public string ReplaceText(string _text)
        {
            _text = _text.Replace("&#231;", "ç").Replace("&#252;", "ü").Replace("&#246;", "ö").Replace("&#39;", "'").Replace("Ä±","ı").Replace("ÄŸ","ğ");
            return _text;
        }

        private void TR_Anlam_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\u001b')
            {
                this.Close();
            }
        }

        private void pictureBox_sesli_sozluk_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://seslisozluk.net/" + ingilizce_Kelime.global.word + "-nedir-ne-demek/" );
        }

        private void pictureBox_tureng_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://tureng.com/tr/turkce-ingilizce/" + ingilizce_Kelime.global.word);
        }

        private void pictureBox_yandex_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://yandex.com.tr/gorsel/search?text=" + ingilizce_Kelime.global.word);
        }

        private void button_favori_Click(object sender, EventArgs e)
        {
            string temp = listBox1.Items[0].ToString();
            string kelime = temp.Substring(0, temp.IndexOf(":"));
            int uzunluk = listBox1.Items.Count;
            int limit = 0;
            for(int i=1;i< listBox1.Items.Count; i++)
            {
                string temp2 = listBox1.Items[i].ToString();
                if (temp2.Substring(0, temp2.IndexOf(":")) == kelime)
                {
                    temp += (temp2.Substring(temp2.IndexOf(":") + 1, temp2.Length - temp2.IndexOf(":") - 1) + ",").Replace("#", "").Replace(":", "");
                    limit++;
                    if (limit > 5)
                        break;
                }
            }
            ((Form1)Application.OpenForms["Form1"]).kelime_ekle(kelime, " ", turkce_5_anlamini_dondur(kelime));
        }

        private string turkce_5_anlamini_dondur(string ingilizce_kelime)
        {
            string turkce_anlamlar = "";
            int limit = 0;
            string[] eng_kelimeler = new string[500];
            string[] tr_anlamlari = new string[500];
            InitializeComponent();
            Uri url = new Uri("https://tureng.com/tr/turkce-ingilizce/" + ingilizce_kelime);
            //WebClient client = new WebClient();
            WebClient wc = new WebClientWithTimeout();
            wc.Encoding = System.Text.Encoding.UTF8;
            string html = wc.DownloadString(url);
            // Adresten istek yapı html kodlarını indiriyoruz.     

            HtmlAgilityPack.HtmlDocument dokuman = new HtmlAgilityPack.HtmlDocument();
            dokuman.LoadHtml(html);
            // İndirdiğimiz html kodlarını bir HtmlDocment nesnesine yüklüyoruz.     
            HtmlNodeCollection tr = dokuman.DocumentNode.SelectNodes("//td[@class='en tm']");
            HtmlNodeCollection en = dokuman.DocumentNode.SelectNodes("//td[@class='tr ts']");
            int sayac = 0;
            foreach (var baslik in tr)
            {
                if (baslik.FirstChild.InnerText == ingilizce_kelime)
                {
                    int temp_int = 0;
                    foreach (var turkce in en)
                    {
                        if (temp_int == sayac)
                        {
                            turkce_anlamlar += turkce.FirstChild.InnerText;
                            limit++;
                            if (limit != 5)
                                turkce_anlamlar += ", ";
                            else
                                goto atla;
                            break;
                        }
                        temp_int++;
                    }
                    sayac++;
                }
                else
                    break;
            }
        atla:

            turkce_anlamlar = yaziyi_duzelt(turkce_anlamlar);


            return turkce_anlamlar;
        }

        public string yaziyi_duzelt(string _text)
        {
            _text = _text.Replace("&#231;", "ç").Replace("&#252;", "ü").Replace("&#246;", "ö").Replace("&#39;", "'").Replace("Ä±", "ı").Replace("ÄŸ", "ğ");
            return _text;
        }


    }

   
    public class WebClientWithTimeout1 : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest wr = base.GetWebRequest(address);
            wr.Timeout = 5000; // timeout in milliseconds (ms)
            return wr;
        }
    }
}
