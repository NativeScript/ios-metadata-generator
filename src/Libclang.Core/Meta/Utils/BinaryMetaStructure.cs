using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Libclang.Core.Meta.Utils
{
    public class BinaryMetaStructure
    {
        public const int HasName = 7;
        public const int IsIosAppExtensionAvailable = 6;

        public const int FunctionIsVariadic = 5;
        public const int FunctionOwnsReturnedCocoaObject = 4;

        public const int MemberIsLocalJsNameDuplicate = 0;
        public const int MemberHasJsNameDuplicateInHierarchy = 1;

        public const int MethodIsVariadic = 2;
        public const int MethodIsNullTerminatedVariadic = 3;
        public const int MethodOwnsReturnedCocoaObject = 4;

        public const int PropertyHasGetter = 2;
        public const int PropertyHasSetter = 3;

        public BinaryMetaStructure()
        {
            this.Flags = new bool[8];
        }

        public string Name { get; set; }

        public string JsName { get; set; }

        public bool[] Flags { get; set; }

        public string Framework { get; set; }

        public Common.Version IntrducedIn { get; set; }

        public Object Info { get; set; }

        public MetaStructureType Type
        {
            get
            {
                string bits = String.Join("", String.Join("", this.Flags.Select(f => f ? "1" : "0")).Substring(0, 3).Reverse());
                return (MetaStructureType)Convert.ToInt32(bits, 2);
            }
            set
            {
                int numValue = (int)value;
                for (int i = 0; i < 3; i++)
                {
                    this.Flags[i] = ((numValue >> i) & 1) == 1;
                }
            }
        }
    }

    public static class BinaryMetaStructureExtensions
    {
        public static readonly Dictionary<string, byte> FrameworkToId = new Dictionary<string, byte>
        {
            {"Accelerate", 1}, {"Accounts", 2}, {"AddressBook", 3}, {"AddressBookUI", 4}, {"AdSupport", 5}, {"AssetsLibrary", 6}, {"AudioToolbox", 7}, {"AudioUnit", 8}, {"AVFoundation", 9},
            {"CFNetwork", 10}, {"CoreAudio", 11}, {"CoreBluetooth", 12}, {"CoreData", 13}, {"CoreFoundation", 14}, {"CoreGraphics", 15}, {"CoreImage", 16}, {"CoreLocation", 17},
            {"CoreMedia", 18}, {"CoreMIDI", 19}, {"CoreMotion", 20}, {"CoreTelephony", 21}, {"CoreText", 22}, {"CoreVideo", 23}, {"EventKit", 24}, {"EventKitUI", 25},
            {"ExternalAccessory", 26}, {"Foundation", 27}, {"GameController", 28}, {"GameKit", 29}, {"GLKit", 30}, {"GSS", 31}, {"iAd", 32}, {"ImageIO", 33}, {"JavaScriptCore", 34},
            {"MapKit", 35}, {"MediaAccessibility", 36}, {"MediaPlayer", 37}, {"MediaToolbox", 38}, {"MessageUI", 39}, {"MobileCoreServices", 40}, {"MultipeerConnectivity", 41},
            {"NewsstandKit", 42}, {"OpenAL", 43}, {"OpenGLES", 44}, {"PassKit", 45}, {"QuartzCore", 46}, {"QuickLook", 47}, {"SafariServices", 48}, {"Security", 49}, {"Social", 50},
            {"SpriteKit", 51}, {"StoreKit", 52}, {"SystemConfiguration", 53}, {"Twitter", 54}, {"UIKit", 55}, {"UsrLib", 56}, {"Metal", 57}, {"VideoToolbox", 58}, {"CloudKit", 59},
            {"HealthKit", 60}, {"HomeKit", 61}, {"AVKit", 62}, {"CoreAudioKit", 63}, {"LocalAuthentication", 64}, {"NetworkExtension", 65}, {"NotificationCenter", 66},
            {"Photos", 67}, {"PhotosUI", 68}, {"PushKit", 69}, {"vecLib", 70}, {"vImage", 71}, {"WebKit", 72}
        };

        public static byte ToByte(this bool[] flags)
        {
            if (flags.Length > 8)
            {
                throw new ArgumentException("The flags must be max 8.", "flags");
            }

            byte result = 0;
            for (int i = 0; i < flags.Length; i++)
            {
                if (flags[i])
                {
                    result |= (byte)(1 << i);
                }
            }
            return result;
        }

        public static byte ToByte(this string framework)
        {
            return FrameworkToId[framework];
        }

        public static byte ToByte(this Common.Version version)
        {
            byte result = 0;
            if (version != null && version.Major != -1)
            {
                Debug.Assert(version.Major >= 1 && version.Major <= 31);
                result |= (byte)(version.Major << 3);
                if (version.Minor != -1)
                {
                    Debug.Assert(version.Minor >= 0 && version.Minor <= 7);
                    result |= (byte)version.Minor;
                }
            }
            return result;
        }

        public static BinaryMetaStructure ChangeToJsCode(this BinaryMetaStructure structure, string jsCode)
        {
            Debug.Assert(structure.Info == null, "The JS Code will override other information.");
            structure.Type = MetaStructureType.JsCode;
            structure.Info = new Pointer(jsCode);
            return structure;
        }
    }

    public class Pointer
    {
        public Pointer(object value = null)
        {
            this.Value = value;
        }

        public Object Value { get; set; }
    }

    public class ArrayCount
    {
        public ArrayCount(uint value = 0)
        {
            this.Value = value;
        }

        public uint Value { get; set; }
    }
}
