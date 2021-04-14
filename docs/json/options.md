This section is for defining individual items in a ComboBox or DropDown menu. As such, only refer to this section if your control is of type `DropDown`, `DropDownMenu`, `Select` or `Crosshair`.

```
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

---

### Label

Text for the option that is displayed on screen.

```
"Label": "Broesel"
```

### Value

The underlying value of this option, that will be used by the editor.

```
"Value": "1"
```

### Files

A list of files, each with specific instructions on which element property needs to be updated and with which values. The path to each file is relative to the root of the HUD and each nested object within the file path has to match what's in the HUD file, otherwise the editor will not be able to apply the changes. For more information on this property, [see here][docs-files].

```
"Files": {
	"resource/ui/huditemeffectmeter.res": {
		"HudItemEffectMeter": {
			"xpos": "c-60",
			"ypos": "c120"
		},
		"ItemEffectMeterLabel": {
			"wide": "120"
		},
		"ItemEffectMeter": {
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

### FileName

Name of the file, found in `CustomizationsFolder` that will need to be moved to `EnabledFolder` if this option is selected.

!!! note
    Only add this property if you want a customization file to be moved for this option. 

```
"FileName": "hudplayerhealth-broesel.res"
```

### Special

Special case property for commands that otherwise cannot be defined in the schema file. For more information on this property, [see here][docs-special].

!!! note
    Only add this property if you want to use a special command that's available [here][docs-special].
	
```
"Special": "StockBackgrounds"
```

<!-- MARKDOWN LINKS -->
[json-budhud]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/src/TF2HUD.Editor/JSON/budhud.json
[json-flawhud]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/src/TF2HUD.Editor/JSON/flawhud.json
[json-rayshud]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/src/TF2HUD.Editor/JSON/rayshud.json
[json-sample]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/docs/resources/sample.json
[docs-files]: https://www.editor.criticalflaw.ca/json/files/
[docs-special]: https://www.editor.criticalflaw.ca/json/special/