using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POC
{
    public class UserActivityLog
    {
        public string systemusername { get; set; }
        public string systemdomain { get; set; }
        public string revitusername { get; set; }
        public string revitversion { get; set; }
        public string revitfilename { get; set; }
        public string toolname { get; set; }
        public string toolversion { get; set; }
        public string featureused { get; set; }
        public string description { get; set; }
        public string runstatus { get; set; }
        public DateTime startdate { get; set; }
        public DateTime enddate { get; set; }
    }
}
