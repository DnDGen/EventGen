using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DnDGen.EventGen.Tests.Integration")]
[assembly: InternalsVisibleTo("DnDGen.EventGen.Tests.Unit")]
namespace DnDGen.EventGen
{
    public interface ClientIDManager
    {
        void SetClientID(Guid clientID);
        Guid GetClientID();
    }
}
