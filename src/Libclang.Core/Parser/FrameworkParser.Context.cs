using Libclang.Core.Ast;
using Libclang.Core.Generator;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Libclang.Core.Parser
{
    public interface IDocumentResolver
    {
        DocumentDeclaration GetDocumentForDeclaration(IDeclaration declaration);
    }

    public class AutomaticDocumentResolver : IDocumentResolver
    {
        public AutomaticDocumentResolver(IList<DocumentDeclaration> documents, string sdkPath)
        {
            this.documents = documents;
            this.sdkPath = sdkPath;
        }

        private IList<DocumentDeclaration> documents;
        private static string clangPath = System.IO.Path.DirectorySeparatorChar + "clang";
        private string sdkPath;

        public DocumentDeclaration GetDocumentForDeclaration(IDeclaration declaration)
        {
            string framework = this.GetFrameworkName(declaration);
            DocumentDeclaration document = this.documents.SingleOrDefault(c => c.Name == framework);
            if (document == null)
            {
                document = new DocumentDeclaration(framework);
                document.Version = this.GetFrameworkVersion(declaration);
                this.documents.Add(document);
            }
            return document;
        }

        protected virtual string GetFrameworkName(IDeclaration declaration)
        {
            if (declaration.Location == null)
            {
                return null;
            }

            Match match = Regex.Match(declaration.Location.Filename, @"(\w+).framework");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            if (declaration.Location.Filename.EndsWith("sqlite3.h"))
            {
                return "Sqlite";
            }

            if (declaration.Location.Filename.Contains(clangPath))
            {
                return "clang";
            }

            if (declaration.Location.Filename.StartsWith(sdkPath))
            {
                return "UsrLib";
            }

            return "include";
        }

        protected virtual decimal GetFrameworkVersion(IDeclaration declaration)
        {
            if (declaration.Location == null)
            {
                return 0;
            }

            Match match = Regex.Match(declaration.Location.Filename, @"iPhoneOS(\d.\d)");
            if (match.Success)
            {
                decimal iosVersion;
                decimal.TryParse(match.Groups[1].Value, out iosVersion);
                return iosVersion;
            }

            return 0;
        }
    }

    public class SingleDocumentResolver : IDocumentResolver
    {
        public SingleDocumentResolver(DocumentDeclaration document)
        {
            this.document = document;
        }

        private DocumentDeclaration document;

        public DocumentDeclaration GetDocumentForDeclaration(IDeclaration declaration)
        {
            return this.document;
        }
    }

    public partial class FrameworkParser
    {
        public class ParserContext
        {
            public ParserContext(string sdkPath)
            {
                this.resolver = new AutomaticDocumentResolver(this.documents, sdkPath);
            }

            public ParserContext(DocumentDeclaration document)
            {
                this.resolver = new SingleDocumentResolver(document);
            }

            public readonly IList<DocumentDeclaration> documents = new List<DocumentDeclaration>();

            public readonly IDictionary<string, BaseDeclaration> usrToDeclaration =
                new Dictionary<string, BaseDeclaration>();

            public readonly IDictionary<string, IList<Types.DeclarationReferenceType>> usrToUnresolvedReferences =
                new Dictionary<string, IList<Types.DeclarationReferenceType>>();

            public readonly IDictionary<string, BaseDeclaration> nameToDeclaration =
                new Dictionary<string, BaseDeclaration>();

            public readonly IDocumentResolver resolver;
            private uint anonymousIDCount = 1;

            public void AddToUSRCache(BaseDeclaration declaration)
            {
                if (declaration.USR != null && !this.usrToDeclaration.ContainsKey(declaration.USR))
                {
                    this.usrToDeclaration.Add(declaration.USR, declaration);
                }
            }

            public T GetFromUSRCache<T>(string usr)
                where T : BaseDeclaration
            {
                BaseDeclaration decl = null;
                this.usrToDeclaration.TryGetValue(usr, out decl);
                return decl as T;
            }

            public void AddToNameCache(BaseDeclaration declaration)
            {
                if (!string.IsNullOrEmpty(declaration.Name) && !this.nameToDeclaration.ContainsKey(declaration.Name))
                {
                    this.nameToDeclaration.Add(declaration.Name, declaration);
                }
            }

            public T GetFromNameCache<T>(string fullName)
                where T : BaseDeclaration
            {
                return this.GetFromNameCache(fullName) as T;
            }

            public BaseDeclaration GetFromNameCache(string fullName)
            {
                BaseDeclaration decl = null;
                this.nameToDeclaration.TryGetValue(fullName, out decl);
                return decl;
            }

            public uint GetAnonymousTypeID()
            {
                uint currentId = this.anonymousIDCount;
                this.anonymousIDCount += 1;
                return currentId;
            }
        }
    }
}
