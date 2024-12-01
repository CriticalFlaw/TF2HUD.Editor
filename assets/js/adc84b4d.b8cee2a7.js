"use strict";(self.webpackChunk=self.webpackChunk||[]).push([[863],{5680:(e,n,o)=>{o.d(n,{xA:()=>s,yg:()=>m});var t=o(6540);function a(e,n,o){return n in e?Object.defineProperty(e,n,{value:o,enumerable:!0,configurable:!0,writable:!0}):e[n]=o,e}function r(e,n){var o=Object.keys(e);if(Object.getOwnPropertySymbols){var t=Object.getOwnPropertySymbols(e);n&&(t=t.filter((function(n){return Object.getOwnPropertyDescriptor(e,n).enumerable}))),o.push.apply(o,t)}return o}function l(e){for(var n=1;n<arguments.length;n++){var o=null!=arguments[n]?arguments[n]:{};n%2?r(Object(o),!0).forEach((function(n){a(e,n,o[n])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(o)):r(Object(o)).forEach((function(n){Object.defineProperty(e,n,Object.getOwnPropertyDescriptor(o,n))}))}return e}function c(e,n){if(null==e)return{};var o,t,a=function(e,n){if(null==e)return{};var o,t,a={},r=Object.keys(e);for(t=0;t<r.length;t++)o=r[t],n.indexOf(o)>=0||(a[o]=e[o]);return a}(e,n);if(Object.getOwnPropertySymbols){var r=Object.getOwnPropertySymbols(e);for(t=0;t<r.length;t++)o=r[t],n.indexOf(o)>=0||Object.prototype.propertyIsEnumerable.call(e,o)&&(a[o]=e[o])}return a}var i=t.createContext({}),u=function(e){var n=t.useContext(i),o=n;return e&&(o="function"==typeof e?e(n):l(l({},n),e)),o},s=function(e){var n=u(e.components);return t.createElement(i.Provider,{value:n},e.children)},d="mdxType",p={inlineCode:"code",wrapper:function(e){var n=e.children;return t.createElement(t.Fragment,{},n)}},g=t.forwardRef((function(e,n){var o=e.components,a=e.mdxType,r=e.originalType,i=e.parentName,s=c(e,["components","mdxType","originalType","parentName"]),d=u(o),g=a,m=d["".concat(i,".").concat(g)]||d[g]||p[g]||r;return o?t.createElement(m,l(l({ref:n},s),{},{components:o})):t.createElement(m,l({ref:n},s))}));function m(e,n){var o=arguments,a=n&&n.mdxType;if("string"==typeof e||a){var r=o.length,l=new Array(r);l[0]=g;var c={};for(var i in n)hasOwnProperty.call(n,i)&&(c[i]=n[i]);c.originalType=e,c[d]="string"==typeof e?e:a,l[1]=c;for(var u=2;u<r;u++)l[u]=o[u];return t.createElement.apply(null,l)}return t.createElement.apply(null,o)}g.displayName="MDXCreateElement"},5924:(e,n,o)=>{o.r(n),o.d(n,{assets:()=>s,contentTitle:()=>i,default:()=>m,frontMatter:()=>c,metadata:()=>u,toc:()=>d});var t=o(8168),a=o(8587),r=(o(6540),o(5680)),l=["components"],c={title:"Background"},i=void 0,u={unversionedId:"json/backgrounds",id:"json/backgrounds",title:"Background",description:"There are 3 special commands that control the management of backgrounds",source:"@site/docs/json/backgrounds.md",sourceDirName:"json",slug:"/json/backgrounds",permalink:"/TF2HUD.Editor/json/backgrounds",draft:!1,editUrl:"https://github.com/CriticalFlaw/TF2HUD.Editor/tree/master/docs/json/backgrounds.md",tags:[],version:"current",lastUpdatedAt:1733012517,formattedLastUpdatedAt:"Dec 1, 2024",frontMatter:{title:"Background"},sidebar:"jsonSideBar",previous:{title:"Animations",permalink:"/TF2HUD.Editor/json/animations"}},s={},d=[{value:"Custom Backgrounds",id:"custom-backgrounds",level:2},{value:"Priority",id:"priority",level:2},{value:"Example Code",id:"example-code",level:2},{value:"CheckBox",id:"checkbox",level:3},{value:"ComboBox",id:"combobox",level:3}],p={toc:d},g="wrapper";function m(e){var n=e.components,o=(0,a.A)(e,l);return(0,r.yg)(g,(0,t.A)({},p,o,{components:n,mdxType:"MDXLayout"}),(0,r.yg)("p",null,"There are 3 special commands that control the management of backgrounds"),(0,r.yg)("ul",null,(0,r.yg)("li",{parentName:"ul"},(0,r.yg)("inlineCode",{parentName:"li"},"StockBackgrounds")),(0,r.yg)("li",{parentName:"ul"},(0,r.yg)("inlineCode",{parentName:"li"},"HUDBackground")),(0,r.yg)("li",{parentName:"ul"},(0,r.yg)("inlineCode",{parentName:"li"},"CustomBackground"))),(0,r.yg)("p",null,"To switch to or inbetween different backgrounds included in your hud, use the special ",(0,r.yg)("inlineCode",{parentName:"p"},"HUDBackground")," property on a control and pass the name of the background you want to enable in the ",(0,r.yg)("inlineCode",{parentName:"p"},"SpecialParameters")," array, excluding the '_widescreen' suffix and .vtf file extension."),(0,r.yg)("p",null,"TF2HUD.Editor will always copy the accompanying _widescreen.vtf file when handling backgrounds."),(0,r.yg)("pre",null,(0,r.yg)("code",{parentName:"pre",className:"language-json"},'  {\n    "Special": "HUDBackground",\n    "SpecialParameters": [\n      "background_upward" // Will enable background_upward.vtf and background_upward_widescreen.vtf\n    ]\n  }\n')),(0,r.yg)("h2",{id:"custom-backgrounds"},"Custom Backgrounds"),(0,r.yg)("p",null,"To allow the user to set a custom background from a jpg or png, use the ",(0,r.yg)("inlineCode",{parentName:"p"},"CustomBackground")," control, along with the ",(0,r.yg)("inlineCode",{parentName:"p"},"CustomBackground")," special property"),(0,r.yg)("pre",null,(0,r.yg)("code",{parentName:"pre",className:"language-json"},'  {\n    "Type": "CustomBackground",\n    "Special": "CustomBackground"\n  }\n')),(0,r.yg)("admonition",{type:"caution"},(0,r.yg)("p",{parentName:"admonition"},"You must use the ",(0,r.yg)("inlineCode",{parentName:"p"},"CustomBackground")," type AND the ",(0,r.yg)("inlineCode",{parentName:"p"},"CustomBackground")," Special property for custom backgrounds to work properly")),(0,r.yg)("h2",{id:"priority"},"Priority"),(0,r.yg)("p",null,"If the HUD JSON schema includes a ",(0,r.yg)("inlineCode",{parentName:"p"},"CustomBackground")," control, the editor will use the background selected by the user, if the user does not have a custom background selected, it will use the HUD Background defined by the ",(0,r.yg)("inlineCode",{parentName:"p"},"HUDBackground")," or ",(0,r.yg)("inlineCode",{parentName:"p"},"StockBackgrounds")," special commands"),(0,r.yg)("h2",{id:"example-code"},"Example Code"),(0,r.yg)("h3",{id:"checkbox"},"CheckBox"),(0,r.yg)("p",null,"This control will toggle between using stock TF2 backgrounds and the default state of your HUD's ",(0,r.yg)("inlineCode",{parentName:"p"},"materials/console")," folder."),(0,r.yg)("pre",null,(0,r.yg)("code",{parentName:"pre",className:"language-json"},'  {\n    "Name": "my_hud_use_stock_backgrounds", // Change this to the name of your hud and provide a name for the control\n    "Label": "Use Stock Backgrounds",\n    "Type": "CheckBox",\n    "Value": "false", // Default to default state of materials/console folder\n    "Special:": "StockBackgrounds",\n  }\n')),(0,r.yg)("h3",{id:"combobox"},"ComboBox"),(0,r.yg)("p",null,"This sample creates a ComboBox control where the first two options enable a different custom background image, as well as an option to set default TF2 backgrounds."),(0,r.yg)("pre",null,(0,r.yg)("code",{parentName:"pre",className:"language-json"},'  {\n    "Name": "my_hud_background_selector_control", // Change this to the name of your hud and provide a name for the control\n    "Label": "Menu Background",\n    "Type": "ComboBox",\n    "ToolTip": "Change the style of background image shown on the main menu.",\n    "Value": "0", // The Default selected item\n    "Restart": true, // Tell TF2HUD.Editor that applying this customization requires restarting TF2\n    "Options": [\n      {\n        "Label": "Modern Background",\n        "Value": "0",\n        "Special":"HUDBackground",\n        "SpecialParameters": [\n          "background_modern"\n        ]\n      },\n      {\n        "Label": "Classic Background",\n        "Value": "1",\n        "Special": "HUDBackground",\n        "SpecialParameters": [\n        "background_classic"\n        ]\n      },\n      {\n        "Label": "Default Backgrounds",\n        "Value": "2",\n        "Special": "StockBackgrounds"\n      }\n    ]\n  }\n')))}m.isMDXComponent=!0}}]);