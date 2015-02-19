using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Libclang.Core.Ast;

namespace Libclang
{
    internal class Statistics
    {
        private const string RootPath = "/Users/jzhekov/Desktop/Trash/IOSHEADERS/Frameworks/";

        public static void PrintStatistics(IEnumerable<ModuleDeclaration> frameworks)
        {
            var headers = new[]
            {
                "Name", "Functions", "Structures", "Unions", "Enums", "Protocols", "Interfaces", "Categories", "Methods",
                "Variadics"
            };
            var row = string.Join("\t", headers.Select((x, i) => "{" + i + "}"));

            Console.WriteLine(string.Join("\t", headers));

            var total = new int[headers.Length - 1];

            foreach (var framework in frameworks)
            {
                var filtered = framework.Declarations.Where(x =>
                    x.Location.Filename.StartsWith(RootPath + framework.Name)).ToList();

                var methods = Enumerable.Concat(
                    filtered.OfType<InterfaceDeclaration>().SelectMany(x => x.Methods),
                    filtered.OfType<CategoryDeclaration>().SelectMany(x => x.Methods));

                var current = new[]
                {
                    filtered.OfType<FunctionDeclaration>().Count(),
                    filtered.OfType<StructDeclaration>().Count(),
                    filtered.OfType<UnionDeclaration>().Count(),
                    filtered.OfType<EnumDeclaration>().Count(),
                    filtered.OfType<ProtocolDeclaration>().Count(),
                    filtered.OfType<InterfaceDeclaration>().Count(),
                    filtered.OfType<CategoryDeclaration>().Count(),
                    methods.Count(),
                    filtered.OfType<FunctionDeclaration>().Count(x => x.IsVariadic) + methods.Count(x => x.IsVariadic),
                };

                for (int i = 0; i < current.Length; i++)
                {
                    total[i] += current[i];
                }

                Console.WriteLine(row,
                    Enumerable.Concat(new[] {framework.Name}, current.Select(x => x.ToString())).ToArray());
            }

            Console.WriteLine(row, Enumerable.Concat(new[] {"TOTAL"}, total.Select(x => x.ToString())).ToArray());
        }
    }
}
