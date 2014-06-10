using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;
using System.IO;


namespace ConversationFieldDetector
{
    public class Mixer
    {
        public const int MIXER_SHORT_NAME_CHARS = 16;
        public const int MIXER_LONG_NAME_CHARS = 64;
        public const int MAXPNAMELEN = 32;

        public const uint MIXER_OBJECTF_HANDLE = 0x80000000;
        public const uint MIXER_OBJECTF_MIXER = 0x0;
        public const uint MIXER_OBJECTF_HMIXER = (MIXER_OBJECTF_HANDLE | MIXER_OBJECTF_MIXER);
        public const uint MIXER_OBJECTF_WAVEOUT = 0x10000000;
        public const uint MIXER_OBJECTF_HWAVEOUT = (MIXER_OBJECTF_HANDLE | MIXER_OBJECTF_WAVEOUT);
        public const uint MIXER_OBJECTF_WAVEIN = 0x20000000;
        public const uint MIXER_OBJECTF_HWAVEIN = (MIXER_OBJECTF_HANDLE | MIXER_OBJECTF_WAVEIN);
        public const uint MIXER_OBJECTF_MIDIOUT = 0x30000000;
        public const uint MIXER_OBJECTF_HMIDIOUT = (MIXER_OBJECTF_HANDLE | MIXER_OBJECTF_MIDIOUT);
        public const uint MIXER_OBJECTF_MIDIIN = 0x40000000;
        public const uint MIXER_OBJECTF_HMIDIIN = (MIXER_OBJECTF_HANDLE | MIXER_OBJECTF_MIDIIN);
        public const uint MIXER_OBJECTF_AUX = 0x50000000;

        public const int MMSYSERR_BASE = 0;
        public const int MMSYSERR_NOERROR = 0;
        public const int MIXERR_BASE = 0x400;
        public const int MMSYSERR_BADDEVICEID = (MMSYSERR_BASE + 2);
        public const int MMSYSERR_NOTENABLED = (MMSYSERR_BASE + 3);
        public const int MMSYSERR_ALLOCATED = (MMSYSERR_BASE + 4);
        public const int MMSYSERR_INVALHANDLE = (MMSYSERR_BASE + 5);
        public const int MMSYSERR_NODRIVER = (MMSYSERR_BASE + 6);
        public const int MMSYSERR_NOMEM = (MMSYSERR_BASE + 7);
        public const int MMSYSERR_NOTSUPPORTED = (MMSYSERR_BASE + 8);
        public const int MMSYSERR_BADERRNUM = (MMSYSERR_BASE + 9);
        public const int MMSYSERR_INVALFLAG = (MMSYSERR_BASE + 10);
        public const int MMSYSERR_INVALPARAM = (MMSYSERR_BASE + 11);
        public const int MMSYSERR_HANDLEBUSY = (MMSYSERR_BASE + 12);

        public const int MMSYSERR_INVALIDALIAS = (MMSYSERR_BASE + 13);
        public const int MMSYSERR_BADDB = (MMSYSERR_BASE + 14);
        public const int MMSYSERR_KEYNOTFOUND = (MMSYSERR_BASE + 15);
        public const int MMSYSERR_READERROR = (MMSYSERR_BASE + 16);
        public const int MMSYSERR_WRITEERROR = (MMSYSERR_BASE + 17);
        public const int MMSYSERR_DELETEERROR = (MMSYSERR_BASE + 18);
        public const int MMSYSERR_VALNOTFOUND = (MMSYSERR_BASE + 19);
        public const int MMSYSERR_NODRIVERCB = (MMSYSERR_BASE + 20);
        public const int MMSYSERR_LASTERROR = (MMSYSERR_BASE + 20);

        public const int MIXER_GETCONTROLDETAILSF_LISTTEXT = 0x1;
        public const int MIXER_GETLINEINFOF_COMPONENTTYPE = 0x3;
        public const int MIXER_GETCONTROLDETAILSF_VALUE = 0x0;
        public const int MIXER_GETLINECONTROLSF_ONEBYTYPE = 0x2;
        public const int MIXER_SETCONTROLDETAILSF_VALUE = 0x0;
        public const int MIXER_GETLINEINFOF_DESTINATION = 0x0;
        public const int MIXER_GETLINEINFOF_SOURCE = 0x1;
        public const int MIXER_GETLINEINFOF_LINEID = 0x2;
        public const int MIXER_GETLINEINFOF_TARGETTYPE = 0x4;

        public const int MIXERLINE_COMPONENTTYPE_DST_FIRST = 0x0;
        public const int MIXERLINE_COMPONENTTYPE_DST_UNDEFINED = (MIXERLINE_COMPONENTTYPE_DST_FIRST + 0);
        public const int MIXERLINE_COMPONENTTYPE_DST_DIGITAL = (MIXERLINE_COMPONENTTYPE_DST_FIRST + 1);
        public const int MIXERLINE_COMPONENTTYPE_DST_LINE = (MIXERLINE_COMPONENTTYPE_DST_FIRST + 2);
        public const int MIXERLINE_COMPONENTTYPE_DST_MONITOR = (MIXERLINE_COMPONENTTYPE_DST_FIRST + 3);
        public const int MIXERLINE_COMPONENTTYPE_DST_SPEAKERS = (MIXERLINE_COMPONENTTYPE_DST_FIRST + 4);
        public const int MIXERLINE_COMPONENTTYPE_DST_HEADPHONES = (MIXERLINE_COMPONENTTYPE_DST_FIRST + 5);
        public const int MIXERLINE_COMPONENTTYPE_DST_TELEPHONE = (MIXERLINE_COMPONENTTYPE_DST_FIRST + 6);
        public const int MIXERLINE_COMPONENTTYPE_DST_WAVEIN = (MIXERLINE_COMPONENTTYPE_DST_FIRST + 7);
        public const int MIXERLINE_COMPONENTTYPE_DST_VOICEIN = (MIXERLINE_COMPONENTTYPE_DST_FIRST + 8);
        public const int MIXERLINE_COMPONENTTYPE_DST_LAST = (MIXERLINE_COMPONENTTYPE_DST_FIRST + 8);

        public const int MIXERLINE_COMPONENTTYPE_SRC_FIRST = 0x1000;
        public const int MIXERLINE_COMPONENTTYPE_SRC_UNDEFINED = (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 0);
        public const int MIXERLINE_COMPONENTTYPE_SRC_DIGITAL = (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 1);
        public const int MIXERLINE_COMPONENTTYPE_SRC_LINE = (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 2);
        public const int MIXERLINE_COMPONENTTYPE_SRC_MICROPHONE = (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 3);
        public const int MIXERLINE_COMPONENTTYPE_SRC_SYNTHESIZER = (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 4);
        public const int MIXERLINE_COMPONENTTYPE_SRC_COMPACTDISC = (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 5);
        public const int MIXERLINE_COMPONENTTYPE_SRC_TELEPHONE = (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 6);
        public const int MIXERLINE_COMPONENTTYPE_SRC_PCSPEAKER = (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 7);
        public const int MIXERLINE_COMPONENTTYPE_SRC_WAVEOUT = (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 8);
        public const int MIXERLINE_COMPONENTTYPE_SRC_AUXILIARY = (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 9);
        public const int MIXERLINE_COMPONENTTYPE_SRC_ANALOG = (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 10);
        public const int MIXERLINE_COMPONENTTYPE_SRC_LAST = (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 10);

        public const uint MIXERCONTROL_CT_CLASS_MASK = 0xF0000000;
        public const uint MIXERCONTROL_CT_CLASS_CUSTOM = 0x00000000;
        public const uint MIXERCONTROL_CT_CLASS_METER = 0x10000000;
        public const uint MIXERCONTROL_CT_CLASS_SWITCH = 0x20000000;
        public const uint MIXERCONTROL_CT_CLASS_NUMBER = 0x30000000;
        public const uint MIXERCONTROL_CT_CLASS_SLIDER = 0x40000000;
        public const uint MIXERCONTROL_CT_CLASS_FADER = 0x50000000;
        public const uint MIXERCONTROL_CT_CLASS_TIME = 0x60000000;
        public const uint MIXERCONTROL_CT_CLASS_LIST = 0x70000000;

        public const uint MIXERCONTROL_CT_SUBCLASS_MASK = 0x0F000000;
        public const uint MIXERCONTROL_CT_SC_SWITCH_BOOLEAN = 0x00000000;
        public const uint MIXERCONTROL_CT_SC_SWITCH_BUTTON = 0x01000000;
        public const uint MIXERCONTROL_CT_SC_METER_POLLED = 0x00000000;
        public const uint MIXERCONTROL_CT_SC_TIME_MICROSECS = 0x00000000;
        public const uint MIXERCONTROL_CT_SC_TIME_MILLISECS = 0x01000000;
        public const uint MIXERCONTROL_CT_SC_LIST_SINGLE = 0x00000000;
        public const uint MIXERCONTROL_CT_SC_LIST_MULTIPLE = 0x01000000;
        public const uint MIXERCONTROL_CT_UNITS_MASK = 0x00FF0000;
        public const uint MIXERCONTROL_CT_UNITS_CUSTOM = 0x00000000;
        public const uint MIXERCONTROL_CT_UNITS_BOOLEAN = 0x00010000;
        public const uint MIXERCONTROL_CT_UNITS_SIGNED = 0x00020000;
        public const uint MIXERCONTROL_CT_UNITS_UNSIGNED = 0x00030000;
        public const uint MIXERCONTROL_CT_UNITS_DECIBELS = 0x00040000;
        public const uint MIXERCONTROL_CT_UNITS_PERCENT = 0x00050000;

