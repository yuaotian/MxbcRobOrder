using System.Diagnostics;
using Masuit.Tools;

namespace MxbcRobOrderWinFormsApp
{
    public partial class FormMain : Form
    {
        //�����б�ͷ 11:00~20:00 ����
        private readonly List<string> _robTimeColumnData = [];

        private static int _robOrderCount = 0;

        // ���ʱ�� ���� ��һ�η�����ʱ��͸��¿���
        private static int _intervalTime = 5;

        private static DateTime _serverTime = DateTime.Now;

        private static bool _isRunning = true;
        private static string? _recordTime;

        /// <summary>
        ///  ������
        /// </summary>
        public FormMain()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  ���ڼ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form_Main_Load(object sender, EventArgs e)
        {
            //���ڽ�ֹ���
            this.MaximizeBox = false;
            //���ڽ�ֹ�϶�
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            //��ʼ�����
            listViewRobTime.Columns.Add("tag", "��������");
            listViewRobTime.Columns.Add("robTime", "����ʱ��");
            listViewRobTime.Columns.Add("status", "����״̬");
            listViewRobTime.View = View.Details; // ������ͼΪ��ϸ��Ϣ��ͼ
            listViewRobTime.GridLines = true; // ��ʾ������
            listViewRobTime.FullRowSelect = true; // ����ѡ������
            // ����ListViewΪ�Զ������
            listViewRobTime.OwnerDraw = true;
            // ����¼�������
            listViewRobTime.DrawItem += ListViewRobTime_DrawItem;
            listViewRobTime.DrawSubItem += ListViewRobTime_DrawSubItem;
            listViewRobTime.DrawColumnHeader += ListViewRobTime_DrawColumnHeader;
            //�����п�
            listViewRobTime.Columns[2].Width = 220;
            //��ȡ����
            ReadConfig();

            var now = RefreshServeInfo();
            var nextHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddHours(1); // ������һ������
            var countDown = nextHour - now; // ���㵱ǰʱ������һ������Ĳ�
            // ���±�ǩ��ʾ����ʱ
            labelCountDown.Text = $@"������һ���������� {countDown.Hours} Сʱ {countDown.Minutes} ���� {countDown.Seconds} ��";

            //������ʱ��
            timerRobOrderMxbc.Interval = Convert.ToInt32(textBoxDelayRobOrder.Text);
            timerRobOrderMxbc.Tick += TimerRobOrderMxbc_Tick;

            //mxbc����infoˢ�¼��
            timerServiceRefresh.Interval = Convert.ToInt32(textBoxServiceRefresh.Text);
            Console.WriteLine(@"mxbc����infoˢ�¼��: " + timerServiceRefresh.Interval);
            timerServiceRefresh.Tick += TimerServiceRefresh_Tick;
            timerServiceRefresh.Start();

            //�����ʱ
            timerCountDown.Interval = 1000;
            Console.WriteLine(@"�����ʱ: " + timerCountDown.Interval);
            timerCountDown.Tick += TimerCountDown_Tick;
            timerCountDown.Start();
        }

        private void TimerServiceRefresh_Tick(object? sender, EventArgs e)
        {
            Task.Run(RefreshServeInfo);
        }

