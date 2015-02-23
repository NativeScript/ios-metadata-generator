using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libclang.Core.Ast.Filters
{
    public interface IDeclarationsFilter
    {
        void Filter(IList<IDeclaration> declarations);
    }
}
