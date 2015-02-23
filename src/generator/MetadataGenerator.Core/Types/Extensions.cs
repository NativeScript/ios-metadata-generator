using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MetadataGenerator.Core.Ast;
using MetadataGenerator.Core.Common;
using MetadataGenerator.Core.Generator;

namespace MetadataGenerator.Core.Types
{
    public static class Extensions
    {
        public static bool IsPrimitive(this TypeDefinition self)
        {
            TypeDefinition resolvedSelf = self.Resolve();
            if (resolvedSelf is DeclarationReferenceType)
            {
                DeclarationReferenceType declarationRef = resolvedSelf as DeclarationReferenceType;
                return declarationRef.Target is EnumDeclaration;
            }
            return resolvedSelf is PrimitiveType;
        }

        public static bool IsPrimitiveBoolean(this TypeDefinition self)
        {
            // if is bool or _Bools
            return (self is PrimitiveType && (((PrimitiveType) self).Type == PrimitiveTypeType.Bool));
        }

        public static bool IsObjCBOOL(this TypeDefinition self)
        {
            // if is BOOL or Boolean
            if ((self is DeclarationReferenceType) && ((self as DeclarationReferenceType).Target is TypedefDeclaration) &&
                (((self as DeclarationReferenceType).Target as TypedefDeclaration).Name == "BOOL" ||
                 ((self as DeclarationReferenceType).Target as TypedefDeclaration).Name == "Boolean"))
            {
                return true;
            }
            return false;
        }

        public static bool IsUnichar(this TypeDefinition self)
        {
            // if is unichar
            if ((self is DeclarationReferenceType) && ((self as DeclarationReferenceType).Target is TypedefDeclaration) &&
                ((self as DeclarationReferenceType).Target as TypedefDeclaration).Name == "unichar")
            {
                return true;
            }
            return false;
        }

        public static bool IsObjCType(this TypeDefinition self)
        {
            TypeDefinition resolvedSelf = self.Resolve();
            if (resolvedSelf is IdType)
            {
                return true;
            }

            if (resolvedSelf is InstanceType)
            {
                return true;
            }

            if (resolvedSelf is PointerType)
            {
                TypeDefinition resolvedPointerTarget = (resolvedSelf as PointerType).Target;
                return resolvedPointerTarget is DeclarationReferenceType &&
                       (resolvedPointerTarget as DeclarationReferenceType).Target is BaseClass;
            }

            return false;
        }

