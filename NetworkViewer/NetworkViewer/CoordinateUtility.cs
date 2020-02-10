using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace jp.tfv.math
{
    class CoordinateUtility
    {
        /// <summary>
        /// a1座標、a2座標を通過する直線とb1座標、b2座標を通過する直線の交点を計算します。
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <returns></returns>
        public static Coordinate intersection(Coordinate a1, Coordinate a2, Coordinate b1, Coordinate b2)
        {
            // 交点座標
            Coordinate cross_coord = new Coordinate();

            if (a1.x == b1.x && a1.y == b1.y)
            {
                // a1とb1が同じ座標の場合
                cross_coord.x = a1.x;
                cross_coord.y = a1.x;

                return cross_coord;
            }
            else if (a1.x == b2.x && a1.y == b2.y)
            {
                // a1とb2が同じ座標の場合
                cross_coord.x = a1.x;
                cross_coord.y = a1.y;

                return cross_coord;
            }
            else if (a2.x == b1.x && a2.y == b1.y)
            {
                // a2とb1が同じ座標の場合
                cross_coord.x = a2.x;
                cross_coord.y = a2.y;

                return cross_coord;
            }
            else if (a2.x == b2.x && a2.y == b2.y)
            {
                // a2とb2が同じ座標の場合
                cross_coord.x = a2.x;
                cross_coord.y = a2.y;

                return cross_coord;
            }

            // a1-a2, b1-b2の各長さが0でないかチェックする
            var distance_a = CoordinateUtility.getDistance(a1, a2);
            var distance_b = CoordinateUtility.getDistance(b1, b2);
            if (distance_a == 0 || distance_b == 0) throw new ArgumentException("distance is 0");

            // aベクトル、bベクトルを作成
            Vector2 vec_a = CoordinateUtility.getVector2(a1, a2);
            Vector2 vec_b = CoordinateUtility.getVector2(b1, b2);

            // a1-a2, b1-b2の向きが同じ方向か間逆の方向かをチェックする
            Boolean parallel = CoordinateUtility.checkParallel(vec_a, vec_b);
            if (parallel) throw new ArgumentException("this relationship is parallel");

            // a1-a2, b1-b2の各直線の傾き、切片を計算する
            float[] line_a = CoordinateUtility.getLineVariable(a1, a2);
            float[] line_b = CoordinateUtility.getLineVariable(b1, b2);

            if (float.IsNaN(line_a[0]))
            {
                // a1-a2がY軸に平行な直線の場合
                cross_coord.x = a1.x;
                cross_coord.y = line_b[0] * cross_coord.x + line_b[1]; //  y=ax+b
            }
            else if (float.IsNaN(line_b[0]))
            {
                // b1-b2がY軸に平行な直線の場合
                cross_coord.x = b1.x;
                cross_coord.y = line_a[0] * cross_coord.x + line_a[1]; //  y=ax+b
            }
            else if (line_a[0] == 0.0f)
            {
                // a1-a2がX軸に平行な直線の場合
                cross_coord.x = (line_a[1] - line_b[1]) / line_b[0]; //  x=(y-b)/a
                cross_coord.y = line_a[1];
            }
            else if (line_b[0] == 0.0f)
            {
                // b1-b2がX軸に平行な直線の場合
                cross_coord.x = (line_b[1] - line_a[1]) / line_a[0]; //  x=(y-b)/a
                cross_coord.y = line_b[1];
            }
            else
            {
                // 逆行列から交点を求める
                // m11, m12
                // m21, m22
                // m31=0, m32=0
                // の構造

                // y = p1*x + q1
                // y = p2*x + q2
                // を以下のように展開(行列)
                // |p1 -1||x| = |-p1|
                // |p2 -1||y| = |-p2|
                // x,yは逆行列を求めて右辺との行列の積で計算できる

                Matrix3x2 matrix = new Matrix3x2(line_a[0], -1.0f, line_b[0], -1.0f, 0.0f, 0.0f);
                // 逆行列
                Matrix3x2 matrix_invert;
                Matrix3x2.Invert(matrix, out matrix_invert);

                cross_coord.x = matrix_invert.M11 * -line_a[1] + matrix_invert.M12 * -line_b[1];
                cross_coord.y = matrix_invert.M21 * -line_a[1] + matrix_invert.M22 * -line_b[1];


            }

            return cross_coord;
        }

        /// <summary>
        /// a1-a2のベクトル（a1起点）とb2-b1のベクトル(b2起点)の交点を計算する
        /// 但し、平行ではなく交点がa1-a2とb2-b1の直線上に見つからないか、線分上に存在しない場合はa2,b1の中点を返す
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <returns></returns>
        public static Coordinate vectorIntersection(Coordinate a1, Coordinate a2, Coordinate b1, Coordinate b2)
        {
            Coordinate coord = CoordinateUtility.intersection(a1, a2, b1, b2);

            if (CoordinateUtility.getDistance(a1, coord) < CoordinateUtility.getDistance(a2, coord) || CoordinateUtility.getDistance(b2, coord) < CoordinateUtility.getDistance(b1, coord))
            {
                // 交点座標がa1またはb2に近いに近い場合
                if (!CoordinateUtility.checkPointAndLineSegment(a1, a2, coord) && !CoordinateUtility.checkPointAndLineSegment(b1, b2, coord))
                {
                    coord.x = (a2.x + b1.x) / 2.0f;
                    coord.y = (a2.y + b1.y) / 2.0f;
                }
            }

            return coord;
        }

        /// <summary>
        /// a1-a2線分とb2-b1の線分の交点を計算する
        /// 但し、平行ではなく交点がa1-a2とb2-b1の直線上に見つからないか、線分上に存在しない場合はnullを返す
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <returns></returns>
        public static Coordinate lineSegmentIntersection(Coordinate a1, Coordinate a2, Coordinate b1, Coordinate b2)
        {
            Coordinate coord = CoordinateUtility.intersection(a1, a2, b1, b2);

            if (CoordinateUtility.checkPointAndLineSegment(a1, a2, coord) && CoordinateUtility.checkPointAndLineSegment(b1, b2, coord))
            {
                return coord;
            }

            return null;
        }


        /// <summary>
        /// Vector2を返します
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <returns></returns>
        public static Vector2 getVector2(Coordinate a1, Coordinate a2)
        {
            return new Vector2(a2.x - a1.x, a2.y - a1.y);
        }

        /// <summary>
        /// a,bの間の距離を計算
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <returns></returns>
        public static float getDistance(Coordinate a1, Coordinate a2)
        {
            return (float)(Math.Sqrt(Math.Pow(a2.x - a1.x, 2) + Math.Pow(a2.y - a1.y, 2)));
        }

        /// <summary>
        /// 内積を計算します
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static float inner(Vector2 vec1, Vector2 vec2)
        {

            return vec1.X * vec2.X + vec1.Y * vec2.Y;

        }

        /// <summary>
        /// ベクトルが同じ方向か間逆の方向の場合trueを返します
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static Boolean checkParallel(Vector2 vec1, Vector2 vec2)
        {
            // 単位ベクトルにする
            Vector2 normalize_vec1 = Vector2.Normalize(vec1);
            Vector2 normalize_vec2 = Vector2.Normalize(vec2);

            if (normalize_vec1.X == normalize_vec2.X && normalize_vec1.Y == normalize_vec2.Y)
            {
                // ベクトルが方向を向いている
                return true;
            }
            else if (normalize_vec1.X == -normalize_vec2.X && normalize_vec1.Y == -normalize_vec2.Y)
            {
                // ベクトルが間逆の方向を向いている
                return true;
            }
            return false;
        }

        /// <summary>
        /// a1-a2ベクトル, b1-b2ベクトルが同じ方向か間逆の方向の場合trueを返します
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <returns></returns>
        public static Boolean checkParallel(Coordinate a1, Coordinate a2, Coordinate b1, Coordinate b2)
        {
            Vector2 vec1 = CoordinateUtility.getVector2(a1, a2);
            Vector2 vec2 = CoordinateUtility.getVector2(b1, b2);

            return checkParallel(vec1, vec2);
        }

        /// <summary>
        /// ２点の座標を引数にして、その２点の直線を求めます。
        /// 結果の[0]：傾き、[1]：切片
        /// ※[0]がfloat.NaNの時はY軸に平行の直線になります
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <returns></returns>
        public static float[] getLineVariable(Coordinate a1, Coordinate a2)
        {
            float[] variable = new float[2];

            // 傾きを求める
            if ((a2.x - a1.x) == 0)
            {
                // Y軸に並行する関数なのでaの値はNan
                variable[0] = float.NaN;

                // 切片はないのでここでreturn
                return variable;
            }
            else
            {
                variable[0] = (a2.y - a1.y) / (a2.x - a1.x);
            }

            // 切片を求める
            variable[1] = a1.y - variable[0] * a1.x;

            return variable;
        }

        /// <summary>
        /// a1→a2の直線に並行する座標を計算する
        /// size：平行に離す距離
        /// direction：平行にする方向を進行方向の右側か左側かの設定 true:左側、false:右側
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <param name="size"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static Coordinate[] calcParallelCoordinate(Coordinate a1, Coordinate a2, float size, bool direction)
        {
            Vector2 vec = CoordinateUtility.getVector2(a1, a2);

            double radian = Math.PI * 90.0 / 180.0;
            if (!direction) radian *= -1;
            // radianが正の場合、反時計回りに回転　負の場合、時計回りに回転

            // 回転行列
            float cos = (float)Math.Cos(radian);
            float sin = (float)Math.Sin(radian);
            Matrix3x2 matrix = new Matrix3x2(cos, -sin, sin, cos, 0, 0);

            // 直行するベクトル
            Vector2 vec_cross = new Vector2(matrix.M11 * vec.X + matrix.M12 * vec.Y, matrix.M21 * vec.X + matrix.M22 * vec.Y);

            Vector2 normalize_vec_cross = Vector2.Normalize(vec_cross);

            Coordinate[] coodinates = new Coordinate[2];

            // 起点側
            coodinates[0] = new Coordinate(a1.x + normalize_vec_cross.X * size, a1.y + normalize_vec_cross.Y * size);

            // 終点側
            coodinates[1] = new Coordinate(a2.x + normalize_vec_cross.X * size, a2.y + normalize_vec_cross.Y * size);

            return coodinates;

        }

        /// <summary>
        /// a1を中心にa2を回転させた座標を計算する。角度は、rotation > 0 時計回り、rotation < 0 反時計回り rotation は度
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static Coordinate calcRotationCoordinate(Coordinate a1, Coordinate a2, double rotation)
        {
            Vector2 vec = CoordinateUtility.getVector2(a1, a2);

            double radian = Math.PI * 90.0 / 180.0;

            // 回転行列
            float cos = (float)Math.Cos(radian);
            float sin = (float)Math.Sin(radian);
            Matrix3x2 matrix = new Matrix3x2(cos, -sin, sin, cos, 0, 0);

            // 直行するベクトル
            Vector2 vec_rotate = new Vector2(matrix.M11 * vec.X + matrix.M12 * vec.Y, matrix.M21 * vec.X + matrix.M22 * vec.Y);

            return new Coordinate(a1.x + vec_rotate.X, a1.y + vec_rotate.Y);
        }



        /// <summary>
        /// lineUp→lineDownの線分とpointの距離を計算
        /// </summary>
        /// <param name="lineUp"></param>
        /// <param name="lineDown"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static float calcPointAndLineSegment(Coordinate lineUp, Coordinate lineDown, Coordinate point)
        {
            double vec_x = lineDown.x - lineUp.x;
            double vec_y = lineDown.y - lineUp.y;

            double x2 = Math.Pow(vec_x, 2);
            double y2 = Math.Pow(vec_y, 2);

            double r2 = x2 + y2;
            double tt = -(vec_x * (lineUp.x - point.x) + vec_y * (lineUp.y - point.y));
            if (tt < 0)
            {
                // 線分の範囲外 lineX1,lineY1方向
                return (float)(Math.Pow(lineUp.x - point.x, 2) + Math.Pow(lineUp.y - point.y, 2));
            }
            if (tt > r2)
            {
                // 線分の範囲外 lineX2,lineY2方向
                return (float)(Math.Pow(lineDown.x - point.x, 2) + Math.Pow(lineDown.y - point.y, 2));
            }
            double f1 = vec_x * (lineUp.y - point.y) - vec_y * (lineUp.x - point.x);
            return (float)((f1 * f1) / r2);
        }

        /// <summary>
        /// 座標aから座標bの線分と点pとの距離を計算
        /// </summary>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        /// <param name="bX"></param>
        /// <param name="bY"></param>
        /// <param name="pX"></param>
        /// <param name="pY"></param>
        /// <returns></returns>
        public static float calcPointAndLineSegment(float aX, float aY, float bX, float bY, float pX, float pY)
        {
            double a = bX - aX;
            double b = bY - aY;

            double a2 = a * a;
            double b2 = b * b;

            double r2 = a2 + b2;
            double tt = -(a * (aX - pX) + b * (aY - pY));
            if (tt < 0)
            {
                // 線分の範囲外 lineX1,lineY1方向
                return (float)(Math.Pow(aX - pX, 2) + Math.Pow(aY - pY, 2));
            }
            if (tt > r2)
            {
                // 線分の範囲外 lineX2,lineY2方向
                return (float)(Math.Pow(bX - pX, 2) + Math.Pow(bY - pY, 2));
            }
            double f1 = a * (aY - pY) - b * (aX - pX);
            return (float)((f1 * f1) / r2);
        }

        /// <summary>
        /// 点から線分に下ろした垂線が線分と交わるかチェック
        /// </summary>
        /// <param name="lineUp"></param>
        /// <param name="lineDown"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool checkPointAndLineSegment(Coordinate lineUp, Coordinate lineDown, Coordinate point)
        {
            double vec_x = lineDown.x - lineUp.x;
            double vec_y = lineDown.y - lineUp.y;

            double x2 = Math.Pow(vec_x, 2);
            double y2 = Math.Pow(vec_y, 2);

            double r2 = x2 + y2;
            double tt = -(vec_x * (lineUp.x - point.x) + vec_y * (lineUp.y - point.y));

            if (tt < 0)
            {
                // 線分の範囲外 lineX1,lineY1方向
                return false;
            }
            if (tt > r2)
            {
                // 線分の範囲外 lineX2,lineY2方向
                return false;
            }
            return true;
        }


        /// <summary>
        /// 点から線分に下ろした垂線が線分と交わるかチェック
        /// </summary>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        /// <param name="bX"></param>
        /// <param name="bY"></param>
        /// <param name="pX"></param>
        /// <param name="pY"></param>
        /// <returns></returns>
        public static bool checkPointAndLineSegment(float aX, float aY, float bX, float bY, float pX, float pY)
        {
            double a = bX - aX;
            double b = bY - aY;

            double a2 = a * a;
            double b2 = b * b;

            double r2 = a2 + b2;
            double tt = -(a * (aX - pX) + b * (aY - pY));

            if (tt < 0)
            {
                // 線分の範囲外 lineX1,lineY1方向
                return false;
            }
            if (tt > r2)
            {
                // 線分の範囲外 lineX2,lineY2方向
                return false;
            }

            return true;
        }

    }
}
