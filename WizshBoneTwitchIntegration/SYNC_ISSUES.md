# Profile sync - open issues and planned work

Written 2026-09-29. Covers the profile **Sync** feature (`helpers/ProfileSyncHelper.cs`,
`ProfileManager.IsSyncedProfile` / `MarkAsSynced`, the Sync button in `GUI/tabs/ProfilesTab.cs`) and how
the new GUI treats a synced profile. Nothing in this document has been verified in-game yet.

## How sync works today (short)

- Sync sends the active profile's whole `profile.yaml` (redeems, creature groups, settings) to every
  connected player through the `WBTI_SyncProfile` RPC.
- Whoever presses Sync is effectively "the owner". The sender's own profile is **not** marked synced; every
  receiver's copy gets a `.synced` marker file.
- A receiver writes the file **immediately**, with no prompt. If the profile is active it is hot-reloaded.
  The server relays the package on to the other clients.
- A synced profile is read-only in the UI for the things that affect redeems and gameplay (see
  "Lock rules" below).

## Planned: confirm before a sync is applied

**Goal:** a receiving player must accept before anything is written to their profile.

### Requirements
1. **Accept / decline dialog** on the receiver before `RPC_ReceiveProfile` writes anything. Declining leaves
   the disk and the loaded data untouched.
2. **Extra warning when the incoming profile has the same name as a local profile that is *not* synced.**
   That case silently replaces the player's own work today (a one-deep `profile.yaml.bak` is the only
   protection). The dialog should say plainly that their local profile will be replaced, and name it.
3. The normal case (new name, or re-sync of an already-synced profile) gets the plain "accept this profile?"
   text.

### Design notes / things to decide
- **The dialog must work without the shell open.** A sync can arrive while the player has never pressed F3,
  so the shell (and `ToastNotifications`, which is initialised inside it) may not exist. Reuse the exit-confirm
  path (`WizshBoneGUI.ShowExitConfirm`) or build the dialog so it can open standalone;
  `GUI/dialogs/ConfirmDialog.cs` already has `title / description / confirmText / cancelText / onConfirm /
  onCancel`. It needs `InputBlockGate` handling like the other dialogs.
- **Hold the package in memory** (profile name + yaml) until the player answers. Keep only the latest pending
  package per profile name, so a sender re-syncing does not stack dialogs. Consider a cap on how many
  different pending profiles can queue up.
- **Server relay must not depend on the host's answer.** Today the server writes, then relays. With a
  prompt, the server should relay the untouched package immediately (it is only a relay) and let each
  player, including the host, accept or decline independently. Otherwise one player declining blocks
  everyone else.
- **Who is asking?** The dialog should show the sender's player name. The RPC only gives a peer uid, and for
  a relayed package the uid is the *server*, not the original sender. The original sender's name has to be
  put in the package (and ideally stamped by the server so it cannot be spoofed by a client).
- **Feedback to the sender:** currently none (no "3 accepted, 1 declined"). Optional, but worth deciding
  together with the above.
- **Accepting should run the same tail as today:** backup (`.bak`) if a non-synced local profile is being
  replaced, write, `MarkAsSynced`, hot-reload if active, HUD message, `ProfileReceived` event.
- **Optional:** "always accept from this player" / "don't ask again". Probably not for a first version.

## Known issues (not fixed)

