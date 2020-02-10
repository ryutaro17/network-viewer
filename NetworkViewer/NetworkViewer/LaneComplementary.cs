using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using jp.tfv.math;
using jp.tfv.network;
using System.IO;

namespace NetworkViewer
{
    /// <summary>
    /// レーンの補完点データ
    /// </summary>
    class LaneComplementary
    {
        private static readonly String FILE_NAME = "laneComplementary.dat";

        // レーン序列番号
        public int index { set; get; }
        // 起点からの距離(m)
        public double distance { set; get; }
        // 座標
        public Coordinate coord { set; get; }

        public static bool readLaneComplementaryFile(String rootDirectory, IDictionary<int, Lane> laneDic)
        {
            // 区切り文字
            char[] delimiterChars = { ',', ' ' };

            try
            {
                // ファイルが存在しなかったらtrueで返す
                if (!System.IO.File.Exists(rootDirectory + FILE_NAME)) return true;

                using (StreamReader reader = new StreamReader(rootDirectory + FILE_NAME))
                {
                    while (reader.Peek() > 0)
                    {
                        string line = reader.ReadLine();
                        string[] values = line.Split(delimiterChars);

                        int laneId = Int32.Parse(values[0]);

                        LaneComplementary laneComplementary = new LaneComplementary();
                        laneComplementary.index = Int32.Parse(values[1]);
                        laneComplementary.distance = Double.Parse(values[2]);
                        laneComplementary.coord = new Coordinate(float.Parse(values[3]), float.Parse(values[4]));

                        Lane lane = laneDic[laneId];
                        lane.laneComplementaryList.Add(laneComplementary);
                    }
                }

                // laneComplementaryのリストのソートを行う
                foreach (KeyValuePair<int, Lane> kvp in laneDic)
                {
                    Lane lane = kvp.Value;
                    // TODO:一時的に先頭にlaneの起点座標、末尾にlaneの終点座標を入れる
                    LaneComplementary laneComplementary1 = new LaneComplementary();
                    laneComplementary1.index = 0;
                    laneComplementary1.distance = 0;
                    laneComplementary1.coord = lane.upCoord;
                    lane.laneComplementaryList.Add(laneComplementary1);

                    LaneComplementary laneComplementary2 = new LaneComplementary();
                    laneComplementary2.index = lane.laneComplementaryList.Count;
                    laneComplementary2.distance = lane.distance;
                    laneComplementary2.coord = lane.dnCoord;
                    lane.laneComplementaryList.Add(laneComplementary2);

                    lane.laneComplementaryList.Sort((a, b) => a.index - b.index);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("{0}で読み込みでエラーが発生しました。", FILE_NAME);
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }
    }
}
