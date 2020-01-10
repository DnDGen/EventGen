using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("EventGen.Tests.Integration")]
[assembly: InternalsVisibleTo("EventGen.Tests.Unit")]
namespace EventGen
{
    public interface ClientIDManager
    {
        void SetClientID(Guid clientID);
        Guid GetClientID();
    }
}
