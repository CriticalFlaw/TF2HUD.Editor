"use strict";(self.webpackChunk=self.webpackChunk||[]).push([[438],{3331:(e,n,t)=>{t.d(n,{R:()=>r,x:()=>s});var a=t(8101);const i={},o=a.createContext(i);function r(e){const n=a.useContext(o);return a.useMemo((function(){return"function"==typeof e?e(n):{...n,...e}}),[n,e])}function s(e){let n;return n=e.disableParentContext?"function"==typeof e.components?e.components(i):e.components||i:r(e.components),a.createElement(o.Provider,{value:n},e.children)}},8385:(e,n,t)=>{t.r(n),t.d(n,{assets:()=>l,contentTitle:()=>s,default:()=>h,frontMatter:()=>r,metadata:()=>a,toc:()=>c});const a=JSON.parse('{"id":"json/animations","title":"Animations","description":"A definition of a HUD animations file contains event names and values, where each value contains a list of HUD animation objects.","source":"@site/docs/json/animations.md","sourceDirName":"json","slug":"/json/animations","permalink":"/TF2HUD.Editor/json/animations","draft":false,"unlisted":false,"editUrl":"https://github.com/CriticalFlaw/TF2HUD.Editor/tree/master/docs/json/animations.md","tags":[],"version":"current","lastUpdatedAt":1742562077000,"frontMatter":{"title":"Animations"},"sidebar":"jsonSideBar","previous":{"title":"HUD Files","permalink":"/TF2HUD.Editor/json/files"},"next":{"title":"Background","permalink":"/TF2HUD.Editor/json/backgrounds"}}');var i=t(5105),o=t(3331);const r={title:"Animations"},s=void 0,l={},c=[{value:"Interpolator Parameters",id:"interpolator-parameters",level:2},{value:"Example HUD Animation",id:"example-hud-animation",level:2}];function d(e){const n={admonition:"admonition",code:"code",h2:"h2",li:"li",p:"p",pre:"pre",ul:"ul",...(0,o.R)(),...e.components};return(0,i.jsxs)(i.Fragment,{children:[(0,i.jsx)(n.p,{children:"A definition of a HUD animations file contains event names and values, where each value contains a list of HUD animation objects."}),"\n",(0,i.jsx)(n.p,{children:"The main properties for writing a standard HUD animation are:"}),"\n",(0,i.jsxs)(n.ul,{children:["\n",(0,i.jsx)(n.li,{children:(0,i.jsx)(n.code,{children:"Type"})}),"\n",(0,i.jsx)(n.li,{children:(0,i.jsx)(n.code,{children:"Element"})}),"\n",(0,i.jsx)(n.li,{children:(0,i.jsx)(n.code,{children:"Property"})}),"\n",(0,i.jsx)(n.li,{children:(0,i.jsx)(n.code,{children:"Value"})}),"\n",(0,i.jsx)(n.li,{children:(0,i.jsx)(n.code,{children:"Interpolator"})}),"\n",(0,i.jsx)(n.li,{children:(0,i.jsx)(n.code,{children:"Delay"})}),"\n",(0,i.jsx)(n.li,{children:(0,i.jsx)(n.code,{children:"Duration"})}),"\n"]}),"\n",(0,i.jsx)(n.admonition,{type:"caution",children:(0,i.jsx)(n.p,{children:"Unlike HUD files, animation events are not merged, and each time an event is written, it overwrites the other occurences of that event (in the current file). This is to prevent animations from breaking."})}),"\n",(0,i.jsx)(n.h2,{id:"interpolator-parameters",children:"Interpolator Parameters"}),"\n",(0,i.jsxs)(n.p,{children:["The frequency parameter of a HUD animation with the interpolator ",(0,i.jsx)(n.code,{children:"Pulse"})," can be passed via the ",(0,i.jsx)(n.code,{children:"Frequency"})," property"]}),"\n",(0,i.jsxs)(n.p,{children:["The bias parameter of a HUD animation with the interpolators ",(0,i.jsx)(n.code,{children:"Gain"})," and ",(0,i.jsx)(n.code,{children:"Bias"})," can be passed via the ",(0,i.jsx)(n.code,{children:"Bias"})," property"]}),"\n",(0,i.jsxs)(n.p,{children:["The randomness parameter of a HUD animation with the interpolator ",(0,i.jsx)(n.code,{children:"Flicker"})," can be passed via the ",(0,i.jsx)(n.code,{children:"Randomness"})," property"]}),"\n",(0,i.jsx)(n.h2,{id:"example-hud-animation",children:"Example HUD Animation"}),"\n",(0,i.jsx)(n.pre,{children:(0,i.jsx)(n.code,{className:"language-json",children:'"Files": {\n  "scripts/hudanimations_examplehud.txt": {\n    "HudHealthBonusPulse": [\n      {\n        // Animate\tPlayerStatusHealthBonusImage \tAlpha\t\t255\t\tLinear 0.0 0.2\n        "Type": "Animate",\n        "Element": "PlayerStatusHealthBonusImage",\n        "Property": "Alpha",\n        "Value": "255",\n        "Interpolator": "Linear",\n        "Delay": "0.0",\n        "Duration": "0.2"\n      },\n      {\n        // Animate\tPlayerStatusHealthBonusImage \tAlpha\t\t0\t\t\tLinear 0.2 0.4\n        "Type": "Animate",\n        "Element": "PlayerStatusHealthBonusImage",\n        "Property": "Alpha",\n        "Value": "0",\n        "Interpolator": "Linear",\n        "Delay": "0.2",\n        "Duration": "0.4"\n      },\n      {\n        // RunEvent HudHealthBonusPulseLoop\t0.4\n        "Type": "RunEvent",\n        "Event": "HudHealthBonusPulseLoop",\n        "Delay": "0.4"\n      }\n    ]\n  }\n}\n'})})]})}function h(e={}){const{wrapper:n}={...(0,o.R)(),...e.components};return n?(0,i.jsx)(n,{...e,children:(0,i.jsx)(d,{...e})}):d(e)}}}]);