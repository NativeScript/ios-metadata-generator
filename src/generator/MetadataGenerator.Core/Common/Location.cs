using System;

namespace Libclang.Core.Common
{
    public class Location
    {
        public string Filename { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }

        public Location(string fileName, int line, int column)
        {
            this.Filename = fileName;
            this.Line = line;
            this.Column = column;
        }

#if DEBUG
        public override string ToString()
        {
            return string.Format("LOCATION: Filename: {0}; Line: {1}; Column: {2}", this.Filename, this.Line,
                this.Column);
        }
#endif

        public static implicit operator Location(NClang.ClangIndex.Location location)
        {
            Location newLocation = new Location(location.FileLocation.File.FileName, location.FileLocation.Line,
                location.FileLocation.Column);
            return newLocation;
        }
    }
}
