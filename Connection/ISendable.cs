using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoChat
{
    /// <summary>
    /// ConnectionManager�ɂ�著�M�\�Ƃ��邽�߂̃C���^�t�F�[�X
    /// </summary>
    public interface ISendable
    {
        /// <summary>
        /// �f�[�^�^�C�v���擾����B
        /// </summary>
        int Type { get; }

        /// <summary>
        /// �f�[�^�̃o�C�g����擾����B
        /// </summary>
        /// <returns></returns>
        byte[] GetDataBytes();
    }
}
