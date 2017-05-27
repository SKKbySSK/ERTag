namespace TagTest
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.lyr = new System.Windows.Forms.TextBox();
            this.art = new System.Windows.Forms.TextBox();
            this.alb = new System.Windows.Forms.TextBox();
            this.tit = new System.Windows.Forms.TextBox();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(337, 24);
            this.textBox1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(353, 28);
            this.textBox1.TabIndex = 0;
            this.textBox1.Text = "Test.bin";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(854, 21);
            this.button1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(138, 40);
            this.button1.TabIndex = 1;
            this.button1.Text = "Read";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(854, 72);
            this.button2.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(138, 40);
            this.button2.TabIndex = 2;
            this.button2.Text = "Write";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(854, 122);
            this.button3.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(138, 40);
            this.button3.TabIndex = 4;
            this.button3.Text = "Get";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(337, 126);
            this.textBox2.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(503, 28);
            this.textBox2.TabIndex = 3;
            this.textBox2.Text = "C:\\Users\\skkby\\Music\\Echosmith\\Talking Dreams";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(337, 164);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 21);
            this.label1.TabIndex = 5;
            this.label1.Text = "Time RWTag";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(22, 21);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(304, 290);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 6;
            this.pictureBox1.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(337, 201);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(106, 21);
            this.label2.TabIndex = 7;
            this.label2.Text = "Time MTag";
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(337, 75);
            this.textBox3.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(503, 28);
            this.textBox3.TabIndex = 8;
            this.textBox3.Text = "Test.bin";
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(730, 170);
            this.pictureBox2.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(262, 256);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 9;
            this.pictureBox2.TabStop = false;
            // 
            // lyr
            // 
            this.lyr.Location = new System.Drawing.Point(22, 453);
            this.lyr.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.lyr.Name = "lyr";
            this.lyr.Size = new System.Drawing.Size(301, 28);
            this.lyr.TabIndex = 10;
            this.lyr.Text = "Lyrics";
            // 
            // art
            // 
            this.art.Location = new System.Drawing.Point(22, 410);
            this.art.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.art.Name = "art";
            this.art.Size = new System.Drawing.Size(301, 28);
            this.art.TabIndex = 11;
            this.art.Text = "Artist";
            // 
            // alb
            // 
            this.alb.Location = new System.Drawing.Point(22, 366);
            this.alb.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.alb.Name = "alb";
            this.alb.Size = new System.Drawing.Size(301, 28);
            this.alb.TabIndex = 12;
            this.alb.Text = "Album";
            // 
            // tit
            // 
            this.tit.Location = new System.Drawing.Point(22, 322);
            this.tit.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tit.Name = "tit";
            this.tit.Size = new System.Drawing.Size(301, 28);
            this.tit.TabIndex = 13;
            this.tit.Text = "Title";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(438, 254);
            this.button4.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(138, 40);
            this.button4.TabIndex = 14;
            this.button4.Text = "Get";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(702, 21);
            this.button5.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(138, 40);
            this.button5.TabIndex = 15;
            this.button5.Text = "Open";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1014, 546);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.tit);
            this.Controls.Add(this.alb);
            this.Controls.Add(this.art);
            this.Controls.Add(this.lyr);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox1);
            this.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.TextBox lyr;
        private System.Windows.Forms.TextBox art;
        private System.Windows.Forms.TextBox alb;
        private System.Windows.Forms.TextBox tit;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
    }
}

