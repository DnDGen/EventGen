using Ninject.Modules;

namespace DnDGen.EventGen.IoC.Modules
{
    internal class EventGenModule : NinjectModule
    {
        public override void Load()
        {
            Bind<GenEventQueue>().To<DomainGenEventQueue>().InSingletonScope();
            Bind<ClientIDManager>().To<ThreadClientIDManager>().InSingletonScope();
        }
    }
}
