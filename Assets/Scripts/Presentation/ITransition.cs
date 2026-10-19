using System.Collections;

namespace IT.Presentation
{
    // Story 5.4 R3 (DD1 ratified): the pluggable transition contract. Coroutine-shaped so
    // the transporter can sequence COVER → load → REVEAL. Implementations run on UNSCALED
    // time (the transporter holds timeScale at 0 through the cover — the DQ-7 precedent).
    // FadeTransition is v1's only implementation.
    public interface ITransition
    {
        IEnumerator Cover();
        IEnumerator Reveal();
        // Abort path: drop the overlay instantly (the transporter's finally cannot yield).
        void ResetImmediate();
    }
}
