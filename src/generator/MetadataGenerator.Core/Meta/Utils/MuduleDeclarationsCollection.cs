using System;
using Libclang.Core.Ast;
using System.Collections.ObjectModel;
using Libclang.Core.Meta.Filters;

namespace Libclang.Core.Meta.Utils
{
    public class MuduleDeclarationsCollection : Collection<IDeclaration>
    {
        public MuduleDeclarationsCollection(ModuleDeclaration module)
        {
            this.Module = module;
        }

        public ModuleDeclaration Module { get; private set; }

        public void Apply(params IMetaFilter[] filters)
        {
            foreach (IMetaFilter filter in filters)
            {
                filter.Filter(this);
            }
        }
    }
}
