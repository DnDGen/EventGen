using EventGen.IoC.Modules;
using Ninject;

namespace EventGen.IoC
{
    public class EventGenModuleLoader
    {
        public void LoadModules(IKernel kernel)
        {
            kernel.Load<EventGenModule>();
        }
    }
}
