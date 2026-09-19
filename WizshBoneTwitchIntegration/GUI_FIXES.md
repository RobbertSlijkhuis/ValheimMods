# GUI fixes

Open items for the new `GUI/` shell (F3). `GUI_OLD/` is legacy and not covered here.

## Redeem wizard

- [ ] **Redo the step 3 cards.** They currently look messy. Needs a fresh layout pass (reward-preview card, the other cards in `RedeemWizard.BuildStep3`, spacing and alignment).
- [ ] **Build step 2.** The per-type config is still an inert placeholder. It needs to be real so each redeem type can be configured per redeem.

## Settings tab

- [ ] **Redo the layout.** Settings feels messy right now, like wizard step 3. Needs a fresh layout pass.

## Placeholder descriptions

Most descriptions in these places are placeholders and need real text:

- [ ] Tabs (under-title descriptions and card descriptions)
- [ ] Redeem wizard
- [ ] Redeem history

## Redeem type descriptions and defaults

- [ ] Write a description and default settings for each redeem type, stored in a place still to be determined. Every time a new redeem of that type is created, it takes its starting values from the defaults for that type. This ties into the step 1 type descriptions and the step 2 per-type config in the redeem wizard.

## Help page: guide list

- [ ] Add a list of guides to the Help page. Selecting one replaces the main section with that guide's content, with a back button to return to the list. These are longer, more extensive explanations than the short descriptions elsewhere. Planned guides:
  - **Creating redeems with the wizard:** how to make a redeem step by step.
  - **The redeem list and history:** how the Redeems list and the Redeem history work, including what "Complete" and "Refund" do.
  - **Viewers:** how to customise viewers, their monsters and so on.
  - **Profiles:** redeems and settings are bound to a profile, plus all the actions a profile supports (create, copy, rename, delete, import, export, sync, reload).
  - **Twitch login:** the steps to log in to Twitch and connect the account.

## Special thanks and future plans

- [ ] Add a new button somewhere (location to be determined) that shows a special thanks section and the plans for future features.
