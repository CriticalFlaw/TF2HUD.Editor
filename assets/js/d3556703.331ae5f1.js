"use strict";(self.webpackChunk=self.webpackChunk||[]).push([[527],{3905:(e,t,n)=>{n.d(t,{Zo:()=>m,kt:()=>d});var a=n(7294);function l(e,t,n){return t in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function o(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var a=Object.getOwnPropertySymbols(e);t&&(a=a.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),n.push.apply(n,a)}return n}function r(e){for(var t=1;t<arguments.length;t++){var n=null!=arguments[t]?arguments[t]:{};t%2?o(Object(n),!0).forEach((function(t){l(e,t,n[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(n)):o(Object(n)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(n,t))}))}return e}function i(e,t){if(null==e)return{};var n,a,l=function(e,t){if(null==e)return{};var n,a,l={},o=Object.keys(e);for(a=0;a<o.length;a++)n=o[a],t.indexOf(n)>=0||(l[n]=e[n]);return l}(e,t);if(Object.getOwnPropertySymbols){var o=Object.getOwnPropertySymbols(e);for(a=0;a<o.length;a++)n=o[a],t.indexOf(n)>=0||Object.prototype.propertyIsEnumerable.call(e,n)&&(l[n]=e[n])}return l}var s=a.createContext({}),p=function(e){var t=a.useContext(s),n=t;return e&&(n="function"==typeof e?e(t):r(r({},t),e)),n},m=function(e){var t=p(e.components);return a.createElement(s.Provider,{value:t},e.children)},u="mdxType",c={inlineCode:"code",wrapper:function(e){var t=e.children;return a.createElement(a.Fragment,{},t)}},h=a.forwardRef((function(e,t){var n=e.components,l=e.mdxType,o=e.originalType,s=e.parentName,m=i(e,["components","mdxType","originalType","parentName"]),u=p(n),h=l,d=u["".concat(s,".").concat(h)]||u[h]||c[h]||o;return n?a.createElement(d,r(r({ref:t},m),{},{components:n})):a.createElement(d,r({ref:t},m))}));function d(e,t){var n=arguments,l=t&&t.mdxType;if("string"==typeof e||l){var o=n.length,r=new Array(o);r[0]=h;var i={};for(var s in t)hasOwnProperty.call(t,s)&&(i[s]=t[s]);i.originalType=e,i[u]="string"==typeof e?e:l,r[1]=i;for(var p=2;p<o;p++)r[p]=n[p];return a.createElement.apply(null,r)}return a.createElement.apply(null,n)}h.displayName="MDXCreateElement"},83:(e,t,n)=>{n.r(t),n.d(t,{assets:()=>m,contentTitle:()=>s,default:()=>d,frontMatter:()=>i,metadata:()=>p,toc:()=>u});var a=n(7462),l=n(3366),o=(n(7294),n(3905)),r=["components"],i={title:"User Controls"},s=void 0,p={unversionedId:"json/controls",id:"json/controls",title:"User Controls",description:"This section covers the controls that are displayed on the HUD page, grouped with other controls of similar purpose. This will include properties only available to specific types of controls.",source:"@site/docs/json/controls.md",sourceDirName:"json",slug:"/json/controls",permalink:"/TF2HUD.Editor/json/controls",draft:!1,editUrl:"https://github.com/CriticalFlaw/TF2HUD.Editor/tree/master/docs/json/controls.md",tags:[],version:"current",lastUpdatedAt:1699063106,formattedLastUpdatedAt:"Nov 4, 2023",frontMatter:{title:"User Controls"},sidebar:"jsonSideBar",previous:{title:"Main Settings",permalink:"/TF2HUD.Editor/json/base"},next:{title:"List Options",permalink:"/TF2HUD.Editor/json/options"}},m={},u=[{value:"Name",id:"name",level:3},{value:"Label",id:"label",level:3},{value:"Type",id:"type",level:3},{value:"Value",id:"value",level:3},{value:"Tooltip",id:"tooltip",level:3},{value:"Restart",id:"restart",level:3},{value:"Preview",id:"preview",level:3},{value:"Special",id:"special",level:3},{value:"SpecialParameters",id:"specialparameters",level:3},{value:"Files",id:"files",level:3},{value:"FileName",id:"filename",level:3},{value:"RenameFile",id:"renamefile",level:3},{value:"ComboFiles",id:"combofiles",level:3},{value:"Options",id:"options",level:3},{value:"Pulse",id:"pulse",level:3},{value:"Shadow",id:"shadow",level:3},{value:"Minimum",id:"minimum",level:3},{value:"Maximum",id:"maximum",level:3},{value:"Increment",id:"increment",level:3},{value:"Width",id:"width",level:3}],c={toc:u},h="wrapper";function d(e){var t=e.components,n=(0,l.Z)(e,r);return(0,o.kt)(h,(0,a.Z)({},c,n,{components:t,mdxType:"MDXLayout"}),(0,o.kt)("p",null,"This section covers the controls that are displayed on the HUD page, grouped with other controls of similar purpose. This will include properties only available to specific types of controls."),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre",className:"language-json"},'"Crosshair":\n[\n    {\n        "Name": "fh_toggle_xhair_enable",\n        "Label": "Toggle the Crosshair",\n        "Type": "Checkbox",\n        "ToolTip": "Toggle crosshair visibility.",\n        "Value": "false",\n        "Files": {\n            ...\n        }\n    },\n    {\n        "Name": "fh_toggle_xhair_pulse",\n        "Label": "Toggle the Hitmarker",\n        "Type": "Checkbox",\n        "ToolTip": "Toggle crosshair hitmarker.",\n        "Value": "true",\n        "Files": {\n            ...\n        }\n    },\n    {\n        "Name": "fh_val_xhair_style",\n        "Label": "Style",\n        "ToolTip": "Style of crosshair.",\n        "Type": "Crosshair",\n        "Value": "<",\n        "Options":: {\n            ...\n        }\n    },\n    {\n        "Name": "fh_val_xhair_size",\n        "Label": "Size",\n        "Type": "IntegerUpDown",\n        "Value": "18",\n        "Minimum": "10",\n        "Maximum": "30",\n        "Increment": "1",\n        "ToolTip": "Size of the crosshair.",\n        "Files": {\n            ...\n        }\n    },\n    {\n        "Name": "fh_color_xhair_normal",\n        "Label": "Crosshair",\n        "Type": "ColorPicker",\n        "ToolTip": "Default crosshair color.",\n        "Value": "242 242 242 255",\n        "Files": {\n            ...\n        }\n    },\n    {\n        "Name": "fh_color_xhair_pulse",\n        "Label": "Hitmarker",\n        "Type": "ColorPicker",\n        "ToolTip": "Color of crosshair when hitting another player.",\n        "Value": "255 0 0 255",\n        "Files":  {\n            ...\n        }\n    }\n]\n')),(0,o.kt)("h3",{id:"name"},"Name"),(0,o.kt)("p",null,(0,o.kt)("strong",{parentName:"p"},"Required"),". Name of the control. This name must be unique, have no spaces and suggest the control's purpose."),(0,o.kt)("admonition",{type:"note"},(0,o.kt)("p",{parentName:"admonition"},"To avoid conflicts, prefix each name with an abbreviation for the HUD. Example; budhud is bh, flawhud is fh.")),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre",className:"language-json"},'"Name": "fh_color_health_buff"\n')),(0,o.kt)("hr",null),(0,o.kt)("h3",{id:"label"},"Label"),(0,o.kt)("p",null,(0,o.kt)("strong",{parentName:"p"},"Required"),". Text displayed near the control. This space is limited, so save longer explanations for the ",(0,o.kt)("a",{parentName:"p",href:"http://criticalflaw.ca/TF2HUD.Editor/json/controls/#tooltip"},"Tooltip")," property."),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre",className:"language-json"},'"Label": "Buffed Health"\n')),(0,o.kt)("hr",null),(0,o.kt)("h3",{id:"type"},"Type"),(0,o.kt)("p",null,(0,o.kt)("strong",{parentName:"p"},"Required"),". Defines the type of control this will appear as on the page. Below are the supported types:"),(0,o.kt)("ul",null,(0,o.kt)("li",{parentName:"ul"},(0,o.kt)("strong",{parentName:"li"},"CheckBox")," - Toggling this will either enable or disablee the customization option attached to this control."),(0,o.kt)("li",{parentName:"ul"},(0,o.kt)("strong",{parentName:"li"},"ColorPicker")," - Opens a color picker for the user to select an RGBA color. Can also use ",(0,o.kt)("strong",{parentName:"li"},"Color"),", ",(0,o.kt)("strong",{parentName:"li"},"Colour")," or ",(0,o.kt)("strong",{parentName:"li"},"ColourPicker"),"."),(0,o.kt)("li",{parentName:"ul"},(0,o.kt)("strong",{parentName:"li"},"ComboBox")," - Contains a list of ",(0,o.kt)("a",{parentName:"li",href:"http://criticalflaw.ca/TF2HUD.Editor/json/controls/#options"},"options"),", each with their own customization instructions. Can also use ",(0,o.kt)("strong",{parentName:"li"},"DropDown"),", ",(0,o.kt)("strong",{parentName:"li"},"DropDownMenu")," or ",(0,o.kt)("strong",{parentName:"li"},"Select"),"."),(0,o.kt)("li",{parentName:"ul"},(0,o.kt)("strong",{parentName:"li"},"Number")," - An integer counter ranging between set minimum and maximum values. Commonly to be used for crosshair sizes and number of rows on the killfeed. Can also use ",(0,o.kt)("strong",{parentName:"li"},"Integer")," or ",(0,o.kt)("strong",{parentName:"li"},"IntegerUpDown"),"."),(0,o.kt)("li",{parentName:"ul"},(0,o.kt)("strong",{parentName:"li"},"Crosshair")," - Contains a list of styles from ",(0,o.kt)("a",{parentName:"li",href:"https://github.com/Hypnootize/TF2-Hud-Crosshairs"},"Hypnotize's Crosshair Pack")," that are applied to the HUD's ",(0,o.kt)("inlineCode",{parentName:"li"},"hudlayout.res")," file. Can also use ",(0,o.kt)("strong",{parentName:"li"},"CustomCrosshair"),"."),(0,o.kt)("li",{parentName:"ul"},(0,o.kt)("strong",{parentName:"li"},"Background")," - Provides the user with the option to select an image file to convert into VTF as a replacement for the HUD's background. Can also use ",(0,o.kt)("strong",{parentName:"li"},"CustomBackground"),"."),(0,o.kt)("li",{parentName:"ul"},(0,o.kt)("strong",{parentName:"li"},"TextBox")," - Text field contents of which will be used as the value for a property in a given hUD file. Can also use ",(0,o.kt)("strong",{parentName:"li"},"Text"),".")),(0,o.kt)("hr",null),(0,o.kt)("h3",{id:"value"},"Value"),(0,o.kt)("p",null,(0,o.kt)("strong",{parentName:"p"},"Required"),". Default value for the control, compatible with the selected control ",(0,o.kt)("a",{parentName:"p",href:"http://criticalflaw.ca/TF2HUD.Editor/json/controls/#type"},"type"),". Allowed values per type are listed below:"),(0,o.kt)("ul",null,(0,o.kt)("li",{parentName:"ul"},(0,o.kt)("strong",{parentName:"li"},"CheckBox")," - true, false."),(0,o.kt)("li",{parentName:"ul"},(0,o.kt)("strong",{parentName:"li"},"ColorPicker")," - RGBA color code, ",(0,o.kt)("strong",{parentName:"li"},"30 30 30 200"),"."),(0,o.kt)("li",{parentName:"ul"},(0,o.kt)("strong",{parentName:"li"},"ComboBox")," - Integer value of the option selected."),(0,o.kt)("li",{parentName:"ul"},(0,o.kt)("strong",{parentName:"li"},"IntegerUpDown")," - Integer value within the set range."),(0,o.kt)("li",{parentName:"ul"},(0,o.kt)("strong",{parentName:"li"},"Crosshair")," - Integer value of the option selected."),(0,o.kt)("li",{parentName:"ul"},(0,o.kt)("strong",{parentName:"li"},"Background")," - Not required."),(0,o.kt)("li",{parentName:"ul"},(0,o.kt)("strong",{parentName:"li"},"TextBox")," - Not required.")),(0,o.kt)("hr",null),(0,o.kt)("h3",{id:"tooltip"},"Tooltip"),(0,o.kt)("p",null,(0,o.kt)("strong",{parentName:"p"},"Optional"),". Text that shown when the user hovers their mouse over control."),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre",className:"language-json"},'"Tooltip": "Color of player\'s health, when buffed."\n')),(0,o.kt)("hr",null),(0,o.kt)("h3",{id:"restart"},"Restart"),(0,o.kt)("p",null,(0,o.kt)("strong",{parentName:"p"},"Optional"),". If true, the editor will tell the user that the game must be restarted for this customization to apply."),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre",className:"language-json"},'"Restart": false\n')),(0,o.kt)("hr",null),(0,o.kt)("h3",{id:"preview"},"Preview"),(0,o.kt)("p",null,(0,o.kt)("strong",{parentName:"p"},"Optional"),". Sets the image that previews the effect of this customization option. If a valid image is supplied, a question mark button will appear near the control that will open a modal with the linked image when pressed."),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre",className:"language-json"},'"Preview": "https://user-images.githubusercontent.com/6818236/114957712-9bd4d400-9e2f-11eb-8612-479313086c47.jpg",\n')),(0,o.kt)("hr",null),(0,o.kt)("h3",{id:"special"},"Special"),(0,o.kt)("p",null,(0,o.kt)("strong",{parentName:"p"},"Optional"),". Special case property for customizations that otherwise cannot be through the schema. For more information, see ",(0,o.kt)("a",{parentName:"p",href:"http://criticalflaw.ca/TF2HUD.Editor/json/special/"},"this section"),"."),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre",className:"language-json"},'"Special": "StockBackgrounds"\n')),(0,o.kt)("hr",null),(0,o.kt)("h3",{id:"specialparameters"},"SpecialParameters"),(0,o.kt)("p",null,(0,o.kt)("strong",{parentName:"p"},"Optional"),". This parameter is required when using the special command ",(0,o.kt)("inlineCode",{parentName:"p"},"HUDBackground"),", see ",(0,o.kt)("a",{parentName:"p",href:"http://criticalflaw.ca/TF2HUD.Editor/json/backgrounds/"},"Custom Backgrounds")),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre",className:"language-json"},'"SpecialParameters": []\n')),(0,o.kt)("hr",null),(0,o.kt)("h3",{id:"files"},"Files"),(0,o.kt)("p",null,(0,o.kt)("strong",{parentName:"p"},"Optional"),". Defines a list of files that will need to be updated if the given option is selected, where each file path is relative to the root of the HUD."),(0,o.kt)("p",null,"For in depth documentation on File editing, see ",(0,o.kt)("a",{parentName:"p",href:"http://criticalflaw.ca/TF2HUD.Editor/json/files/"},"this section"),"."),(0,o.kt)("admonition",{type:"caution"},(0,o.kt)("p",{parentName:"admonition"},"Each nested object within the file path has to match the contents of the HUD file, otherwise the editor will not be able to apply the changes.")),(0,o.kt)("hr",null),(0,o.kt)("h3",{id:"filename"},"FileName"),(0,o.kt)("p",null,(0,o.kt)("strong",{parentName:"p"},"Optional"),". Name of the file or folder that will be moved from ",(0,o.kt)("inlineCode",{parentName:"p"},"CustomizationsFolder")," to ",(0,o.kt)("inlineCode",{parentName:"p"},"EnabledFolder")," if this option is selected."),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre",className:"language-json"},'"FileName": "hudplayerhealth-broesel.res"\n')),(0,o.kt)("hr",null),(0,o.kt)("h3",{id:"renamefile"},"RenameFile"),(0,o.kt)("p",null,(0,o.kt)("strong",{parentName:"p"},"Optional"),". Name of a file or folder that will be renamed or moved based on the value of the associated control. This property can be useful for performing a large number of customizations that are already implemented using folder based customization. Folder renames or moves should end with a ",(0,o.kt)("inlineCode",{parentName:"p"},"/"),"."),(0,o.kt)("p",null,"Only applies to:"),(0,o.kt)("ul",null,(0,o.kt)("li",{parentName:"ul"},(0,o.kt)("inlineCode",{parentName:"li"},"CheckBox")),(0,o.kt)("li",{parentName:"ul"},(0,o.kt)("inlineCode",{parentName:"li"},"ComboBox"))),(0,o.kt)("p",null,(0,o.kt)("strong",{parentName:"p"},"CheckBox:")),(0,o.kt)("p",null,"If the CheckBox is checked, the ",(0,o.kt)("inlineCode",{parentName:"p"},"example-customization")," folder will be moved into the ",(0,o.kt)("inlineCode",{parentName:"p"},"enabled")," folder, else it will be moved out."),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre",className:"language-json"},'"Label": "Enable complicated customization",\n"Type": "CheckBox",\n"RenameFile": {\n    "OldName": "customizations/example-customization/",\n    "NewName": "customizations/enabled/example-customization/"\n}\n')),(0,o.kt)("p",null,(0,o.kt)("strong",{parentName:"p"},"ComboBox:")),(0,o.kt)("p",null,"Only the selected ComboBox value RenameFile.NewName will be enabled, other options will be renamed or moved back to the RenameFile.OldName."),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre",className:"language-json"},'"Label": "Resolution",\n"Type": "ComboBox",\n"Options": [\n    {\n        "Label": "4x3",\n        "Value": "0",\n        "RenameFile": {\n            "OldName": "customizations/4x3-customization/",\n            "NewName": "customizations/enabled/4x3-customization/"\n        }\n    },\n    {\n        "Label": "16x9",\n        "Value": "1",\n        "RenameFile": {\n            "OldName": "customizations/16x9-customization/",\n            "NewName": "customizations/enabled/16x9-customization/"\n        }\n    }\n]\n\n')),(0,o.kt)("hr",null),(0,o.kt)("h3",{id:"combofiles"},"ComboFiles"),(0,o.kt)("p",null,(0,o.kt)("strong",{parentName:"p"},"Optional, ComboBox Only"),". Lists all the files that will be handled by this control, this is used for returning everything back to normal if the user does not make a selection."),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre",className:"language-json"},'"ComboFiles": [\n    "hudplayerhealth-broesel.res",\n    "hudplayerhealth-cross.res"\n],\n')),(0,o.kt)("hr",null),(0,o.kt)("h3",{id:"options"},"Options"),(0,o.kt)("p",null,(0,o.kt)("strong",{parentName:"p"},"Optional, ComboBox Only"),". Lists all the options on the list. For information on how each option is defined, ",(0,o.kt)("a",{parentName:"p",href:"http://criticalflaw.ca/TF2HUD.Editor/json/options/"},"see here"),"."),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre",className:"language-json"},'...\n"Type": "ComboBox",\n"Value": "0",\n"Options": [\n    {\n        "Label": "Flash",\n        "Value": "0",\n        ...\n')),(0,o.kt)("hr",null),(0,o.kt)("h3",{id:"pulse"},"Pulse"),(0,o.kt)("p",null,(0,o.kt)("strong",{parentName:"p"},"Optional, ColorPicker Only"),". If true, the color will have a new entry in the client scheme with a reduced alpha."),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre",className:"language-json"},'"Pulse": true\n')),(0,o.kt)("hr",null),(0,o.kt)("h3",{id:"shadow"},"Shadow"),(0,o.kt)("p",null,(0,o.kt)("strong",{parentName:"p"},"Optional, ColorPicker Only"),". If true, the color will have a new entry in the client scheme where each color channel is darkened by 40%."),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre",className:"language-json"},'"Shadow": true\n')),(0,o.kt)("hr",null),(0,o.kt)("h3",{id:"minimum"},"Minimum"),(0,o.kt)("p",null,(0,o.kt)("strong",{parentName:"p"},"Optional, IntegerUpDown Only"),". Sets the minimum value that the integer counter can go down to."),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre",className:"language-json"},'"Minimum": 10\n')),(0,o.kt)("hr",null),(0,o.kt)("h3",{id:"maximum"},"Maximum"),(0,o.kt)("p",null,(0,o.kt)("strong",{parentName:"p"},"Optional, IntegerUpDown Only"),". Sets the maximum value that the integer counter can go down to."),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre",className:"language-json"},'"Maximum": 30\n')),(0,o.kt)("hr",null),(0,o.kt)("h3",{id:"increment"},"Increment"),(0,o.kt)("p",null,(0,o.kt)("strong",{parentName:"p"},"Optional, IntegerUpDown Only"),". Sets the number by which the integer counter value will change."),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre",className:"language-json"},'"Increment": 2\n')),(0,o.kt)("hr",null),(0,o.kt)("h3",{id:"width"},"Width"),(0,o.kt)("p",null,(0,o.kt)("strong",{parentName:"p"},"Optional"),". Override the width of the control with a different value. Default width of any given control is varied."),(0,o.kt)("pre",null,(0,o.kt)("code",{parentName:"pre",className:"language-json"},'"Width": 200\n')))}d.isMDXComponent=!0}}]);