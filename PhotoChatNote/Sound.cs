using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace PhotoChat
{
    /// <summary>
    /// 音声リンクを表す。
    /// </summary>
    public class Sound : PhotoChatNote
    {
        #region フィールド・プロパティ
        
        private static readonly Bitmap soundImage = Properties.Resources.sound;
        private string soundFileName;

        /// <summary>
        /// 音声ファイル名を取得する。
        /// </summary>
        public string SoundFileName
        {
            get { return soundFileName; }
        }

        #endregion




        #region コンストラクタ

        /// <summary>
        /// 音声リンクを作成する。
        /// </summary>
        /// <param name="author">音声データを貼り付けたユーザ名</param>
        /// <param name="serialNumber">通し番号</param>
        /// <param name="photoName">貼り付けた写真名</param>
        /// <param name="point">貼り付けた位置</param>
        /// <param name="soundFileName">音声データのファイル名</param>
        public Sound(string author, long serialNumber,
            string photoName, Point point, string soundFileName)
        {
            this.type = TypeSound;
            this.author = author;
            this.id = PhotoChatClient.Instance.ID;
            this.serialNumber = serialNumber;
            this.date = DateTime.Now;
            this.photoName = photoName;
            this.range = new Rectangle(point, soundImage.Size);
            this.soundFileName = soundFileName;
            this.infoText = author + date.ToString(PhotoChat.DateFormat);
        }


        /// <summary>
        /// データ文字列からインスタンスを作成する。
        /// </summary>
        /// <param name="dataString">データ文字列</param>
        public Sound(string dataString)
        {
            this.type = TypeSound;
            InterpretDataString(dataString);
        }

        #endregion




        #region データ文字列処理

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
                sb.Append("PhotoName=").Append(photoName).Append(PhotoChat.Delimiter);
                sb.Append("Point=").Append(range.X).Append(
                    SubDelimiter).Append(range.Y).Append(PhotoChat.Delimiter);
                sb.Append("SoundFileName=").Append(soundFileName);
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
                this.photoName = dsd["PhotoName"];
                this.soundFileName = dsd["SoundFileName"];
                this.infoText = author + date.ToString(PhotoChat.DateFormat);

                // 位置の読み取り
                string str = dsd["Point"];
                int index = str.IndexOf(SubDelimiter);
                int x = int.Parse(str.Substring(0, index));
                int y = int.Parse(str.Substring(index + 1));
                this.range = new Rectangle(new Point(x, y), soundImage.Size);
            }
            catch (Exception e)
            {
                throw new UnsupportedDataException(e.Message);
            }
        }

        #endregion




        /// <summary>
        /// 音声リンクを移動する。
        /// </summary>
        /// <param name="dx">右方向に移動する距離</param>
        /// <param name="dy">下方向に移動する距離</param>
        public override void Move(int dx, int dy)
        {
            range.X += dx;
            range.Y += dy;
        }




        #region 描画

        /// <summary>
        /// 音声アイコンを描画する。
        /// </summary>
        /// <param name="g">グラフィックコンテキスト</param>
        public override void Paint(Graphics g)
        {
            try
            {
                g.DrawImage(soundImage, range.Location);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// 音声アイコンを拡大倍率に応じて描画する。
        /// </summary>
        /// <param name="g">グラフィックコンテキスト</param>
        /// <param name="scaleFactor">拡大倍率</param>
        public override void Paint(Graphics g, float scaleFactor)
        {
            try
            {
                // 拡大描画
                float x = range.X * scaleFactor;
                float y = range.Y * scaleFactor;
                float width = range.Width * scaleFactor;
                float height = range.Height * scaleFactor;
                g.DrawImage(soundImage, x, y, width, height);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
            }
        }

        #endregion
    }
}
