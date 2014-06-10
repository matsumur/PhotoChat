using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoChat
{
    /// <summary>
    /// データ文字列の読み取りのためのDictionary拡張クラス。
    /// コンストラクタの引数にデータ文字列を与えることで、
    /// Dictionaryのインタフェースで読み取りができる。
    /// </summary>
    public class DataStringDictionary : Dictionary<string, string>
    {
        #region コンストラクタ

        /// <summary>
        /// データ文字列からDataStringDictionaryを作成する。
        /// </summary>
        /// <param name="dataString">データ文字列</param>
        public DataStringDictionary(string dataString)
        {
            int index;
            string key, value;

            // データ文字列を1つずつ読み取りDictionaryに要素を追加する
            foreach (string str in dataString.Split(
                new Char[] { PhotoChat.Delimiter }, StringSplitOptions.RemoveEmptyEntries))
            {
                index = str.IndexOf('=');
                if (index < 0) continue;
                key = str.Substring(0, index);
                value = str.Substring(index + 1);
                this[key] = value;
            }
        }

        #endregion




        #region 値取得

        /// <summary>
        /// キーに対応する値を返す。
        /// キーがないときはdefaultValueを返す。
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="defaultValue">キーがないときに返す値</param>
        /// <returns>キーに対応する値。なければdefaultValueを返す。</returns>
        public string GetValue(string key, string defaultValue)
        {
            string value;
            if (TryGetValue(key, out value))
                return value;
            else
                return defaultValue;
        }

        #endregion
    }
}
