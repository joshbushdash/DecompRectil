using System;
using System.Linq;
using System.Collections.Generic;

namespace DecompRectil
{
    public class PartioningManager
    {
        List<Vertice> Vertices = new List<Vertice>();
        List<Chord> HorizontalChord = new List<Chord>();
        List<Chord> VerticalChord = new List<Chord>();

        public PartioningManager(List<float[]> roomWallPList)
        {
            //序列最后一个元素为最低点的横墙，且从左往右方向
            roomWallPList.Sort((a, b) => b[1].CompareTo(a[1]));
            int index = roomWallPList.Count - 1;
            int iw = index;
            //非横墙
            while (roomWallPList[index][1] != roomWallPList[index][3])
            {
                var temp = roomWallPList[iw];
                roomWallPList[iw--] = roomWallPList[index];
                roomWallPList[index] = temp;
            }
            if (roomWallPList[index][0] > roomWallPList[index][2])
            {
                SwapPoint(roomWallPList[index]);
            }
            //墙首尾相接排序，顺序自动为逆时针
            for (int p = index, q = 0; q < index + 1; p = q++)
            {
                for (int qq = q; qq < index + 1; qq++)
                {
                    if (roomWallPList[p][2] == roomWallPList[qq][0] && roomWallPList[p][3] == roomWallPList[qq][1])
                    {
                        var temp = roomWallPList[q];
                        roomWallPList[q] = roomWallPList[qq];
                        roomWallPList[qq] = temp;
                        break;
                    }
                    else if (roomWallPList[p][2] == roomWallPList[qq][2] && roomWallPList[p][3] == roomWallPList[qq][3])
                    {
                        SwapPoint(roomWallPList[qq]);
                        var tempR = roomWallPList[q];
                        roomWallPList[q] = roomWallPList[qq];
                        roomWallPList[qq] = tempR;
                        break;
                    }
                    if (qq == index)
                    {
                        //ErrorMessage.AddMessage(roomWallPList[p].m_ID, "该墙在房间" + room.m_ID + "中，墙尾端没有相接的墙");
                        return;
                    }
                }
            }
            //判断每个点的凹凸性，并把凹点和凸点添加到Vertices中，目标点是cur的startPoint
            var pre = roomWallPList[index - 1];
            var cur = roomWallPList[index];
            int num = 0;
            for (int i = 0; i < index + 1; i++)
            {
                pre = cur;
                cur = roomWallPList[i];
                bool concave = false;
                if (pre[1] == pre[3])
                {
                    //跳过180°的点
                    if (cur[1] == cur[3])
                        continue;
                    bool dirct0 = pre[0] < cur[0];
                    bool dirct1 = cur[1] < cur[3];
                    concave = dirct0 != dirct1;
                }
                else
                {
                    if (cur[1] != cur[3])
                        continue;
                    bool dirct0 = pre[1] < cur[1];
                    bool dirct1 = cur[0] < cur[2];
                    concave = dirct0 == dirct1;
                }
                Vertices.Add(new Vertice(new Point2D(cur[0], cur[1]), num++, concave));
            }
            for (int p = num - 1, q = 0; q < num; p = q++)
            {
                //添加前后点
                Vertices[p].next = Vertices[q];
                Vertices[q].previous = Vertices[p];
                //定义水平和竖直弦，跳过当前点，和相邻的凹点
                for (int qq = p + 2; qq < num; qq++)
                {
                    //跳过同一个点，跳过凸点
                    if (!Vertices[p].concave || !Vertices[qq].concave)
                        continue;
                    if (Vertices[p].point.x == Vertices[qq].point.x)
                    {
                        Chord chord = new Chord(Vertices[p].point.y, Vertices[qq].point.y, Vertices[p].point.x, Vertices[p], Vertices[qq], false);
                        if (chord.CheckChord(Vertices))
                            VerticalChord.Add(chord);
                    }
                    else if (Vertices[p].point.y == Vertices[qq].point.y)
                    {
                        Chord chord = new Chord(Vertices[p].point.x, Vertices[qq].point.x, Vertices[p].point.y, Vertices[p], Vertices[qq], true);
                        if (chord.CheckChord(Vertices))
                            HorizontalChord.Add(chord);
                    }
                }
            }
        }

