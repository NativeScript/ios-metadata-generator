using Libclang.Core.Ast;
using Libclang.Core.Types;
using NClang;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Libclang.Core.Parser
{
    public class CDeclarationVisitor : DeclarationVisitor
    {
        // unresolved type usr -> typedef usr
        private readonly IDictionary<string, string> unresolvedTypeDefs = new Dictionary<string, string>();

        public CDeclarationVisitor(DocumentDeclaration document)
            : base(new FrameworkParser.ParserContext(document))
        {
        }

        public CDeclarationVisitor(FrameworkParser.ParserContext context)
            : base(context)
        {
        }

        protected override Dictionary<IndexEntityKind, Func<ClangIndex.DeclarationInfo, BaseDeclaration>>
            CreateDefinitionParserMapper()
        {
            var definitionParserMapper =
                new Dictionary<IndexEntityKind, Func<ClangIndex.DeclarationInfo, BaseDeclaration>>();
            definitionParserMapper.Add(IndexEntityKind.Function, this.VisitFunction);
            definitionParserMapper.Add(IndexEntityKind.Variable, this.VisitVariable);
            definitionParserMapper.Add(IndexEntityKind.Typedef, this.VisitTypedef);
            definitionParserMapper.Add(IndexEntityKind.Enum, this.VisitEnum);
            definitionParserMapper.Add(IndexEntityKind.EnumConstant, this.VisitEnumConstant);
            definitionParserMapper.Add(IndexEntityKind.Struct, this.VisitStruct);
            definitionParserMapper.Add(IndexEntityKind.Union, this.VisitUnion);
            definitionParserMapper.Add(IndexEntityKind.Field, this.VisitField);
            return definitionParserMapper;
        }

        protected override void ResolveUnresolvedReference(BaseDeclaration declaration)
        {
            if (string.IsNullOrEmpty(declaration.USR))
            {
                return;
            }

            IList<DeclarationReferenceType> unresolvedTypes;
            if (this.context.usrToUnresolvedReferences.TryGetValue(declaration.USR, out unresolvedTypes))
            {
                foreach (DeclarationReferenceType unresolvedType in unresolvedTypes)
                {
                    unresolvedType.Target = declaration;
                }
                this.context.usrToUnresolvedReferences.Remove(declaration.USR);

                string unresolvedTypedefUSR;
                if (this.unresolvedTypeDefs.TryGetValue(declaration.USR, out unresolvedTypedefUSR))
                {
                    TypedefDeclaration typedef = this.context.GetFromUSRCache<TypedefDeclaration>(unresolvedTypedefUSR);

                    if (declaration is BaseRecordDeclaration)
                    {
                        (declaration as BaseRecordDeclaration).TypedefName = typedef.Name;
                    }
                    else if (declaration is EnumDeclaration)
                    {
                        (declaration as EnumDeclaration).TypedefName = typedef.Name;
                    }
                }
            }
        }

        protected virtual BaseDeclaration VisitFunction(ClangIndex.DeclarationInfo declaration)
        {
            FunctionDeclaration @function =
                this.context.GetFromUSRCache<FunctionDeclaration>(declaration.Cursor.CreateUSR());
            if (@function == null)
            {
                @function = new FunctionDeclaration(declaration.EntityInfo.Name,
                    ParseClangType(declaration.Cursor.ResultType))
                {
                    IsVariadic = declaration.Cursor.IsVariadic,
                    USR = declaration.Cursor.CreateUSR(),
                    Location = declaration.Location
                };

                // add the arguments
                foreach (ParameterDeclaration paramDecl in this.ParseParameters(declaration.Cursor))
                {
                    @function.AddParameter(paramDecl);
                }

                ClangCursor parentContainer = declaration.SemanticContainer.Cursor;
                System.Diagnostics.Debug.Assert(parentContainer.Kind == CursorKind.TranslationUnit,
                    "The parent container of a function is not the TranslationUnit. " +
                    "Actual Type: " + parentContainer.Kind);
                this.AddDeclaration(@function);
            }

            if (!declaration.IsDefinition)
            {
                this.ResolveUnresolvedReference(@function);
                return null;
            }

            return @function;
        }

        protected virtual BaseDeclaration VisitVariable(ClangIndex.DeclarationInfo declaration)
        {
            if (declaration.IsRedeclaration)
            {
                return null;
            }

            VarDeclaration @var = new VarDeclaration(declaration.EntityInfo.Name,
                this.ParseClangType(declaration.Cursor.CursorType))
            {
                USR = declaration.Cursor.CreateUSR(),
                Location = declaration.Location
            };

            ClangCursor parentContainer = declaration.SemanticContainer.Cursor;
            System.Diagnostics.Debug.Assert(parentContainer.Kind == CursorKind.TranslationUnit,
                "The parent container of a variable is not the TranslationUnit. " +
                "Actual Type: " + parentContainer.Kind);
            this.AddDeclaration(@var);
            return @var;
        }

        protected virtual BaseDeclaration VisitTypedef(ClangIndex.DeclarationInfo declaration)
        {
            System.Diagnostics.Debug.Assert(declaration.IsDefinition,
                "Found forward declared typedef " + declaration.EntityInfo.Name);

            ClangType typeDefUnderlyingType = declaration.Cursor.TypeDefDeclUnderlyingType;
            TypeDefinition parsedType = ParseClangType(typeDefUnderlyingType);

            TypedefDeclaration typedefDeclaration = new TypedefDeclaration(declaration.EntityInfo.Name, parsedType)
            {
                USR = declaration.Cursor.CreateUSR(),
                Location = declaration.Location
            };

            DeclarationReferenceType declRefType = parsedType as DeclarationReferenceType;
            if (declRefType != null && declRefType.Target == null)
            {
                unresolvedTypeDefs.Add(declRefType.TargetUSR, typedefDeclaration.USR);
            }

            ClangCursor parentContainer = declaration.SemanticContainer.Cursor;
            System.Diagnostics.Debug.Assert(parentContainer.Kind == CursorKind.TranslationUnit,
                "The parent container of a typedef is not the TranslationUnit. " +
                "Actual Type: " + parentContainer.Kind);
            this.AddDeclaration(typedefDeclaration);

            return typedefDeclaration;
        }

        protected virtual BaseDeclaration VisitEnum(ClangIndex.DeclarationInfo declaration)
        {
            EnumDeclaration @enum = this.context.GetFromUSRCache<EnumDeclaration>(declaration.Cursor.CreateUSR());
            if (@enum == null)
            {
                var enumType = ParseClangType(declaration.Cursor.EnumDeclIntegerType);
                @enum = new EnumDeclaration(
                    declaration.EntityInfo.Name ?? this.context.GetAnonymousTypeID().ToString(), enumType)
                {
                    USR = declaration.Cursor.CreateUSR(),
                    Location = declaration.Location
                };
                this.context.AddToUSRCache(@enum);
            }

            if (!declaration.IsDefinition || @enum.Document != null)
            {
                this.ResolveUnresolvedReference(@enum);
                return null;
            }

            @enum.Location = declaration.Location;
            ClangCursor parentContainer = declaration.SemanticContainer.Cursor;
            if (parentContainer.Kind == CursorKind.TranslationUnit)
            {
                this.AddDeclaration(@enum, false);
            }
            else
            {
                AddDeclarationToParent(@enum, parentContainer);
            }
            return @enum;
        }

        protected virtual BaseDeclaration VisitEnumConstant(ClangIndex.DeclarationInfo declaration)
        {
            string parentUSR = declaration.SemanticContainer.Cursor.CreateUSR();
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(parentUSR),
                "The parentUSR of this enum constant is empty. " +
                "Enum memmber name: " + declaration.EntityInfo.Name);
            EnumDeclaration @enum = this.context.GetFromUSRCache<EnumDeclaration>(parentUSR);

            string fieldUSR = declaration.Cursor.CreateUSR();
            if (@enum.Fields.Any(c => c.USR == fieldUSR))
            {
                return null;
            }

            EnumMemberDeclaration @enumMember = new EnumMemberDeclaration(declaration.EntityInfo.Name,
                declaration.Cursor.EnumConstantDeclValue, @enum)
            {
                USR = fieldUSR
            };
            @enum.Fields.Add(@enumMember);
            return @enumMember;
        }

        protected virtual BaseDeclaration VisitStruct(ClangIndex.DeclarationInfo declaration)
        {
            StructDeclaration @struct = this.context.GetFromUSRCache<StructDeclaration>(declaration.Cursor.CreateUSR());
            if (@struct == null)
            {
                @struct =
                    new StructDeclaration(declaration.EntityInfo.Name ?? this.context.GetAnonymousTypeID().ToString())
                    {
                        USR = declaration.Cursor.CreateUSR(),
                        Location = declaration.Location
                    };
                this.context.AddToUSRCache(@struct);
            }

            if (!declaration.IsDefinition || @struct.Document != null)
            {
                this.ResolveUnresolvedReference(@struct);
                return null;
            }

            @struct.Location = declaration.Location;
            ClangCursor parentContainer = declaration.SemanticContainer.Cursor;
            if (parentContainer.Kind == CursorKind.TranslationUnit)
            {
                this.AddDeclaration(@struct, false);
            }
            else
            {
                AddDeclarationToParent(@struct, parentContainer);
            }
            return @struct;
        }

        protected virtual BaseDeclaration VisitUnion(ClangIndex.DeclarationInfo declaration)
        {
            UnionDeclaration @union = this.context.GetFromUSRCache<UnionDeclaration>(declaration.Cursor.CreateUSR());
            if (@union == null)
            {
                @union =
                    new UnionDeclaration(declaration.EntityInfo.Name ?? this.context.GetAnonymousTypeID().ToString())
                    {
                        USR = declaration.Cursor.CreateUSR(),
                        Location = declaration.Location
                    };
                this.context.AddToUSRCache(@union);
            }

            if (!declaration.IsDefinition || @union.Document != null)
            {
                this.ResolveUnresolvedReference(@union);
                return null;
            }

            @union.Location = declaration.Location;
            ClangCursor parentContainer = declaration.SemanticContainer.Cursor;
            if (parentContainer.Kind == CursorKind.TranslationUnit)
            {
                this.AddDeclaration(@union, false);
            }
            else
            {
                AddDeclarationToParent(@union, parentContainer);
            }
            return @union;
        }

        protected virtual BaseDeclaration VisitField(ClangIndex.DeclarationInfo declaration)
        {
            string parentUSR = declaration.SemanticContainer.Cursor.CreateUSR();
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(parentUSR), "The parentUSR of this field is empty. " +
                                                                              "Field name: " +
                                                                              declaration.EntityInfo.Name);
            BaseRecordDeclaration currentRecord = this.context.GetFromUSRCache<BaseRecordDeclaration>(parentUSR);

            string fieldUSR = declaration.Cursor.CreateUSR();
            if (currentRecord.Fields.Any(c => c.USR == fieldUSR))
            {
                return null;
            }

            FieldDeclaration @field = new FieldDeclaration(declaration.EntityInfo.Name,
                ParseClangType(declaration.Cursor.CursorType))
            {
                USR = fieldUSR
            };
            currentRecord.Fields.Add(@field);
            return @field;
        }

        private void AddDeclarationToParent(BaseDeclaration declaration, ClangCursor parentContainer)
        {
            if (parentContainer.Kind == CursorKind.StructDeclaration ||
                parentContainer.Kind == CursorKind.UnionDeclaration)
            {
                System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(parentContainer.CreateUSR()),
                    "The parentUSR of this declaration is empty. " +
                    "Declaration name: " + declaration.Name);
                BaseRecordDeclaration parentRecord =
                    this.context.GetFromUSRCache<BaseRecordDeclaration>(parentContainer.CreateUSR());
                FieldDeclaration lastField = parentRecord.Fields.LastOrDefault();
                if (lastField == null || (lastField.Type is DeclarationReferenceType
                                          && (lastField.Type as DeclarationReferenceType).TargetUSR != declaration.USR))
                {
                    parentRecord.Fields.Add(new FieldDeclaration("", new DeclarationReferenceType(declaration)));
                }
            }
            else
            {
                System.Diagnostics.Debug.Fail("Failed to find the parent of this declaration: " + declaration.Name);
            }
        }
    }
}
