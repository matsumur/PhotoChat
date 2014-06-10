using System;
using System.Collections.Generic;
using System.Text;

namespace ConversationFieldDetector
{
    public static class UseFFT
    {
        static FFT.FFT fft = new FFT.FFT(SoundControl.SamplesPerSec);

        /// <summary>
        /// FFT��������short�z��̒������擾����B
        /// </summary>
        /// <returns>short�z��̒���</returns>
        public static int GetFftLength()
        {
            return fft.getLength();
        }


        /// <summary>
        /// �����f�[�^��FFT��������Power�̔z����擾����B
        /// </summary>
        /// <param name="buffer">�����f�[�^</param>
        /// <returns>Power�̔z��</returns>
        public static double[] GetPower(short[] buffer)
        {
            int length = buffer.Length;
            double[] power = new double[length];
            double[] temp = new double[length];

            // double�^�ɕϊ����ăR�s�[
            for (int i = 0; i < length; i++)
                temp[i] = (double)buffer[i];

            // FFT��������
            fft.getPower(temp, power);

            // Power�̕��ς��v�Z
            double average = 0.0;
            for (int i = 0; i < length; i++)
                average += power[i];
            average = average / length;

            // ���ϒl��Power�𐳋K��
            for (int i = 0; i < length; i++)
                power[i] = power[i] / average;

            return power;
        }


        /// <summary>
        /// �����f�[�^��Power���r���ăR�T�C���ގ��x���v�Z����B
        /// ��r������g���т�300����3500�̊ԂȂ̂Ŕz��̒�����3500�ȏ�łȂ���΂Ȃ�Ȃ��B
        /// </summary>
        /// <param name="a">�����f�[�^��Power�z��A</param>
        /// <param name="b">�����f�[�^��Power�z��B</param>
        /// <returns>�R�T�C���ގ��x</returns>
        public static double Compair(double[] a, double[] b)
        {
            // ���g���т��i���ăR�T�C���ގ��x���v�Z
            double x = 0.0, y = 0.0, product = 0.0;
            for (int i = 300; i < 3500; i++)
            {
                product += a[i] * b[i];
                x += Math.Pow(a[i], 2.0);
                y += Math.Pow(b[i], 2.0);
            }
            x = Math.Sqrt(x);
            y = Math.Sqrt(y);
            return product / (x * y);
        }
    }
}
