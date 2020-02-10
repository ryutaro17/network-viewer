using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using jp.tfv.network;

namespace NetworkViewer
{
    class DataStore
    {
        public IDictionary<int, Node> nodeDic = new Dictionary<int, Node>();

        public IDictionary<int, Link> linkDic = new Dictionary<int, Link>();

        public IList<Lane> laneList = new List<Lane>();
        public IDictionary<int, Lane> laneDic = new Dictionary<int, Lane>();

        // 背景
        public IList<Background> backgroundList = new List<Background>();
        public IDictionary<int, Background> backgroundDic = new Dictionary<int, Background>();

        // key:レーンIDの上流で接続されているリンクIDのリスト
        public IDictionary<int, List<int>> upLaneConnectiveDic = new Dictionary<int, List<int>>();
        // key:レーンIDの下流で接続されているリンクIDのリスト
        public IDictionary<int, List<int>> dnLaneConnectiveDic = new Dictionary<int, List<int>>();

        public DataStore()
        {
            if( !readData())
            {
                throw new Exception("データの読み込みに失敗しました。パスの設定やデータファイルについて確認してください。");
            }
            Console.WriteLine("データの読み込みに成功しました");
        }

        private bool readData()
        {
            bool result = true;

            String dataDir = Properties.Settings.Default.dataDir;

            result = Node.readNodeFile(dataDir, ref nodeDic);
            if (!result)
            {
                Console.WriteLine("Nodeデータの読み込みに失敗しました");
                return result;
            }

            result = Link.readLinkFile(dataDir, ref linkDic);
            if (!result)
            {
                Console.WriteLine("Linkデータの読み込みに失敗しました");
                return result;
            }

            result = Lane.readLaneFile(dataDir, ref laneList, ref laneDic);
            if (!result)
            {
                Console.WriteLine("Laneデータの読み込みに失敗しました");
                return result;
            }

            result = LaneComplementary.readLaneComplementaryFile(dataDir, laneDic);
            if (!result)
            {
                Console.WriteLine("LaneComplementaryデータの読み込みに失敗しました");
                return result;
            }

            result = Lane.readLaneConnectiveFile(dataDir, ref laneDic, ref upLaneConnectiveDic, ref dnLaneConnectiveDic);
            if (!result)
            {
                Console.WriteLine("LaneConnectiveデータの読み込みに失敗しました");
                return result;
            }

            result = Background.readBackgroundFile(dataDir, ref backgroundList, ref backgroundDic);
            if (!result)
            {
                Console.WriteLine("Backgroundデータの読み込みに失敗しました");
                return result;
            }

            return result;
        }

    }
}
