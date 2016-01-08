using System.Collections.Generic;
using System.Linq;
using Cfg.Net;
using Cfg.Net.Contracts;
using Pipeline.Configuration;

namespace JunkDrawer {

    public class JunkCfg : CfgNode {

        public JunkCfg(string cfg, params IDependency[] dependencies) : base(dependencies) {
            Load(cfg);
        }

        [Cfg(value = 3)]
        public int Retries { get; set; }

        [Cfg(required = true)]
        public List<Connection> Connections { get; set; }

        protected override void Validate() {
            if (Connections.Count < 2) {
                Error("You need two connections defined; the first one for input, the last one for output.");
            }
        }

        public Connection Input() {
            return Connections.First();
        }

        public Connection Output() {
            return Connections.Last();
        }
    }

}