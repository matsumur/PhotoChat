using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Tuat.Hands;

namespace PhotoChat
{
    /// <summary>
    /// 写真オブジェクト
    /// </summary>
    public class Photo : PhotoChatImage
    {
        #region フィールド・プロパティ

        private const string Properties = "PROPERTIES";
        private static Brush infoBrush = new SolidBrush(PhotoChat.InfoColor);

        private LinkedList<PhotoChatNote> noteList = new LinkedList<PhotoChatNote>();
        private LinkedList<NoteGroup> groupList = new LinkedList<NoteGroup>();
        private List<string> tagList = new List<string>();
        private string proximity = string.Empty;
        private double latitude = 200;
        private double longitude = 200;
        private int noteCount;

        /// <summary>
        /// 表示される書き込みの数を取得
        /// </summary>
        public int NoteCount
        {
            get { return noteCount; }
        }

        /// <summary>
        /// 書き込みリスト（全ての書き込み）の要素数を取得
        /// </summary>
        public int NoteListSize
        {
            get { return noteList.Count; }
        }

        /// <summary>
        /// タグ配列を取得
        /// </summary>
        public string[] TagArray
        {
            get { return tagList.ToArray(); }
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




        #region コンストラクタ

        /// <summary>
        /// 撮影した写真のインスタンスを作成する。
        /// </summary>
        /// <param name="author">撮影者のユーザ名</param>
        /// <param name="serialNumber">データに付ける通し番号</param>
        /// <param name="image">画像データ</param>
        /// <param name="proximityList">近接関係情報</param>
        public Photo(string author, long serialNumber, Bitmap image, string[] proximityList)
            : this(author, serialNumber, image, proximityList, 200, 200) { }


        /// <summary>
        /// 撮影した写真のインスタンスを作成する。
        /// </summary>
        /// <param name="author">撮影者のユーザ名</param>
        /// <param name="serialNumber">データに付ける通し番号</param>
        /// <param name="image">画像データ</param>
        /// <param name="proximityList">近接関係情報</param>
        /// <param name="latitude">撮影地点の緯度</param>
        /// <param name="longitude">撮影地点経度</param>
        public Photo(string author, long serialNumber, Bitmap image,
            string[] proximityList, double latitude, double longitude)
        {
            this.type = TypePhoto;
            this.author = author;
            this.id = PhotoChatClient.Instance.ID;
            this.serialNumber = serialNumber;
            this.date = DateTime.Now;
            this.infoText = author + date.ToString(PhotoChat.DateFormat);
            this.proximity = PhotoChat.ArrayToString(proximityList);
            this.latitude = latitude;
            this.longitude = longitude;
            SetPhotoName();

            // 画像をJPEG形式で保存
            image.Save(GetImageFilePath(photoName), ImageFormat.Jpeg);

            // 表示サイズに変換
            this.image =
                ResizeImage(image, PhotoChat.PhotoWidth, PhotoChat.PhotoHeight);

            // 書き込みファイルの作成
            CreateNotesFile();
        }


        /// <summary>
        /// 取り込んだファイルから写真インスタンスを作成する。
        /// </summary>
        /// <param name="author">撮影者のユーザ名</param>
        /// <param name="serialNumber">データに付ける通し番号</param>
        /// <param name="originalFile">画像ファイルのパス</param>
        /// <param name="proximityList">近接関係情報</param>
        public Photo(string author, long serialNumber, string originalFile, string[] proximityList)
            : this(author, serialNumber, originalFile, proximityList, 200, 200) { }


        /// <summary>
        /// 取り込んだファイルから写真インスタンスを作成する。
        /// </summary>
        /// <param name="author">撮影者のユーザ名</param>
        /// <param name="serialNumber">データに付ける通し番号</param>
        /// <param name="originalFile">画像ファイルのパス</param>
        /// <param name="proximityList">近接関係情報</param>
        /// <param name="latitude">取り込んだ場所の緯度</param>
        /// <param name="longitude">取り込んだ場所の経度</param>
        public Photo(string author, long serialNumber, string originalFile,
            string[] proximityList, double latitude, double longitude)
        {
            this.type = TypePhoto;
            this.author = author;
            this.id = PhotoChatClient.Instance.ID;
            this.serialNumber = serialNumber;
            this.date = DateTime.Now;
            this.infoText = author + date.ToString(PhotoChat.DateFormat);
            this.proximity = PhotoChat.ArrayToString(proximityList);
            this.latitude = latitude;
            this.longitude = longitude;
            SetPhotoName();

            // ファイルの画像データを読み込みJPEG形式で保存する
            Bitmap original = new Bitmap(originalFile);
            original.Save(GetImageFilePath(photoName), ImageFormat.Jpeg);

            // 表示サイズに変換
            this.image =
                ResizeImage(original, PhotoChat.PhotoWidth, PhotoChat.PhotoHeight);
            original.Dispose();

            // 書き込みファイルの作成
            CreateNotesFile();
        }


        /// <summary>
        /// 画像・書き込みファイルを読み込み写真インスタンスを作成する。
        /// </summary>
        /// <param name="photoName">写真名</param>
        public Photo(string photoName) : this(photoName, false) { }


        /// <summary>
        /// オリジナルサイズのBitmapを保持するPhotoを作成する。
        /// </summary>
        /// <param name="photoName">写真名</param>
        /// <param name="isOriginalSize">オリジナルサイズにするならtrue</param>
        public Photo(string photoName, bool isOriginalSize)
        {
            this.type = TypePhoto;

            // 画像ファイルを読み込む
            string filePath = GetImageFilePath(photoName);
            if (!File.Exists(filePath))
                throw new UnsupportedDataException("写真ファイルが見つかりません。");
            Bitmap temp = new Bitmap(filePath);
            if (isOriginalSize)
                this.image = new Bitmap(temp);
            else
                this.image =
                    ResizeImage(temp, PhotoChat.PhotoWidth, PhotoChat.PhotoHeight);
            temp.Dispose();

            // 書き込みファイルを読み込む
            ReadNotesFile(photoName);
        }


        /// <summary>
        /// データ文字列と画像データから写真インスタンスを作成する。
        /// </summary>
        /// <param name="dataString">データ文字列</param>
        /// <param name="imageBytes">画像データのバイト列</param>
        public Photo(string dataString, byte[] imageBytes)
        {
            this.type = TypePhoto;
            InterpretDataString(dataString);

            // 画像を保存する
            string filePath = GetImageFilePath(photoName);
            if (File.Exists(filePath))
                throw new UnsupportedDataException("重複受信：" + photoName);
            try
            {
                using (FileStream fs = File.Create(filePath))
                {
                    fs.Write(imageBytes, 0, imageBytes.Length);
                    fs.Flush();
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return;
            }

            // 画像ファイルを読み込む
            Bitmap original = new Bitmap(filePath);
            this.image =
                ResizeImage(original, PhotoChat.PhotoWidth, PhotoChat.PhotoHeight);
            original.Dispose();

            // 先に受信した書き込みデータがあれば読み込む
            ReadEarlyNotes();

            // 書き込みファイルの作成
            CreateNotesFile();
        }

        #endregion




        #region ファイルパス取得・書き込みファイル入出力

        /// <summary>
        /// 画像ファイルのパスを返す
        /// </summary>
        /// <param name="photoName">写真名</param>
        /// <returns>画像ファイルのパス</returns>
        public static string GetImageFilePath(string photoName)
        {
            try
            {
                return Path.Combine(PhotoChat.PhotoDirectory, photoName + ".jpg");
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return string.Empty;
            }
        }


        /// <summary>
        /// 書き込みファイルを作成する。
        /// </summary>
        private void CreateNotesFile()
        {
            try
            {
                string filePath = PhotoChatNote.GetNotesFilePath(photoName);
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    // プロパティの保存
                    sw.WriteLine(Properties);
                    sw.WriteLine(GetDataString());

                    // 書き込みデータの保存
                    foreach (PhotoChatNote note in noteList)
                    {
                        sw.WriteLine(note.GetSaveString());
                    }
                    sw.Flush();
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 書き込みファイルを読み込む。
        /// </summary>
        /// <param name="photoName">写真名</param>
        private void ReadNotesFile(string photoName)
        {
            try
            {
                string filePath = PhotoChatNote.GetNotesFilePath(photoName);
                using (StreamReader sr = new StreamReader(filePath))
                {
                    // プロパティの読み込み
                    string line = sr.ReadLine();
                    if (line == null || line != Properties)
                        throw new UnsupportedDataException("不正な書き込みファイル");
                    if ((line = sr.ReadLine()) != null)
                        InterpretDataString(line);

                    // 書き込みデータの読み込み
                    ReadNotes(sr);
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                throw new UnsupportedDataException(e.Message);
            }
        }


        /// <summary>
        /// 写真より先に受信した書き込みデータがあれば読み込む。
        /// </summary>
        private void ReadEarlyNotes()
        {
            try
            {
                // 書き込みファイルが作成されていなければ何もしない
                string filePath = PhotoChatNote.GetNotesFilePath(photoName);
                if (!File.Exists(filePath)) return;

                // 先行した書き込みデータを読み込む
                using (StreamReader sr = new StreamReader(filePath))
                {
                    ReadNotes(sr);
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 書き込みデータを読み込む
        /// </summary>
        /// <param name="sr"></param>
        private void ReadNotes(StreamReader sr)
        {
            try
            {
                string line, dataString;
                int type, index;
                while ((line = sr.ReadLine()) != null)
                {
                    index = line.IndexOf(PhotoChat.Delimiter);
                    type = int.Parse(line.Substring(0, index));
                    dataString = line.Substring(index + 1);
                    AddNote(PhotoChatNote.CreateInstance(type, dataString));
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region データ文字列処理

        /// <summary>
        /// データのバイト列（データ文字列と画像データの連結）を返す。
        /// データ文字列の長さを表す4バイト＋データ文字列＋画像データ
        /// </summary>
        /// <returns>データのバイト列</returns>
        public override byte[] GetDataBytes()
        {
            try
            {
                // データ文字列をバイト列に変換
                byte[] dataStringBytes = PhotoChat.DefaultEncoding.GetBytes(GetDataString());
                byte[] lengthBytes = BitConverter.GetBytes(dataStringBytes.Length);

                // 全体のバイト列を作成
                FileInfo file = new FileInfo(GetImageFilePath(photoName));
                int fileLength = (int)file.Length;
                int totalLength = lengthBytes.Length + dataStringBytes.Length + fileLength;
                byte[] dataBytes = new byte[totalLength];

                // データ文字列分をコピー
                int index1 = lengthBytes.Length;
                int index2 = index1 + dataStringBytes.Length;
                lengthBytes.CopyTo(dataBytes, 0);
                dataStringBytes.CopyTo(dataBytes, index1);

                // 画像データをバイト列に追加
                try
                {
                    using (FileStream fs = file.OpenRead())
                    {
                        fs.Read(dataBytes, index2, fileLength);
                    }
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                    return null;
                }

                return dataBytes;
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return new byte[0];
            }
        }


        /// <summary>
        /// 各値の情報をもつデータ文字列を返す。
        /// </summary>
        /// <returns>データ文字列</returns>
        public override string GetDataString()
        {
            try
            {
                StringBuilder sb = new StringBuilder(200);
                sb.Append("Author=").Append(author).Append(PhotoChat.Delimiter);
                sb.Append("ID=").Append(id).Append(PhotoChat.Delimiter);
                sb.Append("SerialNumber=").Append(serialNumber).Append(PhotoChat.Delimiter);
                sb.Append("Ticks=").Append(date.Ticks).Append(PhotoChat.Delimiter);
                sb.Append("Proximity=").Append(proximity).Append(PhotoChat.Delimiter);
                sb.Append("Geo=").Append(latitude).Append(',').Append(longitude);
                return sb.ToString();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return string.Empty;
            }
        }


        /// <summary>
        /// データ文字列を読み取り各値を設定する。
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
                this.proximity = dsd.GetValue("Proximity", string.Empty);
                this.infoText = author + date.ToString(PhotoChat.DateFormat);
                SetPhotoName();

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


        /// <summary>
        /// ユーザIndex文字列を返す。
        /// </summary>
        /// <returns>ユーザIndex文字列</returns>
        public string GetIndexString()
        {
            try
            {
                StringBuilder sb = new StringBuilder(50);
                sb.Append(serialNumber).Append(PhotoChat.Delimiter);
                sb.Append(type).Append(PhotoChat.Delimiter);
                sb.Append(photoName);
                return sb.ToString();
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return string.Empty;
            }
        }

        #endregion




        #region 書き込み追加

        /// <summary>
        /// 書き込みリストへの追加
        /// </summary>
        /// <param name="note">追加する書き込みオブジェクト</param>
        public void AddNote(PhotoChatNote note)
        {
            try
            {
                if (note == null) return;

                lock (noteList)
                {
                    // noteListへの追加
                    InsertInSortedList(note, noteList);
                }

                // 書き込みの種類に応じた処理
                switch (note.Type)
                {
                    case PhotoChatNote.TypeRemoval:
                        Removal removal = (Removal)note;
                        RemoveNote(removal.TargetID, removal.TargetSerialNumber);
                        break;

                    case PhotoChatNote.TypeTag:
                        AddTag((Tag)note);
                        break;

                    default:
                        noteCount++;
                        Grouping(note);
                        break;
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 書き込みグループ（表示用）から削除する。
        /// </summary>
        /// <param name="id">削除データの端末ID</param>
        /// <param name="serialNumber">削除データの通し番号</param>
        private void RemoveNote(string id, long serialNumber)
        {
            try
            {
                lock (groupList)
                {
                    foreach (NoteGroup group in groupList)
                    {
                        if (group.RemoveNote(id, serialNumber))
                        {
                            // グループの要素が空になったらグループを削除
                            if (group.First == null)
                                groupList.Remove(group);

                            noteCount--;
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// タグを追加する。
        /// </summary>
        /// <param name="tag">タグ書き込み</param>
        private void AddTag(Tag tag)
        {
            try
            {
                if (!tagList.Contains(tag.TagString))
                    tagList.Add(tag.TagString);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 書き込みをグループ化する。
        /// </summary>
        /// <param name="note">新しい書き込み</param>
        private void Grouping(PhotoChatNote note)
        {
            try
            {
                if (note.Type == PhotoChatNote.TypeStroke)
                {
                    // ストロークの場合はグループ化できるものがないか調べる
                    foreach (NoteGroup group in groupList)
                    {
                        // 同じユーザ名の書き込みグループを調べる
                        if (!group.IsFixed && group.First.Author == note.Author)
                        {
                            if (group.AddNote(note))
                                return;
                            else
                                break;
                        }
                    }
                }
                // グループ化できないものは新しい書き込みグループ
                InsertInGroupList(note, groupList);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// noteをlistが昇順を保つように挿入する。
        /// </summary>
        /// <param name="note">挿入するPhotoChatNote</param>
        /// <param name="list">PhotoChatNoteが昇順に格納されているLinkedList</param>
        private static void InsertInSortedList(PhotoChatNote note, LinkedList<PhotoChatNote> list)
        {
            try
            {
                // 後ろから挿入箇所を探す
                LinkedListNode<PhotoChatNote> node;
                for (node = list.Last; node != null; node = node.Previous)
                    if (note.CompareTo(node.Value) > 0) break;

                // 挿入
                if (node == null)
                    list.AddFirst(note);
                else
                    list.AddAfter(node, note);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 新しいNoteGroupを作成してlistが昇順を保つように挿入する。
        /// </summary>
        /// <param name="note">新しいPhotoChatNote</param>
        /// <param name="list">NoteGroupが昇順に格納されているLinkedList</param>
        private static void InsertInGroupList(PhotoChatNote note,
                                              LinkedList<NoteGroup> list)
        {
            try
            {
                // 後ろから挿入箇所を探す
                LinkedListNode<NoteGroup> node;
                for (node = list.Last; node != null; node = node.Previous)
                    if (note.CompareTo(node.Value.First) > 0) break;

                // 挿入
                if (node == null)
                    list.AddFirst(new NoteGroup(note));
                else
                    list.AddAfter(node, new NoteGroup(note));
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region 書き込み検索

        /// <summary>
        /// タグが付けられているかどうかを返す。
        /// </summary>
        /// <param name="tag">タグ文字列</param>
        /// <returns>このタグが付けられていればtrueを返す。</returns>
        public bool ContainsTag(string tag)
        {
            return tagList.Contains(tag);
        }


        /// <summary>
        /// pointにある最初に見つかった書き込みを返す。
        /// </summary>
        /// <param name="point">座標Point</param>
        /// <returns>pointにある書き込み。見つからない場合はnull。</returns>
        public PhotoChatNote GetPointedNote(Point point)
        {
            try
            {
                lock (groupList)
                {
                    // グループ単位で検索
                    foreach (NoteGroup group in groupList)
                    {
                        if (group.Contains(point))
                        {
                            // グループ内で最初に見つかった書き込みを返す
                            return group.GetPointedNote(point);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
            return null;
        }


        /// <summary>
        /// pointにある最初に見つかった自分の書き込みを返す。
        /// </summary>
        /// <param name="point">座標Point</param>
        /// <param name="userName">現在のユーザ名</param>
        /// <returns>pointにある自分の書き込み。見つからない場合はnull。</returns>
        public PhotoChatNote GetPointedMyNote(Point point, string userName)
        {
            try
            {
                lock (groupList)
                {
                    // グループ単位で検索
                    foreach (NoteGroup group in groupList)
                    {
                        if (group.First.ID == PhotoChatClient.Instance.ID
                            && group.First.Author == userName
                            && group.Contains(point))
                        {
                            // グループ内で最初に見つかった書き込みを返す
                            return group.GetPointedNote(point);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
            return null;
        }


        /// <summary>
        /// pointにある最初に見つかったリンク書き込みを返す。
        /// </summary>
        /// <param name="point">座標Point</param>
        /// <returns>pointにあるリンク書き込み。見つからない場合はnull。</returns>
        public PhotoChatNote GetPointedLink(Point point)
        {
            try
            {
                lock (groupList)
                {
                    PhotoChatNote note;
                    foreach (NoteGroup group in groupList)
                    {
                        note = group.First;
                        if (note.Type == PhotoChatNote.TypeHyperlink
                            || note.Type == PhotoChatNote.TypeSound)
                        {
                            if (note.Contains(point))
                                return note;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
            return null;
        }

        #endregion




        #region 文字認識

        /// <summary>
        /// 写真上の書き込みを文字認識して結果をタグにする。
        /// </summary>
        /// <param name="form">PhotoChatForm</param>
        public void Recognize(PhotoChatForm form)
        {

            //// ストロークの書き込みグループのみ処理
            //foreach (NoteGroup group in groupList)
            //{
            //    if (group.First.Type == PhotoChatNote.TypeStroke)
            //    {
            //        // グループ内の各ストロークを入力
            //        InkRecognizer.Clear();
            //        foreach (PhotoChatNote note in group.NoteList)
            //        {
            //            InkRecognizer.AddStroke();
            //            foreach (Point point in ((Stroke)note).Points)
            //            {
            //                InkRecognizer.AddPoint(point.X, point.Y);
            //            }
            //        }

            //        // 認識
            //        string result = InkRecognizer.Recognize().Trim();
            //        if (!PhotoChat.ContainsInvalidChars(result))
            //        {
            //            // 形態素解析くらいすべき？
            //            // とりあえずファイルに使えない文字除去＆10文字制限
            //            // もっとマシなコードにするべき
            //            int index;
            //            while ((index = result.IndexOfAny(Path.GetInvalidFileNameChars())) >= 0)
            //            {
            //                result.Remove(index, 1);
            //            }
            //            while ((index = result.IndexOf('?')) >= 0)
            //            {
            //                result.Remove(index, 1);
            //            }
            //            while ((index = result.IndexOf('*')) >= 0)
            //            {
            //                result.Remove(index, 1);
            //            }
            //            if (result.Length > 10)
            //                result = result.Substring(0, 10);
            //            form.NewData(new Tag(form.Client.UserName,
            //                form.Client.GetNewSerialNumber(), photoName, result));
            //        }
            //    }
            //}
        }

        #endregion




        #region 書き込み描画

        /// <summary>
        /// 全ての書き込み（表示用）を描画する。
        /// </summary>
        /// <param name="g">グラフィックコンテキスト</param>
        public void PaintNotes(Graphics g)
        {
            try
            {
                lock (groupList)
                {
                    // グループごとに書き込みを描画
                    foreach (NoteGroup group in groupList)
                    {
                        group.Paint(g);
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 書き込みを指定した割合の数だけ描画する。
        /// </summary>
        /// <param name="g">グラフィックコンテキスト</param>
        /// <param name="end">描画する書き込みの割合（％）</param>
        public void PaintNotes(Graphics g, int percentage)
        {
            try
            {
                lock (groupList)
                {
                    // 割合から描画数を算出しその数だけ描画
                    int count = (noteCount * percentage) / 100;
                    foreach (NoteGroup group in groupList)
                    {
                        if (count == 0) break;
                        group.Paint(g, ref count);
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 全ての書き込み（表示用）を拡大して描画する。
        /// </summary>
        /// <param name="g">グラフィックコンテキスト</param>
        /// <param name="scaleFactor">拡大倍率</param>
        public void PaintNotes(Graphics g, float scaleFactor)
        {
            try
            {
                lock (groupList)
                {
                    foreach (NoteGroup group in groupList)
                    {
                        // グループごとに書き込みを拡大して描画（書き込み情報を付ける）
                        group.Paint(g, scaleFactor);
                    }
                }
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region NoteGroupクラス

        /// <summary>
        /// 書き込みのまとまり
        /// </summary>
        public class NoteGroup
        {
            #region フィールド・プロパティ

            private LinkedList<PhotoChatNote> myNoteList;
            private Rectangle range;
            private long lastTime;
            private bool isFixed;

            /// <summary>
            /// グループ作成時の最初の要素
            /// </summary>
            public PhotoChatNote First
            {
                get
                {
                    if (myNoteList.First != null)
                        return myNoteList.First.Value;
                    else return null;
                }
            }

            /// <summary>
            /// このグループが固定（もう要素の追加ができない）されたかどうか
            /// </summary>
            public bool IsFixed
            {
                get { return isFixed; }
                set { IsFixed = value; }
            }

            /// <summary>
            /// このグループの書き込みオブジェクトのリストを返す。
            /// </summary>
            public LinkedList<PhotoChatNote> NoteList
            {
                get { return myNoteList; }
            }

            #endregion


            #region コンストラクタ

            /// <summary>
            /// 新たな書き込みグループを作成する。
            /// </summary>
            /// <param name="note">最初の書き込み</param>
            public NoteGroup(PhotoChatNote note)
            {
                this.range = note.Range;
                this.lastTime = note.Date.Ticks;
                this.myNoteList = new LinkedList<PhotoChatNote>();
                myNoteList.AddFirst(note);

                // 書き込みがストローク以外であれば固定
                if (note.Type == PhotoChatNote.TypeStroke)
                    isFixed = false;
                else
                    isFixed = true;
            }

            #endregion


            #region グループ操作

            /// <summary>
            /// 書き込みをグループに追加する。
            /// 追加すべきかの確認を行い、追加しない場合はグループを固定する。
            /// </summary>
            /// <param name="note">追加する書き込み</param>
            /// <returns>追加した場合はtrue</returns>
            public bool AddNote(PhotoChatNote note)
            {
                try
                {
                    // 前回の書き込みからの時間間隔を確認
                    long span = note.Date.Ticks - lastTime;
                    if (span > TimeSpan.TicksPerSecond * PhotoChat.GroupingSpan)
                    {
                        isFixed = true;
                        return false;
                    }

                    // グループの書き込み領域との位置間隔を確認
                    int space = PhotoChat.GroupingSpace;
                    Rectangle inflatedRange = Rectangle.Inflate(range, space, space);
                    if (!inflatedRange.IntersectsWith(note.Range))
                    {
                        isFixed = true;
                        return false;
                    }

                    // 書き込みをグループに追加
                    InsertInSortedList(note, myNoteList);
                    lastTime = note.Date.Ticks;
                    range = Rectangle.Union(range, note.Range);
                    return true;
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                    return false;
                }
            }


            /// <summary>
            /// グループから書き込みを削除する。
            /// </summary>
            /// <param name="id">削除する書き込みの端末ID</param>
            /// <param name="serialNumber">削除する書き込みの通し番号</param>
            /// <returns>削除したらtrue、書き込みがなければfalse</returns>
            public bool RemoveNote(string id, long serialNumber)
            {
                try
                {
                    // 端末IDを確認
                    if (id == myNoteList.First.Value.ID)
                    {
                        // グループ内で通し番号が同じものを探す
                        foreach (PhotoChatNote note in myNoteList)
                        {
                            if (serialNumber == note.SerialNumber)
                            {
                                // 削除
                                myNoteList.Remove(note);
                                if (myNoteList.Count != 0)
                                {
                                    // 書き込み領域の再計算
                                    range = myNoteList.First.Value.Range;
                                    foreach (PhotoChatNote temp in myNoteList)
                                    {
                                        range = Rectangle.Union(range, temp.Range);
                                    }
                                }
                                return true;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
                return false;
            }


            /// <summary>
            /// pointがこのグループの書き込み領域内にあるかどうかを返す。
            /// </summary>
            /// <param name="point">点の座標</param>
            /// <returns>pointが領域内にあればtrue</returns>
            public bool Contains(Point point)
            {
                return range.Contains(point);
            }


            /// <summary>
            /// pointにある最初に見つかった書き込みを返す。
            /// </summary>
            /// <param name="point">座標Point</param>
            /// <returns>pointにある書き込み。見つからない場合はnull。</returns>
            public PhotoChatNote GetPointedNote(Point point)
            {
                try
                {
                    foreach (PhotoChatNote note in myNoteList)
                    {
                        if (note.Contains(point)) return note;
                    }
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
                return null;
            }

            #endregion


            #region グループ描画

            /// <summary>
            /// このグループを描画する。
            /// </summary>
            /// <param name="g">グラフィックコンテキスト</param>
            public void Paint(Graphics g)
            {
                try
                {
                    // 書き込んだユーザ名の描画
                    if (range.Width > PhotoChat.TrivialSize
                        || range.Height > PhotoChat.TrivialSize)
                    {
                        g.DrawString(First.Author, PhotoChat.RegularFont, infoBrush,
                            range.X, range.Y - PhotoChat.InfoFontSize);
                    }

                    // ストロークであれば輪郭の描画
                    if (First.Type == PhotoChatNote.TypeStroke)
                    {
                        foreach (PhotoChatNote note in myNoteList)
                            note.PaintOutline(g);
                    }

                    // 書き込みの描画
                    foreach (PhotoChatNote note in myNoteList)
                    {
                        note.Paint(g);
                    }
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
            }


            /// <summary>
            /// このグループを指定数だけ描画する。
            /// </summary>
            /// <param name="g">グラフィックコンテキスト</param>
            /// <param name="count">残りの描画する書き込み数</param>
            public void Paint(Graphics g, ref int count)
            {
                try
                {
                    // 書き込んだユーザ名の描画
                    if (range.Width > PhotoChat.TrivialSize
                        || range.Height > PhotoChat.TrivialSize)
                    {
                        g.DrawString(First.Author, PhotoChat.RegularFont, infoBrush,
                            range.X, range.Y - PhotoChat.InfoFontSize);
                    }

                    // ストロークであれば輪郭の描画
                    if (First.Type == PhotoChatNote.TypeStroke)
                    {
                        int i = count;
                        foreach (PhotoChatNote note in myNoteList)
                        {
                            note.PaintOutline(g);
                            if (--i == 0) break;
                        }
                    }

                    // 書き込みの描画
                    foreach (PhotoChatNote note in myNoteList)
                    {
                        note.Paint(g);
                        if (--count == 0) break;
                    }
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
            }


            /// <summary>
            /// このグループを拡大して描画する。
            /// </summary>
            /// <param name="g">グラフィックコンテキスト</param>
            /// <param name="scaleFactor">拡大倍率</param>
            public void Paint(Graphics g, float scaleFactor)
            {
                try
                {
                    // 書き込み情報の描画
                    if (range.Width > PhotoChat.TrivialSize
                        || range.Height > PhotoChat.TrivialSize)
                    {
                        float x = range.X * scaleFactor;
                        float y = range.Y * scaleFactor - PhotoChat.InfoFontSize;
                        g.DrawString(First.InfoText, PhotoChat.RegularFont, infoBrush, x, y);
                    }

                    // ストロークであれば輪郭の描画
                    if (First.Type == PhotoChatNote.TypeStroke)
                    {
                        foreach (PhotoChatNote note in myNoteList)
                            note.PaintOutline(g, scaleFactor);
                    }

                    // 書き込みの描画
                    foreach (PhotoChatNote note in myNoteList)
                    {
                        note.Paint(g, scaleFactor);
                    }
                }
                catch (Exception e)
                {
                    PhotoChat.WriteErrorLog(e.ToString());
                }
            }

            #endregion
        }

        #endregion
    }
}
