using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using ZedGraph;

namespace DirectPointEdit
{
    public partial class ExampleGraph : Form
    {
        private GraphPane _myPane;
        private PointPairList _points;
        private LineItem _myCurve;

        private bool mouseDown = false;
        private int numPoints = 10;
        private double _sinPart;
        private double _timePart;
        private PointPair lastMousePoint;

        public ExampleGraph()
        {
            InitializeComponent();

            // get arrow resourse
            var _assembly = Assembly.GetExecutingAssembly();
            var _imageStream = _assembly.GetManifestResourceStream("DirectPointEdit.DownArrow.png");
            down.BackgroundImage = new Bitmap(_imageStream);
            up.BackgroundImage = new Bitmap(_imageStream);
            up.BackgroundImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
            right.BackgroundImage = new Bitmap(_imageStream);
            right.BackgroundImage.RotateFlip(RotateFlipType.Rotate270FlipNone);
            left.BackgroundImage = new Bitmap(_imageStream);
            left.BackgroundImage.RotateFlip(RotateFlipType.Rotate90FlipNone);

            _timePart = 1.0 / numPoints;
            _sinPart = 2.0 * Math.PI / numPoints;

            _myPane = zedGraphControl1.GraphPane;
            _myPane.Legend.IsVisible = false;
            _myPane.Title.Text = "Moveable Points";
            _myPane.XAxis.Title.Text = "X";
            _myPane.YAxis.Title.Text = "Y";
            
            _myPane.YAxis.Scale.Min = -1.2;
            _myPane.YAxis.Scale.Max = 1.2;
            _myPane.YAxis.Scale.MajorStep = 1;
            _myPane.YAxis.Scale.MinorStep = 1;

            _myPane.XAxis.Scale.Min = -0.2;
            _myPane.XAxis.Scale.Max = 1.2;
            _myPane.XAxis.Scale.MajorStep = 1;
            _myPane.XAxis.Scale.MinorStep = 1;

            _points = new PointPairList();
            _myCurve = _myPane.AddCurve("My Points", _points, Color.Blue, SymbolType.Circle);
            _myCurve.Symbol.Fill = new Fill(Color.Blue, Color.Red);
            _myCurve.Symbol.Fill.Type = FillType.GradientByZ;
            _myCurve.Symbol.Fill.SecondaryValueGradientColor = Color.Empty;
            _myCurve.Symbol.Size = 15;
            _myCurve.Symbol.Fill.IsVisible = true;
            _myCurve.Symbol.Border.IsVisible = false;

            for (int i = 0; i <= numPoints; i++)
            {
                _points.Add(i * _timePart, Math.Sin(i * _sinPart), 0);
            }

            curveIndex.Maximum = numPoints;
            curveIndex.Value = 2;
            HighlightIndex(2);
        }

        private void RedrawGraph()
        {
            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
        }

        private void HighlightIndex(int index)
        {
            // clear any current highlighting
            _points.Where(point => point.Z == 1).ToList().ForEach(point => point.Z = 0);

            // highlight new point
            _points[index].Z = 1;

            // redraw the graph
            RedrawGraph();
        }

        private void curveIndex_ValueChanged(object sender, EventArgs e)
        {
            // highlight the index
            HighlightIndex((int)curveIndex.Value);
        }
        
        private bool ZedGraphControl1_MouseDownEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            // find the closest point to the click location
            _myPane.FindNearestObject(new PointF(e.X, e.Y), this.CreateGraphics(), out object nearestobject, out int index);

            // highlight the index
            HighlightIndex(index);

            // update the point value boxes
            curveIndex.Value = index;

            // get current mouse position
            lastMousePoint = GetMouseCoords();

            // flag that we have the mouse down
            mouseDown = true;

            // return complete
            return true;
        }

        private bool zedGraphControl1_MouseUpEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            // flag that the mouse is up
            mouseDown = false;

            // perform final move
            MovePointByMouse();
            return true;
        }

        private void zedGraphControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
                MovePointByMouse();
        }

        private PointPair GetMouseCoords()
        {
            _myPane.ReverseTransform(Cursor.Position, out double x, out double y);
            return new PointPair(x, y);
        }

        private void MovePointByMouse()
        {
            var currentMousePoint = GetMouseCoords();

            // get the delta between last mouse move and now
            var deltaPoint = new PointPair(currentMousePoint.X - lastMousePoint.X, currentMousePoint.Y - lastMousePoint.Y);

            // apply the point to the selected point
            _points[(int)curveIndex.Value].X += deltaPoint.X;
            _points[(int)curveIndex.Value].Y += deltaPoint.Y;

            // update last mouse point
            lastMousePoint = currentMousePoint;

            // redraw the graph
            RedrawGraph();
        }

        private void up_Click(object sender, EventArgs e)
        {
            _points[(int)curveIndex.Value].Y += (double)step.Value;

            // redraw the graph
            RedrawGraph();
        }

        private void right_Click(object sender, EventArgs e)
        {
            _points[(int)curveIndex.Value].X += (double)step.Value;

            // redraw the graph
            RedrawGraph();
        }

        private void down_Click(object sender, EventArgs e)
        {
            _points[(int)curveIndex.Value].Y -= (double)step.Value;

            // redraw the graph
            RedrawGraph();
        }

        private void left_Click(object sender, EventArgs e)
        {
            _points[(int)curveIndex.Value].X -= (double)step.Value;

            // redraw the graph
            RedrawGraph();
        }
    }
}
