"use strict";(self.webpackChunk=self.webpackChunk||[]).push([[736],{3905:(e,t,n)=>{n.d(t,{Zo:()=>c,kt:()=>h});var a=n(7294);function o(e,t,n){return t in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function r(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var a=Object.getOwnPropertySymbols(e);t&&(a=a.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),n.push.apply(n,a)}return n}function i(e){for(var t=1;t<arguments.length;t++){var n=null!=arguments[t]?arguments[t]:{};t%2?r(Object(n),!0).forEach((function(t){o(e,t,n[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(n)):r(Object(n)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(n,t))}))}return e}function l(e,t){if(null==e)return{};var n,a,o=function(e,t){if(null==e)return{};var n,a,o={},r=Object.keys(e);for(a=0;a<r.length;a++)n=r[a],t.indexOf(n)>=0||(o[n]=e[n]);return o}(e,t);if(Object.getOwnPropertySymbols){var r=Object.getOwnPropertySymbols(e);for(a=0;a<r.length;a++)n=r[a],t.indexOf(n)>=0||Object.prototype.propertyIsEnumerable.call(e,n)&&(o[n]=e[n])}return o}var s=a.createContext({}),p=function(e){var t=a.useContext(s),n=t;return e&&(n="function"==typeof e?e(t):i(i({},t),e)),n},c=function(e){var t=p(e.components);return a.createElement(s.Provider,{value:t},e.children)},u="mdxType",m={inlineCode:"code",wrapper:function(e){var t=e.children;return a.createElement(a.Fragment,{},t)}},d=a.forwardRef((function(e,t){var n=e.components,o=e.mdxType,r=e.originalType,s=e.parentName,c=l(e,["components","mdxType","originalType","parentName"]),u=p(n),d=o,h=u["".concat(s,".").concat(d)]||u[d]||m[d]||r;return n?a.createElement(h,i(i({ref:t},c),{},{components:n})):a.createElement(h,i({ref:t},c))}));function h(e,t){var n=arguments,o=t&&t.mdxType;if("string"==typeof e||o){var r=n.length,i=new Array(r);i[0]=d;var l={};for(var s in t)hasOwnProperty.call(t,s)&&(l[s]=t[s]);l.originalType=e,l[u]="string"==typeof e?e:o,i[1]=l;for(var p=2;p<r;p++)i[p]=n[p];return a.createElement.apply(null,i)}return a.createElement.apply(null,n)}d.displayName="MDXCreateElement"},2473:(e,t,n)=>{n.r(t),n.d(t,{assets:()=>c,contentTitle:()=>s,default:()=>h,frontMatter:()=>l,metadata:()=>p,toc:()=>u});var a=n(7462),o=n(3366),r=(n(7294),n(3905)),i=["components"],l={title:"List Options"},s=void 0,p={unversionedId:"json/options",id:"json/options",title:"List Options",description:"This section covers individual options available in a list-type controls like DropDown, DropDownMenu or Select. Below is an example of a list control with options for enabling specific animations based on the option selected.",source:"@site/docs/json/options.md",sourceDirName:"json",slug:"/json/options",permalink:"/TF2HUD.Editor/json/options",draft:!1,editUrl:"https://github.com/CriticalFlaw/TF2HUD.Editor/tree/master/docs/json/options.md",tags:[],version:"current",lastUpdatedAt:1699063106,formattedLastUpdatedAt:"Nov 4, 2023",frontMatter:{title:"List Options"},sidebar:"jsonSideBar",previous:{title:"User Controls",permalink:"/TF2HUD.Editor/json/controls"},next:{title:"HUD Files",permalink:"/TF2HUD.Editor/json/files"}},c={},u=[{value:"FileName",id:"filename",level:3},{value:"Files",id:"files",level:3},{value:"Label",id:"label",level:3},{value:"RenameFile",id:"renamefile",level:3},{value:"Value",id:"value",level:3},{value:"Special",id:"special",level:3},{value:"SpecialParameters",id:"specialparameters",level:3}],m={toc:u},d="wrapper";function h(e){var t=e.components,n=(0,o.Z)(e,i);return(0,r.kt)(d,(0,a.Z)({},m,n,{components:t,mdxType:"MDXLayout"}),(0,r.kt)("p",null,"This section covers individual options available in a list-type controls like ",(0,r.kt)("inlineCode",{parentName:"p"},"DropDown"),", ",(0,r.kt)("inlineCode",{parentName:"p"},"DropDownMenu")," or ",(0,r.kt)("inlineCode",{parentName:"p"},"Select"),". Below is an example of a list control with options for enabling specific animations based on the option selected."),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-json"},'"Name": "rh_val_uber_animation",\n"Label": "Uber Style",\n"Type": "ComboBox",\n"Value": "0",\n"Options": [\n    {\n        "Label": "Flash",\n        "Value": "0",\n        "Files": {\n            "scripts/hudanimations_custom.txt": {\n                "comment": [\n                    "RunEvent HudMedicSolidColorCharge",\n                    "RunEvent HudMedicRainbowCharged"\n                ],\n                "uncomment": [\n                    "RunEvent HudMedicOrangePulseCharge"\n                ]\n            }\n        }\n    },\n    {\n        "Label": "Solid",\n        "Value": "1",\n        "Files": {\n            "scripts/hudanimations_custom.txt": {\n                "comment": [\n                    "RunEvent HudMedicOrangePulseCharge",\n                    "RunEvent HudMedicRainbowCharged"\n                ],\n                "uncomment": [\n                    "RunEvent HudMedicSolidColorCharge"\n                ]\n            }\n        }\n    }\n]\n')),(0,r.kt)("h3",{id:"filename"},"FileName"),(0,r.kt)("p",null,(0,r.kt)("strong",{parentName:"p"},"Optional"),". Name of the file or folder that will be moved from ",(0,r.kt)("inlineCode",{parentName:"p"},"CustomizationsFolder")," to ",(0,r.kt)("inlineCode",{parentName:"p"},"EnabledFolder")," if this option is selected."),(0,r.kt)("admonition",{type:"caution"},(0,r.kt)("p",{parentName:"admonition"},"Do not use this property in conjuction with ",(0,r.kt)("strong",{parentName:"p"},"Files")," or ",(0,r.kt)("strong",{parentName:"p"},"Special"),". Only use one of the three separately.")),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-json"},'"FileName": "hudplayerhealth-broesel.res"\n')),(0,r.kt)("hr",null),(0,r.kt)("h3",{id:"files"},"Files"),(0,r.kt)("p",null,(0,r.kt)("strong",{parentName:"p"},"Optional"),". Defines a list of files that will need to be updated if the given option is selected, where each file path is relative to the root of the HUD."),(0,r.kt)("admonition",{type:"caution"},(0,r.kt)("p",{parentName:"admonition"},"Each nested object within the file path has to match the contents of the HUD file, otherwise the editor will not be able to apply the changes.")),(0,r.kt)("p",null,"For in depth documentation on File editing, see ",(0,r.kt)("a",{parentName:"p",href:"http://criticalflaw.ca/TF2HUD.Editor/json/files/"},"this section"),"."),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-json"},'"Files": {\n    "resource/ui/huditemeffectmeter.res": {\n        "HudItemEffectMeter": {\n            "xpos": "c-60",\n            "ypos": "c120"\n        },\n        "ItemEffectMeterLabel": {\n            "wide": "120"\n        }\n    },\n    "resource/ui/huddemomancharge.res": {\n        "ChargeMeter": {\n            "ypos": "c110"\n        }\n    },\n    ...\n}\n')),(0,r.kt)("hr",null),(0,r.kt)("h3",{id:"label"},"Label"),(0,r.kt)("p",null,(0,r.kt)("strong",{parentName:"p"},"Required"),". Sets the name of the option as it will be shown on screen."),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-json"},'"Label": "Broesel"\n')),(0,r.kt)("hr",null),(0,r.kt)("h3",{id:"renamefile"},"RenameFile"),(0,r.kt)("p",null,(0,r.kt)("strong",{parentName:"p"},"Optional"),". Defines the name of the file ",(0,r.kt)("inlineCode",{parentName:"p"},"OldName")," that will be renamed to ",(0,r.kt)("inlineCode",{parentName:"p"},"NewName")," when this option is selected. Revert the file name back to ",(0,r.kt)("inlineCode",{parentName:"p"},"OldName")," when this option is deselected."),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-json"},'"RenameFile": {\n    "OldName": "#users/dane_/",\n    "NewName": "#users/dane/"\n}\n')),(0,r.kt)("hr",null),(0,r.kt)("h3",{id:"value"},"Value"),(0,r.kt)("p",null,(0,r.kt)("strong",{parentName:"p"},"Required"),". Sets the underlying value for this option that will be used by the editor."),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-json"},'"Value": "1"\n')),(0,r.kt)("hr",null),(0,r.kt)("h3",{id:"special"},"Special"),(0,r.kt)("p",null,(0,r.kt)("strong",{parentName:"p"},"Optional"),". Special case property for customizations that otherwise cannot be through the schema. For more information, see ",(0,r.kt)("a",{parentName:"p",href:"http://criticalflaw.ca/TF2HUD.Editor/json/special/"},"this section"),"."),(0,r.kt)("admonition",{type:"caution"},(0,r.kt)("p",{parentName:"admonition"},"Do not use this property in conjuction with ",(0,r.kt)("strong",{parentName:"p"},"Files")," or ",(0,r.kt)("strong",{parentName:"p"},"FileName"),". Only use one of the three separately.")),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-json"},'"Special": "StockBackgrounds"\n')),(0,r.kt)("hr",null),(0,r.kt)("h3",{id:"specialparameters"},"SpecialParameters"),(0,r.kt)("p",null,(0,r.kt)("strong",{parentName:"p"},"Optional"),"."),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-json"},'"SpecialParameters": []\n')))}h.isMDXComponent=!0}}]);