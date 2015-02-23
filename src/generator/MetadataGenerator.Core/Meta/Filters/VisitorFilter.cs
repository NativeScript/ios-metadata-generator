using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MetadataGenerator.Core.Ast;
using MetadataGenerator.Core.Meta.Visitors;
using MetadataGenerator.Core.Meta.Utils;

namespace MetadataGenerator.Core.Meta.Filters
{
    internal class VisitorFilter : IMetaFilter
    {
        public VisitorFilter(IDeclarationVisitor visitor)
        {
            this.visitor = visitor;
        }

        private readonly IDeclarationVisitor visitor;

        public void Filter(ModuleDeclarationsContainer metaContainer)
        {
            this.FilterIntern(metaContainer);
        }

        public void Filter(IEnumerable<IDeclaration> declarations)
        {
            this.FilterIntern(declarations);
        }

        private void FilterIntern(IEnumerable<IDeclaration> declarations)
        {
            IMetaContainerDeclarationVisitor mVisitor = visitor as IMetaContainerDeclarationVisitor;
            if (mVisitor != null)
            {
                mVisitor.Begin(declarations as ModuleDeclarationsContainer);
            }
            foreach (IDeclaration declaration in declarations)
            {
                declaration.Accept(visitor);
            }
            if (mVisitor != null)
            {
                mVisitor.End(declarations as ModuleDeclarationsContainer);
            }
        }
    }
}
