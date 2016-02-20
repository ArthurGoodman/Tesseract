using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Tesseract {
    public partial class Form : System.Windows.Forms.Form {
        private float scaleFactor = 0.0f;
        private float offsetX = 0, offsetY = 0;
        private float alphaX1 = 0, alphaY1 = 0, alphaX2 = 0, alphaY2 = 0;

        private Point lastPos;

        Vector4[] vertices;
        Edge[] edges;

        Matrix4x4 perspective;

        public Form() {
            InitializeComponent();

            MouseWheel += new MouseEventHandler(MouseWheelEvent);

            ResizeRedraw = true;
            DoubleBuffered = true;

            Application.Idle += new EventHandler(Repaint);

            Width = Screen.PrimaryScreen.WorkingArea.Width / 2;
            Height = (int)(Screen.PrimaryScreen.WorkingArea.Height / 1.75);

            CenterToScreen();

            Reset();

            CreateTesseract();
            CreatePerspective();
        }

        private void CreateTesseract() {
            vertices = new Vector4[16];

            vertices[0] = new Vector4(new double[4] { -1, -1, -1, -1 });
            vertices[1] = new Vector4(new double[4] { 1, -1, -1, -1 });
            vertices[2] = new Vector4(new double[4] { -1, 1, -1, -1 });
            vertices[3] = new Vector4(new double[4] { 1, 1, -1, -1 });

            vertices[4] = new Vector4(new double[4] { -1, -1, 1, -1 });
            vertices[5] = new Vector4(new double[4] { 1, -1, 1, -1 });
            vertices[6] = new Vector4(new double[4] { -1, 1, 1, -1 });
            vertices[7] = new Vector4(new double[4] { 1, 1, 1, -1 });

            vertices[8] = new Vector4(new double[4] { -1, -1, -1, 1 });
            vertices[9] = new Vector4(new double[4] { 1, -1, -1, 1 });
            vertices[10] = new Vector4(new double[4] { -1, 1, -1, 1 });
            vertices[11] = new Vector4(new double[4] { 1, 1, -1, 1 });

            vertices[12] = new Vector4(new double[4] { -1, -1, 1, 1 });
            vertices[13] = new Vector4(new double[4] { 1, -1, 1, 1 });
            vertices[14] = new Vector4(new double[4] { -1, 1, 1, 1 });
            vertices[15] = new Vector4(new double[4] { 1, 1, 1, 1 });

            edges = new Edge[32];

            edges[0] = new Edge(vertices[0], vertices[1]);
            edges[1] = new Edge(vertices[1], vertices[3]);
            edges[2] = new Edge(vertices[3], vertices[2]);
            edges[3] = new Edge(vertices[2], vertices[0]);

            edges[4] = new Edge(vertices[4], vertices[5]);
            edges[5] = new Edge(vertices[5], vertices[7]);
            edges[6] = new Edge(vertices[7], vertices[6]);
            edges[7] = new Edge(vertices[6], vertices[4]);

            edges[8] = new Edge(vertices[8], vertices[9]);
            edges[9] = new Edge(vertices[9], vertices[11]);
            edges[10] = new Edge(vertices[11], vertices[10]);
            edges[11] = new Edge(vertices[10], vertices[8]);

            edges[12] = new Edge(vertices[12], vertices[13]);
            edges[13] = new Edge(vertices[13], vertices[15]);
            edges[14] = new Edge(vertices[15], vertices[14]);
            edges[15] = new Edge(vertices[14], vertices[12]);

            edges[16] = new Edge(vertices[0], vertices[4]);
            edges[17] = new Edge(vertices[1], vertices[5]);
            edges[18] = new Edge(vertices[2], vertices[6]);
            edges[19] = new Edge(vertices[3], vertices[7]);

            edges[20] = new Edge(vertices[8], vertices[12]);
            edges[21] = new Edge(vertices[9], vertices[13]);
            edges[22] = new Edge(vertices[10], vertices[14]);
            edges[23] = new Edge(vertices[11], vertices[15]);

            edges[24] = new Edge(vertices[0], vertices[8]);
            edges[25] = new Edge(vertices[1], vertices[9]);
            edges[26] = new Edge(vertices[2], vertices[10]);
            edges[27] = new Edge(vertices[3], vertices[11]);
            edges[28] = new Edge(vertices[4], vertices[12]);
            edges[29] = new Edge(vertices[5], vertices[13]);
            edges[30] = new Edge(vertices[6], vertices[14]);
            edges[31] = new Edge(vertices[7], vertices[15]);
        }

        private void CreatePerspective() {
            double fov = Math.PI / scaleFactor * 7;

            double near = 100;
            double far = 500;

            double s = 1 / Math.Tan(fov / 2);

            perspective = new Matrix4x4(new double[4, 4] {
                { s, 0, 0,                           0                             },
                { 0, s, 0,                           0                             },
                { 0, 0, (far + near) / (far - near), 2 * near * far / (near - far) },
                { 0, 0, 1,                           0                             }
            });
        }

        private void MouseWheelEvent(object sender, MouseEventArgs e) {
            if (e.Delta > 0)
                scaleFactor *= 1.1f;
            else
                scaleFactor /= 1.1f;
        }

        private void Repaint(object sender, EventArgs e) {
            Invalidate();
        }

        private int MapX(float x) {
            return (int)(scaleFactor * (x + offsetX));
        }

        private int MapY(float y) {
            return (int)(scaleFactor * (-y + offsetY));
        }

        private void PaintEvent(object sender, PaintEventArgs e) {
            e.Graphics.FillRectangle(new SolidBrush(Color.LightGray), ClientRectangle);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            e.Graphics.TranslateTransform(ClientSize.Width / 2, ClientSize.Height / 2);

            SolidBrush brush = new SolidBrush(Color.Red);
            Pen pen = new Pen(Color.Black);

            Vector4 offset = new Vector4(new double[] { 0, 0, 7, 0 });

            foreach (Edge edge in edges) {
                Vector4 a = new Vector4(edge.A.Data);
                Vector4 b = new Vector4(edge.B.Data);

                a += offset;
                b += offset;

                a[3] = 1;
                perspective.Map(a);
                a /= a[3];

                b[3] = 1;
                perspective.Map(b);
                b /= b[3];

                int x1 = MapX((float)a[0]);
                int y1 = MapY((float)a[1]);

                int x2 = MapX((float)b[0]);
                int y2 = MapY((float)b[1]);

                e.Graphics.DrawLine(pen, x1, y1, x2, y2);
            }

            const int circleDiameter = 8;

            foreach (Vector4 v in vertices) {
                Vector4 m = new Vector4(v.Data);

                m += offset;

                m[3] = 1;
                perspective.Map(m);
                m /= m[3];

                int x = MapX((float)m[0]);
                int y = MapY((float)m[1]);

                brush.Color = Color.Black;
                e.Graphics.FillEllipse(brush, x - 0.5f - circleDiameter / 2, y - 0.5f - circleDiameter / 2, circleDiameter + 1, circleDiameter + 1);

                brush.Color = Color.Red;
                e.Graphics.FillEllipse(brush, x - circleDiameter / 2, y - circleDiameter / 2, circleDiameter, circleDiameter);
            }
        }

        private void Reset() {
            offsetX = 0;
            offsetX = 0;

            scaleFactor = 100.0f;

            alphaX1 = 0;
            alphaY1 = 0;

            alphaX2 = 0;
            alphaY2 = 0;
        }

        private bool IsFullscreen() {
            return WindowState == FormWindowState.Maximized;
        }

        private void ShowNormal() {
            FormBorderStyle = FormBorderStyle.Fixed3D;
            WindowState = FormWindowState.Normal;
        }

        private void ShowFullscreen() {
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
        }

        private void MouseDownEvent(object sender, MouseEventArgs e) {
            lastPos = e.Location;
        }

        private void MouseMoveEvent(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                offsetX += (e.Location.X - lastPos.X) / scaleFactor;
                offsetY += (e.Location.Y - lastPos.Y) / scaleFactor;
            } else if (e.Button == MouseButtons.Right) {
                if (ModifierKeys == Keys.Control) {
                    alphaX2 = (e.Location.X - lastPos.X) / scaleFactor / 4;
                    alphaY2 = (e.Location.Y - lastPos.Y) / scaleFactor / 4;
                } else {
                    alphaX1 = (e.Location.X - lastPos.X) / scaleFactor / 4;
                    alphaY1 = (e.Location.Y - lastPos.Y) / scaleFactor / 4;
                }

                Rotate();
            }

            lastPos = e.Location;
        }

        private void KeyDownEvent(object sender, KeyEventArgs e) {
            switch (e.KeyCode) {
                case Keys.Escape:
                    if (IsFullscreen())
                        ShowNormal();
                    else
                        Close();
                    break;

                case Keys.F11:
                    if (IsFullscreen())
                        ShowNormal();
                    else
                        ShowFullscreen();
                    break;

                case Keys.Back:
                    Reset();
                    break;
            }

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void Rotate() {
            Matrix4x4 xMatrix1 = new Matrix4x4(new double[4, 4] {
                { Math.Cos(alphaX1), 0, -Math.Sin(alphaX1), 0 },
                { 0,                 1, 0,                  0 },
                { Math.Sin(alphaX1), 0, Math.Cos(alphaX1),  0 },
                { 0,                 0, 0,                  1 }
            });

            Matrix4x4 yMatrix1 = new Matrix4x4(new double[4, 4] {
                { 1, 0,                 0,                  0 },
                { 0, Math.Cos(alphaY1), -Math.Sin(alphaY1), 0 },
                { 0, Math.Sin(alphaY1), Math.Cos(alphaY1),  0 },
                { 0, 0,                 0,                  1 }
            });

            Matrix4x4 xMatrix2 = new Matrix4x4(new double[4, 4] {
                { Math.Cos(alphaX2), 0, 0, -Math.Sin(alphaX2) },
                { 0,                 1, 0, 0                  },
                { 0,                 0, 1, 0                  },
                { Math.Sin(alphaX2), 0, 0, Math.Cos(alphaX2)  }
            });

            Matrix4x4 yMatrix2 = new Matrix4x4(new double[4, 4] {
                { 1, 0,                 0, 0                  },
                { 0, Math.Cos(alphaY2), 0, -Math.Sin(alphaY2) },
                { 0, 0,                 1, 0                  },
                { 0, Math.Sin(alphaY2), 0, Math.Cos(alphaY2)  }
            });

            foreach (Vector4 v in vertices) {
                xMatrix1.Map(v);
                yMatrix1.Map(v);

                xMatrix2.Map(v);
                yMatrix2.Map(v);
            }

            vertices = vertices.OrderByDescending(v => v[2]).ToArray();
        }
    }
}
