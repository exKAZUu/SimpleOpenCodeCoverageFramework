using System.Collections.Generic;

namespace SimpleOccf {
    class Options {
        [CommandLine.Option("handler", DefaultValue = "soccf.Gateway")]
        public string Handler { get; set; }

        [CommandLine.ValueList(typeof(List<string>))]
        public IList<string> Paths { get; set; }
    }
}
