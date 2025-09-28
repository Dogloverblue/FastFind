using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMauiApp
{
    public class ConfigData
    {
        public bool ScanEnabled { get; set; } = true;
        public bool OptionB { get; set; }
        public bool OptionC { get; set; }

        public List<string> FilePaths { get; set; } = [];
}
}
