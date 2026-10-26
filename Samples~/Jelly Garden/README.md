# Jelly Garden

An original, playable **tap-to-pop puzzle** made with uGUI. Import this sample from
Package Manager, open `JellyGarden.unity`, and press **Play** in Device Simulator.
Tap two or more orthogonally connected jellies of the same color. Collect the three
colored targets within 24 moves. Matching groups pop, columns fall and new jellies
arrive. Reset restarts the same seeded board.

The HUD fits `Screen.safeArea`. Portrait and nearly square foldable screens use a
vertical layout; wide landscape screens put the goals beside the board. The
background intentionally fills the entire display. Art is generated in code;
there are no downloaded sprites, external fonts, game SDKs or additional packages.

For a useful UI audit, select `Jelly Garden/Safe area/Game` as the UI root. This
omits the intentionally oversized scenery. Test while the scene is running; its
Canvas is built at runtime. Geometry checks do not prove text readability or
usability on physical hardware.

The sample uses uGUI and the built-in Unity font. It creates an EventSystem when
none exists and supports the legacy input backend or an installed Input System.
The device tools themselves remain Editor-only; this sample is optional runtime
content, copied into your Assets folder only when imported.

Verified in Unity 6000.6.5f1 on 2026-10-26. The tooling's Unity 2022.3 compatibility
evidence is separate; this new sample has not been replayed in 2022.3 yet.

Original code and procedural artwork: MIT, like the tooling. Device surrounds in
the repository's screenshots are covered separately by THIRD_PARTY_NOTICES.md.
