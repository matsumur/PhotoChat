using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoChat
{
    /// <summary>
    /// �s�K�؂ȃf�[�^�̂Ƃ��ɃX���[������O
    /// </summary>
    public class UnsupportedDataException : ApplicationException
    {
        /// <summary>
        /// ��O�C���X�^���X������������B
        /// </summary>
        /// <param name="message">�G���[���b�Z�[�W</param>
        public UnsupportedDataException(string message) : base(message) { }
    }
}
