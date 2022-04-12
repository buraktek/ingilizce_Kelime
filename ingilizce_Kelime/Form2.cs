using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ingilizce_Kelime
{
    public partial class Form2 : Form
    {
        bool mouse_click = false;
        Point ilkkonum;
        int old_x, old_y;
        public Form2()
        {
            InitializeComponent();
            pictureBox1.ImageLocation = Form1.global.url;
            this.Location = new System.Drawing.Point(ingilizce_Kelime.Properties.Settings.Default.son_konum_x, ingilizce_Kelime.Properties.Settings.Default.son_konum_y);
            Form1.global.show = true;
            timer1.Enabled = true;
        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void Button2_MouseDown(object sender, MouseEventArgs e)
        {
            mouse_click = true;
            pictureBox1.Cursor = Cursors.SizeAll; //SizeAll yapmamımızın amacı taşırken hoş görüntü vermek için
            ilkkonum = e.Location; //İlk konuma gördüğünüz gibi değerimizi atıyoruz.
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            Form1.global.show = false;
            timer1.Enabled = false;
            this.Close();
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouse_click)
            {
                //Point x,y;
                int x, y;
                int eski_x, eski_y;
                x = e.X; /*+ button2.Left*/// - (ilkkonum.X);
                // button.left ile soldan uzaklığını ayarlıyoruz. Yani e.X dediğimizde buton üzerinde mouseun hareket ettiği pixeli alacağız + butonun soldan uzaklığını ekliyoruz son olarakta ilk mouseın tıklandığı alanı çıkarıyoruz yoksa butonun en solunda olur mouse imleci. Alttakide aynı şekilde Y koordinati için geçerli
                y = e.Y; /*+ button2.Top*/// - (ilkkonum.Y);
                //Location.X(11);// = 10;
                if ((old_x != x) || (old_y != y))
                {

                    if ((Math.Abs(old_x - x) < 50) || (Math.Abs(old_y - y) < 50))
                    {
                        eski_x = Location.X;
                        eski_y = Location.Y;
                        this.Location = new System.Drawing.Point(eski_x + x, eski_y + y); //e.X + button2.Left - (ilkkonum.X);
                        old_x = x;
                        old_y = y;
                        ingilizce_Kelime.Properties.Settings.Default.son_konum_x = Location.X;
                        ingilizce_Kelime.Properties.Settings.Default.son_konum_y = Location.Y;
                        ingilizce_Kelime.Properties.Settings.Default.Save();
                    }
                }
            }
            try
            {
                Opacity = 1;
            }
            catch
            {

            }
        }
    }
}
