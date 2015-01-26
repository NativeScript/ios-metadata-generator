using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libclang.Core.Ast.Filters
{
    public interface IDeclarationsFilter
    {
        IEnumerable<BaseDeclaration> Filter(IEnumerable<BaseDeclaration> declarations);
    }
}
