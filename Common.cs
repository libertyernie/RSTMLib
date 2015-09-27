using System;
using System.Runtime.InteropServices;

namespace RSTMLib
{
    public enum Endian
    {
        Big = 0xFEFF,
        Little = 0xFFFE
    }

    public unsafe struct BinTag
    {
        byte _1, _2, _3, _4;

        public BinTag(string tag)
        {
            _1 = _2 = _3 = _4 = 0;
            Set(tag);
        }
        public BinTag(uint tag, bool isLittleEndian)
        {
            _1 = _2 = _3 = _4 = 0;
            if (isLittleEndian)
                *(uint*)Address = tag;
            else
                *(buint*)Address = tag;
        }
        public uint Get(bool returnLittleEndian)
        {
            if (returnLittleEndian)
                return *(uint*)Address;
            else
                return *(buint*)Address;
        }

        public string Get() { return new String(Address, 0, 4); }
        public void Set(string tag)
        {
            for (int i = 0; i < 4; i++)
                Address[i] = (sbyte)tag[i];
        }

        public static implicit operator BinTag(string r) { return new BinTag(r); }
        public static implicit operator string(BinTag r) { return r.Get(); }
        public static implicit operator BinTag(uint r) { return new BinTag(r, true); }
        public static implicit operator uint(BinTag r) { return r.Get(true); }

        public sbyte* Address { get { fixed (void* ptr = &this)return (sbyte*)ptr; } }

        public override string ToString()
        {
            return Get();
        }
    }

    struct DataBlock
    {
        private VoidPtr _address;
        private uint _length;

        public VoidPtr Address { get { return _address; } set { _address = value; } }
        public uint Length { get { return _length; } set { _length = value; } }
        public VoidPtr EndAddress { get { return _address + _length; } }

        public DataBlock(VoidPtr address, uint length)
        {
            _address = address;
            _length = length;
        }
    }

    unsafe struct DataBlockCollection
    {
        private DataBlock _block;

        public DataBlockCollection(DataBlock block) { _block = block; }

        private buint* Data { get { return (buint*)_block.EndAddress; } }

        public DataBlock this[int index]
        {
            get { return new DataBlock(_block.Address + Data[index << 1], Data[(index << 1) + 1]); }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct NW4RCommonHeader
    {
        public const uint Size = 0x10;

        public BinTag _tag;
        public bushort _endian;
        public bushort _version;
        public bint _length;
        public bushort _firstOffset;
        public bushort _numEntries;

        public VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
        public DataBlock DataBlock { get { return new DataBlock(Address, Size); } }

        public byte VersionMajor { get { return ((byte*)_version.Address)[0]; } set { ((byte*)_version.Address)[0] = value; } }
        public byte VersionMinor { get { return ((byte*)_version.Address)[1]; } set { ((byte*)_version.Address)[1] = value; } }
        public Endian Endian { get { return (Endian)(short)_endian; } set { _endian = (ushort)value; } }

        public DataBlockCollection Entries { get { return new DataBlockCollection(DataBlock); } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ruint
    {
        public enum RefType
        {
            Address = 0,
            Offset = 1
        }

        public byte _refType;

        public byte _dataType;
        //Specifies which struct to get, ex
        //DataRef<T, T1, T2, T3>
        //if dataType == 2, return T2 struct at address

        public bushort _reserved;
        public bint _dataOffset;

        public ruint(RefType refType, byte dataType, int data)
        {
            _refType = (byte)refType;
            _dataType = dataType;
            _reserved = 0;
            _dataOffset = data;
        }

        public VoidPtr Offset(VoidPtr baseAddr) { return baseAddr + _dataOffset; }

        public static implicit operator ruint(int r) { return new ruint() { _refType = 1, _dataOffset = r }; }
        public static implicit operator int(ruint r) { return r._dataOffset; }
        public static implicit operator ruint(uint r) { return new ruint() { _refType = 1, _dataOffset = (int)r }; }
        public static implicit operator uint(ruint r) { return (uint)r._dataOffset; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct RuintList
    {
        //This address is the base of all ruint entry offsets

        public bint _numEntries;

        public VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
        public ruint* Entries { get { return (ruint*)(Address + 4); } }
        public VoidPtr Data { get { return Address + _numEntries * 8 + 4; } }

        public VoidPtr this[int index]
        {
            get { return (int)Entries[index]; }
            set { Entries[index] = (int)value; }
        }

        public VoidPtr Get(VoidPtr offset, int index) { return offset + Entries[index]; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct RuintCollection
    {
        private ruint _first;

        public VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }
        public ruint* Entries { get { return (ruint*)Address; } }

        public VoidPtr this[int index]
        {
            get { return Address + Entries[index]; }
            set { Entries[index] = (int)(value - Address); }
        }

        public VoidPtr Offset(VoidPtr offset) { return Address + offset; }

        public VoidPtr Get(int index) { return Address + Entries[index]; }

        public void Set(int index, ruint.RefType refType, byte dataType, VoidPtr address)
        {
            *((ruint*)Address + index) = new ruint(refType, dataType, (int)(address - Address));
        }
    }
}
