using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ScreenShotTool
{
    public class RectPointCollection : IEnumerable<RectPoint>
    {
        private RectPoint[] rectPoints;

        public RectPointCollection()
        {
            rectPoints = new RectPoint[8]
            {
                new RectPoint(RectPointLocation.LT),
                new RectPoint(RectPointLocation.T),
                new RectPoint(RectPointLocation.RT),
                new RectPoint(RectPointLocation.L),
                new RectPoint(RectPointLocation.R),
                new RectPoint(RectPointLocation.LB),
                new RectPoint(RectPointLocation.B),
                new RectPoint(RectPointLocation.RB)
            };
        }

        public RectPointCollection SetPointFill(Brush fill)
        {
            foreach(RectPoint point in rectPoints)
            {
                point.pointFill = fill;
            }
            return this;
        }

        public RectPointCollection SetPointThickness(double thickness)
        {
            foreach (RectPoint point in rectPoints)
            {
                point.pointThickness = thickness;
            }
            return this;
        }

        public RectPointCollection SetPointWidthAndHeight(double value)
        {
            foreach (RectPoint point in rectPoints)
            {
                point.pointHeight = value;
                point.pointWidth = value;
            }
            return this;
        }

        public RectPointCollection SetPointMargin(double margin)
        {
            foreach (RectPoint point in rectPoints)
            {
                point.pointMargin = margin;
            }
            return this;
        }

        public void UpdatePointPosition(double width, double height)
        {
            foreach(RectPoint rectPoint in rectPoints)
            {
                rectPoint.UpdatePosition(width, height);
            }
        }

        public RectPoint FindRectbyLocation(RectPointLocation location)
        {
            return rectPoints.Where(point => point.pointLocation == location).FirstOrDefault();
        }

        public RectPointLocation? FindLocationbyRect(Rectangle rectangle)
        {
            return rectPoints.Where(point => point.Point == rectangle).FirstOrDefault()?.pointLocation;
        }

        /// <summary>
        /// 轴对称翻转点
        /// </summary>
        public void OverTurnPoint(AxleType axle)
        {
            //点按照Y轴对称翻转
            //LT <=> RT
            //L <=> R 
            //LB <=> RB
            if(axle == AxleType.Y)
            {
                RectPoint LTPoint = FindRectbyLocation(RectPointLocation.LT);
                RectPoint RTPoint = FindRectbyLocation(RectPointLocation.RT);
                RectPoint LPoint = FindRectbyLocation(RectPointLocation.L);
                RectPoint RPoint = FindRectbyLocation(RectPointLocation.R);
                RectPoint LBPoint = FindRectbyLocation(RectPointLocation.LB);
                RectPoint RBPoint = FindRectbyLocation(RectPointLocation.RB);

                LTPoint.pointLocation = RectPointLocation.RT;
                RTPoint.pointLocation = RectPointLocation.LT;
                LPoint.pointLocation = RectPointLocation.R;
                RPoint.pointLocation = RectPointLocation.L;
                LBPoint.pointLocation = RectPointLocation.RB;
                RBPoint.pointLocation = RectPointLocation.LB;
            }
            //LT <=> LB
            //T <=> B 
            //RB <=> RT
            else
            {
                RectPoint LTPoint = FindRectbyLocation(RectPointLocation.LT);
                RectPoint LBPoint = FindRectbyLocation(RectPointLocation.LB);
                RectPoint TPoint = FindRectbyLocation(RectPointLocation.T);
                RectPoint BPoint = FindRectbyLocation(RectPointLocation.B);
                RectPoint RTPoint = FindRectbyLocation(RectPointLocation.RT);
                RectPoint RBPoint = FindRectbyLocation(RectPointLocation.RB);

                LTPoint.pointLocation = RectPointLocation.LB;
                LBPoint.pointLocation = RectPointLocation.LT;
                TPoint.pointLocation = RectPointLocation.B;
                BPoint.pointLocation = RectPointLocation.T;
                RTPoint.pointLocation = RectPointLocation.RB;
                RBPoint.pointLocation = RectPointLocation.RT;
            }
        }

        public IEnumerator<RectPoint> GetEnumerator()
        {
            foreach(var point in rectPoints)
            {
                yield return point;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    public enum AxleType
    {
        /// <summary>
        /// X轴
        /// </summary>
        X = 0,
        /// <summary>
        /// Y轴
        /// </summary>
        Y = 1
    }
}
