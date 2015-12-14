using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FASystem
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// アプリケーションが終了する時のイベント。
        /// </summary>
        /// <param name="e">イベント データ。</param>
        protected override void OnExit(ExitEventArgs e)
        {
        }


        /// <summary>
        /// アプリケーションが開始される時のイベント。
        /// </summary>
        /// <param name="e">イベント データ。</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            Console.WriteLine("OnStartup");
        }
    }
}
