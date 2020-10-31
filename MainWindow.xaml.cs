using SlackAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using slackoverflow.Models;
using System.Windows.Threading;
using System.ComponentModel;

namespace slackoverflow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string token;
        private string channel;
        private string channnel_id;
        private Dictionary<string,string> users;
        private DispatcherTimer timer=new DispatcherTimer();

        private ObservableCollection<SMessage> messages = new ObservableCollection<SMessage>();

        public MainWindow()
        {
            InitializeComponent();
            token = ConfigurationManager.AppSettings["TOKEN"];
            channel = ConfigurationManager.AppSettings["CHANNEL"];

            // 複数スレッドからコレクション操作できるようにする
            BindingOperations.EnableCollectionSynchronization(this.messages, new object());


            this.CustomerListView.ItemsSource = messages;

            ManualResetEventSlim clientReady = new ManualResetEventSlim(false);
            SlackSocketClient client = new SlackSocketClient(token);
            client.Connect((connected) => {
                // This is called once the client has emitted the RTM start command
                clientReady.Set();
                if (connected.ok)
                {
                    channnel_id = connected?.channels.Where(x => x?.name == channel).Select(x => x.id).FirstOrDefault();
                    users = connected.users.ToDictionary(x => x.id, x => x.name);

                    // インターバルを設定
                    timer.Interval = new TimeSpan(0, 0, 5);
                    timer.Tick += new EventHandler(MyTimerMethod);
                    timer.Start();
                    // 画面が閉じられるときに、タイマを停止
                    this.Closing += new CancelEventHandler(StopTimer);
                }
                else
                {
                    MessageBox.Show(connected.error);
                }

            }, () => {
                // This is called once the RTM client has connected to the end point
            });
            client.OnMessageReceived += (message) =>
            {
                // Handle each message as you receive them
                if (message.channel == channnel_id)
                {
                    var username = message.username;
                    if (message.user !=null && users.ContainsKey(message.user)) 
                    {
                        username = users[message.user];
                    }
                    messages.Add(new SMessage { Comment = message.text ,Name= "@" + username +" : " });
                }
            };
            clientReady.Wait();

        }

        // メッセージ加齢
        private void MyTimerMethod(object sender, EventArgs e)
        {
            foreach (var message in messages)
            {
                message.Life -= 0.2;
            }
            for (int i = messages.Count-1; i >=0; i--)
            {
                if(messages[i].Life <= 0)
                {
                    messages.Remove(messages[i]);
                }
            }
        }
        // タイマを停止
        private void StopTimer(object sender, CancelEventArgs e)
        {
            timer.Stop();
        }


        #region hidden

        protected const int GWL_EXSTYLE = (-20);
        protected const int WS_EX_TRANSPARENT = 0x00000020;

        [DllImport("user32")]
        protected static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32")]
        protected static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwLong);


        protected override void OnSourceInitialized(EventArgs e)
        {

            base.OnSourceInitialized(e);

            //WindowHandle(Win32) を取得
            var handle = new WindowInteropHelper(this).Handle;

            //クリックをスルー
            int extendStyle = GetWindowLong(handle, GWL_EXSTYLE);
            extendStyle |= WS_EX_TRANSPARENT; //フラグの追加
            SetWindowLong(handle, GWL_EXSTYLE, extendStyle);

        }
        #endregion

    }
}
