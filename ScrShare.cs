using System;
using System.IO;
using System.Collections;
using System.Configuration;
using System.Drawing;
using System.ComponentModel;
using System.Threading;
using System.ServiceProcess;
using System.ServiceModel;

namespace ScreenShare
{
	public class ScrShare
	{
        const double DEFAULT_SCREEN_MAGNIFICATION_FACTOR = 0.75;
        const int DEFAULT_SLEEP_BEFORE_SCREEN_CAPTURE_MS = 500;

        public static double ScreenMagnificationFactor { private set; get; }
        
        private const string htmlFileName = @"Htm\RemoteScreen.htm";

        static void Main(string[] args)
		{
            Console.Title = "ScreenShare";
            Console.WriteLine("ScreenShare started.");

            int sleepBeforeScreenCaptureMs = 0;
            try
            {
                ScreenMagnificationFactor = double.Parse(ConfigurationManager.AppSettings["ScreenMagnificationFactor"]);
                sleepBeforeScreenCaptureMs = int.Parse(ConfigurationManager.AppSettings["SleepBeforeScreenCaptureMs"]);
            }
            catch (Exception e)
            {
                Console.WriteLine("Input parameters error. Default values are used:\nScreenMagnificationFactor = {0},\nSleepBeforeScreenCaptureMs = {1}\n", 
                    DEFAULT_SCREEN_MAGNIFICATION_FACTOR, DEFAULT_SLEEP_BEFORE_SCREEN_CAPTURE_MS);
            }
            
            Stream streamRemoteScreenHtm = File.OpenText(htmlFileName).BaseStream;

            CommService.dlgtProcessRequest = (ri =>
            {
                bool br = false;
                int x = ri.X;
                int y = ri.Y;
                if (x > -1 && y > -1)
                {
                    try
                    {
                        double clientScreenSizeFactor = RequestInfo.dctRequestInfo[ri.SessionId].clientScreenSizeFactor;
                        x = (int)(x / clientScreenSizeFactor);
                        y = (int)(y / clientScreenSizeFactor);

                        ActionSimulation.MouseLeftClick(x, y);
                        Thread.Sleep(sleepBeforeScreenCaptureMs); // sleep before capturing screen
                        br = true;
                    }
                    catch (Exception e)
                    {
                    }

                    string text = RelpaceSymbols(ri.Text);
                    if (br && !string.IsNullOrEmpty(text))                           
                        for (int i = 0; i < text.Length; i++)
                            ActionSimulation.SendChar(text[i]);
                }

                return br;
            });

            CommService.dlgtGetResponseStream = (ri =>
            {
                return ri.IsImage ? ScreenCapture.CaptureScreen(ri) : streamRemoteScreenHtm;
            });

            ServiceHost host = new ServiceHost(typeof(CommService));
            host.Open();

            Console.WriteLine(string.Format("Service address: \n{0}/{1}\n", host.BaseAddresses[0].ToString(), CommService.COMMAND));

            BeforeQuit();
		}

        public static string RelpaceSymbols(string s)
        {
            string sr = null;
            if (!string.IsNullOrEmpty(s))
                // The replace procedure is not exhaustive
                sr = s.Replace("%5C", "|").Replace("%5c", @"|").
                       Replace("%7C", @"\").Replace("%7c", @"\").
                       Replace("%3A", ":").Replace("%3a", ":").
                       Replace("%2B", " ").Replace("%2b", " ").

                       Replace("%20", " ").
                       Replace(@"\\", @"\").

                       Replace("%2F", "/").Replace("%2f", "/").
                       Replace("%27", "'").
                       Replace("%2C", ",").Replace("%2c", ",").
                       //Replace("+", " ").
                       Replace("%28", "(").Replace("%29", ")").
                       Replace("%3F", "?").Replace("%3F", "?").
                       Replace("%26", "&").
                       Replace("%3B", ";").Replace("%3b", ";").
                       Replace("%3C", "<").Replace("%3c", "<").
                       Replace("%3E", ">").Replace("%3e", ">")
                       ;

            return sr;
        }

        public static void OnException(string outText, Exception e)
        {
            Console.WriteLine(string.Format("EXCEPTION: {0} \n{1}\n", outText, e));
        }

        private static void BeforeQuit()
        {
            Console.WriteLine("Press any key to quit...");
            Console.ReadKey();
        }
	}
}
