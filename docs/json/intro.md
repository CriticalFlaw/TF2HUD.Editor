Every HUD supported by the editor has a dedicated schema file that defines the page layout and instructions for each customization option. This section will act as a refference guide for the structure of said schema file, what control options are available and things to keep in mind as you're building the schema for your custom HUD.

!!! note
    Use this sample [schema file][json-sample] as a starting point. For refference, also see schemas for [budhud][json-budhud], [flawhud][json-flawhud] and [rayshud][json-rayshud].

## Table of Contents

1. [Main][docs-main] - Base settings like the HUD page layout, path of customization folders, links to download, GitHub, HUDS.TF and more.
2. [Controls][docs-controls] - Individual controls that will be displayed on the form and carry instructions for where and how to apply customizations.
3. [List Options][docs-options] - Options available for the user to choose from a list. Each option can have its own name and specific instructions.

![image](https://user-images.githubusercontent.com/6818236/116594733-8ad89800-a8f0-11eb-948a-84757dedc634.png)

<!-- MARKDOWN LINKS -->
[json-sample]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/docs/resources/sample.json
[json-budhud]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/src/TF2HUD.Editor/JSON/budhud.json
[json-flawhud]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/src/TF2HUD.Editor/JSON/flawhud.json
[json-rayshud]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/src/TF2HUD.Editor/JSON/rayshud.json
[docs-main]: https://www.editor.criticalflaw.ca/json/base
[docs-controls]: https://www.editor.criticalflaw.ca/json/controls
[docs-options]: https://www.editor.criticalflaw.ca/json/options