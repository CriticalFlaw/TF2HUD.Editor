This section is for individual controls that will be displayed on the form, grouped with other controls of similar theme or purpose. Each control will contain instructions on the change that will need to be applied to the HUD's files if enabled.

```
"Crosshair":
[
	{
		"Name": "fh_toggle_xhair_enable",
		"Label": "Toggle the Crosshair",
		"Type": "Checkbox",
		"ToolTip": "Toggle crosshair visibility.",
		"Value": "false",
		"Files": {
			...
		}
	},
	{
		"Name": "fh_toggle_xhair_pulse",
		"Label": "Toggle the Hitmarker",
		"Type": "Checkbox",
		"ToolTip": "Toggle crosshair hitmarker.",
		"Value": "true",
		"Files": {
			...
		}
	},
	{
		"Name": "fh_val_xhair_style",
		"Label": "Style",
		"ToolTip": "Style of crosshair.",
		"Type": "Crosshair",
		"Value": "<",
		"Options":: {
			...
		}
	},
	{
		"Name": "fh_val_xhair_size",
		"Label": "Size",
		"Type": "IntegerUpDown",
		"Value": "18",
		"Minimum": "10",
		"Maximum": "30",
		"Increment": "1",
		"ToolTip": "Size of the crosshair.",
		"Files": {
			...
		}
	},
	{
		"Name": "fh_color_xhair_normal",
		"Label": "Crosshair",
		"Type": "ColorPicker",
		"ToolTip": "Default crosshair color.",
		"Value": "242 242 242 255",
		"Files": {
			...
		}
	},
	{
		"Name": "fh_color_xhair_pulse",
		"Label": "Hitmarker",
		"Type": "ColorPicker",
		"ToolTip": "Color of crosshair when hitting another player.",
		"Value": "255 0 0 255",
		"Files":  {
			...
		}
	}
]
```

---

## Required

### Name

Name of the control. This name must be unique, have no spaces and give an idea as to what this control will do.

!!! note
    To avoid potential conflicts, prefix each control name with an abbreviation for the HUD. Foe example, budhud is bh, flawhud is fh etc.

```
"Name": "fh_color_health_buff"
```

### Label

Text that will be displayed near the control. This space is limited so save longer explanations for the `Tooltip` property.

```
"Label": "Buffed Health"
```

### Type

Defines the type of control this will appear as on the page. Here are the currently supported types:

* `CheckBox` - A toggle, the customization option will either be enabled or disabled depending on the state of this control.
* `Color`, `Colour`, `ColorPicker` and `ColourPicker` - A color picker will open letting the user to select an RGBA color.
* `DropDown`, `DropDownMenu`, `Select` and `ComboBox` - A list of options, capable of holding multiple customizations in one control but can be a bit complex to implement.
* `Number`, `Integer`, `IntegerUpDown` - A number counter that can be changed to any value between a set minimum and maximum. Typically would be used for crosshair sizes and number of rows on the killfeed to display.

```
"Type": "ColorPicker"
```

### Value

Required. The default value for that control. The value has to be compatible with the control type defined earlier. Examples of the values for each control type are:

* true, false - CheckBox
* RGBA Color Code - Color, Colour, ColorPicker, ColourPicker.
* Selected Item Number - DropDown, DropDownMenu, Select, ComboBox, Crosshair.
* Selected Number - Number, Integer, IntegerUpDown.

```
"Value": "0 170 127 255"
```

## Optional

### Tooltip

The text that will display when you hover the mouse over control.

```
"Tooltip": "Color of player's health, when buffed."
```

### Special

Special case property for commands that otherwise cannot be defined in the schema file. For more information on this property, [see here][docs-special].

```
"Special": "StockBackgrounds"
```

### FileName

Name of the file, found in `CustomizationsFolder` that will need to be moved to `EnabledFolder` if this option is selected.

```
"FileName": "hudplayerhealth-broesel.res"
```

### Files

Defines the list of files with nested instructions of where and what needs to be changed in a HUD file for the customization to work.

!!! note
    For details on the how each controls gives instructions to the editor, check out the [Files section here][docs-files].

## ComboBox Only

### ComboFiles

Lists all the files that will be handled by this control, this is used for returning everything back to normal if the user does not make a selection.

### Options

Lists all the selectable options in the menu. Each one will typically have a label, value and instructions on how to apply a customization.

!!! note
    For details on the how each ComboBox option is defined, check out the [Options section here][docs-options].

## ColorPicker Only

### Pulse

If true, a new color entry in the client scheme will be created where the color has a reduced alpha, mimicking a pulse effect.

```
"Pulse": true
```

## IntegerUpDown Only

### Minimum

Sets the minimum value that this control can go down to.

```
"Minimum": 10
```

### Maximum

Sets the maximum value that this control can go up to.

```
"Maximum": 30
```

### Increment

Sets the number by which the control value will go up or down by.

```
"Increment": 2
```

<!-- MARKDOWN LINKS -->
[json-budhud]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/src/TF2HUD.Editor/JSON/budhud.json
[json-flawhud]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/src/TF2HUD.Editor/JSON/flawhud.json
[json-rayshud]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/src/TF2HUD.Editor/JSON/rayshud.json
[json-sample]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/docs/resources/sample.json
[docs-files]: https://www.editor.criticalflaw.ca/json/files/
[docs-special]: https://www.editor.criticalflaw.ca/json/special/
[docs-options]: https://www.editor.criticalflaw.ca/json/options/