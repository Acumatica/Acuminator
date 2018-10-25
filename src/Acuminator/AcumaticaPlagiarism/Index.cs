using System;

namespace AcumaticaPlagiarism
{
    public abstract class Index
    {
        public string Name { get; }
        public string Path { get; }
        public int Line { get; }
        public int Character { get; }

        public Index(string name, string path, int line, int character)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(nameof(name));

            if (string.IsNullOrEmpty(path))
                throw new ArgumentException(nameof(path));

            Name = name;
            Path = path;
            Line = line;
            Character = character;
        }
    }
}
