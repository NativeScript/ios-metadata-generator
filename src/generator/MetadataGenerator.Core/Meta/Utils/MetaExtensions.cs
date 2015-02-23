using MetadataGenerator.Core.Ast;
using System;
using System.Collections.Generic;

namespace MetadataGenerator.Core.Meta.Utils
{
    public static class MetaExtensions
    {
        private static readonly string jsNameMetaKey = "JSNameMeta";
        private static readonly string extendedEncodingMetaKey = "ExtendedEncoding";
        private static readonly string isLocalJsNameDuplicateMetaKey = "IsLocalJsNameDuplicate";
        private static readonly string hasJsNameDuplicateInHierarchyMetaKey = "HasJsNameDuplicateInHierarchy";
        private static readonly string valueMetaKey = "Value";

        // JSName

        public static string GetJSName(this BaseDeclaration declaration)
        {
            return GetMeta(declaration, jsNameMetaKey) as string;
        }

        public static void SetJSName(this BaseDeclaration declaration, string jsname)
        {
            SetMeta(declaration, jsNameMetaKey, jsname);
        }

        // Extended encoding

        public static IEnumerable<TypeEncoding> GetExtendedEncoding(this BaseRecordDeclaration declaration)
        {
            return GetMeta(declaration, extendedEncodingMetaKey) as IEnumerable<TypeEncoding>;
        }

        public static void SetExtendedEncoding(this BaseRecordDeclaration declaration, IEnumerable<TypeEncoding> extendedEncoding)
        {
            SetMeta(declaration, extendedEncodingMetaKey, extendedEncoding);
        }

        public static IEnumerable<TypeEncoding> GetExtendedEncoding(this FunctionDeclaration declaration)
        {
            return GetMeta(declaration, extendedEncodingMetaKey) as IEnumerable<TypeEncoding>;
        }

        public static void SetExtendedEncoding(this FunctionDeclaration declaration, IEnumerable<TypeEncoding> extendedEncoding)
        {
            SetMeta(declaration, extendedEncodingMetaKey, extendedEncoding);
        }

        public static TypeEncoding GetExtendedEncoding(this PropertyDeclaration declaration)
        {
            return GetMeta(declaration, extendedEncodingMetaKey) as TypeEncoding;
        }

        public static void SetExtendedEncoding(this PropertyDeclaration declaration, TypeEncoding extendedEncoding)
        {
            SetMeta(declaration, extendedEncodingMetaKey, extendedEncoding);
        }

        public static TypeEncoding GetExtendedEncoding(this VarDeclaration declaration)
        {
            return GetMeta(declaration, extendedEncodingMetaKey) as TypeEncoding;
        }

        public static void SetExtendedEncoding(this VarDeclaration declaration, TypeEncoding extendedEncoding)
        {
            SetMeta(declaration, extendedEncodingMetaKey, extendedEncoding);
        }

        public static TypeEncoding GetExtendedEncoding(this FieldDeclaration declaration)
        {
            return GetMeta(declaration, extendedEncodingMetaKey) as TypeEncoding;
        }

        public static void SetExtendedEncoding(this FieldDeclaration declaration, TypeEncoding extendedEncoding)
        {
            SetMeta(declaration, extendedEncodingMetaKey, extendedEncoding);
        }

        // IsLocalJsNameDuplicate

        public static bool? GetIsLocalJsNameDuplicate(this MethodDeclaration declaration)
        {
            return GetMeta(declaration, isLocalJsNameDuplicateMetaKey) as bool?;
        }

        public static void SetIsLocalJsNameDuplicate(this MethodDeclaration declaration, bool? isLocalJsNameDuplicate)
        {
            SetMeta(declaration, isLocalJsNameDuplicateMetaKey, isLocalJsNameDuplicate);
        }

        public static bool? GetIsLocalJsNameDuplicate(this PropertyDeclaration declaration)
        {
            return GetMeta(declaration, isLocalJsNameDuplicateMetaKey) as bool?;
        }

        public static void SetIsLocalJsNameDuplicate(this PropertyDeclaration declaration, bool? isLocalJsNameDuplicate)
        {
            SetMeta(declaration, isLocalJsNameDuplicateMetaKey, isLocalJsNameDuplicate);
        }

        // HasJsNameDuplicateInHierarchy

        public static bool? GetHasJsNameDuplicateInHierarchy(this MethodDeclaration declaration)
        {
            return GetMeta(declaration, hasJsNameDuplicateInHierarchyMetaKey) as bool?;
        }

        public static void SetHasJsNameDuplicateInHierarchy(this MethodDeclaration declaration, bool? hasJsNameDuplicateInHierarchy)
        {
            SetMeta(declaration, hasJsNameDuplicateInHierarchyMetaKey, hasJsNameDuplicateInHierarchy);
        }

        public static bool? GetHasJsNameDuplicateInHierarchy(this PropertyDeclaration declaration)
        {
            return GetMeta(declaration, hasJsNameDuplicateInHierarchyMetaKey) as bool?;
        }

        public static void SetHasJsNameDuplicateInHierarchy(this PropertyDeclaration declaration, bool? hasJsNameDuplicateInHierarchy)
        {
            SetMeta(declaration, hasJsNameDuplicateInHierarchyMetaKey, hasJsNameDuplicateInHierarchy);
        }

        private static object GetMeta(BaseDeclaration declaration, string key)
        {
            object obj;
            declaration.MetadataPropertyBag.TryGetValue(key, out obj);
            return obj;
        }

        // Value

        public static object GetValue(this VarDeclaration declaration)
        {
            return GetMeta(declaration, valueMetaKey);
        }

        public static void SetValue(this VarDeclaration declaration, object value)
        {
            SetMeta(declaration, valueMetaKey, value);
        }

        private static void SetMeta(BaseDeclaration declaration, string key, object value)
        {
            var bag = declaration.MetadataPropertyBag;
            if (!bag.ContainsKey(key))
            {
                bag.Add(key, value);
            }
            else
            {
                bag[key] = value;
            }
        }
    }
}
