using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("DnDGen.EventGen.Tests.Integration")]
[assembly: InternalsVisibleTo("DnDGen.EventGen.Tests.Unit")]
namespace DnDGen.EventGen
{
    public interface ClientIDManager
    {
        void SetClientID(Guid clientID);
        void SetClientID(Guid clientID, Task task);
        Guid GetClientID();
    }
}
