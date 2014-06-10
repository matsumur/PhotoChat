using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoChat
{
    /// <summary>
    /// �f�[�^������̓ǂݎ��̂��߂�Dictionary�g���N���X�B
    /// �R���X�g���N�^�̈����Ƀf�[�^�������^���邱�ƂŁA
    /// Dictionary�̃C���^�t�F�[�X�œǂݎ�肪�ł���B
    /// </summary>
    public class DataStringDictionary : Dictionary<string, string>
    {
        #region �R���X�g���N�^

        /// <summary>
        /// �f�[�^�����񂩂�DataStringDictionary���쐬����B
        /// </summary>
        /// <param name="dataString">�f�[�^������</param>
        public DataStringDictionary(string dataString)
        {
            int index;
            string key, value;

            // �f�[�^�������1���ǂݎ��Dictionary�ɗv�f��ǉ�����
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




        #region �l�擾

        /// <summary>
        /// �L�[�ɑΉ�����l��Ԃ��B
        /// �L�[���Ȃ��Ƃ���defaultValue��Ԃ��B
        /// </summary>
        /// <param name="key">�L�[</param>
        /// <param name="defaultValue">�L�[���Ȃ��Ƃ��ɕԂ��l</param>
        /// <returns>�L�[�ɑΉ�����l�B�Ȃ����defaultValue��Ԃ��B</returns>
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
