using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunkDrawer {
    public class JunkImporter {
        private readonly IJunkLogger _logger;

        public JunkImporter(IJunkLogger logger) {
            _logger = logger;
        }

        public Response Import(Request request) {
            return new Response();
        }
    }
}
