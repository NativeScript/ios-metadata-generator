using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MetadataGenerator.Core.Ast;
using MetadataGenerator.Core.Common;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace MetadataGenerator.DocsetParser
{
    public static class DocsetParser
    {
        private static readonly Dictionary<string, Type> typeDict = new Dictionary<string, Type>()
        {
            { "cat", typeof(CategoryDeclaration) },
            { "intf", typeof(ProtocolDeclaration) },
            { "cl", typeof(InterfaceDeclaration) },
            { "data", typeof(VarDeclaration) },
            { "tdef", typeof(TypedefDeclaration) },
            { "func", typeof(FunctionDeclaration) },
            { "econst", typeof(EnumDeclaration) },
            { "tag", typeof(BaseRecordDeclaration) }
        };
        private static DbProviderFactory factory = System.Data.SQLite.SQLiteFactory.Instance;
        private static string connString = @"Data Source=Docset - 8.1.sqlite;Version=3;";

        public static string GetStringIfPresent(this DbDataReader reader, int ordinal)
        {
            return !reader.IsDBNull(ordinal) ? reader.GetString(ordinal) : string.Empty;
        }

        public static IEnumerable<TokenMetadata> GetData(IosVersion iosVersion) // TODO
        {
            string query = string.Format(@"SELECT ZTOKEN.ZTOKENNAME, ZTOKENTYPE.ZTYPENAME, ZTOKENMETAINFORMATION.ZABSTRACT, ZTOKENMETAINFORMATION.ZANCHOR, ZTOKENMETAINFORMATION.ZDECLARATION, ZTOKENMETAINFORMATION.ZDEPRECATIONSUMMARY
                                           FROM ZTOKENMETAINFORMATION
                                           JOIN ZTOKEN ON ZTOKENMETAINFORMATION.ZTOKEN = ZTOKEN.Z_PK
                                           JOIN ZTOKENTYPE ON ZTOKEN.ZTOKENTYPE == ZTOKENTYPE.Z_PK
                                           WHERE ZTOKENTYPE.ZTYPENAME IN {0}
                                           ORDER BY ZTOKEN.ZTOKENNAME", FormatSet("econst", "tag", "tdef", "data"));

            List<TokenMetadata> data = new List<TokenMetadata>();
            using (DbConnection conn = factory.CreateConnection())
            {
                conn.ConnectionString = connString;
                conn.Open();

                using (DbCommand command = conn.CreateCommand())
                {
                    command.CommandText = query;
                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new TokenMetadata()
                            {
                                Name = reader.GetString(0),
                                Type = typeDict[reader.GetString(1)],
                                Abstract = reader.GetStringIfPresent(2),
                                Anchor = reader.GetStringIfPresent(3),
                                Declaration = reader.GetStringIfPresent(4),
                                DeprecationSummary = reader.GetStringIfPresent(5)
                            });
                        }
                    }
                }
            }
            return data;
        }

        public static TokenMetadata[] GetTokens()
        {
            using (DbConnection conn = factory.CreateConnection())
            {
                conn.ConnectionString = connString;
                conn.Open();

                var tokens = GetTokens(conn).OrderBy(c => c.Module).ThenBy(c => c.Type.Name).ToArray();
                return tokens;
            }
        }

        private static string FormatSet(params string[] items)
        {
            StringBuilder builder = new StringBuilder("(");
            foreach (var item in items)
            {
                builder.AppendFormat("\"{0}\", ", item);
            }
            builder.Remove(builder.Length - 2, 2);
            builder.Append(")");
            return builder.ToString();
        }

        private static string GetModuleName(string framework, string headerPath)
        {
            if (!string.IsNullOrEmpty(framework))
            {
                return framework;
            }

            if (headerPath.StartsWith(@"/usr/include/objc"))
            {
                return "ObjectiveC";
            }
            else if (headerPath.StartsWith(@"/usr/include/dispatch"))
            {
                return "Dispatch";
            }
            else if (headerPath == @"/usr/include/MacTypes.h")
            {
                return "MacTypes";
            }
            throw new Exception(string.Format("No module found for {0}", headerPath));
        }

        private static string FixName(string name, Type type)
        {
            if (type == typeof(CategoryDeclaration))
            {
                Regex pattern = new Regex(@".*\((.*)\)");
                string categoryName = pattern.Match(name).Groups[1].Value;
                return categoryName;
            }
            return name;
        }

        private static IEnumerable<TokenMetadata> GetTokens(DbConnection conn)
        {
            string[] tokenTypes = { "cat", "intf", "cl", "data", "tdef", "func" };
            string query = string.Format(@"SELECT ZTOKEN.ZTOKENNAME, ZTOKENTYPE.ZTYPENAME, ZHEADER.ZHEADERPATH, ZHEADER.ZFRAMEWORKNAME
                                           FROM ZTOKENMETAINFORMATION
                                           JOIN ZTOKEN ON ZTOKENMETAINFORMATION.ZTOKEN = ZTOKEN.Z_PK
                                           JOIN ZHEADER ON ZTOKENMETAINFORMATION.ZDECLAREDIN = ZHEADER.Z_PK
                                           JOIN ZTOKENTYPE ON ZTOKEN.ZTOKENTYPE == ZToKENTYPE.Z_PK
                                           WHERE ZTOKEN.ZLANGUAGE != 3 AND
                                                 ZTOKENTYPE.ZTYPENAME IN {0}AND
                                                 ZTOKENMETAINFORMATION.Z_PK NOT IN (SELECT Z_16REMOVEDAFTERINVERSE FROM Z_3REMOVEDAFTERINVERSE)", FormatSet(tokenTypes));

            using (DbCommand command = conn.CreateCommand())
            {
                command.CommandText = query;
                using (DbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string tokenName = reader.GetString(0);
                        string tokenType = reader.GetString(1);
                        string headerPath = reader.GetString(2);
                        string frameworkName = reader.GetStringIfPresent(3);
                        var type = typeDict[tokenType];
                        yield return new TokenMetadata()
                        {
                            Type = type,
                            Name = FixName(tokenName, type),
                            Module = GetModuleName(frameworkName, headerPath)
                        };
                    }
                }
            }
        }
    }
}
