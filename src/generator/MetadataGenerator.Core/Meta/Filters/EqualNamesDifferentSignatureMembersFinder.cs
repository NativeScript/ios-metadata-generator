using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MetadataGenerator.Core.Meta.Utils;
using MetadataGenerator.Core.Ast;

namespace MetadataGenerator.Core.Meta.Filters
{
    internal class EqualNamesDifferentSignatureMembersFinder : BaseMetaFilter
    {
        private readonly bool selectorComparison; // if false -> JsName Comparison

        public EqualNamesDifferentSignatureMembersFinder()
            : this(null, true)
        {
        }

        public EqualNamesDifferentSignatureMembersFinder(StreamWriter logger, bool selectiorComparison)
            : base(logger)
        {
            this.selectorComparison = selectiorComparison;
        }

        protected override Action<ModuleDeclarationsContainer, IDeclaration> ActionForEach
        {
            get { return this.ProcessSingleMeta; }
        }

        protected void ProcessSingleMeta(ModuleDeclarationsContainer metaContainer, IDeclaration meta)
        {
            if (meta is InterfaceDeclaration)
            {
                InterfaceDeclaration interfaceMeta = (InterfaceDeclaration)meta;
                MethodDeclaration[] allMethods = this.AllMethodsOf(interfaceMeta).ToArray();
                for (int i = 0; i < allMethods.Length; i++)
                {
                    MethodDeclaration method = allMethods[i];
                    for (int j = i + 1; j < allMethods.Length; j++)
                    {
                        MethodDeclaration possibleDuplicate = allMethods[j];
                        bool haveEqualNames = this.selectorComparison
                            ? (method.Selector == possibleDuplicate.Selector)
                            : (method.GetJSName() == possibleDuplicate.GetJSName());
                        if (haveEqualNames && method.IsStatic == possibleDuplicate.IsStatic &&
                            method.GetExtendedEncoding() != possibleDuplicate.GetExtendedEncoding())
                        {
                            string copyMethodMark = (method.Selector == "copy" || method.Selector == "copy:")
                                ? "(copy)"
                                : string.Empty;
                            string staticMark = (method.IsStatic) ? "+" : "-";
                            this.Log("{0} in: {1} ->  {2}[{3} {4}] [ {5} ]  -> {2}[{6} {7}] [ {8} ] ",
                                copyMethodMark, meta.Name, staticMark,
                                method.Parent.Name, method.Selector, method.GetExtendedEncoding(),
                                possibleDuplicate.Parent.Name, possibleDuplicate.Selector,
                                possibleDuplicate.GetExtendedEncoding());
                        }
                    }
                }
            }
        }

        private IEnumerable<MethodDeclaration> AllMethodsOf(BaseClass meta)
        {
            IEnumerable<MethodDeclaration> allMethods = meta.Methods
                .Union(meta.Properties.Select(m => m.Getter))
                .Union(meta.Properties.Select(m => m.Setter))
                .Where(m => m != null)
                .Union(meta.ImplementedProtocols.SelectMany(p => AllMethodsOf(p)));

            if (meta is InterfaceDeclaration)
            {
                InterfaceDeclaration interfaceMeta = (InterfaceDeclaration)meta;
                allMethods = allMethods.Union(interfaceMeta.Categories.SelectMany(c => AllMethodsOf(c)));
                InterfaceDeclaration baseMeta = interfaceMeta.Base;
                if (baseMeta != null)
                {
                    allMethods = allMethods.Union(AllMethodsOf(baseMeta));
                }
            }

            return allMethods;
        }
    }
}
