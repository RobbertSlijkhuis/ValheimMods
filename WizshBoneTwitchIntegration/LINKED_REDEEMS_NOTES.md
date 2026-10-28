# Linked redeems - design notes

Status: **paused, no code written, no plan file written.** Gathered 2026-10-28 on branch `feature/twitch-helix-migration`.
The user wants to fix other problems first (see "Blocker" below) before continuing this feature.
Everything under "Decided" was decided by the user. Everything under "Proposed / open" was not.

## Goal

In the redeem wizard's last step (Step 3, today "Twitch reward setup") a redeem can now be either:

1. **A new redeem** - creates its own reward on Twitch (today's behaviour), or
2. **Linked** - the redeem's effect fires when *another reward* is redeemed. The other reward is either one of our own redeems (a redeem with a real reward) or an external reward that already exists on the channel (made by hand on twitch.tv or by another tool).

A linked redeem has no reward of its own and does not manage or touch the target in any way. The target has its own management.

## How things work today (verified in code)

- `TwitchApi/TwitchRewardSync.cs` - creates/edits/deletes our rewards through Helix. `ListAsync(broadcasterId, onlyOwn)` already reads `id` and `title` of every reward (`HelixReward` in `TwitchApi/Models/HelixRewardModels.cs`). `Match` pairs wanted rewards to existing ones by redeem id first, then by title. Rewards of ours that no redeem wants are deleted (`leftovers`).
- `TwitchApi/TwitchRewardLimitHelper.cs` - the 50-reward check. `FetchAsync` reads all rewards and our own, but keeps only the counts and our own list. The list of other people's rewards is thrown away.
- `components/TwitchCustomRewards.cs`:
  - `m_rewardIds` (redeem id -> Twitch reward id): **session-only**, rebuilt every sync, cleared at logout.
  - `IsPushedToTwitch` decides what is sent (enabled, not `Undefined`, global keys allow). `CountPushedRedeems` feeds the slots counter.
  - `PushAsync` blocks the whole push if `wanted.Count + others > 50`.
  - `FindRedeemFor` (line ~227): matches a redemption to a redeem by reward id via `m_rewardIds`, then **falls back to a title match across all redeems**.
  - `HandleRedeem` (line ~269): global gates (banned user, no local player, paused, sleeping, teleporting) -> per-redeem gates (safe zone / `ignoreWard`, dungeon restriction for TerrainEdit + SpawnAbility types) -> big per-type `if` chain -> `FinishRedeem` (leaderboard + auto-resolve). `RedeemException` -> `HandleRedeemException` -> refund.
  - `Update()` dequeues **one redemption per frame**, so a burst of different redemptions keeps some spacing.
  - `ClearAsync` deletes all of *our* rewards at logout.
- `models/data/RedeemData.cs` has a permanent `id` (assigned on profile load, saved, new for copies). The title can change; the id never does.
- EventSub (`TwitchApi/TwitchEventSubClient.cs:344`) subscribes with only `broadcaster_user_id` - **no reward filter**, so redemptions on any reward of the channel, including external ones, already reach `HandleRedeem`.
- Redeems tab row (`GUI/tabs/RedeemsTab.cs` ~585-674): title in `ValheimBeige`, type text in blue `new Color(0.56f, 0.7f, 0.79f)`, cost text `$"{points} pts"`. Cost sort uses `a.points` (~line 510). Synced profiles are view-only (`isSynced`).
- Wizard: `GUI/editors/RedeemWizard.cs`, 3 steps (choose effect -> parameters -> Twitch reward setup). `BuildStep3` ~line 640, `Finish` ~line 1029 -> `RedeemManager.AddRedeem/UpdateRedeem`.

## Twitch constraints

- A Twitch app can only edit/delete rewards created by its own client id (`only_manageable_rewards`). External rewards can't be managed - which is fine, linked redeems never manage anything.
- **Updating a redemption's status (complete/refund) only works for rewards our app created; otherwise Twitch answers 403.** Confirmed by several Twitch dev-forum answers, **not** by the official reference (the page was too long to read the endpoint section). Sources:
  - https://discuss.dev.twitch.com/t/bug-update-custom-reward-redemption-gives-403/61230
  - https://discuss.dev.twitch.com/t/refunding-channel-points-error-403/29929
  - https://discuss.dev.twitch.com/t/call-a-url-when-user-get-reward-with-channel-points/36354
  - Could still be verified against https://dev.twitch.tv/docs/api/reference/#update-redemption-status
- Twitch reward titles are unique per channel (case-insensitive).

## Decided (by the user)

**Link rules**
- A link must point at a redeem that has a real reward on Twitch, or at an external reward. **No chains.**
- Several redeems may link to the same target.
- A redeem that has links pointing at it **cannot itself become linked** (the wizard blocks the switch with a message).
- A broken link (target deleted, disabled/key-blocked so it has no reward, missing, bad hand-edited `profile.yaml`) = the linked redeem stays **inactive with a warning**. No separate load-time validation: "if it's broken, it's broken".
- Links work through **IDs only**: target redeem's `id` for our own, Twitch reward id (plus title for display) for external. A linked redeem is **never matched by title**.

**What a linked redeem is on Twitch**
- Not pushed: `IsPushedToTwitch` is false for it. No slot used, never created/edited/deleted, never touched by `ClearAsync`. It is excluded from `CountPushedRedeems` and the "after next boss" counts.
- Step 3 for a linked redeem is a reward picker; the title/points/colour/limits/cooldown cards are not used.

**Handling a redemption**
- If the target's own effect fails (any of the existing refund paths) -> the redemption is refunded and **no linked effect runs**.
- If a linked effect fails or is skipped by its own per-redeem gates -> **nothing happens** (no refund, no retry). Per-redeem gates (safe zone, dungeon) are evaluated per linked redeem; global gates apply to the whole redemption.
- No extra delay: linked effects must not wait behind the target's delayed effects (e.g. the surprise chest's delayed re-check).
- **All effects of one redemption start in the same frame, the target first** ("for now"; can be revisited if hitching shows up in-game; the alternative was one effect per frame via a coroutine).
- External reward: there is no target effect; every linked effect just runs or skips on its own gates. Completing/refunding is **not possible** (403) so it is skipped for them: `ResolveAsync`/auto-resolve retry (~lines 547-554) must skip redemptions from external rewards, and `HasUnresolvedRedeems` must not count them.
- Auto-resolve/leaderboard happen once per redemption, not per effect.

