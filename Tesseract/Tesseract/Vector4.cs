using System.Linq;

namespace Tesseract {
    public class Vector4 {
        public double[] Data { get; private set; }

        public Vector4() {
            Data = new double[4];
        }

        public Vector4(double[] data) {
            Data = data.Take(4).ToArray();
        }

        public static Vector4 operator +(Vector4 a, Vector4 b) {
            double[] data = new double[4];

            for (int i = 0; i < 4; i++)
                data[i] = a.Data[i] + b.Data[i];

            return new Vector4(data);
        }

        public static Vector4 operator -(Vector4 a, Vector4 b) {
            double[] data = new double[4];

            for (int i = 0; i < 4; i++)
                data[i] = a.Data[i] - b.Data[i];

            return new Vector4(data);
        }

        public static Vector4 operator /(Vector4 a, double f) {
            double[] data = new double[4];

            for (int i = 0; i < 4; i++)
                data[i] = a.Data[i] / f;

            return new Vector4(data);
        }

        public double this[int i] {
            get {
                return Data[i];
            }

            set {
                Data[i] = value;
            }
        }
    }
}