        public static bool IsTypeDefToPointerToOpaqueStruct(this TypeDefinition self)
        {
            var declReference = self as DeclarationReferenceType;
            if (declReference != null)
            {
                if (declReference.Target is TypedefDeclaration)
                {
                    var typedef = declReference.Target as TypedefDeclaration;
                    if (typedef.OldType is PointerType &&
                        ((PointerType) typedef.OldType).Target is DeclarationReferenceType &&
                        ((DeclarationReferenceType) ((PointerType) typedef.OldType).Target).Target is
                            BaseRecordDeclaration &&
                        ((BaseRecordDeclaration)
                            ((DeclarationReferenceType) ((PointerType) typedef.OldType).Target).Target).IsOpaque)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsCFType(this TypeDefinition self)
        {
            var declReference = self as DeclarationReferenceType;
            if (declReference != null)
            {
                if (declReference.Target is TypedefDeclaration)
                {
                    var typedef = declReference.Target as TypedefDeclaration;
                    if (typedef.Name.EndsWith("Ref") && typedef.OldType is PointerType &&
                        ((PointerType) typedef.OldType).Target is DeclarationReferenceType &&
                        ((DeclarationReferenceType) ((PointerType) typedef.OldType).Target).Target is
                            BaseRecordDeclaration &&
                        ((BaseRecordDeclaration)
                            ((DeclarationReferenceType) ((PointerType) typedef.OldType).Target).Target).IsOpaque)
                    {
                        return true;
                    }
                }
            }

            return self.IsCFTypeRef();
        }

        private static bool IsCFTypeRef(this TypeDefinition self)
        {
            var declReference = self as DeclarationReferenceType;
            if (declReference != null)
            {
                if (declReference.Target is TypedefDeclaration)
                {
                    var typedef = declReference.Target as TypedefDeclaration;
                    if (typedef.Name == "CFTypeRef")
                    {
                        return true;
                    }

                    return typedef.OldType.IsCFTypeRef();
                }
            }

            return false;
        }

        public static InterfaceDeclaration ToObjcType(this TypeDefinition self)
        {
            TypeDefinition resolvedSelf = self.Resolve();

            if ((resolvedSelf is PointerType) && ((resolvedSelf as PointerType).Target is DeclarationReferenceType) &&
                ((resolvedSelf as PointerType).Target as DeclarationReferenceType).Target is InterfaceDeclaration)
            {
                return ((resolvedSelf as PointerType).Target as DeclarationReferenceType).Target as InterfaceDeclaration;
            }

            throw new ArgumentException("Not a objc type: " + self);
        }

        public static bool IsVoid(this TypeDefinition self)
        {
            TypeDefinition resolvedSelf = self.Resolve();
            return resolvedSelf is PrimitiveType && (resolvedSelf as PrimitiveType).Type == PrimitiveTypeType.Void;
        }

        public static FunctionPointerType AsBlock(this TypeDefinition self)
        {
            self = self.Resolve();
            if (!((self is FunctionPointerType) && (self as FunctionPointerType).IsBlock))
            {
                return null;
            }

            return (FunctionPointerType) self;
        }

        public static FunctionPointerType AsFunctionPointer(this TypeDefinition self)
        {
            self = self.Resolve();
            if (!((self is FunctionPointerType) && !(self as FunctionPointerType).IsBlock))
            {
                return null;
            }

            return (FunctionPointerType) self;
        }

        public static TypeDefinition Resolve(this TypeDefinition self)
        {
            if ((self is DeclarationReferenceType) && (self as DeclarationReferenceType).Target is TypedefDeclaration)
            {
                self = ((self as DeclarationReferenceType).Target as TypedefDeclaration).UnderlyingType;
            }
            return self;
        }

        public static PrimitiveType ResolvePrimitive(this TypeDefinition self)
        {
            TypeDefinition resolvedSelf = self.Resolve();
            if (resolvedSelf is PrimitiveType)
            {
                return resolvedSelf as PrimitiveType;
            }

            if (resolvedSelf is DeclarationReferenceType)
            {
                DeclarationReferenceType declarationRef = resolvedSelf as DeclarationReferenceType;
                if (declarationRef.Target is EnumDeclaration)
                {
                    return (declarationRef.Target as EnumDeclaration).UnderlyingType.ResolvePrimitive();
                }
            }

            throw new Exception("Not a primitive type");
        }

        public static TypeDefinition ResolveWithEnums(this TypeDefinition self)
        {
            self = self.Resolve();
            if ((self is DeclarationReferenceType) && ((self as DeclarationReferenceType).Target is EnumDeclaration))
            {
                self = ((self as DeclarationReferenceType).Target as EnumDeclaration).UnderlyingType.Resolve();
            }
            return self;
        }

        public static string ToCTypeString(this PrimitiveTypeType self)
        {
            switch (self)
            {
                case PrimitiveTypeType.Void:
                    return "void";
                case PrimitiveTypeType.Bool:
                    return "_Bool";
                case PrimitiveTypeType.CharU:
                case PrimitiveTypeType.CharS:
                    return "char";
                case PrimitiveTypeType.UChar:
                    return "unsigned char";
                case PrimitiveTypeType.UShort:
                    return "unsigned short";
                case PrimitiveTypeType.UInt:
                    return "unsigned int";
                case PrimitiveTypeType.ULong:
                    return "unsigned long";
                case PrimitiveTypeType.ULongLong:
                    return "unsigned long long";
                    //case PrimitiveTypeType.UInt128:
                    //    return "__uint128_t";
                case PrimitiveTypeType.SChar:
                    return "signed char";
                case PrimitiveTypeType.WChar:
                    return "wchar_t";
                case PrimitiveTypeType.Short:
                    return "short";
                case PrimitiveTypeType.Int:
                    return "int";
                case PrimitiveTypeType.Long:
                    return "long";
                case PrimitiveTypeType.LongLong:
                    return "long long";
                    //case PrimitiveTypeType.Int128:
                    //    return "__int128_t";
                case PrimitiveTypeType.Float:
                    return "float";
                case PrimitiveTypeType.Double:
                    return "double";
                case PrimitiveTypeType.LongDouble:
                    return "long double";
                default:
                    throw new Exception();
            }
        }

        public static string ToJSValueSelectorSuffix(this TypeDefinition self)
        {
            if (self == null)
            {
                throw new ArgumentNullException("self");
            }

            if (self.IsPrimitive())
            {
                switch (self.ResolvePrimitive().Type)
                {
                    case PrimitiveTypeType.Void:
                        throw new InvalidOperationException("Cannot marshall to void");
                    case PrimitiveTypeType.Bool:
                        return "Bool";
                    case PrimitiveTypeType.CharU:
                    case PrimitiveTypeType.UChar:
                    case PrimitiveTypeType.Char16:
                    case PrimitiveTypeType.Char32:
                    case PrimitiveTypeType.UShort:
                    case PrimitiveTypeType.UInt:
                        return "UInt32";
                    case PrimitiveTypeType.WChar:
                    case PrimitiveTypeType.CharS:
                    case PrimitiveTypeType.SChar:
                    case PrimitiveTypeType.Short:
                    case PrimitiveTypeType.Int:
                        return "Int32";

                    case PrimitiveTypeType.Float:
                    case PrimitiveTypeType.Double:
                        return "Double";
                }
            }

            return string.Empty;
        }

        public static string FromJSValueSelectorSuffix(this TypeDefinition self)
        {
            if (self == null)
            {
                throw new ArgumentNullException("self");
            }

            switch (self.ResolvePrimitive().Type)
            {
                case PrimitiveTypeType.Bool:
                    return "Bool";
                case PrimitiveTypeType.CharU:
                case PrimitiveTypeType.UChar:
                case PrimitiveTypeType.Char16:
                case PrimitiveTypeType.Char32:
                case PrimitiveTypeType.UShort:
                case PrimitiveTypeType.UInt:
                case PrimitiveTypeType.ULong:
                case PrimitiveTypeType.ULongLong:
                case PrimitiveTypeType.UInt128:
                    return "UInt32";
                case PrimitiveTypeType.CharS:
                case PrimitiveTypeType.SChar:
                case PrimitiveTypeType.WChar:
                case PrimitiveTypeType.Short:
                case PrimitiveTypeType.Int:
                case PrimitiveTypeType.Long:
                case PrimitiveTypeType.LongLong:
                case PrimitiveTypeType.Int128:
                    return "Int32";
                case PrimitiveTypeType.Float:
                case PrimitiveTypeType.Double:
                case PrimitiveTypeType.LongDouble:
                    return "Double";
            }

            throw new ArgumentException("self");
        }

        public static string GetDefaultValue(this TypeDefinition type)
        {
            string typeString = type is InstanceType ? "id" : type.ToString();
            return string.Format("({0}){1}", typeString, type.GetBaseRecordDeclaration() != null ? "{0}" : "0");
        }

        public static string ToFfiType(this TypeDefinition type)
        {
            if (type.IsPrimitive())
            {
                var primitiveType = type.ResolvePrimitive().Type;

                switch (primitiveType)
                {
                    case PrimitiveTypeType.Void:
                        return "ffi_type_void";
                    case PrimitiveTypeType.Bool:
                        return "ffi_type_uint8";
                    case PrimitiveTypeType.CharU:
                        return "ffi_type_uchar";
                    case PrimitiveTypeType.UChar:
                        return "ffi_type_uchar";
                    case PrimitiveTypeType.Char16:
                        return "ffi_type_uint16";
                    case PrimitiveTypeType.Char32:
                        return "ffi_type_sint32";
                    case PrimitiveTypeType.UShort:
                        return "ffi_type_ushort";
                    case PrimitiveTypeType.UInt:
                        return "ffi_type_uint";
                    case PrimitiveTypeType.ULong:
                        return "ffi_type_ulong";
                    case PrimitiveTypeType.ULongLong:
                        return "ffi_type_uint64";
                    case PrimitiveTypeType.UInt128:
                        throw new ArgumentException("UInt128 ffi is not supported");
                    case PrimitiveTypeType.CharS:
                        return "ffi_type_schar";
                    case PrimitiveTypeType.SChar:
                        return "ffi_type_schar";
                    case PrimitiveTypeType.WChar:
                        return "ffi_type_schar";
                    case PrimitiveTypeType.Short:
                        return "ffi_type_sshort";
                    case PrimitiveTypeType.Int:
                        return "ffi_type_sint";
                    case PrimitiveTypeType.Long:
                        return "ffi_type_slong";
                    case PrimitiveTypeType.LongLong:
                        return "ffi_type_sint64";
                    case PrimitiveTypeType.Int128:
                        throw new ArgumentException("Int128 ffi is not supported");
                    case PrimitiveTypeType.Float:
                        return "ffi_type_float";
                    case PrimitiveTypeType.Double:
                        return "ffi_type_double";
                    case PrimitiveTypeType.LongDouble:
                        return "ffi_type_longdouble";
                    default:
                        throw new ArgumentException("Unknown primitive type");
                }
            }

            TypeDefinition resolved = type.Resolve();
            if (resolved is PointerType || resolved is IncompleteArrayType || resolved is IdType ||
                resolved is SelectorType || resolved is ProtocolType || resolved is ClassType ||
                resolved is InstanceType || resolved is FunctionPointerType)
            {
                return "ffi_type_pointer";
            }

            return "ffi_type_void";
        }

        public static bool IsBigIntType(this TypeDefinition type)
        {
            type = type.Resolve();

            if (!(type is PrimitiveType))
            {
                return false;
            }

            var primitiveType = type as PrimitiveType;

            if (!primitiveType.Type.IsEqualToAny(
                PrimitiveTypeType.Long, PrimitiveTypeType.LongLong,
                PrimitiveTypeType.ULong, PrimitiveTypeType.ULongLong))
            {
                return false;
            }

            return true;
        }

        public static PrimitiveTypeType AsBigIntType(this TypeDefinition type)
        {
            type = type.Resolve();

            if (!(type.IsBigIntType()))
            {
                throw new ArgumentException();
            }

            return (type as PrimitiveType).Type;
        }
    }
}
