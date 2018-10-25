using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForceGraph
{
    class ForceGraph
    {
        List<Tuple<string,Dependency>> _dependencies = new List<Tuple<string,Dependency>>();
        struct DependencyDetails
        {
            public String fromDac;
            public String toDac;
            public String fromField;
            public String toField;
            public String connection;
        }

        class Dependency
        {
            public List<DependencyDetails> outDependency;
            public List<DependencyDetails> inDependency;
        }
        //dumb load
        public void LoadGraph(string fileName)
        {
            var file = new System.IO.StreamReader(@"dependencies.txt");
            string line;
            while ((line = file.ReadLine()) != null)
            {
                var dependency = new Dependency();
                string dac = null;
                dependency.outDependency = new List<DependencyDetails>();
                dependency.inDependency = new List<DependencyDetails>();
                var split = line.Split(new char[] { ';' });
                dac = split[0];
                var outSplit = split[1].Split(new char[] { '|' });
                foreach(var o in outSplit)
                {
                    if (string.IsNullOrEmpty(o))
                        continue;
                    var dSplit = o.Split(new char[] { ',' });
                    DependencyDetails d = new DependencyDetails();
                    d.fromDac = dSplit[0];
                    d.fromField = dSplit[1];
                    d.toDac = dSplit[2];
                    d.toField = dSplit[3];
                    d.connection = dSplit[4];
                    dependency.outDependency.Add(d);
                }
                var inSplit = split[2].Split(new char[] { '|' });
                foreach(var i in inSplit)
                {
                    if (string.IsNullOrEmpty(i))
                        continue;
                    var dSplit = i.Split(new char[] { ',' });
                    DependencyDetails d = new DependencyDetails();
                    d.fromDac = dSplit[0];
                    d.fromField = dSplit[1];
                    d.toDac = dSplit[2];
                    d.toField = dSplit[3];
                    d.connection = dSplit[4];
                    dependency.inDependency.Add(d);
                }
                var tuple = new Tuple<string, Dependency>(dac, dependency);
                _dependencies.Add(tuple);
            }
        }
    }
}
