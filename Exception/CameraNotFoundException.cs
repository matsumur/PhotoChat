using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoChat
{
    /// <summary>
    /// カメラが見つからないときにスローされる例外
    /// </summary>
    public class CameraNotFoundException : ApplicationException
    {
        /// <summary>
        /// 例外インスタンスを初期化する。
        /// </summary>
        /// <param name="message">エラーメッセージ</param>
        public CameraNotFoundException(string message) : base(message) { }
    }
}
