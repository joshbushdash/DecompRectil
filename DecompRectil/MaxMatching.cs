using System;
using System.Collections.Generic;
using System.Linq;

namespace DecompRectil
{
    static partial class BipartiteGrape
    {
        //https://github.com/tatsuyafujisaki/hopcroft-karp
        /// <summary>
        /// 输入二分图左侧点个数，右侧点个数，及边；输出左、右点序列的配对
        /// </summary>
        /// <param name="leftsNumber"></param>
        /// <param name="rightsNumber"></param>
        /// <param name="edges"></param>
        /// <returns></returns>
        public static Dictionary<int, int> MaxMatching(int leftsNumber,
                                                       int rightsNumber,
                                                       IReadOnlyDictionary<int, HashSet<int>> edges)
        {
            // "distance" is from a starting left to another left when zig-zaging left, right, left, right, left in DFS.

            // Take the following for example:
            // left1 -> (unmatched edge) -> right1 -> (matched edge) -> left2 -> (unmatched edge) -> right2 -> (matched edge) -> left3
            // distance can be as follows.
            // distances[left1] = 0 (Starting left is distance 0.)
            // distances[left2] = distances[left1] + 1 = 1
            // distances[left3] = distances[left2] + 1 = 2

            // Note
            // Both a starting left and an ending left are unmatched with right.
            // Moving from left to right uses a unmatched edge.
            // Moving from right to left uses a matched edge.

            var distances = new Dictionary<int, long>();
            //队列，先进先出
            var q = new Queue<int>();

            // All lefts start as being unmatched with any right.
            int[] leftArray = Enumerable.Range(0, leftsNumber).ToArray();
            var toMatchedRight = leftArray.ToDictionary(s => s, s => -1);

            // All rights start as being unmatched with any left.
            int[] rightArray = Enumerable.Range(0, rightsNumber).ToArray();
            var toMatchedLeft = rightArray.ToDictionary(s => s, s => -1);

            // Note
            // toMatchedRight and toMatchedLeft are the same thing but inverse to each other.
            // Using either of them is enough but inefficient
            // because a dictionary cannot be straightforwardly looked up bi-directionally.

            while (HasAugmentingPath(leftArray, edges, toMatchedRight, toMatchedLeft, distances, q))
            {
                foreach (var unmatchedLeft in leftArray.Where(left => toMatchedRight[left] == -1))
                {
                    TryMatching(unmatchedLeft, edges, toMatchedRight, toMatchedLeft, distances);
                }
            }

            // Remove unmatches
            RemoveItems(toMatchedRight, kvp => kvp.Value == -1);

            // Return matches
            return toMatchedRight;
        }

        // BFS（广度优先搜索）
        //HK方法就是一次性找到多条不相交（没有公共点）的增广路径
        static bool HasAugmentingPath(IEnumerable<int> lefts,
                                      IReadOnlyDictionary<int, HashSet<int>> edges,
                                      IReadOnlyDictionary<int, int> toMatchedRight,
                                      IReadOnlyDictionary<int, int> toMatchedLeft,
                                      IDictionary<int, long> distances,
                                      Queue<int> q)
        {
            foreach (var left in lefts)
            {
                //用q记录左边未匹配顶点的个数，并初始化路程dis的个数为0
                //如果左边的点已匹配，则路程为正无穷
                if (toMatchedRight[left] == -1)
                {
                    distances[left] = 0;
                    q.Enqueue(left);
                }
                else
                {
                    distances[left] = long.MaxValue;
                }
            }
            //-1代表增广路劲的终点，找到这里就停止了，distance[-1]记录增广路径走过的路程数，初始设正无穷；且是所有增广路径的终点
            distances[-1] = long.MaxValue;

            while (0 < q.Count)
            {
                var left = q.Dequeue();

                //初始左点需要是未匹配的点，dis不等于正无穷，且该点的所有边均为未匹配的边
                if (distances[left] < distances[-1])
                {
                    //增广规则，下一个右点需要是有链接的边
                    foreach (var right in edges[left])
                    {
                        var nextLeft = toMatchedLeft[right];
                        //根据增广规则，下一个左点需要是匹配过的点，且未访问过，匹配过的点dis等于正无穷，访问过的点dis是一个正值
                        if (distances[nextLeft] == long.MaxValue)
                        {
                            // The nextLeft has not been visited and is being visited.
                            distances[nextLeft] = distances[left] + 1;
                            //该点加入队列末尾（后进后出），即又从左点q开始寻找下一条路径
                            q.Enqueue(nextLeft);
                        }
                    }
                }
            }
            //当-1没有赋值，则代表找不到增广路径了
            return distances[-1] != long.MaxValue;
        }

        // DFS（深度优先搜索）
        static bool TryMatching(int left,
                                IReadOnlyDictionary<int, HashSet<int>> edges,
                                IDictionary<int, int> toMatchedRight,
                                IDictionary<int, int> toMatchedLeft,
                                IDictionary<int, long> distances)
        {
            //-1为增广路径终点
            if (left == -1)
            {
                return true;
            }

            //dis只是存储了增广路径走过的左边的点，所以要找到这两个点之间是哪个右点
            foreach (var right in edges[left])
            {
                var nextLeft = toMatchedLeft[right];
                if (distances[nextLeft] == distances[left] + 1)
                {
                    if (TryMatching(nextLeft, edges, toMatchedRight, toMatchedLeft, distances))
                    {
                        toMatchedLeft[right] = left;
                        toMatchedRight[left] = right;
                        return true;
                    }
                }
            }

            // The left could not match any right.
            distances[left] = long.MaxValue;

            return false;
        }

        static void RemoveItems<T1, T2>(IDictionary<T1, T2> d, Func<KeyValuePair<T1, T2>, bool> isRemovable)
        {
            foreach (var kvp in d.Where(isRemovable).ToList())
            {
                d.Remove(kvp.Key);
            }
        }
    }
}