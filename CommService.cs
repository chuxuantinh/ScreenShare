using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.ServiceBus.Web;

namespace ScreenShare
{
    [ServiceContract]
    interface ICommContract
    {
        [OperationContract(Action = "GET", ReplyAction = "GETRESPONSE")]
        Message ProcessClientMessage(Message param);
    }

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
    public class CommService : ICommContract
    {
        public const string COMMAND = "RemoteScreen";
        const string PICTURE = "Picture";

        public delegate Stream GetResponseStreamDelegate(RequestInfo ri);
        public static GetResponseStreamDelegate dlgtGetResponseStream = null;

        public delegate object ProcessRequestDelegate(RequestInfo ri);
        public static ProcessRequestDelegate dlgtProcessRequest = null;

        public Message ProcessClientMessage(Message param)  
        {
            Message response = null;
            Stream stream = null;
            RequestInfo ri = ParseParam(param);

            object reqProcResult;
            if (dlgtGetResponseStream != null)
                reqProcResult = dlgtProcessRequest(ri);

            if (dlgtGetResponseStream != null)
                stream = dlgtGetResponseStream(ri);
            
            if (stream != null && stream.Length > 0)
            {
                stream.Position = 0;
                string contentType = ri.IsImage ? "image/jpeg" : "text/html";
                response = StreamMessageHelper.CreateMessage(OperationContext.Current.IncomingMessageVersion, "GETRESPONSE", stream);

                HttpResponseMessageProperty responseProperty = new HttpResponseMessageProperty();
                responseProperty.Headers.Add("Content-Type", contentType);
                response.Properties.Add(HttpResponseMessageProperty.Name, responseProperty);
            }
            
            return response;  
        }

        private static RequestInfo ParseParam(Message param)
        {
            Dictionary<string, string> dctArgs = null;
            DataType dataType = DataType.Html;
            string strParam = param.Properties.Via.PathAndQuery;
            if (strParam.ToLower().Contains(PICTURE.ToLower()))
                dataType = DataType.Image;

            dctArgs = ParseParam(param.Properties.Via.Query);
            if (dctArgs != null && dctArgs.Count > 0)
                dataType = DataType.Image;

            return new RequestInfo(dataType, dctArgs);
        }

        private static Dictionary<string, string> ParseParam(string query)
        {
            Dictionary<string, string> dctArgs = null;
            if (!string.IsNullOrEmpty(query))
            {
                string[] temp = query.Split(new char[] { '?', '=', '&' });
                if (temp.Length > 0)
                {
                    dctArgs = new Dictionary<string, string>();
                    for (int i = 1; i < temp.Length; i += 2)
                        dctArgs.Add(temp[i].ToLower(), temp[i + 1]);
                }
            }

            return dctArgs;
        }
    }
}
