using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpartansLib.Structure
{
    public unsafe class UnmanagedDictionary<TKey>
    {
        public enum EntryValueType
        {
            None,
            SByte,
            Byte,
            Short,
            UShort,
            Int,
            UInt,
            Long,
            ULong,
            Char,
            Float,
            Double,
            Decimal,
            Bool,
            Enum,
            CustomStruct,
            Unknown
        }

        private struct Entry
        {
            public uint Hash;
            public TKey Key;
            public void* Value;
            public EntryValueType Type;
            public GCHandle Handle;

            public Entry(TKey key, void* value, uint hash, EntryValueType type, GCHandle handle)
            {
                Key = key;
                Value = value;
                Hash = hash;
                Type = type;
                Handle = handle;
            }
        }

        public struct PointerRef
        {
            public void* Ref;
        }

        private List<Entry>[] _entries = new List<Entry>[47];
        private int _count;

        private void _Resize()
        {
            var newEntries = new List<Entry>[(int)(_entries.Length * 1.5)];
            foreach (var list in _entries)
            {
                if (list == null) continue;
                foreach (var e in list)
                {
                    var hash = (uint)(e.Key.GetHashCode() % newEntries.Length);
                    if (newEntries[hash] != null)
                        newEntries[hash].Add(new Entry(e.Key, e.Value, hash, e.Type, e.Handle));
                    else
                        newEntries[hash] = new List<Entry> { new Entry(e.Key, e.Value, hash, e.Type, e.Handle) };
                }
            }
            _entries = newEntries;
        }

        private Entry _GetEntry(TKey index)
        {
            var hash = index.GetHashCode() % _entries.Length;
            if (_entries[hash] == null) return default;
            return _entries[hash].First(p => p.Key.Equals(index));
        }

        public void AddOrSet(TKey index, void* value, EntryValueType type, GCHandle handle)
        {
            var loadFactor = _count / _entries.Length;
            if (_count / _entries.Length > .8) _Resize();
            uint hash = (uint)(index.GetHashCode() % _entries.Length);
            if (_entries[hash] != null)
            {

                var innerIndex = _entries[hash].FindIndex(e => e.Key.Equals(index));
                if (innerIndex != -1)
                {
                    _entries[hash][innerIndex] = new Entry(index, value, hash, type, handle);
                    return;
                }
                _count++;
                _entries[hash].Add(new Entry(index, value, hash, type, handle));
                return;
            }
            _count++;
            _entries[hash] = new List<Entry> { new Entry(index, value, hash, type, handle) };
        }

        private static readonly Type
                _typeSByte = typeof(sbyte),
                _typeByte = typeof(byte),
                _typeShort = typeof(short),
                _typeUShort = typeof(ushort),
                _typeInt = typeof(int),
                _typeUInt = typeof(uint),
                _typeLong = typeof(long),
                _typeULong = typeof(ulong),
                _typeChar = typeof(char),
                _typeFloat = typeof(float),
                _typeDouble = typeof(double),
                _typeDecimal = typeof(decimal),
                _typeBool = typeof(bool);

        private static EntryValueType _GetValueTypeByType(Type type)
        {
            if (type == _typeSByte) return EntryValueType.SByte;
            if (type == _typeByte) return EntryValueType.Byte;
            if (type == _typeShort) return EntryValueType.Short;
            if (type == _typeUShort) return EntryValueType.UShort;
            if (type == _typeInt) return EntryValueType.Int;
            if (type == _typeUInt) return EntryValueType.UInt;
            if (type == _typeLong) return EntryValueType.Long;
            if (type == _typeULong) return EntryValueType.ULong;
            if (type == _typeChar) return EntryValueType.Char;
            if (type == _typeFloat) return EntryValueType.Float;
            if (type == _typeDouble) return EntryValueType.Double;
            if (type == _typeDecimal) return EntryValueType.Decimal;
            if (type == _typeBool) return EntryValueType.Bool;
            return EntryValueType.Unknown;
        }

        public static bool IsPrimitiveValueType(Type type)
        {
            return _GetValueTypeByType(type) != EntryValueType.Unknown;
        }

        public void AddOrSet<T>(TKey index, T value) where T : unmanaged
        {
            var handle = GCHandle.Alloc(value, GCHandleType.Pinned);
            AddOrSet(index, (void*)handle.AddrOfPinnedObject(), _GetValueTypeByType(typeof(T)), handle);
        }

        public bool TryGet<T>(TKey index, out T result) where T : unmanaged
        {
            var entry = _GetEntry(index);
            if (_GetValueTypeByType(typeof(T)) != entry.Type)
            {
                result = default;
                return false;
            }
            result = *(T*)entry.Value;
            return true;
        }

        public T Get<T>(TKey index) where T : unmanaged
        {
            TryGet(index, out T result);
            return result;
        }

        public void Remove(TKey index)
        {
            uint hash = (uint)(index.GetHashCode() % _entries.Length);
            if (_entries[hash] != null)
            {
                var innerIndex = _entries[hash].FindIndex(e => e.Key.Equals(index));
                if (innerIndex != -1)
                {
                    _count--;
                    _entries[hash][innerIndex].Handle.Free();
                    _entries[hash].RemoveAt(innerIndex);
                }
            }
        }

        public void Clear()
        {
            foreach (var list in _entries)
            {
                foreach (var e in list)
                    e.Handle.Free();
                list.Clear();
            }
        }

        public bool ContainsKey(TKey index)
        {
            return _GetEntry(index).Hash != 0;
        }
    }
}