using System.Diagnostics;
using Masuit.Tools;

namespace MxbcRobOrderWinFormsApp
{
    public partial class FormMain : Form
    {
        //定义列表头 11:00~20:00 整点
        private readonly List<string> _robTimeColumnData = [];

        private static int _robOrderCount = 0;

        // 间隔时间 五秒 查一次服务器时间和更新口令
        private static int _intervalTime = 5;

        private static DateTime _serverTime = DateTime.Now;

        private static bool _isRunning = true;
        private static string? _recordTime;

        /// <summary>
        ///  主窗口
        /// </summary>
        public FormMain()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  窗口加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form_Main_Load(object sender, EventArgs e)
        {
            //窗口禁止最大化
            this.MaximizeBox = false;
            //窗口禁止拖动
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            //初始化表格
            listViewRobTime.Columns.Add("tag", "抢单日期");
            listViewRobTime.Columns.Add("robTime", "抢单时间");
            listViewRobTime.Columns.Add("status", "抢单状态");
            listViewRobTime.View = View.Details; // 设置视图为详细信息视图
            listViewRobTime.GridLines = true; // 显示网格线
            listViewRobTime.FullRowSelect = true; // 允许选择整行
            // 设置ListView为自定义绘制
            listViewRobTime.OwnerDraw = true;
            // 添加事件处理函数
            listViewRobTime.DrawItem += ListViewRobTime_DrawItem;
            listViewRobTime.DrawSubItem += ListViewRobTime_DrawSubItem;
            listViewRobTime.DrawColumnHeader += ListViewRobTime_DrawColumnHeader;
            //设置列宽
            listViewRobTime.Columns[2].Width = 220;
            //读取配置
            ReadConfig();

            var now = RefreshServeInfo();
            var nextHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddHours(1); // 计算下一个整点
            var countDown = nextHour - now; // 计算当前时间与下一个整点的差
            // 更新标签显示倒计时
            labelCountDown.Text = $@"距离下一轮抢单还有 {countDown.Hours} 小时 {countDown.Minutes} 分钟 {countDown.Seconds} 秒";

            //抢单定时器
            timerRobOrderMxbc.Interval = Convert.ToInt32(textBoxDelayRobOrder.Text);
            timerRobOrderMxbc.Tick += TimerRobOrderMxbc_Tick;

            //mxbc服务info刷新间隔
            timerServiceRefresh.Interval = Convert.ToInt32(textBoxServiceRefresh.Text);
            Console.WriteLine(@"mxbc服务info刷新间隔: " + timerServiceRefresh.Interval);
            timerServiceRefresh.Tick += TimerServiceRefresh_Tick;
            timerServiceRefresh.Start();

            //间隔定时
            timerCountDown.Interval = 1000;
            Console.WriteLine(@"间隔定时: " + timerCountDown.Interval);
            timerCountDown.Tick += TimerCountDown_Tick;
            timerCountDown.Start();
        }

        private void TimerServiceRefresh_Tick(object? sender, EventArgs e)
        {
            Task.Run(RefreshServeInfo);
        }

        /// <summary>
        /// 刷新服务器时间和更新口令
        /// </summary>
        /// <returns></returns>
        private DateTime RefreshServeInfo()
        {
            //添加数据
            var now = DateTime.Now;
            var proxy = GetProxyHostPort();
            var secretWordInfoDto = MxbcService.GetSecretWordInfo(proxy);
            if (secretWordInfoDto == null)
            {
                //MessageBox.Show(@"获取接口信息异常，可能IP被Ban了，加上代理后软件会根据【刷新间隔】重新尝试", @"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LabelLogInfoShow("获取信息接口异常，可能IP被Ban了，请加上代理并且启用代理后，软件会根据【刷新间隔】重新尝试！");
                return now;
            }

            //清空数据
            listViewRobTime.Items.Clear();
            textBoxSecretWord.Text = secretWordInfoDto.data.hintWord.Contains("本场口令")
                ? secretWordInfoDto.data.hintWord["本场口令：".Length..]
                : secretWordInfoDto.data.hintWord;

            //刷新数据
            now = secretWordInfoDto.data.serverTime;
            _serverTime = now;
            labelServerTime.Text = $@"服务器时间：{now:yyyy-MM-dd HH:mm:ss}";
            secretWordInfoDto.data.roundList.ForEach(round =>
            {
                _robTimeColumnData.Add(round.startTime);
                var todayDate = round.localDateTime;
                var item = new ListViewItem(round.date);
                item.SubItems.Add(round.startTime);
                if (todayDate > now)
                {
                    item.SubItems.Add(@"少年，时间尚早，莫急");
                }
                else
                {
                    // 在整点时间内持续一小时视为进行中
                    item.SubItems.Add(now <= todayDate.AddHours(1) ? "正在进行，速抢！" : "已经结束嘞，等待下一轮吧");
                }

                item.UseItemStyleForSubItems = false; // 允许子项有自己的样式
                item.SubItems[0].BackColor = Color.White;
                item.SubItems[1].BackColor = Color.White;
                listViewRobTime.Items.Add(item);
            });

            return now;
        }

