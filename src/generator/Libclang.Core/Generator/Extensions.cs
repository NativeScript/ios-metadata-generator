using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Libclang.Core.Ast;
using Libclang.Core.Common;
using Libclang.Core.Types;
using Libclang.Core.Meta.Utils;

namespace Libclang.Core.Generator
{
    public static class Extensions
    {
        public static void AppendLine(this StringBuilder sb, string format, params object[] args)
        {
            sb.AppendFormat(format, args).AppendLine();
        }

        public static void Clear(this DirectoryInfo directoryInfo)
        {
            if (!directoryInfo.Exists)
            {
                return;
            }

            foreach (var file in directoryInfo.EnumerateFiles())
            {
                file.Delete();
            }
            foreach (var dir in directoryInfo.EnumerateDirectories())
            {
                dir.Delete(recursive: true);
            }
        }

        public static string ModuleNameOrNull(this BaseDeclaration declaration)
        {
            return (declaration.Module == null) ? null : declaration.Module.Name;
        }

        public static bool IsAnonymousWithoutTypedef(this BaseRecordDeclaration record)
        {
            return record.IsAnonymous && string.IsNullOrEmpty(record.TypedefName);
        }

        public static bool IsAnonymousWithTypedef(this BaseRecordDeclaration record)
        {
            return record.IsAnonymous && !string.IsNullOrEmpty(record.TypedefName);
        }

        public static bool IsAnonymousWithoutTypedef(this EnumDeclaration record)
        {
            return record.IsAnonymous && string.IsNullOrEmpty(record.TypedefName);
        }

        public static bool IsAnonymousWithTypedef(this EnumDeclaration record)
        {
            return record.IsAnonymous && !string.IsNullOrEmpty(record.TypedefName);
        }

        public static string GetNameOrTypedefName(this EnumDeclaration @enum)
        {
            return @enum.IsAnonymous ? @enum.TypedefName : @enum.Name;
        }

        public static BaseRecordDeclaration GetBaseRecordDeclaration(this TypeDefinition type)
        {
            type = type.Resolve();

            if ((type is DeclarationReferenceType) &&
                ((type as DeclarationReferenceType).Target is BaseRecordDeclaration))
            {
                var declaration = ((type as DeclarationReferenceType).Target as BaseRecordDeclaration);

                if (declaration.IsOpaque)
                {
                    return null;
                }

                return declaration;
            }

            return null;
        }

        public static BaseRecordDeclaration GetPointerToBaseRecordDeclaration(this TypeDefinition type)
        {
            type = type.Resolve();

            if (!(type is PointerType))
            {
                return null;
            }

            type = (type as PointerType).Target;
            return type.GetBaseRecordDeclaration();
        }

        public static bool IsSelectorType(this TypeDefinition type)
        {
            return type.Resolve() is SelectorType;
        }

        public static bool IsClassType(this TypeDefinition type)
        {
            return type.Resolve() is ClassType;
        }

        public static bool IsProtocolType(this TypeDefinition type)
        {
            type = type.Resolve();

            if (!(type is PointerType))
            {
                return false;
            }

            type = (type as PointerType).Target;
            return type.Resolve() is ProtocolType;
        }

        public static PrimitiveTypeType? AsPrimitivePointerType(this TypeDefinition type)
        {
            type = type.ResolveWithEnums();

            if (type is IncompleteArrayType)
            {
                while (type is IncompleteArrayType)
                {
                    type = (type as IncompleteArrayType).ElementType;
                }

                if (type is PrimitiveType)
                {
                    return (type as PrimitiveType).Type;
                }

                return null;
            }

            if (!(type is PointerType))
            {
                return null;
            }

            while (type is PointerType)
            {
                type = (type as PointerType).Target;
            }

            if (!(type is PrimitiveType))
            {
                return null;
            }

            return (type as PrimitiveType).Type;
        }

