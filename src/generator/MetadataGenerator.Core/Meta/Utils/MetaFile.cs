using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Libclang.Core.Meta.Utils
{
    public class MetaFile : IBinarySerializable
    {
        private readonly IBinaryConverter converter;

        private readonly List<BinarySymbol> globalTableSymbols;

        private readonly List<BinarySymbol> categoriesSymbols;

        private readonly BytesList heap;

        private readonly List<string> referedModules;

        private readonly Dictionary<string, CalculatedOffset> uniqueStrings;

        public MetaFile()
        {
            this.converter = new DefaultBinaryConverter()
            {
                OffsetSize = 4,
                ArrayCountSize = 4,
                ModuleIdSize = 2,
                StringEncoding = Encoding.ASCII,
                OffsetCalculator = this.AddOffsetValue,
                ModuleIdCalculator = this.CalculateModuleId
            };

            this.globalTableSymbols = new List<BinarySymbol>();
            this.categoriesSymbols = new List<BinarySymbol>();
            this.heap = new BytesList();
            this.heap.Append(0); // the first byte in the heap is null
            this.referedModules = new List<string>();
            this.uniqueStrings = new Dictionary<string, CalculatedOffset>();
        }

        public void AddSymbol(BinarySymbol symbol)
        {
            if (symbol.Type == BinarySymbol.SymbolType.Category)
            {
                this.categoriesSymbols.Add(symbol);
            }
            else
            {
                this.globalTableSymbols.Add(symbol);
            }
        }

        public object GetBinaryStructure()
        {
            // Global Table
            BinaryHashTable gt = new BinaryHashTable(SymbolHasher);
            gt.AddRange(this.globalTableSymbols);
            BytesList globalTableSection = this.converter.Convert(gt);

            // Refered Modules Names Array
            BinaryArray<NotCalculatedOffset> modulesNamesStructure = new BinaryArray<NotCalculatedOffset>(this.referedModules.Select(m => new NotCalculatedOffset(m)));
            BytesList modulesNamesSection = this.converter.Convert(modulesNamesStructure);

            // Categories Array
            BinaryArray<NotCalculatedOffset> categoriesStructure = new BinaryArray<NotCalculatedOffset>(this.categoriesSymbols.Select(c => new NotCalculatedOffset(c)));
            BytesList categoriesSection = this.converter.Convert(categoriesStructure);

            List<object> file = new List<object>() 
            {
                globalTableSection.Length, globalTableSection,
                modulesNamesSection.Length, modulesNamesSection, 
                categoriesSection.Length, categoriesSection,
                heap.Length, heap
            };

            return file;
        }

        public void SaveAs(string filePath)
        {
            BytesList file = this.converter.Convert(this);
            using (FileStream writer = File.OpenWrite(filePath))
            {
                writer.Write(file.ToArray(), 0, (int)file.Length);
            }
        }

        private static uint SymbolHasher(Object obj)
        {
            return JscStringHasher.Hash(((BinarySymbol)obj).JsName);
        }

        private CalculatedOffset AddOffsetValue(NotCalculatedOffset notCalculatedOffset)
        {
            if (notCalculatedOffset.Value != null)
            {
                if (notCalculatedOffset.Value is string)
                {
                    string stringValue = (string)notCalculatedOffset.Value;
                    if (this.uniqueStrings.ContainsKey(stringValue))
                    {
                        return this.uniqueStrings[stringValue];
                    }
                    else
                    {
                        CalculatedOffset stringOffsetInHeap = new CalculatedOffset(this.heap.Append(this.converter.Convert(stringValue)));
                        this.uniqueStrings.Add(stringValue, stringOffsetInHeap);
                        return stringOffsetInHeap;
                    }
                }
                
                return new CalculatedOffset(this.heap.Append(this.converter.Convert(notCalculatedOffset.Value)));
            }

            return new CalculatedOffset(0); // null
        }

        private int CalculateModuleId(string fullModuleName)
        {
            int id = this.referedModules.BinarySearch(fullModuleName);
            if (id >= 0)
            {
                return id;
            }
            int newId = ~id;
            this.referedModules.Insert(newId, fullModuleName);
            return newId;
        }
    }
}
