namespace Tesseract {
    class Edge {
        public Vector4 A { get; set; }
        public Vector4 B { get; set; }

        public Edge(Vector4 a, Vector4 b) {
            A = a;
            B = b;
        }
    }
}
