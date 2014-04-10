using System;
using System.Configuration;
using System.Linq;
using Transformalize.Configuration;
using Transformalize.Libs.NLog;
using Transformalize.Main;

namespace JunkDrawer {
    public class JunkReporter {

        private readonly Logger _log = LogManager.GetLogger("JunkDrawer.JunkReporter");
        private const string JUNK_SUMMARY = "JunkSummary";

        public void Report() {
            var matches = ((TransformalizeConfiguration)ConfigurationManager.GetSection("transformalize")).Processes.Cast<ProcessConfigurationElement>().Where(p => p.Name.Equals(JUNK_SUMMARY, StringComparison.OrdinalIgnoreCase)).ToArray();

            if (matches.Length <= 0) return;

            try {
                ProcessFactory.Create(JUNK_SUMMARY, new Options() { Mode = "init" })[0].Run();
                ProcessFactory.Create(JUNK_SUMMARY)[0].Run();
            } catch (Exception e) {
                _log.Warn("Sorry. I couldn't produce the junk summary {0}. {1}.", matches[0].Connections["output"].File, e.Message);
            }
        }
    }
}