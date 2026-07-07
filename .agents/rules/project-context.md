PROJECT: Ink Jam (working title — final name confirmed in Phase 4, Task P4.1)
WHAT THIS IS
A hybrid-casual spatial-extraction slide puzzle for Android, in the same
proven subgenre as Color Block Jam / Screwdom. Core loop: a grid of
colored/patterned tile-pieces; the player drags a piece and it slides in
a straight line until it hits a wall, another piece, or an obstacle;
the goal is to get every piece off the board through the exit frame
that matches its color/pattern. One sentence to explain, learnable in
one level, hard to master by level 100. Do not redesign this core loop
— it is deliberately not reinvented, because the genre leaders have
already proven it works.
THE DIFFERENTIATING HOOK — "INK BLEED"
A hazard tile that spreads to one adjacent cell every N player moves,
like ink soaking through paper. A "bled" cell locks any tile on it until
enough surrounding tiles are cleared to "cleanse" it. This does not
exist in any competitor's obstacle roster — it is the game's real,
defensible point of difference. Treat it with more care than the other
(more generic) obstacle types.
FULL OBSTACLE ROSTER (v1)
Fixed tile (immovable blocker) -> Layered tile (must be "exited" N times
before it actually leaves) -> Ink Bleed (the hook) -> Linked/Twin tiles
(a pair that must both exit within one move of each other). Unlock
cadence: roughly one new obstacle type every 40-50 levels, in that
order, so Ink Bleed lands deliberately late as a real "new thing"
moment rather than up front.
ART DIRECTION
Original anime/manga-inspired ink-wash aesthetic — brushstroke textures,
manga screen-tone shading, panel-border UI framing. This is ORIGINAL art
in that style, never licensed/traced characters (Bleach, Naruto, Demon
Slayer, etc.) — using real IP here is a copyright problem that gets an
app pulled, not a legal gray area. This visual approach is a genuine,
validated gap: anime style is common in gacha/RPG mobile games but
almost absent in this specific puzzle subgenre, where most competitors
use generic 3D low-poly or flat cartoon assets.
CONTENT SCOPE
~150-200 hand-tuned levels covering onboarding through all 4
obstacle-type introductions with a properly tested difficulty curve.
Everything past that point is fully procedural (Section on the
generator below) — do not hand-build more than this.
PROCEDURAL GENERATOR — SOLVABILITY BY CONSTRUCTION
Levels are never generated-then-validated (too slow, can produce
unsolvable boards). Instead: start from a SOLVED state (every tile
already exited into its matching frame), then run the game rules in
REVERSE a random number of times — pick an exited tile, slide it
backward onto the board along a legal reverse path, repeat N times.
The exact reverse of that scramble sequence IS a guaranteed valid
solution, so no solver or validity-checker is ever needed. Obstacles are
layered in during the reverse-walk itself. This is the single most
technically important system in the project — treat generator work with
Planning Mode, not Fast Mode.
MONETIZATION MODEL
Roughly 75-80% ads / 20-25% subscription (deliberately more ad-weighted
than a typical hybrid-casual title, because puzzle players tolerate and
like ads more than strategy/RPG players do). Rewarded video is
opt-in-only and is the primary revenue driver — never force it, always
show an explicit "watch ad for X" prompt the player taps voluntarily.
Interstitials are secondary and capped: between levels only, every 3-4
levels, never mid-puzzle, always closable within 15 seconds (a hard
Google Play policy requirement), and skipped entirely for a new
player's first several sessions. Banner ads live on the main
menu/level-select screen only — never inside the active puzzle view.
Subscription tier is called "Senpai Pass": removes all ads, grants
exclusive tile-skins/board themes, a small monthly soft-currency
stipend, and unlocks Daily Challenge retry.TARGET MARKET
India-first (not a "test market, monetize elsewhere" framing — India IS
the target). This means: test on genuine mid-range Android hardware,
not just a flagship phone or the Editor; keep shader/particle
complexity conservative; expect a lower blended eCPM than a Tier-1-only
app (a fixed feature of this market, not a solvable problem); and treat
Hindi/Hinglish store-listing and marketing localization as worthwhile,
while treating full in-game text translation as a small, manageable
surface area since the core mechanic is visual/numeric, not
text-heavy.
TECH STACK
- Engine: Unity 6 LTS, C#.
- Ads: Google AdMob as the base network + AppLovin MAX as the
mediation layer on top (never run AdMob standalone).
- Billing: Google Play Billing Library v7+ (older versions are
actively blocked by Play).
- Analytics: Firebase Analytics + Crashlytics, wired in from day one.
- Backend: none. Local device save only — no account system, no cloud
sync, for v1.
- Target Android 15 (API level 35) minimum. Play Integrity API for any
integrity checks (never the deprecated SafetyNet API).
CODING STANDARDS (non-negotiable across every task in this project)
- No empty catch blocks — every caught exception is handled or logged
with intent, never silently swallowed.
- Single-responsibility scripts/classes. Do not let one file grow into
a god-class.
- Core game logic — Board, Tile, the slide-movement engine, obstacle
rules, and the procedural generator — lives in plain C# classes under
Scripts/Core and Scripts/Generator with ZERO references to
UnityEngine, MonoBehaviour, Transform, or Input. MonoBehaviours
(Scripts/Gameplay, Scripts/UI) are thin adapters that translate Unity
scene/input events into calls on that plain-C# core, and translate
core state back into visuals. This mirrors the "services never touch
req/res" separation used in this project's other (web) codebases —
same principle, applied to Unity.
- No hardcoded difficulty/balance numbers scattered through code —
tunable constants live in one clearly-marked config/ScriptableObject
location, because Phase 3 will need to retune these against real
player data without a code archaeology exercise.
- Level data is versioned JSON (a formatVersion field from day one) so
the format can evolve without breaking old save/level data.
FOLDER STRUCTURE (set up in Task P0.1, referenced by every later task)
Assets/Scripts/Core
— pure C#, zero UnityEngine references
Assets/Scripts/Gameplay
— MonoBehaviours driving Core from the scene
Assets/Scripts/Obstacles
— one file per obstacle type, shared IObstacle
Assets/Scripts/Generator
— procedural level generator, pure C#
Assets/Scripts/UI
— MonoBehaviours for menus/HUD
Assets/Scripts/Data
— ScriptableObjects + JSON level DTOs
Assets/Resources/Levels
— hand-built level JSON files
OUT OF SCOPE FOR V1 (do not build unless a later task explicitly asks)
No multiplayer/social/friend mechanics. No LiveOps calendar/seasonal
events beyond the festival-timed cosmetic mentioned in Phase 4. No
offerwall. No coin-pack IAP beyond the subscription. No cloud
save/account system.
