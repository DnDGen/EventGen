using DnDGen.EventGen.IoC.Modules;
using Ninject;

namespace DnDGen.EventGen.IoC
{
    public class EventGenModuleLoader
    {
        public void LoadModules(IKernel kernel)
        {
            kernel.Load<EventGenModule>();
        }
    }
}
