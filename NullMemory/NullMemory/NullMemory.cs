using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

/* -------------------------------------------------- *
 *  INFORMATION :                            *
 *  This class coded by Lufzys         *
 *  https://github.com/Lufzys          *
 * ----------------------------------------------------*/

namespace NullMemory
{
    public class NullMemory
    {
        public Process NullProcess = default(Process);
        public IntPtr ProcessHandle = IntPtr.Zero;
        public string ProcessName = string.Empty;
        public int ProcessId = -1;

        public NullMemory(string processName)
        {
            ProcessName = processName;
        }

        public NullMemory (int processId)
        {
            ProcessId = processId;
        }

        public Enums.InitializeResult Initialize()
        {
            if(ProcessIsRunning())
            {
                if (ProcessId != -1)
                {
                    try
                    {
                        NullProcess = Process.GetProcessById(ProcessId);
                        ProcessName = NullProcess.ProcessName;
                        ProcessId = NullProcess.Id;
                        ProcessHandle = Imports.OpenProcess(Constants.PROCESS_VM_OPERATION | Constants.PROCESS_VM_READ | Constants.PROCESS_VM_WRITE, false, ProcessId);
                        return Enums.InitializeResult.Succesfully;
                    }
                    catch
                    {
                        return Enums.InitializeResult.HandleCouldNotAccess;
                    }
                }
                else if (ProcessName != string.Empty)
                {
                    try
                    {
                        NullProcess = Process.GetProcessesByName(ProcessName).FirstOrDefault();
                        ProcessName = NullProcess.ProcessName;
                        ProcessId = NullProcess.Id;
                        ProcessHandle = Imports.OpenProcess(Constants.PROCESS_VM_OPERATION | Constants.PROCESS_VM_READ | Constants.PROCESS_VM_WRITE, false, ProcessId);
                        return Enums.InitializeResult.Succesfully;
                    }
                    catch
                    {
                        return Enums.InitializeResult.HandleCouldNotAccess;
                    }
                }
                else
                {
                    return Enums.InitializeResult.Error;
                }
            }
            else
            {
                return Enums.InitializeResult.ProcessNotFound;
            }
        }

