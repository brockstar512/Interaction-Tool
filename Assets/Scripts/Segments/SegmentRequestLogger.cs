using UnityEngine;
using IT.Player.Control;

namespace IT.Segments
{
    /// <summary>
    /// THROWAWAY diagnostic (Story 5.2, Rung 5): subscribes to all four <see cref="SegmentRouter"/> events
    /// and echoes each to the Console with a distinct <c>[SegmentRouter]</c> prefix, so the Rung 7 manual
    /// verification sweep can watch requests fire in real time. Mirrors Story 5.1's throwaway <c>[Segment]</c>
    /// enter/exit logs; scheduled for pre-ship removal at Story 5.2 close (delete this file + the one
    /// GameObject it is attached to in SegmentTest).
    ///
    /// Attach by hand to an empty GameObject in the SegmentTest scene. Subscribes in OnEnable AND Start
    /// (whichever finds the boot-created router first) and unsubscribes in OnDisable — a <c>_subscribed</c>
    /// guard keeps that idempotent.
    /// </summary>
    public class SegmentRequestLogger : MonoBehaviour
    {
        const string Tag = "[SegmentRouter]";
        bool _subscribed;

        void OnEnable() => TrySubscribe();
        void Start()    => TrySubscribe();   // covers the case where the router wasn't created yet at OnEnable
        void OnDisable() => Unsubscribe();

        void TrySubscribe()
        {
            if (_subscribed) return;
            var r = SegmentRouter.TryGetInstance();
            if (r == null) return;   // router lives on SystemsRoot; retried from Start once boot has run

            r.OnCameraModeChangeRequested += OnCamera;
            r.SlideRequested              += OnSlide;
            r.OnTransportRequested        += OnTransport;
            r.OnScenerRequested           += OnScener;
            _subscribed = true;
        }

        void Unsubscribe()
        {
            if (!_subscribed) return;
            var r = SegmentRouter.TryGetInstance();
            if (r != null)
            {
                r.OnCameraModeChangeRequested -= OnCamera;
                r.SlideRequested              -= OnSlide;
                r.OnTransportRequested        -= OnTransport;
                r.OnScenerRequested           -= OnScener;
            }
            _subscribed = false;
        }

        void OnCamera(SegmentBounds seg, CameraMode mode, PlayerWrapper player)
            => Debug.Log($"{Tag} CAMERA MODE CHANGE — {Id(seg)} → {mode}, player {PlayerId(player)}");

        void OnSlide(SegmentBounds from, SegmentBounds to, SegmentEdge edge, PlayerWrapper player)
            => Debug.Log($"{Tag} SLIDE — {Id(from)} → {Id(to)} ({edge}), player {PlayerId(player)}");

        void OnTransport(TransportRequest r)
            => Debug.Log($"{Tag} TRANSPORT — {Id(r.FromSegment)} → {Id(r.TargetSegment)} ({r.Edge}), " +
                         $"spawn '{r.SpawnPointId}', player {PlayerId(r.Player)}");

        void OnScener(ScenerRequest r)
            => Debug.Log($"{Tag} SCENE — {Id(r.FromSegment)} → \"{r.TargetScenePath}\" ({r.Edge}), " +
                         $"spawn '{r.SpawnPointId}', player {PlayerId(r.Player)}");

        static string Id(SegmentBounds s) => s != null ? s.SegmentId : "∅";

        // Roster index, so logs read "player 0" / "player 1". -1 if the roster or wrapper can't be found.
        static int PlayerId(PlayerWrapper player)
        {
            var roster = PlayerRoster.TryGetInstance();
            if (roster != null && player != null)
            {
                var wrappers = roster.Wrappers;
                for (int i = 0; i < wrappers.Count; i++)
                    if (wrappers[i] == player) return i;
            }
            return -1;
        }
    }
}
