﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using DsCore.Win32;

namespace DsCore.MemoryX64
{
    public class Codecave
    {
        private Process _Process;
        private IntPtr _ProcessHandle;
        private IntPtr _ModuleBaseAddress = IntPtr.Zero;
        private UInt64 _Cave_Address = 0;
        private UInt64 _CaveOffset = 0;

        public UInt64 CaveAddress
        {
            get { return _Cave_Address; }
        }
        public UInt64 CaveOffset
        {
            get { return _CaveOffset; }
        }

        public Codecave(Process p, IntPtr BaseAddress)
        {
            _Process = p;
            _ModuleBaseAddress = BaseAddress;
        }

        /// <summary>
        /// Trying to access the process
        /// </summary>
        /// <returns>True if success, otherwise False</returns>
        public bool Open()
        {
            _ProcessHandle = _Process.Handle;
            if (_ProcessHandle != IntPtr.Zero)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Reserves a region of memory within the virtual address space of a specified process. 
        /// The function initializes the memory it allocates to zero.
        /// </summary>
        /// <param name="Size">The size of the region of memory to allocate, in bytes.</param>
        /// <returns>True is success, otherwise False</returns>
        public bool Alloc(UInt32 Size)
        {
            //Allocation mémoire
            _Cave_Address = (UInt64)Win32API.VirtualAllocEx(_ProcessHandle, IntPtr.Zero, Size, MemoryAllocType.MEM_COMMIT, MemoryPageProtect.PAGE_EXECUTE_READWRITE);
            if (_Cave_Address != 0)
                return true;
            else
                return false;
        }

        //jmp [Address]
        public bool Write_jmp(UInt64 AbsoluteAddress)
        {
            List<Byte> Buffer = new List<byte>();
            Buffer.Add(0xFF);
            Buffer.Add(0x25);
            Buffer.Add(0x00);
            Buffer.Add(0x00);
            Buffer.Add(0x00);
            Buffer.Add(0x00);
            Buffer.AddRange(BitConverter.GetBytes(AbsoluteAddress));
            return Write_Bytes(Buffer.ToArray());
        }

        //nop
        public bool Write_nop(int Amount = 1)
        {
            List<Byte> Buffer = new List<byte>();
            for (int i = 0; i < Amount; i++)
            {
                Buffer.Add(0x90);
            }
            return Write_Bytes(Buffer.ToArray());
        }

        /// <summary>
        /// Write bytes in memory, read from a string like "00 00 00 00"
        /// </summary>
        /// <param name="StrBuffer">String formated series of bytes to write</param>
        /// <returns>True if success, otherwise False</returns>
        public bool Write_StrBytes(String StrBuffer)
        {
            String[] sBytes = StrBuffer.Split(' ');
            List<Byte> Buffer = new List<byte>();
            foreach (String hex in sBytes)
            {
                Buffer.Add((byte)Convert.ToInt32(hex, 16));
            }
            return Write_Bytes(Buffer.ToArray());
        }

        /// <summary>
        /// Write bytes in memory, read from an array of bytes
        /// </summary>
        /// <param name="Buffer">Array of bytes to write</param>
        /// <returns>True if success, otherwise False</returns>
        public bool Write_Bytes(Byte[] Buffer)
        {
            UIntPtr BytesWritten = UIntPtr.Zero;
            if (Win32API.WriteProcessMemoryX64(_ProcessHandle, (IntPtr)(_Cave_Address + _CaveOffset), Buffer, (UIntPtr)Buffer.Length, out BytesWritten))
            {
                _CaveOffset += (UInt64)BytesWritten;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
