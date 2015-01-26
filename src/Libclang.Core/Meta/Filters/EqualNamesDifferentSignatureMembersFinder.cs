using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Libclang.Core.Meta.Utils;

namespace Libclang.Core.Meta.Filters
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

        protected override Action<MetaContainer, Meta, string> ActionForEach
        {
            get { return this.ProcessSingleMeta; }
        }

        protected void ProcessSingleMeta(MetaContainer metaContainer, Meta meta, string key)
        {
            if (meta is InterfaceMeta)
            {
                InterfaceMeta interfaceMeta = (InterfaceMeta) meta;
                MethodMeta[] allMethods = this.AllMethodsOf(interfaceMeta).ToArray();
                for (int i = 0; i < allMethods.Length; i++)
                {
                    MethodMeta method = allMethods[i];
                    for (int j = i + 1; j < allMethods.Length; j++)
                    {
                        MethodMeta possibleDuplicate = allMethods[j];
                        bool haveEqualNames = this.selectorComparison
                            ? (method.Selector == possibleDuplicate.Selector)
                            : (method.JSName == possibleDuplicate.JSName);
                        if (haveEqualNames && method.IsStatic == possibleDuplicate.IsStatic &&
                            method.ExtendedEncoding != possibleDuplicate.ExtendedEncoding)
                        {
                            string copyMethodMark = (method.Selector == "copy" || method.Selector == "copy:")
                                ? "(copy)"
                                : string.Empty;
                            string staticMark = (method.IsStatic) ? "+" : "-";
                            this.Log("{0} in: {1} ->  {2}[{3} {4}] [ {5} ]  -> {2}[{6} {7}] [ {8} ] ",
                                copyMethodMark, meta.Name, staticMark,
                                method.Parent.Name, method.Selector, method.ExtendedEncoding,
                                possibleDuplicate.Parent.Name, possibleDuplicate.Selector,
                                possibleDuplicate.ExtendedEncoding);
                        }
                    }
                }
            }
        }

        private IEnumerable<MethodMeta> AllMethodsOf(BaseClassMeta meta)
        {
            IEnumerable<MethodMeta> allMethods = meta.Methods
                .Union(meta.Properties.Select(m => m.Getter))
                .Union(meta.Properties.Select(m => m.Setter))
                .Where(m => m != null)
                .Union(meta.ImplementedProtocols.SelectMany(p => AllMethodsOf(p)));

            if (meta is InterfaceMeta)
            {
                InterfaceMeta interfaceMeta = (InterfaceMeta) meta;
                allMethods = allMethods.Union(interfaceMeta.Categories.SelectMany(c => AllMethodsOf(c)));
                InterfaceMeta baseMeta = interfaceMeta.Base;
                if (baseMeta != null)
                {
                    allMethods = allMethods.Union(AllMethodsOf(baseMeta));
                }
            }

            return allMethods;
        }
    }
}
