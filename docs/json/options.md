---
title: List Options
---

This section covers individual options available in a list-type controls like `DropDown`, `DropDownMenu` or `Select`. Below is an example of a list control with options for enabling specific animations based on the option selected.

```json
"Name": "rh_val_uber_animation",
"Label": "Uber Style",
"Type": "ComboBox",
"Value": "0",
"Options": [
  {
    "Label": "Flash",
    "Value": "0",
    "Files": {
      "scripts/hudanimations_custom.txt": {
        "comment": [
          "RunEvent HudMedicSolidColorCharge",
          "RunEvent HudMedicRainbowCharged"
        ],
        "uncomment": [
          "RunEvent HudMedicOrangePulseCharge"
        ]
      }
    }
  },
  {
    "Label": "Solid",
    "Value": "1",
    "Files": {
      "scripts/hudanimations_custom.txt": {
        "comment": [
          "RunEvent HudMedicOrangePulseCharge",
          "RunEvent HudMedicRainbowCharged"
        ],
        "uncomment": [
          "RunEvent HudMedicSolidColorCharge"
        ]
      }
    }
  }
]
```

### FileName

**Optional**. Name of the file or folder that will be moved from `CustomizationsFolder` to `EnabledFolder` if this option is selected.

:::caution
Do not use this property in conjuction with **Files** or **Special**. Only use one of the three separately.
:::

```json
"FileName": "hudplayerhealth-broesel.res"
```

---


### Files

**Optional**. Defines a list of files that will need to be updated if the given option is selected, where each file path is relative to the root of the HUD.

:::caution
Each nested object within the file path has to match the contents of the HUD file, otherwise the editor will not be able to apply the changes.
:::

For in depth documentation on File editing, see [this section][docs-files].

```json
"Files": {
	"resource/ui/huditemeffectmeter.res": {
		"HudItemEffectMeter": {
			"xpos": "c-60",
			"ypos": "c120"
		},
		"ItemEffectMeterLabel": {
			"wide": "120"
		}
	},
	"resource/ui/huddemomancharge.res": {
		"ChargeMeter": {
			"ypos": "c110"
		}
	},
	...
}
```

---

### Label

**Required**. Sets the name of the option as it will be shown on screen.

```json
"Label": "Broesel"
```

---

### RenameFile

**Optional**. Defines the name of the file `OldName` that will be renamed to `NewName` when this option is selected. Revert the file name back to `OldName` when this option is deselected.

```json
"RenameFile": {
	"OldName": "#users/dane_/",
	"NewName": "#users/dane/"
}
```

---

### Value

**Required**. Sets the underlying value for this option that will be used by the editor.

```json
"Value": "1"
```

---

### Special

**Optional**. Special case property for customizations that otherwise cannot be through the schema. For more information, see [this section][docs-special].

:::caution
Do not use this property in conjuction with **Files** or **FileName**. Only use one of the three separately.
:::

```json
"Special": "StockBackgrounds"
```

---

### SpecialParameters

**Optional**.

```json
"SpecialParameters": []
```

<!-- MARKDOWN LINKS -->
[docs-special]: http://criticalflaw.ca/TF2HUD.Editor/json/special/
[docs-files]: http://criticalflaw.ca/TF2HUD.Editor/json/files/
