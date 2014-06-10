using System;
using System.Collections.Generic;
using System.Text;

namespace ConversationFieldDetector
{
    public interface ISendable
    {
        /// <summary>
        /// データタイプを取得する。
        /// </summary>
        int Type { get; }

        /// <summary>
        /// データのバイト列を取得する。
        /// </summary>
        /// <returns></returns>
        byte[] GetDataBytes();
    }
}