        private async void TimerRobOrderMxbc_Tick(object? sender, EventArgs e)
        {
            var token = textBoxToken.Text;
            var secretWord = textBoxSecretWord.Text;

            //不为空
            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(secretWord))
            {
                var round = GetHourTime(); //判断是否是整点
                var robOrderCount = Convert.ToInt32(textBoxOrderCount.Text);
                if (_isRunning && _robTimeColumnData.Contains(round) && _robOrderCount <= robOrderCount)
                {
                    var proxyHostPort = GetProxyHostPort();
                    // 使用 await 异步调用方法
                    var result = MxbcService.mxbcRobOrder(token, secretWord, round, proxyHostPort);
                    _robOrderCount++;
                    //插入到对应时间表格里面
                    UpdateSubItemsStatus(round, result);
                    if (result.Contains("已抢完") || result.Contains("接口异常") || result.Contains("IP风控"))
                    {
                        //重置计数
                        _robOrderCount = robOrderCount;
                        //取消运行和执行，直到下一次
                        _isRunning = false;
                        _recordTime = GetHourTime();
                        //更新组件结束
                        UpdateComponentsStatus(true);
                        //提示抢单完成
                        //MessageBox.Show(result);
                        LabelLogInfoShow($"{_recordTime} 本轮抢单：{_robOrderCount}次,已停止,等待下一轮,Msg:{result}");
                    }
                    else
                    {
                        LabelLogInfoShow($"{_recordTime} 本轮抢单：{_robOrderCount}次,已停止,等待下一轮,Msg:{result}");
                    }

                    LabelLogInfoShow($@"开始抢单 延迟:{textBoxDelayRobOrder.Text}/ms, 本轮抢单：{_robOrderCount}次");
                }

                if (_robOrderCount >= 1 && _robOrderCount >= robOrderCount)
                {
                    _robOrderCount = 0;
                    // 使用 Task.Delay 替代 Thread.Sleep 实现异步延迟
                    await Task.Delay(Convert.ToInt32(textBoxDelayRobOrder.Text));
                }

                //提示开始抢单
                Console.WriteLine($@"开始抢单 延迟:{textBoxDelayRobOrder.Text}/ms");
                LabelLogInfoShow($@"开始抢单 延迟:{textBoxDelayRobOrder.Text}/ms, 本轮抢单：{_robOrderCount}次");
            }
            else
            {
                //MessageBox.Show(@"请填写Token和口令");
                labelCountDown.Text = @"请填写Token和口令！口令会定时刷新不用担心！";
                UpdateComponentsStatus(true);
            }
        }

        private string? GetProxyHostPort()
        {
            string? proxyHostPort = null;
            if (!checkBoxIsProxy.Checked) return proxyHostPort;
            //获取代理 一行一个
            var proxyInfoPoolText = textBoxProxyInfo.Text;
            if (string.IsNullOrEmpty(proxyInfoPoolText)) return proxyHostPort;
            var proxyInfoPool = proxyInfoPoolText.Split("\n");
            //随机一个
            var strictNext = new Random().StrictNext(proxyInfoPool.Length - 1);
            proxyHostPort = proxyInfoPool[strictNext];
            return proxyHostPort;
        }

        private static string GetHourTime()
        {
            return DateTime.Now.ToString("HH:mm");
        }

