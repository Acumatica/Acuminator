using Microsoft.CodeAnalysis;
using System;

namespace AcumaticaPlagiarism
{
    public abstract class Index
    {
        public string Name { get; }
        public FileLinePositionSpan Location { get; }

        public Index(string name, FileLinePositionSpan location)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(nameof(name));
            }

            if (!location.IsValid)
            {
                throw new ArgumentException(nameof(location));
            }

            Name = name;
            Location = location;
        }
    }
}
