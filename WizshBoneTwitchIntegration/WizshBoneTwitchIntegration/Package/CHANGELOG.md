### 1.0.2
- Added leaderboards: tracks viewers, points, redeem count, favourite redeems, previous names and caused deaths.
- Fixed the login flow (single authorise, 4-hour token-expiry warning).
- Fixed a bug that could leave toasts stuck forever, and added a Clear Toasts button in the Help tab.
- Fixed starred creatures (e.g. one-star deer, starred wolves) not showing their recolour on the skin.
- Fixed recolours deviating from the entered HEX value.
- Colour picker: applies on change, accepts paste and strips a leading `#`; click a colour in the Viewer list to copy its hex code.
- Added a second (emission) colour for recolours, and recolours for Eikthyr through Yagluth.
- New creature form fields: position, name and emission colour.
- Rockies can now be claimed with `claim:`, can talk and can be recoloured.
- Scrolling no longer zooms the camera while the UI is open.
- Creating a profile can now start empty or based on another profile.
- Importing into a live profile now warns first.
- Status HUD now defaults to below the minimap.
- Status HUD now shows the configured key for opening the main UI.
- Status HUD now shows "Twitch:" and "Live:" labels, each with a green/red status dot.
- Restored the missing HUD section in Settings.

### 1.0.1
- Clamped number settings.
- Removed leftover debug log messages, including chat message logging.

### 1.0.0
- Initial release.
