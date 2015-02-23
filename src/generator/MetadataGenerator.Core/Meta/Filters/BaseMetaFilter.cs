using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MetadataGenerator.Core.Meta.Utils;
using MetadataGenerator.Core.Ast;

namespace MetadataGenerator.Core.Meta.Filters
{
    internal class BaseMetaFilter : IMetaFilter
    {
        protected virtual Action<ModuleDeclarationsContainer> ActionForContainer { get; set; }

        protected virtual Action<ModuleDeclarationsContainer, IDeclaration> ActionForEach { get; set; }

        protected virtual Action<ModuleDeclarationsContainer, BaseClass, BaseClass> ActionForEachPair { get; set; }

        public TextWriter Logger { get; set; }

        public BaseMetaFilter(TextWriter logger)
        {
            this.ActionForContainer = null;
            this.ActionForEach = null;
            this.ActionForEachPair = null;
            this.Logger = logger;
        }

        public void Filter(ModuleDeclarationsContainer metaContainer)
        {
            this.Begin(metaContainer);
            this.Iterate(metaContainer);
            this.End(metaContainer);
        }

        protected virtual void Begin(ModuleDeclarationsContainer metaContainer)
        {
        }

        protected virtual void End(ModuleDeclarationsContainer metaContainer)
        {
        }

        private void Iterate(ModuleDeclarationsContainer metaContainer)
        {
            if (this.ActionForContainer != null)
            {
                this.ActionForContainer(metaContainer);
            }

            if (this.ActionForEach != null || this.ActionForEachPair != null)
            {
                foreach (IDeclaration declaration in metaContainer)
                {
                    if (this.ActionForEach != null)
                    {
                        this.ActionForEach(metaContainer, declaration);
                    }
                    if (this.ActionForEachPair != null && declaration is BaseClass)
                    {
                        this.IteratePredecessorsOf(declaration as BaseClass, metaContainer);
                    }
                }
            }
        }

        private void IteratePredecessorsOf(BaseClass @class, ModuleDeclarationsContainer metaContainer)
        {
            foreach (ProtocolDeclaration protocol in @class.ImplementedProtocols)
            {
                this.IteratePredecessorSuccessorPair(metaContainer, protocol, @class);
                // if is interface, remove duplicates from categories
                if (@class is InterfaceDeclaration)
                {
                    foreach (CategoryDeclaration fromCategory in ((InterfaceDeclaration)@class).Categories)
                    {
                        this.IteratePredecessorSuccessorPair(metaContainer, protocol, fromCategory);
                    }
                }
            }

            if (@class is InterfaceDeclaration)
            {
                InterfaceDeclaration baseMeta = ((InterfaceDeclaration)@class).Base;
                if (baseMeta != null)
                {
                    this.IteratePredecessorSuccessorPair(metaContainer, baseMeta, @class);
                    // if is interface, remove duplicates from categories
                    foreach (CategoryDeclaration classCategory in ((InterfaceDeclaration)@class).Categories)
                    {
                        this.IteratePredecessorSuccessorPair(metaContainer, baseMeta, classCategory);
                    }
                }
            }
        }

        private void IteratePredecessorSuccessorPair(ModuleDeclarationsContainer metaContainer, BaseClass predecessor,
            BaseClass successor)
        {
            this.ActionForEachPair(metaContainer, predecessor, successor);

            // Recursively remove all duplicates in hierarchy
            foreach (ProtocolDeclaration protocol in predecessor.ImplementedProtocols)
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
            if (predecessor is InterfaceDeclaration)
            {
                foreach (CategoryDeclaration category in ((InterfaceDeclaration)predecessor).Categories)
                {
                    this.IteratePredecessorSuccessorPair(metaContainer, category, successor);
                }

                InterfaceDeclaration baseMeta = ((InterfaceDeclaration)predecessor).Base;
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