        /// <summary>
        /// ˢ�·�����ʱ��͸��¿���
        /// </summary>
        /// <returns></returns>
        private DateTime RefreshServeInfo()
        {
            //�������
            var now = DateTime.Now;
            var proxy = GetProxyHostPort();
            var secretWordInfoDto = MxbcService.GetSecretWordInfo(proxy);
            if (secretWordInfoDto == null)
            {
                //MessageBox.Show(@"��ȡ�ӿ���Ϣ�쳣������IP��Ban�ˣ����ϴ�����������ݡ�ˢ�¼�������³���", @"����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LabelLogInfoShow("��ȡ��Ϣ�ӿ��쳣������IP��Ban�ˣ�����ϴ��������ô�����������ݡ�ˢ�¼�������³��ԣ�");
                return now;
            }

            //�������
            listViewRobTime.Items.Clear();
            textBoxSecretWord.Text = secretWordInfoDto.data.hintWord.Contains("��������")
                ? secretWordInfoDto.data.hintWord["�������".Length..]
                : secretWordInfoDto.data.hintWord;

            //ˢ������
            now = secretWordInfoDto.data.serverTime;
            _serverTime = now;
            labelServerTime.Text = $@"������ʱ�䣺{now:yyyy-MM-dd HH:mm:ss}";
            secretWordInfoDto.data.roundList.ForEach(round =>
            {
                _robTimeColumnData.Add(round.startTime);
                var todayDate = round.localDateTime;
                var item = new ListViewItem(round.date);
                item.SubItems.Add(round.startTime);
                if (todayDate > now)
                {
                    item.SubItems.Add(@"���꣬ʱ�����磬Ī��");
                }
                else
                {
                    // ������ʱ���ڳ���һСʱ��Ϊ������
                    item.SubItems.Add(now <= todayDate.AddHours(1) ? "���ڽ��У�������" : "�Ѿ������ϣ��ȴ���һ�ְ�");
                }

                item.UseItemStyleForSubItems = false; // �����������Լ�����ʽ
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

            //��Ϊ��
            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(secretWord))
            {
                var round = GetHourTime(); //�ж��Ƿ�������
                var robOrderCount = Convert.ToInt32(textBoxOrderCount.Text);
                if (_isRunning && _robTimeColumnData.Contains(round) && _robOrderCount <= robOrderCount)
                {
                    var proxyHostPort = GetProxyHostPort();
                    // ʹ�� await �첽���÷���
                    var result = MxbcService.mxbcRobOrder(token, secretWord, round, proxyHostPort);
                    _robOrderCount++;
                    //���뵽��Ӧʱ��������
                    UpdateSubItemsStatus(round, result);
                    if (result.Contains("������") || result.Contains("�ӿ��쳣") || result.Contains("IP���"))
                    {
                        //���ü���
                        _robOrderCount = robOrderCount;
                        //ȡ�����к�ִ�У�ֱ����һ��
                        _isRunning = false;
                        _recordTime = GetHourTime();
                        //�����������
                        UpdateComponentsStatus(true);
                        //��ʾ�������
                        //MessageBox.Show(result);
                        LabelLogInfoShow($"{_recordTime} ����������{_robOrderCount}��,��ֹͣ,�ȴ���һ��,Msg:{result}");
                    }
                    else
                    {
                        LabelLogInfoShow($"{_recordTime} ����������{_robOrderCount}��,��ֹͣ,�ȴ���һ��,Msg:{result}");
                    }

                    LabelLogInfoShow($@"��ʼ���� �ӳ�:{textBoxDelayRobOrder.Text}/ms, ����������{_robOrderCount}��");
                }

                if (_robOrderCount >= 1 && _robOrderCount >= robOrderCount)
                {
                    _robOrderCount = 0;
                    // ʹ�� Task.Delay ��� Thread.Sleep ʵ���첽�ӳ�
                    await Task.Delay(Convert.ToInt32(textBoxDelayRobOrder.Text));
                }

                //��ʾ��ʼ����
                Console.WriteLine($@"��ʼ���� �ӳ�:{textBoxDelayRobOrder.Text}/ms");
                LabelLogInfoShow($@"��ʼ���� �ӳ�:{textBoxDelayRobOrder.Text}/ms, ����������{_robOrderCount}��");
            }
            else
            {
                //MessageBox.Show(@"����дToken�Ϳ���");
                labelCountDown.Text = @"����дToken�Ϳ������ᶨʱˢ�²��õ��ģ�";
                UpdateComponentsStatus(true);
            }
        }

        private string? GetProxyHostPort()
        {
            string? proxyHostPort = null;
            if (!checkBoxIsProxy.Checked) return proxyHostPort;
            //��ȡ���� һ��һ��
            var proxyInfoPoolText = textBoxProxyInfo.Text;
            if (string.IsNullOrEmpty(proxyInfoPoolText)) return proxyHostPort;
            var proxyInfoPool = proxyInfoPoolText.Split("\n");
            //���һ��
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
            // �첽ִ�����񣬵����ȴ���ɣ���ֹ����UI�̣߳�
            Task.Run(HandleCountDownAsync);
        }

        private void HandleCountDownAsync()
        {
            var now = DateTime.Now;
            DateTime? nextRobTime = null;
            // ����ʱ���Ѱ����һ������ʱ��
            foreach (var time in _robTimeColumnData)
            {
                var potentialNextTime = DateTime.Parse($"{DateTime.Now:yyyy-MM-dd} {time}");
                // ���ʱ���Ѿ���ȥ�������һ��
                if (potentialNextTime < now)
                {
                    potentialNextTime = potentialNextTime.AddDays(1);
                }

                // ���ʱ������������Ϳ�ʼ����
                if (potentialNextTime == now)
                {
                    UpdateComponentsStatus(false);
                }

                // ѡȡ�����δ��ʱ���
                if (nextRobTime == null || potentialNextTime < nextRobTime)
                {
                    nextRobTime = potentialNextTime;
                }
            }

            // ���㵹��ʱ
            if (!nextRobTime.HasValue)
            {
                labelCountDown.Text = @"���ڻ�û�е�����ʱ���أ�����Ī�����Ű�";

                return;
            }

            var countDown = nextRobTime.Value - now;
            // ���±�ǩ��ʾ����ʱ
            labelCountDown.Text = $@"������һ���������� {countDown.Hours} Сʱ {countDown.Minutes} ���� {countDown.Seconds} ��";
            Console.WriteLine(
                $@"{countDown.TotalSeconds}/�� ʣ��ʱ�䣺{countDown.Hours}Сʱ {countDown.Minutes}���� {countDown.Seconds}�룬����ʱ����ʱ�䣺{nextRobTime.Value.ToString("yyyy-MM-dd HH:mm")}");
            //������ʾ
            if (countDown.Seconds % 5 == 0)
            {
                LabelLogInfoShow(@"���ֶ���ȡToken���ұ��ֲ�Ϊ�գ�����ᶨʱˢ�µģ����������5��ͻῪʼ�����ˣ�");
            }

            if (countDown.Seconds % 10 == 0)
            {
                LabelLogInfoShow(@"��ϲ�����̲����ٱ�\�������ѣ�");
            }

            if (countDown.Seconds % 15 == 0)
            {
                LabelLogInfoShow(@"��ϲ���Լ�����Ӱ�������");
            }

            if (countDown.Seconds % 20 == 0)
            {
                LabelLogInfoShow(@"���갢��������...�ȿȣ���Ȼ�Ǻ������ļ���������~");
            }

            //�������ˢ�·�����ʱ��Ϳ���
            if (countDown.TotalSeconds % 5 == 0)
            {
                Task.Run(RefreshServeInfo);
            }
        }

        // ʵ��DrawColumnHeader�¼�
        private static void ListViewRobTime_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true; // ʹ��Ĭ�ϵ���ͷ����
        }

