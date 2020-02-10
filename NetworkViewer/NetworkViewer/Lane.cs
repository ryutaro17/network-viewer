using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NetworkViewer;
using System.Collections.Concurrent;
using jp.tfv.math;

using System.Runtime.CompilerServices;

namespace jp.tfv.network
{
    /// <summary>
    /// レーン情報
    /// </summary>
    internal class Lane
    {
        private static readonly String FILE_NAME_1 = "lane.dat";
        private static readonly String FILE_NAME_2 = "laneConnective.dat";

        // レーンID
        public int id { set; get; }
        // レーンのルール（）
        public char rule { set; get; }
        // 上流側の座標
        public Coordinate upCoord { set; get; }
        // 下流側の座標
        public Coordinate dnCoord { set; get; }
        // 上流側で接続されているレーンIDのリスト
        public List<int> upLaneIdList = new List<int>();
        // 下流側で接続されているレーンIDのリスト
        public List<int> dnLaneIdList = new List<int>();
        // レーンの距離(m)
        public double distance { set; get; }
        // 補完点のリスト
        public List<LaneComplementary> laneComplementaryList = new List<LaneComplementary>();

        public static bool readLaneFile(String rootDirectory, ref IList<Lane> laneList, ref IDictionary<int, Lane> laneDic)
        {
            // 区切り文字
            char[] delimiterChars = { ',', ' ' };

            try
            {
                using (StreamReader reader = new StreamReader(rootDirectory + FILE_NAME_1))
                {
                    while (reader.Peek() > 0)
                    {
                        string line = reader.ReadLine();
                        string[] values = line.Split(delimiterChars);

                        Lane lane = new Lane();
                        lane.id = Int32.Parse(values[0]);
                        lane.rule = Char.Parse(values[1]);

                        lane.upCoord = new Coordinate(float.Parse(values[2]), float.Parse(values[3]));
                        lane.dnCoord = new Coordinate(float.Parse(values[4]), float.Parse(values[5]));

                        if (values.Length > 6)
                        {
                            lane.distance = double.Parse(values[6]);
                            laneList.Add(lane);
                        }

                        laneDic.Add(lane.id, lane);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}で読み込みでエラーが発生しました。", FILE_NAME_1);
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        public static bool readLaneConnectiveFile(String rootDirectory, ref IDictionary<int, Lane> laneDic, ref IDictionary<int, List<int>> upLaneConnectiveDic, ref IDictionary<int, List<int>> dnLaneConnectiveDic)
        {
            // 区切り文字
            char[] delimiterChars = { ',', ' ' };

            try
            {
                using (StreamReader reader = new StreamReader(rootDirectory + FILE_NAME_2))
                {
                    while (reader.Peek() > 0)
                    {
                        string line = reader.ReadLine();
                        string[] values = line.Split(delimiterChars);

                        int upLaneId = Int32.Parse(values[0]);
                        int dnLaneId = Int32.Parse(values[1]);

                        Lane upLane = laneDic[upLaneId];
                        upLane.dnLaneIdList.Add(dnLaneId);

                        Lane dnLane = laneDic[dnLaneId];
                        dnLane.upLaneIdList.Add(upLaneId);

                        if (dnLaneConnectiveDic.ContainsKey(upLaneId))
                        {
                            List<int> dnLaneIdList = dnLaneConnectiveDic[upLaneId];
                            dnLaneIdList.Add(dnLaneId);
                        }
                        else
                        {
                            List<int> dnLaneIdList = new List<int>();
                            dnLaneIdList.Add(dnLaneId);
                            dnLaneConnectiveDic.Add(upLaneId, dnLaneIdList);
                        }

                        if (upLaneConnectiveDic.ContainsKey(dnLaneId))
                        {
                            List<int> upLaneIdList = upLaneConnectiveDic[dnLaneId];
                            upLaneIdList.Add(upLaneId);

                        }
                        else
                        {
                            List<int> upLaneIdList = new List<int>();
                            upLaneIdList.Add(upLaneId);
                            upLaneConnectiveDic.Add(dnLaneId, upLaneIdList);
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}で読み込みでエラーが発生しました。", FILE_NAME_2);
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }
    }
}