        private void UpdateSubItemsStatus(string round, string? result)
        {
            foreach (ListViewItem item in listViewRobTime.Items)
            {
                if (!item.Text.Equals(round)) continue;
                item.SubItems[2].Text = result;
                return;
            }
        }

        private void TimerCountDown_Tick(object? sender, EventArgs e)
        {
            // 异步执行任务，但不等待完成（防止阻塞UI线程）
            Task.Run(HandleCountDownAsync);
        }

        private void HandleCountDownAsync()
        {
            var now = DateTime.Now;
            DateTime? nextRobTime = null;
            // 遍历时间段寻找下一个抢单时间
            foreach (var time in _robTimeColumnData)
            {
                var potentialNextTime = DateTime.Parse($"{DateTime.Now:yyyy-MM-dd} {time}");
                // 如果时间已经过去，则添加一天
                if (potentialNextTime < now)
                {
                    potentialNextTime = potentialNextTime.AddDays(1);
                }

                // 如果时间正好是整点就开始任务
                if (potentialNextTime == now)
                {
                    UpdateComponentsStatus(false);
                }

                // 选取最近的未来时间点
                if (nextRobTime == null || potentialNextTime < nextRobTime)
                {
                    nextRobTime = potentialNextTime;
                }
            }

            // 计算倒计时
            if (!nextRobTime.HasValue)
            {
                labelCountDown.Text = @"现在还没有到抢单时间呢，少年莫急挂着吧";

                return;
            }

            var countDown = nextRobTime.Value - now;
            // 更新标签显示倒计时
            labelCountDown.Text = $@"距离下一轮抢单还有 {countDown.Hours} 小时 {countDown.Minutes} 分钟 {countDown.Seconds} 秒";
            Console.WriteLine(
                $@"{countDown.TotalSeconds}/秒 剩余时间：{countDown.Hours}小时 {countDown.Minutes}分钟 {countDown.Seconds}秒，倒计时结束时间：{nextRobTime.Value.ToString("yyyy-MM-dd HH:mm")}");
            //五秒提示
            if (countDown.Seconds % 5 == 0)
            {
                LabelLogInfoShow(@"请手动获取Token，且保持不为空，口令会定时刷新的，在整点最后5秒就会开始抢单了！");
            }

            if (countDown.Seconds % 10 == 0)
            {
                LabelLogInfoShow(@"你喜欢喝奶茶吗？少冰\五分糖最佳！");
            }

            if (countDown.Seconds % 15 == 0)
            {
                LabelLogInfoShow(@"好喜欢吃煎饼果子啊！！！");
            }

            if (countDown.Seconds % 20 == 0)
            {
                LabelLogInfoShow(@"少年阿宾和蜜桃...咳咳，当然是和蜜桃四季春更搭啦~");
            }

            //最后五秒刷新服务器时间和口令
            if (countDown.TotalSeconds % 5 == 0)
            {
                Task.Run(RefreshServeInfo);
            }
        }