        // ʵ��DrawItem�¼�
        private static void ListViewRobTime_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            // ������ɺ�ָʾ��ʹ��Ĭ�ϻ�������������ı�
            e.DrawDefault = true;
        }

        // ʵ��DrawSubItem�¼�
        private static void ListViewRobTime_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(e.SubItem.BackColor), e.Bounds); // ��䱳��

            // ���ò�ͬ״̬����ɫ
            var textColor = e.SubItem.Text switch
            {
                "���꣬ʱ�����磬Ī��" => Color.Gray,
                "���ڽ��У�������" => Color.Coral,
                "�Ѿ������ϣ��ȴ���һ�ְ�" => Color.Red,
                "���ܴ��Ҳ�������ˣ���ȥ����!!!" => Color.Green,
                //�����ɫ
                _ => Color.FromArgb(new Random().Next(0, 255), new Random().Next(0, 255), new Random().Next(0, 255))
            };

            // �����ı�
            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.Item.Font, e.Bounds, textColor, TextFormatFlags.Left);
        }

        private void ButtonTaskStart_Click(object sender, EventArgs e)
        {
            if (checkBoxIsProxy.Checked)
            {
                if (string.IsNullOrEmpty(textBoxProxyInfo.Text))
                {
                    //MessageBox.Show(@"����д������Ϣ��һ��һ����ʾ����127.0.0.1:8080����ע��ֻ֧��HTTP����");
                    LabelLogInfoShow("����д������Ϣ��һ��һ����ʾ����127.0.0.1:8080����ע��ֻ֧��HTTP����");
                    return;
                }
            }

            var token = textBoxToken.Text;
            var secretWord = textBoxSecretWord.Text;
            //��Ϊ��
            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(secretWord))
            {
                //��ʼ����ʱ��
                UpdateComponentsStatus(false);
                //��ʼ���� ȡ��ǰʱ��
                var round = GetHourTime();
                //���round�������ֱ�ӿ���
                if (_robTimeColumnData.Contains(round))
                {
                    var proxyHostPort = GetProxyHostPort();
                    buttonTaskStart.Text = @"ʱ��ոպã�����������...";
                    var result = MxbcService.mxbcRobOrder(token, secretWord, round, proxyHostPort);
                    UpdateSubItemsStatus(round, result);
                }

                buttonTaskStart.Text = @"�Ȱ���...";
                Console.WriteLine(@"��ʼ���� �ӳ�:{0}/ms", textBoxDelayRobOrder.Text);
                LabelLogInfoShow($@"��ʼ���� �ӳ�:{textBoxDelayRobOrder.Text}/ms");
                SaveConfig();
            }
            else
            {
                MessageBox.Show(@"����дToken�Ϳ���");
                //labelCountDown.Text = @"����дToken�Ϳ���";
                //��ʼ����ʱ��
                UpdateComponentsStatus(true);
            }
        }

        private void SaveConfig()
        {
            //���浽�����ļ�
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
            //�����ɫ
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
            //�����true����ͣ
            if (status)
            {
                buttonTaskStart.Text = "��ʼ����";
                //��ͣ��ʱ��
                timerRobOrderMxbc.Interval = 999999999;
                timerRobOrderMxbc.Stop();
            }
            else
            {
                IsRunRob();
                //˵���ǵ�һ�Σ���ô��ֱ��ִ��
                if (_recordTime == null && _isRunning)
                {
                    buttonTaskStart.Text = @"�Ȱ���...";
                    // �򿪶�ʱ��
                    timerRobOrderMxbc.Interval = Convert.ToInt32(textBoxDelayRobOrder.Text);
                    timerRobOrderMxbc.Start();
                }
            }

            SaveConfig();
        }

        private void IsRunRob()
        {
            if (string.IsNullOrEmpty(_recordTime)) return;
            // ���Խ� _recordTime ����Ϊ DateTime
            if (DateTime.TryParse(_recordTime, out var parsedRecordTime))
            {
                // ��ȡ��ǰʱ��
                var now = DateTime.Now;
                // ���õ�ǰʱ�����һ������
                var nextHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddHours(1);
                // ��� parsedRecordTime �Ƿ���ڵ�ǰʱ�����һ������
                if (parsedRecordTime >= nextHour)
                {
                    _isRunning = true;
                    buttonTaskStart.Text = @"�Ȱ���...";
                    // �򿪶�ʱ��
                    timerRobOrderMxbc.Interval = Convert.ToInt32(textBoxDelayRobOrder.Text);
                    timerRobOrderMxbc.Start();
                }
                else
                {
                    // ���������������������Ҫ���� buttonTaskStart ���ı�����֪�û������ܿ�ʼ
                    LabelLogInfoShow($"������ʱ������,��Ϊ��ǰ��{_recordTime}��ʱ������[δ����|Ip���|�쳣]��ȴ�����һ��ʱ��㣺{nextHour}");
                }
            }
            else
            {
                LabelLogInfoShow("����ʱ��ʧ��"); // �������ʧ�ܣ�������Ҫ�������
            }
        }

        private async void buttonGetToken_Click(object sender, EventArgs e)
        {
            buttonGetToken.Enabled = false; // ���ð�ť��ֹ�ظ����
            buttonGetToken.Text = @"���ڴ���..."; // ��ʾ�û����ڴ���
            try
            {
                const string processName = "WeChatAppEx";
                const string searchKeyword = "&accessToken=";
                // ��ȡ����ƥ��������Ľ���
                var processes = Process.GetProcessesByName(processName);
                //������ڿ�
                if (processes.Length == 0)
                {
                    MessageBox.Show(@"δ�ҵ� WeChatAppEx ���̣���������С���򣬽���ҳ��", @"�������", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                // ִ���첽����
                var result = await Task.Run(() =>
                {
                    var tmpStr = ProcessMemorySearch.SearchProcessMemory(processName, searchKeyword, 200);
                    return tmpStr?[13..];
                });

                if (result == null)
                {
                    MessageBox.Show(@"δ�ҵ� accessToken ��������С���򣬽���ҳ��", @"�������", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    textBoxToken.Text = result;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"��������: {ex.Message}", @"����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LabelLogInfoShow($@"��������: {ex.Message}");
            }
            finally
            {
                buttonGetToken.Enabled = true; // �ָ���ť״̬
                if (!string.IsNullOrEmpty(textBoxToken.Text))
                {
                    LabelLogInfoShow($@"��ȡToken�ɹ�!!!");
                }

                buttonGetToken.Text = @"��ȡToken"; // �ָ���ť�ı�
                SaveConfig();
            }
        }

        private void buttonQueryCoupon_Click(object sender, EventArgs e)
        {
            MessageBox.Show($@"��������: �ӿ��쳣�����Ժ����Ի��Լ�ȥ��ɡ�", @"����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            LabelLogInfoShow($@"��������: �ӿ��쳣�����Ժ����Ի��Լ�ȥС����App��ɡ�");
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
        ///  ��ֹ���������
        /// </summary>
        /// <param name="e"></param>
        private static void DigitalInput(KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                // ��ֹ���������
                case '\b':
                    return;
                //�������������˸��
                case < '0':
                //������������0-9����
                case > '9':
                    e.Handled = true;
                    break;
            }
        }
    }
}