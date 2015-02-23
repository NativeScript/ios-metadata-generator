using Libclang.Core.Ast;
using Libclang.Core.Ast.Filters;
using Libclang.Core.Meta.Filters;
using Libclang.Core.Meta.Utils;
using Libclang.Core.Meta.Visitors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Libclang.Core.Meta
{
    public class MetadataGenerator
    {
        public MetadataGenerator(string outputFolder)
        {
            this.outputFolder = outputFolder;
            this.debugDir = Path.Combine(this.outputFolder, "Debug");
        }

        private readonly string outputFolder;
        private readonly string debugDir;

        public void GenerateMetadata(IEnumerable<ModuleDeclaration> modules)
        {
            IDeclarationVisitor jsNameVisitor = new JSNameVisitor();
            IDeclarationVisitor extendedEncodingVisitor = new ExtendedEncodingVisitor();

            IEnumerable<ModuleDeclarationsContainer> moduleContainers = TransformationVisitor.Transform(modules, jsNameVisitor, extendedEncodingVisitor);
            foreach (ModuleDeclarationsContainer container in moduleContainers)
            {
                YamlSerializer yamlSerializer = new YamlSerializer();

                container.Apply(
                    new RemoveCategoriesFilter(),
                    new RemoveDuplicateMembersFilter(),
                    new MarkMembersWithSameJsNamesInHierarchyFilter(),
#if DEBUG
                    //new DebugSerializer(debugDir, string.Format("{0}MetadataDump", container.ModuleName)),
                    new VisitorFilter(new SymbolsCounter(Path.Combine(debugDir, string.Format("{0}SymbolsCount.txt", container.ModuleName)))),
#endif
                    new VisitorFilter(yamlSerializer));

                yamlSerializer.Save(this.outputFolder, container.ModuleName);
            }
        }
    }
}
