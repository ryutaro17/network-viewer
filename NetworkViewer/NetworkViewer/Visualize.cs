using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using jp.tfv.network;
using jp.tfv.math;
using System.Collections.Concurrent;
using System.Numerics;

namespace NetworkViewer
{
    class VisualizeFactory
    {

        public static Visualize createVisualize(DataStore _dataStore)
        {
            Visualize visualize = null;

            if (Visualize.VISUALIZE_TYPE == VisualizeType.V2D)
            {
                visualize = new Visualize2D(_dataStore);
            }
            else if (Visualize.VISUALIZE_TYPE == VisualizeType.V3D)
            {
                visualize = new Visualize3D(_dataStore);
            }
            else
            {
                throw new Exception("Visualize Type error.");
            }

            return visualize;
        }

    }

    enum VisualizeType : int
    {
        V2D,
        V3D
    }

    abstract class Visualize
    {
        public static readonly VisualizeType VISUALIZE_TYPE = VisualizeType.V3D;

        // 表示する画面のwidth
        public float screenWidth { set; get; }
        // 表示する画面のheight
        public float screenHeight { set; get; }

        protected DataStore dataStore;

        // ネットワーク表示用のColor
        protected Pen pen_lane = new Pen(Color.DarkGray, 2.0f);
        protected Pen pen_link = new Pen(Color.Blue, 2.0f);

        // 事故車両の色
        protected Brush accidentColor = new SolidBrush(Color.Tomato);

        // 一時的接触の車両の色
        protected Brush forcedLaneChangeColor = new SolidBrush(Color.BlueViolet);

        // 地図の中心座標（world）
        protected float[] mapCenter = new float[2];

        protected Visualize(DataStore _dataStore)
        {
            this.dataStore = _dataStore;
        }

