using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectSound;

namespace ConversationFieldDetector
{
    /// <summary>
    /// ���������𓝊�����N���X�B
    /// </summary>
    public class SoundControl
    {
        #region �萔

        //Wav�t�H�[�}�b�g
        public const int SamplesPerSec = 16000;
        public const int BitsPerSample = 16;
        public const int Channels = 2; //mono
        public const int BlockAlign = Channels * BitsPerSample / 8;
        public const int AvgBytesPerSec = SamplesPerSec * BlockAlign;

        /// <summary>
        /// �ߐڊ֌W�ɂ���Ɣ��肷�鉹���ގ��x��臒l
        /// </summary>
        public const double ProximityThreshold = 0.8;

        /// <summary>
        /// ���O�Ȃǂ�ۑ�����V�X�e���f�B���N�g���̃p�X
        /// </summary>
        public static string SystemDirectory;

        /// <summary>
        /// ���O�t�@�C���̃p�X
        /// </summary>
        private static string LogFile;

        /// <summary>
        /// �����O�o�b�t�@����Notify���s����
        /// </summary>
        private const int NotificationTimes = 3;

        #endregion

        #region �t�B�[���h�E�v���p�e�B
        private string id;

        public delegate void SendBackSignalDelegate(ConversationFieldDetector.ISendable soundData);
        private SendBackSignalDelegate sendBackDeligate;
        private AsyncCallback completeSendBack;

        private DataBuffer dataBuffer;
        private Thread recordingThread;
        private volatile bool isActive = false;

        // �L���E��������֘A
        private double volumeAverage = 5000;
        private double denominator = 0;

        // �ߐڃZ���T
        private int sendInterval;
        private int proximityKeepCycle;
        private Dictionary<string, int> userDictionary = new Dictionary<string, int>();
        private List<string> nearUserList = new List<string>();

        // capture�̈ʒu
        private int captureBufferSize;
        private int nextCaptureOffset;
        // ��x�ɘ^������f�[�^�T�C�Y
        private int notifySize;
        // Notify�C�x���g
        private Notify applicationNotify;
        // Notify�̈ʒu
        private BufferPositionNotify[] positionNotify = new BufferPositionNotify[NotificationTimes];
        // Notify���N�����C�x���g
        private AutoResetEvent notificationEvent;
        // �^���o�b�t�@
        private CaptureBuffer applicationBuffer;
        // �f�o�C�X
        private Capture applicationDevice;
        // Wav�̃t�H�[�}�b�g
        private WaveFormat inputFormat;

        private static string soundDirectory;
        private static string errorFile;

        private static Dictionary<string, long> diffTimeDictonary;

        public static string SoundDirectory
        {
            get { return soundDirectory; }
            set { soundDirectory = value; }
        }

        /// <summary>
        /// �ߐڃZ���V���O�p�f�[�^�̑��M�������擾����B
        /// </summary>
        public int SendInterval
        {
            get { return sendInterval; }
        }

        /// <summary>
        /// ��x�̋ߐڔ��肪�e�����y�ڂ����������擾����B
        /// </summary>
        public int ProximityKeepCycle
        {
            get { return proximityKeepCycle; }
        }

        /// <summary>
        /// �[��ID���擾����B
        /// </summary>
        public string ID
        {
            get { return this.id; }
        }

        /// <summary>
        /// �ߐڊ֌W�ɂ��郆�[�U�̒[��ID�̔z����擾����B
        /// </summary>
        /// <returns>�ߐڊ֌W�ɂ���[��ID�̔z��</returns>
        public string[] GetNearUserID()
        {
            return nearUserList.ToArray();
        }

        /// <summary>
        /// SoundControl���N�������ǂ������擾����B
        /// </summary>
        public bool IsActive
        {
            get { return isActive; }
        }

        #endregion


        #region �R���X�g���N�^�A�������E�I������

