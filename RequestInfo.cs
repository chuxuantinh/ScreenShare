using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScreenShare
{
    public enum DataType
    {
        Html,
        Image,
    }

    public class State
    {
        public double clientScreenSizeFactor = double.NaN;
    }

    public class RequestInfo
    {
        public static Dictionary<double, State> dctRequestInfo = new Dictionary<double, State>();

        private DataType dataType;

        public RequestInfo(DataType dataType, Dictionary<string, string> dctArgs)
        {
            this.dataType = dataType;
 
            int x = -1; 
            int y  =-1; 
            Text = string.Empty; 
            IsFullSize = false; 
            Browser = string.Empty; 
            SessionId = double.NaN;

            if (dctArgs != null)
            {
                X = dctArgs.ContainsKey("x") && int.TryParse(dctArgs["x"], out x) ? x : -1;
                Y = dctArgs.ContainsKey("y") && int.TryParse(dctArgs["y"], out y) ? y : -1;
                
                if (dctArgs.ContainsKey("text"))
                    Text = dctArgs["text"];
                
                if (dctArgs.ContainsKey("full"))
                    IsFullSize = dctArgs["full"] == "1";
               
                if (dctArgs.ContainsKey("browser"))
                    Browser = dctArgs["browser"];
                
                if (dctArgs.ContainsKey("sessionid"))
                    SessionId = double.Parse(dctArgs["sessionid"]);
            }
        }

        public bool IsImage
        {
            get { return dataType == DataType.Image; }
        }

        public int X { private set; get; }
        public int Y  { private set; get; }
        public string Text { private set; get; }
        public bool IsFullSize { private set; get; }
        public string Browser { private set; get; }
        public double SessionId { private set; get; }
    }
}
