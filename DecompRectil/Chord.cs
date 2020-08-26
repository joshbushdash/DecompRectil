using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecompRectil
{

    class Chord
    {
        public float start;
        public float end;
        public float location;
        public Vertice startP;
        public Vertice endP;
        public bool m_DirectionIsX;
        public Chord(float start, float end, float location, Vertice startP, Vertice endP, bool m_DirectionIsX)
        {
            this.location = location;
            this.m_DirectionIsX = m_DirectionIsX;
            if (start < end)
            {
                this.start = start;
                this.end = end;
                this.startP = startP;
                this.endP = endP;
            }
            else
            {
                this.start = end;
                this.end = start;
                this.startP = endP;
                this.endP = startP;
            }
        }
        //检验凹点连线是否穿过边界
        public bool CheckChord(List<Vertice> vertices)
        {
            for (int p = vertices.Count - 1, q = 0; q < vertices.Count - 1; p = q++)
            {
                if (m_DirectionIsX)
                {
                    //共边情况，两凹点间不能有第三个凹点
                    if (vertices[p].concave
                        && vertices[p].point.y == location
                        && (vertices[p].point.x - start) * (vertices[p].point.x - end) < 0)
                        return false;
                    //垂直情况，凹点连线不能穿过边界
                    if (vertices[p].point.x == vertices[q].point.x
                        && (location - vertices[p].point.y) * (location - vertices[q].point.y) < 0
                        && (vertices[p].point.x - start) * (vertices[p].point.x - end) < 0)
                        return false;
                }
                else
                {
                    //共边情况，两凹点间不能有第三个凹点
                    if (vertices[p].concave
                        && vertices[p].point.x == location
                        && (vertices[p].point.y - start) * (vertices[p].point.y - end) < 0)
                        return false;
                    //垂直情况，凹点连线不能穿过边界
                    if (vertices[p].point.y == vertices[q].point.y
                        && (location - vertices[p].point.x) * (location - vertices[q].point.x) < 0
                        && (vertices[p].point.y - start) * (vertices[p].point.y - end) < 0)
                        return false;
                }
            }
            return true;
        }
    }
}