        /// <summary>
        /// 判断是否是矩形房间
        /// </summary>
        /// <returns></returns>
        public bool CheckRecRoom()
        {
            if (Vertices.Count == 4)
                return true;
            else
                return false;
        }

        //https://github.com/mikolalysenko/rectangle-decomposition
        //及论文http://library.utia.cas.cz/separaty/2012/ZOI/suk-rectangular%20decomposition%20of%20binary%20images.pdf
        public List<float[]> Partion()
        {
            Dictionary<int, HashSet<int>> crossing = new Dictionary<int, HashSet<int>>();
            //找到相交弦，第一坐标为水平chord，第二坐标为竖向chord
            for (int i = 0; i < HorizontalChord.Count; i++)
            {
                HashSet<int> iCrossing = new HashSet<int>();
                for (int j = 0; j < VerticalChord.Count; j++)
                {
                    if ((HorizontalChord[i].location - VerticalChord[j].start) * (HorizontalChord[i].location - VerticalChord[j].end) <= 0
                        && (VerticalChord[j].location - HorizontalChord[i].start) * (VerticalChord[j].location - HorizontalChord[i].end) <= 0)
                        iCrossing.Add(j);
                }
                crossing.Add(i, iCrossing);
            }
            Dictionary<int, HashSet<int>> maxIndependetSet = BipartiteGrape.MaxIndependentSet(HorizontalChord.Count, VerticalChord.Count, crossing);
            List<Chord> independentChore = new List<Chord>();
            foreach (int i in maxIndependetSet[0])
            {
                independentChore.Add(HorizontalChord[i]);
            }
            foreach (int i in maxIndependetSet[1])
            {
                independentChore.Add(VerticalChord[i]);
            }
            foreach (Chord chord in independentChore)
            {
                splitSegment(chord);
            }
            splitConcave();
            return findRegions();
        }

        private void splitSegment(Chord independentChore)
        {
            var a = independentChore.startP;
            var b = independentChore.endP;
            var pa = a.previous;
            var na = a.next;
            var pb = b.previous;
            var nb = b.next;


            //Fix concavity
            a.concave = false;
            b.concave = false;

            //Compute orientation
            bool ao, bo;
            //a为左方或者下方的点
            if (independentChore.m_DirectionIsX)
            {
                //a的方向为真/假的情况
                //  --->a       |  
                //      |       V
                //      V   <---a
                //b的方向为真/假的情况
                //   ^          b--->
                //   |          ^       
                //   b<---      |        
                ao = pa.point.y == a.point.y;
                bo = pb.point.y == b.point.y;
            }
            else
            {
                //a的方向为真/假的情况
                //   a--->    --->a   
                //   ^            |            
                //   |            V           
                //b的方向为真/假的情况
                //     |      ^      
                //     V      |         
                // <---b      b<---  
                ao = pa.point.x == a.point.x;
                bo = pb.point.x == b.point.x;
            }
            if (ao && bo)
            {
                //Case 1:
                //            ^
                //            |
                //  --->A+++++B<---
                //      |
                //      V
                a.previous = pb;
                pb.next = a;
                b.previous = pa;
                pa.next = b;
            }
            else if (ao && !bo)
            {
                //Case 2:
                //  --->A+++++B--->
                //      |     ^
                //      V     |
                a.previous = b;
                b.next = a;
                pa.next = nb;
                nb.previous = pa;
            }
            else if (!ao && bo)
            {
                //Case 3:
                //      |     ^
                //      V     |
                //  <---A+++++B<---
                a.next = b;
                b.previous = a;
                na.previous = pb;
                pb.next = na;
            }
            else if (!ao && !bo)
            {
                //Case 4:
                //      |
                //      V
                //  <---A+++++B--->
                //            ^
                //            |
                a.next = nb;
                nb.previous = a;
                b.next = na;
                na.previous = b;
            }
        }

