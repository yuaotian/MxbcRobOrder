namespace MxbcRobOrderWinFormsApp
{
    partial class FormMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            buttonTaskStart = new Button();
            buttonTaskEnd = new Button();
            textBoxToken = new TextBox();
            label1 = new Label();
            buttonGetToken = new Button();
            listViewRobTime = new ListView();
            textBoxSecretWord = new TextBox();
            label4 = new Label();
            label2 = new Label();
            listViewCoupon = new ListView();
            labelCountDown = new Label();
            timerServiceRefresh = new System.Windows.Forms.Timer(components);
            timerRobOrderMxbc = new System.Windows.Forms.Timer(components);
            textBoxDelayRobOrder = new TextBox();
            label5 = new Label();
            label6 = new Label();
            textBoxProxyInfo = new TextBox();
            buttonQueryCoupon = new Button();
            checkBoxIsProxy = new CheckBox();
            textBoxOrderCount = new TextBox();
            label8 = new Label();
            label9 = new Label();
            labelServerTime = new Label();
            label10 = new Label();
            textBoxServiceRefresh = new TextBox();
            labelLogInfo = new Label();
            timerCountDown = new System.Windows.Forms.Timer(components);
            SuspendLayout();
            // 
            // buttonTaskStart
            // 
            buttonTaskStart.Location = new Point(12, 416);
            buttonTaskStart.Name = "buttonTaskStart";
            buttonTaskStart.Size = new Size(104, 38);
            buttonTaskStart.TabIndex = 6;
            buttonTaskStart.Text = "开始";
            buttonTaskStart.UseVisualStyleBackColor = true;
            buttonTaskStart.Click += ButtonTaskStart_Click;
            // 
            // buttonTaskEnd
            // 
            buttonTaskEnd.Location = new Point(12, 460);
            buttonTaskEnd.Name = "buttonTaskEnd";
            buttonTaskEnd.Size = new Size(104, 38);
            buttonTaskEnd.TabIndex = 7;
            buttonTaskEnd.Text = "结束";
            buttonTaskEnd.UseVisualStyleBackColor = true;
            buttonTaskEnd.Click += buttonTaskEnd_Click;
            // 
            // textBoxToken
            // 
            textBoxToken.Location = new Point(16, 27);
            textBoxToken.Multiline = true;
            textBoxToken.Name = "textBoxToken";
            textBoxToken.ScrollBars = ScrollBars.Vertical;
            textBoxToken.Size = new Size(190, 38);
            textBoxToken.TabIndex = 8;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(13, 8);
            label1.Name = "label1";
            label1.Size = new Size(56, 17);
            label1.TabIndex = 9;
            label1.Text = "Token：";
            // 
            // buttonGetToken
            // 
            buttonGetToken.Location = new Point(210, 28);
            buttonGetToken.Name = "buttonGetToken";
            buttonGetToken.Size = new Size(104, 38);
            buttonGetToken.TabIndex = 10;
            buttonGetToken.Text = "获取本地Token";
            buttonGetToken.UseVisualStyleBackColor = true;
            buttonGetToken.Click += buttonGetToken_Click;
            // 
            // listViewRobTime
            // 
            listViewRobTime.Location = new Point(12, 90);
            listViewRobTime.Name = "listViewRobTime";
            listViewRobTime.Size = new Size(325, 285);
            listViewRobTime.TabIndex = 11;
            listViewRobTime.UseCompatibleStateImageBehavior = false;
            listViewRobTime.View = View.Details;
            // 
            // textBoxSecretWord
            // 
            textBoxSecretWord.Location = new Point(345, 44);
            textBoxSecretWord.Name = "textBoxSecretWord";
            textBoxSecretWord.Size = new Size(188, 23);
            textBoxSecretWord.TabIndex = 13;
            textBoxSecretWord.Text = "茉莉奶绿 白月光";
            textBoxSecretWord.TextAlign = HorizontalAlignment.Center;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(345, 24);
            label4.Name = "label4";
            label4.Size = new Size(68, 17);
            label4.TabIndex = 14;
            label4.Text = "本场口令：";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(476, 70);
            label2.Name = "label2";
            label2.Size = new Size(56, 17);
            label2.TabIndex = 4;
            label2.Text = "优惠劵：";
            // 
            // listViewCoupon
            // 
            listViewCoupon.Location = new Point(345, 90);
            listViewCoupon.Name = "listViewCoupon";
            listViewCoupon.Size = new Size(188, 285);
            listViewCoupon.TabIndex = 3;
            listViewCoupon.UseCompatibleStateImageBehavior = false;
            listViewCoupon.View = View.Details;
            // 
            // labelCountDown
            // 
            labelCountDown.AutoSize = true;
            labelCountDown.Location = new Point(16, 69);
            labelCountDown.Name = "labelCountDown";
            labelCountDown.Size = new Size(242, 17);
            labelCountDown.TabIndex = 15;
            labelCountDown.Text = "距离下一轮抢单还有 Xx 小时 Xx 分钟 Xx 秒";
            // 
            // textBoxDelayRobOrder
            // 
            textBoxDelayRobOrder.Location = new Point(187, 434);
            textBoxDelayRobOrder.Name = "textBoxDelayRobOrder";
            textBoxDelayRobOrder.Size = new Size(60, 23);
            textBoxDelayRobOrder.TabIndex = 16;
            textBoxDelayRobOrder.Text = "1500";
            textBoxDelayRobOrder.TextAlign = HorizontalAlignment.Center;
            textBoxDelayRobOrder.KeyPress += textBoxDelayRobOrder_KeyPress;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(122, 440);
            label5.Name = "label5";
            label5.Size = new Size(59, 17);
            label5.TabIndex = 17;
            label5.Text = "秒抢延迟:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(13, 571);
            label6.Name = "label6";
            label6.Size = new Size(502, 17);
            label6.TabIndex = 18;
            label6.Text = " 【重点声明】仅用于测试和学习参考\\研究之用禁止用于非法攻击,请在下载后24小时内删除。";
            // 
            // textBoxProxyInfo
            // 
            textBoxProxyInfo.Location = new Point(320, 434);
            textBoxProxyInfo.Multiline = true;
            textBoxProxyInfo.Name = "textBoxProxyInfo";
            textBoxProxyInfo.ScrollBars = ScrollBars.Vertical;
            textBoxProxyInfo.Size = new Size(218, 98);
            textBoxProxyInfo.TabIndex = 19;
            textBoxProxyInfo.Text = "127.0.0.1:7890";
            // 
            // buttonQueryCoupon
            // 
            buttonQueryCoupon.Location = new Point(12, 502);
            buttonQueryCoupon.Name = "buttonQueryCoupon";
            buttonQueryCoupon.Size = new Size(104, 38);
            buttonQueryCoupon.TabIndex = 20;
            buttonQueryCoupon.Text = "查卷";
            buttonQueryCoupon.UseVisualStyleBackColor = true;
            buttonQueryCoupon.Click += buttonQueryCoupon_Click;
            // 
            // checkBoxIsProxy
            // 
            checkBoxIsProxy.AutoSize = true;
            checkBoxIsProxy.Location = new Point(322, 411);
            checkBoxIsProxy.Name = "checkBoxIsProxy";
            checkBoxIsProxy.Size = new Size(75, 21);
            checkBoxIsProxy.TabIndex = 22;
            checkBoxIsProxy.Text = "启用代理";
            checkBoxIsProxy.UseVisualStyleBackColor = true;
            // 
            // textBoxOrderCount
            // 
            textBoxOrderCount.Location = new Point(187, 463);
            textBoxOrderCount.Name = "textBoxOrderCount";
            textBoxOrderCount.Size = new Size(60, 23);
            textBoxOrderCount.TabIndex = 23;
            textBoxOrderCount.Text = "15";
            textBoxOrderCount.TextAlign = HorizontalAlignment.Center;
            textBoxOrderCount.KeyPress += TextBoxOrderCount_KeyPress;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(122, 466);
            label8.Name = "label8";
            label8.Size = new Size(59, 17);
            label8.TabIndex = 24;
            label8.Text = "秒抢次数:";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Microsoft YaHei UI", 7F);
            label9.Location = new Point(320, 535);
            label9.Name = "label9";
            label9.Size = new Size(218, 16);
            label9.TabIndex = 25;
            label9.Text = "PS：一行一个，随机取，请保持低延迟的国内IP";
            // 
            // labelServerTime
            // 
            labelServerTime.AutoSize = true;
            labelServerTime.Location = new Point(201, 7);
            labelServerTime.Name = "labelServerTime";
            labelServerTime.Size = new Size(80, 17);
            labelServerTime.TabIndex = 26;
            labelServerTime.Text = "服务器时间：";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(122, 496);
            label10.Name = "label10";
            label10.Size = new Size(59, 17);
            label10.TabIndex = 28;
            label10.Text = "刷新间隔:";
            // 
            // textBoxServiceRefresh
            // 
            textBoxServiceRefresh.Location = new Point(187, 493);
            textBoxServiceRefresh.Name = "textBoxServiceRefresh";
            textBoxServiceRefresh.Size = new Size(60, 23);
            textBoxServiceRefresh.TabIndex = 27;
            textBoxServiceRefresh.Text = "5000";
            textBoxServiceRefresh.TextAlign = HorizontalAlignment.Center;
            textBoxServiceRefresh.KeyPress += textBoxSerivceRefresh_KeyPress;
            // 
            // labelLogInfo
            // 
            labelLogInfo.AutoSize = true;
            labelLogInfo.Location = new Point(13, 380);
            labelLogInfo.Name = "labelLogInfo";
            labelLogInfo.Size = new Size(188, 17);
            labelLogInfo.TabIndex = 29;
            labelLogInfo.Text = "听说煎饼果子卷鲨鱼辣椒最配啦。";
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(547, 603);
            Controls.Add(labelLogInfo);
            Controls.Add(label10);
            Controls.Add(textBoxServiceRefresh);
            Controls.Add(labelServerTime);
            Controls.Add(label9);
            Controls.Add(label8);
            Controls.Add(textBoxOrderCount);
            Controls.Add(checkBoxIsProxy);
            Controls.Add(buttonQueryCoupon);
            Controls.Add(textBoxProxyInfo);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(textBoxDelayRobOrder);
            Controls.Add(labelCountDown);
            Controls.Add(label4);
            Controls.Add(textBoxSecretWord);
            Controls.Add(listViewRobTime);
            Controls.Add(buttonGetToken);
            Controls.Add(label1);
            Controls.Add(textBoxToken);
            Controls.Add(buttonTaskEnd);
            Controls.Add(buttonTaskStart);
            Controls.Add(label2);
            Controls.Add(listViewCoupon);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FormMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "蜜穴宾撑 V2.1  ";
            Load += Form_Main_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button buttonTaskStart;
        private Button buttonTaskEnd;
        private TextBox textBoxToken;
        private Label label1;
        private Button buttonGetToken;
        private ListView listViewRobTime;
        private TextBox textBoxSecretWord;
        private Label label4;
        private Label label2;
        private ListView listViewCoupon;
        private Label labelCountDown;
        private System.Windows.Forms.Timer timerServiceRefresh;
        private System.Windows.Forms.Timer timerRobOrderMxbc;
        private TextBox textBoxDelayRobOrder;
        private Label label5;
        private Label label6;
        private TextBox textBoxProxyInfo;
        private Button buttonQueryCoupon;
        private CheckBox checkBoxIsProxy;
        private TextBox textBoxOrderCount;
        private Label label8;
        private Label label9;
        private Label labelServerTime;
        private Label label10;
        private TextBox textBoxServiceRefresh;
        private Label labelLogInfo;
        private System.Windows.Forms.Timer timerCountDown;
    }
}
