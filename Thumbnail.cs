using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Threading;

namespace PhotoChat
{
    /// <summary>
    /// サムネイルオブジェクト
    /// </summary>
    public class Thumbnail : IComparable, IComparable<Thumbnail>
    {
        #region フィールド・プロパティ

        // サムネイル一覧表示用
        private static Bitmap imageBuffer;
        private static volatile bool needBufferResize = true;
        private const int Margin = 6;
        private const int TopMargin = 15;
        private const int HeightLimit = 255 - TopMargin - Margin;
        private static int imageWidth = 1;
        private static int imageHeight = 1;
        private static readonly SolidBrush brush = new SolidBrush(Color.Black);
        private const int InfoOffsetX = 1;
        private const int InfoOffsetY = 0;
        private const int ImageOffsetX = Margin;
        private const int ImageOffsetY = TopMargin;
        private static readonly Bitmap unreadImage = Properties.Resources.unread;
        private static int unreadOffsetX = 0;
        private static int unreadOffsetY = 0;
        private static readonly Bitmap updatedImage = Properties.Resources.updated;
        private static int updatedOffsetX = 0;
        private static int updatedOffsetY = 0;
        private static readonly Bitmap markerImage = Properties.Resources.marker;
        private static int markerOffsetX = 0;
        private const int MarkerOffsetY = TopMargin;
        private static readonly Bitmap attentionImage = Properties.Resources.attention;
        private static int attentionOffsetX = 0;
        private const int AttentionOffsetY = TopMargin;
        private static readonly SolidBrush nameBrush = new SolidBrush(Color.Blue);
        private const float NameOffsetX = Margin + 2;
        private static float nameOffsetY = 0;
        private static readonly Pen showingPen = new Pen(Color.Blue, 2);

        // 基本情報
        private string photoName;
        private string author;
        private string id;
        private long serialNumber;
        private DateTime date;
        private List<string> tagList = new List<string>();
        private string proximity = string.Empty;
        private double latitude = 200;
        private double longitude = 200;

        // 描画用情報
        private Bitmap image;
        private string infoText;
        private Dictionary<string, string> nameDictionary;
        private int noteCount;
        private bool unread;
        private bool updated;
        private bool marked;
        private bool attentionFlag;
        private bool showing = false;
        private int needImageUpdate = 0;
        private int needDataSave = 0;


        /// <summary>
        /// サムネイルに対応する写真名を取得する。
        /// </summary>
        public string PhotoName
        {
            get { return photoName; }
        }

        /// <summary>
        /// 撮影者のユーザ名を取得する。
        /// </summary>
        public string Author
        {
            get { return author; }
        }

        /// <summary>
        /// 撮影した端末IDを取得する。
        /// </summary>
        public string ID
        {
            get { return id; }
        }

        /// <summary>
        /// 通し番号を取得する。
        /// </summary>
        public long SerialNumber
        {
            get { return serialNumber; }
        }

        /// <summary>
        /// 写真の撮影時刻を取得する。
        /// </summary>
        public DateTime Date
        {
            get { return date; }
        }

        /// <summary>
        /// コンマ区切りのタグを取得する。
        /// </summary>
        public string Tags
        {
            get { return PhotoChat.ArrayToString(tagList.ToArray()); }
        }

        /// <summary>
        /// 近接関係情報を取得する。
        /// </summary>
        public string Proximity
        {
            get { return proximity; }
        }

        /// <summary>
        /// 緯度を取得する。
        /// </summary>
        public double Latitude
        {
            get { return latitude; }
        }

        /// <summary>
        /// 経度を取得する。s
        /// </summary>
        public double Longitude
        {
            get { return longitude; }
        }

        #endregion




        #region コンストラクタ・デストラクタ