        private void splitConcave()
        {
            List<Chord> toLeft = new List<Chord>();
            List<Chord> toRight = new List<Chord>();
            foreach (Vertice v in Vertices)
            {
                if (v.next.point.y == v.point.y)
                {
                    if (v.next.point.x < v.point.x)
                        toLeft.Add(new Chord(v.point.x, v.next.point.x , 0, v, v.next, true));
                    else
                        toRight.Add(new Chord(v.point.x, v.next.point.x, 0, v, v.next, false));
                }
            }
            for (int i = 0; i < Vertices.Count; i++)
            {
                var v = Vertices[i];
                if (!v.concave)
                    continue;

                //判断空间内部是在点的上面还是下面，正值为在上，负值为在下
                int direction = -1;
                if (v.previous.point.x == v.point.x)
                {
                    if (v.previous.point.y < v.point.y)
                        direction = 1;
                }
                else
                {
                    if (v.next.point.y < v.point.y)
                        direction = 1;
                }

                //找到非独立凹点最近的chord（竖向切割）
                float closestY = int.MaxValue * direction;
                Chord closestChore = null;
                if (direction < 0)
                {
                    var rights = (from a in toRight
                                  where (a.startP.point.x - v.point.x) * (a.endP.point.x - v.point.x) <= 0
                                  && a.startP.point.y < v.point.y
                                  select a).ToList();

                    rights.Sort((a, b) => b.startP.point.y.CompareTo(a.startP.point.y));// if (rights != null)
                    closestChore = rights[0];
                    //if closestChore.startP.point.y > closestY
                    closestY = closestChore.startP.point.y;
                }
                else
                {
                    var lefts = (from a in toLeft
                                 where (a.startP.point.x - v.point.x) * (a.endP.point.x - v.point.x) <= 0
                                 && a.startP.point.y > v.point.y
                                 select a).ToList();

                    lefts.Sort((a, b) => a.startP.point.y.CompareTo(b.startP.point.y)); // if (lefts != null)
                    closestChore = lefts[0];
                    //if closestChore.startP.point.y < closestY
                    closestY = closestChore.startP.point.y;
                }
                //形成新的切割点
                Vertice splitA = new Vertice(new Point2D(v.point.x, closestY), Vertices.Count(), false);
                Vertices.Add(splitA);
                Vertice splitB = new Vertice(new Point2D(v.point.x, closestY), Vertices.Count(), false);
                Vertices.Add(splitB);

                v.concave = false;

                splitA.previous = closestChore.startP;
                closestChore.startP.next = splitA;
                splitB.next = closestChore.endP;
                closestChore.endP.previous = splitB;

                List<Chord> chords = null;
                if (direction < 0)
                    chords = toRight;
                else
                    chords = toLeft;
                chords.Remove(closestChore);
                chords.Add(new Chord(closestChore.startP.point.x, splitA.point.x, 0, closestChore.startP, splitA, true));
                chords.Add(new Chord(splitB.point.x, closestChore.endP.point.x, 0, splitB, closestChore.endP, true));

                if (v.previous.point.x == v.point.x)
                {
                    // Case 1
                    //        |
                    //        V
                    //    <---*
                    //        +
                    //        +
                    //        +
                    //    ---A/B---
                    splitA.next = v;
                    splitB.previous = v.previous;
                }
                else
                {
                    // Case 2
                    //        ^ 
                    //        |
                    //        *<---
                    //        +
                    //        +
                    //        +
                    //    ---A/B---
                    splitA.next = v.next;
                    splitB.previous = v;
                }
                splitA.next.previous = splitA;
                splitB.previous.next = splitB;
            }
        }

        private List<float[]> findRegions()
        {
            List<float[]> regions = new List<float[]>();
            for (int i = 0; i < Vertices.Count; i++)
            {
                if (Vertices[i].visited)
                    continue;
                float lowerleftX = float.MaxValue;
                float lowerleftY = float.MaxValue;
                float uprightX = -float.MaxValue;
                float uprightY = -float.MaxValue;
                while (!Vertices[i].visited)
                {
                    lowerleftX = Math.Min(Vertices[i].point.x, lowerleftX);
                    lowerleftY = Math.Min(Vertices[i].point.y, lowerleftY);
                    uprightX = Math.Max(Vertices[i].point.x, uprightX);
                    uprightY = Math.Max(Vertices[i].point.y, uprightY);
                    Vertices[i].visited = true;
                    Vertices[i] = Vertices[i].next;
                }
                regions.Add(new float[4] { lowerleftX, lowerleftY, uprightX, uprightY });
            }
            return regions;
        }

        private void SwapPoint(float[] point)
        {
            var tempx = point[0];
            var tempy = point[1];
            point[0] = point[2];
            point[1] = point[3];
            point[2] = tempx;
            point[3] = tempy;
        }
    }
}
