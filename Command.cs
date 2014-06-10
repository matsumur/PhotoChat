using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoChat
{
    /// <summary>
    /// 内部コマンドをカプセル化
    /// </summary>
    public class Command : ISendable
    {
        #region フィールド・プロパティ

        public const int TypeRequest = 31;
        public const int TypeInform = 32;
        public const int TypeTransfer = 33;
        public const int TypeSelect = 34;
        public const int TypeConnect = 35;
        public const int TypeDisconnect = 36;
        public const int TypeLogin = 37;
        public const int TypeSession = 38;


        /// <summary>
        /// コマンドのタイプ
        /// </summary>
        public int Type
        {
            get { return type; }
        }
        private int type;

        /// <summary>
        /// 通し番号
        /// </summary>
        public long SerialNumber
        {
            get { return serialNumber; }
        }
        private long serialNumber;

        /// <summary>
        /// タイムスタンプ
        /// </summary>
        public DateTime TimeStamp
        {
            get { return timeStamp; }
        }
        private DateTime timeStamp;

        /// <summary>
        /// データ作成者の端末ID
        /// </summary>
        public string AuthorID
        {
            get { return authorID; }
        }
        private string authorID;

        /// <summary>
        /// コマンド発信元の端末ID
        /// </summary>
        public string SourceID
        {
            get { return sourceID; }
        }
        private string sourceID;

        /// <summary>
        /// ユーザ名
        /// </summary>
        public string UserName
        {
            get { return userName; }
        }
        private string userName;

        /// <summary>
        /// 写真名
        /// </summary>
        public string PhotoName
        {
            get { return photoName; }
        }
        private string photoName;

        #endregion




        #region コンストラクタ

        /// <summary>
        /// コマンドを作成する。
        /// インスタンス作成メソッドから呼び出される。
        /// </summary>
        /// <param name="type">コマンドのタイプ</param>
        /// <param name="serialNumber">通し番号</param>
        /// <param name="timeStamp">タイムスタンプ</param>
        /// <param name="authorID">データ作成者の端末ID</param>
        /// <param name="sourceID">コマンド発信元の端末ID</param>
        /// <param name="userName">ユーザ名</param>
        /// <param name="photoName">写真名</param>
        private Command(int type, long serialNumber, DateTime timeStamp,
            string authorID, string sourceID, string userName, string photoName)
        {
            this.type = type;
            this.serialNumber = serialNumber;
            this.timeStamp = timeStamp;
            this.authorID = authorID;
            this.sourceID = sourceID;
            this.userName = userName;
            this.photoName = photoName;
        }


        /// <summary>
        /// データのバイト列からインスタンスを作成する。
        /// </summary>
        /// <param name="type">コマンドのタイプ</param>
        /// <param name="dataBytes">データのバイト列</param>
        public Command(int type, byte[] dataBytes)
        {
            this.type = type;
            InterpretDataBytes(dataBytes);
        }

        #endregion




        #region データ文字列処理

        /// <summary>
        /// 各値の情報をもつデータのバイト列を返す。
        /// </summary>
        /// <returns>データのバイト列</returns>
        public byte[] GetDataBytes()
        {
            try
            {
                StringBuilder sb = new StringBuilder(200);
                sb.Append("SerialNumber=").Append(serialNumber).Append(PhotoChat.Delimiter);
                sb.Append("TimeStamp=").Append(timeStamp.Ticks).Append(PhotoChat.Delimiter);
                sb.Append("AuthorID=").Append(authorID).Append(PhotoChat.Delimiter);
                sb.Append("SourceID=").Append(sourceID).Append(PhotoChat.Delimiter);
                sb.Append("UserName=").Append(userName).Append(PhotoChat.Delimiter);
                sb.Append("PhotoName=").Append(photoName);
                return PhotoChat.DefaultEncoding.GetBytes(sb.ToString());
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return new byte[0];
            }
        }


        /// <summary>
        /// データのバイト列を読み取り各値を設定する。
        /// </summary>
        /// <param name="dataBytes">データのバイト列</param>
        private void InterpretDataBytes(byte[] dataBytes)
        {
            try
            {
                string dataString = PhotoChat.DefaultEncoding.GetString(dataBytes);
                DataStringDictionary dsd = new DataStringDictionary(dataString);
                this.serialNumber = long.Parse(dsd["SerialNumber"]);
                this.timeStamp = new DateTime(long.Parse(dsd["TimeStamp"]));
                this.authorID = dsd["AuthorID"];
                this.sourceID = dsd["SourceID"];
                this.userName = dsd["UserName"];
                this.photoName = dsd["PhotoName"];
            }
            catch (Exception e)
            {
                throw new UnsupportedDataException(e.Message);
            }
        }

        #endregion




        #region コマンド作成

        /// <summary>
        /// データ送信要求コマンドを作成する。
        /// </summary>
        /// <param name="serialNumber">要求するデータの通し番号</param>
        /// <param name="authorID">要求するデータの作成者ID</param>
        /// <param name="sourceID">コマンド発信元のID</param>
        /// <returns>データ送信要求コマンド</returns>
        public static Command CreateRequestCommand(
            long serialNumber, string authorID, string sourceID)
        {
            try
            {
                return new Command(
                    TypeRequest, serialNumber, DateTime.Now, authorID, sourceID, "null", "null");
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// 新着データ通知コマンドを作成する。
        /// </summary>
        /// <param name="image">新着画像データ</param>
        /// <param name="sourceID">コマンド発信元のID</param>
        /// <returns>新着データ通知コマンド</returns>
        public static Command CreateInformCommand(PhotoChatImage image, string sourceID)
        {
            try
            {
                return new Command(TypeInform, image.SerialNumber,
                    DateTime.Now, image.ID, sourceID, image.Author, image.PhotoName);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// 新着データ通知コマンドをコマンドを作成する。
        /// </summary>
        /// <param name="note">新着書き込みデータ</param>
        /// <param name="sourceID">コマンド発信元のID</param>
        /// <returns>新着データ通知コマンド</returns>
        public static Command CreateInformCommand(PhotoChatNote note, string sourceID)
        {
            try
            {
                return new Command(TypeInform, note.SerialNumber,
                    DateTime.Now, note.ID, sourceID, note.Author, note.PhotoName);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// 新着データ通知コマンドをコマンドを作成する。
        /// </summary>
        /// <param name="sharedFile">新着共有ファイル</param>
        /// <param name="sourceID">コマンド発信元のID</param>
        /// <returns>新着データ通知コマンド</returns>
        public static Command CreateInformCommand(SharedFile sharedFile, string sourceID)
        {
            try
            {
                return new Command(TypeInform, sharedFile.SerialNumber,
                    DateTime.Now, sharedFile.ID, sourceID, "null", "null");
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// IDに対応する保持データ通知コマンドを作成する。
        /// </summary>
        /// <param name="floorNumber">保持データの最小番号</param>
        /// <param name="ceilingNumber">保持データの最大番号</param>
        /// <param name="authorID">対象のID</param>
        /// <param name="sourceID">コマンド発信元のID</param>
        /// <returns>保持データ通知コマンド</returns>
        public static Command CreateTransferCommand(
            long floorNumber, long ceilingNumber, string authorID, string sourceID)
        {
            try
            {
                return new Command(TypeTransfer, ceilingNumber,
                    DateTime.Now, authorID, sourceID, "null", floorNumber.ToString());
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// 写真選択コマンドを作成する。
        /// </summary>
        /// <param name="id">選択を行ったユーザの端末ID</param>
        /// <param name="userName">選択を行ったユーザ名</param>
        /// <param name="photoName">選択した写真名</param>
        /// <returns>写真選択コマンド</returns>
        public static Command CreateSelectCommand(string id, string userName, string photoName)
        {
            try
            {
                return new Command(TypeSelect, 0, DateTime.Now, id, id, userName, photoName);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// 接続通知コマンドを作成する。
        /// </summary>
        /// <param name="id">端末ID</param>
        /// <param name="userName">ユーザ名</param>
        /// <returns>接続通知コマンド</returns>
        public static Command CreateConnectCommand(string id, string userName)
        {
            try
            {
                return new Command(TypeConnect, 0, DateTime.Now, id, id, userName, "null");
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// 切断通知コマンドを作成する。
        /// </summary>
        /// <param name="authorID">切断した端末ID</param>
        /// <param name="sourceID">コマンド発信元の端末ID</param>
        /// <returns>切断通知コマンド</returns>
        public static Command CreateDisconnectCommand(string authorID, string sourceID)
        {
            try
            {
                return new Command(TypeDisconnect, 0,
                    DateTime.Now, authorID, sourceID, "null", "Disconnect");
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// ログインコマンドを作成する。
        /// </summary>
        /// <param name="id">端末ID</param>
        /// <param name="mailAddress">メールアドレス</param>
        /// <param name="password">パスワード</param>
        /// <param name="userName">ユーザ名</param>
        /// <returns>ログインコマンド</returns>
        public static Command CreateLoginCommand(
            string id, string mailAddress, string password, string userName)
        {
            try
            {
                return new Command(TypeLogin, 0, DateTime.Now, id, password, userName, mailAddress);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }


        /// <summary>
        /// セッションアップロード通知コマンドを作成する。
        /// </summary>
        /// <param name="id">端末ID</param>
        /// <param name="sessionID">セッションID</param>
        /// <param name="sessionName">セッション名</param>
        /// <param name="isPublic">全体公開するかどうか</param>
        /// <returns>セッションアップロード通知コマンド</returns>
        public static Command CreateSessionCommand(
            string id, string sessionID, string sessionName, bool isPublic)
        {
            try
            {
                if (isPublic)
                    return new Command(TypeSession, 1, DateTime.Now, id, id, sessionID, sessionName);
                else
                    return new Command(TypeSession, 0, DateTime.Now, id, id, sessionID, sessionName);
            }
            catch (Exception e)
            {
                PhotoChat.WriteErrorLog(e.ToString());
                return null;
            }
        }

        #endregion
    }
}
