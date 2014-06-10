using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Tuat.Hands
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tagRecognitionCandidate
    {
        // 対象の文字文字パターン
        public int Start;        // 文字パターンの先頭ストローク
        public int End;          // 文字パターンの終端ストローク　

        // 認識結果
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 81)]
        public byte[] Result;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public double[] Score;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public ushort[] Code;

        // 字形情報
        public int BoundsLeft;
        public int BoundsTop;
        public int BoundsRight;
        public int BoundsBottom;
    };


    [StructLayout(LayoutKind.Sequential)]
    public struct tagRecognitionResult
    {
	    public IntPtr		Result;
	    public IntPtr       Candidate;
	    public int			Count;
    };

    public class RecognitionCandidate
    {
        // 対象の文字文字パターン
        public int Start;        // 文字パターンの先頭ストローク
        public int End;          // 文字パターンの終端ストローク　

        // 認識結果
        public string Result;
        public double[] Score;
        public ushort[] Code;

        // 字形情報
        Rectangle Bounds;

        public RecognitionCandidate(tagRecognitionCandidate rc)
        {
            Start = rc.Start;
            End = rc.End;

            Score = new double[rc.Score.Length];
            rc.Score.CopyTo(Score, 0);
            Code = new ushort[rc.Code.Length];
            rc.Code.CopyTo(Code, 0);

            Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
            Result = sjisEnc.GetString(rc.Result);
            Result = Result.Substring(0, Result.Length - 1);
            Bounds = new Rectangle(rc.BoundsLeft, rc.BoundsTop,
                        rc.BoundsRight - rc.BoundsLeft, rc.BoundsBottom - rc.BoundsTop);
        }
    }


    static public class InkRecognizer
    {
        [DllImport("HandsInkRecognizer.dll")]
        private extern static bool HandsInkInitialize();
        [DllImport("HandsInkRecognizer.dll")]
        private extern static bool HandsInkInitializeDirectory(byte[] directoryPathName);
        [DllImport("HandsInkRecognizer.dll")]
        private extern static void HandsInkClose();
        [DllImport("HandsInkRecognizer.dll")]
        private extern static bool HandsInkSetResolution(int width, int height);
        [DllImport("HandsInkRecognizer.dll")]
        private extern static bool HandsInkAddBoundingBox(int left, int top, int right, int bottom);
        [DllImport("HandsInkRecognizer.dll")]
        private extern static void HandsInkClearBoundingBox();
        [DllImport("HandsInkRecognizer.dll")]
        private extern static bool HandsInkAddPoints(POINT points, int count);
        [DllImport("HandsInkRecognizer.dll")]
        private extern static bool HandsInkAddStroke();
        [DllImport("HandsInkRecognizer.dll")]
        private extern static bool HandsInkAddPoint(int x, int y);
        [DllImport("HandsInkRecognizer.dll")]
        private extern static void HandsInkClear();
        [DllImport("HandsInkRecognizer.dll")]
        private extern static StringBuilder HandsInkRecognize();
        //[DllImport("HandsInkRecognizer.dll")]
        //private extern static void HandsInkRecognize1(StringBuilder result);
        [DllImport("HandsInkRecognizer.dll")]
        private extern static IntPtr HandsInkGetRecognitionResult();
        [DllImport("HandsInkRecognizer.dll")]
        private extern static bool HandsInkReadWordList(byte[] fileName);
        [DllImport("HandsInkRecognizer.dll")]
        private extern static StringBuilder HandsInkRecognizeWithWordList();
        [DllImport("HandsInkRecognizer.dll")]
        private extern static StringBuilder HandsInkGetResultWithWordList();
        [DllImport("HandsInkRecognizer.dll")]
        private extern static void HandsInkRemoveFigure();
        [DllImport("HandsInkRecognizer.dll")]
        private extern static void HandsInkCreateLattice();
        [DllImport("HandsInkRecognizer.dll")]
        private extern static bool HandsInkLoadInkFromInkML(byte[] fileName);
        [DllImport("HandsInkRecognizer.dll")]
        private extern static bool HandsInkAddInkFromInkML(byte[] fileName);
        [DllImport("HandsInkRecognizer.dll")]
        private extern static bool HandsInkLoadLatticeFromFile(byte[] fileName);
        [DllImport("HandsInkRecognizer.dll")]
        private extern static bool HandsInkAddLatticeFromFile(byte[] fileName);
        [DllImport("HandsInkRecognizer.dll")]
        private extern static bool HandsInkReadWndFile(byte[] fileName);
        [DllImport("HandsInkRecognizer.dll")]
        private extern static bool HandsInkSaveLatticeToFile(byte[] fileName);
        [DllImport("HandsInkRecognizer.dll")]
        private extern static bool HandsInkExportLatticeToFile(byte[] fileName, int index);
        [DllImport("HandsInkRecognizer.dll")]
        private extern static void HandsInkSetCustomFilterCode(byte[] code);
        [DllImport("HandsInkRecognizer.dll")]
        private extern static void HandsInkClearCustomFilter();
        [DllImport("HandsInkRecognizer.dll")]
        private extern static StringBuilder HandsInkGetCustomFilter();
        [DllImport("HandsInkRecognizer.dll")]
        private extern static void HandsInkSetHiragana(bool flag);
        [DllImport("HandsInkRecognizer.dll")]
        private extern static void HandsInkSetKatakana(bool flag);
        [DllImport("HandsInkRecognizer.dll")]
        private extern static void HandsInkSetAlphabet(bool flag);
        [DllImport("HandsInkRecognizer.dll")]
        private extern static void HandsInkSetJis1Kanji(bool flag);
        [DllImport("HandsInkRecognizer.dll")]
        private extern static void HandsInkSetJis2Kanji(bool flag);
        [DllImport("HandsInkRecognizer.dll")]
        private extern static void HandsInkSetNumeric(bool flag);
        [DllImport("HandsInkRecognizer.dll")]
        private extern static void HandsInkSetSymbol(bool flag);
        [DllImport("HandsInkRecognizer.dll")]
        private extern static void HandsInkSetGreek(bool flag);
        [DllImport("HandsInkRecognizer.dll")]
        private extern static void HandsInkSetPunctuation(bool flag);
        [DllImport("HandsInkRecognizer.dll")]
        private extern static bool HandsInkGetHiragana();
        [DllImport("HandsInkRecognizer.dll")]
        private extern static bool HandsInkGetKatakana();
        [DllImport("HandsInkRecognizer.dll")]
        private extern static bool HandsInkGetAlphabet();
        [DllImport("HandsInkRecognizer.dll")]
        private extern static bool HandsInkGetJis1Kanji();
        [DllImport("HandsInkRecognizer.dll")]
        private extern static bool HandsInkGetJis2Kanji();
        [DllImport("HandsInkRecognizer.dll")]
        private extern static bool HandsInkGetNumeric();
        [DllImport("HandsInkRecognizer.dll")]
        private extern static bool HandsInkGetSymbol();
        [DllImport("HandsInkRecognizer.dll")]
        private extern static bool HandsInkGetGreek();
        [DllImport("HandsInkRecognizer.dll")]
        private extern static bool HandsInkGetPunctuation();

        static public bool Initialize()
        {
            return HandsInkInitialize();
        }

        static public bool InitializeDirectory(string directoryPath)
        {
            Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
            byte[] bytes = sjisEnc.GetBytes(directoryPath);
            
            return HandsInkInitializeDirectory(bytes);
        }

        static public void Close()
        {
            HandsInkClose();
        }

        static public bool SetResolution(int width, int height)
        {
            return HandsInkSetResolution(width, height);
        }

        static public bool AddBoundingBox(Rectangle bounds)
        {
            return AddBoundingBox(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
        }


        static public bool AddBoundingBox(int left, int top, int right, int bottom)
        {
            return HandsInkAddBoundingBox(left, top, right, bottom);
        }

        static public void ClearBoundingBox()
        {
            HandsInkClearBoundingBox();
        }

        static public bool AddPoints( POINT points, int count)
        {
            return HandsInkAddPoints(points, count);
        }

        static public bool AddStroke()
        {
            return HandsInkAddStroke();
        }

        static public bool AddPoint(int x, int y)
        {
            return HandsInkAddPoint(x, y);
        }

        static public void Clear()
        {
            HandsInkClear();
        }

        static public string Recognize()
        {
            StringBuilder result = HandsInkRecognize();
            return result.ToString();
        }

        static public RecognitionCandidate[] GetRecognitionResult()
        {
            IntPtr resultP = HandsInkGetRecognitionResult();
            tagRecognitionResult result = 
                (tagRecognitionResult)Marshal.PtrToStructure(resultP, typeof(tagRecognitionResult));

            RecognitionCandidate[] candArray = new RecognitionCandidate[result.Count];
            IntPtr candP = result.Candidate;

            for (int i = 0; i < result.Count; i++)
            {
                tagRecognitionCandidate rc = (tagRecognitionCandidate)Marshal.PtrToStructure(
                                                                candP, typeof(tagRecognitionCandidate));

                candArray[i] = new RecognitionCandidate(rc);
                candP = (IntPtr)((int)candP + Marshal.SizeOf(rc));
            }

            return candArray;
        }

        static public bool ReadWordList(string fileName)
        {
            Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
			byte[] bytes = sjisEnc.GetBytes(fileName);
        
            return HandsInkReadWordList(bytes);
        }

        static public string RecognizeWithWordList()
        {
            StringBuilder result = HandsInkRecognizeWithWordList();
            return result.ToString();
        }

        static public StringBuilder GetResultWithWordList()
        {
            return HandsInkGetResultWithWordList();
        }

        static public void RemoveFigure()
        {
            HandsInkRemoveFigure();
        }

        static public void CreateLattice()
        {
            HandsInkCreateLattice();
        }

        static public bool LoadInkFromInkML(string fileName)
        {
            Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
            byte[] bytes = sjisEnc.GetBytes(fileName);
            
            return HandsInkLoadInkFromInkML(bytes);
        }

        static public bool AddInkFromInkML(string fileName)
        {
            Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
            byte[] bytes = sjisEnc.GetBytes(fileName);

            return HandsInkAddInkFromInkML(bytes);
        }

        static public bool LoadLatticeFromFile(string fileName)
        {
            Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
            byte[] bytes = sjisEnc.GetBytes(fileName);

            return HandsInkLoadLatticeFromFile(bytes);
        }

        static public bool AddLatticeFromFile(string fileName)
        {
            Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
            byte[] bytes = sjisEnc.GetBytes(fileName);

            return HandsInkAddLatticeFromFile(bytes);
        }

        static public bool ReadWndFile(string fileName)
        {
            Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
            byte[] bytes = sjisEnc.GetBytes(fileName);

            return HandsInkReadWndFile(bytes);
        }

        static public bool SaveLatticeToFile(string fileName)
        {
            Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
            byte[] bytes = sjisEnc.GetBytes(fileName);

            return HandsInkSaveLatticeToFile(bytes);
        }

        static public bool ExportLatticeToFile(string fileName, int index)
        {
            Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
            byte[] bytes = sjisEnc.GetBytes(fileName);

            return HandsInkExportLatticeToFile(bytes, index);
        }

        static public string GetLattice(int index)
        {
            // 頭悪っ！！
            Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
            byte[] bytes = sjisEnc.GetBytes("lattice.txt");
            HandsInkExportLatticeToFile(bytes, index);
            StreamReader sr = new StreamReader("lattice.txt");
            return sr.ReadToEnd();
        }


        static public void SetCustomFilterCode(string code)
        {
            Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");
            byte[] bytes = sjisEnc.GetBytes(code);

            HandsInkSetCustomFilterCode(bytes);
        }

        static public void ClearCustomFilter()
        {
            HandsInkClearCustomFilter();
        }

        static public string GetCustomFilter()
        {
            StringBuilder filter = HandsInkGetCustomFilter();
            return filter.ToString();
        }
        
        static public bool Hiragana
        {
            get
            {
                return HandsInkGetHiragana();
            }
            set
            {
                HandsInkSetHiragana(value);
            }
        }

        static public void SetHiragana(bool flag)
        {
            HandsInkSetHiragana(flag);
        }

        static public void SetKatakana(bool flag)
        {
            HandsInkSetKatakana(flag);
        }

        static public void SetAlphabet(bool flag)
        {
            HandsInkSetAlphabet(flag);
        }

        static public void SetJis1Kanji(bool flag)
        {
            HandsInkSetJis1Kanji(flag);
        }

        static public void SetJis2Kanji(bool flag)
        {
            HandsInkSetJis2Kanji(flag);
        }

        static public void SetNumeric(bool flag)
        {
            HandsInkSetNumeric(flag);
        }

        static public void SetSymbol(bool flag)
        {
            HandsInkSetSymbol(flag);
        }

        static public void SetGreek(bool flag)
        {
            HandsInkSetGreek(flag);
        }

        static public void SetPunctuation(bool flag)
        {
            HandsInkSetPunctuation(flag);
        }

        static public bool GetHiragana()
        {
            return HandsInkGetHiragana();
        }

        static public bool GetKatakana()
        {
            return HandsInkGetKatakana();
        }

        static public bool GetAlphabet()
        {
            return HandsInkGetAlphabet();
        }

        static public bool GetJis1Kanji()
        {
            return HandsInkGetJis1Kanji();
        }

        static public bool GetJis2Kanji()
        {
            return HandsInkGetJis2Kanji();
        }

        static public bool GetNumeric()
        {
            return HandsInkGetNumeric();
        }

        static public bool GetSymbol()
        {
            return HandsInkGetSymbol();
        }

        static public bool GetGreek()
        {
            return HandsInkGetGreek();
        }

        static public bool GetPunctuation()
        {
            return HandsInkGetPunctuation();
        }
    }
}
