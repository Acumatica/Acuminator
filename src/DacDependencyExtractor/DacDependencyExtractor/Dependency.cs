using System;
using System.Collections.Generic;

namespace DacDependencyExtractor
{
    enum Connection
    {
        ForeignReference,
        Parent,
        Select,
        Default,
        Selector
    }
    struct DependencyDetails
    {
        public Type fromDac;
        public Type toDac;
        public Type fromField;
        public Type toField;
        public Connection connection;
    }

    class Dependency
    {
        public List<DependencyDetails> outDependency;
        public List<DependencyDetails> inDependency;
    }
}