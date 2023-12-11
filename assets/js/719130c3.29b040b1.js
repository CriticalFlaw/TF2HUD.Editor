"use strict";(self.webpackChunk=self.webpackChunk||[]).push([[636],{3905:(e,n,t)=>{t.d(n,{Zo:()=>m,kt:()=>f});var a=t(7294);function r(e,n,t){return n in e?Object.defineProperty(e,n,{value:t,enumerable:!0,configurable:!0,writable:!0}):e[n]=t,e}function i(e,n){var t=Object.keys(e);if(Object.getOwnPropertySymbols){var a=Object.getOwnPropertySymbols(e);n&&(a=a.filter((function(n){return Object.getOwnPropertyDescriptor(e,n).enumerable}))),t.push.apply(t,a)}return t}function o(e){for(var n=1;n<arguments.length;n++){var t=null!=arguments[n]?arguments[n]:{};n%2?i(Object(t),!0).forEach((function(n){r(e,n,t[n])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(t)):i(Object(t)).forEach((function(n){Object.defineProperty(e,n,Object.getOwnPropertyDescriptor(t,n))}))}return e}function l(e,n){if(null==e)return{};var t,a,r=function(e,n){if(null==e)return{};var t,a,r={},i=Object.keys(e);for(a=0;a<i.length;a++)t=i[a],n.indexOf(t)>=0||(r[t]=e[t]);return r}(e,n);if(Object.getOwnPropertySymbols){var i=Object.getOwnPropertySymbols(e);for(a=0;a<i.length;a++)t=i[a],n.indexOf(t)>=0||Object.prototype.propertyIsEnumerable.call(e,t)&&(r[t]=e[t])}return r}var p=a.createContext({}),s=function(e){var n=a.useContext(p),t=n;return e&&(t="function"==typeof e?e(n):o(o({},n),e)),t},m=function(e){var n=s(e.components);return a.createElement(p.Provider,{value:n},e.children)},u="mdxType",c={inlineCode:"code",wrapper:function(e){var n=e.children;return a.createElement(a.Fragment,{},n)}},d=a.forwardRef((function(e,n){var t=e.components,r=e.mdxType,i=e.originalType,p=e.parentName,m=l(e,["components","mdxType","originalType","parentName"]),u=s(t),d=r,f=u["".concat(p,".").concat(d)]||u[d]||c[d]||i;return t?a.createElement(f,o(o({ref:n},m),{},{components:t})):a.createElement(f,o({ref:n},m))}));function f(e,n){var t=arguments,r=n&&n.mdxType;if("string"==typeof e||r){var i=t.length,o=new Array(i);o[0]=d;var l={};for(var p in n)hasOwnProperty.call(n,p)&&(l[p]=n[p]);l.originalType=e,l[u]="string"==typeof e?e:r,o[1]=l;for(var s=2;s<i;s++)o[s]=t[s];return a.createElement.apply(null,o)}return a.createElement.apply(null,t)}d.displayName="MDXCreateElement"},4610:(e,n,t)=>{t.r(n),t.d(n,{assets:()=>m,contentTitle:()=>p,default:()=>f,frontMatter:()=>l,metadata:()=>s,toc:()=>u});var a=t(7462),r=t(3366),i=(t(7294),t(3905)),o=["components"],l={title:"Animations"},p=void 0,s={unversionedId:"json/animations",id:"json/animations",title:"Animations",description:"A definition of a HUD animations file contains event names and values, where each value contains a list of HUD animation objects.",source:"@site/docs/json/animations.md",sourceDirName:"json",slug:"/json/animations",permalink:"/TF2HUD.Editor/json/animations",draft:!1,editUrl:"https://github.com/CriticalFlaw/TF2HUD.Editor/tree/master/docs/json/animations.md",tags:[],version:"current",lastUpdatedAt:1702311825,formattedLastUpdatedAt:"Dec 11, 2023",frontMatter:{title:"Animations"},sidebar:"jsonSideBar",previous:{title:"HUD Files",permalink:"/TF2HUD.Editor/json/files"},next:{title:"Background",permalink:"/TF2HUD.Editor/json/backgrounds"}},m={},u=[{value:"Interpolator Parameters",id:"interpolator-parameters",level:2},{value:"Example HUD Animation",id:"example-hud-animation",level:2}],c={toc:u},d="wrapper";function f(e){var n=e.components,t=(0,r.Z)(e,o);return(0,i.kt)(d,(0,a.Z)({},c,t,{components:n,mdxType:"MDXLayout"}),(0,i.kt)("p",null,"A definition of a HUD animations file contains event names and values, where each value contains a list of HUD animation objects."),(0,i.kt)("p",null,"The main properties for writing a standard HUD animation are:"),(0,i.kt)("ul",null,(0,i.kt)("li",{parentName:"ul"},(0,i.kt)("inlineCode",{parentName:"li"},"Type")),(0,i.kt)("li",{parentName:"ul"},(0,i.kt)("inlineCode",{parentName:"li"},"Element")),(0,i.kt)("li",{parentName:"ul"},(0,i.kt)("inlineCode",{parentName:"li"},"Property")),(0,i.kt)("li",{parentName:"ul"},(0,i.kt)("inlineCode",{parentName:"li"},"Value")),(0,i.kt)("li",{parentName:"ul"},(0,i.kt)("inlineCode",{parentName:"li"},"Interpolator")),(0,i.kt)("li",{parentName:"ul"},(0,i.kt)("inlineCode",{parentName:"li"},"Delay")),(0,i.kt)("li",{parentName:"ul"},(0,i.kt)("inlineCode",{parentName:"li"},"Duration"))),(0,i.kt)("admonition",{type:"caution"},(0,i.kt)("p",{parentName:"admonition"},"Unlike HUD files, animation events are not merged, and each time an event is written, it overwrites the other occurences of that event (in the current file). This is to prevent animations from breaking.")),(0,i.kt)("h2",{id:"interpolator-parameters"},"Interpolator Parameters"),(0,i.kt)("p",null,"The frequency parameter of a HUD animation with the interpolator ",(0,i.kt)("inlineCode",{parentName:"p"},"Pulse")," can be passed via the ",(0,i.kt)("inlineCode",{parentName:"p"},"Frequency")," property"),(0,i.kt)("p",null,"The bias parameter of a HUD animation with the interpolators ",(0,i.kt)("inlineCode",{parentName:"p"},"Gain")," and ",(0,i.kt)("inlineCode",{parentName:"p"},"Bias")," can be passed via the ",(0,i.kt)("inlineCode",{parentName:"p"},"Bias")," property"),(0,i.kt)("p",null,"The randomness parameter of a HUD animation with the interpolator ",(0,i.kt)("inlineCode",{parentName:"p"},"Flicker")," can be passed via the ",(0,i.kt)("inlineCode",{parentName:"p"},"Randomness")," property"),(0,i.kt)("h2",{id:"example-hud-animation"},"Example HUD Animation"),(0,i.kt)("pre",null,(0,i.kt)("code",{parentName:"pre",className:"language-json"},'"Files": {\n  "scripts/hudanimations_examplehud.txt": {\n    "HudHealthBonusPulse": [\n      {\n        // Animate  PlayerStatusHealthBonusImage    Alpha       255     Linear 0.0 0.2\n        "Type": "Animate",\n        "Element": "PlayerStatusHealthBonusImage",\n        "Property": "Alpha",\n        "Value": "255",\n        "Interpolator": "Linear",\n        "Delay": "0.0",\n        "Duration": "0.2"\n      },\n      {\n        // Animate  PlayerStatusHealthBonusImage    Alpha       0           Linear 0.2 0.4\n        "Type": "Animate",\n        "Element": "PlayerStatusHealthBonusImage",\n        "Property": "Alpha",\n        "Value": "0",\n        "Interpolator": "Linear",\n        "Delay": "0.2",\n        "Duration": "0.4"\n      },\n      {\n        // RunEvent HudHealthBonusPulseLoop 0.4\n        "Type": "RunEvent",\n        "Event": "HudHealthBonusPulseLoop",\n        "Delay": "0.4"\n      }\n    ]\n  }\n}\n')))}f.isMDXComponent=!0}}]);