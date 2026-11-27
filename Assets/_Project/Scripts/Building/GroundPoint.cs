namespace Colony.Building
{
    /// <summary>
    /// A position on the build level, in metres from the centre of the building grid.
    /// It has no height: every building stands at the same level.
    /// </summary>
    public readonly struct GroundPoint
    {
        public GroundPoint(float x, float z)
        {
            X = x;
            Z = z;
        }

        /// <summary>Metres along the X axis (the direction of grid columns).</summary>
        public float X { get; }

        /// <summary>Metres along the Z axis (the direction of grid rows).</summary>
        public float Z { get; }
    }
}
