﻿using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Platforms.Wpf.Views;
using ProxySuper.Core.Models.Hosts;
using ProxySuper.Core.Services;
using ProxySuper.Core.ViewModels;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProxySuper.WPF.Views
{
    /// <summary>
    /// TrojanGoInstallerView.xaml 的交互逻辑
    /// </summary>
    [MvxWindowPresentation(Identifier = nameof(TrojanGoInstallerView), Modal = true)]
    public partial class TrojanGoInstallerView : MvxWindow
    {
        public TrojanGoInstallerView()
        {
            InitializeComponent();
        }

        public new TrojanGoInstallerViewModel ViewModel
        {
            get
            {
                return (TrojanGoInstallerViewModel)base.ViewModel;
            }
        }

        public TrojanGoProject Project { get; set; }

        private SshClient _sshClient;
        private void OpenConnect()
        {

            WriteOutput("正在登陆服务器 ...");
            var conneInfo = CreateConnectionInfo(ViewModel.Host);
            _sshClient = new SshClient(conneInfo);
            try
            {
                _sshClient.Connect();
            }
            catch (Exception ex)
            {
                WriteOutput("登陆失败！");
                WriteOutput(ex.Message);
                return;
            }
            WriteOutput("登陆服务器成功！");

            ViewModel.Connected = true;
            Project = new TrojanGoProject(_sshClient, ViewModel.Settings, WriteOutput);
        }

        private void WriteOutput(string outShell)
        {
            if (!outShell.EndsWith("\n"))
            {
                outShell += "\n";
            }

            Dispatcher.Invoke(() =>
            {
                OutputTextBox.AppendText(outShell);
                OutputTextBox.ScrollToEnd();
            });
        }

        private ConnectionInfo CreateConnectionInfo(Host host)
        {
            AuthenticationMethod auth = null;

            if (host.SecretType == LoginSecretType.Password)
            {
                auth = new PasswordAuthenticationMethod(host.UserName, host.Password);
            }
            else if (host.SecretType == LoginSecretType.PrivateKey)
            {
                auth = new PrivateKeyAuthenticationMethod(host.UserName, new PrivateKeyFile(host.PrivateKeyPath));
            }

            if (host.Proxy.Type == LocalProxyType.None)
            {
                return new ConnectionInfo(host.Address, host.Port, host.UserName, auth);
            }
            else
            {
                return new ConnectionInfo(
                    host: host.Address,
                    port: host.Port,
                    username: host.UserName,
                    proxyType: (ProxyTypes)(int)host.Proxy.Type,
                    proxyHost: host.Proxy.Address,
                    proxyPort: host.Proxy.Port,
                    proxyUsername: host.Proxy.UserName,
                    proxyPassword: host.Proxy.Password,
                    authenticationMethods: auth);
            }

        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            base.Loaded += (sender, arg) =>
            {
                Task.Factory.StartNew(OpenConnect);
            };
        }

        private void Install(object sender, RoutedEventArgs e) { }

        private void UpdateSettings(object sender, RoutedEventArgs e) { }

        private void Uninstall(object sender, RoutedEventArgs e) { }

        private void UploadWeb(object sender, RoutedEventArgs e) { }

        private void UploadCert(object sender, RoutedEventArgs e) { }

        private void InstallCert(object sender, RoutedEventArgs e) { }

    }
}