        /// <summary>
        /// ���������Ǘ������쐬����B
        /// </summary>
        /// <param name="id">���̒[����ID(�ǂ̒[�����߂��ɂ���̂��̕\���ɗ��p)</param>
        /// <param name="sendBack">��r�p�f�[�^�擾deligate</param>
        /// <param name="sendInterval">�ߐڃZ���V���O�p�f�[�^���M����(�~���b, 7000ms�ȉ��Ƃ���)</param>
        /// <param name="fftLength">��r��������T�E���h�f�[�^�̃o�C�g��(32000/s)</param>
        /// <param name="proximityKeepCycle">��x��True���肪�e�����y�ڂ�������</param>
        /// <param name="soundDirectory">�������O(16kbps, 16bit)�ۑ��̂��߂̃f�B���N�g��</param>
        /// <param name="errorFile">�G���[���O�������o���t�@�C��</param>
        public SoundControl(string id, SendBackSignalDelegate sendBack, int fftLength, int sendInterval, int proximityKeepCycle, string soundDirectory, string errorFile)
        {
            try
            {
                this.id = id;
                SystemDirectory = Path.Combine(soundDirectory, "system");
                LogFile = Path.Combine(SystemDirectory, DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");
                diffTimeDictonary = new Dictionary<string, long>();

                sendBackDeligate = sendBack;
                completeSendBack = new AsyncCallback(CompleteSendBackMethod);
                SoundControl.soundDirectory = soundDirectory;
                SoundControl.errorFile = errorFile;

                if (sendInterval > 7000) sendInterval = 7000;
                CreateDirectory();
                this.sendInterval = sendInterval;
                this.proximityKeepCycle = proximityKeepCycle;
                dataBuffer = new DataBuffer(this, fftLength);


                //���ʂ̐ݒ�
                int mixer;
                Mixer.init(out mixer); //Mixer������
                Mixer.SetMainVolume(mixer, 50); //Main�̃{�����[����50%�ɐݒ�
                Mixer.SetWaveOutVolume(mixer, 50); //WavOut��50%�ɐݒ�
                Mixer.SetMicRecordVolume(mixer, 100); //�}�C�N�^����100%�ɐݒ�

                // �f�o�C�X������
                CaptureDevicesCollection devices = new CaptureDevicesCollection();
                applicationDevice = new Capture(devices[0].DriverGuid);
                if (applicationDevice != null)
                {
                    inputFormat = new WaveFormat();
                    inputFormat.BitsPerSample = BitsPerSample;
                    inputFormat.Channels = Channels;
                    inputFormat.SamplesPerSecond = SamplesPerSec;
                    inputFormat.FormatTag = WaveFormatTag.Pcm;
                    inputFormat.BlockAlign = BlockAlign;
                    inputFormat.AverageBytesPerSecond = AvgBytesPerSec;

                    recordingThread = new Thread(new ThreadStart(RecordWorker));
                    recordingThread.IsBackground = true;
                    CreateCaptureBuffer();
                    isActive = true;
                }
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �T�E���h�f�[�^�ۑ��p�f�B���N�g�����쐬����B
        /// </summary>
        private void CreateDirectory()
        {
            if (!Directory.Exists(soundDirectory))
                Directory.CreateDirectory(soundDirectory);
            if (!Directory.Exists(SystemDirectory))
                Directory.CreateDirectory(SystemDirectory);
        }


        /// <summary>
        /// �^���o�b�t�@������������B
        /// </summary>
        private void CreateCaptureBuffer()
        {
            notificationEvent = new AutoResetEvent(false);
            notifySize = DataBuffer.PacketSize;
            notifySize -= notifySize % inputFormat.BlockAlign;
            captureBufferSize = notifySize * NotificationTimes;

            for (int i = 0; i < NotificationTimes; i++)
            {
                positionNotify[i].Offset = (notifySize * i) + notifySize - 1;
                positionNotify[i].EventNotifyHandle = notificationEvent.SafeWaitHandle.DangerousGetHandle();
            }

            CaptureBufferDescription captureDescription = new CaptureBufferDescription();
            captureDescription.BufferBytes = captureBufferSize;
            captureDescription.Format = inputFormat;

            applicationBuffer = new CaptureBuffer(captureDescription, applicationDevice);
            applicationNotify = new Notify(applicationBuffer);
            applicationNotify.SetNotificationPositions(positionNotify, NotificationTimes);
            nextCaptureOffset = 0;
        }


        /// <summary>
        /// ���������������B
        /// </summary>
        public void Close()
        {
            if (!isActive) return;
            isActive = false;

            try
            {
                Stop();
                dataBuffer.Close();
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
        }

        #endregion




        #region ���[�U�ǉ�

        /// <summary>
        /// ���[�U��ǉ�����B
        /// </summary>
        /// <param name="id">�ǉ�����[��ID</param>
        /// <param name="diffTime">�[���Ԃ̎��ԍ�(����̎��v���i��ł���ꍇ�͐��̒l, �x��Ă���Εs�̒l��Ticks)</param>
        public void AddUser(string id, long diffTime)
        {
            try
            {
                if (userDictionary.ContainsKey(id))
                {
                    userDictionary[id] = 0;
                }
                else
                {
                    userDictionary.Add(id, 0);
                }

                if (diffTimeDictonary.ContainsKey(id))
                {
                    diffTimeDictonary[id] = diffTime;
                }
                else
                {
                    diffTimeDictonary.Add(id, diffTime);
                }
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
        }

        /// <summary>
        /// ���[�U���폜����B
        /// </summary>
        /// <param name="id">�폜����[��ID</param>
        public void RemoveUser(string id)
        {
            try
            {
                userDictionary.Remove(id);
                nearUserList.Remove(id);
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
        }

        #endregion


        #region �펞�^��

        /// <summary>
        /// �펞�^�����J�n����B
        /// </summary>
        public void Start()
        {
            try
            {
                recordingThread.Start();
                applicationBuffer.Start(true);
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �펞�^�����I������B
        /// </summary>
        public void Stop()
        {
            try
            {
                applicationBuffer.Stop();
                if (notificationEvent != null)
                    notificationEvent.Set();
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// �펞�����^���Ɏg���e�l�����Z�b�g����B
        /// </summary>
        public void Reset()
        {
            dataBuffer.StopRecording();
            volumeAverage = 5000;
            denominator = 0;
        }


        /// <summary>
        /// �펞�^���������s���B
        /// </summary>
        private void RecordWorker()
        {
            short[] buffer = new short[DataBuffer.PacketSize / sizeof(short)];
            while (isActive)
            {
                try
                {
                    // �L���v�`���o�b�t�@�����܂�܂őҋ@
                    notificationEvent.WaitOne(Timeout.Infinite, true);

                    // �L���v�`���T�C�Y���Z�o
                    int capturePosition, readPosition;
                    applicationBuffer.GetCurrentPosition(out capturePosition, out readPosition);
                    int lockSize = readPosition - nextCaptureOffset;
                    if (lockSize < 0)
                        lockSize += captureBufferSize;
                    lockSize -= lockSize % notifySize;
                    if (lockSize == 0)
                        continue;

                    // �L���v�`���f�[�^�擾
                    byte[] captureData = (byte[])applicationBuffer.Read(
                        nextCaptureOffset, typeof(byte), LockFlag.None, lockSize);

                    // �p�P�b�g�ɐ؂蕪���ď���
                    for (int i = 0; i < captureData.Length / DataBuffer.PacketSize; i++)
                    {
                        System.Buffer.BlockCopy(captureData,
                            DataBuffer.PacketSize * i, buffer, 0, DataBuffer.PacketSize);
                        dataBuffer.AddData(buffer, DetectTalking(buffer), DateTime.Now.Ticks);
                    }

                    // ����̃I�t�Z�b�g���v�Z
                    nextCaptureOffset += captureData.Length;
                    if (nextCaptureOffset >= captureBufferSize)
                        nextCaptureOffset -= captureBufferSize;
                }
                catch (Exception e)
                {
                    SoundControl.WriteErrorLog(e.ToString());
                }
            }
        }


        /// <summary>
        /// ���ʕ��ςƔ�r���ėL����Ԃ��ǂ����𔻒肷��B
        /// </summary>
        /// <param name="data">�����f�[�^</param>
        /// <returns>�L����Ԃł����true, ������Ԃł����false��Ԃ��B</returns>
        private bool DetectTalking(short[] data)
        {
            bool result = false;
            short max = 0;

            // ���ʂ̍ő�l�𒲂ׂ�
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == short.MinValue)
                    continue;
                short abs = Math.Abs(data[i]);
                if (max < abs) max = abs;
            }

            // ���ʕ��ς��傫�ȉ�������ΗL����ԂƔ���
            if (max > volumeAverage * 1.5)
                result = true;

            // ���ʕ��ς̍X�V
            UpdateVolumeAverage(max);

            return result;
        }


        /// <summary>
        /// ���ʕ��ς��X�V����B
        /// </summary>
        /// <param name="value">�V���Ɏ擾���ꂽ����</param>
        private void UpdateVolumeAverage(double value)
        {
            denominator += 1;
            volumeAverage += (value - volumeAverage) / denominator;
        }

        #endregion


        #region �{�C�X���[��

        /// <summary>
        /// �{�C�X���[���̘^�����J�n����B
        /// </summary>
        public void StartVoiceMail()
        {
            dataBuffer.StartVoiceMail();
        }

        /// <summary>
        /// �{�C�X���[���̘^�����~����B
        /// </summary>
        /// <returns>�{�C�X���[���t�@�C���̃p�X</returns>
        public string EndVoiceMail()
        {
            return dataBuffer.EndVoiceMail();
        }

        #endregion


        #region �ߐڃZ���T�p�f�[�^����M

        /// <summary>
        /// �ߐڃZ���T�p�����f�[�^�𑗐M����B
        /// </summary>
        /// <param name="soundData">���M���鉹���f�[�^</param>
        public void SendAll(SoundData soundData)
        {
            try
            {
                sendBackDeligate.BeginInvoke((ConversationFieldDetector.ISendable)soundData, completeSendBack, null);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
                SoundControl.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ��M�����ߐڃZ���T�p�����f�[�^����������B
        /// </summary>
        /// <param name="soundData">��M���������f�[�^</param>
        public void Receive(SoundData soundData)
        {
            try
            {
                if (!diffTimeDictonary.ContainsKey(soundData.ID)) return;

                soundData.Time -= diffTimeDictonary[soundData.ID];
                // �R�T�C���ގ��x�̌v�Z
                double similarity = dataBuffer.Compair(soundData);

                if (similarity > ProximityThreshold)
                {
                    // �ގ��x��臒l�ȏ�Ȃ�ߐڃ��[�U���X�g�ɒǉ�
                    userDictionary[soundData.ID] = proximityKeepCycle;
                    if (!nearUserList.Contains(soundData.ID))
                        nearUserList.Add(soundData.ID);
                }
                else
                {
                    int temp;
                    if (userDictionary.TryGetValue(soundData.ID, out temp))
                    {
                        if (temp > 0)
                        {
                            // �������͋ߐڔ�����p��
                            userDictionary[soundData.ID] = temp - 1;
                            if (userDictionary[soundData.ID] == 0)
                                nearUserList.Remove(soundData.ID);
                        }
                    }
                }

                WriteLog(soundData.ID + " : " + similarity.ToString());
                WriteLogWindow(
                    "\r\n##Proximity##\r\n" + similarity.ToString() + " with " + soundData.ID);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
                SoundControl.WriteErrorLog(e.ToString());
            }
        }

        #endregion


        #region ���O

        /// <summary>
        /// ���O�t�@�C���ɒǋL����B
        /// </summary>
        /// <param name="log">�ǋL���郍�O</param>
        public static void WriteLog(string log)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(LogFile))
                {
                    sw.WriteLine(log);
                    sw.Flush();
                }
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
        }


        /// <summary>
        /// ���O�E�B���h�E�ɏo�͂���B
        /// </summary>
        /// <param name="log">�o�͂��郍�O</param>
        public void WriteLogWindow(string log)
        {
            try
            {
                //client.Form.LogWindow.AddLine(log);
            }
            catch (Exception e)
            {
                SoundControl.WriteErrorLog(e.ToString());
            }
        }

        #endregion

        public static void WriteErrorLog(string errorLog)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(errorFile, true, Encoding.UTF8))
                {
                    sw.WriteLine(errorLog);
                    sw.Flush();
                }
            }
            catch { }
        }

        /// <summary>
        /// IP�ǉ����I�������Ƃ��ɃR�[���o�b�N�����
        /// </summary>
        /// <param name="asyncResult"></param>
        private void CompleteSendBackMethod(IAsyncResult asyncResult)
        {
            sendBackDeligate.EndInvoke(asyncResult);
        }
    }
}
