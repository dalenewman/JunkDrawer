#region license
// JunkDrawer.Autofac
// Copyright 2013 Dale Newman
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
using Pipeline.Contracts;

namespace JunkDrawer.Autofac {
    public class AutofacJunkBootstrapper : IJunkBootstrapper {
        private readonly ILifetimeScope _scope;
        public AutofacJunkBootstrapper(JunkRequest request, IPipelineLogger logger = null) {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new JunkModule(request, logger));
            builder.RegisterModule(new JunkImportModule());
            builder.RegisterModule(new JunkPageModule());
            _scope = builder.Build().BeginLifetimeScope();
        }

        public AutofacJunkBootstrapper(IPipelineLogger logger = null) {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new JunkModule(logger));
            builder.RegisterModule(new JunkImportModule());
            builder.RegisterModule(new JunkPageModule());
            _scope = builder.Build().BeginLifetimeScope();
        }

        public void Dispose() {
            _scope.Dispose();
        }

        public T Resolve<T>() where T : IResolvable {
            return _scope.Resolve<T>();
        }

        public T Resolve<T>(JunkRequest request) where T : IResolvable {
            return _scope.Resolve<T>(new TypedParameter(typeof(JunkRequest), request));
        }

        public T Resolve<T>(JunkRequest request, JunkResponse response) where T : IResolvable {
            return _scope.Resolve<T>(new TypedParameter(typeof(JunkRequest), request), new TypedParameter(typeof(JunkResponse), response));
        }

    }
}