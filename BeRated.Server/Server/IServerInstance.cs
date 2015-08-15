using System;

namespace BeRated.Server
{
    public interface IServerInstance : IDisposable
    {
        string GetMarkup(string json);
    }
}