        /// <summary>
        /// PhotoインスタンスからThumbnailを作成する。
        /// </summary>
        /// <param name="photo">Photoインスタンス</param>
        public Thumbnail(Photo photo)
        {
            // Photoからデータの取得
            this.photoName = photo.PhotoName;
            this.author = photo.Author;
            this.id = photo.ID;
            this.serialNumber = photo.SerialNumber;
            this.date = photo.Date;
            this.proximity = photo.Proximity;
            this.latitude = photo.Latitude;
            this.longitude = photo.Longitude;
            this.noteCount = photo.NoteCount;
            AddTag(photo.TagArray);

            // その他のフィールドの初期化
            this.image = GetImage(photoName, imageWidth, imageHeight);
            this.infoText = author + date.ToString(PhotoChat.DateFormat);
            this.nameDictionary = new Dictionary<string, string>();
            this.unread = true;
            this.updated = false;
            this.marked = false;
            this.attentionFlag = false;

            // サムネイルデータ保存
            Interlocked.Increment(ref needDataSave);
            SaveData();
        }


        /// <summary>
        /// Thumbnailファイルを読み込みインスタンスを作成する。
        /// </summary>
        /// <param name="photoName">写真名</param>
        public Thumbnail(string photoName)
        {
            // サムネイル画像を読み込む
            this.photoName = photoName;
            this.image = GetImage(photoName, imageWidth, imageHeight);

            // ファイルからデータを読む
            ReadDataFile(photoName);

            // その他のフィールドの初期化
            this.infoText = author + date.ToString(PhotoChat.DateFormat);
            this.nameDictionary = new Dictionary<string, string>();
        }


