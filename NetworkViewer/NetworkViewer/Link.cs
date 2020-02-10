using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace jp.tfv.network
{
    /// <summary>
    /// リンク情報
    /// </summary>
    class Link
    {
        private static readonly String FILE_NAME = "link.dat";

        // リンクID
        public int id { set; get; }
        // 上流側ノードID
        public int upNodeId { set; get; }
        // 下流側ノードID
        public int dnNodeId { set; get; }
        // 距離（m）
        public double distance { set; get; }
        // 車線数
        public int laneCount { set; get; }
        // 
        public char status { set; get; }

        public static bool readLinkFile(String rootDirectory, ref IDictionary<int, Link> linkDic)
        {
            // 区切り文字
            char[] delimiterChars = { ',', ' ' };

            try
            {
                using (StreamReader reader = new StreamReader(rootDirectory + FILE_NAME))
                {
                    while (reader.Peek() > 0)
                    {
                        string line = reader.ReadLine();
                        string[] values = line.Split(delimiterChars);

                        Link link = new Link();
                        link.id = Int32.Parse(values[0]);
                        link.upNodeId = Int32.Parse(values[1]);
                        link.dnNodeId = Int32.Parse(values[2]);
                        link.distance = Double.Parse(values[3]);
                        link.laneCount = Int32.Parse(values[4]);
                        link.status = Char.Parse(values[5]);

                        linkDic.Add(link.id, link);
                    }
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
