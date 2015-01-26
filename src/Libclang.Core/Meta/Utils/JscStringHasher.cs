using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Libclang.Core.Meta.Utils
{
    public class JscStringHasher
    {
        // Golden ratio. Arbitrary start value to avoid mapping all zeros to a hash value of zero.
        private const uint StringHashingStartValue = 0x9E3779B9U;
        private static readonly Encoding encoding = new UTF8Encoding();
        private static readonly int flagCount = 8;

        private uint hash;
        private bool hasPendingCharacter;
        private ushort pendingCharacter;

        public static uint Hash(string value)
        {
            return JscStringHasher.ComputeHashAndMaskTop8Bits(value);
        }

        public static uint ComputeHashAndMaskTop8Bits(string value)
        {
            JscStringHasher hasher = new JscStringHasher();
            hasher.AddCharactersAssumingAligned(value);
            return hasher.HashWithTop8BitsMasked();
        }

        public JscStringHasher()
        {
            this.Reset();
        }

        private void AddCharactersAssumingAligned(string key)
        {
            byte[] bytes = encoding.GetBytes(key);
            AddCharactersAssumingAligned(bytes, bytes.Length);
        }

        private void AddCharactersAssumingAligned(byte[] data, int length)
        {
            Debug.Assert(!hasPendingCharacter);

            int remainder = length & 1;
            length >>= 1;

            int dataIndex = 0;
            while (length-- > 0)
            {
                AddCharactersAssumingAligned(Convert(data[dataIndex]), Convert(data[dataIndex + 1]));
                dataIndex += 2;
            }

            if (remainder != 0)
                AddCharacter(Convert(data[dataIndex]));
        }

        private void AddCharactersAssumingAligned(ushort a, ushort b)
        {
            Debug.Assert(!hasPendingCharacter);
            hash += a;
            hash = (uint) ((hash << 16) ^ ((b << 11) ^ hash));
            hash += hash >> 11;
        }

        private void AddCharacter(ushort character)
        {
            if (hasPendingCharacter)
            {
                hasPendingCharacter = false;
                AddCharactersAssumingAligned(pendingCharacter, character);
                return;
            }

            pendingCharacter = character;
            hasPendingCharacter = true;
        }

        private uint HashWithTop8BitsMasked()
        {
            uint result = AvalancheBits();

            // Reserving space from the high bits for flags preserves most of the hash's
            // value, since hash lookup typically masks out the high bits anyway.
            int sizeOfResult = 4;
            result &= (1U << (sizeOfResult*8 - flagCount)) - 1;

            // This avoids ever returning a hash code of 0, since that is used to
            // signal "hash not computed yet". Setting the high bit maintains
            // reasonable fidelity to a hash code of 0 because it is likely to yield
            // exactly 0 when hash lookup masks out the high bits.
            if (result == 0)
                result = 0x80000000 >> flagCount;

            return result;
        }

        private uint AvalancheBits()
        {
            uint result = hash;

            // Handle end case.
            if (hasPendingCharacter)
            {
                result += pendingCharacter;
                result ^= result << 11;
                result += result >> 17;
            }

            // Force "avalanching" of final 31 bits.
            result ^= result << 3;
            result += result >> 5;
            result ^= result << 2;
            result += result >> 15;
            result ^= result << 10;

            return result;
        }

        private static ushort Convert(byte value)
        {
            return (ushort) value;
        }

        private void Reset()
        {
            this.hash = StringHashingStartValue;
            this.hasPendingCharacter = false;
            this.pendingCharacter = 0;
        }
    }
}