**Leaderboard and death credit**
- The leaderboard records **only the target** (points added once, +1 use under the target's title; the reward's Twitch title for an external reward). `LeaderboardHelper.RecordRedeem(event, RedeemData)` therefore needs a variant that takes a plain title.
- Death credit: the redeemer's id/name come from the shared event, so the right viewer is credited. Deaths are credited under the same title the leaderboard uses (target's title; Twitch title for an external reward). `RedeemerTagHelper.TryResolve` (line 46) currently returns false when no redeem of ours matches the event's title, so external rewards would get no credit - needs a fallback to the event's reward title.

**Redeems tab**
- Points column shows **"Linked"** instead of `{points} pts`; the redeem's name uses the same blue as the type text.
- Broken link: the "Linked" text turns **red**; the tooltip names the problem (target missing / disabled). A healthy link's tooltip shows the target's name.
- Cost sort: linked redeems sort **as if they cost 0** (the sort must not read their old `points`).
- Trigger button: runs **only the redeem's own effect**, for targets and linked redeems alike (no fake redemption through the links).
- Copy: copying a linked redeem (also to another profile) copies it as is. A link to one of our own redeems is then broken in the other profile (by the broken-link rule); an external link keeps working.
- Deleting a target: the confirmation says how many links will break. Switching a target Off: a toast with the same count.
- Synced profile: linked redeems are view-only, like every other redeem.

## Title lookups - findings and the user's conclusion

