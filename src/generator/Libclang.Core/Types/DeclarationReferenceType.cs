using Libclang.Core.Ast;
using Libclang.Core.Generator;
using Libclang.Core.Meta.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Libclang.Core.Types
{
    public class DeclarationReferenceType : TypeDefinition
    {
        private static Dictionary<string, string> CFOpaqueStructsPointers = new Dictionary<string, string>
        {
            {"CFArrayRef", "NSArray"},
            {"CFAttributedStringRef", "NSAttributedString"},
            {"CFCalendarRef", "NSCalendar"},
            {"CFCharacterSetRef", "NSCharacterSet"},
            {"CFDataRef", "NSData"},
            {"CFDateRef", "NSDate"},
            {"CFDictionaryRef", "NSDictionary"},
            {"CFErrorRef", "NSError"},
            {"CFLocaleRef", "NSLocale"},
            {"CFMutableArrayRef", "NSMutableArray"},
            {"CFMutableAttributedStringRef", "NSMutableAttributedString"},
            {"CFMutableCharacterSetRef", "NSMutableCharacterSet"},
            {"CFMutableDataRef", "NSMutableData"},
            {"CFMutableDictionaryRef", "NSMutableDictionary"},
            {"CFMutableSetRef", "NSMutableSet"},
            {"CFMutableStringRef", "NSMutableString"},
            {"CFNumberRef", "NSNumber"},
            {"CFReadStreamRef", "NSInputStream"},
            {"CFRunLoopTimerRef", "NSTimer"},
            {"CFSetRef", "NSSet"},
            {"CFStringRef", "NSString"},
            {"CFTimeZoneRef", "NSTimeZone"},
            {"CFURLRef", "NSURL"},
            {"CFWriteStreamRef", "NSOutputStream"},
        };

        public BaseDeclaration Target { get; set; }

        internal string TargetUSR { get; set; }

        public DeclarationReferenceType(BaseDeclaration target)
        {
            this.Target = target;
        }

        internal override string ToStringInternal(string identifier, bool isOuter = false)
        {
            if (identifier.Length > 0)
            {
                identifier = " " + identifier;
            }
            return ToStringHelper() + (Target != null ? Target.FullName : "__UNRESOLVED__") + identifier;
        }

        public override IEnumerable<TypeDefinition> ReferedTypes
        {
            get
            {
                return base.ReferedTypes.Union(this.Target.ReferedTypes);
            }
        }

        protected override bool? IsSupportedInternal(Dictionary<TypeDefinition, bool> typesCache, Dictionary<BaseDeclaration, bool> declarationsCache)
        {
            if (this.Target is UnionDeclaration || this.Target is UnresolvedDeclaration)
            {
                return false;
            }

            return base.IsSupportedInternal(typesCache, declarationsCache);
        }

        public override TypeEncoding ToTypeEncoding()
        {
            if (this.Target is TypedefDeclaration)
            {
                // if is BOOL
                if (this.IsObjCBOOL())
                {
                    return TypeEncoding.Bool;
                }

                // if is unichar
                if (this.IsUnichar())
                {
                    return TypeEncoding.Unichar;
                }

                var typeDef = this.Target as TypedefDeclaration;
                if (typeDef.UnderlyingType.IsPrimitive())
                {
                    return typeDef.UnderlyingType.ResolvePrimitive().ToTypeEncoding();
                }

                // if is pointer to opaque structure
                if (this.IsTypeDefToPointerToOpaqueStruct())
                {
                    // if is pointer to CF opaque structure
                    if (CFOpaqueStructsPointers.ContainsKey(typeDef.Name))
                    {
                        return TypeEncoding.Interface(CFOpaqueStructsPointers[typeDef.Name], typeDef.Module.FullName);
                    }
                }

                return typeDef.UnderlyingType.ToTypeEncoding();
            }
            else if (this.Target is BaseRecordDeclaration)
            {
                BaseRecordDeclaration record = (BaseRecordDeclaration) this.Target;

                if (record.IsOpaque)
                {
                    return TypeEncoding.Void;
                }

                if (record.IsAnonymousWithoutTypedef())
                {
                    var typeEncodingFields = record.Fields.Select(f => new RecordField()
                    {
                        Name = f.Name,
                        TypeEncoding = f.Type.ToTypeEncoding()
                    });

                    if (record is StructDeclaration)
                    {
                        return TypeEncoding.AnonymousStruct(typeEncodingFields);
                    }
                    else if (record is UnionDeclaration)
                    {
                        return TypeEncoding.AnonymousUnion(typeEncodingFields);
                    }
                }
                else
                {
                    if (this.Target.Module == null)
                    {
                        return TypeEncoding.Unknown;
                    }
                    if (record is StructDeclaration)
                    {
                        return TypeEncoding.Struct(record.GetJSName(), record.Module.FullName);
                    }
                    else if (record is UnionDeclaration)
                    {
                        return TypeEncoding.Union(record.GetJSName(), record.Module.FullName);
                    }
                }

                throw new Exception("Unknown type of record.");
            }
            else if (this.Target is EnumDeclaration)
            {
                return (this.Target as EnumDeclaration).UnderlyingType.ToTypeEncoding();
            }
            else if (this.Target is InterfaceDeclaration)
            {
                if (this.Target.Module == null)
                {
                    return TypeEncoding.Unknown;
                }

                return TypeEncoding.InterfaceDeclaration(this.Target.GetJSName(), this.Target.Module.FullName);
            }
            else if (this.Target is UnresolvedDeclaration)
            {
                throw new Exception("Unable to calculate type encoding of unresolved declaration.");
            }

            throw new Exception("Unable to calculate type encoding.");
        }
    }
}
