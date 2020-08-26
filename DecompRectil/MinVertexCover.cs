using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecompRectil
{
    static partial class BipartiteGrape
    {
        //https://stackoverflow.com/questions/12449554/find-minimum-vertex-cover-for-bipartite-graph-given-the-maximum-matching
        public static Dictionary<int, HashSet<int>> MinVertices(int leftsNumber,
                                                       int rightsNumber,
                                                       IReadOnlyDictionary<int, HashSet<int>> edges)
        {
            Dictionary<int, int> maxMatching = MaxMatching(leftsNumber, rightsNumber, edges);
            Dictionary<int, HashSet<int>> unmatchedEdge = new Dictionary<int, HashSet<int>>();
            foreach (var left in edges.Keys)
            {
                HashSet<int> unmatched = new HashSet<int>();
                foreach (var right in edges[left])
                {
                    if (maxMatching.Keys.Contains(left))
                    {
                        if (maxMatching[left] == right)
                            continue;
                    }
                    unmatched.Add(right);
                }
                unmatchedEdge.Add(left, unmatched);
            }

            int[] leftArray = Enumerable.Range(0, leftsNumber).ToArray();
            int[] rightArray = Enumerable.Range(0, rightsNumber).ToArray();
            int[] unmatchedLeft = leftArray.Except(maxMatching.Keys.ToArray()).ToArray();
            int[] unmatchedRight = leftArray.Except(maxMatching.Values.ToArray()).ToArray();
            Dictionary<int, HashSet<int>> cover = new Dictionary<int, HashSet<int>>();
            cover.Add(0, new HashSet<int>());
            cover.Add(1, new HashSet<int>());

            var q = new Queue<int>();

            //左侧未匹配点，开始走增广路，将左侧的点加入cover点集
            foreach (int left in unmatchedLeft)
            {
                q.Enqueue(left);
            }
            while (q.Count() > 0)
            {
                var start = q.Dequeue();
                if (unmatchedEdge.ContainsKey(start))
                {
                    foreach(int end in unmatchedEdge[start])
                    {
                        cover[1].Add(end);
                        if (maxMatching.ContainsValue(end))
                            q.Enqueue(maxMatching.First(p => p.Value == end).Key);
                        maxMatching.Remove(start);
                    }
                }
            }

            //右侧未匹配点，开始走增广路，将右侧的点加入cover点集
            foreach (int right in unmatchedRight)
            {
                q.Enqueue(right);
            }
            while (q.Count() > 0)
            {
                var start = q.Dequeue();
                foreach (int end in FindLeftByRight(unmatchedEdge, start))
                {
                    cover[0].Add(end);
                    if (maxMatching.ContainsKey(end))
                        q.Enqueue(maxMatching.First(p => p.Key == end).Value);
                    maxMatching.Remove(start);
                }
            }

            //剩余的未匹配的边上的点，加入cover点集
            foreach (int cover0 in cover[0])
            {
                if (maxMatching.ContainsKey(cover0))
                    maxMatching.Remove(cover0);
            }
            foreach (int cover1 in cover[1])
            {
                if (maxMatching.ContainsValue(cover1))
                    maxMatching.Remove(maxMatching.First(p => p.Value == cover1).Key);
            }
            foreach (int key in maxMatching.Keys)
            {
                cover[0].Add(key);
            }
            return cover;
        }

        static HashSet<int> FindLeftByRight(Dictionary<int, HashSet<int>> edges, int right)
        {
            //HashSet元素具有不重复性
            HashSet<int> lefts = new HashSet<int>();
            foreach (int left in edges.Keys)
            {
                foreach (int right1 in edges[left])
                {
                    if (right1 == right)
                        lefts.Add(left);
                }
            }
            return lefts;
        }
    }
}