        // 实现DrawColumnHeader事件
        private static void ListViewRobTime_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true; // 使用默认的列头绘制
        }

        // 实现DrawItem事件
        private static void ListViewRobTime_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            // 绘制完成后，指示不使用默认绘制来绘制项的文本
            e.DrawDefault = true;
        }

        // 实现DrawSubItem事件
        private static void ListViewRobTime_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(e.SubItem.BackColor), e.Bounds); // 填充背景

            // 设置不同状态的颜色
            var textColor = e.SubItem.Text switch
            {
                "少年，时间尚早，莫急" => Color.Gray,
                "正在进行，速抢！" => Color.Coral,
                "已经结束嘞，等待下一轮吧" => Color.Red,
                "可能大概也许抢到了，快去看看!!!" => Color.Green,
                //随机颜色
                _ => Color.FromArgb(new Random().Next(0, 255), new Random().Next(0, 255), new Random().Next(0, 255))
            };

            // 绘制文本
            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.Item.Font, e.Bounds, textColor, TextFormatFlags.Left);
        }

        private void ButtonTaskStart_Click(object sender, EventArgs e)
        {
            if (checkBoxIsProxy.Checked)
            {
                if (string.IsNullOrEmpty(textBoxProxyInfo.Text))
                {
                    //MessageBox.Show(@"请填写代理信息，一行一个，示例：127.0.0.1:8080，请注意只支持HTTP代理");
                    LabelLogInfoShow("请填写代理信息，一行一个，示例：127.0.0.1:8080，请注意只支持HTTP代理");
                    return;
                }
            }

            var token = textBoxToken.Text;
            var secretWord = textBoxSecretWord.Text;
            //不为空
            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(secretWord))
            {
                //开始任务定时器
                UpdateComponentsStatus(false);
                //开始抢单 取当前时间
                var round = GetHourTime();
                //如果round是整点就直接开抢
                if (_robTimeColumnData.Contains(round))
                {
                    var proxyHostPort = GetProxyHostPort();
                    buttonTaskStart.Text = @"时间刚刚好，正在抢单中...";
                    var result = MxbcService.mxbcRobOrder(token, secretWord, round, proxyHostPort);
                    UpdateSubItemsStatus(round, result);
                }

                buttonTaskStart.Text = @"等啊等...";
                Console.WriteLine(@"开始抢单 延迟:{0}/ms", textBoxDelayRobOrder.Text);
                LabelLogInfoShow($@"开始抢单 延迟:{textBoxDelayRobOrder.Text}/ms");
                SaveConfig();
            }
            else
            {
                MessageBox.Show(@"请填写Token和口令");
                //labelCountDown.Text = @"请填写Token和口令";
                //开始任务定时器
                UpdateComponentsStatus(true);
            }
        }

        private void SaveConfig()
        {
            //保存到配置文件
            //textBoxDelayRobOrder = "1500",
            //textBoxOrderCount = "15",
            //textBoxSerivceRefresh = "5000",
            //checkBoxIsProxy = false,
            //textBoxProxyInfo = "127.0.0.1:8080",
            ConfigurationHelper.SetAppSetting("textBoxDelayRobOrder", textBoxDelayRobOrder.Text);
            ConfigurationHelper.SetAppSetting("textBoxOrderCount", textBoxOrderCount.Text);
            ConfigurationHelper.SetAppSetting("textBoxServiceRefresh", textBoxServiceRefresh.Text);
            ConfigurationHelper.SetAppSetting("checkBoxIsProxy", checkBoxIsProxy.Checked.ToString());
            ConfigurationHelper.SetAppSetting("textBoxProxyInfo", textBoxProxyInfo.Text);
        }

        private void ReadConfig()
        {
            textBoxDelayRobOrder.Text = ConfigurationHelper.GetAppSetting("textBoxDelayRobOrder") ?? "1500";
            textBoxOrderCount.Text = ConfigurationHelper.GetAppSetting("textBoxOrderCount") ?? "15";
            textBoxServiceRefresh.Text = ConfigurationHelper.GetAppSetting("textBoxServiceRefresh") ?? "5000";
            checkBoxIsProxy.Checked =
                bool.Parse(string.IsNullOrEmpty(ConfigurationHelper.GetAppSetting("checkBoxIsProxy"))
                    ? "false"
                    : ConfigurationHelper.GetAppSetting("checkBoxIsProxy"));
            textBoxProxyInfo.Text = ConfigurationHelper.GetAppSetting("textBoxProxyInfo") ?? "127.0.0.1:7890";
        }

        private void LabelLogInfoShow(string info)
        {
            labelLogInfo.Text = info;
            //随机颜色
            labelLogInfo.ForeColor = Color.FromArgb(new Random().Next(0, 255), new Random().Next(0, 255),
                new Random().Next(0, 255));
        }

        private void buttonTaskEnd_Click(object sender, EventArgs e)
        {
            UpdateComponentsStatus(true);
        }

        private void UpdateComponentsStatus(bool status)
        {
            buttonTaskStart.Enabled = status;
            textBoxDelayRobOrder.Enabled = status;
            textBoxOrderCount.Enabled = status;
            textBoxProxyInfo.Enabled = status;
            textBoxDelayRobOrder.Enabled = status;
            textBoxServiceRefresh.Enabled = status;
            textBoxToken.Enabled = status;
            textBoxSecretWord.Enabled = status;
            checkBoxIsProxy.Enabled = status;
            //如果是true就暂停
            if (status)
            {
                buttonTaskStart.Text = "开始抢单";
                //暂停定时器
                timerRobOrderMxbc.Interval = 999999999;
                timerRobOrderMxbc.Stop();
            }
            else
            {
                IsRunRob();
                //说明是第一次，那么就直接执行
                if (_recordTime == null && _isRunning)
                {
                    buttonTaskStart.Text = @"等啊等...";
                    // 打开定时器
                    timerRobOrderMxbc.Interval = Convert.ToInt32(textBoxDelayRobOrder.Text);
                    timerRobOrderMxbc.Start();
                }
            }

            SaveConfig();
        }

        private void IsRunRob()
        {
            if (string.IsNullOrEmpty(_recordTime)) return;
            // 尝试将 _recordTime 解析为 DateTime
            if (DateTime.TryParse(_recordTime, out var parsedRecordTime))
            {
                // 获取当前时间
                var now = DateTime.Now;
                // 设置当前时间的下一个整点
                var nextHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddHours(1);
                // 检查 parsedRecordTime 是否大于当前时间的下一个整点
                if (parsedRecordTime >= nextHour)
                {
                    _isRunning = true;
                    buttonTaskStart.Text = @"等啊等...";
                    // 打开定时器
                    timerRobOrderMxbc.Interval = Convert.ToInt32(textBoxDelayRobOrder.Text);
                    timerRobOrderMxbc.Start();
                }
                else
                {
                    // 如果不满足条件，可能需要设置 buttonTaskStart 的文本来告知用户还不能开始
                    LabelLogInfoShow($"不满足时间条件,因为当前【{_recordTime}】时间点可能[未抢到|Ip风控|异常]需等待到下一个时间点：{nextHour}");
                }
            }
            else
            {
                LabelLogInfoShow("解析时间失败"); // 如果解析失败，可能需要处理错误
            }
        }

        private async void buttonGetToken_Click(object sender, EventArgs e)
        {
            buttonGetToken.Enabled = false; // 禁用按钮防止重复点击
            buttonGetToken.Text = @"正在处理..."; // 提示用户正在处理
            try
            {
                const string processName = "WeChatAppEx";
                const string searchKeyword = "&accessToken=";
                // 获取所有匹配进程名的进程
                var processes = Process.GetProcessesByName(processName);
                //如果等于空
                if (processes.Length == 0)
                {
                    MessageBox.Show(@"未找到 WeChatAppEx 进程，请先启动小程序，进入活动页面", @"搜索结果", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                // 执行异步操作
                var result = await Task.Run(() =>
                {
                    var tmpStr = ProcessMemorySearch.SearchProcessMemory(processName, searchKeyword, 200);
                    return tmpStr?[13..];
                });

                if (result == null)
                {
                    MessageBox.Show(@"未找到 accessToken 请先启动小程序，进入活动页面", @"搜索结果", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    textBoxToken.Text = result;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"发生错误: {ex.Message}", @"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LabelLogInfoShow($@"发生错误: {ex.Message}");
            }
            finally
            {
                buttonGetToken.Enabled = true; // 恢复按钮状态
                if (!string.IsNullOrEmpty(textBoxToken.Text))
                {
                    LabelLogInfoShow($@"获取Token成功!!!");
                }

                buttonGetToken.Text = @"获取Token"; // 恢复按钮文本
                SaveConfig();
            }
        }

        private void buttonQueryCoupon_Click(object sender, EventArgs e)
        {
            MessageBox.Show($@"发生错误: 接口异常，请稍后再试或自己去查吧。", @"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            LabelLogInfoShow($@"发生错误: 接口异常，请稍后再试或自己去小程序、App查吧。");
        }

        private void TextBoxOrderCount_KeyPress(object sender, KeyPressEventArgs e)
        {
            DigitalInput(e);
        }

        private void textBoxDelayRobOrder_KeyPress(object sender, KeyPressEventArgs e)
        {
            DigitalInput(e);
        }

        private void textBoxSerivceRefresh_KeyPress(object sender, KeyPressEventArgs e)
        {
            DigitalInput(e);
        }

        /// <summary>
        ///  禁止输入非数字
        /// </summary>
        /// <param name="e"></param>
        private static void DigitalInput(KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                // 禁止输入非数字
                case '\b':
                    return;
                //这是允许输入退格键
                case < '0':
                //这是允许输入0-9数字
                case > '9':
                    e.Handled = true;
                    break;
            }
        }
    }
}