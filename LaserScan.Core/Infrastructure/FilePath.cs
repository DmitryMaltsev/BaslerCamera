using System;
using System.Linq;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    public class FilePath
    {
        #region Properties
        public string Path { get; }
        #endregion

        #region Constructors
        public FilePath(string path) =>
            Path = string.IsNullOrEmpty(path) ?
                      throw new ArgumentException("path cannot be null or empty") :
                      System.IO.Path.GetInvalidPathChars().Intersect(path).Any() ?
                         throw new ArgumentException("path contains illegal characters") :
                         System.IO.Path.GetFullPath(path.Trim());
        #endregion

        #region Methods
        public System.IO.FileInfo GetInfo() => new(Path);

        public FilePath Combine(params string[] paths) =>
            System.IO.Path.Combine(paths.Prepend(Path).ToArray());
        #endregion

        #region Virtual methods
        public virtual bool Equals(FilePath other) =>
            Path.Equals(other?.Path, StringComparison.InvariantCultureIgnoreCase);
        #endregion

        #region Operators methods
        public static implicit operator FilePath(string name) => new(name);
        #endregion

        #region Override methods
        public override string ToString() => Path;

        public override int GetHashCode() => Path.ToLowerInvariant().GetHashCode();
        #endregion
    }
}
