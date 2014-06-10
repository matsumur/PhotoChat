using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Net;
using System.Text;
using System.Management;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;

namespace PhotoChat
{
    /// <summary>
    /// エントリポイント、各種定数、ユーティリティメソッドをもつ。
    /// </summary>
    public static class PhotoChat
    {
        private static Mutex mutex;
        private static object logLock = new object();
        private static object errorLock = new object();

        #region 定数：通信

        /// <summary>
        /// データ送受信ポート番号
        /// </summary>
        public const int TcpPort = 59635;

        /// <summary>
        /// ブロードキャスト送受信ポート番号
        /// </summary>
        public const int UdpPort = 59636;

        /// <summary>
        /// サーバアップロードポート番号
        /// </summary>
        public const int ServerUploadPort = 59637;

        /// <summary>
        /// PhotoChatサーバのIPアドレス
        /// </summary>
        public static readonly IPAddress ServerUploadAddress = IPAddress.Parse("130.54.22.67");

        /// <summary>
        /// UDPブロードキャストを行う時間間隔（ミリ秒）
        /// </summary>
        public const int BroadcastInterval = 5000;

        /// <summary>
        /// 接続待ちを解除するアクセス回数
        /// </summary>
        public const int WaitingLimit = 3;

        #endregion


        #region 定数：GUI時間間隔

        /// <summary>
        /// 撮影モード時にカメラ画像をキャプチャする間隔（ミリ秒）
        /// </summary>
        public const int CameraCaptureInterval = 50;

        /// <summary>
        /// 撮影モードを継続する時間（ミリ秒）
        /// </summary>
        public const int CapturingTime = 120000;

        /// <summary>
        /// 写真自動選択モードにおける写真切り替え間隔の下限（秒）
        /// </summary>
        public const int AutoSelectSpan = 3;

        /// <summary>
        /// 更新のあったサムネイルのデータ保存を行う周期（ミリ秒）
        /// </summary>
        public const int SaveThumbnailsInterval = 10000;

        /// <summary>
        /// 接続情報表示ラベルに接続・切断情報を表示しておく時間（ミリ秒）
        /// </summary>
        public const int ShowingConnectionInfoTime = 5000;

        /// <summary>
        /// 近接関係表示の更新周期（ミリ秒）
        /// </summary>
        public const int UpdateProximityInterval = 2000;

        #endregion


        #region 定数：上限

        /// <summary>
        /// ユーザIndexファイル1つあたりに記録するデータ数
        /// </summary>
        public const int UserIndexFileSize = 100;

        /// <summary>
        /// 閲覧履歴の保持数
        /// </summary>
        public const int HistorySize = 10;

        /// <summary>
        /// 記憶しておくユーザ名の数の上限
        /// </summary>
        public const int MaxUserNameListSize = 5;

        /// <summary>
        /// 画像一括保存時に画像フォルダをこの枚数ごとに分ける
        /// </summary>
        public const int SaveFolderSize = 50;

        /// <summary>
        /// ハイパーリンクドラッグを開始するマウス押下後の移動距離の閾値
        /// </summary>
        public const int HyperlinkDragSize = 20;

        /// <summary>
        /// 最近のセッションとして保持する数
        /// </summary>
        public const int SessionListSize = 20;

        /// <summary>
        /// 最近入力したタグとして保持する数
        /// </summary>
        public const int InputTagListSize = 30;

        #endregion


        #region 定数：区切り文字、ヘッダ

        /// <summary>
        /// 区切り文字：タブ
        /// </summary>
        public const char Delimiter = '\t';

        /// <summary>
        /// UDP応答シグナルのヘッダ
        /// </summary>
        public const string AckHeader = "ACK";

        /// <summary>
        /// UDP時刻送信メッセージのヘッダ
        /// </summary>
        public const string TimeHeader = "TIME";

        /// <summary>
        /// 時間差返信時のヘッダ
        /// </summary>
        public const string TimeDiffHeader = "DIFFTIME";

        /// <summary>
        /// ユーザ名、タグ、セッション名に使用できない文字を通知するメッセージ
        /// </summary>
        public const string InvalidCharsMessage = @"\ / , : * ? "" < > | などの文字は使用できません。";

        #endregion


        #region 定数：画像サイズ・表示位置

        /// <summary>
        /// 表示する写真の幅
        /// </summary>
        public const int PhotoWidth = 560;

        /// <summary>
        /// 表示する写真の高さ
        /// </summary>
        public const int PhotoHeight = 420;

