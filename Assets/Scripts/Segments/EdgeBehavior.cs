namespace IT.Segments
{
    /// <summary>
    /// What a player crossing OUT through a given segment edge should trigger (Story 5.2).
    /// Authored per-edge on <c>SegmentConfig</c> (Rung 3); resolved by the SegmentRouter (Rung 5)
    /// off the <c>SegmentExited</c> crossing (spec DD3) and dispatched as a typed request.
    ///
    /// Fixed set → an enum, not data (C-B). There is deliberately NO <c>None</c> value: an
    /// unconfigured edge — or a segment with no <c>SegmentConfig</c> at all — resolves to
    /// <see cref="Seamless"/>, the do-nothing default (spec Q2).
    ///
    /// DEFAULT VALUE: <see cref="Seamless"/> is 0, so it is the C# enum default. Segments without an
    /// explicit <c>SegmentConfig</c>, or a <c>SegmentConfig</c> with unconfigured edges, default to
    /// Seamless behaviour for all four edges. This matches spec DD1: a minimum-viable segment
    /// (<c>SegmentBounds</c> only) has all edges Seamless.
    /// </summary>
    public enum EdgeBehavior
    {
        /// <summary>Free crossing; the router fires no request. The default for any unconfigured edge.</summary>
        Seamless,

        /// <summary>
        /// Player crosses freely (like <see cref="Seamless"/>), but the camera pans deliberately to the
        /// neighbour — <c>SlideRequested</c> → Epic 6 Story 6.3. Per-player crossing (Q8); the crossing
        /// half is Seamless, only the camera half is distinct (spec DD7).
        /// </summary>
        Slide,

        /// <summary>
        /// In-scene relocation to another segment in the SAME scene (Ruling 1 invariant) —
        /// <c>TransportRequested</c> → Story 5.4. All-together for multi-player (Q6).
        /// </summary>
        Transport,

        /// <summary>
        /// Cross-scene relocation — <c>SceneRequested</c> → Story 5.4 (scene load + spawn).
        /// All-together, forced by scene unload (Q6); carry-over governed by Epic 4.5 CarryOverMode.
        /// </summary>
        Scene,
    }
}
