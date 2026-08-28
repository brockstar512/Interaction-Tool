using System;

namespace IT.Presentation
{
    // 4.6.1 (DD1/DD3): the Surface-2 request shape. The coordinator routes these;
    // the module owns the queue (game-over PREEMPTS and CLEARS, all else FIFO —
    // DQ-5(iii) ruled). Options are label + callback pairs; the module renders
    // buttons, the requester owns the consequences (constraint 1 both ways).
    public enum PromptPriority { Normal, GameOver }

    public class PromptRequest
    {
        public string Title;
        public string Body;
        public PromptPriority Priority = PromptPriority.Normal;
        public (string label, Action onChosen)[] Options;
        // 4.6.2 R5 (OQ-F(i) ruled): > 0 = a self-dismissing TOAST — auto-selects
        // option 0 after this many UNSCALED seconds (must run while paused).
        // 0 (default) = a normal prompt; no behavior change for existing callers.
        public float AutoDismissSeconds;
    }

    // DQ-1(c): typed per-surface contracts. Surface 2 ships in this story; the
    // other three are DECLARED now so the coordinator's handle set is stable —
    // their members arrive with their stories (4.6.3 Hud, 4.6.4 World/Fx).
    public interface IScreenPromptModule
    {
        void Enqueue(PromptRequest request);
    }

    public interface IWorldPromptModule { }   // 4.6.4 extends
    // 4.6.3 (DQ-4(b) ruled): Surface-3 contract — HUDManager implements it and
    // the static dies; PlayerStatusManager reaches it via Presentation.Hud.
    public interface IHudModule
    {
        IT.Player.HUD.PlayerStatusHUD InitializePlayerHUD(
            IT.Player.StateMachine.PlayerStateMachine player, string playerId);
        void DestroyPlayerHUD(string playerId);
    }
    public interface IScreenFxModule { }      // 4.6.4 extends
}
