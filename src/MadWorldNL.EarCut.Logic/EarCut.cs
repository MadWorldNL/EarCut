namespace MadWorldNL.EarCut.Logic;

public static class EarCut
{
    public static List<int> Calculate(double[] data) {
        return Calculate(data, null, 2);
    }
    
    public static List<int> Calculate(double[] data, int[] holeIndices, int dim) {

        bool hasHoles = holeIndices != null && holeIndices.Length > 0;
        int outerLen = hasHoles ? holeIndices[0] * dim : data.Length;

        Node outerNode = LinkedList(data, 0, outerLen, dim, true);

        List<int> triangles = [];

        if (outerNode == null || outerNode.next == outerNode.prev)
            return triangles;

        double minX = 0;
        double minY = 0;
        double maxX = 0;
        double maxY = 0;
        double invSize = Double.MinValue;

        if (hasHoles)
            outerNode = EliminateHoles(data, holeIndices, outerNode, dim);

        // if the shape is not too simple, we'll use z-order curve hash later;
        // calculate polygon bbox
        if (data.Length > 80 * dim) {
            minX = maxX = data[0];
            minY = maxY = data[1];

            for (int i = dim; i < outerLen; i += dim) {
                double x = data[i];
                double y = data[i + 1];
                if (x < minX)
                    minX = x;
                if (y < minY)
                    minY = y;
                if (x > maxX)
                    maxX = x;
                if (y > maxY)
                    maxY = y;
            }

            // minX, minY and size are later used to transform coords into
            // integers for z-order calculation
            invSize = Math.Max(maxX - minX, maxY - minY);
            invSize = invSize != 0.0 ? 1.0 / invSize : 0.0;
        }

        EarcutLinked(outerNode, triangles, dim, minX, minY, invSize, int.MinValue);

        return triangles;
    }
    
    private static void EarcutLinked(Node ear, List<int> triangles, int dim, double minX, double minY, double invSize, int pass) {
        if (ear == null)
            return;

        // interlink polygon nodes in z-order
        if (pass == int.MinValue && invSize != double.MinValue)
            IndexCurve(ear, minX, minY, invSize);

        Node stop = ear;

        // iterate through ears, slicing them one by one
        while (ear.prev != ear.next) {
            Node prev = ear.prev;
            Node next = ear.next;

            if (invSize != Double.MinValue ? IsEarHashed(ear, minX, minY, invSize) : IsEar(ear)) {
                // cut off the triangle
                triangles.Add(prev.i / dim);
                triangles.Add(ear.i / dim);
                triangles.Add(next.i / dim);

                RemoveNode(ear);

                // skipping the next vertice leads to less sliver triangles
                ear = next.next;
                stop = next.next;

                continue;
            }

            ear = next;

            // if we looped through the whole remaining polygon and can't find
            // any more ears
            if (ear == stop) {
                // try filtering points and slicing again
                if (pass == int.MinValue) {
                    EarcutLinked(FilterPoints(ear, null), triangles, dim, minX, minY, invSize, 1);

                    // if this didn't work, try curing all small
                    // self-intersections locally
                } else if (pass == 1) {
                    ear = CureLocalIntersections(FilterPoints(ear, null), triangles, dim);
                    EarcutLinked(ear, triangles, dim, minX, minY, invSize, 2);

                    // as a last resort, try splitting the remaining polygon
                    // into two
                } else if (pass == 2) {
                    SplitEarcut(ear, triangles, dim, minX, minY, invSize);
                }

                break;
            }
        }
    }
    
    private static void SplitEarcut(Node start, List<int> triangles, int dim, double minX, double minY, double size) {
        // look for a valid diagonal that divides the polygon into two
        Node a = start;
        do {
            Node b = a.next.next;
            while (b != a.prev) {
                if (a.i != b.i && IsValidDiagonal(a, b)) {
                    // split the polygon in two by the diagonal
                    Node c = SplitPolygon(a, b);

                    // filter colinear points around the cuts
                    a = FilterPoints(a, a.next);
                    c = FilterPoints(c, c.next);

                    // run earcut on each half
                    EarcutLinked(a, triangles, dim, minX, minY, size, int.MinValue);
                    EarcutLinked(c, triangles, dim, minX, minY, size, int.MinValue);
                    return;
                }
                b = b.next;
            }
            a = a.next;
        } while (a != start);
    }
    
