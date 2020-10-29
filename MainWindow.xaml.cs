﻿using SlackAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        private ObservableCollection<Message> messages = new ObservableCollection<Message>();

        public MainWindow()
        {
            InitializeComponent();
            token = ConfigurationManager.AppSettings["TOKEN"];
            channel = ConfigurationManager.AppSettings["CHANNEL"];

            this.CustomerListView.ItemsSource = messages;

            ManualResetEventSlim clientReady = new ManualResetEventSlim(false);
            SlackSocketClient client = new SlackSocketClient(token);
            client.Connect((connected) => {
                // This is called once the client has emitted the RTM start command
                clientReady.Set();
                channnel_id =  connected.channels.Where(x => x.name == channel).Select(x => x.id).FirstOrDefault();
            }, () => {
                // This is called once the RTM client has connected to the end point
            });
            client.OnMessageReceived += (message) =>
            {
                // Handle each message as you receive them
                if (message.channel == channnel_id)
                {
                    messages.Add(new Message { Text = message.text });
                }
            };
            clientReady.Wait();

        }

        class Message
        {
            public string Text { get; set; }
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
