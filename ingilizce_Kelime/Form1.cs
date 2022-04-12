using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Globalization;
using System.Net;
using System.IO;
using Microsoft.Win32;
using HtmlAgilityPack;

namespace ingilizce_Kelime
{

    public partial class Form1 : Form
    {
        MessageBoxManager mesaj = new MessageBoxManager();
        Random rastgele = new Random();
        bool secildi_mi = false;
        bool busy = false;
        bool basla = false;
        string[] kutuptane_paths = new string[50];
        int[] kalici_index_hafiza = new int[500];
        int gunluk_kelime_sayisi = 0;
        int gunluk_kelime_limiti = 10;
        int gozukme_suresi = 50000;
        int ezberlenen = 0;
        int ezberlenecek = 0;
        int istenmeyen = 0;
        int toplam = 0;
        string float_point = "";
        DialogResult secenek;
        Bitmap bitmap;
        private string imageUrl;
        string source1 = "libs\\eng-tr.mdb";
        private OleDbConnection baglanti;
        int islem_yapilacak_kelime_indexi = 0;
        System.Windows.Forms.ToolTip ToolTip1 = new System.Windows.Forms.ToolTip();
        string[] sources = new string[500];

        KeyboardHook hook = new KeyboardHook();
        public Form1()
        {
            InitializeComponent();
            kutuphane_sirala();
            gecmisi_goster();
            if (ingilizce_Kelime.Properties.Settings.Default.last_location.Length > 0)
            {
                source1 = ingilizce_Kelime.Properties.Settings.Default.last_location;
                int counter = metroComboBox_libs1.Items.Count;
                while (counter>0)
                {
                    if (source1 == sources[counter-1])
                    {
                        metroComboBox_libs1.SelectedIndex = counter-1;
                        break;
                    }
                    counter--;
                }
            }
            baglanti = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + source1);
            notifyIcon1.Visible = true;
            Ezberlenen_Kelime_Sayisi();
            notifyIcon1.ShowBalloonTip(3000);
            for (int i = 0; i < 10; i++)
                kalici_index_hafiza[i] = 0;

            MessageBoxManager.Yes = "Ezberledim";
            MessageBoxManager.No = "İstemiyorum";
            MessageBoxManager.Cancel = "İptal";
            MessageBoxManager.Register();

            CultureInfo ci = CultureInfo.CurrentUICulture;
            float_point = (Convert.ToDouble(0.1, new CultureInfo(ci.Name))).ToString().Substring(1, 1);
            global.ing_kelime = "ball";
            ToolTip1.AutoPopDelay = 15000;

            // register the event that is fired after the key press.
            hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);
            // register the control + alt + F12 combination as hot key.
            hook.RegisterHotKey(ingilizce_Kelime.ModifierKeys.Control /*| ModifierKeys.Alt*/, Keys.F12);


