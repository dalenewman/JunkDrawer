using System;

namespace JunkDrawer {
    public interface IJunkBootstrapper : IDisposable {
        T Resolve<T>() where T : IResolvable;
    }
}