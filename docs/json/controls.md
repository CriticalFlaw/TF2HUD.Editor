This section covers the controls that are displayed on the HUD page, grouped with other controls of similar purpose. This will include properties only available to specific types of controls.

```json
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

### Name

**Required**. Name of the control. This name must be unique, have no spaces and suggest the control's purpose.

!!! note
    To avoid conflicts, prefix each name with an abbreviation for the HUD. Example; budhud is bh, flawhud is fh.

```json
"Name": "fh_color_health_buff"
```

---

### Label

**Required**. Text displayed near the control. This space is limited, so save longer explanations for the [Tooltip](https://www.editor.criticalflaw.ca/json/controls/#tooltip) property.

```json
"Label": "Buffed Health"
```

---

### Type

**Required**. Defines the type of control this will appear as on the page. Below are the supported types:

* **CheckBox** - Toggling this will either enable or disablee the customization option attached to this control.
* **ColorPicker** - Opens a color picker for the user to select an RGBA color. Can also use **Color**, **Colour** or **ColourPicker**.
* **ComboBox** - Contains a list of [options](https://www.editor.criticalflaw.ca/json/controls/#options), each with their own customization instructions. Can also use **DropDown**, **DropDownMenu** or **Select**.
* **IntegerUpDown** - An integer counter ranging between set minimum and maximum values. Commonly to be used for crosshair sizes and number of rows on the killfeed. Can also use **Number** or **Integer**.
* **Crosshair** - TODO. Can also use **CustomCrosshair**.
* **Background** - TODO. Can also use **CustomBackground**.
* **TextBox** - TODO. Can also use **Text**.

---

### Value

**Required**. Default value for the control, compatible with the selected control [type](https://www.editor.criticalflaw.ca/json/controls/#type). Allowed values per type are listed below:

* **CheckBox** - true, false.
* **ColorPicker** - RGBA color code, **30 30 30 200**.
* **ComboBox** - Integer value of the option selected.
* **IntegerUpDown** - Integer value within the set range.
* **Crosshair** - Integer value of the option selected.
* **Background** - Not required.
* **TextBox** - Not required.

---

### Tooltip

**Optional**. Text that shown when the user hovers their mouse over control.

```json
"Tooltip": "Color of player's health, when buffed."
```

---

### Restart

**Optional**. If true, the editor will tell the user that the game must be restarted for this customization to apply.

```json
"Restart": false
```

---

### Preview

**Optional**. Sets the image that previews the effect of this customization option. If a valid image is supplied, a question mark button will appear near the control that will open a modal with the linked image when pressed.

```json
"Preview": "https://user-images.githubusercontent.com/6818236/114957712-9bd4d400-9e2f-11eb-8612-479313086c47.jpg",
```

---

### Special

**Optional**. Special case property for customizations that otherwise cannot be through the schema. For more information, see [this section][docs-special].

```json
"Special": "StockBackgrounds"
```

---

### SpecialParameters

**Optional**. This parameter is required when using the special command `HUDBackground`, see [Custom Backgrounds][docs-backgrounds]

```json
"SpecialParameters": []
```

---

### Files

**Optional**. Defines a list of files that will need to be updated if the given option is selected, where each file path is relative to the root of the HUD.

For in depth documentation on File editing, see [this section][docs-files].

!!! warning
    Each nested object within the file path has to match the contents of the HUD file, otherwise the editor will not be able to apply the changes.

---

### FileName

**Optional**. Name of the file or folder that will be moved from `CustomizationsFolder` to `EnabledFolder` if this option is selected.

```json
"FileName": "hudplayerhealth-broesel.res"
```

---

### RenameFile

**Optional**. TODO

```json
"RenameFile": ""
```

---

### ComboFiles

**Optional, ComboBox Only**. Lists all the files that will be handled by this control, this is used for returning everything back to normal if the user does not make a selection.

```json
"ComboFiles": [
	"hudplayerhealth-broesel.res",
	"hudplayerhealth-cross.res"
],
```

---

### Options

**Optional, ComboBox Only**. Lists all the options on the list. For information on how each option is defined, [see here][docs-options].

```json
...
"Type": "ComboBox",
"Value": "0",
"Options": [
	{
		"Label": "Flash",
		"Value": "0",
		...
```

---

### Pulse

**Optional, ColorPicker Only**. If true, the color will have a new entry in the client scheme with a reduced alpha.

```json
"Pulse": true
```

---

### Shadow

**Optional, ColorPicker Only**. If true, the color will have a new entry in the client scheme where each color channel is darkened by 40%.

```json
"Shadow": true
```

---

### Minimum

**Optional, IntegerUpDown Only**. Sets the minimum value that the integer counter can go down to.

```json
"Minimum": 10
```

---

### Maximum

**Optional, IntegerUpDown Only**. Sets the maximum value that the integer counter can go down to.

```json
"Maximum": 30
```

---

### Increment

**Optional, IntegerUpDown Only**. Sets the number by which the integer counter value will change.

```json
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
[docs-animations]: https://www.editor.criticalflaw.ca/json/animations/
[docs-backgrounds]: https://www.editor.criticalflaw.ca/json/backgrounds/