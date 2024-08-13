using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using Masuit.Tools.Security;
using Newtonsoft.Json;
using Sunny.UI;

namespace MxbcRobOrderWinFormsApp
{
   

    internal static class ProgramMain
    {
        public const double Version = 2.0;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Task.Run(HttpClientUtil.CheckVersionInfo);
            ApplicationConfiguration.Initialize();
            Application.Run(new FormMain());
        }
    }
}