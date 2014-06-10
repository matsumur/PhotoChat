using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

namespace PhotoChat
{
    /// <summary>
    /// �X�g���[�N�F���@�\��񋟂���N���X�B
    /// Step1�F�F���Ώۂ̓_��𓙊Ԋu��64�̓_��ɕϊ�����B
    /// Step2�F��]���Ďn�_�Əd�S�����Ԓ��������낦��B
    /// Step3�F�g��k�����ăT�C�Y�����낦�A�d�S�����_�Ƃ�����W�n�ɕϊ�����B
    /// Step4�F�e���v���[�g�Ɗe�_���r���ėގ��x���v�Z����B
    /// </summary>
    public class StrokeRecognizer
    {
        #region �t�B�[���h�E�萔�E�v���p�e�B

        private List<Template> templateList = new List<Template>();
        private const char Comma = ',';
        private const char Space = ' ';


        /// <summary>
        /// ���T���v���ɂ���ē�����_�̐�
        /// </summary>
        public const int ResampleLength = 64;

        /// <summary>
        /// �T�C�Y�ϊ��ɂ���đ傫�������킹�鐳���`�̃T�C�Y
        /// </summary>
        public const float SquareSize = 50;

        /// <summary>
        /// ���_
        /// </summary>
        private static readonly PointF Origin = new PointF(0, 0);

        /// <summary>
        /// ������
        /// </summary>
        private static readonly double GoldenRatio = 0.5 * (-1 + Math.Sqrt(5));

        /// <summary>
        /// �ŗǂ̊p�x�����߂�Ƃ��̏�������p�x�i45�x�j
        /// </summary>
        private const double MaxAngle = Math.PI / 4;

        /// <summary>
        /// �ŗǂ̊p�x�����߂�Ƃ��̏��������p�x�i-45�x�j
        /// </summary>
        private const double MinAngle = -(Math.PI / 4);

        /// <summary>
        /// �ŗǂ̊p�x�����߂�Ƃ��̏I�������臒l�p�x�i2�x�j
        /// </summary>
        private const double AngleThreshold = Math.PI * 2 / 180;

        /// <summary>
        /// Step4�̔�r���ʂ̍ő�l
        /// </summary>
        private static readonly double MaxDistance =
            Math.Sqrt(SquareSize * SquareSize + SquareSize * SquareSize) / 2;


        /// <summary>
        /// �e���v���[�g�̔z����擾����B
        /// </summary>
        public Template[] Templates
        {
            get { return templateList.ToArray(); }
        }

        #endregion


        #region �R���X�g���N�^�E�e���v���[�g�t�@�C������

        /// <summary>
        /// �e���v���[�g��ǂݍ��݃X�g���[�N�F���̏��������s���B
        /// </summary>
        public StrokeRecognizer()
        {
            LoadTemplateList();
        }


        /// <summary>
        /// �e���v���[�g���X�g��ǂݍ��ށB
        /// </summary>
        private void LoadTemplateList()
        {
            if (File.Exists(PhotoChat.StrokeTemplateFile))
            {
                using (StreamReader sr = new StreamReader(PhotoChat.StrokeTemplateFile))
                {
                    // �e���v���[�g���A�_��̏��ɓǂݎ��
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string name = line;
                        PointF[] points = new PointF[ResampleLength];
                        int i = 0;
                        foreach (string str in sr.ReadLine().Split(
                            new Char[] { Space }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            // �e�_�̓ǂݎ��
                            int index = str.IndexOf(Comma);
                            float x = float.Parse(str.Substring(0, index));
                            float y = float.Parse(str.Substring(index + 1));
                            points[i++] = new PointF(x, y);
                        }

                        // �e���v���[�g���X�g�ɒǉ�
                        templateList.Add(new Template(name, points));
                    }
                }
            }
        }