    private static bool IsValidDiagonal(Node a, Node b) {
        //return a.next.i != b.i && a.prev.i != b.i && !intersectsPolygon(a, b) && locallyInside(a, b) && locallyInside(b, a) && middleInside(a, b);

        return a.next.i != b.i && a.prev.i != b.i && !IntersectsPolygon(a, b) && // dones't intersect other edges
               (LocallyInside(a, b) && LocallyInside(b, a) && MiddleInside(a, b) && // locally visible
                (Area(a.prev, a, b.prev) != 0 || Area(a, b.prev, b) != 0) || // does not create opposite-facing sectors
                Equals(a, b) && Area(a.prev, a, a.next) > 0 && Area(b.prev, b, b.next) > 0); // special zero-length case
    }
    
    private static bool MiddleInside(Node a, Node b) {
        Node p = a;
        bool inside = false;
        double px = (a.x + b.x) / 2;
        double py = (a.y + b.y) / 2;
        do {
            if (((p.y > py) != (p.next.y > py)) && (px < (p.next.x - p.x) * (py - p.y) / (p.next.y - p.y) + p.x))
                inside = !inside;
            p = p.next;
        } while (p != a);

        return inside;
    }
    
    private static bool IntersectsPolygon(Node a, Node b) {
        Node p = a;
        do {
            if (p.i != a.i && p.next.i != a.i && p.i != b.i && p.next.i != b.i && Intersects(p, p.next, a, b))
                return true;
            p = p.next;
        } while (p != a);

        return false;
    }
    
    private static bool Intersects(Node p1, Node q1, Node p2, Node q2) {
        if ((Equals(p1, p2) && Equals(q1, q2)) || (Equals(p1, q2) && Equals(p2, q1)))
            return true;
        double o1 = Sign(Area(p1, q1, p2));
        double o2 = Sign(Area(p1, q1, q2));
        double o3 = Sign(Area(p2, q2, p1));
        double o4 = Sign(Area(p2, q2, q1));

        if (o1 != o2 && o3 != o4)
            return true; // general case

        if (o1 == 0 && onSegment(p1, p2, q1))
            return true; // p1, q1 and p2 are collinear and p2 lies on p1q1
        if (o2 == 0 && onSegment(p1, q2, q1))
            return true; // p1, q1 and q2 are collinear and q2 lies on p1q1
        if (o3 == 0 && onSegment(p2, p1, q2))
            return true; // p2, q2 and p1 are collinear and p1 lies on p2q2
        if (o4 == 0 && onSegment(p2, q1, q2))
            return true; // p2, q2 and q1 are collinear and q1 lies on p2q2

        return false;
    }
    
    // for collinear points p, q, r, check if point q lies on segment pr
    private static bool onSegment(Node p, Node q, Node r) {
        return q.x <= Math.Max(p.x, r.x) && q.x >= Math.Min(p.x, r.x) && q.y <= Math.Max(p.y, r.y) && q.y >= Math.Min(p.y, r.y);
    }
    
    private static double Sign(double num) {
        return num > 0 ? 1 : num < 0 ? -1 : 0;
    }
    
    private static Node CureLocalIntersections(Node start, List<int> triangles, int dim) {
        Node p = start;
        do {
            Node a = p.prev, b = p.next.next;

            if (!Equals(a, b) && Intersects(a, p, p.next, b) && LocallyInside(a, b) && LocallyInside(b, a)) {

                triangles.Add(a.i / dim);
                triangles.Add(p.i / dim);
                triangles.Add(b.i / dim);

                // remove two nodes involved
                RemoveNode(p);
                RemoveNode(p.next);

                p = start = b;
            }
            p = p.next;
        } while (p != start);

        return FilterPoints(p, null);
    }
    