        public static string GetPointerName(this TypeDefinition type)
        {
            if (type.AsPrimitivePointerType() == null)
            {
                throw new ArgumentException("Type is not a pointer");
            }

            var result = new StringBuilder(type.AsPrimitivePointerType().Value.ToCTypeString());

            type = type.Resolve();

            while (type is IncompleteArrayType || type is PointerType)
            {
                if (type is IncompleteArrayType)
                {
                    type = (type as IncompleteArrayType).ElementType;
                }

                if (type is PointerType)
                {
                    type = (type as PointerType).Target;
                }

                result.Append("*");
            }

            if (result[result.Length - 1] == '*')
            {
                result.Length--;
            }

            return result.ToString();
        }

        public static TypeEncoding GetReturnTypeEncoding(this IFunction function)
        {
            if (function is MethodDeclaration)
            {
                MethodDeclaration method = (MethodDeclaration)function;
                if (method.HasRelatedResultType && !(method.ReturnType is InstanceType))
                {
                    return TypeEncoding.Instancetype;
                }
                if ((method.Name.StartsWith("string") || (method.Parent.Name == "NSNull" && method.Name == "null")) && method.IsStatic)
                {
                    return TypeEncoding.Instancetype;
                }
            }

            return function.ReturnType.ToTypeEncoding();
        }

        public static IEnumerable<TypeEncoding> GetParamTypeEncoding(this IFunction function)
        {
            return function.Parameters.Select(p => p.Type.ToTypeEncoding());
        }

        public static bool IsValidFunction(this FunctionDeclaration function)
        {
            // TODO
            if (function.Name.IsEqualToAny(
                // Not available with ARC
                "dispatch_release", "dispatch_retain", "CFMakeCollectable", "NSAllocateObject", "NSCopyObject",
                "NSDeallocateObject", "NSDecrementExtraRefCountWasZero", "NSExtraRefCount", "NSIncrementExtraRefCount",
                "NSMakeCollectable", "NSShouldRetainWithZone", "object_getIndexedIvars",
                // Null argument
                "sigaddset", "sigaltstack", "sigdelset", "sigemptyset", "sigfillset", "sigismember",
                // Can not synthesize block
                "add_profil", "unwhiteout", "profil", "pfctlinput", "zopen", "longjmperror",
                "__darwin_fd_isset",

                // Missing in dylib
                "sqlite3_column_database_name", "sqlite3_column_database_name16", "sqlite3_column_origin_name",
                "sqlite3_column_origin_name16", "sqlite3_column_table_name", "sqlite3_column_table_name16",
                "sqlite3_enable_load_extension", "sqlite3_load_extension", "sqlite3_mutex_held", "sqlite3_mutex_notheld",
                "sqlite3_table_column_metadata", "sqlite3_unlock_notify",
                "CFBundleCloseBundleResourceMap", "CFBundleOpenBundleResourceFiles", "CFBundleOpenBundleResourceMap",
                "CFURLCreateFromFSRef", "CFURLGetFSRef", "acl_valid_link_np"
                ))
            {
                return false;
            }

            IEnumerable<TypeDefinition> types =
                function.Parameters.Select(x => x.Type).Concat(new[] {function.ReturnType});

            if (types.Any(x =>
            {
                TypeDefinition type = x.ResolveWithEnums();

                bool decl = (type is DeclarationReferenceType) &&
                            ((type as DeclarationReferenceType).Target is BaseRecordDeclaration &&
                             ((type as DeclarationReferenceType).Target as BaseRecordDeclaration).IsOpaque);

                return decl;
            }))
            {
                return false;
            }

            if (types.Any(x => (x.Resolve() is VectorType)))
            {
                return false;
            }

            return true;
        }

        public static IEnumerable<MethodDeclaration> InstanceMethods(this BaseClass declaration)
        {
            return declaration.Methods.Where(m => !m.IsStatic);
        }

        public static IEnumerable<MethodDeclaration> StaticMethods(this BaseClass declaration)
        {
            return declaration.Methods.Where(m => m.IsStatic);
        }
    }
}
