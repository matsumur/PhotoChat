using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoChat
{
    /// <summary>
    /// �J������������Ȃ��Ƃ��ɃX���[������O
    /// </summary>
    public class CameraNotFoundException : ApplicationException
    {
        /// <summary>
        /// ��O�C���X�^���X������������B
        /// </summary>
        /// <param name="message">�G���[���b�Z�[�W</param>
        public CameraNotFoundException(string message) : base(message) { }
    }
}