        public const uint MIXERCONTROL_CONTROLTYPE_CUSTOM = (MIXERCONTROL_CT_CLASS_CUSTOM | MIXERCONTROL_CT_UNITS_CUSTOM);
        public const uint MIXERCONTROL_CONTROLTYPE_BOOLEANMETER = (MIXERCONTROL_CT_CLASS_METER | MIXERCONTROL_CT_SC_METER_POLLED | MIXERCONTROL_CT_UNITS_BOOLEAN);
        public const uint MIXERCONTROL_CONTROLTYPE_SIGNEDMETER = (MIXERCONTROL_CT_CLASS_METER | MIXERCONTROL_CT_SC_METER_POLLED | MIXERCONTROL_CT_UNITS_SIGNED);
        public const uint MIXERCONTROL_CONTROLTYPE_PEAKMETER = (MIXERCONTROL_CONTROLTYPE_SIGNEDMETER + 1);
        public const uint MIXERCONTROL_CONTROLTYPE_UNSIGNEDMETER = (MIXERCONTROL_CT_CLASS_METER | MIXERCONTROL_CT_SC_METER_POLLED | MIXERCONTROL_CT_UNITS_UNSIGNED);
        public const uint MIXERCONTROL_CONTROLTYPE_BOOLEAN = (MIXERCONTROL_CT_CLASS_SWITCH | MIXERCONTROL_CT_SC_SWITCH_BOOLEAN | MIXERCONTROL_CT_UNITS_BOOLEAN);
        public const uint MIXERCONTROL_CONTROLTYPE_ONOFF = (MIXERCONTROL_CONTROLTYPE_BOOLEAN + 1);
        public const uint MIXERCONTROL_CONTROLTYPE_MUTE = (MIXERCONTROL_CONTROLTYPE_BOOLEAN + 2);
        public const uint MIXERCONTROL_CONTROLTYPE_MONO = (MIXERCONTROL_CONTROLTYPE_BOOLEAN + 3);
        public const uint MIXERCONTROL_CONTROLTYPE_LOUDNESS = (MIXERCONTROL_CONTROLTYPE_BOOLEAN + 4);
        public const uint MIXERCONTROL_CONTROLTYPE_STEREOENH = (MIXERCONTROL_CONTROLTYPE_BOOLEAN + 5);
        public const uint MIXERCONTROL_CONTROLTYPE_BUTTON = (MIXERCONTROL_CT_CLASS_SWITCH | MIXERCONTROL_CT_SC_SWITCH_BUTTON | MIXERCONTROL_CT_UNITS_BOOLEAN);
        public const uint MIXERCONTROL_CONTROLTYPE_DECIBELS = (MIXERCONTROL_CT_CLASS_NUMBER | MIXERCONTROL_CT_UNITS_DECIBELS);
        public const uint MIXERCONTROL_CONTROLTYPE_SIGNED = (MIXERCONTROL_CT_CLASS_NUMBER | MIXERCONTROL_CT_UNITS_SIGNED);
        public const uint MIXERCONTROL_CONTROLTYPE_UNSIGNED = (MIXERCONTROL_CT_CLASS_NUMBER | MIXERCONTROL_CT_UNITS_UNSIGNED);
        public const uint MIXERCONTROL_CONTROLTYPE_PERCENT = (MIXERCONTROL_CT_CLASS_NUMBER | MIXERCONTROL_CT_UNITS_PERCENT);
        public const uint MIXERCONTROL_CONTROLTYPE_SLIDER = (MIXERCONTROL_CT_CLASS_SLIDER | MIXERCONTROL_CT_UNITS_SIGNED);
        public const uint MIXERCONTROL_CONTROLTYPE_PAN = (MIXERCONTROL_CONTROLTYPE_SLIDER + 1);
        public const uint MIXERCONTROL_CONTROLTYPE_QSOUNDPAN = (MIXERCONTROL_CONTROLTYPE_SLIDER + 2);
        public const uint MIXERCONTROL_CONTROLTYPE_FADER = (MIXERCONTROL_CT_CLASS_FADER | MIXERCONTROL_CT_UNITS_UNSIGNED);
        public const uint MIXERCONTROL_CONTROLTYPE_VOLUME = (MIXERCONTROL_CONTROLTYPE_FADER + 1);
        public const uint MIXERCONTROL_CONTROLTYPE_BASS = (MIXERCONTROL_CONTROLTYPE_FADER + 2);
        public const uint MIXERCONTROL_CONTROLTYPE_TREBLE = (MIXERCONTROL_CONTROLTYPE_FADER + 3);
        public const uint MIXERCONTROL_CONTROLTYPE_EQUALIZER = (MIXERCONTROL_CONTROLTYPE_FADER + 4);
        public const uint MIXERCONTROL_CONTROLTYPE_SINGLESELECT = (MIXERCONTROL_CT_CLASS_LIST | MIXERCONTROL_CT_SC_LIST_SINGLE | MIXERCONTROL_CT_UNITS_BOOLEAN);
        public const uint MIXERCONTROL_CONTROLTYPE_MUX = (MIXERCONTROL_CONTROLTYPE_SINGLESELECT + 1);
        public const uint MIXERCONTROL_CONTROLTYPE_MULTIPLESELECT = (MIXERCONTROL_CT_CLASS_LIST | MIXERCONTROL_CT_SC_LIST_MULTIPLE | MIXERCONTROL_CT_UNITS_BOOLEAN);
        public const uint MIXERCONTROL_CONTROLTYPE_MIXER = (MIXERCONTROL_CONTROLTYPE_MULTIPLESELECT + 1);
        public const uint MIXERCONTROL_CONTROLTYPE_MICROTIME = (MIXERCONTROL_CT_CLASS_TIME | MIXERCONTROL_CT_SC_TIME_MICROSECS | MIXERCONTROL_CT_UNITS_UNSIGNED);
        public const uint MIXERCONTROL_CONTROLTYPE_MILLITIME = (MIXERCONTROL_CT_CLASS_TIME | MIXERCONTROL_CT_SC_TIME_MILLISECS | MIXERCONTROL_CT_UNITS_UNSIGNED);

        [DllImport("WinMM.dll", CharSet = CharSet.Ansi)]
        public static extern int mixerClose(int hmx);
        [DllImport("WinMM.dll", CharSet = CharSet.Ansi)]
        public static extern int mixerGetControlDetails(int hmxobj, ref MIXERCONTROLDETAILS pmxcd, int fdwDetails);
        [DllImport("WinMM.dll", CharSet = CharSet.Ansi)]
        public static extern int mixerGetDevCaps(int uMxId, ref MIXERCAPS pmxcaps, int cbmxcaps);
        [DllImport("WinMM.dll", CharSet = CharSet.Ansi)]
        public static extern int mixerGetID(int hmxobj, out int pumxID, uint fdwId);
        [DllImport("WinMM.dll", CharSet = CharSet.Ansi)]
        public static extern int mixerGetLineControls(int hmxobj, ref MIXERLINECONTROLS pmxlc, int fdwControls);
        [DllImport("WinMM.dll", CharSet = CharSet.Ansi)]
        public static extern int mixerGetLineInfo(int hmxobj, ref MIXERLINE pmxl, int fdwInfo);
        [DllImport("WinMM.dll", CharSet = CharSet.Ansi)]
        public static extern int mixerGetNumDevs();
        [DllImport("WinMM.dll", CharSet = CharSet.Ansi)]
        public static extern int mixerMessage(int hmx, int uMsg, int dwParam1, int dwParam2);
        [DllImport("WinMM.dll", CharSet = CharSet.Ansi)]
        public static extern int mixerOpen(out int phmx, int uMxId, int dwCallback, int dwInstance, int fdwOpen);
        [DllImport("WinMM.dll", CharSet = CharSet.Ansi)]
        public static extern int mixerSetControlDetails(int hmxobj, ref MIXERCONTROLDETAILS pmxcd, int fdwDetails);