        public bool ProcessIsRunning()
        {
            if (ProcessId != -1)
            {
                foreach (Process proc in Process.GetProcesses())
                {
                    if (proc.Id == ProcessId)
                    {
                        return true;
                    }
                }
                return false;
            }
            else if (ProcessName != string.Empty)
            {
                if (Process.GetProcessesByName(ProcessName).Length != 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        #region Read/Write (Normal & Kernel)

        private int m_iNumberOfBytesRead;
        private int m_iNumberOfBytesWritten;

        public  T ReadMemory<T>(int address) where T : struct
        {
            int ByteSize = Marshal.SizeOf(typeof(T));

            byte[] buffer = new byte[ByteSize];

            Imports.ReadProcessMemory((int)ProcessHandle, address, buffer, buffer.Length, ref m_iNumberOfBytesRead);

            return ByteArrayToStructure<T>(buffer);
        }

        public  byte[] ReadMemory(int offset, int size)
        {
            byte[] buffer = new byte[size];

            Imports.ReadProcessMemory((int)ProcessHandle, offset, buffer, size, ref m_iNumberOfBytesRead);

            return buffer;

        }

        public  bool ReadBytes(int StartingAdress, ref byte[] output)
        {
            if (Imports.ReadProcessMemory((int)ProcessHandle, StartingAdress, output, output.Length, ref m_iNumberOfBytesRead)) return true;
            else return false;
        }

        public  byte[] ReadBytes(int StartingAdress, int length)
        {
            byte[] output = new byte[length];
            if (Imports.ReadProcessMemory((int)ProcessHandle, StartingAdress, output, output.Length, ref m_iNumberOfBytesRead)) return output;
            else return null;
        }

        public  string ReadStringASCII(int offset, int size)
        {
            return Encoding.ASCII.GetString(ReadMemory(offset, size)).CutString();
        }
        public  string ReadStringUnicode(int offset, int size)
        {
            return Encoding.Unicode.GetString(ReadMemory(offset, size)).CutString();
        }
        public  string ReadStringUTF(int offset, int size)
        {
            return Encoding.UTF8.GetString(ReadMemory(offset, size)).CutString();
        }

        public  float[] ReadMatrix<T>(int address, int MatrixSize) where T : struct
        {
            int ByteSize = Marshal.SizeOf(typeof(T));

            byte[] buffer = new byte[ByteSize * MatrixSize];

            Imports.ReadProcessMemory((int)ProcessHandle, address, buffer, buffer.Length, ref m_iNumberOfBytesRead);

            return ConvertToFloatArray(buffer);
        }

        public  void WriteMemory<T>(int address, object Value) where T : struct
        {
            byte[] buffer = StructureToByteArray(Value);

            Imports.WriteProcessMemory((int)ProcessHandle, address, buffer, buffer.Length, out m_iNumberOfBytesWritten);
        }

        [SuppressUnmanagedCodeSecurity]
        public class Kernel
        {
            public Kernel(NullMemory nullMem)
            {
                ProcessHandle = nullMem.ProcessHandle;
            }
            private IntPtr ProcessHandle = IntPtr.Zero;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe T Read<T>(IntPtr address) where T : struct
            {
                byte[] buffer = new byte[Unsafe.SizeOf<T>()];

                Imports.NtReadVirtualMemory(ProcessHandle, address, buffer, buffer.Length, 0);

                fixed (byte* b = buffer)
                    return Unsafe.Read<T>(b);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe bool Write<T>(IntPtr address, T value) where T : struct
            {
                byte[] buffer = new byte[Unsafe.SizeOf<T>()];

                fixed (byte* b = buffer)
                    Unsafe.Write<T>(b, value);

                return Imports.NtWriteVirtualMemory(ProcessHandle, address, buffer, buffer.Length, 0);
            }
        }
        #endregion

        #region Extensions | WorldToScreen & FindDMAAddy
        public Vector2 WorldToScreen(Vector3 target, int width, int height, float[] viewMatrix /*float[16]*/)
        {
            Vector2 _worldToScreenPos;
            Vector3 to;
            float w = 0.0f;
            float[] viewmatrix = new float[16];
            viewmatrix = viewMatrix;

            to.X = viewmatrix[0] * target.X + viewmatrix[1] * target.Y + viewmatrix[2] * target.Z + viewmatrix[3];
            to.Y = viewmatrix[4] * target.X + viewmatrix[5] * target.Y + viewmatrix[6] * target.Z + viewmatrix[7];

            w = viewmatrix[12] * target.X + viewmatrix[13] * target.Y + viewmatrix[14] * target.Z + viewmatrix[15];

            // behind us
            if (w < 0.01f)
                return new Vector2(0, 0);

            to.X *= (1.0f / w);
            to.Y *= (1.0f / w);

            //int width = Main.ScreenSize.Width;
            //int height = Main.ScreenSize.Height;

            float x = width / 2;
            float y = height / 2;

            x += 0.5f * to.X * width + 0.5f;
            y -= 0.5f * to.Y * height + 0.5f;

            to.X = x;
            to.Y = y;

            _worldToScreenPos.X = to.X;
            _worldToScreenPos.Y = to.Y;
            return _worldToScreenPos;
        }

        public  IntPtr FindDMAAddy(IntPtr ptr, int[] offsets) // https://stackoverflow.com/questions/35788512/c-sharp-need-to-add-one-offset-to-two-addresses-that-leave-up-to-my-value
        {
            var buffer = new byte[IntPtr.Size];
            foreach (int i in offsets)
            {
                Imports.ReadProcessMemory((int)ProcessHandle, (int)ptr, buffer, buffer.Length, ref m_iNumberOfBytesRead);

                ptr = (IntPtr.Size == 4)
                ? IntPtr.Add(new IntPtr(BitConverter.ToInt32(buffer, 0)), i)
                : ptr = IntPtr.Add(new IntPtr(BitConverter.ToInt64(buffer, 0)), i);
            }
            return ptr;
        }
        #endregion

        #region SigScan
        public  int FindPattern(Module module, byte[] pattern, string mask)
        {
            byte[] moduleBytes = new byte[module.Size];

            if (ReadBytes(module.Address, ref moduleBytes))
            {
                for (int i = 0; i < module.Size; i++)
                {
                    bool found = true;

                    for (int l = 0; l < mask.Length; l++)
                    {
                        found = mask[l] == '?' || moduleBytes[l + i] == pattern[l];

                        if (!found)
                            break;
                    }

                    if (found)
                        return i;
                }
            }

            return 0;
        }
        public  Int32 FindPattern(Module module, string sig, int offset, int extra, bool relative)
        {
            byte[] moduleDump = new byte[module.Size];
            int moduleAddress = module.Address;

            if (ReadBytes(module.Address, ref moduleDump))
            {
                byte[] pattern = SignatureToPattern(sig);
                string mask = GetSignatureMask(sig);
                IntPtr address = IntPtr.Zero;

                for (int i = 0; i < module.Size; i++)
                {
                    if (address == IntPtr.Zero && pattern.Length + i < module.Size)
                    {
                        bool isSuccess = true;

                        for (int k = 0; k < pattern.Length; k++)
                        {
                            if (mask[k] == '?')
                                continue;


                            if (pattern[k] != moduleDump[i + k])
                                isSuccess = false;
                        }

                        if (!isSuccess) continue;

                        if (address == IntPtr.Zero)
                        {
                            if (relative)
                            {
                                return BitConverter.ToInt32(ReadBytes(module.Address + i + offset, 4), 0) + extra - module.Address;
                            }
                            else
                            {
                                return BitConverter.ToInt32(ReadBytes(module.Address + i + offset, 4), 0) + extra;
                            }
                        }
                    }
                }
            }

            return -1;
        }

        private  byte[] SignatureToPattern(string sig)
        {
            string[] parts = sig.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            byte[] patternArray = new byte[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "?")
                {
                    patternArray[i] = 0;
                    continue;
                }

                if (!byte.TryParse(parts[i], System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.DefaultThreadCurrentCulture, out patternArray[i]))
                {
                    throw new Exception();
                }
            }

            return patternArray;
        }

        private  string GetSignatureMask(string sig)
        {
            string[] parts = sig.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string mask = "";

            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "?")
                {
                    mask += "?";
                }
                else
                {
                    mask += "x";
                }
            }

            return mask;
        }

        private  string GetFullfilledMask(byte[] buffer)
        {
            string result = "";
            for (int i = 0; i < buffer.Length; i++)
            {
                result += "x";
            }
            return result;
        }
        #endregion

        #region Only GetModuleAddress Func.
        public int GetModuleByName(string moduleName)
        {
            foreach (ProcessModule module in NullProcess.Modules)
            {
                if (module.ModuleName == moduleName)
                {
                    return (Int32)module.BaseAddress;
                }
            }
            return -1;
        }
        #endregion

        #region Module Object
        public class Module
        {
            public Process process;
            public string Name;
            public int Address;
            public int Size;

            public Module(Process proc, string nName)
            {
                process = proc;
                Name = nName;
                Get();
            }

            private void Get()
            {
                foreach (ProcessModule m in process.Modules)
                {
                    if (m.ModuleName == Name)
                    {
                        Address = (Int32)m.BaseAddress;
                        Size = (Int32)m.ModuleMemorySize;
                    }
                }
            }

            public string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Process => " + process.ProcessName);
                sb.AppendLine("Name    => " + Name);
                sb.AppendLine("Address => 0x" + Address.ToString("X"));
                sb.AppendLine("Size    => 0x" + Size.ToString("X"));
                return sb.ToString();
            }
        }
        #endregion

        #region Transformation

        public  float[] ConvertToFloatArray(byte[] bytes)
        {
            if (bytes.Length % 4 != 0) throw new ArgumentException();

            float[] floats = new float[bytes.Length / 4];

            for (int i = 0; i < floats.Length; i++) floats[i] = BitConverter.ToSingle(bytes, i * 4);

            return floats;
        }

        private  T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            try
            {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }

        private  byte[] StructureToByteArray(object obj)
        {
            int length = Marshal.SizeOf(obj);

            byte[] array = new byte[length];

            IntPtr pointer = Marshal.AllocHGlobal(length);

            Marshal.StructureToPtr(obj, pointer, true);
            Marshal.Copy(pointer, array, 0, length);
            Marshal.FreeHGlobal(pointer);

            return array;
        }

        #endregion
    }
}