        /// <summary>
        /// �e���v���[�g���X�g��ۑ�����B
        /// </summary>
        private void SaveTemplateList()
        {
            using (StreamWriter sw = new StreamWriter(PhotoChat.StrokeTemplateFile))
            {
                // �e���v���[�g���A�_��̏��Ɋe�e���v���[�g����������
                foreach (Template template in templateList)
                {
                    sw.WriteLine(template.Name);
                    foreach (PointF p in template.Points)
                        sw.Write(p.X.ToString() + Comma + p.Y.ToString() + Space);
                    sw.WriteLine();
                }
                sw.Flush();
            }
        }

        #endregion


        #region �X�g���[�N�F���A�e���v���[�g�ǉ��E�擾�E�폜

        /// <summary>
        /// �X�g���[�N�F�����s���B
        /// </summary>
        /// <param name="points">�X�g���[�N�̓_��</param>
        /// <returns>�F�����ʁi�e���v���[�g���ƃX�R�A�j</returns>
        public Result Recognize(PointF[] points)
        {
            // �_��̒������Q�ȏ�̂Ƃ��̂ݔF������
            if (points.Length < 2)
                return new Result(string.Empty, 0);

            // Step1�F���T���v��
            PointF[] candidate = Resample(points, ResampleLength);

            // Step2�F��]
            PointF centroid = Centroid(candidate);
            candidate = Rotate(candidate, centroid);

            // Step3�F���T�C�Y�E���W�ϊ�
            candidate = ScaleToSquare(candidate, SquareSize);
            centroid = Centroid(candidate);
            candidate = TranslateToOrigin(candidate, centroid);

            // Step4�F�e���v���[�g�Ɣ�r
            double best = double.MaxValue;
            string name = string.Empty;
            foreach (Template template in templateList)
            {
                double distance = DistanceAtBestAngle(candidate, template.Points);
                if (best > distance)
                {
                    best = distance;
                    name = template.Name;
                }
            }
            return new Result(name, 1 - best / MaxDistance);
        }


        /// <summary>
        /// �X�g���[�N�F���̃e���v���[�g��ǉ�����B
        /// </summary>
        /// <param name="name">�ǉ�����e���v���[�g��</param>
        /// <param name="points">�e���v���[�g�̓_��</param>
        public void AddTemplate(string name, PointF[] points)
        {
            // Step1�F���T���v��
            PointF[] template = Resample(points, ResampleLength);

            // Step2�F��]
            PointF centroid = Centroid(template);
            template = Rotate(template, centroid);

            // Step3�F���T�C�Y�E���W�ϊ�
            template = ScaleToSquare(template, SquareSize);
            centroid = Centroid(template);
            template = TranslateToOrigin(template, centroid);

            // �e���v���[�g�ǉ�
            templateList.Add(new Template(name, template));
            SaveTemplateList();
        }


        /// <summary>
        /// �e���v���[�g���X�g����w�肵�����O�ƈ�v����ŏ��Ɍ��������e���v���[�g��Ԃ��B
        /// </summary>
        /// <param name="name">�e���v���[�g��</param>
        /// <returns>�ŏ��Ɍ��������e���v���[�g�B������Ȃ������ꍇ��Empty��Ԃ��B</returns>
        public Template GetTemplate(string name)
        {
            for (int i = 0; i < templateList.Count; i++)
            {
                if (templateList[i].Name == name)
                    return templateList[i];
            }
            return Template.Empty;
        }


