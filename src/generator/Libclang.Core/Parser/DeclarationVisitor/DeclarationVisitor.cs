using Libclang.Core.Ast;
using Libclang.Core.Common;
using Libclang.Core.Types;
using NClang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Libclang.Core.Parser
{
    public interface IDeclarationVisitor
    {
        void VisitDeclaration(ClangIndex.DeclarationInfo declaration);
    }

    public abstract class DeclarationVisitor : IDeclarationVisitor
    {
        internal readonly FrameworkParser.ParserContext context;

        private readonly Dictionary<IndexEntityKind, Func<ClangIndex.DeclarationInfo, BaseDeclaration>>
            definitionParserMapper;

        private readonly HashSet<TypeKind> builtinTypes = new HashSet<TypeKind>(new TypeKind[]
        {
            TypeKind.Void, TypeKind.Bool, TypeKind.CharU, TypeKind.UChar, TypeKind.Char16, TypeKind.Char32,
            TypeKind.UShort, TypeKind.UInt, TypeKind.ULong, TypeKind.ULongLong, TypeKind.UInt128, TypeKind.CharS,
            TypeKind.SChar, TypeKind.WChar, TypeKind.Short, TypeKind.Int, TypeKind.Long, TypeKind.LongLong,
            TypeKind.Int128, TypeKind.Float, TypeKind.Double, TypeKind.LongDouble
        });

        public DeclarationVisitor(FrameworkParser.ParserContext context)
        {
            this.context = context;
            this.definitionParserMapper = this.CreateDefinitionParserMapper();
        }

        public void VisitDeclaration(ClangIndex.DeclarationInfo declaration)
        {
            Func<ClangIndex.DeclarationInfo, BaseDeclaration> parser;
            if (this.definitionParserMapper.TryGetValue(declaration.EntityInfo.Kind, out parser))
            {
                ClangPlatformAvailability iosAvailability;
                ClangPlatformAvailability iosAppExtensionAvailability;
                if (!isIOSAvailable(declaration.Cursor, out iosAvailability, out iosAppExtensionAvailability))
                {
                    return;
                }

                BaseDeclaration parsedDeclaration = parser(declaration);
                if (parsedDeclaration != null)
                {
                    this.ResolveUnresolvedReference(parsedDeclaration);
                    parsedDeclaration.Location = declaration.Location;
                    parsedDeclaration.IosAvailability = iosAvailability;
                    parsedDeclaration.IosAppExtensionAvailability = iosAppExtensionAvailability;
                    parsedDeclaration.IsDefinition = declaration.IsDefinition;
                    parsedDeclaration.IsContainer = declaration.IsContainer;
                }
            }
        }

        public virtual TypeDefinition ParseClangType(ClangType type)
        {
            TypeDefinition element = null;

            if (builtinTypes.Contains(type.Kind))
            {
                element = new PrimitiveType((PrimitiveTypeType) type.Kind);
            }
            else if (type.Spelling == "half" || type.Spelling == "__fp16")
            {
                element = new PrimitiveType(PrimitiveTypeType.Float16);
            }
            else if (type.Spelling == "__uint128_t")
            {
                element = new PrimitiveType(PrimitiveTypeType.UInt128);
            }
            else if (type.Kind == TypeKind.Pointer || type.Kind == TypeKind.ObjCObjectPointer ||
                     type.Kind == TypeKind.BlockPointer)
            {
                ClangType pointerType = type.PointeeType;
                Match idMatch = Regex.Match(pointerType.Spelling, @"id<(.+)>$");
                Match classMatch = Regex.Match(pointerType.Spelling, @"Class<(.+)>\s?\*?$");

                if (pointerType.Kind == TypeKind.Unexposed && (idMatch.Success || pointerType.Spelling == "id"))
                {
                    IdType @id = new IdType();
                    if (idMatch.Success)
                    {
                        foreach (string protocolName in idMatch.Groups[1].Value.Split(','))
                        {
                            ProtocolDeclaration _protoDeclaration =
                                this.context.GetFromNameCache<ProtocolDeclaration>(protocolName.Trim());
                            @id.ImplementedProtocols.Add(_protoDeclaration);
                        }
                    }
                    element = @id;
                }
                else if (pointerType.Kind == TypeKind.Unexposed &&
                         (classMatch.Success || pointerType.Spelling.StartsWith("Class")))
                {
                    ClassType @class = new ClassType();
                    if (classMatch.Success)
                    {
                        foreach (string protocolName in classMatch.Groups[1].Value.Split(','))
                        {
                            ProtocolDeclaration _protoDeclaration =
                                this.context.GetFromNameCache<ProtocolDeclaration>(protocolName.Trim());
                            @class.ImplementedProtocols.Add(_protoDeclaration);
                        }
                    }
                    element = @class;
                }
                else if (pointerType.Kind == TypeKind.ObjCSel)
                {
                    element = new SelectorType();
                }
                else if ((pointerType.Kind == TypeKind.Unexposed &&
                          (pointerType.CanonicalType.Kind == TypeKind.FunctionProto ||
                           pointerType.CanonicalType.Kind == TypeKind.FunctionNoProto)))
                {
                    FunctionPointerType @functionType = this.ParseFunctionPointer(pointerType);
                    @functionType.IsBlock = type.Kind == TypeKind.BlockPointer;
                    element = @functionType;
                }
                else
                {
                    element = new PointerType(this.ParseClangType(pointerType));
                }
            }
            else if (type.Kind == TypeKind.ConstantArray)
            {
                element = new ConstantArrayType(type.ArraySize, ParseClangType(type.ArrayElementType));
            }
            else if (type.Kind == TypeKind.IncompleteArray)
            {
                element = new IncompleteArrayType(ParseClangType(type.ArrayElementType));
            }
            else if (type.Kind == TypeKind.Vector)
            {
                element = new VectorType(type.ElementCount, ParseClangType(type.ElementType));
            }
            else if (type.Spelling.Contains("ext_vector_type"))
            {
                element = new VectorType(-1, null);
            }
            else if (type.Kind == TypeKind.Complex)
            {
                element = new ComplexType(ParseClangType(type.ElementType));
            }
            else if (type.Kind == TypeKind.ObjCSel)
            {
                element = new SelectorType();
            }
            else if (type.Kind == TypeKind.ObjCId || type.Spelling == "id" || type.Spelling.EndsWith(" id"))
            {
                element = new IdType();
            }
            else if (type.Kind == TypeKind.ObjCClass || type.Spelling == "Class" || type.Spelling.EndsWith(" Class"))
            {
                element = new ClassType();
            }
            else if (type.Kind == TypeKind.FunctionProto || type.Kind == TypeKind.FunctionNoProto)
            {
                FunctionPointerType @functionType = this.ParseFunctionPointer(type);
                element = @functionType;
            }
            else if (type.TypeDeclaration.Kind != CursorKind.NoDeclarationFound)
            {
                string typeDeclName = type.TypeDeclaration.Spelling;
                if (typeDeclName.IsEqualToAny("__builtin_va_list", "__va_list_tag"))
                {
                    element = new VaListType();
                }
                else if (typeDeclName == "Protocol")
                {
                    element = new ProtocolType();
                }
                else if (typeDeclName == "instancetype")
                {
                    element = new InstanceType();
                }
                else
                {
                    string typeDeclarationUSR = type.TypeDeclaration.CreateUSR();
                    System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(typeDeclarationUSR),
                        "The usr of this type declaration is empty. " +
                        "Type declaration: " + type.TypeDeclaration.Spelling);
                    DeclarationReferenceType declaration = new DeclarationReferenceType(null)
                    {
                        TargetUSR = typeDeclarationUSR
                    };
                    BaseDeclaration target = null;

                    if (!this.context.usrToDeclaration.TryGetValue(typeDeclarationUSR, out target))
                    {
                        string typeDeclarationName = type.TypeDeclaration.Spelling;
                        if (!string.IsNullOrEmpty(typeDeclarationName))
                        {
                            target = this.context.GetFromNameCache(typeDeclarationName);
                        }
                    }
                    if (target == null)
                    {
                        IList<DeclarationReferenceType> unresolvedReference;
                        if (this.context.usrToUnresolvedReferences.TryGetValue(typeDeclarationUSR,
                            out unresolvedReference))
                        {
                            unresolvedReference.Add(declaration);
                        }
                        else
                        {
                            this.context.usrToUnresolvedReferences.Add(typeDeclarationUSR,
                                new List<DeclarationReferenceType>() {declaration});
                        }
                    }
                    declaration.Target = target;

                    element = declaration;
                }
            }
            else
            {
                throw new Exception(string.Format("Invalid type: {0} {1}", type.Kind, type.Spelling));
            }

            System.Diagnostics.Debug.Assert(element != null, "We should have parsed the type by now");

            element.IsConst = type.IsConstQualifiedType;
            element.IsVolatile = type.IsVolatileQualifiedType;
            element.IsRestrict = type.IsRestrictQualifiedType;

            return element;
        }

        protected virtual void AddDeclaration(BaseDeclaration declaration, bool addToUSRCache = true,
            bool addToNameCache = true)
        {
            if (declaration.Module != null)
            {
                ModuleDeclaration module = declaration.Module;
                module.Add(declaration);
            }
            if (addToNameCache)
            {
                this.context.AddToNameCache(declaration);
            }
            if (addToUSRCache)
            {
                this.context.AddToUSRCache(declaration);
            }
        }

        protected virtual void ResolveUnresolvedReference(BaseDeclaration declaration)
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
            }
        }

        protected abstract Dictionary<IndexEntityKind, Func<ClangIndex.DeclarationInfo, BaseDeclaration>>
            CreateDefinitionParserMapper();

        protected IEnumerable<ParameterDeclaration> ParseParameters(ClangCursor cursor)
        {
            for (int i = 0; i < cursor.ArgumentCount; i++)
            {
                ClangCursor argument = cursor.GetArgument(i);
                ObjCDeclarationQualifierFlags qualifiers = argument.ObjCDeclQualifiers;
                ParameterDeclaration paramDecl = new ParameterDeclaration(argument.DisplayName,
                    ParseClangType(argument.CursorType))
                {
                    IsBycopy = qualifiers.HasFlag(ObjCDeclarationQualifierFlags.Bycopy),
                    IsByref = qualifiers.HasFlag(ObjCDeclarationQualifierFlags.Byref),
                    IsIn = qualifiers.HasFlag(ObjCDeclarationQualifierFlags.In),
                    IsInout = qualifiers.HasFlag(ObjCDeclarationQualifierFlags.Inout),
                    IsOneway = qualifiers.HasFlag(ObjCDeclarationQualifierFlags.Oneway),
                    IsOut = qualifiers.HasFlag(ObjCDeclarationQualifierFlags.Out)
                };

                yield return paramDecl;
            }
        }

        private FunctionPointerType ParseFunctionPointer(ClangType type)
        {
            FunctionPointerType @functionType = new FunctionPointerType(this.ParseClangType(type.ResultType),
                this.context.GetAnonymousTypeID());
            @functionType.IsVariadic = type.IsFunctionTypeVariadic;

            for (int i = 0; i < type.ArgumentTypeCount; i++)
            {
                ClangType argumentType = type.GetArgumentType(i);
                @functionType.Parameters.Add(new ParameterDeclaration(string.Empty, this.ParseClangType(argumentType)));
            }

            return @functionType;
        }

        public static bool isIOSAvailable(ClangCursor cursor, out ClangPlatformAvailability iosAvailability, out ClangPlatformAvailability iosAppExtensionAvailability)
        {
            bool always_deprecated = false;
            string deprecated_message = string.Empty;
            bool always_unavailable = false;
            string unavailable_message = string.Empty;
            ClangPlatformAvailability[] platformAvailability = cursor.GetPlatformAvailability(out always_deprecated,
                out deprecated_message, out always_unavailable, out unavailable_message);
            
            iosAvailability = platformAvailability.SingleOrDefault(c => c.Platform.Equals("ios"));
            iosAppExtensionAvailability = platformAvailability.SingleOrDefault(c => c.Platform.Equals("ios_app_extension"));

            bool iosUnavailable = iosAvailability != null && iosAvailability.IsUnavailable;
            return !always_unavailable && !iosUnavailable;
        }

        public static bool[] HasStringsInFileNearCursor(ClangCursor cursor, params string[] values)
        {
            int startLine = cursor.CursorExtent.Start.FileLocation.Line - 1;
            int endLine = cursor.CursorExtent.End.FileLocation.Line;
            var lines =
                System.IO.File.ReadLines(cursor.Location.FileLocation.File.FileName)
                    .Skip(startLine)
                    .Take(endLine - startLine)
                    .ToArray();

            return values.Select(v => lines.Any(l => l.Contains(v))).ToArray();
        }
    }
}
