using System;

namespace BeRated.Server
{
    class ServerInstance : IServerInstance
    {
        [ServerMethod]
        public string Test(string argument1, int argument2)
        {
            return string.Format("{0}: {1}", argument1, 2 * argument2);
        }

        [ServerMethod]
        public Tuple<string, int> Test2()
        {
            return new Tuple<string, int>("String", 123);
        }

        void IDisposable.Dispose()
        {
        }

        string IServerInstance.GetMarkup(string json)
        {
            return "<!doctype html>\n<head>\n<title>Title</title>\n<script>var output = JSON.parse('" + json.Replace("'", "\\'") + "');</script>\n<body>\n</body>\n</html>";
        }
    }
}