The title fallback in `FindRedeemFor` is used for:
1. The Trigger button and the `WBTIUseRedeem` console command (`commands/UseRedeemCommand.cs:67`): they build a fake event with only a title and no reward id (`helpers/TestRedeemEventHelper.cs`).
2. Real redemptions while `m_rewardIds` doesn't know the reward yet (after login until the first sync finishes, or after a cancelled sync / an unreadable created id).

Why it matters for links: a linked redeem has no reward of its own, so an unrelated external reward with the same title could fire it through the fallback. (Disabled redeems already have this problem today; not part of this feature.)

**Bigger finding:** the Twitch title is also the redeem's identity inside saved world objects. These components save `CustomRewardTitle` in their ZDO and look the redeem up with `RedeemHelper.GetRedeemByTitle` in `Awake` to read their config (verified in `TwitchPersistentDamage.cs:56-81`):
- `TwitchPersistentDamage` (`PersistentDamage_WBTI`), `TwitchCreaturePersistentData` (`CreatureData_WBTI`), `TwitchPiecePersistentData` (`WBTIPersistentPieceData`), `TwitchSurpriseChest` (`SurpriseChestData_WBTI`)
- `TwitchBasePersistentData` saves `"{redeemerId}|{rawTitle}"` (`WBTI_Redeemer`) for death credit.
- `RedeemerTagHelper.TryResolve` resolves the credit title from the event title.
- A rough count of title/redeem-title identifiers: 91 occurrences in 25 files (includes the leaderboard, settings and wizard, so only part of this is the ZDO identity).

Consequence for links: a linked effect run with the *original* event would store the target's (or the external reward's) title, so its spawned objects would load the **target's** damage/creature/chest config, or fail to find any redeem for an external reward. A workaround (an event copy per linked effect carrying the linked redeem's own title) would also change what death credit resolves, conflicting with the decision above unless the original reward title travels separately.

**The user's conclusion:** stop building on titles; identity should be the permanent `RedeemData.id`, and the title lookups (including the `FindRedeemFor` fallback) should go. The user said this is needed for the feature, not optional. (I had called it "not needed for this feature" - that was wrong.)

Implications (not yet designed):
- Store the redeem `id` in the ZDOs instead of the title. **Old saved worlds contain titles**, so old objects need a title -> id fallback read (or a migration) for a while.
- `FindRedeemFor` matches by reward id only; Trigger and the console command pass the `RedeemData` directly into the handling code (new `HandleRedeem` overload / signature change).
- Real redemptions in the short window before the id map is filled would become "not in the active profile" (left unresolved in the history) - unless that window is closed some other way.
- Death credit / leaderboard are keyed by title strings on purpose (display names). They stay title-keyed, but the credit must keep the *target's* title while the object's config identity is the linked redeem's id.

## Proposed / open (not decided)

- Whether to remove the `FindRedeemFor` title fallback entirely. The user leans yes ("too risky"), tied to the ID-based identity work above; the alternatives offered were skipping only linked redeems, or skipping all unpushed redeems.
- Exact shape of the ID-based identity change (which components, ZDO format, migration for old worlds, how the original reward title travels for credit).
- Step 3 UI: where the "new redeem / link to another" choice sits, how the picker lists our redeems and the channel's external rewards (needs the full reward list kept from `FetchAsync`), and how it blocks duplicates/invalid targets. **Not examined yet.**
- Persisting the link: a new field(s) on `RedeemData` saved in `profile.yaml` (target redeem id, or reward id + title for external). Not designed.
- The 50-limit math: linked redeems must not count (`wanted.Count` only covers pushed redeems, so this should follow from `IsPushedToTwitch`; to confirm during planning).
- Search in the Redeems tab ("title or type") - whether it should also match the target's name. Not discussed.

## Blocker

The user wants to fix other problems first - most likely the title-as-identity problem above - before continuing. Resume with: read this file, then continue with the Step 3 UI and the identity design, and only then write a plan. The user wants to be told explicitly before a plan is written ("I'll tell you when").
