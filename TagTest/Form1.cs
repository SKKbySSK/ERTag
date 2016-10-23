﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace TagTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RWTag.TagReader Reader = new RWTag.TagReader();
            RWTag.Tag tag = Reader.GetTag(new FileStream(textBox1.Text, FileMode.OpenOrCreate), Path.GetExtension(textBox1.Text));
            if (tag.Image != null) pictureBox1.Image = (Image)new ImageConverter().ConvertFrom(tag.Image);
            Reader.Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            RWTag.MP3.ID3v2.Tag tagW = new RWTag.MP3.ID3v2.Tag(new RWTag.SettableStream(new FileStream(textBox3.Text, FileMode.OpenOrCreate)));
            RWTag.Tag tag = new RWTag.Tag();
            tag.Title = "Test";
            tag.Album = "Album";
            tag.Comment = "Nothing";
            tag.Date = DateTime.Now;
            tag.DiscNumber = 1;
            tag.TotalDiscNumber = 1;
            tag.Track = 40;
            tag.TotalTrack = 50;
            tag.Genre = "カントリー";
            tag.Artist = "Kaisei Sunaga";
            tag.ImageMIMEType = RWTag.ImageMIME.jpeg;
            tag.Image = GetBytes(pictureBox2.Image);
            tag.ImageDescription = "Cover";
            tagW.Write(tag);
            tagW.Dispose();
        }

        private byte[] GetBytes(Image Image)
        {
            byte[] ImageBytes;

            // メモリストリームの生成
            using (MemoryStream ms = new MemoryStream())
            {
                // Image画像を、bmp形式でストリームに保存
                Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                // ストリームのデーターをバイト型配列に変換
                ImageBytes = ms.ToArray();

                // ストリームのクローズ
                ms.Close();
            }

            return ImageBytes;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string[] files = Directory.GetFiles(textBox2.Text, "*.mp3");

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            foreach (string f in files)
            {
                RWTag.TagReader Reader = new RWTag.TagReader();
                RWTag.Tag tag = Reader.GetTag(new FileStream(f, FileMode.OpenOrCreate), Path.GetExtension(f));
                Reader.Dispose();
            }
            sw.Stop();
            label1.Text = sw.ElapsedMilliseconds.ToString();

            sw.Reset();
            sw.Start();
            foreach (string f in files)
            {
                LAPP.MTag.Tag tag = LAPP.MTag.TagReader.GetTag(f);
            }
            sw.Stop();
            label2.Text = sw.ElapsedMilliseconds.ToString();
        }
    }
}