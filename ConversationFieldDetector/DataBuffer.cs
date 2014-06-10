using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

namespace ConversationFieldDetector
{
    /// <summary>
    /// �����^���o�b�t�@���Ǘ�����N���X
    /// </summary>
    public class DataBuffer
    {
        #region �萔

        /// <summary>
        /// �^������(�~���b)
        /// </summary>
        public const int RecInterval = 30;

        /// <summary>
        /// �^���p�P�b�g�̃f�[�^��(�o�C�g�P��)
        /// </summary>
        public const int PacketSize = SoundControl.AvgBytesPerSec * RecInterval / 1000;

        /// <summary>
        /// �^���p�P�b�g�������O�o�b�t�@�̃p�P�b�g�ɕ�����Ƃ��̕�����
        /// </summary>
        public const int PacketDivisor = 2;

        /// <summary>
        /// �����O�o�b�t�@�ɓ����p�P�b�g�̃f�[�^�T�C�Y(short�P��)
        /// </summary>
        public const int BufferPacketSize = (PacketSize / sizeof(short)) / PacketDivisor;

        /// <summary>
        /// �����O�o�b�t�@�̒���(�p�P�b�g��)
        /// RingBufferLength > SilentCandidateLength �łȂ���΂Ȃ�Ȃ�
        /// </summary>
        public const int RingBufferLength = 300 * PacketDivisor;

        /// <summary>
        /// �^����~�̂��߂̖�����Ԕ���ɗp����u���b�N�̒���
        /// </summary>
        public const int SilentCandidateLength = (3000 / RecInterval) * PacketDivisor;

        /// <summary>
        /// �^�����~���閳���t���O�̐���臒l
        /// </summary>
        public const int RecStopThreshold = (int)(SilentCandidateLength * 0.9);

        /// <summary>
        /// �^���J�n�̂��߂̗L����Ԕ���ɗp����u���b�N�̒���
        /// </summary>
        public const int SpeechCandidateLength = 30;

        /// <summary>
        /// �^�����J�n����L���t���O�̐���臒l�B
        /// </summary>
        public const int RecStartThreshold = (int)(SpeechCandidateLength * 0.8);

        /// <summary>
        /// ������r���ɒ[���Ԃ̎����̃Y�����z�����邽�߂̃}�[�W���i�p�P�b�g���j
        /// </summary>
        public const int ComparisonMargin = 10;

        /// <summary>
        /// ������r�ɗp���鉹���f�[�^�������ł���Ɣ��肷��臒l
        /// </summary>
        public const int SilentDataThreshold = 10;

        #endregion


        #region �t�B�[���h�E�v���p�e�B

        private SoundControl soundControl;
        private Codec.mp3_Encoder encoder;
        private string soundFile;
        private volatile bool voiceMailFlag = false;
        private volatile bool isRecording;

        // FFT
        private int fftInterval;
        private int fftLength;  // FFT�ɗp����z��̒���
        private int fftPacketCount;  // FFT�ɗp����p�P�b�g��
        private int fftCount;

        // �����O�o�b�t�@
        private short[] soundBuffer = new short[RingBufferLength * BufferPacketSize];
        private bool[] talkFlagBuffer = new bool[RingBufferLength];
        private long[] timeBuffer = new long[RingBufferLength];
        private int currentPosition;

        // �L���E�����t���O�J�E���g
        private int silentCount;
        private int silentJudgeHead;
        private int speechCount;
        private int speechJudgeHead;

        #endregion