    private static bool IsEar(Node ear) {
        Node a = ear.prev, b = ear, c = ear.next;

        if (Area(a, b, c) >= 0)
            return false; // reflex, can't be an ear

        // now make sure we don't have other points inside the potential ear
        Node p = ear.next.next;

        while (p != ear.prev) {
            if (PointInTriangle(a.x, a.y, b.x, b.y, c.x, c.y, p.x, p.y) && Area(p.prev, p, p.next) >= 0)
                return false;
            p = p.next;
        }

        return true;
    }
    
    private static bool IsEarHashed(Node ear, double minX, double minY, double invSize) {
        Node a = ear.prev;
        Node b = ear;
        Node c = ear.next;

        if (Area(a, b, c) >= 0)
            return false; // reflex, can't be an ear

        // triangle bbox; min & max are calculated like this for speed
        double minTX = a.x < b.x ? (a.x < c.x ? a.x : c.x) : (b.x < c.x ? b.x : c.x), minTY = a.y < b.y ? (a.y < c.y ? a.y : c.y) : (b.y < c.y ? b.y : c.y),
                maxTX = a.x > b.x ? (a.x > c.x ? a.x : c.x) : (b.x > c.x ? b.x : c.x), maxTY = a.y > b.y ? (a.y > c.y ? a.y : c.y) : (b.y > c.y ? b.y : c.y);

        // z-order range for the current triangle bbox;
        double minZ = ZOrder(minTX, minTY, minX, minY, invSize);
        double maxZ = ZOrder(maxTX, maxTY, minX, minY, invSize);

        // first look for points inside the triangle in increasing z-order
        Node p = ear.prevZ;
        Node n = ear.nextZ;

        while (p != null && p.z >= minZ && n != null && n.z <= maxZ) {
            if (p != ear.prev && p != ear.next && PointInTriangle(a.x, a.y, b.x, b.y, c.x, c.y, p.x, p.y) && Area(p.prev, p, p.next) >= 0)
                return false;
            p = p.prevZ;

            if (n != ear.prev && n != ear.next && PointInTriangle(a.x, a.y, b.x, b.y, c.x, c.y, n.x, n.y) && Area(n.prev, n, n.next) >= 0)
                return false;
            n = n.nextZ;
        }

        // look for remaining points in decreasing z-order
        while (p != null && p.z >= minZ) {
            if (p != ear.prev && p != ear.next && PointInTriangle(a.x, a.y, b.x, b.y, c.x, c.y, p.x, p.y) && Area(p.prev, p, p.next) >= 0)
                return false;
            p = p.prevZ;
        }

        // look for remaining points in increasing z-order
        while (n != null && n.z <= maxZ) {
            if (n != ear.prev && n != ear.next && PointInTriangle(a.x, a.y, b.x, b.y, c.x, c.y, n.x, n.y) && Area(n.prev, n, n.next) >= 0)
                return false;
            n = n.nextZ;
        }

        return true;
    }
    
    // z-order of a point given coords and inverse of the longer side of data bbox
    private static double ZOrder(double x, double y, double minX, double minY, double invSize) {
        // coords are transformed into non-negative 15-bit integer range
        int lx = (int)(32767 * (x - minX) * invSize);
        int ly = (int)(32767 * (y - minY) * invSize);

        lx = (lx | (lx << 8)) & 0x00FF00FF;
        lx = (lx | (lx << 4)) & 0x0F0F0F0F;
        lx = (lx | (lx << 2)) & 0x33333333;
        lx = (lx | (lx << 1)) & 0x55555555;

        ly = (ly | (ly << 8)) & 0x00FF00FF;
        ly = (ly | (ly << 4)) & 0x0F0F0F0F;
        ly = (ly | (ly << 2)) & 0x33333333;
        ly = (ly | (ly << 1)) & 0x55555555;

        return lx | (ly << 1);
    }
    
    private static void IndexCurve(Node start, double minX, double minY, double invSize) {
        Node p = start;
        do {
            if (p.z == Double.MinValue)
                p.z = ZOrder(p.x, p.y, minX, minY, invSize);
            p.prevZ = p.prev;
            p.nextZ = p.next;
            p = p.next;
        } while (p != start);

        p.prevZ.nextZ = null;
        p.prevZ = null;

        SortLinked(p);
    }
    
