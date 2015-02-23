using NClang;
using System;
using System.Diagnostics;

namespace MetadataGenerator.Core.Parser
{
    internal static class CustomUSR
    {
        public static string CreateUSR(this ClangCursor cursor)
        {
            ClangCursor canonical = cursor.CanonicalCursor;
            if ((canonical.Kind == CursorKind.StructDeclaration || canonical.Kind == CursorKind.UnionDeclaration ||
                 canonical.Kind == CursorKind.EnumDeclaration) && string.IsNullOrEmpty(canonical.Spelling))
            {
                return canonical.GetHashCode().ToString();
            }

            return cursor.UnifiedSymbolResolution;
        }

        public static string CreateUSR(string usr, ClangSourceLocation.PhysicalLocation location)
        {
            Debug.Assert(!string.IsNullOrEmpty(usr), "Should have usr!");

            int hash = location.File.FileName.GetHashCode() +
                       location.Line + location.Column;

            return usr + hash.ToString();
        }
    }
}
