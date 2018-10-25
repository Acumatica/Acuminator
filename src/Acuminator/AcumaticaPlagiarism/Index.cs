using System;

namespace AcumaticaPlagiarism
{
    public abstract class Index
    {
        public string Name { get; private set; }
        public string Location { get; private set; }

        public Index(string name, string location)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(nameof(name));

            if (string.IsNullOrEmpty(location))
                throw new ArgumentException(nameof(location));

            Name = name;
            Location = location;
        }
    }
}