        /// <summary>
        /// サムネイルの幅
        /// </summary>
        public const int ThumbnailWidth = 240;

        /// <summary>
        /// サムネイルの高さ
        /// </summary>
        public const int ThumbnailHeight = 180;

        /// <summary>
        /// サムネイルボタンの幅
        /// </summary>
        public const int ThumbnailBoxWidth = 120;

        /// <summary>
        /// サムネイルボタンの高さ
        /// </summary>
        public const int ThumbnailBoxHeight = 90;

        /// <summary>
        /// 写真左側の余白
        /// </summary>
        public const int LeftMargin = 25;

        /// <summary>
        /// 写真右側の余白
        /// </summary>
        public const int RightMargin = 25;

        /// <summary>
        /// 写真上部の余白
        /// </summary>
        public const int TopMargin = 25;

        /// <summary>
        /// 写真下部の余白
        /// </summary>
        public const int BottomMargin = 25;

        /// <summary>
        /// 写真を表示するときの余白を含めた幅
        /// </summary>
        public const int ReviewWidth = PhotoWidth + LeftMargin + RightMargin;

        /// <summary>
        /// 写真を表示するときの余白を含めた高さ
        /// </summary>
        public const int ReviewHeight = PhotoHeight + TopMargin + BottomMargin;

        /// <summary>
        /// タグ付箋の幅
        /// </summary>
        public const int TagWidth = 100;

        /// <summary>
        /// タグ付箋の高さ
        /// </summary>
        public const int TagHeight = 30;

        /// <summary>
        /// 音声データ表示域の高さ
        /// </summary>
        public const int SoundHeight = 40;

        #endregion


        #region 定数：書き込みグループ化

        /// <summary>
        /// 書き込みグループ化で同じグループとみなす時間間隔の上限（秒）
        /// </summary>
        public const int GroupingSpan = 60;

        /// <summary>
        /// 書き込みグループ化で同じグループとみなす位置間隔の上限（pixel）
        /// </summary>
        public const int GroupingSpace = 50;

        /// <summary>
        /// 書き込みグループのユーザ名表示を省略するサイズの上限（pixel）
        /// </summary>
        public const int TrivialSize = 30;

        #endregion


        #region 定数：フォント

        /// <summary>
        /// 情報文字列のフォント名
        /// </summary>
        public const string FontName = "ＭＳ Ｐゴシック";

        /// <summary>
        /// 情報文字列のフォントサイズ
        /// </summary>
        public const float InfoFontSize = 11;

        /// <summary>
        /// 情報文字列用の通常フォント
        /// </summary>
        public static readonly Font RegularFont = new Font(FontName, InfoFontSize);

        /// <summary>
        /// 情報文字列用の太字フォント
        /// </summary>
        public static readonly Font BoldFont = new Font(FontName, InfoFontSize, FontStyle.Bold);

        /// <summary>
        /// 情報文字列の色
        /// </summary>
        public static readonly Color InfoColor = Color.Red;

        #endregion


        #region 定数：エンコーディング

        /// <summary>
        /// 通信などに用いる文字エンコーディング
        /// </summary>
        public static readonly Encoding DefaultEncoding = Encoding.Unicode;

        /// <summary>
        /// ファイル出力に用いる文字エンコーディング
        /// </summary>
        public static readonly Encoding FileEncoding = Encoding.GetEncoding("Shift_JIS");

        #endregion


        #region 定数：デフォルト値

        /// <summary>
        /// ストローク書き込みの幅の初期値
        /// </summary>
        public const int DefaultStrokeSize = 3;

        /// <summary>
        /// テキスト書き込みのフォントサイズの初期値
        /// </summary>
        public const int DefaultTextFontSize = 20;

        #endregion


        #region 定数：タグ

        /// <summary>
        /// 写真用タグ文字列
        /// </summary>
        public const string Photograph = "写真";

        /// <summary>
        /// 白紙メモ用タグ文字列
        /// </summary>
        public const string Notepad = "白紙メモ";

        /// <summary>
        /// インポート画像ファイル用タグ文字列
        /// </summary>
        public const string ImageData = "画像";

        /// <summary>
        /// 屋内タグ
        /// </summary>
        public const string Indoor = "屋内";

        #endregion


        #region 定数：音声処理

        /// <summary>
        /// 近接関係センシング用音声データ送信周期（ミリ秒）
        /// </summary>
        public const int VoiceSendInterval = 3000;

