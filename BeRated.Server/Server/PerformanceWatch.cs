using Ashod;
using System.Diagnostics;

namespace BeRated.Server
{
    class PerformanceWatch
    {
        private Stopwatch _Stopwatch;

        public PerformanceWatch()
        {
            _Stopwatch = new Stopwatch();
            _Stopwatch.Start();
        }

        public void Print(string description)
        {
            _Stopwatch.Stop();
            Logger.Log("{0}: {1} ms", description, _Stopwatch.ElapsedMilliseconds);
            _Stopwatch.Restart();
        }
    }
}
