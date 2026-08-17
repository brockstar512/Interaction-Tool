using System;
using System.Collections.Generic;
using IT.Player.Persistence;

namespace IT.Core.Save
{
    // SAVE.1 (DD1): THE save-file envelope — the one shape progress crosses the disk
    // boundary in. Composes the C-H player DTO with world flags and the resume scene;
    // currentSceneId lives HERE, never on PlayerStateDTO (PB.1 DD8). Class (not struct)
    // as the JsonUtility root; produced/consumed only by PlayerStateBuilder (C-H —
    // the Builder owns serialization, this file owns only the shape).
    [Serializable]
    public class SaveGameDTO
    {
        // SAVE.B B-4: stamped at write by the Builder; B-2: envelope-only, no per-DTO
        // versions. B-3: ANY shape change bumps this. B-1: mismatch = log-and-proceed
        // (structural fail-alive via JsonUtility ignore-unknown/default-missing);
        // the first real migration transfers by name to the first version-bumping story.
        public const int CurrentVersion = 1;

        public int dtoVersion;
        public PlayerStateDTO primaryPlayer;    // primary-only (locked scope); P2+ never serialize (A-3)
        public List<FlagEntry> worldFlags;      // E-1 wire format — dict at runtime, list on the wire
        public string currentSceneId;           // E-2: scene name, active scene at save time
    }

    // SAVE.1 (E-1, owner-ruled): JsonUtility cannot serialize Dictionary — permanent
    // flags cross the wire as a list of entries (PB.3 List<ItemStateDTO> precedent).
    // Uniqueness is NOT enforced at the JSON layer; the Builder's load-side conversion
    // dupe-detects with FIRST-WINS + structured log.
    [Serializable]
    public struct FlagEntry
    {
        public string key;
        public bool value;
    }
}