            metroTabControl1.TabPages.RemoveAt(2);            //TODO: Kütüphane düzenleme kısmını ekle;

        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            try
            {
                if (Clipboard.GetText().Length > 0)
                {
                    string word = Clipboard.GetText().ToString().ToLower().Replace("#", "").Replace("!", "").Replace("_", "").Replace(".", "").Replace(",", "");
                    if (word.Substring(word.Length - 1, 1) == " ")
                        word = word.Substring(0, word.Length - 1);
                    ingilizce_Kelime.global.word = word;
                    TR_Anlam f2 = new TR_Anlam();
                    f2.StartPosition = FormStartPosition.Manual;
                    f2.Left = Cursor.Position.X + 10;
                    f2.Top = Cursor.Position.Y + 10;
                    f2.Show();
                }
            }
            catch
            {
                MessageBox.Show("Seçilen kelime sözlükte bulunamadı", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void gecmise_ekle(string kelime, string anlam)
        {
            dataGridView1.Rows.Add(dataGridView1.Rows.Count, kelime, anlam);
        }

        public void gecmisi_goster()
        {
            dataGridView1.Rows.Clear();
            String[] dosya_split = ingilizce_Kelime.Properties.Settings.Default.history.Split('#');
            int temp_index = 0, anlam_index = 0;
            foreach (String satir in dosya_split)
            {
                if (satir != "")
                {
                    dataGridView1.Rows.Add();
                    dataGridView1.Rows[temp_index].Cells[0].Value = temp_index + 1;
                    anlam_index = satir.IndexOf("@");
                    dataGridView1.Rows[temp_index].Cells[1].Value = satir.Substring(0, anlam_index);
                    dataGridView1.Rows[temp_index++].Cells[2].Value = satir.Substring(anlam_index+1, satir.Length - anlam_index - 1);
                }
            }
        }

        private void kutuphane_sirala()
        {
            int i = 0;
            string[] dosyalar = Directory.GetFiles("libs\\");
            metroComboBox_lib.Items.Clear();
            metroComboBox_libs1.Items.Clear();
            foreach (string dosya in dosyalar)
            {
                sources[i++] = dosya;
                metroComboBox_lib.Items.Add(dosya.Replace("libs\\", "").Replace(".mdb",""));
                metroComboBox_libs1.Items.Add(dosya.Replace("libs\\", "").Replace(".mdb", ""));
            }
        }
        private void Ezberlenen_Kelime_Sayisi()
        {
            /*
            #region deneme
            string sorgu2 = "select count (A) FROM Amber";
            OleDbCommand komut = new OleDbCommand(sorgu2, baglanti);
            baglanti.Open();
            toplam_sayac_sayisi = Convert.ToInt32(komut.ExecuteScalar().ToString());
            baglanti.Close();
            #endregion
            */
            #region silme

            try
            {
                {
                    string silmeSorgusu = "DELETE from Amber Where A=@musterino";
                    //musterino parametresine bağlı olarak müşteri kaydını silen sql sorgusu
                    OleDbCommand silKomutu = new OleDbCommand(silmeSorgusu, baglanti);
                    baglanti.Open();
                    silKomutu.Parameters.AddWithValue("@musterino", "");
                    silKomutu.ExecuteNonQuery();
                    baglanti.Close();
                    //MessageBox.Show("Kayıt Silindi...");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                //MessageBox.Show("Bitti");
            }

            #endregion



            ezberlenen = 0;
            ezberlenecek = 0;
            istenmeyen = 0;
            toplam = 0;
            string temp, sorgu = "SELECT A FROM Amber";
            OleDbCommand komut2 = new OleDbCommand(sorgu, baglanti);
            baglanti.Open();
            busy = true;
            OleDbDataReader oku2 = komut2.ExecuteReader();
            while (oku2.Read())
            {
                temp = oku2.GetString(0);
                if (temp == "+")
                {
                    ezberlenen++;
                }
                else if (temp == "x")
                {
                    istenmeyen++;
                }
                else if (temp == "-")
                    ezberlenecek++;
                else if (temp == "")
                    break;
            }
            toplam = ezberlenecek + ezberlenen + istenmeyen;
            oku2.Close();
            baglanti.Close();
            busy = false;
            label_ezberlenen.Text = ezberlenen.ToString();
            label_ezberlenecek.Text = ezberlenecek.ToString();
            label_yoksayilan_kelime_sayisi_.Text = istenmeyen.ToString();
            label_toplam.Text = toplam.ToString();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!busy)
            {
                basla = true;
                string sorgu;
                string ezberlendi_mi = "";
                OleDbDataReader oku;

            #region Kelime Seç
            tekrar:
                if (gunluk_kelime_sayisi < gunluk_kelime_limiti)
                {
                    global.kelime_index = rastgele.Next(0, 2500);

                    for (int k = 0; k < gunluk_kelime_sayisi; k++) //seçilen kelimelerden biri mi?
                    {
                        if (kalici_index_hafiza[k] == global.kelime_index)
                            goto tekrar;
                    }

                    kalici_index_hafiza[gunluk_kelime_sayisi] = global.kelime_index;
                    gunluk_kelime_sayisi++;
                }
                else
                {
                    int rast = rastgele.Next(0, gunluk_kelime_limiti);
                    global.kelime_index = kalici_index_hafiza[rast];
                }

                #endregion Kelime Seç

                sorgu = "SELECT A,B,C,D,E FROM Amber Where B like '" + global.kelime_index.ToString() + "%'";
                OleDbCommand komutt = new OleDbCommand(sorgu, baglanti);

                baglanti.Open();
                busy = true;
                oku = komutt.ExecuteReader();

                if (oku.Read())
                {
                    ezberlendi_mi = oku.GetString(0);
                    if (ezberlendi_mi != "-")       //ezberlenen bir kelime mi?
                    {
                        baglanti.Close();
                        goto tekrar;
                    }

                    notifyIcon1.BalloonTipTitle = oku.GetString(2) + "   (" + oku.GetString(3) + ")";
                    global.ingilizce = notifyIcon1.BalloonTipTitle;
                    global.ing_kelime = oku.GetString(2);
                    notifyIcon1.BalloonTipText = oku.GetString(4);
                    global.turkce = notifyIcon1.BalloonTipText;

                }
                baglanti.Close();
                busy = false;
                if (checkBox_photo.Checked)
                {
                    Foto_Goster();
                    if (!global.show)
                    {
                        Form2 f2 = new Form2();
                        f2.Show();
                    }
                }
                notifyIcon1.ShowBalloonTip(gozukme_suresi);
            }
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            try
            {
                this.Visible = true;
                Ezberlenen_Kelime_Sayisi();
                try
                {
                    timer3.Enabled = true;
                    //ShowInTaskbar = true;
                    this.WindowState = FormWindowState.Normal;
                }
                catch
                { }
            }
            catch
            {
                MessageBox.Show("notfyicon error!");
            }
        }
        public class global
        {
            public static string turkce;
            public static string ingilizce;
            public static int kelime_index = 999999;
            public static string url = "";
            public static string ing_kelime = "";
            public static bool show = false;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (!busy)
            {
                if (secenek == DialogResult.Yes)    //Kelime Ezberlendi Seçildiyse
                {
                    string sorgu = "Update Amber Set A=@A Where B like @B";
                    using (OleDbCommand komut = new OleDbCommand(sorgu, baglanti))
                    {
                        komut.Parameters.Add("@A", OleDbType.VarChar).Value = "+";
                        komut.Parameters.Add("@B", OleDbType.VarChar).Value = islem_yapilacak_kelime_indexi.ToString();
                        busy = true;
                        baglanti.Open();
                        komut.ExecuteNonQuery();
                        baglanti.Close();
                    }
                    busy = false;
                    secildi_mi = false;
                    timer2.Enabled = false;
                    for (int k = 0; k < gunluk_kelime_limiti; k++)
                    {
                        if (kalici_index_hafiza[k] == islem_yapilacak_kelime_indexi)
                        {
                            kalici_index_hafiza[k] = 0;
                            gunluk_kelime_sayisi--;
                        }
                    }
                }

                else if (secenek == DialogResult.Cancel)
                {
                    secildi_mi = false;
                    timer2.Enabled = false;
                }

                else if (secenek == DialogResult.No)
                {
                    string sorgu1 = "Update Amber Set A=@A Where B=@B";
                    using (OleDbCommand komut = new OleDbCommand(sorgu1, baglanti))
                    {
                        komut.Parameters.AddWithValue("@A", "x");
                        komut.Parameters.AddWithValue("@B", islem_yapilacak_kelime_indexi);
                        busy = true;
                        baglanti.Open();
                        komut.ExecuteNonQuery();
                        baglanti.Close();
                    }
                    busy = false;
                    secildi_mi = false;
                    timer2.Enabled = false;
                    for (int k = 0; k < gunluk_kelime_limiti; k++)
                    {
                        if (kalici_index_hafiza[k] == islem_yapilacak_kelime_indexi)
                        {
                            kalici_index_hafiza[k] = 0;
                            gunluk_kelime_sayisi--;
                        }
                    }
                }
                Ezberlenen_Kelime_Sayisi();
            }
        }

        private void textBox_gozukme_suresi_TextChanged(object sender, EventArgs e)
        {
            if (textBox_gozukme_suresi.TextLength > 0)
                gozukme_suresi = Convert.ToInt32(textBox_gozukme_suresi.Text) * 1000;
        }

        private void Only_Tab_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        #region Actions

        private void Only_Numeric_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Verify that the pressed key isn't CTRL or any non-numeric digit
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar == ','))
            {
                e.Handled = true;
            }
        }

        private void Only_Numeric_And_Floating_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.') && (e.KeyChar != ','))
            {
                e.Handled = true;
            }
            char ch_float_point = Convert.ToChar(float_point);
            // If you want, you can allow decimal (float) numbers
            if ((e.KeyChar == ',') || (e.KeyChar == '.'))
            {
                e.KeyChar = ch_float_point;
                if (((sender as TextBox).Text.IndexOf(ch_float_point) > -1) || ((sender as TextBox).Text.Length == 0))
                    e.Handled = true;
            }
        }

        #endregion Actions

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox_toplam_kelime_sayisi.TextLength > 0)
            {
                gunluk_kelime_limiti = Convert.ToInt32(textBox_toplam_kelime_sayisi.Text);

                if (gunluk_kelime_sayisi > gunluk_kelime_limiti)
                    gunluk_kelime_sayisi = gunluk_kelime_limiti + 1;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if ((textBox3.TextLength > 0) && (textBox3.Text != "0"))
                {
                    double temp = Convert.ToDouble(textBox3.Text);
                    temp *= 60000;
                    if (temp < 7000)
                        temp = 7000;
                    timer1.Interval = Convert.ToInt32(temp);
                }
            }
            catch
            { }
        }

        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            if ((!secildi_mi) && (basla))
            {
                secildi_mi = true;
                this.TopMost = true;
                secenek = 0;
                System.Threading.Thread.Sleep(5);
                secenek = MessageBox.Show("Kelimeyi Ezberledin mi? Yoksa Ezberlemek istemiyor musun?", "İnglizce - Türkçe", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                islem_yapilacak_kelime_indexi = global.kelime_index;
                System.Threading.Thread.Sleep(5);
                timer2.Enabled = true;
                this.Focus();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.ShowInTaskbar = false;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
            }
        }

        Bitmap Resim(string Url)
        {
            WebRequest rs = WebRequest.Create(Url);
            return (Bitmap)Bitmap.FromStream(rs.GetResponse().GetResponseStream());
        }

        public void Foto_Goster()
        {
            string hedef = "https://yandex.com.tr/gorsel/search?text=" + global.ing_kelime;
            string temp = "", taranacak_string = ".png";


            WebRequest SiteyeBaglantiTalebi = HttpWebRequest.Create(hedef);
            WebResponse GelenCevap = SiteyeBaglantiTalebi.GetResponse();
            StreamReader CevapOku = new StreamReader(GelenCevap.GetResponseStream(), Encoding.GetEncoding("windows-1254"));
            string KaynakKodlar = CevapOku.ReadToEnd();

            int baslangic2 = KaynakKodlar.IndexOf(taranacak_string);
            if (baslangic2 != -1)
            {
                temp = KaynakKodlar.Substring(baslangic2 + 4 - 200, 200);
                baslangic2 = temp.LastIndexOf("http");
                if (baslangic2 != -1)
                    temp = temp.Substring(baslangic2, 200 - baslangic2);

            }


            try
            {
                global.url = temp;
                bitmap = new Bitmap("temp");
                //pictureBox1.Image = Resim(@"temp");
                //pictureBox1.Image = Resim(@"MRExFXXv2+t5DTB/v2vJ1nO1nS1bleD8vb/AEwOHBpRRZWKmhtHvu4vt+WemJN4AZA1cUP2toPmG+/auf559jTlT2IP++c/bSgqe59Xv8vz+mB4WLuyAk7bSxVDg9jX++clOckIGZKJ7gkdxYv+H+udbA4XxVptyI37JI/jX+WR2PRljQBJ+n0GTnVwCRCp9/6+2Rz7OVbtRB/32wOZPAoI2BhwLv8Aa96yVdP6qjoNzBX974s/MZxfs+fRhvv3wOl1rqSeWyKQxYV8wB7k5xdNpLFUwJC0QfYEEn8uw7HkfPPSNN7gdq7/AO+15+8nI4FV+GhVc2f64HNeDzHpSTfAs3fB7kjk853PhYVCRX6x/oM58aEEEd1Nj6c5INBEVjF9zZP5k3gejGMYDGMYDGMYDGMYDGMYDGMYDGMYH5yxBu+fgdNnrxgeWLT0Qc9WMYDPPqdKG59/656MYHLbSke2Ps/0zqZ/No+WBzBp8/v2fj/TOjtGf0DA8UGi5s/wz3YxgMYxgMYxgMYxgMYxgMYxgMYxgMYxgMYxgMYxgMYxgMYxgMYxgMYxgMYxgMYxgMYxgf/Z");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void MetroButton1_Click(object sender, EventArgs e)
        {
            try
            {
                OleDbConnection baglanti2 = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + sources[metroComboBox_lib.SelectedIndex]);
                baglanti2.Open();
                OleDbCommand komut = new OleDbCommand("INSERT INTO Amber (A, B, C, D, E) VALUES ('" + "-" + "' , '" + (toplam + 1).ToString() + "','" + metroTextBox_ingilizce.Text + "', '" + metroTextBox3.Text + "', '" + metroTextBox_turkce.Text + "' )", baglanti2);
                komut.ExecuteNonQuery();
                komut.Dispose();
                baglanti2.Close();
                Ezberlenen_Kelime_Sayisi();
                MessageBox.Show("Kelime Eklendi", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                metroTextBox_ingilizce.Text = metroTextBox_turkce.Text = metroTextBox3.Text = "";
            }
            catch
            {
                MessageBox.Show("Eklenemedi!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            if(kelime_ekle(metroTextBox_ingilizce.Text, metroTextBox3.Text , metroTextBox_turkce.Text))
                metroTextBox_ingilizce.Text = metroTextBox_turkce.Text = metroTextBox3.Text = "";
        }

        public bool kelime_ekle(string ingilizce_kelime, string okunusu, string turkce_anlam)
        {
            try
            {
                OleDbConnection baglanti2 = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + sources[metroComboBox_libs1.SelectedIndex]);
                baglanti2.Open();
                OleDbCommand komut = new OleDbCommand("INSERT INTO Amber (A, B, C, D, E) VALUES ('" + "-" + "' , '" + (toplam + 1).ToString() + "','" + ingilizce_kelime + "', '" + okunusu + "', '" + turkce_anlam + "' )", baglanti2);
                komut.ExecuteNonQuery();
                komut.Dispose();
                baglanti2.Close();
                Ezberlenen_Kelime_Sayisi();
                //MessageBox.Show("Kelime Eklendi", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;

            }
            catch
            {
                //MessageBox.Show("Eklenemedi!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
            tekrar:
                int random = rastgele.Next(0, 2500);
                string durum = "";
                string sorgu = "SELECT A,B,C,D,E FROM Amber Where B like '" + random.ToString() + "%'";
                OleDbCommand komutt = new OleDbCommand(sorgu, baglanti);
                OleDbDataReader oku;

                baglanti.Open();
                busy = true;
                oku = komutt.ExecuteReader();

                if (oku.Read())
                {
                    durum = oku.GetString(0);
                    if (durum != "-")       //ezberlenen bir kelime mi?
                    {
                        baglanti.Close();
                        goto tekrar;
                    }

                    label_EN.Text = oku.GetString(2);
                    label_okunus.Text = "(" + oku.GetString(3) + ")";
                    label_TR.Text = oku.GetString(4);
                }
                baglanti.Close();
                busy = false;
            }
            catch
            {
                ;//   MessageBox.Show(exp);
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            string sorgu = "Update Amber Set A=@A Where C=@C";
            OleDbCommand komut = new OleDbCommand(sorgu, baglanti);
            komut = new OleDbCommand(sorgu, baglanti);

            komut.Parameters.AddWithValue("@A", "+");
            komut.Parameters.AddWithValue("@B", label_EN.Text);
            busy = true;
            baglanti.Open();
            komut.ExecuteNonQuery();
            baglanti.Close();

            Button1_Click(null, null);
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            string sorgu = "Update Amber Set A=@A Where C=@C";
            OleDbCommand komut = new OleDbCommand(sorgu, baglanti);
            komut = new OleDbCommand(sorgu, baglanti);

            komut.Parameters.AddWithValue("@A", "x");
            komut.Parameters.AddWithValue("@B", label_EN.Text);
            busy = true;
            baglanti.Open();
            komut.ExecuteNonQuery();
            baglanti.Close();

            Button1_Click(null, null);
        }

        private void MetroTabPage1_Enter(object sender, EventArgs e)
        {
            Ezberlenen_Kelime_Sayisi();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue("My Program", "\"" + Application.ExecutablePath + "\"");
            }
            source1 = Application.StartupPath + "\\libs\\eng-tr.mdb";
            ingilizce_Kelime.Properties.Settings.Default.last_location = source1;
            ingilizce_Kelime.Properties.Settings.Default.Save();
            baglanti = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + source1);

            MessageBox.Show("Program Windows Açılırken Otomatik Olarak Açılacak", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.DeleteValue("My Program", false);
            }
            MessageBox.Show("Program Windows Açılırken Otomatik Olarak Açılma iptal Edildi", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public string ReplaceText(string _text)
        {
            _text = _text.Replace("&#231;", "ç").Replace("&#252;", "ü").Replace("&#246;", "ö").Replace("&#39;", "'").Replace("Ä±", "ı").Replace("ÄŸ", "ğ");
            return _text;
        }
        private void button6_Click(object sender, EventArgs e)
        {
            bool basarili = false;
            metroTextBox_turkce.Text = "";
            if (metroTextBox_ingilizce.Text.Length > 0)
            {
                Uri url = new Uri("https://tureng.com/tr/turkce-ingilizce/" + metroTextBox_ingilizce.Text);
                WebClient wc = new WebClientWithTimeout();
                wc.Encoding = System.Text.Encoding.UTF8;
                string temp = "", html = wc.DownloadString(url);
                HtmlAgilityPack.HtmlDocument dokuman = new HtmlAgilityPack.HtmlDocument();
                dokuman.LoadHtml(html);
                HtmlNodeCollection tr = dokuman.DocumentNode.SelectNodes("//td[@class='en tm']");
                HtmlNodeCollection en = dokuman.DocumentNode.SelectNodes("//td[@class='tr ts']");
                int sayac = 0;
                string temp_cesit = "";
                foreach (var baslik in tr)
                {
                    if (baslik.FirstChild.InnerText == metroTextBox_ingilizce.Text)
                    {
                        if (temp_cesit != baslik.ChildNodes["i"].InnerText)
                        {
                            temp += baslik.ChildNodes["i"].InnerText;
                            temp_cesit = baslik.ChildNodes["i"].InnerText;
                        }

                        int temp_int = 0;
                        foreach (var turkce in en)
                        {
                            if (temp_int == sayac)
                            {

                                temp += ReplaceText(turkce.FirstChild.InnerText) + " , ";
                                basarili = true;
                                break;
                            }
                            temp_int++;
                        }
                        sayac++;
                        if ((sayac >= 5) || (temp.Length > 50))
                            break;
                    }
                    else
                        break;
                }
                if (basarili)
                {
                    temp = temp.Substring(0, temp.Length - 2);
                    metroTextBox_turkce.Text = temp;
                }
                else
                {
                    MessageBox.Show("Kelime bulunamadı!");
                }
                /*if (sayac == 0)
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
                }*/

            }
        }


        private void tooltip_temizle()
        {
            ToolTip1.ToolTipTitle = "";
            ToolTip1.ToolTipIcon = ToolTipIcon.None;
        }


        private void button6_MouseHover(object sender, EventArgs e)
        {
            tooltip_temizle();
            ToolTip1.SetToolTip(this.button6, "ingilizce kelimenin türkçe anlamını bulur ve türkçe kısmına yazar");
        }

        private void button2_MouseHover(object sender, EventArgs e)
        {
            tooltip_temizle();
            ToolTip1.SetToolTip(this.button2, "Kelimeyi ezberlenen kelimeler kısmına alır ve tekrar göstermez");
        }

        private void button3_MouseEnter(object sender, EventArgs e)
        {
            tooltip_temizle();
            ToolTip1.SetToolTip(this.button3, "Kelimeyi yoksayılan kelimeler kısmına alır ve tekrar göstermez");

        }

        private void label2_MouseHover(object sender, EventArgs e)
        {
            tooltip_temizle();
            ToolTip1.SetToolTip(this.label2, "Siz ezberledim/istemiyorum seçeneğini seçene kadar rastgele seçilen " + Environment.NewLine + textBox_toplam_kelime_sayisi.Text + " adet kelime içerisinden bir kelime " + (Convert.ToDouble(textBox3.Text) * 60).ToString() + " saniyede bir rastgele gösterilir.");
        }

        private void textBox_toplam_kelime_sayisi_MouseHover(object sender, EventArgs e)
        {
            tooltip_temizle();
            ToolTip1.SetToolTip(this.textBox_toplam_kelime_sayisi, "Siz ezberledim/istemiyorum seçeneğini seçene kadar rastgele seçilen " + Environment.NewLine + textBox_toplam_kelime_sayisi.Text + " adet kelime içerisinden bir kelime " + (Convert.ToDouble(textBox3.Text) * 60).ToString() + " saniyede bir rastgele gösterilir.");
        }

        private void label3_MouseHover(object sender, EventArgs e)
        {
            tooltip_temizle();
            ToolTip1.SetToolTip(this.label3, "Siz ezberledim/istemiyorum seçeneğini seçene kadar rastgele seçilen " + Environment.NewLine + textBox_toplam_kelime_sayisi.Text + " adet kelime içerisinden bir kelime " + (Convert.ToDouble(textBox3.Text) * 60).ToString() + " saniyede bir rastgele gösterilir.");
        }

        private void textBox3_MouseHover(object sender, EventArgs e)
        {
            tooltip_temizle();
            ToolTip1.SetToolTip(this.textBox3, "Siz ezberledim/istemiyorum seçeneğini seçene kadar rastgele seçilen " + Environment.NewLine + textBox_toplam_kelime_sayisi.Text + " adet kelime içerisinden bir kelime " + (Convert.ToDouble(textBox3.Text) * 60).ToString() + " saniyede bir rastgele gösterilir.");
        }

        #region DENEME
        private void button7_Click(object sender, EventArgs e)
        {
            bool basarili = false;
            string[] ing_kelimeler = new string[5000];
            string[] ing_kelimeler2 = new string[5000];
            string[] tr_anlamlar = new string[5000];
            int kelime_sayisi = 0;
            //if (metroTextBox_ingilizce.Text.Length > 0)
            {
                Uri url = new Uri("https://www.usingenglish.com/reference/phrasal-verbs/list.html");
                WebClient wc = new WebClientWithTimeout();
                wc.Encoding = System.Text.Encoding.UTF8;
                string temp = "", html = wc.DownloadString(url);
                HtmlAgilityPack.HtmlDocument dokuman = new HtmlAgilityPack.HtmlDocument();
                dokuman.LoadHtml(html);
                HtmlNodeCollection tr2 = dokuman.DocumentNode.SelectNodes("//div[@class='wraplist']");
                int i = 0, k = 0, harf_sayisi = 0, pointer = 0;
                foreach (var baslik in tr2)
                {
                    ing_kelimeler[i] = baslik.FirstChild.InnerText;
                    ing_kelimeler2[i++] = baslik.ChildNodes[1].InnerText;
                    harf_sayisi++;
                }
                i = 0;
                for (k = 0; k < harf_sayisi; k++)
                {
                    while (ing_kelimeler2[k].Length > 0)
                    {
                        pointer = ing_kelimeler2[k].IndexOf("\n\n\n");
                        if (pointer != -1)
                        {
                            ing_kelimeler[i++] = ing_kelimeler2[k].Substring(0, pointer).Replace("\n", "").ToLower();
                            ing_kelimeler2[k] = ing_kelimeler2[k].Substring(pointer + 3, ing_kelimeler2[k].Length - pointer - 3);
                        }
                        else
                        {
                            pointer = ing_kelimeler2[k].IndexOf("\n\n \n");
                            if (pointer != -1)
                            {
                                ing_kelimeler[i++] = ing_kelimeler2[k].Substring(0, pointer).Replace("\n", "").ToLower();
                                ing_kelimeler2[k] = ing_kelimeler2[k].Substring(pointer + 3, ing_kelimeler2[k].Length - pointer - 3);
                            }
                            else
                                break;
                        }
                    }
                }
                kelime_sayisi = i;
                int bulunamayanlar = 0;
                i = 0;
                for (i = 0; i < kelime_sayisi; i++)
                {
                    url = new Uri("https://tureng.com/tr/turkce-ingilizce/" + ing_kelimeler[i].Replace(" ", "%20"));
                    wc = new WebClientWithTimeout();
                    wc.Encoding = System.Text.Encoding.UTF8;
                    temp = "";
                    html = wc.DownloadString(url);
                    dokuman = new HtmlAgilityPack.HtmlDocument();
                    dokuman.LoadHtml(html);
                    HtmlNodeCollection tr = dokuman.DocumentNode.SelectNodes("//td[@class='en tm']");
                    HtmlNodeCollection en = dokuman.DocumentNode.SelectNodes("//td[@class='tr ts']");
                    int sayac = 0;
                    string temp_cesit = "";
                    if (tr != null)
                    {
                        foreach (var baslik in tr)
                        {
                            if (baslik.FirstChild.InnerText == ing_kelimeler[i])
                            {
                                if (temp_cesit != baslik.ChildNodes["i"].InnerText)
                                {
                                    temp += baslik.ChildNodes["i"].InnerText;
                                    temp_cesit = baslik.ChildNodes["i"].InnerText;
                                }

                                int temp_int = 0;
                                foreach (var turkce in en)
                                {
                                    if ((temp_int == sayac) && (sayac != 0))
                                    {
                                        if (ReplaceText(turkce.FirstChild.InnerText).Length > 0)
                                        {
                                            temp += ReplaceText(turkce.FirstChild.InnerText) + " , ";
                                            basarili = true;
                                            break;
                                        }
                                    }
                                    temp_int++;
                                }
                                sayac++;
                                if ((sayac >= 5) || (temp.Length > 80))
                                    break;
                            }
                            else
                                break;
                        }
                    }
                    if ((basarili) && (temp.Length > 1))
                    {
                        temp = temp.Substring(0, temp.Length - 2);
                        tr_anlamlar[i] = temp;
                    }
                    else
                    {
                        bulunamayanlar++;
                        //MessageBox.Show("Kelime bulunamadı!");
                    }
                }
                if (bulunamayanlar > 0)
                    MessageBox.Show(bulunamayanlar + "Adet Kelime bulunamadı!");
                ;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            bool basarili = false;
            string[] ing_kelimeler = new string[5000];
            string[] ing_kelimeler2 = new string[5000];
            string[] tr_anlamlar = new string[5000];
            int kelime_sayisi = 0;
            //if (metroTextBox_ingilizce.Text.Length > 0)
            {
                Uri url = new Uri("https://www.skypeenglishclasses.com/english-phrasal-verbs/");
                WebClient wc = new WebClientWithTimeout();
                wc.Encoding = System.Text.Encoding.UTF8;
                string temp = "", html = wc.DownloadString(url);
                HtmlAgilityPack.HtmlDocument dokuman = new HtmlAgilityPack.HtmlDocument();
                dokuman.LoadHtml(html);
                HtmlNodeCollection tr2 = dokuman.DocumentNode.SelectNodes("//a[@class='idoms-table']");//dokuman.DocumentNode.SelectNodes("//a[@class='wraplist']");
                int i = 0, k = 0, harf_sayisi = 0, pointer = 0;
                foreach (var baslik in tr2)
                {
                    ing_kelimeler[i] = baslik.FirstChild.InnerText;
                    ing_kelimeler2[i++] = baslik.ChildNodes[1].InnerText;
                    harf_sayisi++;
                }
                i = 0;
                for (k = 0; k < harf_sayisi; k++)
                {
                    while (ing_kelimeler2[k].Length > 0)
                    {
                        pointer = ing_kelimeler2[k].IndexOf("\n\n\n");
                        if (pointer != -1)
                        {
                            ing_kelimeler[i++] = ing_kelimeler2[k].Substring(0, pointer).Replace("\n", "").ToLower();
                            ing_kelimeler2[k] = ing_kelimeler2[k].Substring(pointer + 3, ing_kelimeler2[k].Length - pointer - 3);
                        }
                        else
                        {
                            pointer = ing_kelimeler2[k].IndexOf("\n\n \n");
                            if (pointer != -1)
                            {
                                ing_kelimeler[i++] = ing_kelimeler2[k].Substring(0, pointer).Replace("\n", "").ToLower();
                                ing_kelimeler2[k] = ing_kelimeler2[k].Substring(pointer + 3, ing_kelimeler2[k].Length - pointer - 3);
                            }
                            else
                                break;
                        }
                    }
                }
                kelime_sayisi = i;
                int bulunamayanlar = 0;
                i = 0;
                for (i = 0; i < kelime_sayisi; i++)
                {
                    url = new Uri("https://tureng.com/tr/turkce-ingilizce/" + ing_kelimeler[i].Replace(" ", "%20"));
                    wc = new WebClientWithTimeout();
                    wc.Encoding = System.Text.Encoding.UTF8;
                    temp = "";
                    html = wc.DownloadString(url);
                    dokuman = new HtmlAgilityPack.HtmlDocument();
                    dokuman.LoadHtml(html);
                    HtmlNodeCollection tr = dokuman.DocumentNode.SelectNodes("//td[@class='en tm']");
                    HtmlNodeCollection en = dokuman.DocumentNode.SelectNodes("//td[@class='tr ts']");
                    int sayac = 0;
                    string temp_cesit = "";
                    if (tr != null)
                    {
                        foreach (var baslik in tr)
                        {
                            if (baslik.FirstChild.InnerText == ing_kelimeler[i])
                            {
                                if (temp_cesit != baslik.ChildNodes["i"].InnerText)
                                {
                                    temp += baslik.ChildNodes["i"].InnerText;
                                    temp_cesit = baslik.ChildNodes["i"].InnerText;
                                }

                                int temp_int = 0;
                                foreach (var turkce in en)
                                {
                                    if ((temp_int == sayac) && (sayac != 0))
                                    {
                                        if (ReplaceText(turkce.FirstChild.InnerText).Length > 0)
                                        {
                                            temp += ReplaceText(turkce.FirstChild.InnerText) + " , ";
                                            basarili = true;
                                            break;
                                        }
                                    }
                                    temp_int++;
                                }
                                sayac++;
                                if ((sayac >= 5) || (temp.Length > 80))
                                    break;
                            }
                            else
                                break;
                        }
                    }
                    if ((basarili) && (temp.Length > 1))
                    {
                        temp = temp.Substring(0, temp.Length - 2);
                        tr_anlamlar[i] = temp;
                    }
                    else
                    {
                        bulunamayanlar++;
                        //MessageBox.Show("Kelime bulunamadı!");
                    }
                }
                if (bulunamayanlar > 0)
                    MessageBox.Show(bulunamayanlar + "Adet Kelime bulunamadı!");
                ;
            }
        }

        #endregion


        private void button9_Click(object sender, EventArgs e)
        {
            string isim = "libs\\" + textBox_yeni_kutuphane_ismi.Text + ".mdb";

            string kaynak = @"libs\\empty.mdb";
            if (System.IO.File.Exists(kaynak))
            {
                if (System.IO.File.Exists(@isim))
                    File.Delete(@isim);
                File.Copy(kaynak, isim);
            }
            kutuphane_sirala();
        }

        private void metroComboBox_libs1_SelectedIndexChanged(object sender, EventArgs e)
        {
            source1 = sources[metroComboBox_libs1.SelectedIndex];
            baglanti = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + source1);
            ingilizce_Kelime.Properties.Settings.Default.last_location = source1;
            ingilizce_Kelime.Properties.Settings.Default.Save();
            Ezberlenen_Kelime_Sayisi();
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            ShowInTaskbar = true;
            timer3.Enabled = false;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;
            this.Visible = false;
        }
    }

    public class global
    {
        public static string word = "";
        public static bool yenile = true;
        public static bool kaydetme = false;
    }
    public sealed class KeyboardHook : IDisposable
    {
        // Registers a hot key with Windows.
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        // Unregisters the hot key with Windows.
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        /// <summary>
        /// Represents the window that is used internally to get the messages.
        /// </summary>
        private class Window : NativeWindow, IDisposable
        {
            private static int WM_HOTKEY = 0x0312;

            public Window()
            {
                // create the handle for the window.
                this.CreateHandle(new CreateParams());
            }

            /// <summary>
            /// Overridden to get the notifications.
            /// </summary>
            /// <param name="m"></param>
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                // check if we got a hot key pressed.
                if (m.Msg == WM_HOTKEY)
                {
                    // get the keys.
                    Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                    ModifierKeys modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

                    // invoke the event to notify the parent.
                    if (KeyPressed != null)
                        KeyPressed(this, new KeyPressedEventArgs(modifier, key));
                }
            }

            public event EventHandler<KeyPressedEventArgs> KeyPressed;

            #region IDisposable Members

            public void Dispose()
            {
                this.DestroyHandle();
            }

            #endregion
        }

        private Window _window = new Window();
        private int _currentId;

        public KeyboardHook()
        {
            // register the event of the inner native window.
            _window.KeyPressed += delegate (object sender, KeyPressedEventArgs args)
            {
                if (KeyPressed != null)
                    KeyPressed(this, args);
            };
        }

        /// <summary>
        /// Registers a hot key in the system.
        /// </summary>
        /// <param name="modifier">The modifiers that are associated with the hot key.</param>
        /// <param name="key">The key itself that is associated with the hot key.</param>
        public void RegisterHotKey(ModifierKeys modifier, Keys key)
        {
            // increment the counter.
            _currentId = _currentId + 1;

            // register the hot key.
            if (!RegisterHotKey(_window.Handle, _currentId, (uint)modifier, (uint)key))
                throw new InvalidOperationException("Couldn’t register the hot key.");
        }

        /// <summary>
        /// A hot key has been pressed.
        /// </summary>
        public event EventHandler<KeyPressedEventArgs> KeyPressed;

        #region IDisposable Members

        public void Dispose()
        {
            // unregister all the registered hot keys.
            for (int i = _currentId; i > 0; i--)
            {
                UnregisterHotKey(_window.Handle, i);
            }

            // dispose the inner native window.
            _window.Dispose();
        }

        #endregion
    }

    /// <summary>
    /// Event Args for the event that is fired after the hot key has been pressed.
    /// </summary>
    public class KeyPressedEventArgs : EventArgs
    {
        private ModifierKeys _modifier;
        private Keys _key;

        internal KeyPressedEventArgs(ModifierKeys modifier, Keys key)
        {
            _modifier = modifier;
            _key = key;
        }

        public ModifierKeys Modifier
        {
            get { return _modifier; }
        }

        public Keys Key
        {
            get { return _key; }
        }
    }

    /// <summary>
    /// The enumeration of possible modifiers.
    /// </summary>
    [Flags]
    public enum ModifierKeys : uint
    {
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }


    public class WebClientWithTimeout : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest wr = base.GetWebRequest(address);
            wr.Timeout = 5000; // timeout in milliseconds (ms)
            return wr;
        }
    }

}