        public struct MIXERCAPS
        {
            public short wMid;
            public short wPid;
            public int vDriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAXPNAMELEN)]
            public string szPname;
            public int fdwSupport;
            public int cDestinations;
        }

        public struct MIXERCONTROL
        {
            public int cbStruct;
            public int dwControlID;
            public int dwControlType;
            public int fdwControl;
            public int cMultipleItems;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MIXER_SHORT_NAME_CHARS)]
            public string szShortName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MIXER_LONG_NAME_CHARS)]
            public string szName;
            public int lMinimum;
            public int lMaximum;
            [MarshalAs(UnmanagedType.U4, SizeConst = 10)]
            public int reserved;
        }

        public struct MIXERCONTROLDETAILS
        {
            public int cbStruct;
            public int dwControlID;
            public int cChannels;
            public int item;
            public int cbDetails;
            public IntPtr paDetails;
        }

        public struct MIXERCONTROLDETAILS_BOOLEAN
        {
            public short fValue;
        }

        public struct MIXERCONTROLDETAILS_LISTTEXT
        {
            public int dwParam1;
            public int dwParam2;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MIXER_LONG_NAME_CHARS)]
            public string szName;
        }

        public struct MIXERCONTROLDETAILS_UNSIGNED
        {
            public int dwValue;
        }

        public struct MIXERCONTROLDETAILS_SIGNED
        {
            public int lValue;
        }

        public struct MIXERLINE
        {
            public int cbStruct;
            public int dwDestination;
            public int dwSource;
            public int dwLineID;
            public int fdwLine;
            public int dwUser;
            public uint dwComponentType;
            public int cChannels;
            public int cConnections;
            public int cControls;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MIXER_SHORT_NAME_CHARS)]
            public string szShortName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MIXER_LONG_NAME_CHARS)]
            public string szName;
            public int dwType;
            public int dwDeviceID;
            public short wMid;
            public short wPid;
            public int vDriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAXPNAMELEN)]
            public string szPname;
        }

        public static void init(out int mixer)
        {
            mixerOpen(out mixer, 0, 0, 0, 0);
        }    

        public struct MIXERLINECONTROLS
        {
            public int cbStruct;
            public int dwLineID;
            public uint dwControl;
            public int cControls;
            public int cbmxctrl;
            public IntPtr pamxctrl;
        }

        private static bool GetControlByID(int mixer, int dst_id, int src_id, uint ctrl_type, out MIXERCONTROL outCtrl)
        {
            outCtrl = new MIXERCONTROL();
            MIXERLINE dst_line = new MIXERLINE();

            dst_line.cbStruct = Marshal.SizeOf(dst_line);
            dst_line.dwDestination = dst_id;
            int retval = mixerGetLineInfo(mixer, ref dst_line, MIXER_GETLINEINFOF_DESTINATION);
            if (retval != MMSYSERR_NOERROR)
                return false;

            if (src_id < 0)
            {
                MIXERLINECONTROLS dst_lc = new MIXERLINECONTROLS();
                int mcSize = 152;
                dst_lc.pamxctrl = Marshal.AllocCoTaskMem(mcSize);
                dst_lc.cbStruct = Marshal.SizeOf(dst_lc);
                dst_lc.dwLineID = dst_line.dwLineID;
                dst_lc.dwControl = ctrl_type;
                dst_lc.cControls = 1;
                dst_lc.cbmxctrl = mcSize;

                retval = mixerGetLineControls(mixer, ref dst_lc, MIXER_GETLINECONTROLSF_ONEBYTYPE);
                if (retval != MMSYSERR_NOERROR)
                {
                    Marshal.FreeCoTaskMem(dst_lc.pamxctrl);
                    return false;
                }

                MIXERCONTROL ctrl = new MIXERCONTROL();
                ctrl.cbStruct = mcSize;

                ctrl = (MIXERCONTROL)Marshal.PtrToStructure(dst_lc.pamxctrl, typeof(MIXERCONTROL));
                outCtrl = ctrl;
                Marshal.FreeCoTaskMem(dst_lc.pamxctrl);
                return true;
            }
            else
            {
                MIXERLINE src_line = new MIXERLINE();
                src_line.cbStruct = dst_line.cbStruct;
                src_line.dwDestination = dst_line.dwDestination;
                src_line.dwSource = src_id;
                retval = mixerGetLineInfo(mixer, ref src_line, MIXER_GETLINEINFOF_SOURCE);
                if (retval != MMSYSERR_NOERROR)
                    return false;

                MIXERLINECONTROLS src_lc = new MIXERLINECONTROLS();
                int mcSize = 152;
                src_lc.pamxctrl = Marshal.AllocCoTaskMem(mcSize);
                src_lc.cbStruct = Marshal.SizeOf(src_lc);
                src_lc.dwLineID = src_line.dwLineID;
                src_lc.dwControl = ctrl_type;
                src_lc.cControls = 1;
                src_lc.cbmxctrl = mcSize;

                retval = mixerGetLineControls(mixer, ref src_lc, MIXER_GETLINECONTROLSF_ONEBYTYPE);
                if (retval != MMSYSERR_NOERROR)
                {
                    Marshal.FreeCoTaskMem(src_lc.pamxctrl);
                    return false;
                }

                MIXERCONTROL ctrl = new MIXERCONTROL();
                ctrl.cbStruct = mcSize;
                ctrl = (MIXERCONTROL)Marshal.PtrToStructure(src_lc.pamxctrl, typeof(MIXERCONTROL));
                outCtrl = ctrl;
                Marshal.FreeCoTaskMem(src_lc.pamxctrl);
                return true;
            }
        }

        private static bool GetControlByType(int mixer, uint dst_type, uint src_type, uint ctrl_type, out MIXERCONTROL outCtrl)
        {
            outCtrl = new MIXERCONTROL();
            MIXERCAPS mc = new MIXERCAPS();
            int retval = mixerGetDevCaps(mixer, ref mc, Marshal.SizeOf(mc));
            if (retval != MMSYSERR_NOERROR)
                return false;

            int num_dest = mc.cDestinations;
            for (int j = 0; j < num_dest; j++)
            {
                MIXERLINE dst_line = new MIXERLINE();
                dst_line.cbStruct = Marshal.SizeOf(dst_line);
                dst_line.dwDestination = j;
                retval = mixerGetLineInfo(mixer, ref dst_line, MIXER_GETLINEINFOF_DESTINATION);
                if (retval == MMSYSERR_NOERROR && dst_line.dwComponentType == dst_type)
                {
                    if (src_type == 0)
                    {
                        MIXERLINECONTROLS dst_lc = new MIXERLINECONTROLS();
                        int mcSize = 152;
                        dst_lc.pamxctrl = Marshal.AllocCoTaskMem(mcSize);
                        dst_lc.cbStruct = Marshal.SizeOf(dst_lc);
                        dst_lc.dwLineID = dst_line.dwLineID;
                        dst_lc.dwControl = ctrl_type;
                        dst_lc.cControls = 1;
                        dst_lc.cbmxctrl = mcSize;

                        retval = mixerGetLineControls(mixer, ref dst_lc, MIXER_GETLINECONTROLSF_ONEBYTYPE);
                        if (retval == MMSYSERR_NOERROR)
                        {
                            MIXERCONTROL ctrl = new MIXERCONTROL();
                            ctrl.cbStruct = mcSize;
                            ctrl = (MIXERCONTROL)Marshal.PtrToStructure(dst_lc.pamxctrl, typeof(MIXERCONTROL));
                            outCtrl = ctrl;
                            Marshal.FreeCoTaskMem(dst_lc.pamxctrl);
                            return true;
                        }
                        Marshal.FreeCoTaskMem(dst_lc.pamxctrl);
                        return false;
                    }
                    else
                    {
                        int num_src = dst_line.cConnections;
                        for (int k = 0; k < num_src; k++)
                        {
                            MIXERLINE src_line = new MIXERLINE();
                            src_line.cbStruct = dst_line.cbStruct;
                            src_line.dwDestination = dst_line.dwDestination;
                            src_line.dwSource = k;
                            retval = mixerGetLineInfo(mixer, ref src_line, MIXER_GETLINEINFOF_SOURCE);
                            if (retval == MMSYSERR_NOERROR && src_line.dwComponentType == src_type)
                            {
                                MIXERLINECONTROLS src_lc = new MIXERLINECONTROLS();
                                int mcSize = 152;
                                src_lc.pamxctrl = Marshal.AllocCoTaskMem(mcSize);
                                src_lc.cbStruct = Marshal.SizeOf(src_lc);
                                src_lc.dwLineID = src_line.dwLineID;
                                src_lc.dwControl = ctrl_type;
                                src_lc.cControls = 1;
                                src_lc.cbmxctrl = mcSize;

                                retval = mixerGetLineControls(mixer, ref src_lc, MIXER_GETLINECONTROLSF_ONEBYTYPE);
                                if (retval == MMSYSERR_NOERROR)
                                {
                                    MIXERCONTROL ctrl = new MIXERCONTROL();
                                    ctrl.cbStruct = mcSize;
                                    ctrl = (MIXERCONTROL)Marshal.PtrToStructure(src_lc.pamxctrl, typeof(MIXERCONTROL));
                                    outCtrl = ctrl;
                                    Marshal.FreeCoTaskMem(src_lc.pamxctrl);
                                    return true;
                                }
                                Marshal.FreeCoTaskMem(src_lc.pamxctrl);
                                return false;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static int GetVolume(int mixerID, uint dst_type, uint src_type)
        {
            int mixer;
            int val;
            MIXERCONTROL volCtrl = new MIXERCONTROL();

            mixerOpen(out mixer, mixerID, 0, 0, 0);
            bool success = GetControlByType(mixer, dst_type, src_type, MIXERCONTROL_CONTROLTYPE_VOLUME, out volCtrl);
            if (success == true)
            {
                MIXERCONTROLDETAILS details = new MIXERCONTROLDETAILS();

                int sizeofMIXERCONTROLDETAILS = Marshal.SizeOf(typeof(MIXERCONTROLDETAILS));
                int sizeofMIXERCONTROLDETAILS_UNSIGNED = Marshal.SizeOf(typeof(MIXERCONTROLDETAILS_UNSIGNED));
                details.cbStruct = sizeofMIXERCONTROLDETAILS;
                details.dwControlID = volCtrl.dwControlID;
                details.paDetails = Marshal.AllocCoTaskMem(sizeofMIXERCONTROLDETAILS_UNSIGNED);
                details.cChannels = 1;
                details.item = 0;
                details.cbDetails = sizeofMIXERCONTROLDETAILS_UNSIGNED;

                int retval = mixerGetControlDetails(mixer, ref details, MIXER_GETCONTROLDETAILSF_VALUE);
                if (retval == MMSYSERR_NOERROR)
                {
                    MIXERCONTROLDETAILS_UNSIGNED du = (MIXERCONTROLDETAILS_UNSIGNED)Marshal.PtrToStructure(details.paDetails, typeof(MIXERCONTROLDETAILS_UNSIGNED));
                    val = du.dwValue;
                }
                else val = -1;
                Marshal.FreeCoTaskMem(details.paDetails);
            }
            else val = -1;
            mixerClose(mixer);
            return val;
        }

        private static bool SetVolume(int mixerID, uint dst_type, uint src_type, int val)
        {
            int mixer;
            MIXERCONTROL volCtrl = new MIXERCONTROL();
            int currentVol = GetVolume(mixerID, dst_type, src_type);
            if (currentVol == val) return true;

            mixerOpen(out mixer, mixerID, 0, 0, 0);
            bool success = GetControlByType(mixer, dst_type, src_type, MIXERCONTROL_CONTROLTYPE_VOLUME, out volCtrl);
            if (success == true)
            {
                if (val > volCtrl.lMaximum) val = volCtrl.lMaximum;
                if (val < volCtrl.lMinimum) val = volCtrl.lMinimum;

                MIXERCONTROLDETAILS mxcd = new MIXERCONTROLDETAILS();
                MIXERCONTROLDETAILS_UNSIGNED vol = new MIXERCONTROLDETAILS_UNSIGNED();

                mxcd.item = 0;
                mxcd.dwControlID = volCtrl.dwControlID;
                mxcd.cbStruct = Marshal.SizeOf(mxcd);
                mxcd.cbDetails = Marshal.SizeOf(vol);

                mxcd.cChannels = 1;
                vol.dwValue = val;

                mxcd.paDetails = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(MIXERCONTROLDETAILS_UNSIGNED)));
                Marshal.StructureToPtr(vol, mxcd.paDetails, false);

                int retval = mixerSetControlDetails(mixer, ref mxcd, MIXER_SETCONTROLDETAILSF_VALUE);
                if (retval == MMSYSERR_NOERROR)
                {
                    currentVol = GetVolume(mixerID, dst_type, src_type);
                    if (currentVol != val)
                        success = false;
                    else success = true;
                }
                else success = false;
                Marshal.FreeCoTaskMem(mxcd.paDetails);
            }
            else success = false;
            mixerClose(mixer);
            return success;
        }

        private static bool GetMute(int mixerID, uint dst_type, uint src_type)
        {
            int mixer;
            MIXERCONTROL muteCtrl = new MIXERCONTROL();
            int val;

            mixerOpen(out mixer, mixerID, 0, 0, 0);
            bool success = GetControlByType(mixer, dst_type, src_type, MIXERCONTROL_CONTROLTYPE_MUTE, out muteCtrl);
            if (success == true)
            {
                MIXERCONTROLDETAILS details = new MIXERCONTROLDETAILS();

                int sizeofMIXERCONTROLDETAILS = Marshal.SizeOf(typeof(MIXERCONTROLDETAILS));
                int sizeofMIXERCONTROLDETAILS_UNSIGNED = Marshal.SizeOf(typeof(MIXERCONTROLDETAILS_UNSIGNED));
                details.cbStruct = sizeofMIXERCONTROLDETAILS;
                details.dwControlID = muteCtrl.dwControlID;
                details.paDetails = Marshal.AllocCoTaskMem(sizeofMIXERCONTROLDETAILS_UNSIGNED);
                details.cChannels = 1;
                details.item = 0;
                details.cbDetails = sizeofMIXERCONTROLDETAILS_UNSIGNED;

                int retval = mixerGetControlDetails(mixer, ref details, MIXER_GETCONTROLDETAILSF_VALUE);
                if (retval == MMSYSERR_NOERROR)
                {
                    MIXERCONTROLDETAILS_UNSIGNED du = (MIXERCONTROLDETAILS_UNSIGNED)Marshal.PtrToStructure(details.paDetails, typeof(MIXERCONTROLDETAILS_UNSIGNED));
                    val = du.dwValue;
                }
                else val = -1;
                Marshal.FreeCoTaskMem(details.paDetails);
            }
            else val = -1;
            mixerClose(mixer);
            return (val == 1);
        }

        private static bool GetBool(int mixer, MIXERCONTROL ctrl, out bool b)
        {
            MIXERCONTROLDETAILS mxcd = new MIXERCONTROLDETAILS();

            int sizeofMIXERCONTROLDETAILS = Marshal.SizeOf(typeof(MIXERCONTROLDETAILS));
            int sizeofMIXERCONTROLDETAILS_UNSIGNED = Marshal.SizeOf(typeof(MIXERCONTROLDETAILS_UNSIGNED));
            mxcd.cbStruct = sizeofMIXERCONTROLDETAILS;
            mxcd.dwControlID = ctrl.dwControlID;
            mxcd.paDetails = Marshal.AllocCoTaskMem(sizeofMIXERCONTROLDETAILS_UNSIGNED);
            mxcd.cChannels = 1;
            mxcd.item = 0;
            mxcd.cbDetails = sizeofMIXERCONTROLDETAILS_UNSIGNED;

            int retval = mixerGetControlDetails(mixer, ref mxcd, MIXER_GETCONTROLDETAILSF_VALUE);
            if (retval != MMSYSERR_NOERROR)
            {
                Marshal.FreeCoTaskMem(mxcd.paDetails);
                b = false;
                return false;
            }

            MIXERCONTROLDETAILS_UNSIGNED du = (MIXERCONTROLDETAILS_UNSIGNED)Marshal.PtrToStructure(mxcd.paDetails, typeof(MIXERCONTROLDETAILS_UNSIGNED));
            if (du.dwValue == 1)
                b = true;
            else b = false;

            Marshal.FreeCoTaskMem(mxcd.paDetails);
            return true;
        }

        private static bool SetBool(int mixer, MIXERCONTROL ctrl, bool b)
        {
            bool current;
            if (!GetBool(mixer, ctrl, out current))
                return false;

            if (current == b)
                return true;

            MIXERCONTROLDETAILS mxcd = new MIXERCONTROLDETAILS();
            MIXERCONTROLDETAILS_UNSIGNED val = new MIXERCONTROLDETAILS_UNSIGNED();

            mxcd.item = 0;
            mxcd.dwControlID = ctrl.dwControlID;
            mxcd.cbStruct = Marshal.SizeOf(mxcd);
            mxcd.cbDetails = Marshal.SizeOf(val);

            mxcd.cChannels = 1;
            if (b) val.dwValue = 1;
            else val.dwValue = 0;

            mxcd.paDetails = Marshal.AllocCoTaskMem(Marshal.SizeOf(val));
            Marshal.StructureToPtr(val, mxcd.paDetails, false);

            int retval = mixerSetControlDetails(mixer, ref mxcd, MIXER_SETCONTROLDETAILSF_VALUE);
            if (retval != MMSYSERR_NOERROR)
            {
                Marshal.FreeCoTaskMem(mxcd.paDetails);
                return false;
            }
            Marshal.FreeCoTaskMem(mxcd.paDetails);

            if (!GetBool(mixer, ctrl, out current))
                return false;

            if (current != b)
                return false;
            else return true;
        }

        private static bool GetUnsigned(int mixer, MIXERCONTROL ctrl, out int num)
        {
            MIXERCONTROLDETAILS mxcd = new MIXERCONTROLDETAILS();
            num = 0;

            int sizeofMIXERCONTROLDETAILS = Marshal.SizeOf(mxcd);
            int sizeofMIXERCONTROLDETAILS_UNSIGNED = Marshal.SizeOf(typeof(MIXERCONTROLDETAILS_UNSIGNED));
            mxcd.cbStruct = sizeofMIXERCONTROLDETAILS;
            mxcd.dwControlID = ctrl.dwControlID;
            mxcd.paDetails = Marshal.AllocCoTaskMem(sizeofMIXERCONTROLDETAILS_UNSIGNED);
            mxcd.cChannels = 1;
            mxcd.item = 0;
            mxcd.cbDetails = sizeofMIXERCONTROLDETAILS_UNSIGNED;

            int retval = mixerGetControlDetails(mixer, ref mxcd, MIXER_GETCONTROLDETAILSF_VALUE);
            if (retval != MMSYSERR_NOERROR)
            {
                Marshal.FreeCoTaskMem(mxcd.paDetails);
                return false;
            }

            MIXERCONTROLDETAILS_UNSIGNED du =
                (MIXERCONTROLDETAILS_UNSIGNED)Marshal.PtrToStructure(mxcd.paDetails, typeof(MIXERCONTROLDETAILS_UNSIGNED));
            num = du.dwValue;

            Marshal.FreeCoTaskMem(mxcd.paDetails);
            return true;
        }

        private static bool SetUnsigned(int mixer, MIXERCONTROL ctrl, int num)
        {
            int current_val;
            if (!GetUnsigned(mixer, ctrl, out current_val))
                return false;

            if (current_val == num)
                return true;

            if (num > ctrl.lMaximum) num = ctrl.lMaximum;
            if (num < ctrl.lMinimum) num = ctrl.lMinimum;

            MIXERCONTROLDETAILS mxcd = new MIXERCONTROLDETAILS();
            MIXERCONTROLDETAILS_UNSIGNED val = new MIXERCONTROLDETAILS_UNSIGNED();

            mxcd.item = 0;
            mxcd.dwControlID = ctrl.dwControlID;
            mxcd.cbStruct = Marshal.SizeOf(mxcd);
            mxcd.cbDetails = Marshal.SizeOf(val);

            mxcd.cChannels = 1;
            val.dwValue = num;

            mxcd.paDetails = Marshal.AllocCoTaskMem(Marshal.SizeOf(val));
            Marshal.StructureToPtr(val, mxcd.paDetails, false);

            int retval = mixerSetControlDetails(mixer, ref mxcd, MIXER_SETCONTROLDETAILSF_VALUE);
            Marshal.FreeCoTaskMem(mxcd.paDetails);

            if (retval != MMSYSERR_NOERROR)
                return false;

            if (!GetUnsigned(mixer, ctrl, out current_val))
                return false;

            if (current_val == num)
                return true;
            else
                return false;
        }

        private static bool SetMute(int mixerID, uint dst_type, uint src_type, bool val)
        {
            int mixer;
            MIXERCONTROL muteCtrl = new MIXERCONTROL();
            bool currentMute = GetMute(mixerID, dst_type, src_type);
            if (currentMute == val) return true;

            mixerOpen(out mixer, mixerID, 0, 0, 0);
            bool success = GetControlByType(mixer, dst_type, src_type, MIXERCONTROL_CONTROLTYPE_MUTE, out muteCtrl);
            if (success == true)
            {

                MIXERCONTROLDETAILS mxcd = new MIXERCONTROLDETAILS();
                MIXERCONTROLDETAILS_UNSIGNED mute = new MIXERCONTROLDETAILS_UNSIGNED();

                mxcd.item = 0;
                mxcd.dwControlID = muteCtrl.dwControlID;
                mxcd.cbStruct = Marshal.SizeOf(mxcd);
                mxcd.cbDetails = Marshal.SizeOf(mute);

                mxcd.cChannels = 1;
                if (val) mute.dwValue = 1;
                else mute.dwValue = 0;

                mxcd.paDetails = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(MIXERCONTROLDETAILS_UNSIGNED)));
                Marshal.StructureToPtr(mute, mxcd.paDetails, false);

                int retval = mixerSetControlDetails(mixer, ref mxcd, MIXER_SETCONTROLDETAILSF_VALUE);
                if (retval == MMSYSERR_NOERROR)
                {
                    currentMute = GetMute(mixerID, dst_type, src_type);
                    if (currentMute != val)
                        success = false;
                    else success = true;
                }
                else success = false;
                Marshal.FreeCoTaskMem(mxcd.paDetails);
            }
            else success = false;
            mixerClose(mixer);
            return success;
        }

        public static string GetDevName(int mixer)
        {
            MIXERCAPS mc = new MIXERCAPS();
            int retval = mixerGetDevCaps(mixer, ref mc, Marshal.SizeOf(mc));
            if (retval != MMSYSERR_NOERROR)
                return "";

            return mc.szPname;
        }

        public static string GetLineName(int mixer, uint dst_type, uint src_type)
        {
            MIXERCAPS mc = new MIXERCAPS();
            int retval = mixerGetDevCaps(mixer, ref mc, Marshal.SizeOf(mc));
            if (retval != MMSYSERR_NOERROR)
                return "";

            int num_dest = mc.cDestinations;
            for (int j = 0; j < num_dest; j++)
            {
                MIXERLINE dst_line = new MIXERLINE();
                dst_line.cbStruct = Marshal.SizeOf(dst_line);
                dst_line.dwDestination = j;
                retval = mixerGetLineInfo(mixer, ref dst_line, MIXER_GETLINEINFOF_DESTINATION);
                if (retval == MMSYSERR_NOERROR && dst_line.dwComponentType == dst_type)
                {
                    if (src_type == 0)
                        return dst_line.szName;
                    else
                    {
                        int num_src = dst_line.cConnections;
                        for (int k = 0; k < num_src; k++)
                        {
                            MIXERLINE src_line = new MIXERLINE();
                            src_line.cbStruct = dst_line.cbStruct;
                            src_line.dwDestination = dst_line.dwDestination;
                            src_line.dwSource = k;
                            retval = mixerGetLineInfo(mixer, ref src_line, MIXER_GETLINEINFOF_SOURCE);
                            if (retval == MMSYSERR_NOERROR && src_line.dwComponentType == src_type)
                                return src_line.szName;
                        }
                    }
                }
            }
            return "";
        }

        private static ArrayList save;
        public static void SaveState()
        {
            save = new ArrayList();

            uint[] ctrl_list =
			{
				MIXERCONTROL_CONTROLTYPE_CUSTOM,
				MIXERCONTROL_CONTROLTYPE_BOOLEANMETER,
				MIXERCONTROL_CONTROLTYPE_SIGNEDMETER,
				MIXERCONTROL_CONTROLTYPE_PEAKMETER,
				MIXERCONTROL_CONTROLTYPE_BOOLEAN,
				MIXERCONTROL_CONTROLTYPE_ONOFF,
				MIXERCONTROL_CONTROLTYPE_MUTE,
				MIXERCONTROL_CONTROLTYPE_MONO,
				MIXERCONTROL_CONTROLTYPE_LOUDNESS,
				MIXERCONTROL_CONTROLTYPE_STEREOENH,
				MIXERCONTROL_CONTROLTYPE_BUTTON,
				MIXERCONTROL_CONTROLTYPE_DECIBELS,
				MIXERCONTROL_CONTROLTYPE_SIGNED,
				MIXERCONTROL_CONTROLTYPE_SLIDER,
				MIXERCONTROL_CONTROLTYPE_PAN,
				MIXERCONTROL_CONTROLTYPE_QSOUNDPAN,
				MIXERCONTROL_CONTROLTYPE_SINGLESELECT,
				MIXERCONTROL_CONTROLTYPE_MUX,
				MIXERCONTROL_CONTROLTYPE_MULTIPLESELECT,
				MIXERCONTROL_CONTROLTYPE_MIXER,
				MIXERCONTROL_CONTROLTYPE_UNSIGNEDMETER,
				MIXERCONTROL_CONTROLTYPE_UNSIGNED,
				MIXERCONTROL_CONTROLTYPE_BASS,
				MIXERCONTROL_CONTROLTYPE_EQUALIZER,
				MIXERCONTROL_CONTROLTYPE_FADER,
				MIXERCONTROL_CONTROLTYPE_TREBLE,
				MIXERCONTROL_CONTROLTYPE_VOLUME,
				MIXERCONTROL_CONTROLTYPE_MICROTIME,
				MIXERCONTROL_CONTROLTYPE_MILLITIME,
				MIXERCONTROL_CONTROLTYPE_PERCENT,
			};

            int num_mixers = mixerGetNumDevs();
            int mixer = -1;
            int retval = -1;

            for (int i = 0; i < num_mixers; i++)
            {
                mixerOpen(out mixer, i, 0, 0, 0);
                MIXERCAPS mc = new MIXERCAPS();

                retval = mixerGetDevCaps(mixer, ref mc, Marshal.SizeOf(mc));
                if (retval == MMSYSERR_NOERROR)
                {
                    int num_dest = mc.cDestinations;
                    for (int j = 0; j < num_dest; j++)
                    {
                        MIXERLINE dst_line = new MIXERLINE();
                        dst_line.cbStruct = Marshal.SizeOf(dst_line);
                        dst_line.dwDestination = j;

                        retval = mixerGetLineInfo(mixer, ref dst_line, MIXER_GETLINEINFOF_DESTINATION);
                        if (retval == MMSYSERR_NOERROR)
                        {
                            for (int k = 0; k < 30; k++)
                            {
                                if (ctrl_list[k] == MIXERCONTROL_CONTROLTYPE_MUX)
                                    retval = -1;
                                MIXERLINECONTROLS dst_lc = new MIXERLINECONTROLS();
                                int mcSize = 152;
                                dst_lc.pamxctrl = Marshal.AllocCoTaskMem(mcSize);
                                dst_lc.cbStruct = Marshal.SizeOf(dst_lc);
                                dst_lc.dwLineID = dst_line.dwLineID;
                                dst_lc.dwControl = ctrl_list[k];
                                dst_lc.cControls = 1;
                                dst_lc.cbmxctrl = mcSize;

                                retval = mixerGetLineControls(mixer, ref dst_lc, MIXER_GETLINECONTROLSF_ONEBYTYPE);
                                if (retval == MMSYSERR_NOERROR)
                                {
                                    MIXERCONTROL ctrl = new MIXERCONTROL();
                                    ctrl.cbStruct = mcSize;
                                    ctrl = (MIXERCONTROL)Marshal.PtrToStructure(dst_lc.pamxctrl, typeof(MIXERCONTROL));
                                    MIXERCONTROLDETAILS details = new MIXERCONTROLDETAILS();
                                    int size = Marshal.SizeOf(typeof(MIXERCONTROLDETAILS_UNSIGNED));
                                    details.dwControlID = ctrl.dwControlID;
                                    details.item = ctrl.cMultipleItems;
                                    details.cbDetails = size;
                                    if (details.item > 0) size *= ctrl.cMultipleItems;
                                    details.paDetails = Marshal.AllocCoTaskMem(size);
                                    details.cbStruct = Marshal.SizeOf(details);
                                    details.cChannels = 1;

                                    retval = mixerGetControlDetails(mixer, ref details, MIXER_GETCONTROLDETAILSF_VALUE);
                                    if (retval == MMSYSERR_NOERROR)
                                    {
                                        if (details.item > 0)
                                        {
                                            MIXERCONTROLDETAILS_UNSIGNED[] val = new MIXERCONTROLDETAILS_UNSIGNED[details.item];
                                            for (int m = 0; m < details.item; m++)
                                            {
                                                IntPtr ptr = new IntPtr(details.paDetails.ToInt32() + m * details.cbDetails);
                                                val[m] = new MIXERCONTROLDETAILS_UNSIGNED();
                                                val[m] = (MIXERCONTROLDETAILS_UNSIGNED)Marshal.PtrToStructure(ptr, typeof(MIXERCONTROLDETAILS_UNSIGNED));
                                                if (val[m].dwValue == 1)
                                                {
                                                    save.Add(m);
                                                    m = details.item;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            MIXERCONTROLDETAILS_UNSIGNED val = new MIXERCONTROLDETAILS_UNSIGNED();
                                            val = (MIXERCONTROLDETAILS_UNSIGNED)Marshal.PtrToStructure(details.paDetails, typeof(MIXERCONTROLDETAILS_UNSIGNED));
                                            save.Add(val.dwValue);
                                        }
                                    }
                                    Marshal.FreeCoTaskMem(details.paDetails);
                                }
                                Marshal.FreeCoTaskMem(dst_lc.pamxctrl);
                            }

                            int num_src = dst_line.cConnections;
                            for (int k = 0; k < num_src; k++)
                            {
                                MIXERLINE src_line = new MIXERLINE();
                                src_line.cbStruct = dst_line.cbStruct;
                                src_line.dwDestination = dst_line.dwDestination;
                                src_line.dwSource = k;

                                retval = mixerGetLineInfo(mixer, ref src_line, MIXER_GETLINEINFOF_SOURCE);
                                if (retval == MMSYSERR_NOERROR)
                                {
                                    for (int l = 0; l < 30; l++)
                                    {
                                        MIXERLINECONTROLS src_lc = new MIXERLINECONTROLS();
                                        int mcSize = 152;
                                        src_lc.pamxctrl = Marshal.AllocCoTaskMem(mcSize);
                                        src_lc.cbStruct = Marshal.SizeOf(src_lc);
                                        src_lc.dwLineID = src_line.dwLineID;
                                        src_lc.dwControl = ctrl_list[l];
                                        src_lc.cControls = 1;
                                        src_lc.cbmxctrl = mcSize;

                                        retval = mixerGetLineControls(mixer, ref src_lc, MIXER_GETLINECONTROLSF_ONEBYTYPE);
                                        if (retval == MMSYSERR_NOERROR)
                                        {
                                            MIXERCONTROL ctrl = new MIXERCONTROL();
                                            ctrl.cbStruct = mcSize;
                                            ctrl = (MIXERCONTROL)Marshal.PtrToStructure(src_lc.pamxctrl, typeof(MIXERCONTROL));
                                            MIXERCONTROLDETAILS details = new MIXERCONTROLDETAILS();
                                            int size = Marshal.SizeOf(typeof(MIXERCONTROLDETAILS_UNSIGNED));
                                            details.dwControlID = ctrl.dwControlID;
                                            details.paDetails = Marshal.AllocCoTaskMem(size);
                                            details.cbStruct = Marshal.SizeOf(details);
                                            details.cChannels = 1;
                                            details.item = 0;
                                            details.cbDetails = size;

                                            retval = mixerGetControlDetails(mixer, ref details, MIXER_GETCONTROLDETAILSF_VALUE);
                                            if (retval == MMSYSERR_NOERROR)
                                            {
                                                if (details.item > 0)
                                                {
                                                    MIXERCONTROLDETAILS_UNSIGNED[] val = new MIXERCONTROLDETAILS_UNSIGNED[details.item];
                                                    for (int m = 0; m < details.item; m++)
                                                    {
                                                        IntPtr ptr = new IntPtr(details.paDetails.ToInt32() + m * details.cbDetails);
                                                        val[m] = new MIXERCONTROLDETAILS_UNSIGNED();
                                                        val[m] = (MIXERCONTROLDETAILS_UNSIGNED)Marshal.PtrToStructure(ptr, typeof(MIXERCONTROLDETAILS_UNSIGNED));
                                                        if (val[m].dwValue == 1)
                                                        {
                                                            save.Add(m);
                                                            m = details.item;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    MIXERCONTROLDETAILS_UNSIGNED val = new MIXERCONTROLDETAILS_UNSIGNED();
                                                    val = (MIXERCONTROLDETAILS_UNSIGNED)Marshal.PtrToStructure(details.paDetails, typeof(MIXERCONTROLDETAILS_UNSIGNED));
                                                    save.Add(val.dwValue);
                                                }
                                            }
                                            Marshal.FreeCoTaskMem(details.paDetails);
                                        }
                                        Marshal.FreeCoTaskMem(src_lc.pamxctrl);
                                    }
                                }
                            }
                        }
                    }
                }
                mixerClose(mixer);
            }
        }

        public static void RestoreState()
        {
            int index = 0;
            uint[] ctrl_list =
			{
				MIXERCONTROL_CONTROLTYPE_CUSTOM,
				MIXERCONTROL_CONTROLTYPE_BOOLEANMETER,
				MIXERCONTROL_CONTROLTYPE_SIGNEDMETER,
				MIXERCONTROL_CONTROLTYPE_PEAKMETER,
				MIXERCONTROL_CONTROLTYPE_BOOLEAN,
				MIXERCONTROL_CONTROLTYPE_ONOFF,
				MIXERCONTROL_CONTROLTYPE_MUTE,
				MIXERCONTROL_CONTROLTYPE_MONO,
				MIXERCONTROL_CONTROLTYPE_LOUDNESS,
				MIXERCONTROL_CONTROLTYPE_STEREOENH,
				MIXERCONTROL_CONTROLTYPE_BUTTON,
				MIXERCONTROL_CONTROLTYPE_DECIBELS,
				MIXERCONTROL_CONTROLTYPE_SIGNED,
				MIXERCONTROL_CONTROLTYPE_SLIDER,
				MIXERCONTROL_CONTROLTYPE_PAN,
				MIXERCONTROL_CONTROLTYPE_QSOUNDPAN,
				MIXERCONTROL_CONTROLTYPE_SINGLESELECT,
				MIXERCONTROL_CONTROLTYPE_MUX,
				MIXERCONTROL_CONTROLTYPE_MULTIPLESELECT,
				MIXERCONTROL_CONTROLTYPE_MIXER,
				MIXERCONTROL_CONTROLTYPE_UNSIGNEDMETER,
				MIXERCONTROL_CONTROLTYPE_UNSIGNED,
				MIXERCONTROL_CONTROLTYPE_BASS,
				MIXERCONTROL_CONTROLTYPE_EQUALIZER,
				MIXERCONTROL_CONTROLTYPE_FADER,
				MIXERCONTROL_CONTROLTYPE_TREBLE,
				MIXERCONTROL_CONTROLTYPE_VOLUME,
				MIXERCONTROL_CONTROLTYPE_MICROTIME,
				MIXERCONTROL_CONTROLTYPE_MILLITIME,
				MIXERCONTROL_CONTROLTYPE_PERCENT,
			};

            int num_mixers = mixerGetNumDevs();
            int mixer = -1;
            int retval = -1;

            for (int i = 0; i < num_mixers; i++)
            {
                mixerOpen(out mixer, i, 0, 0, 0);
                MIXERCAPS mc = new MIXERCAPS();

                retval = mixerGetDevCaps(mixer, ref mc, Marshal.SizeOf(mc));
                if (retval == MMSYSERR_NOERROR)
                {
                    int num_dest = mc.cDestinations;
                    for (int j = 0; j < num_dest; j++)
                    {
                        MIXERLINE dst_line = new MIXERLINE();
                        dst_line.cbStruct = Marshal.SizeOf(dst_line);
                        dst_line.dwDestination = j;

                        retval = mixerGetLineInfo(mixer, ref dst_line, MIXER_GETLINEINFOF_DESTINATION);
                        if (retval == MMSYSERR_NOERROR)
                        {
                            for (int k = 0; k < 30; k++)
                            {
                                MIXERLINECONTROLS dst_lc = new MIXERLINECONTROLS();
                                int mcSize = 152;
                                dst_lc.pamxctrl = Marshal.AllocCoTaskMem(mcSize);
                                dst_lc.cbStruct = Marshal.SizeOf(dst_lc);
                                dst_lc.dwLineID = dst_line.dwLineID;
                                dst_lc.dwControl = ctrl_list[k];
                                dst_lc.cControls = 1;
                                dst_lc.cbmxctrl = mcSize;

                                retval = mixerGetLineControls(mixer, ref dst_lc, MIXER_GETLINECONTROLSF_ONEBYTYPE);
                                if (retval == MMSYSERR_NOERROR)
                                {
                                    MIXERCONTROL ctrl = new MIXERCONTROL();
                                    ctrl.cbStruct = mcSize;
                                    ctrl = (MIXERCONTROL)Marshal.PtrToStructure(dst_lc.pamxctrl, typeof(MIXERCONTROL));
                                    MIXERCONTROLDETAILS details = new MIXERCONTROLDETAILS();
                                    int size = Marshal.SizeOf(typeof(MIXERCONTROLDETAILS_SIGNED));
                                    details.dwControlID = ctrl.dwControlID;
                                    details.cbDetails = size;
                                    details.item = ctrl.cMultipleItems;
                                    if (details.item > 0) size *= details.item;
                                    details.paDetails = Marshal.AllocCoTaskMem(size);
                                    details.cbStruct = Marshal.SizeOf(details);
                                    details.cChannels = 1;

                                    if (details.item > 0)
                                    {
                                        if (index != save.Count)
                                        {
                                            int rec_index = (int)save[index];
                                            int[] val = new int[details.item];
                                            for (int m = 0; m < details.item; m++)
                                            {
                                                if (m == rec_index) val[m] = 1;
                                                else val[m] = 0;
                                            }
                                            Marshal.Copy(val, 0, details.paDetails, details.item);

                                            retval = mixerSetControlDetails(mixer, ref details, MIXER_GETCONTROLDETAILSF_VALUE);
                                            if (retval == MMSYSERR_NOERROR)
                                                index++;
                                        }
                                    }
                                    else
                                    {
                                        MIXERCONTROLDETAILS_UNSIGNED val = new MIXERCONTROLDETAILS_UNSIGNED();
                                        val.dwValue = (int)save[index];
                                        Marshal.StructureToPtr(val, details.paDetails, false);

                                        retval = mixerSetControlDetails(mixer, ref details, MIXER_GETCONTROLDETAILSF_VALUE);
                                        if (retval == MMSYSERR_NOERROR)
                                            index++;
                                    }
                                    Marshal.FreeCoTaskMem(details.paDetails);
                                }
                                Marshal.FreeCoTaskMem(dst_lc.pamxctrl);
                            }

                            int num_src = dst_line.cConnections;
                            for (int k = 0; k < num_src; k++)
                            {
                                MIXERLINE src_line = new MIXERLINE();
                                src_line.cbStruct = dst_line.cbStruct;
                                src_line.dwDestination = dst_line.dwDestination;
                                src_line.dwSource = k;

                                retval = mixerGetLineInfo(mixer, ref src_line, MIXER_GETLINEINFOF_SOURCE);
                                if (retval == MMSYSERR_NOERROR)
                                {
                                    for (int l = 0; l < 30; l++)
                                    {
                                        MIXERLINECONTROLS src_lc = new MIXERLINECONTROLS();
                                        int mcSize = 152;
                                        src_lc.pamxctrl = Marshal.AllocCoTaskMem(mcSize);
                                        src_lc.cbStruct = Marshal.SizeOf(src_lc);
                                        src_lc.dwLineID = src_line.dwLineID;
                                        src_lc.dwControl = ctrl_list[l];
                                        src_lc.cControls = 1;
                                        src_lc.cbmxctrl = mcSize;

                                        retval = mixerGetLineControls(mixer, ref src_lc, MIXER_GETLINECONTROLSF_ONEBYTYPE);
                                        if (retval == MMSYSERR_NOERROR)
                                        {
                                            MIXERCONTROL ctrl = new MIXERCONTROL();
                                            ctrl.cbStruct = mcSize;
                                            ctrl = (MIXERCONTROL)Marshal.PtrToStructure(src_lc.pamxctrl, typeof(MIXERCONTROL));
                                            MIXERCONTROLDETAILS details = new MIXERCONTROLDETAILS();
                                            int size;
                                            if (l < 20) size = Marshal.SizeOf(typeof(MIXERCONTROLDETAILS_SIGNED));
                                            else size = Marshal.SizeOf(typeof(MIXERCONTROLDETAILS_UNSIGNED));
                                            details.dwControlID = ctrl.dwControlID;
                                            details.paDetails = Marshal.AllocCoTaskMem(size);
                                            details.cbStruct = Marshal.SizeOf(details);
                                            details.cChannels = 1;
                                            details.item = ctrl.cMultipleItems;
                                            details.cbDetails = size;

                                            if (details.item > 0)
                                            {
                                                int rec_index = (int)save[index];
                                                MIXERCONTROLDETAILS_UNSIGNED[] val = new MIXERCONTROLDETAILS_UNSIGNED[details.item];
                                                for (int m = 0; m < details.item; m++)
                                                    val[m] = new MIXERCONTROLDETAILS_UNSIGNED();
                                                val[rec_index].dwValue = 1;
                                                Marshal.StructureToPtr(val[0], details.paDetails, false);
                                            }
                                            else
                                            {
                                                MIXERCONTROLDETAILS_UNSIGNED val = new MIXERCONTROLDETAILS_UNSIGNED();
                                                val.dwValue = (int)save[index];
                                                Marshal.StructureToPtr(val, details.paDetails, false);
                                            }

                                            retval = mixerSetControlDetails(mixer, ref details, MIXER_GETCONTROLDETAILSF_VALUE);
                                            if (retval == MMSYSERR_NOERROR)
                                                index++;

                                            Marshal.FreeCoTaskMem(details.paDetails);
                                        }
                                        Marshal.FreeCoTaskMem(src_lc.pamxctrl);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            mixerClose(mixer);
        }

        public static void SaveStateText()
        {
            StreamWriter sw = new StreamWriter("mixer_state.txt", false);

            foreach (int i in save)
            {
                sw.WriteLine(i.ToString());
            }
            sw.Close();
        }

        public static void ReadStateText()
        {
            if (!File.Exists("mixer_state.txt"))
                return;

            StreamReader sr = new StreamReader("mixer_state.txt");
            save = new ArrayList();
            string input = "";
            while ((input = sr.ReadLine()) != null)
            {
                save.Add(Int32.Parse(input));
            }
            sr.Close();
        }

        public static int GetMainVolume(int mixerID)
        {
            int val = GetVolume(mixerID, MIXERLINE_COMPONENTTYPE_DST_SPEAKERS, 0);
            if (val == -1) return -1;
            else return val * 100 / UInt16.MaxValue;
        }

        public static int GetWaveOutVolume(int mixerID)
        {
            int val = GetVolume(mixerID, MIXERLINE_COMPONENTTYPE_DST_SPEAKERS,
                MIXERLINE_COMPONENTTYPE_SRC_WAVEOUT);
            if (val == -1) return -1;
            else return val * 100 / UInt16.MaxValue;
        }

        public static int GetLineInPlayVolume(int mixerID)
        {
            int val = GetVolume(mixerID, MIXERLINE_COMPONENTTYPE_DST_SPEAKERS,
                MIXERLINE_COMPONENTTYPE_SRC_LINE);
            if (val == -1) return -1;
            else return val * 100 / UInt16.MaxValue;
        }

        public static int GetLineInRecordVolume(int mixerID)
        {
            int val = GetVolume(mixerID, MIXERLINE_COMPONENTTYPE_DST_WAVEIN,
                MIXERLINE_COMPONENTTYPE_SRC_LINE);
            if (val == -1) return -1;
            else return val * 100 / UInt16.MaxValue;
        }

        public static int GetMicPlayVolume(int mixerID)
        {
            int val = GetVolume(mixerID, MIXERLINE_COMPONENTTYPE_DST_SPEAKERS,
                MIXERLINE_COMPONENTTYPE_SRC_MICROPHONE);
            if (val == -1) return -1;
            else return val * 100 / UInt16.MaxValue;
        }

        public static int GetMicRecordVolume(int mixerID)
        {
            int val = GetVolume(mixerID, MIXERLINE_COMPONENTTYPE_DST_WAVEIN,
                MIXERLINE_COMPONENTTYPE_SRC_MICROPHONE);
            if (val == -1) return -1;
            else return val * 100 / UInt16.MaxValue;
        }

        public static bool SetMainVolume(int mixerID, int percent)
        {
            return SetVolume(mixerID, MIXERLINE_COMPONENTTYPE_DST_SPEAKERS, 0, UInt16.MaxValue * percent / 100);
        }

        public static bool SetWaveOutVolume(int mixerID, int percent)
        {
            return SetVolume(mixerID, MIXERLINE_COMPONENTTYPE_DST_SPEAKERS,
                MIXERLINE_COMPONENTTYPE_SRC_WAVEOUT,
                UInt16.MaxValue * percent / 100);
        }

        public static bool SetWaveInVolume(int mixerID, int percent)
        {
            return SetVolume(mixerID, MIXERLINE_COMPONENTTYPE_DST_WAVEIN,
                MIXERLINE_COMPONENTTYPE_SRC_WAVEOUT,
                UInt16.MaxValue * percent / 100);
        }

        public static bool SetLineInPlayVolume(int mixerID, int percent)
        {
            return SetVolume(mixerID, MIXERLINE_COMPONENTTYPE_DST_SPEAKERS,
                MIXERLINE_COMPONENTTYPE_SRC_LINE,
                UInt16.MaxValue * percent / 100);
        }

        public static bool SetLineInRecordVolume(int mixerID, int percent)
        {
            return SetVolume(mixerID, MIXERLINE_COMPONENTTYPE_DST_WAVEIN,
                MIXERLINE_COMPONENTTYPE_SRC_LINE,
                UInt16.MaxValue * percent / 100);
        }

        public static bool SetMicPlayVolume(int mixerID, int percent)
        {
            if (GetDevName(mixerID) == "SB Audigy Audio [BC00]")
            {
                return SetVolume(mixerID, MIXERLINE_COMPONENTTYPE_DST_SPEAKERS,
                    MIXERLINE_COMPONENTTYPE_SRC_LINE,
                    UInt16.MaxValue * percent / 100);
            }
            else
            {
                return SetVolume(mixerID, MIXERLINE_COMPONENTTYPE_DST_SPEAKERS,
                    MIXERLINE_COMPONENTTYPE_SRC_MICROPHONE,
                    UInt16.MaxValue * percent / 100);
            }
        }

        public static bool SetMicRecordVolume(int mixerID, int percent)
        {
            if (GetDevName(mixerID) == "SB Audigy Audio [BC00]")
            {
                return SetVolume(mixerID, MIXERLINE_COMPONENTTYPE_DST_WAVEIN,
                    MIXERLINE_COMPONENTTYPE_SRC_LINE,
                    UInt16.MaxValue * percent / 100);
            }
            else
            {
                return SetVolume(mixerID, MIXERLINE_COMPONENTTYPE_DST_WAVEIN,
                    MIXERLINE_COMPONENTTYPE_SRC_MICROPHONE,
                    UInt16.MaxValue * percent / 100);
            }
        }

        public static bool GetMainMute(int mixerID)
        {
            return GetMute(mixerID, MIXERLINE_COMPONENTTYPE_DST_SPEAKERS, 0);
        }

        public static bool GetWaveOutMute(int mixerID)
        {
            return GetMute(mixerID, MIXERLINE_COMPONENTTYPE_DST_SPEAKERS,
                MIXERLINE_COMPONENTTYPE_SRC_WAVEOUT);
        }

        public static bool GetLineInMute(int mixerID)
        {
            return GetMute(mixerID, MIXERLINE_COMPONENTTYPE_DST_SPEAKERS,
                MIXERLINE_COMPONENTTYPE_SRC_LINE);
        }

        public static bool GetMicMute(int mixerID)
        {
            return GetMute(mixerID, MIXERLINE_COMPONENTTYPE_DST_SPEAKERS,
                MIXERLINE_COMPONENTTYPE_SRC_MICROPHONE);
        }

        public static bool SetMainMute(int mixerID, bool val)
        {
            return SetMute(mixerID, MIXERLINE_COMPONENTTYPE_DST_SPEAKERS, 0, val);
        }

        public static bool SetWaveOutMute(int mixerID, bool val)
        {
            return SetMute(mixerID, MIXERLINE_COMPONENTTYPE_DST_SPEAKERS,
                MIXERLINE_COMPONENTTYPE_SRC_WAVEOUT, val);
        }

        public static bool SetLineInMute(int mixerID, bool val)
        {
            return SetMute(mixerID, MIXERLINE_COMPONENTTYPE_DST_SPEAKERS,
                MIXERLINE_COMPONENTTYPE_SRC_LINE, val);
        }

        public static bool SetMicMute(int mixerID, bool val)
        {
            return SetMute(mixerID, MIXERLINE_COMPONENTTYPE_DST_SPEAKERS,
                MIXERLINE_COMPONENTTYPE_SRC_MICROPHONE, val);
        }

        public static int GetNumMuxLines(int mixerID)
        {
            int mixer;
            MIXERCONTROL ctrl = new MIXERCONTROL();

            mixerOpen(out mixer, mixerID, 0, 0, 0);
            bool response = GetControlByType(mixer, MIXERLINE_COMPONENTTYPE_DST_WAVEIN, 0, MIXERCONTROL_CONTROLTYPE_MUX, out ctrl);
            mixerClose(mixer);

            if (response)
                return ctrl.cMultipleItems;
            else
                return -1;
        }

        public static bool GetMuxLineNames(int mixerID, out ArrayList a)
        {
            int mixer;
            MIXERCONTROL ctrl = new MIXERCONTROL();
            a = new ArrayList();

            mixerOpen(out mixer, mixerID, 0, 0, 0);
            GetControlByType(mixer, MIXERLINE_COMPONENTTYPE_DST_WAVEIN, 0, MIXERCONTROL_CONTROLTYPE_MUX, out ctrl);

            int n = ctrl.cMultipleItems;
            MIXERCONTROLDETAILS mxcd = new MIXERCONTROLDETAILS();
            mxcd.cbStruct = Marshal.SizeOf(mxcd);
            mxcd.dwControlID = ctrl.dwControlID;
            mxcd.cChannels = 1;
            mxcd.item = n;

            int size = Marshal.SizeOf(typeof(MIXERCONTROLDETAILS_LISTTEXT)) * n;
            mxcd.cbDetails = Marshal.SizeOf(typeof(MIXERCONTROLDETAILS_LISTTEXT));
            mxcd.paDetails = Marshal.AllocCoTaskMem(size);

            int result = mixerGetControlDetails(mixer, ref mxcd, MIXER_GETCONTROLDETAILSF_LISTTEXT);

            if (result != MMSYSERR_NOERROR)
            {
                mixerClose(mixer);
                return false;
            }

            for (int i = 0; i < mxcd.item; i++)
            {
                MIXERCONTROLDETAILS_LISTTEXT ltxt = new MIXERCONTROLDETAILS_LISTTEXT();
                IntPtr ptr = new IntPtr(mxcd.paDetails.ToInt32() + i * mxcd.cbDetails);

                ltxt = (MIXERCONTROLDETAILS_LISTTEXT)Marshal.PtrToStructure(ptr, typeof(MIXERCONTROLDETAILS_LISTTEXT));
                a.Add(ltxt.szName);
            }
            mixerClose(mixer);
            return true;
        }

        public static int GetMux(int mixerID)
        {
            int mixer;
            MIXERCONTROL ctrl = new MIXERCONTROL();

            mixerOpen(out mixer, mixerID, 0, 0, 0);
            GetControlByType(mixer, MIXERLINE_COMPONENTTYPE_DST_WAVEIN, 0, MIXERCONTROL_CONTROLTYPE_MUX, out ctrl);

            MIXERCONTROLDETAILS details = new MIXERCONTROLDETAILS();
            int size = Marshal.SizeOf(typeof(MIXERCONTROLDETAILS_UNSIGNED));
            details.dwControlID = ctrl.dwControlID;
            details.item = ctrl.cMultipleItems;
            details.cbDetails = size;
            if (details.item > 0) size *= ctrl.cMultipleItems;
            details.paDetails = Marshal.AllocCoTaskMem(size);
            details.cbStruct = Marshal.SizeOf(details);
            details.cChannels = 1;

            int retval = mixerGetControlDetails(mixer, ref details, MIXER_GETCONTROLDETAILSF_VALUE);
            if (retval == MMSYSERR_NOERROR)
            {
                if (details.item > 0)
                {
                    MIXERCONTROLDETAILS_UNSIGNED[] val = new MIXERCONTROLDETAILS_UNSIGNED[details.item];
                    for (int m = 0; m < details.item; m++)
                    {
                        IntPtr ptr = new IntPtr(details.paDetails.ToInt32() + m * details.cbDetails);
                        val[m] = new MIXERCONTROLDETAILS_UNSIGNED();
                        val[m] = (MIXERCONTROLDETAILS_UNSIGNED)Marshal.PtrToStructure(ptr, typeof(MIXERCONTROLDETAILS_UNSIGNED));
                        if (val[m].dwValue == 1)
                        {
                            retval = m;
                            m = details.item;
                        }
                    }
                }
            }
            mixerClose(mixer);
            return retval;
        }

        public static bool SetMux(int mixerID, int index)
        {
            int mixer;
            int retval = -1;
            MIXERCONTROL ctrl = new MIXERCONTROL();

            mixerOpen(out mixer, mixerID, 0, 0, 0);
            GetControlByType(mixer, MIXERLINE_COMPONENTTYPE_DST_WAVEIN, 0, MIXERCONTROL_CONTROLTYPE_MUX, out ctrl);

            MIXERCONTROLDETAILS details = new MIXERCONTROLDETAILS();
            int size = Marshal.SizeOf(typeof(MIXERCONTROLDETAILS_SIGNED));
            details.dwControlID = ctrl.dwControlID;
            details.cbDetails = size;
            details.item = ctrl.cMultipleItems;
            if (details.item > 0) size *= details.item;
            details.paDetails = Marshal.AllocCoTaskMem(size);
            details.cbStruct = Marshal.SizeOf(details);
            details.cChannels = 1;

            if (details.item > 0)
            {
                int[] val = new int[details.item];
                for (int m = 0; m < details.item; m++)
                {
                    if (m == index) val[m] = 1;
                    else val[m] = 0;
                }
                Marshal.Copy(val, 0, details.paDetails, details.item);

                retval = mixerSetControlDetails(mixer, ref details, MIXER_GETCONTROLDETAILSF_VALUE);
            }
            if (retval == MMSYSERR_NOERROR)
                return true;
            else return false;
        }
    }
}