    private static Node SortLinked(Node list) {
        int inSize = 1;
        

        int numMerges;
        do {
            Node p = list;
            list = null;
            Node tail = null;
            numMerges = 0;

            while (p != null) {
                numMerges++;
                Node q = p;
                int pSize = 0;
                for (int i = 0; i < inSize; i++) {
                    pSize++;
                    q = q.nextZ;
                    if (q == null)
                        break;
                }

                int qSize = inSize;

                while (pSize > 0 || (qSize > 0 && q != null)) {
                    Node e;
                    if (pSize == 0) {
                        e = q;
                        q = q.nextZ;
                        qSize--;
                    } else if (qSize == 0 || q == null) {
                        e = p;
                        p = p.nextZ;
                        pSize--;
                    } else if (p.z <= q.z) {
                        e = p;
                        p = p.nextZ;
                        pSize--;
                    } else {
                        e = q;
                        q = q.nextZ;
                        qSize--;
                    }

                    if (tail != null)
                        tail.nextZ = e;
                    else
                        list = e;

                    e.prevZ = tail;
                    tail = e;
                }

                p = q;
            }

            tail.nextZ = null;
            inSize *= 2;

        } while (numMerges > 1);

        return list;
    }
    
    private static Node EliminateHoles(double[] data, int[] holeIndices, Node outerNode, int dim)
    {
        var queue = new List<Node>();

        int len = holeIndices.Length;
        for (int i = 0; i < len; i++) {
            int start = holeIndices[i] * dim;
            int end = i < len - 1 ? holeIndices[i + 1] * dim : data.Length;
            Node list = LinkedList(data, start, end, dim, false);
            if (list == list.next)
                list.steiner = true;
            queue.Add(GetLeftmost(list));
        }

        queue.Sort((o1, o2) =>
        {
            if (o1.x - o2.x > 0)
            {
                return 1;
            }

            if (o1.x - o2.x < 0)
            {
                return -2;
            }

            return 0;
        });

        foreach (var node in queue)
        {
            EliminateHole(node, outerNode);
            outerNode = FilterPoints(outerNode, outerNode.next);
        }

        return outerNode;
    }
    
    private static Node FilterPoints(Node start, Node end) {
        if (start == null)
            return start;
        if (end == null)
            end = start;

        Node p = start;
        bool again;

        do {
            again = false;

            if (!p.steiner && Equals(p, p.next) || Area(p.prev, p, p.next) == 0) {
                RemoveNode(p);
                p = end = p.prev;
                if (p == p.next)
                    break;
                again = true;
            } else {
                p = p.next;
            }
        } while (again || p != end);

        return end;
    }
    
    private static bool Equals(Node p1, Node p2) {
        return p1.x == p2.x && p1.y == p2.y;
    }
    
    private static double Area(Node p, Node q, Node r) {
        return (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);
    }
    
    private static void EliminateHole(Node hole, Node outerNode) {
        outerNode = FindHoleBridge(hole, outerNode);
        if (outerNode != null) {
            Node b = SplitPolygon(outerNode, hole);
            
            // filter collinear points around the cuts
            FilterPoints(outerNode, outerNode.next);
            FilterPoints(b, b.next);
        }
    }
    
    private static Node SplitPolygon(Node a, Node b) {
        Node a2 = new Node(a.i, a.x, a.y);
        Node b2 = new Node(b.i, b.x, b.y);
        Node an = a.next;
        Node bp = b.prev;

        a.next = b;
        b.prev = a;

        a2.next = an;
        an.prev = a2;

        b2.next = a2;
        a2.prev = b2;

        bp.next = b2;
        b2.prev = bp;

        return b2;
    }
    