        /// <summary>
        /// 一度の近接判定が影響を及ぼす周期数
        /// </summary>
        public const int PositionRefreshInterval = 5;

        #endregion


        #region 定数：アップロード

        /// <summary>
        /// FlickrAPIキー
        /// </summary>
        public const string FlickrApiKey = "afde806256eba2d7ffb1589ab03a5d28";

        /// <summary>
        /// Flickr秘密文字列
        /// </summary>
        public const string FlickrSharedSecret = "56cdaad61959ea53";

        #endregion


        #region 定数：その他

        /// <summary>
        /// DateTimeの日付変換フォーマット
        /// </summary>
        public const string DateFormat = "(HH:mm:ss)";

        #endregion


        #region ディレクトリ・ファイルパス

        /// <summary>
        /// データ保存ディレクトリのパス
        /// </summary>
        public static readonly string DataDirectory = Path.Combine(Environment.CurrentDirectory, "data");

        /// <summary>
        /// 写真ファイル保存ディレクトリのパス
        /// </summary>
        public static readonly string PhotoDirectory = Path.Combine(DataDirectory, "photo");

        /// <summary>
        /// サムネイルファイル保存ディレクトリのパス
        /// </summary>
        public static readonly string ThumbnailDirectory = Path.Combine(DataDirectory, "thumbnail");

        /// <summary>
        /// 書き込みデータファイル保存ディレクトリのパス
        /// </summary>
        public static readonly string NotesDirectory = Path.Combine(DataDirectory, "note");

        /// <summary>
        /// 音声データファイル保存ディレクトリのパス
        /// </summary>
        public static readonly string SoundDirectory = Path.Combine(DataDirectory, "sound");

        /// <summary>
        /// ユーザ別インデックスファイル保存ディレクトリのパス
        /// </summary>
        public static readonly string UserIndexDirectory = Path.Combine(DataDirectory, "userIndex");

        /// <summary>
        /// タグファイル保存ディレクトリのパス
        /// </summary>
        public static readonly string TagDirectory = Path.Combine(DataDirectory, "tag");

        /// <summary>
        /// セッションごとのタグファイル保存ディレクトリのパス
        /// </summary>
        public static readonly string TagListDirectory = Path.Combine(TagDirectory, "list");

        /// <summary>
        /// 最近入力したタグを保存するファイル
        /// </summary>
        public static readonly string InputTagsFile = Path.Combine(TagListDirectory, "InputTags.dat");

        /// <summary>
        /// セッションファイル保存ディレクトリのパス
        /// </summary>
        public static readonly string SessionDirectory = Path.Combine(DataDirectory, "session");

        /// <summary>
        /// セッションリストファイルのパス
        /// </summary>
        public static readonly string SessionManagerFile = Path.Combine(SessionDirectory, "sessionManager.dat");

        /// <summary>
        /// アップロード管理ファイルのパス
        /// </summary>
        public static readonly string UploadSessionFile = Path.Combine(SessionDirectory, "uploadSession.dat");

        /// <summary>
        /// アップロードアカウント情報ファイルのパス
        /// </summary>
        public static readonly string UploadAccountFile = Path.Combine(SessionDirectory, "UPS.dat");
        
        /// <summary>
        /// リンク音声ファイル保存ディレクトリのパス
        /// </summary>
        public static readonly string SoundNoteDirectory = Path.Combine(DataDirectory, "soundNote");

        /// <summary>
        /// 地理情報ファイル保存ディレクトリのパス
        /// </summary>
        public static readonly string GeoDirectory = Path.Combine(DataDirectory, "geo");

        /// <summary>
        /// 拡張機能用ファイル保存ディレクトリのパス
        /// </summary>
        public static readonly string AdvancedDirectory = Path.Combine(DataDirectory, "advanced");

        /// <summary>
        /// ストローク認識用ファイル保存ディレクトリのパス
        /// </summary>
        public static readonly string StrokeRecognizerDirectory = Path.Combine(AdvancedDirectory, "StrokeRecognizer");

        /// <summary>
        /// ブロードキャストアドレス記録ファイル
        /// </summary>
        public static readonly string IPFile = Path.Combine(AdvancedDirectory, "BroadcastAddress.dat");

        /// <summary>
        /// ストローク認識テンプレートファイルのパス
        /// </summary>
        public static readonly string StrokeTemplateFile = Path.Combine(StrokeRecognizerDirectory, "StrokeTemplate.dat");