        #region �R���X�g���N�^�A�I������

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="soundControl">�T�E���h�f�[�^�����Ƃ肷�邽�߂�SoundControl�I�u�W�F�N�g</param>
        /// <param name="fftLength">��r�ɂ����钷��(32000/�b)</param>
        public DataBuffer(SoundControl soundControl, int fftLength)
        {
            this.soundControl = soundControl;
            encoder = new Codec.mp3_Encoder();
            isRecording = false;

            // FFT������
            fftInterval = soundControl.SendInterval / RecInterval * PacketDivisor;
            this.fftLength = 2 ^ ((int)(Math.Log(fftLength) / Math.Log(2)) + 1);
            fftPacketCount = fftLength / BufferPacketSize + 1;
            fftCount = 0;

            // �����O�o�b�t�@������
            Array.Clear(soundBuffer, 0, soundBuffer.Length);
            Array.Clear(talkFlagBuffer, 0, talkFlagBuffer.Length);
            Array.Clear(timeBuffer, 0, timeBuffer.Length);
            currentPosition = 0;

            // �L���E�����t���O�J�E���g������
            silentCount = SilentCandidateLength;
            silentJudgeHead = RingBufferLength - SilentCandidateLength;
            speechCount = 0;
            speechJudgeHead = RingBufferLength - SpeechCandidateLength;
        }


        /// <summary>
        /// �v���O�����I������
        /// </summary>
        public void Close()
        {
            try
            {
                if (isRecording)
                {
                    isRecording = false;
                    encoder.end();
                }
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region ���[�e�B���e�B���\�b�h

        /// <summary>
        /// �����O�o�b�t�@�̌��݈ʒu���ړ�����B
        /// </summary>
        /// <param name="origin">��_</param>
        /// <param name="offset">�ړ��ʁi��Βl��RingBufferLength�ȉ��j</param>
        /// <returns>�V�����ʒu</returns>
        private int SeekPosition(int origin, int offset)
        {
            int position = origin + offset;
            if (position >= RingBufferLength)
                position -= RingBufferLength;
            else if (position < 0)
                position += RingBufferLength;

            return position;
        }


        /// <summary>
        /// �[���Ԃ̎����̃Y�����z�����邽�߂ɔ�r�J�n�ʒu���v�Z����B
        /// </summary>
        /// <param name="origin">��_</param>
        /// <param name="remoteTalkFlags">��r����[���̗L���t���O�z��</param>
        /// <returns>��r�J�n�ʒu</returns>
        private int GetCompairPosition(int origin, bool[] remoteTalkFlags)
        {
            int result = origin;
            int max = 0;

            for (int i = -ComparisonMargin; i < ComparisonMargin; i++)
            {
                int position = SeekPosition(origin, i);
                bool[] localTalkFlags = GetTalkFlags(position, fftPacketCount);
                int score = CompairBoolArray(remoteTalkFlags, localTalkFlags);
                if (score > max)
                {
                    result = position;
                    max = score;
                }
            }
            return result;
        }


        /// <summary>
        /// ���M�f�[�^�̊J�n�ʒu���擾����B
        /// �L���t���O���ł�������Ԃ�I������B
        /// �������A������Ԃł���Ɣ��肳����-1��Ԃ��B
        /// </summary>
        /// <param name="position">�ŐV�o�b�t�@�ʒu</param>
        /// <returns>���M�f�[�^�̊J�n�ʒu�B�܂��́A������ԂƔ��肳����-1��Ԃ��B</returns>
        private int GetSendDataPosition(int position)
        {
            // �ł��Â���Ԃŏ�����
            int index = SeekPosition(position, -fftInterval + 1);
            int next = SeekPosition(index, fftPacketCount);
            int count = CountBool(GetTalkFlags(index, fftPacketCount));
            int max = count;
            int result = index;

            // �O��̑��M�ȍ~�ŗL���t���O�̍ł�������Ԃ�T��
            for (int i = 0; i < fftInterval - fftPacketCount; i++)
            {
                // 1�u���b�N���炵�ėL���t���O�����X�V
                if (talkFlagBuffer[index++])
                    count--;
                if (talkFlagBuffer[next++])
                    count++;

                // �L���t���O�����ő�ł����result���X�V
                if (count > max)
                {
                    max = count;
                    result = index;
                }
                if (max == fftPacketCount) break;

                // �����O�o�b�t�@�̂Ȃ��ڏ���
                if (index == RingBufferLength)
                    index = 0;
                if (next == RingBufferLength)
                    next = 0;
            }

            // �L���t���O�������l��菬������Ζ�����ԂƔ���
            if (max < SilentDataThreshold)
                return -1;
            else
                return result;
        }


        /// <summary>
        /// �Q��bool����r����v����v�f�𐔂���B
        /// </summary>
        /// <param name="a">bool��A</param>
        /// <param name="b">bool��B</param>
        /// <returns>��v�����v�f��</returns>
        public static int CompairBoolArray(bool[] a, bool[] b)
        {
            int count = 0;
            for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
            {
                if (a[i] == b[i]) count++;
            }
            return count;
        }


        /// <summary>
        /// bool�z��̒���true�v�f�̐����J�E���g����B
        /// </summary>
        /// <param name="boolArray">bool�z��</param>
        /// <returns>true�v�f�̐�</returns>
        public static int CountBool(bool[] boolArray)
        {
            int count = 0;
            for (int i = 0; i < boolArray.Length; i++)
                if (boolArray[i]) count++;
            return count;
        }


        /// <summary>
        /// ���ݎ�����yyyyMMdd_HHmmss�t�H�[�}�b�g�ŕ�����ɂ��ĕԂ��B
        /// </summary>
        /// <returns>���ݎ����̕�����</returns>
        public static string GetNowString()
        {
            return DateTime.Now.ToString("yyyyMMdd_HHmmss");
        }

        #endregion


        #region �{�C�X���[��

        /// <summary>
        /// �{�C�X���[���^�����J�n����B
        /// </summary>
        public void StartVoiceMail()
        {
            voiceMailFlag = true;
            StopRecording();
            StartRecording(currentPosition);
        }


        /// <summary>
        /// �{�C�X���[���^�����~����B
        /// </summary>
        /// <returns>�{�C�X���[���t�@�C����</returns>
        public string EndVoiceMail()
        {
            voiceMailFlag = false;
            return StopRecording();
        }

        #endregion


        #region �f�[�^�̒ǉ�

        /// <summary>
        /// �����O�o�b�t�@�Ƀf�[�^��ǉ�����B
        /// </summary>
        /// <param name="sound">�����f�[�^</param>
        /// <param name="talkFlag">�L����Ԃ��ǂ����i�����Ȃ�false�j</param>
        /// <param name="time">�����f�[�^�̎���</param>
        public void AddData(short[] sound, bool talkFlag, long time)
        {
            try
            {
                sound.CopyTo(soundBuffer, currentPosition * BufferPacketSize);

                // �������ď���
                for (int i = 0; i < PacketDivisor; i++)
                {
                    talkFlagBuffer[currentPosition] = talkFlag;
                    timeBuffer[currentPosition] = time + (RecInterval / PacketDivisor) * i;
                    ProcessData(currentPosition);
                    currentPosition++;
                    if (currentPosition == RingBufferLength)
                        currentPosition = 0;
                }
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �ǉ����ꂽ�����f�[�^��ۑ����A�ߐڃZ���T�p�f�[�^�𑗐M����B
        /// </summary>
        /// <param name="currentPosition">�ŐV�f�[�^�̃o�b�t�@�ʒu</param>
        private void ProcessData(int currentPosition)
        {
            // ��莞�Ԃ��ƂɋߐڃZ���T�p�f�[�^�𑗐M
            if (++fftCount == fftInterval)
            {
                SendFftData(currentPosition);
                fftCount = 0;
            }

            // �L���E�����t���O�J�E���g�X�V
            UpdateJudgeCount(currentPosition);

            // �����L�^
            if (isRecording)
            {
                // �^����~����
                if (silentCount > RecStopThreshold && !voiceMailFlag)
                    StopRecording();
                else
                    Record(currentPosition);
            }
            else
            {
                // �^���J�n����
                if (speechCount > RecStartThreshold)
                    StartRecording(currentPosition);
            }
        }


        /// <summary>
        /// �L���E�����t���O�J�E���g���X�V����B
        /// </summary>
        /// <param name="currentPosition">�ŐV�f�[�^�̃o�b�t�@�ʒu</param>
        private void UpdateJudgeCount(int currentPosition)
        {
            // �͈͊O�ɏo��t���O���̌v�Z
            if (!talkFlagBuffer[silentJudgeHead++])
                silentCount--;
            if (talkFlagBuffer[speechJudgeHead++])
                speechCount--;

            // ���݈ʒu�̃t���O���̌v�Z
            if (talkFlagBuffer[currentPosition])
                speechCount++;
            else
                silentCount++;

            // �����O�o�b�t�@�̋��E�ɒB�����ꍇ�̏���
            if (silentJudgeHead == RingBufferLength)
                silentJudgeHead = 0;
            if (speechJudgeHead == RingBufferLength)
                speechJudgeHead = 0;
        }

        #endregion


        #region �����L�^

        /// <summary>
        /// �^�����J�n����B
        /// </summary>
        /// <param name="position">�����O�o�b�t�@�̐擪</param>
        private void StartRecording(int position)
        {
            try
            {
                if (!isRecording)
                {
                    isRecording = true;
                    soundFile = Path.Combine(SoundControl.SoundDirectory, GetNowString());
                    encoder.setFileName(soundFile);
                    encoder.init(SoundControl.SamplesPerSec, 32, 64);
                    Record(position);
                    soundControl.WriteLogWindow("RecStart\r\n");
                }
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �^�����~����B
        /// </summary>
        /// <returns>�����t�@�C����</returns>
        public string StopRecording()
        {
            try
            {
                if (isRecording)
                {
                    isRecording = false;
                    encoder.end();
                    soundControl.WriteLogWindow("RecStop\r\n");
                }
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
            return soundFile + ".mp3";
        }


        /// <summary>
        /// �����f�[�^���t�@�C���ɏo�͂���B
        /// </summary>
        /// <param name="position">�o�͂��郊���O�o�b�t�@�̈ʒu</param>
        private void Record(int position)
        {
            try
            {
                int index = SeekPosition(position, -SpeechCandidateLength);
                short[] buffer = GetData(index, 1);
                encoder.encode(buffer, BufferPacketSize);
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
        }

        #endregion


        #region �����O�o�b�t�@����̃f�[�^�擾

        /// <summary>
        /// ���������O�o�b�t�@����̃f�[�^�����o���B
        /// </summary>
        /// <param name="index">�J�n�ʒu�iBufferPacketSize�P�ʁj</param>
        /// <param name="length">���o���f�[�^�̒����iBufferPacketSize�P�ʁj</param>
        /// <returns>�����f�[�^�z��</returns>
        private short[] GetData(int index, int length)
        {
            try
            {
                short[] buffer = new short[length * BufferPacketSize];

                // �o�b�t�@����R�s�[
                if ((index + length) > RingBufferLength)
                {
                    // �����O�o�b�t�@�̋��E���܂����ꍇ
                    int formerLength = RingBufferLength - index;
                    int latterLength = length - formerLength;
                    Array.Copy(soundBuffer, index * BufferPacketSize,
                        buffer, 0, formerLength * BufferPacketSize);
                    Array.Copy(soundBuffer, 0,
                        buffer, formerLength * BufferPacketSize, latterLength * BufferPacketSize);
                }
                else
                {
                    // ���E���܂����������K�v�Ȃ��ꍇ
                    Array.Copy(soundBuffer, index * BufferPacketSize,
                        buffer, 0, length * BufferPacketSize);
                }

                return buffer;
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
                return new short[0];
            }
        }


        /// <summary>
        /// �L���t���O�����O�o�b�t�@����f�[�^�����o���B
        /// </summary>
        /// <param name="index">�J�n�ʒu</param>
        /// <param name="length">���o������</param>
        /// <returns>�L���t���O�z��</returns>
        private bool[] GetTalkFlags(int index, int length)
        {
            try
            {
                bool[] talkFlags = new bool[length];

                // �o�b�t�@����R�s�[
                for (int i = 0; i < length; i++)
                {
                    talkFlags[i] = talkFlagBuffer[index++];
                    if (index == RingBufferLength)
                        index = 0;
                }

                return talkFlags;
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
                return new bool[0];
            }
        }

        #endregion


        #region �ߐڃZ���T

        /// <summary>
        /// �ߐڃZ���T�p�����f�[�^���r����B
        /// </summary>
        /// <param name="soundData">��M���������f�[�^</param>
        /// <returns>��r���ʁi�ގ��x�j</returns>
        public double Compair(SoundData soundData)
        {
            try
            {
                if (soundData.Data.Length > 1)
                {
                    // �f�[�^�������ł��߂��u���b�N��T��
                    int index = SeekPosition(currentPosition, 100);
                    while (index != currentPosition)
                    {
                        // �ȒP�ɑ���̃f�[�^�������z�����u�ԂƂ���
                        if (timeBuffer[index] > soundData.Time)
                            break;
                        if (++index == RingBufferLength)
                            index = 0;
                    }

                    // �Ή����鎩���̉����o�b�t�@���L����Ԃł��邩�m�F
                    bool[] localTalkingFlag = GetTalkFlags(index, fftPacketCount);
                    if (CountBool(localTalkingFlag) > SilentDataThreshold)
                    {
                        // �����f�[�^�̔�r
                        short[] data = GetData(index, fftPacketCount);
                        double[] localFrequency = UseFFT.GetPower(data);
                        return UseFFT.Compair(localFrequency, soundData.Data);
                    }
                }
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
            return 0;
        }


        /// <summary>
        /// �ߐڃZ���T�p�����f�[�^�𑗐M����B
        /// </summary>
        /// <param name="position">�ŐV�o�b�t�@�ʒu</param>
        private void SendFftData(int position)
        {
            try
            {
                int index = GetSendDataPosition(position);
                if (index == -1)
                {
                    soundControl.SendAll(
                        new SoundData(timeBuffer[position], new double[0], soundControl.ID));
                }
                else
                {
                    double[] fftData = UseFFT.GetPower(GetData(index, fftPacketCount));
                    soundControl.SendAll(
                        new SoundData(timeBuffer[index], fftData, soundControl.ID));
                }
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
        }

        #endregion
    }
}