    // David Eberly's algorithm for finding a bridge between hole and outer
    // polygon
    private static Node FindHoleBridge(Node hole, Node outerNode) {
        Node p = outerNode;
        double hx = hole.x;
        double hy = hole.y;
        double qx = -Double.MaxValue;
        Node m = null;

        // find a segment intersected by a ray from the hole's leftmost point to
        // the left;
        // segment's endpoint with lesser x will be potential connection point
        do {
            if (hy <= p.y && hy >= p.next.y) {
                double x = p.x + (hy - p.y) * (p.next.x - p.x) / (p.next.y - p.y);
                if (x <= hx && x > qx) {
                    qx = x;
                    if (x == hx) {
                        if (hy == p.y)
                            return p;
                        if (hy == p.next.y)
                            return p.next;
                    }
                    m = p.x < p.next.x ? p : p.next;
                }
            }
            p = p.next;
        } while (p != outerNode);

        if (m == null)
            return null;

        if (hx == qx)
            return m; // hole touches outer segment; pick leftmost endpoint

        // look for points inside the triangle of hole point, segment
        // intersection and endpoint;
        // if there are no points found, we have a valid connection;
        // otherwise choose the point of the minimum angle with the ray as
        // connection point

        Node stop = m;
        double mx = m.x;
        double my = m.y;
        double tanMin = Double.MaxValue;
        double tan;

        p = m;

        do {
            if (hx >= p.x && p.x >= mx && PointInTriangle(hy < my ? hx : qx, hy, mx, my, hy < my ? qx : hx, hy, p.x, p.y)) {

                tan = Math.Abs(hy - p.y) / (hx - p.x); // tangential

                if (LocallyInside(p, hole) && (tan < tanMin || (tan == tanMin && (p.x > m.x || (p.x == m.x && SectorContainsSector(m, p)))))) {
                    m = p;
                    tanMin = tan;
                }
            }

            p = p.next;
        } while (p != stop);

        return m;
    }
    
    private static bool LocallyInside(Node a, Node b) {
        return Area(a.prev, a, a.next) < 0 ? Area(a, b, a.next) >= 0 && Area(a, a.prev, b) >= 0 : Area(a, b, a.prev) < 0 || Area(a, a.next, b) < 0;
    }
    
    // whether sector in vertex m contains sector in vertex p in the same
    // coordinates
    private static bool SectorContainsSector(Node m, Node p) {
        return Area(m.prev, m, p.prev) < 0 && Area(p.next, m, m.next) < 0;
    }
    
    private static bool PointInTriangle(double ax, double ay, double bx, double by, double cx, double cy, double px, double py) {
        return (cx - px) * (ay - py) - (ax - px) * (cy - py) >= 0 && (ax - px) * (by - py) - (bx - px) * (ay - py) >= 0
                                                                  && (bx - px) * (cy - py) - (cx - px) * (by - py) >= 0;
    }
    
    private static Node GetLeftmost(Node start) {
        Node p = start;
        Node leftmost = start;
        do {
            if (p.x < leftmost.x || (p.x == leftmost.x && p.y < leftmost.y))
                leftmost = p;
            p = p.next;
        } while (p != start);
        return leftmost;
    }
    
    private static Node LinkedList(double[] data, int start, int end, int dim, bool clockwise) {
        Node last = null;
        if (clockwise == (SignedArea(data, start, end, dim) > 0)) {
            for (int i = start; i < end; i += dim) {
                last = InsertNode(i, data[i], data[i + 1], last);
            }
        } else {
            for (int i = (end - dim); i >= start; i -= dim) {
                last = InsertNode(i, data[i], data[i + 1], last);
            }
        }

        if (last != null && Equals(last, last.next)) {
            RemoveNode(last);
            last = last.next;
        }
        return last;
    }
    
    private static void RemoveNode(Node p) {
        p.next.prev = p.prev;
        p.prev.next = p.next;

        if (p.prevZ != null) {
            p.prevZ.nextZ = p.nextZ;
        }
        if (p.nextZ != null) {
            p.nextZ.prevZ = p.prevZ;
        }
    }
    
    private static Node InsertNode(int i, double x, double y, Node last) {
        Node p = new Node(i, x, y);

        if (last == null) {
            p.prev = p;
            p.next = p;
        } else {
            p.next = last.next;
            p.prev = last;
            last.next.prev = p;
            last.next = p;
        }
        return p;
    }
    
    private static double SignedArea(double[] data, int start, int end, int dim) {
        double sum = 0;
        var j = end - dim;
        for (var i = start; i < end; i += dim) {
            sum += (data[j] - data[i]) * (data[i + 1] + data[j + 1]);
            j = i;
        }
        return sum;
    }
}