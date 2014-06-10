using System;
using System.Collections.Generic;
using System.Text;

namespace ConversationFieldDetector
{
    public static class UseFFT
    {
        static FFT.FFT fft = new FFT.FFT(SoundControl.SamplesPerSec);

        /// <summary>
        /// FFTをかけるshort配列の長さを取得する。
        /// </summary>
        /// <returns>short配列の長さ</returns>
        public static int GetFftLength()
        {
            return fft.getLength();
        }


        /// <summary>
        /// 音声データにFFTをかけてPowerの配列を取得する。
        /// </summary>
        /// <param name="buffer">音声データ</param>
        /// <returns>Powerの配列</returns>
        public static double[] GetPower(short[] buffer)
        {
            int length = buffer.Length;
            double[] power = new double[length];
            double[] temp = new double[length];

            // double型に変換してコピー
            for (int i = 0; i < length; i++)
                temp[i] = (double)buffer[i];

            // FFTをかける
            fft.getPower(temp, power);

            // Powerの平均を計算
            double average = 0.0;
            for (int i = 0; i < length; i++)
                average += power[i];
            average = average / length;

            // 平均値でPowerを正規化
            for (int i = 0; i < length; i++)
                power[i] = power[i] / average;

            return power;
        }


        /// <summary>
        /// 音声データのPowerを比較してコサイン類似度を計算する。
        /// 比較する周波数帯は300から3500の間なので配列の長さは3500以上でなければならない。
        /// </summary>
        /// <param name="a">音声データのPower配列A</param>
        /// <param name="b">音声データのPower配列B</param>
        /// <returns>コサイン類似度</returns>
        public static double Compair(double[] a, double[] b)
        {
            // 周波数帯を絞ってコサイン類似度を計算
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
