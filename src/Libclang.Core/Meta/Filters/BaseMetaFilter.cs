using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Libclang.Core.Meta.Utils;

namespace Libclang.Core.Meta.Filters
{
    internal class BaseMetaFilter : IMetaFilter
    {
        protected virtual Action<MetaContainer> ActionForContainer { get; set; }

        protected virtual Action<MetaContainer, Meta, string> ActionForEach { get; set; }

        protected virtual Action<MetaContainer, BaseClassMeta, BaseClassMeta> ActionForEachPair { get; set; }

        public TextWriter Logger { get; set; }

        public BaseMetaFilter(TextWriter logger)
        {
            this.ActionForContainer = null;
            this.ActionForEach = null;
            this.ActionForEachPair = null;
            this.Logger = logger;
        }

        public void Filter(MetaContainer metaContainer)
        {
            this.Begin(metaContainer);
            this.Iterate(metaContainer);
            this.End(metaContainer);
        }

        protected virtual void Begin(MetaContainer metaContainer)
        {
        }

        protected virtual void End(MetaContainer metaContainer)
        {
        }

        private void Iterate(MetaContainer metaContainer)
        {
            if (this.ActionForContainer != null)
            {
                this.ActionForContainer(metaContainer);
            }

            if (this.ActionForEach != null || this.ActionForEachPair != null)
            {
                foreach (KeyValuePair<string, Meta> pair in metaContainer)
                {
                    if (this.ActionForEach != null)
                    {
                        this.ActionForEach(metaContainer, pair.Value, pair.Key);
                    }
                    if (this.ActionForEachPair != null && pair.Value is BaseClassMeta)
                    {
                        BaseClassMeta classMeta = (BaseClassMeta) pair.Value;
                        this.IteratePredecessorsOf(classMeta, metaContainer);
                    }
                }
            }
        }

        private void IteratePredecessorsOf(BaseClassMeta @class, MetaContainer metaContainer)
        {
            foreach (ProtocolMeta protocol in @class.ImplementedProtocols)
            {
                this.IteratePredecessorSuccessorPair(metaContainer, protocol, @class);
                // if is interface, remove duplicates from categories
                if (@class is InterfaceMeta)
                {
                    foreach (CategoryMeta fromCategory in ((InterfaceMeta) @class).Categories)
                    {
                        this.IteratePredecessorSuccessorPair(metaContainer, protocol, fromCategory);
                    }
                }
            }

            if (@class is InterfaceMeta)
            {
                InterfaceMeta baseMeta = ((InterfaceMeta) @class).Base;
                if (baseMeta != null)
                {
                    this.IteratePredecessorSuccessorPair(metaContainer, baseMeta, @class);
                    // if is interface, remove duplicates from categories
                    foreach (CategoryMeta classCategory in ((InterfaceMeta) @class).Categories)
                    {
                        this.IteratePredecessorSuccessorPair(metaContainer, baseMeta, classCategory);
                    }
                }
            }
        }

        private void IteratePredecessorSuccessorPair(MetaContainer metaContainer, BaseClassMeta predecessor,
            BaseClassMeta successor)
        {
            this.ActionForEachPair(metaContainer, predecessor, successor);

            // Recursively remove all duplicates in hierarchy
            foreach (ProtocolMeta protocol in predecessor.ImplementedProtocols)
            {
                if (protocol == successor)
                {
                    // A protocol is implemnted by itself.
                    this.Log("Error: Protocol {0} is implemnted by itself.", protocol.Name);
                }
                else
                {
                    this.IteratePredecessorSuccessorPair(metaContainer, protocol, successor);
                }
            }
            if (predecessor is InterfaceMeta)
            {
                foreach (CategoryMeta category in ((InterfaceMeta) predecessor).Categories)
                {
                    this.IteratePredecessorSuccessorPair(metaContainer, category, successor);
                }

                InterfaceMeta baseMeta = ((InterfaceMeta) predecessor).Base;
                if (baseMeta != null)
                {
                    this.IteratePredecessorSuccessorPair(metaContainer, baseMeta, successor);
                }
            }
        }

        protected void Log(string message, params object[] parameters)
        {
            if (this.Logger != null)
            {
                this.Logger.WriteLine(message, parameters);
            }
        }
    }
}
