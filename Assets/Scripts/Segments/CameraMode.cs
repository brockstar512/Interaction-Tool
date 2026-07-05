namespace IT.Segments
{
    /// <summary>
    /// Per-segment camera behaviour selector (Story 5.2), authored on <c>SegmentConfig</c> and
    /// REQUESTED via the router's <c>CameraModeChangeRequested</c> (Q5) — 5.2 only asks; Epic 6 executes.
    ///
    /// v1 PLACEHOLDER set (Q4). Epic 6 owns the real modes (FixedLocked, DeadzoneFollow, AlwaysFollow,
    /// SegmentSlide, …) and will EXTEND this enum.
    ///
    /// BACKWARD-COMPAT INVARIANT (DD6): Epic 6 may ADD values, but MUST NOT rename, remove, or reassign
    /// the underlying int of an existing value — so v1-authored data (<c>cameraMode = Follow</c>) stays
    /// valid across the extension. Explicit int values are pinned here to make that contract enforceable.
    /// </summary>
    public enum CameraMode
    {
        /// <summary>Camera follows the player (deadzone / framing specifics are Epic 6's).</summary>
        Follow = 0,

        /// <summary>Camera holds a fixed framing for the segment.</summary>
        Fixed = 1,
    }
}
