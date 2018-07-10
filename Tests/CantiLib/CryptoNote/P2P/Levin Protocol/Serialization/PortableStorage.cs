﻿using Canti.Data;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Canti.CryptoNote.P2P
{
    internal partial class PortableStorage
    {
        // Dictionary of entries
        Dictionary<string, object> Entries = new Dictionary<string, object>();

        // Add an entry
        internal bool AddEntry(string Name, object Value)
        {
            // Get object type
            SerializationType Type = GetType(Value);

            // Add to entry table
            if (Type != SerializationType.INVALID) Entries.Add(Name, Value);

            // Entry is of invalid type
            else return false; // TODO - throw error
            return true;
        }

        // Serializes entry table to a byte array
        internal byte[] Serialize(bool IncludeHeader = true)
        {
            // Create an output array
            byte[] Output = new byte[0];

            // Write header
            if (IncludeHeader)
            {
                // Add signatures
                Output = Encoding.AppendToByteArray(Encoding.UintToByteArray(PORTABLE_STORAGE_SIGNATUREA), Output);
                Output = Encoding.AppendToByteArray(Encoding.UintToByteArray(PORTABLE_STORAGE_SIGNATUREB), Output);

                // Add version number
                Output = Encoding.AppendToByteArray(new byte[] { PORTABLE_STORAGE_FORMAT_VER }, Output);
            }

            // Add array length
            Output = Encoding.AppendToByteArray(SerializeVarInt(Entries.Count), Output);

            // Iterate over objects
            foreach (KeyValuePair<string, object> Entry in Entries)
            {
                // Serialize object
                byte[] EntryBytes = SerializeEntry(Entry.Key, Entry.Value);

                // Add to output array
                Output = Encoding.AppendToByteArray(EntryBytes, Output);
            }

            // Return output array
            return Output;
        }

        // Deserializes a byte array to a storage object
        internal bool Deserialize(byte[] Data)
        {
            //

            // Return result
            return false;
        }

        // Gets an object's serialization type
        internal static SerializationType GetType(object Value)
        {
            // Get serialization type
            SerializationType Type;
            if (Value == null) Type = SerializationType.INVALID;
            else if (Value.GetType() == typeof(long)) Type = SerializationType.LONG;
            else if (Value.GetType() == typeof(int)) Type = SerializationType.INT;
            else if (Value.GetType() == typeof(short)) Type = SerializationType.SHORT;
            else if (Value.GetType() == typeof(sbyte)) Type = SerializationType.SBYTE;
            else if (Value.GetType() == typeof(ulong)) Type = SerializationType.ULONG;
            else if (Value.GetType() == typeof(uint)) Type = SerializationType.UINT;
            else if (Value.GetType() == typeof(ushort)) Type = SerializationType.USHORT;
            else if (Value.GetType() == typeof(byte)) Type = SerializationType.BYTE;
            else if (Value.GetType() == typeof(double)) Type = SerializationType.DOUBLE;
            else if (Value.GetType() == typeof(string)) Type = SerializationType.STRING;
            else if (Value.GetType() == typeof(bool)) Type = SerializationType.BOOL;
            else if (Value.GetType().IsArray) Type = SerializationType.ARRAY;
            else Type = SerializationType.OBJECT;
            return Type;
        }

        // Serializes an object to a byte array
        internal static byte[] SerializeObject(object Value, bool IncludeType = true)
        {
            // Create an output array
            byte[] Output = new byte[0];
            
            // Add object type
            SerializationType Type = GetType(Value);
            if (IncludeType) Output = new byte[] { (byte)Type };

            // Create entry bytes buffer
            byte[] EntryBytes = new byte[0];

            // Type is 64 bit
            if (Type == SerializationType.LONG || Type == SerializationType.ULONG)
            {
                // Encode bytes
                ulong Input = Convert.ToUInt64(Value);
                EntryBytes = new byte[8];
                EntryBytes[0] = (byte)Input;
                EntryBytes[1] = (byte)((Input >> 8) & 0xFF);
                EntryBytes[2] = (byte)((Input >> 16) & 0xFF);
                EntryBytes[3] = (byte)((Input >> 24) & 0xFF);
                EntryBytes[4] = (byte)((Input >> 32) & 0xFF);
                EntryBytes[5] = (byte)((Input >> 40) & 0xFF);
                EntryBytes[6] = (byte)((Input >> 48) & 0xFF);
                EntryBytes[7] = (byte)((Input >> 56) & 0xFF);
            }

            // Type is 32 bit
            else if (Type == SerializationType.INT || Type == SerializationType.UINT)
            {
                // Encode bytes
                uint Input = Convert.ToUInt32(Value);
                EntryBytes = new byte[4];
                EntryBytes[0] = (byte)Input;
                EntryBytes[1] = (byte)((Input >> 8) & 0xFF);
                EntryBytes[2] = (byte)((Input >> 16) & 0xFF);
                EntryBytes[3] = (byte)((Input >> 24) & 0xFF);
            }

            // Type is 16 bit
            else if (Type == SerializationType.SHORT || Type == SerializationType.USHORT)
            {
                // Encode bytes
                ushort Input = Convert.ToUInt16(Value);
                EntryBytes = new byte[2];
                EntryBytes[0] = (byte)Input;
                EntryBytes[1] = (byte)((Input >> 8) & 0xFF);
            }

            // Type is 8 bit
            else if (Type == SerializationType.SBYTE || Type == SerializationType.BYTE)
            {
                // Encode bytes
                byte Input = Convert.ToByte(Value);
                EntryBytes = new byte[1];
                EntryBytes[0] = Input;
            }

            // Type is double
            else if (Type == SerializationType.DOUBLE)
            {
                // Encode bytes
                ulong Input = (ulong)Convert.ToDouble(Value);
                EntryBytes = new byte[8];
                EntryBytes[0] = (byte)Input;
                EntryBytes[1] = (byte)((Input >> 8) & 0xFF);
                EntryBytes[2] = (byte)((Input >> 16) & 0xFF);
                EntryBytes[3] = (byte)((Input >> 24) & 0xFF);
                EntryBytes[4] = (byte)((Input >> 32) & 0xFF);
                EntryBytes[5] = (byte)((Input >> 40) & 0xFF);
                EntryBytes[6] = (byte)((Input >> 48) & 0xFF);
                EntryBytes[7] = (byte)((Input >> 56) & 0xFF);
            }

            // Type is string
            else if (Type == SerializationType.STRING)
            {
                // Check string length
                if (((string)Value).Length > MAX_STRING_LEN_POSSIBLE) EntryBytes = new byte[0];

                // Encode bytes
                EntryBytes = Encoding.StringToByteArray((string)Value);

                // Add string length
                EntryBytes = Encoding.AppendToByteArray(EntryBytes, SerializeVarInt(((string)Value).Length));
            }

            // Type is bool
            else if (Type == SerializationType.BOOL)
            {
                // Encode bytes
                EntryBytes = new byte[1] { (bool)Value ? (byte)1 : (byte)0 };
            }

            // Type is object
            else if (Type == SerializationType.OBJECT)
            {
                // Check if object has serialization method
                Type ObjectType = Value.GetType();
                MethodInfo Method = ObjectType.GetMethod("Serialize");
                if (Method == null) throw new Exception("Could not serialize object: No Serialize() method found in object type " + ObjectType.Name);

                // Encode bytes
                EntryBytes = (byte[])Method.Invoke(Value, null); // TODO - Fix this shit ???
            }

            // Type is array
            else if (Type == SerializationType.ARRAY)
            {
                // Encode bytes
                EntryBytes = SerializeArray((Array)Value);
            }

            // Return result
            if (EntryBytes.Length > 0) return Encoding.AppendToByteArray(EntryBytes, Output);
            else return new byte[0];
        }

        // Serializes a variable int to a byte array
        private static byte[] SerializeVarInt<T>(T Value) where T : IConvertible
        {
            // Create an output buffer
            byte[] Output = new byte[0];

            // Verify type
            SerializationType Type = GetType(Value);
            if ((int)Type < (int)SerializationType.LONG || (int)Type > (int)SerializationType.BYTE) return new byte[0];

            // Check varint size
            ulong Size = Convert.ToUInt64(Value);

            // Value is 8 bit
            if (Size <= 63)
            {
                // Encode varint
                byte Converted = Convert.ToByte(Value);
                byte Input = (byte)(Converted << 2);
                Input |= PORTABLE_RAW_SIZE_MARK_BYTE;

                // Encode bytes to buffer
                Output = new byte[1];
                Output[0] = (byte)Input;
            }

            // Value is 16 bit
            else if (Size <= 16383)
            {
                // Encode varint
                ushort Converted = Convert.ToUInt16(Value);
                ushort Input = (ushort)(Converted << 2);
                Input |= PORTABLE_RAW_SIZE_MARK_WORD;

                // Encode bytes to buffer
                Output = new byte[2];
                Output[0] = (byte)Input;
                Output[1] = (byte)((Input >> 8) & 0xFF);
            }

            // Value is 32 bit
            else if (Size <= 1073741823)
            {
                // Encode varint
                uint Converted = Convert.ToUInt32(Value);
                uint Input = (uint)(Converted << 2);
                Input |= PORTABLE_RAW_SIZE_MARK_DWORD;

                // Encode bytes to buffer
                Output = new byte[4];
                Output[0] = (byte)Input;
                Output[1] = (byte)((Input >> 8) & 0xFF);
                Output[2] = (byte)((Input >> 16) & 0xFF);
                Output[3] = (byte)((Input >> 24) & 0xFF);
            }

            // Value is 64 bit
            else if (Size <= 4611686018427387903)
            {
                // Encode varint
                ulong Converted = Convert.ToUInt64(Value);
                ulong Input = (ulong)(Converted << 2);
                Input |= PORTABLE_RAW_SIZE_MARK_INT64;

                // Encode bytes to buffer
                Output = new byte[8];
                Output[0] = (byte)Input;
                Output[1] = (byte)((Input >> 8) & 0xFF);
                Output[2] = (byte)((Input >> 16) & 0xFF);
                Output[3] = (byte)((Input >> 24) & 0xFF);
                Output[4] = (byte)((Input >> 32) & 0xFF);
                Output[5] = (byte)((Input >> 40) & 0xFF);
                Output[6] = (byte)((Input >> 48) & 0xFF);
                Output[7] = (byte)((Input >> 56) & 0xFF);
            }

            // Return encoded varint buffer
            return Output;
        }

        // Deserializes a variable int from a byte array, and returns a new offset value
        internal static T DeserializeVarInt<T>(byte[] Data, int Offset, out int NewOffset) where T : IConvertible
        {
            // Get byte size
            int SizeMask = Data[Offset] & PORTABLE_RAW_SIZE_MARK_MASK;
            int BytesLeft = 0;

            // Type is 8 bit
            if (SizeMask == PORTABLE_RAW_SIZE_MARK_BYTE) BytesLeft = 0;

            // Type is 16 bit
            else if (SizeMask == PORTABLE_RAW_SIZE_MARK_WORD) BytesLeft = 1;

            // Type is 32 bit
            else if (SizeMask == PORTABLE_RAW_SIZE_MARK_DWORD) BytesLeft = 3;

            // Type is 64 bit
            else if (SizeMask == PORTABLE_RAW_SIZE_MARK_INT64) BytesLeft = 7;

            // Calculate output number
            int Output = Data[Offset];
            for (NewOffset = Offset; NewOffset <= Offset + BytesLeft; NewOffset++)
                Output |= Data[NewOffset] << ((NewOffset - Offset) * 8);
            Output >>= 2;

            // Return result
            return (T)Convert.ChangeType(Output, typeof(T));
        }

        // Serializes an array to a byte array
        internal static byte[] SerializeArray(Array Value)
        {
            // Create an output array
            byte[] Output = new byte[0];

            // Add array size
            Output = SerializeVarInt(Value.LongLength);

            // Loop through each array object
            for (int i = 0; i < Value.Length; i++)
            {
                // Read object
                object Child = Value.GetValue(i);

                // Add bytes
                Output = Encoding.AppendToByteArray(SerializeObject(Child), Output);
            }

            // Return output array
            return Output;
        }

        // Serializes an entry to a byte array
        internal static byte[] SerializeEntry(string Name, object Value)
        {
            // Serialize name
            byte[] NameLength = new byte[] { (byte)Name.Length };
            byte[] NameBytes = Encoding.StringToByteArray(Name);
            NameBytes = Encoding.AppendToByteArray(NameBytes, NameLength);

            // Serialize object
            byte[] ObjectBytes = SerializeObject(Value);

            // Return output array
            return Encoding.AppendToByteArray(ObjectBytes, NameBytes);
        }





        internal bool AddEntryAsBinary(string name, object[] value)
        {
            // Make sure array contains values
            if (value == null && value.Length > 0) return false;
            
            // Create a buffer to hold all elements within array
            byte[] blob = new byte[value.Length * System.Runtime.InteropServices.Marshal.SizeOf(value.SyncRoot)];

            // Create an element of the array type, and assign the first element in the array to it
            //Type T = value.GetType().GetElementType();
            //var Element = Activator.CreateInstance(T);
            //Element = value[0];

            // Iterate through each element in the array
            for (int i = 0; i < value.Length; i++) blob = Encoding.AppendToByteArray(Encoding.ObjectToByteArray(value[0]), blob);

            /*T* ptr = reinterpret_cast<T*>(&blob[0]);

            for (const auto&item : value) {
                *ptr++ = item;
            }
            
            serializer.binary(blob, name);*/
            AddEntry(name, Encoding.ByteArrayToString(blob));
            return true;
        }
    }
}