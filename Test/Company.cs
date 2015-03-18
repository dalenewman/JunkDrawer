using System;

namespace Test {

    public class Company {
        public string Name { get; set; }
        public string WebSite { get; set; }
        public DateTime Created { get; set; }
    }

    public class CompanyForOldExcel
    {
        public string Name { get; set; }
        public string WebSite { get; set; }
        public Int32 Created { get; set; }
    }
}