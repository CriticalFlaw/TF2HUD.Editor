The `Files` property defines a list of instructions made up of HUD elements and values to apply to the HUD.

The files property can contain 0 or more file paths relative to the root of the HUD. Each file path can be seperated by slash (/), backslash (\\) or double backslash (\\\\).

!!! note
    Files that have the extensions `.res`, `.vmt` and `.vdf` are treated as HUD files, files that have the `.txt` extension are treated as HUD Animations files (see [HUD Animations][docs-animations]).

If the file does not exist, TF2HUD.Editor will create it with the values specified. If it does, the editor will merge the values specified with the already existing HUD values.

For HUD files that have a header element that matches their file name (such as `"Resource/UI/HudMedicCharge.res"`), the editor will apply the values specified inside the header element, for other files (such as clientscheme files), the object will need to specify the absolute desired location of the value.

Containing header element:

```json
"Files": {
  "resource/ui/hudplayerhealth.res": {
    // "Resource/UI/HudPlayerHealth.res" is not present
    "PlayerStatusHealthValue": {
        "fgcolor": "$value"
    }
  }
}
```

No containing header element:

```json
  "Files": {
    "resource/clientscheme.res": {
      "Scheme": {
        "Colors": {
            "Health Color": "$value"
        }
      }
    }
  }
```


Note that the following example is **NOT** correct:

```json
  "Files": {
    "resource/ui/hudplayerhealth.res": {
      // "Resource/UI/HudPlayerHealth.res" is present, but will be inside itself!
      "Resource/UI/HudPlayerHealth.res": {
        "PlayerStatusHealthValue": {
          "fgcolor": "$value"
        }
      }
    }
  }
```

## Special Keys

Special keys can appear anywhere within a file entry in the `Files` object, however they are performed before the HUD properties are written to the file and will not appear inside the HUD file.

Special Keys also do not care about the structure of the HUD elements, and will overwrite instances of their instructions anywhere.

#### Replace

The `Replace` special key is for use with the CheckBox control (see [Controls][docs-controls]). It takes a list that contains 2 strings of text and replaces raw text in the file based on the value of the CheckBox

If the checkbox is checked, the editor will replace all occurences of the first item in the list with the second item. if the CheckBox is unchecked, the editor will replace all occurences of the second item in the list with the first item.

```json
{
  ...
  "replace": [
    "Red",
    "Green"
  ]
  ...
}
```

Always ensure you `replace` usage is as greedy as possible, for example the following code will leak text and break the HUD:

```json
{
  ...
  "replace": [
    "HUD_Font_",
    "HUD_Font_Lato_"
  ]
  ...
}
```

After being run multiple times, this code will result in `HUD_Font_Lato_Lato_Lato_Lato_`

!!! warning
    It is not recommended to write VDF in the parameters of a special key, as the formatting of the HUD will change when the editor writed the specified properties

## HUD Element Keywords

Unlike Special Keys, HUD element keywords work within the structure of a HUD file.

#### True/False

The true/false object will evaluate the value of the CheckBox control and return the value that matches the setting of the CheckBox control,

```json
{
  "Label": "Enable Custom Crosshair",
  ...
  "Files": {
  "scripts/hudlayout.res": {
    "Crosshair": {
      "visible": {
        "true": "1",
        "false": "0"
      }
    }
  }
}

```

## Operating System Tags

Operating System Tags can be represents by putting a `^` and then the tag in the property name

```json
  ...
  "xpos": "10",
  "xpos^[$WIN32]": "20"
  ...
```

After being written to the HUD, this will be represented as:

```
"xpos"    "10"
"xpos"    "20" [$WIN32]
```

<!-- MARKDOWN LINKS -->
[docs-controls]: https://www.editor.criticalflaw.ca/json/controls/