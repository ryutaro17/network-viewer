using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jp.tfv.math
{
    /// <summary>
    /// 座標計算用
    /// </summary>
    class Coordinate
    {
        public float x { set; get; }
        public float y { set; get; }

        public Coordinate()
        {
            this.x = float.NaN;
            this.y = float.NaN;
        }

        public Coordinate(float _x, float _y)
        {
            this.x = _x;
            this.y = _y;
        }

    }
}
