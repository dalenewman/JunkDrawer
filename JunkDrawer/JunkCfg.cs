using System.Collections.Generic;
using Transformalize.Configuration;
using Transformalize.Libs.Cfg.Net;

namespace JunkDrawer {

    public class JunkCfg : CfgNode {

        public JunkCfg(string cfg) {
            Load(cfg);
        }

        [Cfg(required = false)]
        public List<TflConnection> Connections { get; set; }

        [Cfg]
        public List<TflLog> Log { get; set; }

        [Cfg(required = false)]
        public List<JunkFileInspection> FileInspection { get; set; }

        protected override void Validate() {

            if (Connections.Count < 1) {
                AddProblem("You must have at least one connection defined.");
            }

            if (FileInspection.Count != 1) {
                AddProblem("You must have one file inspection defined.");
            }

            if (Log.Count == 0) {
                var log = this.GetDefaultOf<TflLog>(l => {
                    l.Name = "file";
                    l.Provider = "file";
                    l.Level = "Informational";
                    l.File = "Junk-Drawer-{Date}.log";
                });
                Log.Add(log);
            }
        }
    }

    public class JunkFileInspection : TflFileInspection {

        [Cfg(value = 5)]
        public int Retries { get; set; }
    }
}