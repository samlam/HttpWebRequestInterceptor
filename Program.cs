using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpWebReqInterceptor
{
    class Program
    {
        static void Main(string[] args)
        {
            string serviceRoot = "https://movie-database-imdb-alternative.p.rapidapi.com";
            string mockServerUrl = "http://localhost:1080";

            HttpWebRequestInterceptor interceptor = new HttpWebRequestInterceptor(new HttpWebRequestHandler(mockServerUrl));

            WebRequest req = HttpWebRequest.Create(new Uri(serviceRoot));

            WebResponse resp = req.GetResponse();
            Console.WriteLine(new StreamReader(resp.GetResponseStream()).ReadToEnd());

            Console.ReadKey();
        }
    }



}
