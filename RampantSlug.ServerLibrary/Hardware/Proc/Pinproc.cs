﻿// The MIT License
// 
// Copyright (c) 2010 Adam Preble
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

//
// C# wrapper for libpinproc (http://github.com/preble/libpinproc)
// Initial version written 10/31/2010 by Adam Preble
// 
// To compile and run with Mono:
// 
//   gmcs pinproc.cs testmain.cs
//   mono pinproc.exe
//


using System;
using System.Runtime.InteropServices;

namespace RampantSlug.ServerLibrary.Hardware.Proc
{
    public enum LogLevel
    {
        Verbose = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    };

    public enum MachineType
    {
        Invalid = 0,
        Custom = 1,
        WPCAlphanumeric = 2,
        WPC = 3,
        WPC95 = 4,
        SternWhitestar = 5,
        SternSAM = 6,
        PDB = 7
    };

  /*  public enum SwitchType
    {
        NO = 0,
        NC = 1
    };*/

    public enum Result { Success = 1, Failure = 0 };

    public enum EventType
    {
        Invalid = 0,
        SwitchClosedDebounced = 1,
        SwitchOpenDebounced = 2,
        SwitchClosedNondebounced = 3,
        SwitchOpenNondebounced = 4,
        DMDFrameDisplayed = 5,
        None = 6
    };

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct DriverGlobalConfig
    {
        public bool EnableOutputs;
        public bool GlobalPolarity;
        public bool UseClear;
        public bool StrobeStartSelect;
        public byte StartStrobeTime;
        public byte MatrixRowEnableIndex1;
        public byte MatrixRowEnableIndex0;
        public bool ActiveLowMatrixRows;
        public bool EncodeEnables;
        public bool TickleSternWatchdog;
        public bool WatchdogExpired;
        public bool WatchdogEnable;
        public UInt16 WatchdogResetTime;
    };

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct DriverGroupConfig
    {
        public byte GroupNum;
        public UInt16 SlowTime;
        public byte EnableIndex;
        public byte RowActivateIndex;
        public byte RowEnableSelect;
        public bool Matrixed;
        public bool Polarity;
        public bool Active;
        public bool DisableStrobeAfter;
    };

    [StructLayout(LayoutKind.Sequential, Size = 28), Serializable]
    public struct DriverState
    {
        public UInt16 DriverNum;
        public byte OutputDriveTime;
        public bool Polarity;
        public bool State;
        public bool WaitForFirstTimeSlot;
        public UInt32 Timeslots;
        public byte PatterOnTime;
        public byte PatterOffTime;
        public bool PatterEnable;
        public bool futureEnable;

        public override string ToString() { return string.Format("DriverState num={0}", DriverNum); }
    };

    // Status: Tested good.
    [StructLayout(LayoutKind.Sequential, Size = 12), Serializable]
    public struct Event
    {
        public EventType Type;
        public UInt32 Value;
        public UInt32 Time;

        public override string ToString() { return string.Format("Event type={0} value={1}", Type, Value); }
    };

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct SwitchConfig
    {
        public bool Clear;
        public bool HostEventsEnable;
        public bool UseColumn9;
        public bool UseColumn8;
        public byte DirectMatrixScanLoopTime;
        public byte PulsesBeforeCheckingRX;
        public byte InactivePulsesAfterBurst;
        public byte PulsesPerBurst;
        public byte PulseHalfPeriodTime;
    };

    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct SwitchRule
    {
        public bool ReloadActive;
        public bool NotifyHost;
    };

