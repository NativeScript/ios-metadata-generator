using Libclang.Core.Ast;
using Libclang.Core.Parser;
using NClang;
using System;

namespace Libclang.Tests
{
    public static class LibclangHelper
    {
        private static string[] clangArgs = new[]
        {
            "-v",
            "-x", "objective-c",
            "-arch", "armv7",
            "-target", "arm-apple-darwin14.0.0",
            "-std=gnu99",
            "-miphoneos-version-min=7.0"
        };

        public static void ParseCodeWithVisitor(string code, params IDeclarationVisitor[] visitors)
        {
            string tempFile = System.IO.Path.GetTempFileName();
            System.IO.File.WriteAllText(tempFile, code);
            try
            {
                ParseFileWithVisitor(tempFile, visitors);
            }
            finally
            {
                System.IO.File.Delete(tempFile);
            }
        }

        public static void ParseFileWithVisitor(string filepath, params IDeclarationVisitor[] visitors)
        {
            using (ClangIndex index = ClangService.CreateIndex())
            using (
                ClangTranslationUnit translationUnit = index.ParseTranslationUnit(filepath, clangArgs,
                    new ClangUnsavedFile[0], TranslationUnitFlags.SkipFunctionBodies))
            {
                foreach (var diagnostic in translationUnit.DiagnosticSet.Items)
                {
                    string message =
                        diagnostic.Format(DiagnosticDisplayOptions.DisplayOption |
                                          DiagnosticDisplayOptions.DisplaySourceLocation);
                    Console.WriteLine(message);
                }

                using (var indexAction = index.CreateIndexAction())
                {
                    var callback = new ClangIndexerCallbacks();

                    foreach (var visitor in visitors)
                    {
                        callback.IndexDeclaration +=
                            (IntPtr clientData, ClangIndex.DeclarationInfo declaration) =>
                                visitor.VisitDeclaration(declaration);
                    }

                    indexAction.IndexTranslationUnit(IntPtr.Zero, new[] {callback}, IndexOptionFlags.None,
                        translationUnit);
                }
            }
        }
    }
}
