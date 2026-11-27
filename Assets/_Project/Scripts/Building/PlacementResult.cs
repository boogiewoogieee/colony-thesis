namespace Colony.Building
{
    /// <summary>
    /// Answer to "may a building stand on these cells?".
    /// </summary>
    public enum PlacementResult
    {
        Allowed,
        OutsideGrid,
        Occupied,
    }
}
