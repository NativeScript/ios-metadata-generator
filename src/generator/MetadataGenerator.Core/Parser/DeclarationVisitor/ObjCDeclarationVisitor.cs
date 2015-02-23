using Libclang.Core.Ast;
using Libclang.Core.Types;
using NClang;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Libclang.Core.Parser
{
    public class ObjCDeclarationVisitor : DeclarationVisitor
    {
        public ObjCDeclarationVisitor(ModuleDeclaration document)
            : base(new FrameworkParser.ParserContext(document))
        {
        }

        public ObjCDeclarationVisitor(FrameworkParser.ParserContext context)
            : base(context)
        {
        }

        protected override Dictionary<IndexEntityKind, Func<ClangIndex.DeclarationInfo, BaseDeclaration>>
            CreateDefinitionParserMapper()
        {
            var definitionParserMapper =
                new Dictionary<IndexEntityKind, Func<ClangIndex.DeclarationInfo, BaseDeclaration>>();
            definitionParserMapper.Add(IndexEntityKind.ObjCClass, this.VisitObjCClass);
            definitionParserMapper.Add(IndexEntityKind.ObjCProtocol, this.VisitObjCProtocol);
            definitionParserMapper.Add(IndexEntityKind.ObjCCategory, this.VisitObjCCategory);
            definitionParserMapper.Add(IndexEntityKind.ObjCInstanceMethod, this.VisitObjCInstanceMethod);
            definitionParserMapper.Add(IndexEntityKind.ObjCClassMethod, this.VisitObjCClassMethod);
            definitionParserMapper.Add(IndexEntityKind.ObjCProperty, this.VisitObjCProperty);
            return definitionParserMapper;
        }

        protected virtual BaseDeclaration VisitObjCClass(ClangIndex.DeclarationInfo declaration)
        {
            InterfaceDeclaration @class =
                this.context.GetFromNameCache<InterfaceDeclaration>(declaration.EntityInfo.Name);
            if (@class == null)
            {
                @class = new InterfaceDeclaration(declaration.EntityInfo.Name);
                this.context.AddToNameCache(@class);
            }

            if (!declaration.IsContainer)
            {
                this.ResolveUnresolvedReference(@class);
                return null;
            }

            @class.Location = declaration.Location;
            @class.Module = context.resolver.GetModule(declaration.Cursor);
            @class.USR = declaration.Cursor.CreateUSR();

            ClangIndex.ObjCInterfaceDeclarationInfo info = declaration.ObjCInterfaceDeclaration;

            if (info.Super != null)
            {
                @class.Base =
                    this.context.GetFromUSRCache<InterfaceDeclaration>(info.Super.EntityInfo.Cursor.CreateUSR());
            }
            if (info.Protocols.Count > 0)
            {
                foreach (ProtocolDeclaration protocol in this.GetImplementedProtocols(declaration))
                {
                    @class.ImplementedProtocols.Add(protocol);
                }
                Debug.Assert(@class.ImplementedProtocols.Count == info.Protocols.Count,
                    "The number of protocols found doesn't match");
            }

            ClangCursor parentContainer = declaration.SemanticContainer.Cursor;
            System.Diagnostics.Debug.Assert(parentContainer.Kind == CursorKind.TranslationUnit,
                "The parent container of a class is not the TranslationUnit. " +
                "Actual Type: " + parentContainer.Kind);
            this.AddDeclaration(@class, true, false);
            return @class;
        }

        protected virtual BaseDeclaration VisitObjCProtocol(ClangIndex.DeclarationInfo declaration)
        {
            ProtocolDeclaration @proto = this.context.GetFromNameCache<ProtocolDeclaration>(declaration.EntityInfo.Name);
            if (@proto == null)
            {
                @proto = new ProtocolDeclaration(declaration.EntityInfo.Name);
                this.context.AddToNameCache(@proto);
            }

            if (!declaration.IsContainer)
            {
                this.ResolveUnresolvedReference(@proto);
                return null;
            }

            @proto.Location = declaration.Location;
            @proto.Module = context.resolver.GetModule(declaration.Cursor);
            @proto.USR = declaration.Cursor.CreateUSR();
            ClangCursor parentContainer = declaration.SemanticContainer.Cursor;
            System.Diagnostics.Debug.Assert(parentContainer.Kind == CursorKind.TranslationUnit,
                "The parent container of a protocol is not the TranslationUnit. " +
                "Actual Type: " + parentContainer.Kind);
            this.AddDeclaration(@proto, true, false);

            ClangIndex.ObjCProtocolReferenceListDeclarationInfo implementedProtocols =
                declaration.ObjCProtocolReferenceListDeclaration;
            if (implementedProtocols.Count > 0)
            {
                foreach (ProtocolDeclaration protocol in this.GetImplementedProtocols(declaration))
                {
                    @proto.ImplementedProtocols.Add(protocol);
                }
                Debug.Assert(@proto.ImplementedProtocols.Count == implementedProtocols.Count,
                    "The number of protocols found doesn't match");
            }
            return @proto;
        }

        protected virtual BaseDeclaration VisitObjCCategory(ClangIndex.DeclarationInfo declaration)
        {
            System.Diagnostics.Debug.Assert(declaration.IsDefinition,
                "Found forward declared category " + declaration.EntityInfo.Name);

            ClangIndex.ObjCCategoryDeclarationInfo info = declaration.ObjCCategoryDeclaration;
            CategoryDeclaration category = new CategoryDeclaration(declaration.EntityInfo.Name,
                this.context.GetFromUSRCache<InterfaceDeclaration>(info.Class.Cursor.CreateUSR()))
            {
                USR = declaration.Cursor.CreateUSR(),
                Location = declaration.Location,
                Module = context.resolver.GetModule(declaration.Cursor)
            };

            ClangIndex.ObjCProtocolReferenceListDeclarationInfo implementedProtocols =
                declaration.ObjCProtocolReferenceListDeclaration;
            if (implementedProtocols.Count > 0)
            {
                foreach (ProtocolDeclaration protocol in this.GetImplementedProtocols(declaration))
                {
                    category.ImplementedProtocols.Add(protocol);
                }
                Debug.Assert(category.ImplementedProtocols.Count == implementedProtocols.Count,
                    "The number of protocols found doesn't match");
            }

            category.ExtendedInterface.Categories.Add(category);
            ClangCursor parentContainer = declaration.SemanticContainer.Cursor;
            System.Diagnostics.Debug.Assert(parentContainer.Kind == CursorKind.TranslationUnit,
                "The parent container of a category is not the TranslationUnit. " +
                "Actual Type: " + parentContainer.Kind);
            this.AddDeclaration(category);
            return category;
        }

        protected virtual MethodDeclaration VisitMethodDeclaration(ClangIndex.DeclarationInfo declaration,
            BaseClass owner, bool isStatic)
        {
            ClangCursor cursor = declaration.Cursor;
            MethodDeclaration @method = new MethodDeclaration(owner, declaration.EntityInfo.Name,
                ParseClangType(cursor.ResultType), cursor.DeclObjCTypeEncoding)
            {
                IsVariadic = cursor.IsVariadic,
                IsOptional = cursor.IsObjCOptional,
                IsImplicit = declaration.IsImplicit,
                IsStatic = isStatic
            };

            bool[] flags = ObjCDeclarationVisitor.HasStringsInFileNearCursor(cursor, "NS_REQUIRES_NIL_TERMINATION", "NS_RETURNS_RETAINED", "NS_RETURNS_NOT_RETAINED");
            @method.IsNilTerminatedVariadic = flags[0];
            @method.NsReturnsRetained = flags[1] ? (bool?)true : (flags[2] ? (bool?)false : null);

            // add the arguments
            foreach (ParameterDeclaration paramDecl in this.ParseParameters(cursor))
            {
                @method.AddParameter(paramDecl);
            }

            owner.Methods.Add(@method);

            // Add this method to its overriden parent
            foreach (ClangCursor overridenCursor in cursor.OverridenCursors)
            {
                ClangCursor parent = overridenCursor.LexicalParent;
                BaseClass baseClass = this.context.GetFromUSRCache<BaseClass>(parent.CreateUSR());
                MethodDeclaration @baseMethod = baseClass.Methods.Single(c => c.Name == @method.Name);
                // TODO: Check if we haven't already added this method to this parent.
                @baseMethod.Overrides.Add(@method);
            }

            return @method;
        }

        protected virtual BaseDeclaration VisitObjCInstanceMethod(ClangIndex.DeclarationInfo declaration)
        {
            BaseClass parentClass = this.GetParentClass(declaration.LexicalContainer.Cursor);
            if (parentClass == null)
            {
                return null;
            }
            return VisitMethodDeclaration(declaration, parentClass, false);
        }

        protected virtual BaseDeclaration VisitObjCClassMethod(ClangIndex.DeclarationInfo declaration)
        {
            BaseClass parentClass = this.GetParentClass(declaration.LexicalContainer.Cursor);
            if (parentClass == null)
            {
                return null;
            }
            return VisitMethodDeclaration(declaration, parentClass, true);
        }

        protected virtual BaseDeclaration VisitObjCProperty(ClangIndex.DeclarationInfo declaration)
        {
            ClangIndex.ObjCPropertyDeclarationInfo info = declaration.ObjCPropertyDeclaration;
            BaseClass currentInterface = this.GetParentClass(declaration.LexicalContainer.Cursor);
            if (currentInterface == null)
            {
                return null;
            }

            ObjCPropertyAttributeFlags propertyFlags = declaration.Cursor.ObjCPropertyAttributes;

            PropertyDeclaration propertyDeclaration = new PropertyDeclaration(currentInterface,
                declaration.EntityInfo.Name, ParseClangType(declaration.Cursor.CursorType))
            {
                IsAssign = propertyFlags.HasFlag(ObjCPropertyAttributeFlags.Assign),
                IsAtomic = propertyFlags.HasFlag(ObjCPropertyAttributeFlags.Atomic),
                IsCopy = propertyFlags.HasFlag(ObjCPropertyAttributeFlags.Copy),
                IsNonatomic = propertyFlags.HasFlag(ObjCPropertyAttributeFlags.NonAtomic),
                IsReadonly = propertyFlags.HasFlag(ObjCPropertyAttributeFlags.ReadOnly),
                IsReadwrite = propertyFlags.HasFlag(ObjCPropertyAttributeFlags.ReadWrite),
                IsRetain = propertyFlags.HasFlag(ObjCPropertyAttributeFlags.Retain),
                IsStrong = propertyFlags.HasFlag(ObjCPropertyAttributeFlags.Strong),
                IsUnsafeUnretained = propertyFlags.HasFlag(ObjCPropertyAttributeFlags.UnsafeUnretained),
                IsWeak = propertyFlags.HasFlag(ObjCPropertyAttributeFlags.Weak),
                HasCustomGetter = propertyFlags.HasFlag(ObjCPropertyAttributeFlags.Getter),
                HasCustomSetter = propertyFlags.HasFlag(ObjCPropertyAttributeFlags.Setter)
            };

            var instanceMethods = currentInterface.Methods.Where(x => !x.IsStatic);

            Debug.Assert(info.Getter != null, "We should always have a getter!");
            Debug.Assert(!(propertyDeclaration.IsReadwrite && info.Setter == null), "We should have a setter!");

            propertyDeclaration.Getter = instanceMethods.Single(c => c.Name == info.Getter.Name);
            if (info.Setter != null)
            {
                propertyDeclaration.Setter = instanceMethods.Single(c => c.Name == info.Setter.Name);
            }

            currentInterface.Properties.Add(propertyDeclaration);

            return propertyDeclaration;
        }

        private IEnumerable<ProtocolDeclaration> GetImplementedProtocols(ClangIndex.DeclarationInfo declaration)
        {
            var protocols = declaration.Cursor.GetChildrenOfKind(CursorKind.ObjCProtocolReference);
            foreach (ClangCursor protocolReferenceCursor in protocols)
            {
                ProtocolDeclaration proto =
                    this.context.GetFromUSRCache<ProtocolDeclaration>(protocolReferenceCursor.Definition.CreateUSR());
                if (proto == null)
                {
                    proto =
                        this.context.GetFromNameCache<ProtocolDeclaration>(protocolReferenceCursor.Definition.Spelling);
                }
                Debug.Assert(proto != null,
                    "Coundn't find protocol declaration neither from its USR, nor from its name. " +
                    "Protocol name: " + protocolReferenceCursor.Definition.Spelling);
                yield return proto;
            }
        }

        private BaseClass GetParentClass(ClangCursor parentContainer)
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(parentContainer.CreateUSR()),
                "We should have USR for the parent container of a class.");
            return this.context.GetFromUSRCache<BaseClass>(parentContainer.CreateUSR());
        }
    }
}
