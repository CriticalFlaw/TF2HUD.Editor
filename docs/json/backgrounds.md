---
title: Background
---

There are 3 special commands that control the management of backgrounds

 - `StockBackgrounds`
 - `HUDBackground`
 - `CustomBackground`

To switch to or inbetween different backgrounds included in your hud, use the special `HUDBackground` property on a control and pass the name of the background you want to enable in the `SpecialParameters` array, excluding the '_widescreen' suffix and .vtf file extension.

TF2HUD.Editor will always copy the accompanying _widescreen.vtf file when handling backgrounds.

```json
  {
    "Special": "HUDBackground",
    "SpecialParameters": [
      "background_upward" // Will enable background_upward.vtf and background_upward_widescreen.vtf
    ]
  }
```

## Custom Backgrounds

To allow the user to set a custom background from a jpg or png, use the `CustomBackground` control, along with the `CustomBackground` special property

```json
  {
    "Type": "CustomBackground",
    "Special": "CustomBackground"
  }
```

:::caution
You must use the `CustomBackground` type AND the `CustomBackground` Special property for custom backgrounds to work properly
:::

## Priority

If the HUD JSON schema includes a `CustomBackground` control, the editor will use the background selected by the user, if the user does not have a custom background selected, it will use the HUD Background defined by the `HUDBackground` or `StockBackgrounds` special commands

## Example Code

### CheckBox

This control will toggle between using stock TF2 backgrounds and the default state of your HUD's `materials/console` folder.

```json
  {
    "Name": "my_hud_use_stock_backgrounds", // Change this to the name of your hud and provide a name for the control
    "Label": "Use Stock Backgrounds",
    "Type": "CheckBox",
    "Value": "false", // Default to default state of materials/console folder
    "Special:": "StockBackgrounds",
  }
```

### ComboBox

This sample creates a ComboBox control where the first two options enable a different custom background image, as well as an option to set default TF2 backgrounds.

```json
  {
    "Name": "my_hud_background_selector_control", // Change this to the name of your hud and provide a name for the control
    "Label": "Menu Background",
    "Type": "ComboBox",
    "ToolTip": "Change the style of background image shown on the main menu.",
    "Value": "0", // The Default selected item
    "Restart": true, // Tell TF2HUD.Editor that applying this customization requires restarting TF2
    "Options": [
      {
        "Label": "Modern Background",
        "Value": "0",
        "Special":"HUDBackground",
        "SpecialParameters": [
          "background_modern"
        ]
      },
      {
        "Label": "Classic Background",
        "Value": "1",
        "Special": "HUDBackground",
        "SpecialParameters": [
        "background_classic"
        ]
      },
      {
        "Label": "Default Backgrounds",
        "Value": "2",
        "Special": "StockBackgrounds"
      }
    ]
  }
```