        /// <summary>
        /// ログファイル保存ディレクトリのパス
        /// </summary>
        public static readonly string LogDirectory = Path.Combine(DataDirectory, "log");

        /// <summary>
        /// ログファイルのパス
        /// </summary>
        public static readonly string LogFile = Path.Combine(
            LogDirectory, DateTime.Now.ToString("yyyyMMdd_HHmmss") + "LOG.csv");

        /// <summary>
        /// エラーログファイルのパス
        /// </summary>
        public static readonly string ErrorFile = Path.Combine(
            LogDirectory, DateTime.Now.ToString("yyyyMMdd_HHmmss") + "ERROR.log");

        /// <summary>
        /// GPSログファイルのパス
        /// </summary>
        public static readonly string GpsFile = Path.Combine(
            LogDirectory, DateTime.Now.ToString("yyyyMMdd_HHmmss") + "GPS.log");

        /// <summary>
        /// 設定ファイルのパス
        /// </summary>
        public static readonly string ConfigFile = Path.Combine(DataDirectory, "config.dat");

        /// <summary>
        /// Flickrトークン保存ファイルのパス
        /// </summary>
        public static readonly string FlickrTokenFile = Path.Combine(DataDirectory, "FlickrToken.dat");

        /// <summary>
        /// 状態保存ファイルのファイル名
        /// </summary>
        public const string StatusFile = "status.dat";

        #endregion




        #region エントリポイント

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                // 多重起動を防ぐ
                bool createdNew;
                mutex = new Mutex(true, "PhotoChatMutex", out createdNew);
                if (!createdNew)
                {
                    MessageBox.Show("PhotoChatはすでに起動しています。");
                    return;
                }

                // データ保存ディレクトリの作成
                CreateDirectories();

                // 起動する
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
                PhotoChatClient client = PhotoChatClient.CreateInstance();
                Application.Run(client.Form);