        /// <summary>
        /// �e���v���[�g���X�g����w�肵�����O�̃e���v���[�g���폜����B
        /// </summary>
        /// <param name="name">�폜����e���v���[�g��</param>
        /// <returns>�폜�����ꍇ��true��Ԃ��B</returns>
        public bool RemoveTemplate(string name)
        {
            for (int i = 0; i < templateList.Count; i++)
            {
                if (templateList[i].Name == name)
                {
                    templateList.RemoveAt(i);
                    SaveTemplateList();
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// �e���v���[�g���X�g����w�肵���ʒu�ɂ���e���v���[�g���폜����B
        /// </summary>
        /// <param name="index">�폜����e���v���[�g�̃C���f�b�N�X</param>
        public void RemoveTemplateAt(int index)
        {
            templateList.RemoveAt(index);
            SaveTemplateList();
        }


        /// <summary>
        /// ���T���v���ƃ��T�C�Y���s�����摜�쐬�p�X�g���[�N�_����擾����B
        /// </summary>
        /// <param name="points">���̃X�g���[�N�_��</param>
        /// <returns>�摜�쐬�p�̃X�g���[�N�_��</returns>
        public PointF[] GetPointsForImage(PointF[] points)
        {
            // ���T���v��
            PointF[] newPoints = Resample(points, ResampleLength);

            // ���T�C�Y
            newPoints = ScaleToSquare(newPoints, SquareSize);

            // �`��̂��߂̍��W�ϊ�
            PointF centroid = Centroid(newPoints);
            centroid = new PointF(centroid.X - 25, centroid.Y - 25);
            return TranslateToOrigin(newPoints, centroid);
        }

        #endregion


        #region Step1�F���T���v��

        /// <summary>
        /// �_���num�̓��Ԋu�ȓ_��ɕϊ�����B
        /// </summary>
        /// <param name="points">���̓_��</param>
        /// <param name="num">�ϊ���̓_�̐�</param>
        /// <returns>�ϊ������_��</returns>
        private static PointF[] Resample(PointF[] points, int num)
        {
            // �V�����_��p�z��̍쐬�Ɗe�_�̋������v�Z
            PointF[] newPoints = new PointF[num];
            double space = PathLength(points) / (num - 1);

            // �_��̕ϊ�
            newPoints[0] = points[0];
            int index = 1, i = 1;
            double stock = 0;
            while (index <= num - 2)
            {
                // �����̒~�ς��ϊ���̓_�̋������z������V���ȓ_�����߂�
                double distance = Distance(points[i - 1], points[i]);
                if ((stock + distance) >= space)
                {
                    // �V���ȓ_�̍쐬
                    double scale = (space - stock) / distance;
                    double x = points[i - 1].X + ((points[i].X - points[i - 1].X) * scale);
                    double y = points[i - 1].Y + ((points[i].Y - points[i - 1].Y) * scale);
                    PointF point = new PointF((float)x, (float)y);

                    // �_�̒ǉ�
                    newPoints[index++] = point;

                    // ���̓_�����߂鏀��
                    points[i - 1] = point;
                    stock = 0;
                }
                else
                {
                    // �ϊ���̓_�̋����𒴂���܂ŋ�����~��
                    stock += distance;
                    i++;
                }
            }
            newPoints[num - 1] = points[points.Length - 1];

            return newPoints;
        }


        /// <summary>
        /// �_��S�̂̒��������߂�B
        /// </summary>
        /// <param name="points">�_��</param>
        /// <returns>�_��S�̂̒���</returns>
        private static double PathLength(PointF[] points)
        {
            double length = 0;
            for (int i = 1; i < points.Length; i++)
                length += Distance(points[i - 1], points[i]);
            return length;
        }

        #endregion


        #region Step2�F��]

        /// <summary>
        /// �n�_�Əd�S�����Ԓ����̊p�x��0�x�ɂȂ�悤�_�����]����B
        /// </summary>
        /// <param name="points">�_��</param>
        /// <param name="centroid">�_��̏d�S</param>
        /// <returns>��]�����_��</returns>
        private static PointF[] Rotate(PointF[] points, PointF centroid)
        {
            // �n�_�Əd�S�����Ԓ����̊p�x�����߂�
            double theta = Math.Atan2(points[0].Y - centroid.Y, points[0].X - centroid.X);

            // �p�x��0�x�ɂȂ�悤��]
            return RotateBy(points, centroid, -theta);
        }


        /// <summary>
        /// �_����w�肵���p�x������]����B
        /// </summary>
        /// <param name="points">�_��</param>
        /// <param name="center">��]�̒��S�ƂȂ�_</param>
        /// <param name="theta">��]����p�x�i���W�A���j</param>
        /// <returns>��]�����_��</returns>
        private static PointF[] RotateBy(PointF[] points, PointF center, double theta)
        {
            PointF[] newPoints = new PointF[points.Length];
            double cos = Math.Cos(theta);
            double sin = Math.Sin(theta);
            for (int i = 0; i < points.Length; i++)
            {
                // �e�_�𒆐S�_�����ɉ�]
                double dx = points[i].X - center.X;
                double dy = points[i].Y - center.Y;
                double x = dx * cos - dy * sin + center.X;
                double y = dx * sin + dy * cos + center.Y;
                newPoints[i] = new PointF((float)x, (float)y);
            }
            return newPoints;
        }

        #endregion


        #region Step3�F���T�C�Y�E���W�ϊ�

        /// <summary>
        /// �_����w�肵���T�C�Y�̐����`�ɍ����悤���T�C�Y����B
        /// </summary>
        /// <param name="points">�_��</param>
        /// <param name="size">�����`�̃T�C�Y</param>
        /// <returns>���T�C�Y�����_��</returns>
        private static PointF[] ScaleToSquare(PointF[] points, float size)
        {
            // �g��k���{�������߂�
            SizeF boundingSize = BoundingSize(points);
            float scaleX = size / boundingSize.Width;
            float scaleY = size / boundingSize.Height;

            // ���T�C�Y
            PointF[] newPoints = new PointF[points.Length];
            for (int i = 0; i < points.Length; i++)
                newPoints[i] = new PointF(points[i].X * scaleX, points[i].Y * scaleY);
            return newPoints;
        }


        /// <summary>
        /// �_����͂ގl�p�`�̃T�C�Y�����߂�
        /// </summary>
        /// <param name="points">�_��</param>
        /// <returns>�_����͂ގl�p�`�̃T�C�Y</returns>
        private static SizeF BoundingSize(PointF[] points)
        {
            float minX = points[0].X, minY = points[0].Y;
            float maxX = minX, maxY = minY;
            for (int i = 1; i < points.Length; i++)
            {
                // X������
                if (minX > points[i].X)
                    minX = points[i].X;
                else if (maxX < points[i].X)
                    maxX = points[i].X;

                // Y������
                if (minY > points[i].Y)
                    minY = points[i].Y;
                else if (maxY < points[i].Y)
                    maxY = points[i].Y;
            }
            return new SizeF(maxX - minX, maxY - minY);
        }


        /// <summary>
        /// �_���origin�����_�Ƃ�����W�n�ɕϊ�����B
        /// </summary>
        /// <param name="points">�_��</param>
        /// <param name="origin">���_�Ƃ���_</param>
        /// <returns>�ϊ������_��</returns>
        private static PointF[] TranslateToOrigin(PointF[] points, PointF origin)
        {
            PointF[] newPoints = new PointF[points.Length];
            for (int i = 0; i < points.Length; i++)
                newPoints[i] = new PointF(points[i].X - origin.X, points[i].Y - origin.Y);
            return newPoints;
        }

        #endregion


        #region Step4�F��r

        /// <summary>
        /// �Q�̓_�񂪍ł���v����p�x�����������@(Golden Section Search)�ɂ��T�����A
        /// ���̊p�x�ɂ�����e�_�̂���̑傫����Ԃ��B
        /// </summary>
        /// <param name="a">�_��A</param>
        /// <param name="b">�_��B</param>
        /// <returns>��r���ʁi�������قǗǂ����ʁj</returns>
        private static double DistanceAtBestAngle(PointF[] a, PointF[] b)
        {
            // ������
            double floorAngle = MinAngle;
            double ceilingAngle = MaxAngle;
            double angle1 = GoldenRatio * floorAngle + (1 - GoldenRatio) * ceilingAngle;
            double distanceAtAngle1 = PathDistance(RotateBy(a, Origin, angle1), b);
            double angle2 = (1 - GoldenRatio) * floorAngle + GoldenRatio * ceilingAngle;
            double distanceAtAngle2 = PathDistance(RotateBy(a, Origin, angle2), b);

            // ���������T���ōŗǂ̒l�����߂�
            while ((ceilingAngle - floorAngle) > AngleThreshold)
            {
                if (distanceAtAngle1 < distanceAtAngle2)
                {
                    // ������ړ�
                    ceilingAngle = angle2;
                    angle2 = angle1;
                    distanceAtAngle2 = distanceAtAngle1;
                    angle1 = GoldenRatio * floorAngle + (1 - GoldenRatio) * ceilingAngle;
                    distanceAtAngle1 = PathDistance(RotateBy(a, Origin, angle1), b);
                }
                else
                {
                    // �������ړ�
                    floorAngle = angle1;
                    angle1 = angle2;
                    distanceAtAngle1 = distanceAtAngle2;
                    angle2 = (1 - GoldenRatio) * floorAngle + GoldenRatio * ceilingAngle;
                    distanceAtAngle2 = PathDistance(RotateBy(a, Origin, angle2), b);
                }
            }
            return Math.Min(distanceAtAngle1, distanceAtAngle2);
        }


        /// <summary>
        /// �Q�̓_����r������̑傫�������߂�B
        /// </summary>
        /// <param name="a">�_��A</param>
        /// <param name="b">�_��B</param>
        /// <returns>��r���ʁi�傫���قǂ��ꂪ�傫���j</returns>
        private static double PathDistance(PointF[] a, PointF[] b)
        {
            double distance = 0;
            for (int i = 0; i < a.Length; i++)
                distance += Distance(a[i], b[i]);
            return distance / a.Length;
        }

        #endregion


        #region �����E�d�S

        /// <summary>
        /// �Q�_�Ԃ̋��������߂�B
        /// </summary>
        /// <param name="p1">�_�P</param>
        /// <param name="p2">�_�Q</param>
        /// <returns>�Q�_�Ԃ̋���</returns>
        private static double Distance(PointF p1, PointF p2)
        {
            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }


        /// <summary>
        /// �_��̏d�S�����߂�B
        /// </summary>
        /// <param name="points">�_��</param>
        /// <returns>�d�S</returns>
        private static PointF Centroid(PointF[] points)
        {
            float x = 0;
            float y = 0;
            for (int i = 0; i < points.Length; i++)
            {
                x += points[i].X;
                y += points[i].Y;
            }
            return new PointF(x / points.Length, y / points.Length);
        }

        #endregion


        #region �F�����ʍ\����

        /// <summary>
        /// �X�g���[�N�F���̌��ʂ��i�[����\����
        /// </summary>
        public struct Result
        {
            private string name;
            private double score;

            /// <summary>
            /// �F�����ʍ\���̂��쐬����B
            /// </summary>
            /// <param name="name">�ł��X�R�A�̍��������e���v���[�g�̖��O</param>
            /// <param name="score">�X�R�A</param>
            public Result(string name, double score)
            {
                this.name = name;
                this.score = score;
            }

            /// <summary>
            /// �ł��X�R�A�̍��������e���v���[�g�̖��O���擾����B
            /// </summary>
            public string Name
            {
                get { return name; }
            }

            /// <summary>
            /// �X�R�A���擾����B
            /// </summary>
            public double Score
            {
                get { return score; }
            }
        }

        #endregion


        #region �e���v���[�g�\����

        public struct Template
        {
            public static readonly Template Empty = new Template(string.Empty, new PointF[0]);
            private string name;
            private PointF[] points;

            /// <summary>
            /// �e���v���[�g�\���̂��쐬����B
            /// </summary>
            /// <param name="name">�e���v���[�g��</param>
            /// <param name="points">�e���v���[�g�_��</param>
            public Template(string name, PointF[] points)
            {
                this.name = name;
                this.points = points;
            }

            /// <summary>
            /// �e���v���[�g�����擾����B
            /// </summary>
            public string Name
            {
                get { return name; }
            }

            /// <summary>
            /// �e���v���[�g�̓_����擾����B
            /// </summary>
            public PointF[] Points
            {
                get { return points; }
            }
        }

        #endregion
    }
}