| # | Issue | Notes |
|---|-------|-------|
| 1 | **No owner identity.** Any player can push a sync and overwrite everyone's copy of a profile with that name. | Only the receiver's own Sync button is disabled (synced profiles). A player who has a same-named *unsynced* profile can push over the real one. The confirmation dialog above reduces the damage; it does not create an owner. |
| 2 | **The lock is UI-only.** | Copying a synced profile makes an unlocked copy (probably intended). The yaml can be edited on disk. Console/test commands and `RedeemManager` / `ProfileSettingsHelper` themselves do not check `IsSyncedProfile`. If real enforcement is wanted it belongs in the data layer. |
| 3 | **A sync overwrites the receiver's local values of settings that stay editable.** | Chatting settings, Auto resolve, Enable on login and the discount % are editable on a synced profile but live inside `profile.yaml`, so the next sync replaces them. Option: keep those as local overrides that survive a sync. |
| 4 | **Twitch-side rewards may not follow a sync.** | After a sync the redeem list is reloaded, but nothing re-pushes rewards to Twitch (`TwitchCustomRewards.SetRewards`). Unverified - check whether a reload / Profiles-tab "Reload" is needed before the receiver's Twitch rewards match the new redeems. |
| 5 | **No schema/version check on the incoming yaml.** | A profile from a newer/older mod version may not deserialize. Only length and name are validated. |
| 6 | **Package size is only capped at 2 MB of yaml** (`MaxYamlLength`). | Whether Valheim's RPC layer accepts a package that large is unverified. |
| 7 | **The `.bak` backup is one deep and has no UI.** | A second sync of the same name overwrites the first backup; restoring means renaming the file by hand. |
| 8 | **Viewers tab and Creature groups tab were not checked for sync interplay.** | Unknown whether viewer data is per-profile or global. Creature groups is still hidden/stubbed. |
| 9 | **Profiles tab:** Delete is not blocked on synced profiles (probably fine), and it was not checked whether synced profiles are visibly marked in the list. | |
| 10 | **Sender gets no feedback** beyond "no players connected". | See the planned dialog notes. |

## Already fixed this round (for reference)

- Sync receiver now validates the profile name (`ProfileManager.IsValidProfileName`, also rejects `.`/`..` and
  trailing dot/space) and rejects empty or oversized yaml before touching the disk. Previously a crafted
  name could write `profile.yaml` / `.synced` outside the profiles folder.
- Receiving a sync backs up a replaced non-synced local profile to `profile.yaml.bak`.
- Receiving a sync shows a HUD message, raises `ProfileSyncHelper.ProfileReceived`, refreshes the visible
  tab, and closes an open redeem wizard (without saving) if the active profile was replaced.
- Redeems tab on a synced profile: New disabled, On/Off disabled, Delete hidden, Edit becomes **Show**
  (view-only wizard, `RedeemWizard.OpenView`). Edit/create/toggle/delete handlers also refuse.
- Settings tab on a synced profile is view-only per row for **Redeem prefix, Creatures, Indestructible,
  Safezones**.
- Home: "Create a new redeem" disabled on a synced profile; Active profile card says "synced, read-only".

## Lock rules (current)

| Area | On a synced profile |
|------|---------------------|
| Redeems tab | view-only (Show), no new / toggle / delete; Test and Copy still work |
| Settings: Redeem prefix, Creatures, Indestructible, Safezones | view-only |
| Settings: Enable redeems on login, Auto resolve, whole Chatting section | editable |
| Home: chatting, auto resolve, enable on login, discount | editable |
| Home: Create a new redeem | disabled |
| Profiles tab: Import into it, Rename, Sync | blocked (existing checks) |

## Needs checking in-game

- Disabled look of the custom widgets (SearchableDropdown, SearchableChecklist, color swatch, entry lists)
  under a `CanvasGroup` with `interactable = false` - text must stay readable.
- In the view-only wizard, dropdowns/checklists cannot be opened, so only the selected value or summary is
  visible, not the full option list. Decide whether that is enough for people debugging a broken profile.
- Scrollbars in the view-only wizard stay draggable (`KeepScrollbarsInteractive`).
- Settings tab: only the intended rows are locked on a synced profile; description line text is correct.
- A real sync between two players: HUD message, wizard closing when a sync lands mid-edit, tab refresh,
  server relay to a third player, `.bak` created when replacing a same-named local profile.
- Step 3 of the redeem wizard (unrelated to sync): label/input gap, toggle label centering, prefix badge
  look, Back/Cancel/Next layout.
