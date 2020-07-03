using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Chat_App.Methods.Api_elements
{
    public class APICallReq
    {
        public static StreamReader streamReader(string jsonStr, HttpWebRequest httpWebRequest)
        {
            //Api settings:
            httpWebRequest.ContentType = "text/json";
            httpWebRequest.Method = "POST";

            //Sending the message
            JObject json = JObject.Parse(jsonStr);
            var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream());
            streamWriter.Write(json);
            streamWriter.Flush();
            httpWebRequest.Timeout = 999999;

            //Recieving the response
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            Stream stream = httpResponse.GetResponseStream();
            return new StreamReader(stream, Encoding.UTF8);

        }
    }
}
