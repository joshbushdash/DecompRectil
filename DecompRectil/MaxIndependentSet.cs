using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecompRectil
{
    static partial class BipartiteGrape
    {
        //最大独立点集就是最小覆盖点集的补集
        /// <summary>
        /// 返回两个hashset，第一个是左侧点序号，第二个是右侧点序号
        /// </summary>
        /// <param name="leftsNumber"></param>
        /// <param name="rightsNumber"></param>
        /// <param name="edges"></param>
        /// <returns></returns>
        public static Dictionary<int, HashSet<int>> MaxIndependentSet(int leftsNumber,
                                                       int rightsNumber,
                                                       IReadOnlyDictionary<int, HashSet<int>> edges)
        {
            Dictionary<int, HashSet<int>> minVertexCover = MinVertices(leftsNumber, rightsNumber, edges);
            int[] leftArray = Enumerable.Range(0, leftsNumber).ToArray();
            int[] rightArray = Enumerable.Range(0, rightsNumber).ToArray();
            HashSet<int> leftDependent = new HashSet<int>(leftArray.Except(minVertexCover[0]));
            HashSet<int> rightDependent = new HashSet<int>(rightArray.Except(minVertexCover[1]));

            Dictionary<int, HashSet<int>> dependentSet = new Dictionary<int, HashSet<int>>();
            dependentSet.Add(0, leftDependent);
            dependentSet.Add(1, rightDependent);
            return dependentSet;
        }
    }
}