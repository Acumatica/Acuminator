using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace ForceGraph
{
    class ForceGraph
    {
        Dictionary<string,Dependency> _dependencies = new Dictionary<string, Dependency>();
        Dictionary<string, int> _str2ind = new Dictionary<string, int>();
        Dictionary<int, string> _ind2str = new Dictionary<int, string>();
        public struct DependencyDetails
        {
            public String fromDac;
            public String toDac;
            public String fromField;
            public String toField;
            public String connection;
        }

        public class Dependency
        {
            public List<DependencyDetails> outDependency;
            public List<DependencyDetails> inDependency;
        }
        //dumb load
        public void LoadGraph(string fileName)
        {
            var file = ManifestLoader.LoadTextFile(@"dependencies.txt");
            int index = 0;
            foreach (var line in file.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
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
                _dependencies.Add(dac, dependency);
                _str2ind.Add(dac, index);
                _ind2str.Add(index, dac);
                ++index;
            }
        }

        public List<string> GetDacs()
        {
            return _dependencies.Keys.ToList();
        }

        public Dependency GetDependency(string dac)
        {
            return _dependencies[dac];
        }

        public Dependency GetDependency(int index)
        {
            return _dependencies[_ind2str[index]];
        }

        public int GetIndex(string Dac)
        {
            return _str2ind[Dac];
        }

        public string GetDac(int index)
        {
            return _ind2str[index];
        }

    }
}