                // 終了前にMutexを解放
                mutex.ReleaseMutex();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                Application.Exit();
            }
        }


        /// <summary>
        /// データ保存ディレクトリの作成
        /// </summary>
        private static void CreateDirectories()
        {
            if (!Directory.Exists(PhotoDirectory))
                Directory.CreateDirectory(PhotoDirectory);
            if (!Directory.Exists(ThumbnailDirectory))
                Directory.CreateDirectory(ThumbnailDirectory);
            if (!Directory.Exists(NotesDirectory))
                Directory.CreateDirectory(NotesDirectory);
            if (!Directory.Exists(SoundDirectory))
                Directory.CreateDirectory(SoundDirectory);
            if (!Directory.Exists(UserIndexDirectory))
                Directory.CreateDirectory(UserIndexDirectory);
            if (!Directory.Exists(TagDirectory))
                Directory.CreateDirectory(TagDirectory);
            if (!Directory.Exists(TagListDirectory))
                Directory.CreateDirectory(TagListDirectory);
            if (!Directory.Exists(SessionDirectory))
                Directory.CreateDirectory(SessionDirectory);
            if (!Directory.Exists(SoundNoteDirectory))
                Directory.CreateDirectory(SoundNoteDirectory);
            if (!Directory.Exists(GeoDirectory))
                Directory.CreateDirectory(GeoDirectory);
            if (!Directory.Exists(AdvancedDirectory))
                Directory.CreateDirectory(AdvancedDirectory);
            if (!Directory.Exists(StrokeRecognizerDirectory))
                Directory.CreateDirectory(StrokeRecognizerDirectory);
            if (!Directory.Exists(LogDirectory))
                Directory.CreateDirectory(LogDirectory);
        }

        #endregion




        #region ユーティリティ：その他

        /// <summary>
        /// ブロードキャストアドレスを取得する。
        /// </summary>
        /// <returns>ブロードキャストアドレス</returns>
        public static IPAddress GetBroadcastAddress()
        {
            try
            {
                // 設定ファイルを読み込む
                if (File.Exists(IPFile))
                {
                    string address;
                    using (StreamReader sr = new StreamReader(IPFile))
                    {
                        address = sr.ReadLine();
                    }
                    return IPAddress.Parse(address);
                }
                else
                {
                    // 本当のブロードキャストアドレスは何故か失敗
                    //IPAddress broadcastAddress = IPAddress.Broadcast;

                    IPAddress broadcastAddress = IPAddress.Parse("192.168.59.255");
                    using (StreamWriter sw = new StreamWriter(IPFile, false))
                    {
                        sw.WriteLine("192.168.59.255");
                    }
                    return broadcastAddress;
                }
            }
            catch (Exception e)
            {
                WriteErrorLog(e.ToString());
                return IPAddress.Parse("192.168.0.255");
            }
        }


        /// <summary>
        /// ローカルIPアドレスを取得する。
        /// </summary>
        /// <returns>ローカルIPアドレス</returns>
        public static IPAddress GetLocalIP()
        {
            string ip;
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                    "Select IPAddress from Win32_NetworkAdapterConfiguration where IPEnabled=TRUE");
                foreach (ManagementObject mo in searcher.Get())
                {
                    ip = ((string[])mo.Properties["IPAddress"].Value)[0];
                    return IPAddress.Parse(ip);
                }
            }
            catch (Exception e)
            {
                WriteErrorLog(e.ToString());
                Application.Exit();
            }
            return null;
        }


        /// <summary>
        /// ファイルからキーと値のコレクションを読み取る。
        /// キーと値は'='でつなぎ、各組はPhotoChat.DelimiterLineで区切られることが前提。
        /// ファイル読み取りエラー処理は別途必要。
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <returns>キーと値のコレクション</returns>
        public static Dictionary<string, string> GetDictionaryFromFile(string fileName)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            using (StreamReader sr = new StreamReader(fileName))
            {
                string line, key, value;
                int index;
                while ((line = sr.ReadLine()) != null)
                {
                    index = line.IndexOf('=');
                    if (index < 0) continue;
                    key = line.Substring(0, index);
                    value = line.Substring(index + 1);
                    dictionary[key] = value;
                }
            }
            return dictionary;
        }

        #endregion


        #region ユーティリティ：配列処理

        /// <summary>
        /// 文字列配列の積集合を返す。
        /// </summary>
        /// <param name="array1">文字列配列１（配列長が短い方をこちらにしたほうが高速）</param>
        /// <param name="array2">文字列配列２</param>
        /// <returns>文字列配列の積集合</returns>
        public static string[] Intersect(string[] array1, string[] array2)
        {
            List<string> list = new List<string>(array1.Length);
            Array.Sort<string>(array2);
            for (int i = 0; i < array1.Length; i++)
            {
                // 共通の要素があればリストに追加
                if (Array.BinarySearch<string>(array2, array1[i]) >= 0)
                    list.Add(array1[i]);
            }
            return list.ToArray();
        }


        /// <summary>
        /// 文字列配列の積集合を返す。
        /// </summary>
        /// <param name="arrays">文字列配列の配列</param>
        /// <returns>文字列配列の積集合</returns>
        public static string[] Intersect(string[][] arrays)
        {
            if (arrays.Length > 0)
            {
                // 配列長でソート
                Array.Sort<string[]>(arrays, new Comparison<string[]>(CompareArrayLength));

                // 積集合を求める
                string[] result = arrays[0];
                for (int i = 1; i < arrays.Length; i++)
                    result = Intersect(result, arrays[i]);
                return result;
            }
            else
                return null;
        }


        /// <summary>
        /// 文字列配列の和集合を返す。
        /// </summary>
        /// <param name="array1">文字列配列１（配列長が短い方をこちらにしたほうが高速）</param>
        /// <param name="array2">文字列配列２</param>
        /// <returns>文字列配列の和集合</returns>
        public static string[] Union(string[] array1, string[] array2)
        {
            List<string> list = new List<string>(array1.Length + array2.Length);
            list.AddRange(array2);
            Array.Sort<string>(array2);
            for (int i = 0; i < array1.Length; i++)
            {
                // 共通の要素でなければリストに追加
                if (Array.BinarySearch<string>(array2, array1[i]) < 0)
                    list.Add(array1[i]);
            }
            return list.ToArray();
        }


        /// <summary>
        /// 文字列配列の和集合を返す。
        /// </summary>
        /// <param name="arrays">文字列配列の配列</param>
        /// <returns>文字列配列の和集合</returns>
        public static string[] Union(string[][] arrays)
        {
            if (arrays.Length > 0)
            {
                // 積集合を求める
                string[] result = arrays[0];
                for (int i = 1; i < arrays.Length; i++)
                    result = Union(arrays[i], result);
                return result;
            }
            else
                return null;
        }


        /// <summary>
        /// 文字列配列の長さを比較する。（Comparisonデリゲートに対応）
        /// </summary>
        /// <param name="array1">文字列配列１</param>
        /// <param name="array2">文字列配列２</param>
        /// <returns>比較結果（array1.Length - array2.Length）</returns>
        public static int CompareArrayLength(string[] array1, string[] array2)
        {
            if (array1 == null)
            {
                if (array2 == null)
                    return 0;
                else
                    return -1;
            }
            else
            {
                if (array2 == null)
                    return 1;
                else
                    return array1.Length - array2.Length;
            }
        }

        #endregion


        #region ユーティリティ：文字列処理

        /// <summary>
        /// 文字列配列をコンマ区切りの１つの文字列に変換する。
        /// </summary>
        /// <param name="array">変換する文字列配列</param>
        /// <returns>変換した文字列</returns>
        public static string ArrayToString(string[] array)
        {
            if (array.Length == 0) return string.Empty;

            StringBuilder sb = new StringBuilder(array[0], 200);
            for (int i = 1; i < array.Length; i++)
                sb.Append(',').Append(array[i]);
            return sb.ToString();
        }


        /// <summary>
        /// 文字列をコンマで分割して文字列配列に変換する。
        /// </summary>
        /// <param name="str">変換する文字列</param>
        /// <returns>変換した文字列配列</returns>
        public static string[] StringToArray(string str)
        {
            return str.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }


        /// <summary>
        /// 文字列中にファイル名に使用できない文字が含まれているか調べる。
        /// </summary>
        /// <param name="str">調べる文字列</param>
        /// <returns>含まれていればtrue、含まれていなければfalseを返す。</returns>
        public static bool ContainsInvalidChars(string str)
        {
            if (str.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                return true;
            if (str.Contains("?") || str.Contains("*"))
                return true;

            return false;
        }

        #endregion


        #region ユーティリティ：音声再生

        /// <summary>
        /// 音声ファイルを再生する。
        /// </summary>
        /// <param name="filePath">音声ファイルのパス</param>
        public static void AudioPlay(string filePath)
        {
            try
            {
                // 再生中の音声があれば停止
                AudioStop();

                if (File.Exists(filePath))
                {
                    //pass
                }
            }
            catch (Exception e)
            {
                WriteErrorLog(e.ToString());
            }
        }

        /// <summary>
        /// 音声再生を一時停止する。
        /// </summary>
        public static void AudioPause()
        {
            try
            {
                //pass
            }
            catch (Exception e)
            {
                WriteErrorLog(e.ToString());
            }
        }

        /// <summary>
        /// 音声再生を再開する。
        /// </summary>
        public static void AudioRestart()
        {
            try
            {
                //pass
            }
            catch (Exception e)
            {
                WriteErrorLog(e.ToString());
            }
        }

        /// <summary>
        /// 音声再生を停止する。
        /// </summary>
        public static void AudioStop()
        {
            try
            {
                //pass
            }
            catch (Exception e)
            {
                WriteErrorLog(e.ToString());
            }
        }

        #endregion


        #region ログ追記

        /// <summary>
        /// ログファイルに追記する。
        /// </summary>
        /// <param name="action">アクション文字列</param>
        /// <param name="obj1">アクションの対象</param>
        /// <param name="obj2">アクションが起こった場</param>
        public static void WriteLog(string action, string obj1, string obj2)
        {
            try
            {
                lock (logLock)
                {
                    using (StreamWriter sw = new StreamWriter(LogFile, true, FileEncoding))
                    {
                        sw.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                        sw.Write(',');
                        sw.Write(action);
                        sw.Write(',');
                        sw.Write(obj1);
                        sw.Write(',');
                        sw.WriteLine(obj2);
                        sw.Flush();
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// エラーログファイルに追記する。
        /// </summary>
        /// <param name="errorLog">エラーログ</param>
        public static void WriteErrorLog(string errorLog)
        {
            try
            {
                lock (errorLock)
                {
                    using (StreamWriter sw = new StreamWriter(ErrorFile, true, FileEncoding))
                    {
                        sw.WriteLine(errorLog);
                        sw.Flush();
                    }
                }
            }
            catch { }
        }


        /// <summary>
        /// GPSログファイルに追記する。
        /// </summary>
        /// <param name="gpsLog">GPSログ</param>
        public static void WriteGpsLog(string gpsLog)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(GpsFile, true, FileEncoding))
                {
                    sw.Write(gpsLog);
                    sw.Flush();
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion
    }
}