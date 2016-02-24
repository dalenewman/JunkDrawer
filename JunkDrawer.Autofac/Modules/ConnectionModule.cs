using System.Collections.Generic;
using Autofac;
using Pipeline.Configuration;

namespace JunkDrawer.Autofac.Modules {
    public abstract class ConnectionModule : Module {
        readonly IEnumerable<Connection> _connections;

        protected ConnectionModule(IEnumerable<Connection> connections) {
            _connections = connections;
        }

        protected abstract void RegisterConnection(ContainerBuilder builder, Connection connection);

        protected override void Load(ContainerBuilder builder) {
            foreach (var c in _connections) {
                RegisterConnection(builder, c);
            }
        }
    }
}