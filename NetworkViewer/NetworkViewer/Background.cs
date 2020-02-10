using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using jp.tfv.math;

namespace NetworkViewer
{
    class Background
    {
        // ID
        public int id { set; get; }
        // 属性
        public char attr { set; get; }
        // 上流側の座標
        public Coordinate upCoord { set; get; }
        // 下流側の座標
        public Coordinate dnCoord { set; get; }

        public static bool readBackgroundFile(String rootDirectory, ref IList<Background> backgroundList, ref IDictionary<int, Background> backgroundDic)
        {
            // 区切り文字
            char[] delimiterChars = { ' ' };

            try
            {
                using (StreamReader reader = new StreamReader(rootDirectory + "background.dat"))
                {
                    while (reader.Peek() > 0)
                    {
                        string line = reader.ReadLine();
                        string[] values = line.Split(delimiterChars);

                        Background lane = new Background();
                        lane.id = Int32.Parse(values[0]);
                        lane.attr = Char.Parse(values[1]);

                        lane.upCoord = new Coordinate(float.Parse(values[2]), float.Parse(values[3]));
                        lane.dnCoord = new Coordinate(float.Parse(values[4]), float.Parse(values[5]));

                        backgroundList.Add(lane);
                        backgroundDic.Add(lane.id, lane);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("background.dat読み込みでエラーが発生しました。");
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

    }
}
