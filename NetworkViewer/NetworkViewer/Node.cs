using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using jp.tfv.math;

namespace jp.tfv.network
{
    /// <summary>
    /// ノード情報
    /// </summary>
    class Node
    {
        private static readonly String FILE_NAME = "node.dat";

        public int id { set; get; }
        public Coordinate coord { set; get; }

        public static bool readNodeFile(String rootDirectory, ref IDictionary<int, Node> nodeDic)
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

                        Node node = new Node();
                        node.id = Int32.Parse(values[0]);
                        node.coord = new Coordinate(float.Parse(values[1]), float.Parse(values[2]));

                        nodeDic.Add(node.id, node);
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
