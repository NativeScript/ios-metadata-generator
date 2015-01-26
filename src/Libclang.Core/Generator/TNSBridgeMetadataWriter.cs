using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Libclang.Core.Ast;
using Libclang.Core.Ast.Filters;
using Libclang.Core.Meta.Filters;
using Libclang.Core.Meta.Utils;
using Libclang.Core.Types;

namespace Libclang.Core.Generator
{
    public class TnsBridgeMetadataWriter
    {
        public TnsBridgeMetadataWriter(IEnumerable<DocumentDeclaration> frameworks)
        {
            this.Frameworks = frameworks;
        }

        public IEnumerable<DocumentDeclaration> Frameworks { get; private set; }

		public MetaContainer Generate(string metadataFile)
        {
            return this.GenerateMetadataContainer(metadataFile);
        }

        public MetaContainer GenerateMetadataContainer(string metadataFile)
        {
            MetaContainer metaContainer = new MetaContainer();
            metaContainer.Duplicates.Add (typeof(StructDeclaration), "kevent", "flock", "sigvec", "sigaction");
            metaContainer.Duplicates.Add(typeof(UnionDeclaration), "wait");
            metaContainer.Duplicates.Add(typeof(VarDeclaration), "timezone");
            metaContainer.Duplicates.Add(typeof(ProtocolDeclaration), "NSObject", "AVVideoCompositionInstruction", "OS_dispatch_data");

            this.AddFrameworksTo(metaContainer);
            this.Filter(metaContainer, metadataFile);

            return metaContainer;
        }

        private void AddFrameworksTo(MetaContainer metaContainer)
        {
            IEnumerable<BaseRecordDeclaration> allRecords = this.Frameworks.SelectMany(f => f.Records);

            IEnumerable<FunctionDeclaration> allFunctions = this.Frameworks.SelectMany(f => f.Functions).Where(f => f.IsValidFunction());

            IEnumerable<EnumDeclaration> allEnums = this.Frameworks.SelectMany(f => f.Enums);

            IEnumerable<VarDeclaration> allVars = Frameworks.SelectMany(f => f.Vars);

            IEnumerable<ProtocolDeclaration> allProtocols = this.Frameworks.SelectMany(f => f.Protocols);

            IEnumerable<InterfaceDeclaration> allInterfaces = this.Frameworks.SelectMany(f => f.Interfaces);

            IEnumerable<BaseDeclaration> allDeclarations = allRecords
                .Union(allFunctions.Cast<BaseDeclaration>())
                .Union(allEnums.Cast<BaseDeclaration>())
                .Union(allVars.Cast<BaseDeclaration>())
                .Union(allProtocols.Cast<BaseDeclaration>())
                .Union(allInterfaces.Cast<BaseDeclaration>())
                .Union(allProtocols.Cast<BaseDeclaration>());

            allDeclarations = this.Filter(allDeclarations);

            metaContainer.AddDeclarations(allDeclarations);
        }

        private IEnumerable<BaseDeclaration> Filter(IEnumerable<BaseDeclaration> declarations)
        {
            IDeclarationsFilter[] filters = new IDeclarationsFilter[] {
                new RemoveNotSupportedDeclarationsFilters()
            };

            foreach (IDeclarationsFilter filter in filters)
	        {
		        declarations = filter.Filter(declarations);
	        }

            return declarations;
        }

        private MetaContainer Filter(MetaContainer metaContainer, string metadataFile)
        {
            string metadataFileWithoutExt = Path.GetDirectoryName(metadataFile) + "/" + Path.GetFileNameWithoutExtension(metadataFile);
#if DEBUG
            using(var symbolsCount = new StreamWriter(metadataFileWithoutExt + "-count.txt"))
#endif
            {
                IMetaFilter[] filters = new IMetaFilter[]
                {
                    new RemoveDuplicateMembersFilter(),
                    new MarkMembersWithSameJsNamesInHierarchyFilter(),
                    new AnonymousEnumsToConstantsFilter(),
                    new BinarySerializer(metadataFile)
#if DEBUG
                    , new SymbolsCounter(symbolsCount)
                    , new DebugSerializer(metadataFileWithoutExt + ".xml")
#endif
                };
                metaContainer.Filter(filters);
            }

            return metaContainer;
        }
    }
}
