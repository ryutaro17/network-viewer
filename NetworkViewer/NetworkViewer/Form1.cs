using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetworkViewer
{
    public partial class Form1 : Form
    {

        private DataStore dataStore;

        private Visualize visualize;

        private Bitmap mapImage;

        private bool press = false;

        private int[] pressScreenCoord = new int[2]; // マウスダウンしたときのscreen座標
        private float[] pressMapCenterCoord = new float[2]; // マウスダウンしたときのworld座標のmapCenter座標

        public Form1()
        {
            InitializeComponent();

            this.BackColor = Color.Black;

            this.MouseWheel += new MouseEventHandler(Form1_MouseWheel);

            dataStore = new DataStore();
            visualize = VisualizeFactory.createVisualize(dataStore);
            visualize.screenWidth = this.Width;
            visualize.screenHeight = this.Height;

        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            press = false;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (press)
            {
                if(visualize.updateMapCenter(pressMapCenterCoord, pressScreenCoord, e))
                {
                    DisposeMapImage();
                    Invalidate();
                }
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            pressScreenCoord[0] = e.Location.X;
            pressScreenCoord[1] = e.Location.Y;

            pressMapCenterCoord = visualize.getMapCenter();

            press = true;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                Graphics g = e.Graphics;

                Pen pen_red_1 = new Pen(Color.Red, 1);
                {
                    if (mapImage == null)
                    {
                        mapImage = visualize.createBitmap();
                    }
                    g.DrawImage(mapImage, new Point(0, 0));
                }

                // 中心の十字
                g.DrawLine(pen_red_1, this.Width / 2 - 5, this.Height / 2, this.Width / 2 + 5, this.Height / 2);
                g.DrawLine(pen_red_1, this.Width / 2, this.Height / 2 - 5, this.Width / 2, this.Height / 2 + 5);

            }
            catch (OutOfMemoryException ex)
            {
                long currentSet = Environment.WorkingSet;
                Console.WriteLine("現在のメモリ使用量は{0}byteです。", currentSet.ToString("N0"));

                Console.WriteLine("例外：{0}", ex.ToString());
            }
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine("例外：{0}", ex.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("例外：{0}", ex.ToString());
            }


        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            DisposeMapImage();
            visualize.screenWidth = this.Width;
            visualize.screenHeight = this.Height;
            Invalidate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DoubleBuffered = true;
        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (visualize.updateZoom(e))
            {
                DisposeMapImage();
                Invalidate();
            }
        }

        private void DisposeMapImage()
        {
            if (mapImage != null)
            {
                mapImage.Dispose();
                mapImage = null;
            }
        }
    }
}