        /// <summary>
        /// 未保存データを保存しリソースを解放する。
        /// </summary>
        public void Dispose()
        {
            try
            {
                // 未保存のデータがあれば保存
                SaveData();

                // リソース解放
                if (image != null)
                {
                    image.Dispose();
                    image = null;
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region ファイル入出力

        /// <summary>
        /// サムネイル画像を保存する。
        /// </summary>
        /// <param name="photoName">写真名</param>
        /// <param name="newImage">サムネイル化する画像</param>
        public static void SaveImage(string photoName, Bitmap newImage)
        {
            try
            {
                Bitmap image = PhotoChatImage.ResizeImage(
                    newImage, PhotoChat.ThumbnailWidth, PhotoChat.ThumbnailHeight);
                image.Save(GetImageFilePath(photoName), ImageFormat.Jpeg);
                image.Dispose();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// サムネイルデータを保存する。
        /// </summary>
        public void SaveData()
        {
            if (Interlocked.Exchange(ref needDataSave, 0) != 0)
            {
                try
                {
                    string filePath = GetDataFilePath(photoName);
                    using (StreamWriter sw = new StreamWriter(filePath))
                    {
                        // サムネイルデータの書き込み
                        sw.WriteLine(GetDataString());
                        sw.Flush();
                    }
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
            }
        }


        /// <summary>
        /// サムネイルデータファイルを読み込む。
        /// </summary>
        /// <param name="photoName">写真名</param>
        private void ReadDataFile(string photoName)
        {
            try
            {
                string filePath = GetDataFilePath(photoName);
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string line = sr.ReadLine();
                    InterpretDataString(line);
                }
            }
            catch (Exception e)
            {
                throw new UnsupportedDataException(e.Message);
            }
        }


        /// <summary>
        /// サムネイル画像ファイルのパスを取得する。。
        /// </summary>
        /// <param name="photoName">写真名</param>
        /// <returns>サムネイル画像ファイルのパス</returns>
        public static string GetImageFilePath(string photoName)
        {
            return Path.Combine(PhotoChat.ThumbnailDirectory, photoName + ".jpg");
        }


        /// <summary>
        /// サムネイルデータファイルのパスを取得する。。
        /// </summary>
        /// <param name="photoName">写真名</param>
        /// <returns>サムネイルファイルのパス</returns>
        public static string GetDataFilePath(string photoName)
        {
            return Path.Combine(PhotoChat.ThumbnailDirectory, photoName + ".dat");
        }


        /// <summary>
        /// 指定したサイズにサムネイル画像を縮小して返す。
        /// ただし指定サイズが画像ファイル以上の場合はサイズ変換しない。
        /// サムネイル画像がまだ無い場合はnullを返す。
        /// </summary>
        /// <param name="photoName">写真名</param>
        /// <param name="width">サムネイルの幅</param>
        /// <param name="height">サムネイルの高さ</param>
        /// <returns>指定したサイズのサムネイル画像。画像がない場合はnull。</returns>
        public static Bitmap GetImage(string photoName, int width, int height)
        {
            try
            {
                // 画像ファイルが存在するか確認
                string filePath = GetImageFilePath(photoName);
                if (!File.Exists(filePath))
                    return null;

                // 画像を指定サイズに変換して返す
                Bitmap original = new Bitmap(filePath);
                Bitmap image = PhotoChatImage.ResizeImage(original, width, height);
                original.Dispose();
                return image;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// 写真に付けられたタグをコンマ区切りで取得する。
        /// インスタンスが存在する場合はTagsプロパティから取得したほうが速い。
        /// </summary>
        /// <param name="photoName">写真名</param>
        /// <returns>写真に付けられたタグのコンマ区切り文字列</returns>
        public static string GetTags(string photoName)
        {
            try
            {
                string filePath = GetDataFilePath(photoName);
                if (File.Exists(filePath))
                {
                    string line;
                    using (StreamReader sr = new StreamReader(filePath))
                    {
                        line = sr.ReadLine();
                    }
                    DataStringDictionary dsd = new DataStringDictionary(line);
                    return dsd.GetValue("TagList", string.Empty);
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
            return string.Empty;
        }

        #endregion




        #region データ文字列処理

        /// <summary>
        /// 各フィールドの情報をもつデータ文字列を返す。
        /// </summary>
        /// <returns>データ文字列</returns>
        private string GetDataString()
        {
            try
            {
                StringBuilder sb = new StringBuilder(300);
                sb.Append("Author=").Append(author).Append(PhotoChat.Delimiter);
                sb.Append("ID=").Append(id).Append(PhotoChat.Delimiter);
                sb.Append("SerialNumber=").Append(serialNumber).Append(PhotoChat.Delimiter);
                sb.Append("Ticks=").Append(date.Ticks).Append(PhotoChat.Delimiter);
                sb.Append("TagList=").Append(PhotoChat.ArrayToString(tagList.ToArray())).Append(PhotoChat.Delimiter);
                sb.Append("Proximity=").Append(proximity).Append(PhotoChat.Delimiter);
                sb.Append("Geo=").Append(latitude).Append(',').Append(longitude).Append(PhotoChat.Delimiter);
                sb.Append("NoteCount=").Append(noteCount).Append(PhotoChat.Delimiter);
                sb.Append("Unread=").Append(unread).Append(PhotoChat.Delimiter);
                sb.Append("Updated=").Append(updated).Append(PhotoChat.Delimiter);
                sb.Append("Marked=").Append(marked).Append(PhotoChat.Delimiter);
                sb.Append("AttentionFlag=").Append(attentionFlag).Append(PhotoChat.Delimiter);
                sb.Append("NeedImageUpdate=").Append(needImageUpdate);
                return sb.ToString();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return string.Empty;
            }
        }


        /// <summary>
        /// データ文字列を読み取り各フィールドを設定する。
        /// </summary>
        /// <param name="dataString">データ文字列</param>
        private void InterpretDataString(string dataString)
        {
            try
            {
                DataStringDictionary dsd = new DataStringDictionary(dataString);

                this.author = dsd["Author"];
                this.id = dsd["ID"];
                this.serialNumber = long.Parse(dsd["SerialNumber"]);
                this.date = new DateTime(long.Parse(dsd["Ticks"]));
                AddTag(PhotoChat.StringToArray(dsd.GetValue("TagList", string.Empty)));
                this.proximity = dsd.GetValue("Proximity", string.Empty);
                this.noteCount = int.Parse(dsd.GetValue("NoteCount", "0"));
                this.unread = bool.Parse(dsd.GetValue("Unread", "false"));
                this.updated = bool.Parse(dsd.GetValue("Updated", "false"));
                this.marked = bool.Parse(dsd.GetValue("Marked", "false"));
                this.attentionFlag = bool.Parse(dsd.GetValue("AttentionFlag", "false"));
                this.needImageUpdate = int.Parse(dsd.GetValue("NeedImageUpdate", "0"));

                // GPS座標の読み取り
                string str = dsd.GetValue("Geo", "200,200");
                int index = str.IndexOf(',');
                this.latitude = double.Parse(str.Substring(0, index));
                this.longitude = double.Parse(str.Substring(index + 1));
            }
            catch (Exception e)
            {
                throw new UnsupportedDataException(e.Message);
            }
        }

        #endregion




        #region サムネイル更新・サイズ変更

        /// <summary>
        /// サムネイル画像を再読み込みする。
        /// </summary>
        public void ReloadImage()
        {
            try
            {
                Image temp = image;
                image = GetImage(photoName, imageWidth, imageHeight);
                if (temp != null) temp.Dispose();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// サムネイル画像を更新する。
        /// </summary>
        /// <param name="newImage">更新</param>
        public void UpdateImage(Bitmap newImage)
        {
            try
            {
                if (Interlocked.Exchange(ref needImageUpdate, 0) != 0)
                {
                    SaveImage(photoName, newImage);
                    Image temp = image;
                    this.image = PhotoChatImage.ResizeImage(newImage, imageWidth, imageHeight);
                    if (temp != null) temp.Dispose();
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// サムネイル一覧に表示する項目のサイズを計算・設定し、項目の高さを返す。
        /// </summary>
        /// <param name="width">サムネイル一覧の幅</param>
        /// <returns>算出したサムネイル項目の高さ</returns>
        public static int Resize(int width)
        {
            // サムネイル画像のサイズを計算
            imageWidth = width - Margin - Margin;
            imageHeight = (int)(imageWidth * 0.75);
            if (imageHeight > HeightLimit) imageHeight = HeightLimit;

            // オフセット値を計算
            unreadOffsetX = imageWidth + Margin - unreadImage.Width;
            unreadOffsetY = imageHeight + TopMargin - unreadImage.Height;
            updatedOffsetX = imageWidth + Margin - updatedImage.Width;
            updatedOffsetY = imageHeight + TopMargin - updatedImage.Height;
            markerOffsetX = imageWidth + Margin - markerImage.Width;
            attentionOffsetX = imageWidth + Margin - attentionImage.Width;

            // バッファ更新フラグを立て、サムネイル項目の高さを返す
            needBufferResize = true;
            return imageHeight + TopMargin + Margin;
        }

        #endregion




        #region サムネイルデータ処理

        /// <summary>
        /// 書き込み数を1増やす。
        /// </summary>
        public void IncrementNoteCount()
        {
            try
            {
                noteCount++;
                if (!showing)
                    updated = true;
                Interlocked.Increment(ref needDataSave);
                Interlocked.Increment(ref needImageUpdate);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 書き込み数を1減らす。
        /// </summary>
        public void DecrementNoteCount()
        {
            try
            {
                noteCount--;
                Interlocked.Increment(ref needDataSave);
                Interlocked.Increment(ref needImageUpdate);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// タグを追加する。
        /// </summary>
        /// <param name="tag">タグ文字列</param>
        public void AddTag(string tag)
        {
            try
            {
                if (!tagList.Contains(tag))
                    tagList.Add(tag);
                Interlocked.Increment(ref needDataSave);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// タグを追加する。
        /// </summary>
        /// <param name="tagArray">タグ配列</param>
        public void AddTag(string[] tagArray)
        {
            try
            {
                for (int i = 0; i < tagArray.Length; i++)
                    AddTag(tagArray[i]);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// サムネイルに目印を付けるかどうか設定する。
        /// </summary>
        /// <param name="marked">目印を付けるならtrue。</param>
        public void SetMarker(bool marked)
        {
            try
            {
                this.marked = marked;
                Interlocked.Increment(ref needDataSave);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// サムネイルに注目マークを付けるかどうか設定する。
        /// </summary>
        /// <param name="attentionFlag">注目マークを付けるならtrue。</param>
        public void SetAttention(bool attentionFlag)
        {
            try
            {
                this.attentionFlag = attentionFlag;
                Interlocked.Increment(ref needDataSave);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// このサムネイルを表示状態にする。
        /// </summary>
        /// <returns>初めて表示したときはtrue、そうでないときはfalseを返す。</returns>
        public bool Visit()
        {
            try
            {
                showing = true;
                if (unread || updated)
                {
                    unread = false;
                    updated = false;
                    Interlocked.Increment(ref needDataSave);
                    return true;
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
            return false;
        }


        /// <summary>
        /// このサムネイルを非表示状態にする。
        /// </summary>
        public void Leave()
        {
            showing = false;
        }

        #endregion




        #region ユーザ名追加・削除

        /// <summary>
        /// この写真を選択中のユーザ名を追加する。
        /// </summary>
        /// <param name="id">端末ID</param>
        /// <param name="userName">ユーザ名</param>
        public void AddUserName(string id, string userName)
        {
            try
            {
                nameDictionary[id] = userName;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 選択中のユーザ名を削除する。
        /// </summary>
        /// <param name="id">端末ID</param>
        public void RemoveUserName(string id)
        {
            try
            {
                nameDictionary.Remove(id);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region サムネイル一覧の項目の描画

        /// <summary>
        /// このサムネイル項目を描画する。
        /// </summary>
        /// <param name="g">グラフィックコンテキスト</param>
        /// <param name="bounds">描画範囲</param>
        public void Paint(Graphics g, Rectangle bounds)
        {
            try
            {
                // 必要であれば描画バッファのサイズ変更
                if (needBufferResize)
                {
                    if (imageBuffer != null)
                        imageBuffer.Dispose();
                    imageBuffer = new Bitmap(bounds.Width, bounds.Height);
                }

                // バッファへの描画
                using (Graphics bg = Graphics.FromImage(imageBuffer))
                {
                    // 書き込み数を表す背景の描画
                    int power = noteCount * 2;
                    if (power > 255) power = 255;
                    bg.Clear(Color.FromArgb(255, 255 - power, 255 - power));

                    // 撮影者と時刻の描画
                    bg.DrawString(infoText, PhotoChat.RegularFont, brush, InfoOffsetX, InfoOffsetY);

                    // サムネイル画像の描画
                    bg.DrawImage(image, ImageOffsetX, ImageOffsetY, imageWidth, imageHeight);

                    // 新着・更新マークの描画
                    if (unread)
                        bg.DrawImage(unreadImage, unreadOffsetX, unreadOffsetY);
                    else
                    {
                        if (updated)
                            bg.DrawImage(updatedImage, updatedOffsetX, updatedOffsetY);
                    }

                    // タグマークの描画
                    if (marked)
                        bg.DrawImage(markerImage, markerOffsetX, MarkerOffsetY);
                    if (attentionFlag)
                        bg.DrawImage(attentionImage, attentionOffsetX, AttentionOffsetY);

                    // 写真選択中ユーザ名の描画
                    nameOffsetY = ImageOffsetY;
                    foreach (string userName in nameDictionary.Values)
                    {
                        bg.DrawString(userName, PhotoChat.RegularFont, nameBrush, NameOffsetX, nameOffsetY);
                        nameOffsetY += PhotoChat.RegularFont.Size;
                    }

                    // 表示中枠の描画
                    if (showing)
                        bg.DrawRectangle(showingPen, 1, 1, bounds.Width - 2, bounds.Height - 2);
                }

                // バッファの描画
                g.DrawImage(imageBuffer, bounds.X, bounds.Y);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region IComparable メンバ

        /// <summary>
        /// 現在のインスタンスを同じ型の別のオブジェクトと比較する。
        /// </summary>
        /// <param name="obj">このインスタンスと比較するオブジェクト</param>
        /// <returns>比較結果</returns>
        public int CompareTo(object obj)
        {
            // 型を調べる
            Thumbnail other = obj as Thumbnail;
            if (other == null)
            {
                throw new ArgumentException();
            }

            // 比較
            return CompareTo(other);
        }


        /// <summary>
        /// 現在のThumbnailを別のThumbnailと比較する。
        /// </summary>
        /// <param name="another">比較するThumbnail</param>
        /// <returns>比較結果</returns>
        public int CompareTo(Thumbnail other)
        {
            int result = date.CompareTo(other.Date);
            if (result == 0)
                result = photoName.CompareTo(other.PhotoName);
            return result;
        }

        #endregion
    }
}