        /// <summary>
        /// screen表示用に計算したx,yが描画する画面に入っているかチェックする
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        protected bool checkScreen(float x, float y)
        {
            if ((x >= 0) && (x <= screenWidth) && (y >= 0) && (y <= screenHeight))
            {
                return true;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 背景地図を描画したBitmapを返します
        /// </summary>
        /// <returns></returns>
        public abstract Bitmap createBitmap();

        /// <summary>
        /// 地図の中心の座標（world）を返します
        /// </summary>
        /// <returns></returns>
        public abstract float[] getMapCenter();

        /// <summary>
        /// 地図の中心位置を更新します
        /// </summary>
        /// <param name="pressMapCenter"></param>
        /// <param name="pressScreen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public abstract bool updateMapCenter(float[] pressMapCenter, int[] pressScreen, MouseEventArgs e);

        /// <summary>
        /// Zoomを更新します（マウスのWheelでの挙動）
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public abstract bool updateZoom(MouseEventArgs e);

        /// <summary>
        /// 描画対象のレーン
        /// </summary>
        protected class ViewBackGround
        {
            public float x1 { get; }
            public float y1 { get; }
            public float x2 { get; }
            public float y2 { get; }
            public Pen pen { get; }

            public ViewBackGround(float _x1, float _y1, float _x2, float _y2, Pen _pen)
            {
                this.x1 = _x1;
                this.y1 = _y1;
                this.x2 = _x2;
                this.y2 = _y2;
                this.pen = _pen;
            }
        }
    }

    class Visualize2D : Visualize
    {
        // 50mをscaleが1の時に1pxとする
        public static readonly float PER_PX = 50.0f;

        public float scale { set; get; } = 1.0f;

        // 地図の中心位置（world座標）
        public float mapCenterX { set; get; }
        public float mapCenterY { set; get; }

        public Visualize2D(DataStore _dataStore) : base(_dataStore)
        {
            float maxX= float.MinValue;
            float maxY = float.MinValue;

            float minX = float.MaxValue;
            float minY = float.MaxValue;

            foreach (Link link in dataStore.linkDic.Values)
            {
                Node upNode = dataStore.nodeDic[link.upNodeId];
                Node dnNode = dataStore.nodeDic[link.dnNodeId];

                if (minX > upNode.coord.x) minX = upNode.coord.x;
                if (maxX < upNode.coord.x) maxX = upNode.coord.x;
                if (minY > upNode.coord.y) minY = upNode.coord.y;
                if (maxY < upNode.coord.y) maxY = upNode.coord.y;

                if (minX > dnNode.coord.x) minX = dnNode.coord.x;
                if (maxX < dnNode.coord.x) maxX = dnNode.coord.x;
                if (minY > dnNode.coord.y) minY = dnNode.coord.y;
                if (maxY < dnNode.coord.y) maxY = dnNode.coord.y;
            }

            mapCenterX = (maxX + minX) / 2;
            mapCenterY = (maxY + minY) / 2;

            Console.WriteLine("centerX:{0}, centerY:{1}", mapCenterX, mapCenterY);

        }

        /// <summary>
        /// world座標mapXからscreenのXを計算する（原点はscreenの真ん中）
        /// </summary>
        /// <param name="mapX"></param>
        /// <returns></returns>
        public float calcScreenX(float mapX)
        {
            double dX = (mapX - mapCenterX) * scale / PER_PX + this.screenWidth / 2;
            return (float)dX;
        }

        /// <summary>
        /// world座標mapYからscreenのYを計算する（原点はscreenの真ん中）
        /// </summary>
        /// <param name="mapY"></param>
        /// <returns></returns>
        public float calcScreenY(float mapY)
        {
            double dY = -1 * ((mapY - mapCenterY) * scale / PER_PX - this.screenHeight / 2);
            return (float)dY;
        }

        /// <summary>
        /// screenのXからworld座標のmapXを計算する
        /// </summary>
        /// <param name="screenX"></param>
        /// <returns></returns>
        public float calcMapX(float screenX)
        {
            double mapX = (screenX + mapCenterX * (((double)scale) / PER_PX) - this.screenWidth / 2) / (((double)scale) / PER_PX);
            return (float)mapX;
        }

        /// <summary>
        /// screenのYからworld座標のmapYを計算する
        /// </summary>
        /// <param name="screenY"></param>
        /// <returns></returns>
        public float calcMapY(float screenY)
        {
            double mapY = (screenY + mapCenterY * (((double)scale) / PER_PX) - this.screenHeight / 2) / (((double)scale) / PER_PX) * -1;
            return (float)mapY;
        }

        public override Bitmap createBitmap()
        {
            Bitmap mapImage = new Bitmap((int)this.screenWidth, (int)this.screenHeight);
            Graphics imageG = Graphics.FromImage(mapImage);

#if PARALLEL
            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = 10;

            ConcurrentBag<ViewBackGround> viewBackgroundList = new ConcurrentBag<ViewBackGround>();
            Parallel.ForEach(dataStore.backgroundList, options, background =>
            {
                float x1 = calcScreenX(background.upCoord.x);
                float y1 = calcScreenY(background.upCoord.y);

                float x2 = calcScreenX(background.dnCoord.x);
                float y2 = calcScreenY(background.dnCoord.y);

                if (checkScreen(x1, y1) || checkScreen(x2, y2))
                {
                    ViewBackGround viewBackground = new ViewBackGround(x1, y1, x2, y2, pen_lane);
                    viewBackgroundList.Add(viewBackground);
                }
            });

            if (viewBackgroundList.Count() > 0)
            {
                foreach (ViewBackGround v in viewBackgroundList)
                {
                    imageG.DrawLine(v.pen, v.x1, v.y1, v.x2, v.y2);
                }
            }

#else
            List<ViewBackGround> viewBackgroundList = new List<ViewBackGround>();
            foreach (Background background in dataStore.backgroundList)
            {
                float x1 = calcScreenX(background.upCoord.x);
                float y1 = calcScreenY(background.upCoord.y);

                float x2 = calcScreenX(background.dnCoord.x);
                float y2 = calcScreenY(background.dnCoord.y);

                if (checkScreen(x1, y1) || checkScreen(x2, y2))
                {
                    ViewBackGround viewBackground = new ViewBackGround(x1, y1, x2, y2, pen_lane);
                    viewBackgroundList.Add(viewBackground);
                }
            }

                        if (viewBackgroundList.Count() > 0)
            {
                foreach (ViewBackGround v in viewBackgroundList)
                {
                    imageG.DrawLine(v.pen, v.x1, v.y1, v.x2, v.y2);
                }
            }

#endif

            imageG.Dispose();

            return mapImage;
        }

        /// <summary>
        /// 地図の中心位置（world座標）を更新します
        /// </summary>
        /// <param name="pressMapCenter"></param>
        /// <param name="pressScreen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public override bool updateMapCenter(float[] pressMapCenter, int[] pressScreen, MouseEventArgs e)
        {
            float diffX = calcMapX(e.Location.X) - calcMapX(pressScreen[0]);
            float diffY = calcMapY(e.Location.Y) - calcMapY(pressScreen[1]);

            if( diffX != 0.0 || diffY != 0.0)
            {
                mapCenterX = pressMapCenter[0] - diffX;
                mapCenterY = pressMapCenter[1] - diffY;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 地図の中心位置を返します
        /// </summary>
        /// <returns></returns>
        public override float[] getMapCenter()
        {
            mapCenter[0] = mapCenterX;
            mapCenter[1] = mapCenterY;
            return mapCenter;
        }

        /// <summary>
        /// マウスのイベントでZOOMを更新します
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override bool updateZoom(MouseEventArgs e)
        {

            // カーソル位置（Scale変更前）
            float prevMapX = calcMapX(e.Location.X);
            float prevMapY = calcMapY(e.Location.Y);

            // 縮尺を変える前のscale
            float prev_scale = scale;

            if (e.Delta > 0)
            {
                if (scale < 10)
                {
                    scale += 1;
                }
                else if (scale < 100)
                {
                    scale += 10;
                }
                else if (scale < 1000)
                {
                    scale += 50;
                }
                else
                {
                    scale += 100;
                }
            }
            else
            {
                if (scale <= 10)
                {
                    scale -= 1;
                }
                else if (scale <= 100)
                {
                    scale -= 10;
                }
                else if (scale <= 1000)
                {
                    scale -= 50;
                }
                else
                {
                    scale -= 100;
                }

                if (scale < 1) scale = 1;

            }

            if (prev_scale != scale)
            {
                // 拡大の中心をカーソルにする
                float mapX = calcMapX(e.Location.X);
                float mapY = calcMapY(e.Location.Y);

                float diffX = prevMapX - mapX;
                float diffY = prevMapY - mapY;

                mapCenterX += diffX;
                mapCenterY += diffY;

                return true;
            }
            else
            {
                return false;
            }
        }
    }

    class Visualize3D : Visualize
    {
        // 右手座標系
        // XZ平面に地図を表示

        // 地図の中心位置（world座標）
        public float mapCenterX { set; get; }
        public float mapCenterZ { set; get; }

        public float cameraX { set; get; }
        public float cameraY { set; get; }
        public float cameraZ { set; get; }

        public float rotation { set; get; } = 0.0f;
        public float angle { set; get; } = 30.0f;
        public float scale { set; get; } = 1.0f;

        protected Matrix4x4 vpMatrix;
        protected Matrix4x4 inv_vpMatrix = Matrix4x4.Identity;
        protected Matrix4x4 viewportMatrix;
        protected Matrix4x4 inv_viewportMatrix = Matrix4x4.Identity;

        public Visualize3D(DataStore _dataStore) : base(_dataStore)
        {
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            float minX = float.MaxValue;
            float minY = float.MaxValue;

            foreach (Link link in dataStore.linkDic.Values)
            {
                Node upNode = dataStore.nodeDic[link.upNodeId];
                Node dnNode = dataStore.nodeDic[link.dnNodeId];

                if (minX > upNode.coord.x) minX = upNode.coord.x;
                if (maxX < upNode.coord.x) maxX = upNode.coord.x;
                if (minY > upNode.coord.y) minY = upNode.coord.y;
                if (maxY < upNode.coord.y) maxY = upNode.coord.y;

                if (minX > dnNode.coord.x) minX = dnNode.coord.x;
                if (maxX < dnNode.coord.x) maxX = dnNode.coord.x;
                if (minY > dnNode.coord.y) minY = dnNode.coord.y;
                if (maxY < dnNode.coord.y) maxY = dnNode.coord.y;
            }

            mapCenterX = (maxX + minX) / 2;
            mapCenterZ = (maxY + minY) / 2;

            // この設定はカメラと始点の距離を設定するだけ、実際のカメラ位置はangleを使ってresetMatrixで作成される
            cameraX = mapCenterX;
            cameraY = 0;
            cameraZ = mapCenterZ - 5000.0f;

        }

        public override Bitmap createBitmap()
        {
            resetMatrix();

            // カメラと視点との距離
            double distance = Math.Sqrt(Math.Pow(cameraX - mapCenterX, 2) + Math.Pow(cameraY, 2) + Math.Pow(cameraZ - mapCenterZ, 2));

            Bitmap mapImage = new Bitmap((int)this.screenWidth, (int)this.screenHeight);

            Graphics imageG = Graphics.FromImage(mapImage);

            List<ViewBackGround> viewBackgroundList = new List<ViewBackGround>();
            foreach (Background background in dataStore.backgroundList)
            {
                Vector4[] disp = calcDisplayLineSegmentCoord(background.upCoord.x, background.upCoord.y, background.dnCoord.x, background.dnCoord.y);

                if (disp == null) continue;

                float x1 = disp[0].X;
                float y1 = disp[0].Y;

                float x2 = disp[1].X;
                float y2 = disp[1].Y;

                if (checkScreen(x1, y1) || checkScreen(x2, y2))
                {
                    ViewBackGround viewBackground = new ViewBackGround(x1, y1, x2, y2, pen_lane);
                    viewBackgroundList.Add(viewBackground);
                }
            }

            if (viewBackgroundList.Count() > 0)
            {
                foreach (ViewBackGround v in viewBackgroundList)
                {
                    imageG.DrawLine(v.pen, v.x1, v.y1, v.x2, v.y2);
                }
            }

            imageG.Dispose();

            return mapImage;
        }


        // マウスが10px以上動かないと地図の中心・カメラの視点を変更しないようにするために利用する変数
        private int prevScreenX = 0;
        private int prevScreenY = 0;
        public override bool updateMapCenter(float[] pressMapCenter, int[] pressScreen, MouseEventArgs e)
        {
            if (Math.Sqrt(Math.Pow(e.Location.X - prevScreenX, 2) + Math.Pow(e.Location.Y - prevScreenY, 2)) <= 10)
            {
                return false;
            }

            Vector4 mouseDownVec4 = this.calcDisplayToWorld(pressScreen[0], pressScreen[1]);
            Vector4 mouseMoveVec4 = this.calcDisplayToWorld(e.Location.X, e.Location.Y);

            float diff_x = mouseDownVec4.X - mouseMoveVec4.X;
            float diff_z = mouseDownVec4.Z - mouseMoveVec4.Z;

            // カメラと地図の中心の差分を計算
            float diff_camera_to_center_x = this.cameraX - this.mapCenterX;
            float diff_camera_to_center_z = this.cameraZ - this.mapCenterZ;

            // マウスで動かした分、地図の中心を移動する
            this.mapCenterX = pressMapCenter[0] + diff_x;
            this.mapCenterZ = pressMapCenter[1] + diff_z;

            // 地図の中心に事前に計算（カメラと地図の中心の差分）した差分を足し合わせる
            this.cameraX = mapCenterX + diff_camera_to_center_x;
            this.cameraZ = mapCenterZ + diff_camera_to_center_z;

            prevScreenX = e.Location.X;
            prevScreenY = e.Location.Y;

            return true;
        }

        public override float[] getMapCenter()
        {
            mapCenter[0] = mapCenterX;
            mapCenter[1] = mapCenterZ;
            return mapCenter;
        }

        public override bool updateZoom(MouseEventArgs e)
        {
            // カメラと視点との距離が100mを切ったら寄らない
            double distance = Math.Sqrt(Math.Pow(this.cameraX - this.mapCenterX, 2) + Math.Pow(this.cameraY, 2) + Math.Pow(this.cameraZ - this.mapCenterZ, 2));

            //Console.WriteLine("distance:{0}", distance);

            if (e.Delta < 0)
            {
                if (distance > 50000.0f) return false;
                this.scale = 1.2f;
            }
            else
            {
                if (distance < 500) return false;
                this.scale = 1 / 1.2f;
            }

            {
                /*
                // 拡大の中心位置は画面中心
                float diff_x = (view3D.cameraX - view3D.mapCenterX) * view3D.scale;
                float diff_z = (view3D.cameraZ - view3D.mapCenterZ) * view3D.scale;

                view3D.cameraX = view3D.mapCenterX + diff_x;
                view3D.cameraZ = view3D.mapCenterZ + diff_z;
                view3D.cameraY = view3D.cameraY * view3D.scale;
                */
            }

            {
                // 拡大の中心位置はカーソル
                Vector4 vec4 = this.calcDisplayToWorld(e.Location.X, e.Location.Y);

                float diff_x = (this.cameraX - this.mapCenterX) * this.scale;
                float diff_z = (this.cameraZ - this.mapCenterZ) * this.scale;

                this.mapCenterX = vec4.X + (this.mapCenterX - vec4.X) * this.scale;
                this.mapCenterZ = vec4.Z + (this.mapCenterZ - vec4.Z) * this.scale;

                this.cameraX = this.mapCenterX + diff_x;
                this.cameraZ = this.mapCenterZ + diff_z;
                this.cameraY = this.cameraY * this.scale;
            }

            return true;
        }

        public void resetMatrix()
        {
            // カメラと視点の距離
            float distance_camera_to_target = (float)Math.Sqrt(Math.Pow(cameraX - mapCenterX, 2) + Math.Pow(cameraY, 2) + Math.Pow(cameraZ - mapCenterZ, 2));

            // X軸でangle回転させるマトリクス(-angleとマイナスにしている)
            Matrix4x4 rotXMatrix = Matrix4x4.CreateRotationX((float)(Math.PI * -angle / 180.0));

            // Y軸でrotation回転させるマトリクス
            Matrix4x4 rotYMatrix = Matrix4x4.CreateRotationY((float)(Math.PI * rotation / 180.0));

            // X軸回転→Y軸回転のマトリクス
            Matrix4x4 rotMatrix = rotXMatrix * rotYMatrix;

            // カメラの位置を一時的にZ軸上から原点を視点とするようにする
            Vector4 camera_vec4_tmp = new Vector4(0, 0, -distance_camera_to_target, 1);

            Vector4 camera_vec4 = Vector4.Transform(camera_vec4_tmp, rotMatrix);

            cameraX = camera_vec4.X + mapCenterX;
            cameraZ = camera_vec4.Z + mapCenterZ;
            cameraY = camera_vec4.Y;

            // カメラの位置
            Vector3 cameraPosition = new Vector3(cameraX, cameraY, cameraZ);
            // 焦点の位置
            Vector3 targetPosition = new Vector3(mapCenterX, 0, mapCenterZ);
            // カメラの上方向
            Vector3 cameraUp = new Vector3(0, -1, 0);

            // ビュー変換行列
            Matrix4x4 viewMatrix = Matrix4x4.CreateLookAt(cameraPosition, targetPosition, cameraUp);
            // パースペクティブ変換行列
            Matrix4x4 perspectiveMatrix = Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI * 60.0 / 180.0), screenWidth / screenHeight, 100.0f, 5000.0f);

            // ビューポート変換行列
            viewportMatrix = Matrix4x4.Identity;
            viewportMatrix.M11 = screenWidth / 2;
            viewportMatrix.M41 = screenWidth / 2;
            viewportMatrix.M22 = -screenHeight / 2;
            viewportMatrix.M42 = screenHeight / 2;

            vpMatrix = viewMatrix * perspectiveMatrix;

            bool bool_inv_viewport = Matrix4x4.Invert(viewportMatrix, out inv_viewportMatrix);
            if (!bool_inv_viewport)
            {
                Console.WriteLine("viewport invert 失敗");
            }

            vpMatrix = viewMatrix * perspectiveMatrix;

            bool bool_inv_vp = Matrix4x4.Invert(vpMatrix, out inv_vpMatrix);
            if (!bool_inv_vp)
            {
                Console.WriteLine("vp invert 失敗");
            }
        }

        public Vector4 calc3Dto2D(Vector4 vector)
        {
            return Vector4.Transform(vector, vpMatrix);
        }

        public Vector4? calcDisplayCoord(float world_x, float world_y, float world_z)
        {
            Vector4 vec3d = new Vector4(world_x, world_y, world_z, 1);
            Vector4 vec2d = calc3Dto2D(vec3d);

            Console.WriteLine("{0}", vec2d.ToString());

            /// Wがマイナスだと画面上側に映りこみがある（私的におこなっている）
            if (vec2d.W < 0)
            {
                return null;
            }
            else
            {
                vec2d = vec2d / vec2d.W;
                return Vector4.Transform(vec2d, viewportMatrix);
            }
        }

        public Vector4[] calcDisplayLineSegmentCoord(float world_x1, float world_y1, float world_x2, float world_y2)
        {
            Vector4 vec3d_1 = new Vector4(world_x1, 0, world_y1, 1);
            Vector4 vec2d_1 = calc3Dto2D(vec3d_1);

            Vector4 vec3d_2 = new Vector4(world_x2, 0, world_y2, 1);
            Vector4 vec2d_2 = calc3Dto2D(vec3d_2);

            if (vec2d_1.W <= 0 && vec2d_2.W <= 0)
            {
                return null;
            }
            else if (vec2d_1.W > 0 && vec2d_2.W > 0)
            {
                vec2d_1 = vec2d_1 / vec2d_1.W;
                vec2d_2 = vec2d_2 / vec2d_2.W;

                return new Vector4[] { Vector4.Transform(vec2d_1, viewportMatrix), Vector4.Transform(vec2d_2, viewportMatrix) };
            }
            else
            {
                // XZ平面上で、カメラと焦点のベクトルに直行し且つカメラ（XZ）を通過するベクトルと、起点・終点の交差する点を求める
                // カメラ（XZ）を中心に視点を時計回りに回転させた座標
                Coordinate camera_coord = new Coordinate(cameraX, cameraZ);
                Coordinate focus_coord = new Coordinate(mapCenterX, mapCenterZ);
                Coordinate focus_rotate_coord = CoordinateUtility.calcRotationCoordinate(camera_coord, focus_coord, 90.0);

                // カメラ（XZ）と求めた座標をとおる線と起点・終点の交点を計算
                Coordinate intersection_coord = CoordinateUtility.intersection(camera_coord, focus_rotate_coord, new Coordinate(world_x1, world_y1), new Coordinate(world_x2, world_y2));

                if (intersection_coord != null)
                {
                    if (vec2d_1.W < 0)
                    {
                        vec3d_1 = new Vector4(intersection_coord.x, 0, intersection_coord.y, 1);
                        vec2d_1 = calc3Dto2D(vec3d_1);

                    }
                    else
                    {
                        vec3d_2 = new Vector4(intersection_coord.x, 0, intersection_coord.y, 1);
                        vec2d_2 = calc3Dto2D(vec3d_2);
                    }

                    if (vec2d_1.W <= 0 && vec2d_2.W <= 0)
                    {
                        return null;
                    }
                    else if (vec2d_1.W > 0 && vec2d_2.W > 0)
                    {
                        vec2d_1 = vec2d_1 / vec2d_1.W;
                        vec2d_2 = vec2d_2 / vec2d_2.W;

                        return new Vector4[] { Vector4.Transform(vec2d_1, viewportMatrix), Vector4.Transform(vec2d_2, viewportMatrix) };
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// ディスプレイのクリックした位置をワールド座標に変換します
        /// </summary>
        /// <param name="clickX"></param>
        /// <param name="clickY"></param>
        /// <returns></returns>
        public Vector4 calcDisplayToWorld(float clickX, float clickY)
        {
            resetMatrix();

            Vector4 click_vec4 = new Vector4(clickX, clickY, 0.0f, 1.0f);
            Vector4 world_vec4 = Vector4.Transform(click_vec4, inv_viewportMatrix);

            //Console.WriteLine("disp x:{0} y:{1} z:{2} w:{3}", world_vec4.X, world_vec4.Y, world_vec4.Z, world_vec4.W);

            world_vec4.Z = (inv_vpMatrix.M12 * world_vec4.X + inv_vpMatrix.M22 * world_vec4.Y + inv_vpMatrix.M42) * -1.0f / inv_vpMatrix.M32;

            float W = 1.0f / (inv_vpMatrix.M14 * world_vec4.X + inv_vpMatrix.M24 * world_vec4.Y + inv_vpMatrix.M34 * world_vec4.Z + inv_vpMatrix.M44);

            world_vec4 = world_vec4 * W;
            world_vec4 = Vector4.Transform(world_vec4, inv_vpMatrix);

            //Console.WriteLine("world x:{0} y:{1} z:{2} w:{3}", world_vec4.X, world_vec4.Y, world_vec4.Z, world_vec4.W);

            return world_vec4;
        }

    }

}
