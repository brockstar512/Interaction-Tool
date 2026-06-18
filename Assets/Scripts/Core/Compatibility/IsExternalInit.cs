// Compiler shim: enables C# 9 `init`-only setters under Unity's .NET Standard 2.1
// profile, which does NOT ship System.Runtime.CompilerServices.IsExternalInit
// (verified absent from NetStandard/ref/2.1.0/netstandard.dll in 6000.1.12f1).
//
// Required by IT.Player.Control.PlayerInputState (Story 3.1). Zero runtime cost —
// the type is referenced only by the compiler when binding `init` accessors.
namespace System.Runtime.CompilerServices
{
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit { }
}
