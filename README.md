

## Proof of Concept on Intercepting HttpWebRequest

Manipuate the Uri object before http request was sent, good solution to forward requests to mockserver

sample code

```csharp
string serviceRoot = "https://movie-database-imdb-alternative.p.rapidapi.com";
string mockServerUrl = "http://localhost:1080";

HttpWebRequestInterceptor interceptor = 
    new HttpWebRequestInterceptor(new HttpWebRequestHandler(mockServerUrl));

WebRequest req = HttpWebRequest.Create(new Uri(serviceRoot));

WebResponse resp = req.GetResponse();
Console.WriteLine(new StreamReader(resp.GetResponseStream()).ReadToEnd());

```


manipulate the handler to add / modify the behavior

```csharp

   void IHttpWebRequestHandler.Add(HttpWebRequest request)
    {
        // update both _OriginUri and _Uri inside HttpWebRequest

        Uri newUri = new Uri(newUrl);

        request.GetType().InvokeMember("_OriginUri"
            , BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField
            , null
            , request
            , new object[] { newUri });

        request.GetType().InvokeMember("_Uri"
            , BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField
            , null
            , request
            , new object[] { newUri });

        typeof(WebRequest).InvokeMember("SetupCacheProtocol"
            , BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod
            , null
            , request
            , new object[] { newUri });
    }

```