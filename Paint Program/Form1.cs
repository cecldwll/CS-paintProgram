using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Paint_Program
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        
            // set the form size
            this.Width = 950;
            this.Height = 700;
            bm = new Bitmap(pic.Width, pic.Height); // create a bitmap that matches the size of the picturebox
            g = Graphics.FromImage(bm); // create graphics object to draw on the bitmap
            g.Clear(Color.White); // clear the drawing area with white color
            pic.Image = bm; // assign the bitmap to the picturebox's image

        }

        // declare variable names
        Bitmap bm;
        Graphics g;
        bool paint = false;
        Point px, py;
        Pen p = new Pen(Color.Black, 1); // pen for drawing
        Pen erase = new Pen(Color.White, 10); // pen for erasing
        int index; // to keep track of selected tool
        int x, y, sX, sY, cX, cY; // coordinates for drawing shapes

        // create variable names for color dialogbox & new color
        ColorDialog cd = new ColorDialog();
        Color new_color;

        private void pic_MouseDown(object sender, MouseEventArgs e)
        {
            paint = true;
            py = e.Location;

            cX = e.X; // store starting coordinates for drawing shapes
            cY = e.Y;
        }

        private void pic_MouseMove(object sender, MouseEventArgs e)
        {
            if (paint) // if painting is true, draw based on the selected tool
            {
                if (index == 1) // pencil tool for free-hand drawing
                {
                    px = e.Location;
                    g.DrawLine(p, px, py);
                    py = px;
                }
                if (index == 2) // eraser tool
                {
                    px = e.Location;
                    g.DrawLine(erase, px, py);
                    py = px;
                }
            }
            pic.Refresh(); // refresh the picturebox to show the drawing

            x = e.X; // update coordinates as mouse moves
            y = e.Y;
            sX = e.X - cX; // width of shape
            sY = e.Y - cY; // height of shape
        }

        private void pic_MouseUp(object sender, MouseEventArgs e)
        {
            paint = false; // stop painting when mouse is up

            sX = x - cX;
            sY = y - cY;

            if (index == 3) // draw ellipse when mouse is released
            {
                g.DrawEllipse(p, cX, cY, sX, sY);
            }

            if (index == 4) // draw rectangle when mouse is released
            {
                g.DrawRectangle(p, cX, cY, sX, sY);
            }

            if (index == 5) // draw line when mouse is released
            {
                g.DrawLine(p, cX, cY, x, y); 
            }
        }

        // buttons to switch between different tools
        private void btn_pencil_Click(object sender, EventArgs e)
        {
            index = 1; // pencil tool
        }

        private void btn_eraser_Click(object sender, EventArgs e)
        {
            index = 2; // eraser tool
        }

        private void btn_ellipse_Click(object sender, EventArgs e)
        {
            index = 3; // ellipse tool
        }

        private void btn_rect_Click(object sender, EventArgs e)
        {
            index = 4; // rectangle tool
        }

        private void btn_line_Click(object sender, EventArgs e)
        {
            index = 5; // line tool
        }

        private void pic_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics; 

            if (paint) // draw shapes in real-time as user drags the mouse
            {
                if (index == 3) 
                {
                    g.DrawEllipse(p, cX, cY, sX, sY);
                }

                if (index == 4)
                {
                    g.DrawRectangle(p, cX, cY, sX, sY);
                }

                if (index == 5) 
                {
                    g.DrawLine(p, cX, cY, x, y);
                }
            }
        }

        private void btn_clear_Click(object sender, EventArgs e)
        { // clear the drawing area
            g.Clear(Color.White);
            pic.Image = bm;
            index = 0;
        }

        private void btn_color_Click(object sender, EventArgs e)
        { // open color dialog to pick a new color for the pen
            cd.ShowDialog();
            new_color = cd.Color;
            pic_color.BackColor = cd.Color;
            p.Color = cd.Color;
        }

        static Point set_point(PictureBox pb, Point pt)
        { // helper function to calculate points in relation to the picturebox
            float pX = 1f * pb.Image.Width / pb.Width;
            float pY = 1f * pb.Image.Height / pb.Height;
            return new Point((int)(pt.X * pX), (int)(pt.Y * pY));
        }

        private void color_picker_MouseClick(object sender, MouseEventArgs e)
        { // set the color from the color picker image
            Point point = set_point(color_picker, e.Location);
            pic_color.BackColor = ((Bitmap)color_picker.Image).GetPixel(point.X, point.Y);
            new_color = pic_color.BackColor;
            p.Color = pic_color.BackColor; // set pen color to selected color
        }

        private void validate(Bitmap bm, Stack<Point>sp, int x, int y, Color old_color, Color new_color)
        { // validate the pixel color before filling it with new color
            Color cx = bm.GetPixel(x, y);
            if (cx==old_color)
            {
                sp.Push(new Point(x, y));
                bm.SetPixel(x, y, new_color); // fill the pixel with new color
            }
        }

        public void Fill(Bitmap bm, int x, int y, Color new_clr)
        { // flood fill function to fill shapes with color
            Color old_color = bm.GetPixel(x, y);
            Stack<Point> pixel = new Stack<Point>();
            pixel.Push(new Point(x, y));
            bm.SetPixel(x, y, new_clr);
            if (old_color == new_clr) return; // stop if the color is already the same

            while(pixel.Count > 0)
            { // fill surrounding pixels until all are colored
                Point pt = (Point)pixel.Pop();
                if(pt.X > 0 && pt.Y > 0 && pt.X < bm.Width - 1 && pt.Y < bm.Height - 1)
                {
                    validate(bm, pixel, pt.X - 1, pt.Y, old_color, new_clr);
                    validate(bm, pixel, pt.X, pt.Y - 1, old_color, new_clr);
                    validate(bm, pixel, pt.X + 1, pt.Y, old_color, new_clr);
                    validate(bm, pixel, pt.X, pt.Y + 1, old_color, new_clr);
                }
            }

        }

        private void pic_MouseClick(object sender, MouseEventArgs e)
        { // handle mouse click to trigger fill tool
            if (index == 7)
            {
                Point point = set_point(pic,e.Location);
                Fill(bm, point.X, point.Y, new_color); // fill the area with selected color
            }
        }

        private void btn_fill_Click(object sender, EventArgs e)
        { // select fill tool
            index = 7;
        }

        private void btn_save_Click(object sender, EventArgs e)
        { // save the drawing as an image file
            var sfd = new SaveFileDialog();
            sfd.Filter = "Image(*.jpg) |*.jpg| (*.*|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Bitmap btm = bm.Clone(new Rectangle(0, 0, pic.Width, pic.Height), bm.PixelFormat);
                btm.Save(sfd.FileName, ImageFormat.Jpeg); // save the image as jpeg
                MessageBox.Show("Image Save Sucessfully!");
            }

        }
    }
}