    // Status: Partial test passed (only checked DeHighCycles)
    [StructLayout(LayoutKind.Sequential, Size = 60), Serializable]
    public struct DMDConfig
    {
        public byte NumRows;
        public UInt16 NumColumns;
        public byte NumSubFrames;
        public byte NumFrameBuffers;
        public bool AutoIncBufferWrPtr;
        public bool EnableFrameEvents;
        public bool Enable;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 8)]
        public Byte[] RclkLowCycles;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 8)]
        public Byte[] LatchHighCycles;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U2, SizeConst = 8)]
        public UInt16[] DeHighCycles;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 8)]
        public Byte[] DotclkHalfPeriod;

        public DMDConfig(int columns, int rows)
        {
            this.AutoIncBufferWrPtr = true;
            this.NumRows = (byte)rows;
            this.NumColumns = (UInt16)columns;
            this.NumSubFrames = 4;
            this.NumFrameBuffers = 3;
            this.Enable = true;
            this.EnableFrameEvents = true;
            this.RclkLowCycles = new byte[8];
            this.LatchHighCycles = new byte[8];
            this.DeHighCycles = new UInt16[8];
            this.DotclkHalfPeriod = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                this.RclkLowCycles[i] = 15;
                this.LatchHighCycles[i] = 15;
                this.DotclkHalfPeriod[i] = 1;
            }
            // 60 fps timing:
            this.DeHighCycles[0] = 90;
            this.DeHighCycles[1] = 190;
            this.DeHighCycles[2] = 50;
            this.DeHighCycles[3] = 377;
        }
    };

    /// <summary>
    /// Container for libpinproc (via P/Invoke) methods.
    /// </summary>
    public abstract class PinProc
    {
        // Constant
        public const int kPRSwitchPhysicalFirst = 0;
        public const int kPRSwitchPhysicalLast = 223;
        public const int kPRSwitchVirtualFirst = 224;
        public const int kPRSwitchVirtualLast = 255;
        public const int kPRSwitchCount = 256;
        public const int kPRDriverCount = 256;
        public const int kPRSwitchRulesCount = (kPRSwitchCount << 2);
        // General Methods //

        // Status: Presumed good
        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PRLogSetLevel(LogLevel level);

        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int PRGetVersionInfo(ref UInt16 version, ref UInt16 revision, ref UInt16 combined);

        // Status: Good
        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern string PRGetLastErrorText();

        // Status: Good
        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PRCreate(MachineType machineType);

        // Status: Good
        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PRDelete(IntPtr handle);

        // Staus: Presumed good
        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result PRReset(IntPtr handle, UInt32 flags);

        // Status: Good
        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result PRFlushWriteData(IntPtr handle);


        // Driver Methods //


        // Status: Presumed good
        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result PRDriverWatchdogTickle(IntPtr handle);


        // Status: Soft tested
        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result PRDriverGetState(IntPtr handle, byte driverNum, ref DriverState driverState);

        // Status: Soft tested
        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result PRDriverUpdateState(IntPtr handle, ref DriverState driverState);

        // Status: UNTESTED
        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result PRDriverUpdateGlobalConfig(IntPtr handle, ref DriverGlobalConfig driverGlobalConfig);

        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result PRDriverGetGroupConfig(IntPtr handle, byte groupNum, ref DriverGroupConfig driverGroupConfig);

        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result PRDriverUpdateGroupConfig(IntPtr handle, ref DriverGroupConfig driverGroupConfig);

        // Status: UNTESTED
        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PRDriverStateDisable(ref DriverState state);

        // Status: UNTESTED
        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PRDriverStatePulse(ref DriverState state, byte milliseconds);

        // Status: UNTESTED
        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PRDriverStateSchedule(ref DriverState state, UInt32 schedule, byte cycleSeconds, bool now);

        // Status: UNTESTED
        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PRDriverStatePatter(ref DriverState state, UInt16 millisecondsOn, UInt16 millisecondsOff, UInt16 originalOnTime);

        // Status: UNTESTED
        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PRDriverStatePulsedPatter(ref DriverState state, UInt16 millisecondsOn, UInt16 millisecondsOff, UInt16 patterTime);

        // Status: UNTESTED
        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PRDriverFuturePulse(IntPtr handle, byte driverNum, UInt16 milliseconds, UInt16 futureTime);

        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PRDriverStateFuturePulse(ref DriverState state, UInt16 milliseconds, UInt16 futureTime);

        // Status: Good
        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt16 PRDecode(MachineType machineType, string str);

        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result PRDriverGroupDisable(IntPtr handle, byte groupNum);


        // Switch & Event Methods //


        // Status: Good
        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int PRGetEvents(IntPtr handle, [In, Out] Event[] events, int maxEvents);

        // Status: UNTESTED
        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result PRSwitchUpdateConfig(IntPtr handle, ref SwitchConfig switchConfig);

        // Status: UNTESTED
        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result PRSwitchUpdateRule(IntPtr handle, byte switchNum, EventType eventType, ref SwitchRule rule, DriverState[] linkedDrivers, int numDrivers, bool drive_outputs_now);

        // Status: UNTESTED
        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result PRSwitchGetStates(IntPtr handle, [In, Out] EventType[] switchStates, UInt16 numSwitches);



        // DMD Methods //


        // Status: UNTESTED
        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result PRDMDDraw(IntPtr handle, byte[] dots);

        // Status: Good
        [DllImport("pinproc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result PRDMDUpdateConfig(IntPtr handle, ref DMDConfig config);


    }
}

