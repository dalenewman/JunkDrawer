#region license
// JunkDrawer
// An easier way to import excel or delimited files into a database.
// Copyright 2013-2017 Dale Newman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using Autofac;
using Transformalize.Contracts;

namespace JunkDrawer.Autofac {
    public class Bootstrapper : IJunkBootstrapper {

        private readonly ILifetimeScope _scope;

        public Bootstrapper(Request request, IPipelineLogger logger = null) {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new JunkModule(request, logger));
            builder.RegisterModule(new ImportModule());
            builder.RegisterModule(new PageModule());
            _scope = builder.Build().BeginLifetimeScope();
        }

        public Bootstrapper(IPipelineLogger logger = null) {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new JunkModule(logger));
            builder.RegisterModule(new ImportModule());
            builder.RegisterModule(new PageModule());
            _scope = builder.Build().BeginLifetimeScope();
        }

        public void Dispose() {
            _scope.Dispose();
        }

        public T Resolve<T>() where T : IResolvable {
            return _scope.Resolve<T>();
        }

        public T Resolve<T>(Request request) where T : IResolvable {
            return _scope.Resolve<T>(new TypedParameter(typeof(Request), request));
        }

        public T Resolve<T>(Request request, Response response) where T : IResolvable {
            return _scope.Resolve<T>(new TypedParameter(typeof(Request), request), new TypedParameter(typeof(Response), response));
        }

    }
}