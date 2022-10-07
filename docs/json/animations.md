---
title: Animations
---

A definition of a HUD animations file contains event names and values, where each value contains a list of HUD animation objects.

The main properties for writing a standard HUD animation are:

 - `Type`
 - `Element`
 - `Property`
 - `Value`
 - `Interpolator`
 - `Delay`
 - `Duration`

:::caution
Unlike HUD files, animation events are not merged, and each time an event is written, it overwrites the other occurences of that event (in the current file). This is to prevent animations from breaking.
:::

## Interpolator Parameters

The frequency parameter of a HUD animation with the interpolator `Pulse` can be passed via the `Frequency` property

The bias parameter of a HUD animation with the interpolators `Gain` and `Bias` can be passed via the `Bias` property


## Example HUD Animation

```json
"Files": {
  "scripts/hudanimations_examplehud.txt": {
    "HudHealthBonusPulse": [
      {
        // Animate	PlayerStatusHealthBonusImage 	Alpha		255		Linear 0.0 0.2
        "Type": "Animate",
        "Element": "PlayerStatusHealthBonusImage",
        "Property": "Alpha",
        "Value": "255",
        "Interpolator": "Linear",
        "Delay": "0.0",
        "Duration": "0.2"
      },
      {
        // Animate	PlayerStatusHealthBonusImage 	Alpha		0			Linear 0.2 0.4
        "Type": "Animate",
        "Element": "PlayerStatusHealthBonusImage",
        "Property": "Alpha",
        "Value": "0",
        "Interpolator": "Linear",
        "Delay": "0.2",
        "Duration": "0.4"
      },
      {
        // RunEvent HudHealthBonusPulseLoop	0.4
        "Type": "RunEvent",
        "Event": "HudHealthBonusPulseLoop",
        "Delay": "0.4"
      }
    ]
  }
}
```
