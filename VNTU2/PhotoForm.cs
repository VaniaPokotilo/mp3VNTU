using System;
using System.Drawing;
using System.Windows.Forms;

namespace VNTU2
{
    public partial class PhotoForm : Form
    {
        public PhotoForm()
        {
            InitializeComponent();
        }
        
        public PhotoForm(Image imageToShow)
        {
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.Image = imageToShow;
            this.ControlBox = false;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}