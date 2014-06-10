using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoChat
{
    /// <summary>
    /// 不適切なデータのときにスローされる例外
    /// </summary>
    public class UnsupportedDataException : ApplicationException
    {
        /// <summary>
        /// 例外インスタンスを初期化する。
        /// </summary>
        /// <param name="message">エラーメッセージ</param>
        public UnsupportedDataException(string message) : base(message) { }
    }
}
